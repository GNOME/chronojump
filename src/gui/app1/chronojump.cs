//La camera 1 va mes rapid que la 0, provar de canviar i activatr primer la 1 a veure que tal

//- Arreglar problema de no coincidencia entre imatge mini i imatge gran, per exemple session6, atleta 1
//- modo simulado curses 4 curses no acaba la ultima
//TODO: que es pugui seleccionar si es vol una webcam o 2


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
 * Copyright (C) 2004-2021   Xavier de Blas <xaviblas@gmail.com>
 */


using System;
using Gtk;
using Gdk;
using Glade;
using System.IO.Ports;
using Mono.Unix;
using System.IO; //"File" things
using System.Collections; //ArrayList
using System.Collections.Generic; //List
using System.Threading;
using System.Diagnostics;

public partial class ChronoJumpWindow 
{
	[Widget] Gtk.Window app1;

	[Widget] Gtk.HBox hbox_gui_tests;
	[Widget] Gtk.SpinButton spin_gui_tests;
	[Widget] Gtk.ComboBox combo_gui_tests;
	[Widget] Gtk.Button button_carles;
	
	[Widget] Gtk.Notebook notebook_chronojump_logo;
	[Widget] Gtk.Viewport viewport_chronojump_logo;
	[Widget] Gtk.Image image_chronojump_logo;
	[Widget] Gtk.DrawingArea drawingarea_chronojump_logo;

	[Widget] Gtk.Notebook notebook_start; 		//start window or program
	[Widget] Gtk.Notebook notebook_mode_selector; 	//use to display the mode images to select different modes
	[Widget] Gtk.Notebook notebook_mode_selector2; //for selection of jumps, runs, runs photocell, encoder
	[Widget] Gtk.Table table_start_selector_rt_other;
	[Widget] Gtk.Button button_start_selector_show_more;
	[Widget] Gtk.Button button_start_selector_show_less;
	[Widget] Gtk.Notebook notebook_sup;
	[Widget] Gtk.HBox hbox_other;
	[Widget] Gtk.HBox hbox_contacts_capture_top;
	[Widget] Gtk.Notebook notebook_capture_analyze; //not encoder
	[Widget] Gtk.Notebook notebook_contacts_execute_or;
	[Widget] Gtk.Notebook notebook_analyze; //not encoder
	[Widget] Gtk.VBox vbox_contacts_capture_graph;
	[Widget] Gtk.HBox hbox_message_permissions_at_boot;
	[Widget] Gtk.Label label_message_permissions_at_boot;
	[Widget] Gtk.HBox hbox_message_camera_at_boot;
	[Widget] Gtk.Notebook notebook_import;

	[Widget] Gtk.Button button_show_modes_contacts;
	[Widget] Gtk.Button button_show_modes_encoder;
	[Widget] Gtk.HBox hbox_change_modes_contacts;
	[Widget] Gtk.HBox hbox_change_modes_encoder;
	[Widget] Gtk.HBox hbox_change_modes_jumps;
	[Widget] Gtk.HBox hbox_change_modes_runs;
	[Widget] Gtk.RadioButton radio_change_modes_contacts_jumps_simple;
	[Widget] Gtk.RadioButton radio_change_modes_contacts_jumps_reactive;
	[Widget] Gtk.RadioButton radio_change_modes_contacts_runs_simple;
	[Widget] Gtk.RadioButton radio_change_modes_contacts_runs_intervallic;
	[Widget] Gtk.RadioButton radio_change_modes_contacts_runs_encoder;
	[Widget] Gtk.RadioButton radio_change_modes_contacts_force_sensor;
	[Widget] Gtk.RadioButton radio_change_modes_encoder_gravitatory;
	[Widget] Gtk.RadioButton radio_change_modes_encoder_inertial;
	[Widget] Gtk.Image image_change_modes_contacts_jumps_simple;
	[Widget] Gtk.Image image_change_modes_contacts_jumps_reactive;
	[Widget] Gtk.Image image_change_modes_contacts_runs_simple;
	[Widget] Gtk.Image image_change_modes_contacts_runs_reactive;
	[Widget] Gtk.Image image_change_modes_contacts_runs_intervallic;
	[Widget] Gtk.Image image_change_modes_contacts_force_sensor;
	[Widget] Gtk.Image image_change_modes_contacts_runs_encoder;
	[Widget] Gtk.Image image_change_modes_encoder_gravitatory;
	[Widget] Gtk.Image image_change_modes_encoder_inertial;

	[Widget] Gtk.Alignment alignment_contacts_show_graph_table;
	[Widget] Gtk.HBox hbox_contacts_capture_show_need_one;
	[Widget] Gtk.CheckButton check_contacts_capture_graph;
	[Widget] Gtk.CheckButton check_contacts_capture_table;

	[Widget] Gtk.EventBox eventbox_button_show_modes_contacts;
	[Widget] Gtk.EventBox eventbox_change_modes_contacts_jumps_simple;
	[Widget] Gtk.EventBox eventbox_change_modes_contacts_jumps_reactive;
	[Widget] Gtk.EventBox eventbox_change_modes_contacts_runs_simple;
	[Widget] Gtk.EventBox eventbox_change_modes_contacts_runs_intervallic;
	[Widget] Gtk.EventBox eventbox_change_modes_contacts_runs_encoder;
	[Widget] Gtk.EventBox eventbox_change_modes_contacts_force_sensor;
	[Widget] Gtk.EventBox eventbox_change_modes_encoder_gravitatory;
	[Widget] Gtk.EventBox eventbox_change_modes_encoder_inertial;
	[Widget] Gtk.EventBox eventbox_button_show_modes_encoder;
	[Widget] Gtk.EventBox eventbox_radio_mode_contacts_capture;
	[Widget] Gtk.EventBox eventbox_radio_mode_contacts_analyze;
	[Widget] Gtk.EventBox eventbox_radio_mode_encoder_capture_small;
	[Widget] Gtk.EventBox eventbox_radio_mode_encoder_analyze_small;
	[Widget] Gtk.EventBox eventbox_radio_mode_pulses_small;
	[Widget] Gtk.EventBox eventbox_radio_mode_multi_chronopic_small;
	[Widget] Gtk.EventBox eventbox_button_open_chronojump;
	[Widget] Gtk.EventBox eventbox_button_help_close;
	[Widget] Gtk.EventBox eventbox_button_news_close;
	[Widget] Gtk.EventBox eventbox_button_exit_cancel;
	[Widget] Gtk.EventBox eventbox_button_exit_confirm;

	[Widget] Gtk.HBox hbox_contacts_sup_capture_analyze_two_buttons;
	[Widget] Gtk.HBox hbox_radio_mode_contacts_analyze_buttons;
	[Widget] Gtk.HBox hbox_radio_mode_contacts_analyze_jump_simple_buttons;

	[Widget] Gtk.Image image_button_show_modes_contacts_grid;
	[Widget] Gtk.Image image_button_show_modes_encoder_grid;

	//radio group
	[Widget] Gtk.RadioButton radio_mode_contacts_capture;
	[Widget] Gtk.RadioButton radio_mode_contacts_analyze;

	//radio group
	[Widget] Gtk.RadioButton radio_mode_contacts_jumps_profile;
	[Widget] Gtk.RadioButton radio_mode_contacts_jumps_dj_optimal_fall;
	[Widget] Gtk.RadioButton radio_mode_contacts_jumps_weight_fv_profile;
	[Widget] Gtk.RadioButton radio_mode_contacts_jumps_evolution;
	[Widget] Gtk.RadioButton radio_mode_contacts_jumps_rj_fatigue;
	[Widget] Gtk.RadioButton radio_mode_contacts_runs_evolution;
	[Widget] Gtk.RadioButton radio_mode_contacts_sprint;
	[Widget] Gtk.RadioButton radio_mode_contacts_advanced;

	[Widget] Gtk.Label label_sprint_person_name;

	[Widget] Gtk.Label label_version;
	[Widget] Gtk.Label label_version_hidden; //just to have logo aligned on the middle
	//[Widget] Gtk.Image image_selector_start_encoder_inertial;

	[Widget] Gtk.RadioButton radio_mode_pulses_small;
	[Widget] Gtk.RadioButton radio_mode_multi_chronopic_small;

	[Widget] Gtk.RadioButton radio_mode_encoder_capture_small;
	[Widget] Gtk.RadioButton radio_mode_encoder_analyze_small;

	[Widget] Gtk.Image image_persons_new_1;
	[Widget] Gtk.Image image_persons_new_plus;
	[Widget] Gtk.Image image_persons_open_1;
	[Widget] Gtk.Image image_persons_open_plus;

	[Widget] Gtk.Image image_export_encoder_signal;

	//contact tests execute buttons
	[Widget] Gtk.Image image_button_finish;
	[Widget] Gtk.Image image_button_cancel; //needed this specially because theme cancel sometimes seems "record"
	//encoder tests execute buttons
	//[Widget] Gtk.Image image_encoder_capture_execute;
	[Widget] Gtk.Image image_encoder_capture_finish;
	[Widget] Gtk.Image image_encoder_capture_cancel;

	[Widget] Gtk.Frame frame_contacts_graph_table;
	[Widget] Gtk.HPaned hpaned_contacts_graph_table;
	[Widget] Gtk.TreeView treeview_persons;
	[Widget] Gtk.TreeView treeview_jumps;
	[Widget] Gtk.TreeView treeview_jumps_rj;
	[Widget] Gtk.TreeView treeview_runs;
	[Widget] Gtk.TreeView treeview_runs_interval;
	[Widget] Gtk.TreeView treeview_runs_interval_sprint;
	[Widget] Gtk.TreeView treeview_reaction_times;
	[Widget] Gtk.TreeView treeview_pulses;
	[Widget] Gtk.TreeView treeview_multi_chronopic;
	
	[Widget] Gtk.HBox hbox_combo_select_jumps;
	[Widget] Gtk.HBox hbox_combo_select_jumps_rj;
	[Widget] Gtk.HBox hbox_combo_select_runs;
	[Widget] Gtk.HBox hbox_combo_select_runs_interval;
	[Widget] Gtk.HBox hbox_combo_select_contacts_top_with_arrows;
	[Widget] Gtk.HBox hbox_combo_select_contacts_top;

	//auto mode	
	//[Widget] Gtk.Box hbox_jump_types_options;
	[Widget] Gtk.Box hbox_jump_auto_controls;
	[Widget] Gtk.Image image_auto_person_skip;
	[Widget] Gtk.Image image_auto_person_remove;
	[Widget] Gtk.Button button_auto_start;
	[Widget] Gtk.Label label_jump_auto_current_person;
	[Widget] Gtk.Label label_jump_auto_current_test;
		
	[Widget] Gtk.Image image_line_session_avg;
	[Widget] Gtk.Image image_line_session_max;
	[Widget] Gtk.Image image_line_person_avg;
	[Widget] Gtk.Image image_line_person_max;
	[Widget] Gtk.Image image_line_person_max_all_sessions;

//	[Widget] Gtk.Box hbox_combo_pulses;
	[Widget] Gtk.VBox vbox_jumps;
	//[Widget] Gtk.Box hbox_jumps_test;
	[Widget] Gtk.Box hbox_jumps_rj;
	[Widget] Gtk.VBox vbox_runs;
	[Widget] Gtk.HBox hbox_runs_interval_all; //normal and compujump
	[Widget] Gtk.VBox vbox_runs_interval;
	[Widget] Gtk.VBox vbox_runs_interval_compujump;
	[Widget] Gtk.Box hbox_other_mc;
	[Widget] Gtk.Box hbox_other_pulses;
	
	[Widget] Gtk.ComboBox combo_select_jumps;
	[Widget] Gtk.ComboBox combo_select_jumps_rj;
	[Widget] Gtk.ComboBox combo_select_runs;
	[Widget] Gtk.ComboBox combo_select_runs_interval;
	[Widget] Gtk.ComboBox combo_select_contacts_top;

	//new since 1.6.3. Using gui/cjCombo.cs
	CjComboSelectJumps comboSelectJumps;
	CjComboSelectJumpsRj comboSelectJumpsRj;
	CjComboSelectRuns comboSelectRuns;
	CjComboSelectRunsI comboSelectRunsI;
	CjCombo comboSelectContactsTop;

//	[Widget] Gtk.ComboBox combo_pulses;

	//menu person
	[Widget] Gtk.Button button_persons_up;
	[Widget] Gtk.Button button_persons_down;

	//tests
	[Widget] Gtk.Notebook notebook_contacts_capture_doing_wait;
	[Widget] Gtk.Button button_contacts_bells;
	[Widget] Gtk.Button button_contacts_capture_session_overview;
	[Widget] Gtk.Button button_contacts_capture_load;
	[Widget] Gtk.Button button_contacts_exercise_close_and_recalculate;
	[Widget] Gtk.VBox vbox_contacts_signal_comment;
	[Widget] Gtk.TextView textview_contacts_signal_comment;
	[Widget] Gtk.Button button_contacts_signal_save_comment;
	[Widget] Gtk.Frame frame_jumps_automatic;
	[Widget] Gtk.Notebook notebook_jumps_automatic;
	[Widget] Gtk.HBox hbox_contacts_device_adjust_threshold;

	//jumps
	[Widget] Gtk.Button button_edit_selected_jump;
	[Widget] Gtk.Button button_video_play_selected_jump;
	[Widget] Gtk.Button button_delete_selected_jump;
	[Widget] Gtk.Button button_edit_selected_jump_rj;
	[Widget] Gtk.Button button_video_play_selected_jump_rj;
	[Widget] Gtk.Button button_delete_selected_jump_rj;
	[Widget] Gtk.Button button_repair_selected_jump_rj;

	[Widget] Gtk.Image extra_windows_jumps_image_dj_fall_calculate;
	[Widget] Gtk.Image extra_windows_jumps_image_dj_fall_predefined;
	[Widget] Gtk.HBox hbox_extra_window_jumps_fall_height;

	//runs
	[Widget] Gtk.Button button_edit_selected_run;
	[Widget] Gtk.Button button_video_play_selected_run;
	[Widget] Gtk.Button button_delete_selected_run;
	[Widget] Gtk.Button button_edit_selected_run_interval;
	[Widget] Gtk.Button button_video_play_selected_run_interval;
	[Widget] Gtk.Button button_delete_selected_run_interval;
	[Widget] Gtk.Button button_repair_selected_run_interval;


	//other
	//reaction time
	[Widget] Gtk.Button button_edit_selected_reaction_time;
	[Widget] Gtk.Button button_video_play_selected_reaction_time;
	[Widget] Gtk.Button button_delete_selected_reaction_time;
	[Widget] Gtk.SpinButton spinbutton_animation_lights_speed;
	[Widget] Gtk.SpinButton spinbutton_flicker_lights_speed;
	[Widget] Gtk.CheckButton check_reaction_time_disc_red;
	[Widget] Gtk.CheckButton check_reaction_time_disc_yellow;
	[Widget] Gtk.CheckButton check_reaction_time_disc_green;
	[Widget] Gtk.CheckButton check_reaction_time_disc_buzzer;
	[Widget] Gtk.SpinButton spinbutton_discriminative_lights_minimum;
	[Widget] Gtk.SpinButton spinbutton_discriminative_lights_maximum;

	//pulse
	[Widget] Gtk.Button button_edit_selected_pulse;
	[Widget] Gtk.Button button_video_play_selected_pulse;
	[Widget] Gtk.Button button_delete_selected_pulse;
	[Widget] Gtk.Button button_repair_selected_pulse;

	[Widget] Gtk.Box vbox_execute_test;
	[Widget] Gtk.Button button_execute_test;
	[Widget] Gtk.Viewport viewport_chronopics;
	[Widget] Gtk.Viewport viewport_chronopic_encoder;

	[Widget] Gtk.Label label_threshold;

	//force sensor
	[Widget] Gtk.HBox hbox_capture_phases_time;
	[Widget] Gtk.VBox vbox_contacts_load_recalculate;

	//multiChronopic	
	[Widget] Gtk.Button button_edit_selected_multi_chronopic;
	[Widget] Gtk.Button button_video_play_selected_multi_chronopic;
	[Widget] Gtk.Button button_delete_selected_multi_chronopic;
//	[Widget] Gtk.Box hbox_multi_chronopic_buttons;
//	[Widget] Gtk.Button button_multi_chronopic_start;
//	[Widget] Gtk.Button button_run_analysis;
//	[Widget] Gtk.Entry extra_window_spin_run_analysis_distance;
//	[Widget] Gtk.CheckButton extra_window_check_multichronopic_sync;
//	[Widget] Gtk.CheckButton extra_window_check_multichronopic_delete_first;
//	[Widget] Gtk.Entry entry_multi_chronopic_cp2;

	//widgets for enable or disable
	[Widget] Gtk.Frame frame_persons;
	[Widget] Gtk.Frame frame_persons_top;
	[Widget] Gtk.VBox vbox_persons_bottom;
	[Widget] Gtk.HBox hbox_persons_bottom_photo;
	[Widget] Gtk.VBox vbox_persons_bottom_no_photo;
	[Widget] Gtk.HBox hbox_persons_bottom_no_photo;
	[Widget] Gtk.Button button_recuperate_person;
	[Widget] Gtk.Button button_recuperate_persons_from_session;
	[Widget] Gtk.Button button_person_add_single;
	[Widget] Gtk.Button button_person_add_multiple;

	[Widget] Gtk.Button button_contacts_exercise_close_and_capture;
	[Widget] Gtk.Notebook notebook_execute;
	[Widget] Gtk.Notebook notebook_results;
	[Widget] Gtk.Notebook notebook_options_top;
		
	[Widget] Gtk.EventBox eventbox_image_test;
	[Widget] Gtk.Image image_test;
	[Widget] Gtk.Button button_image_test_zoom;
	[Widget] Gtk.Image image_test_zoom;
	[Widget] Gtk.Button button_delete_last_test;
	[Widget] Gtk.Button button_inspect_last_test_run_simple;
	[Widget] Gtk.Button button_inspect_last_test_run_intervallic;
	//[Widget] Gtk.VBox vbox_last_test_buttons;

	[Widget] Gtk.HBox hbox_chronopics_and_more;
	[Widget] Gtk.Button button_activate_chronopics;
	[Widget] Gtk.Alignment alignment_button_threshold;
	[Widget] Gtk.Alignment alignment_button_force_sensor_adjust;
	[Widget] Gtk.Button button_force_sensor_sync;

	//non standard icons	
	[Widget] Gtk.Image image_jump_reactive_bell;
	[Widget] Gtk.Image image_run_interval_bell;
	[Widget] Gtk.Image image_jump_reactive_repair;
	[Widget] Gtk.Image image_run_interval_repair;
	[Widget] Gtk.Image image_multi_chronopic_repair;
	[Widget] Gtk.Image image_pulse_repair;
	[Widget] Gtk.Image image_delete_last_test;
	[Widget] Gtk.Image image_jump_delete;
	[Widget] Gtk.Image image_jump_reactive_delete;
	[Widget] Gtk.Image image_run_delete;
	[Widget] Gtk.Image image_run_interval_delete;
	[Widget] Gtk.Image image_reaction_time_delete;
	[Widget] Gtk.Image image_pulse_delete;
	[Widget] Gtk.Image image_multi_chronopic_delete;
	[Widget] Gtk.Image image_jump_type_delete_simple;
	[Widget] Gtk.Image image_jump_type_delete_reactive;
	[Widget] Gtk.Image image_run_type_delete_simple;
	[Widget] Gtk.Image image_run_type_delete_intervallic;

	[Widget] Gtk.Image image_jumps_zoom;
	[Widget] Gtk.Image image_jumps_rj_zoom;
	[Widget] Gtk.Image image_runs_zoom;
	[Widget] Gtk.Image image_runs_interval_zoom;
	[Widget] Gtk.Image image_reaction_times_zoom;
	[Widget] Gtk.Image image_pulses_zoom;
	[Widget] Gtk.Image image_multi_chronopic_zoom;
	
	//encoder
	//[Widget] Gtk.Image image_encoder_analyze_zoom;
	[Widget] Gtk.Image image_encoder_analyze_stats;
	[Widget] Gtk.Image image_encoder_analyze_image_save;
	[Widget] Gtk.Image image_encoder_analyze_1RM_save;
	[Widget] Gtk.Image image_encoder_analyze_table_save;
	[Widget] Gtk.Image image_encoder_signal_delete;
	[Widget] Gtk.Image image_encoder_inertial_instructions;
	[Widget] Gtk.Label label_gravitatory_vpf_propulsive;

	//forcesensor
	[Widget] Gtk.Image image_forcesensor_analyze_save_signal;
	[Widget] Gtk.Image image_forcesensor_analyze_save_rfd_auto;
	[Widget] Gtk.Image image_forcesensor_analyze_save_rfd_manual;

	[Widget] Gtk.VBox vbox_help;

	Random rand;

	//persons
	private TreeViewPersons myTreeViewPersons;
	//normal jumps
	private TreeViewJumps myTreeViewJumps;
	//rj jumps
	private TreeViewJumpsRj myTreeViewJumpsRj;
	//normal runs
	private TreeViewRuns myTreeViewRuns;
	//runs interval
	private TreeViewRunsInterval myTreeViewRunsInterval;
	//reaction times
	private TreeViewReactionTimes myTreeViewReactionTimes;
	//pulses
	private TreeViewPulses myTreeViewPulses;
	//multiChronopic
	private TreeViewMultiChronopic myTreeViewMultiChronopic;
	
	private Preferences preferences;
	private List<ForceSensorRFD> rfdList;
	private ForceSensorImpulse impulse;

	private static Person currentPerson;
	private static Session currentSession;
	private static PersonSession currentPersonSession;
	private static bool definedSession;
	private static Jump currentJump;
	private static JumpRj currentJumpRj;
	private static Run currentRun;
	private static RunInterval currentRunInterval;
	private static ReactionTime currentReactionTime;
	private static Pulse currentPulse;
	private static MultiChronopic currentMultiChronopic;
	
	private static EventExecute currentEventExecute;

	//Used by Cancel and Finish
	private static EventType currentEventType;

	private static JumpType currentJumpType;
	private static JumpType currentJumpRjType;
	bool thisJumpIsSimple;	//needed on updating
	bool lastJumpIsSimple;	//needed on update
	private static RunType currentRunType;
	private static RunType currentRunIntervalType;
	bool thisRunIsSimple;	//needed on updating
	bool lastRunIsSimple;	//needed on update
	private static PulseType currentPulseType;
	private static ReactionTimeType currentReactionTimeType;
	private static MultiChronopicType currentMultiChronopicType;
	private static Report report;
	private static List<News> newsAtDB_l; //to not read/write SQL on pingThread and at the same time outside of thread
	private static List<News> newsAtServer_l; //to not read/write SQL on pingThread and at the same time outside of thread

	private static bool followSignals = true;

	//windows needed
	ChronopicRegisterWindow chronopicRegisterWin;
	PreferencesWindow preferencesWin;
	PersonRecuperateWindow personRecuperateWin; 
	PersonsRecuperateFromOtherSessionWindow personsRecuperateFromOtherSessionWin; 
	PersonAddModifyWindow personAddModifyWin; 
	PersonAddMultipleWindow personAddMultipleWin;
	PersonShowAllEventsWindow personShowAllEventsWin;
	PersonSelectWindow personSelectWin;
	JumpsMoreWindow jumpsMoreWin;
	JumpsRjMoreWindow jumpsRjMoreWin;
	EditJumpWindow editJumpWin;
	EditJumpRjWindow editJumpRjWin;
	RepairJumpRjWindow repairJumpRjWin;
	JumpTypeAddWindow jumpTypeAddWin;
	
	RunsMoreWindow runsMoreWin;
	RunsIntervalMoreWindow runsIntervalMoreWin;
	RunTypeAddWindow runTypeAddWin;
	EditRunWindow editRunWin;
	RepairRunIntervalWindow repairRunIntervalWin;
	EditRunIntervalWindow editRunIntervalWin;

	EditReactionTimeWindow editReactionTimeWin;

	EditPulseWindow editPulseWin;
	RepairPulseWindow repairPulseWin;
	
	EditMultiChronopicWindow editMultiChronopicWin;
	
	ConfirmWindowJumpRun confirmWinJumpRun;	//for deleting jumps and RJ jumps (and runs)
	ReportWindow reportWin;
	RepetitiveConditionsWindow repetitiveConditionsWin;
	GenericWindow genericWin;
		
	ExecuteAutoWindow executeAutoWin;

	static Thread pingThread;

	private bool createdStatsWin;
	
	private UtilAll.OperatingSystems operatingSystem;
	private string progVersion;
	private string progName;
	private enum notebook_start_pages { PROGRAM, SENDLOG, EXITCONFIRM, SOCIALNETWORKPOLL }
	private enum notebook_sup_pages { START, CONTACTS, ENCODER, SESSION, NETWORKSPROBLEMS, HELP, NEWS }
	private enum notebook_contacts_execute_or_pages { EXECUTE, INSTRUCTIONS, FORCESENSORADJUST, RACEINSPECTOR }
	private enum notebook_analyze_pages { STATISTICS, JUMPSPROFILE, JUMPSDJOPTIMALFALL, JUMPSWEIGHTFVPROFILE, JUMPSEVOLUTION, JUMPSRJFATIGUE,
		RUNSEVOLUTION, SPRINT, FORCESENSOR, RACEENCODER }

	private string runningFileName; //useful for knowing if there are two chronojump instances

	//int chronopicCancelledTimes = 0;

	ChronopicRegister chronopicRegister;
	Chronopic2016 cp2016;
	private PhotocellWirelessCapture photocellWirelessCapture;
	private Threshold threshold;

	RestTime restTime;
	//to control method that is updating restTimes on treeview_persons and personsOnTop
	bool updatingRestTimes = false;

	//only called the first time the software runs
	//and only on windows
	private void on_language_clicked(object o, EventArgs args) {
		//languageChange();
		//createMainWindow("");
	}


	bool app1Shown = false;
	bool needToShowChronopicRegisterWindow;

	public ChronoJumpWindow(string progVersion, string progName, string runningFileName, SplashWindow splashWin,
			bool showSendLog, string sendLogMessage, string topMessage, bool showCameraStop)
	{
		this.progVersion = progVersion;
		this.progName = progName;
		this.runningFileName = runningFileName;

		//record GetOsEnum on variables to not call it all the time
		operatingSystem = UtilAll.GetOSEnum();

		Glade.XML gxml;
		gxml = Glade.XML.FromAssembly (Util.GetGladePath() + "app1.glade", "app1", "chronojump");
		gxml.Autoconnect(this);

		//put an icon to window
		UtilGtk.IconWindow(app1);

		string buildVersion = UtilAll.ReadVersionFromBuildInfo();
		LogB.Information("Build version:" + buildVersion);

		//if buildVersion has eg. 1.9.0-1980-gc4b2941f remove the git hash (only show 1.9.0-1980)
		string [] buildVersionSplit = buildVersion.Split(new char[] {'-'});
		if(buildVersionSplit.Length == 3)
			buildVersion = buildVersionSplit[0] + "-" + buildVersionSplit[1];

		label_version.Text = buildVersion;
		label_version_hidden.Text = buildVersion;

		//manage app1 will not be hiding other windows at start
		app1Shown = false;
		needToShowChronopicRegisterWindow = false;

		//show chronojump logo on down-left area
		changeTestImage("", "", "LOGO");
	
		//white bg
		eventbox_image_test.ModifyBg(StateType.Normal, UtilGtk.WHITE);
	
		//new DialogMessage(Constants.MessageTypes.INFO, UtilGtk.ScreenHeightFitted(false).ToString() );
		//UtilGtk.ResizeIfNeeded(stats_window);

		report = new Report(-1); //when a session is loaded or created, it will change the report.SessionID value
		//TODO: check what happens if a session it's deleted
		//i think report it's deactivated until a new session is created or loaded, 
		//but check what happens if report window is opened

		//put videoOn as false before loading preferences to start always without the camera
		//this is good if camera produces crash
		SqlitePreferences.Update("videoOn", "False", false);

		// ------ Loading preferences ------

		if(splashWin != null)
			splashWin.UpdateLabel(Catalog.GetString(Constants.SplashMessages[7]));
		
		//preferencesLoaded is a fix to a gtk#-net-windows-bug where radiobuttons raise signals
		//at initialization of chronojump and gives problems if this signals are raised while preferences are loading
		loadPreferencesAtStart ();

		Config.UseSystemColor = preferences.colorBackgroundOsColor;
		Config.ColorBackground = preferences.colorBackground;
		Config.ColorBackgroundIsDark = UtilGtk.ColorIsDark(preferences.colorBackground);

		checkbutton_video_contacts.Visible = true;

		if(topMessage != "") {
			label_message_permissions_at_boot.Text = topMessage;
			hbox_message_permissions_at_boot.Visible = true;
		}

		bool showSocialNetworkPoll = (preferences.socialNetworkDatetime == "");
		//show send log if needed or other messages
		if (showSendLog)
		{
			show_send_log(sendLogMessage, preferences.crashLogLanguage);
			notebook_start.CurrentPage = Convert.ToInt32(notebook_start_pages.SENDLOG);
		}
		else if (showSocialNetworkPoll)
		{
			notebook_start.CurrentPage = Convert.ToInt32(notebook_start_pages.SOCIALNETWORKPOLL);
			socialNetworkPollInit();
		}
		else
			notebook_sup.CurrentPage = Convert.ToInt32(notebook_sup_pages.START);

		if(showCameraStop)
			hbox_message_camera_at_boot.Visible = true;

		
		// ------ Creating widgets ------

		if(splashWin != null)
			splashWin.UpdateLabel(Catalog.GetString(Constants.SplashMessages[8]));

		createTreeView_persons (treeview_persons);

		createTreeView_jumps (treeview_jumps);
		createTreeView_jumps_rj (treeview_jumps_rj);
		createTreeView_runs (treeview_runs);
		createTreeView_runs_interval (treeview_runs_interval);
		createTreeView_runs_interval_sprint (treeview_runs_interval_sprint);
		createTreeView_reaction_times (treeview_reaction_times);
		createTreeView_pulses (treeview_pulses);
		createTreeView_multi_chronopic (false, treeview_multi_chronopic);
		
		rfdList = SqliteForceSensorRFD.SelectAll(false);
		impulse = SqliteForceSensorRFD.SelectImpulse(false);
		initForceSensor();
		initRunEncoder();

		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_show_modes_contacts, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_change_modes_contacts_jumps_simple, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_change_modes_contacts_jumps_reactive, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_change_modes_contacts_runs_simple, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_change_modes_contacts_runs_intervallic, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_change_modes_contacts_force_sensor, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_change_modes_contacts_runs_encoder, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_change_modes_encoder_gravitatory, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_change_modes_encoder_inertial, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_show_modes_encoder, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_radio_mode_contacts_capture, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_radio_mode_contacts_analyze, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_radio_mode_encoder_capture_small, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_radio_mode_encoder_analyze_small, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_radio_mode_pulses_small, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_radio_mode_multi_chronopic_small, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_open_chronojump, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_help_close, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_news_close, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_exit_cancel, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_exit_confirm, UtilGtk.YELLOW, UtilGtk.YELLOW_LIGHT);
		app1s_eventboxes_paint();

		createComboSelectJumps(true);
		createComboSelectJumpsDjOptimalFall(true);
		createComboSelectJumpsWeightFVProfile(true);
		createComboSelectJumpsEvolution(true);
		createComboSelectJumpsRj(true);
		createComboSelectJumpsRjFatigue(true);
		createComboSelectJumpsRjFatigueNum(true);
		combo_jumps_rj_fatigue_divide_in.Active = 0;
		createComboSelectRuns(true);
		createComboSelectRunsEvolution(true);
		createComboSelectRunsEvolutionDistance();
		createComboSelectRunsInterval(true);

		//reaction_times has no combo
		//createComboPulses();
		//createComboMultiChronopic();
		createdStatsWin = false;

		createComboSessionLoadTags(true);

		repetitiveConditionsWin = RepetitiveConditionsWindow.Create();
		//to have objects ok to be able to be readed before viewing the repetitiveConditionsWin
		repetitiveConditionsWin.View(Constants.BellModes.ENCODERGRAVITATORY, preferences, encoderRhythm, false); //not viewWindow
		repetitiveConditionsWin.View(Constants.BellModes.FORCESENSOR, preferences, encoderRhythm, false); //not viewWindow
		repetitiveConditionsWin.FakeButtonClose.Clicked += new EventHandler(on_repetitive_conditions_closed);

		on_extra_window_multichronopic_test_changed(new object(), new EventArgs());
		on_extra_window_pulses_test_changed(new object(), new EventArgs());
		on_extra_window_reaction_times_test_changed(new object(), new EventArgs());
		on_extra_window_runs_interval_test_changed(new object(), new EventArgs());
		on_extra_window_runs_test_changed(new object(), new EventArgs());
		on_extra_window_jumps_rj_test_changed(new object(), new EventArgs());
		on_extra_window_jumps_test_changed(new object(), new EventArgs());
		//changeTestImage("", "", "LOGO");
		//We have no session, mark some widgets as ".Sensitive = false"
		sensitiveGuiNoSession();
		definedSession = false;
		
		rand = new Random(40);

		putNonStandardIcons();
		eventExecutePutNonStandardIcons();
		//eventExecuteCreateComboGraphResultsSize();


		/*
	
		if(chronopicPort != Constants.ChronopicDefaultPortWindows && 
				(chronopicPort != Constants.ChronopicDefaultPortLinux && File.Exists(chronopicPort))
		  ) {
			ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString("Do you want to connect to Chronopic now?"), "", "");
			confirmWin.Button_accept.Clicked += new EventHandler(chronopicAtStart);
		}
		*/

		stats_win_create();
		createdStatsWin = true;
		//stats_win_initializeSession();

		//these are constructed only one time
		threshold = new Threshold();
		cp2016 = new Chronopic2016();

		// ------ Creating sprint widgets ------

		spinbutton_sprint_export_image_width.Value = preferences.exportGraphWidth;
		spinbutton_sprint_export_image_height.Value = preferences.exportGraphHeight;
		notebook_sprint_analyze_top.CurrentPage = 0;


		// ------ Creating encoder widgets ------

		if(splashWin != null)
			splashWin.UpdateLabel(Catalog.GetString(Constants.SplashMessages[9]));

		//done here because in Glade we cannot use the TextBuffer.Changed
		textview_contacts_signal_comment.Buffer.Changed += new EventHandler(on_textview_contacts_signal_comment_key_press_event);

		contactsInitialize();
		encoderInitializeStuff();	

		//done before configInitRead because that will change some Tooltips
		addShortcutsToTooltips(operatingSystem == UtilAll.OperatingSystems.MACOSX);

		LogB.Information("Calling configInitRead from gui / ChronojumpWindow");
		configInitRead();

		//presentationInit();

		videoCaptureInitialize();

		initializeRestTimeLabels();
		restTime = new RestTime();
		updatingRestTimes = true;
		GLib.Timeout.Add(1000, new GLib.TimeoutHandler(updateRestTimes)); //each s, better than 5s for don't have problems sorting data on treeview

		// ------ Starting main window ------

		if(splashWin != null)
			splashWin.UpdateLabel(Catalog.GetString(Constants.SplashMessages[10]));

		/*
		 * start a ping in other thread
		 * http://www.mono-project.com/docs/gui/gtksharp/responsive-applications/
		 * Gtk.Application.Invoke
		 * but only start it if not on Compujump
		 */
		if( ! configChronojump.Compujump)
		{
			LogB.Information("Ping thread will start");

			pingThread = new Thread (new ThreadStart (pingAndNewsAtStart));
			GLib.Idle.Add (new GLib.IdleHandler (pulsePingAndNewsGTK));
			pingThread.Start();
		} else
			LogB.Information("Ping discarded (Compujump)");

		if(preferences.loadLastSessionAtStart && preferences.lastSessionID > 0 && ! configChronojump.Compujump)
		{
			// 1) to avoid impossibility to start Chronojump if there's any problem with this session, first put this to false
			SqlitePreferences.Update(SqlitePreferences.LoadLastSessionAtStart, false, false);

			// 2) load the session (but check if it really exists (extra check))
			Session sessionLoading = SqliteSession.Select (preferences.lastSessionID.ToString());
			if(sessionLoading.UniqueID != -1)
			{
				currentSession = sessionLoading;
				on_load_session_accepted();

				// 3) put preference to true again
				SqlitePreferences.Update(SqlitePreferences.LoadLastSessionAtStart, true, false);
			} else
				if(! check_menu_session.Active)
					check_menu_session.Click(); //have session menu opened
		} else
			if(! check_menu_session.Active)
				check_menu_session.Click(); //have sesion menu opened

		if(! showSendLog && ! showSocialNetworkPoll && preferences.loadLastModeAtStart &&
				preferences.lastMode != Constants.Modes.UNDEFINED && ! configChronojump.Compujump)
		{
			// 0) note this code is repeated on gui/sendLog.cs on_button_open_chronojump_clicked()
			// 1) to avoid impossibility to start Chronojump if there's any problem with this mode, first put this to false
			SqlitePreferences.Update(SqlitePreferences.LoadLastModeAtStart, false, false);

			// 2) change mode
			changeModeCheckRadios (preferences.lastMode);

			// 3) put preference to true again
			SqlitePreferences.Update(SqlitePreferences.LoadLastModeAtStart, true, false);
		}

		createComboSelectContactsTop (); //need to at least have it not null (to not crash on a import session)

		initialize_menu_or_menu_tiny();

		testNewStuff();

		//show before destroying/hiding app1 to see if this fixes rare problems of exiting/not showing app1
		LogB.Information("Showing app1");
		app1.Show();

		//ensure chronopicRegisterWindow is shown after (on top of) app1
		app1Shown = true;

		//done after app1.Show in order to be able to gather the colors
		doLabelsContrast(configChronojump.PersonWinHide);

		if(needToShowChronopicRegisterWindow)
		{
			LogB.Information("Show chronopic resgister win");
			chronopicRegisterWin.Show();
		}

		if(! showSendLog && ! showSocialNetworkPoll)
		{
			if (shouldAskBackupScheduled ())
				backupScheduledAsk ();
			else if(notebook_sup.CurrentPage == Convert.ToInt32(notebook_sup_pages.START))
				new ChronojumpLogo (notebook_chronojump_logo, drawingarea_chronojump_logo, viewport_chronojump_logo, preferences.logoAnimatedShow, preferences.fontType.ToString());
		}

		//done at the end to ensure main window is shown
		if(splashWin != null) {
			LogB.Information("Destroying splashWin");
			splashWin.Destroy();
		}
		else {
			LogB.Information("Hiding splashWin");
			SplashWindow.Hide();
		}

		LogB.Information("Chronojump window started");
	}

	private void contactsInitialize()
	{
		followSignals = false;
		check_contacts_capture_graph.Active = preferences.contactsCaptureDisplay.ShowGraph;
		check_contacts_capture_table.Active = preferences.contactsCaptureDisplay.ShowTable;
		followSignals = true;
		//call here to have the gui updated and preferences.encoderCaptureShowOnlyBars correctly assigned
		on_check_contacts_capture_show_modes_clicked (new object (), new EventArgs ());
	}

	//used on this free labels that have to contrast with background
	private void doLabelsContrast(bool personsAtTop)
	{
		if(personsAtTop)
		{
			if(! Config.UseSystemColor)
			{
				UtilGtk.ContrastLabelsHBox (Config.ColorBackgroundIsDark, hbox_top_person);
				UtilGtk.ContrastLabelsHBox (Config.ColorBackgroundIsDark, hbox_top_person_encoder);
				UtilGtk.ContrastLabelsTable (Config.ColorBackgroundIsDark, table_rest_time_contacts);
				UtilGtk.ContrastLabelsTable (Config.ColorBackgroundIsDark, table_rest_time_encoder);
			}

			if(! Config.UseSystemColor && Config.ColorBackgroundIsDark)
			{
				image_contacts_rest_time_dark_blue.Visible = false;
				image_contacts_rest_time_clear_yellow.Visible = true;
				image_encoder_rest_time_dark_blue.Visible = false;
				image_encoder_rest_time_clear_yellow.Visible = true;
			} else {
				image_contacts_rest_time_dark_blue.Visible = true;
				image_contacts_rest_time_clear_yellow.Visible = false;
				image_encoder_rest_time_dark_blue.Visible = true;
				image_encoder_rest_time_clear_yellow.Visible = false;
			}
		} else {
			if(! Config.UseSystemColor)
			{
				UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, label_session_at_frame_session);
				UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, label_current_session);
				UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, label_persons_at_frame_persons);
				UtilGtk.ContrastLabelsVBox (Config.ColorBackgroundIsDark, vbox_person_manage_create);
				UtilGtk.ContrastLabelsVBox (Config.ColorBackgroundIsDark, vbox_person_manage_load);
				UtilGtk.ContrastLabelsVBox (Config.ColorBackgroundIsDark, vbox_persons_bottom_no_photo);
			}

			Pixbuf pixbuf;
			pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_folders.png");
			if(Config.ColorBackgroundIsDark)
				pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_folders_yellow.png");
			image_menu_folders.Pixbuf = pixbuf;

			pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_person.png");
			if(Config.ColorBackgroundIsDark)
				pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_person_yellow.png");
			image_person2.Pixbuf = pixbuf;

			pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_no_photo.png");
			if(Config.ColorBackgroundIsDark)
				pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_no_photo_yellow.png");
			image_current_person.Pixbuf = pixbuf;
		}

		if(! Config.UseSystemColor)
		{
			UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundIsDark, app1s_notebook);
			UtilGtk.ContrastLabelsVBox (Config.ColorBackgroundIsDark, vbox_help);
		}

		LogB.Information(string.Format("UseSystemColor: {0}, ColorBackgroundIsDark: {1}", Config.UseSystemColor, Config.ColorBackgroundIsDark));
		if(! Config.UseSystemColor && Config.ColorBackgroundIsDark)
		{
			image_session_new_blue.Visible = false;
			image_session_load3_blue.Visible = false;
			image_session_import1_blue.Visible = false;
			image_news_blue.Visible = false;

			image_session_new_yellow.Visible = true;
			image_session_load3_yellow.Visible = true;
			image_session_import1_yellow.Visible = true;
			image_news_yellow.Visible = true;
		} else {
			image_session_new_blue.Visible = true;
			image_session_load3_blue.Visible = true;
			image_session_import1_blue.Visible = true;
			image_news_blue.Visible = true;

			image_session_new_yellow.Visible = false;
			image_session_load3_yellow.Visible = false;
			image_session_import1_yellow.Visible = false;
			image_news_yellow.Visible = false;
		}
		UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, label_news_frame);
	}

	private void testNewStuff()
	{
		//uncomment it to tests the method for add suffixes _copy2, _copy3 to encoderConfiguration
		//SqliteEncoderConfiguration.IfNameExistsAddSuffixDoTests();

		//Start window with moving widgets. Disabled
		//moveStartTestInitial();

		//uploadEncoderData test
		//Json js = new Json();
		//js.UploadEncoderData();

		/*
		//LeastSquaresParabole tests

		//a) straight line:
		LeastSquaresLine lsl = new LeastSquaresLine();
		lsl.Test();
		LogB.Information(string.Format("slope = {0}; intercept = {1}", lsl.Slope, lsl.Intercept));

		//b) LeastSquaresParabole test
		LeastSquaresParabole lsp = new LeastSquaresParabole();
		lsp.Test();
		LogB.Information(string.Format("coef = {0} {1} {2}", lsp.Coef[0], lsp.Coef[1], lsp.Coef[2]));
		*/

		//new VersionCompareTests();
		/*
		if(configChronojump.PlaySoundsFromFile)
		{
			Util.CreateSoundList();
			Util.UseSoundList = true;
			captureContWithCurves = false; //note set and reps are not currently saved
		}
		*/

		/*
		ChronoDebug cDebug = new ChronoDebug("ChronoDebug test new stuff");
		cDebug.Start();
		Random rnd = new Random();
		cDebug.Add("Created rnd");

		int reps = 1000000;
		for(int i=0; i < reps; i++)
			rnd.Next();

		cDebug.Add(string.Format("Done {0} rnds!", reps));
		cDebug.StopAndPrint();
		*/

		//Util.TestSortDoublesListstring();

		//Test ForceSensor GetVariabilityAndAccuracy: getVariabilityCVRMSSD
		//ForceSensorCapturePoints.TestVariabilityCVRMSSD();

		//InterpolateSignal.TestInterpolateBetween();
		//InterpolateSignal.TestCosineAndCubicInterpolate(true);

		//ConvertBooleansInt.Test();

		/*
		//Get size of automated backups dir
		int files;
		int sizeInKB;
		Util.GetBackupsSize (out files, out sizeInKB);
		LogB.Information(string.Format("Backups files: {0}, total size: {1} KB.",
					files, sizeInKB));
		*/

		//MovingAverage.TestCalculate();
	}



/*
	private void chronopicAtStart(object o, EventArgs args) {
		//make active menuitem chronopic, and this
		//will raise other things
	}
*/

/*
	private bool normalGUIOld = true; //to know if we changed state. Start as true
	private void on_app1_size_allocate(object obj, SizeAllocatedArgs args) {
		int width;
		int height;
		app1.GetSize(out width, out height);
		if(width >= 1000)
			normalGUI = true;
		else 
			normalGUI = false;
		if(normalGUI != normalGUIOld) {
			Log.WriteLine("Change Size. New is normal? -> " + normalGUI.ToString());
			normalGUIOld = normalGUI;
			changeGUIAspect();
		}
	}
	
	private void changeGUIAspect() {
		//QueryChildPacking(frame_test_options,
		if(normalGUI) {
			//if change these values, change also in glade
			//frame_test_options.BoxChild.Expand(true);
		} else {
			//frame_test_options.BoxChild.Expand(false);
		}
	}
*/

	//different than on_preferences_activate (opening preferences window)
	private void loadPreferencesAtStart ()
	{
		preferences = Preferences.LoadAllFromSqlite();
		LogB.Mute = preferences.muteLogs;

		LogB.Information (string.Format(Catalog.GetString("Chronojump database version file: {0}"), 
					preferences.databaseVersion));

		configInitFromPreferences();

		//---- encoder ----

		encoderRhythm = new EncoderRhythm(
				preferences.encoderRhythmActive, preferences.encoderRhythmRepsOrPhases,
				preferences.encoderRhythmRepSeconds,
				preferences.encoderRhythmEccSeconds, preferences.encoderRhythmConSeconds,
				preferences.encoderRhythmRestRepsSeconds, preferences.encoderRhythmRestAfterEcc,
				preferences.encoderRhythmRepsCluster, preferences.encoderRhythmRestClustersSeconds);

		//---- jumps ----

		checkbutton_allow_finish_rj_after_time.Active = preferences.allowFinishRjAfterTime;
		check_jumps_weight_fv_profile_only_best_in_weight.Active = preferences.jumpsFVProfileOnlyBestInWeight;
		if(preferences.jumpsFVProfileShowFullGraph)
			radio_jumps_weight_fv_profile_show_full_graph.Active = true;
		else
			radio_jumps_weight_fv_profile_zoom_to_points.Active = true;
		check_jumps_evolution_only_best_in_session.Active = preferences.jumpsEvolutionOnlyBestInSession;

		//---- runs ----
		check_runs_evolution_only_best_in_session.Active = preferences.runsEvolutionOnlyBestInSession;
		check_runs_evolution_show_time.Active = preferences.runsEvolutionShowTime;

		//---- video ----

		UtilGtk.ColorsCheckOnlyPrelight(checkbutton_video_contacts);
		UtilGtk.ColorsCheckOnlyPrelight(checkbutton_video_encoder);
		
		//don't raise the signal	
		checkbutton_video_contacts.Clicked -= new EventHandler(on_checkbutton_video_contacts_clicked);
		checkbutton_video_contacts.Active = preferences.videoOn;
		checkbutton_video_contacts.Clicked += new EventHandler(on_checkbutton_video_contacts_clicked);
		//don't raise the signal	
		checkbutton_video_encoder.Clicked -= new EventHandler(on_checkbutton_video_encoder_clicked);
		checkbutton_video_encoder.Active = preferences.videoOn;
		checkbutton_video_encoder.Clicked += new EventHandler(on_checkbutton_video_encoder_clicked);
		
		changeVideoButtons(preferences.videoOn);

		//change language works on windows. On Linux let's change the locale
		//if(UtilAll.IsWindows())
		//	languageChange();

		//pass to report
		report.preferences = preferences;
		report.Progversion = progVersion;

		if(myTreeViewPersons != null)
			myTreeViewPersons.RestSecondsMark = get_configured_rest_time_in_seconds();

		LogB.Information ( Catalog.GetString ("Preferences loaded") );
	}

	/*
	 * languageChange is not related to windows and linux, is related to .net or mono
	 * on .net (windows) we can change language. On mono, we use locale
	 * now since 0.53 svn, we use mono on windows and linux, then this is not used
	 *
	private void languageChange () {
		string myLanguage = SqlitePreferences.Select("language");
		if ( myLanguage != "0") {
			try {
				Log.WriteLine("myLanguage: {0}", myLanguage);
				System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(myLanguage);
				System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(myLanguage);
				//probably only works on newly created windows, if change, then say user has to restart
				Log.WriteLine ("Changed language to {0}", myLanguage );
			} catch {
				new DialogMessage(Catalog.GetString("There's a problem with this language on this computer. Please, choose another language."));
			}
		}
	}
*/

	/* ---------------------------------------------------------
	 * ----------------  test modes (small GUI) ----------------
	 *  --------------------------------------------------------
	 */


	public void on_radio_mode_pulses_small_toggled (object obj, EventArgs args) {
		if(radio_mode_pulses_small.Active)
		{
			sensitiveLastTestButtons(false);
			notebooks_change(Constants.Modes.OTHER);
			on_extra_window_pulses_test_changed(obj, args);

			event_graph_label_graph_test.Visible = true;
			vbox_contacts_simple_graph_controls.Visible = false;

			//align_check_vbox_contacts_graph_legend.Visible = false;
			//vbox_contacts_graph_legend.Visible = false;
		}
	}

	public void on_radio_mode_multi_chronopic_small_toggled (object obj, EventArgs args) {
		if(radio_mode_multi_chronopic_small.Active)
		{
			sensitiveLastTestButtons(false);
			notebooks_change(Constants.Modes.OTHER);
			on_extra_window_multichronopic_test_changed(obj, args);

			event_graph_label_graph_test.Visible = true;
			vbox_contacts_simple_graph_controls.Visible = false;

			//align_check_vbox_contacts_graph_legend.Visible = false;
			//vbox_contacts_graph_legend.Visible = false;
		}
	}

	public void on_radio_mode_encoder_capture_small_toggled (object obj, EventArgs args) {
		if(radio_mode_encoder_capture_small.Active) 
			notebook_encoder_sup.CurrentPage = 0;
	}

	public void on_radio_mode_encoder_analyze_small_toggled (object o, EventArgs args) {
		if(radio_mode_encoder_analyze_small.Active)
		{
			notebook_encoder_sup.CurrentPage = 1;

			if(radio_encoder_analyze_individual_current_set.Active)
				on_radio_encoder_analyze_individual_current_set (o, args);
			else if(radio_encoder_analyze_individual_current_session.Active)
				on_radio_encoder_analyze_individual_current_session (o, args);
			else if(radio_encoder_analyze_individual_all_sessions.Active)
				on_radio_encoder_analyze_individual_all_sessions (o, args);
			else if(radio_encoder_analyze_groupal_current_session.Active)
				on_radio_encoder_analyze_groupal_current_session (o, args);
		}
	}

	
	/* ---------------------------------------------------------
	 * ----------------  TREEVIEW (generic) --------------------
	 *  --------------------------------------------------------
	 */

	private void expandOrMinimizeTreeView(TreeViewEvent tvEvent, TreeView tv) {
		if(tvEvent.ExpandState == TreeViewEvent.ExpandStates.MINIMIZED) 
			tv.CollapseAll();
		else if (tvEvent.ExpandState == TreeViewEvent.ExpandStates.OPTIMAL) {
			tv.CollapseAll();
			tvEvent.ExpandOptimal();
		} else   //MAXIMIZED
			tv.ExpandAll();

		//Log.WriteLine("IS " + tvEvent.ExpandState);
	}

	private void on_treeview_button_release_event (object o, ButtonReleaseEventArgs args)
	{
		Gdk.EventButton e = args.Event;
		Gtk.TreeView myTv = (Gtk.TreeView) o;
		if (e.Button == 3) {
			if(myTv == treeview_persons && currentPerson != null) {
				treeviewPersonsContextMenu(currentPerson);
			} else if(myTv == treeview_jumps) {
				if (myTreeViewJumps.EventSelectedID > 0) {
					Jump myJump = SqliteJump.SelectJumpData( myTreeViewJumps.EventSelectedID, false );
					treeviewJumpsContextMenu(myJump);
				}
			} else if(myTv == treeview_jumps_rj) {
				if (myTreeViewJumpsRj.EventSelectedID > 0) {
					JumpRj myJump = SqliteJumpRj.SelectJumpData( "jumpRj", myTreeViewJumpsRj.EventSelectedID, false );
					treeviewJumpsRjContextMenu(myJump);
				}
			} else if(myTv == treeview_runs) {
				if (myTreeViewRuns.EventSelectedID > 0) {
					Run myRun = SqliteRun.SelectRunData( myTreeViewRuns.EventSelectedID, false );
					treeviewRunsContextMenu(myRun);
				}
			} else if(myTv == treeview_runs_interval) {
				if (myTreeViewRunsInterval.EventSelectedID > 0) {
					RunInterval myRun = SqliteRunInterval.SelectRunData( Constants.RunIntervalTable, myTreeViewRunsInterval.EventSelectedID, false );
					treeviewRunsIntervalContextMenu(myRun);
				}
			} else if(myTv == treeview_reaction_times) {
				if (myTreeViewReactionTimes.EventSelectedID > 0) {
					ReactionTime myRt = SqliteReactionTime.SelectReactionTimeData( myTreeViewReactionTimes.EventSelectedID, false );
					treeviewReactionTimesContextMenu(myRt);
				}
			} else if(myTv == treeview_pulses) {
				if (myTreeViewPulses.EventSelectedID > 0) {
					Pulse myPulse = SqlitePulse.SelectPulseData( myTreeViewPulses.EventSelectedID, false );
					treeviewPulsesContextMenu(myPulse);
				}
			} else if(myTv == treeview_multi_chronopic) {
				if (myTreeViewMultiChronopic.EventSelectedID > 0) {
					MultiChronopic mc = SqliteMultiChronopic.SelectMultiChronopicData( myTreeViewMultiChronopic.EventSelectedID, false );
					treeviewMultiChronopicContextMenu(mc);
				}
			} else
				LogB.Information(myTv.ToString());
		}
	}

	/* ---------------------------------------------------------
	 * ----------------  TREEVIEW PERSONS ----------------------
	 *  --------------------------------------------------------
	 */

	private void createTreeView_persons (Gtk.TreeView tv) {
		myTreeViewPersons = new TreeViewPersons(tv, get_configured_rest_time_in_seconds());
		tv.Selection.Changed += onTreeviewPersonsSelectionEntry;
	}

	private void fillTreeView_persons () {
		ArrayList myPersons = SqlitePersonSession.SelectCurrentSessionPersons(
				currentSession.UniqueID, 
				false); //means: do not returnPersonAndPSlist

		if(myPersons.Count > 0) {
			//fill treeview
			myTreeViewPersons.Fill(myPersons, restTime);
		}
	}

	private void on_treeview_persons_up (object o, EventArgs args) {
		myTreeViewPersons.SelectPreviousRow(currentPerson.UniqueID);
	}
	
	private void on_treeview_persons_down (object o, EventArgs args) {
		myTreeViewPersons.SelectNextRow(currentPerson.UniqueID);
	}
	
	//return true if selection is done (there's any person)
	private bool selectRowTreeView_persons(Gtk.TreeView tv, int rowNum)
	{
		LogB.Information("selectRowTreeView_persons");

		if(! myTreeViewPersons.SelectRow(rowNum))
			return false;
		
		//the selection of row in treeViewPersons.SelectRow is not a real selection 
		//and unfortunately doesn't raises the on_treeview_persons_cursor_changed ()
		//for this reason we reproduce the method here
		TreeModel model;
		TreeIter iter;
		if (tv.Selection.GetSelected (out model, out iter)) {
			string selectedID = (string) model.GetValue (iter, 0); //ID, Name
			currentPerson = SqlitePerson.Select(Convert.ToInt32(selectedID));
			currentPersonSession = SqlitePersonSession.Select(Convert.ToInt32(selectedID), currentSession.UniqueID);
			label_person_change();
			TreePath path = model.GetPath (iter);
			tv.ScrollToCell (path, null, true, 0, 0);
		
			return true;
		} else {
			return false;
		}
	}

	private void on_button_image_current_person_zoom_clicked(object o, EventArgs args)
	{
		new DialogImageTest(currentPerson.Name,
				Util.UserPhotoURL(false, currentPerson.UniqueID),
				DialogImageTest.ArchiveType.FILE);
	}
	
	private void treeview_persons_storeReset()
	{
		myTreeViewPersons.RemoveColumns();
		myTreeViewPersons = new TreeViewPersons(treeview_persons, get_configured_rest_time_in_seconds());
	}
	
	//private void on_treeview_persons_cursor_changed (object o, EventArgs args) {
	private void onTreeviewPersonsSelectionEntry (object o, EventArgs args)
	{
		TreeModel model;
		TreeIter iter;

		// you get the iter and the model if something is selected
		if (((TreeSelection)o).GetSelected(out model, out iter)) {
			string selectedID = (string) model.GetValue (iter, 0); //ID, Name
		
			currentPerson = SqlitePerson.Select(Convert.ToInt32(selectedID));
			currentPersonSession = SqlitePersonSession.Select(Convert.ToInt32(selectedID), currentSession.UniqueID);
			label_person_change();
	
			personChanged();
			button_persons_up.Sensitive = ! myTreeViewPersons.IsFirst(currentPerson.UniqueID);
			button_persons_down.Sensitive = ! myTreeViewPersons.IsLast(currentPerson.UniqueID);
		}
	}

	private void personChanged()
	{
		sensitiveLastTestButtons(false);

		label_current_person.Text = currentPerson.Name;

		//update 4 results treeviews
		myTreeViewJumps.SelectPerson(currentPerson.Name);
		myTreeViewJumpsRj.SelectPerson(currentPerson.Name);
		myTreeViewRuns.SelectPerson(currentPerson.Name);
		myTreeViewRunsInterval.SelectPerson(currentPerson.Name);

		//1) change on jumps, runs, pulse capture graph
		if(current_mode == Constants.Modes.JUMPSSIMPLE)
		{
			if(! configChronojump.Exhibition)
				updateGraphJumpsSimple();

			update_label_extra_window_jumps_radiobutton_weight_percent_as_kg(
					(currentJumpType.HasWeight && extra_window_jumps_radiobutton_weight.Active));

			if(notebook_analyze.CurrentPage == Convert.ToInt32(notebook_analyze_pages.JUMPSPROFILE))
				jumpsProfileDo(true); //calculate data
			else if(notebook_analyze.CurrentPage == Convert.ToInt32(notebook_analyze_pages.JUMPSDJOPTIMALFALL))
				jumpsDjOptimalFallDo(true); //calculate data
			else if(notebook_analyze.CurrentPage == Convert.ToInt32(notebook_analyze_pages.JUMPSWEIGHTFVPROFILE))
				jumpsWeightFVProfileDo(true); //calculate data
			else if(notebook_analyze.CurrentPage == Convert.ToInt32(notebook_analyze_pages.JUMPSEVOLUTION))
				jumpsEvolutionDo(true); //calculate data
		}
		else if(current_mode == Constants.Modes.JUMPSREACTIVE)
		{
			blankJumpReactiveRealtimeCaptureGraph ();
			updateGraphJumpsReactive();

			update_label_extra_window_jumps_rj_radiobutton_weight_percent_as_kg(
					(currentJumpRjType.HasWeight && extra_window_jumps_rj_radiobutton_weight.Active));

			if(notebook_analyze.CurrentPage == Convert.ToInt32(notebook_analyze_pages.JUMPSRJFATIGUE))
				createComboSelectJumpsRjFatigueNum (false);
		}
		else if(current_mode == Constants.Modes.RUNSSIMPLE)
		{
			updateGraphRunsSimple();

			if(notebook_analyze.CurrentPage == Convert.ToInt32(notebook_analyze_pages.RUNSEVOLUTION))
				runsEvolutionDo(true, true); //calculate data
		}
		else if(current_mode == Constants.Modes.RUNSINTERVALLIC)
		{
			blankRunIntervalRealtimeCaptureGraph ();
			updateGraphRunsInterval();

			if(currentPerson != null)
				label_sprint_person_name.Text = string.Format(Catalog.GetString("Sprints of {0}"), currentPerson.Name);
			createTreeView_runs_interval_sprint (treeview_runs_interval_sprint);

			if(currentPerson != null)
				label_sprint_export_data.Text = currentPerson.Name;
			else
				label_sprint_export_data.Text = "";

			label_sprint_export_result.Text = "";
			button_sprint_export_result_open.Visible = false;
		}
		//else if(current_mode == Constants.Modes.RUNSENCODER)
		//{
		//}
		else if(current_mode == Constants.Modes.RT)
			updateGraphReactionTimes();

		//2) changes on encoder and forceSensor
		encoderPersonChanged();
		forceSensorPersonChanged();
		runEncoderPersonChanged();
	}

	private void treeviewPersonsContextMenu(Person myPerson) {
		Menu myMenu = new Menu ();
		Gtk.MenuItem myItem;

		myItem = new MenuItem ( Catalog.GetString("Edit") + " " + myPerson.Name);
		myItem.Activated += on_edit_current_person_clicked_from_main_gui;
		myMenu.Attach( myItem, 0, 1, 0, 1 );

		myItem = new MenuItem ( Catalog.GetString("Show all tests of") + " " + myPerson.Name);
		myItem.Activated += on_show_all_person_events_activate;
		myMenu.Attach( myItem, 0, 1, 1, 2 );

		Gtk.SeparatorMenuItem mySep = new SeparatorMenuItem();
		myMenu.Attach( mySep, 0, 1, 2, 3 );

		myItem = new MenuItem ( string.Format(Catalog.GetString("Delete {0} from this session"),myPerson.Name));
		myItem.Activated += on_delete_current_person_from_session_clicked;
		myMenu.Attach( myItem, 0, 1, 3, 4 );

		myMenu.Popup();
		myMenu.ShowAll();
	}
		

	private void resetAllTreeViews(bool fillTests, bool resetPersons, bool fillPersons)
	{
		//persons
		if(resetPersons) {
			treeview_persons_storeReset();
			if(fillPersons)
				fillTreeView_persons();
		}

		treeview_jumps_rj_storeReset();
		treeview_runs_storeReset();
		treeview_runs_interval_storeReset();
		treeview_pulses_storeReset();
		treeview_reaction_times_storeReset();

		//Leave SQL opened in all this process
		Sqlite.Open(); // ------------------------------

		treeview_multi_chronopic_storeReset(true); //this neeed DB

		if(fillTests)
		{
			fillTreeView_jumps_rj(Constants.AllJumpsNameStr(), true);
			fillTreeView_runs(Constants.AllRunsNameStr(), true);
			fillTreeView_runs_interval(Constants.AllRunsNameStr(), true);
			fillTreeView_pulses(Constants.AllPulsesNameStr(), true);
			fillTreeView_reaction_times("reactionTime", true);
			fillTreeView_multi_chronopic(true);
		}

		if(fillTests)
			pre_fillTreeView_jumps (true);
		else
			treeview_jumps_storeReset();

		//close SQL opened in all this process
		Sqlite.Close(); // ------------------------------
	}


	/* ---------------------------------------------------------
	 * ----------------  TREEVIEW JUMPS ------------------------
	 *  --------------------------------------------------------
	 */

	private void createTreeView_jumps (Gtk.TreeView tv) {
		//myTreeViewJumps is a TreeViewJumps instance
		myTreeViewJumps = new TreeViewJumps(tv, preferences, TreeViewEvent.ExpandStates.MINIMIZED);

		//the glade cursor_changed does not work on mono 1.2.5 windows
		tv.CursorChanged += on_treeview_jumps_cursor_changed; 
	}

	private void fillTreeView_jumps (string filter) {
		fillTreeView_jumps(filter, false);
	}
	private void fillTreeView_jumps (string filter, bool dbconOpened)
	{
		if (currentSession == null) {
			/*
			 * This happens when the user "Imports a session": Chronojump tries to
			 * update comboboxes, it reaches here because the comboboxes are updated
			 * But if the user didn't have any
			 * open session currentSession variable (see below) is null and it crashed here
			 * (when it did currentSession.UniqueID with currentSession==null)
			 */
			return;
		}

		string [] myJumps = SqliteJump.SelectJumpsSA (dbconOpened, currentSession.UniqueID, -1, "", "",
				Sqlite.Orders_by.DEFAULT, -1);

		myTreeViewJumps.Fill(myJumps, filter);

		expandOrMinimizeTreeView((TreeViewEvent) myTreeViewJumps, treeview_jumps);
	}

	private void on_button_jumps_zoom_clicked (object o, EventArgs args) {
		myTreeViewJumps.ExpandState = myTreeViewJumps.ZoomChange(myTreeViewJumps.ExpandState);
		if(myTreeViewJumps.ExpandState == TreeViewEvent.ExpandStates.MINIMIZED) {
			treeview_jumps.CollapseAll();
			image_jumps_zoom.Pixbuf = new Pixbuf(null, Util.GetImagePath(false) + "zoom_in.png");
		} else {
			treeview_jumps.ExpandAll();
			image_jumps_zoom.Pixbuf = new Pixbuf(null, Util.GetImagePath(false) + "zoom_out.png");
		}
	}
	
	private void treeview_jumps_storeReset() {
		myTreeViewJumps.RemoveColumns();
		
		myTreeViewJumps = new TreeViewJumps(treeview_jumps, preferences, myTreeViewJumps.ExpandState);
	}

	private void on_treeview_jumps_cursor_changed (object o, EventArgs args)
	{
		LogB.Information("Cursor changed");

		sensitiveLastTestButtons(false);

		// don't select if it's a person, 
		// is for not confusing with the person treeviews that controls who jumps
		if (myTreeViewJumps.EventSelectedID == 0) {
			myTreeViewJumps.Unselect();
			showHideActionEventButtons(false, "Jump"); //hide
		} else {
			showHideActionEventButtons(true, "Jump"); //show
		}
	}

	private void treeviewJumpsContextMenu(Jump myJump) {
		Menu myMenu = new Menu ();
		Gtk.MenuItem myItem;

		/*
		myItem = new MenuItem ( Catalog.GetString("Play Video") + " " + 
				myJump.Type + " (" + myJump.PersonName + ")");
		if(File.Exists(Util.GetVideoFileName(currentSession.UniqueID, 
				Constants.TestTypes.JUMP, myTreeViewJumps.EventSelectedID))) {
			myItem.Activated += on_video_play_selected_jump_clicked;
			myItem.Sensitive = true;
		} else 
			myItem.Sensitive = false;
		myMenu.Attach( myItem, 0, 1, 0, 1 );
		*/

		myItem = new MenuItem ( Catalog.GetString("Edit selected") + " " + myJump.Type + " (" + myJump.PersonName + ")");
		myItem.Activated += on_edit_selected_jump_clicked;
		myMenu.Attach( myItem, 0, 1, 0, 1 );
	
		Gtk.SeparatorMenuItem mySep = new SeparatorMenuItem();
		myMenu.Attach( mySep, 0, 1, 1, 2 );

		myItem = new MenuItem ( Catalog.GetString("Delete selected") + " " + myJump.Type + " (" + myJump.PersonName + ")");
		myItem.Activated += on_delete_selected_jump_clicked;
		myMenu.Attach( myItem, 0, 1, 2, 3 );

		myMenu.Popup();
		myMenu.ShowAll();
	}

	/* ---------------------------------------------------------
	 * ----------------  TREEVIEW JUMPS RJ ---------------------
	 *  --------------------------------------------------------
	 */

	private void createTreeView_jumps_rj (Gtk.TreeView tv) {
		myTreeViewJumpsRj = new TreeViewJumpsRj (tv, preferences, TreeViewEvent.ExpandStates.MINIMIZED);

		//the glade cursor_changed does not work on mono 1.2.5 windows
		tv.CursorChanged += on_treeview_jumps_rj_cursor_changed; 
	}

	private void fillTreeView_jumps_rj (string filter) {
		fillTreeView_jumps_rj (filter, false);
	}
	private void fillTreeView_jumps_rj (string filter, bool dbconOpened)
	{
		if (currentSession == null) {
			/*
			 * This happens when the user "Imports a session": Chronojump tries to
			 * update comboboxes, it reaches here because the comboboxes are updated
			 * But if the user didn't have any
			 * open session currentSession variable (see below) is null and it crashed here
			 * (when it did currentSession.UniqueID with currentSession==null)
			 */
			return;
		}

		string [] myJumps = SqliteJumpRj.SelectJumpsSA (dbconOpened, currentSession.UniqueID, -1, "", "");
		myTreeViewJumpsRj.Fill(myJumps, filter);

		expandOrMinimizeTreeView((TreeViewEvent) myTreeViewJumpsRj, treeview_jumps_rj);

	}

	private void on_button_jumps_rj_zoom_clicked (object o, EventArgs args) {
		myTreeViewJumpsRj.ExpandState = myTreeViewJumpsRj.ZoomChange(myTreeViewJumpsRj.ExpandState);
		if(myTreeViewJumpsRj.ExpandState == TreeViewEvent.ExpandStates.MINIMIZED) {
			treeview_jumps_rj.CollapseAll();
			image_jumps_rj_zoom.Pixbuf = new Pixbuf(null, Util.GetImagePath(false) + "zoom_in.png");
		} else if(myTreeViewJumpsRj.ExpandState == TreeViewEvent.ExpandStates.OPTIMAL) {
			treeview_jumps_rj.CollapseAll();
			myTreeViewJumpsRj.ExpandOptimal();
			image_jumps_rj_zoom.Pixbuf = new Pixbuf(null, Util.GetImagePath(false) + "zoom_in.png");
		} else {
			treeview_jumps_rj.ExpandAll();
			image_jumps_rj_zoom.Pixbuf = new Pixbuf(null, Util.GetImagePath(false) + "zoom_out.png");
		}
	}

	private void treeview_jumps_rj_storeReset() {
		myTreeViewJumpsRj.RemoveColumns();
		myTreeViewJumpsRj = new TreeViewJumpsRj (treeview_jumps_rj, preferences, myTreeViewJumpsRj.ExpandState);
	}

	private void on_treeview_jumps_rj_cursor_changed (object o, EventArgs args)
	{
		sensitiveLastTestButtons(false);

		// don't select if it's a person, 
		// is for not confusing with the person treeviews that controls who jumps
		if (myTreeViewJumpsRj.EventSelectedID == 0) {
			myTreeViewJumpsRj.Unselect();
			showHideActionEventButtons(false, "JumpRj");
		} else if (myTreeViewJumpsRj.EventSelectedID == -1) {
			myTreeViewJumpsRj.SelectHeaderLine();
			showHideActionEventButtons(true, "JumpRj");
		} else {
			showHideActionEventButtons(true, "JumpRj");
		}
	}

	private void treeviewJumpsRjContextMenu(JumpRj myJump) {
		Menu myMenu = new Menu ();
		Gtk.MenuItem myItem;

		/*
		myItem = new MenuItem ( Catalog.GetString("Play Video") + " " + 
				myJump.Type + " (" + myJump.PersonName + ")");
		if(File.Exists(Util.GetVideoFileName(currentSession.UniqueID, 
				Constants.TestTypes.JUMP_RJ, myTreeViewJumpsRj.EventSelectedID))) {
			myItem.Activated += on_video_play_selected_jump_rj_clicked;
			myItem.Sensitive = true;
		} else 
			myItem.Sensitive = false;
		myMenu.Attach( myItem, 0, 1, 0, 1 );
		*/
		
		myItem = new MenuItem ( Catalog.GetString("Edit selected") + " " + myJump.Type + " (" + myJump.PersonName + ")");
		myItem.Activated += on_edit_selected_jump_rj_clicked;
		myMenu.Attach( myItem, 0, 1, 0, 1 );

		myItem = new MenuItem ( Catalog.GetString("Repair selected") + " " + myJump.Type + " (" + myJump.PersonName + ")");
		myItem.Activated += on_repair_selected_jump_rj_clicked;
		myMenu.Attach( myItem, 0, 1, 1, 2 );
		
		Gtk.SeparatorMenuItem mySep = new SeparatorMenuItem();
		myMenu.Attach( mySep, 0, 1, 2, 3 );

		myItem = new MenuItem ( Catalog.GetString("Delete selected") + " " + myJump.Type + " (" + myJump.PersonName + ")");
		myItem.Activated += on_delete_selected_jump_rj_clicked;
		myMenu.Attach( myItem, 0, 1, 3, 4 );

		myMenu.Popup();
		myMenu.ShowAll();
	}

	/* ---------------------------------------------------------
	 * ----------------  TREEVIEW RUNS -------------------------
	 *  --------------------------------------------------------
	 */

	private void createTreeView_runs (Gtk.TreeView tv) {
		//myTreeViewRuns is a TreeViewRuns instance
		myTreeViewRuns = new TreeViewRuns (tv, preferences.digitsNumber, preferences.metersSecondsPreferred, TreeViewEvent.ExpandStates.MINIMIZED );

		//the glade cursor_changed does not work on mono 1.2.5 windows
		tv.CursorChanged += on_treeview_runs_cursor_changed; 
	}

	private void fillTreeView_runs (string filter) {
		fillTreeView_runs (filter, false);
	}
	private void fillTreeView_runs (string filter, bool dbconOpened)
	{
		if (currentSession == null) {
			/*
			 * This happens when the user "Imports a session": Chronojump tries to
			 * update comboboxes, it reaches here because the comboboxes are updated
			 * But if the user didn't have any
			 * open session currentSession variable (see below) is null and it crashed here
			 * (when it did currentSession.UniqueID with currentSession==null)
			 */
			return;
		}

		string [] myRuns = SqliteRun.SelectRunsSA (dbconOpened, currentSession.UniqueID, -1, "",
				Sqlite.Orders_by.DEFAULT, -1);

		myTreeViewRuns.Fill(myRuns, filter);

		expandOrMinimizeTreeView((TreeViewEvent) myTreeViewRuns, treeview_runs);
	}
	
	private void on_button_runs_zoom_clicked (object o, EventArgs args) {
		myTreeViewRuns.ExpandState = myTreeViewRuns.ZoomChange(myTreeViewRuns.ExpandState);
		if(myTreeViewRuns.ExpandState == TreeViewEvent.ExpandStates.MINIMIZED) {
			treeview_runs.CollapseAll();
			image_runs_zoom.Pixbuf = new Pixbuf(null, Util.GetImagePath(false) + "zoom_in.png");
		} else {
			treeview_runs.ExpandAll();
			image_runs_zoom.Pixbuf = new Pixbuf(null, Util.GetImagePath(false) + "zoom_out.png");
		}
	}
	
	private void treeview_runs_storeReset() {
		myTreeViewRuns.RemoveColumns();
		myTreeViewRuns = new TreeViewRuns(treeview_runs, preferences.digitsNumber, preferences.metersSecondsPreferred, myTreeViewRuns.ExpandState);
	}

	private void on_treeview_runs_cursor_changed (object o, EventArgs args)
	{
		sensitiveLastTestButtons(false);

		// don't select if it's a person, 
		// is for not confusing with the person treeviews that controls who runs
		if (myTreeViewRuns.EventSelectedID == 0) {
			myTreeViewRuns.Unselect();
			showHideActionEventButtons(false, "Run");
		} else {
			showHideActionEventButtons(true, "Run");
		}
	}

	private void treeviewRunsContextMenu(Run myRun) {
		Menu myMenu = new Menu ();
		Gtk.MenuItem myItem;

		/*
		myItem = new MenuItem ( Catalog.GetString("Play Video") + " " + 
				myRun.Type + " (" + myRun.PersonName + ")");
		if(File.Exists(Util.GetVideoFileName(currentSession.UniqueID, 
				Constants.TestTypes.RUN, myTreeViewRuns.EventSelectedID))) {
			myItem.Activated += on_video_play_selected_run_clicked;
			myItem.Sensitive = true;
		} else 
			myItem.Sensitive = false;
		myMenu.Attach( myItem, 0, 1, 0, 1 );
		*/

		myItem = new MenuItem ( Catalog.GetString("Edit selected") + " " + myRun.Type + " (" + myRun.PersonName + ")");
		myItem.Activated += on_edit_selected_run_clicked;
		myMenu.Attach( myItem, 0, 1, 0, 1 );

		Gtk.SeparatorMenuItem mySep = new SeparatorMenuItem();
		myMenu.Attach( mySep, 0, 1, 1, 2 );

		myItem = new MenuItem ( Catalog.GetString("Delete selected") + " " + myRun.Type + " (" + myRun.PersonName + ")");
		myItem.Activated += on_delete_selected_run_clicked;
		myMenu.Attach( myItem, 0, 1, 2, 3 );

		myMenu.Popup();
		myMenu.ShowAll();
	}

	/* ---------------------------------------------------------
	 * ----------------  TREEVIEW RUN INTERVAL -----------------
	 *  --------------------------------------------------------
	 */

	private void createTreeView_runs_interval (Gtk.TreeView tv) {
		//myTreeViewRunsInterval is a TreeViewRunsInterval instance
		myTreeViewRunsInterval = new TreeViewRunsInterval (tv, preferences.digitsNumber, preferences.metersSecondsPreferred, TreeViewEvent.ExpandStates.MINIMIZED);

		//the glade cursor_changed does not work on mono 1.2.5 windows
		tv.CursorChanged += on_treeview_runs_interval_cursor_changed; 
	}

	private void fillTreeView_runs_interval (string filter) {
		fillTreeView_runs_interval (filter, false);
	}
	private void fillTreeView_runs_interval (string filter, bool dbconOpened)
	{
		if (currentSession == null) {
			/*
			 * This happens when the user "Imports a session": Chronojump tries to
			 * update comboboxes, it reaches here because the comboboxes are updated
			 * But if the user didn't have any
			 * open session currentSession variable (see below) is null and it crashed here
			 * (when it did currentSession.UniqueID with currentSession==null)
			 */
			return;
		}

		string [] myRuns = SqliteRunInterval.SelectRunsSA (dbconOpened, currentSession.UniqueID, -1, "");
		myTreeViewRunsInterval.Fill(myRuns, filter);
		expandOrMinimizeTreeView((TreeViewEvent) myTreeViewRunsInterval, treeview_runs_interval);
	}
	
	private void on_button_runs_interval_zoom_clicked (object o, EventArgs args) {
		myTreeViewRunsInterval.ExpandState = myTreeViewRunsInterval.ZoomChange(myTreeViewRunsInterval.ExpandState);
		if(myTreeViewRunsInterval.ExpandState == TreeViewEvent.ExpandStates.MINIMIZED) {
			treeview_runs_interval.CollapseAll();
			image_runs_interval_zoom.Pixbuf = new Pixbuf(null, Util.GetImagePath(false) + "zoom_in.png");
		} else if(myTreeViewRunsInterval.ExpandState == TreeViewEvent.ExpandStates.OPTIMAL) {
			treeview_runs_interval.CollapseAll();
			myTreeViewRunsInterval.ExpandOptimal();
			image_runs_interval_zoom.Pixbuf = new Pixbuf(null, Util.GetImagePath(false) + "zoom_in.png");
		} else {
			treeview_runs_interval.ExpandAll();
			image_runs_interval_zoom.Pixbuf = new Pixbuf(null, Util.GetImagePath(false) + "zoom_out.png");
		}
	}

	private void treeview_runs_interval_storeReset() {
		myTreeViewRunsInterval.RemoveColumns();
		myTreeViewRunsInterval = new TreeViewRunsInterval (treeview_runs_interval,  
				preferences.digitsNumber, preferences.metersSecondsPreferred, myTreeViewRunsInterval.ExpandState);
	}

	private void on_treeview_runs_interval_cursor_changed (object o, EventArgs args)
	{
		sensitiveLastTestButtons(false);

		// don't select if it's a person, 
		// is for not confusing with the person treeviews that controls who runs
		if (myTreeViewRunsInterval.EventSelectedID == 0) {
			myTreeViewRunsInterval.Unselect();
			showHideActionEventButtons(false, "RunInterval");
		} else if (myTreeViewRunsInterval.EventSelectedID == -1) {
			myTreeViewRunsInterval.SelectHeaderLine();
			showHideActionEventButtons(true, "RunInterval");
		} else {
			showHideActionEventButtons(true, "RunInterval");
		}
	}

	private void treeviewRunsIntervalContextMenu(RunInterval myRun) {
		Menu myMenu = new Menu ();
		Gtk.MenuItem myItem;

		/*
		myItem = new MenuItem ( Catalog.GetString("Play Video") + " " + 
				myRun.Type + " (" + myRun.PersonName + ")");
		if(File.Exists(Util.GetVideoFileName(currentSession.UniqueID, 
				Constants.TestTypes.RUN_I, myTreeViewRunsInterval.EventSelectedID))) {
			myItem.Activated += on_video_play_selected_run_interval_clicked;
			myItem.Sensitive = true;
		} else 
			myItem.Sensitive = false;
		myMenu.Attach( myItem, 0, 1, 0, 1 );
		*/

		myItem = new MenuItem ( Catalog.GetString("Edit selected") + " " + myRun.Type + " (" + myRun.PersonName + ")");
		myItem.Activated += on_edit_selected_run_interval_clicked;
		myMenu.Attach( myItem, 0, 1, 0, 1 );

		myItem = new MenuItem ( Catalog.GetString("Repair selected") + " " + myRun.Type + " (" + myRun.PersonName + ")");
		myItem.Activated += on_repair_selected_run_interval_clicked;
		myMenu.Attach( myItem, 0, 1, 1, 2 );
		
		Gtk.SeparatorMenuItem mySep = new SeparatorMenuItem();
		myMenu.Attach( mySep, 0, 1, 2, 3 );

		myItem = new MenuItem ( Catalog.GetString("Delete selected") + " " + myRun.Type + " (" + myRun.PersonName + ")");
		myItem.Activated += on_delete_selected_run_interval_clicked;
		myMenu.Attach( myItem, 0, 1, 3, 4 );

		myMenu.Popup();
		myMenu.ShowAll();
	}

	/* ---------------------------------------------------------
	 * ----------------  TREEVIEW REACTION TIMES ---------------
	 *  --------------------------------------------------------
	 */

	private void createTreeView_reaction_times (Gtk.TreeView tv) {
		//myTreeViewReactionTimes is a TreeViewReactionTimes instance
		myTreeViewReactionTimes = new TreeViewReactionTimes( tv, preferences.digitsNumber, TreeViewEvent.ExpandStates.MINIMIZED);

		//the glade cursor_changed does not work on mono 1.2.5 windows
		tv.CursorChanged += on_treeview_reaction_times_cursor_changed; 
	}

	private void fillTreeView_reaction_times (string filter) {
		fillTreeView_reaction_times (filter, false);
	}
	private void fillTreeView_reaction_times (string filter, bool dbconOpened)
	{
		//do not crash if arrive here with no session
		if(currentSession == null)
			return;

		string [] myRTs = SqliteReactionTime.SelectReactionTimes(dbconOpened, currentSession.UniqueID, -1, "", 
				Sqlite.Orders_by.DEFAULT, -1);

		myTreeViewReactionTimes.Fill(myRTs, filter);
		expandOrMinimizeTreeView((TreeViewEvent) myTreeViewReactionTimes, treeview_reaction_times);
	}
	
	private void on_button_reaction_times_zoom_clicked (object o, EventArgs args) {
		myTreeViewReactionTimes.ExpandState = myTreeViewReactionTimes.ZoomChange(
				myTreeViewReactionTimes.ExpandState);
		if(myTreeViewReactionTimes.ExpandState == TreeViewEvent.ExpandStates.MINIMIZED) {
			treeview_reaction_times.CollapseAll();
			image_reaction_times_zoom.Pixbuf = new Pixbuf(null, Util.GetImagePath(false) + "zoom_in.png");
		} else {
			treeview_reaction_times.ExpandAll();
			image_reaction_times_zoom.Pixbuf = new Pixbuf(null, Util.GetImagePath(false) + "zoom_out.png");
		}
	}
	
	private void treeview_reaction_times_storeReset() {
		myTreeViewReactionTimes.RemoveColumns();
		myTreeViewReactionTimes = new TreeViewReactionTimes( treeview_reaction_times, preferences.digitsNumber, myTreeViewReactionTimes.ExpandState );
	}

	private void on_treeview_reaction_times_cursor_changed (object o, EventArgs args)
	{
		sensitiveLastTestButtons(false);

		// don't select if it's a person, 
		// is for not confusing with the person treeviews that controls who is executing
		if (myTreeViewReactionTimes.EventSelectedID == 0) {
			myTreeViewReactionTimes.Unselect();
			showHideActionEventButtons(false, "ReactionTime");
		} else {
			showHideActionEventButtons(true, "ReactionTime");
		}
	}

	private void treeviewReactionTimesContextMenu(ReactionTime myRt) {
		Menu myMenu = new Menu ();
		Gtk.MenuItem myItem;

		/*
		myItem = new MenuItem ( Catalog.GetString("Play Video") + " " + 
				myRt.Type + " (" + myRt.PersonName + ")");
		if(File.Exists(Util.GetVideoFileName(currentSession.UniqueID, 
				Constants.TestTypes.RT, myTreeViewReactionTimes.EventSelectedID))) {
			myItem.Activated += on_video_play_selected_reaction_time_clicked;
			myItem.Sensitive = true;
		} else 
			myItem.Sensitive = false;
		myMenu.Attach( myItem, 0, 1, 0, 1 );
		*/

		myItem = new MenuItem ( Catalog.GetString("Edit selected") + " " + myRt.Type + " (" + myRt.PersonName + ")");
		myItem.Activated += on_edit_selected_reaction_time_clicked;
		myMenu.Attach( myItem, 0, 1, 0, 1 );

		Gtk.SeparatorMenuItem mySep = new SeparatorMenuItem();
		myMenu.Attach( mySep, 0, 1, 1, 2 );

		myItem = new MenuItem ( Catalog.GetString("Delete selected") + " " + myRt.Type + " (" + myRt.PersonName + ")");
		myItem.Activated += on_delete_selected_reaction_time_clicked;
		myMenu.Attach( myItem, 0, 1, 2, 3 );

		myMenu.Popup();
		myMenu.ShowAll();
	}

	/* ---------------------------------------------------------
	 * ----------------  TREEVIEW PULSES -----------------------
	 *  --------------------------------------------------------
	 */

	private void createTreeView_pulses (Gtk.TreeView tv) {
		//myTreeViewPulses is a TreeViewPulses instance
		myTreeViewPulses = new TreeViewPulses( tv, preferences.digitsNumber, TreeViewEvent.ExpandStates.MINIMIZED );

		//the glade cursor_changed does not work on mono 1.2.5 windows
		tv.CursorChanged += on_treeview_pulses_cursor_changed; 
	}

	private void fillTreeView_pulses (string filter) {
		fillTreeView_pulses (filter, false);
	}
	private void fillTreeView_pulses (string filter, bool dbconOpened)
	{
		//do not crash if arrive here with no session
		if(currentSession == null)
			return;

		string [] myPulses = SqlitePulse.SelectPulses(dbconOpened, currentSession.UniqueID, -1);
		myTreeViewPulses.Fill(myPulses, filter);
		expandOrMinimizeTreeView((TreeViewEvent) myTreeViewPulses, treeview_pulses);
	}
	
	private void on_button_pulses_zoom_clicked (object o, EventArgs args) {
		myTreeViewPulses.ExpandState = myTreeViewPulses.ZoomChange(myTreeViewPulses.ExpandState);
		if(myTreeViewPulses.ExpandState == TreeViewEvent.ExpandStates.MINIMIZED) {
			treeview_pulses.CollapseAll();
			image_pulses_zoom.Pixbuf = new Pixbuf(null, Util.GetImagePath(false) + "zoom_in.png");
		} else if(myTreeViewPulses.ExpandState == TreeViewEvent.ExpandStates.OPTIMAL) {
			treeview_pulses.CollapseAll();
			myTreeViewPulses.ExpandOptimal();
			image_pulses_zoom.Pixbuf = new Pixbuf(null, Util.GetImagePath(false) + "zoom_in.png");
		} else {
			treeview_pulses.ExpandAll();
			image_pulses_zoom.Pixbuf = new Pixbuf(null, Util.GetImagePath(false) + "zoom_out.png");
		}
	}

	private void treeview_pulses_storeReset() {
		myTreeViewPulses.RemoveColumns();
		myTreeViewPulses = new TreeViewPulses( treeview_pulses, preferences.digitsNumber, myTreeViewPulses.ExpandState );
	}

	private void on_treeview_pulses_cursor_changed (object o, EventArgs args)
	{
		sensitiveLastTestButtons(false);

		// don't select if it's a person, 
		// is for not confusing with the person treeviews that controls who is executing
		if (myTreeViewPulses.EventSelectedID == 0) {
			myTreeViewPulses.Unselect();
			showHideActionEventButtons(false, "Pulse");
		} else if (myTreeViewPulses.EventSelectedID == -1) {
			myTreeViewPulses.SelectHeaderLine();
			showHideActionEventButtons(true, "Pulse");
		} else {
			showHideActionEventButtons(true, "Pulse");
		}
	}

	private void treeviewPulsesContextMenu(Pulse myPulse) {
		Menu myMenu = new Menu ();
		Gtk.MenuItem myItem;

		/*
		myItem = new MenuItem ( Catalog.GetString("Play Video") + " " + 
				myPulse.Type + " (" + myPulse.PersonName + ")");
		if(File.Exists(Util.GetVideoFileName(currentSession.UniqueID, 
				Constants.TestTypes.PULSE, myTreeViewPulses.EventSelectedID))) {
			myItem.Activated += on_video_play_selected_pulse_clicked;
			myItem.Sensitive = true;
		} else 
			myItem.Sensitive = false;
		myMenu.Attach( myItem, 0, 1, 0, 1 );
		*/

		myItem = new MenuItem ( Catalog.GetString("Edit selected") + " " + myPulse.Type + " (" + myPulse.PersonName + ")");
		myItem.Activated += on_edit_selected_pulse_clicked;
		myMenu.Attach( myItem, 0, 1, 0, 1 );

		myItem = new MenuItem ( Catalog.GetString("Repair selected") + " " + myPulse.Type + " (" + myPulse.PersonName + ")");
		myItem.Activated += on_repair_selected_pulse_clicked;
		myMenu.Attach( myItem, 0, 1, 1, 2 );
		
		Gtk.SeparatorMenuItem mySep = new SeparatorMenuItem();
		myMenu.Attach( mySep, 0, 1, 2, 3 );

		myItem = new MenuItem ( Catalog.GetString("Delete selected") + " " + myPulse.Type + " (" + myPulse.PersonName + ")");
		myItem.Activated += on_delete_selected_pulse_clicked;
		myMenu.Attach( myItem, 0, 1, 3, 4 );

		myMenu.Popup();
		myMenu.ShowAll();
	}

	/* ---------------------------------------------------------
	 * ----------------  TREEVIEW MULTI CHRONOPIC --------------
	 *  --------------------------------------------------------
	 */

	private void createTreeView_multi_chronopic (bool dbconOpened, Gtk.TreeView tv) {
		//myTreeViewMultiChronopic is a TreeViewMultiChronopic instance
		if(definedSession)
			myTreeViewMultiChronopic = new TreeViewMultiChronopic( tv, preferences.digitsNumber, 
					TreeViewEvent.ExpandStates.MINIMIZED, SqliteMultiChronopic.MaxCPs(dbconOpened, currentSession.UniqueID) );
		else
			myTreeViewMultiChronopic = new TreeViewMultiChronopic( tv, preferences.digitsNumber, 
					TreeViewEvent.ExpandStates.MINIMIZED, 2);

		//the glade cursor_changed does not work on mono 1.2.5 windows
		tv.CursorChanged += on_treeview_multi_chronopic_cursor_changed; 
	}
	
	private void fillTreeView_multi_chronopic () {
		fillTreeView_multi_chronopic (false);
	}
	private void fillTreeView_multi_chronopic (bool dbconOpened)
	{
		//do not crash if arrive here with no session
		if(currentSession == null)
			return;

		string [] mcs = SqliteMultiChronopic.SelectTests(dbconOpened, currentSession.UniqueID, -1);
		myTreeViewMultiChronopic.Fill(mcs, "");
		expandOrMinimizeTreeView((TreeViewEvent) myTreeViewMultiChronopic, treeview_multi_chronopic);
	}
	
	private void on_button_multi_chronopic_zoom_clicked (object o, EventArgs args) {
		myTreeViewMultiChronopic.ExpandState = myTreeViewMultiChronopic.ZoomChange(myTreeViewMultiChronopic.ExpandState);
		if(myTreeViewMultiChronopic.ExpandState == TreeViewEvent.ExpandStates.MINIMIZED) {
			treeview_multi_chronopic.CollapseAll();
			image_multi_chronopic_zoom.Pixbuf = new Pixbuf(null, Util.GetImagePath(false) + "zoom_in.png");
		} else if(myTreeViewMultiChronopic.ExpandState == TreeViewEvent.ExpandStates.OPTIMAL) {
			treeview_multi_chronopic.CollapseAll();
			myTreeViewMultiChronopic.ExpandOptimal();
			image_multi_chronopic_zoom.Pixbuf = new Pixbuf(null, Util.GetImagePath(false) + "zoom_in.png");
		} else {
			treeview_multi_chronopic.ExpandAll();
			image_multi_chronopic_zoom.Pixbuf = new Pixbuf(null, Util.GetImagePath(false) + "zoom_out.png");
		}
	}
	
	private void treeview_multi_chronopic_storeReset(bool dbconOpened) {
		myTreeViewMultiChronopic.RemoveColumns();
		if(definedSession)
			myTreeViewMultiChronopic = new TreeViewMultiChronopic( treeview_multi_chronopic, preferences.digitsNumber, 
					myTreeViewMultiChronopic.ExpandState, SqliteMultiChronopic.MaxCPs(dbconOpened, currentSession.UniqueID) );
		else
			myTreeViewMultiChronopic = new TreeViewMultiChronopic( treeview_multi_chronopic, preferences.digitsNumber, 
					myTreeViewMultiChronopic.ExpandState, 2);
	}

	private void on_treeview_multi_chronopic_cursor_changed (object o, EventArgs args)
	{
		LogB.Information("Cursor changed");
		sensitiveLastTestButtons(false);

		// don't select if it's a person, 
		// is for not confusing with the person treeviews that controls who does events
		if (myTreeViewMultiChronopic.EventSelectedID == 0) {
			myTreeViewMultiChronopic.Unselect();
			showHideActionEventButtons(false, Constants.MultiChronopicName); //hide
		} else if (myTreeViewMultiChronopic.EventSelectedID == -1) {
			myTreeViewMultiChronopic.SelectHeaderLine();
			showHideActionEventButtons(true, Constants.MultiChronopicName);
		} else {
			showHideActionEventButtons(true, Constants.MultiChronopicName); //show
		}
	}

	private void treeviewMultiChronopicContextMenu(MultiChronopic mc) {
		Menu myMenu = new Menu ();
		Gtk.MenuItem myItem;

		/*
		myItem = new MenuItem ( Catalog.GetString("Play Video") + " " + 
				mc.Type + " (" + mc.PersonName + ")");
		if(File.Exists(Util.GetVideoFileName(currentSession.UniqueID, 
				Constants.TestTypes.MULTICHRONOPIC, myTreeViewMultiChronopic.EventSelectedID))) {
			myItem.Activated += on_video_play_selected_multi_chronopic_clicked;
			myItem.Sensitive = true;
		} else 
			myItem.Sensitive = false;
		myMenu.Attach( myItem, 0, 1, 0, 1 );
		*/

		myItem = new MenuItem ( Catalog.GetString("Edit selected") + " " + mc.Type + " (" + mc.PersonName + ")");
		myItem.Activated += on_edit_selected_multi_chronopic_clicked;
		myMenu.Attach( myItem, 0, 1, 0, 1 );

		/*
		myItem = new MenuItem ( Catalog.GetString("Repair selected") + " " + mc.Type + " (" + mc.PersonName + ")");
		myItem.Activated += on_repair_selected_multi_chronopic_clicked;
		myMenu.Attach( myItem, 0, 1, 2, 3 );
		*/
		
		Gtk.SeparatorMenuItem mySep = new SeparatorMenuItem();
		myMenu.Attach( mySep, 0, 1, 1, 2 );

		myItem = new MenuItem ( Catalog.GetString("Delete selected") + " " + mc.Type + " (" + mc.PersonName + ")");
		myItem.Activated += on_delete_selected_multi_chronopic_clicked;
		myMenu.Attach( myItem, 0, 1, 2, 3 );

		myMenu.Popup();
		myMenu.ShowAll();
	}




	/* ---------------------------------------------------------
	 * ----------------  CREATE AND UPDATE COMBOS ---------------
	 *  --------------------------------------------------------
	 */
	
	// ---------------- combo_select ----------------------

	private bool comboSelectContactsTopNoFollow;
	private void createComboSelectContactsTop ()
	{
		//deactivate signal
		if(combo_select_contacts_top != null)
			combo_select_contacts_top.Changed -= new EventHandler (on_combo_select_contacts_top_changed);

		//delete children if any
		if(hbox_combo_select_contacts_top.Children.Length > 0)
			hbox_combo_select_contacts_top.Remove(combo_select_contacts_top);

		//code for each mode
		if(current_mode == Constants.Modes.JUMPSSIMPLE)
		{
			comboSelectContactsTop = new CjComboSelectJumps (combo_select_contacts_top, hbox_combo_select_contacts_top, false);
			combo_select_contacts_top = comboSelectContactsTop.Combo;
			combo_select_contacts_top.Active = combo_select_jumps.Active;
			combo_select_contacts_top.Sensitive = true;
		}
		else if(current_mode == Constants.Modes.JUMPSREACTIVE)
		{
			comboSelectContactsTop = new CjComboSelectJumpsRj (combo_select_contacts_top, hbox_combo_select_contacts_top);
			combo_select_contacts_top = comboSelectContactsTop.Combo;
			combo_select_contacts_top.Active = combo_select_jumps_rj.Active;
			combo_select_contacts_top.Sensitive = true;
		}
		else if(current_mode == Constants.Modes.RUNSSIMPLE)
		{
			comboSelectContactsTop = new CjComboSelectRuns(combo_select_contacts_top, hbox_combo_select_contacts_top);
			combo_select_contacts_top = comboSelectContactsTop.Combo;
			combo_select_contacts_top.Active = combo_select_runs.Active;
			combo_select_contacts_top.Sensitive = true;
		}
		else if(current_mode == Constants.Modes.RUNSINTERVALLIC)
		{
			comboSelectContactsTop = new CjComboSelectRunsI(combo_select_contacts_top, hbox_combo_select_contacts_top);
			combo_select_contacts_top = comboSelectContactsTop.Combo;
			combo_select_contacts_top.Active = combo_select_runs_interval.Active;
			combo_select_contacts_top.Sensitive = true;
		}
		else if(current_mode == Constants.Modes.RUNSENCODER || current_mode == Constants.Modes.FORCESENSOR)
		{
			if(combo_select_contacts_top == null)
				combo_select_contacts_top = ComboBox.NewText ();

			//copy the values form combo_run_encoder_exercise or combo_force_sensor_exercise
			if(current_mode == Constants.Modes.RUNSENCODER)
			{
				UtilGtk.ComboUpdate(combo_select_contacts_top,
						UtilGtk.ComboGetValues (combo_run_encoder_exercise), "");
				combo_select_contacts_top.Active = combo_run_encoder_exercise.Active;
			}
			else //(current_mode == Constants.Modes.FORCESENSOR)
			{
				UtilGtk.ComboUpdate(combo_select_contacts_top,
						UtilGtk.ComboGetValues (combo_force_sensor_exercise), "");
				combo_select_contacts_top.Active = combo_force_sensor_exercise.Active;
			}

			combo_select_contacts_top.Sensitive = true;
			hbox_combo_select_contacts_top.PackStart(combo_select_contacts_top, true, true, 0);
			hbox_combo_select_contacts_top.ShowAll();
		}
		else { //undefined, encoder ...
			/*
			   need to have it created in order to not crash when open Chronojump as encoder,
			   import a session with jumps... data and then combo_select_contacts_top
			   wants to be refreshed but is null. so need to initialize now
			 */
			if(combo_select_contacts_top == null)
				combo_select_contacts_top = ComboBox.NewText ();

			combo_select_contacts_top.Sensitive = true;
			hbox_combo_select_contacts_top.PackStart(combo_select_contacts_top, true, true, 0);
			hbox_combo_select_contacts_top.ShowAll();
		}

		//activate signal
		combo_select_contacts_top.Changed += new EventHandler (on_combo_select_contacts_top_changed);
	}

	private void createComboSelectJumps(bool create) 
	{
		if(create)
		{
			comboSelectJumps = new CjComboSelectJumps(combo_select_jumps, hbox_combo_select_jumps, false);
			combo_select_jumps = comboSelectJumps.Combo;
			combo_select_jumps.Changed += new EventHandler (on_combo_select_jumps_changed);
		} else {
			comboSelectJumps.Fill();
			combo_select_jumps = comboSelectJumps.Combo;
		}
	}
	
	private void createComboSelectJumpsRj(bool create)
	{
		if(create)
		{
			comboSelectJumpsRj = new CjComboSelectJumpsRj(combo_select_jumps_rj, hbox_combo_select_jumps_rj);
			combo_select_jumps_rj = comboSelectJumpsRj.Combo;
			combo_select_jumps_rj.Changed += new EventHandler (on_combo_select_jumps_rj_changed);
		} else {
			comboSelectJumpsRj.Fill();
			combo_select_jumps_rj = comboSelectJumpsRj.Combo;
		}
	}
	
	private void createComboSelectRuns(bool create)
	{
		if(create)
		{
			comboSelectRuns = new CjComboSelectRuns(combo_select_runs, hbox_combo_select_runs);
			combo_select_runs = comboSelectRuns.Combo;
			combo_select_runs.Changed += new EventHandler (on_combo_select_runs_changed);
		} else {
			comboSelectRuns.Fill();
			combo_select_runs = comboSelectRuns.Combo;
		}
	}

	private void createComboSelectRunsInterval(bool create)
	{
		if(create)
		{
			comboSelectRunsI = new CjComboSelectRunsI(combo_select_runs_interval, hbox_combo_select_runs_interval);
			combo_select_runs_interval = comboSelectRunsI.Combo;
			combo_select_runs_interval.Changed += new EventHandler (on_combo_select_runs_interval_changed);
		} else {
			comboSelectRunsI.Fill();
			combo_select_runs_interval = comboSelectRunsI.Combo;
		}
	}

	private void contacts_exercise_left_button (Gtk.ComboBox combo, Gtk.Button button_left, Gtk.Button button_right)
	{
		combo = UtilGtk.ComboSelectPrevious(combo);

		button_left.Sensitive = (combo.Active > 0);
		button_right.Sensitive = true;

		if(current_mode == Constants.Modes.JUMPSSIMPLE || current_mode == Constants.Modes.JUMPSREACTIVE ||
				current_mode == Constants.Modes.RUNSSIMPLE || current_mode == Constants.Modes.RUNSINTERVALLIC ||
				current_mode == Constants.Modes.RUNSENCODER || current_mode == Constants.Modes.FORCESENSOR)
		{
			button_combo_select_contacts_top_left.Sensitive = (combo.Active > 0);
			button_combo_select_contacts_top_right.Sensitive = true;
		}
	}
	private void contacts_exercise_right_button (Gtk.ComboBox combo, Gtk.Button button_left, Gtk.Button button_right)
	{
		bool isLast;
		combo = UtilGtk.ComboSelectNext(combo, out isLast);

		button_left.Sensitive = true;
		button_right.Sensitive = ! isLast;

		if(current_mode == Constants.Modes.JUMPSSIMPLE || current_mode == Constants.Modes.JUMPSREACTIVE ||
				current_mode == Constants.Modes.RUNSSIMPLE || current_mode == Constants.Modes.RUNSINTERVALLIC ||
				current_mode == Constants.Modes.RUNSENCODER || current_mode == Constants.Modes.FORCESENSOR)
		{
			button_combo_select_contacts_top_left.Sensitive = true;
			button_combo_select_contacts_top_right.Sensitive = ! isLast;
		}
	}


	// ---------------- combo_result TODO: delete all ----------------------

	//no need of reationTimes

	/*
	private void createComboPulses() {
		combo_pulses = ComboBox.NewText();
		UtilGtk.ComboUpdate(combo_pulses, SqlitePulseType.SelectPulseTypes(Constants.AllPulsesNameStr(), true), ""); //without filter, only select name
		
		combo_pulses.Active = 0;
		combo_pulses.Changed += new EventHandler (on_combo_pulses_changed);

		hbox_combo_pulses.PackStart(combo_pulses, true, true, 0);
		hbox_combo_pulses.ShowAll();
		combo_pulses.Sensitive = false;
	}
	*/

	/*
	private void createComboMultiChronopic() 
	{
		button_multi_chronopic_start.Sensitive = false;
		button_run_analysis.Sensitive = false;
		extra_window_spin_run_analysis_distance.Sensitive = false;
	}
	*/


	// -------------- combo select tests changed --------

	private void on_combo_select_contacts_top_changed (object o, EventArgs args)
	{
		LogB.Information("on_combo_select_contacts_top_changed");
		if(current_mode == Constants.Modes.JUMPSSIMPLE)
			on_combo_select_jumps_changed(o, args);
		else if(current_mode == Constants.Modes.JUMPSREACTIVE)
			on_combo_select_jumps_rj_changed(o, args);
		else if(current_mode == Constants.Modes.RUNSSIMPLE)
			on_combo_select_runs_changed(o, args);
		else if(current_mode == Constants.Modes.RUNSINTERVALLIC)
			on_combo_select_runs_interval_changed(o, args);
		else if(current_mode == Constants.Modes.RUNSENCODER)
			on_combo_run_encoder_exercise_changed(o, args);
		else if(current_mode == Constants.Modes.FORCESENSOR)
			on_combo_force_sensor_exercise_changed(o, args);
	}

	private void on_combo_select_jumps_changed(object o, EventArgs args)
	{
		LogB.Information("on_combo_select_jumps_changed");
		ComboBox combo = o as ComboBox;
		if (o == null) {
			LogB.Information("o is null");
			return;
		}

		//two combobox are linked ---->
		if(comboSelectContactsTopNoFollow)
		{
			LogB.Information("no follow");
			return;
		}

		comboSelectContactsTopNoFollow = true;
		//LogB.Information("combo_select_contacts_top is null: " + (combo_select_contacts_top == null).ToString());
		if (o == combo_select_jumps)
		{
			LogB.Information("o is combo_select_jumps");
			combo_select_contacts_top.Active = combo_select_jumps.Active;
		}
		else if (o == combo_select_contacts_top)
		{
			LogB.Information("o is combo_select_contacts_top");
			combo_select_jumps.Active = combo_select_contacts_top.Active;
		}
		comboSelectContactsTopNoFollow = false;
		//<---- two combobox are linked

		sensitiveLastTestButtons(false);

		string myText = UtilGtk.ComboGetActive(combo);
		LogB.Information("Selected: " + myText);

		//sensitivity of left/right buttons
		button_combo_jumps_exercise_capture_left.Sensitive = (combo_select_jumps.Active > 0);
		button_combo_jumps_exercise_capture_right.Sensitive = ! UtilGtk.ComboSelectedIsLast(combo_select_jumps);
		button_combo_select_contacts_top_left.Sensitive = (combo_select_jumps.Active > 0);
		button_combo_select_contacts_top_right.Sensitive = ! UtilGtk.ComboSelectedIsLast(combo_select_jumps);

		//show extra window options
		on_extra_window_jumps_test_changed(o, args);

		//update the treeview
		pre_fillTreeView_jumps(false);
	}
	
	private void on_combo_select_jumps_rj_changed(object o, EventArgs args)
	{
		LogB.Information("on_combo_select_jumps_rj_changed");
		ComboBox combo = o as ComboBox;
		if (o == null) {
			LogB.Information("o is null");
			return;
		}

		//two combobox are linked ---->
		if(comboSelectContactsTopNoFollow)
		{
			LogB.Information("no follow");
			return;
		}

		comboSelectContactsTopNoFollow = true;
		if (o == combo_select_jumps_rj)
		{
			LogB.Information("o is combo_select_jumps_rj");
			combo_select_contacts_top.Active = combo_select_jumps_rj.Active;
		}
		else if (o == combo_select_contacts_top)
		{
			LogB.Information("o is combo_select_contacts_top");
			combo_select_jumps_rj.Active = combo_select_contacts_top.Active;
		}
		comboSelectContactsTopNoFollow = false;
		//<---- two combobox are linked

		sensitiveLastTestButtons(false);

		string myText = UtilGtk.ComboGetActive(combo);
		LogB.Information("Selected: " + myText); 

		//sensitivity of left/right buttons
		button_combo_jumps_rj_exercise_capture_left.Sensitive = (combo_select_jumps_rj.Active > 0);
		button_combo_jumps_rj_exercise_capture_right.Sensitive = ! UtilGtk.ComboSelectedIsLast(combo_select_jumps_rj);
		button_combo_select_contacts_top_left.Sensitive = (combo_select_jumps_rj.Active > 0);
		button_combo_select_contacts_top_right.Sensitive = ! UtilGtk.ComboSelectedIsLast(combo_select_jumps_rj);

		//show extra window options
		on_extra_window_jumps_rj_test_changed(o, args);

		//update the treeview
		pre_fillTreeView_jumps_rj(false);
	}
	
	private void on_combo_select_runs_changed(object o, EventArgs args)
	{
		ComboBox combo = o as ComboBox;
		if (o == null)
			return;

		//two combobox are linked ---->
		if(comboSelectContactsTopNoFollow)
			return;

		comboSelectContactsTopNoFollow = true;
		if (o == combo_select_runs)
			combo_select_contacts_top.Active = combo_select_runs.Active;
		else if (o == combo_select_contacts_top)
			combo_select_runs.Active = combo_select_contacts_top.Active;
		comboSelectContactsTopNoFollow = false;
		//<---- two combobox are linked

		sensitiveLastTestButtons(false);

		string myText = UtilGtk.ComboGetActive(combo);
		LogB.Information("Selected: " + myText); 

		//sensitivity of left/right buttons
		button_combo_runs_exercise_capture_left.Sensitive = (combo_select_runs.Active > 0);
		button_combo_runs_exercise_capture_right.Sensitive = ! UtilGtk.ComboSelectedIsLast(combo_select_runs);
		button_combo_select_contacts_top_left.Sensitive = (combo_select_runs.Active > 0);
		button_combo_select_contacts_top_right.Sensitive = ! UtilGtk.ComboSelectedIsLast(combo_select_runs);

		//show extra window options
		on_extra_window_runs_test_changed(o, args);

		//update the treeview
		pre_fillTreeView_runs(false);
	}
	
	private void on_combo_select_runs_interval_changed(object o, EventArgs args)
	{
		ComboBox combo = o as ComboBox;
		if (o == null)
			return;

		//two combobox are linked ---->
		if(comboSelectContactsTopNoFollow)
			return;

		comboSelectContactsTopNoFollow = true;
		if (o == combo_select_runs_interval)
			combo_select_contacts_top.Active = combo_select_runs_interval.Active;
		else if (o == combo_select_contacts_top)
			combo_select_runs_interval.Active = combo_select_contacts_top.Active;
		comboSelectContactsTopNoFollow = false;
		//<---- two combobox are linked

		sensitiveLastTestButtons(false);

		string myText = UtilGtk.ComboGetActive(combo);
		LogB.Information("Selected: " + myText); 

		//sensitivity of left/right buttons
		button_combo_runs_interval_exercise_capture_left.Sensitive = (combo_select_runs_interval.Active > 0);
		button_combo_runs_interval_exercise_capture_right.Sensitive = ! UtilGtk.ComboSelectedIsLast(combo_select_runs_interval);
		button_combo_select_contacts_top_left.Sensitive = (combo_select_runs_interval.Active > 0);
		button_combo_select_contacts_top_right.Sensitive = ! UtilGtk.ComboSelectedIsLast(combo_select_runs_interval);

		//show extra window options
		on_extra_window_runs_interval_test_changed(o, args);

		//update the treeview
		pre_fillTreeView_runs_interval(false);
	}
	
	private void pre_fillTreeView_jumps (bool dbconOpened)
	{
		treeview_jumps_storeReset();

		if(radio_contacts_graph_allTests.Active)
			fillTreeView_jumps(Constants.AllJumpsNameStr(), dbconOpened);
		else if (combo_select_jumps != null)
			fillTreeView_jumps(UtilGtk.ComboGetActive(combo_select_jumps), dbconOpened);
	}

	private void pre_fillTreeView_jumps_rj (bool dbconOpened)
	{
		treeview_jumps_rj_storeReset();

		if(radio_contacts_graph_allTests.Active)
			fillTreeView_jumps_rj(Constants.AllJumpsNameStr(), dbconOpened);
		else if (combo_select_jumps_rj != null)
			fillTreeView_jumps_rj(UtilGtk.ComboGetActive(combo_select_jumps_rj), dbconOpened);
	}

	private void pre_fillTreeView_runs (bool dbconOpened)
	{
		treeview_runs_storeReset();

		if(radio_contacts_graph_allTests.Active)
			fillTreeView_runs(Constants.AllRunsNameStr(), dbconOpened);
		else if (combo_select_runs != null)
			fillTreeView_runs(UtilGtk.ComboGetActive(combo_select_runs), dbconOpened);
	}

	private void pre_fillTreeView_runs_interval (bool dbconOpened)
	{
		treeview_runs_interval_storeReset();

		if(radio_contacts_graph_allTests.Active)
			fillTreeView_runs_interval(Constants.AllRunsNameStr(), dbconOpened);
		else if (combo_select_runs_interval != null)
			fillTreeView_runs_interval(UtilGtk.ComboGetActive(combo_select_runs_interval), dbconOpened);
	}

	// -------------- combo result tests changed. TODO: delete all --------

	//no need of reationTimes because is done in:
	//gui/reactionTime on_extra_window_reaction_times_test_changed()

	/*
	private void on_combo_pulses_changed(object o, EventArgs args)
	{
		//combo_pulses.Changed -= new EventHandler (on_combo_pulses_changed);

		ComboBox combo = o as ComboBox;
		if (o == null)
			return;

		sensitiveLastTestButtons(false);

		string myText = UtilGtk.ComboGetActive(combo);

		treeview_pulses_storeReset();
		fillTreeView_pulses(myText);
	}
	*/


	/* ---------------------------------------------------------
	 * ----------------  MINIMIZE, DELETE EVENT, QUIT  ---------
	 *  --------------------------------------------------------
	 */

	/*
	private void on_button_minimize_clicked (object o, EventArgs args) {
		app1.Iconify();
	}
	*/

	private void on_delete_event (object o, DeleteEventArgs args) {
		args.RetVal = true;

		/*
		//cannot terminate chronojump untile press 'cancel' if  autodetect encoder is working
		if(cpDetect != null && cpDetect.Detecting == true)
		return;
		*/

		on_quit1_activate (new object(), new EventArgs ());
	}


	private void on_quit1_activate (object o, EventArgs args) {
		/*
		if(chronopicCancelledTimes > 0 && UtilAll.IsWindows()) {
			confirmWinJumpRun = ConfirmWindowJumpRun.Show( 
					Catalog.GetString("Attention, current version of Chronojump gets hanged on exit\nif user has cancelled detection of Chronopic."),
					Catalog.GetString("Sorry, you will have to close Chronojump using CTRL + ALT + DEL."));
			confirmWinJumpRun.Button_accept.Clicked += new EventHandler(on_quit2_activate);
		} else
			on_quit2_activate(new object(), new EventArgs());
		
		*/

		bool needConfirmOnExit = false;
		if(needConfirmOnExit)
			notebook_start.CurrentPage = Convert.ToInt32(notebook_start_pages.EXITCONFIRM);
		else
			on_quit2_activate(new object(), new EventArgs());
	}

	private void on_button_exit_cancel_clicked (object o, EventArgs args)
	{
		notebook_start.CurrentPage = Convert.ToInt32(notebook_start_pages.PROGRAM);
	}
	private void on_button_exit_confirm_clicked (object o, EventArgs args)
	{
		on_quit2_activate(new object(), new EventArgs());
	}

	private void on_quit2_activate (object o, EventArgs args)
	{
		LogB.Information("Bye!");

		updatingRestTimes = false;

		//close contacts capture
		if(currentEventExecute != null && currentEventExecute.IsThreadRunning())
		{
			LogB.Information("Closing contacts capture thread...");
			currentEventExecute.Cancel = true;
			LogB.Information("Done!");

			LogB.Information("Closing camera if opened...");
			ExecuteProcess.KillExternalProcess (WebcamFfmpeg.GetExecutableCapture(operatingSystem));
			LogB.Information("Done!");

			//do not need this, above cancelling is enough
			//currentEventExecute.ThreadAbort();
		}

		if(threadRFID != null && threadRFID.IsAlive)
		{
			LogB.Information("Closing threadRFID");

			rfid.Stop();
			rfidProcessCancel = true;

			System.Threading.Thread.Sleep(250);

			if(threadRFID.IsAlive)
			{
				threadRFID.Abort();
			}
		}

		if(threadImport != null && threadImport.IsAlive)
		{
			LogB.Information("Closing threadImport");
			threadImport.Abort();
		}

		if(app1s_threadBackup != null && app1s_threadBackup.IsAlive)
		{
			LogB.Information("Closing app1s_threadBackup");
			app1s_threadBackup.Abort();
		}

		if(app1s_threadExport != null && app1s_threadExport.IsAlive)
		{
			LogB.Information("Closing app1s_threadExport");
			app1s_threadExport.Abort();
		}

		//if capturing on the background finish it
		if(eCaptureInertialBG != null)
			stopCapturingInertialBG();

		cp2016.SerialPortsCloseIfNeeded(true);

		//exit start ping if has not ended
		if(pingThread != null && pingThread.IsAlive)
		{
			LogB.Information("Closing ping thread");
			//pingThread.Abort();
			jsPing.PingAbort();
		}

		//printing remaining logs in the non-gtk thread
		LogB.Information("Printing non-GTK thread remaining log");
		LogB.Information(LogSync.ReadAndEmpty());
	
		try {	
			File.Delete(runningFileName);
		} catch {
			LogB.Information(string.Format(Catalog.GetString("Could not delete file:\n{0}"),
						runningFileName));
		}
		
		if(File.Exists(Util.GetDatabaseTempDir() + System.IO.Path.DirectorySeparatorChar + "chronojump.db"))
			File.Move(Util.GetDatabaseTempDir() + System.IO.Path.DirectorySeparatorChar + "chronojump.db",
				Util.GetDatabaseDir() + System.IO.Path.DirectorySeparatorChar + "chronojump.db");
		
		LogB.Information("Bye2!");

		if(preferences.encoderCaptureInfinite)
			on_button_encoder_capture_finish_cont_clicked(new object(), new EventArgs());
		else
			encoderRProcCapture.SendEndProcess();

		encoderRProcAnalyze.SendEndProcess();

		//cancel force capture process
		if(capturingForce == arduinoCaptureStatus.STARTING || capturingForce == arduinoCaptureStatus.CAPTURING)
		{
			LogB.Information("cancelling force capture");
//			forceProcessCancel = true;
			forceProcessKill = true;
			Thread.Sleep(1500); //wait 1.5s to actually thread can be cancelled
		}
		if(forceOtherThread != null && forceOtherThread.IsAlive)
			forceOtherThread.Abort();
		if(portFSOpened)
			portFS.Close();
		if(photocellWirelessCapture != null && photocellWirelessCapture.PortOpened)
			photocellWirelessCapture.Disconnect();

		//cancel runEncoder capture process
		if(capturingRunEncoder == arduinoCaptureStatus.STARTING || capturingRunEncoder == arduinoCaptureStatus.CAPTURING)
		{
			LogB.Information("cancelling runEncoder capture");
			runEncoderProcessCancel = true;
		}
		if(portREOpened)
			portRE.Close();

		LogB.Information("Updates on SQL");
		//as display stuff can be changed during capture, store at SQL here

		if(preferences.contactsCaptureDisplay.GetInt != preferences.contactsCaptureDisplayStored.GetInt)
			SqlitePreferences.Update(SqlitePreferences.ContactsCaptureDisplayStr,
					preferences.contactsCaptureDisplay.GetInt.ToString(), false);

		if(preferences.encoderCaptureShowOnlyBars.GetInt != preferences.encoderCaptureShowOnlyBarsStored.GetInt)
			SqlitePreferences.Update("encoderCaptureShowOnlyBars",
					preferences.encoderCaptureShowOnlyBars.GetInt.ToString(), false);

		if(preferences.runEncoderCaptureDisplaySimple != preferences.runEncoderCaptureDisplaySimpleStored)
			SqlitePreferences.Update(SqlitePreferences.RunEncoderCaptureDisplaySimple,
					preferences.runEncoderCaptureDisplaySimple.ToString(), false);

		LogB.Information("Bye3!");

		//TODO: if camera is opened close it! Note that this is intended to kill a remaining ffmpeg process
		//but maybe we will kill any ffmpeg instance open by any other possible program on the computer
		//maybe better kill ffmpeg before opening other instance
		//and at end check if it is running that process and kill the last one ffmpeg instance
		//LogB.Information("Bye4!");
		
		Log.End();

		Application.Quit();
		
		//Environment.Exit(Environment.ExitCode);
	}
	
	/* ---------------------------------------------------------
	 * ----------------  SESSION NEW, LOAD, EXPORT, DELETE -----
	 *  --------------------------------------------------------
	 */

	private void on_new_activate (object o, EventArgs args)
	{
		LogB.Information("new session");

		menus_and_mode_sensitive (false);
		app1s_notebook_sup_entered_from = notebook_sup.CurrentPage;
		notebook_sup.CurrentPage = Convert.ToInt32(notebook_sup_pages.SESSION);
		sessionAddEditShow (App1saeModes.ADDSESSION);
	}

	private void setApp1Title(string sessionName, Constants.Modes mode)
	{
		string title = progName;

		if(sessionName != "") {
			title += " - " + sessionName;
			label_current_session.Text = sessionName;
			label_current_session.TooltipText = sessionName;
		} else {
			label_current_session.Text = "----";
			label_current_session.TooltipText = "----";
		}

		if(mode != Constants.Modes.UNDEFINED)
		{
			string modePrint = "";
			if(mode == Constants.Modes.JUMPSSIMPLE)
				modePrint = Catalog.GetString("Jumps simple");
			else if(mode == Constants.Modes.JUMPSREACTIVE)
				modePrint = Catalog.GetString("Jumps multiple");
			else if(mode == Constants.Modes.RUNSSIMPLE)
				modePrint = Catalog.GetString("Races simple");
			else if(mode == Constants.Modes.RUNSINTERVALLIC)
				modePrint = Catalog.GetString("Races intervallic");
			else if(mode == Constants.Modes.RUNSENCODER)
				modePrint = Catalog.GetString("Race analyzer");
			else if(mode == Constants.Modes.POWERGRAVITATORY)
				modePrint = Catalog.GetString("Encoder (gravitatory)");
			else if(mode == Constants.Modes.POWERINERTIAL)
				modePrint = Catalog.GetString("Encoder (inertial)");
			else if(mode == Constants.Modes.FORCESENSOR)
				modePrint = Catalog.GetString("Force sensor");
			else if(mode == Constants.Modes.RT)
				modePrint = Catalog.GetString("Reaction time");
			else if(mode == Constants.Modes.OTHER)
				modePrint = Catalog.GetString("Other");
			else
				modePrint = ""; //should never happen

			if(modePrint != "")
				title += " - " + modePrint;
		}

		if(preferences.debugMode)
			title += " - DEBUG MODE";

		app1.Title = title;
	}
	
	private void on_new_session_accepted ()
	{
		//serverUniqueID is undefined until session is updated
		currentSession.ServerUniqueID = Constants.ServerUndefinedID;

		menus_and_mode_sensitive (true);
		setApp1Title(currentSession.Name, current_mode);

		if(createdStatsWin) {
			stats_win_initializeSession();
		}

		resetAllTreeViews(false, true, false); //fillTests, resetPersons, fillPersons

		//if we are on analyze tab, switch to capture tab
		radio_mode_contacts_capture.Active = true;
		radio_mode_encoder_capture_small.Active = true;

		//show hidden widgets
		sensitiveGuiNoSession();
		sensitiveGuiYesSession();

		sensitiveGuiNoPerson();
		currentPerson = null;
		definedSession = true;

		//for sure, jumpsExists is false, because we create a new session

		hbox_persons_bottom_photo.Sensitive = false;
		vbox_persons_bottom_no_photo.Sensitive = false;
		label_top_person_name.Text = "";
		label_top_encoder_person_name.Text = "";

		//update report
		report.SessionID = currentSession.UniqueID;
		report.StatisticsRemove();
		try {
			reportWin.FillTreeView();
		} catch {} //reportWin is still not created, not need to Fill again

		//feedback (more in 1st session created)
		string feedbackLoadUsers = Catalog.GetString ("Session created, now add or load persons.");
		new DialogMessage(Constants.MessageTypes.INFO, feedbackLoadUsers);

		//fold menu session
		check_menu_session.Click();

		//unfold menu person
		check_manage_persons.Click();

		SqlitePreferences.Update(SqlitePreferences.LastSessionID, currentSession.UniqueID.ToString(), false);
	}
	
	private void on_edit_session_activate (object o, EventArgs args) 
	{
		LogB.Information("edit session");
		
		if(currentSession == null || currentSession.UniqueID == -1) {
			new DialogMessage(Constants.MessageTypes.WARNING, "Cannot edit a missing session");
			return;
		}

		if(currentSession.Name == Constants.SessionSimulatedName)
			new DialogMessage(Constants.MessageTypes.INFO, Constants.SessionProtectedStr());
		else {
			notebook_sup.CurrentPage = Convert.ToInt32(notebook_sup_pages.SESSION);

			sessionAddEditUseSession (currentSession);
			sessionAddEditShow (App1saeModes.EDITCURRENTSESSION);
		}
	}
	
	private void on_edit_session_accepted ()
	{
		setApp1Title(currentSession.Name, current_mode);
		app1s_label_session_set_name();

		if(createdStatsWin) {
			stats_win_initializeSession();
		}
	}

	private void on_open_activate (object o, EventArgs args) 
	{
		LogB.Information("open session");

		//store which page we are on notebook_sup, except if we clicked on "more" from the session tab
		if(notebook_sup.CurrentPage != Convert.ToInt32(notebook_sup_pages.SESSION))
			app1s_notebook_sup_entered_from = notebook_sup.CurrentPage;

		menus_and_mode_sensitive(false);
		sessionLoadWindowShow(app1s_windowType.LOAD_SESSION);
		notebook_sup.CurrentPage = Convert.ToInt32(notebook_sup_pages.SESSION);
	}

	//called from open session OR from gui/networks configInit when config.SessionMode == Config.SessionModeEnum.UNIQUE
	private void on_load_session_accepted () 
	{
		setApp1Title(currentSession.Name, current_mode);
	
		if(createdStatsWin && ! configChronojump.Exhibition) //slow Sqlite calls for Exhibition big data
			stats_win_initializeSession();

		resetAllTreeViews(! configChronojump.Exhibition, true, true); //fillTests, resetPersons, fillPersons

		//if we are on analyze tab, switch to capture tab
		radio_mode_contacts_capture.Active = true;
		radio_mode_encoder_capture_small.Active = true;

		bool foundPersons = false;

		//on Compujump don't start with first person, wait to it's rfid
		if( ! configChronojump.Compujump)
			foundPersons = selectRowTreeView_persons(treeview_persons, 0);

		//show hidden widgets, and sensitivize
		menus_and_mode_sensitive(true);
		sensitiveGuiNoSession();
		sensitiveGuiYesSession();

		definedSession = true;

		//fold menu session
		if(check_menu_session.Active)
			check_menu_session.Click();

		hbox_persons_bottom_photo.Sensitive = false;
		vbox_persons_bottom_no_photo.Sensitive = false;
		LogB.Information("foundPersons: " + foundPersons.ToString());
		//if there are persons
		if(foundPersons) {
			//activate the gui for persons in main window
			sensitiveGuiYesPerson();
			label_top_person_name.Text = "<b>" + currentPerson.Name + "</b>";
			label_top_person_name.UseMarkup = true;
			label_top_encoder_person_name.Text = "<b>" + currentPerson.Name + "</b>";
			label_top_encoder_person_name.UseMarkup = true;
		} else {
			sensitiveGuiNoPerson();
			currentPerson = null;
			label_top_person_name.Text = "";
			label_top_encoder_person_name.Text = "";

			check_manage_persons.Click(); //unfold persons.
		}

		//update report
		report.SessionID = currentSession.UniqueID;
		report.StatisticsRemove();

		if(reportWin != null)
			reportWin.FillTreeView();

		//update other widgets
		//analyze export labels:
		label_sprint_export_data.Text = currentSession.Name;
		label_force_sensor_export_data.Text = currentSession.Name;
		label_run_encoder_export_data.Text = currentSession.Name;

		chronojumpWindowTestsNext();

		SqlitePreferences.Update(SqlitePreferences.LastSessionID, currentSession.UniqueID.ToString(), false);
	}
	
	private void closeSession()
	{
		definedSession = false;
		currentSession = null;
		sensitiveGuiNoSession();

		setApp1Title("", current_mode);
		app1s_label_session_set_name();
	}

	private void reloadSession()
	{
		// If there is an opened session it reloads it. Otherwise it does nothing.

		if (currentSession != null) {
			LogB.Information ("Reloading a session");
			Session openedSession = currentSession;
			closeSession ();
			currentSession = openedSession;
			on_load_session_accepted ();
		} else {
			LogB.Information ("Reload session but no session was opened: doing nothing");
		}
	}

	private OverviewWindow overviewWin;
	private void on_session_overview_clicked (object o, EventArgs args)
	{
		if (currentSession == null || currentPerson == null)
			return;

		Constants.Modes m = current_mode;

		if(m == Constants.Modes.POWERGRAVITATORY || m == Constants.Modes.POWERINERTIAL)
		{
			overviewWin = EncoderOverviewWindow.Show (app1, currentEncoderGI, currentSession.UniqueID, currentPerson.UniqueID);
		}
		else if(m == Constants.Modes.FORCESENSOR)
			overviewWin = ForceSensorOverviewWindow.Show (app1, currentSession.UniqueID, currentPerson.UniqueID);
		else if(m == Constants.Modes.RUNSENCODER)
			overviewWin = RunEncoderOverviewWindow.Show (app1, currentSession.UniqueID, currentPerson.UniqueID);

		overviewWin.Button_select_this_person.Clicked -= new EventHandler(on_overview_select_person);
		overviewWin.Button_select_this_person.Clicked += new EventHandler(on_overview_select_person);
	}

	private void on_overview_select_person (object o, EventArgs args)
	{
		if(overviewWin.SelectedPersonID != -1)
		{
			//LogB.Information("selected: " + overviewWin.SelectedPersonID.ToString());
			selectRowTreeView_persons(treeview_persons,
					myTreeViewPersons.FindRow(overviewWin.SelectedPersonID));

			overviewWin.HideAndNull();
		}
	}

	private void on_export_session_accepted(object o, EventArgs args)
	{
		if(currentSession == null || currentSession.UniqueID == -1) {
			new DialogMessage(Constants.MessageTypes.WARNING, "Cannot edit a missing session");
			return;
		}

		new ExportSessionCSV(currentSession, app1, preferences);
	}



	/* ---------------------------------------------------------
	 * ----------------  SOME CALLBACKS ------------------------
	 *  --------------------------------------------------------
	 */

	//edit
	private void on_cut1_activate (object o, EventArgs args) {
	}
	
	private void on_copy1_activate (object o, EventArgs args) {
	}
	
	private void on_paste1_activate (object o, EventArgs args) {
	}


	private void on_preferences_activate (object o, EventArgs args) 
	{
		if(notebook_sup.CurrentPage == Convert.ToInt32(notebook_sup_pages.START))
			preferencesWin = PreferencesWindow.Show(preferences, Constants.Modes.UNDEFINED, configChronojump.Compujump, progVersion);
		else
			preferencesWin = PreferencesWindow.Show(preferences, current_mode, configChronojump.Compujump, progVersion);

		preferencesWin.FakeButtonConfigurationImported.Clicked += new EventHandler(on_preferences_import_configuration);
		preferencesWin.FakeButtonDebugModeStart.Clicked += new EventHandler(on_preferences_debug_mode_start);
		preferencesWin.Button_accept.Clicked += new EventHandler(on_preferences_accepted);
	}
		
	private void on_preferences_import_configuration (object o, EventArgs args)
	{
		/*
		preferencesWin.FakeButtonConfigurationImported.Clicked -= new EventHandler(on_preferences_import_configuration);
		
		configInit();
		LogB.Information("Initialized configuration");
		*/
	}

	private void on_preferences_accepted (object o, EventArgs args) 
	{
		preferences = preferencesWin.GetPreferences;
		LogB.Mute = preferences.muteLogs;

		if(checkbutton_video_contacts.Active) {
			videoCapturePrepare(false); //if error, show message
		}

		if(configChronojump.Compujump)
		{
			viewport_chronopics.Sensitive = preferences.networksAllowChangeDevices;
			button_activate_chronopics_encoder.Sensitive = preferences.networksAllowChangeDevices;
			button_activate_chronopics_networks_problems.Sensitive = preferences.networksAllowChangeDevices;
		}

		//change language works on windows. On Linux let's change the locale
		//if(UtilAll.IsWindows()) 
		//	languageChange();

		configInitFromPreferences();

		if(repetitiveConditionsWin != null)
		{
			repetitiveConditionsWin.VolumeOn = preferences.volumeOn;
			repetitiveConditionsWin.Gstreamer = preferences.gstreamer;
		}

		try {
			if(createdStatsWin) {
				//statsWin.PrefsDigitsNumber = preferences.digitsNumber;
				//statsWin.WeightStatsPercent = preferences.weightStatsPercent;
				//statsWin.HeightPreferred = preferences.heightPreferred;

				stats_win_fillTreeView_stats(false, true);
			}

			//pass to report
			report.preferences = preferences;
			
			
			createTreeView_jumps (treeview_jumps);
			createTreeView_jumps_rj (treeview_jumps_rj);
			createTreeView_runs (treeview_runs);
			createTreeView_runs_interval (treeview_runs_interval);
			createTreeView_runs_interval_sprint (treeview_runs_interval_sprint);
			createTreeView_pulses(treeview_pulses);
			createTreeView_reaction_times(treeview_reaction_times);
			createTreeView_multi_chronopic(false, treeview_multi_chronopic);

			pre_fillTreeView_jumps(false);
			pre_fillTreeView_jumps_rj(false);
			pre_fillTreeView_runs(false);
			pre_fillTreeView_runs_interval(false);
			//on_combo_pulses_changed(combo_pulses, args);

			//currently no combo_reaction_times
			treeview_reaction_times_storeReset();
			fillTreeView_reaction_times("reactionTime");

			//currently no combo_multi_chronopic
			treeview_multi_chronopic_storeReset(false);
			fillTreeView_multi_chronopic();

			if(current_mode == Constants.Modes.POWERGRAVITATORY){
				label_gravitatory_vpf_propulsive.Visible = preferences.encoderPropulsive;
			}
		}
		catch {
			LogB.Information("catched at on_preferences_accepted ()");
		}

		Config.ColorBackground = preferences.colorBackground;
		Config.ColorBackgroundIsDark = UtilGtk.ColorIsDark(preferences.colorBackground);

		//repaint labels that are on the background
		//TODO: only if color changed or personWinHide
		Config.UseSystemColor = preferences.colorBackgroundOsColor;
		doLabelsContrast(configChronojump.PersonWinHide);

		if(myTreeViewPersons != null)
			myTreeViewPersons.RestSecondsMark = get_configured_rest_time_in_seconds();

		//TODO: only if personWinHide changed
		initialize_menu_or_menu_tiny();

		// ---------- force sensor changes -------------->

		//(check that pen has already been defined)
		if(pen_black_force_capture != null)
			pen_black_force_capture.SetLineAttributes (preferences.forceSensorGraphsLineWidth, Gdk.LineStyle.Solid, Gdk.CapStyle.Round, Gdk.JoinStyle.Round);
		if(pen_black_force_ai != null)
			pen_black_force_ai.SetLineAttributes (preferences.forceSensorGraphsLineWidth, Gdk.LineStyle.Solid, Gdk.CapStyle.Round, Gdk.JoinStyle.Round);

		//TODO: only if have changed
		setForceSensorAnalyzeABSliderIncrements();
		setForceSensorAnalyzeMaxAVGInWindow();

		// update force_capture_drawingarea
		if(current_mode == Constants.Modes.FORCESENSOR)// && radiobutton_force_sensor_analyze_manual.Active)
			forceSensorDoGraphAI(false);

		// <---------- end of force sensor changes --------------
	}


	/*
	 * menu test selectors
	 */

	private void show_start_page()
	{
		notebook_mode_selector.CurrentPage = 0;
		notebook_sup.CurrentPage = Convert.ToInt32(notebook_sup_pages.START);

		//show title
		string tempSessionName = "";
		if(currentSession != null)
			tempSessionName = currentSession.Name;

		setApp1Title(tempSessionName, Constants.Modes.UNDEFINED);
		new ChronojumpLogo (notebook_chronojump_logo, drawingarea_chronojump_logo, viewport_chronojump_logo, preferences.logoAnimatedShow, preferences.fontType.ToString());
	}

	private void on_button_start_selector_show_more_clicked (object o, EventArgs args)
	{
		button_start_selector_show_more.Visible = false;
		button_start_selector_show_less.Visible = true;

		table_start_selector_rt_other.Visible = true;
	}
	private void on_button_start_selector_show_less_clicked (object o, EventArgs args)
	{
		button_start_selector_show_more.Visible = true;
		button_start_selector_show_less.Visible = false;

		table_start_selector_rt_other.Visible = false;
	}

	private void on_button_show_modes_clicked (object o, EventArgs args)
        {
                show_start_page();
        }

	//this will take care on top radios and then call changeMode()
	//is called on start notebook or on start chronojump by networks configuration or by lastMode
	private void changeModeCheckRadios (Constants.Modes m)
	{
		if (m == Constants.Modes.JUMPSSIMPLE)
		{
			if(radio_change_modes_contacts_jumps_simple.Active)
				changeMode (Constants.Modes.JUMPSSIMPLE);
			else
				radio_change_modes_contacts_jumps_simple.Active = true;
		}
		else if (m == Constants.Modes.JUMPSREACTIVE)
		{
			if(radio_change_modes_contacts_jumps_reactive.Active)
				changeMode (Constants.Modes.JUMPSREACTIVE);
			else
				radio_change_modes_contacts_jumps_reactive.Active = true;
		}
		else if (m == Constants.Modes.RUNSSIMPLE)
		{
			if(radio_change_modes_contacts_runs_simple.Active)
				changeMode (Constants.Modes.RUNSSIMPLE);
			else
				radio_change_modes_contacts_runs_simple.Active = true;
		}
		else if (m == Constants.Modes.RUNSINTERVALLIC)
		{
			if(radio_change_modes_contacts_runs_intervallic.Active)
				changeMode (Constants.Modes.RUNSINTERVALLIC);
			else
				radio_change_modes_contacts_runs_intervallic.Active = true;
		}
		else if (m == Constants.Modes.RUNSENCODER)
		{
			if(radio_change_modes_contacts_runs_encoder.Active)
				changeMode (Constants.Modes.RUNSENCODER);
			else
				radio_change_modes_contacts_runs_encoder.Active = true;
		}
		else if (m == Constants.Modes.POWERGRAVITATORY)
		{
			if(radio_change_modes_encoder_gravitatory.Active)
				changeMode (Constants.Modes.POWERGRAVITATORY);
			else
				radio_change_modes_encoder_gravitatory.Active = true;
		}
		else if (m == Constants.Modes.POWERINERTIAL)
		{
			if(radio_change_modes_encoder_inertial.Active)
				changeMode (Constants.Modes.POWERINERTIAL);
			else
				radio_change_modes_encoder_inertial.Active = true;
		}
		else //for modes that do not have radios like forceSensor, RT, other
			changeMode (m);
	}

	private Constants.Modes current_mode;
	private Constants.Modes last_menuitem_mode; //store it to decide not change threshold when change from jumps to jumpsRj
	private bool last_menuitem_mode_defined = false; //undefined when first time entry on a mode (jumps, jumpRj, ...)

	//this is called by above method changeModeCheckRadios or directly by clicking the top radio buttons
	private void changeMode (Constants.Modes m)
	{
		LogB.Information("MODE", m.ToString());
		current_mode = m;

		string tempSessionName = "";
		if(currentSession != null)
			tempSessionName = currentSession.Name;

		setApp1Title(tempSessionName, current_mode);

		//maybe we have the force sensor port opened, close it:
		if(portFSOpened)
			forceSensorDisconnect();

		if(photocellWirelessCapture != null && photocellWirelessCapture.PortOpened)
			photocellWirelessCapture.Disconnect();

		//run simple will be the only one with its drawing are
		button_inspect_last_test_run_simple.Visible = false;

		hbox_other.Visible = false;
		sensitiveLastTestButtons(false);

		//show capture graph and/or table
		if(m != Constants.Modes.POWERGRAVITATORY && m != Constants.Modes.POWERINERTIAL)
		{
			if(m == Constants.Modes.FORCESENSOR || m == Constants.Modes.RUNSENCODER)
			{
				alignment_contacts_show_graph_table.Visible = false;
				//force sensor & race analyzer do not show graph. graphs are on right notebook: notebook_results
				vbox_contacts_capture_graph.Visible = false;
				notebook_results.Visible = true;
			} else {
				alignment_contacts_show_graph_table.Visible = true;
				on_check_contacts_capture_show_modes_clicked (new object(), new EventArgs());
			}
		}

		//cancel force capture process if mode is changed
		if(capturingForce == arduinoCaptureStatus.STARTING || capturingForce == arduinoCaptureStatus.CAPTURING)
		{
			LogB.Information("cancelling force capture");
			forceProcessCancel = true;
		}
		//cancel runEncoder capture process if mode is changed
		if(capturingRunEncoder == arduinoCaptureStatus.STARTING || capturingRunEncoder == arduinoCaptureStatus.CAPTURING)
		{
			LogB.Information("cancelling runEncoder capture");
			runEncoderProcessCancel = true;
		}

		hbox_change_modes_jumps.Visible = false;
		hbox_change_modes_runs.Visible = false;
		radio_change_modes_contacts_force_sensor.Visible = false;

		button_contacts_bells.Sensitive = false;

		radio_mode_contacts_capture.Active = true; //it is safe to change to capture, because analyze has different graphs depending on mode
		radio_mode_encoder_capture_small.Active = true; //it is safe to change to capture, to ensure all widgets are ok on analyze (everything seems ok, but just to have same behaviour than in contacts)

		button_jumps_simple_capture_save_image.Visible = false;
		button_runs_simple_capture_save_image.Visible = false;
		radio_mode_contacts_jumps_profile.Active = true;
		hbox_radio_mode_contacts_analyze_buttons.Visible = false;
		radio_mode_contacts_jumps_rj_fatigue.Visible = false;
		radio_mode_contacts_runs_evolution.Visible = false;
		radio_mode_contacts_sprint.Visible = false;
		hbox_race_analyzer_capture_tab_result_views.Visible = false;
		notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.STATISTICS);
		button_inspect_last_test_run_intervallic.Visible = false;
		alignment_button_force_sensor_adjust.Visible = false;
		button_force_sensor_sync.Visible = false;
		vbox_contacts_load_recalculate.Visible = false;
		button_contacts_exercise_close_and_recalculate.Visible = false;
		vbox_contacts_signal_comment.Visible = false;
		frame_jumps_automatic.Visible = false;
		check_run_show_time.Visible = false;

		hbox_combo_select_contacts_top_with_arrows.Visible = false; //TODO: this will be unneded

		event_execute_label_message.Text = "";
		event_execute_drawingarea.Visible = false;

		hbox_drawingarea_realtime_capture_cairo.Visible = false;
		vbox_event_execute_drawingarea_run_interval_realtime_capture_cairo.Visible = false;

		//blank exercise options: useful for changing from jumps or runs to forceSensor, runEncoder, reaction time, other
		label_contacts_exercise_selected_name.Visible = true; //will not be visible when all the contacts_top combo is implemented
		label_contacts_exercise_selected_options.Text = "";

		//on OSX R is not installed by default. Check if it's installed. Needed for encoder and force sensor
		if(
				( m == Constants.Modes.POWERGRAVITATORY ||
				  m == Constants.Modes.POWERINERTIAL ||
				  m == Constants.Modes.FORCESENSOR ) &&
				operatingSystem == UtilAll.OperatingSystems.MACOSX &&
				! Util.FileExists(Constants.ROSX) )
		{
			new DialogMessage(Constants.MessageTypes.WARNING,
					Catalog.GetString("Sorry, R software is not installed.") +
					"\n" + Catalog.GetString("Please, install it from here:") +
					"\n\n" + Constants.RmacDownload,
					"button_go_r_mac");
			show_start_page();
			return;
		}


		Pixbuf pixbufModeGrid = new Pixbuf (null, Util.GetImagePath(false) + "image_modes.png");
		Pixbuf pixbufModeCurrent;

		if(m == Constants.Modes.JUMPSSIMPLE || m == Constants.Modes.JUMPSREACTIVE)
		{
			button_execute_test_show_connect_or_execute(! cp2016.SuccededConnectContactsRealThread);

			notebook_sup.CurrentPage = Convert.ToInt32(notebook_sup_pages.CONTACTS);
			//notebook_capture_analyze.ShowTabs = true;
			hbox_contacts_sup_capture_analyze_two_buttons.Visible = true;
			alignment_button_threshold.Visible = true;

			label_contacts_exercise_selected_options.Visible = true;
			image_top_laterality_contacts.Visible = false;

			hbox_change_modes_jumps.Visible = true;

			if(m == Constants.Modes.JUMPSSIMPLE) 
			{
				notebooks_change(m);
				on_extra_window_jumps_test_changed(new object(), new EventArgs());

				event_graph_label_graph_test.Visible = false;
				vbox_contacts_simple_graph_controls.Visible = true;
				button_jumps_simple_capture_save_image.Visible = true;

				//align_check_vbox_contacts_graph_legend.Visible = true;
				//vbox_contacts_graph_legend.Visible = false;

				frame_jumps_automatic.Visible = true;
				pixbufModeCurrent = new Pixbuf (null, Util.GetImagePath(false) + "image_jump_simple.png");

				if(radio_mode_contacts_analyze.Active)
					radio_mode_contacts_analyze_buttons_visible (m);
			} else {
				notebooks_change(m);
				button_contacts_bells.Sensitive = true;
				on_extra_window_jumps_rj_test_changed(new object(), new EventArgs());

				event_graph_label_graph_test.Visible = false;
				vbox_contacts_simple_graph_controls.Visible = true;

				hbox_drawingarea_realtime_capture_cairo.Visible = true;

				//align_check_vbox_contacts_graph_legend.Visible = false;
				//vbox_contacts_graph_legend.Visible = false;

				pixbufModeCurrent = new Pixbuf (null, Util.GetImagePath(false) + "image_jump_reactive.png");
			}

			createComboSelectContactsTop ();
			label_contacts_exercise_selected_name.Visible = false;
			hbox_combo_select_contacts_top_with_arrows.Visible = true; //this will be unneded

			/*
			if(radio_mode_contacts_jumps_profile.Active || radio_mode_contacts_jumps_dj_optimal_fall.Active ||
					radio_mode_contacts_jumps_weight_fv_profile.Active || radio_mode_contacts_jumps_evolution.Active)
				radio_mode_contacts_capture.Active = true;
				*/

			if(radio_mode_contacts_analyze.Active)
				radio_mode_contacts_analyze_buttons_visible (m);

			pixbufModeGrid = new Pixbuf (null, Util.GetImagePath(false) + "image_modes_jump.png");
		}
		else if(m == Constants.Modes.RUNSSIMPLE || m == Constants.Modes.RUNSINTERVALLIC)
		{
			if(cp2016.StoredWireless)
				button_execute_test_show_connect_or_execute(false);
			else
				button_execute_test_show_connect_or_execute(! cp2016.SuccededConnectContactsRealThread);

			notebook_sup.CurrentPage = Convert.ToInt32(notebook_sup_pages.CONTACTS);
			//notebook_capture_analyze.ShowTabs = true;
			hbox_contacts_sup_capture_analyze_two_buttons.Visible = true;
			alignment_button_threshold.Visible = true;

			label_contacts_exercise_selected_options.Visible = true;
			image_top_laterality_contacts.Visible = false;

			hbox_change_modes_runs.Visible = true;
			check_run_show_time.Visible = true;

			if(m == Constants.Modes.RUNSSIMPLE) 
			{
				notebooks_change(m);
				on_extra_window_runs_test_changed(new object(), new EventArgs());

				event_graph_label_graph_test.Visible = false;
				vbox_contacts_simple_graph_controls.Visible = true;
				button_runs_simple_capture_save_image.Visible = true;

				//align_check_vbox_contacts_graph_legend.Visible = true;
				//vbox_contacts_graph_legend.Visible = false;

				button_inspect_last_test_run_simple.Visible = true;
				pixbufModeCurrent = new Pixbuf (null, Util.GetImagePath(false) + "image_run_simple.png");
			}
			else
			{
				notebooks_change(m);
				button_contacts_bells.Sensitive = true;
				on_extra_window_runs_interval_test_changed(new object(), new EventArgs());
				event_execute_drawingarea_realtime_capture_cairo.Visible = true;
				button_inspect_last_test_run_intervallic.Visible = true;

				event_graph_label_graph_test.Visible = false;
				vbox_contacts_simple_graph_controls.Visible = true;

				hbox_drawingarea_realtime_capture_cairo.Visible = true;
				vbox_event_execute_drawingarea_run_interval_realtime_capture_cairo.Visible = true;

				//align_check_vbox_contacts_graph_legend.Visible = false;
				//vbox_contacts_graph_legend.Visible = false;

				createTreeView_runs_interval_sprint (treeview_runs_interval_sprint);
				pixbufModeCurrent = new Pixbuf (null, Util.GetImagePath(false) + "image_run_multiple.png");

				if(radio_mode_contacts_analyze.Active)
					radio_mode_contacts_analyze_buttons_visible (m);
			}

			createComboSelectContactsTop ();
			label_contacts_exercise_selected_name.Visible = false;
			hbox_combo_select_contacts_top_with_arrows.Visible = true; //this will be unneded


			pixbufModeGrid = new Pixbuf (null, Util.GetImagePath(false) + "image_modes_run.png");
		}
		else if(m == Constants.Modes.POWERGRAVITATORY || m == Constants.Modes.POWERINERTIAL) 
		{
			notebook_sup.CurrentPage = Convert.ToInt32(notebook_sup_pages.ENCODER);


			/*
			 * If there's a signal on gravitatory and we move to inertial, 
			 * interface has to change to YESPERSON (meaning no_signal).
			 * But, if there's no person shoud continue on NOPERSON
			 */
			if(currentPerson != null &&
					selectRowTreeView_persons(treeview_persons, myTreeViewPersons.FindRow(currentPerson.UniqueID)))
				encoderButtonsSensitive(encoderSensEnum.YESPERSON);
			
			blankEncoderInterface();

			//combos should show encoder exercises of current type (encoderGI)
			createEncoderComboExerciseAndAnalyze();

			/*
			   only needed if change from grav analyze to inertial analyze (or viceversa) directly.
			   But it is disabled because on change mode chronojump goes to capture.
			//updateEncoderAnalyzeExercisesPre();
			*/

			bool changed = false;
			if(m == Constants.Modes.POWERGRAVITATORY)
			{
				//change encoderConfigurationCurrent if needed
				if(encoderConfigurationCurrent.has_inertia)
				{
					EncoderConfigurationSQLObject econfSO = SqliteEncoderConfiguration.SelectActive(Constants.EncoderGI.GRAVITATORY);
					encoderConfigurationCurrent = econfSO.encoderConfiguration;
					label_encoder_selected.Text = econfSO.name;
					label_encoder_top_selected.Text = econfSO.name;
					setEncoderTypePixbuf();

					changed = true;
				}
				
				currentEncoderGI = Constants.EncoderGI.GRAVITATORY;
				hbox_capture_1RM.Visible = true;

				//notebook_encoder_capture_extra_mass.CurrentPage = 0;
				//TODO: show also info on the top
				label_button_encoder_select.Text = Catalog.GetString("Configure gravitatory encoder");
				label_encoder_exercise_mass.Visible = true;
				vbox_encoder_exercise_mass.Visible = true;
				label_encoder_exercise_inertia.Visible = false;
				vbox_encoder_exercise_inertia.Visible = false;
				pixbufModeCurrent = new Pixbuf (null, Util.GetImagePath(false) + "image_weight.png");

				if(radio_encoder_analyze_individual_current_set.Active || radio_encoder_analyze_individual_current_session.Active)
				{
					radiobutton_encoder_analyze_1RM.Visible = true;
					if(radiobutton_encoder_analyze_1RM.Active)
						hbox_combo_encoder_analyze_1RM.Visible=true;
					radiobutton_encoder_analyze_neuromuscular_profile.Visible = true;
				}
				//hbox_encoder_capture_1_or_cont.Visible = true;
				vbox_angle_now.Visible = false;
				label_gravitatory_vpf_propulsive.Visible = preferences.encoderPropulsive;

				notebook_encoder_top.Page = 0;
			} else {
				//change encoderConfigurationCurrent if needed
				if(! encoderConfigurationCurrent.has_inertia)
				{
					EncoderConfigurationSQLObject econfSO = SqliteEncoderConfiguration.SelectActive(Constants.EncoderGI.INERTIAL);
					encoderConfigurationCurrent = econfSO.encoderConfiguration;
					label_encoder_selected.Text = econfSO.name;
					label_encoder_top_selected.Text = econfSO.name;
					setEncoderTypePixbuf();

					changed = true;
				}
				
				currentEncoderGI = Constants.EncoderGI.INERTIAL;
				hbox_capture_1RM.Visible = false;

				//notebook_encoder_capture_extra_mass.CurrentPage = 1;
				//TODO: show also info on the top
				label_button_encoder_select.Text = Catalog.GetString("Configure inertial encoder");
				label_encoder_exercise_mass.Visible = false;
				vbox_encoder_exercise_mass.Visible = false;
				label_encoder_exercise_inertia.Visible = true;
				vbox_encoder_exercise_inertia.Visible = true;

				radiobutton_encoder_analyze_1RM.Visible = false;
				hbox_combo_encoder_analyze_1RM.Visible=false;
				radiobutton_encoder_analyze_neuromuscular_profile.Visible = false;
				
				vbox_angle_now.Visible = true;
				label_gravitatory_vpf_propulsive.Visible = false;

				notebook_encoder_top.Page = 1;
				pixbufModeCurrent = new Pixbuf (null, Util.GetImagePath(false) + "image_inertia.png");
			}
			encoderGuiChangesAfterEncoderConfigurationWin(true);
			if(changed) {
				prepareAnalyzeRepetitions ();
			}

			if(! encoderPreferencesSet)
			{
				setEncoderExerciseOptionsFromPreferences();
				encoderPreferencesSet = true;
			}

			pixbufModeGrid = new Pixbuf (null, Util.GetImagePath(false) + "image_modes_encoder.png");
		} 
		else if(m == Constants.Modes.FORCESENSOR)
		{
			button_execute_test_show_connect_or_execute(false);

			notebook_sup.CurrentPage = Convert.ToInt32(notebook_sup_pages.CONTACTS);
			notebooks_change(m);

			vbox_contacts_load_recalculate.Visible = true;
			button_contacts_exercise_close_and_recalculate.Visible = true;

			vbox_contacts_signal_comment.Visible = true;
			vbox_contacts_signal_comment.SetSizeRequest(
					button_contacts_signal_save_comment.SizeRequest().Width, -1);

			button_contacts_capture_load.Sensitive = myTreeViewPersons.IsThereAnyRecord();
			button_contacts_capture_session_overview.Sensitive = myTreeViewPersons.IsThereAnyRecord();

			button_contacts_bells.Sensitive = true;
			//notebook_capture_analyze.ShowTabs = false; //only capture tab is shown (only valid for "OTHER" tests)
			hbox_contacts_sup_capture_analyze_two_buttons.Visible = true;
			alignment_button_threshold.Visible = false;
			alignment_button_force_sensor_adjust.Visible = true;
			//button_force_sensor_sync.Visible = true; //TODO: show again when it fully works, now is hidden for 2.1.0 release
			//notebook_capture_analyze.GetNthPage(2).Hide(); //hide jumpsProfile on other tests

			event_graph_label_graph_test.Visible = true;
			vbox_contacts_simple_graph_controls.Visible = false;

			radio_change_modes_contacts_force_sensor.Visible = true;
			//align_check_vbox_contacts_graph_legend.Visible = false;
			//vbox_contacts_graph_legend.Visible = false;

			setLabelContactsExerciseSelected(m);
			//better use the followin so we will have the Elastic/not elastic display on mode change
			on_combo_force_sensor_exercise_changed (new object(), new EventArgs ());
			//setLabelContactsExerciseSelectedOptionsForceSensor();

			label_contacts_exercise_selected_options.Visible = true;
			image_top_laterality_contacts.Visible = true;
			setForceSensorLateralityPixbuf();

			createComboSelectContactsTop ();
			label_contacts_exercise_selected_name.Visible = false;
			hbox_combo_select_contacts_top_with_arrows.Visible = true; //this will be unneded

			pixbufModeCurrent = new Pixbuf (null, Util.GetImagePath(false) + "force_sensor_icon.png");
			pixbufModeGrid = new Pixbuf (null, Util.GetImagePath(false) + "image_modes_force.png");
		}
		else if(m == Constants.Modes.RUNSENCODER)
		{
			button_execute_test_show_connect_or_execute(false);

			notebook_sup.CurrentPage = Convert.ToInt32(notebook_sup_pages.CONTACTS);
			notebooks_change(m);

			vbox_contacts_load_recalculate.Visible = true;
			button_contacts_exercise_close_and_recalculate.Visible = true;

			vbox_contacts_signal_comment.Visible = true;
			vbox_contacts_signal_comment.SetSizeRequest(
					button_contacts_signal_save_comment.SizeRequest().Width, -1);

			button_contacts_capture_load.Sensitive = myTreeViewPersons.IsThereAnyRecord();
			button_contacts_capture_session_overview.Sensitive = myTreeViewPersons.IsThereAnyRecord();

			//notebook_capture_analyze.ShowTabs = false; //only capture tab is shown (only valid for "OTHER" tests)
			hbox_contacts_sup_capture_analyze_two_buttons.Visible = true;
			alignment_button_threshold.Visible = false;
			//notebook_capture_analyze.GetNthPage(2).Hide(); //hide jumpsProfile on other tests

			hbox_race_analyzer_capture_tab_result_views.Visible = true;
			hbox_change_modes_runs.Visible = true;

			//align_check_vbox_contacts_graph_legend.Visible = false;
			//vbox_contacts_graph_legend.Visible = false;

			combo_race_analyzer_device.Active = 0;
			forceSensorImageTestChange();
			setLabelContactsExerciseSelected(m);

			label_contacts_exercise_selected_options.Visible = true;
			image_top_laterality_contacts.Visible = false;

			createComboSelectContactsTop ();
			label_contacts_exercise_selected_name.Visible = false;
			hbox_combo_select_contacts_top_with_arrows.Visible = true; //this will be unneded

			pixbufModeCurrent = new Pixbuf (null, Util.GetImagePath(false) + "race_encoder_icon.png");
			pixbufModeGrid = new Pixbuf (null, Util.GetImagePath(false) + "image_modes_run.png");
		}
		else if(m == Constants.Modes.RT)
		{
			event_execute_drawingarea.Visible = true;
			button_execute_test_show_connect_or_execute(! cp2016.SuccededConnectContactsRealThread);

			notebook_sup.CurrentPage = Convert.ToInt32(notebook_sup_pages.CONTACTS);
			notebooks_change(m);
			on_extra_window_reaction_times_test_changed(new object(), new EventArgs());

			notebook_capture_analyze.CurrentPage = 0;
			//notebook_capture_analyze.ShowTabs = false; //only capture tab is shown (only valid for "OTHER" tests)
			hbox_contacts_sup_capture_analyze_two_buttons.Visible = false;
			alignment_button_threshold.Visible = true;
			//notebook_capture_analyze.GetNthPage(2).Hide(); //hide jumpsProfile on other tests

			label_contacts_exercise_selected_options.Visible = true;
			image_top_laterality_contacts.Visible = false;

			pixbufModeCurrent = new Pixbuf (null, Util.GetImagePath(false) + "reaction_time_icon.png");
			pixbufModeGrid = new Pixbuf (null, Util.GetImagePath(false) + "image_modes_rt.png");
		}
		else {	//m == Constants.Modes.OTHER (contacts / other)
			event_execute_drawingarea.Visible = true;
			button_execute_test_show_connect_or_execute(! cp2016.SuccededConnectContactsRealThread);

			notebook_sup.CurrentPage = Convert.ToInt32(notebook_sup_pages.CONTACTS);
			hbox_other.Visible = true;
			notebooks_change(m);
			if(radio_mode_pulses_small.Active)
				on_extra_window_pulses_test_changed(new object(), new EventArgs());
			else
				on_extra_window_multichronopic_test_changed(new object(), new EventArgs());

			notebook_capture_analyze.CurrentPage = 0;
			//notebook_capture_analyze.ShowTabs = false; //only capture tab is shown (only valid for "OTHER" tests)
			hbox_contacts_sup_capture_analyze_two_buttons.Visible = false;
			alignment_button_threshold.Visible = true;
			//notebook_capture_analyze.GetNthPage(2).Hide(); //hide jumpsProfile on other tests

			label_contacts_exercise_selected_options.Visible = true;
			image_top_laterality_contacts.Visible = false;

			pixbufModeCurrent = new Pixbuf (null, Util.GetImagePath(false) + "other_icon.png");
			pixbufModeGrid = new Pixbuf (null, Util.GetImagePath(false) + "image_modes_other.png");
		}

		image_button_show_modes_contacts_grid.Pixbuf = pixbufModeGrid;
		image_button_show_modes_encoder_grid.Pixbuf = pixbufModeGrid;

		if(m == Constants.Modes.POWERGRAVITATORY || m == Constants.Modes.POWERINERTIAL)
			image_encoder_exercise.Pixbuf = pixbufModeCurrent;

		//show feedback icon
		Pixbuf pixbufBellActive = new Pixbuf (null, Util.GetImagePath(false) + "stock_bell_active.png");
		Pixbuf pixbufBellInactive = new Pixbuf (null, Util.GetImagePath(false) + "stock_bell_none.png");
		if(
				( (m == Constants.Modes.JUMPSSIMPLE || m == Constants.Modes.JUMPSREACTIVE) &&
				  repetitiveConditionsWin.FeedbackActive(Constants.BellModes.JUMPS)) ||
				( (m == Constants.Modes.RUNSSIMPLE || m == Constants.Modes.RUNSINTERVALLIC) &&
				  repetitiveConditionsWin.FeedbackActive(Constants.BellModes.RUNS)) ||
				( m == Constants.Modes.FORCESENSOR &&
				  repetitiveConditionsWin.FeedbackActive(Constants.BellModes.FORCESENSOR)) )
			image_contacts_bell.Pixbuf = pixbufBellActive;
		else
			image_contacts_bell.Pixbuf = pixbufBellInactive;

		if( (m == Constants.Modes.POWERGRAVITATORY || m == Constants.Modes.POWERINERTIAL)
				&&  repetitiveConditionsWin.FeedbackActive(Constants.BellModes.ENCODERGRAVITATORY) )
			image_encoder_bell.Pixbuf = pixbufBellActive;
		else
			image_encoder_bell.Pixbuf = pixbufBellInactive;


		//show the program
		notebook_start.CurrentPage = Convert.ToInt32(notebook_start_pages.PROGRAM);

		if(m != Constants.Modes.POWERGRAVITATORY && m != Constants.Modes.POWERINERTIAL)
		{
			//don't change threshold if changing from jumpssimple to jumpsreactive ...
			if(! last_menuitem_mode_defined ||
					( m == Constants.Modes.JUMPSSIMPLE &&
					  last_menuitem_mode != Constants.Modes.JUMPSREACTIVE ) ||
					( m == Constants.Modes.JUMPSREACTIVE &&
					  last_menuitem_mode != Constants.Modes.JUMPSSIMPLE ) ||
					( m == Constants.Modes.RUNSSIMPLE &&
					  last_menuitem_mode != Constants.Modes.RUNSINTERVALLIC ) ||
					( m == Constants.Modes.RUNSINTERVALLIC &&
					  last_menuitem_mode != Constants.Modes.RUNSSIMPLE ) ||
					m == Constants.Modes.RT || m == Constants.Modes.OTHER )
			{
				if(threshold.SelectTresholdForThisMode(m))
				{
					label_threshold.Text = //Catalog.GetString("Threshold") + " " +
						threshold.GetLabel() + " ms";

					last_menuitem_mode = m;
				}
			}
		}

		//on capture, show phases, time, record if we are not on forcesensor mode
		showHideCaptureSpecificControls (m);

		last_menuitem_mode_defined = true;

		SqlitePreferences.Update(SqlitePreferences.LastMode, m.ToString(), false);

		chronopicRegisterUpdate(false);

		chronojumpWindowTestsNext();

		setLabelContactsExerciseSelectedOptions();

	}

	private void on_check_contacts_capture_show_modes_clicked (object o, EventArgs args)
	{
		if(! followSignals) //TODO: check if has to be this boolean
			return;

		vbox_contacts_capture_graph.Visible = check_contacts_capture_graph.Active;
		notebook_results.Visible = check_contacts_capture_table.Active;

		//when showing both widgets, start at the middle
		if(vbox_contacts_capture_graph.Visible && notebook_results.Visible)
			hpaned_contacts_graph_table.Position = Convert.ToInt32(frame_contacts_graph_table.Allocation.Width / 2.0);

		if (check_contacts_capture_graph.Active || check_contacts_capture_table.Active)
			hbox_contacts_capture_show_need_one.Visible = false;
		else
			hbox_contacts_capture_show_need_one.Visible = true;

		/*
		   update the preferences variable
		   note as can be changed while capturing, it will be saved to SQL on exit
		   to not have problems with SQL while capturing
		   */
		preferences.contactsCaptureDisplay = new ContactsCaptureDisplay(
				check_contacts_capture_table.Active,
				check_contacts_capture_graph.Active);
	}

	private void radio_mode_contacts_analyze_buttons_visible (Constants.Modes m)
	{
		if(m == Constants.Modes.JUMPSSIMPLE)
		{
			hbox_radio_mode_contacts_analyze_buttons.Visible = true;
			hbox_radio_mode_contacts_analyze_jump_simple_buttons.Visible = true;
			radio_mode_contacts_jumps_rj_fatigue.Visible = false;
			radio_mode_contacts_runs_evolution.Visible = false;
			radio_mode_contacts_sprint.Visible = false;
		}
		else if(m == Constants.Modes.JUMPSREACTIVE)
		{
			hbox_radio_mode_contacts_analyze_buttons.Visible = true;
			hbox_radio_mode_contacts_analyze_jump_simple_buttons.Visible = false;
			radio_mode_contacts_jumps_rj_fatigue.Visible = true;
			radio_mode_contacts_runs_evolution.Visible = false;
			radio_mode_contacts_sprint.Visible = false;

			radio_mode_contacts_jumps_rj_fatigue.Active = true;
		}
		else if(m == Constants.Modes.RUNSSIMPLE)
		{
			hbox_radio_mode_contacts_analyze_buttons.Visible = true;
			hbox_radio_mode_contacts_analyze_jump_simple_buttons.Visible = false;
			radio_mode_contacts_jumps_rj_fatigue.Visible = false;
			radio_mode_contacts_runs_evolution.Visible = true;
			radio_mode_contacts_sprint.Visible = false;

			radio_mode_contacts_runs_evolution.Active = true;
		}
		else if(m == Constants.Modes.RUNSINTERVALLIC)
		{
			hbox_radio_mode_contacts_analyze_buttons.Visible = true;
			hbox_radio_mode_contacts_analyze_jump_simple_buttons.Visible = false;
			radio_mode_contacts_jumps_rj_fatigue.Visible = false;
			radio_mode_contacts_runs_evolution.Visible = false;
			radio_mode_contacts_sprint.Visible = true;

			radio_mode_contacts_sprint.Active = true;
		}
	}

	private void showHideCaptureSpecificControls(Constants.Modes m)
	{
		hbox_capture_phases_time.Visible = (m != Constants.Modes.FORCESENSOR && m != Constants.Modes.RUNSENCODER);

		if(! configChronojump.Compujump)
			showWebcamCaptureContactsControls(true);
	}

	void setEncoderTypePixbuf()
	{
		Pixbuf pixbuf;
		if(encoderConfigurationCurrent.type == Constants.EncoderType.LINEAR)
			pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "encoder-linear-blue.png");
		else if(encoderConfigurationCurrent.type == Constants.EncoderType.ROTARYFRICTION)
			pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "encoder-rotary-friction-blue.png");
		else // if(encoderConfigurationCurrent.type == Constants.EncoderType.ROTARYAXIS)
			pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "encoder-rotary-axis-blue.png");

		image_encoder_top_selected_type.Pixbuf = pixbuf;
		image_encoder_selected_type.Pixbuf = pixbuf;
	}

	/*
	ChronopicDetect cpDetect;
	private void autoDetectChronopic(Constants.Modes m)
	{
		if(m == Constants.Modes.POWERGRAVITATORY || m == Constants.Modes.POWERINERTIAL) 
		{
			hbox_chronopic_encoder_detecting.Visible = true;
			viewport_chronopic_encoder.Visible = false;

			cpDetect = new ChronopicDetect(
					chronopicWin.SP,
					progressbar_chronopic_encoder_detecting, 
					button_chronopic_encoder_detecting_cancel,
					button_chronopic_encoder_detecting_info,
					configAutodetectPort
					);
			
			cpDetect.Detect("ENCODER");

			cpDetect.FakeButtonDone.Clicked += new EventHandler(on_autoDetectChronopic_encoder_done);
		} 
		else {
			//disabled on Windows until is fixed //TODO
			if(UtilAll.IsWindows()) {
				return;
			}

			hbox_chronopic_detecting.Visible = true;
			viewport_chronopics.Visible = false;

			cpDetect = new ChronopicDetect(
					chronopicWin.SP,
					progressbar_chronopic_detecting, 
					button_chronopic_detecting_cancel,
					button_chronopic_detecting_info,
					configAutodetectPort
					);
			
			cpDetect.Detect("NORMAL");

			cpDetect.FakeButtonDone.Clicked += new EventHandler(on_autoDetectChronopic_normal_done);
		}
	}
	private void on_autoDetectChronopic_encoder_done(object o, EventArgs args) 
	{
		cpDetect.FakeButtonDone.Clicked -= new EventHandler(on_autoDetectChronopic_encoder_done);
			
		hbox_chronopic_encoder_detecting.Visible = false;
		viewport_chronopic_encoder.Visible = true;
		
		string str = cpDetect.Detected;

		if(str != null && str != "") {
			LogB.Information("Detected at port: " + str);
			createChronopicWindow(true, str);
		}
		else {
			LogB.Information("Not detected.");
			createChronopicWindow(true, Util.GetDefaultPort());
		}
	
		on_autoDetectChronopic_all_done();
	}
	private void on_autoDetectChronopic_normal_done(object o, EventArgs args) 
	{
		cpDetect.FakeButtonDone.Clicked -= new EventHandler(on_autoDetectChronopic_normal_done);
			
		hbox_chronopic_detecting.Visible = false;
		viewport_chronopics.Visible = true;
	
		string str = cpDetect.Detected;

		if(str != null && str != "") {
			LogB.Information("Detected at port: " + str);

			//set connected stuff for chronopicWin
			chronopicWin.Connected = true;
		
			//set cpd for chronopicWin
			ChronopicPortData cpd = new ChronopicPortData(1, str, true);
			ArrayList cpdArray = new ArrayList();
			cpdArray.Add(cpd);
			
			LogB.Debug("chronopicWin is null? " + (chronopicWin == null).ToString());
			LogB.Debug("chronopicWin.CP is null? " + (chronopicWin.CP == null).ToString());
			
			createChronopicWindow(cpDetect.getCP(), cpdArray, true, str);
			
			LogB.Debug("chronopicWin.CP is null? " + (chronopicWin.CP == null).ToString());
		
			change_multitest_firmware(getMenuItemMode());
		}
		else {
			LogB.Information("Not detected.");
			createChronopicWindow(true, Util.GetDefaultPort());
		}
	
		on_autoDetectChronopic_all_done();
	}
	private void on_autoDetectChronopic_all_done() 
	{
		//main_menu.Sensitive = true;
	}
	*/
		


	private void on_button_selector_start_jumps_clicked(object o, EventArgs args) 
	{
		notebook_mode_selector2.CurrentPage = 0; //jumps
		notebook_mode_selector.CurrentPage = 1; //2nd selector
	}

	private void on_button_selector_start_jumps_simple_clicked(object o, EventArgs args) 
	{
		changeModeCheckRadios (Constants.Modes.JUMPSSIMPLE);
	}
	private void on_button_selector_start_jumps_reactive_clicked(object o, EventArgs args) 
	{
		changeModeCheckRadios (Constants.Modes.JUMPSREACTIVE);
	}
	private void on_radio_change_modes_contacts_jumps_simple_toggled (object o, EventArgs args)
	{
		if(radio_change_modes_contacts_jumps_simple.Active)
			changeMode (Constants.Modes.JUMPSSIMPLE);
	}
	private void on_radio_change_modes_contacts_jumps_reactive_toggled (object o, EventArgs args)
	{
		if(radio_change_modes_contacts_jumps_reactive.Active)
			changeMode (Constants.Modes.JUMPSREACTIVE);
	}
	
	private void on_button_selector_start_runs_clicked(object o, EventArgs args) 
	{
		notebook_mode_selector2.CurrentPage = 1; //runs
		notebook_mode_selector.CurrentPage = 1; //2nd selector
	}
	private void on_button_selector_start_runs_photocell_clicked(object o, EventArgs args)
	{
		notebook_mode_selector2.CurrentPage = 2; //runs photocell
		notebook_mode_selector.CurrentPage = 1; //2nd selector
	}
	private void on_button_selector_start_runs_simple_clicked(object o, EventArgs args)
	{
		changeModeCheckRadios (Constants.Modes.RUNSSIMPLE);
	}
	private void on_button_selector_start_runs_intervallic_clicked(object o, EventArgs args) 
	{
		changeModeCheckRadios (Constants.Modes.RUNSINTERVALLIC);
	}
	private void on_button_selector_start_race_encoder_clicked(object o, EventArgs args)
	{
		changeModeCheckRadios (Constants.Modes.RUNSENCODER);
	}
	private void on_radio_change_modes_contacts_runs_simple_toggled (object o, EventArgs args)
	{
		if(radio_change_modes_contacts_runs_simple.Active)
			changeMode (Constants.Modes.RUNSSIMPLE);
	}
	private void on_radio_change_modes_contacts_runs_intervallic_toggled (object o, EventArgs args)
	{
		if(radio_change_modes_contacts_runs_intervallic.Active)
			changeMode (Constants.Modes.RUNSINTERVALLIC);
	}
	private void on_radio_change_modes_contacts_runs_encoder_toggled (object o, EventArgs args)
	{
		if(radio_change_modes_contacts_runs_encoder.Active)
			changeMode (Constants.Modes.RUNSENCODER);
	}
	
	private void on_button_selector_start_encoder_clicked(object o, EventArgs args) 
	{
		notebook_mode_selector2.CurrentPage = 3; //encoder
		notebook_mode_selector.CurrentPage = 1; //2nd selector
	}

	private void on_button_selector_start_encoder_gravitatory_clicked(object o, EventArgs args) 
	{
		changeModeCheckRadios (Constants.Modes.POWERGRAVITATORY);
	}
	private void on_button_selector_start_encoder_inertial_clicked(object o, EventArgs args) 
	{
		changeModeCheckRadios (Constants.Modes.POWERINERTIAL);
	}
	private void on_radio_change_modes_encoder_gravitatory_toggled (object o, EventArgs args)
	{
		if(radio_change_modes_encoder_gravitatory.Active)
			changeMode (Constants.Modes.POWERGRAVITATORY);
	}
	private void on_radio_change_modes_encoder_inertial_toggled (object o, EventArgs args)
	{
		if(radio_change_modes_encoder_inertial.Active)
			changeMode (Constants.Modes.POWERINERTIAL);
	}

	private void on_button_selector_start_force_sensor_clicked(object o, EventArgs args)
	{
		changeMode (Constants.Modes.FORCESENSOR);
	}

	private void on_button_selector_start_rt_clicked(object o, EventArgs args)
	{
		changeMode (Constants.Modes.RT);
	}

	private void on_button_selector_start_other_clicked(object o, EventArgs args)
	{
		changeMode (Constants.Modes.OTHER);
	}

	private void on_button_start_back_clicked(object o, EventArgs args)
	{
		if(notebook_mode_selector2.CurrentPage == 2) //runs photocell
			notebook_mode_selector2.CurrentPage = 1; //runs
		else
			notebook_mode_selector.CurrentPage = 0; //main
	}

	[Widget] Gtk.Label label_button_execute_connect;

	private void button_execute_test_show_connect_or_execute (bool connect_or_execute)
	{
		string space = Catalog.GetString("Space");

		if(connect_or_execute)
		{
			label_button_execute_connect.Visible = true;
			image_button_execute.Visible = false;
			button_execute_test.TooltipText = Catalog.GetString("Connect") + string.Format(" ({0}+{1})", kCtrl, space);
		} else
		{
			label_button_execute_connect.Visible = false;
			image_button_execute.Visible = true;
			button_execute_test.TooltipText = Catalog.GetString("Execute test") + string.Format(" ({0}+{1})", kCtrl, space);
		}
	}

	/*
	 * end of menu test selectors
	 */

	

	/*
	 * cancel and finish
	 */


	private void on_cancel_clicked (object o, EventArgs args) 
	{
		event_execute_ButtonCancel.Clicked -= new EventHandler(on_cancel_clicked);

		if(capturingForce == arduinoCaptureStatus.STARTING || capturingForce == arduinoCaptureStatus.CAPTURING)
		{
			LogB.Information("cancel clicked on force");
			forceProcessCancel = true;
			return;
		}
		if(capturingRunEncoder == arduinoCaptureStatus.STARTING || capturingRunEncoder == arduinoCaptureStatus.CAPTURING)
		{
			LogB.Information("cancel clicked on runEncoder");
			runEncoderProcessCancel = true;
			return;
		}

		LogB.Information("cancel clicked one");

		//this will mark the test as cancelled
		currentEventExecute.Cancel = true;

		//this will actually cancel Read_cambio and then Read_event in order to really cancel
		Chronopic.CancelDo();

		//let update stats
		if(createdStatsWin)
			showUpdateStatsAndHideData(true);
	}
	
	private void on_cancel_multi_clicked (object o, EventArgs args) 
	{
		LogB.Information("cancel multi clicked one");

		//this will mark the test as cancelled
		currentEventExecute.Cancel = true;

		//this will actually cancel Read_cambio and then Read_event in order to really cancel
		Chronopic.CancelDo();
	}

	private void on_finish_clicked (object o, EventArgs args) 
	{
		//to avoid doble finish or cancel while finishing
		hideButtons();

		if(capturingForce == arduinoCaptureStatus.STARTING || capturingForce == arduinoCaptureStatus.CAPTURING)
		{
			LogB.Information("finish clicked on force");
			forceProcessFinish = true;
			return;
		}
		if(capturingRunEncoder == arduinoCaptureStatus.STARTING || capturingRunEncoder == arduinoCaptureStatus.CAPTURING)
		{
			LogB.Information("finish clicked on runEncoder");
			runEncoderProcessFinish = true;
			return;
		}


		LogB.Information("finish clicked one");

		event_execute_ButtonFinish.Clicked -= new EventHandler(on_finish_clicked);
		currentEventExecute.Finish = true;
	
		//this will actually cancel Read_cambio and then Read_event in order to really finish
		Chronopic.FinishDo();
		
		//let update stats
		if(createdStatsWin)
			showUpdateStatsAndHideData(true);
	}
		
	private void on_finish_multi_clicked (object o, EventArgs args) 
	{
		event_execute_ButtonFinish.Clicked -= new EventHandler(on_finish_multi_clicked);

		currentEventExecute.Finish = true;
		
		//runA is not called for this, because it ends different
		//and there's a message on gui/eventExecute.cs for runA	
		LogB.Debug("Calling finish on multi");
		//if(currentMultiChronopicType.Name != Constants.RunAnalysisName && cp2016.StoredCanCaptureContacts)
		//	checkFinishMultiTotally(o, args);

		//this will actually cancel Read_cambio and then Read_event in order to really finish
		Chronopic.FinishDo();

		LogB.Debug("Called finish on multi");
	}

	DialogThreshold dialogThreshold;
	private void on_threshold_clicked (object o, EventArgs args)
	{
		dialogThreshold = new DialogThreshold(current_mode, threshold.GetT);
		dialogThreshold.FakeButtonClose.Clicked += new EventHandler(on_threshold_close);
	}

	private void on_threshold_close (object o, EventArgs args)
	{
		dialogThreshold.FakeButtonClose.Clicked -= new EventHandler(on_threshold_close);

		threshold.UpdateFromGUI(dialogThreshold.ThresholdCurrent);
		label_threshold.Text = //Catalog.GetString("Threshold") + " " +
			threshold.GetLabel() + " ms";

		dialogThreshold.DestroyDialog();
	}

	void on_button_execute_test_clicked (object o, EventArgs args) 
	{
		if(current_mode == Constants.Modes.FORCESENSOR)
		{
			//LogB.Debug("radio_mode_force_sensor");
			/*
			 * force sensor is not FTDI
			 on_force_sensor_activate(canCaptureC);
			 */

			on_buttons_force_sensor_clicked(button_execute_test, new EventArgs ());
			return;
		}
		if(current_mode == Constants.Modes.RUNSENCODER)
		{
			LogB.Debug("runs_encoder");
			/*
			 * runs encoder is not FTDI
			 */

			on_runs_encoder_capture_clicked ();
			return;
		}

		// stop capturing inertial on the background if we start capturing a contacts test
		if(encoderThreadBG != null && encoderThreadBG.IsAlive)
		{
			stopCapturingInertialBG();
		}

		if(current_mode == Constants.Modes.RUNSINTERVALLIC && compujumpAutologout != null)
			compujumpAutologout.StartCapturingRunInterval();

		chronopicRegisterUpdate(false);

		if( chronopicRegister.NumConnectedOfType(ChronopicRegisterPort.Types.RUN_WIRELESS) == 1 && (
			current_mode == Constants.Modes.RUNSSIMPLE ||
			current_mode == Constants.Modes.RUNSINTERVALLIC) )
		{
			//cp2016.StoredCanCaptureContacts = true;
			cp2016.StoredWireless = true;
			button_execute_test_show_connect_or_execute(false); //to not show "connect"

			on_button_execute_test_acceptedPre_start_camera(WebcamStartedTestStart.CHRONOPIC);
			return;
		}
		cp2016.StoredWireless = false;

		int numContacts = chronopicRegister.NumConnectedOfType(ChronopicRegisterPort.Types.CONTACTS);
		LogB.Information("numContacts: " + numContacts);

		//check if chronopics have changed
		if(numContacts >= 2 && current_mode == Constants.Modes.OTHER && radio_mode_multi_chronopic_small.Active)
		{
			chronopicConnectionSequenceInit(2);
		}
		else if(numContacts >= 1) //will get first
		{
			chronopicConnectionSequenceInit(1);
		}
		else //(numContacts == 0)
		{
			//store a boolean in order to read info faster
			cp2016.StoredCanCaptureContacts = false;

			/*
			 * if serial port gets opened, then a new USB connection will use different ttyUSB on Linux
			 * and maybe is the cause for blocking the port on OSX
			 * close the port if opened
			 */
			cp2016.SerialPortsCloseIfNeeded(true);

			//simulated tests are only allowed on SIMULATED session
			if(currentSession.Name != Constants.SessionSimulatedName)
			{
				//new DialogMessage(Constants.MessageTypes.WARNING, Constants.SimulatedTestsNotAllowed);
	                        //UtilGtk.DeviceColors(viewport_chronopics, false);
				//open device window
				chronopicRegisterUpdate(true);

				return;
			}
			on_button_execute_test_acceptedPre_start_camera(WebcamStartedTestStart.CHRONOPIC);
		}
	        UtilGtk.DeviceColors(viewport_chronopics, true);
	}

	// camera stuff if needed
	// true is chronopic
	// false is arduino like force sensor or run encoder
	public enum WebcamStartedTestStart { CHRONOPIC, FORCESENSOR, RUNENCODER};

	private void on_button_execute_test_acceptedPre_start_camera(WebcamStartedTestStart wsts)
	{
		LogB.Information("on_button_execute_test_acceptedPre_start_camera " + wsts.ToString());
		button_video_play_this_test_contacts_sensitive (WebcamManage.GuiContactsEncoder.CONTACTS, false);

		webcamManage = new WebcamManage();
		if(! webcamStart (WebcamManage.GuiContactsEncoder.CONTACTS, 1))
		{
			if(wsts == WebcamStartedTestStart.FORCESENSOR)
				forceSensorCapturePre3_GTK_cameraCalled();
			else if(wsts == WebcamStartedTestStart.RUNENCODER)
				runEncoderCapturePre3_GTK_cameraCalled();
			else // (wsts == WebcamStartedTestStart.CHRONOPIC)
				on_button_execute_test_accepted();

			return;
		}

		bool waitUntilRecording = true;
		if(! waitUntilRecording)
		{
			notebook_video_contacts.CurrentPage = 1;
			if(wsts == WebcamStartedTestStart.FORCESENSOR)
				forceSensorCapturePre3_GTK_cameraCalled();
			else if(wsts == WebcamStartedTestStart.RUNENCODER)
				runEncoderCapturePre3_GTK_cameraCalled();
			else // (wsts == WebcamStartedTestStart.CHRONOPIC)
				on_button_execute_test_accepted();
		}
	}

	void on_button_execute_test_accepted ()
	{
		bool canCaptureC = cp2016.StoredCanCaptureContacts;
		bool wireless = cp2016.StoredWireless;

		if(canCaptureC && cp2016.CP == null)
		{
			new DialogMessage(Constants.MessageTypes.WARNING,
					"Problems connecting with Chronopic." + "\n\n" + "Please, restart Chronojump");
			return;
		}

		/*
		 * We need to do this to ensure no cancel_clicked calls accumulate
		 * if we don't do tue -= now, after 10 tests, if we cancel last one,
		 * it wWill enter on_cancel_clicked 10 times at the end
		 */
		event_execute_ButtonCancel.Clicked -= new EventHandler(on_cancel_clicked);
		event_execute_ButtonFinish.Clicked -= new EventHandler(on_finish_clicked);

		if(current_mode == Constants.Modes.JUMPSSIMPLE)
		{
			on_normal_jump_activate(canCaptureC);
		}
		else if(current_mode == Constants.Modes.JUMPSREACTIVE)
		{
			on_rj_activate(canCaptureC);
		}
		else if(current_mode == Constants.Modes.RUNSSIMPLE)
		{
			extra_window_runs_distance = Convert.ToDouble(label_runs_simple_track_distance_value.Text);

			on_normal_run_activate(canCaptureC, wireless);
		}
		else if(current_mode == Constants.Modes.RUNSINTERVALLIC)
		{
			//RSA runs cannot be simulated because it's complicated to manage the countdown event...
			if(currentRunIntervalType.IsRSA && ! canCaptureC) {
				new DialogMessage(Constants.MessageTypes.WARNING, 
						Catalog.GetString("Sorry, RSA tests cannot be simulated."));
				return;
			}

			//extra_window_runs_interval_distance = (double) extra_window_runs_interval_spinbutton_distance.Value;
			extra_window_runs_interval_distance = Convert.ToDouble(label_runs_interval_track_distance_value.Text);
			extra_window_runs_interval_limit = extra_window_runs_interval_spinbutton_limit.Value;
			
			on_run_interval_activate(canCaptureC, wireless);
		}
		else if(current_mode == Constants.Modes.RT)
		{
			if(extra_window_radio_reaction_time_discriminative.Active)
				reaction_time_discriminative_lights_prepare();

			on_reaction_time_activate (canCaptureC);
		}
		else if(radio_mode_pulses_small.Active)
		{
			on_pulse_activate (canCaptureC);
		}
		else if(radio_mode_multi_chronopic_small.Active)
		{
			on_multi_chronopic_start_clicked(canCaptureC);
		}

		//if a test has been deleted
		//notebook_results_data changes to page 3: "deleted test"
		//when a new test is done
		//this notebook has to poing again to data of it's test
		change_notebook_results_data();
	}

	private void contactsShowCaptureDoingButtons(bool captureDoing)
	{
		if(captureDoing)
			notebook_contacts_capture_doing_wait.CurrentPage = 1;
		else
			notebook_contacts_capture_doing_wait.CurrentPage = 0;
	}

	private void on_button_contacts_capture_load_clicked (object o, EventArgs args)
	{
		//on this case should not arrive here becuase sensitivity does not allow it. But extra check just in case.
		if(currentPerson == null || currentSession == null)
			return;

		if(current_mode == Constants.Modes.FORCESENSOR)
			force_sensor_load();
		else if(current_mode == Constants.Modes.RUNSENCODER)
			run_encoder_load();
	}

	private void on_button_contacts_recalculate_clicked (object o, EventArgs args)
	{
		if(current_mode == Constants.Modes.FORCESENSOR)
			force_sensor_recalculate();
		else if(current_mode == Constants.Modes.RUNSENCODER)
			run_encoder_recalculate();
	}

	void on_textview_contacts_signal_comment_key_press_event (object o, EventArgs args)
	{
		button_contacts_signal_save_comment.Label = Catalog.GetString("Save comment");
		button_contacts_signal_save_comment.Sensitive = true;
	}
	void on_button_contacts_signal_save_comment_clicked (object o, EventArgs args)
	{
		if(current_mode == Constants.Modes.FORCESENSOR)
		{
			currentForceSensor.Comments = UtilGtk.TextViewGetCommentValidSQL(textview_contacts_signal_comment);
			currentForceSensor.UpdateSQLJustComments(false);
		}
		else if(current_mode == Constants.Modes.RUNSENCODER)
		{
			currentRunEncoder.Comments = UtilGtk.TextViewGetCommentValidSQL(textview_contacts_signal_comment);
			currentRunEncoder.UpdateSQLJustComments(false);
		}

		button_contacts_signal_save_comment.Label = Catalog.GetString("Saved comment.");
		button_contacts_signal_save_comment.Sensitive = false;
	}

	private Constants.BellModes getBellMode (Constants.Modes m)
	{
		if(m == Constants.Modes.JUMPSREACTIVE)
			return Constants.BellModes.JUMPS;
		else if(m == Constants.Modes.RUNSINTERVALLIC)
			return Constants.BellModes.RUNS;
		else if(m == Constants.Modes.POWERGRAVITATORY)
			return Constants.BellModes.ENCODERGRAVITATORY;
		else if(m == Constants.Modes.POWERINERTIAL)
			return Constants.BellModes.ENCODERINERTIAL;
		else if(m == Constants.Modes.FORCESENSOR)
			return Constants.BellModes.FORCESENSOR;

		//default to JUMPSREACTIVE
		return Constants.BellModes.JUMPS;
	}

	private void on_button_contacts_bells_clicked (object o, EventArgs args)
	{
		Constants.Modes m = current_mode;
		if(m != Constants.Modes.JUMPSREACTIVE &&
				m != Constants.Modes.RUNSINTERVALLIC &&
				m != Constants.Modes.FORCESENSOR)
			return;

		repetitiveConditionsWin.View(getBellMode(m), preferences, encoderRhythm, true);
	}

	private void change_notebook_results_data()
	{
		//there are some notebook_execute pages that have not notebook_results_data pages
		//like jump simple (0), run simple (2), reaction time (4)
		//if(notebook_execute.CurrentPage == 1) //reactive jump
		//	notebook_results_data.CurrentPage = 0;
		//else if(notebook_execute.CurrentPage == 3) //interval run
		//if(notebook_execute.CurrentPage == 3) //interval run
		//	notebook_results_data.CurrentPage = 1;
		//else if(notebook_execute.CurrentPage == 6) //pulse
		if(notebook_execute.CurrentPage == 6) //pulse
			notebook_results_data.CurrentPage = 2;
	}

	private void changeTestImage(string eventTypeString, string eventName, string fileNameString) {
		Pixbuf pixbuf; //main image
		Pixbuf pixbufZoom; //icon of zoom image (if shown can have two different images)

		switch (fileNameString) {
			case "LOGO":
				pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameLogo);
				button_image_test_zoom.Hide();
			break;
			case "FORCESENSOR_NOT_ELASTIC":
				pixbuf = new Pixbuf (null, Util.GetImagePath(true) + Constants.FileNameForceSensorNotElastic);
				button_image_test_zoom.Hide();
			break;
			case "FORCESENSOR_ELASTIC":
				pixbuf = new Pixbuf (null, Util.GetImagePath(true) + Constants.FileNameForceSensorElastic);
				button_image_test_zoom.Hide();
			break;
			case "RUNSENCODER":
				//pixbuf = new Pixbuf (null, Util.GetImagePath(true) + Constants.FileNameRunEncoder);
				pixbuf = new Pixbuf (null, Util.GetImagePath(true) + "no_image.png");
				button_image_test_zoom.Hide();
			break;
			case "":
				pixbuf = new Pixbuf (null, Util.GetImagePath(true) + "no_image.png");
				button_image_test_zoom.Hide();
			break;
			default:
				pixbuf = new Pixbuf (null, Util.GetImagePath(true) + fileNameString);

				//button image test zoom will have a different image depending on if there's text
				//future: change tooltip also
				if(eventTypeString != "" && eventName != "" && eventTypeHasLongDescription (eventTypeString, eventName))
					pixbufZoom = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameZoomInWithTextIcon);
				else 
					pixbufZoom = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameZoomInIcon);

				image_test_zoom.Pixbuf = pixbufZoom;
				button_image_test_zoom.Show();
			break;
		}
		image_test.Pixbuf = pixbuf;
	}

	private bool eventTypeHasLongDescription (string eventTypeString, string eventName) {
		if(eventTypeString != "" && eventName != "")
		{
			EventType myType = new EventType ();

			if(eventTypeString == EventType.Types.JUMP.ToString()) 
				myType = new JumpType(eventName);
			else if (eventTypeString == EventType.Types.RUN.ToString()) 
				myType = new RunType(eventName);
			else if (eventTypeString == EventType.Types.REACTIONTIME.ToString()) 
				myType = new ReactionTimeType(eventName);
			else if (eventTypeString == EventType.Types.PULSE.ToString()) 
				myType = new PulseType(eventName);
			else if (eventTypeString == EventType.Types.MULTICHRONOPIC.ToString()) 
				myType = new MultiChronopicType(eventName);
			else LogB.Error("Error on eventTypeHasLongDescription");

			if(myType.HasLongDescription)
				return true;
		}
		return false;
	}


	/* ---------------------------------------------------------
	 * ----------------  JUMPS EXECUTION (no RJ) ----------------
	 *  --------------------------------------------------------
	 */

	//suitable for all jumps not repetitive
	private void on_normal_jump_activate (bool canCaptureC)
	{
		if(execute_auto_doing)
			sensitiveGuiAutoExecuteOrWait (true);
		
		double jumpWeight = 0;

		//to store how this test is for future jumps (prepare)
		LastJumpSimpleTypeParams ljstp = new LastJumpSimpleTypeParams(currentJumpType.Name);

		if(currentJumpType.HasWeight)
		{
			double selectedWeight = (double) extra_window_jumps_spinbutton_weight.Value;
			if(extra_window_jumps_option == "%")
				jumpWeight = selectedWeight;
			else {
				jumpWeight = Util.WeightFromKgToPercent(
						selectedWeight,
						currentPersonSession.Weight);
				ljstp.weightIsPercent = false;
			}
			ljstp.weightValue = selectedWeight;
		}

		double myFall = 0;
		ljstp.fallmm = 0;
		if(currentJumpType.HasFall) {
			if(extra_window_jumps_check_dj_fall_calculate.Active) {
				myFall = -1;
				ljstp.fallmm = -1;
			} else {
				myFall = (double) extra_window_jumps_spinbutton_fall.Value;
				ljstp.fallmm = Convert.ToInt32(myFall * 10);
			}
		}

		//to store how this test is for future jumps (do)
		if(currentJumpType.HasWeight || currentJumpType.HasFall)
			SqliteJumpType.LastJumpSimpleTypeParamsInsertOrUpdate(ljstp);

		string description = "";
		if(currentJumpType.Name == "slCMJleft" || currentJumpType.Name == "slCMJright") {
			description = slCMJString(); 

			extra_window_jumps_spin_single_leg_distance.Value = 0;
			extra_window_jumps_spin_single_leg_angle.Value = 90;
		}
			
		//used by cancel and finish
		//currentEventType = new JumpType();
		currentEventType = currentJumpType;
			
		//hide jumping buttons
		if(! execute_auto_doing)
			sensitiveGuiEventDoing(false);

		//show the event doing window
		double progressbarLimit = 3; //3 phases for show the Dj
		if(myFall == -1)
			progressbarLimit = 4; //4 if there's a pre-jump
		else if( currentJumpType.StartIn || 
				currentJumpType.Name == Constants.TakeOffName || 
				currentJumpType.Name == Constants.TakeOffWeightName)
			progressbarLimit = 2; //2 for normal jump (or take off)
			
		//don't let update until test finishes
		if(createdStatsWin)
			showUpdateStatsAndHideData(false);

		event_execute_initializeVariables(
			! canCaptureC,	//is simulated
			currentPerson.UniqueID, 
			currentPerson.Name, 
			Catalog.GetString("Phases"),  	  //name of the different moments
			Constants.JumpTable, //tableName
			currentJumpType.Name 
			);
		event_execute_button_cancel.Sensitive = true;

		ExecutingGraphData egd = event_execute_prepareForTest ();

		event_execute_ButtonCancel.Clicked += new EventHandler(on_cancel_clicked);
		event_execute_ButtonFinish.Clicked += new EventHandler(on_finish_clicked);

		currentEventExecute = new JumpExecute(currentPerson.UniqueID, currentPerson.Name, 
				currentSession.UniqueID, currentJumpType.Name, myFall, jumpWeight,
				cp2016.CP, preferences.digitsNumber,
				preferences.volumeOn, preferences.gstreamer,
				progressbarLimit, egd, description, configChronojump.Exhibition,
				preferences.heightPreferred,
				Convert.ToInt32(spin_contacts_graph_last_limit.Value),
				radio_contacts_graph_allTests.Active, radio_contacts_graph_allPersons.Active);


		//UtilGtk.ChronopicColors(viewport_chronopics, label_chronopics, label_connected_chronopics, chronopicWin.Connected);

		if (! canCaptureC)
			currentEventExecute.SimulateInitValues(rand);

		contactsShowCaptureDoingButtons(true);
		if( currentJumpType.StartIn ) 
			currentEventExecute.Manage();
		else 
			currentEventExecute.ManageFall();

		thisJumpIsSimple = true; //used by: on_event_execute_update_graph_in_progress_clicked
		currentEventExecute.FakeButtonUpdateGraph.Clicked += 
			new EventHandler(on_event_execute_update_graph_in_progress_clicked);
		currentEventExecute.FakeButtonThreadDyed.Clicked += new EventHandler(on_test_finished_can_touch_gtk);
	}	
	

	private void on_jump_finished ()
	{
		//test can be deleted if not cancelled
		sensitiveLastTestButtons(! currentEventExecute.Cancel);

		if ( ! currentEventExecute.Cancel ) {
			currentJump = (Jump) currentEventExecute.EventDone;
		
			if(currentJumpType.Name == "slCMJleft" || currentJumpType.Name == "slCMJright") {
				if(extra_window_jumps_radiobutton_single_leg_mode_vertical.Active)
					currentJump.Description += " 0 90";
				else {
					currentJump.Description += " 0 90";
					
					//unsensitive slCMJ options 
					table_extra_window_jumps_single_leg_radios.Sensitive = false;
					//but show the input cm
					notebook_contacts_capture_doing_wait.CurrentPage = 2;
				}
				SqliteJump.UpdateDescription(Constants.JumpTable, 
						currentJump.UniqueID, currentJump.Description);
			}

			myTreeViewJumps.PersonWeight = currentPersonSession.Weight;
			myTreeViewJumps.Add(currentPerson.Name, currentJump);
			
			if(createdStatsWin) {
				showUpdateStatsAndHideData(true);
			}
		
			lastJumpIsSimple = true;
		
			//unhide buttons for delete last jump
			if(! execute_auto_doing)
				sensitiveGuiYesEvent();

			if(configChronojump.Exhibition && configChronojump.ExhibitionStationType == ExhibitionTest.testTypes.JUMP)
				SqliteJson.UploadExhibitionTest(getExhibitionTestFromGui(ExhibitionTest.testTypes.JUMP, Convert.ToDouble(Util.GetHeightInCentimeters(currentJump.Tv.ToString()))));
		} 
		else if( currentEventExecute.ChronopicDisconnected )
			chronopicDisconnectedWhileExecuting();
		
		//unhide buttons that allow jumping
		if(execute_auto_doing) {
			execute_auto_order_pos ++;
			execute_auto_select();
			sensitiveGuiAutoExecuteOrWait (false);
		}

		//Cairo graph is not updated if window is not resized, so force update
		updateGraphJumpsSimple();

		//stop camera (storing value or not)
		if(currentEventExecute.Cancel || currentJump == null)
			webcamEnd (Constants.TestTypes.JUMP, -1);
		else
			webcamEnd (Constants.TestTypes.JUMP, currentJump.UniqueID);

		//since 0.7.4.1 when test is done, treeview select it. action event button have to be shown
		//this has to be after webcamRecordEnd in order to see if video is created
		showHideActionEventButtons(true, "Jump"); //show
	}

	private void chronopicDisconnectedWhileExecuting() {
		LogB.Error("DISCONNECTED gui/cj");
		//createChronopicWindow(true, "");
		//chronopicWin.Connected = false;
	}

	private void on_test_finished_can_touch_gtk (object o, EventArgs args)
	{
		currentEventExecute.FakeButtonThreadDyed.Clicked -= new EventHandler(on_test_finished_can_touch_gtk);
		contactsShowCaptureDoingButtons(false);

		on_event_execute_EventEnded();

		LogB.Information(" cantouch 0: calling on_xxx_finished...");
		switch (currentEventType.Type)
		{
			case EventType.Types.JUMP:
				if(thisJumpIsSimple)
					on_jump_finished();
				else
					on_jump_rj_finished();
				break;
			case EventType.Types.RUN:
				if(thisRunIsSimple)
					on_run_finished();
				else
					on_run_interval_finished();
				break;
			case EventType.Types.REACTIONTIME:
				on_reaction_time_finished();
				break;
			case EventType.Types.PULSE:
				on_pulse_finished();
				break;
			case EventType.Types.MULTICHRONOPIC:
				on_multi_chronopic_finished();
				break;
		}

		LogB.Information(" cantouch1 ");

		//if webcam started then do not call sensitiveGuiEventDone(), because will be called at webcamEnd (that has a delay)
		if(! execute_auto_doing && ! webcamManage.ReallyStarted)
			sensitiveGuiEventDone();

		LogB.Information(" cantouch3 ");

		if (currentEventExecute.Cancel)
		{
			event_execute_progressbar_event.Fraction = 0;
			event_execute_progressbar_time.Fraction = 0;
			event_execute_label_event_value.Text = "";
			event_execute_label_time_value.Text = "";
		} else {
			event_execute_progressbar_time.Fraction = 1;

			restTime.AddOrModify(currentPerson.UniqueID, currentPerson.Name, true);
			updateRestTimes();
		}

		chronojumpWindowTestsNext();
	}

	//called each second and after a test
	bool updateRestTimes()
	{
		if(! updatingRestTimes)
			return false;

		//Compujump manage autologout
		if( currentPerson != null && configChronojump.Compujump && compujumpAutologout != null)
		{
			if(compujumpAutologout.ShouldILogoutNow())
					//restTime.CompujumpPersonNeedLogout(currentPerson.UniqueID), 		     //3' since last executed test
			{
				compujumpPersonLogoutDo();

				label_logout_seconds.Text = "";
				label_logout_seconds_encoder.Text = "";
			} else {
				/*
				 * TODO: implement when it's nicer and only is displayed when 10 seconds remain
				label_logout_seconds.Text = compujumpAutologout.RemainingSeconds(false);
				label_logout_seconds_encoder.Text = compujumpAutologout.RemainingSeconds(false);
				*/
				string logoutSecondsStr = "";
				if(! compujumpAutologout.IsCompujumpCapturing())
				{
					int remainingSeconds = compujumpAutologout.RemainingSeconds();
					if(remainingSeconds <= 10)
						logoutSecondsStr = string.Format("Logout\nin {0} s", compujumpAutologout.RemainingSeconds());
				}

				label_logout_seconds.Text = logoutSecondsStr;
				label_logout_seconds_encoder.Text = logoutSecondsStr;
			}
		}

		if( ! configChronojump.PersonWinHide)
		{
			myTreeViewPersons.UpdateRestTimes(restTime);
			return true;
		}

		if(current_mode == Constants.Modes.POWERGRAVITATORY ||
			       current_mode == Constants.Modes.POWERINERTIAL)
		{
			updateTopRestTimesEncoder();
		} else {
			updateTopRestTimesContacts();
		}

		return true;
	}

	private int get_configured_rest_time_in_seconds()
	{
		if(preferences.restTimeMinutes < 0)
			return 0;
		else
			return 60 * preferences.restTimeMinutes + preferences.restTimeSeconds;
	}

	/* ---------------------------------------------------------
	 * ----------------  JUMPS RJ EXECUTION  ------------------
	 *  --------------------------------------------------------
	 */
	
	private void on_rj_activate (bool canCaptureC)
	{
		double progressbarLimit = 0;
		
		//to store how this test is for future jumps (prepare)
		LastJumpRjTypeParams ljrtp = new LastJumpRjTypeParams(currentJumpRjType.Name);

		//if it's a unlimited interval run, put -1 as limit value
		if(currentJumpRjType.Unlimited) {
			progressbarLimit = -1;
		} else {
			if(currentJumpRjType.FixedValue > 0) {
				progressbarLimit = currentJumpRjType.FixedValue;
			} else {
				progressbarLimit = (double) extra_window_jumps_rj_spinbutton_limit.Value;
				ljrtp.limitedValue = Convert.ToInt32(progressbarLimit);
			}
		}

		double jumpWeight = 0;
		if(currentJumpRjType.HasWeight)
		{
			double selectedWeight = (double) extra_window_jumps_rj_spinbutton_weight.Value;
			if(extra_window_jumps_rj_option == "%") {
				jumpWeight = selectedWeight;
			} else {
				jumpWeight = Util.WeightFromKgToPercent(
						selectedWeight,
						currentPersonSession.Weight);
				ljrtp.weightIsPercent = false;
			}
			ljrtp.weightValue = selectedWeight;
		}
		double myFall = 0;
		if( currentJumpRjType.HasFall || currentJumpRjType.Name == Constants.RunAnalysisName)
		{
			myFall = (double) extra_window_jumps_rj_spinbutton_fall.Value;
			ljrtp.fallmm = Convert.ToInt32(myFall * 10);
		}

		//to store how this test is for future jumps (do)
		if( (! currentJumpRjType.Unlimited && currentJumpRjType.FixedValue == 0) ||
				currentJumpType.HasWeight || currentJumpType.HasFall )
			SqliteJumpType.LastJumpRjTypeParamsInsertOrUpdate(ljrtp);

		//used by cancel and finish
		//currentEventType = new JumpRjType();
		currentEventType = currentJumpRjType;
			
		//hide jumping buttons
		sensitiveGuiEventDoing(false);
	
		//don't let update until test finishes
		if(createdStatsWin)
			showUpdateStatsAndHideData(false);

		//show the event doing window
		event_execute_initializeVariables(
			! canCaptureC,	//is simulated
			currentPerson.UniqueID, 
			currentPerson.Name, 
			Catalog.GetString("Jumps"),  	  //name of the different moments
			Constants.JumpRjTable, //tableName
			currentJumpRjType.Name
			);
		event_execute_button_cancel.Sensitive = true;

		ExecutingGraphData egd = event_execute_prepareForTest ();
		blankJumpReactiveRealtimeCaptureGraph ();

		event_execute_ButtonCancel.Clicked += new EventHandler(on_cancel_clicked);
		event_execute_ButtonFinish.Clicked += new EventHandler(on_finish_clicked);

		currentEventExecute = new JumpRjExecute(currentPerson.UniqueID, currentPerson.Name, 
				currentSession.UniqueID, currentJumpRjType.Name, myFall, jumpWeight, 
				progressbarLimit, currentJumpRjType.JumpsLimited, 
				cp2016.CP, preferences.digitsNumber,
				checkbutton_allow_finish_rj_after_time.Active,
				preferences.volumeOn, preferences.gstreamer,
				repetitiveConditionsWin, progressbarLimit, egd);
		
		//suitable for limited by jump and time
		//simulated always simulate limited by jumps
		if(! canCaptureC)
			currentEventExecute.SimulateInitValues(rand);
		
		contactsShowCaptureDoingButtons(true);
		currentEventExecute.Manage();

		thisJumpIsSimple = false; //used by: on_event_execute_update_graph_in_progress_clicked
		currentEventExecute.FakeButtonUpdateGraph.Clicked += 
			new EventHandler(on_event_execute_update_graph_in_progress_clicked);
		currentEventExecute.FakeButtonThreadDyed.Clicked += new EventHandler(on_test_finished_can_touch_gtk);
	}
		
	private void on_jump_rj_finished ()
	{
		LogB.Information("ON JUMP RJ FINISHED");
		
		//test can be deleted if not cancelled
		sensitiveLastTestButtons(! currentEventExecute.Cancel);

		if ( ! currentEventExecute.Cancel ) {
			currentJumpRj = (JumpRj) currentEventExecute.EventDone;
			
			//if user clicked in finish earlier
			if(currentEventExecute.Finish) {
				currentJumpRj.Jumps = Util.GetNumberOfJumps(currentJumpRj.TvString, false);
				if(currentJumpRj.JumpsLimited) {
					currentJumpRj.Limited = currentJumpRj.Jumps.ToString() + "J";
				} else {
					currentJumpRj.Limited = Util.GetTotalTime(
							currentJumpRj.TcString, currentJumpRj.TvString) + "T";
				}
			}

			myTreeViewJumpsRj.PersonWeight = currentPersonSession.Weight;
			myTreeViewJumpsRj.Add(currentPerson.Name, currentJumpRj);
			
			if(createdStatsWin) {
				showUpdateStatsAndHideData(true);
			}

			lastJumpIsSimple = false;

			//unhide buttons for delete last jump
			sensitiveGuiYesEvent();

			//put correct time value in eventWindow (put the time from chronopic and not onTimer soft chronometer)
			event_execute_LabelTimeValue = Util.GetTotalTime(currentJumpRj.TcString, currentJumpRj.TvString);
			//possible deletion of last jump can make the jumps on event window be false
			event_execute_LabelEventValue = currentJumpRj.Jumps;
		} 
		else if( currentEventExecute.ChronopicDisconnected )
			chronopicDisconnectedWhileExecuting();
		
		//delete the temp tables if exists
		Sqlite.DeleteTempEvents("tempJumpRj");

		//Cairo graph is not updated if window is not resized, so force update
		updateGraphJumpsReactive();

		//stop camera (storing value or not)
		if(currentEventExecute.Cancel || currentJumpRj == null) {
			//webcamEndTwoCams (Constants.TestTypes.JUMP_RJ, -1);
			webcamEnd (Constants.TestTypes.JUMP_RJ, -1);
		}
		else {
			//webcamEndTwoCams (Constants.TestTypes.JUMP_RJ, currentJumpRj.UniqueID);
			webcamEnd (Constants.TestTypes.JUMP_RJ, currentJumpRj.UniqueID);
		}

		//since 0.7.4.1 when test is done, treeview select it. action event button have to be shown
		//this has to be after webcamRecordEnd in order to see if video is created
		showHideActionEventButtons(true, "JumpRj"); //show
	}

	/* ---------------------------------------------------------
	 * ----------------  RUNS EXECUTION (no interval) ----------
	 *  --------------------------------------------------------
	 */

	//suitable for all runs not repetitive
	private void on_normal_run_activate (bool canCaptureC, bool wireless)
	{
		LogB.Information("on_normal_run_activate");
		//if distance can be always different in this run,
		//show values selected in runExtraWin
		double myDistance = 0;		
		if(currentRunType.Distance == 0) {
			myDistance = extra_window_runs_distance;
		} else {
			myDistance = currentRunType.Distance;
		}
		
		//used by cancel and finish
		//currentEventType = new RunType();
		currentEventType = currentRunType;
			
		//hide jumping (running) buttons
		sensitiveGuiEventDoing(false);

		//show the event doing window
		
		double progressbarLimit = 3; //same for startingIn than out (before)
		
		//don't let update until test finishes
		if(createdStatsWin)
			showUpdateStatsAndHideData(false);

		string wirelessPort = "";
		int wirelessBauds = 0;
		if(wireless) {
			wirelessPort = chronopicRegister.ConnectedOfType(ChronopicRegisterPort.Types.RUN_WIRELESS).Port;
			wirelessBauds = 115200;
		}

		event_execute_initializeVariables(
			(! canCaptureC && ! wireless),	//is simulated
			currentPerson.UniqueID, 
			currentPerson.Name, 
			Catalog.GetString("Phases"),  	  //name of the different moments
			Constants.RunTable, //tableName
			currentRunType.Name 
			);

		event_execute_button_cancel.Sensitive = true;

		ExecutingGraphData egd = event_execute_prepareForTest ();
		
		event_execute_ButtonCancel.Clicked += new EventHandler(on_cancel_clicked);
		event_execute_ButtonFinish.Clicked += new EventHandler(on_finish_clicked);


		if(photocellWirelessCapture == null)
			photocellWirelessCapture = new PhotocellWirelessCapture(wirelessPort);

		currentEventExecute = new RunExecute(
				currentPerson.UniqueID, currentSession.UniqueID, 
				currentRunType.Name, myDistance, 
				cp2016.CP, photocellWirelessCapture, wirelessPort, wirelessBauds,
				preferences.digitsNumber, preferences.metersSecondsPreferred,
				preferences.volumeOn, preferences.gstreamer,
				progressbarLimit, egd,
				preferences.runDoubleContactsMode,
				preferences.runDoubleContactsMS,
				preferences.runSpeedStartArrival,
				check_run_simple_with_reaction_time.Active,
				image_run_execute_running,
				image_run_execute_photocell,
				Convert.ToInt32(spin_contacts_graph_last_limit.Value),
				radio_contacts_graph_allTests.Active, radio_contacts_graph_allPersons.Active
				);

		if (! canCaptureC && ! wireless)
			currentEventExecute.SimulateInitValues(rand);

		contactsShowCaptureDoingButtons(true);
		currentEventExecute.Manage();

		thisRunIsSimple = true; //used by: on_event_execute_update_graph_in_progress_clicked
		currentEventExecute.FakeButtonUpdateGraph.Clicked += 
			new EventHandler(on_event_execute_update_graph_in_progress_clicked);
		currentEventExecute.FakeButtonThreadDyed.Clicked += new EventHandler(on_test_finished_can_touch_gtk);
	}
	
	private void on_run_finished ()
	{
		//test can be deleted if not cancelled
		sensitiveLastTestButtons(! currentEventExecute.Cancel);

		if ( ! currentEventExecute.Cancel ) {
			currentRun = (Run) currentEventExecute.EventDone;
			
			currentRun.MetersSecondsPreferred = preferences.metersSecondsPreferred;

			myTreeViewRuns.Add(currentPerson.Name, currentRun);

			//since 0.7.4.1 when test is done, treeview select it. action event button have to be shown 
			//this has to be after webcamRecordEnd in order to see if video is created
			showHideActionEventButtons(true, "Run"); //show

			if(createdStatsWin) {
				showUpdateStatsAndHideData(true);
			}

			lastRunIsSimple = true;

			//unhide buttons for delete last jump
			sensitiveGuiYesEvent();

			//put correct time value in eventWindow (put the time from chronopic and not onTimer soft chronometer)
			event_execute_LabelTimeValue = currentRun.Time;

			if(configChronojump.Exhibition && configChronojump.ExhibitionStationType == ExhibitionTest.testTypes.RUN)
				SqliteJson.UploadExhibitionTest(getExhibitionTestFromGui(ExhibitionTest.testTypes.RUN, currentRun.Time));
		}
		else if( currentEventExecute.ChronopicDisconnected )
			chronopicDisconnectedWhileExecuting();

		//Cairo graph is not updated if window is not resized, so force update
		updateGraphRunsSimple();

		//stop camera (storing value or not)
		if(currentEventExecute.Cancel || currentRun == null)
			webcamEnd (Constants.TestTypes.RUN, -1);
		else
			webcamEnd (Constants.TestTypes.RUN, currentRun.UniqueID);
	}

	/* ---------------------------------------------------------
	 * ----------------  RUNS EXECUTION (interval) ----------
	 *  --------------------------------------------------------
	 */


	private void on_run_interval_activate (bool canCaptureC, bool wireless)
	{
		LogB.Information("run interval accepted");

		//if distance can be always different in this run,
		//show values selected in runExtraWin
		double distanceInterval = 0;		
		if(currentRunIntervalType.Distance == 0) {
			distanceInterval = extra_window_runs_interval_distance;
		} else {
			distanceInterval = currentRunIntervalType.Distance;
		}

		double progressbarLimit = 0;
		//if it's a unlimited interval run, put -1 as limit value
		if(currentRunIntervalType.Unlimited) {
			progressbarLimit = -1;
		} else {
			if(currentRunIntervalType.FixedValue > 0) {
				progressbarLimit = currentRunIntervalType.FixedValue;
			} else {
				progressbarLimit = extra_window_runs_interval_limit;
			}
		}


		//used by cancel and finish
		//currentEventType = new RunType();
		currentEventType = currentRunIntervalType;
			
		//hide running buttons
		sensitiveGuiEventDoing(false);
		button_inspect_last_test_run_intervallic.Sensitive = false;
		
		//don't let update until test finishes
		if(createdStatsWin)
			showUpdateStatsAndHideData(false);

		string wirelessPort = "";
		int wirelessBauds = 0;
		if(wireless) {
			wirelessPort = chronopicRegister.ConnectedOfType(ChronopicRegisterPort.Types.RUN_WIRELESS).Port;
			wirelessBauds = 115200;
		}

		//show the event doing window
		event_execute_initializeVariables(
			(! canCaptureC && ! wireless),	//is simulated
			currentPerson.UniqueID, 
			currentPerson.Name, 
			Catalog.GetString("Laps"),  	  //name of the different moments
			Constants.RunIntervalTable, //tableName
			currentRunIntervalType.Name
			);
		event_execute_button_cancel.Sensitive = true;

		ExecutingGraphData egd = event_execute_prepareForTest ();
		blankRunIntervalRealtimeCaptureGraph ();

		event_execute_ButtonCancel.Clicked += new EventHandler(on_cancel_clicked);
		event_execute_ButtonFinish.Clicked += new EventHandler(on_finish_clicked);

		if(photocellWirelessCapture == null)
			photocellWirelessCapture = new PhotocellWirelessCapture(wirelessPort);

		currentEventExecute = new RunIntervalExecute(
				currentPerson.UniqueID, currentSession.UniqueID, currentRunIntervalType.Name, 
				distanceInterval, progressbarLimit, currentRunIntervalType.TracksLimited, 
				cp2016.CP, photocellWirelessCapture, wirelessPort, wirelessBauds,
				preferences.digitsNumber, preferences.metersSecondsPreferred,
				preferences.volumeOn, preferences.gstreamer,
				repetitiveConditionsWin,
				progressbarLimit, egd,
				preferences.runIDoubleContactsMode,
				preferences.runIDoubleContactsMS,
				preferences.runSpeedStartArrival,
				check_run_interval_with_reaction_time.Active,
				image_run_execute_running,
				image_run_execute_photocell
				);

		//suitable for limited by tracks and time
		if(! canCaptureC && ! wireless)
			currentEventExecute.SimulateInitValues(rand);

		contactsShowCaptureDoingButtons(true);
		currentEventExecute.Manage();

		thisRunIsSimple = false; //used by: on_event_execute_update_graph_in_progress_clicked
		currentEventExecute.FakeButtonUpdateGraph.Clicked += 
			new EventHandler(on_event_execute_update_graph_in_progress_clicked);
		currentEventExecute.FakeButtonThreadDyed.Clicked += new EventHandler(on_test_finished_can_touch_gtk);
	}


	private void on_run_interval_finished ()
	{
		//test can be deleted if not cancelled
		sensitiveLastTestButtons(! currentEventExecute.Cancel);
		button_inspect_last_test_run_intervallic.Sensitive = ! currentEventExecute.Cancel;

		if ( ! currentEventExecute.Cancel ) {
			currentRunInterval = (RunInterval) currentEventExecute.EventDone;

			currentRunInterval.MetersSecondsPreferred = preferences.metersSecondsPreferred;

			//if user clicked in finish earlier
			if(currentEventExecute.Finish) {
				currentRunInterval.Tracks = Util.GetNumberOfJumps(currentRunInterval.IntervalTimesString, false);
				if(currentRunInterval.TracksLimited) {
					currentRunInterval.Limited = currentRunInterval.Tracks.ToString() + "R";
				} else {
					currentRunInterval.Limited = Util.GetTotalTime(
							currentRunInterval.IntervalTimesString) + "T";
				}
			}
			myTreeViewRunsInterval.Add(currentPerson.Name, currentRunInterval);

			//since 0.7.4.1 when test is done, treeview select it. action event button have to be shown 
			//this has to be after webcamRecordEnd in order to see if video is created
			showHideActionEventButtons(true, "RunInterval"); //show

			if(createdStatsWin) {
				showUpdateStatsAndHideData(true);
			}

			lastRunIsSimple = false;

			//unhide buttons for delete last jump
			sensitiveGuiYesEvent();

			//put correct time value in eventWindow (put the time from chronopic and not onTimer soft chronometer)
			event_execute_LabelTimeValue = currentRunInterval.TimeTotal;
			//possible deletion of last run can make the runs on event window be false
			event_execute_LabelEventValue = currentRunInterval.Tracks;

			addTreeView_runs_interval_sprint (currentRunInterval, currentRunIntervalType);

			if(configChronojump.Compujump)
			{
				calculateSprintAndUpload();
			}
		}
		else if( currentEventExecute.ChronopicDisconnected )
			chronopicDisconnectedWhileExecuting();
		
		//stop camera (storing value or not)
		if(currentEventExecute.Cancel || currentRunInterval == null)
			webcamEnd (Constants.TestTypes.RUN_I, -1);
		else
			webcamEnd (Constants.TestTypes.RUN_I, currentRunInterval.UniqueID);

		//delete the temp tables if exists
		Sqlite.DeleteTempEvents("tempRunInterval");

		event_execute_drawingarea_realtime_capture_cairo.QueueDraw();

		//Cairo graph is not updated if window is not resized, so force update
		updateGraphRunsInterval();

		if(compujumpAutologout != null)
			compujumpAutologout.EndCapturingRunInterval();
	}

	private void calculateSprintAndUpload()
	{
		string positions = RunInterval.GetSprintPositions(
				currentRunInterval.DistanceInterval, //distanceInterval. == -1 means variable distances
				currentRunInterval.IntervalTimesString,
				currentRunIntervalType.DistancesString 	//distancesString
				);
		if(positions == "")
			return;

		positions = Util.ChangeChars(positions, ",", ".");
		positions = "0;" + positions;

		string splitTimes = RunInterval.GetSplitTimes(currentRunInterval.IntervalTimesString, preferences.digitsNumber);
		splitTimes = Util.ChangeChars(splitTimes, ",", ".");
		splitTimes = "0;" + splitTimes;

		sprintRGraph = new SprintRGraph (positions,
				splitTimes,
				currentPersonSession.Weight, //TODO: can be more if extra weight
				currentPersonSession.Height,
				currentPerson.Name,
				25);

		bool sprintRDoneOk = on_button_sprint_do ();
		string stringResultsFile = RunInterval.GetCSVResultsURL();
		string line = "";
		if(! sprintRDoneOk || ! File.Exists(stringResultsFile))
			return;

		string contents = Util.ReadFile(stringResultsFile, false);
		if (contents == null)
			return;

		using (StringReader reader = new StringReader (contents))
		{
			line = reader.ReadLine ();	//headers
			if(line == null)
				return;

			line = reader.ReadLine ();	//data
			if(line == null)
				return;
		}

		//"";"Mass";"Height";"Temperature";"Vw";"Ka";"K.fitted";"Vmax.fitted";"amax.fitted";"fmax.fitted";"fmax.rel.fitted";"sfv.fitted";"sfv.rel.fitted";"sfv.lm";"sfv.rel.lm";"pmax.fitted";"pmax.rel.fitted";"tpmax.fitted";"F0";"F0.rel";"V0";"pmax.lm";"pmax.rel.lm"

		string [] results = line.Split(new char[] {';'});
		if(results.Length < 14)
			return;

		double k = Convert.ToDouble(Util.ChangeDecimalSeparator(results[6])); //K.fitted
		double vmax = Convert.ToDouble(Util.ChangeDecimalSeparator(results[7])); //Vmax.fitted
		double amax = Convert.ToDouble(Util.ChangeDecimalSeparator(results[8])); //amax.fitted
		double fmax = Convert.ToDouble(Util.ChangeDecimalSeparator(results[10])); //fmax.rel.fitted
		double pmax = Convert.ToDouble(Util.ChangeDecimalSeparator(results[16])); //pmax.rel.fitted

		UploadSprintDataObject usdo = new UploadSprintDataObject(
				-1, //uniqueID
				currentPerson.UniqueID,
				sprintRGraph.Positions, sprintRGraph.GetSplitTimesAsList(),
				k, vmax, amax, fmax, pmax);

		JsonCompujump js = new JsonCompujump(configChronojump.CompujumpDjango);
		if( ! js.UploadSprintData(usdo) )
		{
			LogB.Error(js.ResultMessage);
			
			//since 2.1.3 do not store in Temp, if there are network errors, it is not going to be uploaded later, because wristbands can be re-assigned
			//SqliteJson.InsertTempSprint(false, usdo); //insert only if couldn't be uploaded
		}
	}

	/* ---------------------------------------------------------
	 * ----------------  REACTION TIMES EXECUTION --------------
	 *  --------------------------------------------------------
	 */

	
	//suitable for reaction times
	private void on_reaction_time_activate (bool canCaptureC)
	{
		//used by cancel and finish
		currentEventType = new ReactionTimeType();
			
		//hide jumping buttons
		sensitiveGuiEventDoing(false);

		//show the event doing window
		double progressbarLimit = 2;
			
		//don't let update until test finishes
		if(createdStatsWin)
			showUpdateStatsAndHideData(false);

		event_execute_initializeVariables(
			! canCaptureC,	//is simulated
			currentPerson.UniqueID, 
			currentPerson.Name, 
			Catalog.GetString("Phases"),  	  //name of the different moments
			Constants.ReactionTimeTable, //tableName
			"" 
			);
		event_execute_button_cancel.Sensitive = true;

		ExecutingGraphData egd = event_execute_prepareForTest ();

		event_execute_ButtonCancel.Clicked += new EventHandler(on_cancel_clicked);
		event_execute_ButtonFinish.Clicked += new EventHandler(on_finish_clicked);

		string sep = "";
		string description = "";
		if(extra_window_radio_reaction_time_discriminative.Active) {
			if(check_reaction_time_disc_red.Active == true) {
				description += sep + Catalog.GetString("red");
				sep = " + ";
			}
			if(check_reaction_time_disc_yellow.Active == true) {
				description += sep + Catalog.GetString("yellow");
				sep = " + ";
			}
			if(check_reaction_time_disc_green.Active == true) {
				description += sep + Catalog.GetString("green");
				sep = " + ";
			}
			if(check_reaction_time_disc_buzzer.Active == true) {
				description += sep + Catalog.GetString("buzzer");
				sep = " + ";
			}
		}
		else if(extra_window_radio_reaction_time_animation_lights.Active)
			description = spinbutton_flicker_lights_speed.Value.ToString() + " - " + label_animation_lights_interval.Text;

		currentEventExecute = new ReactionTimeExecute(currentPerson.UniqueID, currentPerson.Name, 
				currentSession.UniqueID, currentReactionTimeType.Name, 
				cp2016.CP, preferences.digitsNumber,
				preferences.volumeOn, preferences.gstreamer,
				progressbarLimit, egd, description);

		if (! canCaptureC)
			currentEventExecute.SimulateInitValues(rand);
	
		//not on simulated because results would be always 0
		if( canCaptureC &&
				(extra_window_radio_reaction_time_discriminative.Active || 
				 extra_window_radio_reaction_time_animation_lights.Active) )
			//TODO: do also for flickr
			currentEventExecute.StartIn = false;
		
		currentEventExecute.FakeButtonReactionTimeStart.Clicked += new EventHandler(on_event_execute_reaction_time_start);

		contactsShowCaptureDoingButtons(true);
		currentEventExecute.Manage(); //check that platform is ok
		
		currentEventExecute.FakeButtonUpdateGraph.Clicked += 
			new EventHandler(on_event_execute_update_graph_in_progress_clicked);
		currentEventExecute.FakeButtonThreadDyed.Clicked += new EventHandler(on_test_finished_can_touch_gtk);
	}	

	private void on_event_execute_reaction_time_start (object o, EventArgs args) 
	{
		currentEventExecute.FakeButtonReactionTimeStart.Clicked -= new EventHandler(on_event_execute_reaction_time_start);

		//Fire leds or buzzer on discriminative (if not simulated)
		if(cp2016.StoredCanCaptureContacts)
		{
			if(extra_window_radio_reaction_time_discriminative.Active) {
				Thread.Sleep(Convert.ToInt32(discriminativeStartTime * 1000)); //in ms

				ChronopicAuto cs = new ChronopicStartReactionTimeAnimation();
				cs.CharToSend = discriminativeCharToSend;
				cs.Write(cp2016.SP, 0);
			}
			else if(extra_window_radio_reaction_time_animation_lights.Active) {
				int speed = Convert.ToInt32(spinbutton_animation_lights_speed.Value);
				ChronopicAuto cs = new ChronopicStartReactionTimeAnimation();
				cs.CharToSend = "l";
				cs.Write(cp2016.SP,speed);
			}

			LogB.Information("on_event_execute_reaction_time_start check if need to open SP");
			if(! cp2016.SP.IsOpen) {
				LogB.Information("opening SP...");
				cp2016.SP.Open();
			}

			/*
			 * some machines needed to flush
			 * - my Linux laptop two bytes
			 * - a linux guest on windows host (virtual box) don't need
			 * Note this will not allow reaction time be lower than 100 ms (DefaultTimeout on chronopic.cs)
			 */
			LogB.Information("Going to flush by time out");	//needed on some machines
			cp2016.CP.FlushByTimeOut();
			LogB.Information("flushed!");	
		}

		contactsShowCaptureDoingButtons(true);
		currentEventExecute.Manage2();
	}


	private void on_reaction_time_finished ()
	{
		//test can be deleted if not cancelled
		sensitiveLastTestButtons(! currentEventExecute.Cancel);

		if ( ! currentEventExecute.Cancel ) {

			currentReactionTime = (ReactionTime) currentEventExecute.EventDone;
			
			myTreeViewReactionTimes.Add(currentPerson.Name, currentReactionTime);

			//since 0.7.4.1 when test is done, treeview select it. action event button have to be shown 
			//this has to be after webcamRecordEnd in order to see if video is created
			showHideActionEventButtons(true, "ReactionTime"); //show
		
			if(createdStatsWin) {
				showUpdateStatsAndHideData(true);
			}
		
			//unhide buttons for delete last reaction time
			sensitiveGuiYesEvent();
		}
		else if( currentEventExecute.ChronopicDisconnected )
			chronopicDisconnectedWhileExecuting();

		//stop camera (storing value or not)
		if(currentEventExecute.Cancel || currentReactionTime == null)
			webcamEnd (Constants.TestTypes.RT, -1);
		else
			webcamEnd (Constants.TestTypes.RT, currentReactionTime.UniqueID);

	}

	/* ---------------------------------------------------------
	 * ----------------  PULSES EXECUTION ----------------------
	 *  --------------------------------------------------------
	 */

	private void on_pulse_activate (bool canCaptureC)
	{
		LogB.Information("pulse accepted");
	
		double pulseStep = 0;
		int totalPulses = 0;

		if(currentPulseType.Name == "Free") {
			pulseStep = currentPulseType.FixedPulse; // -1
			totalPulses = currentPulseType.TotalPulsesNum; //-1
		} else { //custom (info comes from Extra Window
			pulseStep = extra_window_pulses_spinbutton_pulse_step.Value;
			if(extra_window_pulses_checkbutton_unlimited.Active)
				totalPulses = currentPulseType.TotalPulsesNum; //-1
			else
				totalPulses = Convert.ToInt32(
						extra_window_pulses_spinbutton_total_pulses.Value); //-1: unlimited; or 'n': limited by 'n' pulses
		}

		//used by cancel and finish
		//currentEventType = new PulseType();
		currentEventType = currentPulseType;
			
		//hide pulse buttons
		sensitiveGuiEventDoing(false);
		
		//don't let update until test finishes
		if(createdStatsWin)
			showUpdateStatsAndHideData(false);

		//show the event doing window
		event_execute_initializeVariables(
			! canCaptureC,	//is simulated
			currentPerson.UniqueID, 
			currentPerson.Name, 
			Catalog.GetString("Pulses"),  	  //name of the different moments
			Constants.PulseTable, //tableName
			currentPulseType.Name 
			);
		event_execute_button_cancel.Sensitive = true;

		ExecutingGraphData egd = event_execute_prepareForTest ();

		event_execute_ButtonCancel.Clicked += new EventHandler(on_cancel_clicked);
		event_execute_ButtonFinish.Clicked += new EventHandler(on_finish_clicked);

		currentEventExecute = new PulseExecute(currentPerson.UniqueID, currentPerson.Name, 
				currentSession.UniqueID, currentPulseType.Name, pulseStep, totalPulses, 
				cp2016.CP, preferences.digitsNumber,
				preferences.volumeOn, preferences.gstreamer, egd);
		
		if(! canCaptureC)
			currentEventExecute.SimulateInitValues(rand);
		
		contactsShowCaptureDoingButtons(true);
		currentEventExecute.Manage();
		
		currentEventExecute.FakeButtonUpdateGraph.Clicked += 
			new EventHandler(on_event_execute_update_graph_in_progress_clicked);
		currentEventExecute.FakeButtonThreadDyed.Clicked += new EventHandler(on_test_finished_can_touch_gtk);
	}

	private void on_pulse_finished ()
	{
		LogB.Information("pulse finished");
		
		//test can be deleted if not cancelled
		sensitiveLastTestButtons(! currentEventExecute.Cancel);

		if ( ! currentEventExecute.Cancel ) {
			/*
			 * CURRENTLY NOT NEEDED... check
			//if user clicked in finish earlier
			if(currentPulse.Finish) {
				currentRunInterval.Tracks = Util.GetNumberOfJumps(currentRunInterval.IntervalTimesString, false);
				if(currentRunInterval.TracksLimited) {
					currentRunInterval.Limited = currentRunInterval.Tracks.ToString() + "R";
				} else {
					currentRunInterval.Limited = Util.GetTotalTime(
							currentRunInterval.IntervalTimesString) + "T";
				}
			}
			*/
			
			currentPulse = (Pulse) currentEventExecute.EventDone;
			
			myTreeViewPulses.Add(currentPerson.Name, currentPulse);

			//since 0.7.4.1 when test is done, treeview select it. action event button have to be shown 
			//this has to be after webcamRecordEnd in order to see if video is created
			showHideActionEventButtons(true, "Pulse"); //show
			
			if(createdStatsWin) {
				showUpdateStatsAndHideData(true);
			}
			
			//unhide buttons for delete last jump
			sensitiveGuiYesEvent();

			//put correct time value in eventWindow (put the time from chronopic and not onTimer soft chronometer)
			event_execute_LabelTimeValue = Util.GetTotalTime(currentPulse.TimesString);
		}
		else if( currentEventExecute.ChronopicDisconnected )
			chronopicDisconnectedWhileExecuting();

		//stop camera (storing value or not)
		if(currentEventExecute.Cancel || currentPulse == null)
			webcamEnd (Constants.TestTypes.PULSE, -1);
		else
			webcamEnd (Constants.TestTypes.PULSE, currentPulse.UniqueID);

	}

	/* ---------------------------------------------------------
	 * ----------------  MULTI CHRONOPIC EXECUTION -------------
	 *  --------------------------------------------------------
	 */

	//recreate is used when a Chronopic was disconnected
	//
	//encoderPort is usually "" and will be Util.GetDefaultPort
	//but, since 1.5.1 when selecting encoder option from main menu,
	//then encoderPort will be found and send here

	/*
	//normal call
	private void createChronopicWindow(bool recreate, string encoderPort) 
	{
		ArrayList cpd = new ArrayList();
		for(int i=1; i<=4;i++) {
			ChronopicPortData cpdata = new ChronopicPortData(i,"",false);
			cpd.Add(cpdata);
		}
		createChronopicWindow(null, cpd, recreate, encoderPort);
	}
	//called directly on autodetect (detected cp and cpd is send)
	private void createChronopicWindow(Chronopic cp, ArrayList cpd, bool recreate, string encoderPort) 
	{
		if(encoderPort == "")
			encoderPort = Util.GetDefaultPort();

		chronopicWin = ChronopicWindow.Create(cp, cpd, encoderPort, recreate, preferences.volumeOn);
		//chronopicWin.FakeButtonCancelled.Clicked += new EventHandler(on_chronopic_window_cancelled);
		
		if(notebook_sup.CurrentPage == 1) {
			int cps = chronopicWin.NumConnected();
			LogB.Debug("cps: " + cps.ToString());
			chronopicContactsLabels(cps, recreate);
		}
		else //(notebook_sup.CurrentPage == 2)
			chronopicEncoderLabels(recreate);
		
		if(recreate)	
			label_chronopics_multitest.Text = "";
	}
	*/

	private void on_chronopic_contacts_clicked (object o, EventArgs args)
	{
		/*
		ChronopicWindow.ChronojumpMode cmode = ChronopicWindow.ChronojumpMode.JUMPORRUN;
		if(current_mode == Constants.Modes.OTHER)
			cmode = ChronopicWindow.ChronojumpMode.OTHER;

		chronopicWin = ChronopicWindow.View(cmode, preferences.volumeOn);
		//chronopicWin.FakeWindowReload.Clicked += new EventHandler(chronopicWindowReload);
		chronopicWin.FakeWindowDone.Clicked += new EventHandler(on_chronopic_window_contacts_connected_or_done);
		*/

		//TODO: on Windows need to close the sp if it's open, and maybe the cp
		chronopicRegisterUpdate(true);
	}

	private void on_chronopic_encoder_clicked (object o, EventArgs args) {
		/*
		chronopicWin = ChronopicWindow.View(ChronopicWindow.ChronojumpMode.ENCODER, preferences.volumeOn);
		//chronopicWin.FakeWindowReload.Clicked += new EventHandler(chronopicWindowReload);
		chronopicWin.FakeWindowDone.Clicked += new EventHandler(on_chronopic_window_encoder_connected_or_done);
		*/

		//TODO: on Windows need to close the sp if it's open, and maybe the cp

		chronopicRegisterUpdate(true);
	}

	private void on_button_activate_chronopics_networks_problems_clicked (object o, EventArgs args)
	{
		chronopicRegisterUpdate(true);
	}
	
	/*	
	private void on_chronopic_window_cancelled (object o, EventArgs ags) {
		chronopicWin.FakeButtonCancelled.Clicked -= new EventHandler(on_chronopic_window_cancelled);
		chronopicCancelledTimes ++;
	}
	*/

	/*	
	private void chronopicWindowReload(object o, EventArgs args) {
		//chronopicWin.FakeWindowReload.Clicked -= new EventHandler(chronopicWindowReload);

		//store ports info and update labels if necessary
		on_chronopic_window_connected_or_done (o, args);

		//create chronopic window again (maybe new ports)
		//createChronopicWindow(true);

		//show it
		chronopicWin = ChronopicWindow.View(preferences.volumeOn);
	}
	*/

	private void on_chronopic_window_contacts_connected_or_done (object o, EventArgs args)
	{
		/*
		chronopicWin.FakeWindowDone.Clicked -= new EventHandler(on_chronopic_window_contacts_connected_or_done);
		int cps = chronopicWin.NumConnected();

		if(radio_mode_multi_chronopic_small.Active)	
			on_extra_window_multichronopic_test_changed(new object(), new EventArgs());
		
		if(cps > 0)
			change_multitest_firmware(getMenuItemMode());
		else 
			label_chronopics_multitest.Text = "";
		
		chronopicContactsLabels(cps, true);
		*/
	}
	
	private void on_chronopic_window_encoder_connected_or_done (object o, EventArgs args)
	{
		/*
		chronopicWin.FakeWindowDone.Clicked -= new EventHandler(on_chronopic_window_encoder_connected_or_done);

		chronopicEncoderLabels(true);
		*/
	}


	private void chronopicContactsLabels(int cps, bool colorize) {
		/*
		//string text = "<b>" + cps.ToString() + "</b>";
		string text = cps.ToString();
		
		label_connected_chronopics.Text = text;
		//label_connected_chronopics.UseMarkup = true; 
		
		LogB.Debug("cpwin connected: " + chronopicWin.Connected.ToString());	
		if(colorize)
			UtilGtk.ChronopicColors(viewport_chronopics, 
					label_chronopics, label_connected_chronopics, 
					chronopicWin.Connected);
					*/
	}

	private void chronopicEncoderLabels(bool colorize)
	{
		/*
		LogB.Information("at chronopicEncoderLabels");
		string encoderPort = chronopicWin.GetEncoderPort();
		LogB.Debug("gui/chronojump.cs encoderPort:", encoderPort);

		if(encoderPort != null && encoderPort != "" && encoderPort != Util.GetDefaultPort())
		{
			label_chronopic_encoder.Text = Catalog.GetString("Encoder connected");
			image_chronopic_encoder_no.Visible = false;
			image_chronopic_encoder_yes.Visible = true;
		}
		else {
			label_chronopic_encoder.Text = Catalog.GetString("Encoder disconnected");
			image_chronopic_encoder_no.Visible = true;
			image_chronopic_encoder_yes.Visible = false;
		}
		
		if(colorize)
			UtilGtk.ChronopicColors(viewport_chronopic_encoder, 
					label_chronopic_encoder, new Gtk.Label(),
					encoderPort != "");

		LogB.Information("at chronopicEncoderLabels end");
		*/
	}


	private void on_multi_chronopic_start_clicked (bool canCaptureC)
	{
		//new DialogMessage(Constants.MessageTypes.WARNING, "Disabled on version 1.6.3.");
		//return;

		LogB.Information("multi chronopic accepted");
		
		bool syncAvailable = false;
		if(currentMultiChronopicType.SyncAvailable && extra_window_check_multichronopic_sync.Active)
			syncAvailable = true;


		//used by cancel and finish
		currentEventType = new MultiChronopicType();
			
		//hide pulse buttons
		sensitiveGuiEventDoing(false);
		
		//don't let update until test finishes
		if(createdStatsWin)
			showUpdateStatsAndHideData(false);

		//show the event doing window
		event_execute_initializeVariables(
			! canCaptureC,	//is simulated
			currentPerson.UniqueID, 
			currentPerson.Name, 
			Catalog.GetString("Changes"),  	  //name of the different moments
			Constants.MultiChronopicTable, //tableName
			currentMultiChronopicType.Name
			); 
		event_execute_button_cancel.Sensitive = true;

		ExecutingGraphData egd = event_execute_prepareForTest ();

		event_execute_ButtonCancel.Clicked += new EventHandler(on_cancel_multi_clicked);
		event_execute_ButtonFinish.Clicked += new EventHandler(on_finish_multi_clicked);

		//bool syncAvailable = false;
		//if(currentMultiChronopicType.SyncAvailable && extra_window_check_multichronopic_sync.Active)
		//	syncAvailable = true;

		currentEventExecute = new MultiChronopicExecute(
				currentPerson.UniqueID, currentPerson.Name,
				currentSession.UniqueID, currentMultiChronopicType.Name,
				cp2016.CP, cp2016.CP2,
				syncAvailable, extra_window_check_multichronopic_delete_first.Active,
				extra_window_spin_run_analysis_distance.Value.ToString(), egd
				);

		//mark to only get inside on_multi_chronopic_finished one time
		multiFinishing = false;
		contactsShowCaptureDoingButtons(true);
		currentEventExecute.Manage();

		currentEventExecute.FakeButtonUpdateGraph.Clicked += 
			new EventHandler(on_event_execute_update_graph_in_progress_clicked);
//		currentEventExecute.FakeButtonRunATouchPlatform.Clicked += new EventHandler(on_event_execute_RunATouchPlatform);
		currentEventExecute.FakeButtonThreadDyed.Clicked += new EventHandler(on_test_finished_can_touch_gtk);
	}

	bool multiFinishing;
	private void on_multi_chronopic_finished ()
	{
		if(multiFinishing)
			return;
		else
			multiFinishing = true;

		//test can be deleted if not cancelled
		sensitiveLastTestButtons(! currentEventExecute.Cancel);

		if(currentMultiChronopicType.Name == Constants.RunAnalysisName && ! currentEventExecute.MultiChronopicRunAUsedCP2()) 
			//new DialogMessage(Constants.MessageTypes.WARNING, 
			//		Catalog.GetString("This Run Analysis is not valid because there are no strides."));
			currentEventExecute.RunANoStrides();
		else if ( ! currentEventExecute.Cancel ) {
LogB.Debug("mc finished 0");

			   //on runAnalysis test, when cp1 ends, run ends,
			   //but cp2 is still waiting event
			   //with this will ask cp2 to press button
			   //solves problem with threads at ending

			//on_finish_multi_clicked(o, args);
			//this produces also a crash:
			//new DialogMessage(Constants.MessageTypes.INFO, "Please, touch a platform now.");
LogB.Debug("mc finished 1");
			//call write here, because if done in execute/MultiChronopic, will be called n times if n chronopics are working
			currentEventExecute.MultiChronopicWrite(false);
LogB.Debug("mc finished 2");
			currentMultiChronopic = (MultiChronopic) currentEventExecute.EventDone;
LogB.Debug("mc finished 3");
			//this produces also a crash:
			//new DialogMessage(Constants.MessageTypes.INFO, "Please, touch a platform now.");

LogB.Debug("mc finished 4");
			
			//if this multichronopic has more chronopics than other in session, then reload treeview, else simply add
			if(currentMultiChronopic.CPs() != SqliteMultiChronopic.MaxCPs(false, currentSession.UniqueID)) {
				treeview_multi_chronopic_storeReset(false);
				fillTreeView_multi_chronopic();
			} else
				myTreeViewMultiChronopic.Add(currentPerson.Name, currentMultiChronopic);
LogB.Debug("mc finished 5");

			//since 0.7.4.1 when test is done, treeview select it. action event button have to be shown 
			//this has to be after webcamRecordEnd in order to see if video is created
			showHideActionEventButtons(true, Constants.MultiChronopicName); //show
		
			//unhide buttons for delete last test
			sensitiveGuiYesEvent();
		}
		else if( currentEventExecute.ChronopicDisconnected )
			chronopicDisconnectedWhileExecuting();


		//stop camera (storing value or not)
		if(currentEventExecute.Cancel || currentMultiChronopic == null)
			webcamEnd (Constants.TestTypes.MULTICHRONOPIC, -1);
		else
			webcamEnd (Constants.TestTypes.MULTICHRONOPIC, currentMultiChronopic.UniqueID);
	}
		

	/* ---------------------------------------------------------
	 * ----------------  EVENTS EDIT ---------------------------
	 *  --------------------------------------------------------
	 */

	int eventOldPerson;

	private void on_edit_selected_jump_clicked (object o, EventArgs args) {
		//notebooks_change(0); see "notebooks_change sqlite problem"
		LogB.Information("Edit selected jump (normal)");
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person (check also if it's not a individual RJ, the pass the parent RJ)
		if (myTreeViewJumps.EventSelectedID > 0) {
			//3.- obtain the data of the selected jump
			Jump myJump = SqliteJump.SelectJumpData( myTreeViewJumps.EventSelectedID, false );
			eventOldPerson = myJump.PersonID;
		
			//4.- edit this jump
			editJumpWin = EditJumpWindow.Show(app1, myJump, preferences.weightStatsPercent, preferences.digitsNumber);
			editJumpWin.Button_accept.Clicked += new EventHandler(on_edit_selected_jump_accepted);
		}
	}
	
	private void on_edit_selected_jump_rj_clicked (object o, EventArgs args) {
		//notebooks_change(1); see "notebooks_change sqlite problem"
		LogB.Information("Edit selected jump (RJ)");
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person (check also if it's not a individual RJ, the pass the parent RJ)
		if (myTreeViewJumpsRj.EventSelectedID > 0) {
			//3.- obtain the data of the selected jump
			JumpRj myJump = SqliteJumpRj.SelectJumpData( "jumpRj", myTreeViewJumpsRj.EventSelectedID, false );
			eventOldPerson = myJump.PersonID;
		
			//4.- edit this jump
			editJumpRjWin = EditJumpRjWindow.Show(app1, myJump, preferences.weightStatsPercent, preferences.digitsNumber);
			editJumpRjWin.Button_accept.Clicked += new EventHandler(on_edit_selected_jump_rj_accepted);
		}
	}
	
	private void on_edit_selected_jump_accepted (object o, EventArgs args) {
		LogB.Information("edit selected jump accepted");
	
		Jump myJump = SqliteJump.SelectJumpData( myTreeViewJumps.EventSelectedID, false );

		//if person changed, fill treeview again, if not, only update it's line
		if(eventOldPerson == myJump.PersonID) {
			if(! preferences.weightStatsPercent) {
				double personWeight = SqlitePersonSession.SelectAttribute(
						false, myJump.PersonID, currentSession.UniqueID, Constants.Weight);
				myJump.Weight = Util.WeightFromPercentToKg(myJump.Weight, personWeight);
			}
			myTreeViewJumps.Update(myJump);
		}
		else
			pre_fillTreeView_jumps(false);

		if(! configChronojump.Exhibition)
			updateGraphJumpsSimple();

		if(createdStatsWin) 
			stats_win_fillTreeView_stats(false, false);
	}
	
	private void on_edit_selected_jump_rj_accepted (object o, EventArgs args) {
		LogB.Information("edit selected jump RJ accepted");
	
		JumpRj myJump = SqliteJumpRj.SelectJumpData( "jumpRj", myTreeViewJumpsRj.EventSelectedID, false );
		
		//if person changed, fill treeview again, if not, only update it's line
		if(eventOldPerson == myJump.PersonID) {
			if(! preferences.weightStatsPercent) {
				double personWeight = SqlitePersonSession.SelectAttribute(
						false, myJump.PersonID, currentSession.UniqueID, Constants.Weight);
				myJump.Weight = Util.WeightFromPercentToKg(myJump.Weight, personWeight);
			}
			myTreeViewJumpsRj.Update(myJump);
		}
		else
			pre_fillTreeView_jumps_rj(false);

		updateGraphJumpsReactive();

		if(createdStatsWin) 
			stats_win_fillTreeView_stats(false, false);
	}
	
	private void on_edit_selected_run_clicked (object o, EventArgs args) {
		//notebooks_change(2); see "notebooks_change sqlite problem"
		LogB.Information("Edit selected run (normal)");
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person (check also if it's not a individual RJ, the pass the parent RJ)
		if (myTreeViewRuns.EventSelectedID > 0) {
			//3.- obtain the data of the selected run
			Run myRun = SqliteRun.SelectRunData( myTreeViewRuns.EventSelectedID, false );
			myRun.MetersSecondsPreferred = preferences.metersSecondsPreferred;
			eventOldPerson = myRun.PersonID;
		
			//4.- edit this run
			editRunWin = EditRunWindow.Show(app1, myRun, preferences.digitsNumber, preferences.metersSecondsPreferred);
			editRunWin.Button_accept.Clicked += new EventHandler(on_edit_selected_run_accepted);
		}
	}
	
	private void on_edit_selected_run_interval_clicked (object o, EventArgs args) {
		//notebooks_change(3); see "notebooks_change sqlite problem"
		LogB.Information("Edit selected run interval");
		//1.- check that there's a line selected
		//2.- check that this line is a run and not a person (check also if it's not a individual subrun, the pass the parent run)
		if (myTreeViewRunsInterval.EventSelectedID > 0) {
			//3.- obtain the data of the selected run
			RunInterval myRun = SqliteRunInterval.SelectRunData( Constants.RunIntervalTable, myTreeViewRunsInterval.EventSelectedID, false );
			eventOldPerson = myRun.PersonID;
		
			//4.- edit this run
			editRunIntervalWin = EditRunIntervalWindow.Show(app1, myRun, preferences.digitsNumber, preferences.metersSecondsPreferred);
			editRunIntervalWin.Button_accept.Clicked += new EventHandler(on_edit_selected_run_interval_accepted);
		}
	}
	
	private void on_edit_selected_run_accepted (object o, EventArgs args) {
		LogB.Information("edit selected run accepted");
		
		Run myRun = SqliteRun.SelectRunData( myTreeViewRuns.EventSelectedID, false );
		
		//if person changed, fill treeview again, if not, only update it's line
		if(eventOldPerson == myRun.PersonID)
			myTreeViewRuns.Update(myRun);
		else
			pre_fillTreeView_runs(false);

		updateGraphRunsSimple();

		if(createdStatsWin) 
			stats_win_fillTreeView_stats(false, false);
	}
	
	private void on_edit_selected_run_interval_accepted (object o, EventArgs args) {
		LogB.Information("edit selected run interval accepted");
		
		RunInterval myRun = SqliteRunInterval.SelectRunData( Constants.RunIntervalTable, myTreeViewRunsInterval.EventSelectedID, false );

		//if person changed, fill treeview again, if not, only update it's line
		if(eventOldPerson == myRun.PersonID)
			myTreeViewRunsInterval.Update(myRun);
		else
			pre_fillTreeView_runs_interval(false);

		if(createdStatsWin)
			stats_win_fillTreeView_stats(false, false);
	}

	private void on_edit_selected_reaction_time_clicked (object o, EventArgs args) {
		//notebooks_change(4); see "notebooks_change sqlite problem"
		LogB.Information("Edit selected reaction time");
		//1.- check that there's a line selected
		//2.- check that this line is a event and not a person
		if (myTreeViewReactionTimes.EventSelectedID > 0) {
			//3.- obtain the data of the selected event
			ReactionTime myRT = SqliteReactionTime.SelectReactionTimeData( myTreeViewReactionTimes.EventSelectedID, false );
			eventOldPerson = myRT.PersonID;
		
			//4.- edit this event
			editReactionTimeWin = EditReactionTimeWindow.Show(app1, myRT, preferences.digitsNumber);
			editReactionTimeWin.Button_accept.Clicked += new EventHandler(on_edit_selected_reaction_time_accepted);
		}
	}
	
	private void on_edit_selected_reaction_time_accepted (object o, EventArgs args) {
		LogB.Information("edit selected reaction time accepted");
		
		ReactionTime myRT = SqliteReactionTime.SelectReactionTimeData( myTreeViewReactionTimes.EventSelectedID, false);

		//if person changed, fill treeview again, if not, only update it's line
		if(eventOldPerson == myRT.PersonID)
			myTreeViewReactionTimes.Update(myRT);
		else {
			treeview_reaction_times_storeReset();
			fillTreeView_reaction_times(currentReactionTimeType.Name);
		}
		
		updateGraphReactionTimes();
	}
	
	private void on_edit_selected_pulse_clicked (object o, EventArgs args) {
		//notebooks_change(5); see "notebooks_change sqlite problem"
		LogB.Information("Edit selected pulse");
		//1.- check that there's a line selected
		//2.- check that this line is a event and not a person
		if (myTreeViewPulses.EventSelectedID > 0) {
			//3.- obtain the data of the selected event
			Pulse myPulse = SqlitePulse.SelectPulseData( myTreeViewPulses.EventSelectedID, false );
			eventOldPerson = myPulse.PersonID;
		
			//4.- edit this event
			editPulseWin = EditPulseWindow.Show(app1, myPulse, preferences.digitsNumber);
			editPulseWin.Button_accept.Clicked += new EventHandler(on_edit_selected_pulse_accepted);
		}
	}
	
	private void on_edit_selected_pulse_accepted (object o, EventArgs args) {
		/*
		LogB.Information("edit selected pulse accepted");
		
		Pulse myPulse = SqlitePulse.SelectPulseData( myTreeViewPulses.EventSelectedID, false );

		//if person changed, fill treeview again, if not, only update it's line
		if(eventOldPerson == myPulse.PersonID)
			myTreeViewPulses.Update(myPulse);
		else {
			treeview_pulses_storeReset();
			fillTreeView_pulses(UtilGtk.ComboGetActive(combo_pulses));
		}
		*/
	}
	
	private void on_edit_selected_multi_chronopic_clicked (object o, EventArgs args) {
		//notebooks_change(6); see "notebooks_change sqlite problem"
		LogB.Information("Edit selected multi chronopic");
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person (check also if it's not a individual RJ, the pass the parent RJ)
		if (myTreeViewMultiChronopic.EventSelectedID > 0) {
			//3.- obtain the data of the selected test
			MultiChronopic mc = SqliteMultiChronopic.SelectMultiChronopicData( myTreeViewMultiChronopic.EventSelectedID, false );
			eventOldPerson = mc.PersonID;
		
			//4.- edit this jump
			editMultiChronopicWin = EditMultiChronopicWindow.Show(app1, mc, preferences.digitsNumber);
			editMultiChronopicWin.Button_accept.Clicked += new EventHandler(on_edit_selected_multi_chronopic_accepted);
		}
	}

	private void on_edit_selected_multi_chronopic_accepted (object o, EventArgs args) {
		LogB.Information("edit selected multi chronopic accepted");
	
		MultiChronopic mc = SqliteMultiChronopic.SelectMultiChronopicData( myTreeViewMultiChronopic.EventSelectedID, false );
		
		//if person changed, fill treeview again, if not, only update it's line
		if(eventOldPerson == mc.PersonID) 
			myTreeViewMultiChronopic.Update(mc);
		else {
			treeview_multi_chronopic_storeReset(false);
			fillTreeView_multi_chronopic();
		}
	}
	

	/* ---------------------------------------------------------
	 * ----------------  EVENTS DELETE -------------------------
	 *  --------------------------------------------------------
	 */
	
	private void on_delete_last_test_clicked (object o, EventArgs args)
	{
		if(current_mode == Constants.Modes.FORCESENSOR)
		{
			force_sensor_delete_current_test_pre_question();
			return;
		}
		else if(current_mode == Constants.Modes.RUNSENCODER)
		{
			run_encoder_delete_current_test_pre_question();
			return;
		}

		delete_last_test_chronopic_clicked (o, args);
	}
	private void delete_last_test_chronopic_clicked (object o, EventArgs args)
	{
		if(currentEventType == null) //precaution
			return;

		switch (currentEventType.Type)
		{
			case EventType.Types.JUMP:
				if(lastJumpIsSimple) {
					//maybe, after executing the test, user has selected other test on treeview
					//delete this is called on execute
					//we should ensure we are deleting last jump and not the selected jump
					//force selection of last jump
					if(currentJump.UniqueID != myTreeViewJumps.EventSelectedID)
						myTreeViewJumps.SelectEvent(currentJump.UniqueID);
					on_delete_selected_jump_clicked(o, args);
				} else {
					if(currentJumpRj.UniqueID != myTreeViewJumpsRj.EventSelectedID)
						myTreeViewJumpsRj.SelectEvent(currentJumpRj.UniqueID);
					on_delete_selected_jump_rj_clicked(o, args);
				}
				break;
			case EventType.Types.RUN:
				if(lastRunIsSimple) {
					if(currentRun.UniqueID != myTreeViewRuns.EventSelectedID)
						myTreeViewRuns.SelectEvent(currentRun.UniqueID);
					on_delete_selected_run_clicked(o, args);
				} else {
					if(currentRunInterval.UniqueID != myTreeViewRunsInterval.EventSelectedID)
						myTreeViewRunsInterval.SelectEvent(currentRunInterval.UniqueID);
					on_delete_selected_run_interval_clicked(o, args);
				}
				break;
			case EventType.Types.PULSE:
				if(currentPulse.UniqueID != myTreeViewPulses.EventSelectedID)
					myTreeViewPulses.SelectEvent(currentPulse.UniqueID);
				on_delete_selected_pulse_clicked(o, args);
				break;
			case EventType.Types.REACTIONTIME:
				if(currentReactionTime.UniqueID != myTreeViewReactionTimes.EventSelectedID)
					myTreeViewReactionTimes.SelectEvent(currentReactionTime.UniqueID);
				on_delete_selected_reaction_time_clicked(o, args);
				break;
			case EventType.Types.MULTICHRONOPIC:
				if(currentMultiChronopic.UniqueID != myTreeViewMultiChronopic.EventSelectedID)
					myTreeViewMultiChronopic.SelectEvent(currentMultiChronopic.UniqueID);
				on_delete_selected_multi_chronopic_clicked(o, args);
				break;
		}
	}

	private void on_button_inspect_last_test_clicked (object o, EventArgs args)
	{
		if(currentEventExecute == null)
			return;

		//sensitivize gui
		menus_and_mode_sensitive(false);
		hbox_contacts_capture_top.Sensitive = false;
		//button_inspect_last_test.Sensitive = false; //unneeded because it will be on hbox_contacts_capture_top
		hbox_contacts_sup_capture_analyze_two_buttons.Sensitive = false;
		hbox_top_person.Sensitive = false;

		notebook_contacts_execute_or.CurrentPage = Convert.ToInt32(notebook_contacts_execute_or_pages.RACEINSPECTOR);
		label_run_simple_double_contacts.Text = currentEventExecute.GetInspectorMessages();
		label_run_simple_double_contacts.UseMarkup = true;
	}

	private void on_button_race_inspector_close_clicked (object o, EventArgs args)
	{
		//unsensitivize gui
		menus_and_mode_sensitive(true);
		hbox_contacts_capture_top.Sensitive = true;
		//button_inspect_last_test.Sensitive = true; //unneeded because it will be on hbox_contacts_capture_top
		hbox_contacts_sup_capture_analyze_two_buttons.Sensitive = true;
		hbox_top_person.Sensitive = true;

		notebook_contacts_execute_or.CurrentPage = Convert.ToInt32(notebook_contacts_execute_or_pages.EXECUTE);
	}

	private void deleted_last_test_update_widgets() {
		sensitiveLastTestButtons(false);
		UtilGtk.ClearDrawingArea(event_execute_drawingarea, event_execute_pixmap);
	}
	
	private void on_delete_selected_jump_clicked (object o, EventArgs args) {
		//notebooks_change(0); see "notebooks_change sqlite problem"
		LogB.Information("delete this jump (normal)");
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person
		LogB.Information(myTreeViewJumps.EventSelectedID.ToString());
		if (myTreeViewJumps.EventSelectedID > 0) {
			//3.- display confirmwindow of deletion 
			if (preferences.askDeletion) {
				confirmWinJumpRun = ConfirmWindowJumpRun.Show(Catalog.GetString("Do you want to delete this jump?"), "");
				confirmWinJumpRun.Button_accept.Clicked += new EventHandler(on_delete_selected_jump_accepted);
			} else {
				on_delete_selected_jump_accepted(o, args);
			}
		}
	}
	
	private void on_delete_selected_jump_rj_clicked (object o, EventArgs args) {
		//notebooks_change(1); see "notebooks_change sqlite problem"
		LogB.Information("delete this reactive jump");
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person (check also if it's not a individual RJ, the pass the parent RJ)
		if (myTreeViewJumpsRj.EventSelectedID > 0) {
			//3.- display confirmwindow of deletion 
			if (preferences.askDeletion) {
				confirmWinJumpRun = ConfirmWindowJumpRun.Show( Catalog.GetString("Do you want to delete this jump?"), "");
				confirmWinJumpRun.Button_accept.Clicked += new EventHandler(on_delete_selected_jump_rj_accepted);
			} else {
				on_delete_selected_jump_rj_accepted(o, args);
			}
		}
	}
	
	private void on_delete_selected_jump_accepted (object o, EventArgs args) {
		LogB.Information("accept delete this jump");
		int id = myTreeViewJumps.EventSelectedID;
		
		Sqlite.Delete(false, Constants.JumpTable, id);
		
		myTreeViewJumps.DelEvent(id);
		showHideActionEventButtons(false, "Jump");
		
		if(createdStatsWin) {
			stats_win_fillTreeView_stats(false, false);
		}
		Util.DeleteVideo(currentSession.UniqueID, Constants.TestTypes.JUMP, id );
		//we can be here being called from jump treeview (not from execute tab)
		//then what we are deleting is selected jump, not last jump 
		//only if selected is last, then
		//change executing window: drawingarea, button_delete, "deleted test" message
		try {
			if(currentJump.UniqueID == id)
				deleted_last_test_update_widgets();
		} catch {
			//there's no currentJump (no one jumped), then it crashed,
			//but don't need to update widgets
		}
		
		if(! configChronojump.Exhibition)
			updateGraphJumpsSimple();

		//if auto mode, show last person/test again
		if(execute_auto_doing) {
			execute_auto_order_pos --;
			execute_auto_select();
		}
	}

	private void on_delete_selected_jump_rj_accepted (object o, EventArgs args) {
		LogB.Information("accept delete this jump");
		int id = myTreeViewJumpsRj.EventSelectedID;
		
		Sqlite.Delete(false, Constants.JumpRjTable, id);
		
		myTreeViewJumpsRj.DelEvent(id);
		showHideActionEventButtons(false, "JumpRj");

		if(createdStatsWin) {
			stats_win_fillTreeView_stats(false, false);
		}
		Util.DeleteVideo(currentSession.UniqueID, Constants.TestTypes.JUMP_RJ, id );
		try {
			if(currentJumpRj.UniqueID == id)
				deleted_last_test_update_widgets();
		} catch {
			//there's no currentJumpRj (no one jumped), then it crashed,
			//but don't need to update widgets
		}

		updateGraphJumpsReactive();
	}
	
	private void on_delete_selected_run_clicked (object o, EventArgs args) {
		//notebooks_change(2); see "notebooks_change sqlite problem"
		LogB.Information("delete this race (normal)");
		
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person
		if (myTreeViewRuns.EventSelectedID > 0) {
			//3.- display confirmwindow of deletion 
			if (preferences.askDeletion) {
				confirmWinJumpRun = ConfirmWindowJumpRun.Show(Catalog.GetString("Do you want to delete this race?"), "");
				confirmWinJumpRun.Button_accept.Clicked += new EventHandler(on_delete_selected_run_accepted);
			} else {
				on_delete_selected_run_accepted(o, args);
			}
		}
	}
		
	
	private void on_delete_selected_run_interval_clicked (object o, EventArgs args) {
		//notebooks_change(3); see "notebooks_change sqlite problem"
		LogB.Information("delete this race interval");
		//1.- check that there's a line selected
		//2.- check that this line is a run and not a person (check also if it's a subrun, pass the parent run)
		if (myTreeViewRunsInterval.EventSelectedID > 0) {
			//3.- display confirmwindow of deletion 
			if (preferences.askDeletion) {
				confirmWinJumpRun = ConfirmWindowJumpRun.Show(
						Catalog.GetString("Do you want to delete this race?"), "");
				confirmWinJumpRun.Button_accept.Clicked += new EventHandler(on_delete_selected_run_interval_accepted);
			} else {
				on_delete_selected_run_interval_accepted(o, args);
			}
		}
	}

	private void on_delete_selected_run_accepted (object o, EventArgs args) {
		LogB.Information("accept delete this race");
		int id = myTreeViewRuns.EventSelectedID;
		
		Sqlite.Delete(false, Constants.RunTable, id);
		
		myTreeViewRuns.DelEvent(id);
		showHideActionEventButtons(false, "Run");
		
		if(createdStatsWin) {
			stats_win_fillTreeView_stats(false, false);
		}
		Util.DeleteVideo(currentSession.UniqueID, Constants.TestTypes.RUN, id );
		try {
			if(currentRun.UniqueID == id)
				deleted_last_test_update_widgets();
		} catch {
			//there's no currentRun (no one done it now), then it crashed,
			//but don't need to update widgets
		}
		
		updateGraphRunsSimple();
	}

	private void on_delete_selected_run_interval_accepted (object o, EventArgs args) {
		LogB.Information("accept delete this race");
		int id = myTreeViewRunsInterval.EventSelectedID;
		
		Sqlite.Delete(false, Constants.RunIntervalTable, id);
		
		myTreeViewRunsInterval.DelEvent(id);
		showHideActionEventButtons(false, "RunInterval");

		if(createdStatsWin) {
			stats_win_fillTreeView_stats(false, false);
		}
		Util.DeleteVideo(currentSession.UniqueID, Constants.TestTypes.RUN_I, id );
		try {
			if(currentRunInterval.UniqueID == id)
				deleted_last_test_update_widgets();
		} catch {
			//there's no currentRunInterval (no one done it now), then it crashed,
			//but don't need to update widgets
		}
	}
	
	private void on_delete_selected_reaction_time_clicked (object o, EventArgs args) {
		//notebooks_change(4); see "notebooks_change sqlite problem"
		LogB.Information("delete this reaction time");
		
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person
		LogB.Information(myTreeViewReactionTimes.EventSelectedID.ToString());
		if (myTreeViewReactionTimes.EventSelectedID > 0) {
			//3.- display confirmwindow of deletion 
			if (preferences.askDeletion) {
				confirmWinJumpRun = ConfirmWindowJumpRun.Show("Do you want to delete this test?", "");
				confirmWinJumpRun.Button_accept.Clicked += new EventHandler(on_delete_selected_reaction_time_accepted);
			} else {
				on_delete_selected_reaction_time_accepted(o, args);
			}
		}
	}
		
	private void on_delete_selected_reaction_time_accepted (object o, EventArgs args) {
		LogB.Information("accept delete this reaction time");
		int id = myTreeViewReactionTimes.EventSelectedID;
		
		Sqlite.Delete(false, Constants.ReactionTimeTable, id);
		
		myTreeViewReactionTimes.DelEvent(id);
		showHideActionEventButtons(false, "ReactionTime");

		Util.DeleteVideo(currentSession.UniqueID, Constants.TestTypes.RT, id );
		try {
			if(currentReactionTime.UniqueID == id)
				deleted_last_test_update_widgets();
		} catch {
			//there's no currentReactionTime (no one done it now), then it crashed,
			//but don't need to update widgets
		}

		updateGraphReactionTimes();
	}

	private void on_delete_selected_pulse_clicked (object o, EventArgs args) {
		//notebooks_change(5); see "notebooks_change sqlite problem"
		LogB.Information("delete this pulse");
		
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person
		LogB.Information(myTreeViewPulses.EventSelectedID.ToString());
		if (myTreeViewPulses.EventSelectedID > 0) {
			//3.- display confirmwindow of deletion 
			if (preferences.askDeletion) {
				confirmWinJumpRun = ConfirmWindowJumpRun.Show("Do you want to delete this test?", "");
				confirmWinJumpRun.Button_accept.Clicked += new EventHandler(on_delete_selected_pulse_accepted);
			} else {
				on_delete_selected_pulse_accepted(o, args);
			}
		}
	}
		
	private void on_delete_selected_pulse_accepted (object o, EventArgs args) {
		LogB.Information("accept delete this pulse");
		int id = myTreeViewPulses.EventSelectedID;
		
		Sqlite.Delete(false, Constants.PulseTable, id);
		
		myTreeViewPulses.DelEvent(id);
		showHideActionEventButtons(false, "Pulse");

		Util.DeleteVideo(currentSession.UniqueID, Constants.TestTypes.PULSE, id );
		try {
			if(currentPulse.UniqueID == id)
				deleted_last_test_update_widgets();
		} catch {
			//there's no currentPulse (no one done it now), then it crashed,
			//but don't need to update widgets
		}
	}

	private void on_delete_selected_multi_chronopic_clicked (object o, EventArgs args) {
		//notebooks_change(6); see "notebooks_change sqlite problem"
		LogB.Information("delete this multi chronopic");
		//1.- check that there's a line selected
		//2.- check that this line is a test and not a person (check also if it's not a individual mc, then pass the parent mc)
		if (myTreeViewMultiChronopic.EventSelectedID > 0) {
			//3.- display confirmwindow of deletion 
			if (preferences.askDeletion) {
				confirmWinJumpRun = ConfirmWindowJumpRun.Show( Catalog.GetString("Do you want to delete this test?"), "");
				confirmWinJumpRun.Button_accept.Clicked += new EventHandler(on_delete_selected_multi_chronopic_accepted);
			} else {
				on_delete_selected_multi_chronopic_accepted(o, args);
			}
		}
	}
	
	private void on_delete_selected_multi_chronopic_accepted (object o, EventArgs args) {
		LogB.Information("accept delete this multi chronopic");
		int id = myTreeViewMultiChronopic.EventSelectedID;
		
		Sqlite.Delete(false, Constants.MultiChronopicTable, id);
		
		myTreeViewMultiChronopic.DelEvent(id);
		showHideActionEventButtons(false, Constants.MultiChronopicName);
		
		Util.DeleteVideo(currentSession.UniqueID, Constants.TestTypes.MULTICHRONOPIC, id );
		try {
			if(currentMultiChronopic.UniqueID == id)
				deleted_last_test_update_widgets();
		} catch {
			//there's no currentMultiChronopic (no one done it now), then it crashed,
			//but don't need to update widgets
		}
	}
	



	/* ---------------------------------------------------------
	 * ----------------  EVENTS TYPE ADD -----------------------
	 *  --------------------------------------------------------
	 */

	
	private void on_jump_simple_type_add_clicked (object o, EventArgs args) {
		LogB.Information("Add simple new jump type");
			
		jumpTypeAddWin = JumpTypeAddWindow.Show(app1, true); //is simple
		jumpTypeAddWin.FakeButtonAccept.Clicked += new EventHandler(on_jump_type_add_accepted);
	}
	
	private void on_jump_reactive_type_add_clicked (object o, EventArgs args) {
		LogB.Information("Add reactive new jump type");
			
		jumpTypeAddWin = JumpTypeAddWindow.Show(app1, false); //is reactive
		jumpTypeAddWin.FakeButtonAccept.Clicked += new EventHandler(on_jump_type_add_accepted);
	}
	
	private void on_jump_type_add_accepted (object o, EventArgs args) {
		LogB.Information("ACCEPTED Add new jump type");
		if(jumpTypeAddWin.InsertedSimple) {
			createComboSelectJumps(false);
			createComboSelectJumpsDjOptimalFall(false);
			createComboSelectJumpsWeightFVProfile(false);
			createComboSelectJumpsEvolution(false);

			pre_fillTreeView_jumps(false);
			combo_select_jumps.Active = UtilGtk.ComboMakeActive(combo_select_jumps, jumpTypeAddWin.Name);

			new DialogMessage(Constants.MessageTypes.INFO, Catalog.GetString("Added simple jump type."));
		} else {
			createComboSelectJumpsRj(false);
			createComboSelectJumpsRjFatigue(false);
			//createComboSelectJumpsRjFatigueNum(false); do not need because will be updated by createComboSelectJumpsRjFatigue
			
			pre_fillTreeView_jumps_rj(false);
			combo_select_jumps_rj.Active = UtilGtk.ComboMakeActive(combo_select_jumps_rj, jumpTypeAddWin.Name);

			new DialogMessage(Constants.MessageTypes.INFO, Catalog.GetString("Added reactive jump type."));
		}
		updateComboStats();
	}

	private void on_run_simple_type_add_activate (object o, EventArgs args) {
		LogB.Information("Add simple new run type");
			
		runTypeAddWin = RunTypeAddWindow.Show(app1, true); //is simple
		runTypeAddWin.FakeButtonAccept.Clicked += new EventHandler(on_run_type_add_accepted);
	}
	
	private void on_run_intervallic_type_add_activate (object o, EventArgs args) {
		LogB.Information("Add intervallic new run type");
			
		runTypeAddWin = RunTypeAddWindow.Show(app1, false); //is intervallic
		runTypeAddWin.FakeButtonAccept.Clicked += new EventHandler(on_run_type_add_accepted);
	}
	
	private void on_run_type_add_accepted (object o, EventArgs args) {
		LogB.Information("ACCEPTED Add new run type");
		if(runTypeAddWin.InsertedSimple) {
			createComboSelectRuns(false);
			createComboSelectRunsEvolution(false);

			pre_fillTreeView_runs(false);
			combo_select_runs.Active = UtilGtk.ComboMakeActive(combo_select_runs, runTypeAddWin.Name);

			new DialogMessage(Constants.MessageTypes.INFO, Catalog.GetString("Added simple race type."));
		} else {
			createComboSelectRunsInterval(false);
			
			pre_fillTreeView_runs_interval(false);

			combo_select_runs_interval.Active = UtilGtk.ComboMakeActive(combo_select_runs_interval, runTypeAddWin.Name);

			new DialogMessage(Constants.MessageTypes.INFO, Catalog.GetString("Added intervallic race type."));
		}
		updateComboStats();
	}

	//reactiontime has no types

	private void on_pulse_type_add_activate (object o, EventArgs args) {
		LogB.Information("Add new pulse type");
	}
	
	private void on_pulse_type_add_accepted (object o, EventArgs args) {
		LogB.Information("ACCEPTED Add new pulse type");
	}

	/* ---------------------------------------------------------
	 * ----------------  EVENTS TYPE DELETE --------------------
	 *  --------------------------------------------------------
	 */

	private void on_jump_type_delete_simple (object o, EventArgs args) {
		jumpsMoreWin = JumpsMoreWindow.Show(app1, false); //delete jump type
		jumpsMoreWin.Button_deleted_test.Clicked += new EventHandler(on_deleted_jump_type);
	}
	
	private void on_jump_type_delete_reactive (object o, EventArgs args) {
		jumpsRjMoreWin = JumpsRjMoreWindow.Show(app1, false); //delete jump type
		jumpsRjMoreWin.Button_deleted_test.Clicked += new EventHandler(on_deleted_jump_rj_type);
	}
	
	private void on_run_type_delete_simple (object o, EventArgs args) {
		runsMoreWin = RunsMoreWindow.Show(app1, false); //delete run type
		runsMoreWin.Button_deleted_test.Clicked += new EventHandler(on_deleted_run_type);
	}
	
	private void on_run_type_delete_intervallic (object o, EventArgs args) {
		runsIntervalMoreWin = RunsIntervalMoreWindow.Show(app1, false); //delete run type
		runsIntervalMoreWin.Button_deleted_test.Clicked += new EventHandler(on_deleted_run_i_type);
	}
	
	//----
	
	private void on_deleted_jump_type (object o, EventArgs args)
	{
		string translatedName = comboSelectJumps.GetNameTranslated(jumpsMoreWin.SelectedEventName);
		combo_select_jumps = comboSelectJumps.DeleteValue(translatedName);
		pre_fillTreeView_jumps(false);

		extra_window_jumps_initialize(new JumpType("Free"));
	}

	private void on_deleted_jump_rj_type (object o, EventArgs args)
	{
		string translatedName = comboSelectJumpsRj.GetNameTranslated(jumpsRjMoreWin.SelectedEventName);
		combo_select_jumps_rj = comboSelectJumpsRj.DeleteValue(translatedName);
		pre_fillTreeView_jumps_rj(false);

		extra_window_jumps_rj_initialize(new JumpType("RJ(j)"));
	}

	private void on_deleted_run_type (object o, EventArgs args)
	{
		string translatedName = comboSelectRuns.GetNameTranslated(runsMoreWin.SelectedEventName);
		combo_select_runs = comboSelectRuns.DeleteValue(translatedName);
		pre_fillTreeView_runs(false);

		extra_window_runs_initialize(new RunType("Custom"));
	}

	private void on_deleted_run_i_type (object o, EventArgs args)
	{
		string translatedName = comboSelectRunsI.GetNameTranslated(runsIntervalMoreWin.SelectedEventName);
		combo_select_runs_interval = comboSelectRunsI.DeleteValue(translatedName);
		pre_fillTreeView_runs_interval(false);

		extra_window_runs_interval_initialize(new RunType("byLaps"));
	}

	/* ---------------------------------------------------------
	 * ----------------  EVENTS REPAIR -------------------------
	 *  --------------------------------------------------------
	 */
	
	private void on_repair_selected_jump_rj_clicked (object o, EventArgs args) {
		//notebooks_change(1); see "notebooks_change sqlite problem"
		LogB.Information("Repair selected subjump");
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person (check also if it's not a individual RJ, the pass the parent RJ)
		if (myTreeViewJumpsRj.EventSelectedID > 0) {
			//3.- obtain the data of the selected jump
			JumpRj myJump = SqliteJumpRj.SelectJumpData( "jumpRj", myTreeViewJumpsRj.EventSelectedID, false );
		
			//4.- edit this jump
			repairJumpRjWin = RepairJumpRjWindow.Show(app1, myJump, preferences.digitsNumber);
			repairJumpRjWin.Button_accept.Clicked += new EventHandler(on_repair_selected_jump_rj_accepted);
		}
	}
	
	private void on_repair_selected_jump_rj_accepted (object o, EventArgs args) {
		LogB.Information("Repair selected reactive jump accepted");
		
		treeview_jumps_rj_storeReset();
		fillTreeView_jumps_rj(UtilGtk.ComboGetActive(combo_select_jumps_rj));
		
		if(createdStatsWin) {
			stats_win_fillTreeView_stats(false, false);
		}
	}
	
	private void on_repair_selected_run_interval_clicked (object o, EventArgs args) {
		//notebooks_change(3); see "notebooks_change sqlite problem"
		LogB.Information("Repair selected subrun");
		//1.- check that there's a line selected
		//2.- check that this line is a run and not a person 
		//(check also if it's not a individual run interval, then pass the parent run interval)
		if (myTreeViewRunsInterval.EventSelectedID > 0) {
			//3.- obtain the data of the selected run
			RunInterval myRun = SqliteRunInterval.SelectRunData( Constants.RunIntervalTable, myTreeViewRunsInterval.EventSelectedID, false );
		
			//4.- edit this run
			repairRunIntervalWin = RepairRunIntervalWindow.Show(app1, myRun, preferences.digitsNumber);
			repairRunIntervalWin.Button_accept.Clicked += new EventHandler(on_repair_selected_run_interval_accepted);
		}
	}
	
	private void on_repair_selected_run_interval_accepted (object o, EventArgs args) {
		LogB.Information("repair selected run interval accepted");
		
		treeview_runs_interval_storeReset();
		fillTreeView_runs_interval(UtilGtk.ComboGetActive(combo_select_runs_interval));
		createTreeView_runs_interval_sprint (treeview_runs_interval_sprint);
		
		if(createdStatsWin) {
			stats_win_fillTreeView_stats(false, false);
		}
	}

	private void on_repair_selected_pulse_clicked (object o, EventArgs args) {
		//notebooks_change(5); see "notebooks_change sqlite problem"
		LogB.Information("Repair selected pulse");
		//1.- check that there's a line selected
		//2.- check that this line is a pulse and not a person 
		//(check also if it's not a individual pulse, then pass the parent pulse)
		if (myTreeViewPulses.EventSelectedID > 0) {
			//3.- obtain the data of the selected pulse
			Pulse myPulse = SqlitePulse.SelectPulseData( myTreeViewPulses.EventSelectedID, false );
		
			//4.- edit this pulse
			repairPulseWin = RepairPulseWindow.Show(app1, myPulse, preferences.digitsNumber);
			repairPulseWin.Button_accept.Clicked += new EventHandler(on_repair_selected_pulse_accepted);
		}
	}
	
	private void on_repair_selected_pulse_accepted (object o, EventArgs args) {
	/*
		LogB.Information("repair selected pulse accepted");
		
		treeview_pulses_storeReset();
		fillTreeView_pulses(UtilGtk.ComboGetActive(combo_pulses));
		*/
	}

	private void on_repair_selected_multi_chronopic_clicked (object o, EventArgs args) {
		//notebooks_change(6); see "notebooks_change sqlite problem"
		LogB.Information("Repair selected multichronopic");
	}

	/* ---------------------------------------------------------
	 * ----------------  AUTO MODE -----------------------------
	 *  --------------------------------------------------------
	 */

	private void on_button_auto_start_clicked (object o, EventArgs args) {

//TODO: put five buttons in a viewport than can be colorified

		executeAutoWin = ExecuteAutoWindow.Show(app1, currentSession.UniqueID);
		executeAutoWin.FakeButtonAccept.Clicked += new EventHandler(on_button_auto_start_accepted);
	}

	ArrayList execute_auto_order;
	int execute_auto_order_pos;
	bool execute_auto_doing = false;
	private void on_button_auto_start_accepted (object o, EventArgs args) {
		executeAutoWin.FakeButtonAccept.Clicked -= new EventHandler(on_button_auto_start_accepted);

		sensitiveGuiAutoStartEnd (true);
		notebook_jumps_automatic.CurrentPage = 1;
	
		execute_auto_order = executeAutoWin.GetOrderedData();
		execute_auto_order_pos = 0;
		execute_auto_doing = true;

		executeAutoWin.Close();

		execute_auto_select();
	}

	private void execute_auto_select() 
	{
		if(execute_auto_order_pos >= execute_auto_order.Count) {
			on_button_auto_end_clicked (new object (), new EventArgs());
			return;
		}

		ExecuteAuto ea = (ExecuteAuto) execute_auto_order[execute_auto_order_pos];
		int rowToSelect = myTreeViewPersons.FindRow(ea.personUniqueID);
		if(rowToSelect != -1) {
			//this will update also currentPerson
			selectRowTreeView_persons(treeview_persons, rowToSelect);
			label_jump_auto_current_person.Text = currentPerson.Name;

			//select the test
			combo_select_jumps = comboSelectJumps.SelectById(ea.testUniqueID);
			label_jump_auto_current_test.Text = "(" + ea.testTrName + ")";
			
			//put GUI on auto_waiting
			sensitiveGuiAutoExecuteOrWait (false);
		}
	}

	private void on_button_auto_end_clicked (object o, EventArgs args) 
	{
		sensitiveGuiAutoStartEnd (false);
		notebook_jumps_automatic.CurrentPage = 0;
		execute_auto_doing = false;
	}
	
	private void on_button_auto_order_clicked (object o, EventArgs args) {
		executeAutoWin = ExecuteAutoWindow.ShowJustOrder(app1, execute_auto_order, execute_auto_order_pos);
	}

	private void on_button_auto_skip_person_clicked (object o, EventArgs args) {
		execute_auto_order = ExecuteAuto.SkipPerson(execute_auto_order, execute_auto_order_pos, currentPerson);
		
		//update currentPerson and labels from current position
		execute_auto_select();
	}

	private void on_button_auto_remove_person_clicked (object o, EventArgs args) {
		execute_auto_order = ExecuteAuto.RemovePerson(execute_auto_order, execute_auto_order_pos, currentPerson);
		
		//update currentPerson and labels from current position
		execute_auto_select();
	}


	/* ---------------------------------------------------------
	 * ----------------  SOME MORE CALLBACKS---------------------
	 *  --------------------------------------------------------
	 */

	//changed by chronojump when it's needed
	private void notebooks_change(Constants.Modes mode)
	{
		LogB.Information("notebooks_change");
		//LogB.Debug(new StackFrame(1).GetMethod().Name);

		//LogB.Information("currentPage" + notebook_execute.CurrentPage.ToString());
		//LogB.Information("desiredPage" + desiredPage.ToString());

		if(mode == Constants.Modes.JUMPSSIMPLE)
		{
			notebook_execute.CurrentPage = 0;
			notebook_options_top.CurrentPage = 0;
			notebook_results.CurrentPage = 0;

			if(currentJumpType != null)
				changeTestImage(EventType.Types.JUMP.ToString(),
						currentJumpType.Name, currentJumpType.ImageFileName);
		} else if(mode == Constants.Modes.JUMPSREACTIVE)
		{
			notebook_execute.CurrentPage = 1;
			notebook_options_top.CurrentPage = 1;
			notebook_results.CurrentPage = 1;

			if(currentJumpRjType != null)
				changeTestImage(EventType.Types.JUMP.ToString(),
						currentJumpRjType.Name, currentJumpRjType.ImageFileName);
		} else if(mode == Constants.Modes.RUNSSIMPLE)
		{
			notebook_execute.CurrentPage = 2;
			notebook_options_top.CurrentPage = 2;
			notebook_results.CurrentPage = 2;

			if(currentRunType != null)
				changeTestImage(EventType.Types.RUN.ToString(),
						currentRunType.Name, currentRunType.ImageFileName);
		} else if(mode == Constants.Modes.RUNSINTERVALLIC)
		{
			notebook_execute.CurrentPage = 3;
			notebook_options_top.CurrentPage = 3;
			notebook_results.CurrentPage = 3;

			if(currentRunIntervalType != null)
				changeTestImage(EventType.Types.RUN.ToString(),
						currentRunIntervalType.Name, currentRunIntervalType.ImageFileName);
		} else if(mode == Constants.Modes.RUNSENCODER)
		{
			notebook_execute.CurrentPage = 8;
			notebook_options_top.CurrentPage = 8;
			notebook_results.CurrentPage = 8;
			changeTestImage("", "", "RUNSENCODER");
			event_execute_button_finish.Sensitive = false;
		} else if(mode == Constants.Modes.FORCESENSOR)
		{
			notebook_execute.CurrentPage = 4;
			notebook_options_top.CurrentPage = 4; //but at FORCESENSOR this notebook is not shown until adjust button is clicked
			notebook_results.CurrentPage = 4;

			if(frame_force_sensor_elastic.Visible)
				changeTestImage("", "", "FORCESENSOR_ELASTIC");
			else
				changeTestImage("", "", "FORCESENSOR_NOT_ELASTIC");

			event_execute_button_finish.Sensitive = false;
		} else if(mode == Constants.Modes.RT)
		{
			notebook_execute.CurrentPage = 5;
			notebook_options_top.CurrentPage = 5;
			notebook_results.CurrentPage = 5;

			if(currentReactionTimeType != null)
				changeTestImage(EventType.Types.REACTIONTIME.ToString(),
						currentReactionTimeType.Name, currentReactionTimeType.ImageFileName);
		} else if(mode == Constants.Modes.OTHER)
		{
			if(radio_mode_multi_chronopic_small.Active)
			{
				notebook_execute.CurrentPage = 7;
				notebook_options_top.CurrentPage = 7;
				notebook_results.CurrentPage = 7;

				if(currentMultiChronopicType != null)
					changeTestImage(EventType.Types.MULTICHRONOPIC.ToString(),
							currentMultiChronopicType.Name, currentMultiChronopicType.ImageFileName);
			} else { //pulses
				notebook_execute.CurrentPage = 6;
				notebook_options_top.CurrentPage = 6;
				notebook_results.CurrentPage = 6;

				if(currentPulseType != null)
					changeTestImage(EventType.Types.PULSE.ToString(),
							currentPulseType.Name, currentPulseType.ImageFileName);
			}
		}

		//delete capture graph
		UtilGtk.ClearDrawingArea(event_execute_drawingarea, event_execute_pixmap);
		//change table under graph
		change_notebook_results_data();

		//button_execute_test have to be non sensitive in multichronopic without two cps
		//else has to be sensitive

		//if there are persons
		if(mode == Constants.Modes.OTHER && radio_mode_multi_chronopic_small.Active)
		{
			/*
			 * disabled on 1.6.3
			if (chronopicWin.NumConnected() >= 2)
				extra_window_multichronopic_can_do(true);
			else 
				extra_window_multichronopic_can_do(false);
				*/
		} else {
			button_execute_test.Sensitive = myTreeViewPersons.IsThereAnyRecord();
			button_auto_start.Sensitive = myTreeViewPersons.IsThereAnyRecord();
		}

		//Attention: "notebooks_change sqlite problem"
		//This will call stats_win_change_test_type
		//that will call on_combo_stats_type_changed
		//that will call updateComboStats
		//and that will call Sqlite.
		//This is dangerous because it can crash when notebooks_change is called after deleting a test
		//just disable notebooks change in that situation
		stats_win_change_test_type(notebook_execute.CurrentPage);
	}
	
	//changed by user clicking on notebook tabs
	private void on_notebook_change_by_user (object o, SwitchPageArgs args) {
		//show chronojump logo on down-left area
		//changeTestImage("", "", "LOGO");
	}

	//help
	private void on_menuitem_manual_activate (object o, EventArgs args) {
		/*
		new DialogMessage(Constants.MessageTypes.HELP, 
				Catalog.GetString("There's a copy of Chronojump Manual at:") + "\n" + 
				"<i>" + Path.GetFullPath(Util.GetManualDir()) + "</i>\n\n" + 
				Catalog.GetString("Newer versions will be on this site:") +"\n" + 
				"<i>http://www.chronojump.org/multimedia.html</i>");
				*/

		if(! Util.OpenURL (System.IO.Path.GetFullPath(Util.GetManualDir())))
			new DialogMessage(Constants.MessageTypes.WARNING, "Sorry, manual folder does not exist.");
	}

	private void on_menuitem_formulas_activate (object o, EventArgs args) {
		new DialogMessage(Constants.MessageTypes.INFO, "Here there will be bibliographic information about formulas and some notes.\n\nProbably this will be a window and not a dialog\n\nNote text is selectable");
	}

	private void on_shortcuts_clicked (object o, EventArgs args)
	{
		new DialogShortcuts(operatingSystem == UtilAll.OperatingSystems.MACOSX);
	}

	private void on_menuitem_check_last_version_activate (object o, EventArgs args) 
	{
		Json js = new Json();
		bool success = js.GetLastVersion(progVersion);

		if(success) {
			LogB.Information(js.ResultMessage);
			new DialogMessage(
					"Chronojump",
					Constants.MessageTypes.INFO, 
					js.ResultMessage
					);
		}
		else {
			LogB.Error(js.ResultMessage);
			new DialogMessage(
					"Chronojump",
					Constants.MessageTypes.WARNING, 
					js.ResultMessage);
		}
	}

	private bool pulsePingAndNewsGTK ()
	{
		if(! pingThread.IsAlive)
		{
			// 1)  highlight news iccons if there are new news
			//if there is no network serverNewsDatetime will be empty, so do not highligh new products
			//highligh if server news date is not empty and server news date is different than client news date
			if(
					preferences.serverNewsDatetime != null &&
					preferences.serverNewsDatetime != "" &&
					preferences.serverNewsDatetime != preferences.clientNewsDatetime)
			{
				Pixbuf pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_store_has_new_products.png");
				image_menu_news.Pixbuf = pixbuf;
				image_menu_news1.Pixbuf = pixbuf;
			}

			// 2) make news buttons sensitive
			button_menu_news.Sensitive = true;
			button_menu_news1.Sensitive = true;

			// 3) end this pulse
			LogB.Information("pulsePingAndNews ending here");
			LogB.ThreadEnded();
			return false;
		}

		Thread.Sleep (250);
		//Log.Write(" (PulseGTK:" + thread.ThreadState.ToString() + ") ");
		return true;
	}

	//declared here in order to be easy closed on exit Chronojump
	Json jsPing;
	private void pingAndNewsAtStart()
	{
		jsPing = new Json();
		if(pingDo())
		{
			getNewsDatetime();

			//also manage pending poll
			if(preferences.socialNetworkDatetime == "-1")
			{
				Json js = new Json();
				bool success = js.SocialNetworkPoll(preferences.machineID, preferences.socialNetwork);
				if(success) {
					SqlitePreferences.Update(SqlitePreferences.SocialNetwork, preferences.socialNetwork, false);
					SqlitePreferences.Update(SqlitePreferences.SocialNetworkDatetime,
							UtilDate.ToFile(DateTime.Now), false);
				}
			}
		}
	}

	private bool pingDo()
	{
		LogB.Information("version at pingDo:" + UtilAll.ReadVersionFromBuildInfo());
		bool success = jsPing.Ping(UtilAll.GetOS(), UtilAll.ReadVersionFromBuildInfo(), preferences.machineID);

		if(success)
			LogB.Information(jsPing.ResultMessage);
		else
			LogB.Error(jsPing.ResultMessage);

		return success;

	}

	private void getNewsDatetime()
	{
		LogB.Information("getNewsDatetime()");
		if(jsPing.GetNewsDatetime())
			preferences.serverNewsDatetime = jsPing.ResultMessage;
	}

	private void on_preferences_debug_mode_start (object o, EventArgs args) {
		//first delete debug file
		Util.FileDelete(System.IO.Path.GetTempPath() + "chronojump-debug.txt");

		encoderRProcCapture.Debug = true;
		encoderRProcAnalyze.Debug = true;
		preferences.debugMode = true; //be used by force sensor, importer (can be used easily for all software)
		LogB.PrintAllThreads = true;

		//hbox_gui_tests.Visible = true;
		//button_carles.Visible = true;

		//menuitem_check_race_encoder_capture_simulate.Visible = true;

		preferencesWin.DebugActivated();

		if(currentSession == null)
			setApp1Title("", current_mode);
		else
			setApp1Title(currentSession.Name, current_mode);
	}

	//use chronojumpConfig
	private void on_button_gui_tests_clicked (object o, EventArgs args)
	{
		if(currentSession == null)
		{
			new DialogMessage(Constants.MessageTypes.WARNING, "Need to open a session");
			return;
		}

		chronojumpWindowTestsStart(
				currentSession.UniqueID,
				CJTests.SequenceEncoderGraphSetBars);
	}

	//use DEBUG and selector on main gui (previous to 2.0)
	/*
	private void on_button_gui_tests_old_clicked (object o, EventArgs args)
	{
		string selected = UtilGtk.ComboGetActive(combo_gui_tests);
		if(selected == "")
		{
			new DialogMessage(Constants.MessageTypes.WARNING, "Need to select one test");
			return;
		}

		if (selected == "EncoderGravitatoryCapture" || selected == "EncoderInertialCapture")
		{
			if (currentSession == null)
			{
				new DialogMessage(Constants.MessageTypes.WARNING, "Need to load SIMULATED session");
				return;
			}

			if (selected == "EncoderGravitatoryCapture")
				chronojumpWindowTestsStart(
						Convert.ToInt32(spin_gui_tests.Value),
						CJTests.SequenceEncoderGravitatoryCapture);
			else // (selected == "EncoderInertialCapture")
				chronojumpWindowTestsStart(
						Convert.ToInt32(spin_gui_tests.Value),
						CJTests.SequenceEncoderInertialCapture);
		}
		else
			new DialogMessage(Constants.MessageTypes.WARNING, "Selected test: " + selected);

		// other tests:
		//CJTests.SequenceChangeMultitest
		//CJTests.SequenceRJsSimulatedFinishCancel
	}
	*/
	
	private void on_button_carles_clicked (object o, EventArgs args)
	{
		/*
		if (currentPerson == null || currentSession == null)
			return;

		JumpsDjOptimalFall jdof = new JumpsDjOptimalFall();
		jdof.Calculate(currentPerson.UniqueID, currentSession.UniqueID);
		*/


		/*
		bool showInWindow = true;

		Json js = new Json();
		bool success = js.UploadEncoderData();

		if(success) {
			LogB.Information(js.ResultMessage);
			if(showInWindow)
				new DialogMessage(
						"Chronojump",
						Constants.MessageTypes.INFO,
						js.ResultMessage);
		}
		else {
			LogB.Error(js.ResultMessage);
			if(showInWindow)
				new DialogMessage(
						"Chronojump",
						Constants.MessageTypes.WARNING,
						js.ResultMessage);
		}
*/
		/*
		new DialogMessage(
				"Chronojump",
				Constants.MessageTypes.INFO,
				"Temporarily Disabled");
		*/

		//carles stuff
	}


	private void on_about1_activate (object o, EventArgs args) {
		string translator_credits = Catalog.GetString ("translator-credits");
		//only print if exist (don't print 'translator-credits' word
		if(translator_credits == "translator-credits") 
			translator_credits = "";

		new About(progVersion, translator_credits);
	}

	private void on_repetitive_conditions_closed(object o, EventArgs args)
	{
		//update bell color if feedback exists
		Constants.Modes m = current_mode;
		Pixbuf pixbuf;

		Constants.BellModes bellMode = getBellMode(m);
		if(m == Constants.Modes.JUMPSREACTIVE || m == Constants.Modes.RUNSINTERVALLIC)
		{
			if(repetitiveConditionsWin.FeedbackActive(bellMode))
				pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_bell_active.png");
			else
				pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_bell_none.png");

			image_contacts_bell.Pixbuf = pixbuf;
		}
		else if(m == Constants.Modes.POWERGRAVITATORY || m == Constants.Modes.POWERINERTIAL)
		{
			if(repetitiveConditionsWin.FeedbackActive(bellMode))
				pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_bell_active.png");
			else
				pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_bell_none.png");

			image_encoder_bell.Pixbuf = pixbuf;

			Sqlite.Open();

			//mainVariable
			Constants.EncoderVariablesCapture mainVariable = Constants.SetEncoderVariablesCapture(
					repetitiveConditionsWin.GetMainVariable);
			if( preferences.encoderCaptureMainVariable != mainVariable ) {
				SqlitePreferences.Update("encoderCaptureMainVariable", mainVariable.ToString(), true);
				preferences.encoderCaptureMainVariable = mainVariable;
			}
			string mainVariableStr = Constants.GetEncoderVariablesCapture(mainVariable);

			//secondaryVariable
			Constants.EncoderVariablesCapture secondaryVariable = Constants.SetEncoderVariablesCapture(
					repetitiveConditionsWin.GetSecondaryVariable);
			if( preferences.encoderCaptureSecondaryVariable != secondaryVariable ) {
				SqlitePreferences.Update("encoderCaptureSecondaryVariable", secondaryVariable.ToString(), true);
				preferences.encoderCaptureSecondaryVariable = secondaryVariable;
			}
			string secondaryVariableStr = Constants.GetEncoderVariablesCapture(secondaryVariable);

			//secondaryVariableShow
			bool secondaryVariableShow = repetitiveConditionsWin.GetSecondaryVariableShow;
			if( preferences.encoderCaptureSecondaryVariableShow != secondaryVariableShow ) {
				SqlitePreferences.Update("encoderCaptureSecondaryVariableShow", secondaryVariableShow.ToString(), true);
				preferences.encoderCaptureSecondaryVariableShow = secondaryVariableShow;
			}
			if(! secondaryVariableShow)
				secondaryVariableStr = "";

			if(preferences.encoderCaptureFeedbackEccon != repetitiveConditionsWin.GetEncoderCaptureFeedbackEccon) {
				SqlitePreferences.Update(SqlitePreferences.EncoderCaptureFeedbackEccon,
						repetitiveConditionsWin.GetEncoderCaptureFeedbackEccon.ToString(), true);
				preferences.encoderCaptureFeedbackEccon = repetitiveConditionsWin.GetEncoderCaptureFeedbackEccon;
			}

			if(preferences.encoderCaptureInertialEccOverloadMode != repetitiveConditionsWin.GetEncoderCaptureEccOverloadMode) {
				SqlitePreferences.Update(SqlitePreferences.EncoderCaptureInertialEccOverloadMode,
						repetitiveConditionsWin.GetEncoderCaptureEccOverloadMode.ToString(), true);
				preferences.encoderCaptureInertialEccOverloadMode = repetitiveConditionsWin.GetEncoderCaptureEccOverloadMode;
			}

			preferences.encoderCaptureMainVariableThisSetOrHistorical = Preferences.PreferencesChange(
				SqlitePreferences.EncoderCaptureMainVariableThisSetOrHistorical,
				preferences.encoderCaptureMainVariableThisSetOrHistorical,
				repetitiveConditionsWin.EncoderRelativeToSet);

			preferences.encoderCaptureMainVariableGreaterActive = Preferences.PreferencesChange(
				SqlitePreferences.EncoderCaptureMainVariableGreaterActive,
				preferences.encoderCaptureMainVariableGreaterActive,
				repetitiveConditionsWin.EncoderAutomaticHigherActive);

			preferences.encoderCaptureMainVariableGreaterValue = Preferences.PreferencesChange(
				SqlitePreferences.EncoderCaptureMainVariableGreaterValue,
				preferences.encoderCaptureMainVariableGreaterValue,
				repetitiveConditionsWin.EncoderAutomaticHigherValue);

			preferences.encoderCaptureMainVariableLowerActive = Preferences.PreferencesChange(
				SqlitePreferences.EncoderCaptureMainVariableLowerActive,
				preferences.encoderCaptureMainVariableLowerActive,
				repetitiveConditionsWin.EncoderAutomaticLowerActive);

			preferences.encoderCaptureMainVariableLowerValue = Preferences.PreferencesChange(
				SqlitePreferences.EncoderCaptureMainVariableLowerValue,
				preferences.encoderCaptureMainVariableLowerValue,
				repetitiveConditionsWin.EncoderAutomaticLowerValue);

			preferences.encoderCaptureShowLoss = Preferences.PreferencesChange(
				SqlitePreferences.EncoderCaptureShowLoss,
				preferences.encoderCaptureShowLoss,
				repetitiveConditionsWin.EncoderCaptureShowLoss);

			Sqlite.Close();


			//treeview_encoder should be updated (to colorize some cells)
			//only if there was data
			//this avoids misbehaviour when bell is pressed and there's no data in treeview
			EncoderCurve curve = treeviewEncoderCaptureCurvesGetCurve(1, false);
			if(curve.N != null) {
				List<string> contents = Util.ReadFileAsStringList(UtilEncoder.GetEncoderCurvesTempFileName());
				encoderUpdateTreeViewCapture(contents); //this updates encoderCaptureCurves

				findAndMarkSavedCurves(false, false); //SQL closed; don't update curve SQL records (like future1: meanPower)

				//also update the bars plot (to show colors depending on bells changes)
				if(captureCurvesBarsData.Count > 0) {
					double mainVariableHigher = repetitiveConditionsWin.GetMainVariableHigher(mainVariableStr);
					double mainVariableLower = repetitiveConditionsWin.GetMainVariableLower(mainVariableStr);
					//plotCurvesGraphDoPlot(mainVariableStr, mainVariableHigher, mainVariableLower,

					if(encoderGraphDoPlot != null)
					{
						encoderGraphDoPlot.NewPreferences(preferences);
						encoderGraphDoPlot.Start(
								mainVariableStr, mainVariableHigher, mainVariableLower,
								secondaryVariableStr, preferences.encoderCaptureShowLoss,
								false,
								findEccon(true),
								repetitiveConditionsWin,
								encoderConfigurationCurrent.has_inertia,
								configChronojump.PlaySoundsFromFile,
								captureCurvesBarsData,
								encoderCaptureListStore,
								preferences.encoderCaptureMainVariableThisSetOrHistorical,
								sendMaxPowerSpeedForceIntersession(mainVariable),
								sendMaxPowerSpeedForceIntersessionDate(mainVariable));
					}
				} else
					UtilGtk.ErasePaint(encoder_capture_curves_bars_drawingarea, encoder_capture_curves_bars_pixmap);
			}

			//rhythm
			encoderRhythm = repetitiveConditionsWin.Encoder_rhythm_get_values();
			//updates preferences object and Sqlite preferences
			preferences.UpdateEncoderRhythm(encoderRhythm);
		}
		else if(m == Constants.Modes.FORCESENSOR)
		{
			if(repetitiveConditionsWin.FeedbackActive(bellMode))
				pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_bell_active.png");
			else
				pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_bell_none.png");

			image_contacts_bell.Pixbuf = pixbuf;

			Preferences.ForceSensorCaptureFeedbackActiveEnum feedbackActive = repetitiveConditionsWin.GetForceSensorFeedback;
			if(preferences.forceSensorCaptureFeedbackActive != feedbackActive)
			{
				SqlitePreferences.Update(SqlitePreferences.ForceSensorCaptureFeedbackActive, feedbackActive.ToString(), false);
				preferences.forceSensorCaptureFeedbackActive = feedbackActive;
			}

			//change the rest of values only if feedback is active
			if(feedbackActive == Preferences.ForceSensorCaptureFeedbackActiveEnum.RECTANGLE)
			{
				int feedbackAt = repetitiveConditionsWin.GetForceSensorFeedbackRectangleAt;
				if(preferences.forceSensorCaptureFeedbackAt != feedbackAt)
				{
					SqlitePreferences.Update(SqlitePreferences.ForceSensorCaptureFeedbackAt, feedbackAt.ToString(), false);
					preferences.forceSensorCaptureFeedbackAt = feedbackAt;
				}

				int feedbackRange = repetitiveConditionsWin.GetForceSensorFeedbackRectangleRange;
				if(preferences.forceSensorCaptureFeedbackRange != feedbackRange)
				{
					SqlitePreferences.Update(SqlitePreferences.ForceSensorCaptureFeedbackRange, feedbackRange.ToString(), false);
					preferences.forceSensorCaptureFeedbackRange = feedbackRange;
				}
			}
			else if(feedbackActive == Preferences.ForceSensorCaptureFeedbackActiveEnum.PATH)
			{
				int feedbackPathMax = repetitiveConditionsWin.GetForceSensorFeedbackPathMax;
				if(preferences.forceSensorFeedbackPathMax != feedbackPathMax)
				{
					SqlitePreferences.Update(SqlitePreferences.ForceSensorFeedbackPathMax, feedbackPathMax.ToString(), false);
					preferences.forceSensorFeedbackPathMax = feedbackPathMax;
				}

				int feedbackPathMin = repetitiveConditionsWin.GetForceSensorFeedbackPathMin;
				if(preferences.forceSensorFeedbackPathMin != feedbackPathMin)
				{
					SqlitePreferences.Update(SqlitePreferences.ForceSensorFeedbackPathMin, feedbackPathMin.ToString(), false);
					preferences.forceSensorFeedbackPathMin = feedbackPathMin;
				}

				int feedbackPathMasters = repetitiveConditionsWin.GetForceSensorFeedbackPathMasters;
				if(preferences.forceSensorFeedbackPathMasters != feedbackPathMasters)
				{
					SqlitePreferences.Update(SqlitePreferences.ForceSensorFeedbackPathMasters, feedbackPathMasters.ToString(), false);
					preferences.forceSensorFeedbackPathMasters = feedbackPathMasters;
				}

				int feedbackPathMasterSeconds = repetitiveConditionsWin.GetForceSensorFeedbackPathMasterSeconds;
				if(preferences.forceSensorFeedbackPathMasterSeconds != feedbackPathMasterSeconds)
				{
					SqlitePreferences.Update(SqlitePreferences.ForceSensorFeedbackPathMasterSeconds, feedbackPathMasterSeconds.ToString(), false);
					preferences.forceSensorFeedbackPathMasterSeconds = feedbackPathMasterSeconds;
				}

				int feedbackPathLineWidth = repetitiveConditionsWin.GetForceSensorFeedbackPathLineWidth;
				if(preferences.forceSensorFeedbackPathLineWidth != feedbackPathLineWidth)
				{
					SqlitePreferences.Update(SqlitePreferences.ForceSensorFeedbackPathLineWidth, feedbackPathLineWidth.ToString(), false);
					preferences.forceSensorFeedbackPathLineWidth = feedbackPathLineWidth;
				}
			}
		}
	}

	private void on_radio_mode_contacts_capture_toggled (object o, EventArgs args)
	{
		if(! radio_mode_contacts_capture.Active)
			return;

		notebook_capture_analyze.CurrentPage = 0;
	}
	private void on_radio_mode_contacts_analyze_toggled (object o, EventArgs args)
	{
		if(! radio_mode_contacts_analyze.Active)
			return;

		if(current_mode == Constants.Modes.JUMPSSIMPLE ||
				current_mode == Constants.Modes.JUMPSREACTIVE ||
				current_mode == Constants.Modes.RUNSSIMPLE ||
				current_mode == Constants.Modes.RUNSINTERVALLIC)
		{
			radio_mode_contacts_analyze_buttons_visible (current_mode);

			if(current_mode == Constants.Modes.JUMPSSIMPLE)
			{
				if(radio_mode_contacts_jumps_profile.Active)
				{
					notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.JUMPSPROFILE);
					jumpsProfileDo(true);
				}

				if(radio_mode_contacts_jumps_dj_optimal_fall.Active)
				{
					notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.JUMPSDJOPTIMALFALL);
					jumpsDjOptimalFallDo(true);
				}

				if(radio_mode_contacts_jumps_weight_fv_profile.Active)
				{
					notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.JUMPSWEIGHTFVPROFILE);
					jumpsWeightFVProfileDo(true);
				}

				if(radio_mode_contacts_jumps_evolution.Active)
				{
					notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.JUMPSEVOLUTION);
					jumpsEvolutionDo(true);
				}
			}
			else if (current_mode == Constants.Modes.JUMPSREACTIVE)
			{
				if(radio_mode_contacts_jumps_rj_fatigue.Active)
				{
					notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.JUMPSRJFATIGUE);
					createComboSelectJumpsRjFatigue (false);

					//Active should be the same than on capture tab
					combo_select_jumps_rj_fatigue.Active = combo_select_jumps_rj.Active;

					//Active should be the last one to see the correct test after a capture
					if(comboSelectJumpsRjFatigueNum.Count > 0)
						combo_select_jumps_rj_fatigue_num.Active = comboSelectJumpsRjFatigueNum.Count -1;
				}
			}
			else if(current_mode == Constants.Modes.RUNSSIMPLE)
			{
				if(radio_mode_contacts_runs_evolution.Active)
				{
					notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.RUNSEVOLUTION);
					runsEvolutionDo(true, true);
				}
			}
		}
		else if(current_mode == Constants.Modes.FORCESENSOR)
			notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.FORCESENSOR);
		else if(current_mode == Constants.Modes.RUNSENCODER)
			notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.RACEENCODER);
		else
			notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.STATISTICS);

		notebook_capture_analyze.CurrentPage = 1;
	}

	private void on_radio_mode_contacts_jumps_profile_toggled (object o, EventArgs args)
	{
		if(radio_mode_contacts_jumps_profile.Active)
		{
			notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.JUMPSPROFILE);
			jumpsProfileDo(true);
		}
	}
	private void on_radio_mode_contacts_jumps_dj_optimal_fall_toggled (object o, EventArgs args)
	{
		if(radio_mode_contacts_jumps_dj_optimal_fall.Active)
		{
			notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.JUMPSDJOPTIMALFALL);
			jumpsDjOptimalFallDo(true);
		}
	}
	private void on_radio_mode_contacts_jumps_weight_fv_profile_toggled (object o, EventArgs args)
	{
		if(radio_mode_contacts_jumps_weight_fv_profile.Active)
		{
			notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.JUMPSWEIGHTFVPROFILE);
			jumpsWeightFVProfileDo(true);
		}
	}
	private void on_radio_mode_contacts_jumps_evolution_toggled (object o, EventArgs args)
	{
		if(radio_mode_contacts_jumps_evolution.Active)
		{
			notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.JUMPSEVOLUTION);
			jumpsEvolutionDo(true);
		}
	}
	private void on_radio_mode_contacts_advanced_toggled (object o, EventArgs args)
	{
		if(radio_mode_contacts_advanced.Active)
			notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.STATISTICS);
	}
	private void on_radio_mode_contacts_jumps_rj_fatigue_toggled (object o, EventArgs args)
	{
		if(radio_mode_contacts_jumps_rj_fatigue.Active)
		{
			notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.JUMPSRJFATIGUE);
			createComboSelectJumpsRjFatigue (false);
		}
	}
	private void on_radio_mode_contacts_runs_evolution_toggled (object o, EventArgs args)
	{
		if(radio_mode_contacts_runs_evolution.Active)
		{
			notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.RUNSEVOLUTION);
			runsEvolutionDo(true, true);
		}
	}
	private void on_radio_mode_contacts_sprint_toggled (object o, EventArgs args)
	{
		if(radio_mode_contacts_sprint.Active)
			notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.SPRINT);
	}

	/* ---------------------------------------------------------
	 * ----------------  SENSITIVE GUI METHODS-------------------
	 *  --------------------------------------------------------
	 */

	private void menuSessionSensitive(bool option)
	{
		frame_session_more_this_session.Sensitive = option;
	}
	
	private void menuPersonSelectedSensitive(bool option)
	{
		if(! option || currentPerson == null)
		{
			button_persons_up.Sensitive = false;
			button_persons_down.Sensitive = false;
		} else {
			button_persons_up.Sensitive = ! myTreeViewPersons.IsFirst(currentPerson.UniqueID);
			button_persons_down.Sensitive = ! myTreeViewPersons.IsLast(currentPerson.UniqueID);
		}

		hbox_persons_bottom_photo.Sensitive = option;
		vbox_persons_bottom_no_photo.Sensitive = option;
	}

	private void sensitiveGuiNoSession () 
	{
		//menuitems
		menuSessionSensitive(false);
		menuPersonSelectedSensitive(false);
		
		button_image_test_zoom.Sensitive = false;
		frame_persons.Sensitive = false;
		button_recuperate_person.Sensitive = false;
		button_recuperate_persons_from_session.Sensitive = false;
		button_person_add_single.Sensitive = false;
		button_person_add_multiple.Sensitive = false;
		hbox_persons_bottom_photo.Sensitive = false;
		vbox_persons_bottom_no_photo.Sensitive = false;
	
		button_contacts_person_change.Sensitive = false;
		button_encoder_person_change.Sensitive = false;
		frame_contacts_exercise.Sensitive = false;
		
		//notebooks
		notebook_analyze.Sensitive = false;
		notebook_results.Sensitive = false;
		encoder_sensitive_all_except_device(false);

		vbox_stats.Sensitive = false;
		
		sensitiveLastTestButtons(false);
		vbox_execute_test.Sensitive = false;
		button_execute_test.Sensitive = false;
		button_auto_start.Sensitive = false;
		alignment_button_force_sensor_adjust.Sensitive = false;
		button_force_sensor_sync.Sensitive = false;

		encoderButtonsSensitive(encoderSensEnum.NOSESSION);
		
		eventExecuteHideAllTables();
	}
	
	private void sensitiveGuiYesSession () 
	{
		button_image_test_zoom.Sensitive = true;
		frame_persons.Sensitive = true;
		button_recuperate_person.Sensitive = true;
		button_recuperate_persons_from_session.Sensitive = true;
		button_person_add_single.Sensitive = true;
		button_person_add_multiple.Sensitive = true;
		
		button_contacts_person_change.Sensitive = true;
		button_encoder_person_change.Sensitive = true;
		alignment_button_force_sensor_adjust.Sensitive = true;
		button_force_sensor_sync.Sensitive = true;
		
		menuSessionSensitive(true);
		vbox_stats.Sensitive = true;
		
		//changeTestImage("", "", "LOGO");
	}

	//only called by delete person functions (if we run out of persons)
	private void sensitiveGuiNoPerson ()
	{
		LogB.Information("sensitiveGuiNoPerson");
		vbox_jumps.Sensitive = false;
		hbox_jumps_rj.Sensitive = false;
		button_execute_test.Sensitive = false;
		button_auto_start.Sensitive = false;
		button_contacts_capture_load.Sensitive = false;
		button_contacts_capture_session_overview.Sensitive = false;

		encoderButtonsSensitive(encoderSensEnum.NOPERSON);
		//don't cal personChanged because it will make changes on analyze repetitions and currentPerson == null
		//personChanged();

		if(notebook_encoder_sup.CurrentPage == 1)
			notebook_encoder_sup.CurrentPage = 0;

		frame_contacts_exercise.Sensitive = false;
		notebook_analyze.Sensitive = false;
		notebook_results.Sensitive = false;
		encoder_sensitive_all_except_device(false);

		treeview_persons.Sensitive = false;
		
		menuPersonSelectedSensitive(false);
		vbox_execute_test.Sensitive = false;

		label_top_person_name.Text = "";
		label_top_encoder_person_name.Text = "";
	}
	
	private void sensitiveGuiYesPerson ()
	{
		LogB.Information("sensitiveGuiYesPerson");
		vbox_jumps.Sensitive = true;
		hbox_jumps_rj.Sensitive = true;
		button_execute_test.Sensitive = true;
		button_auto_start.Sensitive = true;
		button_contacts_capture_load.Sensitive = true;
		button_contacts_capture_session_overview.Sensitive = true;

		encoderButtonsSensitive(encoderSensEnum.YESPERSON);
		personChanged();
		
		frame_contacts_exercise.Sensitive = true;
		notebook_analyze.Sensitive = true;
		notebook_results.Sensitive = true;
		encoder_sensitive_all_except_device(true);

		if(! configChronojump.Exhibition)
			treeview_persons.Sensitive = true;
		
		menuPersonSelectedSensitive(true);
	
		//unsensitive edit, delete, repair events because no event is initially selected
		showHideActionEventButtons(false, "ALL");

		combo_select_jumps.Sensitive = true;
		combo_select_jumps_rj.Sensitive = true;
		combo_select_runs.Sensitive = true;
		combo_select_runs_interval.Sensitive = true;
		//combo_pulses.Sensitive = true;
		
		vbox_execute_test.Sensitive = true;
	}
	
	private void sensitiveGuiYesEvent () {
	}
	
	private void sensitiveGuiEventDoing (bool cont)
	{
		menus_and_mode_sensitive(false);
		
		//jumpsProfile has Sqlite calls. Don't do them while jumping
		//but don't unsensitive the notebook because user need to "finish" or cancel"
		//notebook_capture_analyze.Sensitive = true; 
		radio_mode_contacts_analyze.Hide();

		if(cont)
		{
			frame_persons_top.Sensitive = false;
			//treeview_persons is shown (person can be changed)

			vbox_persons_bottom.Sensitive = false;
		} else
			frame_persons.Sensitive = false;
		
		button_execute_test.Sensitive = false;
		button_auto_start.Sensitive = false;
		hbox_contacts_camera.Sensitive = false;
		
		button_contacts_person_change.Sensitive = false;
		button_encoder_person_change.Sensitive = false;

		image_inertial_extended.Visible = true;
		button_encoder_inertial_recalibrate.Visible = false;

		encoderButtonsSensitive(encoderSensEnum.PROCESSINGR);
		
		//hbox
		//hbox_jumps_test.Sensitive = false;
		//hbox_jump_types_options.Sensitive = false;
		
		hbox_jumps_rj.Sensitive = false;
		vbox_runs.Sensitive = false;
		hbox_runs_interval_all.Sensitive = false;
		hbox_other_mc.Sensitive = false;
		hbox_other_pulses.Sensitive = false;
		sensitiveLastTestButtons(false);

		//do not allow to touch buttons on show table
		notebook_results.Sensitive = false;

		button_activate_chronopics.Sensitive = false;
		button_activate_chronopics_encoder.Sensitive = false;
		alignment_button_threshold.Sensitive = false;
		alignment_button_force_sensor_adjust.Sensitive = false;
		button_force_sensor_sync.Sensitive = false;
		button_auto_start.Sensitive = false;
		frame_contacts_exercise.Sensitive = false;

		//hbox_multi_chronopic_buttons.Sensitive = false;
	}
   
	private void sensitiveGuiEventDone ()
	{
		LogB.Information(" sensitiveGuiEventDone start ");

		menus_and_mode_sensitive(true);

		//jumpsProfile has Sqlite calls. Don't do them while jumping
		//but don't unsensitive the notebook because user need to "finish" or cancel"
		//notebook_capture_analyze.Sensitive = true; 
		radio_mode_contacts_analyze.Visible = true;

		frame_persons.Sensitive = true;
		//check this is sensitive (because on cont was unsensitive)
		if(! frame_persons_top.Sensitive)
			frame_persons_top.Sensitive = true;
		if(! vbox_persons_bottom.Sensitive)
			vbox_persons_bottom.Sensitive = true;

		button_execute_test.Sensitive = true;
		button_auto_start.Sensitive = true;
		hbox_contacts_camera.Sensitive = true;

		button_contacts_person_change.Sensitive = true;
		button_encoder_person_change.Sensitive = true;

		//allow show the recalibrate button
		if(encoderInertialCalibratedFirstTime)
		{
			image_inertial_extended.Visible = false;
			button_encoder_inertial_recalibrate.Visible = true;
		}

		if(encoderCaptureCurves != null && encoderCaptureCurves.Count > 0)
			encoderButtonsSensitive(encoderSensEnum.DONEYESSIGNAL);
		else
			encoderButtonsSensitive(encoderSensEnum.DONENOSIGNAL);

		//hbox
		//hbox_jumps_test.Sensitive = true;
		//hbox_jump_types_options.Sensitive = true;
		
		hbox_jumps_rj.Sensitive = true;
		vbox_runs.Sensitive = true;
		hbox_runs_interval_all.Sensitive = true;
		hbox_other_mc.Sensitive = true;
		hbox_other_pulses.Sensitive = true;
		//hbox_multi_chronopic_buttons.Sensitive = true;

		//allow to touch buttons on show table
		notebook_results.Sensitive = true;

		button_activate_chronopics.Sensitive = true;

		if(! configChronojump.Compujump)
			button_activate_chronopics_encoder.Sensitive = true;

		alignment_button_threshold.Sensitive = true;
		alignment_button_force_sensor_adjust.Sensitive = true;
		button_force_sensor_sync.Sensitive = true;
		button_auto_start.Sensitive = true;
		frame_contacts_exercise.Sensitive = true;

		//forceSensor and runEncoder does not use currentEventExecute
		if(current_mode == Constants.Modes.FORCESENSOR)
		{
			sensitiveLastTestButtons(! forceProcessCancel && ! forceProcessError);
			LogB.Information(" sensitiveGuiEventDone end (forceSensor)");
			return;
		} else if(current_mode == Constants.Modes.RUNSENCODER)
		{
			sensitiveLastTestButtons(! runEncoderProcessCancel && ! runEncoderProcessError);
			LogB.Information(" sensitiveGuiEventDone end (runsEncoder)");
			return;
		}

		sensitiveLastTestButtons(true);

		//allow repeat last jump or run (check also if it wasn't cancelled)
		if(currentEventExecute != null && ! currentEventExecute.Cancel) {
			switch (currentEventType.Type) {
				case EventType.Types.REACTIONTIME:
					LogB.Information("sensitiveGuiEventDone reaction time");
					break;
				case EventType.Types.PULSE:
					LogB.Information("sensitiveGuiEventDone pulse");
					break;
				case EventType.Types.MULTICHRONOPIC:
					LogB.Information("sensitiveGuiEventDone multichronopic");
					break;
				default:
					LogB.Information("sensitiveGuiEventDone default");
					break;
			}
			button_delete_last_test.Sensitive = true;
		}
		else
			sensitiveLastTestButtons(false);

		LogB.Information(" sensitiveGuiEventDone end (not forceSensor)");
	}

	//to sensitive on and off the play_this_test and delete_this_test
	private void sensitiveLastTestButtons(bool sensitive)
	{
		LogB.Information("sensitiveLastTestButtons: " + sensitive.ToString());
		//vbox_last_test_buttons.Sensitive = sensitive; TODO:
		button_delete_last_test.Sensitive = sensitive;
	}
	/*
	 * sensitive GUI on executeAuto methods 
	 */

	bool showRunWirelessDevice = true;
	private void chronopicRegisterUpdate(bool openWindow)
	{
		//on Windows need to close the port before reading with FTDI dll
		if(UtilAll.IsWindows())
			cp2016.SerialPortsCloseIfNeeded(false);

		ChronopicRegisterSelectOS cros = new ChronopicRegisterSelectOS();
		chronopicRegister = cros.Do(configChronojump.Compujump, showRunWirelessDevice);
		
		/*
		 * If Chronopic has been disconnected on OSX, port gets blocked
		 * (no new tty is assigned until serial port is closed)
		 * maybe need to reconnect USB cables
		 */
		if(operatingSystem == UtilAll.OperatingSystems.MACOSX &&
				chronopicRegister.Crpl.L.Count == 0)
		{
			cp2016.SerialPortsCloseIfNeeded(true);
			Thread.Sleep(250);
			chronopicRegister = cros.Do(configChronojump.Compujump, showRunWirelessDevice);
		}


		/*
		 * openWindow: false, just generates the list,
		 * but if first time since cjump running and there are unknown Chronopics, window is opened
		if(! cp2016.WindowOpened && chronopicRegister.UnknownFound())
			openWindow = true;

			removed this because now contacts tests and encoder open device on pressing capture button when
			- some device is connected
			- that device is not configured
			O: do the same for force sensor and runEncoder
		 */

		if(openWindow)
		{
			chronopicRegisterWin = new ChronopicRegisterWindow(app1, chronopicRegister.Crpl.L,
					configChronojump.Compujump,	//to show/hide ARDUINO_RFID
					showRunWirelessDevice);

			cp2016.WindowOpened = true;

			if(app1Shown)
				chronopicRegisterWin.Show();
			else
				needToShowChronopicRegisterWindow = true;
		}
	}

	private void on_chronopic_register_win_close_networks_check_encoder (object o, EventArgs args)
	{
		label_encoder_checked_error.Visible = false;
		chronopicRegisterUpdate(false);
		if(chronopicRegister.NumConnectedOfType(ChronopicRegisterPort.Types.ENCODER) > 0)
			notebook_sup.CurrentPage = Convert.ToInt32(notebook_sup_pages.ENCODER);
		else
			label_encoder_checked_error.Visible = true;
	}

	//trying to fix when an OSX disconnects and reconnects same chronopic (and it has captured)
	private void closeSerialPort (object o, EventArgs args)
	{
		//cp2016.SerialPortsCloseIfNeeded();
	}

	//start/end auto mode
	private void sensitiveGuiAutoStartEnd (bool start) {
		//if automode, sensitiveGuiEventDoing, sensitiveGuiEventDone don't work
		menus_and_mode_sensitive (! start);
		frame_persons.Sensitive 	= ! start;
		frame_contacts_exercise.Sensitive = ! start;

		hbox_jump_auto_controls.Visible  = start;

		radio_mode_contacts_analyze.Visible = ! start;
		if(current_mode == Constants.Modes.JUMPSSIMPLE)
			hbox_radio_mode_contacts_analyze_buttons.Visible = ! start;

		//when start, put button delete_last_test as not sensitive
		//(just for the test previous to the auto process)
		if(start)
			button_delete_last_test.Sensitive = false;
	}
	
	//true: executing a test; false: waiting a test to be executed
	private void sensitiveGuiAutoExecuteOrWait (bool execute) {
		//if automode, sensitiveGuiEventDoing, sensitiveGuiEventDone don't work
		button_activate_chronopics.Sensitive 	= ! execute;
		alignment_button_threshold.Sensitive 		= ! execute;
		button_execute_test.Sensitive 		= ! execute;
		sensitiveLastTestButtons(! execute);
	}

	private void showHideActionEventButtons(bool show, string type) {
		bool success = false;
		bool recordedVideo = false;

		if(type == "ALL" || type == "Jump") {
			button_edit_selected_jump.Sensitive = show;
			button_delete_selected_jump.Sensitive = show;
		} 
		if (type == "ALL" || type == "JumpRj") {
			button_edit_selected_jump_rj.Sensitive = show;
			button_delete_selected_jump_rj.Sensitive = show;
			button_repair_selected_jump_rj.Sensitive = show;
		} 
		if (type == "ALL" || type == "Run") {
			button_edit_selected_run.Sensitive = show;
			button_delete_selected_run.Sensitive = show;
		} 
		if (type == "ALL" || type == "RunInterval") {
			button_edit_selected_run_interval.Sensitive = show;
			button_delete_selected_run_interval.Sensitive = show;
			button_repair_selected_run_interval.Sensitive = show;
			
		} 
		if (type == "ALL" || type == "ReactionTime") {
			button_edit_selected_reaction_time.Sensitive = show;
			button_delete_selected_reaction_time.Sensitive = show;
		} 
		if (type == "ALL" || type == "Pulse") {
			// menuitem_edit_selected_pulse.Sensitive = show;
			// menuitem_delete_selected_pulse.Sensitive = show;
			button_edit_selected_pulse.Sensitive = show;
			button_delete_selected_pulse.Sensitive = show;
			button_repair_selected_pulse.Sensitive = show;
		} 
		if (type == "ALL" || type == Constants.MultiChronopicName) {
			button_edit_selected_multi_chronopic.Sensitive = show;
			button_delete_selected_multi_chronopic.Sensitive = show;
			
		} 

		button_video_play_selected_test(current_mode);
		//LogB.Information("recordedVideo = " + recordedVideo.ToString());
	}
	
	
	/*
	 * voluntary crash for testing purposes 
	 */

	private void on_debug_crash_activate (object o, EventArgs args) {
		bool voluntaryCrashAllowed = true;
		if(voluntaryCrashAllowed) {
			ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString("Done for testing purposes. Chronojump will exit badly"),
					"", "Are you sure you want to crash application?");
			confirmWin.Button_accept.Clicked += new EventHandler(crashing);
		} else {
			new DialogMessage(Constants.MessageTypes.INFO, "Currently disabled.");
		}
	}

	private void crashing (object o, EventArgs args) {
		string [] myString = new String [3];
		LogB.Error(myString[5]);
	}

	private void on_menuitem_server_activate (object o, EventArgs args) {
		LogB.Information("SERVER");
	}


}
