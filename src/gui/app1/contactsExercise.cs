/*
 * This file is part of ChronoJump
 *
 * Chronojump is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or   
 *    (at your option) any later version.
 *    
 * Chronojump is distributed in the hope that it will be useful,
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
using Gdk;
using Mono.Unix;

public partial class ChronoJumpWindow 
{
	[Widget] Gtk.Frame frame_contacts_exercise;
	[Widget] Gtk.Button button_combo_select_contacts_top_left;
	[Widget] Gtk.Button button_combo_select_contacts_top_right;
	[Widget] Gtk.Button button_contacts_exercise;
	[Widget] Gtk.Label label_contacts_exercise_selected_name;
	[Widget] Gtk.Label label_contacts_exercise_selected_options;

	private void on_button_contacts_exercise_clicked (object o, EventArgs args)
	{
		menus_and_mode_sensitive(false);
		notebook_contacts_capture_doing_wait.Sensitive = false;
		hbox_contacts_device_adjust_threshold.Sensitive = false;
		frame_persons.Sensitive = false;
		combo_select_contacts_top.Sensitive = false;
		frame_contacts_exercise.Sensitive = false;
		hbox_contacts_sup_capture_analyze_two_buttons.Sensitive = false;
		hbox_top_person.Sensitive = false;

		button_contacts_exercise_close_and_capture.Sensitive = myTreeViewPersons.IsThereAnyRecord();
		notebook_contacts_execute_or.CurrentPage = Convert.ToInt32(notebook_contacts_execute_or_pages.INSTRUCTIONS);
	}
	private void on_button_contacts_exercise_close_clicked (object o, EventArgs args)
	{
		menus_and_mode_sensitive(true);
		notebook_contacts_capture_doing_wait.Sensitive = true;
		hbox_contacts_device_adjust_threshold.Sensitive = true;
		frame_persons.Sensitive = true;
		combo_select_contacts_top.Sensitive = true;
		frame_contacts_exercise.Sensitive = true;
		hbox_contacts_sup_capture_analyze_two_buttons.Sensitive = true;
		hbox_top_person.Sensitive = true;

		notebook_contacts_execute_or.CurrentPage = Convert.ToInt32(notebook_contacts_execute_or_pages.EXECUTE);
	}
	private void on_button_contacts_exercise_close_and_capture_clicked (object o, EventArgs args)
	{
		on_button_contacts_exercise_close_clicked (o, args);
		on_button_execute_test_clicked(o, args);
	}
	private void on_button_contacts_exercise_close_and_recalculate_clicked (object o, EventArgs args)
	{
		on_button_contacts_exercise_close_clicked (o, args);
		on_button_contacts_recalculate_clicked(o, args);
	}


	private void on_button_image_test_zoom_clicked(object o, EventArgs args)
	{
		EventType myType;
		if(current_mode == Constants.Modes.JUMPSSIMPLE)
			myType = currentJumpType;
		else if(current_mode == Constants.Modes.JUMPSREACTIVE)
			myType = currentJumpRjType;
		else if(current_mode == Constants.Modes.RUNSSIMPLE)
			myType = currentRunType;
		else if(current_mode == Constants.Modes.RUNSINTERVALLIC)
			myType = currentRunIntervalType;
		//else if(current_mode == Constants.Modes.RUNSENCODER)
		//	myType = currentRunIntervalType;
		//else if(current_mode == Constants.Modes.FORCESENSOR
		//	myType = currentForceType;
		else if(current_mode == Constants.Modes.RT)
			myType = currentReactionTimeType;
		else //if(current_mode == Constants.Modes.OTHER
		{
			if(radio_mode_multi_chronopic_small.Active)
				myType = currentMultiChronopicType;
			else //if(radio_mode_pulses_small.Active)
				myType = currentPulseType;
		}
			
		if(myType.Name == "DJa" && extra_window_jumps_check_dj_fall_calculate.Active)
			new DialogImageTest("", Util.GetImagePath(false) + "jump_dj_a_inside.png", DialogImageTest.ArchiveType.ASSEMBLY);
		else if(myType.Name == "DJna" && extra_window_jumps_check_dj_fall_calculate.Active)
			new DialogImageTest("", Util.GetImagePath(false) + "jump_dj_inside.png", DialogImageTest.ArchiveType.ASSEMBLY);
		else
			new DialogImageTest(myType);
	}

	private void setLabelContactsExerciseSelected(Constants.Modes m)
	{
		string name = "";
		if(m == Constants.Modes.JUMPSSIMPLE)
			name = UtilGtk.ComboGetActive(combo_select_jumps);
		else if(m == Constants.Modes.JUMPSREACTIVE)
			name = UtilGtk.ComboGetActive(combo_select_jumps_rj);
		else if(m == Constants.Modes.RUNSSIMPLE)
			name = UtilGtk.ComboGetActive(combo_select_runs);
		else if(m == Constants.Modes.RUNSINTERVALLIC)
			name = UtilGtk.ComboGetActive(combo_select_runs_interval);
		else if(m == Constants.Modes.FORCESENSOR)
			name = UtilGtk.ComboGetActive(combo_force_sensor_exercise);
		else if(m == Constants.Modes.RUNSENCODER)
			name = UtilGtk.ComboGetActive(combo_run_encoder_exercise);

		if(name == "")
			name = Catalog.GetString("Need to create an exercise.");

		label_contacts_exercise_selected_name.Text = name;
	}
	private void setLabelContactsExerciseSelected(string name)
	{
		label_contacts_exercise_selected_name.Text = name;
	}

	private void on_contacts_exercise_value_changed (object o, EventArgs args)
	{
		setLabelContactsExerciseSelectedOptions();
	}

	private void setLabelContactsExerciseSelectedOptions()
	{
		LogB.Information("TT0");
		LogB.Information(current_mode.ToString());

		if(current_mode == Constants.Modes.JUMPSSIMPLE)
			setLabelContactsExerciseSelectedOptionsJumpsSimple();
		if(current_mode == Constants.Modes.JUMPSREACTIVE)
			setLabelContactsExerciseSelectedOptionsJumpsReactive();
	}

	private void setLabelContactsExerciseSelectedOptionsJumpsSimple()
	{
		LogB.Information("TT1");
		if(currentEventType == null)
			return;

		LogB.Information("TT2");
		string name = "";
		string sep = "";

		if(((JumpType) currentEventType).HasWeight)
		{
			if(extra_window_jumps_radiobutton_weight.Active)
				name += sep + label_extra_window_jumps_radiobutton_weight_percent_as_kg.Text;
			else
				name += sep + extra_window_jumps_spinbutton_weight.Value.ToString() + " kg";
				sep = "; ";
		}
		if(((JumpType) currentEventType).HasFall)
		{
			if(! extra_window_jumps_check_dj_fall_calculate.Active)
			{
				name += sep + extra_window_jumps_spinbutton_fall.Value.ToString() + " cm";
				sep = "; ";
			}
		}

		label_contacts_exercise_selected_options.Text = name;
	}

	private void setLabelContactsExerciseSelectedOptionsJumpsReactive()
	{
		LogB.Information("TT1");
		if(currentEventType == null)
			return;

		LogB.Information("TT2");
		string name = "";
		string sep = "";

		if(((JumpType) currentEventType).FixedValue >= 0)
		{
			name += extra_window_jumps_rj_spinbutton_limit.Value.ToString();
			if(((JumpType) currentEventType).JumpsLimited)
				name += " " + Catalog.GetString("jumps");
			else
				name += " s";

			sep = "; ";
		}
		if(((JumpType) currentEventType).HasWeight)
		{
			if(extra_window_jumps_rj_radiobutton_weight.Active)
				name += sep + label_extra_window_jumps_rj_radiobutton_weight_percent_as_kg.Text;
			else
				name += sep + extra_window_jumps_rj_spinbutton_weight.Value.ToString() + " kg";
			sep = "; ";
		}
		if(((JumpType) currentEventType).HasFall)
		{
			name += sep + extra_window_jumps_rj_spinbutton_fall.Value.ToString() + " cm";
			sep = "; ";
		}

		label_contacts_exercise_selected_options.Text = name;
	}

	private void setLabelContactsExerciseSelectedOptionsRunsSimple()
	{
		label_contacts_exercise_selected_options.Text = label_runs_simple_track_distance_value.Text + " " + label_runs_simple_track_distance_units.Text;
	}

	private void setLabelContactsExerciseSelectedOptionsRunsInterval()
	{
		LogB.Information("TT1");
		if(currentEventType == null)
			return;

		LogB.Information("TT2");
		string name = "";
		string sep = "";

		if( ((RunType) currentEventType).Distance >= 0 )
		{
			name = label_runs_interval_track_distance_value.Text + " m";
			sep = "; ";
		}
		if( ! ((RunType) currentEventType).Unlimited )
		{
			name += sep + extra_window_runs_interval_spinbutton_limit.Value;

			if( ((RunType) currentEventType).TracksLimited )
				name += " " + Catalog.GetString("laps");
			else
				name += " s";
		}

		label_contacts_exercise_selected_options.Text = name;
	}

	private void on_button_combo_select_contacts_top_left_clicked (object o, EventArgs args)
	{
		if(current_mode == Constants.Modes.JUMPSSIMPLE)
			contacts_exercise_left_button (combo_select_jumps,
					button_combo_jumps_exercise_capture_left,
					button_combo_jumps_exercise_capture_right);
		else if(current_mode == Constants.Modes.JUMPSREACTIVE)
			contacts_exercise_left_button (combo_select_jumps_rj,
					button_combo_jumps_rj_exercise_capture_left,
					button_combo_jumps_rj_exercise_capture_right);
		else if(current_mode == Constants.Modes.RUNSSIMPLE)
			contacts_exercise_left_button (combo_select_runs,
					button_combo_runs_exercise_capture_left,
					button_combo_runs_exercise_capture_right);
		else if(current_mode == Constants.Modes.RUNSINTERVALLIC)
			contacts_exercise_left_button (combo_select_runs_interval,
					button_combo_runs_interval_exercise_capture_left,
					button_combo_runs_interval_exercise_capture_right);
		else if(current_mode == Constants.Modes.RUNSENCODER)
			contacts_exercise_left_button (combo_run_encoder_exercise,
					button_combo_run_encoder_exercise_capture_left,
					button_combo_run_encoder_exercise_capture_right);
		else if(current_mode == Constants.Modes.FORCESENSOR)
			contacts_exercise_left_button (combo_force_sensor_exercise,
					button_combo_force_sensor_exercise_capture_left,
					button_combo_force_sensor_exercise_capture_right);
	}
	private void on_button_combo_select_contacts_top_right_clicked (object o, EventArgs args)
	{
		if(current_mode == Constants.Modes.JUMPSSIMPLE)
			contacts_exercise_right_button (combo_select_jumps,
					button_combo_jumps_exercise_capture_left,
					button_combo_jumps_exercise_capture_right);
		else if(current_mode == Constants.Modes.JUMPSREACTIVE)
			contacts_exercise_right_button (combo_select_jumps_rj,
					button_combo_jumps_rj_exercise_capture_left,
					button_combo_jumps_rj_exercise_capture_right);
		else if(current_mode == Constants.Modes.RUNSSIMPLE)
			contacts_exercise_right_button (combo_select_runs,
					button_combo_runs_exercise_capture_left,
					button_combo_runs_exercise_capture_right);
		else if(current_mode == Constants.Modes.RUNSINTERVALLIC)
			contacts_exercise_right_button (combo_select_runs_interval,
					button_combo_runs_interval_exercise_capture_left,
					button_combo_runs_interval_exercise_capture_right);
		else if(current_mode == Constants.Modes.RUNSENCODER)
			contacts_exercise_right_button (combo_run_encoder_exercise,
					button_combo_run_encoder_exercise_capture_left,
					button_combo_run_encoder_exercise_capture_right);
		else if(current_mode == Constants.Modes.FORCESENSOR)
			contacts_exercise_right_button (combo_force_sensor_exercise,
					button_combo_force_sensor_exercise_capture_left,
					button_combo_force_sensor_exercise_capture_right);
	}
}

public class ContactsCaptureDisplay : BooleansInt
{
	//constructor when we have the 0-7 value
	public ContactsCaptureDisplay(int selection)
	{
		this.i = selection;
	}

	//constructor with the 2 booleans
	public ContactsCaptureDisplay(bool showBit1, bool showBit2)
	{
		this.i = 0;
		if(showBit1)
			i ++;
		if(showBit2)
			i += 2;
	}

	public bool ShowGraph
	{
		get { return Bit2; }
	}

	public bool ShowTable
	{
		get { return Bit1; }
	}

	//just to debug
	public override string ToString()
	{
		return string.Format("selected: {0} (ShowGraph: {1}, ShowTable: {2})",
				i, ShowGraph, ShowTable);
	}
}
