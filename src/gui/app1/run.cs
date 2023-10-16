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
using Mono.Unix;


//--------------------------------------------------------
//---------------- run extra WIDGET --------------------
//---------------- in 0.9.3 included in main gui ---------
//--------------------------------------------------------

public partial class ChronoJumpWindow
{
	// at glade ---->
	//options runs
	Gtk.Button button_combo_runs_exercise_capture_left;
	Gtk.Button button_combo_runs_exercise_capture_right;
	Gtk.Button button_run_type_delete_simple;
	Gtk.CheckButton check_run_simple_with_reaction_time;
	Gtk.Button button_runs_simple_track_distance;
	Gtk.Label label_runs_simple_track_distance_value;
	Gtk.Label label_runs_simple_track_distance_units;
			
	//options runs interval
	Gtk.Button button_combo_runs_interval_exercise_capture_left;
	Gtk.Button button_combo_runs_interval_exercise_capture_right;
	Gtk.Button button_run_type_delete_interval;
	Gtk.Button button_runs_interval_track_distance;
	Gtk.Label label_runs_interval_track_distance_value;
	//Gtk.Label label_runs_interval_track_distance_units; //always "m"
	Gtk.HBox hbox_runs_limited_by_tracks;
	Gtk.HBox hbox_runs_limited_by_time;
	Gtk.SpinButton extra_window_runs_interval_spinbutton_limit_tracks;
	Gtk.SpinButton extra_window_runs_interval_spinbutton_limit_time;
	Gtk.CheckButton check_run_interval_with_reaction_time;

	Gtk.Box box_contacts_export_data_runs;
	Gtk.CheckButton check_contacts_export_runs_simple;
	Gtk.CheckButton check_contacts_export_runs_intervallic;
	// <---- at glade
	

	double extra_window_runs_distance = 10;
	double extra_window_runs_interval_distance = 10;
	double extra_window_runs_interval_limit_tracks = 3;
	double extra_window_runs_interval_limit_time = 10;

	private RunType previousRunType; //used on More to turnback if cancel or delete event is pressed
	private RunType previousRunIntervalType; //used on More to turnback if cancel or delete event is pressed


	//creates and if is not predefined, checks database to gather all the data
	//simple == true  for simple runs, and false for intervallic
	private RunType createRunType(string name, bool simple) {
		RunType t = new RunType(name);
		
		if(! t.IsPredefined) {
			if(simple) {
				t = SqliteRunType.SelectAndReturnRunType(name, false);
				t.ImageFileName = SqliteEvent.GraphLinkSelectFileName(Constants.RunTable, name);
			} else {
				t = SqliteRunIntervalType.SelectAndReturnRunIntervalType(name, false);
				t.ImageFileName = SqliteEvent.GraphLinkSelectFileName(Constants.RunIntervalTable, name);
			}
		}
		return t;
	}
	
	//left-right buttons on runs combo exercise selection
	private void on_button_combo_runs_exercise_capture_left_clicked (object o, EventArgs args)
	{
		contacts_exercise_left_button (combo_select_runs,
				button_combo_runs_exercise_capture_left,
				button_combo_runs_exercise_capture_right);
	}
	private void on_button_combo_runs_exercise_capture_right_clicked (object o, EventArgs args)
	{
		contacts_exercise_right_button (combo_select_runs,
				button_combo_runs_exercise_capture_left,
				button_combo_runs_exercise_capture_right);
	}

	//left-right buttons on runs_interval combo exercise selection
	private void on_button_combo_runs_interval_exercise_capture_left_clicked (object o, EventArgs args)
	{
		contacts_exercise_left_button (combo_select_runs_interval,
				button_combo_runs_interval_exercise_capture_left,
				button_combo_runs_interval_exercise_capture_right);
	}
	private void on_button_combo_runs_interval_exercise_capture_right_clicked (object o, EventArgs args)
	{
		contacts_exercise_right_button (combo_select_runs_interval,
				button_combo_runs_interval_exercise_capture_left,
				button_combo_runs_interval_exercise_capture_right);
	}
	
	private void on_extra_window_runs_test_changed(object o, EventArgs args)
	{
		string runEnglishName = comboSelectRuns.GetSelectedNameEnglish();
		currentRunType = createRunType(runEnglishName, true);

		//change name of this radio but only if changed (to not have it flickering on hpaned1 displacement)
		if(radio_contacts_graph_currentTest.Label != Catalog.GetString(currentRunType.Name))
			radio_contacts_graph_currentTest.Label = Catalog.GetString(currentRunType.Name);
		
		extra_window_runs_initialize(currentRunType);
	}

	private void on_extra_window_runs_more(object o, EventArgs args)
	{
		previousRunType = currentRunType;

		runsMoreWin = RunsMoreWindow.Show(app1, true);
		runsMoreWin.Button_accept.Clicked += new EventHandler(on_more_runs_accepted);
		runsMoreWin.Button_cancel.Clicked += new EventHandler(on_more_runs_cancelled);
		runsMoreWin.Button_selected.Clicked += new EventHandler(on_more_runs_update_test);
	}
	
	private void on_extra_window_runs_interval_test_changed(object o, EventArgs args)
	{
		string runEnglishName = comboSelectRunsI.GetSelectedNameEnglish();
		currentRunIntervalType = createRunType(runEnglishName, false);

		//change name of this radio but only if changed (to not have it flickering on hpaned1 displacement)
		if(radio_contacts_graph_currentTest.Label != Catalog.GetString(currentRunIntervalType.Name))
			radio_contacts_graph_currentTest.Label = Catalog.GetString(currentRunIntervalType.Name);
		
		extra_window_runs_interval_initialize(currentRunIntervalType);
	}
	
	private void on_extra_window_runs_interval_more(object o, EventArgs args)
	{
		previousRunIntervalType = currentRunIntervalType;

		runsIntervalMoreWin = RunsIntervalMoreWindow.Show(app1, true);
		runsIntervalMoreWin.Button_accept.Clicked += new EventHandler(on_more_runs_interval_accepted);
		runsIntervalMoreWin.Button_cancel.Clicked += new EventHandler(on_more_runs_interval_cancelled);
		runsIntervalMoreWin.Button_selected.Clicked += new EventHandler(on_more_runs_interval_update_test);
	}
	
	private void extra_window_runs_initialize(RunType myRunType) 
	{
		currentEventType = myRunType;
		changeTestImage(EventType.Types.RUN.ToString(), myRunType.Name, myRunType.ImageFileName);
		setLabelContactsExerciseSelected(Constants.Modes.RUNSSIMPLE);

		label_runs_simple_track_distance_units.Text = "m";

		if(myRunType.Distance > 0) {
			label_runs_simple_track_distance_value.Text = myRunType.Distance.ToString();
			extra_window_showDistanceData(myRunType, true, false);	//visible, sensitive
		} else {
			if(myRunType.Name == "Margaria") {
				label_runs_simple_track_distance_value.Text = "1050";
				label_runs_simple_track_distance_units.Text = "mm";
			} else {
				label_runs_simple_track_distance_value.Text = extra_window_runs_distance.ToString();
			}
			extra_window_showDistanceData(myRunType, true, true);	//visible, sensitive
		}

		button_run_type_delete_simple.Sensitive = ! myRunType.IsPredefined;

		updateGraphRunsSimple();
		setLabelContactsExerciseSelectedOptionsRunsSimple();
	}
	private void updateGraphRunsSimple () 
	{
		if(currentPerson == null || currentSession == null)
			return;

		//intializeVariables if not done before
		event_execute_initializeVariables(
			(! cp2016.StoredCanCaptureContacts && ! cp2016.StoredWireless), //is simulated
			currentPerson.UniqueID, 
			currentPerson.Name, 
			Catalog.GetString("Phases"),  	  //name of the different moments
			Constants.RunTable, //tableName
			currentRunType.Name 
			);

		string typeTemp = currentEventType.Name;
		if(radio_contacts_graph_allTests.Active)
			typeTemp = "";

		int selectedID = -1;
		if (myTreeViewRuns != null && myTreeViewRuns.EventSelectedID > 0)
			selectedID = myTreeViewRuns.EventSelectedID;

		PrepareEventGraphRunSimple eventGraph = new PrepareEventGraphRunSimple(
				1, 1, //both unused
				currentSession.UniqueID,
				currentPerson.UniqueID, radio_contacts_results_personAll.Active,
				-1 * Convert.ToInt32 (spin_contacts_graph_last_limit.Value), //negative: end limit
				Constants.RunTable, typeTemp, selectedID);
		
		//if(eventGraph.personMAXAtSQLAllSessions > 0 || eventGraph.runsAtSQL.Count > 0)
		//	PrepareRunSimpleGraph(eventGraph, false); //don't animate

		string personStr = "";
		if(! radio_contacts_results_personAll.Active)
			personStr = currentPerson.Name;

		LogB.Information("event_execute_drawingarea_cairo == null: ",
			(event_execute_drawingarea_cairo == null).ToString());

		cairoPaintBarsPre = new CairoPaintBarsPreRunSimple (
				event_execute_drawingarea_cairo, preferences.fontType.ToString(), current_mode,
				personStr, typeTemp, preferences.digitsNumber);

		cairoPaintBarsPre.StoreEventGraphRuns (eventGraph);
		//PrepareRunSimpleGraph(cairoPaintBarsPre.eventGraphRunsStored, false); //do not need, draw event will graph it:
		event_execute_drawingarea_cairo.QueueDraw ();

		cairoManageRunDoubleContacts = new CairoManageRunDoubleContacts (
				event_execute_drawingarea_run_simple_double_contacts, preferences.fontType.ToString() );
	}
	private void updateGraphRunsInterval ()
	{
		if(currentPerson == null || currentSession == null)
			return;

		//we do not plot graph, but we want to update label event_graph_label_graph_test
		//intializeVariables if not done before
		event_execute_initializeVariables(
			(! cp2016.StoredCanCaptureContacts && ! cp2016.StoredWireless), //is simulated
			currentPerson.UniqueID,
			currentPerson.Name,
			Catalog.GetString("Phases"),  	  //name of the different moments
			Constants.RunIntervalTable, //tableName
			currentRunIntervalType.Name
			);

		string typeTemp = currentEventType.Name;
		if(radio_contacts_graph_allTests.Active)
			typeTemp = "";

		int selectedID = -1;
		if (myTreeViewRunsInterval != null && myTreeViewRunsInterval.EventSelectedID > 0)
			selectedID = myTreeViewRunsInterval.EventSelectedID;

		PrepareEventGraphRunInterval eventGraph = new PrepareEventGraphRunInterval(
				currentSession.UniqueID, currentPerson.UniqueID,
				radio_contacts_results_personAll.Active,
				-1 * Convert.ToInt32 (spin_contacts_graph_last_limit.Value), //negative: end limit
				typeTemp, selectedID);

		string personStr = "";
		if(! radio_contacts_results_personAll.Active)
			personStr = currentPerson.Name;

		cairoPaintBarsPre = new CairoPaintBarsPreRunInterval (
				event_execute_drawingarea_cairo, preferences.fontType.ToString(), current_mode,
				personStr, typeTemp, preferences.digitsNumber);

		cairoPaintBarsPre.StoreEventGraphRunsInterval (eventGraph);
		//PrepareRunIntervalGraph (cairoPaintBarsPre.eventGraphRunsIntervalStored, false); //do not need, draw event will graph it:
		event_execute_drawingarea_cairo.QueueDraw ();

		cairoManageRunDoubleContacts = new CairoManageRunDoubleContacts (
				event_execute_drawingarea_run_simple_double_contacts, preferences.fontType.ToString() );
	}
	
	private void extra_window_runs_interval_initialize(RunType myRunType) 
	{
		currentEventType = myRunType;
		changeTestImage(EventType.Types.RUN.ToString(), myRunType.Name, myRunType.ImageFileName);
		setLabelContactsExerciseSelected(Constants.Modes.RUNSINTERVALLIC);

		if(myRunType.Distance > 0) {
			label_runs_interval_track_distance_value.Text = myRunType.Distance.ToString();
			extra_window_showDistanceData(myRunType, true, false);	//visible, sensitive
		} else if(myRunType.Distance == 0) {
			label_runs_interval_track_distance_value.Text = extra_window_runs_interval_distance.ToString();
			extra_window_showDistanceData(myRunType, true, true);	//visible, sensitive
		} else { //variableDistancesString (eg. MTGUG) don't show anything
			extra_window_showDistanceData(myRunType, false, false);	//visible, sensitive
		}

		if(! myRunType.Unlimited)
		{
			if(myRunType.FixedValue > 0)
			{
				if(myRunType.TracksLimited)
					extra_window_runs_interval_spinbutton_limit_tracks.Value = myRunType.FixedValue;
				else
					extra_window_runs_interval_spinbutton_limit_time.Value = myRunType.FixedValue;

				extra_window_showLimitData (myRunType.TracksLimited, true, false);	//visible, sensitive
			} else {
				extra_window_runs_interval_spinbutton_limit_tracks.Value = extra_window_runs_interval_limit_tracks;
				extra_window_runs_interval_spinbutton_limit_time.Value = extra_window_runs_interval_limit_time;

				//set minimum value == 1
				double min; double max;
				if (myRunType.TracksLimited)
				{
					extra_window_runs_interval_spinbutton_limit_tracks.GetRange(out min, out max);
					extra_window_runs_interval_spinbutton_limit_tracks.SetRange(1, max);
				} else {
					extra_window_runs_interval_spinbutton_limit_time.GetRange(out min, out max);
					extra_window_runs_interval_spinbutton_limit_time.SetRange(1, max);
				}
				
				extra_window_showLimitData (myRunType.TracksLimited, true, true);	//visible, sensitive
			}
		} else {
			extra_window_showLimitData (myRunType.TracksLimited, false, false);	//visible, sensitive
		}

		button_run_type_delete_interval.Sensitive = ! myRunType.IsPredefined;

		updateGraphRunsInterval();
		setLabelContactsExerciseSelectedOptionsRunsInterval();
	}

	private void on_more_runs_update_test (object o, EventArgs args) 
	{
		currentEventType = new RunType(runsMoreWin.SelectedEventName);
		comboSelectRuns.MakeActive(runsMoreWin.SelectedEventName);
	}
	
	private void on_more_runs_interval_update_test (object o, EventArgs args)
	{
		currentEventType = new RunType(runsIntervalMoreWin.SelectedEventName);
		comboSelectRunsI.MakeActive(runsIntervalMoreWin.SelectedEventName);
	}
	
	
	//used from the dialogue "runs more"
	private void on_more_runs_accepted (object o, EventArgs args) 
	{
		runsMoreWin.Button_accept.Clicked -= new EventHandler(on_more_runs_accepted);
	
		currentRunType = createRunType(runsMoreWin.SelectedEventName, true);
		
		extra_window_runs_initialize(currentRunType);
				
		//destroy the win for not having updating problems if a new run type is created
		runsMoreWin.Destroy();
	}
	
	private void on_more_runs_interval_accepted (object o, EventArgs args) 
	{
		runsIntervalMoreWin.Button_accept.Clicked -= new EventHandler(on_more_runs_interval_accepted);
		
		currentRunIntervalType = createRunType(runsIntervalMoreWin.SelectedEventName, false);

		extra_window_runs_interval_initialize(currentRunIntervalType);

		//destroy the win for not having updating problems if a new runInterval type is created
		runsIntervalMoreWin.Destroy();
	}
	
	//if it's cancelled (or deleted event) select desired toolbar button
	private void on_more_runs_cancelled (object o, EventArgs args) 
	{
		currentRunType = previousRunType;
		combo_select_runs.Active = UtilGtk.ComboMakeActive(combo_select_runs, currentRunType.Name);
	}
	
	private void on_more_runs_interval_cancelled (object o, EventArgs args) 
	{
		currentRunIntervalType = previousRunIntervalType;
		combo_select_runs_interval.Active = UtilGtk.ComboMakeActive(combo_select_runs_interval, currentRunIntervalType.Name);
	}

	private void extra_window_showDistanceData (RunType myRunType, bool show, bool sensitive ) {
		if(myRunType.HasIntervals) {
			button_runs_interval_track_distance.Visible = show;
			button_runs_interval_track_distance.Sensitive = sensitive;
		} else {
			button_runs_simple_track_distance.Visible = show;
			button_runs_simple_track_distance.Sensitive = sensitive;
		}
	}
	
	private void extra_window_showLimitData (bool tracksLimited, bool show, bool sensitive )
	{
		if (tracksLimited)
		{
			hbox_runs_limited_by_time.Visible = false;

			hbox_runs_limited_by_tracks.Visible = show;
			extra_window_runs_interval_spinbutton_limit_tracks.Sensitive = sensitive;
		}
		else
		{
			hbox_runs_limited_by_tracks.Visible = false;

			hbox_runs_limited_by_time.Visible = show;
			extra_window_runs_interval_spinbutton_limit_time.Sensitive = sensitive;
		}
	}

	private void on_extra_window_runs_interval_spinbutton_limit_value_changed (object o, EventArgs args)
	{
		setLabelContactsExerciseSelectedOptionsRunsInterval();
	}

	// ----
	// ---- start of track distance
	// ----

	// ---- 1) gui calls

	private void on_button_runs_simple_track_distance_clicked (object o, EventArgs args)
	{
		string text = Catalog.GetString("Lap distance (between barriers)");
		string labelAtLeft = Catalog.GetString("Distance in meters");

		if(currentRunType.Name == "Margaria")
		{
			text = Catalog.GetString("Vertical distance between stairs third and nine.");
			labelAtLeft = Catalog.GetString("Distance in millimeters");
		}

		createGenericWinForTrackDistance(true, text, labelAtLeft,
				Convert.ToDouble(label_runs_simple_track_distance_value.Text));
	}

	private void on_button_runs_interval_track_distance_clicked (object o, EventArgs args)
	{
		string text = Catalog.GetString("Lap distance (between barriers)");
		string labelAtLeft = Catalog.GetString("Distance in meters");

		createGenericWinForTrackDistance(false, text, labelAtLeft,
				Convert.ToDouble(label_runs_interval_track_distance_value.Text));
	}

	// ---- 2) create genericWin

	private void createGenericWinForTrackDistance(bool simpleOrInterval, string text, string labelAtLeft, double initialValue)
	{
		genericWin = GenericWindow.Show(Catalog.GetString("Track distance"), text, Constants.GenericWindowShow.HBOXSPINDOUBLE2, true);

		genericWin.LabelSpinDouble2 = labelAtLeft;
		genericWin.SetSpinDouble2Increments(0.1, 1);
		genericWin.SetSpinDouble2Range(0, 100000.0);
		genericWin.SetSpinDouble2Digits(1);
		genericWin.SetSpinDouble2Value(initialValue);

		if(simpleOrInterval) {
			genericWin.Button_accept.Clicked -= new EventHandler(on_button_runs_simple_track_distance_accepted);
			genericWin.Button_accept.Clicked += new EventHandler(on_button_runs_simple_track_distance_accepted);
		} else {
			genericWin.Button_accept.Clicked -= new EventHandler(on_button_runs_interval_track_distance_accepted);
			genericWin.Button_accept.Clicked += new EventHandler(on_button_runs_interval_track_distance_accepted);
		}
	}

	// ---- 3) return from genericWin
	void on_button_runs_simple_track_distance_accepted (object obj, EventArgs args)
	{
		genericWin.Button_accept.Clicked -= new EventHandler(on_button_runs_simple_track_distance_accepted);

		label_runs_simple_track_distance_value.Text = Util.TrimDecimals(genericWin.SpinDouble2Selected.ToString(),
				preferences.digitsNumber);

		setLabelContactsExerciseSelectedOptionsRunsSimple();
	}

	void on_button_runs_interval_track_distance_accepted (object obj, EventArgs args)
	{
		genericWin.Button_accept.Clicked -= new EventHandler(on_button_runs_interval_track_distance_accepted);

		label_runs_interval_track_distance_value.Text = Util.TrimDecimals(genericWin.SpinDouble2Selected.ToString(),
				preferences.digitsNumber);

		setLabelContactsExerciseSelectedOptionsRunsInterval();
	}

	// ---- end of track distance

	private bool changingCheckboxesRunWithReactionTime = false;
	private void on_check_run_simple_with_reaction_time_clicked (object o, EventArgs args)
	{
		//avoid cyclic calls
		if(changingCheckboxesRunWithReactionTime)
			return;

		changingCheckboxesRunWithReactionTime = true;
		check_run_interval_with_reaction_time.Active = check_run_simple_with_reaction_time.Active;
		changingCheckboxesRunWithReactionTime = false;
	}
	private void on_check_run_interval_with_reaction_time_clicked (object o, EventArgs args)
	{
		//avoid cyclic calls
		if(changingCheckboxesRunWithReactionTime)
			return;

		changingCheckboxesRunWithReactionTime = true;
		check_run_simple_with_reaction_time.Active = check_run_interval_with_reaction_time.Active;
		changingCheckboxesRunWithReactionTime = false;
	}

	//valid for simple and for intervallic
	private void on_button_run_with_reaction_time_help_clicked (object o, EventArgs args)
	{
		new DialogMessage(Constants.MessageTypes.INFO,
				"\nThis feature needs a new Chronojump device under development." +
				"\n\nTest will start when person is at start pad and push button is pressed" +
				"\nreaction time will be in ms and as a comment on Description"
				);
	}


	// ---- save runs simple image start ---->

	private void on_button_runs_capture_save_image_selected (string destination)
	{
		if(event_execute_drawingarea_cairo == null)
			return;

		LogB.Information("Saving");
		CairoUtil.GetScreenshotFromDrawingArea (event_execute_drawingarea_cairo, destination);
	}
	private void on_overwrite_file_runs_capture_save_image_accepted (object o, EventArgs args)
	{
		on_button_runs_capture_save_image_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), exportFileName);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}

	// <---- save runs simple image end ----

	private void connectWidgetsRun (Gtk.Builder builder)
	{
		//options runs
		button_combo_runs_exercise_capture_left = (Gtk.Button) builder.GetObject ("button_combo_runs_exercise_capture_left");
		button_combo_runs_exercise_capture_right = (Gtk.Button) builder.GetObject ("button_combo_runs_exercise_capture_right");
		button_run_type_delete_simple = (Gtk.Button) builder.GetObject ("button_run_type_delete_simple");
		check_run_simple_with_reaction_time = (Gtk.CheckButton) builder.GetObject ("check_run_simple_with_reaction_time");
		button_runs_simple_track_distance = (Gtk.Button) builder.GetObject ("button_runs_simple_track_distance");
		label_runs_simple_track_distance_value = (Gtk.Label) builder.GetObject ("label_runs_simple_track_distance_value");
		label_runs_simple_track_distance_units = (Gtk.Label) builder.GetObject ("label_runs_simple_track_distance_units");

		//options runs interval
		button_combo_runs_interval_exercise_capture_left = (Gtk.Button) builder.GetObject ("button_combo_runs_interval_exercise_capture_left");
		button_combo_runs_interval_exercise_capture_right = (Gtk.Button) builder.GetObject ("button_combo_runs_interval_exercise_capture_right");
		button_run_type_delete_interval = (Gtk.Button) builder.GetObject ("button_run_type_delete_interval");
		button_runs_interval_track_distance = (Gtk.Button) builder.GetObject ("button_runs_interval_track_distance");
		label_runs_interval_track_distance_value = (Gtk.Label) builder.GetObject ("label_runs_interval_track_distance_value");
		//label_runs_interval_track_distance_units = (Gtk.Label) builder.GetObject ("label_runs_interval_track_distance_units"); //always "m"
		hbox_runs_limited_by_tracks = (Gtk.HBox) builder.GetObject ("hbox_runs_limited_by_tracks");
		hbox_runs_limited_by_time = (Gtk.HBox) builder.GetObject ("hbox_runs_limited_by_time");
		extra_window_runs_interval_spinbutton_limit_tracks = (Gtk.SpinButton) builder.GetObject ("extra_window_runs_interval_spinbutton_limit_tracks");
		extra_window_runs_interval_spinbutton_limit_time = (Gtk.SpinButton) builder.GetObject ("extra_window_runs_interval_spinbutton_limit_time");
		check_run_interval_with_reaction_time = (Gtk.CheckButton) builder.GetObject ("check_run_interval_with_reaction_time");

		box_contacts_export_data_runs = (Gtk.Box) builder.GetObject ("box_contacts_export_data_runs");
		check_contacts_export_runs_simple = (Gtk.CheckButton) builder.GetObject ("check_contacts_export_runs_simple");
		check_contacts_export_runs_intervallic = (Gtk.CheckButton) builder.GetObject ("check_contacts_export_runs_intervallic");
	}
}


