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

	//appearance tab
	Gtk.CheckButton check_appearance_maximized;
	Gtk.CheckButton check_appearance_maximized_undecorated;
	Gtk.CheckButton check_appearance_person_win_hide;
	Gtk.CheckButton check_appearance_person_photo;
	Gtk.Alignment alignment_undecorated;
	Gtk.Label label_recommended_undecorated;
	Gtk.RadioButton radio_font_courier;
	Gtk.RadioButton radio_font_helvetica;
	Gtk.Label label_radio_font_needs_restart;
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
	Gtk.Image image_button_jumps_dj_heights_times_help;
	Gtk.CheckButton checkbutton_initial_speed;
	Gtk.CheckButton checkbutton_jump_rsi;
//	Gtk.CheckButton checkbutton_angle;
	Gtk.CheckButton checkbutton_show_tv_tc_index;
	Gtk.Box hbox_indexes;
	Gtk.RadioButton radiobutton_show_q_index;
	Gtk.RadioButton radiobutton_show_dj_index;
	Gtk.RadioButton radio_jumps_dj_heights;
	Gtk.RadioButton radio_jumps_dj_times;
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
	Gtk.SpinButton spin_force_sensor_analyze_ab_slider_increment;
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
	Gtk.VBox vbox_need_restart;
		
	//advanced tab
	Gtk.CheckButton checkbutton_ask_deletion;
	Gtk.Box box_combo_decimals;
	Gtk.CheckButton checkbutton_mute_logs;
	Gtk.RadioButton radio_export_latin;
	Gtk.RadioButton radio_export_non_latin;
	Gtk.Label label_advanced_feedback;
	Gtk.ToggleButton toggle_gc_collect_on_close;
	Gtk.ToggleButton toggle_never_close;
	Gtk.VBox vbox_version;
	Gtk.Label label_progVersion;
	Gtk.Frame frame_networks;
	Gtk.CheckButton check_networks_devices;
	Gtk.Button button_debug_mode;

	Gtk.RadioButton radio_python_2;
	Gtk.RadioButton radio_python_3;

	Gtk.Button button_close;
	Gtk.Image image_button_close;
	// <---- at glade


	Gtk.ComboBoxText combo_camera;
	Gtk.ComboBoxText combo_camera_pixel_format;
	Gtk.ComboBoxText combo_camera_resolution;
	Gtk.ComboBoxText combo_camera_framerate;
	Gtk.ComboBoxText combo_language;
	Gtk.ComboBoxText combo_decimals;

	public Gtk.Button FakeButtonMaximizeChanges;
	public Gtk.Button FakeButtonPersonWin;
	public Gtk.Button FakeButtonConfigurationImported;
	public Gtk.Button FakeButtonDebugModeStart;
	
	static PreferencesWindow PreferencesWindowBox;

	private RGBA colorBackground;
	private bool signalsNoFollow;

	private UtilAll.OperatingSystems operatingSystem;
	private Preferences preferences; //stored to update SQL if anything changed
//	private Thread thread;

	//string databaseURL;
	//string databaseTempURL;
	
	ListStore langsStore;

	private enum notebook_top_pages { PREFERENCES, SELECTTABS, HELP }

	const int JUMPSPAGE = 2;
	const int RUNSPAGE = 3;
	const int WEIGHTSINERTIALPAGE = 4;
	const int ISOMETRICELASTICPAGE = 5;

	static private WebcamDeviceList wd_list;
	private WebcamFfmpegSupportedModes wfsm;


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
		FakeButtonDebugModeStart = new Gtk.Button();
	}

	static public PreferencesWindow Show (
			Preferences preferences,
			Constants.Modes menu_mode, bool compujump, string progVersion)
	{
		if (PreferencesWindowBox == null) {
			PreferencesWindowBox = new PreferencesWindow ();
		}

		PreferencesWindowBox.notebook_top.CurrentPage = Convert.ToInt32(notebook_top_pages.PREFERENCES);
		PreferencesWindowBox.operatingSystem = UtilAll.GetOSEnum();

		if(compujump)
		{
			PreferencesWindowBox.check_appearance_person_win_hide.Sensitive = false;

			//show version
			PreferencesWindowBox.vbox_version.Visible = true;
			PreferencesWindowBox.label_progVersion.Text = "<b>" + progVersion + "</b>";
			PreferencesWindowBox.label_progVersion.UseMarkup = true;
			PreferencesWindowBox.check_networks_devices.Active = preferences.networksAllowChangeDevices;
		}
		PreferencesWindowBox.frame_networks.Visible = compujump;

		if(menu_mode !=	Constants.Modes.JUMPSSIMPLE && menu_mode != Constants.Modes.JUMPSREACTIVE)
		{
			PreferencesWindowBox.notebook.GetNthPage(JUMPSPAGE).Hide();
			PreferencesWindowBox.check_view_jumps.Active = false;
		} if(menu_mode != Constants.Modes.RUNSSIMPLE && menu_mode != Constants.Modes.RUNSINTERVALLIC &&
				menu_mode != Constants.Modes.RUNSENCODER)
		{
			PreferencesWindowBox.notebook.GetNthPage(RUNSPAGE).Hide();
			PreferencesWindowBox.check_view_runs.Active = false;
		} if(menu_mode != Constants.Modes.POWERGRAVITATORY && menu_mode != Constants.Modes.POWERINERTIAL)
		{
			PreferencesWindowBox.notebook.GetNthPage(WEIGHTSINERTIALPAGE).Hide();
			PreferencesWindowBox.check_view_weights_inertial.Active = false;
		}
		if(! Constants.ModeIsFORCESENSOR (menu_mode))
		{
			PreferencesWindowBox.notebook.GetNthPage(ISOMETRICELASTICPAGE).Hide();
			PreferencesWindowBox.check_view_isometric_elastic.Active = false;
		}

		PreferencesWindowBox.preferences = preferences;

		PreferencesWindowBox.createComboLanguage();
		Pixbuf pixbuf;


		PreferencesWindowBox.image_button_close.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_close.png");

		//appearance tab

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_rest.png");
		PreferencesWindowBox.image_rest.Pixbuf = pixbuf;

		//to avoid changing the sqlite and gui undecorated mode when activating maximized
		PreferencesWindowBox.signalsNoFollow = true;

		if(preferences.maximized == Preferences.MaximizedTypes.NO)
		{
			PreferencesWindowBox.check_appearance_maximized.Active = false;
			PreferencesWindowBox.alignment_undecorated.Visible = false;
			PreferencesWindowBox.label_recommended_undecorated.Visible = false;
		}
		else {
			PreferencesWindowBox.check_appearance_maximized.Active = true;
			PreferencesWindowBox.alignment_undecorated.Visible = true;
			PreferencesWindowBox.label_recommended_undecorated.Visible = true;
			PreferencesWindowBox.check_appearance_maximized_undecorated.Active =
				(preferences.maximized == Preferences.MaximizedTypes.YESUNDECORATED);
		}

		PreferencesWindowBox.signalsNoFollow = false;

		if(preferences.personWinHide)
			PreferencesWindowBox.check_appearance_person_win_hide.Active = true;
		else
			PreferencesWindowBox.check_appearance_person_win_hide.Active = false;

		PreferencesWindowBox.check_appearance_person_photo.Sensitive = ! preferences.personWinHide;

		if(preferences.personPhoto)
			PreferencesWindowBox.check_appearance_person_photo.Active = true;
		else
			PreferencesWindowBox.check_appearance_person_photo.Active = false;

		if(preferences.logoAnimatedShow)
			PreferencesWindowBox.check_logo_animated.Active = true;
		else
			PreferencesWindowBox.check_logo_animated.Active = false;

		PreferencesWindowBox.hbox_last_session_and_mode.Visible = ! compujump;

		if(preferences.loadLastSessionAtStart)
			PreferencesWindowBox.check_session_autoload_at_start.Active = true;
		else
			PreferencesWindowBox.check_session_autoload_at_start.Active = false;

		if(preferences.loadLastModeAtStart)
			PreferencesWindowBox.check_mode_autoload_at_start.Active = true;
		else
			PreferencesWindowBox.check_mode_autoload_at_start.Active = false;

		if(preferences.fontType == Preferences.FontTypes.Courier)
			PreferencesWindowBox.radio_font_courier.Active = true;
		else
			PreferencesWindowBox.radio_font_helvetica.Active = true;

		PreferencesWindowBox.check_rest_time.Active = (preferences.restTimeMinutes >= 0);
		PreferencesWindowBox.on_check_rest_time_toggled (new object (), new EventArgs ());

		if(preferences.restTimeMinutes >= 0)
		{
			PreferencesWindowBox.spinbutton_rest_minutes.Value = preferences.restTimeMinutes;
			PreferencesWindowBox.spinbutton_rest_seconds.Value = preferences.restTimeSeconds;
		} else { //min == -1 means no restTime
			PreferencesWindowBox.spinbutton_rest_minutes.Value = 2;
			PreferencesWindowBox.spinbutton_rest_seconds.Value = 0;
		}

		//multimedia tab
		if(preferences.volumeOn) {
			PreferencesWindowBox.checkbutton_volume.Active = true; 
			PreferencesWindowBox.alignment_multimedia_sounds.Visible = true;
		} else {
			PreferencesWindowBox.checkbutton_volume.Active = false; 
			PreferencesWindowBox.alignment_multimedia_sounds.Visible = false;
		}

		//hide video for compujump
		if(compujump)
			PreferencesWindowBox.notebook_multimedia.GetNthPage(1).Hide();

		PreferencesWindowBox.label_camera_error.Visible = false;

		PreferencesWindowBox.label_webcam_windows.Visible =
			(PreferencesWindowBox.operatingSystem == UtilAll.OperatingSystems.WINDOWS);

		PreferencesWindowBox.hbox_not_recommended_when_not_on_windows.Visible =
			! (PreferencesWindowBox.operatingSystem == UtilAll.OperatingSystems.WINDOWS);

		if(PreferencesWindowBox.operatingSystem == UtilAll.OperatingSystems.WINDOWS ||
				PreferencesWindowBox.operatingSystem == UtilAll.OperatingSystems.MACOSX)
		{
			if(preferences.gstreamer == Preferences.GstreamerTypes.FFPLAY)
				PreferencesWindowBox.radio_ffplay.Active = true;
			else //(preferences.gstreamer == Preferences.GstreamerTypes.SYSTEMSOUNDS)
				PreferencesWindowBox.radio_sound_systemsounds.Active = true;

			PreferencesWindowBox.radio_gstreamer_0_1.Visible = false;
			PreferencesWindowBox.radio_gstreamer_1_0.Visible = false;
		}
		else //LINUX
		{
			if(preferences.gstreamer == Preferences.GstreamerTypes.GST_0_1)
				PreferencesWindowBox.radio_gstreamer_0_1.Active = true;
			else if(preferences.gstreamer == Preferences.GstreamerTypes.GST_1_0)
				PreferencesWindowBox.radio_gstreamer_1_0.Active = true;
			else if(preferences.gstreamer == Preferences.GstreamerTypes.FFPLAY)
				PreferencesWindowBox.radio_ffplay.Active = true;
			else //(preferences.gstreamer == Preferences.GstreamerTypes.SYSTEMSOUNDS)
				PreferencesWindowBox.radio_sound_systemsounds.Active = true;
		}

		PreferencesWindowBox.label_test_sound_result.Text = "";

		PreferencesWindowBox.notebook_multimedia_video.CurrentPage = 0; //show only check_devices button

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "audio.png");
		PreferencesWindowBox.image_multimedia_audio.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "videocamera_on.png");
		PreferencesWindowBox.image_multimedia_video.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_photo_preview.png");
		PreferencesWindowBox.image_video_preview.Pixbuf = pixbuf;

		PreferencesWindowBox.spin_camera_stop_after.Value = preferences.videoStopAfter;
		//PreferencesWindowBox.vbox_camera_stop_after.Visible = (preferences.videoStopAfter > 0);
		PreferencesWindowBox.hbox_camera_stop_after_seconds.Visible = (preferences.videoStopAfter > 0);
		PreferencesWindowBox.check_camera_stop_after.Active = (preferences.videoStopAfter > 0);

		PreferencesWindowBox.createComboDecimals ();

		if(preferences.showPower)
			PreferencesWindowBox.checkbutton_power.Active = true; 
		else
			PreferencesWindowBox.checkbutton_power.Active = false; 
		
		if(preferences.showStiffness)
			PreferencesWindowBox.checkbutton_stiffness.Active = true; 
		else
			PreferencesWindowBox.checkbutton_stiffness.Active = false; 
		
		if(preferences.showInitialSpeed)  
			PreferencesWindowBox.checkbutton_initial_speed.Active = true; 
		else 
			PreferencesWindowBox.checkbutton_initial_speed.Active = false; 

		if(preferences.showJumpRSI)
			PreferencesWindowBox.checkbutton_jump_rsi.Active = true;
		else
			PreferencesWindowBox.checkbutton_jump_rsi.Active = false;

		/*
		if(preferences.showAngle)  
			PreferencesWindowBox.checkbutton_angle.Active = true; 
		else 
			PreferencesWindowBox.checkbutton_angle.Active = false; 
		*/

		if(preferences.showQIndex || preferences.showDjIndex) { 
			PreferencesWindowBox.checkbutton_show_tv_tc_index.Active = true; 
			if(preferences.showQIndex) {
				PreferencesWindowBox.radiobutton_show_q_index.Active = true; 
			} else {
				PreferencesWindowBox.radiobutton_show_dj_index.Active = true; 
			}
			PreferencesWindowBox.hbox_indexes.Show();
		}
		else {
			PreferencesWindowBox.checkbutton_show_tv_tc_index.Active = false; 
			PreferencesWindowBox.hbox_indexes.Hide();
		}

		if(preferences.heightPreferred)
			PreferencesWindowBox.radio_jumps_dj_heights.Active = true;
		else
			PreferencesWindowBox.radio_jumps_dj_times.Active = true;

		if(preferences.askDeletion)
			PreferencesWindowBox.checkbutton_ask_deletion.Active = true; 
		else 
			PreferencesWindowBox.checkbutton_ask_deletion.Active = false; 

		if(preferences.muteLogs)
			PreferencesWindowBox.checkbutton_mute_logs.Active = true;
		else
			PreferencesWindowBox.checkbutton_mute_logs.Active = false;

		PreferencesWindowBox.button_debug_mode.Sensitive = ! preferences.debugMode;

		if(preferences.weightStatsPercent)  
			PreferencesWindowBox.radio_weight_percent.Active = true; 
		else 
			PreferencesWindowBox.radio_weight_kg.Active = true; 

		if(preferences.metersSecondsPreferred)  
			PreferencesWindowBox.radio_speed_ms.Active = true; 
		else 
			PreferencesWindowBox.radio_speed_km.Active = true; 


		if(preferences.runSpeedStartArrival)  
			PreferencesWindowBox.radio_runs_speed_start_arrival.Active = true; 
		else 
			PreferencesWindowBox.radio_runs_speed_start_leaving.Active = true; 


		//start of double contacts stuff ----

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_run_simple.png");
		PreferencesWindowBox.image_races_simple.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_run_multiple.png");
		PreferencesWindowBox.image_races_intervallic.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_info.png");
		PreferencesWindowBox.image_jumps_power_help.Pixbuf = pixbuf;
		PreferencesWindowBox.image_jumps_stiffness_help.Pixbuf = pixbuf;
		PreferencesWindowBox.image_button_jumps_dj_heights_times_help.Pixbuf = pixbuf;
		PreferencesWindowBox.image_run_speed_start_help.Pixbuf = pixbuf;
		PreferencesWindowBox.image_encoder_inactivity_help.Pixbuf = pixbuf;
		PreferencesWindowBox.image_encoder_capture_cut_by_triggers_help.Pixbuf = pixbuf;
		PreferencesWindowBox.image_encoder_inertial_analyze_eq_mass_help.Pixbuf = pixbuf;

		if (menu_mode == Constants.Modes.RUNSSIMPLE || menu_mode == Constants.Modes.RUNSINTERVALLIC)
		{
			PreferencesWindowBox.notebook_races.CurrentPage = 0;

			if(menu_mode ==	Constants.Modes.RUNSSIMPLE)
				PreferencesWindowBox.notebook_races_double_contacts.CurrentPage = 0;
			else if(menu_mode == Constants.Modes.RUNSINTERVALLIC)
				PreferencesWindowBox.notebook_races_double_contacts.CurrentPage = 1;
		} else if (menu_mode == Constants.Modes.RUNSENCODER)
			PreferencesWindowBox.notebook_races.CurrentPage = 1;

		PreferencesWindowBox.checkbutton_runs_prevent_double_contact.Active = 
			(preferences.runDoubleContactsMode != Constants.DoubleContact.NONE);
		PreferencesWindowBox.checkbutton_runs_i_prevent_double_contact.Active = 
			(preferences.runIDoubleContactsMode != Constants.DoubleContact.NONE);

		PreferencesWindowBox.spinbutton_runs_prevent_double_contact.Value = 
			preferences.runDoubleContactsMS;
		PreferencesWindowBox.spinbutton_runs_i_prevent_double_contact.Value = 
			preferences.runIDoubleContactsMS;

		//---- end of double contacts stuff		


		if(preferences.CSVExportDecimalSeparator == "COMMA")
			PreferencesWindowBox.radio_export_latin.Active = true; 
		else
			PreferencesWindowBox.radio_export_non_latin.Active = true; 

	
		//encoder capture -->

		PreferencesWindowBox.spin_encoder_capture_time.Value = preferences.encoderCaptureTime;

		if(preferences.encoderCaptureInactivityEndTime < 0) {
			PreferencesWindowBox.check_encoder_capture_inactivity_end_time.Active = false;
			PreferencesWindowBox.hbox_encoder_capture_inactivity_time.Sensitive = false;
			PreferencesWindowBox.spin_encoder_capture_inactivity_end_time.Value = 3;
		} else {
			PreferencesWindowBox.check_encoder_capture_inactivity_end_time.Active = true;
			PreferencesWindowBox.hbox_encoder_capture_inactivity_time.Sensitive = true;
			PreferencesWindowBox.spin_encoder_capture_inactivity_end_time.Value = preferences.encoderCaptureInactivityEndTime;
		}


		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_weight.png");
		PreferencesWindowBox.image_encoder_gravitatory.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_inertia.png");
		PreferencesWindowBox.image_encoder_inertial.Pixbuf = pixbuf;
		PreferencesWindowBox.image_encoder_inertial2.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_encoder_triggers_no.png");
		PreferencesWindowBox.image_encoder_triggers.Pixbuf = pixbuf;

		if(preferences.encoderCaptureInertialDiscardFirstN > 0) {
			PreferencesWindowBox.checkbutton_encoder_capture_inertial_discard_first_n.Active = true;
			PreferencesWindowBox.spin_encoder_capture_inertial_discard_first_n.Value = preferences.encoderCaptureInertialDiscardFirstN;
			PreferencesWindowBox.box_encoder_capture_inertial_discard_first_n.Visible = true;
		} else {
			PreferencesWindowBox.checkbutton_encoder_capture_inertial_discard_first_n.Active = false;
			PreferencesWindowBox.spin_encoder_capture_inertial_discard_first_n.Value = 3;
			PreferencesWindowBox.box_encoder_capture_inertial_discard_first_n.Visible = false;
		}

		if(preferences.encoderCaptureShowNRepetitions < 0) {
			PreferencesWindowBox.radio_encoder_capture_show_all_bars.Active = true;
			PreferencesWindowBox.spin_encoder_capture_show_only_some_bars.Value = 10;
		} else {
			PreferencesWindowBox.radio_encoder_capture_show_only_some_bars.Active = true;
			PreferencesWindowBox.spin_encoder_capture_show_only_some_bars.Value = preferences.encoderCaptureShowNRepetitions;
		}


		PreferencesWindowBox.spin_encoder_capture_barplot_font_size.Value = preferences.encoderCaptureBarplotFontSize;
		PreferencesWindowBox.check_show_start_and_duration.Active = preferences.encoderShowStartAndDuration;

		if(preferences.encoderCaptureCutByTriggers == Preferences.TriggerTypes.NO_TRIGGERS)
			PreferencesWindowBox.radio_encoder_triggers_no.Active = true;
		else {
			PreferencesWindowBox.radio_encoder_triggers_yes.Active = true;
			if(preferences.encoderCaptureCutByTriggers == Preferences.TriggerTypes.START_AT_CAPTURE)
				PreferencesWindowBox.radio_encoder_triggers_yes_start_at_capture.Active = true;
			else
				PreferencesWindowBox.radio_encoder_triggers_yes_start_at_first_trigger.Active = true;
		}

		if(preferences.encoderCaptureInfinite)
			PreferencesWindowBox.check_encoder_capture_infinite.Active = true;
		else
			PreferencesWindowBox.check_encoder_capture_infinite.Active = false;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "cont.png");
		PreferencesWindowBox.image_encoder_capture_infinite.Pixbuf = pixbuf;

		if(preferences.encoderRepetitionCriteriaGravitatory == Preferences.EncoderRepetitionCriteria.ECC_CON)
			PreferencesWindowBox.radio_encoder_rep_criteria_gravitatory_ecc_con.Active = true;
		else if(preferences.encoderRepetitionCriteriaGravitatory == Preferences.EncoderRepetitionCriteria.ECC)
			PreferencesWindowBox.radio_encoder_rep_criteria_gravitatory_ecc.Active = true;
		else // if(preferences.encoderRepetitionCriteriaGravitatory == Preferences.EncoderRepetitionCriteria.CON)
			PreferencesWindowBox.radio_encoder_rep_criteria_gravitatory_con.Active = true;

		if(preferences.encoderRepetitionCriteriaInertial == Preferences.EncoderRepetitionCriteria.ECC_CON)
			PreferencesWindowBox.radio_encoder_rep_criteria_inertial_ecc_con.Active = true;
		else if(preferences.encoderRepetitionCriteriaInertial == Preferences.EncoderRepetitionCriteria.ECC)
			PreferencesWindowBox.radio_encoder_rep_criteria_inertial_ecc.Active = true;
		else // if(preferences.encoderRepetitionCriteriaInertial == Preferences.EncoderRepetitionCriteria.CON)
			PreferencesWindowBox.radio_encoder_rep_criteria_inertial_con.Active = true;

		//encoder other -->
		PreferencesWindowBox.checkbutton_encoder_propulsive.Active = preferences.encoderPropulsive;

		if(preferences.encoderWorkKcal)
			PreferencesWindowBox.radio_encoder_work_kcal.Active = true;
		else
			PreferencesWindowBox.radio_encoder_work_joules.Active = true;

		if(preferences.encoderInertialGraphsX == Preferences.EncoderInertialGraphsXTypes.INERTIA_MOMENT)
			PreferencesWindowBox.radio_encoder_inertial_analyze_inertia_moment.Active = true;
		else if(preferences.encoderInertialGraphsX == Preferences.EncoderInertialGraphsXTypes.DIAMETER)
			PreferencesWindowBox.radio_encoder_inertial_analyze_diameter.Active = true;
		else
			PreferencesWindowBox.radio_encoder_inertial_analyze_equivalent_mass.Active = true;

		PreferencesWindowBox.spin_encoder_smooth_con.Value = preferences.encoderSmoothCon;

		if(preferences.encoder1RMMethod == Constants.Encoder1RMMethod.NONWEIGHTED)
			PreferencesWindowBox.radio_encoder_1RM_nonweighted.Active = true;
		else if(preferences.encoder1RMMethod == Constants.Encoder1RMMethod.WEIGHTED)
			PreferencesWindowBox.radio_encoder_1RM_weighted.Active = true;
		else if(preferences.encoder1RMMethod == Constants.Encoder1RMMethod.WEIGHTED2)
			PreferencesWindowBox.radio_encoder_1RM_weighted2.Active = true;
		else //(preferences.encoder1RMMethod == Constants.Encoder1RMMethod.WEIGHTED3)
			PreferencesWindowBox.radio_encoder_1RM_weighted3.Active = true;

		//done here and not in glade to be shown with the decimal point of user language	
		PreferencesWindowBox.label_encoder_con.Text = (0.7).ToString();

		//forceSensor -->

		PreferencesWindowBox.spin_force_sensor_capture_width_graph_seconds.Value = preferences.forceSensorCaptureWidthSeconds;

		if(preferences.forceSensorCaptureScroll)
			PreferencesWindowBox.radio_force_sensor_capture_scroll.Active = true;
		else
			PreferencesWindowBox.radio_force_sensor_capture_zoom_out.Active = true;

		PreferencesWindowBox.spin_force_sensor_elastic_ecc_min_displ.Value = preferences.forceSensorElasticEccMinDispl;
		PreferencesWindowBox.spin_force_sensor_elastic_con_min_displ.Value = preferences.forceSensorElasticConMinDispl;
		PreferencesWindowBox.spin_force_sensor_not_elastic_ecc_min_force.Value = preferences.forceSensorNotElasticEccMinForce;
		PreferencesWindowBox.spin_force_sensor_not_elastic_con_min_force.Value = preferences.forceSensorNotElasticConMinForce;

		PreferencesWindowBox.spin_force_sensor_graphs_line_width.Value = preferences.forceSensorGraphsLineWidth;

		if(preferences.forceSensorVariabilityMethod == Preferences.VariabilityMethodEnum.RMSSD)
		{
			PreferencesWindowBox.radio_force_sensor_variability_rmssd.Active = true;
			PreferencesWindowBox.hbox_force_sensor_lag.Visible = true;
		} else if(preferences.forceSensorVariabilityMethod == Preferences.VariabilityMethodEnum.CVRMSSD)
		{
			PreferencesWindowBox.radio_force_sensor_variability_cvrmssd.Active = true;
			PreferencesWindowBox.hbox_force_sensor_lag.Visible = true;
		} else if(preferences.forceSensorVariabilityMethod == Preferences.VariabilityMethodEnum.CV)
		{
			PreferencesWindowBox.radio_force_sensor_variability_cv.Active = true;
			PreferencesWindowBox.hbox_force_sensor_lag.Visible = false;
		} else {
			PreferencesWindowBox.radio_force_sensor_variability_old.Active = true;
			PreferencesWindowBox.hbox_force_sensor_lag.Visible = false;
		}

		PreferencesWindowBox.spin_force_sensor_variability_lag.Value = preferences.forceSensorVariabilityLag;

		PreferencesWindowBox.spin_force_sensor_analyze_ab_slider_increment.Value = preferences.forceSensorAnalyzeABSliderIncrement;
		PreferencesWindowBox.spin_force_sensor_analyze_max_avg_force_in_window.Value = preferences.forceSensorAnalyzeMaxAVGInWindow;

		//runEncoder -->
		PreferencesWindowBox.spin_run_encoder_acceleration.Value = preferences.runEncoderMinAccel;
		PreferencesWindowBox.spin_run_encoder_pps.Value = preferences.runEncoderPPS;
		PreferencesWindowBox.update_run_encoder_gui_pps_equivalence_and_max ();

		//language -->
		if(preferences.language == "")
			PreferencesWindowBox.radio_language_detected.Active = true;
		else
			PreferencesWindowBox.radio_language_force.Active = true;

		if(preferences.RGraphsTranslate)
			PreferencesWindowBox.radio_graphs_translate.Active = true;
		else
			PreferencesWindowBox.radio_graphs_no_translate.Active = true;
			
		//allow signal to be called
		PreferencesWindowBox.hbox_language_signalOn = true;
		
		if(preferences.useHeightsOnJumpIndexes)
			PreferencesWindowBox.radio_use_heights_on_jump_indexes.Active = true;
		else
			PreferencesWindowBox.radio_do_not_use_heights_on_jump_indexes.Active = true;

		if(preferences.importerPythonVersion == Preferences.pythonVersionEnum.Python2)
			PreferencesWindowBox.radio_python_2.Active = true;
		else //if(preferences.importerPythonVersion == Preferences.pythonVersionEnum.Python3)
			PreferencesWindowBox.radio_python_3.Active = true;

		PreferencesWindowBox.colorChoosedLastDefined = false;
		if(preferences.colorBackgroundOsColor) {
			PreferencesWindowBox.radio_color_os.Active = true;
			PreferencesWindowBox.button_color_choose.Sensitive = false;

			//do not show the visible tag at open the window, only when user changes to this option.
			PreferencesWindowBox.label_radio_color_os_needs_restart.Visible = false;
		}
		else if((preferences.colorBackgroundString).ToLower() == "#0e1e46") {
			PreferencesWindowBox.radio_color_chronojump_blue.Active = true;
			PreferencesWindowBox.button_color_choose.Sensitive = false;
		}
		else {
			PreferencesWindowBox.colorChoosedLast = preferences.colorBackground;
			PreferencesWindowBox.colorChoosedLastDefined = true;

			PreferencesWindowBox.radio_color_custom.Active = true;
			PreferencesWindowBox.button_color_choose.Sensitive = true;
		}

		PreferencesWindowBox.colorBackground = UtilGtk.ColorParse(preferences.colorBackgroundString);
		PreferencesWindowBox.paintColorChronojump ();
		PreferencesWindowBox.paintDrawingArea (PreferencesWindowBox.colorBackground);
		PreferencesWindowBox.paintBg (PreferencesWindowBox.colorBackground);


		//tabs selection widgets
		PreferencesWindowBox.image_view_more_tabs_close.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_close.png");
		PreferencesWindowBox.label_mandatory_tabs.Text = "<b>" + PreferencesWindowBox.label_mandatory_tabs.Text + "</b>";
		PreferencesWindowBox.label_mandatory_tabs.UseMarkup = true;
		PreferencesWindowBox.label_selectable_tabs.Text = "<b>" + PreferencesWindowBox.label_selectable_tabs.Text + "</b>";
		PreferencesWindowBox.label_selectable_tabs.UseMarkup = true;

		//help
		PreferencesWindowBox.image_help_close.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_close.png");

		PreferencesWindowBox.preferences_win.Show ();
		return PreferencesWindowBox;
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
		using (ColorSelectionDialog colorSelectionDialog = new ColorSelectionDialog (Catalog.GetString("Select color")))
		{
			colorSelectionDialog.TransientFor = preferences_win;
			colorSelectionDialog.ColorSelection.CurrentRgba = colorBackground;
			colorSelectionDialog.ColorSelection.HasPalette = true;

			if (colorSelectionDialog.Run () == (int) ResponseType.Ok)
			{
				// A) changes on preferences gui
				colorBackground = colorSelectionDialog.ColorSelection.CurrentRgba;

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

			colorSelectionDialog.Hide ();
		}
	}

	private void on_check_session_autoload_at_start_toggled (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		preferences.loadLastSessionAtStart = Preferences.PreferencesChange (
				false, SqlitePreferences.LoadLastSessionAtStart, preferences.loadLastSessionAtStart,
				PreferencesWindowBox.check_session_autoload_at_start.Active);

	}

	private void on_check_mode_autoload_at_start_toggled (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		preferences.loadLastModeAtStart = Preferences.PreferencesChange (
				false, SqlitePreferences.LoadLastModeAtStart, preferences.loadLastModeAtStart,
				PreferencesWindowBox.check_mode_autoload_at_start.Active);
	}

	private void on_check_logo_animated_toggled (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		preferences.logoAnimatedShow = Preferences.PreferencesChange(
				false, SqlitePreferences.LogoAnimatedShow, preferences.logoAnimatedShow,
				PreferencesWindowBox.check_logo_animated.Active);
	}

	private void on_check_rest_time_toggled (object o, EventArgs args)
	{
		// A) changes on preferences gui
		Pixbuf pixbuf;
		if(check_rest_time.Active)
		{
			hbox_rest_time_values.Visible = true;
			pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_rest.png");
		} else
		{
			hbox_rest_time_values.Visible = false;
			pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_rest_inactive.png");
		}
		PreferencesWindowBox.image_rest.Pixbuf = pixbuf;

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
		int minutes = (int) PreferencesWindowBox.spinbutton_rest_minutes.Value;
		int seconds = (int) PreferencesWindowBox.spinbutton_rest_seconds.Value;

		//if we had some time selected previously and now we selected no rest time
		if(preferences.restTimeMinutes >= 0 && ! PreferencesWindowBox.check_rest_time.Active)
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
		alignment_undecorated.Visible = check_appearance_maximized.Active;
		label_recommended_undecorated.Visible = check_appearance_maximized.Active;

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
		if( preferences.personWinHide != PreferencesWindowBox.check_appearance_person_win_hide.Active ) {
			SqlitePreferences.Update("personWinHide", PreferencesWindowBox.check_appearance_person_win_hide.Active.ToString(), false);
			preferences.personWinHide = PreferencesWindowBox.check_appearance_person_win_hide.Active;
			FakeButtonPersonWin.Click ();
		}
	}

	private void on_check_appearance_person_photo_toggled (object obj, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		if( preferences.personPhoto != PreferencesWindowBox.check_appearance_person_photo.Active ) {
			SqlitePreferences.Update("personPhoto", PreferencesWindowBox.check_appearance_person_photo.Active.ToString(), false);
			preferences.personPhoto = PreferencesWindowBox.check_appearance_person_photo.Active;
			FakeButtonPersonWin.Click ();
		}
	}

	/* callbacks SQL change at any change for tab: jumps */

	private void on_checkbutton_power_toggled (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		if( preferences.showPower != PreferencesWindowBox.checkbutton_power.Active ) {
			SqlitePreferences.Update("showPower", PreferencesWindowBox.checkbutton_power.Active.ToString(), false);
			preferences.showPower = PreferencesWindowBox.checkbutton_power.Active;
		}
	}
	private void on_checkbutton_stiffness_toggled (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		if( preferences.showStiffness != PreferencesWindowBox.checkbutton_stiffness.Active ) {
			SqlitePreferences.Update("showStiffness", PreferencesWindowBox.checkbutton_stiffness.Active.ToString(), false);
			preferences.showStiffness = PreferencesWindowBox.checkbutton_stiffness.Active;
		}
	}
	private void on_checkbutton_initial_speed_toggled (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		if( preferences.showInitialSpeed != PreferencesWindowBox.checkbutton_initial_speed.Active ) {
			SqlitePreferences.Update("showInitialSpeed", PreferencesWindowBox.checkbutton_initial_speed.Active.ToString(), false);
			preferences.showInitialSpeed = PreferencesWindowBox.checkbutton_initial_speed.Active;
		}
	}
	private void on_checkbutton_jump_rsi_toggled (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		if( preferences.showJumpRSI != PreferencesWindowBox.checkbutton_jump_rsi.Active ) {
			SqlitePreferences.Update(SqlitePreferences.ShowJumpRSI, PreferencesWindowBox.checkbutton_jump_rsi.Active.ToString(), false);
			preferences.showJumpRSI = PreferencesWindowBox.checkbutton_jump_rsi.Active;
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
		if(PreferencesWindowBox.checkbutton_show_tv_tc_index.Active) {
			preferences.showQIndex = Preferences.PreferencesChange(
					false, "showQIndex", preferences.showQIndex,
					PreferencesWindowBox.radiobutton_show_q_index.Active);
			preferences.showDjIndex = Preferences.PreferencesChange(
					false, "showDjIndex", preferences.showDjIndex,
					PreferencesWindowBox.radiobutton_show_dj_index.Active);
		} else {
			preferences.showQIndex = Preferences.PreferencesChange(
					false, "showQIndex", preferences.showQIndex, false);
			preferences.showDjIndex = Preferences.PreferencesChange(
					false, "showDjIndex", preferences.showDjIndex, false);
		}
	}


	private void on_radio_jumps_dj_heights_times_toggled (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		if ( ((Gtk.RadioButton) o).Active)
			preferences.heightPreferred = Preferences.PreferencesChange(
					false, "heightPreferred",
					preferences.heightPreferred,
					radio_jumps_dj_heights.Active);
	}

	private void on_radio_weight_percent_kg_toggled (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		if ( ((Gtk.RadioButton) o).Active)
			if( preferences.weightStatsPercent != PreferencesWindowBox.radio_weight_percent.Active ) {
				SqlitePreferences.Update("weightStatsPercent", PreferencesWindowBox.radio_weight_percent.Active.ToString(), false);
				preferences.weightStatsPercent = PreferencesWindowBox.radio_weight_percent.Active;
			}
	}
	private void on_radio_use_heights_or_not_on_jump_indexes_toggled (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		if ( ((Gtk.RadioButton) o).Active)
			if( preferences.useHeightsOnJumpIndexes != PreferencesWindowBox.radio_use_heights_on_jump_indexes.Active ) {
				SqlitePreferences.Update("useHeightsOnJumpIndexes",
						PreferencesWindowBox.radio_use_heights_on_jump_indexes.Active.ToString(), false);
				preferences.useHeightsOnJumpIndexes = PreferencesWindowBox.radio_use_heights_on_jump_indexes.Active;
			}
	}

	/* callbacks SQL change at any change for tab: races */

	private void on_radio_speed_ms_km_toggled (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		if ( ((Gtk.RadioButton) o).Active)
			if( preferences.metersSecondsPreferred != PreferencesWindowBox.radio_speed_ms.Active ) {
				SqlitePreferences.Update("metersSecondsPreferred", PreferencesWindowBox.radio_speed_ms.Active.ToString(), false);
				preferences.metersSecondsPreferred = PreferencesWindowBox.radio_speed_ms.Active;
			}
	}
	private void on_radio_runs_speed_start_arrival_leaving_toggled (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		if ( ((Gtk.RadioButton) o).Active)
			if( preferences.runSpeedStartArrival != PreferencesWindowBox.radio_runs_speed_start_arrival.Active ) {
				SqlitePreferences.Update("runSpeedStartArrival", PreferencesWindowBox.radio_runs_speed_start_arrival.Active.ToString(), false);
				preferences.runSpeedStartArrival = PreferencesWindowBox.radio_runs_speed_start_arrival.Active;
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
				! PreferencesWindowBox.checkbutton_runs_prevent_double_contact.Active)
		{
				SqlitePreferences.Update("runDoubleContactsMode", Constants.DoubleContact.NONE.ToString(), false);
				preferences.runDoubleContactsMode = Constants.DoubleContact.NONE;
		}
		else if(PreferencesWindowBox.checkbutton_runs_prevent_double_contact.Active)
		{
			if( preferences.runDoubleContactsMode != Constants.DoubleContact.BIGGEST_TC ) {
				SqlitePreferences.Update("runDoubleContactsMode", Constants.DoubleContact.BIGGEST_TC.ToString(), false);
				preferences.runDoubleContactsMode = Constants.DoubleContact.BIGGEST_TC;
			}

			if(preferences.runDoubleContactsMS != (int) PreferencesWindowBox.spinbutton_runs_prevent_double_contact.Value) {
				SqlitePreferences.Update("runDoubleContactsMS",
						PreferencesWindowBox.spinbutton_runs_prevent_double_contact.Value.ToString(), false); //saved as string
				preferences.runDoubleContactsMS = (int) spinbutton_runs_prevent_double_contact.Value;
			}
		}
	}

	private void changeRunIntervalDoubleContactOnPreferencesAndDB ()
	{
		//2.1 was FIRST or AVERAGE or LAST and now will be NONE
		if( (preferences.runIDoubleContactsMode != Constants.DoubleContact.NONE) &&
				! PreferencesWindowBox.checkbutton_runs_i_prevent_double_contact.Active)
		{
				SqlitePreferences.Update("runIDoubleContactsMode", Constants.DoubleContact.NONE.ToString(), false);
				preferences.runIDoubleContactsMode = Constants.DoubleContact.NONE;
		}
		else if(PreferencesWindowBox.checkbutton_runs_i_prevent_double_contact.Active)
		{
			if( preferences.runIDoubleContactsMode != Constants.DoubleContact.BIGGEST_TC ) {
				SqlitePreferences.Update("runIDoubleContactsMode", Constants.DoubleContact.BIGGEST_TC.ToString(), false);
				preferences.runIDoubleContactsMode = Constants.DoubleContact.BIGGEST_TC;
			}

			if(preferences.runIDoubleContactsMS != (int) PreferencesWindowBox.spinbutton_runs_i_prevent_double_contact.Value) {
				SqlitePreferences.Update("runIDoubleContactsMS",
						PreferencesWindowBox.spinbutton_runs_i_prevent_double_contact.Value.ToString(), false); //saved as string
				preferences.runIDoubleContactsMS = (int) spinbutton_runs_i_prevent_double_contact.Value;
			}
		}
	}

	/* callbacks SQL change at any change for tab: encoder - capture */

	private void on_spin_encoder_capture_time_value_changed (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		preferences.encoderCaptureTime = Preferences.PreferencesChange(
				false, "encoderCaptureTime",
				preferences.encoderCaptureTime,
				(int) PreferencesWindowBox.spin_encoder_capture_time.Value);
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
		if(! PreferencesWindowBox.check_encoder_capture_inactivity_end_time.Active)
		{
			SqlitePreferences.Update("encoderCaptureInactivityEndTime", "-1", false);
			preferences.encoderCaptureInactivityEndTime = -1;
		} else {
			preferences.encoderCaptureInactivityEndTime = Preferences.PreferencesChange(
					false, "encoderCaptureInactivityEndTime",
					preferences.encoderCaptureInactivityEndTime,
					(int) PreferencesWindowBox.spin_encoder_capture_inactivity_end_time.Value);
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
		int spinEncoderCaptureDiscardFirstN = Convert.ToInt32(PreferencesWindowBox.spin_encoder_capture_inertial_discard_first_n.Value);
		if(! checkbutton_encoder_capture_inertial_discard_first_n.Active)
			spinEncoderCaptureDiscardFirstN = 0;

		if(spinEncoderCaptureDiscardFirstN != preferences.encoderCaptureInertialDiscardFirstN)
		{
			SqlitePreferences.Update("encoderCaptureInertialDiscardFirstN", spinEncoderCaptureDiscardFirstN.ToString(), false);
			preferences.encoderCaptureInertialDiscardFirstN = spinEncoderCaptureDiscardFirstN;
		}
	}

	private void on_radio_encoder_rep_criteria_gravitatory_toggled (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		//radio_encoder_rep_criteria_gravitatory_*
		if(PreferencesWindowBox.radio_encoder_rep_criteria_gravitatory_ecc_con.Active &&
				preferences.encoderRepetitionCriteriaGravitatory != Preferences.EncoderRepetitionCriteria.ECC_CON)
		{
			SqlitePreferences.Update(SqlitePreferences.EncoderRepetitionCriteriaGravitatoryStr,
					Preferences.EncoderRepetitionCriteria.ECC_CON.ToString(), false);
			preferences.encoderRepetitionCriteriaGravitatory = Preferences.EncoderRepetitionCriteria.ECC_CON;
		}
		else if(PreferencesWindowBox.radio_encoder_rep_criteria_gravitatory_ecc.Active &&
				preferences.encoderRepetitionCriteriaGravitatory != Preferences.EncoderRepetitionCriteria.ECC)
		{
			SqlitePreferences.Update(SqlitePreferences.EncoderRepetitionCriteriaGravitatoryStr,
					Preferences.EncoderRepetitionCriteria.ECC.ToString(), false);
			preferences.encoderRepetitionCriteriaGravitatory = Preferences.EncoderRepetitionCriteria.ECC;
		}
		else if(PreferencesWindowBox.radio_encoder_rep_criteria_gravitatory_con.Active &&
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
		if(PreferencesWindowBox.radio_encoder_rep_criteria_inertial_ecc_con.Active &&
				preferences.encoderRepetitionCriteriaInertial != Preferences.EncoderRepetitionCriteria.ECC_CON)
		{
			SqlitePreferences.Update(SqlitePreferences.EncoderRepetitionCriteriaInertialStr,
					Preferences.EncoderRepetitionCriteria.ECC_CON.ToString(), false);
			preferences.encoderRepetitionCriteriaInertial = Preferences.EncoderRepetitionCriteria.ECC_CON;
		}
		else if(PreferencesWindowBox.radio_encoder_rep_criteria_inertial_ecc.Active &&
				preferences.encoderRepetitionCriteriaInertial != Preferences.EncoderRepetitionCriteria.ECC)
		{
			SqlitePreferences.Update(SqlitePreferences.EncoderRepetitionCriteriaInertialStr,
					Preferences.EncoderRepetitionCriteria.ECC.ToString(), false);
			preferences.encoderRepetitionCriteriaInertial = Preferences.EncoderRepetitionCriteria.ECC;
		}
		else if(PreferencesWindowBox.radio_encoder_rep_criteria_inertial_con.Active &&
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
		if( preferences.encoderCaptureShowNRepetitions > 0 && PreferencesWindowBox.radio_encoder_capture_show_all_bars.Active )
		{
			SqlitePreferences.Update("encoderCaptureShowNRepetitions", "-1", false);
			preferences.encoderCaptureShowNRepetitions = -1;
		}
		else if( PreferencesWindowBox.radio_encoder_capture_show_only_some_bars.Active &&
				preferences.encoderCaptureShowNRepetitions != (int) PreferencesWindowBox.spin_encoder_capture_show_only_some_bars.Value) {
			SqlitePreferences.Update("encoderCaptureShowNRepetitions",
					PreferencesWindowBox.spin_encoder_capture_show_only_some_bars.Value.ToString(), false);
			preferences.encoderCaptureShowNRepetitions = (int) PreferencesWindowBox.spin_encoder_capture_show_only_some_bars.Value;
		}
	}

	private void on_spin_encoder_capture_barplot_font_size_value_changed (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		preferences.encoderCaptureBarplotFontSize = Preferences.PreferencesChange(
				false, "encoderCaptureBarplotFontSize",
				preferences.encoderCaptureBarplotFontSize,
				(int) PreferencesWindowBox.spin_encoder_capture_barplot_font_size.Value);
	}

	private void on_check_show_start_and_duration_clicked (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		preferences.encoderShowStartAndDuration = Preferences.PreferencesChange(
				false, "encoderShowStartAndDuration",
				preferences.encoderShowStartAndDuration,
				PreferencesWindowBox.check_show_start_and_duration.Active);
	}

	private void on_radio_encoder_triggers_toggled (object o, EventArgs args)
	{
		// A) changes on preferences gui
		Pixbuf pixbuf;
		if(radio_encoder_triggers_no.Active)
		{
			vbox_encoder_triggers_yes.Visible = false;
			pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_encoder_triggers_no.png");
		PreferencesWindowBox.image_encoder_triggers.Pixbuf = pixbuf;
		} else {
			vbox_encoder_triggers_yes.Visible = true;
			pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_encoder_triggers.png");
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
		if(PreferencesWindowBox.radio_encoder_triggers_no.Active &&
				preferences.encoderCaptureCutByTriggers != Preferences.TriggerTypes.NO_TRIGGERS)
		{
			SqlitePreferences.Update("encoderCaptureCutByTriggers", Preferences.TriggerTypes.NO_TRIGGERS.ToString(), false);
			preferences.encoderCaptureCutByTriggers = Preferences.TriggerTypes.NO_TRIGGERS;
		}
		else if(PreferencesWindowBox.radio_encoder_triggers_yes.Active &&
				PreferencesWindowBox.radio_encoder_triggers_yes_start_at_capture.Active &&
				preferences.encoderCaptureCutByTriggers != Preferences.TriggerTypes.START_AT_CAPTURE)
		{
			SqlitePreferences.Update("encoderCaptureCutByTriggers", Preferences.TriggerTypes.START_AT_CAPTURE.ToString(), false);
			preferences.encoderCaptureCutByTriggers = Preferences.TriggerTypes.START_AT_CAPTURE;
		}
		else if(PreferencesWindowBox.radio_encoder_triggers_yes.Active &&
				PreferencesWindowBox.radio_encoder_triggers_yes_start_at_first_trigger.Active &&
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
				PreferencesWindowBox.check_encoder_capture_infinite.Active);
	}


	/* callbacks SQL change at any change for tab: encoder - other */

	private void on_checkbutton_encoder_propulsive_clicked (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		preferences.encoderPropulsive = Preferences.PreferencesChange(
				false, "encoderPropulsive",
				preferences.encoderPropulsive,
				PreferencesWindowBox.checkbutton_encoder_propulsive.Active);
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
		if(PreferencesWindowBox.radio_encoder_inertial_analyze_inertia_moment.Active)
			return Preferences.EncoderInertialGraphsXTypes.INERTIA_MOMENT;
		else if(PreferencesWindowBox.radio_encoder_inertial_analyze_diameter.Active)
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
				(double) PreferencesWindowBox.spin_encoder_smooth_con.Value);
	}
	private void on_radio_encoder_1RM_weight_toggled (object o, EventArgs args)
	{
		Constants.Encoder1RMMethod encoder1RMMethod;
		if(PreferencesWindowBox.radio_encoder_1RM_nonweighted.Active)
			encoder1RMMethod = Constants.Encoder1RMMethod.NONWEIGHTED;
		else if(PreferencesWindowBox.radio_encoder_1RM_weighted.Active)
			encoder1RMMethod = Constants.Encoder1RMMethod.WEIGHTED;
		else if(PreferencesWindowBox.radio_encoder_1RM_weighted2.Active)
			encoder1RMMethod = Constants.Encoder1RMMethod.WEIGHTED2;
		else // (PreferencesWindowBox.radio_encoder_1RM_weighted3.Active)
			encoder1RMMethod = Constants.Encoder1RMMethod.WEIGHTED3;

		if(preferences.encoder1RMMethod != encoder1RMMethod) {
			SqlitePreferences.Update("encoder1RMMethod", encoder1RMMethod.ToString(), false);
			preferences.encoder1RMMethod = encoder1RMMethod;
		}
	}


	/* callbacks SQL change at any change for tab: forceSensor */

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
		if(PreferencesWindowBox.radio_force_sensor_variability_rmssd.Active &&
				preferences.forceSensorVariabilityMethod != Preferences.VariabilityMethodEnum.RMSSD)
		{
			SqlitePreferences.Update(SqlitePreferences.ForceSensorVariabilityMethod, Preferences.VariabilityMethodEnum.RMSSD.ToString(), false);
			preferences.forceSensorVariabilityMethod = Preferences.VariabilityMethodEnum.RMSSD;
		}
		else if(PreferencesWindowBox.radio_force_sensor_variability_cvrmssd.Active &&
				preferences.forceSensorVariabilityMethod != Preferences.VariabilityMethodEnum.CVRMSSD)
		{
			SqlitePreferences.Update(SqlitePreferences.ForceSensorVariabilityMethod, Preferences.VariabilityMethodEnum.CVRMSSD.ToString(), false);
			preferences.forceSensorVariabilityMethod = Preferences.VariabilityMethodEnum.CVRMSSD;
		}
		else if(PreferencesWindowBox.radio_force_sensor_variability_cv.Active &&
				preferences.forceSensorVariabilityMethod != Preferences.VariabilityMethodEnum.CV)
		{
			SqlitePreferences.Update(SqlitePreferences.ForceSensorVariabilityMethod, Preferences.VariabilityMethodEnum.CV.ToString(), false);
			preferences.forceSensorVariabilityMethod = Preferences.VariabilityMethodEnum.CV;
		}
		else if(PreferencesWindowBox.radio_force_sensor_variability_old.Active &&
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
	private void on_spin_force_sensor_analyze_ab_slider_increment_value_changed (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		preferences.forceSensorAnalyzeABSliderIncrement = Preferences.PreferencesChange(
				false,
				SqlitePreferences.ForceSensorAnalyzeABSliderIncrement,
				preferences.forceSensorAnalyzeABSliderIncrement,
				Convert.ToDouble(spin_force_sensor_analyze_ab_slider_increment.Value));
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
		if( preferences.volumeOn != PreferencesWindowBox.checkbutton_volume.Active ) {
			SqlitePreferences.Update ("volumeOn", PreferencesWindowBox.checkbutton_volume.Active.ToString(), false);
			preferences.volumeOn = PreferencesWindowBox.checkbutton_volume.Active;
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

	private void on_radio_language_toggled (object obj, EventArgs args)
	{
		// A) changes on preferences gui
		hbox_combo_language.Sensitive = radio_language_force.Active;

		if(hbox_language_signalOn)
			vbox_need_restart.Visible = true;

		// B) changes on preferences object and SqlitePreferences
		changeLanguageOnPreferencesAndDB ();
	}
	private	void combo_language_changed (object obj, EventArgs args)
	{
		// A) changes on preferences gui
		if(hbox_language_signalOn)
			vbox_need_restart.Visible = true;

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
				PreferencesWindowBox.radio_export_latin.Active)
		{
			SqlitePreferences.Update ("CSVExportDecimalSeparator","COMMA", false);
			preferences.CSVExportDecimalSeparator = "COMMA";
		}
		else if (preferences.CSVExportDecimalSeparator == "COMMA" &&
				! PreferencesWindowBox.radio_export_latin.Active)
		{
			SqlitePreferences.Update ("CSVExportDecimalSeparator","POINT", false);
			preferences.CSVExportDecimalSeparator = "POINT";
		}
	}

	private void on_radio_translate_toggled (object obj, EventArgs args)
	{
		// A) changes on preferences gui
		if(hbox_language_signalOn)
			vbox_need_restart.Visible = true;

		// B) changes on preferences object and SqlitePreferences
		if (preferences.RGraphsTranslate != PreferencesWindowBox.radio_graphs_translate.Active) {
			SqlitePreferences.Update ("RGraphsTranslate",
					PreferencesWindowBox.radio_graphs_translate.Active.ToString(), false);
			preferences.RGraphsTranslate = PreferencesWindowBox.radio_graphs_translate.Active;
		}
	}

	/* callbacks SQL change at any change for tab: advanced */

	private void on_check_networks_devices_clicked (object o, EventArgs args)
	{
		// this is not stored in SQL. used on networks
		preferences.networksAllowChangeDevices = PreferencesWindowBox.check_networks_devices.Active;
	}

	private void on_checkbutton_ask_deletion_clicked (object o, EventArgs args)
	{
		// B) changes on preferences object and SqlitePreferences
		if (preferences.askDeletion != PreferencesWindowBox.checkbutton_ask_deletion.Active) {
			SqlitePreferences.Update ("askDeletion", PreferencesWindowBox.checkbutton_ask_deletion.Active.ToString(), false);
			preferences.askDeletion = PreferencesWindowBox.checkbutton_ask_deletion.Active;
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

	private void on_radio_font_courier_toggled (object o, EventArgs args)
	{
		// A) changes on preferences gui
		label_radio_font_needs_restart.Visible = true;

		// B) changes on preferences object and SqlitePreferences
		changeFontOnPreferencesAndDB ();
	}
	private void on_radio_font_helvetica_toggled (object o, EventArgs args)
	{
		// A) changes on preferences gui
		label_radio_font_needs_restart.Visible = true;

		// B) changes on preferences object and SqlitePreferences
		changeFontOnPreferencesAndDB ();
	}
	private void changeFontOnPreferencesAndDB ()
	{
		if (preferences.fontType == Preferences.FontTypes.Courier && radio_font_helvetica.Active)
		{
			SqlitePreferences.Update (SqlitePreferences.FontsOnGraphs, Preferences.FontTypes.Helvetica.ToString(), false);
			preferences.fontType = Preferences.FontTypes.Helvetica;
		}
		else if (preferences.fontType == Preferences.FontTypes.Helvetica && radio_font_courier.Active)
		{
			SqlitePreferences.Update (SqlitePreferences.FontsOnGraphs, Preferences.FontTypes.Courier.ToString(), false);
			preferences.fontType = Preferences.FontTypes.Courier;
		}
	}

	private void on_checkbutton_mute_logs_clicked (object o, EventArgs args)
	{
		/* disabled. Only false since 2.3.0-2

		// B) changes on preferences object and SqlitePreferences
		if (preferences.muteLogs != PreferencesWindowBox.checkbutton_mute_logs.Active) {
			SqlitePreferences.Update ("muteLogs", PreferencesWindowBox.checkbutton_mute_logs.Active.ToString(), false);
			preferences.muteLogs = PreferencesWindowBox.checkbutton_mute_logs.Active;
		}
		*/
	}

	// view more tabs ---->

	private void on_button_view_more_tabs_clicked (object o, EventArgs args)
	{
		PreferencesWindowBox.notebook_top.CurrentPage = Convert.ToInt32(notebook_top_pages.SELECTTABS);
		hbox_buttons_bottom.Sensitive = false;
	}
	private void on_button_view_more_tabs_close_clicked (object o, EventArgs args)
	{
		PreferencesWindowBox.notebook_top.CurrentPage = Convert.ToInt32(notebook_top_pages.PREFERENCES);
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
			PreferencesWindowBox.notebook.GetNthPage(page).Show();
			PreferencesWindowBox.notebook.CurrentPage = page;

		} else
			PreferencesWindowBox.notebook.GetNthPage(page).Hide();
	}

	// <---- endo of view more tabs

	// help ---->

	private enum helpTypes { NORMAL, STIFFNESS }
	//does not use markup on textview
	private void showHelp (string title, helpTypes helpType, string message)
	{
		preferences_win.Title = Catalog.GetString("Preferences") + " / " + Catalog.GetString("Help:") + " " + title;
		PreferencesWindowBox.notebook_top.CurrentPage = Convert.ToInt32(notebook_top_pages.HELP);
		hbox_buttons_bottom.Sensitive = false;

		hbox_stiffness_formula.Visible = (helpType == helpTypes.STIFFNESS);

		textview_help_message.Buffer.Text = message;
	}

	private void on_button_help_close_clicked (object o, EventArgs args)
	{
		preferences_win.Title = Catalog.GetString("Preferences");
		PreferencesWindowBox.notebook_top.CurrentPage = Convert.ToInt32(notebook_top_pages.PREFERENCES);
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
		UtilGtk.WidgetColor (notebook_top, Config.ColorBackgroundShifted);
		UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundShiftedIsDark, notebook_top);

		//notebook
		UtilGtk.WidgetColor (notebook, Config.ColorBackgroundShifted);
		UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundShiftedIsDark, notebook);

		//notebook_races
		UtilGtk.WidgetColor (notebook_races, Config.ColorBackgroundShifted);
		UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundShiftedIsDark, notebook_races);

		//notebook_races_double_contacts
		UtilGtk.WidgetColor (notebook_races_double_contacts, Config.ColorBackgroundShifted);
		UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundShiftedIsDark, notebook_races_double_contacts);

		//notebook_force_sensor
		UtilGtk.WidgetColor (notebook_force_sensor, Config.ColorBackgroundShifted);
		UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundShiftedIsDark, notebook_force_sensor);

		//notebook_encoder
		UtilGtk.WidgetColor (notebook_encoder, Config.ColorBackgroundShifted);
		UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundShiftedIsDark, notebook_encoder);

		//notebook_multimedia
		UtilGtk.WidgetColor (notebook_multimedia, Config.ColorBackgroundShifted);
		UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundShiftedIsDark, notebook_multimedia);
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
		LogB.Information (UtilGtk.GetRGBA (UtilGtk.Colors.BLUE_CHRONOJUMP).Red.ToString());
		LogB.Information (UtilGtk.GetRGBA (UtilGtk.Colors.BLUE_CHRONOJUMP).Green.ToString());
		LogB.Information (UtilGtk.GetRGBA (UtilGtk.Colors.BLUE_CHRONOJUMP).Blue.ToString());
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
	private void on_button_jumps_dj_heights_times_help_clicked (object o, EventArgs args) {
		showHelp(Catalog.GetString("Variables to show at reactive jumps:"), helpTypes.NORMAL,
				Catalog.GetString("In jumps like Dj, Dja you can select to show the heights of the jumps or the times.") + "\n\n" +
				Catalog.GetString("But keep in mind that if you display reactive jumps with other jumps (like CMJ, ABK), then heights will be shown."));
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
		PreferencesWindowBox.createComboCamera(preferences.videoDevice,
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
			wfsm = new WebcamFfmpegSupportedModesWindows(cameraCode);
		else
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
	bool hbox_language_signalOn = false;
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
		if( preferences.showAngle != PreferencesWindowBox.checkbutton_angle.Active ) {
			SqlitePreferences.Update("showAngle", PreferencesWindowBox.checkbutton_angle.Active.ToString(), false);
			preferences.showAngle = PreferencesWindowBox.checkbutton_angle.Active;
		}
		*/

		PreferencesWindowBox.preferences_win.Hide();
		PreferencesWindowBox = null;
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
			PreferencesWindowBox.preferences_win.Hide();
			PreferencesWindowBox = null;
//		}
	}

	/*
	 * TODO: problem is database stored is a chronojump.db or a folder (if images and videos were saved).
	 * FileChooserAction only lets you use one type
	 * In the future backup db as tgz or similar

	 void on_button_db_restore_clicked (object o, EventArgs args)
	 {
		fc = new Gtk.FileChooserDialog(Catalog.GetString("Restore database from:"),
			preferences_win,
			FileChooserAction.SelectFolder,
			Catalog.GetString("Cancel"),ResponseType.Cancel,
			Catalog.GetString("Restore"),ResponseType.Accept
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
		Gtk.FileChooserDialog fc = new Gtk.FileChooserDialog(Catalog.GetString("Import configuration file"),
				preferences_win,
				FileChooserAction.Open,
				Catalog.GetString("Cancel"),ResponseType.Cancel,
				Catalog.GetString("Import"),ResponseType.Accept
				);
		
		fc.Filter = new FileFilter();
		//it can handle future archives like: chronojump_config_SOME_VENDOR.txt
		//and it will be copied to chronojump_config.txt
		fc.Filter.AddPattern("chronojump_config*.txt");
	
		bool success = false;	
		if (fc.Run() == (int)ResponseType.Accept) 
		{
			try {
				File.Copy(fc.Filename, Util.GetConfigFileName(), true);
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
		//Don't forget to call Destroy() or the FileChooserDialog window won't get closed.
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

	// <---- end SQL stress tests ----


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

	private void on_button_test_bluetooth_clicked (object o, EventArgs args)
	{
	}

	private Preferences.MaximizedTypes get_maximized_from_gui()
	{
		if( ! PreferencesWindowBox.check_appearance_maximized.Active )
			return Preferences.MaximizedTypes.NO;

		if( ! PreferencesWindowBox.check_appearance_maximized_undecorated.Active )
			return Preferences.MaximizedTypes.YES;

		return Preferences.MaximizedTypes.YESUNDECORATED;
	}

	private Preferences.pythonVersionEnum get_pythonVersion_from_gui()
	{
		if( PreferencesWindowBox.radio_python_2.Active)
			return Preferences.pythonVersionEnum.Python2;
		else //if( PreferencesWindowBox.radio_python_3.Active)
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

		//appearance tab
		check_appearance_maximized = (Gtk.CheckButton) builder.GetObject ("check_appearance_maximized");
		check_appearance_maximized_undecorated = (Gtk.CheckButton) builder.GetObject ("check_appearance_maximized_undecorated");
		check_appearance_person_win_hide = (Gtk.CheckButton) builder.GetObject ("check_appearance_person_win_hide");
		check_appearance_person_photo = (Gtk.CheckButton) builder.GetObject ("check_appearance_person_photo");
		alignment_undecorated = (Gtk.Alignment) builder.GetObject ("alignment_undecorated");
		label_recommended_undecorated = (Gtk.Label) builder.GetObject ("label_recommended_undecorated");
		radio_font_courier = (Gtk.RadioButton) builder.GetObject ("radio_font_courier");
		radio_font_helvetica = (Gtk.RadioButton) builder.GetObject ("radio_font_helvetica");
		label_radio_font_needs_restart = (Gtk.Label) builder.GetObject ("label_radio_font_needs_restart");
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
		image_button_jumps_dj_heights_times_help = (Gtk.Image) builder.GetObject ("image_button_jumps_dj_heights_times_help");
		checkbutton_initial_speed = (Gtk.CheckButton) builder.GetObject ("checkbutton_initial_speed");
		checkbutton_jump_rsi = (Gtk.CheckButton) builder.GetObject ("checkbutton_jump_rsi");
		//	checkbutton_angle = (Gtk.CheckButton) builder.GetObject ("checkbutton_angle");
		checkbutton_show_tv_tc_index = (Gtk.CheckButton) builder.GetObject ("checkbutton_show_tv_tc_index");
		hbox_indexes = (Gtk.Box) builder.GetObject ("hbox_indexes");
		radiobutton_show_q_index = (Gtk.RadioButton) builder.GetObject ("radiobutton_show_q_index");
		radiobutton_show_dj_index = (Gtk.RadioButton) builder.GetObject ("radiobutton_show_dj_index");
		radio_jumps_dj_heights = (Gtk.RadioButton) builder.GetObject ("radio_jumps_dj_heights");
		radio_jumps_dj_times = (Gtk.RadioButton) builder.GetObject ("radio_jumps_dj_times");
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
		spin_force_sensor_analyze_ab_slider_increment = (Gtk.SpinButton) builder.GetObject ("spin_force_sensor_analyze_ab_slider_increment");
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
		vbox_need_restart = (Gtk.VBox) builder.GetObject ("vbox_need_restart");

		//advanced tab
		checkbutton_ask_deletion = (Gtk.CheckButton) builder.GetObject ("checkbutton_ask_deletion");
		box_combo_decimals = (Gtk.Box) builder.GetObject ("box_combo_decimals");
		checkbutton_mute_logs = (Gtk.CheckButton) builder.GetObject ("checkbutton_mute_logs");
		radio_export_latin = (Gtk.RadioButton) builder.GetObject ("radio_export_latin");
		radio_export_non_latin = (Gtk.RadioButton) builder.GetObject ("radio_export_non_latin");
		label_advanced_feedback = (Gtk.Label) builder.GetObject ("label_advanced_feedback");
		toggle_gc_collect_on_close = (Gtk.ToggleButton) builder.GetObject ("toggle_gc_collect_on_close");
		toggle_never_close = (Gtk.ToggleButton) builder.GetObject ("toggle_never_close");
		vbox_version = (Gtk.VBox) builder.GetObject ("vbox_version");
		label_progVersion = (Gtk.Label) builder.GetObject ("label_progVersion");
		frame_networks = (Gtk.Frame) builder.GetObject ("frame_networks");
		check_networks_devices = (Gtk.CheckButton) builder.GetObject ("check_networks_devices");
		button_debug_mode = (Gtk.Button) builder.GetObject ("button_debug_mode");

		radio_python_2 = (Gtk.RadioButton) builder.GetObject ("radio_python_2");
		radio_python_3 = (Gtk.RadioButton) builder.GetObject ("radio_python_3");

		button_close = (Gtk.Button) builder.GetObject ("button_close");
		image_button_close = (Gtk.Image) builder.GetObject ("image_button_close");
		combo_decimals = (Gtk.ComboBoxText) builder.GetObject ("combo_decimals");
	}
}
