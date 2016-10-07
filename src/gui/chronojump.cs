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
 * Copyright (C) 2004-2016   Xavier de Blas <xaviblas@gmail.com> 
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
using LongoMatch.Gui;
using LongoMatch.Video.Capturer;
using LongoMatch.Video.Common;
using LongoMatch.Video.Utils;
using System.Threading;
using System.Diagnostics;

public partial class ChronoJumpWindow 
{
	[Widget] Gtk.Window app1;
	[Widget] Gtk.MenuBar main_menu;
	[Widget] Gtk.MenuItem menuitem_mode;
	
	[Widget] Gtk.HBox hbox_gui_tests;
	[Widget] Gtk.SpinButton spin_gui_tests;
	[Widget] Gtk.Button button_carles;
	
	[Widget] Gtk.RadioMenuItem radio_menuitem_mode_jumps_simple;
	[Widget] Gtk.RadioMenuItem radio_menuitem_mode_jumps_reactive;
	[Widget] Gtk.RadioMenuItem radio_menuitem_mode_runs_simple;
	[Widget] Gtk.RadioMenuItem radio_menuitem_mode_runs_intervallic;
	[Widget] Gtk.RadioMenuItem radio_menuitem_mode_power_gravitatory;
	[Widget] Gtk.RadioMenuItem radio_menuitem_mode_power_inertial;
	[Widget] Gtk.RadioMenuItem radio_menuitem_mode_other;

	[Widget] Gtk.MenuItem menuitem_mode_selected_jumps_simple;
	[Widget] Gtk.MenuItem menuitem_mode_selected_jumps_reactive;
	[Widget] Gtk.MenuItem menuitem_mode_selected_runs_simple;
	[Widget] Gtk.MenuItem menuitem_mode_selected_runs_intervallic;
	[Widget] Gtk.MenuItem menuitem_mode_selected_power_gravitatory;
	[Widget] Gtk.MenuItem menuitem_mode_selected_power_inertial;
	[Widget] Gtk.MenuItem menuitem_mode_selected_other;
	
	[Widget] Gtk.Notebook notebook_start; //use to display the start images to select different modes
	[Widget] Gtk.Notebook notebook_sup;
	[Widget] Gtk.HBox hbox_other;
	[Widget] Gtk.Notebook notebook_capture_graph_table;
	[Widget] Gtk.Notebook notebook_capture_analyze; //not encoder

	
	[Widget] Gtk.Label label_version;
	[Widget] Gtk.Image image_logo;
	[Widget] Gtk.Image image_selector_start_jumps;
	[Widget] Gtk.Image image_selector_start_runs;
	[Widget] Gtk.Image image_selector_start_encoder_gravitatory;
	//[Widget] Gtk.Image image_selector_start_encoder_inertial;
	
	[Widget] Gtk.Notebook notebook_selector_start_jumps;
	[Widget] Gtk.Viewport viewport_selector_start_jumps;
	[Widget] Gtk.Label label_selector_start_jumps_simple;
	[Widget] Gtk.Label label_selector_start_jumps_reactive;
	[Widget] Gtk.Notebook notebook_selector_start_runs;
	[Widget] Gtk.Viewport viewport_selector_start_runs;
	[Widget] Gtk.Label label_selector_start_runs_simple;
	[Widget] Gtk.Label label_selector_start_runs_intervallic;
	[Widget] Gtk.Notebook notebook_selector_start_encoder;
	[Widget] Gtk.Viewport viewport_selector_start_encoder;
	[Widget] Gtk.Label label_selector_start_encoder_gravitatory;
	[Widget] Gtk.Label label_selector_start_encoder_inertial;
	
	[Widget] Gtk.RadioButton radio_mode_reaction_times_small;
	[Widget] Gtk.RadioButton radio_mode_pulses_small;
	[Widget] Gtk.RadioButton radio_mode_multi_chronopic_small;
	[Widget] Gtk.RadioButton radio_mode_encoder_capture_small;
	[Widget] Gtk.RadioButton radio_mode_encoder_analyze_small;
	[Widget] Gtk.Image image_mode_jumps_small;
	[Widget] Gtk.Image image_mode_jumps_reactive_small;
	[Widget] Gtk.Image image_mode_runs_small;
	[Widget] Gtk.Image image_mode_runs_intervallic_small;
	[Widget] Gtk.Image image_mode_reaction_times_small;
	[Widget] Gtk.Image image_mode_pulses_small;
	[Widget] Gtk.Image image_mode_multi_chronopic_small;
	[Widget] Gtk.Label label_mode_jumps_small;
	[Widget] Gtk.Label label_mode_jumps_reactive_small;
	[Widget] Gtk.Label label_mode_runs_small;
	[Widget] Gtk.Label label_mode_runs_intervallic_small;
	[Widget] Gtk.Image image_mode_encoder_gravitatory;
	[Widget] Gtk.Image image_mode_encoder_inertial;
	[Widget] Gtk.Label label_mode_reaction_times_small;
	[Widget] Gtk.Label label_mode_pulses_small;
	[Widget] Gtk.Label label_mode_multi_chronopic_small;

	[Widget] Gtk.Image image_persons_new_1;
	[Widget] Gtk.Image image_persons_new_plus;
	[Widget] Gtk.Image image_persons_open_1;
	[Widget] Gtk.Image image_persons_open_plus;

	[Widget] Gtk.Notebook notebook_session_person;
	//[Widget] Gtk.Box vbox_persons;

	[Widget] Gtk.TreeView treeview_persons;
	[Widget] Gtk.TreeView treeview_jumps;
	[Widget] Gtk.TreeView treeview_jumps_rj;
	[Widget] Gtk.TreeView treeview_runs;
	[Widget] Gtk.TreeView treeview_runs_interval;
	[Widget] Gtk.TreeView treeview_reaction_times;
	[Widget] Gtk.TreeView treeview_pulses;
	[Widget] Gtk.TreeView treeview_multi_chronopic;
	
	[Widget] Gtk.Box hbox_combo_select_jumps;
	[Widget] Gtk.Box hbox_combo_select_jumps_rj;
	[Widget] Gtk.Box hbox_combo_select_runs;
	[Widget] Gtk.Box hbox_combo_select_runs_interval;

	//auto mode	
	[Widget] Gtk.Box hbox_jump_types_options;
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

	
	[Widget] Gtk.Box hbox_combo_result_jumps;
	[Widget] Gtk.Box hbox_combo_result_jumps_rj;
	[Widget] Gtk.Box hbox_combo_result_runs;
	[Widget] Gtk.Box hbox_combo_result_runs_interval;
	
	[Widget] Gtk.Box hbox_combo_pulses;
	[Widget] Gtk.Box hbox_jumps;
	[Widget] Gtk.Box hbox_jumps_test;
	[Widget] Gtk.Box hbox_jumps_rj;
	[Widget] Gtk.Box hbox_runs;
	[Widget] Gtk.Box hbox_runs_interval;
	[Widget] Gtk.Box hbox_pulses;
	
	[Widget] Gtk.ComboBox combo_select_jumps;
	[Widget] Gtk.ComboBox combo_select_jumps_rj;
	[Widget] Gtk.ComboBox combo_select_runs;
	[Widget] Gtk.ComboBox combo_select_runs_interval;
	
	[Widget] Gtk.ComboBox combo_result_jumps;
	[Widget] Gtk.ComboBox combo_result_jumps_rj;
	[Widget] Gtk.ComboBox combo_result_runs;
	[Widget] Gtk.ComboBox combo_result_runs_interval;
	
	[Widget] Gtk.ComboBox combo_pulses;

	//menus
	[Widget] Gtk.MenuItem session_menuitem;
	[Widget] Gtk.MenuItem help_menuitem;

	//menu session
	[Widget] Gtk.MenuItem menuitem_edit_session;
	[Widget] Gtk.MenuItem menuitem_delete_session;
	[Widget] Gtk.MenuItem menuitem_export_csv;
	[Widget] Gtk.MenuItem menuitem_export_xml;
	[Widget] Gtk.MenuItem menuitem_encoder_session_overview;
	[Widget] Gtk.Image image_session_open;
		
	//menu person
	[Widget] Gtk.Button button_persons_up;
	[Widget] Gtk.Button button_persons_down;
	[Widget] Gtk.Button button_edit_current_person;
	[Widget] Gtk.Button button_show_all_person_events;
	[Widget] Gtk.Button button_delete_current_person;
	[Widget] Gtk.Label label_current_person;
	[Widget] Gtk.Label label_current_person_title;
	
	//tests
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

	[Widget] Gtk.Button button_jumps_result_help_power;
	[Widget] Gtk.Button button_jumps_result_help_stiffness;
	[Widget] Gtk.Button button_jumps_rj_result_help_power;
	[Widget] Gtk.Button button_jumps_rj_result_help_stiffness;
	
	[Widget] Gtk.DrawingArea drawingarea_jumps_profile;
	[Widget] Gtk.ScrolledWindow scrolledwindow_jumps_profile_help;
	[Widget] Gtk.ScrolledWindow scrolledwindow_jumps_profile_training;
	[Widget] Gtk.Image image_tab_jumps_profile;
	
	
	//runs
	[Widget] Gtk.MenuItem menu_execute_simple_runs1;
	[Widget] Gtk.MenuItem menu_execute_intervallic_runs1;
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
	[Widget] Gtk.Box hbox_chronopics;
	[Widget] Gtk.Label label_chronopics;
	[Widget] Gtk.Label label_connected_chronopics;
	[Widget] Gtk.Label label_chronopics_multitest;
	[Widget] Gtk.Image image_simulated_warning;
	[Widget] Gtk.Box hbox_chronopic_detecting;
	[Widget] Gtk.ProgressBar progressbar_chronopic_detecting;
	[Widget] Gtk.Button button_chronopic_detecting_cancel;
	[Widget] Gtk.Button button_chronopic_detecting_info;
	[Widget] Gtk.Box hbox_chronopic_encoder_detecting;
	[Widget] Gtk.ProgressBar progressbar_chronopic_encoder_detecting;
	[Widget] Gtk.Button button_chronopic_encoder_detecting_cancel;
	[Widget] Gtk.Button button_chronopic_encoder_detecting_info;
	[Widget] Gtk.Viewport viewport_chronopic_encoder;
	[Widget] Gtk.Label label_chronopic_encoder;
	[Widget] Gtk.Image image_chronopic_encoder_no;
	[Widget] Gtk.Image image_chronopic_encoder_yes;
	
	[Widget] Gtk.HBox hbox_video_capture;
	[Widget] Gtk.Label label_video_feedback;
	[Widget] Gtk.CheckButton checkbutton_video;
	//[Widget] Gtk.Label label_video;
	[Widget] Gtk.Image image_video_yes;
	[Widget] Gtk.Image image_video_no;


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
	[Widget] Gtk.Button button_new;
	[Widget] Gtk.Button button_open;
	[Widget] Gtk.Frame frame_persons;
	[Widget] Gtk.Button button_recuperate_person;
	[Widget] Gtk.Button button_recuperate_persons_from_session;
	[Widget] Gtk.Button button_person_add_single;
	[Widget] Gtk.Button button_person_add_multiple;

	[Widget] Gtk.Button button_run_custom;
	[Widget] Gtk.Button button_run_20m;
	[Widget] Gtk.Button button_run_100m;
	[Widget] Gtk.Button button_run_200m;
	[Widget] Gtk.Button button_run_400m;
	[Widget] Gtk.Button button_run_gesell;
	[Widget] Gtk.Button button_run_20yard;
	[Widget] Gtk.Button button_run_505;
	[Widget] Gtk.Button button_run_illinois;
	[Widget] Gtk.Button button_run_margaria;
	[Widget] Gtk.Button button_run_shuttle;
	[Widget] Gtk.Button button_run_zigzag;
	[Widget] Gtk.Button button_run_interval_by_laps;
	[Widget] Gtk.Button button_run_interval_by_time;
	[Widget] Gtk.Button button_run_interval_unlimited;
	[Widget] Gtk.Button button_run_interval_mtgug;
	[Widget] Gtk.Button button_reaction_time_execute;

	[Widget] Gtk.Notebook notebook_execute;
	[Widget] Gtk.Notebook notebook_results;
	[Widget] Gtk.Notebook notebook_options_top;
		
	[Widget] Gtk.Frame frame_share_data;
	
	[Widget] Gtk.EventBox eventbox_image_test;
	[Widget] Gtk.Image image_test;
	[Widget] Gtk.Button button_image_test_zoom;
	[Widget] Gtk.Image image_test_zoom;
	[Widget] Gtk.Box vbox_this_test_buttons;
	[Widget] Gtk.Button button_video_play_this_test;
	[Widget] Gtk.Button button_delete_this_test;
		
	[Widget] Gtk.Button button_upload_session;
	[Widget] Gtk.Button button_activate_chronopics;

	//non standard icons	
	[Widget] Gtk.Image image_jump_reactive_bell;
	[Widget] Gtk.Image image_run_interval_bell;
	[Widget] Gtk.Image image_jump_reactive_repair;
	[Widget] Gtk.Image image_run_interval_repair;
	[Widget] Gtk.Image image_pulse_repair;
	[Widget] Gtk.Image image_person_delete;
	[Widget] Gtk.Image image_delete_this_test;
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
	[Widget] Gtk.Image image_encoder_capture_zoom;
	//[Widget] Gtk.Image image_encoder_analyze_zoom;
	[Widget] Gtk.Image image_encoder_analyze_stats;
	[Widget] Gtk.Image image_encoder_analyze_image_save;
	[Widget] Gtk.Image image_encoder_analyze_1RM_save;
	[Widget] Gtk.Image image_encoder_analyze_table_save;
	[Widget] Gtk.Image image_encoder_signal_delete;
	[Widget] Gtk.Image image_encoder_inertial_instructions;


	Random rand;

	//persons
	private TreeStore treeview_persons_store;
	private TreeViewPersons myTreeViewPersons;
	private Gtk.Button fakeButtonPersonUp;
	private Gtk.Button fakeButtonPersonDown;
	//normal jumps
	private TreeStore treeview_jumps_store;
	private TreeViewJumps myTreeViewJumps;
	//rj jumps
	private TreeStore treeview_jumps_rj_store;
	private TreeViewJumpsRj myTreeViewJumpsRj;
	//normal runs
	private TreeStore treeview_runs_store;
	private TreeViewRuns myTreeViewRuns;
	//runs interval
	private TreeStore treeview_runs_interval_store;
	private TreeViewRunsInterval myTreeViewRunsInterval;
	//reaction times
	private TreeStore treeview_reaction_times_store;
	private TreeViewReactionTimes myTreeViewReactionTimes;
	//pulses
	private TreeStore treeview_pulses_store;
	private TreeViewPulses myTreeViewPulses;
	//multiChronopic
	private TreeStore treeview_multi_chronopic_store;
	private TreeViewMultiChronopic myTreeViewMultiChronopic;
	
	private Preferences preferences;

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

	//windows needed
	PreferencesWindow preferencesWin;
	SessionAddEditWindow sessionAddEditWin;
	SessionLoadWindow sessionLoadWin;
	PersonRecuperateWindow personRecuperateWin; 
	PersonsRecuperateFromOtherSessionWindow personsRecuperateFromOtherSessionWin; 
	PersonAddModifyWindow personAddModifyWin; 
	PersonAddMultipleWindow personAddMultipleWin; 
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
	ErrorWindow errorWin;
	ReportWindow reportWin;
	RepetitiveConditionsWindow repetitiveConditionsWin;
	GenericWindow genericWin;
		
	EvaluatorWindow evalWin;
	PersonNotUploadWindow personNotUploadWin; 
	ExecuteAutoWindow executeAutoWin;
	
	ChronopicWindow chronopicWin;
	ChronopicWizardWindow chronopicWizardWin;
	string wizardPortContacts;
	string wizardPortEncoder;
		
	static Thread pingThread;

	
	/*
	 * useful to not check for Chronopic if changing select_menuitem_mode_toggled from a 50 to a 50
	 * great for 1.6.3 where people change from simple jumps to reactive jumps and Chronopic don't need to change
	 *
	 * maybe it will be replaced by chronopic_wizard_window

	private enum chronopicTypes { CONTACTS50, CONTACTS10, ENCODER }
	private chronopicTypes lastChronopicType;
	 */
	
	private bool firstRjValue;
	private double rjTcCount;
	private double rjTvCount;
	private string rjTcString;
	private string rjTvString;
	
	private bool createdStatsWin;
	
	private string progVersion;
	private string progName;

	private string runningFileName; //useful for knowing if there are two chronojump instances

	private bool normalGUI; //false means small gui

	private bool debugMode; //currently only for encoder R

	//int chronopicCancelledTimes = 0;


	//only called the first time the software runs
	//and only on windows
	private void on_language_clicked(object o, EventArgs args) {
		//languageChange();
		//createMainWindow("");
	}

	private void on_button_image_test_zoom_clicked(object o, EventArgs args) {
		EventType myType;
		if(radio_menuitem_mode_jumps_simple.Active) 
			myType = currentJumpType;
		else if(radio_menuitem_mode_jumps_reactive.Active) 
			myType = currentJumpRjType;
		else if(radio_menuitem_mode_runs_simple.Active) 
			myType = currentRunType;
		else if(radio_menuitem_mode_runs_intervallic.Active) 
			myType = currentRunIntervalType;
		else if(radio_mode_reaction_times_small.Active) 
			myType = currentReactionTimeType;
		else if(radio_mode_pulses_small.Active) 
			myType = currentPulseType;
		else //if(radio_mode_multi_chronopic_small.Active) 
			myType = currentMultiChronopicType;
			
		if(myType.Name == "DJa" && extra_window_jumps_check_dj_fall_calculate.Active)
			new DialogImageTest("", Util.GetImagePath(false) + "jump_dj_a_inside.png", DialogImageTest.ArchiveType.ASSEMBLY);
		else if(myType.Name == "DJna" && extra_window_jumps_check_dj_fall_calculate.Active)
			new DialogImageTest("", Util.GetImagePath(false) + "jump_dj_inside.png", DialogImageTest.ArchiveType.ASSEMBLY);
		else
			new DialogImageTest(myType);
	}
	
	
	public ChronoJumpWindow(string progVersion, string progName, string runningFileName)
	{
		this.progVersion = progVersion;
		this.progName = progName;
		this.runningFileName = runningFileName;
		

		Glade.XML gxml;
		gxml = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "app1", "chronojump");
		gxml.Autoconnect(this);

		//put an icon to window
		UtilGtk.IconWindow(app1);
	
		//show chronojump logo on down-left area
		changeTestImage("", "", "LOGO");
	
		//white bg
		eventbox_image_test.ModifyBg(StateType.Normal, UtilGtk.WHITE);
	
		//start with the Mode selector	
		notebook_start.CurrentPage = 0;

		//new DialogMessage(Constants.MessageTypes.INFO, UtilGtk.ScreenHeightFitted(false).ToString() );
		//UtilGtk.ResizeIfNeeded(stats_window);
		
		//app1.Maximize(); //this was for starting at fullscreen

		report = new Report(-1); //when a session is loaded or created, it will change the report.SessionID value
		//TODO: check what happens if a session it's deleted
		//i think report it's deactivated until a new session is created or loaded, 
		//but check what happens if report window is opened

		//put videoOn as false before loading preferences to start always without the camera
		//this is good if camera produces crash
		SqlitePreferences.Update("videoOn", "False", false);
		
		//preferencesLoaded is a fix to a gtk#-net-windows-bug where radiobuttons raise signals
		//at initialization of chronojump and gives problems if this signals are raised while preferences are loading
		loadPreferences ();

		createTreeView_persons (treeview_persons);

		createTreeView_jumps (treeview_jumps);
		createTreeView_jumps_rj (treeview_jumps_rj);
		createTreeView_runs (treeview_runs);
		createTreeView_runs_interval (treeview_runs_interval);
		createTreeView_reaction_times (treeview_reaction_times);
		createTreeView_pulses (treeview_pulses);
		createTreeView_multi_chronopic (false, treeview_multi_chronopic);

		createComboSelectJumps(true);
		createComboSelectJumpsRj(true);
		createComboSelectRuns(true);
		createComboSelectRunsInterval(true);
		
		createComboResultJumps();
		createComboResultJumpsRj();
		createComboResultRuns();
		createComboResultRunsInterval();

		//reaction_times has no combo
		createComboPulses();
		//createComboMultiChronopic();
		createdStatsWin = false;
		
		repetitiveConditionsWin = RepetitiveConditionsWindow.Create();
		repetitiveConditionsWin.FakeButtonClose.Clicked += new EventHandler(on_repetitive_conditions_closed);

		createChronopicWindow(false, "");
		wizardPortContacts = "";
		wizardPortEncoder = "";
	
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
	
		formatModeMenu();	
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

		encoderInitializeStuff();	

		//presentationInit();

		videoCaptureInitialize();
	
		//leave empty on new releases	
		//string buildDate = " (2016-07-27)";
		string buildDate = " (d)";
		label_version.Text = progVersion + buildDate;
		LogB.Information("Build date:" + buildDate);

		LeastSquares ls = new LeastSquares();
		ls.Test();
		LogB.Information(string.Format("coef = {0} {1} {2}", ls.Coef[0], ls.Coef[1], ls.Coef[2]));

		/*
		 * start a ping in other thread
		 * http://www.mono-project.com/docs/gui/gtksharp/responsive-applications/
		 * Gtk.Application.Invoke
		 */
		pingThread = new Thread (new ThreadStart (pingAtStart));
		pingThread.Start();

		//testing ChronopicRegister
		ChronopicRegisterSelectOS cros = new ChronopicRegisterSelectOS();
		ChronopicRegister cr = cros.Do();

		ChronopicRegisterWindow crw = new ChronopicRegisterWindow();
		
		//moveStartTestInitial();
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
			label_current_person_title.Text=Catalog.GetString("Current person");
		} else {
			//frame_test_options.BoxChild.Expand(false);
			label_current_person_title.Text=Catalog.GetString("Current");
		}
	}
*/

	private void formatModeMenu() 
	{
		((Label) radio_menuitem_mode_jumps_simple.Child).Text = 
			"   " + ((Label) radio_menuitem_mode_jumps_simple.Child).Text;
		((Label) radio_menuitem_mode_jumps_reactive.Child).Text = 
			"   " + ((Label) radio_menuitem_mode_jumps_reactive.Child).Text;

		((Label) radio_menuitem_mode_runs_simple.Child).Text = 
			"   " + ((Label) radio_menuitem_mode_runs_simple.Child).Text;
		((Label) radio_menuitem_mode_runs_intervallic.Child).Text = 
			"   " + ((Label) radio_menuitem_mode_runs_intervallic.Child).Text;
		
		((Label) radio_menuitem_mode_power_gravitatory.Child).Text = 
			"   " + ((Label) radio_menuitem_mode_power_gravitatory.Child).Text;
		((Label) radio_menuitem_mode_power_inertial.Child).Text = 
			"   " + ((Label) radio_menuitem_mode_power_inertial.Child).Text;
	}

	private void putNonStandardIcons() {
		Pixbuf pixbuf;
	
		//change colors of tests mode

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
		
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderGravitatory);
		image_mode_encoder_gravitatory.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameEncoderInertial);
		image_mode_encoder_inertial.Pixbuf = pixbuf;
	
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

		//persons buttons
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameNew1);
		image_persons_new_1.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameNewPlus);
		image_persons_new_plus.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameOpen1);
		image_persons_open_1.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameOpenPlus);
		image_persons_open_plus.Pixbuf = pixbuf;



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
		image_delete_this_test.Pixbuf = pixbuf;
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

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameLogo);
		image_logo.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameLogo320); //changed to 270 for the presentation
		image_presentation_logo.Pixbuf = pixbuf;

	}

	private void loadPreferences () 
	{
		preferences = Preferences.LoadAllFromSqlite();

		LogB.Information (string.Format(Catalog.GetString("Chronojump database version file: {0}"), 
					preferences.databaseVersion));

		checkbutton_allow_finish_rj_after_time.Active = preferences.allowFinishRjAfterTime;

		//---- video ----

		UtilGtk.ColorsCheckOnlyPrelight(checkbutton_video);
		UtilGtk.ColorsCheckOnlyPrelight(checkbutton_video_encoder);
		
		//don't raise the signal	
		checkbutton_video.Clicked -= new EventHandler(on_checkbutton_video_clicked);
		checkbutton_video.Active = preferences.videoOn;
		checkbutton_video.Clicked += new EventHandler(on_checkbutton_video_clicked);
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


	public void on_radio_mode_reaction_times_small_toggled (object obj, EventArgs args) {
		if(radio_mode_reaction_times_small.Active) {
			notebooks_change(4);
			on_extra_window_reaction_times_test_changed(obj, args);
			hbox_results_legend.Visible = true;
		}
	}

	public void on_radio_mode_pulses_small_toggled (object obj, EventArgs args) {
		if(radio_mode_pulses_small.Active) {
			notebooks_change(5);
			on_extra_window_pulses_test_changed(obj, args);
			hbox_results_legend.Visible = false;
		}
	}

	public void on_radio_mode_multi_chronopic_small_toggled (object obj, EventArgs args) {
		if(radio_mode_multi_chronopic_small.Active) {
			notebooks_change(6);
			on_extra_window_multichronopic_test_changed(obj, args);
			hbox_results_legend.Visible = false;
		}
	}
	
	public void on_radio_mode_encoder_capture_small_toggled (object obj, EventArgs args) {
		if(radio_mode_encoder_capture_small.Active) 
			notebook_encoder_sup.CurrentPage = 0;
	}

	public void on_radio_mode_encoder_analyze_small_toggled (object obj, EventArgs args) {
		if(radio_mode_encoder_analyze_small.Active) 
			notebook_encoder_sup.CurrentPage = 1;
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

	private void on_treeview_button_release_event (object o, ButtonReleaseEventArgs args) {
		Gdk.EventButton e = args.Event;
		Gtk.TreeView myTv = (Gtk.TreeView) o;
		if (e.Button == 3) {
			if(myTv == treeview_persons) {
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
		myTreeViewPersons = new TreeViewPersons( tv );
		tv.Selection.Changed += onTreeviewPersonsSelectionEntry;
	}

	private void fillTreeView_persons () {
		ArrayList myPersons = SqlitePersonSession.SelectCurrentSessionPersons(
				currentSession.UniqueID, 
				false); //means: do not returnPersonAndPSlist

		if(myPersons.Count > 0) {
			//fill treeview
			myTreeViewPersons.Fill(myPersons);
		}
	}

	private void on_treeview_persons_up (object o, EventArgs args) {
		myTreeViewPersons.SelectPreviousRow(currentPerson.UniqueID);
	}
	
	private void on_treeview_persons_down (object o, EventArgs args) {
		myTreeViewPersons.SelectNextRow(currentPerson.UniqueID);
	}
	
	//return true if selection is done (there's any person)
	private bool selectRowTreeView_persons(Gtk.TreeView tv, TreeStore store, int rowNum) 
	{
		myTreeViewPersons.SelectRow(rowNum);
		
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
		
			return true;
		} else {
			return false;
		}
	}

	void label_person_change() {
		label_current_person.Text = "<b>" + currentPerson.Name + "</b>"; 
		label_current_person.UseMarkup = true; 
		label_encoder_person_name.Text = "<b>" + currentPerson.Name + "</b>"; 
		label_encoder_person_name.UseMarkup = true; 
	}
	
	private void treeview_persons_storeReset() {
		myTreeViewPersons.RemoveColumns();
		myTreeViewPersons = new TreeViewPersons(treeview_persons);
	}
	
	//private void on_treeview_persons_cursor_changed (object o, EventArgs args) {
	private void onTreeviewPersonsSelectionEntry (object o, EventArgs args) {
		TreeModel model;
		TreeIter iter;

		// you get the iter and the model if something is selected
		if (((TreeSelection)o).GetSelected(out model, out iter)) {
			string selectedID = (string) model.GetValue (iter, 0); //ID, Name
		
			currentPerson = SqlitePerson.Select(Convert.ToInt32(selectedID));
			currentPersonSession = SqlitePersonSession.Select(Convert.ToInt32(selectedID), currentSession.UniqueID);
			label_person_change();
	
			personChanged();	
		}
	}

	private void personChanged() {
		//1) change on jumps, runs, pulse capture graph
		if(radio_menuitem_mode_jumps_simple.Active) {
			updateGraphJumpsSimple();

			if(notebook_capture_analyze.CurrentPage == 2) //Jumps Profile
				jumpsProfileDo(true); //calculate data
		}
		else if(radio_menuitem_mode_runs_simple.Active) 
			updateGraphRunsSimple();
		else if(radio_mode_reaction_times_small.Active) 
			updateGraphReactionTimes();
		
		//2) change on encoder
		encoderPersonChanged();
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
		

	private void resetAllTreeViews( bool alsoPersons) {
		if(alsoPersons) {
			//load the persons treeview
			treeview_persons_storeReset();
			fillTreeView_persons();
		}

		//Leave SQL opened in all this process
		Sqlite.Open(); // ------------------------------

		//load the jumps treeview
		treeview_jumps_storeReset();
		fillTreeView_jumps(Constants.AllJumpsName, true);

		//load the jumps_rj treeview_rj
		treeview_jumps_rj_storeReset();
		fillTreeView_jumps_rj(Constants.AllJumpsName, true);

		//load the runs treeview
		treeview_runs_storeReset();
		fillTreeView_runs(Constants.AllRunsName, true);

		//load the runs_interval treeview
		treeview_runs_interval_storeReset();
		fillTreeView_runs_interval(Constants.AllRunsName, true);

		//load the pulses treeview
		treeview_pulses_storeReset();
		fillTreeView_pulses(Constants.AllPulsesName, true);

		//load the reaction_times treeview
		treeview_reaction_times_storeReset();
		fillTreeView_reaction_times("reactionTime", true);

		//load the multiChronopic treeview
		treeview_multi_chronopic_storeReset(true);
		fillTreeView_multi_chronopic(true);
		

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
	
		//show or hide help_power and help_stiffness depending on preferences
		button_jumps_result_help_power.Visible = preferences.showPower;
		button_jumps_result_help_stiffness.Visible = preferences.showStiffness;
	}

	private void fillTreeView_jumps (string filter) {
		fillTreeView_jumps(filter, false);
	}
	private void fillTreeView_jumps (string filter, bool dbconOpened) {
		string [] myJumps;
	
		myJumps = SqliteJump.SelectJumps(dbconOpened, currentSession.UniqueID, -1, "", "",
				Sqlite.Orders_by.DEFAULT, -1);

		myTreeViewJumps.Fill(myJumps, filter);

		expandOrMinimizeTreeView((TreeViewEvent) myTreeViewJumps, treeview_jumps);
	}

	private void on_button_jumps_zoom_clicked (object o, EventArgs args) {
		myTreeViewJumps.ExpandState = myTreeViewJumps.ZoomChange(myTreeViewJumps.ExpandState);
		if(myTreeViewJumps.ExpandState == TreeViewEvent.ExpandStates.MINIMIZED)
			treeview_jumps.CollapseAll();
		else
			treeview_jumps.ExpandAll();
	}
	
	private void treeview_jumps_storeReset() {
		myTreeViewJumps.RemoveColumns();
		
		myTreeViewJumps = new TreeViewJumps(treeview_jumps, preferences, myTreeViewJumps.ExpandState);
	}

	private void on_treeview_jumps_cursor_changed (object o, EventArgs args) {
		LogB.Information("Cursor changed");
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
	
		//show or hide help_power and help_stiffness depending on preferences
		button_jumps_rj_result_help_power.Visible = preferences.showPower;
		button_jumps_rj_result_help_stiffness.Visible = preferences.showStiffness;
	}

	private void fillTreeView_jumps_rj (string filter) {
		fillTreeView_jumps_rj (filter, false);
	}
	private void fillTreeView_jumps_rj (string filter, bool dbconOpened) {
		string [] myJumps;
		myJumps = SqliteJumpRj.SelectJumps(dbconOpened, currentSession.UniqueID, -1, "", "");
		myTreeViewJumpsRj.Fill(myJumps, filter);

		expandOrMinimizeTreeView((TreeViewEvent) myTreeViewJumpsRj, treeview_jumps_rj);

	}

	private void on_button_jumps_rj_zoom_clicked (object o, EventArgs args) {
		myTreeViewJumpsRj.ExpandState = myTreeViewJumpsRj.ZoomChange(myTreeViewJumpsRj.ExpandState);
		if(myTreeViewJumpsRj.ExpandState == TreeViewEvent.ExpandStates.MINIMIZED)
			treeview_jumps_rj.CollapseAll();
		else if(myTreeViewJumpsRj.ExpandState == TreeViewEvent.ExpandStates.OPTIMAL) {
			treeview_jumps_rj.CollapseAll();
			myTreeViewJumpsRj.ExpandOptimal();
		} else
			treeview_jumps_rj.ExpandAll();
	}

	private void treeview_jumps_rj_storeReset() {
		myTreeViewJumpsRj.RemoveColumns();
		myTreeViewJumpsRj = new TreeViewJumpsRj (treeview_jumps_rj, preferences, myTreeViewJumpsRj.ExpandState);
	}

	private void on_treeview_jumps_rj_cursor_changed (object o, EventArgs args) {
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
	private void fillTreeView_runs (string filter, bool dbconOpened) {
		string [] myRuns = SqliteRun.SelectRuns(dbconOpened, currentSession.UniqueID, -1, "",
				Sqlite.Orders_by.DEFAULT, -1);

		myTreeViewRuns.Fill(myRuns, filter);

		expandOrMinimizeTreeView((TreeViewEvent) myTreeViewRuns, treeview_runs);
	}
	
	private void on_button_runs_zoom_clicked (object o, EventArgs args) {
		myTreeViewRuns.ExpandState = myTreeViewRuns.ZoomChange(myTreeViewRuns.ExpandState);
		if(myTreeViewRuns.ExpandState == TreeViewEvent.ExpandStates.MINIMIZED)
			treeview_runs.CollapseAll();
		else
			treeview_runs.ExpandAll();
	}
	
	private void treeview_runs_storeReset() {
		myTreeViewRuns.RemoveColumns();
		myTreeViewRuns = new TreeViewRuns(treeview_runs, preferences.digitsNumber, preferences.metersSecondsPreferred, myTreeViewRuns.ExpandState);
	}

	private void on_treeview_runs_cursor_changed (object o, EventArgs args) {
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
	private void fillTreeView_runs_interval (string filter, bool dbconOpened) {
		string [] myRuns = SqliteRunInterval.SelectRuns(dbconOpened, currentSession.UniqueID, -1, "");
		myTreeViewRunsInterval.Fill(myRuns, filter);
		expandOrMinimizeTreeView((TreeViewEvent) myTreeViewRunsInterval, treeview_runs_interval);
	}
	
	private void on_button_runs_interval_zoom_clicked (object o, EventArgs args) {
		myTreeViewRunsInterval.ExpandState = myTreeViewRunsInterval.ZoomChange(myTreeViewRunsInterval.ExpandState);
		if(myTreeViewRunsInterval.ExpandState == TreeViewEvent.ExpandStates.MINIMIZED)
			treeview_runs_interval.CollapseAll();
		else if(myTreeViewRunsInterval.ExpandState == TreeViewEvent.ExpandStates.OPTIMAL) {
			treeview_runs_interval.CollapseAll();
			myTreeViewRunsInterval.ExpandOptimal();
		} else
			treeview_runs_interval.ExpandAll();
	}

	private void treeview_runs_interval_storeReset() {
		myTreeViewRunsInterval.RemoveColumns();
		myTreeViewRunsInterval = new TreeViewRunsInterval (treeview_runs_interval,  
				preferences.digitsNumber, preferences.metersSecondsPreferred, myTreeViewRunsInterval.ExpandState);
	}

	private void on_treeview_runs_interval_cursor_changed (object o, EventArgs args) {
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
	private void fillTreeView_reaction_times (string filter, bool dbconOpened) {
		string [] myRTs = SqliteReactionTime.SelectReactionTimes(dbconOpened, currentSession.UniqueID, -1, "", 
				Sqlite.Orders_by.DEFAULT, -1);

		myTreeViewReactionTimes.Fill(myRTs, filter);
		expandOrMinimizeTreeView((TreeViewEvent) myTreeViewReactionTimes, treeview_reaction_times);
	}
	
	private void on_button_reaction_times_zoom_clicked (object o, EventArgs args) {
		myTreeViewReactionTimes.ExpandState = myTreeViewReactionTimes.ZoomChange(
				myTreeViewReactionTimes.ExpandState);
		if(myTreeViewReactionTimes.ExpandState == TreeViewEvent.ExpandStates.MINIMIZED)
			treeview_reaction_times.CollapseAll();
		else
			treeview_reaction_times.ExpandAll();
	}
	
	private void treeview_reaction_times_storeReset() {
		myTreeViewReactionTimes.RemoveColumns();
		myTreeViewReactionTimes = new TreeViewReactionTimes( treeview_reaction_times, preferences.digitsNumber, myTreeViewReactionTimes.ExpandState );
	}

	private void on_treeview_reaction_times_cursor_changed (object o, EventArgs args) {
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
	private void fillTreeView_pulses (string filter, bool dbconOpened) {
		string [] myPulses = SqlitePulse.SelectPulses(dbconOpened, currentSession.UniqueID, -1);
		myTreeViewPulses.Fill(myPulses, filter);
		expandOrMinimizeTreeView((TreeViewEvent) myTreeViewPulses, treeview_pulses);
	}
	
	private void on_button_pulses_zoom_clicked (object o, EventArgs args) {
		myTreeViewPulses.ExpandState = myTreeViewPulses.ZoomChange(myTreeViewPulses.ExpandState);
		if(myTreeViewPulses.ExpandState == TreeViewEvent.ExpandStates.MINIMIZED)
			treeview_pulses.CollapseAll();
		else if(myTreeViewPulses.ExpandState == TreeViewEvent.ExpandStates.OPTIMAL) {
			treeview_pulses.CollapseAll();
			myTreeViewPulses.ExpandOptimal();
		} else
			treeview_pulses.ExpandAll();
	}

	private void treeview_pulses_storeReset() {
		myTreeViewPulses.RemoveColumns();
		myTreeViewPulses = new TreeViewPulses( treeview_pulses, preferences.digitsNumber, myTreeViewPulses.ExpandState );
	}

	private void on_treeview_pulses_cursor_changed (object o, EventArgs args) {
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
	private void fillTreeView_multi_chronopic (bool dbconOpened) {
		string [] mcs = SqliteMultiChronopic.SelectTests(dbconOpened, currentSession.UniqueID, -1);
		myTreeViewMultiChronopic.Fill(mcs, "");
		expandOrMinimizeTreeView((TreeViewEvent) myTreeViewMultiChronopic, treeview_multi_chronopic);
	}
	
	private void on_button_multi_chronopic_zoom_clicked (object o, EventArgs args) {
		myTreeViewMultiChronopic.ExpandState = myTreeViewMultiChronopic.ZoomChange(myTreeViewMultiChronopic.ExpandState);
		if(myTreeViewMultiChronopic.ExpandState == TreeViewEvent.ExpandStates.MINIMIZED)
			treeview_multi_chronopic.CollapseAll();
		else if(myTreeViewMultiChronopic.ExpandState == TreeViewEvent.ExpandStates.OPTIMAL) {
			treeview_multi_chronopic.CollapseAll();
			myTreeViewMultiChronopic.ExpandOptimal();
		} else
			treeview_multi_chronopic.ExpandAll();
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

	private void on_treeview_multi_chronopic_cursor_changed (object o, EventArgs args) {
		LogB.Information("Cursor changed");
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
	
	string [] selectJumpsString;
	string [] selectJumpsRjString;
	string [] selectRunsString;
	string [] selectRunsIntervalString;

	// ---------------- combo_select ----------------------

	private void createComboSelectJumps(bool create) {
		if(create)
			combo_select_jumps = ComboBox.NewText ();

		string [] jumpTypes = SqliteJumpType.SelectJumpTypes(false, "", "", false); //without alljumpsname, without filter, not only name
		selectJumpsString = new String [jumpTypes.Length];
		string [] jumpNamesToCombo = new String [jumpTypes.Length];
		int i =0;
		foreach(string jumpType in jumpTypes) {
			string [] j = jumpType.Split(new char[] {':'});
			string nameTranslated = Catalog.GetString(j[1]);
			selectJumpsString[i] = 
				j[0] + ":" + j[1] + ":" + nameTranslated + ":" +	//uniqueID, name, nameTranslated
				j[2] + ":" + j[3] + ":" + j[4];				//startIn, weight, description
			jumpNamesToCombo[i] = nameTranslated;
			i++;
		}

		UtilGtk.ComboUpdate(combo_select_jumps, jumpNamesToCombo, "");
		combo_select_jumps.Active = 0;
		combo_select_jumps.Changed += new EventHandler (on_combo_select_jumps_changed);

		if(create) {
			hbox_combo_select_jumps.PackStart(combo_select_jumps, true, true, 0);
			hbox_combo_select_jumps.ShowAll();
			combo_select_jumps.Sensitive = false;
		}
	}
	
	private void createComboSelectJumpsRj(bool create) {
		if(create)
			combo_select_jumps_rj = ComboBox.NewText ();

		string [] jumpTypes = SqliteJumpType.SelectJumpRjTypes("", false); //without alljumpsname, not only name
		selectJumpsRjString = new String [jumpTypes.Length];
		string [] jumpNamesToCombo = new String [jumpTypes.Length];
		int i =0;
		foreach(string jumpType in jumpTypes) {
			string [] j = jumpType.Split(new char[] {':'});
			string nameTranslated = Catalog.GetString(j[1]);
			selectJumpsRjString[i] = 
				j[0] + ":" + j[1] + ":" + nameTranslated + ":" +	//uniqueID, name, nameTranslated
				j[2] + ":" + j[3] + ":" + j[4] + ":" +			//startIn, weight, jumpsLimited
				j[5] + ":" + j[6];					//fixedValue, description
			jumpNamesToCombo[i] = nameTranslated;
			i++;
		}

		UtilGtk.ComboUpdate(combo_select_jumps_rj, jumpNamesToCombo, "");
		combo_select_jumps_rj.Active = 0;
		combo_select_jumps_rj.Changed += new EventHandler (on_combo_select_jumps_rj_changed);

		if(create) {
			hbox_combo_select_jumps_rj.PackStart(combo_select_jumps_rj, true, true, 0);
			hbox_combo_select_jumps_rj.ShowAll();
			combo_select_jumps_rj.Sensitive = false;
		}
	}
	
	private void createComboSelectRuns(bool create) {
		if(create)
			combo_select_runs = ComboBox.NewText ();

		string [] runTypes = SqliteRunType.SelectRunTypes("", false); //without allrunsname, not only name
		selectRunsString = new String [runTypes.Length];
		string [] runNamesToCombo = new String [runTypes.Length];
		int i =0;
		foreach(string runType in runTypes) {
			string [] j = runType.Split(new char[] {':'});
			string nameTranslated = Catalog.GetString(j[1]);
			selectRunsString[i] = 
				j[0] + ":" + j[1] + ":" + nameTranslated + ":" +	//uniqueID, name, nameTranslated
				j[2] + ":" + j[3];					//distance, description
			runNamesToCombo[i] = nameTranslated;
			i++;
		}

		UtilGtk.ComboUpdate(combo_select_runs, runNamesToCombo, "");
		combo_select_runs.Active = 0;
		combo_select_runs.Changed += new EventHandler (on_combo_select_runs_changed);

		if(create) {
			hbox_combo_select_runs.PackStart(combo_select_runs, true, true, 0);
			hbox_combo_select_runs.ShowAll();
			combo_select_runs.Sensitive = false;
		}
	}

	private void createComboSelectRunsInterval(bool create) {
		if(create)
			combo_select_runs_interval = ComboBox.NewText ();

		string [] runTypes = SqliteRunIntervalType.SelectRunIntervalTypes("", false); //without allrunsname, not only name
		selectRunsIntervalString = new String [runTypes.Length];
		string [] runNamesToCombo = new String [runTypes.Length];
		int i =0;
		foreach(string runType in runTypes) {
			string [] j = runType.Split(new char[] {':'});
			string nameTranslated = Catalog.GetString(j[1]);
			selectRunsIntervalString[i] = 
				j[0] + ":" + j[1] + ":" + nameTranslated + ":" +	//uniqueID, name, nameTranslated
				j[2] + ":" + j[3] + ":" + j[4] + ":" +			//distance, tracksLimited, fixedValue
				j[5] + ":" + j[6] + ":" + j[6];				//unlimited, description, distancesString
			runNamesToCombo[i] = nameTranslated;
			i++;
		}

		UtilGtk.ComboUpdate(combo_select_runs_interval, runNamesToCombo, "");
		combo_select_runs_interval.Active = 0;
		combo_select_runs_interval.Changed += new EventHandler (on_combo_select_runs_interval_changed);

		if(create) {
			hbox_combo_select_runs_interval.PackStart(combo_select_runs_interval, true, true, 0);
			hbox_combo_select_runs_interval.ShowAll();
			combo_select_runs_interval.Sensitive = false;
		}
	}


	// ---------------- combo_result ----------------------

	private void createComboResultJumps() {
		combo_result_jumps = ComboBox.NewText ();
		UtilGtk.ComboUpdate(combo_result_jumps,
				SqliteJumpType.SelectJumpTypes(false, Constants.AllJumpsName, "", true), //with alljumpsname, without filter, only select name
			       	"");
		
		combo_result_jumps.Active = 0;
		combo_result_jumps.Changed += new EventHandler (on_combo_result_jumps_changed);

		hbox_combo_result_jumps.PackStart(combo_result_jumps, true, true, 0);
		hbox_combo_result_jumps.ShowAll();
		combo_result_jumps.Sensitive = false;
	}
	
	private void createComboResultJumpsRj() {
		combo_result_jumps_rj = ComboBox.NewText();
		UtilGtk.ComboUpdate(combo_result_jumps_rj, SqliteJumpType.SelectJumpRjTypes(Constants.AllJumpsName, true), ""); //only select name
		
		combo_result_jumps_rj.Active = 0;
		combo_result_jumps_rj.Changed += new EventHandler (on_combo_result_jumps_rj_changed);

		hbox_combo_result_jumps_rj.PackStart(combo_result_jumps_rj, true, true, 0);
		hbox_combo_result_jumps_rj.ShowAll();
		combo_result_jumps_rj.Sensitive = false;
	}
	
	private void createComboResultRuns() {
		combo_result_runs = ComboBox.NewText();
		UtilGtk.ComboUpdate(combo_result_runs, SqliteRunType.SelectRunTypes(Constants.AllRunsName, true), ""); //without filter, only select name
		
		combo_result_runs.Active = 0;
		combo_result_runs.Changed += new EventHandler (on_combo_result_runs_changed);

		hbox_combo_result_runs.PackStart(combo_result_runs, true, true, 0);
		hbox_combo_result_runs.ShowAll();
		combo_result_runs.Sensitive = false;
	}

	private void createComboResultRunsInterval() {
		combo_result_runs_interval = ComboBox.NewText();
		UtilGtk.ComboUpdate(combo_result_runs_interval, SqliteRunIntervalType.SelectRunIntervalTypes(Constants.AllRunsName, true), ""); //without filter, only select name
		
		combo_result_runs_interval.Active = 0;
		combo_result_runs_interval.Changed += new EventHandler (on_combo_result_runs_interval_changed);

		hbox_combo_result_runs_interval.PackStart(combo_result_runs_interval, true, true, 0);
		hbox_combo_result_runs_interval.ShowAll();
		combo_result_runs_interval.Sensitive = false;
	}
	
	//no need of reationTimes

	private void createComboPulses() {
		combo_pulses = ComboBox.NewText();
		UtilGtk.ComboUpdate(combo_pulses, SqlitePulseType.SelectPulseTypes(Constants.AllPulsesName, true), ""); //without filter, only select name
		
		combo_pulses.Active = 0;
		combo_pulses.Changed += new EventHandler (on_combo_pulses_changed);

		hbox_combo_pulses.PackStart(combo_pulses, true, true, 0);
		hbox_combo_pulses.ShowAll();
		combo_pulses.Sensitive = false;
	}

	/*
	private void createComboMultiChronopic() 
	{
		button_multi_chronopic_start.Sensitive = false;
		button_run_analysis.Sensitive = false;
		extra_window_spin_run_analysis_distance.Sensitive = false;
	}
	*/


	// -------------- combo select tests changed --------

	private void on_combo_select_jumps_changed(object o, EventArgs args) {
		ComboBox combo = o as ComboBox;
		if (o == null)
			return;
		string myText = UtilGtk.ComboGetActive(combo);
		LogB.Information("Selected: " + myText); 

		//show extra window options
		on_extra_window_jumps_test_changed(o, args);
	}
	
	private void on_combo_select_jumps_rj_changed(object o, EventArgs args) {
		ComboBox combo = o as ComboBox;
		if (o == null)
			return;
		string myText = UtilGtk.ComboGetActive(combo);
		LogB.Information("Selected: " + myText); 

		//show extra window options
		on_extra_window_jumps_rj_test_changed(o, args);
	}
	
	private void on_combo_select_runs_changed(object o, EventArgs args) {
		ComboBox combo = o as ComboBox;
		if (o == null)
			return;
		string myText = UtilGtk.ComboGetActive(combo);
		LogB.Information("Selected: " + myText); 

		//show extra window options
		on_extra_window_runs_test_changed(o, args);
	}
	
	private void on_combo_select_runs_interval_changed(object o, EventArgs args) {
		ComboBox combo = o as ComboBox;
		if (o == null)
			return;
		string myText = UtilGtk.ComboGetActive(combo);
		LogB.Information("Selected: " + myText); 

		//show extra window options
		on_extra_window_runs_interval_test_changed(o, args);
	}
	
	// -------------- combo result tests changed --------
	
	private void on_combo_result_jumps_changed(object o, EventArgs args) {
		//combo_result_jumps.Changed -= new EventHandler (on_combo_result_jumps_changed);

		ComboBox combo = o as ComboBox;
		if (o == null)
			return;
		string myText = UtilGtk.ComboGetActive(combo);

		treeview_jumps_storeReset();
		fillTreeView_jumps(myText);
	}
	
	
	private void on_combo_result_jumps_rj_changed(object o, EventArgs args) {
		//combo_result_jumps_rj.Changed -= new EventHandler (on_combo_result_jumps_rj_changed);

		ComboBox combo = o as ComboBox;
		if (o == null)
			return;
		string myText = UtilGtk.ComboGetActive(combo);

		treeview_jumps_rj_storeReset();
		fillTreeView_jumps_rj(myText);
	}

	private void on_combo_result_runs_changed(object o, EventArgs args) {
		//combo_result_runs.Changed -= new EventHandler (on_combo_result_runs_changed);

		ComboBox combo = o as ComboBox;
		if (o == null)
			return;
		string myText = UtilGtk.ComboGetActive(combo);

		treeview_runs_storeReset();
		fillTreeView_runs(myText);
	}

	private void on_combo_result_runs_interval_changed(object o, EventArgs args) {
		//combo_result_runs_interval.Changed -= new EventHandler (on_combo_result_runs_interval_changed);

		ComboBox combo = o as ComboBox;
		if (o == null)
			return;
		string myText = UtilGtk.ComboGetActive(combo);

		treeview_runs_interval_storeReset();
		fillTreeView_runs_interval(myText);
	}

	//no need of reationTimes because is done in:
	//gui/reactionTime on_extra_window_reaction_times_test_changed()
	
	private void on_combo_pulses_changed(object o, EventArgs args) {
		//combo_pulses.Changed -= new EventHandler (on_combo_pulses_changed);

		ComboBox combo = o as ComboBox;
		if (o == null)
			return;
		string myText = UtilGtk.ComboGetActive(combo);

		treeview_pulses_storeReset();
		fillTreeView_pulses(myText);
	}


	/* ---------------------------------------------------------
	 * ----------------  DELETE EVENT, QUIT  -----------------------
	 *  --------------------------------------------------------
	 */
	
	private void on_delete_event (object o, DeleteEventArgs args) {
		args.RetVal = true;
		
		//cannot terminate chronojump untile press 'cancel' if  autodetect encoder is working
		if(cpDetect != null && cpDetect.Detecting == true)
			return;
    
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
		on_quit2_activate(new object(), new EventArgs());
	}
		

	private void on_quit2_activate (object o, EventArgs args) {
		LogB.Information("Bye!");

		if(chronopicWin.Connected == true) {
			chronopicWin.SerialPortsCloseIfNeeded();
		}
	
		try {	
			File.Delete(runningFileName);
		} catch {
			new DialogMessage(Constants.MessageTypes.WARNING, 
					string.Format(Catalog.GetString("Could not delete file:\n{0}"), runningFileName));
		}
		
		if(File.Exists(Util.GetDatabaseTempDir() + System.IO.Path.DirectorySeparatorChar + "chronojump.db"))
			File.Move(Util.GetDatabaseTempDir() + System.IO.Path.DirectorySeparatorChar + "chronojump.db",
				Util.GetDatabaseDir() + System.IO.Path.DirectorySeparatorChar + "chronojump.db");
		
		LogB.Information("Bye2!");
		
		encoderRProcCapture.SendEndProcess();
		encoderRProcAnalyze.SendEndProcess();

		LogB.Information("Bye3!");

		//exit start ping if has not ended
		if(pingThread.IsAlive)
			pingThread.Abort();
		
		Log.End();

		Application.Quit();
		
		//Environment.Exit(Environment.ExitCode);
	}
	
	/* ---------------------------------------------------------
	 * ----------------  SESSION NEW, LOAD, EXPORT, DELETE -----
	 *  --------------------------------------------------------
	 */

	private void on_new_activate (object o, EventArgs args) {
		LogB.Information("new session");
		sessionAddEditWin = SessionAddEditWindow.Show(app1, new Session());
		sessionAddEditWin.FakeButtonAccept.Clicked -= new EventHandler(on_new_session_accepted);
		sessionAddEditWin.FakeButtonAccept.Clicked += new EventHandler(on_new_session_accepted);
	}
	
	private void on_new_session_accepted (object o, EventArgs args) {
		if(sessionAddEditWin.CurrentSession != null) 
		{
			currentSession = sessionAddEditWin.CurrentSession;
			sessionAddEditWin.HideAndNull();

			//serverUniqueID is undefined until session is updated
			currentSession.ServerUniqueID = Constants.ServerUndefinedID;

			app1.Title = progName + " - " + currentSession.Name;

			if(createdStatsWin) {
				stats_win_initializeSession();
			}
		
			resetAllTreeViews(true); //boolean means: "also persons"

			//show hidden widgets
			sensitiveGuiNoSession();
			sensitiveGuiYesSession();
			definedSession = true;

			//for sure, jumpsExists is false, because we create a new session

			button_edit_current_person.Sensitive = false;
			button_show_all_person_events.Sensitive = false;
			button_delete_current_person.Sensitive = false;
		
			//update report
			report.SessionID = currentSession.UniqueID;
			report.StatisticsRemove();
			try {
				reportWin.FillTreeView();
			} catch {} //reportWin is still not created, not need to Fill again
	
			//feedback (more in 1st session created)
			string feedbackLoadUsers = Catalog.GetString ("Session created, now add or load persons.");
			new DialogMessage(Constants.MessageTypes.INFO, feedbackLoadUsers);
		}
	}
	
	private void on_edit_session_activate (object o, EventArgs args) 
	{
		LogB.Information("edit session");
		
		if(currentSession.Name == Constants.SessionSimulatedName)
			new DialogMessage(Constants.MessageTypes.INFO, Constants.SessionProtected);
		else {
			sessionAddEditWin = SessionAddEditWindow.Show(app1, currentSession);
			sessionAddEditWin.FakeButtonAccept.Clicked -= new EventHandler(on_edit_session_accepted);
			sessionAddEditWin.FakeButtonAccept.Clicked += new EventHandler(on_edit_session_accepted);
		}
	}
	
	private void on_edit_session_accepted (object o, EventArgs args) {
		if(sessionAddEditWin.CurrentSession != null) 
		{
			currentSession = sessionAddEditWin.CurrentSession;
			sessionAddEditWin.HideAndNull();
			
			app1.Title = progName + " - " + currentSession.Name;

			if(createdStatsWin) {
				stats_win_initializeSession();
			}
		}
	}

	private void on_button_encoder_import_chronojump_session(object o, EventArgs args)
	{
		sessionLoadWin = SessionLoadWindow.Show (app1, SessionLoadWindow.WindowType.IMPORT_SESSION);
		sessionLoadWin.Button_accept.Clicked += new EventHandler(on_load_session_accepted_to_import);
	}

	//from import session
	private void on_load_session_accepted_to_import(object o, EventArgs args)
	{
		int sessionNumber = sessionLoadWin.CurrentSessionId();
		string databasePath = sessionLoadWin.DatabasePath();
		LogB.Information (databasePath);

		ImportSessionFromDatabase (databasePath, sessionNumber);
	}

	private void ImportSessionFromDatabase(string databasePath, int sessionNumber)
	{
		string source_filename = databasePath;
		string destination_filename = Sqlite.DatabaseFilePath;
		string session = Convert.ToString (sessionNumber);

		ChronojumpImporter chronojumpImporter = new ChronojumpImporter (source_filename, destination_filename, session);

		ChronojumpImporter.Result result = chronojumpImporter.import ();

		if (result.success) {
			//update stats combos
			updateComboStats ();

			new DialogMessage (Constants.MessageTypes.INFO, Catalog.GetString ("Session imported."));
		} else {
			LogB.Debug ("Chronojump Importer error: ", result.error);
			new DialogMessage (Constants.MessageTypes.WARNING, result.error);
		}
	}

	private void on_open_activate (object o, EventArgs args) 
	{
		LogB.Information("open session");
		sessionLoadWin = SessionLoadWindow.Show(app1, SessionLoadWindow.WindowType.LOAD_SESSION);
		sessionLoadWin.Button_accept.Clicked += new EventHandler(on_load_session_accepted_from_open);
	}

	//from open session
	private void on_load_session_accepted_from_open (object o, EventArgs args) 
	{
		currentSession = sessionLoadWin.CurrentSession;
		on_load_session_accepted();
	}
	//called from open session OR from gui/networks configInit when config.SessionMode == Config.SessionModeEnum.UNIQUE
	private void on_load_session_accepted () 
	{
		app1.Title = progName + " - " + currentSession.Name;
	
		if(createdStatsWin) {
			stats_win_initializeSession();
		}
		
		resetAllTreeViews(true); //boolean means: "also persons"

		bool foundPersons = selectRowTreeView_persons(treeview_persons, treeview_persons_store, 0);
		
		//show hidden widgets
		sensitiveGuiNoSession();
		sensitiveGuiYesSession();
		definedSession = true;
		
		button_edit_current_person.Sensitive = false;
		button_show_all_person_events.Sensitive = false;
		button_delete_current_person.Sensitive = false;

		//if there are persons
		if(foundPersons) {
			//activate the gui for persons in main window
			sensitiveGuiYesPerson();
		}

		//update report
		report.SessionID = currentSession.UniqueID;
		report.StatisticsRemove();

		if(reportWin != null)
			reportWin.FillTreeView();
		
		chronojumpWindowTestsNext();
	}
	
	
	private void on_delete_session_activate (object o, EventArgs args) 
	{
		LogB.Information("--- delete session ---");
		
		if(currentSession.Name == Constants.SessionSimulatedName)
			new DialogMessage(Constants.MessageTypes.INFO, Constants.SessionProtected);
		else {
			ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString("Are you sure you want to delete the current session"), "", Catalog.GetString("and all the session tests?"));
			confirmWin.Button_accept.Clicked += new EventHandler(on_delete_session_accepted);
		}
	}
	
	private void on_delete_session_accepted (object o, EventArgs args) 
	{
		new DialogMessage(Constants.MessageTypes.INFO, Catalog.GetString("Deleted session and all its tests."));
		SqliteSession.DeleteAllStuff(currentSession.UniqueID.ToString());
		
		sensitiveGuiNoSession();
		definedSession = false;
		app1.Title = progName + "";
	}

	
	private void on_export_session_activate(object o, EventArgs args) {
		ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString("Encoder data will not be exported."), "", "");
		confirmWin.Button_accept.Clicked += new EventHandler(on_export_session_accepted);
	}

	private void on_export_session_accepted(object o, EventArgs args) {
		new ExportSessionCSV(currentSession, app1, preferences);
	}

	
	/* ---------------------------------------------------------
	 * ----------------  PERSON RECUPERATE, LOAD, EDIT, DELETE -
	 *  --------------------------------------------------------
	 */
	
	private void on_recuperate_person_clicked (object o, EventArgs args) {
		LogB.Information("recuperate person");
		personRecuperateWin = PersonRecuperateWindow.Show(app1, currentSession, preferences.digitsNumber);
		personRecuperateWin.FakeButtonDone.Clicked += new EventHandler(on_recuperate_person_accepted);
	}

	private void on_recuperate_person_accepted (object o, EventArgs args) {
		LogB.Information("here!!!");
		currentPerson = personRecuperateWin.CurrentPerson;
		currentPersonSession = personRecuperateWin.CurrentPersonSession;
		label_person_change();
		
		myTreeViewPersons.Add(currentPerson.UniqueID.ToString(), currentPerson.Name);

		int rowToSelect = myTreeViewPersons.FindRow(currentPerson.UniqueID);
		if(rowToSelect != -1) {
			selectRowTreeView_persons(treeview_persons,
					treeview_persons_store, 
					rowToSelect);
			sensitiveGuiYesPerson();
		}
	}
		
	private void on_recuperate_persons_from_session_clicked (object o, EventArgs args) {
		LogB.Information("recuperate persons from other session");
		personsRecuperateFromOtherSessionWin = PersonsRecuperateFromOtherSessionWindow.Show(app1, currentSession);
		personsRecuperateFromOtherSessionWin.FakeButtonDone.Clicked += new EventHandler(on_recuperate_persons_from_session_accepted);
	}
	
	private void on_recuperate_persons_from_session_accepted (object o, EventArgs args) {
		currentPerson = personsRecuperateFromOtherSessionWin.CurrentPerson;
		currentPersonSession = personsRecuperateFromOtherSessionWin.CurrentPersonSession;
		label_person_change();

		treeview_persons_storeReset();
		fillTreeView_persons();
		int rowToSelect = myTreeViewPersons.FindRow(currentPerson.UniqueID);
		if(rowToSelect != -1) {
			selectRowTreeView_persons(treeview_persons,
					treeview_persons_store, 
					rowToSelect);
			sensitiveGuiYesPerson();
		}
	}
	
	bool person_add_single_called_from_person_select_window;
	private void on_person_add_single_from_main_gui (object o, EventArgs args) {
		person_add_single_called_from_person_select_window = false;
		person_add_single();
	}

	private void person_add_single () {
		personAddModifyWin = PersonAddModifyWindow.Show(app1, 
				currentSession, new Person(-1), 
				preferences.digitsNumber, checkbutton_video, useVideo
				);
		//-1 means we are adding a new person
		//if we were modifying it will be it's uniqueID
		
		personAddModifyWin.FakeButtonAccept.Clicked += new EventHandler(on_person_add_single_accepted);
	}
	
	private void on_person_add_single_accepted (object o, EventArgs args) {
		if (personAddModifyWin.CurrentPerson != null)
		{
			currentPerson = personAddModifyWin.CurrentPerson;
			currentPersonSession = SqlitePersonSession.Select(currentPerson.UniqueID, currentSession.UniqueID);
			label_person_change();
			myTreeViewPersons.Add(currentPerson.UniqueID.ToString(), currentPerson.Name);

			//when adding new person, photos cannot be recorded as currentPerson.UniqueID
			//because it was undefined. Copy them now
			if(File.Exists(Util.GetPhotoTempFileName(false)) && File.Exists(Util.GetPhotoTempFileName(true))) {
				try {
					File.Move(Util.GetPhotoTempFileName(false), 
							Util.GetPhotoFileName(false, currentPerson.UniqueID));
				} catch {
					File.Copy(Util.GetPhotoTempFileName(false), 
							Util.GetPhotoFileName(false, currentPerson.UniqueID), true);
				}
				try {
					File.Move(Util.GetPhotoTempFileName(true), 
							Util.GetPhotoFileName(true, currentPerson.UniqueID));
				} catch {
					File.Copy(Util.GetPhotoTempFileName(true), 
							Util.GetPhotoFileName(true, currentPerson.UniqueID), true);
				}
			}
			
			int rowToSelect = myTreeViewPersons.FindRow(currentPerson.UniqueID);
			if(rowToSelect != -1) {
				selectRowTreeView_persons(treeview_persons,
						treeview_persons_store, 
						rowToSelect);
				sensitiveGuiYesPerson();
				//appbar2.Push( 1, Catalog.GetString("Successfully added") + " " + currentPerson.Name );
			}
			
			if(person_add_single_called_from_person_select_window) {
				ArrayList myPersons = SqlitePersonSession.SelectCurrentSessionPersons(
						currentSession.UniqueID, 
						false); //means: do not returnPersonAndPSlist
				personSelectWin.Update(myPersons);
			}
		}
	}

	//show spinbutton window asking for how many people to create	
	private void on_person_add_multiple_clicked (object o, EventArgs args) {
		personAddMultipleWin = PersonAddMultipleWindow.Show(app1, currentSession);
		personAddMultipleWin.Button_accept.Clicked += new EventHandler(on_person_add_multiple_accepted);
	}
	
	private void on_person_add_multiple_accepted (object o, EventArgs args) {
		if (personAddMultipleWin.CurrentPerson != null)
		{
			currentPerson = personAddMultipleWin.CurrentPerson;
			currentPersonSession = SqlitePersonSession.Select(currentPerson.UniqueID, currentSession.UniqueID);
			label_person_change();
			treeview_persons_storeReset();
			fillTreeView_persons();
			int rowToSelect = myTreeViewPersons.FindRow(currentPerson.UniqueID);
			if(rowToSelect != -1) {
				selectRowTreeView_persons(treeview_persons,
						treeview_persons_store, 
						rowToSelect);
				sensitiveGuiYesPerson();
			
				string myString = string.Format(
						Catalog.GetPluralString(
							"Successfully added one person.", 
							"Successfully added {0} persons.", 
							personAddMultipleWin.PersonsCreatedCount),
						personAddMultipleWin.PersonsCreatedCount);
				//appbar2.Push( 1, Catalog.GetString(myString) );
			}
		}
	}
	
	bool person_edit_single_called_from_person_select_window;
	private void on_edit_current_person_clicked_from_main_gui (object o, EventArgs args) {
		person_edit_single_called_from_person_select_window = false;
		person_edit_single();
	}

	private void person_edit_single() {
		LogB.Information("modify person");

		personAddModifyWin = PersonAddModifyWindow.Show(app1, currentSession, currentPerson, 
				preferences.digitsNumber, checkbutton_video, useVideo
				); 
		personAddModifyWin.FakeButtonAccept.Clicked += new EventHandler(on_edit_current_person_accepted);
	}
	
	private void on_edit_current_person_accepted (object o, EventArgs args) {
		if (personAddModifyWin.CurrentPerson != null)
		{
			currentPerson = personAddModifyWin.CurrentPerson;
			currentPersonSession = SqlitePersonSession.Select(currentPerson.UniqueID, currentSession.UniqueID);
			label_person_change();
			treeview_persons_storeReset();
			fillTreeView_persons();
			
			int rowToSelect = myTreeViewPersons.FindRow(currentPerson.UniqueID);
			if(rowToSelect != -1) {
				selectRowTreeView_persons(treeview_persons,
						treeview_persons_store, 
						rowToSelect);
				sensitiveGuiYesPerson();
			}

			on_combo_result_jumps_changed(combo_result_jumps, args);
			on_combo_result_jumps_rj_changed(combo_result_jumps_rj, args);
			on_combo_result_runs_changed(combo_result_runs, args);
			on_combo_result_runs_interval_changed(combo_result_runs_interval, args);
			on_combo_pulses_changed(combo_pulses, args);

			if(createdStatsWin) {
				stats_win_fillTreeView_stats(false, true);
			}

//			personAddModifyWin.Destroy();
			
			if(person_edit_single_called_from_person_select_window) {
				ArrayList myPersons = SqlitePersonSession.SelectCurrentSessionPersons(
						currentSession.UniqueID, 
						false); //means: do not returnPersonAndPSlist
				personSelectWin.Update(myPersons);
			}
		}
	}

	
	private void on_show_all_person_events_activate (object o, EventArgs args) {
		PersonShowAllEventsWindow.Show(app1, currentSession.UniqueID, currentPerson);
	}
	
	
	private void on_delete_current_person_from_session_clicked (object o, EventArgs args) {
		LogB.Information("delete current person from this session");
		ConfirmWindow confirmWin = ConfirmWindow.Show(
				Catalog.GetString("Are you sure you want to delete the current person and all his/her tests (jumps, runs, pulses, ...) from this session?\n(His/her personal data and tests in other sessions will remain intact.)"), "", 
				Catalog.GetString("Current Person: ") + currentPerson.Name);

		confirmWin.Button_accept.Clicked += new EventHandler(on_delete_current_person_from_session_accepted);
	}
	
	private void on_delete_current_person_from_session_accepted (object o, EventArgs args) 
	{
		new DialogMessage(Constants.MessageTypes.INFO, Catalog.GetString("Deleted person and all his/her tests on this session."));
		SqlitePersonSession.DeletePersonFromSessionAndTests(
				currentSession.UniqueID.ToString(), currentPerson.UniqueID.ToString());
		
		resetAllTreeViews(true); //boolean means: "also persons"
		bool foundPersons = selectRowTreeView_persons(treeview_persons, treeview_persons_store, 0);
			
		if(createdStatsWin) {
			stats_win_fillTreeView_stats(false, true);
		}
		
		//if there are no persons
		if(!foundPersons) {
			sensitiveGuiNoPerson ();
			if(createdStatsWin) {
				stats_win_hide();
			}
		}
	}

	private void on_button_encoder_person_change_clicked (object o, EventArgs args) 
	{
		ArrayList myPersons = SqlitePersonSession.SelectCurrentSessionPersons(
				currentSession.UniqueID, 
				false); //means: do not returnPersonAndPSlist

		personSelectWin = PersonSelectWindow.Show(app1, myPersons);
		personSelectWin.FakeButtonAddPerson.Clicked += new EventHandler(on_button_encoder_person_add_person);
		personSelectWin.FakeButtonEditPerson.Clicked += new EventHandler(on_button_encoder_person_edit_person);
		personSelectWin.FakeButtonDeletePerson.Clicked += new EventHandler(on_button_encoder_person_delete_person);
		personSelectWin.FakeButtonDone.Clicked += new EventHandler(on_button_encoder_person_change_done);
	}
	private void on_button_encoder_person_add_person(object o, EventArgs args)
	{
		person_add_single_called_from_person_select_window = true;
		person_add_single();
	}
	private void on_button_encoder_person_edit_person(object o, EventArgs args)
	{
		currentPerson = personSelectWin.SelectedPerson; 
		
		person_edit_single_called_from_person_select_window = true;
		person_edit_single();
	}
	private void on_button_encoder_person_delete_person(object o, EventArgs args)
	{
		currentPerson = personSelectWin.SelectedPerson;
		
		//without confirm, because it's already confirmed on PersonSelect
		on_delete_current_person_from_session_accepted (o, args);
				
		ArrayList myPersons = SqlitePersonSession.SelectCurrentSessionPersons(
				currentSession.UniqueID, 
				false); //means: do not returnPersonAndPSlist
		personSelectWin.Update(myPersons);
		personSelectWin.Button_delete_confirm_focus(false, false);
	}
	private void on_button_encoder_person_change_done(object o, EventArgs args) 
	{
		currentPerson = personSelectWin.SelectedPerson; 
		currentPersonSession = SqlitePersonSession.Select(currentPerson.UniqueID, currentSession.UniqueID);
		label_person_change();

		personChanged();
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
		preferencesWin = PreferencesWindow.Show(preferences, getMenuItemMode());
		
		preferencesWin.FakeButtonImported.Clicked += new EventHandler(on_preferences_import_configuration);
		preferencesWin.FakeButtonDebugModeStart.Clicked += new EventHandler(on_preferences_debug_mode_start);
		preferencesWin.Button_accept.Clicked += new EventHandler(on_preferences_accepted);
	}
		
	private void on_preferences_import_configuration (object o, EventArgs args) {
		preferencesWin.FakeButtonImported.Clicked -= new EventHandler(on_preferences_import_configuration);
		
		configInit();
		LogB.Information("Initialized configuration");
	}

	private void on_preferences_accepted (object o, EventArgs args) 
	{
		preferences = preferencesWin.GetPreferences;

		if(checkbutton_video.Active) {
			videoCapturePrepare(false); //if error, show message
		}

		//change language works on windows. On Linux let's change the locale
		//if(UtilAll.IsWindows()) 
		//	languageChange();
		

		if(repetitiveConditionsWin != null)
			repetitiveConditionsWin.VolumeOn = preferences.volumeOn;

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
			createTreeView_pulses(treeview_pulses);
			createTreeView_reaction_times(treeview_reaction_times);
			createTreeView_multi_chronopic(false, treeview_multi_chronopic);
			
			on_combo_result_jumps_changed(combo_result_jumps, args);
			on_combo_result_jumps_rj_changed(combo_result_jumps_rj, args);
			on_combo_result_runs_changed(combo_result_runs, args);
			on_combo_result_runs_interval_changed(combo_result_runs_interval, args);
			on_combo_pulses_changed(combo_pulses, args);

			//currently no combo_reaction_times
			treeview_reaction_times_storeReset();
			fillTreeView_reaction_times("reactionTime");

			//currently no combo_multi_chronopic
			treeview_multi_chronopic_storeReset(false);
			fillTreeView_multi_chronopic();
		}
		catch 
		{
		}
	}


	/*
	 * menu test selectors
	 */

	private void on_menuitem_mode_main_menu_activate (object o, EventArgs args) 
	{
		reset_buttons_selector_start();
		notebook_start.CurrentPage = 0;
		
		//don't show menu bar on start page
		main_menu.Visible = false;
	}	
	
	private void select_menuitem_mode_toggled(Constants.Menuitem_modes m) 
	{
		menuitem_mode_selected_jumps_simple.Visible = false;
		menuitem_mode_selected_jumps_reactive.Visible = false;
		menuitem_mode_selected_runs_simple.Visible = false;
		menuitem_mode_selected_runs_intervallic.Visible = false;
		menuitem_mode_selected_power_gravitatory.Visible = false;
		menuitem_mode_selected_power_inertial.Visible = false;
		menuitem_mode_selected_other.Visible = false;
			
		LogB.Information("MODE", m.ToString());
		
		//default for everythong except encoder	
		menuitem_encoder_session_overview.Visible = false;
		menuitem_export_encoder_signal.Visible = false;
		menuitem_export_csv.Visible = true;

		hbox_other.Visible = false;

		if(m == Constants.Menuitem_modes.JUMPSSIMPLE || m == Constants.Menuitem_modes.JUMPSREACTIVE)
		{
			notebook_sup.CurrentPage = 0;
			notebook_capture_analyze.ShowTabs = true;
			if(m == Constants.Menuitem_modes.JUMPSSIMPLE) 
			{
				menuitem_mode_selected_jumps_simple.Visible = true;
				notebooks_change(0);
				on_extra_window_jumps_test_changed(new object(), new EventArgs());
				hbox_results_legend.Visible = true;
				notebook_capture_analyze.GetNthPage(2).Show(); //show jumpsProfile on jumps simple
			} else 
			{
				menuitem_mode_selected_jumps_reactive.Visible = true;
				notebooks_change(1);
				on_extra_window_jumps_rj_test_changed(new object(), new EventArgs());
				hbox_results_legend.Visible = false;
				notebook_capture_analyze.GetNthPage(2).Hide(); //hide jumpsProfile on jumps reactive
			}
		}
		else if(m == Constants.Menuitem_modes.RUNSSIMPLE || m == Constants.Menuitem_modes.RUNSINTERVALLIC)
		{
			notebook_sup.CurrentPage = 0;
			notebook_capture_analyze.ShowTabs = true;

			if(m == Constants.Menuitem_modes.RUNSSIMPLE) 
			{
				menuitem_mode_selected_runs_simple.Visible = true;
				notebooks_change(2);
				on_extra_window_runs_test_changed(new object(), new EventArgs());
				hbox_results_legend.Visible = true;
			}
			else
			{
				menuitem_mode_selected_runs_intervallic.Visible = true;
				notebooks_change(3);
				on_extra_window_runs_interval_test_changed(new object(), new EventArgs());
				hbox_results_legend.Visible = false;
			}
			notebook_capture_analyze.GetNthPage(2).Hide(); //hide jumpsProfile on runs
		}
		else if(m == Constants.Menuitem_modes.POWERGRAVITATORY || m == Constants.Menuitem_modes.POWERINERTIAL) 
		{
			menuitem_encoder_session_overview.Visible = true;
			menuitem_export_encoder_signal.Visible = true;
			menuitem_export_csv.Visible = false;

			//on OSX R is not installed by default. Check if it's installed. Needed for encoder
			if( UtilAll.GetOSEnum() == UtilAll.OperatingSystems.MACOSX &&
					! Util.FileExists(Constants.ROSX) )
			{
				new DialogMessage(Constants.MessageTypes.WARNING,
						Catalog.GetString("Sorry, R software is not installed.") +
						"\n" + Catalog.GetString("Please, install it from here:") +
						"\n\nhttp://cran.cnr.berkeley.edu/bin/macosx/R-latest.pkg");
				return;
			}

			notebook_sup.CurrentPage = 1;


			/*
			 * If there's a signal on gravitatory and we move to inertial, 
			 * interface has to change to YESPERSON (meaning no_signal).
			 * But, if there's no person shoud continue on NOPERSON
			 */
			if(selectRowTreeView_persons(treeview_persons, treeview_persons_store, 0))
				encoderButtonsSensitive(encoderSensEnum.YESPERSON);
			
			blankEncoderInterface();

			bool changed = false;
			if(m == Constants.Menuitem_modes.POWERGRAVITATORY) {
				menuitem_mode_selected_power_gravitatory.Visible = true;

				//change encoderConfigurationCurrent if needed
				if(encoderConfigurationCurrent.has_inertia) {
					encoderConfigurationCurrent = new EncoderConfiguration(); //LINEAR, not INERTIAL
					changed = true;
				}
				
				currentEncoderGI = Constants.EncoderGI.GRAVITATORY;
				hbox_capture_1RM.Visible = true;
				notebook_encoder_capture_extra_mass.CurrentPage = 0;
				if(radio_encoder_analyze_individual_current_set.Active || radio_encoder_analyze_individual_current_session.Active) {
					radiobutton_encoder_analyze_1RM.Visible = true;
					radiobutton_encoder_analyze_neuromuscular_profile.Visible = true;
				}
				hbox_encoder_capture_1_or_cont.Visible = true;
			} else {
				menuitem_mode_selected_power_inertial.Visible = true;

				//change encoderConfigurationCurrent if needed
				if(! encoderConfigurationCurrent.has_inertia) {
					encoderConfigurationCurrent = new EncoderConfiguration(
							Constants.EncoderConfigurationNames.ROTARYAXISINERTIAL);	
					encoderConfigurationCurrent.SetInertialDefaultOptions();
					changed = true;
				}
				
				currentEncoderGI = Constants.EncoderGI.INERTIAL;
				hbox_capture_1RM.Visible = false;
				notebook_encoder_capture_extra_mass.CurrentPage = 1;
				radiobutton_encoder_analyze_1RM.Visible = false;
				radiobutton_encoder_analyze_neuromuscular_profile.Visible = false;
				
				radio_encoder_capture_1set.Active = true;
				hbox_encoder_capture_1_or_cont.Visible = false;
			}
			encoderGuiChangesAfterEncoderConfigurationWin(true);
			if(changed) {
				prepareAnalyzeRepetitions ();
			}
		} 
		else {	//m == Constants.Menuitem_modes.OTHER (contacts / other)
			notebook_sup.CurrentPage = 0;
			hbox_other.Visible = true;
			menuitem_mode_selected_other.Visible = true;
			radio_mode_reaction_times_small.Active = true;

			notebook_capture_analyze.CurrentPage = 0;
			notebook_capture_analyze.ShowTabs = false; //only capture tab is shown (only valid for "OTHER" tests)
			//notebook_capture_analyze.GetNthPage(2).Hide(); //hide jumpsProfile on other tests
		}

		//show the program
		notebook_start.CurrentPage = 1;

		//it's not visible at startup
		main_menu.Visible = true;


		//if wizard has been used mark Chronopic as connected or disconnected depending if port exists
		if(Constants.Menuitem_mode_IsContacts(m) && wizardPortContacts != "")
			chronopicWin.Connected =
				Util.FoundInStringArray(ChronopicPorts.GetPorts(), wizardPortContacts);
		else if(! Constants.Menuitem_mode_IsContacts(m) && wizardPortEncoder != "")
			chronopicWin.Connected =
				Util.FoundInStringArray(ChronopicPorts.GetPorts(), wizardPortEncoder);


		//change multitest firmware or autoDetectChronopic
		if(Constants.Menuitem_mode_IsContacts(m))
		{
			if(chronopicWin.Connected)
				change_multitest_firmware(m);
			else
				autoDetectChronopic(m); //on contacts will perform change_multitest_firmware at the end
		}
		else if(wizardPortEncoder == "")
			autoDetectChronopic(m);

		chronojumpWindowTestsNext();
	}
	
	ChronopicDetect cpDetect;
	private void autoDetectChronopic(Constants.Menuitem_modes m)
	{
		main_menu.Sensitive = false;

		if(m == Constants.Menuitem_modes.POWERGRAVITATORY || m == Constants.Menuitem_modes.POWERINERTIAL) 
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
				main_menu.Sensitive = true;
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
		main_menu.Sensitive = true;
	}
		
	private bool previousMultitestFirmwareDefined = false;
	private Constants.Menuitem_modes previousMultitestFirmware;

	//change debounce time automatically on change menuitem mode (if multitest firmware)
	private void change_multitest_firmware(Constants.Menuitem_modes m) 
	{
		LogB.Information("change_multitest_firmware");

		//---- 1 if don't need to change, return
		if(previousMultitestFirmwareDefined && 
				! Constants.Menuitem_mode_multitest_should_change(previousMultitestFirmware, m)) 
		{
			LogB.Information("don't need to change multitest firmware");
			return;
		}

		label_chronopics_multitest.Text = "";
		
		//---- 2 if is not connected, return
		
		if(! chronopicWin.Connected)
			return;

		//---- 3 if port does not exists, show cp window and return

		//http://www.raspberrypi.org/forums/viewtopic.php?f=66&t=88415
		//https://bugzilla.xamarin.com/show_bug.cgi?id=15514
		if(! UtilAll.IsWindows ()) {
			if(! File.Exists(chronopicWin.GetContactsFirstPort())) {
				LogB.Information("Chronopic has been disconnected");
				createChronopicWindow(true, "");
				chronopicWin.Connected = false;
				return;
			}
		}

		LogB.Information("change_multitest_firmware 1");
		
		LogB.Debug("chronopicWin is null? " + (chronopicWin == null).ToString());

		int cps = chronopicWin.NumConnected();
		LogB.Debug("cps: " + cps.ToString());
		
		LogB.Debug("chronopicWin.Connected? " + chronopicWin.Connected.ToString());
		
		Chronopic.Plataforma ps;
		bool ok = (chronopicWin.CP).Read_platform(out ps);
		if(!ok) {
			LogB.Information("Chronopic has been disconnected");
			createChronopicWindow(true, "");
			chronopicWin.Connected = false;
		        return;
		}

	
		LogB.Information("change_multitest_firmware 2");
		ChronopicAuto ca;
	
		/*	
		try {
			ca = new ChronopicAutoCheck();
			//problems with windows using this:
			string chronopicVersion = ca.Read(chronopicWin.SP);
			LogB.Debug("version: " + chronopicVersion);
		} catch {
			LogB.Information("Could not read from Chronopic with method 1");
			return;
		}
		*/

		//---- 4 try to communicate with multitest firmware (return if cannot connect)
		
		LogB.Information("Trying method 2");
		bool isChronopicAuto = false;
		try {
			string result = chronopicWin.CheckAuto(out isChronopicAuto);
			LogB.Debug("version: " + result);
		} catch {
			LogB.Information("Could not read from Chronopic with method 2");
			return;
		}
		
		//---- 5 change 10 <-> 50 ms

		LogB.Information("change_multitest_firmware 3");
		if(isChronopicAuto) {
			int debounceChange = 50;
			if(m == Constants.Menuitem_modes.RUNSSIMPLE || m == Constants.Menuitem_modes.RUNSINTERVALLIC)
				debounceChange = 10;

			int msChanged = chronopicWin.ChangeMultitestFirmware(debounceChange);
			if(msChanged != -1) {
				if(msChanged == 50)
					label_chronopics_multitest.Text = "[" + Catalog.GetString("Jumps") + "]";
				else if(msChanged == 10)
					label_chronopics_multitest.Text = "[" + Catalog.GetString("Runs") + "]";
				else
					label_chronopics_multitest.Text = "";
			} else
				label_chronopics_multitest.Text = "";
		}
	
		previousMultitestFirmwareDefined = true;
		previousMultitestFirmware = m;
	}

	private Constants.Menuitem_modes getMenuItemMode() 
	{
		if(radio_menuitem_mode_jumps_simple.Active)
			return Constants.Menuitem_modes.JUMPSSIMPLE;
		else if(radio_menuitem_mode_jumps_reactive.Active)
			return Constants.Menuitem_modes.JUMPSREACTIVE;
		else if(radio_menuitem_mode_runs_simple.Active)
			return Constants.Menuitem_modes.RUNSSIMPLE;
		else if(radio_menuitem_mode_runs_intervallic.Active)
			return Constants.Menuitem_modes.RUNSINTERVALLIC;
		else if(radio_menuitem_mode_power_gravitatory.Active)
			return Constants.Menuitem_modes.POWERGRAVITATORY;
		else if(radio_menuitem_mode_power_inertial.Active)
			return Constants.Menuitem_modes.POWERINERTIAL;
		else // if(radio_menuitem_mode_other.Active)
			return Constants.Menuitem_modes.OTHER;
	}

	private void reset_buttons_selector_start_from_gui(object o, EventArgs args) {
		reset_buttons_selector_start();
	}
	private void reset_buttons_selector_start()
	{
		notebook_selector_start_jumps.CurrentPage = 0;
		notebook_selector_start_runs.CurrentPage = 0;
		notebook_selector_start_encoder.CurrentPage = 0;
	}

	private void on_radio_menuitem_mode_activate(object o, EventArgs args) 
	{
		//togglebutton sends signal two times (deactivate/activate), just get the good signal
		//http://stackoverflow.com/questions/10755541/mono-gtk-radiobutton-clicked-event-firing-twice
		if( ! (o as Gtk.RadioMenuItem).Active )
			return;
		
		select_menuitem_mode_toggled(getMenuItemMode());
	}

	private void on_button_selector_start_jumps_clicked(object o, EventArgs args) 
	{
		notebook_selector_start_jumps.CurrentPage = 1;
		notebook_selector_start_runs.CurrentPage = 0;
		notebook_selector_start_encoder.CurrentPage = 0;
	}
	private void on_button_selector_start_jumps_simple_clicked(object o, EventArgs args) 
	{
		if(radio_menuitem_mode_jumps_simple.Active) {
			//needed if people select again the same option
			select_menuitem_mode_toggled(Constants.Menuitem_modes.JUMPSSIMPLE);
		}
		else
			radio_menuitem_mode_jumps_simple.Active = true;
	}
	private void on_button_selector_start_jumps_reactive_clicked(object o, EventArgs args) 
	{
		if(radio_menuitem_mode_jumps_reactive.Active)
			select_menuitem_mode_toggled(Constants.Menuitem_modes.JUMPSREACTIVE);
		else
			radio_menuitem_mode_jumps_reactive.Active = true;
	}
	
	private void on_button_selector_start_runs_clicked(object o, EventArgs args) 
	{
		notebook_selector_start_jumps.CurrentPage = 0;
		notebook_selector_start_runs.CurrentPage = 1;
		notebook_selector_start_encoder.CurrentPage = 0;
	}
	private void on_button_selector_start_runs_simple_clicked(object o, EventArgs args)
	{
		if(radio_menuitem_mode_runs_simple.Active)
			select_menuitem_mode_toggled(Constants.Menuitem_modes.RUNSSIMPLE);
		else
			radio_menuitem_mode_runs_simple.Active = true;
	}
	private void on_button_selector_start_runs_intervallic_clicked(object o, EventArgs args) 
	{
		if(radio_menuitem_mode_runs_intervallic.Active)
			select_menuitem_mode_toggled(Constants.Menuitem_modes.RUNSINTERVALLIC);
		else
			radio_menuitem_mode_runs_intervallic.Active = true;
	}
	
	private void on_button_selector_start_encoder_clicked(object o, EventArgs args) 
	{
		notebook_selector_start_jumps.CurrentPage = 0;
		notebook_selector_start_runs.CurrentPage = 0;
		notebook_selector_start_encoder.CurrentPage = 1;
	}
	private void on_button_selector_start_encoder_gravitatory_clicked(object o, EventArgs args) 
	{
		if(radio_menuitem_mode_power_gravitatory.Active)
			select_menuitem_mode_toggled(Constants.Menuitem_modes.POWERGRAVITATORY);
		else
			radio_menuitem_mode_power_gravitatory.Active = true;
	}
	private void on_button_selector_start_encoder_inertial_clicked(object o, EventArgs args) 
	{
		if(radio_menuitem_mode_power_inertial.Active)
			select_menuitem_mode_toggled(Constants.Menuitem_modes.POWERINERTIAL);
		else
			radio_menuitem_mode_power_inertial.Active = true;
	}
	
	private void on_button_selector_start_other_clicked(object o, EventArgs args) 
	{
		if(radio_menuitem_mode_other.Active)
			select_menuitem_mode_toggled(Constants.Menuitem_modes.OTHER);
		else
			radio_menuitem_mode_other.Active = true;
	}
	
	/*
	 * end of menu test selectors
	 */

	

	/*
	 * videoOn
	 */
	

	//at what tab of notebook_sup there's the video_capture
	private int video_capture_notebook_sup = 0;

	//changed by user clicking on notebook tabs
	private void on_notebook_sup_switch_page (object o, SwitchPageArgs args) {
		if( 
				(notebook_sup.CurrentPage == 0 && video_capture_notebook_sup == 1) ||
				(notebook_sup.CurrentPage == 1 && video_capture_notebook_sup == 0)) 
		{
			//first stop showing video
			bool wasActive = false;
			if(checkbutton_video.Active) {
				wasActive = true;
				checkbutton_video.Active = false;
			}

			if(notebook_sup.CurrentPage == 0) {
				//remove video capture from encoder tab
				viewport_video_capture_encoder.Remove(capturer);
				//add in contacts tab
				hbox_video_capture.PackStart(capturer, true, true, 0);
			} else {
				//remove video capture from contacts tab
				hbox_video_capture.Remove(capturer);
				//add in encoder tab

				//switch to capture tab			
				radiobutton_video_encoder_capture.Active = true;

				//sometimes it seems is not removed and then cannot be added again
				//just add if not exists
				//maybe this error was because before we were not doing the:
				//radiobutton_video_encoder_capture.Active = true;
				if(viewport_video_capture_encoder.Child == null)
					viewport_video_capture_encoder.Add(capturer);
			}
		
			if(wasActive) 
				checkbutton_video.Active = true;
		
			video_capture_notebook_sup = notebook_sup.CurrentPage;
		}
	}

	CapturerBin capturer;
	private void videoCaptureInitialize() 
	{
		capturer = new CapturerBin();
		
		hbox_video_capture.PackStart(capturer, true, true, 0);
		
		videoCapturePrepare(false); //if error, show message
	}

	int videoDeviceNum = 0;	
	private void videoCapturePrepare(bool showErrorMessage) {
		LogB.Information("videoCapturePPPPPPPPPPPPPPPPPrepare");
		List<LongoMatch.Video.Utils.Device> devices = LongoMatch.Video.Utils.Device.ListVideoDevices();
		if(devices.Count == 0) {
			if(showErrorMessage)
				new DialogMessage(Constants.MessageTypes.WARNING, Constants.CameraNotFound);
			return;
		}


		CapturePropertiesStruct s = new CapturePropertiesStruct();

		s.OutputFile = Util.GetVideoTempFileName();

		s.VideoBitrate =  1000;
		s.AudioBitrate =  128;
		s.CaptureSourceType = CaptureSourceType.System;
		s.Width = 360;
		s.Height = 288;
		
		foreach(LongoMatch.Video.Utils.Device dev in devices){
			LogB.Information(dev.ID.ToString());
			LogB.Information(dev.IDProperty.ToString());
			LogB.Information(dev.DeviceType.ToString());
		}
			
		s.DeviceID = devices[videoDeviceNum].ID;
		

		capturer.CaptureProperties = s;

		//checkbutton_video and checkbutton_video_encoder are synchronized
		if(checkbutton_video.Active)
			capturer.Type = CapturerType.Live;
		else
			capturer.Type = CapturerType.Fake;
		capturer.Visible=true;

		try {
			capturer.Stop();
		} catch {}
		capturer.Run();
	}
	
	
	private void changeVideoButtons(bool myVideo) {
		image_video_yes.Visible = myVideo;
		image_video_no.Visible = ! myVideo;
	}
	
	private void on_checkbutton_video_clicked(object o, EventArgs args) {
		if(checkbutton_video.Active) {
			preferences.videoOn = true;
			SqlitePreferences.Update("videoOn", "True", false);
		} else {
			preferences.videoOn = false;
			SqlitePreferences.Update("videoOn", "False", false);
		}
		//change encoder checkbox but don't raise the signal	
		checkbutton_video_encoder.Clicked -= new EventHandler(on_checkbutton_video_encoder_clicked);
		checkbutton_video_encoder.Active = preferences.videoOn;
		checkbutton_video_encoder.Clicked += new EventHandler(on_checkbutton_video_encoder_clicked);
		
		changeVideoButtons(preferences.videoOn);
		
		videoCapturePrepare(true); //if error, show message
	}

	private void on_checkbutton_video_encoder_clicked(object o, EventArgs args) {
		if(checkbutton_video_encoder.Active) {
			preferences.videoOn = true;
			SqlitePreferences.Update("videoOn", "True", false);
		} else {
			preferences.videoOn = false;
			SqlitePreferences.Update("videoOn", "False", false);
		}
		//change contacts checkbox but don't raise the signal	
		checkbutton_video.Clicked -= new EventHandler(on_checkbutton_video_clicked);
		checkbutton_video.Active = preferences.videoOn;
		checkbutton_video.Clicked += new EventHandler(on_checkbutton_video_clicked);
		
		//changeVideoButtons(preferences.videoOn);
	
		//will start on record	
		videoCapturePrepare(true); //if error, show message
	}


	/*
	 * cancel and finish
	 */


	private void on_cancel_clicked (object o, EventArgs args) 
	{
		event_execute_ButtonCancel.Clicked -= new EventHandler(on_cancel_clicked);

		LogB.Information("cancel clicked one");

		//this will cancel jumps or runs
		currentEventExecute.Cancel = true;

		if(chronopicWin.Connected)
			checkCancelTotally(o, args);

		//let update stats
		//nothing changed, but stats update button cannot be insensitive,
		//because probably some jump type has changed it's jumper
		//the unsensitive of button stats is for showing the user, that he has to update manually
		//because it's not automatically updated
		//because it crashes in some thread problem
		//that will be fixed in other release
		//if(createdStatsWin)
		//	showUpdateStatsAndHideData(true);
	}
	
	private void on_cancel_multi_clicked (object o, EventArgs args) 
	{
		LogB.Information("cancel multi clicked one");

		//this will cancel jumps or runs
		currentEventExecute.Cancel = true;

		if(chronopicWin.Connected)
			checkCancelMultiTotally(o, args);
	}


	//if user doesn't touch the platform after pressing "cancel", sometimes it gets waiting a Read_event
	//now the event cancels ok, and next will be ok, also	
	private void checkCancelTotally (object o, EventArgs args) 
	{
		if(currentEventExecute.TotallyCancelled) 
			LogB.Information("totallyCancelled");
		else {
			LogB.Information("NOT-totallyCancelled ");
			errorWin = ErrorWindow.Show(Catalog.GetString("Please, touch the contact platform for full cancelling.") + "\n" +
					Catalog.GetString("Then press Accept") + "\n");
			errorWin.Button_accept.Clicked -= new EventHandler(checkCancelTotally);
			errorWin.Button_accept.Clicked += new EventHandler(checkCancelTotally);
			
			//abort test when there are problems with USB disconnected	
			errorWin.Show_button_abort();
			errorWin.Button_abort.Clicked += new EventHandler(abortTest);
		}
	}
	
	private void checkCancelMultiTotally (object o, EventArgs args) 
	{
		bool needCancel1 = false;
		bool needCancel2 = false;
		bool needCancel3 = false;
		bool needCancel4 = false;
			
		needCancel1 = !currentEventExecute.TotallyCancelledMulti1;
		if(currentEventExecute.Chronopics > 1) {
			needCancel2 = !currentEventExecute.TotallyCancelledMulti2;
			if(currentEventExecute.Chronopics > 2) {
				needCancel3 = !currentEventExecute.TotallyCancelledMulti3;
				if(currentEventExecute.Chronopics > 3)
					needCancel4 = !currentEventExecute.TotallyCancelledMulti4;
			}
		}

		if(needCancel1 || needCancel2 || needCancel3 || needCancel4) {
//			LogB.Information("NOT-totallyCancelled ");
			string cancelStr = "";
			string sep = "";
			if(needCancel1) {
				cancelStr += sep + "1";
				sep = ", ";
			}
			if(needCancel2) {
				cancelStr += sep + "2";
				sep = ", ";
			}
			if(needCancel3) {
				cancelStr += sep + "3";
				sep = ", ";
			}
			if(needCancel4) {
				cancelStr += sep + "4";
				sep = ", ";
			}

			errorWin = ErrorWindow.Show(string.Format(Catalog.GetString("Please, touch the contact platform on Chronopic/s [{0}] for full cancelling.\nThen press button\n"), cancelStr));
			errorWin.Button_accept.Clicked += new EventHandler(checkCancelMultiTotally);
		}
	}
		
		
	private void on_finish_clicked (object o, EventArgs args) 
	{
		currentEventExecute.Finish = true;
	
		if(chronopicWin.Connected)
			checkFinishTotally(o, args);
		
		//let update stats
		if(createdStatsWin)
			showUpdateStatsAndHideData(true);
	}
		
	//mark to only get inside on_multi_chronopic_finished one time
	//static bool multiFinishingByClickFinish;
	private void on_finish_multi_clicked (object o, EventArgs args) 
	{
		/*
		if(multiFinishingByClickFinish)
			return;
		else
			multiFinishingByClickFinish =  true;
			*/

		currentEventExecute.Finish = true;
		
		//runA is not called for this, because it ends different
		//and there's a message on gui/eventExecute.cs for runA	
		LogB.Debug("RR1");
		if(currentMultiChronopicType.Name != Constants.RunAnalysisName && chronopicWin.Connected) {
			checkFinishMultiTotally(o, args);
		}
		LogB.Debug("RR2");
		
		//let update stats
		//if(createdStatsWin)
		//	showUpdateStatsAndHideData(true);
	}
		
	//if user doesn't touch the platform after pressing "finish", sometimes it gets waiting a Read_event
	//now the event finishes ok, and next will be ok
	//
	//not for multiChronopic:
	
	private void checkFinishTotally (object o, EventArgs args) 
	{
		if(currentEventExecute.TotallyFinished) 
			LogB.Information("totallyFinished");
		else {
			LogB.Information("NOT-totallyFinished ");
			errorWin = ErrorWindow.Show(Catalog.GetString("Please, touch the contact platform for full finishing.") + "\n" +
					Catalog.GetString("Then press Accept") + "\n");
			errorWin.Button_accept.Clicked -= new EventHandler(checkFinishTotally);
			errorWin.Button_accept.Clicked += new EventHandler(checkFinishTotally);
		
			//abort test when there are problems with USB disconnected	
			errorWin.Show_button_abort();
			errorWin.Button_abort.Clicked += new EventHandler(abortTest);
		}
	}
	
	private void abortTest (object o, EventArgs args) {
		errorWin.Button_abort.Clicked -= new EventHandler(abortTest);
		
		LogB.Warning("Destroying error window");
		errorWin.HideAndNull();
		
		LogB.Warning("Going to abort thread");
		currentEventExecute.ThreadAbort();
		LogB.Warning("Aborted");
		
		sensitiveGuiEventDone();
		LogB.Warning("Sensitivity restored");
	}

	//runA is not called for this, because it ends different
	//and there's a message on gui/eventExecute.cs for runA	
	private void checkFinishMultiTotally (object o, EventArgs args) 
	{
		bool needFinish1 = false;
		bool needFinish2 = false;
		bool needFinish3 = false;
		bool needFinish4 = false;
			
		LogB.Information("cfmt 0");
		needFinish1 = !currentEventExecute.TotallyFinishedMulti1;
		if(currentEventExecute.Chronopics > 1) {
			LogB.Information("cfmt 1");
			needFinish2 = !currentEventExecute.TotallyFinishedMulti2;
			if(currentEventExecute.Chronopics > 2) {
				LogB.Information("cfmt 2");
				needFinish3 = !currentEventExecute.TotallyFinishedMulti3;
				if(currentEventExecute.Chronopics > 3) {
					LogB.Information("cfmt 3");
					needFinish4 = !currentEventExecute.TotallyFinishedMulti4;
				}
			}
		}
		LogB.Information("cfmt 4");

		if(needFinish1 || needFinish2 || needFinish3 || needFinish4) {
//			LogB.Information("NOT-totallyFinishled ");
			string cancelStr = "";
			string sep = "";
			if(needFinish1) {
				cancelStr += sep + "1";
				sep = ", ";
			}
			if(needFinish2) {
				cancelStr += sep + "2";
				sep = ", ";
			}
			if(needFinish3) {
				cancelStr += sep + "3";
				sep = ", ";
			}
			if(needFinish4) {
				cancelStr += sep + "4";
				sep = ", ";
			}
		
			LogB.Information("cfmt 5");
			//try here because maybe solves problems in runAnalysis when seem to update the eventExecuteWindow at the same time as tries to show this errorWindow
				errorWin = ErrorWindow.Show(string.Format(
							Catalog.GetString("Please, touch the contact platform on Chronopic/s [{0}] for full finishing.") + 
							"\n" + Catalog.GetString("Then press this button:\n"), cancelStr));
				LogB.Information("cfmt 6");
				errorWin.Button_accept.Clicked += new EventHandler(checkFinishMultiTotally);
				LogB.Information("cfmt 7");
			//}
		} else {
			LogB.Information("totallyFinished");
			/*
			//call write here, because if done in execute/MultiChronopic, will be called n times if n chronopics are working
			currentEventExecute.MultiChronopicWrite(false);
			currentMultiChronopic = (MultiChronopic) currentEventExecute.EventDone;
		

			//if this multichronopic has more chronopics than other in session, then reload treeview, else simply add
			if(currentMultiChronopic.CPs() != SqliteMultiChronopic.MaxCPs(currentSession.UniqueID)) {
				treeview_multi_chronopic_storeReset(false);
				fillTreeView_multi_chronopic();
			} else
				myTreeViewMultiChronopic.Add(currentPerson.Name, currentMultiChronopic);
			
			//since 0.7.4.1 when test is done, treeview select it. action event button have to be shown 
			showHideActionEventButtons(true, Constants.MultiChronopicName); //show
		
			//unhide buttons for delete last test
			sensitiveGuiYesEvent();
			*/
		}
	}


	void on_button_execute_test_clicked (object o, EventArgs args) 
	{
		//http://www.raspberrypi.org/forums/viewtopic.php?f=66&t=88415
		//https://bugzilla.xamarin.com/show_bug.cgi?id=15514
		if(! UtilAll.IsWindows() && chronopicWin.Connected) {
			if(! File.Exists(chronopicWin.GetContactsFirstPort())) {
				LogB.Information("Chronopic has been disconnected");
				createChronopicWindow(true, "");
				chronopicWin.Connected = false;
				return;
			}
		}

		on_button_execute_test_accepted(o, args);
	}
	
	void on_button_execute_test_accepted (object o, EventArgs args) 
	{
		//simulated tests are only allowed on SIMULATED session
		if(currentSession.Name != Constants.SessionSimulatedName && ! chronopicWin.Connected) {
			new DialogMessage(Constants.MessageTypes.WARNING, Constants.SimulatedTestsNotAllowed);
			return;
		}
		
		if(radio_menuitem_mode_jumps_simple.Active) 
		{
			LogB.Debug("radio_menuitem_mode_jumps_simple");
			on_normal_jump_activate(o, args);
		}
		else if(radio_menuitem_mode_jumps_reactive.Active) 
		{
			LogB.Debug("radio_menuitem_mode_jumps_reactive");
			on_rj_activate(o, args);
		}
		else if(radio_menuitem_mode_runs_simple.Active) {
			LogB.Debug("radio_menuitem_mode_runs_simple");
			extra_window_runs_distance = (double) extra_window_runs_spinbutton_distance.Value;
			
			on_normal_run_activate(o, args);
		}
		else if(radio_menuitem_mode_runs_intervallic.Active) {
			LogB.Debug("radio_mode_runs_i_small");
			//RSA runs cannot be simulated because it's complicated to manage the countdown event...
			if(currentRunIntervalType.IsRSA && !chronopicWin.Connected) {
				new DialogMessage(Constants.MessageTypes.WARNING, 
						Catalog.GetString("Sorry, RSA tests cannot be simulated."));
				return;
			}

			extra_window_runs_interval_distance = (double) extra_window_runs_interval_spinbutton_distance.Value;
			extra_window_runs_interval_limit = extra_window_runs_interval_spinbutton_limit.Value;
			
			on_run_interval_activate(o, args);
		}
		else if(radio_mode_reaction_times_small.Active) {
			LogB.Debug("radio_mode_rt");
	
			if(extra_window_radio_reaction_time_discriminative.Active)
				reaction_time_discriminative_lights_prepare();

			on_reaction_time_activate (o, args);
		}
		else if(radio_mode_pulses_small.Active) {
			LogB.Debug("radio_mode_pulses");
			on_pulse_activate (o, args);
		}
		else if(radio_mode_multi_chronopic_small.Active) {
			LogB.Debug("radio_mode_mc");
			on_multi_chronopic_start_clicked(o, args);
		}

		//if a test has been deleted
		//notebook_results_data changes to page 3: "deleted test"
		//when a new test is done
		//this notebook has to poing again to data of it's test
		change_notebook_results_data();
	}

	private void change_notebook_results_data()
	{
		//there are some notebook_execut pages that have not notebook_results_data pages
		//like jump simple (0), run simple (2), reaction time (4)
		if(notebook_execute.CurrentPage == 1) //reactive jump
			notebook_results_data.CurrentPage = 0;
		else if(notebook_execute.CurrentPage == 3) //interval run
			notebook_results_data.CurrentPage = 1;
		else if(notebook_execute.CurrentPage == 5) //pulse
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
	private void on_normal_jump_activate (object o, EventArgs args) 
	{
		if(execute_auto_doing)
			sensitiveGuiAutoExecuteOrWait (true);
		
		//currentJumpType is already defined in selecting name from combo or from jumpsMoreWin
		//string jumpEnglishName = Util.FindOnArray(':',2,1, UtilGtk.ComboGetActive(combo_select_jumps), selectJumpsString);
		//currentJumpType = new JumpType(jumpEnglishName);

		double jumpWeight = 0;
		if(currentJumpType.HasWeight) {
			if(extra_window_jumps_option == "%") 
				jumpWeight = (double) extra_window_jumps_spinbutton_weight.Value;
			else 
				jumpWeight = Util.WeightFromKgToPercent(
						(double) extra_window_jumps_spinbutton_weight.Value, 
						currentPersonSession.Weight);
		}
		double myFall = 0;
		if(currentJumpType.HasFall) {
			if(extra_window_jumps_check_dj_fall_calculate.Active)
				myFall = -1;
			else
				myFall = (double) extra_window_jumps_spinbutton_fall.Value;
		}

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
			sensitiveGuiEventDoing();

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
			! chronopicWin.Connected,	//is simulated
			currentPerson.UniqueID, 
			currentPerson.Name, 
			Catalog.GetString("Phases"),  	  //name of the different moments
			Constants.JumpTable, //tableName
			currentJumpType.Name 
			);
		ExecutingGraphData egd = event_execute_prepareForTest ();

		event_execute_ButtonCancel.Clicked += new EventHandler(on_cancel_clicked);
		event_execute_ButtonFinish.Clicked += new EventHandler(on_finish_clicked);

		//when user clicks on update the eventExecute window 
		//(for showing with his new confgured values: max, min and guides
		event_execute_ButtonUpdate.Clicked -= new EventHandler(on_update_clicked); //if we don't do this, on_update_clicked it's called 'n' times when 'n' events are done
		event_execute_ButtonUpdate.Clicked += new EventHandler(on_update_clicked);

		currentEventExecute = new JumpExecute(currentPerson.UniqueID, currentPerson.Name, 
				currentSession.UniqueID, currentJumpType.Name, myFall, jumpWeight,
				chronopicWin.CP, event_execute_label_message, app1, preferences.digitsNumber, preferences.volumeOn,
				progressbarLimit, egd, description);


		UtilGtk.ChronopicColors(viewport_chronopics, label_chronopics, label_connected_chronopics, chronopicWin.Connected);


		if (!chronopicWin.Connected) 
			currentEventExecute.SimulateInitValues(rand);
		
		if( currentJumpType.StartIn ) 
			currentEventExecute.Manage();
		else 
			currentEventExecute.ManageFall();
		
		thisJumpIsSimple = true; //used by: on_event_execute_update_graph_in_progress_clicked
		currentEventExecute.FakeButtonUpdateGraph.Clicked += 
			new EventHandler(on_event_execute_update_graph_in_progress_clicked);
		currentEventExecute.FakeButtonEventEnded.Clicked += new EventHandler(on_event_execute_EventEnded);

		currentEventExecute.FakeButtonFinished.Clicked += new EventHandler(on_jump_finished);
		currentEventExecute.FakeButtonThreadDyed.Clicked += new EventHandler(on_test_finished_can_touch_gtk);
	}	
	

	private void on_jump_finished (object o, EventArgs args)
	{
		currentEventExecute.FakeButtonFinished.Clicked -= new EventHandler(on_jump_finished);
	
		//test can be deleted if not cancelled
		button_delete_this_test.Sensitive = ! currentEventExecute.Cancel;

		if ( ! currentEventExecute.Cancel ) {
			currentJump = (Jump) currentEventExecute.EventDone;
		
			if(currentJumpType.Name == "slCMJleft" || currentJumpType.Name == "slCMJright") {
				if(extra_window_jumps_radiobutton_single_leg_mode_vertical.Active)
					currentJump.Description += " 0 90";
				else {
					currentJump.Description += " 0 90";
					
					//unsensitive slCMJ options 
					hbox_extra_window_jumps_single_leg_radios.Sensitive = false;
					//but show the input cm
					notebook_options_after_execute.CurrentPage = 1;
				}
				SqliteJump.UpdateDescription(Constants.JumpTable, 
						currentJump.UniqueID, currentJump.Description);
			}

			//move video file if exists
			if(preferences.videoOn)
				if (! Util.CopyTempVideo(currentSession.UniqueID, Constants.TestTypes.JUMP, currentJump.UniqueID))
					new DialogMessage(Constants.MessageTypes.WARNING, 
							Catalog.GetString("Sorry, video cannot be stored."));

			myTreeViewJumps.PersonWeight = currentPersonSession.Weight;
			myTreeViewJumps.Add(currentPerson.Name, currentJump);
			
			//since 0.7.4.1 when test is done, treeview select it. action event button have to be shown 
			showHideActionEventButtons(true, "Jump"); //show
		
			if(createdStatsWin) {
				showUpdateStatsAndHideData(true);
			}
		
			lastJumpIsSimple = true;
		
			//unhide buttons for delete last jump
			if(! execute_auto_doing)
				sensitiveGuiYesEvent();
		} 
		else if( currentEventExecute.ChronopicDisconnected )
			chronopicDisconnectedWhileExecuting();
		
		//unhide buttons that allow jumping
		if(execute_auto_doing) {
			execute_auto_order_pos ++;
			execute_auto_select();
			sensitiveGuiAutoExecuteOrWait (false);
		}
	}

	private void chronopicDisconnectedWhileExecuting() {
		LogB.Error("DISCONNECTED gui/cj");
		createChronopicWindow(true, "");
		chronopicWin.Connected = false;
	}
		
	private void on_test_finished_can_touch_gtk (object o, EventArgs args)
	{
		currentEventExecute.FakeButtonThreadDyed.Clicked -= new EventHandler(on_test_finished_can_touch_gtk);
		LogB.Information(" cantouch1 ");
		if(! execute_auto_doing)
			sensitiveGuiEventDone();
		LogB.Information(" cantouch3 ");

		if ( ! currentEventExecute.Cancel )
			event_execute_progressbar_time.Fraction = 1; 
	}



	/* ---------------------------------------------------------
	 * ----------------  JUMPS RJ EXECUTION  ------------------
	 *  --------------------------------------------------------
	 */
	
	private void on_rj_activate (object o, EventArgs args) 
	{
		//currentJumpRjType is already defined in selecting name from combo or from jumpsMoreWin
		//string jumpEnglishName = Util.FindOnArray(':',2,1, UtilGtk.ComboGetActive(combo_select_jumps_rj), selectJumpsRjString);
		//currentJumpRjType = new JumpType(jumpEnglishName);

		double progressbarLimit = 0;
		
		//if it's a unlimited interval run, put -1 as limit value
		if(currentJumpRjType.Unlimited) {
			progressbarLimit = -1;
		} else {
			if(currentJumpRjType.FixedValue > 0) {
				progressbarLimit = currentJumpRjType.FixedValue;
			} else {
				progressbarLimit = (double) extra_window_jumps_rj_spinbutton_limit.Value;
			}
		}

		double jumpWeight = 0;
		if(currentJumpRjType.HasWeight) {
			if(extra_window_jumps_rj_option == "%") {
				jumpWeight = (double) extra_window_jumps_rj_spinbutton_weight.Value;
			} else {
				jumpWeight = Util.WeightFromKgToPercent(
						(double) extra_window_jumps_rj_spinbutton_weight.Value,
						currentPersonSession.Weight);
			}
		}
		double myFall = 0;
		if( currentJumpRjType.HasFall || currentJumpRjType.Name == Constants.RunAnalysisName)
			myFall = (double) extra_window_jumps_rj_spinbutton_fall.Value;
			
		//used by cancel and finish
		//currentEventType = new JumpRjType();
		currentEventType = currentJumpRjType;
			
		//hide jumping buttons
		sensitiveGuiEventDoing();
	
		//don't let update until test finishes
		if(createdStatsWin)
			showUpdateStatsAndHideData(false);

		//show the event doing window
		event_execute_initializeVariables(
			! chronopicWin.Connected,	//is simulated
			currentPerson.UniqueID, 
			currentPerson.Name, 
			Catalog.GetString("Jumps"),  	  //name of the different moments
			Constants.JumpRjTable, //tableName
			currentJumpRjType.Name
			);
		ExecutingGraphData egd = event_execute_prepareForTest ();
		
		event_execute_ButtonCancel.Clicked += new EventHandler(on_cancel_clicked);
		event_execute_ButtonFinish.Clicked += new EventHandler(on_finish_clicked);
		
		//when user clicks on update the eventExecute window 
		//(for showing with his new configured values: max, min and guides
		event_execute_ButtonUpdate.Clicked -= new EventHandler(on_update_clicked); //if we don't do this, on_update_clicked it's called 'n' times when 'n' events are done
		event_execute_ButtonUpdate.Clicked += new EventHandler(on_update_clicked);
	
		currentEventExecute = new JumpRjExecute(currentPerson.UniqueID, currentPerson.Name, 
				currentSession.UniqueID, currentJumpRjType.Name, myFall, jumpWeight, 
				progressbarLimit, currentJumpRjType.JumpsLimited, 
				chronopicWin.CP, event_execute_label_message, app1, preferences.digitsNumber,
				checkbutton_allow_finish_rj_after_time.Active, preferences.volumeOn, 
				repetitiveConditionsWin, progressbarLimit, egd
				);
		
		
		//suitable for limited by jump and time
		//simulated always simulate limited by jumps
		if(!chronopicWin.Connected) 
			currentEventExecute.SimulateInitValues(rand);
		
		currentEventExecute.Manage();
		
		thisJumpIsSimple = false; //used by: on_event_execute_update_graph_in_progress_clicked
		currentEventExecute.FakeButtonUpdateGraph.Clicked += 
			new EventHandler(on_event_execute_update_graph_in_progress_clicked);
		currentEventExecute.FakeButtonEventEnded.Clicked += new EventHandler(on_event_execute_EventEnded);
		currentEventExecute.FakeButtonFinished.Clicked += new EventHandler(on_jump_rj_finished);
		currentEventExecute.FakeButtonThreadDyed.Clicked += new EventHandler(on_test_finished_can_touch_gtk);
	}
		
	private void on_jump_rj_finished (object o, EventArgs args) 
	{
		LogB.Information("ON JUMP RJ FINISHED");
		
		currentEventExecute.FakeButtonFinished.Clicked -= new EventHandler(on_jump_rj_finished);
		
		//test can be deleted if not cancelled
		button_delete_this_test.Sensitive = ! currentEventExecute.Cancel;

		if ( ! currentEventExecute.Cancel ) {
			currentJumpRj = (JumpRj) currentEventExecute.EventDone;
			
			//move video file if exists
			if(preferences.videoOn)
				if(! Util.CopyTempVideo(currentSession.UniqueID, Constants.TestTypes.JUMP_RJ, currentJumpRj.UniqueID))
					new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Sorry, video cannot be stored."));

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
			
			//since 0.7.4.1 when test is done, treeview select it. action event button have to be shown 
			showHideActionEventButtons(true, "JumpRj"); //show

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
	}

	/* ---------------------------------------------------------
	 * ----------------  RUNS EXECUTION (no interval) ----------
	 *  --------------------------------------------------------
	 */

	//suitable for all runs not repetitive
	private void on_normal_run_activate (object o, EventArgs args) 
	{
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
		sensitiveGuiEventDoing();
	
		//show the event doing window
		
		double progressbarLimit = 3; //same for startingIn than out (before)
		
		//don't let update until test finishes
		if(createdStatsWin)
			showUpdateStatsAndHideData(false);

		event_execute_initializeVariables(
			! chronopicWin.Connected,	//is simulated
			currentPerson.UniqueID, 
			currentPerson.Name, 
			Catalog.GetString("Phases"),  	  //name of the different moments
			Constants.RunTable, //tableName
			currentRunType.Name 
			);
		ExecutingGraphData egd = event_execute_prepareForTest ();
		
		event_execute_ButtonCancel.Clicked += new EventHandler(on_cancel_clicked);
		event_execute_ButtonFinish.Clicked += new EventHandler(on_finish_clicked);


		//when user clicks on update the eventExecute window 
		//(for showing with his new confgured values: max, min and guides
		event_execute_ButtonUpdate.Clicked -= new EventHandler(on_update_clicked); //if we don't do this, on_update_clicked it's called 'n' times when 'n' events are done
		event_execute_ButtonUpdate.Clicked += new EventHandler(on_update_clicked);


		currentEventExecute = new RunExecute(
				currentPerson.UniqueID, currentSession.UniqueID, 
				currentRunType.Name, myDistance, 
				chronopicWin.CP, event_execute_label_message, app1,
				preferences.digitsNumber, preferences.metersSecondsPreferred, preferences.volumeOn, 
				progressbarLimit, egd,
				preferences.runDoubleContactsMode,
				preferences.runDoubleContactsMS,
				preferences.runSpeedStartArrival
				);
		
		if (!chronopicWin.Connected) 
			currentEventExecute.SimulateInitValues(rand);
			
		currentEventExecute.Manage();

		thisRunIsSimple = true; //used by: on_event_execute_update_graph_in_progress_clicked
		currentEventExecute.FakeButtonUpdateGraph.Clicked += 
			new EventHandler(on_event_execute_update_graph_in_progress_clicked);
		currentEventExecute.FakeButtonEventEnded.Clicked += new EventHandler(on_event_execute_EventEnded);
		currentEventExecute.FakeButtonFinished.Clicked += new EventHandler(on_run_finished);
		currentEventExecute.FakeButtonThreadDyed.Clicked += new EventHandler(on_test_finished_can_touch_gtk);
	}
	
	private void on_run_finished (object o, EventArgs args)
	{
		currentEventExecute.FakeButtonFinished.Clicked -= new EventHandler(on_run_finished);
		
		//test can be deleted if not cancelled
		button_delete_this_test.Sensitive = ! currentEventExecute.Cancel;

		if ( ! currentEventExecute.Cancel ) {
			currentRun = (Run) currentEventExecute.EventDone;
			
			//move video file if exists
			if(preferences.videoOn)
				if(! Util.CopyTempVideo(currentSession.UniqueID, Constants.TestTypes.RUN, currentRun.UniqueID))
					new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Sorry, video cannot be stored."));
			
			currentRun.MetersSecondsPreferred = preferences.metersSecondsPreferred;

			myTreeViewRuns.Add(currentPerson.Name, currentRun);
			
			//since 0.7.4.1 when test is done, treeview select it. action event button have to be shown 
			showHideActionEventButtons(true, "Run"); //show
		
			if(createdStatsWin) {
				showUpdateStatsAndHideData(true);
			}
		
			lastRunIsSimple = true;

			//unhide buttons for delete last jump
			sensitiveGuiYesEvent();

			//put correct time value in eventWindow (put the time from chronopic and not onTimer soft chronometer)
			event_execute_LabelTimeValue = currentRun.Time;
		}
		else if( currentEventExecute.ChronopicDisconnected )
			chronopicDisconnectedWhileExecuting();
	}

	/* ---------------------------------------------------------
	 * ----------------  RUNS EXECUTION (interval) ----------
	 *  --------------------------------------------------------
	 */

	
	private void on_run_interval_activate (object o, EventArgs args)
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
		sensitiveGuiEventDoing();
		
		//don't let update until test finishes
		if(createdStatsWin)
			showUpdateStatsAndHideData(false);

		//show the event doing window
		event_execute_initializeVariables(
			! chronopicWin.Connected,	//is simulated
			currentPerson.UniqueID, 
			currentPerson.Name, 
			Catalog.GetString("Tracks"),  	  //name of the different moments
			Constants.RunIntervalTable, //tableName
			currentRunIntervalType.Name
			);
		ExecutingGraphData egd = event_execute_prepareForTest ();

		event_execute_ButtonCancel.Clicked += new EventHandler(on_cancel_clicked);
		event_execute_ButtonFinish.Clicked += new EventHandler(on_finish_clicked);

		//when user clicks on update the eventExecute window 
		//(for showing with his new confgured values: max, min and guides
		event_execute_ButtonUpdate.Clicked -= new EventHandler(on_update_clicked); //if we don't do this, on_update_clicked it's called 'n' times when 'n' events are done
		event_execute_ButtonUpdate.Clicked += new EventHandler(on_update_clicked);
	
		currentEventExecute = new RunIntervalExecute(
				currentPerson.UniqueID, currentSession.UniqueID, currentRunIntervalType.Name, 
				distanceInterval, progressbarLimit, currentRunIntervalType.TracksLimited, 
				chronopicWin.CP, event_execute_label_message, app1,
				preferences.digitsNumber, preferences.metersSecondsPreferred, preferences.volumeOn, repetitiveConditionsWin, 
				progressbarLimit, egd,
				preferences.runIDoubleContactsMode,
				preferences.runIDoubleContactsMS,
				preferences.runSpeedStartArrival
				);
		
		
		//suitable for limited by tracks and time
		if(!chronopicWin.Connected)
			currentEventExecute.SimulateInitValues(rand);
			
		currentEventExecute.Manage();
		
		thisRunIsSimple = false; //used by: on_event_execute_update_graph_in_progress_clicked
		currentEventExecute.FakeButtonUpdateGraph.Clicked += 
			new EventHandler(on_event_execute_update_graph_in_progress_clicked);
		currentEventExecute.FakeButtonEventEnded.Clicked += new EventHandler(on_event_execute_EventEnded);
		currentEventExecute.FakeButtonFinished.Clicked += new EventHandler(on_run_interval_finished);
		currentEventExecute.FakeButtonThreadDyed.Clicked += new EventHandler(on_test_finished_can_touch_gtk);
	}


	private void on_run_interval_finished (object o, EventArgs args) 
	{
		currentEventExecute.FakeButtonFinished.Clicked -= new EventHandler(on_run_interval_finished);
		
		//test can be deleted if not cancelled
		button_delete_this_test.Sensitive = ! currentEventExecute.Cancel;

		if ( ! currentEventExecute.Cancel ) {
			currentRunInterval = (RunInterval) currentEventExecute.EventDone;

			//move video file if exists
			if(preferences.videoOn)
				if(! Util.CopyTempVideo(currentSession.UniqueID, Constants.TestTypes.RUN_I, currentRunInterval.UniqueID))
					new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Sorry, video cannot be stored."));

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
		}
		else if( currentEventExecute.ChronopicDisconnected )
			chronopicDisconnectedWhileExecuting();
		
		//delete the temp tables if exists
		Sqlite.DeleteTempEvents("tempRunInterval");
	}

	/* ---------------------------------------------------------
	 * ----------------  REACTION TIMES EXECUTION --------------
	 *  --------------------------------------------------------
	 */

	
	//suitable for reaction times
	private void on_reaction_time_activate (object o, EventArgs args) 
	{
		//used by cancel and finish
		currentEventType = new ReactionTimeType();
			
		//hide jumping buttons
		sensitiveGuiEventDoing();

		//show the event doing window
		double progressbarLimit = 2;
			
		//don't let update until test finishes
		if(createdStatsWin)
			showUpdateStatsAndHideData(false);

		event_execute_initializeVariables(
			! chronopicWin.Connected,	//is simulated
			currentPerson.UniqueID, 
			currentPerson.Name, 
			Catalog.GetString("Phases"),  	  //name of the different moments
			Constants.ReactionTimeTable, //tableName
			"" 
			);
		ExecutingGraphData egd = event_execute_prepareForTest ();

		event_execute_ButtonCancel.Clicked += new EventHandler(on_cancel_clicked);
		event_execute_ButtonFinish.Clicked += new EventHandler(on_finish_clicked);

		//when user clicks on update the eventExecute window 
		//(for showing with his new confgured values: max, min and guides
		event_execute_ButtonUpdate.Clicked -= new EventHandler(on_update_clicked); //if we don't do this, on_update_clicked it's called 'n' times when 'n' events are done
		event_execute_ButtonUpdate.Clicked += new EventHandler(on_update_clicked);

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
				chronopicWin.CP, event_execute_label_message, app1, preferences.digitsNumber, preferences.volumeOn,
				progressbarLimit, egd, description
				);

		if (!chronopicWin.Connected) 
			currentEventExecute.SimulateInitValues(rand);
	
		//not on simulated because results would be always 0
		if( chronopicWin.Connected &&
				(extra_window_radio_reaction_time_discriminative.Active || 
				 extra_window_radio_reaction_time_animation_lights.Active) )
			//TODO: do also for flickr
			currentEventExecute.StartIn = false;
		
		currentEventExecute.FakeButtonReactionTimeStart.Clicked += new EventHandler(on_event_execute_reaction_time_start);

		currentEventExecute.Manage(); //check that platform is ok
		
		currentEventExecute.FakeButtonUpdateGraph.Clicked += 
			new EventHandler(on_event_execute_update_graph_in_progress_clicked);
		currentEventExecute.FakeButtonEventEnded.Clicked += new EventHandler(on_event_execute_EventEnded);
		currentEventExecute.FakeButtonFinished.Clicked += new EventHandler(on_reaction_time_finished);
		currentEventExecute.FakeButtonThreadDyed.Clicked += new EventHandler(on_test_finished_can_touch_gtk);
	}	

	private void on_event_execute_reaction_time_start (object o, EventArgs args) 
	{
		currentEventExecute.FakeButtonReactionTimeStart.Clicked -= new EventHandler(on_event_execute_reaction_time_start);

		//Fire leds or buzzer on discriminative (if not simulated)
		if(chronopicWin.Connected) {		
			if(extra_window_radio_reaction_time_discriminative.Active) {
				Thread.Sleep(Convert.ToInt32(discriminativeStartTime * 1000)); //in ms

				ChronopicAuto cs = new ChronopicStartReactionTimeAnimation();
				cs.CharToSend = discriminativeCharToSend;
				cs.Write(chronopicWin.SP, 0);
			}
			else if(extra_window_radio_reaction_time_animation_lights.Active) {
				int speed = Convert.ToInt32(spinbutton_animation_lights_speed.Value);
				ChronopicAuto cs = new ChronopicStartReactionTimeAnimation();
				cs.CharToSend = "l";
				cs.Write(chronopicWin.SP,speed);
			}

			LogB.Information("opening port at gui/chronojump.cs");	
			chronopicWin.SP.Open();

			/*
			 * some machines needed to flush
			 * - my Linux laptop two bytes
			 * - a linux guest on windows host (virtual box) don't need
			 * Note this will not allow reaction time be lower than 100 ms (DefaultTimeout on chronopic.cs)
			 */
			LogB.Information("Going to flush by time out");	//needed on some machines
			chronopicWin.CP.FlushByTimeOut();
			LogB.Information("flushed!");	
		}

		currentEventExecute.Manage2();
	}


	private void on_reaction_time_finished (object o, EventArgs args)
	{
		currentEventExecute.FakeButtonFinished.Clicked -= new EventHandler(on_reaction_time_finished);
		
		//test can be deleted if not cancelled
		button_delete_this_test.Sensitive = ! currentEventExecute.Cancel;

		if ( ! currentEventExecute.Cancel ) {

			currentReactionTime = (ReactionTime) currentEventExecute.EventDone;
			
			//move video file if exists
			if(preferences.videoOn)
				if(! Util.CopyTempVideo(currentSession.UniqueID, Constants.TestTypes.RT, currentReactionTime.UniqueID))
					new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Sorry, video cannot be stored."));
			
			myTreeViewReactionTimes.Add(currentPerson.Name, currentReactionTime);
			
			//since 0.7.4.1 when test is done, treeview select it. action event button have to be shown 
			showHideActionEventButtons(true, "ReactionTime"); //show
		
			if(createdStatsWin) {
				showUpdateStatsAndHideData(true);
			}
		
			//unhide buttons for delete last reaction time
			sensitiveGuiYesEvent();
		}
		else if( currentEventExecute.ChronopicDisconnected )
			chronopicDisconnectedWhileExecuting();
	}

	/* ---------------------------------------------------------
	 * ----------------  PULSES EXECUTION ----------------------
	 *  --------------------------------------------------------
	 */

	private void on_pulse_activate (object o, EventArgs args)
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
		sensitiveGuiEventDoing();
		
		//don't let update until test finishes
		if(createdStatsWin)
			showUpdateStatsAndHideData(false);

		//show the event doing window
		event_execute_initializeVariables(
			! chronopicWin.Connected,	//is simulated
			currentPerson.UniqueID, 
			currentPerson.Name, 
			Catalog.GetString("Pulses"),  	  //name of the different moments
			Constants.PulseTable, //tableName
			currentPulseType.Name 
			);
		ExecutingGraphData egd = event_execute_prepareForTest ();

		event_execute_ButtonCancel.Clicked += new EventHandler(on_cancel_clicked);
		event_execute_ButtonFinish.Clicked += new EventHandler(on_finish_clicked);
		
		//when user clicks on update the eventExecute window 
		//(for showing with his new confgured values: max, min and guides
		event_execute_ButtonUpdate.Clicked -= new EventHandler(on_update_clicked); //if we don't do this, on_update_clicked it's called 'n' times when 'n' events are done
		event_execute_ButtonUpdate.Clicked += new EventHandler(on_update_clicked);

		currentEventExecute = new PulseExecute(currentPerson.UniqueID, currentPerson.Name, 
				currentSession.UniqueID, currentPulseType.Name, pulseStep, totalPulses, 
				chronopicWin.CP, event_execute_label_message, 
				app1, preferences.digitsNumber, preferences.volumeOn, egd
				);
		
		if(!chronopicWin.Connected)	
			currentEventExecute.SimulateInitValues(rand);
		
		currentEventExecute.Manage();
		
		currentEventExecute.FakeButtonUpdateGraph.Clicked += 
			new EventHandler(on_event_execute_update_graph_in_progress_clicked);
		currentEventExecute.FakeButtonEventEnded.Clicked += new EventHandler(on_event_execute_EventEnded);
		currentEventExecute.FakeButtonFinished.Clicked += new EventHandler(on_pulse_finished);
		currentEventExecute.FakeButtonThreadDyed.Clicked += new EventHandler(on_test_finished_can_touch_gtk);
	}

	private void on_pulse_finished (object o, EventArgs args) 
	{
		LogB.Information("pulse finished");
		
		currentEventExecute.FakeButtonFinished.Clicked -= new EventHandler(on_pulse_finished);
		
		//test can be deleted if not cancelled
		button_delete_this_test.Sensitive = ! currentEventExecute.Cancel;

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
			
			//move video file if exists
			if(preferences.videoOn)
				if(! Util.CopyTempVideo(currentSession.UniqueID, Constants.TestTypes.PULSE, currentPulse.UniqueID))
					new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Sorry, video cannot be stored."));

			myTreeViewPulses.Add(currentPerson.Name, currentPulse);
			
			//since 0.7.4.1 when test is done, treeview select it. action event button have to be shown 
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
		
		if(notebook_sup.CurrentPage == 0) {
			int cps = chronopicWin.NumConnected();
			LogB.Debug("cps: " + cps.ToString());
			chronopicContactsLabels(cps, recreate);
		}
		else //(notebook_sup.CurrentPage == 1)
			chronopicEncoderLabels(recreate);
		
		if(recreate)	
			label_chronopics_multitest.Text = "";
	}

	private void on_chronopic_contacts_clicked (object o, EventArgs args) {
		ChronopicWindow.ChronojumpMode cmode = ChronopicWindow.ChronojumpMode.JUMPORRUN;
		if(radio_menuitem_mode_other.Active)
			cmode = ChronopicWindow.ChronojumpMode.OTHER;

		chronopicWin = ChronopicWindow.View(cmode, preferences.volumeOn);
		//chronopicWin.FakeWindowReload.Clicked += new EventHandler(chronopicWindowReload);
		chronopicWin.FakeWindowDone.Clicked += new EventHandler(on_chronopic_window_contacts_connected_or_done);
	}

	private void on_chronopic_encoder_clicked (object o, EventArgs args) {
		chronopicWin = ChronopicWindow.View(ChronopicWindow.ChronojumpMode.ENCODER, preferences.volumeOn);
		//chronopicWin.FakeWindowReload.Clicked += new EventHandler(chronopicWindowReload);
		chronopicWin.FakeWindowDone.Clicked += new EventHandler(on_chronopic_window_encoder_connected_or_done);
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

	private void on_chronopic_window_contacts_connected_or_done (object o, EventArgs args) {
		chronopicWin.FakeWindowDone.Clicked -= new EventHandler(on_chronopic_window_contacts_connected_or_done);
		int cps = chronopicWin.NumConnected();

		if(radio_mode_multi_chronopic_small.Active)	
			on_extra_window_multichronopic_test_changed(new object(), new EventArgs());
		
		if(cps > 0)
			change_multitest_firmware(getMenuItemMode());
		else 
			label_chronopics_multitest.Text = "";
		
		chronopicContactsLabels(cps, true);
	}
	
	private void on_chronopic_window_encoder_connected_or_done (object o, EventArgs args) {
		chronopicWin.FakeWindowDone.Clicked -= new EventHandler(on_chronopic_window_encoder_connected_or_done);

		chronopicEncoderLabels(true);
	}


	private void chronopicContactsLabels(int cps, bool colorize) {
		//string text = "<b>" + cps.ToString() + "</b>";
		string text = cps.ToString();
		
		label_connected_chronopics.Text = text;
		//label_connected_chronopics.UseMarkup = true; 
		
		LogB.Debug("cpwin connected: " + chronopicWin.Connected.ToString());	
		if(colorize)
			UtilGtk.ChronopicColors(viewport_chronopics, 
					label_chronopics, label_connected_chronopics, 
					chronopicWin.Connected);
	}

	private void chronopicEncoderLabels(bool colorize) {
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
	}


	private void on_multi_chronopic_start_clicked (object o, EventArgs args) {
		LogB.Information("multi chronopic accepted");
		
		bool syncAvailable = false;
		if(currentMultiChronopicType.SyncAvailable && extra_window_check_multichronopic_sync.Active)
			syncAvailable = true;


		//used by cancel and finish
		currentEventType = new MultiChronopicType();
			
		//hide pulse buttons
		sensitiveGuiEventDoing();
		
		//don't let update until test finishes
		if(createdStatsWin)
			showUpdateStatsAndHideData(false);

		//show the event doing window
		event_execute_initializeVariables(
			! chronopicWin.Connected,	//is simulated
			currentPerson.UniqueID, 
			currentPerson.Name, 
			Catalog.GetString("Changes"),  	  //name of the different moments
			Constants.MultiChronopicTable, //tableName
			currentMultiChronopicType.Name
			); 
		ExecutingGraphData egd = event_execute_prepareForTest ();

		event_execute_ButtonCancel.Clicked += new EventHandler(on_cancel_multi_clicked);
		event_execute_ButtonFinish.Clicked += new EventHandler(on_finish_multi_clicked);
		
		//when user clicks on update the eventExecute window 
		//(for showing with his new confgured values: max, min and guides
		event_execute_ButtonUpdate.Clicked -= new EventHandler(on_update_clicked); //if we don't do this, on_update_clicked it's called 'n' times when 'n' events are done
		event_execute_ButtonUpdate.Clicked += new EventHandler(on_update_clicked);


		/*
		bool syncAvailable = false;
		if(currentMultiChronopicType.SyncAvailable && extra_window_check_multichronopic_sync.Active)
			syncAvailable = true;
			*/

		int numConnected = chronopicWin.NumConnected();

		if(numConnected == 1)
			currentEventExecute = new MultiChronopicExecute(
					currentPerson.UniqueID, currentPerson.Name, 
					currentSession.UniqueID, currentMultiChronopicType.Name, 
					chronopicWin.CP, 
					syncAvailable, extra_window_check_multichronopic_delete_first.Active, 
					extra_window_spin_run_analysis_distance.Value.ToString(),
					app1, egd
					);
		else if(numConnected == 2)
			currentEventExecute = new MultiChronopicExecute(
					currentPerson.UniqueID, currentPerson.Name, 
					currentSession.UniqueID, currentMultiChronopicType.Name,  
					chronopicWin.CP, chronopicWin.CP2, 
					syncAvailable, extra_window_check_multichronopic_delete_first.Active, 
					extra_window_spin_run_analysis_distance.Value.ToString(),
					app1, egd
					);
		else if(numConnected == 3)
			currentEventExecute = new MultiChronopicExecute(
					currentPerson.UniqueID, currentPerson.Name, 
					currentSession.UniqueID, currentMultiChronopicType.Name,
					chronopicWin.CP, chronopicWin.CP2, chronopicWin.CP3, 
					syncAvailable, extra_window_check_multichronopic_delete_first.Active, 
					extra_window_spin_run_analysis_distance.Value.ToString(),
					app1, egd
					);
		else if(numConnected == 4)
			currentEventExecute = new MultiChronopicExecute(
					currentPerson.UniqueID, currentPerson.Name, 
					currentSession.UniqueID, currentMultiChronopicType.Name,
					chronopicWin.CP, chronopicWin.CP2, chronopicWin.CP3, chronopicWin.CP4,
					syncAvailable, extra_window_check_multichronopic_delete_first.Active, 
					extra_window_spin_run_analysis_distance.Value.ToString(),
					app1, egd
					);

		//if(!chronopicWin.Connected)	
		//	currentEventExecute.SimulateInitValues(rand);


		//mark to only get inside on_multi_chronopic_finished one time
		multiFinishing = false;
		currentEventExecute.Manage();

		currentEventExecute.FakeButtonUpdateGraph.Clicked += 
			new EventHandler(on_event_execute_update_graph_in_progress_clicked);
		currentEventExecute.FakeButtonEventEnded.Clicked += new EventHandler(on_event_execute_EventEnded);
//		currentEventExecute.FakeButtonRunATouchPlatform.Clicked += new EventHandler(on_event_execute_RunATouchPlatform);
		currentEventExecute.FakeButtonFinished.Clicked += new EventHandler(on_multi_chronopic_finished);
		currentEventExecute.FakeButtonThreadDyed.Clicked += new EventHandler(on_test_finished_can_touch_gtk);
	}

	bool multiFinishing;
	private void on_multi_chronopic_finished (object o, EventArgs args) {
		if(multiFinishing)
			return;
		else
			multiFinishing = true;

		currentEventExecute.FakeButtonFinished.Clicked -= new EventHandler(on_multi_chronopic_finished);

		//test can be deleted if not cancelled
		button_delete_this_test.Sensitive = ! currentEventExecute.Cancel;

		if(currentMultiChronopicType.Name == Constants.RunAnalysisName && ! currentEventExecute.MultiChronopicRunAUsedCP2()) 
			//new DialogMessage(Constants.MessageTypes.WARNING, 
			//		Catalog.GetString("This Run Analysis is not valid because there are no strides."));
			currentEventExecute.RunANoStrides();
		else if ( ! currentEventExecute.Cancel ) {
LogB.Debug("T");
			/*
			   on runAnalysis test, when cp1 ends, run ends,
			   but cp2 is still waiting event
			   with this will ask cp2 to press button
			   solves problem with threads at ending
			   */

			//on_finish_multi_clicked(o, args);
			//this produces also a crash:
			//new DialogMessage(Constants.MessageTypes.INFO, "Please, touch a platform now.");
LogB.Debug("U");
			//call write here, because if done in execute/MultiChronopic, will be called n times if n chronopics are working
			currentEventExecute.MultiChronopicWrite(false);
LogB.Debug("V");
			currentMultiChronopic = (MultiChronopic) currentEventExecute.EventDone;
LogB.Debug("W");
			//move video file if exists
			if(preferences.videoOn)
				if(! Util.CopyTempVideo(currentSession.UniqueID, 
							Constants.TestTypes.MULTICHRONOPIC, currentMultiChronopic.UniqueID))
					new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Sorry, video cannot be stored."));

			//this produces also a crash:
			//new DialogMessage(Constants.MessageTypes.INFO, "Please, touch a platform now.");

LogB.Debug("W2");
			
			//if this multichronopic has more chronopics than other in session, then reload treeview, else simply add
			if(currentMultiChronopic.CPs() != SqliteMultiChronopic.MaxCPs(false, currentSession.UniqueID)) {
				treeview_multi_chronopic_storeReset(false);
				fillTreeView_multi_chronopic();
			} else
				myTreeViewMultiChronopic.Add(currentPerson.Name, currentMultiChronopic);
LogB.Debug("X");
			
			//since 0.7.4.1 when test is done, treeview select it. action event button have to be shown 
			showHideActionEventButtons(true, Constants.MultiChronopicName); //show
		
			//unhide buttons for delete last test
			sensitiveGuiYesEvent();
		}
		else if( currentEventExecute.ChronopicDisconnected )
			chronopicDisconnectedWhileExecuting();
	}
		

	/*
	 * update button is clicked on eventWindow, chronojump.cs delegate points here
	 */
	
	private void on_update_clicked (object o, EventArgs args) {
		LogB.Information("--On_update_clicked--");
		try {
			switch (currentEventType.Type) {
				case EventType.Types.JUMP:
					if(lastJumpIsSimple && radio_menuitem_mode_jumps_simple.Active) 
						PrepareJumpSimpleGraph(currentEventExecute.PrepareEventGraphJumpSimpleObject, false);
					else if (radio_menuitem_mode_jumps_reactive.Active)
						PrepareJumpReactiveGraph(
								Util.GetLast(currentJumpRj.TvString), Util.GetLast(currentJumpRj.TcString),
								currentJumpRj.TvString, currentJumpRj.TcString, preferences.volumeOn, repetitiveConditionsWin);
					break;
				case EventType.Types.RUN:
					if(lastRunIsSimple && radio_menuitem_mode_runs_simple.Active) 
						PrepareRunSimpleGraph(currentEventExecute.PrepareEventGraphRunSimpleObject, false);
					else if(radio_menuitem_mode_runs_intervallic.Active) {
						RunType runType = SqliteRunIntervalType.SelectAndReturnRunIntervalType(currentRunInterval.Type, false);
						double distanceTotal = Util.GetRunITotalDistance(currentRunInterval.DistanceInterval, 
								runType.DistancesString, currentRunInterval.Tracks);

						double distanceInterval = currentRunInterval.DistanceInterval;
						if(distanceInterval == -1) //variable distances
							distanceInterval = Util.GetRunIVariableDistancesStringRow(
									runType.DistancesString, (int) currentRunInterval.Tracks -1);
						
						PrepareRunIntervalGraph(distanceInterval, 
								Util.GetLast(currentRunInterval.IntervalTimesString), 
								currentRunInterval.IntervalTimesString, 
								distanceTotal,
								runType.DistancesString,
								currentRunInterval.StartIn,
								preferences.volumeOn, repetitiveConditionsWin);
					}
					break;
				case EventType.Types.PULSE:
					if(radio_mode_pulses_small.Active)
						PreparePulseGraph(Util.GetLast(currentPulse.TimesString), currentPulse.TimesString);
					break;
				case EventType.Types.REACTIONTIME:
					if(radio_mode_reaction_times_small.Active)
						PrepareReactionTimeGraph(currentEventExecute.PrepareEventGraphReactionTimeObject, false);
					break;
				case EventType.Types.MULTICHRONOPIC:
					if(radio_mode_multi_chronopic_small.Active)
						PrepareMultiChronopicGraph(
								//currentMultiChronopic.timestamp, 
								Util.IntToBool(currentMultiChronopic.Cp1StartedIn), 
								Util.IntToBool(currentMultiChronopic.Cp2StartedIn), 
								Util.IntToBool(currentMultiChronopic.Cp3StartedIn), 
								Util.IntToBool(currentMultiChronopic.Cp4StartedIn), 
								currentMultiChronopic.Cp1InStr, 
								currentMultiChronopic.Cp1OutStr,
								currentMultiChronopic.Cp2InStr, 
								currentMultiChronopic.Cp2OutStr,
								currentMultiChronopic.Cp3InStr, 
								currentMultiChronopic.Cp3OutStr,
								currentMultiChronopic.Cp4InStr, 
								currentMultiChronopic.Cp4OutStr);
					break;
			}
		}
		catch {
			errorWin = ErrorWindow.Show(Catalog.GetString("Cannot update. Probably this test was deleted."));
		}
	
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
		else {
			treeview_jumps_storeReset();
			fillTreeView_jumps(UtilGtk.ComboGetActive(combo_result_jumps));
		}
		
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
		else {
			treeview_jumps_rj_storeReset();
			fillTreeView_jumps_rj(UtilGtk.ComboGetActive(combo_result_jumps_rj));
		}

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
		else {
			treeview_runs_storeReset();
			fillTreeView_runs(UtilGtk.ComboGetActive(combo_result_runs));
		}
		
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
		else {
			treeview_runs_interval_storeReset();
			fillTreeView_runs_interval(UtilGtk.ComboGetActive(combo_result_runs_interval));
		}
		
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
		LogB.Information("edit selected pulse accepted");
		
		Pulse myPulse = SqlitePulse.SelectPulseData( myTreeViewPulses.EventSelectedID, false );

		//if person changed, fill treeview again, if not, only update it's line
		if(eventOldPerson == myPulse.PersonID)
			myTreeViewPulses.Update(myPulse);
		else {
			treeview_pulses_storeReset();
			fillTreeView_pulses(UtilGtk.ComboGetActive(combo_pulses));
		}
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
	 * ----------------  EVENTS PLAY VIDEO ---------------------
	 *  --------------------------------------------------------
	 */

	//Not used on encoder	
	private bool playVideo(string fileName, bool play) 
	{
		if(File.Exists(fileName)) {
			LogB.Information("Play video starting...");
			PlayerBin player = new PlayerBin();
			player.Open(fileName);

			//without these lines works also but has less functionalities (speed, go to ms)
			Gtk.Window d = new Gtk.Window(Catalog.GetString("Playing video"));
			d.Add(player);
			d.Modal = true;
			d.SetDefaultSize(500,400);
			d.ShowAll();
			d.DeleteEvent += delegate(object sender, DeleteEventArgs e) {player.Close(); player.Dispose();};

			if(play) {
				LogB.Information("Play video playing...");
				player.Play();
			}
			return true;	
		}
		return false;	
	}


	private void on_video_play_this_test_clicked (object o, EventArgs args) {
		Constants.TestTypes type = Constants.TestTypes.JUMP;
		int id = 0;
		switch (currentEventType.Type) {
			case EventType.Types.JUMP:
				if(lastJumpIsSimple) {
					type = Constants.TestTypes.JUMP;
					id = currentJump.UniqueID;
				}
				else {
					type = Constants.TestTypes.JUMP_RJ;
					id = currentJumpRj.UniqueID;
				} break;
			case EventType.Types.RUN:
				if(lastRunIsSimple) {
					type = Constants.TestTypes.RUN;
					id = currentRun.UniqueID;
				} else {
					type = Constants.TestTypes.RUN_I;
					id = currentRunInterval.UniqueID;
				}
				break;
			case EventType.Types.PULSE:
				type = Constants.TestTypes.PULSE;
				id = currentPulse.UniqueID;
				break;
			case EventType.Types.REACTIONTIME:
				type = Constants.TestTypes.RT;
				id = currentReactionTime.UniqueID;
				break;
			case EventType.Types.MULTICHRONOPIC:
				type = Constants.TestTypes.MULTICHRONOPIC;
				id = currentMultiChronopic.UniqueID;
				break;
		}

		playVideo(Util.GetVideoFileName(currentSession.UniqueID, type, id), true);
	}

	private void on_video_play_selected_jump_clicked (object o, EventArgs args) {
		if (myTreeViewJumps.EventSelectedID > 0) 
			playVideo(Util.GetVideoFileName(currentSession.UniqueID, 
						Constants.TestTypes.JUMP,
						myTreeViewJumps.EventSelectedID), true);
	}

	private void on_video_play_selected_jump_rj_clicked (object o, EventArgs args) {
		if (myTreeViewJumpsRj.EventSelectedID > 0) 
			playVideo(Util.GetVideoFileName(currentSession.UniqueID, 
						Constants.TestTypes.JUMP_RJ,
						myTreeViewJumpsRj.EventSelectedID), true);
	}

	private void on_video_play_selected_run_clicked (object o, EventArgs args) {
		if (myTreeViewRuns.EventSelectedID > 0) 
			playVideo(Util.GetVideoFileName(currentSession.UniqueID, 
						Constants.TestTypes.RUN,
						myTreeViewRuns.EventSelectedID), true);
	}

	private void on_video_play_selected_run_interval_clicked (object o, EventArgs args) {
		if (myTreeViewRunsInterval.EventSelectedID > 0) 
			playVideo(Util.GetVideoFileName(currentSession.UniqueID, 
						Constants.TestTypes.RUN_I,
						myTreeViewRunsInterval.EventSelectedID), true);
	}

	private void on_video_play_selected_reaction_time_clicked (object o, EventArgs args) {
		if (myTreeViewReactionTimes.EventSelectedID > 0) 
			playVideo(Util.GetVideoFileName(currentSession.UniqueID, 
						Constants.TestTypes.RT,
						myTreeViewReactionTimes.EventSelectedID), true);
	}

	private void on_video_play_selected_pulse_clicked (object o, EventArgs args) {
		if (myTreeViewPulses.EventSelectedID > 0) 
			playVideo(Util.GetVideoFileName(currentSession.UniqueID, 
						Constants.TestTypes.PULSE,
						myTreeViewPulses.EventSelectedID), true);
	}

	private void on_video_play_selected_multi_chronopic_clicked (object o, EventArgs args) {
		if (myTreeViewMultiChronopic.EventSelectedID > 0) 
			playVideo(Util.GetVideoFileName(currentSession.UniqueID, 
						Constants.TestTypes.MULTICHRONOPIC,
						myTreeViewMultiChronopic.EventSelectedID), true);
	}

	/* ---------------------------------------------------------
	 * ----------------  EVENTS DELETE -------------------------
	 *  --------------------------------------------------------
	 */
	
	private void on_delete_this_test_clicked (object o, EventArgs args) {
		switch (currentEventType.Type) {
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

	private void deleted_last_test_update_widgets() {
		vbox_this_test_buttons.Sensitive = false;
		UtilGtk.ClearDrawingArea(event_execute_drawingarea, event_execute_pixmap);
		notebook_results_data.CurrentPage = 3; //shows "deleted test"
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
				confirmWinJumpRun = ConfirmWindowJumpRun.Show( Catalog.GetString("Do you want to delete this jump?"), 
						 Catalog.GetString("Attention: Deleting a Reactive subjump will delete the whole jump"));
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
	}
	
	private void on_delete_selected_run_clicked (object o, EventArgs args) {
		//notebooks_change(2); see "notebooks_change sqlite problem"
		LogB.Information("delete this run (normal)");
		
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person
		if (myTreeViewRuns.EventSelectedID > 0) {
			//3.- display confirmwindow of deletion 
			if (preferences.askDeletion) {
				confirmWinJumpRun = ConfirmWindowJumpRun.Show(Catalog.GetString("Do you want to delete this run?"), "");
				confirmWinJumpRun.Button_accept.Clicked += new EventHandler(on_delete_selected_run_accepted);
			} else {
				on_delete_selected_run_accepted(o, args);
			}
		}
	}
		
	
	private void on_delete_selected_run_interval_clicked (object o, EventArgs args) {
		//notebooks_change(3); see "notebooks_change sqlite problem"
		LogB.Information("delete this run interval");
		//1.- check that there's a line selected
		//2.- check that this line is a run and not a person (check also if it's a subrun, pass the parent run)
		if (myTreeViewRunsInterval.EventSelectedID > 0) {
			//3.- display confirmwindow of deletion 
			if (preferences.askDeletion) {
				confirmWinJumpRun = ConfirmWindowJumpRun.Show( Catalog.GetString("Do you want to delete this run?"), 
						 Catalog.GetString("Attention: Deleting a Intervallic subrun will delete the whole run"));
				confirmWinJumpRun.Button_accept.Clicked += new EventHandler(on_delete_selected_run_interval_accepted);
			} else {
				on_delete_selected_run_interval_accepted(o, args);
			}
		}
	}

	private void on_delete_selected_run_accepted (object o, EventArgs args) {
		LogB.Information("accept delete this run");
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
		LogB.Information("accept delete this run");
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
			createComboSelectJumps(false); //this will update also the selectJumpsString

			UtilGtk.ComboUpdate(combo_result_jumps, 
					SqliteJumpType.SelectJumpTypes(false, Constants.AllJumpsName, "", true), ""); //without filter, only select name
			new DialogMessage(Constants.MessageTypes.INFO, Catalog.GetString("Added simple jump."));
		} else {
			createComboSelectJumpsRj(false); //this will update also the selectJumpsRjString
			
			UtilGtk.ComboUpdate(combo_result_jumps_rj, 
					SqliteJumpType.SelectJumpRjTypes(Constants.AllJumpsName, true), ""); //without filter, only select name
			new DialogMessage(Constants.MessageTypes.INFO, Catalog.GetString("Added reactive jump."));
		}
		updateComboStats();
		combo_select_jumps.Active = 0;
		combo_select_jumps_rj.Active = 0;
		combo_result_jumps.Active = 0;
		combo_result_jumps_rj.Active = 0;
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
			createComboSelectRuns(false); //this will update also the selectRunsString

			UtilGtk.ComboUpdate(combo_result_runs, 
					SqliteRunType.SelectRunTypes(Constants.AllRunsName, true), ""); //without filter, only select name
			new DialogMessage(Constants.MessageTypes.INFO, Catalog.GetString("Added simple run."));
		} else {
			createComboSelectRunsInterval(false); //this will update also the selectRunsIntervalString
			
			UtilGtk.ComboUpdate(combo_result_runs_interval, 
					SqliteRunIntervalType.SelectRunIntervalTypes(Constants.AllRunsName, true), ""); //without filter, only select name
			new DialogMessage(Constants.MessageTypes.INFO, Catalog.GetString("Added intervallic run."));
		}
		updateComboStats();
		combo_select_runs.Active = 0;
		combo_select_runs_interval.Active = 0;
		combo_result_runs.Active = 0;
		combo_result_runs_interval.Active = 0;
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
	
	private void on_deleted_jump_type (object o, EventArgs args) {
		//first delete if from combos
		string translatedName = Util.FindOnArray(':', 2, 1, jumpsMoreWin.SelectedEventName, selectJumpsString);
		UtilGtk.ComboDelThisValue(combo_select_jumps, translatedName);
		UtilGtk.ComboDelThisValue(combo_result_jumps, translatedName);
		
		//2nd delete if from global string. -1 selects all row
		string row = Util.FindOnArray(':',1, -1, jumpsMoreWin.SelectedEventName, selectJumpsString);
		selectJumpsString = Util.DeleteString(selectJumpsString, row);

		combo_select_jumps.Active = 0;
		combo_result_jumps.Active = 0;

		extra_window_jumps_initialize(new JumpType("Free"));
	}

	private void on_deleted_jump_rj_type (object o, EventArgs args) {
		//first delete if from combos
		string translatedName = Util.FindOnArray(':', 2, 1, jumpsRjMoreWin.SelectedEventName, selectJumpsRjString);
		UtilGtk.ComboDelThisValue(combo_select_jumps_rj, translatedName);
		UtilGtk.ComboDelThisValue(combo_result_jumps_rj, translatedName);
		
		//2nd delete if from global string. -1 selects all row
		string row = Util.FindOnArray(':',1, -1, jumpsRjMoreWin.SelectedEventName, selectJumpsRjString);
		selectJumpsRjString = Util.DeleteString(selectJumpsRjString, row);

		combo_select_jumps_rj.Active = 0;
		combo_result_jumps_rj.Active = 0;

		extra_window_jumps_rj_initialize(new JumpType("RJ(j)"));
	}

	private void on_deleted_run_type (object o, EventArgs args) {
		extra_window_runs_initialize(new RunType("Custom"));
	}
	private void on_deleted_run_i_type (object o, EventArgs args) {
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
		fillTreeView_jumps_rj(UtilGtk.ComboGetActive(combo_result_jumps_rj));
		
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
		fillTreeView_runs_interval(UtilGtk.ComboGetActive(combo_result_runs_interval));
		
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
		LogB.Information("repair selected pulse accepted");
		
		treeview_pulses_storeReset();
		fillTreeView_pulses(UtilGtk.ComboGetActive(combo_pulses));
	}

	private void on_repair_selected_multi_chronopic_clicked (object o, EventArgs args) {
		//notebooks_change(6); see "notebooks_change sqlite problem"
		LogB.Information("Repair selected multichronopic");
	}

	/* ---------------------------------------------------------
	 * ----------------  Info on power and stiffness -----------
	 *  --------------------------------------------------------
	 */


	private void on_button_jumps_jumpsRj_result_help_power_clicked (object o, EventArgs args) {
		new DialogMessage(Constants.MessageTypes.INFO, Constants.HelpPower);
	}
	private void on_button_jumps_jumpsRj_result_help_stiffness_clicked (object o, EventArgs args) {
		new DialogMessage(Constants.MessageTypes.INFO, Constants.HelpStiffness,	"hbox_stiffness_formula");
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
			selectRowTreeView_persons(treeview_persons, treeview_persons_store, rowToSelect);
			label_jump_auto_current_person.Text = currentPerson.Name;

			//select the test
			int rowTest = Convert.ToInt32(Util.FindOnArray(':', 0, -2, ea.testUniqueID.ToString(), selectJumpsString));
			combo_select_jumps.Active = rowTest;
			label_jump_auto_current_test.Text = "(" + ea.testTrName + ")";
			
			//put GUI on auto_waiting
			sensitiveGuiAutoExecuteOrWait (false);
		}
	}

	private void on_button_auto_end_clicked (object o, EventArgs args) 
	{
		sensitiveGuiAutoStartEnd (false);
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
	private void notebooks_change(int desiredPage) {
		LogB.Information("notebooks_change");
		//LogB.Debug(new StackFrame(1).GetMethod().Name);

		//LogB.Information("currentPage" + notebook_execute.CurrentPage.ToString());
		//LogB.Information("desiredPage" + desiredPage.ToString());

		while(notebook_execute.CurrentPage < desiredPage) 
			notebook_execute.NextPage();
		while(notebook_execute.CurrentPage > desiredPage) 
			notebook_execute.PrevPage();
		
		//delete capture graph 
		UtilGtk.ClearDrawingArea(event_execute_drawingarea, event_execute_pixmap);
		//change table under graph
		change_notebook_results_data();

		while(notebook_results.CurrentPage < desiredPage) 
			notebook_results.NextPage();
		while(notebook_results.CurrentPage > desiredPage) 
			notebook_results.PrevPage();

		while(notebook_options_top.CurrentPage < desiredPage) 
			notebook_options_top.NextPage();
		while(notebook_options_top.CurrentPage > desiredPage) 
			notebook_options_top.PrevPage();
		
	
		//change test image according to notebook_execute	
		if(notebook_execute.CurrentPage == 0)
			changeTestImage(EventType.Types.JUMP.ToString(), 
					currentJumpType.Name, currentJumpType.ImageFileName);
		else if(notebook_execute.CurrentPage == 1)
			changeTestImage(EventType.Types.JUMP.ToString(), 
					currentJumpRjType.Name, currentJumpRjType.ImageFileName);
		else if(notebook_execute.CurrentPage == 2)
			changeTestImage(EventType.Types.RUN.ToString(), 
					currentRunType.Name, currentRunType.ImageFileName);
		else if(notebook_execute.CurrentPage == 3)
			changeTestImage(EventType.Types.RUN.ToString(), 
					currentRunIntervalType.Name, currentRunIntervalType.ImageFileName);
		else if(notebook_execute.CurrentPage == 4)
			changeTestImage(EventType.Types.REACTIONTIME.ToString(), 
					currentReactionTimeType.Name, currentReactionTimeType.ImageFileName);
		else if(notebook_execute.CurrentPage == 5)
			changeTestImage(EventType.Types.PULSE.ToString(), 
					currentPulseType.Name, currentPulseType.ImageFileName);
		else if(notebook_execute.CurrentPage == 6) {
			changeTestImage(EventType.Types.MULTICHRONOPIC.ToString(), 
					currentMultiChronopicType.Name, currentMultiChronopicType.ImageFileName);
		}
	
		//button_execute_test have to be non sensitive in multichronopic without two cps
		//else has to be sensitive

		//if there are persons
		if (notebook_execute.CurrentPage == 6) {
			if (chronopicWin.NumConnected() >= 2)
				extra_window_multichronopic_can_do(true);
			else 
				extra_window_multichronopic_can_do(false);
		} else {
			button_execute_test.Sensitive = myTreeViewPersons.IsThereAnyRecord();
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
		LogB.Information("Opening manual at: " + System.IO.Path.GetFullPath(Util.GetManualDir())); 
		try {
			System.Diagnostics.Process.Start(System.IO.Path.GetFullPath(Util.GetManualDir())); 
		} catch {
			new DialogMessage(Constants.MessageTypes.WARNING, "Sorry, manual folder does not exist.");
		}
	}

	private void on_menuitem_formulas_activate (object o, EventArgs args) {
		new DialogMessage(Constants.MessageTypes.INFO, "Here there will be bibliographic information about formulas and some notes.\n\nProbably this will be a window and not a dialog\n\nNote text is selectable");
	}

	private void on_menuitem_accelerators_activate (object o, EventArgs args) {
		new DialogMessage(
				Catalog.GetString("Accelerators help"),
				Constants.MessageTypes.INFO, 
				Catalog.GetString("Use these keys in order to work faster.") + "\n\n" +
				"- " + Catalog.GetString("On execute test tab:") + "\n\n" +
				"<tt><b>CTRL+p</b></tt> " + Catalog.GetString("Edit selected person") + "\n" +
				"<tt><b>CTRL+" + Catalog.GetString("CURSOR_UP") + "</b></tt> " + Catalog.GetString("Select previous person") + "\n" +
				"<tt><b>CTRL+" + Catalog.GetString("CURSOR_DOWN") + "</b></tt> " + Catalog.GetString("Select next person") + "\n" +
				"<tt><b>(space)</b></tt> " + Catalog.GetString("Execute test") + "\n" +
				"<tt><b>v</b></tt> " + Catalog.GetString("Play video of this test") + " " + Catalog.GetString("(if available)")+ "\n" +
				"<tt><b>d</b></tt> " + Catalog.GetString("Delete this test") + "\n" +
				"\n" + "- " + Catalog.GetString("On results tab:") + "\n\n" +
				"<tt><b>z</b></tt> " + Catalog.GetString("Zoom change") + "\n" +
				"<tt><b>v</b></tt> " + Catalog.GetString("Play video of selected test") + " " + Catalog.GetString("(if available)")+ "\n" +
				"<tt><b>e</b></tt> " + Catalog.GetString("Edit selected test") + "\n" +
				"<tt><b>d</b></tt> " + Catalog.GetString("Delete selected test") + "\n" +
				"<tt><b>r</b></tt> " + Catalog.GetString("Repair selected test") + " " + Catalog.GetString("(if available)")
				);
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

	private void on_menuitem_ping_activate (object o, EventArgs args) 
	{
		pingDo(true);
	}
	private void pingAtStart()
	{
		pingDo(false);
	}
	private void pingDo(bool showInWindow)
	{
		Json js = new Json();
		bool success = js.Ping(UtilAll.GetOS(), UtilAll.ReadVersion(), preferences.machineID);

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

		/*
		new DialogMessage(
				"Chronojump",
				Constants.MessageTypes.INFO, 
				"Temporarily Disabled");
		*/
	}
	
	
	private void on_preferences_debug_mode_start (object o, EventArgs args) {
		//first delete debug file
		Util.FileDelete(System.IO.Path.GetTempPath() + "chronojump-debug.txt");

		encoderRProcCapture.Debug = true;
		encoderRProcAnalyze.Debug = true;
	
		hbox_gui_tests.Visible = true;
		button_carles.Visible = true;

		preferencesWin.DebugActivated();
	}

	private void on_button_gui_tests_clicked (object o, EventArgs args) {
		chronojumpWindowTestsStart(Convert.ToInt32(spin_gui_tests.Value -1));
	}
	
	private void on_button_carles_clicked (object o, EventArgs args) {
		//carles stuff
	}


	private void on_about1_activate (object o, EventArgs args) {
		string translator_credits = Catalog.GetString ("translator-credits");
		//only print if exist (don't print 'translator-credits' word
		if(translator_credits == "translator-credits") 
			translator_credits = "";

		new About(progVersion, translator_credits);
	}
		
	private void on_button_rj_bells_clicked(object o, EventArgs args) {
		repetitiveConditionsWin.View(Constants.BellModes.JUMPS, preferences.volumeOn);
	}

	private void on_button_time_bells_clicked(object o, EventArgs args) {
		repetitiveConditionsWin.View(Constants.BellModes.RUNS, preferences.volumeOn);
	}
	
	private void on_repetitive_conditions_closed(object o, EventArgs args) {
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
				string mainVariable = Constants.GetEncoderVariablesCapture(preferences.encoderCaptureMainVariable);
				double mainVariableHigher = repetitiveConditionsWin.GetMainVariableHigher(mainVariable);
				double mainVariableLower = repetitiveConditionsWin.GetMainVariableLower(mainVariable);
				plotCurvesGraphDoPlot(mainVariable, mainVariableHigher, mainVariableLower, captureCurvesBarsData,
						repetitiveConditionsWin.EncoderInertialDiscardFirstThree,
						false);	//not capturing
			} else
				UtilGtk.ErasePaint(encoder_capture_curves_bars_drawingarea, encoder_capture_curves_bars_pixmap);
		}
	}
	

	JumpsProfile jumpsProfile;

	private void jumpsProfileDo (bool calculateData)
	{
		if(currentPerson == null || currentSession == null)
			return;
		
		if(jumpsProfile == null) {
			jumpsProfile = new JumpsProfile();
			calculateData = true;
		}

		if(calculateData)
			jumpsProfile.Calculate(currentPerson.UniqueID, currentSession.UniqueID);

		JumpsProfileGraph.Do(jumpsProfile.GetIndexes(), drawingarea_jumps_profile);
	}
	private void on_drawingarea_jumps_profile_expose_event (object o, ExposeEventArgs args) 
	{
		jumpsProfileDo(false); //do not calculate data
		//data is calculated on switch page (at notebook_capture_analyze) or on change person
	}
	
	private void on_notebook_capture_analyze_switch_page (object o, SwitchPageArgs args) {
		if(notebook_capture_analyze.CurrentPage == 2)
			jumpsProfileDo(true);
	}

	private void on_button_jumps_profile_help_clicked (object o, EventArgs args) {
		scrolledwindow_jumps_profile_training.Visible = false;
		scrolledwindow_jumps_profile_help.Visible = ! scrolledwindow_jumps_profile_help.Visible;
	}

	private void on_button_jumps_profile_training_clicked (object o, EventArgs args) {
		scrolledwindow_jumps_profile_help.Visible = false;
		scrolledwindow_jumps_profile_training.Visible = ! scrolledwindow_jumps_profile_training.Visible;
	}


	/* ---------------------------------------------------------
	 * ----------------  SENSITIVE GUI METHODS-------------------
	 *  --------------------------------------------------------
	 */

	private void menuSessionSensitive(bool option)
	{
		menuitem_edit_session.Sensitive = option;
		menuitem_delete_session.Sensitive = option;
		menuitem_export_csv.Sensitive = option;
		//menuitem_export_xml.Sensitive = option; not implemented yet
		menuitem_encoder_session_overview.Sensitive = option;

	}
	
	private void menuPersonSelectedSensitive(bool option)
	{
		button_persons_up.Sensitive = option;
		button_persons_down.Sensitive = option;
		button_edit_current_person.Sensitive = option;
		button_show_all_person_events.Sensitive = option;
		button_delete_current_person.Sensitive = option;
	}

	private void sensitiveGuiNoSession () 
	{
		notebook_session_person.CurrentPage = 0;

		treeview_persons.Sensitive = false;
		
		//menuitems
		menuSessionSensitive(false);
		menuPersonSelectedSensitive(false);
		
		button_image_test_zoom.Sensitive = false;
		frame_persons.Sensitive = false;
		button_recuperate_person.Sensitive = false;
		button_recuperate_persons_from_session.Sensitive = false;
		button_person_add_single.Sensitive = false;
		button_person_add_multiple.Sensitive = false;
		button_edit_current_person.Sensitive = false;
		button_delete_current_person.Sensitive = false;
	
		button_encoder_person_change.Sensitive = false;
		
		//notebooks
		notebook_execute.Sensitive = false;
		//hbox_chronopics.Sensitive = false;
		notebook_results.Sensitive = false;
		notebook_options_top.Sensitive = false;
		notebook_encoder_sup.Sensitive = false;
		vbox_stats.Sensitive = false;
		frame_share_data.Sensitive = false;
		
		vbox_this_test_buttons.Sensitive = false;
		vbox_execute_test.Sensitive = false;
		button_execute_test.Sensitive = false;

		encoderButtonsSensitive(encoderSensEnum.NOSESSION);
		
		eventExecuteHideAllTables();
	}
	
	private void sensitiveGuiYesSession () 
	{
		notebook_session_person.CurrentPage = 1;

		button_image_test_zoom.Sensitive = true;
		frame_persons.Sensitive = true;
		button_recuperate_person.Sensitive = true;
		button_recuperate_persons_from_session.Sensitive = true;
		button_person_add_single.Sensitive = true;
		button_person_add_multiple.Sensitive = true;
		
		button_encoder_person_change.Sensitive = true;
		
		menuSessionSensitive(true);
		vbox_stats.Sensitive = true;
		frame_share_data.Sensitive = true;
		
		//changeTestImage("", "", "LOGO");
	}

	//only called by delete person functions (if we run out of persons)
	private void sensitiveGuiNoPerson () {
		hbox_jumps.Sensitive = false;
		hbox_jumps_rj.Sensitive = false;
		button_execute_test.Sensitive = false;
		
		encoderButtonsSensitive(encoderSensEnum.NOPERSON);
		personChanged();
		
		notebook_execute.Sensitive = false;
		//hbox_chronopics.Sensitive = false;
		notebook_results.Sensitive = false;
		notebook_options_top.Sensitive = false;
		notebook_encoder_sup.Sensitive = false;
		treeview_persons.Sensitive = false;
		
		menuPersonSelectedSensitive(false);
		vbox_execute_test.Sensitive = false;

		label_encoder_person_name.Text = "";
	}
	
	private void sensitiveGuiYesPerson () {
		hbox_jumps.Sensitive = true;
		hbox_jumps_rj.Sensitive = true;
		button_execute_test.Sensitive = true;

		encoderButtonsSensitive(encoderSensEnum.YESPERSON);
		personChanged();
		
		notebook_execute.Sensitive = true;
		//hbox_chronopics.Sensitive = true;
		notebook_results.Sensitive = true;
		notebook_options_top.Sensitive = true;
		notebook_encoder_sup.Sensitive = true;
		treeview_persons.Sensitive = true;
		
		menuPersonSelectedSensitive(true);
	
		//unsensitive edit, delete, repair events because no event is initially selected
		showHideActionEventButtons(false, "ALL");

		combo_select_jumps.Sensitive = true;
		combo_result_jumps.Sensitive = true;
		combo_select_jumps_rj.Sensitive = true;
		combo_result_jumps_rj.Sensitive = true;
		combo_select_runs.Sensitive = true;
		combo_result_runs.Sensitive = true;
		combo_select_runs_interval.Sensitive = true;
		combo_result_runs_interval.Sensitive = true;
		combo_pulses.Sensitive = true;
		
		vbox_execute_test.Sensitive = true;
	}
	
	private void sensitiveGuiYesEvent () {
	}
	
	private void sensitiveGuiEventDoing () {
		session_menuitem.Sensitive = false;
		menuitem_mode.Sensitive = false;
		
		//jumpsProfile has Sqlite calls. Don't do them while jumping
		//but don't unsensitive the notebook because user need to "finish" or cancel"
		//notebook_capture_analyze.Sensitive = true; 
		notebook_capture_analyze.GetNthPage(1).Hide();
		if(radio_menuitem_mode_jumps_simple.Active)
			notebook_capture_analyze.GetNthPage(2).Hide();
		
		
		help_menuitem.Sensitive = false;
		frame_persons.Sensitive = false;
		
		button_execute_test.Sensitive = false;
		
		button_encoder_person_change.Sensitive = false;

		encoderButtonsSensitive(encoderSensEnum.PROCESSINGR);
		
		//hbox
		hbox_jumps_test.Sensitive = false;
		hbox_jump_types_options.Sensitive = false;
		
		hbox_jumps_rj.Sensitive = false;
		hbox_runs.Sensitive = false;
		hbox_runs_interval.Sensitive = false;
		hbox_pulses.Sensitive = false;
		vbox_this_test_buttons.Sensitive = false;
		
		button_upload_session.Sensitive = false;
		button_activate_chronopics.Sensitive = false;
		notebook_options_top.Sensitive = false;
		event_execute_button_update.Sensitive = false;
		
		//hbox_multi_chronopic_buttons.Sensitive = false;
	}
   
	private void sensitiveGuiEventDone () {
		LogB.Information(" sensitiveGuiEventDone start ");

		session_menuitem.Sensitive = true;
		menuitem_mode.Sensitive = true;

		//jumpsProfile has Sqlite calls. Don't do them while jumping
		//but don't unsensitive the notebook because user need to "finish" or cancel"
		//notebook_capture_analyze.Sensitive = true; 
		notebook_capture_analyze.GetNthPage(1).Show();
		if(radio_menuitem_mode_jumps_simple.Active)
			notebook_capture_analyze.GetNthPage(2).Show();
		
		help_menuitem.Sensitive = true;
		frame_persons.Sensitive = true;

		button_execute_test.Sensitive = true;
		
		button_encoder_person_change.Sensitive = true;
	
		if(encoderCaptureCurves != null && encoderCaptureCurves.Count > 0)
			encoderButtonsSensitive(encoderSensEnum.DONEYESSIGNAL);
		else
			encoderButtonsSensitive(encoderSensEnum.DONENOSIGNAL);

		//hbox
		hbox_jumps_test.Sensitive = true;
		hbox_jump_types_options.Sensitive = true;
		
		hbox_jumps_rj.Sensitive = true;
		hbox_runs.Sensitive = true;
		hbox_runs_interval.Sensitive = true;
		hbox_pulses.Sensitive = true;
		//hbox_multi_chronopic_buttons.Sensitive = true;
		vbox_this_test_buttons.Sensitive = true;
		
		button_upload_session.Sensitive = true;
		button_activate_chronopics.Sensitive = true;
		notebook_options_top.Sensitive = true;
		event_execute_button_update.Sensitive = true;

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
		}
		LogB.Information(" sensitiveGuiEventDone end ");
	}
	
	/*
	 * sensitive GUI on executeAuto methods 
	 */

	private void on_autodetect_help_clicked (object o, EventArgs args) {
		new DialogMessage(
				Catalog.GetString("Help on auto-detect"),
				Constants.MessageTypes.INFO, 
				Catalog.GetString("Modes:") + "\n\n" +
				"- <b>" + Catalog.GetString("Active") + "</b>:  " +
					Catalog.GetString("Auto-detection on hardware is active.") + " " + Catalog.GetString("Default mode") + ".\n\n" +
				"- <b>" + Catalog.GetString("Active, discarding first port") + "</b>:  " + 
					Catalog.GetString("Use this option on computers where first port fails.") + "\n\n" +
				"- <b>" + Catalog.GetString("Inactive") + "</b>:  " + 
					Catalog.GetString("Auto-detection on hardware is inactive.") + " " + Catalog.GetString("Use it if you have problems at start or at capture.")
				);
	}
	
	private void on_button_chronopic_wizard_clicked (object o, EventArgs args) 
	{
		chronopicWizardWin = ChronopicWizardWindow.Show();
	
		chronopicWizardWin.FakeButtonChronopicWizardFinished.Clicked -= 
			new EventHandler(chronopic_wizard_finished);
		chronopicWizardWin.FakeButtonChronopicWizardFinished.Clicked += new 
			EventHandler(chronopic_wizard_finished);
	}
	private void chronopic_wizard_finished (object o, EventArgs args) 
	{
		chronopicWizardWin.FakeButtonChronopicWizardFinished.Clicked -= 
			new EventHandler(chronopic_wizard_finished);
		
		wizardPortContacts = chronopicWizardWin.PortContacts;
		wizardPortEncoder = chronopicWizardWin.PortEncoder;
		
		LogB.Information("wizardPortContacts: " + wizardPortContacts);
		LogB.Information("wizardPortEncoder: " + wizardPortEncoder);
	
		/*
		 * createChronopicWindow (pass cp,...)
		 * recreate is true because it has been created on first ChronojumpWindow call
		 */
		//contacts and encoder
		if(wizardPortContacts != "")
		{
			Chronopic cpW; //cp Wizard
			chronopicWin.CreateSPifNeeded(wizardPortContacts);
			cpW = new Chronopic(chronopicWin.SP);
			ChronopicPortData cpdata = new ChronopicPortData(1, wizardPortContacts, true);
			ArrayList cpd = new ArrayList();
			cpd.Add(cpdata);

			if(wizardPortEncoder == "") //no encoder
				createChronopicWindow(cpW, cpd, true, "");
			else
				createChronopicWindow(cpW, cpd, true, wizardPortEncoder);
		}
		else { //only encoder
			createChronopicWindow(true, wizardPortEncoder);
		}
		
		//need to do this because createChronopicWindow does it but relying on menuitem_mode, and now we are on start page	
		if(wizardPortEncoder != "")
			chronopicEncoderLabels(true);

		//all the needed info is take. Can destroy wizard window 
		chronopicWizardWin.HideAndNull();
	}

	//start/end auto mode
	private void sensitiveGuiAutoStartEnd (bool start) {
		//if automode, sensitiveGuiEventDoing, sensitiveGuiEventDone don't work
		session_menuitem.Sensitive 	= ! start;
		menuitem_mode.Sensitive 	= ! start;
		help_menuitem.Sensitive 	= ! start;
		frame_persons.Sensitive 	= ! start;

		hbox_jumps_test.Visible 	= ! start;
		button_auto_start.Visible 	= ! start;	
		hbox_jump_types_options.Visible = ! start;
		hbox_jump_auto_controls.Visible  = start;
		
		notebook_capture_analyze.GetNthPage(1).Visible = ! start;
		if(radio_menuitem_mode_jumps_simple.Active)
			notebook_capture_analyze.GetNthPage(2).Visible = ! start;

		//when start, put button delete_this_test as not sensitive
		//(just for the test previous to the auto process)
		if(start)
			button_delete_this_test.Sensitive = false;
	}
	
	//true: executing a test; false: waiting a test to be executed
	private void sensitiveGuiAutoExecuteOrWait (bool execute) {
		//if automode, sensitiveGuiEventDoing, sensitiveGuiEventDone don't work
		button_activate_chronopics.Sensitive 	= ! execute;
		button_execute_test.Sensitive 		= ! execute;
		notebook_options_top.Sensitive 		= ! execute;
		vbox_this_test_buttons.Sensitive 	= ! execute;
	}


	private void showHideActionEventButtons(bool show, string type) {
		bool success = false;
		bool recordedVideo = false;
		if(type == "ALL" || type == "Jump") {
			button_edit_selected_jump.Sensitive = show;
			button_delete_selected_jump.Sensitive = show;

			button_video_play_selected_jump.Sensitive = false;
			if (myTreeViewJumps.EventSelectedID > 0 && File.Exists(Util.GetVideoFileName(
							currentSession.UniqueID, 
							Constants.TestTypes.JUMP,
							myTreeViewJumps.EventSelectedID))) {
				button_video_play_selected_jump.Sensitive = true;
				recordedVideo = true;
			}


			success = true;
		} 
		if (type == "ALL" || type == "JumpRj") {
			button_edit_selected_jump_rj.Sensitive = show;
			button_delete_selected_jump_rj.Sensitive = show;
			button_repair_selected_jump_rj.Sensitive = show;

			button_video_play_selected_jump_rj.Sensitive = false;
			if (myTreeViewJumpsRj.EventSelectedID > 0 && File.Exists(Util.GetVideoFileName(
							currentSession.UniqueID, 
							Constants.TestTypes.JUMP_RJ,
							myTreeViewJumpsRj.EventSelectedID))) {
				button_video_play_selected_jump_rj.Sensitive = true;
				recordedVideo = true;
			}

			success = true;
		} 
		if (type == "ALL" || type == "Run") {
			button_edit_selected_run.Sensitive = show;
			button_delete_selected_run.Sensitive = show;

			button_video_play_selected_run.Sensitive = false;
			if (myTreeViewRuns.EventSelectedID > 0 && File.Exists(Util.GetVideoFileName(
							currentSession.UniqueID, 
							Constants.TestTypes.RUN,
							myTreeViewRuns.EventSelectedID))) {
				button_video_play_selected_run.Sensitive = true;
				recordedVideo = true;
			}

			success = true;
		} 
		if (type == "ALL" || type == "RunInterval") {
			button_edit_selected_run_interval.Sensitive = show;
			button_delete_selected_run_interval.Sensitive = show;
			button_repair_selected_run_interval.Sensitive = show;
			
			button_video_play_selected_run_interval.Sensitive = false;
			if (myTreeViewRunsInterval.EventSelectedID > 0 && File.Exists(Util.GetVideoFileName(
							currentSession.UniqueID, 
							Constants.TestTypes.RUN_I,
							myTreeViewRunsInterval.EventSelectedID))) {
				button_video_play_selected_run_interval.Sensitive = true;
				recordedVideo = true;
			}

			success = true;
		} 
		if (type == "ALL" || type == "ReactionTime") {
			button_edit_selected_reaction_time.Sensitive = show;
			button_delete_selected_reaction_time.Sensitive = show;
			
			button_video_play_selected_reaction_time.Sensitive = false;
			if (myTreeViewReactionTimes.EventSelectedID > 0 && File.Exists(Util.GetVideoFileName(
							currentSession.UniqueID, 
							Constants.TestTypes.RT,
							myTreeViewReactionTimes.EventSelectedID))) {
				button_video_play_selected_reaction_time.Sensitive = true;
				recordedVideo = true;
			}

			success = true;
		} 
		if (type == "ALL" || type == "Pulse") {
			// menuitem_edit_selected_pulse.Sensitive = show;
			// menuitem_delete_selected_pulse.Sensitive = show;
			button_edit_selected_pulse.Sensitive = show;
			button_delete_selected_pulse.Sensitive = show;
			button_repair_selected_pulse.Sensitive = show;
			
			button_video_play_selected_pulse.Sensitive = false;
			if (myTreeViewPulses.EventSelectedID > 0 && File.Exists(Util.GetVideoFileName(
							currentSession.UniqueID, 
							Constants.TestTypes.PULSE,
							myTreeViewPulses.EventSelectedID))) {
				button_video_play_selected_pulse.Sensitive = true;
				recordedVideo = true;
			}

			success = true;
		} 
		if (type == "ALL" || type == Constants.MultiChronopicName) {
			button_edit_selected_multi_chronopic.Sensitive = show;
			button_delete_selected_multi_chronopic.Sensitive = show;
			
			button_video_play_selected_multi_chronopic.Sensitive = false;
			if (myTreeViewMultiChronopic.EventSelectedID > 0 && File.Exists(Util.GetVideoFileName(
							currentSession.UniqueID, 
							Constants.TestTypes.MULTICHRONOPIC,
							myTreeViewMultiChronopic.EventSelectedID))) {
				button_video_play_selected_multi_chronopic.Sensitive = true;
				recordedVideo = true;
			}

			success = true;
		} 
		if (!success)
			LogB.Error(string.Format("Error in showHideActionEventButtons, type: {0}", type));

		button_video_play_this_test.Sensitive = recordedVideo;
	}
	
	
	/*
	 * voluntary crash for testing purposes 
	 */

	private void on_debug_crash_activate (object o, EventArgs args) {
		bool voluntaryCrashAllowed = true;
		if(voluntaryCrashAllowed) {
			ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString("Done for testing purposes. Chronojump will exit badly"), "", "Are you sure you want to crash application?");
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
