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
using System.IO; 
using Gtk;
using Gdk;
using Glade;
using System.Collections;
using System.Collections.Generic; //List<T>
using System.Threading;
using Mono.Unix;
using System.Diagnostics; 	//for detect OS and for Process
using LongoMatch.Gui;


public partial class ChronoJumpWindow 
{

	[Widget] Gtk.HBox hbox_encoder_capture_top;
	[Widget] Gtk.Label label_encoder_exercise_mass;
	[Widget] Gtk.VBox vbox_encoder_exercise_mass;
	[Widget] Gtk.Label label_encoder_exercise_inertia;
	[Widget] Gtk.VBox vbox_encoder_exercise_inertia;

	[Widget] Gtk.Button button_encoder_select;
	[Widget] Gtk.SpinButton spin_encoder_extra_weight;
	[Widget] Gtk.Label label_encoder_displaced_weight;
	[Widget] Gtk.HBox hbox_capture_1RM;
	[Widget] Gtk.Label label_encoder_1RM_percent;
	[Widget] Gtk.Label label_encoder_im_total;
	[Widget] Gtk.SpinButton spin_encoder_im_weights_n;
	[Widget] Gtk.Entry entry_encoder_im_weights_n;
	[Widget] Gtk.HBox hbox_combo_encoder_anchorage;
	[Widget] Gtk.ComboBox combo_encoder_anchorage;

	[Widget] Gtk.Label label_encoder_selected;	
	[Widget] Gtk.Image image_encoder_top_selected_type;
	[Widget] Gtk.Image image_encoder_selected_type;

	[Widget] Gtk.Notebook notebook_encoder_top;
	[Widget] Gtk.Label label_encoder_top_selected;
	[Widget] Gtk.Label label_encoder_top_exercise;
	[Widget] Gtk.Label label_encoder_top_extra_mass;
	[Widget] Gtk.Label label_encoder_top_1RM_percent;
	[Widget] Gtk.Label label_encoder_top_weights;
	[Widget] Gtk.Label label_encoder_top_im;

	//this is Kg*cm^2 because there's limitation of Glade on 3 decimals. 
	//at SQL it's in Kg*cm^2 also because it's stored as int
	//at graph.R is converted to Kg*m^2 ( /10000 )
	//[Widget] Gtk.SpinButton spin_encoder_capture_inertial; 
	
	[Widget] Gtk.Box hbox_encoder_sup_capture_analyze;
	[Widget] Gtk.Box hbox_encoder_sup_capture_analyze_two_buttons;
	[Widget] Gtk.Box hbox_encoder_configuration;
	[Widget] Gtk.Frame frame_encoder_capture_options;
	
	[Widget] Gtk.Box hbox_encoder_capture_wait;
	[Widget] Gtk.Box vbox_encoder_capture_doing;
	[Widget] Gtk.VScale vscale_encoder_capture_inertial_angle_now;
	[Widget] Gtk.VBox vbox_angle_now;
	[Widget] Gtk.Label label_encoder_capture_inertial_angle_now;
	
	[Widget] Gtk.RadioButton radio_encoder_capture_1set;
	[Widget] Gtk.RadioButton radio_encoder_capture_cont;
	[Widget] Gtk.Button button_encoder_capture;
	[Widget] Gtk.Button button_encoder_inertial_calibrate;
	[Widget] Gtk.Label label_wait;

	
	[Widget] Gtk.Image image_encoder_bell;
	[Widget] Gtk.Button button_encoder_capture_cancel;
	[Widget] Gtk.Button button_encoder_capture_finish;
	[Widget] Gtk.Button button_encoder_capture_finish_cont;
	[Widget] Gtk.Button button_encoder_recalculate;
	[Widget] Gtk.Button button_encoder_load_signal;
	[Widget] Gtk.Button button_encoder_load_signal_on_analyze;
	[Widget] Gtk.Viewport viewport_image_encoder_capture;
	[Widget] Gtk.Image image_encoder_capture;
	[Widget] Gtk.ProgressBar encoder_pulsebar_capture;
	[Widget] Gtk.VBox vbox_encoder_signal_comment;
	[Widget] Gtk.TextView textview_encoder_signal_comment;
	[Widget] Gtk.Button button_encoder_signal_save_comment;
	[Widget] Gtk.MenuItem menuitem_export_encoder_signal;
	[Widget] Gtk.Button button_encoder_delete_signal;
	
	[Widget] Gtk.VPaned vpaned_encoder_main;
	[Widget] Gtk.VPaned vpaned_encoder_capture_video_and_set_graph;
		
	//encoder video
	[Widget] Gtk.Alignment alignment_video_encoder;
	[Widget] Gtk.Notebook notebook_video_encoder;
	[Widget] Gtk.Viewport viewport_video_capture_encoder;
	[Widget] Gtk.Viewport viewport_video_play_encoder;
	[Widget] Gtk.RadioButton radiobutton_video_encoder_capture;
	[Widget] Gtk.RadioButton radiobutton_video_encoder_play;
	[Widget] Gtk.RadioButton radiobutton_video_encoder_options;
	[Widget] Gtk.Label label_video_encoder_filename;
	[Widget] Gtk.TextView textview_video_encoder_folder;
	[Widget] Gtk.Button button_video_encoder_open_folder;
	[Widget] Gtk.Label label_video_feedback_encoder;
	[Widget] Gtk.CheckButton checkbutton_video_encoder;
	
	[Widget] Gtk.Notebook notebook_encoder_sup;
	[Widget] Gtk.Notebook notebook_encoder_capture;
	
	[Widget] Gtk.Box hbox_combo_encoder_exercise_capture;
	[Widget] Gtk.ComboBox combo_encoder_exercise_capture;
	[Widget] Gtk.RadioButton radio_encoder_eccon_concentric;
	[Widget] Gtk.RadioButton radio_encoder_eccon_eccentric_concentric;
	[Widget] Gtk.RadioButton radio_encoder_laterality_both;
	[Widget] Gtk.RadioButton radio_encoder_laterality_r;
	[Widget] Gtk.RadioButton radio_encoder_laterality_l;
	[Widget] Gtk.Box hbox_encoder_capture_curves_save_all_none;

	[Widget] Gtk.Button button_encoder_capture_curves_all;
	[Widget] Gtk.Button button_encoder_capture_curves_best;
	[Widget] Gtk.Button button_encoder_capture_curves_none;
	[Widget] Gtk.Button button_encoder_capture_curves_4top;
	
	[Widget] Gtk.Notebook notebook_analyze_results;
	[Widget] Gtk.Box hbox_combo_encoder_exercise_analyze;
	[Widget] Gtk.ComboBox combo_encoder_exercise_analyze;

	[Widget] Gtk.Box hbox_combo_encoder_analyze_cross;
	[Widget] Gtk.ComboBox combo_encoder_analyze_cross;
	
	[Widget] Gtk.Box hbox_combo_encoder_analyze_1RM;
	[Widget] Gtk.ComboBox combo_encoder_analyze_1RM;
	
	[Widget] Gtk.Box hbox_encoder_analyze_show_powerbars;
	[Widget] Gtk.CheckButton check_encoder_analyze_show_time_to_peak_power;
	[Widget] Gtk.CheckButton check_encoder_analyze_show_range;

	[Widget] Gtk.Box hbox_encoder_analyze_show_SAFE;
	[Widget] Gtk.CheckButton check_encoder_analyze_show_speed;
	[Widget] Gtk.CheckButton check_encoder_analyze_show_accel;
	[Widget] Gtk.CheckButton check_encoder_analyze_show_force;
	[Widget] Gtk.CheckButton check_encoder_analyze_show_power;
	
	[Widget] Gtk.CheckButton checkbutton_crossvalidate;
	[Widget] Gtk.Button button_encoder_analyze;
	[Widget] Gtk.Box hbox_encoder_analyze_progress;
	[Widget] Gtk.Button button_encoder_analyze_cancel;
	[Widget] Gtk.Box hbox_encoder_user_curves;
	[Widget] Gtk.Label label_encoder_user_curves_active_num;
	[Widget] Gtk.Label label_encoder_user_curves_all_num;

	[Widget] Gtk.Table table_encoder_analyze_instant;
	[Widget] Gtk.HScale hscale_encoder_analyze_a;
	[Widget] Gtk.CheckButton checkbutton_encoder_analyze_b;
	[Widget] Gtk.HScale hscale_encoder_analyze_b;
	[Widget] Gtk.HBox hbox_buttons_scale_encoder_analyze_b;
	[Widget] Gtk.Label label_encoder_analyze_time_a;
	[Widget] Gtk.Label label_encoder_analyze_displ_a;
	[Widget] Gtk.Label label_encoder_analyze_speed_a;
	[Widget] Gtk.Label label_encoder_analyze_accel_a;
	[Widget] Gtk.Label label_encoder_analyze_force_a;
	[Widget] Gtk.Label label_encoder_analyze_power_a;
	[Widget] Gtk.Label label_encoder_analyze_time_b;
	[Widget] Gtk.Label label_encoder_analyze_displ_b;
	[Widget] Gtk.Label label_encoder_analyze_speed_b;
	[Widget] Gtk.Label label_encoder_analyze_accel_b;
	[Widget] Gtk.Label label_encoder_analyze_force_b;
	[Widget] Gtk.Label label_encoder_analyze_power_b;
	[Widget] Gtk.Label label_encoder_analyze_time_diff;
	[Widget] Gtk.Label label_encoder_analyze_displ_diff;
	[Widget] Gtk.Label label_encoder_analyze_speed_diff;
	[Widget] Gtk.Label label_encoder_analyze_accel_diff;
	[Widget] Gtk.Label label_encoder_analyze_force_diff;
	[Widget] Gtk.Label label_encoder_analyze_power_diff;
	[Widget] Gtk.Label label_encoder_analyze_displ_average;
	[Widget] Gtk.Label label_encoder_analyze_speed_average;
	[Widget] Gtk.Label label_encoder_analyze_accel_average;
	[Widget] Gtk.Label label_encoder_analyze_force_average;
	[Widget] Gtk.Label label_encoder_analyze_power_average;
	[Widget] Gtk.Label label_encoder_analyze_displ_max;
	[Widget] Gtk.Label label_encoder_analyze_speed_max;
	[Widget] Gtk.Label label_encoder_analyze_accel_max;
	[Widget] Gtk.Label label_encoder_analyze_force_max;
	[Widget] Gtk.Label label_encoder_analyze_power_max;
	[Widget] Gtk.Label label_encoder_analyze_diff;
	[Widget] Gtk.Label label_encoder_analyze_average;
	[Widget] Gtk.Label label_encoder_analyze_max;
	[Widget] Gtk.Button button_encoder_analyze_AB_save;

	[Widget] Gtk.Button button_encoder_analyze_image_save;
	[Widget] Gtk.Button button_encoder_analyze_table_save;
	[Widget] Gtk.Button button_encoder_analyze_1RM_save;

	[Widget] Gtk.RadioButton radio_encoder_analyze_individual_current_set;
	[Widget] Gtk.RadioButton radio_encoder_analyze_individual_current_session;
	[Widget] Gtk.RadioButton radio_encoder_analyze_individual_all_sessions;
	[Widget] Gtk.RadioButton radio_encoder_analyze_groupal_current_session;

	[Widget] Gtk.Image image_encoder_analyze_individual_current_set;
	[Widget] Gtk.Image image_encoder_analyze_individual_current_session;
	[Widget] Gtk.Image image_encoder_analyze_individual_all_sessions;
	[Widget] Gtk.Image image_encoder_analyze_groupal_current_session;

	[Widget] Gtk.HBox hbox_encoder_analyze_current_signal;
	
	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_powerbars;
	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_cross;
	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_1RM;
	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_single;
	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_side;
	[Widget] Gtk.RadioButton radiobutton_encoder_analyze_neuromuscular_profile;
	[Widget] Gtk.Image image_encoder_analyze_powerbars;
	[Widget] Gtk.Image image_encoder_analyze_cross;
	[Widget] Gtk.Image image_encoder_analyze_1RM;
	[Widget] Gtk.Image image_encoder_analyze_side;
	[Widget] Gtk.Image image_encoder_analyze_single;
	[Widget] Gtk.Image image_encoder_analyze_nmp;
	[Widget] Gtk.HBox hbox_encoder_analyze_intersession;
	[Widget] Gtk.CheckButton check_encoder_intersession_x_is_date;
	[Widget] Gtk.HBox hbox_combo_encoder_analyze_weights;
	[Widget] Gtk.ComboBox combo_encoder_analyze_weights;
	
	[Widget] Gtk.Button button_encoder_analyze_help;


	//[Widget] Gtk.RadioButton radiobutton_encoder_analyze_superpose;
	[Widget] Gtk.CheckButton check_encoder_analyze_eccon_together;
	[Widget] Gtk.Image image_encoder_analyze_eccon_together;
	[Widget] Gtk.Image image_encoder_analyze_eccon_separated;
	
	[Widget] Gtk.Image image_encoder_analyze_speed;
	[Widget] Gtk.Image image_encoder_analyze_accel;
	[Widget] Gtk.Image image_encoder_analyze_force;
	[Widget] Gtk.Image image_encoder_analyze_power;
	
	[Widget] Gtk.Image image_encoder_analyze_mean;
	[Widget] Gtk.Image image_encoder_analyze_max;
	[Widget] Gtk.Image image_encoder_analyze_range;
	[Widget] Gtk.Image image_encoder_analyze_time_to_pp;

	[Widget] Gtk.Box hbox_encoder_analyze_curve_num;
	[Widget] Gtk.Box hbox_combo_encoder_analyze_curve_num_combo;
	[Widget] Gtk.ComboBox combo_encoder_analyze_curve_num_combo;
	[Widget] Gtk.Label label_encoder_analyze_side_max;

	[Widget] Gtk.CheckButton check_encoder_analyze_mean_or_max;

	[Widget] Gtk.ScrolledWindow scrolledwindow_image_encoder_analyze;
//	[Widget] Gtk.Viewport viewport_image_encoder_analyze;
	[Widget] Gtk.Notebook notebook_encoder_analyze;
	[Widget] Gtk.Image image_encoder_analyze;
	[Widget] Gtk.ProgressBar encoder_pulsebar_analyze;
	
	[Widget] Gtk.Alignment alignment_treeview_encoder_capture_curves;
	[Widget] Gtk.TreeView treeview_encoder_capture_curves;
	[Widget] Gtk.TreeView treeview_encoder_analyze_curves;
	
	[Widget] Gtk.Notebook notebook_encoder_capture_or_exercise_or_instructions;
	
	[Widget] Gtk.DrawingArea encoder_capture_signal_drawingarea;

	[Widget] Gtk.DrawingArea encoder_capture_curves_bars_drawingarea;
	Gdk.Pixmap encoder_capture_signal_pixmap = null;
	Gdk.Pixmap encoder_capture_curves_bars_pixmap = null;

	ArrayList encoderCaptureCurves;
        Gtk.ListStore encoderCaptureListStore;
	Gtk.ListStore encoderAnalyzeListStore; //can be EncoderCurves or EncoderNeuromuscularData

	Thread encoderThread;
	Thread encoderThreadBG;
	

	bool encoderPreferencesSet = false;

	int image_encoder_width;
	int image_encoder_height;

	private string encoderAnalysis="powerBars";
	private string ecconLast;
	private string encoderTimeStamp;
	private string encoderSignalUniqueID;

	private EncoderAnalyzeInstant eai;

	private ArrayList array1RM;

	//private static double [] encoderReaded;		//data coming from encoder and converted (can be double)
	//private static int [] encoderReaded;		//data coming from encoder and converted
	//private static int encoderCaptureCountdown;
	//private static Gdk.Point [] encoderCapturePoints;		//stored to be realtime displayed
	//private static int encoderCapturePointsCaptured;		//stored to be realtime displayed
	//private static int encoderCapturePointsPainted;			//stored to be realtime displayed
	EncoderCapture eCapture;
	
	//Contains curves captured to be analyzed by R
	//private static EncoderCaptureCurveArray ecca;
	//private static bool eccaCreated = false;

	private static bool encoderProcessCancel;
	private static bool encoderProcessProblems;
	private static bool encoderProcessFinish;
	private static bool encoderProcessFinishContMode;

	EncoderConfigurationWindow encoder_configuration_win;

	EncoderConfiguration encoderConfigurationCurrent;
	Constants.EncoderGI currentEncoderGI; //store here to not have to check the GUI and have thread problems
	bool firstSetOfCont; //used to don't erase the screen on cont after first set

	/* 
	 * this contains last EncoderSQL captured, recalculated or loaded
	 * 
	 * before using this, saving a curve used the combo values on the top,
	 * but this combo values can be changed by the user, and the he could click on save curve,
	 * then power values (results of curves on graph.R) can be saved with bad weight, exerciseID, ...
	 *
	 * Now, with lastEncoderSQLSignal, saved curves and export curves will take the weight, exerciseID, ...
	 * last capture, recalculate and load. Better usability
	 */
	EncoderSQL lastEncoderSQLSignal;

	/*
	 * CAPTURE is the capture from csharp (not from external python)
	 * CAPTURE_EXTERNAL is deprecated (from Python)
	 *
	 * difference between:
	 * CURVES: calcule and recalculate, autosaves the signal at end
	 * LOAD curves does snot
	 *
	 * CAPTURE_IM records to get the inertia moment but does not calculate curves in R and not updates the treeview
	 * CURVES_AC (After Capture) is like curves but does not start a new thread (uses same pulse as capture)
	 */
	enum encoderActions { CAPTURE_BG, CAPTURE, CAPTURE_EXTERNAL, CURVES, CURVES_AC, LOAD, ANALYZE, CAPTURE_IM, CURVES_IM }
	
	//STOPPING is used to stop the camera. It has to be called only one time
	enum encoderCaptureProcess { CAPTURING, STOPPING, STOPPED } 
	static encoderCaptureProcess capturingCsharp;	
		
	EncoderRProcCapture encoderRProcCapture;
	EncoderRProcAnalyze encoderRProcAnalyze;

	/* 
	 *
	 * To understand this class threads amnd GUI, see diagram:
	 * encoder-threads.dia
	 *
	 */

	enum encoderSensEnum { 
		NOSESSION, NOPERSON, YESPERSON, PROCESSINGCAPTURE, PROCESSINGR, DONENOSIGNAL, DONEYESSIGNAL }
	encoderSensEnum encoderSensEnumStored; //tracks how was sensitive before PROCESSINGCAPTURE or PROCESSINGR
	
	//for writing text
	Pango.Layout layout_encoder_capture_signal;
	Pango.Layout layout_encoder_capture_curves_bars;
	Pango.Layout layout_encoder_capture_curves_bars_text; //e, c

 
	Gdk.GC pen_black_encoder_capture;
	Gdk.GC pen_gray;

	Gdk.GC pen_red_encoder_capture;
	Gdk.GC pen_red_dark_encoder_capture;
	Gdk.GC pen_red_light_encoder_capture;
	Gdk.GC pen_green_encoder_capture;
	Gdk.GC pen_green_dark_encoder_capture;
	Gdk.GC pen_green_light_encoder_capture;
	Gdk.GC pen_blue_encoder_capture;
	Gdk.GC pen_blue_dark_encoder_capture;
	Gdk.GC pen_blue_light_encoder_capture;
	Gdk.GC pen_yellow_encoder_capture;
	
	Gdk.GC pen_white_encoder_capture;
	Gdk.GC pen_selected_encoder_capture;
	

	//TODO:put zoom,unzoom (at side of delete curve)  in capture curves (for every curve)
	//
	//TODO: capture also with webcam an attach it to signal or curve
	//
	//TODO: peak power in eccentric in absolute values
	//
	//TODO: on cross, spline and force speed and power speed should have a spar value higher, like 0.7. On the other hand, the other cross graphs, haveload(mass) in the X lot more discrete, there is good to put 0.5
	


	private void encoderInitializeStuff()
	{
		encoder_pulsebar_capture.Fraction = 1;
		encoder_pulsebar_capture.Text = "";
		encoder_pulsebar_analyze.Fraction = 1;
		encoder_pulsebar_analyze.Text = "";

		//read from SQL
		EncoderConfigurationSQLObject econfSO = SqliteEncoderConfiguration.SelectActive(Constants.EncoderGI.GRAVITATORY);
		encoderConfigurationCurrent = econfSO.encoderConfiguration;
		label_encoder_selected.Text = econfSO.name;
		label_encoder_top_selected.Text = econfSO.name;
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
		
		captureCurvesBarsData = new ArrayList(0);
		
		try {	
			playVideoEncoderInitialSetup();
		} catch {
			//it crashes on Raspberry, Banana
		}

		capturingCsharp = encoderCaptureProcess.STOPPED;

		//done here because in Glade we cannot use the TextBuffer.Changed
		textview_encoder_signal_comment.Buffer.Changed += new EventHandler(on_textview_encoder_signal_comment_key_press_event);

		//configInit();
	
		//triggers
		triggerList = new TriggerList();
		showTriggerTab(false);
	}


	void on_button_encoder_select_clicked (object o, EventArgs args)
	{
		encoder_configuration_win = EncoderConfigurationWindow.View(
				currentEncoderGI, SqliteEncoderConfiguration.SelectActive(currentEncoderGI),
				UtilGtk.ComboGetActive(combo_encoder_anchorage), (int) spin_encoder_im_weights_n.Value); //used on inertial

		encoder_configuration_win.Button_close.Clicked += new EventHandler(on_encoder_configuration_win_closed);

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

	void on_encoder_configuration_win_closed (object o, EventArgs args)
	{
		encoder_configuration_win.Button_close.Clicked -= new EventHandler(on_encoder_configuration_win_closed);
		
		EncoderConfiguration eConfNew = encoder_configuration_win.GetAcceptedValues();
		label_encoder_selected.Text = encoder_configuration_win.Entry_save_name;
		label_encoder_top_selected.Text = encoder_configuration_win.Entry_save_name;

		if(encoderConfigurationCurrent == eConfNew)
			return;
			
		bool combo_encoder_anchorage_should_update = (encoderConfigurationCurrent.list_d != eConfNew.list_d);
		
		encoderConfigurationCurrent = eConfNew;
		LogB.Information("EncoderConfigurationCurrent = " + encoderConfigurationCurrent.ToStringOutput(EncoderConfiguration.Outputs.SQL));
		setEncoderTypePixbuf();
	
		encoderGuiChangesAfterEncoderConfigurationWin(combo_encoder_anchorage_should_update);
	}
	void encoderGuiChangesAfterEncoderConfigurationWin(bool combo_encoder_anchorage_should_update) 
	{
		if(encoderConfigurationCurrent.has_inertia) {
			if(combo_encoder_anchorage_should_update) {
				UtilGtk.ComboUpdate(combo_encoder_anchorage, encoderConfigurationCurrent.list_d.L);
				combo_encoder_anchorage.Active = UtilGtk.ComboMakeActive(
						combo_encoder_anchorage,
						encoderConfigurationCurrent.d.ToString()
						);
			}

			encoderConfigurationCurrent.extraWeightN = (int) spin_encoder_im_weights_n.Value; 
			encoderConfigurationCurrent.inertiaTotal = UtilEncoder.CalculeInertiaTotal(encoderConfigurationCurrent);
			label_encoder_im_total.Text = encoderConfigurationCurrent.inertiaTotal.ToString();
			label_encoder_top_im.Text = Catalog.GetString("Inertia M.") + ": " + label_encoder_im_total.Text;
		}
	}
	
	void on_combo_encoder_anchorage_changed (object o, EventArgs args) {
		string selected = UtilGtk.ComboGetActive(combo_encoder_anchorage);
		if(selected != "" && Util.IsNumber(selected, true))
			encoderConfigurationCurrent.d = Convert.ToDouble(selected);
	}


	// ---- start of spin_encoder_im_weights_n ---->
	/*
	 * when spin is seen the others (-1, entry, +1) are not seen
	 * -1, 1 change the entry
	 * entry changes de spin
	 * spin does not change anything
	 */
	
	void on_button_encoder_im_weights_n_minus_clicked (object o, EventArgs args) {
		changeImWeights(-1);
	}
	void on_button_encoder_im_weights_n_plus_clicked (object o, EventArgs args) {
		changeImWeights(+1);
	}
	private void changeImWeights(int change) {
		int newValue = Convert.ToInt32(entry_encoder_im_weights_n.Text) + change;

		double min, max;
		spin_encoder_im_weights_n.GetRange(out min, out max);
		if(newValue >= Convert.ToDouble(min) && newValue <= Convert.ToDouble(max))
			entry_encoder_im_weights_n.Text = newValue.ToString();
	}

	void on_spin_encoder_im_weights_n_value_changed (object o, EventArgs args) {
		encoderConfigurationCurrent.extraWeightN = (int) spin_encoder_im_weights_n.Value; 
		encoderConfigurationCurrent.inertiaTotal = UtilEncoder.CalculeInertiaTotal(encoderConfigurationCurrent);
		label_encoder_im_total.Text = encoderConfigurationCurrent.inertiaTotal.ToString();
		label_encoder_top_im.Text = Catalog.GetString("Inertia M.") + ": " + label_encoder_im_total.Text;
	}
	void on_entry_encoder_im_weights_n_changed (object o, EventArgs args) 
	{
		if(entry_encoder_im_weights_n.Text == "" || entry_encoder_im_weights_n.Text == "00")
			entry_encoder_im_weights_n.Text = "0";
		else if(Util.IsNumber(entry_encoder_im_weights_n.Text, false)) //cannot be decimal
			spin_encoder_im_weights_n.Value = Convert.ToInt32(entry_encoder_im_weights_n.Text);
		else
			entry_encoder_im_weights_n.Text = spin_encoder_im_weights_n.Value.ToString();

		label_encoder_top_weights.Text = Catalog.GetString("Weights") + ": " + entry_encoder_im_weights_n.Text;
	}

	// <---- end of spin_encoder_im_weights_n ----
	

	
	void on_encoder_configuration_win_capture_inertial_do (object o, EventArgs args) 
	{
		on_button_encoder_capture_calcule_im();
	}
	
	
	private void on_button_encoder_bells_clicked(object o, EventArgs args)
	{
		if(radio_menuitem_mode_power_gravitatory.Active)
			repetitiveConditionsWin.View(Constants.BellModes.ENCODERGRAVITATORY, preferences.volumeOn);
		else
			repetitiveConditionsWin.View(Constants.BellModes.ENCODERINERTIAL, preferences.volumeOn);
	}

	/*
	private bool encoderCheckPort()	
	{
		if(File.Exists(UtilAll.GetECapSimSignalFileName())) //simulatedEncoder
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

	double findMaxPowerIntersession()
	{
		//finding historical maxPower of a person in an exercise
		Constants.EncoderGI encGI = getEncoderGI();
		ArrayList arrayTemp = SqliteEncoder.Select(false, -1, currentPerson.UniqueID, -1, encGI,
					getExerciseIDFromCombo(exerciseCombos.CAPTURE), "curve", EncoderSQL.Eccons.ALL,
					false, false);
		double maxPower = 0;
		if(encGI == Constants.EncoderGI.GRAVITATORY)
		{
			//TODO: do a regression to find maxPower with a value of extraWeight unused
			int extraWeight = Convert.ToInt32(spin_encoder_extra_weight.Value);
			foreach(EncoderSQL es in arrayTemp)
				if(Convert.ToInt32(es.extraWeight) == extraWeight && Convert.ToDouble(es.future1) > maxPower)
					maxPower = Convert.ToDouble(es.future1);
		}
		else if(encGI == Constants.EncoderGI.INERTIAL)
		{
			foreach(EncoderSQL es in arrayTemp)
				if(es.encoderConfiguration == encoderConfigurationCurrent && Convert.ToDouble(es.future1) > maxPower)
					maxPower = Convert.ToDouble(es.future1);
		}

		return maxPower;
	}

	bool canCaptureEncoder()
	{
		if(currentSession.Name == Constants.SessionSimulatedName && testsActive)
			return true;

		chronopicRegisterUpdate(false);
		int numEncoders = chronopicRegister.NumConnectedOfType(ChronopicRegisterPort.Types.ENCODER);
		LogB.Information("numEncoders: " + numEncoders);
		if(numEncoders == 0) {
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Encoder is not connected"));
			return false;
		}
		if(numEncoders > 1) {
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("More than 1 encoders are connected"));
			return false;
		}

		return true;
	}

	EncoderCaptureInertialBackground eCaptureInertialBG; //only created one time
	void on_button_encoder_inertial_calibrate_clicked (object o, EventArgs args)
	{
		if(! canCaptureEncoder())
			return;

		/*
		 * if user calibrates again: put 0 value
		 * if calibration was not running: start it
		 */
		if(encoderThreadBG != null && encoderThreadBG.IsAlive)
			eCaptureInertialBG.AngleNow = 0;
		else
		{
			encoderThreadStart(encoderActions.CAPTURE_BG);
		}
	}
	void on_button_encoder_inertial_recalibrate_clicked (object o, EventArgs args)
	{
		notebook_encoder_capture_or_exercise_or_instructions.Page = 2;
	}

	private void setEncoderExerciseOptionsFromPreferences()
	{
		Sqlite.Open();

		//1. exercise
		string exerciseID = "";
		if(currentEncoderGI == Constants.EncoderGI.GRAVITATORY)
			exerciseID = SqlitePreferences.Select(SqlitePreferences.EncoderExerciseIDGravitatory, true);
		else
			exerciseID = SqlitePreferences.Select(SqlitePreferences.EncoderExerciseIDInertial, true);

		string exerciseNameTranslated = Util.FindOnArray(':', 0, 2, exerciseID.ToString(),
				encoderExercisesTranslationAndBodyPWeight);

		combo_encoder_exercise_capture.Active = UtilGtk.ComboMakeActive(
				combo_encoder_exercise_capture, exerciseNameTranslated);

		//2 contraction
		string contraction = "";
		if(currentEncoderGI == Constants.EncoderGI.GRAVITATORY)
			contraction = SqlitePreferences.Select(SqlitePreferences.EncoderContractionGravitatory, true);
		else
			contraction = SqlitePreferences.Select(SqlitePreferences.EncoderContractionInertial, true);

		radio_encoder_eccon_concentric.Active = (contraction == Constants.Concentric);

		//3 laterality
		string laterality = "";
		if(currentEncoderGI == Constants.EncoderGI.GRAVITATORY)
			laterality = SqlitePreferences.Select(SqlitePreferences.EncoderLateralityGravitatory, true);
		else
			laterality = SqlitePreferences.Select(SqlitePreferences.EncoderLateralityInertial, true);

		if(laterality == "RL")
			radio_encoder_laterality_both.Active = true;
		else if(laterality == "R")
			radio_encoder_laterality_r.Active = true;
		else //if(laterality == "L")
			radio_encoder_laterality_l.Active = true;

		//4 mass / weights
		string mass = SqlitePreferences.Select(SqlitePreferences.EncoderMassGravitatory, true);
		entry_raspberry_extra_weight.Text = mass;

		string weights = SqlitePreferences.Select(SqlitePreferences.EncoderWeightsInertial, true);
		entry_encoder_im_weights_n.Text = weights;


		Sqlite.Close();
	}

	private void saveEncoderExerciseOptionsToPreferences()
	{
		//store execution params on SQL for next Chronojump start
		Sqlite.Open();

		//1 exercise
		int exerciseID = getExerciseIDFromCombo (exerciseCombos.CAPTURE);
		if(currentEncoderGI == Constants.EncoderGI.GRAVITATORY)
			SqlitePreferences.Update (SqlitePreferences.EncoderExerciseIDGravitatory, exerciseID.ToString(), true);
		else
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
		string laterality = "RL";
		if(radio_encoder_laterality_r.Active)
			laterality = "R";
		else if(radio_encoder_laterality_l.Active)
			laterality = "L";

		if(currentEncoderGI == Constants.EncoderGI.GRAVITATORY)
			SqlitePreferences.Update (SqlitePreferences.EncoderLateralityGravitatory, laterality, true);
		else
			SqlitePreferences.Update (SqlitePreferences.EncoderLateralityInertial, laterality, true);

		//4 mass / weights
		if(currentEncoderGI == Constants.EncoderGI.GRAVITATORY)
			SqlitePreferences.Update (SqlitePreferences.EncoderMassGravitatory,
					Util.ConvertToPoint(findMass(Constants.MassType.EXTRA)), //when save on sql, do not include person weight
					true);
		else
			SqlitePreferences.Update (SqlitePreferences.EncoderWeightsInertial,
					spin_encoder_im_weights_n.Value.ToString(),
					true);

		Sqlite.Close();

	}

	double maxPowerIntersessionOnCapture;
	//called from main GUI
	void on_button_encoder_capture_clicked (object o, EventArgs args) 
	{
		saveEncoderExerciseOptionsToPreferences();

		on_button_encoder_capture_clicked_do (true);
	}

	void on_button_encoder_capture_clicked_do (bool firstSet)
	{
//		if(eCaptureInertialBG != null)
//			eCaptureInertialBG.Finish();

		firstSetOfCont = firstSet;

		maxPowerIntersessionOnCapture = findMaxPowerIntersession();
		//LogB.Information("maxPower: " + maxPowerIntersessionOnCapture);

		if(encoderThreadBG != null && encoderThreadBG.IsAlive) //if we are capturing on the background ...
		{
			// stop capturing on the background if we start capturing gravitatory
			if(! encoderConfigurationCurrent.has_inertia)
			{
				stopCapturingInertialBG();
			}
		}
		else //if we are NOT capturing on the background ...
		{
			//check if chronopics have changed
			if(! canCaptureEncoder() )
				return;

			if(encoderConfigurationCurrent.has_inertia)
			{
				//show inertia calibrate instructions. User will click on calibrate and this method will be called again

				sensitiveGuiEventDoing(radio_encoder_capture_cont.Active);
				button_encoder_inertial_calibrate.Sensitive = true;
				label_wait.Text = " ";
				notebook_encoder_capture_or_exercise_or_instructions.Page = 2;

				return;
			}
		}


		//This notebook has capture (signal plotting), and curves (shows R graph)	
		if(notebook_encoder_capture.CurrentPage == 1)
			notebook_encoder_capture.PrevPage();

		radiobutton_video_encoder_capture.Active = true;

		sensitiveGuiEventDoing(radio_encoder_capture_cont.Active);

		LogB.Debug("Calling encoderThreadStart for capture");

		//record this encoderConfiguration to SQL for next Chronojump open
		SqliteEncoderConfiguration.UpdateActive(false, currentEncoderGI, encoderConfigurationCurrent);

		needToCallPrepareEncoderGraphs = false;
		encoderProcessFinish = false;
		encoderThreadStart(encoderActions.CAPTURE);

		textview_encoder_signal_comment.Buffer.Text = "";

		LogB.Debug("end of Calling encoderThreadStart for capture");
	}
	
	void on_button_encoder_capture_calcule_im () 
	{
		//check if chronopics have changed
		if(! canCaptureEncoder())
			return;

		encoder_configuration_win.Button_encoder_capture_inertial_do_chronopic_ok();
		
		//tis notebook has capture (signal plotting), and curves (shows R graph)	
		if(notebook_encoder_capture.CurrentPage == 1)
			notebook_encoder_capture.PrevPage();
		
		encoderProcessFinish = false;
		encoderThreadStart(encoderActions.CAPTURE_IM);
	}


	void on_combo_encoder_exercise_capture_changed (object o, EventArgs args) {
		if(UtilGtk.ComboGetActive(combo_encoder_exercise_capture) != "") { //needed because encoder_exercise_edit updates this combo and can be without values in the changing process
			array1RMUpdate(false);
			encoder_change_displaced_weight_and_1RM ();
			label_encoder_top_exercise.Text = UtilGtk.ComboGetActive(combo_encoder_exercise_capture);
		}
	}
	
	void on_combo_encoder_exercise_analyze_changed (object o, EventArgs args) {
		prepareAnalyzeRepetitions ();
	}

	// ---- change extra weight start ----
	/*
	 * when spin is seen the others (-10, -1, entry, +1, +10) are not seen
	 * -10, -1, 1, +10 change the entry
	 * entry changes de spin
	 * spin does not change anything
	 */
	
	void on_button_encoder_raspberry_extra_weight_minus_10_clicked (object o, EventArgs args) {
		rapsberryChangeExtraWeight(-10);
	}
	void on_button_encoder_raspberry_extra_weight_minus_1_clicked (object o, EventArgs args) {
		rapsberryChangeExtraWeight(-1);
	}
	void on_button_encoder_raspberry_extra_weight_plus_10_clicked (object o, EventArgs args) {
		rapsberryChangeExtraWeight(+10);
	}
	void on_button_encoder_raspberry_extra_weight_plus_1_clicked (object o, EventArgs args) {
		rapsberryChangeExtraWeight(+1);
	}
	void rapsberryChangeExtraWeight(int change) {
		/*
		 * don't change spin to avoid circular problems
		 * spin_encoder_extra_weight.Value += change;
		 * change the entry
		 * */
		int newValue = Convert.ToInt32(entry_raspberry_extra_weight.Text) + change;

		double min, max;
		spin_encoder_extra_weight.GetRange(out min, out max);
		if(newValue >= Convert.ToDouble(min) && newValue <= Convert.ToDouble(max))
			entry_raspberry_extra_weight.Text = newValue.ToString();
	}

	void on_spin_encoder_extra_weight_value_changed (object o, EventArgs args) 
	{
		//don't need to:
		//array1RMUpdate(false);
		//because then we will be calling SQL at each spinbutton increment

		encoder_change_displaced_weight_and_1RM ();
	}
	void on_entry_raspberry_extra_weight_changed (object o, EventArgs args) 
	{
		if(entry_raspberry_extra_weight.Text == "" || entry_raspberry_extra_weight.Text == "00")
			entry_raspberry_extra_weight.Text = "0";
		else if(Util.IsNumber(entry_raspberry_extra_weight.Text, false)) //cannot be decimal
			spin_encoder_extra_weight.Value = Convert.ToInt32(entry_raspberry_extra_weight.Text);
		else
			entry_raspberry_extra_weight.Text = spin_encoder_extra_weight.Value.ToString();

		label_encoder_top_extra_mass.Text = entry_raspberry_extra_weight.Text + " Kg";
	}

	void encoder_change_displaced_weight_and_1RM () 
	{
		//displaced weight
		label_encoder_displaced_weight.Text = (findMass(Constants.MassType.DISPLACED)).ToString();

		double load1RM = 0;
		if(array1RM.Count > 0)
			load1RM = ((Encoder1RM) array1RM[0]).load1RM; //take only the first in array (will be the last uniqueID)

		if(load1RM == 0 || findMass(Constants.MassType.EXTRA) == 0)
		{
			label_encoder_1RM_percent.Text = "";
			label_encoder_top_1RM_percent.Text = "";
		}
		else
		{
			label_encoder_1RM_percent.Text = Util.TrimDecimals(
					(100 * findMass(Constants.MassType.EXTRA) / ( load1RM * 1.0 )).ToString(), 1);
			label_encoder_top_1RM_percent.Text = label_encoder_1RM_percent.Text + " %1RM";
		}
	}
	
	// ---- end of change extra weight ----
	


	//array1RM variable is not local because we need to perform calculations at each change on displaced_weight
	void array1RMUpdate (bool returnPersonNameAndExerciseName) 
	{
		if(currentPerson != null)
			array1RM = SqliteEncoder.Select1RM(
					false, currentPerson.UniqueID, -1, //-1: currentSession = all sessions
					getExerciseIDFromCombo(exerciseCombos.CAPTURE), returnPersonNameAndExerciseName);
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
	
		genericWin = GenericWindow.Show(false,	//don't show now
				string.Format(Catalog.GetString("Saved 1RM values of athlete {0} in {1} exercise."), 
					currentPerson.Name, UtilGtk.ComboGetActive(combo_encoder_exercise_capture)) + "\n" + 
				Catalog.GetString("If you want to delete a row, right click on it.") + "\n" + 
				Catalog.GetString("If there is more than one value, top one will be used."),
				bigArray);

		genericWin.SetTreeview(columnsString, false, dataPrint, new ArrayList(), Constants.ContextMenu.DELETE, false);
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
		int uniqueID = SqliteEncoder.Insert1RM(false, currentPerson.UniqueID, currentSession.UniqueID, 
				getExerciseIDFromCombo (exerciseCombos.CAPTURE), genericWin.SpinDouble2Selected);

		genericWin.Row_add(new string[] {
				uniqueID.ToString(), currentPerson.Name, UtilGtk.ComboGetActive(combo_encoder_exercise_capture),
				d.ToString(), currentSession.DateShort
				}
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
	//CAPTURE_EXTERNAL, CURVES, LOAD (signal is defined)
	//CAPTURE_EXTERNAL is not implemented because it's deprecated
	void encoderCalculeCurves(encoderActions action)
	{
		if(action == encoderActions.CURVES_AC) 
		{
			encoderTimeStamp = UtilDate.ToFile(DateTime.Now);
			encoderSignalUniqueID = "-1"; //mark to know that there's no ID for this until it's saved on database
			encoderThreadStart(action);
		} else {
			//calculate and recalculate saves the curve at end
			//load does not save the curve 
		       if(File.Exists(UtilEncoder.GetEncoderDataTempFileName()))
			       encoderThreadStart(action);
		       else
			       encoder_pulsebar_capture.Text = Catalog.GetString("Missing data.");
		}
	}

	
	void on_button_encoder_cancel_clicked (object o, EventArgs args) 
	{
		eCapture.Cancel();
	}

	void on_button_encoder_capture_finish_clicked (object o, EventArgs args) 
	{
		eCapture.Finish();
		encoderProcessFinish = true;
			
	}
	void on_button_encoder_capture_finish_cont_clicked (object o, EventArgs args) 
	{
		encoderProcessFinishContMode = true;
		on_button_encoder_capture_finish_clicked (o, args); 
	}

	void on_button_encoder_recalculate_clicked (object o, EventArgs args)
	{
		if(triggerList != null && triggerList.Count() > 0 && findEccon(false) != "c")
		{
			ConfirmWindow confirmWin = ConfirmWindow.Show(
					Catalog.GetString("Recalculate this set will remove existing triggers."),
					Catalog.GetString("Are you sure!"), "");
			confirmWin.Button_accept.Clicked += new EventHandler(on_button_encoder_recalculate_clicked_do);
		}
		else
			on_button_encoder_recalculate_clicked_do (o, args);
	}
	void on_button_encoder_recalculate_clicked_do (object o, EventArgs args)
	{
		//record this encoderConfiguration to SQL for next Chronojump open
		SqliteEncoderConfiguration.UpdateActive(false, currentEncoderGI, encoderConfigurationCurrent);
		encoderCalculeCurves(encoderActions.CURVES);
	}

	void on_textview_encoder_signal_comment_key_press_event (object o, EventArgs args) {
		button_encoder_signal_save_comment.Label = Catalog.GetString("Save comment");
		button_encoder_signal_save_comment.Sensitive = true;
	}
	void on_button_encoder_signal_save_comment_clicked (object o, EventArgs args) {
		LogB.Debug(encoderSignalUniqueID);
		if(encoderSignalUniqueID != null && Convert.ToInt32(encoderSignalUniqueID) > 0) {
			Sqlite.Update(false, Constants.EncoderTable, "description", "", 
					Util.RemoveTildeAndColonAndDot(textview_encoder_signal_comment.Buffer.Text), 
					"uniqueID", encoderSignalUniqueID);
			button_encoder_signal_save_comment.Label = Catalog.GetString("Saved comment.");
			button_encoder_signal_save_comment.Sensitive = false;
		}
	}




	private void encoderUpdateTreeViewCapture(List<string> contents)
	{
		if (contents == null || contents.Count == 0) {
			encoderButtonsSensitive(encoderSensEnum.DONENOSIGNAL);
		} else {
			treeviewEncoderCaptureRemoveColumns();
			int curvesNum = createTreeViewEncoderCapture(contents);
			if(curvesNum == 0) 
				encoderButtonsSensitive(encoderSensEnum.DONENOSIGNAL);
			else {
				if(! radio_encoder_eccon_concentric.Active)
					curvesNum = curvesNum / 2;
			
				string [] activeCurvesList = new String[curvesNum +1];
				activeCurvesList[0] = Catalog.GetString("All");
				for(int i=1; i <= curvesNum; i++)
					activeCurvesList[i] = i.ToString();
				UtilGtk.ComboUpdate(combo_encoder_analyze_curve_num_combo, activeCurvesList, "");
				combo_encoder_analyze_curve_num_combo.Active = 
					UtilGtk.ComboMakeActive(combo_encoder_analyze_curve_num_combo, activeCurvesList[1]);
				
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


	private void encoderDoCurvesGraphR_curves() {
		encoderDoCurvesGraphR("curves");
	}
	private void encoderDoCurvesGraphR_curvesAC() {
		encoderDoCurvesGraphR("curvesAC");
	}

	private void setLastEncoderSQLSignal()
	{
		//without this we loose the videoURL on recalculate
		string videoURL = "";		
		if(encoderSignalUniqueID != null && encoderSignalUniqueID != "-1") {
			string file = Util.GetVideoFileName(currentSession.UniqueID, 
				Constants.TestTypes.ENCODER, Convert.ToInt32(encoderSignalUniqueID));

			if(file != null && file != "" && File.Exists(file))
				videoURL = file;
		}

		string laterality = Catalog.GetString("RL");
		if(radio_encoder_laterality_r.Active)
			laterality = Catalog.GetString("R");
		else if(radio_encoder_laterality_l.Active)
			laterality = Catalog.GetString("L");

		//see explanation on the top of this file
		lastEncoderSQLSignal = new EncoderSQL(
				"-1",
				currentPerson.UniqueID,
				currentSession.UniqueID,
				getExerciseIDFromCombo(exerciseCombos.CAPTURE),	
				findEccon(true), 	//force ecS (ecc-conc separated)
				laterality,
				Util.ConvertToPoint(findMass(Constants.MassType.EXTRA)), //when save on sql, do not include person weight
				"",	//signalOrCurve,
				"", 	//fileSaved,	//to know date do: select substr(name,-23,19) from encoder;
				"",	//path,			//url
				preferences.encoderCaptureTime, 
				preferences.EncoderCaptureMinHeight(encoderConfigurationCurrent.has_inertia), 
				Util.RemoveTildeAndColonAndDot(textview_encoder_signal_comment.Buffer.Text), //desc,
				"", videoURL,		//status, videoURL
				encoderConfigurationCurrent,
				"","","",	//future1, 2, 3
				Util.FindOnArray(':', 2, 1, UtilGtk.ComboGetActive(combo_encoder_exercise_capture), 
					encoderExercisesTranslationAndBodyPWeight)	//exerciseName (english)
				);
	}

	//this is called by non gtk thread. Don't do gtk stuff here
	//I suppose reading gtk is ok, changing will be the problem
	//called on calculatecurves, recalculate and load
	//analysisSent can be "curves" or "curvesAC"
	private void encoderDoCurvesGraphR(string analysisSent)
	{
		LogB.Debug("encoderDoCurvesGraphR() start");

		setLastEncoderSQLSignal();

		string analysis = analysisSent;
		string analysisOptions = getEncoderAnalysisOptions();

		EncoderParams ep = new EncoderParams(
				preferences.EncoderCaptureMinHeight(encoderConfigurationCurrent.has_inertia), 
				getExercisePercentBodyWeightFromComboCapture (),
				Util.ConvertToPoint(findMass(Constants.MassType.BODY)),
				Util.ConvertToPoint(findMass(Constants.MassType.EXTRA)),
				findEccon(true),					//force ecS (ecc-conc separated)
				analysis,
				"none",				//analysisVariables (not needed in create curves). Cannot be blank
				analysisOptions,
				preferences.encoderCaptureCheckFullyExtended,
				preferences.encoderCaptureCheckFullyExtendedValue,
				encoderConfigurationCurrent,
				Util.ConvertToPoint(preferences.encoderSmoothCon),	//R decimal: '.'
			       	0, 			//curve is not used here
				image_encoder_width, image_encoder_height,
				preferences.CSVExportDecimalSeparator 
				);

		EncoderStruct es = new EncoderStruct(
				UtilEncoder.GetEncoderDataTempFileName(), 
				UtilEncoder.GetEncoderGraphTempFileName(),
				UtilEncoder.GetEncoderCurvesTempFileName(), 
				UtilEncoder.GetEncoderScriptsPathWithoutLastSep(),
				UtilEncoder.GetEncoderTempPathWithoutLastSep(),
				ep);


		string title = Util.ChangeSpaceAndMinusForUnderscore(currentPerson.Name) + "-" +
			Util.ChangeSpaceAndMinusForUnderscore(UtilGtk.ComboGetActive(combo_encoder_exercise_capture));
		if(encoderConfigurationCurrent.has_inertia)
			title += "-(" + encoderConfigurationCurrent.inertiaTotal.ToString() + " " + Catalog.GetString("Inertia M.") + ")";
		else
			title += "-(" + Util.ConvertToPoint(findMass(Constants.MassType.DISPLACED)) + "Kg)";

		//triggers stuff
		if(analysisSent == "curvesAC")
			triggerList = eCapture.GetTriggers();

		//triggers only on concentric
		if(triggerList == null || findEccon(false) != "c")
			triggerList = new TriggerList();

		//send data to encoderRProcAnalyze
		encoderRProcAnalyze.SendData(
				title,
				false,	//do not use neuromuscularProfile script
				preferences.RGraphsTranslate,
				triggerList
				); 
		bool result = encoderRProcAnalyze.StartOrContinue(es);
				
		if(result)
			//store this to show 1,2,3,4,... or 1e,1c,2e,2c,... in RenderN
			//if is not stored, it can change when changed eccon radiobutton on cursor is in treeview
			ecconLast = findEccon(false);
		else {
			encoderProcessProblems = true;
		}
		LogB.Debug("encoderDoCurvesGraphR() end");
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

		encSelReps.PassVariables(currentPerson, currentSession, currentEncoderGI,
				button_encoder_analyze, getExerciseIDFromCombo(exerciseCombos.ANALYZE),
				preferences.askDeletion);

		encSelReps.Do();

		updateUserCurvesLabelsAndCombo(false);
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
				       	-1, "", EncoderSQL.Eccons.ALL, false, true)[0];
		} catch {
			eSQLfound = false;
			LogB.Warning("Catched! seems it's already deleted");
		}

		//remove the file
		if(eSQLfound)
			Util.FileDelete(eSQL.GetFullURL(false));	//don't convertPathToR

		Sqlite.Delete(dbconOpened, Constants.EncoderTable, Convert.ToInt32(uniqueID));

		ArrayList escArray = SqliteEncoder.SelectSignalCurve(dbconOpened, 
				-1, Convert.ToInt32(uniqueID),	//signal, curve
				-1, -1); 			//msStart, msEnd
		if(eSQLfound)
			SqliteEncoder.DeleteSignalCurveWithCurveID(dbconOpened, 
					Convert.ToInt32(eSQL.uniqueID)); //delete by curveID on SignalCurve table
		//if deleted curve is from current signal, uncheck it in encoderCaptureCurves
		if(escArray.Count > 0) {
			EncoderSignalCurve esc = (EncoderSignalCurve) escArray[0];
			if(esc.signalID == Convert.ToInt32(encoderSignalUniqueID))
				encoderCaptureSelectBySavedCurves(esc.msCentral, false);
		}

		//TODO: change encSelReps and this will change labels
		updateUserCurvesLabelsAndCombo(dbconOpened);
	}
		
	private Constants.EncoderGI getEncoderGI() {
		/*
		if(radio_menuitem_mode_power_gravitatory.Active)
			return = Constants.EncoderGI.GRAVITATORY;
		else //if(radio_menuitem_mode_power_inertial.Active)
			return = Constants.EncoderGI.INERTIAL;
			*/
		return currentEncoderGI;
	}

	//separated to be called also from guiT
	ArrayList encoderLoadSignalData() {
		return SqliteEncoder.Select(
				false, -1, currentPerson.UniqueID, currentSession.UniqueID, getEncoderGI(),
				-1, "signal", EncoderSQL.Eccons.ALL, 
				false, true);
	}
	//this is called when user clicks on load signal
	void on_button_encoder_load_signal_clicked (object o, EventArgs args) {
		on_encoder_load_signal_clicked (Convert.ToInt32(encoderSignalUniqueID));
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
			Catalog.GetString("Encoder"),
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
		
		genericWin = GenericWindow.Show(false,	//don't show now
				string.Format(Catalog.GetString("Select set of athlete {0} on this session."), 
					currentPerson.Name) + "\n" + 
				Catalog.GetString("If you want to edit or delete a row, right click on it."), bigArray);

		genericWin.SetTreeview(columnsString, false, dataPrint, new ArrayList(), Constants.ContextMenu.EDITDELETE, true);
	
		//find all persons in current session
		ArrayList personsPre = SqlitePersonSession.SelectCurrentSessionPersons(currentSession.UniqueID,
				false); //means: do not returnPersonAndPSlist

		string [] persons = new String[personsPre.Count];
		count = 0;
	        foreach	(Person p in personsPre)
			persons[count++] = p.UniqueID.ToString() + ":" + p.Name;
		genericWin.SetComboValues(persons, currentPerson.UniqueID + ":" + currentPerson.Name);
		genericWin.SetComboLabel(Catalog.GetString("Change the owner of selected set") + 
				" (" + Catalog.GetString("code") + ":" + Catalog.GetString("name") + ")");
		genericWin.ShowEditRow(false);

		//select row corresponding to current signal
		genericWin.SelectRowWithID(0, myEncoderSignalUniqueID); //colNum, id
		
		genericWin.CommentColumn = 9;
	
		genericWin.ShowButtonCancel(true);
		genericWin.SetButtonAcceptLabel(Catalog.GetString("Load"));
		genericWin.SetButtonCancelLabel(Catalog.GetString("Close"));
		genericWin.SetButtonAcceptSensitive(false);
		genericWin.Button_accept.Clicked += new EventHandler(on_encoder_load_signal_accepted);
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

		ArrayList data = SqliteEncoder.Select(
				false, uniqueID, currentPerson.UniqueID, currentSession.UniqueID, Constants.EncoderGI.ALL,
				-1, "signal", EncoderSQL.Eccons.ALL, 
				false, true);

		bool success = false;
		foreach(EncoderSQL eSQL in data) {	//it will run only one time
			success = UtilEncoder.CopyEncoderDataToTemp(eSQL.url, eSQL.filename);
			if(success) {
				string exerciseNameTranslated =Util.FindOnArray(':', 1, 2, eSQL.exerciseName, 
						encoderExercisesTranslationAndBodyPWeight);
				combo_encoder_exercise_capture.Active = UtilGtk.ComboMakeActive(combo_encoder_exercise_capture, exerciseNameTranslated);

				if(eSQL.ecconLong == Catalog.GetString(Constants.Concentric))
					radio_encoder_eccon_concentric.Active = true;
				else
					radio_encoder_eccon_eccentric_concentric.Active = true;

				//laterality is stored on English but translated on encoder sqlite select
				if(eSQL.laterality == Catalog.GetString("RL"))
					radio_encoder_laterality_both.Active = true;
				else if(eSQL.laterality == Catalog.GetString("R"))
					radio_encoder_laterality_r.Active = true;
				else //if(eSQL.laterality == Catalog.GetString("L"))
					radio_encoder_laterality_l.Active = true;

				spin_encoder_extra_weight.Value = Convert.ToInt32(eSQL.extraWeight);
				entry_raspberry_extra_weight.Text = Convert.ToInt32(eSQL.extraWeight).ToString();

				preferences.EncoderChangeMinHeight(eSQL.encoderConfiguration.has_inertia, eSQL.minHeight);
				//TODO: show info to user in a dialog,
				//but check if more info have to be shown on this process

				textview_encoder_signal_comment.Buffer.Text = eSQL.description;
				encoderTimeStamp = eSQL.GetDate(false); 
				encoderSignalUniqueID = eSQL.uniqueID;
			
				//has to be done here, because if done in encoderThreadStart or in finishPulsebar it crashes 
				//notebook_video_encoder.CurrentPage = 1;
				radiobutton_video_encoder_play.Active = true;
			
				encoderConfigurationCurrent = eSQL.encoderConfiguration;

				//manage EncoderConfigurationSQLObject
				SqliteEncoderConfiguration.MarkAllAsUnactive(false, currentEncoderGI);
				EncoderConfigurationSQLObject econfSO = SqliteEncoderConfiguration.SelectByEconf(false, currentEncoderGI, eSQL.encoderConfiguration);

				//if user has deleted this econfSO, create it again
				if(econfSO.uniqueID == -1)
				{
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
				} else {
					//if exists on datbase mark and update sql row as active
					econfSO.active = true;
					SqliteEncoderConfiguration.Update(false, currentEncoderGI, econfSO.name, econfSO);
				}

				encoderConfigurationGUIUpdate();
				label_encoder_selected.Text = econfSO.name;
				label_encoder_top_selected.Text = econfSO.name;

				//triggers
				triggerList = new TriggerList(
						SqliteTrigger.Select(
							false, Trigger.Modes.ENCODER,
							Convert.ToInt32(encoderSignalUniqueID))
						);
				showTriggersAndTab();
			}
		}

		//test: try to compress signal in order to send if.
		//obviously this is not going to be done here

		//LogB.Information("Trying compress function");
		//LogB.Information(UtilEncoder.CompressSignal(UtilEncoder.GetEncoderDataTempFileName()));

		if(success) {	
			//record this encoderConfiguration to SQL for next Chronojump open
			SqliteEncoderConfiguration.UpdateActive(false, currentEncoderGI, encoderConfigurationCurrent);

			//force a recalculate but not save the curve (we are loading)
			encoderCalculeCurves(encoderActions.LOAD);
		
			radio_encoder_analyze_individual_current_set.Active = true;

			encoderButtonsSensitive(encoderSensEnumStored);
		}
	}

	protected void on_encoder_load_signal_row_edit (object o, EventArgs args) {
		LogB.Information("row edit at load signal");
		LogB.Information(genericWin.TreeviewSelectedUniqueID.ToString());
		genericWin.ShowEditRow(true);
	}
	
	protected void on_encoder_load_signal_row_edit_apply (object o, EventArgs args) {
		LogB.Information("row edit apply at load signal");
			
		int curveID = genericWin.TreeviewSelectedUniqueID;
		EncoderSQL eSQL = (EncoderSQL) SqliteEncoder.Select(false, curveID, 0, 0, Constants.EncoderGI.ALL,
				-1, "", EncoderSQL.Eccons.ALL, false, true)[0];
		
		//if changed comment, update SQL, and update treeview
		//first remove conflictive characters
		string comment = Util.RemoveTildeAndColonAndDot(genericWin.EntryEditRow);
		if(comment != eSQL.description) {
			eSQL.description = comment;
			SqliteEncoder.Update(false, eSQL);

			//update treeview
			genericWin.on_edit_selected_done_update_treeview();
		}

		//if changed person, proceed
		LogB.Information("new person: " + genericWin.GetComboSelected);
		int newPersonID = Util.FetchID(genericWin.GetComboSelected);
		if(newPersonID != currentPerson.UniqueID) {
			EncoderSQL eSQLChangedPerson = eSQL.ChangePerson(genericWin.GetComboSelected);
			SqliteEncoder.Update(false, eSQLChangedPerson);
			
			genericWin.RemoveSelectedRow();
		}

		genericWin.ShowEditRow(false);
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
		if(signalID == Convert.ToInt32(encoderSignalUniqueID))
			on_button_encoder_delete_signal_accepted (o, args);
		else {
			EncoderSQL eSQL = (EncoderSQL) SqliteEncoder.Select(
					false, signalID, 0, 0, Constants.EncoderGI.ALL,
					-1, "signal", EncoderSQL.Eccons.ALL, false, true)[0];
		
			//delete signal and related curves (both from SQL and files)
			encoderSignalDelete(eSQL.GetFullURL(false), signalID);	//don't convertPathToR

			//genericWin selected row is deleted, unsensitive the "load" button
			genericWin.SetButtonAcceptSensitive(false);
		}
		genericWin.Delete_row_accepted();
	}
				
	void encoderConfigurationGUIUpdate()
	{
		if(encoderConfigurationCurrent.has_inertia)
		{
			notebook_encoder_top.Page = 1;
			label_encoder_exercise_mass.Visible = false;
			vbox_encoder_exercise_mass.Visible = false;
			label_encoder_exercise_inertia.Visible = true;
			vbox_encoder_exercise_inertia.Visible = true;
			
			if(! encoderConfigurationCurrent.list_d.IsEmpty())
			{
				UtilGtk.ComboUpdate(combo_encoder_anchorage, encoderConfigurationCurrent.list_d.L);
				combo_encoder_anchorage.Active = UtilGtk.ComboMakeActive(
						combo_encoder_anchorage, 
						encoderConfigurationCurrent.d.ToString()
						);
			}

			//this will update also spin_encoder_im_weights_n
			entry_encoder_im_weights_n.Text = encoderConfigurationCurrent.extraWeightN.ToString();

			label_encoder_im_total.Text = encoderConfigurationCurrent.inertiaTotal.ToString();
			label_encoder_top_im.Text = Catalog.GetString("Inertia M.") + ": " + label_encoder_im_total.Text;
		}
		else {
			notebook_encoder_top.Page = 0;
			label_encoder_exercise_mass.Visible = true;
			vbox_encoder_exercise_mass.Visible = true;
			label_encoder_exercise_inertia.Visible = false;
			vbox_encoder_exercise_inertia.Visible = false;
		}
	}

	void encoderSignalDelete (string signalURL, int signalID) 
	{
		//remove signal file
		Util.FileDelete(signalURL);

		//delete signal from encoder table
		Sqlite.Delete(false, Constants.EncoderTable, signalID);

		//find related curves using encoderSignalCurve table
		ArrayList linkedCurves = SqliteEncoder.SelectSignalCurve(
				false, signalID, -1, -1, -1);	//DBopened, signal, curve, msStart, msEnd
		
		//delete related curves: files and records from encoder table
		foreach(EncoderSignalCurve esc in linkedCurves) 
		{
			//select related curves to find URL
			EncoderSQL eSQL = (EncoderSQL) SqliteEncoder.Select(
					false, esc.curveID, -1, -1, Constants.EncoderGI.ALL,
					-1, "curve", EncoderSQL.Eccons.ALL, false, true)[0];
			
			//delete file
			Util.FileDelete(eSQL.GetFullURL(false));	//don't convertPathToR

			//delete curve from encoder table
			Sqlite.Delete(false, Constants.EncoderTable, esc.curveID);
		}
		
		//delete related records from encoderSignalCurve table
		Sqlite.DeleteSelectingField(false, Constants.EncoderSignalCurveTable, 
				"signalID", signalID.ToString());

		//delete related triggers
		SqliteTrigger.DeleteByModeID(false, signalID);
	}
	
	void on_button_encoder_export_all_curves_clicked (object o, EventArgs args) 
	{
		checkFile(Constants.EncoderCheckFileOp.CAPTURE_EXPORT_ALL);
	}

	// encoder session overview

	void on_menuitem_encoder_session_overview_activate (object o, EventArgs args) 
	{
		EncoderOverviewWindow.Show (app1, currentEncoderGI, currentSession.UniqueID);
	}
	
	void on_button_encoder_export_all_curves_file_selected (string selectedFileName) 
	{
		string analysisOptions = getEncoderAnalysisOptions();

		string displacedMass = Util.ConvertToPoint( lastEncoderSQLSignal.extraWeight + (
					getExercisePercentBodyWeightFromName(lastEncoderSQLSignal.exerciseName) *
					currentPersonSession.Weight
					) );	
		
		EncoderParams ep = new EncoderParams(
				lastEncoderSQLSignal.minHeight, 
				getExercisePercentBodyWeightFromName (lastEncoderSQLSignal.exerciseName),
				Util.ConvertToPoint(findMass(Constants.MassType.BODY)),
				Util.ConvertToPoint(findMass(Constants.MassType.EXTRA)),
				findEccon(false), //do not force ecS (ecc-conc separated) //not taken from lastEncoderSQLSignal because there is (true)
				"exportCSV",
				"none",						//analysisVariables (not needed in create curves). Cannot be blank
				analysisOptions,
				preferences.encoderCaptureCheckFullyExtended,
				preferences.encoderCaptureCheckFullyExtendedValue,
				encoderConfigurationCurrent,
				Util.ConvertToPoint(preferences.encoderSmoothCon),	//R decimal: '.'
				-1,
				image_encoder_width,
				image_encoder_height,
				preferences.CSVExportDecimalSeparator 
				);

		string dataFileName = UtilEncoder.GetEncoderDataTempFileName();

		EncoderStruct encoderStruct = new EncoderStruct(
				dataFileName, 
				UtilEncoder.GetEncoderGraphTempFileName(),
				Util.GetEncoderExportTempFileName(), 
				UtilEncoder.GetEncoderScriptsPathWithoutLastSep(),
				UtilEncoder.GetEncoderTempPathWithoutLastSep(),
				ep);

		encoderRProcAnalyze.ExportFileName = selectedFileName;

		encoderRProcAnalyze.SendData(
				Util.ChangeSpaceAndMinusForUnderscore(currentPerson.Name) + "-" + 
				Util.ChangeSpaceAndMinusForUnderscore(lastEncoderSQLSignal.exerciseName) + 
					"-(" + displacedMass + "Kg)",
				false, 			//do not use neuromuscularProfile script
				preferences.RGraphsTranslate,
				new TriggerList()
				);
		encoderRProcAnalyze.StartOrContinue(encoderStruct);

		//encoder_pulsebar_capture.Text = string.Format(Catalog.GetString(
		//			"Exported to {0}."), UtilEncoder.GetEncoderExportTempFileName());
	}
						
	void on_button_encoder_save_AB_file_selected (string selectedFileName)
	{
		int msa = Convert.ToInt32(hscale_encoder_analyze_a.Value);
		int msb = Convert.ToInt32(hscale_encoder_analyze_b.Value);
		
		eai.ExportToCSV(msa, msb, selectedFileName, preferences.CSVExportDecimalSeparator);
	}

	string exportFileName;	
	protected void checkFile (Constants.EncoderCheckFileOp checkFileOp)
	{
		string exportString = ""; 
		if(checkFileOp == Constants.EncoderCheckFileOp.CAPTURE_EXPORT_ALL)
			exportString = Catalog.GetString ("Export set in CSV format");
		else if(checkFileOp == Constants.EncoderCheckFileOp.ANALYZE_SAVE_IMAGE)
			exportString = Catalog.GetString ("Save image");
		else if(checkFileOp == Constants.EncoderCheckFileOp.ANALYZE_SAVE_AB)
			exportString = Catalog.GetString ("Export repetition in CSV format");
		else if(checkFileOp == Constants.EncoderCheckFileOp.ANALYZE_SAVE_TABLE)
			exportString = Catalog.GetString ("Save table");
		
		string nameString = currentPerson.Name + "_" + currentSession.DateShortAsSQL;
		if(checkFileOp == Constants.EncoderCheckFileOp.CAPTURE_EXPORT_ALL)
			nameString += "_encoder_set_export.csv";
		else if(checkFileOp == Constants.EncoderCheckFileOp.ANALYZE_SAVE_IMAGE)
			nameString += ".png";
		else if(checkFileOp == Constants.EncoderCheckFileOp.ANALYZE_SAVE_AB)
			nameString += "_encoder_repetition_export.csv";
		else if(checkFileOp == Constants.EncoderCheckFileOp.ANALYZE_SAVE_TABLE)
			nameString += "_encoder_curves_table.csv";
		
		Gtk.FileChooserDialog fc=
			new Gtk.FileChooserDialog(exportString,
					app1,
					FileChooserAction.Save,
					Catalog.GetString("Cancel"),ResponseType.Cancel,
					Catalog.GetString("Accept"),ResponseType.Accept
					);
		fc.CurrentName = nameString;

		if (fc.Run() == (int)ResponseType.Accept) 
		{
			exportFileName = fc.Filename;
			//add ".csv" if needed
			if(checkFileOp == Constants.EncoderCheckFileOp.CAPTURE_EXPORT_ALL ||
					checkFileOp == Constants.EncoderCheckFileOp.ANALYZE_SAVE_AB ||
					checkFileOp == Constants.EncoderCheckFileOp.ANALYZE_SAVE_TABLE)
				exportFileName = Util.AddCsvIfNeeded(exportFileName);
			else
				exportFileName = Util.AddPngIfNeeded(exportFileName);
			try {
				if (File.Exists(exportFileName)) {
					LogB.Information(string.Format(
								"File {0} exists with attributes {1}, created at {2}", 
								exportFileName, 
								File.GetAttributes(exportFileName), 
								File.GetCreationTime(exportFileName)));
					LogB.Information("Overwrite...");
					ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString(
								"Are you sure you want to overwrite file: "), "", 
							exportFileName);

					if(checkFileOp == Constants.EncoderCheckFileOp.CAPTURE_EXPORT_ALL)
						confirmWin.Button_accept.Clicked += 
							new EventHandler(on_overwrite_file_export_all_curves_accepted);
					else if(checkFileOp == Constants.EncoderCheckFileOp.ANALYZE_SAVE_IMAGE)
						confirmWin.Button_accept.Clicked += 
							new EventHandler(on_overwrite_file_encoder_save_image_accepted);
					else if(checkFileOp == Constants.EncoderCheckFileOp.ANALYZE_SAVE_AB)
						confirmWin.Button_accept.Clicked += 
							new EventHandler(on_overwrite_file_encoder_save_AB_accepted);
					else if(checkFileOp == Constants.EncoderCheckFileOp.ANALYZE_SAVE_TABLE)
						confirmWin.Button_accept.Clicked += 
							new EventHandler(on_overwrite_file_encoder_save_table_accepted);

				} else {
					if(checkFileOp == Constants.EncoderCheckFileOp.CAPTURE_EXPORT_ALL)
						on_button_encoder_export_all_curves_file_selected (exportFileName);
					else if(checkFileOp == Constants.EncoderCheckFileOp.ANALYZE_SAVE_IMAGE)
						on_button_encoder_save_image_file_selected (exportFileName);
					else if(checkFileOp == Constants.EncoderCheckFileOp.ANALYZE_SAVE_AB)
						on_button_encoder_save_AB_file_selected (exportFileName);
					else if(checkFileOp == Constants.EncoderCheckFileOp.ANALYZE_SAVE_TABLE)
						on_button_encoder_save_table_file_selected (exportFileName);

					string myString = string.Format(Catalog.GetString("Saved to {0}"), 
							exportFileName);
					if(checkFileOp == Constants.EncoderCheckFileOp.CAPTURE_EXPORT_ALL ||
							checkFileOp == Constants.EncoderCheckFileOp.ANALYZE_SAVE_AB)
						myString += Constants.GetSpreadsheetString(preferences.CSVExportDecimalSeparator);
					new DialogMessage(Constants.MessageTypes.INFO, myString);
				}
			} catch {
				string myString = string.Format(
						Catalog.GetString("Cannot save file {0} "), exportFileName);
				new DialogMessage(Constants.MessageTypes.WARNING, myString);
			}
		}
		else {
			LogB.Information("cancelled");
			//report does not currently send the appBar reference
			new DialogMessage(Constants.MessageTypes.INFO, Catalog.GetString("Cancelled."));
			fc.Hide ();
			return ;
		}
		
		//Don't forget to call Destroy() or the FileChooserDialog window won't get closed.
		fc.Destroy();
		
		return;
	}
	private void on_overwrite_file_export_all_curves_accepted(object o, EventArgs args)
	{
		on_button_encoder_export_all_curves_file_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), 
				exportFileName) + Constants.GetSpreadsheetString(preferences.CSVExportDecimalSeparator);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}
	private void on_overwrite_file_encoder_save_image_accepted(object o, EventArgs args)
	{
		on_button_encoder_save_image_file_selected (exportFileName);

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
		on_button_encoder_save_table_file_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), exportFileName);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
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
		EncoderSQL eSQL = (EncoderSQL) SqliteEncoder.Select(
				false, Convert.ToInt32(encoderSignalUniqueID), 0, 0, Constants.EncoderGI.ALL,
				-1, "signal", EncoderSQL.Eccons.ALL, false, true)[0];

		//delete signal and related curves (both from SQL and files)
		encoderSignalDelete(eSQL.GetFullURL(false), Convert.ToInt32(encoderSignalUniqueID));
	
		removeSignalFromGuiBecauseDeletedOrCancelled();

		encoder_pulsebar_capture.Text = Catalog.GetString("Set deleted");
		textview_encoder_signal_comment.Buffer.Text = "";
	}
	void removeSignalFromGuiBecauseDeletedOrCancelled() 
	{
		encoderSignalUniqueID = "-1";
		image_encoder_capture.Sensitive = false;
		treeviewEncoderCaptureRemoveColumns();
		UtilGtk.ErasePaint(encoder_capture_curves_bars_drawingarea, encoder_capture_curves_bars_pixmap);
		
		encoderButtonsSensitive(encoderSensEnum.DONENOSIGNAL);
		
		//need this because DONENOSIGNAL allows to recalculate with different parameters, 
		//but when deleted or cancelled, then don't allow
		button_encoder_recalculate.Sensitive = false;
		
		encoder_capture_curves_bars_drawingarea.Visible = false;
	}


	private void updateUserCurvesLabelsAndCombo(bool dbconOpened) 
	{
		label_encoder_user_curves_active_num.Text = encSelReps.RepsActive.ToString();
		label_encoder_user_curves_all_num.Text = encSelReps.RepsAll.ToString();
		
		if(radio_encoder_analyze_individual_current_set.Active)
			updateComboEncoderAnalyzeCurveNumFromCurrentSet ();
		else if(radio_encoder_analyze_individual_current_session.Active) 
		{
			ArrayList data = SqliteEncoder.Select(
					dbconOpened, -1, currentPerson.UniqueID, currentSession.UniqueID, getEncoderGI(),
					getExerciseIDFromCombo(exerciseCombos.ANALYZE),
					"curve", EncoderSQL.Eccons.ALL, 
					false, true);
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
			activeCurvesList = new String[rows +1];
			activeCurvesList[0] = Catalog.GetString("All");
			for(int i=1; i <= rows; i++)
				activeCurvesList[i] = i.ToString();
			defaultValue = 1;
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
		string desc = "";
		if(mode == "curve") {
			EncoderCurve curve = treeviewEncoderCaptureCurvesGetCurve(selectedID,true);

			//some start at ,5 because of the spline filtering
			int curveStart = Convert.ToInt32(decimal.Truncate(Convert.ToDecimal(curve.Start)));

			int duration = Convert.ToInt32(decimal.Truncate(Convert.ToDecimal(curve.Duration)));

			meanPowerStr = curve.MeanPower;

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
			
				meanPowerStr = curveNext.MeanPower; //take power of concentric phase
			}
			
			/*
			 * at inertial signals, first curve is eccentric (can be to left or right, maybe positive or negative)
			 * graph.R manages correctly this
			 * But, when saved a curve, eg. concentric this can be positive or negative
			 * (depending on the rotating sign of inertial machine at that curve)
			 * if it's concentric, and it's full of -1,-2,... we have to change sign
			 * if it's eccentric-concentric, and in the eccentric phase is positive, then we should change sign of both phases
			 */
			int inertialCheckStart = 0;
			int inertialCheckDuration = 0;
			if(encoderConfigurationCurrent.has_inertia) {
				inertialCheckStart = curveStart;
				if(ecconLast == "c")
					inertialCheckDuration = duration;
				else {
					//see if sign is ok just looking if eccentric phase is negative or not
					inertialCheckDuration = Convert.ToInt32(decimal.Truncate(Convert.ToDecimal(curve.Duration)));
				}
			}
		
			desc = Util.RemoveTildeAndColonAndDot(textview_encoder_signal_comment.Buffer.Text);

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
			SqliteEncoder.SignalCurveInsert(dbconOpened, 
					Convert.ToInt32(encoderSignalUniqueID), curveIDMax +1,
					Convert.ToInt32(curveStart + (duration /2)));

			path = UtilEncoder.GetEncoderSessionDataCurveDir(currentSession.UniqueID);
		} else { //signal
			desc = Util.RemoveTildeAndColonAndDot(textview_encoder_signal_comment.Buffer.Text);

			fileSaved = UtilEncoder.CopyTempToEncoderData (currentSession.UniqueID, currentPerson.UniqueID, 
					currentPerson.Name, encoderTimeStamp);
			
			//there was a problem copying
			if(fileSaved == "")
				return "";

			path = UtilEncoder.GetEncoderSessionDataSignalDir(currentSession.UniqueID);
		}
		
		string myID = "-1";	
		if(mode == "signal")
			myID = encoderSignalUniqueID;

		//assign values from lastEncoderSQLSignal (last calculate curves or reload), and change new things
		EncoderSQL eSQL = lastEncoderSQLSignal;
		eSQL.uniqueID = myID;
		eSQL.signalOrCurve = signalOrCurve;
		eSQL.filename = fileSaved;
		eSQL.url = path;
		eSQL.description = desc;
		if(mode == "curve") {
			eSQL.status = "active";
			eSQL.future1 = meanPowerStr;
		}

		eSQL.encoderConfiguration = encoderConfigurationCurrent;

		//if is a signal that we just loaded, then don't insert, do an update
		//we know it because encoderUniqueID is != than "-1" if we loaded something from database
		//This also saves curves
		if(myID == "-1") {
			myID = SqliteEncoder.Insert(dbconOpened, eSQL).ToString(); //Adding on SQL
			if(mode == "signal") {
				encoderSignalUniqueID = myID;
				feedback = Catalog.GetString("Set saved");
			
				viewport_video_play_encoder.Sensitive = false;
				//copy video	
				if(preferences.videoOn) {
					if(Util.CopyTempVideo(currentSession.UniqueID, 
								Constants.TestTypes.ENCODER, 
								Convert.ToInt32(encoderSignalUniqueID))) {
						eSQL.videoURL = Util.GetVideoFileName(currentSession.UniqueID, 
								Constants.TestTypes.ENCODER, 
								Convert.ToInt32(encoderSignalUniqueID));
						//need assign uniqueID to update and add the URL of video
						eSQL.uniqueID = encoderSignalUniqueID;
						SqliteEncoder.Update(dbconOpened, eSQL);
					
						//notebook_video_encoder.CurrentPage = 1;
						radiobutton_video_encoder_play.Active  = true;
						
						viewport_video_play_encoder.Sensitive = true;

					} else {
						new DialogMessage(Constants.MessageTypes.WARNING, 
								Catalog.GetString("Sorry, video cannot be stored."));
					}
				}
			}
		}
		else {
			LogB.Warning("TOSTRING1");
			eSQL.ToString();
			//only signal is updated
			SqliteEncoder.Update(dbconOpened, eSQL); //Adding on SQL
			LogB.Warning("TOSTRING2");
			eSQL.ToString();
			feedback = Catalog.GetString("Set updated");
		}
		
		LogB.Debug("At encoderSaveSignalOrCurve done");
		return feedback;
	}


	private void on_button_encoder_analyze_clicked (object o, EventArgs args) 
	{
		//if userCurves and no data, return
		if( ! radio_encoder_analyze_individual_current_set.Active)
		{
			ArrayList data = SqliteEncoder.Select(
					false, -1, currentPerson.UniqueID, currentSession.UniqueID, getEncoderGI(),
					getExerciseIDFromCombo(exerciseCombos.ANALYZE), "curve", EncoderSQL.Eccons.ALL, 
					false, true);

			if(data.Count == 0) {
				new DialogMessage(Constants.MessageTypes.WARNING, 
						Catalog.GetString("Sorry, no repetitions selected."));
				return;
			}
		
			//cannot do inter/intra person with some cross graphs
			if(encoderAnalysis == "cross") 
			{
				string nameTemp = Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_analyze_cross),
						encoderAnalyzeCrossTranslation);

				if( (radio_encoder_analyze_individual_all_sessions.Active ||
						radio_encoder_analyze_groupal_current_session.Active) &&
						(
						 nameTemp == "Speed,Power / Load" || 
						 nameTemp == Catalog.GetString("Speed,Power / Load")
						)) {
					new DialogMessage(Constants.MessageTypes.WARNING, 
							Catalog.GetString("Sorry, this graph is not supported yet.") +
							"\n\nIntersession or Interperson - cross variables" +
							"\n- Speed,Power / Load"
							);

					return;
				}
			}
			
			//cannot do inter/intra person with some 1RM graphs
			if(encoderAnalysis == "1RM") 
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
				
				//cannot do 1RM with different exercises
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

						string exName = eSQL.exerciseName;
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
			}
		}
	
		button_encoder_analyze.Visible = false;
		hbox_encoder_analyze_progress.Visible = true;

		encoderThreadStart(encoderActions.ANALYZE);
	}

	//this is called by non gtk thread. Don't do gtk stuff here
	//I suppose reading gtk is ok, changing will be the problem
	private void encoderDoCaptureBG ()
	{
		eCaptureInertialBG.CaptureBG(
			currentSession.Name == Constants.SessionSimulatedName && testsActive
			);
	}

	private void stopCapturingInertialBG()
	{
		LogB.Information("Stopping capturing Inertial BG");
		eCaptureInertialBG.FinishBG();
		EncoderCaptureInertialBackgroundStatic.Abort();
		eCaptureInertialBG = null;
		vscale_encoder_capture_inertial_angle_now.Value = 0;
	}

	//this is called by non gtk thread. Don't do gtk stuff here
	//I suppose reading gtk is ok, changing will be the problem
	private void encoderDoCaptureCsharp () 
	{
		bool cutByTriggers = false;
		//triggers only work on gravitatory, concentric
		if(preferences.encoderCaptureCutByTriggers &&
				currentEncoderGI == Constants.EncoderGI.GRAVITATORY && eCapture.Eccon == "c")
			cutByTriggers = true;

		encoderRProcCapture.CutByTriggers = cutByTriggers;

		bool capturedOk = eCapture.Capture(
				UtilEncoder.GetEncoderDataTempFileName(),
				encoderRProcCapture,
				configChronojump.Compujump,
				cutByTriggers
				);

		//wait to ensure capture thread has ended
		Thread.Sleep(50);	
		
		//on simulated sleep more to ensure data is written to disc
		if(currentSession.Name == Constants.SessionSimulatedName && testsActive)
			Thread.Sleep(1500);

		LogB.Debug("Going to stop");		
		capturingCsharp = encoderCaptureProcess.STOPPING;

		//will start calcule curves thread
		if(capturedOk)
		{
			if(radio_encoder_capture_cont.Active && ! captureContWithCurves)
			{
				LogB.Debug("Don't need to to encoderCalculeCurves");
				encoderTimeStamp = UtilDate.ToFile(DateTime.Now);
				encoderSignalUniqueID = "-1"; //mark to know that there's no ID for this until it's saved on database
				setLastEncoderSQLSignal();
			} else
			{
				LogB.Debug("Going to encoderCalculeCurves");
				encoderCalculeCurves(encoderActions.CURVES_AC);
			}
		} else
			encoderProcessCancel = true;
	}
	
	//this is called by non gtk thread. Don't do gtk stuff here
	//don't change properties like setting a Visibility status: Gtk.Widget.set_Visible
	//I suppose reading gtk is ok, changing will be the problem
	private void encoderDoCaptureCsharpIM () 
	{
		encoderRProcCapture.CutByTriggers = false; //do not cutByTriggers on inertial, yet.

		bool capturedOk = eCapture.Capture(
				UtilEncoder.GetEncoderDataTempFileName(),
				encoderRProcCapture,
				false, 	//compujump
				false 	//cutByTriggers
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
		//but we don't want to change encoderAnalysis because we want to know again if == "cross" (or "1RM")
		//encoderAnalysis can be "cross" and sendAnalysis be "Speed / Load"
		//encoderAnalysis can be "1RM" and sendAnalysis be "1RMBadilloBench, ...
		string sendAnalysis = encoderAnalysis;

		//see doProcess at encoder/graph.R
		string analysisVariables = "none"; //cannot be blank

		string crossName = "";
		if(sendAnalysis == "cross") {
			crossName = Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_analyze_cross),
						encoderAnalyzeCrossTranslation);
			
			if(
					crossName == "Power / Load" || crossName == "Speed / Load" || 
					crossName == "Force / Load" || crossName == "Speed,Power / Load" || 
					crossName == "Force / Speed"|| crossName == "Power / Speed" )
			{
				//convert: "Force / Speed" in: "Force;Speed;mean"
				string [] crossNameFull = crossName.Split(new char[] {' '});
				analysisVariables = crossNameFull[0] + ";" + crossNameFull[2]; //[1]=="/"
				if(check_encoder_analyze_mean_or_max.Active)
					analysisVariables += ";mean";
				else
					analysisVariables += ";max";
			} 
			else if (crossName == "Power / Date" || crossName == "Speed / Date" || crossName == "Force / Date" ) 
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
		
		if(sendAnalysis == "powerBars" || sendAnalysis == "single" || sendAnalysis == "side")
			analysisVariables = getAnalysisVariables(sendAnalysis);

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
			if(encoderAnalysis == "neuromuscularProfile") {
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
						getExerciseIDFromCombo(exerciseCombos.ANALYZE),
						"curve", ecconSelect, 
						false, true);
			}
			else if(radio_encoder_analyze_groupal_current_session.Active) 
			{
				for (int i=0 ; i < encSelReps.EncoderCompareInter.Count ; i ++) {
					ArrayList dataPre = SqliteEncoder.Select(
							false, -1, 
							Util.FetchID(encSelReps.EncoderCompareInter[i].ToString()),
							currentSession.UniqueID,
							getEncoderGI(),
							getExerciseIDFromCombo(exerciseCombos.ANALYZE),
							"curve", EncoderSQL.Eccons.ALL, 
							false, //onlyActive=false. Means: all saved repetitions
							true);
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
							getExerciseIDFromCombo(exerciseCombos.ANALYZE),
							"curve", EncoderSQL.Eccons.ALL,
							false, //onlyActive=false. Means: all saved repetitions
							true);
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
			if(encoderAnalysis == "1RM" &&
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
					EncoderExercise exTemp = (EncoderExercise) SqliteEncoder.SelectEncoderExercises(
						false , exID, false)[0];
				
					sendAnalysis = "1RMAnyExercise";
				        analysisVariables = Util.ConvertToPoint(exTemp.speed1RM) + ";" +
						SqlitePreferences.Select("encoder1RMMethod");
					analysisOptions = "p";
				}
			}

			//-1 because data will be different on any curve
			ep = new EncoderParams(
					-1, 
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
		
			ArrayList eeArray = SqliteEncoder.SelectEncoderExercises(false, -1, false);
			EncoderExercise ex = new EncoderExercise();
						
			LogB.Information("AT ANALYZE");

			int iteratingPerson = -1;
			int iteratingSession = -1;
			double iteratingMassBody = -1;
			int countSeries = 1;

			Sqlite.Open();	
			foreach(EncoderSQL eSQL in data) {
				foreach(EncoderExercise eeSearch in eeArray)
					if(eSQL.exerciseID == eeSearch.uniqueID)
						ex = eeSearch;

				LogB.Debug(" AT ANALYZE 1.1 ");
				//massBody change if we are comparing different persons or sessions
				if(eSQL.personID != iteratingPerson || eSQL.sessionID != iteratingSession) {
					iteratingMassBody = SqlitePersonSession.SelectAttribute(
							true, eSQL.personID, eSQL.sessionID, Constants.Weight);
				}
				LogB.Debug(" AT ANALYZE 1.2 ");

				//seriesName
				string seriesName = "";
				if(radio_encoder_analyze_groupal_current_session.Active)
				{
					foreach(string str in encSelReps.EncoderCompareInter)
						if(Util.FetchID(str) == eSQL.personID)
							seriesName = Util.FetchName(str);
				} else if(radio_encoder_analyze_individual_all_sessions.Active)
				{
					foreach(string str in encSelReps.EncoderCompareInter) {
						LogB.Information(str);
						if(Util.FetchID(str) == eSQL.sessionID)
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
						eSQL.GetDate(true) + "," + 
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
			if(encoderAnalysis == "1RM") {
				if(my1RMName == "1RM Any exercise") {
					//get speed1RM (from combo)
					EncoderExercise ex = (EncoderExercise) SqliteEncoder.SelectEncoderExercises(
							false, getExerciseIDFromCombo(exerciseCombos.CAPTURE), false)[0];

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
			int curveNum = 0; //all
			if(Util.IsNumber(UtilGtk.ComboGetActive(combo_encoder_analyze_curve_num_combo), false))
				curveNum = Convert.ToInt32(UtilGtk.ComboGetActive(combo_encoder_analyze_curve_num_combo));

			ep = new EncoderParams(
					preferences.EncoderCaptureMinHeight(encoderConfigurationCurrent.has_inertia), 
					getExercisePercentBodyWeightFromComboCapture (),
					Util.ConvertToPoint(findMass(Constants.MassType.BODY)),
					Util.ConvertToPoint(findMass(Constants.MassType.EXTRA)),
					findEccon(false),		//do not force ecS (ecc-conc separated)
					sendAnalysis,
					analysisVariables, 
					analysisOptions,
					preferences.encoderCaptureCheckFullyExtended,
					preferences.encoderCaptureCheckFullyExtendedValue,
					encoderConfigurationCurrent,
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


		string titleStr = Util.ChangeSpaceAndMinusForUnderscore(currentPerson.Name);
	
		if(encoderAnalysis == "neuromuscularProfile")
			titleStr = "Neuromuscular Profile" + "-" + titleStr;
		else {
			//on signal show encoder exercise, but not in curves because every curve can be of a different exercise
			if(radio_encoder_analyze_individual_current_set.Active) //current set
				titleStr += "-" + Util.ChangeSpaceAndMinusForUnderscore(UtilGtk.ComboGetActive(combo_encoder_exercise_capture));
		}

		//triggers only on concentric
		if(triggerList == null || findEccon(false) != "c")
			triggerList = new TriggerList();

		encoderRProcAnalyze.SendData(
				titleStr, 
				encoderAnalysis == "neuromuscularProfile",
				preferences.RGraphsTranslate,
				triggerList
				);

		encoderRProcAnalyze.StartOrContinue(encoderStruct);
	}
		
	/*
	 * 1) neuromuscular should be separated
	 * 2) if we are analyzing current set and it's concentric separate phases button has to be unsensitive
	 * 3) single and side are together
	 */
	private void block_check_encoder_analyze_eccon_together_if_needed() 
	{
		if(radiobutton_encoder_analyze_neuromuscular_profile.Active) { // 1)
			//separated, mandatory
			check_encoder_analyze_eccon_together.Sensitive = false;
			check_encoder_analyze_eccon_together.Active = false;
		}
		else if( 
				( radio_encoder_analyze_individual_current_set.Active && findEccon(false) == "c" ) || // 2)
				radiobutton_encoder_analyze_single.Active || radiobutton_encoder_analyze_side.Active // 3)
		  ) {
			//together, mandatory
			check_encoder_analyze_eccon_together.Sensitive = false;
			check_encoder_analyze_eccon_together.Active = true;
		}
	}
	
	private void on_radio_encoder_analyze_individual_current_set_toggled (object obj, EventArgs args) 
	{
		if(! radio_encoder_analyze_individual_current_set.Active)
			return;
		
		//not called here
		//prepareAnalyzeRepetitions();
		
	
		createComboAnalyzeCross(false, false); //first creation: false, dateOnX: false
		createComboEncoderAnalyzeWeights(false); //first creation: false

		updateComboEncoderAnalyzeCurveNumFromCurrentSet ();

		hbox_encoder_user_curves.Visible = false;
		hbox_combo_encoder_exercise_analyze.Visible = false;
		
		//this analysis only when not comparing
		radiobutton_encoder_analyze_powerbars.Visible = true;
		radiobutton_encoder_analyze_1RM.Visible = (currentEncoderGI == Constants.EncoderGI.GRAVITATORY);
		radiobutton_encoder_analyze_single.Visible = true;
		radiobutton_encoder_analyze_side.Visible = true;
		radiobutton_encoder_analyze_neuromuscular_profile.Visible = (currentEncoderGI == Constants.EncoderGI.GRAVITATORY);

		check_encoder_analyze_eccon_together.Sensitive = true;
		block_check_encoder_analyze_eccon_together_if_needed();
		hbox_encoder_analyze_intersession.Visible = false;
			
		button_encoder_analyze_sensitiveness();
	
		hbox_encoder_analyze_current_signal.Visible = true;

		showTriggersAndTab();
	}
	
	private void on_radio_encoder_analyze_individual_current_session_toggled (object obj, EventArgs args) 
	{
		if(! radio_encoder_analyze_individual_current_session.Active)
			return;
		
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
		
		hbox_encoder_user_curves.Visible = currentPerson != null;
		
		hbox_combo_encoder_exercise_analyze.Visible = true;

		//this analysis only when not comparing
		radiobutton_encoder_analyze_powerbars.Visible = true;
		radiobutton_encoder_analyze_1RM.Visible = (currentEncoderGI == Constants.EncoderGI.GRAVITATORY);
		radiobutton_encoder_analyze_single.Visible = true;
		radiobutton_encoder_analyze_side.Visible = true;
		radiobutton_encoder_analyze_neuromuscular_profile.Visible = (currentEncoderGI == Constants.EncoderGI.GRAVITATORY);

		check_encoder_analyze_eccon_together.Sensitive = true;
		block_check_encoder_analyze_eccon_together_if_needed();
		hbox_encoder_analyze_intersession.Visible = false;
			
		button_encoder_analyze_sensitiveness();
	
		hbox_encoder_analyze_current_signal.Visible = false;

		showTriggerTab(false);
	}
	
	private void on_radio_encoder_analyze_individual_all_sessions_toggled (object obj, EventArgs args) 
	{
		if(! radio_encoder_analyze_individual_all_sessions.Active)
			return;
		
		prepareAnalyzeRepetitions();
	
		hbox_encoder_analyze_current_signal.Visible = false;
		
		createComboAnalyzeCross(false, check_encoder_intersession_x_is_date.Active);
		combo_encoder_analyze_weights.Visible = check_encoder_intersession_x_is_date.Active;

		hbox_encoder_user_curves.Visible = currentPerson != null;
		
		hbox_combo_encoder_exercise_analyze.Visible = true;
		
		//active cross. The only available for comparing	
		radiobutton_encoder_analyze_cross.Active = true;
		hbox_encoder_analyze_intersession.Visible = true;
		
		//this analysis only when not comparing
		radiobutton_encoder_analyze_powerbars.Visible = false;
		radiobutton_encoder_analyze_1RM.Visible = false;
		radiobutton_encoder_analyze_single.Visible = false;
		radiobutton_encoder_analyze_side.Visible = false;
		radiobutton_encoder_analyze_neuromuscular_profile.Visible = false;

		showTriggerTab(false);
	}

	private void on_radio_encoder_analyze_groupal_current_session_toggled (object obj, EventArgs args) 
	{
		if(! radio_encoder_analyze_groupal_current_session.Active)
			return;
	
		prepareAnalyzeRepetitions();

		hbox_encoder_analyze_current_signal.Visible = false;
		
		createComboAnalyzeCross(false, false); //first creation: false, dateOnX: false
		
		hbox_encoder_user_curves.Visible = currentPerson != null;
		
		hbox_combo_encoder_exercise_analyze.Visible = true;
		
		//active cross. The only available for comparing	
		radiobutton_encoder_analyze_cross.Active = true;
		hbox_encoder_analyze_intersession.Visible = false;
		
		//this analysis only when not comparing
		radiobutton_encoder_analyze_powerbars.Visible = false;
		radiobutton_encoder_analyze_1RM.Visible = false;
		radiobutton_encoder_analyze_single.Visible = false;
		radiobutton_encoder_analyze_side.Visible = false;
		radiobutton_encoder_analyze_neuromuscular_profile.Visible = false;

		showTriggerTab(false);
	}


	private string getAnalysisVariables(string encoderAnalysis) 
	{
		string analysisVariables = "none"; //cannot be blank

		if(encoderAnalysis == "powerBars") {
			if(check_encoder_analyze_show_time_to_peak_power.Active)
				analysisVariables = "TimeToPeakPower";
			else
				analysisVariables = "NoTimeToPeakPower";

			if(check_encoder_analyze_show_range.Active)
				analysisVariables += ";Range";
			else
				analysisVariables += ";NoRange";
		}
		else {  //(encoderAnalysis == "single" || encoderAnalysis == "side")
			if(check_encoder_analyze_show_speed.Active)
				analysisVariables = "Speed";
			else
				analysisVariables = "NoSpeed";

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


	//encoder analysis modes

	private void on_radiobutton_encoder_analyze_single_toggled (object obj, EventArgs args) {
		hbox_encoder_analyze_curve_num.Visible=true;
		hbox_combo_encoder_analyze_curve_num_combo.Visible = true;
		hbox_combo_encoder_analyze_cross.Visible=false;
		hbox_combo_encoder_analyze_1RM.Visible=false;
		check_encoder_analyze_mean_or_max.Visible=false;
		hbox_encoder_analyze_show_powerbars.Visible=false;
		hbox_encoder_analyze_show_SAFE.Visible=true;
		encoderAnalysis = "single";
		
		//together, mandatory
		check_encoder_analyze_eccon_together.Sensitive=false;
		check_encoder_analyze_eccon_together.Active = true;
	
		button_encoder_analyze_help.Visible = false;
		label_encoder_analyze_side_max.Visible = false;

		//restore 1RM Bench Press sensitiveness
		check_encoder_analyze_mean_or_max.Sensitive = true;
		
		encoderButtonsSensitive(encoderSensEnumStored);
		button_encoder_analyze_sensitiveness();
	}

	/*
	private void on_radiobutton_encoder_analyze_superpose_toggled (object obj, EventArgs args) {
		hbox_encoder_analyze_curve_num.Visible=true;
		hbox_combo_encoder_analyze_curve_num_combo.Visible = true;
		hbox_combo_encoder_analyze_cross.Visible=false;
		hbox_combo_encoder_analyze_1RM.Visible=false;
		check_encoder_analyze_mean_or_max.Visible=false;
		hbox_encoder_analyze_show_powerbars.Visible=false;
		hbox_encoder_analyze_show_SAFE.Visible=true;
		encoderAnalysis="superpose";
		
		//together, mandatory
		check_encoder_analyze_eccon_together.Sensitive=false;
		check_encoder_analyze_eccon_together.Active = true;
		
		button_encoder_analyze_help.Visible = false;
		
		//restore 1RM Bench Press sensitiveness
		check_encoder_analyze_mean_or_max.Sensitive = true;
		
		button_encoder_analyze_sensitiveness();
	}
	*/
	private void on_radiobutton_encoder_analyze_side_toggled (object obj, EventArgs args) {
		hbox_encoder_analyze_curve_num.Visible=false;
		hbox_combo_encoder_analyze_curve_num_combo.Visible = false;
		hbox_combo_encoder_analyze_cross.Visible=false;
		hbox_combo_encoder_analyze_1RM.Visible=false;
		check_encoder_analyze_mean_or_max.Visible=false;
		hbox_encoder_analyze_show_powerbars.Visible=false;
		hbox_encoder_analyze_show_SAFE.Visible=true;
		encoderAnalysis = "side";
		
		//together, mandatory
		check_encoder_analyze_eccon_together.Sensitive=false;
		check_encoder_analyze_eccon_together.Active = true;

		button_encoder_analyze_help.Visible = false;
		
		//restore 1RM Bench Press sensitiveness
		check_encoder_analyze_mean_or_max.Sensitive = true;
		
		encoderButtonsSensitive(encoderSensEnumStored);
		button_encoder_analyze_sensitiveness();
	}
	private void on_radiobutton_encoder_analyze_powerbars_toggled (object obj, EventArgs args) {
		hbox_encoder_analyze_curve_num.Visible=false;
		hbox_combo_encoder_analyze_curve_num_combo.Visible = false;
		hbox_combo_encoder_analyze_cross.Visible=false;
		hbox_combo_encoder_analyze_1RM.Visible=false;
		check_encoder_analyze_mean_or_max.Visible=false;
		hbox_encoder_analyze_show_powerbars.Visible=true;
		hbox_encoder_analyze_show_SAFE.Visible=false;
		encoderAnalysis="powerBars";
		
		check_encoder_analyze_eccon_together.Sensitive=true;
		block_check_encoder_analyze_eccon_together_if_needed();

		button_encoder_analyze_help.Visible = false;
		label_encoder_analyze_side_max.Visible = false;

		//restore 1RM Bench Press sensitiveness
		check_encoder_analyze_mean_or_max.Sensitive = true;
		
		encoderButtonsSensitive(encoderSensEnumStored);
		button_encoder_analyze_sensitiveness();
	}
	
	private void on_radiobutton_encoder_analyze_cross_toggled (object obj, EventArgs args) {
		hbox_encoder_analyze_curve_num.Visible=false;
		hbox_combo_encoder_analyze_curve_num_combo.Visible = false;
		hbox_combo_encoder_analyze_cross.Visible=true;
		hbox_combo_encoder_analyze_1RM.Visible=false;
		check_encoder_analyze_mean_or_max.Visible=true;
		hbox_encoder_analyze_show_powerbars.Visible=false;
		hbox_encoder_analyze_show_SAFE.Visible=false;
		encoderAnalysis="cross";
		
		check_encoder_analyze_eccon_together.Sensitive=true;
		
		//block_check_encoder_analyze_eccon_together_if_needed();
		//done here:
		on_combo_encoder_analyze_cross_changed (obj, args);

		button_encoder_analyze_help.Visible = false;
		label_encoder_analyze_side_max.Visible = false;

		encoderButtonsSensitive(encoderSensEnumStored);
		button_encoder_analyze_sensitiveness();
	}
	
	private void on_radiobutton_encoder_analyze_1RM_toggled (object obj, EventArgs args) {
		hbox_encoder_analyze_curve_num.Visible=false;
		hbox_combo_encoder_analyze_curve_num_combo.Visible = false;
		hbox_combo_encoder_analyze_cross.Visible=false;
		hbox_combo_encoder_analyze_1RM.Visible=true;
		check_encoder_analyze_mean_or_max.Visible=true;
		hbox_encoder_analyze_show_powerbars.Visible=false;
		hbox_encoder_analyze_show_SAFE.Visible=false;
		encoderAnalysis="1RM";
		
		check_encoder_analyze_eccon_together.Sensitive=true;
		
		//block_check_encoder_analyze_eccon_together_if_needed();
		//done here:
		on_combo_encoder_analyze_1RM_changed (obj, args);

		button_encoder_analyze_help.Visible = false;
		label_encoder_analyze_side_max.Visible = false;

		encoderButtonsSensitive(encoderSensEnumStored);
		button_encoder_analyze_sensitiveness();
	}
	
	private void on_radiobutton_encoder_analyze_neuromuscular_profile_toggled (object obj, EventArgs args) {
		hbox_encoder_analyze_curve_num.Visible=false;
		hbox_combo_encoder_analyze_curve_num_combo.Visible = false;
		hbox_combo_encoder_analyze_cross.Visible=false;
		hbox_combo_encoder_analyze_1RM.Visible=false;
		check_encoder_analyze_mean_or_max.Visible=false;
		hbox_encoder_analyze_show_powerbars.Visible=false;
		hbox_encoder_analyze_show_SAFE.Visible=false;
		encoderAnalysis="neuromuscularProfile";
		
		//separated, mandatory
		check_encoder_analyze_eccon_together.Sensitive=false;
		check_encoder_analyze_eccon_together.Active = false;
	
		button_encoder_analyze_help.Visible = true;
		label_encoder_analyze_side_max.Visible = false;
		check_encoder_analyze_mean_or_max.Sensitive = false;
		
		encoderButtonsSensitive(encoderSensEnumStored);
		button_encoder_analyze_sensitiveness();
	}
	
	//end of encoder analysis modes

	private void on_check_encoder_analyze_eccon_together_toggled (object obj, EventArgs args) {
		image_encoder_analyze_eccon_together.Visible = check_encoder_analyze_eccon_together.Active;
		image_encoder_analyze_eccon_separated.Visible = ! check_encoder_analyze_eccon_together.Active;
	}
	
	private void on_check_encoder_analyze_mean_or_max_toggled (object obj, EventArgs args) {
		image_encoder_analyze_mean.Visible = check_encoder_analyze_mean_or_max.Active;
		image_encoder_analyze_max.Visible = ! check_encoder_analyze_mean_or_max.Active;
	}
	
	
	private void on_button_encoder_analyze_help_clicked (object obj, EventArgs args) {
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

			Catalog.GetString("Analysis uses the best three jumps using 'jump height' criterion.") + "\n\n" +
			Catalog.GetString("Lapuente and De Blas. Adapted from Wagner:") + "\nhttp://spartapoint.com/category/spartapoint-101/";
		
		new DialogMessage(Constants.MessageTypes.INFO, str);
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
	private double findMass(Constants.MassType massType) 
	{
		if(currentPersonSession == null)
			return 0;

		double extraWeight = spin_encoder_extra_weight.Value;
		if(encoderConfigurationCurrent.has_inertia)
			extraWeight = 0;

		if(massType == Constants.MassType.BODY)
			return currentPersonSession.Weight;
		else if(massType == Constants.MassType.EXTRA)
			return extraWeight;
		else //(massType == Constants.MassType.DISPLACED)
			return extraWeight + 
				( currentPersonSession.Weight * getExercisePercentBodyWeightFromComboCapture() ) / 100.0;
	}

	//this is used in 1RM return to substract the weight of the body (if used on exercise)
	private double massWithoutPerson(double massTotal, string exerciseName) {
		int percentBodyWeight = getExercisePercentBodyWeightFromName(exerciseName);
		if(currentPersonSession.Weight == 0 || percentBodyWeight == 0 || percentBodyWeight == -1)
			return massTotal;
		else
			return massTotal - (currentPersonSession.Weight * percentBodyWeight / 100.0);
	}

	private string findEccon(bool forceEcconSeparated)
	{
		if(radio_encoder_eccon_concentric.Active)
			return "c";
		else
		{
			if(forceEcconSeparated || ! check_encoder_analyze_eccon_together.Active)
				return "ecS";
			else 
				return "ec";
		}
		/*
		 * unavailable until find while concentric data on concentric is the same than in ecc-con,
		 * but is very different than in con-ecc
		else //Constants.ConcentricEccentric
		{
			if(forceEcconSeparated || ! check_encoder_analyze_eccon_together.Active)
				return "ceS";
			else 
				return "ce";
		}
		*/
	}
	
	/* encoder exercise stuff */
	
	
	string [] encoderExercisesTranslationAndBodyPWeight;
//	string [] encoderEcconTranslation;
//	string [] encoderLateralityTranslation;
	string [] encoderAnalyzeCrossTranslation;
	string [] encoderAnalyze1RMTranslation;
	
	protected void createEncoderCombos() 
	{
		//create combo exercises
		combo_encoder_exercise_capture = ComboBox.NewText ();
		combo_encoder_exercise_analyze = ComboBox.NewText ();
		
		createEncoderComboExerciseAndAnalyze();
		
		combo_encoder_exercise_capture.Changed += new EventHandler (on_combo_encoder_exercise_capture_changed);
		combo_encoder_exercise_analyze.Changed += new EventHandler (on_combo_encoder_exercise_analyze_changed);
		
		/* ConcentricEccentric
		 * unavailable until find while concentric data on concentric is the same than in ecc-con,
		 * but is very different than in con-ecc
		 */

		//create combo encoder anchorage
		combo_encoder_anchorage = Gtk.ComboBox.NewText();
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
		combo_encoder_analyze_1RM = ComboBox.NewText ();
		UtilGtk.ComboUpdate(combo_encoder_analyze_1RM, comboAnalyze1RMOptionsTranslated, "");
		combo_encoder_analyze_1RM.Active = UtilGtk.ComboMakeActive(combo_encoder_analyze_1RM, 
				Catalog.GetString(comboAnalyze1RMOptions[0]));
		combo_encoder_analyze_1RM.Changed += new EventHandler (on_combo_encoder_analyze_1RM_changed);


		//create combo analyze curve num combo
		//is not an spinbutton because values can be separated: "3,4,7,21"
		combo_encoder_analyze_curve_num_combo = ComboBox.NewText ();
		UtilGtk.ComboUpdate(combo_encoder_analyze_curve_num_combo, Util.StringToStringArray(""), "");
		
		
		//pack combos

		hbox_combo_encoder_exercise_capture.PackStart(combo_encoder_exercise_capture, true, true, 0);
		hbox_combo_encoder_exercise_capture.ShowAll();
		combo_encoder_exercise_capture.Sensitive = true;
		
		hbox_combo_encoder_exercise_analyze.PackStart(combo_encoder_exercise_analyze, true, true, 0);
		//hbox_combo_encoder_exercise_analyze.ShowAll(); //hbox will be shown only on intersession & interperson
		combo_encoder_exercise_analyze.ShowAll();
		combo_encoder_exercise_analyze.Sensitive = true;

		hbox_combo_encoder_anchorage.PackStart(combo_encoder_anchorage, false, true, 0);
		hbox_combo_encoder_anchorage.ShowAll();
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

		label_encoder_top_extra_mass.Text = entry_raspberry_extra_weight.Text + " Kg";

		if(label_encoder_1RM_percent.Text == "")
			label_encoder_top_1RM_percent.Text = "";
		else
			label_encoder_top_1RM_percent.Text = label_encoder_1RM_percent.Text + " %1RM";

		label_encoder_top_weights.Text = Catalog.GetString("Weights") + ": " + entry_encoder_im_weights_n.Text;
		label_encoder_top_im.Text = Catalog.GetString("Inertia M.") + ": " + label_encoder_im_total.Text;
	}
	
	//this is called also when an exercise is deleted to update the combo and the string []
	protected void createEncoderComboExerciseAndAnalyze() {
		ArrayList encoderExercises = SqliteEncoder.SelectEncoderExercises(false, -1, false);
		encoderExercisesTranslationAndBodyPWeight = new String [encoderExercises.Count];
		string [] exerciseNamesToCombo = new String [encoderExercises.Count];
		int i =0;
		foreach(EncoderExercise ex in encoderExercises) {
			string nameTranslated = Catalog.GetString(ex.name);
			encoderExercisesTranslationAndBodyPWeight[i] = 
				ex.uniqueID + ":" + ex.name + ":" + nameTranslated + ":" + ex.percentBodyWeight;
			exerciseNamesToCombo[i] = Catalog.GetString(ex.name);
			i++;
		}
		
		UtilGtk.ComboUpdate(combo_encoder_exercise_capture, exerciseNamesToCombo, "");
		combo_encoder_exercise_capture.Active = UtilGtk.ComboMakeActive(combo_encoder_exercise_capture, 
				Catalog.GetString(((EncoderExercise) encoderExercises[0]).name));
	
		exerciseNamesToCombo = addAllExercisesToComboExerciseAnalyze(exerciseNamesToCombo);
		
		UtilGtk.ComboUpdate(combo_encoder_exercise_analyze, exerciseNamesToCombo, "");
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
				"Power / Load", "Speed / Load", "Force / Load", "Speed,Power / Load", "Force / Speed", "Power / Speed"
			};
			comboAnalyzeCrossOptionsTranslated = new string [] { 
				Catalog.GetString("Power / Load"), Catalog.GetString("Speed / Load"), 
				Catalog.GetString("Force / Load"), Catalog.GetString("Speed,Power / Load"), 
				Catalog.GetString("Force / Speed"), Catalog.GetString("Power / Speed")
			}; //if added more, change the int in the 'for' below
			encoderAnalyzeCrossTranslation = new String [comboAnalyzeCrossOptions.Length];
			for(int j=0; j < 6 ; j++)
				encoderAnalyzeCrossTranslation[j] = 
					comboAnalyzeCrossOptions[j] + ":" + comboAnalyzeCrossOptionsTranslated[j];
		} else {
			//create combo analyze cross (variables)
			comboAnalyzeCrossOptions = new string [] { "Power / Date", "Speed / Date", "Force / Date" };
			comboAnalyzeCrossOptionsTranslated = new string [] { 
				Catalog.GetString("Power / Date"), 
				Catalog.GetString("Speed / Date"), 
				Catalog.GetString("Force / Date") 
			}; //if added more, change the int in the 'for' below
			encoderAnalyzeCrossTranslation = new String [comboAnalyzeCrossOptions.Length];
			for(int j=0; j < 3 ; j++)
				encoderAnalyzeCrossTranslation[j] = 
					comboAnalyzeCrossOptions[j] + ":" + comboAnalyzeCrossOptionsTranslated[j];
		}

		if(firstCreation)
			combo_encoder_analyze_cross = ComboBox.NewText ();

		UtilGtk.ComboUpdate(combo_encoder_analyze_cross, comboAnalyzeCrossOptionsTranslated, "");
		combo_encoder_analyze_cross.Active = UtilGtk.ComboMakeActive(combo_encoder_analyze_cross, 
				lastActive);

		if(firstCreation) {
			combo_encoder_analyze_cross.Changed += new EventHandler (on_combo_encoder_analyze_cross_changed);

			hbox_combo_encoder_analyze_cross.PackStart(combo_encoder_analyze_cross, true, true, 0);
			hbox_combo_encoder_analyze_cross.ShowAll(); 
			combo_encoder_analyze_cross.Sensitive = true;
			hbox_combo_encoder_analyze_cross.Visible = false; //do not show hbox at start
		}
	}
		
	private void createComboEncoderAnalyzeWeights(bool firstCreation) 
	{
		if(firstCreation)
			combo_encoder_analyze_weights = ComboBox.NewText ();
	
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

	void on_check_encoder_intersession_x_is_date_toggled (object o, EventArgs args) {
		createComboAnalyzeCross(false, check_encoder_intersession_x_is_date.Active);
		
		if(check_encoder_intersession_x_is_date.Active) {
			createComboEncoderAnalyzeWeights(false);
			combo_encoder_analyze_weights.Visible = true;
		} else
			combo_encoder_analyze_weights.Visible = false;
	}	


	void on_radio_encoder_eccon_toggled (object o, EventArgs args)
	{
		//those will be true again when loading a new encoder test or capturing
		treeview_encoder_capture_curves.Sensitive = false;
		hbox_encoder_capture_curves_save_all_none.Sensitive = false;

		check_encoder_analyze_eccon_together.Sensitive = true;
		block_check_encoder_analyze_eccon_together_if_needed();
		setEcconPixbuf();
	}

	void setEcconPixbuf()
	{
		Pixbuf pixbuf;
		if(radio_encoder_eccon_concentric.Active)
			pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "muscle-concentric.png");
		else
			pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "muscle-excentric-concentric.png");

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
			pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "laterality-right.png");
		else if(radio_encoder_laterality_l.Active)
			pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "laterality-left.png");
		else
			pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "laterality-both.png");

		image_top_laterality.Pixbuf = pixbuf;
	}

	void on_button_encoder_capture_curves_all_clicked (object o, EventArgs args) {
		encoderCaptureSaveCurvesAllNoneBest(Constants.EncoderAutoSaveCurve.ALL, 
				Constants.GetEncoderVariablesCapture(preferences.encoderCaptureMainVariable));
	}
	void on_button_encoder_capture_curves_best_clicked (object o, EventArgs args) {
		encoderCaptureSaveCurvesAllNoneBest(Constants.EncoderAutoSaveCurve.BEST,
				Constants.GetEncoderVariablesCapture(preferences.encoderCaptureMainVariable));
	}
	void on_button_encoder_capture_curves_none_clicked (object o, EventArgs args) {
		encoderCaptureSaveCurvesAllNoneBest(Constants.EncoderAutoSaveCurve.NONE,
				Constants.GetEncoderVariablesCapture(preferences.encoderCaptureMainVariable));
	}
	void on_button_encoder_capture_curves_4top_clicked (object o, EventArgs args) {
		encoderCaptureSaveCurvesAllNoneBest(Constants.EncoderAutoSaveCurve.FROM4TOPENULTIMATE,
				Constants.GetEncoderVariablesCapture(preferences.encoderCaptureMainVariable));
	}


	void on_combo_encoder_analyze_cross_changed (object o, EventArgs args)
	{
		if (! radiobutton_encoder_analyze_cross.Active)
			return;

		check_encoder_analyze_mean_or_max.Sensitive = true;
		check_encoder_analyze_eccon_together.Sensitive = true;
		block_check_encoder_analyze_eccon_together_if_needed();
			
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
	
	void on_button_encoder_analyze_image_save_clicked (object o, EventArgs args)
	{
		/* file is in:
		 * /tmp/chronojump-last-encoder-graph.png
		 * but if a capture curves has done, file is named the same
		 * make unsensitive the capture image after loading or capturing a new signal
		 * or changing person, loading session, ...
		 */

		checkFile(Constants.EncoderCheckFileOp.ANALYZE_SAVE_IMAGE);
	}
	void on_button_encoder_save_image_file_selected (string destination)
	{
		try {
			File.Copy(UtilEncoder.GetEncoderGraphTempFileName(), destination, true);
		} catch {
			string myString = string.Format(
					Catalog.GetString("Cannot save file {0} "), destination);
			new DialogMessage(Constants.MessageTypes.WARNING, myString);
		}
	}

	void on_button_encoder_analyze_table_save_clicked (object o, EventArgs args)
	{
		/* file is in:
		 * /tmp/chronojump-last-encoder-curves.txt
		 * but if a capture curves has done, file is named the same
		 * make unsensitive the capture table after loading or capturing a new signal
		 * or changing person, loading session, ...
		 * No problem. Is nice to play with seinsitiveness, but the reading will be from treeview and not from file
		 */

		checkFile(Constants.EncoderCheckFileOp.ANALYZE_SAVE_TABLE);
	}

	void on_button_encoder_save_table_file_selected (string destination)
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
							GetTreeviewEncoderAnalyzeHeaders(false), sep), false));
				//write curves rows
				ArrayList array = getTreeViewCurves(encoderAnalyzeListStore);
				foreach (EncoderCurve ec in array)
					writer.WriteLine(ec.ToCSV(false, preferences.CSVExportDecimalSeparator));
			}
			
			writer.Flush();
			writer.Close();
			((IDisposable)writer).Dispose();
		} catch {
			string myString = string.Format(
					Catalog.GetString("Cannot save file {0} "), destination);
			new DialogMessage(Constants.MessageTypes.WARNING, myString);
		}
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
			exerciseID = getExerciseIDFromCombo (exerciseCombos.CAPTURE);
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
				myString = string.Format(Catalog.GetString("1RM found: {0} Kg."), load1RM) + "\n" +
					string.Format(Catalog.GetString("Displaced body weight in this exercise: {0}%."),
							getExercisePercentBodyWeightFromTable()) + "\n" +
					string.Format(Catalog.GetString("Saved 1RM without displaced body weight: {0} Kg."),
							load1RMWithoutPerson);
			*/
		}
		else {
			exerciseID = getExerciseIDFromTable();

			SqliteEncoder.Insert1RM(false, currentPerson.UniqueID, currentSession.UniqueID,
					exerciseID, load1RM);

			myString = string.Format(Catalog.GetString("Saved 1RM: {0} Kg."), load1RM);
		}

		array1RMUpdate(false);
		encoder_change_displaced_weight_and_1RM ();
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}



	int getExerciseIDFromName (string name) {
		LogB.Information("getExerciseIDFromName for: " + name);

		//first try translated
		string idFound = Util.FindOnArray(':', 2, 0, name, 
				encoderExercisesTranslationAndBodyPWeight);
		if(Util.IsNumber(idFound, false))
			return Convert.ToInt32(idFound);
		
		//second try english
		idFound = Util.FindOnArray(':', 1, 0, name, 
				encoderExercisesTranslationAndBodyPWeight);
		if(Util.IsNumber(idFound, false))
			return Convert.ToInt32(idFound);
		
		//third, send error value
		return -1;
	}

	enum exerciseCombos { CAPTURE, ANALYZE }
	
	int getExerciseIDFromCombo (exerciseCombos combo) {
		if(combo == exerciseCombos.CAPTURE)
			return getExerciseIDFromName (UtilGtk.ComboGetActive(combo_encoder_exercise_capture));
		else
			return getExerciseIDFromName (UtilGtk.ComboGetActive(combo_encoder_exercise_analyze));
	}

	int getExerciseIDFromTable () {
		return getExerciseIDFromName (getExerciseNameFromTable());
	}
	
	string getExerciseNameFromTable () { //from first data row
		ArrayList array = getTreeViewCurves(encoderAnalyzeListStore);
		return ( (EncoderCurve) array[0] ).Exercise;
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

	void on_button_encoder_exercise_clicked (object o, EventArgs args)
	{
		encoder_exercise_show_hide (true);
	}
	void on_button_encoder_exercise_close_clicked (object o, EventArgs args)
	{
		encoder_exercise_show_hide (false);
	}
	private void encoder_exercise_show_hide (bool show)
	{
		if(show)
			notebook_encoder_capture_or_exercise_or_instructions.Page = 1;
		else
			notebook_encoder_capture_or_exercise_or_instructions.Page = 0;

		main_menu.Sensitive = ! show;
		hbox_encoder_sup_capture_analyze.Sensitive = ! show;
		notebook_session_person.Sensitive = ! show;
		hbox_encoder_configuration.Sensitive = ! show;
		hbox_encoder_capture_top.Sensitive = ! show;
		vpaned_encoder_capture_video_and_set_graph.Sensitive = ! show;
	}

	//info is now info and edit (all values can be changed), and detete (there's delete button)
	void on_button_encoder_exercise_edit_clicked (object o, EventArgs args) 
	{
		EncoderExercise ex = (EncoderExercise) SqliteEncoder.SelectEncoderExercises(
				false, getExerciseIDFromCombo(exerciseCombos.CAPTURE), false)[0];

		ArrayList bigArray = new ArrayList();

		ArrayList a1 = new ArrayList();
		ArrayList a2 = new ArrayList();
		ArrayList a3 = new ArrayList();
		ArrayList a4 = new ArrayList();
		ArrayList a5 = new ArrayList();

		string exerciseName = ex.name;
		if(ex.IsPredefined())
			exerciseName = Catalog.GetString(ex.name);

		//0 is the widgget to show; 1 is the editable; 2 id default value

		//name cannot be changed because we have to detect if new name already exists, check problems with translations,
		//but most important, if user can change name and then click delete, it will be a mess to confirm that the type "newname" or "oldname" will be deleted
		a1.Add(Constants.GenericWindowShow.ENTRY); a1.Add(false); a1.Add(exerciseName);
		bigArray.Add(a1);

		a2.Add(Constants.GenericWindowShow.SPININT); a2.Add(true); a2.Add("");
		bigArray.Add(a2);
		
		a3.Add(Constants.GenericWindowShow.ENTRY2); a3.Add(true); a3.Add(ex.ressistance);
		bigArray.Add(a3);
		
		a4.Add(Constants.GenericWindowShow.ENTRY3); a4.Add(true); a4.Add(ex.description);
		bigArray.Add(a4);
		
		a5.Add(Constants.GenericWindowShow.HBOXSPINDOUBLE2); a5.Add(true); a5.Add("");
		bigArray.Add(a5);
		
		
		genericWin = GenericWindow.Show(false, Catalog.GetString("Encoder exercise name:"), bigArray);
		genericWin.LabelSpinInt = Catalog.GetString("Displaced body weight") + " (%)";
		
		//genericWin.SetSpinRange(ex.percentBodyWeight, ex.percentBodyWeight); //done this because IsEditable does not affect the cursors
		genericWin.SetSpinRange(0, 100);
		genericWin.SetSpinValue(ex.percentBodyWeight);
		
		genericWin.LabelEntry2 = Catalog.GetString("Resistance");
		genericWin.LabelEntry3 = Catalog.GetString("Description");
		genericWin.LabelSpinDouble2 = Catalog.GetString("Speed at 1RM");
		genericWin.SetSpinDouble2Value(ex.speed1RM);
		genericWin.SetSpinDouble2Increments(0.001,0.1);
		/*
		 * Now this is in encoder configuration
		genericWin.LabelSpinInt2 = Catalog.GetString("Body angle") + " (º)";
		genericWin.SetSpin2Range(ex.bodyAngle,ex.bodyAngle); //done this because IsEditable does not affect the cursors
		genericWin.LabelSpinInt3 = Catalog.GetString("Weight angle") + " (º)";
		genericWin.SetSpin3Range(ex.weightAngle,ex.weightAngle); //done this because IsEditable does not affect the cursors
		*/
		genericWin.ShowButtonCancel(false);
		
		genericWin.ShowButtonDelete(true);
		genericWin.Button_delete.Clicked += new EventHandler(on_button_encoder_exercise_delete);
		
		genericWin.nameUntranslated = ex.name;
		genericWin.uniqueID = ex.uniqueID;
		
		genericWin.Button_accept.Clicked += new EventHandler(on_button_encoder_exercise_edit_accepted);
		genericWin.ShowNow();
	}

	void on_button_encoder_exercise_add_clicked (object o, EventArgs args) 
	{
		ArrayList bigArray = new ArrayList();

		ArrayList a1 = new ArrayList();
		ArrayList a2 = new ArrayList();
		ArrayList a3 = new ArrayList();
		ArrayList a4 = new ArrayList();
		ArrayList a5 = new ArrayList();

		//0 is the widgget to show; 1 is the editable; 2 id default value
		a1.Add(Constants.GenericWindowShow.ENTRY); a1.Add(true); a1.Add("");
		bigArray.Add(a1);

		a2.Add(Constants.GenericWindowShow.SPININT); a2.Add(true); a2.Add("");
		bigArray.Add(a2);
		
		a3.Add(Constants.GenericWindowShow.ENTRY2); a3.Add(true); a3.Add("");
		bigArray.Add(a3);
		
		a4.Add(Constants.GenericWindowShow.ENTRY3); a4.Add(true); a4.Add("");
		bigArray.Add(a4);
		
		a5.Add(Constants.GenericWindowShow.HBOXSPINDOUBLE2); a5.Add(true); a5.Add("");
		bigArray.Add(a5);
		
		
		genericWin = GenericWindow.Show(false,	//don't show now
				Catalog.GetString("Write the name of the encoder exercise:"), bigArray);
		genericWin.LabelSpinInt = Catalog.GetString("Displaced body weight") + " (%)";
		genericWin.SetSpinRange(0, 100);
		genericWin.LabelEntry2 = Catalog.GetString("Resistance");
		genericWin.LabelEntry3 = Catalog.GetString("Description");
		genericWin.LabelSpinDouble2 = Catalog.GetString("Speed at 1RM");
		genericWin.SetSpinDouble2Increments(0.001,0.1);
		/*
		 * Now this is in encoder configuration
		genericWin.LabelSpinInt2 = Catalog.GetString("Body angle") + " (º)";
		genericWin.SetSpin2Range(0,90);
		genericWin.SetSpin2Value(90);
		genericWin.LabelSpinInt3 = Catalog.GetString("Weight angle") + " (º)";
		genericWin.SetSpin3Range(0,90);
		genericWin.SetSpin3Value(90);
		*/
		genericWin.SetButtonAcceptLabel(Catalog.GetString("Add"));
		
		genericWin.HideOnAccept = false;

		genericWin.Button_accept.Clicked += new EventHandler(on_button_encoder_exercise_add_accepted);
		genericWin.ShowNow();
	}
	
	void on_button_encoder_exercise_edit_accepted (object o, EventArgs args) {
		encoder_exercise_edit(false);
		genericWin.Button_accept.Clicked -= new EventHandler(on_button_encoder_exercise_edit_accepted);
	}
	void on_button_encoder_exercise_add_accepted (object o, EventArgs args) {
		encoder_exercise_edit(true);
		genericWin.Button_accept.Clicked -= new EventHandler(on_button_encoder_exercise_add_accepted);
	}
			
	void encoder_exercise_edit (bool adding) 
	{
		string name = Util.RemoveTildeAndColonAndDot(genericWin.EntrySelected);
		name = Util.RemoveChar(name, '"');

		if(adding)
			LogB.Information("Trying to insert: " + name);
		else
			LogB.Information("Trying to edit: " + name);

		if(name == "")
			genericWin.SetLabelError(Catalog.GetString("Error: Missing name of exercise."));
		else if (adding && Sqlite.Exists(false, Constants.EncoderExerciseTable, name))
			genericWin.SetLabelError(string.Format(Catalog.GetString(
							"Error: An exercise named '{0}' already exists."), name));
		else {
			if(adding)
				SqliteEncoder.InsertExercise(false, name, genericWin.SpinIntSelected, 
						genericWin.Entry2Selected, genericWin.Entry3Selected,
						Util.ConvertToPoint(genericWin.SpinDouble2Selected)
						);
			else
				SqliteEncoder.UpdateExercise(false, genericWin.nameUntranslated, name, genericWin.SpinIntSelected, 
						genericWin.Entry2Selected, genericWin.Entry3Selected,
						Util.ConvertToPoint(genericWin.SpinDouble2Selected)
						);

			ArrayList encoderExercises = SqliteEncoder.SelectEncoderExercises(false,-1, false);
			encoderExercisesTranslationAndBodyPWeight = new String [encoderExercises.Count];
			string [] exerciseNamesToCombo = new String [encoderExercises.Count];
			int i =0;
			foreach(EncoderExercise ex in encoderExercises) {
				string nameTranslated = ex.name;
				//do not translated user created exercises
				//this names are in SqliteEncoder.initializeTableEncoderExercise()
				if(ex.name == "Bench press" || ex.name == "Squat" || ex.name == "Jump")
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

			genericWin.HideAndNull();
			LogB.Information("done");
		}
	}
	
	void on_button_encoder_exercise_delete (object o, EventArgs args) 
	{
		EncoderExercise ex = (EncoderExercise) SqliteEncoder.SelectEncoderExercises(
				false, genericWin.uniqueID, false)[0];
		if(ex.IsPredefined()) {
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Sorry, predefined exercises cannot be deleted."));
			return;
		}

		ArrayList array = SqliteEncoder.SelectEncoderRowsOfAnExercise(false, genericWin.uniqueID); //dbconOpened, exerciseID

		if(array.Count > 0) {
			//there are some records of this exercise on encoder table, do not delete
			genericWin.SetTextview(
					Catalog.GetString("Sorry, this exercise cannot be deleted.") + "\n" +
					Catalog.GetString("Please delete first the following repetitions:"));

			ArrayList nonSensitiveRows = new ArrayList();
			for(int i=0; i < array.Count; i ++)
				nonSensitiveRows.Add(i);

			genericWin.SetTreeview(
					new string [] {
					"count",	//not shown, unused
					Catalog.GetString("Repetitions"), Catalog.GetString("Person"),
					Catalog.GetString("Session"), Catalog.GetString("Date") }, 
					false, array, nonSensitiveRows, Constants.ContextMenu.NONE, false);

			genericWin.ShowTextview();
			genericWin.ShowTreeview();
			genericWin.ShowButtonDelete(false);
			genericWin.DeletingExerciseHideSomeWidgets();
		
			genericWin.Button_accept.Clicked -= new EventHandler(on_button_encoder_exercise_edit_accepted);
			genericWin.Button_accept.Clicked += new EventHandler(on_button_encoder_exercise_do_not_delete);
		} else {
			//encoder table has not records of this exercise
			//delete exercise
			Sqlite.Delete(false, Constants.EncoderExerciseTable, genericWin.uniqueID);
			//delete 1RM records of this exercise
			Sqlite.DeleteFromAnInt(false, Constants.Encoder1RMTable, "exerciseID", genericWin.uniqueID);

			genericWin.HideAndNull();

			createEncoderComboExerciseAndAnalyze();
			combo_encoder_exercise_capture.Active = 0;
			combo_encoder_exercise_analyze.Active = 0;

			new DialogMessage(Constants.MessageTypes.INFO, Catalog.GetString("Exercise deleted."));
		}
	}

	//accept does not save changes, just closes window
	void on_button_encoder_exercise_do_not_delete (object o, EventArgs args) {
		genericWin.Button_accept.Clicked -= new EventHandler(on_button_encoder_exercise_do_not_delete);
		genericWin.HideAndNull();
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
	}
	
	/* called on:
	 * encoderPersonChanged()
	 * select_menuitem_mode_toggled(Constants.Menuitem_modes m)
	 */
	private void blankEncoderInterface()
	{
		if(radio_encoder_analyze_individual_current_set.Active)
			updateComboEncoderAnalyzeCurveNumFromCurrentSet ();
		else
			prepareAnalyzeRepetitions();
	
		//blank the encoderCaptureListStore
		encoderCaptureListStore = new Gtk.ListStore (typeof (EncoderCurve));
		button_encoder_analyze_sensitiveness();
		
		treeviewEncoderCaptureRemoveColumns();

		if(encoder_capture_curves_bars_pixmap != null)
			UtilGtk.ErasePaint(encoder_capture_curves_bars_drawingarea, encoder_capture_curves_bars_pixmap);
		if(encoder_capture_signal_pixmap != null)
			UtilGtk.ErasePaint(encoder_capture_signal_drawingarea, encoder_capture_signal_pixmap);

		image_encoder_capture.Sensitive = false;
		image_encoder_analyze.Sensitive = false;
		treeview_encoder_analyze_curves.Sensitive = false;
		button_encoder_analyze_image_save.Sensitive = false;
		button_encoder_analyze_AB_save.Sensitive = false;
		button_encoder_analyze_table_save.Sensitive = false;
		button_encoder_analyze_1RM_save.Visible = false;
	}

	private void encoderButtonsSensitive(encoderSensEnum option) 
	{
		LogB.Debug(option.ToString());

		//columns
		//c0 button_encoder_capture, hbox_encoder_sup_capture_analyze_two_buttons,
		//	hbox_encoder_configuration, frame_encoder_capture_options
		//c1 button_encoder_recalculate
		//c2 button_encoder_load_signal
		//c3 hbox_encoder_capture_curves_save_all_none, menuitem_export_encoder_signal 
		//	button_encoder_delete_signal, vbox_encoder_signal_comment,
		//	and images: image_encoder_capture , image_encoder_analyze.Sensitive. Update: both NOT managed here
		//UNUSED c4 button_encoder_save_curve, entry_encoder_curve_comment
		//c5 button_encoder_analyze
		//c6 hbox_encoder_user_curves
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
		int [] doneNoSignal = 		{1, 1, 1, 0, 0, 1, 1, 0, 0};
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
		hbox_encoder_sup_capture_analyze_two_buttons.Sensitive = Util.IntToBool(table[0]);
		hbox_encoder_configuration.Sensitive = Util.IntToBool(table[0]);
		frame_encoder_capture_options.Sensitive = Util.IntToBool(table[0]);

		button_encoder_recalculate.Sensitive = Util.IntToBool(table[1]);
		
		button_encoder_load_signal.Sensitive = Util.IntToBool(table[2]);
		button_encoder_load_signal_on_analyze.Sensitive = Util.IntToBool(table[2]);
		
		hbox_encoder_capture_curves_save_all_none.Sensitive = Util.IntToBool(table[3]);
		menuitem_export_encoder_signal.Sensitive = Util.IntToBool(table[3]);
		button_encoder_delete_signal.Sensitive = Util.IntToBool(table[3]);
		vbox_encoder_signal_comment.Sensitive = Util.IntToBool(table[3]);
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
		if(analyze_sensitive && radiobutton_encoder_analyze_side.Active) {
			analyze_sensitive = curvesNumOkToSideCompare();
			label_encoder_analyze_side_max.Visible = ! analyze_sensitive;
		} else
			label_encoder_analyze_side_max.Visible = false;

		button_encoder_analyze.Sensitive = analyze_sensitive;

		hbox_encoder_user_curves.Visible = 
			(Util.IntToBool(table[6]) && ! radio_encoder_analyze_individual_current_set.Active);
		
		button_encoder_capture_cancel.Sensitive = Util.IntToBool(table[7]);
		button_encoder_analyze_cancel.Sensitive = Util.IntToBool(table[7]);
		
		button_encoder_capture_finish.Sensitive = Util.IntToBool(table[8]);
		button_encoder_capture_finish_cont.Sensitive = Util.IntToBool(table[8]);
	}
	
	private void button_encoder_analyze_sensitiveness() {
		bool analyze_sensitive = false;
		if(radio_encoder_analyze_individual_current_set.Active) {
			int rows = UtilGtk.CountRows(encoderCaptureListStore);
			
			//button_encoder_analyze.Sensitive = encoderTimeStamp != null;
			
			analyze_sensitive = (rows > 0);
			if(analyze_sensitive && radiobutton_encoder_analyze_side.Active) {
				analyze_sensitive = curvesNumOkToSideCompare();
				label_encoder_analyze_side_max.Visible = ! analyze_sensitive;
			}
		} else {
			analyze_sensitive = (currentPerson != null && encSelReps.RepsActive > 0);
			if(analyze_sensitive && radiobutton_encoder_analyze_side.Active) {
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
	}


	/* end of sensitivity stuff */	
	
	/*
	 * update encoder capture graph stuff
	 */

	enum UpdateEncoderPaintModes { GRAVITATORY, INERTIAL, CALCULE_IM }
	private void updateEncoderCaptureGraphPaint(UpdateEncoderPaintModes mode)
	{
		if(eCapture.EncoderCapturePoints == null)
			return;

		//this happens when EncoderCaptureShowOnlyBars=TRUE
		if(encoder_capture_signal_drawingarea == null || encoder_capture_signal_pixmap == null)
			return;

		bool refreshAreaOnly = false;
		
		//mark meaning screen should be erased
		if(eCapture.EncoderCapturePointsPainted == -1) {
			UtilGtk.ErasePaint(encoder_capture_signal_drawingarea, encoder_capture_signal_pixmap);
			eCapture.EncoderCapturePointsPainted = 0;
		}

		//also can be optimized to do not erase window every time and only add points since last time
		int last = eCapture.EncoderCapturePointsCaptured;
		int toDraw = eCapture.EncoderCapturePointsCaptured - eCapture.EncoderCapturePointsPainted;

		//LogB.Information("last - toDraw:" + last + " - " + toDraw);	

		//fixes crash at the end
		if(toDraw == 0)
			return;

		int maxY=-1;
		int minY=10000;

		Gdk.Point [] paintPoints = new Gdk.Point[toDraw];
		Gdk.Point [] paintPointsInertial = new Gdk.Point[toDraw];

		for(int j=0, i = eCapture.EncoderCapturePointsPainted +1 ; i <= last ; i ++, j++)
		{
			paintPoints[j] = eCapture.EncoderCapturePoints[i];

			if(refreshAreaOnly) {
				if(eCapture.EncoderCapturePoints[i].Y > maxY)
					maxY = eCapture.EncoderCapturePoints[i].Y;
				if(eCapture.EncoderCapturePoints[i].Y < minY)
					minY = eCapture.EncoderCapturePoints[i].Y;
			}

		}

		if(mode == UpdateEncoderPaintModes.INERTIAL) {
			for(int j=0, i = eCapture.EncoderCapturePointsPainted +1 ; i <= last ; i ++, j ++)
			{
				//only assign the points if they are different than paintPoints
				if(eCapture.EncoderCapturePointsInertialDisc[i] != eCapture.EncoderCapturePoints[i] &&
						(i % 800) <= 520 //dashed accepting 520 points and discarding 280
				  ) {
					paintPointsInertial[j] = eCapture.EncoderCapturePointsInertialDisc[i];

					if(refreshAreaOnly) {
						if(eCapture.EncoderCapturePointsInertialDisc[i].Y > maxY)
							maxY = eCapture.EncoderCapturePointsInertialDisc[i].Y;
						if(eCapture.EncoderCapturePointsInertialDisc[i].Y < minY)
							minY = eCapture.EncoderCapturePointsInertialDisc[i].Y;
					}
				}
			}
			encoder_capture_signal_pixmap.DrawPoints(pen_gray, paintPointsInertial);
		}
		//paint this after the inertial because this should mask the other
		encoder_capture_signal_pixmap.DrawPoints(pen_black_encoder_capture, paintPoints);
		

		//write title
		string title = "";
		if(mode == UpdateEncoderPaintModes.CALCULE_IM)
			title = Catalog.GetString("Inertia M.");
		else {
			title = currentPerson.Name + " (";
			if(encoderConfigurationCurrent.has_inertia)
				title += encoderConfigurationCurrent.inertiaTotal.ToString() + " " + Catalog.GetString("Inertia M.") + ")";
			else	
				title += findMass(Constants.MassType.EXTRA).ToString() + "Kg)";
		}
		layout_encoder_capture_signal.SetMarkup(title);
		

		encoder_capture_signal_pixmap.DrawLayout(pen_blue_encoder_capture, 5, 5, layout_encoder_capture_signal);

		if(refreshAreaOnly) {
			/*			
						LogB.Information("pp X-TD-W: " + 
						paintPoints[0].X.ToString() + " - " + 
						paintPoints[toDraw-1].X.ToString() + " - " + 
						(paintPoints[toDraw-1].X-paintPoints[0].X).ToString());
						*/

			int startX = paintPoints[0].X;
			/*
			 * this helps to ensure that no white points are drawed
			 * caused by this int when eCapture.EncoderCapturePoints are assigned:
			 * Convert.ToInt32(width*i/recordingTime)
			 */
			int exposeMargin = 4;
			if(startX -exposeMargin > 0)
				startX -= exposeMargin;	


			encoder_capture_signal_drawingarea.QueueDrawArea( 			// -- refresh
					startX,
					minY,
					(paintPoints[toDraw-1].X-paintPoints[0].X ) + exposeMargin,
					maxY-minY
					);
			//if refreshAreaOnly is true, then repeat above instruction for paintPointsInertial
			
			LogB.Information("minY - maxY " + minY + " - " + maxY);
		} else
			encoder_capture_signal_drawingarea.QueueDraw(); 			// -- refresh

		eCapture.EncoderCapturePointsPainted = eCapture.EncoderCapturePointsCaptured;
	}

	static List<string> encoderCaptureStringR;
	static ArrayList captureCurvesBarsData;
	
	//if we are capturing, play sounds
	void plotCurvesGraphDoPlot(string mainVariable, double mainVariableHigher, double mainVariableLower, 
			ArrayList data6Variables, bool discardFirstThree, bool capturing) 
	{
		UtilGtk.ErasePaint(encoder_capture_curves_bars_drawingarea, encoder_capture_curves_bars_pixmap);

		int graphWidth=encoder_capture_curves_bars_drawingarea.Allocation.Width;
		int graphHeight=encoder_capture_curves_bars_drawingarea.Allocation.Height;
	
		ArrayList data = new ArrayList (data6Variables.Count);
		foreach(EncoderBarsData ebd in data6Variables)
			data.Add(ebd.GetValue(mainVariable));

		//Get max min avg values of this set
		double maxThisSet = -100000;
		double minThisSet = 100000;
		double sum = 0;

		foreach(double d in data) {
			if(d > maxThisSet)
				maxThisSet = d;
			if(d < minThisSet)
				minThisSet = d;
			sum += d;
		}
		if(maxThisSet <= 0)
			return;	

		//get maxIntersession and update it if is lower than maxThisSet
		double maxIntersession = -100000;
		if(mainVariable == Constants.MeanPower)
			maxIntersession = maxPowerIntersessionOnCapture;

		if(maxThisSet > maxIntersession)
			maxIntersession = maxThisSet;

		double maxAbsolute = maxThisSet;
		if(maxIntersession > maxAbsolute)
			maxAbsolute = maxIntersession;

		repetitiveConditionsWin.ResetBestSetValue();
		repetitiveConditionsWin.UpdateBestSetValue(maxAbsolute);

		int textWidth = 1;
		int textHeight = 1;

		int left_margin = 10;
		int right_margin = 0;
		int top_margin = 20 + 3 * preferences.encoderCaptureBarplotFontSize;
		int bottom_margin = 8 + preferences.encoderCaptureBarplotFontSize;
		//bars will be plotted here
		int graphHeightSafe = graphHeight - (top_margin + bottom_margin);
	
			

		//plot bars
		int sep = 20;	//between reps

		if (data.Count >= 10 && data.Count < 20) {
			sep = 10;
			layout_encoder_capture_curves_bars.FontDescription =
				Pango.FontDescription.FromString ("Courier " + (preferences.encoderCaptureBarplotFontSize -2).ToString());
		} else if (data.Count >= 20) {
			sep = 2;
			layout_encoder_capture_curves_bars.FontDescription =
				Pango.FontDescription.FromString ("Courier " + (preferences.encoderCaptureBarplotFontSize -4).ToString());
			left_margin = 2;
		}

		layout_encoder_capture_curves_bars_text.FontDescription =
			Pango.FontDescription.FromString ("Courier " + preferences.encoderCaptureBarplotFontSize.ToString());
		layout_encoder_capture_curves_bars_text.FontDescription.Weight = Pango.Weight.Bold;
		
		string eccon = findEccon(true);

		Rectangle rect;

		Gdk.GC my_pen_ecc_con_e; //ecc-con eccentric
		Gdk.GC my_pen_ecc_con_c; //ecc-con concentric
		Gdk.GC my_pen_con;	//concentric
		Gdk.GC my_pen;		//pen we are going to use

		int dLeft = 0;
		int count = 0;
	
		//to show saved curves on DoPlot	
		TreeIter iter;
		
		//sum saved curves to do avg
		double sumSaved = 0; 
		double countSaved = 0;
		
		//draw line for person max intersession
		if(mainVariable == Constants.MeanPower && maxIntersession > 0)
		{
			//layout_encoder_capture_curves_bars_text.SetMarkup("Person's best: (" + Util.TrimDecimals(maxIntersession, 1) + "W)");
			layout_encoder_capture_curves_bars_text.SetMarkup("Person's best:");
			layout_encoder_capture_curves_bars_text.GetPixelSize(out textWidth, out textHeight);
			encoder_capture_curves_bars_pixmap.DrawLayout (pen_yellow_encoder_capture,
						left_margin, top_margin - textHeight,
						layout_encoder_capture_curves_bars_text);

			encoder_capture_curves_bars_pixmap.DrawLine(pen_yellow_encoder_capture,
					left_margin, top_margin,
					graphWidth - right_margin, top_margin);

			layout_encoder_capture_curves_bars_text.SetMarkup(Util.TrimDecimals(maxIntersession, 1) + "W");
			layout_encoder_capture_curves_bars_text.GetPixelSize(out textWidth, out textHeight);
			encoder_capture_curves_bars_pixmap.DrawLayout (pen_yellow_encoder_capture,
						graphWidth - (right_margin + textWidth),
						top_margin - textHeight,
						layout_encoder_capture_curves_bars_text);
		}
		
		bool iterOk = encoderCaptureListStore.GetIterFirst(out iter);
		foreach(double dFor in data) {
			int dWidth = 0;
			int dHeight = 0;

			//if values are negative, invert it
			//this happens specially in the speeds in eccentric
			//we use dFor because we cannot change the iteration variable
			double d = dFor;
			if(d < 0)
				d *= -1;

			dHeight = Convert.ToInt32(graphHeightSafe * d / maxAbsolute * 1.0);
			int dBottom = graphHeight - bottom_margin;
			int dTop = dBottom - dHeight;


			if (data.Count == 1)	//do not fill all the screen with only one bar
				dWidth = Convert.ToInt32((graphWidth - left_margin - right_margin) / 2.0); 
			else
				dWidth = Convert.ToInt32((graphWidth - left_margin - right_margin) / data.Count * 1.0);

			dLeft = left_margin + dWidth * count;
		
			//dWidth = dWidth - sep to have separation between bars
			//but if eccon != "c" then have like this: ec ec ec
			if (eccon == "c") {
				dWidth = dWidth - sep;
			} else {
				double sep_ec_mult = 1.2;
				dWidth = Convert.ToInt32(dWidth - sep * sep_ec_mult);

				if(Util.IsEven(count +1)) //par
					dLeft = Convert.ToInt32(dLeft - sep * sep_ec_mult);
			}
			//just in case there are too much bars
			if(dWidth < 1)
				dWidth = 1;

			//select pen color for bars and sounds
			string myColor = repetitiveConditionsWin.AssignColorAutomatic(d);

			bool discarded = false;
			if(encoderConfigurationCurrent.has_inertia) {
				if(eccon == "c" && discardFirstThree && count < 3)
					discarded = true;
				else if(eccon != "c" && discardFirstThree && count < 6)
					discarded = true;
				else if ((eccon == "ec" || eccon == "ecS") && count == 0)
					discarded = true;	//on inertial devices "ec" or "ecS", the first ecc cannot have feedback
			}

			if( ! discarded && ( myColor == UtilGtk.ColorGood || (mainVariableHigher != -1 && d >= mainVariableHigher) ) )
			{
				my_pen_ecc_con_e = pen_green_dark_encoder_capture;
				my_pen_ecc_con_c = pen_green_light_encoder_capture;
				my_pen_con = pen_green_encoder_capture;
				//play sound if value is high, volumeOn == true, is last value, capturing
				if(preferences.volumeOn && count == data.Count -1 && capturing)
					Util.PlaySound(Constants.SoundTypes.GOOD, preferences.volumeOn);
			}
			else if( ! discarded && ( myColor == UtilGtk.ColorBad || (mainVariableLower != -1 && d <= mainVariableLower) ) )
			{
				my_pen_ecc_con_e = pen_red_dark_encoder_capture;
				my_pen_ecc_con_c = pen_red_light_encoder_capture;
				my_pen_con = pen_red_encoder_capture;
				//play sound if value is low, volumeOn == true, is last value, capturing
				if(preferences.volumeOn && count == data.Count -1 && capturing)
					Util.PlaySound(Constants.SoundTypes.BAD, preferences.volumeOn);
			}
			else {
				my_pen_ecc_con_e = pen_blue_dark_encoder_capture;
				my_pen_ecc_con_c = pen_blue_light_encoder_capture;
				my_pen_con = pen_blue_encoder_capture;
			}
			
			//know if ecc or con to paint with dark or light pen
			if (eccon == "ec" || eccon == "ecS") {
				bool isEven = Util.IsEven(count +1);
				
				//on inertial devices "ec" or "ecS", the first ecc has to be gray
				if(encoderConfigurationCurrent.has_inertia && count == 0)
					my_pen = pen_gray;
				else {
					if(isEven) //par, concentric
						my_pen = my_pen_ecc_con_c;
					else
						my_pen = my_pen_ecc_con_e;
				}
			} else {
				my_pen = my_pen_con;
			}

			//paint bar:	
			rect = new Rectangle(dLeft, dTop, dWidth, dHeight);
			encoder_capture_curves_bars_pixmap.DrawRectangle(my_pen, true, rect);
			encoder_capture_curves_bars_pixmap.DrawRectangle(pen_black_encoder_capture, false, rect);


			//write the result	
			if(mainVariable == Constants.MeanSpeed || mainVariable == Constants.MaxSpeed)
				layout_encoder_capture_curves_bars.SetMarkup(Util.TrimDecimals(d,2));
			else //force and powers
				layout_encoder_capture_curves_bars.SetMarkup(Util.TrimDecimals(d,0));
			
			textWidth = 1;
			textHeight = 1;
			layout_encoder_capture_curves_bars.GetPixelSize(out textWidth, out textHeight); 
			encoder_capture_curves_bars_pixmap.DrawLayout (pen_black_encoder_capture, 
					Convert.ToInt32( (dLeft + dWidth/2) - textWidth/2), 	//x
					dTop - (5 + preferences.encoderCaptureBarplotFontSize), //y
					layout_encoder_capture_curves_bars);
			//end of: write the result

			//paint diagonal line to distinguish eccentric-concentric	
			if (eccon == "ec" || eccon == "ecS") {
				bool isEven = Util.IsEven(count +1);
				
				if(isEven)
					encoder_capture_curves_bars_pixmap.DrawLine(pen_white_encoder_capture, 
							dLeft, dBottom, dLeft + dWidth, dTop);
				else
					encoder_capture_curves_bars_pixmap.DrawLine(pen_white_encoder_capture, 
							dLeft, dTop, dLeft + dWidth, dBottom);
			}
			
			bool curveSaved = false;	
			if( iterOk && ((EncoderCurve) encoderCaptureListStore.GetValue (iter, 0)).Record ) {
				curveSaved = true;
				sumSaved += dFor;
				countSaved ++;
			}
			
			//add text on the bottom
			if (eccon == "c" || Util.IsEven(count +1)) //par
			{
				int startX = Convert.ToInt32(dLeft + dWidth/2);
				string bottomText = (count +1).ToString();
				if (eccon != "c") {
					startX = dLeft;
					bottomText = ((count +1) / 2).ToString();
				}

				layout_encoder_capture_curves_bars.SetMarkup(bottomText);
				textWidth = 1;
				textHeight = 1;
				layout_encoder_capture_curves_bars.GetPixelSize(out textWidth, out textHeight); 
				int myX = Convert.ToInt32( startX - textWidth/2);
				int myY = Convert.ToInt32(dTop + dHeight + (bottom_margin /2) - textHeight/2);
			
				if(curveSaved) {
					rect = new Rectangle(myX -2, myY -1, textWidth +4, graphHeight - (myY -1) -1);
					encoder_capture_curves_bars_pixmap.DrawRectangle(pen_selected_encoder_capture, false, rect);
				}
				
				//write the text
				encoder_capture_curves_bars_pixmap.DrawLayout (pen_black_encoder_capture, 
						myX, myY,
						layout_encoder_capture_curves_bars);
			}

			count ++;
			iterOk = encoderCaptureListStore.IterNext (ref iter);
		}
		//end plot bars
	

		//plot title
		string units = "";
		int decimals;
		
		if(mainVariable == Constants.MeanSpeed || mainVariable == Constants.MaxSpeed) {
			units = "m/s";
			decimals = 2;
		} else if(mainVariable == Constants.MeanForce || mainVariable == Constants.MaxForce) {
			units = "N";
			decimals = 1;
		}
		else { //powers
			units =  "W";
			decimals = 1;
		}
		
		//add avg and avg of saved values
		string title = mainVariable + " [X = " + 
			Util.TrimDecimals( (sum / data.Count), decimals) + 
			" " + units;

		if(countSaved > 0)
			title += "; X" + Catalog.GetString("saved") + " = " + 
				Util.TrimDecimals( (sumSaved / countSaved), decimals) + 
				" " + units;

		title += "; Loss: " + Util.TrimDecimals(100.0 * (maxThisSet - minThisSet) / maxThisSet, decimals) + "%]";

		layout_encoder_capture_curves_bars_text.SetMarkup(title);
		textWidth = 1;
		textHeight = 1;
		layout_encoder_capture_curves_bars_text.GetPixelSize(out textWidth, out textHeight);
		encoder_capture_curves_bars_pixmap.DrawLayout (pen_black_encoder_capture, 
				Convert.ToInt32( (graphWidth/2) - textWidth/2), 0, //x, y 
				layout_encoder_capture_curves_bars_text);

		//end plot title	
		

		encoder_capture_curves_bars_drawingarea.QueueDraw(); 			// -- refresh
		encoder_capture_curves_bars_drawingarea.Visible = true;
	}

	private void callPlotCurvesGraphDoPlot() {
		if(captureCurvesBarsData.Count > 0) {
			string mainVariable = Constants.GetEncoderVariablesCapture(preferences.encoderCaptureMainVariable);
			double mainVariableHigher = repetitiveConditionsWin.GetMainVariableHigher(mainVariable);
			double mainVariableLower = repetitiveConditionsWin.GetMainVariableLower(mainVariable);
			plotCurvesGraphDoPlot(mainVariable, mainVariableHigher, mainVariableLower, captureCurvesBarsData,
					repetitiveConditionsWin.EncoderInertialDiscardFirstThree,
					false);	//not capturing
		} else if( ! ( radio_encoder_capture_cont.Active && ! firstSetOfCont) )
			UtilGtk.ErasePaint(encoder_capture_curves_bars_drawingarea, encoder_capture_curves_bars_pixmap);
	}

	private void plotCurvesGraphDoPlotMessage(string message) 
	{
		Pango.Layout layout_message = new Pango.Layout (encoder_capture_curves_bars_drawingarea.PangoContext);
		layout_message.FontDescription = Pango.FontDescription.FromString ("Courier 10");
		
		int graphWidth=encoder_capture_curves_bars_drawingarea.Allocation.Width;
		int graphHeight=encoder_capture_curves_bars_drawingarea.Allocation.Height;

		layout_message.SetMarkup(message);
		int textWidth = 1;
		int textHeight = 1;
		layout_message.GetPixelSize(out textWidth, out textHeight); 
		
		int xStart = Convert.ToInt32(graphWidth/2 - textWidth/2);
		int yStart = Convert.ToInt32(graphHeight/2 - textHeight/2);

		//draw horizontal line behind (across all graph)
		Rectangle rect = new Rectangle(0, yStart + textHeight -1, graphWidth, 1);
		encoder_capture_curves_bars_pixmap.DrawRectangle(pen_yellow_encoder_capture, true, rect);

		//draw rectangle behind text
		rect = new Rectangle(xStart -2, yStart -2, textWidth +2, textHeight +2);
		encoder_capture_curves_bars_pixmap.DrawRectangle(pen_yellow_encoder_capture, true, rect);
		
		//write text inside
		encoder_capture_curves_bars_pixmap.DrawLayout (pen_black_encoder_capture, xStart, yStart, layout_message);
	}

	int encoder_capture_curves_allocationXOld;
	int encoder_capture_curves_allocationYOld;
	bool encoder_capture_curves_sizeChanged;
	public void on_encoder_capture_curves_bars_drawingarea_configure_event(object o, ConfigureEventArgs args)
	{
		Gdk.EventConfigure ev = args.Event;
		Gdk.Window window = ev.Window;
		Gdk.Rectangle allocation = encoder_capture_curves_bars_drawingarea.Allocation;

		LogB.Debug("CONFIGURE");

		if(encoder_capture_curves_bars_pixmap == null || encoder_capture_curves_sizeChanged || 
				allocation.Width != encoder_capture_curves_allocationXOld ||
				allocation.Height != encoder_capture_curves_allocationYOld)
		{
			if(encoder_capture_curves_bars_pixmap == null || ! radio_encoder_capture_cont.Active || firstSetOfCont)
				encoder_capture_curves_bars_pixmap = new Gdk.Pixmap (window, allocation.Width, allocation.Height, -1);

			callPlotCurvesGraphDoPlot();
		
			encoder_capture_curves_sizeChanged = false;
		}

		encoder_capture_curves_allocationXOld = allocation.Width;
		encoder_capture_curves_allocationYOld = allocation.Height;
	}

	public void on_encoder_capture_curves_bars_drawingarea_expose_event(object o, ExposeEventArgs args)
	{
		/* in some mono installations, configure_event is not called, but expose_event yes. 
		 * Do here the initialization
		 */
		LogB.Debug("EXPOSE");

		Gdk.Rectangle allocation = encoder_capture_curves_bars_drawingarea.Allocation;
		if(encoder_capture_curves_bars_pixmap == null || encoder_capture_curves_sizeChanged || 
				allocation.Width != encoder_capture_curves_allocationXOld ||
				allocation.Height != encoder_capture_curves_allocationYOld) 
		{

			encoder_capture_curves_bars_pixmap = new Gdk.Pixmap (
					encoder_capture_curves_bars_drawingarea.GdkWindow, allocation.Width, allocation.Height, -1);
			
			callPlotCurvesGraphDoPlot();
			
			encoder_capture_curves_sizeChanged = false;
		}

		Gdk.Rectangle area = args.Event.Area;

		//sometimes this is called when paint is finished
		//don't let this erase win
		if(encoder_capture_curves_bars_pixmap != null) {
			args.Event.Window.DrawDrawable(encoder_capture_curves_bars_drawingarea.Style.WhiteGC, 
					encoder_capture_curves_bars_pixmap,
				area.X, area.Y,
				area.X, area.Y,
				area.Width, area.Height);
		}
		
		encoder_capture_curves_allocationXOld = allocation.Width;
		encoder_capture_curves_allocationYOld = allocation.Height;
	}

	
	int encoder_capture_signal_allocationXOld;
	bool encoder_capture_signal_sizeChanged;
	public void on_encoder_capture_signal_drawingarea_configure_event(object o, ConfigureEventArgs args)
	{
		Gdk.EventConfigure ev = args.Event;
		Gdk.Window window = ev.Window;

		Gdk.Rectangle allocation = encoder_capture_signal_drawingarea.Allocation;
		
		if(encoder_capture_signal_pixmap == null || encoder_capture_signal_sizeChanged || 
				allocation.Width != encoder_capture_signal_allocationXOld) 
		{
			encoder_capture_signal_pixmap = new Gdk.Pixmap (window, allocation.Width, allocation.Height, -1);
		
			if(eCapture != null && capturingCsharp == encoderCaptureProcess.CAPTURING)
				eCapture.EncoderCapturePointsPainted = -1; //mark meaning screen should be erased and start painting from the beginning
			else
				UtilGtk.ErasePaint(encoder_capture_signal_drawingarea, encoder_capture_signal_pixmap);
			
			encoder_capture_signal_sizeChanged = false;
		}

		encoder_capture_signal_allocationXOld = allocation.Width;
	}
	
	public void on_encoder_capture_signal_drawingarea_expose_event(object o, ExposeEventArgs args)
	{
		/* in some mono installations, configure_event is not called, but expose_event yes. 
		 * Do here the initialization
		 */
		//LogB.Debug("EXPOSE");
		
		Gdk.Rectangle allocation = encoder_capture_signal_drawingarea.Allocation;
		if(encoder_capture_signal_pixmap == null || encoder_capture_signal_sizeChanged || 
				allocation.Width != encoder_capture_signal_allocationXOld) {
			encoder_capture_signal_pixmap = new Gdk.Pixmap (encoder_capture_signal_drawingarea.GdkWindow, 
					allocation.Width, allocation.Height, -1);
			
			if(eCapture != null && capturingCsharp == encoderCaptureProcess.CAPTURING)
				eCapture.EncoderCapturePointsPainted = -1; //mark meaning screen should be erased and start painting from the beginning
			else
				UtilGtk.ErasePaint(encoder_capture_signal_drawingarea, encoder_capture_signal_pixmap);

			encoder_capture_signal_sizeChanged = false;
		}

		Gdk.Rectangle area = args.Event.Area;

		//sometimes this is called when pait is finished
		//don't let this erase win
		if(encoder_capture_signal_pixmap != null) {
			args.Event.Window.DrawDrawable(encoder_capture_signal_drawingarea.Style.WhiteGC, encoder_capture_signal_pixmap,
				area.X, area.Y,
				area.X, area.Y,
				area.Width, area.Height);
		}
		
		encoder_capture_signal_allocationXOld = allocation.Width;
	}



	/*
	 * end of update encoder capture graph stuff
	 */
	
	
	/* thread stuff */

	private void encoderThreadStart(encoderActions action)
	{
		encoderProcessCancel = false;
					
		if(action == encoderActions.CAPTURE_BG)
		{
			shownWaitAtInertialCapture = false;
			calledCaptureInertial = false;
			timeCalibrated = DateTime.Now;

			if(currentSession.Name == Constants.SessionSimulatedName && testsActive)
				eCaptureInertialBG = new EncoderCaptureInertialBackground("");
			else
				eCaptureInertialBG = new EncoderCaptureInertialBackground(
						chronopicRegister.ConnectedOfType(ChronopicRegisterPort.Types.ENCODER).Port);

			encoderThreadBG = new Thread(new ThreadStart(encoderDoCaptureBG));
			GLib.Idle.Add (new GLib.IdleHandler (pulseGTKEncoderCaptureBG));

			LogB.ThreadStart();
			LogB.Mute = true; //mute logs to improve stability (encoder inertial test only works with muted log
			encoderThreadBG.Start();
		}

		else if(action == encoderActions.CAPTURE || action == encoderActions.CAPTURE_IM)
		{
			//encoder_pulsebar_capture.Text = Catalog.GetString("Please, wait.");
			LogB.Information("encoderThreadStart begins");
				
			if(action == encoderActions.CAPTURE) {
				runEncoderCaptureNoRDotNetInitialize();
			}

			image_encoder_width = UtilGtk.WidgetWidth(viewport_image_encoder_capture)-5; 
			image_encoder_height = UtilGtk.WidgetHeight(viewport_image_encoder_capture)-5;

			//don't need to be false because ItemToggled is deactivated during capture
			treeview_encoder_capture_curves.Sensitive = true;

			//on continuous mode do not erase bars at beginning of capture in order to see last bars
			if(action == encoderActions.CAPTURE && radio_encoder_capture_cont.Active) {
				prepareEncoderGraphs(false, true); //bars, signal
				plotCurvesGraphDoPlotMessage("Previous set");
			} else
				prepareEncoderGraphs(true, true);

			//eccaCreated = false;

			if(action == encoderActions.CAPTURE) {
				encoderStartVideoRecord();

				//remove treeview columns
				if( ! (action == encoderActions.CAPTURE && radio_encoder_capture_cont.Active) )
					treeviewEncoderCaptureRemoveColumns();

				encoderCaptureStringR = new List<string>();
				encoderCaptureStringR.Add(
						",series,exercise,mass,start,width,height," + 
						"meanSpeed,maxSpeed,maxSpeedT," +
						"meanPower,peakPower,peakPowerT,pp_ppt," +
						"meanForce, maxForce, maxForceT");

				string filename = UtilEncoder.GetEncoderCaptureTempFileName();
				if(File.Exists(filename))
					File.Delete(filename);

				encoderCaptureReadedLines = 0;
				deleteAllCapturedCurveFiles();

				capturingCsharp = encoderCaptureProcess.CAPTURING;
			}


			if(action == encoderActions.CAPTURE) {
				captureCurvesBarsData = new ArrayList();

				needToRefreshTreeviewCapture = false;

				if(encoderConfigurationCurrent.has_inertia)
				{
					eCapture = new EncoderCaptureInertial();
				} else
					eCapture = new EncoderCaptureGravitatory();

				int recordingTime = preferences.encoderCaptureTime;
				if(radio_encoder_capture_cont.Active)
					encoderProcessFinishContMode = false; //will be true when finish button is pressed

				string portName = "";
				if( ! (currentSession.Name == Constants.SessionSimulatedName && testsActive))
					portName = chronopicRegister.ConnectedOfType(ChronopicRegisterPort.Types.ENCODER).Port;

				eCapture.InitGlobal(
						encoder_capture_signal_drawingarea.Allocation.Width,
						encoder_capture_signal_drawingarea.Allocation.Height,
						recordingTime,
						preferences.encoderCaptureInactivityEndTime,
						radio_encoder_capture_cont.Active,
						findEccon(true),
						portName,
						(encoderConfigurationCurrent.has_inertia && eCaptureInertialBG != null),
						configChronojump.EncoderCaptureShowOnlyBars,
						currentSession.Name == Constants.SessionSimulatedName && testsActive
						);

				if(encoderConfigurationCurrent.has_inertia && eCaptureInertialBG != null)
				{
					eCaptureInertialBG.StoreData = true;
					eCapture.InitCalibrated(eCaptureInertialBG.AngleNow);

					if(currentSession.Name == Constants.SessionSimulatedName && testsActive)
						eCaptureInertialBG.SimulatedReset();
				}

				encoderThread = new Thread(new ThreadStart(encoderDoCaptureCsharp));
				GLib.Idle.Add (new GLib.IdleHandler (pulseGTKEncoderCaptureAndCurves));
			}
			else { //action == encoderActions.CAPTURE_IM)

				eCapture = new EncoderCaptureIMCalc();
				eCapture.InitGlobal(
						encoder_capture_signal_drawingarea.Allocation.Width,
						encoder_capture_signal_drawingarea.Allocation.Height,
						preferences.encoderCaptureTimeIM,
						preferences.encoderCaptureInactivityEndTime,
						false,
						findEccon(true),
						chronopicRegister.ConnectedOfType(ChronopicRegisterPort.Types.ENCODER).Port,
						false,
						false,
						false
						);

				encoderThread = new Thread(new ThreadStart(encoderDoCaptureCsharpIM));
				GLib.Idle.Add (new GLib.IdleHandler (pulseGTKEncoderCaptureIM));
			}

			encoderShowCaptureDoingButtons(true);

			LogB.Information("encoderThreadStart middle");
			encoderButtonsSensitive(encoderSensEnum.PROCESSINGCAPTURE);

			LogB.ThreadStart();
			LogB.Mute = true; //mute logs to improve stability (encoder inertial test only works with muted log
			encoderThread.Start();
		} else if(
				action == encoderActions.CURVES || 
				action == encoderActions.LOAD ||
				action == encoderActions.CURVES_AC)	//this does not run a pulseGTK
		{
				
			if(action == encoderActions.CURVES || action == encoderActions.LOAD) 
			{
				//______ 1) prepareEncoderGraphs
				
				//image_encoder_width = UtilGtk.WidgetWidth(viewport_image_encoder_capture)-5; 
				//make graph half width of Chronojump window
				//but if video is disabled, then make it wider because thegraph will be much taller
				if(configChronojump.UseVideo)
					image_encoder_width = Convert.ToInt32(UtilGtk.WidgetWidth(app1) / 2);
				else
					image_encoder_width = Convert.ToInt32(UtilGtk.WidgetWidth(app1));
				
				//-2 to accomadate the width slider without needing a height slider
				image_encoder_height = UtilGtk.WidgetHeight(viewport_image_encoder_capture) -2;

				prepareEncoderGraphs(true, true);
				
				
				//_______ 2) run stuff
				
				//don't need because ItemToggled is deactivated during capture
				//treeview_encoder_capture_curves.Sensitive = false;
				
				encoderThread = new Thread(new ThreadStart(encoderDoCurvesGraphR_curves));
				if(action == encoderActions.CURVES)
					GLib.Idle.Add (new GLib.IdleHandler (pulseGTKEncoderCurves));
				else // action == encoderActions.LOAD
					GLib.Idle.Add (new GLib.IdleHandler (pulseGTKEncoderLoad));
				encoderButtonsSensitive(encoderSensEnum.PROCESSINGR);
				
				LogB.ThreadStart();
				encoderThread.Start(); 
			} else { //CURVES_AC
				//______ 1) prepareEncoderGraphs
				//don't call directly to prepareEncoderGraphs() here because it's called from a Non-GTK thread
				needToCallPrepareEncoderGraphs = true; //needToCallPrepareEncoderGraphs will not erase them
				
				//this is defined on capture process
				//image_encoder_width = UtilGtk.WidgetWidth(viewport_image_encoder_capture)-5; 
				//image_encoder_height = UtilGtk.WidgetHeight(viewport_image_encoder_capture)-5;
				
				
				//_______ 2) run stuff
				//this does not run a pulseGTK
				encoderDoCurvesGraphR_curvesAC();
				encoderButtonsSensitive(encoderSensEnum.PROCESSINGR);
			}
		} else { //encoderActions.ANALYZE
			
			//the -5 is because image is inside (is smaller than) viewport
			image_encoder_width = UtilGtk.WidgetWidth(scrolledwindow_image_encoder_analyze)-10;
			image_encoder_height = UtilGtk.WidgetHeight(scrolledwindow_image_encoder_analyze)-10;
			if(encoderAnalysis == "single") {
				image_encoder_height -= UtilGtk.WidgetHeight(table_encoder_analyze_instant); //to allow hslides and table
			}

			encoder_pulsebar_analyze.Text = Catalog.GetString("Please, wait.");
			encoderRProcAnalyze.status = EncoderRProc.Status.WAITING;
	
			encoderRProcAnalyze.CrossValidate = checkbutton_crossvalidate.Active;
		
			encoderThread = new Thread(new ThreadStart(encoderDoAnalyze));
			GLib.Idle.Add (new GLib.IdleHandler (pulseGTKEncoderAnalyze));

			encoderButtonsSensitive(encoderSensEnum.PROCESSINGR);
			treeview_encoder_analyze_curves.Sensitive = false;
			button_encoder_analyze_image_save.Sensitive = false;
			button_encoder_analyze_AB_save.Sensitive = false;
			button_encoder_analyze_table_save.Sensitive = false;
			button_encoder_analyze_1RM_save.Visible = false;

			LogB.ThreadStart();
			encoderThread.Start(); 
		}
	}

	//while capturing, some buttons are hidden, others are shown
	void encoderShowCaptureDoingButtons(bool show) {
		hbox_encoder_capture_wait.Visible = ! show;
		vbox_encoder_capture_doing.Visible = show;

		button_encoder_capture_cancel.Visible = ! radio_encoder_capture_cont.Active;
		button_encoder_capture_finish.Visible = ! radio_encoder_capture_cont.Active;
		button_encoder_capture_finish_cont.Visible = radio_encoder_capture_cont.Active;
	}

	void prepareEncoderGraphs(bool eraseBars, bool eraseSignal) 
	{
		LogB.Debug("prepareEncoderGraphs() start (should be on first thread: GTK)");
		
		if(eraseBars && encoder_capture_curves_bars_pixmap != null)
			UtilGtk.ErasePaint(encoder_capture_curves_bars_drawingarea, encoder_capture_curves_bars_pixmap);

		if(eraseSignal && encoder_capture_signal_pixmap != null)
			UtilGtk.ErasePaint(encoder_capture_signal_drawingarea, encoder_capture_signal_pixmap);


		layout_encoder_capture_signal = new Pango.Layout (encoder_capture_signal_drawingarea.PangoContext);
		layout_encoder_capture_signal.FontDescription = Pango.FontDescription.FromString ("Courier 10");

		layout_encoder_capture_curves_bars = new Pango.Layout (encoder_capture_curves_bars_drawingarea.PangoContext);
		layout_encoder_capture_curves_bars.FontDescription =
			Pango.FontDescription.FromString ("Courier " + preferences.encoderCaptureBarplotFontSize.ToString());
		
		layout_encoder_capture_curves_bars_text = new Pango.Layout (encoder_capture_curves_bars_drawingarea.PangoContext);
		layout_encoder_capture_curves_bars_text.FontDescription = Pango.FontDescription.FromString ("Courier 10");

		//defined as encoder_capture_curves_bars_drawingarea instead of encoder_capture_signal_drawingarea
		//because the 2nd is null if config.EncoderCaptureShowOnlyBars == TRUE
		pen_black_encoder_capture = new Gdk.GC(encoder_capture_curves_bars_drawingarea.GdkWindow);
		pen_gray = new Gdk.GC(encoder_capture_curves_bars_drawingarea.GdkWindow);
		pen_red_encoder_capture = new Gdk.GC(encoder_capture_curves_bars_drawingarea.GdkWindow);
		pen_red_light_encoder_capture = new Gdk.GC(encoder_capture_curves_bars_drawingarea.GdkWindow);
		pen_green_encoder_capture = new Gdk.GC(encoder_capture_curves_bars_drawingarea.GdkWindow);
		pen_blue_encoder_capture = new Gdk.GC(encoder_capture_curves_bars_drawingarea.GdkWindow);
		pen_white_encoder_capture = new Gdk.GC(encoder_capture_curves_bars_drawingarea.GdkWindow);
		pen_selected_encoder_capture = new Gdk.GC(encoder_capture_curves_bars_drawingarea.GdkWindow);
		pen_red_light_encoder_capture = new Gdk.GC(encoder_capture_curves_bars_drawingarea.GdkWindow);
		pen_red_dark_encoder_capture = new Gdk.GC(encoder_capture_curves_bars_drawingarea.GdkWindow);
		pen_green_light_encoder_capture = new Gdk.GC(encoder_capture_curves_bars_drawingarea.GdkWindow);
		pen_green_dark_encoder_capture = new Gdk.GC(encoder_capture_curves_bars_drawingarea.GdkWindow);
		pen_blue_dark_encoder_capture = new Gdk.GC(encoder_capture_curves_bars_drawingarea.GdkWindow);
		pen_blue_light_encoder_capture = new Gdk.GC(encoder_capture_curves_bars_drawingarea.GdkWindow);
		pen_yellow_encoder_capture = new Gdk.GC(encoder_capture_curves_bars_drawingarea.GdkWindow);

		Gdk.Colormap colormap = Gdk.Colormap.System;
		colormap.AllocColor (ref UtilGtk.BLACK,true,true);
		colormap.AllocColor (ref UtilGtk.GRAY,true,true);
		colormap.AllocColor (ref UtilGtk.RED_PLOTS,true,true);
		colormap.AllocColor (ref UtilGtk.RED_DARK,true,true);
		colormap.AllocColor (ref UtilGtk.RED_LIGHT,true,true);
		colormap.AllocColor (ref UtilGtk.GREEN_PLOTS,true,true);
		colormap.AllocColor (ref UtilGtk.GREEN_DARK,true,true);
		colormap.AllocColor (ref UtilGtk.GREEN_LIGHT,true,true);
		colormap.AllocColor (ref UtilGtk.BLUE_PLOTS,true,true);
		colormap.AllocColor (ref UtilGtk.BLUE_DARK,true,true);
		colormap.AllocColor (ref UtilGtk.BLUE_LIGHT,true,true);
		colormap.AllocColor (ref UtilGtk.YELLOW,true,true);
		colormap.AllocColor (ref UtilGtk.WHITE,true,true);
		colormap.AllocColor (ref UtilGtk.SELECTED,true,true);

		pen_black_encoder_capture.Foreground = UtilGtk.BLACK;
		pen_gray.Foreground = UtilGtk.GRAY;
		pen_red_encoder_capture.Foreground = UtilGtk.RED_PLOTS;
		pen_red_dark_encoder_capture.Foreground = UtilGtk.RED_DARK;
		pen_red_light_encoder_capture.Foreground = UtilGtk.RED_LIGHT;
		pen_green_encoder_capture.Foreground = UtilGtk.GREEN_PLOTS;
		pen_green_dark_encoder_capture.Foreground = UtilGtk.GREEN_DARK;
		pen_green_light_encoder_capture.Foreground = UtilGtk.GREEN_LIGHT;
		pen_blue_encoder_capture.Foreground = UtilGtk.BLUE_PLOTS;
		pen_blue_dark_encoder_capture.Foreground = UtilGtk.BLUE_DARK;
		pen_blue_light_encoder_capture.Foreground = UtilGtk.BLUE_LIGHT;
		pen_yellow_encoder_capture.Foreground = UtilGtk.YELLOW;
		pen_white_encoder_capture.Foreground = UtilGtk.WHITE;
		pen_selected_encoder_capture.Foreground = UtilGtk.SELECTED;

		pen_black_encoder_capture.SetLineAttributes (2, Gdk.LineStyle.Solid, Gdk.CapStyle.NotLast, Gdk.JoinStyle.Miter);
		pen_selected_encoder_capture.SetLineAttributes (2, Gdk.LineStyle.Solid, Gdk.CapStyle.NotLast, Gdk.JoinStyle.Miter);
		
		LogB.Debug("prepareEncoderGraphs() end");
	}



	private void runEncoderCaptureNoRDotNetInitialize() 
	{
		EncoderParams ep = new EncoderParams(
				preferences.EncoderCaptureMinHeight(encoderConfigurationCurrent.has_inertia), 
				getExercisePercentBodyWeightFromComboCapture (),
				Util.ConvertToPoint(findMass(Constants.MassType.BODY)),
				Util.ConvertToPoint(findMass(Constants.MassType.EXTRA)),
				findEccon(true),					//force ecS (ecc-conc separated)
				"-",		//analysis
				"none",		//analysisVariables (not needed in create curves). Cannot be blank
				getEncoderAnalysisOptions(),	//used on capture for pass the 'p' of propulsive
				preferences.encoderCaptureCheckFullyExtended,
				preferences.encoderCaptureCheckFullyExtendedValue,
				encoderConfigurationCurrent,
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
				ep);

		encoderRProcCapture.StartOrContinue(es);
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
	 * 1) In the beginning we used RDotNet for C# - R communication. But it was buggy, complex, problems with try catch, ...
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

			encoderCaptureStringR.Add(string.Format("\n" + 
						"{0},2,a,3,4," + //id, seriesName, exerciseName, massBody, massExtra
						"{1},{2},{3}," + //start, width, height
						"{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}",
					strs[0],
					strs[1], strs[2], strs[3],	//start, width, height
					strs[4], strs[5], strs[6],	//speeds
					strs[7], strs[8], strs[9],	//powers
					strs[10],			//pp/ppt
					strs[11], strs[12], strs[13]));	//forces

			//LogB.Debug("encoderCaptureStringR");
			//LogB.Debug(encoderCaptureStringR);

			double meanSpeed = Convert.ToDouble(Util.ChangeDecimalSeparator(strs[4]));
			double maxSpeed = Convert.ToDouble(Util.ChangeDecimalSeparator(strs[5]));
			double meanForce = Convert.ToDouble(Util.ChangeDecimalSeparator(strs[11]));
			double maxForce = Convert.ToDouble(Util.ChangeDecimalSeparator(strs[12]));
			double meanPower = Convert.ToDouble(Util.ChangeDecimalSeparator(strs[7]));
			double peakPower = Convert.ToDouble(Util.ChangeDecimalSeparator(strs[8]));
			captureCurvesBarsData.Add(new EncoderBarsData(meanSpeed, maxSpeed, meanForce, maxForce, meanPower, peakPower));
			
			LogB.Information("activating needToRefreshTreeviewCapture");

			//executed on GTK thread pulse method
			needToRefreshTreeviewCapture = true;
		}
	}

	bool shownWaitAtInertialCapture;
	bool calledCaptureInertial;
	DateTime timeCalibrated;
	private bool pulseGTKEncoderCaptureBG ()
	{
		if(! encoderThreadBG.IsAlive) {
			return false;
		}

		if(! shownWaitAtInertialCapture)
		{
			button_encoder_inertial_calibrate.Sensitive = false;
			label_wait.Text = string.Format("Exercise will start in {0} seconds.", 3);
			shownWaitAtInertialCapture = true;
		}

		if(! calledCaptureInertial)
		{
			int elapsed = Convert.ToInt32(DateTime.Now.Subtract(timeCalibrated).TotalSeconds);
			if(elapsed > 3)
			{
				calledCaptureInertial = true;
				on_button_encoder_capture_clicked_do (true);
			} else
				label_wait.Text = string.Format("Exercise will start in {0} seconds.", 3 - elapsed);
		}

		int newValue = eCaptureInertialBG.AngleNow;
		//resize vscale if needed
		//0 is at the graphical top. abs(+-100) is on the bottom, but is called adjustment Upper
		int upper = Convert.ToInt32(vscale_encoder_capture_inertial_angle_now.Adjustment.Upper);
		if(Math.Abs(newValue) > upper)
			vscale_encoder_capture_inertial_angle_now.SetRange(0, upper *2);

		//update vscale value
		vscale_encoder_capture_inertial_angle_now.Value = Math.Abs(newValue);
		label_encoder_capture_inertial_angle_now.Text = newValue.ToString();


		Thread.Sleep (50);

		//don't plot info here because this is working all the time
		//LogB.Information(" CapBG:"+ encoderThreadBG.ThreadState.ToString());

		if(newValue < -100000 || newValue > 100000)
		{
			LogB.Information("Encoder seems to be disconnected");
			stopCapturingInertialBG();
		}

		return true;
	}
				
	static bool needToCallPrepareEncoderGraphs; //this will not erase them
	private bool pulseGTKEncoderCaptureAndCurves ()
	{
		if(needToCallPrepareEncoderGraphs) 
		{
			image_encoder_width = UtilGtk.WidgetWidth(viewport_image_encoder_capture)-5; 
			image_encoder_height = UtilGtk.WidgetHeight(viewport_image_encoder_capture)-5;
				
			prepareEncoderGraphs(false, false); //do not erase them
			needToCallPrepareEncoderGraphs = false;
		}

		//if on inertia and already showing instructions, return to page 0
		if(notebook_encoder_capture_or_exercise_or_instructions.Page == 2)
			notebook_encoder_capture_or_exercise_or_instructions.Page = 0;

		if(! encoderThread.IsAlive || encoderProcessCancel)
		{
			LogB.Information("End from capture"); 
			LogB.ThreadEnding();

			if(eCaptureInertialBG != null)
				eCaptureInertialBG.StoreData = false;

			finishPulsebar(encoderActions.CURVES_AC);

			if(encoderProcessCancel) {
				//stop video		
				encoderStopVideoRecord();
			}

			LogB.ThreadEnded(); 
			return false;
		}
		if(capturingCsharp == encoderCaptureProcess.CAPTURING) 
		{
			updatePulsebar(encoderActions.CAPTURE); //activity on pulsebar
			
			//capturingSendCurveToR(); //unused, done while capturing
			readingCurveFromR();

			if(encoderConfigurationCurrent.has_inertia)
				updateEncoderCaptureGraphPaint(UpdateEncoderPaintModes.INERTIAL);
			else
				updateEncoderCaptureGraphPaint(UpdateEncoderPaintModes.GRAVITATORY);

			if(needToRefreshTreeviewCapture) 
			{
				//LogB.Error("HERE YES");
				//LogB.Error(encoderCaptureStringR);

				treeviewEncoderCaptureRemoveColumns();
				eCapture.Ecca.curvesAccepted = createTreeViewEncoderCapture(encoderCaptureStringR);

				//if(plotCurvesBars) {
				string mainVariable = Constants.GetEncoderVariablesCapture(preferences.encoderCaptureMainVariable);
				double mainVariableHigher = repetitiveConditionsWin.GetMainVariableHigher(mainVariable);
				double mainVariableLower = repetitiveConditionsWin.GetMainVariableLower(mainVariable);
				//TODO:
				//captureCurvesBarsData.Add(new EncoderBarsData(meanSpeed, maxSpeed, meanPower, peakPower));
				//captureCurvesBarsData.Add(new EncoderBarsData(20, 39, 10, 40));

				plotCurvesGraphDoPlot(mainVariable, mainVariableHigher, mainVariableLower, captureCurvesBarsData, 
						repetitiveConditionsWin.EncoderInertialDiscardFirstThree,
						true);	//capturing
				//}

				needToRefreshTreeviewCapture = false;
			}

			//changed trying to fix crash of nuell 27/may/2016
			//LogB.Debug(" Cap:", encoderThread.ThreadState.ToString());
			//LogB.Information(" Cap:" + encoderThread.ThreadState.ToString());
		} else if(capturingCsharp == encoderCaptureProcess.STOPPING) {
			//stop video		
			encoderStopVideoRecord();

			//don't allow to press cancel or finish
			button_encoder_capture_cancel.Sensitive = false;
			button_encoder_capture_finish.Sensitive = false;
			button_encoder_capture_finish_cont.Sensitive = false;

			capturingCsharp = encoderCaptureProcess.STOPPED;
		} else {	//STOPPED	
			LogB.Debug("at pulseGTKEncoderCaptureAndCurves stopped");		
			//do curves, capturingCsharp has ended
			updatePulsebar(encoderActions.CURVES); //activity on pulsebar
			//LogB.Debug(" Cur:", encoderThread.ThreadState.ToString());
			LogB.Information(" Cur:" + encoderThread.ThreadState.ToString());
		}
			
		Thread.Sleep (50);

		return true;
	}
	
	private bool pulseGTKEncoderCaptureIM ()
	{
		if(! encoderThread.IsAlive || encoderProcessCancel) {
			LogB.ThreadEnding(); 
			finishPulsebar(encoderActions.CAPTURE_IM);
			
			LogB.ThreadEnded(); 
			return false;
		}
		updatePulsebar(encoderActions.CAPTURE_IM); //activity on pulsebar
		updateEncoderCaptureGraphPaint(UpdateEncoderPaintModes.CALCULE_IM);

		Thread.Sleep (25);
		//LogB.Debug(" CapIM:", encoderThread.ThreadState.ToString());
		LogB.Information(" CapIM:"+ encoderThread.ThreadState.ToString());
		return true;
	}
	
	
	private bool pulseGTKEncoderCurves ()
	{
		if(! encoderThread.IsAlive || encoderProcessCancel) {
			LogB.Information("End from curves"); 
			LogB.ThreadEnding(); 
			if(encoderProcessCancel){
				encoderRProcAnalyze.CancelRScript = true;
			}

			finishPulsebar(encoderActions.CURVES);
			
			LogB.ThreadEnded(); 
			return false;
		}
		updatePulsebar(encoderActions.CURVES); //activity on pulsebar
		Thread.Sleep (50);
		//LogB.Debug(" Cur:", encoderThread.ThreadState.ToString());
		LogB.Information(" Cur:" + encoderThread.ThreadState.ToString());
		return true;
	}
	
	private bool pulseGTKEncoderLoad ()
	{
		if(! encoderThread.IsAlive || encoderProcessCancel) {
			LogB.ThreadEnding(); 
			if(encoderProcessCancel){
				encoderRProcAnalyze.CancelRScript = true;
			}

			finishPulsebar(encoderActions.LOAD);
			
			LogB.ThreadEnded(); 
			return false;
		}
		updatePulsebar(encoderActions.LOAD); //activity on pulsebar
		Thread.Sleep (50);
		//LogB.Debug(" L:", encoderThread.ThreadState.ToString());
		LogB.Information(" L:" + encoderThread.ThreadState.ToString());
		return true;
	}
	
	private bool pulseGTKEncoderAnalyze ()
	{
		if( encoderRProcAnalyze.status == EncoderRProc.Status.DONE || ! encoderThread.IsAlive || encoderProcessCancel) {
			LogB.ThreadEnding(); 
			if(encoderProcessCancel){
				encoderRProcAnalyze.CancelRScript = true;
			}

			finishPulsebar(encoderActions.ANALYZE);
			
			LogB.ThreadEnded(); 
			return false;
		}
		updatePulsebar(encoderActions.ANALYZE); //activity on pulsebar
		Thread.Sleep (50);
		//LogB.Debug(" A:", encoderThread.ThreadState.ToString());
		LogB.Information(" A:" + encoderThread.ThreadState.ToString());
		return true;
	}
	
	private void updatePulsebar (encoderActions action) 
	{
		if(action == encoderActions.CAPTURE && radio_encoder_capture_cont.Active) {
			encoder_pulsebar_capture.Text = "";
			encoder_pulsebar_capture.Pulse();
			return;
		}

		if(action == encoderActions.CAPTURE || action == encoderActions.CAPTURE_IM) 
		{
			int selectedTime = preferences.encoderCaptureTime;
			if(action == encoderActions.CAPTURE_IM)
				selectedTime = preferences.encoderCaptureTimeIM;

			encoder_pulsebar_capture.Fraction = Util.DivideSafeFraction(
					(selectedTime - eCapture.Countdown), selectedTime);
			encoder_pulsebar_capture.Text = eCapture.Countdown + " s";
	
			return;
		}

		try {
			string contents = Catalog.GetString("Please, wait.");
			double fraction = -1;
			/*
			if(Util.FileExists(UtilEncoder.GetEncoderStatusTempFileName())) {
				contents = Util.ReadFile(UtilEncoder.GetEncoderStatusTempFileName(), true);
				//contents is:
				//(1/5) Starting R
				//(5/5) R tasks done

				//-48: ascii 0 char
				if(System.Char.IsDigit(contents[1]) && System.Char.IsDigit(contents[3]))
					fraction = Util.DivideSafeFraction(
							Convert.ToInt32(contents[1]-48), Convert.ToInt32(contents[3]-48) );
			}
			*/

			if(Util.FileExists(UtilEncoder.GetEncoderStatusTempBaseFileName() + "6.txt")) 
			{
				fraction = 6;
				contents = Catalog.GetString("R tasks done");
			}
			else if(Util.FileExists(UtilEncoder.GetEncoderStatusTempBaseFileName() + "5.txt")) 
			{
				fraction = 5;
				contents = "Smoothing done";
			} else if(Util.FileExists(UtilEncoder.GetEncoderStatusTempBaseFileName() + "4.txt")) 
			{
				fraction = 4;
				if(encoderRProcAnalyze.CurvesReaded > 0)
					contents = encoderRProcAnalyze.CurvesReaded.ToString();
				else
					contents = Catalog.GetString("Repetitions processed");
			} else if(Util.FileExists(UtilEncoder.GetEncoderStatusTempBaseFileName() + "3.txt")) 
			{
				fraction = 3;
				if(encoderRProcAnalyze.CurvesReaded > 0)
					contents = encoderRProcAnalyze.CurvesReaded.ToString();
				else
					contents = Catalog.GetString("Starting process");
			} else if(Util.FileExists(UtilEncoder.GetEncoderStatusTempBaseFileName() + "2.txt")) 
			{
				fraction = 2;
				contents = Catalog.GetString("Loading libraries");
			} else if(Util.FileExists(UtilEncoder.GetEncoderStatusTempBaseFileName() + "1.txt")) 
			{
				fraction = 1;
				contents = Catalog.GetString("Starting R");
			}

			if(action == encoderActions.CURVES || action == encoderActions.LOAD) {
				if(fraction == -1)
					encoder_pulsebar_capture.Pulse();
				else
					encoder_pulsebar_capture.Fraction = Util.DivideSafeFraction(fraction, 6); 
				
				encoder_pulsebar_capture.Text = contents;
			} else {
				if(fraction == -1)
					encoder_pulsebar_analyze.Pulse();
				else
					encoder_pulsebar_analyze.Fraction = Util.DivideSafeFraction(fraction, 6);

				encoder_pulsebar_analyze.Text = contents;
			}
		} catch {
			//UtilEncoder.GetEncoderStatusTempBaseFileName() 1,2,3,4,5 is deleted at the end of the process
			//this can make crash updatePulsebar sometimes
			LogB.Warning("catched at updatePulsebar");
		}
	}
	

	// -------------- drawingarea_encoder_analyze_instant
	
	Pixbuf drawingarea_encoder_analyze_cairo_pixbuf;
	
	[Widget] Gtk.DrawingArea drawingarea_encoder_analyze_instant;
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
		checkFile(Constants.EncoderCheckFileOp.ANALYZE_SAVE_AB);
	}

	public void on_drawingarea_encoder_analyze_instant_expose_event(object o, ExposeEventArgs args)
	{
		if(drawingarea_encoder_analyze_cairo_pixbuf == null)
			return;

		DrawingArea area = (DrawingArea) o;
		using (Cairo.Context g = Gdk.CairoHelper.Create (area.GdkWindow)) 
		{
			//add image
			Gdk.CairoHelper.SetSourcePixbuf (g, drawingarea_encoder_analyze_cairo_pixbuf, 0, 0);
			g.Paint();

			//add rectangle
			g.SetSourceRGBA(0.906, 0.745, 0.098, 1); //Chronojump yellow
			
			int xposa = eai.GetVerticalLinePosition(Convert.ToInt32(hscale_encoder_analyze_a.Value));
			g.MoveTo(xposa, 0);
			g.LineTo(xposa, drawingarea_encoder_analyze_cairo_pixbuf.Height);
		
			if(checkbutton_encoder_analyze_b.Active) {
				int xposb = eai.GetVerticalLinePosition(Convert.ToInt32(hscale_encoder_analyze_b.Value));

				if(xposb != xposa) {
					g.SetSourceRGBA(0.906, 0.745, 0.098, .5); //Chronojump yellow, half transp
					g.Rectangle(xposa ,0, xposb-xposa, drawingarea_encoder_analyze_cairo_pixbuf.Height);
					g.Fill();
				}
			}
			
			g.Stroke();

			g.GetTarget ().Dispose ();
		}
	}
	
	// -------------- end of drawingarea_encoder_analyze_instant


	bool captureContWithCurves = true;
	private void finishPulsebar(encoderActions action) {
		if(
				action == encoderActions.CAPTURE || 
				action == encoderActions.CAPTURE_IM || 
				action == encoderActions.CURVES || 
				action == encoderActions.CURVES_AC || 
				action == encoderActions.LOAD )
		{
			LogB.Information("ffffffinishPulsebarrrrr");
		
			//save video will be later at encoderSaveSignalOrCurve, because there encoderSignalUniqueID will be known
			
			if(action == encoderActions.CURVES || action == encoderActions.CURVES_AC)
				sensitiveGuiEventDone();
			
			if(encoderProcessCancel || encoderProcessProblems) {
				//tis notebook has capture (signal plotting), and curves (shows R graph)	
				if(notebook_encoder_capture.CurrentPage == 0 )
					notebook_encoder_capture.NextPage();
				encoder_pulsebar_capture.Fraction = 1;
			
				if(encoderProcessProblems) {
					new DialogMessage(Constants.MessageTypes.WARNING, 
							Catalog.GetString("Sorry. Error doing graph.") + 
							"\n" + Catalog.GetString("Maybe R is not installed.") + 
							"\n" + Catalog.GetString("Please, install it from here:") +
							"\n\nhttp://cran.cnr.berkeley.edu/bin/macosx/R-latest.pkg");
					encoderProcessProblems = false;
				} else {
					if(action == encoderActions.CAPTURE_IM)
						encoder_configuration_win.Button_encoder_capture_inertial_do_ended(0,"Cancelled");
					else
						encoder_pulsebar_capture.Text = Catalog.GetString("Cancelled");
				}
			}
			else if( (action == encoderActions.CAPTURE || action == encoderActions.CAPTURE_IM) 
					&& encoderProcessFinish ) {
				encoder_pulsebar_capture.Text = Catalog.GetString("Finished");
			} 
			else if(action == encoderActions.CURVES || action == encoderActions.CURVES_AC || action == encoderActions.LOAD) 
			{
				//this notebook has capture (signal plotting), and curves (shows R graph)	
				if(notebook_encoder_capture.CurrentPage == 0)
					notebook_encoder_capture.NextPage();

				//variables for plotting curves bars graph
				string mainVariable = Constants.GetEncoderVariablesCapture(preferences.encoderCaptureMainVariable);
				double mainVariableHigher = repetitiveConditionsWin.GetMainVariableHigher(mainVariable);
				double mainVariableLower = repetitiveConditionsWin.GetMainVariableLower(mainVariable);

				if(radio_encoder_capture_cont.Active && ! captureContWithCurves)
				{
					//will use captureCurvesBarsData (created on capture)
					LogB.Information("at fff with captureCurvesBarsData =");
					LogB.Information(captureCurvesBarsData.Count.ToString());
				} else {
					List<string> contents = Util.ReadFileAsStringList(UtilEncoder.GetEncoderCurvesTempFileName());

					image_encoder_capture = UtilGtk.OpenImageSafe(
							UtilEncoder.GetEncoderGraphTempFileName(),
							image_encoder_capture);

					encoderUpdateTreeViewCapture(contents); //this updates encoderCaptureCurves
					image_encoder_capture.Sensitive = true;

					captureCurvesBarsData = new ArrayList();
					foreach (EncoderCurve curve in encoderCaptureCurves) {
						captureCurvesBarsData.Add(new EncoderBarsData(
									Convert.ToDouble(curve.MeanSpeed),
									Convert.ToDouble(curve.MaxSpeed),
									Convert.ToDouble(curve.MeanForce),
									Convert.ToDouble(curve.MaxForce),
									Convert.ToDouble(curve.MeanPower),
									Convert.ToDouble(curve.PeakPower)
									));
					}
				}


				maxPowerIntersessionOnCapture = findMaxPowerIntersession();
				plotCurvesGraphDoPlot(mainVariable, mainVariableHigher, mainVariableLower, captureCurvesBarsData,
						repetitiveConditionsWin.EncoderInertialDiscardFirstThree,
						false);	//not capturing
		
				button_encoder_signal_save_comment.Label = Catalog.GetString("Save comment");
				button_encoder_signal_save_comment.Sensitive = false;
		
				//autosave signal (but not in load)
				if(action == encoderActions.CURVES || action == encoderActions.CURVES_AC)
				{
					bool needToAutoSaveCurve = false;
					if(
							encoderSignalUniqueID == "-1" &&	//if we just captured
							(preferences.encoderAutoSaveCurve == Constants.EncoderAutoSaveCurve.ALL ||
							preferences.encoderAutoSaveCurve == Constants.EncoderAutoSaveCurve.BEST ||
							preferences.encoderAutoSaveCurve == Constants.EncoderAutoSaveCurve.FROM4TOPENULTIMATE) )
						needToAutoSaveCurve = true;

					encoder_pulsebar_capture.Text = encoderSaveSignalOrCurve(false, "signal", 0); //this updates encoderSignalUniqueID

					if(needToAutoSaveCurve)
						encoderCaptureSaveCurvesAllNoneBest(preferences.encoderAutoSaveCurve,
								Constants.GetEncoderVariablesCapture(preferences.encoderCaptureMainVariable));

					if(action == encoderActions.CURVES_AC)
					{
						//1) unMute logs if preferences.muteLogs == true
						LogB.Mute = preferences.muteLogs;

						//1) save the triggers now that we have an encoderSignalUniqueID
						eCapture.SaveTriggers(Convert.ToInt32(encoderSignalUniqueID)); //dbcon is closed
						showTriggersAndTab();

						//2) send the json to server
						//check if encoderCaptureCurves > 0
						//(this is the case of a capture without repetitions or can have on ending cont mode)

						if(configChronojump.Compujump && encoderCaptureCurves.Count > 0)
						{
							UploadEncoderDataObject uo = new UploadEncoderDataObject(encoderCaptureCurves);


							/*
							 * Problems on Json by accents like "Pressió sobre banc"
							 * string exerciseName = UtilGtk.ComboGetActive(combo_encoder_exercise_capture);
							 * right now fixed in json.cs UploadEncoderData()
							 */

							LogB.Information("calling Upload");
							Json js = new Json();
							bool success = js.UploadEncoderData(
									currentPerson.UniqueID,
									1,
									UtilGtk.ComboGetActive(combo_encoder_exercise_capture),
									Util.ConvertToPoint(findMass(Constants.MassType.DISPLACED)), //this is only for gravitatory
									uo);
							LogB.Information(js.ResultMessage);
							LogB.Information("called Upload");

							if(! success) {
								LogB.Error(js.ResultMessage);
								bool showInWindow = false;
								if(showInWindow)
									new DialogMessage(
											"Chronojump",
											Constants.MessageTypes.WARNING,
											js.ResultMessage);
							}
						}
					}

				} else
					encoder_pulsebar_capture.Text = "";
		

				/*
				 * if we captured, but encoderSignalUniqueID has not been changed on encoderSaveSignalOrCurve
				 * because there are no curves (problems detecting, or minimal height so big
				 * then do not continue
				 * because with a value of -1 there will be problems in 
				 * SqliteEncoder.Select(false, Convert.ToInt32(encoderSignalUniqueID), ...)
				 */
				LogB.Information(" encoderSignalUniqueID:" + encoderSignalUniqueID);
				if(encoderSignalUniqueID != "-1")
				{
					// TODO: we never use findEccon() return value. We might be able to stop calling findEccon()
					// since it doesn't seem to do anything else other than returning the value.
					// This needs to be checked if working on this code.
					findEccon(true);

					/*
					 * (0) open Sqlite
					 * (1) if found curves of this signal
					 * 	(1a) this curves are with different eccon, or with different encoderConfiguration.name
					 * 		(1a1) delete the curves (files)
					 * 		(1a2) delete the curves (encoder table)
					 * 		(1a3) and also delete from (encoderSignalCurves table)
					 * 	(1b) or different exercise, or different laterality or different extraWeight, 
					 * 		or different encoderConfiguration (but the name is the same)
					 * 		(1b1) update curves with new data
					 * (2) update analyze labels and combos
					 * (3) update meanPower on SQL encoder
					 * (4) close Sqlite
					 */

					Sqlite.Open(); // (0)

					bool deletedUserCurves = false;
					EncoderSQL currentSignalSQL = (EncoderSQL) SqliteEncoder.Select(
							true, Convert.ToInt32(encoderSignalUniqueID), 0, 0, getEncoderGI(),
							-1, "", EncoderSQL.Eccons.ALL, 
							false, true)[0];


					ArrayList data = SqliteEncoder.Select(
							true, -1, currentPerson.UniqueID, currentSession.UniqueID, getEncoderGI(),
							-1, "curve", EncoderSQL.Eccons.ALL,  
							false, true);
					foreach(EncoderSQL eSQL in data) 
					{
						if(currentSignalSQL.GetDate(false) == eSQL.GetDate(false)) 		// (1)
						{
							// (1a)
							if(findEccon(true) != eSQL.eccon || 
									encoderConfigurationCurrent.name != eSQL.encoderConfiguration.name)
							{
								Util.FileDelete(eSQL.GetFullURL(false));					// (1a1)
								Sqlite.Delete(true, Constants.EncoderTable, Convert.ToInt32(eSQL.uniqueID));	// (1a2)
								SqliteEncoder.DeleteSignalCurveWithCurveID(true, Convert.ToInt32(eSQL.uniqueID)); // (1a3)
								deletedUserCurves = true;
							} else {							// (1b)
								if(currentSignalSQL.exerciseID != eSQL.exerciseID)
									Sqlite.Update(true, Constants.EncoderTable, "exerciseID",
											"", currentSignalSQL.exerciseID.ToString(),
											"uniqueID", eSQL.uniqueID.ToString());

								if(currentSignalSQL.extraWeight != eSQL.extraWeight)
									Sqlite.Update(true, Constants.EncoderTable, "extraWeight",
											"", currentSignalSQL.extraWeight,
											"uniqueID", eSQL.uniqueID.ToString());

								if(currentSignalSQL.laterality != eSQL.laterality)
									Sqlite.Update(true, Constants.EncoderTable, "laterality",
											"", currentSignalSQL.laterality,
											"uniqueID", eSQL.uniqueID.ToString());
								
								if( currentSignalSQL.encoderConfiguration.ToStringOutput(EncoderConfiguration.Outputs.SQL) !=
										eSQL.encoderConfiguration.ToStringOutput(EncoderConfiguration.Outputs.SQL) )
								{
									Sqlite.Update(true, Constants.EncoderTable, "encoderConfiguration",
											"", currentSignalSQL.encoderConfiguration.ToStringOutput(
												EncoderConfiguration.Outputs.SQL),
											"uniqueID", eSQL.uniqueID.ToString());
								}
							}
						}
					}
					if(deletedUserCurves) {
						//TODO: change encSelReps and this will change labels
						updateUserCurvesLabelsAndCombo(true); 	// (2)
					}

					// (3) update meanPower on SQL encoder
					findAndMarkSavedCurves(true, true); //SQL opened; update curve SQL records (like future1: meanPower)
					
					Sqlite.Close(); 					// (4)

				}
				
				playVideoEncoderPrepare(false); //do not play
				
				//set encoder video labels
				string videofile = Util.GetVideoFileName(currentSession.UniqueID, 
						Constants.TestTypes.ENCODER, Convert.ToInt32(encoderSignalUniqueID));
				if(videofile != null && videofile != "" && File.Exists(videofile)) {
					label_video_encoder_filename.Text = Util.GetVideoFileNameOnlyName(
							Constants.TestTypes.ENCODER, 
							Convert.ToInt32(encoderSignalUniqueID));
					textview_video_encoder_folder.Buffer.Text = Util.GetVideoFileNameOnlyFolder(currentSession.UniqueID);
					button_video_encoder_open_folder.Visible = true;
				} else
					button_video_encoder_open_folder.Visible = false;
			}

			if(action == encoderActions.CAPTURE_IM && ! encoderProcessCancel && ! encoderProcessProblems) 
			{
				string imResultText = Util.ChangeDecimalSeparator(
						Util.ReadFile(UtilEncoder.GetEncoderSpecialDataTempFileName(), true) );
				LogB.Information("imResultText = |" + imResultText + "|");

				if(imResultText == "NA" || imResultText == "-1" || imResultText == "")
					encoder_configuration_win.Button_encoder_capture_inertial_do_ended (0, "Error capturing. Maybe more oscillations are needed.");
				else {
					//script calculates Kg*m^2 -> GUI needs Kg*cm^2
					encoder_configuration_win.Button_encoder_capture_inertial_do_ended (
							Convert.ToDouble(imResultText) * 10000.0, "");
				}

				encoderButtonsSensitive(encoderSensEnum.DONENOSIGNAL);
			} else {
				encoderButtonsSensitive(encoderSensEnumStored);

				//an user has one active concentric curve
				//signal of this curve is loaded
				//user change to ecc-con and recalculate
				//then that concentrinc curve disappears
				//button_encoder_analyze have to be unsensitive because there are no curves:
				button_encoder_analyze_sensitiveness();

				//LogB.Debug(Enum.Parse(typeof(encoderActions), action.ToString()).ToString());
				//LogB.Debug(encoderProcessCancel.ToString());
						
				if(encoderProcessCancel)
					removeSignalFromGuiBecauseDeletedOrCancelled();
			}
			
			encoder_pulsebar_capture.Fraction = 1;
			//analyze_image_save only has not to be sensitive now because capture graph will be saved
			image_encoder_analyze.Sensitive = false;
			treeview_encoder_analyze_curves.Sensitive = false;
			button_encoder_analyze_image_save.Sensitive = false;
			button_encoder_analyze_AB_save.Sensitive = false;
			button_encoder_analyze_table_save.Sensitive = false;
			button_encoder_analyze_1RM_save.Visible = false;
		
			encoderShowCaptureDoingButtons(false);

			if(action == encoderActions.CURVES_AC)
			{
				restTime.AddOrModify(currentPerson.UniqueID, true);
				updateRestTimes();
			}

			//on inertial, check after capture if string was not fully extended and was corrected
			if(getMenuItemMode() == Constants.Menuitem_modes.POWERINERTIAL && 
					action == encoderActions.CURVES_AC && 
					Util.FileExists(UtilEncoder.GetEncoderSpecialDataTempFileName())) 
			{
				string str = Util.ReadFile(UtilEncoder.GetEncoderSpecialDataTempFileName(), true);
				if(str != null && str == "SIGNAL CORRECTED")
					new DialogMessage(Constants.MessageTypes.WARNING, 
						Catalog.GetString("Set corrected. string was not fully extended at the beginning."));
			}
		} else { //ANALYZE
			if(encoderProcessCancel) {
				encoder_pulsebar_analyze.Text = Catalog.GetString("Cancelled");
			} else {
				//TODO pensar en si s'ha de fer 1er amb mida petita i despres amb gran (en el zoom),
				//o si es una sola i fa alguna edicio
				
				if(encoderAnalysis == "single") {
					drawingarea_encoder_analyze_cairo_pixbuf = UtilGtk.OpenPixbufSafe(
							UtilEncoder.GetEncoderGraphTempFileName(),
							drawingarea_encoder_analyze_cairo_pixbuf);

					drawingarea_encoder_analyze_instant.QueueDraw(); //will fire ExposeEvent
					
					button_encoder_analyze_AB_save.Visible = checkbutton_encoder_analyze_b.Active;

					notebook_encoder_analyze.CurrentPage = 1;
				} else {
					//maybe image is still not readable
					image_encoder_analyze = UtilGtk.OpenImageSafe(
							UtilEncoder.GetEncoderGraphTempFileName(),
							image_encoder_analyze);
					
					button_encoder_analyze_AB_save.Visible = false;
					
					notebook_encoder_analyze.CurrentPage = 0;
				}

				encoder_pulsebar_analyze.Text = "";

				string contents = Util.ReadFile(UtilEncoder.GetEncoderAnalyzeTableTempFileName(), false);
				if (contents != null && contents != "") {
					if(radiobutton_encoder_analyze_neuromuscular_profile.Active) {
						treeviewEncoderAnalyzeRemoveColumns();
						encoderAnalyzeListStore = new Gtk.ListStore (typeof (EncoderCurve));
						createTreeViewEncoderAnalyzeNeuromuscular(contents);
					} else if(
							radiobutton_encoder_analyze_1RM.Active &&
							Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_analyze_1RM),
								encoderAnalyze1RMTranslation) == "1RM Indirect") {
						treeviewEncoderAnalyzeRemoveColumns();
						encoderAnalyzeListStore = new Gtk.ListStore (typeof (List<double>));
					} else {
						treeviewEncoderAnalyzeRemoveColumns();
						encoderAnalyzeListStore = new Gtk.ListStore (typeof (EncoderCurve));
						createTreeViewEncoderAnalyze(contents);
					}
				}

				if(encoderAnalysis == "single") {
					eai = new EncoderAnalyzeInstant();
					eai.ReadArrayFile(UtilEncoder.GetEncoderInstantDataTempFileName());
					eai.ReadGraphParams(UtilEncoder.GetEncoderSpecialDataTempFileName());

					//ranges should have max value the number of the lines of csv file minus the header
					hscale_encoder_analyze_a.SetRange(1, eai.speed.Count);
					hscale_encoder_analyze_b.SetRange(1, eai.speed.Count);
					//eai.PrintDebug();
				}

			}
		
			button_encoder_analyze.Visible = true;
			hbox_encoder_analyze_progress.Visible = false;

			encoder_pulsebar_analyze.Fraction = 1;
			encoderButtonsSensitive(encoderSensEnumStored);
			image_encoder_analyze.Sensitive = true;
			treeview_encoder_analyze_curves.Sensitive = true;
			
			button_encoder_analyze_image_save.Sensitive = true;
			button_encoder_analyze_AB_save.Sensitive = true;
			button_encoder_analyze_table_save.Sensitive = true;
			
			string my1RMName = Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_analyze_1RM),
						encoderAnalyze1RMTranslation);
			button_encoder_analyze_1RM_save.Visible = 
				(radiobutton_encoder_analyze_1RM.Active &&
				(my1RMName == "1RM Bench Press" || my1RMName == "1RM Squat" ||
				 my1RMName == "1RM Any exercise" || my1RMName == "1RM Indirect") );
			/*
			 * TODO: currently disabled because 
			 * on_button_encoder_analyze_1RM_save_clicked () reads getExerciseNameFromTable()
			 * and encoderAnalyzeListStore is not created because "1RM Indirect" 
			 * currently prints no data on OutputData1
			 *
			 * Solution will be to print data there with a new format 
			 * (new columns) like neuromuscular has done
			 */
		}

		treeview_encoder_capture_curves.Sensitive = true;

		//delete the status filenames
		Util.FileDelete(UtilEncoder.GetEncoderStatusTempBaseFileName() + "1.txt");
		Util.FileDelete(UtilEncoder.GetEncoderStatusTempBaseFileName() + "2.txt");
		Util.FileDelete(UtilEncoder.GetEncoderStatusTempBaseFileName() + "3.txt");
		Util.FileDelete(UtilEncoder.GetEncoderStatusTempBaseFileName() + "4.txt");
		Util.FileDelete(UtilEncoder.GetEncoderStatusTempBaseFileName() + "5.txt");
		Util.FileDelete(UtilEncoder.GetEncoderStatusTempBaseFileName() + "6.txt");
			
		if(action == encoderActions.CURVES_AC && radio_encoder_capture_cont.Active && ! encoderProcessFinishContMode)
			on_button_encoder_capture_clicked_do (false);

		//for chronojumpWindowTests
		LogB.Information("finishPulseBar DONE: " + action.ToString());
		if(
				action == encoderActions.LOAD ||	//load 
				action == encoderActions.CURVES ||	//recalculate
				action == encoderActions.CURVES_AC) 	//curves after capture
			chronojumpWindowTestsNext();
	}

	/*
	 * on capture treeview finds which rows are related to saved SQL curves
	 * mark their rows (meaning saved)
	 * also if updateSQLRecords, then update SQL meanPower of the curve
	 *
	 * This method is called by on_repetitive_conditions_closed, and finishPulsebar
	 */
	private void findAndMarkSavedCurves(bool dbconOpened, bool updateSQLRecords) 
	{
		//run this method with SQL opened to not be closing and opening a lot on the following foreachs
		if(! dbconOpened)
			Sqlite.Open();

		//find the saved curves
		ArrayList linkedCurves = SqliteEncoder.SelectSignalCurve(true, 
				Convert.ToInt32(encoderSignalUniqueID), //signal
				-1, -1, -1);				//curve, msStart,msEnd
		//LogB.Information("SAVED CURVES FOUND");
		//foreach(EncoderSignalCurve esc in linkedCurves)
		//	LogB.Information(esc.ToString());

		int curveCount = 0;
		double curveStart = 0;
		double curveEnd = 0;
		foreach (EncoderCurve curve in encoderCaptureCurves) 
		{
			if(findEccon(true) == "c") {
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
						//update the future1
						Sqlite.Update(true, Constants.EncoderTable, "future1",
								"", curve.MeanPower,
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

	
	/* end of thread stuff */
	
	/* video stuff */
	private void encoderStartVideoRecord() {
		LogB.Information("Starting video if selected on preferences");
		checkbutton_video_encoder.Sensitive = false;
		if(preferences.videoOn) {
			capturer.ClickRec();
			label_video_feedback_encoder.Text = "Rec.";
		}
		//viewport_video_capture_encoder.Sensitive = false;
	}

	private void encoderStopVideoRecord() {
		LogB.Information("Stopping video");
		checkbutton_video_encoder.Sensitive = true;
		if(preferences.videoOn) {
			label_video_feedback_encoder.Text = "";
			capturer.ClickStop();
			videoCapturePrepare(false); //if error, show message
		}
	}

	static PlayerBin playerEncoder;
	private void playVideoEncoderInitialSetup() //this does not work on raspberry
	{
		LogB.Information("Prepare video encoder");
		playerEncoder = new PlayerBin();
		viewport_video_play_encoder.Add(playerEncoder);
		playerEncoder.SeeControlsBox(true);
	}
	void playVideoEncoderPrepare(bool play) 
	{
		LogB.Information("playVideoEncoderDo", play.ToString());
		string file = Util.GetVideoFileName(currentSession.UniqueID, 
				Constants.TestTypes.ENCODER, Convert.ToInt32(encoderSignalUniqueID));

		if(playerEncoder == null) //useful for raspberry because this is not initialized
			return;

		if(file == null || file == "" || ! File.Exists(file)) {
			playerEncoder.Hide();
			return;
		}
		
		try {
			playerEncoder.Show();
			playerEncoder.Open(file);
			if(play)
				playerEncoder.Play();
		} catch {
			new DialogMessage(Constants.MessageTypes.WARNING, 
					Catalog.GetString("Sorry, file not found"));
		}
	}	
	
	public void on_radiobutton_video_encoder_capture_toggled (object obj, EventArgs args) {
		if(radiobutton_video_encoder_capture.Active) {
			notebook_video_encoder.CurrentPage = 0;
		}
	}
	public void on_radiobutton_video_encoder_play_toggled (object obj, EventArgs args) {
		if(radiobutton_video_encoder_play.Active) {
			notebook_video_encoder.CurrentPage = 1;
		}
	}
	public void on_radiobutton_video_encoder_options_toggled (object obj, EventArgs args) {
		if(radiobutton_video_encoder_options.Active) {
			notebook_video_encoder.CurrentPage = 2;
		}
	}
	public void on_button_video_encoder_open_folder_clicked (object obj, EventArgs args) {
		string dir = textview_video_encoder_folder.Buffer.Text;
		try {
			System.Diagnostics.Process.Start(dir); 
		}
		catch {
			new DialogMessage(Constants.MessageTypes.WARNING, 
					Constants.DirectoryCannotOpen + "\n\n" + dir);
		}
	}

	/* end of video stuff */

}	
