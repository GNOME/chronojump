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
 * Copyright (C) 2004-2012   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Glade;
using System.Text; //StringBuilder
using System.Collections; //ArrayList

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

	public EditRunWindow (Gtk.Window parent) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "edit_event", null);
		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(edit_event);
	
		eventBigTypeString = Catalog.GetString("run");
	}

	static new public EditRunWindow Show (Gtk.Window parent, Event myEvent, int pDN, bool metersSecondsPreferred)
	{
		if (EditRunWindowBox == null) {
			EditRunWindowBox = new EditRunWindow (parent);
		}

		EditRunWindowBox.metersSecondsPreferred = metersSecondsPreferred;
		EditRunWindowBox.pDN = pDN;
		
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
			label_speed_units.Text = "Km/h";
	}

	protected override string [] findTypes(Event myEvent) {
		string [] myTypes = SqliteRunType.SelectRunTypes("", true); //don't show allRunsName row, only select name
		return myTypes;
	}
	
	protected override void fillRunStart(Event myEvent) {
		Run myRun = (Run) myEvent;
		if(myRun.InitialSpeed)
			label_run_start_value.Text = Constants.RunStartInitialSpeedYes;
		else
			label_run_start_value.Text = Constants.RunStartInitialSpeedNo;
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
			label_speed_units.Text = "Km/h";
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
		

	protected override void updateEvent(int eventID, int personID, string description) {
		SqliteRun.Update(eventID, UtilGtk.ComboGetActive(combo_eventType), entryDistance, entryTime, personID, description);
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
	[Widget] private Gtk.Notebook notebook_mtgug;

	[Widget] private Gtk.RadioButton radio_mtgug_1_undef;
	[Widget] private Gtk.RadioButton radio_mtgug_1_3;
	[Widget] private Gtk.RadioButton radio_mtgug_1_2;
	[Widget] private Gtk.RadioButton radio_mtgug_1_1;
	[Widget] private Gtk.RadioButton radio_mtgug_1_0;

	[Widget] private Gtk.RadioButton radio_mtgug_2_undef;
	[Widget] private Gtk.RadioButton radio_mtgug_2_3;
	[Widget] private Gtk.RadioButton radio_mtgug_2_2;
	[Widget] private Gtk.RadioButton radio_mtgug_2_1;
	[Widget] private Gtk.RadioButton radio_mtgug_2_0;

	[Widget] private Gtk.RadioButton radio_mtgug_3_undef;
	[Widget] private Gtk.RadioButton radio_mtgug_3_3;
	[Widget] private Gtk.RadioButton radio_mtgug_3_2;
	[Widget] private Gtk.RadioButton radio_mtgug_3_1;
	[Widget] private Gtk.RadioButton radio_mtgug_3_0;

	[Widget] private Gtk.RadioButton radio_mtgug_4_undef;
	[Widget] private Gtk.RadioButton radio_mtgug_4_3;
	[Widget] private Gtk.RadioButton radio_mtgug_4_2;
	[Widget] private Gtk.RadioButton radio_mtgug_4_1;
	[Widget] private Gtk.RadioButton radio_mtgug_4_0;

	[Widget] private Gtk.RadioButton radio_mtgug_5_undef;
	[Widget] private Gtk.RadioButton radio_mtgug_5_3;
	[Widget] private Gtk.RadioButton radio_mtgug_5_2;
	[Widget] private Gtk.RadioButton radio_mtgug_5_1;
	[Widget] private Gtk.RadioButton radio_mtgug_5_0;

	[Widget] private Gtk.RadioButton radio_mtgug_6_undef;
	[Widget] private Gtk.RadioButton radio_mtgug_6_3;
	[Widget] private Gtk.RadioButton radio_mtgug_6_2;
	[Widget] private Gtk.RadioButton radio_mtgug_6_1;
	[Widget] private Gtk.RadioButton radio_mtgug_6_0;

	static EditRunIntervalWindow EditRunIntervalWindowBox;

	EditRunIntervalWindow (Gtk.Window parent) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "edit_event", null);
		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(edit_event);
	
		eventBigTypeString = Catalog.GetString("intervallic run");
	}

	static new public EditRunIntervalWindow Show (Gtk.Window parent, Event myEvent, int pDN, bool metersSecondsPreferred)
	{
		if (EditRunIntervalWindowBox == null) {
			EditRunIntervalWindowBox = new EditRunIntervalWindow (parent);
		}

		EditRunIntervalWindowBox.metersSecondsPreferred = metersSecondsPreferred;
		EditRunIntervalWindowBox.pDN = pDN;
		
		EditRunIntervalWindowBox.initializeValues();

		if(myEvent.Type == "MTGUG") {
			EditRunIntervalWindowBox.notebook_mtgug.Show();
			EditRunIntervalWindowBox.entry_description.Sensitive = false;
			EditRunIntervalWindowBox.fill_mtgug(myEvent.Description);
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
			label_speed_units.Text = "Km/h";
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
			label_run_start_value.Text = Constants.RunStartInitialSpeedYes;
		else
			label_run_start_value.Text = Constants.RunStartInitialSpeedNo;
	}
	
	
	protected override void fillDistance(Event myEvent) {
		RunInterval myRun = (RunInterval) myEvent;
		entry_distance_value.Text = myRun.DistanceInterval.ToString() +
			"x" + myRun.Limited;
		entry_distance_value.Sensitive = false;
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


	protected override void updateEvent(int eventID, int personID, string description) {
		SqliteRunInterval.Update(eventID, personID, description);
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
}


//--------------------------------------------------------
//---------------- Repair runInterval WIDGET -------------
//--------------------------------------------------------

public class RepairRunIntervalWindow 
{
	[Widget] Gtk.Window repair_sub_event;
	[Widget] Gtk.Label label_header;
	[Widget] Gtk.Label label_totaltime_value;
	[Widget] Gtk.TreeView treeview_subevents;
	private TreeStore store;
	[Widget] Gtk.Button button_accept;
	[Widget] Gtk.Button button_add_before;
	[Widget] Gtk.Button button_add_after;
	[Widget] Gtk.Button button_delete;
	[Widget] Gtk.TextView textview1;

	static RepairRunIntervalWindow RepairRunIntervalWindowBox;
	Gtk.Window parent;

	RunType type;
	RunInterval runInterval; //used on button_accept
	

	RepairRunIntervalWindow (Gtk.Window parent, RunInterval myRun, int pDN) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "repair_sub_event", null);
		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(repair_sub_event);

		this.runInterval = myRun;
	
		repair_sub_event.Title = Catalog.GetString("Repair intervallic run");
		
		System.Globalization.NumberFormatInfo localeInfo = new System.Globalization.NumberFormatInfo();
		localeInfo = System.Globalization.NumberFormatInfo.CurrentInfo;
		label_header.Text = string.Format(Catalog.GetString("Use this window to repair this test.\nDouble clic any cell to edit it (decimal separator: '{0}')"), localeInfo.NumberDecimalSeparator);
	
		
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
		//Log.WriteLine(myRun);
		if (RepairRunIntervalWindowBox == null) {
			RepairRunIntervalWindowBox = new RepairRunIntervalWindow (parent, myRun, pDN);
		}
		
		RepairRunIntervalWindowBox.repair_sub_event.Show ();

		return RepairRunIntervalWindowBox;
	}
	
	private string createTextForTextView (RunType myRunType) {
		string runTypeString = string.Format(Catalog.GetString(
					"RunType: {0}."), myRunType.Name);

		string fixedString = "";
		if(myRunType.FixedValue > 0) {
			if(myRunType.TracksLimited) {
				//if it's a run type runsLimited with a fixed value, then don't allow the creation of more runs
				fixedString = "\n" +  string.Format(
						Catalog.GetPluralString(
							"This run type is fixed to one run.", 
							"This run type is fixed to {0} runs.",
							myRunType.FixedValue), 
						myRunType.FixedValue) +
					Catalog.GetString("You cannot add more.");
			} else {
				//if it's a run type timeLimited with a fixed value, then complain when the total time is higher
				fixedString = "\n" + string.Format(
						Catalog.GetPluralString(
							"This run type is fixed to one second.",
							"This run type is fixed to {0} seconds.",
							myRunType.FixedValue),
						myRunType.FixedValue) +
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
		timeColumn.Title = Catalog.GetString("TF");
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
		TreeModel model;
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
		TreeModel model; 
		TreeIter iter; 
		if (treeview_subevents.Selection.GetSelected (out model, out iter)) {
			int position = Convert.ToInt32( (string) model.GetValue (iter, 0) ) -1; //count starts at '0'
			iter = store.InsertNode(position);
			store.SetValue(iter, 1, "0");
			putRowNumbers(store);
		}
	}
	
	void on_button_add_after_clicked (object o, EventArgs args) {
		TreeModel model; 
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
		TreeModel model; 
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
		runInterval.DistanceTotal = runInterval.TimeTotal * runInterval.DistanceInterval;
	
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
		Sqlite.Delete(Constants.RunIntervalTable, runInterval.UniqueID);
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

}

//--------------------------------------------------------
//---------------- run extra WIDGET --------------------
//---------------- in 0.9.3 included in main gui ---------
//--------------------------------------------------------

partial class ChronoJumpWindow
{

	//runs
	//labels notebook_execute	
	[Widget] Gtk.Label label_extra_window_radio_run_custom;
	[Widget] Gtk.Label label_extra_window_radio_run_20m;
	[Widget] Gtk.Label label_extra_window_radio_run_100m;
	[Widget] Gtk.Label label_extra_window_radio_run_200m;
	[Widget] Gtk.Label label_extra_window_radio_run_400m;
	[Widget] Gtk.Label label_extra_window_radio_run_gesell;
	[Widget] Gtk.Label label_extra_window_radio_run_20yard;
	[Widget] Gtk.Label label_extra_window_radio_run_505;
	[Widget] Gtk.Label label_extra_window_radio_run_illinois;
	[Widget] Gtk.Label label_extra_window_radio_run_margaria;
	[Widget] Gtk.Label label_extra_window_radio_run_shuttle;
	[Widget] Gtk.Label label_extra_window_radio_run_zigzag;
	[Widget] Gtk.Label label_extra_window_radio_run_more;
	
	//radio notebook_execute	
	[Widget] Gtk.RadioButton extra_window_radio_run_custom;
	[Widget] Gtk.RadioButton extra_window_radio_run_20m;
	[Widget] Gtk.RadioButton extra_window_radio_run_100m;
	[Widget] Gtk.RadioButton extra_window_radio_run_200m;
	[Widget] Gtk.RadioButton extra_window_radio_run_400m;
	[Widget] Gtk.RadioButton extra_window_radio_run_gesell;
	[Widget] Gtk.RadioButton extra_window_radio_run_20yard;
	[Widget] Gtk.RadioButton extra_window_radio_run_505;
	[Widget] Gtk.RadioButton extra_window_radio_run_illinois;
	[Widget] Gtk.RadioButton extra_window_radio_run_margaria;
	[Widget] Gtk.RadioButton extra_window_radio_run_shuttle;
	[Widget] Gtk.RadioButton extra_window_radio_run_zigzag;
	[Widget] Gtk.RadioButton extra_window_radio_run_more;

	//runs interval
	//labels notebook_execute	
	[Widget] Gtk.Label label_extra_window_radio_run_interval_by_laps;
	[Widget] Gtk.Label label_extra_window_radio_run_interval_by_time;
	[Widget] Gtk.Label label_extra_window_radio_run_interval_unlimited;
	[Widget] Gtk.Label label_extra_window_radio_run_interval_mtgug;
	[Widget] Gtk.Label label_extra_window_radio_run_interval_rsa_test_1;
	[Widget] Gtk.Label label_extra_window_radio_run_interval_more;
	
	//radio notebook_execute	
	[Widget] Gtk.RadioButton extra_window_radio_run_interval_by_laps;
	[Widget] Gtk.RadioButton extra_window_radio_run_interval_by_time;
	[Widget] Gtk.RadioButton extra_window_radio_run_interval_unlimited;
	[Widget] Gtk.RadioButton extra_window_radio_run_interval_mtgug;
	[Widget] Gtk.RadioButton extra_window_radio_run_interval_rsa_test_1;
	[Widget] Gtk.RadioButton extra_window_radio_run_interval_more;

	//options runs
	[Widget] Gtk.Label extra_window_runs_label_distance;
	[Widget] Gtk.SpinButton extra_window_runs_spinbutton_distance;
	[Widget] Gtk.Label extra_window_runs_label_distance_units;
	[Widget] Gtk.Label extra_window_label_runs_no_options;
	
	[Widget] Gtk.Box vbox_runs_prevent_double_contact;
	[Widget] Gtk.CheckButton checkbutton_runs_prevent_double_contact;
	[Widget] Gtk.SpinButton spinbutton_runs_prevent_double_contact;
	[Widget] Gtk.RadioButton radio_runs_prevent_double_contact_first;
	[Widget] Gtk.RadioButton radio_runs_prevent_double_contact_average;
	[Widget] Gtk.RadioButton radio_runs_prevent_double_contact_last;
	
	//options runs interval
	[Widget] Gtk.Label extra_window_runs_interval_label_distance;
	[Widget] Gtk.SpinButton extra_window_runs_interval_spinbutton_distance;
	[Widget] Gtk.Label extra_window_runs_interval_label_distance_units;

	[Widget] Gtk.Label extra_window_runs_interval_label_limit;
	[Widget] Gtk.SpinButton extra_window_runs_interval_spinbutton_limit;
	[Widget] Gtk.Label extra_window_runs_interval_label_limit_units;

	[Widget] Gtk.Label extra_window_label_runs_interval_no_options;

	//selected test labels	
	[Widget] Gtk.Label extra_window_runs_label_selected;
	[Widget] Gtk.Label extra_window_runs_interval_label_selected;

	double extra_window_runs_distance = 100;
	double extra_window_runs_interval_distance = 100;
	double extra_window_runs_interval_limit = 10;

	private RunType previousRunType; //used on More to turnback if cancel or delete event is pressed
	private RunType previousRunIntervalType; //used on More to turnback if cancel or delete event is pressed

	
	private void on_extra_window_runs_test_changed(object o, EventArgs args)
	{
		if(extra_window_radio_run_custom.Active) currentRunType = new RunType("Custom");
		else if(extra_window_radio_run_20m.Active) currentRunType = new RunType("20m");
		else if(extra_window_radio_run_100m.Active) currentRunType = new RunType("100m");
		else if(extra_window_radio_run_200m.Active) currentRunType = new RunType("200m");
		else if(extra_window_radio_run_400m.Active) currentRunType = new RunType("400m");
		else if(extra_window_radio_run_gesell.Active) currentRunType = new RunType("Gesell-DBT");
		else if(extra_window_radio_run_20yard.Active) currentRunType = new RunType("Agility-20Yard");
		else if(extra_window_radio_run_505.Active) currentRunType = new RunType("Agility-505");
		else if(extra_window_radio_run_illinois.Active) currentRunType = new RunType("Agility-Illinois");
		else if(extra_window_radio_run_margaria.Active) currentRunType = new RunType("Margaria");
		else if(extra_window_radio_run_shuttle.Active) currentRunType = new RunType("Agility-Shuttle-Run");
		else if(extra_window_radio_run_zigzag.Active) currentRunType = new RunType("Agility-ZigZag");

		extra_window_runs_initialize(currentRunType);
	}

	private void on_extra_window_runs_more(object o, EventArgs args)
	{
		previousRunType = currentRunType;

		if(extra_window_radio_run_more.Active) {
			runsMoreWin = RunsMoreWindow.Show(app1, true);
			runsMoreWin.Button_accept.Clicked += new EventHandler(on_more_runs_accepted);
			runsMoreWin.Button_cancel.Clicked += new EventHandler(on_more_runs_cancelled);
			runsMoreWin.Button_selected.Clicked += new EventHandler(on_more_runs_draw_image_test);
		}
	}
	
	private void on_extra_window_runs_interval_test_changed(object o, EventArgs args)
	{
		if(extra_window_radio_run_interval_by_laps.Active) currentRunIntervalType = new RunType("byLaps");
		else if(extra_window_radio_run_interval_by_time.Active) currentRunIntervalType = new RunType("byTime");
		else if(extra_window_radio_run_interval_unlimited.Active) currentRunIntervalType = new RunType("unlimited");
		else if(extra_window_radio_run_interval_mtgug.Active) currentRunIntervalType = new RunType("MTGUG");
		else if(extra_window_radio_run_interval_rsa_test_1.Active) currentRunIntervalType = new RunType("RSA 8-4-R3-5");

		extra_window_runs_interval_initialize(currentRunIntervalType);
	}
	
	private void on_extra_window_runs_interval_more(object o, EventArgs args)
	{
		previousRunIntervalType = currentRunIntervalType;

		if(extra_window_radio_run_interval_more.Active) {
			runsIntervalMoreWin = RunsIntervalMoreWindow.Show(app1, true);
			runsIntervalMoreWin.Button_accept.Clicked += new EventHandler(on_more_runs_interval_accepted);
			runsIntervalMoreWin.Button_cancel.Clicked += new EventHandler(on_more_runs_interval_cancelled);
			runsIntervalMoreWin.Button_selected.Clicked += new EventHandler(on_more_runs_interval_draw_image_test);
		}
	}
	
	private void extra_window_runs_initialize(RunType myRunType) 
	{
		extra_window_runs_label_selected.Text = "<b>" + Catalog.GetString(myRunType.Name) + "</b>";
		extra_window_runs_label_selected.UseMarkup = true; 
		currentEventType = myRunType;
		changeTestImage(EventType.Types.RUN.ToString(), myRunType.Name, myRunType.ImageFileName);
		bool hasOptions = false;

		extra_window_runs_label_distance.Text = Catalog.GetString("Track distance\n(between platforms)");
		extra_window_runs_label_distance_units.Text = Catalog.GetString("meters");

		if(myRunType.Distance > 0) {
			extra_window_runs_spinbutton_distance.Value = myRunType.Distance;
			extra_window_showDistanceData(myRunType, true, false);	//visible, sensitive
		} else {
			if(myRunType.Name == "Margaria") {
				extra_window_runs_label_distance.Text = Catalog.GetString("Vertical distance between\nstairs third and nine.");
				extra_window_runs_label_distance_units.Text = Catalog.GetString("Millimeters.");
				extra_window_runs_spinbutton_distance.Value = 1050;
			} else {
				extra_window_runs_spinbutton_distance.Value = extra_window_runs_distance; 
			}
			extra_window_showDistanceData(myRunType, true, true);	//visible, sensitive

		}

		hasOptions = true;
		extra_window_runs_showNoOptions(myRunType, hasOptions);
	}
	
	private void extra_window_runs_interval_initialize(RunType myRunType) 
	{
		extra_window_runs_interval_label_selected.Text = "<b>" + Catalog.GetString(myRunType.Name) + "</b>";
		extra_window_runs_interval_label_selected.UseMarkup = true; 
		currentEventType = myRunType;
		changeTestImage(EventType.Types.RUN.ToString(), myRunType.Name, myRunType.ImageFileName);
		bool hasOptions = false;

		if(myRunType.Distance > 0) {
			extra_window_runs_interval_spinbutton_distance.Value = myRunType.Distance;
			extra_window_showDistanceData(myRunType, true, false);	//visible, sensitive
			hasOptions = true;
		} else if(myRunType.Distance == 0) {
			extra_window_runs_interval_spinbutton_distance.Value = extra_window_runs_interval_distance; 
			extra_window_showDistanceData(myRunType, true, true);	//visible, sensitive
			hasOptions = true;
		} else { //variableDistancesString (eg. MTGUG) don't show anything
			extra_window_showDistanceData(myRunType, false, false);	//visible, sensitive
		}

		if(! myRunType.Unlimited) {
			string tracksName = Catalog.GetString("tracks");
			string secondsName = Catalog.GetString("seconds");
			if(myRunType.TracksLimited) 
				extra_window_runs_interval_label_limit_units.Text = tracksName;
			else 
				extra_window_runs_interval_label_limit_units.Text = secondsName;
			
			if(myRunType.FixedValue > 0) {
				extra_window_runs_interval_spinbutton_limit.Value = myRunType.FixedValue;
				extra_window_showLimitData(true, false);	//visible, sensitive
			} else {
				extra_window_runs_interval_spinbutton_limit.Value = extra_window_runs_interval_limit;
				extra_window_showLimitData(true, true);	//visible, sensitive
			}
			hasOptions = true;
		} else {
			extra_window_showLimitData(false, false);	//visible, sensitive
		}

		extra_window_runs_showNoOptions(myRunType, hasOptions);
	}

	private void on_more_runs_draw_image_test (object o, EventArgs args) {
		currentEventType = new RunType(runsMoreWin.SelectedEventName);
		changeTestImage(currentEventType.Type.ToString(), currentEventType.Name, currentEventType.ImageFileName);
	}
	
	private void on_more_runs_interval_draw_image_test (object o, EventArgs args) {
		currentEventType = new RunType(runsIntervalMoreWin.SelectedEventName);
		changeTestImage(currentEventType.Type.ToString(), currentEventType.Name, currentEventType.ImageFileName);
	}
	
	
	//used from the dialogue "runs more"
	private void on_more_runs_accepted (object o, EventArgs args) 
	{
		runsMoreWin.Button_accept.Clicked -= new EventHandler(on_more_runs_accepted);
	
		currentRunType = new RunType(
				runsMoreWin.SelectedEventName,	//name
				false,				//hasIntervals
				runsMoreWin.SelectedDistance,	//distance
				false,				//tracksLimited (false, because has not intervals)
				0,				//fixedValue (0, because has not intervals)
				false,				//unlimited (false, because has not intervals)
				runsMoreWin.SelectedDescription,
				"", // distancesstring (deactivated now, TODO: activate)
				SqliteEvent.GraphLinkSelectFileName("run", runsMoreWin.SelectedEventName)
				);
		
		extra_window_runs_toggle_desired_button_on_toolbar(currentRunType);
				
		//destroy the win for not having updating problems if a new run type is created
		runsMoreWin.Destroy();
	}
	
	private void on_more_runs_interval_accepted (object o, EventArgs args) 
	{
		runsIntervalMoreWin.Button_accept.Clicked -= new EventHandler(on_more_runs_interval_accepted);
		
		currentRunIntervalType = new RunType(
				runsIntervalMoreWin.SelectedEventName,	//name
				true,					//hasIntervals
				runsIntervalMoreWin.SelectedDistance,
				runsIntervalMoreWin.SelectedTracksLimited,
				runsIntervalMoreWin.SelectedLimitedValue,
				runsIntervalMoreWin.SelectedUnlimited,
				runsIntervalMoreWin.SelectedDescription,
				runsIntervalMoreWin.SelectedDistancesString,
				SqliteEvent.GraphLinkSelectFileName(Constants.RunIntervalTable, runsIntervalMoreWin.SelectedEventName)
				);

		/*
		bool unlimited = false;
		if(runsIntervalMoreWin.SelectedUnlimited)
			unlimited = true;
			*/
		
		extra_window_runs_interval_toggle_desired_button_on_toolbar(currentRunIntervalType);

		//destroy the win for not having updating problems if a new runInterval type is created
		runsIntervalMoreWin.Destroy();
		
		/*
		//go to run extra if we need something to define
		if( currentRunType.Distance == 0 || 
				//(currentRunType.FixedValue == 0 && ! runsIntervalMoreWin.SelectedUnlimited) ) {
				(currentRunType.FixedValue == 0 && ! unlimited) ) {
			on_run_extra_activate(o, args);
		} else {
			on_run_interval_accepted(o, args);
		}
		*/
	}
	
	//if it's cancelled (or deleted event) select desired toolbar button
	private void on_more_runs_cancelled (object o, EventArgs args) 
	{
		currentRunType = previousRunType;
		extra_window_runs_toggle_desired_button_on_toolbar(currentRunType);
	}
	
	private void on_more_runs_interval_cancelled (object o, EventArgs args) 
	{
		currentRunIntervalType = previousRunIntervalType;
		extra_window_runs_interval_toggle_desired_button_on_toolbar(currentRunIntervalType);
	}
	
	private void extra_window_runs_toggle_desired_button_on_toolbar(RunType type) {
		if(type.Name == "Custom") extra_window_radio_run_custom.Active = true;
		else if(type.Name == "20m") extra_window_radio_run_20m.Active = true;
		else if(type.Name == "100m") extra_window_radio_run_100m.Active = true;
		else if(type.Name == "200m") extra_window_radio_run_200m.Active = true;
		else if(type.Name == "400m") extra_window_radio_run_400m.Active = true;
		else if(type.Name == "Gesell-DBT") extra_window_radio_run_gesell.Active = true;
		else if(type.Name == "Agility-20Yard") extra_window_radio_run_20yard.Active = true;
		else if(type.Name == "Agility-505") extra_window_radio_run_505.Active = true;
		else if(type.Name == "Agility-Illinois") extra_window_radio_run_illinois.Active = true;
		else if(type.Name == "Margaria") extra_window_radio_run_margaria.Active = true;
		else if(type.Name == "Agility-Shuttle-Run") extra_window_radio_run_shuttle.Active = true;
		else if(type.Name == "Agility-ZigZag") extra_window_radio_run_zigzag.Active = true;
		else {
			//don't do this:
			//extra_window_radio_run_more.Active = true;
			//because it will be a loop
			//only do:
			extra_window_runs_initialize(type);
		}
	}
	
	private void extra_window_runs_interval_toggle_desired_button_on_toolbar(RunType type) {
		if(type.Name == "byLaps") extra_window_radio_run_interval_by_laps.Active = true;
		else if(type.Name == "byTime") extra_window_radio_run_interval_by_time.Active = true;
		else if(type.Name == "unlimited") extra_window_radio_run_interval_unlimited.Active = true;
		else if(type.Name == "MTGUG") extra_window_radio_run_interval_mtgug.Active = true;
		else if(type.Name == "RSA 8-4-R3-5") extra_window_radio_run_interval_rsa_test_1.Active = true;
		else {
			//don't do this:
			//extra_window_radio_run_interval_more.Active = true;
			//because it will be a loop
			//only do:
			extra_window_runs_interval_initialize(type);
		}
	}

	private void extra_window_showDistanceData (RunType myRunType, bool show, bool sensitive ) {
		if(myRunType.HasIntervals) {
			extra_window_runs_interval_label_distance.Visible = show;
			extra_window_runs_interval_spinbutton_distance.Visible = show;
			extra_window_runs_interval_label_distance_units.Visible = show;
		
			extra_window_runs_interval_label_distance.Sensitive = sensitive;
			extra_window_runs_interval_spinbutton_distance.Sensitive = sensitive;
			extra_window_runs_interval_label_distance_units.Sensitive = sensitive;
		} else {
			extra_window_runs_label_distance.Visible = show;
			extra_window_runs_spinbutton_distance.Visible = show;
			extra_window_runs_label_distance_units.Visible = show;
		
			extra_window_runs_label_distance.Sensitive = sensitive;
			extra_window_runs_spinbutton_distance.Sensitive = sensitive;
			extra_window_runs_label_distance_units.Sensitive = sensitive;
		}
	}
	
	private void extra_window_showLimitData (bool show, bool sensitive ) {
		extra_window_runs_interval_label_limit.Visible = show;
		extra_window_runs_interval_spinbutton_limit.Visible = show;
		extra_window_runs_interval_label_limit_units.Visible = show;
		
		extra_window_runs_interval_label_limit.Sensitive = sensitive;
		extra_window_runs_interval_spinbutton_limit.Sensitive = sensitive;
		extra_window_runs_interval_label_limit_units.Sensitive = sensitive;
	}
	
	private void extra_window_runs_showNoOptions(RunType myRunType, bool hasOptions) {
		if(myRunType.HasIntervals) 
			extra_window_label_runs_interval_no_options.Visible = ! hasOptions;
		else 
			extra_window_label_runs_no_options.Visible = ! hasOptions;
	}

	protected void on_checkbutton_runs_prevent_double_contact_toggled (object o, EventArgs args) {
		vbox_runs_prevent_double_contact.Visible = checkbutton_runs_prevent_double_contact.Active;
	}

}


//--------------------------------------------------------
//---------------- runs_more widget ----------------------
//--------------------------------------------------------

public class RunsMoreWindow : EventMoreWindow 
{
	[Widget] Gtk.Window jumps_runs_more;
	static RunsMoreWindow RunsMoreWindowBox;
	
	private double selectedDistance;
	
	RunsMoreWindow (Gtk.Window parent, bool testOrDelete) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "jumps_runs_more", null);
		gladeXML.Autoconnect(this);
		this.parent = parent;
		this.testOrDelete = testOrDelete;
		
		if(!testOrDelete)
			jumps_runs_more.Title = Catalog.GetString("Delete test type defined by user");
		
		//put an icon to window
		UtilGtk.IconWindow(jumps_runs_more);

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
						myStringFull[1],	//name 
						myStringFull[2], 	//distance
						description
						);
		}	
	}

	protected override void onSelectionEntry (object o, EventArgs args)
	{
		TreeModel model;
		TreeIter iter;
		selectedEventName = "-1";
		selectedDistance = 0;
		selectedDescription = "";

		if (((TreeSelection)o).GetSelected(out model, out iter)) {
			selectedEventName = (string) model.GetValue (iter, 0);
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
		TreeModel model;
		TreeIter iter;

		if (tv.Selection.GetSelected (out model, out iter)) {
			//put selection in selected
			selectedEventName = (string) model.GetValue (iter, 0);
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
	}

	protected override string [] findTestTypesInSessions() {
		return SqliteRun.SelectRuns(-1, -1, selectedEventName); 
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
	[Widget] Gtk.Window jumps_runs_more;
	static RunsIntervalMoreWindow RunsIntervalMoreWindowBox;

	private double selectedDistance;
	private bool selectedTracksLimited;
	private int selectedLimitedValue;
	private bool selectedUnlimited;
	private string selectedDistancesString;
	
	RunsIntervalMoreWindow (Gtk.Window parent, bool testOrDelete) {
		//the glade window is the same as jumps_more
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "jumps_runs_more", null);
		gladeXML.Autoconnect(this);
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
		//select data without inserting an "all jumps", and not obtain only name of jump
		string [] myTypes = SqliteRunIntervalType.SelectRunIntervalTypes("", false);
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
				myLimiter = Catalog.GetString("Tracks");
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
						myStringFull[1],	//name 
						distance,		
						myLimiter,		//tracks or seconds or "unlimited"
						myLimiterValue,		//? or exact value (or '-' in unlimited)
						description
						);
		}	
	}

	//puts a value in private member selected
	protected override void onSelectionEntry (object o, EventArgs args)
	{
		TreeModel model;
		TreeIter iter;
		selectedEventName = "-1";
		selectedDistance = -1;
		selectedTracksLimited = false;
		selectedLimitedValue = 0;
		selectedUnlimited = false; //true if it's an unlimited run
		selectedDescription = "";
		selectedDistancesString = "";

		if (((TreeSelection)o).GetSelected(out model, out iter)) {
			selectedEventName = (string) model.GetValue (iter, 0);

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

			if( (string) model.GetValue (iter, 2) == Catalog.GetString("Tracks") ) {
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
		TreeModel model;
		TreeIter iter;

		if (tv.Selection.GetSelected (out model, out iter)) {
			selectedEventName = (string) model.GetValue (iter, 0);
			
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
			
			if( (string) model.GetValue (iter, 2) == Catalog.GetString("Tracks") ) {
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
	}

	protected override string [] findTestTypesInSessions() {
		return SqliteRunInterval.SelectRuns(-1, -1, selectedEventName); 
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
