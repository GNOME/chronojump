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
 * Copyright (C) 2004-2019   Xavier de Blas <xaviblas@gmail.com> 
 */


using System;
using Gtk;
using Gdk;
using Mono.Unix;

public partial class ChronoJumpWindow 
{
	private void on_button_contacts_exercise_clicked (object o, EventArgs args)
	{
		notebook_contacts_capture_doing_wait.Sensitive = false;
		vbox_contacts_device_and_camera.Sensitive = false;
		notebook_session_person.Sensitive = false;
		main_menu.Sensitive = false;
		button_contacts_exercise.Sensitive = false;
		hbox_contacts_sup_capture_analyze_two_buttons.Sensitive = false;
		hbox_top_person.Sensitive = false;

		button_contacts_exercise_close_and_capture.Sensitive = myTreeViewPersons.IsThereAnyRecord();
		notebook_contacts_execute_or_instructions.CurrentPage = 1;
	}
	private void on_button_contacts_exercise_close_clicked (object o, EventArgs args)
	{
		notebook_contacts_capture_doing_wait.Sensitive = true;
		vbox_contacts_device_and_camera.Sensitive = true;
		notebook_session_person.Sensitive = true;
		main_menu.Sensitive = true;
		button_contacts_exercise.Sensitive = true;
		hbox_contacts_sup_capture_analyze_two_buttons.Sensitive = true;
		hbox_top_person.Sensitive = true;

		notebook_contacts_execute_or_instructions.CurrentPage = 0;
	}
	private void on_button_contacts_exercise_close_and_capture_clicked (object o, EventArgs args)
	{
		on_button_contacts_exercise_close_clicked (o, args);
		on_button_execute_test_clicked(o, args);
	}

	private void on_button_image_test_zoom_clicked(object o, EventArgs args)
	{
		EventType myType;
		if(current_menuitem_mode == Constants.Menuitem_modes.JUMPSSIMPLE)
			myType = currentJumpType;
		else if(current_menuitem_mode == Constants.Menuitem_modes.JUMPSREACTIVE)
			myType = currentJumpRjType;
		else if(current_menuitem_mode == Constants.Menuitem_modes.RUNSSIMPLE)
			myType = currentRunType;
		else if(current_menuitem_mode == Constants.Menuitem_modes.RUNSINTERVALLIC)
			myType = currentRunIntervalType;
		//else if(current_menuitem_mode == Constants.Menuitem_modes.RUNSENCODER)
		//	myType = currentRunIntervalType;
		//else if(current_menuitem_mode == Constants.Menuitem_modes.FORCESENSOR
		//	myType = currentForceType;
		else if(current_menuitem_mode == Constants.Menuitem_modes.RT)
			myType = currentReactionTimeType;
		else //if(current_menuitem_mode == Constants.Menuitem_modes.OTHER
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

	private void setLabelContactsExerciseSelected(Constants.Menuitem_modes m)
	{
		string name = "";
		if(m == Constants.Menuitem_modes.JUMPSSIMPLE)
			name = UtilGtk.ComboGetActive(combo_select_jumps);
		else if(m == Constants.Menuitem_modes.JUMPSREACTIVE)
			name = UtilGtk.ComboGetActive(combo_select_jumps_rj);
		else if(m == Constants.Menuitem_modes.RUNSSIMPLE)
			name = UtilGtk.ComboGetActive(combo_select_runs);
		else if(m == Constants.Menuitem_modes.RUNSINTERVALLIC)
			name = UtilGtk.ComboGetActive(combo_select_runs_interval);
		else if(m == Constants.Menuitem_modes.FORCESENSOR)
			name = UtilGtk.ComboGetActive(combo_force_sensor_exercise);
		else if(m == Constants.Menuitem_modes.RUNSENCODER)
			name = UtilGtk.ComboGetActive(combo_run_encoder_exercise);

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
		LogB.Information(current_menuitem_mode.ToString());
		if(current_menuitem_mode == Constants.Menuitem_modes.JUMPSSIMPLE)
			setLabelContactsExerciseSelectedOptionsJumpsSimple();
	}

	private void setLabelContactsExerciseSelectedOptionsJumpsSimple()
	{
		LogB.Information("TT1");
		if(currentEventType == null)
			return;

		LogB.Information("TT2");
		string name = "";
		string sep = "";

		if(((JumpType) currentEventType).HasFall)
		{
			if(! extra_window_jumps_check_dj_fall_calculate.Active)
			{
				name += extra_window_jumps_spinbutton_fall.Value.ToString() + " cm";
				sep = "; ";
			}
		} if(((JumpType) currentEventType).HasWeight)
		{
			if(extra_window_jumps_radiobutton_weight.Active)
				name += sep + label_extra_window_jumps_radiobutton_weight_percent_as_kg.Text;
			else
				name += sep + extra_window_jumps_spinbutton_weight.Value.ToString() + " kg";
		}

		label_contacts_exercise_selected_options.Text = name;
	}

	private void on_menuitem_mode_activate(object o, EventArgs args)
	{
		Gtk.ImageMenuItem imi = o as Gtk.ImageMenuItem;
		if (o == null)
			return;

		if(o == menuitem_mode_jumps_simple)
		{
			select_menuitem_mode_toggled(Constants.Menuitem_modes.JUMPSSIMPLE);
			image_contacts_exercise.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_jump_simple.png");
		} else if(o == menuitem_mode_jumps_reactive) {
			select_menuitem_mode_toggled(Constants.Menuitem_modes.JUMPSREACTIVE);
			image_contacts_exercise.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_jump_reactive.png");
		} else if(o == menuitem_mode_runs_simple) {
			select_menuitem_mode_toggled(Constants.Menuitem_modes.RUNSSIMPLE);
			image_contacts_exercise.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_run_simple.png");
		} else if(o == menuitem_mode_runs_intervallic) {
			select_menuitem_mode_toggled(Constants.Menuitem_modes.RUNSINTERVALLIC);
			image_contacts_exercise.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_run_multiple.png");
		} else if(o == menuitem_mode_race_encoder) {
			select_menuitem_mode_toggled(Constants.Menuitem_modes.RUNSENCODER);
			image_contacts_exercise.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "race_encoder_icon.png");
		} else if(o == menuitem_mode_power_gravitatory) {
			select_menuitem_mode_toggled(Constants.Menuitem_modes.POWERGRAVITATORY);
			image_encoder_exercise.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_weight.png");
		} else if(o == menuitem_mode_power_inertial) {
			select_menuitem_mode_toggled(Constants.Menuitem_modes.POWERINERTIAL);
			image_encoder_exercise.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_inertia.png");
		} else if(o == menuitem_mode_force_sensor) {
			select_menuitem_mode_toggled(Constants.Menuitem_modes.FORCESENSOR);
			image_contacts_exercise.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "force_sensor_icon.png");
		} else if(o == menuitem_mode_reaction_time) {
			select_menuitem_mode_toggled(Constants.Menuitem_modes.RT);
			image_contacts_exercise.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "reaction_time_icon.png");
		} else if(o == menuitem_mode_other) {
			select_menuitem_mode_toggled(Constants.Menuitem_modes.OTHER);
			image_contacts_exercise.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "other_icon.png");
		}

		changeMenuitemModeWidgets(image_menuitem_mode_jumps_simple, o == menuitem_mode_jumps_simple,
				"image_jump_simple.png", "image_jump_simple_yellow.png", (Label) menuitem_mode_jumps_simple.Child);
		changeMenuitemModeWidgets(image_menuitem_mode_jumps_reactive, o == menuitem_mode_jumps_reactive,
				"image_jump_reactive.png", "image_jump_reactive_yellow.png", (Label) menuitem_mode_jumps_reactive.Child);
		changeMenuitemModeWidgets(image_menuitem_mode_runs_simple, o == menuitem_mode_runs_simple,
				"image_run_simple.png", "image_run_simple_yellow.png", (Label) menuitem_mode_runs_simple.Child);
		changeMenuitemModeWidgets(image_menuitem_mode_runs_intervallic, o == menuitem_mode_runs_intervallic,
				"image_run_multiple.png", "image_run_multiple_yellow.png", (Label) menuitem_mode_runs_intervallic.Child);
		changeMenuitemModeWidgets(image_menuitem_mode_race_encoder, o == menuitem_mode_race_encoder,
				"race_encoder_icon.png", "race_encoder_icon_yellow.png", (Label) menuitem_mode_race_encoder.Child);
		changeMenuitemModeWidgets(image_menuitem_mode_power_gravitatory, o == menuitem_mode_power_gravitatory,
				"image_weight.png", "image_weight_yellow.png", (Label) menuitem_mode_power_gravitatory.Child);
		changeMenuitemModeWidgets(image_menuitem_mode_power_inertial, o == menuitem_mode_power_inertial,
				"image_inertia.png", "image_inertia_yellow.png", (Label) menuitem_mode_power_inertial.Child);
		changeMenuitemModeWidgets(image_menuitem_mode_force_sensor, o == menuitem_mode_force_sensor,
				"force_sensor_icon.png", "force_sensor_icon_yellow.png", (Label) menuitem_mode_force_sensor.Child);
		changeMenuitemModeWidgets(image_menuitem_mode_reaction_time, o == menuitem_mode_reaction_time,
				"reaction_time_icon.png", "reaction_time_icon_yellow.png", (Label) menuitem_mode_reaction_time.Child);
		changeMenuitemModeWidgets(image_menuitem_mode_other, o == menuitem_mode_other,
				"other_icon.png", "other_icon_yellow.png", (Label) menuitem_mode_other.Child);
	}

	private void changeMenuitemModeWidgets(Gtk.Image image, bool active, string pathImageInactive, string pathImageActive, Gtk.Label label)
	{
		Pixbuf pixbuf = new Pixbuf (null, Util.GetImagePath(false) + pathImageInactive);
		if(active)
			pixbuf = new Pixbuf (null, Util.GetImagePath(false) + pathImageActive);

		image.Pixbuf = pixbuf;

		if(active)
		{
			label.Text = Util.AddBoldMarks(label.Text);
			label.UseMarkup = true;
		} else
			label.Text = Util.RemoveBoldMarks(label.Text);
	}

}
