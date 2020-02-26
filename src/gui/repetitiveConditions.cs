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
 * Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Gdk;
using Glade;
using Mono.Unix;

public class RepetitiveConditionsWindow 
{
	[Widget] Gtk.Window repetitive_conditions;
	[Widget] Gtk.Notebook notebook_main;
	//[Widget] Gtk.ScrolledWindow scrolled_conditions;

	[Widget] Gtk.Frame frame_best_and_worst;
	[Widget] Gtk.Box hbox_jump_best_worst;
	[Widget] Gtk.Box hbox_run_best_worst;
	
	[Widget] Gtk.VBox vbox_encoder_stuff;
	[Widget] Gtk.Frame frame_conditions;

	/* jumps */	
	[Widget] Gtk.Box hbox_jump_conditions;
	[Widget] Gtk.CheckButton checkbutton_jump_tf_tc_best;
	[Widget] Gtk.CheckButton checkbutton_jump_tf_tc_worst;

	[Widget] Gtk.CheckButton checkbutton_height_greater;
	[Widget] Gtk.CheckButton checkbutton_height_lower;
	[Widget] Gtk.CheckButton checkbutton_tf_greater;
	[Widget] Gtk.CheckButton checkbutton_tf_lower;
	[Widget] Gtk.CheckButton checkbutton_tc_greater;
	[Widget] Gtk.CheckButton checkbutton_tc_lower;
	[Widget] Gtk.CheckButton checkbutton_tf_tc_greater;
	[Widget] Gtk.CheckButton checkbutton_tf_tc_lower;
	
	[Widget] Gtk.SpinButton spinbutton_height_greater;
	[Widget] Gtk.SpinButton spinbutton_height_lower;
	[Widget] Gtk.SpinButton spinbutton_tf_greater;
	[Widget] Gtk.SpinButton spinbutton_tf_lower;
	[Widget] Gtk.SpinButton spinbutton_tc_greater;
	[Widget] Gtk.SpinButton spinbutton_tc_lower;
	[Widget] Gtk.SpinButton spinbutton_tf_tc_greater;
	[Widget] Gtk.SpinButton spinbutton_tf_tc_lower;

	/* runs */	
	[Widget] Gtk.Box hbox_run_conditions;
	[Widget] Gtk.CheckButton checkbutton_run_time_best;
	[Widget] Gtk.CheckButton checkbutton_run_time_worst;
	
	[Widget] Gtk.CheckButton checkbutton_time_greater;
	[Widget] Gtk.CheckButton checkbutton_time_lower;

	[Widget] Gtk.SpinButton spinbutton_time_greater;
	[Widget] Gtk.SpinButton spinbutton_time_lower;

	/* encoder */
	[Widget] Gtk.Frame frame_encoder_automatic_conditions;
	[Widget] Gtk.HBox hbox_combo_encoder_main_variable;
	[Widget] Gtk.ComboBox combo_encoder_main_variable;
	[Widget] Gtk.RadioButton radio_encoder_relative_to_set;
	[Widget] Gtk.RadioButton radio_encoder_relative_to_historical;
	[Widget] Gtk.Label label_main_variable_text;
	[Widget] Gtk.CheckButton checkbutton_encoder_automatic_greater;
	[Widget] Gtk.CheckButton checkbutton_encoder_automatic_lower;
	[Widget] Gtk.SpinButton spinbutton_encoder_automatic_greater;
	[Widget] Gtk.SpinButton spinbutton_encoder_automatic_lower;
	[Widget] Gtk.CheckButton check_encoder_show_secondary_variable;
	[Widget] Gtk.HBox hbox_combo_encoder_secondary_variable;
	[Widget] Gtk.ComboBox combo_encoder_secondary_variable;
	[Widget] Gtk.RadioButton radio_encoder_eccon_both;
	[Widget] Gtk.RadioButton radio_encoder_eccon_ecc;
	[Widget] Gtk.RadioButton radio_encoder_eccon_con;
	[Widget] Gtk.CheckButton check_encoder_inertial_ecc_overload;
	[Widget] Gtk.CheckButton check_encoder_inertial_ecc_overload_percent;

	[Widget] Gtk.VBox vbox_encoder_manual;
	[Widget] Gtk.CheckButton checkbutton_encoder_show_manual_feedback;
	[Widget] Gtk.Notebook notebook_encoder_conditions;
	[Widget] Gtk.CheckButton checkbutton_encoder_height_higher;
	[Widget] Gtk.CheckButton checkbutton_encoder_height_lower;
	[Widget] Gtk.CheckButton checkbutton_encoder_mean_speed_higher;
	[Widget] Gtk.CheckButton checkbutton_encoder_max_speed_higher;
	[Widget] Gtk.CheckButton checkbutton_encoder_mean_speed_lower;
	[Widget] Gtk.CheckButton checkbutton_encoder_max_speed_lower;
	[Widget] Gtk.CheckButton checkbutton_encoder_mean_force_higher;
	[Widget] Gtk.CheckButton checkbutton_encoder_max_force_higher;
	[Widget] Gtk.CheckButton checkbutton_encoder_mean_force_lower;
	[Widget] Gtk.CheckButton checkbutton_encoder_max_force_lower;
	[Widget] Gtk.CheckButton checkbutton_encoder_power_higher;
	[Widget] Gtk.CheckButton checkbutton_encoder_peakpower_higher;
	[Widget] Gtk.CheckButton checkbutton_encoder_power_lower;
	[Widget] Gtk.CheckButton checkbutton_encoder_peakpower_lower;
	[Widget] Gtk.SpinButton spinbutton_encoder_height_higher;
	[Widget] Gtk.SpinButton spinbutton_encoder_height_lower;
	[Widget] Gtk.SpinButton spinbutton_encoder_mean_speed_higher;
	[Widget] Gtk.SpinButton spinbutton_encoder_max_speed_higher;
	[Widget] Gtk.SpinButton spinbutton_encoder_mean_speed_lower;
	[Widget] Gtk.SpinButton spinbutton_encoder_max_speed_lower;
	[Widget] Gtk.SpinButton spinbutton_encoder_mean_force_higher;
	[Widget] Gtk.SpinButton spinbutton_encoder_max_force_higher;
	[Widget] Gtk.SpinButton spinbutton_encoder_mean_force_lower;
	[Widget] Gtk.SpinButton spinbutton_encoder_max_force_lower;
	[Widget] Gtk.SpinButton spinbutton_encoder_power_higher;
	[Widget] Gtk.SpinButton spinbutton_encoder_peakpower_higher;
	[Widget] Gtk.SpinButton spinbutton_encoder_power_lower;
	[Widget] Gtk.SpinButton spinbutton_encoder_peakpower_lower;
	[Widget] Gtk.Button button_encoder_automatic_greater_minus_1;
	[Widget] Gtk.Button button_encoder_automatic_greater_plus_1;
	[Widget] Gtk.Button button_encoder_automatic_lower_minus_1;
	[Widget] Gtk.Button button_encoder_automatic_lower_plus_1;


	[Widget] Gtk.Button button_test_good;
	[Widget] Gtk.Label label_test_sound_result;
	[Widget] Gtk.Button button_close;

	[Widget] Gtk.HBox hbox_test_bells;
	//bells good (green)
	[Widget] Gtk.Image image_repetitive_best_tf_tc;
	[Widget] Gtk.Image image_repetitive_best_time;
	[Widget] Gtk.Image image_repetitive_height_greater;
	[Widget] Gtk.Image image_repetitive_tf_greater;
	[Widget] Gtk.Image image_repetitive_tc_lower;
	[Widget] Gtk.Image image_repetitive_tf_tc_greater;
	[Widget] Gtk.Image image_repetitive_time_lower;
	[Widget] Gtk.Image image_repetitive_encoder_automatic_greater;
	[Widget] Gtk.Image image_encoder_height_higher;
	[Widget] Gtk.Image image_encoder_mean_speed_higher;
	[Widget] Gtk.Image image_encoder_max_speed_higher;
	[Widget] Gtk.Image image_encoder_mean_force_higher;
	[Widget] Gtk.Image image_encoder_max_force_higher;
	[Widget] Gtk.Image image_encoder_power_higher;
	[Widget] Gtk.Image image_encoder_peakpower_higher;
	[Widget] Gtk.Image image_repetitive_test_good;
	//bells bad (red)
	[Widget] Gtk.Image image_repetitive_worst_tf_tc;
	[Widget] Gtk.Image image_repetitive_worst_time;
	[Widget] Gtk.Image image_repetitive_height_lower;
	[Widget] Gtk.Image image_repetitive_tf_lower;
	[Widget] Gtk.Image image_repetitive_tc_greater;
	[Widget] Gtk.Image image_repetitive_tf_tc_lower;
	[Widget] Gtk.Image image_repetitive_time_greater;
	[Widget] Gtk.Image image_repetitive_encoder_automatic_lower;
	[Widget] Gtk.Image image_encoder_height_lower;
	[Widget] Gtk.Image image_encoder_mean_speed_lower;
	[Widget] Gtk.Image image_encoder_max_speed_lower;
	[Widget] Gtk.Image image_encoder_mean_force_lower;
	[Widget] Gtk.Image image_encoder_max_force_lower;
	[Widget] Gtk.Image image_encoder_power_lower;
	[Widget] Gtk.Image image_encoder_peakpower_lower;
	[Widget] Gtk.Image image_repetitive_test_bad;

	//encoder rhythm
	[Widget] Gtk.Label label_rhythm_tab;
	[Widget] Gtk.CheckButton check_rhythm_active;
	[Widget] Gtk.RadioButton radio_rhythm_together;
	[Widget] Gtk.RadioButton radio_rhythm_separated;
	[Widget] Gtk.Notebook notebook_duration_repetition;
	[Widget] Gtk.VBox vbox_rhythm_cluster;
	[Widget] Gtk.Frame frame_rhythm;
	[Widget] Gtk.CheckButton check_rhythm_use_clusters;
	[Widget] Gtk.SpinButton	spin_rhythm_rep;
	[Widget] Gtk.SpinButton	spin_rhythm_ecc;
	[Widget] Gtk.SpinButton	spin_rhythm_con;
	[Widget] Gtk.Label label_rhythm_ecc_plus_con;
	[Widget] Gtk.SpinButton	spin_rhythm_rest_reps;
	[Widget] Gtk.VBox vbox_rhythm_rest_after;
	[Widget] Gtk.RadioButton radio_rest_after_ecc;
	[Widget] Gtk.SpinButton	spin_rhythm_reps_cluster;
	[Widget] Gtk.SpinButton	spin_rhythm_rest_clusters;
	[Widget] Gtk.Image image_clusters_info;
	[Widget] Gtk.HBox hbox_rhythm_rest_reps_value;
	[Widget] Gtk.CheckButton check_rhythm_rest_reps;

	//forceSensor
	[Widget] Gtk.VBox vbox_force_capture_feedback;
	[Widget] Gtk.CheckButton check_force_sensor_capture_feedback;
	[Widget] Gtk.HBox hbox_force_sensor_capture_feedback;
	[Widget] Gtk.SpinButton spin_force_sensor_capture_feedback_at;
	[Widget] Gtk.SpinButton spin_force_sensor_capture_feedback_range;

	const int FEEDBACKPAGE = 0;
	const int RHYTHMPAGE = 1;

	public Gtk.Button FakeButtonClose;

	//static bool volumeOn;
	bool volumeOn;
	public Preferences.GstreamerTypes gstreamer;

	public enum BestSetValueEnum { CAPTURE_MAIN_VARIABLE, AUTOMATIC_FEEDBACK}
	private double bestSetValueCaptureMainVariable;
	private double bestSetValueAutomaticFeedback;
	private bool update_checkbuttons_encoder_automatic;
	
	static RepetitiveConditionsWindow RepetitiveConditionsWindowBox;
		
	RepetitiveConditionsWindow ()
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "repetitive_conditions.glade", "repetitive_conditions", "chronojump");
		gladeXML.Autoconnect(this);
		
		//don't show until View is called
		repetitive_conditions.Hide ();

		//put an icon to window
		UtilGtk.IconWindow(repetitive_conditions);
		
		FakeButtonClose = new Gtk.Button();
		
		//createComboEncoderAutomaticVariable();
		createComboEncoderMainAndSecondaryVariables();

		bestSetValueCaptureMainVariable = 0;
		bestSetValueCaptureMainVariable = 0;
		notebook_encoder_conditions.CurrentPage = 3; //power

		putNonStandardIcons();

		label_rhythm_tab.Text = Catalog.GetString("Rhythm") + " / " + Catalog.GetString("Protocol");
	}

	static public RepetitiveConditionsWindow Create ()
	{
		if (RepetitiveConditionsWindowBox == null) {
			RepetitiveConditionsWindowBox = new RepetitiveConditionsWindow (); 
		}
	
		//don't show until View is called
		//RepetitiveConditionsWindowBox.repetitive_conditions.Hide ();
		
		return RepetitiveConditionsWindowBox;
	}

	public void View (Constants.BellModes bellMode, Preferences preferences, EncoderRhythm encoderRhythm)
	{
		//when user "deleted_event" the window
		if (RepetitiveConditionsWindowBox == null) {
			RepetitiveConditionsWindowBox = new RepetitiveConditionsWindow (); 
		}
		RepetitiveConditionsWindowBox.update_checkbuttons_encoder_automatic = true;
		RepetitiveConditionsWindowBox.showWidgets(bellMode,
				preferences.encoderCaptureMainVariable, preferences.encoderCaptureSecondaryVariable,
				preferences.encoderCaptureSecondaryVariableShow,
				preferences.encoderCaptureInertialEccOverloadMode,
				preferences.encoderCaptureMainVariableThisSetOrHistorical,
				preferences.encoderCaptureMainVariableGreaterActive,
				preferences.encoderCaptureMainVariableGreaterValue,
				preferences.encoderCaptureMainVariableLowerActive,
				preferences.encoderCaptureMainVariableLowerValue,
				preferences.encoderCaptureFeedbackEccon,
				encoderRhythm,
				preferences.forceSensorCaptureFeedbackActive,
				preferences.forceSensorCaptureFeedbackAt,
				preferences.forceSensorCaptureFeedbackRange);

		RepetitiveConditionsWindowBox.repetitive_conditions.Show ();
		RepetitiveConditionsWindowBox.volumeOn = preferences.volumeOn;
		RepetitiveConditionsWindowBox.gstreamer = preferences.gstreamer;
	}

	void showWidgets(Constants.BellModes bellMode,
			Constants.EncoderVariablesCapture encoderMainVariable,
			Constants.EncoderVariablesCapture encoderSecondaryVariable,
			bool encoderSecondaryVariableShow,
			Preferences.encoderCaptureEccOverloadModes encoderCaptureEccOverloadMode,
			bool encoderCaptureMainVariableThisSetOrHistorical,
			bool encoderCaptureMainVariableGreaterActive,
			int encoderCaptureMainVariableGreaterValue,
			bool encoderCaptureMainVariableLowerActive,
			int encoderCaptureMainVariableLowerValue,
			Preferences.EncoderPhasesEnum encoderCaptureFeedbackEccon,
			EncoderRhythm encoderRhythm,
			bool forceSensorCaptureFeedbackActive,
			int forceSensorCaptureFeedbackAt,
			int forceSensorCaptureFeedbackRange)
	{
		frame_best_and_worst.Hide();
		frame_conditions.Hide();
		hbox_jump_best_worst.Hide();
		hbox_run_best_worst.Hide();
		hbox_jump_conditions.Hide();
		hbox_run_conditions.Hide();
		frame_encoder_automatic_conditions.Hide();
		vbox_encoder_manual.Hide();
		notebook_encoder_conditions.Hide();
		vbox_encoder_stuff.Hide();
		vbox_force_capture_feedback.Hide();
		hbox_test_bells.Hide();

		notebook_main.GetNthPage(RHYTHMPAGE).Hide();
		notebook_main.ShowTabs = false;

		if(bellMode == Constants.BellModes.JUMPS) {
			frame_best_and_worst.Show();
			hbox_jump_best_worst.Show();
			hbox_jump_conditions.Show();
			frame_conditions.Show();
			hbox_test_bells.Show();
		} else if(bellMode == Constants.BellModes.RUNS) {
			frame_best_and_worst.Show();
			hbox_run_best_worst.Show();
			hbox_run_conditions.Show();
			frame_conditions.Show();
			hbox_test_bells.Show();
		} else if (bellMode == Constants.BellModes.ENCODERGRAVITATORY || bellMode == Constants.BellModes.ENCODERINERTIAL)
		{
			vbox_encoder_stuff.Show();
			frame_encoder_automatic_conditions.Show();
			notebook_main.ShowTabs = true;

			vbox_encoder_manual.Show();
			if(checkbutton_encoder_show_manual_feedback.Active)
				notebook_encoder_conditions.Show();

			combo_encoder_main_variable.Active = UtilGtk.ComboMakeActive(combo_encoder_main_variable,
					Constants.GetEncoderVariablesCapture(encoderMainVariable));
			combo_encoder_secondary_variable.Active = UtilGtk.ComboMakeActive(combo_encoder_secondary_variable,
					Constants.GetEncoderVariablesCapture(encoderSecondaryVariable));

			if(encoderCaptureEccOverloadMode == Preferences.encoderCaptureEccOverloadModes.NOT_SHOW) {
				check_encoder_inertial_ecc_overload.Active = false;
				check_encoder_inertial_ecc_overload_percent.Active = false;
				check_encoder_inertial_ecc_overload_percent.Visible = false;
			} else if (encoderCaptureEccOverloadMode == Preferences.encoderCaptureEccOverloadModes.SHOW_LINE) {
				check_encoder_inertial_ecc_overload.Active = true;
				check_encoder_inertial_ecc_overload_percent.Active = false;
				check_encoder_inertial_ecc_overload_percent.Visible = true;
			} else { // (encoderCaptureEccOverloadMode == Preferences.encoderCaptureEccOverloadModes.SHOW_LINE_AND_PERCENT)
				check_encoder_inertial_ecc_overload.Active = true;
				check_encoder_inertial_ecc_overload_percent.Active = true;
				check_encoder_inertial_ecc_overload_percent.Visible = true;
			}

			if(encoderSecondaryVariableShow)
				check_encoder_show_secondary_variable.Active = true;
			else
				check_encoder_show_secondary_variable.Active = false;

			//need to do it "manually" at start
			hbox_combo_encoder_secondary_variable.Visible = check_encoder_show_secondary_variable.Active;

			//MeanPower, MeanSpeed, MeanFOrce can be historical
			if(encoderCaptureMainVariableThisSetOrHistorical || (
						encoderMainVariable != Constants.EncoderVariablesCapture.MeanPower &&
						encoderMainVariable != Constants.EncoderVariablesCapture.MeanSpeed &&
						encoderMainVariable != Constants.EncoderVariablesCapture.MeanForce
					  ) )
				radio_encoder_relative_to_set.Active = true;
			else
				radio_encoder_relative_to_historical.Active = true;

			//the change on spinbuttons here will not have to provoque changes on checkbuttons
			update_checkbuttons_encoder_automatic = false;
			checkbutton_encoder_automatic_greater.Active = encoderCaptureMainVariableGreaterActive;
			spinbutton_encoder_automatic_greater.Value = encoderCaptureMainVariableGreaterValue;
			checkbutton_encoder_automatic_lower.Active = encoderCaptureMainVariableLowerActive;
			spinbutton_encoder_automatic_lower.Value = encoderCaptureMainVariableLowerValue;
			update_checkbuttons_encoder_automatic = true;

			if(encoderCaptureFeedbackEccon == Preferences.EncoderPhasesEnum.ECC)
				radio_encoder_eccon_ecc.Active = true;
			else if(encoderCaptureFeedbackEccon == Preferences.EncoderPhasesEnum.CON)
				radio_encoder_eccon_con.Active = true;
			else
				radio_encoder_eccon_both.Active = true;

			notebook_main.GetNthPage(RHYTHMPAGE).Show();
			encoder_rhythm_set_values(encoderRhythm);
			hbox_test_bells.Show();
		}
		else if(bellMode == Constants.BellModes.FORCESENSOR)
		{
			if(forceSensorCaptureFeedbackActive)
			{
				check_force_sensor_capture_feedback.Active = true;
				hbox_force_sensor_capture_feedback.Sensitive = true;
			} else {
				check_force_sensor_capture_feedback.Active = false;
				hbox_force_sensor_capture_feedback.Sensitive = false;
			}

			spin_force_sensor_capture_feedback_at.Value = forceSensorCaptureFeedbackAt;
			spin_force_sensor_capture_feedback_range.Value = forceSensorCaptureFeedbackRange;

			vbox_force_capture_feedback.Visible = true;
		}

		label_test_sound_result.Text = "";
	}
		
	private void createComboEncoderMainAndSecondaryVariables()
	{
		//1 mainVariable
		combo_encoder_main_variable = ComboBox.NewText ();
		comboEncoderVariableFill(combo_encoder_main_variable);

		hbox_combo_encoder_main_variable.PackStart(combo_encoder_main_variable, false, false, 0);
		hbox_combo_encoder_main_variable.ShowAll();
		combo_encoder_main_variable.Sensitive = true;
		combo_encoder_main_variable.Changed += new EventHandler (on_combo_encoder_main_variable_changed);

		//1 secondaryVariable
		combo_encoder_secondary_variable = ComboBox.NewText ();
		comboEncoderVariableFill(combo_encoder_secondary_variable);

		hbox_combo_encoder_secondary_variable.PackStart(combo_encoder_secondary_variable, false, false, 0);
		hbox_combo_encoder_secondary_variable.ShowAll();
		combo_encoder_secondary_variable.Sensitive = true;
		//combo_encoder_secondary_variable.Changed += new EventHandler (on_combo_encoder_secondary_variable_changed);
	}
	private void comboEncoderVariableFill(Gtk.ComboBox combo)
	{
		string [] values = { Constants.RangeAbsolute, Constants.MeanSpeed, Constants.MaxSpeed, Constants.MeanForce, Constants.MaxForce, Constants.MeanPower, Constants.PeakPower };

		UtilGtk.ComboUpdate(combo, values, "");
	}

	private void on_combo_encoder_main_variable_changed (object o, EventArgs args)
	{
		string mainVariable = UtilGtk.ComboGetActive(combo_encoder_main_variable);

		radio_encoder_relative_to_historical.Visible =
			(mainVariable == "Mean power" || mainVariable == "Mean speed" || mainVariable == "Mean force");

		if(mainVariable != "Mean power" && mainVariable != "Mean speed" && mainVariable != "Mean force")
			radio_encoder_relative_to_set.Active = true;

		label_main_variable_text.Text = mainVariable;
	}

	private void on_check_encoder_show_secondary_variable_toggled (object o, EventArgs args)
	{
		hbox_combo_encoder_secondary_variable.Visible = check_encoder_show_secondary_variable.Active;
	}

	private void on_check_encoder_inertial_ecc_overload_toggled (object o, EventArgs args)
	{
		check_encoder_inertial_ecc_overload_percent.Visible = check_encoder_inertial_ecc_overload.Active;
		if(! check_encoder_inertial_ecc_overload.Active)
			check_encoder_inertial_ecc_overload_percent.Active = false;
	}

	private void putNonStandardIcons() {
		Pixbuf pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_bell_green.png");
		image_repetitive_best_tf_tc.Pixbuf = pixbuf;
		image_repetitive_best_time.Pixbuf = pixbuf;
		image_repetitive_height_greater.Pixbuf = pixbuf;
		image_repetitive_tf_greater.Pixbuf = pixbuf;
		image_repetitive_tc_lower.Pixbuf = pixbuf;
		image_repetitive_tf_tc_greater.Pixbuf = pixbuf;
		image_repetitive_time_lower.Pixbuf = pixbuf;
		image_repetitive_encoder_automatic_greater.Pixbuf = pixbuf;
		image_encoder_height_higher.Pixbuf = pixbuf;
		image_encoder_mean_speed_higher.Pixbuf = pixbuf;
		image_encoder_max_speed_higher.Pixbuf = pixbuf;
		image_encoder_mean_force_higher.Pixbuf = pixbuf;
		image_encoder_max_force_higher.Pixbuf = pixbuf;
		image_encoder_power_higher.Pixbuf = pixbuf;
		image_encoder_peakpower_higher.Pixbuf = pixbuf;
		image_repetitive_test_good.Pixbuf = pixbuf;
		
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_bell_red.png");
		image_repetitive_worst_tf_tc.Pixbuf = pixbuf;
		image_repetitive_worst_time.Pixbuf = pixbuf;
		image_repetitive_height_lower.Pixbuf = pixbuf;
		image_repetitive_tf_lower.Pixbuf = pixbuf;
		image_repetitive_tc_greater.Pixbuf = pixbuf;
		image_repetitive_tf_tc_lower.Pixbuf = pixbuf;
		image_repetitive_time_greater.Pixbuf = pixbuf;
		image_repetitive_encoder_automatic_lower.Pixbuf = pixbuf;
		image_encoder_height_lower.Pixbuf = pixbuf;
		image_encoder_mean_speed_lower.Pixbuf = pixbuf;
		image_encoder_max_speed_lower.Pixbuf = pixbuf;
		image_encoder_mean_force_lower.Pixbuf = pixbuf;
		image_encoder_max_force_lower.Pixbuf = pixbuf;
		image_encoder_power_lower.Pixbuf = pixbuf;
		image_encoder_peakpower_lower.Pixbuf = pixbuf;
		image_repetitive_test_bad.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_info.png");
		image_clusters_info.Pixbuf = pixbuf;
	}

	void on_button_test_clicked (object o, EventArgs args)
	{
		if(volumeOn)
		{
			Util.TestSound = true;

			label_test_sound_result.Text = "";
			Util.SoundCodes sc;
			if (o == button_test_good) 
				sc = Util.PlaySound(Constants.SoundTypes.GOOD, true, gstreamer);
			else //button_test_bad
				sc = Util.PlaySound(Constants.SoundTypes.BAD, true, gstreamer);

			if(sc == Util.SoundCodes.OK)
				label_test_sound_result.Text = Catalog.GetString("Sound working");
			else
				label_test_sound_result.Text = Catalog.GetString("Sound not working");

			Util.TestSound = false;
		} else
			new DialogMessage(Constants.MessageTypes.INFO, 
					Catalog.GetString("You need to activate sounds in preferences / multimedia."));

	}

	void on_button_close_clicked (object o, EventArgs args)
	{
		RepetitiveConditionsWindowBox.repetitive_conditions.Hide();
		FakeButtonClose.Click();
		//RepetitiveConditionsWindowBox = null;
	}

	void on_delete_event (object o, DeleteEventArgs args)
	{
		//RepetitiveConditionsWindowBox.repetitive_conditions.Hide();
		//RepetitiveConditionsWindowBox = null;
		
		button_close.Click();
		args.RetVal = true;
	}

	public bool FeedbackActive (Constants.BellModes bellMode)
	{
		if(bellMode == Constants.BellModes.JUMPS)
		{
			if(checkbutton_height_greater.Active || checkbutton_height_lower.Active ||
					checkbutton_tf_greater.Active || checkbutton_tf_lower.Active ||
					checkbutton_tc_lower.Active || checkbutton_tc_greater.Active ||
					checkbutton_tf_tc_greater.Active || checkbutton_tf_tc_lower.Active)
				return true;
		}
		else if(bellMode == Constants.BellModes.RUNS)
		{
			if(checkbutton_time_lower.Active || checkbutton_time_greater.Active)
				return true;
		}
		else if (bellMode == Constants.BellModes.ENCODERGRAVITATORY || bellMode == Constants.BellModes.ENCODERINERTIAL)
		{
			if(checkbutton_encoder_automatic_greater.Active || checkbutton_encoder_automatic_lower.Active ||
					checkbutton_encoder_height_higher.Active || checkbutton_encoder_height_lower.Active ||
					checkbutton_encoder_mean_speed_higher.Active || checkbutton_encoder_mean_speed_lower.Active ||
					checkbutton_encoder_max_speed_higher.Active || checkbutton_encoder_max_speed_lower.Active ||
					checkbutton_encoder_mean_force_higher.Active || checkbutton_encoder_mean_force_lower.Active ||
					checkbutton_encoder_max_force_higher.Active || checkbutton_encoder_max_force_lower.Active ||
					checkbutton_encoder_power_higher.Active || checkbutton_encoder_power_lower.Active ||
					checkbutton_encoder_peakpower_higher.Active || checkbutton_encoder_peakpower_lower.Active)
				return true;
		}
		else if(bellMode == Constants.BellModes.FORCESENSOR)
			return check_force_sensor_capture_feedback.Active;

		return false;
	}

	public bool VolumeOn {
		set { volumeOn = value; }
	}
	public Preferences.GstreamerTypes Gstreamer {
		set { gstreamer = value; }
	}

	/* Auto.mark checkbox if spinbutton is changed */
	
	/* jumps */
	void on_spinbutton_height_greater_value_changed (object o, EventArgs args) {
		checkbutton_height_greater.Active = true;
	}
	void on_spinbutton_height_lower_value_changed (object o, EventArgs args) {
		checkbutton_height_lower.Active = true;
	}

	void on_spinbutton_tf_greater_value_changed (object o, EventArgs args) {
		checkbutton_tf_greater.Active = true;
	}
	void on_spinbutton_tf_lower_value_changed (object o, EventArgs args) {
		checkbutton_tf_lower.Active = true;
	}

	void on_spinbutton_tc_greater_value_changed (object o, EventArgs args) {
		checkbutton_tc_greater.Active = true;
	}
	void on_spinbutton_tc_lower_value_changed (object o, EventArgs args) {
		checkbutton_tc_lower.Active = true;
	}

	void on_spinbutton_tf_tc_greater_value_changed (object o, EventArgs args) {
		checkbutton_tf_tc_greater.Active = true;
	}
	void on_spinbutton_tf_tc_lower_value_changed (object o, EventArgs args) {
		checkbutton_tf_tc_lower.Active = true;
	}

	/*runs*/
	void on_spinbutton_time_greater_value_changed (object o, EventArgs args) {
		checkbutton_time_greater.Active = true;
	}
	void on_spinbutton_time_lower_value_changed (object o, EventArgs args) {
		checkbutton_time_lower.Active = true;
	}

	/* encoder */

	void on_radio_encoder_relative_to_toggled (object o, EventArgs args)
	{
		/*
		if(radio_encoder_relative_to_set.Active)
			comboEncoderAutomaticVariableFillThisSet();
		else
			comboEncoderAutomaticVariableFillHistorical();
			*/
	}


	void on_spinbutton_encoder_automatic_greater_value_changed (object o, EventArgs args)
	{
		if(update_checkbuttons_encoder_automatic)
			checkbutton_encoder_automatic_greater.Active = true;
	}
	void on_spinbutton_encoder_automatic_lower_value_changed (object o, EventArgs args)
	{
		if(update_checkbuttons_encoder_automatic)
			checkbutton_encoder_automatic_lower.Active = true;
	}

	void on_button_encoder_automatic_greater_minus_1_clicked (object o, EventArgs args)
	{
		spinbutton_encoder_automatic_greater.Value --;
	}
	void on_button_encoder_automatic_greater_plus_1_clicked (object o, EventArgs args)
	{
		spinbutton_encoder_automatic_greater.Value ++;
	}
	void on_button_encoder_automatic_lower_minus_1_clicked (object o, EventArgs args)
	{
		spinbutton_encoder_automatic_lower.Value --;
	}
	void on_button_encoder_automatic_lower_plus_1_clicked (object o, EventArgs args)
	{
		spinbutton_encoder_automatic_lower.Value ++;
	}
			
	void on_checkbutton_encoder_show_manual_feedback_toggled (object o, EventArgs args) {
		if(checkbutton_encoder_show_manual_feedback.Active)
			notebook_encoder_conditions.Show();
		else
			notebook_encoder_conditions.Hide();
	}
	
	void on_spinbutton_encoder_height_higher_value_changed (object o, EventArgs args) {
		checkbutton_encoder_height_higher.Active = true;
	}
	void on_spinbutton_encoder_height_lower_value_changed (object o, EventArgs args) {
		checkbutton_encoder_height_lower.Active = true;
	}
	
	void on_spinbutton_encoder_mean_speed_higher_value_changed (object o, EventArgs args) {
		checkbutton_encoder_mean_speed_higher.Active = true;
	}
	void on_spinbutton_encoder_mean_speed_lower_value_changed (object o, EventArgs args) {
		checkbutton_encoder_mean_speed_lower.Active = true;
	}
	
	void on_spinbutton_encoder_max_speed_higher_value_changed (object o, EventArgs args) {
		checkbutton_encoder_max_speed_higher.Active = true;
	}
	void on_spinbutton_encoder_max_speed_lower_value_changed (object o, EventArgs args) {
		checkbutton_encoder_max_speed_lower.Active = true;
	}
	
	void on_spinbutton_encoder_mean_force_higher_value_changed (object o, EventArgs args) {
		checkbutton_encoder_mean_force_higher.Active = true;
	}
	void on_spinbutton_encoder_mean_force_lower_value_changed (object o, EventArgs args) {
		checkbutton_encoder_mean_force_lower.Active = true;
	}
	
	void on_spinbutton_encoder_max_force_higher_value_changed (object o, EventArgs args) {
		checkbutton_encoder_max_force_higher.Active = true;
	}
	void on_spinbutton_encoder_max_force_lower_value_changed (object o, EventArgs args) {
		checkbutton_encoder_max_force_lower.Active = true;
	}
	
	void on_spinbutton_encoder_power_higher_value_changed (object o, EventArgs args) {
		checkbutton_encoder_power_higher.Active = true;
	}
	void on_spinbutton_encoder_power_lower_value_changed (object o, EventArgs args) {
		checkbutton_encoder_power_lower.Active = true;
	}

	void on_spinbutton_encoder_peakpower_higher_value_changed (object o, EventArgs args) {
		checkbutton_encoder_peakpower_higher.Active = true;
	}
	void on_spinbutton_encoder_peakpower_lower_value_changed (object o, EventArgs args) {
		checkbutton_encoder_peakpower_lower.Active = true;
	}

	private double getBestSetValue (BestSetValueEnum b)
	{
		if(b == BestSetValueEnum.AUTOMATIC_FEEDBACK)
			return bestSetValueAutomaticFeedback;
		else	// b == BestSetValueEnum.CAPTURE_MAIN_VARIABLE
			return bestSetValueCaptureMainVariable;
	}

	public void ResetBestSetValue (BestSetValueEnum b)
	{
		if(b == BestSetValueEnum.AUTOMATIC_FEEDBACK)
			bestSetValueAutomaticFeedback = 0;
		else	// b == BestSetValueEnum.CAPTURE_MAIN_VARIABLE
			bestSetValueCaptureMainVariable = 0;
	}

	public void UpdateBestSetValue (EncoderCurve curve)
	{
		BestSetValueEnum b = BestSetValueEnum.AUTOMATIC_FEEDBACK;
		string encoderVar = GetMainVariable;
		if(EncoderAutomaticHigherActive || EncoderAutomaticLowerActive)
		{
			if(encoderVar == Constants.MeanSpeed)
				UpdateBestSetValue(b, curve.MeanSpeedD);
			else if(encoderVar == Constants.MaxSpeed)
				UpdateBestSetValue(b, curve.MaxSpeedD);
			else if(encoderVar == Constants.MeanPower)
				UpdateBestSetValue(b, curve.MeanPowerD);
			else if(encoderVar == Constants.PeakPower)
				UpdateBestSetValue(b, curve.PeakPowerD);
			else if(encoderVar == Constants.MeanForce)
				UpdateBestSetValue(b, curve.MeanForceD);
			else if(encoderVar == Constants.MaxForce)
				UpdateBestSetValue(b, curve.MaxForceD);
		}
	}
	public void UpdateBestSetValue(BestSetValueEnum b, double d)
	{
		if(b == BestSetValueEnum.AUTOMATIC_FEEDBACK)
		{
			if(d > bestSetValueAutomaticFeedback)
				bestSetValueAutomaticFeedback = d;
		} else
		{ 	// b == BestSetValueEnum.CAPTURE_MAIN_VARIABLE
			if(d > bestSetValueCaptureMainVariable)
				bestSetValueCaptureMainVariable = d;
		}
	}

	//called from gui/encoderTreeviews.cs
	public string AssignColorAutomatic(BestSetValueEnum b, EncoderCurve curve, string variable, Preferences.EncoderPhasesEnum phaseEnum)
	{
		if(GetMainVariable != variable)
			return UtilGtk.ColorNothing;

		double currentValue = curve.GetParameter(variable);

		return AssignColorAutomatic(b, currentValue, phaseEnum);
	}
	//called from previous function, gui/encoder.cs plotCurvesGraphDoPlot
	public string AssignColorAutomatic(BestSetValueEnum b, double currentValue, Preferences.EncoderPhasesEnum phaseEnum)
	{
		if(radio_encoder_eccon_ecc.Active && phaseEnum != Preferences.EncoderPhasesEnum.ECC)
			return UtilGtk.ColorNothing;
		else if(radio_encoder_eccon_con.Active && phaseEnum != Preferences.EncoderPhasesEnum.CON)
			return UtilGtk.ColorNothing;

		if(EncoderAutomaticHigherActive && currentValue > getBestSetValue(b) * EncoderAutomaticHigherValue / 100)
			return UtilGtk.ColorGood;
		else if (EncoderAutomaticLowerActive && currentValue < getBestSetValue(b) * EncoderAutomaticLowerValue/ 100)
			return UtilGtk.ColorBad;

		return UtilGtk.ColorNothing;
	}

	//encoder rhythm
	private void on_check_rhythm_active_toggled (object o, EventArgs args)
	{
		frame_rhythm.Visible = check_rhythm_active.Active;
	}

	private void on_radio_rhythm_together_toggled (object o, EventArgs args)
	{
		if(radio_rhythm_together.Active)
			notebook_duration_repetition.CurrentPage = 0;
		else
			notebook_duration_repetition.CurrentPage = 1;

		should_show_vbox_rhythm_rest_after();
	}

	private void on_spin_rhythm_phases_values_changed (object o, EventArgs args)
	{
		label_rhythm_ecc_plus_con.Text = Util.TrimDecimals(spin_rhythm_ecc.Value + spin_rhythm_con.Value, 2);
	}

	private void should_show_vbox_rhythm_rest_after()
	{
		vbox_rhythm_rest_after.Visible = ( check_rhythm_use_clusters.Active ||
				( check_rhythm_rest_reps.Active && radio_rhythm_separated.Active ) );
	}

	private void on_check_rhythm_rest_reps_toggled (object o, EventArgs args)
	{
		if(check_rhythm_rest_reps.Active)
			hbox_rhythm_rest_reps_value.Visible = true;
		else
			hbox_rhythm_rest_reps_value.Visible = false;

		should_show_vbox_rhythm_rest_after();
	}

	private void on_check_rhythm_use_clusters_toggled (object o, EventArgs args)
	{
		vbox_rhythm_cluster.Visible = check_rhythm_use_clusters.Active;
		should_show_vbox_rhythm_rest_after();
	}

	private void on_button_use_clusters_help_clicked (object o, EventArgs args)
	{
		new DialogMessage(
				Catalog.GetString("Use clusters"),
				Constants.MessageTypes.INFO,
				Catalog.GetString("Use clusters in order to group repetitions inside a set separated by rest time.")
				+ "\n\n" +
				Catalog.GetString("Set will end when user press Finish or Cancel or when there's no change in the encoder during this time:\n" +
					"1.5 * Rest between clusters") );
	}


	private void on_button_rhythm_default_clicked (object o, EventArgs args)
	{
		//have default values
		EncoderRhythm encoderRhythm = new EncoderRhythm();
		//But have rhythm active
		encoderRhythm.Active = true;

		//modify widgets
		encoder_rhythm_set_values(encoderRhythm);
	}

	private void encoder_rhythm_set_values(EncoderRhythm encoderRhythm)
	{
		check_rhythm_active.Active = encoderRhythm.Active;

		/*
		if(encoderRhythm.RepsOrPhases)
			radio_rhythm_together.Active = true;
		else
			radio_rhythm_separated.Active = true;
			*/
		//just before 1.8.0 always use separated
		radio_rhythm_separated.Active = true;

		//spin_rhythm_rep.Value = encoderRhythm.RepSeconds;
		spin_rhythm_ecc.Value = encoderRhythm.EccSeconds;
		spin_rhythm_con.Value = encoderRhythm.ConSeconds;
		spin_rhythm_rest_reps.Value = encoderRhythm.RestRepsSeconds;
		spin_rhythm_reps_cluster.Value = encoderRhythm.RepsCluster;
		spin_rhythm_rest_clusters.Value = encoderRhythm.RestClustersSeconds;

		frame_rhythm.Visible = check_rhythm_active.Active;

		if(encoderRhythm.RepsOrPhases)
			notebook_duration_repetition.CurrentPage = 0;
		else
			notebook_duration_repetition.CurrentPage = 1;

		if(encoderRhythm.RestRepsSeconds < 0.1)
			check_rhythm_rest_reps.Active = false;

		if(encoderRhythm.UseClusters()) {
			check_rhythm_use_clusters.Active = true;
			vbox_rhythm_cluster.Visible = true;
		} else {
			check_rhythm_use_clusters.Active = false;
			vbox_rhythm_cluster.Visible = false;
		}

		should_show_vbox_rhythm_rest_after();
	}

	public EncoderRhythm Encoder_rhythm_get_values()
	{
		int reps_cluster = Convert.ToInt32(spin_rhythm_reps_cluster.Value);
		if(! check_rhythm_use_clusters.Active && reps_cluster > 1)
			reps_cluster = 1;

		//avoid problems like having spin values of: 1.38777878078145E-16 (true story)
		double restReps = spin_rhythm_rest_reps.Value;
		if(restReps < 0.1 || ! check_rhythm_rest_reps.Active)
			restReps = 0;
		double restClusters = spin_rhythm_rest_clusters.Value;
		if(restClusters < 0.1)
			restClusters = 0;

		return new EncoderRhythm(
				check_rhythm_active.Active, radio_rhythm_together.Active,
				spin_rhythm_rep.Value, spin_rhythm_ecc.Value, spin_rhythm_con.Value,
				restReps, radio_rest_after_ecc.Active,
				reps_cluster, restClusters);
	}

	/* FORCESENSOR */

	private void on_check_force_sensor_capture_feedback_toggled (object o, EventArgs args)
	{
		hbox_force_sensor_capture_feedback.Sensitive = check_force_sensor_capture_feedback.Active;
	}

	public bool GetForceSensorFeedbackActive {
		get { return check_force_sensor_capture_feedback.Active; }
	}
	public int GetForceSensorFeedbackAt {
		get { return Convert.ToInt32(spin_force_sensor_capture_feedback_at.Value); }
	}
	public int GetForceSensorFeedbackRange {
		get { return Convert.ToInt32(spin_force_sensor_capture_feedback_range.Value); }
	}


	/* JUMPS */
	public bool TfTcBest {
		get { return checkbutton_jump_tf_tc_best.Active; }
	}
	public bool TfTcWorst {
		get { return checkbutton_jump_tf_tc_worst.Active; }
	}

	public bool HeightGreater {
		get { return checkbutton_height_greater.Active; }
	}
	public bool HeightLower {
		get { return checkbutton_height_lower.Active; }
	}

	public bool TfGreater {
		get { return checkbutton_tf_greater.Active; }
	}
	public bool TfLower {
		get { return checkbutton_tf_lower.Active; }
	}

	public bool TcGreater {
		get { return checkbutton_tc_greater.Active; }
	}
	public bool TcLower {
		get { return checkbutton_tc_lower.Active; }
	}

	public bool TfTcGreater {
		get { return checkbutton_tf_tc_greater.Active; }
	}
	public bool TfTcLower {
		get { return checkbutton_tf_tc_lower.Active; }
	}

	public double HeightGreaterValue {
		get { return Convert.ToDouble(spinbutton_height_greater.Value); }
	}
	public double HeightLowerValue {
		get { return Convert.ToDouble(spinbutton_height_lower.Value); }
	}

	public double TfGreaterValue {
		get { return Convert.ToDouble(spinbutton_tf_greater.Value); }
	}
	public double TfLowerValue {
		get { return Convert.ToDouble(spinbutton_tf_lower.Value); }
	}

	public double TcGreaterValue {
		get { return Convert.ToDouble(spinbutton_tc_greater.Value); }
	}
	public double TcLowerValue {
		get { return Convert.ToDouble(spinbutton_tc_lower.Value); }
	}

	public double TfTcGreaterValue {
		get { return Convert.ToDouble(spinbutton_tf_tc_greater.Value); }
	}
	public double TfTcLowerValue {
		get { return Convert.ToDouble(spinbutton_tf_tc_lower.Value); }
	}

	/* RUNS */
	public bool RunTimeBest {
		get { return checkbutton_run_time_best.Active; }
	}
	public bool RunTimeWorst {
		get { return checkbutton_run_time_worst.Active; }
	}

	public bool RunTimeGreater {
		get { return checkbutton_time_greater.Active; }
	}
	public bool RunTimeLower {
		get { return checkbutton_time_lower.Active; }
	}

	public double RunTimeGreaterValue {
		get { return Convert.ToDouble(spinbutton_time_greater.Value); }
	}
	public double RunTimeLowerValue {
		get { return Convert.ToDouble(spinbutton_time_lower.Value); }
	}

	/* ENCODER */
	//automatic

	public bool EncoderAutomaticHigherActive {
		get { return checkbutton_encoder_automatic_greater.Active; }
	}
	public int EncoderAutomaticHigherValue {
		get { return Convert.ToInt32(spinbutton_encoder_automatic_greater.Value); }
	}
	public bool EncoderAutomaticLowerActive {
		get { return checkbutton_encoder_automatic_lower.Active; }
	}
	public int EncoderAutomaticLowerValue {
		get { return Convert.ToInt32(spinbutton_encoder_automatic_lower.Value); }
	}

	public bool EncoderRelativeToSet {
		get { return radio_encoder_relative_to_set.Active; }
	}

	public string GetMainVariable {
		get { return UtilGtk.ComboGetActive(combo_encoder_main_variable); }
	}
	public string GetSecondaryVariable {
		get { return UtilGtk.ComboGetActive(combo_encoder_secondary_variable); }
	}
	public bool GetSecondaryVariableShow {
		get { return check_encoder_show_secondary_variable.Active; }
	}

	public Preferences.EncoderPhasesEnum GetEncoderCaptureFeedbackEccon {
		get {
			if(radio_encoder_eccon_ecc.Active)
				return Preferences.EncoderPhasesEnum.ECC;
			else if(radio_encoder_eccon_con.Active)
				return Preferences.EncoderPhasesEnum.CON;
			else
				return Preferences.EncoderPhasesEnum.BOTH;
		}
	}

	public Preferences.encoderCaptureEccOverloadModes GetEncoderCaptureEccOverloadMode {
		get {
			if(check_encoder_inertial_ecc_overload_percent.Active)
				return Preferences.encoderCaptureEccOverloadModes.SHOW_LINE_AND_PERCENT;
			else if (check_encoder_inertial_ecc_overload.Active)
				return Preferences.encoderCaptureEccOverloadModes.SHOW_LINE;
			else
				return Preferences.encoderCaptureEccOverloadModes.NOT_SHOW;
		}
	}

	public double GetMainVariableHigher(string mainVariable) 
	{
		if(mainVariable == Constants.MeanSpeed && EncoderMeanSpeedHigher)
			return EncoderMeanSpeedHigherValue;
		else if(mainVariable == Constants.MaxSpeed && EncoderMaxSpeedHigher)
			return EncoderMaxSpeedHigherValue;
		else if(mainVariable == Constants.MeanForce && EncoderMeanForceHigher)
			return EncoderMeanForceHigherValue;
		else if(mainVariable == Constants.MaxForce && EncoderMaxForceHigher)
			return EncoderMaxForceHigherValue;
		else if(mainVariable == Constants.MeanPower && EncoderPowerHigher)
			return EncoderPowerHigherValue;
		else if(mainVariable == Constants.PeakPower && EncoderPeakPowerHigher)
			return EncoderPeakPowerHigherValue;

		return -1;
	}

	public double GetMainVariableLower(string mainVariable) 
	{
		if(mainVariable == Constants.MeanSpeed && EncoderMeanSpeedLower)
			return EncoderMeanSpeedLowerValue;
		else if(mainVariable == Constants.MaxSpeed && EncoderMaxSpeedLower)
			return EncoderMaxSpeedLowerValue;
		else if(mainVariable == Constants.MeanForce && EncoderMeanForceLower)
			return EncoderMeanForceLowerValue;
		else if(mainVariable == Constants.MaxForce && EncoderMaxForceLower)
			return EncoderMaxForceLowerValue;
		else if(mainVariable == Constants.MeanPower && EncoderPowerLower)
			return EncoderPowerLowerValue;
		else if(mainVariable == Constants.PeakPower && EncoderPeakPowerLower)
			return EncoderPeakPowerLowerValue;
			
		return -1;
	}

	public int Notebook_encoder_conditions_page {
		set { notebook_encoder_conditions.CurrentPage = value; }
	}

	public bool Encoder_show_manual_feedback {
		set { checkbutton_encoder_show_manual_feedback.Active = value; }
	}

	//height
	public bool EncoderHeightHigher {
		get { return checkbutton_encoder_height_higher.Active; }
	}
	public double EncoderHeightHigherValue {
		get { return Convert.ToDouble(spinbutton_encoder_height_higher.Value); }
	}
	
	public bool EncoderHeightLower {
		get { return checkbutton_encoder_height_lower.Active; }
	}
	public double EncoderHeightLowerValue {
		get { return Convert.ToDouble(spinbutton_encoder_height_lower.Value); }
	}

	//speed
	public bool EncoderMeanSpeedHigher {
		get { return checkbutton_encoder_mean_speed_higher.Active; }
		set { checkbutton_encoder_mean_speed_higher.Active = value; } //used on Compujump
	}
	public double EncoderMeanSpeedHigherValue {
		get { return Convert.ToDouble(spinbutton_encoder_mean_speed_higher.Value); }
		set { spinbutton_encoder_mean_speed_higher.Value = value; } //used on Compujump
	}
	
	public bool EncoderMeanSpeedLower {
		get { return checkbutton_encoder_mean_speed_lower.Active; }
		set { checkbutton_encoder_mean_speed_lower.Active = value; } //used on Compujump
	}
	public double EncoderMeanSpeedLowerValue {
		get { return Convert.ToDouble(spinbutton_encoder_mean_speed_lower.Value); }
		set { spinbutton_encoder_mean_speed_lower.Value = value; } //used on Compujump
	}

	public bool EncoderMaxSpeedHigher {
		get { return checkbutton_encoder_max_speed_higher.Active; }
	}
	public double EncoderMaxSpeedHigherValue {
		get { return Convert.ToDouble(spinbutton_encoder_max_speed_higher.Value); }
	}
	
	public bool EncoderMaxSpeedLower {
		get { return checkbutton_encoder_max_speed_lower.Active; }
	}
	public double EncoderMaxSpeedLowerValue {
		get { return Convert.ToDouble(spinbutton_encoder_max_speed_lower.Value); }
	}

	//force
	public bool EncoderMeanForceHigher {
		get { return checkbutton_encoder_mean_force_higher.Active; }
	}
	public double EncoderMeanForceHigherValue {
		get { return Convert.ToDouble(spinbutton_encoder_mean_force_higher.Value); }
	}
	
	public bool EncoderMeanForceLower {
		get { return checkbutton_encoder_mean_force_lower.Active; }
	}
	public double EncoderMeanForceLowerValue {
		get { return Convert.ToDouble(spinbutton_encoder_mean_force_lower.Value); }
	}

	public bool EncoderMaxForceHigher {
		get { return checkbutton_encoder_max_force_higher.Active; }
	}
	public double EncoderMaxForceHigherValue {
		get { return Convert.ToDouble(spinbutton_encoder_max_force_higher.Value); }
	}
	
	public bool EncoderMaxForceLower {
		get { return checkbutton_encoder_max_force_lower.Active; }
	}
	public double EncoderMaxForceLowerValue {
		get { return Convert.ToDouble(spinbutton_encoder_max_force_lower.Value); }
	}

	//power & peakPower
	public bool EncoderPowerHigher {
		get { return checkbutton_encoder_power_higher.Active; }
	}
	public int EncoderPowerHigherValue {
		get { return Convert.ToInt32(spinbutton_encoder_power_higher.Value); }
	}
	
	public bool EncoderPowerLower {
		get { return checkbutton_encoder_power_lower.Active; }
	}
	public int EncoderPowerLowerValue {
		get { return Convert.ToInt32(spinbutton_encoder_power_lower.Value); }
	}

	public bool EncoderPeakPowerHigher {
		get { return checkbutton_encoder_peakpower_higher.Active; }
	}
	public int EncoderPeakPowerHigherValue {
		get { return Convert.ToInt32(spinbutton_encoder_peakpower_higher.Value); }
	}

	public bool EncoderPeakPowerLower {
		get { return checkbutton_encoder_peakpower_lower.Active; }
	}
	public int EncoderPeakPowerLowerValue {
		get { return Convert.ToInt32(spinbutton_encoder_peakpower_lower.Value); }
	}
}

