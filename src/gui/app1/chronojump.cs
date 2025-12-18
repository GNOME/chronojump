/*
//La camera 1 va mes rapid que la 0, provar de canviar i activatr primer la 1 a veure que tal

//- Arreglar problema de no coincidencia entre imatge mini i imatge gran, per exemple session6, atleta 1
//- modo simulado curses 4 curses no acaba la ultima
//TODO: que es pugui seleccionar si es vol una webcam o 2
*/


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
 * Copyright (C) 2004-2025   Xavier de Blas <xaviblas@gmail.com>
 */


using System;
using Gtk;
using Gdk;
//using Glade;
using System.IO.Ports;
using Mono.Unix;
using System.IO; //"File" things
using System.Collections; //ArrayList
using System.Collections.Generic; //List
using System.Threading;
using System.Diagnostics;

public partial class ChronoJumpWindow 
{
	// at glade ---->
	Gtk.Window app1;

	/*
	Gtk.HBox hbox_gui_tests;
	Gtk.SpinButton spin_gui_tests;
	Gtk.ComboBox combo_gui_tests;
	Gtk.Button button_carles;
	*/
	
	Gtk.Notebook notebook_chronojump_logo;
	Gtk.Image image_chronojump_logo;
	Gtk.DrawingArea drawingarea_chronojump_logo;

	Gtk.Notebook notebook_start; 		//start window or program
	Gtk.Notebook notebook_sup;

	Gtk.Box box_contacts_capture_top;

	Gtk.Notebook notebook_capture_analyze; //not encoder
	Gtk.Notebook notebook_contacts_execute_or;
	Gtk.Notebook notebook_analyze;
	Gtk.Box vbox_contacts_capture_graph;
	Gtk.Box hbox_message_permissions_at_boot;
	Gtk.Label label_message_permissions_at_boot;
	Gtk.Box hbox_message_camera_at_boot;
	Gtk.Box box_start_modes_and_version;
	Gtk.Box hbox_start_window_sub;

	Gtk.Button button_show_modes_contacts;
	Gtk.Box hbox_change_modes_contacts;
	Gtk.Box hbox_change_modes_encoder;
	Gtk.Box hbox_change_modes_jumps;
	Gtk.Box hbox_change_modes_runs;
	Gtk.Box hbox_change_modes_force_sensor;
	Gtk.RadioButton radio_change_modes_contacts_jumps_simple;
	Gtk.RadioButton radio_change_modes_contacts_jumps_reactive;
	Gtk.RadioButton radio_change_modes_contacts_runs_simple;
	Gtk.RadioButton radio_change_modes_contacts_runs_intervallic;
	Gtk.RadioButton radio_change_modes_contacts_runs_encoder;
	Gtk.RadioButton radio_change_modes_contacts_runs_beepTest;
	Gtk.RadioButton radio_change_modes_contacts_wilight;
	Gtk.RadioButton radio_change_modes_contacts_fourPlatforms;
	Gtk.RadioButton radio_change_modes_contacts_isometric;
	Gtk.RadioButton radio_change_modes_contacts_elastic;
	Gtk.RadioButton radio_change_modes_encoder_gravitatory;
	Gtk.RadioButton radio_change_modes_encoder_inertial;
	Gtk.Image image_change_modes_contacts_jumps_simple;
	Gtk.Image image_change_modes_contacts_jumps_reactive;
	Gtk.Image image_change_modes_contacts_runs_simple;
	//Gtk.Image image_change_modes_contacts_runs_reactive;
	Gtk.Image image_change_modes_contacts_runs_intervallic;
	Gtk.Image image_change_modes_contacts_wilight;
	Gtk.Image image_change_modes_contacts_fourPlatforms;
	Gtk.Image image_change_modes_contacts_force_sensor;
	Gtk.Image image_change_modes_contacts_force_sensor1;
	Gtk.Image image_change_modes_contacts_runs_encoder;
	Gtk.Image image_change_modes_encoder_gravitatory;
	Gtk.Image image_change_modes_encoder_inertial;

	Gtk.Alignment alignment_contacts_show_graph_table;
	Gtk.Box box_contacts_capture_show_need_one;
	Gtk.Label label_contacts_capture_show_need_one;
	Gtk.CheckButton check_contacts_capture_graph;
	Gtk.CheckButton check_contacts_capture_table;
	Gtk.Button button_contacts_capture_save_image;

	Gtk.EventBox eventbox_button_show_modes_contacts;
	Gtk.EventBox eventbox_change_modes_contacts_jumps_simple;
	Gtk.EventBox eventbox_change_modes_contacts_jumps_reactive;
	Gtk.EventBox eventbox_change_modes_contacts_runs_simple;
	Gtk.EventBox eventbox_change_modes_contacts_runs_intervallic;
	Gtk.EventBox eventbox_change_modes_contacts_runs_encoder;
	Gtk.EventBox eventbox_change_modes_contacts_isometric;
	Gtk.EventBox eventbox_change_modes_contacts_elastic;
	Gtk.EventBox eventbox_change_modes_encoder_gravitatory;
	Gtk.EventBox eventbox_change_modes_encoder_inertial;
	Gtk.EventBox eventbox_radio_mode_contacts_capture;
	Gtk.EventBox eventbox_radio_mode_contacts_analyze;
	Gtk.EventBox eventbox_button_open_chronojump;
	Gtk.EventBox eventbox_button_help_close;
	Gtk.EventBox eventbox_button_news_close;
	Gtk.EventBox eventbox_button_exit_cancel;
	Gtk.EventBox eventbox_button_exit_confirm;

	Gtk.Box hbox_contacts_sup_capture_analyze_two_buttons;
	Gtk.Box hbox_radio_mode_contacts_analyze_buttons;
	Gtk.Box hbox_radio_mode_contacts_analyze_jump_simple_buttons;

	//radio group
	Gtk.RadioButton radio_mode_contacts_capture;
	Gtk.RadioButton radio_mode_contacts_analyze;

	//radio group
	Gtk.RadioButton radio_mode_contacts_jumps_profile;
	Gtk.RadioButton radio_mode_contacts_jumps_dj_optimal_fall;
	Gtk.RadioButton radio_mode_contacts_jumps_weight_fv_profile;
	Gtk.RadioButton radio_mode_contacts_jumps_asymmetry;
	Gtk.RadioButton radio_mode_contacts_jumps_evolution;
	Gtk.RadioButton radio_mode_contacts_jumps_rj_fatigue;
	Gtk.RadioButton radio_mode_contacts_runs_evolution;
	Gtk.RadioButton radio_mode_contacts_sprint;
	Gtk.RadioButton radio_mode_contacts_advanced;
	Gtk.RadioButton radio_mode_contacts_export_csv;
	Gtk.Notebook notebook_analyze_export_csv_data;

	Gtk.RadioButton radio_contacts_export_individual_current_session;
	Gtk.RadioButton radio_contacts_export_individual_all_sessions;
	Gtk.RadioButton radio_contacts_export_groupal_current_session;
	Gtk.Label label_contacts_export_person;
	Gtk.Label label_contacts_export_session;
	Gtk.Label label_contacts_export_result;
	Gtk.Button button_contacts_export_result_open;

	Gtk.Label label_sprint_person_name;

	Gtk.Label label_version;
	//Gtk.Image image_selector_start_encoder_inertial;

	Gtk.Image image_persons_new_1;
	Gtk.Image image_persons_new_plus;
	Gtk.Image image_persons_open_1;
	Gtk.Image image_persons_open_plus;

	Gtk.Image image_encoder_export_signal;

	//contact tests execute buttons
	Gtk.Image image_button_finish;
	Gtk.Image image_button_finish1;
	Gtk.Image image_button_finish2;
	Gtk.Image image_button_cancel; //needed this specially because theme cancel sometimes seems "record"
	Gtk.Image image_button_cancel1;
	Gtk.Image image_button_cancel2;
	//encoder tests execute buttons
	//Gtk.Image image_encoder_capture_execute;
	
	Gtk.Box fullscreen_capture_box_buttons_finish_cancel;
	Gtk.Button fullscreen_capture_button_finish;
	Gtk.Button fullscreen_capture_button_cancel;
	Gtk.Button fullscreen_button_encoder_capture_finish_cont;
	Gtk.Button fullscreen_button_fullscreen_contacts;
	Gtk.Button fullscreen_button_fullscreen_encoder;
	Gtk.Button fullscreen_button_fullscreen_exit;
	Gtk.ProgressBar fullscreen_capture_progressbar;
	Gtk.Label fullscreen_label_person;
	Gtk.Label fullscreen_label_exercise;
	Gtk.Label fullscreen_label_message;
	Gtk.DrawingArea fullscreen_capture_drawingarea_cairo;
	Gtk.Button button_dialog_result_contacts;
	Gtk.Button button_dialog_result_contacts_4p;

	Gtk.VPaned vpaned_tests;
	Gtk.HBox hbox_contacts_graph_table_controls;
	Gtk.Frame frame_contacts_graph_table;
	Gtk.HPaned hpaned_contacts_graph_table;
	Gtk.TreeView treeview_persons;
	
	Gtk.Box hbox_combo_select_jumps;
	Gtk.Box hbox_combo_select_jumps_rj;
	Gtk.Box hbox_combo_select_runs;
	Gtk.Box hbox_combo_select_runs_interval;
	Gtk.Box hbox_combo_select_contacts_top_with_arrows;
	Gtk.Box hbox_combo_select_contacts_top;

	//auto mode	
	//Gtk.Box hbox_jump_types_options;
	Gtk.Box hbox_jump_auto_controls;
	Gtk.Image image_auto_person_skip;
	Gtk.Image image_auto_person_remove;
	Gtk.Button button_auto_start;
	Gtk.Label label_jump_auto_current_person;
	Gtk.Label label_jump_auto_current_test;
		
	Gtk.Image image_line_session_avg;
	Gtk.Image image_line_session_max;
	Gtk.Image image_line_person_avg;
	Gtk.Image image_line_person_max;
	Gtk.Image image_line_person_max_all_sessions;

	//to GTK3 colorize
	Gtk.Frame frame_session;
	Gtk.Box box_database;
	Gtk.Box hbox_frame_database_top;
	Gtk.Box vbox_frame_database_border;
	Gtk.Image image_database_manage_blue;
	Gtk.Image image_database_manage_yellow;
	Gtk.Box vbox_frame_session_border;
	Gtk.Box box_session_more;
	Gtk.Box box_session_load_or_import;
	Gtk.Box box_session_delete;
	Gtk.Box box_session_export;
	Gtk.Box box_session_import;
	Gtk.Box box_session_import_current;
	Gtk.Box box_session_import_confirm;
	Gtk.Box box_session_backup;
	Gtk.Box box_session_data_folder;
	Gtk.Box box_session_import_from_csv;
	Gtk.Box box_help;
	Gtk.Box vbox_news2;
	Gtk.Frame frame_news_downloading;

	Gtk.Box vbox_jumps;
	//Gtk.Box hbox_jumps_test;
	Gtk.Box hbox_jumps_rj;
	Gtk.Box vbox_runs;
	Gtk.Box hbox_runs_interval_all; //normal and compujump
	Gtk.Box vbox_runs_interval;
	Gtk.Box vbox_runs_interval_compujump;

	//menu person
	Gtk.Box vbox_persons;
	Gtk.Box hbox_frame_persons_top;
	//Gtk.Alignment alignment44;
	Gtk.Button button_persons_up;
	Gtk.Button button_persons_down;

	//tests
	Gtk.Notebook notebook_contacts_capture_doing_wait;
	Gtk.Button button_contacts_bells;
	Gtk.Frame frame_jumps_automatic;
	Gtk.Notebook notebook_jumps_automatic;
	Gtk.Box hbox_contacts_device_adjust_threshold;

	Gtk.Button button_contacts_edit_selected;
	Gtk.Button button_contacts_repair_selected;
	Gtk.Button button_contacts_delete_selected;

	Gtk.Image extra_windows_jumps_image_dj_fall_calculate;
	Gtk.Image extra_windows_jumps_image_dj_fall_predefined;
	Gtk.Box hbox_extra_window_jumps_fall_height;
	Gtk.Label label_contacts_exercise_info;

	Gtk.Box vbox_execute_test;
	Gtk.Button button_execute_test;
	Gtk.Viewport viewport_chronopics;
	Gtk.Viewport viewport_chronopic_encoder;

	Gtk.Button button_contacts_json_upload;

	Gtk.Box box_contacts_insert_test;
	Gtk.Notebook notebook_contacts_insert_test;
	Gtk.SpinButton contacts_insert_test_spin_total_time;
	Gtk.SpinButton contacts_insert_test_spin_speed_km_h;
	Gtk.SpinButton contacts_insert_test_spin_track_1_time;
	Gtk.SpinButton contacts_insert_test_spin_track_2_time;
	Gtk.Button contacts_insert_test_button_insert;
	Gtk.Button contacts_insert_test_button_insert_and_upload;
	Gtk.Label contacts_insert_test_label_done;

	//detect devices
	Gtk.Box vbox_micro_discover;
	Gtk.Label label_micro_discover_title;
	Gtk.Label label_micro_discover_not_found;
	Gtk.Frame frame_micro_discover;
	Gtk.VBox vbox_micro_discover_main;
	Gtk.Box	box_micro_discover_assign_manually;
	Gtk.ButtonBox buttonbox_micro_discover_assign_manually;
	Gtk.Button button_micro_discover_refresh;
	Gtk.Image image_micro_discover_refresh;
	Gtk.Grid grid_micro_discover;
	Gtk.Box box_micro_discover_nc;
	Gtk.Label label_micro_discover_nc_current_mode;
	Gtk.Label label_micro_discover_connect_error;
	Gtk.Box hbox_contacts_detect_and_execute;
	Gtk.Box hbox_encoder_detect_and_execute;
	Gtk.Button button_contacts_detect;
	Gtk.Button button_encoder_detect;
	Gtk.Button button_contacts_detect_small;
	Gtk.Button button_encoder_detect_small;
	Gtk.CheckButton check_discover_advanced;
	Gtk.Label label_discover_advanced;
	Gtk.Image image_discover_advanced;
	Gtk.EventBox eventbox_button_micro_discover_cancel_close;
	Gtk.Button button_micro_discover_cancel_close;
	Gtk.Image image_button_micro_discover_cancel_close;
	Gtk.Label label_button_micro_discover_cancel_close;
	Gtk.Image image_button_micro_discover_assign_manually_cancel;
	//Gtk.Image image_micro_discover_mode;

	Gtk.Label label_threshold;

	//force sensor
	Gtk.Box hbox_capture_phases;
	Gtk.Box hbox_capture_time;

	//widgets for enable or disable
	Gtk.Frame frame_persons;
	Gtk.Frame frame_persons_top;
	Gtk.Box vbox_persons_bottom;
	Gtk.Box hbox_persons_bottom_photo;
	Gtk.Button button_recuperate_person;
	Gtk.Button button_recuperate_persons_from_session;
	Gtk.Button button_person_add_single;
	Gtk.Button button_person_add_multiple;

	Gtk.Button button_contacts_exercise_close_and_capture;
	Gtk.Notebook notebook_execute;
	Gtk.Notebook notebook_options_top;
		
	Gtk.EventBox eventbox_image_test;
	Gtk.Image image_test;
	Gtk.Button button_image_test_zoom;
	Gtk.Image image_test_zoom;
	Gtk.Button button_image_test_add_edit;
	Gtk.Image image_test_add_edit;
	Gtk.Button button_inspect_last_test_run_simple;
	Gtk.Button button_inspect_last_test_run_intervallic;
	//Gtk.VBox vbox_last_test_buttons;

	Gtk.Box hbox_chronopics_and_more;
	Gtk.Button button_contacts_devices_networks;
	Gtk.Button button_threshold;
	Gtk.Button button_force_sensor_adjust;
	Gtk.Button button_force_sensor_sync;

	//non standard icons	
	//Gtk.Image image_jump_reactive_bell;
	//Gtk.Image image_run_interval_bell;
	Gtk.Image image_contacts_repair_selected;
	Gtk.Image image_jump_type_delete_simple;
	Gtk.Image image_jump_type_delete_reactive;
	Gtk.Image image_run_type_delete_simple;
	Gtk.Image image_run_type_delete_intervallic;

	Gtk.Image image_results_session_zoom;

	//encoder
	//Gtk.Image image_encoder_analyze_zoom;
	Gtk.Image image_encoder_analyze_stats;
	Gtk.Image image_encoder_analyze_mode_options_close_and_analyze;
	Gtk.Image image_encoder_analyze_image_save;
	Gtk.Image image_encoder_analyze_1RM_save;
	Gtk.Image image_encoder_analyze_table_save;
	Gtk.Image image_encoder_inertial_instructions;
	Gtk.Label label_gravitatory_vpf_propulsive;

	//forcesensor
	Gtk.Image image_forcesensor_analyze_save_signal;
	Gtk.Image image_forcesensor_analyze_save_rfd_auto;
	Gtk.Image image_forcesensor_analyze_save_rfd_manual;

	Gtk.Box vbox_prefs_util_help;

	Gtk.RadioButton radio_menu_2_2_2_jumps;
	Gtk.RadioButton radio_menu_2_2_2_races;
	Gtk.RadioButton radio_menu_2_2_2_wilight;
	Gtk.RadioButton radio_menu_2_2_2_fourPlatforms;
	Gtk.RadioButton radio_menu_2_2_2_force;
	Gtk.RadioButton radio_menu_2_2_2_elastic;
	Gtk.RadioButton radio_menu_2_2_2_weights;
	Gtk.RadioButton radio_menu_2_2_2_inertial;
	Gtk.EventBox eventbox_radio_menu_2_2_2_jumps;
	Gtk.EventBox eventbox_radio_menu_2_2_2_races;
	Gtk.EventBox eventbox_radio_menu_2_2_2_force;
	Gtk.EventBox eventbox_radio_menu_2_2_2_elastic;
	Gtk.EventBox eventbox_radio_menu_2_2_2_weights;
	Gtk.EventBox eventbox_radio_menu_2_2_2_inertial;
	Gtk.Notebook notebook_menu_2_2_2; //0 jumps, 1 races, 2 isometric/elastic/weights/inertial
	Gtk.Label label_selector_menu_2_2_2_title;
	Gtk.Label label_selector_menu_2_2_2_desc;
	Gtk.Alignment align_label_selector_menu_2_2_2_desc;

	Gtk.Label label_exit_confirm;
	Gtk.Button button_exit_cancel;
	// <---- at glade

	Random rand;

	Gtk.ComboBoxText combo_select_jumps;
	Gtk.ComboBoxText combo_select_jumps_rj;
	Gtk.ComboBoxText combo_select_runs;
	Gtk.ComboBoxText combo_select_runs_interval;
	Gtk.ComboBoxText combo_select_contacts_top;

	//new since 1.6.3. Using gui/cjCombo.cs
	CjComboSelectJumps comboSelectJumps;
	CjComboSelectJumpsRj comboSelectJumpsRj;
	CjComboSelectRuns comboSelectRuns;
	CjComboSelectRunsI comboSelectRunsI;
	CjCombo comboSelectContactsTop;

	//persons
	private TreeViewPersons myTreeViewPersons;

	private Preferences preferences;
	private List<ForceSensorRFD> rfdList;
	private ForceSensorImpulse impulse;

	private static Person currentPerson;
	private static Session currentSession;
	private static PersonSession currentPersonSession;
	private static Jump currentJump;
	private static JumpRj currentJumpRj;
	private static Run currentRun;
	private static RunInterval currentRunInterval;
	private static FourPlatforms currentFourPlatforms;

	//to be able to resize cairo jumpRj, runI graphs without needing to check sql all the time
	private static JumpRj selectedJumpRj;
	private static RunInterval selectedRunInterval;
	private static RunType selectedRunIntervalType; //we need this for variable distances

	private static EventExecute currentEventExecute; //this aplies to jumps and races (simple or interval)

	//Used by Cancel and Finish
	private static EventType currentEventType;

	private static JumpType currentJumpType;
	private static JumpType currentJumpRjType;
	bool thisJumpIsSimple;	//needed on updating
	private static RunType currentRunType;
	private static RunType currentRunIntervalType;
	bool thisRunIsSimple;	//needed on updating
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
	PersonMergeWindow personMergeWin;
	PersonSelectWindow personSelectWin;
	JumpsMoreWindow jumpsMoreWin;
	JumpsRjMoreWindow jumpsRjMoreWin;
	EditJumpWindow editJumpWin;
	EditJumpRjWindow editJumpRjWin;
	RepairJumpRjWindow repairJumpRjWin;
	EditBeepTestWindow editBeepTestWin;
	EditForceSensorWindow editForceSensorWin;
	EditEncoderWindow editEncoderWin;
	JumpTypeAddWindow jumpTypeAddWin;
	EditWilightWindow editWilightWin;
	EditFourPlatformsWindow editFourPlatformsWin;

	RunsMoreWindow runsMoreWin;
	RunsIntervalMoreWindow runsIntervalMoreWin;
	RunTypeAddWindow runTypeAddWin;
	EditRunWindow editRunWin;
	RepairRunIntervalWindow repairRunIntervalWin;
	EditRunIntervalWindow editRunIntervalWin;
	EditRunEncoderWindow editRunEncoderWin;

	ConfirmWindowJumpRun confirmWinJumpRun;	//for deleting jumps and RJ jumps (and runs)
	ReportWindow reportWin;
	FeedbackJumpsRj feedbackJumpsRj;
	FeedbackRunsInterval feedbackRunsI;
	FeedbackEncoder feedbackEncoder;
	FeedbackWindow feedbackWin;
	GenericWindow genericWin;
		
	ExecuteAutoWindow executeAutoWin;
	DialogResult dialogResult;

	static Thread pingThread;

	private bool createdStatsWin;
	
	private UtilAll.OperatingSystems operatingSystem;
	private string progVersion;
	private string progName;
	private enum notebook_start_pages { PROGRAM, SENDLOG, EXITCONFIRM, SOCIALNETWORKPOLL, FULLSCREENCAPTURE }
	private enum notebook_sup_pages { START, TESTS, SESSION, NETWORKSPROBLEMS, PREFSUTILHELP, NEWS, MICRODISCOVER, PERSON, DATABASE }
	private enum notebook_contacts_execute_or_pages { EXECUTE, OPTIONSCONTACTS, OPTIONSENCODER, FORCESENSORADJUST, RACEINSPECTOR }
	private enum notebook_execute_pages { JUMPSSIMPLE, JUMPSREACTIVE, RUNSSIMPLE, RUNSINTERVALLIC, FORCESENSOR, RUNSENCODER }
	private enum notebook_options_top_pages { JUMPSSIMPLE, JUMPSREACTIVE, RUNSSIMPLE, RUNSINTERVALLIC, FORCESENSOR, RUNSENCODER }
	private enum notebook_analyze_pages { STATISTICS, JUMPSPROFILE, JUMPSDJOPTIMALFALL, JUMPSWEIGHTFVPROFILE,
		JUMPSASYMMETRY, JUMPSEVOLUTION, JUMPSRJFATIGUE,
		RUNSEVOLUTION, SPRINT, CONTACTS_EXPORT_CSV, SIGNAL_AI, ENCODER}
	private enum notebook_analyze_export_csv_data_pages { JUMPS, RUNS, OTHER }

	private string runningFileName; //useful for knowing if there are two chronojump instances

	//int chronopicCancelledTimes = 0;

	ChronopicRegister chronopicRegister;
	Chronopic2016 cp2016;
	private WichroCapture wichroCapture;
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
	//private bool showSocialNetworkPoll;
	private SplashWindow splashWin;
	private bool showSendLog;

	/*
		note sometimes sensor is still capturing and chronojump closes nicely (then chronojump_running is being deleted),
		so crashedBefore is not useful to identify if forceSensor is still capturing.
		Better use firstCapture, and do it everytime Chronojump is opened
		*/
	private bool crashedBefore; //unused
	private bool firstCapture;
	private BlinkImage blinkCapture;
	private BlinkImage blinkOther;

	public ChronoJumpWindow(string progVersion, string progName, string runningFileName, SplashWindow splashWin,
			bool showSendLog, string sendLogMessage, bool crashedBefore, string topMessage, bool showCameraStop, bool debugModeAtStart)
	{
		this.progVersion = progVersion;
		this.progName = progName;
		this.runningFileName = runningFileName;
		this.splashWin = splashWin;
		this.showSendLog = showSendLog;
		this.crashedBefore = crashedBefore;

		firstCapture = true;

		//record GetOsEnum on variables to not call it all the time
		operatingSystem = UtilAll.GetOSEnum();

		/*
		Glade.XML gxml;
		gxml = Glade.XML.FromAssembly (Util.GetGladePath() + "app1.glade", "app1", "chronojump");
		gxml.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "app1.glade", null);
		connectWidgets (builder);
		connectWidgetsContactsExercise (builder);
		connectWidgetsEncoder (builder);
		connectWidgetsEventExecute (builder);
		connectWidgetsExhibition (builder);
		connectWidgetsIcons (builder);
		connectWidgetsJump (builder);
		connectWidgetsJumpAsymmetry (builder);
		connectWidgetsJumpDjOptimalFall (builder);
		connectWidgetsJumpsProfile (builder);
		connectWidgetsJumpsRjFatigue (builder);
		connectWidgetsJumpsRunsEvolution (builder);
		connectWidgetsJumpsWeightFVProfile (builder);
		connectWidgetsForceSensor (builder);
		connectWidgetsForceSensorAnalyze (builder);
		connectWidgetsSignalAnalyze (builder);
		connectWidgetsMenu (builder);
		connectWidgetsMenuTiny (builder);
		connectWidgetsNetworks (builder);
		connectWidgetsNews (builder);
		connectWidgetsPersons (builder);
		connectWidgetsRemotePerson (builder);
		connectWidgetsRestTime (builder);
		connectWidgetsRun (builder);
		connectWidgetsRunEncoder (builder);
		connectWidgetsRunEncoderAnalyze (builder);
		connectWidgetsSendLogAndPoll (builder);
		connectWidgetsSessionDelete (builder);
		connectWidgetsSessionLoadAndImport (builder);
		connectWidgetsSessionMain (builder);
		connectWidgetsShortcuts (builder);
		connectWidgetsSprint (builder);
		connectWidgetsBeepTest (builder);
		connectWidgetsWilight (builder);
		connectWidgetsFourPlatforms (builder);
		connectWidgetsStats (builder);
		connectWidgetsTrigger (builder);
		connectWidgetsWebcam (builder);
		connectWidgetsPresentation (builder);
		builder.Autoconnect (this);

		//put an icon to window
		UtilGtk.IconWindow(app1);

		string buildVersion = UtilAll.ReadVersionFromBuildInfo();
		LogB.Information("Build version:" + buildVersion);

		//if buildVersion has eg. 1.9.0-1980-gc4b2941f remove the git hash (only show 1.9.0-1980)
		string [] buildVersionSplit = buildVersion.Split(new char[] {'-'});
		if(buildVersionSplit.Length == 3)
			buildVersion = buildVersionSplit[0] + "-" + buildVersionSplit[1];

		label_version.Text = buildVersion;
		label_version.Name = "lightCss";

		//manage app1 will not be hiding other windows at start
		app1Shown = false;
		needToShowChronopicRegisterWindow = false;

		//show chronojump logo on down-left area
		changeTestImage("", "", "LOGO");
	
		eventbox_image_test.Name = "whiteBgCss"; //white bg
	
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
		//it will open preferences on default db or on chronojumpCloudRead.db if reading from cloud
		if(configChronojump == null)
			configChronojump = new Config();
		configChronojump.Read ();

		loadPreferencesAtStartOrCloudViewChangeDB ();

		Config.RUserURLStatic = configChronojump.RUserURL;
		Config.RscriptUserURLStatic = configChronojump.RscriptUserURL;
		Config.PythonUserURLStatic = configChronojump.PythonUserURL;

		Config.UseSystemColor = preferences.colorBackgroundOsColor;

		checkbutton_video_contacts.Visible = true;

		if(topMessage != "") {
			label_message_permissions_at_boot.Text = topMessage;
			hbox_message_permissions_at_boot.Visible = true;
		}

		//showSocialNetworkPoll = (preferences.socialNetworkDatetime == "");
		//show send log if needed or other messages
		if (showSendLog)
		{
			show_send_log(sendLogMessage, preferences.crashLogLanguage);
			notebook_start.CurrentPage = Convert.ToInt32(notebook_start_pages.SENDLOG);
		}
		/*
		else if (showSocialNetworkPoll)
		{
			notebook_start.CurrentPage = Convert.ToInt32(notebook_start_pages.SOCIALNETWORKPOLL);
			socialNetworkPollInit();
		}
		*/
		else
			notebook_sup.CurrentPage = Convert.ToInt32(notebook_sup_pages.START);

		if(showCameraStop)
			hbox_message_camera_at_boot.Visible = true;

		
		// ------ Creating widgets ------

		if(splashWin != null)
			splashWin.UpdateLabel(Catalog.GetString(Constants.SplashMessages[8]));

		createTreeView_persons (treeview_persons);

		createTreeView_resultsSession (treeview_results_session);
		/*
		createTreeView_jumps (treeview_jumps);
		createTreeView_jumps_rj (treeview_jumps_rj);
		createTreeView_runs (treeview_runs);
		createTreeView_runs_interval (treeview_runs_interval);
		createTreeView_wilight (treeview_wilight);
		*/
		createTreeView_runs_interval_sprint (treeview_runs_interval_sprint);

		rfdList = SqliteForceSensorRFD.SelectAll(false);
		impulse = SqliteForceSensorRFD.SelectImpulse(false);
		initForceSensor ();
		initRunEncoder ();

		treeviewResultsSessionAllPersonsAllTests ();

		/*
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_person_close,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_show_modes_contacts,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_change_modes_contacts_jumps_simple,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_change_modes_contacts_jumps_reactive,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_change_modes_contacts_runs_simple,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_change_modes_contacts_runs_intervallic,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_change_modes_contacts_isometric,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_change_modes_contacts_elastic,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_change_modes_contacts_runs_encoder,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_change_modes_encoder_gravitatory,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_change_modes_encoder_inertial,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_show_modes_encoder,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_radio_mode_contacts_capture,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_radio_mode_contacts_analyze,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_radio_mode_encoder_capture_small,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_radio_mode_encoder_analyze_small,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_radio_mode_pulses_small,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_radio_mode_multi_chronopic_small,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_open_chronojump,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_help_close,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_news_close,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_micro_discover_cancel_close,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_exit_cancel,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_button_exit_confirm,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_radio_menu_2_2_2_jumps,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_radio_menu_2_2_2_races,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_radio_menu_2_2_2_force,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_radio_menu_2_2_2_elastic,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_radio_menu_2_2_2_weights,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.EventBoxColorBackgroundActive (eventbox_radio_menu_2_2_2_inertial,
				UtilGtk.Colors.YELLOW, UtilGtk.Colors.YELLOW_LIGHT);
		*/

		app1s_eventboxes_paint();

		createComboSelectJumps(true);
		createComboSelectJumpsDjOptimalFall(true);
		createComboSelectJumpsWeightFVProfile(true);
		createComboSelectJumpsAsymmetry(true);
		createComboSelectJumpsEvolution(true);
		createComboSelectJumpsRj(true);
		createComboSelectJumpsRjFatigue(true);
		createComboSelectJumpsRjFatigueNum(true);
		createCombo_combo_jumps_rj_fatigue_divide_in ();
		createComboSelectRuns(true);
		createComboSelectRunsEvolution(true);
		createComboSelectRunsEvolutionDistance();
		createComboSelectRunsInterval(true);
		createComboSelectContactsTop (); //need to at least have it not null (to not crash on a import session)

		createdStatsWin = false;

		createComboSessionLoadTags(true);

		feedbackJumpsRj = new FeedbackJumpsRj (preferences);
		feedbackRunsI = new FeedbackRunsInterval (preferences);
		feedbackEncoder = new FeedbackEncoder (preferences);
		feedbackWin = FeedbackWindow.Create();
		feedbackWin.FakeButtonClose.Clicked += new EventHandler(on_feedback_closed);
		feedbackWin.FakeButtonQuestionnaireLoad.Clicked -= new EventHandler(on_feedback_questionnaire_load);
		feedbackWin.FakeButtonQuestionnaireLoad.Clicked += new EventHandler(on_feedback_questionnaire_load);
		//to have objects ok to be able to be readed before viewing the feedbackWin

		on_extra_window_runs_interval_test_changed(new object(), new EventArgs());
		on_extra_window_runs_test_changed(new object(), new EventArgs());
		on_extra_window_jumps_rj_test_changed(new object(), new EventArgs());
		on_extra_window_jumps_test_changed(new object(), new EventArgs());
		//changeTestImage("", "", "LOGO");
		//We have no session, mark some widgets as ".Sensitive = false"
		sensitiveGuiNoSession();

		rand = new Random(40);

		putNonStandardIcons();
		eventExecutePutNonStandardIcons();


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

		initContacts1Time ();
		initEncoder1Time ();

		//done before configInitRead because that will change some Tooltips
		addShortcutsToTooltips(operatingSystem == UtilAll.OperatingSystems.MACOSX);

		LogB.Information("Calling configInitRead from gui / ChronojumpWindow");
		configInitDoAtBoot ();

		if (debugModeAtStart)
			on_preferences_debug_mode_start (new object (), new EventArgs ());
	}

	//separated because on cloud, copy to temp has a thread and we want to ensure this is called when copy is done and database is changed
	private void ChronojumpWindowCont ()
	{
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
			changeSessionAtStartOrCloudViewChangeDB ();

		initialize_menu_or_menu_tiny();
		vbox_persons_bottom.Visible = preferences.personPhoto && ! check_menu_session.Active;

		presentationPrepare ();

		testNewStuff();

		//show before destroying/hiding app1 to see if this fixes rare problems of exiting/not showing app1
		LogB.Information("Showing app1");
		app1.Show();

		//ensure chronopicRegisterWindow is shown after (on top of) app1
		app1Shown = true;

		//lastMode WILIGHT only available if is defined on configChronojump
		if (preferences.lastMode == Constants.Modes.WILIGHT && ! configChronojump.Wilight)
			preferences.lastMode = Constants.Modes.UNDEFINED;

		//in networks starting mode is always the defined on chronojump_config CompujumpStationMode
		if (! configChronojump.Compujump)
		{
			if(! showSendLog && //! showSocialNetworkPoll &&
					preferences.loadLastModeAtStart &&
					preferences.lastMode != Constants.Modes.UNDEFINED)
			{
				changeModeAtStartOrCloudViewChangeDB ();
			}
			else if (preferences.lastMode != Constants.Modes.UNDEFINED)
				current_mode = preferences.lastMode; //needed for show_start_page () below
		}

		/*
		 * Disabled until fix some bug with cloud view change database
		 *
		//1 if no session, show new/load session "win"
		//2 but do not show session "win" if cloud and still have not loaded the database
		if (currentSession == null && 	// 1
				! showSendLog &&
				! (configChronojump.ReadFromCloudMainPath != "" && configChronojump.LastDBFullPath == "") && // 2
				! menuSessionIsActive ())
			menuSessionDoClick (); //have session "win" opened
		*/

		//done after app1.Show in order to be able to gather the colors
		doLabelsContrast(configChronojump.PersonWinHide);

		if(needToShowChronopicRegisterWindow)
		{
			LogB.Information("Show chronopic resgister win");
			chronopicRegisterWin.Show();
		}

		//LogB.Information (string.Format ("! showSendLog: {0}, configChronojump.ReadFromCloudMainPath == '': {1}, ! configChronojump.CanOpenExternalDB: {2}",
		//			! showSendLog, configChronojump.ReadFromCloudMainPath == "", ! configChronojump.CanOpenExternalDB));

		if(! showSendLog && //! showSocialNetworkPoll)
			configChronojump.ReadFromCloudMainPath == "" && ! configChronojump.CanOpenExternalDB)
		{
			if (shouldAskBackupScheduled ())
				backupScheduledAsk ();
			else if(notebook_sup.CurrentPage == Convert.ToInt32(notebook_sup_pages.START))
				show_start_page ();
		} else if (configChronojump.ReadFromCloudMainPath != "") //TODO rewrite these nested conditions
			show_start_page ();

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

	private void changeSessionAtStartOrCloudViewChangeDB ()
	{
		// 1) to avoid impossibility to start Chronojump if there's any problem with this session, first put this to false
		SqlitePreferences.Update(SqlitePreferences.LoadLastSessionAtStart, false, false);

		// 2) load the session (but check if it really exists (extra check))
		Session sessionLoading = SqliteSession.Select (preferences.lastSessionID.ToString());
		if(sessionLoading.UniqueID != -1)
		{
			currentSession = sessionLoading;
			on_load_session_accepted();

			// select last person
			if (preferences.lastPersonID > 0)
				selectRowTreeView_persons (treeview_persons,
						myTreeViewPersons.FindRow (preferences.lastPersonID));

			// 3) put preference to true again
			SqlitePreferences.Update(SqlitePreferences.LoadLastSessionAtStart, true, false);
		}
		/* commented, as this will be done after mode change
		   else
		   if(! check_menu_session.Active)
		   check_menu_session.Click(); //have session menu opened
		   */
	}

	private void changeModeAtStartOrCloudViewChangeDB ()
	{
		// 1) to avoid impossibility to start Chronojump if there's any problem with this mode, first put this to false
		SqlitePreferences.Update(SqlitePreferences.LoadLastModeAtStart, false, false);

		// 2) change mode
		changeModeCheckRadios (preferences.lastMode); //this will update current_mode

		// 3) put preference to true again
		SqlitePreferences.Update(SqlitePreferences.LoadLastModeAtStart, true, false);
	}

	private void initContacts1Time ()
	{
		followSignals = false;
		check_contacts_capture_graph.Active = preferences.contactsCaptureDisplay.ShowGraph;
		check_contacts_capture_table.Active = preferences.contactsCaptureDisplay.ShowTable;
		followSignals = true;
		//call here to have the gui updated and preferences.encoderCaptureShowOnlyBars correctly assigned
		on_check_contacts_capture_show_modes_clicked (new object (), new EventArgs ());

		fourPlatformsInit1Time ();
	}

	//used on this free labels that have to contrast with background
	private void doLabelsContrast(bool personsAtTop)
	{
		if (UtilGtk.LogoBlueOrWhite (Config.ColorBackground))
		{
			image_chronojump_logo.Pixbuf = Chronojump.MyPixbuf.Get(
					null, Util.GetImagePath(false) + Constants.FileNameLogoBlueTransp);

			image_logo_contacts.Pixbuf = Chronojump.MyPixbuf.Get(
					null, Util.GetImagePath(false) + Constants.FileNameLogoHorizontalBlue);

			fullscreen_image_logo_horizontal_blue.Visible = true;
			fullscreen_image_logo_horizontal_white.Visible = false;
		} else {
			image_chronojump_logo.Pixbuf = Chronojump.MyPixbuf.Get(
					null, Util.GetImagePath(false) + Constants.FileNameLogoWhiteTransp);

			image_logo_contacts.Pixbuf = Chronojump.MyPixbuf.Get(
					null, Util.GetImagePath(false) + Constants.FileNameLogoHorizontalWhite);

			fullscreen_image_logo_horizontal_blue.Visible = false;
			fullscreen_image_logo_horizontal_white.Visible = true;
		}

		if(personsAtTop)
		{
			if(! Config.UseSystemColor)
			{
				UtilGtk.ContrastLabelsBox (Config.ColorBackgroundIsDark, hbox_top_person);
				UtilGtk.ContrastLabelsGrid (Config.ColorBackgroundIsDark, grid_rest_time_contacts);
				UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, app1s_label_copyToCloud1);
			}

			if(! Config.UseSystemColor && Config.ColorBackgroundIsDark)
			{
				image_contacts_rest_time_dark_blue.Visible = false;
				image_contacts_rest_time_clear_yellow.Visible = true;
			} else {
				image_contacts_rest_time_dark_blue.Visible = true;
				image_contacts_rest_time_clear_yellow.Visible = false;
			}
		} else {
			if(! Config.UseSystemColor)
			{
				UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, label_current_database);
				UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, label_current_session);
			}

			Pixbuf pixbuf;

			pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "cloud_view_blue.png");
			//if(Config.ColorBackgroundIsDark)
			//	pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "cloud_yellow.png");
			image_cloud_view.Pixbuf = pixbuf;

			personsPhotoShowIfNeeded ();
		}

		// "big" label names are assigned here.
		// They will be reassigned to a composed name just later on UtilGtk.ContrastLabelsLabel ()
		label_jump_simple_height.Name = "big_monospace";
		label_jump_simple_height_value.Name = "big_monospace";
		label_run_simple_time.Name = "big_monospace";
		label_run_simple_time_value.Name = "big_monospace";

		label_force_sensor_value_max.Name = "big_monospace";
		label_force_sensor_value.Name = "big_monospace";
		label_force_sensor_value_min.Name = "big_monospace";
		label_force_sensor_value_best_second.Name = "big_monospace";
		label_force_sensor_value_rfd.Name = "big_monospace";

		label_beepTest_time.Name = "big_monospace";
		label_beepTest_stage.Name = "big_monospace";
		label_beepTest_lap.Name = "big_monospace";
		label_beepTest_speed.Name = "big_monospace";
		label_beepTest_runStatus_value.Name = "big_monospace";

		encoder_countdown_label.Name = "monospace";

		if(! Config.UseSystemColor)
		{
			UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundIsDark, app1s_notebook);
			UtilGtk.ContrastLabelsBox (Config.ColorBackgroundIsDark, vbox_prefs_util_help);
			UtilGtk.ContrastLabelsBox (Config.ColorBackgroundIsDark, vbox_micro_discover);
			UtilGtk.ContrastLabelsBox (Config.ColorBackgroundIsDark, vbox_person);
			UtilGtk.ContrastLabelsBox (Config.ColorBackgroundIsDark, box_database);

			/*
			//notebook_sup
			UtilGtk.WidgetColor (notebook_sup, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundShiftedIsDark, notebook_sup);
			*/
			//start (modes)
			UtilGtk.ContrastLabelsBox (Config.ColorBackgroundIsDark, box_start_modes_and_version);
			UtilGtk.WidgetColor (hbox_start_window_sub, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsBox (Config.ColorBackgroundShiftedIsDark, hbox_start_window_sub);
			UtilGtk.WidgetColor (notebook_menu_2_2_2, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundShiftedIsDark, notebook_menu_2_2_2);

			//notebook_capture_analyze
//			UtilGtk.WidgetColor (notebook_capture_analyze, Config.ColorBackgroundShifted);
//			UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundShiftedIsDark, notebook_capture_analyze);
//			UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundShiftedIsDark, notebook_contacts_execute_or);
			UtilGtk.WidgetColor (notebook_capture_analyze, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundShiftedIsDark, notebook_capture_analyze);
			UtilGtk.WidgetColor (notebook_jumps_profile, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundShiftedIsDark, notebook_jumps_profile);
			UtilGtk.WidgetColor (notebook_stats, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundShiftedIsDark, notebook_stats);
			UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, label_gravitatory_vpf_propulsive);

			//notebook_force_sensor_rfd_options
			UtilGtk.WidgetColor (notebook_force_sensor_rfd_options, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundShiftedIsDark, notebook_force_sensor_rfd_options);
			UtilGtk.WidgetColor (notebook_ai_model_graph_table_triggers, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundShiftedIsDark, notebook_ai_model_graph_table_triggers);

			//encoder
			UtilGtk.WidgetColor (spinner_set_loading, Config.ColorBackgroundShifted);
			//all the labels inside the grid grid_encoder_analyze_instant have a white bg, so should call
			UtilGtk.ContrastLabelsGrid (false, grid_encoder_analyze_instant);

			//persons (main)
			//UtilGtk.WidgetColor (vbox_persons, Config.ColorBackgroundShifted);
			//UtilGtk.ContrastLabelsVBox (Config.ColorBackgroundShiftedIsDark, vbox_persons);
			//UtilGtk.WidgetColor (alignment44, Config.ColorBackgroundShifted);
			//UtilGtk.ContrastLabelsWidget (Config.ColorBackgroundShiftedIsDark, alignment44);

			//persons (main)
			UtilGtk.WidgetColor (hbox_frame_persons_top, Config.ColorBackgroundShifted);
			if (myTreeViewPersons.IsThereAnyRecord ())
				UtilGtk.WidgetColor (vbox_persons, Config.ColorBackgroundShifted);
			else
				vbox_persons.Name = "alertCss";

			UtilGtk.ContrastLabelsBox (Config.ColorBackgroundShiftedIsDark, hbox_frame_persons_top);

			//session databse
			UtilGtk.WidgetColor (vbox_frame_database_border, Config.ColorBackgroundShifted);
			UtilGtk.WidgetColor (hbox_frame_database_top, Config.ColorBackgroundShifted);

			//session
			UtilGtk.WidgetColor (vbox_frame_session_border, Config.ColorBackgroundShifted);

			//session (more)
			UtilGtk.WidgetColor (box_session_more, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsBox (Config.ColorBackgroundShiftedIsDark, box_session_more);

			//session (load_or_import)
			UtilGtk.WidgetColor (box_session_load_or_import, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsBox (Config.ColorBackgroundShiftedIsDark, box_session_load_or_import);

			//session (add/edit)
			UtilGtk.WidgetColor (app1sae_notebook_add_edit, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundShiftedIsDark, app1sae_notebook_add_edit);

			//session (delete)
			UtilGtk.WidgetColor (box_session_delete, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsBox (Config.ColorBackgroundShiftedIsDark, box_session_delete);

			//session (export)
			UtilGtk.WidgetColor (box_session_export, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsBox (Config.ColorBackgroundShiftedIsDark, box_session_export);

			//session (import)
			UtilGtk.WidgetColor (box_session_import, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsBox (Config.ColorBackgroundShiftedIsDark, box_session_import);
			UtilGtk.WidgetColor (box_session_import_current, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsBox (Config.ColorBackgroundShiftedIsDark, box_session_import_current);
			UtilGtk.WidgetColor (box_session_import_confirm, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsBox (Config.ColorBackgroundShiftedIsDark, box_session_import_confirm);

			//session (backup)
			UtilGtk.WidgetColor (box_session_backup, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsBox (Config.ColorBackgroundShiftedIsDark, box_session_backup);

			//session (data_folder)
			UtilGtk.WidgetColor (box_session_data_folder, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsBox (Config.ColorBackgroundShiftedIsDark, box_session_data_folder);

			//session (import_from_csv)
			UtilGtk.WidgetColor (box_session_import_from_csv, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsBox (Config.ColorBackgroundShiftedIsDark, box_session_import_from_csv);

			//help
			UtilGtk.WidgetColor (box_help, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsBox (Config.ColorBackgroundShiftedIsDark, box_help);

			//micro discover
			UtilGtk.WidgetColor (frame_micro_discover, Config.ColorBackgroundShifted);
			//UtilGtk.ContrastLabelsFrame (Config.ColorBackgroundShiftedIsDark, frame_micro_discover); //done on grid_micro_discover later on creation

			//news
			UtilGtk.WidgetColor (vbox_news2, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsBox (Config.ColorBackgroundShiftedIsDark, vbox_news2);
			UtilGtk.WidgetColor (frame_news_downloading, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsFrame (Config.ColorBackgroundShiftedIsDark, frame_news_downloading);

			//presentation
			UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, label_presentation_subtitle);

			//fulscreen
			UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, fullscreen_label_person);
			UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, fullscreen_label_exercise);
			UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, fullscreen_label_message);

			//other
			UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, label_remote_person);


			if(Config.ColorBackgroundIsDark)
				image_chronopic_connect_encoder2.Pixbuf =
					Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "image_chronopic_connect_yellow.png");
			else
				image_chronopic_connect_encoder2.Pixbuf =
					Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "image_chronopic_connect.png");
		}

		UtilGtk.WidgetColorGray180 (box_resultsSession_limit);

		LogB.Information(string.Format("UseSystemColor: {0}, ColorBackgroundIsDark: {1}", Config.UseSystemColor, Config.ColorBackgroundIsDark));
		if(! Config.UseSystemColor && Config.ColorBackgroundIsDark)
		{
			image_database_manage_blue.Visible = false;
			image_session_new_blue.Visible = false;
			image_session_load3_blue.Visible = false;
			image_session_more_window_blue.Visible = false;
			image_session_import1_blue.Visible = false;
			image_person_manage_blue.Visible = false;
			image_news_blue.Visible = false;
			image_prefs_util_help_blue.Visible = false;

			image_database_manage_yellow.Visible = true;
			image_session_new_yellow.Visible = true;
			image_session_load3_yellow.Visible = true;
			image_session_more_window_yellow.Visible = true;
			image_session_import1_yellow.Visible = true;
			image_person_manage_yellow.Visible = true;
			image_news_yellow.Visible = true;
			image_prefs_util_help_yellow.Visible = true;
		} else {
			image_database_manage_blue.Visible = true;
			image_session_new_blue.Visible = true;
			image_session_load3_blue.Visible = true;
			image_session_more_window_blue.Visible = true;
			image_session_import1_blue.Visible = true;
			image_person_manage_blue.Visible = true;
			image_news_blue.Visible = true;
			image_prefs_util_help_blue.Visible = true;

			image_database_manage_yellow.Visible = false;
			image_session_new_yellow.Visible = false;
			image_session_load3_yellow.Visible = false;
			image_session_more_window_yellow.Visible = false;
			image_session_import1_yellow.Visible = false;
			image_person_manage_yellow.Visible = false;
			image_news_yellow.Visible = false;
			image_prefs_util_help_yellow.Visible = false;
		}
		UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, label_news_frame);
	}

	private void testNewStuff()
	{
		/*
		//Useful to make crash the software (to check how log is working or any related change)
		List<string> crash_me = new List<string>();
		crash_me.Add ("1");
		LogB.Information (crash_me[55].ToString ());
		*/

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

		//UtilList.TestSortDoublesListstring();

		/*
		//Test ForceSensor GetVariabilityAndAccuracy: getVariabilityCVRMSSD
		VariabilityAndAccuracy vaa = new VariabilityAndAccuracy ();
		vaa.TestVariabilityCVRMSSD (1); //lag
		vaa.TestVariabilityCVRMSSD (2); //lag
		vaa.TestVariabilityCV ();
		*/

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

		//TestObjectsDifferences.Test ();

		if (configChronojump.Wilight)
			box_start_wilight.Visible = true;
		if (configChronojump.FourPlatforms)
			box_start_fourPlatforms.Visible = true;

        //plotSequenceWithoutSending ();

        /*
		 * Debug Wilight blacklist
		WilightTerminalLayout wilightTerminalLayout = new WilightTerminalLayout ();
		wilightTerminalLayout.ReadFile (configChronojump.WilightLayoutURL);
		//wilightCaptureManage = new WilightCaptureManage (wilightTerminalLayout, configChronojump.WilightCommandsURL, "", false);
		wilightCaptureManage = new WilightCaptureManage (wilightTerminalLayout, configChronojump.WilightCommandsURL, "4,11,12", false);
		//wilightCaptureManage = new WilightCaptureManage (wilightTerminalLayout, configChronojump.WilightCommandsURL, "39,41,42", false);
		while (true)
		{
			if (wilightCaptureManage.Finished)
				break;

			WilightCommand wilightCommand = wilightCaptureManage.GetNext ();
			if (wilightCommand.IsEmpty)
				continue;
		}
		on_quit2_activate ();
		*/

		//UtilList.ListRandomize1stAndThenSequentialTest ();
		//PointF.TestSortListXDescending ();

		/*
		   SqliteEncoder se = new SqliteEncoder ();
		   se.TestSelectSetsAndRepsLList (false, -1, 283, Constants.EncoderGI.GRAVITATORY, -1, -1);
		   */

		/*
		 * If comment this again, remember to uncomment the BluetoothLE.Stop at: on_quit2_activate ()
		 *
		//Subscribe to BluetoothLE data changed event
		BluetoothLE.OnDataChanged += BluetoothLE_OnDataChanged;
		//Start BluetoothLE service
		BluetoothLE.Start();
		*/

		/*
		encoderPTForceCaptureDo (
				//new EncoderPTForceCaptureTestsTextEncCountUp ("/dev/ttyACM4"), //eptfc (port), binaryBuffer
				new EncoderPTForceCapture ("/dev/ttyACM3"),
				false,  //binaryBuffer
				"", //"/tmp/forceEncoderPPS1.csv", // csvFile
				10); // seconds
		*/

		//MathUtil.QuartilesTest ();
		Boxplot.Test ();
    }

    /// <summary>
    /// Handles the event triggered when the Bluetooth LE data changes.
    /// </summary>
    /// <remarks>This method processes the updated data received from a Bluetooth LE device.  Use the <see cref="BluetoothLE.DataChangedEventArgs.Value"/> property of <paramref name="e"/>  to access the new data.</remarks>
    /// <param name="sender">The source of the event, typically the Bluetooth LE device.</param>
    /// <param name="e">The event data containing the updated value.</param>
    private void BluetoothLE_OnDataChanged(object sender, BluetoothLE.DataChangedEventArgs e)
    {
        LogB.Information($"[BluetoothLE] {e.Value}");
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
    private void loadPreferencesAtStartOrCloudViewChangeDB ()
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

	private void personChanged()
	{
		LogB.Information ("personChanged start ---->");
		sensitiveLastTestButtons(false);

		if (currentPerson == null)
			LogB.Information ("currentPerson == null" + (currentPerson == null).ToString ());
		else
			LogB.Information ("currentPerson: " + currentPerson.ToString ());

		label_current_person.Text = currentPerson.Name;
		button_person_merge.Sensitive = true;

		if (current_mode == Constants.Modes.JUMPSSIMPLE || current_mode == Constants.Modes.JUMPSREACTIVE ||
				current_mode == Constants.Modes.RUNSSIMPLE || current_mode == Constants.Modes.RUNSINTERVALLIC) {
			if (radio_contacts_export_individual_current_session.Active)
			{
				if(currentPerson != null)
					label_contacts_export_person.Text = currentPerson.Name;
				else
					label_contacts_export_person.Text = "";
			}

			label_contacts_export_result.Text = "";
			button_contacts_export_result_open.Visible = false;
		}

		pre_fillTreeView_resultsSession ();

		if(current_mode == Constants.Modes.JUMPSSIMPLE)
		{
			if(! configChronojump.Exhibition)
				updateGraphJumpsSimple();

			if(currentJumpType != null)
				update_label_extra_window_jumps_radiobutton_weight_percent_as_kg(
						(currentJumpType.HasWeight && extra_window_jumps_radiobutton_weight.Active));

			if(notebook_analyze.CurrentPage == Convert.ToInt32(notebook_analyze_pages.JUMPSPROFILE))
			{
				jumpsProfileCalculate ();
				drawingarea_jumps_profile.QueueDraw ();
			}
			else if(notebook_analyze.CurrentPage == Convert.ToInt32(notebook_analyze_pages.JUMPSDJOPTIMALFALL))
			{
				jumpsDjOptimalFallCalculate ();
				drawingarea_jumps_dj_optimal_fall.QueueDraw ();
			}
			else if(notebook_analyze.CurrentPage == Convert.ToInt32(notebook_analyze_pages.JUMPSWEIGHTFVPROFILE))
			{
				jumpsWeightFVProfileCalculate ();
				drawingarea_jumps_weight_fv_profile.QueueDraw ();
			}
			else if(notebook_analyze.CurrentPage == Convert.ToInt32(notebook_analyze_pages.JUMPSASYMMETRY))
			{
				jumpsAsymmetryCalculate ();
				drawingarea_jumps_asymmetry.QueueDraw ();
			}
			else if(notebook_analyze.CurrentPage == Convert.ToInt32(notebook_analyze_pages.JUMPSEVOLUTION))
			{
				jumpsEvolutionCalculate ();
				drawingarea_jumps_evolution.QueueDraw ();
			}

			//four platforms
			if (chronopicRegister != null)
			{
				ChronopicRegisterPort crp = chronopicRegister.GetSelectedForMode (current_mode);
				if (crp.Port != "" && crp.Type == ChronopicRegisterPort.Types.FOURPLATFORMS)
					updateFourPlatformsJumpsPersonNames ();
			}
		}
		else if(current_mode == Constants.Modes.JUMPSREACTIVE)
		{
			selectedJumpRj = null;

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
			{
				runsEvolutionCalculate (true);
				drawingarea_runs_evolution.QueueDraw ();
			}
		}
		else if(current_mode == Constants.Modes.RUNSINTERVALLIC)
		{
			selectedRunInterval = null;

			blankRunIntervalRealtimeCaptureGraph ();
			updateGraphRunsInterval();

			if(currentPerson != null)
				label_sprint_person_name.Text = string.Format(Catalog.GetString("Sprints of {0}"), currentPerson.Name);
			createTreeView_runs_interval_sprint (treeview_runs_interval_sprint);

			if(currentPerson != null)
				label_sprint_export_data.Text = currentPerson.Name;
			else
				label_sprint_export_data.Text = "";

			label_sprint_export_discarded.Text = "";
			label_sprint_export_result.Text = "";
			button_sprint_export_result_open.Visible = false;
		}
		else if(current_mode == Constants.Modes.BEEPTEST)
			beepTestPersonChanged ();
		else if(Constants.ModeIsENCODER (current_mode))
			encoderPersonChanged();
		else if(Constants.ModeIsFORCESENSOR (current_mode))
			forceSensorPersonChanged();
		else if(current_mode == Constants.Modes.RUNSENCODER)
			runEncoderPersonChanged();
		else if(current_mode == Constants.Modes.WILIGHT)
			updateGraphWilightBars();
		else if (current_mode == Constants.Modes.OTHER) //FOURPLATFORMS
			updateGraphFourPlatformsBars();

		finishPlayVideoIfRunning ();

		if (configChronojump.RemotePersonNextFile != "")
			remotePersonReadAndAssign ();

		LogB.Information ("<---- personChanged end");

		/*
		 * if personChange comes caused by a click on treeviewResults (or graph that changes treeviewResults)
		 * now that we have changed everything related to the person, select the row on treeviewResults
		 * and it will not come back here thanks to treeviewPersonsChangesTreeViewResultsSessionSignalsNoFollow
		 */
		if (personChangingFromResultsId >= 0)
		{
			selectResultsSessionId (personChangingFromResultsId, true);
			personChangingFromResultsId = -1;
		}
	}



	/* ---------------------------------------------------------
	 * ----------------  CREATE AND UPDATE COMBOS ---------------
	 *  --------------------------------------------------------
	 */
	
	// ---------------- combo_select ----------------------

	private bool comboSelectContactsTopNoFollow;
	private void createComboSelectContactsTop ()
	{
		LogB.Information ("createComboSelectContactsTop");
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
		else if(current_mode == Constants.Modes.RUNSENCODER || Constants.ModeIsFORCESENSOR (current_mode))
		{
			if(combo_select_contacts_top == null)
				combo_select_contacts_top = new ComboBoxText ();

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
				combo_select_contacts_top = new ComboBoxText ();

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

	private void contacts_exercise_left_button (Gtk.ComboBoxText combo, Gtk.Button button_left, Gtk.Button button_right)
	{
		combo = UtilGtk.ComboSelectPrevious(combo);

		button_left.Sensitive = (combo.Active > 0);
		button_right.Sensitive = true;

		if(current_mode == Constants.Modes.JUMPSSIMPLE || current_mode == Constants.Modes.JUMPSREACTIVE ||
				current_mode == Constants.Modes.RUNSSIMPLE || current_mode == Constants.Modes.RUNSINTERVALLIC ||
				current_mode == Constants.Modes.RUNSENCODER || Constants.ModeIsFORCESENSOR (current_mode))
		{
			button_combo_select_contacts_top_left.Sensitive = (combo.Active > 0);
			button_combo_select_contacts_top_right.Sensitive = true;
		}
	}
	private void contacts_exercise_right_button (Gtk.ComboBoxText combo, Gtk.Button button_left, Gtk.Button button_right)
	{
		bool isLast;
		combo = UtilGtk.ComboSelectNext(combo, out isLast);

		button_left.Sensitive = true;
		button_right.Sensitive = ! isLast;

		if(current_mode == Constants.Modes.JUMPSSIMPLE || current_mode == Constants.Modes.JUMPSREACTIVE ||
				current_mode == Constants.Modes.RUNSSIMPLE || current_mode == Constants.Modes.RUNSINTERVALLIC ||
				current_mode == Constants.Modes.RUNSENCODER || Constants.ModeIsFORCESENSOR (current_mode))
		{
			button_combo_select_contacts_top_left.Sensitive = true;
			button_combo_select_contacts_top_right.Sensitive = ! isLast;
		}
	}


	// -------------- combo select tests changed --------

	private void on_combo_select_contacts_top_changed (object o, EventArgs args)
	{
		if (comboSelectContactsTopNoFollow)
			return;

		LogB.Information("on_combo_select_contacts_top_changed");

		ComboBoxText combo = o as ComboBoxText;
		if(combo == null || UtilGtk.ComboGetActive(combo) == "")
		{
			LogB.Information(" ...but is null or empty.");
			//happens at end of import, at least on windows at innolab virtual machine
			return;
		}

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
		else if(Constants.ModeIsFORCESENSOR (current_mode))
			on_combo_force_sensor_exercise_changed(o, args);
	}

	private void on_combo_select_jumps_changed(object o, EventArgs args)
	{
		LogB.Information("on_combo_select_jumps_changed");
		ComboBoxText combo = o as ComboBoxText;
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
		pre_fillTreeView_resultsSession ();
	}
	
	private void on_combo_select_jumps_rj_changed(object o, EventArgs args)
	{
		LogB.Information("on_combo_select_jumps_rj_changed");
		ComboBoxText combo = o as ComboBoxText;
		if (o == null) {
			LogB.Information("o is null");
			return;
		}
		if(UtilGtk.ComboGetActive(combo) == "")
		{
			LogB.Information(" ...but is null or empty.");
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
		pre_fillTreeView_resultsSession ();
	}
	
	private void on_combo_select_runs_changed(object o, EventArgs args)
	{
		ComboBoxText combo = o as ComboBoxText;
		if (o == null)
			return;
		if(UtilGtk.ComboGetActive(combo) == "")
		{
			LogB.Information(" ...but is null or empty.");
			return;
		}

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
		pre_fillTreeView_resultsSession ();
	}
	
	private void on_combo_select_runs_interval_changed(object o, EventArgs args)
	{
		ComboBoxText combo = o as ComboBoxText;
		if (o == null)
			return;
		if(UtilGtk.ComboGetActive(combo) == "")
		{
			LogB.Information(" ...but is null or empty.");
			return;
		}

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
		pre_fillTreeView_resultsSession ();
	}
	
	//private int treeViewResultsSessionLastPersonID = -1; //never used
	private void pre_fillTreeView_resultsSession ()
	{
		if (pre_fillTreeView_resultsSession_NO)
			return;

		treeviewResultsSessionNoCheckPersonChange = true;

		if (treeViewResultsSession != null)
		{
			TreeViewEvent.LastPersonID = treeViewResultsSession.GetPersonIDOfSelectedRow;
			//LogB.Information ("TreeViewEvent.LastPersonID : " + TreeViewEvent.LastPersonID.ToString ());
		}

		treeview_results_session_storeReset ();

		if (current_mode == Constants.Modes.JUMPSSIMPLE)
		{
			if(radio_contacts_graph_allTests.Active)
				fillTreeView_jumps (Constants.AllJumpsNameStr(), false);
			else if (combo_select_jumps != null)
				fillTreeView_jumps (UtilGtk.ComboGetActive(combo_select_jumps), false);
		}
		else if (current_mode == Constants.Modes.JUMPSREACTIVE)
		{
			if(radio_contacts_graph_allTests.Active)
				fillTreeView_jumps_rj(Constants.AllJumpsNameStr(), false);
			else if (combo_select_jumps_rj != null)
				fillTreeView_jumps_rj(UtilGtk.ComboGetActive(combo_select_jumps_rj), false);
		}
		else if (current_mode == Constants.Modes.RUNSSIMPLE)
		{
			if(radio_contacts_graph_allTests.Active)
				fillTreeView_runs(Constants.AllRunsNameStr(), false);
			else if (combo_select_runs != null)
				fillTreeView_runs(UtilGtk.ComboGetActive(combo_select_runs), false);
		} else if (current_mode == Constants.Modes.RUNSINTERVALLIC)
		{
			if(radio_contacts_graph_allTests.Active)
				fillTreeView_runs_interval(Constants.AllRunsNameStr(), false);
			else if (combo_select_runs_interval != null)
				fillTreeView_runs_interval(UtilGtk.ComboGetActive(combo_select_runs_interval), false);
		} else if (current_mode == Constants.Modes.RUNSENCODER)
		{
			if(radio_contacts_graph_allTests.Active)
				fillTreeView_runEncoder (Constants.AllTestsNameStr(), false);
			else if (combo_run_encoder_exercise != null)
				fillTreeView_runEncoder (UtilGtk.ComboGetActive (combo_run_encoder_exercise), false);
		} else if (current_mode == Constants.Modes.BEEPTEST)
		{
			if (radio_contacts_graph_allTests.Active)
				fillTreeView_beepTest (Constants.AllTestsNameStr (), false);
			else if (combo_beepTest_type != null)
				fillTreeView_beepTest (UtilGtk.ComboGetActive (combo_beepTest_type), false);
		} else if (current_mode == Constants.Modes.WILIGHT)
		{
			fillTreeView_wilight ("", false);
		} else if (Constants.ModeIsFORCESENSOR (current_mode))
		{
			if (radio_contacts_graph_allTests.Active)
				fillTreeView_forceSensor (Constants.AllTestsNameStr (), false);
			else if (combo_force_sensor_exercise != null)
				fillTreeView_forceSensor (UtilGtk.ComboGetActive(combo_force_sensor_exercise), false);
		} else if (Constants.ModeIsENCODER (current_mode))
		{
			if (radio_contacts_graph_allTests.Active)
				fillTreeView_encoder (Constants.AllTestsNameStr (), false);
			else if (combo_encoder_exercise_capture != null)
				fillTreeView_encoder (UtilGtk.ComboGetActive (combo_encoder_exercise_capture), false);
		} else if (current_mode == Constants.Modes.OTHER)
		{
			fillTreeView_fourPlatforms ("", false);
		} else
			return;

		if (currentPerson != null)
		{
			treeViewResultsSession.CurrentPersonID = currentPerson.UniqueID; //to show in bold
			treeViewResultsSession.SelectPerson (currentPerson.Name);
		}

		treeviewResultsSessionNoCheckPersonChange = false;
	}

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


	private void on_quit1_activate (object o, EventArgs args)
	{
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
		else if (configChronojump.CopyToCloudOnExit && configChronojump.CopyToCloudFullPath != "")
		{
			//to not allow double click on copyToCloud and exit
			//box_prefs_help_news_exit.Sensitive = false; not because it does not show the progressbars if unsensitive
			app1s_button_copyToCloud.Sensitive = false;
			button_menu_exit.Sensitive = false;
			vbox_menu_tiny_menu.Sensitive = false;

			if (! exitChronojumpAfterCopyToCloudStarted) //to not doing n times on double click delete event
				on_copyToCloud_when_exit ();
		} else
			on_quit2_activate ();
	}

	private void on_button_exit_cancel_clicked (object o, EventArgs args)
	{
		notebook_start.CurrentPage = Convert.ToInt32(notebook_start_pages.PROGRAM);
	}
	private void on_button_exit_confirm_clicked (object o, EventArgs args)
	{
		on_quit2_activate ();
	}

	private void on_quit2_activate ()
	{
		LogB.Information("Bye!");

        //Stop the BluetoothLE service if it was started
        //BluetoothLE.Stop();

        updatingRestTimes = false;

		//close contacts capture
		if(currentEventExecute != null && currentEventExecute.IsThreadRunning())
		{
			LogB.Information("Closing contacts capture thread...");

			//this will mark the test as cancelled
			currentEventExecute.Cancel = true;

			//this will actually cancel Read_cambio and then Read_event in order to really cancel
			Chronopic.CancelDo();
			System.Threading.Thread.Sleep (500); //need to wait a bit to allow other thread to be closed

			if (currentEventExecute.IsThreadRunning())
			{
				LogB.Information("currentEventExecute still running, waiting 1s more");
				System.Threading.Thread.Sleep (1000);
			}
			LogB.Information ("currentEventExecute still running? " + currentEventExecute.IsThreadRunning().ToString ());

			LogB.Information("Closing camera if opened...");
			ExecuteProcess.KillExternalProcess (WebcamFfmpeg.GetExecutableCapture(operatingSystem));
			LogB.Information("Done!");
		}

		finishPlayVideoIfRunning ();

		if(threadRFID != null && threadRFID.IsAlive)
		{
			LogB.Information("Closing threadRFID");

			rfid.Stop();
			rfidProcessCancel = true;
			System.Threading.Thread.Sleep(500);

			if(threadRFID != null && threadRFID.IsAlive)
			{
				//threadRFID.Abort(); // obsolete on dotnet
				LogB.Information("threadRFID still running");

				System.Threading.Thread.Sleep (1000); //need to wait a bit to allow other thread to be closed
				LogB.Information ("threadRFID still running? " + threadRFID.IsAlive.ToString ());
			}
		}

		if (threadRemoteTest != null && threadRemoteTest.IsAlive)
		{
			LogB.Information("Closing threadRemoteTest");
			remoteTest.Stop ();
		}

		if (threadBeepTest != null && threadBeepTest.IsAlive && beepTestCM != null)
			beepTestCM.Finish ();

		if(threadImport != null && threadImport.IsAlive)
		{
			LogB.Information("Closing threadImport");
			//threadImport.Abort(); //obsolete on dotnet

			threadImportCancel = true;
			System.Threading.Thread.Sleep(500);

			if(threadImport != null && threadImport.IsAlive)
			{
				System.Threading.Thread.Sleep (1000); //need to wait a bit to allow other thread to be closed
				LogB.Information ("threadImport still running? " + threadImport.IsAlive.ToString ());
			}
		}

		if(app1s_threadBackup != null && app1s_threadBackup.IsAlive)
		{
			LogB.Information("Closing app1s_threadBackup");
			//app1s_threadBackup.Abort(); //obsolete on dotnet

			//threadBackupCancel = true;
			if (app1s_uc != null)
				app1s_uc.Cancel = true; //this will exit the recursively copy

			System.Threading.Thread.Sleep(500);

			if(app1s_threadBackup != null && app1s_threadBackup.IsAlive)
			{
				System.Threading.Thread.Sleep (1000); //need to wait a bit to allow other thread to be closed
				LogB.Information ("app1s_threadBackup still running? " + app1s_threadBackup.IsAlive.ToString ());
			}
		}

		if(app1s_threadExport != null && app1s_threadExport.IsAlive)
		{
			LogB.Information("Closing app1s_threadExport");
			//app1s_threadExport.Abort(); //obsolete on dotnet

			cancelExport = true;
			System.Threading.Thread.Sleep(500);

			if(app1s_threadExport != null && app1s_threadExport.IsAlive)
			{
				System.Threading.Thread.Sleep (1000); //need to wait a bit to allow other thread to be closed
				LogB.Information ("app1s_threadExport still running? " + app1s_threadExport.IsAlive.ToString ());
			}
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

		//close discoverThread
		if (discoverWin != null)
			discoverWin.CancelCloseFromUser ();

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
		{
			//forceOtherThread.Abort(); //obsolete on dotnet
			forceProcessCancel = true;
		}
		if(portFSOpened)
			portFS.Close();
		if(wichroCapture != null && wichroCapture.PortOpened) //seems it works also for wilight
			wichroCapture.Disconnect();

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

		if (currentPerson != null && currentPerson.UniqueID > 0)
			SqlitePreferences.Update (SqlitePreferences.LastPersonID, currentPerson.UniqueID.ToString (), false);

		//if user maximizes (not using preferences window), the sqlite variable gets not updated, update here on exit
		SqlitePreferences.Update ("maximized", preferences.maximized.ToString(), false);

		LogB.Information("Bye3!");

		//TODO: if camera is opened close it! Note that this is intended to kill a remaining ffmpeg process
		//but maybe we will kill any ffmpeg instance open by any other possible program on the computer
		//maybe better kill ffmpeg before opening other instance
		//and at end check if it is running that process and kill the last one ffmpeg instance
		//LogB.Information("Bye4!");

		if (configChronojump.ReadFromCloudMainPath != "" && storedDBFilename != "")
			Util.FileCopySafe (
					Path.Combine(Util.GetCloudReadTempDir (), "database", "chronojump.db"),
					Path.Combine(storedDBFilename, "database", "chronojump.db"),
					true); //overwrite

		Log.End();

		Application.Quit();
		
		//Environment.Exit(Environment.ExitCode);
	}
	
	/* ---------------------------------------------------------
	 * ----------------  SESSION NEW, LOAD, EXPORT, DELETE -----
	 *  --------------------------------------------------------
	 */

	private void on_new_session_activate (object o, EventArgs args)
	{
		LogB.Information("new session");
		sessionAddEditShow (App1saeModes.ADDSESSION);
	}

	private void setApp1Title(string sessionName, Constants.Modes mode)
	{
		string title = progName;

		if(sessionName != "") {
			title += " - " + sessionName;
			label_current_session.Text = "<b>" + sessionName + "</b>";
			label_current_session.UseMarkup = true;
			label_current_session.TooltipText = sessionName;
			frame_session.Name = "";
		} else {
			label_current_session.Text = "----";
			label_current_session.TooltipText = "";
			frame_session.Name = "alertCss";
		}

		if(mode != Constants.Modes.UNDEFINED)
		{
			string modeStr = Constants.ModePrint (mode);
			if(modeStr != "")
				title += " - " + modeStr;
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
		person_search.Text = "";

		if(createdStatsWin) {
			stats_win_initializeSession();
		}

		treeviewResultsSessionAllPersonsAllTests ();
		resetAllTreeViews(false, true, false); //fillTests, resetPersons, fillPersons

		//if we are on analyze tab, switch to capture tab
		radio_mode_contacts_capture.Active = true;

		//show hidden widgets
		sensitiveGuiNoSession();
		sensitiveGuiYesSession();

		sensitiveGuiNoPerson();
		currentPerson = null;

		//for sure, jumpsExists is false, because we create a new session

		hbox_persons_bottom_photo.Sensitive = false;
		label_current_person.Text = "";
		label_top_person_name.Text = "";
		button_person_merge.Sensitive = false;

		//update report
		report.SessionID = currentSession.UniqueID;
		report.StatisticsRemove();
		try {
			reportWin.FillTreeView();
		} catch {} //reportWin is still not created, not need to Fill again

		label_ai_export_person.Text = "";
		label_contacts_export_person.Text = "";
		label_ai_export_session.Text = currentSession.Name;
		label_contacts_export_session.Text = currentSession.Name;
		label_contacts_export_result.Text = "";
		button_contacts_export_result_open.Visible = false;

		if (configChronojump.RemotePersonNextFile != "")
			remotePersonReadAndAssign ();

		//feedback (more in 1st session created)
		string feedbackLoadUsers = Catalog.GetString ("Session created, now add or load persons.");
		new DialogMessage(Constants.MessageTypes.INFO, feedbackLoadUsers);

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

	private void on_open_session_activate (object o, EventArgs args)
	{
		LogB.Information("open session");
		sessionLoadWindowShow(app1s_windowType.LOAD_SESSION);
	}

	//called from open session OR from gui/networks configInit when config.SessionMode == Config.SessionModeEnum.UNIQUE
	private void on_load_session_accepted () 
	{
		setApp1Title(currentSession.Name, current_mode);
		person_search.Text = "";
	
		if(createdStatsWin && ! configChronojump.Exhibition) //slow Sqlite calls for Exhibition big data
			stats_win_initializeSession();

		treeviewResultsSessionAllPersonsAllTests ();
		resetAllTreeViews(! configChronojump.Exhibition, true, true); //fillTests, resetPersons, fillPersons

		//if we are on analyze tab, switch to capture tab
		radio_mode_contacts_capture.Active = true;

		bool foundPersons = false;

		//on Compujump don't start with first person, wait to it's rfid
		if( ! configChronojump.Compujump)
			foundPersons = selectRowTreeView_persons(treeview_persons, 0);

		//show hidden widgets, and sensitivize
		menus_and_mode_sensitive(true);
		sensitiveGuiNoSession();
		sensitiveGuiYesSession();

		hbox_persons_bottom_photo.Sensitive = false;
		LogB.Information("foundPersons: " + foundPersons.ToString());
		//if there are persons
		if(foundPersons) {
			//activate the gui for persons in main window
			sensitiveGuiYesPerson();
			label_top_person_name.Text = "<b>" + currentPerson.Name + "</b>";
			label_top_person_name.UseMarkup = true;
			//label_top_encoder_person_name.Text = "<b>" + currentPerson.Name + "</b>";
			//label_top_encoder_person_name.UseMarkup = true;
		} else {
			currentPerson = null;
			sensitiveGuiNoPerson ();
		}
		button_person_merge.Sensitive = foundPersons;

		//update report
		report.SessionID = currentSession.UniqueID;
		report.StatisticsRemove();

		if(reportWin != null)
			reportWin.FillTreeView();

		//update other widgets
		//analyze export labels:
		label_sprint_export_data.Text = currentSession.Name;

		if(currentPerson != null)
		{
			label_ai_export_person.Text = currentPerson.Name;
			label_contacts_export_person.Text = currentPerson.Name;
		} else {
			label_ai_export_person.Text = "";
			label_contacts_export_person.Text = "";
		}
		label_ai_export_session.Text = currentSession.Name;
		label_contacts_export_session.Text = currentSession.Name;
		label_contacts_export_result.Text = "";
		button_contacts_export_result_open.Visible = false;

		if (configChronojump.RemotePersonNextFile != "")
			remotePersonReadAndAssign ();

		chronojumpWindowTestsNext();

		SqlitePreferences.Update(SqlitePreferences.LastSessionID, currentSession.UniqueID.ToString(), false);
	}
	
	private void closeSession()
	{
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

	/* TODO: remove this when we are sure that treeViewResultsEncoder works flawlessly
       private OverviewWindow overviewWin;
       private void on_session_overview_clicked (object o, EventArgs args)
       {
               if (currentSession == null || currentPerson == null)
                       return;

               Constants.Modes m = current_mode;

               if(Constants.ModeIsENCODER (m))
                       overviewWin = EncoderOverviewWindow.Show (app1, currentEncoderGI, currentSession.UniqueID, currentPerson.UniqueID);

               overviewWin.Button_select_this_person.Clicked -= new EventHandler(on_overview_select_person);
               overviewWin.Button_select_this_person.Clicked += new EventHandler(on_overview_select_person);
       }

       private void on_overview_select_person (object o, EventArgs args)
       {
               if(overviewWin.SelectedPersonID != -1)
               {
                       person_search.Text = "";
                       //LogB.Information("selected: " + overviewWin.SelectedPersonID.ToString());
                       selectRowTreeView_persons(treeview_persons,
                                       myTreeViewPersons.FindRow(overviewWin.SelectedPersonID));

                       overviewWin.HideAndNull();
               }
       }
       */

	private void on_radio_contacts_export_individual_current_session_toggled (object o, EventArgs args)
	{
		if (currentPerson != null)
			label_contacts_export_person.Text = currentPerson.Name;
		else
			label_contacts_export_person.Text = "";

		if (currentSession != null)
			label_contacts_export_session.Text = currentSession.Name;
		else
			label_contacts_export_session.Text = "";

		check_contacts_export_jumps_simple_mean_max_tables.Visible = check_contacts_export_jumps_simple.Active;
		label_contacts_export_result.Text = "";
		button_contacts_export_result_open.Visible = false;
	}
	private void on_radio_contacts_export_individual_all_sessions_toggled (object o, EventArgs args)
	{
		if(currentPerson != null)
			label_contacts_export_person.Text = currentPerson.Name;
		else
			label_contacts_export_person.Text = "";

		label_contacts_export_session.Text = Catalog.GetString ("All");

		check_contacts_export_jumps_simple_mean_max_tables.Visible = false;
		label_contacts_export_result.Text = "";
		button_contacts_export_result_open.Visible = false;
	}
	private void on_radio_contacts_export_groupal_current_session_toggled (object o, EventArgs args)
	{
		label_contacts_export_person.Text = Catalog.GetString ("All");
		if (currentSession != null)
			label_contacts_export_session.Text = currentSession.Name;
		else
			label_contacts_export_session.Text = "";

		check_contacts_export_jumps_simple_mean_max_tables.Visible = check_contacts_export_jumps_simple.Active;
		label_contacts_export_result.Text = "";
		button_contacts_export_result_open.Visible = false;
	}

	private void on_check_contacts_export_jumps_simple_toggled (object o, EventArgs args)
	{
		check_contacts_export_jumps_simple_mean_max_tables.Visible =
			(! radio_contacts_export_individual_all_sessions.Active &&
			 check_contacts_export_jumps_simple.Active);
	}

	ExportSessionCSV contactsExportCSV;
	private void on_button_contacts_export_clicked (object o, EventArgs args)
	{
		button_contacts_export_result_open.Visible = false;
		if(currentSession == null || currentSession.UniqueID == -1) {
			new DialogMessage(Constants.MessageTypes.WARNING, "Cannot export a missing session");
			return;
		}

		int personID = -1;
		string personName = "";
		if (currentPerson != null)
		{
			personID = currentPerson.UniqueID;
			personName = currentPerson.Name;
		}
		int sessionID = currentSession.UniqueID;
		bool jumpsSimpleMeanMaxTables = check_contacts_export_jumps_simple_mean_max_tables.Active;

		/*if (radio_contacts_export_individual_current_session.Active)
		{
			personID = personID;
			personName = personName;
			sessionID = currentSession.UniqueID;
			jumpsSimpleMeanMaxTables = check_contacts_export_jumps_simple_mean_max_tables.Active;
		} else*/ if (radio_contacts_export_individual_all_sessions.Active)
		{
			//personID = personID;
			//personName = personName;
			sessionID = -1;
			jumpsSimpleMeanMaxTables = false;
		} else if (radio_contacts_export_groupal_current_session.Active)
		{
			personID = -1;
			personName = "";
			sessionID = currentSession.UniqueID;
			jumpsSimpleMeanMaxTables = check_contacts_export_jumps_simple_mean_max_tables.Active;
		}

		contactsExportCSV = new ExportSessionCSV ();
		if (current_mode == Constants.Modes.JUMPSSIMPLE || current_mode == Constants.Modes.JUMPSREACTIVE)
		{
			contactsExportCSV = new ExportSessionCSV (currentSession, app1, preferences,
					"jumps", personID, personName, sessionID,
					check_contacts_export_jumps_simple.Active,
					jumpsSimpleMeanMaxTables,
					check_contacts_export_jumps_reactive.Active,
					false, false,
					false);
		} else if (current_mode == Constants.Modes.RUNSSIMPLE || current_mode == Constants.Modes.RUNSINTERVALLIC)
		{
			contactsExportCSV = new ExportSessionCSV (currentSession, app1, preferences,
					"races", personID, personName, sessionID,
					false, false, false,
					check_contacts_export_runs_simple.Active,
					check_contacts_export_runs_intervallic.Active,
					false);
		} else if (current_mode == Constants.Modes.OTHER)
		{
			contactsExportCSV = new ExportSessionCSV (currentSession, app1, preferences,
					"fourPlatforms", personID, personName, sessionID,
					false, false, false,
					false, false,
					true);
		}

		contactsExportCSV.FakeButtonDone.Clicked -= new EventHandler (on_button_contacts_export_done);
		contactsExportCSV.FakeButtonDone.Clicked += new EventHandler (on_button_contacts_export_done);

		contactsExportCSV.Do ();
	}

	private void on_button_contacts_export_done (object o, EventArgs args)
	{
		bool success = (contactsExportCSV.DoneEnum == ExportSession.DoneEnumType.SUCCESS && contactsExportCSV.Filename != "");

		if (success)
		{
			label_contacts_export_result.Text = string.Format (Catalog.GetString ("Saved to {0}"), contactsExportCSV.Filename) +
				Constants.GetSpreadsheetString (preferences.CSVExportDecimalSeparator);
			label_contacts_export_result.UseMarkup = true;
			button_contacts_export_result_open.Visible = true;
		} else {
			if (contactsExportCSV.DoneEnum == ExportSession.DoneEnumType.CANCEL) 
				label_contacts_export_result.Text = Catalog.GetString ("Cancelled.");
			else if (contactsExportCSV.DoneEnum == ExportSession.DoneEnumType.NODATA) 
				label_contacts_export_result.Text = Catalog.GetString ("Not enough data.");
			else if (contactsExportCSV.DoneEnum == ExportSession.DoneEnumType.CANNOTCOPY) 
				label_contacts_export_result.Text = string.Format (Catalog.GetString ("Cannot export to file {0} "), contactsExportCSV.Filename);

			button_contacts_export_result_open.Visible = false;
		}
	}

	private void on_button_contacts_export_result_open_clicked (object o, EventArgs args)
	{
		if(! Util.OpenURL (contactsExportCSV.Filename))
			new DialogMessage (Constants.MessageTypes.WARNING,
					Constants.DirectoryCannotOpenStr() + "\n\n" + contactsExportCSV.Filename);
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
		Constants.Modes m = current_mode;
		if(notebook_sup.CurrentPage == Convert.ToInt32(notebook_sup_pages.START))
			m = Constants.Modes.UNDEFINED;

		preferencesWin = PreferencesWindow.Show(preferences, m, configChronojump.Compujump,
				//configChronojump, progVersion);
				progVersion);

		preferencesWin.FakeButtonMaximizeChanges.Clicked -= new EventHandler (on_preferences_maximize_changes);
		preferencesWin.FakeButtonMaximizeChanges.Clicked += new EventHandler (on_preferences_maximize_changes);
		preferencesWin.FakeButtonPersonWin.Clicked -= new EventHandler (on_preferences_personWin_changes);
		preferencesWin.FakeButtonPersonWin.Clicked += new EventHandler (on_preferences_personWin_changes);
		preferencesWin.FakeButtonConfigurationImported.Clicked += new EventHandler(on_preferences_import_configuration);
		preferencesWin.FakeButtonConfigurationImported.Clicked += new EventHandler(on_preferences_import_configuration);
		preferencesWin.FakeButtonDebugModeStart.Clicked += new EventHandler(on_preferences_debug_mode_start);
		preferencesWin.FakeButtonDeleteDevices.Clicked += new EventHandler(on_preferences_delete_devices);
		preferencesWin.FakeButtonColorsChanged.Clicked += new EventHandler(on_preferences_colors_changed);
		preferencesWin.Button_close.Clicked += new EventHandler(on_preferences_closed);
	}

	private void on_preferences_maximize_changes (object o, EventArgs args)
	{
		preferences = preferencesWin.GetPreferences;
		//LogB.Information ("preferences.maximized = " + preferences.maximized);
		maximizeOrNot (false); //fromPreferences

		//TODO: undecorated is not working if Chronojump started undecorated
	}

	//show at top/left, if show at left show photo or not
	private void on_preferences_personWin_changes (object o, EventArgs args)
	{
		preferences = preferencesWin.GetPreferences;
		configInitFromPreferences();
		initialize_menu_or_menu_tiny();
	}

	private void on_preferences_import_configuration (object o, EventArgs args)
	{
		/*
		preferencesWin.FakeButtonConfigurationImported.Clicked -= new EventHandler(on_preferences_import_configuration);
		
		configInit();
		LogB.Information("Initialized configuration");
		*/
	}

	private void on_preferences_colors_changed (object o, EventArgs args)
	{
		UtilGtk.ApplyCSS (preferences.fontSizeAtGui);
	}

	private void on_preferences_closed (object o, EventArgs args)
	{
		// 1) changes on cloud
		// 1.a) if Read changed path from "" to != "" or viceversa need to restart
		// 1.b) if Copy changed path from "" to != "" or viceversa need to restart
		Config configHere = new Config();
		configHere.Read ();
		if (
				(configChronojump.ReadFromCloudMainPath == "") != (configHere.ReadFromCloudMainPath == "") || // 1.a
				(configChronojump.CopyToCloudFullPath == "") != (configHere.CopyToCloudFullPath == "") // 1.b
				)
		{
			//gui force restart
			notebook_start.CurrentPage = Convert.ToInt32(notebook_start_pages.EXITCONFIRM);
			UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundIsDark, label_exit_confirm);
			label_exit_confirm.Text = Catalog.GetString ("Changes on cloud options force to restart Chronojump to apply changes");
			button_exit_cancel.Visible = false;
			return;
		}

		// 1.c) if read path changed, use new path
		if (configHere.ReadFromCloudMainPath != "" && configHere.ReadFromCloudMainPath != configChronojump.ReadFromCloudMainPath)
			configChronojump.ReadFromCloudMainPath = configHere.ReadFromCloudMainPath;

		// 1.d) if copy path changed, use new path
		if (configHere.CopyToCloudFullPath != "" && configHere.CopyToCloudFullPath != configChronojump.CopyToCloudFullPath)
			configChronojump.CopyToCloudFullPath = configHere.CopyToCloudFullPath;

		preferences = preferencesWin.GetPreferences;
		LogB.Mute = preferences.muteLogs;

		if(checkbutton_video_contacts.Active) {
			videoCapturePrepare(false); //if error, show message
		}

		if(configChronojump.Compujump)
		{
			viewport_chronopics.Sensitive = preferences.networksAllowChangeDevices;
			button_encoder_devices_networks.Sensitive = preferences.networksAllowChangeDevices;
			button_contacts_devices_networks_problems.Sensitive = preferences.networksAllowChangeDevices;
		}

		//change language works on windows. On Linux let's change the locale
		//if(UtilAll.IsWindows()) 
		//	languageChange();

		configInitFromPreferences();

		if(feedbackWin != null)
		{
			feedbackWin.VolumeOn = preferences.volumeOn;
			feedbackWin.Gstreamer = preferences.gstreamer;
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
			
			
			createTreeView_resultsSession (treeview_results_session);
			/*
			createTreeView_jumps (treeview_jumps);
			createTreeView_jumps_rj (treeview_jumps_rj);
			createTreeView_runs (treeview_runs);
			createTreeView_runs_interval (treeview_runs_interval);
			createTreeView_wilight (treeview_wilight);
			*/
			createTreeView_runs_interval_sprint (treeview_runs_interval_sprint);

			pre_fillTreeView_resultsSession ();

			if(current_mode == Constants.Modes.POWERGRAVITATORY){
				label_gravitatory_vpf_propulsive.Visible = preferences.encoderPropulsive;
			}
		}
		catch {
			LogB.Information("catched at on_preferences_accepted ()");
		}

		/*
		LogB.Information ("Config.ColorBackground RGB");
		LogB.Information (Config.ColorBackground.Red.ToString());
		LogB.Information (Config.ColorBackground.Green.ToString());
		LogB.Information (Config.ColorBackground.Blue.ToString());
		*/

		//repaint labels that are on the background
		//TODO: only if color changed or personWinHide
		Config.UseSystemColor = preferences.colorBackgroundOsColor;
		doLabelsContrast(configChronojump.PersonWinHide);
		vbox_persons_bottom.Visible = preferences.personPhoto && ! check_menu_session.Active;

		UtilGtk.ApplyCSS (preferences.fontSizeAtGui);


		if(myTreeViewPersons != null)
		{
			treeview_persons_storeReset();
			myTreeViewPersons.RestSecondsMark = get_configured_rest_time_in_seconds();
			fillTreeView_persons();
		}

		//not done here because done on click the checkboxes at preferences win
		//initialize_menu_or_menu_tiny();

		// ---------- force sensor changes -------------->

		//TODO: only if have changed
		setForceSensorAnalyzeABSliderIncrements();
		setForceSensorAnalyzeMaxAVGInWindow();

		// update force_capture_drawingarea
		if (Constants.ModeIsFORCESENSOR (current_mode))// && radiobutton_force_sensor_analyze_manual.Active)
		{
			if (Util.FileExists(lastForceSensorFullPath) && currentForceSensor != null)
				force_sensor_recalculate (currentForceSensor.CaptureOption, currentForceSensor.Laterality);

			forceSensorPrepareGraphAI ();
		}

		// <---------- end of force sensor changes --------------
	}


	/*
	 * menu test selectors
	 */

	private void show_start_page()
	{
		notebook_sup.CurrentPage = Convert.ToInt32(notebook_sup_pages.START);

		//a double click cannot be managed now because start window is clicked from the "Mode" button at any mode

		if (current_mode == Constants.Modes.UNDEFINED ||
				current_mode == Constants.Modes.JUMPSSIMPLE ||
				current_mode == Constants.Modes.JUMPSREACTIVE)
		{
			radio_menu_2_2_2_jumps.Active = true;
			button_menu_2_2_2_manage (radio_menu_2_2_2_jumps, false); //cannot be a double click
		}
		else if (current_mode == Constants.Modes.RUNSSIMPLE ||
				current_mode == Constants.Modes.RUNSINTERVALLIC ||
				current_mode == Constants.Modes.RUNSENCODER ||
				current_mode == Constants.Modes.BEEPTEST)
		{
			radio_menu_2_2_2_races.Active = true;
			button_menu_2_2_2_manage (radio_menu_2_2_2_races, false);
		}
		else if (current_mode == Constants.Modes.WILIGHT)
		{
			radio_menu_2_2_2_wilight.Active = true;
			button_menu_2_2_2_manage (radio_menu_2_2_2_wilight, false);
		}
		else if (current_mode == Constants.Modes.OTHER)
		{
			radio_menu_2_2_2_fourPlatforms.Active = true;
			button_menu_2_2_2_manage (radio_menu_2_2_2_fourPlatforms, false);
		}
		else if (current_mode == Constants.Modes.FORCESENSORISOMETRIC || current_mode == Constants.Modes.FORCESENSORELASTIC)
		{
			radio_menu_2_2_2_force.Active = true;
			button_menu_2_2_2_manage (radio_menu_2_2_2_force, false);
		}
		else if (current_mode == Constants.Modes.POWERGRAVITATORY)
		{
			radio_menu_2_2_2_weights.Active = true;
			button_menu_2_2_2_manage (radio_menu_2_2_2_weights, false);
		}
		else if (current_mode == Constants.Modes.POWERINERTIAL)
		{
			radio_menu_2_2_2_inertial.Active = true;
			button_menu_2_2_2_manage (radio_menu_2_2_2_inertial, false);
		}

		//myTreeViewPersons.UpdateTestsNBlank ();
		treeview_persons_storeReset_start_page (); //send UNDEFINED
		fillTreeView_persons();

		//show title
		string tempSessionName = "";
		if(currentSession != null)
			tempSessionName = currentSession.Name;

		setApp1Title(tempSessionName, Constants.Modes.UNDEFINED);
		new ChronojumpLogo (notebook_chronojump_logo, drawingarea_chronojump_logo, false);//preferences.logoAnimatedShow);
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
		else if (m == Constants.Modes.BEEPTEST)
		{
			if(radio_change_modes_contacts_runs_beepTest.Active)
				changeMode (Constants.Modes.BEEPTEST);
			else
				radio_change_modes_contacts_runs_beepTest.Active = true;
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
		else if (m == Constants.Modes.FORCESENSORISOMETRIC)
		{
			if(radio_change_modes_contacts_isometric.Active)
				changeMode (Constants.Modes.FORCESENSORISOMETRIC);
			else
				radio_change_modes_contacts_isometric.Active = true;
		}
		else if (m == Constants.Modes.FORCESENSORELASTIC)
		{
			if(radio_change_modes_contacts_elastic.Active)
				changeMode (Constants.Modes.FORCESENSORELASTIC);
			else
				radio_change_modes_contacts_elastic.Active = true;
		}
		else //for modes that do not have radios like RT, other
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

		setApp1Title(tempSessionName, m);

		treeview_results_session_storeReset ();

		//maybe we have the force sensor port opened or runEncoder port opened, close it:
		if(portFSOpened)
			forceSensorDisconnect();
		if(portREOpened)
			runEncoderDisconnect();

		if(wichroCapture != null && wichroCapture.PortOpened)
			wichroCapture.Disconnect();

		//run simple will be the only one with its drawing area
		button_inspect_last_test_run_simple.Visible = false;

		sensitiveLastTestButtons(false);

		//contacts delete test buttons: edit, delete (visible)
		button_contacts_edit_selected.Visible = true;
		button_contacts_delete_selected.Visible = true;

		//contacts test buttons: edit, delete (sensitive)
		button_contacts_edit_selected.Sensitive = false;
		button_contacts_delete_selected.Sensitive = false;

		button_contacts_repair_selected.Visible = (m == Constants.Modes.JUMPSREACTIVE || m == Constants.Modes.RUNSINTERVALLIC);
		button_contacts_repair_selected.Sensitive = false;

		//show capture graph and/or table
		alignment_contacts_show_graph_table.Visible = true;
		if (Constants.ModeIsENCODER (m))
		{
			vbox_angle_now.Visible = m == Constants.Modes.POWERINERTIAL;
			on_check_encoder_capture_show_modes_clicked (new object(), new EventArgs());
		} else
			on_check_contacts_capture_show_modes_clicked (new object(), new EventArgs());

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

		notebook_sup.CurrentPage = Convert.ToInt32(notebook_sup_pages.TESTS);
		//top buttons on capture
		if (m == Constants.Modes.OTHER || m == Constants.Modes.BEEPTEST || m == Constants.Modes.WILIGHT)
		{
			box_contacts_capture_top.Visible = false;
			hbox_encoder_capture_top.Visible = false;
		}
		else if (Constants.ModeIsENCODER (m))
		{
			box_contacts_capture_top.Visible = false;
			hbox_encoder_capture_top.Visible = true;
		} else {
			box_contacts_capture_top.Visible = true;
			hbox_encoder_capture_top.Visible = false;
		}

		hbox_change_modes_jumps.Visible = false;
		hbox_change_modes_runs.Visible = false;
		hbox_change_modes_force_sensor.Visible = false;
		hbox_change_modes_encoder.Visible = false;
		radio_mode_contacts_analyze.Visible = true;
		radio_change_modes_contacts_wilight.Visible = false;
		radio_change_modes_contacts_fourPlatforms.Visible = false;

		button_contacts_bells.Sensitive = false;

		radio_mode_contacts_capture.Active = true; //it is safe to change to capture, because analyze has different graphs depending on mode

		button_contacts_capture_save_image.Visible = false;
		radio_mode_contacts_jumps_profile.Active = true;
		hbox_radio_mode_contacts_analyze_buttons.Visible = false;
		radio_mode_contacts_jumps_rj_fatigue.Visible = false;
		radio_mode_contacts_runs_evolution.Visible = false;
		radio_mode_contacts_sprint.Visible = false;
		grid_race_analyzer_capture_tab_result_views.Visible = false;
		notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.STATISTICS);
		button_inspect_last_test_run_intervallic.Visible = false;
		button_force_sensor_adjust.Visible = false;
		button_force_sensor_sync.Visible = false;
		frame_jumps_automatic.Visible = false;
		box_wilight.Visible = false;
		box_wilight_commands.Visible = false;
		grid_fourPlatforms.Visible = false;
		box_event_execute_label_message.Visible = true;
		hbox_encoder_show_signal_table.Visible = Constants.ModeIsENCODER (m);
		box_contacts_graph_exercise.Visible = true;
		label_resultsSession_encoder_saved_repetitions.Visible = false;

		hbox_combo_select_contacts_top_with_arrows.Visible = false; //TODO: this will be unneded

		event_execute_label_message.Text = "";
		box_jump_simple_height.Visible = false;
		box_run_simple_time.Visible = false;

		// ---- box_capture_current ---->
		if (m == Constants.Modes.JUMPSSIMPLE || m == Constants.Modes.RUNSSIMPLE)
		{
			box_capture_current.Visible = false;
		} else {
			box_capture_current.Visible = true;

			align_drawingarea_realtime_capture_cairo.Visible = (
					m == Constants.Modes.JUMPSREACTIVE ||
					m == Constants.Modes.RUNSINTERVALLIC ||
					m == Constants.Modes.OTHER ||
					m == Constants.Modes.WILIGHT);

			box_capture_current_forceSensor.Visible = false;
			hbox_capture_current_runEncoder.Visible = false;
			box_beepTest.Visible = false;
			vbox_capture_current_encoder.Visible = false;

			if (Constants.ModeIsFORCESENSOR (m))
				box_capture_current_forceSensor.Visible = true;
			else if(m == Constants.Modes.RUNSENCODER)
				hbox_capture_current_runEncoder.Visible = true;
			else if (m == Constants.Modes.BEEPTEST)
				box_beepTest.Visible = true;
			else if (Constants.ModeIsENCODER (m))
				vbox_capture_current_encoder.Visible = true;
		}

		// to select heights/times on barplot (jumpsReactive)
		if (m == Constants.Modes.JUMPSSIMPLE || m == Constants.Modes.JUMPSREACTIVE)
		{
			box_resultsSession_jumpVar.Visible = true;
			if (preferences.heightPreferred)
				radio_resultsSession_jump_heights.Active = true;
			else
				radio_resultsSession_jump_times.Active = true;
		} else
			box_resultsSession_jumpVar.Visible = false;

		if (m == Constants.Modes.RUNSSIMPLE || m == Constants.Modes.RUNSINTERVALLIC)
			box_resultsSession_runVar.Visible = true;
		else
			box_resultsSession_runVar.Visible = false;

		if (m == Constants.Modes.RUNSENCODER)
			box_resultsSession_raVar.Visible = true;
		else
			box_resultsSession_raVar.Visible = false;

		if (Constants.ModeIsFORCESENSOR (m))
			box_resultsSession_forceVar.Visible = true;
		else
			box_resultsSession_forceVar.Visible = false;

		resultsSession_bestLast_controls ();

		vbox_event_execute_drawingarea_run_interval_realtime_capture_cairo.Visible = false; //just runEncoder

		// <---- box_capture_current ----

		if(chronopicRegister == null)
			chronopicRegisterUpdate(false);
		chronopicRegister.ListSelectedForAllModes (); //debug
		//if not selected, assign (auto-select) any compatible (using NumConnectedOfType)
		if (chronopicRegister.GetSelectedForMode (m).Port == "")
			chronopicRegister.SetAnyCompatibleConnectedAsSelected (m);
		//show button_detect depending on selected or not
		button_detect_show_hide (chronopicRegister.GetSelectedForMode (m).Port == "");

		if ( Config.SimulatedCapture && (Constants.ModeIsFORCESENSOR (m) || Constants.ModeIsENCODER (m)) )
			button_detect_show_hide (false);

		fullscreen_button_fullscreen_contacts.Visible = false;

		button_dialog_result_contacts.Visible = (
				m == Constants.Modes.JUMPSSIMPLE ||
				m == Constants.Modes.JUMPSREACTIVE ||
				m == Constants.Modes.RUNSSIMPLE ||
				m == Constants.Modes.RUNSINTERVALLIC); // ||
				//m == Constants.Modes.OTHER); //FourPlatforms no need, it is a different button always shown on that mode


		//blank exercise options: useful for changing from jumps or runs to forceSensor, runEncoder, reaction time, other
		label_contacts_exercise_selected_name.Visible = true; //will not be visible when all the contacts_top combo is implemented
		label_contacts_exercise_selected_options_blank ();
		label_contacts_exercise_info.Text = "";


		//on OSX R is not installed by default. Check if it's installed. Needed for encoder and force sensor
		if (Constants.ModeIsENCODER (m) || Constants.ModeIsFORCESENSOR (m))
		{
			if (operatingSystem == UtilAll.OperatingSystems.MACOSX &&
				! Util.FileExists(Constants.ROSX))
			{
				showMacRInstallMessage ();
				show_start_page ();
				return;
			}
			else if (operatingSystem == UtilAll.OperatingSystems.LINUX &&
				! ExecuteProcess.InstalledOnLinux ("R"))
			{
				showLinuxRInstallMessage ();
				show_start_page ();
				return;
			}
		}

		if(m == Constants.Modes.JUMPSSIMPLE || m == Constants.Modes.JUMPSREACTIVE)
		{
			//notebook_capture_analyze.ShowTabs = true;
			button_threshold.Visible = true;

			label_contacts_exercise_selected_options_visible (true);
			image_top_laterality_contacts.Visible = false;

			hbox_change_modes_jumps.Visible = true;
			button_contacts_capture_save_image.Visible = true;

			if(m == Constants.Modes.JUMPSSIMPLE) 
			{
				notebooks_change(m);
				on_extra_window_jumps_test_changed(new object(), new EventArgs());

				box_resultsSession_bestLast.Visible = true;

				//align_check_vbox_contacts_graph_legend.Visible = true;
				//vbox_contacts_graph_legend.Visible = false;

				frame_jumps_automatic.Visible = true;

				if(radio_mode_contacts_analyze.Active)
					radio_mode_contacts_analyze_buttons_visible (m);

				showHideFourPlatformsJumpsDrawingArea ();
			} else {
				notebooks_change(m);
				button_contacts_bells.Sensitive = true;
				on_extra_window_jumps_rj_test_changed(new object(), new EventArgs());

				box_resultsSession_bestLast.Visible = true;

				box_capture_current.Visible = true;

				//align_check_vbox_contacts_graph_legend.Visible = false;
				//vbox_contacts_graph_legend.Visible = false;
			}

			createComboSelectContactsTop ();
			label_contacts_exercise_selected_name.Visible = false;
			hbox_combo_select_contacts_top_with_arrows.Visible = true; //this will be unneded
			on_radio_contacts_graph_test_toggled (new object (), new EventArgs ()); //to ensure data is updated

			notebook_analyze_export_csv_data.CurrentPage = Convert.ToInt32 (notebook_analyze_export_csv_data_pages.JUMPS);
			check_contacts_export_jumps_simple.Active = (m == Constants.Modes.JUMPSSIMPLE);
			check_contacts_export_jumps_simple_mean_max_tables.Active = (m == Constants.Modes.JUMPSSIMPLE);
			check_contacts_export_jumps_reactive.Active = (m == Constants.Modes.JUMPSREACTIVE);
			radio_contacts_export_individual_current_session.Active = true;
			on_radio_contacts_export_individual_current_session_toggled (new object (), new EventArgs ());

			/*
			if(radio_mode_contacts_jumps_profile.Active || radio_mode_contacts_jumps_dj_optimal_fall.Active ||
					radio_mode_contacts_jumps_weight_fv_profile.Active || radio_mode_contacts_jumps_evolution.Active)
				radio_mode_contacts_capture.Active = true;
				*/

			feedbackWin.View(Constants.BellModes.JUMPS, preferences, encoderRhythm, false); //not viewWindow
			if(radio_mode_contacts_analyze.Active)
				radio_mode_contacts_analyze_buttons_visible (m);
		}
		else if(m == Constants.Modes.RUNSSIMPLE || m == Constants.Modes.RUNSINTERVALLIC)
		{
			LogB.Information("change mode is called");
			LogB.Information(string.Format("wireless conditions A: {0}, {1}",
				cp2016.StoredWireless, chronopicRegister != null));

			LogB.Information(string.Format("wireless conditions B: {0}, {1}",
				cp2016.StoredWireless, chronopicRegister != null));

			//notebook_capture_analyze.ShowTabs = true;
			button_threshold.Visible = ! (chronopicRegister != null && chronopicRegister.NumConnectedOfType(ChronopicRegisterPort.Types.RUN_WIRELESS) == 1);

			label_contacts_exercise_selected_options_visible (true);
			image_top_laterality_contacts.Visible = false;

			hbox_change_modes_runs.Visible = true;
			button_contacts_capture_save_image.Visible = true;

			if(m == Constants.Modes.RUNSSIMPLE) 
			{
				notebooks_change(m);
				on_extra_window_runs_test_changed(new object(), new EventArgs());

				box_resultsSession_bestLast.Visible = true;

				//align_check_vbox_contacts_graph_legend.Visible = true;
				//vbox_contacts_graph_legend.Visible = false;

				//show icon but have it unsensitive until there's a run
				button_inspect_last_test_run_simple.Visible = true;
				button_inspect_last_test_run_simple.Sensitive = false;
			}
			else
			{
				notebooks_change(m);
				button_contacts_bells.Sensitive = true;
				on_extra_window_runs_interval_test_changed(new object(), new EventArgs());

				//show icon but have it unsensitive until there's a run
				button_inspect_last_test_run_intervallic.Visible = true;
				button_inspect_last_test_run_intervallic.Sensitive = false;

				box_resultsSession_bestLast.Visible = true;

				box_capture_current.Visible = true;
				vbox_event_execute_drawingarea_run_interval_realtime_capture_cairo.Visible = true;

				//align_check_vbox_contacts_graph_legend.Visible = false;
				//vbox_contacts_graph_legend.Visible = false;

				createTreeView_runs_interval_sprint (treeview_runs_interval_sprint);

				if(radio_mode_contacts_analyze.Active)
					radio_mode_contacts_analyze_buttons_visible (m);
			}

			notebook_analyze_export_csv_data.CurrentPage = Convert.ToInt32 (notebook_analyze_export_csv_data_pages.RUNS);
			check_contacts_export_runs_simple.Active = (m == Constants.Modes.RUNSSIMPLE);
			check_contacts_export_runs_intervallic.Active = (m == Constants.Modes.RUNSINTERVALLIC);
			radio_contacts_export_individual_current_session.Active = true;
			on_radio_contacts_export_individual_current_session_toggled (new object (), new EventArgs ());

			feedbackWin.View(Constants.BellModes.RUNS, preferences, encoderRhythm, false); //not viewWindow
			createComboSelectContactsTop ();
			label_contacts_exercise_selected_name.Visible = false;
			hbox_combo_select_contacts_top_with_arrows.Visible = true; //this will be unneded
			on_radio_contacts_graph_test_toggled (new object (), new EventArgs ()); //to ensure data is updated
		}
		else if (Constants.ModeIsENCODER (m))
		{
			/*
			 * If there's a signal on gravitatory and we move to inertial, 
			 * interface has to change to YESPERSON (meaning no_signal).
			 * But, if there's no person shoud continue on NOPERSON
			 */
			if(currentPerson != null &&
					selectRowTreeView_persons(treeview_persons, myTreeViewPersons.FindRow(currentPerson.UniqueID)))
				encoderButtonsSensitive(encoderSensEnum.YESPERSON);
			
			blankEncoderInterface();
			hbox_change_modes_encoder.Visible = true;
			radio_change_modes_encoder_gravitatory.Visible = (m == Constants.Modes.POWERGRAVITATORY);
			radio_change_modes_encoder_inertial.Visible = (m == Constants.Modes.POWERINERTIAL);

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
				// change encoderConfigurationNewCapture if needed
				if (encoderConfigurationNewCapture.has_inertia)
				{
					EncoderConfigurationSQLObject econfSO = SqliteEncoderConfiguration.SelectActive(Constants.EncoderGI.GRAVITATORY);
					encoderConfigurationNewCapture = econfSO.encoderConfiguration;
					setEncoderConfigurationLabels (econfSO.name.ToString (), encoderConfigurationNewCapture.code);
					setEncoderTypePixbuf();

					changed = true;
				}

				currentEncoderGI = Constants.EncoderGI.GRAVITATORY;
				encoder_change_displaced_weight_and_1RM ();
				hbox_capture_1RM.Visible = true;

				//notebook_encoder_capture_extra_mass.CurrentPage = 0;
				label_button_encoder_select.Text = Catalog.GetString("Configure");
				label_encoder_exercise_mass.Visible = true;
				hbox_encoder_exercise_mass.Visible = true;
				label_encoder_exercise_inertia.Visible = false;
				box_encoder_exercise_inertia.Visible = false;
				hbox_encoder_exercise_gravitatory_min_mov.Visible = true;
				hbox_encoder_exercise_inertial_min_mov.Visible = false;

				if(radio_encoder_analyze_individual_current_set.Active || radio_encoder_analyze_individual_current_session.Active)
				{
					radiobutton_encoder_analyze_1RM.Visible = true;
					if(radiobutton_encoder_analyze_1RM.Active)
						hbox_combo_encoder_analyze_1RM.Visible=true;
				}

				if(radio_encoder_analyze_individual_current_set.Active ||
						radio_encoder_analyze_individual_current_session.Active ||
						radio_encoder_analyze_groupal_current_session.Active)
					radiobutton_encoder_analyze_neuromuscular_profile.Visible = true;

				//hbox_encoder_capture_1_or_cont.Visible = true;
				label_gravitatory_vpf_propulsive.Visible = preferences.encoderPropulsive;

				notebook_encoder_top.Page = 0;
				image_encoder_exercise.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "image_weight.png");
			}
			else //(m == Constants.Modes.POWERINERTIAL)
			{
				//change encoderConfigurationNewCapture if needed
				if(! encoderConfigurationNewCapture.has_inertia)
				{
					EncoderConfigurationSQLObject econfSO = SqliteEncoderConfiguration.SelectActive(Constants.EncoderGI.INERTIAL);
					encoderConfigurationNewCapture = econfSO.encoderConfiguration;
					setEncoderConfigurationLabels (econfSO.name.ToString (), encoderConfigurationNewCapture.code);
					setEncoderTypePixbuf();

					changed = true;
				}
				
				currentEncoderGI = Constants.EncoderGI.INERTIAL;
				hbox_capture_1RM.Visible = false;

				//notebook_encoder_capture_extra_mass.CurrentPage = 1;
				label_button_encoder_select.Text = Catalog.GetString("Configure");
				label_encoder_exercise_mass.Visible = false;
				hbox_encoder_exercise_mass.Visible = false;
				label_encoder_exercise_inertia.Visible = true;
				box_encoder_exercise_inertia.Visible = true;
				hbox_encoder_exercise_gravitatory_min_mov.Visible = false;
				hbox_encoder_exercise_inertial_min_mov.Visible = true;

				radiobutton_encoder_analyze_1RM.Visible = false;
				hbox_combo_encoder_analyze_1RM.Visible=false;
				radiobutton_encoder_analyze_neuromuscular_profile.Visible = false;
				
				label_gravitatory_vpf_propulsive.Visible = false;

				notebook_encoder_top.Page = 1;
				image_encoder_exercise.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "image_inertia.png");
			}

			feedbackWin.View(Constants.BellModes.ENCODERGRAVITATORY, preferences, encoderRhythm, false); //not viewWindow
			label_resultsSession_encoder_saved_repetitions.Visible = true;
			encoderConfigurationGUIUpdate();
			encoderGuiChangesAfterEncoderConfigurationWin(true);
			if(changed) {
				prepareAnalyzeRepetitions ();
			}

			if(! encoderPreferencesSet)
			{
				setEncoderExerciseOptionsFromPreferences();
				encoderPreferencesSet = true;
			}
		} 
		else if(Constants.ModeIsFORCESENSOR (m))
		{
			notebooks_change(m);
			box_capture_current.Visible = true;

			blankForceSensorInterface();
			if (m == Constants.Modes.FORCESENSORISOMETRIC)
			{
				tvFS_AB = new TreeviewFSAnalyze (treeview_ai_AB, "A", "B");
				tvFS_CD = new TreeviewFSAnalyze (treeview_ai_CD, "C", "D");
			} else //if (m == Constants.Modes.FORCESENSORELASTIC)
			{
				tvFS_AB = new TreeviewFSAnalyzeElastic (treeview_ai_AB, "A", "B");
				tvFS_CD = new TreeviewFSAnalyzeElastic (treeview_ai_CD, "C", "D");
			}
			tvFS_other = new TreeviewFSAnalyzeOther (treeview_force_sensor_ai_other);

			//we need combo_select_contacts_top before updateForceExerciseCombo
			createComboSelectContactsTop ();
			label_contacts_exercise_selected_name.Visible = false;
			hbox_combo_select_contacts_top_with_arrows.Visible = true; //this will be unneded

			//combos should show exercises (isometric or elastic)
			updateForceExerciseCombo ();

			button_contacts_bells.Sensitive = true;
			//notebook_capture_analyze.ShowTabs = false; //only capture tab is shown (only valid for "OTHER" tests)
			button_threshold.Visible = false;
			button_force_sensor_adjust.Visible = true;
			//button_force_sensor_sync.Visible = true; //TODO: show again when it fully works, now is hidden for 2.1.0 release
			//notebook_capture_analyze.GetNthPage(2).Hide(); //hide jumpsProfile on other tests

			box_resultsSession_bestLast.Visible = true;

			hbox_change_modes_force_sensor.Visible = true;
			radio_change_modes_contacts_isometric.Visible = (m == Constants.Modes.FORCESENSORISOMETRIC || m == Constants.Modes.FORCESENSORELASTIC);
			radio_change_modes_contacts_elastic.Visible = (m == Constants.Modes.FORCESENSORISOMETRIC || m == Constants.Modes.FORCESENSORELASTIC);

			//align_check_vbox_contacts_graph_legend.Visible = false;
			//vbox_contacts_graph_legend.Visible = false;

			setLabelContactsExerciseSelected(m);
			//better use the followin so we will have the Elastic/not elastic display on mode change
			on_combo_force_sensor_exercise_changed (new object(), new EventArgs ());
			//setLabelContactsExerciseSelectedOptionsForceSensor();

			feedbackWin.View(Constants.BellModes.FORCESENSOR, preferences, encoderRhythm, false); //not viewWindow
			label_contacts_exercise_selected_options_visible (false);
			image_top_laterality_contacts.Visible = true;
			setForceSensorLateralityPixbuf();
			fullscreen_button_fullscreen_contacts.Visible = true;

			signalAnalyzeButtonsVisibility ();

			//forceSensor and runEncoder
			check_run_encoder_export_instantaneous.Visible = false;
			label_run_encoder_export_discarded.Visible = false;
			//forceSensor and runEncoder notebook_ai_model_graph_table_triggers
			notebook_ai_model_graph_table_triggers.GetNthPage
				(Convert.ToInt32 (notebook_ai_model_graph_table_triggers_pages.TRIGGERS)).Hide();
			notebook_ai_model_graph_table_triggers.GetNthPage
				(Convert.ToInt32 (notebook_ai_model_graph_table_triggers_pages.TABLE)).Hide();
			notebook_ai_model_graph_table_triggers.ShowTabs = false;
			notebook_ai_model_graph_table_triggers.ShowBorder = false;

			//force_sensor_recalculate ();
		}
		else if(m == Constants.Modes.RUNSENCODER)
		{
			notebooks_change(m);
			box_capture_current.Visible = true;

			button_contacts_bells.Sensitive = true;

			//notebook_capture_analyze.ShowTabs = false; //only capture tab is shown (only valid for "OTHER" tests)
			button_threshold.Visible = false;
			//notebook_capture_analyze.GetNthPage(2).Hide(); //hide jumpsProfile on other tests

			grid_race_analyzer_capture_tab_result_views.Visible = true;
			hbox_change_modes_runs.Visible = true;

			//align_check_vbox_contacts_graph_legend.Visible = false;
			//vbox_contacts_graph_legend.Visible = false;

			combo_race_analyzer_device.Active = 0;
			runEncoderImageTestChange();
			setLabelContactsExerciseSelected(m);

			label_contacts_exercise_selected_options_visible (false);
			image_top_laterality_contacts.Visible = false;

			on_combo_run_encoder_exercise_changed (new object(), new EventArgs ());

			feedbackWin.View(Constants.BellModes.RUNSENCODER, preferences, encoderRhythm, false); //not viewWindow
			createComboSelectContactsTop ();
			label_contacts_exercise_selected_name.Visible = false;
			hbox_combo_select_contacts_top_with_arrows.Visible = true; //this will be unneded

			signalAnalyzeButtonsVisibility ();
			button_video_play_this_test.Sensitive = (currentRunEncoder != null && currentRunEncoder.VideoURL != "");

			//forceSensor and runEncoder
			check_run_encoder_export_instantaneous.Visible = true;
			label_run_encoder_export_discarded.Visible = true;
			//forceSensor and runEncoder notebook_ai_model_graph_table_triggers
			notebook_ai_model_graph_table_triggers.GetNthPage
				(Convert.ToInt32 (notebook_ai_model_graph_table_triggers_pages.TRIGGERS)).Show();
			notebook_ai_model_graph_table_triggers.GetNthPage
				(Convert.ToInt32 (notebook_ai_model_graph_table_triggers_pages.TABLE)).Show();
			notebook_ai_model_graph_table_triggers.ShowTabs = true;
			notebook_ai_model_graph_table_triggers.ShowBorder = true;

			tvRA_AB = new TreeviewRAAnalyze (treeview_ai_AB, "A", "B");
			tvRA_CD = new TreeviewRAAnalyze (treeview_ai_CD, "C", "D");
		}
		else if (m == Constants.Modes.OTHER) //(contacts / other)
		{
			//similar to WILIGHT
			notebooks_change(m);
			//radio_mode_contacts_analyze.Visible = false;

			radio_change_modes_contacts_fourPlatforms.Visible = true;

			grid_fourPlatforms.Visible = true;
			box_contacts_graph_exercise.Visible = false; //selection of exercise

			box_capture_current.Visible = true;

			//analyze has just export_csv
			notebook_analyze_export_csv_data.CurrentPage = Convert.ToInt32 (notebook_analyze_export_csv_data_pages.OTHER);
			//wilightApp1Init ();
		}

		if (m == Constants.Modes.BEEPTEST)
		{
			notebooks_change(m);
			radio_mode_contacts_analyze.Visible = false;

			box_event_execute_label_message.Visible = false;
			hbox_change_modes_runs.Visible = true; //TODO: add beep test

			box_capture_current.Visible = true;

			beepTestApp1Init ();

			box_contacts_graph_show_graph_table.Visible = false; //do not show the graph/table selector
			vbox_contacts_capture_graph.Visible = false; //do not show results_session graph
			scrolledwindow_treeview_results_session.Visible = true;
			box_results_session_zoom.Visible = true;
		} else {
			box_contacts_graph_show_graph_table.Visible = true;
		}

		if (m == Constants.Modes.WILIGHT)
		{
			notebooks_change(m);
			radio_mode_contacts_analyze.Visible = false;

			//hbox_change_modes_runs.Visible = true; //TODO: add beep test
			radio_change_modes_contacts_wilight.Visible = true;

			box_wilight.Visible = true;
			box_contacts_graph_exercise.Visible = false; //selection of exercise

			box_capture_current.Visible = true;
			wilightApp1Init ();
		}

		// on change mode, always select all tests
		if (radio_contacts_graph_currentTest.Active)
			radio_contacts_graph_allTests.Active = true;

		on_treeview_results_session_cursor_changed (new object (), new EventArgs ());

		//show feedback icon
		Pixbuf pixbufBellActive = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "stock_bell_active.png");
		Pixbuf pixbufBellInactive = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "stock_bell_none.png");
		if(
				( (m == Constants.Modes.JUMPSSIMPLE || m == Constants.Modes.JUMPSREACTIVE) &&
				  feedbackWin.FeedbackActive(Constants.BellModes.JUMPS)) ||
				( (m == Constants.Modes.RUNSSIMPLE || m == Constants.Modes.RUNSINTERVALLIC) &&
				  feedbackWin.FeedbackActive(Constants.BellModes.RUNS)) ||
				( Constants.ModeIsFORCESENSOR (m) &&
				  feedbackWin.FeedbackActive(Constants.BellModes.FORCESENSOR)) )
			image_contacts_bell.Pixbuf = pixbufBellActive;
		else
			image_contacts_bell.Pixbuf = pixbufBellInactive;

		if (Constants.ModeIsENCODER (m) &&
				feedbackWin.FeedbackActive(Constants.BellModes.ENCODERGRAVITATORY) )
			image_encoder_bell.Pixbuf = pixbufBellActive;
		else
			image_encoder_bell.Pixbuf = pixbufBellInactive;


		//show the program
		notebook_start.CurrentPage = Convert.ToInt32(notebook_start_pages.PROGRAM);

		if (! Constants.ModeIsENCODER (m))
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

		//json upload
		button_contacts_json_upload.Visible = configChronojump.JsonUploadNeedsButton &&
			(m == Constants.Modes.JUMPSSIMPLE ||
			 m == Constants.Modes.RUNSSIMPLE ||
			 m == Constants.Modes.RUNSINTERVALLIC);

		//grid insert
		if (m == Constants.Modes.RUNSSIMPLE && configChronojump.CanInsertTests) {
			box_contacts_insert_test.Visible = true;
			notebook_contacts_insert_test.CurrentPage = 0;
		}
		else if (m == Constants.Modes.RUNSINTERVALLIC && configChronojump.CanInsertTests) {
			box_contacts_insert_test.Visible = true;
			notebook_contacts_insert_test.CurrentPage = 1;
		} else
			box_contacts_insert_test.Visible = false;


		//on capture, show phases, time, record if we are not on forcesensor mode
		showHideCaptureSpecificControls (m);

		forceSensorShowFilterIfNeeded ();

		last_menuitem_mode_defined = true;

		SqlitePreferences.Update(SqlitePreferences.LastMode, m.ToString(), false);

		chronojumpWindowTestsNext();

		if (m != Constants.Modes.JUMPSSIMPLE && m != Constants.Modes.RUNSSIMPLE)
			GLib.Timeout.Add (50, new GLib.TimeoutHandler (vpaned_tests_center));

		hpaned_contacts_graph_table_center_if_needed ();

		setLabelContactsExerciseSelectedOptions();

		//beepTest changes treeview persons, redo it
		if(myTreeViewPersons != null)
		{
			treeview_persons_storeReset();
			myTreeViewPersons.RestSecondsMark = get_configured_rest_time_in_seconds();
			fillTreeView_persons();
		}
		updatingRestTimes = (m != Constants.Modes.BEEPTEST);

		if (Constants.ModeIsENCODER (m))
			GLib.Timeout.Add (75, new GLib.TimeoutHandler (encoder2ndRowPos)); // here it works correctly (at least on my computer). Just before treeview_persons stuff, it doesn't.

		if (remoteTest != null)
			remoteTest.Current_mode = m;

		changeModeIcon (m);
	}

	private void showMacRInstallMessage ()
	{
		new DialogMessage(Constants.MessageTypes.WARNING,
				Catalog.GetString("Sorry, R software is not installed.") +
				"\n" + Catalog.GetString("Please, install it from here:") +
				"\n\n" + Constants.RmacDownload,
				"button_go_r_mac");
	}
	private void showLinuxRInstallMessage ()
	{
		new DialogMessage (Constants.MessageTypes.WARNING,
				Catalog.GetString ("Sorry, R software is not installed."));
	}
	private void showLinux7zInstallMessage ()
	{
		new DialogMessage (Constants.MessageTypes.WARNING,
				string.Format (Catalog.GetString ("Sorry, {0} software is not installed."), "7z"));
	}

	private void on_check_contacts_capture_show_modes_clicked (object o, EventArgs args)
	{
		if(! followSignals) //TODO: check if has to be this boolean
			return;

		vbox_contacts_capture_graph.Visible = check_contacts_capture_graph.Active;

		scrolledwindow_treeview_results_session.Visible = check_contacts_capture_table.Active;
		box_results_session_zoom.Visible = check_contacts_capture_table.Active;

		hpaned_contacts_graph_table_center_if_needed ();

		if (check_contacts_capture_graph.Active || check_contacts_capture_table.Active)
		{
			box_contacts_capture_show_need_one.Visible = false;
			GLib.Timeout.Add (50, new GLib.TimeoutHandler (vpaned_tests_center_if_max_pos));
		} else
		{
			label_contacts_capture_show_need_one.Text = "<b>" + Catalog.GetString("Select at least one") + "</b>";
			label_contacts_capture_show_need_one.UseMarkup = true;
			box_contacts_capture_show_need_one.Visible = true;

			//vpaned_tests.Position = vpaned_tests.MaxPosition; // does not work
			GLib.Timeout.Add (50, new GLib.TimeoutHandler (vpaned_tests_max_pos)); // work
		}

		/*
		   update the preferences variable
		   note as can be changed while capturing, it will be saved to SQL on exit
		   to not have problems with SQL while capturing
		   */
		preferences.contactsCaptureDisplay = new ContactsCaptureDisplay(
				check_contacts_capture_table.Active,
				check_contacts_capture_graph.Active);
	}

	private bool vpaned_tests_max_pos()
	{
		vpaned_tests.Position = vpaned_tests.MaxPosition;
		return false;
	}

	private bool vpaned_tests_center ()
	{
		vpaned_tests.Position = Convert.ToInt32 (vpaned_tests.Allocation.Height / 2.0);
		return false;
	}

	private bool vpaned_tests_center_if_max_pos ()
	{
		if (vpaned_tests.Position == vpaned_tests.MaxPosition)
			vpaned_tests.Position = Convert.ToInt32 (vpaned_tests.Allocation.Height / 2.0);

		return false;
	}

	//when showing both widgets, start at the middle
	private void hpaned_contacts_graph_table_center_if_needed ()
	{
		if(vbox_contacts_capture_graph.Visible && scrolledwindow_treeview_results_session.Visible)
			hpaned_contacts_graph_table.Position = Convert.ToInt32(frame_contacts_graph_table.Allocation.Width / 2.0);
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
		hbox_capture_phases.Visible = ( ! Constants.ModeIsFORCESENSOR (m) && m != Constants.Modes.RUNSENCODER);
		hbox_capture_time.Visible = ( ! Constants.ModeIsFORCESENSOR (m) && m != Constants.Modes.RUNSENCODER);

		if(! configChronojump.Compujump)
			showWebcamCaptureContactsControls(true);
	}

	void setEncoderTypePixbuf()
	{
		Pixbuf pixbuf = encoderConfigurationNewCapture.GetPixbuf;
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
		


	//jumps
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

	//runs
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
	private void on_button_selector_start_beepTest_clicked(object o, EventArgs args)
	{
		changeModeCheckRadios (Constants.Modes.BEEPTEST);
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
	private void on_radio_change_modes_contacts_runs_beepTest_toggled (object o, EventArgs args)
	{
		if(radio_change_modes_contacts_runs_beepTest.Active)
			changeMode (Constants.Modes.BEEPTEST);
	}

	private void on_button_selector_start_wilight_clicked (object o, EventArgs args)
	{
		changeModeCheckRadios (Constants.Modes.WILIGHT);
	}

	private void on_button_selector_start_fourPlatforms_clicked (object o, EventArgs args)
	{
		changeModeCheckRadios (Constants.Modes.OTHER);
	}

	//forceSensor (isometric, elastic)
	private void on_button_selector_start_force_sensor_isometric_clicked(object o, EventArgs args)
	{
		changeModeCheckRadios (Constants.Modes.FORCESENSORISOMETRIC);
	}
	private void on_button_selector_start_force_sensor_elastic_clicked(object o, EventArgs args)
	{
		changeModeCheckRadios (Constants.Modes.FORCESENSORELASTIC);
	}
	private void on_radio_change_modes_contacts_isometric_toggled (object o, EventArgs args)
	{
		if (radio_change_modes_contacts_isometric.Active)
			changeMode (Constants.Modes.FORCESENSORISOMETRIC);
	}
	private void on_radio_change_modes_contacts_elastic_toggled (object o, EventArgs args)
	{
		if (radio_change_modes_contacts_elastic.Active)
			changeMode (Constants.Modes.FORCESENSORELASTIC);
	}

	//encoder
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

	/*
	private void on_button_selector_start_rt_clicked(object o, EventArgs args)
	{
		changeMode (Constants.Modes.RT);
	}

	private void on_button_selector_start_other_clicked(object o, EventArgs args)
	{
		changeMode (Constants.Modes.OTHER);
	}
	*/


	//clicked from start_window
	private void on_button_menu_2_2_2_clicked (object o, EventArgs args)
	{
		//manage only Active events
		if (! ((Gtk.RadioButton) o).Active)
			return;

		//note on glade the event is button_clicked to receive a click when the radio is already active

		button_menu_2_2_2_manage (o, true); //can have double click if that notebook_menu_2_2_2 is already that value
	}

	private void button_menu_2_2_2_manage (object o, bool canBeDoubleClick)
	{
		string title = "";
		string desc = "";
		if (o == (object) radio_menu_2_2_2_jumps)
		{
			canBeDoubleClick = false;

			title = "Jumps";
			desc = Catalog.GetString ("Measured by a contact platform");
			notebook_menu_2_2_2.CurrentPage = 0;
		}
		else if (o == (object) radio_menu_2_2_2_races)
		{
			canBeDoubleClick = false;

			title = "Races";
			desc = ""; //"Measured by …";

			notebook_menu_2_2_2.CurrentPage = 1;
		}
		else if (o == (object) radio_menu_2_2_2_wilight)
		{
			title = "Reaction time";
			desc = "Reaction time tests with Wilight"; //TODO: make it translatable
			notebook_menu_2_2_2.CurrentPage = 3;
		}
		else if (o == (object) radio_menu_2_2_2_fourPlatforms)
		{
			title = "Four platforms";
			desc = "Four platforms mode with custom device"; //TODO: make it translatable
			notebook_menu_2_2_2.CurrentPage = 3;
		}
		else if (o == (object) radio_menu_2_2_2_force)
		{
			title = "Force tests";
			desc = Catalog.GetString ("Force exercises measured by a force sensor");
			notebook_menu_2_2_2.CurrentPage = 2;
		}
		else if (o == (object) radio_menu_2_2_2_weights)
		{
			title = "Weights";
			desc = Catalog.GetString ("Speed/power exercises displacing weights measured by an encoder");
			notebook_menu_2_2_2.CurrentPage = 3;
		}
		else if (o == (object) radio_menu_2_2_2_inertial)
		{
			title = "Inertial";
			desc = Catalog.GetString ("Speed/power exercises rotating an inertial machine and measured by an encoder");
			notebook_menu_2_2_2.CurrentPage = 3;
		}

		//do not show desc on races (it has its own labels on table columns)
		align_label_selector_menu_2_2_2_desc.Visible = (o != (object) radio_menu_2_2_2_races);

		// if we already have clicked before, execute go!
		if (canBeDoubleClick && label_selector_menu_2_2_2_title.Text == Catalog.GetString(title)) //note the "<b></b>" are not on .Text
			on_button_menu_2_2_2_go_clicked (new object (), new EventArgs ());
		else {
			if (title != "")
			{
				label_selector_menu_2_2_2_title.Text = "<b>" + Catalog.GetString(title) + "</b>";
				label_selector_menu_2_2_2_title.UseMarkup = true;
			}
			if (desc != "")
				label_selector_menu_2_2_2_desc.Text = desc;
		}
	}

	private void on_button_menu_2_2_2_go_clicked (object o, EventArgs args)
	{
		//jumps, races, force modes have their own buttons
		if (radio_menu_2_2_2_wilight.Active)
			on_button_selector_start_wilight_clicked (new object (), new EventArgs ());
		else if (radio_menu_2_2_2_fourPlatforms.Active)
			on_button_selector_start_fourPlatforms_clicked (new object (), new EventArgs ());
		else if (radio_menu_2_2_2_weights.Active)
			on_button_selector_start_encoder_gravitatory_clicked (new object (), new EventArgs ());
		else if (radio_menu_2_2_2_inertial.Active)
			on_button_selector_start_encoder_inertial_clicked (new object (), new EventArgs ());
	}

	private void on_button_contacts_capture_save_image_clicked (object o, EventArgs args)
	{
		if(current_mode == Constants.Modes.JUMPSSIMPLE)
			checkFile(Constants.CheckFileOp.JUMPS_SIMPLE_CAPTURE_SAVE_IMAGE);
		else if(current_mode == Constants.Modes.JUMPSREACTIVE)
			checkFile(Constants.CheckFileOp.JUMPS_REACTIVE_CAPTURE_SAVE_IMAGE);
		else if(current_mode == Constants.Modes.RUNSSIMPLE)
			checkFile(Constants.CheckFileOp.RUNS_SIMPLE_CAPTURE_SAVE_IMAGE);
		else if(current_mode == Constants.Modes.RUNSINTERVALLIC)
			checkFile(Constants.CheckFileOp.RUNS_INTERVALLIC_CAPTURE_SAVE_IMAGE);
	}

	/*
	 * end of menu test selectors
	 */

	

	/*
	 * cancel ------->
	 */

	private void on_cancel_clicked (object o, EventArgs args) 
	{
		Button buttonClicked = o as Button;
		if (o == null)
			return;

		fullscreenLastCapture = (buttonClicked == fullscreen_capture_button_cancel);

		event_execute_ButtonCancel.Clicked -= new EventHandler(on_cancel_clicked);

		showHideBlinkIcon (blinkCapture, false);
		if (blinkCapture != null)
			blinkCapture.End ();

		if (cp2016.StoredWireless && currentEventExecute != null && currentEventExecute.ChronopicDisconnected)
		{
			button_detect_show_hide (true);
		}

		ChronopicRegisterPort crp = chronopicRegister.GetSelectedForMode (current_mode);

		if (Constants.ModeIsFORCESENSOR (current_mode))
			on_cancel_clicked_2_forceSensor ();
		else if (current_mode == Constants.Modes.RUNSENCODER)
			on_cancel_clicked_2_raceAnalyzer ();
		else if (current_mode == Constants.Modes.OTHER)
			on_cancel_clicked_2_other ();
		else if (current_mode == Constants.Modes.JUMPSSIMPLE &&
				crp.Port != "" && crp.Type == ChronopicRegisterPort.Types.FOURPLATFORMS)
			on_cancel_clicked_2_other ();
		else
			on_cancel_clicked_2_contacts_generic ();
	}

	private void on_cancel_clicked_2_forceSensor ()
	{
		LogB.Information (string.Format ("cancel clicked on force, capturingForce = {0}", capturingForce));
		if(capturingForce == arduinoCaptureStatus.STARTING || capturingForce == arduinoCaptureStatus.CAPTURING)
			forceProcessCancel = true;
	}

	private void on_cancel_clicked_2_raceAnalyzer ()
	{
		LogB.Information (string.Format ("cancel clicked on raceAnalyzer, capturingRunEncoder = {0}", capturingRunEncoder));
		if(capturingRunEncoder == arduinoCaptureStatus.STARTING || capturingRunEncoder == arduinoCaptureStatus.CAPTURING)
			runEncoderProcessCancel = true;
	}

	private void on_cancel_clicked_2_other ()
	{
		LogB.Information (string.Format ("cancel clicked on fourPlatforms, capturingFourPlatforms = {0}", capturingFourPlatforms));
		if(capturingFourPlatforms == arduinoCaptureStatus.STARTING || capturingFourPlatforms == arduinoCaptureStatus.CAPTURING)
			fourPlatformsProcessCancel = true;
	}

	private void on_cancel_clicked_2_contacts_generic ()
	{
		LogB.Information("cancel clicked contacts generic");

		if (webcamStatusEnum == WebcamStatusEnum.RECORDING)
		{
			webcamManage.RecordingStop ();
			webcamStatusEnum = WebcamStatusEnum.NOCAMERA;
			webcamRestoreGui (false);
		}

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

	/*
	 * <-------- cancel
	 */

	/*
	 * finish ------->
	 */

	private void on_finish_clicked (object o, EventArgs args) 
	{
		Button buttonClicked = o as Button;
		if (o == null)
			return;

		fullscreenLastCapture = (buttonClicked == fullscreen_capture_button_finish);

		//to avoid doble finish or cancel while finishing
		hideButtons();
		ChronopicRegisterPort crp = chronopicRegister.GetSelectedForMode (current_mode);

		if (Constants.ModeIsFORCESENSOR (current_mode))
			on_finish_clicked_2_forceSensor ();
		else if (current_mode == Constants.Modes.RUNSENCODER)
			on_finish_clicked_2_raceAnalyzer ();
		else if (current_mode == Constants.Modes.OTHER)
			on_finish_clicked_2_other ();
		else if (current_mode == Constants.Modes.JUMPSSIMPLE &&
				crp.Port != "" && crp.Type == ChronopicRegisterPort.Types.FOURPLATFORMS)
			on_finish_clicked_2_other ();
		else
			on_finish_clicked_2_contacts_generic ();
	}

	private void on_finish_clicked_2_forceSensor ()
	{
		LogB.Information (string.Format ("finish clicked on force, capturingForce = {0}", capturingForce));
		if(capturingForce == arduinoCaptureStatus.STARTING || capturingForce == arduinoCaptureStatus.CAPTURING)
			forceProcessFinish = true;
	}

	private void on_finish_clicked_2_raceAnalyzer ()
	{
		LogB.Information (string.Format ("finish clicked on raceAnalyzer, capturingRunEncoder = {0}", capturingRunEncoder));
		if(capturingRunEncoder == arduinoCaptureStatus.STARTING || capturingRunEncoder == arduinoCaptureStatus.CAPTURING)
			runEncoderProcessFinish = true;
	}

	private void on_finish_clicked_2_other ()
	{
		LogB.Information (string.Format ("finish clicked on fourPlatforms, capturingFourPlatforms = {0}", capturingFourPlatforms));
		if(capturingFourPlatforms == arduinoCaptureStatus.STARTING || capturingFourPlatforms == arduinoCaptureStatus.CAPTURING)
			fourPlatformsProcessFinish = true;
	}

	private void on_finish_clicked_2_contacts_generic ()
	{
		LogB.Information("finish clicked contacts generic");

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

	/*
	 * <-------- finish
	 */

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

	/*
	   ----------------- discover / detect devices --------->
	   */

	//also manages if networks or not, on networks do not show
	private void button_detect_show_hide (bool show)
	{
		if (configChronojump.Compujump)
			return;

		// Cloud-view cannot capture
		if (configChronojump.ReadFromCloudMainPath != "")
		{
			button_contacts_detect.Visible = false;
			hbox_contacts_detect_and_execute.Visible = false;
			button_encoder_detect.Visible = false;
			hbox_encoder_detect_and_execute.Visible = false;
			return;
		}

		button_contacts_detect.Visible = show;
		hbox_contacts_detect_and_execute.Visible = ! show;

		button_encoder_detect.Visible = show;
		hbox_encoder_detect_and_execute.Visible = ! show;
	}

	DiscoverWindow discoverWin;
	private void on_button_detect_clicked (object o, EventArgs args)
	{
		app1s_notebook_sup_entered_from = notebook_sup.CurrentPage;
		detect_devices_do ();
	}

	private void detect_devices_do ()
	{
		notebook_sup.CurrentPage = Convert.ToInt32 (notebook_sup_pages.MICRODISCOVER);
		event_execute_label_message.Text = "";
		menus_and_mode_sensitive (false);

		if(encoderThreadBG != null && encoderThreadBG.IsAlive)
		{
			stopCapturingInertialBG();

			//to have time on Windows to really have sp port closed and be able to read on chronopicRegister and/or discoverWin
			System.Threading.Thread.Sleep (1000);
		}

		if (Constants.ModeIsFORCESENSOR (current_mode) && portFSOpened)
			forceSensorDisconnect ();
		else if (current_mode == Constants.Modes.RUNSENCODER && portREOpened)
			runEncoderDisconnect ();

		chronopicRegisterUpdate (false);

		label_micro_discover_title.Text = string.Format (Catalog.GetString (
					"Detect devices compatible with: <b>{0}</b>"), Constants.ModePrint (current_mode));
		label_micro_discover_title.UseMarkup = true;
		box_micro_discover_nc.Visible = false;
		label_micro_discover_nc_current_mode.Text = Constants.ModePrint (current_mode);
		label_micro_discover_connect_error.Visible = false;

		discoverWin = new DiscoverWindow (app1,
				current_mode, chronopicRegister,
				vbox_micro_discover_main,
				button_micro_discover_refresh,
				image_micro_discover_refresh,
				box_micro_discover_assign_manually,
				buttonbox_micro_discover_assign_manually,
				label_micro_discover_not_found,
				grid_micro_discover,
				box_micro_discover_nc,
				button_micro_discover_cancel_close,
				image_button_micro_discover_cancel_close,
				label_button_micro_discover_cancel_close,
				image_button_micro_discover_assign_manually_cancel,
				check_discover_advanced.Active, check_discover_advanced,
				label_discover_advanced, image_discover_advanced,
				Constants.ModeIcon (current_mode),
				label_micro_discover_connect_error,
				Config.ColorBackgroundShiftedIsDark
				);

		if(! Config.UseSystemColor)
			UtilGtk.ContrastLabelsGrid (Config.ColorBackgroundShiftedIsDark, grid_micro_discover);

		discoverWin.FakeButtonClose.Clicked -= new EventHandler (on_discoverWindow_closed);
		discoverWin.FakeButtonClose.Clicked += new EventHandler (on_discoverWindow_closed);
	}

	private void on_button_micro_discover_refresh_clicked (object o, EventArgs args)
	{
		detect_devices_do ();
	}

	private void on_check_discover_advanced_toggled (object o, EventArgs args)
	{
		if (discoverWin != null)
			discoverWin.ShowAdvanced (check_discover_advanced.Active);
	}

	private void on_button_micro_discover_assign_manually_cancel_clicked (object o, EventArgs args)
	{
		if (discoverWin != null)
			discoverWin.ShowAssignManuallyBox (false);
	}

	private void on_button_micro_discover_cancel_close_clicked (object o, EventArgs args)
	{
		if (discoverWin != null)
		{
			discoverWin.CancelCloseFromUser ();
			button_detect_show_hide (true); //as closed without use this, then show the big button again
		}
	}

	private void on_discoverWindow_closed (object o, EventArgs args)
	{
		chronopicRegister = discoverWin.ChronopicRegisterGet;

		//if(discoverWin.PortSelected != "")
		if(discoverWin.PortSelected.Port != "")
		{
			chronopicRegister.SetSelectedForMode (discoverWin.PortSelected, current_mode);
			button_detect_show_hide (false);

			//do not show the threshold on WICHRO
			//if ( chronopicRegister.NumConnectedOfType (ChronopicRegisterPort.Types.RUN_WIRELESS) == 1)
			if (current_mode == Constants.Modes.RUNSSIMPLE || current_mode == Constants.Modes.RUNSINTERVALLIC)
				button_threshold.Visible = (discoverWin.PortSelected.Type != ChronopicRegisterPort.Types.RUN_WIRELESS);
			else if (current_mode == Constants.Modes.WILIGHT)
				entry_wilight_port.Text = discoverWin.PortSelected.Port;
			else if (current_mode == Constants.Modes.OTHER) //FOURPLATFORMS
				entry_fourPlatforms_port.Text = discoverWin.PortSelected.Port;

			// close portFSOpened after discover to ensure do a forceSensorConnect()
			if (Constants.ModeIsFORCESENSOR (current_mode) && portFSOpened)
				portFSOpened = false;
			// same for runEncoder
			else if (current_mode == Constants.Modes.RUNSENCODER && portREOpened)
				portREOpened = false;

			if (current_mode == Constants.Modes.JUMPSSIMPLE)
				showHideFourPlatformsJumpsDrawingArea ();
		}

		notebook_sup.CurrentPage = app1s_notebook_sup_entered_from;
		menus_and_mode_sensitive (true);
	}


	/*
	   <---------------- fullscreen stuff --------
	   */

	private enum fullScreenChangeEnum { DONOTHING, CHANGETOFULL, CHANGETONORMAL };
	private fullScreenChangeEnum fullScreenChange = fullScreenChangeEnum.DONOTHING;
	private bool fullscreenLastCapture;
	private bool fullscreenCaptureSignalsNoFollow = false;

	private void on_fullscreeen_finish (object o, EventArgs args)
	{
		on_fullscreen_button_fullscreen_exit_clicked (o, args);

		if (Constants.ModeIsENCODER (current_mode))
			on_button_encoder_capture_finish_clicked (o, args);
		else
			on_finish_clicked (o, args);
	}

	private void on_fullscreen_button_encoder_capture_finish_cont_clicked (object o, EventArgs args)
	{
		on_fullscreen_button_fullscreen_exit_clicked (o, args);
		encoderProcessFinishContMode = true;
		on_button_encoder_capture_finish_clicked (o, args); //note this o will be checked
	}

	private void on_fullscreeen_cancel (object o, EventArgs args)
	{
		on_fullscreen_button_fullscreen_exit_clicked (o, args);

		if (Constants.ModeIsENCODER (current_mode))
			on_button_encoder_cancel_clicked (o, args);
		else
			on_cancel_clicked (o, args);
	}

	private void on_fullscreen_button_fullscreen_clicked (object o, EventArgs args)
	{
		fullscreenCaptureSignalsNoFollow = true;

		//always Decorated on Windows (fixes bug on 11 Pro)
		if (! UtilAll.IsWindows ())
			app1.Decorated = false;

		app1.Maximize ();

		notebook_start.CurrentPage = Convert.ToInt32 (notebook_start_pages.FULLSCREENCAPTURE);

		if (Constants.ModeIsFORCESENSOR (current_mode) || Constants.ModeIsENCODER (current_mode))
		{
			fullScreenChange = fullScreenChangeEnum.CHANGETOFULL;

			if (currentPerson != null)
				fullscreen_label_person.Text = currentPerson.Name;

			if (Constants.ModeIsFORCESENSOR (current_mode) && currentForceSensorExercise != null)
				fullscreen_label_exercise.Text = currentForceSensorExercise.Name;
			else if (Constants.ModeIsENCODER (current_mode))
				fullscreen_label_exercise.Text = label_encoder_top_exercise.Text;

			fullscreen_label_message.Text = "";
			fullscreen_capture_progressbar.Visible = Constants.ModeIsENCODER (current_mode);

			if (Constants.ModeIsENCODER (current_mode) && preferences.encoderCaptureInfinite)
			{
				fullscreen_button_encoder_capture_finish_cont.Visible = true;
				fullscreen_capture_box_buttons_finish_cancel.Visible = false;
			} else {
				fullscreen_button_encoder_capture_finish_cont.Visible = false;
				fullscreen_capture_box_buttons_finish_cancel.Visible = true;
			}
		}
	}

	private void on_fullscreen_button_fullscreen_exit_clicked (object o, EventArgs args)
	{
		maximizeOrNot (false); //use preferences
		fullscreenCaptureSignalsNoFollow = false;

		notebook_start.CurrentPage = Convert.ToInt32 (notebook_start_pages.PROGRAM);

		//if (Constants.ModeIsFORCESENSOR (current_mode))
			fullScreenChange = fullScreenChangeEnum.CHANGETONORMAL;
	}

	private void on_fullscreen_capture_drawingarea_cairo_draw (object o, DrawnArgs args)
	{
		if (Constants.ModeIsFORCESENSOR (current_mode))
			updateForceSensorCaptureSignalCairo (true);
		else if (Constants.ModeIsENCODER (current_mode))
			if (prepareEventGraphEncoderCurrent != null)
				prepareEncoderSignalBarplotCairo (true);
	}

	/*
	 * If the window is maximized or fullscreen does not get stored, so we need to catch this event
	 * With this and the GetSize we have everything. GetSize returns the size of the unmaximized win.
	 * All this is needed for returning to previous after finishing fullscreen on capture
	 */
	private void on_app1_window_state_event (object o, WindowStateEventArgs args)
	{
		LogB.Information ("on_app1_window_state_event");

		//on fullscreen capture we do not want to record the status of the window
		if (fullscreenCaptureSignalsNoFollow)
			return;

		//these numbers are related to: https://docs.gtk.org/gdk3/flags.WindowState.html
		//LogB.Information (args.Event.NewWindowState.ToString ());

		//This works: says "Maximized"
		//LogB.Information ("Maximized? " + (args.Event.NewWindowState & Gdk.WindowState.Maximized).ToString ());
		if ( (args.Event.NewWindowState & Gdk.WindowState.Maximized).ToString () == "Maximized")
		{
			LogB.Information ("Maximized!");
			if (app1.Decorated)
				preferences.maximized = Preferences.MaximizedTypes.YES;
			else
				preferences.maximized = Preferences.MaximizedTypes.YESUNDECORATED;
		} else {
			preferences.maximized = Preferences.MaximizedTypes.NO;
		}

		//this does not work, at least on Linux/Gnome:
		if ( (args.Event.NewWindowState & Gdk.WindowState.Fullscreen).ToString () == "Fullscreen")
			LogB.Information ("Fullscreen!");
	}

	/*
	   <---------------- end of discover / detect devices --------
	   */


	private void on_button_execute_test_clicked (object o, EventArgs args)
	{
		if (Constants.ModeIsFORCESENSOR (current_mode))
		{
			LogB.Debug ("execute test mode: force_sensor");
			/*
			 * force sensor is not FTDI
			 */

			if (! Config.SimulatedCapture && chronopicRegister.GetSelectedForMode (current_mode).Port == "")
				on_button_detect_clicked (o, args); //open discover win
			else
				on_buttons_force_sensor_clicked (button_execute_test, new EventArgs ());

			return;
		}
		if(current_mode == Constants.Modes.RUNSENCODER)
		{
			LogB.Debug ("execute test mode: runs_encoder");
			/*
			 * runs encoder is not FTDI
			 */

			if (chronopicRegister.GetSelectedForMode (current_mode).Port == "")
				on_button_detect_clicked (o, args); //open discover win
			else
				on_runs_encoder_capture_clicked ();

			return;
		}
		if(current_mode == Constants.Modes.OTHER)
		{
			LogB.Debug ("execute test mode: other");
			/*
			 * FourPlatforms is not FTDI
			 */

			if (chronopicRegister.GetSelectedForMode (current_mode).Port == "")
				on_button_detect_clicked (o, args); //open discover win
			else
				on_four_platforms_capture_clicked (o);

			return;
		}
		if(current_mode == Constants.Modes.JUMPSSIMPLE) //jumps simple with fourPlatforms
		{
			ChronopicRegisterPort crp = chronopicRegister.GetSelectedForMode (current_mode);
			if (crp.Port != "" && crp.Type == ChronopicRegisterPort.Types.FOURPLATFORMS)
			{
				on_four_platforms_capture_clicked (o);
				return;
			}
		}

		// stop capturing inertial on the background if we start capturing a contacts test
		if(encoderThreadBG != null && encoderThreadBG.IsAlive)
		{
			stopCapturingInertialBG();
		}

		if(current_mode == Constants.Modes.RUNSINTERVALLIC && compujumpAutologout != null)
			compujumpAutologout.StartCapturingRunInterval();

		//WICHRO
		if ( chronopicRegister.NumConnectedOfType(ChronopicRegisterPort.Types.RUN_WIRELESS) == 1 &&
				(current_mode == Constants.Modes.RUNSSIMPLE || current_mode == Constants.Modes.RUNSINTERVALLIC) )
		{
			//cp2016.StoredCanCaptureContacts = true;
			cp2016.StoredWireless = true;

			on_button_execute_test_acceptedPre_start_camera(WebcamStartedTestStart.CHRONOPIC);
			return;
		} else
			cp2016.StoredWireless = false;

		if (current_mode == Constants.Modes.JUMPSSIMPLE || current_mode == Constants.Modes.JUMPSREACTIVE ||
				current_mode == Constants.Modes.RUNSSIMPLE || current_mode == Constants.Modes.RUNSINTERVALLIC)
		{
			// non-wichro 2.2.2
			chronopicRegister.ListSelectedForAllModes (); //debug

			//on networks as device detect is not used, mark one compatible as selected
			if (configChronojump.Compujump)
			{
				chronopicRegisterUpdate (false);
				chronopicRegister.SetAnyCompatibleConnectedAsSelected (current_mode);
			}

			if (chronopicRegister.GetSelectedForMode (current_mode).Port == "")
			{
				// simulated test can be done on SIMULATED session
				if(currentSession.Name == Constants.SessionSimulatedName)
					on_button_execute_test_acceptedPre_start_camera (WebcamStartedTestStart.CHRONOPIC);
				else
				{
					if (configChronojump.Compujump)
						new DialogMessage (Constants.MessageTypes.WARNING,
								Catalog.GetString("Device not found"));
					else
						on_button_detect_clicked (o, args); //open discover win
				}
			} else {
				LogB.Information ("getSelectedFormode: " + chronopicRegister.GetSelectedForMode (current_mode).ToString ());
				chronopicConnectionSequenceInit (chronopicRegister.GetSelectedForMode (current_mode));
			}

			/* before 2.2.2
			//Done before the Wichro capture.
			//If we want to use this before Wichro capture, then we will need to call first Arduino.Disconnect.
			chronopicRegisterUpdate(false);

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

			 */
			/*
			 * if serial port gets opened, then a new USB connection will use different ttyUSB on Linux
			 * and maybe is the cause for blocking the port on OSX
			 * close the port if opened
			 */
			/*
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
			 */
		}

	        UtilGtk.DeviceColors(viewport_chronopics, true);
	}

	private void showHideBlinkIcon (BlinkImage bi, bool show)
	{
		if (bi == null)
			return;

		if (bi.Status == Blink.StatusEnum.RUNNING && bi.IsOn)
		{
			bi.ImageOn.Visible = true;
			bi.ImageOff.Visible = false;
		} else {
			bi.ImageOn.Visible = false;
			bi.ImageOff.Visible = true;
		}
	}

	// camera stuff if needed
	// true is chronopic
	// false is arduino like force sensor or run encoder
	public enum WebcamStartedTestStart { CHRONOPIC, FORCESENSOR, RUNENCODER};

	private void on_button_execute_test_acceptedPre_start_camera(WebcamStartedTestStart wsts)
	{
		LogB.Information("on_button_execute_test_acceptedPre_start_camera " + wsts.ToString());
		button_video_play_this_test.Sensitive = false;

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
			on_simple_jump_activate(canCaptureC);
		}
		else if(current_mode == Constants.Modes.JUMPSREACTIVE)
		{
			on_rj_activate(canCaptureC);
		}
		else if(current_mode == Constants.Modes.RUNSSIMPLE)
		{
			extra_window_runs_distance = Convert.ToDouble(label_runs_simple_track_distance_value.Text);

			on_simple_run_activate(canCaptureC, wireless);
		}
		else if(current_mode == Constants.Modes.RUNSINTERVALLIC)
		{
			//RSA runs cannot be simulated because it's complicated to manage the countdown event...
			if(currentRunIntervalType.IsRSA && ! canCaptureC && ! wireless) {
				new DialogMessage(Constants.MessageTypes.WARNING, 
						Catalog.GetString("Sorry, RSA tests cannot be simulated."));
				return;
			}

			//extra_window_runs_interval_distance = (double) extra_window_runs_interval_spinbutton_distance.Value;
			extra_window_runs_interval_distance = Convert.ToDouble(label_runs_interval_track_distance_value.Text);
			extra_window_runs_interval_limit_tracks = extra_window_runs_interval_spinbutton_limit_tracks.Value;
			extra_window_runs_interval_limit_time = extra_window_runs_interval_spinbutton_limit_time.Value;
			
			on_run_interval_activate(canCaptureC, wireless);
		}

		if (dialogResult != null && dialogResult.Visible)
			dialog_result_set_labels ();
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

		Gtk.Button b = o as Gtk.Button;
		if (o == null) {
			LogB.Information("o is null");
			return;
		}

		if (Constants.ModeIsFORCESENSOR (current_mode) || current_mode == Constants.Modes.RUNSENCODER)
		{
			//immediately change the radios
			if (b == button_signal_analyze_load_ab && ! radio_ai_ab.Active)
				radio_ai_ab.Click ();
			if (b == button_signal_analyze_load_cd && ! radio_ai_cd.Active)
				radio_ai_cd.Click ();
		}

		if (Constants.ModeIsFORCESENSOR (current_mode))
			force_sensor_load (b == button_signal_analyze_load_cd); //allows to choose person and session
		else if(current_mode == Constants.Modes.RUNSENCODER)
			run_encoder_load (b == button_signal_analyze_load_cd); //allows to choose person and session
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
		else if (Constants.ModeIsFORCESENSOR (m))
			return Constants.BellModes.FORCESENSOR;
		else if(m == Constants.Modes.RUNSENCODER)
			return Constants.BellModes.RUNSENCODER;

		//default to JUMPSREACTIVE
		return Constants.BellModes.JUMPS;
	}

	private void on_button_contacts_bells_clicked (object o, EventArgs args)
	{
		Constants.Modes m = current_mode;
		if(m != Constants.Modes.JUMPSREACTIVE &&
				m != Constants.Modes.RUNSINTERVALLIC &&
				! Constants.ModeIsFORCESENSOR (m) &&
				m != Constants.Modes.RUNSENCODER)
			return;

		feedbackWin.View(getBellMode(m), preferences, encoderRhythm, true);
	}

	private void changeTestImage(string eventTypeString, string eventName, string fileNameString)
	{
		Pixbuf pixbuf; //main image
		Pixbuf pixbufZoom; //icon of zoom image (if shown can have two different images)

		switch (fileNameString) {
			case "LOGO":
				pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + Constants.FileNameLogo2Col);
				button_image_test_zoom.Hide();
			break;
			case "RUNSENCODER":
				//pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(true) + Constants.FileNameRunEncoder);
				pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(true) + "no_image.png");
				button_image_test_zoom.Hide();
			break;
			case "":
				pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(true) + "no_image.png");
				button_image_test_zoom.Hide();
			break;
			default:
				pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(true) + fileNameString);

				//button image test zoom will have a different image depending on if there's text
				//future: change tooltip also
				if(eventTypeString != "" && eventName != "" && eventTypeHasLongDescription (eventTypeString, eventName))
					pixbufZoom = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + Constants.FileNameZoomInWithTextIcon);
				else 
					pixbufZoom = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + Constants.FileNameZoomInIcon);

				image_test_zoom.Pixbuf = pixbufZoom;
				button_image_test_zoom.Show();
			break;
		}
		image_test.Pixbuf = pixbuf;
	}
	//2023 exercise images are now on multimedia folder, and can be added, edited, ...
	private void changeTestImage (int exerciseID)
	{
		ExerciseImage ei = new ExerciseImage (current_mode, exerciseID);
		if (ei.GetUrlIfExists (true) == "")
		{
			image_test.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(true) + "no_image.png");
			button_image_test_zoom.Hide ();
		} else
		{
			image_test.Pixbuf = UtilGtk.OpenPixbufSafe (ei.GetUrlIfExists (true), image_test.Pixbuf);

			image_test_zoom.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + Constants.FileNameZoomInIcon);
			button_image_test_zoom.Show ();
		}
	}

	private bool eventTypeHasLongDescription (string eventTypeString, string eventName) {
		if(eventTypeString != "" && eventName != "")
		{
			EventType myType = new EventType ();

			if(eventTypeString == EventType.Types.JUMP.ToString()) 
				myType = new JumpType(eventName);
			else if (eventTypeString == EventType.Types.RUN.ToString()) 
				myType = new RunType(eventName);
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
	private void on_simple_jump_activate (bool canCaptureC)
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
		if (currentJumpType.HasFall (configChronojump.Compujump))
		{
			if(extra_window_jumps_check_dj_fall_calculate.Active) {
				myFall = -1;
				ljstp.fallmm = -1;
			} else {
				myFall = (double) extra_window_jumps_spinbutton_fall.Value;
				ljstp.fallmm = Convert.ToInt32(myFall * 10);
			}
		}

		//to store how this test is for future jumps (do)
		if(currentJumpType.HasWeight || currentJumpType.HasFall (configChronojump.Compujump))
			SqliteJumpType.LastJumpSimpleTypeParamsInsertOrUpdate (ljstp);

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
			progressbarLimit = 2; //2 for simple jump (or take off)
			
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

		currentEventExecute = new JumpExecute (
				currentPerson.UniqueID, currentPerson.Name, currentPersonSession.Weight,
				currentSession.UniqueID, currentJumpType.UniqueID, currentJumpType.Name, myFall, jumpWeight,
				cp2016.CP, preferences.digitsNumber,
				preferences.volumeOn, preferences.gstreamer,
				progressbarLimit, egd, description,
				//configChronojump.Exhibition,
				//preferences.heightPreferred,
				preferences.metersSecondsPreferred,
				Convert.ToInt32(spin_resultsSession_limit.Value),
				radio_contacts_graph_allTests.Active, radio_contacts_results_personAll.Active,
				image_jump_execute_air, image_jump_execute_land,
				(configChronojump.Compujump && check_contacts_networks_upload.Active),
				configChronojump.CompujumpStationID, configChronojump.CompujumpDjango,
				webcamStatusEnumSetStart (),
				configChronojump.JsonUploadNeedsButton,
				configChronojump.JsonUploadJumpSimpleTestScript
				);



		//UtilGtk.ChronopicColors(viewport_chronopics, label_chronopics, label_connected_chronopics, chronopicWin.Connected);

		if (! canCaptureC)
			currentEventExecute.SimulateInitValues(rand);

		contactsShowCaptureDoingButtons(true);

		bool managedOk = true;
		if (currentJumpType.StartIn)
			managedOk = currentEventExecute.Manage();
		else 
			managedOk = currentEventExecute.ManageFall();

		if (! managedOk) {
			if (currentEventExecute.ChronopicDisconnected)
			{
				chronopicDisconnectedWhileExecuting ();
				contactsShowCaptureDoingButtons (false);
				on_test_jumpSM_runSI_finished_can_touch_gtk (new object (), new EventArgs ());
			}
			return;
		}

		thisJumpIsSimple = true; //used by: on_event_execute_update_graph_in_progress_clicked
		currentEventExecute.FakeButtonUpdateGraph.Clicked += 
			new EventHandler(on_event_execute_update_graph_in_progress_clicked);

		currentEventExecute.FakeButtonCameraStopIfNeeded.Clicked -= new EventHandler (on_test_finished_stop_camera_if_needed);
		currentEventExecute.FakeButtonCameraStopIfNeeded.Clicked += new EventHandler (on_test_finished_stop_camera_if_needed);

		currentEventExecute.FakeButtonThreadDyed.Clicked += new EventHandler(on_test_jumpSM_runSI_finished_can_touch_gtk);
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
					grid_extra_window_jumps_single_leg_radios.Sensitive = false;
					//but show the input cm
					notebook_contacts_capture_doing_wait.CurrentPage = 2;
				}
				SqliteJump.UpdateDescription(Constants.JumpTable, 
						currentJump.UniqueID, currentJump.Description);
			}

			if(createdStatsWin) {
				showUpdateStatsAndHideData(true);
			}
		
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

		string savedVideoStr = "";
		if (webcamStatusEnum == WebcamStatusEnum.STOPPED)
			savedVideoStr = EventEndedSaveVideoFile (Constants.TestTypes.JUMP, currentJump.UniqueID);

		if ( ! currentEventExecute.Cancel )
		{
			treeViewResultsSession.PersonWeight = currentPersonSession.Weight;
			treeViewResultsSession.Add (currentPerson.UniqueID, currentPerson.Name, currentJump, savedVideoStr);

			if (dialogResult != null)
				dialogResult.UpdateLabelResult (Util.TrimDecimals (currentJump.Height, 2));
		}

		//2.2.1 Cairo graph is not updated if window is not resized, so force update
		//since 2.2.2 graph is not updated at test end by write. is updated here to not have to readers on separated threads
		updateGraphJumpsSimple();

		//since 0.7.4.1 when test is done, treeview select it. action event button have to be shown
		//this has to be after webcamRecordEnd in order to see if video is created
		showHideActionEventButtons(true); //show

		if (remoteTest != null && configChronojump.RemoteTestJumpSimpleFile != "")
			remoteTest.Captured (configChronojump.RemoteTestJumpSimpleFile);
	}

	private void chronopicDisconnectedWhileExecuting() {
		LogB.Error("DISCONNECTED gui/cj");
		//createChronopicWindow(true, "");
		//chronopicWin.Connected = false;

		button_detect_show_hide (true);
	}

	private void on_test_finished_stop_camera_if_needed (object o, EventArgs args)
	{
		 if (webcamStatusEnum == WebcamStatusEnum.RECORDING)
		 {
			 webcamEndingRecordingStop (); //stop as soon as possible to sync test with the end of video
			 Thread.Sleep (50); //Wait
		 }
		 else if (webcamStatusEnum == WebcamStatusEnum.STOPPING)
		 {
			 webcamEndingRecordingStopDo ();
		 }
		 /* this and save the file will be done after when we know the uniqueID
		 else if (webcamStatusEnum == WebcamStatusEnum.STOPPED)
		 {
			 currentEventExecute.CameraRecording = false;
			 webcamRestoreGui (success);
		 }
		 */
		 else if (webcamStatusEnum == WebcamStatusEnum.STOPPED)
		 {
			 currentEventExecute.CameraRecording = false;
		 }
	}

	// this is called at end of jump simple/multiple and run simple/intervallic
	private void on_test_jumpSM_runSI_finished_can_touch_gtk (object o, EventArgs args)
	{
		currentEventExecute.FakeButtonThreadDyed.Clicked -= new EventHandler(on_test_jumpSM_runSI_finished_can_touch_gtk);
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
		}

		LogB.Information(" cantouch1 ");

		//if webcam started then do not call sensitiveGuiEventDone(), because will be called at webcamEnd (that has a delay)
		if(! execute_auto_doing && ! webcamManage.ReallyStarted)
			sensitiveGuiEventDone();

		LogB.Information(" cantouch3 ");

		updatePersonTestsN (false);

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
			return true; //will be called again to not do nothing but it will work again when we are out of beepTest (and updatingRestTimes is true

		//Compujump manage autologout
		if( currentPerson != null && configChronojump.Compujump && compujumpAutologout != null)
		{
			if(compujumpAutologout.ShouldILogoutNow())
					//restTime.CompujumpPersonNeedLogout(currentPerson.UniqueID), 		     //3' since last executed test
			{
				compujumpPersonLogoutDo();
				label_logout_seconds.Text = "";
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
			}
		}

		if( ! configChronojump.PersonWinHide)
		{
			myTreeViewPersons.UpdateRestTimes(restTime);
			return true;
		}

		updateTopRestTimes ();

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
		if( currentJumpRjType.HasFall (configChronojump.Compujump) || currentJumpRjType.Name == Constants.RunAnalysisName)
		{
			myFall = (double) extra_window_jumps_rj_spinbutton_fall.Value;
			ljrtp.fallmm = Convert.ToInt32(myFall * 10);
		}

		//to store how this test is for future jumps (do)
		if( (! currentJumpRjType.Unlimited && currentJumpRjType.FixedValue == 0) ||
				currentJumpType.HasWeight || currentJumpType.HasFall (configChronojump.Compujump) )
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

		currentEventExecute = new JumpRjExecute(
				currentPerson.UniqueID, currentPerson.Name, currentPersonSession.Weight,
				currentSession.UniqueID, currentJumpRjType.UniqueID, currentJumpRjType.Name,
				myFall, jumpWeight,
				progressbarLimit, currentJumpRjType.JumpsLimited, 
				cp2016.CP, preferences.digitsNumber,
				checkbutton_allow_finish_rj_after_time.Active,
				preferences.volumeOn, preferences.gstreamer,
				preferences.metersSecondsPreferred,
				feedbackJumpsRj, progressbarLimit, egd,
				image_jump_execute_air, image_jump_execute_land,
				(configChronojump.Compujump && check_contacts_networks_upload.Active),
				configChronojump.CompujumpStationID, configChronojump.CompujumpDjango,
				webcamStatusEnumSetStart ());
		
		//suitable for limited by jump and time
		//simulated always simulate limited by jumps
		if(! canCaptureC)
			currentEventExecute.SimulateInitValues(rand);
		
		contactsShowCaptureDoingButtons(true);
		if (! currentEventExecute.Manage())
		{
			if (currentEventExecute.ChronopicDisconnected)
			{
				chronopicDisconnectedWhileExecuting ();
				contactsShowCaptureDoingButtons (false);
				on_test_jumpSM_runSI_finished_can_touch_gtk (new object (), new EventArgs ());
			}
			return;
		}

		thisJumpIsSimple = false; //used by: on_event_execute_update_graph_in_progress_clicked
		currentEventExecute.FakeButtonUpdateGraph.Clicked += 
			new EventHandler(on_event_execute_update_graph_in_progress_clicked);

		currentEventExecute.FakeButtonCameraStopIfNeeded.Clicked -= new EventHandler (on_test_finished_stop_camera_if_needed);
		currentEventExecute.FakeButtonCameraStopIfNeeded.Clicked += new EventHandler (on_test_finished_stop_camera_if_needed);

		currentEventExecute.FakeButtonThreadDyed.Clicked += new EventHandler(on_test_jumpSM_runSI_finished_can_touch_gtk);
	}
		
	private void on_jump_rj_finished ()
	{
		LogB.Information("ON JUMP RJ FINISHED");
		
		//test can be deleted if not cancelled
		sensitiveLastTestButtons(! currentEventExecute.Cancel);

		if ( ! currentEventExecute.Cancel )
		{
			currentJumpRj = (JumpRj) currentEventExecute.EventDone;
			selectedJumpRj = currentJumpRj;

			//if user clicked in finish earlier
			if(currentEventExecute.Finish) {
				currentJumpRj.Jumps = Util.GetNumberOfJumps(currentJumpRj.TvString, false);
				if(currentJumpRjType.JumpsLimited) {
					currentJumpRj.Limited = currentJumpRj.Jumps.ToString() + "J";
				} else {
					currentJumpRj.Limited = Util.GetTotalTime(
							currentJumpRj.TcString, currentJumpRj.TvString) + "T";
				}
			}

			
			if(createdStatsWin) {
				showUpdateStatsAndHideData(true);
			}

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
		SqliteTests.DeleteTempEvents("tempJumpRj");

		string savedVideoStr = "";
		if (webcamStatusEnum == WebcamStatusEnum.STOPPED)
			savedVideoStr = EventEndedSaveVideoFile (Constants.TestTypes.JUMP_RJ, currentJumpRj.UniqueID);

		if ( ! currentEventExecute.Cancel ) {
			treeViewResultsSession.PersonWeight = currentPersonSession.Weight;
			treeViewResultsSession.Add (currentPerson.UniqueID, currentPerson.Name, currentJumpRj, savedVideoStr);
		}

		//Cairo graph is not updated if window is not resized, so force update
		updateGraphJumpsReactive();

		//since 0.7.4.1 when test is done, treeview select it. action event button have to be shown
		//this has to be after webcamRecordEnd in order to see if video is created
		showHideActionEventButtons(true); //show
	}

	/* ---------------------------------------------------------
	 * ----------------  RUNS EXECUTION (no interval) ----------
	 *  --------------------------------------------------------
	 */

	//suitable for all runs not repetitive
	private void on_simple_run_activate (bool canCaptureC, bool wireless)
	{
		LogB.Information("on_simple_run_activate");
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
		button_inspect_last_test_run_simple.Sensitive = false;

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

		if(wichroCapture == null || wichroCapture.PortName != wirelessPort)
			wichroCapture = new WichroCapture (wirelessPort);

		currentEventExecute = new RunExecute(
				currentPerson.UniqueID, currentSession.UniqueID, 
				currentRunType.Name, myDistance, 
				cp2016.CP, wichroCapture, wirelessPort, wirelessBauds,
				preferences.digitsNumber, preferences.metersSecondsPreferred,
				preferences.volumeOn, preferences.gstreamer,
				progressbarLimit, egd,
				preferences.runDoubleContactsMode,
				preferences.runDoubleContactsMS,
				preferences.runSpeedStartArrival,
				check_run_simple_with_reaction_time.Active,
				image_run_execute_running,
				image_run_execute_photocell_icon,
				label_run_execute_photocell_code,
				Convert.ToInt32(spin_resultsSession_limit.Value),
				radio_contacts_graph_allTests.Active, radio_contacts_results_personAll.Active,
				webcamStatusEnumSetStart (),
				configChronojump.WichroSensorOnceA, configChronojump.WichroSensorOnceB,
				configChronojump.JsonUploadNeedsButton,
				configChronojump.JsonUploadRunSimpleTestScript,
				configChronojump.JsonUploadRunSimpleRankingScript
				);

		if (! canCaptureC && ! wireless)
			currentEventExecute.SimulateInitValues(rand);

		contactsShowCaptureDoingButtons(true);
		if (! currentEventExecute.Manage())
		{
			if (currentEventExecute.ChronopicDisconnected)
			{
				chronopicDisconnectedWhileExecuting ();
				contactsShowCaptureDoingButtons (false);
				on_test_jumpSM_runSI_finished_can_touch_gtk (new object (), new EventArgs ());
			}
			return;
		}

		if(wireless && currentEventExecute.ChronopicDisconnected)
			button_detect_show_hide (true);

		thisRunIsSimple = true; //used by: on_event_execute_update_graph_in_progress_clicked
		currentEventExecute.FakeButtonUpdateGraph.Clicked += 
			new EventHandler(on_event_execute_update_graph_in_progress_clicked);

		currentEventExecute.FakeButtonCameraStopIfNeeded.Clicked -= new EventHandler (on_test_finished_stop_camera_if_needed);
		currentEventExecute.FakeButtonCameraStopIfNeeded.Clicked += new EventHandler (on_test_finished_stop_camera_if_needed);

		currentEventExecute.FakeButtonThreadDyed.Clicked += new EventHandler(on_test_jumpSM_runSI_finished_can_touch_gtk);
	}
	
	private void on_run_finished ()
	{
		//test can be deleted if not cancelled
		sensitiveLastTestButtons(! currentEventExecute.Cancel);
		button_inspect_last_test_run_simple.Sensitive = ! currentEventExecute.Cancel;

		if ( ! currentEventExecute.Cancel )
			currentRun = (Run) currentEventExecute.EventDone;

		string savedVideoStr = "";
		if (webcamStatusEnum == WebcamStatusEnum.STOPPED)
			savedVideoStr = EventEndedSaveVideoFile (Constants.TestTypes.RUN, currentRun.UniqueID);

		if ( ! currentEventExecute.Cancel )
		{
			currentRun.MetersSecondsPreferred = preferences.metersSecondsPreferred;

			treeViewResultsSession.Add (currentPerson.UniqueID, currentPerson.Name, currentRun, savedVideoStr);

			//since 0.7.4.1 when test is done, treeview select it. action event button have to be shown 
			//this has to be after webcamRecordEnd in order to see if video is created
			showHideActionEventButtons(true); //show

			if(createdStatsWin) {
				showUpdateStatsAndHideData(true);
			}

			//unhide buttons for delete last jump
			sensitiveGuiYesEvent();

			//put correct time value in eventWindow (put the time from chronopic and not onTimer soft chronometer)
			event_execute_LabelTimeValue = currentRun.Time;

			if (dialogResult != null)
				dialogResult.UpdateLabelResult (Util.TrimDecimals (currentRun.Speed, 2));

			if(configChronojump.Exhibition && configChronojump.ExhibitionStationType == ExhibitionTest.testTypes.RUN)
				SqliteJson.UploadExhibitionTest(getExhibitionTestFromGui(ExhibitionTest.testTypes.RUN, currentRun.Time));
		}
		else if( currentEventExecute.ChronopicDisconnected )
			chronopicDisconnectedWhileExecuting();

		//2.2.1 Cairo graph is not updated if window is not resized, so force update
		//since 2.2.2 graph is not updated at test end by write. is updated here to not have to readers on separated threads
		updateGraphRunsSimple();

		if (webcamStatusEnum == WebcamStatusEnum.STOPPED)
		{
			bool saved = webcamEndingSaveFile (Constants.TestTypes.RUN, currentRun.UniqueID);
			webcamRestoreGui (saved);
		}
	}


	// contacts tests insert, upload -------------->

	//user clicks on upload after doing a test (only visible if config.JsonUploadNeedsButton)
	private void on_button_contacts_json_upload_clicked (object o, EventArgs args)
	{
		if (currentEventExecute == null)
			return;

		if (current_mode == Constants.Modes.JUMPSSIMPLE)
		{
			((RunExecute) currentEventExecute).JsonUploadTestScriptDo ();
		}
		else if (current_mode == Constants.Modes.RUNSSIMPLE && currentEventExecute.GetType () == typeof(RunExecute))
		{
			((RunExecute) currentEventExecute).JsonUploadTestScriptDo ();
			((RunExecute) currentEventExecute).JsonUploadRankingScriptDo ();
		}
		else if (current_mode == Constants.Modes.RUNSINTERVALLIC && currentEventExecute.GetType () == typeof(RunIntervalExecute))
		{
			((RunIntervalExecute) currentEventExecute).JsonUploadTestScriptDo ();
			((RunIntervalExecute) currentEventExecute).JsonUploadRankingScriptDo ();
		}
	}

	private bool signalNoFollowContactsInsertRuns;
	private void on_contacts_insert_test_spin_total_time_value_changed (object o, EventArgs args)
	{
		if (signalNoFollowContactsInsertRuns)
			return;

		if (! Util.IsNumber (label_runs_simple_track_distance_value.Text, true))
			return;

		signalNoFollowContactsInsertRuns = true;
		contacts_insert_test_spin_speed_km_h.Value =
			3.6 * Convert.ToDouble (label_runs_simple_track_distance_value.Text) / contacts_insert_test_spin_total_time.Value;
		signalNoFollowContactsInsertRuns = false;
	}
	private void on_contacts_insert_test_spin_speed_km_h_value_changed (object o, EventArgs args)
	{
		if (signalNoFollowContactsInsertRuns)
			return;

		if (! Util.IsNumber (label_runs_simple_track_distance_value.Text, true))
			return;

		signalNoFollowContactsInsertRuns = true;
		contacts_insert_test_spin_total_time.Value =
			3.6 * Convert.ToDouble (label_runs_simple_track_distance_value.Text) / contacts_insert_test_spin_speed_km_h.Value;
		signalNoFollowContactsInsertRuns = false;
	}

	//user insert a test (only visible if config.CanIsertTests)
	private void on_contacts_insert_test_button_insert_clicked (object o, EventArgs args)
	{
		contacts_insert_test_button_insert_do (false);
	}
	private void on_contacts_insert_test_button_insert_and_upload_clicked (object o, EventArgs args)
	{
		contacts_insert_test_button_insert_do (true);
	}
	private void contacts_insert_test_button_insert_do (bool upload)
	{
		double distance = 0;
		if (current_mode == Constants.Modes.RUNSSIMPLE &&
				contacts_insert_test_spin_total_time.Value > 0)
		{
			if (Util.IsNumber (label_runs_simple_track_distance_value.Text, true))
				distance = Convert.ToDouble (label_runs_simple_track_distance_value.Text);
			else
				return;

			currentEventExecute = new RunExecute (
					currentPerson.UniqueID, currentSession.UniqueID,
					comboSelectRuns.GetSelectedNameEnglish(), distance,
					contacts_insert_test_spin_total_time.Value,
					configChronojump.JsonUploadRunSimpleTestScript,
					configChronojump.JsonUploadRunSimpleRankingScript
					);

			treeViewResultsSession.Add (currentPerson.UniqueID, currentPerson.Name, (Run) currentEventExecute.EventDone, "");
			updateGraphRunsSimple();
		}
		else if (current_mode == Constants.Modes.RUNSINTERVALLIC &&
				contacts_insert_test_spin_track_1_time.Value > 0 &&
				contacts_insert_test_spin_track_2_time.Value > 0)
		{
			if (Util.IsNumber (label_runs_interval_track_distance_value.Text, true))
				distance = Convert.ToDouble (label_runs_interval_track_distance_value.Text);
			else
				return;

			currentEventExecute = new RunIntervalExecute (
					currentPerson.UniqueID, currentSession.UniqueID,
					comboSelectRunsI.GetSelectedNameEnglish(), distance,
					contacts_insert_test_spin_track_1_time.Value,
					contacts_insert_test_spin_track_2_time.Value,
					configChronojump.JsonUploadRunIntervalTestScript,
					configChronojump.JsonUploadRunIntervalRankingScript
					);

			/*
			 * note there are other transformation used on_run_interval_finished that maybe there are not needed here as they are already done on currentEventExecute creation
			 * like: MetersSecondsPreferred, Tracks, Limited
			 */
			treeViewResultsSession.Add (currentPerson.UniqueID, currentPerson.Name, (RunInterval) currentEventExecute.EventDone, "");
			updateGraphRunsInterval();
		}

		//user uploads after inserting a test (only visible if config.CanIsertTests)
		if (upload)
			on_button_contacts_json_upload_clicked (new object (), new EventArgs ());
	}

	// <---------------- contacts tests insert, upload


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
				if (currentRunIntervalType.TracksLimited)
					progressbarLimit = extra_window_runs_interval_limit_tracks;
				else
					progressbarLimit = extra_window_runs_interval_limit_time;
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

		if(wichroCapture == null || wichroCapture.PortName != wirelessPort)
			wichroCapture = new WichroCapture (wirelessPort);

		currentEventExecute = new RunIntervalExecute(
				currentPerson.UniqueID, currentSession.UniqueID, currentRunIntervalType.Name, 
				distanceInterval, progressbarLimit, currentRunIntervalType.TracksLimited, 
				cp2016.CP, wichroCapture, wirelessPort, wirelessBauds,
				preferences.digitsNumber, preferences.metersSecondsPreferred,
				preferences.volumeOn, preferences.gstreamer,
				feedbackRunsI,
				progressbarLimit, egd,
				preferences.runIDoubleContactsMode,
				preferences.runIDoubleContactsMS,
				preferences.runSpeedStartArrival,
				check_run_interval_with_reaction_time.Active,
				image_run_execute_running,
				image_run_execute_photocell_icon,
				label_run_execute_photocell_code,
				webcamStatusEnumSetStart (),
				configChronojump.WichroSensorOnceA, configChronojump.WichroSensorOnceB,
				configChronojump.JsonUploadNeedsButton,
				configChronojump.JsonUploadRunIntervalTestScript,
				configChronojump.JsonUploadRunIntervalRankingScript
				);

		//suitable for limited by tracks and time
		if(! canCaptureC && ! wireless)
			currentEventExecute.SimulateInitValues(rand);

		contactsShowCaptureDoingButtons(true);
		if (! currentEventExecute.Manage())
		{
			if (currentEventExecute.ChronopicDisconnected)
			{
				chronopicDisconnectedWhileExecuting ();
				contactsShowCaptureDoingButtons (false);
				on_test_jumpSM_runSI_finished_can_touch_gtk (new object (), new EventArgs ());
			}
			return;
		}

		if(wireless && currentEventExecute.ChronopicDisconnected)
			button_detect_show_hide (true);

		thisRunIsSimple = false; //used by: on_event_execute_update_graph_in_progress_clicked
		currentEventExecute.FakeButtonUpdateGraph.Clicked += 
			new EventHandler(on_event_execute_update_graph_in_progress_clicked);

		currentEventExecute.FakeButtonCameraStopIfNeeded.Clicked -= new EventHandler (on_test_finished_stop_camera_if_needed);
		currentEventExecute.FakeButtonCameraStopIfNeeded.Clicked += new EventHandler (on_test_finished_stop_camera_if_needed);

		currentEventExecute.FakeButtonThreadDyed.Clicked += new EventHandler(on_test_jumpSM_runSI_finished_can_touch_gtk);
	}


	private void on_run_interval_finished ()
	{
		//test can be deleted if not cancelled
		sensitiveLastTestButtons(! currentEventExecute.Cancel);
		button_inspect_last_test_run_intervallic.Sensitive = ! currentEventExecute.Cancel;

		if ( ! currentEventExecute.Cancel )
			currentRunInterval = (RunInterval) currentEventExecute.EventDone;

		string savedVideoStr = "";
		if (webcamStatusEnum == WebcamStatusEnum.STOPPED)
			savedVideoStr = EventEndedSaveVideoFile (Constants.TestTypes.RUN_I, currentRunInterval.UniqueID);

		if ( ! currentEventExecute.Cancel )
		{
			selectedRunInterval = currentRunInterval;
			selectedRunIntervalType = currentRunIntervalType;

			//fix showing 2414 at end of 3L3R capture. With this shows: 24,14
			if (selectedRunIntervalType.DistancesString != "")
				selectedRunIntervalType.DistancesString = Util.ChangeDecimalSeparator (selectedRunIntervalType.DistancesString);

			currentRunInterval.MetersSecondsPreferred = preferences.metersSecondsPreferred;

			//if user clicked in finish earlier
			if(currentEventExecute.Finish) {
				currentRunInterval.Tracks = Util.GetNumberOfJumps(currentRunInterval.IntervalTimesString, false);
				if(currentRunIntervalType.TracksLimited) {
					currentRunInterval.Limited = currentRunInterval.Tracks.ToString() + "R";
				} else {
					currentRunInterval.Limited = Util.GetTotalTime(
							currentRunInterval.IntervalTimesString) + "T";
				}
			}

			treeViewResultsSession.Add (currentPerson.UniqueID, currentPerson.Name, currentRunInterval, savedVideoStr);

			//since 0.7.4.1 when test is done, treeview select it. action event button have to be shown 
			//this has to be after webcamRecordEnd in order to see if video is created
			showHideActionEventButtons(true); //show

			if(createdStatsWin) {
				showUpdateStatsAndHideData(true);
			}

			//unhide buttons for delete last jump
			sensitiveGuiYesEvent();

			//put correct time value in eventWindow (put the time from chronopic and not onTimer soft chronometer)
			event_execute_LabelTimeValue = currentRunInterval.TimeTotal;
			//possible deletion of last run can make the runs on event window be false
			event_execute_LabelEventValue = currentRunInterval.Tracks;

			addTreeView_runs_interval_sprint (currentRunInterval, currentRunIntervalType);

			if (configChronojump.Compujump && check_contacts_networks_upload.Active)
				calculateSprintAndUpload();
		}
		else if( currentEventExecute.ChronopicDisconnected )
			chronopicDisconnectedWhileExecuting();

		if (webcamStatusEnum == WebcamStatusEnum.STOPPED)
		{
			bool saved = webcamEndingSaveFile (Constants.TestTypes.RUN_I, currentRunInterval.UniqueID);
			webcamRestoreGui (saved);
		}

		//delete the temp tables if exists
		SqliteTests.DeleteTempEvents("tempRunInterval");

		drawingarea_results_realtime.QueueDraw();

		//Cairo graph is not updated if window is not resized, so force update
		updateGraphRunsInterval();

		if(compujumpAutologout != null)
			compujumpAutologout.EndCapturingRunInterval();

		if (remoteTest != null && configChronojump.RemoteTestRunIntervalFile != "")
			remoteTest.Captured (configChronojump.RemoteTestRunIntervalFile);
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
	 * ----------------  MULTI CHRONOPIC EXECUTION -------------
	 *  --------------------------------------------------------
	 */

	private void on_chronopic_contacts_clicked (object o, EventArgs args)
	{
		/*
		   1) gui changes
		ChronopicWindow.ChronojumpMode cmode = ChronopicWindow.ChronojumpMode.JUMPORRUN;
		if(current_mode == Constants.Modes.OTHER)
			cmode = ChronopicWindow.ChronojumpMode.OTHER;

		chronopicWin = ChronopicWindow.View(cmode, preferences.volumeOn);
		//chronopicWin.FakeWindowReload.Clicked += new EventHandler(chronopicWindowReload);
		chronopicWin.FakeWindowDone.Clicked += new EventHandler(on_chronopic_window_contacts_connected_or_done);
		*/

		// 2) close ports. Close Arduino capture before calling to device
		if(wichroCapture != null && wichroCapture.PortOpened)
			wichroCapture.Disconnect();

		// 3) show window
		chronopicRegisterUpdate(true);
	}

	private void on_chronopic_encoder_clicked (object o, EventArgs args)
	{
		/*
		chronopicWin = ChronopicWindow.View(ChronopicWindow.ChronojumpMode.ENCODER, preferences.volumeOn);
		//chronopicWin.FakeWindowReload.Clicked += new EventHandler(chronopicWindowReload);
		chronopicWin.FakeWindowDone.Clicked += new EventHandler(on_chronopic_window_encoder_connected_or_done);
		*/

		// 2) close ports. Close Arduino capture before calling to device
		if(wichroCapture != null && wichroCapture.PortOpened)
			wichroCapture.Disconnect();

		// 3) show window
		chronopicRegisterUpdate(true);
	}

	private void on_button_contacts_devices_networks_problems_clicked (object o, EventArgs args)
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


	/* ---------------------------------------------------------
	 * ----------------  EVENTS EDIT ---------------------------
	 *  --------------------------------------------------------
	 */

	int eventOldPerson;

	private void on_button_tests_edit_selected_clicked (object o, EventArgs args)
	{
		if (current_mode == Constants.Modes.JUMPSSIMPLE)
			on_edit_selected_jump_clicked (o, args);
		else if (current_mode == Constants.Modes.JUMPSREACTIVE)
			on_edit_selected_jump_rj_clicked (o, args);
		else if (current_mode == Constants.Modes.RUNSSIMPLE)
			on_edit_selected_run_clicked (o, args);
		else if (current_mode == Constants.Modes.RUNSINTERVALLIC)
			on_edit_selected_run_interval_clicked (o, args);
		else if (current_mode == Constants.Modes.RUNSENCODER)
			on_edit_selected_runEncoder_clicked (o, args);
		else if (current_mode == Constants.Modes.BEEPTEST)
			on_edit_selected_beepTest_clicked (o, args);
		else if (Constants.ModeIsFORCESENSOR (current_mode))
			on_edit_selected_forceSensor_clicked (o, args);
		else if (Constants.ModeIsENCODER (current_mode))
			on_edit_selected_encoder_clicked (o, args);
		else if (current_mode == Constants.Modes.WILIGHT)
			on_edit_selected_wilight_clicked (o, args);
		else if (current_mode == Constants.Modes.OTHER)
			on_edit_selected_fourPlatforms_clicked (o, args);

		//all this are on src/gui/modes
	}


	/* ---------------------------------------------------------
	 * ----------------  EVENTS DELETE -------------------------
	 *  --------------------------------------------------------
	 */

	private void deleted_last_test_update_widgets() {
		sensitiveLastTestButtons(false);
	}

	private void on_button_tests_delete_selected_clicked (object o, EventArgs args)
	{
		//TODO: implement all using on_delete_selected_test_clicked

		if (current_mode == Constants.Modes.JUMPSSIMPLE)
			on_delete_selected_jump_clicked (o, args);
		else if (current_mode == Constants.Modes.JUMPSREACTIVE)
			on_delete_selected_jump_rj_clicked (o, args);
		else if (current_mode == Constants.Modes.RUNSSIMPLE)
			on_delete_selected_run_clicked (o, args);
		else if (current_mode == Constants.Modes.RUNSINTERVALLIC)
			on_delete_selected_run_interval_clicked (o, args);
		else if(current_mode == Constants.Modes.RUNSENCODER)
			run_encoder_delete_current_test_pre_question();
		else if (Constants.ModeIsFORCESENSOR (current_mode))
			force_sensor_delete_current_test_pre_question();
		else if (Constants.ModeIsENCODER (current_mode))
			on_button_encoder_delete_signal_clicked (o, args);
		else if (current_mode == Constants.Modes.BEEPTEST || current_mode == Constants.Modes.WILIGHT || current_mode == Constants.Modes.OTHER)
			on_delete_selected_test_clicked (o, args);
	}

	
	/* ---------------------------------------------------------
	 * ----------------  EVENTS INSPECT ------------------------
	 *  --------------------------------------------------------
	 */

	private void on_button_inspect_last_test_clicked (object o, EventArgs args)
	{
		if(currentEventExecute == null)
			return;

		//sensitivize gui
		menus_and_mode_sensitive(false);
		box_contacts_capture_top.Sensitive = false;
		//button_inspect_last_test.Sensitive = false; //unneeded because it will be on box_contacts_capture_top
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
		box_contacts_capture_top.Sensitive = true;
		//button_inspect_last_test.Sensitive = true; //unneeded because it will be on box_contacts_capture_top
		hbox_contacts_sup_capture_analyze_two_buttons.Sensitive = true;
		hbox_top_person.Sensitive = true;

		notebook_contacts_execute_or.CurrentPage = Convert.ToInt32(notebook_contacts_execute_or_pages.EXECUTE);
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
	
	private void on_jump_type_add_accepted (object o, EventArgs args)
	{
		LogB.Information("ACCEPTED Add new jump type");
		if(jumpTypeAddWin.InsertedSimple) {
			createComboSelectJumps(false);
			createComboSelectJumpsDjOptimalFall(false);
			createComboSelectJumpsWeightFVProfile(false);
			createComboSelectJumpsAsymmetry(false);
			createComboSelectJumpsEvolution(false);

			pre_fillTreeView_resultsSession ();
			combo_select_jumps.Active = UtilGtk.ComboMakeActive(combo_select_jumps, jumpTypeAddWin.Name);
			update_combo_select_contacts_top_using_combo (combo_select_jumps);

			new DialogMessage(Constants.MessageTypes.INFO, Catalog.GetString("Added simple jump type."));
		} else {
			createComboSelectJumpsRj(false);
			createComboSelectJumpsRjFatigue(false);
			//createComboSelectJumpsRjFatigueNum(false); do not need because will be updated by createComboSelectJumpsRjFatigue
			
			pre_fillTreeView_resultsSession ();
			combo_select_jumps_rj.Active = UtilGtk.ComboMakeActive(combo_select_jumps_rj, jumpTypeAddWin.Name);
			update_combo_select_contacts_top_using_combo (combo_select_jumps_rj);

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

	private void on_run_type_add_accepted (object o, EventArgs args)
	{
		LogB.Information("ACCEPTED Add new run type");
		if(runTypeAddWin.InsertedSimple) {
			createComboSelectRuns(false);
			createComboSelectRunsEvolution(false);

			pre_fillTreeView_resultsSession ();
			combo_select_runs.Active = UtilGtk.ComboMakeActive(combo_select_runs, runTypeAddWin.Name);

			update_combo_select_contacts_top_using_combo (combo_select_runs);

			new DialogMessage(Constants.MessageTypes.INFO, Catalog.GetString("Added simple race type."));
		} else {
			createComboSelectRunsInterval(false);
			
			pre_fillTreeView_resultsSession ();

			combo_select_runs_interval.Active = UtilGtk.ComboMakeActive(combo_select_runs_interval, runTypeAddWin.Name);
			update_combo_select_contacts_top_using_combo (combo_select_runs_interval);

			new DialogMessage(Constants.MessageTypes.INFO, Catalog.GetString("Added intervallic race type."));
		}
		updateComboStats();
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
		pre_fillTreeView_resultsSession ();

		extra_window_jumps_initialize(new JumpType("Free"));
	}

	private void on_deleted_jump_rj_type (object o, EventArgs args)
	{
		string translatedName = comboSelectJumpsRj.GetNameTranslated(jumpsRjMoreWin.SelectedEventName);
		combo_select_jumps_rj = comboSelectJumpsRj.DeleteValue(translatedName);
		pre_fillTreeView_resultsSession ();

		extra_window_jumps_rj_initialize(new JumpType("RJ(j)"));
	}

	private void on_deleted_run_type (object o, EventArgs args)
	{
		string translatedName = comboSelectRuns.GetNameTranslated(runsMoreWin.SelectedEventName);
		combo_select_runs = comboSelectRuns.DeleteValue(translatedName);
		pre_fillTreeView_resultsSession ();

		extra_window_runs_initialize(new RunType("Custom"));
	}

	private void on_deleted_run_i_type (object o, EventArgs args)
	{
		string translatedName = comboSelectRunsI.GetNameTranslated(runsIntervalMoreWin.SelectedEventName);
		combo_select_runs_interval = comboSelectRunsI.DeleteValue(translatedName);
		pre_fillTreeView_resultsSession ();

		extra_window_runs_interval_initialize(new RunType("byLaps"));
	}

	/* ---------------------------------------------------------
	 * ----------------  EVENTS REPAIR -------------------------
	 *  --------------------------------------------------------
	 */
	
	private void on_button_tests_repair_selected_clicked (object o, EventArgs args)
	{
		if (current_mode == Constants.Modes.JUMPSREACTIVE)
			on_repair_selected_jump_rj_clicked (o, args);
		else if (current_mode == Constants.Modes.RUNSINTERVALLIC)
			on_repair_selected_run_interval_clicked (o, args);
		//TODO, but is lot of code, try to merge 1st: RepairJumpRjWindow, RepairRunIntervalWindow
		//else if (current_mode == Constants.Modes.OTHER)
		//	on_repair_selected_fourPlatforms_clicked (o, args);
	}

	private void on_repair_selected_jump_rj_clicked (object o, EventArgs args) {
		//notebooks_change(1); see "notebooks_change sqlite problem"
		LogB.Information("Repair selected subjump");
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person (check also if it's not a individual RJ, the pass the parent RJ)
		if (treeViewResultsSession.EventSelectedID >= 0) {
			//3.- obtain the data of the selected jump
			JumpRj myJump = SqliteJumpRj.SelectJumpData( "jumpRj", treeViewResultsSession.EventSelectedID, false, false );
		
			//4.- edit this jump
			repairJumpRjWin = RepairJumpRjWindow.Show(app1, myJump, preferences.digitsNumber);
			repairJumpRjWin.Button_accept.Clicked += new EventHandler(on_repair_selected_jump_rj_accepted);
		}
	}
	
	private void on_repair_selected_jump_rj_accepted (object o, EventArgs args)
	{
		LogB.Information("Repair selected reactive jump accepted");

		int tempSelected = -1;
		if (treeViewResultsSession.EventSelectedID >= 0)
			tempSelected = treeViewResultsSession.EventSelectedID;

		selectedJumpRj = null; // after the repair need to check again jump from SQL and draw at top correctly

		pre_fillTreeView_resultsSession ();
		
		if(createdStatsWin)
			stats_win_fillTreeView_stats(false, false);

		if (tempSelected > 0)
			selectResultsSessionId (tempSelected, false);

		//update both graphs
		drawingarea_results_session.QueueDraw ();
		drawingarea_results_realtime.QueueDraw ();
	}
	
	private void on_repair_selected_run_interval_clicked (object o, EventArgs args) {
		//notebooks_change(3); see "notebooks_change sqlite problem"
		LogB.Information("Repair selected subrun");
		//1.- check that there's a line selected
		//2.- check that this line is a run and not a person 
		//(check also if it's not a individual run interval, then pass the parent run interval)
		if (treeViewResultsSession.EventSelectedID >= 0) {
			//3.- obtain the data of the selected run
			RunInterval myRun = SqliteRunInterval.SelectRunData( Constants.RunIntervalTable, treeViewResultsSession.EventSelectedID, false, false );
		
			//4.- edit this run
			repairRunIntervalWin = RepairRunIntervalWindow.Show(app1, myRun, preferences.digitsNumber);
			repairRunIntervalWin.Button_accept.Clicked += new EventHandler(on_repair_selected_run_interval_accepted);
		}
	}
	
	private void on_repair_selected_run_interval_accepted (object o, EventArgs args)
	{
		LogB.Information("repair selected run interval accepted");

		int tempSelected = -1;
		if (treeViewResultsSession.EventSelectedID >= 0)
			tempSelected = treeViewResultsSession.EventSelectedID;

		selectedRunInterval = null; // after the repair need to check again run from SQL and draw at top correctly

		pre_fillTreeView_resultsSession ();
		createTreeView_runs_interval_sprint (treeview_runs_interval_sprint);
		
		if(createdStatsWin)
			stats_win_fillTreeView_stats(false, false);

		if (tempSelected > 0)
			selectResultsSessionId (tempSelected, false);

		//update both graphs
		drawingarea_results_session.QueueDraw ();
		drawingarea_results_realtime.QueueDraw ();
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
		//LogB.Debug(new StackFrame(1).GetMethod().Name);

		LogB.Information ("notebooks_change start, currentPage: " + notebook_execute.CurrentPage.ToString());

		if(mode == Constants.Modes.JUMPSSIMPLE)
		{
			notebook_execute.CurrentPage = Convert.ToInt32 (notebook_execute_pages.JUMPSSIMPLE);
			notebook_options_top.CurrentPage = Convert.ToInt32 (notebook_options_top_pages.JUMPSSIMPLE);

			if(currentJumpType != null)
				changeTestImage(EventType.Types.JUMP.ToString(),
						currentJumpType.Name, currentJumpType.ImageFileName);
		} else if(mode == Constants.Modes.JUMPSREACTIVE)
		{
			notebook_execute.CurrentPage = Convert.ToInt32 (notebook_execute_pages.JUMPSREACTIVE);
			notebook_options_top.CurrentPage = Convert.ToInt32 (notebook_options_top_pages.JUMPSREACTIVE);

			if(currentJumpRjType != null)
				changeTestImage(EventType.Types.JUMP.ToString(),
						currentJumpRjType.Name, currentJumpRjType.ImageFileName);
		} else if(mode == Constants.Modes.RUNSSIMPLE)
		{
			notebook_execute.CurrentPage = Convert.ToInt32 (notebook_execute_pages.RUNSSIMPLE);
			notebook_options_top.CurrentPage = Convert.ToInt32 (notebook_options_top_pages.RUNSSIMPLE);

			if(currentRunType != null)
				changeTestImage(EventType.Types.RUN.ToString(),
						currentRunType.Name, currentRunType.ImageFileName);
		} else if(mode == Constants.Modes.RUNSINTERVALLIC)
		{
			notebook_execute.CurrentPage = Convert.ToInt32 (notebook_execute_pages.RUNSINTERVALLIC);
			notebook_options_top.CurrentPage = Convert.ToInt32 (notebook_options_top_pages.RUNSINTERVALLIC);

			if(currentRunIntervalType != null)
				changeTestImage(EventType.Types.RUN.ToString(),
						currentRunIntervalType.Name, currentRunIntervalType.ImageFileName);
		} else if(mode == Constants.Modes.RUNSENCODER)
		{
			notebook_execute.CurrentPage = Convert.ToInt32 (notebook_execute_pages.RUNSENCODER);
			notebook_options_top.CurrentPage = Convert.ToInt32 (notebook_options_top_pages.RUNSENCODER);
			changeTestImage("", "", "RUNSENCODER");
			event_execute_button_finish.Sensitive = false;
		} else if(mode == Constants.Modes.BEEPTEST)
		{
			/*
			notebook_execute.CurrentPage = 8; //not shown on beep test
			notebook_options_top.CurrentPage = 8;//not shown on beep test
			changeTestImage("", "", "RUNSENCODER");//not shown on beep test
			event_execute_button_finish.Sensitive = false;//not shown on beep test
			*/
		} else if (Constants.ModeIsFORCESENSOR (mode))
		{
			notebook_execute.CurrentPage = Convert.ToInt32 (notebook_execute_pages.FORCESENSOR);
			notebook_options_top.CurrentPage =
				Convert.ToInt32 (notebook_options_top_pages.FORCESENSOR); //but at FORCESENSOR this notebook is not shown until adjust button is clicked

			event_execute_button_finish.Sensitive = false;
			fullscreen_button_fullscreen_contacts.Sensitive = false;
		} else if(mode == Constants.Modes.WILIGHT)
		{
//			notebook_execute.CurrentPage = Convert.ToInt32 (notebook_execute_pages.JUMPSREACTIVE);
//			notebook_options_top.CurrentPage = Convert.ToInt32 (notebook_options_top_pages.JUMPSREACTIVE);

			/*
			if(currentJumpRjType != null)
				changeTestImage(EventType.Types.JUMP.ToString(),
						currentJumpRjType.Name, currentJumpRjType.ImageFileName);
						*/
		}

		//button_execute_test have to be non sensitive in multichronopic without two cps
		//else has to be sensitive

		//if there are persons
		button_execute_test.Sensitive = myTreeViewPersons.IsThereAnyRecord();
		button_auto_start.Sensitive = myTreeViewPersons.IsThereAnyRecord();

		LogB.Information ("notebooks_change almost end, currentPage: " + notebook_execute.CurrentPage.ToString());

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
				Pixbuf pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "image_store_has_new_products.png");
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
			// getNewsDatetime(); cancelled until definitely fixed problem with Eduroam

			/*
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
			*/
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

	private void on_preferences_debug_mode_start (object o, EventArgs args)
	{
		//first delete debug file
		Util.FileDelete(System.IO.Path.GetTempPath() + "chronojump-debug.txt");

		encoderRProcCapture.Debug = true;
		encoderRProcAnalyze.Debug = true;
		preferences.debugMode = true; //be used by force sensor, importer (can be used easily for all software)
		LogB.PrintAllThreads = true;

		LogB.Information ("\n\n***\nDEBUG active\n***\n");

		//hbox_gui_tests.Visible = true;
		//button_carles.Visible = true;

		//menuitem_check_race_encoder_capture_simulate.Visible = true;

		if (preferencesWin != null) //as on_preferences_debug_mode_start is called also on start with "debug" param
			preferencesWin.DebugActivated();

		if(currentSession == null)
			setApp1Title("", current_mode);
		else
			setApp1Title(currentSession.Name, current_mode);
	}

	private void on_preferences_delete_devices (object o, EventArgs args)
	{
		chronopicRegisterUpdate (false);
		button_detect_show_hide (true);
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

		string versionStr = progVersion;

		//DB version
		if(File.Exists(System.IO.Path.Combine(Util.GetDatabaseDir(), "chronojump.db"))) {
			try {
				Sqlite.Connect();
				versionStr += "\nDB version: " + SqlitePreferences.Select("databaseVersion", false);
				Sqlite.DisConnect();
			}
			catch {
				Console.WriteLine("Cannot check DB version, failed checking");
				Sqlite.DisConnect();
			}
		}

		new About(versionStr, translator_credits);
	}

	private void on_feedback_closed(object o, EventArgs args)
	{
		//update bell color if feedback exists
		Constants.Modes m = current_mode;
		Pixbuf pixbuf;

		Constants.BellModes bellMode = getBellMode(m);
		if(m == Constants.Modes.JUMPSREACTIVE || m == Constants.Modes.RUNSINTERVALLIC)
		{
			// Update bell
			if(feedbackWin.FeedbackActive(bellMode))
				pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "stock_bell_active.png");
			else
				pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "stock_bell_none.png");

			image_contacts_bell.Pixbuf = pixbuf;

			// 2) Update SQL and preferences object
			Sqlite.Open(); // ------>

			if(m == Constants.Modes.JUMPSREACTIVE)
			{
				// .*Active (boolean) prefs
				preferences.jumpsRjFeedbackShowBestTvTc = Preferences.PreferencesChange(
						true,
						SqlitePreferences.JumpsRjFeedbackShowBestTvTc,
						preferences.jumpsRjFeedbackShowBestTvTc,
						feedbackWin.JumpsRjFeedbackShowBestTvTc);

				preferences.jumpsRjFeedbackShowWorstTvTc = Preferences.PreferencesChange(
						true,
						SqlitePreferences.JumpsRjFeedbackShowWorstTvTc,
						preferences.jumpsRjFeedbackShowWorstTvTc,
						feedbackWin.JumpsRjFeedbackShowWorstTvTc);

				preferences.jumpsRjFeedbackHeightGreaterActive = Preferences.PreferencesChange(
						true,
						SqlitePreferences.JumpsRjFeedbackHeightGreaterActive,
						preferences.jumpsRjFeedbackHeightGreaterActive,
						feedbackWin.JumpsRjFeedbackHeightGreaterActive);

				preferences.jumpsRjFeedbackHeightLowerActive = Preferences.PreferencesChange(
						true,
						SqlitePreferences.JumpsRjFeedbackHeightLowerActive,
						preferences.jumpsRjFeedbackHeightLowerActive,
						feedbackWin.JumpsRjFeedbackHeightLowerActive);

				preferences.jumpsRjFeedbackTvGreaterActive = Preferences.PreferencesChange(
						true,
						SqlitePreferences.JumpsRjFeedbackTvGreaterActive,
						preferences.jumpsRjFeedbackTvGreaterActive,
						feedbackWin.JumpsRjFeedbackTvGreaterActive);

				preferences.jumpsRjFeedbackTvLowerActive = Preferences.PreferencesChange(
						true,
						SqlitePreferences.JumpsRjFeedbackTvLowerActive,
						preferences.jumpsRjFeedbackTvLowerActive,
						feedbackWin.JumpsRjFeedbackTvLowerActive);

				preferences.jumpsRjFeedbackTcGreaterActive = Preferences.PreferencesChange(
						true,
						SqlitePreferences.JumpsRjFeedbackTcGreaterActive,
						preferences.jumpsRjFeedbackTcGreaterActive,
						feedbackWin.JumpsRjFeedbackTcGreaterActive);

				preferences.jumpsRjFeedbackTcLowerActive = Preferences.PreferencesChange(
						true,
						SqlitePreferences.JumpsRjFeedbackTcLowerActive,
						preferences.jumpsRjFeedbackTcLowerActive,
						feedbackWin.JumpsRjFeedbackTcLowerActive);

				// (double) prefs
				preferences.jumpsRjFeedbackHeightGreater = Preferences.PreferencesChange(
						true,
						SqlitePreferences.JumpsRjFeedbackHeightGreater,
						preferences.jumpsRjFeedbackHeightGreater,
						feedbackWin.JumpsRjFeedbackHeightGreater);

				preferences.jumpsRjFeedbackHeightLower = Preferences.PreferencesChange(
						true,
						SqlitePreferences.JumpsRjFeedbackHeightLower,
						preferences.jumpsRjFeedbackHeightLower,
						feedbackWin.JumpsRjFeedbackHeightLower);

				preferences.jumpsRjFeedbackTvGreater = Preferences.PreferencesChange(
						true,
						SqlitePreferences.JumpsRjFeedbackTvGreater,
						preferences.jumpsRjFeedbackTvGreater,
						feedbackWin.JumpsRjFeedbackTvGreater);

				preferences.jumpsRjFeedbackTvLower = Preferences.PreferencesChange(
						true,
						SqlitePreferences.JumpsRjFeedbackTvLower,
						preferences.jumpsRjFeedbackTvLower,
						feedbackWin.JumpsRjFeedbackTvLower);

				preferences.jumpsRjFeedbackTcGreater = Preferences.PreferencesChange(
						true,
						SqlitePreferences.JumpsRjFeedbackTcGreater,
						preferences.jumpsRjFeedbackTcGreater,
						feedbackWin.JumpsRjFeedbackTcGreater);

				preferences.jumpsRjFeedbackTcLower = Preferences.PreferencesChange(
						true,
						SqlitePreferences.JumpsRjFeedbackTcLower,
						preferences.jumpsRjFeedbackTcLower,
						feedbackWin.JumpsRjFeedbackTcLower);
			}
			else if(m == Constants.Modes.RUNSINTERVALLIC)
			{
				// .*Active (boolean) prefs
				preferences.runsIFeedbackShowBestSpeed = Preferences.PreferencesChange(
						true,
						SqlitePreferences.RunsIFeedbackShowBestSpeed,
						preferences.runsIFeedbackShowBestSpeed,
						feedbackWin.RunsIFeedbackSpeedBestActive);

				preferences.runsIFeedbackShowWorstSpeed = Preferences.PreferencesChange(
						true,
						SqlitePreferences.RunsIFeedbackShowWorstSpeed,
						preferences.runsIFeedbackShowWorstSpeed,
						feedbackWin.RunsIFeedbackSpeedWorstActive);

				preferences.runsIFeedbackShowBest = Preferences.PreferencesChange(
						true,
						SqlitePreferences.RunsIFeedbackShowBest,
						preferences.runsIFeedbackShowBest,
						feedbackWin.RunsIFeedbackTimeBestActive);

				preferences.runsIFeedbackShowWorst = Preferences.PreferencesChange(
						true,
						SqlitePreferences.RunsIFeedbackShowWorst,
						preferences.runsIFeedbackShowWorst,
						feedbackWin.RunsIFeedbackTimeWorstActive);

				preferences.runsIFeedbackSpeedGreaterActive = Preferences.PreferencesChange(
						true,
						SqlitePreferences.RunsIFeedbackSpeedGreaterActive,
						preferences.runsIFeedbackSpeedGreaterActive,
						feedbackWin.RunsIFeedbackSpeedGreaterActive);

				preferences.runsIFeedbackSpeedLowerActive = Preferences.PreferencesChange(
						true,
						SqlitePreferences.RunsIFeedbackSpeedLowerActive,
						preferences.runsIFeedbackSpeedLowerActive,
						feedbackWin.RunsIFeedbackSpeedLowerActive);

				preferences.runsIFeedbackTimeGreaterActive = Preferences.PreferencesChange(
						true,
						SqlitePreferences.RunsIFeedbackTimeGreaterActive,
						preferences.runsIFeedbackTimeGreaterActive,
						feedbackWin.RunsIFeedbackTimeGreaterActive);

				preferences.runsIFeedbackTimeLowerActive = Preferences.PreferencesChange(
						true,
						SqlitePreferences.RunsIFeedbackTimeLowerActive,
						preferences.runsIFeedbackTimeLowerActive,
						feedbackWin.RunsIFeedbackTimeLowerActive);

				// (double) prefs
				preferences.runsIFeedbackSpeedGreater = Preferences.PreferencesChange(
						true,
						SqlitePreferences.RunsIFeedbackSpeedGreater,
						preferences.runsIFeedbackSpeedGreater,
						feedbackWin.RunsIFeedbackSpeedGreater);

				preferences.runsIFeedbackSpeedLower = Preferences.PreferencesChange(
						true,
						SqlitePreferences.RunsIFeedbackSpeedLower,
						preferences.runsIFeedbackSpeedLower,
						feedbackWin.RunsIFeedbackSpeedLower);

				preferences.runsIFeedbackTimeGreater = Preferences.PreferencesChange(
						true,
						SqlitePreferences.RunsIFeedbackTimeGreater,
						preferences.runsIFeedbackTimeGreater,
						feedbackWin.RunsIFeedbackTimeGreater);

				preferences.runsIFeedbackTimeLower = Preferences.PreferencesChange(
						true,
						SqlitePreferences.RunsIFeedbackTimeLower,
						preferences.runsIFeedbackTimeLower,
						feedbackWin.RunsIFeedbackTimeLower);
			}

			Sqlite.Close(); // <------

			drawingarea_results_realtime.QueueDraw ();
		}
		else if (Constants.ModeIsENCODER (m))
		{
			// 1) Update bell
			if(feedbackWin.FeedbackActive(bellMode))
				pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "stock_bell_active.png");
			else
				pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "stock_bell_none.png");

			image_encoder_bell.Pixbuf = pixbuf;

			// 2) Update SQL and preferences object
			Sqlite.Open(); // ------>

			//mainVariable
			Constants.EncoderVariablesCapture mainVariable = Constants.SetEncoderVariablesCapture(
					feedbackWin.GetMainVariable);
			if( preferences.encoderCaptureMainVariable != mainVariable ) {
				SqlitePreferences.Update("encoderCaptureMainVariable", mainVariable.ToString(), true);
				preferences.encoderCaptureMainVariable = mainVariable;
			}
			string mainVariableStr = Constants.GetEncoderVariablesCapture(mainVariable);

			((TreeViewEncoder) treeViewResultsSession).EncoderCaptureMainVariable = mainVariable;
			treeViewResultsSession.ResultsInBarsRowChanged (); //to update treeViewResultsSession bold values

			//secondaryVariable
			Constants.EncoderVariablesCapture secondaryVariable = Constants.SetEncoderVariablesCapture(
					feedbackWin.GetSecondaryVariable);
			if( preferences.encoderCaptureSecondaryVariable != secondaryVariable ) {
				SqlitePreferences.Update("encoderCaptureSecondaryVariable", secondaryVariable.ToString(), true);
				preferences.encoderCaptureSecondaryVariable = secondaryVariable;
			}
			string secondaryVariableStr = Constants.GetEncoderVariablesCapture(secondaryVariable);

			//secondaryVariableShow
			bool secondaryVariableShow = feedbackWin.GetSecondaryVariableShow;
			if( preferences.encoderCaptureSecondaryVariableShow != secondaryVariableShow ) {
				SqlitePreferences.Update("encoderCaptureSecondaryVariableShow", secondaryVariableShow.ToString(), true);
				preferences.encoderCaptureSecondaryVariableShow = secondaryVariableShow;
			}
			if(! secondaryVariableShow)
				secondaryVariableStr = "";

			/* these 6 are not stored on DB yet */
			preferences.encoderCaptureSecondaryVariableYAxisCustom = feedbackWin.GetSecondaryVariableYAxisCustom;
			preferences.encoderCaptureSecondaryVariableYAxisCustomMax = feedbackWin.GetSecondaryVariableYAxisCustomMax;
			preferences.encoderCaptureSecondaryVariableYAxisCustomMin = feedbackWin.GetSecondaryVariableYAxisCustomMin;
			preferences.encoderSignalDisplAxisCustom = feedbackWin.GetEncoderSignalDisplAxisCustom;
			preferences.encoderSignalDisplAxisCustomMax = feedbackWin.GetEncoderSignalDisplAxisCustomMax;
			preferences.encoderSignalDisplAxisCustomMin = feedbackWin.GetEncoderSignalDisplAxisCustomMin;


			if(preferences.encoderCaptureFeedbackEccon != feedbackWin.GetEncoderCaptureFeedbackEccon) {
				SqlitePreferences.Update(SqlitePreferences.EncoderCaptureFeedbackEccon,
						feedbackWin.GetEncoderCaptureFeedbackEccon.ToString(), true);
				preferences.encoderCaptureFeedbackEccon = feedbackWin.GetEncoderCaptureFeedbackEccon;
			}

			if(preferences.encoderCaptureInertialEccOverloadMode != feedbackWin.GetEncoderCaptureEccOverloadMode) {
				SqlitePreferences.Update(SqlitePreferences.EncoderCaptureInertialEccOverloadMode,
						feedbackWin.GetEncoderCaptureEccOverloadMode.ToString(), true);
				preferences.encoderCaptureInertialEccOverloadMode = feedbackWin.GetEncoderCaptureEccOverloadMode;
			}

			preferences.encoderCaptureMainVariableThisSetOrHistorical = Preferences.PreferencesChange(
					true,
					SqlitePreferences.EncoderCaptureMainVariableThisSetOrHistorical,
					preferences.encoderCaptureMainVariableThisSetOrHistorical,
					feedbackWin.EncoderRelativeToSet);

			preferences.encoderCaptureMainVariableGreaterActive = Preferences.PreferencesChange(
					true,
					SqlitePreferences.EncoderCaptureMainVariableGreaterActive,
					preferences.encoderCaptureMainVariableGreaterActive,
					feedbackWin.EncoderAutomaticHigherActive);

			preferences.encoderCaptureMainVariableGreaterValue = Preferences.PreferencesChange(
					true,
					SqlitePreferences.EncoderCaptureMainVariableGreaterValue,
					preferences.encoderCaptureMainVariableGreaterValue,
					feedbackWin.EncoderAutomaticHigherValue);

			preferences.encoderCaptureMainVariableLowerActive = Preferences.PreferencesChange(
					true,
					SqlitePreferences.EncoderCaptureMainVariableLowerActive,
					preferences.encoderCaptureMainVariableLowerActive,
					feedbackWin.EncoderAutomaticLowerActive);

			preferences.encoderCaptureMainVariableLowerValue = Preferences.PreferencesChange(
					true,
					SqlitePreferences.EncoderCaptureMainVariableLowerValue,
					preferences.encoderCaptureMainVariableLowerValue,
					feedbackWin.EncoderAutomaticLowerValue);

			preferences.encoderCaptureShowLoss = Preferences.PreferencesChange(
					true,
					SqlitePreferences.EncoderCaptureShowLoss,
					preferences.encoderCaptureShowLoss,
					feedbackWin.EncoderCaptureShowLoss);

			bool encoderFeedbackAsteroidsActive = feedbackWin.GetEncoderFeedbackAsteroidsActive;
			if(preferences.encoderFeedbackAsteroidsActive != encoderFeedbackAsteroidsActive)
			{
				//SqlitePreferences.Update(SqlitePreferences.ForceSensorFeedbackAsteroidsDark, encoderFeedbackAsteroidsActive.ToString(), false); //TODO
				preferences.encoderFeedbackAsteroidsActive = encoderFeedbackAsteroidsActive;
			}

			Sqlite.Close(); // <------

			// 3) Update treeview and graphs

			//treeview_encoder should be updated (to colorize some cells)
			//only if there was data
			//this avoids misbehaviour when bell is pressed and there's no data in treeview
			EncoderCurve curve = treeviewEncoderCaptureCurvesGetCurve(1, false);
			if(curve.N != null) {
				List<string> contents = Util.ReadFileAsStringList(UtilEncoder.GetEncoderCurvesTempFileName(), "");
				encoderUpdateTreeViewCapture(contents); //this updates encoderCaptureCurves

				findAndMarkSavedCurves(false, false); //SQL closed; don't update curve SQL records (like future1: meanPower)

				//also update the bars plot (to show colors depending on bells changes)
				if(captureCurvesBarsData_l.Count > 0) {
					double mainVariableHigher = feedbackWin.GetMainVariableHigher(mainVariableStr);
					double mainVariableLower = feedbackWin.GetMainVariableLower(mainVariableStr);
					//plotCurvesGraphDoPlot(mainVariableStr, mainVariableHigher, mainVariableLower,

					//Cairo
					LogB.Information ("Called prepareEventGraphEncoderCurrent at on_feedback_closed Plot capturing: false");
					prepareEventGraphEncoderCurrent = new PrepareEventGraphEncoderCurrent (
							mainVariableStr, mainVariableHigher, mainVariableLower,
							secondaryVariableStr, preferences.encoderCaptureShowLoss,
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
					encoder_capture_curves_bars_drawingarea_cairo.QueueDraw ();
				} else {
					//UtilGtk.ErasePaint(encoder_capture_curves_bars_drawingarea, encoder_capture_curves_bars_pixmap);
					//TODO: do it on Cairo
				}
			}

			radio_resultsSession_best.Label = Catalog.GetString (preferences.encoderCaptureMainVariable.ToString ());
			UtilGtk.ContrastLabelsWidget (Config.ColorBackgroundIsDark, radio_resultsSession_best);

			//rhythm
			encoderRhythm = feedbackWin.Encoder_rhythm_get_values();
			//updates preferences object and Sqlite preferences
			preferences.UpdateEncoderRhythm(encoderRhythm);
		}
		else if (Constants.ModeIsFORCESENSOR (m))
		{
			// 1) Update bell
			if(feedbackWin.FeedbackActive(bellMode))
				pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "stock_bell_active.png");
			else
				pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "stock_bell_none.png");

			image_contacts_bell.Pixbuf = pixbuf;

			// 2) Update SQL and preferences object

			Preferences.ForceSensorCaptureFeedbackActiveEnum feedbackActive = feedbackWin.GetForceSensorFeedback;
			if(preferences.forceSensorCaptureFeedbackActive != feedbackActive)
			{
				SqlitePreferences.Update(SqlitePreferences.ForceSensorCaptureFeedbackActive, feedbackActive.ToString(), false);
				preferences.forceSensorCaptureFeedbackActive = feedbackActive;
			}

			//change the rest of values only if feedback is active
			if(feedbackActive == Preferences.ForceSensorCaptureFeedbackActiveEnum.RECTANGLE)
			{
				int feedbackAt = feedbackWin.GetForceSensorFeedbackRectangleAt;
				if(preferences.forceSensorCaptureFeedbackAt != feedbackAt)
				{
					SqlitePreferences.Update(SqlitePreferences.ForceSensorCaptureFeedbackAt, feedbackAt.ToString(), false);
					preferences.forceSensorCaptureFeedbackAt = feedbackAt;
				}

				int feedbackRange = feedbackWin.GetForceSensorFeedbackRectangleRange;
				if(preferences.forceSensorCaptureFeedbackRange != feedbackRange)
				{
					SqlitePreferences.Update(SqlitePreferences.ForceSensorCaptureFeedbackRange, feedbackRange.ToString(), false);
					preferences.forceSensorCaptureFeedbackRange = feedbackRange;
				}
			}
			else if(feedbackActive == Preferences.ForceSensorCaptureFeedbackActiveEnum.PATH)
			{
				int feedbackPathMax = feedbackWin.GetForceSensorFeedbackPathMax;
				if(preferences.forceSensorFeedbackPathMax != feedbackPathMax)
				{
					SqlitePreferences.Update(SqlitePreferences.ForceSensorFeedbackPathMax, feedbackPathMax.ToString(), false);
					preferences.forceSensorFeedbackPathMax = feedbackPathMax;
				}

				int feedbackPathMin = feedbackWin.GetForceSensorFeedbackPathMin;
				if(preferences.forceSensorFeedbackPathMin != feedbackPathMin)
				{
					SqlitePreferences.Update(SqlitePreferences.ForceSensorFeedbackPathMin, feedbackPathMin.ToString(), false);
					preferences.forceSensorFeedbackPathMin = feedbackPathMin;
				}

				int feedbackPathMasters = feedbackWin.GetForceSensorFeedbackPathMasters;
				if(preferences.forceSensorFeedbackPathMasters != feedbackPathMasters)
				{
					SqlitePreferences.Update(SqlitePreferences.ForceSensorFeedbackPathMasters, feedbackPathMasters.ToString(), false);
					preferences.forceSensorFeedbackPathMasters = feedbackPathMasters;
				}

				int feedbackPathMasterSeconds = feedbackWin.GetForceSensorFeedbackPathMasterSeconds;
				if(preferences.forceSensorFeedbackPathMasterSeconds != feedbackPathMasterSeconds)
				{
					SqlitePreferences.Update(SqlitePreferences.ForceSensorFeedbackPathMasterSeconds, feedbackPathMasterSeconds.ToString(), false);
					preferences.forceSensorFeedbackPathMasterSeconds = feedbackPathMasterSeconds;
				}

				int feedbackPathLineWidth = feedbackWin.GetForceSensorFeedbackPathLineWidth;
				if(preferences.forceSensorFeedbackPathLineWidth != feedbackPathLineWidth)
				{
					SqlitePreferences.Update(SqlitePreferences.ForceSensorFeedbackPathLineWidth, feedbackPathLineWidth.ToString(), false);
					preferences.forceSensorFeedbackPathLineWidth = feedbackPathLineWidth;
				}
			}
			else if(feedbackActive == Preferences.ForceSensorCaptureFeedbackActiveEnum.QUESTIONNAIRE)
			{
				int feedbackQuestionnaireMax = feedbackWin.GetForceSensorFeedbackQuestionnaireMax;
				if(preferences.forceSensorFeedbackQuestionnaireMax != feedbackQuestionnaireMax)
				{
					//SqlitePreferences.Update(SqlitePreferences.ForceSensorFeedbackQuestionnaireMax, feedbackQuestionnaireMax.ToString(), false); //TODO
					preferences.forceSensorFeedbackQuestionnaireMax = feedbackQuestionnaireMax;
				}

				int feedbackQuestionnaireMin = feedbackWin.GetForceSensorFeedbackQuestionnaireMin;
				if(preferences.forceSensorFeedbackQuestionnaireMin != feedbackQuestionnaireMin)
				{
					//SqlitePreferences.Update(SqlitePreferences.ForceSensorFeedbackQuestionnaireMin, feedbackQuestionnaireMin.ToString(), false); //TODO
					preferences.forceSensorFeedbackQuestionnaireMin = feedbackQuestionnaireMin;
				}

				int feedbackQuestionnaireN = feedbackWin.GetForceSensorFeedbackQuestionnaireN;
				if(preferences.forceSensorFeedbackQuestionnaireN != feedbackQuestionnaireN)
				{
					//SqlitePreferences.Update(SqlitePreferences.ForceSensorFeedbackQuestionnaireN, feedbackQuestionnaireN.ToString(), false); //TODO
					preferences.forceSensorFeedbackQuestionnaireN = feedbackQuestionnaireN;
				}

				int feedbackQuestionnaireQDuration = feedbackWin.GetForceSensorFeedbackQuestionnaireQDuration;
				if(preferences.forceSensorFeedbackQuestionnaireQDuration != feedbackQuestionnaireQDuration)
				{
					//SqlitePreferences.Update(SqlitePreferences.ForceSensorFeedbackQuestionnaireQDuration, feedbackQuestionnaireQDuration.ToString(), false); //TODO
					preferences.forceSensorFeedbackQuestionnaireQDuration = feedbackQuestionnaireQDuration;
				}

				string feedbackQuestionnaireFile = "";
				if (feedbackWin.GetForceSensorFeedbackQuestionnaireRadioFileIsActive)
					feedbackQuestionnaireFile = feedbackWin.GetForceSensorFeedbackQuestionnaireFile;
				if(preferences.forceSensorFeedbackQuestionnaireFile != feedbackQuestionnaireFile)
				{
					//SqlitePreferences.Update(SqlitePreferences.ForceSensorFeedbackQuestionnaireFile, feedbackQuestionnaireFile.ToString(), false); //TODO
					preferences.forceSensorFeedbackQuestionnaireFile = feedbackQuestionnaireFile;
				}

				preferences.forceSensorFeedbackQuestionnaireArithmetical =
					feedbackWin.GetForceSensorFeedbackQuestionnaireRadioArithmeticalIsActive;
				if (preferences.forceSensorFeedbackQuestionnaireArithmetical)
					preferences.forceSensorFeedbackQuestionnaireArithmeticalDifficulty = feedbackWin.GetForceSensorFeedbackQuestionnaireRadioArithmeticalDifficulty;
			}
		}
		else if(m == Constants.Modes.RUNSENCODER)
		{
			drawingarea_race_analyzer_capture_position_time.QueueDraw ();
			drawingarea_race_analyzer_capture_speed_time.QueueDraw ();
			drawingarea_race_analyzer_capture_accel_time.QueueDraw ();
		}

		//asteroids is for forceSensor and for encoder. TODO: So better remove the "forceSensor"
		if ( (Constants.ModeIsENCODER (m) && preferences.encoderFeedbackAsteroidsActive) ||
				(Constants.ModeIsFORCESENSOR (m) &&
				 preferences.forceSensorCaptureFeedbackActive == Preferences.ForceSensorCaptureFeedbackActiveEnum.ASTEROIDS) )
		{
			int feedbackAsteroidsMax = feedbackWin.GetForceSensorFeedbackAsteroidsMax;
			if(preferences.forceSensorFeedbackAsteroidsMax != feedbackAsteroidsMax)
			{
				//SqlitePreferences.Update(SqlitePreferences.ForceSensorFeedbackAsteroidsMax, feedbackAsteroidsMax.ToString(), false); //TODO
				preferences.forceSensorFeedbackAsteroidsMax = feedbackAsteroidsMax;
			}

			int feedbackAsteroidsMin = feedbackWin.GetForceSensorFeedbackAsteroidsMin;
			if(preferences.forceSensorFeedbackAsteroidsMin != feedbackAsteroidsMin)
			{
				//SqlitePreferences.Update(SqlitePreferences.ForceSensorFeedbackAsteroidsMin, feedbackAsteroidsMin.ToString(), false); //TODO
				preferences.forceSensorFeedbackAsteroidsMin = feedbackAsteroidsMin;
			}

			bool forceSensorFeedbackAsteroidsDark = feedbackWin.GetForceSensorFeedbackAsteroidsDark;
			if(preferences.forceSensorFeedbackAsteroidsDark != forceSensorFeedbackAsteroidsDark)
			{
				//SqlitePreferences.Update(SqlitePreferences.ForceSensorFeedbackAsteroidsDark, forceSensorFeedbackAsteroidsDark.ToString(), false); //TODO
				preferences.forceSensorFeedbackAsteroidsDark = forceSensorFeedbackAsteroidsDark;
			}

			int forceSensorFeedbackAsteroidsFrequency = feedbackWin.GetForceSensorFeedbackAsteroidsFrequency;
			if(preferences.forceSensorFeedbackAsteroidsFrequency != forceSensorFeedbackAsteroidsFrequency)
			{
				//SqlitePreferences.Update(SqlitePreferences.ForceSensorFeedbackAsteroidsFrequency, forceSensorFeedbackAsteroidsFrequency.ToString(), false); //TODO
				preferences.forceSensorFeedbackAsteroidsFrequency = forceSensorFeedbackAsteroidsFrequency;
			}

			int forceSensorFeedbackShotsFrequency = feedbackWin.GetForceSensorFeedbackShotsFrequency;
			if(preferences.forceSensorFeedbackShotsFrequency != forceSensorFeedbackShotsFrequency)
			{
				//SqlitePreferences.Update(SqlitePreferences.ForceSensorFeedbackShotsFrequency, forceSensorFeedbackShotsFrequency.ToString(), false); //TODO
				preferences.forceSensorFeedbackShotsFrequency = forceSensorFeedbackShotsFrequency;
			}
		}

		if (Constants.ModeIsENCODER (m) || Constants.ModeIsFORCESENSOR (m))
		{
			bool signalDirectionHorizontal = feedbackWin.GetSignalDirectionHorizontal;
			if(preferences.signalDirectionHorizontal != signalDirectionHorizontal)
			{
				//SqlitePreferences.Update(SqlitePreferences.SignalDirectionHorizontal, signalDirectionHorizontal.ToString(), false); //TODO
				preferences.signalDirectionHorizontal = signalDirectionHorizontal;
				fixEncoderCaptureWidgetsGeometry ();
			}
		}

		updateGraphResultsSessionByMode (); //for example on encoder to update graph if changed main variable
	}

	private void on_feedback_questionnaire_load (object o, EventArgs args)
	{
		if (questionnaire == null)
			questionnaire = new Questionnaire (
					preferences.forceSensorFeedbackQuestionnaireN,
					preferences.forceSensorFeedbackQuestionnaireQDuration,
					feedbackWin.GetForceSensorFeedbackQuestionnaireFile);

		feedbackWin.button_force_sensor_capture_feedback_questionnaire_load_analyzed (
				questionnaire.FileIsOk (feedbackWin.GetForceSensorFeedbackQuestionnaireFile));
	}

	private void on_radio_mode_contacts_capture_toggled (object o, EventArgs args)
	{
		if(! radio_mode_contacts_capture.Active)
			return;

		//if on raceAnalyzer analyze and go to capture (but analyze zoom is active), exit zoom
		if (current_mode == Constants.Modes.RUNSENCODER && check_ai_zoom.Active)
			check_ai_zoom.Active = false;

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
					jumpsProfileCalculate ();
					drawingarea_jumps_profile.QueueDraw ();
				}

				if(radio_mode_contacts_jumps_dj_optimal_fall.Active)
				{
					notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.JUMPSDJOPTIMALFALL);
					jumpsDjOptimalFallCalculate ();
					drawingarea_jumps_dj_optimal_fall.QueueDraw ();
				}

				if(radio_mode_contacts_jumps_weight_fv_profile.Active)
				{
					notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.JUMPSWEIGHTFVPROFILE);
					jumpsWeightFVProfileCalculate ();
					drawingarea_jumps_weight_fv_profile.QueueDraw ();
				}

				if(radio_mode_contacts_jumps_asymmetry.Active)
				{
					notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.JUMPSASYMMETRY);
					jumpsAsymmetryCalculate ();
					drawingarea_jumps_asymmetry.QueueDraw ();
				}

				if(radio_mode_contacts_jumps_evolution.Active)
				{
					notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.JUMPSEVOLUTION);
					jumpsEvolutionCalculate ();
					drawingarea_jumps_evolution.QueueDraw ();
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
					runsEvolutionCalculate (true);
					drawingarea_runs_evolution.QueueDraw ();
				}
			}
			if(radio_mode_contacts_export_csv.Active)
			{
				notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.CONTACTS_EXPORT_CSV);
			}
		}
		else if (Constants.ModeIsFORCESENSOR (current_mode))
			notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.SIGNAL_AI);
		else if(current_mode == Constants.Modes.RUNSENCODER)
			notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.SIGNAL_AI);
		else if (Constants.ModeIsENCODER (current_mode))
		{
			notebook_analyze.CurrentPage = Convert.ToInt32 (notebook_analyze_pages.ENCODER);

			if(radio_encoder_analyze_individual_current_set.Active)
				on_radio_encoder_analyze_individual_current_set (o, args);
			else if(radio_encoder_analyze_individual_current_session.Active)
				on_radio_encoder_analyze_individual_current_session (o, args);
			else if(radio_encoder_analyze_individual_all_sessions.Active)
				on_radio_encoder_analyze_individual_all_sessions (o, args);
			else if(radio_encoder_analyze_groupal_current_session.Active)
				on_radio_encoder_analyze_groupal_current_session (o, args);
		}
		else if(current_mode == Constants.Modes.OTHER)
		{
			notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.CONTACTS_EXPORT_CSV);
		}
		else
			notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.STATISTICS);

		notebook_capture_analyze.CurrentPage = 1;
	}

	private void on_radio_mode_contacts_jumps_profile_toggled (object o, EventArgs args)
	{
		if(radio_mode_contacts_jumps_profile.Active)
		{
			notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.JUMPSPROFILE);
			jumpsProfileCalculate ();
			drawingarea_jumps_profile.QueueDraw ();
		}
	}
	private void on_radio_mode_contacts_jumps_dj_optimal_fall_toggled (object o, EventArgs args)
	{
		if(radio_mode_contacts_jumps_dj_optimal_fall.Active)
		{
			notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.JUMPSDJOPTIMALFALL);
			jumpsDjOptimalFallCalculate ();
			drawingarea_jumps_dj_optimal_fall.QueueDraw ();
		}
	}
	private void on_radio_mode_contacts_jumps_weight_fv_profile_toggled (object o, EventArgs args)
	{
		if(radio_mode_contacts_jumps_weight_fv_profile.Active)
		{
			notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.JUMPSWEIGHTFVPROFILE);
			jumpsWeightFVProfileCalculate ();
			drawingarea_jumps_weight_fv_profile.QueueDraw ();
		}
	}
	private void on_radio_mode_contacts_jumps_asymmetry_toggled (object o, EventArgs args)
	{
		if (radio_mode_contacts_jumps_asymmetry.Active)
		{
			notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.JUMPSASYMMETRY);
			jumpsAsymmetryCalculate ();
			drawingarea_jumps_asymmetry.QueueDraw ();
		}
	}
	private void on_radio_mode_contacts_jumps_evolution_toggled (object o, EventArgs args)
	{
		if(radio_mode_contacts_jumps_evolution.Active)
		{
			notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.JUMPSEVOLUTION);
			jumpsEvolutionCalculate ();
			drawingarea_jumps_evolution.QueueDraw ();
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
			runsEvolutionCalculate (true);
			drawingarea_runs_evolution.QueueDraw ();
		}
	}
	private void on_radio_mode_contacts_sprint_toggled (object o, EventArgs args)
	{
		if(radio_mode_contacts_sprint.Active)
			notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.SPRINT);
	}
	private void on_radio_mode_contacts_export_csv_toggled (object o, EventArgs args)
	{
		if (radio_mode_contacts_export_csv.Active)
			notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.CONTACTS_EXPORT_CSV);
	}

	/* ---------------------------------------------------------
	 * ----------------  SENSITIVE GUI METHODS-------------------
	 *  --------------------------------------------------------
	 */

	private void menuSessionSensitive(bool option)
	{
		box_session_more_this_session.Sensitive = option;
		button_menu_session_export.Sensitive = option;
		button_menu_session_import_from_csv.Sensitive = option;
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
		button_edit_current_person_h.Sensitive = option;
		button_delete_current_person_h.Sensitive = option;
	}

	private void sensitiveGuiNoSession () 
	{
		LogB.Information("sensitiveGuiNoSession");

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
	
		button_contacts_person_change.Sensitive = false;
		frame_contacts_exercise.Sensitive = false;
		
		//notebooks
		notebook_analyze.Sensitive = false;
		scrolledwindow_treeview_results_session.Sensitive = false;
		box_results_session_zoom.Sensitive = false;
		encoder_sensitive_all_except_device(false);

		vbox_stats.Sensitive = false;
		
		sensitiveLastTestButtons(false);
		vbox_execute_test.Sensitive = false;
		button_execute_test.Sensitive = false;
		button_auto_start.Sensitive = false;
		button_force_sensor_adjust.Sensitive = false;
		button_force_sensor_sync.Sensitive = false;

		encoderButtonsSensitive(encoderSensEnum.NOSESSION);
		
		eventExecuteHideAllTables();
	}
	
	private void sensitiveGuiYesSession () 
	{
		LogB.Information("sensitiveGuiYesSession");

		button_image_test_zoom.Sensitive = true;
		frame_persons.Sensitive = true;
		button_recuperate_person.Sensitive = true;
		button_recuperate_persons_from_session.Sensitive = true;
		button_person_add_single.Sensitive = true;
		button_person_add_multiple.Sensitive = true;
		
		button_contacts_person_change.Sensitive = true;
		button_force_sensor_adjust.Sensitive = true;
		button_force_sensor_sync.Sensitive = true;
		
		menuSessionSensitive(true);
		vbox_stats.Sensitive = true;
		
		//changeTestImage("", "", "LOGO");
	}

	//only called by delete person functions (if we run out of persons)
	private void sensitiveGuiNoPerson ()
	{
		LogB.Information("sensitiveGuiNoPerson");

		vbox_persons.Name = "alertCss";

		vbox_jumps.Sensitive = false;
		hbox_jumps_rj.Sensitive = false;
		button_execute_test.Sensitive = false;
		button_auto_start.Sensitive = false;

		encoderButtonsSensitive(encoderSensEnum.NOPERSON);
		//don't cal personChanged because it will make changes on analyze repetitions and currentPerson == null
		//personChanged();

		frame_contacts_exercise.Sensitive = false;
		notebook_analyze.Sensitive = false;
		scrolledwindow_treeview_results_session.Sensitive = false;
		box_results_session_zoom.Sensitive = false;
		encoder_sensitive_all_except_device(false);

		treeview_persons.Sensitive = false;
		
		menuPersonSelectedSensitive(false);
		vbox_execute_test.Sensitive = false;

		label_current_person.Text = "";
		label_top_person_name.Text = "";
		button_person_merge.Sensitive = false;

		if(createdStatsWin)
			stats_win_hide();
	}
	
	private void sensitiveGuiYesPerson ()
	{
		LogB.Information("sensitiveGuiYesPerson");

		vbox_persons.Name = "shiftedCss";

		vbox_jumps.Sensitive = true;
		hbox_jumps_rj.Sensitive = true;
		button_execute_test.Sensitive = true;
		button_auto_start.Sensitive = true;

		encoderButtonsSensitive(encoderSensEnum.YESPERSON);
		personChanged();
		
		frame_contacts_exercise.Sensitive = true;
		notebook_analyze.Sensitive = true;
		scrolledwindow_treeview_results_session.Sensitive = true;
		box_results_session_zoom.Sensitive = true;
		encoder_sensitive_all_except_device(true);

		if(! configChronojump.Exhibition)
			treeview_persons.Sensitive = true;
		
		menuPersonSelectedSensitive(true);
	
		//unsensitive edit, delete, repair events because no event is initially selected
		showHideActionEventButtons (false);

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
		LogB.Information("sensitiveGuiEventDoing");

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

		image_inertial_extended.Visible = true;
		button_encoder_inertial_recalibrate.Visible = false;

		encoderButtonsSensitive(encoderSensEnum.PROCESSINGR);
		
		//hbox
		//hbox_jumps_test.Sensitive = false;
		//hbox_jump_types_options.Sensitive = false;
		
		hbox_jumps_rj.Sensitive = false;
		vbox_runs.Sensitive = false;
		hbox_runs_interval_all.Sensitive = false;
		sensitiveLastTestButtons(false);

		//do not allow to touch session graph/table buttons
		hbox_contacts_graph_table_controls.Sensitive = false;
		frame_contacts_graph_table.Sensitive = false;

		button_contacts_devices_networks.Sensitive = false;
		button_encoder_devices_networks.Sensitive = false;
		button_threshold.Sensitive = false;
		button_force_sensor_adjust.Sensitive = false;
		button_force_sensor_sync.Sensitive = false;
		button_auto_start.Sensitive = false;
		frame_contacts_exercise.Sensitive = false;
	}
  
	private void sensitiveGuiEventDone ()
	{
		LogB.Information(" sensitiveGuiEventDone start ");

		// get the caller method
		// 1 == skip frames, false = no file info
		// var callingMethod = new System.Diagnostics.StackTrace(1, false).GetFrame(0).GetMethod();
		// LogB.Information ("callingMethod: " + callingMethod.ToString ());

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

		//allow show the recalibrate button
		if(encoderInertialCalibratedFirstTime)
		{
			image_inertial_extended.Visible = false;
			button_encoder_inertial_recalibrate.Visible = true;
		}

		if(encoderCaptureCurves != null && encoderCaptureCurves.Count > 0 && encoderProcessCancel == false)
			encoderButtonsSensitive(encoderSensEnum.DONEYESSIGNAL);
		else
			encoderButtonsSensitive(encoderSensEnum.DONENOSIGNAL);

		//hbox
		//hbox_jumps_test.Sensitive = true;
		//hbox_jump_types_options.Sensitive = true;
		
		hbox_jumps_rj.Sensitive = true;
		vbox_runs.Sensitive = true;
		hbox_runs_interval_all.Sensitive = true;

		//allow to touch session graph/table buttons
		hbox_contacts_graph_table_controls.Sensitive = true;
		frame_contacts_graph_table.Sensitive = true;

		button_contacts_devices_networks.Sensitive = true;

		if(! configChronojump.Compujump)
			button_encoder_devices_networks.Sensitive = true;

		button_threshold.Sensitive = true;
		button_force_sensor_adjust.Sensitive = true;
		button_force_sensor_sync.Sensitive = true;
		button_auto_start.Sensitive = true;
		frame_contacts_exercise.Sensitive = true;

		//forceSensor and runEncoder does not use currentEventExecute
		if (Constants.ModeIsFORCESENSOR (current_mode))
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

		// encoder has not currentEventExecute
		if (Constants.ModeIsENCODER (current_mode))
			return;

		//allow repeat last jump or run (check also if it wasn't cancelled)
		if(currentEventExecute != null && ! currentEventExecute.Cancel)
			button_contacts_delete_selected.Sensitive = true;
		else
			sensitiveLastTestButtons(false);

		LogB.Information (string.Format (" sensitiveGuiEventDone end mode: {0}", current_mode));
	}

	//TODO: remove this and use showHideActionEventButtons, and rename it
	//to sensitive on and off the play_this_test and delete_this_test
	private void sensitiveLastTestButtons(bool sensitive)
	{
		LogB.Information("sensitiveLastTestButtons: " + sensitive.ToString());
		//vbox_last_test_buttons.Sensitive = sensitive; TODO:
		button_contacts_delete_selected.Sensitive = sensitive;
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
		chronopicRegister = cros.Do(configChronojump.Compujump, showRunWirelessDevice, configChronojump.FTDIalways);
		
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
			chronopicRegister = cros.Do(configChronojump.Compujump, showRunWirelessDevice, false);
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
			notebook_sup.CurrentPage = Convert.ToInt32(notebook_sup_pages.TESTS);
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
			button_contacts_delete_selected.Sensitive = false;
	}
	
	//true: executing a test; false: waiting a test to be executed
	private void sensitiveGuiAutoExecuteOrWait (bool execute) {
		//if automode, sensitiveGuiEventDoing, sensitiveGuiEventDone don't work
		button_contacts_devices_networks.Sensitive 	= ! execute;
		button_threshold.Sensitive 		= ! execute;
		button_execute_test.Sensitive 		= ! execute;
		sensitiveLastTestButtons(! execute);
	}

	private void showHideActionEventButtons (bool show)
	{
		//bool success = false;
		//bool recordedVideo = false;

		button_contacts_edit_selected.Sensitive = show;

		if (current_mode == Constants.Modes.JUMPSREACTIVE || current_mode == Constants.Modes.RUNSINTERVALLIC)
			button_contacts_repair_selected.Sensitive = show;

		button_contacts_delete_selected.Sensitive = show;
		button_video_play_selected_test (current_mode);
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


	private void connectWidgets (Gtk.Builder builder)
	{
		app1 = (Gtk.Window) builder.GetObject ("app1");

		/*
		   hbox_gui_tests = (Gtk.HBox) builder.GetObject ("hbox_gui_tests");
		   spin_gui_tests = (Gtk.SpinButton) builder.GetObject ("spin_gui_tests");
		   combo_gui_tests = (Gtk.ComboBoxText) builder.GetObject ("combo_gui_tests");
		   button_carles = (Gtk.Button) builder.GetObject ("button_carles");
		   */

		notebook_chronojump_logo = (Gtk.Notebook) builder.GetObject ("notebook_chronojump_logo");
		image_chronojump_logo = (Gtk.Image) builder.GetObject ("image_chronojump_logo");
		drawingarea_chronojump_logo = (Gtk.DrawingArea) builder.GetObject ("drawingarea_chronojump_logo");

		notebook_start = (Gtk.Notebook) builder.GetObject ("notebook_start"); 		//start window or program
		notebook_sup = (Gtk.Notebook) builder.GetObject ("notebook_sup");

		box_contacts_capture_top = (Gtk.Box) builder.GetObject ("box_contacts_capture_top");

		notebook_capture_analyze = (Gtk.Notebook) builder.GetObject ("notebook_capture_analyze"); //not encoder
		notebook_contacts_execute_or = (Gtk.Notebook) builder.GetObject ("notebook_contacts_execute_or");
		notebook_analyze = (Gtk.Notebook) builder.GetObject ("notebook_analyze"); //not encoder
		vbox_contacts_capture_graph = (Gtk.Box) builder.GetObject ("vbox_contacts_capture_graph");
		hbox_message_permissions_at_boot = (Gtk.Box) builder.GetObject ("hbox_message_permissions_at_boot");
		label_message_permissions_at_boot = (Gtk.Label) builder.GetObject ("label_message_permissions_at_boot");
		hbox_message_camera_at_boot = (Gtk.Box) builder.GetObject ("hbox_message_camera_at_boot");
		box_start_modes_and_version = (Gtk.Box) builder.GetObject ("box_start_modes_and_version");
		hbox_start_window_sub = (Gtk.Box) builder.GetObject ("hbox_start_window_sub");

		button_show_modes_contacts = (Gtk.Button) builder.GetObject ("button_show_modes_contacts");
		hbox_change_modes_contacts = (Gtk.Box) builder.GetObject ("hbox_change_modes_contacts");
		hbox_change_modes_encoder = (Gtk.Box) builder.GetObject ("hbox_change_modes_encoder");
		hbox_change_modes_jumps = (Gtk.Box) builder.GetObject ("hbox_change_modes_jumps");
		hbox_change_modes_runs = (Gtk.Box) builder.GetObject ("hbox_change_modes_runs");
		hbox_change_modes_force_sensor = (Gtk.Box) builder.GetObject ("hbox_change_modes_force_sensor");
		radio_change_modes_contacts_jumps_simple = (Gtk.RadioButton) builder.GetObject ("radio_change_modes_contacts_jumps_simple");
		radio_change_modes_contacts_jumps_reactive = (Gtk.RadioButton) builder.GetObject ("radio_change_modes_contacts_jumps_reactive");
		radio_change_modes_contacts_runs_simple = (Gtk.RadioButton) builder.GetObject ("radio_change_modes_contacts_runs_simple");
		radio_change_modes_contacts_runs_intervallic = (Gtk.RadioButton) builder.GetObject ("radio_change_modes_contacts_runs_intervallic");
		radio_change_modes_contacts_runs_encoder = (Gtk.RadioButton) builder.GetObject ("radio_change_modes_contacts_runs_encoder");
		radio_change_modes_contacts_runs_beepTest = (Gtk.RadioButton) builder.GetObject ("radio_change_modes_contacts_runs_beepTest");
		radio_change_modes_contacts_wilight = (Gtk.RadioButton) builder.GetObject ("radio_change_modes_contacts_wilight");
		radio_change_modes_contacts_fourPlatforms = (Gtk.RadioButton) builder.GetObject ("radio_change_modes_contacts_fourPlatforms");
		radio_change_modes_contacts_isometric = (Gtk.RadioButton) builder.GetObject ("radio_change_modes_contacts_isometric");
		radio_change_modes_contacts_elastic = (Gtk.RadioButton) builder.GetObject ("radio_change_modes_contacts_elastic");
		radio_change_modes_encoder_gravitatory = (Gtk.RadioButton) builder.GetObject ("radio_change_modes_encoder_gravitatory");
		radio_change_modes_encoder_inertial = (Gtk.RadioButton) builder.GetObject ("radio_change_modes_encoder_inertial");
		image_change_modes_contacts_jumps_simple = (Gtk.Image) builder.GetObject ("image_change_modes_contacts_jumps_simple");
		image_change_modes_contacts_jumps_reactive = (Gtk.Image) builder.GetObject ("image_change_modes_contacts_jumps_reactive");
		image_change_modes_contacts_runs_simple = (Gtk.Image) builder.GetObject ("image_change_modes_contacts_runs_simple");
		//image_change_modes_contacts_runs_reactive = (Gtk.Image) builder.GetObject ("image_change_modes_contacts_runs_reactive");
		image_change_modes_contacts_runs_intervallic = (Gtk.Image) builder.GetObject ("image_change_modes_contacts_runs_intervallic");
		image_change_modes_contacts_wilight = (Gtk.Image) builder.GetObject ("image_change_modes_contacts_wilight");
		image_change_modes_contacts_fourPlatforms = (Gtk.Image) builder.GetObject ("image_change_modes_contacts_fourPlatforms");
		image_change_modes_contacts_force_sensor = (Gtk.Image) builder.GetObject ("image_change_modes_contacts_force_sensor");
		image_change_modes_contacts_force_sensor1 = (Gtk.Image) builder.GetObject ("image_change_modes_contacts_force_sensor1");
		image_change_modes_contacts_runs_encoder = (Gtk.Image) builder.GetObject ("image_change_modes_contacts_runs_encoder");
		image_change_modes_encoder_gravitatory = (Gtk.Image) builder.GetObject ("image_change_modes_encoder_gravitatory");
		image_change_modes_encoder_inertial = (Gtk.Image) builder.GetObject ("image_change_modes_encoder_inertial");

		alignment_contacts_show_graph_table = (Gtk.Alignment) builder.GetObject ("alignment_contacts_show_graph_table");
		box_contacts_capture_show_need_one = (Gtk.Box) builder.GetObject ("box_contacts_capture_show_need_one");
		label_contacts_capture_show_need_one = (Gtk.Label) builder.GetObject ("label_contacts_capture_show_need_one");
		check_contacts_capture_graph = (Gtk.CheckButton) builder.GetObject ("check_contacts_capture_graph");
		check_contacts_capture_table = (Gtk.CheckButton) builder.GetObject ("check_contacts_capture_table");
		button_contacts_capture_save_image = (Gtk.Button) builder.GetObject ("button_contacts_capture_save_image");

		eventbox_button_show_modes_contacts = (Gtk.EventBox) builder.GetObject ("eventbox_button_show_modes_contacts");
		eventbox_change_modes_contacts_jumps_simple = (Gtk.EventBox) builder.GetObject ("eventbox_change_modes_contacts_jumps_simple");
		eventbox_change_modes_contacts_jumps_reactive = (Gtk.EventBox) builder.GetObject ("eventbox_change_modes_contacts_jumps_reactive");
		eventbox_change_modes_contacts_runs_simple = (Gtk.EventBox) builder.GetObject ("eventbox_change_modes_contacts_runs_simple");
		eventbox_change_modes_contacts_runs_intervallic = (Gtk.EventBox) builder.GetObject ("eventbox_change_modes_contacts_runs_intervallic");
		eventbox_change_modes_contacts_runs_encoder = (Gtk.EventBox) builder.GetObject ("eventbox_change_modes_contacts_runs_encoder");
		eventbox_change_modes_contacts_isometric = (Gtk.EventBox) builder.GetObject ("eventbox_change_modes_contacts_isometric");
		eventbox_change_modes_contacts_elastic = (Gtk.EventBox) builder.GetObject ("eventbox_change_modes_contacts_elastic");
		eventbox_change_modes_encoder_gravitatory = (Gtk.EventBox) builder.GetObject ("eventbox_change_modes_encoder_gravitatory");
		eventbox_change_modes_encoder_inertial = (Gtk.EventBox) builder.GetObject ("eventbox_change_modes_encoder_inertial");
		eventbox_radio_mode_contacts_capture = (Gtk.EventBox) builder.GetObject ("eventbox_radio_mode_contacts_capture");
		eventbox_radio_mode_contacts_analyze = (Gtk.EventBox) builder.GetObject ("eventbox_radio_mode_contacts_analyze");
		eventbox_button_open_chronojump = (Gtk.EventBox) builder.GetObject ("eventbox_button_open_chronojump");
		eventbox_button_help_close = (Gtk.EventBox) builder.GetObject ("eventbox_button_help_close");
		eventbox_button_news_close = (Gtk.EventBox) builder.GetObject ("eventbox_button_news_close");
		eventbox_button_exit_cancel = (Gtk.EventBox) builder.GetObject ("eventbox_button_exit_cancel");
		eventbox_button_exit_confirm = (Gtk.EventBox) builder.GetObject ("eventbox_button_exit_confirm");

		hbox_contacts_sup_capture_analyze_two_buttons = (Gtk.Box) builder.GetObject ("hbox_contacts_sup_capture_analyze_two_buttons");
		hbox_radio_mode_contacts_analyze_buttons = (Gtk.Box) builder.GetObject ("hbox_radio_mode_contacts_analyze_buttons");
		hbox_radio_mode_contacts_analyze_jump_simple_buttons = (Gtk.Box) builder.GetObject ("hbox_radio_mode_contacts_analyze_jump_simple_buttons");

		//radio group
		radio_mode_contacts_capture = (Gtk.RadioButton) builder.GetObject ("radio_mode_contacts_capture");
		radio_mode_contacts_analyze = (Gtk.RadioButton) builder.GetObject ("radio_mode_contacts_analyze");

		//radio group
		radio_mode_contacts_jumps_profile = (Gtk.RadioButton) builder.GetObject ("radio_mode_contacts_jumps_profile");
		radio_mode_contacts_jumps_dj_optimal_fall = (Gtk.RadioButton) builder.GetObject ("radio_mode_contacts_jumps_dj_optimal_fall");
		radio_mode_contacts_jumps_weight_fv_profile = (Gtk.RadioButton) builder.GetObject ("radio_mode_contacts_jumps_weight_fv_profile");
		radio_mode_contacts_jumps_asymmetry = (Gtk.RadioButton) builder.GetObject ("radio_mode_contacts_jumps_asymmetry");
		radio_mode_contacts_jumps_evolution = (Gtk.RadioButton) builder.GetObject ("radio_mode_contacts_jumps_evolution");
		radio_mode_contacts_jumps_rj_fatigue = (Gtk.RadioButton) builder.GetObject ("radio_mode_contacts_jumps_rj_fatigue");
		radio_mode_contacts_runs_evolution = (Gtk.RadioButton) builder.GetObject ("radio_mode_contacts_runs_evolution");
		radio_mode_contacts_sprint = (Gtk.RadioButton) builder.GetObject ("radio_mode_contacts_sprint");
		radio_mode_contacts_advanced = (Gtk.RadioButton) builder.GetObject ("radio_mode_contacts_advanced");
		radio_mode_contacts_export_csv = (Gtk.RadioButton) builder.GetObject ("radio_mode_contacts_export_csv");
		notebook_analyze_export_csv_data = (Gtk.Notebook) builder.GetObject ("notebook_analyze_export_csv_data");

		radio_contacts_export_individual_current_session = (Gtk.RadioButton) builder.GetObject ("radio_contacts_export_individual_current_session");
		radio_contacts_export_individual_all_sessions = (Gtk.RadioButton) builder.GetObject ("radio_contacts_export_individual_all_sessions");
		radio_contacts_export_groupal_current_session = (Gtk.RadioButton) builder.GetObject ("radio_contacts_export_groupal_current_session");
		label_contacts_export_person = (Gtk.Label) builder.GetObject ("label_contacts_export_person");
		label_contacts_export_session = (Gtk.Label) builder.GetObject ("label_contacts_export_session");
		label_contacts_export_result = (Gtk.Label) builder.GetObject ("label_contacts_export_result");
		button_contacts_export_result_open = (Gtk.Button) builder.GetObject ("button_contacts_export_result_open");

		label_sprint_person_name = (Gtk.Label) builder.GetObject ("label_sprint_person_name");

		label_version = (Gtk.Label) builder.GetObject ("label_version");
		//image_selector_start_encoder_inertial = (Gtk.Image) builder.GetObject ("image_selector_start_encoder_inertial");

		image_persons_new_1 = (Gtk.Image) builder.GetObject ("image_persons_new_1");
		image_persons_new_plus = (Gtk.Image) builder.GetObject ("image_persons_new_plus");
		image_persons_open_1 = (Gtk.Image) builder.GetObject ("image_persons_open_1");
		image_persons_open_plus = (Gtk.Image) builder.GetObject ("image_persons_open_plus");

		image_encoder_export_signal = (Gtk.Image) builder.GetObject ("image_encoder_export_signal");

		//contact tests execute buttons
		image_button_finish = (Gtk.Image) builder.GetObject ("image_button_finish");
		image_button_finish1 = (Gtk.Image) builder.GetObject ("image_button_finish1");
		image_button_finish2 = (Gtk.Image) builder.GetObject ("image_button_finish2");
		image_button_cancel = (Gtk.Image) builder.GetObject ("image_button_cancel"); //needed this specially because theme cancel sometimes seems "record"
		image_button_cancel1 = (Gtk.Image) builder.GetObject ("image_button_cancel1");
		image_button_cancel2 = (Gtk.Image) builder.GetObject ("image_button_cancel2");
		//encoder tests execute buttons
		//image_encoder_capture_execute = (Gtk.Image) builder.GetObject ("image_encoder_capture_execute");
		fullscreen_capture_box_buttons_finish_cancel = (Gtk.Box) builder.GetObject ("fullscreen_capture_box_buttons_finish_cancel");
		fullscreen_capture_button_finish = (Gtk.Button) builder.GetObject ("fullscreen_capture_button_finish");
		fullscreen_capture_button_cancel = (Gtk.Button) builder.GetObject ("fullscreen_capture_button_cancel");
		fullscreen_button_encoder_capture_finish_cont = (Gtk.Button) builder.GetObject ("fullscreen_button_encoder_capture_finish_cont");
		fullscreen_button_fullscreen_contacts = (Gtk.Button) builder.GetObject ("fullscreen_button_fullscreen_contacts");
		fullscreen_button_fullscreen_encoder = (Gtk.Button) builder.GetObject ("fullscreen_button_fullscreen_encoder");
		fullscreen_button_fullscreen_exit = (Gtk.Button) builder.GetObject ("fullscreen_button_fullscreen_exit");
		fullscreen_capture_progressbar = (Gtk.ProgressBar) builder.GetObject ("fullscreen_capture_progressbar");
		fullscreen_label_person = (Gtk.Label) builder.GetObject ("fullscreen_label_person");
		fullscreen_label_exercise = (Gtk.Label) builder.GetObject ("fullscreen_label_exercise");
		fullscreen_label_message = (Gtk.Label) builder.GetObject ("fullscreen_label_message");
		fullscreen_capture_drawingarea_cairo = (Gtk.DrawingArea) builder.GetObject ("fullscreen_capture_drawingarea_cairo");
		button_dialog_result_contacts = (Gtk.Button) builder.GetObject ("button_dialog_result_contacts");
		button_dialog_result_contacts_4p = (Gtk.Button) builder.GetObject ("button_dialog_result_contacts_4p");

		hbox_contacts_graph_table_controls = (Gtk.HBox) builder.GetObject ("hbox_contacts_graph_table_controls");
		vpaned_tests = (Gtk.VPaned) builder.GetObject ("vpaned_tests");
		frame_contacts_graph_table = (Gtk.Frame) builder.GetObject ("frame_contacts_graph_table");
		hpaned_contacts_graph_table = (Gtk.HPaned) builder.GetObject ("hpaned_contacts_graph_table");
		treeview_persons = (Gtk.TreeView) builder.GetObject ("treeview_persons");
		scrolledwindow_treeview_results_session = (Gtk.ScrolledWindow) builder.GetObject ("scrolledwindow_treeview_results_session");
		box_results_session_zoom = (Gtk.Box) builder.GetObject ("box_results_session_zoom");
		treeview_results_session = (Gtk.TreeView) builder.GetObject ("treeview_results_session");
		treeview_runs_interval_sprint = (Gtk.TreeView) builder.GetObject ("treeview_runs_interval_sprint");

		hbox_combo_select_jumps = (Gtk.Box) builder.GetObject ("hbox_combo_select_jumps");
		hbox_combo_select_jumps_rj = (Gtk.Box) builder.GetObject ("hbox_combo_select_jumps_rj");
		hbox_combo_select_runs = (Gtk.Box) builder.GetObject ("hbox_combo_select_runs");
		hbox_combo_select_runs_interval = (Gtk.Box) builder.GetObject ("hbox_combo_select_runs_interval");
		hbox_combo_select_contacts_top_with_arrows = (Gtk.Box) builder.GetObject ("hbox_combo_select_contacts_top_with_arrows");
		hbox_combo_select_contacts_top = (Gtk.Box) builder.GetObject ("hbox_combo_select_contacts_top");

		//auto mode	
		//hbox_jump_types_options = (Gtk.Box) builder.GetObject ("hbox_jump_types_options");
		hbox_jump_auto_controls = (Gtk.Box) builder.GetObject ("hbox_jump_auto_controls");
		image_auto_person_skip = (Gtk.Image) builder.GetObject ("image_auto_person_skip");
		image_auto_person_remove = (Gtk.Image) builder.GetObject ("image_auto_person_remove");
		button_auto_start = (Gtk.Button) builder.GetObject ("button_auto_start");
		label_jump_auto_current_person = (Gtk.Label) builder.GetObject ("label_jump_auto_current_person");
		label_jump_auto_current_test = (Gtk.Label) builder.GetObject ("label_jump_auto_current_test");

		image_line_session_avg = (Gtk.Image) builder.GetObject ("image_line_session_avg");
		image_line_session_max = (Gtk.Image) builder.GetObject ("image_line_session_max");
		image_line_person_avg = (Gtk.Image) builder.GetObject ("image_line_person_avg");
		image_line_person_max = (Gtk.Image) builder.GetObject ("image_line_person_max");
		image_line_person_max_all_sessions = (Gtk.Image) builder.GetObject ("image_line_person_max_all_sessions");

		box_database = (Gtk.Box) builder.GetObject ("box_database");
		frame_session = (Gtk.Frame) builder.GetObject ("frame_session");
		hbox_frame_database_top = (Gtk.Box) builder.GetObject ("hbox_frame_database_top");
		vbox_frame_database_border = (Gtk.Box) builder.GetObject ("vbox_frame_database_border");
		image_database_manage_blue = (Gtk.Image) builder.GetObject ("image_database_manage_blue");
		image_database_manage_yellow = (Gtk.Image) builder.GetObject ("image_database_manage_yellow");
		vbox_frame_session_border = (Gtk.Box) builder.GetObject ("vbox_frame_session_border");
		box_session_more = (Gtk.Box) builder.GetObject ("box_session_more");
		box_session_load_or_import = (Gtk.Box) builder.GetObject ("box_session_load_or_import");
		box_session_delete = (Gtk.Box) builder.GetObject ("box_session_delete");
		box_session_export = (Gtk.Box) builder.GetObject ("box_session_export");
		box_session_import = (Gtk.Box) builder.GetObject ("box_session_import");
		box_session_import_current = (Gtk.Box) builder.GetObject ("box_session_import_current");
		box_session_import_confirm = (Gtk.Box) builder.GetObject ("box_session_import_confirm");
		box_session_backup = (Gtk.Box) builder.GetObject ("box_session_backup");
		box_session_data_folder = (Gtk.Box) builder.GetObject ("box_session_data_folder");
		box_session_import_from_csv = (Gtk.Box) builder.GetObject ("box_session_import_from_csv");
		box_help = (Gtk.Box) builder.GetObject ("box_help");
		vbox_news2 = (Gtk.Box) builder.GetObject ("vbox_news2");
		frame_news_downloading = (Gtk.Frame) builder.GetObject ("frame_news_downloading");

		//	hbox_combo_pulses = (Gtk.Box) builder.GetObject ("hbox_combo_pulses");
		vbox_jumps = (Gtk.Box) builder.GetObject ("vbox_jumps");
		//hbox_jumps_test = (Gtk.Box) builder.GetObject ("hbox_jumps_test");
		hbox_jumps_rj = (Gtk.Box) builder.GetObject ("hbox_jumps_rj");
		vbox_runs = (Gtk.Box) builder.GetObject ("vbox_runs");
		hbox_runs_interval_all = (Gtk.Box) builder.GetObject ("hbox_runs_interval_all"); //normal and compujump
		vbox_runs_interval = (Gtk.Box) builder.GetObject ("vbox_runs_interval");
		vbox_runs_interval_compujump = (Gtk.Box) builder.GetObject ("vbox_runs_interval_compujump");

		//menu person
		vbox_persons = (Gtk.Box) builder.GetObject ("vbox_persons");
		hbox_frame_persons_top = (Gtk.Box) builder.GetObject ("hbox_frame_persons_top");
		//alignment44 = (Gtk.Alignment) builder.GetObject ("alignment44");
		button_persons_up = (Gtk.Button) builder.GetObject ("button_persons_up");
		button_persons_down = (Gtk.Button) builder.GetObject ("button_persons_down");

		//tests
		notebook_contacts_capture_doing_wait = (Gtk.Notebook) builder.GetObject ("notebook_contacts_capture_doing_wait");
		button_contacts_bells = (Gtk.Button) builder.GetObject ("button_contacts_bells");
		frame_jumps_automatic = (Gtk.Frame) builder.GetObject ("frame_jumps_automatic");
		notebook_jumps_automatic = (Gtk.Notebook) builder.GetObject ("notebook_jumps_automatic");
		hbox_contacts_device_adjust_threshold = (Gtk.Box) builder.GetObject ("hbox_contacts_device_adjust_threshold");

		button_contacts_edit_selected = (Gtk.Button) builder.GetObject ("button_contacts_edit_selected");
		button_contacts_repair_selected = (Gtk.Button) builder.GetObject ("button_contacts_repair_selected");
		button_contacts_delete_selected = (Gtk.Button) builder.GetObject ("button_contacts_delete_selected");

		//jumps
		extra_windows_jumps_image_dj_fall_calculate = (Gtk.Image) builder.GetObject ("extra_windows_jumps_image_dj_fall_calculate");
		extra_windows_jumps_image_dj_fall_predefined = (Gtk.Image) builder.GetObject ("extra_windows_jumps_image_dj_fall_predefined");
		hbox_extra_window_jumps_fall_height = (Gtk.Box) builder.GetObject ("hbox_extra_window_jumps_fall_height");
		label_contacts_exercise_info = (Gtk.Label) builder.GetObject ("label_contacts_exercise_info");

		vbox_execute_test = (Gtk.Box) builder.GetObject ("vbox_execute_test");
		button_execute_test = (Gtk.Button) builder.GetObject ("button_execute_test");
		viewport_chronopics = (Gtk.Viewport) builder.GetObject ("viewport_chronopics");
		viewport_chronopic_encoder = (Gtk.Viewport) builder.GetObject ("viewport_chronopic_encoder");

		button_contacts_json_upload = (Gtk.Button) builder.GetObject ("button_contacts_json_upload");

		box_contacts_insert_test = (Gtk.Box) builder.GetObject ("box_contacts_insert_test");
		notebook_contacts_insert_test = (Gtk.Notebook) builder.GetObject ("notebook_contacts_insert_test");
		contacts_insert_test_spin_total_time = (Gtk.SpinButton) builder.GetObject ("contacts_insert_test_spin_total_time");
		contacts_insert_test_spin_speed_km_h = (Gtk.SpinButton) builder.GetObject ("contacts_insert_test_spin_speed_km_h");
		contacts_insert_test_spin_track_1_time = (Gtk.SpinButton) builder.GetObject ("contacts_insert_test_spin_track_1_time");
		contacts_insert_test_spin_track_2_time = (Gtk.SpinButton) builder.GetObject ("contacts_insert_test_spin_track_2_time");
		contacts_insert_test_button_insert = (Gtk.Button) builder.GetObject ("contacts_insert_test_button_insert");
		contacts_insert_test_button_insert_and_upload = (Gtk.Button) builder.GetObject ("contacts_insert_test_button_insert_and_upload");
		contacts_insert_test_label_done = (Gtk.Label) builder.GetObject ("contacts_insert_test_label_done");

		//detect devices
		vbox_micro_discover = (Gtk.Box) builder.GetObject ("vbox_micro_discover");
		label_micro_discover_title = (Gtk.Label) builder.GetObject ("label_micro_discover_title");
		label_micro_discover_not_found = (Gtk.Label) builder.GetObject ("label_micro_discover_not_found");
		frame_micro_discover = (Gtk.Frame) builder.GetObject ("frame_micro_discover");
		vbox_micro_discover_main = (Gtk.VBox) builder.GetObject ("vbox_micro_discover_main");
		button_micro_discover_refresh = (Gtk.Button) builder.GetObject ("button_micro_discover_refresh");
		image_micro_discover_refresh = (Gtk.Image) builder.GetObject ("image_micro_discover_refresh");
		box_micro_discover_assign_manually = (Gtk.Box) builder.GetObject ("box_micro_discover_assign_manually");
		buttonbox_micro_discover_assign_manually = (Gtk.ButtonBox) builder.GetObject ("buttonbox_micro_discover_assign_manually");
		grid_micro_discover = (Gtk.Grid) builder.GetObject ("grid_micro_discover");
		box_micro_discover_nc = (Gtk.Box) builder.GetObject ("box_micro_discover_nc");
		label_micro_discover_nc_current_mode = (Gtk.Label) builder.GetObject ("label_micro_discover_nc_current_mode");
		label_micro_discover_connect_error = (Gtk.Label) builder.GetObject ("label_micro_discover_connect_error");
		hbox_contacts_detect_and_execute = (Gtk.Box) builder.GetObject ("hbox_contacts_detect_and_execute");
		hbox_encoder_detect_and_execute = (Gtk.Box) builder.GetObject ("hbox_encoder_detect_and_execute");
		button_contacts_detect = (Gtk.Button) builder.GetObject ("button_contacts_detect");
		button_encoder_detect = (Gtk.Button) builder.GetObject ("button_encoder_detect");
		button_contacts_detect_small = (Gtk.Button) builder.GetObject ("button_contacts_detect_small");
		button_encoder_detect_small = (Gtk.Button) builder.GetObject ("button_encoder_detect_small");
		eventbox_button_micro_discover_cancel_close = (Gtk.EventBox) builder.GetObject ("eventbox_button_micro_discover_cancel_close");
		check_discover_advanced = (Gtk.CheckButton) builder.GetObject ("check_discover_advanced");
		label_discover_advanced = (Gtk.Label) builder.GetObject ("label_discover_advanced");
		image_discover_advanced = (Gtk.Image) builder.GetObject ("image_discover_advanced");
		button_micro_discover_cancel_close = (Gtk.Button) builder.GetObject ("button_micro_discover_cancel_close");
		image_button_micro_discover_cancel_close = (Gtk.Image) builder.GetObject ("image_button_micro_discover_cancel_close");
		label_button_micro_discover_cancel_close = (Gtk.Label) builder.GetObject ("label_button_micro_discover_cancel_close");
		image_button_micro_discover_assign_manually_cancel = (Gtk.Image) builder.GetObject ("image_button_micro_discover_assign_manually_cancel");
		//image_micro_discover_mode = (Gtk.Image) builder.GetObject ("image_micro_discover_mode");

		label_threshold = (Gtk.Label) builder.GetObject ("label_threshold");

		//force sensor
		hbox_capture_phases = (Gtk.Box) builder.GetObject ("hbox_capture_phases");
		hbox_capture_time = (Gtk.Box) builder.GetObject ("hbox_capture_time");

		//widgets for enable or disable
		frame_persons = (Gtk.Frame) builder.GetObject ("frame_persons");
		frame_persons_top = (Gtk.Frame) builder.GetObject ("frame_persons_top");
		vbox_persons_bottom = (Gtk.Box) builder.GetObject ("vbox_persons_bottom");
		hbox_persons_bottom_photo = (Gtk.Box) builder.GetObject ("hbox_persons_bottom_photo");
		button_recuperate_person = (Gtk.Button) builder.GetObject ("button_recuperate_person");
		button_recuperate_persons_from_session = (Gtk.Button) builder.GetObject ("button_recuperate_persons_from_session");
		button_person_add_single = (Gtk.Button) builder.GetObject ("button_person_add_single");
		button_person_add_multiple = (Gtk.Button) builder.GetObject ("button_person_add_multiple");

		button_contacts_exercise_close_and_capture = (Gtk.Button) builder.GetObject ("button_contacts_exercise_close_and_capture");
		notebook_execute = (Gtk.Notebook) builder.GetObject ("notebook_execute");
		notebook_options_top = (Gtk.Notebook) builder.GetObject ("notebook_options_top");

		eventbox_image_test = (Gtk.EventBox) builder.GetObject ("eventbox_image_test");
		image_test = (Gtk.Image) builder.GetObject ("image_test");
		button_image_test_zoom = (Gtk.Button) builder.GetObject ("button_image_test_zoom");
		image_test_zoom = (Gtk.Image) builder.GetObject ("image_test_zoom");
		button_image_test_add_edit = (Gtk.Button) builder.GetObject ("button_image_test_add_edit");
		image_test_add_edit = (Gtk.Image) builder.GetObject ("image_test_add_edit");
		button_inspect_last_test_run_simple = (Gtk.Button) builder.GetObject ("button_inspect_last_test_run_simple");
		button_inspect_last_test_run_intervallic = (Gtk.Button) builder.GetObject ("button_inspect_last_test_run_intervallic");
		//vbox_last_test_buttons = (Gtk.VBox) builder.GetObject ("vbox_last_test_buttons");

		hbox_chronopics_and_more = (Gtk.Box) builder.GetObject ("hbox_chronopics_and_more");
		button_contacts_devices_networks = (Gtk.Button) builder.GetObject ("button_contacts_devices_networks");
		button_threshold = (Gtk.Button) builder.GetObject ("button_threshold");
		button_force_sensor_adjust = (Gtk.Button) builder.GetObject ("button_force_sensor_adjust");
		button_force_sensor_sync = (Gtk.Button) builder.GetObject ("button_force_sensor_sync");

		//non standard icons	
		//image_jump_reactive_bell = (Gtk.Image) builder.GetObject ("image_jump_reactive_bell");
		//image_run_interval_bell = (Gtk.Image) builder.GetObject ("image_run_interval_bell");
		image_contacts_repair_selected = (Gtk.Image) builder.GetObject ("image_contacts_repair_selected");
		image_jump_type_delete_simple = (Gtk.Image) builder.GetObject ("image_jump_type_delete_simple");
		image_jump_type_delete_reactive = (Gtk.Image) builder.GetObject ("image_jump_type_delete_reactive");
		image_run_type_delete_simple = (Gtk.Image) builder.GetObject ("image_run_type_delete_simple");
		image_run_type_delete_intervallic = (Gtk.Image) builder.GetObject ("image_run_type_delete_intervallic");

		image_results_session_zoom = (Gtk.Image) builder.GetObject ("image_results_session_zoom");

		//encoder
		//image_encoder_analyze_zoom = (Gtk.Image) builder.GetObject ("image_encoder_analyze_zoom");
		image_encoder_analyze_stats = (Gtk.Image) builder.GetObject ("image_encoder_analyze_stats");
		image_encoder_analyze_mode_options_close_and_analyze = (Gtk.Image) builder.GetObject ("image_encoder_analyze_mode_options_close_and_analyze");
		image_encoder_analyze_image_save = (Gtk.Image) builder.GetObject ("image_encoder_analyze_image_save");
		image_encoder_analyze_1RM_save = (Gtk.Image) builder.GetObject ("image_encoder_analyze_1RM_save");
		image_encoder_analyze_table_save = (Gtk.Image) builder.GetObject ("image_encoder_analyze_table_save");
		image_encoder_inertial_instructions = (Gtk.Image) builder.GetObject ("image_encoder_inertial_instructions");
		label_gravitatory_vpf_propulsive = (Gtk.Label) builder.GetObject ("label_gravitatory_vpf_propulsive");

		//forcesensor
		image_forcesensor_analyze_save_signal = (Gtk.Image) builder.GetObject ("image_forcesensor_analyze_save_signal");
		image_forcesensor_analyze_save_rfd_auto = (Gtk.Image) builder.GetObject ("image_forcesensor_analyze_save_rfd_auto");
		image_forcesensor_analyze_save_rfd_manual = (Gtk.Image) builder.GetObject ("image_forcesensor_analyze_save_rfd_manual");

		vbox_prefs_util_help = (Gtk.Box) builder.GetObject ("vbox_prefs_util_help");

		radio_menu_2_2_2_jumps = (Gtk.RadioButton) builder.GetObject ("radio_menu_2_2_2_jumps");
		radio_menu_2_2_2_races = (Gtk.RadioButton) builder.GetObject ("radio_menu_2_2_2_races");
		radio_menu_2_2_2_wilight = (Gtk.RadioButton) builder.GetObject ("radio_menu_2_2_2_wilight");
		radio_menu_2_2_2_fourPlatforms = (Gtk.RadioButton) builder.GetObject ("radio_menu_2_2_2_fourPlatforms");
		radio_menu_2_2_2_force = (Gtk.RadioButton) builder.GetObject ("radio_menu_2_2_2_force");
		radio_menu_2_2_2_elastic = (Gtk.RadioButton) builder.GetObject ("radio_menu_2_2_2_elastic");
		radio_menu_2_2_2_weights = (Gtk.RadioButton) builder.GetObject ("radio_menu_2_2_2_weights");
		radio_menu_2_2_2_inertial = (Gtk.RadioButton) builder.GetObject ("radio_menu_2_2_2_inertial");
		eventbox_radio_menu_2_2_2_jumps = (Gtk.EventBox) builder.GetObject ("eventbox_radio_menu_2_2_2_jumps");
		eventbox_radio_menu_2_2_2_races = (Gtk.EventBox) builder.GetObject ("eventbox_radio_menu_2_2_2_races");
		eventbox_radio_menu_2_2_2_force = (Gtk.EventBox) builder.GetObject ("eventbox_radio_menu_2_2_2_force");
		eventbox_radio_menu_2_2_2_elastic = (Gtk.EventBox) builder.GetObject ("eventbox_radio_menu_2_2_2_elastic");
		eventbox_radio_menu_2_2_2_weights = (Gtk.EventBox) builder.GetObject ("eventbox_radio_menu_2_2_2_weights");
		eventbox_radio_menu_2_2_2_inertial = (Gtk.EventBox) builder.GetObject ("eventbox_radio_menu_2_2_2_inertial");
		notebook_menu_2_2_2 = (Gtk.Notebook) builder.GetObject ("notebook_menu_2_2_2"); //0 jumps, 1 races, 2 isometric/elastic/weights/inertial
		label_selector_menu_2_2_2_title = (Gtk.Label) builder.GetObject ("label_selector_menu_2_2_2_title");
		label_selector_menu_2_2_2_desc = (Gtk.Label) builder.GetObject ("label_selector_menu_2_2_2_desc");
		align_label_selector_menu_2_2_2_desc = (Gtk.Alignment) builder.GetObject ("align_label_selector_menu_2_2_2_desc");

		label_exit_confirm = (Gtk.Label) builder.GetObject ("label_exit_confirm");
		button_exit_cancel = (Gtk.Button) builder.GetObject ("button_exit_cancel");
	}

}
