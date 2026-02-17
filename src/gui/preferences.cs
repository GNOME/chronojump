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
using Gdk;
using Gtk;
//using Glade;
//using Gnome;
//using GLib; //for Value
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using System.Collections.Generic; //List<T>
using Mono.Unix;
using System.Globalization; //CultureInfo stuff
using System.Diagnostics;  //Stopwatch
using System.Text.RegularExpressions; //Regex


/*
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib;
*/

public class PreferencesWindow
{
	// at glade ---->
	Gtk.Window preferences_win;
	Gtk.Notebook notebook_top;
	Gtk.Notebook notebook;
	Gtk.HBox hbox_buttons_bottom;

	//view more tabs
	Gtk.CheckButton check_view_jumps;
	Gtk.CheckButton check_view_runs;
	Gtk.CheckButton check_view_weights_inertial;
	Gtk.CheckButton check_view_isometric_elastic;
	//tabs selection widgets
	Gtk.Image image_view_more_tabs_close;
	Gtk.Label label_mandatory_tabs;
	Gtk.Label label_selectable_tabs;

	//help widgets
	Gtk.HBox hbox_stiffness_formula;
	Gtk.TextView textview_help_message;
	Gtk.Image image_help_close;

	//main, person tabs
	Gtk.CheckButton check_appearance_maximized;
	Gtk.CheckButton check_appearance_maximized_undecorated;
	Gtk.CheckButton check_appearance_person_win_hide;
	Gtk.CheckButton check_appearance_person_clubID;
	Gtk.CheckButton check_appearance_person_photo;
	Gtk.RadioButton radio_font_size_default;
	Gtk.RadioButton radio_font_size_custom;
	Gtk.Box box_font_size_custom;
	Gtk.SpinButton spin_font_size_custom;
	Gtk.Alignment alignment_undecorated;
//	Gtk.Label label_recommended_undecorated;
	Gtk.RadioButton radio_font_courier;
	Gtk.RadioButton radio_font_helvetica;
	Gtk.RadioButton radio_font_noto_sans_cjk_sc;
	Gtk.CheckButton check_rest_time;
	Gtk.Image image_rest;
	Gtk.HBox hbox_rest_time_values;
	Gtk.SpinButton spinbutton_rest_minutes;
	Gtk.SpinButton spinbutton_rest_seconds;

	Gtk.RadioButton radio_color_custom;
	Gtk.RadioButton radio_color_chronojump_blue;
	Gtk.RadioButton radio_color_os;
	Gtk.DrawingArea drawingarea_background_color;
	Gtk.Button button_color_choose;
	Gtk.DrawingArea drawingarea_background_color_chronojump_blue;
	Gtk.Label label_radio_color_os_needs_restart;

	Gtk.CheckButton check_logo_animated;
	Gtk.HBox hbox_last_session_and_mode;
	Gtk.CheckButton check_session_autoload_at_start;
	Gtk.CheckButton check_mode_autoload_at_start;


	//jumps tab	
//	Gtk.Label label_jumps;
	Gtk.CheckButton checkbutton_power;
	Gtk.CheckButton checkbutton_stiffness;
	Gtk.Image image_jumps_power_help;
	Gtk.Image image_jumps_stiffness_help;
	Gtk.CheckButton checkbutton_initial_speed;
	Gtk.CheckButton checkbutton_jump_rsi;
//	Gtk.CheckButton checkbutton_angle;
	Gtk.CheckButton checkbutton_show_tv_tc_index;
	Gtk.Box hbox_indexes;
	Gtk.RadioButton radiobutton_show_q_index;
	Gtk.RadioButton radiobutton_show_dj_index;
	Gtk.RadioButton radio_weight_percent;
	Gtk.RadioButton radio_weight_kg;
	Gtk.RadioButton radio_use_heights_on_jump_indexes;
	Gtk.RadioButton radio_do_not_use_heights_on_jump_indexes;
			
	//runs tab	
	Gtk.Notebook notebook_races;
	Gtk.Image image_run_speed_start_help;
	Gtk.RadioButton radio_speed_ms;
	Gtk.RadioButton radio_speed_km;
	Gtk.RadioButton radio_runs_speed_start_arrival; 
	Gtk.RadioButton radio_runs_speed_start_leaving; 
	Gtk.Image image_races_simple;
	Gtk.Image image_races_intervallic;
	Gtk.Notebook notebook_races_double_contacts;
	Gtk.Box vbox_runs_prevent_double_contact;
	Gtk.CheckButton checkbutton_runs_prevent_double_contact;
	Gtk.SpinButton spinbutton_runs_prevent_double_contact;
	Gtk.Box vbox_runs_i_prevent_double_contact;
	Gtk.CheckButton checkbutton_runs_i_prevent_double_contact;
	Gtk.SpinButton spinbutton_runs_i_prevent_double_contact;
	
	//encoder tab
	Gtk.Notebook notebook_encoder;
	//capture
	Gtk.SpinButton spin_encoder_capture_time;
	Gtk.CheckButton check_encoder_capture_inactivity_end_time;
	Gtk.HBox hbox_encoder_capture_inactivity_time;
	Gtk.SpinButton spin_encoder_capture_inactivity_end_time;
	Gtk.HBox hbox_encoder_capture_curves_save;
	Gtk.SpinButton spin_encoder_capture_curves_best_n;
	Gtk.Label label_encoder_capture_save_repetitions_explanation;
	Gtk.Image image_encoder_gravitatory;
	Gtk.Image image_encoder_inertial;
	Gtk.Image image_encoder_inertial2;
	Gtk.Image image_encoder_triggers;
	Gtk.CheckButton checkbutton_encoder_capture_inertial_discard_first_n;
	Gtk.Box box_encoder_capture_inertial_discard_first_n;
	Gtk.SpinButton spin_encoder_capture_inertial_discard_first_n;
	Gtk.SpinButton spin_encoder_capture_show_only_some_bars;
	Gtk.RadioButton radio_encoder_capture_show_all_bars;
	Gtk.RadioButton radio_encoder_capture_show_only_some_bars;
	Gtk.SpinButton spin_encoder_capture_barplot_font_size;
	Gtk.CheckButton check_show_start_and_duration;
	Gtk.RadioButton radio_encoder_triggers_no;
	Gtk.RadioButton radio_encoder_triggers_yes;
	Gtk.VBox vbox_encoder_triggers_yes;
	Gtk.RadioButton radio_encoder_triggers_yes_start_at_capture;
	Gtk.RadioButton radio_encoder_triggers_yes_start_at_first_trigger;
	Gtk.Image image_encoder_inactivity_help;
	Gtk.Image image_encoder_capture_cut_by_triggers_help;
	Gtk.CheckButton check_encoder_capture_infinite;
	Gtk.Image image_encoder_capture_infinite;
	Gtk.RadioButton radio_encoder_rep_criteria_gravitatory_ecc_con;
	Gtk.RadioButton radio_encoder_rep_criteria_gravitatory_ecc;
	Gtk.RadioButton radio_encoder_rep_criteria_gravitatory_con;
	Gtk.RadioButton radio_encoder_rep_criteria_inertial_ecc_con;
	Gtk.RadioButton radio_encoder_rep_criteria_inertial_ecc;
	Gtk.RadioButton radio_encoder_rep_criteria_inertial_con;
	//analyze
	Gtk.CheckButton checkbutton_encoder_propulsive;
	Gtk.RadioButton radio_encoder_work_kcal;
	Gtk.RadioButton radio_encoder_work_joules;
	Gtk.RadioButton radio_encoder_inertial_analyze_equivalent_mass;
	Gtk.RadioButton radio_encoder_inertial_analyze_inertia_moment;
	Gtk.RadioButton radio_encoder_inertial_analyze_diameter;
	Gtk.Image image_encoder_inertial_analyze_eq_mass_help;
	Gtk.SpinButton spin_encoder_smooth_con;
	Gtk.Label label_encoder_con;
	Gtk.RadioButton radio_encoder_1RM_nonweighted;
	Gtk.RadioButton radio_encoder_1RM_weighted;
	Gtk.RadioButton radio_encoder_1RM_weighted2;
	Gtk.RadioButton radio_encoder_1RM_weighted3;

	//forceSensor tab
	Gtk.CheckButton check_force_sensor_isometric_butterworth;
	Gtk.CheckButton check_force_sensor_elastic_butterworth;
	Gtk.Box box_force_sensor_isometric_butterworth_values;
	Gtk.Box box_force_sensor_elastic_butterworth_values;
	Gtk.SpinButton spin_force_sensor_isometric_butterworth;
	Gtk.SpinButton spin_force_sensor_elastic_butterworth;
	Gtk.Notebook notebook_force_sensor;
	Gtk.SpinButton spin_force_sensor_capture_width_graph_seconds;
	Gtk.RadioButton radio_force_sensor_capture_zoom_out;
	Gtk.RadioButton radio_force_sensor_capture_scroll;
	Gtk.SpinButton spin_force_sensor_elastic_ecc_min_displ;
	Gtk.SpinButton spin_force_sensor_elastic_con_min_displ;
	Gtk.SpinButton spin_force_sensor_not_elastic_ecc_min_force;
	Gtk.SpinButton spin_force_sensor_not_elastic_con_min_force;
	Gtk.SpinButton spin_force_sensor_graphs_line_width;
	Gtk.RadioButton radio_force_sensor_variability_rmssd;
	Gtk.RadioButton radio_force_sensor_variability_cvrmssd;
	Gtk.RadioButton radio_force_sensor_variability_cv;
	Gtk.RadioButton radio_force_sensor_variability_old;
	Gtk.HBox hbox_force_sensor_lag;
	Gtk.SpinButton spin_force_sensor_variability_lag;
	Gtk.SpinButton spin_force_sensor_analyze_best_stability_in_window;
	Gtk.SpinButton spin_force_sensor_analyze_max_avg_force_in_window;

	//runEncoder tab
	Gtk.SpinButton spin_run_encoder_acceleration;
	Gtk.SpinButton spin_run_encoder_pps;
	Gtk.Label label_pps_equivalent;
	Gtk.Label label_pps_maximum;

	//multimedia tab
	Gtk.CheckButton checkbutton_volume;
	Gtk.Alignment alignment_multimedia_sounds;
	Gtk.RadioButton radio_gstreamer_0_1;
	Gtk.RadioButton radio_gstreamer_1_0;
	Gtk.RadioButton radio_ffplay;
	Gtk.RadioButton radio_sound_systemsounds;
	Gtk.HBox hbox_not_recommended_when_not_on_windows;
	Gtk.Label label_test_sound_result;
	Gtk.Notebook notebook_multimedia;
	Gtk.Box hbox_combo_camera;
	Gtk.HBox hbox_camera_resolution_framerate;
	Gtk.HBox hbox_camera_resolution_custom;
	Gtk.SpinButton spin_camera_resolution_custom_width;
	Gtk.SpinButton spin_camera_resolution_custom_height;
	Gtk.HBox hbox_camera_framerate_custom;
	Gtk.SpinButton spin_camera_framerate_custom;
	Gtk.Entry entry_camera_framerate_custom_decimals;
	//Gtk.Label label_camera_pixel_format;
	Gtk.Label label_camera_pixel_format_current;
	Gtk.Label label_camera_resolution_current;
	Gtk.Label label_camera_framerate_current;
	Gtk.HBox hbox_combo_camera_pixel_format;
	Gtk.Box hbox_combo_camera_resolution;
	Gtk.Box hbox_combo_camera_framerate;
	Gtk.Label label_camera_error;
	Gtk.Label label_webcam_windows;
	Gtk.Image image_multimedia_audio;
	Gtk.Image image_multimedia_video;
	Gtk.Image image_video_preview;
	Gtk.Button button_video_preview;
	Gtk.Label label_video_preview_error;
	Gtk.CheckButton check_camera_stop_after;
	Gtk.CheckButton check_camera_advanced;
	Gtk.Frame frame_camera_advanced;
	//Gtk.VBox vbox_camera_stop_after_all;
	//Gtk.VBox vbox_camera_stop_after;
	Gtk.HBox hbox_camera_stop_after_seconds;
	Gtk.SpinButton spin_camera_stop_after;
	Gtk.Grid grid_video_advanced_actions;
	Gtk.Label label_video_check_ffmpeg_running;
	Gtk.Label label_video_check_ffplay_running;
	Gtk.Button button_video_ffmpeg_kill;
	Gtk.Button button_video_ffplay_kill;
	Gtk.Label label_camera_check_running;
	Gtk.Notebook notebook_multimedia_video;

	//language tab
	Gtk.Box hbox_combo_language;
	Gtk.RadioButton radio_language_detected;
	Gtk.RadioButton radio_language_force;
	Gtk.RadioButton radio_graphs_translate;
	Gtk.RadioButton radio_graphs_no_translate;
		
	//advanced tab
	Gtk.Notebook notebook_advanced;
	Gtk.Image image_advanced_cloud;
	Gtk.Image image_advanced_logs;
	Gtk.Image image_advanced_more;
	Gtk.Grid grid_database;
	Gtk.Label label_database_id;
	Gtk.Entry entry_database_name;
	Gtk.CheckButton checkbutton_ask_deletion;
	Gtk.Box box_combo_decimals;
	Gtk.CheckButton checkbutton_mute_logs;
	Gtk.RadioButton radio_export_latin;
	Gtk.RadioButton radio_export_non_latin;
	Gtk.Label label_advanced_feedback;
	Gtk.Button button_delete_devices;
	Gtk.ToggleButton toggle_gc_collect_on_close;
	Gtk.ToggleButton toggle_never_close;
	Gtk.VBox vbox_version;
	Gtk.Label label_progVersion;
	Gtk.Frame frame_networks;
	Gtk.CheckButton check_networks_devices;

	// cloud
	Gtk.RadioButton radio_cloud_no;
	Gtk.RadioButton radio_cloud_capture;
	Gtk.RadioButton radio_cloud_view;
	Gtk.Button button_cloud_capture_path;
	Gtk.Button button_cloud_view_path;
	Gtk.Button button_cloud_view_databases;
	Gtk.Label label_radio_cloud_no;
	Gtk.Label label_radio_cloud_no_recommended;
	Gtk.Label label_radio_cloud_capture;
	Gtk.Label label_radio_cloud_view;
	Gtk.Image image_cloud_capture;
	Gtk.Image image_cloud_view;
	Gtk.Image image_cloud_schema;
	Gtk.Label label_cloud_capture_path;
	Gtk.Label label_cloud_view_path;
	// silicon
	Gtk.Box box_silicon_cloud_path_choose;
	Gtk.Box	box_silicon_cloud_path_capture;
	Gtk.Box box_silicon_cloud_path_view;
	Gtk.Entry entry_silicon_cloud_capture_path;
	Gtk.Entry entry_silicon_cloud_view_path;
	Gtk.Label label_silicon_cloud_path_does_not_exists;

	Gtk.Image image_advanced_bluetooth;
	Gtk.Entry entry_bluetooth_url;
	Gtk.Button button_bluetooth_start;
	Gtk.Button button_bluetooth_end;
	Gtk.TextView textview_bluetooth;

	Gtk.Button button_debug_mode;

	Gtk.Entry entry_send_log;
	Gtk.TextView textview_send_log_comments;
	Gtk.RadioButton radio_send_log_current;
	Gtk.RadioButton radio_send_log_previous;
	Gtk.Button button_send_log;
	Gtk.Image image_button_send_log;
	Gtk.Image image_send_log_no;
	Gtk.Image image_send_log_yes;
	Gtk.TextView textview_send_log_message;

	Gtk.Image image_advanced_r;
	Gtk.Image image_advanced_python;
	Gtk.RadioButton radio_r_default;
	Gtk.RadioButton radio_r_other;
	Gtk.Button button_r_choose;
	Gtk.Button button_r_autodetect;
	Gtk.Entry entry_r_user_location;
	Gtk.RadioButton radio_rscript_default;
	Gtk.RadioButton radio_rscript_other;
	Gtk.Button button_rscript_choose;
	Gtk.Button button_rscript_autodetect;
	Gtk.Button button_rscript_usr_local_bin;
	Gtk.Entry entry_rscript_user_location;
	Gtk.RadioButton radio_python_default;
	Gtk.RadioButton radio_python_other;
	Gtk.Button button_python_choose;
	Gtk.Button button_python_autodetect;
	Gtk.Entry entry_python_user_location;

	Gtk.RadioButton radio_python_2;
	Gtk.RadioButton radio_python_3;

	Gtk.Label label_restart;
	Gtk.HBox hbox_buttoms_bottom;
	Gtk.Button button_close;
	Gtk.Image image_button_close;
	// <---- at glade


	Gtk.ComboBoxText combo_encoder_capture_curves_save;
	Gtk.ComboBoxText combo_camera;
	Gtk.ComboBoxText combo_camera_pixel_format;
	Gtk.ComboBoxText combo_camera_resolution;
	Gtk.ComboBoxText combo_camera_framerate;
	Gtk.ComboBoxText combo_language;
	Gtk.ComboBoxText combo_decimals;

	public Gtk.Button FakeButtonMaximizeChanges;
	public Gtk.Button FakeButtonPersonWin;
	public Gtk.Button FakeButtonConfigurationImported;
	public Gtk.Button FakeButtonColorsChanged;
	public Gtk.Button FakeButtonDebugModeStart;
	public Gtk.Button FakeButtonDeleteDevices;
	
	static PreferencesWindow PWBox;

	private RGBA colorBackground;
	private bool signalsNoFollow;
	string [] encoderCaptureCurvesSaveOptionsTranslation;

	private UtilAll.OperatingSystems operatingSystem;
	private Preferences preferences; //stored to update SQL if anything changed
//	private Thread thread;

	/* using configAtPrefs and not pass configChronojump from app1
	 * this is done because at prefs we maybe change cloud stuff, and this will update chronojump_config file
	 * but not configChronojump. And then if we open preferences again, radios do not correspond to the previously changed radios.
	 * So better read config here all the time that will read cloud stuff on chronojump_config.txt
	 */
	//private Config configChronojump;
	private Config configAtPrefs;

	//string databaseURL;
	//string databaseTempURL;
	
	ListStore langsStore;

	private enum notebook_top_pages { PREFERENCES, SELECTTABS, HELP }

	const int JUMPSPAGE = 2;
	const int RUNSPAGE = 3;
	const int ISOMETRICELASTICPAGE = 4;
	const int WEIGHTSINERTIALPAGE = 5;

	static private WebcamDeviceList wd_list;
	private WebcamFfmpegSupportedModes wfsm;

	private static bool bluetoothHandlersAssigned; // to not have double feedback at 2nd preferences open

	PreferencesWindow ()
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "preferences_win.glade", "preferences_win", "chronojump");
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "preferences_win.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);
		
		//put an icon to window
		UtilGtk.IconWindow(preferences_win);
		preferences_win.Title = Catalog.GetString("Preferences");

		//database and log files stuff
		//databaseURL = Util.GetDatabaseDir() + System.IO.Path.DirectorySeparatorChar  + "chronojump.db";
		//databaseTempURL = Util.GetDatabaseTempDir() + System.IO.Path.DirectorySeparatorChar  + "chronojump.db";
		
		FakeButtonMaximizeChanges = new Gtk.Button ();
		FakeButtonPersonWin = new Gtk.Button ();
		FakeButtonConfigurationImported = new Gtk.Button();
		FakeButtonColorsChanged = new Gtk.Button ();
		FakeButtonDebugModeStart = new Gtk.Button();
		FakeButtonDeleteDevices = new Gtk.Button ();

		if (! bluetoothHandlersAssigned)
		{
			BluetoothLE.OnInstalling += BluetoothLE_OnInstalling;
			BluetoothLE.OnBleakVersion += BluetoothLE_OnBleakVersion;
			BluetoothLE.OnScanning += BluetoothLE_OnScanning;
			BluetoothLE.OnDataChanged += BluetoothLE_OnDataChanged;
			BluetoothLE.OnDeviceChanged += BluetoothLE_OnDeviceChanged;
			bluetoothHandlersAssigned = true;
		}
	}

	static public PreferencesWindow Show (
			Preferences preferences,
			//Constants.Modes menu_mode, bool compujump, Config configChronojump, string progVersion)
			Constants.Modes menu_mode, bool compujump, string progVersion)
	{
		if (PWBox == null) {
			PWBox = new PreferencesWindow ();
		}

		PWBox.notebook_top.CurrentPage = Convert.ToInt32(notebook_top_pages.PREFERENCES);
		PWBox.operatingSystem = UtilAll.GetOSEnum();
		//PWBox.configChronojump = configChronojump;
		PWBox.configAtPrefs = new Config ();
		PWBox.configAtPrefs.Read ();

		if(compujump)
		{
			PWBox.check_appearance_person_win_hide.Sensitive = false;

			//show version
			PWBox.vbox_version.Visible = true;
			PWBox.label_progVersion.Text = "<b>" + progVersion + "</b>";
			PWBox.label_progVersion.UseMarkup = true;
			PWBox.check_networks_devices.Active = preferences.networksAllowChangeDevices;
			PWBox.button_delete_devices.Sensitive = false;
		}
		PWBox.frame_networks.Visible = compujump;

		if(menu_mode !=	Constants.Modes.JUMPSSIMPLE && menu_mode != Constants.Modes.JUMPSREACTIVE)
		{
			PWBox.notebook.GetNthPage(JUMPSPAGE).Hide();
			PWBox.check_view_jumps.Active = false;
		} if(menu_mode != Constants.Modes.RUNSSIMPLE && menu_mode != Constants.Modes.RUNSINTERVALLIC &&
				menu_mode != Constants.Modes.RUNSENCODER)
		{
			PWBox.notebook.GetNthPage(RUNSPAGE).Hide();
			PWBox.check_view_runs.Active = false;
		} if(menu_mode != Constants.Modes.POWERGRAVITATORY && menu_mode != Constants.Modes.POWERINERTIAL)
		{
			PWBox.notebook.GetNthPage(WEIGHTSINERTIALPAGE).Hide();
			PWBox.check_view_weights_inertial.Active = false;
		}
		if(! Constants.ModeIsFORCESENSOR (menu_mode))
		{
			PWBox.notebook.GetNthPage(ISOMETRICELASTICPAGE).Hide();
			PWBox.check_view_isometric_elastic.Active = false;
		}

		PWBox.preferences = preferences;

		Pixbuf pixbuf;


		PWBox.image_button_close.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "image_close.png");

		//main, person tabs

		pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "image_rest.png");
		PWBox.image_rest.Pixbuf = pixbuf;

		//to avoid changing the sqlite and gui undecorated mode when activating maximized
		PWBox.signalsNoFollow = true;

		if(preferences.maximized == Preferences.MaximizedTypes.NO)
		{
			PWBox.check_appearance_maximized.Active = false;
			PWBox.alignment_undecorated.Visible = false;
//			PWBox.label_recommended_undecorated.Visible = false;
		}
		else {
			PWBox.check_appearance_maximized.Active = true;
			if (UtilAll.IsWindows ())
				PWBox.alignment_undecorated.Visible = false;
			else
				PWBox.alignment_undecorated.Visible = true;
//			PWBox.label_recommended_undecorated.Visible = true;
			PWBox.check_appearance_maximized_undecorated.Active =
				(preferences.maximized == Preferences.MaximizedTypes.YESUNDECORATED);
		}

		PWBox.signalsNoFollow = false;

		if(preferences.personWinHide)
			PWBox.check_appearance_person_win_hide.Active = true;
		else
			PWBox.check_appearance_person_win_hide.Active = false;

		PWBox.check_appearance_person_photo.Sensitive = ! preferences.personWinHide;

		if(preferences.personClubID)
			PWBox.check_appearance_person_clubID.Active = true;
		else
			PWBox.check_appearance_person_clubID.Active = false;

		if(preferences.personPhoto)
			PWBox.check_appearance_person_photo.Active = true;
		else
			PWBox.check_appearance_person_photo.Active = false;

		if (preferences.fontSizeAtGui < 0)
		{
			PWBox.box_font_size_custom.Visible = false;
			PWBox.radio_font_size_default.Active = true;
		} else {
			PWBox.box_font_size_custom.Visible = true;
			PWBox.spin_font_size_custom.Value = preferences.fontSizeAtGui;
			PWBox.radio_font_size_custom.Active = true;
		}

		if(preferences.logoAnimatedShow)
			PWBox.check_logo_animated.Active = true;
		else
			PWBox.check_logo_animated.Active = false;

		PWBox.hbox_last_session_and_mode.Visible = ! compujump;

		if(preferences.loadLastSessionAtStart)
			PWBox.check_session_autoload_at_start.Active = true;
		else
			PWBox.check_session_autoload_at_start.Active = false;

		if(preferences.loadLastModeAtStart)
			PWBox.check_mode_autoload_at_start.Active = true;
		else
			PWBox.check_mode_autoload_at_start.Active = false;

		PWBox.signalsNoFollow = true;
		if(preferences.fontType == Preferences.FontTypes.Courier)
			PWBox.radio_font_courier.Active = true;
		else if (preferences.fontType == Preferences.FontTypes.Helvetica)
			PWBox.radio_font_helvetica.Active = true;
		else //if(preferences.fontType == Preferences.FontTypes.Noto_Sans_CJ_SC)
			PWBox.radio_font_noto_sans_cjk_sc.Active = true;
		PWBox.signalsNoFollow = false;

		PWBox.check_rest_time.Active = (preferences.restTimeMinutes >= 0);
		PWBox.on_check_rest_time_toggled (new object (), new EventArgs ());

		if(preferences.restTimeMinutes >= 0)
		{
			PWBox.spinbutton_rest_minutes.Value = preferences.restTimeMinutes;
			PWBox.spinbutton_rest_seconds.Value = preferences.restTimeSeconds;
		} else { //min == -1 means no restTime
			PWBox.spinbutton_rest_minutes.Value = 2;
			PWBox.spinbutton_rest_seconds.Value = 0;
		}



		if(preferences.showPower)
			PWBox.checkbutton_power.Active = true; 
		else
			PWBox.checkbutton_power.Active = false; 
		
		if(preferences.showStiffness)
			PWBox.checkbutton_stiffness.Active = true; 
		else
			PWBox.checkbutton_stiffness.Active = false; 
		
		if(preferences.showInitialSpeed)  
			PWBox.checkbutton_initial_speed.Active = true; 
		else 
			PWBox.checkbutton_initial_speed.Active = false; 

		if(preferences.showJumpRSI)
			PWBox.checkbutton_jump_rsi.Active = true;
		else
			PWBox.checkbutton_jump_rsi.Active = false;

		/*
		if(preferences.showAngle)  
			PWBox.checkbutton_angle.Active = true; 
		else 
			PWBox.checkbutton_angle.Active = false; 
		*/

		if(preferences.showQIndex || preferences.showDjIndex) { 
			PWBox.checkbutton_show_tv_tc_index.Active = true; 
			if(preferences.showQIndex) {
				PWBox.radiobutton_show_q_index.Active = true; 
			} else {
				PWBox.radiobutton_show_dj_index.Active = true; 
			}
			PWBox.hbox_indexes.Show();
		}
		else {
			PWBox.checkbutton_show_tv_tc_index.Active = false; 
			PWBox.hbox_indexes.Hide();
		}

		if(preferences.weightStatsPercent)  
			PWBox.radio_weight_percent.Active = true; 
		else 
			PWBox.radio_weight_kg.Active = true; 

		if(preferences.metersSecondsPreferred)  
			PWBox.radio_speed_ms.Active = true; 
		else 
			PWBox.radio_speed_km.Active = true; 


		if(preferences.runSpeedStartArrival)  
			PWBox.radio_runs_speed_start_arrival.Active = true; 
		else 
			PWBox.radio_runs_speed_start_leaving.Active = true; 


		//start of double contacts stuff ----

		pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "image_run_simple.png");
		PWBox.image_races_simple.Pixbuf = pixbuf;
		pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "image_run_multiple.png");
		PWBox.image_races_intervallic.Pixbuf = pixbuf;

		pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "image_info.png");
		PWBox.image_jumps_power_help.Pixbuf = pixbuf;
		PWBox.image_jumps_stiffness_help.Pixbuf = pixbuf;
		PWBox.image_run_speed_start_help.Pixbuf = pixbuf;
		PWBox.image_encoder_inactivity_help.Pixbuf = pixbuf;
		PWBox.image_encoder_capture_cut_by_triggers_help.Pixbuf = pixbuf;
		PWBox.image_encoder_inertial_analyze_eq_mass_help.Pixbuf = pixbuf;

		if (menu_mode == Constants.Modes.RUNSSIMPLE || menu_mode == Constants.Modes.RUNSINTERVALLIC)
		{
			PWBox.notebook_races.CurrentPage = 0;

			if(menu_mode ==	Constants.Modes.RUNSSIMPLE)
				PWBox.notebook_races_double_contacts.CurrentPage = 0;
			else if(menu_mode == Constants.Modes.RUNSINTERVALLIC)
				PWBox.notebook_races_double_contacts.CurrentPage = 1;
		} else if (menu_mode == Constants.Modes.RUNSENCODER)
			PWBox.notebook_races.CurrentPage = 1;

		PWBox.checkbutton_runs_prevent_double_contact.Active = 
			(preferences.runDoubleContactsMode != Constants.DoubleContact.NONE);
		PWBox.checkbutton_runs_i_prevent_double_contact.Active = 
			(preferences.runIDoubleContactsMode != Constants.DoubleContact.NONE);

		PWBox.spinbutton_runs_prevent_double_contact.Value = 
			preferences.runDoubleContactsMS;
		PWBox.spinbutton_runs_i_prevent_double_contact.Value = 
			preferences.runIDoubleContactsMS;

		//---- end of double contacts stuff		


		if(preferences.CSVExportDecimalSeparator == "COMMA")
			PWBox.radio_export_latin.Active = true; 
		else
			PWBox.radio_export_non_latin.Active = true; 

	
		//encoder capture -->

		PWBox.spin_encoder_capture_time.Value = preferences.encoderCaptureTime;

		if(preferences.encoderCaptureInactivityEndTime < 0) {
			PWBox.check_encoder_capture_inactivity_end_time.Active = false;
			PWBox.hbox_encoder_capture_inactivity_time.Sensitive = false;
			PWBox.spin_encoder_capture_inactivity_end_time.Value = 3;
		} else {
			PWBox.check_encoder_capture_inactivity_end_time.Active = true;
			PWBox.hbox_encoder_capture_inactivity_time.Sensitive = true;
			PWBox.spin_encoder_capture_inactivity_end_time.Value = preferences.encoderCaptureInactivityEndTime;
		}

		PWBox.createComboEncoderCaptureCurvesSave ();

		pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "image_weight.png");
		PWBox.image_encoder_gravitatory.Pixbuf = pixbuf;
		pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "image_inertia.png");
		PWBox.image_encoder_inertial.Pixbuf = pixbuf;
		PWBox.image_encoder_inertial2.Pixbuf = pixbuf;
		pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "image_encoder_triggers_no.png");
		PWBox.image_encoder_triggers.Pixbuf = pixbuf;

		if(preferences.encoderCaptureInertialDiscardFirstN > 0) {
			PWBox.checkbutton_encoder_capture_inertial_discard_first_n.Active = true;
			PWBox.spin_encoder_capture_inertial_discard_first_n.Value = preferences.encoderCaptureInertialDiscardFirstN;
			PWBox.box_encoder_capture_inertial_discard_first_n.Visible = true;
		} else {
			PWBox.checkbutton_encoder_capture_inertial_discard_first_n.Active = false;
			PWBox.spin_encoder_capture_inertial_discard_first_n.Value = 3;
			PWBox.box_encoder_capture_inertial_discard_first_n.Visible = false;
		}

		if(preferences.encoderCaptureShowNRepetitions < 0) {
			PWBox.radio_encoder_capture_show_all_bars.Active = true;
			PWBox.spin_encoder_capture_show_only_some_bars.Value = 10;
		} else {
			PWBox.radio_encoder_capture_show_only_some_bars.Active = true;
			PWBox.spin_encoder_capture_show_only_some_bars.Value = preferences.encoderCaptureShowNRepetitions;
		}


		PWBox.spin_encoder_capture_barplot_font_size.Value = preferences.encoderCaptureBarplotFontSize;
		PWBox.check_show_start_and_duration.Active = preferences.encoderShowStartAndDuration;

		if(preferences.encoderCaptureCutByTriggers == Preferences.TriggerTypes.NO_TRIGGERS)
			PWBox.radio_encoder_triggers_no.Active = true;
		else {
			PWBox.radio_encoder_triggers_yes.Active = true;
			if(preferences.encoderCaptureCutByTriggers == Preferences.TriggerTypes.START_AT_CAPTURE)
				PWBox.radio_encoder_triggers_yes_start_at_capture.Active = true;
			else
				PWBox.radio_encoder_triggers_yes_start_at_first_trigger.Active = true;
		}

		if(preferences.encoderCaptureInfinite)
			PWBox.check_encoder_capture_infinite.Active = true;
		else
			PWBox.check_encoder_capture_infinite.Active = false;

		pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "cont.png");
		PWBox.image_encoder_capture_infinite.Pixbuf = pixbuf;

		if(preferences.encoderRepetitionCriteriaGravitatory == Preferences.EncoderRepetitionCriteria.ECC_CON)
			PWBox.radio_encoder_rep_criteria_gravitatory_ecc_con.Active = true;
		else if(preferences.encoderRepetitionCriteriaGravitatory == Preferences.EncoderRepetitionCriteria.ECC)
			PWBox.radio_encoder_rep_criteria_gravitatory_ecc.Active = true;
		else // if(preferences.encoderRepetitionCriteriaGravitatory == Preferences.EncoderRepetitionCriteria.CON)
			PWBox.radio_encoder_rep_criteria_gravitatory_con.Active = true;

		if(preferences.encoderRepetitionCriteriaInertial == Preferences.EncoderRepetitionCriteria.ECC_CON)
			PWBox.radio_encoder_rep_criteria_inertial_ecc_con.Active = true;
		else if(preferences.encoderRepetitionCriteriaInertial == Preferences.EncoderRepetitionCriteria.ECC)
			PWBox.radio_encoder_rep_criteria_inertial_ecc.Active = true;
		else // if(preferences.encoderRepetitionCriteriaInertial == Preferences.EncoderRepetitionCriteria.CON)
			PWBox.radio_encoder_rep_criteria_inertial_con.Active = true;

		//encoder other -->
		PWBox.checkbutton_encoder_propulsive.Active = preferences.encoderPropulsive;

		if(preferences.encoderWorkKcal)
			PWBox.radio_encoder_work_kcal.Active = true;
		else
			PWBox.radio_encoder_work_joules.Active = true;

		if(preferences.encoderInertialGraphsX == Preferences.EncoderInertialGraphsXTypes.INERTIA_MOMENT)
			PWBox.radio_encoder_inertial_analyze_inertia_moment.Active = true;
		else if(preferences.encoderInertialGraphsX == Preferences.EncoderInertialGraphsXTypes.DIAMETER)
			PWBox.radio_encoder_inertial_analyze_diameter.Active = true;
		else
			PWBox.radio_encoder_inertial_analyze_equivalent_mass.Active = true;

		PWBox.spin_encoder_smooth_con.Value = preferences.encoderSmoothCon;

		if(preferences.encoder1RMMethod == Constants.Encoder1RMMethod.NONWEIGHTED)
			PWBox.radio_encoder_1RM_nonweighted.Active = true;
		else if(preferences.encoder1RMMethod == Constants.Encoder1RMMethod.WEIGHTED)
			PWBox.radio_encoder_1RM_weighted.Active = true;
		else if(preferences.encoder1RMMethod == Constants.Encoder1RMMethod.WEIGHTED2)
			PWBox.radio_encoder_1RM_weighted2.Active = true;
		else //(preferences.encoder1RMMethod == Constants.Encoder1RMMethod.WEIGHTED3)
			PWBox.radio_encoder_1RM_weighted3.Active = true;

		//done here and not in glade to be shown with the decimal point of user language	
		PWBox.label_encoder_con.Text = (0.7).ToString();

		//forceSensor -->
		PWBox.signalsNoFollow = true;

		//	butterworth isometric
		PWBox.check_force_sensor_isometric_butterworth.Active = preferences.forceSensorIsometricButterworth >= 0;
		PWBox.box_force_sensor_isometric_butterworth_values.Sensitive = preferences.forceSensorIsometricButterworth >= 0;
		if (preferences.forceSensorIsometricButterworth < 0)
			PWBox.spin_force_sensor_isometric_butterworth.Value = 15;
		else
			PWBox.spin_force_sensor_isometric_butterworth.Value = preferences.forceSensorIsometricButterworth;

		//	butterworth elastic
		PWBox.check_force_sensor_elastic_butterworth.Active = preferences.forceSensorElasticButterworth >= 0;
		PWBox.box_force_sensor_elastic_butterworth_values.Sensitive = preferences.forceSensorElasticButterworth >= 0;
		if (preferences.forceSensorElasticButterworth < 0)
			PWBox.spin_force_sensor_elastic_butterworth.Value = 3;
		else
			PWBox.spin_force_sensor_elastic_butterworth.Value = preferences.forceSensorElasticButterworth;

		PWBox.signalsNoFollow = false;


		PWBox.spin_force_sensor_capture_width_graph_seconds.Value = preferences.forceSensorCaptureWidthSeconds;

		if(preferences.forceSensorCaptureScroll)
			PWBox.radio_force_sensor_capture_scroll.Active = true;
		else
			PWBox.radio_force_sensor_capture_zoom_out.Active = true;

		PWBox.spin_force_sensor_elastic_ecc_min_displ.Value = preferences.forceSensorElasticEccMinDispl;
		PWBox.spin_force_sensor_elastic_con_min_displ.Value = preferences.forceSensorElasticConMinDispl;
		PWBox.spin_force_sensor_not_elastic_ecc_min_force.Value = preferences.forceSensorNotElasticEccMinForce;
		PWBox.spin_force_sensor_not_elastic_con_min_force.Value = preferences.forceSensorNotElasticConMinForce;

		PWBox.spin_force_sensor_graphs_line_width.Value = preferences.forceSensorGraphsLineWidth;

		if(preferences.forceSensorVariabilityMethod == Preferences.VariabilityMethodEnum.RMSSD)
		{
			PWBox.radio_force_sensor_variability_rmssd.Active = true;
			PWBox.hbox_force_sensor_lag.Visible = true;
		} else if(preferences.forceSensorVariabilityMethod == Preferences.VariabilityMethodEnum.CVRMSSD)
		{
			PWBox.radio_force_sensor_variability_cvrmssd.Active = true;
			PWBox.hbox_force_sensor_lag.Visible = true;
		} else if(preferences.forceSensorVariabilityMethod == Preferences.VariabilityMethodEnum.CV)
		{
			PWBox.radio_force_sensor_variability_cv.Active = true;
			PWBox.hbox_force_sensor_lag.Visible = false;
		} else {
			PWBox.radio_force_sensor_variability_old.Active = true;
			PWBox.hbox_force_sensor_lag.Visible = false;
		}

		PWBox.spin_force_sensor_variability_lag.Value = preferences.forceSensorVariabilityLag;
		PWBox.spin_force_sensor_analyze_best_stability_in_window.Value = preferences.forceSensorAnalyzeBestStabilityInWindow;
		PWBox.spin_force_sensor_analyze_max_avg_force_in_window.Value = preferences.forceSensorAnalyzeMaxAVGInWindow;

		//runEncoder -->
		PWBox.spin_run_encoder_acceleration.Value = preferences.runEncoderMinAccel;
		PWBox.spin_run_encoder_pps.Value = preferences.runEncoderPPS;
		PWBox.update_run_encoder_gui_pps_equivalence_and_max ();

		if(preferences.useHeightsOnJumpIndexes)
			PWBox.radio_use_heights_on_jump_indexes.Active = true;
		else
			PWBox.radio_do_not_use_heights_on_jump_indexes.Active = true;

		if(preferences.importerPythonVersion == Preferences.pythonVersionEnum.Python2)
			PWBox.radio_python_2.Active = true;
		else //if(preferences.importerPythonVersion == Preferences.pythonVersionEnum.Python3)
			PWBox.radio_python_3.Active = true;

		PWBox.colorChoosedLastDefined = false;
		if(preferences.colorBackgroundOsColor) {
			PWBox.radio_color_os.Active = true;
			PWBox.button_color_choose.Sensitive = false;

			//do not show the visible tag at open the window, only when user changes to this option.
			PWBox.label_radio_color_os_needs_restart.Visible = false;
		}
		else if((preferences.colorBackgroundString).ToLower() == "#0e1e46") {
			PWBox.radio_color_chronojump_blue.Active = true;
			PWBox.button_color_choose.Sensitive = false;
		}
		else {
			PWBox.colorChoosedLast = preferences.colorBackground;
			PWBox.colorChoosedLastDefined = true;

			PWBox.radio_color_custom.Active = true;
			PWBox.button_color_choose.Sensitive = true;
		}

		PWBox.colorBackground = UtilGtk.ColorParse(preferences.colorBackgroundString);
		PWBox.paintColorChronojump ();
		PWBox.paintDrawingArea (PWBox.colorBackground);
		PWBox.paintBg (PWBox.colorBackground);


		//tabs selection widgets
		PWBox.image_view_more_tabs_close.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "image_close.png");
		PWBox.label_mandatory_tabs.Text = "<b>" + PWBox.label_mandatory_tabs.Text + "</b>";
		PWBox.label_mandatory_tabs.UseMarkup = true;
		PWBox.label_selectable_tabs.Text = "<b>" + PWBox.label_selectable_tabs.Text + "</b>";
		PWBox.label_selectable_tabs.UseMarkup = true;

		//help
		PWBox.image_help_close.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "image_close.png");

		showTabMultimedia (preferences, compujump);
		showTabLanguage (preferences);
		showTabAdvanced (preferences);

		PWBox.preferences_win.Show ();
		return PWBox;
	}

	private static void showTabMultimedia (Preferences preferences, bool compujump)
	{
		if(preferences.volumeOn) {
			PWBox.checkbutton_volume.Active = true;
			PWBox.alignment_multimedia_sounds.Visible = true;
		} else {
			PWBox.checkbutton_volume.Active = false;
			PWBox.alignment_multimedia_sounds.Visible = false;
		}

		//hide video for compujump
		if(compujump)
			PWBox.notebook_multimedia.GetNthPage(1).Hide();

		PWBox.label_camera_error.Visible = false;

		PWBox.label_webcam_windows.Visible =
			(PWBox.operatingSystem == UtilAll.OperatingSystems.WINDOWS);

		PWBox.hbox_not_recommended_when_not_on_windows.Visible =
			! (PWBox.operatingSystem == UtilAll.OperatingSystems.WINDOWS);

		if(PWBox.operatingSystem == UtilAll.OperatingSystems.WINDOWS ||
				PWBox.operatingSystem == UtilAll.OperatingSystems.MACOSX)
		{
			if(preferences.gstreamer == Preferences.GstreamerTypes.FFPLAY)
				PWBox.radio_ffplay.Active = true;
			else //(preferences.gstreamer == Preferences.GstreamerTypes.SYSTEMSOUNDS)
				PWBox.radio_sound_systemsounds.Active = true;

			PWBox.radio_gstreamer_0_1.Visible = false;
			PWBox.radio_gstreamer_1_0.Visible = false;
		}
		else //LINUX
		{
			if(preferences.gstreamer == Preferences.GstreamerTypes.GST_0_1)
				PWBox.radio_gstreamer_0_1.Active = true;
			else if(preferences.gstreamer == Preferences.GstreamerTypes.GST_1_0)
				PWBox.radio_gstreamer_1_0.Active = true;
			else if(preferences.gstreamer == Preferences.GstreamerTypes.FFPLAY)
				PWBox.radio_ffplay.Active = true;
			else //(preferences.gstreamer == Preferences.GstreamerTypes.SYSTEMSOUNDS)
				PWBox.radio_sound_systemsounds.Active = true;
		}

		PWBox.label_test_sound_result.Text = "";

		PWBox.notebook_multimedia_video.CurrentPage = 0; //show only check_devices button

		Pixbuf pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "audio.png");
		PWBox.image_multimedia_audio.Pixbuf = pixbuf;
		pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "videocamera_on.png");
		PWBox.image_multimedia_video.Pixbuf = pixbuf;
		pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "image_photo_preview.png");
		PWBox.image_video_preview.Pixbuf = pixbuf;

		PWBox.spin_camera_stop_after.Value = preferences.videoStopAfter;
		//PWBox.vbox_camera_stop_after.Visible = (preferences.videoStopAfter > 0);
		PWBox.hbox_camera_stop_after_seconds.Visible = (preferences.videoStopAfter > 0);
		PWBox.check_camera_stop_after.Active = (preferences.videoStopAfter > 0);
	}

	private static void showTabLanguage (Preferences preferences)
	{
		PWBox.createComboLanguage();

		PWBox.signalsNoFollow = true;
		if(preferences.language == "")
			PWBox.radio_language_detected.Active = true;
		else
			PWBox.radio_language_force.Active = true;

		if(preferences.RGraphsTranslate)
			PWBox.radio_graphs_translate.Active = true;
		else
			PWBox.radio_graphs_no_translate.Active = true;
		PWBox.signalsNoFollow = false;
	}

	private static void showTabAdvanced (Preferences preferences)
	{
		// sub tab: cloud ---->

		PWBox.label_radio_cloud_no.Text = "<b>" + PWBox.label_radio_cloud_no.Text + "</b>";
		PWBox.label_radio_cloud_no_recommended.Text = "<b>" + PWBox.label_radio_cloud_no_recommended.Text + "</b>";
		PWBox.label_radio_cloud_capture.Text = "<b>" + PWBox.label_radio_cloud_capture.Text + "</b>";
		PWBox.label_radio_cloud_view.Text = "<b>" + PWBox.label_radio_cloud_view.Text + "</b>";
		PWBox.label_radio_cloud_no.UseMarkup = true;
		PWBox.label_radio_cloud_no_recommended.UseMarkup = true;
		PWBox.label_radio_cloud_capture.UseMarkup = true;
		PWBox.label_radio_cloud_view.UseMarkup = true;

		PWBox.image_advanced_cloud.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "cloud_blue.png");
		PWBox.image_advanced_logs.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "log.png");
		PWBox.image_advanced_more.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "image_more_horiz.png");
		PWBox.image_cloud_capture.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "cloud_upload_blue.png");
		PWBox.image_cloud_view.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "cloud_view_blue.png");
		PWBox.image_cloud_schema.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "cloud_schema_small.png");
		PWBox.image_button_send_log.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "send_blue.png");
		PWBox.image_advanced_r.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "language-r.png");
		PWBox.image_advanced_python.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "language-python.png");
		PWBox.image_advanced_bluetooth.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "bluetooth.png");

		PWBox.entry_bluetooth_url.Text = BluetoothLE.GetScriptURL ();
		PWBox.entry_bluetooth_url.Sensitive = true;
		PWBox.button_bluetooth_start.Sensitive = true;
		PWBox.button_bluetooth_end.Sensitive = false;

		PWBox.signalsNoFollow = true;
		if (PWBox.configAtPrefs.CopyToCloudFullPath !=  "")
		{
			PWBox.radio_cloud_capture.Active = true;
			PWBox.label_cloud_capture_path.Text = PWBox.configAtPrefs.CopyToCloudFullPath;
			PWBox.label_cloud_capture_path.TooltipText = PWBox.configAtPrefs.CopyToCloudFullPath;
		} else if (PWBox.configAtPrefs.ReadFromCloudMainPath !=  "")
		{
			PWBox.radio_cloud_view.Active = true;
			PWBox.label_cloud_view_path.Text = PWBox.configAtPrefs.ReadFromCloudMainPath;
			PWBox.label_cloud_view_path.TooltipText = PWBox.configAtPrefs.ReadFromCloudMainPath;
		} else
			PWBox.radio_cloud_no.Active = true;
		PWBox.signalsNoFollow = false;

		PWBox.buttons_cloud_sensitive ();

		// sub tab: logs ---->
		PWBox.emailStoredForSendLog = SqlitePreferences.Select("email");
		if(PWBox.emailStoredForSendLog != null && PWBox.emailStoredForSendLog != "" && PWBox.emailStoredForSendLog != "0")
			PWBox.entry_send_log.Text = PWBox.emailStoredForSendLog;

		// sub tab: more ---->

		PWBox.signalsNoFollow = true;

		LogB.Information ("Config.RUserURLStatic = " + Config.RUserURLStatic.ToString ());
		LogB.Information ("Config.RscriptUserURLStatic = " + Config.RscriptUserURLStatic.ToString ());
		LogB.Information ("Config.PythonUserURLStatic = " + Config.PythonUserURLStatic.ToString ());

		if (PWBox.operatingSystem == UtilAll.OperatingSystems.WINDOWS)
		{
			PWBox.button_r_autodetect.Visible = false;
			PWBox.button_rscript_autodetect.Visible = false;
			PWBox.button_rscript_usr_local_bin.Visible = false;
			PWBox.button_python_autodetect.Visible = false;
		}
		else if (PWBox.operatingSystem == UtilAll.OperatingSystems.MACOSX)
		{
			PWBox.button_r_choose.Visible = false;
			PWBox.button_rscript_choose.Visible = false;
			PWBox.button_rscript_usr_local_bin.Visible = true;
			PWBox.button_python_choose.Visible = false;
		}

		if (Config.RUserURLStatic == "") {
			PWBox.radio_r_default.Active = true;
			PWBox.button_r_choose.Sensitive = false;
			PWBox.button_r_autodetect.Sensitive = false;
			PWBox.entry_r_user_location.Sensitive = false;
			PWBox.entry_r_user_location.Text = "";
		} else {
			PWBox.radio_r_other.Active = true;
			PWBox.button_r_choose.Sensitive = true;
			PWBox.button_r_autodetect.Sensitive = true;
			PWBox.entry_r_user_location.Sensitive = true;
			PWBox.entry_r_user_location.Text = Config.RUserURLStatic;
		}

		if (Config.RscriptUserURLStatic == "") {
			PWBox.radio_rscript_default.Active = true;
			PWBox.button_rscript_choose.Sensitive = false;
			PWBox.button_rscript_autodetect.Sensitive = false;
			PWBox.button_rscript_usr_local_bin.Sensitive = false;
			PWBox.entry_rscript_user_location.Sensitive = false;
			PWBox.entry_rscript_user_location.Text = "";
		} else {
			PWBox.radio_rscript_other.Active = true;
			PWBox.button_rscript_choose.Sensitive = true;
			PWBox.button_rscript_autodetect.Sensitive = true;
			PWBox.button_rscript_usr_local_bin.Sensitive = true;
			PWBox.entry_rscript_user_location.Sensitive = true;
			PWBox.entry_rscript_user_location.Text = Config.RscriptUserURLStatic;
		}

		if (Config.PythonUserURLStatic == "") {
			PWBox.radio_python_default.Active = true;
			PWBox.button_python_choose.Sensitive = false;
			PWBox.button_python_autodetect.Sensitive = false;
			PWBox.entry_python_user_location.Sensitive = false;
			PWBox.entry_python_user_location.Text = "";
		} else {
			PWBox.radio_python_other.Active = true;
			PWBox.button_python_choose.Sensitive = true;
			PWBox.button_python_autodetect.Sensitive = true;
			PWBox.entry_python_user_location.Sensitive = true;
			PWBox.entry_python_user_location.Text = Config.PythonUserURLStatic;
		}
		PWBox.signalsNoFollow = false;

		PWBox.label_database_id.Text = preferences.machineID;
		PWBox.entry_database_name.Text = preferences.machineName;
		if (PWBox.configAtPrefs.ReadFromCloudMainPath != "") // disable database_name change on cloud_view
			PWBox.entry_database_name.Sensitive = false;

		if(preferences.askDeletion)
			PWBox.checkbutton_ask_deletion.Active = true;
		else
			PWBox.checkbutton_ask_deletion.Active = false;

		PWBox.createComboDecimals ();

		if(preferences.muteLogs)
			PWBox.checkbutton_mute_logs.Active = true;
		else
			PWBox.checkbutton_mute_logs.Active = false;

		PWBox.button_debug_mode.Sensitive = ! preferences.debugMode;
	}


	/* callbacks SQL change at any change for tab: main */

	private void on_radio_color_custom_toggled (object o, EventArgs args)
	{
		// A) changes on preferences gui
		button_color_choose.Sensitive = true;
		label_radio_color_os_needs_restart.Visible = false;

		if (colorChoosedLastDefined)
		{
			colorBackground = colorChoosedLast;
			Config.SetColors (colorBackground);
		}

		// B) changes on preferences object and SqlitePreferences
		preferences.colorBackgroundString = Preferences.PreferencesChange(
				false,
				SqlitePreferences.ColorBackground, preferences.colorBackgroundString,
				UtilGtk.ColorToHex (colorBackground)); //this does the reverse of Gdk.Color.Parse on UtilGtk.ColorParse()
		preferences.colorBackgroundOsColor = Preferences.PreferencesChange(
				false,
				SqlitePreferences.ColorBackgroundOsColor, preferences.colorBackgroundOsColor,
				false);

		Config.SetColors (colorBackground);
		paintBg (colorBackground);
	}
	private void on_radio_color_chronojump_blue_toggled (object o, EventArgs args)
	{
		// A) changes on preferences gui
		button_color_choose.Sensitive = false;
		label_radio_color_os_needs_restart.Visible = false;

		// B) changes on preferences object and SqlitePreferences
		preferences.colorBackgroundString = Preferences.PreferencesChange(
				false,
				SqlitePreferences.ColorBackground, preferences.colorBackgroundString,
				"#0e1e46");
		preferences.colorBackgroundOsColor = Preferences.PreferencesChange(
				false,
				SqlitePreferences.ColorBackgroundOsColor, preferences.colorBackgroundOsColor,
				false);

		colorBackground = UtilGtk.ColorParse (preferences.colorBackgroundString);
		Config.SetColors (preferences.colorBackground);
		paintBg (colorBackground);
	}
	private void on_radio_color_os_toggled (object o, EventArgs args)
	{
		// A) changes on preferences gui
		button_color_choose.Sensitive = false;
		label_radio_color_os_needs_restart.Visible = true;

		// B) changes on preferences object and SqlitePreferences
		//radio_color_os does not change the colorBackgroundString, it changes the Config.UseSystemColor
		//but note that on showing cairo and execute graphs, primary color will be colorBackground
		preferences.colorBackgroundOsColor = Preferences.PreferencesChange(
				false,
				SqlitePreferences.ColorBackgroundOsColor, preferences.colorBackgroundOsColor,
				true);

		Config.SetColors (preferences.colorBackground);
		paintBg (colorBackground);
	}

	RGBA colorChoosedLast; //to have stored color chosen color from click to color chosen, chronojump, color chosen
	bool colorChoosedLastDefined;
	private void on_button_color_choose_clicked(object o, EventArgs args)
	{
		using (ColorChooserDialog colorChooserDialog = new ColorChooserDialog (Catalog.GetString("Select color"), preferences_win))
		{
			colorChooserDialog.Rgba = colorBackground;

			if (colorChooserDialog.Run () == (int) ResponseType.Ok)
			{
				// A) changes on preferences gui
				colorBackground = colorChooserDialog.Rgba;

				// B) changes on preferences object and SqlitePreferences
				preferences.colorBackgroundString = Preferences.PreferencesChange(
						false,
						SqlitePreferences.ColorBackground, preferences.colorBackgroundString,
						UtilGtk.ColorToHex (colorBackground)); //this does the reverse of Gdk.Color.Parse on UtilGtk.ColorParse()
				preferences.colorBackgroundOsColor = Preferences.PreferencesChange(
						false,
						SqlitePreferences.ColorBackgroundOsColor, preferences.colorBackgroundOsColor,
						false);

				colorChoosedLast = colorBackground;
				colorChoosedLastDefined = true;

				Config.SetColors (colorBackground);
				paintDrawingArea (colorBackground);
				paintBg (colorBackground);
			}

			colorChooserDialog.Hide ();
		}
	}

	private void on_check_session_autoload_at_start_toggled (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		preferences.loadLastSessionAtStart = Preferences.PreferencesChange (
				false, SqlitePreferences.LoadLastSessionAtStart, preferences.loadLastSessionAtStart,
				PWBox.check_session_autoload_at_start.Active);

	}

	private void on_check_mode_autoload_at_start_toggled (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		preferences.loadLastModeAtStart = Preferences.PreferencesChange (
				false, SqlitePreferences.LoadLastModeAtStart, preferences.loadLastModeAtStart,
				PWBox.check_mode_autoload_at_start.Active);
	}

	private void on_check_logo_animated_toggled (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		preferences.logoAnimatedShow = Preferences.PreferencesChange(
				false, SqlitePreferences.LogoAnimatedShow, preferences.logoAnimatedShow,
				PWBox.check_logo_animated.Active);
	}

	private void on_check_rest_time_toggled (object o, EventArgs args)
	{
		// A) changes on preferences gui
		Pixbuf pixbuf;
		if(check_rest_time.Active)
		{
			hbox_rest_time_values.Visible = true;
			pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "image_rest.png");
		} else
		{
			hbox_rest_time_values.Visible = false;
			pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "image_rest_inactive.png");
		}
		PWBox.image_rest.Pixbuf = pixbuf;

		// B) changes on preferences object and SqlitePreferences
		changeRestTimeOnPreferencesAndDB ();
	}

	private void on_spinbutton_rest_minutes_value_changed (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		changeRestTimeOnPreferencesAndDB ();
	}
	private void on_spinbutton_rest_seconds_value_changed (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		changeRestTimeOnPreferencesAndDB ();
	}

	private void changeRestTimeOnPreferencesAndDB ()
	{
		bool changeRestTime = false;
		int minutes = (int) PWBox.spinbutton_rest_minutes.Value;
		int seconds = (int) PWBox.spinbutton_rest_seconds.Value;

		//if we had some time selected previously and now we selected no rest time
		if(preferences.restTimeMinutes >= 0 && ! PWBox.check_rest_time.Active)
		{
			changeRestTime = true;
			minutes = -1;
			seconds = 0;
		} else
		{
			if(preferences.restTimeMinutes != minutes)
				changeRestTime = true;
			if(preferences.restTimeSeconds != seconds)
				changeRestTime = true;
		}

		if(changeRestTime)
		{
			SqlitePreferences.Update (SqlitePreferences.RestTimeMinutes, minutes.ToString(), false);
			preferences.restTimeMinutes = minutes;
			SqlitePreferences.Update (SqlitePreferences.RestTimeSeconds, seconds.ToString(), false);
			preferences.restTimeSeconds = seconds;
		}
	}

	/* callbacks SQL change at any change for tab: screen */

	private void on_check_appearance_maximized_toggled (object obj, EventArgs args)
	{
		if (signalsNoFollow)
			return;

		// A) changes on preferences gui
		alignment_undecorated.Visible = ! UtilAll.IsWindows () && check_appearance_maximized.Active;
//		label_recommended_undecorated.Visible = check_appearance_maximized.Active;

		// B) changes on preferences object and SqlitePreferences
		Preferences.MaximizedTypes maximizedTypeFromGUI = get_maximized_from_gui();
		if(preferences.maximized != maximizedTypeFromGUI)
		{
			SqlitePreferences.Update ("maximized", maximizedTypeFromGUI.ToString(), false);
			preferences.maximized = maximizedTypeFromGUI;
			FakeButtonMaximizeChanges.Click ();
		}
	}

	private void on_check_appearance_maximized_undecorated_toggled (object obj, EventArgs args)
	{
		if (signalsNoFollow)
			return;

		// B) changes on preferences object and SqlitePreferences
		Preferences.MaximizedTypes maximizedTypeFromGUI = get_maximized_from_gui();
		if(preferences.maximized != maximizedTypeFromGUI)
		{
			SqlitePreferences.Update ("maximized", maximizedTypeFromGUI.ToString(), false);
			preferences.maximized = maximizedTypeFromGUI;
			FakeButtonMaximizeChanges.Click ();
		}
	}

	private void on_check_appearance_person_win_hide_toggled (object obj, EventArgs args)
	{
		// A) changes on preferences gui
		check_appearance_person_photo.Sensitive = ! check_appearance_person_win_hide.Active;

		// B) changes on preferences object and SqlitePreferences
		if( preferences.personWinHide != PWBox.check_appearance_person_win_hide.Active ) {
			SqlitePreferences.Update("personWinHide", PWBox.check_appearance_person_win_hide.Active.ToString(), false);
			preferences.personWinHide = PWBox.check_appearance_person_win_hide.Active;
			FakeButtonPersonWin.Click ();
		}
	}

	private void on_check_appearance_person_clubID_toggled (object obj, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		if( preferences.personClubID != PWBox.check_appearance_person_clubID.Active ) {
			SqlitePreferences.Update("personClubID", PWBox.check_appearance_person_clubID.Active.ToString(), false);
			preferences.personClubID = PWBox.check_appearance_person_clubID.Active;
			FakeButtonPersonWin.Click ();
		}
	}

	private void on_check_appearance_person_photo_toggled (object obj, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		if( preferences.personPhoto != PWBox.check_appearance_person_photo.Active ) {
			SqlitePreferences.Update("personPhoto", PWBox.check_appearance_person_photo.Active.ToString(), false);
			preferences.personPhoto = PWBox.check_appearance_person_photo.Active;
			FakeButtonPersonWin.Click ();
		}
	}

	private void on_radio_font_size_default_toggled (object o, EventArgs args)
	{
		// A) changes on preferences gui
		box_font_size_custom.Visible = false;

		// B) changes on preferences object and SqlitePreferences
		if (preferences.fontSizeAtGui >= 0) {
			SqlitePreferences.Update("fontSizeAtGui", "-1", false); //saved as string
			preferences.fontSizeAtGui = (int) -1;
		}
	}
	private void on_radio_font_size_custom_toggled (object o, EventArgs args)
	{
		// A) changes on preferences gui
		box_font_size_custom.Visible = true;

		// B) changes on preferences object and SqlitePreferences
		if (preferences.fontSizeAtGui < 0) {
			SqlitePreferences.Update("fontSizeAtGui",
					PWBox.spin_font_size_custom.Value.ToString(), false); //saved as string
			preferences.fontSizeAtGui = (int) spin_font_size_custom.Value;
		}
	}

	private void on_spin_font_size_custom_value_changed (object o, EventArgs args)
	{
		if (preferences.fontSizeAtGui != (int) PWBox.spin_font_size_custom.Value)
			preferences.fontSizeAtGui = Preferences.PreferencesChange(
					false, "fontSizeAtGui",
					preferences.fontSizeAtGui,
					(int) PWBox.spin_font_size_custom.Value);
	}

	/* callbacks SQL change at any change for tab: jumps */

	private void on_checkbutton_power_toggled (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		if( preferences.showPower != PWBox.checkbutton_power.Active ) {
			SqlitePreferences.Update("showPower", PWBox.checkbutton_power.Active.ToString(), false);
			preferences.showPower = PWBox.checkbutton_power.Active;
		}
	}
	private void on_checkbutton_stiffness_toggled (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		if( preferences.showStiffness != PWBox.checkbutton_stiffness.Active ) {
			SqlitePreferences.Update("showStiffness", PWBox.checkbutton_stiffness.Active.ToString(), false);
			preferences.showStiffness = PWBox.checkbutton_stiffness.Active;
		}
	}
	private void on_checkbutton_initial_speed_toggled (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		if( preferences.showInitialSpeed != PWBox.checkbutton_initial_speed.Active ) {
			SqlitePreferences.Update("showInitialSpeed", PWBox.checkbutton_initial_speed.Active.ToString(), false);
			preferences.showInitialSpeed = PWBox.checkbutton_initial_speed.Active;
		}
	}
	private void on_checkbutton_jump_rsi_toggled (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		if( preferences.showJumpRSI != PWBox.checkbutton_jump_rsi.Active ) {
			SqlitePreferences.Update(SqlitePreferences.ShowJumpRSI, PWBox.checkbutton_jump_rsi.Active.ToString(), false);
			preferences.showJumpRSI = PWBox.checkbutton_jump_rsi.Active;
		}
	}

	private void on_checkbutton_show_tv_tc_index_clicked (object o, EventArgs args)
	{
		// A) changes on preferences gui
		if(checkbutton_show_tv_tc_index.Active)
			hbox_indexes.Show();
		else
			hbox_indexes.Hide();

		// B) changes on preferences object and SqlitePreferences
		changeQDJIndexOnPreferencesAndDB ();
	}
	private void on_radiobutton_show_q_index_toggled (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		changeQDJIndexOnPreferencesAndDB ();
	}
	private void on_radiobutton_show_dj_index_toggled (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		changeQDJIndexOnPreferencesAndDB ();
	}
	private void changeQDJIndexOnPreferencesAndDB ()
	{
		if(PWBox.checkbutton_show_tv_tc_index.Active) {
			preferences.showQIndex = Preferences.PreferencesChange(
					false, "showQIndex", preferences.showQIndex,
					PWBox.radiobutton_show_q_index.Active);
			preferences.showDjIndex = Preferences.PreferencesChange(
					false, "showDjIndex", preferences.showDjIndex,
					PWBox.radiobutton_show_dj_index.Active);
		} else {
			preferences.showQIndex = Preferences.PreferencesChange(
					false, "showQIndex", preferences.showQIndex, false);
			preferences.showDjIndex = Preferences.PreferencesChange(
					false, "showDjIndex", preferences.showDjIndex, false);
		}
	}

	private void on_radio_weight_percent_kg_toggled (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		if ( ((Gtk.RadioButton) o).Active)
			if( preferences.weightStatsPercent != PWBox.radio_weight_percent.Active ) {
				SqlitePreferences.Update("weightStatsPercent", PWBox.radio_weight_percent.Active.ToString(), false);
				preferences.weightStatsPercent = PWBox.radio_weight_percent.Active;
			}
	}
	private void on_radio_use_heights_or_not_on_jump_indexes_toggled (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		if ( ((Gtk.RadioButton) o).Active)
			if( preferences.useHeightsOnJumpIndexes != PWBox.radio_use_heights_on_jump_indexes.Active ) {
				SqlitePreferences.Update("useHeightsOnJumpIndexes",
						PWBox.radio_use_heights_on_jump_indexes.Active.ToString(), false);
				preferences.useHeightsOnJumpIndexes = PWBox.radio_use_heights_on_jump_indexes.Active;
			}
	}

	/* callbacks SQL change at any change for tab: races */

	private void on_radio_speed_ms_km_toggled (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		if ( ((Gtk.RadioButton) o).Active)
			if( preferences.metersSecondsPreferred != PWBox.radio_speed_ms.Active ) {
				SqlitePreferences.Update("metersSecondsPreferred", PWBox.radio_speed_ms.Active.ToString(), false);
				preferences.metersSecondsPreferred = PWBox.radio_speed_ms.Active;
			}
	}
	private void on_radio_runs_speed_start_arrival_leaving_toggled (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		if ( ((Gtk.RadioButton) o).Active)
			if( preferences.runSpeedStartArrival != PWBox.radio_runs_speed_start_arrival.Active ) {
				SqlitePreferences.Update("runSpeedStartArrival", PWBox.radio_runs_speed_start_arrival.Active.ToString(), false);
				preferences.runSpeedStartArrival = PWBox.radio_runs_speed_start_arrival.Active;
			}
	}

	private void on_checkbutton_runs_prevent_double_contact_toggled (object o, EventArgs args)
	{
		// A) changes on preferences gui
		vbox_runs_prevent_double_contact.Visible = checkbutton_runs_prevent_double_contact.Active;

		// B) changes on preferences object and SqlitePreferences
		changeRunSimpleDoubleContactOnPreferencesAndDB ();
	}

	private void on_checkbutton_runs_i_prevent_double_contact_toggled (object o, EventArgs args)
	{
		// A) changes on preferences gui
		vbox_runs_i_prevent_double_contact.Visible = checkbutton_runs_i_prevent_double_contact.Active;

		// B) changes on preferences object and SqlitePreferences
		changeRunIntervalDoubleContactOnPreferencesAndDB ();
	}

	private void on_spinbutton_runs_prevent_double_contact_value_changed (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		changeRunSimpleDoubleContactOnPreferencesAndDB ();
	}
	private void on_spinbutton_runs_i_prevent_double_contact_value_changed (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		changeRunIntervalDoubleContactOnPreferencesAndDB ();
	}

	private void changeRunSimpleDoubleContactOnPreferencesAndDB ()
	{
		//1.1 was FIRST or AVERAGE or LAST and now will be NONE
		if( (preferences.runDoubleContactsMode != Constants.DoubleContact.NONE) &&
				! PWBox.checkbutton_runs_prevent_double_contact.Active)
		{
				SqlitePreferences.Update("runDoubleContactsMode", Constants.DoubleContact.NONE.ToString(), false);
				preferences.runDoubleContactsMode = Constants.DoubleContact.NONE;
		}
		else if(PWBox.checkbutton_runs_prevent_double_contact.Active)
		{
			if( preferences.runDoubleContactsMode != Constants.DoubleContact.BIGGEST_TC ) {
				SqlitePreferences.Update("runDoubleContactsMode", Constants.DoubleContact.BIGGEST_TC.ToString(), false);
				preferences.runDoubleContactsMode = Constants.DoubleContact.BIGGEST_TC;
			}

			if(preferences.runDoubleContactsMS != (int) PWBox.spinbutton_runs_prevent_double_contact.Value) {
				SqlitePreferences.Update("runDoubleContactsMS",
						PWBox.spinbutton_runs_prevent_double_contact.Value.ToString(), false); //saved as string
				preferences.runDoubleContactsMS = (int) spinbutton_runs_prevent_double_contact.Value;
			}
		}
	}

	private void changeRunIntervalDoubleContactOnPreferencesAndDB ()
	{
		//2.1 was FIRST or AVERAGE or LAST and now will be NONE
		if( (preferences.runIDoubleContactsMode != Constants.DoubleContact.NONE) &&
				! PWBox.checkbutton_runs_i_prevent_double_contact.Active)
		{
				SqlitePreferences.Update("runIDoubleContactsMode", Constants.DoubleContact.NONE.ToString(), false);
				preferences.runIDoubleContactsMode = Constants.DoubleContact.NONE;
		}
		else if(PWBox.checkbutton_runs_i_prevent_double_contact.Active)
		{
			if( preferences.runIDoubleContactsMode != Constants.DoubleContact.BIGGEST_TC ) {
				SqlitePreferences.Update("runIDoubleContactsMode", Constants.DoubleContact.BIGGEST_TC.ToString(), false);
				preferences.runIDoubleContactsMode = Constants.DoubleContact.BIGGEST_TC;
			}

			if(preferences.runIDoubleContactsMS != (int) PWBox.spinbutton_runs_i_prevent_double_contact.Value) {
				SqlitePreferences.Update("runIDoubleContactsMS",
						PWBox.spinbutton_runs_i_prevent_double_contact.Value.ToString(), false); //saved as string
				preferences.runIDoubleContactsMS = (int) spinbutton_runs_i_prevent_double_contact.Value;
			}
		}
	}

	private void createComboEncoderCaptureCurvesSave ()
	{
		// combo_encoder_capture_curves_save ---->
		PWBox.combo_encoder_capture_curves_save = new ComboBoxText ();

		string [] comboEncoderCaptureCurvesSaveOptionsTranslated = {
			Catalog.GetString(Constants.EncoderAutoSaveCurvesStrings[0]),
			Catalog.GetString(Constants.EncoderAutoSaveCurvesStrings[1]),
			Catalog.GetString(Constants.EncoderAutoSaveCurvesStrings[2]),
			Catalog.GetString(Constants.EncoderAutoSaveCurvesStrings[3]),
			Catalog.GetString(Constants.EncoderAutoSaveCurvesStrings[4]),
			Catalog.GetString(Constants.EncoderAutoSaveCurvesStrings[5]) };
		encoderCaptureCurvesSaveOptionsTranslation = new String [comboEncoderCaptureCurvesSaveOptionsTranslated.Length];
		for(int j=0; j < comboEncoderCaptureCurvesSaveOptionsTranslated.Length ; j++)
			encoderCaptureCurvesSaveOptionsTranslation[j] =
				Constants.EncoderAutoSaveCurvesStrings[j] + ":" + comboEncoderCaptureCurvesSaveOptionsTranslated[j];
		UtilGtk.ComboUpdate (combo_encoder_capture_curves_save, comboEncoderCaptureCurvesSaveOptionsTranslated, "");
		combo_encoder_capture_curves_save.Active = UtilGtk.ComboMakeActive (combo_encoder_capture_curves_save,
				Catalog.GetString (Constants.GetEncoderAutoSaveCurvesStrings (preferences.encoderAutoSaveCurve)));
		combo_encoder_capture_curves_save.Changed += new EventHandler (on_combo_encoder_capture_curves_save_changed);

		hbox_encoder_capture_curves_save.PackStart (combo_encoder_capture_curves_save, true, true, 0);
		hbox_encoder_capture_curves_save.ShowAll ();

		spin_encoder_capture_curves_best_n.Value = preferences.encoderAutoSaveCurveBestNValue;
		manageVisibilityOf_spin_encoder_capture_curves_best_n ();
		// <---- combo_encoder_capture_curves_save
	}

	/* callbacks SQL change at any change for tab: encoder - capture */

	private void on_spin_encoder_capture_time_value_changed (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		preferences.encoderCaptureTime = Preferences.PreferencesChange(
				false, "encoderCaptureTime",
				preferences.encoderCaptureTime,
				(int) PWBox.spin_encoder_capture_time.Value);
	}

	private void on_check_encoder_capture_inactivity_end_time_clicked (object o, EventArgs args)
	{
		// A) changes on preferences gui
		hbox_encoder_capture_inactivity_time.Sensitive = check_encoder_capture_inactivity_end_time.Active;

		// B) changes on preferences object and SqlitePreferences
		changeEncoderInactivityEndTimeOnPreferencesAndDB ();
	}
	private void on_spin_encoder_capture_inactivity_end_time_value_changed (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		changeEncoderInactivityEndTimeOnPreferencesAndDB ();
	}
	private void changeEncoderInactivityEndTimeOnPreferencesAndDB ()
	{
		if(! PWBox.check_encoder_capture_inactivity_end_time.Active)
		{
			SqlitePreferences.Update("encoderCaptureInactivityEndTime", "-1", false);
			preferences.encoderCaptureInactivityEndTime = -1;
		} else {
			preferences.encoderCaptureInactivityEndTime = Preferences.PreferencesChange(
					false, "encoderCaptureInactivityEndTime",
					preferences.encoderCaptureInactivityEndTime,
					(int) PWBox.spin_encoder_capture_inactivity_end_time.Value);
		}
	}

	private void on_checkbutton_encoder_capture_inertial_discard_first_n_toggled (object o, EventArgs args)
	{
		// A) changes on preferences gui
		box_encoder_capture_inertial_discard_first_n.Visible = (checkbutton_encoder_capture_inertial_discard_first_n.Active);

		// B) changes on preferences object and SqlitePreferences
		changeEncoderInertialDiscardFirstNOnPreferencesAndDB ();
	}
	private void on_spin_encoder_capture_inertial_discard_first_n_value_changed (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		changeEncoderInertialDiscardFirstNOnPreferencesAndDB ();
	}
	private void changeEncoderInertialDiscardFirstNOnPreferencesAndDB ()
	{
		int spinEncoderCaptureDiscardFirstN = Convert.ToInt32(PWBox.spin_encoder_capture_inertial_discard_first_n.Value);
		if(! checkbutton_encoder_capture_inertial_discard_first_n.Active)
			spinEncoderCaptureDiscardFirstN = 0;

		if(spinEncoderCaptureDiscardFirstN != preferences.encoderCaptureInertialDiscardFirstN)
		{
			SqlitePreferences.Update("encoderCaptureInertialDiscardFirstN", spinEncoderCaptureDiscardFirstN.ToString(), false);
			preferences.encoderCaptureInertialDiscardFirstN = spinEncoderCaptureDiscardFirstN;
		}
	}

	// ---- combo_encoder_capture_curves_save ---->

	private void on_combo_encoder_capture_curves_save_changed (object o, EventArgs args)
	{
		manageVisibilityOf_spin_encoder_capture_curves_best_n ();
	}

	private void manageVisibilityOf_spin_encoder_capture_curves_best_n ()
	{
		string englishStr = Util.FindOnArray(
				':',1,0,UtilGtk.ComboGetActive(combo_encoder_capture_curves_save),
					encoderCaptureCurvesSaveOptionsTranslation);
		spin_encoder_capture_curves_best_n.Visible = (englishStr == "Best n" || englishStr == "Best n consecutive");

		write_label_encoder_capture_save_repetitions_explanation (englishStr);

		// changes on preferences and DB
		changeEncoderCaptureCurvesSaveOnPreferencesAndDB ();
	}

	private void write_label_encoder_capture_save_repetitions_explanation (string englishStr)
	{
		string explanationStr = "";
		Constants.EncoderAutoSaveCurve easc = Constants.GetEncoderAutoSaveCurvesEnum (englishStr);

		switch (easc)
		{
			case Constants.EncoderAutoSaveCurve.BEST :
				explanationStr = Catalog.GetString ("At the end of the capture, save the best repetition.");
				break;
			case Constants.EncoderAutoSaveCurve.BESTN :
				explanationStr = Catalog.GetString ("At the end of the capture, save the best n repetitions.");
				break;
			case Constants.EncoderAutoSaveCurve.BESTNCONSECUTIVE :
				explanationStr = Catalog.GetString ("At the end of the capture, save the best n consecutive repetitions.");
				break;
			case Constants.EncoderAutoSaveCurve.ALL :
				explanationStr = Catalog.GetString ("At the end of the capture, save all repetitions.");
				break;
			case Constants.EncoderAutoSaveCurve.FROM4TOPENULTIMATE :
				explanationStr = Catalog.GetString ("At the end of the capture, save all repetitions except the last one.");
				break;
			case Constants.EncoderAutoSaveCurve.NONE :
				explanationStr = Catalog.GetString ("At the end of the capture, do not automatically save any repetition.");
				break;
		}

		label_encoder_capture_save_repetitions_explanation.Text = explanationStr;
	}

	private void on_spin_encoder_capture_curves_best_n_value_changed (object o, EventArgs args)
	{
		// changes on preferences and DB
		changeEncoderCaptureCurvesSaveOnPreferencesAndDB ();
	}

	private void changeEncoderCaptureCurvesSaveOnPreferencesAndDB ()
	{
		//1) gest Constants.EncoderAutoSaveCurve
		string englishOption = Util.FindOnArray (':',1,0,
				UtilGtk.ComboGetActive (combo_encoder_capture_curves_save),
				encoderCaptureCurvesSaveOptionsTranslation);

		Constants.EncoderAutoSaveCurve easc = Constants.GetEncoderAutoSaveCurvesEnum (englishOption);

		//2) update preferences
		preferences.encoderAutoSaveCurve = easc;

		//3) update Sqlite
		SqlitePreferences.Update ("encoderAutoSaveCurve", easc.ToString(), false);

		if(easc == Constants.EncoderAutoSaveCurve.BESTN || easc == Constants.EncoderAutoSaveCurve.BESTNCONSECUTIVE)
			SqlitePreferences.Update (
					SqlitePreferences.EncoderAutoSaveCurveBestNValue,
					spin_encoder_capture_curves_best_n.Value.ToString(), false);
	}

	// <---- combo_encoder_capture_curves_save ----

	private void on_radio_encoder_rep_criteria_gravitatory_toggled (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		//radio_encoder_rep_criteria_gravitatory_*
		if(PWBox.radio_encoder_rep_criteria_gravitatory_ecc_con.Active &&
				preferences.encoderRepetitionCriteriaGravitatory != Preferences.EncoderRepetitionCriteria.ECC_CON)
		{
			SqlitePreferences.Update(SqlitePreferences.EncoderRepetitionCriteriaGravitatoryStr,
					Preferences.EncoderRepetitionCriteria.ECC_CON.ToString(), false);
			preferences.encoderRepetitionCriteriaGravitatory = Preferences.EncoderRepetitionCriteria.ECC_CON;
		}
		else if(PWBox.radio_encoder_rep_criteria_gravitatory_ecc.Active &&
				preferences.encoderRepetitionCriteriaGravitatory != Preferences.EncoderRepetitionCriteria.ECC)
		{
			SqlitePreferences.Update(SqlitePreferences.EncoderRepetitionCriteriaGravitatoryStr,
					Preferences.EncoderRepetitionCriteria.ECC.ToString(), false);
			preferences.encoderRepetitionCriteriaGravitatory = Preferences.EncoderRepetitionCriteria.ECC;
		}
		else if(PWBox.radio_encoder_rep_criteria_gravitatory_con.Active &&
				preferences.encoderRepetitionCriteriaGravitatory != Preferences.EncoderRepetitionCriteria.CON)
		{
			SqlitePreferences.Update(SqlitePreferences.EncoderRepetitionCriteriaGravitatoryStr,
					Preferences.EncoderRepetitionCriteria.CON.ToString(), false);
			preferences.encoderRepetitionCriteriaGravitatory = Preferences.EncoderRepetitionCriteria.CON;
		}
	}

	private void on_radio_encoder_rep_criteria_inertial_toggled (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		//radio_encoder_rep_criteria_inertial_*
		if(PWBox.radio_encoder_rep_criteria_inertial_ecc_con.Active &&
				preferences.encoderRepetitionCriteriaInertial != Preferences.EncoderRepetitionCriteria.ECC_CON)
		{
			SqlitePreferences.Update(SqlitePreferences.EncoderRepetitionCriteriaInertialStr,
					Preferences.EncoderRepetitionCriteria.ECC_CON.ToString(), false);
			preferences.encoderRepetitionCriteriaInertial = Preferences.EncoderRepetitionCriteria.ECC_CON;
		}
		else if(PWBox.radio_encoder_rep_criteria_inertial_ecc.Active &&
				preferences.encoderRepetitionCriteriaInertial != Preferences.EncoderRepetitionCriteria.ECC)
		{
			SqlitePreferences.Update(SqlitePreferences.EncoderRepetitionCriteriaInertialStr,
					Preferences.EncoderRepetitionCriteria.ECC.ToString(), false);
			preferences.encoderRepetitionCriteriaInertial = Preferences.EncoderRepetitionCriteria.ECC;
		}
		else if(PWBox.radio_encoder_rep_criteria_inertial_con.Active &&
				preferences.encoderRepetitionCriteriaInertial != Preferences.EncoderRepetitionCriteria.CON)
		{
			SqlitePreferences.Update(SqlitePreferences.EncoderRepetitionCriteriaInertialStr,
					Preferences.EncoderRepetitionCriteria.CON.ToString(), false);
			preferences.encoderRepetitionCriteriaInertial = Preferences.EncoderRepetitionCriteria.CON;
		}
	}

	private void on_radio_encoder_capture_show_all_bars_toggled (object o, EventArgs args)
	{
		// A) changes on preferences gui
		spin_encoder_capture_show_only_some_bars.Sensitive = false;

		// B) changes on preferences object and SqlitePreferences
		changeEncoderCaptureShowOnlyBarsOnPreferencesAndDB ();
	}
	private void on_radio_encoder_capture_show_only_some_bars_toggled (object o, EventArgs args)
	{
		// A) changes on preferences gui
		spin_encoder_capture_show_only_some_bars.Sensitive = true;

		// B) changes on preferences object and SqlitePreferences
		changeEncoderCaptureShowOnlyBarsOnPreferencesAndDB ();
	}
	private void on_spin_encoder_capture_show_only_some_bars_value_changed (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		changeEncoderCaptureShowOnlyBarsOnPreferencesAndDB ();
	}
	private void changeEncoderCaptureShowOnlyBarsOnPreferencesAndDB ()
	{
		if( preferences.encoderCaptureShowNRepetitions > 0 && PWBox.radio_encoder_capture_show_all_bars.Active )
		{
			SqlitePreferences.Update("encoderCaptureShowNRepetitions", "-1", false);
			preferences.encoderCaptureShowNRepetitions = -1;
		}
		else if( PWBox.radio_encoder_capture_show_only_some_bars.Active &&
				preferences.encoderCaptureShowNRepetitions != (int) PWBox.spin_encoder_capture_show_only_some_bars.Value) {
			SqlitePreferences.Update("encoderCaptureShowNRepetitions",
					PWBox.spin_encoder_capture_show_only_some_bars.Value.ToString(), false);
			preferences.encoderCaptureShowNRepetitions = (int) PWBox.spin_encoder_capture_show_only_some_bars.Value;
		}
	}

	private void on_spin_encoder_capture_barplot_font_size_value_changed (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		preferences.encoderCaptureBarplotFontSize = Preferences.PreferencesChange(
				false, "encoderCaptureBarplotFontSize",
				preferences.encoderCaptureBarplotFontSize,
				(int) PWBox.spin_encoder_capture_barplot_font_size.Value);
	}

	private void on_check_show_start_and_duration_clicked (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		preferences.encoderShowStartAndDuration = Preferences.PreferencesChange(
				false, "encoderShowStartAndDuration",
				preferences.encoderShowStartAndDuration,
				PWBox.check_show_start_and_duration.Active);
	}

	private void on_radio_encoder_triggers_toggled (object o, EventArgs args)
	{
		// A) changes on preferences gui
		Pixbuf pixbuf;
		if(radio_encoder_triggers_no.Active)
		{
			vbox_encoder_triggers_yes.Visible = false;
			pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "image_encoder_triggers_no.png");
		PWBox.image_encoder_triggers.Pixbuf = pixbuf;
		} else {
			vbox_encoder_triggers_yes.Visible = true;
			pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "image_encoder_triggers.png");
		}
		image_encoder_triggers.Pixbuf = pixbuf;

		// B) changes on preferences object and SqlitePreferences
		changeEncoderCaptureTriggersOnPreferencesAndDB ();
	}
	private void on_radio_encoder_triggers_yes_start_at_toggled (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		changeEncoderCaptureTriggersOnPreferencesAndDB ();
	}
	private void changeEncoderCaptureTriggersOnPreferencesAndDB ()
	{
		if(PWBox.radio_encoder_triggers_no.Active &&
				preferences.encoderCaptureCutByTriggers != Preferences.TriggerTypes.NO_TRIGGERS)
		{
			SqlitePreferences.Update("encoderCaptureCutByTriggers", Preferences.TriggerTypes.NO_TRIGGERS.ToString(), false);
			preferences.encoderCaptureCutByTriggers = Preferences.TriggerTypes.NO_TRIGGERS;
		}
		else if(PWBox.radio_encoder_triggers_yes.Active &&
				PWBox.radio_encoder_triggers_yes_start_at_capture.Active &&
				preferences.encoderCaptureCutByTriggers != Preferences.TriggerTypes.START_AT_CAPTURE)
		{
			SqlitePreferences.Update("encoderCaptureCutByTriggers", Preferences.TriggerTypes.START_AT_CAPTURE.ToString(), false);
			preferences.encoderCaptureCutByTriggers = Preferences.TriggerTypes.START_AT_CAPTURE;
		}
		else if(PWBox.radio_encoder_triggers_yes.Active &&
				PWBox.radio_encoder_triggers_yes_start_at_first_trigger.Active &&
				preferences.encoderCaptureCutByTriggers != Preferences.TriggerTypes.START_AT_FIRST_ON)
		{
			SqlitePreferences.Update("encoderCaptureCutByTriggers", Preferences.TriggerTypes.START_AT_FIRST_ON.ToString(), false);
			preferences.encoderCaptureCutByTriggers = Preferences.TriggerTypes.START_AT_FIRST_ON;
		}
	}

	private void on_check_encoder_capture_infinite_clicked (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		preferences.encoderCaptureInfinite = Preferences.PreferencesChange(
				false,
				SqlitePreferences.EncoderCaptureInfinite, preferences.encoderCaptureInfinite,
				PWBox.check_encoder_capture_infinite.Active);
	}


	/* callbacks SQL change at any change for tab: encoder - other */

	private void on_checkbutton_encoder_propulsive_clicked (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		preferences.encoderPropulsive = Preferences.PreferencesChange(
				false, "encoderPropulsive",
				preferences.encoderPropulsive,
				PWBox.checkbutton_encoder_propulsive.Active);
	}

	private void on_radio_encoder_work_toggled (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		preferences.encoderWorkKcal = Preferences.PreferencesChange(
				false, SqlitePreferences.EncoderWorkKcal,
				preferences.encoderWorkKcal,
				radio_encoder_work_kcal.Active);
	}

	private void on_radio_encoder_inertial_analyze_toggled (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		Preferences.EncoderInertialGraphsXTypes encoderInertialGraphsXFromGUI = get_encoderInertialGraphsX_from_gui();
		if(preferences.encoderInertialGraphsX != encoderInertialGraphsXFromGUI)
		{
			SqlitePreferences.Update(SqlitePreferences.EncoderInertialGraphsX, encoderInertialGraphsXFromGUI.ToString(), false);
			preferences.encoderInertialGraphsX = encoderInertialGraphsXFromGUI;
		}
	}
	private Preferences.EncoderInertialGraphsXTypes get_encoderInertialGraphsX_from_gui()
	{
		if(PWBox.radio_encoder_inertial_analyze_inertia_moment.Active)
			return Preferences.EncoderInertialGraphsXTypes.INERTIA_MOMENT;
		else if(PWBox.radio_encoder_inertial_analyze_diameter.Active)
			return Preferences.EncoderInertialGraphsXTypes.DIAMETER;
		else
			return Preferences.EncoderInertialGraphsXTypes.EQUIVALENT_MASS;
	}

	private void on_spin_encoder_smooth_con_value_changed (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		preferences.encoderSmoothCon = Preferences.PreferencesChange(
				false, "encoderSmoothCon",
				preferences.encoderSmoothCon,
				(double) PWBox.spin_encoder_smooth_con.Value);
	}
	private void on_radio_encoder_1RM_weight_toggled (object o, EventArgs args)
	{
		Constants.Encoder1RMMethod encoder1RMMethod;
		if(PWBox.radio_encoder_1RM_nonweighted.Active)
			encoder1RMMethod = Constants.Encoder1RMMethod.NONWEIGHTED;
		else if(PWBox.radio_encoder_1RM_weighted.Active)
			encoder1RMMethod = Constants.Encoder1RMMethod.WEIGHTED;
		else if(PWBox.radio_encoder_1RM_weighted2.Active)
			encoder1RMMethod = Constants.Encoder1RMMethod.WEIGHTED2;
		else // (PWBox.radio_encoder_1RM_weighted3.Active)
			encoder1RMMethod = Constants.Encoder1RMMethod.WEIGHTED3;

		if(preferences.encoder1RMMethod != encoder1RMMethod) {
			SqlitePreferences.Update("encoder1RMMethod", encoder1RMMethod.ToString(), false);
			preferences.encoder1RMMethod = encoder1RMMethod;
		}
	}


	/* callbacks SQL change at any change for tab: forceSensor */

	//butterworth - isometric mode
	private void on_check_force_sensor_isometric_butterworth_clicked (object o, EventArgs args)
	{
		if (signalsNoFollow)
			return;

		// A) changes on preferences gui
		box_force_sensor_isometric_butterworth_values.Sensitive = check_force_sensor_isometric_butterworth.Active;

		// B) changes on preferences object and SqlitePreferences
		changeForceSensorIsometricButterworthOnPreferencesAndDB ();
	}
	private void changeForceSensorIsometricButterworthOnPreferencesAndDB ()
	{
		if(! PWBox.check_force_sensor_isometric_butterworth.Active)
		{
			SqlitePreferences.Update(SqlitePreferences.ForceSensorIsometricButterworth, "-1", false);
			preferences.forceSensorIsometricButterworth = -1;
		} else
			on_spin_force_sensor_isometric_butterworth_value_changed (new object (), new EventArgs ());
	}

	private void on_spin_force_sensor_isometric_butterworth_value_changed (object o, EventArgs args)
	{
		if (signalsNoFollow)
			return;

		// B) changes on preferences object and SqlitePreferences
		preferences.forceSensorIsometricButterworth = Preferences.PreferencesChange(
				false,
				SqlitePreferences.ForceSensorIsometricButterworth,
				preferences.forceSensorIsometricButterworth,
				Convert.ToDouble(spin_force_sensor_isometric_butterworth.Value));
	}

	//butterworth - elastic mode
	private void on_check_force_sensor_elastic_butterworth_clicked (object o, EventArgs args)
	{
		if (signalsNoFollow)
			return;

		// A) changes on preferences gui
		box_force_sensor_elastic_butterworth_values.Sensitive = check_force_sensor_elastic_butterworth.Active;

		// B) changes on preferences object and SqlitePreferences
		changeForceSensorElasticButterworthOnPreferencesAndDB ();
	}
	private void changeForceSensorElasticButterworthOnPreferencesAndDB ()
	{
		if(! PWBox.check_force_sensor_elastic_butterworth.Active)
		{
			SqlitePreferences.Update(SqlitePreferences.ForceSensorElasticButterworth, "-1", false);
			preferences.forceSensorElasticButterworth = -1;
		} else
			on_spin_force_sensor_elastic_butterworth_value_changed (new object (), new EventArgs ());
	}

	private void on_spin_force_sensor_elastic_butterworth_value_changed (object o, EventArgs args)
	{
		if (signalsNoFollow)
			return;

		// B) changes on preferences object and SqlitePreferences
		preferences.forceSensorElasticButterworth = Preferences.PreferencesChange(
				false,
				SqlitePreferences.ForceSensorElasticButterworth,
				preferences.forceSensorElasticButterworth,
				Convert.ToDouble(spin_force_sensor_elastic_butterworth.Value));
	}


	private void on_spin_force_sensor_capture_width_graph_seconds_value_changed (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		preferences.forceSensorCaptureWidthSeconds = Preferences.PreferencesChange(
				false,
				SqlitePreferences.ForceSensorCaptureWidthSeconds,
				preferences.forceSensorCaptureWidthSeconds,
				Convert.ToInt32(spin_force_sensor_capture_width_graph_seconds.Value));
	}

	private void on_radio_force_sensor_capture_scroll_zoom_toggled (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		preferences.forceSensorCaptureScroll = Preferences.PreferencesChange(
				false,
				SqlitePreferences.ForceSensorCaptureScroll,
				preferences.forceSensorCaptureScroll,
				radio_force_sensor_capture_scroll.Active);
	}

	private void on_spin_force_sensor_graphs_line_width_value_changed (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		preferences.forceSensorGraphsLineWidth = Preferences.PreferencesChange(
				false,
				SqlitePreferences.ForceSensorGraphsLineWidth,
				preferences.forceSensorGraphsLineWidth,
				Convert.ToInt32(spin_force_sensor_graphs_line_width.Value));
	}

	private void on_spin_force_sensor_not_elastic_ecc_min_force_value_changed (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		preferences.forceSensorNotElasticEccMinForce = Preferences.PreferencesChange(
				false,
				SqlitePreferences.ForceSensorNotElasticEccMinForce,
				preferences.forceSensorNotElasticEccMinForce,
				Convert.ToInt32(spin_force_sensor_not_elastic_ecc_min_force.Value));
	}

	private void on_spin_force_sensor_not_elastic_con_min_force_value_changed (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		preferences.forceSensorNotElasticConMinForce = Preferences.PreferencesChange(
				false,
				SqlitePreferences.ForceSensorNotElasticConMinForce,
				preferences.forceSensorNotElasticConMinForce,
				Convert.ToInt32(spin_force_sensor_not_elastic_con_min_force.Value));
	}

	private void on_spin_force_sensor_elastic_ecc_min_displ_value_changed (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		preferences.forceSensorElasticEccMinDispl = Preferences.PreferencesChange(
				false,
				SqlitePreferences.ForceSensorElasticEccMinDispl,
				preferences.forceSensorElasticEccMinDispl,
				Convert.ToDouble(spin_force_sensor_elastic_ecc_min_displ.Value));
	}

	private void on_spin_force_sensor_elastic_con_min_displ_value_changed (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		preferences.forceSensorElasticConMinDispl = Preferences.PreferencesChange(
				false,
				SqlitePreferences.ForceSensorElasticConMinDispl,
				preferences.forceSensorElasticConMinDispl,
				Convert.ToDouble(spin_force_sensor_elastic_con_min_displ.Value));
	}

	private void on_radio_force_sensor_variability_toggled (object o, EventArgs args)
	{
		// A) changes on preferences gui
		//only manage active
		if (! ((Gtk.RadioButton) o).Active)
			return;

		if (o == (object) radio_force_sensor_variability_rmssd)
			hbox_force_sensor_lag.Visible = true;
		else if (o == (object) radio_force_sensor_variability_cvrmssd)
			hbox_force_sensor_lag.Visible = true;
		else if (o == (object) radio_force_sensor_variability_cv)
			hbox_force_sensor_lag.Visible = false;
		else // (o == (object) radio_force_sensor_variability_old)
			hbox_force_sensor_lag.Visible = false;

		// B) changes on preferences object and SqlitePreferences
		if(PWBox.radio_force_sensor_variability_rmssd.Active &&
				preferences.forceSensorVariabilityMethod != Preferences.VariabilityMethodEnum.RMSSD)
		{
			SqlitePreferences.Update(SqlitePreferences.ForceSensorVariabilityMethod, Preferences.VariabilityMethodEnum.RMSSD.ToString(), false);
			preferences.forceSensorVariabilityMethod = Preferences.VariabilityMethodEnum.RMSSD;
		}
		else if(PWBox.radio_force_sensor_variability_cvrmssd.Active &&
				preferences.forceSensorVariabilityMethod != Preferences.VariabilityMethodEnum.CVRMSSD)
		{
			SqlitePreferences.Update(SqlitePreferences.ForceSensorVariabilityMethod, Preferences.VariabilityMethodEnum.CVRMSSD.ToString(), false);
			preferences.forceSensorVariabilityMethod = Preferences.VariabilityMethodEnum.CVRMSSD;
		}
		else if(PWBox.radio_force_sensor_variability_cv.Active &&
				preferences.forceSensorVariabilityMethod != Preferences.VariabilityMethodEnum.CV)
		{
			SqlitePreferences.Update(SqlitePreferences.ForceSensorVariabilityMethod, Preferences.VariabilityMethodEnum.CV.ToString(), false);
			preferences.forceSensorVariabilityMethod = Preferences.VariabilityMethodEnum.CV;
		}
		else if(PWBox.radio_force_sensor_variability_old.Active &&
				preferences.forceSensorVariabilityMethod != Preferences.VariabilityMethodEnum.CHRONOJUMP_OLD)
		{
			SqlitePreferences.Update(SqlitePreferences.ForceSensorVariabilityMethod, Preferences.VariabilityMethodEnum.CHRONOJUMP_OLD.ToString(), false);
			preferences.forceSensorVariabilityMethod = Preferences.VariabilityMethodEnum.CHRONOJUMP_OLD;
		}
	}

	private void on_spin_force_sensor_variability_lag_value_changed (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		preferences.forceSensorVariabilityLag = Preferences.PreferencesChange(
				false,
				SqlitePreferences.ForceSensorVariabilityLag,
				preferences.forceSensorVariabilityLag,
				Convert.ToInt32(spin_force_sensor_variability_lag.Value));
	}

	private void on_spin_force_sensor_analyze_best_stability_in_window_value_changed (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		preferences.forceSensorAnalyzeBestStabilityInWindow = Preferences.PreferencesChange(
				false,
				SqlitePreferences.ForceSensorAnalyzeBestStabilityInWindow,
				preferences.forceSensorAnalyzeBestStabilityInWindow,
				Convert.ToDouble(spin_force_sensor_analyze_best_stability_in_window.Value));
	}

	private void on_spin_force_sensor_analyze_max_avg_force_in_window_value_changed (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		preferences.forceSensorAnalyzeMaxAVGInWindow = Preferences.PreferencesChange(
				false,
				SqlitePreferences.ForceSensorAnalyzeMaxAVGInWindow,
				preferences.forceSensorAnalyzeMaxAVGInWindow,
				Convert.ToDouble(spin_force_sensor_analyze_max_avg_force_in_window.Value));
	}

	/* callbacks SQL change at any change for tab: raceAnalyzer */

	private void on_spin_run_encoder_acceleration_value_changed (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		preferences.runEncoderMinAccel = Preferences.PreferencesChange (
				false,
				SqlitePreferences.RunEncoderMinAccel,
				preferences.runEncoderMinAccel,
				Convert.ToDouble (spin_run_encoder_acceleration.Value));
	}

	private void on_spin_run_encoder_pps_value_changed (object o, EventArgs args)
	{
		// A) changes on preferences gui
		update_run_encoder_gui_pps_equivalence_and_max ();

		// B) changes on preferences object and SqlitePreferences
		preferences.runEncoderPPS = Preferences.PreferencesChange (
				false,
				SqlitePreferences.RunEncoderPPS,
				preferences.runEncoderPPS,
				Convert.ToInt32 (spin_run_encoder_pps.Value));
	}

	private void update_run_encoder_gui_pps_equivalence_and_max ()
	{
		label_pps_equivalent.Text = string.Format(Catalog.GetString("{0} pps is equivalent to a resolution of {1} cm."),
				spin_run_encoder_pps.Value, 0.3003 * spin_run_encoder_pps.Value);

		label_pps_maximum.Text = string.Format(Catalog.GetString("{0} pps allows to record up to {1} m/s."),
				spin_run_encoder_pps.Value, spin_run_encoder_pps.Value * 4);
	}

	/* callbacks SQL change at any change for tab: multimedia/sound */

	private void on_checkbutton_volume_clicked (object o, EventArgs args)
	{
		// A) changes on preferences gui
		alignment_multimedia_sounds.Visible = checkbutton_volume.Active;

		// B) changes on preferences object and SqlitePreferences
		if( preferences.volumeOn != PWBox.checkbutton_volume.Active ) {
			SqlitePreferences.Update ("volumeOn", PWBox.checkbutton_volume.Active.ToString(), false);
			preferences.volumeOn = PWBox.checkbutton_volume.Active;
		}
	}

	private void on_multimedia_sound_radios_toggled (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		if( preferences.gstreamer != Preferences.GstreamerTypes.GST_1_0 && radio_gstreamer_1_0.Active)
		{
			SqlitePreferences.Update(Preferences.GstreamerStr, Preferences.GstreamerTypes.GST_1_0.ToString(), false);
			preferences.gstreamer = Preferences.GstreamerTypes.GST_1_0;
		}
		else if( preferences.gstreamer != Preferences.GstreamerTypes.GST_0_1 && radio_gstreamer_0_1.Active)
		{
			SqlitePreferences.Update(Preferences.GstreamerStr, Preferences.GstreamerTypes.GST_0_1.ToString(), false);
			preferences.gstreamer = Preferences.GstreamerTypes.GST_0_1;
		}
		else if( preferences.gstreamer != Preferences.GstreamerTypes.FFPLAY && radio_ffplay.Active)
		{
			SqlitePreferences.Update(Preferences.GstreamerStr, Preferences.GstreamerTypes.FFPLAY.ToString(), false);
			preferences.gstreamer = Preferences.GstreamerTypes.FFPLAY;
		}
		else if( preferences.gstreamer != Preferences.GstreamerTypes.SYSTEMSOUNDS && radio_sound_systemsounds.Active)
		{
			SqlitePreferences.Update(Preferences.GstreamerStr, Preferences.GstreamerTypes.SYSTEMSOUNDS.ToString(), false);
			preferences.gstreamer = Preferences.GstreamerTypes.SYSTEMSOUNDS;
		}
	}

	/* callbacks SQL change at any change for tab: multimedia/camera */

	private void on_combo_camera_changed (object o, EventArgs args)
	{
		// A) changes on preferences gui

		//if camera changes then do not allow to view/change format, resolution, framerate, or preview until configure button is clicked
		label_camera_pixel_format_current.Visible = false;
		label_camera_resolution_current.Visible = false;
		label_camera_framerate_current.Visible = false;

		hbox_combo_camera_pixel_format.Visible = false;
		hbox_combo_camera_resolution.Visible = false;
		hbox_combo_camera_framerate.Visible = false;

		//blank camera values
		UtilGtk.ComboDelAll(combo_camera_pixel_format);
		UtilGtk.ComboDelAll(combo_camera_resolution);
		UtilGtk.ComboDelAll(combo_camera_framerate);

		//do not allow to preview
		button_video_preview.Sensitive = false;

		// B) changes on preferences object and SqlitePreferences
		string cameraCode = wd_list.GetCodeOfFullname (UtilGtk.ComboGetActive (combo_camera));
		if (cameraCode != "" && preferences.videoDevice != cameraCode) {
			SqlitePreferences.Update ("videoDevice", cameraCode, false);
			preferences.videoDevice = cameraCode;
		}
	}

	private void on_combo_camera_pixel_format_changed (object o, EventArgs args)
	{
		// A) changes on preferences gui
		string pixelFormat = getSelectedPixelFormat ();

		if(pixelFormat != "" && wfsm != null)
		{
			string currentResolution = getSelectedResolution();
			UtilGtk.ComboUpdate(combo_camera_resolution, wfsm.PopulateListByPixelFormat(pixelFormat));
			combo_camera_resolution.Active = UtilGtk.ComboMakeActive(combo_camera_resolution, currentResolution);
			button_video_preview.Sensitive = true;
		}

		// B) changes on preferences object and SqlitePreferences
		if (preferences.videoDevicePixelFormat != pixelFormat) {
			SqlitePreferences.Update ("videoDevicePixelFormat", pixelFormat, false);
			preferences.videoDevicePixelFormat = pixelFormat;
		}
	}

	private void on_combo_camera_resolution_changed (object o, EventArgs args)
	{
		// A) changes on preferences gui
		string pixelFormat = UtilGtk.ComboGetActive(combo_camera_pixel_format);
		string resolution = UtilGtk.ComboGetActive(combo_camera_resolution);
		hbox_camera_resolution_custom.Visible = resolution == Catalog.GetString("Custom");

		if(resolution != "" && resolution != Catalog.GetString("Custom") && wfsm != null)
		{
			string currentFramerate = getSelectedFramerate();
			UtilGtk.ComboUpdate(combo_camera_framerate, wfsm.GetFramerates (pixelFormat, resolution));
			combo_camera_framerate.Active = UtilGtk.ComboMakeActive(combo_camera_framerate, currentFramerate);
		}

		// B) changes on preferences object and SqlitePreferences
		resolution = getSelectedResolution ();
		if (preferences.videoDeviceResolution != resolution) {
			SqlitePreferences.Update( "videoDeviceResolution", resolution, false);
			preferences.videoDeviceResolution = resolution;
		}
	}

	private void on_combo_camera_framerate_changed (object o, EventArgs args)
	{
		// A) changes on preferences gui
		hbox_camera_framerate_custom.Visible = UtilGtk.ComboGetActive(combo_camera_framerate) == Catalog.GetString("Custom");

		// B) changes on preferences object and SqlitePreferences
		string framerate = getSelectedFramerate ();
		if (preferences.videoDeviceFramerate != framerate) {
			SqlitePreferences.Update ("videoDeviceFramerate", framerate, false);
			preferences.videoDeviceFramerate = framerate; //if it has decimals, separator should be a point
		}
	}

	private void on_check_camera_stop_after_toggled (object o, EventArgs args)
	{
		// A) changes on preferences gui
		hbox_camera_stop_after_seconds.Visible = check_camera_stop_after.Active;

		// B) changes on preferences object and SqlitePreferences
		changeCameraStopAfterOnPreferencesAndDB ();
	}
	private void on_spin_camera_stop_after_value_changed (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		changeCameraStopAfterOnPreferencesAndDB ();
	}
	private void changeCameraStopAfterOnPreferencesAndDB ()
	{
		int selected_camera_stop_after = Convert.ToInt32 (spin_camera_stop_after.Value);
		if (! check_camera_stop_after.Active)
			selected_camera_stop_after = 0;
		if (preferences.videoStopAfter != selected_camera_stop_after) {
			SqlitePreferences.Update("videoStopAfter", selected_camera_stop_after.ToString(), false);
			preferences.videoStopAfter = selected_camera_stop_after;
		}
	}

	/* callbacks SQL change at any change for tab: language */

	private void restartLabelShow ()
	{
		hbox_buttons_bottom.Visible = false;
		label_restart.Visible = true;
		GLib.Timeout.Add(1500, new GLib.TimeoutHandler (restartLabelHide));
	}
	private bool restartLabelHide ()
	{
		hbox_buttons_bottom.Visible = true;
		label_restart.Visible = false;

		return false; //do not call this again
	}

	private void on_radio_language_toggled (object obj, EventArgs args)
	{
		// A) changes on preferences gui
		hbox_combo_language.Sensitive = radio_language_force.Active;

		if(! signalsNoFollow)
			restartLabelShow ();

		// B) changes on preferences object and SqlitePreferences
		changeLanguageOnPreferencesAndDB ();
	}
	private	void combo_language_changed (object obj, EventArgs args)
	{
		// A) changes on preferences gui
		if(! signalsNoFollow)
			restartLabelShow ();

		// B) changes on preferences object and SqlitePreferences
		changeLanguageOnPreferencesAndDB ();
	}
	private void changeLanguageOnPreferencesAndDB ()
	{
		string selectedLanguage = getSelectedLanguage();

		//if there was a language on SQL but now "detected" is selected, put "" in language on SQL
		if (preferences.language != "" && radio_language_detected.Active) {
			SqlitePreferences.Update ("language", "", false);
			preferences.language = "";
		}
		//if force a language, and SQL language is != than selected language, change language on SQL
		else if (radio_language_force.Active && preferences.language != selectedLanguage) {
			SqlitePreferences.Update ("language", selectedLanguage, false);
			preferences.language = selectedLanguage;
		}
	}

	private void on_radio_export_latin_non_latin_toggled (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		if ( ! ((Gtk.RadioButton) o).Active)
			return;

		if (preferences.CSVExportDecimalSeparator == "POINT" &&
				PWBox.radio_export_latin.Active)
		{
			SqlitePreferences.Update ("CSVExportDecimalSeparator","COMMA", false);
			preferences.CSVExportDecimalSeparator = "COMMA";
		}
		else if (preferences.CSVExportDecimalSeparator == "COMMA" &&
				! PWBox.radio_export_latin.Active)
		{
			SqlitePreferences.Update ("CSVExportDecimalSeparator","POINT", false);
			preferences.CSVExportDecimalSeparator = "POINT";
		}
	}

	private void on_radio_translate_toggled (object obj, EventArgs args)
	{
		// A) changes on preferences gui
		if(! signalsNoFollow)
			restartLabelShow ();

		// B) changes on preferences object and SqlitePreferences
		if (preferences.RGraphsTranslate != PWBox.radio_graphs_translate.Active) {
			SqlitePreferences.Update ("RGraphsTranslate",
					PWBox.radio_graphs_translate.Active.ToString(), false);
			preferences.RGraphsTranslate = PWBox.radio_graphs_translate.Active;
		}
	}

	/* callbacks SQL change at any change for tab: advanced */

	private void on_check_networks_devices_clicked (object o, EventArgs args)
	{
		// this is not stored in SQL. used on networks
		preferences.networksAllowChangeDevices = PWBox.check_networks_devices.Active;
	}

	private void on_checkbutton_ask_deletion_clicked (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		if (preferences.askDeletion != PWBox.checkbutton_ask_deletion.Active) {
			SqlitePreferences.Update ("askDeletion", PWBox.checkbutton_ask_deletion.Active.ToString(), false);
			preferences.askDeletion = PWBox.checkbutton_ask_deletion.Active;
		}
	}

	private void on_combo_decimals_changed (object o, EventArgs args)
	{
		if (UtilGtk.ComboGetActive (combo_decimals) == "")
			return;

		// B) changes on preferences object and SqlitePreferences
		if (preferences.digitsNumber != Convert.ToInt32(UtilGtk.ComboGetActive(combo_decimals))) {
			SqlitePreferences.Update ("digitsNumber", UtilGtk.ComboGetActive(combo_decimals), false);
			preferences.digitsNumber = Convert.ToInt32(UtilGtk.ComboGetActive(combo_decimals));
		}
	}

	private void on_radio_python_2_3_toggled (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		Preferences.pythonVersionEnum pythonVersionFromGUI = get_pythonVersion_from_gui();
		if (preferences.importerPythonVersion != pythonVersionFromGUI)
		{
			SqlitePreferences.Update (SqlitePreferences.ImporterPythonVersion, pythonVersionFromGUI.ToString(), false);
			preferences.importerPythonVersion = pythonVersionFromGUI;
		}
	}

	private void on_radio_font_toggled (object o, EventArgs args)
	{
		if (signalsNoFollow)
			return;

		// A) changes on preferences gui

		// B) changes on preferences object and SqlitePreferences
		changeFontOnPreferencesAndDB ();
	}
	private void changeFontOnPreferencesAndDB ()
	{
		if (radio_font_helvetica.Active && preferences.fontType != Preferences.FontTypes.Helvetica)
		{
			SqlitePreferences.Update (SqlitePreferences.FontsOnGraphs, Preferences.FontTypes.Helvetica.ToString(), false);
			preferences.fontType = Preferences.FontTypes.Helvetica;
		}
		else if (radio_font_courier.Active && preferences.fontType != Preferences.FontTypes.Courier)
		{
			SqlitePreferences.Update (SqlitePreferences.FontsOnGraphs, Preferences.FontTypes.Courier.ToString(), false);
			preferences.fontType = Preferences.FontTypes.Courier;
		}
		else if (radio_font_noto_sans_cjk_sc.Active && preferences.fontType != Preferences.FontTypes.Noto_Sans_CJK_SC)
		{
			SqlitePreferences.Update (SqlitePreferences.FontsOnGraphs, Preferences.FontTypes.Noto_Sans_CJK_SC.ToString(), false);
			preferences.fontType = Preferences.FontTypes.Noto_Sans_CJK_SC;
		}
	}

	private void on_radio_cloud_no_toggled (object o, EventArgs args)
	{
		if (signalsNoFollow)
			return;

		configAtPrefs.UpdateFieldEnsuringDefaultConfigFile (
			Config.OpEnum.CopyToCloudFullPath.ToString (), "");
		configAtPrefs.UpdateFieldEnsuringDefaultConfigFile (
			Config.OpEnum.ReadFromCloudMainPath.ToString (), "");
		configAtPrefs.UpdateFieldEnsuringDefaultConfigFile (
			Config.OpEnum.LastDBFullPath.ToString (), "");

		buttons_cloud_sensitive ();
		restartLabelShow ();
	}

	private void on_radio_cloud_capture_toggled (object o, EventArgs args)
	{
		if (signalsNoFollow)
			return;

		if (label_cloud_capture_path.Text != "")
			configAtPrefs.UpdateFieldEnsuringDefaultConfigFile (
				Config.OpEnum.CopyToCloudFullPath.ToString (), label_cloud_capture_path.Text);
		configAtPrefs.UpdateFieldEnsuringDefaultConfigFile (
			Config.OpEnum.ReadFromCloudMainPath.ToString (), "");

		buttons_cloud_sensitive ();
		restartLabelShow ();
	}

	private void on_radio_cloud_view_toggled (object o, EventArgs args)
	{
		if (signalsNoFollow)
			return;

		configAtPrefs.UpdateFieldEnsuringDefaultConfigFile (
			Config.OpEnum.CopyToCloudFullPath.ToString (), "");
		if (label_cloud_view_path.Text != "")
			configAtPrefs.UpdateFieldEnsuringDefaultConfigFile (
					Config.OpEnum.ReadFromCloudMainPath.ToString (), label_cloud_view_path.Text);

		buttons_cloud_sensitive ();
		restartLabelShow ();
	}

	private void on_button_cloud_capture_path_clicked (object o, EventArgs args)
	{
		button_cloud_set_path (true);
	}
	private void on_button_cloud_view_path_clicked (object o, EventArgs args)
	{
		button_cloud_set_path (false);
	}
	private void button_cloud_set_path (bool capture) //capture or view
	{
		FileChooserAction action = FileChooserAction.SelectFolder;

		//mac arm64 crashes on SelectFolder, use Open. The problem in Open is it cannot select a folder that has contents. Only an empty folder
		//so better introduce it manually
		if (UtilAll.IsMacSilicon ())
		{
			// action = FileChooserAction.Open;
			button_cloud_set_path_mac_silicon (capture);
			return;
		}

		Gtk.FileChooserNative fc = new Gtk.FileChooserNative(Catalog.GetString("Select cloud directory"),
				preferences_win,
				action,
				Catalog.GetString("Select"),
				Catalog.GetString("Cancel")
				);

		bool shouldRestart = false; //no restart if path changed, restart if changes from "" to something

		if (fc.Run() == (int)ResponseType.Accept)
		{
			if (capture)
			{
				if (fc.Filename == UtilAll.GetDefaultLocalDataDir (false) ||
						fc.Filename == UtilAll.GetDefaultLocalDataDir (true))
				{
					new DialogMessage (Constants.MessageTypes.WARNING,
							Catalog.GetString ("Error. This path is not valid."));
				} else {
					if (label_cloud_capture_path.Text == "")
						shouldRestart = true;

					button_cloud_set_path_done (true, fc.Filename);
				}
			} else { 	//view
				if (label_cloud_view_path.Text == "")
					shouldRestart = true;

				button_cloud_set_path_done (false, fc.Filename);
				button_cloud_view_databases.Sensitive = (fc.Filename != null);
			}
			buttons_cloud_sensitive ();

			if (shouldRestart)
				restartLabelShow ();
		}

		fc.Hide ();

		//Don't forget to call Destroy() or the FileChooserNative window won't get closed.
		fc.Destroy();
	}

	private void button_cloud_set_path_done (bool capture, string path) //capture or view
	{
		if (capture)
		{
			label_cloud_capture_path.Text = path;
			label_cloud_capture_path.TooltipText = path;
			configAtPrefs.UpdateFieldEnsuringDefaultConfigFile (
					Config.OpEnum.CopyToCloudFullPath.ToString (), path);
		} else {
			label_cloud_view_path.Text = path;
			label_cloud_view_path.TooltipText = path;
			configAtPrefs.UpdateFieldEnsuringDefaultConfigFile (
					Config.OpEnum.ReadFromCloudMainPath.ToString (), path);
		}
	}


	// ---- Silicon ---->
	private void button_cloud_set_path_mac_silicon (bool capture)
	{
		box_silicon_cloud_path_choose.Visible = true;
		box_silicon_cloud_path_capture.Visible = capture;
		box_silicon_cloud_path_view.Visible = ! capture;
		label_silicon_cloud_path_does_not_exists.Visible = false;
	}

	private void on_button_silicon_cloud_capture_path_apply_clicked (object o, EventArgs args)
	{
		if (! Directory.Exists (entry_silicon_cloud_capture_path.Text))
		{
			label_silicon_cloud_path_does_not_exists.Visible = true;
			return;
		} else
			label_silicon_cloud_path_does_not_exists.Visible = false;

		bool shouldRestart = false;
		if (label_cloud_capture_path.Text == "")
			shouldRestart = true;

		button_cloud_set_path_done (true, entry_silicon_cloud_capture_path.Text);
		box_silicon_cloud_path_choose.Visible = false;

		buttons_cloud_sensitive ();

		if (shouldRestart)
			restartLabelShow ();
	}

	private void on_button_silicon_cloud_view_path_apply_clicked (object o, EventArgs args)
	{
		if (! Directory.Exists (entry_silicon_cloud_view_path.Text))
		{
			label_silicon_cloud_path_does_not_exists.Visible = true;
			return;
		} else
			label_silicon_cloud_path_does_not_exists.Visible = false;

		bool shouldRestart = false;
		if (label_cloud_view_path.Text == "")
			shouldRestart = true;

		button_cloud_set_path_done (false, entry_silicon_cloud_view_path.Text);
		button_cloud_view_databases.Sensitive = (entry_silicon_cloud_view_path.Text != "");
		box_silicon_cloud_path_choose.Visible = false;

		buttons_cloud_sensitive ();

		if (shouldRestart)
			restartLabelShow ();
	}
	// <---- Silicon ----


	private void buttons_cloud_sensitive ()
	{
		button_cloud_capture_path.Sensitive = false;
		button_cloud_view_path.Sensitive = false;
		button_cloud_view_databases.Sensitive = false;

		if (radio_cloud_capture.Active)
			button_cloud_capture_path.Sensitive = true;
		else if (radio_cloud_view.Active)
		{
			button_cloud_view_path.Sensitive = true;
			button_cloud_view_databases.Sensitive = (label_cloud_view_path.Text != "");
		}
	}

	private void on_button_cloud_view_databases_clicked (object o, EventArgs args)
	{
		List<DirectoryInfo> dir_l = Util.GetCloudViewDatabases (label_cloud_view_path.Text);
		string str = Catalog.GetString ("Not found any database at path:") + "\n" +
				label_cloud_view_path.Text;
		if (dir_l.Count > 0)
		{
			str = Catalog.GetString (string.Format ("Databases found at {0}:",
				label_cloud_view_path.Text));

			str += "\n";
			foreach (DirectoryInfo dir in dir_l)
				str += "\n- " + dir.Name;
		}

		new DialogMessage(Constants.MessageTypes.INFO, 450, 400, str);
	}

	private void on_button_cloud_schema_zoom_clicked (object o, EventArgs args)
	{
		new DialogImageTest (
				Catalog.GetString("Chronojump cloud schema"),
				Util.GetImagePath(false) + "cloud_schema.png",
				DialogImageTest.ArchiveType.ASSEMBLY,
				"", 600, 306
				);
	}

	// ---- r_user_location ---->

	private void on_radio_r_default_toggled (object o, EventArgs args)
	{
		if (signalsNoFollow)
			return;

		button_r_choose.Sensitive = false;
		button_r_autodetect.Sensitive = false;
		entry_r_user_location.Sensitive = false;
		rUserChanges ("");
	}
	private void on_radio_r_other_toggled (object o, EventArgs args)
	{
		if (signalsNoFollow)
			return;

		button_r_choose.Sensitive = true;
		button_r_autodetect.Sensitive = true;
		entry_r_user_location.Sensitive = true;
		rUserChanges ("");
	}
	private void on_button_r_choose_clicked (object o, EventArgs args)
	{
		signalsNoFollow = true;

		string url = chooseFile (Catalog.GetString ("Please, select R file"));
		rUserChanges (url);

		signalsNoFollow = false;
	}

	private void on_button_r_autodetect_clicked (object o, EventArgs args)
	{
		//TODO
	}

	private void on_entry_r_user_location_changed (object o, EventArgs args)
	{
		if (signalsNoFollow)
			return;

		string url = entry_r_user_location.Text;
		rUserChangesB (url);
	}

	private void rUserChanges (string url)
	{
		rUserChangesA (url);
		rUserChangesB (url);
	}
	private void rUserChangesA (string url)
	{
		// A) changes on preferences gui
		entry_r_user_location.Text = url;
	}
	private void rUserChangesB (string url)
	{
		// B) changes on Config object and file
		if (Config.RUserURLStatic != url)
		{
			Config.RUserURLStatic = url;
			configAtPrefs.UpdateFieldEnsuringDefaultConfigFile (
					Config.OpEnum.RUserURL.ToString (), url);
		}
	}

	// <---- r_user_location

	// ---- rscript_user_location ---->

	private void on_radio_rscript_default_toggled (object o, EventArgs args)
	{
		if (signalsNoFollow)
			return;

		button_rscript_choose.Sensitive = false;
		button_rscript_autodetect.Sensitive = false;
		button_rscript_usr_local_bin.Sensitive = false;
		entry_rscript_user_location.Sensitive = false;
		rscriptUserChanges ("");
	}
	private void on_radio_rscript_other_toggled (object o, EventArgs args)
	{
		if (signalsNoFollow)
			return;

		button_rscript_choose.Sensitive = true;
		button_rscript_autodetect.Sensitive = true;
		button_rscript_usr_local_bin.Sensitive = true;
		entry_rscript_user_location.Sensitive = true;
		rscriptUserChanges ("");
	}
	private void on_button_rscript_choose_clicked (object o, EventArgs args)
	{
		signalsNoFollow = true;

		string url = chooseFile (Catalog.GetString ("Please, select Rscript file"));
		rscriptUserChanges (url);

		signalsNoFollow = false;
	}

	private void on_button_rscript_autodetect_clicked (object o, EventArgs args)
	{
		signalsNoFollow = true;

		string url = ExecuteProcess.WhereInstalled ("Rscript");
		if (url != "")
			rscriptUserChanges (url);

		signalsNoFollow = false;
	}

	private void on_button_rscript_usr_local_bin_clicked (object o, EventArgs args)
	{
		signalsNoFollow = true;
		rscriptUserChanges ("/usr/local/bin/Rscript");
		signalsNoFollow = false;
	}

	private void on_entry_rscript_user_location_changed (object o, EventArgs args)
	{
		if (signalsNoFollow)
			return;

		string url = entry_rscript_user_location.Text;
		rscriptUserChangesB (url);
	}

	private void rscriptUserChanges (string url)
	{
		rscriptUserChangesA (url);
		rscriptUserChangesB (url);
	}
	private void rscriptUserChangesA (string url)
	{
		// A) changes on preferences gui
		entry_rscript_user_location.Text = url;
	}
	private void rscriptUserChangesB (string url)
	{
		// B) changes on Config object and file
		if (Config.RscriptUserURLStatic != url)
		{
			Config.RscriptUserURLStatic = url;
			configAtPrefs.UpdateFieldEnsuringDefaultConfigFile (
					Config.OpEnum.RscriptUserURL.ToString (), url);
		}
	}

	// <---- rscript_user_location

	// ---- python_user_location ---->

	private void on_radio_python_default_toggled (object o, EventArgs args)
	{
		if (signalsNoFollow)
			return;

		button_python_choose.Sensitive = false;
		button_python_autodetect.Sensitive = false;
		entry_python_user_location.Sensitive = false;
		pythonUserChanges ("");
	}
	private void on_radio_python_other_toggled (object o, EventArgs args)
	{
		if (signalsNoFollow)
			return;

		button_python_choose.Sensitive = true;
		button_python_autodetect.Sensitive = true;
		entry_python_user_location.Sensitive = true;
		pythonUserChanges ("");
	}
	private void on_button_python_choose_clicked (object o, EventArgs args)
	{
		signalsNoFollow = true;

		string url = chooseFile (Catalog.GetString ("Please, select Python file"));
		pythonUserChanges (url);

		signalsNoFollow = false;
	}

	private void on_button_python_autodetect_clicked (object o, EventArgs args)
	{
		signalsNoFollow = true;

		string url = ExecuteProcess.WhereInstalled ("python3");
		if (url != "")
			pythonUserChanges (url);

		signalsNoFollow = false;
	}

	private void on_entry_python_user_location_changed (object o, EventArgs args)
	{
		if (signalsNoFollow)
			return;

		string url = entry_python_user_location.Text;
		pythonUserChangesB (url);
	}

	private void pythonUserChanges (string url)
	{
		pythonUserChangesA (url);
		pythonUserChangesB (url);
	}
	private void pythonUserChangesA (string url)
	{
		// A) changes on preferences gui
		entry_python_user_location.Text = url;
	}

	private void pythonUserChangesB (string url)
	{
		// B) changes on Config object and file
		if (Config.PythonUserURLStatic != url)
		{
			Config.PythonUserURLStatic = url;
			configAtPrefs.UpdateFieldEnsuringDefaultConfigFile (
					Config.OpEnum.PythonUserURL.ToString (), url);
		}
	}

	// <---- python_user_location

	private string chooseFile (string text)
	{
		string url = "";
		Gtk.FileChooserNative fc = new Gtk.FileChooserNative(text,
				preferences_win,
				FileChooserAction.Open,
				Catalog.GetString("Select"), Catalog.GetString("Cancel")
				);
		if (fc.Run() == (int)ResponseType.Accept) 
		{
			url = fc.Filename; //include path?
		}
		//Don't forget to call Destroy() or the FileChooserNative window won't get closed.
		fc.Destroy();

		return url;
	}

	private void on_checkbutton_mute_logs_clicked (object o, EventArgs args)
	{
		/* disabled. Only false since 2.3.0-2

		// B) changes on preferences object and SqlitePreferences
		if (preferences.muteLogs != PWBox.checkbutton_mute_logs.Active) {
			SqlitePreferences.Update ("muteLogs", PWBox.checkbutton_mute_logs.Active.ToString(), false);
			preferences.muteLogs = PWBox.checkbutton_mute_logs.Active;
		}
		*/
	}

	// view more tabs ---->

	private void on_button_view_more_tabs_clicked (object o, EventArgs args)
	{
		PWBox.notebook_top.CurrentPage = Convert.ToInt32(notebook_top_pages.SELECTTABS);
		hbox_buttons_bottom.Sensitive = false;
	}
	private void on_button_view_more_tabs_close_clicked (object o, EventArgs args)
	{
		PWBox.notebook_top.CurrentPage = Convert.ToInt32(notebook_top_pages.PREFERENCES);
		hbox_buttons_bottom.Sensitive = true;
	}

	private void on_check_view_jumps_clicked (object o,EventArgs args)
	{
		tabShowHide(check_view_jumps.Active, JUMPSPAGE);
	}
	private void on_check_view_runs_clicked (object o,EventArgs args)
	{
		tabShowHide(check_view_runs.Active, RUNSPAGE);
	}
	private void on_check_view_weights_inertial_clicked (object o,EventArgs args)
	{
		tabShowHide(check_view_weights_inertial.Active, WEIGHTSINERTIALPAGE);
	}
	private void on_check_view_isometric_elastic_clicked (object o,EventArgs args)
	{
		tabShowHide(check_view_isometric_elastic.Active, ISOMETRICELASTICPAGE);
	}

	private void tabShowHide (bool active, int page)
	{
		if(active) {
			PWBox.notebook.GetNthPage(page).Show();
			PWBox.notebook.CurrentPage = page;

		} else
			PWBox.notebook.GetNthPage(page).Hide();
	}

	// <---- endo of view more tabs

	// help ---->

	private enum helpTypes { NORMAL, STIFFNESS }
	//does not use markup on textview
	private void showHelp (string title, helpTypes helpType, string message)
	{
		preferences_win.Title = Catalog.GetString("Preferences") + " / " + Catalog.GetString("Help:") + " " + title;
		PWBox.notebook_top.CurrentPage = Convert.ToInt32(notebook_top_pages.HELP);
		hbox_buttons_bottom.Sensitive = false;

		hbox_stiffness_formula.Visible = (helpType == helpTypes.STIFFNESS);

		textview_help_message.Buffer.Text = message;
	}

	private void on_button_help_close_clicked (object o, EventArgs args)
	{
		preferences_win.Title = Catalog.GetString("Preferences");
		PWBox.notebook_top.CurrentPage = Convert.ToInt32(notebook_top_pages.PREFERENCES);
		hbox_buttons_bottom.Sensitive = true;
	}


	// <---- end of help


	RGBA colorDrawingArea;
	private void paintDrawingArea (RGBA color)
	{
		//UtilGtk.PaintColorDrawingArea (drawingarea_background_color, color);
		colorDrawingArea = color;
		drawingarea_background_color.QueueDraw ();
	}

	private void paintBg (RGBA color)
	{
		if(preferences.colorBackgroundOsColor)
			return;

		//window
		UtilGtk.WindowColor (preferences_win, color);

		//notebook_top
		notebook_top.Name = "bgCss";
		UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundShiftedIsDark, notebook_top);

		//notebook
		notebook.Name = "bgCss";
		UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundShiftedIsDark, notebook);

		//notebook_races
		notebook_races.Name = "shiftedCss";
		UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundShiftedIsDark, notebook_races);

		//notebook_races_double_contacts
		notebook_races_double_contacts.Name = "shiftedCss";
		UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundShiftedIsDark, notebook_races_double_contacts);

		//notebook_force_sensor
		notebook_force_sensor.Name = "shiftedCss";
		UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundShiftedIsDark, notebook_force_sensor);

		//notebook_encoder
		notebook_encoder.Name = "shiftedCss";
		UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundShiftedIsDark, notebook_encoder);

		//notebook_multimedia
		notebook_multimedia.Name = "shiftedCss";
		UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundShiftedIsDark, notebook_multimedia);

		//notebook_advanced
		notebook_advanced.Name = "shiftedCss";
		UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundShiftedIsDark, notebook_advanced);

		UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, label_restart);

		//send signal to ApplyCSS
		FakeButtonColorsChanged.Click ();
	}

	private void on_drawingarea_background_color_draw (object o, Gtk.DrawnArgs args)
	{
		DrawingArea da = (DrawingArea) o;
		Cairo.Context cr = args.Cr;

		CairoUtil.PaintDrawingArea (da, cr, colorDrawingArea);
	}

	private void paintColorChronojump ()
	{
		drawingarea_background_color_chronojump_blue.QueueDraw ();
	}
	private void on_drawingarea_background_color_chronojump_blue_draw (object o, Gtk.DrawnArgs args)
	{
		DrawingArea da = (DrawingArea) o;
		Cairo.Context cr = args.Cr;

		/*
		LogB.Information ("going to paint in BLUE_CHRONOJUMP");
		LogB.Information (UtilGtk.GetRGBAs (UtilGtk.Colors.BLUE_CHRONOJUMP).Red);
		LogB.Information (UtilGtk.GetRGBAs (UtilGtk.Colors.BLUE_CHRONOJUMP).Green);
		LogB.Information (UtilGtk.GetRGBAs (UtilGtk.Colors.BLUE_CHRONOJUMP).Blue);
		*/
		CairoUtil.PaintDrawingArea (da, cr, UtilGtk.GetRGBA (UtilGtk.Colors.BLUE_CHRONOJUMP));
	}


	/* ---------------------------------------------------------
	 * ----------------  Jumps. Info on power and stiffness -----------
	 *  --------------------------------------------------------
	 */

	//both valid for jumps and jumps_rj
	private void on_button_jumps_power_help_clicked (object o, EventArgs args) {
		showHelp(Catalog.GetString("Power"), helpTypes.NORMAL, Constants.HelpPowerStr());
	}
	private void on_button_jumps_stiffness_help_clicked (object o, EventArgs args) {
		showHelp(Catalog.GetString("Stiffness"), helpTypes.STIFFNESS, Constants.HelpStiffnessStr());
	}

	private void on_button_encoder_capture_cut_by_triggers_help_clicked (object o, EventArgs args)
	{
		showHelp("Chronojump triggers",
				helpTypes.NORMAL,
				Catalog.GetString("If active, repetitions will be cut from set using triggers.") + " " +
				Catalog.GetString("Trigger signal will be produced by a button connected to the Chronopic.") + "\n\n" +
				Catalog.GetString("This will be only used on gravitatory mode, concentric contraction.") + "\n\n" +
				Catalog.GetString("If \"Cut by triggers\" is inactive, repetitions will be cut automatically (default behaviour),") + " " +
				Catalog.GetString("but pressing trigger button while capturing will plot vertical lines during analyze instant graphs.") + "\n\n" +
				Catalog.GetString("Encoder Chronopics have trigger functionality since 2017.") + " " +
			        Catalog.GetString("You can check if your encoder Chronopic accepts triggers pressing test button.") + " " +
				Catalog.GetString("Your Chronopic is ready for triggers if the green light at the side of Chronopic test button changes it's state on pressing this button.") + " " +
				Catalog.GetString("At Chronojump website there's a hand push button for using triggers with ease.")
				);
		/*
		 * not on ecc-con because we cannot guaranteee that there will be an ecc and con phase,
		 * and then R findECPhases() will fail
		 */
	}

	/*
	 * end of triggers stuff
	 */


	private void createComboCamera(string current, string pixelFormat, string resolution, string framerate)
	{
		// 1) videoDevice

		combo_camera = new ComboBoxText ();

		/*
		 * declare both because there is a return just here and if they are undeclred the method:
		 * on_button_close_clicked () will fail
		 */
		combo_camera_pixel_format = new ComboBoxText ();
		combo_camera_resolution = new ComboBoxText ();
		combo_camera_framerate = new ComboBoxText ();

		if(wd_list.Count() == 0) {
			//devices = Util.StringToStringArray(Constants.CameraNotFound);
			label_camera_error.Text = wd_list.Error;
			label_camera_error.Visible = true;
			current = "";

			hbox_camera_resolution_framerate.Visible = false;
			check_camera_advanced.Visible = false;

			return;
		}

		//UtilGtk.ComboUpdate(combo_camera, wd_list.GetCodes());
		UtilGtk.ComboUpdate(combo_camera, wd_list.GetFullnames());
		hbox_combo_camera.PackStart(combo_camera, true, true, 0);
		combo_camera.Changed += new EventHandler (on_combo_camera_changed);
		hbox_combo_camera.ShowAll();

		//if(current >= devices.Count)
		//	current = 0;
		
		combo_camera.Active = UtilGtk.ComboMakeActive(combo_camera, wd_list.GetFullnameOfCode(current));

		// 2) pixel_format

		List<string> pixelFormats = new List<string>();
		if(pixelFormat != "")
		{
			pixelFormats.Add(pixelFormat);
			UtilGtk.ComboUpdate(combo_camera_pixel_format, pixelFormats);
			combo_camera_pixel_format.Active = 0;

			label_camera_pixel_format_current.Text = pixelFormat;
			label_camera_pixel_format_current.Visible = true;
			button_video_preview.Sensitive = true;
		}

		hbox_combo_camera_pixel_format.PackStart(combo_camera_pixel_format, true, true, 0);
		//not shown because label is shown
		//hbox_combo_camera_pixel_format.ShowAll();
		//hbox_combo_camera_pixel_format.Sensitive = false;
		combo_camera_pixel_format.Changed += new EventHandler (on_combo_camera_pixel_format_changed);

		// 3) resolution

		List<string> resolutions = new List<string>();
		/*
		 * do not have this default values, just write the option on sqlite (if any)
		 *
		resolutions.Add("320x240");
		resolutions.Add("640x480");
		resolutions.Add("1280x720");
		resolutions.Add(Catalog.GetString("Custom")); //in SQL will be stored the values not "Custom" text
		UtilGtk.ComboUpdate(combo_camera_resolution, resolutions);

		if(resolution == "") //(first time using this) give a value
			resolution = "640x480";


		bool found = false;
		foreach(string str in resolutions)
			if(str == resolution)
				found = true;

		if(found)
			combo_camera_resolution.Active = UtilGtk.ComboMakeActive(combo_camera_resolution, resolution);
		else {
			combo_camera_resolution.Active = UtilGtk.ComboMakeActive(combo_camera_resolution, Catalog.GetString("Custom"));
			string [] strFull = resolution.Split('x');
			if(strFull.Length == 2) {
				spin_camera_resolution_custom_width.Value = Convert.ToInt32(strFull[0]);
				spin_camera_resolution_custom_height.Value = Convert.ToInt32(strFull[1]);
			}
			hbox_camera_resolution_custom.Visible = true;
		}
		*/
		if(resolution != "")
		{
			resolutions.Add(resolution);
			UtilGtk.ComboUpdate(combo_camera_resolution, resolutions);
			combo_camera_resolution.Active = 0;

			label_camera_resolution_current.Text = resolution;
			label_camera_resolution_current.Visible = true;
		}

		hbox_combo_camera_resolution.PackStart(combo_camera_resolution, true, true, 0);
		//not shown because label is shown
		//hbox_combo_camera_resolution.ShowAll();
		//hbox_combo_camera_resolution.Sensitive = false;
		combo_camera_resolution.Changed += new EventHandler (on_combo_camera_resolution_changed);

		// 4) framerate

		combo_camera_framerate = new ComboBoxText ();
		List<string> framerates = new List<string>();
		/*
		 * do not have this default values, just write the option on sqlite (if any)
		 *
		framerates.Add("30");
		framerates.Add("60");
		framerates.Add(Catalog.GetString("Custom")); //in SQL will be stored the values not "Custom" text
		UtilGtk.ComboUpdate(combo_camera_framerate, framerates);

		if(framerate == "") //(first time using this) give a value
			framerate = "30";

		found = false;
		foreach(string str in framerates)
			if(str == framerate)
				found = true;

		if(found)
			combo_camera_framerate.Active = UtilGtk.ComboMakeActive(combo_camera_framerate, framerate);
		else {
			combo_camera_framerate.Active = UtilGtk.ComboMakeActive(combo_camera_framerate, Catalog.GetString("Custom"));
			string [] strFull = framerate.Split(new char[] {'.'});

			if(strFull.Length == 1)
			{
				spin_camera_framerate_custom.Value = Convert.ToInt32(framerate);
			}
			else if(strFull.Length == 2)
			{
				spin_camera_framerate_custom.Value = Convert.ToInt32(strFull[0]);
				entry_camera_framerate_custom_decimals.Text = strFull[1];
			}
			hbox_camera_framerate_custom.Visible = true;
		}
		*/
		if(framerate != "")
		{
			framerates.Add(framerate);
			UtilGtk.ComboUpdate(combo_camera_framerate, framerates);
			combo_camera_framerate.Active = 0;

			label_camera_framerate_current.Text = framerate;
			label_camera_framerate_current.Visible = true;
		}

		hbox_combo_camera_framerate.PackStart(combo_camera_framerate, true, true, 0);
		//not shown because label is shown
		//hbox_combo_camera_framerate.ShowAll();
		//hbox_combo_camera_framerate.Sensitive = false;
		combo_camera_framerate.Changed += new EventHandler (on_combo_camera_framerate_changed);
	}



	private void on_check_camera_advanced_toggled (object o, EventArgs args)
	{
		frame_camera_advanced.Visible = check_camera_advanced.Active;
	}


	// ---- multimedia stuff

	private void on_button_test_sound_clicked (object o, EventArgs args)
	{
		label_test_sound_result.Text = "";
		Util.SoundCodes sc;
		Util.TestSound = true;

		if(radio_gstreamer_0_1.Active)
			sc = Util.PlaySound(Constants.SoundTypes.GOOD, true, Preferences.GstreamerTypes.GST_0_1);
		else if(radio_gstreamer_1_0.Active)
			sc = Util.PlaySound(Constants.SoundTypes.GOOD, true, Preferences.GstreamerTypes.GST_1_0);
		else if(radio_ffplay.Active)
			sc = Util.PlaySound(Constants.SoundTypes.GOOD, true, Preferences.GstreamerTypes.FFPLAY);
		else
			sc = Util.PlaySound(Constants.SoundTypes.GOOD, true, Preferences.GstreamerTypes.SYSTEMSOUNDS);

		if(sc == Util.SoundCodes.OK)
			label_test_sound_result.Text = Catalog.GetString("Sound working");
		else
			label_test_sound_result.Text = Catalog.GetString("Sound not working");

		Util.TestSound = false;
	}

	private void on_button_check_video_devices_clicked (object o, EventArgs args)
	{
		try {
			wd_list = UtilMultimedia.GetVideoDevices();
		} catch {
			new DialogMessage (Constants.MessageTypes.WARNING,
					Catalog.GetString ("Error. Could not check video devices."));
			return;
		}

		notebook_multimedia_video.CurrentPage = 1;
		PWBox.createComboCamera(preferences.videoDevice,
				preferences.videoDevicePixelFormat, preferences.videoDeviceResolution, preferences.videoDeviceFramerate);
	}

	//for mac and maybe windows, because in Linux it founds a default mode and it works
	private void on_button_video_get_supported_modes_clicked (object o, EventArgs args)
	{
		string cameraCode = wd_list.GetCodeOfFullname(UtilGtk.ComboGetActive(combo_camera));
		if(cameraCode == "")
			return;

		if(operatingSystem == UtilAll.OperatingSystems.LINUX)
		{
			string number = "0";

			//allows to use two-digit codes
			Match match = Regex.Match(cameraCode, @"/dev/video/(\d+)");
			if(match.Groups.Count == 2)
				number = match.Value;

			wfsm = new WebcamFfmpegSupportedModesLinux(number);
		}
		else if(operatingSystem == UtilAll.OperatingSystems.WINDOWS)
		{
			//wfsm = new WebcamFfmpegSupportedModesWindows(cameraCode);
			//last ffmpeg version seems to work better with name instead of code
			wfsm = new WebcamFfmpegSupportedModesWindows (UtilGtk.ComboGetActive (combo_camera));
		} else
			wfsm = new WebcamFfmpegSupportedModesMac(cameraCode);

		wfsm.GetModes();

		if(wfsm.ErrorStr != "")
		{
			/*
			new DialogMessage("Chronojump - Modes of this webcam",
					Constants.MessageTypes.WARNING, wfsm.ErrorStr);
			*/
			label_camera_error.Text = wfsm.ErrorStr;
			label_camera_error.Visible = true;

			return;
		}

		/*
		//display the result (if any)
		if(wfsm.ModesStr != "")
			new DialogMessage("Chronojump - Modes of this webcam",
					Constants.MessageTypes.INFO, wfsm.ModesStr, true); //showScrolledWinBar
		*/

		bool fillCombos = true;
		if(fillCombos)
		{
			string currentPixelFormat = getSelectedPixelFormat();
			UtilGtk.ComboUpdate(combo_camera_pixel_format, wfsm.GetPixelFormats());
			combo_camera_pixel_format.Active = UtilGtk.ComboMakeActive(combo_camera_pixel_format, currentPixelFormat);
			button_video_preview.Sensitive = true;

			/*
			//not shown because label is shown
			hbox_combo_camera_pixel_format.Sensitive = true;
			hbox_combo_camera_resolution.Sensitive = true;
			hbox_combo_camera_framerate.Sensitive = true;
			*/
			label_camera_pixel_format_current.Visible = false;
			label_camera_resolution_current.Visible = false;
			label_camera_framerate_current.Visible = false;

			hbox_combo_camera_pixel_format.ShowAll();
			hbox_combo_camera_resolution.ShowAll();
			hbox_combo_camera_framerate.ShowAll();
		}
	}

	private void on_button_video_preview_clicked (object o, EventArgs args)
	{
		label_video_preview_error.Visible = false;

		//this allows us to update the previous label, if not we have to end camera play
		GLib.Timeout.Add(100, new GLib.TimeoutHandler(button_video_preview_do));
	}
	private bool button_video_preview_do ()
	{
		string cameraCode = wd_list.GetCodeOfFullname(UtilGtk.ComboGetActive(combo_camera));
		if(cameraCode == "")
			return false; //do not call again

		Webcam webcamPlay = new WebcamFfmpeg (Webcam.Action.PLAYPREVIEW, UtilAll.GetOSEnum(), cameraCode,
				getSelectedPixelFormat(), getSelectedResolution(), getSelectedFramerate());

		Webcam.Result result = webcamPlay.PlayPreviewNoBackground ();
		if(! result.success) {
			label_video_preview_error.Text = result.error;
			label_video_preview_error.Visible = true;
		}

		return false; //do not call again
	}

	private string getSelectedPixelFormat()
	{
		return UtilGtk.ComboGetActive (combo_camera_pixel_format);
	}
	private string getSelectedResolution()
	{
		string selected = UtilGtk.ComboGetActive(combo_camera_resolution);
		if(selected == Catalog.GetString("Custom"))
			selected = string.Format("{0}x{1}", spin_camera_resolution_custom_width.Value, spin_camera_resolution_custom_height.Value);

		return selected;
	}
	private string getSelectedFramerate()
	{
		string selected = UtilGtk.ComboGetActive(combo_camera_framerate);
		if(selected == Catalog.GetString("Custom"))
		{
			string decStr = entry_camera_framerate_custom_decimals.Text;
			if(decStr != "0" && Util.IsNumber(decStr, false))
				selected = string.Format("{0}.{1}", spin_camera_framerate_custom.Value, decStr); //decimal in ffmpeg has to be '.'
			else
				selected = string.Format("{0}", spin_camera_framerate_custom.Value);
		}

		LogB.Information("selected framerate: " + selected);
		return selected;
	}

	private void on_button_video_check_ffmpeg_ffplay_running_clicked(object o, EventArgs args)
	{
		label_video_check_ffmpeg_running.Text = Catalog.GetString("Not running");
		label_video_check_ffplay_running.Text = Catalog.GetString("Not running");
		button_video_ffmpeg_kill.Visible = false;
		button_video_ffplay_kill.Visible = false;
		label_camera_check_running.Text = "";

		//bool runningFfmpeg = false;
		//bool runningFfplay = false;

		if(ExecuteProcess.IsRunning3 (-1, WebcamFfmpeg.GetExecutableCapture(operatingSystem)))
		{
			//runningFfmpeg = true;
			label_video_check_ffmpeg_running.Text = Catalog.GetString("Running");
			button_video_ffmpeg_kill.Visible = true;
		}

		if(ExecuteProcess.IsRunning3 (-1, WebcamFfmpeg.GetExecutablePlay(operatingSystem)))
		{
			//runningFfplay = true;
			label_video_check_ffplay_running.Text = Catalog.GetString("Running");
			button_video_ffplay_kill.Visible = true;
		}

		grid_video_advanced_actions.Visible = true;
	}

	private void on_button_video_ffmpeg_kill_clicked (object o, EventArgs args)
	{
		if(ExecuteProcess.KillExternalProcess (WebcamFfmpeg.GetExecutableCapture(operatingSystem)))
		{
			label_camera_check_running.Text = "Killed camera process";
			label_video_check_ffmpeg_running.Text = Catalog.GetString("Not running");
			button_video_ffmpeg_kill.Visible = false;
		}
		else
			label_camera_check_running.Text = "Cannot kill camera process";
	}
	private void on_button_video_ffplay_kill_clicked (object o, EventArgs args)
	{
		if(ExecuteProcess.KillExternalProcess (WebcamFfmpeg.GetExecutablePlay(operatingSystem)))
		{
			label_camera_check_running.Text = "Killed play process";
			label_video_check_ffplay_running.Text = Catalog.GetString("Not running");
			button_video_ffplay_kill.Visible = false;
		}
		else
			label_camera_check_running.Text = "Cannot kill play process";
	}

	// ---- end of multimedia stuff

	// ---- Language stuff

	private void createComboLanguage() {
		
		combo_language = new ComboBoxText ();
		fillLanguages();

		hbox_combo_language.PackStart(combo_language, false, false, 0);
		hbox_combo_language.ShowAll();
	}

	private void createComboDecimals ()
	{
		combo_decimals = UtilGtk.CreateComboBoxText (
				box_combo_decimals,
				new List<string> { "1", "2", "3" },
				preferences.digitsNumber.ToString () );

		combo_decimals.Changed += new EventHandler (on_combo_decimals_changed);
	}

	//from Longomatch ;)
	//(C) Andoni Morales Alastruey
	void fillLanguages () {
		int index = 0, active = 0;

		langsStore = new ListStore(typeof(string), typeof(CultureInfo));

		foreach (CultureInfo lang in UtilLanguage.Languages) {
			langsStore.AppendValues(lang.NativeName, lang);
			if (preferences.language != "" && lang.Name == preferences.language)
				active = index;
			index ++;
		}
		combo_language.Model = langsStore;
		combo_language.Active = active;
		combo_language.Changed += combo_language_changed;
	}

	string getSelectedLanguage()
	{
		TreeIter iter;
		CultureInfo info;

		combo_language.GetActiveIter (out iter);
		info = (CultureInfo) langsStore.GetValue (iter, 1);
		if (info == null) {
			return "";
		} else {
			return info.Name;
		}
	}

	// ---- end of Language stuff

			
	private void on_button_run_speed_start_help_clicked (object o, EventArgs args)
	{
		showHelp(Catalog.GetString("Race measurement"), helpTypes.NORMAL,
				Catalog.GetString(
					"\"Speed start\" means when athlete does not start with \"contact\" on the " +
					"first platform or photocell.\n" +
					"It starts before and arrives there with some speed.") /* +
				"\n\n" +
				Catalog.GetString("Chronojump race reaction time device allows to record reaction time and race time.") +
				"\n -" +
				Catalog.GetString("Reaction time is displayed on Description column.") +
				"\n -" +
				Catalog.GetString("If first option is chosen, race time includes reaction time.")
				*/
				);
	}

	void on_button_close_clicked (object o, EventArgs args)
	{
		/*
		if( preferences.showAngle != PWBox.checkbutton_angle.Active ) {
			SqlitePreferences.Update("showAngle", PWBox.checkbutton_angle.Active.ToString(), false);
			preferences.showAngle = PWBox.checkbutton_angle.Active;
		}
		*/

		if (bluetoothReading)
			bluetooth_stop ();

		PWBox.preferences_win.Hide();
		PWBox = null;
	}

	void on_preferences_delete_event (object o, DeleteEventArgs args)
	{
		LogB.Information("calling on_delete_event at preferences");
/*
		//do not hide/exit if copyiing
		if (thread != null && thread.IsAlive)
			args.RetVal = true;
		else {
*/
			PWBox.preferences_win.Hide();
			PWBox = null;
//		}
	}

	/*
	 * TODO: problem is database stored is a chronojump.db or a folder (if images and videos were saved).
	 * FileChooserAction only lets you use one type
	 * In the future backup db as tgz or similar

	 void on_button_db_restore_clicked (object o, EventArgs args)
	 {
		FileChooserAction action = FileChooserAction.SelectFolder;
		//mac arm64 crashes on SelectFolder, use Open. The problem in Open is it cannot select a folder that has contents. Only an empty folder
		if (UtilAll.IsMacSilicon ())
			action = FileChooserAction.Open;

		fc = new Gtk.FileChooserNative(Catalog.GetString("Restore database from:"),
			preferences_win,
			action,
			Catalog.GetString("Restore"), Catalog.GetString("Cancel")
		);

		ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString("Are you sure you want to restore?"));
		confirmWin.Button_accept.Clicked += new EventHandler(on_overwrite_file_accepted);
	 }
	 */

	
	void on_button_logs_folder_open_clicked (object o, EventArgs args)
	{
		string dir = UtilAll.GetLogsDir("");
		LogB.Information(dir);
		
		if( ! new System.IO.DirectoryInfo(dir).Exists) {
			try {
				Directory.CreateDirectory (dir);
			} catch {
				showHelp(Catalog.GetString("Error"), helpTypes.NORMAL,
						Catalog.GetString("Cannot create directory.") + "\n\n" + dir);
				return;
			}
		}

		if(! Util.OpenURL (dir))
			showHelp(Catalog.GetString("Error"), helpTypes.NORMAL,
					Constants.DirectoryCannotOpenStr() + "\n\n" + dir);
	}
	
	void on_button_tmp_folder_open_clicked (object o, EventArgs args)
	{
		string dir = UtilAll.GetTempDir(); //potser cal una arrobar abans (a windows)

		if( ! new System.IO.DirectoryInfo(dir).Exists)
		{
			LogB.Warning(dir);
			return;
		}

		if(! Util.OpenURL (dir))
			showHelp(Catalog.GetString("Error"), helpTypes.NORMAL,
					Constants.DirectoryCannotOpenStr() + "\n\n" + dir);
	}

	void on_button_import_configuration_clicked (object o, EventArgs args)
	{
		Gtk.FileChooserNative fc = new Gtk.FileChooserNative(Catalog.GetString("Import configuration file"),
				preferences_win,
				FileChooserAction.Open,
				Catalog.GetString("Import"), Catalog.GetString("Cancel")
				);
		
		fc.Filter = new FileFilter();
		//it can handle future archives like: chronojump_config_SOME_VENDOR.txt
		//and it will be copied to chronojump_config.txt
		fc.Filter.AddPattern("chronojump_config*.txt");
	
		bool success = false;	
		if (fc.Run() == (int)ResponseType.Accept) 
		{
			try {
				File.Copy(fc.Filename, Util.GetConfigFileName(false), true);
				LogB.Information("Imported configuration");

				//will launch configInit() from gui/chronojump.cs
				FakeButtonConfigurationImported.Click();

				success = true;
			} catch {
				LogB.Warning("Catched! Configuration cannot be imported");
				showHelp(Catalog.GetString("Error"), helpTypes.NORMAL,
						Catalog.GetString("Error importing data."));
			}
		}
		//Don't forget to call Destroy() or the FileChooserNative window won't get closed.
		fc.Destroy();

		if(success)
			showHelp("", helpTypes.NORMAL, Catalog.GetString("Successfully imported."));
	}

	//encoder
	private void on_button_inactivity_help_clicked (object o, EventArgs args)
	{
		showHelp(Catalog.GetString("End capture by inactivity"), helpTypes.NORMAL,
				Catalog.GetString("If a repetition has been found, test will end at selected inactivity seconds.") + "\n\n" +
				Catalog.GetString("If a repetition has not been found, test will end at selected inactivity seconds (x2).") + "\n" +
				Catalog.GetString("This will let the person to have more time to start movement.") + "\n\n" +
				Catalog.GetString("On inertial, to avoid never ending capture because cone is slowly moving at the end, this criteria is added:") + "\n" +
				Catalog.GetString("If passed the double of configured inactivity seconds since last phase, capture will end.")
				);
	}

	private void on_button_encoder_inertial_analyze_eq_mass_help_clicked (object o, EventArgs args)
	{
		new DialogImageTest (
				Catalog.GetString("Equivalent mass"),
				Util.GetImagePath(false) + "equivalentMass.png",
				DialogImageTest.ArchiveType.ASSEMBLY,
				Catalog.GetString("The equivalent mass is a measure of the resistance of a body to change its linear or rotary velocity.") + "\n" +
				Catalog.GetString("From the point of view of a person pulling from a rope, the situation in the image is equivalent to a rotary inertial machine.") + "\n" +
				Catalog.GetString("The equivalent mass takes in account the different configurations of a inertial machine (diameters, inertia momentum, extra weights and force multipliers).") + "\n" +
				Catalog.GetString("This way it is possible to compare the resistance of diferent configurations on diferent machines."), -1, -1
				);
	}

	// ---- start SQL stress tests ---->

	private void on_SQL_stress_test_safe_short_clicked (object o, EventArgs args) {
		LogB.Information("start safe short stress test ---->");
		sql_stress_test(1000);
	}
	private void on_SQL_stress_test_safe_long_clicked (object o, EventArgs args) {
		LogB.Information("start safe long stress test ---->");
		sql_stress_test(4000);
	}
	private void on_SQL_stress_test_not_safe_short_clicked (object o, EventArgs args) {
		LogB.Information("start not safe short stress test ---->");
		Sqlite.SafeClose = false;
		sql_stress_test(1000);
		Sqlite.SafeClose = true;
	}
	private void on_SQL_stress_test_not_safe_long_clicked (object o, EventArgs args) {
		LogB.Information("start not safe long stress test ---->");
		Sqlite.SafeClose = false;
		sql_stress_test(4000);
		Sqlite.SafeClose = true;
	}
	private void sql_stress_test (int times) {
		Stopwatch sw = new Stopwatch();

		sw.Start();

		//trying if new way of Sqlite.Close disposing dbcmd fixes problems when multiple open / close connection
		for(int i=0 ; i < times; i++) {
			LogB.Debug (" i=" + i.ToString());
			LogB.Debug(SqlitePreferences.Select("databaseVersion"));
		}
		sw.Stop();

		string message = "Success!" + 
			" Done " + times + " times." + 
			" Elapsed " + sw.ElapsedMilliseconds + " ms";
		LogB.Information(message);
	
		label_advanced_feedback.Text = message;
	}

	private void on_sql_test_crash_mac_silicon_clicked (object o, EventArgs args)
	{
		label_advanced_feedback.Text = "";
		if (Sqlite.TestCrashOnMacARM ())
			label_advanced_feedback.Text = "SQL tests MacARM Ok!";
	}

	// <---- end SQL stress tests ----

	// ---- send log ---->

	string emailStoredForSendLog;
	//note this method is a ripoff of the method on src/gui/sendLogAndPoll.cs
	private void on_button_send_log_clicked (object o, EventArgs args)
	{
		button_send_log.Sensitive = false;
		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.Text = "";
		textview_send_log_message.Buffer = tb;

		//this allows us to update the textbuffer if button is clicked again
		GLib.Timeout.Add (200, new GLib.TimeoutHandler (button_send_log_do));
	}
	private bool button_send_log_do ()
	{
		string email = entry_send_log.Text.ToString();
		//email can be validated with Util.IsValidEmail(string)
		//or other methods, but maybe there's no need of complexity now

		//1st save email on sqlite
		if(email != null && email != "" && email != "0" && email != emailStoredForSendLog)
			SqlitePreferences.Update("email", email, false);

		string comments = "";
		/*
		//2nd add language as comments
		string language = get_send_log_language();
		SqlitePreferences.Update("crashLogLanguage", language, false);
		comments = "Answer in: " + language + "\n";
		*/

		//3rd if there are comments, add them at the beginning of the file
		comments += textview_send_log_comments.Buffer.Text;

		//4th send Json
		Json js = new Json();
		bool success = js.PostCrashLog (radio_send_log_current.Active, email, comments);

		if(success) {
			image_send_log_yes.Show();
			image_send_log_no.Hide();
			LogB.Information(js.ResultMessage);
		} else {
			image_send_log_yes.Hide();
			image_send_log_no.Show();
			LogB.Error(js.ResultMessage);
		}

		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.Text = js.ResultMessage;
		textview_send_log_message.Buffer = tb;
		button_send_log.Sensitive = true;

		return false; //do not call again
	}

	// <---- end of sendLog ----

	private void on_debug_mode_clicked (object o, EventArgs args)
	{
		//will be managed from gui/chronojump.cs
		button_debug_mode.Sensitive = false;
		FakeButtonDebugModeStart.Click();
	}
	public void DebugActivated() {
		label_advanced_feedback.Text = "Debug mode on while Chronojump is running.";
	}

	private void on_toggle_gc_collect_on_close_toggled(object o, EventArgs args)
	{
		if(toggle_gc_collect_on_close.Active) {
			Sqlite.GCCollect = true;
			new DialogMessage(Constants.MessageTypes.INFO, "GCCollect: ACTIVE!");
		} else {
			Sqlite.GCCollect = false;
			new DialogMessage(Constants.MessageTypes.INFO, "GCCollect: UNACTIVE! (default)");
		}
	}

	private void on_toggle_never_close_toggled(object o, EventArgs args)
	{
		if(toggle_never_close.Active) {
			Sqlite.NeverCloseDB = true;
			new DialogMessage(Constants.MessageTypes.INFO, "Never close: ACTIVE!");
		} else {
			Sqlite.NeverCloseDB = false;
			new DialogMessage(Constants.MessageTypes.INFO, "Never close: UNACTIVE! (default)");
		}
	}

	private void on_button_delete_devices_clicked (object o, EventArgs args)
	{
		SqliteChronopicRegister.DeleteAll (false);
		label_advanced_feedback.Text = Catalog.GetString ("Deleted stored devices.");
		FakeButtonDeleteDevices.Click ();
	}


	/* ---------------------
	 * bluetooth start ---->
	 * -------------------*/

	private bool bluetoothReading = false;
	//use the string to not have crash by manipulating the TextBuffer outside the pulse thread
	static string tbBluetoothText = "";
	static bool needToUpdateTextViewBluetooth;
	TextBuffer tbBluetooth = new TextBuffer (new TextTagTable());
	static Thread threadBluetooth;

	private void on_button_bluetooth_start_clicked (object o, EventArgs args)
	{
		//TODO:
		/*
		Bluetooth bl = new Bluetooth ();
		bl.TestInit ();
		*/

		if(! File.Exists (entry_bluetooth_url.Text))
		{
			LogB.Information ("Error. Bluetooth start file not found: " + entry_bluetooth_url.Text);
			tbBluetooth.Text = Catalog.GetString ("Error. File not found.");
			textview_bluetooth.Buffer = tbBluetooth;
			bluetoothSensitiveDoing (false);

			return;
		}

		bluetoothSensitiveDoing (true);

		bluetoothReading = true;
		textview_bluetooth.Name = "fontSize9";
		tbBluetoothText = "";
		bluetooth_textview_update ("\nStarting communication... ");

		threadBluetooth = new Thread (new ThreadStart (bluetoothDo));
		GLib.Idle.Add (new GLib.IdleHandler (pulseBluetooth));

		LogB.ThreadStart();
		threadBluetooth.Start();
	}

	private void bluetoothSensitiveDoing (bool doing)
	{
		entry_bluetooth_url.Sensitive = ! doing;
		button_bluetooth_start.Sensitive = ! doing;
		button_bluetooth_end.Sensitive = doing;
	}

	private void bluetoothDo ()
	{
		//Start BluetoothLE service
		BluetoothLE.SetProcess (entry_bluetooth_url.Text);
		BluetoothLE.Start ();
	}

	// by GTK thread
	private bool pulseBluetooth ()
	{
		if (needToUpdateTextViewBluetooth)
		{
			tbBluetooth.Text = tbBluetoothText;
			textview_bluetooth.Buffer = tbBluetooth;
			UtilGtk.TextViewScrollToEnd (textview_bluetooth);
			needToUpdateTextViewBluetooth = false;
		}
		if (! bluetoothReading)
			return false;

		//LogB.Debug (" \npulseBluetooth:" + threadBluetooth.ThreadState.ToString());
		Thread.Sleep (50);
		return true;
	}

	private void on_button_bluetooth_end_clicked (object o, EventArgs args)
	{
		if (bluetoothReading)
		{
			bluetooth_stop ();
			bluetoothSensitiveDoing (false);
		}
	}

	private void bluetooth_stop ()
	{
		//Stop the BluetoothLE service if it was started
		BluetoothLE.Stop();
		bluetoothReading = false;
	}

	private void bluetooth_textview_update (string str)
	{
		tbBluetoothText += str;
		needToUpdateTextViewBluetooth = true;
	}

	/// <summary>
	/// Handles the event triggered when the Bluetooth LE data changes.
	/// check above: bluetoothHandlersAssigned
	/// </summary>
	private void BluetoothLE_OnInstalling(object sender, BluetoothLE.InstallingEventArgs e)
	{
		bluetooth_textview_update ($"\nInstalling: {e.Value}");
	}
	private void BluetoothLE_OnBleakVersion(object sender, BluetoothLE.BleakVersionEventArgs e)
	{
		bluetooth_textview_update ($"\nBleak version: {e.Value}");
	}
	private void BluetoothLE_OnScanning(object sender)
	{
		bluetooth_textview_update ($"\nStart scanning ...");
	}
	private void BluetoothLE_OnDataChanged(object sender, BluetoothLE.DataChangedEventArgs e)
	{
		//bluetooth_textview_update ($"\n{e.CharacteristicUUID} {e.CharacteristicName} {e.Value}");
		bluetooth_textview_update ($"\n{e.CharacteristicName} {e.Value}");
	}
	private void BluetoothLE_OnDeviceChanged(object sender, BluetoothLE.DeviceEventArgs e)
	{
		bluetooth_textview_update ($"\n{e.Action} {e.Ip} {e.Value}");
	}
	
	/* ---------------------
	 * <---- bluetooth end 
	 * -------------------*/


	private void on_entry_database_name_changed (object o, EventArgs args)
	{
		entry_database_name.Text = Util.MakeValidSQL (entry_database_name.Text);
		preferences.machineName = Preferences.PreferencesChange (
				false, "machineName",
				preferences.machineName,
				entry_database_name.Text);
	}

	private Preferences.MaximizedTypes get_maximized_from_gui()
	{
		if( ! PWBox.check_appearance_maximized.Active )
			return Preferences.MaximizedTypes.NO;

		if( ! PWBox.check_appearance_maximized_undecorated.Active )
			return Preferences.MaximizedTypes.YES;

		return Preferences.MaximizedTypes.YESUNDECORATED;
	}

	private Preferences.pythonVersionEnum get_pythonVersion_from_gui()
	{
		if( PWBox.radio_python_2.Active)
			return Preferences.pythonVersionEnum.Python2;
		else //if( PWBox.radio_python_3.Active)
			return Preferences.pythonVersionEnum.Python3;
	}

	public Button Button_close
	{
		set { button_close = value; }
		get { return button_close;  }
	}

	public Preferences GetPreferences 
	{
		get { return preferences;  }
	}

	private void connectWidgets (Gtk.Builder builder)
	{
		preferences_win = (Gtk.Window) builder.GetObject ("preferences_win");
		notebook_top = (Gtk.Notebook) builder.GetObject ("notebook_top");
		notebook = (Gtk.Notebook) builder.GetObject ("notebook");
		hbox_buttons_bottom = (Gtk.HBox) builder.GetObject ("hbox_buttons_bottom");

		//view more tabs
		check_view_jumps = (Gtk.CheckButton) builder.GetObject ("check_view_jumps");
		check_view_runs = (Gtk.CheckButton) builder.GetObject ("check_view_runs");
		check_view_weights_inertial = (Gtk.CheckButton) builder.GetObject ("check_view_weights_inertial");
		check_view_isometric_elastic = (Gtk.CheckButton) builder.GetObject ("check_view_isometric_elastic");
		//tabs selection widgets
		image_view_more_tabs_close = (Gtk.Image) builder.GetObject ("image_view_more_tabs_close");
		label_mandatory_tabs = (Gtk.Label) builder.GetObject ("label_mandatory_tabs");
		label_selectable_tabs = (Gtk.Label) builder.GetObject ("label_selectable_tabs");

		//help widgets
		hbox_stiffness_formula = (Gtk.HBox) builder.GetObject ("hbox_stiffness_formula");
		textview_help_message = (Gtk.TextView) builder.GetObject ("textview_help_message");
		image_help_close = (Gtk.Image) builder.GetObject ("image_help_close");

		//main, person tabs
		check_appearance_maximized = (Gtk.CheckButton) builder.GetObject ("check_appearance_maximized");
		check_appearance_maximized_undecorated = (Gtk.CheckButton) builder.GetObject ("check_appearance_maximized_undecorated");
		check_appearance_person_win_hide = (Gtk.CheckButton) builder.GetObject ("check_appearance_person_win_hide");
		check_appearance_person_clubID = (Gtk.CheckButton) builder.GetObject ("check_appearance_person_clubID");
		check_appearance_person_photo = (Gtk.CheckButton) builder.GetObject ("check_appearance_person_photo");
		radio_font_size_custom = (Gtk.RadioButton) builder.GetObject ("radio_font_size_custom");
		radio_font_size_default = (Gtk.RadioButton) builder.GetObject ("radio_font_size_default");
		box_font_size_custom = (Gtk.Box) builder.GetObject ("box_font_size_custom");
		spin_font_size_custom = (Gtk.SpinButton) builder.GetObject ("spin_font_size_custom");
		alignment_undecorated = (Gtk.Alignment) builder.GetObject ("alignment_undecorated");
//		label_recommended_undecorated = (Gtk.Label) builder.GetObject ("label_recommended_undecorated");
		radio_font_courier = (Gtk.RadioButton) builder.GetObject ("radio_font_courier");
		radio_font_helvetica = (Gtk.RadioButton) builder.GetObject ("radio_font_helvetica");
		radio_font_noto_sans_cjk_sc = (Gtk.RadioButton) builder.GetObject ("radio_font_noto_sans_cjk_sc");
		check_rest_time = (Gtk.CheckButton) builder.GetObject ("check_rest_time");
		image_rest = (Gtk.Image) builder.GetObject ("image_rest");
		hbox_rest_time_values = (Gtk.HBox) builder.GetObject ("hbox_rest_time_values");
		spinbutton_rest_minutes = (Gtk.SpinButton) builder.GetObject ("spinbutton_rest_minutes");
		spinbutton_rest_seconds = (Gtk.SpinButton) builder.GetObject ("spinbutton_rest_seconds");

		radio_color_custom = (Gtk.RadioButton) builder.GetObject ("radio_color_custom");
		radio_color_chronojump_blue = (Gtk.RadioButton) builder.GetObject ("radio_color_chronojump_blue");
		radio_color_os = (Gtk.RadioButton) builder.GetObject ("radio_color_os");
		drawingarea_background_color = (Gtk.DrawingArea) builder.GetObject ("drawingarea_background_color");
		button_color_choose = (Gtk.Button) builder.GetObject ("button_color_choose");
		drawingarea_background_color_chronojump_blue = (Gtk.DrawingArea) builder.GetObject ("drawingarea_background_color_chronojump_blue");
		label_radio_color_os_needs_restart = (Gtk.Label) builder.GetObject ("label_radio_color_os_needs_restart");

		check_logo_animated = (Gtk.CheckButton) builder.GetObject ("check_logo_animated");
		hbox_last_session_and_mode = (Gtk.HBox) builder.GetObject ("hbox_last_session_and_mode");
		check_session_autoload_at_start = (Gtk.CheckButton) builder.GetObject ("check_session_autoload_at_start");
		check_mode_autoload_at_start = (Gtk.CheckButton) builder.GetObject ("check_mode_autoload_at_start");


		//jumps tab	
		//	label_jumps = (Gtk.Label) builder.GetObject ("label_jumps");
		checkbutton_power = (Gtk.CheckButton) builder.GetObject ("checkbutton_power");
		checkbutton_stiffness = (Gtk.CheckButton) builder.GetObject ("checkbutton_stiffness");
		image_jumps_power_help = (Gtk.Image) builder.GetObject ("image_jumps_power_help");
		image_jumps_stiffness_help = (Gtk.Image) builder.GetObject ("image_jumps_stiffness_help");
		checkbutton_initial_speed = (Gtk.CheckButton) builder.GetObject ("checkbutton_initial_speed");
		checkbutton_jump_rsi = (Gtk.CheckButton) builder.GetObject ("checkbutton_jump_rsi");
		//	checkbutton_angle = (Gtk.CheckButton) builder.GetObject ("checkbutton_angle");
		checkbutton_show_tv_tc_index = (Gtk.CheckButton) builder.GetObject ("checkbutton_show_tv_tc_index");
		hbox_indexes = (Gtk.Box) builder.GetObject ("hbox_indexes");
		radiobutton_show_q_index = (Gtk.RadioButton) builder.GetObject ("radiobutton_show_q_index");
		radiobutton_show_dj_index = (Gtk.RadioButton) builder.GetObject ("radiobutton_show_dj_index");
		radio_weight_percent = (Gtk.RadioButton) builder.GetObject ("radio_weight_percent");
		radio_weight_kg = (Gtk.RadioButton) builder.GetObject ("radio_weight_kg");
		radio_use_heights_on_jump_indexes = (Gtk.RadioButton) builder.GetObject ("radio_use_heights_on_jump_indexes");
		radio_do_not_use_heights_on_jump_indexes = (Gtk.RadioButton) builder.GetObject ("radio_do_not_use_heights_on_jump_indexes");

		//runs tab	
		notebook_races = (Gtk.Notebook) builder.GetObject ("notebook_races");
		image_run_speed_start_help = (Gtk.Image) builder.GetObject ("image_run_speed_start_help");
		radio_speed_ms = (Gtk.RadioButton) builder.GetObject ("radio_speed_ms");
		radio_speed_km = (Gtk.RadioButton) builder.GetObject ("radio_speed_km");
		radio_runs_speed_start_arrival = (Gtk.RadioButton) builder.GetObject ("radio_runs_speed_start_arrival"); 
		radio_runs_speed_start_leaving = (Gtk.RadioButton) builder.GetObject ("radio_runs_speed_start_leaving"); 
		image_races_simple = (Gtk.Image) builder.GetObject ("image_races_simple");
		image_races_intervallic = (Gtk.Image) builder.GetObject ("image_races_intervallic");
		notebook_races_double_contacts = (Gtk.Notebook) builder.GetObject ("notebook_races_double_contacts");
		vbox_runs_prevent_double_contact = (Gtk.Box) builder.GetObject ("vbox_runs_prevent_double_contact");
		checkbutton_runs_prevent_double_contact = (Gtk.CheckButton) builder.GetObject ("checkbutton_runs_prevent_double_contact");
		spinbutton_runs_prevent_double_contact = (Gtk.SpinButton) builder.GetObject ("spinbutton_runs_prevent_double_contact");
		vbox_runs_i_prevent_double_contact = (Gtk.Box) builder.GetObject ("vbox_runs_i_prevent_double_contact");
		checkbutton_runs_i_prevent_double_contact = (Gtk.CheckButton) builder.GetObject ("checkbutton_runs_i_prevent_double_contact");
		spinbutton_runs_i_prevent_double_contact = (Gtk.SpinButton) builder.GetObject ("spinbutton_runs_i_prevent_double_contact");

		//encoder tab
		notebook_encoder = (Gtk.Notebook) builder.GetObject ("notebook_encoder");
		//capture
		spin_encoder_capture_time = (Gtk.SpinButton) builder.GetObject ("spin_encoder_capture_time");
		check_encoder_capture_inactivity_end_time = (Gtk.CheckButton) builder.GetObject ("check_encoder_capture_inactivity_end_time");
		hbox_encoder_capture_inactivity_time = (Gtk.HBox) builder.GetObject ("hbox_encoder_capture_inactivity_time");
		spin_encoder_capture_inactivity_end_time = (Gtk.SpinButton) builder.GetObject ("spin_encoder_capture_inactivity_end_time");
		hbox_encoder_capture_curves_save = (Gtk.HBox) builder.GetObject ("hbox_encoder_capture_curves_save");
		spin_encoder_capture_curves_best_n = (Gtk.SpinButton) builder.GetObject ("spin_encoder_capture_curves_best_n");
		label_encoder_capture_save_repetitions_explanation = (Gtk.Label) builder.GetObject ("label_encoder_capture_save_repetitions_explanation");
		image_encoder_gravitatory = (Gtk.Image) builder.GetObject ("image_encoder_gravitatory");
		image_encoder_inertial = (Gtk.Image) builder.GetObject ("image_encoder_inertial");
		image_encoder_inertial2 = (Gtk.Image) builder.GetObject ("image_encoder_inertial2");
		image_encoder_triggers = (Gtk.Image) builder.GetObject ("image_encoder_triggers");
		checkbutton_encoder_capture_inertial_discard_first_n = (Gtk.CheckButton) builder.GetObject ("checkbutton_encoder_capture_inertial_discard_first_n");
		box_encoder_capture_inertial_discard_first_n = (Gtk.Box) builder.GetObject ("box_encoder_capture_inertial_discard_first_n");
		spin_encoder_capture_inertial_discard_first_n = (Gtk.SpinButton) builder.GetObject ("spin_encoder_capture_inertial_discard_first_n");
		spin_encoder_capture_show_only_some_bars = (Gtk.SpinButton) builder.GetObject ("spin_encoder_capture_show_only_some_bars");
		radio_encoder_capture_show_all_bars = (Gtk.RadioButton) builder.GetObject ("radio_encoder_capture_show_all_bars");
		radio_encoder_capture_show_only_some_bars = (Gtk.RadioButton) builder.GetObject ("radio_encoder_capture_show_only_some_bars");
		spin_encoder_capture_barplot_font_size = (Gtk.SpinButton) builder.GetObject ("spin_encoder_capture_barplot_font_size");
		check_show_start_and_duration = (Gtk.CheckButton) builder.GetObject ("check_show_start_and_duration");
		radio_encoder_triggers_no = (Gtk.RadioButton) builder.GetObject ("radio_encoder_triggers_no");
		radio_encoder_triggers_yes = (Gtk.RadioButton) builder.GetObject ("radio_encoder_triggers_yes");
		vbox_encoder_triggers_yes = (Gtk.VBox) builder.GetObject ("vbox_encoder_triggers_yes");
		radio_encoder_triggers_yes_start_at_capture = (Gtk.RadioButton) builder.GetObject ("radio_encoder_triggers_yes_start_at_capture");
		radio_encoder_triggers_yes_start_at_first_trigger = (Gtk.RadioButton) builder.GetObject ("radio_encoder_triggers_yes_start_at_first_trigger");
		image_encoder_inactivity_help = (Gtk.Image) builder.GetObject ("image_encoder_inactivity_help");
		image_encoder_capture_cut_by_triggers_help = (Gtk.Image) builder.GetObject ("image_encoder_capture_cut_by_triggers_help");
		check_encoder_capture_infinite = (Gtk.CheckButton) builder.GetObject ("check_encoder_capture_infinite");
		image_encoder_capture_infinite = (Gtk.Image) builder.GetObject ("image_encoder_capture_infinite");
		radio_encoder_rep_criteria_gravitatory_ecc_con = (Gtk.RadioButton) builder.GetObject ("radio_encoder_rep_criteria_gravitatory_ecc_con");
		radio_encoder_rep_criteria_gravitatory_ecc = (Gtk.RadioButton) builder.GetObject ("radio_encoder_rep_criteria_gravitatory_ecc");
		radio_encoder_rep_criteria_gravitatory_con = (Gtk.RadioButton) builder.GetObject ("radio_encoder_rep_criteria_gravitatory_con");
		radio_encoder_rep_criteria_inertial_ecc_con = (Gtk.RadioButton) builder.GetObject ("radio_encoder_rep_criteria_inertial_ecc_con");
		radio_encoder_rep_criteria_inertial_ecc = (Gtk.RadioButton) builder.GetObject ("radio_encoder_rep_criteria_inertial_ecc");
		radio_encoder_rep_criteria_inertial_con = (Gtk.RadioButton) builder.GetObject ("radio_encoder_rep_criteria_inertial_con");
		//analyze
		checkbutton_encoder_propulsive = (Gtk.CheckButton) builder.GetObject ("checkbutton_encoder_propulsive");
		radio_encoder_work_kcal = (Gtk.RadioButton) builder.GetObject ("radio_encoder_work_kcal");
		radio_encoder_work_joules = (Gtk.RadioButton) builder.GetObject ("radio_encoder_work_joules");
		radio_encoder_inertial_analyze_equivalent_mass = (Gtk.RadioButton) builder.GetObject ("radio_encoder_inertial_analyze_equivalent_mass");
		radio_encoder_inertial_analyze_inertia_moment = (Gtk.RadioButton) builder.GetObject ("radio_encoder_inertial_analyze_inertia_moment");
		radio_encoder_inertial_analyze_diameter = (Gtk.RadioButton) builder.GetObject ("radio_encoder_inertial_analyze_diameter");
		image_encoder_inertial_analyze_eq_mass_help = (Gtk.Image) builder.GetObject ("image_encoder_inertial_analyze_eq_mass_help");
		spin_encoder_smooth_con = (Gtk.SpinButton) builder.GetObject ("spin_encoder_smooth_con");
		label_encoder_con = (Gtk.Label) builder.GetObject ("label_encoder_con");
		radio_encoder_1RM_nonweighted = (Gtk.RadioButton) builder.GetObject ("radio_encoder_1RM_nonweighted");
		radio_encoder_1RM_weighted = (Gtk.RadioButton) builder.GetObject ("radio_encoder_1RM_weighted");
		radio_encoder_1RM_weighted2 = (Gtk.RadioButton) builder.GetObject ("radio_encoder_1RM_weighted2");
		radio_encoder_1RM_weighted3 = (Gtk.RadioButton) builder.GetObject ("radio_encoder_1RM_weighted3");

		//forceSensor tab
		check_force_sensor_isometric_butterworth = (Gtk.CheckButton) builder.GetObject ("check_force_sensor_isometric_butterworth");
		box_force_sensor_isometric_butterworth_values = (Gtk.Box) builder.GetObject ("box_force_sensor_isometric_butterworth_values");
		spin_force_sensor_isometric_butterworth = (Gtk.SpinButton) builder.GetObject ("spin_force_sensor_isometric_butterworth");
		check_force_sensor_elastic_butterworth = (Gtk.CheckButton) builder.GetObject ("check_force_sensor_elastic_butterworth");
		box_force_sensor_elastic_butterworth_values = (Gtk.Box) builder.GetObject ("box_force_sensor_elastic_butterworth_values");
		spin_force_sensor_elastic_butterworth = (Gtk.SpinButton) builder.GetObject ("spin_force_sensor_elastic_butterworth");
		notebook_force_sensor = (Gtk.Notebook) builder.GetObject ("notebook_force_sensor");
		spin_force_sensor_capture_width_graph_seconds = (Gtk.SpinButton) builder.GetObject ("spin_force_sensor_capture_width_graph_seconds");
		radio_force_sensor_capture_zoom_out = (Gtk.RadioButton) builder.GetObject ("radio_force_sensor_capture_zoom_out");
		radio_force_sensor_capture_scroll = (Gtk.RadioButton) builder.GetObject ("radio_force_sensor_capture_scroll");
		spin_force_sensor_elastic_ecc_min_displ = (Gtk.SpinButton) builder.GetObject ("spin_force_sensor_elastic_ecc_min_displ");
		spin_force_sensor_elastic_con_min_displ = (Gtk.SpinButton) builder.GetObject ("spin_force_sensor_elastic_con_min_displ");
		spin_force_sensor_not_elastic_ecc_min_force = (Gtk.SpinButton) builder.GetObject ("spin_force_sensor_not_elastic_ecc_min_force");
		spin_force_sensor_not_elastic_con_min_force = (Gtk.SpinButton) builder.GetObject ("spin_force_sensor_not_elastic_con_min_force");
		spin_force_sensor_graphs_line_width = (Gtk.SpinButton) builder.GetObject ("spin_force_sensor_graphs_line_width");
		radio_force_sensor_variability_rmssd = (Gtk.RadioButton) builder.GetObject ("radio_force_sensor_variability_rmssd");
		radio_force_sensor_variability_cvrmssd = (Gtk.RadioButton) builder.GetObject ("radio_force_sensor_variability_cvrmssd");
		radio_force_sensor_variability_cv = (Gtk.RadioButton) builder.GetObject ("radio_force_sensor_variability_cv");
		radio_force_sensor_variability_old = (Gtk.RadioButton) builder.GetObject ("radio_force_sensor_variability_old");
		hbox_force_sensor_lag = (Gtk.HBox) builder.GetObject ("hbox_force_sensor_lag");
		spin_force_sensor_variability_lag = (Gtk.SpinButton) builder.GetObject ("spin_force_sensor_variability_lag");
		spin_force_sensor_analyze_best_stability_in_window = (Gtk.SpinButton) builder.GetObject ("spin_force_sensor_analyze_best_stability_in_window");
		spin_force_sensor_analyze_max_avg_force_in_window = (Gtk.SpinButton) builder.GetObject ("spin_force_sensor_analyze_max_avg_force_in_window");

		//runEncoder tab
		spin_run_encoder_acceleration = (Gtk.SpinButton) builder.GetObject ("spin_run_encoder_acceleration");
		spin_run_encoder_pps = (Gtk.SpinButton) builder.GetObject ("spin_run_encoder_pps");
		label_pps_equivalent = (Gtk.Label) builder.GetObject ("label_pps_equivalent");
		label_pps_maximum = (Gtk.Label) builder.GetObject ("label_pps_maximum");

		//multimedia tab
		checkbutton_volume = (Gtk.CheckButton) builder.GetObject ("checkbutton_volume");
		alignment_multimedia_sounds = (Gtk.Alignment) builder.GetObject ("alignment_multimedia_sounds");
		radio_gstreamer_0_1 = (Gtk.RadioButton) builder.GetObject ("radio_gstreamer_0_1");
		radio_gstreamer_1_0 = (Gtk.RadioButton) builder.GetObject ("radio_gstreamer_1_0");
		radio_ffplay = (Gtk.RadioButton) builder.GetObject ("radio_ffplay");
		radio_sound_systemsounds = (Gtk.RadioButton) builder.GetObject ("radio_sound_systemsounds");
		hbox_not_recommended_when_not_on_windows = (Gtk.HBox) builder.GetObject ("hbox_not_recommended_when_not_on_windows");
		label_test_sound_result = (Gtk.Label) builder.GetObject ("label_test_sound_result");
		notebook_multimedia = (Gtk.Notebook) builder.GetObject ("notebook_multimedia");
		hbox_combo_camera = (Gtk.Box) builder.GetObject ("hbox_combo_camera");
		hbox_camera_resolution_framerate = (Gtk.HBox) builder.GetObject ("hbox_camera_resolution_framerate");
		hbox_camera_resolution_custom = (Gtk.HBox) builder.GetObject ("hbox_camera_resolution_custom");
		spin_camera_resolution_custom_width = (Gtk.SpinButton) builder.GetObject ("spin_camera_resolution_custom_width");
		spin_camera_resolution_custom_height = (Gtk.SpinButton) builder.GetObject ("spin_camera_resolution_custom_height");
		hbox_camera_framerate_custom = (Gtk.HBox) builder.GetObject ("hbox_camera_framerate_custom");
		spin_camera_framerate_custom = (Gtk.SpinButton) builder.GetObject ("spin_camera_framerate_custom");
		entry_camera_framerate_custom_decimals = (Gtk.Entry) builder.GetObject ("entry_camera_framerate_custom_decimals");
		//label_camera_pixel_format = (Gtk.Label) builder.GetObject ("label_camera_pixel_format");
		label_camera_pixel_format_current = (Gtk.Label) builder.GetObject ("label_camera_pixel_format_current");
		label_camera_resolution_current = (Gtk.Label) builder.GetObject ("label_camera_resolution_current");
		label_camera_framerate_current = (Gtk.Label) builder.GetObject ("label_camera_framerate_current");
		hbox_combo_camera_pixel_format = (Gtk.HBox) builder.GetObject ("hbox_combo_camera_pixel_format");
		hbox_combo_camera_resolution = (Gtk.Box) builder.GetObject ("hbox_combo_camera_resolution");
		hbox_combo_camera_framerate = (Gtk.Box) builder.GetObject ("hbox_combo_camera_framerate");
		label_camera_error = (Gtk.Label) builder.GetObject ("label_camera_error");
		label_webcam_windows = (Gtk.Label) builder.GetObject ("label_webcam_windows");
		image_multimedia_audio = (Gtk.Image) builder.GetObject ("image_multimedia_audio");
		image_multimedia_video = (Gtk.Image) builder.GetObject ("image_multimedia_video");
		image_video_preview = (Gtk.Image) builder.GetObject ("image_video_preview");
		button_video_preview = (Gtk.Button) builder.GetObject ("button_video_preview");
		label_video_preview_error = (Gtk.Label) builder.GetObject ("label_video_preview_error");
		check_camera_stop_after = (Gtk.CheckButton) builder.GetObject ("check_camera_stop_after");
		check_camera_advanced = (Gtk.CheckButton) builder.GetObject ("check_camera_advanced");
		frame_camera_advanced = (Gtk.Frame) builder.GetObject ("frame_camera_advanced");
		//vbox_camera_stop_after_all = (Gtk.VBox) builder.GetObject ("vbox_camera_stop_after_all");
		//vbox_camera_stop_after = (Gtk.VBox) builder.GetObject ("vbox_camera_stop_after");
		hbox_camera_stop_after_seconds = (Gtk.HBox) builder.GetObject ("hbox_camera_stop_after_seconds");
		spin_camera_stop_after = (Gtk.SpinButton) builder.GetObject ("spin_camera_stop_after");
		grid_video_advanced_actions = (Gtk.Grid) builder.GetObject ("grid_video_advanced_actions");
		label_video_check_ffmpeg_running = (Gtk.Label) builder.GetObject ("label_video_check_ffmpeg_running");
		label_video_check_ffplay_running = (Gtk.Label) builder.GetObject ("label_video_check_ffplay_running");
		button_video_ffmpeg_kill = (Gtk.Button) builder.GetObject ("button_video_ffmpeg_kill");
		button_video_ffplay_kill = (Gtk.Button) builder.GetObject ("button_video_ffplay_kill");
		label_camera_check_running = (Gtk.Label) builder.GetObject ("label_camera_check_running");
		notebook_multimedia_video = (Gtk.Notebook) builder.GetObject ("notebook_multimedia_video");

		//language tab
		hbox_combo_language = (Gtk.Box) builder.GetObject ("hbox_combo_language");
		radio_language_detected = (Gtk.RadioButton) builder.GetObject ("radio_language_detected");
		radio_language_force = (Gtk.RadioButton) builder.GetObject ("radio_language_force");
		radio_graphs_translate = (Gtk.RadioButton) builder.GetObject ("radio_graphs_translate");
		radio_graphs_no_translate = (Gtk.RadioButton) builder.GetObject ("radio_graphs_no_translate");

		//advanced tab
		notebook_advanced = (Gtk.Notebook) builder.GetObject ("notebook_advanced");
		image_advanced_cloud = (Gtk.Image) builder.GetObject ("image_advanced_cloud");
		image_advanced_logs = (Gtk.Image) builder.GetObject ("image_advanced_logs");
		image_advanced_more = (Gtk.Image) builder.GetObject ("image_advanced_more");
		grid_database = (Gtk.Grid) builder.GetObject ("grid_database");
		label_database_id = (Gtk.Label) builder.GetObject ("label_database_id");
		entry_database_name = (Gtk.Entry) builder.GetObject ("entry_database_name");
		checkbutton_ask_deletion = (Gtk.CheckButton) builder.GetObject ("checkbutton_ask_deletion");
		box_combo_decimals = (Gtk.Box) builder.GetObject ("box_combo_decimals");
		checkbutton_mute_logs = (Gtk.CheckButton) builder.GetObject ("checkbutton_mute_logs");
		radio_export_latin = (Gtk.RadioButton) builder.GetObject ("radio_export_latin");
		radio_export_non_latin = (Gtk.RadioButton) builder.GetObject ("radio_export_non_latin");
		label_advanced_feedback = (Gtk.Label) builder.GetObject ("label_advanced_feedback");
		button_delete_devices = (Gtk.Button) builder.GetObject ("button_delete_devices");
		toggle_gc_collect_on_close = (Gtk.ToggleButton) builder.GetObject ("toggle_gc_collect_on_close");
		toggle_never_close = (Gtk.ToggleButton) builder.GetObject ("toggle_never_close");
		vbox_version = (Gtk.VBox) builder.GetObject ("vbox_version");
		label_progVersion = (Gtk.Label) builder.GetObject ("label_progVersion");
		frame_networks = (Gtk.Frame) builder.GetObject ("frame_networks");
		check_networks_devices = (Gtk.CheckButton) builder.GetObject ("check_networks_devices");
		radio_cloud_no = (Gtk.RadioButton) builder.GetObject ("radio_cloud_no");
		radio_cloud_capture = (Gtk.RadioButton) builder.GetObject ("radio_cloud_capture");
		radio_cloud_view = (Gtk.RadioButton) builder.GetObject ("radio_cloud_view");
		button_cloud_capture_path = (Gtk.Button) builder.GetObject ("button_cloud_capture_path");
		button_cloud_view_path = (Gtk.Button) builder.GetObject ("button_cloud_view_path");
		button_cloud_view_databases = (Gtk.Button) builder.GetObject ("button_cloud_view_databases");
		label_radio_cloud_no = (Gtk.Label) builder.GetObject ("label_radio_cloud_no");
		label_radio_cloud_no_recommended = (Gtk.Label) builder.GetObject ("label_radio_cloud_no_recommended");
		label_radio_cloud_capture = (Gtk.Label) builder.GetObject ("label_radio_cloud_capture");
		label_radio_cloud_view = (Gtk.Label) builder.GetObject ("label_radio_cloud_view");
		image_cloud_capture = (Gtk.Image) builder.GetObject ("image_cloud_capture");
		image_cloud_view = (Gtk.Image) builder.GetObject ("image_cloud_view");
		image_cloud_schema = (Gtk.Image) builder.GetObject ("image_cloud_schema");
		label_cloud_capture_path = (Gtk.Label) builder.GetObject ("label_cloud_capture_path");
		label_cloud_view_path = (Gtk.Label) builder.GetObject ("label_cloud_view_path");
		box_silicon_cloud_path_choose = (Gtk.Box) builder.GetObject ("box_silicon_cloud_path_choose");
		box_silicon_cloud_path_capture = (Gtk.Box) builder.GetObject ("box_silicon_cloud_path_capture");
		box_silicon_cloud_path_view = (Gtk.Box) builder.GetObject ("box_silicon_cloud_path_view");
		entry_silicon_cloud_capture_path = (Gtk.Entry) builder.GetObject ("entry_silicon_cloud_capture_path");
		entry_silicon_cloud_view_path = (Gtk.Entry) builder.GetObject ("entry_silicon_cloud_view_path");
		label_silicon_cloud_path_does_not_exists = (Gtk.Label) builder.GetObject ("label_silicon_cloud_path_does_not_exists");

		image_advanced_bluetooth = (Gtk.Image) builder.GetObject ("image_advanced_bluetooth");
		entry_bluetooth_url = (Gtk.Entry) builder.GetObject ("entry_bluetooth_url");
		button_bluetooth_start = (Gtk.Button) builder.GetObject ("button_bluetooth_start");
		button_bluetooth_end = (Gtk.Button) builder.GetObject ("button_bluetooth_end");
		textview_bluetooth = (Gtk.TextView) builder.GetObject ("textview_bluetooth");

		button_debug_mode = (Gtk.Button) builder.GetObject ("button_debug_mode");

		entry_send_log = (Gtk.Entry) builder.GetObject ("entry_send_log");
		textview_send_log_comments = (Gtk.TextView) builder.GetObject ("textview_send_log_comments");
		radio_send_log_current = (Gtk.RadioButton) builder.GetObject ("radio_send_log_current");
		radio_send_log_previous = (Gtk.RadioButton) builder.GetObject ("radio_send_log_previous");
		button_send_log = (Gtk.Button) builder.GetObject ("button_send_log");
		image_button_send_log = (Gtk.Image) builder.GetObject ("image_button_send_log");
		image_send_log_no = (Gtk.Image) builder.GetObject ("image_send_log_no");
		image_send_log_yes = (Gtk.Image) builder.GetObject ("image_send_log_yes");
		textview_send_log_message = (Gtk.TextView) builder.GetObject ("textview_send_log_message");

		image_advanced_r = (Gtk.Image) builder.GetObject ("image_advanced_r");
		image_advanced_python = (Gtk.Image) builder.GetObject ("image_advanced_python");
		radio_r_default = (Gtk.RadioButton) builder.GetObject ("radio_r_default");
		radio_r_other = (Gtk.RadioButton) builder.GetObject ("radio_r_other");
		entry_r_user_location = (Gtk.Entry) builder.GetObject ("entry_r_user_location");
		button_r_choose = (Gtk.Button) builder.GetObject ("button_r_choose");
		button_r_autodetect = (Gtk.Button) builder.GetObject ("button_r_autodetect");
		radio_rscript_default = (Gtk.RadioButton) builder.GetObject ("radio_rscript_default");
		radio_rscript_other = (Gtk.RadioButton) builder.GetObject ("radio_rscript_other");
		entry_rscript_user_location = (Gtk.Entry) builder.GetObject ("entry_rscript_user_location");
		button_rscript_choose = (Gtk.Button) builder.GetObject ("button_rscript_choose");
		button_rscript_autodetect = (Gtk.Button) builder.GetObject ("button_rscript_autodetect");
		button_rscript_usr_local_bin = (Gtk.Button) builder.GetObject ("button_rscript_usr_local_bin");
		radio_python_default = (Gtk.RadioButton) builder.GetObject ("radio_python_default");
		radio_python_other = (Gtk.RadioButton) builder.GetObject ("radio_python_other");
		button_python_choose = (Gtk.Button) builder.GetObject ("button_python_choose");
		button_python_autodetect = (Gtk.Button) builder.GetObject ("button_python_autodetect");
		entry_python_user_location = (Gtk.Entry) builder.GetObject ("entry_python_user_location");

		radio_python_2 = (Gtk.RadioButton) builder.GetObject ("radio_python_2");
		radio_python_3 = (Gtk.RadioButton) builder.GetObject ("radio_python_3");

		label_restart = (Gtk.Label) builder.GetObject ("label_restart");
		hbox_buttoms_bottom = (Gtk.HBox) builder.GetObject ("hbox_buttoms_bottom");
		button_close = (Gtk.Button) builder.GetObject ("button_close");
		image_button_close = (Gtk.Image) builder.GetObject ("image_button_close");
		combo_decimals = (Gtk.ComboBoxText) builder.GetObject ("combo_decimals");
	}
}
