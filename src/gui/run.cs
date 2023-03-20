/*
 * This file is part of ChronoJump
 *
 * ChronoJump is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or   
 *    (at your option) any later version.
 *    
 * ChronoJump is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
 *    GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 * Copyright (C) 2004-2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Gtk;
//using Glade;
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using System.Collections.Generic; //List<T>
using System.Threading;
using Mono.Unix;

//--------------------------------------------------------
//---------------- EDIT RUN WIDGET -----------------------
//--------------------------------------------------------

public class EditRunWindow : EditEventWindow
{
	static EditRunWindow EditRunWindowBox;
	private int mistakes;

	//for inheritance
	protected EditRunWindow () {
	}

	public EditRunWindow (Gtk.Window parent)
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "edit_event.glade", "edit_event", null);
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "edit_event.glade", null);
		connectWidgetsEditEvent (builder);
		builder.Autoconnect (this);

		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(edit_event);
	
		eventBigTypeString = Catalog.GetString("race");
	}

	static public EditRunWindow Show (Gtk.Window parent, Event myEvent, int pDN, bool metersSecondsPreferred)
	{
		if (EditRunWindowBox == null) {
			EditRunWindowBox = new EditRunWindow (parent);
		}

		EditRunWindowBox.metersSecondsPreferred = metersSecondsPreferred;
		EditRunWindowBox.pDN = pDN;

		EditRunWindowBox.colorize();

		EditRunWindowBox.initializeValues();
		
		EditRunWindowBox.fillDialog (myEvent);
		
		if(myEvent.Type == "Margaria")
			EditRunWindowBox.entry_description.Sensitive = false;
		if(myEvent.Type == "Gesell-DBT") {
			EditRunWindowBox.showMistakes = true;
			EditRunWindowBox.combo_eventType.Sensitive=false;
			EditRunWindowBox.entry_description.Sensitive = false;
			EditRunWindowBox.mistakes = Convert.ToInt32(myEvent.Description);
			EditRunWindowBox.spin_mistakes.Value = Convert.ToInt32(myEvent.Description);
		}

		EditRunWindowBox.edit_event.Show ();

		return EditRunWindowBox;
	}
	
	protected override void initializeValues () {
		typeOfTest = Constants.TestTypes.RUN;
		showType = true;
		showRunStart = true;
		showTv = false;
		showTc= false;
		showFall = false;
		showDistance = true;
		distanceCanBeDecimal = true;
		showTime = true;
		showSpeed = true;
		showWeight = false;
		showLimited = false;
		showMistakes = false;
		
		if(metersSecondsPreferred)
			label_speed_units.Text = "m/s";
		else
			label_speed_units.Text = "km/h";
	}

	protected override string [] findTypes(Event myEvent) {
		string [] myTypes = SqliteRunType.SelectRunTypes("", true); //don't show allRunsName row, only select name
		return myTypes;
	}
	
	protected override void fillRunStart(Event myEvent) {
		Run myRun = (Run) myEvent;
		if(myRun.InitialSpeed)
			label_run_start_value.Text = Constants.RunStartInitialSpeedYesStr();
		else
			label_run_start_value.Text = Constants.RunStartInitialSpeedNoStr();
	}
	
	protected override void fillDistance(Event myEvent) {
		Run myRun = (Run) myEvent;
		entryDistance = myRun.Distance.ToString();
		entry_distance_value.Text = Util.TrimDecimals(entryDistance, pDN);
		//if the eventtype has not a predefined distance, make the widget sensitive
		RunType myRunType = new RunType (myRun.Type);
		if(myRunType.Distance == 0) {
			entry_distance_value.Sensitive = true;
		} else {
			entry_distance_value.Sensitive = false;
		}
	}
	
	protected override void fillTime(Event myEvent) {
		Run myRun = (Run) myEvent;
		entryTime = myRun.Time.ToString();
		
		//show all the decimals for not triming there in edit window using
		//(and having different values in formulae like GetHeightInCm ...)
		//entry_time_value.Text = Util.TrimDecimals(entryTime, pDN);
		entry_time_value.Text = entryTime;
	}
	
	protected override void fillSpeed(Event myEvent) {
		Run myRun = (Run) myEvent;
		label_speed_value.Text = Util.TrimDecimals(myRun.Speed.ToString(), pDN);
		
		if(metersSecondsPreferred)
			label_speed_units.Text = "m/s";
		else
			label_speed_units.Text = "km/h";
	}

	protected override void createSignal() {
		//only for jumps & runs
		combo_eventType.Changed += new EventHandler (on_combo_eventType_changed);
	}
	
	private void on_combo_eventType_changed (object o, EventArgs args) {
		//if the distance of the new runType is fixed, put this distance
		//if not conserve the old
		RunType myRunType = new RunType (UtilGtk.ComboGetActive(combo_eventType));
		if(myRunType.Distance != 0) {
			entryDistance = myRunType.Distance.ToString();
			entry_distance_value.Text = "";
			entry_distance_value.Text = Util.TrimDecimals(entryDistance, pDN);
			entry_distance_value.Sensitive = false;
		} else {
			entry_distance_value.Sensitive = true;
		}
		
		label_speed_value.Text = Util.TrimDecimals(
				Util.GetSpeed (entryDistance, entryTime, metersSecondsPreferred) , pDN);
	}
	
	protected override void on_spin_mistakes_changed (object o, EventArgs args) {
		if(Util.IsNumber(spin_mistakes.Value.ToString(), true) && entry_time_value.Text.ToString().Length > 0) {
			double timeWithoutMistakes = Convert.ToDouble(entry_time_value.Text.ToString()) - 2 * mistakes;
			entry_time_value.Text = (timeWithoutMistakes + 2 * spin_mistakes.Value).ToString();
			entryTime = entry_time_value.Text.ToString();
			
			mistakes = Convert.ToInt32(spin_mistakes.Value);
			
			entry_description.Text = mistakes.ToString();
		}
	}
		

	protected override void updateEvent(int eventID, int personID, string description)
	{
		SqliteRun.Update (eventID, UtilGtk.ComboGetActive(combo_eventType),
				Convert.ToDouble (entry_distance_value.Text),
				entryTime, personID, description);
	}

	protected override void on_button_cancel_clicked (object o, EventArgs args)
	{
		EditRunWindowBox.edit_event.Hide();
		EditRunWindowBox = null;
	}
	
	protected override void on_delete_event (object o, DeleteEventArgs args)
	{
		EditRunWindowBox.edit_event.Hide();
		EditRunWindowBox = null;
	}
	
	protected override void hideWindow() {
		EditRunWindowBox.edit_event.Hide();
		EditRunWindowBox = null;
	}
}
	
//--------------------------------------------------------
//---------------- EDIT RUN INTERVAL WIDGET --------------
//--------------------------------------------------------

public class EditRunIntervalWindow : EditRunWindow
{
	private Gtk.Notebook notebook_mtgug;

	private Gtk.RadioButton radio_mtgug_1_undef;
	private Gtk.RadioButton radio_mtgug_1_3;
	private Gtk.RadioButton radio_mtgug_1_2;
	private Gtk.RadioButton radio_mtgug_1_1;
	private Gtk.RadioButton radio_mtgug_1_0;

	private Gtk.RadioButton radio_mtgug_2_undef;
	private Gtk.RadioButton radio_mtgug_2_3;
	private Gtk.RadioButton radio_mtgug_2_2;
	private Gtk.RadioButton radio_mtgug_2_1;
	private Gtk.RadioButton radio_mtgug_2_0;

	private Gtk.RadioButton radio_mtgug_3_undef;
	private Gtk.RadioButton radio_mtgug_3_3;
	private Gtk.RadioButton radio_mtgug_3_2;
	private Gtk.RadioButton radio_mtgug_3_1;
	private Gtk.RadioButton radio_mtgug_3_0;

	private Gtk.RadioButton radio_mtgug_4_undef;
	private Gtk.RadioButton radio_mtgug_4_3;
	private Gtk.RadioButton radio_mtgug_4_2;
	private Gtk.RadioButton radio_mtgug_4_1;
	private Gtk.RadioButton radio_mtgug_4_0;

	private Gtk.RadioButton radio_mtgug_5_undef;
	private Gtk.RadioButton radio_mtgug_5_3;
	private Gtk.RadioButton radio_mtgug_5_2;
	private Gtk.RadioButton radio_mtgug_5_1;
	private Gtk.RadioButton radio_mtgug_5_0;

	private Gtk.RadioButton radio_mtgug_6_undef;
	private Gtk.RadioButton radio_mtgug_6_3;
	private Gtk.RadioButton radio_mtgug_6_2;
	private Gtk.RadioButton radio_mtgug_6_1;
	private Gtk.RadioButton radio_mtgug_6_0;

	static EditRunIntervalWindow EditRunIntervalWindowBox;

	private double tracks = -1;
	private string distancesString; //to manage agility/non agility tests in order to know totalDistance, this will not change

	EditRunIntervalWindow (Gtk.Window parent)
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "edit_event.glade", "edit_event", null);
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "edit_event.glade", null);
		connectWidgetsEditEvent (builder);
		connectWidgetsEditRunI (builder);
		builder.Autoconnect (this);

		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(edit_event);
	
		eventBigTypeString = Catalog.GetString("intervallic race");
	}

	static new public EditRunIntervalWindow Show (Gtk.Window parent, Event myEvent, int pDN, bool metersSecondsPreferred)
	{
		if (EditRunIntervalWindowBox == null) {
			EditRunIntervalWindowBox = new EditRunIntervalWindow (parent);
		}

		EditRunIntervalWindowBox.metersSecondsPreferred = metersSecondsPreferred;
		EditRunIntervalWindowBox.pDN = pDN;

		EditRunIntervalWindowBox.colorize();

		EditRunIntervalWindowBox.initializeValues();

		if(myEvent.Type == "MTGUG") {
			EditRunIntervalWindowBox.notebook_mtgug.Show();
			EditRunIntervalWindowBox.entry_description.Sensitive = false;
			EditRunIntervalWindowBox.fill_mtgug(myEvent.Description);

			UtilGtk.WidgetColor (EditRunIntervalWindowBox.notebook_mtgug, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundIsDark, EditRunIntervalWindowBox.notebook_mtgug);
		} else {
			EditRunIntervalWindowBox.notebook_mtgug.Hide();
			EditRunIntervalWindowBox.entry_description.Sensitive = true;
		}

		EditRunIntervalWindowBox.fillDialog (myEvent);

		EditRunIntervalWindowBox.edit_event.Show ();

		return EditRunIntervalWindowBox;
	}
	
	protected override void initializeValues () {
		typeOfTest = Constants.TestTypes.RUN_I;
		showType = true;
		showRunStart = true;
		showTv = false;
		showTc= false;
		showFall = false;
		showDistance = true;
		distanceCanBeDecimal = true;
		showTime = true;
		showSpeed = true;
		showWeight = false;
		showLimited = true;
		showMistakes = false;
		
		if(metersSecondsPreferred)
			label_speed_units.Text = "m/s";
		else
			label_speed_units.Text = "km/h";
	}

	//this disallows loops on radio actions	
	private bool toggleRaisesSignal = true;

	private void fill_mtgug (string description) {
		string [] d = description.Split(new char[] {' '});
	
		toggleRaisesSignal = false;

		switch(d[0]) {
			case "u": radio_mtgug_1_undef.Active = true; break;
			case "3": radio_mtgug_1_3.Active = true; break;
			case "2": radio_mtgug_1_2.Active = true; break;
			case "1": radio_mtgug_1_1.Active = true; break;
			case "0": radio_mtgug_1_0.Active = true; break;
		}
		switch(d[1]) {
			case "u": radio_mtgug_2_undef.Active = true; break;
			case "3": radio_mtgug_2_3.Active = true; break;
			case "2": radio_mtgug_2_2.Active = true; break;
			case "1": radio_mtgug_2_1.Active = true; break;
			case "0": radio_mtgug_2_0.Active = true; break;
		}
		switch(d[2]) {
			case "u": radio_mtgug_3_undef.Active = true; break;
			case "3": radio_mtgug_3_3.Active = true; break;
			case "2": radio_mtgug_3_2.Active = true; break;
			case "1": radio_mtgug_3_1.Active = true; break;
			case "0": radio_mtgug_3_0.Active = true; break;
		}
		switch(d[3]) {
			case "u": radio_mtgug_4_undef.Active = true; break;
			case "3": radio_mtgug_4_3.Active = true; break;
			case "2": radio_mtgug_4_2.Active = true; break;
			case "1": radio_mtgug_4_1.Active = true; break;
			case "0": radio_mtgug_4_0.Active = true; break;
		}
		switch(d[4]) {
			case "u": radio_mtgug_5_undef.Active = true; break;
			case "3": radio_mtgug_5_3.Active = true; break;
			case "2": radio_mtgug_5_2.Active = true; break;
			case "1": radio_mtgug_5_1.Active = true; break;
			case "0": radio_mtgug_5_0.Active = true; break;
		}
		switch(d[5]) {
			case "u": radio_mtgug_6_undef.Active = true; break;
			case "3": radio_mtgug_6_3.Active = true; break;
			case "2": radio_mtgug_6_2.Active = true; break;
			case "1": radio_mtgug_6_1.Active = true; break;
			case "0": radio_mtgug_6_0.Active = true; break;
		}
		
		toggleRaisesSignal = true;
	}

	protected override void on_radio_mtgug_1_toggled(object o, EventArgs args) {
		if(toggleRaisesSignal) {
			string [] d = entry_description.Text.Split(new char[] {' '});
			if(radio_mtgug_1_undef.Active)
				d[0] = "u";	
			else if(radio_mtgug_1_3.Active)
				d[0] = "3";	
			else if(radio_mtgug_1_2.Active)
				d[0] = "2";	
			else if(radio_mtgug_1_1.Active)
				d[0] = "1";	
			else if(radio_mtgug_1_0.Active)
				d[0] = "0";	

			entry_description.Text = d[0] + " " + d[1] + " " + d[2] + " " + d[3] + " " + d[4] + " " + d[5];
			fill_mtgug(entry_description.Text);
		}
	}

	protected override void on_radio_mtgug_2_toggled(object o, EventArgs args) {
		if(toggleRaisesSignal) {
			string [] d = entry_description.Text.Split(new char[] {' '});
			if(radio_mtgug_2_undef.Active)
				d[1] = "u";	
			else if(radio_mtgug_2_3.Active)
				d[1] = "3";	
			else if(radio_mtgug_2_2.Active)
				d[1] = "2";	
			else if(radio_mtgug_2_1.Active)
				d[1] = "1";	
			else if(radio_mtgug_2_0.Active)
				d[1] = "0";	

			entry_description.Text = d[0] + " " + d[1] + " " + d[2] + " " + d[3] + " " + d[4] + " " + d[5];
			fill_mtgug(entry_description.Text);
		}
	}

	protected override void on_radio_mtgug_3_toggled(object o, EventArgs args) {
		if(toggleRaisesSignal) {
			string [] d = entry_description.Text.Split(new char[] {' '});
			if(radio_mtgug_3_undef.Active)
				d[2] = "u";	
			else if(radio_mtgug_3_3.Active)
				d[2] = "3";	
			else if(radio_mtgug_3_2.Active)
				d[2] = "2";	
			else if(radio_mtgug_3_1.Active)
				d[2] = "1";	
			else if(radio_mtgug_3_0.Active)
				d[2] = "0";	

			entry_description.Text = d[0] + " " + d[1] + " " + d[2] + " " + d[3] + " " + d[4] + " " + d[5];
			fill_mtgug(entry_description.Text);
		}
	}

	protected override void on_radio_mtgug_4_toggled(object o, EventArgs args) {
		if(toggleRaisesSignal) {
			string [] d = entry_description.Text.Split(new char[] {' '});
			if(radio_mtgug_4_undef.Active)
				d[3] = "u";	
			else if(radio_mtgug_4_3.Active)
				d[3] = "3";	
			else if(radio_mtgug_4_2.Active)
				d[3] = "2";	
			else if(radio_mtgug_4_1.Active)
				d[3] = "1";	
			else if(radio_mtgug_4_0.Active)
				d[3] = "0";	

			entry_description.Text = d[0] + " " + d[1] + " " + d[2] + " " + d[3] + " " + d[4] + " " + d[5];
			fill_mtgug(entry_description.Text);
		}
	}

	protected override void on_radio_mtgug_5_toggled(object o, EventArgs args) {
		if(toggleRaisesSignal) {
			string [] d = entry_description.Text.Split(new char[] {' '});
			if(radio_mtgug_5_undef.Active)
				d[4] = "u";	
			else if(radio_mtgug_5_3.Active)
				d[4] = "3";	
			else if(radio_mtgug_5_2.Active)
				d[4] = "2";	
			else if(radio_mtgug_5_1.Active)
				d[4] = "1";	
			else if(radio_mtgug_5_0.Active)
				d[4] = "0";	

			entry_description.Text = d[0] + " " + d[1] + " " + d[2] + " " + d[3] + " " + d[4] + " " + d[5];
			fill_mtgug(entry_description.Text);
		}
	}

	protected override void on_radio_mtgug_6_toggled(object o, EventArgs args) {
		if(toggleRaisesSignal) {
			string [] d = entry_description.Text.Split(new char[] {' '});
			if(radio_mtgug_6_undef.Active)
				d[5] = "u";	
			else if(radio_mtgug_6_3.Active)
				d[5] = "3";	
			else if(radio_mtgug_6_2.Active)
				d[5] = "2";	
			else if(radio_mtgug_6_1.Active)
				d[5] = "1";	
			else if(radio_mtgug_6_0.Active)
				d[5] = "0";	

			entry_description.Text = d[0] + " " + d[1] + " " + d[2] + " " + d[3] + " " + d[4] + " " + d[5];
			fill_mtgug(entry_description.Text);
		}
	}




	protected override string [] findTypes(Event myEvent) {
		//type cannot change on run interval
		combo_eventType.Sensitive=false;

		string [] myTypes;
		myTypes = SqliteRunIntervalType.SelectRunIntervalTypes("", true); //don't show allRunsName row, only select name
		return myTypes;
	}
	
	protected override void fillRunStart(Event myEvent) {
		RunInterval myRun = (RunInterval) myEvent;
		if(myRun.InitialSpeed)
			label_run_start_value.Text = Constants.RunStartInitialSpeedYesStr();
		else
			label_run_start_value.Text = Constants.RunStartInitialSpeedNoStr();
	}
	
	
	protected override void fillDistance(Event myEvent)
	{
		RunInterval myRun = (RunInterval) myEvent;

		//distanceAtInit = 0;
		tracks = myRun.Tracks;

		//string distancesString = "";
		distancesString = "";
		List<object> selectRunITypes_l = SqliteRunIntervalType.SelectRunIntervalTypesNew ("", false);
		entry_distance_value.Sensitive = false;

		//1 on agility test show the distances string in meters
		if (myRun.DistanceInterval < 0)
		{
			if (selectRunITypes_l != null && selectRunITypes_l.Count > 0)
				distancesString = SelectRunITypes.RunIntervalTypeDistancesString (myRun.Type, selectRunITypes_l);
		}

		if (distancesString != "")
		{
			entry_distance_value.Text = RunType.DistancesStringAsMeters (distancesString);
			label_distance_units.Hide ();
		} else {
			//2 on the rest of tests show interval x times
			entry_distance_value.Text = myRun.DistanceInterval.ToString();
			label_distance_units.Show ();

			if (selectRunITypes_l != null && selectRunITypes_l.Count > 0)
				foreach (SelectRunITypes srit in selectRunITypes_l)
				{
					if (srit.NameEnglish == myRun.Type && srit.Distance == 0)
					{
						entry_distance_value.Sensitive = true;
						//distanceAtInit = myRun.DistanceInterval;
						break;
					}
				}
		}
	}

	protected override void fillTime(Event myEvent) {
		RunInterval myRun = (RunInterval) myEvent;
		label_time_title.Text = Catalog.GetString("Total Time");
		
		//show all the decimals for not triming there in edit window using
		//(and having different values in formulae like GetHeightInCm ...)
		//entry_time_value.Text = Util.TrimDecimals(myRun.TimeTotal.ToString(), pDN);
		entry_time_value.Text = myRun.TimeTotal.ToString();
		
		//don't allow to change totaltime in rjedit
		entry_time_value.Sensitive = false; 
	}

	protected override void on_entry_distance_changed (object o, EventArgs args)
	{
		if (Util.IsNumber(entry_distance_value.Text.ToString(), distanceCanBeDecimal))
		{
			label_speed_value.Text = Util.TrimDecimals(
					Util.GetSpeed (
						Util.GetRunITotalDistance (Convert.ToDouble(entry_distance_value.Text), distancesString, tracks), //TODO: check this ToDouble works on RSA
						Convert.ToDouble (entryTime), //totalTime
						metersSecondsPreferred) , pDN);
			button_accept.Sensitive = true;
		} else {
			button_accept.Sensitive = false;
			//entry_distance_value.Text = "";
			//entry_distance_value.Text = entryDistance;
		}
	}

	protected override void fillSpeed(Event myEvent) {
		RunInterval myRun = (RunInterval) myEvent;
		label_speed_value.Text = Util.TrimDecimals( 
				Util.GetSpeed(
					myRun.DistanceTotal.ToString(),
					myRun.TimeTotal.ToString(), 
					metersSecondsPreferred), pDN);
	}
	
	protected override void fillLimited(Event myEvent) {
		RunInterval myRun = (RunInterval) myEvent;
		label_limited_value.Text = Util.GetLimitedRounded(myRun.Limited, pDN);
	}


	protected override void updateEvent (int eventID, int personID, string description)
	{
		LogB.Information (string.Format (
			"updateEvent eventID: {0}, entry_distance_value.Text: {1}, tracks: {2}, personID: {3}, description: {4}",
			eventID, entry_distance_value.Text, tracks, personID, description));

		double distanceInterval = 0;
		if (Util.IsNumber (entry_distance_value.Text, true))
			distanceInterval = Convert.ToDouble (entry_distance_value.Text);
		else
			distanceInterval = -1;

		SqliteRunInterval.Update (eventID, distanceInterval, tracks, distancesString, personID, description);
	}

	protected override void on_button_cancel_clicked (object o, EventArgs args)
	{
		EditRunIntervalWindowBox.edit_event.Hide();
		EditRunIntervalWindowBox = null;
	}
	
	protected override void on_delete_event (object o, DeleteEventArgs args)
	{
		EditRunIntervalWindowBox.edit_event.Hide();
		EditRunIntervalWindowBox = null;
	}
	
	protected override void hideWindow() {
		EditRunIntervalWindowBox.edit_event.Hide();
		EditRunIntervalWindowBox = null;
	}

	private void connectWidgetsEditRunI (Gtk.Builder builder)
	{
		notebook_mtgug = (Gtk.Notebook) builder.GetObject ("notebook_mtgug");
		radio_mtgug_1_undef = (Gtk.RadioButton) builder.GetObject ("radio_mtgug_1_undef");
		radio_mtgug_1_3 = (Gtk.RadioButton) builder.GetObject ("radio_mtgug_1_3");
		radio_mtgug_1_2 = (Gtk.RadioButton) builder.GetObject ("radio_mtgug_1_2");
		radio_mtgug_1_1 = (Gtk.RadioButton) builder.GetObject ("radio_mtgug_1_1");
		radio_mtgug_1_0 = (Gtk.RadioButton) builder.GetObject ("radio_mtgug_1_0");
		radio_mtgug_2_undef = (Gtk.RadioButton) builder.GetObject ("radio_mtgug_2_undef");
		radio_mtgug_2_3 = (Gtk.RadioButton) builder.GetObject ("radio_mtgug_2_3");
		radio_mtgug_2_2 = (Gtk.RadioButton) builder.GetObject ("radio_mtgug_2_2");
		radio_mtgug_2_1 = (Gtk.RadioButton) builder.GetObject ("radio_mtgug_2_1");
		radio_mtgug_2_0 = (Gtk.RadioButton) builder.GetObject ("radio_mtgug_2_0");
		radio_mtgug_3_undef = (Gtk.RadioButton) builder.GetObject ("radio_mtgug_3_undef");
		radio_mtgug_3_3 = (Gtk.RadioButton) builder.GetObject ("radio_mtgug_3_3");
		radio_mtgug_3_2 = (Gtk.RadioButton) builder.GetObject ("radio_mtgug_3_2");
		radio_mtgug_3_1 = (Gtk.RadioButton) builder.GetObject ("radio_mtgug_3_1");
		radio_mtgug_3_0 = (Gtk.RadioButton) builder.GetObject ("radio_mtgug_3_0");
		radio_mtgug_4_undef = (Gtk.RadioButton) builder.GetObject ("radio_mtgug_4_undef");
		radio_mtgug_4_3 = (Gtk.RadioButton) builder.GetObject ("radio_mtgug_4_3");
		radio_mtgug_4_2 = (Gtk.RadioButton) builder.GetObject ("radio_mtgug_4_2");
		radio_mtgug_4_1 = (Gtk.RadioButton) builder.GetObject ("radio_mtgug_4_1");
		radio_mtgug_4_0 = (Gtk.RadioButton) builder.GetObject ("radio_mtgug_4_0");
		radio_mtgug_5_undef = (Gtk.RadioButton) builder.GetObject ("radio_mtgug_5_undef");
		radio_mtgug_5_3 = (Gtk.RadioButton) builder.GetObject ("radio_mtgug_5_3");
		radio_mtgug_5_2 = (Gtk.RadioButton) builder.GetObject ("radio_mtgug_5_2");
		radio_mtgug_5_1 = (Gtk.RadioButton) builder.GetObject ("radio_mtgug_5_1");
		radio_mtgug_5_0 = (Gtk.RadioButton) builder.GetObject ("radio_mtgug_5_0");
		radio_mtgug_6_undef = (Gtk.RadioButton) builder.GetObject ("radio_mtgug_6_undef");
		radio_mtgug_6_3 = (Gtk.RadioButton) builder.GetObject ("radio_mtgug_6_3");
		radio_mtgug_6_2 = (Gtk.RadioButton) builder.GetObject ("radio_mtgug_6_2");
		radio_mtgug_6_1 = (Gtk.RadioButton) builder.GetObject ("radio_mtgug_6_1");
		radio_mtgug_6_0 = (Gtk.RadioButton) builder.GetObject ("radio_mtgug_6_0");
	}
}


//--------------------------------------------------------
//---------------- Repair runInterval WIDGET -------------
//--------------------------------------------------------

public class RepairRunIntervalWindow 
{
	Gtk.Window repair_sub_event;
	Gtk.HBox hbox_notes_and_totaltime;
	Gtk.Label label_header;
	Gtk.Label label_totaltime_value;
	Gtk.TreeView treeview_subevents;
	Gtk.Button button_accept;
	Gtk.Button button_add_before;
	Gtk.Button button_add_after;
	Gtk.Button button_delete;
	Gtk.TextView textview1;

	private TreeStore store;
	static RepairRunIntervalWindow RepairRunIntervalWindowBox;

	RunType type;
	RunInterval runInterval; //used on button_accept
	

	RepairRunIntervalWindow (Gtk.Window parent, RunInterval myRun, int pDN)
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "repair_sub_event.glade", "repair_sub_event", null);
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "repair_sub_event.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);

		repair_sub_event.Parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(repair_sub_event);

		this.runInterval = myRun;
	
		repair_sub_event.Title = Catalog.GetString("Repair intervallic race");
		label_header.Text = Constants.GetRepairWindowMessage ();
	
		
		type = SqliteRunIntervalType.SelectAndReturnRunIntervalType(myRun.Type, false);
		
		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.Text = createTextForTextView(type);
		textview1.Buffer = tb;
		
		createTreeView(treeview_subevents);
		//count, time
		store = new TreeStore(typeof (string), typeof (string));
		treeview_subevents.Model = store;
		fillTreeView (treeview_subevents, store, myRun, pDN);
	
		button_add_before.Sensitive = false;
		button_add_after.Sensitive = false;
		button_delete.Sensitive = false;
		
		label_totaltime_value.Text = getTotalTime().ToString() + " " + Catalog.GetString("seconds");
		
		treeview_subevents.Selection.Changed += onSelectionEntry;
	}
	
	static public RepairRunIntervalWindow Show (Gtk.Window parent, RunInterval myRun, int pDN)
	{
		//LogB.Information(myRun);
		if (RepairRunIntervalWindowBox == null) {
			RepairRunIntervalWindowBox = new RepairRunIntervalWindow (parent, myRun, pDN);
		}
		
		//manage window color
		if(! Config.UseSystemColor)
		{
			UtilGtk.WindowColor(RepairRunIntervalWindowBox.repair_sub_event, Config.ColorBackground);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, RepairRunIntervalWindowBox.label_header);
			UtilGtk.ContrastLabelsHBox(Config.ColorBackgroundIsDark, RepairRunIntervalWindowBox.hbox_notes_and_totaltime);
		}

		RepairRunIntervalWindowBox.repair_sub_event.Show ();

		return RepairRunIntervalWindowBox;
	}
	
	private string createTextForTextView (RunType myRunType) {
		string runTypeString = string.Format(Catalog.GetString(
					"RaceType: {0}."), myRunType.Name);

		string fixedString = "";
		if(myRunType.FixedValue > 0) {
			if(myRunType.TracksLimited) {
				//if it's a run type runsLimited with a fixed value, then don't allow the creation of more runs
				fixedString = "\n" +  string.Format(
						Catalog.GetPluralString(
							"This race type is fixed to one lap.",
							"This race type is fixed to {0} laps.",
							myRunType.FixedValue), 
						myRunType.FixedValue) + " " +
					Catalog.GetString("You cannot add more.");
			} else {
				//if it's a run type timeLimited with a fixed value, then complain when the total time is higher
				fixedString = "\n" + string.Format(
						Catalog.GetPluralString(
							"This race type is fixed to one second.",
							"This race type is fixed to {0} seconds.",
							myRunType.FixedValue),
						myRunType.FixedValue) + " " +
					Catalog.GetString("Totaltime cannot be greater.");
			}
		}
		return runTypeString + fixedString;
	}

	
	private void createTreeView (Gtk.TreeView myTreeView) {
		myTreeView.HeadersVisible=true;
		int count = 0;

		myTreeView.AppendColumn ( Catalog.GetString ("Count"), new CellRendererText(), "text", count++);
		//myTreeView.AppendColumn ( Catalog.GetString ("Time"), new CellRendererText(), "text", count++);

		Gtk.TreeViewColumn timeColumn = new Gtk.TreeViewColumn ();
		timeColumn.Title = Catalog.GetString("Lap time");
		Gtk.CellRendererText timeCell = new Gtk.CellRendererText ();
		timeCell.Editable = true;
		timeCell.Edited += timeCellEdited;
		timeColumn.PackStart (timeCell, true);
		timeColumn.AddAttribute(timeCell, "text", count ++);
		myTreeView.AppendColumn ( timeColumn );
	}
	
	private void timeCellEdited (object o, Gtk.EditedArgs args)
	{
		Gtk.TreeIter iter;
		store.GetIter (out iter, new Gtk.TreePath (args.Path));
		if(Util.IsNumber(args.NewText, true)) {
			//if it's limited by fixed value of seconds
			//and new seconds are bigger than allowed, return
			if(type.FixedValue > 0 && ! type.TracksLimited &&
					getTotalTime() //current total time in treeview
					- Convert.ToDouble((string) treeview_subevents.Model.GetValue(iter,1)) //-old cell
					+ Convert.ToDouble(args.NewText) //+new cell
					> type.FixedValue) {	//bigger than allowed
				return;
			} else {
				store.SetValue(iter, 1, args.NewText);

				//update the totaltime label
				label_totaltime_value.Text = getTotalTime().ToString() + " " + Catalog.GetString("seconds");
			}
		}
		
		//if is not number or if it was -1, the old data will remain
	}

	private double getTotalTime() {
		TreeIter myIter;
		double totalTime = 0;
		bool iterOk = store.GetIterFirst (out myIter);
		if(iterOk) {
			do {
				double myTime = Convert.ToDouble((string) treeview_subevents.Model.GetValue(myIter, 1));
				totalTime += myTime;
			} while (store.IterNext (ref myIter));
		}
		return totalTime;
	}
	
	private void fillTreeView (Gtk.TreeView tv, TreeStore store, RunInterval myRun, int pDN)
	{
		if(myRun.IntervalTimesString.Length > 0) {
			string [] timeArray = myRun.IntervalTimesString.Split(new char[] {'='});

			int count = 0;
			foreach (string myTime in timeArray) {
				store.AppendValues ( (count+1).ToString(), Util.TrimDecimals(myTime, pDN) );
				count ++;
			}
		}
	}

	void onSelectionEntry (object o, EventArgs args) {
		ITreeModel model;
		TreeIter iter;
		
		if (((TreeSelection)o).GetSelected(out model, out iter)) {
			button_add_before.Sensitive = true;
			button_add_after.Sensitive = true;
			button_delete.Sensitive = true;

			//don't allow to add a row before or after 
			//if the runtype is fixed to n runs and we reached n
			if(type.FixedValue > 0 && type.TracksLimited) {
				int lastRow = 0;
				do {
					lastRow = Convert.ToInt32 ((string) model.GetValue (iter, 0));
				} while (store.IterNext (ref iter));

				//don't allow if max rows reached
				if(lastRow == type.FixedValue) {
					button_add_before.Sensitive = false;
					button_add_after.Sensitive = false;
				}
			}
		}
	}

	void on_button_add_before_clicked (object o, EventArgs args) {
		ITreeModel model; 
		TreeIter iter; 
		if (treeview_subevents.Selection.GetSelected (out model, out iter)) {
			int position = Convert.ToInt32( (string) model.GetValue (iter, 0) ) -1; //count starts at '0'
			iter = store.InsertNode(position);
			store.SetValue(iter, 1, "0");
			putRowNumbers(store);
		}
	}
	
	void on_button_add_after_clicked (object o, EventArgs args) {
		ITreeModel model; 
		TreeIter iter; 
		if (treeview_subevents.Selection.GetSelected (out model, out iter)) {
			int position = Convert.ToInt32( (string) model.GetValue (iter, 0) ); //count starts at '0'
			iter = store.InsertNode(position);
			store.SetValue(iter, 1, "0");
			putRowNumbers(store);
		}
	}
	
	private void putRowNumbers(TreeStore myStore) {
		TreeIter myIter;
		bool iterOk = myStore.GetIterFirst (out myIter);
		if(iterOk) {
			int count = 1;
			do {
				store.SetValue(myIter, 0, (count++).ToString());
			} while (myStore.IterNext (ref myIter));
		}
	}
		
	void on_button_delete_clicked (object o, EventArgs args) {
		ITreeModel model; 
		TreeIter iter; 
		if (treeview_subevents.Selection.GetSelected (out model, out iter)) {
			store.Remove(ref iter);
			putRowNumbers(store);
		
			label_totaltime_value.Text = getTotalTime().ToString() + " " + Catalog.GetString("seconds");

			button_add_before.Sensitive = false;
			button_add_after.Sensitive = false;
			button_delete.Sensitive = false;
		}
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		//foreach all lines... extrac intervalTimesString
		TreeIter myIter;
		string timeString = "";
		
		bool iterOk = store.GetIterFirst (out myIter);
		if(iterOk) {
			string equal= ""; //first iteration should not appear '='
			do {
				timeString = timeString + equal + (string) treeview_subevents.Model.GetValue (myIter, 1);
				equal = "=";
			} while (store.IterNext (ref myIter));
		}
			
		//calculate other variables needed for runInterval creation
		
		runInterval.Tracks = Util.GetNumberOfJumps(timeString, false); //don't need a GetNumberOfRuns, this works
		runInterval.TimeTotal = Util.GetTotalTime(timeString);

		//distanceTotal calculation caring if distances are variable
		string distancesString = "";
		if(runInterval.DistanceInterval == -1)
			distancesString = type.DistancesString;

		runInterval.DistanceTotal = Util.GetRunITotalDistance(runInterval.DistanceInterval, distancesString, runInterval.Tracks);


		if(timeString != runInterval.IntervalTimesString)
			runInterval.IntervalTimesString = timeString;
	
		if(type.FixedValue > 0) {
			//if this t'Type has a fixed value of runs or time, limitstring has not changed
			if(type.TracksLimited) {
				runInterval.Limited = type.FixedValue.ToString() + "R";
			} else {
				runInterval.Limited = type.FixedValue.ToString() + "T";
			}
		} else {
			//else limitstring should be calculated
			if(type.TracksLimited) {
				runInterval.Limited = runInterval.Tracks.ToString() + "R";
			} else {
				runInterval.Limited = runInterval.TimeTotal + "T";
			}
		}

		//save it deleting the old first for having the same uniqueID
		Sqlite.Delete(false, Constants.RunIntervalTable, runInterval.UniqueID);
		runInterval.InsertAtDB(false, Constants.RunIntervalTable); 
		/*
		SqliteRun.InsertInterval(false, Constants.RunIntervalTable, runInterval.UniqueID.ToString(), 
				runInterval.PersonID, runInterval.SessionID, 
				runInterval.Type, 
				runs * runInterval.DistanceInterval,	//distanceTotal
				Util.GetTotalTime(timeString), //timeTotal
				runInterval.DistanceInterval,		//distanceInterval
				timeString, runs, 
				runInterval.Description,
				limitString
				);
				*/

		//close the window
		RepairRunIntervalWindowBox.repair_sub_event.Hide();
		RepairRunIntervalWindowBox = null;
	}

	void on_button_cancel_clicked (object o, EventArgs args)
	{
		RepairRunIntervalWindowBox.repair_sub_event.Hide();
		RepairRunIntervalWindowBox = null;
	}
	
	void on_delete_event (object o, DeleteEventArgs args)
	{
		RepairRunIntervalWindowBox.repair_sub_event.Hide();
		RepairRunIntervalWindowBox = null;
	}
	
	public Button Button_accept 
	{
		set { button_accept = value;	}
		get { return button_accept;	}
	}

	private void connectWidgets (Gtk.Builder builder)
	{
		repair_sub_event = (Gtk.Window) builder.GetObject ("repair_sub_event");
		hbox_notes_and_totaltime = (Gtk.HBox) builder.GetObject ("hbox_notes_and_totaltime");
		label_header = (Gtk.Label) builder.GetObject ("label_header");
		label_totaltime_value = (Gtk.Label) builder.GetObject ("label_totaltime_value");
		treeview_subevents = (Gtk.TreeView) builder.GetObject ("treeview_subevents");
		button_accept = (Gtk.Button) builder.GetObject ("button_accept");
		button_add_before = (Gtk.Button) builder.GetObject ("button_add_before");
		button_add_after = (Gtk.Button) builder.GetObject ("button_add_after");
		button_delete = (Gtk.Button) builder.GetObject ("button_delete");
		textview1 = (Gtk.TextView) builder.GetObject ("textview1");
	}
}

//--------------------------------------------------------
//---------------- runs_more widget ----------------------
//--------------------------------------------------------

public class RunsMoreWindow : EventMoreWindow 
{
	Gtk.Window jumps_runs_more;
	static RunsMoreWindow RunsMoreWindowBox;
	
	private double selectedDistance;
	
	RunsMoreWindow (Gtk.Window parent, bool testOrDelete)
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "jumps_runs_more.glade", "jumps_runs_more", null);
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "jumps_runs_more.glade", null);
		connectWidgetsEventMore (builder);
		jumps_runs_more = (Gtk.Window) builder.GetObject ("jumps_runs_more");
		builder.Autoconnect (this);

		this.parent = parent;
		this.testOrDelete = testOrDelete;
		
		if(!testOrDelete)
			jumps_runs_more.Title = Catalog.GetString("Delete test type defined by user");
		
		//put an icon to window
		UtilGtk.IconWindow(jumps_runs_more);

		//manage window color
		if(! Config.UseSystemColor)
			UtilGtk.WindowColor(jumps_runs_more, Config.ColorBackground);

		selectedEventType = EventType.Types.RUN.ToString();
		//name, distance, description
		store = new TreeStore(typeof (string), typeof (string), typeof (string));
		
		initializeThings();
	}
	
	static public RunsMoreWindow Show (Gtk.Window parent, bool testOrDelete)
	{
		if (RunsMoreWindowBox == null) {
			RunsMoreWindowBox = new RunsMoreWindow (parent, testOrDelete);
		}
		RunsMoreWindowBox.jumps_runs_more.Show ();
		
		return RunsMoreWindowBox;
	}
	
	protected override void createTreeView (Gtk.TreeView tv) {
		tv.HeadersVisible=true;
		int count = 0;
		
		tv.AppendColumn ( Catalog.GetString ("Name"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Distance"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Description"), new CellRendererText(), "text", count++);
	}
	
	protected override void fillTreeView (Gtk.TreeView tv, TreeStore store) 
	{
		//select data without inserting an "all jumps", and not obtain only name of jump
		string [] myRunTypes = SqliteRunType.SelectRunTypes("", false);

		//remove typesTranslated
		typesTranslated = new String [myRunTypes.Length];
		int count = 0;

		foreach (string myType in myRunTypes) {
			string [] myStringFull = myType.Split(new char[] {':'});
			if(myStringFull[2] == "0") {
				myStringFull[2] = Catalog.GetString("Not defined");
			}

			RunType tempType = new RunType (myStringFull[1]);
			string description  = getDescriptionLocalised(tempType, myStringFull[3]);

			//if we are going to execute: show all types
			//if we are going to delete: show user defined types
			if(testOrDelete || ! tempType.IsPredefined)
				store.AppendValues (
						//myStringFull[0], //don't display the uniqueID
						Catalog.GetString(myStringFull[1]),	//name 
						myStringFull[2], 	//distance
						description
						);
			
			//create typesTranslated
			typesTranslated [count++] = myStringFull[1] + ":" + Catalog.GetString(myStringFull[1]);
		}	
	}


	protected override void onSelectionEntry (object o, EventArgs args)
	{
		ITreeModel model;
		TreeIter iter;
		selectedEventName = "-1";
		selectedDistance = 0;
		selectedDescription = "";

		if (((TreeSelection)o).GetSelected(out model, out iter)) {
			string translatedName = (string) model.GetValue (iter, 0);
			//get name in english
			selectedEventName = Util.FindOnArray(':', 1, 0, translatedName, typesTranslated);
			if( (string) model.GetValue (iter, 1) == Catalog.GetString("Not defined") ) {
				selectedDistance = 0;
			} else {
				selectedDistance = Convert.ToDouble( (string) model.GetValue (iter, 1) );
			}
			selectedDescription = (string) model.GetValue (iter, 2);
			
			if(testOrDelete) {
				button_accept.Sensitive = true;
				//update graph image test on main window
				button_selected.Click();
			} else
				button_delete_type.Sensitive = true;
		}
	}
	
	protected override void on_row_double_clicked (object o, Gtk.RowActivatedArgs args)
	{
		//return if we are to delete a test
		if(!testOrDelete)
			return;

		TreeView tv = (TreeView) o;
		ITreeModel model;
		TreeIter iter;

		if (tv.Selection.GetSelected (out model, out iter)) {
			string translatedName = (string) model.GetValue (iter, 0);
			//get name in english
			selectedEventName = Util.FindOnArray(':', 1, 0, translatedName, typesTranslated);
			
			if( (string) model.GetValue (iter, 1) == Catalog.GetString("Not defined") ) {
				selectedDistance = 0;
			} else {
				selectedDistance = Convert.ToDouble( (string) model.GetValue (iter, 1) );
			}
			selectedDescription = (string) model.GetValue (iter, 2);

			//activate on_button_accept_clicked()
			button_accept.Activate();
		}
	}
	
	protected override void deleteTestLine() {
		SqliteRunType.Delete(selectedEventName);
		
		//delete from typesTranslated
		string row = Util.FindOnArray(':',0, -1, selectedEventName, typesTranslated);
		LogB.Information("row " + row);
		typesTranslated = Util.DeleteString(typesTranslated, row);
	}

	protected override string [] findTestTypesInSessions() {
		return SqliteRun.SelectRunsSA (false, -1, -1, selectedEventName,
				Sqlite.Orders_by.DEFAULT, -1);
	}

	void on_button_cancel_clicked (object o, EventArgs args)
	{
		RunsMoreWindowBox.jumps_runs_more.Hide();
		RunsMoreWindowBox = null;
	}
	
	void on_jumps_runs_more_delete_event (object o, DeleteEventArgs args)
	{
		RunsMoreWindowBox.jumps_runs_more.Hide();
		RunsMoreWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		RunsMoreWindowBox.jumps_runs_more.Hide();
	}
	
	//when a run is done using runsMoreWindow, the accept doesn't destroy this instance, because 
	//later we need data from it.
	//This is used for destroying, then if a new run type is added, it will be shown at first time clicking "more" button
	public void Destroy() {		
		RunsMoreWindowBox = null;
	}

	public double SelectedDistance {
		get { return selectedDistance; }
	}
}

//--------------------------------------------------------
//---------------- runs_interval_more widget ------------------
//--------------------------------------------------------

public class RunsIntervalMoreWindow : EventMoreWindow 
{
	Gtk.Window jumps_runs_more;

	static RunsIntervalMoreWindow RunsIntervalMoreWindowBox;

	private double selectedDistance;
	private bool selectedTracksLimited;
	private int selectedLimitedValue;
	private bool selectedUnlimited;
	private string selectedDistancesString;
	
	RunsIntervalMoreWindow (Gtk.Window parent, bool testOrDelete)
	{
		/*
		//the glade window is the same as jumps_more
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "jumps_runs_more.glade", "jumps_runs_more", "chronojump");
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "jumps_runs_more.glade", null);
		connectWidgetsEventMore (builder);
		jumps_runs_more = (Gtk.Window) builder.GetObject ("jumps_runs_more");
		builder.Autoconnect (this);

		this.parent = parent;
		this.testOrDelete = testOrDelete;
		
		if(!testOrDelete)
			jumps_runs_more.Title = Catalog.GetString("Delete test type defined by user");
		
		//put an icon to window
		UtilGtk.IconWindow(jumps_runs_more);
		
		selectedEventType = EventType.Types.RUN.ToString();
		//name, distance, limited by tracks or seconds, limit value, description
		store = new TreeStore(typeof (string), typeof (string), typeof(string),
				typeof (string), typeof (string) );
		
		initializeThings();
	}
	
	static public RunsIntervalMoreWindow Show (Gtk.Window parent, bool testOrDelete)
	{
		if (RunsIntervalMoreWindowBox == null) {
			RunsIntervalMoreWindowBox = new RunsIntervalMoreWindow (parent, testOrDelete);
		}
		RunsIntervalMoreWindowBox.jumps_runs_more.Show ();
		
		return RunsIntervalMoreWindowBox;
	}
	
	protected override void createTreeView (Gtk.TreeView tv) {
		tv.HeadersVisible=true;
		int count = 0;

		tv.AppendColumn ( Catalog.GetString ("Name"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Distance"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Limited by"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Limited value"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Description"), new CellRendererText(), "text", count++);
	}
	
	protected override void fillTreeView (Gtk.TreeView tv, TreeStore store) 
	{
		//select data without inserting an "all runs", and not obtain only name of run
		string [] myTypes = SqliteRunIntervalType.SelectRunIntervalTypes("", false);
		
		//remove typesTranslated
		typesTranslated = new String [myTypes.Length];
		int count = 0;

		foreach (string myType in myTypes) {
			string [] myStringFull = myType.Split(new char[] {':'});
			
			string distance = myStringFull[2];
			if(distance == "0") 
				distance = Catalog.GetString("Not defined");
			else if(distance == "-1") 
				distance = myStringFull[7]; //distancesString

			
			//limited
			string myLimiter = "";
			string myLimiterValue = "";
			
			//check if it's unlimited
			if(myStringFull[5] == "1") {
				myLimiter= Catalog.GetString("Unlimited");
				myLimiterValue = "-";
			} else {
				myLimiter = Catalog.GetString("Laps");
				if(myStringFull[3] == "0") {
					myLimiter = Catalog.GetString("Seconds");
				}
				myLimiterValue = "?";
				if(Convert.ToDouble(myStringFull[4]) > 0) {
					myLimiterValue = myStringFull[4];
				}
			}

			RunType tempType = new RunType (myStringFull[1]);
			string description  = getDescriptionLocalised(tempType, myStringFull[6]);

			//if we are going to execute: show all types
			//if we are going to delete: show user defined types
			if(testOrDelete || ! tempType.IsPredefined)
				store.AppendValues (
						//myStringFull[0], //don't display de uniqueID
						Catalog.GetString(myStringFull[1]),	//name 
						distance,		
						myLimiter,		//tracks or seconds or "unlimited"
						myLimiterValue,		//? or exact value (or '-' in unlimited)
						description
						);

			//create typesTranslated
			typesTranslated [count++] = myStringFull[1] + ":" + Catalog.GetString(myStringFull[1]);
		}	
	}

	//puts a value in private member selected
	protected override void onSelectionEntry (object o, EventArgs args)
	{
		ITreeModel model;
		TreeIter iter;
		selectedEventName = "-1";
		selectedDistance = -1;
		selectedTracksLimited = false;
		selectedLimitedValue = 0;
		selectedUnlimited = false; //true if it's an unlimited run
		selectedDescription = "";
		selectedDistancesString = "";

		if (((TreeSelection)o).GetSelected(out model, out iter)) {
			string translatedName = (string) model.GetValue (iter, 0);
			//get name in english
			selectedEventName = Util.FindOnArray(':', 1, 0, translatedName, typesTranslated);

			//selectedDistance = Convert.ToDouble( (string) model.GetValue (iter, 1) );
			/*
			 * manage distances from testtypes that have different distance for each track
			 * they are expressed as: (eg for MTGUG: "1-7-19")
			 * if a '-' exists then distances are variable, else, distance is defined
			 */
			string distance = (string) model.GetValue (iter, 1);
			if(distance == Catalog.GetString("Not defined")) 
				selectedDistance = 0;
			else if(distance.Contains("-")) {
				selectedDistance = -1;
				selectedDistancesString = distance;
			} else 
				selectedDistance = Convert.ToDouble(distance);


			if( (string) model.GetValue (iter, 2) == Catalog.GetString("Unlimited") ) {
				selectedUnlimited = true;
			} 

			if( (string) model.GetValue (iter, 2) == Catalog.GetString("Laps") ) {
				selectedTracksLimited = true;
			}

			if( (string) model.GetValue (iter, 3) == "?" || (string) model.GetValue (iter, 3) == "-" ) {
				selectedLimitedValue = 0;
			} else {
				selectedLimitedValue = Convert.ToInt32( (string) model.GetValue (iter, 3) );
			}
		
			selectedDescription = (string) model.GetValue (iter, 4);

			if(testOrDelete) {
				button_accept.Sensitive = true;
				//update graph image test on main window
				button_selected.Click();
			} else
				button_delete_type.Sensitive = true;
		}
	}
	
	protected override void on_row_double_clicked (object o, Gtk.RowActivatedArgs args)
	{
		//return if we are to delete a test
		if(!testOrDelete)
			return;

		TreeView tv = (TreeView) o;
		ITreeModel model;
		TreeIter iter;

		if (tv.Selection.GetSelected (out model, out iter)) {
			string translatedName = (string) model.GetValue (iter, 0);
			//get name in english
			selectedEventName = Util.FindOnArray(':', 1, 0, translatedName, typesTranslated);
			
			//selectedDistance = Convert.ToDouble( (string) model.GetValue (iter, 1) );
			
			string distance = (string) model.GetValue (iter, 1);
			if(distance == Catalog.GetString("Not defined")) 
				selectedDistance = 0;
			else if(distance.Contains("-")) {
				selectedDistance = -1;
				selectedDistancesString = distance;
			} else 
				selectedDistance = Convert.ToDouble(distance);


			if( (string) model.GetValue (iter, 2) == Catalog.GetString("Unlimited") ) {
				selectedUnlimited = true;
			} 
			
			if( (string) model.GetValue (iter, 2) == Catalog.GetString("Laps") ) {
				selectedTracksLimited = true;
			}

			if( (string) model.GetValue (iter, 3) == "?" || (string) model.GetValue (iter, 3) == "-" ) {
				selectedLimitedValue = 0;
			} else {
				selectedLimitedValue = Convert.ToInt32( (string) model.GetValue (iter, 3) );
			}
			
			selectedDescription = (string) model.GetValue (iter, 4);
			
			//activate on_button_accept_clicked()
			button_accept.Activate();
		}
	}
	
	protected override void deleteTestLine() {
		SqliteRunIntervalType.Delete(selectedEventName);
		
		//delete from typesTranslated
		string row = Util.FindOnArray(':',0, -1, selectedEventName, typesTranslated);
		typesTranslated = Util.DeleteString(typesTranslated, row);
	}

	protected override string [] findTestTypesInSessions() {
		return SqliteRunInterval.SelectRunsSA (false, -1, -1, selectedEventName);
	}
	
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		RunsIntervalMoreWindowBox.jumps_runs_more.Hide();
		RunsIntervalMoreWindowBox = null;
	}
	
	void on_jumps_runs_more_delete_event (object o, DeleteEventArgs args)
	{
		RunsIntervalMoreWindowBox.jumps_runs_more.Hide();
		RunsIntervalMoreWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		RunsIntervalMoreWindowBox.jumps_runs_more.Hide();
	}
	
	//when a runInterval is done using runsIntervalMoreWindow, the accept doesn't destroy this instance, because 
	//later we need data from it.
	//This is used for destroying, then if a new runInterval type is added, it will be shown at first time clicking "more" button
	public void Destroy() {		
		RunsIntervalMoreWindowBox = null;
	}
	
	public double SelectedDistance {
		get { return selectedDistance; }
	}
	
	public string SelectedDistancesString {
		get { return selectedDistancesString; }
	}
	
	public bool SelectedTracksLimited {
		get { return selectedTracksLimited; }
	}
	
	public int SelectedLimitedValue { 
		get { return selectedLimitedValue; }
	}
	
	public bool SelectedUnlimited {
		get { return selectedUnlimited; }
	}
}
