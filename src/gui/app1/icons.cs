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
 * Copyright (C) 2017-2023   Xavier de Blas <xaviblas@gmail.com>
 */


using System;
using Gtk;
using Gdk;
//using Glade;

public partial class ChronoJumpWindow 
{
	Gtk.Frame frame_image_logo_icon;
	Gtk.Image image_logo_icon;
	Gtk.Image image_logo_icon_transp;
	//Gtk.Image image_home;
	//Gtk.Image image_home1;
	//Gtk.Image image_minimize;
	//Gtk.Image image_minimize1;
	Gtk.Image image_fullscreen;
	Gtk.Image image_fullscreen_encoder;
	Gtk.Image image_fullscreen_exit;
	Gtk.Image image_session_delete;
	Gtk.Image app1s_image_button_close;
	Gtk.Image app1s_image_search;
	Gtk.Image image_db_backup;
	Gtk.Image app1s_image_button_backup_start;
	Gtk.Image image_db_view;
	Gtk.Image image_mode_encoder_capture;
	Gtk.Image image_person;
	Gtk.Image image_person1;
	Gtk.Image image_edit_current_person_h;
	//Gtk.Image image_persons_up;
	//Gtk.Image image_persons_down;
	Gtk.Image image_contacts_exercise_settings;
	Gtk.Image image_contacts_exercise_close;
	Gtk.Image image_contacts_capture_load;
	Gtk.Image image_contacts_session_overview;
	Gtk.Image image_encoder_session_overview;
	Gtk.Image image_contacts_recalculate;
	Gtk.Image image_all_persons_events_h;
	Gtk.Image image_person_delete_h;
	Gtk.Image image_merge_persons;
	Gtk.Image image_chronopic_connect_contacts;
	Gtk.Image image_chronopic_connect_contacts1;
	Gtk.Image image_chronopic_connect_contacts2;
	Gtk.Image image_chronopic_connect_contacts3;
	Gtk.Image image_chronopic_connect_contacts4;
	Gtk.Image image_chronopic_connect_encoder;
	Gtk.Image image_chronopic_connect_encoder1;
	Gtk.Image image_chronopic_connect_encoder2;
	Gtk.Image image_micro_discover_device;
	Gtk.Image image_button_contacts_detect;
	Gtk.Image image_button_encoder_detect;
	Gtk.Image image_button_execute;
	Gtk.Image image_contacts_bell;
	Gtk.Image image_contacts_close_and_capture;
	Gtk.Image image_encoder_close_and_capture;
	Gtk.Image image_tests_capture;
	Gtk.Image image_tests_analyze_jump_rj;
	Gtk.Image image_tests_sprint;
	Gtk.Image image_tests_analyze_general;
	Gtk.Image image_info1;
	Gtk.Image image_info2;
	Gtk.Image image_info3;
	Gtk.Image image_info4;
	Gtk.Image image_run_simple_with_reaction_time_help;
	Gtk.Image image_run_interval_with_reaction_time_help;
	Gtk.Image image_info_sessions_info;
	Gtk.Image image_add_test1;
	Gtk.Image image_add_test2;
	Gtk.Image image_add_test3;
	Gtk.Image image_add_test4;
	Gtk.Image image_add_test5;
	Gtk.Image image_test_inspect;
	Gtk.Image image_test_inspect1;
	Gtk.Image image_button_contacts_exercise_actions_cancel;
	Gtk.Image image_button_contacts_exercise_actions_edit_do;
	Gtk.Image image_button_contacts_exercise_actions_add_do;

	Gtk.Image image_logo_contacts;
	Gtk.Image image_logo_contacts_transp;
	Gtk.Image image_logo_encoder;
	Gtk.Image image_logo_encoder_transp;
	Gtk.Image fullscreen_image_logo;
	Gtk.Image fullscreen_image_logo_transp;

	Gtk.Image image_selector_start_jumps1;
	Gtk.Image image_selector_start_runs1;
	Gtk.Image image_selector_start_isometric;
	Gtk.Image image_selector_start_elastic;
	Gtk.Image image_selector_start_displ_weights;
	Gtk.Image image_selector_start_inertial;

	//Gtk.Image image_down_menu_2_2_2;
	Gtk.Image image_start;
	Gtk.Image image_mode_jumps_small1;
	Gtk.Image image_mode_jumps_reactive_small1;
	Gtk.Image image_mode_runs_small2;
	Gtk.Image image_mode_runs_intervallic_small2;
	Gtk.Image image_mode_race_encoder_small1;

	Gtk.Image image_help_blue;
	Gtk.Image image_help_yellow;
	Gtk.Image image_button_help_close;

	Gtk.Image image_contacts_edit_selected;
	Gtk.Image image_contacts_delete_selected;

	Gtk.Image image_button_contacts_capture_save_image_chart;
	Gtk.Image image_button_contacts_capture_save_image_disk;

	//jump
	Gtk.Image image_jump_execute_air;
	Gtk.Image image_jump_execute_land;

	//run
	Gtk.Image image_run_simple_show_time;
	Gtk.Image image_run_execute_running;
	Gtk.Image image_run_execute_photocell_icon;
	Gtk.Label label_run_execute_photocell_code;
	Gtk.Image image_sprint_analyze_save;
	Gtk.Image image_sprint_analyze_table_save;
	Gtk.Image image_sprint_analyze_table_save_1;
	Gtk.Image image_sprint_export_cancel;
	Gtk.Image image_race_inspector_close;

	Gtk.Image image_contacts_export_individual_current_session;
	Gtk.Image image_contacts_export_individual_all_sessions;
	Gtk.Image image_contacts_export_groupal_current_session;
	Gtk.Image image_contacts_export_cancel;

	//encoder images
	Gtk.Image image_top_eccon;
	Gtk.Image image_encoder_eccon_concentric;
	Gtk.Image image_encoder_eccon_eccentric_concentric;
	Gtk.Image image_top_laterality;
	Gtk.Image image_top_laterality_contacts;
	Gtk.Image image_encoder_laterality_both;
	Gtk.Image image_encoder_laterality_r;
	Gtk.Image image_encoder_laterality_l;
	Gtk.Image image_top_extra_mass;
	Gtk.Image image_extra_mass;
	Gtk.Image image_encoder_inertial_top_weights;
	Gtk.Image image_encoder_inertial_weights;
	Gtk.Image image_recalculate;
	Gtk.Image image_encoder_configuration;
	Gtk.Image image_encoder_exercise;
	Gtk.Image image_button_encoder_exercise_actions_cancel;
	Gtk.Image image_button_encoder_exercise_actions_edit_do;
	Gtk.Image image_button_encoder_exercise_actions_add_do;
	Gtk.Image image_button_radio_encoder_exercise_help;
	Gtk.Image image_encoder_exercise_settings;
	Gtk.Image image_encoder_exercise_close;
	Gtk.Image image_encoder_capture_open;
	Gtk.Image image_encoder_capture_open1;
	Gtk.Image image_encoder_capture_execute;
	Gtk.Image image_encoder_exercise_edit;
	Gtk.Image image_encoder_exercise_add;
	Gtk.Image image_encoder_exercise_delete;
	Gtk.Image image_encoder_1RM_info;
	//Gtk.Image image_encoder_exercise_close;
	Gtk.Image image_inertial_rolled;
	//Gtk.Image image_inertial_half_rolled;
	Gtk.Image image_inertial_extended;
	Gtk.Image image_encoder_calibrate;
	Gtk.Image image_encoder_recalibrate;
	Gtk.Image image_encoder_triggers;
	Gtk.Image image_encoder_rhythm_rest;
	Gtk.Image image_encoder_analyze_mode_options;
	Gtk.Image image_encoder_analyze_mode_options_close;
	Gtk.Image image_encoder_analyze_cancel;
	Gtk.Image image_encoder_analyze_image_compujump_send_email_image;
	Gtk.Image image_encoder_analyze_image_compujump_send_email_send;
	Gtk.Image image_encoder_analyze_check;
	Gtk.Image image_encoder_capture_inertial_ecc;
	Gtk.Image image_encoder_capture_inertial_con;

	//force sensor
	//Gtk.Image image_selector_start_force_sensor;
	Gtk.Image image_force_sensor_adjust_help;
	Gtk.Image image_force_sensor_analyze_load_abcd;
	Gtk.Image image_force_sensor_analyze_load_ab;
	Gtk.Image image_force_sensor_analyze_load_cd;
	Gtk.Image image_force_sensor_tare;
	Gtk.Image image_force_sensor_calibrate;
	Gtk.Image image_force_sensor_capture_adjust;
	Gtk.Image image_force_sensor_capture_adjust1;
	Gtk.Image image_force_sensor_capture_adjust_close;
	Gtk.Image image_force_sensor_analyze_options;
	Gtk.Image image_signal_analyze_move_cd_close;
	Gtk.Image image_force_sensor_analyze_options_close;
	Gtk.Image image_force_sensor_analyze_analyze;
	Gtk.Image image_force_sensor_analyze_options_close_and_analyze;
	Gtk.Image image_force_sensor_exercise_edit;
	Gtk.Image image_force_sensor_exercise_delete;
	Gtk.Image image_force_sensor_laterality_both;
	Gtk.Image image_force_sensor_laterality_r;
	Gtk.Image image_force_sensor_laterality_l;
	Gtk.Image image_ai_export_cancel;
	Gtk.Image image_force_sensor_ai_chained_hscales_link;
	Gtk.Image image_force_sensor_ai_chained_hscales_link_off;
	Gtk.Image image_force_sensor_analyze_load_abcd_north_west;
	Gtk.Image image_force_sensor_analyze_load_abcd_north_east;
	Gtk.Image image_ai_move_cd_pre;
	Gtk.Image image_ai_move_cd_align_left;
	Gtk.Image image_ai_move_cd_align_center;
	Gtk.Image image_ai_move_cd_align_right;
	Gtk.Image image_force_sensor_ai_zoom;
	Gtk.Image image_force_sensor_ai_zoom_out;
	Gtk.Image image_button_force_sensor_analyze_back_to_signal;
	Gtk.Image image_mode_contacts_export_csv;
	Gtk.Image image_force_sensor_analyze_export;

	Gtk.Image image_hscale_force_sensor_ai_a_first;
	Gtk.Image image_hscale_force_sensor_ai_a_last;
	Gtk.Image image_hscale_force_sensor_ai_a_pre;
	Gtk.Image image_hscale_force_sensor_ai_a_pre_1s;
	Gtk.Image image_hscale_force_sensor_ai_a_post;
	Gtk.Image image_hscale_force_sensor_ai_a_post_1s;

	Gtk.Image image_hscale_force_sensor_ai_b_first;
	Gtk.Image image_hscale_force_sensor_ai_b_last;
	Gtk.Image image_hscale_force_sensor_ai_b_pre;
	Gtk.Image image_hscale_force_sensor_ai_b_pre_1s;
	Gtk.Image image_hscale_force_sensor_ai_b_post;
	Gtk.Image image_hscale_force_sensor_ai_b_post_1s;

	//race encoder
	Gtk.Image image_button_contacts_run_encoder_capture_save_image_chart;
	Gtk.Image image_button_contacts_run_encoder_capture_save_image_disk;
	Gtk.Image image_run_encoder_exercise_edit;
	Gtk.Image image_run_encoder_exercise_add;
	Gtk.Image image_run_encoder_exercise_delete;
	Gtk.Image image_run_encoder_exercise_is_sprint_help;
	Gtk.Image image_run_encoder_exercise_angle_default_help;

	//video play icons
	Gtk.Image image_video_play_this_test_contacts;
	Gtk.Image image_video_play_this_test_encoder;
	Gtk.Image image_video_contacts_preview;
	Gtk.Image image_video_encoder_preview;

	Gtk.Image image_encoder_capture_image_save;
	Gtk.Image image_encoder_capture_image_save_1;
	Gtk.Image image_encoder_capture_curves_save;
	Gtk.Image image_encoder_analyze_table_save_1;
	Gtk.Image image_encoder_analyze_image_save_1;
	Gtk.Image image_encoder_analyze_1RM_save_1;
	Gtk.Image image_encoder_analyze_image_save_2;
	Gtk.Image image_forcesensor_analyze_image_save;
	Gtk.Image image_forcesensor_analyze_image_save1;
	Gtk.Image image_forcesensor_analyze_image_save2;
	Gtk.Image image_forcesensor_analyze_image_save3;
	Gtk.Image image_forcesensor_analyze_image_save4;
	Gtk.Image image_forcesensor_analyze_image_save5;
	Gtk.Image image_forcesensor_analyze_image_save6;
	Gtk.Image image_forcesensor_analyze_image_save7;
	Gtk.Image image_forcesensor_analyze_image_save8;
	Gtk.Image image_forcesensor_analyze_image_save9;
	Gtk.Image image_sprint_analyze_image_save;
	Gtk.Image image_raceAnalyzer_table_save_grid;
	Gtk.Image image_sprint_table_save_disk;
	Gtk.Image image_sprint_table_save_grid;
	Gtk.Image image_raceAnalyzer_table_save_disk;
	Gtk.Image image_message_permissions_at_boot;
	Gtk.Image image_camera_at_boot;

	private void putNonStandardIcons()
	{
		Pixbuf pixbuf;

		viewport_chronojump_logo.OverrideBackgroundColor (StateFlags.Normal,
				UtilGtk.GetRGBA (UtilGtk.Colors.BLUE_CHRONOJUMP));
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameLogoTransparent);
		image_chronojump_logo.Pixbuf = pixbuf;

		//hide label_version_hidden
		label_version_hidden.OverrideColor (StateFlags.Normal,
				UtilGtk.GetRGBA (UtilGtk.Colors.BLUE_CHRONOJUMP));
		//show label_version on white
		label_version.OverrideColor (StateFlags.Normal,
				UtilGtk.GetRGBA (UtilGtk.Colors.WHITE));

		//change colors of tests mode

		/*
		 * start of material design icons ---->
		 */

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_settings.png");
		image_menu_preferences.Pixbuf = pixbuf;
		image_menu_preferences1.Pixbuf = pixbuf;
		image_contacts_exercise_settings.Pixbuf = pixbuf;
		image_encoder_exercise_settings.Pixbuf = pixbuf;

		//pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "persons_manage.png");
		//image_persons_manage.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_store_blue.png");
		image_menu_news.Pixbuf = pixbuf;
		image_menu_news1.Pixbuf = pixbuf;
		image_news_blue.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_store_yellow.png");
		image_news_yellow.Pixbuf = pixbuf;

		//pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_minimize.png");
		//image_minimize.Pixbuf = pixbuf;
		//image_minimize1.Pixbuf = pixbuf;
		image_fullscreen.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_fullscreen.png");
		image_fullscreen_encoder.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_fullscreen.png");
		image_fullscreen_exit.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_fullscreen_exit.png");

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_quit.png");
		image_menu_quit.Pixbuf = pixbuf;
		image_menu_quit1.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "cloud_blue.png");
		if(Config.ColorBackgroundIsDark)
			pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "cloud_yellow.png");
		image_cloud.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_folders.png");
		if(Config.ColorBackgroundIsDark)
			pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_folders_yellow.png");

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_folders.png");
		image_menu_folders1.Pixbuf = pixbuf;
		image_menu_folders2.Pixbuf = pixbuf;
		image_session_more_window_blue.Pixbuf = pixbuf;
		image_session_more_window_yellow.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_folders_yellow.png");

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_db_backup.png");
		image_db_backup.Pixbuf = pixbuf;
		app1s_image_button_backup_start.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_more_horiz.png");
		image_session_more.Pixbuf = pixbuf;
		image_session_more1.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_help_blue.png");
		image_menu_help.Pixbuf = pixbuf;
		image_menu_help1.Pixbuf = pixbuf;
		image_help_blue.Pixbuf = pixbuf;
		image_jumps_profile_sjl_help.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_help_yellow.png");
		image_help_yellow.Pixbuf = pixbuf;

		image_button_radio_encoder_exercise_help.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_book.png");
		image_menu_help_documents.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_keyboard.png");
		image_menu_help_shortcuts.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_about.png");
		image_menu_help_about.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_warning_red.png");
		image_message_permissions_at_boot.Pixbuf = pixbuf;
		image_button_force_sensor_stiffness_problem.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "muscle-excentric.png");
		image_encoder_capture_inertial_ecc.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "muscle-concentric.png");
		image_top_eccon.Pixbuf = pixbuf;
		image_encoder_capture_inertial_con.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "muscle-concentric.png");
		image_encoder_eccon_concentric.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "muscle-excentric-concentric.png");
		image_encoder_eccon_eccentric_concentric.Pixbuf = pixbuf;


		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "laterality-both.png");
		image_top_laterality.Pixbuf = pixbuf;
		image_top_laterality_contacts.Pixbuf = pixbuf;
		image_encoder_laterality_both.Pixbuf = pixbuf;
		image_force_sensor_laterality_both.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "laterality-right.png");
		image_encoder_laterality_r.Pixbuf = pixbuf;
		image_force_sensor_laterality_r.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "laterality-left.png");
		image_encoder_laterality_l.Pixbuf = pixbuf;
		image_force_sensor_laterality_l.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "extra-mass.png");
		image_top_extra_mass.Pixbuf = pixbuf;
		image_extra_mass.Pixbuf = pixbuf;
		image_encoder_inertial_top_weights.Pixbuf = pixbuf;
		image_encoder_inertial_weights.Pixbuf = pixbuf;
		image_force_sensor_calibrate.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_no_photo.png");
		if(Config.ColorBackgroundIsDark)
			pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_no_photo_yellow.png");
		image_current_person.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_weight.png");
		image_change_modes_encoder_gravitatory.Pixbuf = pixbuf;
		app1s_image_show_data_encoder_grav.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_inertia.png");
		image_change_modes_encoder_inertial.Pixbuf = pixbuf;
		app1s_image_show_data_encoder_inertial.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_jump.png");
		app1s_image_show_data_jumps.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_run.png");
		app1s_image_show_data_runs.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "race_encoder_icon.png");
		app1s_image_show_data_isometric.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "isometric.png");
		app1s_image_show_data_elastic.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "elastic.png");

		image_run_simple_show_time.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_run_time.png");

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_capture.png");
		image_tests_capture.Pixbuf = pixbuf;
		image_mode_encoder_capture.Pixbuf = pixbuf;
		image_contacts_close_and_capture.Pixbuf = pixbuf;
		image_encoder_close_and_capture.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_sprint.png");
		image_tests_sprint.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "arrow_back.png");
		image_button_force_sensor_analyze_back_to_signal.Pixbuf = pixbuf;
		image_app1s_button_back.Pixbuf = pixbuf;
		image_app1s_button_import_from_csv_errors_back.Pixbuf = pixbuf;

		//pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_person_pin.png");
		if(Config.ColorBackgroundIsDark)
			pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_person_yellow.png");

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_person.png");
		image_person.Pixbuf = pixbuf;
		image_person1.Pixbuf = pixbuf;
		app1s_image_show_data_persons.Pixbuf = pixbuf;
		image_persons_manage.Pixbuf = pixbuf;

		image_person_manage_blue.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_person.png");
		image_person_manage_yellow.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_person_yellow.png");

		image_merge_persons.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "merge.png");

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_info.png");
		image_info1.Pixbuf = pixbuf;
		image_info2.Pixbuf = pixbuf;
		image_info3.Pixbuf = pixbuf;
		image_info4.Pixbuf = pixbuf;
		image_run_simple_with_reaction_time_help.Pixbuf = pixbuf;
		image_run_interval_with_reaction_time_help.Pixbuf = pixbuf;
		image_run_encoder_exercise_is_sprint_help.Pixbuf = pixbuf;
		image_run_encoder_exercise_angle_default_help.Pixbuf = pixbuf;
		image_info_sessions_info.Pixbuf = pixbuf;
		image_encoder_1RM_info.Pixbuf = pixbuf;
		image_force_sensor_adjust_help.Pixbuf = pixbuf;

		//pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_add_test.png");
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_add.png");
		image_encoder_exercise_add.Pixbuf = pixbuf;
		image_add_test1.Pixbuf = pixbuf;
		image_add_test2.Pixbuf = pixbuf;
		image_add_test3.Pixbuf = pixbuf;
		image_add_test4.Pixbuf = pixbuf;
		image_add_test5.Pixbuf = pixbuf;
		image_run_encoder_exercise_add.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_test_inspect.png");
		image_test_inspect.Pixbuf = pixbuf;
		image_test_inspect1.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_chronopic_connect.png");
		image_chronopic_connect_contacts.Pixbuf = pixbuf;
		image_chronopic_connect_contacts1.Pixbuf = pixbuf;
		image_chronopic_connect_contacts2.Pixbuf = pixbuf;
		image_chronopic_connect_contacts3.Pixbuf = pixbuf;
		image_chronopic_connect_contacts4.Pixbuf = pixbuf;
		image_chronopic_connect_encoder.Pixbuf = pixbuf;
		image_chronopic_connect_encoder1.Pixbuf = pixbuf;
		image_chronopic_connect_encoder2.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_chronopic_connect_yellow.png");
		image_micro_discover_device.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_recalculate.png");
		image_recalculate.Pixbuf = pixbuf;
		image_contacts_recalculate.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_build_24.png");
		image_contacts_repair_selected.Pixbuf = pixbuf;
		image_encoder_configuration.Pixbuf = pixbuf;
		image_force_sensor_capture_adjust.Pixbuf = pixbuf;
		image_force_sensor_capture_adjust1.Pixbuf = pixbuf;
		image_force_sensor_analyze_options.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_build_16.png");
		image_encoder_analyze_mode_options.Pixbuf = pixbuf;

		//assign here to have gui ok and not having chronojump logo at top right outside the screen
		//all these will change when mode is selected
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_weight.png");
		image_encoder_exercise.Pixbuf = pixbuf;


		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "folder_new.png");
		image_session_new1.Pixbuf = pixbuf;
		image_session_new3.Pixbuf = pixbuf;
		image_session_new_blue.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "folder_new_yellow.png");
		image_session_new_yellow.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "folder_open.png");
		image_session_load1.Pixbuf = pixbuf;
		image_session_load2.Pixbuf = pixbuf;
		image_session_load3_blue.Pixbuf = pixbuf;
		app1s_image_load.Pixbuf = pixbuf;
		app1s_image_open_database.Pixbuf = pixbuf;
		app1s_image_button_backup_select.Pixbuf = pixbuf;
		app1s_image_button_export_select.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "folder_open_yellow.png");
		image_session_load3_yellow.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "folder_open_set.png");
		image_encoder_capture_open.Pixbuf = pixbuf;
		image_encoder_capture_open1.Pixbuf = pixbuf;
		image_contacts_capture_load.Pixbuf = pixbuf;
		image_force_sensor_analyze_load_abcd.Pixbuf = pixbuf;
		image_force_sensor_analyze_load_ab.Pixbuf = pixbuf;
		image_force_sensor_analyze_load_cd.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "zero.png");
		image_force_sensor_tare.Pixbuf = pixbuf;
		//pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "run_2x.png");

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_capture.png");
		image_button_contacts_detect.Pixbuf = pixbuf;
		image_button_encoder_detect.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_capture_big.png");
		image_encoder_capture_execute.Pixbuf = pixbuf;
		image_button_execute.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_edit.png");
		image_encoder_exercise_edit.Pixbuf = pixbuf;
		image_force_sensor_exercise_edit.Pixbuf = pixbuf;
		image_run_encoder_exercise_edit.Pixbuf = pixbuf;
		image_edit_current_person_h.Pixbuf = pixbuf;
		image_session_edit2.Pixbuf = pixbuf;
		app1s_image_edit.Pixbuf = pixbuf;
		image_contacts_edit_selected.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_close.png");
		image_contacts_exercise_close.Pixbuf = pixbuf;
		image_race_inspector_close.Pixbuf = pixbuf;
		image_encoder_exercise_close.Pixbuf = pixbuf;
		image_signal_analyze_move_cd_close.Pixbuf = pixbuf;
		image_force_sensor_analyze_options_close.Pixbuf = pixbuf;
		image_encoder_analyze_mode_options_close.Pixbuf = pixbuf;
		image_force_sensor_capture_adjust_close.Pixbuf = pixbuf;
		app1s_image_button_close.Pixbuf = pixbuf;
		image_news_close.Pixbuf = pixbuf;
		image_button_person_close.Pixbuf = pixbuf;
		image_app1s_button_import_from_csv_close.Pixbuf = pixbuf;


		/*
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_up.png");
		image_persons_up.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_down.png");
		image_persons_down.Pixbuf = pixbuf;
		*/
		//pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_down.png");
		//image_down_menu_2_2_2.Pixbuf = pixbuf;
		
		//persons buttons
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_person_add.png");
		image_persons_new_1.Pixbuf = pixbuf;
		//image_persons_new_2.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_group_add.png");
		image_persons_new_plus.Pixbuf = pixbuf;
		//image_persons_new_plus_2.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_person_outline.png");
		image_persons_open_1.Pixbuf = pixbuf;
		image_radio_contacts_results_personCurrent.Pixbuf = pixbuf;
		//dimage_persons_open_2.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_group_outline.png");
		image_persons_open_plus.Pixbuf = pixbuf;
		image_radio_contacts_results_personAll.Pixbuf = pixbuf;
		//image_persons_open_plus_2.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_rest.png");
		image_encoder_rhythm_rest.Pixbuf = pixbuf;
		image_contacts_rest_time_dark_blue.Pixbuf = pixbuf;
		image_encoder_rest_time_dark_blue.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_rest_yellow.png");
		image_encoder_rest_time_clear_yellow.Pixbuf = pixbuf;
		image_contacts_rest_time_clear_yellow.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "folder_check.png");
		image_all_persons_events_h.Pixbuf = pixbuf;
		image_db_view.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "inertial_rolled.png");
		image_inertial_rolled.Pixbuf = pixbuf;
		//pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "inertial_half_rolled.png");
		//image_inertial_half_rolled.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "inertial_extended.png");
		image_inertial_extended.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "calibrate.png");
		image_encoder_calibrate.Pixbuf = pixbuf;
		image_encoder_recalibrate.Pixbuf = pixbuf;

		image_video_contacts_yes1.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "videocamera_on.png");
		image_video_contacts_yes.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "videocamera_on.png");
		image_video_encoder_yes.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "videocamera_on.png");
		image_video_encoder_yes1.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "videocamera_on.png");
		image_video_contacts_no.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "videocamera_off.png");
		image_video_encoder_no.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "videocamera_off.png");

		image_camera_at_boot.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "videocamera_off.png");

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_photo_preview.png");
		image_video_contacts_preview.Pixbuf = pixbuf;
		image_video_encoder_preview.Pixbuf = pixbuf;

		/*
		 * <------ end of material design icons
		 */

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameLogo); //44 px height
		image_logo_contacts.Pixbuf = pixbuf;
		image_logo_encoder.Pixbuf = pixbuf;
		fullscreen_image_logo.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameLogoTransparent40h); //40 px heigh
		image_logo_contacts_transp.Pixbuf = pixbuf;
		image_logo_encoder_transp.Pixbuf = pixbuf;
		fullscreen_image_logo_transp.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameIcon);
		image_logo_icon.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameIconTransp);
		image_logo_icon_transp.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_jump_2x.png");
		image_selector_start_jumps1.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "run_mov_2x.png");
		image_selector_start_runs1.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "isometric_2x.png");
		image_selector_start_isometric.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "elastic_2x.png");
		image_selector_start_elastic.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_weight_mov_2x.png");
		image_selector_start_displ_weights.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_inertia_2x_2col.png");
		image_selector_start_inertial.Pixbuf = pixbuf;
		/*
		 * gui for small screens
		 */

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "start.png");
		image_start.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_jump_simple.png");
		image_mode_jumps_small1.Pixbuf = pixbuf;
		image_change_modes_contacts_jumps_simple.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_jump_reactive.png");
		image_mode_jumps_reactive_small1.Pixbuf = pixbuf;
		image_change_modes_contacts_jumps_reactive.Pixbuf = pixbuf;
		image_tests_analyze_jump_rj.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_run_simple.png");
		image_mode_runs_small2.Pixbuf = pixbuf;
		image_change_modes_contacts_runs_simple.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_run_multiple.png");
		image_mode_runs_intervallic_small2.Pixbuf = pixbuf;
		image_change_modes_contacts_runs_intervallic.Pixbuf = pixbuf;
		image_change_modes_contacts_force_sensor.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "isometric.png");
		image_change_modes_contacts_force_sensor1.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "elastic.png");
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "race_encoder_icon.png");
		image_mode_race_encoder_small1.Pixbuf = pixbuf;
		image_change_modes_contacts_runs_encoder.Pixbuf = pixbuf;

		image_jump_execute_air.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_jump_air.png");
		image_jump_execute_land.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_jump_land.png");

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "run_2x.png");
		image_run_execute_running.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "run_photocell.png");
		image_run_execute_photocell_icon.Pixbuf = pixbuf;
		
		image_check_runI_realtime_rel_abs.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "bar_relative.png");

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameJumpsFallCalculate);
		extra_windows_jumps_image_dj_fall_calculate.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameJumpsFallPredefined);
		extra_windows_jumps_image_dj_fall_predefined.Pixbuf = pixbuf;
		image_tab_jumps_dj_optimal_fall.Pixbuf = pixbuf;

		extra_window_jumps_image_fall.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_fall.png");
		extra_window_jumps_rj_image_fall.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_fall.png");
		extra_window_jumps_image_weight.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "extra-mass.png");
		extra_window_jumps_rj_image_weight.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "extra-mass.png");

		image_tab_jumps_weight_fv_profile.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "jumps-fv.png");
		image_tab_jumps_asymmetry.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "laterality-both.png");


		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "line_session_avg.png");
		image_line_session_avg.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "line_session_max.png");
		image_line_session_max.Pixbuf = pixbuf;
		
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "line_person_avg.png");
		image_line_person_avg.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "line_person_max.png");
		image_line_person_max.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "line_person_max_all_sessions.png");
		image_line_person_max_all_sessions.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameImport);
		image_session_import.Pixbuf = pixbuf;
		image_session_import1.Pixbuf = pixbuf;
		image_session_import1_blue.Pixbuf = pixbuf;
		app1s_image_import.Pixbuf = pixbuf;
		app1s_image_import1.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameImportYellow);
		image_session_import1_yellow.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameExport);
		image_session_export.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "csv.png");
		image_mode_contacts_export_csv.Pixbuf = pixbuf;
		image_force_sensor_analyze_export.Pixbuf = pixbuf;
		image_export_encoder_signal.Pixbuf = pixbuf;

		//reaction times changes
		//UtilGtk.ColorsTestLabel(viewport_chronopics, label_extra_window_radio_reaction_time);
		//UtilGtk.ColorsTestLabel(viewport_chronopics, label_extra_window_radio_reaction_time_animation_lights);
		//UtilGtk.ColorsTestLabel(viewport_chronopics, label_extra_window_radio_reaction_time_flicker);
		//UtilGtk.ColorsTestLabel(viewport_chronopics, label_extra_window_radio_reaction_time_discriminative);
		UtilGtk.ColorsRadio(viewport_chronopics, extra_window_radio_reaction_time);
		UtilGtk.ColorsRadio(viewport_chronopics, extra_window_radio_reaction_time_animation_lights);
		UtilGtk.ColorsRadio(viewport_chronopics, extra_window_radio_reaction_time_flicker);
		UtilGtk.ColorsRadio(viewport_chronopics, extra_window_radio_reaction_time_discriminative);

		//pulses changes
		//UtilGtk.ColorsTestLabel(viewport_chronopics, label_extra_window_radio_pulses_free);
		//UtilGtk.ColorsTestLabel(viewport_chronopics, label_extra_window_radio_pulses_custom);
		UtilGtk.ColorsRadio(viewport_chronopics, extra_window_radio_pulses_free);
		UtilGtk.ColorsRadio(viewport_chronopics, extra_window_radio_pulses_custom);

		//multichronopic changes
		//UtilGtk.ColorsTestLabel(viewport_chronopics, label_extra_window_radio_multichronopic_start);
		//UtilGtk.ColorsTestLabel(viewport_chronopics, label_extra_window_radio_multichronopic_run_analysis);
		UtilGtk.ColorsRadio(viewport_chronopics, extra_window_radio_multichronopic_start);
		UtilGtk.ColorsRadio(viewport_chronopics, extra_window_radio_multichronopic_run_analysis);

		//open buttons (this is shown better in windows than the default open icon)
		//pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameOpen);
		//not changed because it's small. TODO: do bigger
		//image_encoder_capture_open.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_bell_none.png");
		image_contacts_bell.Pixbuf = pixbuf;
		image_encoder_bell.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_delete.png");
		image_person_delete_h.Pixbuf = pixbuf;
		image_contacts_delete_selected.Pixbuf = pixbuf;
		image_jump_type_delete_simple.Pixbuf = pixbuf;
		image_jump_type_delete_reactive.Pixbuf = pixbuf;
		image_run_type_delete_simple.Pixbuf = pixbuf;
		image_run_type_delete_intervallic.Pixbuf = pixbuf;
		image_session_delete.Pixbuf = pixbuf;
		image_encoder_exercise_delete.Pixbuf = pixbuf;
		image_force_sensor_exercise_delete.Pixbuf = pixbuf;
		image_run_encoder_exercise_delete.Pixbuf = pixbuf;
		image_button_cancel.Pixbuf = pixbuf;
		image_button_cancel1.Pixbuf = pixbuf;
		image_encoder_signal_delete.Pixbuf = pixbuf;
		app1s_image_delete.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_cancel.png");
		image_contacts_export_cancel.Pixbuf = pixbuf;
		image_sprint_export_cancel.Pixbuf = pixbuf;
		image_button_contacts_exercise_actions_cancel.Pixbuf = pixbuf;
		image_button_encoder_exercise_actions_cancel.Pixbuf = pixbuf;
		image_encoder_analyze_cancel.Pixbuf = pixbuf;
		image_ai_export_cancel.Pixbuf = pixbuf;
		app1s_image_cancel.Pixbuf = pixbuf;
		image_app1sae_button_cancel.Pixbuf = pixbuf;
		image_app1s_button_delete_cancel.Pixbuf = pixbuf;
		image_app1s_button_delete_close.Pixbuf = pixbuf;
		image_app1s_button_export_cancel.Pixbuf = pixbuf;
		image_app1s_button_export_close.Pixbuf = pixbuf;
		image_app1s_button_backup_cancel_close.Pixbuf = pixbuf;
		image_app1s_button_view_data_folder_close.Pixbuf = pixbuf;
		image_app1s_button_view_data_folder_close.Pixbuf = pixbuf;
		image_app1s_button_cancel1.Pixbuf = pixbuf;
		image_button_help_close.Pixbuf = pixbuf;
		image_button_micro_discover_cancel_close.Pixbuf = pixbuf;

		//accept
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_done_blue.png");
		image_app1sae_button_accept.Pixbuf = pixbuf;
		image_app1s_button_delete_accept.Pixbuf = pixbuf;

		//zoom icons, done like this because there's one zoom icon created ad-hoc, 
		//and is not nice that the other are different for an user theme change

		//video play icons
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "video_play.png");
		image_video_play_this_test_contacts.Pixbuf = pixbuf;
		image_video_play_this_test_encoder.Pixbuf = pixbuf;

		//white background in chronopic viewports
		UtilGtk.DeviceColors(viewport_chronopics, true);
		UtilGtk.DeviceColors(viewport_chronopic_encoder, true);

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_grid_on.png");
		image_contacts_session_overview.Pixbuf = pixbuf;
		image_sprint_analyze_table_save.Pixbuf = pixbuf;
		image_encoder_session_overview.Pixbuf = pixbuf;
		image_encoder_analyze_table_save.Pixbuf = pixbuf;
		image_sprint_table_save_grid.Pixbuf = pixbuf;
		image_raceAnalyzer_table_save_grid.Pixbuf = pixbuf;

		//encoder
		UtilGtk.ColorsRadio(viewport_chronopics, radiobutton_encoder_analyze_powerbars);
		UtilGtk.ColorsRadio(viewport_chronopics, radiobutton_encoder_analyze_cross);
		UtilGtk.ColorsRadio(viewport_chronopics, radiobutton_encoder_analyze_instantaneous);
		UtilGtk.ColorsRadio(viewport_chronopics, radiobutton_encoder_analyze_neuromuscular_profile);
		
		UtilGtk.ColorsCheckbox(viewport_chronopics, check_encoder_analyze_show_position);
		UtilGtk.ColorsCheckbox(viewport_chronopics, check_encoder_analyze_show_speed);
		UtilGtk.ColorsCheckbox(viewport_chronopics, check_encoder_analyze_show_accel);
		UtilGtk.ColorsCheckbox(viewport_chronopics, check_encoder_analyze_show_force);
		UtilGtk.ColorsCheckbox(viewport_chronopics, check_encoder_analyze_show_power);
		
		UtilGtk.ColorsCheckbox(viewport_chronopics, check_encoder_analyze_show_range);
		UtilGtk.ColorsCheckbox(viewport_chronopics, check_encoder_analyze_show_time_to_peak_power);

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_analyze.png");
		image_button_contacts_capture_save_image_chart.Pixbuf = pixbuf;
		image_encoder_analyze_stats.Pixbuf = pixbuf;
		image_encoder_analyze_mode_options_close_and_analyze.Pixbuf = pixbuf;
		image_encoder_capture_image_save.Pixbuf = pixbuf;
		image_encoder_analyze_image_save.Pixbuf = pixbuf;
		image_forcesensor_analyze_save_signal.Pixbuf = pixbuf;
		image_forcesensor_analyze_save_rfd_auto.Pixbuf = pixbuf;
		image_forcesensor_analyze_save_rfd_manual.Pixbuf = pixbuf;
		image_force_sensor_analyze_analyze.Pixbuf = pixbuf;
		image_force_sensor_analyze_options_close_and_analyze.Pixbuf = pixbuf;
		image_jumps_profile_save.Pixbuf = pixbuf;
		image_jumps_dj_optimal_fall_save.Pixbuf = pixbuf;
		image_jumps_weight_fv_profile_save.Pixbuf = pixbuf;
		image_jumps_asymmetry_save.Pixbuf = pixbuf;
		image_jumps_evolution_save.Pixbuf = pixbuf;
		image_runs_evolution_save.Pixbuf = pixbuf;
		image_jumps_rj_fatigue_save.Pixbuf = pixbuf;
		image_sprint_analyze_save.Pixbuf = pixbuf;
		image_button_contacts_run_encoder_capture_save_image_chart.Pixbuf = pixbuf;
		image_encoder_analyze_image_compujump_send_email_image.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "save.png");
		image_button_contacts_capture_save_image_disk.Pixbuf = pixbuf;
		image_jumps_rj_fatigue_image_save.Pixbuf = pixbuf;
		image_sprint_analyze_table_save_1.Pixbuf = pixbuf;
		image_button_contacts_exercise_actions_edit_do.Pixbuf = pixbuf;
		image_button_contacts_exercise_actions_add_do.Pixbuf = pixbuf;
		image_button_encoder_exercise_actions_edit_do.Pixbuf = pixbuf;
		image_button_encoder_exercise_actions_add_do.Pixbuf = pixbuf;
		image_encoder_capture_curves_save.Pixbuf = pixbuf;
		image_encoder_analyze_table_save_1.Pixbuf = pixbuf;
		image_encoder_capture_image_save_1.Pixbuf = pixbuf;
		image_encoder_analyze_image_save_1.Pixbuf = pixbuf;
		image_encoder_analyze_1RM_save_1.Pixbuf = pixbuf;
		image_encoder_analyze_image_save_2.Pixbuf = pixbuf;
		image_forcesensor_analyze_image_save.Pixbuf = pixbuf;
		image_forcesensor_analyze_image_save1.Pixbuf = pixbuf;
		image_forcesensor_analyze_image_save2.Pixbuf = pixbuf;
		image_forcesensor_analyze_image_save3.Pixbuf = pixbuf;
		image_forcesensor_analyze_image_save4.Pixbuf = pixbuf;
		image_forcesensor_analyze_image_save5.Pixbuf = pixbuf;
		image_forcesensor_analyze_image_save6.Pixbuf = pixbuf;
		image_forcesensor_analyze_image_save7.Pixbuf = pixbuf;
		image_forcesensor_analyze_image_save8.Pixbuf = pixbuf;
		image_forcesensor_analyze_image_save9.Pixbuf = pixbuf;
		image_runs_evolution_analyze_image_save.Pixbuf = pixbuf;
		image_button_contacts_run_encoder_capture_save_image_disk.Pixbuf = pixbuf;
		image_sprint_analyze_image_save.Pixbuf = pixbuf;
		image_sprint_table_save_disk.Pixbuf = pixbuf;
		image_raceAnalyzer_table_save_disk.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_email.png");
		image_encoder_analyze_image_compujump_send_email_send.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_analyze_general.png");
		image_tests_analyze_general.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "jumps-profile-pie.png");
		image_tab_jumps_profile.Pixbuf = pixbuf;
		
		// execute tests --->
		/*
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "gtk-apply.png"); //trying high contrast (black)
		image_button_execute.Pixbuf = pixbuf;
		image_encoder_capture_execute.Pixbuf = pixbuf;
		*/

		//pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "finish.png"); //gnome (white)
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "floppy.png");
		image_button_finish.Pixbuf = pixbuf;
		image_button_finish1.Pixbuf = pixbuf;

		//pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "gtk-cancel.png"); //high contrast (black)
		//image_button_cancel.Pixbuf = pixbuf;
		// <--- end of execute tests


		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeIndividualCurrentSetIcon);
		image_sprint_analyze_individual_current_set.Pixbuf = pixbuf;
		image_encoder_analyze_individual_current_set.Pixbuf = pixbuf;
		image_force_sensor_analyze_individual_current_set.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeIndividualCurrentSessionIcon);
		image_contacts_export_individual_current_session.Pixbuf = pixbuf;
		image_sprint_analyze_individual_current_session.Pixbuf = pixbuf;
		image_encoder_analyze_individual_current_session.Pixbuf = pixbuf;
		image_force_sensor_analyze_individual_current_session.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeIndividualAllSessionsIcon);
		image_contacts_export_individual_all_sessions.Pixbuf = pixbuf;
		image_encoder_analyze_individual_all_sessions.Pixbuf = pixbuf;
		image_sprint_analyze_individual_all_sessions.Pixbuf = pixbuf;
		image_force_sensor_analyze_individual_all_sessions.Pixbuf = pixbuf;
		image_tab_jumps_evolution.Pixbuf = pixbuf;
		image_tab_runs_evolution.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeGroupalCurrentSessionIcon);
		image_contacts_export_groupal_current_session.Pixbuf = pixbuf;
		image_sprint_analyze_groupal_current_session.Pixbuf = pixbuf;
		image_encoder_analyze_groupal_current_session.Pixbuf = pixbuf;
		image_force_sensor_analyze_groupal_current_session.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzePowerbarsIcon);
		image_encoder_analyze_powerbars.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeCrossIcon);
		image_encoder_analyze_cross.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyze1RMIcon);
		image_encoder_analyze_1RM.Pixbuf = pixbuf;
		image_encoder_analyze_1RM_save.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeInstantaneousIcon);
		image_encoder_analyze_instantaneous.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeSideIcon);
		image_encoder_analyze_side.Pixbuf = pixbuf;
		image_encoder_analyze_selected_side.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeSuperposeIcon);
		image_encoder_analyze_superpose.Pixbuf = pixbuf;
		image_encoder_analyze_selected_superpose.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeSingleIcon);
		image_encoder_analyze_single.Pixbuf = pixbuf;
		image_encoder_analyze_selected_single.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeAllSetIcon);
		image_encoder_analyze_all_set.Pixbuf = pixbuf;
		image_encoder_analyze_selected_all_set.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeNmpIcon);
		image_encoder_analyze_nmp.Pixbuf = pixbuf;
		
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeEcconTogetherIcon);
		image_encoder_analyze_eccon_together.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeEcconSeparatedIcon);
		image_encoder_analyze_eccon_separated.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzePositionIcon);
		image_encoder_analyze_position.Pixbuf = pixbuf;
		image_encoder_analyze_show_SAFE_position.Pixbuf = pixbuf;
		image_force_sensor_capture_show_distance.Pixbuf = pixbuf;
		image_force_sensor_analyze_show_distance.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeSpeedIcon);
		image_encoder_analyze_speed.Pixbuf = pixbuf;
		image_force_sensor_capture_show_speed.Pixbuf = pixbuf;
		image_force_sensor_analyze_show_speed.Pixbuf = pixbuf;
		image_encoder_analyze_show_SAFE_speed.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeAccelIcon);
		image_encoder_analyze_accel.Pixbuf = pixbuf;
		image_encoder_analyze_show_SAFE_accel.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeForceIcon);
		image_encoder_analyze_force.Pixbuf = pixbuf;
		image_encoder_analyze_show_SAFE_force.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzePowerIcon);
		image_encoder_analyze_power.Pixbuf = pixbuf;
		image_encoder_analyze_show_SAFE_power.Pixbuf = pixbuf;
		image_force_sensor_capture_show_power.Pixbuf = pixbuf;
		image_force_sensor_analyze_show_power.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeMeanIcon);
		image_encoder_analyze_mean.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeMaxIcon);
		image_encoder_analyze_max.Pixbuf = pixbuf;
		//pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeRangeIcon);
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzePositionIcon);
		image_encoder_analyze_range.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeTimeToPPIcon);
		image_encoder_analyze_time_to_pp.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderInertialInstructions);
		image_encoder_inertial_instructions.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_encoder_triggers.png");
		image_encoder_triggers.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_check.png");
		image_encoder_analyze_check.Pixbuf = pixbuf;

		//auto mode
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameAutoPersonSkipIcon);
		image_auto_person_skip.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameAutoPersonRemoveIcon);
		image_auto_person_remove.Pixbuf = pixbuf;
				
		//pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameLogo320); //changed to 270 for the presentation
		//image_presentation_logo.Pixbuf = pixbuf;

		pixbuf = new Pixbuf(null, Util.GetImagePath(false) + "first.png");
		image_hscale_force_sensor_ai_a_first.Pixbuf = pixbuf;
		image_hscale_force_sensor_ai_b_first.Pixbuf = pixbuf;
		pixbuf = new Pixbuf(null, Util.GetImagePath(false) + "left_cut.png");
		image_hscale_force_sensor_ai_a_pre_1s.Pixbuf = pixbuf;
		image_hscale_force_sensor_ai_b_pre_1s.Pixbuf = pixbuf;
		pixbuf = new Pixbuf(null, Util.GetImagePath(false) + "left.png");
		image_hscale_force_sensor_ai_a_pre.Pixbuf = pixbuf;
		image_hscale_force_sensor_ai_b_pre.Pixbuf = pixbuf;
		image_presentation_left.Pixbuf = pixbuf;

		pixbuf = new Pixbuf(null, Util.GetImagePath(false) + "right.png");
		image_hscale_force_sensor_ai_a_post.Pixbuf = pixbuf;
		image_hscale_force_sensor_ai_b_post.Pixbuf = pixbuf;
		image_presentation_right.Pixbuf = pixbuf;
		pixbuf = new Pixbuf(null, Util.GetImagePath(false) + "right_cut.png");
		image_hscale_force_sensor_ai_a_post_1s.Pixbuf = pixbuf;
		image_hscale_force_sensor_ai_b_post_1s.Pixbuf = pixbuf;
		pixbuf = new Pixbuf(null, Util.GetImagePath(false) + "last.png");
		image_hscale_force_sensor_ai_a_last.Pixbuf = pixbuf;
		image_hscale_force_sensor_ai_b_last.Pixbuf = pixbuf;

		pixbuf = new Pixbuf(null, Util.GetImagePath(false) + "link.png");
		image_force_sensor_ai_chained_hscales_link.Pixbuf = pixbuf;
		pixbuf = new Pixbuf(null, Util.GetImagePath(false) + "link_off.png");
		image_force_sensor_ai_chained_hscales_link_off.Pixbuf = pixbuf;

		image_force_sensor_analyze_load_abcd_north_west.Pixbuf = new Pixbuf (null, Util.GetImagePath (false) + "north_west.png");
		image_force_sensor_analyze_load_abcd_north_east.Pixbuf = new Pixbuf (null, Util.GetImagePath (false) + "north_east.png");
		image_ai_move_cd_pre.Pixbuf = new Pixbuf (null, Util.GetImagePath (false) + "move_sides.png");
		image_ai_move_cd_align_left.Pixbuf = new Pixbuf (null, Util.GetImagePath (false) + "align_horiz_left.png");
		image_ai_move_cd_align_center.Pixbuf = new Pixbuf (null, Util.GetImagePath (false) + "align_horiz_center.png");
		image_ai_move_cd_align_right.Pixbuf = new Pixbuf (null, Util.GetImagePath (false) + "align_horiz_right.png");

		app1s_image_search.Pixbuf = new Pixbuf(null, Util.GetImagePath(false) + "search.png");

		pixbuf = new Pixbuf(null, Util.GetImagePath(false) + "zoom_in.png");
		image_force_sensor_ai_zoom.Pixbuf = pixbuf;
		image_jumps_zoom.Pixbuf = pixbuf;
		image_jumps_rj_zoom.Pixbuf = pixbuf;
		image_runs_zoom.Pixbuf = pixbuf;
		image_runs_interval_zoom.Pixbuf = pixbuf;
		image_reaction_times_zoom.Pixbuf = pixbuf;
		image_pulses_zoom.Pixbuf = pixbuf;
		image_multi_chronopic_zoom.Pixbuf = pixbuf;
		image_force_sensor_ai_zoom_out.Pixbuf = new Pixbuf(null, Util.GetImagePath(false) + "zoom_out_red.png");

		image_test_add_edit.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_attachment.png");
	}


	private void connectWidgetsIcons (Gtk.Builder builder)
	{
		frame_image_logo_icon = (Gtk.Frame) builder.GetObject ("frame_image_logo_icon");
		image_logo_icon = (Gtk.Image) builder.GetObject ("image_logo_icon");
		image_logo_icon_transp = (Gtk.Image) builder.GetObject ("image_logo_icon_transp");
		//image_home = (Gtk.Image) builder.GetObject ("image_home");
		//image_home1 = (Gtk.Image) builder.GetObject ("image_home1");
		//image_minimize = (Gtk.Image) builder.GetObject ("image_minimize");
		//image_minimize1 = (Gtk.Image) builder.GetObject ("image_minimize1");
		image_fullscreen = (Gtk.Image) builder.GetObject ("image_fullscreen");
		image_fullscreen_encoder = (Gtk.Image) builder.GetObject ("image_fullscreen_encoder");
		image_fullscreen_exit = (Gtk.Image) builder.GetObject ("image_fullscreen_exit");
		image_session_delete = (Gtk.Image) builder.GetObject ("image_session_delete");
		app1s_image_button_close = (Gtk.Image) builder.GetObject ("app1s_image_button_close");
		app1s_image_search = (Gtk.Image) builder.GetObject ("app1s_image_search");
		image_db_backup = (Gtk.Image) builder.GetObject ("image_db_backup");
		app1s_image_button_backup_start = (Gtk.Image) builder.GetObject ("app1s_image_button_backup_start");
		image_db_view = (Gtk.Image) builder.GetObject ("image_db_view");
		image_mode_encoder_capture = (Gtk.Image) builder.GetObject ("image_mode_encoder_capture");
		image_person = (Gtk.Image) builder.GetObject ("image_person");
		image_person1 = (Gtk.Image) builder.GetObject ("image_person1");
		image_edit_current_person_h = (Gtk.Image) builder.GetObject ("image_edit_current_person_h");
		//image_persons_up = (Gtk.Image) builder.GetObject ("image_persons_up");
		//image_persons_down = (Gtk.Image) builder.GetObject ("image_persons_down");
		image_contacts_exercise_settings = (Gtk.Image) builder.GetObject ("image_contacts_exercise_settings");
		image_contacts_exercise_close = (Gtk.Image) builder.GetObject ("image_contacts_exercise_close");
		image_contacts_capture_load = (Gtk.Image) builder.GetObject ("image_contacts_capture_load");
		image_contacts_session_overview = (Gtk.Image) builder.GetObject ("image_contacts_session_overview");
		image_encoder_session_overview = (Gtk.Image) builder.GetObject ("image_encoder_session_overview");
		image_contacts_recalculate = (Gtk.Image) builder.GetObject ("image_contacts_recalculate");
		image_all_persons_events_h = (Gtk.Image) builder.GetObject ("image_all_persons_events_h");
		image_person_delete_h = (Gtk.Image) builder.GetObject ("image_person_delete_h");
		image_merge_persons = (Gtk.Image) builder.GetObject ("image_merge_persons");
		image_chronopic_connect_contacts = (Gtk.Image) builder.GetObject ("image_chronopic_connect_contacts");
		image_chronopic_connect_contacts1 = (Gtk.Image) builder.GetObject ("image_chronopic_connect_contacts1");
		image_chronopic_connect_contacts2 = (Gtk.Image) builder.GetObject ("image_chronopic_connect_contacts2");
		image_chronopic_connect_contacts3 = (Gtk.Image) builder.GetObject ("image_chronopic_connect_contacts3");
		image_chronopic_connect_contacts4 = (Gtk.Image) builder.GetObject ("image_chronopic_connect_contacts4");
		image_chronopic_connect_encoder = (Gtk.Image) builder.GetObject ("image_chronopic_connect_encoder");
		image_chronopic_connect_encoder1 = (Gtk.Image) builder.GetObject ("image_chronopic_connect_encoder1");
		image_chronopic_connect_encoder2 = (Gtk.Image) builder.GetObject ("image_chronopic_connect_encoder2");
		image_micro_discover_device = (Gtk.Image) builder.GetObject ("image_micro_discover_device");
		image_button_contacts_detect = (Gtk.Image) builder.GetObject ("image_button_contacts_detect");
		image_button_encoder_detect = (Gtk.Image) builder.GetObject ("image_button_encoder_detect");
		image_button_execute = (Gtk.Image) builder.GetObject ("image_button_execute");
		image_contacts_bell = (Gtk.Image) builder.GetObject ("image_contacts_bell");
		image_contacts_close_and_capture = (Gtk.Image) builder.GetObject ("image_contacts_close_and_capture");
		image_encoder_close_and_capture = (Gtk.Image) builder.GetObject ("image_encoder_close_and_capture");
		image_tests_capture = (Gtk.Image) builder.GetObject ("image_tests_capture");
		image_tests_analyze_jump_rj = (Gtk.Image) builder.GetObject ("image_tests_analyze_jump_rj");
		image_tests_sprint = (Gtk.Image) builder.GetObject ("image_tests_sprint");
		image_tests_analyze_general = (Gtk.Image) builder.GetObject ("image_tests_analyze_general");
		image_info1 = (Gtk.Image) builder.GetObject ("image_info1");
		image_info2 = (Gtk.Image) builder.GetObject ("image_info2");
		image_info3 = (Gtk.Image) builder.GetObject ("image_info3");
		image_info4 = (Gtk.Image) builder.GetObject ("image_info4");
		image_run_simple_with_reaction_time_help = (Gtk.Image) builder.GetObject ("image_run_simple_with_reaction_time_help");
		image_run_interval_with_reaction_time_help = (Gtk.Image) builder.GetObject ("image_run_interval_with_reaction_time_help");
		image_info_sessions_info = (Gtk.Image) builder.GetObject ("image_info_sessions_info");
		image_add_test1 = (Gtk.Image) builder.GetObject ("image_add_test1");
		image_add_test2 = (Gtk.Image) builder.GetObject ("image_add_test2");
		image_add_test3 = (Gtk.Image) builder.GetObject ("image_add_test3");
		image_add_test4 = (Gtk.Image) builder.GetObject ("image_add_test4");
		image_add_test5 = (Gtk.Image) builder.GetObject ("image_add_test5");
		image_test_inspect = (Gtk.Image) builder.GetObject ("image_test_inspect");
		image_test_inspect1 = (Gtk.Image) builder.GetObject ("image_test_inspect1");
		image_button_contacts_exercise_actions_cancel = (Gtk.Image) builder.GetObject ("image_button_contacts_exercise_actions_cancel");
		image_button_contacts_exercise_actions_edit_do = (Gtk.Image) builder.GetObject ("image_button_contacts_exercise_actions_edit_do");
		image_button_contacts_exercise_actions_add_do = (Gtk.Image) builder.GetObject ("image_button_contacts_exercise_actions_add_do");

		image_logo_contacts = (Gtk.Image) builder.GetObject ("image_logo_contacts");
		image_logo_contacts_transp = (Gtk.Image) builder.GetObject ("image_logo_contacts_transp");
		image_logo_encoder = (Gtk.Image) builder.GetObject ("image_logo_encoder");
		image_logo_encoder_transp = (Gtk.Image) builder.GetObject ("image_logo_encoder_transp");
		fullscreen_image_logo = (Gtk.Image) builder.GetObject ("fullscreen_image_logo");
		fullscreen_image_logo_transp = (Gtk.Image) builder.GetObject ("fullscreen_image_logo_transp");

		image_selector_start_jumps1 = (Gtk.Image) builder.GetObject ("image_selector_start_jumps1");
		image_selector_start_runs1 = (Gtk.Image) builder.GetObject ("image_selector_start_runs1");
		image_selector_start_isometric = (Gtk.Image) builder.GetObject ("image_selector_start_isometric");
		image_selector_start_elastic = (Gtk.Image) builder.GetObject ("image_selector_start_elastic");
		image_selector_start_displ_weights = (Gtk.Image) builder.GetObject ("image_selector_start_displ_weights");
		image_selector_start_inertial = (Gtk.Image) builder.GetObject ("image_selector_start_inertial");

		//image_down_menu_2_2_2 = (Gtk.Image) builder.GetObject ("image_down_menu_2_2_2");
		image_start = (Gtk.Image) builder.GetObject ("image_start");
		image_mode_jumps_small1 = (Gtk.Image) builder.GetObject ("image_mode_jumps_small1");
		image_mode_jumps_reactive_small1 = (Gtk.Image) builder.GetObject ("image_mode_jumps_reactive_small1");
		image_mode_runs_small2 = (Gtk.Image) builder.GetObject ("image_mode_runs_small2");
		image_mode_runs_intervallic_small2 = (Gtk.Image) builder.GetObject ("image_mode_runs_intervallic_small2");
		image_mode_race_encoder_small1 = (Gtk.Image) builder.GetObject ("image_mode_race_encoder_small1");

		image_help_blue = (Gtk.Image) builder.GetObject ("image_help_blue");
		image_help_yellow = (Gtk.Image) builder.GetObject ("image_help_yellow");
		image_button_help_close = (Gtk.Image) builder.GetObject ("image_button_help_close");

		image_contacts_edit_selected = (Gtk.Image) builder.GetObject ("image_contacts_edit_selected");

		image_contacts_delete_selected = (Gtk.Image) builder.GetObject ("image_contacts_delete_selected");

		image_button_contacts_capture_save_image_chart = (Gtk.Image) builder.GetObject ("image_button_contacts_capture_save_image_chart");
		image_button_contacts_capture_save_image_disk = (Gtk.Image) builder.GetObject ("image_button_contacts_capture_save_image_disk");

		//jump
		image_jump_execute_air = (Gtk.Image) builder.GetObject ("image_jump_execute_air");
		image_jump_execute_land = (Gtk.Image) builder.GetObject ("image_jump_execute_land");

		//run
		image_run_simple_show_time = (Gtk.Image) builder.GetObject ("image_run_simple_show_time");
		image_run_execute_running = (Gtk.Image) builder.GetObject ("image_run_execute_running");
		image_run_execute_photocell_icon = (Gtk.Image) builder.GetObject ("image_run_execute_photocell_icon");
		label_run_execute_photocell_code = (Gtk.Label) builder.GetObject ("label_run_execute_photocell_code");
		image_sprint_analyze_save = (Gtk.Image) builder.GetObject ("image_sprint_analyze_save");
		image_sprint_analyze_table_save = (Gtk.Image) builder.GetObject ("image_sprint_analyze_table_save");
		image_sprint_analyze_table_save_1 = (Gtk.Image) builder.GetObject ("image_sprint_analyze_table_save_1");
		image_sprint_export_cancel = (Gtk.Image) builder.GetObject ("image_sprint_export_cancel");
		image_race_inspector_close = (Gtk.Image) builder.GetObject ("image_race_inspector_close");
		image_sprint_analyze_individual_current_set = (Gtk.Image) builder.GetObject ("image_sprint_analyze_individual_current_set");
		image_contacts_export_individual_current_session = (Gtk.Image) builder.GetObject ("image_contacts_export_individual_current_session");
		image_contacts_export_individual_all_sessions = (Gtk.Image) builder.GetObject ("image_contacts_export_individual_all_sessions");
		image_contacts_export_groupal_current_session = (Gtk.Image) builder.GetObject ("image_contacts_export_groupal_current_session");
		image_contacts_export_cancel = (Gtk.Image) builder.GetObject ("image_contacts_export_cancel");

		//encoder images
		image_top_eccon = (Gtk.Image) builder.GetObject ("image_top_eccon");
		image_encoder_eccon_concentric = (Gtk.Image) builder.GetObject ("image_encoder_eccon_concentric");
		image_encoder_eccon_eccentric_concentric = (Gtk.Image) builder.GetObject ("image_encoder_eccon_eccentric_concentric");
		image_top_laterality = (Gtk.Image) builder.GetObject ("image_top_laterality");
		image_top_laterality_contacts = (Gtk.Image) builder.GetObject ("image_top_laterality_contacts");
		image_encoder_laterality_both = (Gtk.Image) builder.GetObject ("image_encoder_laterality_both");
		image_encoder_laterality_r = (Gtk.Image) builder.GetObject ("image_encoder_laterality_r");
		image_encoder_laterality_l = (Gtk.Image) builder.GetObject ("image_encoder_laterality_l");
		image_top_extra_mass = (Gtk.Image) builder.GetObject ("image_top_extra_mass");
		image_extra_mass = (Gtk.Image) builder.GetObject ("image_extra_mass");
		image_encoder_inertial_top_weights = (Gtk.Image) builder.GetObject ("image_encoder_inertial_top_weights");
		image_encoder_inertial_weights = (Gtk.Image) builder.GetObject ("image_encoder_inertial_weights");
		image_recalculate = (Gtk.Image) builder.GetObject ("image_recalculate");
		image_encoder_configuration = (Gtk.Image) builder.GetObject ("image_encoder_configuration");
		image_encoder_exercise = (Gtk.Image) builder.GetObject ("image_encoder_exercise");
		image_button_encoder_exercise_actions_cancel = (Gtk.Image) builder.GetObject ("image_button_encoder_exercise_actions_cancel");
		image_button_encoder_exercise_actions_edit_do = (Gtk.Image) builder.GetObject ("image_button_encoder_exercise_actions_edit_do");
		image_button_encoder_exercise_actions_add_do = (Gtk.Image) builder.GetObject ("image_button_encoder_exercise_actions_add_do");
		image_button_radio_encoder_exercise_help = (Gtk.Image) builder.GetObject ("image_button_radio_encoder_exercise_help");
		image_encoder_exercise_settings = (Gtk.Image) builder.GetObject ("image_encoder_exercise_settings");
		image_encoder_exercise_close = (Gtk.Image) builder.GetObject ("image_encoder_exercise_close");
		image_encoder_capture_open = (Gtk.Image) builder.GetObject ("image_encoder_capture_open");
		image_encoder_capture_open1 = (Gtk.Image) builder.GetObject ("image_encoder_capture_open1");
		image_encoder_capture_execute = (Gtk.Image) builder.GetObject ("image_encoder_capture_execute");
		image_encoder_exercise_edit = (Gtk.Image) builder.GetObject ("image_encoder_exercise_edit");
		image_encoder_exercise_add = (Gtk.Image) builder.GetObject ("image_encoder_exercise_add");
		image_encoder_exercise_delete = (Gtk.Image) builder.GetObject ("image_encoder_exercise_delete");
		image_encoder_1RM_info = (Gtk.Image) builder.GetObject ("image_encoder_1RM_info");
		//image_encoder_exercise_close = (Gtk.Image) builder.GetObject ("image_encoder_exercise_close");
		image_inertial_rolled = (Gtk.Image) builder.GetObject ("image_inertial_rolled");
		//image_inertial_half_rolled = (Gtk.Image) builder.GetObject ("image_inertial_half_rolled");
		image_inertial_extended = (Gtk.Image) builder.GetObject ("image_inertial_extended");
		image_encoder_calibrate = (Gtk.Image) builder.GetObject ("image_encoder_calibrate");
		image_encoder_recalibrate = (Gtk.Image) builder.GetObject ("image_encoder_recalibrate");
		image_encoder_triggers = (Gtk.Image) builder.GetObject ("image_encoder_triggers");
		image_encoder_rhythm_rest = (Gtk.Image) builder.GetObject ("image_encoder_rhythm_rest");
		image_encoder_analyze_mode_options = (Gtk.Image) builder.GetObject ("image_encoder_analyze_mode_options");
		image_encoder_analyze_mode_options_close = (Gtk.Image) builder.GetObject ("image_encoder_analyze_mode_options_close");
		image_encoder_analyze_cancel = (Gtk.Image) builder.GetObject ("image_encoder_analyze_cancel");
		image_encoder_analyze_image_compujump_send_email_image = (Gtk.Image) builder.GetObject ("image_encoder_analyze_image_compujump_send_email_image");
		image_encoder_analyze_image_compujump_send_email_send = (Gtk.Image) builder.GetObject ("image_encoder_analyze_image_compujump_send_email_send");
		image_encoder_analyze_check = (Gtk.Image) builder.GetObject ("image_encoder_analyze_check");
		image_encoder_capture_inertial_ecc = (Gtk.Image) builder.GetObject ("image_encoder_capture_inertial_ecc");
		image_encoder_capture_inertial_con = (Gtk.Image) builder.GetObject ("image_encoder_capture_inertial_con");

		//force sensor
		//image_selector_start_force_sensor = (Gtk.Image) builder.GetObject ("image_selector_start_force_sensor");
		image_force_sensor_adjust_help = (Gtk.Image) builder.GetObject ("image_force_sensor_adjust_help");
		image_force_sensor_analyze_load_abcd = (Gtk.Image) builder.GetObject ("image_force_sensor_analyze_load_abcd");
		image_force_sensor_analyze_load_ab = (Gtk.Image) builder.GetObject ("image_force_sensor_analyze_load_ab");
		image_force_sensor_analyze_load_cd = (Gtk.Image) builder.GetObject ("image_force_sensor_analyze_load_cd");
		image_force_sensor_tare = (Gtk.Image) builder.GetObject ("image_force_sensor_tare");
		image_force_sensor_calibrate = (Gtk.Image) builder.GetObject ("image_force_sensor_calibrate");
		image_force_sensor_capture_adjust = (Gtk.Image) builder.GetObject ("image_force_sensor_capture_adjust");
		image_force_sensor_capture_adjust1 = (Gtk.Image) builder.GetObject ("image_force_sensor_capture_adjust1");
		image_force_sensor_capture_adjust_close = (Gtk.Image) builder.GetObject ("image_force_sensor_capture_adjust_close");
		image_force_sensor_analyze_options = (Gtk.Image) builder.GetObject ("image_force_sensor_analyze_options");
		image_signal_analyze_move_cd_close = (Gtk.Image) builder.GetObject ("image_signal_analyze_move_cd_close");
		image_force_sensor_analyze_options_close = (Gtk.Image) builder.GetObject ("image_force_sensor_analyze_options_close");
		image_force_sensor_analyze_analyze = (Gtk.Image) builder.GetObject ("image_force_sensor_analyze_analyze");
		image_force_sensor_analyze_options_close_and_analyze = (Gtk.Image) builder.GetObject ("image_force_sensor_analyze_options_close_and_analyze");
		image_force_sensor_exercise_edit = (Gtk.Image) builder.GetObject ("image_force_sensor_exercise_edit");
		image_force_sensor_exercise_delete = (Gtk.Image) builder.GetObject ("image_force_sensor_exercise_delete");
		image_force_sensor_laterality_both = (Gtk.Image) builder.GetObject ("image_force_sensor_laterality_both");
		image_force_sensor_laterality_r = (Gtk.Image) builder.GetObject ("image_force_sensor_laterality_r");
		image_force_sensor_laterality_l = (Gtk.Image) builder.GetObject ("image_force_sensor_laterality_l");
		image_ai_export_cancel = (Gtk.Image) builder.GetObject ("image_ai_export_cancel");
		image_force_sensor_ai_chained_hscales_link = (Gtk.Image) builder.GetObject ("image_force_sensor_ai_chained_hscales_link");
		image_force_sensor_ai_chained_hscales_link_off = (Gtk.Image) builder.GetObject ("image_force_sensor_ai_chained_hscales_link_off");
		image_force_sensor_analyze_load_abcd_north_west = (Gtk.Image) builder.GetObject ("image_force_sensor_analyze_load_abcd_north_west");
		image_force_sensor_analyze_load_abcd_north_east = (Gtk.Image) builder.GetObject ("image_force_sensor_analyze_load_abcd_north_east");
		image_ai_move_cd_pre = (Gtk.Image) builder.GetObject ("image_ai_move_cd_pre");
		image_ai_move_cd_align_left = (Gtk.Image) builder.GetObject ("image_ai_move_cd_align_left");
		image_ai_move_cd_align_center = (Gtk.Image) builder.GetObject ("image_ai_move_cd_align_center");
		image_ai_move_cd_align_right = (Gtk.Image) builder.GetObject ("image_ai_move_cd_align_right");
		image_force_sensor_ai_zoom = (Gtk.Image) builder.GetObject ("image_force_sensor_ai_zoom");
		image_force_sensor_ai_zoom_out = (Gtk.Image) builder.GetObject ("image_force_sensor_ai_zoom_out");
		image_button_force_sensor_analyze_back_to_signal = (Gtk.Image) builder.GetObject ("image_button_force_sensor_analyze_back_to_signal");
		image_mode_contacts_export_csv= (Gtk.Image) builder.GetObject ("image_mode_contacts_export_csv");
		image_force_sensor_analyze_export = (Gtk.Image) builder.GetObject ("image_force_sensor_analyze_export");

		image_hscale_force_sensor_ai_a_first = (Gtk.Image) builder.GetObject ("image_hscale_force_sensor_ai_a_first");
		image_hscale_force_sensor_ai_a_last = (Gtk.Image) builder.GetObject ("image_hscale_force_sensor_ai_a_last");
		image_hscale_force_sensor_ai_a_pre = (Gtk.Image) builder.GetObject ("image_hscale_force_sensor_ai_a_pre");
		image_hscale_force_sensor_ai_a_pre_1s = (Gtk.Image) builder.GetObject ("image_hscale_force_sensor_ai_a_pre_1s");
		image_hscale_force_sensor_ai_a_post = (Gtk.Image) builder.GetObject ("image_hscale_force_sensor_ai_a_post");
		image_hscale_force_sensor_ai_a_post_1s = (Gtk.Image) builder.GetObject ("image_hscale_force_sensor_ai_a_post_1s");

		image_hscale_force_sensor_ai_b_first = (Gtk.Image) builder.GetObject ("image_hscale_force_sensor_ai_b_first");
		image_hscale_force_sensor_ai_b_last = (Gtk.Image) builder.GetObject ("image_hscale_force_sensor_ai_b_last");
		image_hscale_force_sensor_ai_b_pre = (Gtk.Image) builder.GetObject ("image_hscale_force_sensor_ai_b_pre");
		image_hscale_force_sensor_ai_b_pre_1s = (Gtk.Image) builder.GetObject ("image_hscale_force_sensor_ai_b_pre_1s");
		image_hscale_force_sensor_ai_b_post = (Gtk.Image) builder.GetObject ("image_hscale_force_sensor_ai_b_post");
		image_hscale_force_sensor_ai_b_post_1s = (Gtk.Image) builder.GetObject ("image_hscale_force_sensor_ai_b_post_1s");

		//race encoder
		image_button_contacts_run_encoder_capture_save_image_chart = (Gtk.Image) builder.GetObject ("image_button_contacts_run_encoder_capture_save_image_chart");
		image_button_contacts_run_encoder_capture_save_image_disk = (Gtk.Image) builder.GetObject ("image_button_contacts_run_encoder_capture_save_image_disk");
		image_run_encoder_exercise_edit = (Gtk.Image) builder.GetObject ("image_run_encoder_exercise_edit");
		image_run_encoder_exercise_add = (Gtk.Image) builder.GetObject ("image_run_encoder_exercise_add");
		image_run_encoder_exercise_delete = (Gtk.Image) builder.GetObject ("image_run_encoder_exercise_delete");
		image_run_encoder_exercise_is_sprint_help = (Gtk.Image) builder.GetObject ("image_run_encoder_exercise_is_sprint_help");
		image_run_encoder_exercise_angle_default_help = (Gtk.Image) builder.GetObject ("image_run_encoder_exercise_angle_default_help");

		//video play icons
		image_video_play_this_test_contacts = (Gtk.Image) builder.GetObject ("image_video_play_this_test_contacts");
		image_video_play_this_test_encoder = (Gtk.Image) builder.GetObject ("image_video_play_this_test_encoder");
		image_video_contacts_preview = (Gtk.Image) builder.GetObject ("image_video_contacts_preview");
		image_video_encoder_preview = (Gtk.Image) builder.GetObject ("image_video_encoder_preview");

		image_encoder_capture_image_save = (Gtk.Image) builder.GetObject ("image_encoder_capture_image_save");
		image_encoder_capture_image_save_1 = (Gtk.Image) builder.GetObject ("image_encoder_capture_image_save_1");
		image_encoder_capture_curves_save = (Gtk.Image) builder.GetObject ("image_encoder_capture_curves_save");
		image_encoder_analyze_table_save_1 = (Gtk.Image) builder.GetObject ("image_encoder_analyze_table_save_1");
		image_encoder_analyze_image_save_1 = (Gtk.Image) builder.GetObject ("image_encoder_analyze_image_save_1");
		image_encoder_analyze_1RM_save_1 = (Gtk.Image) builder.GetObject ("image_encoder_analyze_1RM_save_1");
		image_encoder_analyze_image_save_2 = (Gtk.Image) builder.GetObject ("image_encoder_analyze_image_save_2");
		image_forcesensor_analyze_image_save = (Gtk.Image) builder.GetObject ("image_forcesensor_analyze_image_save");
		image_forcesensor_analyze_image_save1 = (Gtk.Image) builder.GetObject ("image_forcesensor_analyze_image_save1");
		image_forcesensor_analyze_image_save2 = (Gtk.Image) builder.GetObject ("image_forcesensor_analyze_image_save2");
		image_forcesensor_analyze_image_save3 = (Gtk.Image) builder.GetObject ("image_forcesensor_analyze_image_save3");
		image_forcesensor_analyze_image_save4 = (Gtk.Image) builder.GetObject ("image_forcesensor_analyze_image_save4");
		image_forcesensor_analyze_image_save5 = (Gtk.Image) builder.GetObject ("image_forcesensor_analyze_image_save5");
		image_forcesensor_analyze_image_save6 = (Gtk.Image) builder.GetObject ("image_forcesensor_analyze_image_save6");
		image_forcesensor_analyze_image_save7 = (Gtk.Image) builder.GetObject ("image_forcesensor_analyze_image_save7");
		image_forcesensor_analyze_image_save8 = (Gtk.Image) builder.GetObject ("image_forcesensor_analyze_image_save8");
		image_forcesensor_analyze_image_save9 = (Gtk.Image) builder.GetObject ("image_forcesensor_analyze_image_save9");
		image_sprint_analyze_image_save = (Gtk.Image) builder.GetObject ("image_sprint_analyze_image_save");
		image_raceAnalyzer_table_save_grid = (Gtk.Image) builder.GetObject ("image_raceAnalyzer_table_save_grid");
		image_sprint_table_save_disk = (Gtk.Image) builder.GetObject ("image_sprint_table_save_disk");
		image_sprint_table_save_grid = (Gtk.Image) builder.GetObject ("image_sprint_table_save_grid");
		image_raceAnalyzer_table_save_disk = (Gtk.Image) builder.GetObject ("image_raceAnalyzer_table_save_disk");
		image_message_permissions_at_boot = (Gtk.Image) builder.GetObject ("image_message_permissions_at_boot");
		image_camera_at_boot = (Gtk.Image) builder.GetObject ("image_camera_at_boot");
	}
}
