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
 * Copyright (C) 2017   Xavier de Blas <xaviblas@gmail.com> 
 */


using System;
using Gtk;
using Gdk;
using Glade;

public partial class ChronoJumpWindow 
{
	[Widget] Gtk.Image image_mode_main_menu;
	//[Widget] Gtk.Image image_home;
	//[Widget] Gtk.Image image_home1;
	[Widget] Gtk.Image image_settings2;
	[Widget] Gtk.Image image_minimize;
	//[Widget] Gtk.Image image_minimize1;
	[Widget] Gtk.Image image_quit3;
	[Widget] Gtk.Image image_quit_from_app1;
	[Widget] Gtk.Image image_session_new2;
	[Widget] Gtk.Image image_session_edit;
	[Widget] Gtk.Image image_session_delete;
	[Widget] Gtk.Image image_session_preferences;
	[Widget] Gtk.Image image_mode_encoder_capture;
	[Widget] Gtk.Image image_current_person_zoom;
	[Widget] Gtk.Image image_current_person_zoom_h;
	[Widget] Gtk.Image image_person;
	[Widget] Gtk.Image image_person1;
	[Widget] Gtk.Image image_edit_current_person;
	[Widget] Gtk.Image image_edit_current_person_h;
	//[Widget] Gtk.Image image_persons_up;
	//[Widget] Gtk.Image image_persons_down;
	[Widget] Gtk.Image image_contacts_exercise;
	[Widget] Gtk.Image image_contacts_exercise_close;
	[Widget] Gtk.Image image_contacts_capture_load;
	[Widget] Gtk.Image image_contacts_session_overview;
	[Widget] Gtk.Image image_encoder_session_overview;
	[Widget] Gtk.Image image_contacts_recalculate;
	[Widget] Gtk.Image image_rest;
	[Widget] Gtk.Image image_all_persons_events;
	[Widget] Gtk.Image image_all_persons_events_h;
	[Widget] Gtk.Image image_person_delete;
	[Widget] Gtk.Image image_person_delete_h;
	[Widget] Gtk.Image image_chronopic_connect_contacts;
	[Widget] Gtk.Image image_chronopic_connect_encoder;
	[Widget] Gtk.Image image_chronopic_connect_encoder1;
	[Widget] Gtk.Image image_button_execute;
	[Widget] Gtk.Image image_contacts_bell;
	[Widget] Gtk.Image image_contacts_close_and_capture;
	[Widget] Gtk.Image image_encoder_close_and_capture;
	[Widget] Gtk.Image image_tests_capture;
	[Widget] Gtk.Image image_tests_sprint;
	[Widget] Gtk.Image image_tests_analyze_general;
	[Widget] Gtk.Image image_tests_analyze_general1;
	[Widget] Gtk.Image image_info1;
	[Widget] Gtk.Image image_info2;
	[Widget] Gtk.Image image_info3;
	[Widget] Gtk.Image image_info4;
	[Widget] Gtk.Image image_run_simple_with_reaction_time_help;
	[Widget] Gtk.Image image_run_interval_with_reaction_time_help;
	[Widget] Gtk.Image image_info_sessions_info;
	[Widget] Gtk.Image image_add_test1;
	[Widget] Gtk.Image image_add_test2;
	[Widget] Gtk.Image image_add_test3;
	[Widget] Gtk.Image image_add_test4;
	[Widget] Gtk.Image image_add_test5;
	[Widget] Gtk.Image image_test_inspect;
	[Widget] Gtk.Image image_test_inspect1;

	[Widget] Gtk.Image image_selector_start_jumps;
	[Widget] Gtk.Image image_selector_start_runs;
	[Widget] Gtk.Image image_selector_start_encoder;
	[Widget] Gtk.Image image_selector_start_rt;
	[Widget] Gtk.Image image_selector_start_other;
	[Widget] Gtk.Image image_selector_start_back;

	[Widget] Gtk.Image image_menuitem_mode_jumps_simple;
	[Widget] Gtk.Image image_menuitem_mode_jumps_reactive;
	[Widget] Gtk.Image image_menuitem_mode_runs_simple;
	[Widget] Gtk.Image image_menuitem_mode_runs_intervallic;
	[Widget] Gtk.Image image_menuitem_mode_race_encoder;
	[Widget] Gtk.Image image_menuitem_mode_power_gravitatory;
	[Widget] Gtk.Image image_menuitem_mode_power_inertial;
	[Widget] Gtk.Image image_menuitem_mode_force_sensor;
	[Widget] Gtk.Image image_menuitem_mode_reaction_time;
	[Widget] Gtk.Image image_menuitem_mode_other;

	[Widget] Gtk.Image image_mode_jumps_small;
	[Widget] Gtk.Image image_mode_jumps_reactive_small;
	[Widget] Gtk.Image image_mode_runs_small;
	[Widget] Gtk.Image image_mode_runs_small1;
	[Widget] Gtk.Image image_mode_runs_intervallic_small;
	[Widget] Gtk.Image image_mode_runs_intervallic_small1;
	[Widget] Gtk.Image image_mode_race_encoder_small;
	[Widget] Gtk.Image image_mode_pulses_small;
	[Widget] Gtk.Image image_mode_multi_chronopic_small;
	[Widget] Gtk.Image image_mode_encoder_gravitatory;
	[Widget] Gtk.Image image_mode_encoder_inertial;

	[Widget] Gtk.Label label_start_selector_jumps;
	[Widget] Gtk.Label label_start_selector_races1;
	[Widget] Gtk.Label label_start_selector_races;
	[Widget] Gtk.Label label_start_selector_encoder;

	//run
	[Widget] Gtk.Image image_run_execute_running;
	[Widget] Gtk.Image image_run_execute_photocell;

	//encoder images
	[Widget] Gtk.Image image_top_eccon;
	[Widget] Gtk.Image image_encoder_eccon_concentric;
	[Widget] Gtk.Image image_encoder_eccon_eccentric_concentric;
	[Widget] Gtk.Image image_top_laterality;
	[Widget] Gtk.Image image_encoder_laterality_both;
	[Widget] Gtk.Image image_encoder_laterality_r;
	[Widget] Gtk.Image image_encoder_laterality_l;
	[Widget] Gtk.Image image_top_extra_mass;
	[Widget] Gtk.Image image_extra_mass;
	[Widget] Gtk.Image image_encoder_inertial_top_weights;
	[Widget] Gtk.Image image_encoder_inertial_weights;
	[Widget] Gtk.Image image_recalculate;
	[Widget] Gtk.Image image_encoder_configuration;
	[Widget] Gtk.Image image_encoder_exercise;
	[Widget] Gtk.Image image_encoder_exercise_close;
	[Widget] Gtk.Image image_encoder_capture_open;
	[Widget] Gtk.Image image_encoder_capture_open1;
	[Widget] Gtk.Image image_encoder_capture_execute;
	[Widget] Gtk.Image image_encoder_exercise_edit;
	[Widget] Gtk.Image image_encoder_exercise_add;
	[Widget] Gtk.Image image_encoder_exercise_delete;
	[Widget] Gtk.Image image_encoder_1RM_info;
	//[Widget] Gtk.Image image_encoder_exercise_close;
	[Widget] Gtk.Image image_inertial_rolled;
	//[Widget] Gtk.Image image_inertial_half_rolled;
	[Widget] Gtk.Image image_inertial_extended;
	[Widget] Gtk.Image image_encoder_calibrate;
	[Widget] Gtk.Image image_encoder_recalibrate;
	[Widget] Gtk.Image image_encoder_triggers;
	[Widget] Gtk.Image image_encoder_rhythm_rest;
	[Widget] Gtk.Image image_encoder_analyze_mode_options;
	[Widget] Gtk.Image image_encoder_analyze_mode_options_close;
	[Widget] Gtk.Image image_encoder_analyze_cancel;
	[Widget] Gtk.Image image_encoder_analyze_image_compujump_send_email_image;
	[Widget] Gtk.Image image_encoder_analyze_image_compujump_send_email_send;
	[Widget] Gtk.Image image_encoder_analyze_check;

	//force sensor
	[Widget] Gtk.Image image_selector_start_force_sensor;
	[Widget] Gtk.Image image_force_sensor_adjust_help;
	[Widget] Gtk.Image image_force_sensor_analyze_load;
	[Widget] Gtk.Image image_force_sensor_tare;
	[Widget] Gtk.Image image_force_sensor_calibrate;
	[Widget] Gtk.Image image_force_sensor_open_folder;
	[Widget] Gtk.Image image_force_sensor_check_version;
	[Widget] Gtk.Image image_force_sensor_capture_adjust;
	[Widget] Gtk.Image image_force_sensor_capture_adjust_close;
	[Widget] Gtk.Image image_force_sensor_analyze_options;
	[Widget] Gtk.Image image_force_sensor_analyze_options_close;
	[Widget] Gtk.Image image_force_sensor_analyze_analyze;
	[Widget] Gtk.Image image_force_sensor_analyze_options_close_and_analyze;
	[Widget] Gtk.Image image_force_sensor_exercise_edit;
	[Widget] Gtk.Image image_force_sensor_exercise_delete;
	[Widget] Gtk.Image image_force_sensor_laterality_both;
	[Widget] Gtk.Image image_force_sensor_laterality_r;
	[Widget] Gtk.Image image_force_sensor_laterality_l;

	//race encoder
	[Widget] Gtk.Image image_race_encoder_open_folder;
	[Widget] Gtk.Image image_run_encoder_exercise_edit;
	[Widget] Gtk.Image image_run_encoder_exercise_add;
	[Widget] Gtk.Image image_run_encoder_exercise_delete;
	[Widget] Gtk.Image image_run_encoder_analyze_load;

	//video play icons
	[Widget] Gtk.Image image_video_play_this_test_contacts;
	[Widget] Gtk.Image image_video_play_this_test_encoder;
	[Widget] Gtk.Image image_video_play_selected_jump;
	[Widget] Gtk.Image image_video_play_selected_jump_rj;
	[Widget] Gtk.Image image_video_play_selected_run;
	[Widget] Gtk.Image image_video_play_selected_run_interval;
	[Widget] Gtk.Image image_video_play_selected_pulse;
	[Widget] Gtk.Image image_video_play_selected_reaction_time;
	[Widget] Gtk.Image image_video_play_selected_multi_chronopic;
	[Widget] Gtk.Image image_video_contacts_preview;
	[Widget] Gtk.Image image_video_encoder_preview;

	[Widget] Gtk.Image image_encoder_capture_curves_save;
	[Widget] Gtk.Image image_encoder_analyze_table_save_1;
	[Widget] Gtk.Image image_encoder_analyze_image_save_1;
	[Widget] Gtk.Image image_encoder_analyze_1RM_save_1;
	[Widget] Gtk.Image image_encoder_analyze_image_save_2;
	[Widget] Gtk.Image image_forcesensor_analyze_image_save;
	[Widget] Gtk.Image image_forcesensor_analyze_image_save1;
	[Widget] Gtk.Image image_forcesensor_analyze_image_save2;
	[Widget] Gtk.Image image_forcesensor_analyze_image_save3;
	[Widget] Gtk.Image image_forcesensor_analyze_image_save4;
	[Widget] Gtk.Image image_forcesensor_analyze_image_save5;
	[Widget] Gtk.Image image_forcesensor_analyze_image_save6;
	[Widget] Gtk.Image image_forcesensor_analyze_image_save7;
	[Widget] Gtk.Image image_sprint_analyze_image_save;
	[Widget] Gtk.Image image_message_permissions_at_boot;
	[Widget] Gtk.Image image_camera_at_boot;

	private void putNonStandardIcons()
	{
		Pixbuf pixbuf;

		viewport_chronojump_logo.ModifyBg(StateType.Normal, new Gdk.Color(0x0e,0x1e,0x46));
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameLogoTransparent);
		image_chronojump_logo.Pixbuf = pixbuf;

		//hide label_version_hidden
		label_version_hidden.ModifyFg(StateType.Normal, new Gdk.Color(0x0e,0x1e,0x46));
		//show label_version on white
		label_version.ModifyFg(StateType.Normal, new Gdk.Color(0xff,0xff,0xff));

		//change colors of tests mode

		/*
		 * start of material design icons ---->
		 */

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_home.png");
		image_mode_main_menu.Pixbuf = pixbuf;
		//image_home.Pixbuf = pixbuf;
		//image_home1.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_settings.png");
		image_settings2.Pixbuf = pixbuf;
		image_session_preferences.Pixbuf = pixbuf;
		image_menu_preferences.Pixbuf = pixbuf;
		image_menu_preferences1.Pixbuf = pixbuf;

		//pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_minimize.png");
		//image_minimize.Pixbuf = pixbuf;
		//image_minimize1.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_quit.png");
		image_quit3.Pixbuf = pixbuf;
		image_quit_from_app1.Pixbuf = pixbuf;
		image_menu_quit.Pixbuf = pixbuf;
		image_menu_quit1.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_menu.png");
		image_radio_show_menu.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_modes.png");
		image_button_show_modes.Pixbuf = pixbuf;
		image_button_show_modes1.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_folders.png");
		image_menu_folders.Pixbuf = pixbuf;
		image_menu_folders1.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_more_horiz.png");
		image_session_more.Pixbuf = pixbuf;
		image_session_more1.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_help.png");
		image_menu_help.Pixbuf = pixbuf;
		image_menu_help1.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_book.png");
		image_menu_help_documents.Pixbuf = pixbuf;
		image_menu_help_documents1.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_keyboard.png");
		image_menu_help_accelerators.Pixbuf = pixbuf;
		image_menu_help_accelerators1.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_about.png");
		image_menu_help_about.Pixbuf = pixbuf;
		image_menu_help_about1.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_warning_red.png");
		image_message_permissions_at_boot.Pixbuf = pixbuf;
		image_button_force_sensor_stiffness_problem.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "muscle-concentric.png");
		image_top_eccon.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "muscle-concentric.png");
		image_encoder_eccon_concentric.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "muscle-excentric-concentric.png");
		image_encoder_eccon_eccentric_concentric.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "laterality-both.png");
		image_top_laterality.Pixbuf = pixbuf;
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

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "folder_check.png");
		image_force_sensor_open_folder.Pixbuf = pixbuf;
		image_race_encoder_open_folder.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "zoom.png");
		image_force_sensor_check_version.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "portrait_zoom.png");
		image_current_person_zoom.Pixbuf = pixbuf;
		image_current_person_zoom_h.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_no_photo.png");
		image_current_person.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_weight.png");
		image_mode_encoder_gravitatory.Pixbuf = pixbuf;
		image_menuitem_mode_power_gravitatory.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_inertia.png");
		image_mode_encoder_inertial.Pixbuf = pixbuf;
		image_menuitem_mode_power_inertial.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "force_sensor_icon.png");
		image_menuitem_mode_force_sensor.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "reaction_time_icon.png");
		image_menuitem_mode_reaction_time.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "other_icon.png");
		image_menuitem_mode_other.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_capture.png");
		image_tests_capture.Pixbuf = pixbuf;
		image_mode_encoder_capture.Pixbuf = pixbuf;
		image_contacts_close_and_capture.Pixbuf = pixbuf;
		image_encoder_close_and_capture.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_sprint.png");
		image_tests_sprint.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "arrow_back.png");
		image_selector_start_back.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_person_pin.png");
		image_radio_show_persons.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_person.png");
		image_person.Pixbuf = pixbuf;
		image_person1.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_info.png");
		image_info1.Pixbuf = pixbuf;
		image_info2.Pixbuf = pixbuf;
		image_info3.Pixbuf = pixbuf;
		image_info4.Pixbuf = pixbuf;
		image_run_simple_with_reaction_time_help.Pixbuf = pixbuf;
		image_run_interval_with_reaction_time_help.Pixbuf = pixbuf;
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
		image_chronopic_connect_encoder.Pixbuf = pixbuf;
		image_chronopic_connect_encoder1.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_recalculate.png");
		image_recalculate.Pixbuf = pixbuf;
		image_contacts_recalculate.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_build_24.png");
		image_encoder_configuration.Pixbuf = pixbuf;
		image_force_sensor_capture_adjust.Pixbuf = pixbuf;
		image_force_sensor_analyze_options.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_build_16.png");
		image_encoder_analyze_mode_options.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_jump_simple.png"); //but will change depending on mode
		image_contacts_exercise.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_weight.png"); //but will change depending on mode
		image_encoder_exercise.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "folder_new.png");
		image_session_new1.Pixbuf = pixbuf;
		image_session_new2.Pixbuf = pixbuf;
		image_session_new3.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "folder_open.png");
		image_session_open.Pixbuf = pixbuf;
		image_session_load1.Pixbuf = pixbuf;
		image_session_load2.Pixbuf = pixbuf;
		image_encoder_capture_open.Pixbuf = pixbuf;
		image_encoder_capture_open1.Pixbuf = pixbuf;
		image_contacts_capture_load.Pixbuf = pixbuf;
		image_force_sensor_analyze_load.Pixbuf = pixbuf;
		image_run_encoder_analyze_load.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "zero.png");
		image_force_sensor_tare.Pixbuf = pixbuf;
		//pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "run_2x.png");
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_capture_big.png");
		image_encoder_capture_execute.Pixbuf = pixbuf;
		image_button_execute.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_edit.png");
		image_encoder_exercise_edit.Pixbuf = pixbuf;
		image_force_sensor_exercise_edit.Pixbuf = pixbuf;
		image_run_encoder_exercise_edit.Pixbuf = pixbuf;
		image_edit_current_person.Pixbuf = pixbuf;
		image_edit_current_person_h.Pixbuf = pixbuf;
		image_session_edit.Pixbuf = pixbuf;
		image_session_edit2.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_close.png");
		image_contacts_exercise_close.Pixbuf = pixbuf;
		image_encoder_exercise_close.Pixbuf = pixbuf;
		image_force_sensor_analyze_options_close.Pixbuf = pixbuf;
		image_encoder_analyze_mode_options_close.Pixbuf = pixbuf;
		image_force_sensor_capture_adjust_close.Pixbuf = pixbuf;


		/*
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_up.png");
		image_persons_up.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_down.png");
		image_persons_down.Pixbuf = pixbuf;
		*/
		
		//persons buttons
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_person_add.png");
		image_persons_new_1.Pixbuf = pixbuf;
		//image_persons_new_2.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_group_add.png");
		image_persons_new_plus.Pixbuf = pixbuf;
		//image_persons_new_plus_2.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_person_outline.png");
		image_persons_open_1.Pixbuf = pixbuf;
		//dimage_persons_open_2.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_group_outline.png");
		image_persons_open_plus.Pixbuf = pixbuf;
		//image_persons_open_plus_2.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_rest.png");
		image_rest.Pixbuf = pixbuf;
		image_encoder_rhythm_rest.Pixbuf = pixbuf;
		image_contacts_rest_time_dark_blue.Pixbuf = pixbuf;
		image_encoder_rest_time_dark_blue.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_rest_yellow.png");
		image_encoder_rest_time_clear_yellow.Pixbuf = pixbuf;
		image_contacts_rest_time_clear_yellow.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "folder_check.png");
		image_all_persons_events.Pixbuf = pixbuf;
		image_all_persons_events_h.Pixbuf = pixbuf;

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

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "selector-jumps.png");
		image_selector_start_jumps.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "selector-runs.png");
		image_selector_start_runs.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "selector-encoder.png");
		image_selector_start_encoder.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "selector-force.png");
		image_selector_start_force_sensor.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "selector-rt.png");
		image_selector_start_rt.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "selector-multichronopic.png");
		image_selector_start_other.Pixbuf = pixbuf;

		/*
		 * gui for small screens
		 */

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_jump_simple.png");
		image_mode_jumps_small.Pixbuf = pixbuf;
		image_menuitem_mode_jumps_simple.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_jump_reactive.png");
		image_mode_jumps_reactive_small.Pixbuf = pixbuf;
		image_menuitem_mode_jumps_reactive.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_run_simple.png");
		image_mode_runs_small.Pixbuf = pixbuf;
		image_mode_runs_small1.Pixbuf = pixbuf;
		image_menuitem_mode_runs_simple.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_run_multiple.png");
		image_mode_runs_intervallic_small.Pixbuf = pixbuf;
		image_mode_runs_intervallic_small1.Pixbuf = pixbuf;
		image_menuitem_mode_runs_intervallic.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "race_encoder_icon.png");
		image_menuitem_mode_race_encoder.Pixbuf = pixbuf;
		image_mode_race_encoder_small.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "run_2x.png");
		image_run_execute_running.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "run_photocell.png");
		image_run_execute_photocell.Pixbuf = pixbuf;
		
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNamePulse);
		image_mode_pulses_small.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameMultiChronopic);
		image_mode_multi_chronopic_small.Pixbuf = pixbuf;
		
		label_start_selector_jumps.Text = "<b>" + label_start_selector_jumps.Text + "</b>";
		label_start_selector_races1.Text = "<b>" + label_start_selector_races1.Text + "</b>";
		label_start_selector_races.Text = "<b>" + label_start_selector_races.Text + "</b>";
		label_start_selector_encoder.Text = "<b>" + label_start_selector_encoder.Text + "</b>";
		label_start_selector_jumps.UseMarkup = true;
		label_start_selector_races1.UseMarkup = true;
		label_start_selector_races.UseMarkup = true;
		label_start_selector_encoder.UseMarkup = true;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameJumpsFallCalculate);
		extra_windows_jumps_image_dj_fall_calculate.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameJumpsFallPredefined);
		extra_windows_jumps_image_dj_fall_predefined.Pixbuf = pixbuf;
		image_tab_jumps_dj_optimal_fall.Pixbuf = pixbuf;

		image_tab_jumps_weight_fv_profile.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "jumps-fv.png");


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
		image_import_database.Pixbuf = pixbuf;
		image_session_import.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameExport);
		image_export_csv.Pixbuf = pixbuf;
		image_export_encoder_signal.Pixbuf = pixbuf;
		image_session_export.Pixbuf = pixbuf;

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
		//image_session_open.Pixbuf = pixbuf;
		//not changed because it's small. TODO: do bigger
		//image_encoder_capture_open.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_bell_none.png");
		image_contacts_bell.Pixbuf = pixbuf;
		image_encoder_bell.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "preferences-system.png");
		image_jump_reactive_repair.Pixbuf = pixbuf;
		image_run_interval_repair.Pixbuf = pixbuf;
		image_pulse_repair.Pixbuf = pixbuf;
		
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_delete.png");
		image_person_delete.Pixbuf = pixbuf;
		image_person_delete_h.Pixbuf = pixbuf;
		image_delete_last_test.Pixbuf = pixbuf;
		image_jump_delete.Pixbuf = pixbuf;
		image_jump_reactive_delete.Pixbuf = pixbuf;
		image_run_delete.Pixbuf = pixbuf;
		image_run_interval_delete.Pixbuf = pixbuf;
		image_reaction_time_delete.Pixbuf = pixbuf;
		image_pulse_delete.Pixbuf = pixbuf;
		image_multi_chronopic_delete.Pixbuf = pixbuf;
		image_jump_type_delete_simple.Pixbuf = pixbuf;
		image_jump_type_delete_reactive.Pixbuf = pixbuf;
		image_run_type_delete_simple.Pixbuf = pixbuf;
		image_run_type_delete_intervallic.Pixbuf = pixbuf;
		image_session_delete.Pixbuf = pixbuf;
		image_encoder_exercise_delete.Pixbuf = pixbuf;
		image_force_sensor_exercise_delete.Pixbuf = pixbuf;
		image_run_encoder_exercise_delete.Pixbuf = pixbuf;
		image_button_cancel.Pixbuf = pixbuf;
		image_encoder_capture_cancel.Pixbuf = pixbuf;
		image_encoder_signal_delete.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_cancel.png");
		image_encoder_analyze_cancel.Pixbuf = pixbuf;
		
		//zoom icons, done like this because there's one zoom icon created ad-hoc, 
		//and is not nice that the other are different for an user theme change
	
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameZoomFitIcon);
		image_jumps_zoom.Pixbuf = pixbuf;
		image_jumps_rj_zoom.Pixbuf = pixbuf;
		image_runs_zoom.Pixbuf = pixbuf;
		image_runs_interval_zoom.Pixbuf = pixbuf;
		image_reaction_times_zoom.Pixbuf = pixbuf;
		image_pulses_zoom.Pixbuf = pixbuf;
		image_multi_chronopic_zoom.Pixbuf = pixbuf;

		//video play icons
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "video_play.png");
		image_video_play_this_test_contacts.Pixbuf = pixbuf;
		image_video_play_this_test_encoder.Pixbuf = pixbuf;
		image_video_play_selected_jump.Pixbuf = pixbuf;
		image_video_play_selected_jump_rj.Pixbuf = pixbuf;
		image_video_play_selected_run.Pixbuf = pixbuf;
		image_video_play_selected_run_interval.Pixbuf = pixbuf;
		image_video_play_selected_pulse.Pixbuf = pixbuf;
		image_video_play_selected_reaction_time.Pixbuf = pixbuf;
		image_video_play_selected_multi_chronopic.Pixbuf = pixbuf;

		//white background in chronopic viewports
		UtilGtk.DeviceColors(viewport_chronopics, true);
		UtilGtk.DeviceColors(viewport_chronopic_encoder, true);

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_grid_on.png");
		image_contacts_session_overview.Pixbuf = pixbuf;
		image_encoder_session_overview.Pixbuf = pixbuf;

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
		image_encoder_analyze_stats.Pixbuf = pixbuf;
		image_encoder_analyze_image_save.Pixbuf = pixbuf;
		image_forcesensor_analyze_save_signal.Pixbuf = pixbuf;
		image_forcesensor_analyze_save_rfd_auto.Pixbuf = pixbuf;
		image_forcesensor_analyze_save_rfd_manual.Pixbuf = pixbuf;
		image_force_sensor_analyze_analyze.Pixbuf = pixbuf;
		image_force_sensor_analyze_options_close_and_analyze.Pixbuf = pixbuf;
		image_jumps_profile_save.Pixbuf = pixbuf;
		image_jumps_dj_optimal_fall_save.Pixbuf = pixbuf;
		image_jumps_weight_fv_profile_save.Pixbuf = pixbuf;
		image_jumps_evolution_save.Pixbuf = pixbuf;
		image_encoder_analyze_image_compujump_send_email_image.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "save.png");
		image_encoder_capture_curves_save.Pixbuf = pixbuf;
		image_encoder_analyze_table_save_1.Pixbuf = pixbuf;
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
		image_sprint_analyze_image_save.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "grid.png");
		image_encoder_analyze_table_save.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_email.png");
		image_encoder_analyze_image_compujump_send_email_send.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_analyze_general.png");
		image_tests_analyze_general.Pixbuf = pixbuf;
		image_tests_analyze_general1.Pixbuf = pixbuf;

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
		image_encoder_capture_finish.Pixbuf = pixbuf;

		//pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "gtk-cancel.png"); //high contrast (black)
		//image_button_cancel.Pixbuf = pixbuf;
		//image_encoder_capture_cancel.Pixbuf = pixbuf;
		// <--- end of execute tests


		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeIndividualCurrentSetIcon);
		image_encoder_analyze_individual_current_set.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeIndividualCurrentSessionIcon);
		image_encoder_analyze_individual_current_session.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeIndividualAllSessionsIcon);
		image_encoder_analyze_individual_all_sessions.Pixbuf = pixbuf;
		image_tab_jumps_evolution.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeGroupalCurrentSessionIcon);
		image_encoder_analyze_groupal_current_session.Pixbuf = pixbuf;

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
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeSpeedIcon);
		image_encoder_analyze_speed.Pixbuf = pixbuf;
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
	}
}
