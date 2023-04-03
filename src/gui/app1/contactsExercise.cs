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
 * Copyright (C) 2004-2023   Xavier de Blas <xaviblas@gmail.com>
 */


using System;
using Gtk;
//using Glade;
using Gdk;
using Mono.Unix;

public partial class ChronoJumpWindow 
{
	// at glade ---->
	Gtk.Frame frame_contacts_exercise;
	Gtk.Button button_combo_select_contacts_top_left;
	Gtk.Button button_combo_select_contacts_top_right;
	Gtk.Button button_contacts_exercise;
	Gtk.Frame frame_image_test;
	Gtk.Label label_contacts_exercise_selected_name;
	Gtk.Image image_contacts_exercise_selected_options1;
	Gtk.Image image_contacts_exercise_selected_options2;
	Gtk.Image image_contacts_exercise_selected_options3;
	Gtk.Label label_contacts_exercise_selected_options1;
	Gtk.Label label_contacts_exercise_selected_options2;
	Gtk.Label label_contacts_exercise_selected_options3;
	Gtk.HBox hbox_contacts_exercise_actions;
	Gtk.Button button_contacts_exercise_actions_edit_do;
	Gtk.Button button_contacts_exercise_actions_add_do;
	Gtk.Label label_contacts_exercise_error;
	// <---- at glade

	private void update_combo_select_contacts_top_using_combo (Gtk.ComboBoxText cb)
	{
		if(combo_select_contacts_top == null)
			return;

		comboSelectContactsTopNoFollow = true;

		UtilGtk.ComboUpdate (combo_select_contacts_top, UtilGtk.ComboGetValues (cb), "");
		combo_select_contacts_top.Active = cb.Active;

		comboSelectContactsTopNoFollow = false;
	}

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

		//do not show the image on runEncoder
		frame_image_test.Visible = current_mode != Constants.Modes.RUNSENCODER;

		//right now only can add/edit exercise images on forceSensor
		button_image_test_add_edit.Visible = Constants.ModeIsFORCESENSOR (current_mode);

		frame_run_encoder_exercise.Visible = false; //TODO: implement more modes in the future

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

	private void on_button_image_test_zoom_clicked (object o, EventArgs args)
	{
		if (Constants.ModeIsFORCESENSOR (current_mode))
			image_test_zoomByFiles ();
		else
			image_test_zoomByAssemblies ();
	}

	private void image_test_zoomByFiles () //right now only on: Constants.ModeIsFORCESENSOR (current_mode)
	{
		if(UtilGtk.ComboGetActive (combo_force_sensor_exercise) == "")
			return;

		ForceSensorExercise ex = (ForceSensorExercise) SqliteForceSensorExercise.Select (
                                false, getExerciseIDFromAnyCombo (combo_force_sensor_exercise, forceSensorComboExercisesString, false), -1, false, "")[0];

		ExerciseImage ei = new ExerciseImage (current_mode, ex.UniqueID);
		if (ei.GetUrlIfExists (false) != "")
			new DialogImageTest (UtilGtk.ComboGetActive (combo_force_sensor_exercise), ei.GetUrlIfExists (false),
					DialogImageTest.ArchiveType.FILE, "", app1.Allocation.Width, app1.Allocation.Height);
	}

	private void image_test_zoomByAssemblies ()
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

	private void on_button_image_test_add_edit_clicked (object o, EventArgs args)
	{
		Gtk.FileChooserDialog fc =
			new Gtk.FileChooserDialog(Catalog.GetString("Select an image"),
					null,
					FileChooserAction.Open,
					Catalog.GetString("Cancel"), ResponseType.Cancel,
					Catalog.GetString("Select"), ResponseType.Accept
					);

		fc.Filter = new FileFilter();
		fc.Filter.AddPattern("*.jpeg");
		fc.Filter.AddPattern("*.JPEG");
		fc.Filter.AddPattern("*.jpg");
		fc.Filter.AddPattern("*.JPG");
		fc.Filter.AddPattern("*.png");
		fc.Filter.AddPattern("*.PNG");

		if (fc.Run() == (int)ResponseType.Accept)
		{
			LogB.Warning ("Copying image ...");
			ForceSensorExercise ex = (ForceSensorExercise) SqliteForceSensorExercise.Select (
					false, getExerciseIDFromAnyCombo (combo_force_sensor_exercise, forceSensorComboExercisesString, false), -1, false, "")[0];

			ExerciseImage ei = new ExerciseImage (current_mode, ex.UniqueID);
			ei.CopyImageToLocal (fc.Filename);
			changeTestImage (ex.UniqueID);

			/*
			try {
				//File.Copy (fc.Filename)...
			} catch {
				LogB.Warning("Catched, maybe is used by another program");
				fc.Destroy();
				return;
			}
			*/
		}

		//Don't forget to call Destroy() or the FileChooserDialog window won't get closed.
		fc.Destroy();
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
		else if (Constants.ModeIsFORCESENSOR (m))
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

	private void label_contacts_exercise_selected_options_blank ()
	{
		image_contacts_exercise_selected_options1.Visible = false;
		image_contacts_exercise_selected_options2.Visible = false;
		image_contacts_exercise_selected_options3.Visible = false;

		label_contacts_exercise_selected_options1.Text = "";
		label_contacts_exercise_selected_options2.Text = "";
		label_contacts_exercise_selected_options3.Text = "";
	}

	private void label_contacts_exercise_selected_options_visible (bool visible)
	{
		image_contacts_exercise_selected_options1.Visible = visible;
		image_contacts_exercise_selected_options2.Visible = visible;
		image_contacts_exercise_selected_options3.Visible = visible;

		label_contacts_exercise_selected_options1.Visible = visible;
		label_contacts_exercise_selected_options2.Visible = visible;
		label_contacts_exercise_selected_options3.Visible = visible;
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
		LogB.Information("setLabelContactsExerciseSelectedOptionsJumpsSimple TT1");
		if(currentEventType == null)
			return;

		LogB.Information("TT2");
		string name = "";

		if(((JumpType) currentEventType).HasWeight)
		{
			if(extra_window_jumps_radiobutton_weight.Active)
				name = label_extra_window_jumps_radiobutton_weight_percent_as_kg.Text;
			else
				name = extra_window_jumps_spinbutton_weight.Value.ToString() + " kg";

			label_contacts_exercise_selected_options1.Text = name;
			image_contacts_exercise_selected_options1.Pixbuf =
				new Pixbuf (null, Util.GetImagePath(false) + "extra-mass.png");
			image_contacts_exercise_selected_options1.Visible = true;
		} else {
			label_contacts_exercise_selected_options1.Text = "";
			image_contacts_exercise_selected_options1.Visible = false;
		}

		LogB.Information("TT3");
		if( ((JumpType) currentEventType).HasFall (configChronojump.Compujump) &&
				! extra_window_jumps_check_dj_fall_calculate.Active )
		{
			name = extra_window_jumps_spinbutton_fall.Value.ToString() + " cm";
			label_contacts_exercise_selected_options2.Text = name;

			image_contacts_exercise_selected_options2.Pixbuf =
				new Pixbuf (null, Util.GetImagePath(false) + "image_fall.png");
			image_contacts_exercise_selected_options2.Visible = true;
		} else {
			label_contacts_exercise_selected_options2.Text = "";
			image_contacts_exercise_selected_options2.Visible = false;
		}

		label_contacts_exercise_selected_options3.Text = "";
		image_contacts_exercise_selected_options3.Visible = false;
		LogB.Information("TT4");
	}

	private void setLabelContactsExerciseSelectedOptionsJumpsReactive()
	{
		LogB.Information("setLabelContactsExerciseSelectedOptionsJumpsReactive TT1");
		if(currentEventType == null)
			return;

		LogB.Information("TT2");
		string name = "";

		if(((JumpType) currentEventType).FixedValue >= 0)
		{
			name = extra_window_jumps_rj_spinbutton_limit.Value.ToString();
			if(((JumpType) currentEventType).JumpsLimited)
				name += " " + Catalog.GetString("jumps");
			else
				name += " s";

			label_contacts_exercise_selected_options1.Text = name;
			image_contacts_exercise_selected_options1.Visible = false; //TODO: change to true when have the image;
		} else {
			label_contacts_exercise_selected_options1.Text = "";
			image_contacts_exercise_selected_options1.Visible = false;
		}

		LogB.Information("TT3");
		if(((JumpType) currentEventType).HasWeight)
		{
			if(extra_window_jumps_rj_radiobutton_weight.Active)
				name = label_extra_window_jumps_rj_radiobutton_weight_percent_as_kg.Text;
			else
				name = extra_window_jumps_rj_spinbutton_weight.Value.ToString() + " kg";

			label_contacts_exercise_selected_options2.Text = name;
			image_contacts_exercise_selected_options2.Pixbuf =
				new Pixbuf (null, Util.GetImagePath(false) + "extra-mass.png");
			image_contacts_exercise_selected_options2.Visible = true;
		} else {
			label_contacts_exercise_selected_options2.Text = "";
			image_contacts_exercise_selected_options2.Visible = false;
		}

		LogB.Information("TT4");
		if(((JumpType) currentEventType).HasFall (configChronojump.Compujump))
		{
			name = extra_window_jumps_rj_spinbutton_fall.Value.ToString() + " cm";
			label_contacts_exercise_selected_options3.Text = name;
			image_contacts_exercise_selected_options3.Pixbuf =
				new Pixbuf (null, Util.GetImagePath(false) + "image_fall.png");
			image_contacts_exercise_selected_options3.Visible = true;
		} else {
			label_contacts_exercise_selected_options3.Text = "";
			image_contacts_exercise_selected_options3.Visible = false;
		}
		LogB.Information("TT5");
	}

	private void setLabelContactsExerciseSelectedOptionsRunsSimple()
	{
		label_contacts_exercise_selected_options1.Text = label_runs_simple_track_distance_value.Text;
		image_contacts_exercise_selected_options1.Visible = false; //TODO: change to true when have the image;

		label_contacts_exercise_selected_options2.Text = label_runs_simple_track_distance_units.Text;
		image_contacts_exercise_selected_options2.Visible = false; //TODO: change to true when have the image;

		label_contacts_exercise_selected_options3.Text = "";
		image_contacts_exercise_selected_options3.Visible = false;
	}

	private void setLabelContactsExerciseSelectedOptionsRunsInterval()
	{
		LogB.Information("setLabelContactsExerciseSelectedOptionsRunsInterval TT1");
		if(currentEventType == null)
			return;

		LogB.Information("TT2");
		string name = "";

		if( ((RunType) currentEventType).Distance >= 0 )
		{
			name = label_runs_interval_track_distance_value.Text + " m";
			label_contacts_exercise_selected_options1.Text = name;
			image_contacts_exercise_selected_options1.Visible = false; //TODO: change to true when have the image;
		} else {
			label_contacts_exercise_selected_options1.Text = "";
			image_contacts_exercise_selected_options1.Visible = false;
		}

		LogB.Information("TT3");
		if( ! ((RunType) currentEventType).Unlimited )
		{
			name = "";

			if( ((RunType) currentEventType).TracksLimited )
			{
				name = extra_window_runs_interval_spinbutton_limit_tracks.Value.ToString();
				name += " " + Catalog.GetString("laps");
			} else {
				name = extra_window_runs_interval_spinbutton_limit_time.Value.ToString();
				name += " s";
			}

			label_contacts_exercise_selected_options2.Text = name;
			image_contacts_exercise_selected_options2.Visible = false; //TODO: change to true when have the image;
		} else {
			label_contacts_exercise_selected_options2.Text = "";
			image_contacts_exercise_selected_options2.Visible = false;
		}

		label_contacts_exercise_selected_options3.Text = "";
		image_contacts_exercise_selected_options3.Visible = false;
		LogB.Information("TT4");
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
		else if (Constants.ModeIsFORCESENSOR (current_mode))
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
		else if (Constants.ModeIsFORCESENSOR (current_mode))
			contacts_exercise_right_button (combo_force_sensor_exercise,
					button_combo_force_sensor_exercise_capture_left,
					button_combo_force_sensor_exercise_capture_right);
	}


	// ---- start of manage exercise edit/add from the app1.glade ---->

	private void show_contacts_exercise_add_edit (bool adding)
	{
		notebook_execute.Sensitive = false;
		frame_run_encoder_exercise.Visible = true; //TODO: in the future implement more modes
		hbox_contacts_exercise_actions.Sensitive = false;
		notebook_options_top.Visible = false;
		label_contacts_exercise_error.Text = "";


		if(adding) {
			button_contacts_exercise_actions_edit_do.Visible = false;
			button_contacts_exercise_actions_add_do.Visible = true;
		} else {
			button_contacts_exercise_actions_edit_do.Visible = true;
			button_contacts_exercise_actions_add_do.Visible = false;
		}
	}

	private void hide_contacts_exercise_add_edit ()
	{
		notebook_execute.Sensitive = true;
		frame_run_encoder_exercise.Visible = false;
		hbox_contacts_exercise_actions.Sensitive = true;
		notebook_options_top.Visible = true;
	}

	private void on_button_contacts_exercise_actions_cancel_clicked (object o, EventArgs args)
	{
		hide_contacts_exercise_add_edit ();
	}

	private void on_button_contacts_exercise_actions_edit_do_clicked (object o, EventArgs args)
	{
		if(current_mode == Constants.Modes.RUNSENCODER)
			if(run_encoder_exercise_do_add_or_edit (false))
				hide_contacts_exercise_add_edit ();
	}

	private void on_button_contacts_exercise_actions_add_do_clicked (object o, EventArgs args)
	{
		if(current_mode == Constants.Modes.RUNSENCODER)
			if(run_encoder_exercise_do_add_or_edit (true))
				hide_contacts_exercise_add_edit ();
	}

	// <---- end of manage exercise edit/add from the app1.glade ----


	private void connectWidgetsContactsExercise (Gtk.Builder builder)
	{
		frame_contacts_exercise = (Gtk.Frame) builder.GetObject ("frame_contacts_exercise");
		button_combo_select_contacts_top_left = (Gtk.Button) builder.GetObject ("button_combo_select_contacts_top_left");
		button_combo_select_contacts_top_right = (Gtk.Button) builder.GetObject ("button_combo_select_contacts_top_right");
		button_contacts_exercise = (Gtk.Button) builder.GetObject ("button_contacts_exercise");
		frame_image_test = (Gtk.Frame) builder.GetObject ("frame_image_test");
		label_contacts_exercise_selected_name = (Gtk.Label) builder.GetObject ("label_contacts_exercise_selected_name");
		image_contacts_exercise_selected_options1 = (Gtk.Image) builder.GetObject ("image_contacts_exercise_selected_options1");
		image_contacts_exercise_selected_options2 = (Gtk.Image) builder.GetObject ("image_contacts_exercise_selected_options2");
		image_contacts_exercise_selected_options3 = (Gtk.Image) builder.GetObject ("image_contacts_exercise_selected_options3");
		label_contacts_exercise_selected_options1 = (Gtk.Label) builder.GetObject ("label_contacts_exercise_selected_options1");
		label_contacts_exercise_selected_options2 = (Gtk.Label) builder.GetObject ("label_contacts_exercise_selected_options2");
		label_contacts_exercise_selected_options3 = (Gtk.Label) builder.GetObject ("label_contacts_exercise_selected_options3");
		hbox_contacts_exercise_actions = (Gtk.HBox) builder.GetObject ("hbox_contacts_exercise_actions");
		button_contacts_exercise_actions_edit_do = (Gtk.Button) builder.GetObject ("button_contacts_exercise_actions_edit_do");
		button_contacts_exercise_actions_add_do = (Gtk.Button) builder.GetObject ("button_contacts_exercise_actions_add_do");
		label_contacts_exercise_error = (Gtk.Label) builder.GetObject ("label_contacts_exercise_error");
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
