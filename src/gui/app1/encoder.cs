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
 * Copyright (C) 2004-2026   Xavier de Blas <xaviblas@gmail.com>
 */
using System;
using System.IO; 
using Gtk;
using Gdk;
//using Glade;
using System.Collections;
using System.Collections.Generic; //List<T>
using System.Threading;
using Mono.Unix;
using System.Diagnostics; 	//for detect OS and for Process


public partial class ChronoJumpWindow 
{
	// at glade ---->
	Gtk.HBox hbox_encoder_capture_top;
	Gtk.Label label_button_encoder_select;
	Gtk.Label label_encoder_exercise_mass;
	Gtk.HBox hbox_encoder_exercise_mass;
	Gtk.Label label_encoder_exercise_inertia;
	Gtk.Box box_encoder_exercise_inertia;
	Gtk.HBox hbox_encoder_exercise_gravitatory_min_mov;
	Gtk.HBox hbox_encoder_exercise_inertial_min_mov;
	Gtk.SpinButton spin_encoder_capture_min_height_gravitatory;
	Gtk.SpinButton spin_encoder_capture_min_height_inertial;
	Gtk.VBox vbox_capture_current_encoder;

	Gtk.Button button_encoder_select;
	Gtk.SpinButton spin_encoder_extra_weight;
	Gtk.Label label_encoder_displaced_weight;
	Gtk.HBox hbox_capture_1RM;
	Gtk.Label label_encoder_1RM_percent;
	Gtk.Label label_encoder_im_total;
	Gtk.Label label_encoder_equivalent_mass;
	Gtk.SpinButton spin_encoder_im_weights_n;
	Gtk.HBox hbox_combo_encoder_anchorage;

	Gtk.Label label_encoder_selected;	
	Gtk.Image image_encoder_top_selected_type;
	Gtk.Image image_encoder_selected_type;

	Gtk.Notebook notebook_encoder_top;
	Gtk.Notebook notebook_hpaned_encoder_or_exercise_config;
	Gtk.Label label_encoder_top_selected;
	Gtk.Label label_encoder_top_exercise;
	Gtk.Label label_encoder_top_extra_mass;
	Gtk.Label label_encoder_top_1RM_percent;
	Gtk.Label label_encoder_top_weights;
	Gtk.Label label_encoder_top_im;

	//this is kg*cm^2 because there's limitation of Glade on 3 decimals. 
	//at SQL it's in kg*cm^2 also because it's stored as int
	//at graph.R is converted to kg*m^2 ( /10000 )
	//Gtk.SpinButton spin_encoder_capture_inertial; 

	Gtk.Box hbox_encoder_configuration;
	Gtk.Frame frame_encoder_capture_options;
	Gtk.HBox hbox_encoder_capture_actions;
	Gtk.VBox vbox_inertial_instructions;
	
	Gtk.Box hbox_encoder_capture_wait;
	Gtk.Box box_encoder_capture_doing;
	Gtk.VScale vscale_encoder_capture_inertial_angle_now;
	Gtk.VBox vbox_angle_now;
	Gtk.Label label_encoder_capture_inertial_angle_now;

	Gtk.Button button_encoder_capture;

	Gtk.Box box_encoder_capture_csharp_r_both;
	Gtk.RadioButton radio_encoder_capture_csharp;
	Gtk.RadioButton radio_encoder_capture_r;
	Gtk.RadioButton radio_encoder_capture_both;

	//encoder calibrate/recalibrate widgets
	Gtk.Button button_encoder_inertial_calibrate;
	Gtk.Button button_encoder_inertial_recalibrate;
	Gtk.Label label_calibrate_output_message;
	Gtk.Button button_encoder_inertial_calibrate_close;
	Gtk.Label label_wait;

	
	Gtk.Image image_encoder_bell;
	Gtk.Button button_encoder_capture_cancel;
	Gtk.Button button_encoder_capture_finish;
	Gtk.Button button_encoder_capture_finish_cont;
	Gtk.Button button_encoder_bells;
	Gtk.Button button_encoder_load_signal_at_analyze;
	Gtk.ProgressBar encoder_pulsebar_capture;
	Gtk.Label encoder_countdown_label;
	Gtk.Box box_encoder_capture_rhythm;
	Gtk.Box box_encoder_capture_rhythm_doing;
	Gtk.Box box_encoder_capture_rhythm_rest;
	Gtk.ProgressBar encoder_pulsebar_rhythm_eccon;
	Gtk.Label label_encoder_rhythm_rest;
	Gtk.Label label_rhythm;
	Gtk.Label label_rhythm_rep;
	Gtk.VBox vbox_capturing_with_triggers;
	Gtk.Button button_encoder_export_signal;
//	Gtk.Button button_menu_encoder_export_set;

	Gtk.Button button_encoder_devices_networks;
	//Gtk.Button button_encoder_devices_networks_problems;

	//encoder capture tab view options
	Gtk.HBox hbox_encoder_show_signal_table;
	Gtk.CheckButton check_encoder_capture_table;
	Gtk.CheckButton check_encoder_capture_signal;
	Gtk.VBox vbox_encoder_bars_table_and_save_reps;
	Gtk.Alignment alignment_encoder_capture_curves_bars_drawingarea;

	Gtk.Box hbox_combo_encoder_exercise_capture;
	Gtk.RadioButton radio_encoder_eccon_concentric;
	Gtk.RadioButton radio_encoder_eccon_eccentric_concentric;
	Gtk.RadioButton radio_encoder_laterality_both;
	Gtk.RadioButton radio_encoder_laterality_r;
	Gtk.RadioButton radio_encoder_laterality_l;

	//exercise edit/add
	Gtk.HBox hbox_encoder_exercise_close_and;
	Gtk.HBox hbox_encoder_exercise_select;
	Gtk.HBox hbox_encoder_exercise_actions;
	Gtk.Button button_encoder_exercise_actions_edit_do;
	Gtk.Button button_encoder_exercise_actions_add_do;
	Gtk.Notebook notebook_encoder_exercise;
	Gtk.Entry entry_encoder_exercise_name;
	Gtk.RadioButton radio_encoder_exercise_gravitatory;
	Gtk.RadioButton radio_encoder_exercise_inertial;
	Gtk.RadioButton radio_encoder_exercise_all;
	Gtk.Button button_radio_encoder_exercise_help;
	Gtk.SpinButton spin_encoder_exercise_displaced_body_weight;
	Gtk.SpinButton spin_encoder_exercise_speed_1rm;
	Gtk.HBox hbox_encoder_exercise_speed_1rm;
	Gtk.Entry entry_encoder_exercise_resistance;
	Gtk.Entry entry_encoder_exercise_description;

	/*
	//used on guiTests
	Gtk.Button button_encoder_capture_curves_all;
	Gtk.Button button_encoder_capture_curves_best;
	Gtk.Button button_encoder_capture_curves_none;
	Gtk.Button button_encoder_capture_curves_4top;
	*/
	Gtk.Button button_encoder_capture_image_save;

	Gtk.Notebook notebook_analyze_results;
	Gtk.Box hbox_combo_encoder_exercise_analyze;
	Gtk.HBox hbox_combo_encoder_laterality_analyze;

	Gtk.Box hbox_combo_encoder_analyze_cross_sup; //includes "Profile" label and the hbox
	Gtk.Box hbox_combo_encoder_analyze_cross;
	Gtk.Box hbox_combo_encoder_analyze_1RM;
	
	Gtk.Box hbox_encoder_analyze_show_powerbars;
	Gtk.CheckButton check_encoder_analyze_show_impulse;
	Gtk.CheckButton check_encoder_analyze_show_time_to_peak_power;
	Gtk.CheckButton check_encoder_analyze_show_range;

	Gtk.HBox hbox_encoder_analyze_individual_groupwise;
	Gtk.HBox hbox_encoder_analyze_instantaneous;
	Gtk.CheckButton check_encoder_analyze_show_position;
	Gtk.CheckButton check_encoder_analyze_show_speed;
	Gtk.CheckButton check_encoder_analyze_show_accel;
	Gtk.CheckButton check_encoder_analyze_show_force;
	Gtk.CheckButton check_encoder_analyze_show_power;
	Gtk.CheckButton checkbutton_encoder_analyze_side_share_x;

	Gtk.Frame frame_encoder_analyze_options;
	Gtk.Grid grid_encoder_analyze_options;
	Gtk.Image image_encoder_analyze_show_SAFE_position;
	Gtk.Image image_encoder_analyze_show_SAFE_speed;
	Gtk.Image image_encoder_analyze_show_SAFE_accel;
	Gtk.Image image_encoder_analyze_show_SAFE_force;
	Gtk.Image image_encoder_analyze_show_SAFE_power;
	
	Gtk.CheckButton checkbutton_crossvalidate;
	Gtk.Button button_encoder_analyze;
	Gtk.Button button_encoder_analyze_mode_options_close_and_analyze;
	Gtk.Box hbox_encoder_analyze_progress;
	Gtk.Button button_encoder_analyze_cancel;
	Gtk.Button button_encoder_analyze_data_select_curves;
	Gtk.Label label_encoder_user_curves_active_num;
	Gtk.Label label_encoder_user_curves_all_num;

	Gtk.VBox vbox_encoder_analyze_instant;
	Gtk.Grid grid_encoder_analyze_instant;
	Gtk.Box grid_encoder_analyze_instant_box_hscale_a;
	Gtk.Box grid_encoder_analyze_instant_box_hscale_b;
	Gtk.HScale hscale_encoder_analyze_a;
	Gtk.CheckButton checkbutton_encoder_analyze_b;
	Gtk.HScale hscale_encoder_analyze_b;
	Gtk.HBox hbox_buttons_scale_encoder_analyze_b;
	Gtk.Label label_encoder_analyze_time_a;
	Gtk.Label label_encoder_analyze_displ_a;
	Gtk.Label label_encoder_analyze_speed_a;
	Gtk.Label label_encoder_analyze_accel_a;
	Gtk.Label label_encoder_analyze_force_a;
	Gtk.Label label_encoder_analyze_power_a;
	Gtk.Label label_encoder_analyze_time_b;
	Gtk.Label label_encoder_analyze_displ_b;
	Gtk.Label label_encoder_analyze_speed_b;
	Gtk.Label label_encoder_analyze_accel_b;
	Gtk.Label label_encoder_analyze_force_b;
	Gtk.Label label_encoder_analyze_power_b;
	Gtk.Label label_encoder_analyze_time_diff;
	Gtk.Label label_encoder_analyze_displ_diff;
	Gtk.Label label_encoder_analyze_speed_diff;
	Gtk.Label label_encoder_analyze_accel_diff;
	Gtk.Label label_encoder_analyze_force_diff;
	Gtk.Label label_encoder_analyze_power_diff;
	Gtk.Label label_encoder_analyze_displ_average;
	Gtk.Label label_encoder_analyze_speed_average;
	Gtk.Label label_encoder_analyze_accel_average;
	Gtk.Label label_encoder_analyze_force_average;
	Gtk.Label label_encoder_analyze_power_average;
	Gtk.Label label_encoder_analyze_displ_max;
	Gtk.Label label_encoder_analyze_speed_max;
	Gtk.Label label_encoder_analyze_accel_max;
	Gtk.Label label_encoder_analyze_force_max;
	Gtk.Label label_encoder_analyze_power_max;
	Gtk.Label label_encoder_analyze_diff;
	Gtk.Label label_encoder_analyze_average;
	Gtk.Label label_encoder_analyze_max;
	Gtk.Button button_encoder_analyze_AB_save;

	Gtk.Button button_encoder_analyze_image_save;
	Gtk.Button button_encoder_analyze_table_save;
	Gtk.Button button_encoder_analyze_1RM_save;

	Gtk.RadioButton radio_encoder_analyze_individual_current_set;
	Gtk.RadioButton radio_encoder_analyze_individual_current_session;
	Gtk.RadioButton radio_encoder_analyze_individual_all_sessions;
	Gtk.RadioButton radio_encoder_analyze_groupal_current_session;

	Gtk.Image image_encoder_analyze_individual_current_set;
	Gtk.Image image_encoder_analyze_individual_current_session;
	Gtk.Image image_encoder_analyze_individual_all_sessions;
	Gtk.Image image_encoder_analyze_groupal_current_session;

	Gtk.HBox hbox_encoder_analyze_current_signal;
	
	Gtk.RadioButton radiobutton_encoder_analyze_powerbars;
	Gtk.RadioButton radiobutton_encoder_analyze_cross;
	Gtk.RadioButton radiobutton_encoder_analyze_1RM;
	Gtk.RadioButton radiobutton_encoder_analyze_instantaneous;
	Gtk.RadioButton radiobutton_encoder_analyze_single;
	Gtk.RadioButton radiobutton_encoder_analyze_side;
	Gtk.RadioButton radiobutton_encoder_analyze_superpose;
	Gtk.RadioButton radiobutton_encoder_analyze_all_set;
	Gtk.RadioButton radiobutton_encoder_analyze_neuromuscular_profile;
	Gtk.Image image_encoder_analyze_powerbars;
	Gtk.Image image_encoder_analyze_cross;
	Gtk.Image image_encoder_analyze_1RM;
	Gtk.Image image_encoder_analyze_instantaneous;
	Gtk.Image image_encoder_analyze_single;
	Gtk.Image image_encoder_analyze_side;
	Gtk.Image image_encoder_analyze_superpose;
	Gtk.Image image_encoder_analyze_all_set;
	Gtk.Image image_encoder_analyze_nmp;
	Gtk.Image image_encoder_analyze_selected_single;
	Gtk.Image image_encoder_analyze_selected_side;
	Gtk.Image image_encoder_analyze_selected_superpose;
	Gtk.Image image_encoder_analyze_selected_all_set;
	Gtk.Label label_encoder_analyze_selected;
	Gtk.HBox hbox_encoder_analyze_intersession;
	Gtk.CheckButton check_encoder_intersession_x_is_date;
	Gtk.CheckButton check_encoder_separate_session_in_days;
	Gtk.HBox hbox_combo_encoder_analyze_weights;
	
	Gtk.Button button_encoder_analyze_neuromuscular_help;


	Gtk.CheckButton check_encoder_analyze_eccon_together;
	Gtk.Image image_encoder_analyze_eccon_together;
	Gtk.Image image_encoder_analyze_eccon_separated;
	
	Gtk.Image image_encoder_analyze_position;
	Gtk.Image image_encoder_analyze_speed;
	Gtk.Image image_encoder_analyze_accel;
	Gtk.Image image_encoder_analyze_force;
	Gtk.Image image_encoder_analyze_power;
	
	Gtk.HBox hbox_encoder_analyze_mean;
	Gtk.HBox hbox_encoder_analyze_max;
	Gtk.Image image_encoder_analyze_mean;
	Gtk.Image image_encoder_analyze_max;
	Gtk.Image image_encoder_analyze_range;
	Gtk.Image image_encoder_analyze_time_to_pp;

	Gtk.Box hbox_encoder_analyze_curve_num;
	Gtk.Box hbox_combo_encoder_analyze_curve_num_combo;
	Gtk.Label label_encoder_analyze_side_max;

	Gtk.CheckButton check_encoder_analyze_mean_or_max;

	Gtk.ScrolledWindow scrolledwindow_image_encoder_analyze;
//	Gtk.Viewport viewport_image_encoder_analyze;
	Gtk.Notebook notebook_encoder_analyze;
	Gtk.Image image_encoder_analyze;
	Gtk.ProgressBar encoder_pulsebar_analyze;
	Gtk.Box box_set_loading;
	Gtk.Spinner spinner_set_loading;
	Gtk.Label label_set_loading;
	Gtk.ProgressBar encoder_pulsebar_load_signal_at_analyze;
	Gtk.Label label_encoder_load_signal_at_analyze;
	
	Gtk.Alignment alignment_treeview_encoder_capture_curves;
	Gtk.Paned hpaned_encoder_capture_current;
	Gtk.TreeView treeview_encoder_capture_curves;
	Gtk.TreeView treeview_encoder_analyze_curves;

	Gtk.DrawingArea encoder_capture_signal_drawingarea_cairo;
	Gtk.DrawingArea encoder_capture_curves_bars_drawingarea_cairo;
	Gtk.DrawingArea drawingarea_encoder_analyze_instant;
	// <---- at glade


	ArrayList encoderCaptureCurves;
        Gtk.ListStore encoderCaptureListStore;
	Gtk.ListStore encoderAnalyzeListStore; //can be EncoderCurves or EncoderNeuromuscularData

	Gtk.ComboBoxText combo_encoder_anchorage;
	Gtk.ComboBoxText combo_encoder_exercise_capture;
	Gtk.ComboBoxText combo_encoder_exercise_analyze;
	Gtk.ComboBoxText combo_encoder_laterality_analyze;
	Gtk.ComboBoxText combo_encoder_analyze_cross;
	Gtk.ComboBoxText combo_encoder_analyze_1RM;
	Gtk.ComboBoxText combo_encoder_analyze_weights;
	Gtk.ComboBoxText combo_encoder_analyze_curve_num_combo;

	bool encoderPreferencesSet = false;

	int image_encoder_width;
	int image_encoder_height;

	private string encoderSelectedAnalysis = "powerBars"; //used to know wich options are selected (cannot be changed during analysis)
	//this two variables are only for naming user-saved encoder analyze image
	private string encoderSendedAnalysis = "";
	private static string encoderLastAnalysis = "";

	private string ecconLast;
	private string encoderTimeStamp;
	private int encoderSignalUniqueID;

	private EncoderAnalyzeInstant eai;

	private ArrayList array1RM;

	EncoderCapture eCapture;
	
	//Contains curves captured to be analyzed by R
	//private static EncoderCaptureCurveArray ecca;
	//private static bool eccaCreated = false;

	private static bool encoderProcessCancel;
	private static bool encoderProcessProblems;
	private static bool encoderProcessFinish;
	private static bool encoderProcessFinishContMode;
	private static Stopwatch encoderCaptureStopwatch;

	private static EncoderRhythmExecute encoderRhythmExecute;
	private static EncoderRhythm encoderRhythm;

	EncoderConfigurationWindow encoder_configuration_win;

	bool firstSetOfCont; //used to don't erase the screen on cont after first set
	bool encoderInertialCalibratedFirstTime; //allow showing the recalibrate button

	private double maxPowerIntersession;
	private double maxSpeedIntersession;
	private double maxForceIntersession;
	private string maxPowerIntersessionDate;
	private string maxSpeedIntersessionDate;
	private string maxForceIntersessionDate;

	/* 
	 * this contains last EncoderSQL captured, recalculated or loaded
	 * 
	 * before using this, saving a curve used the combo values on the top,
	 * but this combo values can be changed by the user, and the he could click on save curve,
	 * then power values (results of curves on graph.R) can be saved with bad weight, exerciseID, …
	 *
	 * Now, with currentEncoderSQLSet, saved curves and export curves will take the weight, exerciseID, …
	 * last capture, recalculate and load. Better usability
	 */
	EncoderSQL currentEncoderSQLSet;

	//EncoderConfiguration encoderConfigurationCurrent; //do not use this, use currentEncoderSQLSet.encoderConfiguration
	//- on capture use this new calculated encoderConfiguration. call it encoderConfigurationNewCapture
	//- if we are working with currentEncoderSQLSet (eg load, recalculate), then use currentEncoderSQLSet.encoderConfiguration
	EncoderConfiguration encoderConfigurationNewCapture;

	// TODO: this should be removed and just use Constants.GetEncoderGIByMode (current_mode) 
	Constants.EncoderGI currentEncoderGI; //store here to not have to check the GUI and have thread problems

	/*
	 * CAPTURE is the capture from csharp (not from external python)
	 *
	 * difference between:
	 * RECALCULATE: recalculate, autosaves the signal at end
	 * LOAD curves does not save at the end?
	 *
	 * CAPTURE_IM records to get the inertia moment but does not calculate curves in R and not updates the treeview
	 * CURVES_AC (After Capture) is like curves but does not start a new thread (uses same pulse as capture)
	 */
	enum encoderActions { CAPTURE_BG, CAPTURE, RECALCULATE, CURVES_AC, LOAD, ANALYZE, CAPTURE_IM, CURVES_IM }
	
	//STOPPING is used to stop the camera. It has to be called only one time
	enum encoderCaptureProcess { CAPTURING, STOPPING, STOPPED } 
	static encoderCaptureProcess capturingCsharp;
	private EncoderCapture.CsharpOrR csharpOrR;

	EncoderRProcCapture encoderRProcCapture;
	EncoderRProcAnalyze encoderRProcAnalyze;

	/* 
	 *
	 * To understand this class threads amnd GUI, see diagram:
	 * encoder-threads.dia
	 *
	 */

	CairoGraphEncoderSignal cairoGraphEncoderSignal;
	static List<PointF> cairoGraphEncoderSignalPoints_l;
	static List<PointF> cairoGraphEncoderSignalInertialPoints_l;

	PrepareEventGraphEncoderCurrent prepareEventGraphEncoderCurrent;

	enum encoderSensEnum { 
		NOSESSION, NOPERSON, YESPERSON, PROCESSINGCAPTURE, PROCESSINGR, DONENOSIGNAL, DONEYESSIGNAL }
	encoderSensEnum encoderSensEnumStored; //tracks how was sensitive before PROCESSINGCAPTURE or PROCESSINGR
	
	//TODO:put zoom,unzoom (at side of delete curve)  in capture curves (for every curve)
	//
	//TODO: capture also with webcam an attach it to signal or curve
	//
	//TODO: peak power in eccentric in absolute values
	//
	//TODO: on cross, spline and force speed and power speed should have a spar value higher, like 0.7. On the other hand, the other cross graphs, haveload(mass) in the X lot more discrete, there is good to put 0.5
	

	private void initEncoder1Time ()
	{
		box_encoder_capture_csharp_r_both.Visible = false; //operatingSystem == UtilAll.OperatingSystems.LINUX;

		encoder_pulsebar_capture.Fraction = 1;
		encoder_countdown_label.Text = "";
		encoder_pulsebar_load_signal_at_analyze.Fraction = 1;
		encoder_pulsebar_load_signal_at_analyze.Text = "";
		encoder_pulsebar_analyze.Fraction = 1;
		encoder_pulsebar_analyze.Text = "";

		//read from SQL
		EncoderConfigurationSQLObject econfSO = SqliteEncoderConfiguration.SelectActive(Constants.EncoderGI.GRAVITATORY);
		encoderConfigurationNewCapture = econfSO.encoderConfiguration;
		setEncoderConfigurationLabels (econfSO.name.ToString (), encoderConfigurationNewCapture.code);
		setEncoderTypePixbuf();
		
		encoderCaptureListStore = new Gtk.ListStore (typeof (EncoderCurve));
		
		encSelReps = new EncoderSelectRepetitions();

		//the glade cursor_changed does not work on mono 1.2.5 windows
		//treeview_encoder_capture_curves.CursorChanged += on_treeview_encoder_capture_curves_cursor_changed;
		//changed, now unselectable because there are the checkboxes

		array1RM = new ArrayList();

		createEncoderCombos();
		
		encoderConfigurationGUIUpdate();
		
		//on start it's concentric and powerbars. Eccon-together should be unsensitive	
		check_encoder_analyze_eccon_together.Sensitive = false;

		//spin_encoder_capture_inertial.Value = Convert.ToDouble(Util.ChangeDecimalSeparator(
		//			SqlitePreferences.Select("inertialmomentum")));
		
		//initialize capture and analyze classes		
		encoderRProcCapture = new EncoderRProcCapture();
		encoderRProcAnalyze = new EncoderRProcAnalyze();
		
		captureCurvesBarsData_l = new List<EncoderBarsData> ();

		LogB.Information("after play 0");
		capturingCsharp = encoderCaptureProcess.STOPPED;
		LogB.Information("after play 1");

		button_encoder_inertial_recalibrate.Visible = false;
		LogB.Information("after play 2");
		encoderInertialCalibratedFirstTime = false; //allow show the recalibrate button
		LogB.Information("after play 3");
		LogB.Information("after play 4");

		//configInit();
	
		//triggers
		triggerListEncoder = new TriggerList();
		LogB.Information("after play 5");
		showEncoderAnalyzeTriggerTab(false);
		LogB.Information("after play 6");

		followSignals = false;
		check_encoder_capture_table.Active = preferences.encoderCaptureShowOnlyBars.ShowTable;
		check_encoder_capture_signal.Active = preferences.encoderCaptureShowOnlyBars.ShowSignal;

		updateGraphEncoderSessionBars ();
		followSignals = true;
	}

	void on_button_encoder_select_clicked (object o, EventArgs args)
	{
		encoder_configuration_win = EncoderConfigurationWindow.View(
				currentEncoderGI, SqliteEncoderConfiguration.SelectActive(currentEncoderGI),
				UtilGtk.ComboGetActive(combo_encoder_anchorage),
				(int) spin_encoder_im_weights_n.Value, //used on inertial
				true); 		//allow to calcule IM on inertial

		encoder_configuration_win.Button_close.Clicked += new EventHandler (on_encoder_configuration_win_app1_closed);

		//unregister eventHandler first, then register. This avoids to have registered twice
		try {
			encoder_configuration_win.Button_encoder_capture_inertial_do.Clicked -= 
				new EventHandler(on_encoder_configuration_win_capture_inertial_do);
		} catch { }
		encoder_configuration_win.Button_encoder_capture_inertial_do.Clicked += 
			new EventHandler(on_encoder_configuration_win_capture_inertial_do);

		encoder_configuration_win.Button_encoder_capture_inertial_cancel.Clicked += 
			new EventHandler(on_button_encoder_cancel_clicked);
	}

	// different than when called edit_event
	void on_encoder_configuration_win_app1_closed (object o, EventArgs args)
	{
		encoder_configuration_win.Button_close.Clicked -= new EventHandler (on_encoder_configuration_win_app1_closed);
		
		EncoderConfiguration eConfNew = encoder_configuration_win.GetAcceptedValues();

		if (encoderConfigurationNewCapture == eConfNew)
			return;
			
		bool combo_encoder_anchorage_should_update = (encoderConfigurationNewCapture.list_d != eConfNew.list_d);
		
		encoderConfigurationNewCapture = eConfNew;

		EncoderConfigurationSQLObject econfSO;
		if (current_mode == Constants.Modes.POWERGRAVITATORY)
			econfSO = SqliteEncoderConfiguration.SelectActive (Constants.EncoderGI.GRAVITATORY);
		else
			econfSO = SqliteEncoderConfiguration.SelectActive (Constants.EncoderGI.INERTIAL);

		setEncoderConfigurationLabels (econfSO.name.ToString (), encoderConfigurationNewCapture.code);

		LogB.Information("encoderConfigurationNewCapture = " + encoderConfigurationNewCapture.ToStringOutput(EncoderConfiguration.Outputs.SQL));
		setEncoderTypePixbuf();
	
		encoderGuiChangesAfterEncoderConfigurationWin(combo_encoder_anchorage_should_update);
	}
	void encoderGuiChangesAfterEncoderConfigurationWin (bool combo_encoder_anchorage_should_update) 
	{
		if (encoderConfigurationNewCapture.has_inertia)
		{
			if (combo_encoder_anchorage_should_update) {
				UtilGtk.ComboUpdate (combo_encoder_anchorage, encoderConfigurationNewCapture.list_d.L);
				combo_encoder_anchorage.Active = UtilGtk.ComboMakeActive (
						combo_encoder_anchorage,
						encoderConfigurationNewCapture.d.ToString()
						);
			}

			encoderConfigurationNewCapture.extraWeightN = (int) spin_encoder_im_weights_n.Value; 
			encoderConfigurationNewCapture.inertiaTotal = UtilEncoder.CalculeInertiaTotal (encoderConfigurationNewCapture);
			label_encoder_im_total.Text = encoderConfigurationNewCapture.inertiaTotal.ToString();
			label_encoder_top_im.Text = Catalog.GetString("Inertia M.") + ": " + label_encoder_im_total.Text;

			label_encoder_equivalent_mass.Text = Util.TrimDecimals (UtilEncoder.CalculateEquivalentMass (encoderConfigurationNewCapture), 1);
		}
	}
	
	void on_combo_encoder_anchorage_changed (object o, EventArgs args)
	{
		string selected = UtilGtk.ComboGetActive(combo_encoder_anchorage);
		if(selected != "" && Util.IsNumber(selected, true))
			encoderConfigurationNewCapture.d = Convert.ToDouble(selected);

		label_encoder_equivalent_mass.Text = Util.TrimDecimals (UtilEncoder.CalculateEquivalentMass (encoderConfigurationNewCapture), 1);
	}


	// ---- start of spin_encoder_im_weights_n ---->
	
	//add-remove weights on encoder inertial using '+', '-'
	private void on_fake_button_encoder_exercise_im_weights_n_plus_clicked(object o, EventArgs args)
	{
		spin_encoder_im_weights_n.Value += 1;
	}
	private void on_fake_button_encoder_exercise_im_weights_n_minus_clicked(object o, EventArgs args)
	{
		spin_encoder_im_weights_n.Value -= 1;
	}

	void on_spin_encoder_im_weights_n_value_changed (object o, EventArgs args)
	{
		encoderConfigurationNewCapture.extraWeightN = (int) spin_encoder_im_weights_n.Value; 
		encoderConfigurationNewCapture.inertiaTotal = UtilEncoder.CalculeInertiaTotal (encoderConfigurationNewCapture);
		label_encoder_im_total.Text = encoderConfigurationNewCapture.inertiaTotal.ToString();
		label_encoder_top_im.Text = Catalog.GetString("Inertia M.") + ": " + label_encoder_im_total.Text;

		label_encoder_top_weights.Text = spin_encoder_im_weights_n.Value.ToString ();

		label_encoder_equivalent_mass.Text = Util.TrimDecimals (UtilEncoder.CalculateEquivalentMass (encoderConfigurationNewCapture), 1);
	}

	// <---- end of spin_encoder_im_weights_n ----
	

	
	void on_encoder_configuration_win_capture_inertial_do (object o, EventArgs args) 
	{
		on_button_encoder_capture_calcule_im();
	}
	
	
	private void on_button_encoder_bells_clicked(object o, EventArgs args)
	{
		feedbackWin.View(getBellMode(current_mode), preferences, encoderRhythm, true);
	}

	/*
	private bool encoderCheckPort()	
	{
		if(File.Exists(Util.GetECapSimSignalFileName())) //simulatedEncoder
			return true;

		string port = chronopicWin.GetEncoderPort();
		string errorMessage = "";

		if( port == null || port == "" || port == Util.GetDefaultPort() )
			errorMessage = "Chronopic port is not configured";
		else if( ! UtilAll.IsWindows() )
		       if( ! File.Exists(port) )
				errorMessage = "Chronopic has been disconnected";


		if(errorMessage != "") {
			LogB.Warning(errorMessage);
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString(errorMessage));
			createChronopicWindow(true, "");
			return false;
		}

		return true;
	}
	*/

	// find best historical values for feedback on meanPower, meanSpeed, meanForce
	// called on encoderActions.CAPTURE,  encoderActions.CURVES_AC encoderConfiguration will be encoderConfigurationNewCapture 
	// called on encoderActions.LOAD,  encoderActions.RECALCULATE encoderConfiguration will be currentEncoderSQLSet.encoderConfiguration
	private void findMaxPowerSpeedForceIntersession (int exerciseID, EncoderConfiguration encoderConfiguration, string laterality, double extraWeight)
	{
		//finding historical maxPower of a person in an exercise
		Constants.EncoderGI encGI = getEncoderGI();
		ArrayList arrayTemp = SqliteEncoder.Select(false, -1, currentPerson.UniqueID, -1, encGI,
					exerciseID, "curve",
					EncoderSQL.Eccons.ALL, laterality,
					false, false, false);

		maxPowerIntersession = 0;
		maxSpeedIntersession = 0;
		maxForceIntersession = 0;
		maxPowerIntersessionDate = "";
		maxSpeedIntersessionDate = "";
		maxForceIntersessionDate = "";

		//TODO: do a regression to find maxPower with a value of extraWeight unused
		if(encGI == Constants.EncoderGI.INERTIAL)
			extraWeight = 0;

		foreach(EncoderSQL es in arrayTemp)
		{
			if(
					( encGI == Constants.EncoderGI.GRAVITATORY &&
					 es.repCriteria == preferences.encoderRepetitionCriteriaGravitatory &&
					 Util.SimilarDouble(Convert.ToDouble(Util.ChangeDecimalSeparator(es.extraWeight)), extraWeight) ) ||
					( encGI == Constants.EncoderGI.INERTIAL &&
					 es.repCriteria == preferences.encoderRepetitionCriteriaInertial &&
					 encoderConfiguration.Equals (es.encoderConfiguration) )
			  ) {
				if(Convert.ToDouble(es.meanPower) > maxPowerIntersession)
				{
					maxPowerIntersession = Convert.ToDouble(es.meanPower);
					maxPowerIntersessionDate = es.GetDateStr();
				}
				if(Convert.ToDouble(es.meanSpeed) > maxSpeedIntersession)
				{
					maxSpeedIntersession = Convert.ToDouble(es.meanSpeed);
					maxSpeedIntersessionDate = es.GetDateStr();
				}
				if(Convert.ToDouble(es.meanForce) > maxForceIntersession)
				{
					maxForceIntersession = Convert.ToDouble(es.meanForce);
					maxForceIntersessionDate = es.GetDateStr();
				}
			}
		}

		//LogB.Information(string.Format("maxPowerIntersession: {0}, date: {1}",
		//			maxPowerIntersession, maxPowerIntersessionDate));
	}

	bool canCaptureEncoder()
	{
		if (Config.SimulatedCapture)
			return true;

		chronopicRegisterUpdate(false);

		//need to restore selectedForMode_l after the chronopicRegisterUpdate
		if (chronopicRegister.GetSelectedForMode (current_mode).Port == "")
			chronopicRegister.SetAnyCompatibleConnectedAsSelected (current_mode);

		int numEncoders = chronopicRegister.NumConnectedOfType(ChronopicRegisterPort.Types.ENCODER);
		LogB.Information("numEncoders: " + numEncoders);
		if(numEncoders == 0)
		{
			//show viewport chronopic encoder with a color
			UtilGtk.DeviceColors(viewport_chronopic_encoder, false);

			//if networks (compujump) show the label and image of missing
			if(configChronojump.Compujump)
				networksShowDeviceMissingEncoder(true);
			else {
				/*
				 * if not on networks (compujump): open device window.
				 * this is not done on networks because we prefer that a responsible
				 * manages correctly the two devices (encoder and rfid)
				 * and this responsible first need to "gain permission" on preferences/advanced
				 */
				on_chronopic_encoder_clicked(new object(), new EventArgs());
			}

			return false;
		}
		if(numEncoders > 1) {
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("More than 1 encoders are connected"));
			UtilGtk.DeviceColors(viewport_chronopic_encoder, false);
			return false;
		}

		if(configChronojump.Compujump)
			networksShowDeviceMissingEncoder(false);

		UtilGtk.DeviceColors(viewport_chronopic_encoder, true);
		return true;
	}

	EncoderCaptureInertialBackground eCaptureInertialBG; //only created one time
	void on_button_encoder_inertial_calibrate_clicked (object o, EventArgs args)
	{
		/*
		 * only call canCaptureEncoder() if we are not capturing in the background
		 * this avoids problems with recalibrate on windows (port gets missing)
		 */
		if( (encoderThreadBG == null || ! encoderThreadBG.IsAlive) && ! canCaptureEncoder() )
			return;

		//allow show the recalibrate button
		encoderInertialCalibratedFirstTime = true;
		label_calibrate_output_message.Text = Catalog.GetString("Calibrated");

		/*
		 * if user calibrates again: put 0 value
		 * if calibration was not running: start it
		 */
		if(encoderThreadBG != null && encoderThreadBG.IsAlive)
			eCaptureInertialBG.AngleNow = 0;
		else
			encoderThreadStart(encoderActions.CAPTURE_BG);
	}
	void on_button_encoder_inertial_recalibrate_clicked (object o, EventArgs args)
	{
		prepareForEncoderInertiaCalibrate();
	}
	void prepareForEncoderInertiaCalibrate()
	{
		sensitiveGuiEventDoing(preferences.encoderCaptureInfinite);
		button_encoder_inertial_calibrate.Sensitive = true;
		button_encoder_inertial_calibrate_close.Sensitive = true;
		label_wait.Text = " ";
		label_calibrate_output_message.Text = "";

		vbox_encoder_bars_table_and_save_reps.Visible = false;
		vbox_inertial_instructions.Visible = true;
	}

	private void on_button_encoder_inertial_calibrate_close_clicked (object o, EventArgs args)
	{
		vbox_encoder_bars_table_and_save_reps.Visible = true;
		vbox_inertial_instructions.Visible = false;

		sensitiveGuiEventDone();
	}

	private void setEncoderExerciseOptionsFromPreferences()
	{
		Sqlite.Open();

		//1. exercise
		string exerciseID = "";
		if(current_mode == Constants.Modes.POWERGRAVITATORY)
			exerciseID = SqlitePreferences.Select(SqlitePreferences.EncoderExerciseIDGravitatory, true);
		else //(current_mode == Constants.Modes.POWERINERTIAL)
			exerciseID = SqlitePreferences.Select(SqlitePreferences.EncoderExerciseIDInertial, true);

		string exerciseNameTranslated = Util.FindOnArray(':', 0, 2, exerciseID.ToString(),
				encoderExercisesTranslationAndBodyPWeight);

		/*
		 * close/open db because "combo_encoder_exercise_capture.Active" changing will call:
		 * void on_combo_encoder_exercise_capture_changed (object o, EventArgs args)
		 * and this will call array1RMUpdate() that will close/open SQL
		 */
		Sqlite.Close();
		combo_encoder_exercise_capture.Active = UtilGtk.ComboMakeActive(
				combo_encoder_exercise_capture, exerciseNameTranslated);
		Sqlite.Open();

		//2 contraction
		string contraction = "";
		if(current_mode == Constants.Modes.POWERGRAVITATORY)
			contraction = SqlitePreferences.Select(SqlitePreferences.EncoderContractionGravitatory, true);
		else //(current_mode == Constants.Modes.POWERINERTIAL)
			contraction = SqlitePreferences.Select(SqlitePreferences.EncoderContractionInertial, true);

		if(contraction == Constants.Concentric)
			radio_encoder_eccon_concentric.Active = true;
		else
			radio_encoder_eccon_eccentric_concentric.Active = true;

		//3 laterality
		string laterality = "";
		if(current_mode == Constants.Modes.POWERGRAVITATORY)
			laterality = SqlitePreferences.Select(SqlitePreferences.EncoderLateralityGravitatory, true);
		else //(current_mode == Constants.Modes.POWERINERTIAL)
			laterality = SqlitePreferences.Select(SqlitePreferences.EncoderLateralityInertial, true);

		if(laterality == "RL")
			radio_encoder_laterality_both.Active = true;
		else if(laterality == "R")
			radio_encoder_laterality_r.Active = true;
		else //if(laterality == "L")
			radio_encoder_laterality_l.Active = true;

		/*
		   Do not use this SqlitePreferences rows because this fields are on encoderConfiguration table (Active)
		//4 mass / weights
		string mass = SqlitePreferences.Select(SqlitePreferences.EncoderMassGravitatory, true);
		spin_encoder_extra_weight.Value = Convert.ToDouble(Util.ChangeDecimalSeparator(mass));

		string weights = SqlitePreferences.Select(SqlitePreferences.EncoderWeightsInertial, true);
		entry_encoder_im_weights_n.Text = weights;
		*/


		Sqlite.Close();
	}

	private void saveEncoderExerciseOptionsToPreferences()
	{
		//store execution params on SQL for next Chronojump start
		Sqlite.Open();

		//1 exercise
		int exerciseID = getExerciseIDFromEncoderCombo (exerciseCombos.CAPTURE);
		if(current_mode == Constants.Modes.POWERGRAVITATORY)
			SqlitePreferences.Update (SqlitePreferences.EncoderExerciseIDGravitatory, exerciseID.ToString(), true);
		else //(current_mode == Constants.Modes.POWERINERTIAL)
			SqlitePreferences.Update (SqlitePreferences.EncoderExerciseIDInertial, exerciseID.ToString(), true);

		//2 contraction
		string eccon = Constants.Concentric;
		if(radio_encoder_eccon_eccentric_concentric.Active)
			eccon = Constants.EccentricConcentric;

		if(currentEncoderGI == Constants.EncoderGI.GRAVITATORY)
			SqlitePreferences.Update (SqlitePreferences.EncoderContractionGravitatory, eccon, true);
		else
			SqlitePreferences.Update (SqlitePreferences.EncoderContractionInertial, eccon, true);

		//3 laterality
		string laterality = getLateralityFromGui(true);

		if(current_mode == Constants.Modes.POWERGRAVITATORY)
			SqlitePreferences.Update (SqlitePreferences.EncoderLateralityGravitatory, laterality, true);
		else //(current_mode == Constants.Modes.POWERINERTIAL)
			SqlitePreferences.Update (SqlitePreferences.EncoderLateralityInertial, laterality, true);

		/*
		   Do not use this SqlitePreferences rows because this fields are on encoderConfiguration table (Active)
		//4 mass / weights
		if(current_mode == Constants.Modes.POWERGRAVITATORY)
			SqlitePreferences.Update (SqlitePreferences.EncoderMassGravitatory,
					Util.ConvertToPoint(findMass(Constants.MassType.EXTRA)), //when save on sql, do not include person weight
					true);
		else //(current_mode == Constants.Modes.POWERINERTIAL)
			SqlitePreferences.Update (SqlitePreferences.EncoderWeightsInertial,
					spin_encoder_im_weights_n.Value.ToString(),
					true);
		*/

		Sqlite.Close();

	}

	//called from main GUI
	void on_button_encoder_capture_clicked (object o, EventArgs args) 
	{
		if(! selectedEncoderExerciseExists())
		{
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Need to create/select an exercise."));
			return;
		}

		saveEncoderExerciseOptionsToPreferences();

		if (! Config.SimulatedCapture && chronopicRegister.GetSelectedForMode (current_mode).Port == "")
			on_button_detect_clicked (o, args); //open discover win
		else
			on_button_encoder_capture_clicked_do (true);
	}

	void on_button_encoder_capture_clicked_do (bool firstSet)
	{
//		if(eCaptureInertialBG != null)
//			eCaptureInertialBG.Finish();

		csharpOrR = EncoderCapture.CsharpOrR.R;
		/*
		 * Once fixed the capture on Silicon the probem was the need of a buffer, then the encoder C# (EncoderLikeR) is not needed anymore.
		if (UtilAll.IsLinux ())
		{
			if (radio_encoder_capture_csharp.Active)
				csharpOrR = EncoderCapture.CsharpOrR.CSHARP;
			else if (radio_encoder_capture_both.Active)
				csharpOrR = EncoderCapture.CsharpOrR.BOTH;
		}
		*/

		firstSetOfCont = firstSet;

		findMaxPowerSpeedForceIntersession (
				getExerciseIDFromEncoderCombo (exerciseCombos.CAPTURE),
				encoderConfigurationNewCapture,
				getLateralityFromGui (true),
				Convert.ToDouble (spin_encoder_extra_weight.Value));
		//LogB.Information("maxPower: " + maxPowerIntersession);

		if(encoderThreadBG != null && encoderThreadBG.IsAlive) //if we are capturing on the background …
		{
			// stop capturing on the background if we start capturing gravitatory
			if(! encoderConfigurationNewCapture.has_inertia)
			{
				stopCapturingInertialBG();
			}
		}
		else //if we are NOT capturing on the background …
		{
			//check if chronopics have changed
			/*
			if(! canCaptureEncoder() )
				return;
				*/
			chronopicRegister.ListSelectedForAllModes (); //debug
			if (! Config.SimulatedCapture && chronopicRegister.GetSelectedForMode (current_mode).Port == "")
			{
				if (! configChronojump.Compujump)
					on_button_detect_clicked (new object (), new EventArgs ()); //open discover win
			}

			if(encoderConfigurationNewCapture.has_inertia)
			{
				prepareForEncoderInertiaCalibrate();
				return;
			}
		}

		sensitiveGuiEventDoing(preferences.encoderCaptureInfinite);

		cairoGraphEncoderSignal = null;
		cairoGraphEncoderSignalPoints_l = new List<PointF>();
		cairoGraphEncoderSignalInertialPoints_l = new List<PointF>();

		LogB.Debug("Calling encoderThreadStart for capture");

		//record this encoderConfiguration to SQL for next Chronojump open
		SqliteEncoderConfiguration.UpdateActive(false, currentEncoderGI, encoderConfigurationNewCapture);

		encoderProcessFinish = false;
		CairoPaintBarsPreEncoderCurrent.RepetitionsPlayed_l = new List<int> ();

		if (preferences.encoderFeedbackAsteroidsActive)
			asteroids = new Asteroids (
					preferences.forceSensorFeedbackAsteroidsMax,
					preferences.forceSensorFeedbackAsteroidsMin,
					preferences.forceSensorFeedbackAsteroidsDark,
					preferences.forceSensorFeedbackAsteroidsFrequency,
					preferences.forceSensorFeedbackShotsFrequency,
					false, preferences.encoderCaptureTime); //not micros (encoder goes in millis)

		encoderThreadStart(encoderActions.CAPTURE);

		LogB.Debug("end of Calling encoderThreadStart for capture");
	}

	void on_button_encoder_capture_calcule_im () 
	{
		//check if chronopics have changed
		if(! canCaptureEncoder())
			return;

		encoder_configuration_win.Button_encoder_capture_inertial_do_chronopic_ok();
		encoder_configuration_win.Label_capture_time(
				preferences.encoderCaptureTimeIM,
				EncoderCaptureIMCalc.InactivityEndTime);

		encoderProcessFinish = false;
		encoderThreadStart(encoderActions.CAPTURE_IM);
	}


	private void on_combo_encoder_exercise_capture_changed (object o, EventArgs args)
	{
		if(UtilGtk.ComboGetActive(combo_encoder_exercise_capture) != "") { //needed because encoder_exercise_edit updates this combo and can be without values in the changing process
			array1RMUpdate(false);
			encoder_change_displaced_weight_and_1RM ();
			label_encoder_top_exercise.Text = UtilGtk.ComboGetActive(combo_encoder_exercise_capture);
			radio_contacts_graph_currentTest.Label =  UtilGtk.ComboGetActive (combo_encoder_exercise_capture);

			//update session treeview, session barplot, blank current set graphs, current set treeview
	                pre_fillTreeView_resultsSession ();
			updateGraphResultsSessionByMode ();
			blankEncoderCurrentSetGraphs ();
			treeviewEncoderCaptureRemoveColumns ();

			//sensitivity of left/right buttons
			button_combo_encoder_exercise_capture_left.Sensitive = (combo_encoder_exercise_capture.Active > 0);
			button_combo_encoder_exercise_capture_right.Sensitive = ! UtilGtk.ComboSelectedIsLast(combo_encoder_exercise_capture);

			button_encoder_exercise_edit.Sensitive = true;
			button_encoder_exercise_delete.Sensitive = true;
		} else {
			label_encoder_top_exercise.Text = "";
			button_combo_encoder_exercise_capture_left.Sensitive = false;
			button_combo_encoder_exercise_capture_right.Sensitive = false;

			button_encoder_exercise_edit.Sensitive = false;
			button_encoder_exercise_delete.Sensitive = false;
		}
	}
	
	private bool comboEncoderNoFollow;
	void on_combo_encoder_exercise_analyze_changed (object o, EventArgs args)
	{
		if (comboEncoderNoFollow)
			return;

		prepareAnalyzeRepetitions ();
	}

	// ---- change extra weight start ----
	/*
	 * when spin is seen the others (-10, -1, entry, +1, +10) are not seen
	 * -10, -1, 1, +10 change the entry
	 * entry changes de spin
	 * spin does not change anything
	 */

	//add-remove weights on encoder gravitatory using '+', '-'
	private void on_fake_button_encoder_exercise_weight_plus_clicked(object o, EventArgs args)
	{
		on_button_encoder_raspberry_extra_weight_plus_1_clicked (new object (), new EventArgs ());
	}
	private void on_fake_button_encoder_exercise_weight_minus_clicked(object o, EventArgs args)
	{
		on_button_encoder_raspberry_extra_weight_minus_1_clicked (new object (), new EventArgs ());
	}
	
	void on_button_encoder_raspberry_extra_weight_minus_10_clicked (object o, EventArgs args) {
		encoderCaptureChangeExtraWeight(-10);
	}
	void on_button_encoder_raspberry_extra_weight_minus_1_clicked (object o, EventArgs args) {
		encoderCaptureChangeExtraWeight(-1);
	}
	void on_button_encoder_raspberry_extra_weight_plus_10_clicked (object o, EventArgs args) {
		encoderCaptureChangeExtraWeight(+10);
	}
	void on_button_encoder_raspberry_extra_weight_plus_1_clicked (object o, EventArgs args) {
		encoderCaptureChangeExtraWeight(+1);
	}
	void encoderCaptureChangeExtraWeight(int change)
	{
		double newValue = spin_encoder_extra_weight.Value + change;

		double min, max;
		spin_encoder_extra_weight.GetRange(out min, out max);
		if(newValue < min)
			spin_encoder_extra_weight.Value = min;
		else if(newValue > max)
			spin_encoder_extra_weight.Value = max;
		else
			spin_encoder_extra_weight.Value = newValue;
	}

	void on_spin_encoder_extra_weight_value_changed (object o, EventArgs args) 
	{
		//don't need to:
		//array1RMUpdate(false);
		//because then we will be calling SQL at each spinbutton increment

		encoder_change_displaced_weight_and_1RM ();

		label_encoder_top_extra_mass.Text = Util.TrimDecimals(spin_encoder_extra_weight.Value, 2) + " kg";
	}

	void encoder_change_displaced_weight_and_1RM () 
	{
		//displaced weight
		label_encoder_displaced_weight.Text = Util.TrimDecimals (findMassFromGui (Constants.MassType.DISPLACED),2);

		double load1RM = 0;
		if(array1RM.Count > 0)
			load1RM = ((Encoder1RM) array1RM[0]).load1RM; //take only the first in array (will be the last uniqueID)

		if(load1RM == 0 || findMassFromGui (Constants.MassType.EXTRA) == 0)
		{
			label_encoder_1RM_percent.Text = "";
			label_encoder_top_1RM_percent.Text = "";
		}
		else
		{
			label_encoder_1RM_percent.Text = Util.TrimDecimals(
					(100 * findMassFromGui (Constants.MassType.EXTRA) / ( load1RM * 1.0 )).ToString(), 1);
			label_encoder_top_1RM_percent.Text = label_encoder_1RM_percent.Text + " %1RM";
		}
	}
	
	// ---- end of change extra weight ----
	


	//array1RM variable is not local because we need to perform calculations at each change on displaced_weight
	void array1RMUpdate (bool returnPersonNameAndExerciseName) 
	{
		if(currentPerson != null)
			array1RM = SqliteEncoder1RM.Select1RM(
					false, currentPerson.UniqueID, -1, //-1: currentSession = all sessions
					getExerciseIDFromEncoderCombo(exerciseCombos.CAPTURE), returnPersonNameAndExerciseName);
	}

	void on_button_encoder_1RM_win_clicked (object o, EventArgs args) 
	{
		array1RMUpdate(true);
		
		ArrayList dataPrint = new ArrayList();
		foreach(Encoder1RM e1RM in array1RM) {
			dataPrint.Add(e1RM.ToStringArray2());
		}

		string [] columnsString = {
			"ID",
			Catalog.GetString("Person"),
			Catalog.GetString("Exercise"),
			Catalog.GetString("Load 1RM"),
			Catalog.GetString("Session date")
		};

		ArrayList bigArray = new ArrayList();
		ArrayList a1 = new ArrayList();
		ArrayList a2 = new ArrayList();
		ArrayList a3 = new ArrayList();

		//0 is the widgget to show; 1 is the editable; 2 id default value
		a1.Add(Constants.GenericWindowShow.TREEVIEW); a1.Add(true); a1.Add("");
		bigArray.Add(a1);
		
		a2.Add(Constants.GenericWindowShow.HBOXSPINDOUBLE2); a2.Add(true); a2.Add("");
		bigArray.Add(a2);
	
		a3.Add(Constants.GenericWindowShow.BUTTONMIDDLE); a3.Add(true); a3.Add("");
		bigArray.Add(a3);
	
		genericWin = GenericWindow.Show("1RM", false,	//don't show now
				string.Format(Catalog.GetString("Saved 1RM values of athlete {0} in {1} exercise."), 
					currentPerson.Name, UtilGtk.ComboGetActive(combo_encoder_exercise_capture)) + "\n" + 
				Catalog.GetString("If you want to delete a row, right click on it.") + "\n" + 
				Catalog.GetString("If there is more than one value, top one will be used."),
				bigArray);

		genericWin.SetTreeview(columnsString, false, dataPrint, new ArrayList(), GenericWindow.EditActions.DELETE, false);
		genericWin.LabelSpinDouble2 = Catalog.GetString("Manually add");
		genericWin.SetSpinDouble2Increments(0.1,1);
		genericWin.SetSpinDouble2Range(0,5000);
		genericWin.SetButtonMiddleLabel(Catalog.GetString("Add 1RM value"));
	
		//find all persons in current session
		ArrayList personsPre = SqlitePersonSession.SelectCurrentSessionPersons(
				currentSession.UniqueID,
				false); //means: do not returnPersonAndPSlist

		string [] persons = new String[personsPre.Count];
		int count = 0;
	        foreach	(Person p in personsPre)
			persons[count++] = p.UniqueID.ToString() + ":" + p.Name;
		
		genericWin.ShowButtonCancel(false);
		genericWin.SetButtonAcceptSensitive(true);
		genericWin.SetButtonCancelLabel(Catalog.GetString("Close"));
		//manage selected, unselected curves
		genericWin.Button_middle.Clicked -= new EventHandler(on_encoder_1RM_win_row_added);
		genericWin.Button_middle.Clicked += new EventHandler(on_encoder_1RM_win_row_added);
		
		genericWin.Button_accept.Clicked += new EventHandler(on_spin_encoder_extra_weight_value_changed);
		
		genericWin.Button_row_delete.Clicked -= new EventHandler(on_encoder_1RM_win_row_delete);
		genericWin.Button_row_delete.Clicked += new EventHandler(on_encoder_1RM_win_row_delete);

		//used when we don't need to read data, 
		//and we want to ensure next window will be created at needed size
		//genericWin.DestroyOnAccept=true;
		//here is comented because we are going to read the checkboxes

		genericWin.ShowNow();
	}

	private void on_encoder_1RM_win_row_added (object o, EventArgs args) 
	{
		LogB.Information("row adding at encoder 1RM");
		
		double d = genericWin.SpinDouble2Selected;
		int uniqueID = SqliteEncoder1RM.Insert1RM(false, currentPerson.UniqueID, currentSession.UniqueID, 
				getExerciseIDFromEncoderCombo (exerciseCombos.CAPTURE), genericWin.SpinDouble2Selected);

		genericWin.Row_add_beginning_or_end (new string[] {
				uniqueID.ToString(), currentPerson.Name, UtilGtk.ComboGetActive(combo_encoder_exercise_capture),
				d.ToString(), currentSession.DateShort
				}, true
				);
		
		array1RMUpdate(false);
		encoder_change_displaced_weight_and_1RM ();
		
		LogB.Information("row added at encoder 1RM");
	}

	protected void on_encoder_1RM_win_row_delete (object o, EventArgs args) {
		LogB.Information("row delete at encoder 1RM");

		int uniqueID = genericWin.TreeviewSelectedUniqueID;
		LogB.Information(uniqueID.ToString());

		Sqlite.Delete(false, Constants.Encoder1RMTable, Convert.ToInt32(uniqueID));
		
		array1RMUpdate(false);
		encoder_change_displaced_weight_and_1RM ();
		
		genericWin.Delete_row_accepted();
	}
	
	//action can be CURVES_AC (After Capture) (where signal does not exists, need to define it)
	//RECALCULATE, LOAD (signal is defined)
	void encoderCalculeCurves (encoderActions action)
	{
		if(action == encoderActions.CURVES_AC) 
		{
			encoderTimeStamp = UtilDate.ToFile(DateTime.Now);
			encoderSignalUniqueID = -1; //mark to know that there's no ID for this until it's saved on database
			encoderThreadStart(action);
		} else {
			//curves_ac & recalculate saves the curve at end
			//load does not save the curve 
		       if(File.Exists(UtilEncoder.GetEncoderDataTempFileName()))
			       encoderThreadStart (action);
		       else {
			       event_execute_label_message.Text = Catalog.GetString("Missing data.");
			       fullscreen_label_message.Text = Catalog.GetString("Missing data.");
		       }
		}
	}

	
	void on_button_encoder_cancel_clicked (object o, EventArgs args) 
	{
		Button buttonClicked = o as Button;
		if (o == null)
			return;

		fullscreenLastCapture = (buttonClicked == fullscreen_capture_button_cancel);

		if (blinkCapture != null)
			blinkCapture.End ();
		showHideBlinkIcon (blinkCapture, false);

		eCapture.Cancel();
	}

	void on_button_encoder_analyze_cancel_clicked (object o, EventArgs args)
	{
		encoderProcessCancel = true;
	}

	void on_button_encoder_capture_finish_clicked (object o, EventArgs args) 
	{
		Button buttonClicked = o as Button;
		if (o == null)
			return;

		fullscreenLastCapture = (buttonClicked == fullscreen_capture_button_finish ||
				buttonClicked == fullscreen_button_encoder_capture_finish_cont);

		eCapture.Finish();
		encoderProcessFinish = true;
	}
	//finish without pressing finish button. store fullScreenLastCapture variable
	private void on_encoder_capture_finish_by_time (object o, EventArgs args)
	{
		if (notebook_start.CurrentPage == Convert.ToInt32 (notebook_start_pages.FULLSCREENCAPTURE))
		{
			fullscreenLastCapture = true;

			//exit fullscreen except if we are on capture infinite
			if (! preferences.encoderCaptureInfinite)
				fullscreen_button_fullscreen_exit.Click ();
		} else
			fullscreenLastCapture = false;
	}

	void on_button_encoder_capture_finish_cont_clicked (object o, EventArgs args) 
	{
		encoderProcessFinishContMode = true;
		on_button_encoder_capture_finish_clicked (o, args); 
	}

	private void on_check_encoder_capture_show_modes_clicked (object o, EventArgs args)
	{
		if(! followSignals)
			return;

		alignment_treeview_encoder_capture_curves.Visible = check_encoder_capture_table.Active;
		encoder_capture_signal_drawingarea_cairo.Visible = check_encoder_capture_signal.Active;
		vbox_encoder_bars_table_and_save_reps.Visible = true;
		vbox_contacts_capture_graph.Visible = check_contacts_capture_graph.Active;

		scrolledwindow_treeview_results_session.Visible = check_contacts_capture_table.Active;
		box_results_session_zoom.Visible = check_contacts_capture_table.Active;

		fixEncoderCaptureWidgetsGeometry ();

		/*
		   update the preferences variable
		   note as can be changed while capturing, it will be saved to SQL on exit
		   to not have problems with SQL while capturing
		   */
		preferences.encoderCaptureShowOnlyBars = new EncoderCaptureDisplay (
				check_encoder_capture_signal.Active,
				check_encoder_capture_table.Active,
				true); //bars
	}

	private void fixEncoderCaptureWidgetsGeometry ()
	{
		GLib.Timeout.Add (50, new GLib.TimeoutHandler (encoder2ndRowPos));
	}

	/*
	private bool encoder1stRowAllHeight () //done later in order to have table and/or signal hidden
	{
		vpaned_encoder_main.Position = vpaned_encoder_main.MaxPosition;
		return false;
	}
	*/

	private bool encoder2ndRowPos ()
	{
		if (! check_encoder_capture_signal.Active && ! check_encoder_capture_table.Active)
		{
			hpaned_encoder_capture_current.Position = hpaned_encoder_capture_current.MaxPosition;
			return false;
		}

		if (current_mode == Constants.Modes.POWERGRAVITATORY)
			hpaned_encoder_capture_current.Position = Convert.ToInt32 (
					hpaned_encoder_capture_current.Allocation.Width / 2.0);
		else //if (current_mode == Constants.Modes.POWERINERTIAL)
			hpaned_encoder_capture_current.Position = Convert.ToInt32 (
					(hpaned_encoder_capture_current.Allocation.Width +
					 vbox_angle_now.Allocation.Width +20 	//+20: angle_now has an horiz sep of 10 at each side
					) / 2.0);

		return false;
	}


	private void encoderUpdateTreeViewCapture(List<string> contents)
	{
		//LogB.Information("CONTENTS: " + Util.ListStringToString (contents));
		//LogB.Information("CONTENTS count: " + contents.Count.ToString());
		if (contents == null || contents.Count == 0) {
			encoderButtonsSensitive(encoderSensEnum.DONENOSIGNAL);
		} else {
			treeviewEncoderCaptureRemoveColumns();
			int curvesNum = createTreeViewEncoderCapture(contents);
			if(curvesNum == 0) {
				encoderButtonsSensitive(encoderSensEnum.DONENOSIGNAL);

				//remove last set on cont if there is no data
				if(preferences.encoderCaptureInfinite)
					removeSignalFromGuiBecauseDeletedOrCancelled();

				if(//configChronojump.EncoderCaptureShowOnlyBars &&
						! preferences.encoderCaptureInfinite)
				{
					string minStr = Catalog.GetString ("Minimal range of movement");
					if (current_mode == Constants.Modes.POWERINERTIAL)
						minStr = Catalog.GetString ("Minimal length");

					new DialogMessage (Constants.MessageTypes.WARNING, 500, 300,
							Catalog.GetString("Sorry, no repetitions matched your criteria.") + "\n\n" +
							minStr + ": " + currentEncoderSQLSet.minHeight.ToString () + " cm");
				}
			}
			else {
				if (currentEncoderSQLSet.eccon != "c")
					curvesNum = curvesNum / 2;

				string [] activeCurvesList = new String[curvesNum];
				for(int i=0; i < curvesNum; i++)
					activeCurvesList[i] = (i+1).ToString();
				UtilGtk.ComboUpdate(combo_encoder_analyze_curve_num_combo, activeCurvesList, "");
				combo_encoder_analyze_curve_num_combo.Active = 
					UtilGtk.ComboMakeActive(combo_encoder_analyze_curve_num_combo, activeCurvesList[0]);
				
				encoderButtonsSensitive(encoderSensEnum.DONEYESSIGNAL);
			}
		}
	}
	
	private void treeviewEncoderCaptureRemoveColumns() {
		Gtk.TreeViewColumn [] myColumns = treeview_encoder_capture_curves.Columns;
		foreach (Gtk.TreeViewColumn column in myColumns) 
			treeview_encoder_capture_curves.RemoveColumn (column);

		//blank the encoderCaptureListStore
		encoderCaptureListStore = new Gtk.ListStore (typeof (EncoderCurve));
	}

	private void treeviewEncoderAnalyzeRemoveColumns() {
		Gtk.TreeViewColumn [] myColumns = treeview_encoder_analyze_curves.Columns;
		foreach (Gtk.TreeViewColumn column in myColumns) 
			treeview_encoder_analyze_curves.RemoveColumn (column);
	}


	private string getEncoderAnalysisOptions() {
		string analysisOptions = "-";
		if(preferences.encoderPropulsive)
			analysisOptions = "p";

		return analysisOptions;
	}


	private void encoderDoCurvesGraphR_recalculate ()
	{
		// send curves as this is the analysis will be sent to EncoderParams
		//encoderDoCurvesGraphR (encoderActions.RECALCULATE, "curves"); //encoderAction not needed
		encoderDoCurvesGraphR ("curves");
	}
	private void encoderDoCurvesGraphR_load ()
	{
		// send curves as this is the analysis will be sent to EncoderParams
		//encoderDoCurvesGraphR (encoderActions.LOAD, "curves"); //encoderAction not needed
		encoderDoCurvesGraphR ("curves");
	}
	private void encoderDoCurvesGraphR_curvesAC()
	{
		setCurrentEncoderSQLSetAtCapture (); //here currentEncoderSQLSet is updated 
		//encoderDoCurvesGraphR (encoderActions.CURVES_AC, "curvesAC"); //encoderAction not needed
		encoderDoCurvesGraphR ("curvesAC");
	}

	private void setCurrentEncoderSQLSetAtCapture ()
	{
		//without this we loose the videoURL on recalculate
		string videoURL = "";		
		if (encoderSignalUniqueID >= 0) {
			string file = Util.GetVideoFileName(currentSession.UniqueID, 
				Constants.TestTypes.ENCODER, encoderSignalUniqueID);

			if(file != null && file != "" && File.Exists(file))
				videoURL = file;
		}

		string laterality = getLateralityFromGui(false);

		//see explanation on the top of this file
		currentEncoderSQLSet = new EncoderSQL (
				-1,
				currentPerson.UniqueID,
				currentSession.UniqueID,
				getExerciseIDFromEncoderCombo (exerciseCombos.CAPTURE),
				findEcconFromCaptureGui (true), 	//force ecS (ecc-conc separated)
				laterality,
				Util.ConvertToPoint (findMassFromGui (Constants.MassType.EXTRA)), //when save on sql, do not include person weight
				"",	//signalOrCurve,
				"", 	//fileSaved,	//to know date do: select substr(name,-23,19) from encoder;
				"",	//path,			//url
				preferences.encoderCaptureTime, 
				getEncoderMinHeightOnGuiCapture (),
				"", //desc,
				"", videoURL,		//status, videoURL
				encoderConfigurationNewCapture,
				"","","",	//future1, 2, 3
				preferences.GetEncoderRepetitionCriteria (current_mode),
				encoderConfigurationNewCapture.has_inertia,
				0,0,0,0,
				Util.FindOnArray (':', 2, 1, UtilGtk.ComboGetActive(combo_encoder_exercise_capture),
					encoderExercisesTranslationAndBodyPWeight)	//exerciseName (english)
		);
	}

	/*
	 * this is called by non gtk thread. Don't do gtk stuff here
	 * I suppose reading gtk is ok, changing will be the problem
	 * 
	 * called on calculatecurves, recalculate and load
	 * analysisSent can be "curves" or "curvesAC"
	 * note CURVES_AC defined currentEncoderSQLSet just before
	 * also the other encoderActions have currentEncoderSQLSet
	 * so do not need to send encoderAction
	 */
	private void encoderDoCurvesGraphR (string analysisSent)
	{
		LogB.Debug("encoderDoCurvesGraphR() start");

		string analysis = analysisSent;
		string analysisOptions = getEncoderAnalysisOptions();

		if (image_encoder_width < 100)
			image_encoder_width = 100; //Not crash R with a png height of -1 or "figure margins too large"
		if (image_encoder_height < 100)
			image_encoder_height = 100; //Not crash R with a png height of -1 or "figure margins too large"

		int percentWeight = getExercisePercentBodyWeightFromID (currentEncoderSQLSet.exerciseID);
		double bodyWeight = findMassFromGui (Constants.MassType.BODY); //from gui is ok for all encoderActions, as it just take person weight
		double extraWeight = currentEncoderSQLSet.extraWeightD;
		string eccon = findEcconFromCurrentSet (true);

		EncoderParams ep = new EncoderParams (
				currentEncoderSQLSet.minHeight,
				percentWeight,
				Util.ConvertToPoint (bodyWeight),
				Util.ConvertToPoint (extraWeight),
				eccon,
				analysis,
				"none",				//analysisVariables (not needed in create curves). Cannot be blank
				analysisOptions,
				preferences.encoderCaptureCheckFullyExtended,
				preferences.encoderCaptureCheckFullyExtendedValue,
				currentEncoderSQLSet.encoderConfiguration,
				Util.ConvertToPoint(preferences.encoderSmoothCon),	//R decimal: '.'
			       	0, 			//curve is not used here
				image_encoder_width, image_encoder_height,
				preferences.CSVExportDecimalSeparator 
				);

		EncoderStruct es = new EncoderStruct (
				UtilEncoder.GetEncoderDataTempFileName(), 
				UtilEncoder.GetEncoderGraphTempFileName(),
				UtilEncoder.GetEncoderCurvesTempFileName(), 
				UtilEncoder.GetEncoderScriptsPathWithoutLastSep(),
				UtilEncoder.GetEncoderTempPathWithoutLastSep(),
				ep);

		string exerciseName = currentEncoderSQLSet.ExerciseName;
		double displacedMass = extraWeight + (bodyWeight + percentWeight) / 100.0;

		string title = Util.ChangeSpaceAndMinusForUnderscore (currentPerson.Name) + "-" +
			Util.ChangeSpaceAndMinusForUnderscore (exerciseName);
		if (currentEncoderSQLSet.encoderConfiguration.has_inertia)
			title += "-(" + currentEncoderSQLSet.encoderConfiguration.inertiaTotal.ToString() + " " + Catalog.GetString("Inertia M.") + ")";
		else
			title += "-(" + Util.ConvertToPoint (displacedMass) + "kg)";

		//triggers stuff
		if(analysisSent == "curvesAC")
			triggerListEncoder = eCapture.GetTriggers();

		//triggers only on concentric
		if (triggerListEncoder == null || eccon != "c")
			triggerListEncoder = new TriggerList();

		//send data to encoderRProcAnalyze
		encoderRProcAnalyze.SendData(
				title,
				currentPerson.Name, //used on singleFile
				false,	//do not use neuromuscularProfile script
				preferences.RGraphsTranslate,
				(preferences.encoderCaptureCutByTriggers != Preferences.TriggerTypes.NO_TRIGGERS),
				triggerListEncoder,
				getAnalysisMode(),
				preferences.encoderInertialGraphsX
				); 
		bool result = encoderRProcAnalyze.StartOrContinue(es);
				
		if(result)
			//store this to show 1,2,3,4,… or 1e,1c,2e,2c,… in RenderN
			//if is not stored, it can change when changed eccon radiobutton on cursor is in treeview
		{
			ecconLast = findEcconFromCurrentSet (false);
		}
		else {
			encoderProcessProblems = true;
		}

		LogB.Debug("encoderDoCurvesGraphR() end");
	}
	

	private void on_combo_encoder_laterality_analyze_changed (object o, EventArgs args)
	{
		if(currentPerson != null)
			prepareAnalyzeRepetitions();
	}

	private EncoderSelectRepetitions encSelReps;

	void on_button_encoder_analyze_data_select_curves_clicked (object o, EventArgs args) {
		encSelReps.FakeButtonDone.Clicked += new EventHandler(on_analyze_repetitions_selected);

		if(encSelReps == null)
			prepareAnalyzeRepetitions();

		encSelReps.Show();
	}
	
	void on_analyze_repetitions_selected (object o, EventArgs args) {
		LogB.Information("on_analyze_repetitions_selected");
		encSelReps.FakeButtonDone.Clicked -= new EventHandler(on_analyze_repetitions_selected);

		updateUserCurvesLabelsAndCombo(false);
	}
	
	//called on changing radio mode (! show), and on clicking button_encoder_analyze_data_select_curves (show)
	//not called on current_set
	void prepareAnalyzeRepetitions () 
	{
		if(currentPerson == null || currentSession == null)
			return;

		if(radio_encoder_analyze_individual_current_session.Active) 
		{
			if(encSelReps == null || encSelReps.Type != EncoderSelectRepetitions.Types.INDIVIDUAL_CURRENT_SESSION)
				encSelReps = new EncoderSelectRepetitionsIndividualCurrentSession();

			encSelReps.FakeButtonDeleteCurve.Clicked -= new EventHandler(on_delete_encoder_curve);
			encSelReps.FakeButtonDeleteCurve.Clicked += new EventHandler(on_delete_encoder_curve);
		}
		else if(radio_encoder_analyze_individual_all_sessions.Active)
		{
			if(encSelReps == null || encSelReps.Type != EncoderSelectRepetitions.Types.INDIVIDUAL_ALL_SESSIONS)
				encSelReps = new EncoderSelectRepetitionsIndividualAllSessions();
		}
		else if(radio_encoder_analyze_groupal_current_session.Active)
		{
			if(encSelReps == null || encSelReps.Type != EncoderSelectRepetitions.Types.GROUPAL_CURRENT_SESSION)
				encSelReps = new EncoderSelectRepetitionsGroupalCurrentSession();
		}
		else
			return; //error

		//laterality
		encSelReps.PassVariables(currentPerson, currentSession, currentEncoderGI,
				button_encoder_analyze, getExerciseIDFromEncoderCombo(exerciseCombos.ANALYZE),
				getLateralityOnAnalyze(), preferences.askDeletion);

		encSelReps.Do();

		updateUserCurvesLabelsAndCombo(false);
	}

	private EncoderSelectRepetitions.Lateralities getLateralityOnAnalyze ()
	{
		string lateralityActive = UtilGtk.ComboGetActive (combo_encoder_laterality_analyze);
		if(lateralityActive == Catalog.GetString("Any laterality"))
			return EncoderSelectRepetitions.Lateralities.ANY;
		if(lateralityActive == Catalog.GetString("Both"))
			return EncoderSelectRepetitions.Lateralities.RL;
		if(lateralityActive == Catalog.GetString("Left"))
			return EncoderSelectRepetitions.Lateralities.L;
		if(lateralityActive == Catalog.GetString("Right"))
			return EncoderSelectRepetitions.Lateralities.R;

		return EncoderSelectRepetitions.Lateralities.ANY;
	}
	//if Any then return "" to not select by laterality on SqliteEncoder.Select
	private string getLateralityOnAnalyzeToSQL ()
	{
		EncoderSelectRepetitions.Lateralities laterality = getLateralityOnAnalyze ();
		if(laterality == EncoderSelectRepetitions.Lateralities.ANY)
			return "";
		else
			return laterality.ToString();
	}

	void on_delete_encoder_curve (object o, EventArgs args)
	{
		LogB.Information("at on_delete_encoder_curve");
		delete_encoder_curve(false, encSelReps.DeleteCurveID);
	}	
	void delete_encoder_curve(bool dbconOpened, int uniqueID) 
	{
		LogB.Information(uniqueID.ToString());
		bool eSQLfound = true;

		//EncoderSQL eSQL = (EncoderSQL) SqliteEncoder.Select(dbconOpened, uniqueID, 0, 0, -1, "", EncoderSQL.Eccons.ALL, false, true)[0];
		//WARNING because SqliteEncoder.Select may not return nothing, and then cannot be assigned to eSQL
		//do this:
		
		EncoderSQL eSQL = new EncoderSQL();
		try {
			eSQL = (EncoderSQL) SqliteEncoder.Select(dbconOpened, uniqueID, 0, 0, Constants.EncoderGI.ALL,
					-1, "", EncoderSQL.Eccons.ALL, "", false, true, false)[0];
		} catch {
			eSQLfound = false;
			LogB.Warning("Catched! seems it's already deleted");
		}

		//remove the file
		if(eSQLfound)
			Util.FileDelete(eSQL.GetFullURL(false));	//don't convertPathToR

		Sqlite.Delete(dbconOpened, Constants.EncoderTable, Convert.ToInt32(uniqueID));

		ArrayList escArray = SqliteEncoderSignalCurve.SelectSignalCurve(dbconOpened, 
				-1, Convert.ToInt32(uniqueID),	//signal, curve
				-1, -1); 			//msStart, msEnd
		if(eSQLfound)
			SqliteEncoderSignalCurve.DeleteSignalCurveWithCurveID(dbconOpened, 
					Convert.ToInt32(eSQL.UniqueID)); //delete by curveID on SignalCurve table
		//if deleted curve is from current signal, uncheck it in encoderCaptureCurves
		if(escArray.Count > 0) {
			EncoderSignalCurve esc = (EncoderSignalCurve) escArray[0];
			if(esc.signalID == encoderSignalUniqueID)
				encoderCaptureSelectBySavedCurves(esc.msCentral, false);
		}

		//TODO: change encSelReps and this will change labels
		updateUserCurvesLabelsAndCombo(dbconOpened);
	}

	private Constants.EncoderGI getEncoderGI()
	{
		return currentEncoderGI;
	}

	//separated to be called also from guiT
	ArrayList encoderLoadSignalData() {
		return SqliteEncoder.Select(
				false, -1, currentPerson.UniqueID, currentSession.UniqueID, getEncoderGI(),
				-1, "signal", EncoderSQL.Eccons.ALL, "",
				false, true, false);
	}
	//this is called when user clicks on load signal (currently only on analyze)
	void on_button_encoder_load_signal_clicked (object o, EventArgs args) {
		on_encoder_load_signal_clicked (encoderSignalUniqueID);
	}
	//this can be called also by guiT
	void on_encoder_load_signal_clicked (int myEncoderSignalUniqueID) 
	{
		ArrayList data = encoderLoadSignalData();

		ArrayList dataPrint = new ArrayList();
		int count = 1;
		foreach(EncoderSQL es in data) 
			dataPrint.Add(es.ToStringArray(count++,false,true,true,false));
		
		string [] columnsString = {
			Catalog.GetString("ID"),
			Catalog.GetString("Set"),
			Catalog.GetString("Exercise"),
			Catalog.GetString("Laterality"),
			Catalog.GetString("Extra weight"),
			Catalog.GetString("Encoder configuration"),
			Catalog.GetString("Contraction"),
			Catalog.GetString("Date"),
			Catalog.GetString("Video"),
			Catalog.GetString("Comment")
		};

		ArrayList bigArray = new ArrayList();
		ArrayList a1 = new ArrayList();
		ArrayList a2 = new ArrayList();
		//0 is the widgget to show; 1 is the editable; 2 id default value
		a1.Add(Constants.GenericWindowShow.TREEVIEW); a1.Add(true); a1.Add("");
		bigArray.Add(a1);
	
		a2.Add(Constants.GenericWindowShow.COMBO); a2.Add(true); a2.Add("");
		bigArray.Add(a2);
		
		genericWin = GenericWindow.Show (Catalog.GetString("Load set"), false,	//don't show now
				string.Format(Catalog.GetString("Select set of athlete {0} on this session."), 
					currentPerson.Name), bigArray);

		genericWin.SetTreeview(columnsString, false, dataPrint, new ArrayList(), GenericWindow.EditActions.EDITPLAYDELETE, true);

		//find all persons in current session
		ArrayList personsPre = SqlitePersonSession.SelectCurrentSessionPersons(currentSession.UniqueID,
				false); //means: do not returnPersonAndPSlist

		string [] persons = new String[personsPre.Count];
		count = 0;
	        foreach	(Person p in personsPre)
			persons[count++] = p.UniqueID.ToString() + ":" + p.Name;
		genericWin.SetComboEditValues (persons, currentPerson.UniqueID + ":" + currentPerson.Name);
		//genericWin.SetComboLabel(Catalog.GetString("Change the owner of selected set") +
		//		" (" + Catalog.GetString("code") + ":" + Catalog.GetString("name") + ")");
		genericWin.SetComboLabel(Catalog.GetString("Change person"));
		genericWin.ShowEditRow(false);

		//select row corresponding to current signal
		genericWin.SelectRowWithID(0, myEncoderSignalUniqueID); //colNum, id

		genericWin.VideoColumn = 8;
		genericWin.CommentColumn = 9;

		genericWin.ShowButtonCancel(true);
		genericWin.SetButtonAcceptLabel(Catalog.GetString("Load"));
		genericWin.SetButtonCancelLabel(Catalog.GetString("Close"));
		genericWin.SetButtonAcceptSensitive(false);
		genericWin.Button_accept.Clicked += new EventHandler(on_encoder_load_signal_accepted);
		genericWin.Button_row_play.Clicked += new EventHandler(on_encoder_load_signal_row_play);
		genericWin.Button_row_edit.Clicked += new EventHandler(on_encoder_load_signal_row_edit);
		genericWin.Button_row_edit_apply.Clicked += new EventHandler(on_encoder_load_signal_row_edit_apply);
		genericWin.Button_row_delete.Clicked += new EventHandler(on_encoder_load_signal_row_delete_pre);

		genericWin.ShowNow();
	}
	
	protected void on_encoder_load_signal_accepted (object o, EventArgs args)
	{
		LogB.Information("on load signal accepted");
		genericWin.Button_accept.Clicked -= new EventHandler(on_encoder_load_signal_accepted);

		int uniqueID = genericWin.TreeviewSelectedRowID();

		genericWin.HideAndNull();

		on_encoder_load_signal_accepted_do (uniqueID);
	}

	protected void on_encoder_load_signal_accepted_do (int uniqueID)
	{
		sensitiveGuiEventDoing (false);

		ArrayList data = SqliteEncoder.Select(
				false, uniqueID, currentPerson.UniqueID, currentSession.UniqueID, Constants.EncoderGI.ALL,
				-1, "signal", EncoderSQL.Eccons.ALL, "",
				false, true, false);

		bool success = false;
		foreach(EncoderSQL eSQL in data)
		{	//it will run only one time
			success = UtilEncoder.CopyEncoderDataToTemp(eSQL.url, eSQL.filename);
			if(success)
			{
				currentEncoderSQLSet = eSQL;

				/*
				 * maxPowerIntersession it's defined (Sqlite select) on capture and after capture
				 * if we have not captured yet, just Sqlite select now
				 */
				if(! feedbackWin.EncoderRelativeToSet)
					findMaxPowerSpeedForceIntersession (
							currentEncoderSQLSet.exerciseID,
							currentEncoderSQLSet.encoderConfiguration,
							currentEncoderSQLSet.Laterality,
							currentEncoderSQLSet.extraWeightD);

				//TODO: show info to user in a dialog,
				//but check if more info have to be shown on this process

				encoderTimeStamp = eSQL.GetDatetimeStr(false);
				encoderSignalUniqueID = eSQL.UniqueID;

				//has to be done here, because if done in encoderThreadStart or in finishPulsebar it crashes 
				button_video_play_this_test.Sensitive = (eSQL.videoURL != "");

				//encoderConfigurationCurrent = eSQL.encoderConfiguration;

				//manage EncoderConfigurationSQLObject
				SqliteEncoderConfiguration.MarkAllAsUnactive(false, currentEncoderGI);
				EncoderConfigurationSQLObject econfSO = SqliteEncoderConfiguration.SelectByEconf(false, currentEncoderGI, eSQL.encoderConfiguration);

				//if user has deleted this econfSO, create it again
				if(econfSO.uniqueID == -1)
				{
					/*
					   old (before Chronojump 2.2.2): create a Unnamed and if exists, a Unnamed_copy, _copy2, ...
					   problem is when we import a lot of sessions, each time we load a set with a config that we don't have in the database,
					   it creates a new config, all named Unnamed_copy* and it's a mess

					string name = SqliteEncoderConfiguration.IfNameExistsAddSuffix(Catalog.GetString("Unnamed"), "_" + Catalog.GetString("copy"));

					econfSO = new EncoderConfigurationSQLObject(
							-1,				//uniqueID
							currentEncoderGI,		//encoderGI
							true,				//active
							name,				//name
							eSQL.encoderConfiguration,	//encoderConfiguration
							""				//description
							);
					SqliteEncoderConfiguration.Insert(false, econfSO);
					*/
					/*
					   new (at Chronojump 2.2.2):
					   Only one Unnamed.
					   - If it exists, delete it. Then create with new configuration (could update but delete and insert seems safer because there are more params)
					   - If it does not exist the Unnamed, create it
					   If user renames it, then an Unnamed will be created next time that a set with new different config is loaded
					   See: https://gitlab.gnome.org/GNOME/chronojump/-/issues/96
					   */
					string unnamedTrans = Catalog.GetString("Unnamed");
					EncoderConfigurationSQLObject econfSOUnnamed =
						SqliteEncoderConfiguration.SelectByEncoderGIAndName(false, currentEncoderGI, unnamedTrans);

					if(econfSOUnnamed.uniqueID >= 0) // if exists, delete it
						SqliteEncoderConfiguration.Delete (false, currentEncoderGI, unnamedTrans);

					econfSO = new EncoderConfigurationSQLObject(
							-1,				//uniqueID
							currentEncoderGI,		//encoderGI
							true,				//active
							unnamedTrans,			//name
							eSQL.encoderConfiguration,	//encoderConfiguration
							""				//description
							);
					SqliteEncoderConfiguration.Insert(false, econfSO);
				} else {
					//if exists on database mark and update sql row as active
					econfSO.active = true;
					SqliteEncoderConfiguration.Update(false, currentEncoderGI, econfSO.name, econfSO);
				}

				//triggers
				triggerListEncoder = new TriggerList(
						SqliteTrigger.Select(
							false, Trigger.Modes.ENCODER,
							encoderSignalUniqueID)
						);
				showEncoderAnalyzeTriggersAndTab();
			}
		}

		//test: try to compress signal in order to send if.
		//obviously this is not going to be done here

		//LogB.Information("Trying compress function");
		//LogB.Information(UtilEncoder.CompressSignal(UtilEncoder.GetEncoderDataTempFileName()));

		if(success) {	
			//force a recalculate but not save the curve (we are loading)
			encoderCalculeCurves (encoderActions.LOAD);
			radio_encoder_analyze_individual_current_set.Active = true;
		}
		else
			sensitiveGuiEventDone ();
	}

	protected void on_encoder_load_signal_row_play (object o, EventArgs args)
	{
		LogB.Information("row play at load signal");
		LogB.Information(genericWin.TreeviewSelectedUniqueID.ToString());
		playVideo(Util.GetVideoFileName(currentSession.UniqueID,
				Constants.TestTypes.ENCODER, genericWin.TreeviewSelectedUniqueID));
	}

	protected void on_encoder_load_signal_row_edit (object o, EventArgs args) {
		LogB.Information("row edit at load signal");
		LogB.Information(genericWin.TreeviewSelectedUniqueID.ToString());
		genericWin.ShowEditRow(true);
	}
	
	protected void on_encoder_load_signal_row_edit_apply (object o, EventArgs args)
	{
		LogB.Information("row edit apply at load signal. Opening db:");

		Sqlite.Open();

		//1) select set
		int setID = genericWin.TreeviewSelectedUniqueID;
		EncoderSQL eSQL_set = (EncoderSQL) SqliteEncoder.Select(true, setID, 0, 0, Constants.EncoderGI.ALL,
				-1, "", EncoderSQL.Eccons.ALL, "", false, true, false)[0];

		//2) if changed comment, update SQL, and update treeview
		//first remove conflictive characters
		string comment = Util.RemoveTildeAndColonAndDot(genericWin.EntryEditRow);
		if(comment != eSQL_set.Description)
		{
			eSQL_set.Description = comment;
			SqliteEncoder.Update(true, eSQL_set);

			//update treeview
			genericWin.on_edit_selected_done_update_treeview();
		}

		//3) change the session param and the url of signal and curves (if any)
		string idName = genericWin.GetComboEditSelected;
		LogB.Information("new person: " + idName);
		int newPersonID = Util.FetchID(idName);
		if(newPersonID != currentPerson.UniqueID)
		{
			//change stuff on signal
			EncoderSQL eSQLChangedPerson = eSQL_set.ChangePerson(idName);
			SqliteEncoder.Update(true, eSQLChangedPerson);
			genericWin.RemoveSelectedRow();
			genericWin.SetButtonAcceptSensitive(false);

			//select linkedReps (if any)
			ArrayList linkedReps = SqliteEncoderSignalCurve.SelectSignalCurve(
					true, setID, -1, -1, -1);	//DBopened, signal, curve, msStart, msEnd

			//change stuff on repetitions (if any)
			foreach (EncoderSignalCurve esc in linkedReps)
			{
				EncoderSQL eSQL = (EncoderSQL) SqliteEncoder.Select(true, esc.curveID, 0, 0, Constants.EncoderGI.ALL,
						-1, "curve", EncoderSQL.Eccons.ALL, "", false, true, false)[0];

				eSQLChangedPerson = eSQL.ChangePerson(idName);
				SqliteEncoder.Update(true, eSQLChangedPerson);
			}
		}

		genericWin.ShowEditRow(false);
		genericWin.SensitiveEditDeleteIfSelected();

		//remove signal from gui just in case the edited signal is the same we have loaded
		removeSignalFromGuiBecauseDeletedOrCancelled();

		Sqlite.Close();
	}
	
	protected void on_encoder_load_signal_row_delete_pre (object o, EventArgs args)
	{
		if(preferences.askDeletion) {
			ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString(
						"Are you sure you want to delete this set?"), Catalog.GetString("Saved repetitions related to this set will also be deleted."), "");
			confirmWin.Button_accept.Clicked += new EventHandler(on_encoder_load_signal_row_delete);
		} else
			on_encoder_load_signal_row_delete (o, args);
	}
	protected void on_encoder_load_signal_row_delete (object o, EventArgs args)
	{
		LogB.Information("row delete at load signal");

		int signalID = genericWin.TreeviewSelectedUniqueID;
		LogB.Information(signalID.ToString());

		//if it's current signal use the delete signal from the gui interface that updates gui
		if(signalID == encoderSignalUniqueID)
			on_button_encoder_delete_signal_accepted (o, args);
		else {
			EncoderSQL eSQL = (EncoderSQL) SqliteEncoder.Select(
					false, signalID, 0, 0, Constants.EncoderGI.ALL,
					-1, "signal", EncoderSQL.Eccons.ALL, "", false, true, false)[0];
		
			//delete signal and related curves (both from SQL and files)
			encoderSignalDelete (eSQL.GetFullURL(false), eSQL.videoURL, signalID);	//don't convertPathToR

			//genericWin selected row is deleted, unsensitive the "load" button
			genericWin.SetButtonAcceptSensitive(false);
		}
		genericWin.Delete_row_accepted();
	}

	private void encoderLoadToPaintData ()
	{
		if (current_mode == Constants.Modes.POWERINERTIAL)
			eCapture = new EncoderCaptureInertial();
		else // if (current_mode == Constants.Modes.POWERGRAVITATORY)
			eCapture = new EncoderCaptureGravitatory();

		cairoGraphEncoderSignal = null;
		cairoGraphEncoderSignalPoints_l = new List<PointF>();
		cairoGraphEncoderSignalInertialPoints_l = new List<PointF>();

		eCapture.LoadFromFile (current_mode == Constants.Modes.POWERINERTIAL,
				preferences.signalDirectionHorizontal);
		eCapture.PointsPainted = -1;
		if (current_mode == Constants.Modes.POWERINERTIAL) {
			updateEncoderCaptureGraphPaintData (UpdateEncoderPaintModes.INERTIAL);
			//updateEncoderCaptureSignalCairo (true, false); //inertial, forceRedraw
		} else {
			updateEncoderCaptureGraphPaintData (UpdateEncoderPaintModes.GRAVITATORY);
			//updateEncoderCaptureSignalCairo (false, false);
		}
		//eCapture.PointsPainted = 0;
		//encoder_capture_signal_drawingarea_cairo.QueueDraw (); //aixo no hauria de caldre aqui pq ja es deu fer al thread de sota

		// show the signal realtime cairo graph (not the R generated)
//		notebook_encoder_capture.CurrentPage = 0; //TODO: return to show the Page 1 at end
	}

	// called at: initEncoder1Time , changeMode
	void encoderConfigurationGUIUpdate ()
	{
		if(current_mode == Constants.Modes.POWERINERTIAL)
		{
			notebook_encoder_top.Page = 1;
			label_encoder_exercise_mass.Visible = false;
			hbox_encoder_exercise_mass.Visible = false;
			label_encoder_exercise_inertia.Visible = true;
			box_encoder_exercise_inertia.Visible = true;
			hbox_encoder_exercise_gravitatory_min_mov.Visible = false;
			hbox_encoder_exercise_inertial_min_mov.Visible = true;
			
			if(! encoderConfigurationNewCapture.list_d.IsEmpty())
			{
				UtilGtk.ComboUpdate(combo_encoder_anchorage, encoderConfigurationNewCapture.list_d.L);
				combo_encoder_anchorage.Active = UtilGtk.ComboMakeActive(
						combo_encoder_anchorage, 
						encoderConfigurationNewCapture.d.ToString()
						);
			}

			spin_encoder_im_weights_n.Value = encoderConfigurationNewCapture.extraWeightN;

			label_encoder_im_total.Text = encoderConfigurationNewCapture.inertiaTotal.ToString();
			label_encoder_top_im.Text = Catalog.GetString("Inertia M.") + ": " + label_encoder_im_total.Text;

			spin_encoder_capture_min_height_inertial.Value = preferences.EncoderCaptureMinHeight(true);

			label_encoder_equivalent_mass.Text = Util.TrimDecimals (UtilEncoder.CalculateEquivalentMass (encoderConfigurationNewCapture), 1);
		}
		else { //(current_mode == Constants.Modes.POWERGRAVITATORY)
			notebook_encoder_top.Page = 0;
			label_encoder_exercise_mass.Visible = true;
			hbox_encoder_exercise_mass.Visible = true;
			label_encoder_exercise_inertia.Visible = false;
			box_encoder_exercise_inertia.Visible = false;
			hbox_encoder_exercise_gravitatory_min_mov.Visible = true;
			hbox_encoder_exercise_inertial_min_mov.Visible = false;
			spin_encoder_capture_min_height_gravitatory.Value = preferences.EncoderCaptureMinHeight(false);
		}
	}

	private void setEncoderConfigurationLabels (string savedName, string code)
	{
		label_encoder_selected.Text = string.Format ("{0} ({1})", savedName, code);
		label_encoder_top_selected.Text = savedName;
	}

	void encoderSignalDelete (string signalURL, string videoRel, int signalID)
	{
		//remove signal file
		Util.FileDelete(signalURL);

		//delete signal from encoder table
		Sqlite.Delete(false, Constants.EncoderTable, signalID);

		//find related curves using encoderSignalCurve table
		ArrayList linkedCurves = SqliteEncoderSignalCurve.SelectSignalCurve(
				false, signalID, -1, -1, -1);	//DBopened, signal, curve, msStart, msEnd

		//delete related curves: files and records from encoder table
		foreach(EncoderSignalCurve esc in linkedCurves) 
		{
			//select related curves to find URL
			ArrayList array = SqliteEncoder.Select(
					false, esc.curveID, -1, -1, Constants.EncoderGI.ALL,
					-1, "curve", EncoderSQL.Eccons.ALL, "", false, true, false);

			if (array != null && array.Count > 0)
			{
				EncoderSQL eSQL = (EncoderSQL) array[0];
				//delete file
				if(eSQL != null)
					Util.FileDelete(eSQL.GetFullURL(false));	//don't convertPathToR
			}

			//delete curve from encoder table
			Sqlite.Delete(false, Constants.EncoderTable, esc.curveID);
		}

		//delete related records from encoderSignalCurve table
		Sqlite.DeleteSelectingField(false, Constants.EncoderSignalCurveTable, 
				"signalID", signalID.ToString());

		//delete related triggers
		SqliteTrigger.DeleteByModeID(false, Trigger.Modes.ENCODER, signalID);

		//delete video
		if (videoRel != "")
			Util.FileDelete (Util.MakeURLabsolute (videoRel)); //note having it relative will also work
	}

	void on_button_encoder_export_signal_clicked (object o, EventArgs args) 
	{
		checkFile(Constants.CheckFileOp.ENCODER_CAPTURE_EXPORT_SIGNAL);
	}

	void on_button_encoder_export_signal_file_selected (string selectedFileName) 
	{
		string analysisOptions = getEncoderAnalysisOptions();
		string displacedMass = Util.ConvertToPoint (findDisplacedMassFromSQL ());

		EncoderParams ep = new EncoderParams(
				currentEncoderSQLSet.minHeight, 
				getExercisePercentBodyWeightFromID (currentEncoderSQLSet.exerciseID),
				Util.ConvertToPoint (findMassFromGui (Constants.MassType.BODY)), //from gui is ok, as it just take person weight
				Util.ConvertToPoint (currentEncoderSQLSet.extraWeightD),
				findEcconFromCurrentSet (false),
				"exportCSV",
				"none",						//analysisVariables (not needed in create curves). Cannot be blank
				analysisOptions,
				preferences.encoderCaptureCheckFullyExtended,
				preferences.encoderCaptureCheckFullyExtendedValue,
				currentEncoderSQLSet.encoderConfiguration,
				Util.ConvertToPoint(preferences.encoderSmoothCon),	//R decimal: '.'
				-1,
				image_encoder_width,
				image_encoder_height,
				preferences.CSVExportDecimalSeparator 
				);


		EncoderStruct encoderStruct = new EncoderStruct(
				UtilEncoder.GetEncoderDataTempFileName(),
				UtilEncoder.GetEncoderGraphTempFileName(),
				Util.GetEncoderExportTempFileName(), 
				UtilEncoder.GetEncoderScriptsPathWithoutLastSep(),
				UtilEncoder.GetEncoderTempPathWithoutLastSep(),
				ep);

		encoderRProcAnalyze.ExportFileName = selectedFileName;

		encoderRProcAnalyze.SendData(
				Util.ChangeSpaceAndMinusForUnderscore(currentPerson.Name) + "-" + 
				Util.ChangeSpaceAndMinusForUnderscore(currentEncoderSQLSet.ExerciseName) + 
					"-(" + displacedMass + "kg)",
				currentPerson.Name,
				false, 			//do not use neuromuscularProfile script
				preferences.RGraphsTranslate,
				(preferences.encoderCaptureCutByTriggers != Preferences.TriggerTypes.NO_TRIGGERS),
				new TriggerList(),
				getAnalysisMode(),
				preferences.encoderInertialGraphsX
				);
		encoderRProcAnalyze.StartOrContinue(encoderStruct);

		//event_execute_label_message.Text = string.Format(Catalog.GetString(
		//			"Exported to {0}."), UtilEncoder.GetEncoderExportTempFileName());
	}

	private EncoderGraphROptions.AnalysisModes getAnalysisMode()
	{
		EncoderGraphROptions.AnalysisModes am = EncoderGraphROptions.AnalysisModes.INDIVIDUAL_CURRENT_SET; //default

		if(radio_encoder_analyze_individual_current_set.Active)
			am = EncoderGraphROptions.AnalysisModes.INDIVIDUAL_CURRENT_SET;
		else if(radio_encoder_analyze_individual_current_session.Active)
			am = EncoderGraphROptions.AnalysisModes.INDIVIDUAL_CURRENT_SESSION;
		else if(radio_encoder_analyze_individual_all_sessions.Active)
			am = EncoderGraphROptions.AnalysisModes.INDIVIDUAL_ALL_SESSIONS;
		else if(radio_encoder_analyze_groupal_current_session.Active)
			am = EncoderGraphROptions.AnalysisModes.GROUPAL_CURRENT_SESSION;

		return am;
	}
						
	void on_button_encoder_save_AB_file_selected (string selectedFileName)
	{
		int msa = Convert.ToInt32(hscale_encoder_analyze_a.Value);
		int msb = Convert.ToInt32(hscale_encoder_analyze_b.Value);
		
		eai.ExportToCSV(msa, msb, selectedFileName, preferences.CSVExportDecimalSeparator);
	}

	string exportFileName;	
	//to export a folder check below method
	protected bool checkFile (Constants.CheckFileOp checkFileOp)
	{
		string exportString = ""; 
		if(checkFileOp == Constants.CheckFileOp.ENCODER_CAPTURE_EXPORT_SIGNAL)
			exportString = Catalog.GetString ("Export set in CSV format");
		else if(
				checkFileOp == Constants.CheckFileOp.JUMPS_SIMPLE_CAPTURE_SAVE_IMAGE ||
				checkFileOp == Constants.CheckFileOp.JUMPS_REACTIVE_CAPTURE_SAVE_IMAGE ||
				checkFileOp == Constants.CheckFileOp.JUMPS_PROFILE_SAVE_IMAGE ||
				checkFileOp == Constants.CheckFileOp.JUMPS_DJ_OPTIMAL_FALL_SAVE_IMAGE ||
				checkFileOp == Constants.CheckFileOp.JUMPS_ASYMMETRY_BILATERAL_SAVE_IMAGE ||
				checkFileOp == Constants.CheckFileOp.JUMPS_ASYMMETRY_ASYMMETRY_SAVE_IMAGE ||
				checkFileOp == Constants.CheckFileOp.JUMPS_WEIGHT_FV_PROFILE_SAVE_IMAGE ||
				checkFileOp == Constants.CheckFileOp.JUMPS_EVOLUTION_SAVE_IMAGE ||
				checkFileOp == Constants.CheckFileOp.JUMPS_RJ_FATIGUE_SAVE_IMAGE ||
				checkFileOp == Constants.CheckFileOp.RUNS_SIMPLE_CAPTURE_SAVE_IMAGE ||
				checkFileOp == Constants.CheckFileOp.RUNS_INTERVALLIC_CAPTURE_SAVE_IMAGE ||
				checkFileOp == Constants.CheckFileOp.RUNS_EVOLUTION_SAVE_IMAGE ||
				checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_SAVE_IMAGE ||
				checkFileOp == Constants.CheckFileOp.ENCODER_ANALYZE_SAVE_IMAGE ||
				checkFileOp == Constants.CheckFileOp.ENCODER_ANALYZE_SAVE_IMAGE_CURRENT_SESSION ||
				checkFileOp == Constants.CheckFileOp.FORCESENSOR_SAVE_IMAGE_SIGNAL ||
				checkFileOp == Constants.CheckFileOp.FORCESENSOR_SAVE_IMAGE_MODEL ||
				checkFileOp == Constants.CheckFileOp.FORCESENSOR_SAVE_IMAGE_RFD_MANUAL ||
				checkFileOp == Constants.CheckFileOp.RUNENCODER_CAPTURE_SAVE_IMAGE ||
				checkFileOp == Constants.CheckFileOp.RUNENCODER_ANALYZE_SAVE_IMAGE_MODEL)
			exportString = Catalog.GetString ("Save image");
		else if(
				checkFileOp == Constants.CheckFileOp.ENCODER_ANALYZE_SAVE_AB ||
				checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_EXPORT_INDIVIDUAL_CURRENT_SESSION_NO_IMAGES ||
				checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_EXPORT_INDIVIDUAL_ALL_SESSIONS_NO_IMAGES ||
				checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_EXPORT_GROUPAL_CURRENT_SESSION_NO_IMAGES ||
				checkFileOp == Constants.CheckFileOp.FORCESENSOR_EXPORT_INDIVIDUAL_CURRENT_SESSION_NO_IMAGES ||
				checkFileOp == Constants.CheckFileOp.FORCESENSOR_EXPORT_INDIVIDUAL_ALL_SESSIONS_NO_IMAGES ||
				checkFileOp == Constants.CheckFileOp.FORCESENSOR_EXPORT_GROUPAL_CURRENT_SESSION_NO_IMAGES ||
				checkFileOp == Constants.CheckFileOp.RUNENCODER_EXPORT_INDIVIDUAL_CURRENT_SESSION_NO_IMAGES ||
				checkFileOp == Constants.CheckFileOp.RUNENCODER_EXPORT_INDIVIDUAL_ALL_SESSIONS_NO_IMAGES ||
				checkFileOp == Constants.CheckFileOp.RUNENCODER_EXPORT_GROUPAL_CURRENT_SESSION_NO_IMAGES)
			exportString = Catalog.GetString ("Export repetition in CSV format");
		else if(checkFileOp == Constants.CheckFileOp.ENCODER_ANALYZE_SAVE_TABLE ||
				checkFileOp == Constants.CheckFileOp.ENCODER_ANALYZE_SAVE_TABLE_CURRENT_SESSION ||
				checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_SAVE_TABLE ||
				checkFileOp == Constants.CheckFileOp.RUNENCODER_ANALYZE_SAVE_TABLE)
			exportString = Catalog.GetString ("Save table");

		// 2) write the name of the file: nameString

		string nameString = currentPerson.Name + "_" + currentSession.DateShortAsSQL;

		// persons
		if (checkFileOp == Constants.CheckFileOp.PERSONS_EXPORT_THIS_SESSION)
			nameString = "ChronojumpPersons_" + currentSession.Name + "_" + currentSession.DateShortAsSQL + ".csv";
		else if (checkFileOp == Constants.CheckFileOp.PERSONS_EXPORT_ANY_SESSION)
			nameString = "ChronojumpPersons_" + UtilDate.ToFile(DateTime.Now) + ".csv";

		if(
				checkFileOp == Constants.CheckFileOp.ENCODER_ANALYZE_SAVE_IMAGE_CURRENT_SESSION ||
				checkFileOp == Constants.CheckFileOp.ENCODER_ANALYZE_SAVE_TABLE_CURRENT_SESSION ||
				checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_EXPORT_GROUPAL_CURRENT_SESSION_NO_IMAGES ||
				checkFileOp == Constants.CheckFileOp.FORCESENSOR_EXPORT_GROUPAL_CURRENT_SESSION_NO_IMAGES ||
				checkFileOp == Constants.CheckFileOp.RUNENCODER_EXPORT_GROUPAL_CURRENT_SESSION_NO_IMAGES )
			nameString = currentSession.Name + "_" + currentSession.DateShortAsSQL;

		//on intersession do not show session in nameString
		else if(
				checkFileOp == Constants.CheckFileOp.JUMPS_ASYMMETRY_BILATERAL_SAVE_IMAGE ||
				checkFileOp == Constants.CheckFileOp.JUMPS_ASYMMETRY_ASYMMETRY_SAVE_IMAGE ||
				checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_EXPORT_INDIVIDUAL_ALL_SESSIONS_NO_IMAGES ||
				checkFileOp == Constants.CheckFileOp.FORCESENSOR_EXPORT_INDIVIDUAL_ALL_SESSIONS_NO_IMAGES ||
				checkFileOp == Constants.CheckFileOp.RUNENCODER_EXPORT_INDIVIDUAL_ALL_SESSIONS_NO_IMAGES)
			nameString = currentPerson.Name;

		//on encoder analyze save image, show analysis on filename
		else if(
				( checkFileOp == Constants.CheckFileOp.ENCODER_ANALYZE_SAVE_IMAGE ||
				checkFileOp == Constants.CheckFileOp.ENCODER_ANALYZE_SEND_IMAGE )
				&&
				encoderLastAnalysis != "null" && encoderLastAnalysis != "" )
		{
			nameString += "_" + encoderLastAnalysis;
		}

		//on force sensor add exercise and laterality
		//and if elastic, exercise should have (stiffness)
		if (
				(currentForceSensor != null && currentForceSensorExercise != null) &&
				(checkFileOp == Constants.CheckFileOp.FORCESENSOR_SAVE_IMAGE_SIGNAL ||
				 checkFileOp == Constants.CheckFileOp.FORCESENSOR_SAVE_IMAGE_MODEL ||
				 checkFileOp == Constants.CheckFileOp.FORCESENSOR_SAVE_IMAGE_RFD_MANUAL ||
				 checkFileOp == Constants.CheckFileOp.FORCESENSOR_ANALYZE_SAVE_AB ||
				 checkFileOp == Constants.CheckFileOp.FORCESENSOR_ANALYZE_SAVE_CD) )
		{
			string exName = Util.RemoveBackSlash (Util.RemoveSlash (currentForceSensorExercise.Name));

			if(currentForceSensorExercise.ComputeAsElastic)
				nameString += "_" + exName + "_Stiffness" + currentForceSensor.Stiffness.ToString();
			else
				nameString += "_" + exName;

			nameString += "_" + Catalog.GetString (currentForceSensor.Laterality);
		}

		//when we send an image we just want to define the name
		if(checkFileOp == Constants.CheckFileOp.ENCODER_ANALYZE_SEND_IMAGE)
		{
			exportFileName = nameString;
			return true;
		}

		if(checkFileOp == Constants.CheckFileOp.ENCODER_CAPTURE_EXPORT_SIGNAL)
			nameString += "_encoder_set_export.csv";
		else if(checkFileOp == Constants.CheckFileOp.ENCODER_CAPTURE_SAVE_IMAGE)
			nameString += "_encoder_set.png";
		else if(
				checkFileOp == Constants.CheckFileOp.JUMPS_SIMPLE_CAPTURE_SAVE_IMAGE ||
				checkFileOp == Constants.CheckFileOp.JUMPS_REACTIVE_CAPTURE_SAVE_IMAGE ||
				checkFileOp == Constants.CheckFileOp.RUNS_SIMPLE_CAPTURE_SAVE_IMAGE ||
				checkFileOp == Constants.CheckFileOp.RUNS_INTERVALLIC_CAPTURE_SAVE_IMAGE)
		{
			//if showing all persons, do not person name on filename
			if(radio_contacts_results_personAll.Active)
				nameString = currentSession.DateShortAsSQL;

			string testType = "";
			if(checkFileOp == Constants.CheckFileOp.JUMPS_SIMPLE_CAPTURE_SAVE_IMAGE)
				testType = "_" + Util.ChangeChars(Catalog.GetString("Jumps simple"), " ", "_") + "_";
			else if(checkFileOp == Constants.CheckFileOp.JUMPS_REACTIVE_CAPTURE_SAVE_IMAGE)
				testType = "_" + Util.ChangeChars(Catalog.GetString("Jumps multiple"), " ", "_") + "_";
			else if(checkFileOp == Constants.CheckFileOp.RUNS_SIMPLE_CAPTURE_SAVE_IMAGE)
				testType = "_" + Util.ChangeChars(Catalog.GetString("Races simple"), " ", "_") + "_";
			else if(checkFileOp == Constants.CheckFileOp.RUNS_INTERVALLIC_CAPTURE_SAVE_IMAGE)
				testType = "_" + Util.ChangeChars(Catalog.GetString("Races intervallic"), " ", "_") + "_";

			//if showing a jump or all, show on filename
			if(radio_contacts_graph_allTests.Active)
				nameString += testType + Catalog.GetString("all") + ".png";
			else
				nameString += testType + radio_contacts_graph_currentTest.Label + ".png";
		}
		else if(checkFileOp == Constants.CheckFileOp.JUMPS_PROFILE_SAVE_IMAGE)
			nameString += "_jumps_profile.png";
		else if(checkFileOp == Constants.CheckFileOp.JUMPS_DJ_OPTIMAL_FALL_SAVE_IMAGE)
			nameString += "_jumps_dj_optimal_fall.png";
		else if(checkFileOp == Constants.CheckFileOp.JUMPS_ASYMMETRY_BILATERAL_SAVE_IMAGE)
			nameString += "_jumps_asymmetry_bilateral.png";
		else if(checkFileOp == Constants.CheckFileOp.JUMPS_ASYMMETRY_ASYMMETRY_SAVE_IMAGE)
			nameString += "_jumps_asymmetry_asymmetry.png";
		else if(checkFileOp == Constants.CheckFileOp.JUMPS_WEIGHT_FV_PROFILE_SAVE_IMAGE)
			nameString += "_jumps_fv_profile.png";
		else if(checkFileOp == Constants.CheckFileOp.JUMPS_EVOLUTION_SAVE_IMAGE)
			nameString += "_jumps_by_time.png";
		else if(checkFileOp == Constants.CheckFileOp.JUMPS_RJ_FATIGUE_SAVE_IMAGE)
			nameString += "_jumps_rj_fatigue.png";
		else if(checkFileOp == Constants.CheckFileOp.RUNS_EVOLUTION_SAVE_IMAGE)
			nameString += "_runs_by_time.png";
		else if(checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_SAVE_IMAGE)
			nameString += "_runs_sprint.png";
		else if(checkFileOp == Constants.CheckFileOp.ENCODER_ANALYZE_SAVE_IMAGE ||
				checkFileOp == Constants.CheckFileOp.ENCODER_ANALYZE_SAVE_IMAGE_CURRENT_SESSION)
			nameString += "_encoder.png";
		else if(checkFileOp == Constants.CheckFileOp.FORCESENSOR_SAVE_IMAGE_SIGNAL)
			nameString += "_force_sensor_set.png";
		else if(checkFileOp == Constants.CheckFileOp.FORCESENSOR_SAVE_IMAGE_MODEL)
			nameString += "_force_sensor_rfd_model.png";
		else if(checkFileOp == Constants.CheckFileOp.FORCESENSOR_SAVE_IMAGE_RFD_MANUAL)
			nameString += "_force_sensor_general_analysis.png";
		else if(checkFileOp == Constants.CheckFileOp.RUNENCODER_CAPTURE_SAVE_IMAGE)
			nameString += "_race_analyzer_capture.png";
		else if(checkFileOp == Constants.CheckFileOp.RUNENCODER_ANALYZE_SAVE_IMAGE_MODEL)
			nameString += "_race_analyzer_model.png";
		else if(checkFileOp == Constants.CheckFileOp.ENCODER_ANALYZE_SAVE_AB)
			nameString += "_encoder_repetition_export.csv";
		else if(checkFileOp == Constants.CheckFileOp.FORCESENSOR_ANALYZE_SAVE_AB)
			nameString += "_forcesensor_analyze_AB_export.csv";
		else if(checkFileOp == Constants.CheckFileOp.FORCESENSOR_ANALYZE_SAVE_CD)
			nameString += "_forcesensor_analyze_CD_export.csv";
		else if(
				checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_EXPORT_INDIVIDUAL_CURRENT_SESSION_NO_IMAGES ||
				checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_EXPORT_INDIVIDUAL_ALL_SESSIONS_NO_IMAGES ||
				checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_EXPORT_GROUPAL_CURRENT_SESSION_NO_IMAGES)
			nameString += "_races_sprint_export.csv";
		else if(
				checkFileOp == Constants.CheckFileOp.FORCESENSOR_EXPORT_INDIVIDUAL_CURRENT_SESSION_NO_IMAGES ||
				checkFileOp == Constants.CheckFileOp.FORCESENSOR_EXPORT_INDIVIDUAL_ALL_SESSIONS_NO_IMAGES ||
				checkFileOp == Constants.CheckFileOp.FORCESENSOR_EXPORT_GROUPAL_CURRENT_SESSION_NO_IMAGES)
		{
			if (current_mode == Constants.Modes.FORCESENSORISOMETRIC)
				nameString += "_isometric_export.csv";
			else if (current_mode == Constants.Modes.FORCESENSORELASTIC)
				nameString += "_elastic_export.csv";
		}
		else if(
				checkFileOp == Constants.CheckFileOp.RUNENCODER_EXPORT_INDIVIDUAL_CURRENT_SESSION_NO_IMAGES ||
				checkFileOp == Constants.CheckFileOp.RUNENCODER_EXPORT_INDIVIDUAL_ALL_SESSIONS_NO_IMAGES ||
				checkFileOp == Constants.CheckFileOp.RUNENCODER_EXPORT_GROUPAL_CURRENT_SESSION_NO_IMAGES)
			nameString += "_raceAnalyzer_export.csv";
		else if(checkFileOp == Constants.CheckFileOp.ENCODER_ANALYZE_SAVE_TABLE ||
				checkFileOp == Constants.CheckFileOp.ENCODER_ANALYZE_SAVE_TABLE_CURRENT_SESSION)
			nameString += "_encoder_curves_table.csv";
		else if(checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_SAVE_TABLE)
			nameString += "_runs_sprint_table.csv";
		else if(checkFileOp == Constants.CheckFileOp.RUNENCODER_ANALYZE_SAVE_TABLE)
			nameString += "_raceAnalyzer_table.csv";

		// 3) prepare and Run the dialog


		Gtk.FileChooserNative fc =
			new Gtk.FileChooserNative (exportString,
					app1,
					FileChooserAction.Save,
					Catalog.GetString("Accept"),
					Catalog.GetString("Cancel")
					);
		fc.CurrentName = nameString;

		if (fc.Run() == (int)ResponseType.Accept) 
		{
			exportFileName = fc.Filename;
			//add ".csv" if needed (because maybe user has removed it)
			if(
					checkFileOp == Constants.CheckFileOp.PERSONS_EXPORT_THIS_SESSION ||
					checkFileOp == Constants.CheckFileOp.PERSONS_EXPORT_ANY_SESSION ||
					checkFileOp == Constants.CheckFileOp.ENCODER_CAPTURE_EXPORT_SIGNAL ||
					checkFileOp == Constants.CheckFileOp.ENCODER_ANALYZE_SAVE_AB ||
					checkFileOp == Constants.CheckFileOp.FORCESENSOR_ANALYZE_SAVE_AB ||
					checkFileOp == Constants.CheckFileOp.FORCESENSOR_ANALYZE_SAVE_CD ||
					checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_SAVE_TABLE ||
					checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_EXPORT_INDIVIDUAL_CURRENT_SESSION_NO_IMAGES ||
					checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_EXPORT_INDIVIDUAL_ALL_SESSIONS_NO_IMAGES ||
					checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_EXPORT_GROUPAL_CURRENT_SESSION_NO_IMAGES ||
					checkFileOp == Constants.CheckFileOp.FORCESENSOR_EXPORT_INDIVIDUAL_CURRENT_SESSION_NO_IMAGES ||
					checkFileOp == Constants.CheckFileOp.FORCESENSOR_EXPORT_INDIVIDUAL_ALL_SESSIONS_NO_IMAGES ||
					checkFileOp == Constants.CheckFileOp.FORCESENSOR_EXPORT_GROUPAL_CURRENT_SESSION_NO_IMAGES ||
					checkFileOp == Constants.CheckFileOp.RUNENCODER_EXPORT_INDIVIDUAL_CURRENT_SESSION_NO_IMAGES ||
					checkFileOp == Constants.CheckFileOp.RUNENCODER_EXPORT_INDIVIDUAL_ALL_SESSIONS_NO_IMAGES ||
					checkFileOp == Constants.CheckFileOp.RUNENCODER_EXPORT_GROUPAL_CURRENT_SESSION_NO_IMAGES ||
					checkFileOp == Constants.CheckFileOp.ENCODER_ANALYZE_SAVE_TABLE ||
					checkFileOp == Constants.CheckFileOp.ENCODER_ANALYZE_SAVE_TABLE_CURRENT_SESSION ||
					checkFileOp == Constants.CheckFileOp.RUNENCODER_ANALYZE_SAVE_TABLE)
				exportFileName = Util.AddCsvIfNeeded(exportFileName);
			else {
				//ENCODER_ANALYZE_SAVE_IMAGE, ENCODER_ANALYZE_SAVE_IMAGE_CURRENT_SESSION, FORCESENSOR_SAVE_IMAGE_SIGNAL,
				//FORCESENSOR_SAVE_IMAGE_MODEL, FORCESENSOR_SAVE_IMAGE_RFD_MANUAL
				//… and sure other modes
				exportFileName = Util.AddPngIfNeeded(exportFileName);
			}

			try {
				Config.ErrorInExport = false;

				if(File.Exists(exportFileName))
				{
					LogB.Information(string.Format(
								"File {0} exists with attributes {1}, created at {2}",
								exportFileName, 
								File.GetAttributes(exportFileName), 
								File.GetCreationTime(exportFileName)));
					LogB.Information("Overwrite …");

					ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString(
								"Are you sure you want to overwrite: "), "",
							exportFileName);

					// TODO: add switch
					if (checkFileOp == Constants.CheckFileOp.PERSONS_EXPORT_THIS_SESSION)
						confirmWin.Button_accept.Clicked +=
							new EventHandler(on_overwrite_file_persons_export_this_session_accepted);
					else if (checkFileOp == Constants.CheckFileOp.PERSONS_EXPORT_ANY_SESSION)
						confirmWin.Button_accept.Clicked +=
							new EventHandler(on_overwrite_file_persons_export_all_sessions_accepted);
					else if(checkFileOp == Constants.CheckFileOp.JUMPS_SIMPLE_CAPTURE_SAVE_IMAGE ||
							checkFileOp == Constants.CheckFileOp.JUMPS_REACTIVE_CAPTURE_SAVE_IMAGE)
						confirmWin.Button_accept.Clicked +=
							new EventHandler(on_overwrite_file_jumps_capture_save_image_accepted);
					else if(checkFileOp == Constants.CheckFileOp.JUMPS_PROFILE_SAVE_IMAGE)
						confirmWin.Button_accept.Clicked +=
							new EventHandler(on_overwrite_file_jumps_profile_save_image_accepted);
					else if(checkFileOp == Constants.CheckFileOp.JUMPS_DJ_OPTIMAL_FALL_SAVE_IMAGE)
						confirmWin.Button_accept.Clicked +=
							new EventHandler(on_overwrite_file_jumps_dj_optimal_fall_save_image_accepted);
					else if(checkFileOp == Constants.CheckFileOp.JUMPS_ASYMMETRY_BILATERAL_SAVE_IMAGE ||
							checkFileOp == Constants.CheckFileOp.JUMPS_ASYMMETRY_ASYMMETRY_SAVE_IMAGE)
						confirmWin.Button_accept.Clicked +=
							new EventHandler(on_overwrite_file_jumps_asymmetry_save_image_accepted);
					else if(checkFileOp == Constants.CheckFileOp.JUMPS_WEIGHT_FV_PROFILE_SAVE_IMAGE)
						confirmWin.Button_accept.Clicked +=
							new EventHandler(on_overwrite_file_jumps_weight_fv_profile_save_image_accepted);
					else if(checkFileOp == Constants.CheckFileOp.JUMPS_EVOLUTION_SAVE_IMAGE)
						confirmWin.Button_accept.Clicked +=
							new EventHandler(on_overwrite_file_jumps_evolution_save_image_accepted);
					else if(checkFileOp == Constants.CheckFileOp.JUMPS_RJ_FATIGUE_SAVE_IMAGE)
						confirmWin.Button_accept.Clicked +=
							new EventHandler(on_overwrite_file_jumps_rj_fatigue_save_image_accepted);
					else if(checkFileOp == Constants.CheckFileOp.RUNS_SIMPLE_CAPTURE_SAVE_IMAGE ||
							checkFileOp == Constants.CheckFileOp.RUNS_INTERVALLIC_CAPTURE_SAVE_IMAGE)
						confirmWin.Button_accept.Clicked +=
							new EventHandler(on_overwrite_file_runs_capture_save_image_accepted);
					else if(checkFileOp == Constants.CheckFileOp.RUNS_EVOLUTION_SAVE_IMAGE)
						confirmWin.Button_accept.Clicked +=
							new EventHandler(on_overwrite_file_runs_evolution_save_image_accepted);
					else if(checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_SAVE_IMAGE)
						confirmWin.Button_accept.Clicked +=
							new EventHandler(on_overwrite_file_runs_sprint_save_image_accepted);
					else if(checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_SAVE_TABLE)
						confirmWin.Button_accept.Clicked +=
							new EventHandler(on_overwrite_file_runs_sprint_save_table_accepted);
					else if(checkFileOp == Constants.CheckFileOp.ENCODER_CAPTURE_SAVE_IMAGE)
						confirmWin.Button_accept.Clicked +=
							new EventHandler(on_overwrite_file_encoder_capture_save_image_accepted);
					else if(checkFileOp == Constants.CheckFileOp.ENCODER_CAPTURE_EXPORT_SIGNAL)
						confirmWin.Button_accept.Clicked += 
							new EventHandler(on_overwrite_file_export_all_curves_accepted);
					else if(checkFileOp == Constants.CheckFileOp.ENCODER_ANALYZE_SAVE_IMAGE ||
							checkFileOp == Constants.CheckFileOp.ENCODER_ANALYZE_SAVE_IMAGE_CURRENT_SESSION)
						confirmWin.Button_accept.Clicked += 
							new EventHandler(on_overwrite_file_encoder_analyze_save_image_accepted);
					else if(checkFileOp == Constants.CheckFileOp.ENCODER_ANALYZE_SAVE_AB)
						confirmWin.Button_accept.Clicked += 
							new EventHandler(on_overwrite_file_encoder_save_AB_accepted);
					else if(checkFileOp == Constants.CheckFileOp.ENCODER_ANALYZE_SAVE_TABLE ||
							checkFileOp == Constants.CheckFileOp.ENCODER_ANALYZE_SAVE_TABLE_CURRENT_SESSION)
						confirmWin.Button_accept.Clicked += 
							new EventHandler(on_overwrite_file_encoder_save_table_accepted);
					else if(checkFileOp == Constants.CheckFileOp.FORCESENSOR_SAVE_IMAGE_SIGNAL)
						confirmWin.Button_accept.Clicked +=
							new EventHandler(on_overwrite_file_forcesensor_save_image_signal_accepted);
					else if(checkFileOp == Constants.CheckFileOp.FORCESENSOR_SAVE_IMAGE_MODEL)
						confirmWin.Button_accept.Clicked +=
							new EventHandler(on_overwrite_file_forcesensor_save_image_rfd_model_accepted);
					else if(checkFileOp == Constants.CheckFileOp.FORCESENSOR_SAVE_IMAGE_RFD_MANUAL)
						confirmWin.Button_accept.Clicked +=
							new EventHandler(on_overwrite_file_forcesensor_save_image_rfd_manual_accepted);
					else if(checkFileOp == Constants.CheckFileOp.FORCESENSOR_ANALYZE_SAVE_AB)
						confirmWin.Button_accept.Clicked +=
							new EventHandler(on_overwrite_file_forcesensor_save_AB_accepted);
					else if(checkFileOp == Constants.CheckFileOp.FORCESENSOR_ANALYZE_SAVE_CD)
						confirmWin.Button_accept.Clicked +=
							new EventHandler(on_overwrite_file_forcesensor_save_CD_accepted);
					else if(
							checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_EXPORT_INDIVIDUAL_CURRENT_SESSION_NO_IMAGES ||
							checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_EXPORT_INDIVIDUAL_ALL_SESSIONS_NO_IMAGES ||
							checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_EXPORT_GROUPAL_CURRENT_SESSION_NO_IMAGES )
						confirmWin.Button_accept.Clicked +=
							new EventHandler(on_overwrite_file_sprint_export_accepted);
					else if(
							checkFileOp == Constants.CheckFileOp.FORCESENSOR_EXPORT_INDIVIDUAL_CURRENT_SESSION_NO_IMAGES ||
							checkFileOp == Constants.CheckFileOp.FORCESENSOR_EXPORT_INDIVIDUAL_ALL_SESSIONS_NO_IMAGES ||
							checkFileOp == Constants.CheckFileOp.FORCESENSOR_EXPORT_GROUPAL_CURRENT_SESSION_NO_IMAGES )
						confirmWin.Button_accept.Clicked +=
							new EventHandler(on_overwrite_file_forcesensor_export_accepted);
					else if(
							checkFileOp == Constants.CheckFileOp.RUNENCODER_EXPORT_INDIVIDUAL_CURRENT_SESSION_NO_IMAGES ||
							checkFileOp == Constants.CheckFileOp.RUNENCODER_EXPORT_INDIVIDUAL_ALL_SESSIONS_NO_IMAGES ||
							checkFileOp == Constants.CheckFileOp.RUNENCODER_EXPORT_GROUPAL_CURRENT_SESSION_NO_IMAGES )
						confirmWin.Button_accept.Clicked +=
							new EventHandler(on_overwrite_file_runencoder_export_accepted);
					else if(checkFileOp == Constants.CheckFileOp.RUNENCODER_CAPTURE_SAVE_IMAGE)
						confirmWin.Button_accept.Clicked +=
							new EventHandler(on_overwrite_file_runencoder_capture_image_save_accepted);
					else if(checkFileOp == Constants.CheckFileOp.RUNENCODER_ANALYZE_SAVE_IMAGE_MODEL)
						confirmWin.Button_accept.Clicked +=
							new EventHandler(on_overwrite_file_runencoder_analyze_image_save_accepted);
					else if(checkFileOp == Constants.CheckFileOp.RUNENCODER_ANALYZE_SAVE_TABLE)
						confirmWin.Button_accept.Clicked +=
							new EventHandler(on_overwrite_file_raceAnalyzer_save_table_accepted);

				} else {
					if (checkFileOp == Constants.CheckFileOp.PERSONS_EXPORT_THIS_SESSION)
						on_persons_export_this_session_selected (exportFileName);
					else if (checkFileOp == Constants.CheckFileOp.PERSONS_EXPORT_ANY_SESSION)
						on_persons_export_all_sessions_selected (exportFileName);
					else if(checkFileOp == Constants.CheckFileOp.JUMPS_SIMPLE_CAPTURE_SAVE_IMAGE ||
							checkFileOp == Constants.CheckFileOp.JUMPS_REACTIVE_CAPTURE_SAVE_IMAGE)
						on_button_jumps_capture_save_image_selected (exportFileName);
					else if(checkFileOp == Constants.CheckFileOp.JUMPS_PROFILE_SAVE_IMAGE)
						on_button_jumps_profile_save_image_selected (exportFileName);
					else if(checkFileOp == Constants.CheckFileOp.JUMPS_DJ_OPTIMAL_FALL_SAVE_IMAGE)
						on_button_jumps_dj_optimal_fall_save_image_selected (exportFileName);
					else if(checkFileOp == Constants.CheckFileOp.JUMPS_ASYMMETRY_BILATERAL_SAVE_IMAGE ||
							checkFileOp == Constants.CheckFileOp.JUMPS_ASYMMETRY_ASYMMETRY_SAVE_IMAGE)
						on_button_jumps_asymmetry_save_image_selected (exportFileName);
					else if(checkFileOp == Constants.CheckFileOp.JUMPS_WEIGHT_FV_PROFILE_SAVE_IMAGE)
						on_button_jumps_weight_fv_profile_save_image_selected (exportFileName);
					else if(checkFileOp == Constants.CheckFileOp.JUMPS_EVOLUTION_SAVE_IMAGE)
						on_button_jumps_evolution_save_image_selected (exportFileName);
					else if(checkFileOp == Constants.CheckFileOp.JUMPS_RJ_FATIGUE_SAVE_IMAGE)
						on_button_jumps_rj_fatigue_save_image_selected (exportFileName);
					else if(checkFileOp == Constants.CheckFileOp.RUNS_SIMPLE_CAPTURE_SAVE_IMAGE ||
							checkFileOp == Constants.CheckFileOp.RUNS_INTERVALLIC_CAPTURE_SAVE_IMAGE)
						on_button_runs_capture_save_image_selected (exportFileName);
					else if(checkFileOp == Constants.CheckFileOp.RUNS_EVOLUTION_SAVE_IMAGE)
						on_button_runs_evolution_save_image_selected (exportFileName);
					else if(checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_SAVE_IMAGE)
						on_button_runs_sprint_save_image_selected (exportFileName);
					else if(checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_SAVE_TABLE)
						on_button_runs_sprint_save_table_selected (exportFileName);
					else if(checkFileOp == Constants.CheckFileOp.ENCODER_CAPTURE_SAVE_IMAGE)
						on_button_encoder_capture_save_image_file_selected (exportFileName);
					else if(checkFileOp == Constants.CheckFileOp.ENCODER_CAPTURE_EXPORT_SIGNAL)
						on_button_encoder_export_signal_file_selected (exportFileName);
					else if(checkFileOp == Constants.CheckFileOp.ENCODER_ANALYZE_SAVE_IMAGE ||
							checkFileOp == Constants.CheckFileOp.ENCODER_ANALYZE_SAVE_IMAGE_CURRENT_SESSION)
						on_button_encoder_analyze_save_image_file_selected (exportFileName);
					else if(checkFileOp == Constants.CheckFileOp.ENCODER_ANALYZE_SAVE_AB)
						on_button_encoder_save_AB_file_selected (exportFileName);
					else if(checkFileOp == Constants.CheckFileOp.ENCODER_ANALYZE_SAVE_TABLE ||
							checkFileOp == Constants.CheckFileOp.ENCODER_ANALYZE_SAVE_TABLE_CURRENT_SESSION)
						on_button_encoder_save_table_file_selected (exportFileName, true);
					else if(checkFileOp == Constants.CheckFileOp.FORCESENSOR_SAVE_IMAGE_SIGNAL)
						on_button_forcesensor_save_image_signal_file_selected (exportFileName);
					else if(checkFileOp == Constants.CheckFileOp.FORCESENSOR_SAVE_IMAGE_MODEL)
						on_button_forcesensor_save_image_rfd_model_file_selected (exportFileName);
					else if(checkFileOp == Constants.CheckFileOp.FORCESENSOR_SAVE_IMAGE_RFD_MANUAL)
						on_button_forcesensor_save_image_rfd_manual_file_selected (exportFileName);
					else if(checkFileOp == Constants.CheckFileOp.FORCESENSOR_ANALYZE_SAVE_AB)
						on_button_force_sensor_save_AB_file_selected (exportFileName);
					else if(checkFileOp == Constants.CheckFileOp.FORCESENSOR_ANALYZE_SAVE_CD)
						on_button_force_sensor_save_CD_file_selected (exportFileName);
					else if(
							checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_EXPORT_INDIVIDUAL_CURRENT_SESSION_NO_IMAGES ||
							checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_EXPORT_INDIVIDUAL_ALL_SESSIONS_NO_IMAGES ||
							checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_EXPORT_GROUPAL_CURRENT_SESSION_NO_IMAGES )
						on_button_sprint_export_file_selected (exportFileName);
					else if(
							checkFileOp == Constants.CheckFileOp.FORCESENSOR_EXPORT_INDIVIDUAL_CURRENT_SESSION_NO_IMAGES ||
							checkFileOp == Constants.CheckFileOp.FORCESENSOR_EXPORT_INDIVIDUAL_ALL_SESSIONS_NO_IMAGES ||
							checkFileOp == Constants.CheckFileOp.FORCESENSOR_EXPORT_GROUPAL_CURRENT_SESSION_NO_IMAGES )
						on_button_force_sensor_export_file_selected (exportFileName);
					else if(
							checkFileOp == Constants.CheckFileOp.RUNENCODER_EXPORT_INDIVIDUAL_CURRENT_SESSION_NO_IMAGES ||
							checkFileOp == Constants.CheckFileOp.RUNENCODER_EXPORT_INDIVIDUAL_ALL_SESSIONS_NO_IMAGES ||
							checkFileOp == Constants.CheckFileOp.RUNENCODER_EXPORT_GROUPAL_CURRENT_SESSION_NO_IMAGES )
						on_button_run_encoder_export_file_selected (exportFileName);
					else if(checkFileOp == Constants.CheckFileOp.RUNENCODER_CAPTURE_SAVE_IMAGE)
						on_button_run_encoder_capture_image_save_selected (exportFileName);
					else if(checkFileOp == Constants.CheckFileOp.RUNENCODER_ANALYZE_SAVE_IMAGE_MODEL)
						on_button_run_encoder_analyze_image_save_selected (exportFileName);
					else if(checkFileOp == Constants.CheckFileOp.RUNENCODER_ANALYZE_SAVE_TABLE)
						on_button_raceAnalyzer_save_table_file_selected (exportFileName);

					//show message, but not in long processes managed by a thread
					if(
							! Config.ErrorInExport &&
							checkFileOp != Constants.CheckFileOp.PERSONS_EXPORT_THIS_SESSION &&
							checkFileOp != Constants.CheckFileOp.PERSONS_EXPORT_ANY_SESSION &&
							checkFileOp != Constants.CheckFileOp.RUNS_SPRINT_EXPORT_INDIVIDUAL_CURRENT_SESSION_NO_IMAGES &&
							checkFileOp != Constants.CheckFileOp.RUNS_SPRINT_EXPORT_INDIVIDUAL_ALL_SESSIONS_NO_IMAGES &&
							checkFileOp != Constants.CheckFileOp.RUNS_SPRINT_EXPORT_GROUPAL_CURRENT_SESSION_NO_IMAGES &&
							checkFileOp != Constants.CheckFileOp.FORCESENSOR_EXPORT_INDIVIDUAL_CURRENT_SESSION_NO_IMAGES &&
							checkFileOp != Constants.CheckFileOp.FORCESENSOR_EXPORT_INDIVIDUAL_ALL_SESSIONS_NO_IMAGES &&
							checkFileOp != Constants.CheckFileOp.FORCESENSOR_EXPORT_GROUPAL_CURRENT_SESSION_NO_IMAGES &&
							checkFileOp != Constants.CheckFileOp.RUNENCODER_EXPORT_INDIVIDUAL_CURRENT_SESSION_NO_IMAGES &&
							checkFileOp != Constants.CheckFileOp.RUNENCODER_EXPORT_INDIVIDUAL_ALL_SESSIONS_NO_IMAGES &&
							checkFileOp != Constants.CheckFileOp.RUNENCODER_EXPORT_GROUPAL_CURRENT_SESSION_NO_IMAGES )
					{
						string myString = string.Format(Catalog.GetString("Saved to {0}"), 
								exportFileName);
						if(checkFileOp == Constants.CheckFileOp.ENCODER_CAPTURE_EXPORT_SIGNAL ||
								checkFileOp == Constants.CheckFileOp.ENCODER_ANALYZE_SAVE_AB ||
								checkFileOp == Constants.CheckFileOp.FORCESENSOR_ANALYZE_SAVE_AB ||
								checkFileOp == Constants.CheckFileOp.FORCESENSOR_ANALYZE_SAVE_CD)
							myString += Constants.GetSpreadsheetString(preferences.CSVExportDecimalSeparator);
						new DialogMessage(Constants.MessageTypes.INFO, myString);
					}
				}

				if (Config.ErrorInExport)
					new DialogMessage (Constants.MessageTypes.WARNING,
							string.Format (Catalog.GetString ("Cannot save file {0}"), exportFileName) +
							"\n\n" + Catalog.GetString ("Possible causes:") +
							"\n- " + Catalog.GetString ("The disk may be full.") +
							"\n- " + Catalog.GetString ("The file may already be open in another application."));
			} catch {
				string myString = string.Format(
						Catalog.GetString("Cannot save file {0} "), exportFileName);
				new DialogMessage(Constants.MessageTypes.WARNING, myString);
			}
		}
		else {
			LogB.Information("cancelled");
			//report does not currently send the appBar reference
			//new DialogMessage(Constants.MessageTypes.INFO, Catalog.GetString("Cancelled."));
			fc.Hide ();
			return false;
		}
		
		//Don't forget to call Destroy() or the FileChooserNative window won't get closed.
		fc.Destroy();
		
		return true;
	}

	//to export a file check above method
	private bool checkFolder (Constants.CheckFileOp checkFileOp)
	{
		string nameString = checkFolderGetName (checkFileOp);
		return checkFolderSelectFolder (checkFileOp, nameString);
	}

	private string checkFolderGetName (Constants.CheckFileOp checkFileOp)
	{
		string nameString = currentPerson.Name + "_" + currentSession.DateShortAsSQL;
		if(
				checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_EXPORT_GROUPAL_CURRENT_SESSION_YES_IMAGES ||
				checkFileOp == Constants.CheckFileOp.FORCESENSOR_EXPORT_GROUPAL_CURRENT_SESSION_YES_IMAGES ||
				checkFileOp == Constants.CheckFileOp.RUNENCODER_EXPORT_GROUPAL_CURRENT_SESSION_YES_IMAGES)
			nameString = currentSession.Name + "_" + currentSession.DateShortAsSQL;
		else if(
				checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_EXPORT_INDIVIDUAL_ALL_SESSIONS_YES_IMAGES ||
				checkFileOp == Constants.CheckFileOp.FORCESENSOR_EXPORT_INDIVIDUAL_ALL_SESSIONS_YES_IMAGES ||
				checkFileOp == Constants.CheckFileOp.RUNENCODER_EXPORT_INDIVIDUAL_ALL_SESSIONS_YES_IMAGES)
			nameString = currentPerson.Name;

		if(
				checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_EXPORT_INDIVIDUAL_CURRENT_SESSION_YES_IMAGES ||
				checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_EXPORT_INDIVIDUAL_ALL_SESSIONS_YES_IMAGES ||
				checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_EXPORT_GROUPAL_CURRENT_SESSION_YES_IMAGES)
			nameString += "_races_sprint_export";
		else if(
				checkFileOp == Constants.CheckFileOp.FORCESENSOR_EXPORT_INDIVIDUAL_CURRENT_SESSION_YES_IMAGES ||
				checkFileOp == Constants.CheckFileOp.FORCESENSOR_EXPORT_INDIVIDUAL_ALL_SESSIONS_YES_IMAGES ||
				checkFileOp == Constants.CheckFileOp.FORCESENSOR_EXPORT_GROUPAL_CURRENT_SESSION_YES_IMAGES)
		{
			if (current_mode == Constants.Modes.FORCESENSORISOMETRIC)
				nameString += "_isometric_export";
			else if (current_mode == Constants.Modes.FORCESENSORELASTIC)
				nameString += "_elastic_export";
		}
		else if(
				checkFileOp == Constants.CheckFileOp.RUNENCODER_EXPORT_INDIVIDUAL_CURRENT_SESSION_YES_IMAGES ||
				checkFileOp == Constants.CheckFileOp.RUNENCODER_EXPORT_INDIVIDUAL_ALL_SESSIONS_YES_IMAGES ||
				checkFileOp == Constants.CheckFileOp.RUNENCODER_EXPORT_GROUPAL_CURRENT_SESSION_YES_IMAGES)
			nameString += "_raceAnalyzer_export";

		return nameString;
	}

	private bool checkFolderSelectFolder (Constants.CheckFileOp checkFileOp, string nameString)
	{
		FileChooserAction action = FileChooserAction.SelectFolder;
		//mac arm64 crashes on SelectFolder, use Open. The problem in Open is it cannot select a folder that has contents. Only an empty folder
		if (UtilAll.IsMacSilicon ())
			action = FileChooserAction.Open;

		Gtk.FileChooserNative fc =
			new Gtk.FileChooserNative (Catalog.GetString ("Export data and graphs"),
					app1,
					action,
					Catalog.GetString("Accept"),
					Catalog.GetString("Cancel")
					);

		if (fc.Run() == (int)ResponseType.Accept)
		{
			/*
			   it is a folder but we call it exportFileName because this is the expected name on overwrite functions
			   maybe we can change it to exportURL on the future
			   */
			exportFileName = Path.Combine (fc.Filename, nameString);
			LogB.Information("exportFileName: " + exportFileName);

			checkFolderWrite (checkFileOp);
		}
		else {
			LogB.Information("cancelled");
			//report does not currently send the appBar reference
			//new DialogMessage(Constants.MessageTypes.INFO, Catalog.GetString("Cancelled."));
			fc.Hide ();
			return false;
		}

		//Don't forget to call Destroy() or the FileChooserNative window won't get closed.
		fc.Destroy();

		return true;
	}

	private void checkFolderWrite (Constants.CheckFileOp checkFileOp)
	{
		try {
			if(Directory.Exists(exportFileName))
			{
				LogB.Information(string.Format(
							"Dir {0} exists with attributes {1}, created at {2}",
							exportFileName,
							File.GetAttributes(exportFileName),
							File.GetCreationTime(exportFileName)));
				LogB.Information("Overwrite …");

				ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString(
							"Are you sure you want to overwrite: "), "",
						exportFileName);

				if(
						checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_EXPORT_INDIVIDUAL_CURRENT_SESSION_YES_IMAGES ||
						checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_EXPORT_INDIVIDUAL_ALL_SESSIONS_YES_IMAGES ||
						checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_EXPORT_GROUPAL_CURRENT_SESSION_YES_IMAGES )
				{
					confirmWin.Button_accept.Clicked +=
						new EventHandler(on_overwrite_file_sprint_export_accepted);
					confirmWin.Button_cancel.Clicked +=
						new EventHandler(on_overwrite_file_sprint_export_cancelled);
				} else if(
						checkFileOp == Constants.CheckFileOp.FORCESENSOR_EXPORT_INDIVIDUAL_CURRENT_SESSION_YES_IMAGES ||
						checkFileOp == Constants.CheckFileOp.FORCESENSOR_EXPORT_INDIVIDUAL_ALL_SESSIONS_YES_IMAGES ||
						checkFileOp == Constants.CheckFileOp.FORCESENSOR_EXPORT_GROUPAL_CURRENT_SESSION_YES_IMAGES )
				{
					confirmWin.Button_accept.Clicked +=
						new EventHandler(on_overwrite_file_forcesensor_export_accepted);
					confirmWin.Button_cancel.Clicked +=
						new EventHandler(on_overwrite_file_forcesensor_export_cancelled);
				} else if(
						checkFileOp == Constants.CheckFileOp.RUNENCODER_EXPORT_INDIVIDUAL_CURRENT_SESSION_YES_IMAGES ||
						checkFileOp == Constants.CheckFileOp.RUNENCODER_EXPORT_INDIVIDUAL_ALL_SESSIONS_YES_IMAGES ||
						checkFileOp == Constants.CheckFileOp.RUNENCODER_EXPORT_GROUPAL_CURRENT_SESSION_YES_IMAGES )
				{
					confirmWin.Button_accept.Clicked +=
						new EventHandler(on_overwrite_file_runencoder_export_accepted);
					confirmWin.Button_cancel.Clicked +=
						new EventHandler(on_overwrite_file_runencoder_export_cancelled);
				}
			}
			else {
				if(
						checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_EXPORT_INDIVIDUAL_CURRENT_SESSION_YES_IMAGES ||
						checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_EXPORT_INDIVIDUAL_ALL_SESSIONS_YES_IMAGES ||
						checkFileOp == Constants.CheckFileOp.RUNS_SPRINT_EXPORT_GROUPAL_CURRENT_SESSION_YES_IMAGES )
					on_button_sprint_export_file_selected (exportFileName);
				else if(
						checkFileOp == Constants.CheckFileOp.FORCESENSOR_EXPORT_INDIVIDUAL_CURRENT_SESSION_YES_IMAGES ||
						checkFileOp == Constants.CheckFileOp.FORCESENSOR_EXPORT_INDIVIDUAL_ALL_SESSIONS_YES_IMAGES ||
						checkFileOp == Constants.CheckFileOp.FORCESENSOR_EXPORT_GROUPAL_CURRENT_SESSION_YES_IMAGES )
					on_button_force_sensor_export_file_selected (exportFileName);
				else if(
						checkFileOp == Constants.CheckFileOp.RUNENCODER_EXPORT_INDIVIDUAL_CURRENT_SESSION_YES_IMAGES ||
						checkFileOp == Constants.CheckFileOp.RUNENCODER_EXPORT_INDIVIDUAL_ALL_SESSIONS_YES_IMAGES ||
						checkFileOp == Constants.CheckFileOp.RUNENCODER_EXPORT_GROUPAL_CURRENT_SESSION_YES_IMAGES )
					on_button_run_encoder_export_file_selected (exportFileName);
			}
		} catch {
			string myString = string.Format(
					Catalog.GetString("Cannot save file {0} "), exportFileName);
			new DialogMessage(Constants.MessageTypes.WARNING, myString);
		}
	}

	private void on_overwrite_file_export_all_curves_accepted(object o, EventArgs args)
	{
		on_button_encoder_export_signal_file_selected (exportFileName);

		if (Config.ErrorInExport)
			return;

		string myString = string.Format(Catalog.GetString("Saved to {0}"), 
				exportFileName) + Constants.GetSpreadsheetString(preferences.CSVExportDecimalSeparator);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}
	private void on_overwrite_file_encoder_capture_save_image_accepted(object o, EventArgs args)
	{
		on_button_encoder_capture_save_image_file_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), exportFileName);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}
	private void on_overwrite_file_encoder_analyze_save_image_accepted(object o, EventArgs args)
	{
		on_button_encoder_analyze_save_image_file_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), exportFileName);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}
	private void on_overwrite_file_encoder_save_AB_accepted(object o, EventArgs args)
	{
		on_button_encoder_save_AB_file_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), 
				exportFileName) + Constants.GetSpreadsheetString(preferences.CSVExportDecimalSeparator);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}
	private void on_overwrite_file_encoder_save_table_accepted(object o, EventArgs args)
	{
		on_button_encoder_save_table_file_selected (exportFileName, true);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), exportFileName);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}
	private void on_overwrite_file_forcesensor_save_AB_accepted(object o, EventArgs args)
	{
		on_button_force_sensor_save_AB_file_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"),
				exportFileName) + Constants.GetSpreadsheetString(preferences.CSVExportDecimalSeparator);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}
	private void on_overwrite_file_forcesensor_save_CD_accepted(object o, EventArgs args)
	{
		on_button_force_sensor_save_CD_file_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"),
				exportFileName) + Constants.GetSpreadsheetString(preferences.CSVExportDecimalSeparator);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}

	private void on_overwrite_file_sprint_export_accepted (object o, EventArgs args)
	{
		on_button_sprint_export_file_selected (exportFileName); //file or folder
	}
	private void on_overwrite_file_sprint_export_cancelled(object o, EventArgs args)
	{
		//TODO: sensitivity
	}

	private void on_overwrite_file_forcesensor_export_accepted(object o, EventArgs args)
	{
		on_button_force_sensor_export_file_selected (exportFileName); //file or folder

		/*
		string myString = string.Format(Catalog.GetString("Saved to {0}"),
				exportFileName) + Constants.GetSpreadsheetString(preferences.CSVExportDecimalSeparator);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
		*/
	}
	private void on_overwrite_file_forcesensor_export_cancelled(object o, EventArgs args)
	{
		forceSensorButtonsSensitive(true);
		hbox_ai_export_top_modes.Sensitive = true;
	}

	private void on_overwrite_file_runencoder_export_accepted(object o, EventArgs args)
	{
		on_button_run_encoder_export_file_selected (exportFileName); //file or folder
	}
	private void on_overwrite_file_runencoder_export_cancelled(object o, EventArgs args)
	{
		runEncoderButtonsSensitive(true);
	}

	void on_button_encoder_delete_signal_clicked (object o, EventArgs args) 
	{
		if(preferences.askDeletion) {
			ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString(
						"Are you sure you want to delete this set?"), Catalog.GetString("Saved repetitions related to this set will also be deleted."), "");
			confirmWin.Button_accept.Clicked += new EventHandler(on_button_encoder_delete_signal_accepted);
		} else
			on_button_encoder_delete_signal_accepted (o, args);
	}	

	void on_button_encoder_delete_signal_accepted (object o, EventArgs args) 
	{
		LogB.Information ("on_button_encoder_delete_signal_accepted");
		LogB.Information ("encoderSignalUniqueID: " + encoderSignalUniqueID.ToString ());
		EncoderSQL eSQL = (EncoderSQL) SqliteEncoder.Select(
				false, encoderSignalUniqueID, 0, 0, Constants.EncoderGI.ALL,
				-1, "signal", EncoderSQL.Eccons.ALL, "", false, true, false)[0];

		//delete signal and related curves (both from SQL and files)
		encoderSignalDelete (eSQL.GetFullURL(false), eSQL.videoURL, encoderSignalUniqueID);
	
		removeSignalFromGuiBecauseDeletedOrCancelled();

		event_execute_label_message.Text = Catalog.GetString("Set deleted");
	}
	void removeSignalFromGuiBecauseDeletedOrCancelled() 
	{
		encoderSignalUniqueID = -1;
		treeviewEncoderCaptureRemoveColumns();
		updateEncoderAnalyzeExercisesPre ();
		cairoPaintBarsPreCurrent = new CairoPaintBarsPreEncoderCurrent (
				encoder_capture_curves_bars_drawingarea_cairo,
				preferences.fontTypeToGraph());
		prepareEventGraphEncoderCurrent = null; //to avoid is repainted again, and sound be repeated;

		encoderButtonsSensitive(encoderSensEnum.DONENOSIGNAL);
	}

	/*
	 * called on:
	 * radio_encoder_analyze_ (when changes)
	 * on captured set
	 * on delete set
	 * on change exercise of set
	 * on change player
	 * on change session
	 */
	private void updateEncoderAnalyzeExercisesPre ()
	{
		string selected = UtilGtk.ComboGetActive(combo_encoder_exercise_analyze);

		createEncoderComboExerciseAndAnalyze();

		Sqlite.Open (); // ---->

		if(radio_encoder_analyze_individual_current_session.Active)
			updateEncoderAnalyzeExercises (true, currentPerson.UniqueID, currentSession.UniqueID, selected);
		else if(radio_encoder_analyze_individual_all_sessions.Active)
			updateEncoderAnalyzeExercises (true, currentPerson.UniqueID, -1, selected);
		else if(radio_encoder_analyze_groupal_current_session.Active)
			updateEncoderAnalyzeExercises (true, -1, currentSession.UniqueID, selected);

		Sqlite.Close (); // <----
	}
	private void updateEncoderAnalyzeExercises (bool dbconOpened, int personID, int sessionID, string selectedPreviously)
	{
		List<int> listFound = SqliteEncoderExercise.SelectAnalyzeExercisesInCurves (
				dbconOpened, personID, sessionID, Constants.GetEncoderGIByMode (current_mode));
		foreach(int i in listFound)
			LogB.Information(i.ToString());

		List<int> rowsToRemove = new List<int>();
		TreeIter iter;
		if(! combo_encoder_exercise_analyze.Model.GetIterFirst(out iter))
			return;

		int count = 0;
		do {
			string str = (string) combo_encoder_exercise_analyze.Model.GetValue (iter, 0);
			if(count == 0)
			{
				//at the moment don't delete All exercises,
				//but in the future do it if there's less than 2
				count ++;
				continue;
			}

			int exID = getExerciseIDFromName (
					encoderExercisesTranslationAndBodyPWeight,
					str, true);

			if(listFound.IndexOf(exID) == -1)
				rowsToRemove.Add(count);

			count ++;
		} while (combo_encoder_exercise_analyze.Model.IterNext (ref iter));

		//remove them starting at end to have the indexes ok
		if(rowsToRemove.Count == 0)
			return;

		for (int i = rowsToRemove.Count - 1; i >= 0; i--)
		{
			//LogB.Information("Deleting row: " + rowsToRemove[i]);
			UtilGtk.ComboDelByPosition(combo_encoder_exercise_analyze, rowsToRemove[i]);
		}

		combo_encoder_exercise_analyze.Active = UtilGtk.ComboMakeActive(
				combo_encoder_exercise_analyze, selectedPreviously);
	}

	private void updateUserCurvesLabelsAndCombo(bool dbconOpened) 
	{

		LogB.Information("updateUserCurvesLabelsAndCombo()");

		label_encoder_user_curves_active_num.Text = encSelReps.RepsActive.ToString();
		label_encoder_user_curves_all_num.Text = encSelReps.RepsAll.ToString();
		
		if(radio_encoder_analyze_individual_current_set.Active)
			updateComboEncoderAnalyzeCurveNumFromCurrentSet ();
		else if(radio_encoder_analyze_individual_current_session.Active) 
		{
			ArrayList data = SqliteEncoder.Select(
					dbconOpened, -1, currentPerson.UniqueID, currentSession.UniqueID, getEncoderGI(),
					getExerciseIDFromEncoderCombo(exerciseCombos.ANALYZE),
					"curve", EncoderSQL.Eccons.ALL, "",
					false, true, true);
			updateComboEncoderAnalyzeCurveNumSavedReps(data);
		} //interperson and intersession modes don't use combo_encoder_analyze_curve_num_combo
		
		if(radio_encoder_analyze_individual_all_sessions.Active) {
			LogB.Information("EncoderInterSessionDateOnXWeights");
			foreach (double d in encSelReps.EncoderInterSessionDateOnXWeights)
				LogB.Information(d.ToString());
		
			createComboEncoderAnalyzeWeights(false);
		}
	
		button_encoder_analyze_sensitiveness();
	}
	
	private void updateComboEncoderAnalyzeCurveNumFromCurrentSet () 
	{
		int rows = UtilGtk.CountRows(encoderCaptureListStore);

		if(ecconLast != "c")
			rows = rows / 2;

		int defaultValue = 0;
		string [] activeCurvesList;
		if(rows == 0)
			activeCurvesList = Util.StringToStringArray("");
		else {
			activeCurvesList = new String[rows];
			for(int i=0; i < rows; i++)
				activeCurvesList[i] = (i+1).ToString();
			defaultValue = 0;
		}

		UtilGtk.ComboUpdate(combo_encoder_analyze_curve_num_combo, activeCurvesList, "");
		combo_encoder_analyze_curve_num_combo.Active = 
			UtilGtk.ComboMakeActive(combo_encoder_analyze_curve_num_combo, activeCurvesList[defaultValue]);
	}
	//saved repetitions
	private void updateComboEncoderAnalyzeCurveNumSavedReps (ArrayList data)
	{
		string [] checkboxes = new string[data.Count]; //to store active or inactive status of curves
		int count = 0;
		foreach(EncoderSQL eSQL in data) {
			checkboxes[count++] = eSQL.status;
		}
		List<int> activeCurvesList = UtilEncoder.GetActiveCheckboxesList(checkboxes);
		UtilGtk.ComboUpdate(combo_encoder_analyze_curve_num_combo, activeCurvesList);
		if(activeCurvesList.Count > 0)
			combo_encoder_analyze_curve_num_combo.Active = 
				UtilGtk.ComboMakeActive(combo_encoder_analyze_curve_num_combo, activeCurvesList[0].ToString());
	}

	string encoderSaveSignalOrCurve (bool dbconOpened, string mode, int selectedID) 
	{
		//mode is different than type. 
		//mode can be curve or signal
		//type is to print on db at type column: curve or signal + (bar or jump)
		string signalOrCurve = "";
		string feedback = "";
		string fileSaved = "";
		string path = "";

		LogB.Debug("At encoderSaveSignalOrCurve");
		
		if(mode == "curve") {
			signalOrCurve = "curve";
			feedback = Catalog.GetString("Saved");
		} else 	{	//mode == "signal"
			signalOrCurve = "signal";
		
			//check if data is ok (maybe encoder was not connected, then don't save this signal)
			EncoderCurve curveExist = treeviewEncoderCaptureCurvesGetCurve(1, false);
			if(curveExist.N == null) 
				return "";
		}
		
		string meanPowerStr = "";
		string meanSpeedStr = "";
		string meanForceStr = "";
		double maxPower = 0;
		double maxSpeed = 0;
		double maxForce = 0;
		double rangeAbs = 0;
		if(mode == "curve")
		{
			EncoderCurve curve = treeviewEncoderCaptureCurvesGetCurve(selectedID,true);

			//some start at ,5 because of the spline filtering
			int curveStart = Convert.ToInt32(decimal.Truncate(Convert.ToDecimal(curve.Start)));

			int duration = Convert.ToInt32(decimal.Truncate(Convert.ToDecimal(curve.Duration)));

			meanPowerStr = curve.MeanPower;
			meanSpeedStr = curve.MeanSpeed;
			meanForceStr = curve.MeanForce;
			maxPower = curve.PeakPowerD;
			maxSpeed = curve.MaxSpeedD;
			maxForce = curve.MaxForceD;
			rangeAbs = curve.RangeAbs;

			if(ecconLast != "c") {
				EncoderCurve curveNext = treeviewEncoderCaptureCurvesGetCurve(selectedID+1,false);
				
				//since isometric phase has been implemented (Chronojump 1.3.6)
				//curveE.start + curveE.duration < curveC.start (because there's isometric between)
				int curveEccEnd = curveStart + duration;
				int curveConStart = Convert.ToInt32(
						decimal.Truncate(Convert.ToDecimal(curveNext.Start)));
				int curveConDuration = Convert.ToInt32(
						decimal.Truncate(Convert.ToDecimal(curveNext.Duration)));
				int isometricDuration = curveConStart - curveEccEnd;

				//duration is duration of ecc + duration of iso + duration of concentric
				duration += (isometricDuration + curveConDuration);
			
				Preferences.EncoderRepetitionCriteria repCriteria =
					preferences.GetEncoderRepetitionCriteria (current_mode);

				if(repCriteria == Preferences.EncoderRepetitionCriteria.ECC_CON)
				{
					meanPowerStr = UtilAll.DivideSafe (curve.MeanPowerD + curveNext.MeanPowerD, 2).ToString();
					meanSpeedStr = UtilAll.DivideSafe (curve.MeanSpeedD + curveNext.MeanSpeedD, 2).ToString();
					meanForceStr = UtilAll.DivideSafe (curve.MeanForceD + curveNext.MeanForceD, 2).ToString();
					maxPower = UtilAll.DivideSafe (curve.PeakPowerD + curveNext.PeakPowerD, 2);
					maxSpeed = UtilAll.DivideSafe (curve.MaxSpeedD + curveNext.MaxSpeedD, 2);
					maxForce = UtilAll.DivideSafe (curve.MaxForceD + curveNext.MaxForceD, 2);
					rangeAbs = UtilAll.DivideSafe (curve.RangeAbs + curveNext.RangeAbs, 2);
				}
				else if(repCriteria == Preferences.EncoderRepetitionCriteria.ECC)
				{
					meanPowerStr = curve.MeanPower;
					meanSpeedStr = curve.MeanSpeed;
					meanForceStr = curve.MeanForce;
					maxPower = curve.PeakPowerD;
					maxSpeed = curve.MaxSpeedD;
					maxForce = curve.MaxForceD;
					rangeAbs = curve.RangeAbs;
				}
				else //if(repCriteria == Preferences.EncoderRepetitionCriteria.CON)
				{
					meanPowerStr = curveNext.MeanPower;
					meanSpeedStr = curveNext.MeanSpeed;
					meanForceStr = curveNext.MeanForce;
					maxPower = curveNext.PeakPowerD;
					maxSpeed = curveNext.MaxSpeedD;
					maxForce = curveNext.MaxForceD;
					rangeAbs = curveNext.RangeAbs;
				}
			}
			
			/*
			 * at inertial signals, first curve is eccentric (can be to left or right, maybe positive or negative)
			 * graph.R manages correctly this
			 * But, when saved a curve, eg. concentric this can be positive or negative
			 * (depending on the rotating sign of inertial machine at that curve)
			 * if it's concentric, and it's full of -1,-2, … we have to change sign
			 * if it's eccentric-concentric, and in the eccentric phase is positive, then we should change sign of both phases
			 * This is done on UtilEncoder.EncoderSaveCurve()
			 */
			int inertialCheckStart = 0;
			int inertialCheckDuration = 0;
			if (current_mode == Constants.Modes.POWERINERTIAL)
			{
				inertialCheckStart = curveStart;
				if(ecconLast == "c")
					inertialCheckDuration = duration;
				else {
					//see if sign is ok just looking if eccentric phase is negative or not
					inertialCheckDuration = Convert.ToInt32(decimal.Truncate(Convert.ToDecimal(curve.Duration)));
				}
			}
		
			LogB.Information(curveStart + "->" + duration);
		
			int curveIDMax;
			int countCurveIDs = Sqlite.Count(Constants.EncoderTable, dbconOpened);
			if(countCurveIDs == 0)
				curveIDMax = 0;
			else
				curveIDMax = Sqlite.Max(Constants.EncoderTable, "uniqueID", dbconOpened);
			
			//save raw file to hard disk
			fileSaved = UtilEncoder.EncoderSaveCurve(UtilEncoder.GetEncoderDataTempFileName(), 
					curveStart, duration,
					inertialCheckStart, inertialCheckDuration, (ecconLast == "c"), 
					currentSession.UniqueID, currentPerson.UniqueID, 
					currentPerson.Name, encoderTimeStamp, curveIDMax);
			
			//there was a problem copying
			if(fileSaved == "")
				return "";

			//save it to SQL (encoderSignalCurve table)
			SqliteEncoderSignalCurve.SignalCurveInsert(dbconOpened, 
					encoderSignalUniqueID, curveIDMax +1,
					Convert.ToInt32(curveStart + (duration /2)));

			path = UtilEncoder.GetEncoderSessionDataCurveDir(currentSession.UniqueID);
		} else //signal
		{
			fileSaved = UtilEncoder.CopyTempToEncoderData (currentSession.UniqueID, currentPerson.UniqueID, 
					currentPerson.Name, encoderTimeStamp);
			
			//there was a problem copying
			if(fileSaved == "")
				return "";

			path = UtilEncoder.GetEncoderSessionDataSignalDir(currentSession.UniqueID);
		}
		
		int myID = -1;
		if(mode == "signal")
			myID = encoderSignalUniqueID;

		//assign values from currentEncoderSQLSet (last calculate curves or reload), and change new things
		//currentEncoderSQLSet has been created on capture (encoderAction.CURVES_AC) before encoderDoCurvesGraphR
		EncoderSQL eSQL = currentEncoderSQLSet;

		eSQL.UniqueID = myID;
		eSQL.signalOrCurve = signalOrCurve;
		eSQL.filename = fileSaved;
		eSQL.url = path;
		//eSQL.Description = "";
		eSQL.hasInertia = (current_mode == Constants.Modes.POWERINERTIAL);

		if(mode == "curve") {
			eSQL.status = "active";
			eSQL.meanPower = meanPowerStr;
			eSQL.meanSpeed = meanSpeedStr;
			eSQL.meanForce = meanForceStr;
			eSQL.maxPower = maxPower;
			eSQL.maxSpeed = maxSpeed;
			eSQL.maxForce = maxForce;
			eSQL.rangeAbs = rangeAbs;
		}

		//eSQL.encoderConfiguration = encoderConfigurationCurrent; // needed?

		//if is a signal that we just loaded, then don't insert, do an update
		//we know it because encoderUniqueID is != than "-1" if we loaded something from database
		//This also saves curves
		if(myID == -1) {
			myID = SqliteEncoder.Insert (dbconOpened, eSQL); //Adding on SQL
			if(mode == "signal") {
				encoderSignalUniqueID = myID;
				feedback = Catalog.GetString("Set saved");
				updatePersonTestsN (false);

				//copy video	
				if(preferences.videoOn) {
					if(Util.CopyTempVideo(currentSession.UniqueID,
								Constants.TestTypes.ENCODER,
								encoderSignalUniqueID))
					{
						eSQL.videoURL = Util.GetVideoFileName(currentSession.UniqueID,
								Constants.TestTypes.ENCODER,
								encoderSignalUniqueID);
						//need assign uniqueID to update and add the URL of video
						eSQL.UniqueID = encoderSignalUniqueID;
						SqliteEncoder.Update(dbconOpened, eSQL);

						button_video_play_this_test.Sensitive = true;
					} else {
						new DialogMessage(Constants.MessageTypes.WARNING,
								Catalog.GetString("Sorry, video cannot be stored."));
					}
				}
			}
		}
		else {
			LogB.Warning("TOSTRING1");
			LogB.Information (eSQL.ToString());
			//only signal is updated
			SqliteEncoder.Update(dbconOpened, eSQL); //Adding on SQL
			LogB.Warning("TOSTRING2");
			LogB.Information (eSQL.ToString());
			feedback = Catalog.GetString("Set updated");
		}
		
		LogB.Debug("At encoderSaveSignalOrCurve done");
		return feedback;
	}


	private void on_button_encoder_analyze_clicked (object o, EventArgs args) 
	{
		if(radio_encoder_analyze_individual_current_session.Active)
		{
			//if current session and no data of this person and session, return
			ArrayList data = SqliteEncoder.Select(
					false, -1, currentPerson.UniqueID, currentSession.UniqueID, getEncoderGI(),
					getExerciseIDFromEncoderCombo(exerciseCombos.ANALYZE), "curve", EncoderSQL.Eccons.ALL, "",
					false, true, true);

			if(data.Count == 0) {
				new DialogMessage(Constants.MessageTypes.WARNING,
						Catalog.GetString("Sorry, no repetitions selected."));
				return;
			}

			//1RM can be individual current set or individual current session
			if(encoderSelectedAnalysis == "1RM")
			{
				string nameTemp = Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_analyze_1RM),
						encoderAnalyze1RMTranslation);

				//cannot do 1RM with different exercises (individual current session)
				if(
						nameTemp == "1RM Any exercise" ||
						nameTemp == Catalog.GetString("1RM Any exercise") ||
						nameTemp == "1RM Bench Press" ||
						nameTemp == Catalog.GetString("1RM Bench Press") ||
						nameTemp == "1RM Squat" ||
						nameTemp == Catalog.GetString("1RM Squat")
						//no 1RM Indirect because cannot be done with saved curves
				  ) {
					bool differentExercises = false;
					string oldExName = "";
					foreach(EncoderSQL eSQL in data)
					{
						if(eSQL.status == "inactive")
							continue;

						string exName = eSQL.ExerciseName;
						if(oldExName != "" && exName != oldExName)
							differentExercises = true;
						oldExName = exName;
					}
					if(differentExercises) {
						new DialogMessage(Constants.MessageTypes.WARNING,
								Catalog.GetString("Sorry, cannot calculate 1RM of different exercises.") + "\n" +
								Catalog.GetString("Please select repetitions of only one exercise type."));
						return;
					}
				}

				//cannot do 1RM Any exercise without the "speed at 1RM" exercise parameter
				if(nameTemp == "1RM Any exercise" || nameTemp == Catalog.GetString("1RM Any exercise"))
				{
					EncoderSQL eSQL = (EncoderSQL) data[0];
					EncoderExercise exTemp = SqliteEncoderExercise.SelectEncoderExercises (
							false , eSQL.exerciseID, false, Constants.EncoderGI.GRAVITATORY)[0];

					if(exTemp.speed1RM == 0) {
						new DialogMessage(Constants.MessageTypes.WARNING,
								string.Format(
									Catalog.GetString("Sorry, parameter: 'speed at 1RM' on exercise: '{0}' cannot be 0 for this analysis."),
									eSQL.ExerciseName) + "\n\n" +
								Catalog.GetString("Please edit exercise parameters on capture tab."));
						return;
					}
				}
			}
		} //end individual current session

		//1RM Any exercise cannot be calculated with just one set (needs different weights)
		if(radio_encoder_analyze_individual_current_set.Active)
		{
			if(encoderSelectedAnalysis == "1RM")
			{
				string nameTemp = Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_analyze_1RM),
						encoderAnalyze1RMTranslation);
				if(nameTemp == "1RM Any exercise" || nameTemp == Catalog.GetString("1RM Any exercise"))
				{
					new DialogMessage(Constants.MessageTypes.WARNING,
							Catalog.GetString("Sorry, cannot calculate this 1RM test on one set."));
					return;
				}
			}
		}

		if( ! radio_encoder_analyze_individual_current_set.Active)
		{
			//cannot do inter/intra person with some cross graphs
			if(encoderSelectedAnalysis == "cross")
			{
				string nameTemp = Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_analyze_cross),
						encoderAnalyzeCrossTranslation);

				if( (radio_encoder_analyze_individual_all_sessions.Active ||
						radio_encoder_analyze_groupal_current_session.Active) &&
						(
						 nameTemp == "(Speed,Power) - Load" ||
						 nameTemp == Catalog.GetString("(Speed,Power) - Load")
						)) {
					new DialogMessage(Constants.MessageTypes.WARNING, 
							Catalog.GetString("Sorry, this graph is not supported yet.") +
							"\n\nIntersession or Interperson - cross variables" +
							"\n- (Speed,Power) - Load"
							);

					return;
				}
			}
			
			//cannot do inter/intra person with some 1RM graphs
			if(encoderSelectedAnalysis == "1RM")
			{
				string nameTemp = Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_analyze_1RM),
						encoderAnalyze1RMTranslation);

				if((radio_encoder_analyze_individual_all_sessions.Active ||
						radio_encoder_analyze_groupal_current_session.Active) &&
						(
						 nameTemp == "1RM Any exercise" ||
						 nameTemp == Catalog.GetString("1RM Any exercise") ||
						 nameTemp == "1RM Bench Press" ||
						 nameTemp == Catalog.GetString("1RM Bench Press") ||
						 nameTemp == "1RM Squat" ||
						 nameTemp == Catalog.GetString("1RM Squat")
						 //no 1RM Indirect because cannot be done with saved curves
						)) {
					new DialogMessage(Constants.MessageTypes.WARNING, 
							Catalog.GetString("Sorry, this graph is not supported yet.") +
							"\n\nIntersession or Interperson" +
							"\n- 1RM Any exercise" +
							"\n- 1RM Bench Press" +
							"\n- 1RM Squat"
							//no 1RM Indirect because cannot be done with saved curves
							);

					return;
				}
				
			}
		}

		//TODO: also only do the graph if there's more than one session selected
		//Pmax(F0,V0) is not translated
		if( encoderSelectedAnalysis == "cross" &&
				UtilGtk.ComboGetActive(combo_encoder_analyze_cross) == "Pmax(F0,V0)" &&
				! radio_encoder_analyze_individual_all_sessions.Active )
		{
			new DialogMessage(Constants.MessageTypes.WARNING,
					Catalog.GetString("Sorry, this graph is not supported yet.") +
					"\n\nPmax(f0,V0) only works at intersession.");

			return;
		}

		button_encoder_analyze.Visible = false;
		hbox_encoder_analyze_progress.Visible = true;
		button_encoder_analyze_cancel.Sensitive = true;

		encoderThreadStart(encoderActions.ANALYZE);
	}

	//this is called by non gtk thread. Don't do gtk stuff here
	//I suppose reading gtk is ok, changing will be the problem
	private void encoderDoCaptureBG ()
	{
		eCaptureInertialBG.CaptureBG (Config.SimulatedCapture);
	}

	private void stopCapturingInertialBG()
	{
		LogB.Information("Stopping capturing Inertial BG");
		eCaptureInertialBG.FinishBG();
		EncoderCaptureInertialBackgroundStatic.Abort();
		eCaptureInertialBG = null;
		vscale_encoder_capture_inertial_angle_now.Value = 0;
		image_encoder_capture_inertial_ecc.Visible = false;
		image_encoder_capture_inertial_con.Visible = false;
	}

	/*
	 * this is called by non gtk thread. Don't do gtk stuff here
	 * I suppose reading gtk is ok, changing will be the problem
	 *
	 * This method captures using Csharp (opposite to very old capture using a python script or directly R)
	 * but the analysis of the data during capture will be done by R or by Csharp depending on cshapOrR
	 */
	private void encoderDoCaptureCsharp ()
	{
		bool capturedOk = eCapture.Capture(
				UtilEncoder.GetEncoderDataTempFileName(),
				encoderRProcCapture,
				configChronojump.Compujump,
				encoderRProcCapture.CutByTriggers,
				encoderRhythm.RestClustersForEncoderCaptureAutoEnding(),
				configChronojump.PlaySoundsFromFile,
				preferences.signalDirectionHorizontal
				);

		//wait to ensure capture thread has ended
		Thread.Sleep(50);	
		
		//on simulated sleep more to ensure data is written to disc
		if (Config.SimulatedCapture)
			Thread.Sleep(1500);

		LogB.Debug("Going to stop");		
		capturingCsharp = encoderCaptureProcess.STOPPING;

		//will start calcule curves thread
		if(capturedOk)
		{
			if(preferences.encoderCaptureInfinite && ! captureContWithCurves)
			{
				LogB.Debug("Don't need to to encoderCalculeCurves");
				encoderTimeStamp = UtilDate.ToFile(DateTime.Now);
				encoderSignalUniqueID = -1; //mark to know that there's no ID for this until it's saved on database
				setCurrentEncoderSQLSetAtCapture ();
			} else
			{
				LogB.Debug("Going to encoderCalculeCurves");
				encoderCalculeCurves (encoderActions.CURVES_AC);
			}
		} else
			encoderProcessCancel = true;
	}
	
	//this is used only on calculating inertia moment
	//this is called by non gtk thread. Don't do gtk stuff here
	//don't change properties like setting a Visibility status: Gtk.Widget.set_Visible
	//I suppose reading gtk is ok, changing will be the problem
	private void encoderDoCaptureCsharpIM ()
	{
		bool capturedOk = eCapture.Capture(
				UtilEncoder.GetEncoderDataTempFileName(),
				encoderRProcCapture,
				false, 	//compujump
				Preferences.TriggerTypes.NO_TRIGGERS,
				0,  //encoderRhythm.RestClustersForEncoderCaptureAutoEnding()
				false, //configChronojump.PlaySoundsFromFile
				preferences.signalDirectionHorizontal//,
				//false
				);

		//wait to ensure capture thread has ended
		Thread.Sleep(500);	

		if(capturedOk)
			UtilEncoder.RunEncoderCalculeIM(
					encoder_configuration_win.Spin_im_weight,
					encoder_configuration_win.Spin_im_length,
					encoderRProcAnalyze
					);
		else
			encoderProcessCancel = true;
	}


	//this is called by non gtk thread. Don't do gtk stuff here
	//I suppose reading gtk is ok, changing will be the problem
	private void encoderDoAnalyze () 
	{
		EncoderParams ep = new EncoderParams();
		string dataFileName = "";
		
		string analysisOptions = getEncoderAnalysisOptions();

		//use this send because we change it to send it to R
		//but we don't want to change encoderSelectedAnalysis because we want to know again if == "cross" (or "1RM")
		//encoderSelectedAnalysis can be "cross" and sendAnalysis be "Speed / Load"
		//encoderSelectedAnalysis can be "1RM" and sendAnalysis be "1RMBadilloBench, …
		string sendAnalysis = encoderSelectedAnalysis;

		//see doProcess at encoder/graph.R
		string analysisVariables = "none"; //cannot be blank
		string titleStr = "";

		string crossName = "";
		if(sendAnalysis == "cross") {
			crossName = Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_analyze_cross),
						encoderAnalyzeCrossTranslation);
			
			if(
					crossName == "Power - Load" || crossName == "Speed - Load" || crossName == "Force - Load" ||
					crossName == "Pmax(F0,V0)" ||
					crossName == "(Speed,Power) - Load" ||
					crossName == "(Force,Power) - Speed"||
					crossName == "Load - Speed"||
					crossName == "Power - Speed" )
			{
				if(crossName == "Pmax(F0,V0)")
					analysisVariables = "Pmax(F0,V0);Pmax(F0,V0)"; //this is not used but we want to preserve chunks between ';'
				else {
					//convert: "(Force,Power) - Speed" in: "(Force,Power);Speed;mean"
					string [] crossNameFull = crossName.Split(new char[] {' '});

					//remove the '(', ')'
					crossNameFull[0] = Util.RemoveChar (crossNameFull[0], '(', false);
					crossNameFull[0] = Util.RemoveChar (crossNameFull[0], ')', false);

					analysisVariables = crossNameFull[0] + ";" + crossNameFull[2]; //[1]=="-"
				}

				if(check_encoder_analyze_mean_or_max.Active)
					analysisVariables += ";mean";
				else
					analysisVariables += ";max";
			} 
			else if (crossName == "Power - Date" || crossName == "Speed - Date" || crossName == "Force - Date")
			{
				/*
				 * In order to recycle paintCrossVariables in encoder/graph.R, 
				 * we send "Force / Date" as "Force;Load;(mean or max);Date" and there variables will be swapped
				 */
				//convert: "Force / Date" in: "Force;Load;mean;Date"
				string [] crossNameFull = crossName.Split(new char[] {' '});
				analysisVariables = crossNameFull[0] + ";Load";
				if(check_encoder_analyze_mean_or_max.Active)
					analysisVariables += ";mean;Date";
				else
					analysisVariables += ";max;Date";
			}

		}
		
		string my1RMName = "";
		if(sendAnalysis == "1RM") {
			my1RMName = Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_analyze_1RM),
						encoderAnalyze1RMTranslation);
			
			//(my1RMName == "1RM Any exercise") done below different for curve and signal
			if(my1RMName == "1RM Bench Press") {
				sendAnalysis = "1RMBadilloBench";
				analysisOptions = "p";
			} else if(my1RMName == "1RM Squat") {
				sendAnalysis = "1RMBadilloSquat";
				analysisOptions = "p";
			}
		}
		
		if(sendAnalysis == "powerBars" || sendAnalysis == "single" || sendAnalysis == "singleAllSet" ||
				sendAnalysis == "side" || sendAnalysis == "sideShareX" || sendAnalysis == "superpose")
		{
			analysisVariables = getAnalysisVariables(sendAnalysis);
		}

		if( ! radio_encoder_analyze_individual_current_set.Active) //not current set
		{
			string myEccon = "ec";
			if(! check_encoder_analyze_eccon_together.Active)
				myEccon = "ecS";
			int myCurveNum = -1;
			if(sendAnalysis == "single")
				myCurveNum = Convert.ToInt32(UtilGtk.ComboGetActive(
							combo_encoder_analyze_curve_num_combo));

			
			dataFileName = UtilEncoder.GetEncoderGraphInputMulti();

			//neuromuscularProfile works only with ec, do not use c curves
			EncoderSQL.Eccons ecconSelect = EncoderSQL.Eccons.ALL; 	
			if(encoderSelectedAnalysis == "neuromuscularProfile") {
				ecconSelect = EncoderSQL.Eccons.ecS; 	
			}

			//double bodyMass = Convert.ToDouble(currentPersonSession.Weight);

			//select curves for this person
			ArrayList data = new ArrayList();

			if(radio_encoder_analyze_individual_current_session.Active)
			{
				//select currentPerson, currentSession curves
				//onlyActive is false to have all the curves
				//this is a need for "single" to select on display correct curve
				data = SqliteEncoder.Select(
						false, -1, currentPerson.UniqueID, currentSession.UniqueID, getEncoderGI(),
						getExerciseIDFromEncoderCombo(exerciseCombos.ANALYZE),
						"curve", ecconSelect, getLateralityOnAnalyzeToSQL(),
						false, true, true);
			}
			else if(radio_encoder_analyze_groupal_current_session.Active) 
			{
				for (int i=0 ; i < encSelReps.EncoderCompareInter.Count ; i ++) {
					ArrayList dataPre = SqliteEncoder.Select(
							false, -1, 
							Util.FetchID(encSelReps.EncoderCompareInter[i].ToString()),
							currentSession.UniqueID,
							getEncoderGI(),
							getExerciseIDFromEncoderCombo(exerciseCombos.ANALYZE),
							"curve", EncoderSQL.Eccons.ALL, getLateralityOnAnalyzeToSQL(),
							false, //onlyActive=false. Means: all saved repetitions
							true, true);
					foreach(EncoderSQL eSQL in dataPre) {
						eSQL.status = "active"; //force all to be active on interperson
						data.Add(eSQL);
					}
				}
				LogB.Information("ENCODERCOMPAREINTER GROUP");
				foreach (string str in encSelReps.EncoderCompareInter)
					LogB.Information(str);
			} else if(radio_encoder_analyze_individual_all_sessions.Active) 
			{
				for (int i=0 ; i < encSelReps.EncoderCompareInter.Count ; i ++) {
					ArrayList dataPre = SqliteEncoder.Select(
							false, -1,
							currentPerson.UniqueID, 
							Util.FetchID(encSelReps.EncoderCompareInter[i].ToString()),
							getEncoderGI(),
							getExerciseIDFromEncoderCombo(exerciseCombos.ANALYZE),
							"curve", EncoderSQL.Eccons.ALL, getLateralityOnAnalyzeToSQL(),
							false, //onlyActive=false. Means: all saved repetitions
							true, true);
					foreach(EncoderSQL eSQL in dataPre) {
						string comboWeightsValue = UtilGtk.ComboGetActive(combo_encoder_analyze_weights);
						if(check_encoder_intersession_x_is_date.Active &&
								comboWeightsValue != Catalog.GetString("All weights") &&
								comboWeightsValue != Util.ChangeDecimalSeparator(eSQL.extraWeight))
							continue;
							
						eSQL.status = "active"; //force all to be active on intersession
						data.Add(eSQL);
					}
				}
				LogB.Information("ENCODERCOMPAREINTER INTERSESSION");
				foreach (string str in encSelReps.EncoderCompareInter)
					LogB.Information(str);
			}
			
			//1RM is calculated using curves
			//cannot be curves of different exercises
			//because is 1RM of a person on an exercise
			//this is checked at: "on_button_encoder_analyze_clicked()"
			if(encoderSelectedAnalysis == "1RM" &&
					(my1RMName == "1RM Bench Press" || my1RMName == "1RM Squat" || my1RMName == "1RM Any exercise") )
			{
				//get exercise ID
				int exID = -1;
				foreach(EncoderSQL eSQL in data) {
					if(eSQL.status == "active") { 
						exID = eSQL.exerciseID;
						break;
					}
				}

				if(my1RMName == "1RM Any exercise") {
					//get speed1RM (from exercise of curve on SQL, not from combo)
					EncoderExercise exTemp = SqliteEncoderExercise.SelectEncoderExercises (
							false , exID, false, Constants.EncoderGI.GRAVITATORY)[0];
				
					sendAnalysis = "1RMAnyExercise";
				        analysisVariables = Util.ConvertToPoint(exTemp.speed1RM) + ";" +
						SqlitePreferences.Select("encoder1RMMethod");
					analysisOptions = "p";
				}
			}

			//-1 because data will be different on any curve
			ep = new EncoderParams (
					preferences.EncoderCaptureMinHeight (current_mode == Constants.Modes.POWERINERTIAL),
					-1, 		//exercisePercentBodyWeight
					"-1",		//massBody
					"-1",		//massExtra
					myEccon,	//this decides if analysis will be together or separated
					sendAnalysis,
					analysisVariables,
					analysisOptions,
					preferences.encoderCaptureCheckFullyExtended,
					preferences.encoderCaptureCheckFullyExtendedValue,
					new EncoderConfiguration(),
					Util.ConvertToPoint(preferences.encoderSmoothCon),	//R decimal: '.'
					myCurveNum,
					image_encoder_width, 
					image_encoder_height,
					preferences.CSVExportDecimalSeparator 
					);


			//create dataFileName
			TextWriter writer = File.CreateText(dataFileName);
			writer.WriteLine("status,seriesName,exerciseName,massBody,massExtra,dateTime,fullURL,eccon,percentBodyWeight," + 
					"econfName, econfd, econfD, econfAnglePush, econfAngleWeight, econfInertia, econfGearedDown, laterality");

			List<EncoderExercise> ex_l = SqliteEncoderExercise.SelectEncoderExercises (
					false, -1, false, Constants.GetEncoderGIByMode (current_mode));
			EncoderExercise ex = new EncoderExercise();
						
			LogB.Information("AT ANALYZE");

			int iteratingPerson = -1;
			int iteratingSession = -1;
			double iteratingMassBody = -1;
			int countSeries = 1;

			Sqlite.Open();	
			foreach(EncoderSQL eSQL in data) {
				foreach(EncoderExercise eeSearch in ex_l)
					if(eSQL.exerciseID == eeSearch.uniqueID)
						ex = eeSearch;

				LogB.Debug(" AT ANALYZE 1.1 ");
				//massBody change if we are comparing different persons or sessions
				if(eSQL.PersonID != iteratingPerson || eSQL.SessionID != iteratingSession) {
					iteratingMassBody = SqlitePersonSession.SelectAttribute(
							true, eSQL.PersonID, eSQL.SessionID, Constants.Weight);
				}
				LogB.Debug(" AT ANALYZE 1.2 ");

				//seriesName
				string seriesName = "";
				if(radio_encoder_analyze_groupal_current_session.Active)
				{
					foreach(string str in encSelReps.EncoderCompareInter)
						if(Util.FetchID(str) == eSQL.PersonID)
						{
							seriesName = Util.FetchName(str);
							//to show correctly name of person on title if there is only one serie (one person)
							//because if is multiseries it displays	correctly the names as series, but if not, it displayed currentPerson.Name
							//so just display one person name and if it has only one serie (one person), name will be ok
							titleStr = seriesName;
						}
				} else if(radio_encoder_analyze_individual_all_sessions.Active)
				{
					foreach(string str in encSelReps.EncoderCompareInter) {
						LogB.Information(str);
						if(Util.FetchID(str) == eSQL.SessionID)
							seriesName = Util.FetchName(str);
					}
					if(seriesName == "")
						seriesName = currentSession.DateShortAsSQL;
				}
				if(seriesName == "")
					seriesName = currentPerson.Name;

				/*
				 * to avoid problems on reading files from R and strange character encoding
				 * (this problem happens in Parallels (MacOSX)
				 * copy to temp
				 * and tell the csv file that it's in temp
				 */

				string safeFullURL = Path.Combine(Path.GetTempPath(),
						"chronojump_enc_curve_" + countSeries.ToString() + ".txt");
				string fullURL = safeFullURL; 
				
				try {
					File.Copy(eSQL.GetFullURL(false), safeFullURL, true);
					fullURL = fullURL.Replace("\\","/");	//R needs this separator path: "/" in all systems 
				} catch {
					fullURL = eSQL.GetFullURL(true);	//convertPathToR
				}

				writer.WriteLine(eSQL.status + "," + 
						Util.ChangeChars(seriesName,","," ") + "," + //person name cannot have commas
						ex.name + "," +
						Util.ConvertToPoint(iteratingMassBody).ToString() + "," + 
						Util.ConvertToPoint(Convert.ToDouble(eSQL.extraWeight)) + "," +
						eSQL.GetDatetimeStr(true) + "," +
						fullURL + "," +	
						eSQL.eccon + "," + 	//this is the eccon of every curve
						ex.percentBodyWeight.ToString() + "," +
						eSQL.encoderConfiguration.ToStringOutput(EncoderConfiguration.Outputs.RCSV) + "," +
						eSQL.LateralityToEnglish()
						);
				countSeries ++;
			}
			writer.Flush();
			LogB.Debug(" closing writer ");
			writer.Close();
			LogB.Debug(" disposing writer ");
			((IDisposable)writer).Dispose();
			LogB.Debug(" AT ANALYZE 2 ");
			Sqlite.Close();	

		} else {	//current set
			if(encoderSelectedAnalysis == "1RM") {
				if(my1RMName == "1RM Any exercise") {
					//get speed1RM (from combo)
					EncoderExercise ex = SqliteEncoderExercise.SelectEncoderExercises(
							false, currentEncoderSQLSet.exerciseID,
							false, Constants.EncoderGI.GRAVITATORY)[0];

					sendAnalysis = "1RMAnyExercise";
					analysisVariables = Util.ConvertToPoint(ex.speed1RM) + ";" + 
						SqlitePreferences.Select("encoder1RMMethod");
					analysisOptions = "p";
				}
				else if(my1RMName == "1RM Indirect") {
					sendAnalysis = "1RMIndirect";
				}
			}
			
			//if combo_encoder_analyze_curve_num_combo "All" is selected, then use a 0, else get the number
			int curveNum = 0;
			if(radiobutton_encoder_analyze_all_set.Active)
				curveNum = 0;
			else if(radiobutton_encoder_analyze_single.Active)
			{
				if(Util.IsNumber(UtilGtk.ComboGetActive(combo_encoder_analyze_curve_num_combo), false))
					curveNum = Convert.ToInt32(UtilGtk.ComboGetActive(combo_encoder_analyze_curve_num_combo));
			}

			ep = new EncoderParams(
					currentEncoderSQLSet.minHeight,
					getExercisePercentBodyWeightFromID (currentEncoderSQLSet.exerciseID),
					Util.ConvertToPoint (findMassFromGui (Constants.MassType.BODY)), //no problem, set is of current person
					Util.ConvertToPoint (currentEncoderSQLSet.extraWeightD),
					findEcconFromAnalyzeGui (false),	//do not force ecS (ecc-conc separated)
					sendAnalysis,
					analysisVariables, 
					analysisOptions,
					preferences.encoderCaptureCheckFullyExtended,
					preferences.encoderCaptureCheckFullyExtendedValue,
					currentEncoderSQLSet.encoderConfiguration,
					Util.ConvertToPoint(preferences.encoderSmoothCon),	//R decimal: '.'
					curveNum,
					image_encoder_width,
					image_encoder_height,
					preferences.CSVExportDecimalSeparator 
					);
			
			dataFileName = UtilEncoder.GetEncoderDataTempFileName();
		}

		EncoderStruct encoderStruct = new EncoderStruct(
				dataFileName, 
				UtilEncoder.GetEncoderGraphTempFileName(),
				UtilEncoder.GetEncoderAnalyzeTableTempFileName(),
				UtilEncoder.GetEncoderScriptsPathWithoutLastSep(),
				UtilEncoder.GetEncoderTempPathWithoutLastSep(),
				ep);


		if(! radio_encoder_analyze_groupal_current_session.Active)
			titleStr = Util.ChangeSpaceAndMinusForUnderscore(currentPerson.Name);

		if(encoderSelectedAnalysis == "neuromuscularProfile")
		{
			if (radio_encoder_analyze_groupal_current_session.Active)
				titleStr = "Neuromuscular Profile";
			else
				titleStr = "Neuromuscular Profile" + "-" + titleStr;
		}
		else {
			//on signal show encoder exercise, but not in curves because every curve can be of a different exercise
			if(radio_encoder_analyze_individual_current_set.Active) //current set
			{
			//	titleStr += "-" + Util.ChangeSpaceAndMinusForUnderscore(UtilGtk.ComboGetActive(combo_encoder_exercise_capture)); //TODO
				titleStr += "-" + Util.ChangeSpaceAndMinusForUnderscore (currentEncoderSQLSet.ExerciseName); // check this
			}
		}

		//used for naming user-saved encoder analyze image
		if(sendAnalysis == "cross")
		{
			string temp = Util.ChangeChars(crossName, " / ", "-");
			temp = Util.ChangeChars(temp, ",", "-"); //needed for "(Speed,Power) - Load"
			encoderSendedAnalysis = temp;
		}
		else
			encoderSendedAnalysis = sendAnalysis;

		//triggers only on concentric
		if (triggerListEncoder == null || findEcconFromAnalyzeGui (false) != "c")
			triggerListEncoder = new TriggerList();

		encoderRProcAnalyze.SendData(
				titleStr, 
				currentPerson.Name, //used on singleFile
				encoderSelectedAnalysis == "neuromuscularProfile",
				preferences.RGraphsTranslate,
				(preferences.encoderCaptureCutByTriggers != Preferences.TriggerTypes.NO_TRIGGERS),
				triggerListEncoder,
				getAnalysisMode(),
				preferences.encoderInertialGraphsX
				);

		encoderRProcAnalyze.StartOrContinue(encoderStruct);
	}
		
	/*
	 * 1 neuromuscular should be separated
	 * 2 if we are analyzing current set and it's concentric separate phases button has to be unsensitive
	 * 3 single, side and superpose are together
	 */
	private void block_check_encoder_analyze_eccon_together_if_needed() 
	{
		if(radiobutton_encoder_analyze_neuromuscular_profile.Active) { // 1
			//separated, mandatory
			check_encoder_analyze_eccon_together.Sensitive = false;
			check_encoder_analyze_eccon_together.Active = false;
		}
		else if( 
				( radio_encoder_analyze_individual_current_set.Active && findEcconFromAnalyzeGui (false) == "c" ) || // 2
				( radiobutton_encoder_analyze_instantaneous.Active &&
				(radiobutton_encoder_analyze_single.Active ||
					radiobutton_encoder_analyze_side.Active ||
					radiobutton_encoder_analyze_superpose.Active) ) // 3
		  ) {
			//together, mandatory
			check_encoder_analyze_eccon_together.Sensitive = false;
			check_encoder_analyze_eccon_together.Active = true;
		}
	}

	private void on_radio_encoder_analyze_individual_current_set (object o, EventArgs args)
	{
		hbox_combo_encoder_laterality_analyze.Visible = false;

		//not called here
		//prepareAnalyzeRepetitions();
		
		createComboAnalyzeCross(false, false); //first creation: false, dateOnX: false
		createComboEncoderAnalyzeWeights(false); //first creation: false

		updateComboEncoderAnalyzeCurveNumFromCurrentSet ();

		button_encoder_analyze_data_select_curves.Visible = false;
		hbox_combo_encoder_exercise_analyze.Visible = false;
		
		//this analysis only when not comparing
		radiobutton_encoder_analyze_instantaneous.Visible = true;
		radiobutton_encoder_analyze_powerbars.Visible = true;
		radiobutton_encoder_analyze_1RM.Visible = (currentEncoderGI == Constants.EncoderGI.GRAVITATORY);
		radiobutton_encoder_analyze_single.Visible = true;
		radiobutton_encoder_analyze_side.Visible = true;
		radiobutton_encoder_analyze_superpose.Visible = true;
		radiobutton_encoder_analyze_all_set.Visible = true;

		radiobutton_encoder_analyze_neuromuscular_profile.Visible = (currentEncoderGI == Constants.EncoderGI.GRAVITATORY);

		check_encoder_analyze_eccon_together.Sensitive = true;
		block_check_encoder_analyze_eccon_together_if_needed();
		hbox_encoder_analyze_intersession.Visible = false;
		check_encoder_separate_session_in_days.Active = false;
		check_encoder_separate_session_in_days.Visible = false;

		button_encoder_monthly_change_current_session.Visible = false;

		button_encoder_analyze_sensitiveness();
	
		hbox_encoder_analyze_current_signal.Visible = true;

		showEncoderAnalyzeTriggersAndTab();
	}

	private void on_radio_encoder_analyze_individual_current_session (object o, EventArgs args)
	{
		updateEncoderAnalyzeExercisesPre ();
		hbox_combo_encoder_laterality_analyze.Visible = true;
		prepareAnalyzeRepetitions();

		/*
		if(currentPerson != null) {
			ArrayList data = SqliteEncoder.Select(
					false, -1, currentPerson.UniqueID, currentSession.UniqueID, -1,
					"curve", EncoderSQL.Eccons.ALL,
					false, true);
			int activeCurvesNum = UtilEncoder.GetActiveCurvesNum(data);
			updateComboEncoderAnalyzeCurveNumSavedReps(data, activeCurvesNum);	
		}
		*/

		createComboAnalyzeCross(false, false); //first creation: false, dateOnX: false
		
		button_encoder_analyze_data_select_curves.Visible = currentPerson != null;
		
		hbox_combo_encoder_exercise_analyze.Visible = true;

		//this analysis only when not comparing
		radiobutton_encoder_analyze_instantaneous.Visible = true;
		radiobutton_encoder_analyze_powerbars.Visible = true;
		radiobutton_encoder_analyze_1RM.Visible = (currentEncoderGI == Constants.EncoderGI.GRAVITATORY);
		radiobutton_encoder_analyze_single.Visible = true;
		radiobutton_encoder_analyze_side.Visible = true;
		radiobutton_encoder_analyze_superpose.Visible = true;

		//all_set only available on current signal mode
		if(radiobutton_encoder_analyze_all_set.Active)
			radiobutton_encoder_analyze_single.Active = true;
		radiobutton_encoder_analyze_all_set.Visible = false;

		radiobutton_encoder_analyze_neuromuscular_profile.Visible = (currentEncoderGI == Constants.EncoderGI.GRAVITATORY);

		check_encoder_analyze_eccon_together.Sensitive = true;
		block_check_encoder_analyze_eccon_together_if_needed();
		hbox_encoder_analyze_intersession.Visible = false;
		check_encoder_separate_session_in_days.Active = false;
		check_encoder_separate_session_in_days.Visible = false;

		button_encoder_monthly_change_current_session.Visible = configChronojump.CompujumpUserIsAdmin(currentPerson);

		button_encoder_analyze_sensitiveness();
	
		hbox_encoder_analyze_current_signal.Visible = false;

		showEncoderAnalyzeTriggerTab(false);
	}

	private void on_radio_encoder_analyze_individual_all_sessions (object o, EventArgs args)
	{
		updateEncoderAnalyzeExercisesPre ();
		hbox_combo_encoder_laterality_analyze.Visible = true;
		prepareAnalyzeRepetitions();
	
		hbox_encoder_analyze_current_signal.Visible = false;
		
		createComboAnalyzeCross(false, check_encoder_intersession_x_is_date.Active);
		combo_encoder_analyze_weights.Visible = check_encoder_intersession_x_is_date.Active;

		button_encoder_analyze_data_select_curves.Visible = currentPerson != null;
		
		hbox_combo_encoder_exercise_analyze.Visible = true;
		
		//active cross. The only available for comparing	
		radiobutton_encoder_analyze_cross.Active = true;
		hbox_encoder_analyze_intersession.Visible = true;
		
		set_check_encoder_separate_session_in_days();

		//this analysis only when not comparing
		radiobutton_encoder_analyze_instantaneous.Visible = false;
		radiobutton_encoder_analyze_powerbars.Visible = false;
		radiobutton_encoder_analyze_1RM.Visible = false;
		radiobutton_encoder_analyze_single.Visible = false;
		radiobutton_encoder_analyze_side.Visible = false;
		radiobutton_encoder_analyze_superpose.Visible = false;
		radiobutton_encoder_analyze_neuromuscular_profile.Visible = false;

		button_encoder_monthly_change_current_session.Visible = false;

		showEncoderAnalyzeTriggerTab(false);
	}

	private void on_radio_encoder_analyze_groupal_current_session (object o, EventArgs args)
	{
		updateEncoderAnalyzeExercisesPre ();

		hbox_combo_encoder_laterality_analyze.Visible = true;
		prepareAnalyzeRepetitions();

		hbox_encoder_analyze_current_signal.Visible = false;
		
		createComboAnalyzeCross(false, false); //first creation: false, dateOnX: false
		
		button_encoder_analyze_data_select_curves.Visible = currentPerson != null;
		
		hbox_combo_encoder_exercise_analyze.Visible = true;

		//active cross. The only available for comparing	
		radiobutton_encoder_analyze_cross.Active = true;
		hbox_encoder_analyze_intersession.Visible = false;
		check_encoder_separate_session_in_days.Active = false;
		check_encoder_separate_session_in_days.Visible = false;

		//this analysis only when not comparing
		radiobutton_encoder_analyze_instantaneous.Visible = false;
		radiobutton_encoder_analyze_powerbars.Visible = false;
		radiobutton_encoder_analyze_1RM.Visible = false;
		radiobutton_encoder_analyze_single.Visible = false;
		radiobutton_encoder_analyze_side.Visible = false;
		radiobutton_encoder_analyze_superpose.Visible = false;
		radiobutton_encoder_analyze_neuromuscular_profile.Visible = (currentEncoderGI == Constants.EncoderGI.GRAVITATORY);

		button_encoder_monthly_change_current_session.Visible = false;

		showEncoderAnalyzeTriggerTab(false);
	}


	private string getAnalysisVariables(string analysis)
	{
		string analysisVariables = "none"; //cannot be blank

		if(analysis == "powerBars") {
			if(check_encoder_analyze_show_impulse.Active)
				analysisVariables = "Impulse";
			else
				analysisVariables = "NoImpulse";

			if(check_encoder_analyze_show_time_to_peak_power.Active)
				analysisVariables += ";TimeToPeakPower";
			else
				analysisVariables += ";NoTimeToPeakPower";

			if(check_encoder_analyze_show_range.Active)
				analysisVariables += ";Range";
			else
				analysisVariables += ";NoRange";
		}
		else {  //analysis == "single" || analysis == "singleAllSet" ||
			//analysis == "side" || analysis == "sideShareX" || sendAnalysis == "superpose"
			if(check_encoder_analyze_show_position.Active)
				analysisVariables = "Position";
			else
				analysisVariables = "NoPosition";

			if(check_encoder_analyze_show_speed.Active)
				analysisVariables += ";Speed";
			else
				analysisVariables += ";NoSpeed";

			if(check_encoder_analyze_show_accel.Active)
				analysisVariables += ";Accel";
			else
				analysisVariables += ";NoAccel";

			if(check_encoder_analyze_show_force.Active)
				analysisVariables += ";Force";
			else
				analysisVariables += ";NoForce";

			if(check_encoder_analyze_show_power.Active)
				analysisVariables += ";Power";
			else
				analysisVariables += ";NoPower";
		}
		
		return analysisVariables;
	}

	private void set_check_encoder_separate_session_in_days()
	{
		bool neededConditions =
			! check_encoder_intersession_x_is_date.Active &&
			encoderSelectedAnalysis == "cross" &&
			UtilGtk.ComboGetActive(combo_encoder_analyze_cross) == "Pmax(F0,V0)";

		check_encoder_separate_session_in_days.Sensitive = neededConditions;

		if(! neededConditions)
			check_encoder_separate_session_in_days.Active = false;

		check_encoder_separate_session_in_days.Visible = neededConditions;
	}


	//encoder analysis modes

	private void on_radiobutton_encoder_analyze_powerbars_toggled (object obj, EventArgs args) {
		hbox_encoder_analyze_curve_num.Visible=false;
		hbox_combo_encoder_analyze_curve_num_combo.Visible = false;
		hbox_combo_encoder_analyze_cross_sup.Visible=false;
		hbox_combo_encoder_analyze_1RM.Visible=false;
		check_encoder_analyze_mean_or_max.Visible=false;
		hbox_encoder_analyze_show_powerbars.Visible=true;
		hbox_encoder_analyze_instantaneous.Visible=false;
		checkbutton_encoder_analyze_side_share_x.Visible = false;
		encoderSelectedAnalysis = "powerBars";
		notebook_encoder_analyze.CurrentPage = 0;
		
		check_encoder_analyze_eccon_together.Sensitive=true;
		block_check_encoder_analyze_eccon_together_if_needed();

		button_encoder_analyze_neuromuscular_help.Visible = false;
		label_encoder_analyze_side_max.Visible = false;

		//restore 1RM Bench Press sensitiveness
		check_encoder_analyze_mean_or_max.Sensitive = true;
		
		encoderButtonsSensitive(encoderSensEnumStored);
		button_encoder_analyze_sensitiveness();
	}
	
	private void on_radiobutton_encoder_analyze_cross_toggled (object obj, EventArgs args) {
		hbox_encoder_analyze_curve_num.Visible=false;
		hbox_combo_encoder_analyze_curve_num_combo.Visible = false;
		hbox_combo_encoder_analyze_cross_sup.Visible=true;
		hbox_combo_encoder_analyze_1RM.Visible=false;
		check_encoder_analyze_mean_or_max.Visible=true;
		hbox_encoder_analyze_show_powerbars.Visible=false;
		hbox_encoder_analyze_instantaneous.Visible=false;
		checkbutton_encoder_analyze_side_share_x.Visible = false;
		encoderSelectedAnalysis = "cross";
		notebook_encoder_analyze.CurrentPage = 0;
		
		check_encoder_analyze_eccon_together.Sensitive=true;
		
		//block_check_encoder_analyze_eccon_together_if_needed();
		//done here:
		on_combo_encoder_analyze_cross_changed (obj, args);

		button_encoder_analyze_neuromuscular_help.Visible = false;
		label_encoder_analyze_side_max.Visible = false;

		encoderButtonsSensitive(encoderSensEnumStored);
		button_encoder_analyze_sensitiveness();
	}
	
	private void on_radiobutton_encoder_analyze_1RM_toggled (object obj, EventArgs args) {
		hbox_encoder_analyze_curve_num.Visible=false;
		hbox_combo_encoder_analyze_curve_num_combo.Visible = false;
		hbox_combo_encoder_analyze_cross_sup.Visible=false;
		hbox_combo_encoder_analyze_1RM.Visible=true;
		check_encoder_analyze_mean_or_max.Visible=true;
		hbox_encoder_analyze_show_powerbars.Visible=false;
		hbox_encoder_analyze_instantaneous.Visible=false;
		checkbutton_encoder_analyze_side_share_x.Visible = false;
		encoderSelectedAnalysis = "1RM";
		notebook_encoder_analyze.CurrentPage = 0;
		
		check_encoder_analyze_eccon_together.Sensitive=true;
		
		//block_check_encoder_analyze_eccon_together_if_needed();
		//done here:
		on_combo_encoder_analyze_1RM_changed (obj, args);

		button_encoder_analyze_neuromuscular_help.Visible = false;
		label_encoder_analyze_side_max.Visible = false;

		encoderButtonsSensitive(encoderSensEnumStored);
		button_encoder_analyze_sensitiveness();
	}
	
	private void on_radiobutton_encoder_analyze_neuromuscular_profile_toggled (object obj, EventArgs args) {
		hbox_encoder_analyze_curve_num.Visible=false;
		hbox_combo_encoder_analyze_curve_num_combo.Visible = false;
		hbox_combo_encoder_analyze_cross_sup.Visible=false;
		hbox_combo_encoder_analyze_1RM.Visible=false;
		check_encoder_analyze_mean_or_max.Visible=false;
		hbox_encoder_analyze_show_powerbars.Visible=false;
		hbox_encoder_analyze_instantaneous.Visible=false;
		checkbutton_encoder_analyze_side_share_x.Visible = false;
		encoderSelectedAnalysis = "neuromuscularProfile";
		notebook_encoder_analyze.CurrentPage = 0;
		
		//separated, mandatory
		check_encoder_analyze_eccon_together.Sensitive=false;
		check_encoder_analyze_eccon_together.Active = false;
	
		button_encoder_analyze_neuromuscular_help.Visible = true;
		label_encoder_analyze_side_max.Visible = false;
		check_encoder_analyze_mean_or_max.Sensitive = false;

		encoderButtonsSensitive(encoderSensEnumStored);
		button_encoder_analyze_sensitiveness();
	}
	
	//end of encoder analysis modes

	//encoder analysis instantaneous options

	private void on_radiobutton_encoder_analyze_instantaneous_toggled (object obj, EventArgs args)
	{
		//hbox_encoder_analyze_curve_num.Visible=true; //defined in "4 radiobuttons"
		hbox_combo_encoder_analyze_curve_num_combo.Visible = true;
		hbox_combo_encoder_analyze_cross_sup.Visible=false;
		hbox_combo_encoder_analyze_1RM.Visible=false;
		check_encoder_analyze_mean_or_max.Visible=false;
		hbox_encoder_analyze_show_powerbars.Visible=false;
		hbox_encoder_analyze_instantaneous.Visible=true;
		//checkbutton_encoder_analyze_side_share_x.Visible = false; //defined in "4 radiobuttons"

		//together, mandatory
		check_encoder_analyze_eccon_together.Sensitive=false;
		check_encoder_analyze_eccon_together.Active = true;

		button_encoder_analyze_neuromuscular_help.Visible = false;
		label_encoder_analyze_side_max.Visible = false;

		//restore 1RM Bench Press sensitiveness
		check_encoder_analyze_mean_or_max.Sensitive = true;

		//all_set only available on current signal mode
		if(! radio_encoder_analyze_individual_current_set.Active && radiobutton_encoder_analyze_all_set.Active)
			radiobutton_encoder_analyze_single.Active = true;
		radiobutton_encoder_analyze_all_set.Visible = radio_encoder_analyze_individual_current_set.Active;

		//4 radiobuttons
		if(radiobutton_encoder_analyze_single.Active)
			encoder_instantaneous_gui("single");
		else if(radiobutton_encoder_analyze_side.Active)
			encoder_instantaneous_gui("side");
		else if(radiobutton_encoder_analyze_superpose.Active)
			encoder_instantaneous_gui("superpose");
		else if(radiobutton_encoder_analyze_all_set.Active)
			encoder_instantaneous_gui("singleAllSet");

		encoderButtonsSensitive(encoderSensEnumStored);
		button_encoder_analyze_sensitiveness();
	}

	private void on_button_encoder_analyze_mode_options_clicked (object o, EventArgs args)
	{
		encoderAnalyzeOptionsSensitivity(false);
	}
	private void on_button_encoder_analyze_mode_options_close_clicked (object o, EventArgs args)
	{
		encoderAnalyzeOptionsSensitivity(true);
	}
	private void on_button_encoder_analyze_mode_options_close_and_analyze_clicked (object o, EventArgs args)
	{
		encoderAnalyzeOptionsSensitivity(true);

		//timeout to let the software resize the window and graph in the correct size
		GLib.Timeout.Add(500, new GLib.TimeoutHandler(call_button_encoder_analyze));
	}
	private bool call_button_encoder_analyze()
	{
		on_button_encoder_analyze_clicked (new object (), new EventArgs ());
		return false;
	}

	private void encoderAnalyzeOptionsSensitivity(bool s) //s for sensitive. When show options frame is ! s
	{
		frame_encoder_analyze_options.Visible = ! s;

		hbox_encoder_analyze_individual_groupwise.Sensitive = s;
		grid_encoder_analyze_options.Sensitive = s;
		frame_persons.Sensitive = s;
		menus_and_mode_sensitive(s);
		hbox_contacts_sup_capture_analyze_two_buttons.Sensitive = s;
		hbox_top_person.Sensitive = s;
	}

	private void on_radiobutton_encoder_analyze_instantaneous_options_toggled (object o, EventArgs args)
	{
		hbox_encoder_analyze_curve_num.Visible = false;
		checkbutton_encoder_analyze_side_share_x.Visible = false;

		if(o == (object) radiobutton_encoder_analyze_single)
			encoder_instantaneous_gui("single");
		else if(o == (object) radiobutton_encoder_analyze_side)
			encoder_instantaneous_gui("side");
		else if(o == (object) radiobutton_encoder_analyze_superpose)
			encoder_instantaneous_gui("superpose");
		else if(o == (object) radiobutton_encoder_analyze_all_set)
			encoder_instantaneous_gui("singleAllSet");

		encoderButtonsSensitive(encoderSensEnumStored);
		button_encoder_analyze_sensitiveness();
	}

	private void encoder_instantaneous_gui (string mode)
	{
		if(mode == "single")
		{
			encoderSelectedAnalysis = "single";
			image_encoder_analyze_selected_single.Visible = (radiobutton_encoder_analyze_single.Active);
			label_encoder_analyze_selected.Text = Catalog.GetString("Single repetition");

			hbox_encoder_analyze_curve_num.Visible=true;

			notebook_encoder_analyze.CurrentPage = 1;
			vbox_encoder_analyze_instant.Visible = true;
		}
		else if (mode == "side")
		{
			if(checkbutton_encoder_analyze_side_share_x.Active)
				encoderSelectedAnalysis = "sideShareX";
			else
				encoderSelectedAnalysis = "side";

			image_encoder_analyze_selected_side.Visible = (radiobutton_encoder_analyze_side.Active);
			label_encoder_analyze_selected.Text = Catalog.GetString("Side compare");

			checkbutton_encoder_analyze_side_share_x.Visible = true;

			notebook_encoder_analyze.CurrentPage = 0;
			vbox_encoder_analyze_instant.Visible = false;
		}
		else if (mode == "superpose")
		{
			encoderSelectedAnalysis = "superpose";

			image_encoder_analyze_selected_superpose.Visible = (radiobutton_encoder_analyze_superpose.Active);
			label_encoder_analyze_selected.Text = Catalog.GetString("Superpose");

			notebook_encoder_analyze.CurrentPage = 0;
			vbox_encoder_analyze_instant.Visible = false;
		}
		else if (mode == "singleAllSet")
		{
			encoderSelectedAnalysis = "singleAllSet"; //TODO: define all this

			image_encoder_analyze_selected_all_set.Visible = (radiobutton_encoder_analyze_all_set.Active);
			label_encoder_analyze_selected.Text = Catalog.GetString("All set");

			notebook_encoder_analyze.CurrentPage = 1;
			vbox_encoder_analyze_instant.Visible = true;
		}
	}

	private void on_checkbutton_encoder_analyze_side_share_x_toggled (object o, EventArgs args)
	{
		if(checkbutton_encoder_analyze_side_share_x.Active)
			encoderSelectedAnalysis = "sideShareX";
		else
			encoderSelectedAnalysis = "side";
	}

	private void on_check_encoder_analyze_show_option_toggled (object o, EventArgs args)
	{
		if(o == (object) check_encoder_analyze_show_position)
			image_encoder_analyze_show_SAFE_position.Visible = (check_encoder_analyze_show_position.Active);
		else if(o == (object) check_encoder_analyze_show_speed)
			image_encoder_analyze_show_SAFE_speed.Visible = (check_encoder_analyze_show_speed.Active);
		else if(o == (object) check_encoder_analyze_show_accel)
			image_encoder_analyze_show_SAFE_accel.Visible = (check_encoder_analyze_show_accel.Active);
		else if(o == (object) check_encoder_analyze_show_force)
			image_encoder_analyze_show_SAFE_force.Visible = (check_encoder_analyze_show_force.Active);
		else if(o == (object) check_encoder_analyze_show_power)
			image_encoder_analyze_show_SAFE_power.Visible = (check_encoder_analyze_show_power.Active);
	}

	//end of encoder analysis instantaneous options

	private void on_check_encoder_analyze_eccon_together_toggled (object obj, EventArgs args) {
		image_encoder_analyze_eccon_together.Visible = check_encoder_analyze_eccon_together.Active;
		image_encoder_analyze_eccon_separated.Visible = ! check_encoder_analyze_eccon_together.Active;
	}
	
	private void on_check_encoder_analyze_mean_or_max_toggled (object obj, EventArgs args) {
		hbox_encoder_analyze_mean.Visible = check_encoder_analyze_mean_or_max.Active;
		hbox_encoder_analyze_max.Visible = ! check_encoder_analyze_mean_or_max.Active;
	}
	
	
	private void on_button_encoder_analyze_neuromuscular_help_clicked (object obj, EventArgs args) {
		//currently only active on neuromuscular profile

		string str = 
			Catalog.GetString("About Neuromuscular Profile") + "\n\n" +
			Catalog.GetString("Load = Average eccentric RFD (Ratio of Force Development)") + " (N/s)\n" +
			Catalog.GetString("Explode = Average relative concentric RFD") + " (N/s/kg)\n" +
			Catalog.GetString("Drive = Average relative concentric Impulse") + " (N*s/kg)\n\n" +
			Catalog.GetString("General trends to try to develop an 'equilibrated' neuromuscular profile (always add individual considerations as previous or actual injuries, sport specificity, muscular chains, etc.).") + "\n" +
			Catalog.GetString("If one of the metrics is under developed (weak) compared with the other two, prescribe exercises that emphasize its development.") + "\n" + 
			Catalog.GetString("If one of the metrics is over developed (extreme) compared with the other two, prescribe exercises to emphasize those, but paying attention to flexibility and relaxation of over working muscles.") + "\n\n" +

			Catalog.GetString("General guidelines to improve the neuromuscular profile:") + "\n" +
			
			Catalog.GetString("Load: Perform exercises that develop maximum force (eccentric, isometric or concentric).") + " " +
			Catalog.GetString("Then switch to exercises that develop eccentric force in short time.") + "\n" +

			Catalog.GetString("Explode: Perform exercises that develop force in short time, and maximum force.") + "\n" + 
			Catalog.GetString("Drive: Perform exercises where force is developed during more time.") + "\n\n" +

			"Perform 6 ABK jumps each one with 1 second rest." + "\n\n" + //TODO: translate
			Catalog.GetString("Analysis uses the best three jumps using 'jump height' criterion.") + "\n\n" +
			"Lapuente, M. De Blas. X." + "\n" +
			"Adapted from Wagner: Sparta Jump Scan 101: Load, Explode, and Drive\nhttps://spartascience.com/sparta-101-load-explode-and-drive/" + "\nhttps://spartascience.com/";
		
		new DialogMessage(Catalog.GetString("Neuromuscular profile"), Constants.MessageTypes.INFO, str, true);
	}


	//side compare works only in two modes (current_set and individual_current_session)
	private bool curvesNumOkToSideCompare() {
		if( (radio_encoder_analyze_individual_current_set.Active || radio_encoder_analyze_individual_current_session.Active)
				&& getActiveRepetitions() <= 12 )
			return true;

		return false;
	}

	private int getActiveRepetitions() 
	{
		if(radio_encoder_analyze_individual_current_set.Active) 
		{ 	//current set
			int rowsAtCapture = UtilGtk.CountRows(encoderCaptureListStore);
		
			if (ecconLast == "c")
				return rowsAtCapture;
			else {
				if(rowsAtCapture == 0)
					return 0;
				else
					return rowsAtCapture / 2;
			}
		} else if(radio_encoder_analyze_individual_current_session.Active)
		{
			return encSelReps.RepsActive;
		}
		return 0;
	}


	//BODY and EXTRA are at EncoderParams and sent to graph.R	
	private double findMassFromGui (Constants.MassType massType)
	{
		if(currentPersonSession == null)
			return 0;

		double extraWeight = spin_encoder_extra_weight.Value;
		if (current_mode == Constants.Modes.POWERINERTIAL)
			extraWeight = 0;

		if(massType == Constants.MassType.BODY)
			return currentPersonSession.Weight;
		else if(massType == Constants.MassType.EXTRA)
			return extraWeight;
		else //(massType == Constants.MassType.DISPLACED)
			return extraWeight + 
				( currentPersonSession.Weight * getExercisePercentBodyWeightFromComboCapture() ) / 100.0;
	}

	private double findDisplacedMassFromSQL ()
	{
		return currentEncoderSQLSet.extraWeightD + (
					getExercisePercentBodyWeightFromID (currentEncoderSQLSet.exerciseID) *
					currentPersonSession.Weight
					);
	}

	/* unused right now
	//this is used in 1RM return to substract the weight of the body (if used on exercise)
	private double massWithoutPerson(double massTotal, string exerciseName) {
		int percentBodyWeight = getExercisePercentBodyWeightFromName(exerciseName);
		if(currentPersonSession.Weight == 0 || percentBodyWeight == 0 || percentBodyWeight == -1)
			return massTotal;
		else
			return massTotal - (currentPersonSession.Weight * percentBodyWeight / 100.0);
	}
	*/

	// ---- findEccon ---->

	// only used on capture
	private string findEcconFromCaptureGui (bool forceEcconSeparated)
	{
		/*
		LogB.Information ("called findEcconFromGui from method: ");
		StackTrace stackTrace = new StackTrace();
		LogB.Information ((stackTrace.GetFrame(1).GetMethod().Name));
		*/

		if (radio_encoder_eccon_concentric.Active)
			return "c";
		else {
			if (forceEcconSeparated)
				return "ecS";
			else
				return "ec";
		}
	}
	private string findEcconFromAnalyzeGui (bool forceEcconSeparated)
	{
		// 1st check eccon of current set:
		string eccon = findEcconFromCurrentSet (forceEcconSeparated);

		if (eccon == "c")
			return eccon;

		// if !c then decide with: forceEcconSeparated & check_encoder_analyze_eccon_together
		if (forceEcconSeparated || ! check_encoder_analyze_eccon_together.Active)
			return "ecS";
		else
			return "ec";
	}

	private string findEcconFromCurrentSet (bool forceEcconSeparated)
	{
		// just to be safe
		if (currentEncoderSQLSet == null)
			return "c";

		if (forceEcconSeparated && currentEncoderSQLSet.eccon == "ec")
			return "ecS";

		return currentEncoderSQLSet.eccon;
	}

	// <---- findEccon ----


	private int getEncoderMinHeightOnGuiCapture ()
	{
		if (current_mode == Constants.Modes.POWERGRAVITATORY)
			return Convert.ToInt32 (spin_encoder_capture_min_height_gravitatory.Value);
		else // if (current_mode == Constants.Modes.POWERINERTIAL)
			return Convert.ToInt32 (spin_encoder_capture_min_height_inertial.Value);
	}

	/* encoder exercise stuff */
	
	
	string [] encoderExercisesTranslationAndBodyPWeight;
	//string [] encoderCaptureCurvesSaveOptionsTranslation;
//	string [] encoderEcconTranslation;
//	string [] encoderLateralityTranslation;
	string [] encoderAnalyzeCrossTranslation;
	string [] encoderAnalyze1RMTranslation;

	Button button_combo_encoder_exercise_capture_left;
	Button button_combo_encoder_exercise_capture_right;

	// called by initEncoder (just one time)
	protected void createEncoderCombos() 
	{
		//create combo exercises
		combo_encoder_exercise_capture = new ComboBoxText ();
		combo_encoder_exercise_analyze = new ComboBoxText ();
		
		createEncoderComboExerciseAndAnalyze();
		
		combo_encoder_exercise_capture.Changed += new EventHandler (on_combo_encoder_exercise_capture_changed);
		combo_encoder_exercise_analyze.Changed += new EventHandler (on_combo_encoder_exercise_analyze_changed);

		/* ConcentricEccentric
		 * unavailable until find while concentric data on concentric is the same than in ecc-con,
		 * but is very different than in con-ecc
		 */

		//create combo encoder anchorage
		combo_encoder_anchorage = new ComboBoxText();
		combo_encoder_anchorage.Changed += 
			new EventHandler(on_combo_encoder_anchorage_changed );

		//create combo analyze cross
		createComboAnalyzeCross(true, false);	//first creation, without "dateOnX"
		createComboEncoderAnalyzeWeights(true);	//first creation

		//create combo analyze 1RM
		string [] comboAnalyze1RMOptions = { "1RM Any exercise", "1RM Bench Press", "1RM Squat", "1RM Indirect" };
		string [] comboAnalyze1RMOptionsTranslated = { 
			Catalog.GetString("1RM Any exercise"), Catalog.GetString("1RM Bench Press"),
			Catalog.GetString("1RM Squat"), Catalog.GetString("1RM Indirect")
		}; //if added more, change the int in the 'for' below
		encoderAnalyze1RMTranslation = new String [comboAnalyze1RMOptions.Length];
		for(int j=0; j < 4 ; j++)
			encoderAnalyze1RMTranslation[j] = 
				comboAnalyze1RMOptions[j] + ":" + comboAnalyze1RMOptionsTranslated[j];
		combo_encoder_analyze_1RM = new ComboBoxText ();
		UtilGtk.ComboUpdate(combo_encoder_analyze_1RM, comboAnalyze1RMOptionsTranslated, "");
		combo_encoder_analyze_1RM.Active = UtilGtk.ComboMakeActive(combo_encoder_analyze_1RM, 
				Catalog.GetString(comboAnalyze1RMOptions[0]));
		combo_encoder_analyze_1RM.Changed += new EventHandler (on_combo_encoder_analyze_1RM_changed);


		//create combo analyze curve num combo
		//is not an spinbutton because values can be separated: "3,4,7,21"
		combo_encoder_analyze_curve_num_combo = new ComboBoxText ();
		UtilGtk.ComboUpdate(combo_encoder_analyze_curve_num_combo, Util.StringToStringArray(""), "");


		//pack combos
		button_combo_encoder_exercise_capture_left = UtilGtk.CreateArrowButton (40, 40, UtilGtk.ArrowEnum.LEFT);
		button_combo_encoder_exercise_capture_left.Sensitive = false;
		button_combo_encoder_exercise_capture_left.Clicked += on_button_encoder_exercise_capture_left_clicked;
		hbox_combo_encoder_exercise_capture.PackStart(button_combo_encoder_exercise_capture_left, true, true, 0);

		hbox_combo_encoder_exercise_capture.PackStart(combo_encoder_exercise_capture, true, true, 10);

		button_combo_encoder_exercise_capture_right = UtilGtk.CreateArrowButton (40, 40, UtilGtk.ArrowEnum.RIGHT);
		button_combo_encoder_exercise_capture_right.Sensitive = true;
		button_combo_encoder_exercise_capture_right.Clicked += on_button_encoder_exercise_capture_right_clicked;
		hbox_combo_encoder_exercise_capture.PackStart(button_combo_encoder_exercise_capture_right, true, true, 0);

		hbox_combo_encoder_exercise_capture.ShowAll();
		combo_encoder_exercise_capture.Sensitive = true;
		
		hbox_combo_encoder_exercise_analyze.PackStart(combo_encoder_exercise_analyze, true, true, 0);
		//hbox_combo_encoder_exercise_analyze.ShowAll(); //hbox will be shown only on intersession & interperson
		combo_encoder_exercise_analyze.ShowAll();
		combo_encoder_exercise_analyze.Sensitive = true;

		hbox_combo_encoder_anchorage.PackStart(combo_encoder_anchorage, false, true, 0);
		hbox_combo_encoder_anchorage.ShowAll();

		//restriction for configured Compujump clients
		if(configChronojump.Compujump)
			combo_encoder_anchorage.Sensitive = false;
		else
			combo_encoder_anchorage.Sensitive = true;

		hbox_combo_encoder_analyze_1RM.PackStart(combo_encoder_analyze_1RM, true, true, 0);
		hbox_combo_encoder_analyze_1RM.ShowAll(); 
		combo_encoder_analyze_1RM.Sensitive = true;
		hbox_combo_encoder_analyze_1RM.Visible = false; //do not show hbox at start
	
		hbox_combo_encoder_analyze_curve_num_combo.PackStart(combo_encoder_analyze_curve_num_combo, true, true, 0);
		hbox_combo_encoder_analyze_curve_num_combo.ShowAll(); 
		combo_encoder_analyze_curve_num_combo.Sensitive = true;
		hbox_combo_encoder_analyze_curve_num_combo.Visible = false; //do not show hbox at start

		label_encoder_top_exercise.Text = UtilGtk.ComboGetActive(combo_encoder_exercise_capture);
		setEcconPixbuf();
		setLateralityPixbuf();

		label_encoder_top_extra_mass.Text = spin_encoder_extra_weight.Value + " kg";

		if(label_encoder_1RM_percent.Text == "")
			label_encoder_top_1RM_percent.Text = "";
		else
			label_encoder_top_1RM_percent.Text = label_encoder_1RM_percent.Text + " %1RM";

		label_encoder_top_weights.Text = spin_encoder_im_weights_n.Value.ToString ();
		label_encoder_top_im.Text = Catalog.GetString("Inertia M.") + ": " + label_encoder_im_total.Text;


		//combo_encoder_laterality_analyze
		//string [] comboEncoderLateralityAnalyzeOptions = { "Any laterality", "Both", "Left", "Right" };
		string [] comboEncoderLateralityAnalyzeTranslated = {
			Catalog.GetString("Any laterality"), Catalog.GetString("Both"),
			Catalog.GetString("Left"), Catalog.GetString("Right")
		};

		combo_encoder_laterality_analyze = new ComboBoxText ();
		UtilGtk.ComboUpdate(combo_encoder_laterality_analyze, comboEncoderLateralityAnalyzeTranslated, "");
		combo_encoder_laterality_analyze.Active = 0;
		combo_encoder_laterality_analyze.Visible = false; //because we start on current set radio
		combo_encoder_laterality_analyze.Changed += new EventHandler (on_combo_encoder_laterality_analyze_changed);

		hbox_combo_encoder_laterality_analyze.PackStart(combo_encoder_laterality_analyze, true, true, 0);
		hbox_combo_encoder_laterality_analyze.ShowAll();
		combo_encoder_laterality_analyze.Sensitive = true;
		hbox_combo_encoder_laterality_analyze.Visible = false; //do not show hbox at start
	}

	private void on_button_encoder_exercise_capture_left_clicked(object o, EventArgs args)
	{
		combo_encoder_exercise_capture = UtilGtk.ComboSelectPrevious(combo_encoder_exercise_capture);

		button_combo_encoder_exercise_capture_left.Sensitive = (combo_encoder_exercise_capture.Active > 0);
		button_combo_encoder_exercise_capture_right.Sensitive = true;
	}
	private void on_button_encoder_exercise_capture_right_clicked(object o, EventArgs args)
	{
		bool isLast;
		combo_encoder_exercise_capture = UtilGtk.ComboSelectNext(combo_encoder_exercise_capture, out isLast);

		button_combo_encoder_exercise_capture_left.Sensitive = true;
		button_combo_encoder_exercise_capture_right.Sensitive = ! isLast;
	}

	//this is called also when an exercise is deleted to update the combo and the string []
	//and on change mode POWERGRAVITORY <-> POWERINERTIAL, because encoderExercises can have different type (encoderGI)
	private void createEncoderComboExerciseAndAnalyze()
	{
		// 1) selecte encoderExercises on SQL
		List<EncoderExercise> encoderExercise_l = SqliteEncoderExercise.SelectEncoderExercises (
				false, -1, false, Constants.GetEncoderGIByMode (current_mode));
		// 2) if ! encoderExcises, delete both combos and return
		if (encoderExercise_l.Count == 0)
		{
			encoderExercisesTranslationAndBodyPWeight = new String [0];

			//maybe there are no exercises because last one has been deleted, then combo has to be updated and be empty
			UtilGtk.ComboDelAll(combo_encoder_exercise_capture);
			UtilGtk.ComboDelAll(combo_encoder_exercise_analyze);

			return;
		}

		// 3) define: encoderExercisesTranslationAndBodyPWeight and exerciseNamesToCombo
		button_encoder_exercise_edit.Sensitive = true;
		button_encoder_exercise_delete.Sensitive = true;

		encoderExercisesTranslationAndBodyPWeight = new String [encoderExercise_l.Count];
		string [] exerciseNamesToCombo = new String [encoderExercise_l.Count];
		int i =0;
		foreach (EncoderExercise ex in encoderExercise_l) {
			string nameTranslated = Catalog.GetString(ex.name);
			encoderExercisesTranslationAndBodyPWeight[i] = 
				ex.uniqueID + ":" + ex.name + ":" + nameTranslated + ":" + ex.percentBodyWeight;
			exerciseNamesToCombo[i] = Catalog.GetString(ex.name);
			i++;
		}

		// 4) update combo_encoder_exercise_capture and set active
		string previousExerciseCapture = UtilGtk.ComboGetActive(combo_encoder_exercise_capture);

		UtilGtk.ComboUpdate(combo_encoder_exercise_capture, exerciseNamesToCombo, "");
		if(previousExerciseCapture == "")
			combo_encoder_exercise_capture.Active = UtilGtk.ComboMakeActive (combo_encoder_exercise_capture,
					Catalog.GetString (encoderExercise_l[0].name));
		else
			combo_encoder_exercise_capture.Active = UtilGtk.ComboMakeActive(combo_encoder_exercise_capture,
					previousExerciseCapture);

		// 5) update combo_encoder_exercise_analyze and set active
		exerciseNamesToCombo = addAllExercisesToComboExerciseAnalyze(exerciseNamesToCombo);

		/*
		 * combo update mark as NoFollow to not call his _changed method at any changes.
		 * This change speeds analyze click on groupal from 21s to 1s on sessions like blq 2023 matins
		 * Long time is caused by exerciseNamesToCombo (encoder exercises on DB)
		 */
		//LogB.Information ("exerciseNamesToCombo: " + Util.StringArrayToString (exerciseNamesToCombo, ";"));
		comboEncoderNoFollow = true;
		UtilGtk.ComboUpdate(combo_encoder_exercise_analyze, exerciseNamesToCombo, "");
		comboEncoderNoFollow = false;
		on_combo_encoder_exercise_analyze_changed (new object (), new EventArgs ());

		combo_encoder_exercise_analyze.Active = 0; //first one active "All exercises"
	}
	private string [] addAllExercisesToComboExerciseAnalyze(string [] exerciseNamesToCombo) {

		exerciseNamesToCombo = Util.AddArrayString(exerciseNamesToCombo, Catalog.GetString("All exercises"), true); //first
		encoderExercisesTranslationAndBodyPWeight = Util.AddArrayString(
				encoderExercisesTranslationAndBodyPWeight, 
				-1 + ":" + "All exercises" + ":" + Catalog.GetString("All exercises") + ":" + 0, true); //first

		return(exerciseNamesToCombo);
	}
		
	private void createComboAnalyzeCross(bool firstCreation, bool dateOnX) 
	{
		string lastActive = "";
		if(combo_encoder_analyze_cross != null)
			lastActive = UtilGtk.ComboGetActive(combo_encoder_analyze_cross);

		string [] comboAnalyzeCrossOptions;
		string [] comboAnalyzeCrossOptionsTranslated;
	
		if(! dateOnX) {
			//create combo analyze cross (variables)
			comboAnalyzeCrossOptions = new string [] { 
				"Power - Load", "Speed - Load", "Force - Load",
					"Pmax(F0,V0)",
					"(Speed,Power) - Load",
					"(Force,Power) - Speed",
					"Load - Speed",
					"Power - Speed"
			};
			comboAnalyzeCrossOptionsTranslated = new string [] { 
				Catalog.GetString ("Power - Load"),
				Catalog.GetString ("Speed - Load"),
				Catalog.GetString ("Force - Load"),
				"Pmax(F0,V0)", //will not be translated
				Catalog.GetString ("(Speed,Power) - Load"),
				Catalog.GetString ("(Force,Power) - Speed"),
				Catalog.GetString ("Load - Speed"),
				Catalog.GetString ("Power - Speed")
			}; //if added more, change the int in the 'for' below
			encoderAnalyzeCrossTranslation = new String [comboAnalyzeCrossOptions.Length];
			for(int j=0; j < 8 ; j++)
				encoderAnalyzeCrossTranslation[j] = 
					comboAnalyzeCrossOptions[j] + ":" + comboAnalyzeCrossOptionsTranslated[j];
		} else {
			//create combo analyze cross (variables)
			comboAnalyzeCrossOptions = new string [] { "Power - Date", "Speed - Date", "Force - Date" };
			comboAnalyzeCrossOptionsTranslated = new string [] { 
				Catalog.GetString ("Power - Date"),
				Catalog.GetString ("Speed - Date"),
				Catalog.GetString ("Force - Date")
			}; //if added more, change the int in the 'for' below
			encoderAnalyzeCrossTranslation = new String [comboAnalyzeCrossOptions.Length];
			for(int j=0; j < 3 ; j++)
				encoderAnalyzeCrossTranslation[j] = 
					comboAnalyzeCrossOptions[j] + ":" + comboAnalyzeCrossOptionsTranslated[j];
		}

		if(firstCreation)
			combo_encoder_analyze_cross = new ComboBoxText ();

		UtilGtk.ComboUpdate(combo_encoder_analyze_cross, comboAnalyzeCrossOptionsTranslated, "");
		combo_encoder_analyze_cross.Active = UtilGtk.ComboMakeActive(combo_encoder_analyze_cross, 
				lastActive);

		if(firstCreation) {
			combo_encoder_analyze_cross.Changed += new EventHandler (on_combo_encoder_analyze_cross_changed);

			hbox_combo_encoder_analyze_cross.PackStart(combo_encoder_analyze_cross, true, true, 0);
			hbox_combo_encoder_analyze_cross.ShowAll(); 
			combo_encoder_analyze_cross.Sensitive = true;
			hbox_combo_encoder_analyze_cross_sup.Visible = false; //do not show hbox at start
		}
	}
		
	private void createComboEncoderAnalyzeWeights(bool firstCreation) 
	{
		if(firstCreation)
			combo_encoder_analyze_weights = new ComboBoxText ();
	
		string lastActive = UtilGtk.ComboGetActive(combo_encoder_analyze_weights);

		if(encSelReps.EncoderInterSessionDateOnXWeights != null &&
			encSelReps.EncoderInterSessionDateOnXWeights.Count > 0) {
			UtilGtk.ComboUpdate(combo_encoder_analyze_weights, encSelReps.GetEncoderInterSessionDateOnXWeightsForCombo());
			combo_encoder_analyze_weights.Active = UtilGtk.ComboMakeActive(combo_encoder_analyze_weights, lastActive);
		}

		if(firstCreation) {
			hbox_combo_encoder_analyze_weights.PackStart(combo_encoder_analyze_weights, true, true, 0);
			hbox_combo_encoder_analyze_weights.ShowAll(); 
		}
	}

	//to avoid circular calls
	private bool encoder_x_is_date_session_in_days_nofollow = false;

	void on_check_encoder_intersession_x_is_date_toggled (object o, EventArgs args)
	{
		if(encoder_x_is_date_session_in_days_nofollow)
			return;

		createComboAnalyzeCross(false, check_encoder_intersession_x_is_date.Active);
		
		if(check_encoder_intersession_x_is_date.Active) {
			createComboEncoderAnalyzeWeights(false);
			combo_encoder_analyze_weights.Visible = true;
		} else
			combo_encoder_analyze_weights.Visible = false;

		encoder_x_is_date_session_in_days_nofollow = true;
		set_check_encoder_separate_session_in_days();
		encoder_x_is_date_session_in_days_nofollow = false;
	}

	void on_check_encoder_separate_session_in_days_toggled (object o, EventArgs args)
	{
		if(encoder_x_is_date_session_in_days_nofollow)
			return;

		check_encoder_intersession_x_is_date.Sensitive = ! check_encoder_separate_session_in_days.Active;

		if(check_encoder_separate_session_in_days.Active)
		{
			encoder_x_is_date_session_in_days_nofollow = true;
			check_encoder_intersession_x_is_date.Active = false;
			encoder_x_is_date_session_in_days_nofollow = false;
		}
	}



	void on_radio_encoder_eccon_toggled (object o, EventArgs args)
	{
		//those will be true again when loading a new encoder test or capturing
		treeview_encoder_capture_curves.Sensitive = false;

		check_encoder_analyze_eccon_together.Sensitive = true;
		block_check_encoder_analyze_eccon_together_if_needed();
		setEcconPixbuf();
	}

	void setEcconPixbuf()
	{
		Pixbuf pixbuf;
		if(radio_encoder_eccon_concentric.Active)
			pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "muscle-concentric.png");
		else
			pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "muscle-excentric-concentric.png");

		image_top_eccon.Pixbuf = pixbuf;
	}

	void on_radio_encoder_laterality_toggled (object o, EventArgs args)
	{
		setLateralityPixbuf();
	}

	void setLateralityPixbuf()
	{
		Pixbuf pixbuf;
		if(radio_encoder_laterality_r.Active)
			pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "laterality-right.png");
		else if(radio_encoder_laterality_l.Active)
			pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "laterality-left.png");
		else
			pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "laterality-both.png");

		image_top_laterality.Pixbuf = pixbuf;
	}

	private string getLateralityFromGui (bool english)
	{
		string laterality = "RL";
		if(radio_encoder_laterality_r.Active)
			laterality = "R";
		else if(radio_encoder_laterality_l.Active)
			laterality = "L";

		if(! english)
			laterality = Catalog.GetString(laterality);

		LogB.Information("Laterality: " + laterality);
		return laterality;
	}

	void on_combo_encoder_analyze_cross_changed (object o, EventArgs args)
	{
		if (! radiobutton_encoder_analyze_cross.Active)
			return;

		block_check_encoder_analyze_eccon_together_if_needed();

		//Pmax(F0,V0) is not translated
		if(UtilGtk.ComboGetActive(combo_encoder_analyze_cross) == "Pmax(F0,V0)")
		{
			check_encoder_intersession_x_is_date.Active = false;
			check_encoder_intersession_x_is_date.Sensitive = false;

			//eccon has to be ecS (separated), and R will use only "c"
			check_encoder_analyze_eccon_together.Active = false;
			check_encoder_analyze_eccon_together.Sensitive = false;

			check_encoder_analyze_mean_or_max.Active = true;
			check_encoder_analyze_mean_or_max.Sensitive = false;
		} else {
			check_encoder_intersession_x_is_date.Sensitive = true;
			check_encoder_analyze_eccon_together.Sensitive = true;
			check_encoder_analyze_mean_or_max.Sensitive = true;
		}

		set_check_encoder_separate_session_in_days();

		button_encoder_analyze_sensitiveness();
	}
	
	void on_combo_encoder_analyze_1RM_changed (object o, EventArgs args)
	{
		if (! radiobutton_encoder_analyze_1RM.Active)
			return;

		check_encoder_analyze_mean_or_max.Active = true;
		check_encoder_analyze_mean_or_max.Sensitive = false;
		check_encoder_analyze_eccon_together.Active = false;
		check_encoder_analyze_eccon_together.Sensitive = false;
			
		//1RM Indirect can only be used with current signal	
		if(Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_analyze_1RM),
					encoderAnalyze1RMTranslation) == "1RM Indirect" &&
				! radio_encoder_analyze_individual_current_set.Active) {	//not current set
			button_encoder_analyze.Sensitive = false;
			new DialogMessage(Constants.MessageTypes.WARNING, 
					"1RM Indirect prediction can only be done with current set.");
		}
	
		button_encoder_analyze_sensitiveness();
	}

	void on_button_encoder_capture_image_save_clicked (object o, EventArgs args)
	{
		checkFile(Constants.CheckFileOp.ENCODER_CAPTURE_SAVE_IMAGE);
	}

	private string screenshotURL = "";
	private bool screenshotPending;

	void on_button_encoder_capture_save_image_file_selected (string destination)
	{
		try {
			if(encoder_capture_curves_bars_drawingarea_cairo == null)
				return;

			LogB.Information("Saving");
			//screenshot will be done on _draw (gtk3 way)
			//CairoUtil.GetScreenshotFromDrawingArea (encoder_capture_curves_bars_drawingarea_cairo, destination);

			screenshotURL = destination;
			screenshotPending = true;
			if (notebook_start.CurrentPage == Convert.ToInt32 (notebook_start_pages.FULLSCREENCAPTURE))
				fullscreen_capture_drawingarea_cairo.QueueDraw ();
			else
				encoder_capture_curves_bars_drawingarea_cairo.QueueDraw ();

		} catch {
			string myString = string.Format(
					Catalog.GetString("Cannot save file {0} "), destination);
			new DialogMessage(Constants.MessageTypes.WARNING, myString);
		}
	}
	void on_button_encoder_analyze_image_save_clicked (object o, EventArgs args)
	{
		/* file is in:
		 * /tmp/chronojump-last-encoder-graph.png
		 * but if a capture curves has done, file is named the same
		 * make unsensitive the capture image after loading or capturing a new signal
		 * or changing person, loading session, …
		 */

		if(radio_encoder_analyze_groupal_current_session.Active)
			checkFile(Constants.CheckFileOp.ENCODER_ANALYZE_SAVE_IMAGE_CURRENT_SESSION);
		else
			checkFile(Constants.CheckFileOp.ENCODER_ANALYZE_SAVE_IMAGE);
	}
	void on_button_encoder_analyze_save_image_file_selected (string destination)
	{
		try {
			File.Copy(UtilEncoder.GetEncoderGraphTempFileName(), destination, true);
		} catch {
			string myString = string.Format(
					Catalog.GetString("Cannot save file {0} "), destination);
			new DialogMessage(Constants.MessageTypes.WARNING, myString);
		}
	}

	void on_button_encoder_analyze_image_compujump_send_email_clicked (object o, EventArgs args)
	{
		if(configChronojump.CompujumpUserIsAdmin(currentPerson))
		{
			checkFile(Constants.CheckFileOp.ENCODER_ANALYZE_SEND_IMAGE);
			compujumpSendEmail(Constants.CheckFileOp.ENCODER_ANALYZE_SEND_IMAGE);
		} else
		{
			LogB.Information("rfidWaitingAdminGuiObjects is null: " + (rfidWaitingAdminGuiObjects == null).ToString());
			if(rfidWaitingAdminGuiObjects != null)
			{
				rfid.WaitingAdminStart(SqlitePerson.SelectAttribute(configChronojump.CompujumpAdminID, "future1")); //select rfid
				rfidWaitingAdminGuiObjects.Start();
			}
		}
	}

	void on_button_encoder_analyze_table_save_clicked (object o, EventArgs args)
	{
		/* file is in:
		 * /tmp/chronojump-last-encoder-curves.txt
		 * but if a capture curves has done, file is named the same
		 * make unsensitive the capture table after loading or capturing a new signal
		 * or changing person, loading session, …
		 * No problem. Is nice to play with seinsitiveness, but the reading will be from treeview and not from file
		 */

		if(radio_encoder_analyze_groupal_current_session.Active)
			checkFile(Constants.CheckFileOp.ENCODER_ANALYZE_SAVE_TABLE_CURRENT_SESSION);
		else
			checkFile(Constants.CheckFileOp.ENCODER_ANALYZE_SAVE_TABLE);
	}

	/*
	   if gui then can open a DialogMessage
	   if !gui is eg when sending an email, a dialogMessage will not be opened here
	   */
	private bool on_button_encoder_save_table_file_selected (string destination, bool gui)
	{
		try {
			//this overwrites if needed
			TextWriter writer = File.CreateText(destination);

			string sep = " ";
			if (preferences.CSVExportDecimalSeparator == "COMMA")
				sep = ";";
			else
				sep = ",";

			if(lastTreeviewEncoderAnalyzeIsNeuromuscular) {
				//write header
				writer.WriteLine(Util.RemoveNewLine(Util.StringArrayToString(
							treeviewEncoderAnalyzeNeuromuscularHeaders, sep), false));
				//write curves rows
				ArrayList array = getTreeViewNeuromuscular(encoderAnalyzeListStore);
				foreach (EncoderNeuromuscularData nm in array)
					writer.WriteLine(nm.ToCSV(preferences.CSVExportDecimalSeparator));
			} else {
				//write header
				writer.WriteLine(Util.RemoveNewLine(Util.StringArrayToString(
							GetTreeviewEncoderAnalyzeHeaders(false, current_mode), sep), false));
				//write curves rows
				ArrayList array = getTreeViewCurves(encoderAnalyzeListStore);

				foreach (EncoderCurve ec in array)
				{
					string phase = "";
					if(radio_encoder_analyze_individual_current_set.Active && findEcconFromAnalyzeGui (false) == "ecS" && ec.IsNumberN())
					{
						phase = "e";
						if(Util.IsEven(Convert.ToInt32(ec.N)))
							phase = "c";
					}

					writer.WriteLine(ec.ToCSV(false, current_mode, preferences.CSVExportDecimalSeparator, preferences.encoderWorkKcal, phase));
				}
			}
			
			writer.Flush();
			writer.Close();
			((IDisposable)writer).Dispose();
		} catch {
			string myString = string.Format(
					Catalog.GetString("Cannot save file {0} "), destination);
			if(gui)
				new DialogMessage(Constants.MessageTypes.WARNING, myString);

			return false;
		}

		return true;
	}

	void on_button_encoder_analyze_1RM_save_clicked (object o, EventArgs args)
	{
		string contents = Util.ReadFile(UtilEncoder.GetEncoderSpecialDataTempFileName(), true);
		//if 1RM button is sensitive and there's no 1RM data 
		//(because a not-1RM test have been done and software has a sensitivity bug), return
		if(contents == null || contents == "") {
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Not enough data."));
			return;
		}

		string [] load1RMStr = contents.Split(new char[] {';'});
		string load1RMtemp = Util.ChangeDecimalSeparator(load1RMStr[1]);
		
		//check if it's a number
		if(! Util.IsNumber(load1RMtemp, true)) {
			new DialogMessage(Constants.MessageTypes.WARNING, 
					Catalog.GetString("Error doing operation.") + "\n" +
					Catalog.GetString("Operation cancelled."));
			return;
		}

		double load1RM = Convert.ToDouble(load1RMtemp);

		if(load1RM == -1) {
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Not enough data."));
			return;
		}
		//save it without the body weight
		//string exerciseName = "";
		int exerciseID = 0;
		string myString = "";

		if(Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_analyze_1RM),
					encoderAnalyze1RMTranslation) == "1RM Indirect") 
		{
			new DialogMessage(Constants.MessageTypes.WARNING, "Currently disabled");
			return;

			/*
			exerciseName = UtilGtk.ComboGetActive(combo_encoder_exercise_capture);
			exerciseID = getExerciseIDFromEncoderCombo (exerciseCombos.CAPTURE);
			*/

			/*
			 * on 1RM indirect, right now the returned data is person weight + extra weight
			 * try to give all the info to the user
			 * in close future, this will come as extra weight from R
			 */
			/*
			double load1RMWithoutPerson = massWithoutPerson(load1RM, exerciseName);

			SqliteEncoder.Insert1RM(false, currentPerson.UniqueID, currentSession.UniqueID,
					exerciseID, load1RMWithoutPerson);

			TODO: change this and return the extra mass on 1RM indirect
			Also note it was not working because getExercisePercentBodyWeightFromTable reads analyze table that is empty on 1RMIndirect

			if(load1RM != load1RMWithoutPerson)
				myString = string.Format(Catalog.GetString("1RM found: {0} kg."), load1RM) + "\n" +
					string.Format(Catalog.GetString("Displaced body weight in this exercise: {0}%."),
							getExercisePercentBodyWeightFromTable()) + "\n" +
					string.Format(Catalog.GetString("Saved 1RM without displaced body weight: {0} kg."),
							load1RMWithoutPerson);
			*/
		}
		else {
			exerciseID = getExerciseIDFromEncoderTable();

			SqliteEncoder1RM.Insert1RM(false, currentPerson.UniqueID, currentSession.UniqueID,
					exerciseID, load1RM);

			myString = string.Format(Catalog.GetString("Saved 1RM: {0} kg."), load1RM);
		}

		array1RMUpdate(false);
		encoder_change_displaced_weight_and_1RM ();
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}


	enum exerciseCombos { CAPTURE, ANALYZE }
	
	int getExerciseIDFromEncoderCombo (exerciseCombos combo) {
		if(combo == exerciseCombos.CAPTURE)
			//return getExerciseIDFromName (UtilGtk.ComboGetActive(combo_encoder_exercise_capture));
			return getExerciseIDFromAnyCombo (
					combo_encoder_exercise_capture,
					encoderExercisesTranslationAndBodyPWeight, true);
		else
			//return getExerciseIDFromName (UtilGtk.ComboGetActive(combo_encoder_exercise_analyze));
			return getExerciseIDFromAnyCombo (
					combo_encoder_exercise_analyze,
					encoderExercisesTranslationAndBodyPWeight, true);
	}

	int getExerciseIDFromEncoderTable () {
		//return getExerciseIDFromName (getExerciseNameFromEncoderTable());
		return getExerciseIDFromName (
				encoderExercisesTranslationAndBodyPWeight,
				getExerciseNameFromEncoderTable(), true);
	}
	
	string getExerciseNameFromEncoderTable () { //from first data row
		ArrayList array = getTreeViewCurves(encoderAnalyzeListStore);
		return ( (EncoderCurve) array[0] ).Exercise;
	}

	int getExercisePercentBodyWeightFromID (int exerciseID)
	{
		string found = Util.FindOnArray(':', 0, 3, exerciseID.ToString (),
				encoderExercisesTranslationAndBodyPWeight);
		if (Util.IsNumber(found, false))
			return Convert.ToInt32(found);

		return -1;
	}

	int getExercisePercentBodyWeightFromName (string name) {
		string found = Util.FindOnArray(':', 2, 3, name, 
				encoderExercisesTranslationAndBodyPWeight);
		if(Util.IsNumber(found, false))
			return Convert.ToInt32(found);
		else {
			//try untranslated
			found = Util.FindOnArray(':', 1, 3, name, 
					encoderExercisesTranslationAndBodyPWeight);
			if(Util.IsNumber(found, false))
				return Convert.ToInt32(found);
			else
				return -1;
		}
	}
	int getExercisePercentBodyWeightFromComboCapture () {
		return getExercisePercentBodyWeightFromName (UtilGtk.ComboGetActive(combo_encoder_exercise_capture));
	}
	int getExercisePercentBodyWeightFromTable () { //from first data row
		ArrayList array = getTreeViewCurves(encoderAnalyzeListStore);
		string exerciseName = ( (EncoderCurve) array[0] ).Exercise;
		return getExercisePercentBodyWeightFromName (exerciseName);
	}


	// ---------end of helpful methods -----------

	private void checkIfEncoderMinHeightChanged ()
	{
		if(current_mode == Constants.Modes.POWERGRAVITATORY) {
			preferences.EncoderChangeMinHeight(false,
					Convert.ToInt32(spin_encoder_capture_min_height_gravitatory.Value));

		} else { // (current_mode == Constants.Modes.POWERINERTIAL)
			preferences.EncoderChangeMinHeight(true,
					Convert.ToInt32(spin_encoder_capture_min_height_inertial.Value));
		}
	}

	//useful when there are no exercises (have been removed from database)
	bool selectedEncoderExerciseExists ()
	{
		return (getExerciseIDFromEncoderCombo(exerciseCombos.CAPTURE) != -1);
	}

	private void prepare_encoder_exercise_add_edit (bool adding)
	{
		hbox_encoder_exercise_close_and.Sensitive = false;
		hbox_encoder_exercise_encoder.Sensitive = false;
		hbox_encoder_exercise_select.Sensitive = false;
		hbox_encoder_exercise_actions.Visible = true;
		button_encoder_exercise_actions_edit_do.Visible = ! adding;
		button_encoder_exercise_actions_add_do.Visible = adding;
		notebook_encoder_exercise.Page = 1;
	}

	//info is now info and edit (all values can be changed), and detete (there's delete button)
	void on_button_encoder_exercise_edit_clicked (object o, EventArgs args) 
	{
		if(! selectedEncoderExerciseExists())
		{
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Need to create/select an exercise."));
			return;
		}

		EncoderExercise ex = SqliteEncoderExercise.SelectEncoderExercises (
				false, getExerciseIDFromEncoderCombo(exerciseCombos.CAPTURE),
				false, Constants.GetEncoderGIByMode (current_mode))[0];
		//LogB.Information("exercise: " + ex.ToString());

		prepare_encoder_exercise_add_edit (false);

		entry_encoder_exercise_name.Text = ex.name;
		spin_encoder_exercise_displaced_body_weight.Value = ex.percentBodyWeight;
		spin_encoder_exercise_speed_1rm.Value = ex.speed1RM;
		entry_encoder_exercise_resistance.Text = ex.ressistance;
		entry_encoder_exercise_description.Text = ex.description;

		//conditions for the radios
		//1 select if there gravitatory sets done with this exercise
		bool gravitatoryCaptured = (SqliteEncoder.Select (false, -1, -1, -1, Constants.EncoderGI.GRAVITATORY,
				ex.UniqueID, "all", EncoderSQL.Eccons.ALL, "",
				false, true, false).Count > 0);
		bool inertialCaptured = (SqliteEncoder.Select (false, -1, -1, -1, Constants.EncoderGI.INERTIAL,
				ex.UniqueID, "all", EncoderSQL.Eccons.ALL, "",
				false, true, false).Count > 0);

		button_radio_encoder_exercise_help.Visible = false;
		button_radio_encoder_exercise_help_message = "";

		// problems with exercise type and captured that have been done
		// A) change the exercise to all if this exercise has gravitatory and inertial captures
		if(gravitatoryCaptured && inertialCaptured)
		{
			radio_encoder_exercise_gravitatory.Sensitive = false;
			radio_encoder_exercise_inertial.Sensitive = false;
			radio_encoder_exercise_all.Sensitive = true;

			radio_encoder_exercise_all.Active = true;

			button_radio_encoder_exercise_help.Visible = true;
			button_radio_encoder_exercise_help_message = Catalog.GetString("This exercise has been used on gravitatory and inertial sets.");
		}
		// B) if this exercise is gravitatory but has inertial captures, unsensitive gravitatory and select all
		else if(ex.Type == Constants.EncoderGI.GRAVITATORY && inertialCaptured)
		{
			radio_encoder_exercise_gravitatory.Sensitive = false;
			radio_encoder_exercise_inertial.Sensitive = true;
			radio_encoder_exercise_all.Sensitive = true;

			radio_encoder_exercise_all.Active = true;

			button_radio_encoder_exercise_help.Visible = true;
			button_radio_encoder_exercise_help_message = Catalog.GetString("This exercise has been used on inertial sets.");
		}
		// C) if this exercise is inertial but has gravitatory captures, unsensitive inertial and select all
		else if(ex.Type == Constants.EncoderGI.INERTIAL && gravitatoryCaptured)
		{
			radio_encoder_exercise_gravitatory.Sensitive = true;
			radio_encoder_exercise_inertial.Sensitive = false;
			radio_encoder_exercise_all.Sensitive = true;

			radio_encoder_exercise_all.Active = true;

			button_radio_encoder_exercise_help.Visible = true;
			button_radio_encoder_exercise_help_message = Catalog.GetString("This exercise has been used on gravitatory sets.");
		}
		// No problem
		else {
			if(current_mode == Constants.Modes.POWERGRAVITATORY) {
				radio_encoder_exercise_gravitatory.Sensitive = true;
				radio_encoder_exercise_inertial.Sensitive = false;
			} else { // (current_mode == Constants.Modes.POWERINERTIAL)
				radio_encoder_exercise_gravitatory.Sensitive = false;
				radio_encoder_exercise_inertial.Sensitive = true;
			}

			if(ex.Type == Constants.EncoderGI.GRAVITATORY)
				radio_encoder_exercise_gravitatory.Active = true;
			else if(ex.Type == Constants.EncoderGI.INERTIAL)
				radio_encoder_exercise_inertial.Active = true;
			else
				radio_encoder_exercise_all.Active = true;
		}

		hbox_encoder_exercise_speed_1rm.Sensitive = ! radio_encoder_exercise_inertial.Active;
	}

	private string button_radio_encoder_exercise_help_message;
	private void on_button_radio_encoder_exercise_help_clicked (object o, EventArgs args)
	{
		new DialogMessage(Constants.MessageTypes.INFO, button_radio_encoder_exercise_help_message);
	}

	void on_button_encoder_exercise_add_clicked (object o, EventArgs args) 
	{
		prepare_encoder_exercise_add_edit (true);

		entry_encoder_exercise_name.Text = "";
		spin_encoder_exercise_displaced_body_weight.Value = 0;
		spin_encoder_exercise_speed_1rm.Value = 0;
		entry_encoder_exercise_resistance.Text = "";
		entry_encoder_exercise_description.Text = "";

		if(current_mode == Constants.Modes.POWERGRAVITATORY) {
			radio_encoder_exercise_gravitatory.Sensitive = true;
			radio_encoder_exercise_inertial.Sensitive = false;
		} else { // (current_mode == Constants.Modes.POWERINERTIAL)
			radio_encoder_exercise_gravitatory.Sensitive = false;
			radio_encoder_exercise_inertial.Sensitive = true;
		}

		hbox_encoder_exercise_speed_1rm.Sensitive = true;
		if(current_mode == Constants.Modes.POWERGRAVITATORY)
			radio_encoder_exercise_gravitatory.Active = true;
		else if(current_mode == Constants.Modes.POWERINERTIAL)
			radio_encoder_exercise_inertial.Active = true;
		else //this could not happen
			radio_encoder_exercise_all.Active = true;

		hbox_encoder_exercise_speed_1rm.Sensitive = ! radio_encoder_exercise_inertial.Active;

		button_radio_encoder_exercise_help.Visible = false;
		button_radio_encoder_exercise_help_message = "";
	}

	private void on_radio_encoder_exercise_radios_toggled (object o, EventArgs args)
	{
		hbox_encoder_exercise_speed_1rm.Sensitive =
			(radio_encoder_exercise_gravitatory.Active || radio_encoder_exercise_all.Active);
	}

	private void on_button_encoder_exercise_actions_cancel_clicked (object o, EventArgs args)
	{
		restore_encoder_exercise_sensitivity ();
	}
	private void on_button_encoder_exercise_actions_edit_do_clicked (object o, EventArgs args)
	{
		if(encoder_exercise_do_add_or_edit(false))
			restore_encoder_exercise_sensitivity ();
	}
	private void on_button_encoder_exercise_actions_add_do_clicked (object o, EventArgs args)
	{
		if(encoder_exercise_do_add_or_edit(true))
			restore_encoder_exercise_sensitivity ();
	}

	private void restore_encoder_exercise_sensitivity ()
	{
		hbox_encoder_exercise_close_and.Sensitive = true;
		hbox_encoder_exercise_encoder.Sensitive = true;
		hbox_encoder_exercise_select.Sensitive = true;
		hbox_encoder_exercise_actions.Visible = false;
		notebook_encoder_exercise.Page = 0;
	}


	bool encoder_exercise_do_add_or_edit (bool adding)
	{
		string name = Util.RemoveTildeAndColonAndDot(entry_encoder_exercise_name.Text);
		name = Util.RemoveComma (name); 	//to not make fail encoder exercise on tables sent to R
		name = Util.RemoveSemicolon (name);	//to not make fail encoder exercise on tables sent to R
		name = Util.RemoveChar(name, '"');

		if(adding)
			LogB.Information("Trying to insert: " + name);
		else
			LogB.Information("Trying to edit: " + name);

		if(name == "")
		{
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Error: Missing name of exercise."));
			return false;
		}
		else if (adding && Sqlite.Exists(false, Constants.EncoderExerciseTable, name))
		{
			new DialogMessage(Constants.MessageTypes.WARNING, string.Format(Catalog.GetString(
							"Error: An exercise named '{0}' already exists."), name));
			return false;
		}
		else if (! adding) //if we are editing
		{
			//if we edit, check that this name does not exists (on other exercise, on current editing exercise is obviously fine)
			int getIdOfThis = Sqlite.ExistsAndGetUniqueID(false, Constants.EncoderExerciseTable, name); //if not exists will be -1
			/*
			   LogB.Information("getIdOfThis " + getIdOfThis.ToString());
			   LogB.Information("if from combo " + getExerciseIDFromEncoderCombo(exerciseCombos.CAPTURE).ToString());
			   */

			if(getIdOfThis != -1 && getIdOfThis != getExerciseIDFromEncoderCombo(exerciseCombos.CAPTURE))
			{
				new DialogMessage(Constants.MessageTypes.WARNING, string.Format(Catalog.GetString(
								"Error: An exercise named '{0}' already exists."), name));

				return false;
			}
		}

		Constants.EncoderGI type = Constants.EncoderGI.ALL;
		if(radio_encoder_exercise_gravitatory.Active)
			type = Constants.EncoderGI.GRAVITATORY;
		else if(radio_encoder_exercise_inertial.Active)
			type = Constants.EncoderGI.INERTIAL;

		if(adding)
			SqliteEncoderExercise.InsertExercise(false, -1,
					name,
					Convert.ToInt32(spin_encoder_exercise_displaced_body_weight.Value),
					entry_encoder_exercise_resistance.Text,
					entry_encoder_exercise_description.Text,
					Util.ConvertToPoint(spin_encoder_exercise_speed_1rm.Value),
					type);
		else {
			EncoderExercise ex = new EncoderExercise(
					getExerciseIDFromEncoderCombo(exerciseCombos.CAPTURE),
					name,
					Convert.ToInt32(spin_encoder_exercise_displaced_body_weight.Value),
					entry_encoder_exercise_resistance.Text,
					entry_encoder_exercise_description.Text,
					spin_encoder_exercise_speed_1rm.Value,
					type);
			SqliteEncoderExercise.UpdateExercise(false, ex);
		}

		updateEncoderExercisesGui(name);
		LogB.Information("done");
		return true;
	}

	private void updateEncoderExercisesGui(string name)
	{
		List<EncoderExercise> encoderExercise_l = SqliteEncoderExercise.SelectEncoderExercises (
				false,-1, false, Constants.GetEncoderGIByMode (current_mode));
		encoderExercisesTranslationAndBodyPWeight = new String [encoderExercise_l.Count];
		string [] exerciseNamesToCombo = new String [encoderExercise_l.Count];
		int i =0;
		foreach (EncoderExercise ex in encoderExercise_l) {
			string nameTranslated = ex.name;
			//Translate Chronojump already created exercises in SqliteEncoder.initializeTableEncoderExercise()
			//but do not translate user created exercises
			if(ex.name == "Bench press" || ex.name == "Squat" || ex.name == "Jump" || ex.name == "Free")
				nameTranslated = Catalog.GetString(ex.name);
			encoderExercisesTranslationAndBodyPWeight[i] =
				ex.uniqueID + ":" + ex.name + ":" + nameTranslated + ":" + ex.percentBodyWeight;
			exerciseNamesToCombo[i] = Catalog.GetString(ex.name);
			i++;
		}
		UtilGtk.ComboUpdate(combo_encoder_exercise_capture, exerciseNamesToCombo, "");
		combo_encoder_exercise_capture.Active = UtilGtk.ComboMakeActive(combo_encoder_exercise_capture, name);

		exerciseNamesToCombo = addAllExercisesToComboExerciseAnalyze(exerciseNamesToCombo);

		UtilGtk.ComboUpdate(combo_encoder_exercise_analyze, exerciseNamesToCombo, "");
		combo_encoder_exercise_analyze.Active = UtilGtk.ComboMakeActive(combo_encoder_exercise_analyze, name);
	}
	
	void on_button_encoder_exercise_delete_clicked (object o, EventArgs args)
	{
		if(! selectedEncoderExerciseExists())
		{
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Need to create/select an exercise."));
			return;
		}

		EncoderExercise ex = SqliteEncoderExercise.SelectEncoderExercises (
				false, getExerciseIDFromEncoderCombo(exerciseCombos.CAPTURE), false,
				Constants.GetEncoderGIByMode (current_mode))[0];

		ArrayList array = SqliteEncoderExercise.SelectEncoderSetsOfAnExercise(false, ex.UniqueID); //dbconOpened, exerciseID

		if(array.Count > 0)
		{
			genericWin = GenericWindow.Show(Catalog.GetString("Exercise"),
					Catalog.GetString("Exercise name:"), Constants.GenericWindowShow.ENTRY, false);

			genericWin.EntrySelected = ex.Name;

			//just one button to exit and with ESC accelerator
			genericWin.ShowButtonAccept(false);
			genericWin.SetButtonCancelLabel(Catalog.GetString("Close"));

			//there are some records of this exercise on encoder table, do not delete
			genericWin.SetTextview(
					Catalog.GetString("Sorry, this exercise cannot be deleted until these tests are deleted:"));

			ArrayList nonSensitiveRows = new ArrayList();
			for(int i=0; i < array.Count; i ++)
				nonSensitiveRows.Add(i);

			genericWin.SetTreeview(
					new string [] {
					"count",	//not shown, unused
					Catalog.GetString("Sets"), Catalog.GetString("Person"),
					Catalog.GetString("Session"), Catalog.GetString("Date") }, 
					false, array, nonSensitiveRows, GenericWindow.EditActions.NONE, false);

			genericWin.ShowTextview();
			genericWin.ShowTreeview();
		} else {
			//encoder table has not records of this exercise
			//delete exercise
			Sqlite.Delete(false, Constants.EncoderExerciseTable, ex.UniqueID);
			//delete 1RM records of this exercise
			Sqlite.DeleteFromAnInt(false, Constants.Encoder1RMTable, "exerciseID", ex.UniqueID);

			createEncoderComboExerciseAndAnalyze();
			combo_encoder_exercise_capture.Active = 0;
			combo_encoder_exercise_analyze.Active = 0;

			new DialogMessage(Constants.MessageTypes.INFO, Catalog.GetString("Exercise deleted."));
		}
	}


	/* sensitivity stuff */	
	//called when a person changes
	private void encoderPersonChanged() 
	{
		//on cont person, exercise and mass can be changed
		if(eCapture != null && capturingCsharp == encoderCaptureProcess.CAPTURING)
		{
			eCapture.Cancel();
			Thread.Sleep (100);
			on_button_encoder_capture_clicked (new object(), new EventArgs ());
			return;
		}

		encoderButtonsSensitive(encoderSensEnum.YESPERSON);
		
		array1RMUpdate(false);
		encoder_change_displaced_weight_and_1RM ();
	
		blankEncoderInterface();
		updateEncoderAnalyzeExercisesPre ();
	}
	
	/* called on:
	 * encoderPersonChanged()
	 * changeModeCheckRadios (Constants.Modes m)
	 */
	private void blankEncoderInterface()
	{
		if(radio_encoder_analyze_individual_current_set.Active)
			updateComboEncoderAnalyzeCurveNumFromCurrentSet ();
		else {
			if(currentPerson != null)
				prepareAnalyzeRepetitions();
		}
	
		//blank the encoderCaptureListStore
		encoderCaptureListStore = new Gtk.ListStore (typeof (EncoderCurve));
		button_encoder_analyze_sensitiveness();
		
		treeviewEncoderCaptureRemoveColumns();
		blankEncoderCurrentSetGraphs ();
		updateGraphEncoderSessionBars ();

		image_encoder_analyze.Sensitive = false;
		vbox_encoder_analyze_instant.Visible = false; //play with Visible instead of Sensitive because with Sensitive the pixmap is fully shown
		treeview_encoder_analyze_curves.Sensitive = false;

		button_encoder_analyze_image_save.Sensitive = false;
		button_encoder_analyze_image_compujump_send_email.Sensitive = false;
		button_encoder_analyze_AB_save.Sensitive = false;
		button_encoder_analyze_table_save.Sensitive = false;
		button_encoder_analyze_1RM_save.Visible = false;

		button_video_play_this_test.Sensitive = false;
	}

	private void blankEncoderCurrentSetGraphs ()
	{
		//initialize new captureCurvesBarsData_l to not having the barplot updated on CONFIGURE or EXPOSE after being painted white
		captureCurvesBarsData_l = new List<EncoderBarsData> ();

		//erase cairo barplot
		cairoPaintBarsPreCurrent = new CairoPaintBarsPreEncoderCurrent (
			encoder_capture_curves_bars_drawingarea_cairo,
			preferences.fontTypeToGraph());
		prepareEventGraphEncoderCurrent = null; //to avoid is repainted again, and sound be repeated;

		//erase cairoGraphEncoderSignal
		cairoGraphEncoderSignal = null;
		cairoGraphEncoderSignalPoints_l = new List<PointF>();
		cairoGraphEncoderSignalInertialPoints_l = new List<PointF>();
	}

	private void encoderButtonsSensitive(encoderSensEnum option) 
	{
		LogB.Debug("encoderButtonsSensitive: " + option.ToString());

		//columns
		//c0 button_encoder_capture,
		//	hbox_encoder_configuration, frame_encoder_capture_options
		//c1 // button_encoder_exercise_close_and_recalculate
		//c2 (before it has overview and load) button_encoder_load_signal_at_analyze
		//c3 button_encoder_export_signal,
		//	button_contacts_edit_selecte, button_contacts_delete_selected,
		//	and images: image_encoder_capture , image_encoder_analyze.Sensitive. Update: both NOT managed here
		//	button_encoder_capture_image_save
		//UNUSED c4 button_encoder_save_curve, entry_encoder_curve_comment
		//c5 button_encoder_analyze
		//c6 button_encoder_analyze_data_select_curves
		//c7 button_encoder_capture_cancel and button_encoder_analyze_cancel (on capture and analyze)
		//c8 button_encoder_capture_finish (only on capture)

		//other dependencies
		//c5 True needs 
		//	(signal && treeviewEncoder has rows) || 
		//	(! radio_encoder_analyze_individual_current_set.Active && user has curves))
		//c6 True needs ! radio_encoder_analyze_individual_current_set.Active

		if(option != encoderSensEnum.PROCESSINGCAPTURE && option != encoderSensEnum.PROCESSINGR)
			encoderSensEnumStored = option;
		
		//columns		 	 0  1  2  3  4  5  6  7  8
		int [] noSession = 		{0, 0, 0, 0, 0, 0, 0, 0, 0};
		int [] noPerson = 		{0, 0, 0, 0, 0, 0, 0, 0, 0};
		int [] yesPerson = 		{1, 0, 1, 0, 0, 1, 1, 0, 0};
		int [] processingCapture = 	{0, 0, 0, 0, 0, 0, 1, 1, 1};
		int [] processingR = 		{0, 0, 0, 0, 0, 0, 1, 0, 0};
		int [] doneNoSignal = 		{1, 0, 1, 0, 0, 1, 1, 0, 0};
		int [] doneYesSignal = 		{1, 1, 1, 1, 0, 1, 1, 0, 0};
		int [] table = new int[7];

		switch(option) {
			case encoderSensEnum.NOSESSION:
				table = noSession;
				break;
			case encoderSensEnum.NOPERSON:
				table = noPerson;
				break;
			case encoderSensEnum.YESPERSON:
				table = yesPerson;
				break;
			case encoderSensEnum.PROCESSINGCAPTURE:
				table = processingCapture;
				break;
			case encoderSensEnum.PROCESSINGR:
				table = processingR;
				break;
			case encoderSensEnum.DONENOSIGNAL:
				table = doneNoSignal;
				break;
			case encoderSensEnum.DONEYESSIGNAL:
				table = doneYesSignal;
				break;
		}
		button_encoder_capture.Sensitive = Util.IntToBool(table[0]);
		hbox_contacts_sup_capture_analyze_two_buttons.Sensitive = Util.IntToBool(table[0]);
		frame_encoder_capture_options.Sensitive = Util.IntToBool(table[0]);

		//button_encoder_exercise_close_and_recalculate.Sensitive = Util.IntToBool(table[1]);

		button_encoder_load_signal_at_analyze.Sensitive = Util.IntToBool(table[2]);

		button_encoder_export_signal.Sensitive = Util.IntToBool(table[3]);
		button_contacts_edit_selected.Sensitive = Util.IntToBool(table[3]);
		button_contacts_delete_selected.Sensitive = Util.IntToBool(table[3]);
		button_encoder_capture_image_save.Sensitive = Util.IntToBool(table[3]);
		//image_encoder_capture.Sensitive = Util.IntToBool(table[3]);
		//image_encoder_analyze.Sensitive = Util.IntToBool(table[3]);
		
		//button_encoder_save_curve.Sensitive = Util.IntToBool(table[4]);
		//entry_encoder_curve_comment.Sensitive = Util.IntToBool(table[4]);

		bool analyze_sensitive = 
			(
			 Util.IntToBool(table[5]) && 
			 (
			  (radio_encoder_analyze_individual_current_set.Active &&
			   UtilGtk.CountRows(encoderCaptureListStore) > 0) 
			  ||
			  ( ! radio_encoder_analyze_individual_current_set.Active &&
			   Convert.ToInt32(label_encoder_user_curves_all_num.Text) >0)
			  )
			 );
		//max 12 graphs on side compare
		if(analyze_sensitive && radiobutton_encoder_analyze_instantaneous.Active && radiobutton_encoder_analyze_side.Active) {
			analyze_sensitive = curvesNumOkToSideCompare();
			label_encoder_analyze_side_max.Visible = ! analyze_sensitive;
		} else
			label_encoder_analyze_side_max.Visible = false;

		button_encoder_analyze.Sensitive = analyze_sensitive;

		button_encoder_analyze_data_select_curves.Visible =
			(Util.IntToBool(table[6]) && ! radio_encoder_analyze_individual_current_set.Active);
		
		button_encoder_capture_cancel.Sensitive = Util.IntToBool(table[7]);
		fullscreen_button_fullscreen_encoder.Sensitive = Util.IntToBool(table[7]);
		
		button_encoder_capture_finish.Sensitive = Util.IntToBool(table[8]);
		button_encoder_capture_finish_cont.Sensitive = Util.IntToBool(table[8]);
	}

	//only related to button_encoder_analyze
	private void button_encoder_analyze_sensitiveness()
	{
		bool analyze_sensitive = false;
		if(radio_encoder_analyze_individual_current_set.Active) {
			int rows = UtilGtk.CountRows(encoderCaptureListStore);
			
			//button_encoder_analyze.Sensitive = encoderTimeStamp != null;
			
			analyze_sensitive = (rows > 0);
			if(analyze_sensitive && radiobutton_encoder_analyze_instantaneous.Active && radiobutton_encoder_analyze_side.Active) {
				analyze_sensitive = curvesNumOkToSideCompare();
				label_encoder_analyze_side_max.Visible = ! analyze_sensitive;
			}
		} else {
			analyze_sensitive = (currentPerson != null && encSelReps.RepsActive > 0);
			if(analyze_sensitive && radiobutton_encoder_analyze_instantaneous.Active && radiobutton_encoder_analyze_side.Active) {
				analyze_sensitive = curvesNumOkToSideCompare();
				label_encoder_analyze_side_max.Visible = ! analyze_sensitive;
			}

			//1RM Indirect only works on current set
			if(
					radiobutton_encoder_analyze_1RM.Active &&
					Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_analyze_1RM),
						encoderAnalyze1RMTranslation) == "1RM Indirect")
				analyze_sensitive = false;
		}
		button_encoder_analyze.Sensitive = analyze_sensitive;
		button_encoder_analyze_mode_options_close_and_analyze.Sensitive = analyze_sensitive;
	}

	/*
	 * we want to have device sensitive
	 * and sensitive/unsensitive the rest of widgets
	 * suitable to change device without having a person loaded
	 */
	private void encoder_sensitive_all_except_device(bool s)
	{
		frame_encoder_capture_options.Sensitive = s;
		hbox_encoder_capture_actions.Sensitive = s;
		hbox_video_encoder.Sensitive = s;
		vbox_encoder_bars_table_and_save_reps.Sensitive = s;
	}

	/* end of sensitivity stuff */	

	/*
	 * ------ barplot current set ------>
	 */

	enum UpdateEncoderPaintModes { GRAVITATORY, INERTIAL, CALCULE_IM }
	private void updateEncoderCaptureGraphPaintData (UpdateEncoderPaintModes mode)
	{
		if(eCapture.PointsCaptured == 0 ||
				eCapture.PointsCaptured - eCapture.PointsPainted <= 0)
			return;

		if(mode == UpdateEncoderPaintModes.CALCULE_IM)
		{
			encoder_configuration_win.EncoderReaded (eCapture.Sum, eCapture.IMCalcOscillations);
			return;
		}

		if(mode == UpdateEncoderPaintModes.GRAVITATORY || mode == UpdateEncoderPaintModes.INERTIAL)
		{
			//TODO: check this < instead of <= does not fail on capture
			//this applies to both
			for (int i = eCapture.PointsPainted +1 ; i < eCapture.PointsCaptured ; i ++)
			{
				if (preferences.signalDirectionHorizontal)
					cairoGraphEncoderSignalPoints_l.Add (new PointF (
								eCapture.EncoderCapturePointsCairo[i].X,
								UtilAll.DivideSafe (eCapture.EncoderCapturePointsCairo[i].Y, 10.0) //cm
								));
				else
					cairoGraphEncoderSignalPoints_l.Add (new PointF (
								UtilAll.DivideSafe (eCapture.EncoderCapturePointsCairo[i].X, 10.0), //cm
								eCapture.EncoderCapturePointsCairo[i].Y
								));
			}

			//TODO: check this < instead of <= does not fail on capture
			if (mode == UpdateEncoderPaintModes.INERTIAL)
				for (int i = eCapture.PointsPainted +1 ; i < eCapture.PointsCaptured ; i ++)
				{
					if (preferences.signalDirectionHorizontal)
						cairoGraphEncoderSignalInertialPoints_l.Add (new PointF (
									eCapture.EncoderCapturePointsInertialDiscCairo[i].X,
									UtilAll.DivideSafe (eCapture.EncoderCapturePointsInertialDiscCairo[i].Y, 10.0) //cm
									));
					else
						cairoGraphEncoderSignalInertialPoints_l.Add (new PointF (
									UtilAll.DivideSafe (eCapture.EncoderCapturePointsInertialDiscCairo[i].X, 10.0), //cm
									eCapture.EncoderCapturePointsInertialDiscCairo[i].Y
									));
				}

			eCapture.PointsPainted = eCapture.PointsCaptured;
		}
	}


	static List<string> encoderCaptureStringR;
	static List<EncoderBarsData> captureCurvesBarsData_l;

	private void callPlotCurvesGraphDoPlot()
	{
		if(captureCurvesBarsData_l.Count > 0)
		{
			string mainVariable = Constants.GetEncoderVariablesCapture(preferences.encoderCaptureMainVariable);
			string secondaryVariable = Constants.GetEncoderVariablesCapture(preferences.encoderCaptureSecondaryVariable);
			if(! preferences.encoderCaptureSecondaryVariableShow)
				secondaryVariable = "";
			double mainVariableHigher = feedbackWin.GetMainVariableHigher(mainVariable);
			double mainVariableLower = feedbackWin.GetMainVariableLower(mainVariable);

			//Cairo
			prepareEventGraphEncoderCurrent = new PrepareEventGraphEncoderCurrent (
					mainVariable, mainVariableHigher, mainVariableLower,
					secondaryVariable, preferences.encoderCaptureShowLoss,
					false, //not capturing
					findEcconFromCurrentSet (true),
					findDisplacedMassFromSQL (),
					feedbackEncoder,
					currentEncoderSQLSet.encoderConfiguration.has_inertia,
					configChronojump.PlaySoundsFromFile,
					captureCurvesBarsData_l,
					encoderCaptureListStore,
					preferences.encoderCaptureMainVariableThisSetOrHistorical,
					sendMaxPowerSpeedForceIntersession(preferences.encoderCaptureMainVariable),
					sendMaxPowerSpeedForceIntersessionDate(preferences.encoderCaptureMainVariable),
					preferences.encoderCaptureInertialDiscardFirstN,
					preferences.encoderCaptureShowNRepetitions,
					preferences.volumeOn,
					preferences.gstreamer);

			if (notebook_start.CurrentPage == Convert.ToInt32 (notebook_start_pages.FULLSCREENCAPTURE))
				fullscreen_capture_drawingarea_cairo.QueueDraw ();
			else
				encoder_capture_curves_bars_drawingarea_cairo.QueueDraw ();
		}
	}

	public void on_encoder_capture_curves_bars_drawingarea_cairo_button_press_event (object o, ButtonPressEventArgs args)
	{
		LogB.Information("on_encoder_capture_curves_bars_drawingarea_cairo_button_press_event 0");
		if(cairoPaintBarsPreCurrent == null) //TODO: check also that is the encoder graph and not jumps or whatever
			return;

		LogB.Information("on_encoder_capture_curves_bars_drawingarea_cairo_button_press_event 1");
		int repetition = cairoPaintBarsPreCurrent.FindBarInPixel (args.Event.X, args.Event.Y);
		//LogB.Information("Repetition: " + repetition.ToString());
		if(repetition >= 0)
		{
		LogB.Information("on_encoder_capture_curves_bars_drawingarea_cairo_button_press_event 2");
			//this will be managed by: EncoderCaptureItemToggled()
			encoderCaptureItemToggledArgsPath = repetition.ToString();
			EncoderCaptureItemToggled(new object (), new ToggledArgs());
			encoderCaptureItemToggledArgsPath = "";

			// update the signal graph
			encoder_capture_signal_drawingarea_cairo.QueueDraw ();
		}
	}

	public void on_encoder_capture_curves_bars_drawingarea_cairo_draw (object o, Gtk.DrawnArgs args)
	{
		LogB.Information("on_encoder_capture_curves_bars_drawingarea_cairo_draw A");
		encoder_capture_curves_bars_drawingarea_cairo.AddEvents((int) Gdk.EventMask.ButtonPressMask);

		//if object not defined or not defined fo this mode, return
//TODO: is fist check really needed?
//		if(cairoPaintBarsPreCurrent == null || ! cairoPaintBarsPreCurrent.ModeMatches (current_mode))
//			return;

		LogB.Information("on_encoder_capture_curves_bars_drawingarea_cairo_draw B");

		//note this is the same than on_fullscreen_capture_drawingarea_cairo_draw ()
		if(prepareEventGraphEncoderCurrent != null)
		{
			//prepareEncoderSignalBarplotCairo (false); //just redraw the graph
			prepareEncoderSignalBarplotCairo (true); //TODO: check if true or false
		}
	}

	private void prepareEncoderSignalBarplotCairo (bool calculateAll)
	{
		LogB.Information("prepareEncoderSignalBarplotCairo");
		if(currentPerson == null)
			return;

		Gtk.DrawingArea da = encoder_capture_curves_bars_drawingarea_cairo;
		if (notebook_start.CurrentPage == Convert.ToInt32 (notebook_start_pages.FULLSCREENCAPTURE))
			da = fullscreen_capture_drawingarea_cairo;


		if(cairoPaintBarsPreCurrent == null || calculateAll)
		{
			double videoTime = 0;
			if (webcamPlay != null && webcamPlay.PlayVideoGetSecond > 0)
				videoTime = webcamPlay.PlayVideoGetSecond -diffVideoVsSignal;

			cairoPaintBarsPreCurrent = new CairoPaintBarsPreEncoderCurrent (
					preferences, da, preferences.fontTypeToGraph(),
					currentPerson.Name, "", 3,
					prepareEventGraphEncoderCurrent, videoTime);
		}

		if (screenshotPending)
		{
			cairoPaintBarsPreCurrent.ScreenshotURL = screenshotURL;
			screenshotPending = false;
			screenshotURL = "";
		}

		cairoPaintBarsPreCurrent.Paint();
	}

	public void on_encoder_capture_signal_drawingarea_cairo_draw (object o, Gtk.DrawnArgs args)
	{
		//updateEncoderCaptureSignalCairo (current_mode == Constants.Modes.POWERINERTIAL, true);
		updateEncoderCaptureSignalCairo (current_mode == Constants.Modes.POWERINERTIAL, false); //TODO: recheck if true or false
	}
	private void updateEncoderCaptureSignalCairo (bool inertial, bool forceRedraw)
	{
		if(preferences.encoderCaptureShowOnlyBars == null || ! preferences.encoderCaptureShowOnlyBars.ShowSignal)
			return;

		if(cairoGraphEncoderSignal == null)
		{
			if (preferences.encoderFeedbackAsteroidsActive)
				cairoGraphEncoderSignal = new CairoGraphEncoderSignalAsteroids (
						encoder_capture_signal_drawingarea_cairo, "title",
						preferences.signalDirectionHorizontal);
			else
				cairoGraphEncoderSignal = new CairoGraphEncoderSignal (
						encoder_capture_signal_drawingarea_cairo, "title",
						preferences.encoderSignalDisplAxisCustom,
						preferences.encoderSignalDisplAxisCustomMax,
						preferences.encoderSignalDisplAxisCustomMin,
						preferences.signalDirectionHorizontal);
		}

		if (preferences.encoderFeedbackAsteroidsActive)
			cairoGraphEncoderSignal.PassAsteroids = asteroids;
		else {
		       if (captureCurvesBarsData_l != null && captureCurvesBarsData_l.Count > 0)
			       cairoGraphEncoderSignal.PassRepetitions (captureCurvesBarsData_l);
		}

		double videoTime = 0;
		if (webcamPlay != null && webcamPlay.PlayVideoGetSecond > 0)
		{
			videoTime = webcamPlay.PlayVideoGetSecond -diffVideoVsSignal;
		}

		string eccon = "c";
		if (capturingCsharp == encoderCaptureProcess.CAPTURING)
			eccon = findEcconFromCaptureGui (true);
		else if (currentEncoderSQLSet != null)
			eccon = findEcconFromCurrentSet (true);

		int discardNReps = 0;
		if (current_mode == Constants.Modes.POWERINERTIAL)
			discardNReps = preferences.encoderCaptureInertialDiscardFirstN;

		cairoGraphEncoderSignal.DoSendingList (preferences.fontTypeToGraph(),
				capturingCsharp == encoderCaptureProcess.CAPTURING,
				inertial,
				cairoGraphEncoderSignalPoints_l, cairoGraphEncoderSignalInertialPoints_l,
				encoderCaptureListStore, // to know saved (Record) repetitions
				eccon,
				discardNReps,
				videoTime,
				forceRedraw, CairoXY.PlotTypes.LINES);
	}

	private double sendMaxPowerSpeedForceIntersession(Constants.EncoderVariablesCapture evc)
	{
		if(evc == Constants.EncoderVariablesCapture.MeanPower)
		       return maxPowerIntersession;
		else if(evc == Constants.EncoderVariablesCapture.MeanSpeed)
		       return maxSpeedIntersession;
		else if(evc == Constants.EncoderVariablesCapture.MeanForce)
		       return maxForceIntersession;

		return maxPowerIntersession; //default if any problem
	}
	private string sendMaxPowerSpeedForceIntersessionDate(Constants.EncoderVariablesCapture evc)
	{
		if(evc == Constants.EncoderVariablesCapture.MeanPower)
		       return maxPowerIntersessionDate;
		else if(evc == Constants.EncoderVariablesCapture.MeanSpeed)
		       return maxSpeedIntersessionDate;
		else if(evc == Constants.EncoderVariablesCapture.MeanForce)
		       return maxForceIntersessionDate;

		return maxPowerIntersessionDate; //default if any problem
	}

	/*
	 * <------ barplot current set ------
	 */

	//resultsSession
	private void updateGraphEncoderSessionBars ()
	{
		LogB.Information ("updateGraphEncoderSessionBars");
		LogB.Information (string.Format ("currentPerson == null: {0},  currentSession == null: {1}",
					currentPerson == null, currentSession == null));
		if(currentPerson == null || currentSession == null)
			return;

		// 1. prepare eventGraph object needed for the graph

		//initalizeVariables if not done before
		event_execute_initializeVariables(
			(! cp2016.StoredCanCaptureContacts && ! cp2016.StoredWireless), //is simulated
			currentPerson.UniqueID,
			currentPerson.Name,
			"", //Catalog.GetString("Phases"),  	  //name of the different moments
			Constants.EncoderTable, //tableName
			"" //type
			);

		int selectedID = -1;
		double selectedWeight = -1; //gravitatory
		EncoderConfiguration selectedEconf; //inertial

		int exerciseID; //if test is selected on resultsSession will be this. If not will be the combo on gui for next test

		if (treeViewResultsSession != null && treeViewResultsSession.EventSelectedID >= 0)
		{
			selectedID = treeViewResultsSession.EventSelectedID;
			EncoderSQL eSQL = SqliteEncoder.SelectData (selectedID, false);
			exerciseID = eSQL.exerciseID;
			selectedWeight = eSQL.extraWeightD;
			selectedEconf = eSQL.encoderConfiguration;
		} else {
			exerciseID = getExerciseIDFromAnyCombo (combo_encoder_exercise_capture, encoderExercisesTranslationAndBodyPWeight, false);
			selectedWeight = Convert.ToDouble (spin_encoder_extra_weight.Value);
			selectedEconf = encoderConfigurationNewCapture;
		}

		PrepareEventGraphEncoderSession eventGraph = new PrepareEventGraphEncoderSession (
				currentSession.UniqueID,
				currentPerson.UniqueID, currentPerson.Name, radio_contacts_results_personAll.Active,
				currentEncoderGI,
				get_radio_resultsSession_criteria (),
				preferences.encoderCaptureMainVariable,
				-1 * Convert.ToInt32 (spin_resultsSession_limit.Value), //negative: end limit
				//Constants.EncoderTable, typeTemp,
				exerciseID,
				UtilGtk.ComboGetActive (combo_encoder_exercise_capture),
				selectedID, selectedWeight, selectedEconf, current_mode, radio_contacts_graph_allTests.Active);

		// debug
		//LogB.Information ("debugging");
		//foreach (EncoderSQL eSQL in eventGraph.rowsAtSQL)
		//	LogB.Information (eSQL.ToString ());

		// 2. Do the graph

		string typeTemp = "";
		if(! radio_contacts_graph_allTests.Active)
			typeTemp = UtilGtk.ComboGetActive (combo_encoder_exercise_capture);

		string personStr = "";
		if(! radio_contacts_results_personAll.Active)
			personStr = currentPerson.Name;

		cairoPaintBarsPre = new CairoPaintBarsPreEncoderSession (
				drawingarea_results_session, preferences.fontTypeToGraph(), current_mode,
				personStr,
				typeTemp,
				preferences.digitsNumber,
				radio_contacts_results_personAll.Active, //showPersonName
				currentPerson.UniqueID,
				preferences.encoderCaptureMainVariable, radio_resultsSession_bars.Active
				);

		cairoPaintBarsPre.StoreEventGraphEncoderSession (eventGraph);
		drawingarea_results_session.QueueDraw ();
	}



	/*
	 * end of update encoder capture graph stuff
	 */
	

	//while capturing, some buttons are hidden, others are shown
	void encoderShowCaptureDoingButtons(bool show) {
		hbox_encoder_capture_wait.Visible = ! show;
		box_encoder_capture_doing.Visible = show;

		button_encoder_capture_cancel.Visible = ! preferences.encoderCaptureInfinite;
		button_encoder_capture_finish.Visible = ! preferences.encoderCaptureInfinite;
		button_encoder_capture_finish_cont.Visible = preferences.encoderCaptureInfinite;
	}

	private void runEncoderCaptureNoRDotNetInitialize() 
	{
		EncoderParams encoderParams = new EncoderParams(
				preferences.EncoderCaptureMinHeight (current_mode == Constants.Modes.POWERINERTIAL), 
				getExercisePercentBodyWeightFromComboCapture (),
				Util.ConvertToPoint (findMassFromGui (Constants.MassType.BODY)),
				Util.ConvertToPoint (findMassFromGui (Constants.MassType.EXTRA)),
				findEcconFromCaptureGui (true),			//force ecS (ecc-conc separated)
				"-",		//analysis
				"none",		//analysisVariables (not needed in create curves). Cannot be blank
				getEncoderAnalysisOptions(),	//used on capture for pass the 'p' of propulsive
				preferences.encoderCaptureCheckFullyExtended,
				preferences.encoderCaptureCheckFullyExtendedValue,
				encoderConfigurationNewCapture,
				Util.ConvertToPoint(preferences.encoderSmoothCon),	//R decimal: '.'
			       	0, 			//curve is not used here
				image_encoder_width, image_encoder_height,
				preferences.CSVExportDecimalSeparator 
				);

		EncoderStruct es = new EncoderStruct(
				UtilEncoder.GetEncoderScriptCaptureNoRdotNet(),//1st option used here to allow to call the main capture script
				UtilEncoder.GetEncoderCaptureTempFileName(),   //2nd option used here to print the captured data file
				"none", //UtilEncoder.GetEncoderCurvesTempFileName(), 
				UtilEncoder.GetEncoderScriptsPathWithoutLastSep(),
				UtilEncoder.GetEncoderTempPathWithoutLastSep(),
				encoderParams);

		encoderRProcCapture.NeedRForCapture = (csharpOrR == EncoderCapture.CsharpOrR.R || csharpOrR == EncoderCapture.CsharpOrR.BOTH);
		encoderRProcCapture.StartOrContinue (es);

		if (csharpOrR == EncoderCapture.CsharpOrR.CSHARP || csharpOrR == EncoderCapture.CsharpOrR.BOTH)
			encoderRProcCapture.InitCsharp (encoderParams);
	}
	

	private void deleteAllCapturedCurveFiles()
	{
		foreach (var f in new DirectoryInfo(Path.GetTempPath()).GetFiles(
					Constants.EncoderCaptureTemp + "-*")) {
			    f.Delete();
		}
		Util.FileDelete(UtilEncoder.GetEncoderCaptureTempFileName() + "-*");
	}	
	private string readingCurveFromRFilenameCompose(int curveNum)
	{
		string filenameBegins = UtilEncoder.GetEncoderCaptureTempFileName();
		if(curveNum > 99)
			return(filenameBegins + "-" + curveNum.ToString());	//eg. "filename-123"
		else if(curveNum > 9)
			return(filenameBegins + "-0" + curveNum.ToString());	//eg. "filename-023"
		else //(curveNum <= 9)
			return(filenameBegins + "-00" + curveNum.ToString());	//eg. "filename-003"
	}

	/*
	 * History
	 * 1) In the beginning we used RDotNet for C# - R communication. But it was buggy, complex, problems with try catch, …
	 * 2) Then we used stdin,stdout,stderr communication. Worked fine on Linux and Windows but not in Mac
	 * 3) Then we used a capture.txt file created by R with a row for each curve. But reading it on windows from C# gives file access problems
	 * 4) Now we try to create one file for each curve and read it here with a try/catch
	 */

	static bool needToRefreshTreeviewCapture;
	static int encoderCaptureReadedLines;
	//private void readingCurveFromR (object sendingProcess, DataReceivedEventArgs curveFromR)
	private void readingCurveFromR ()
	{
		/*
		 * 3) method ----
		string filename = UtilEncoder.GetEncoderCaptureTempFileName();
		if(! File.Exists(filename))
			return;
		
		//StreamReader reader = File.OpenText(filename);
		//string line = reader.ReadLine();
		
		string line = "";
		
		//http://stackoverflow.com/a/119572
		var lineCount = File.ReadLines(filename).Count();
		if(lineCount > encoderCaptureReadedLines) {
			//http://stackoverflow.com/a/1262985
			line = File.ReadLines(filename).Skip(encoderCaptureReadedLines ++).Take(1).First();
		}
		 * ---- end of 3) method
		 */

		//4) method ----
		string line = "";
		string filename = readingCurveFromRFilenameCompose(encoderCaptureReadedLines);
		//LogB.Debug("filename = ",filename);
		
		if(! File.Exists(filename))
			return;

		try {
			StreamReader reader = File.OpenText(filename);
			line = reader.ReadLine(); //just read first line
			reader.Close();
		}
		catch {
			LogB.Debug("catched - open later",encoderCaptureReadedLines.ToString());
			return;
		}
		//---- end of 4) method



		//if (!String.IsNullOrEmpty(curveFromR.Data))
		if (!String.IsNullOrEmpty(line))
		{
			//only mark as readed now because line it's not empty
			encoderCaptureReadedLines ++;

			LogB.Information("Without trim");
			//LogB.Information(curveFromR.Data);
			LogB.Information(line);

			//string trimmed = curveFromR.Data.Trim();
			string trimmed = line.Trim();
			LogB.Information("With trim");
			LogB.Information(trimmed);

			//fix if data couldn't be calculated from R
			trimmed = trimmed.Replace("NA","0");

			string [] strs = trimmed.Split(new char[] {','});

			readingCurveFromRCont (strs);
		}
	}

	private void readingCurveFromRCont (string [] strs)
	{
		//LogB.Information("before add: " + Util.StringArrayToString(strs, "///"));
		encoderCaptureStringR.Add(string.Format("\n" +
					"{0},2,a,3,4," + 		//id, seriesName, exerciseName, massBody, massExtra
					"{1},{2},{3}," + 		//start, width, height
					"{4},{5},{6},{7}," + 		//speeds
					"{8},{9},{10},{11}," + 		//powers
					"{12},{13},{14},{15}," + 	//forces
					"{16},{17}", 			//workJ, impulse
					strs[0],
					strs[1], strs[2], strs[3],		//start, width, height
					strs[4], strs[5], strs[6], strs[7],	//speeds
					strs[8], strs[9], strs[10], strs[11], 	//powers
					strs[12], strs[13], strs[14], strs[15], //forces
					strs[16], strs[17] 			//workJ, impulse
					));

		//LogB.Debug("encoderCaptureStringR");
		//LogB.Debug(encoderCaptureStringR);

		double start = Convert.ToDouble(Util.ChangeDecimalSeparator(strs[1]));
		double duration = Convert.ToDouble(Util.ChangeDecimalSeparator(strs[2]));
		double range = Convert.ToDouble(Util.ChangeDecimalSeparator(strs[3]));
		double meanSpeed = Convert.ToDouble(Util.ChangeDecimalSeparator(strs[4]));
		double maxSpeed = Convert.ToDouble(Util.ChangeDecimalSeparator(strs[5]));
		double meanForce = Convert.ToDouble(Util.ChangeDecimalSeparator(strs[12]));
		double maxForce = Convert.ToDouble(Util.ChangeDecimalSeparator(strs[13]));
		double meanPower = Convert.ToDouble(Util.ChangeDecimalSeparator(strs[8]));
		double peakPower = Convert.ToDouble(Util.ChangeDecimalSeparator(strs[9]));
		double workJ = Convert.ToDouble(Util.ChangeDecimalSeparator(strs[16]));
		double impulse = Convert.ToDouble(Util.ChangeDecimalSeparator(strs[17]));
		captureCurvesBarsData_l.Add (new EncoderBarsData (
					start, duration, range, meanSpeed, maxSpeed,
					meanForce, maxForce, meanPower, peakPower, workJ, impulse));

		LogB.Information("activating needToRefreshTreeviewCapture");

		//executed on GTK thread pulse method
		needToRefreshTreeviewCapture = true;
	}


	// -------------- drawingarea_encoder_analyze_instant
	
	Pixbuf drawingarea_encoder_analyze_cairo_pixbuf;
	
	void on_hscale_encoder_analyze_a_value_changed (object o, EventArgs args) {
		if(eai != null) {
			int ms = Convert.ToInt32(hscale_encoder_analyze_a.Value);
			label_encoder_analyze_time_a.Text = ms.ToString();
			label_encoder_analyze_displ_a.Text = Util.TrimDecimals(eai.GetParam("displ",ms), 1); //mm
			label_encoder_analyze_speed_a.Text = Util.TrimDecimals(eai.GetParam("speed",ms), 2);
			label_encoder_analyze_accel_a.Text = Util.TrimDecimals(eai.GetParam("accel",ms), 2);
			label_encoder_analyze_force_a.Text = Util.TrimDecimals(eai.GetParam("force",ms), 1);
			label_encoder_analyze_power_a.Text = Util.TrimDecimals(eai.GetParam("power",ms), 1);
			
			if(checkbutton_encoder_analyze_b.Active)
				encoder_analyze_instant_calculate_params();
		
			drawingarea_encoder_analyze_instant.QueueDraw(); //will fire ExposeEvent
		}
	}

	void on_hscale_encoder_analyze_b_value_changed (object o, EventArgs args) {
		if(eai != null) {
			int msb = Convert.ToInt32(hscale_encoder_analyze_b.Value);
			label_encoder_analyze_time_b.Text = msb.ToString();
			label_encoder_analyze_displ_b.Text = Util.TrimDecimals(eai.GetParam("displ",msb), 1); //mm
			label_encoder_analyze_speed_b.Text = Util.TrimDecimals(eai.GetParam("speed",msb), 2);
			label_encoder_analyze_accel_b.Text = Util.TrimDecimals(eai.GetParam("accel",msb), 2);
			label_encoder_analyze_force_b.Text = Util.TrimDecimals(eai.GetParam("force",msb), 1);
			label_encoder_analyze_power_b.Text = Util.TrimDecimals(eai.GetParam("power",msb), 1);

			encoder_analyze_instant_calculate_params();
		
			drawingarea_encoder_analyze_instant.QueueDraw(); //will fire ExposeEvent
		}
	}

	void on_button_hscale_encoder_analyze_a_pre_clicked(object o, EventArgs args) {
		hscale_encoder_analyze_a.Value -= 1;
	}
	void on_button_hscale_encoder_analyze_a_post_clicked(object o, EventArgs args) {
		hscale_encoder_analyze_a.Value += 1;
	}
	void on_button_hscale_encoder_analyze_b_pre_clicked(object o, EventArgs args) {
		hscale_encoder_analyze_b.Value -= 1;
	}
	void on_button_hscale_encoder_analyze_b_post_clicked(object o, EventArgs args) {
		hscale_encoder_analyze_b.Value += 1;
	}

	void encoder_analyze_instant_calculate_params() {
		int msa = Convert.ToInt32(hscale_encoder_analyze_a.Value);
		int msb = Convert.ToInt32(hscale_encoder_analyze_b.Value);
		bool success = eai.CalculateRangeParams(msa, msb);
		if(success) {
			label_encoder_analyze_time_diff.Text = (msb - msa).ToString();
			label_encoder_analyze_displ_diff.Text = Util.TrimDecimals(eai.GetParam("displ",msb) - eai.GetParam("displ",msa), 1);
			label_encoder_analyze_speed_diff.Text = Util.TrimDecimals(eai.GetParam("speed",msb) - eai.GetParam("speed",msa), 2);
			label_encoder_analyze_accel_diff.Text = Util.TrimDecimals(eai.GetParam("accel",msb) - eai.GetParam("accel",msa), 2);
			label_encoder_analyze_force_diff.Text = Util.TrimDecimals(eai.GetParam("force",msb) - eai.GetParam("force",msa), 1);
			label_encoder_analyze_power_diff.Text = Util.TrimDecimals(eai.GetParam("power",msb) - eai.GetParam("power",msa), 1);

			label_encoder_analyze_displ_average.Text = Util.TrimDecimals(eai.displAverageLast, 1);
			label_encoder_analyze_speed_average.Text = Util.TrimDecimals(eai.speedAverageLast, 2);
			label_encoder_analyze_accel_average.Text = Util.TrimDecimals(eai.accelAverageLast, 2);
			label_encoder_analyze_force_average.Text = Util.TrimDecimals(eai.forceAverageLast, 1);
			label_encoder_analyze_power_average.Text = Util.TrimDecimals(eai.powerAverageLast, 1);

			label_encoder_analyze_displ_max.Text = Util.TrimDecimals(eai.displMaxLast, 1);
			label_encoder_analyze_speed_max.Text = Util.TrimDecimals(eai.speedMaxLast, 2);
			label_encoder_analyze_accel_max.Text = Util.TrimDecimals(eai.accelMaxLast, 2);
			label_encoder_analyze_force_max.Text = Util.TrimDecimals(eai.forceMaxLast, 1);
			label_encoder_analyze_power_max.Text = Util.TrimDecimals(eai.powerMaxLast, 1);
		}
	}

	void on_checkbutton_encoder_analyze_b_toggled (object o, EventArgs args) {
		bool visible = checkbutton_encoder_analyze_b.Active;

		hscale_encoder_analyze_b.Visible = visible;
		hbox_buttons_scale_encoder_analyze_b.Visible = visible;
		label_encoder_analyze_time_b.Visible = visible;
		label_encoder_analyze_displ_b.Visible = visible;
		label_encoder_analyze_speed_b.Visible = visible;
		label_encoder_analyze_accel_b.Visible = visible;
		label_encoder_analyze_force_b.Visible = visible;
		label_encoder_analyze_power_b.Visible = visible;
		label_encoder_analyze_time_diff.Visible = visible;
		label_encoder_analyze_displ_diff.Visible = visible;
		label_encoder_analyze_speed_diff.Visible = visible;
		label_encoder_analyze_accel_diff.Visible = visible;
		label_encoder_analyze_force_diff.Visible = visible;
		label_encoder_analyze_power_diff.Visible = visible;
		label_encoder_analyze_displ_average.Visible = visible;
		label_encoder_analyze_speed_average.Visible = visible;
		label_encoder_analyze_accel_average.Visible = visible;
		label_encoder_analyze_force_average.Visible = visible;
		label_encoder_analyze_power_average.Visible = visible;
		label_encoder_analyze_displ_max.Visible = visible;
		label_encoder_analyze_speed_max.Visible = visible;
		label_encoder_analyze_accel_max.Visible = visible;
		label_encoder_analyze_force_max.Visible = visible;
		label_encoder_analyze_power_max.Visible = visible;
		label_encoder_analyze_diff.Visible = visible;
		label_encoder_analyze_average.Visible = visible;
		label_encoder_analyze_max.Visible = visible;
		button_encoder_analyze_AB_save.Visible = visible;

		drawingarea_encoder_analyze_instant.QueueDraw(); //will fire ExposeEvent
	}
	
	void on_button_encoder_analyze_AB_save_clicked (object o, EventArgs args) 
	{
		checkFile(Constants.CheckFileOp.ENCODER_ANALYZE_SAVE_AB);
	}

	public void on_drawingarea_encoder_analyze_instant_draw (object o, Gtk.DrawnArgs args)
	{
		if(drawingarea_encoder_analyze_cairo_pixbuf == null)
			return;

		if(eai != null)
			CairoUtil.PaintVerticalLinesAndRectangleOnSurface (
					(DrawingArea) o, args,
					eai.GetVerticalLinePosition(Convert.ToInt32(hscale_encoder_analyze_a.Value)),
					eai.GetVerticalLinePosition(Convert.ToInt32(hscale_encoder_analyze_b.Value)),
					checkbutton_encoder_analyze_b.Active,
					9, 18, // top/bottom of the rectangle
					drawingarea_encoder_analyze_cairo_pixbuf);
	}

	// -------------- end of drawingarea_encoder_analyze_instant


	private void uploadEncoderDataObjectIfPossible()
	{
		UploadEncoderDataObject uo = new UploadEncoderDataObject(encoderCaptureCurves, currentEncoderSQLSet.eccon);

		if(current_mode == Constants.Modes.POWERINERTIAL)
		{
			//discard first reps on inertial and if there are not enough reps, then do not upload
			if(! uo.InertialDiscardFirstN(preferences.encoderCaptureInertialDiscardFirstN))
				return;
		}

		uo.Calcule (preferences.GetEncoderRepetitionCriteria (current_mode));

		/*
		 * Problems on Json by accents like "Pressió sobre banc"
		 * string exerciseName = UtilGtk.ComboGetActive(combo_encoder_exercise_capture);
		 * right now fixed in json.cs UploadEncoderData()
		 */

		LogB.Information("calling Upload");
		JsonCompujump js = new JsonCompujump(configChronojump.CompujumpDjango);
		UploadEncoderDataFullObject uedfo = new UploadEncoderDataFullObject(
				-1, //uniqueID
				currentPerson.UniqueID,
				configChronojump.CompujumpStationID,
				currentEncoderSQLSet.exerciseID,
				currentEncoderSQLSet.LateralityToEnglish(),
				Util.ConvertToPoint (findMassFromGui (Constants.MassType.EXTRA)), //this is only for gravitatory
				uo);
		bool success = js.UploadEncoderData(uedfo);

		LogB.Information(js.ResultMessage);
		LogB.Information("called Upload");

		if(! success) {
			LogB.Error(js.ResultMessage);

			//since 2.1.3 do not store in Temp, if there are network errors, it is not going to be uploaded later, because wristbands can be re-assigned
			//SqliteJson.InsertTempEncoder(false, uedfo);

			bool showInWindow = false;
			if(showInWindow)
				new DialogMessage(
						"Chronojump",
						Constants.MessageTypes.WARNING,
						js.ResultMessage);
		}
	}

	//sqlite is opened on this method
	private void manageCurvesOfThisSignal()
	{
		LogB.Information ("manageCurvesOfThisSignal()");
		/*
		 * (1) if found curves of this signal
		 * 	(1a) this curves are with different eccon, or with different encoderConfiguration.name, or curves are shorter than minHeight because this values just changed on edit encoder
		 * 		(1a1) delete the curves (files)
		 * 		(1a2) delete the curves (encoder table)
		 * 		(1a3) and also delete from (encoderSignalCurves table)
		 * 	(1b) or different exercise, or different laterality or different extraWeight,
		 * 		or different encoderConfiguration (but the name is the same)
		 * 		(1b1) update curves with new data
		 * (2) update analyze labels and combos
		 */

		// get the signal
		ArrayList array = SqliteEncoder.Select(
				true, encoderSignalUniqueID, 0, 0, getEncoderGI(),
				-1, "signal", EncoderSQL.Eccons.ALL, "",
				false, true, false);

		if(array.Count == 0)
			return;

		EncoderSQL encoderSQLSet = (EncoderSQL) array[0];

		// get the curves sorted by position in set
		ArrayList data = SqliteEncoder.Select(
				true, -1, currentPerson.UniqueID, currentSession.UniqueID, getEncoderGI(),
				-1, "curve", EncoderSQL.Eccons.ALL, "",
				false, true, true);

		bool deletedUserCurves = false;
		for (int i = 0; i < data.Count; i ++) // not foreach as we want to be able to change each eSQL
		{
			EncoderSQL eSQL = (EncoderSQL) data[i];
			if (encoderSQLSet.GetDatetimeStr(false) == eSQL.GetDatetimeStr(false)) 		// (1)
			{
				// (1a)
				if (encoderSQLSet.eccon != eSQL.eccon ||
						encoderSQLSet.encoderConfiguration.name != eSQL.encoderConfiguration.name ||
						(eSQL.rangeAbs > 0 && UtilAll.DivideSafe (eSQL.rangeAbs, 10) < encoderSQLSet.minHeight) // greater than 0 because sets before 2.5.2 have absRange 0. Without these check, any selected set (previous to 2.5.2 will loose it's repetitions)
				   )

				{
					Util.FileDelete(eSQL.GetFullURL(false));					// (1a1)
					Sqlite.Delete(true, Constants.EncoderTable, Convert.ToInt32(eSQL.UniqueID));	// (1a2)
					SqliteEncoderSignalCurve.DeleteSignalCurveWithCurveID(true, Convert.ToInt32(eSQL.UniqueID)); // (1a3)
					deletedUserCurves = true;
				} else {							// (1b)
					if (eSQL.exerciseID != encoderSQLSet.exerciseID ||
							eSQL.extraWeight != encoderSQLSet.extraWeight ||
							eSQL.Laterality != encoderSQLSet.Laterality ||
							eSQL.encoderConfiguration.ToStringOutput (EncoderConfiguration.Outputs.SQL) !=
							encoderSQLSet.encoderConfiguration.ToStringOutput (EncoderConfiguration.Outputs.SQL) ||
							eSQL.minHeight != encoderSQLSet.minHeight)
					{
						eSQL.exerciseID = encoderSQLSet.exerciseID;
						eSQL.extraWeight = encoderSQLSet.extraWeight;
						eSQL.Laterality = encoderSQLSet.Laterality;
						eSQL.encoderConfiguration = encoderSQLSet.encoderConfiguration;
						eSQL.minHeight = encoderSQLSet.minHeight;

						//update on SQL
						SqliteEncoder.Update (true, eSQL);
					}
				}
			}
		}
		if(deletedUserCurves) {
			//TODO: change encSelReps and this will change labels
			updateUserCurvesLabelsAndCombo(true); 	// (2)
		}
	}

	/*
	 * on capture treeview finds which rows are related to saved SQL curves
	 * mark their rows (meaning saved)
	 * also if updateSQLRecords, then update SQL meanPower of the curve
	 *
	 * This method is called by on_feedback_closed, and finishPulsebar
	 */
	private void findAndMarkSavedCurves(bool dbconOpened, bool updateSQLRecords) 
	{
		//run this method with SQL opened to not be closing and opening a lot on the following foreachs
		if(! dbconOpened)
			Sqlite.Open();

		//find the saved curves
		ArrayList linkedCurves = SqliteEncoderSignalCurve.SelectSignalCurve(true, 
				encoderSignalUniqueID, 		//signal
				-1, -1, -1);			//curve, msStart,msEnd
		//LogB.Information("SAVED CURVES FOUND");
		//foreach(EncoderSignalCurve esc in linkedCurves)
		//	LogB.Information(esc.ToString());

		int curveCount = 0;
		double curveStart = 0;
		double curveEnd = 0;
		foreach (EncoderCurve curve in encoderCaptureCurves) 
		{
			if (currentEncoderSQLSet.eccon == "c") {
				curveStart = Convert.ToDouble(curve.Start);
				curveEnd = Convert.ToDouble(curve.Start) + Convert.ToDouble(curve.Duration);
			} else { //eccon == "ecS"
				if(Util.IsEven(curveCount)) {
					curveStart = Convert.ToDouble(curve.Start);
					curveCount ++;
					continue;
				} else
					curveEnd = Convert.ToDouble(curve.Start) + Convert.ToDouble(curve.Duration);
			}

			foreach(EncoderSignalCurve esc in linkedCurves) {
				if(curveStart <= esc.msCentral && curveEnd >= esc.msCentral)
				{
					LogB.Information(curve.Start + " is saved");
					encoderCaptureSelectBySavedCurves(esc.msCentral, true);

					if(updateSQLRecords) {
						Sqlite.Update (true, Constants.EncoderTable, "future1",
								"", Util.ConvertToPoint (curve.MeanPower),
								"uniqueID", esc.curveID.ToString());
						Sqlite.Update (true, Constants.EncoderTable, "future2",
								"", Util.ConvertToPoint (curve.MeanSpeed),
								"uniqueID", esc.curveID.ToString());
						Sqlite.Update (true, Constants.EncoderTable, "future3",
								"", Util.ConvertToPoint (curve.MeanForce),
								"uniqueID", esc.curveID.ToString());
						Sqlite.Update (true, Constants.EncoderTable, "maxPower",
								"", Util.ConvertToPoint (curve.PeakPower),
								"uniqueID", esc.curveID.ToString());
						Sqlite.Update (true, Constants.EncoderTable, "maxSpeed",
								"", Util.ConvertToPoint (curve.MaxSpeed),
								"uniqueID", esc.curveID.ToString());
						Sqlite.Update (true, Constants.EncoderTable, "maxForce",
								"", Util.ConvertToPoint (curve.MaxForce),
								"uniqueID", esc.curveID.ToString());
						Sqlite.Update (true, Constants.EncoderTable, "rangeAbs",
								"", Util.ConvertToPoint (curve.RangeAbs),
								"uniqueID", esc.curveID.ToString());
					}
					
					break;
				}
			}
			curveCount ++;
		}

		if(! dbconOpened)
			Sqlite.Close();
	}
	

	private void connectWidgetsEncoder (Gtk.Builder builder)
	{
		hbox_encoder_capture_top = (Gtk.HBox) builder.GetObject ("hbox_encoder_capture_top");
		label_button_encoder_select = (Gtk.Label) builder.GetObject ("label_button_encoder_select");
		label_encoder_exercise_mass = (Gtk.Label) builder.GetObject ("label_encoder_exercise_mass");
		hbox_encoder_exercise_mass = (Gtk.HBox) builder.GetObject ("hbox_encoder_exercise_mass");
		label_encoder_exercise_inertia = (Gtk.Label) builder.GetObject ("label_encoder_exercise_inertia");
		box_encoder_exercise_inertia = (Gtk.Box) builder.GetObject ("box_encoder_exercise_inertia");
		hbox_encoder_exercise_gravitatory_min_mov = (Gtk.HBox) builder.GetObject ("hbox_encoder_exercise_gravitatory_min_mov");
		hbox_encoder_exercise_inertial_min_mov = (Gtk.HBox) builder.GetObject ("hbox_encoder_exercise_inertial_min_mov");
		spin_encoder_capture_min_height_gravitatory = (Gtk.SpinButton) builder.GetObject ("spin_encoder_capture_min_height_gravitatory");
		spin_encoder_capture_min_height_inertial = (Gtk.SpinButton) builder.GetObject ("spin_encoder_capture_min_height_inertial");
		vbox_capture_current_encoder = (Gtk.VBox) builder.GetObject ("vbox_capture_current_encoder");

		button_encoder_select = (Gtk.Button) builder.GetObject ("button_encoder_select");
		spin_encoder_extra_weight = (Gtk.SpinButton) builder.GetObject ("spin_encoder_extra_weight");
		label_encoder_displaced_weight = (Gtk.Label) builder.GetObject ("label_encoder_displaced_weight");
		hbox_capture_1RM = (Gtk.HBox) builder.GetObject ("hbox_capture_1RM");
		label_encoder_1RM_percent = (Gtk.Label) builder.GetObject ("label_encoder_1RM_percent");
		label_encoder_im_total = (Gtk.Label) builder.GetObject ("label_encoder_im_total");
		label_encoder_equivalent_mass = (Gtk.Label) builder.GetObject ("label_encoder_equivalent_mass");
		spin_encoder_im_weights_n = (Gtk.SpinButton) builder.GetObject ("spin_encoder_im_weights_n");
		hbox_combo_encoder_anchorage = (Gtk.HBox) builder.GetObject ("hbox_combo_encoder_anchorage");

		label_encoder_selected = (Gtk.Label) builder.GetObject ("label_encoder_selected");	
		image_encoder_top_selected_type = (Gtk.Image) builder.GetObject ("image_encoder_top_selected_type");
		image_encoder_selected_type = (Gtk.Image) builder.GetObject ("image_encoder_selected_type");

		notebook_encoder_top = (Gtk.Notebook) builder.GetObject ("notebook_encoder_top");
		notebook_hpaned_encoder_or_exercise_config = (Gtk.Notebook) builder.GetObject ("notebook_hpaned_encoder_or_exercise_config");
		label_encoder_top_selected = (Gtk.Label) builder.GetObject ("label_encoder_top_selected");
		label_encoder_top_exercise = (Gtk.Label) builder.GetObject ("label_encoder_top_exercise");
		label_encoder_top_extra_mass = (Gtk.Label) builder.GetObject ("label_encoder_top_extra_mass");
		label_encoder_top_1RM_percent = (Gtk.Label) builder.GetObject ("label_encoder_top_1RM_percent");
		label_encoder_top_weights = (Gtk.Label) builder.GetObject ("label_encoder_top_weights");
		label_encoder_top_im = (Gtk.Label) builder.GetObject ("label_encoder_top_im");

		//this is kg*cm^2 because there's limitation of Glade on 3 decimals. 
		//at SQL it's in kg*cm^2 also because it's stored as int
		//at graph.R is converted to kg*m^2 ( /10000 )
		//spin_encoder_capture_inertial = (Gtk.SpinButton) builder.GetObject ("spin_encoder_capture_inertial"); 

		hbox_encoder_configuration = (Gtk.Box) builder.GetObject ("hbox_encoder_configuration");
		frame_encoder_capture_options = (Gtk.Frame) builder.GetObject ("frame_encoder_capture_options");
		hbox_encoder_capture_actions = (Gtk.HBox) builder.GetObject ("hbox_encoder_capture_actions");
		vbox_inertial_instructions = (Gtk.VBox) builder.GetObject ("vbox_inertial_instructions");

		hbox_encoder_capture_wait = (Gtk.Box) builder.GetObject ("hbox_encoder_capture_wait");
		box_encoder_capture_doing = (Gtk.Box) builder.GetObject ("box_encoder_capture_doing");
		vscale_encoder_capture_inertial_angle_now = (Gtk.VScale) builder.GetObject ("vscale_encoder_capture_inertial_angle_now");
		vbox_angle_now = (Gtk.VBox) builder.GetObject ("vbox_angle_now");
		label_encoder_capture_inertial_angle_now = (Gtk.Label) builder.GetObject ("label_encoder_capture_inertial_angle_now");

		button_encoder_capture = (Gtk.Button) builder.GetObject ("button_encoder_capture");

		box_encoder_capture_csharp_r_both = (Gtk.Box) builder.GetObject ("box_encoder_capture_csharp_r_both");
		radio_encoder_capture_csharp = (Gtk.RadioButton) builder.GetObject ("radio_encoder_capture_csharp");
		radio_encoder_capture_r = (Gtk.RadioButton) builder.GetObject ("radio_encoder_capture_r");
		radio_encoder_capture_both = (Gtk.RadioButton) builder.GetObject ("radio_encoder_capture_both");

		//encoder calibrate/recalibrate widgets
		button_encoder_inertial_calibrate = (Gtk.Button) builder.GetObject ("button_encoder_inertial_calibrate");
		button_encoder_inertial_recalibrate = (Gtk.Button) builder.GetObject ("button_encoder_inertial_recalibrate");
		label_calibrate_output_message = (Gtk.Label) builder.GetObject ("label_calibrate_output_message");
		button_encoder_inertial_calibrate_close = (Gtk.Button) builder.GetObject ("button_encoder_inertial_calibrate_close");
		label_wait = (Gtk.Label) builder.GetObject ("label_wait");


		image_encoder_bell = (Gtk.Image) builder.GetObject ("image_encoder_bell");
		button_encoder_capture_cancel = (Gtk.Button) builder.GetObject ("button_encoder_capture_cancel");
		button_encoder_capture_finish = (Gtk.Button) builder.GetObject ("button_encoder_capture_finish");
		button_encoder_capture_finish_cont = (Gtk.Button) builder.GetObject ("button_encoder_capture_finish_cont");
		button_encoder_bells = (Gtk.Button) builder.GetObject ("button_encoder_bells");
		button_encoder_load_signal_at_analyze = (Gtk.Button) builder.GetObject ("button_encoder_load_signal_at_analyze");
		encoder_pulsebar_capture = (Gtk.ProgressBar) builder.GetObject ("encoder_pulsebar_capture");
		encoder_countdown_label = (Gtk.Label) builder.GetObject ("encoder_countdown_label");
		box_encoder_capture_rhythm = (Gtk.Box) builder.GetObject ("box_encoder_capture_rhythm");
		box_encoder_capture_rhythm_doing = (Gtk.Box) builder.GetObject ("box_encoder_capture_rhythm_doing");
		box_encoder_capture_rhythm_rest = (Gtk.Box) builder.GetObject ("box_encoder_capture_rhythm_rest");
		encoder_pulsebar_rhythm_eccon = (Gtk.ProgressBar) builder.GetObject ("encoder_pulsebar_rhythm_eccon");
		label_encoder_rhythm_rest = (Gtk.Label) builder.GetObject ("label_encoder_rhythm_rest");
		label_rhythm = (Gtk.Label) builder.GetObject ("label_rhythm");
		label_rhythm_rep = (Gtk.Label) builder.GetObject ("label_rhythm_rep");
		vbox_capturing_with_triggers = (Gtk.VBox) builder.GetObject ("vbox_capturing_with_triggers");
		button_encoder_export_signal = (Gtk.Button) builder.GetObject ("button_encoder_export_signal");
		//	button_menu_encoder_export_set = (Gtk.Button) builder.GetObject ("button_menu_encoder_export_set");

		button_encoder_devices_networks = (Gtk.Button) builder.GetObject ("button_encoder_devices_networks");
		//button_encoder_devices_networks_problems = (Gtk.Button) builder.GetObject ("button_encoder_devices_networks_problems");

		//encoder capture tab view options
		hbox_encoder_show_signal_table = (Gtk.HBox) builder.GetObject ("hbox_encoder_show_signal_table");
		check_encoder_capture_table = (Gtk.CheckButton) builder.GetObject ("check_encoder_capture_table");
		check_encoder_capture_signal = (Gtk.CheckButton) builder.GetObject ("check_encoder_capture_signal");
		vbox_encoder_bars_table_and_save_reps = (Gtk.VBox) builder.GetObject ("vbox_encoder_bars_table_and_save_reps");
		alignment_encoder_capture_curves_bars_drawingarea = (Gtk.Alignment) builder.GetObject ("alignment_encoder_capture_curves_bars_drawingarea");

		hbox_combo_encoder_exercise_capture = (Gtk.Box) builder.GetObject ("hbox_combo_encoder_exercise_capture");
		radio_encoder_eccon_concentric = (Gtk.RadioButton) builder.GetObject ("radio_encoder_eccon_concentric");
		radio_encoder_eccon_eccentric_concentric = (Gtk.RadioButton) builder.GetObject ("radio_encoder_eccon_eccentric_concentric");
		radio_encoder_laterality_both = (Gtk.RadioButton) builder.GetObject ("radio_encoder_laterality_both");
		radio_encoder_laterality_r = (Gtk.RadioButton) builder.GetObject ("radio_encoder_laterality_r");
		radio_encoder_laterality_l = (Gtk.RadioButton) builder.GetObject ("radio_encoder_laterality_l");

		//exercise edit/add
		hbox_encoder_exercise_close_and = (Gtk.HBox) builder.GetObject ("hbox_encoder_exercise_close_and");
		hbox_encoder_exercise_select = (Gtk.HBox) builder.GetObject ("hbox_encoder_exercise_select");
		hbox_encoder_exercise_actions = (Gtk.HBox) builder.GetObject ("hbox_encoder_exercise_actions");
		button_encoder_exercise_actions_edit_do = (Gtk.Button) builder.GetObject ("button_encoder_exercise_actions_edit_do");
		button_encoder_exercise_actions_add_do = (Gtk.Button) builder.GetObject ("button_encoder_exercise_actions_add_do");
		notebook_encoder_exercise = (Gtk.Notebook) builder.GetObject ("notebook_encoder_exercise");
		entry_encoder_exercise_name = (Gtk.Entry) builder.GetObject ("entry_encoder_exercise_name");
		radio_encoder_exercise_gravitatory = (Gtk.RadioButton) builder.GetObject ("radio_encoder_exercise_gravitatory");
		radio_encoder_exercise_inertial = (Gtk.RadioButton) builder.GetObject ("radio_encoder_exercise_inertial");
		radio_encoder_exercise_all = (Gtk.RadioButton) builder.GetObject ("radio_encoder_exercise_all");
		button_radio_encoder_exercise_help = (Gtk.Button) builder.GetObject ("button_radio_encoder_exercise_help");
		spin_encoder_exercise_displaced_body_weight = (Gtk.SpinButton) builder.GetObject ("spin_encoder_exercise_displaced_body_weight");
		spin_encoder_exercise_speed_1rm = (Gtk.SpinButton) builder.GetObject ("spin_encoder_exercise_speed_1rm");
		hbox_encoder_exercise_speed_1rm = (Gtk.HBox) builder.GetObject ("hbox_encoder_exercise_speed_1rm");
		entry_encoder_exercise_resistance = (Gtk.Entry) builder.GetObject ("entry_encoder_exercise_resistance");
		entry_encoder_exercise_description = (Gtk.Entry) builder.GetObject ("entry_encoder_exercise_description");

		/*
		//used on guiTests
		button_encoder_capture_curves_all = (Gtk.Button) builder.GetObject ("button_encoder_capture_curves_all");
		button_encoder_capture_curves_best = (Gtk.Button) builder.GetObject ("button_encoder_capture_curves_best");
		button_encoder_capture_curves_none = (Gtk.Button) builder.GetObject ("button_encoder_capture_curves_none");
		button_encoder_capture_curves_4top = (Gtk.Button) builder.GetObject ("button_encoder_capture_curves_4top");
		*/
		button_encoder_capture_image_save = (Gtk.Button) builder.GetObject ("button_encoder_capture_image_save");

		notebook_analyze_results = (Gtk.Notebook) builder.GetObject ("notebook_analyze_results");
		hbox_combo_encoder_exercise_analyze = (Gtk.Box) builder.GetObject ("hbox_combo_encoder_exercise_analyze");
		hbox_combo_encoder_laterality_analyze = (Gtk.HBox) builder.GetObject ("hbox_combo_encoder_laterality_analyze");

		hbox_combo_encoder_analyze_cross_sup = (Gtk.Box) builder.GetObject ("hbox_combo_encoder_analyze_cross_sup"); //includes "Profile" label and the hbox
		hbox_combo_encoder_analyze_cross = (Gtk.Box) builder.GetObject ("hbox_combo_encoder_analyze_cross");
		hbox_combo_encoder_analyze_1RM = (Gtk.Box) builder.GetObject ("hbox_combo_encoder_analyze_1RM");

		hbox_encoder_analyze_show_powerbars = (Gtk.Box) builder.GetObject ("hbox_encoder_analyze_show_powerbars");
		check_encoder_analyze_show_impulse = (Gtk.CheckButton) builder.GetObject ("check_encoder_analyze_show_impulse");
		check_encoder_analyze_show_time_to_peak_power = (Gtk.CheckButton) builder.GetObject ("check_encoder_analyze_show_time_to_peak_power");
		check_encoder_analyze_show_range = (Gtk.CheckButton) builder.GetObject ("check_encoder_analyze_show_range");

		hbox_encoder_analyze_individual_groupwise = (Gtk.HBox) builder.GetObject ("hbox_encoder_analyze_individual_groupwise");
		hbox_encoder_analyze_instantaneous = (Gtk.HBox) builder.GetObject ("hbox_encoder_analyze_instantaneous");
		check_encoder_analyze_show_position = (Gtk.CheckButton) builder.GetObject ("check_encoder_analyze_show_position");
		check_encoder_analyze_show_speed = (Gtk.CheckButton) builder.GetObject ("check_encoder_analyze_show_speed");
		check_encoder_analyze_show_accel = (Gtk.CheckButton) builder.GetObject ("check_encoder_analyze_show_accel");
		check_encoder_analyze_show_force = (Gtk.CheckButton) builder.GetObject ("check_encoder_analyze_show_force");
		check_encoder_analyze_show_power = (Gtk.CheckButton) builder.GetObject ("check_encoder_analyze_show_power");
		checkbutton_encoder_analyze_side_share_x = (Gtk.CheckButton) builder.GetObject ("checkbutton_encoder_analyze_side_share_x");

		frame_encoder_analyze_options = (Gtk.Frame) builder.GetObject ("frame_encoder_analyze_options");
		grid_encoder_analyze_options = (Gtk.Grid) builder.GetObject ("grid_encoder_analyze_options");
		image_encoder_analyze_show_SAFE_position = (Gtk.Image) builder.GetObject ("image_encoder_analyze_show_SAFE_position");
		image_encoder_analyze_show_SAFE_speed = (Gtk.Image) builder.GetObject ("image_encoder_analyze_show_SAFE_speed");
		image_encoder_analyze_show_SAFE_accel = (Gtk.Image) builder.GetObject ("image_encoder_analyze_show_SAFE_accel");
		image_encoder_analyze_show_SAFE_force = (Gtk.Image) builder.GetObject ("image_encoder_analyze_show_SAFE_force");
		image_encoder_analyze_show_SAFE_power = (Gtk.Image) builder.GetObject ("image_encoder_analyze_show_SAFE_power");

		checkbutton_crossvalidate = (Gtk.CheckButton) builder.GetObject ("checkbutton_crossvalidate");
		button_encoder_analyze = (Gtk.Button) builder.GetObject ("button_encoder_analyze");
		button_encoder_analyze_mode_options_close_and_analyze = (Gtk.Button) builder.GetObject ("button_encoder_analyze_mode_options_close_and_analyze");
		hbox_encoder_analyze_progress = (Gtk.Box) builder.GetObject ("hbox_encoder_analyze_progress");
		button_encoder_analyze_cancel = (Gtk.Button) builder.GetObject ("button_encoder_analyze_cancel");
		button_encoder_analyze_data_select_curves = (Gtk.Button) builder.GetObject ("button_encoder_analyze_data_select_curves");
		label_encoder_user_curves_active_num = (Gtk.Label) builder.GetObject ("label_encoder_user_curves_active_num");
		label_encoder_user_curves_all_num = (Gtk.Label) builder.GetObject ("label_encoder_user_curves_all_num");

		vbox_encoder_analyze_instant = (Gtk.VBox) builder.GetObject ("vbox_encoder_analyze_instant");
		grid_encoder_analyze_instant = (Gtk.Grid) builder.GetObject ("grid_encoder_analyze_instant");
		grid_encoder_analyze_instant_box_hscale_a = (Gtk.Box) builder.GetObject ("grid_encoder_analyze_instant_box_hscale_a");
		grid_encoder_analyze_instant_box_hscale_b = (Gtk.Box) builder.GetObject ("grid_encoder_analyze_instant_box_hscale_b");
		grid_encoder_analyze_instant_box_hscale_a.Hexpand = true;
		grid_encoder_analyze_instant_box_hscale_b.Hexpand = true;
		hscale_encoder_analyze_a = (Gtk.HScale) builder.GetObject ("hscale_encoder_analyze_a");
		checkbutton_encoder_analyze_b = (Gtk.CheckButton) builder.GetObject ("checkbutton_encoder_analyze_b");
		hscale_encoder_analyze_b = (Gtk.HScale) builder.GetObject ("hscale_encoder_analyze_b");
		hbox_buttons_scale_encoder_analyze_b = (Gtk.HBox) builder.GetObject ("hbox_buttons_scale_encoder_analyze_b");
		label_encoder_analyze_time_a = (Gtk.Label) builder.GetObject ("label_encoder_analyze_time_a");
		label_encoder_analyze_displ_a = (Gtk.Label) builder.GetObject ("label_encoder_analyze_displ_a");
		label_encoder_analyze_speed_a = (Gtk.Label) builder.GetObject ("label_encoder_analyze_speed_a");
		label_encoder_analyze_accel_a = (Gtk.Label) builder.GetObject ("label_encoder_analyze_accel_a");
		label_encoder_analyze_force_a = (Gtk.Label) builder.GetObject ("label_encoder_analyze_force_a");
		label_encoder_analyze_power_a = (Gtk.Label) builder.GetObject ("label_encoder_analyze_power_a");
		label_encoder_analyze_time_b = (Gtk.Label) builder.GetObject ("label_encoder_analyze_time_b");
		label_encoder_analyze_displ_b = (Gtk.Label) builder.GetObject ("label_encoder_analyze_displ_b");
		label_encoder_analyze_speed_b = (Gtk.Label) builder.GetObject ("label_encoder_analyze_speed_b");
		label_encoder_analyze_accel_b = (Gtk.Label) builder.GetObject ("label_encoder_analyze_accel_b");
		label_encoder_analyze_force_b = (Gtk.Label) builder.GetObject ("label_encoder_analyze_force_b");
		label_encoder_analyze_power_b = (Gtk.Label) builder.GetObject ("label_encoder_analyze_power_b");
		label_encoder_analyze_time_diff = (Gtk.Label) builder.GetObject ("label_encoder_analyze_time_diff");
		label_encoder_analyze_displ_diff = (Gtk.Label) builder.GetObject ("label_encoder_analyze_displ_diff");
		label_encoder_analyze_speed_diff = (Gtk.Label) builder.GetObject ("label_encoder_analyze_speed_diff");
		label_encoder_analyze_accel_diff = (Gtk.Label) builder.GetObject ("label_encoder_analyze_accel_diff");
		label_encoder_analyze_force_diff = (Gtk.Label) builder.GetObject ("label_encoder_analyze_force_diff");
		label_encoder_analyze_power_diff = (Gtk.Label) builder.GetObject ("label_encoder_analyze_power_diff");
		label_encoder_analyze_displ_average = (Gtk.Label) builder.GetObject ("label_encoder_analyze_displ_average");
		label_encoder_analyze_speed_average = (Gtk.Label) builder.GetObject ("label_encoder_analyze_speed_average");
		label_encoder_analyze_accel_average = (Gtk.Label) builder.GetObject ("label_encoder_analyze_accel_average");
		label_encoder_analyze_force_average = (Gtk.Label) builder.GetObject ("label_encoder_analyze_force_average");
		label_encoder_analyze_power_average = (Gtk.Label) builder.GetObject ("label_encoder_analyze_power_average");
		label_encoder_analyze_displ_max = (Gtk.Label) builder.GetObject ("label_encoder_analyze_displ_max");
		label_encoder_analyze_speed_max = (Gtk.Label) builder.GetObject ("label_encoder_analyze_speed_max");
		label_encoder_analyze_accel_max = (Gtk.Label) builder.GetObject ("label_encoder_analyze_accel_max");
		label_encoder_analyze_force_max = (Gtk.Label) builder.GetObject ("label_encoder_analyze_force_max");
		label_encoder_analyze_power_max = (Gtk.Label) builder.GetObject ("label_encoder_analyze_power_max");
		label_encoder_analyze_diff = (Gtk.Label) builder.GetObject ("label_encoder_analyze_diff");
		label_encoder_analyze_average = (Gtk.Label) builder.GetObject ("label_encoder_analyze_average");
		label_encoder_analyze_max = (Gtk.Label) builder.GetObject ("label_encoder_analyze_max");
		button_encoder_analyze_AB_save = (Gtk.Button) builder.GetObject ("button_encoder_analyze_AB_save");

		button_encoder_analyze_image_save = (Gtk.Button) builder.GetObject ("button_encoder_analyze_image_save");
		button_encoder_analyze_table_save = (Gtk.Button) builder.GetObject ("button_encoder_analyze_table_save");
		button_encoder_analyze_1RM_save = (Gtk.Button) builder.GetObject ("button_encoder_analyze_1RM_save");

		radio_encoder_analyze_individual_current_set = (Gtk.RadioButton) builder.GetObject ("radio_encoder_analyze_individual_current_set");
		radio_encoder_analyze_individual_current_session = (Gtk.RadioButton) builder.GetObject ("radio_encoder_analyze_individual_current_session");
		radio_encoder_analyze_individual_all_sessions = (Gtk.RadioButton) builder.GetObject ("radio_encoder_analyze_individual_all_sessions");
		radio_encoder_analyze_groupal_current_session = (Gtk.RadioButton) builder.GetObject ("radio_encoder_analyze_groupal_current_session");

		image_encoder_analyze_individual_current_set = (Gtk.Image) builder.GetObject ("image_encoder_analyze_individual_current_set");
		image_encoder_analyze_individual_current_session = (Gtk.Image) builder.GetObject ("image_encoder_analyze_individual_current_session");
		image_encoder_analyze_individual_all_sessions = (Gtk.Image) builder.GetObject ("image_encoder_analyze_individual_all_sessions");
		image_encoder_analyze_groupal_current_session = (Gtk.Image) builder.GetObject ("image_encoder_analyze_groupal_current_session");

		hbox_encoder_analyze_current_signal = (Gtk.HBox) builder.GetObject ("hbox_encoder_analyze_current_signal");

		radiobutton_encoder_analyze_powerbars = (Gtk.RadioButton) builder.GetObject ("radiobutton_encoder_analyze_powerbars");
		radiobutton_encoder_analyze_cross = (Gtk.RadioButton) builder.GetObject ("radiobutton_encoder_analyze_cross");
		radiobutton_encoder_analyze_1RM = (Gtk.RadioButton) builder.GetObject ("radiobutton_encoder_analyze_1RM");
		radiobutton_encoder_analyze_instantaneous = (Gtk.RadioButton) builder.GetObject ("radiobutton_encoder_analyze_instantaneous");
		radiobutton_encoder_analyze_single = (Gtk.RadioButton) builder.GetObject ("radiobutton_encoder_analyze_single");
		radiobutton_encoder_analyze_side = (Gtk.RadioButton) builder.GetObject ("radiobutton_encoder_analyze_side");
		radiobutton_encoder_analyze_superpose = (Gtk.RadioButton) builder.GetObject ("radiobutton_encoder_analyze_superpose");
		radiobutton_encoder_analyze_all_set = (Gtk.RadioButton) builder.GetObject ("radiobutton_encoder_analyze_all_set");
		radiobutton_encoder_analyze_neuromuscular_profile = (Gtk.RadioButton) builder.GetObject ("radiobutton_encoder_analyze_neuromuscular_profile");
		image_encoder_analyze_powerbars = (Gtk.Image) builder.GetObject ("image_encoder_analyze_powerbars");
		image_encoder_analyze_cross = (Gtk.Image) builder.GetObject ("image_encoder_analyze_cross");
		image_encoder_analyze_1RM = (Gtk.Image) builder.GetObject ("image_encoder_analyze_1RM");
		image_encoder_analyze_instantaneous = (Gtk.Image) builder.GetObject ("image_encoder_analyze_instantaneous");
		image_encoder_analyze_single = (Gtk.Image) builder.GetObject ("image_encoder_analyze_single");
		image_encoder_analyze_side = (Gtk.Image) builder.GetObject ("image_encoder_analyze_side");
		image_encoder_analyze_superpose = (Gtk.Image) builder.GetObject ("image_encoder_analyze_superpose");
		image_encoder_analyze_all_set = (Gtk.Image) builder.GetObject ("image_encoder_analyze_all_set");
		image_encoder_analyze_nmp = (Gtk.Image) builder.GetObject ("image_encoder_analyze_nmp");
		image_encoder_analyze_selected_single = (Gtk.Image) builder.GetObject ("image_encoder_analyze_selected_single");
		image_encoder_analyze_selected_side = (Gtk.Image) builder.GetObject ("image_encoder_analyze_selected_side");
		image_encoder_analyze_selected_superpose = (Gtk.Image) builder.GetObject ("image_encoder_analyze_selected_superpose");
		image_encoder_analyze_selected_all_set = (Gtk.Image) builder.GetObject ("image_encoder_analyze_selected_all_set");
		label_encoder_analyze_selected = (Gtk.Label) builder.GetObject ("label_encoder_analyze_selected");
		hbox_encoder_analyze_intersession = (Gtk.HBox) builder.GetObject ("hbox_encoder_analyze_intersession");
		check_encoder_intersession_x_is_date = (Gtk.CheckButton) builder.GetObject ("check_encoder_intersession_x_is_date");
		check_encoder_separate_session_in_days = (Gtk.CheckButton) builder.GetObject ("check_encoder_separate_session_in_days");
		hbox_combo_encoder_analyze_weights = (Gtk.HBox) builder.GetObject ("hbox_combo_encoder_analyze_weights");

		button_encoder_analyze_neuromuscular_help = (Gtk.Button) builder.GetObject ("button_encoder_analyze_neuromuscular_help");


		check_encoder_analyze_eccon_together = (Gtk.CheckButton) builder.GetObject ("check_encoder_analyze_eccon_together");
		image_encoder_analyze_eccon_together = (Gtk.Image) builder.GetObject ("image_encoder_analyze_eccon_together");
		image_encoder_analyze_eccon_separated = (Gtk.Image) builder.GetObject ("image_encoder_analyze_eccon_separated");

		image_encoder_analyze_position = (Gtk.Image) builder.GetObject ("image_encoder_analyze_position");
		image_encoder_analyze_speed = (Gtk.Image) builder.GetObject ("image_encoder_analyze_speed");
		image_encoder_analyze_accel = (Gtk.Image) builder.GetObject ("image_encoder_analyze_accel");
		image_encoder_analyze_force = (Gtk.Image) builder.GetObject ("image_encoder_analyze_force");
		image_encoder_analyze_power = (Gtk.Image) builder.GetObject ("image_encoder_analyze_power");

		hbox_encoder_analyze_mean = (Gtk.HBox) builder.GetObject ("hbox_encoder_analyze_mean");
		hbox_encoder_analyze_max = (Gtk.HBox) builder.GetObject ("hbox_encoder_analyze_max");
		image_encoder_analyze_mean = (Gtk.Image) builder.GetObject ("image_encoder_analyze_mean");
		image_encoder_analyze_max = (Gtk.Image) builder.GetObject ("image_encoder_analyze_max");
		image_encoder_analyze_range = (Gtk.Image) builder.GetObject ("image_encoder_analyze_range");
		image_encoder_analyze_time_to_pp = (Gtk.Image) builder.GetObject ("image_encoder_analyze_time_to_pp");

		hbox_encoder_analyze_curve_num = (Gtk.Box) builder.GetObject ("hbox_encoder_analyze_curve_num");
		hbox_combo_encoder_analyze_curve_num_combo = (Gtk.Box) builder.GetObject ("hbox_combo_encoder_analyze_curve_num_combo");
		label_encoder_analyze_side_max = (Gtk.Label) builder.GetObject ("label_encoder_analyze_side_max");

		check_encoder_analyze_mean_or_max = (Gtk.CheckButton) builder.GetObject ("check_encoder_analyze_mean_or_max");

		scrolledwindow_image_encoder_analyze = (Gtk.ScrolledWindow) builder.GetObject ("scrolledwindow_image_encoder_analyze");
		//	viewport_image_encoder_analyze = (Gtk.Viewport) builder.GetObject ("viewport_image_encoder_analyze");
		notebook_encoder_analyze = (Gtk.Notebook) builder.GetObject ("notebook_encoder_analyze");
		image_encoder_analyze = (Gtk.Image) builder.GetObject ("image_encoder_analyze");
		encoder_pulsebar_analyze = (Gtk.ProgressBar) builder.GetObject ("encoder_pulsebar_analyze");
		box_set_loading = (Gtk.Box) builder.GetObject ("box_set_loading");
		spinner_set_loading = (Gtk.Spinner) builder.GetObject ("spinner_set_loading");
		label_set_loading = (Gtk.Label) builder.GetObject ("label_set_loading");
		encoder_pulsebar_load_signal_at_analyze = (Gtk.ProgressBar) builder.GetObject ("encoder_pulsebar_load_signal_at_analyze");
		label_encoder_load_signal_at_analyze = (Gtk.Label) builder.GetObject ("label_encoder_load_signal_at_analyze");

		alignment_treeview_encoder_capture_curves = (Gtk.Alignment) builder.GetObject ("alignment_treeview_encoder_capture_curves");
		hpaned_encoder_capture_current = (Gtk.Paned) builder.GetObject ("hpaned_encoder_capture_current");
		treeview_encoder_capture_curves = (Gtk.TreeView) builder.GetObject ("treeview_encoder_capture_curves");
		treeview_encoder_analyze_curves = (Gtk.TreeView) builder.GetObject ("treeview_encoder_analyze_curves");

		encoder_capture_signal_drawingarea_cairo = (Gtk.DrawingArea) builder.GetObject ("encoder_capture_signal_drawingarea_cairo");
		encoder_capture_curves_bars_drawingarea_cairo = (Gtk.DrawingArea) builder.GetObject ("encoder_capture_curves_bars_drawingarea_cairo");
		drawingarea_encoder_analyze_instant = (Gtk.DrawingArea) builder.GetObject ("drawingarea_encoder_analyze_instant");
	}
}	

public class EncoderCaptureDisplay : BooleansInt
{
//	private int selection;

	//constructor when we have the 0-7 value
	public EncoderCaptureDisplay(int selection)
	{
		this.i = selection;
	}

	//constructor with the 3 booleans
	public EncoderCaptureDisplay(bool showBit1, bool showBit2, bool showBit3)
	{
		this.i = 0;
		if(showBit1)
			i ++;
		if(showBit2)
			i += 2;
		if(showBit3)
			i += 4;
	}

	public bool ShowBars
	{
		get { return Bit3; }
	}

	public bool ShowTable
	{
		get { return Bit2; }
	}

	public bool ShowSignal
	{
		get { return Bit1; }
	}

	//just to debug
	public override string ToString()
	{
		return string.Format("selected: {0} (ShowBars: {1}, ShowTable: {2}, ShowSignal: {3})",
				i, ShowBars, ShowTable, ShowSignal);
	}
}
