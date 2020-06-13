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
 * Copyright (C) 2004-2020   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Glade;
using System.Text; //StringBuilder
using Mono.Unix;


//--------------------------------------------------------
//---------------- jump extra WIDGET --------------------
//---------------- in 0.9.3 included in main gui ---------
//--------------------------------------------------------

public partial class ChronoJumpWindow
{
	//options jumps
	[Widget] Gtk.Button button_jump_type_delete_simple;
	[Widget] Gtk.SpinButton extra_window_jumps_spinbutton_weight;
	[Widget] Gtk.Box extra_window_jumps_hbox_fall;
	[Widget] Gtk.CheckButton extra_window_jumps_check_dj_fall_calculate;
	[Widget] Gtk.Label extra_window_jumps_label_dj_start_inside;
	[Widget] Gtk.Label extra_window_jumps_label_dj_start_outside;
	[Widget] Gtk.SpinButton extra_window_jumps_spinbutton_fall;
	[Widget] Gtk.HBox hbox_extra_window_jumps_weight;
	[Widget] Gtk.RadioButton extra_window_jumps_radiobutton_kg;
	[Widget] Gtk.RadioButton extra_window_jumps_radiobutton_weight;
	[Widget] Gtk.Label extra_window_jumps_label_weight;
	[Widget] Gtk.CheckButton extra_window_jumps_check_dj_arms;

	//show weight on kg when percent is selected (SJl, CMJl, ABKl)
	[Widget] Gtk.Label label_extra_window_jumps_radiobutton_weight_percent_as_kg;
	[Widget] Gtk.Label label_extra_window_jumps_rj_radiobutton_weight_percent_as_kg;

	//slCMJ	
	[Widget] Gtk.Table table_extra_window_jumps_single_leg_radios;
	[Widget] Gtk.RadioButton extra_window_jumps_radiobutton_single_leg_mode_vertical;
	[Widget] Gtk.RadioButton extra_window_jumps_radiobutton_single_leg_mode_horizontal;
	[Widget] Gtk.RadioButton extra_window_jumps_radiobutton_single_leg_dominance_this_limb;
	[Widget] Gtk.RadioButton extra_window_jumps_radiobutton_single_leg_dominance_opposite;
	[Widget] Gtk.RadioButton extra_window_jumps_radiobutton_single_leg_fall_this_limb;
	[Widget] Gtk.RadioButton extra_window_jumps_radiobutton_single_leg_fall_opposite;
	[Widget] Gtk.SpinButton extra_window_jumps_spin_single_leg_distance;
	[Widget] Gtk.SpinButton extra_window_jumps_spin_single_leg_angle;
	
	//options jumps_rj
	[Widget] Gtk.Button button_jump_type_delete_reactive;
	[Widget] Gtk.Label extra_window_jumps_rj_label_limit;
	[Widget] Gtk.SpinButton extra_window_jumps_rj_spinbutton_limit;
	[Widget] Gtk.Label extra_window_jumps_rj_label_limit_units;
	[Widget] Gtk.SpinButton extra_window_jumps_rj_spinbutton_weight;
	[Widget] Gtk.SpinButton extra_window_jumps_rj_spinbutton_fall;
	[Widget] Gtk.RadioButton extra_window_jumps_rj_radiobutton_kg;
	[Widget] Gtk.RadioButton extra_window_jumps_rj_radiobutton_weight;
	[Widget] Gtk.HBox hbox_extra_window_jumps_rj_weight;
	[Widget] Gtk.Label extra_window_jumps_rj_label_weight;
	[Widget] Gtk.Label extra_window_jumps_rj_label_fall;
	[Widget] Gtk.Label extra_window_jumps_rj_label_cm;
	[Widget] Gtk.CheckButton checkbutton_allow_finish_rj_after_time;

	
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
	//simple == true  for normal jumps, and false for reactive
	private JumpType createJumpType(string name, bool simple) {
		JumpType t = new JumpType(name);
		
		if(! t.IsPredefined) {
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
		currentJumpType = createJumpType(jumpEnglishName, true);
	
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
		setLabelContactsExerciseSelected(Constants.Menuitem_modes.JUMPSSIMPLE);


		if(myJumpType.HasWeight) {
			extra_window_showWeightData(myJumpType, true);	
		} else 
			extra_window_showWeightData(myJumpType, false);	

		if(myJumpType.HasFall) {
			extra_window_showFallData(myJumpType, true);
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

		extra_window_jumps_spinbutton_weight.Value = 100;
		extra_window_jumps_spinbutton_fall.Value = extra_window_jumps_fall;
		if (extra_window_jumps_option == "Kg") {
			extra_window_jumps_radiobutton_kg.Active = true;
		} else {
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
		if(currentPerson == null || currentSession == null)
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

		PrepareEventGraphJumpSimple eventGraph = new PrepareEventGraphJumpSimple(
				tv, tc, currentSession.UniqueID,
				currentPerson.UniqueID, radio_contacts_graph_allPersons.Active, Convert.ToInt32(spin_contacts_graph_last_limit.Value),
				Constants.JumpTable, currentEventType.Name, preferences.jumpsDjGraphHeights);
		
		if(eventGraph.personMAXAtSQLAllSessions > 0 || eventGraph.jumpsAtSQL.Count > 0)
			PrepareJumpSimpleGraph(eventGraph, false); //don't animate
	}
	private void updateGraphJumpsReactive ()
	{
		if(currentPerson == null || currentSession == null)
			return;

		if(event_execute_drawingarea == null || event_execute_pixmap == null)
			return;

		UtilGtk.ErasePaint(event_execute_drawingarea, event_execute_pixmap);
		event_execute_table_jump_reactive_values.Hide();

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
		//TODO paint graph of num of reactive jumps
	}
	
	private void extra_window_jumps_rj_initialize(JumpType myJumpType) 
	{
		currentEventType = myJumpType;
		changeTestImage(EventType.Types.JUMP.ToString(), myJumpType.Name, myJumpType.ImageFileName);
		setLabelContactsExerciseSelected(Constants.Menuitem_modes.JUMPSREACTIVE);
		checkbutton_allow_finish_rj_after_time.Visible = false;
	
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
				extra_window_jumps_rj_spinbutton_limit.Value = extra_window_jumps_rj_limited;
			}
			extra_window_showLimitData (true);
		} else  //unlimited
			extra_window_showLimitData (false);

		if(myJumpType.HasWeight) {
			extra_window_showWeightData(myJumpType, true);	
		} else 
			extra_window_showWeightData(myJumpType, false);	

		if(myJumpType.HasFall || myJumpType.Name == Constants.RunAnalysisName) {
			extra_window_showFallData(myJumpType, true);	
		} else
			extra_window_showFallData(myJumpType, false);

		button_jump_type_delete_reactive.Sensitive = ! myJumpType.IsPredefined;

		extra_window_jumps_rj_spinbutton_weight.Value = extra_window_jumps_rj_weight;
		extra_window_jumps_rj_spinbutton_fall.Value = extra_window_jumps_rj_fall;
		if (extra_window_jumps_rj_option == "Kg") {
			extra_window_jumps_rj_radiobutton_kg.Active = true;
		} else {
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
		extra_window_jumps_initialize(currentJumpType);
	}
	
	private void on_more_jumps_rj_cancelled (object o, EventArgs args) 
	{
		currentJumpRjType = previousJumpRjType;
		extra_window_jumps_rj_initialize(currentJumpRjType);
	}
	
	private void extra_window_showWeightData (JumpType myJumpType, bool show) {
		if(myJumpType.IsRepetitive) {
			hbox_extra_window_jumps_rj_weight.Visible = show;
			update_label_extra_window_jumps_rj_radiobutton_weight_percent_as_kg(show);
		} else {
			hbox_extra_window_jumps_weight.Visible = show;
			update_label_extra_window_jumps_radiobutton_weight_percent_as_kg(show);
		}
	}
	
	private void extra_window_showTechniqueArmsData (bool show, bool sensitive) {
		extra_window_jumps_check_dj_arms.Visible = show;
		extra_window_jumps_check_dj_arms.Sensitive = sensitive;
	}
	
	private void extra_window_showFallData (JumpType myJumpType, bool show) {
		if(myJumpType.IsRepetitive) {
			extra_window_jumps_rj_label_fall.Visible = show;
			extra_window_jumps_rj_spinbutton_fall.Visible = show;
			extra_window_jumps_rj_label_cm.Visible = show;
		
			//only on simple jumps	
			extra_window_jumps_hbox_fall.Visible = false;
		} else 
			extra_window_jumps_hbox_fall.Visible = show;
	}
	
	private void extra_window_showLimitData (bool show) {
		extra_window_jumps_rj_label_limit.Visible = show;
		extra_window_jumps_rj_spinbutton_limit.Visible = show;
		extra_window_jumps_rj_label_limit_units.Visible = show;
	}
	
	private void extra_window_showSingleLegStuff(bool show) {
		table_extra_window_jumps_single_leg_radios.Visible = show;
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
		table_extra_window_jumps_single_leg_radios.Sensitive = true;

		//hide slCMJ distance stuff and show button execute test again
		notebook_contacts_capture_doing_wait.CurrentPage = 0;
	}

}


