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
//---------------- jump extra WIDGET --------------------
//---------------- in 0.9.3 included in main gui ---------
//--------------------------------------------------------

public partial class ChronoJumpWindow
{
	// at glade ---->
	//options jumps
	Gtk.Button button_combo_jumps_exercise_capture_left;
	Gtk.Button button_combo_jumps_exercise_capture_right;
	Gtk.Button button_jump_type_delete_simple;
	Gtk.SpinButton extra_window_jumps_spinbutton_weight;
	Gtk.HBox extra_window_jumps_simple_hbox_start_inside;
	Gtk.Box extra_window_jumps_hbox_fall;
	Gtk.CheckButton extra_window_jumps_check_dj_fall_calculate;
	Gtk.Label extra_window_jumps_label_dj_start_inside;
	Gtk.Label extra_window_jumps_label_dj_start_outside;
	Gtk.Image extra_window_jumps_image_fall;
	Gtk.Image extra_window_jumps_image_weight;
	Gtk.SpinButton extra_window_jumps_spinbutton_fall;
	Gtk.Grid grid_extra_window_jumps_weight;
	Gtk.RadioButton extra_window_jumps_radiobutton_kg;
	Gtk.RadioButton extra_window_jumps_radiobutton_weight;
	//Gtk.Label extra_window_jumps_label_weight;
	Gtk.CheckButton extra_window_jumps_check_dj_arms;

	//show weight on kg when percent is selected (SJl, CMJl, ABKl)
	Gtk.Label label_extra_window_jumps_radiobutton_weight_percent_as_kg;
	Gtk.Label label_extra_window_jumps_rj_radiobutton_weight_percent_as_kg;

	//slCMJ	
	Gtk.Grid grid_extra_window_jumps_single_leg_radios;
	Gtk.RadioButton extra_window_jumps_radiobutton_single_leg_mode_vertical;
	Gtk.RadioButton extra_window_jumps_radiobutton_single_leg_mode_horizontal;
	Gtk.RadioButton extra_window_jumps_radiobutton_single_leg_dominance_this_limb;
	Gtk.RadioButton extra_window_jumps_radiobutton_single_leg_dominance_opposite;
	Gtk.RadioButton extra_window_jumps_radiobutton_single_leg_fall_this_limb;
	Gtk.RadioButton extra_window_jumps_radiobutton_single_leg_fall_opposite;
	Gtk.SpinButton extra_window_jumps_spin_single_leg_distance;
	Gtk.SpinButton extra_window_jumps_spin_single_leg_angle;
	
	//options jumps_rj
	Gtk.Button button_combo_jumps_rj_exercise_capture_left;
	Gtk.Button button_combo_jumps_rj_exercise_capture_right;
	Gtk.Button button_jump_type_delete_reactive;
	Gtk.Label extra_window_jumps_rj_label_limit;
	Gtk.SpinButton extra_window_jumps_rj_spinbutton_limit;
	Gtk.Label extra_window_jumps_rj_label_limit_units;
	Gtk.SpinButton extra_window_jumps_rj_spinbutton_weight;
	Gtk.SpinButton extra_window_jumps_rj_spinbutton_fall;
	Gtk.RadioButton extra_window_jumps_rj_radiobutton_kg;
	Gtk.RadioButton extra_window_jumps_rj_radiobutton_weight;
	Gtk.Grid grid_extra_window_jumps_rj_weight;
	Gtk.HBox hbox_extra_window_jumps_rj_hbox_fall;
	Gtk.Image extra_window_jumps_rj_image_fall;
	Gtk.Image extra_window_jumps_rj_image_weight;
	Gtk.Label extra_window_jumps_label_rj_start_inside;
	Gtk.Label extra_window_jumps_label_rj_start_outside;
	Gtk.CheckButton checkbutton_allow_finish_rj_after_time;

	Gtk.Button button_jumps_extra_weight_minus_10;
	Gtk.Button button_jumps_extra_weight_minus_1;
	Gtk.Button button_jumps_extra_weight_plus_1;
	Gtk.Button button_jumps_extra_weight_plus_10;
	Gtk.Button button_jumps_rj_extra_weight_minus_10;
	Gtk.Button button_jumps_rj_extra_weight_minus_1;
	Gtk.Button button_jumps_rj_extra_weight_plus_1;
	Gtk.Button button_jumps_rj_extra_weight_plus_10;
	Gtk.Button button_jumps_extra_fall_minus_10;
	Gtk.Button button_jumps_extra_fall_minus_1;
	Gtk.Button button_jumps_extra_fall_plus_1;
	Gtk.Button button_jumps_extra_fall_plus_10;
	Gtk.Button button_jumps_rj_extra_fall_minus_10;
	Gtk.Button button_jumps_rj_extra_fall_minus_1;
	Gtk.Button button_jumps_rj_extra_fall_plus_1;
	Gtk.Button button_jumps_rj_extra_fall_plus_10;

	Gtk.Box box_contacts_export_data_jumps;
	Gtk.CheckButton check_contacts_export_jumps_simple;
	Gtk.CheckButton check_contacts_export_jumps_simple_mean_max_tables;
	Gtk.CheckButton check_contacts_export_jumps_reactive;
	// <---- at glade

	//for RunAnalysis
	//but will be used and recorded with "fall"
	//static double distance;

	//jumps
	string extra_window_jumps_option = "%";
	//double extra_window_jumps_weight = 20;
	double extra_window_jumps_fall = 20;
	bool extra_window_jumps_arms = false;
	
	//jumps_rj
	double extra_window_jumps_rj_limited = 10;
	bool extra_window_jumps_rj_jumpsLimited;
	string extra_window_jumps_rj_option = "%";
	double extra_window_jumps_rj_weight = 20;
	double extra_window_jumps_rj_fall = 20;
	
	private JumpType previousJumpType; //used on More to turnback if cancel or delete event is pressed
	private JumpType previousJumpRjType; //used on More to turnback if cancel or delete event is pressed
	
	
	//creates and if is not predefined, checks database to gather all the data
	//simple == true  for simple jumps, and false for reactive
	private JumpType createJumpType(string name, bool simple)
	{
		JumpType t = new JumpType(name);

		//on networks need the uniquedID, so we need to do the SQL selection
		if(! t.IsPredefined || configChronojump.Compujump)
		{
			if(simple) {
				t = SqliteJumpType.SelectAndReturnJumpType(name, false);
				t.ImageFileName = SqliteEvent.GraphLinkSelectFileName(Constants.JumpTable, name);
			} else {
				t = SqliteJumpType.SelectAndReturnJumpRjType(name, false);
				t.ImageFileName = SqliteEvent.GraphLinkSelectFileName(Constants.JumpRjTable, name);
			}
		}

		return t;
	}

	//left-right buttons on jumps combo exercise selection
	private void on_button_combo_jumps_exercise_capture_left_clicked (object o, EventArgs args)
	{
		contacts_exercise_left_button (combo_select_jumps,
				button_combo_jumps_exercise_capture_left, button_combo_jumps_exercise_capture_right);
	}
	private void on_button_combo_jumps_exercise_capture_right_clicked (object o, EventArgs args)
	{
		contacts_exercise_right_button (combo_select_jumps,
				button_combo_jumps_exercise_capture_left, button_combo_jumps_exercise_capture_right);
	}

	//left-right buttons on jumps_rj combo exercise selection
	private void on_button_combo_jumps_rj_exercise_capture_left_clicked (object o, EventArgs args)
	{
		contacts_exercise_left_button (combo_select_jumps_rj,
				button_combo_jumps_rj_exercise_capture_left, button_combo_jumps_rj_exercise_capture_right);
	}
	private void on_button_combo_jumps_rj_exercise_capture_right_clicked (object o, EventArgs args)
	{
		contacts_exercise_right_button (combo_select_jumps_rj,
				button_combo_jumps_rj_exercise_capture_left, button_combo_jumps_rj_exercise_capture_right);
	}

	/*
	 * when pre-jump from inside, dj fall is calculated
	 * when fall down from outside, the dj fall is predefined
	 */
	private void on_extra_window_jumps_check_dj_fall_calculate_toggled (object o, EventArgs args) 
	{
		bool calculate = extra_window_jumps_check_dj_fall_calculate.Active;

		extra_windows_jumps_image_dj_fall_calculate.Visible = calculate;
		extra_window_jumps_label_dj_start_inside.Visible = calculate;
		
		extra_windows_jumps_image_dj_fall_predefined.Visible = ! calculate;
		extra_window_jumps_label_dj_start_outside.Visible = ! calculate;
		hbox_extra_window_jumps_fall_height.Visible = ! calculate;

		if(calculate) {
			if(extra_window_jumps_check_dj_arms.Active) 
				changeTestImage("","", "jump_dj_a_inside.png");
			else
				changeTestImage("","", "jump_dj_inside.png");
		} else {
			if(extra_window_jumps_check_dj_arms.Active) 
				changeTestImage("","", "jump_dj_a.png");
			else
				changeTestImage("","", "jump_dj.png");
		}

		setLabelContactsExerciseSelectedOptionsJumpsSimple();
	}
	
	private void on_extra_window_jumps_test_changed(object o, EventArgs args)
	{
		string jumpEnglishName = comboSelectJumps.GetSelectedNameEnglish();
		if(jumpEnglishName == "") //if there are no jump types
			return;

		currentJumpType = createJumpType(jumpEnglishName, true);

		//change name of this radio but only if changed (to not have it flickering on hpaned1 displacement)
		if(radio_contacts_graph_currentTest.Label != Catalog.GetString(currentJumpType.Name))
			radio_contacts_graph_currentTest.Label = Catalog.GetString(currentJumpType.Name);
	
		extra_window_jumps_initialize(currentJumpType);
	}
	
	private void on_extra_window_jumps_more(object o, EventArgs args)
	{
		previousJumpType = currentJumpType;

		jumpsMoreWin = JumpsMoreWindow.Show(app1, true);
		jumpsMoreWin.Button_accept.Clicked += new EventHandler(on_more_jumps_accepted);
		jumpsMoreWin.Button_cancel.Clicked += new EventHandler(on_more_jumps_cancelled);
		jumpsMoreWin.Button_selected.Clicked += new EventHandler(on_more_jumps_update_test);
	}
	
	private void on_extra_window_jumps_rj_test_changed(object o, EventArgs args)
	{
		string jumpEnglishName = comboSelectJumpsRj.GetSelectedNameEnglish();
		currentJumpRjType = createJumpType(jumpEnglishName, false);

		//change name of this radio but only if changed (to not have it flickering on hpaned1 displacement)
		if(radio_contacts_graph_currentTest.Label != Catalog.GetString(currentJumpRjType.Name))
			radio_contacts_graph_currentTest.Label = Catalog.GetString(currentJumpRjType.Name);

		extra_window_jumps_rj_initialize(currentJumpRjType);
	}

	private void on_extra_window_jumps_rj_more(object o, EventArgs args) 
	{
		previousJumpRjType = currentJumpRjType;

		jumpsRjMoreWin = JumpsRjMoreWindow.Show(app1, true);
		jumpsRjMoreWin.Button_accept.Clicked += new EventHandler(on_more_jumps_rj_accepted);
		jumpsRjMoreWin.Button_cancel.Clicked += new EventHandler(on_more_jumps_rj_cancelled);
		jumpsRjMoreWin.Button_selected.Clicked += new EventHandler(on_more_jumps_rj_update_test);
	}


	private void extra_window_jumps_initialize(JumpType myJumpType) 
	{
		currentEventType = myJumpType;
		changeTestImage(EventType.Types.JUMP.ToString(), myJumpType.Name, myJumpType.ImageFileName);
		setLabelContactsExerciseSelected(Constants.Modes.JUMPSSIMPLE);

		LastJumpSimpleTypeParams ljstp = new LastJumpSimpleTypeParams(myJumpType.Name);
		if(myJumpType.HasWeight || myJumpType.HasFall (configChronojump.Compujump))
			ljstp = SqliteJumpType.LastJumpSimpleTypeParamsSelect(myJumpType.Name); //search it on DB

		if(myJumpType.HasWeight)
			extra_window_showWeightData(myJumpType, true);	
		else
			extra_window_showWeightData(myJumpType, false);	

		if(myJumpType.HasFall (configChronojump.Compujump)) {
			extra_window_showFallData(myJumpType, true);
			if(ljstp.uniqueID != -1)
				extra_window_jumps_check_dj_fall_calculate.Active = (ljstp.fallmm == -1);
		} else
			extra_window_showFallData(myJumpType, false);	
		
		if(myJumpType.Name == "DJa" || myJumpType.Name == "DJna") { 
			//on DJa and DJna (coming from More jumps) need to show technique data but not change
			if(myJumpType.Name == "DJa")
				extra_window_jumps_check_dj_arms.Active = true;
			else //myJumpType.Name == "DJna"
				extra_window_jumps_check_dj_arms.Active = false;

			extra_window_showTechniqueArmsData(true, false); //visible, sensitive
		
			if(extra_window_jumps_check_dj_fall_calculate.Active) {
				if(extra_window_jumps_check_dj_arms.Active) 
					changeTestImage("","", "jump_dj_a_inside.png");
				else
					changeTestImage("","", "jump_dj_inside.png");
			}
		} else if(myJumpType.Name == "DJ") { 
			//user has pressed DJ button
			extra_window_jumps_check_dj_arms.Active = extra_window_jumps_arms;

			on_extra_window_jumps_check_dj_arms_clicked(new object(), new EventArgs());
			extra_window_showTechniqueArmsData(true, true); //visible, sensitive
		} else 
			extra_window_showTechniqueArmsData(false, false); //visible, sensitive

		button_jump_type_delete_simple.Sensitive = ! myJumpType.IsPredefined;

		if( (myJumpType.HasWeight || myJumpType.HasFall (configChronojump.Compujump)) && ljstp.uniqueID != -1) {
			extra_window_jumps_spinbutton_weight.Value = ljstp.weightValue;
			extra_window_jumps_spinbutton_fall.Value = ljstp.fallmm/10.0;
		} else {
			extra_window_jumps_spinbutton_weight.Value = 100;
			extra_window_jumps_spinbutton_fall.Value = extra_window_jumps_fall;
		}

		if(myJumpType.HasWeight && ljstp.uniqueID != -1)
		{
			if(ljstp.weightIsPercent)
				extra_window_jumps_radiobutton_weight.Active = true;
			else
				extra_window_jumps_radiobutton_kg.Active = true;
		} else {
			if (extra_window_jumps_option == "Kg")
				extra_window_jumps_radiobutton_kg.Active = true;
			else
				extra_window_jumps_radiobutton_weight.Active = true;
		}

		extra_window_showSingleLegStuff(myJumpType.Name == "slCMJleft" || myJumpType.Name == "slCMJright");
		if(myJumpType.Name == "slCMJleft" || myJumpType.Name == "slCMJright") {
			extra_window_jumps_spin_single_leg_distance.Value = 0;
			extra_window_jumps_spin_single_leg_angle.Value = 90;
		}

		if(! configChronojump.Exhibition)
			updateGraphJumpsSimple();

		setLabelContactsExerciseSelectedOptionsJumpsSimple();
	}
	private void updateGraphJumpsSimple () 
	{
		LogB.Information("Called updateGraphJumpsSimple");
		if(currentPerson == null || currentSession == null || currentJumpType == null) //currentJumpType needed if there are no jumpTypes
			return;

		double tc = 0.0;
		if(! currentJumpType.StartIn)
			tc = 1; //just a mark meaning that tc has to be shown

		double tv = 1;
		//special cases where there's no tv
		if(currentEventType.Name == Constants.TakeOffName || currentEventType.Name == Constants.TakeOffWeightName)
			tv = 0.0;
		
		//intializeVariables if not done before
		event_execute_initializeVariables(
			! cp2016.StoredCanCaptureContacts, //is simulated
			currentPerson.UniqueID, 
			currentPerson.Name, 
			Catalog.GetString("Phases"),  	  //name of the different moments
			Constants.JumpTable, //tableName
			currentJumpType.Name 
			);

		string typeTemp = currentEventType.Name;
		if(radio_contacts_graph_allTests.Active)
			typeTemp = "";

		int selectedID = -1;
		if (myTreeViewJumps != null && myTreeViewJumps.EventSelectedID > 0)
			selectedID = myTreeViewJumps.EventSelectedID;

		PrepareEventGraphJumpSimple eventGraph = new PrepareEventGraphJumpSimple(
				tv, tc, currentSession.UniqueID,
				currentPerson.UniqueID, radio_contacts_results_personAll.Active,
				-1 * Convert.ToInt32 (spin_contacts_graph_last_limit.Value), //negative: end limit
				Constants.JumpTable, typeTemp, preferences.heightPreferred, selectedID);
		
		//if(eventGraph.personMAXAtSQLAllSessions > 0 || eventGraph.jumpsAtSQL.Count > 0)
		//	PrepareJumpSimpleGraph(eventGraph, false); //don't animate

		string personStr = "";
		if(! radio_contacts_results_personAll.Active)
			personStr = currentPerson.Name;

		cairoPaintBarsPre = new CairoPaintBarsPreJumpSimple (
				event_execute_drawingarea_cairo, preferences.fontType.ToString(), current_mode,
				personStr, typeTemp, preferences.digitsNumber);

		cairoPaintBarsPre.StoreEventGraphJumps (eventGraph);

		//PrepareJumpSimpleGraph(cairoPaintBarsPre.eventGraphJumpsStored, false); //do not need, draw event will graph it:
		event_execute_drawingarea_cairo.QueueDraw ();
	}
	private void updateGraphJumpsReactive ()
	{
		LogB.Information("Called updateGraphJumpsReactive");
		if(currentPerson == null || currentSession == null)
			return;

		//we do not plot graph, but we want to update label event_graph_label_graph_test
		//intializeVariables if not done before
		event_execute_initializeVariables(
			! cp2016.StoredCanCaptureContacts, //is simulated
			currentPerson.UniqueID,
			currentPerson.Name,
			Catalog.GetString("Phases"),  	  //name of the different moments
			Constants.JumpRjTable, //tableName
			currentJumpRjType.Name
			);

		string typeTemp = currentEventType.Name;
		if(radio_contacts_graph_allTests.Active)
			typeTemp = "";

		int selectedID = -1;
		if (myTreeViewJumpsRj != null && myTreeViewJumpsRj.EventSelectedID > 0)
			selectedID = myTreeViewJumpsRj.EventSelectedID;

		PrepareEventGraphJumpReactive eventGraph = new PrepareEventGraphJumpReactive(
				currentSession.UniqueID, currentPerson.UniqueID,
				radio_contacts_results_personAll.Active,
				-1 * Convert.ToInt32 (spin_contacts_graph_last_limit.Value), //negative: end limit
				typeTemp, selectedID);

		string personStr = "";
		if(! radio_contacts_results_personAll.Active)
			personStr = currentPerson.Name;

		cairoPaintBarsPre = new CairoPaintBarsPreJumpReactive (
				event_execute_drawingarea_cairo, preferences.fontType.ToString(), current_mode,
				personStr, typeTemp, preferences.digitsNumber);

		cairoPaintBarsPre.StoreEventGraphJumpsRj (eventGraph);
		//PrepareJumpReactiveGraph (cairoPaintBarsPre.eventGraphJumpsRjStored, false); //do not need, draw event will graph it:
		event_execute_drawingarea_cairo.QueueDraw ();
	}
	
	private void extra_window_jumps_rj_initialize(JumpType myJumpType) 
	{
		currentEventType = myJumpType;
		changeTestImage(EventType.Types.JUMP.ToString(), myJumpType.Name, myJumpType.ImageFileName);
		setLabelContactsExerciseSelected(Constants.Modes.JUMPSREACTIVE);
		checkbutton_allow_finish_rj_after_time.Visible = false;
	
		LastJumpRjTypeParams ljrtp = SqliteJumpType.LastJumpRjTypeParamsSelect(myJumpType.Name); //search it on DB

		if(myJumpType.FixedValue >= 0) {
			string jumpsName = Catalog.GetString("jumps");
			string secondsName = Catalog.GetString("seconds");
			if(myJumpType.JumpsLimited) {
				extra_window_jumps_rj_jumpsLimited = true;
				extra_window_jumps_rj_label_limit_units.Text = jumpsName;
			} else {
				extra_window_jumps_rj_jumpsLimited = false;
				extra_window_jumps_rj_label_limit_units.Text = secondsName;
				checkbutton_allow_finish_rj_after_time.Visible = true;
			}
			if(myJumpType.FixedValue > 0) {
				extra_window_jumps_rj_spinbutton_limit.Sensitive = false;
				extra_window_jumps_rj_spinbutton_limit.Value = myJumpType.FixedValue;
			} else {
				extra_window_jumps_rj_spinbutton_limit.Sensitive = true;
				//extra_window_jumps_rj_spinbutton_limit.Value = extra_window_jumps_rj_limited;
				if(ljrtp.uniqueID != -1)
					extra_window_jumps_rj_spinbutton_limit.Value = ljrtp.limitedValue;
			}
			extra_window_showLimitData (true);
		} else  //unlimited
			extra_window_showLimitData (false);

		if(myJumpType.HasWeight) {
			extra_window_showWeightData(myJumpType, true);	
		} else 
			extra_window_showWeightData(myJumpType, false);	

		if(myJumpType.HasFall (configChronojump.Compujump) || myJumpType.Name == Constants.RunAnalysisName) {
			extra_window_showFallData(myJumpType, true);	
		} else
			extra_window_showFallData(myJumpType, false);

		button_jump_type_delete_reactive.Sensitive = ! myJumpType.IsPredefined;

		if( (myJumpType.HasWeight || myJumpType.HasFall (configChronojump.Compujump)) && ljrtp.uniqueID != -1) {
			extra_window_jumps_rj_spinbutton_weight.Value = ljrtp.weightValue;
			extra_window_jumps_rj_spinbutton_fall.Value = ljrtp.fallmm/10.0;
		} else {
			extra_window_jumps_rj_spinbutton_weight.Value = extra_window_jumps_rj_weight;
			extra_window_jumps_rj_spinbutton_fall.Value = extra_window_jumps_rj_fall;
		}

		if(ljrtp.uniqueID != -1)
		{
			LogB.Information("ljrtp.weightIsPercent: " + ljrtp.weightIsPercent.ToString());
			if(ljrtp.weightIsPercent)
				extra_window_jumps_rj_radiobutton_weight.Active = true;
			else
				extra_window_jumps_rj_radiobutton_kg.Active = true;
		}
		else {
			if (extra_window_jumps_rj_option == "Kg")
				extra_window_jumps_rj_radiobutton_kg.Active = true;
			else
				extra_window_jumps_rj_radiobutton_weight.Active = true;
		}

		if(! configChronojump.Exhibition)
		{
			//do not update graph, but erase it and change label event_graph_label_graph_test
			updateGraphJumpsReactive();
		}

		setLabelContactsExerciseSelectedOptionsJumpsReactive();
	}

	private void on_extra_window_jumps_check_dj_arms_clicked(object o, EventArgs args)
	{
		JumpType j = new JumpType();
		if(extra_window_jumps_check_dj_arms.Active) 
			j = new JumpType("DJa");
		else
			j = new JumpType("DJna");

		changeTestImage(EventType.Types.JUMP.ToString(), j.Name, j.ImageFileName);
	}

	private void on_extra_window_checkbutton_allow_finish_rj_after_time_toggled(object o, EventArgs args)
	{
		SqlitePreferences.Update("allowFinishRjAfterTime", checkbutton_allow_finish_rj_after_time.Active.ToString(), false);
	}

	private void on_more_jumps_update_test (object o, EventArgs args)
	{
		currentEventType = new JumpType(jumpsMoreWin.SelectedEventName);
		comboSelectJumps.MakeActive(jumpsMoreWin.SelectedEventName);
	}
	
	private void on_more_jumps_rj_update_test (object o, EventArgs args) 
	{
		currentEventType = new JumpType(jumpsRjMoreWin.SelectedEventName);
		comboSelectJumpsRj.MakeActive(jumpsRjMoreWin.SelectedEventName);
	}
	
	//used from the dialogue "jumps more"
	private void on_more_jumps_accepted (object o, EventArgs args) 
	{
		jumpsMoreWin.Button_accept.Clicked -= new EventHandler(on_more_jumps_accepted);
		
		currentJumpType = createJumpType(jumpsMoreWin.SelectedEventName, true);

		extra_window_jumps_initialize(currentJumpType);
		
		//destroy the win for not having updating problems if a new jump type is created
		//jumpsMoreWin = null; //don't work
		jumpsMoreWin.Destroy(); //works ;)
	}
	
	//used from the dialogue "jumps rj more"
	private void on_more_jumps_rj_accepted (object o, EventArgs args) 
	{
		jumpsRjMoreWin.Button_accept.Clicked -= new EventHandler(on_more_jumps_rj_accepted);

		currentJumpRjType = createJumpType(jumpsRjMoreWin.SelectedEventName, false);
		
		extra_window_jumps_rj_initialize(currentJumpRjType);
	
		//destroy the win for not having updating problems if a new jump type is created
		jumpsRjMoreWin.Destroy();
	}

	//if it's cancelled (or deleted event) select desired toolbar button
	private void on_more_jumps_cancelled (object o, EventArgs args) 
	{
		currentJumpType = previousJumpType;
		combo_select_jumps.Active = UtilGtk.ComboMakeActive(combo_select_jumps, currentJumpType.Name);
	}
	
	private void on_more_jumps_rj_cancelled (object o, EventArgs args) 
	{
		currentJumpRjType = previousJumpRjType;
		combo_select_jumps_rj.Active = UtilGtk.ComboMakeActive(combo_select_jumps_rj, currentJumpRjType.Name);
	}
	
	private void extra_window_showWeightData (JumpType myJumpType, bool show) {
		if(myJumpType.IsRepetitive) {
			grid_extra_window_jumps_rj_weight.Visible = show;
			update_label_extra_window_jumps_rj_radiobutton_weight_percent_as_kg(show);
		} else {
			grid_extra_window_jumps_weight.Visible = show;
			update_label_extra_window_jumps_radiobutton_weight_percent_as_kg(show);
		}
	}
	
	private void extra_window_showTechniqueArmsData (bool show, bool sensitive) {
		extra_window_jumps_check_dj_arms.Visible = show;
		extra_window_jumps_check_dj_arms.Sensitive = sensitive;
	}
	
	private void extra_window_showFallData (JumpType myJumpType, bool show)
	{
		if(myJumpType.IsRepetitive)
		{
			hbox_extra_window_jumps_rj_hbox_fall.Visible = show;
			extra_window_jumps_label_rj_start_inside.Visible = ! show;
			extra_window_jumps_label_rj_start_outside.Visible = show;
		
			//only on simple jumps	
			extra_window_jumps_hbox_fall.Visible = false;
		} else {
			extra_window_jumps_simple_hbox_start_inside.Visible = ! show;
			extra_window_jumps_hbox_fall.Visible = show;
		}
	}
	
	private void extra_window_showLimitData (bool show) {
		extra_window_jumps_rj_label_limit.Visible = show;
		extra_window_jumps_rj_spinbutton_limit.Visible = show;
		extra_window_jumps_rj_label_limit_units.Visible = show;
	}
	
	private void extra_window_showSingleLegStuff(bool show) {
		grid_extra_window_jumps_single_leg_radios.Visible = show;
	}
			
	private void on_extra_window_jumps_radiobutton_kg_toggled (object o, EventArgs args)
	{
		extra_window_jumps_option = "Kg";
		label_extra_window_jumps_radiobutton_weight_percent_as_kg.Visible = false;
		setLabelContactsExerciseSelectedOptionsJumpsSimple();
	}
	
	private void on_extra_window_jumps_radiobutton_weight_toggled (object o, EventArgs args)
	{
		extra_window_jumps_option = "%";

		update_label_extra_window_jumps_radiobutton_weight_percent_as_kg(true);
		label_extra_window_jumps_radiobutton_weight_percent_as_kg.Visible = true;

		setLabelContactsExerciseSelectedOptionsJumpsSimple();
	}

	private void on_extra_window_jumps_rj_radiobutton_kg_toggled (object o, EventArgs args)
	{
		extra_window_jumps_rj_option = "Kg";
		label_extra_window_jumps_rj_radiobutton_weight_percent_as_kg.Visible = false;
		setLabelContactsExerciseSelectedOptionsJumpsReactive();
	}
	
	private void on_extra_window_jumps_rj_radiobutton_weight_toggled (object o, EventArgs args)
	{
		extra_window_jumps_rj_option = "%";

		update_label_extra_window_jumps_rj_radiobutton_weight_percent_as_kg(true);
		label_extra_window_jumps_rj_radiobutton_weight_percent_as_kg.Visible = true;
	}

	private void on_button_jumps_params_accelerators_clicked (object o, EventArgs args)
	{
		//jumps simple, weight
		if (o == (object) button_jumps_extra_weight_minus_10)
			button_jumps_params_accelerators_do
				(extra_window_jumps_spinbutton_weight, -10);
		else if (o == (object) button_jumps_extra_weight_minus_1)
			button_jumps_params_accelerators_do
				(extra_window_jumps_spinbutton_weight, -1);
		else if (o == (object) button_jumps_extra_weight_plus_1)
			button_jumps_params_accelerators_do
				(extra_window_jumps_spinbutton_weight, 1);
		else if (o == (object) button_jumps_extra_weight_plus_10)
			button_jumps_params_accelerators_do
				(extra_window_jumps_spinbutton_weight, 10);
		//jumps rj weight
		else if (o == (object) button_jumps_rj_extra_weight_minus_10)
			button_jumps_params_accelerators_do
				(extra_window_jumps_rj_spinbutton_weight, -10);
		else if (o == (object) button_jumps_rj_extra_weight_minus_1)
			button_jumps_params_accelerators_do
				(extra_window_jumps_rj_spinbutton_weight, -1);
		else if (o == (object) button_jumps_rj_extra_weight_plus_1)
			button_jumps_params_accelerators_do
				(extra_window_jumps_rj_spinbutton_weight, 1);
		else if (o == (object) button_jumps_rj_extra_weight_plus_10)
			button_jumps_params_accelerators_do
				(extra_window_jumps_rj_spinbutton_weight, 10);
		//jumps simple fall
		else if (o == (object) button_jumps_extra_fall_minus_10)
			button_jumps_params_accelerators_do
				(extra_window_jumps_spinbutton_fall, -10);
		else if (o == (object) button_jumps_extra_fall_minus_1)
			button_jumps_params_accelerators_do
				(extra_window_jumps_spinbutton_fall, -1);
		else if (o == (object) button_jumps_extra_fall_plus_1)
			button_jumps_params_accelerators_do
				(extra_window_jumps_spinbutton_fall, 1);
		else if (o == (object) button_jumps_extra_fall_plus_10)
			button_jumps_params_accelerators_do
				(extra_window_jumps_spinbutton_fall, 10);
		//jumps rj fall
		else if (o == (object) button_jumps_rj_extra_fall_minus_10)
			button_jumps_params_accelerators_do
				(extra_window_jumps_rj_spinbutton_fall, -10);
		else if (o == (object) button_jumps_rj_extra_fall_minus_1)
			button_jumps_params_accelerators_do
				(extra_window_jumps_rj_spinbutton_fall, -1);
		else if (o == (object) button_jumps_rj_extra_fall_plus_1)
			button_jumps_params_accelerators_do
				(extra_window_jumps_rj_spinbutton_fall, 1);
		else if (o == (object) button_jumps_rj_extra_fall_plus_10)
			button_jumps_params_accelerators_do
				(extra_window_jumps_rj_spinbutton_fall, 10);
	}

	private void button_jumps_params_accelerators_do (Gtk.SpinButton spin, int change)
	{
		double newValue = spin.Value + change;

		double min, max;
		spin.GetRange (out min, out max);
		if(newValue < min)
			spin.Value = min;
		else if(newValue > max)
			spin.Value = max;
		else
			spin.Value = newValue;
	}

	private void on_extra_window_jumps_spinbutton_weight_value_changed (object o, EventArgs args)
	{
		update_label_extra_window_jumps_radiobutton_weight_percent_as_kg(true);
		setLabelContactsExerciseSelectedOptionsJumpsSimple();
	}
	private void update_label_extra_window_jumps_radiobutton_weight_percent_as_kg(bool show)
	{
		if(! show || currentPersonSession == null || currentPersonSession.Weight == 0)
		{
			label_extra_window_jumps_radiobutton_weight_percent_as_kg.Text = "";
			return;
		}

		label_extra_window_jumps_radiobutton_weight_percent_as_kg.Text =
			Util.TrimDecimals(Util.WeightFromPercentToKg (
						(double) extra_window_jumps_spinbutton_weight.Value,
						currentPersonSession.Weight), 1) + " Kg";
	}

	private void on_extra_window_jumps_rj_spinbutton_weight_value_changed (object o, EventArgs args)
	{
		update_label_extra_window_jumps_rj_radiobutton_weight_percent_as_kg(true);
		setLabelContactsExerciseSelectedOptionsJumpsReactive();
	}
	private void update_label_extra_window_jumps_rj_radiobutton_weight_percent_as_kg(bool show)
	{
		if(! show || currentPersonSession == null || currentPersonSession.Weight == 0)
		{
			label_extra_window_jumps_rj_radiobutton_weight_percent_as_kg.Text = "";
			return;
		}

		label_extra_window_jumps_rj_radiobutton_weight_percent_as_kg.Text =
			Util.TrimDecimals(Util.WeightFromPercentToKg (
						(double) extra_window_jumps_rj_spinbutton_weight.Value,
						currentPersonSession.Weight), 1) + " Kg";
	}


	private string limitString()
	{
		if(extra_window_jumps_rj_jumpsLimited) 
			return extra_window_jumps_rj_limited.ToString() + "J";
		else 
			return extra_window_jumps_rj_limited.ToString() + "T";
	}

	//do not translate this
	private string slCMJString()
	{
		string str = "";
		if(extra_window_jumps_radiobutton_single_leg_mode_vertical.Active) str = "Vertical";
		else if(extra_window_jumps_radiobutton_single_leg_mode_horizontal.Active) str = "Horizontal";
		else str = "Lateral";
		
		if(extra_window_jumps_radiobutton_single_leg_dominance_this_limb.Active) str += " This";
		else if(extra_window_jumps_radiobutton_single_leg_dominance_opposite.Active) str += " Opposite";
		else str += " Unknown"; //default since 1.4.8

		if(extra_window_jumps_radiobutton_single_leg_fall_this_limb.Active) str += " This";
		else if(extra_window_jumps_radiobutton_single_leg_fall_opposite.Active) str += " Opposite";
		else str += " Both"; //default since 1.4.8

		return str;
	}

	private void on_spin_single_leg_changed(object o, EventArgs args) {
		int distance = Convert.ToInt32(extra_window_jumps_spin_single_leg_distance.Value);
		extra_window_jumps_spin_single_leg_angle.Value =
			Util.CalculateJumpAngle(
					Convert.ToDouble(Util.GetHeightInCentimeters(currentJump.Tv.ToString())), 
					distance );
	}

	private void on_extra_window_jumps_button_single_leg_apply_clicked (object o, EventArgs args) {
		string description = slCMJString();
		int distance = Convert.ToInt32(extra_window_jumps_spin_single_leg_distance.Value);
		int angle = Convert.ToInt32(extra_window_jumps_spin_single_leg_angle.Value);
		currentJump.Description = description + 
			" " + distance.ToString() +
			" " + angle.ToString();
		
		SqliteJump.UpdateDescription(Constants.JumpTable, 
			currentJump.UniqueID, currentJump.Description);
		
		myTreeViewJumps.Update(currentJump);
		
		//sensitive slCMJ options 
		grid_extra_window_jumps_single_leg_radios.Sensitive = true;

		//hide slCMJ distance stuff and show button execute test again
		notebook_contacts_capture_doing_wait.CurrentPage = 0;
	}

	// ---- save jumps simple image start ---->

	private void on_button_jumps_capture_save_image_selected (string destination)
	{
		if(event_execute_drawingarea_cairo == null)
			return;

		LogB.Information("Saving");
		CairoUtil.GetScreenshotFromDrawingArea (event_execute_drawingarea_cairo, destination);
	}
	private void on_overwrite_file_jumps_capture_save_image_accepted (object o, EventArgs args)
	{
		on_button_jumps_capture_save_image_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), exportFileName);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}

	// <---- save jumps simple image end ----

	private void connectWidgetsJump (Gtk.Builder builder)
	{
		//options jumps
		button_combo_jumps_exercise_capture_left = (Gtk.Button) builder.GetObject ("button_combo_jumps_exercise_capture_left");
		button_combo_jumps_exercise_capture_right = (Gtk.Button) builder.GetObject ("button_combo_jumps_exercise_capture_right");
		button_jump_type_delete_simple = (Gtk.Button) builder.GetObject ("button_jump_type_delete_simple");
		extra_window_jumps_spinbutton_weight = (Gtk.SpinButton) builder.GetObject ("extra_window_jumps_spinbutton_weight");
		extra_window_jumps_simple_hbox_start_inside = (Gtk.HBox) builder.GetObject ("extra_window_jumps_simple_hbox_start_inside");
		extra_window_jumps_hbox_fall = (Gtk.Box) builder.GetObject ("extra_window_jumps_hbox_fall");
		extra_window_jumps_check_dj_fall_calculate = (Gtk.CheckButton) builder.GetObject ("extra_window_jumps_check_dj_fall_calculate");
		extra_window_jumps_label_dj_start_inside = (Gtk.Label) builder.GetObject ("extra_window_jumps_label_dj_start_inside");
		extra_window_jumps_label_dj_start_outside = (Gtk.Label) builder.GetObject ("extra_window_jumps_label_dj_start_outside");
		extra_window_jumps_image_fall = (Gtk.Image) builder.GetObject ("extra_window_jumps_image_fall");
		extra_window_jumps_image_weight = (Gtk.Image) builder.GetObject ("extra_window_jumps_image_weight");
		extra_window_jumps_spinbutton_fall = (Gtk.SpinButton) builder.GetObject ("extra_window_jumps_spinbutton_fall");
		grid_extra_window_jumps_weight = (Gtk.Grid) builder.GetObject ("grid_extra_window_jumps_weight");
		extra_window_jumps_radiobutton_kg = (Gtk.RadioButton) builder.GetObject ("extra_window_jumps_radiobutton_kg");
		extra_window_jumps_radiobutton_weight = (Gtk.RadioButton) builder.GetObject ("extra_window_jumps_radiobutton_weight");
		//extra_window_jumps_label_weight = (Gtk.Label) builder.GetObject ("extra_window_jumps_label_weight");
		extra_window_jumps_check_dj_arms = (Gtk.CheckButton) builder.GetObject ("extra_window_jumps_check_dj_arms");

		//show weight on kg when percent is selected (SJl, CMJl, ABKl)
		label_extra_window_jumps_radiobutton_weight_percent_as_kg = (Gtk.Label) builder.GetObject ("label_extra_window_jumps_radiobutton_weight_percent_as_kg");
		label_extra_window_jumps_rj_radiobutton_weight_percent_as_kg = (Gtk.Label) builder.GetObject ("label_extra_window_jumps_rj_radiobutton_weight_percent_as_kg");

		//slCMJ	
		grid_extra_window_jumps_single_leg_radios = (Gtk.Grid) builder.GetObject ("grid_extra_window_jumps_single_leg_radios");
		extra_window_jumps_radiobutton_single_leg_mode_vertical = (Gtk.RadioButton) builder.GetObject ("extra_window_jumps_radiobutton_single_leg_mode_vertical");
		extra_window_jumps_radiobutton_single_leg_mode_horizontal = (Gtk.RadioButton) builder.GetObject ("extra_window_jumps_radiobutton_single_leg_mode_horizontal");
		extra_window_jumps_radiobutton_single_leg_dominance_this_limb = (Gtk.RadioButton) builder.GetObject ("extra_window_jumps_radiobutton_single_leg_dominance_this_limb");
		extra_window_jumps_radiobutton_single_leg_dominance_opposite = (Gtk.RadioButton) builder.GetObject ("extra_window_jumps_radiobutton_single_leg_dominance_opposite");
		extra_window_jumps_radiobutton_single_leg_fall_this_limb = (Gtk.RadioButton) builder.GetObject ("extra_window_jumps_radiobutton_single_leg_fall_this_limb");
		extra_window_jumps_radiobutton_single_leg_fall_opposite = (Gtk.RadioButton) builder.GetObject ("extra_window_jumps_radiobutton_single_leg_fall_opposite");
		extra_window_jumps_spin_single_leg_distance = (Gtk.SpinButton) builder.GetObject ("extra_window_jumps_spin_single_leg_distance");
		extra_window_jumps_spin_single_leg_angle = (Gtk.SpinButton) builder.GetObject ("extra_window_jumps_spin_single_leg_angle");

		//options jumps_rj
		button_combo_jumps_rj_exercise_capture_left = (Gtk.Button) builder.GetObject ("button_combo_jumps_rj_exercise_capture_left");
		button_combo_jumps_rj_exercise_capture_right = (Gtk.Button) builder.GetObject ("button_combo_jumps_rj_exercise_capture_right");
		button_jump_type_delete_reactive = (Gtk.Button) builder.GetObject ("button_jump_type_delete_reactive");
		extra_window_jumps_rj_label_limit = (Gtk.Label) builder.GetObject ("extra_window_jumps_rj_label_limit");
		extra_window_jumps_rj_spinbutton_limit = (Gtk.SpinButton) builder.GetObject ("extra_window_jumps_rj_spinbutton_limit");
		extra_window_jumps_rj_label_limit_units = (Gtk.Label) builder.GetObject ("extra_window_jumps_rj_label_limit_units");
		extra_window_jumps_rj_spinbutton_weight = (Gtk.SpinButton) builder.GetObject ("extra_window_jumps_rj_spinbutton_weight");
		extra_window_jumps_rj_spinbutton_fall = (Gtk.SpinButton) builder.GetObject ("extra_window_jumps_rj_spinbutton_fall");
		extra_window_jumps_rj_radiobutton_kg = (Gtk.RadioButton) builder.GetObject ("extra_window_jumps_rj_radiobutton_kg");
		extra_window_jumps_rj_radiobutton_weight = (Gtk.RadioButton) builder.GetObject ("extra_window_jumps_rj_radiobutton_weight");
		grid_extra_window_jumps_rj_weight = (Gtk.Grid) builder.GetObject ("grid_extra_window_jumps_rj_weight");
		hbox_extra_window_jumps_rj_hbox_fall = (Gtk.HBox) builder.GetObject ("hbox_extra_window_jumps_rj_hbox_fall");
		extra_window_jumps_rj_image_fall = (Gtk.Image) builder.GetObject ("extra_window_jumps_rj_image_fall");
		extra_window_jumps_rj_image_weight = (Gtk.Image) builder.GetObject ("extra_window_jumps_rj_image_weight");
		extra_window_jumps_label_rj_start_inside = (Gtk.Label) builder.GetObject ("extra_window_jumps_label_rj_start_inside");
		extra_window_jumps_label_rj_start_outside = (Gtk.Label) builder.GetObject ("extra_window_jumps_label_rj_start_outside");
		checkbutton_allow_finish_rj_after_time = (Gtk.CheckButton) builder.GetObject ("checkbutton_allow_finish_rj_after_time");

		button_jumps_extra_weight_minus_10 = (Gtk.Button) builder.GetObject ("button_jumps_extra_weight_minus_10");
		button_jumps_extra_weight_minus_1 = (Gtk.Button) builder.GetObject ("button_jumps_extra_weight_minus_1");
		button_jumps_extra_weight_plus_1 = (Gtk.Button) builder.GetObject ("button_jumps_extra_weight_plus_1");
		button_jumps_extra_weight_plus_10 = (Gtk.Button) builder.GetObject ("button_jumps_extra_weight_plus_10");
		button_jumps_rj_extra_weight_minus_10 = (Gtk.Button) builder.GetObject ("button_jumps_rj_extra_weight_minus_10");
		button_jumps_rj_extra_weight_minus_1 = (Gtk.Button) builder.GetObject ("button_jumps_rj_extra_weight_minus_1");
		button_jumps_rj_extra_weight_plus_1 = (Gtk.Button) builder.GetObject ("button_jumps_rj_extra_weight_plus_1");
		button_jumps_rj_extra_weight_plus_10 = (Gtk.Button) builder.GetObject ("button_jumps_rj_extra_weight_plus_10");
		button_jumps_extra_fall_minus_10 = (Gtk.Button) builder.GetObject ("button_jumps_extra_fall_minus_10");
		button_jumps_extra_fall_minus_1 = (Gtk.Button) builder.GetObject ("button_jumps_extra_fall_minus_1");
		button_jumps_extra_fall_plus_1 = (Gtk.Button) builder.GetObject ("button_jumps_extra_fall_plus_1");
		button_jumps_extra_fall_plus_10 = (Gtk.Button) builder.GetObject ("button_jumps_extra_fall_plus_10");
		button_jumps_rj_extra_fall_minus_10 = (Gtk.Button) builder.GetObject ("button_jumps_rj_extra_fall_minus_10");
		button_jumps_rj_extra_fall_minus_1 = (Gtk.Button) builder.GetObject ("button_jumps_rj_extra_fall_minus_1");
		button_jumps_rj_extra_fall_plus_1 = (Gtk.Button) builder.GetObject ("button_jumps_rj_extra_fall_plus_1");
		button_jumps_rj_extra_fall_plus_10 = (Gtk.Button) builder.GetObject ("button_jumps_rj_extra_fall_plus_10");

		box_contacts_export_data_jumps = (Gtk.Box) builder.GetObject ("box_contacts_export_data_jumps");
		check_contacts_export_jumps_simple = (Gtk.CheckButton) builder.GetObject ("check_contacts_export_jumps_simple");
		check_contacts_export_jumps_simple_mean_max_tables = (Gtk.CheckButton) builder.GetObject ("check_contacts_export_jumps_simple_mean_max_tables");
		check_contacts_export_jumps_reactive = (Gtk.CheckButton) builder.GetObject ("check_contacts_export_jumps_reactive");
	}
}
