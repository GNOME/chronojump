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
	[Widget] Gtk.Image image_home;
	[Widget] Gtk.Image image_settings;
	[Widget] Gtk.Image image_quit;
	[Widget] Gtk.Image image_session_new;
	[Widget] Gtk.Image image_session_load;
	[Widget] Gtk.Image image_mode_encoder_capture;
	[Widget] Gtk.Image image_manage_persons;
	[Widget] Gtk.Image image_person;
	[Widget] Gtk.Image image_edit_current_person;
	[Widget] Gtk.Image image_persons_up;
	[Widget] Gtk.Image image_persons_down;
	[Widget] Gtk.Image image_rest;
	[Widget] Gtk.Image image_all_persons_events;
	[Widget] Gtk.Image image_encoder_1RM_info;
	[Widget] Gtk.Image image_chronopic_connect;
	[Widget] Gtk.Image image_recalculate;
	[Widget] Gtk.Image image_encoder_configuration;
	[Widget] Gtk.Image image_encoder_exercise;
	[Widget] Gtk.Image image_encoder_exercise1;
	[Widget] Gtk.Image image_encoder_capture_open;
	[Widget] Gtk.Image image_encoder_capture_1set;
	[Widget] Gtk.Image image_encoder_capture_cont;
	[Widget] Gtk.Image image_encoder_capture_execute;
	[Widget] Gtk.Image image_encoder_exercise_add;
	[Widget] Gtk.Image image_encoder_exercise_edit;
	[Widget] Gtk.Image image_encoder_exercise_close;

	private void putNonStandardIcons()
	{
		Pixbuf pixbuf;

		viewport_chronojump_logo.ModifyBg(StateType.Normal, new Gdk.Color(0x0e,0x1e,0x46));
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameLogoTransparent);
		image_chronojump_logo.Pixbuf = pixbuf;

		//change colors of tests mode

		/*
		 * start of material design icons ---->
		 */

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_home.png");
		image_mode_main_menu.Pixbuf = pixbuf;
		image_home.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_settings.png");
		image_settings.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_quit.png");
		image_quit.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_gravity.png");
		image_mode_encoder_gravitatory.Pixbuf = pixbuf;
		image_gravitatory_not_menu.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_inertia.png");
		image_mode_encoder_inertial.Pixbuf = pixbuf;
		image_inertial_not_menu.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_capture.png");
		image_mode_encoder_capture.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_person_pin.png");
		image_manage_persons.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_person.png");
		image_person.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_info.png");
		image_encoder_1RM_info.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_chronopic_connect.png");
		image_chronopic_connect.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_recalculate.png");
		image_recalculate.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_build.png");
		image_encoder_configuration.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_exercise.png");
		image_encoder_exercise.Pixbuf = pixbuf;
		image_encoder_exercise1.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "folder_new_big.png");
		image_session_new.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "folder_open_big.png");
		image_session_load.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "folder_open.png");
		image_encoder_capture_open.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "one.png");
		image_encoder_capture_1set.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "cont.png");
		image_encoder_capture_cont.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "run.png"); //or use "timer"
		image_encoder_capture_execute.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_add.png");
		image_encoder_exercise_add.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_edit.png");
		image_encoder_exercise_edit.Pixbuf = pixbuf;
		image_edit_current_person.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_close.png");
		image_encoder_exercise_close.Pixbuf = pixbuf;
		
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_up.png");
		image_persons_up.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_down.png");
		image_persons_down.Pixbuf = pixbuf;
		
		//persons buttons
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_person_add.png");
		image_persons_new_1.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_group_add.png");
		image_persons_new_plus.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_person_outline.png");
		image_persons_open_1.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_group_outline.png");
		image_persons_open_plus.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_rest.png");
		image_rest.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_visibility.png");
		image_all_persons_events.Pixbuf = pixbuf;


		/*
		 * <------ end of material design icons
		 */

		/*
		 * gui for small screens
		 */
		viewport_selector_start_jumps.ModifyBg(StateType.Normal, new Gdk.Color(0x0b,0x48,0x6b));
		label_selector_start_jumps_simple.ModifyFg(StateType.Normal, new Gdk.Color(0xff,0xff,0xff));
		label_selector_start_jumps_reactive.ModifyFg(StateType.Normal, new Gdk.Color(0xff,0xff,0xff));
		
		viewport_selector_start_runs.ModifyBg(StateType.Normal, new Gdk.Color(0x3b,0x86,0x86));
		label_selector_start_runs_simple.ModifyFg(StateType.Normal, new Gdk.Color(0xff,0xff,0xff));
		label_selector_start_runs_intervallic.ModifyFg(StateType.Normal, new Gdk.Color(0xff,0xff,0xff));
		
		viewport_selector_start_encoder.ModifyBg(StateType.Normal, new Gdk.Color(0x79,0xbd,0x98));
		label_selector_start_encoder_gravitatory.ModifyFg(StateType.Normal, new Gdk.Color(0xff,0xff,0xff));
		label_selector_start_encoder_inertial.ModifyFg(StateType.Normal, new Gdk.Color(0xff,0xff,0xff));

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameJumps);
		image_mode_jumps_small.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameJumpsRJ);
		image_mode_jumps_reactive_small.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameRuns);
		image_mode_runs_small.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameRunsInterval);
		image_mode_runs_intervallic_small.Pixbuf = pixbuf;
		
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameReactionTime);
		image_mode_reaction_times_small.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNamePulse);
		image_mode_pulses_small.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameMultiChronopic);
		image_mode_multi_chronopic_small.Pixbuf = pixbuf;
		
		
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameJumpsFallCalculate);
		extra_windows_jumps_image_dj_fall_calculate.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameJumpsFallPredefined);
		extra_windows_jumps_image_dj_fall_predefined.Pixbuf = pixbuf;
		
		
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
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameExport);
		image_export_csv.Pixbuf = pixbuf;
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
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameOpen);
		image_session_open.Pixbuf = pixbuf;
		//not changed because it's small. TODO: do bigger
		//image_encoder_capture_open.Pixbuf = pixbuf;



		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_bell.png");
		image_jump_reactive_bell.Pixbuf = pixbuf;
		image_run_interval_bell.Pixbuf = pixbuf;
		image_encoder_bell.Pixbuf = pixbuf;
		
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "preferences-system.png");
		image_jump_reactive_repair.Pixbuf = pixbuf;
		image_run_interval_repair.Pixbuf = pixbuf;
		image_pulse_repair.Pixbuf = pixbuf;
		
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_delete.png");
		image_person_delete.Pixbuf = pixbuf;
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
		
		//encoder
		UtilGtk.ColorsRadio(viewport_chronopics, radiobutton_encoder_analyze_powerbars);
		UtilGtk.ColorsRadio(viewport_chronopics, radiobutton_encoder_analyze_cross);
		UtilGtk.ColorsRadio(viewport_chronopics, radiobutton_encoder_analyze_side);
		UtilGtk.ColorsRadio(viewport_chronopics, radiobutton_encoder_analyze_single);
		UtilGtk.ColorsRadio(viewport_chronopics, radiobutton_encoder_analyze_neuromuscular_profile);
		
		UtilGtk.ColorsCheckbox(viewport_chronopics, check_encoder_analyze_show_speed);
		UtilGtk.ColorsCheckbox(viewport_chronopics, check_encoder_analyze_show_accel);
		UtilGtk.ColorsCheckbox(viewport_chronopics, check_encoder_analyze_show_force);
		UtilGtk.ColorsCheckbox(viewport_chronopics, check_encoder_analyze_show_power);
		
		UtilGtk.ColorsCheckbox(viewport_chronopics, check_encoder_analyze_show_range);
		UtilGtk.ColorsCheckbox(viewport_chronopics, check_encoder_analyze_show_time_to_peak_power);


		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "gpm-statistics.png");
		image_encoder_analyze_stats.Pixbuf = pixbuf;
		image_encoder_analyze_image_save.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_delete.png");
		image_encoder_signal_delete.Pixbuf = pixbuf;
	
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "spreadsheet.png");
		image_encoder_analyze_table_save.Pixbuf = pixbuf;
		
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "jumps-profile-pie.png");
		image_tab_jumps_profile.Pixbuf = pixbuf;
		
		// execute tests --->
		/*
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "gtk-apply.png"); //trying high contrast (black)
		image_button_execute.Pixbuf = pixbuf;
		image_encoder_capture_execute.Pixbuf = pixbuf;
		*/

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "finish.png"); //gnome (white)
		image_button_finish.Pixbuf = pixbuf;
		image_encoder_capture_finish.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "gtk-cancel.png"); //high contrast (black)
		image_button_cancel.Pixbuf = pixbuf;
		image_encoder_capture_cancel.Pixbuf = pixbuf;
		// <--- end of execute tests


		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeIndividualCurrentSetIcon);
		image_encoder_analyze_individual_current_set.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeIndividualCurrentSessionIcon);
		image_encoder_analyze_individual_current_session.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeIndividualAllSessionsIcon);
		image_encoder_analyze_individual_all_sessions.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeGroupalCurrentSessionIcon);
		image_encoder_analyze_groupal_current_session.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzePowerbarsIcon);
		image_encoder_analyze_powerbars.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeCrossIcon);
		image_encoder_analyze_cross.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyze1RMIcon);
		image_encoder_analyze_1RM.Pixbuf = pixbuf;
		image_encoder_analyze_1RM_save.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeSideIcon);
		image_encoder_analyze_side.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeSingleIcon);
		image_encoder_analyze_single.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeNmpIcon);
		image_encoder_analyze_nmp.Pixbuf = pixbuf;
		
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeEcconTogetherIcon);
		image_encoder_analyze_eccon_together.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeEcconSeparatedIcon);
		image_encoder_analyze_eccon_separated.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeSpeedIcon);
		image_encoder_analyze_speed.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeAccelIcon);
		image_encoder_analyze_accel.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeForceIcon);
		image_encoder_analyze_force.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzePowerIcon);
		image_encoder_analyze_power.Pixbuf = pixbuf;
		
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeMeanIcon);
		image_encoder_analyze_mean.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeMaxIcon);
		image_encoder_analyze_max.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeRangeIcon);
		image_encoder_analyze_range.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderAnalyzeTimeToPPIcon);
		image_encoder_analyze_time_to_pp.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderInertialInstructions);
		image_encoder_inertial_instructions.Pixbuf = pixbuf;
		
		//auto mode
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameAutoPersonSkipIcon);
		image_auto_person_skip.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameAutoPersonRemoveIcon);
		image_auto_person_remove.Pixbuf = pixbuf;
				
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameSelectorJumps);
		image_selector_start_jumps.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameSelectorRuns);
		image_selector_start_runs.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameSelectorEncoderGravitatory);
		image_selector_start_encoder_gravitatory.Pixbuf = pixbuf;
		//pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameSelectorEncoderInertial);
		//image_selector_start_encoder_inertial.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameLogo320); //changed to 270 for the presentation
		image_presentation_logo.Pixbuf = pixbuf;

	}
}
