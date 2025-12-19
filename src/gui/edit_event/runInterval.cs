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
 * Copyright (C) 2004-2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Gtk;
using System.Text; //StringBuilder
using System.Collections.Generic; //List<T>
using Mono.Unix;


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
	
	protected override void initializeSpecific ()
	{
		typeOfTest = Constants.TestTypes.RUN_I;
		showType = true;
		showRunStart = true;

		showRunDistance = true;
		distanceCanBeDecimal = true;
		showTime = true;
		showSpeed = true;
		showLimited = true;
		showDescription = true;
		
		if(metersSecondsPreferred)
			label_speed_units.Text = "m/s";
		else
			label_speed_units.Text = "km/h";
	}

	protected override void fillDialogSpecific (Event myEvent)
	{
		RunInterval myRun = (RunInterval) myEvent;
		label_date_value.Text = myRun.Datetime;
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

	protected override void fillRunDistance (Event myEvent)
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


	protected override void updateSQL (int eventID, int personID, string description)
	{
		LogB.Information (string.Format (
			"updateSQL eventID: {0}, entry_distance_value.Text: {1}, tracks: {2}, personID: {3}, description: {4}",
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

public partial class ChronoJumpWindow
{
	private void on_edit_selected_run_interval_clicked (object o, EventArgs args)
	{
		//notebooks_change(3); see "notebooks_change sqlite problem"
		LogB.Information("Edit selected run interval");
		//1.- check that there's a line selected
		//2.- check that this line is a run and not a person (check also if it's not a individual subrun, the pass the parent run)
		int selectedID = treeViewResultsSession.EventSelectedID;
		if (selectedID < 0)
			return;

		//3.- obtain the data of the selected run
		RunInterval myRun = SqliteRunInterval.SelectRunData (Constants.RunIntervalTable, selectedID, false, false );
		eventOldPerson = myRun.PersonID;

		//4.- edit this run
		editRunIntervalWin = EditRunIntervalWindow.Show (app1, myRun, preferences.digitsNumber, preferences.metersSecondsPreferred);
		editRunIntervalWin.Fake_button_finished.Clicked += new EventHandler (on_edit_selected_run_interval_finished);
	}
	
	private void on_edit_selected_run_interval_finished (object o, EventArgs args)
	{
		LogB.Information("edit selected run interval finished");

		int selectedID = treeViewResultsSession.EventSelectedID;
		RunInterval myRun = SqliteRunInterval.SelectRunData (Constants.RunIntervalTable, selectedID, false, false);

		//if person changed, fill treeview again, if not, only update it's line
		//distanceChanged is also managed with no problems because TreeViewEvent.Update has been extend to work with two level treeviews
		if (eventOldPerson != myRun.PersonID)// ||
				//(editRunIntervalWin != null && editRunIntervalWin.DistanceChanged) )
			pre_fillTreeView_resultsSession ();
		else
			treeViewResultsSession.Update (myRun);

		//update the session barplot
		updateGraphRunsInterval();

		//update the selected runI barplot
		selectedRunInterval = SqliteRunInterval.SelectRunData (Constants.RunIntervalTable, selectedID, true, false);
		on_treeview_runs_interval_cursor_changed (new object (), new EventArgs ());

		//update top graph:
		drawingarea_results_realtime.QueueDraw ();

		if(createdStatsWin)
			stats_win_fillTreeView_stats(false, false);
	}
}
