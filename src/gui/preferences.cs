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
 * Copyright (C) 2004-2021   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.IO;
using Gdk;
using Gtk;
using Glade;
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
	[Widget] Gtk.Window preferences_win;
	[Widget] Gtk.Notebook notebook_top;
	[Widget] Gtk.Notebook notebook;
	[Widget] Gtk.HBox hbox_buttons_bottom;

	//view more tabs
	[Widget] Gtk.CheckButton check_view_jumps;
	[Widget] Gtk.CheckButton check_view_runs;
	[Widget] Gtk.CheckButton check_view_encoder;
	[Widget] Gtk.CheckButton check_view_force_sensor;
	[Widget] Gtk.CheckButton check_view_race_analyzer;
	//tabs selection widgets
	[Widget] Gtk.Image image_view_more_tabs_close;
	[Widget] Gtk.Label label_mandatory_tabs;
	[Widget] Gtk.Label label_selectable_tabs;

	//help widgets
	[Widget] Gtk.HBox hbox_stiffness_formula;
	[Widget] Gtk.TextView textview_help_message;
	[Widget] Gtk.Image image_help_close;

	//appearance tab
	[Widget] Gtk.CheckButton check_appearance_maximized;
	[Widget] Gtk.CheckButton check_appearance_maximized_undecorated;
	[Widget] Gtk.CheckButton check_appearance_person_win_hide;
	[Widget] Gtk.CheckButton check_appearance_person_photo;
	[Widget] Gtk.Alignment alignment_undecorated;
	[Widget] Gtk.Label label_recommended_undecorated;
	[Widget] Gtk.RadioButton radio_font_courier;
	[Widget] Gtk.RadioButton radio_font_helvetica;
	[Widget] Gtk.Label label_radio_font_needs_restart;
	[Widget] Gtk.CheckButton check_rest_time;
	[Widget] Gtk.Image image_rest;
	[Widget] Gtk.HBox hbox_rest_time_values;
	[Widget] Gtk.SpinButton spinbutton_rest_minutes;
	[Widget] Gtk.SpinButton spinbutton_rest_seconds;

	[Widget] Gtk.RadioButton radio_color_custom;
	[Widget] Gtk.RadioButton radio_color_chronojump_blue;
	[Widget] Gtk.RadioButton radio_color_os;
	[Widget] Gtk.DrawingArea drawingarea_background_color;
	[Widget] Gtk.Button button_color_choose;
	[Widget] Gtk.DrawingArea drawingarea_background_color_chronojump_blue;
	[Widget] Gtk.Label label_radio_color_os_needs_restart;

	[Widget] Gtk.CheckButton check_logo_animated;
	[Widget] Gtk.HBox hbox_last_session_and_mode;
	[Widget] Gtk.CheckButton check_session_autoload_at_start;
	[Widget] Gtk.CheckButton check_mode_autoload_at_start;


	//jumps tab	
//	[Widget] Gtk.Label label_jumps;
	[Widget] Gtk.CheckButton checkbutton_power;
	[Widget] Gtk.CheckButton checkbutton_stiffness;
	[Widget] Gtk.Image image_jumps_power_help;
	[Widget] Gtk.Image image_jumps_stiffness_help;
	[Widget] Gtk.CheckButton checkbutton_initial_speed;
	[Widget] Gtk.CheckButton checkbutton_jump_rsi;
//	[Widget] Gtk.CheckButton checkbutton_angle;
	[Widget] Gtk.CheckButton checkbutton_show_tv_tc_index;
	[Widget] Gtk.Box hbox_indexes;
	[Widget] Gtk.RadioButton radiobutton_show_q_index;
	[Widget] Gtk.RadioButton radiobutton_show_dj_index;
	[Widget] Gtk.RadioButton radio_jumps_dj_heights;
	[Widget] Gtk.RadioButton radio_jumps_dj_times;
	[Widget] Gtk.RadioButton radio_weight_percent;
	[Widget] Gtk.RadioButton radio_weight_kg;
	[Widget] Gtk.RadioButton radio_use_heights_on_jump_indexes;
	[Widget] Gtk.RadioButton radio_do_not_use_heights_on_jump_indexes;
			
	//runs tab	
	[Widget] Gtk.Image image_run_speed_start_help;
	[Widget] Gtk.RadioButton radio_speed_ms;
	[Widget] Gtk.RadioButton radio_speed_km;
	[Widget] Gtk.RadioButton radio_runs_speed_start_arrival; 
	[Widget] Gtk.RadioButton radio_runs_speed_start_leaving; 
	[Widget] Gtk.Image image_races_simple;
	[Widget] Gtk.Image image_races_intervallic;
	[Widget] Gtk.Notebook notebook_races_double_contacts;
	[Widget] Gtk.Box vbox_runs_prevent_double_contact;
	[Widget] Gtk.CheckButton checkbutton_runs_prevent_double_contact;
	[Widget] Gtk.SpinButton spinbutton_runs_prevent_double_contact;
	[Widget] Gtk.Box vbox_runs_i_prevent_double_contact;
	[Widget] Gtk.CheckButton checkbutton_runs_i_prevent_double_contact;
	[Widget] Gtk.SpinButton spinbutton_runs_i_prevent_double_contact;
	
	//encoder capture tab
	[Widget] Gtk.SpinButton spin_encoder_capture_time;
	[Widget] Gtk.CheckButton check_encoder_capture_inactivity_end_time;
	[Widget] Gtk.HBox hbox_encoder_capture_inactivity_time;
	[Widget] Gtk.SpinButton spin_encoder_capture_inactivity_end_time;
	[Widget] Gtk.Image image_encoder_gravitatory;
	[Widget] Gtk.Image image_encoder_inertial;
	[Widget] Gtk.Image image_encoder_triggers;
	[Widget] Gtk.Notebook notebook_encoder_capture_gi;
	[Widget] Gtk.SpinButton spin_encoder_capture_min_height_gravitatory;
	[Widget] Gtk.SpinButton spin_encoder_capture_min_height_inertial;
	[Widget] Gtk.CheckButton checkbutton_encoder_capture_inertial_discard_first_n;
	[Widget] Gtk.HBox hbox_encoder_capture_inertial_discard_first_n;
	[Widget] Gtk.SpinButton spin_encoder_capture_inertial_discard_first_n;
	[Widget] Gtk.SpinButton spin_encoder_capture_show_only_some_bars;
	[Widget] Gtk.RadioButton radio_encoder_capture_show_all_bars;
	[Widget] Gtk.RadioButton radio_encoder_capture_show_only_some_bars;
	[Widget] Gtk.SpinButton spin_encoder_capture_barplot_font_size;
	[Widget] Gtk.CheckButton check_show_start_and_duration;
	[Widget] Gtk.RadioButton radio_encoder_triggers_no;
	[Widget] Gtk.RadioButton radio_encoder_triggers_yes;
	[Widget] Gtk.VBox vbox_encoder_triggers_yes;
	[Widget] Gtk.RadioButton radio_encoder_triggers_yes_start_at_capture;
	[Widget] Gtk.RadioButton radio_encoder_triggers_yes_start_at_first_trigger;
	[Widget] Gtk.Image image_encoder_inactivity_help;
	[Widget] Gtk.Image image_encoder_capture_cut_by_triggers_help;
	[Widget] Gtk.CheckButton check_encoder_capture_infinite;
	[Widget] Gtk.Image image_encoder_capture_infinite;

	//encoder other tab
	[Widget] Gtk.CheckButton checkbutton_encoder_propulsive;
	[Widget] Gtk.RadioButton radio_encoder_work_kcal;
	[Widget] Gtk.RadioButton radio_encoder_work_joules;
	[Widget] Gtk.RadioButton radio_encoder_inertial_analyze_equivalent_mass;
	[Widget] Gtk.RadioButton radio_encoder_inertial_analyze_inertia_moment;
	[Widget] Gtk.RadioButton radio_encoder_inertial_analyze_diameter;
	[Widget] Gtk.Image image_encoder_inertial_analyze_eq_mass_help;
	[Widget] Gtk.SpinButton spin_encoder_smooth_con;
	[Widget] Gtk.Label label_encoder_con;
	[Widget] Gtk.RadioButton radio_encoder_1RM_nonweighted;
	[Widget] Gtk.RadioButton radio_encoder_1RM_weighted;
	[Widget] Gtk.RadioButton radio_encoder_1RM_weighted2;
	[Widget] Gtk.RadioButton radio_encoder_1RM_weighted3;

	//forceSensor tab
	[Widget] Gtk.SpinButton spin_force_sensor_capture_width_graph_seconds;
	[Widget] Gtk.RadioButton radio_force_sensor_capture_zoom_out;
	[Widget] Gtk.RadioButton radio_force_sensor_capture_scroll;
	[Widget] Gtk.SpinButton spin_force_sensor_elastic_ecc_min_displ;
	[Widget] Gtk.SpinButton spin_force_sensor_elastic_con_min_displ;
	[Widget] Gtk.SpinButton spin_force_sensor_not_elastic_ecc_min_force;
	[Widget] Gtk.SpinButton spin_force_sensor_not_elastic_con_min_force;
	[Widget] Gtk.SpinButton spin_force_sensor_graphs_line_width;
	[Widget] Gtk.RadioButton radio_force_sensor_variability_rmssd;
	[Widget] Gtk.RadioButton radio_force_sensor_variability_cvrmssd;
	[Widget] Gtk.RadioButton radio_force_sensor_variability_old;
	[Widget] Gtk.SpinButton spin_force_sensor_analyze_ab_slider_increment;
	[Widget] Gtk.SpinButton spin_force_sensor_analyze_max_avg_force_in_window;

	//runEncoder tab
	[Widget] Gtk.SpinButton spin_run_encoder_acceleration;
	[Widget] Gtk.SpinButton spin_run_encoder_pps;
	[Widget] Gtk.Label label_pps_equivalent;
	[Widget] Gtk.Label label_pps_maximum;

	//multimedia tab
	[Widget] Gtk.CheckButton checkbutton_volume;
	[Widget] Gtk.RadioButton radio_gstreamer_0_1;
	[Widget] Gtk.RadioButton radio_gstreamer_1_0;
	[Widget] Gtk.RadioButton radio_ffplay;
	[Widget] Gtk.RadioButton radio_sound_systemsounds;
	[Widget] Gtk.HBox hbox_not_recommended_when_not_on_windows;
	[Widget] Gtk.Label label_test_sound_result;
	[Widget] Gtk.Notebook notebook_multimedia;
	[Widget] Gtk.Box hbox_combo_camera;
	[Widget] Gtk.ComboBox combo_camera;
	[Widget] Gtk.HBox hbox_camera_resolution_framerate;
	[Widget] Gtk.HBox hbox_camera_resolution_custom;
	[Widget] Gtk.SpinButton spin_camera_resolution_custom_width;
	[Widget] Gtk.SpinButton spin_camera_resolution_custom_height;
	[Widget] Gtk.HBox hbox_camera_framerate_custom;
	[Widget] Gtk.SpinButton spin_camera_framerate_custom;
	[Widget] Gtk.Entry entry_camera_framerate_custom_decimals;
	[Widget] Gtk.Label label_camera_pixel_format;
	[Widget] Gtk.Label label_camera_pixel_format_current;
	[Widget] Gtk.Label label_camera_resolution_current;
	[Widget] Gtk.Label label_camera_framerate_current;
	[Widget] Gtk.HBox hbox_combo_camera_pixel_format;
	[Widget] Gtk.ComboBox combo_camera_pixel_format;
	[Widget] Gtk.Box hbox_combo_camera_resolution;
	[Widget] Gtk.ComboBox combo_camera_resolution;
	[Widget] Gtk.Box hbox_combo_camera_framerate;
	[Widget] Gtk.ComboBox combo_camera_framerate;
	[Widget] Gtk.Label label_camera_error;
	[Widget] Gtk.Label label_webcam_windows;
	[Widget] Gtk.Image image_multimedia_audio;
	[Widget] Gtk.Image image_multimedia_video;
	[Widget] Gtk.Image image_video_preview;
	[Widget] Gtk.Button button_video_preview;
	[Widget] Gtk.Label label_video_preview_error;
	[Widget] Gtk.CheckButton check_camera_stop_after;
	[Widget] Gtk.CheckButton check_camera_advanced;
	[Widget] Gtk.Frame frame_camera_advanced;
	[Widget] Gtk.VBox vbox_camera_stop_after_all;
	//[Widget] Gtk.VBox vbox_camera_stop_after;
	[Widget] Gtk.HBox hbox_camera_stop_after_seconds;
	[Widget] Gtk.SpinButton spin_camera_stop_after;
	[Widget] Gtk.Table table_video_advanced_actions;
	[Widget] Gtk.Label label_video_check_ffmpeg_running;
	[Widget] Gtk.Label label_video_check_ffplay_running;
	[Widget] Gtk.Button button_video_ffmpeg_kill;
	[Widget] Gtk.Button button_video_ffplay_kill;
	[Widget] Gtk.Label label_camera_check_running;

	//language tab
	[Widget] Gtk.Box hbox_combo_language;
	[Widget] Gtk.ComboBox combo_language;
	[Widget] Gtk.RadioButton radio_language_detected;
	[Widget] Gtk.RadioButton radio_language_force;
	[Widget] Gtk.RadioButton radio_graphs_translate;
	[Widget] Gtk.RadioButton radio_graphs_no_translate;
	[Widget] Gtk.VBox vbox_need_restart;
		
	//advanced tab
	[Widget] Gtk.ComboBox combo_decimals;
	[Widget] Gtk.CheckButton checkbutton_ask_deletion;
	[Widget] Gtk.CheckButton checkbutton_mute_logs;
	[Widget] Gtk.RadioButton radio_export_latin;
	[Widget] Gtk.RadioButton radio_export_non_latin;
	[Widget] Gtk.Label label_advanced_feedback;
	[Widget] Gtk.ToggleButton toggle_gc_collect_on_close;
	[Widget] Gtk.ToggleButton toggle_never_close;
	[Widget] Gtk.VBox vbox_version;
	[Widget] Gtk.Label label_progVersion;
	[Widget] Gtk.Frame frame_networks;
	[Widget] Gtk.CheckButton check_networks_devices;

	[Widget] Gtk.RadioButton radio_python_2;
	[Widget] Gtk.RadioButton radio_python_3;

	[Widget] Gtk.Button button_accept;
	[Widget] Gtk.Button button_cancel;
	[Widget] Gtk.Image image_button_accept;
	[Widget] Gtk.Image image_button_cancel;
	public Gtk.Button FakeButtonConfigurationImported;
	public Gtk.Button FakeButtonDebugModeStart;
	
	static PreferencesWindow PreferencesWindowBox;

	private Gdk.Color colorBackground;

	private UtilAll.OperatingSystems operatingSystem;
	private Preferences preferences; //stored to update SQL if anything changed
//	private Thread thread;

	string databaseURL;
	string databaseTempURL;
	
	ListStore langsStore;

	private enum notebook_top_pages { PREFERENCES, SELECTTABS, HELP }

	const int JUMPSPAGE = 2;
	const int RUNSPAGE = 3;
	const int ENCODERCAPTUREPAGE = 4;
	const int ENCODEROTHERPAGE = 5;
	const int FORCESENSORPAGE = 6;
	const int RUNENCODERPAGE = 7;

	static private WebcamDeviceList wd_list;
	private WebcamFfmpegSupportedModes wfsm;


	PreferencesWindow ()
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "preferences_win.glade", "preferences_win", "chronojump");
		gladeXML.Autoconnect(this);
		
		//put an icon to window
		UtilGtk.IconWindow(preferences_win);
		preferences_win.Title = Catalog.GetString("Preferences");

		//database and log files stuff
		databaseURL = Util.GetDatabaseDir() + System.IO.Path.DirectorySeparatorChar  + "chronojump.db";
		databaseTempURL = Util.GetDatabaseTempDir() + System.IO.Path.DirectorySeparatorChar  + "chronojump.db";
		
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

		if(menu_mode !=	Constants.Modes.JUMPSSIMPLE && menu_mode != Constants.Modes.JUMPSREACTIVE) {
			PreferencesWindowBox.notebook.GetNthPage(JUMPSPAGE).Hide();
			PreferencesWindowBox.check_view_jumps.Active = false;
		} if(menu_mode != Constants.Modes.RUNSSIMPLE && menu_mode != Constants.Modes.RUNSINTERVALLIC) {
			PreferencesWindowBox.notebook.GetNthPage(RUNSPAGE).Hide();
			PreferencesWindowBox.check_view_runs.Active = false;
		} if(menu_mode != Constants.Modes.POWERGRAVITATORY && menu_mode != Constants.Modes.POWERINERTIAL) {
			PreferencesWindowBox.notebook.GetNthPage(ENCODERCAPTUREPAGE).Hide();
			PreferencesWindowBox.notebook.GetNthPage(ENCODEROTHERPAGE).Hide();
			PreferencesWindowBox.check_view_encoder.Active = false;
		}
		if(menu_mode !=	Constants.Modes.FORCESENSOR) {
			PreferencesWindowBox.notebook.GetNthPage(FORCESENSORPAGE).Hide();
			PreferencesWindowBox.check_view_force_sensor.Active = false;
		} if(menu_mode != Constants.Modes.RUNSENCODER) {
			PreferencesWindowBox.notebook.GetNthPage(RUNENCODERPAGE).Hide();
			PreferencesWindowBox.check_view_race_analyzer.Active = false;
		}

		PreferencesWindowBox.preferences = preferences;

		PreferencesWindowBox.createComboLanguage();
		Pixbuf pixbuf;


		PreferencesWindowBox.image_button_cancel.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_cancel.png");
		PreferencesWindowBox.image_button_accept.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_done_blue.png");

		//appearance tab

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_rest.png");
		PreferencesWindowBox.image_rest.Pixbuf = pixbuf;

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
		if(preferences.volumeOn)  
			PreferencesWindowBox.checkbutton_volume.Active = true; 
		else 
			PreferencesWindowBox.checkbutton_volume.Active = false; 

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

		wd_list = UtilMultimedia.GetVideoDevices();
		PreferencesWindowBox.createComboCamera(preferences.videoDevice,
				preferences.videoDevicePixelFormat, preferences.videoDeviceResolution, preferences.videoDeviceFramerate);

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

		string [] decs = {"1", "2", "3"};
		PreferencesWindowBox.combo_decimals.Active = UtilGtk.ComboMakeActive(
				decs, preferences.digitsNumber.ToString());

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
		PreferencesWindowBox.image_run_speed_start_help.Pixbuf = pixbuf;
		PreferencesWindowBox.image_encoder_inactivity_help.Pixbuf = pixbuf;
		PreferencesWindowBox.image_encoder_capture_cut_by_triggers_help.Pixbuf = pixbuf;
		PreferencesWindowBox.image_encoder_inertial_analyze_eq_mass_help.Pixbuf = pixbuf;

		if(menu_mode ==	Constants.Modes.RUNSSIMPLE)
			PreferencesWindowBox.notebook_races_double_contacts.CurrentPage = 0;
		else if(menu_mode == Constants.Modes.RUNSINTERVALLIC)
			PreferencesWindowBox.notebook_races_double_contacts.CurrentPage = 1;

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
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_encoder_triggers_no.png");
		PreferencesWindowBox.image_encoder_triggers.Pixbuf = pixbuf;

		if(menu_mode ==	Constants.Modes.POWERGRAVITATORY)
			PreferencesWindowBox.notebook_encoder_capture_gi.CurrentPage = 0;
		else if(menu_mode == Constants.Modes.POWERINERTIAL)
			PreferencesWindowBox.notebook_encoder_capture_gi.CurrentPage = 1;

		PreferencesWindowBox.spin_encoder_capture_min_height_gravitatory.Value = preferences.encoderCaptureMinHeightGravitatory;
		PreferencesWindowBox.spin_encoder_capture_min_height_inertial.Value = preferences.encoderCaptureMinHeightInertial;

		if(preferences.encoderCaptureInertialDiscardFirstN > 0) {
			PreferencesWindowBox.checkbutton_encoder_capture_inertial_discard_first_n.Active = true;
			PreferencesWindowBox.spin_encoder_capture_inertial_discard_first_n.Value = preferences.encoderCaptureInertialDiscardFirstN;
			PreferencesWindowBox.hbox_encoder_capture_inertial_discard_first_n.Visible = true;
		} else {
			PreferencesWindowBox.checkbutton_encoder_capture_inertial_discard_first_n.Active = false;
			PreferencesWindowBox.spin_encoder_capture_inertial_discard_first_n.Value = 3;
			PreferencesWindowBox.hbox_encoder_capture_inertial_discard_first_n.Visible = false;
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
			PreferencesWindowBox.radio_force_sensor_variability_rmssd.Active = true;
		else if(preferences.forceSensorVariabilityMethod == Preferences.VariabilityMethodEnum.CVRMSSD)
			PreferencesWindowBox.radio_force_sensor_variability_cvrmssd.Active = true;
		else
			PreferencesWindowBox.radio_force_sensor_variability_old.Active = true;

		PreferencesWindowBox.spin_force_sensor_analyze_ab_slider_increment.Value = preferences.forceSensorAnalyzeABSliderIncrement;
		PreferencesWindowBox.spin_force_sensor_analyze_max_avg_force_in_window.Value = preferences.forceSensorAnalyzeMaxAVGInWindow;

		//runEncoder -->
		PreferencesWindowBox.spin_run_encoder_acceleration.Value = preferences.runEncoderMinAccel;
		PreferencesWindowBox.spin_run_encoder_pps.Value = preferences.runEncoderPPS;
		PreferencesWindowBox.update_run_encoder_pps_equivalence_and_max ();

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
			PreferencesWindowBox.radio_color_custom.Active = true;
			PreferencesWindowBox.button_color_choose.Sensitive = true;
		}

		PreferencesWindowBox.colorBackground = UtilGtk.ColorParse(preferences.colorBackgroundString);
		PreferencesWindowBox.paintColorDrawingAreaAndBg (PreferencesWindowBox.colorBackground);
		UtilGtk.PaintColorDrawingArea(PreferencesWindowBox.drawingarea_background_color_chronojump_blue, UtilGtk.BLUE_CHRONOJUMP);

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
	private void on_check_view_encoder_clicked (object o,EventArgs args)
	{
		//on this order to be active the capture page (it is the first one, nw)
		tabShowHide(check_view_encoder.Active, ENCODEROTHERPAGE);
		tabShowHide(check_view_encoder.Active, ENCODERCAPTUREPAGE);
	}
	private void on_check_view_force_sensor_clicked (object o,EventArgs args)
	{
		tabShowHide(check_view_force_sensor.Active, FORCESENSORPAGE);
	}
	private void on_check_view_race_analyzer_clicked (object o,EventArgs args)
	{
		tabShowHide(check_view_race_analyzer.Active, RUNENCODERPAGE);
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


	private void paintColorDrawingAreaAndBg (Gdk.Color color)
	{
		UtilGtk.PaintColorDrawingArea(drawingarea_background_color, color);
		if(! preferences.colorBackgroundOsColor)
			UtilGtk.WindowColor(preferences_win, color);
	}

	private void on_radio_color_custom_toggled (object o, EventArgs args)
	{
		button_color_choose.Sensitive = true;
		label_radio_color_os_needs_restart.Visible = false;
	}
	private void on_radio_color_chronojump_blue_toggled (object o, EventArgs args)
	{
		button_color_choose.Sensitive = false;
		label_radio_color_os_needs_restart.Visible = false;
	}
	private void on_radio_color_os_toggled (object o, EventArgs args)
	{
		button_color_choose.Sensitive = false;
		label_radio_color_os_needs_restart.Visible = true;
	}

	private void on_button_color_choose_clicked(object o, EventArgs args)
	{
		using (ColorSelectionDialog colorSelectionDialog = new ColorSelectionDialog (Catalog.GetString("Select color")))
		{
			colorSelectionDialog.TransientFor = preferences_win;
			colorSelectionDialog.ColorSelection.CurrentColor = colorBackground;
			colorSelectionDialog.ColorSelection.HasPalette = true;

			if (colorSelectionDialog.Run () == (int) ResponseType.Ok) {
				colorBackground = colorSelectionDialog.ColorSelection.CurrentColor;
				paintColorDrawingAreaAndBg(colorBackground);

				/*
				LogB.Information(string.Format("color: red {0}, green {1}, blue {2}",
							colorBackground.Red, colorBackground.Green, colorBackground.Blue));
				LogB.Information(string.Format("color: red {0}, green {1}, blue {2}",
							colorBackground.Red/256.0, colorBackground.Green/256.0, colorBackground.Blue/256.0));
				*/
				LogB.Information("color to string: " + UtilGtk.ColorToColorString(colorBackground));
			}

			colorSelectionDialog.Hide ();
		}
	}

	private void on_radio_font_courier_toggled (object o, EventArgs args)
	{
		label_radio_font_needs_restart.Visible = true;
	}
	private void on_radio_font_helvetica_toggled (object o, EventArgs args)
	{
		label_radio_font_needs_restart.Visible = true;
	}

	private void on_check_rest_time_toggled (object o, EventArgs args)
	{
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
	}

	private void on_radio_encoder_capture_show_all_bars_toggled (object o, EventArgs args)
	{
		spin_encoder_capture_show_only_some_bars.Sensitive = false;
	}
	private void on_radio_encoder_capture_show_only_some_bars_toggled (object o, EventArgs args)
	{
		spin_encoder_capture_show_only_some_bars.Sensitive = true;
	}

	private void on_check_encoder_capture_inactivity_end_time_clicked (object o, EventArgs args)
	{
		hbox_encoder_capture_inactivity_time.Sensitive = check_encoder_capture_inactivity_end_time.Active;
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

	/*
	 * triggers stuff
	 */

	private void on_radio_encoder_triggers_toggled (object obj, EventArgs args)
	{
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


	// ---- race analyzer ---->

	private void on_spin_run_encoder_pps_value_changed (object o, EventArgs args)
	{
		update_run_encoder_pps_equivalence_and_max ();
	}
	private void update_run_encoder_pps_equivalence_and_max ()
	{
		label_pps_equivalent.Text = string.Format(Catalog.GetString("{0} pps is equivalent to a resolution of {1} cm."),
				spin_run_encoder_pps.Value, 0.3003 * spin_run_encoder_pps.Value);

		label_pps_maximum.Text = string.Format(Catalog.GetString("{0} pps allows to record up to {1} m/s."),
				spin_run_encoder_pps.Value, spin_run_encoder_pps.Value * 4);
	}

	// <---- end of race analyzer ----

	private void createComboCamera(string current, string pixelFormat, string resolution, string framerate)
	{
		// 1) videoDevice

		combo_camera = ComboBox.NewText ();

		/*
		 * declare both because there is a return just here and if they are undeclred the method:
		 * on_button_accept_clicked () will fail
		 */
		combo_camera_pixel_format = ComboBox.NewText ();
		combo_camera_resolution = ComboBox.NewText ();
		combo_camera_framerate = ComboBox.NewText ();

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

		combo_camera_framerate = ComboBox.NewText ();
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

	private void on_check_camera_stop_after_toggled (object o, EventArgs args)
	{
		//vbox_camera_stop_after.Visible = check_camera_stop_after.Active;
		hbox_camera_stop_after_seconds.Visible = check_camera_stop_after.Active;
	}

	private void on_combo_camera_changed (object o, EventArgs args)
	{
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
	}

	private void on_combo_camera_pixel_format_changed (object o, EventArgs args)
	{
		string pixelFormat = UtilGtk.ComboGetActive(combo_camera_pixel_format);

		if(pixelFormat != "" && wfsm != null)
		{
			string currentResolution = getSelectedResolution();
			UtilGtk.ComboUpdate(combo_camera_resolution, wfsm.PopulateListByPixelFormat(pixelFormat));
			combo_camera_resolution.Active = UtilGtk.ComboMakeActive(combo_camera_resolution, currentResolution);
			button_video_preview.Sensitive = true;
		}
	}
	private void on_combo_camera_resolution_changed (object o, EventArgs args)
	{
		string pixelFormat = UtilGtk.ComboGetActive(combo_camera_pixel_format);
		string resolution = UtilGtk.ComboGetActive(combo_camera_resolution);
		hbox_camera_resolution_custom.Visible = resolution == Catalog.GetString("Custom");

		if(resolution != "" && resolution != Catalog.GetString("Custom") && wfsm != null)
		{
			string currentFramerate = getSelectedFramerate();
			UtilGtk.ComboUpdate(combo_camera_framerate, wfsm.GetFramerates (pixelFormat, resolution));
			combo_camera_framerate.Active = UtilGtk.ComboMakeActive(combo_camera_framerate, currentFramerate);
		}
	}
	private void on_combo_camera_framerate_changed (object o, EventArgs args)
	{
		hbox_camera_framerate_custom.Visible = UtilGtk.ComboGetActive(combo_camera_framerate) == Catalog.GetString("Custom");
	}

	private void on_check_camera_advanced_toggled (object o, EventArgs args)
	{
		frame_camera_advanced.Visible = check_camera_advanced.Active;
	}

	private void on_check_appearance_maximized_toggled (object obj, EventArgs args)
	{
		alignment_undecorated.Visible = check_appearance_maximized.Active;
		label_recommended_undecorated.Visible = check_appearance_maximized.Active;
	}

	private void on_check_appearance_person_win_hide_toggled (object obj, EventArgs args)
	{
		check_appearance_person_photo.Sensitive = ! check_appearance_person_win_hide.Active;
	}

	private void on_checkbutton_encoder_capture_inertial_discard_first_n_toggled (object obj, EventArgs args)
	{
		hbox_encoder_capture_inertial_discard_first_n.Visible = (checkbutton_encoder_capture_inertial_discard_first_n.Active);
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
		string selected = UtilGtk.ComboGetActive(combo_camera_pixel_format);
		return selected;
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

		table_video_advanced_actions.Visible = true;
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
		
		combo_language = ComboBox.NewText ();
		fillLanguages();

		hbox_combo_language.PackStart(combo_language, false, false, 0);
		hbox_combo_language.ShowAll();
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

	private void on_radio_language_toggled (object obj, EventArgs args) {
		hbox_combo_language.Sensitive = radio_language_force.Active;

		if(hbox_language_signalOn)
			vbox_need_restart.Visible = true;
	}
	private void on_radio_translate_toggled (object obj, EventArgs args) {
		if(hbox_language_signalOn)
			vbox_need_restart.Visible = true;
	}
	private	void combo_language_changed (object obj, EventArgs args) {
		if(hbox_language_signalOn)
			vbox_need_restart.Visible = true;
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

			
	private void on_checkbutton_show_tv_tc_index_clicked (object o, EventArgs args) {
		if(checkbutton_show_tv_tc_index.Active)
			hbox_indexes.Show();
		else
			hbox_indexes.Hide();
	}

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

	private void on_checkbutton_runs_prevent_double_contact_toggled (object o, EventArgs args) {
		vbox_runs_prevent_double_contact.Visible = checkbutton_runs_prevent_double_contact.Active;
	}
	private void on_checkbutton_runs_i_prevent_double_contact_toggled (object o, EventArgs args) {
		vbox_runs_i_prevent_double_contact.Visible = checkbutton_runs_i_prevent_double_contact.Active;
	}

		
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		PreferencesWindowBox.preferences_win.Hide();
		PreferencesWindowBox = null;
	}
	
	void on_preferences_delete_event (object o, DeleteEventArgs args)
	{
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
		string dir = UtilAll.GetLogsDir();
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
				File.Copy(fc.Filename, UtilAll.GetConfigFileName(), true);
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
				Catalog.GetString("This way it is possible to compare the resistance of diferent configurations on diferent machines.")
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

	private void on_debug_mode_clicked (object o, EventArgs args) {
		//will be managed from gui/chronojump.cs
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

	//change stuff in Sqlite and in preferences object that will be retrieved by GetPreferences
	void on_button_accept_clicked (object o, EventArgs args)
	{
		Sqlite.Open();


		//appearance tab
		Preferences.MaximizedTypes maximizedTypeFromGUI = get_maximized_from_gui();
		if(preferences.maximized != maximizedTypeFromGUI)
		{
			SqlitePreferences.Update("maximized", maximizedTypeFromGUI.ToString(), true);
			preferences.maximized = maximizedTypeFromGUI;
		}

		if( preferences.personWinHide != PreferencesWindowBox.check_appearance_person_win_hide.Active ) {
			SqlitePreferences.Update("personWinHide", PreferencesWindowBox.check_appearance_person_win_hide.Active.ToString(), true);
			preferences.personWinHide = PreferencesWindowBox.check_appearance_person_win_hide.Active;
		}
		if( preferences.personPhoto != PreferencesWindowBox.check_appearance_person_photo.Active ) {
			SqlitePreferences.Update("personPhoto", PreferencesWindowBox.check_appearance_person_photo.Active.ToString(), true);
			preferences.personPhoto = PreferencesWindowBox.check_appearance_person_photo.Active;
		}

		if(radio_color_chronojump_blue.Active) 
		{
			preferences.colorBackgroundString = Preferences.PreferencesChange(
					SqlitePreferences.ColorBackground, preferences.colorBackgroundString,
					"#0e1e46");
			preferences.colorBackgroundOsColor = Preferences.PreferencesChange(
					SqlitePreferences.ColorBackgroundOsColor, preferences.colorBackgroundOsColor,
					false);
			preferences.colorBackgroundIsDark = true;
		} else if(radio_color_os.Active)
		{
			//radio_color_os does not change the colorBackgroundString, it changes the Config.UseSystemColor
			//but note that on showing cairo and execute graphs, primary color will be colorBackground
			preferences.colorBackgroundOsColor = Preferences.PreferencesChange(
					SqlitePreferences.ColorBackgroundOsColor, preferences.colorBackgroundOsColor,
					true);
			preferences.colorBackgroundIsDark = false; //is not important as it is not going to be used
		} else {
			preferences.colorBackgroundString = Preferences.PreferencesChange(
					SqlitePreferences.ColorBackground, preferences.colorBackgroundString,
					UtilGtk.ColorToColorString(colorBackground)); //this does the reverse of Gdk.Color.Parse on UtilGtk.ColorParse()
			preferences.colorBackgroundOsColor = Preferences.PreferencesChange(
					SqlitePreferences.ColorBackgroundOsColor, preferences.colorBackgroundOsColor,
					false);
			preferences.colorBackgroundIsDark = UtilGtk.ColorIsDark(colorBackground);
		}

		preferences.logoAnimatedShow = Preferences.PreferencesChange(
				SqlitePreferences.LogoAnimatedShow, preferences.logoAnimatedShow,
				PreferencesWindowBox.check_logo_animated.Active);

		preferences.loadLastSessionAtStart = Preferences.PreferencesChange(
				SqlitePreferences.LoadLastSessionAtStart, preferences.loadLastSessionAtStart,
				PreferencesWindowBox.check_session_autoload_at_start.Active);

		preferences.loadLastModeAtStart = Preferences.PreferencesChange(
				SqlitePreferences.LoadLastModeAtStart, preferences.loadLastModeAtStart,
				PreferencesWindowBox.check_mode_autoload_at_start.Active);


		if(preferences.fontType == Preferences.FontTypes.Courier && radio_font_helvetica.Active)
		{
			SqlitePreferences.Update(SqlitePreferences.FontsOnGraphs, Preferences.FontTypes.Helvetica.ToString(), true);
			preferences.fontType = Preferences.FontTypes.Helvetica;
		}
		else if(preferences.fontType == Preferences.FontTypes.Helvetica && radio_font_courier.Active)
		{
			SqlitePreferences.Update(SqlitePreferences.FontsOnGraphs, Preferences.FontTypes.Courier.ToString(), true);
			preferences.fontType = Preferences.FontTypes.Courier;
		}

		//rest time change start ---->
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
				SqlitePreferences.Update(SqlitePreferences.RestTimeMinutes, minutes.ToString(), true);
				preferences.restTimeMinutes = minutes;
				SqlitePreferences.Update(SqlitePreferences.RestTimeSeconds, seconds.ToString(), true);
				preferences.restTimeSeconds = seconds;
		}
		//<---- rest time change end

		if( preferences.digitsNumber != Convert.ToInt32(UtilGtk.ComboGetActive(combo_decimals)) ) {
			SqlitePreferences.Update("digitsNumber", UtilGtk.ComboGetActive(combo_decimals), true);
			preferences.digitsNumber = Convert.ToInt32(UtilGtk.ComboGetActive(combo_decimals));
		}
		
		if( preferences.showPower != PreferencesWindowBox.checkbutton_power.Active ) {
			SqlitePreferences.Update("showPower", PreferencesWindowBox.checkbutton_power.Active.ToString(), true);
			preferences.showPower = PreferencesWindowBox.checkbutton_power.Active;
		}

		if( preferences.showStiffness != PreferencesWindowBox.checkbutton_stiffness.Active ) {
			SqlitePreferences.Update("showStiffness", PreferencesWindowBox.checkbutton_stiffness.Active.ToString(), true);
			preferences.showStiffness = PreferencesWindowBox.checkbutton_stiffness.Active;
		}
		
		if( preferences.showInitialSpeed != PreferencesWindowBox.checkbutton_initial_speed.Active ) {
			SqlitePreferences.Update("showInitialSpeed", PreferencesWindowBox.checkbutton_initial_speed.Active.ToString(), true);
			preferences.showInitialSpeed = PreferencesWindowBox.checkbutton_initial_speed.Active;
		}

		if( preferences.showJumpRSI != PreferencesWindowBox.checkbutton_jump_rsi.Active ) {
			SqlitePreferences.Update(SqlitePreferences.ShowJumpRSI, PreferencesWindowBox.checkbutton_jump_rsi.Active.ToString(), true);
			preferences.showJumpRSI = PreferencesWindowBox.checkbutton_jump_rsi.Active;
		}

		/*
		if( preferences.showAngle != PreferencesWindowBox.checkbutton_angle.Active ) {
			SqlitePreferences.Update("showAngle", PreferencesWindowBox.checkbutton_angle.Active.ToString(), true);
			preferences.showAngle = PreferencesWindowBox.checkbutton_angle.Active;
		}
		*/

		if(PreferencesWindowBox.checkbutton_show_tv_tc_index.Active) {
			preferences.showQIndex = Preferences.PreferencesChange(
					"showQIndex", preferences.showQIndex,
					PreferencesWindowBox.radiobutton_show_q_index.Active);
			preferences.showDjIndex = Preferences.PreferencesChange(
					"showDjIndex", preferences.showDjIndex,
					PreferencesWindowBox.radiobutton_show_dj_index.Active);
		} else {
			preferences.showQIndex = Preferences.PreferencesChange(
					"showQIndex", preferences.showQIndex, false);
			preferences.showDjIndex = Preferences.PreferencesChange(
					"showDjIndex", preferences.showDjIndex, false);
		}

		preferences.heightPreferred = Preferences.PreferencesChange(
				"heightPreferred",
				preferences.heightPreferred,
				radio_jumps_dj_heights.Active);

		
		if( preferences.askDeletion != PreferencesWindowBox.checkbutton_ask_deletion.Active ) {
			SqlitePreferences.Update("askDeletion", PreferencesWindowBox.checkbutton_ask_deletion.Active.ToString(), true);
			preferences.askDeletion = PreferencesWindowBox.checkbutton_ask_deletion.Active;
		}

		if( preferences.muteLogs != PreferencesWindowBox.checkbutton_mute_logs.Active ) {
			SqlitePreferences.Update("muteLogs", PreferencesWindowBox.checkbutton_mute_logs.Active.ToString(), true);
			preferences.muteLogs = PreferencesWindowBox.checkbutton_mute_logs.Active;
		}

		//this is not stored in SQL
		preferences.networksAllowChangeDevices = PreferencesWindowBox.check_networks_devices.Active;

		if( preferences.weightStatsPercent != PreferencesWindowBox.radio_weight_percent.Active ) {
			SqlitePreferences.Update("weightStatsPercent", PreferencesWindowBox.radio_weight_percent.Active.ToString(), true);
			preferences.weightStatsPercent = PreferencesWindowBox.radio_weight_percent.Active;
		}

		if( preferences.metersSecondsPreferred != PreferencesWindowBox.radio_speed_ms.Active ) {
			SqlitePreferences.Update("metersSecondsPreferred", PreferencesWindowBox.radio_speed_ms.Active.ToString(), true);
			preferences.metersSecondsPreferred = PreferencesWindowBox.radio_speed_ms.Active;
		}
		
		if( preferences.runSpeedStartArrival != PreferencesWindowBox.radio_runs_speed_start_arrival.Active ) {
			SqlitePreferences.Update("runSpeedStartArrival", PreferencesWindowBox.radio_runs_speed_start_arrival.Active.ToString(), true);
			preferences.runSpeedStartArrival = PreferencesWindowBox.radio_runs_speed_start_arrival.Active;
		}
		
		//start of double contacts stuff ----

		//1 simple runs ----
		
		//1.1 was FIRST or AVERAGE or LAST and now will be NONE
		if( (preferences.runDoubleContactsMode != Constants.DoubleContact.NONE) && 
				! PreferencesWindowBox.checkbutton_runs_prevent_double_contact.Active) 
		{
				SqlitePreferences.Update("runDoubleContactsMode", Constants.DoubleContact.NONE.ToString(), true);
				preferences.runDoubleContactsMode = Constants.DoubleContact.NONE;
		}
		else if(PreferencesWindowBox.checkbutton_runs_prevent_double_contact.Active) 
		{
			if( preferences.runDoubleContactsMode != Constants.DoubleContact.BIGGEST_TC ) {
				SqlitePreferences.Update("runDoubleContactsMode", Constants.DoubleContact.BIGGEST_TC.ToString(), true);
				preferences.runDoubleContactsMode = Constants.DoubleContact.BIGGEST_TC;
			}

			if(preferences.runDoubleContactsMS != (int) PreferencesWindowBox.spinbutton_runs_prevent_double_contact.Value) {
				SqlitePreferences.Update("runDoubleContactsMS", 
						PreferencesWindowBox.spinbutton_runs_prevent_double_contact.Value.ToString(), true); //saved as string
				preferences.runDoubleContactsMS = (int) spinbutton_runs_prevent_double_contact.Value;
			}
		}

		//2 intervallic runs ----
		
		//2.1 was FIRST or AVERAGE or LAST and now will be NONE
		if( (preferences.runIDoubleContactsMode != Constants.DoubleContact.NONE) && 
				! PreferencesWindowBox.checkbutton_runs_i_prevent_double_contact.Active) 
		{
				SqlitePreferences.Update("runIDoubleContactsMode", Constants.DoubleContact.NONE.ToString(), true);
				preferences.runIDoubleContactsMode = Constants.DoubleContact.NONE;
		}
		else if(PreferencesWindowBox.checkbutton_runs_i_prevent_double_contact.Active) 
		{
			if( preferences.runIDoubleContactsMode != Constants.DoubleContact.BIGGEST_TC ) {
				SqlitePreferences.Update("runIDoubleContactsMode", Constants.DoubleContact.BIGGEST_TC.ToString(), true);
				preferences.runIDoubleContactsMode = Constants.DoubleContact.BIGGEST_TC;
			}
			
			if(preferences.runIDoubleContactsMS != (int) PreferencesWindowBox.spinbutton_runs_i_prevent_double_contact.Value) {
				SqlitePreferences.Update("runIDoubleContactsMS", 
						PreferencesWindowBox.spinbutton_runs_i_prevent_double_contact.Value.ToString(), true); //saved as string
				preferences.runIDoubleContactsMS = (int) spinbutton_runs_i_prevent_double_contact.Value;
			}
		}

		//---- end of double contacts stuff
		
		//encoder capture ----
	
		preferences.encoderCaptureTime = Preferences.PreferencesChange(
				"encoderCaptureTime",
				preferences.encoderCaptureTime,
				(int) PreferencesWindowBox.spin_encoder_capture_time.Value);

		if(! PreferencesWindowBox.check_encoder_capture_inactivity_end_time.Active)
		{
			SqlitePreferences.Update("encoderCaptureInactivityEndTime", "-1", true);
			preferences.encoderCaptureInactivityEndTime = -1;
		} else {
			preferences.encoderCaptureInactivityEndTime = Preferences.PreferencesChange(
					"encoderCaptureInactivityEndTime",
					preferences.encoderCaptureInactivityEndTime,
					(int) PreferencesWindowBox.spin_encoder_capture_inactivity_end_time.Value);
		}

		preferences.encoderCaptureMinHeightGravitatory = Preferences.PreferencesChange(
				"encoderCaptureMinHeightGravitatory",
				preferences.encoderCaptureMinHeightGravitatory,
				(int) PreferencesWindowBox.spin_encoder_capture_min_height_gravitatory.Value);
	
		preferences.encoderCaptureMinHeightInertial = Preferences.PreferencesChange(
				"encoderCaptureMinHeightInertial",
				preferences.encoderCaptureMinHeightInertial,
				(int) PreferencesWindowBox.spin_encoder_capture_min_height_inertial.Value);

		int spinEncoderCaptureDiscardFirstN = Convert.ToInt32(PreferencesWindowBox.spin_encoder_capture_inertial_discard_first_n.Value);
		if(! checkbutton_encoder_capture_inertial_discard_first_n.Active)
			spinEncoderCaptureDiscardFirstN = 0;
		if(spinEncoderCaptureDiscardFirstN != preferences.encoderCaptureInertialDiscardFirstN)
		{
			SqlitePreferences.Update("encoderCaptureInertialDiscardFirstN", spinEncoderCaptureDiscardFirstN.ToString(), true);
			preferences.encoderCaptureInertialDiscardFirstN = spinEncoderCaptureDiscardFirstN;
		}

		if( preferences.encoderCaptureShowNRepetitions > 0 && PreferencesWindowBox.radio_encoder_capture_show_all_bars.Active )
		{
			SqlitePreferences.Update("encoderCaptureShowNRepetitions", "-1", true);
			preferences.encoderCaptureShowNRepetitions = -1;
		}
		else if( PreferencesWindowBox.radio_encoder_capture_show_only_some_bars.Active &&
				preferences.encoderCaptureShowNRepetitions != (int) PreferencesWindowBox.spin_encoder_capture_show_only_some_bars.Value) {
			SqlitePreferences.Update("encoderCaptureShowNRepetitions",
					PreferencesWindowBox.spin_encoder_capture_show_only_some_bars.Value.ToString(), true);
			preferences.encoderCaptureShowNRepetitions = (int) PreferencesWindowBox.spin_encoder_capture_show_only_some_bars.Value;
		}

		preferences.encoderCaptureBarplotFontSize = Preferences.PreferencesChange(
				"encoderCaptureBarplotFontSize",
				preferences.encoderCaptureBarplotFontSize,
				(int) PreferencesWindowBox.spin_encoder_capture_barplot_font_size.Value);

		preferences.encoderShowStartAndDuration = Preferences.PreferencesChange(
				"encoderShowStartAndDuration",
				preferences.encoderShowStartAndDuration,
				PreferencesWindowBox.check_show_start_and_duration.Active);

		if(PreferencesWindowBox.radio_encoder_triggers_no.Active &&
				preferences.encoderCaptureCutByTriggers != Preferences.TriggerTypes.NO_TRIGGERS)
		{
			SqlitePreferences.Update("encoderCaptureCutByTriggers", Preferences.TriggerTypes.NO_TRIGGERS.ToString(), true);
			preferences.encoderCaptureCutByTriggers = Preferences.TriggerTypes.NO_TRIGGERS;
		}
		else if(PreferencesWindowBox.radio_encoder_triggers_yes.Active &&
				PreferencesWindowBox.radio_encoder_triggers_yes_start_at_capture.Active &&
				preferences.encoderCaptureCutByTriggers != Preferences.TriggerTypes.START_AT_CAPTURE)
		{
			SqlitePreferences.Update("encoderCaptureCutByTriggers", Preferences.TriggerTypes.START_AT_CAPTURE.ToString(), true);
			preferences.encoderCaptureCutByTriggers = Preferences.TriggerTypes.START_AT_CAPTURE;
		}
		else if(PreferencesWindowBox.radio_encoder_triggers_yes.Active &&
				PreferencesWindowBox.radio_encoder_triggers_yes_start_at_first_trigger.Active &&
				preferences.encoderCaptureCutByTriggers != Preferences.TriggerTypes.START_AT_FIRST_ON)
		{
			SqlitePreferences.Update("encoderCaptureCutByTriggers", Preferences.TriggerTypes.START_AT_FIRST_ON.ToString(), true);
			preferences.encoderCaptureCutByTriggers = Preferences.TriggerTypes.START_AT_FIRST_ON;
		}

		preferences.encoderCaptureInfinite = Preferences.PreferencesChange(
				SqlitePreferences.EncoderCaptureInfinite, preferences.encoderCaptureInfinite,
				PreferencesWindowBox.check_encoder_capture_infinite.Active);

		//---- end of encoder capture
		
		//encoder other ----
		
		preferences.encoderPropulsive = Preferences.PreferencesChange(
				"encoderPropulsive",
				preferences.encoderPropulsive,
				PreferencesWindowBox.checkbutton_encoder_propulsive.Active);

		preferences.encoderWorkKcal = Preferences.PreferencesChange(
				SqlitePreferences.EncoderWorkKcal,
				preferences.encoderWorkKcal,
				radio_encoder_work_kcal.Active);

		preferences.encoderSmoothCon = Preferences.PreferencesChange(
				"encoderSmoothCon",
				preferences.encoderSmoothCon,
				(double) PreferencesWindowBox.spin_encoder_smooth_con.Value);

		Preferences.EncoderInertialGraphsXTypes encoderInertialGraphsXFromGUI = get_encoderInertialGraphsX_from_gui();
		if(preferences.encoderInertialGraphsX != encoderInertialGraphsXFromGUI)
		{
			SqlitePreferences.Update(SqlitePreferences.EncoderInertialGraphsX, encoderInertialGraphsXFromGUI.ToString(), true);
			preferences.encoderInertialGraphsX = encoderInertialGraphsXFromGUI;
		}

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
			SqlitePreferences.Update("encoder1RMMethod", encoder1RMMethod.ToString(), true);
			preferences.encoder1RMMethod = encoder1RMMethod;
		}

		//---- end of encoder other

		//forceSensor
		preferences.forceSensorCaptureWidthSeconds = Preferences.PreferencesChange(
				SqlitePreferences.ForceSensorCaptureWidthSeconds,
				preferences.forceSensorCaptureWidthSeconds,
				Convert.ToInt32(spin_force_sensor_capture_width_graph_seconds.Value));

		preferences.forceSensorCaptureScroll = Preferences.PreferencesChange(
				SqlitePreferences.ForceSensorCaptureScroll,
				preferences.forceSensorCaptureScroll,
				radio_force_sensor_capture_scroll.Active);

		preferences.forceSensorElasticEccMinDispl = Preferences.PreferencesChange(
				SqlitePreferences.ForceSensorElasticEccMinDispl,
				preferences.forceSensorElasticEccMinDispl,
				Convert.ToDouble(spin_force_sensor_elastic_ecc_min_displ.Value));
		preferences.forceSensorElasticConMinDispl = Preferences.PreferencesChange(
				SqlitePreferences.ForceSensorElasticConMinDispl,
				preferences.forceSensorElasticConMinDispl,
				Convert.ToDouble(spin_force_sensor_elastic_con_min_displ.Value));
		preferences.forceSensorNotElasticEccMinForce = Preferences.PreferencesChange(
				SqlitePreferences.ForceSensorNotElasticEccMinForce,
				preferences.forceSensorNotElasticEccMinForce,
				Convert.ToInt32(spin_force_sensor_not_elastic_ecc_min_force.Value));
		preferences.forceSensorNotElasticConMinForce = Preferences.PreferencesChange(
				SqlitePreferences.ForceSensorNotElasticConMinForce,
				preferences.forceSensorNotElasticConMinForce,
				Convert.ToInt32(spin_force_sensor_not_elastic_con_min_force.Value));

		preferences.forceSensorGraphsLineWidth = Preferences.PreferencesChange(
				SqlitePreferences.ForceSensorGraphsLineWidth,
				preferences.forceSensorGraphsLineWidth,
				Convert.ToInt32(spin_force_sensor_graphs_line_width.Value));

		//variability
		if(PreferencesWindowBox.radio_force_sensor_variability_rmssd.Active &&
				preferences.forceSensorVariabilityMethod != Preferences.VariabilityMethodEnum.RMSSD)
		{
			SqlitePreferences.Update(SqlitePreferences.ForceSensorVariabilityMethod, Preferences.VariabilityMethodEnum.RMSSD.ToString(), true);
			preferences.forceSensorVariabilityMethod = Preferences.VariabilityMethodEnum.RMSSD;
		}
		else if(PreferencesWindowBox.radio_force_sensor_variability_cvrmssd.Active &&
				preferences.forceSensorVariabilityMethod != Preferences.VariabilityMethodEnum.CVRMSSD)
		{
			SqlitePreferences.Update(SqlitePreferences.ForceSensorVariabilityMethod, Preferences.VariabilityMethodEnum.CVRMSSD.ToString(), true);
			preferences.forceSensorVariabilityMethod = Preferences.VariabilityMethodEnum.CVRMSSD;
		}
		else if(PreferencesWindowBox.radio_force_sensor_variability_old.Active &&
				preferences.forceSensorVariabilityMethod != Preferences.VariabilityMethodEnum.CHRONOJUMP_OLD)
		{
			SqlitePreferences.Update(SqlitePreferences.ForceSensorVariabilityMethod, Preferences.VariabilityMethodEnum.CHRONOJUMP_OLD.ToString(), true);
			preferences.forceSensorVariabilityMethod = Preferences.VariabilityMethodEnum.CHRONOJUMP_OLD;
		}

		preferences.forceSensorAnalyzeABSliderIncrement = Preferences.PreferencesChange(
				SqlitePreferences.ForceSensorAnalyzeABSliderIncrement,
				preferences.forceSensorAnalyzeABSliderIncrement,
				Convert.ToDouble(spin_force_sensor_analyze_ab_slider_increment.Value));

		preferences.forceSensorAnalyzeMaxAVGInWindow = Preferences.PreferencesChange(
				SqlitePreferences.ForceSensorAnalyzeMaxAVGInWindow,
				preferences.forceSensorAnalyzeMaxAVGInWindow,
				Convert.ToDouble(spin_force_sensor_analyze_max_avg_force_in_window.Value));

		//runEncoder ----

		preferences.runEncoderMinAccel = Preferences.PreferencesChange(
				SqlitePreferences.RunEncoderMinAccel,
				preferences.runEncoderMinAccel,
				Convert.ToDouble(spin_run_encoder_acceleration.Value));

		preferences.runEncoderPPS = Preferences.PreferencesChange(
				SqlitePreferences.RunEncoderPPS,
				preferences.runEncoderPPS,
				Convert.ToInt32(spin_run_encoder_pps.Value));

		//multimedia ----
		if( preferences.volumeOn != PreferencesWindowBox.checkbutton_volume.Active ) {
			SqlitePreferences.Update("volumeOn", PreferencesWindowBox.checkbutton_volume.Active.ToString(), true);
			preferences.volumeOn = PreferencesWindowBox.checkbutton_volume.Active;
		}

		if( preferences.gstreamer != Preferences.GstreamerTypes.GST_1_0 && radio_gstreamer_1_0.Active)
		{
			SqlitePreferences.Update(Preferences.GstreamerStr, Preferences.GstreamerTypes.GST_1_0.ToString(), true);
			preferences.gstreamer = Preferences.GstreamerTypes.GST_1_0;
		}
		else if( preferences.gstreamer != Preferences.GstreamerTypes.GST_0_1 && radio_gstreamer_0_1.Active)
		{
			SqlitePreferences.Update(Preferences.GstreamerStr, Preferences.GstreamerTypes.GST_0_1.ToString(), true);
			preferences.gstreamer = Preferences.GstreamerTypes.GST_0_1;
		}
		else if( preferences.gstreamer != Preferences.GstreamerTypes.FFPLAY && radio_ffplay.Active)
		{
			SqlitePreferences.Update(Preferences.GstreamerStr, Preferences.GstreamerTypes.FFPLAY.ToString(), true);
			preferences.gstreamer = Preferences.GstreamerTypes.FFPLAY;
		}
		else if( preferences.gstreamer != Preferences.GstreamerTypes.SYSTEMSOUNDS && radio_sound_systemsounds.Active)
		{
			SqlitePreferences.Update(Preferences.GstreamerStr, Preferences.GstreamerTypes.SYSTEMSOUNDS.ToString(), true);
			preferences.gstreamer = Preferences.GstreamerTypes.SYSTEMSOUNDS;
		}

		//camera stuff

		string cameraCode = wd_list.GetCodeOfFullname(UtilGtk.ComboGetActive(combo_camera));
		if( cameraCode != "" && preferences.videoDevice != cameraCode ) {
			SqlitePreferences.Update("videoDevice", cameraCode, true);
			preferences.videoDevice = cameraCode;
		}

		string pixelFormat = getSelectedPixelFormat();
		if( preferences.videoDevicePixelFormat != pixelFormat ) {
			SqlitePreferences.Update("videoDevicePixelFormat", pixelFormat, true);
			preferences.videoDevicePixelFormat = pixelFormat;
		}

		string resolution = getSelectedResolution();
		if( preferences.videoDeviceResolution != resolution ) {
			SqlitePreferences.Update("videoDeviceResolution", resolution, true);
			preferences.videoDeviceResolution = resolution;
		}

		string framerate = getSelectedFramerate();
		if( preferences.videoDeviceFramerate != framerate ) {
			SqlitePreferences.Update("videoDeviceFramerate", framerate, true);
			preferences.videoDeviceFramerate = framerate; //if it has decimals, separator should be a point
		}

		int selected_camera_stop_after = Convert.ToInt32(spin_camera_stop_after.Value);
		if(! check_camera_stop_after.Active)
			selected_camera_stop_after = 0;
		if( preferences.videoStopAfter != selected_camera_stop_after) {
			SqlitePreferences.Update("videoStopAfter", selected_camera_stop_after.ToString(), true);
			preferences.videoStopAfter = selected_camera_stop_after;
		}

		if(preferences.CSVExportDecimalSeparator == "POINT" && PreferencesWindowBox.radio_export_latin.Active)
		{
			SqlitePreferences.Update("CSVExportDecimalSeparator","COMMA", true); 
			preferences.CSVExportDecimalSeparator = "COMMA";
		}
		else if(preferences.CSVExportDecimalSeparator == "COMMA" && ! PreferencesWindowBox.radio_export_latin.Active)
		{
			SqlitePreferences.Update("CSVExportDecimalSeparator","POINT", true); 
			preferences.CSVExportDecimalSeparator = "POINT";
		}
	
		string selectedLanguage = getSelectedLanguage();

		//if there was a language on SQL but now "detected" is selected, put "" in language on SQL
		if(preferences.language != "" && radio_language_detected.Active) {
			SqlitePreferences.Update("language", "", true);
			preferences.language = "";
		}
		//if force a language, and SQL language is != than selected language, change language on SQL
		else if(radio_language_force.Active && preferences.language != selectedLanguage) {
			SqlitePreferences.Update("language", selectedLanguage, true);
			preferences.language = selectedLanguage;
		}


		if( preferences.RGraphsTranslate != PreferencesWindowBox.radio_graphs_translate.Active ) {
			SqlitePreferences.Update("RGraphsTranslate", 
					PreferencesWindowBox.radio_graphs_translate.Active.ToString(), true);
			preferences.RGraphsTranslate = PreferencesWindowBox.radio_graphs_translate.Active;
		}

		if( preferences.useHeightsOnJumpIndexes != PreferencesWindowBox.radio_use_heights_on_jump_indexes.Active ) {
			SqlitePreferences.Update("useHeightsOnJumpIndexes", 
					PreferencesWindowBox.radio_use_heights_on_jump_indexes.Active.ToString(), true);
			preferences.useHeightsOnJumpIndexes = PreferencesWindowBox.radio_use_heights_on_jump_indexes.Active;
		}

		Preferences.pythonVersionEnum pythonVersionFromGUI = get_pythonVersion_from_gui();
		if(preferences.importerPythonVersion != pythonVersionFromGUI)
		{
			SqlitePreferences.Update(SqlitePreferences.ImporterPythonVersion, pythonVersionFromGUI.ToString(), true);
			preferences.importerPythonVersion = pythonVersionFromGUI;
		}


		Sqlite.Close();

		PreferencesWindowBox.preferences_win.Hide();
		PreferencesWindowBox = null;
	}

	private Preferences.MaximizedTypes get_maximized_from_gui()
	{
		if( ! PreferencesWindowBox.check_appearance_maximized.Active )
			return Preferences.MaximizedTypes.NO;

		if( ! PreferencesWindowBox.check_appearance_maximized_undecorated.Active )
			return Preferences.MaximizedTypes.YES;

		return Preferences.MaximizedTypes.YESUNDECORATED;
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

	private Preferences.pythonVersionEnum get_pythonVersion_from_gui()
	{
		if( PreferencesWindowBox.radio_python_2.Active)
			return Preferences.pythonVersionEnum.Python2;
		else //if( PreferencesWindowBox.radio_python_3.Active)
			return Preferences.pythonVersionEnum.Python3;
	}

	public Button Button_accept 
	{
		set { button_accept = value; }
		get { return button_accept;  }
	}

	public Preferences GetPreferences 
	{
		get { return preferences;  }
	}
}
