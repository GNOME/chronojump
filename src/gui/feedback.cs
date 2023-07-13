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
using Gdk;
//using Glade;
using Mono.Unix;
using System.IO; 		//Path

public class FeedbackWindow 
{
	// at glade ---->
	Gtk.Window feedback;
	Gtk.Notebook notebook_main;
	//Gtk.ScrolledWindow scrolled_conditions;

	Gtk.Box hbox_jump_best_worst;
	Gtk.Grid grid_run_best_worst;
	
	/* jumps */	
	Gtk.Box hbox_jump_conditions;
	Gtk.CheckButton checkbutton_jump_tf_tc_best;
	Gtk.CheckButton checkbutton_jump_tf_tc_worst;

	Gtk.CheckButton checkbutton_height_greater;
	Gtk.CheckButton checkbutton_height_lower;
	Gtk.CheckButton checkbutton_tf_greater;
	Gtk.CheckButton checkbutton_tf_lower;
	Gtk.CheckButton checkbutton_tc_greater;
	Gtk.CheckButton checkbutton_tc_lower;
	Gtk.CheckButton checkbutton_tf_tc_greater;
	Gtk.CheckButton checkbutton_tf_tc_lower;
	
	//Gtk.SpinButton spinbutton_height_greater;
	//Gtk.SpinButton spinbutton_height_lower;
	Gtk.SpinButton spinbutton_tf_greater;
	Gtk.SpinButton spinbutton_tf_lower;
	Gtk.SpinButton spinbutton_tc_greater;
	Gtk.SpinButton spinbutton_tc_lower;
	//Gtk.SpinButton spinbutton_tf_tc_greater;
	//Gtk.SpinButton spinbutton_tf_tc_lower;

	/* runs */	
	Gtk.Box hbox_run_conditions_speed;
	Gtk.Box hbox_run_conditions_time;
	Gtk.CheckButton checkbutton_run_speed_best;
	Gtk.CheckButton checkbutton_run_speed_worst;
	Gtk.CheckButton checkbutton_run_time_best;
	Gtk.CheckButton checkbutton_run_time_worst;
	
	Gtk.CheckButton checkbutton_speed_greater;
	Gtk.CheckButton checkbutton_speed_lower;
	Gtk.CheckButton checkbutton_time_greater;
	Gtk.CheckButton checkbutton_time_lower;

	Gtk.SpinButton spinbutton_speed_greater;
	Gtk.SpinButton spinbutton_speed_lower;
	Gtk.SpinButton spinbutton_time_greater;
	Gtk.SpinButton spinbutton_time_lower;

	/* encoder */
	Gtk.HBox hbox_combo_encoder_main_variable;
	Gtk.RadioButton radio_encoder_relative_to_set;
	Gtk.RadioButton radio_encoder_relative_to_historical;
	Gtk.CheckButton checkbutton_encoder_automatic_greater;
	Gtk.CheckButton checkbutton_encoder_automatic_lower;
	Gtk.SpinButton spinbutton_encoder_automatic_greater;
	Gtk.SpinButton spinbutton_encoder_automatic_lower;
	Gtk.CheckButton check_encoder_show_secondary_variable;
	Gtk.HBox hbox_combo_encoder_secondary_variable;
	Gtk.RadioButton radio_encoder_eccon_both;
	Gtk.RadioButton radio_encoder_eccon_ecc;
	Gtk.RadioButton radio_encoder_eccon_con;
	Gtk.CheckButton check_encoder_inertial_ecc_overload;
	Gtk.CheckButton check_encoder_inertial_ecc_overload_percent;
	Gtk.CheckButton check_encoder_show_loss;

	Gtk.Notebook notebook_encoder_conditions;
	Gtk.CheckButton checkbutton_encoder_height_higher;
	Gtk.CheckButton checkbutton_encoder_height_lower;
	Gtk.CheckButton checkbutton_encoder_mean_speed_higher;
	Gtk.CheckButton checkbutton_encoder_max_speed_higher;
	Gtk.CheckButton checkbutton_encoder_mean_speed_lower;
	Gtk.CheckButton checkbutton_encoder_max_speed_lower;
	Gtk.CheckButton checkbutton_encoder_mean_force_higher;
	Gtk.CheckButton checkbutton_encoder_max_force_higher;
	Gtk.CheckButton checkbutton_encoder_mean_force_lower;
	Gtk.CheckButton checkbutton_encoder_max_force_lower;
	Gtk.CheckButton checkbutton_encoder_power_higher;
	Gtk.CheckButton checkbutton_encoder_peakpower_higher;
	Gtk.CheckButton checkbutton_encoder_power_lower;
	Gtk.CheckButton checkbutton_encoder_peakpower_lower;
	Gtk.SpinButton spinbutton_encoder_height_higher;
	Gtk.SpinButton spinbutton_encoder_height_lower;
	Gtk.SpinButton spinbutton_encoder_mean_speed_higher;
	Gtk.SpinButton spinbutton_encoder_max_speed_higher;
	Gtk.SpinButton spinbutton_encoder_mean_speed_lower;
	Gtk.SpinButton spinbutton_encoder_max_speed_lower;
	Gtk.SpinButton spinbutton_encoder_mean_force_higher;
	Gtk.SpinButton spinbutton_encoder_max_force_higher;
	Gtk.SpinButton spinbutton_encoder_mean_force_lower;
	Gtk.SpinButton spinbutton_encoder_max_force_lower;
	Gtk.SpinButton spinbutton_encoder_power_higher;
	Gtk.SpinButton spinbutton_encoder_peakpower_higher;
	Gtk.SpinButton spinbutton_encoder_power_lower;
	Gtk.SpinButton spinbutton_encoder_peakpower_lower;

	Gtk.CheckButton check_encoder_show_asteroids;

	Gtk.Button button_test_good;
	//Gtk.Button button_test_bad;
	Gtk.Label label_test_sound_result;
	Gtk.Button button_close;

	//bells good (green)
	Gtk.Image image_repetitive_best_tf_tc;
	Gtk.Image image_repetitive_best_speed;
	Gtk.Image image_repetitive_best_time;
	Gtk.Image image_repetitive_height_greater;
	Gtk.Image image_repetitive_tf_greater;
	Gtk.Image image_repetitive_tc_lower;
	Gtk.Image image_repetitive_tf_tc_greater;
	Gtk.Image image_repetitive_speed_lower;
	Gtk.Image image_repetitive_time_lower;
	Gtk.Image image_repetitive_encoder_automatic_greater;
	Gtk.Image image_encoder_height_higher;
	Gtk.Image image_encoder_mean_speed_higher;
	Gtk.Image image_encoder_max_speed_higher;
	Gtk.Image image_encoder_mean_force_higher;
	Gtk.Image image_encoder_max_force_higher;
	Gtk.Image image_encoder_power_higher;
	Gtk.Image image_encoder_peakpower_higher;
	Gtk.Image image_repetitive_test_good;
	//bells bad (red)
	Gtk.Image image_repetitive_worst_tf_tc;
	Gtk.Image image_repetitive_worst_speed;
	Gtk.Image image_repetitive_worst_time;
	Gtk.Image image_repetitive_height_lower;
	Gtk.Image image_repetitive_tf_lower;
	Gtk.Image image_repetitive_tc_greater;
	Gtk.Image image_repetitive_tf_tc_lower;
	Gtk.Image image_repetitive_speed_greater;
	Gtk.Image image_repetitive_time_greater;
	Gtk.Image image_repetitive_encoder_automatic_lower;
	Gtk.Image image_encoder_height_lower;
	Gtk.Image image_encoder_mean_speed_lower;
	Gtk.Image image_encoder_max_speed_lower;
	Gtk.Image image_encoder_mean_force_lower;
	Gtk.Image image_encoder_max_force_lower;
	Gtk.Image image_encoder_power_lower;
	Gtk.Image image_encoder_peakpower_lower;
	Gtk.Image image_repetitive_test_bad;

	//encoder rhythm
	Gtk.Label label_rhythm_tab;
	Gtk.CheckButton check_rhythm_active;
	Gtk.RadioButton radio_rhythm_together;
	Gtk.RadioButton radio_rhythm_separated;
	Gtk.Notebook notebook_duration_repetition;
	Gtk.Frame frame_clusters;
	Gtk.Frame frame_rhythm;
	Gtk.Box box_check_rhythm_use_clusters;
	Gtk.CheckButton check_rhythm_use_clusters;
	Gtk.SpinButton spin_rhythm_rep;
	Gtk.SpinButton spin_rhythm_ecc;
	Gtk.SpinButton spin_rhythm_con;
	Gtk.Label label_rhythm_ecc_plus_con;
	Gtk.SpinButton spin_rhythm_rest_reps;
	Gtk.VBox vbox_rhythm_rest_after;
	Gtk.RadioButton radio_rest_after_ecc;
	Gtk.SpinButton spin_rhythm_reps_cluster;
	Gtk.SpinButton spin_rhythm_rest_clusters;
	Gtk.Image image_clusters_info;
	Gtk.HBox hbox_rhythm_rest_reps_value;
	Gtk.CheckButton check_rhythm_rest_reps;

	//signal
	//forceSensor
	Gtk.Box box_forceSensor_feedback;
	Gtk.Notebook notebook_capture_feedback;
	Gtk.Box box_force_sensor_capture_feedback_show;
	Gtk.CheckButton check_force_sensor_capture_feedback_no;
	Gtk.RadioButton radio_force_sensor_capture_feedback_show_rectangle;
	Gtk.RadioButton radio_force_sensor_capture_feedback_show_path;
	Gtk.RadioButton radio_force_sensor_capture_feedback_show_asteroids;
	Gtk.RadioButton radio_force_sensor_capture_feedback_show_questionnaire;
	//rectangle
	Gtk.SpinButton spin_force_sensor_capture_feedback_rectangle_at;
	Gtk.SpinButton spin_force_sensor_capture_feedback_rectangle_range;
	//path
	Gtk.SpinButton spin_force_sensor_capture_feedback_path_max;
	Gtk.SpinButton spin_force_sensor_capture_feedback_path_min;
	Gtk.SpinButton spin_force_sensor_capture_feedback_path_masters;
	Gtk.SpinButton spin_force_sensor_capture_feedback_path_master_seconds;
	Gtk.SpinButton spin_force_sensor_capture_feedback_path_line_width; //N
	Gtk.Label label_force_sensor_path_recommended;
	//asteroids
	Gtk.SpinButton spin_force_sensor_capture_feedback_asteroids_max;
	Gtk.SpinButton spin_force_sensor_capture_feedback_asteroids_min;
	Gtk.CheckButton check_force_sensor_capture_feedback_asteroids_dark;
	Gtk.SpinButton spin_force_sensor_capture_feedback_asteroids_frequency;
	Gtk.SpinButton spin_force_sensor_capture_feedback_shots_frequency;
	Gtk.Label label_feedback_asteroids_min_units;
	Gtk.Label label_feedback_asteroids_max_units;
	//questionnaire
	Gtk.SpinButton spin_force_sensor_capture_feedback_questionnaire_max;
	Gtk.SpinButton spin_force_sensor_capture_feedback_questionnaire_min;
	Gtk.SpinButton spin_force_sensor_capture_feedback_questionnaire_n;
	Gtk.RadioButton radio_force_sensor_capture_feedback_questionnaire_default;
	Gtk.RadioButton radio_force_sensor_capture_feedback_questionnaire_load;
	Gtk.Image image_force_sensor_capture_feedback_questionnaire_load_info;
	Gtk.Box buttons_force_sensor_capture_feedback_questionnaire_load;
	Gtk.Label label_force_sensor_capture_feedback_questionnaire_load_success;
	//direction
	Gtk.RadioButton radio_signal_direction_horizontal;
	Gtk.RadioButton radio_signal_direction_vertical;
	Gtk.Box box_radio_signal_direction;
	Gtk.Label label_signal_direction_horizontal;

	//runsEncoder
	Gtk.RadioButton radio_run_encoder_power;
	Gtk.RadioButton radio_run_encoder_force;
	//Gtk.RadioButton radio_run_encoder_accel;
	// <---- at glade


	Gtk.ComboBoxText combo_encoder_main_variable;
	Gtk.ComboBoxText combo_encoder_secondary_variable;


	const int JUMPSRUNSPAGE = 0;
	const int ENCODERAUTOPAGE = 1;
	const int ENCODERMANUALPAGE = 2;
	const int ENCODERRHYTHMPAGE = 3;
	const int SIGNALPAGE = 4;
	const int TESTBELLSPAGE = 5;
	const int RUNSENCODERPAGE = 6;

	public Gtk.Button FakeButtonClose;
	public Gtk.Button FakeButtonQuestionnaireLoad;

	//static bool volumeOn;
	bool volumeOn;
	public Preferences.GstreamerTypes gstreamer;

	public enum BestSetValueEnum { CAPTURE_MAIN_VARIABLE, AUTOMATIC_FEEDBACK}
	private double bestSetValueCaptureMainVariable;
	private double bestSetValueAutomaticFeedback;
	private bool update_checkbuttons_encoder_automatic;
	private string forceSensorFeedbackQuestionnaireFile;
	
	static FeedbackWindow FeedbackWindowBox;
		
	FeedbackWindow ()
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "feedback.glade", "feedback", "chronojump");
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "feedback.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);
		
		//don't show until View is called
		feedback.Hide ();

		//put an icon to window
		UtilGtk.IconWindow(feedback);
		
		FakeButtonClose = new Gtk.Button();
		FakeButtonQuestionnaireLoad = new Gtk.Button();
		
		//createComboEncoderAutomaticVariable();
		createComboEncoderMainAndSecondaryVariables();

		bestSetValueCaptureMainVariable = 0;
		bestSetValueCaptureMainVariable = 0;
		notebook_encoder_conditions.CurrentPage = 3; //power

		putNonStandardIcons();

		label_rhythm_tab.Text = Catalog.GetString("Rhythm") + " / " + Catalog.GetString("Protocol");
	}

	static public FeedbackWindow Create ()
	{
		if (FeedbackWindowBox == null) {
			FeedbackWindowBox = new FeedbackWindow (); 
		}
	
		//don't show until View is called
		//FeedbackWindowBox.feedback.Hide ();
		
		return FeedbackWindowBox;
	}

	public void View (Constants.BellModes bellMode, Preferences preferences, EncoderRhythm encoderRhythm, bool viewWindow)
	{
		//when user "deleted_event" the window
		if (FeedbackWindowBox == null) {
			FeedbackWindowBox = new FeedbackWindow (); 
		}
		FeedbackWindowBox.update_checkbuttons_encoder_automatic = true;
		FeedbackWindowBox.showWidgets (bellMode,
				preferences.jumpsRjFeedbackShowBestTvTc,
				preferences.jumpsRjFeedbackShowWorstTvTc,
				preferences.jumpsRjFeedbackTvGreaterActive,
				preferences.jumpsRjFeedbackTvLowerActive,
				preferences.jumpsRjFeedbackTcGreaterActive,
				preferences.jumpsRjFeedbackTcLowerActive,
				preferences.jumpsRjFeedbackTvGreater,
				preferences.jumpsRjFeedbackTvLower,
				preferences.jumpsRjFeedbackTcGreater,
				preferences.jumpsRjFeedbackTcLower,
				preferences.runsIFeedbackShowBestSpeed,
				preferences.runsIFeedbackShowWorstSpeed,
				preferences.runsIFeedbackShowBest,
				preferences.runsIFeedbackShowWorst,
				preferences.runsIFeedbackTimeGreaterActive,
				preferences.runsIFeedbackTimeLowerActive,
				preferences.runsIFeedbackSpeedGreaterActive,
				preferences.runsIFeedbackSpeedLowerActive,
				preferences.runsIFeedbackTimeGreater,
				preferences.runsIFeedbackTimeLower,
				preferences.runsIFeedbackSpeedGreater,
				preferences.runsIFeedbackSpeedLower,
				preferences.encoderCaptureMainVariable, preferences.encoderCaptureSecondaryVariable,
				preferences.encoderCaptureSecondaryVariableShow,
				preferences.encoderCaptureInertialEccOverloadMode,
				preferences.encoderCaptureMainVariableThisSetOrHistorical,
				preferences.encoderCaptureMainVariableGreaterActive,
				preferences.encoderCaptureMainVariableGreaterValue,
				preferences.encoderCaptureMainVariableLowerActive,
				preferences.encoderCaptureMainVariableLowerValue,
				preferences.encoderCaptureFeedbackEccon,
				preferences.encoderCaptureShowLoss,
				encoderRhythm,
				preferences.encoderFeedbackAsteroidsActive,
				preferences.forceSensorCaptureFeedbackActive,
				preferences.forceSensorCaptureFeedbackAt,
				preferences.forceSensorCaptureFeedbackRange,
				preferences.forceSensorFeedbackPathMax,
				preferences.forceSensorFeedbackPathMin,
				preferences.forceSensorFeedbackPathMasters,
				preferences.forceSensorFeedbackPathMasterSeconds,
				preferences.forceSensorFeedbackPathLineWidth,
				preferences.forceSensorFeedbackAsteroidsMax,
				preferences.forceSensorFeedbackAsteroidsMin,
				preferences.forceSensorFeedbackAsteroidsDark,
				preferences.forceSensorFeedbackAsteroidsFrequency,
				preferences.forceSensorFeedbackShotsFrequency,
				preferences.forceSensorFeedbackQuestionnaireMax,
				preferences.forceSensorFeedbackQuestionnaireMin,
				preferences.forceSensorFeedbackQuestionnaireN,
				preferences.signalDirectionHorizontal
				);

		if(viewWindow)
		{
			//manage window color
			if(! Config.UseSystemColor)
			{
				UtilGtk.WindowColor(FeedbackWindowBox.feedback, Config.ColorBackground);

				UtilGtk.WidgetColor (notebook_main, Config.ColorBackgroundShifted);
				UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundShiftedIsDark, notebook_main);

				UtilGtk.WidgetColor (notebook_encoder_conditions, Config.ColorBackgroundShifted);
				UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundShiftedIsDark, notebook_encoder_conditions);
			}

			FeedbackWindowBox.feedback.Show ();
		}

		FeedbackWindowBox.volumeOn = preferences.volumeOn;
		FeedbackWindowBox.gstreamer = preferences.gstreamer;
		FeedbackWindowBox.forceSensorFeedbackQuestionnaireFile = forceSensorFeedbackQuestionnaireFile;
	}

	void showWidgets (Constants.BellModes bellMode,
			bool jumpsRjFeedbackShowBestTvTc,
			bool jumpsRjFeedbackShowWorstTvTc,
			bool jumpsRjFeedbackTvGreaterActive,
			bool jumpsRjFeedbackTvLowerActive,
			bool jumpsRjFeedbackTcGreaterActive,
			bool jumpsRjFeedbackTcLowerActive,
			double jumpsRjFeedbackTvGreater,
			double jumpsRjFeedbackTvLower,
			double jumpsRjFeedbackTcGreater,
			double jumpsRjFeedbackTcLower,
			bool runsIFeedbackShowBestSpeed,
			bool runsIFeedbackShowWorstSpeed,
			bool runsIFeedbackShowBest,
			bool runsIFeedbackShowWorst,
			bool runsIFeedbackTimeGreaterActive,
			bool runsIFeedbackTimeLowerActive,
			bool runsIFeedbackSpeedGreaterActive,
			bool runsIFeedbackSpeedLowerActive,
			double runsIFeedbackTimeGreater,
			double runsIFeedbackTimeLower,
			double runsIFeedbackSpeedGreater,
			double runsIFeedbackSpeedLower,
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
			bool encoderCaptureShowLoss,
			EncoderRhythm encoderRhythm,
			bool encoderFeedbackAsteroidsActive,
			//bool forceSensorCaptureFeedbackActive,
			Preferences.ForceSensorCaptureFeedbackActiveEnum forceSensorCaptureFeedbackActive,
			int forceSensorCaptureFeedbackAt,
			int forceSensorCaptureFeedbackRange,
			int forceSensorFeedbackPathMax,
			int forceSensorFeedbackPathMin,
			int forceSensorFeedbackPathMasters,
			int forceSensorFeedbackPathMasterSeconds,
			int forceSensorFeedbackPathLineWidth,
			int forceSensorFeedbackAsteroidsMax,
			int forceSensorFeedbackAsteroidsMin,
			bool forceSensorFeedbackAsteroidsDark,
			int forceSensorFeedbackAsteroidsFrequency,
			int forceSensorFeedbackShotsFrequency,
			int forceSensorFeedbackQuestionnaireMax,
			int forceSensorFeedbackQuestionnaireMin,
			int forceSensorFeedbackQuestionnaireN,
			bool signalDirectionHorizontal
				)
	{
		hbox_jump_best_worst.Hide();
		grid_run_best_worst.Hide();
		hbox_jump_conditions.Hide();
		hbox_run_conditions_speed.Hide();
		hbox_run_conditions_time.Hide();

		notebook_main.GetNthPage(JUMPSRUNSPAGE).Hide();
		notebook_main.GetNthPage(ENCODERAUTOPAGE).Hide();
		notebook_main.GetNthPage(ENCODERMANUALPAGE).Hide();
		notebook_main.GetNthPage(ENCODERRHYTHMPAGE).Hide();
		notebook_main.GetNthPage(SIGNALPAGE).Hide();
		notebook_main.GetNthPage(TESTBELLSPAGE).Hide();
		notebook_main.GetNthPage(RUNSENCODERPAGE).Hide();
		notebook_main.ShowTabs = false;

		if(bellMode == Constants.BellModes.JUMPS || bellMode == Constants.BellModes.RUNS)
		{
			if(bellMode == Constants.BellModes.JUMPS)
			{
				hbox_jump_best_worst.Show();
				hbox_jump_conditions.Show();

				checkbutton_jump_tf_tc_best.Active = jumpsRjFeedbackShowBestTvTc;
				checkbutton_jump_tf_tc_worst.Active = jumpsRjFeedbackShowWorstTvTc;

				//1st the spinbuttons and then the checkbuttons because spinbutton changes make checkbuttons active
				spinbutton_tf_greater.Value = jumpsRjFeedbackTvGreater;
				spinbutton_tf_lower.Value = jumpsRjFeedbackTvLower;
				spinbutton_tc_greater.Value = jumpsRjFeedbackTcGreater;
				spinbutton_tc_lower.Value = jumpsRjFeedbackTcLower;

				checkbutton_tf_greater.Active = jumpsRjFeedbackTvGreaterActive;
				checkbutton_tf_lower.Active = jumpsRjFeedbackTvLowerActive;
				checkbutton_tc_greater.Active = jumpsRjFeedbackTcGreaterActive;
				checkbutton_tc_lower.Active = jumpsRjFeedbackTcLowerActive;
			}
			else if(bellMode == Constants.BellModes.RUNS)
			{
				grid_run_best_worst.Show();
				hbox_run_conditions_speed.Show();
				hbox_run_conditions_time.Show();

				checkbutton_run_speed_best.Active = runsIFeedbackShowBestSpeed; //speed
				checkbutton_run_speed_worst.Active = runsIFeedbackShowWorstSpeed; //speed
				checkbutton_run_time_best.Active = runsIFeedbackShowBest; //time
				checkbutton_run_time_worst.Active = runsIFeedbackShowWorst; //time
				//1st the spinbuttons and then the checkbuttons because spinbutton changes make checkbuttons active
				spinbutton_time_greater.Value =  runsIFeedbackTimeGreater;
				spinbutton_time_lower.Value =  runsIFeedbackTimeLower;
				spinbutton_speed_greater.Value =  runsIFeedbackSpeedGreater;
				spinbutton_speed_lower.Value =  runsIFeedbackSpeedLower;
				checkbutton_time_greater.Active =  runsIFeedbackTimeGreaterActive;
				checkbutton_time_lower.Active =  runsIFeedbackTimeLowerActive;
				checkbutton_speed_greater.Active =  runsIFeedbackSpeedGreaterActive;
				checkbutton_speed_lower.Active =  runsIFeedbackSpeedLowerActive;
			}

			notebook_main.GetNthPage(JUMPSRUNSPAGE).Show();
			notebook_main.GetNthPage(TESTBELLSPAGE).Show();
			notebook_main.CurrentPage = JUMPSRUNSPAGE;
			notebook_main.ShowTabs = true;
		}
		else if (bellMode == Constants.BellModes.ENCODERGRAVITATORY || bellMode == Constants.BellModes.ENCODERINERTIAL)
		{
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
			check_encoder_show_loss.Active = encoderCaptureShowLoss;

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
			{
				radio_encoder_relative_to_set.Active = true;

				if(encoderCaptureFeedbackEccon == Preferences.EncoderPhasesEnum.ECC)
					radio_encoder_eccon_ecc.Active = true;
				else if(encoderCaptureFeedbackEccon == Preferences.EncoderPhasesEnum.CON)
					radio_encoder_eccon_con.Active = true;
				else
					radio_encoder_eccon_both.Active = true;
			} else
			{
				radio_encoder_relative_to_historical.Active = true;

				radio_encoder_eccon_both.Active = true;
				radio_encoder_eccon_ecc.Sensitive = false;
				radio_encoder_eccon_con.Sensitive = false;
			}

			//the change on spinbuttons here will not have to provoque changes on checkbuttons
			//another solution is do 1st the spinbuttons and then the checkbuttons
			update_checkbuttons_encoder_automatic = false;
			checkbutton_encoder_automatic_greater.Active = encoderCaptureMainVariableGreaterActive;
			spinbutton_encoder_automatic_greater.Value = encoderCaptureMainVariableGreaterValue;
			checkbutton_encoder_automatic_lower.Active = encoderCaptureMainVariableLowerActive;
			spinbutton_encoder_automatic_lower.Value = encoderCaptureMainVariableLowerValue;
			update_checkbuttons_encoder_automatic = true;

			box_forceSensor_feedback.Visible = false;
			check_encoder_show_asteroids.Visible = true;
			check_encoder_show_asteroids.Active = encoderFeedbackAsteroidsActive;
			notebook_capture_feedback.Visible = encoderFeedbackAsteroidsActive;
			notebook_capture_feedback.Page = 2; //asteroids
			label_feedback_asteroids_min_units.Text = "cm";
			label_feedback_asteroids_max_units.Text = "cm";

			notebook_main.GetNthPage(ENCODERAUTOPAGE).Show();
			notebook_main.GetNthPage(ENCODERMANUALPAGE).Show();
			notebook_main.GetNthPage(ENCODERRHYTHMPAGE).Show();
			notebook_main.GetNthPage(SIGNALPAGE).Show();
			notebook_main.GetNthPage(TESTBELLSPAGE).Show();
			notebook_main.CurrentPage = ENCODERAUTOPAGE;
			notebook_main.ShowTabs = true;

			encoder_rhythm_set_values(encoderRhythm);
		}
		else if(bellMode == Constants.BellModes.FORCESENSOR)
		{
			if(forceSensorCaptureFeedbackActive == Preferences.ForceSensorCaptureFeedbackActiveEnum.NO)
			{
				check_force_sensor_capture_feedback_no.Active = true;
				box_force_sensor_capture_feedback_show.Visible = false;
				notebook_capture_feedback.Visible = false;
				box_radio_signal_direction.Visible = true;
				label_signal_direction_horizontal.Visible = false;
			}
			else if(forceSensorCaptureFeedbackActive == Preferences.ForceSensorCaptureFeedbackActiveEnum.RECTANGLE)
			{
				radio_force_sensor_capture_feedback_show_rectangle.Active = true;
				box_force_sensor_capture_feedback_show.Visible = true;
				notebook_capture_feedback.Visible = true;
				notebook_capture_feedback.Page = 0;
				box_radio_signal_direction.Visible = false;
				label_signal_direction_horizontal.Visible = true;
			}
			else if(forceSensorCaptureFeedbackActive == Preferences.ForceSensorCaptureFeedbackActiveEnum.PATH)
			{
				radio_force_sensor_capture_feedback_show_path.Active = true;
				box_force_sensor_capture_feedback_show.Visible = true;
				notebook_capture_feedback.Visible = true;
				notebook_capture_feedback.Page = 1;
				box_radio_signal_direction.Visible = false;
				label_signal_direction_horizontal.Visible = true;
			}
			else if(forceSensorCaptureFeedbackActive == Preferences.ForceSensorCaptureFeedbackActiveEnum.ASTEROIDS)
			{
				radio_force_sensor_capture_feedback_show_asteroids.Active = true;
				box_force_sensor_capture_feedback_show.Visible = true;
				notebook_capture_feedback.Visible = true;
				notebook_capture_feedback.Page = 2;
				box_radio_signal_direction.Visible = true;
				label_signal_direction_horizontal.Visible = false;
			}
			else //if(forceSensorCaptureFeedbackActive == Preferences.ForceSensorCaptureFeedbackActiveEnum.QUESTIONNAIRE)
			{
				radio_force_sensor_capture_feedback_show_questionnaire.Active = true;
				box_force_sensor_capture_feedback_show.Visible = true;
				notebook_capture_feedback.Visible = true;
				notebook_capture_feedback.Page = 3;
				box_radio_signal_direction.Visible = false;
				label_signal_direction_horizontal.Visible = true;
			}

			//rectangle widgets
			spin_force_sensor_capture_feedback_rectangle_at.Value = forceSensorCaptureFeedbackAt;
			spin_force_sensor_capture_feedback_rectangle_range.Value = forceSensorCaptureFeedbackRange;

			//path widgets
			spin_force_sensor_capture_feedback_path_max.Value = forceSensorFeedbackPathMax;
			spin_force_sensor_capture_feedback_path_min.Value = forceSensorFeedbackPathMin;
			spin_force_sensor_capture_feedback_path_masters.Value = forceSensorFeedbackPathMasters;
			spin_force_sensor_capture_feedback_path_master_seconds.Value = forceSensorFeedbackPathMasterSeconds;
			spin_force_sensor_capture_feedback_path_line_width.Value = forceSensorFeedbackPathLineWidth;
			setForceSensorPathRecommendedLabel();

			//asteroids widgets
			spin_force_sensor_capture_feedback_asteroids_max.Value = forceSensorFeedbackAsteroidsMax;
			spin_force_sensor_capture_feedback_asteroids_min.Value = forceSensorFeedbackAsteroidsMin;
			check_force_sensor_capture_feedback_asteroids_dark.Active = forceSensorFeedbackAsteroidsDark;
			spin_force_sensor_capture_feedback_asteroids_frequency.Value = forceSensorFeedbackAsteroidsFrequency;
			spin_force_sensor_capture_feedback_shots_frequency.Value = forceSensorFeedbackShotsFrequency;
			label_feedback_asteroids_min_units.Text = "N";
			label_feedback_asteroids_max_units.Text = "N";

			//questionnaire widgets
			spin_force_sensor_capture_feedback_questionnaire_max.Value = forceSensorFeedbackQuestionnaireMax;
			spin_force_sensor_capture_feedback_questionnaire_min.Value = forceSensorFeedbackQuestionnaireMin;
			spin_force_sensor_capture_feedback_questionnaire_n.Value = forceSensorFeedbackQuestionnaireN;
			if (forceSensorFeedbackQuestionnaireFile == null || forceSensorFeedbackQuestionnaireFile == "")
			{
				radio_force_sensor_capture_feedback_questionnaire_default.Active = true;
				buttons_force_sensor_capture_feedback_questionnaire_load.Sensitive = false;
			} else {
				radio_force_sensor_capture_feedback_questionnaire_load.Active = true;
				buttons_force_sensor_capture_feedback_questionnaire_load.Sensitive = true;
			}
			label_force_sensor_capture_feedback_questionnaire_load_success.Text = "";

			//direction
			if (signalDirectionHorizontal)
				radio_signal_direction_horizontal.Active = true;
			else
				radio_signal_direction_vertical.Active = true;

			box_forceSensor_feedback.Visible = true;
			check_encoder_show_asteroids.Visible = false;

			notebook_main.GetNthPage(SIGNALPAGE).Show();
		}
		else if(bellMode == Constants.BellModes.RUNSENCODER)
		{
			notebook_main.GetNthPage(RUNSENCODERPAGE).Show();
		}

		label_test_sound_result.Text = "";
	}
		
	private void createComboEncoderMainAndSecondaryVariables()
	{
		//1 mainVariable
		combo_encoder_main_variable = new ComboBoxText ();
		comboEncoderVariableFill(combo_encoder_main_variable);

		hbox_combo_encoder_main_variable.PackStart(combo_encoder_main_variable, false, false, 0);
		hbox_combo_encoder_main_variable.ShowAll();
		combo_encoder_main_variable.Sensitive = true;
		combo_encoder_main_variable.Changed += new EventHandler (on_combo_encoder_main_variable_changed);

		//1 secondaryVariable
		combo_encoder_secondary_variable = new ComboBoxText ();
		comboEncoderVariableFill(combo_encoder_secondary_variable);

		hbox_combo_encoder_secondary_variable.PackStart(combo_encoder_secondary_variable, false, false, 0);
		hbox_combo_encoder_secondary_variable.ShowAll();
		combo_encoder_secondary_variable.Sensitive = true;
		//combo_encoder_secondary_variable.Changed += new EventHandler (on_combo_encoder_secondary_variable_changed);
	}
	private void comboEncoderVariableFill(Gtk.ComboBoxText combo)
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

	private void on_check_encoder_show_asteroids_clicked (object o, EventArgs args)
	{
		notebook_capture_feedback.Visible = check_encoder_show_asteroids.Active;
	}

	private void putNonStandardIcons() {
		Pixbuf pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_bell_green.png");
		image_repetitive_best_tf_tc.Pixbuf = pixbuf;
		image_repetitive_best_speed.Pixbuf = pixbuf;
		image_repetitive_best_time.Pixbuf = pixbuf;
		image_repetitive_height_greater.Pixbuf = pixbuf;
		image_repetitive_tf_greater.Pixbuf = pixbuf;
		image_repetitive_tc_lower.Pixbuf = pixbuf;
		image_repetitive_tf_tc_greater.Pixbuf = pixbuf;
		image_repetitive_speed_lower.Pixbuf = pixbuf;
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
		image_repetitive_worst_speed.Pixbuf = pixbuf;
		image_repetitive_worst_time.Pixbuf = pixbuf;
		image_repetitive_height_lower.Pixbuf = pixbuf;
		image_repetitive_tf_lower.Pixbuf = pixbuf;
		image_repetitive_tc_greater.Pixbuf = pixbuf;
		image_repetitive_tf_tc_lower.Pixbuf = pixbuf;
		image_repetitive_speed_greater.Pixbuf = pixbuf;
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
		image_force_sensor_capture_feedback_questionnaire_load_info.Pixbuf = pixbuf;
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
			else //if (o == button_test_bad)
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
		FeedbackWindowBox.feedback.Hide();
		FakeButtonClose.Click();
		//FeedbackWindowBox = null;
	}

	void on_delete_event (object o, DeleteEventArgs args)
	{
		//FeedbackWindowBox.feedback.Hide();
		//FeedbackWindowBox = null;
		
		button_close.Click();
		args.RetVal = true;
	}

	public bool FeedbackActive (Constants.BellModes bellMode)
	{
		if(bellMode == Constants.BellModes.JUMPS)
		{
			if (
					//checkbutton_height_greater.Active || checkbutton_height_lower.Active ||
					checkbutton_jump_tf_tc_best.Active || checkbutton_jump_tf_tc_worst.Active ||
					checkbutton_tf_greater.Active || checkbutton_tf_lower.Active ||
					checkbutton_tc_lower.Active || checkbutton_tc_greater.Active //||
					//checkbutton_tf_tc_greater.Active || checkbutton_tf_tc_lower.Active
					)
				return true;
		}
		else if(bellMode == Constants.BellModes.RUNS)
		{
			if (
					checkbutton_run_speed_best.Active || checkbutton_run_speed_worst.Active ||
					checkbutton_run_time_best.Active || checkbutton_run_time_worst.Active ||
					checkbutton_speed_greater.Active || checkbutton_speed_lower.Active ||
					checkbutton_time_greater.Active || checkbutton_time_lower.Active)
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
			return ! check_force_sensor_capture_feedback_no.Active;

		return false;
	}

	public enum RunsEncoderMainVariableTypes { POWER, FORCE, ACCELERATION };
	public RunsEncoderMainVariableTypes GetRunsEncoderMainVariable ()
	{
		if(radio_run_encoder_power.Active)
			return RunsEncoderMainVariableTypes.POWER;
		else if(radio_run_encoder_force.Active)
			return RunsEncoderMainVariableTypes.FORCE;
		else // if(radio_run_encoder_accel.Active)
			return RunsEncoderMainVariableTypes.ACCELERATION;
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
	void on_spinbutton_speed_greater_value_changed (object o, EventArgs args) {
		checkbutton_speed_greater.Active = true;
	}
	void on_spinbutton_speed_lower_value_changed (object o, EventArgs args) {
		checkbutton_speed_lower.Active = true;
	}
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

		//historical does not allow to differentiate between phases
		if(radio_encoder_relative_to_set.Active)
		{
			radio_encoder_eccon_ecc.Sensitive = true;
			radio_encoder_eccon_con.Sensitive = true;
		} else {
			radio_encoder_eccon_both.Active = true;
			radio_encoder_eccon_ecc.Sensitive = false;
			radio_encoder_eccon_con.Sensitive = false;
		}
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
		//note on "c" phaseEnum will be BOTH

		if(radio_encoder_eccon_ecc.Active && phaseEnum == Preferences.EncoderPhasesEnum.CON)
			return UtilGtk.ColorGray;
		else if(radio_encoder_eccon_con.Active && phaseEnum == Preferences.EncoderPhasesEnum.ECC)
			return UtilGtk.ColorGray;

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

		if (check_rhythm_active.Active)
		{
			box_check_rhythm_use_clusters.Sensitive = true;
		} else {
			box_check_rhythm_use_clusters.Sensitive = false;
			check_rhythm_use_clusters.Active = false;
			frame_clusters.Visible = false;
		}

		should_show_vbox_rhythm_rest_after();
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
				(check_rhythm_active.Active && check_rhythm_rest_reps.Active && radio_rhythm_separated.Active) );
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
		frame_clusters.Visible = check_rhythm_use_clusters.Active;
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
		encoderRhythm.ActiveRhythm = true;

		//modify widgets
		encoder_rhythm_set_values(encoderRhythm);
	}

	private void encoder_rhythm_set_values(EncoderRhythm encoderRhythm)
	{
		check_rhythm_active.Active = encoderRhythm.ActiveRhythm;

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
		else
			check_rhythm_rest_reps.Active = true;

		if (check_rhythm_active.Active)
		{
			box_check_rhythm_use_clusters.Sensitive = true;
			check_rhythm_use_clusters.Active = (encoderRhythm.UseClusters ());
			frame_clusters.Visible = (encoderRhythm.UseClusters ());
		} else {
			box_check_rhythm_use_clusters.Sensitive = false;
			check_rhythm_use_clusters.Active = false;
			frame_clusters.Visible = false;
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

	public bool GetEncoderFeedbackAsteroidsActive {
		get { return check_encoder_show_asteroids.Active; }
	}

	/* FORCESENSOR */

	private void on_check_force_sensor_capture_feedback_no_toggled (object o, EventArgs args)
	{
		if (check_force_sensor_capture_feedback_no.Active)
		{
			box_force_sensor_capture_feedback_show.Visible = false;
			notebook_capture_feedback.Visible = false;
			box_radio_signal_direction.Visible = true;
			label_signal_direction_horizontal.Visible = false;
		} else {
			box_force_sensor_capture_feedback_show.Visible = true;
			notebook_capture_feedback.Visible = true;

			if (radio_force_sensor_capture_feedback_show_rectangle.Active)
				notebook_capture_feedback.Page = 0;
			else if (radio_force_sensor_capture_feedback_show_path.Active)
				notebook_capture_feedback.Page = 1;
			else if (radio_force_sensor_capture_feedback_show_asteroids.Active)
				notebook_capture_feedback.Page = 2;
			else //if (radio_force_sensor_capture_feedback_show_questionnaire.Active)
				notebook_capture_feedback.Page = 3;
		}
	}

	private void on_radio_force_sensor_capture_feedback_show_rectangle_toggled (object o, EventArgs args)
	{
		notebook_capture_feedback.Page = 0;
		box_radio_signal_direction.Visible = false;
		label_signal_direction_horizontal.Visible = true;
	}
	private void on_radio_force_sensor_capture_feedback_show_path_toggled (object o, EventArgs args)
	{
		notebook_capture_feedback.Page = 1;
		box_radio_signal_direction.Visible = false;
		label_signal_direction_horizontal.Visible = true;
	}
	private void on_radio_force_sensor_capture_feedback_show_asteroids_toggled (object o, EventArgs args)
	{
		notebook_capture_feedback.Page = 2;
		box_radio_signal_direction.Visible = true;
		label_signal_direction_horizontal.Visible = false;
	}
	private void on_radio_force_sensor_capture_feedback_show_questionnaire_toggled (object o, EventArgs args)
	{
		notebook_capture_feedback.Page = 3;
		box_radio_signal_direction.Visible = false;
		label_signal_direction_horizontal.Visible = true;
	}

	public Preferences.ForceSensorCaptureFeedbackActiveEnum GetForceSensorFeedback {
		get {
			if(check_force_sensor_capture_feedback_no.Active)
				return Preferences.ForceSensorCaptureFeedbackActiveEnum.NO;
			else if (radio_force_sensor_capture_feedback_show_rectangle.Active)
				return Preferences.ForceSensorCaptureFeedbackActiveEnum.RECTANGLE;
			else if (radio_force_sensor_capture_feedback_show_path.Active)
				return Preferences.ForceSensorCaptureFeedbackActiveEnum.PATH;
			else if (radio_force_sensor_capture_feedback_show_asteroids.Active)
				return Preferences.ForceSensorCaptureFeedbackActiveEnum.ASTEROIDS;
			else //if (radio_force_sensor_capture_feedback_show_questionnaire.Active)
				return Preferences.ForceSensorCaptureFeedbackActiveEnum.QUESTIONNAIRE;
		}
	}

	//force sensor feedback rectangle
	public bool GetForceSensorFeedbackRectangleActive {
		get { return radio_force_sensor_capture_feedback_show_rectangle.Active; }
	}
	public int GetForceSensorFeedbackRectangleAt {
		get { return Convert.ToInt32(spin_force_sensor_capture_feedback_rectangle_at.Value); }
	}
	public int GetForceSensorFeedbackRectangleRange {
		get { return Convert.ToInt32(spin_force_sensor_capture_feedback_rectangle_range.Value); }
	}

	//force sensor feedback path

	private void setForceSensorPathRecommendedLabel ()
	{
		label_force_sensor_path_recommended.Text = string.Format("1/3 * ({0} - {1}) = {2} N",
				Catalog.GetString("Maximum"), Catalog.GetString("Minimum"),
				Convert.ToInt32((spin_force_sensor_capture_feedback_path_max.Value - spin_force_sensor_capture_feedback_path_min.Value) /3));
	}

	private void on_spin_force_sensor_capture_feedback_path_min_value_changed (object o, EventArgs args)
	{
		if(spin_force_sensor_capture_feedback_path_min.Value > spin_force_sensor_capture_feedback_path_max.Value)
			spin_force_sensor_capture_feedback_path_max.Value = spin_force_sensor_capture_feedback_path_min.Value;

		setForceSensorPathRecommendedLabel();
	}
	private void on_spin_force_sensor_capture_feedback_path_max_value_changed (object o, EventArgs args)
	{
		if(spin_force_sensor_capture_feedback_path_max.Value < spin_force_sensor_capture_feedback_path_min.Value)
			spin_force_sensor_capture_feedback_path_min.Value = spin_force_sensor_capture_feedback_path_max.Value;

		setForceSensorPathRecommendedLabel();
	}

	/*
	public bool GetForceSensorFeedbackPathActive {
		get { return radio_force_sensor_capture_feedback_show_path.Active; }
	}
	*/
	public int GetForceSensorFeedbackPathMax {
		get { return Convert.ToInt32(spin_force_sensor_capture_feedback_path_max.Value); }
	}
	public int GetForceSensorFeedbackPathMin {
		get { return Convert.ToInt32(spin_force_sensor_capture_feedback_path_min.Value); }
	}
	public int GetForceSensorFeedbackPathMasters {
		get { return Convert.ToInt32(spin_force_sensor_capture_feedback_path_masters.Value); }
	}
	public int GetForceSensorFeedbackPathMasterSeconds {
		get { return Convert.ToInt32(spin_force_sensor_capture_feedback_path_master_seconds.Value); }
	}
	public int GetForceSensorFeedbackPathLineWidth {
		get { return Convert.ToInt32(spin_force_sensor_capture_feedback_path_line_width.Value); } //N
	}

	// force sensor feedback questionnaire

	public void on_radio_force_sensor_capture_feedback_questionnaire_default_load_toggled (object o, EventArgs args)
	{
		buttons_force_sensor_capture_feedback_questionnaire_load.Sensitive = ! radio_force_sensor_capture_feedback_questionnaire_default.Active;
	}

	public void on_button_force_sensor_capture_feedback_questionnaire_load_info_clicked (object o, EventArgs args)
	{
		new DialogMessage (Constants.MessageTypes.INFO,
				"Rules:" +
				"\n- " + "File is a csv." +
				"\n- " + "First row are column headers." +
				"\n- " + "Columns are separated by , or ;" +
				"\n- " + "First column is the question." +
				"\n- " + "Second column is the correct answer." +
				"\n- " + "Third, fourth and fith columns are the bad answers." +
				"\n\n Questions and answers are going to be randomized.");
	}

	public void on_button_force_sensor_capture_feedback_questionnaire_load_clicked (object o, EventArgs args)
	{
		Gtk.FileChooserDialog fc = new Gtk.FileChooserDialog(Catalog.GetString("Load file"),
				feedback,
				FileChooserAction.Open,
				Catalog.GetString("Cancel"),ResponseType.Cancel,
				Catalog.GetString("Load"),ResponseType.Accept
				);

		fc.Filter = new FileFilter();
		fc.Filter.AddPattern("*.csv");
		fc.Filter.AddPattern("*.CSV");

		if (fc.Run() == (int)ResponseType.Accept)
		{
			try {
				forceSensorFeedbackQuestionnaireFile = fc.Filename;
				FakeButtonQuestionnaireLoad.Click ();
			} catch {
				LogB.Warning("Cannot be loaded");
			}
		}
		//Don't forget to call Destroy() or the FileChooserDialog window won't get closed.
		fc.Destroy();
	}

	public void button_force_sensor_capture_feedback_questionnaire_load_analyzed (bool ok)
	{
		if (ok)
			label_force_sensor_capture_feedback_questionnaire_load_success.Text = Catalog.GetString ("Loaded");
		else {
			label_force_sensor_capture_feedback_questionnaire_load_success.Text = Catalog.GetString ("Error. File not compatible.");
			forceSensorFeedbackQuestionnaireFile = "";
		}
	}

	public int GetForceSensorFeedbackAsteroidsMax {
		get { return Convert.ToInt32(spin_force_sensor_capture_feedback_asteroids_max.Value); }
	}
	public int GetForceSensorFeedbackAsteroidsMin {
		get { return Convert.ToInt32(spin_force_sensor_capture_feedback_asteroids_min.Value); }
	}
	public bool GetForceSensorFeedbackAsteroidsDark {
		get { return check_force_sensor_capture_feedback_asteroids_dark.Active; }
	}
	public int GetForceSensorFeedbackAsteroidsFrequency {
		get { return Convert.ToInt32 (spin_force_sensor_capture_feedback_asteroids_frequency.Value); }
	}
	public int GetForceSensorFeedbackShotsFrequency {
		get { return Convert.ToInt32 (spin_force_sensor_capture_feedback_shots_frequency.Value); }
	}

	public int GetForceSensorFeedbackQuestionnaireMax {
		get { return Convert.ToInt32(spin_force_sensor_capture_feedback_questionnaire_max.Value); }
	}
	public int GetForceSensorFeedbackQuestionnaireMin {
		get { return Convert.ToInt32(spin_force_sensor_capture_feedback_questionnaire_min.Value); }
	}
	public int GetForceSensorFeedbackQuestionnaireN {
		get { return Convert.ToInt32(spin_force_sensor_capture_feedback_questionnaire_n.Value); }
	}
	public bool GetForceSensorFeedbackQuestionnaireDefaultOrFile {
		get { return radio_force_sensor_capture_feedback_questionnaire_default.Active; }
	}
	public string GetForceSensorFeedbackQuestionnaireFile {
		get { return forceSensorFeedbackQuestionnaireFile; }
	}

	public bool GetSignalDirectionHorizontal {
		get { return radio_signal_direction_horizontal.Active; }
	}

	/* JUMPS */

	public bool JumpsRjFeedbackShowBestTvTc {
		get { return checkbutton_jump_tf_tc_best.Active; }
	}
	public bool JumpsRjFeedbackShowWorstTvTc {
		get { return checkbutton_jump_tf_tc_worst.Active; }
	}

	/*
	public bool HeightGreater {
		get { return checkbutton_height_greater.Active; }
	}
	public bool HeightLower {
		get { return checkbutton_height_lower.Active; }
	}
	*/

	public bool JumpsRjFeedbackTvGreaterActive {
		get { return checkbutton_tf_greater.Active; }
	}
	public bool JumpsRjFeedbackTvLowerActive {
		get { return checkbutton_tf_lower.Active; }
	}

	public bool JumpsRjFeedbackTcGreaterActive {
		get { return checkbutton_tc_greater.Active; }
	}
	public bool JumpsRjFeedbackTcLowerActive {
		get { return checkbutton_tc_lower.Active; }
	}

	/*
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
	*/

	public double JumpsRjFeedbackTvGreater {
		get { return Convert.ToDouble(spinbutton_tf_greater.Value); }
	}
	public double JumpsRjFeedbackTvLower {
		get { return Convert.ToDouble(spinbutton_tf_lower.Value); }
	}

	public double JumpsRjFeedbackTcGreater {
		get { return Convert.ToDouble(spinbutton_tc_greater.Value); }
	}
	public double JumpsRjFeedbackTcLower {
		get { return Convert.ToDouble(spinbutton_tc_lower.Value); }
	}

	/*
	public double TfTcGreaterValue {
		get { return Convert.ToDouble(spinbutton_tf_tc_greater.Value); }
	}
	public double TfTcLowerValue {
		get { return Convert.ToDouble(spinbutton_tf_tc_lower.Value); }
	}
	*/

	/* RUNS */
	public bool RunsIFeedbackSpeedBestActive {
		get { return checkbutton_run_speed_best.Active; }
	}
	public bool RunsIFeedbackSpeedWorstActive {
		get { return checkbutton_run_speed_worst.Active; }
	}
	public bool RunsIFeedbackTimeBestActive {
		get { return checkbutton_run_time_best.Active; }
	}
	public bool RunsIFeedbackTimeWorstActive {
		get { return checkbutton_run_time_worst.Active; }
	}

	public bool RunsIFeedbackSpeedGreaterActive {
		get { return checkbutton_speed_greater.Active; }
	}
	public bool RunsIFeedbackSpeedLowerActive {
		get { return checkbutton_speed_lower.Active; }
	}
	public double RunsIFeedbackSpeedGreater {
		get { return Convert.ToDouble(spinbutton_speed_greater.Value); }
	}
	public double RunsIFeedbackSpeedLower {
		get { return Convert.ToDouble(spinbutton_speed_lower.Value); }
	}

	public bool RunsIFeedbackTimeGreaterActive {
		get { return checkbutton_time_greater.Active; }
	}
	public bool RunsIFeedbackTimeLowerActive {
		get { return checkbutton_time_lower.Active; }
	}
	public double RunsIFeedbackTimeGreater {
		get { return Convert.ToDouble(spinbutton_time_greater.Value); }
	}
	public double RunsIFeedbackTimeLower {
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

	public bool EncoderCaptureShowLoss {
		get { return check_encoder_show_loss.Active; }
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
		set {
			if(value)
				notebook_main.GetNthPage(ENCODERMANUALPAGE).Show();
			else
				notebook_main.GetNthPage(ENCODERMANUALPAGE).Hide();
		}
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

	private void connectWidgets (Gtk.Builder builder)
	{
		feedback = (Gtk.Window) builder.GetObject ("feedback");
		notebook_main = (Gtk.Notebook) builder.GetObject ("notebook_main");
		//scrolled_conditions = (Gtk.ScrolledWindow) builder.GetObject ("scrolled_conditions");

		hbox_jump_best_worst = (Gtk.Box) builder.GetObject ("hbox_jump_best_worst");
		grid_run_best_worst = (Gtk.Grid) builder.GetObject ("grid_run_best_worst");

		/* jumps */	
		hbox_jump_conditions = (Gtk.Box) builder.GetObject ("hbox_jump_conditions");
		checkbutton_jump_tf_tc_best = (Gtk.CheckButton) builder.GetObject ("checkbutton_jump_tf_tc_best");
		checkbutton_jump_tf_tc_worst = (Gtk.CheckButton) builder.GetObject ("checkbutton_jump_tf_tc_worst");

		checkbutton_height_greater = (Gtk.CheckButton) builder.GetObject ("checkbutton_height_greater");
		checkbutton_height_lower = (Gtk.CheckButton) builder.GetObject ("checkbutton_height_lower");
		checkbutton_tf_greater = (Gtk.CheckButton) builder.GetObject ("checkbutton_tf_greater");
		checkbutton_tf_lower = (Gtk.CheckButton) builder.GetObject ("checkbutton_tf_lower");
		checkbutton_tc_greater = (Gtk.CheckButton) builder.GetObject ("checkbutton_tc_greater");
		checkbutton_tc_lower = (Gtk.CheckButton) builder.GetObject ("checkbutton_tc_lower");
		checkbutton_tf_tc_greater = (Gtk.CheckButton) builder.GetObject ("checkbutton_tf_tc_greater");
		checkbutton_tf_tc_lower = (Gtk.CheckButton) builder.GetObject ("checkbutton_tf_tc_lower");

		//spinbutton_height_greater = (Gtk.SpinButton) builder.GetObject ("spinbutton_height_greater");
		//spinbutton_height_lower = (Gtk.SpinButton) builder.GetObject ("spinbutton_height_lower");
		spinbutton_tf_greater = (Gtk.SpinButton) builder.GetObject ("spinbutton_tf_greater");
		spinbutton_tf_lower = (Gtk.SpinButton) builder.GetObject ("spinbutton_tf_lower");
		spinbutton_tc_greater = (Gtk.SpinButton) builder.GetObject ("spinbutton_tc_greater");
		spinbutton_tc_lower = (Gtk.SpinButton) builder.GetObject ("spinbutton_tc_lower");
		//spinbutton_tf_tc_greater = (Gtk.SpinButton) builder.GetObject ("spinbutton_tf_tc_greater");
		//spinbutton_tf_tc_lower = (Gtk.SpinButton) builder.GetObject ("spinbutton_tf_tc_lower");

		/* runs */	
		hbox_run_conditions_speed = (Gtk.Box) builder.GetObject ("hbox_run_conditions_speed");
		hbox_run_conditions_time = (Gtk.Box) builder.GetObject ("hbox_run_conditions_time");
		checkbutton_run_speed_best = (Gtk.CheckButton) builder.GetObject ("checkbutton_run_speed_best");
		checkbutton_run_speed_worst = (Gtk.CheckButton) builder.GetObject ("checkbutton_run_speed_worst");
		checkbutton_run_time_best = (Gtk.CheckButton) builder.GetObject ("checkbutton_run_time_best");
		checkbutton_run_time_worst = (Gtk.CheckButton) builder.GetObject ("checkbutton_run_time_worst");

		checkbutton_speed_greater = (Gtk.CheckButton) builder.GetObject ("checkbutton_speed_greater");
		checkbutton_speed_lower = (Gtk.CheckButton) builder.GetObject ("checkbutton_speed_lower");
		spinbutton_speed_greater = (Gtk.SpinButton) builder.GetObject ("spinbutton_speed_greater");
		spinbutton_speed_lower = (Gtk.SpinButton) builder.GetObject ("spinbutton_speed_lower");

		checkbutton_time_greater = (Gtk.CheckButton) builder.GetObject ("checkbutton_time_greater");
		checkbutton_time_lower = (Gtk.CheckButton) builder.GetObject ("checkbutton_time_lower");
		spinbutton_time_greater = (Gtk.SpinButton) builder.GetObject ("spinbutton_time_greater");
		spinbutton_time_lower = (Gtk.SpinButton) builder.GetObject ("spinbutton_time_lower");

		/* encoder */
		hbox_combo_encoder_main_variable = (Gtk.HBox) builder.GetObject ("hbox_combo_encoder_main_variable");
		radio_encoder_relative_to_set = (Gtk.RadioButton) builder.GetObject ("radio_encoder_relative_to_set");
		radio_encoder_relative_to_historical = (Gtk.RadioButton) builder.GetObject ("radio_encoder_relative_to_historical");
		checkbutton_encoder_automatic_greater = (Gtk.CheckButton) builder.GetObject ("checkbutton_encoder_automatic_greater");
		checkbutton_encoder_automatic_lower = (Gtk.CheckButton) builder.GetObject ("checkbutton_encoder_automatic_lower");
		spinbutton_encoder_automatic_greater = (Gtk.SpinButton) builder.GetObject ("spinbutton_encoder_automatic_greater");
		spinbutton_encoder_automatic_lower = (Gtk.SpinButton) builder.GetObject ("spinbutton_encoder_automatic_lower");
		check_encoder_show_secondary_variable = (Gtk.CheckButton) builder.GetObject ("check_encoder_show_secondary_variable");
		hbox_combo_encoder_secondary_variable = (Gtk.HBox) builder.GetObject ("hbox_combo_encoder_secondary_variable");
		radio_encoder_eccon_both = (Gtk.RadioButton) builder.GetObject ("radio_encoder_eccon_both");
		radio_encoder_eccon_ecc = (Gtk.RadioButton) builder.GetObject ("radio_encoder_eccon_ecc");
		radio_encoder_eccon_con = (Gtk.RadioButton) builder.GetObject ("radio_encoder_eccon_con");
		check_encoder_inertial_ecc_overload = (Gtk.CheckButton) builder.GetObject ("check_encoder_inertial_ecc_overload");
		check_encoder_inertial_ecc_overload_percent = (Gtk.CheckButton) builder.GetObject ("check_encoder_inertial_ecc_overload_percent");
		check_encoder_show_loss = (Gtk.CheckButton) builder.GetObject ("check_encoder_show_loss");

		notebook_encoder_conditions = (Gtk.Notebook) builder.GetObject ("notebook_encoder_conditions");
		checkbutton_encoder_height_higher = (Gtk.CheckButton) builder.GetObject ("checkbutton_encoder_height_higher");
		checkbutton_encoder_height_lower = (Gtk.CheckButton) builder.GetObject ("checkbutton_encoder_height_lower");
		checkbutton_encoder_mean_speed_higher = (Gtk.CheckButton) builder.GetObject ("checkbutton_encoder_mean_speed_higher");
		checkbutton_encoder_max_speed_higher = (Gtk.CheckButton) builder.GetObject ("checkbutton_encoder_max_speed_higher");
		checkbutton_encoder_mean_speed_lower = (Gtk.CheckButton) builder.GetObject ("checkbutton_encoder_mean_speed_lower");
		checkbutton_encoder_max_speed_lower = (Gtk.CheckButton) builder.GetObject ("checkbutton_encoder_max_speed_lower");
		checkbutton_encoder_mean_force_higher = (Gtk.CheckButton) builder.GetObject ("checkbutton_encoder_mean_force_higher");
		checkbutton_encoder_max_force_higher = (Gtk.CheckButton) builder.GetObject ("checkbutton_encoder_max_force_higher");
		checkbutton_encoder_mean_force_lower = (Gtk.CheckButton) builder.GetObject ("checkbutton_encoder_mean_force_lower");
		checkbutton_encoder_max_force_lower = (Gtk.CheckButton) builder.GetObject ("checkbutton_encoder_max_force_lower");
		checkbutton_encoder_power_higher = (Gtk.CheckButton) builder.GetObject ("checkbutton_encoder_power_higher");
		checkbutton_encoder_peakpower_higher = (Gtk.CheckButton) builder.GetObject ("checkbutton_encoder_peakpower_higher");
		checkbutton_encoder_power_lower = (Gtk.CheckButton) builder.GetObject ("checkbutton_encoder_power_lower");
		checkbutton_encoder_peakpower_lower = (Gtk.CheckButton) builder.GetObject ("checkbutton_encoder_peakpower_lower");
		spinbutton_encoder_height_higher = (Gtk.SpinButton) builder.GetObject ("spinbutton_encoder_height_higher");
		spinbutton_encoder_height_lower = (Gtk.SpinButton) builder.GetObject ("spinbutton_encoder_height_lower");
		spinbutton_encoder_mean_speed_higher = (Gtk.SpinButton) builder.GetObject ("spinbutton_encoder_mean_speed_higher");
		spinbutton_encoder_max_speed_higher = (Gtk.SpinButton) builder.GetObject ("spinbutton_encoder_max_speed_higher");
		spinbutton_encoder_mean_speed_lower = (Gtk.SpinButton) builder.GetObject ("spinbutton_encoder_mean_speed_lower");
		spinbutton_encoder_max_speed_lower = (Gtk.SpinButton) builder.GetObject ("spinbutton_encoder_max_speed_lower");
		spinbutton_encoder_mean_force_higher = (Gtk.SpinButton) builder.GetObject ("spinbutton_encoder_mean_force_higher");
		spinbutton_encoder_max_force_higher = (Gtk.SpinButton) builder.GetObject ("spinbutton_encoder_max_force_higher");
		spinbutton_encoder_mean_force_lower = (Gtk.SpinButton) builder.GetObject ("spinbutton_encoder_mean_force_lower");
		spinbutton_encoder_max_force_lower = (Gtk.SpinButton) builder.GetObject ("spinbutton_encoder_max_force_lower");
		spinbutton_encoder_power_higher = (Gtk.SpinButton) builder.GetObject ("spinbutton_encoder_power_higher");
		spinbutton_encoder_peakpower_higher = (Gtk.SpinButton) builder.GetObject ("spinbutton_encoder_peakpower_higher");
		spinbutton_encoder_power_lower = (Gtk.SpinButton) builder.GetObject ("spinbutton_encoder_power_lower");
		spinbutton_encoder_peakpower_lower = (Gtk.SpinButton) builder.GetObject ("spinbutton_encoder_peakpower_lower");

		check_encoder_show_asteroids = (Gtk.CheckButton) builder.GetObject ("check_encoder_show_asteroids");

		button_test_good = (Gtk.Button) builder.GetObject ("button_test_good");
		//button_test_bad = (Gtk.Button) builder.GetObject ("button_test_bad");
		label_test_sound_result = (Gtk.Label) builder.GetObject ("label_test_sound_result");
		button_close = (Gtk.Button) builder.GetObject ("button_close");

		//bells good (green)
		image_repetitive_best_tf_tc = (Gtk.Image) builder.GetObject ("image_repetitive_best_tf_tc");
		image_repetitive_best_speed = (Gtk.Image) builder.GetObject ("image_repetitive_best_speed");
		image_repetitive_best_time = (Gtk.Image) builder.GetObject ("image_repetitive_best_time");
		image_repetitive_height_greater = (Gtk.Image) builder.GetObject ("image_repetitive_height_greater");
		image_repetitive_tf_greater = (Gtk.Image) builder.GetObject ("image_repetitive_tf_greater");
		image_repetitive_tc_lower = (Gtk.Image) builder.GetObject ("image_repetitive_tc_lower");
		image_repetitive_tf_tc_greater = (Gtk.Image) builder.GetObject ("image_repetitive_tf_tc_greater");
		image_repetitive_speed_lower = (Gtk.Image) builder.GetObject ("image_repetitive_speed_lower");
		image_repetitive_time_lower = (Gtk.Image) builder.GetObject ("image_repetitive_time_lower");
		image_repetitive_encoder_automatic_greater = (Gtk.Image) builder.GetObject ("image_repetitive_encoder_automatic_greater");
		image_encoder_height_higher = (Gtk.Image) builder.GetObject ("image_encoder_height_higher");
		image_encoder_mean_speed_higher = (Gtk.Image) builder.GetObject ("image_encoder_mean_speed_higher");
		image_encoder_max_speed_higher = (Gtk.Image) builder.GetObject ("image_encoder_max_speed_higher");
		image_encoder_mean_force_higher = (Gtk.Image) builder.GetObject ("image_encoder_mean_force_higher");
		image_encoder_max_force_higher = (Gtk.Image) builder.GetObject ("image_encoder_max_force_higher");
		image_encoder_power_higher = (Gtk.Image) builder.GetObject ("image_encoder_power_higher");
		image_encoder_peakpower_higher = (Gtk.Image) builder.GetObject ("image_encoder_peakpower_higher");
		image_repetitive_test_good = (Gtk.Image) builder.GetObject ("image_repetitive_test_good");
		//bells bad (red)
		image_repetitive_worst_tf_tc = (Gtk.Image) builder.GetObject ("image_repetitive_worst_tf_tc");
		image_repetitive_worst_speed = (Gtk.Image) builder.GetObject ("image_repetitive_worst_speed");
		image_repetitive_worst_time = (Gtk.Image) builder.GetObject ("image_repetitive_worst_time");
		image_repetitive_height_lower = (Gtk.Image) builder.GetObject ("image_repetitive_height_lower");
		image_repetitive_tf_lower = (Gtk.Image) builder.GetObject ("image_repetitive_tf_lower");
		image_repetitive_tc_greater = (Gtk.Image) builder.GetObject ("image_repetitive_tc_greater");
		image_repetitive_tf_tc_lower = (Gtk.Image) builder.GetObject ("image_repetitive_tf_tc_lower");
		image_repetitive_speed_greater = (Gtk.Image) builder.GetObject ("image_repetitive_speed_greater");
		image_repetitive_time_greater = (Gtk.Image) builder.GetObject ("image_repetitive_time_greater");
		image_repetitive_encoder_automatic_lower = (Gtk.Image) builder.GetObject ("image_repetitive_encoder_automatic_lower");
		image_encoder_height_lower = (Gtk.Image) builder.GetObject ("image_encoder_height_lower");
		image_encoder_mean_speed_lower = (Gtk.Image) builder.GetObject ("image_encoder_mean_speed_lower");
		image_encoder_max_speed_lower = (Gtk.Image) builder.GetObject ("image_encoder_max_speed_lower");
		image_encoder_mean_force_lower = (Gtk.Image) builder.GetObject ("image_encoder_mean_force_lower");
		image_encoder_max_force_lower = (Gtk.Image) builder.GetObject ("image_encoder_max_force_lower");
		image_encoder_power_lower = (Gtk.Image) builder.GetObject ("image_encoder_power_lower");
		image_encoder_peakpower_lower = (Gtk.Image) builder.GetObject ("image_encoder_peakpower_lower");
		image_repetitive_test_bad = (Gtk.Image) builder.GetObject ("image_repetitive_test_bad");

		//encoder rhythm
		label_rhythm_tab = (Gtk.Label) builder.GetObject ("label_rhythm_tab");
		check_rhythm_active = (Gtk.CheckButton) builder.GetObject ("check_rhythm_active");
		radio_rhythm_together = (Gtk.RadioButton) builder.GetObject ("radio_rhythm_together");
		radio_rhythm_separated = (Gtk.RadioButton) builder.GetObject ("radio_rhythm_separated");
		notebook_duration_repetition = (Gtk.Notebook) builder.GetObject ("notebook_duration_repetition");
		frame_clusters = (Gtk.Frame) builder.GetObject ("frame_clusters");
		frame_rhythm = (Gtk.Frame) builder.GetObject ("frame_rhythm");
		box_check_rhythm_use_clusters = (Gtk.Box) builder.GetObject ("box_check_rhythm_use_clusters");
		check_rhythm_use_clusters = (Gtk.CheckButton) builder.GetObject ("check_rhythm_use_clusters");
		spin_rhythm_rep = (Gtk.SpinButton) builder.GetObject ("spin_rhythm_rep");
		spin_rhythm_ecc = (Gtk.SpinButton) builder.GetObject ("spin_rhythm_ecc");
		spin_rhythm_con = (Gtk.SpinButton) builder.GetObject ("spin_rhythm_con");
		label_rhythm_ecc_plus_con = (Gtk.Label) builder.GetObject ("label_rhythm_ecc_plus_con");
		spin_rhythm_rest_reps = (Gtk.SpinButton) builder.GetObject ("spin_rhythm_rest_reps");
		vbox_rhythm_rest_after = (Gtk.VBox) builder.GetObject ("vbox_rhythm_rest_after");
		radio_rest_after_ecc = (Gtk.RadioButton) builder.GetObject ("radio_rest_after_ecc");
		spin_rhythm_reps_cluster = (Gtk.SpinButton) builder.GetObject ("spin_rhythm_reps_cluster");
		spin_rhythm_rest_clusters = (Gtk.SpinButton) builder.GetObject ("spin_rhythm_rest_clusters");
		image_clusters_info = (Gtk.Image) builder.GetObject ("image_clusters_info");
		hbox_rhythm_rest_reps_value = (Gtk.HBox) builder.GetObject ("hbox_rhythm_rest_reps_value");
		check_rhythm_rest_reps = (Gtk.CheckButton) builder.GetObject ("check_rhythm_rest_reps");

		//forceSensor
		box_forceSensor_feedback = (Gtk.Box) builder.GetObject ("box_forceSensor_feedback");
		notebook_capture_feedback = (Gtk.Notebook) builder.GetObject ("notebook_capture_feedback");
		box_force_sensor_capture_feedback_show = (Gtk.Box) builder.GetObject ("box_force_sensor_capture_feedback_show");
		check_force_sensor_capture_feedback_no = (Gtk.CheckButton) builder.GetObject ("check_force_sensor_capture_feedback_no");
		radio_force_sensor_capture_feedback_show_rectangle = (Gtk.RadioButton) builder.GetObject ("radio_force_sensor_capture_feedback_show_rectangle");
		radio_force_sensor_capture_feedback_show_path = (Gtk.RadioButton) builder.GetObject ("radio_force_sensor_capture_feedback_show_path");
		radio_force_sensor_capture_feedback_show_asteroids = (Gtk.RadioButton) builder.GetObject ("radio_force_sensor_capture_feedback_show_asteroids");
		radio_force_sensor_capture_feedback_show_questionnaire = (Gtk.RadioButton) builder.GetObject ("radio_force_sensor_capture_feedback_show_questionnaire");
		//rectangle
		spin_force_sensor_capture_feedback_rectangle_at = (Gtk.SpinButton) builder.GetObject ("spin_force_sensor_capture_feedback_rectangle_at");
		spin_force_sensor_capture_feedback_rectangle_range = (Gtk.SpinButton) builder.GetObject ("spin_force_sensor_capture_feedback_rectangle_range");
		//path
		spin_force_sensor_capture_feedback_path_max = (Gtk.SpinButton) builder.GetObject ("spin_force_sensor_capture_feedback_path_max");
		spin_force_sensor_capture_feedback_path_min = (Gtk.SpinButton) builder.GetObject ("spin_force_sensor_capture_feedback_path_min");
		spin_force_sensor_capture_feedback_path_masters = (Gtk.SpinButton) builder.GetObject ("spin_force_sensor_capture_feedback_path_masters");
		spin_force_sensor_capture_feedback_path_master_seconds = (Gtk.SpinButton) builder.GetObject ("spin_force_sensor_capture_feedback_path_master_seconds");
		spin_force_sensor_capture_feedback_path_line_width = (Gtk.SpinButton) builder.GetObject ("spin_force_sensor_capture_feedback_path_line_width"); //N
		label_force_sensor_path_recommended = (Gtk.Label) builder.GetObject ("label_force_sensor_path_recommended");
		//asteroids
		spin_force_sensor_capture_feedback_asteroids_max = (Gtk.SpinButton) builder.GetObject ("spin_force_sensor_capture_feedback_asteroids_max");
		spin_force_sensor_capture_feedback_asteroids_min = (Gtk.SpinButton) builder.GetObject ("spin_force_sensor_capture_feedback_asteroids_min");
		check_force_sensor_capture_feedback_asteroids_dark = (Gtk.CheckButton) builder.GetObject ("check_force_sensor_capture_feedback_asteroids_dark");
		spin_force_sensor_capture_feedback_asteroids_frequency = (Gtk.SpinButton) builder.GetObject ("spin_force_sensor_capture_feedback_asteroids_frequency");
		spin_force_sensor_capture_feedback_shots_frequency = (Gtk.SpinButton) builder.GetObject ("spin_force_sensor_capture_feedback_shots_frequency");
		label_feedback_asteroids_min_units = (Gtk.Label) builder.GetObject ("label_feedback_asteroids_min_units");
		label_feedback_asteroids_max_units = (Gtk.Label) builder.GetObject ("label_feedback_asteroids_max_units");
		//questionnaire
		spin_force_sensor_capture_feedback_questionnaire_max = (Gtk.SpinButton) builder.GetObject ("spin_force_sensor_capture_feedback_questionnaire_max");
		spin_force_sensor_capture_feedback_questionnaire_min = (Gtk.SpinButton) builder.GetObject ("spin_force_sensor_capture_feedback_questionnaire_min");
		spin_force_sensor_capture_feedback_questionnaire_n = (Gtk.SpinButton) builder.GetObject ("spin_force_sensor_capture_feedback_questionnaire_n");
		radio_force_sensor_capture_feedback_questionnaire_default = (Gtk.RadioButton) builder.GetObject ("radio_force_sensor_capture_feedback_questionnaire_default");
		radio_force_sensor_capture_feedback_questionnaire_load = (Gtk.RadioButton) builder.GetObject ("radio_force_sensor_capture_feedback_questionnaire_load");
		image_force_sensor_capture_feedback_questionnaire_load_info = (Gtk.Image) builder.GetObject ("image_force_sensor_capture_feedback_questionnaire_load_info");
		buttons_force_sensor_capture_feedback_questionnaire_load = (Gtk.Box) builder.GetObject ("buttons_force_sensor_capture_feedback_questionnaire_load");
		label_force_sensor_capture_feedback_questionnaire_load_success = (Gtk.Label) builder.GetObject ("label_force_sensor_capture_feedback_questionnaire_load_success");
		//direction
		radio_signal_direction_horizontal = (Gtk.RadioButton) builder.GetObject ("radio_signal_direction_horizontal");
		radio_signal_direction_vertical = (Gtk.RadioButton) builder.GetObject ("radio_signal_direction_vertical");
		box_radio_signal_direction = (Gtk.Box) builder.GetObject ("box_radio_signal_direction");
		label_signal_direction_horizontal = (Gtk.Label) builder.GetObject ("label_signal_direction_horizontal");

		//runsEncoder
		radio_run_encoder_power = (Gtk.RadioButton) builder.GetObject ("radio_run_encoder_power");
		radio_run_encoder_force = (Gtk.RadioButton) builder.GetObject ("radio_run_encoder_force");
		//radio_run_encoder_accel = (Gtk.RadioButton) builder.GetObject ("radio_run_encoder_accel");
	}
}

