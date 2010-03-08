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
 * Copyright (C) 2004-2009   Xavier de Blas <xaviblas@gmail.com> 
 */


using System;
using Gtk;
using Gdk;
using Glade;
using System.IO.Ports;
using Mono.Unix;
using System.IO; //"File" things
using System.Collections; //ArrayList


public class ChronoJumpWindow 
{
	[Widget] Gtk.Window app1;
	[Widget] Gtk.Statusbar appbar2;
	[Widget] Gtk.TreeView treeview_persons;
	[Widget] Gtk.TreeView treeview_jumps;
	[Widget] Gtk.TreeView treeview_jumps_rj;
	[Widget] Gtk.TreeView treeview_runs;
	[Widget] Gtk.TreeView treeview_runs_interval;
	[Widget] Gtk.TreeView treeview_reaction_times;
	[Widget] Gtk.TreeView treeview_pulses;
	[Widget] Gtk.TreeView treeview_multi_chronopic;
	[Widget] Gtk.Box hbox_combo_jumps;
	[Widget] Gtk.Box hbox_combo_jumps_rj;
	[Widget] Gtk.Box hbox_combo_runs;
	[Widget] Gtk.Box hbox_combo_runs_interval;
	[Widget] Gtk.Box hbox_combo_pulses;
	[Widget] Gtk.Box hbox_jumps;
	[Widget] Gtk.Box hbox_jumps_rj;
	[Widget] Gtk.Box hbox_runs;
	[Widget] Gtk.Box hbox_runs_interval;
	[Widget] Gtk.Box hbox_pulses;
	[Widget] Gtk.ComboBox combo_jumps;
	[Widget] Gtk.ComboBox combo_jumps_rj;
	[Widget] Gtk.ComboBox combo_runs;
	[Widget] Gtk.ComboBox combo_runs_interval;
	[Widget] Gtk.ComboBox combo_pulses;

	//menus
	[Widget] Gtk.MenuItem menu_persons;
	[Widget] Gtk.MenuItem menu_jumps;
	[Widget] Gtk.MenuItem menu_runs;
	[Widget] Gtk.MenuItem menu_other;
	[Widget] Gtk.MenuItem menu_tools;

	//menu session
	[Widget] Gtk.MenuItem menuitem_edit_session;
	[Widget] Gtk.MenuItem menuitem_delete_session;
	[Widget] Gtk.MenuItem menuitem_export_csv;
	[Widget] Gtk.MenuItem menuitem_export_xml;
		
	//menu person
	[Widget] Gtk.MenuItem menuitem_person_add_single;
	[Widget] Gtk.MenuItem menuitem_person_add_multiple;
	[Widget] Gtk.MenuItem menuitem_recuperate_person;
	[Widget] Gtk.MenuItem menuitem_recuperate_persons_from_session;
	[Widget] Gtk.MenuItem menuitem_edit_current_person;
	[Widget] Gtk.MenuItem menuitem_show_all_person_events;
	[Widget] Gtk.MenuItem menuitem_delete_current_person_from_session;
	[Widget] Gtk.Button button_edit_current_person;
	[Widget] Gtk.Button button_show_all_person_events;
	
	//tests
	//jumps
	[Widget] Gtk.MenuItem menu_execute_simple_jumps1;
	[Widget] Gtk.MenuItem menuitem_edit_selected_jump;
	[Widget] Gtk.MenuItem menuitem_delete_selected_jump;
	[Widget] Gtk.MenuItem menu_execute_repetitive_jumps1;
	[Widget] Gtk.MenuItem menuitem_edit_selected_jump_rj;
	[Widget] Gtk.MenuItem menuitem_repair_selected_jump_rj;
	[Widget] Gtk.MenuItem menuitem_delete_selected_jump_rj;
	[Widget] Gtk.Button button_edit_selected_jump;
	[Widget] Gtk.Button button_delete_selected_jump;
	[Widget] Gtk.Button button_edit_selected_jump_rj;
	[Widget] Gtk.Button button_delete_selected_jump_rj;
	[Widget] Gtk.Button button_repair_selected_jump_rj;
	[Widget] Gtk.MenuItem menuitem_jump_type_add;
	[Widget] Gtk.MenuItem menuitem_jump_type_delete_simple;
	[Widget] Gtk.MenuItem menuitem_jump_type_delete_reactive;
	
	[Widget] Gtk.MenuItem menuitem_jump_free;
	[Widget] Gtk.MenuItem sj;
	[Widget] Gtk.MenuItem sj_l;
	[Widget] Gtk.MenuItem cmj;
	[Widget] Gtk.MenuItem cmj_l;
	[Widget] Gtk.MenuItem abk;
	[Widget] Gtk.MenuItem dj;
	[Widget] Gtk.MenuItem menuitem_jump_rocket;
	[Widget] Gtk.MenuItem menuitem_take_off;
	[Widget] Gtk.MenuItem more_simple_jumps;
	[Widget] Gtk.MenuItem more_rj;
	[Widget] Gtk.MenuItem rj_j;
	[Widget] Gtk.MenuItem rj_t;
	[Widget] Gtk.MenuItem rj_unlimited;
	[Widget] Gtk.MenuItem rj_hexagon;

	//runs
	[Widget] Gtk.MenuItem menu_execute_simple_runs1;
	[Widget] Gtk.MenuItem menuitem_edit_selected_run;
	[Widget] Gtk.MenuItem menuitem_delete_selected_run;
	[Widget] Gtk.MenuItem menu_execute_intervallic_runs1;
	[Widget] Gtk.MenuItem menuitem_edit_selected_run_interval;
	[Widget] Gtk.MenuItem menuitem_repair_selected_run_interval;
	[Widget] Gtk.MenuItem menuitem_delete_selected_run_interval;
	[Widget] Gtk.MenuItem menuitem_run_type_add;
	[Widget] Gtk.MenuItem menuitem_run_type_delete_simple;
	[Widget] Gtk.MenuItem menuitem_run_type_delete_intervallic;
	[Widget] Gtk.Button button_edit_selected_run;
	[Widget] Gtk.Button button_delete_selected_run;
	[Widget] Gtk.Button button_edit_selected_run_interval;
	[Widget] Gtk.Button button_delete_selected_run_interval;
	[Widget] Gtk.Button button_repair_selected_run_interval;

	[Widget] Gtk.MenuItem menuitem_run_custom;
	[Widget] Gtk.MenuItem menuitem_20m;
	[Widget] Gtk.MenuItem menuitem_100m;
	[Widget] Gtk.MenuItem menuitem_200m;
	[Widget] Gtk.MenuItem menuitem_400m;
	[Widget] Gtk.MenuItem menuitem_run_gesell;
	[Widget] Gtk.MenuItem menuitem_run_20yard;
	[Widget] Gtk.MenuItem menuitem_run_505;
	[Widget] Gtk.MenuItem menuitem_run_illinois;
	[Widget] Gtk.MenuItem menuitem_run_margaria;
	[Widget] Gtk.MenuItem menuitem_run_shuttle;
	[Widget] Gtk.MenuItem menuitem_run_zigzag;
	[Widget] Gtk.MenuItem menuitem_run_interval_by_laps;
	[Widget] Gtk.MenuItem menuitem_run_interval_by_time;
	[Widget] Gtk.MenuItem menuitem_run_interval_unlimited;
	[Widget] Gtk.MenuItem menuitem_run_interval_mtgug;

	//other
	//reaction time
	[Widget] Gtk.MenuItem menuitem_reaction_time;
	[Widget] Gtk.Button button_edit_selected_reaction_time;
	[Widget] Gtk.Button button_delete_selected_reaction_time;
	//pulse
	[Widget] Gtk.MenuItem menuitem_pulse_free;
	[Widget] Gtk.MenuItem menuitem_pulse_custom;
	[Widget] Gtk.Button button_edit_selected_pulse;
	[Widget] Gtk.Button button_delete_selected_pulse;
	[Widget] Gtk.Button button_repair_selected_pulse;
	//multiChronopic	
	[Widget] Gtk.MenuItem menuitem_multi_chronopic_start;
	[Widget] Gtk.MenuItem menuitem_run_analysis;
	[Widget] Gtk.Button button_edit_selected_multi_chronopic;
	[Widget] Gtk.Button button_delete_selected_multi_chronopic;
	[Widget] Gtk.Box hbox_multi_chronopic_buttons;
	[Widget] Gtk.Button button_multi_chronopic_start;
	[Widget] Gtk.Button button_run_analysis;
	[Widget] Gtk.Entry entry_run_analysis_distance;
	[Widget] Gtk.CheckButton check_multi_sync;
	[Widget] Gtk.CheckButton check_multi_delete_first;
	[Widget] Gtk.Entry entry_multi_chronopic_cp2;

	//tools
	//from SportsTracker code
	[Glade.WidgetAttribute]
		private ImageMenuItem
			menuitem_view_stats = null, menuitem_report_window = null, menuitem_chronopic = null;
	[Widget] Gtk.MenuItem menuitem_server_evaluator_data;
	[Widget] Gtk.MenuItem menuitem_server_upload_session;
	[Widget] Gtk.MenuItem menuitem_preferences;


	//widgets for enable or disable
	[Widget] Gtk.Button button_new;
	[Widget] Gtk.Button button_open;
	[Widget] Gtk.Frame frame_persons;
	[Widget] Gtk.Button button_recup_per;
	[Widget] Gtk.Button button_create_per;

	[Widget] Gtk.Button button_free;
	[Widget] Gtk.Button button_sj;
	[Widget] Gtk.Button button_sj_l;
	[Widget] Gtk.Button button_cmj;
	[Widget] Gtk.Button button_cmj_l;
	[Widget] Gtk.Button button_abk;
	[Widget] Gtk.Button button_dj;
	[Widget] Gtk.Button button_rocket;
	[Widget] Gtk.Button button_take_off;
	[Widget] Gtk.Button button_more;
	[Widget] Gtk.Button button_rj_j;
	[Widget] Gtk.Button button_rj_t;
	[Widget] Gtk.Button button_rj_unlimited;
	[Widget] Gtk.Button button_rj_hexagon;
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
	[Widget] Gtk.Button button_pulse_free;
	[Widget] Gtk.Button button_pulse_custom;
	[Widget] Gtk.Button button_reaction_time;
	//[Widget] Gtk.Button button_pulse_more;

	
	[Widget] Gtk.Button button_last;
	[Widget] Gtk.Button button_rj_last;
	[Widget] Gtk.Button button_run_last;
	[Widget] Gtk.Button button_run_interval_last;

	[Widget] Gtk.Notebook notebook;
	
	[Widget] Gtk.Box vbox_image_test;
	[Widget] Gtk.Image image_test;
	[Widget] Gtk.Button button_image_test_zoom;
	[Widget] Gtk.Image image_test_zoom;
	[Widget] Gtk.Label label_image_test;

	//non standard icons	
	[Widget] Gtk.Image image_volume;
	[Widget] Gtk.Image image_jump_reactive_bell;
	[Widget] Gtk.Image image_run_interval_bell;
	[Widget] Gtk.Image image_jump_reactive_repair;
	[Widget] Gtk.Image image_run_interval_repair;
	[Widget] Gtk.Image image_pulse_repair;
	[Widget] Gtk.Image image_jump_delete;
	[Widget] Gtk.Image image_jump_reactive_delete;
	[Widget] Gtk.Image image_run_delete;
	[Widget] Gtk.Image image_run_interval_delete;
	[Widget] Gtk.Image image_reaction_time_delete;
	[Widget] Gtk.Image image_pulse_delete;
	[Widget] Gtk.Image image_multi_chronopic_delete;

	[Widget] Gtk.Image image_jumps_zoom;
	[Widget] Gtk.Image image_jumps_rj_zoom;
	[Widget] Gtk.Image image_runs_zoom;
	[Widget] Gtk.Image image_runs_interval_zoom;
	[Widget] Gtk.Image image_reaction_times_zoom;
	[Widget] Gtk.Image image_pulses_zoom;

	Random rand;
	bool volumeOn;

	//persons
	private TreeStore treeview_persons_store;
	private TreeViewPersons myTreeViewPersons;
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

	//preferences variables
	private static int prefsDigitsNumber;
	private static bool showHeight;
	private static bool showPower;
	private static bool showInitialSpeed;
	private static bool showAngle;
	private static bool showQIndex;
	private static bool showDjIndex;
	private static bool askDeletion;
	private static bool weightPercentPreferred;
	private static bool heightPreferred;
	private static bool metersSecondsPreferred;
	private static bool allowFinishRjAfterTime;

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
	private static RunType currentRunType;
	private static PulseType currentPulseType;
	private static MultiChronopicType currentMultiChronopicType;
	private static Report report;

	//windows needed
	//LanguageWindow languageWin;
	SessionAddEditWindow sessionAddEditWin;
	//SessionEditWindow sessionEditWin;
	SessionLoadWindow sessionLoadWin;
	PersonRecuperateWindow personRecuperateWin; 
	PersonsRecuperateFromOtherSessionWindow personsRecuperateFromOtherSessionWin; 
	PersonAddModifyWindow personAddModifyWin; 
	PersonAddMultipleWindow personAddMultipleWin; 
	PersonShowAllEventsWindow personShowAllEventsWin;
	JumpsMoreWindow jumpsMoreWin;
	JumpsRjMoreWindow jumpsRjMoreWin;
	JumpExtraWindow jumpExtraWin; //for normal and repetitive jumps 
	EditJumpWindow editJumpWin;
	EditJumpRjWindow editJumpRjWin;
	RepairJumpRjWindow repairJumpRjWin;
	JumpTypeAddWindow jumpTypeAddWin;
	
	RunExtraWindow runExtraWin; //for normal and intervaled runs 
	RunsMoreWindow runsMoreWin;
	RunsIntervalMoreWindow runsIntervalMoreWin;
	RunTypeAddWindow runTypeAddWin;
	EditRunWindow editRunWin;
	RepairRunIntervalWindow repairRunIntervalWin;
	EditRunIntervalWindow editRunIntervalWin;

	EditReactionTimeWindow editReactionTimeWin;

	EditPulseWindow editPulseWin;
	PulseExtraWindow pulseExtraWin;
	RepairPulseWindow repairPulseWin;
	
	EditMultiChronopicWindow editMultiChronopicWin;
	
	ConfirmWindowJumpRun confirmWinJumpRun;	//for deleting jumps and RJ jumps (and runs)
	ErrorWindow errorWin;
	StatsWindow statsWin;
	ReportWindow reportWin;
	RepetitiveConditionsWindow repetitiveConditionsWin;
	GenericWindow genericWin;
		
	EvaluatorWindow evalWin;
	QueryServerWindow queryServerWin;
	PersonNotUploadWindow personNotUploadWin; 
	
	ChronopicWindow chronopicWin;
	
	static EventExecuteWindow eventExecuteWin;

	private bool firstTestInSession = true; //since we started chronojump
	private bool firstRjValue;
	private double rjTcCount;
	private double rjTvCount;
	private string rjTcString;
	private string rjTvString;
	
	private bool createdStatsWin;
	
	private bool preferencesLoaded;

	private string progVersion;
	private string progName;

	private string runningFileName; //useful for knowing if there are two chronojump instances

	//const int statusbarID = 1;


	//only called the first time the software runs
	//and only on windows
	private void on_language_clicked(object o, EventArgs args) {
		//languageChange();
		//createMainWindow("");
	}

	private void on_button_image_test_zoom_clicked(object o, EventArgs args) {
		new DialogImageTest(currentEventType);
	}
	
	
	public ChronoJumpWindow(string progVersion, string progName, string runningFileName)
	{
		this.progVersion = progVersion;
		this.progName = progName;
		this.runningFileName = runningFileName;

		Glade.XML gxml;
		gxml = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "app1", null);
		gxml.Autoconnect(this);

		//put an icon to window
		UtilGtk.IconWindow(app1);

		//show chronojump logo on down-left area
		changeTestImage("", "", "LOGO");
				
		//new DialogMessage(Constants.MessageTypes.INFO, UtilGtk.ScreenHeightFitted(false).ToString() );
		//UtilGtk.ResizeIfNeeded(stats_window);
		app1.Maximize();

		report = new Report(-1); //when a session is loaded or created, it will change the report.SessionID value
		//TODO: check what happens if a session it's deleted
		//i think report it's deactivated until a new session is created or loaded, 
		//but check what happens if report window is opened


		//preferencesLoaded is a fix to a gtk#-net-windows-bug where radiobuttons raise signals
		//at initialization of chronojump and gives problems if this signals are raised while preferences are loading
		preferencesLoaded = false;
		loadPreferences ();
		preferencesLoaded = true;

		createTreeView_persons (treeview_persons);
		createTreeView_jumps (treeview_jumps);
		createTreeView_jumps_rj (treeview_jumps_rj);
		createTreeView_runs (treeview_runs);
		createTreeView_runs_interval (treeview_runs_interval);
		createTreeView_reaction_times (treeview_reaction_times);
		createTreeView_pulses (treeview_pulses);
		createTreeView_multi_chronopic (treeview_multi_chronopic);

		createComboJumps();
		createComboJumpsRj();
		createComboRuns();
		createComboRunsInterval();
		//reaction_times has no combo
		createComboPulses();
		createComboMultiChronopic();
		createdStatsWin = false;

		
		repetitiveConditionsWin = RepetitiveConditionsWindow.Create();

		ArrayList cpd = new ArrayList();
		for(int i=1; i<=4;i++) {
			ChronopicPortData a = new ChronopicPortData(i,"",false);
			cpd.Add(a);
		}
		chronopicWin = ChronopicWindow.Create(cpd);


		//We have no session, mark some widgets as ".Sensitive = false"
		sensitiveGuiNoSession();
		definedSession = false;
		
		//if(recuperatedString == "")
			appbar2.Push ( 1, Catalog.GetString ("Ready.") );
		//else
		//	appbar2.Push ( 1, recuperatedString );

		rand = new Random(40);
	
		putNonStandardIcons();	
		/*
	
		if(chronopicPort != Constants.ChronopicDefaultPortWindows && 
				(chronopicPort != Constants.ChronopicDefaultPortLinux && File.Exists(chronopicPort))
		  ) {
			ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString("Do you want to connect to Chronopic now?"), "", "");
			confirmWin.Button_accept.Clicked += new EventHandler(chronopicAtStart);
		}
		*/
	}
/*
	private void chronopicAtStart(object o, EventArgs args) {
		//make active menuitem chronopic, and this
		//will raise other things
		//menuitem_chronopic.Active = true;
	}
*/


	private void putNonStandardIcons() {
		Pixbuf pixbuf;
		
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_bell.png");
		image_jump_reactive_bell.Pixbuf = pixbuf;
		image_run_interval_bell.Pixbuf = pixbuf;
		
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "preferences-system.png");
		image_jump_reactive_repair.Pixbuf = pixbuf;
		image_run_interval_repair.Pixbuf = pixbuf;
		image_pulse_repair.Pixbuf = pixbuf;
		
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_delete.png");
		image_jump_delete.Pixbuf = pixbuf;
		image_jump_reactive_delete.Pixbuf = pixbuf;
		image_run_delete.Pixbuf = pixbuf;
		image_run_interval_delete.Pixbuf = pixbuf;
		image_reaction_time_delete.Pixbuf = pixbuf;
		image_pulse_delete.Pixbuf = pixbuf;
		image_multi_chronopic_delete.Pixbuf = pixbuf;
		
		//zoom icons, done like this because there's one zoom icon created ad-hoc, 
		//and is not nice that the other are different for an user theme change
	
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameZoomFitIcon);
		image_jumps_zoom.Pixbuf = pixbuf;
		image_jumps_rj_zoom.Pixbuf = pixbuf;
		image_runs_zoom.Pixbuf = pixbuf;
		image_runs_interval_zoom.Pixbuf = pixbuf;
		image_reaction_times_zoom.Pixbuf = pixbuf;
		image_pulses_zoom.Pixbuf = pixbuf;

		//menuitems (done differently)
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "chronopic_24.png");
		menuitem_chronopic.Image = new Gtk.Image(pixbuf);
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "gpm-statistics.png");
		menuitem_view_stats.Image = new Gtk.Image(pixbuf);
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_task-assigned.png");
		menuitem_report_window.Image = new Gtk.Image(pixbuf);
	}

	private void loadPreferences () 
	{
		Log.WriteLine (string.Format(Catalog.GetString("Chronojump database version file: {0}"), 
				SqlitePreferences.Select("databaseVersion") ));
		
		//chronopicPort = SqlitePreferences.Select("chronopicPort");
		
		prefsDigitsNumber = Convert.ToInt32 ( SqlitePreferences.Select("digitsNumber") );

	
		if ( SqlitePreferences.Select("allowFinishRjAfterTime") == "True" ) 
			allowFinishRjAfterTime = true;
		 else 
			allowFinishRjAfterTime = false;
		
			
		if ( SqlitePreferences.Select("showHeight") == "True" ) 
			showHeight = true;
		 else 
			showHeight = false;
		
		if ( SqlitePreferences.Select("showPower") == "True" ) 
			showPower = true;
		 else 
			showPower = false;
			
		if ( SqlitePreferences.Select("showInitialSpeed") == "True" ) 
			showInitialSpeed = true;
		 else 
			showInitialSpeed = false;
		
		if ( SqlitePreferences.Select("showAngle") == "True" ) 
			showAngle = true;
		 else 
			showAngle = false;
		
		
		//only one of showQIndex or showDjIndex can be true. Also none of them
		if ( SqlitePreferences.Select("showQIndex") == "True" ) 
			showQIndex = true;
		 else 
			showQIndex = false;
		
			
		if ( SqlitePreferences.Select("showDjIndex") == "True" ) 
			showDjIndex = true;
		 else 
			showDjIndex = false;
		
			
		
		if ( SqlitePreferences.Select("simulated") == "True" ) {
//			simulated = true;
			//menuitem_simulated.Active = true;

//			cpRunning = false;
		} else {
//			simulated = false;
			//menuitem_chronopic.Active = true;
			
//			cpRunning = true;
		}
		
		if ( SqlitePreferences.Select("askDeletion") == "True" ) 
			askDeletion = true;
		 else 
			askDeletion = false;
		

		if ( SqlitePreferences.Select("weightStatsPercent") == "True" ) 
			weightPercentPreferred = true;
		 else 
			weightPercentPreferred = false;
		
		
		if ( SqlitePreferences.Select("heightPreferred") == "True" ) 
			heightPreferred = true;
		 else 
			heightPreferred = false;
		
		
		if ( SqlitePreferences.Select("metersSecondsPreferred") == "True" ) 
			metersSecondsPreferred = true;
		 else 
			metersSecondsPreferred = false;
		
		if ( SqlitePreferences.Select("volumeOn") == "True" ) 
			volumeOn = true;
		 else 
			volumeOn = false;
		changeVolumeButton(volumeOn);
		
		//volumeOn = true;
	
		//change language works on windows. On Linux let's change the locale
		//if(Util.IsWindows())
		//	languageChange();
			
		//pass to report
		report.PrefsDigitsNumber = prefsDigitsNumber;
		report.HeightPreferred = heightPreferred;
		report.WeightStatsPercent = weightPercentPreferred;
		report.Progversion = progVersion;
		
		
		Log.WriteLine ( Catalog.GetString ("Preferences loaded") );
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
					Jump myJump = SqliteJump.SelectJumpData( myTreeViewJumps.EventSelectedID );
					treeviewJumpsContextMenu(myJump);
				}
			} else if(myTv == treeview_jumps_rj) {
				if (myTreeViewJumpsRj.EventSelectedID > 0) {
					JumpRj myJump = SqliteJumpRj.SelectJumpData( "jumpRj", myTreeViewJumpsRj.EventSelectedID );
					treeviewJumpsRjContextMenu(myJump);
				}
			} else if(myTv == treeview_runs) {
				if (myTreeViewRuns.EventSelectedID > 0) {
					Run myRun = SqliteRun.SelectRunData( myTreeViewRuns.EventSelectedID );
					treeviewRunsContextMenu(myRun);
				}
			} else if(myTv == treeview_runs_interval) {
				if (myTreeViewRunsInterval.EventSelectedID > 0) {
					RunInterval myRun = SqliteRunInterval.SelectRunData( Constants.RunIntervalTable, myTreeViewRunsInterval.EventSelectedID );
					treeviewRunsIntervalContextMenu(myRun);
				}
			} else if(myTv == treeview_reaction_times) {
				if (myTreeViewReactionTimes.EventSelectedID > 0) {
					ReactionTime myRt = SqliteReactionTime.SelectReactionTimeData( myTreeViewReactionTimes.EventSelectedID );
					treeviewReactionTimesContextMenu(myRt);
				}
			} else if(myTv == treeview_pulses) {
				if (myTreeViewPulses.EventSelectedID > 0) {
					Pulse myPulse = SqlitePulse.SelectPulseData( myTreeViewPulses.EventSelectedID );
					treeviewPulsesContextMenu(myPulse);
				}
			} else if(myTv == treeview_multi_chronopic) {
				if (myTreeViewMultiChronopic.EventSelectedID > 0) {
					MultiChronopic mc = SqliteMultiChronopic.SelectMultiChronopicData( myTreeViewMultiChronopic.EventSelectedID );
					treeviewMultiChronopicContextMenu(mc);
				}
			} else
				Log.WriteLine(myTv.ToString());
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
		ArrayList myPersons = SqlitePersonSession.SelectCurrentSessionPersons(currentSession.UniqueID); 

		if(myPersons.Count > 0) {
			//fill treeview
			myTreeViewPersons.Fill(myPersons);
		}
	}

	private int findRowOfCurrentPerson(Gtk.TreeView tv, TreeStore store, Person currentPerson) {
		return myTreeViewPersons.FindRow(currentPerson.UniqueID);
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
			return true;
		} else {
			return false;
		}
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
		}
	}

	private void treeviewPersonsContextMenu(Person myPerson) {
		Menu myMenu = new Menu ();
		Gtk.MenuItem myItem;

		myItem = new MenuItem ( Catalog.GetString("Edit") + " " + myPerson.Name);
		myItem.Activated += on_edit_current_person_clicked;
		myMenu.Attach( myItem, 0, 1, 0, 1 );

		myItem = new MenuItem ( Catalog.GetString("Show all tests of") + " " + myPerson.Name);
		myItem.Activated += on_show_all_person_events_activate;
		myMenu.Attach( myItem, 0, 1, 1, 2 );

		Gtk.SeparatorMenuItem mySep = new SeparatorMenuItem();
		myMenu.Attach( mySep, 0, 1, 2, 3 );

		myItem = new MenuItem ( string.Format(Catalog.GetString("Delete {0} from this session"),myPerson.Name));
		myItem.Activated += on_delete_current_person_from_session_activate;
		myMenu.Attach( myItem, 0, 1, 3, 4 );

		myMenu.Popup();
		myMenu.ShowAll();
	}
		
	/* ---------------------------------------------------------
	 * ----------------  SERVER CALLS --------------------------
	 *  --------------------------------------------------------
	 */

	/* 
	 * SERVER CALLBACKS
	 */

	bool serverEvaluatorDoing;
	// upload session and it's persons (callback)
	private void on_menuitem_server_upload_session_pre (object o, EventArgs args) {
		//evaluator stuff
		//Server.ServerUploadEvaluator();
		string evalMessage = "";
		int evalSID = Convert.ToInt32(SqlitePreferences.Select("evaluatorServerID"));
		if(evalSID == Constants.ServerUndefinedID) 
			evalMessage = Catalog.GetString("Please, first fill evaluator data.");
		else 
			evalMessage = Catalog.GetString("Please, first check evaluator data is ok.");
		
		appbar2.Push ( 1, evalMessage );
		
		server_evaluator_data_and_after_upload_session();
	}

	private bool connectedAndCanI (string serverAction) {
		string versionAvailable = Server.Ping(false, "", ""); //false: don't do insertion
		if(versionAvailable != Constants.ServerOffline) { //false: don't do insertion
			if(Server.CanI(serverAction, progVersion))
				return true;
			else
				new DialogMessage(Constants.MessageTypes.WARNING, 
						Catalog.GetString("Your version of Chronojump is too old for this.") + "\n\n" + 
						Catalog.GetString("Please, update to new version: ") + versionAvailable + "\n");
		} else 
			new DialogMessage(Constants.MessageTypes.WARNING, Constants.ServerOffline);

		return false;
	}

	private void on_menuitem_server_stats (object o, EventArgs args) {
		if(connectedAndCanI(Constants.ServerActionStats)) {
			ChronojumpServer myServer = new ChronojumpServer();
			Log.WriteLine(myServer.ConnectDatabase());

			string [] statsServer = myServer.Stats();

			Log.WriteLine(myServer.DisConnectDatabase());

			string [] statsMine = SqliteServer.StatsMine();

			new DialogServerStats(statsServer, statsMine);
		}
	}
	
	private void on_menuitem_server_query_activate (object o, EventArgs args) {
		if(connectedAndCanI(Constants.ServerActionQuery)) 
			queryServerWin = QueryServerWindow.Show(prefsDigitsNumber);
	}
	
	private void on_menuitem_server_ping (object o, EventArgs args) {
		string str = Server.Ping(false, progName, progVersion); //don't do insertion (will show versionAvailable)
		//show online or offline (not the next version of client available)
		if(str != Constants.ServerOffline)
			str = Catalog.GetString(Constants.ServerOnline);
		new DialogMessage(Constants.MessageTypes.INFO, str);
	}
	
	bool uploadSessionAfter;

	//called when after that has to continue with upload session
	private void server_evaluator_data_and_after_upload_session() {
		appbar2.Push ( 1, "" );
		uploadSessionAfter = true;
		server_evaluator_data (); 
	}
	
	//called when only has to be created/updated the evaluator (not update session)
	private void on_menuitem_server_evaluator_data_only (object o, EventArgs args) {
		uploadSessionAfter = false;
		server_evaluator_data (); 
	}
	
	private void server_evaluator_data () {
		ServerEvaluator myEval = SqliteServer.SelectEvaluator(1); 
		evalWin = EvaluatorWindow.Show(myEval);
		evalWin.FakeButtonAccept.Clicked += new EventHandler(on_evaluator_done);
	}

	private void on_evaluator_done (object o, EventArgs args) {
		if(evalWin.Changed) {
			string versionAvailable = Server.Ping(false, "", ""); //false: don't do insertion
			if(versionAvailable != Constants.ServerOffline) { //false: don't do insertion
				ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString("Do you want to upload evaluator data now?"), "", "");
				confirmWin.Button_accept.Clicked += new EventHandler(on_evaluator_upload_accepted);
			} else 
				new DialogMessage(Constants.MessageTypes.WARNING, 
						Catalog.GetString("Currently cannot upload.") + "\n\n" + Constants.ServerOffline);
		}
		else
			if(uploadSessionAfter)
				select_persons_to_discard ();

	}

	private void on_evaluator_upload_accepted (object o, EventArgs args) {
		Server.ServerUploadEvaluator();
		if(uploadSessionAfter)
			select_persons_to_discard ();
	}

	private void select_persons_to_discard () {
		personNotUploadWin = PersonNotUploadWindow.Show(app1, currentSession.UniqueID);
		personNotUploadWin.FakeButtonDone.Clicked += new EventHandler(on_select_persons_to_discard_done);
	}
	
	private void on_select_persons_to_discard_done (object o, EventArgs args) {
		server_upload_session();
	}

	private void on_menuitem_goto_server_website_activate (object o, EventArgs args) {
		System.Diagnostics.Process.Start(Constants.ChronojumpWebsite + Path.DirectorySeparatorChar + "server.html");
	}

	/* 
	 * SERVER CODE
	 */

	private bool checkPersonsMissingData() 
	{
		ArrayList impossibleWeight = new ArrayList(1);
		ArrayList undefinedCountry = new ArrayList(1); //country is required for server
		ArrayList undefinedSport = new ArrayList(1);
		
		ArrayList notToUpload = SqlitePersonSessionNotUpload.SelectAll(currentSession.UniqueID);
		ArrayList persons = SqlitePersonSession.SelectCurrentSessionPersons(currentSession.UniqueID);
		foreach (Person person in persons) 
		{
			if(! Util.FoundInArrayList(notToUpload, person.UniqueID.ToString())) 
			{
				PersonSession ps = SqlitePersonSession.Select(person.UniqueID, currentSession.UniqueID);
				if(ps.Weight <= 10 || ps.Weight >= 300)
					impossibleWeight.Add(person);
				if(person.CountryID == Constants.CountryUndefinedID)
					undefinedCountry.Add(person);
				if(ps.SportID == Constants.SportUndefinedID)
					undefinedSport.Add(person);
				//speciallity and level not required, because person gui obligates to select them when sport is selected
			}
		}

		string weightString = "";
		string countryString = "";
		string sportString = "";
		
		int maxPeopleFail = 7;

		if(impossibleWeight.Count > 0) {
			weightString += "\n\n" + Catalog.GetString("<b>Weight</b> of the following persons is not ok:") + "\n";
			string separator = "";
			int count=0;
			foreach(Person person in impossibleWeight) {
				weightString += separator + person.Name;
				separator = ", ";
				if(++count >= maxPeopleFail) {
					weightString += "...";
					break;
				}
			}
		}

		if(undefinedCountry.Count > 0) {
			countryString += "\n\n" + Catalog.GetString("<b>Country</b> of the following persons is undefined:") + "\n";
			string separator = "";
			int count=0;
			foreach(Person person in undefinedCountry) {
				countryString += separator + person.Name;
				separator = ", ";
				if(++count >= maxPeopleFail) {
					countryString += "...";
					break;
				}
			}
		}

		if(undefinedSport.Count > 0) {
			sportString += "\n\n" + Catalog.GetString("<b>Sport</b> of the following persons is undefined:") + "\n";
			string separator = "";
			int count=0;
			foreach(Person person in undefinedSport) {
				sportString += separator + person.Name;
				separator = ", ";
				if(++count >= maxPeopleFail) {
					sportString += "...";
					break;
				}
			}
		}

		if(weightString.Length > 0 || countryString.Length > 0 || sportString.Length > 0) {
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Please, fix this before uploading:") +
						weightString + countryString + sportString + "\n\n" + 
						Catalog.GetString("Or when upload session again, mark these persons as not to be uploaded.")
						);
			return true; //data is missing
		}
		else
			return false; //data is ok

	}
			
	private void server_upload_session () 
	{
		int evalSID = Convert.ToInt32(SqlitePreferences.Select("evaluatorServerID"));
		if(evalSID != Constants.ServerUndefinedID) {
			if(!checkPersonsMissingData()) {
				string message1 = ""; 
				if(currentSession.ServerUniqueID == Constants.ServerUndefinedID) 
					message1 =  
							Catalog.GetString("Session will be uploaded to server.") + "\n" +  
							Catalog.GetString("Names, date of birth and descriptions of persons will be hidden.") + "\n\n" + 
							Catalog.GetString("You can upload again this session if you add more data or persons.");
				else
					message1 =  
							Catalog.GetString("Session has been uploaded to server before.") + "\n" +  
							Catalog.GetString("Uploading new data.");

				message1 += "\n\n" + Catalog.GetString("All the uploaded data will be licensed as:") + 
						"\n<b>" + Catalog.GetString("Creative Commons Attribution 3.0") + "</b>";


				ConfirmWindow confirmWin = ConfirmWindow.Show(message1, 
							"<u>http://creativecommons.org/licenses/by/3.0/</u>", //label_link
							Catalog.GetString("Are you sure you want to upload this session to server?"));
				confirmWin.Button_accept.Clicked += new EventHandler(on_server_upload_session_accepted);
			}
		}
	}


	private void on_server_upload_session_accepted (object o, EventArgs args) 
	{
		if(connectedAndCanI(Constants.ServerActionUploadSession)) {
			Server.InitializeSessionVariables(app1, currentSession, progName, progVersion);
			Server.ThreadStart();
		}
	}

	private void resetAllTreeViews( bool alsoPersons) {
		if(alsoPersons) {
			//load the persons treeview
			treeview_persons_storeReset();
			fillTreeView_persons();
		}

		//load the jumps treeview
		treeview_jumps_storeReset();
		fillTreeView_jumps(Constants.AllJumpsName);

		//load the jumps_rj treeview_rj
		treeview_jumps_rj_storeReset();
		fillTreeView_jumps_rj(Constants.AllJumpsName);

		//load the runs treeview
		treeview_runs_storeReset();
		fillTreeView_runs(Constants.AllRunsName);

		//load the runs_interval treeview
		treeview_runs_interval_storeReset();
		fillTreeView_runs_interval(Constants.AllRunsName);

		//load the pulses treeview
		treeview_pulses_storeReset();
		fillTreeView_pulses(Constants.AllPulsesName);

		//load the reaction_times treeview
		treeview_reaction_times_storeReset();
		fillTreeView_reaction_times();

		//load the multiChronopic treeview
		treeview_multi_chronopic_storeReset();
		fillTreeView_multi_chronopic();
	}


	/* ---------------------------------------------------------
	 * ----------------  TREEVIEW JUMPS ------------------------
	 *  --------------------------------------------------------
	 */

	private void createTreeView_jumps (Gtk.TreeView tv) {
		//myTreeViewJumps is a TreeViewJumps instance
		myTreeViewJumps = new TreeViewJumps( tv, showHeight, showPower, showInitialSpeed, showAngle, showQIndex, showDjIndex, prefsDigitsNumber, weightPercentPreferred, metersSecondsPreferred, TreeViewEvent.ExpandStates.MINIMIZED);

		//the glade cursor_changed does not work on mono 1.2.5 windows
		tv.CursorChanged += on_treeview_jumps_cursor_changed; 
	}

	private void fillTreeView_jumps (string filter) {
		string [] myJumps;
	
		myJumps = SqliteJump.SelectJumps(currentSession.UniqueID, -1, "", "");
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
		
		myTreeViewJumps = new TreeViewJumps( treeview_jumps, showHeight, showPower, showInitialSpeed, showAngle, showQIndex, showDjIndex, prefsDigitsNumber, weightPercentPreferred, metersSecondsPreferred, myTreeViewJumps.ExpandState );
	}

	private void on_treeview_jumps_cursor_changed (object o, EventArgs args) {
		Log.WriteLine("Cursor changed");
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
		myTreeViewJumpsRj = new TreeViewJumpsRj( tv, showHeight, showInitialSpeed, showQIndex, showDjIndex, prefsDigitsNumber, weightPercentPreferred, metersSecondsPreferred, TreeViewEvent.ExpandStates.MINIMIZED );

		//the glade cursor_changed does not work on mono 1.2.5 windows
		tv.CursorChanged += on_treeview_jumps_rj_cursor_changed; 
	}

	private void fillTreeView_jumps_rj (string filter) {
		string [] myJumps;
		myJumps = SqliteJumpRj.SelectJumps(currentSession.UniqueID, -1, "", "");
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
		myTreeViewJumpsRj = new TreeViewJumpsRj( treeview_jumps_rj, showHeight, showInitialSpeed, showQIndex, showDjIndex, prefsDigitsNumber, weightPercentPreferred, metersSecondsPreferred, myTreeViewJumpsRj.ExpandState );
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
		myTreeViewRuns = new TreeViewRuns( tv, prefsDigitsNumber, metersSecondsPreferred, TreeViewEvent.ExpandStates.MINIMIZED );

		//the glade cursor_changed does not work on mono 1.2.5 windows
		tv.CursorChanged += on_treeview_runs_cursor_changed; 
	}

	private void fillTreeView_runs (string filter) {
		string [] myRuns = SqliteRun.SelectRuns(currentSession.UniqueID, -1, "");
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
		myTreeViewRuns = new TreeViewRuns( treeview_runs, prefsDigitsNumber, metersSecondsPreferred, myTreeViewRuns.ExpandState );
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
		myTreeViewRunsInterval = new TreeViewRunsInterval( tv, prefsDigitsNumber, metersSecondsPreferred, TreeViewEvent.ExpandStates.MINIMIZED );

		//the glade cursor_changed does not work on mono 1.2.5 windows
		tv.CursorChanged += on_treeview_runs_interval_cursor_changed; 
	}

	private void fillTreeView_runs_interval (string filter) {
		string [] myRuns = SqliteRunInterval.SelectRuns(currentSession.UniqueID, -1, "");
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
		myTreeViewRunsInterval = new TreeViewRunsInterval( treeview_runs_interval,  
				prefsDigitsNumber, metersSecondsPreferred, myTreeViewRunsInterval.ExpandState );
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
		myTreeViewReactionTimes = new TreeViewReactionTimes( tv, prefsDigitsNumber, TreeViewEvent.ExpandStates.MINIMIZED );

		//the glade cursor_changed does not work on mono 1.2.5 windows
		tv.CursorChanged += on_treeview_reaction_times_cursor_changed; 
	}

	//private void fillTreeView_reaction_times (string filter) {
	private void fillTreeView_reaction_times () {
		string [] myRTs = SqliteReactionTime.SelectReactionTimes(currentSession.UniqueID, -1);
		myTreeViewReactionTimes.Fill(myRTs, "");
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
		myTreeViewReactionTimes = new TreeViewReactionTimes( treeview_reaction_times, prefsDigitsNumber, myTreeViewReactionTimes.ExpandState );
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
		myTreeViewPulses = new TreeViewPulses( tv, prefsDigitsNumber, TreeViewEvent.ExpandStates.MINIMIZED );

		//the glade cursor_changed does not work on mono 1.2.5 windows
		tv.CursorChanged += on_treeview_pulses_cursor_changed; 
	}

	private void fillTreeView_pulses (string filter) {
		string [] myPulses = SqlitePulse.SelectPulses(currentSession.UniqueID, -1);
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
		myTreeViewPulses = new TreeViewPulses( treeview_pulses, prefsDigitsNumber, myTreeViewPulses.ExpandState );
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

	private void createTreeView_multi_chronopic (Gtk.TreeView tv) {
		//myTreeViewMultiChronopic is a TreeViewMultiChronopic instance
		if(definedSession)
			myTreeViewMultiChronopic = new TreeViewMultiChronopic( tv, prefsDigitsNumber, 
					TreeViewEvent.ExpandStates.MINIMIZED, SqliteMultiChronopic.MaxCPs(currentSession.UniqueID) );
		else
			myTreeViewMultiChronopic = new TreeViewMultiChronopic( tv, prefsDigitsNumber, 
					TreeViewEvent.ExpandStates.MINIMIZED, 2);

		//the glade cursor_changed does not work on mono 1.2.5 windows
		tv.CursorChanged += on_treeview_multi_chronopic_cursor_changed; 
	}
	
	private void fillTreeView_multi_chronopic () {
		string [] mcs = SqliteMultiChronopic.SelectTests(currentSession.UniqueID, -1);
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
	
	private void treeview_multi_chronopic_storeReset() {
		myTreeViewMultiChronopic.RemoveColumns();
		if(definedSession)
			myTreeViewMultiChronopic = new TreeViewMultiChronopic( treeview_multi_chronopic, prefsDigitsNumber, 
					myTreeViewMultiChronopic.ExpandState, SqliteMultiChronopic.MaxCPs(currentSession.UniqueID) );
		else
			myTreeViewMultiChronopic = new TreeViewMultiChronopic( treeview_multi_chronopic, prefsDigitsNumber, 
					myTreeViewMultiChronopic.ExpandState, 2);
	}

	private void on_treeview_multi_chronopic_cursor_changed (object o, EventArgs args) {
		Log.WriteLine("Cursor changed");
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

		myItem = new MenuItem ( Catalog.GetString("Edit selected") + " " + mc.Type + " (" + mc.PersonName + ")");
		myItem.Activated += on_edit_selected_multi_chronopic_clicked;
		myMenu.Attach( myItem, 0, 1, 0, 1 );

		/*
		myItem = new MenuItem ( Catalog.GetString("Repair selected") + " " + mc.Type + " (" + mc.PersonName + ")");
		myItem.Activated += on_repair_selected_multi_chronopic_clicked;
		myMenu.Attach( myItem, 0, 1, 1, 2 );
		*/
		
		Gtk.SeparatorMenuItem mySep = new SeparatorMenuItem();
		//myMenu.Attach( mySep, 0, 1, 2, 3 );
		myMenu.Attach( mySep, 0, 1, 1, 2 );

		myItem = new MenuItem ( Catalog.GetString("Delete selected") + " " + mc.Type + " (" + mc.PersonName + ")");
		myItem.Activated += on_delete_selected_multi_chronopic_clicked;
		//myMenu.Attach( myItem, 0, 1, 3, 4 );
		myMenu.Attach( myItem, 0, 1, 2, 3 );

		myMenu.Popup();
		myMenu.ShowAll();
	}




	/* ---------------------------------------------------------
	 * ----------------  CREATE AND UPDATE COMBOS ---------------
	 *  --------------------------------------------------------
	 */
	private void createComboJumps() {
		combo_jumps = ComboBox.NewText ();
		UtilGtk.ComboUpdate(combo_jumps, SqliteJumpType.SelectJumpTypes(Constants.AllJumpsName, "", true), ""); //without filter, only select name
		
		combo_jumps.Active = 0;
		combo_jumps.Changed += new EventHandler (on_combo_jumps_changed);

		hbox_combo_jumps.PackStart(combo_jumps, true, true, 0);
		hbox_combo_jumps.ShowAll();
		combo_jumps.Sensitive = false;
	}
	
	private void createComboJumpsRj() {
		combo_jumps_rj = ComboBox.NewText();
		UtilGtk.ComboUpdate(combo_jumps_rj, SqliteJumpType.SelectJumpRjTypes(Constants.AllJumpsName, true), ""); //only select name
		
		combo_jumps_rj.Active = 0;
		combo_jumps_rj.Changed += new EventHandler (on_combo_jumps_rj_changed);

		hbox_combo_jumps_rj.PackStart(combo_jumps_rj, true, true, 0);
		hbox_combo_jumps_rj.ShowAll();
		combo_jumps_rj.Sensitive = false;
	}
	
	private void createComboRuns() {
		combo_runs = ComboBox.NewText();
		UtilGtk.ComboUpdate(combo_runs, SqliteRunType.SelectRunTypes(Constants.AllRunsName, true), ""); //without filter, only select name
		
		combo_runs.Active = 0;
		combo_runs.Changed += new EventHandler (on_combo_runs_changed);

		hbox_combo_runs.PackStart(combo_runs, true, true, 0);
		hbox_combo_runs.ShowAll();
		combo_runs.Sensitive = false;
	}

	private void createComboRunsInterval() {
		combo_runs_interval = ComboBox.NewText();
		UtilGtk.ComboUpdate(combo_runs_interval, SqliteRunIntervalType.SelectRunIntervalTypes(Constants.AllRunsName, true), ""); //without filter, only select name
		
		combo_runs_interval.Active = 0;
		combo_runs_interval.Changed += new EventHandler (on_combo_runs_interval_changed);

		hbox_combo_runs_interval.PackStart(combo_runs_interval, true, true, 0);
		hbox_combo_runs_interval.ShowAll();
		combo_runs_interval.Sensitive = false;
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

	private void createComboMultiChronopic() 
	{
		menuitem_multi_chronopic_start.Sensitive = false;
		menuitem_run_analysis.Sensitive = false;
		button_multi_chronopic_start.Sensitive = false;
		button_run_analysis.Sensitive = false;
		entry_run_analysis_distance.Sensitive = false;
	}

	private void on_combo_jumps_changed(object o, EventArgs args) {
		//combo_jumps.Changed -= new EventHandler (on_combo_jumps_changed);

		ComboBox combo = o as ComboBox;
		if (o == null)
			return;
		string myText = UtilGtk.ComboGetActive(combo);

		treeview_jumps_storeReset();
		fillTreeView_jumps(myText);
	}
	
	private void on_combo_jumps_rj_changed(object o, EventArgs args) {
		//combo_jumps_rj.Changed -= new EventHandler (on_combo_jumps_rj_changed);

		ComboBox combo = o as ComboBox;
		if (o == null)
			return;
		string myText = UtilGtk.ComboGetActive(combo);

		treeview_jumps_rj_storeReset();
		fillTreeView_jumps_rj(myText);
	}

	private void on_combo_runs_changed(object o, EventArgs args) {
		//combo_runs.Changed -= new EventHandler (on_combo_runs_changed);

		ComboBox combo = o as ComboBox;
		if (o == null)
			return;
		string myText = UtilGtk.ComboGetActive(combo);

		treeview_runs_storeReset();
		fillTreeView_runs(myText);
	}

	private void on_combo_runs_interval_changed(object o, EventArgs args) {
		//combo_runs_interval.Changed -= new EventHandler (on_combo_runs_interval_changed);

		ComboBox combo = o as ComboBox;
		if (o == null)
			return;
		string myText = UtilGtk.ComboGetActive(combo);

		treeview_runs_interval_storeReset();
		fillTreeView_runs_interval(myText);
	}

	//no need of reationTimes
	
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
		Log.WriteLine("Bye!");
    
		if(chronopicWin.Connected == true) {
			chronopicWin.SerialPortsClose();
		}
	
		try {	
			File.Delete(runningFileName);
		} catch {
			new DialogMessage(Constants.MessageTypes.WARNING, 
					string.Format(Catalog.GetString("Could not delete file:\n{0}"), runningFileName));
		}

		if(File.Exists(Util.GetDatabaseTempDir() + Path.DirectorySeparatorChar + "chronojump.db"))
			File.Move(Util.GetDatabaseTempDir() + Path.DirectorySeparatorChar + "chronojump.db",
				Util.GetDatabaseDir() + Path.DirectorySeparatorChar + "chronojump.db");
		
		Log.WriteLine("Bye2!");

		System.Console.Out.Close();
		//Log.End();
		//Log.Delete();
		Log.WriteLine("Bye3!");
		Application.Quit();
	}

	private void on_quit1_activate (object o, EventArgs args) {
		Log.WriteLine("Bye!");
    
		if(chronopicWin.Connected == true) {
			chronopicWin.SerialPortsClose();
		}
	
		try {	
			File.Delete(runningFileName);
		} catch {
			new DialogMessage(Constants.MessageTypes.WARNING, 
					string.Format(Catalog.GetString("Could not delete file:\n{0}"), runningFileName));
		}
		
		if(File.Exists(Util.GetDatabaseTempDir() + Path.DirectorySeparatorChar + "chronojump.db"))
			File.Move(Util.GetDatabaseTempDir() + Path.DirectorySeparatorChar + "chronojump.db",
				Util.GetDatabaseDir() + Path.DirectorySeparatorChar + "chronojump.db");
		
		Log.WriteLine("Bye2!");
		System.Console.Out.Close();
		//Log.End();
		//Log.Delete();
		Log.WriteLine("Bye3!");
		Application.Quit();
	}
	
	/* ---------------------------------------------------------
	 * ----------------  SESSION NEW, LOAD, EXPORT, DELETE -----
	 *  --------------------------------------------------------
	 */

	private void on_new_activate (object o, EventArgs args) {
		Log.WriteLine("new session");
		sessionAddEditWin = SessionAddEditWindow.Show(app1, new Session());
		sessionAddEditWin.Button_accept.Clicked += new EventHandler(on_new_session_accepted);
	}
	
	private void on_new_session_accepted (object o, EventArgs args) {
		if(sessionAddEditWin.CurrentSession != null) 
		{
			currentSession = sessionAddEditWin.CurrentSession;
			//serverUniqueID is undefined until session is updated
			currentSession.ServerUniqueID = Constants.ServerUndefinedID;

			app1.Title = progName + " - " + currentSession.Name;

			if(createdStatsWin) {
				statsWin.InitializeSession(currentSession);
			}
		
			resetAllTreeViews(true); //boolean means: "also persons"

			//show hidden widgets
			sensitiveGuiNoSession();
			sensitiveGuiYesSession();
			definedSession = true;

			//for sure, jumpsExists is false, because we create a new session

			button_edit_current_person.Sensitive = false;
			menuitem_edit_current_person.Sensitive = false;
			menuitem_delete_current_person_from_session.Sensitive = false;
			button_show_all_person_events.Sensitive = false;
			menuitem_show_all_person_events.Sensitive = false;
		
			//update report
			report.SessionID = currentSession.UniqueID;
			report.StatisticsRemove();
			try {
				reportWin.FillTreeView();
			} catch {} //reportWin is still not created, not need to Fill again

			//feedback (more in 1st session created)
			string feedbackLoadUsers = Catalog.GetString ("Session created, now add or load persons.");
			if(currentSession.UniqueID == 1)
				new DialogMessage(Constants.MessageTypes.INFO, feedbackLoadUsers);
			else
				appbar2.Push ( 1, feedbackLoadUsers);
		}
	}
	
	private void on_edit_session_activate (object o, EventArgs args) {
		Log.WriteLine("edit session");
		sessionAddEditWin = SessionAddEditWindow.Show(app1, currentSession);
		sessionAddEditWin.Button_accept.Clicked += new EventHandler(on_edit_session_accepted);
	}
	
	private void on_edit_session_accepted (object o, EventArgs args) {
		if(sessionAddEditWin.CurrentSession != null) 
		{
			currentSession = sessionAddEditWin.CurrentSession;
			app1.Title = progName + " - " + currentSession.Name;

			if(createdStatsWin) {
				statsWin.InitializeSession(currentSession);
			}
		}
	}
	
	private void on_open_activate (object o, EventArgs args) {
		Log.WriteLine("open session");
		sessionLoadWin = SessionLoadWindow.Show(app1);
		sessionLoadWin.Button_accept.Clicked += new EventHandler(on_load_session_accepted);
		//on_load_session_accepted(o, args);
	}
	
	private void on_load_session_accepted (object o, EventArgs args) {
		currentSession = sessionLoadWin.CurrentSession;
		//currentSession = SqliteSession.Select("1");
		app1.Title = progName + " - " + currentSession.Name;
		
		if(createdStatsWin) {
			statsWin.InitializeSession(currentSession);
		}
		
		resetAllTreeViews(true); //boolean means: "also persons"

		bool foundPersons = selectRowTreeView_persons(treeview_persons, treeview_persons_store, 0);
		
		//show hidden widgets
		sensitiveGuiNoSession();
		sensitiveGuiYesSession();
		definedSession = true;
		
		button_edit_current_person.Sensitive = false;
		menuitem_edit_current_person.Sensitive = false;
		menuitem_delete_current_person_from_session.Sensitive = false;
		button_show_all_person_events.Sensitive = false;
		menuitem_show_all_person_events.Sensitive = false;

		//if there are persons
		if(foundPersons) {
			//activate the gui for persons in main window
			sensitiveGuiYesPerson();
		}

		//update report
		report.SessionID = currentSession.UniqueID;
		report.StatisticsRemove();
		try {
			reportWin.FillTreeView();
		} catch {} //reportWin is still not created, not need to Fill again
	}
	
	
	private void on_delete_session_activate (object o, EventArgs args) {
		Log.WriteLine("delete session");
		ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString("Are you sure you want to delete the current session"), "", Catalog.GetString("and all the session tests?"));
		confirmWin.Button_accept.Clicked += new EventHandler(on_delete_session_accepted);
	}
	
	private void on_delete_session_accepted (object o, EventArgs args) 
	{
		appbar2.Push( 1, Catalog.GetString("Deleted session and all its tests") );
		SqliteSession.DeleteAllStuff(currentSession.UniqueID.ToString());
		
		sensitiveGuiNoSession();
		definedSession = false;
		app1.Title = progName + "";
	}

	
	private void on_export_session_activate(object o, EventArgs args) {
		if (o == (object) menuitem_export_csv) {
			new ExportSessionCSV(currentSession, app1, appbar2, prefsDigitsNumber);
		} else if (o == (object) menuitem_export_xml) {
			new ExportSessionXML(currentSession, app1, appbar2, prefsDigitsNumber);
		} else {
			Log.WriteLine("Error exporting");
		}
	}

	
	/* ---------------------------------------------------------
	 * ----------------  PERSON RECUPERATE, LOAD, EDIT, DELETE -
	 *  --------------------------------------------------------
	 */
	
	private void on_recuperate_person_activate (object o, EventArgs args) {
		Log.WriteLine("recuperate person");
		personRecuperateWin = PersonRecuperateWindow.Show(app1, currentSession, prefsDigitsNumber);
		personRecuperateWin.FakeButtonDone.Clicked += new EventHandler(on_recuperate_person_accepted);
	}

	private void on_recuperate_person_accepted (object o, EventArgs args) {
		currentPerson = personRecuperateWin.CurrentPerson;
		currentPersonSession = SqlitePersonSession.Select(currentPerson.UniqueID, currentSession.UniqueID);
		
		myTreeViewPersons.Add(currentPerson.UniqueID.ToString(), currentPerson.Name);

		int rowToSelect = findRowOfCurrentPerson(treeview_persons, treeview_persons_store, currentPerson);
		if(rowToSelect != -1) {
			selectRowTreeView_persons(treeview_persons,
					treeview_persons_store, 
					rowToSelect);
			sensitiveGuiYesPerson();
		}
	}
		
	private void on_recuperate_persons_from_session_activate (object o, EventArgs args) {
		Log.WriteLine("recuperate persons from other session");
		personsRecuperateFromOtherSessionWin = PersonsRecuperateFromOtherSessionWindow.Show(app1, currentSession);
		personsRecuperateFromOtherSessionWin.FakeButtonDone.Clicked += new EventHandler(on_recuperate_persons_from_session_accepted);
	}
	
	private void on_recuperate_persons_from_session_accepted (object o, EventArgs args) {
		currentPerson = personsRecuperateFromOtherSessionWin.CurrentPerson;
		currentPersonSession = SqlitePersonSession.Select(currentPerson.UniqueID, currentSession.UniqueID);
		treeview_persons_storeReset();
		fillTreeView_persons();
		int rowToSelect = findRowOfCurrentPerson(treeview_persons, treeview_persons_store, currentPerson);
		if(rowToSelect != -1) {
			selectRowTreeView_persons(treeview_persons,
					treeview_persons_store, 
					rowToSelect);
			sensitiveGuiYesPerson();
		}
	}
		
	private void on_person_add_single_activate (object o, EventArgs args) {
		personAddModifyWin = PersonAddModifyWindow.Show(app1, 
				currentSession, new Person(-1), 
				prefsDigitsNumber, false); //don't comes from recuperate window
		//-1 means we are adding a new person
		//if we were modifying it will be it's uniqueID
		
		personAddModifyWin.FakeButtonAccept.Clicked += new EventHandler(on_person_add_single_accepted);
	}
	
	private void on_person_add_single_accepted (object o, EventArgs args) {
		if (personAddModifyWin.CurrentPerson != null)
		{
			currentPerson = personAddModifyWin.CurrentPerson;
			currentPersonSession = SqlitePersonSession.Select(currentPerson.UniqueID, currentSession.UniqueID);
			myTreeViewPersons.Add(currentPerson.UniqueID.ToString(), currentPerson.Name);
			
			int rowToSelect = findRowOfCurrentPerson(treeview_persons, treeview_persons_store, currentPerson);
			if(rowToSelect != -1) {
				selectRowTreeView_persons(treeview_persons,
						treeview_persons_store, 
						rowToSelect);
				sensitiveGuiYesPerson();
				appbar2.Push( 1, Catalog.GetString("Successfully added") + " " + currentPerson.Name );
			}
		}
	}

	//show spinbutton window asking for how many people to create	
	private void on_person_add_multiple_activate (object o, EventArgs args) {
		genericWin = GenericWindow.Show(Catalog.GetString("Select number of persons to add"), Constants.GenericWindowShow.SPIN);
		genericWin.SetSpinRange(1.0, 40.0);
		genericWin.Button_accept.Clicked += new EventHandler(on_person_add_multiple_prepared);
	}
	
	private void on_person_add_multiple_prepared (object o, EventArgs args) {
		genericWin.Button_accept.Clicked -= new EventHandler(on_person_add_multiple_prepared);
		personAddMultipleWin = PersonAddMultipleWindow.Show(app1, currentSession, genericWin.SpinSelected);
		personAddMultipleWin.Button_accept.Clicked += new EventHandler(on_person_add_multiple_accepted);
	}
	
	private void on_person_add_multiple_accepted (object o, EventArgs args) {
		if (personAddMultipleWin.CurrentPerson != null)
		{
			currentPerson = personAddMultipleWin.CurrentPerson;
			currentPersonSession = SqlitePersonSession.Select(currentPerson.UniqueID, currentSession.UniqueID);
			treeview_persons_storeReset();
			fillTreeView_persons();
			int rowToSelect = findRowOfCurrentPerson(treeview_persons, treeview_persons_store, currentPerson);
			if(rowToSelect != -1) {
				selectRowTreeView_persons(treeview_persons,
						treeview_persons_store, 
						rowToSelect);
				sensitiveGuiYesPerson();
			
				string myString = string.Format(Catalog.GetString("Successfully added {0} persons"), personAddMultipleWin.PersonsCreatedCount);
		appbar2.Push( 1, Catalog.GetString(myString) );
			}
		}
	}
	
	private void on_edit_current_person_clicked (object o, EventArgs args) {
		Log.WriteLine("modify person");
		//personAddModifyWin = PersonAddModifyWindow.Show(app1, currentSession, currentPerson.UniqueID, prefsDigitsNumber);
		personAddModifyWin = PersonAddModifyWindow.Show(app1, currentSession, currentPerson, 
				prefsDigitsNumber, false); //don't comes from recuperate window
		personAddModifyWin.FakeButtonAccept.Clicked += new EventHandler(on_edit_current_person_accepted);
	}
	
	private void on_edit_current_person_accepted (object o, EventArgs args) {
		if (personAddModifyWin.CurrentPerson != null)
		{
			currentPerson = personAddModifyWin.CurrentPerson;
			currentPersonSession = SqlitePersonSession.Select(currentPerson.UniqueID, currentSession.UniqueID);
			treeview_persons_storeReset();
			fillTreeView_persons();
			
			int rowToSelect = findRowOfCurrentPerson(treeview_persons, treeview_persons_store, currentPerson);
			if(rowToSelect != -1) {
				selectRowTreeView_persons(treeview_persons,
						treeview_persons_store, 
						rowToSelect);
				sensitiveGuiYesPerson();
			}

			on_combo_jumps_changed(combo_jumps, args);
			on_combo_jumps_rj_changed(combo_jumps_rj, args);
			on_combo_runs_changed(combo_runs, args);
			on_combo_runs_interval_changed(combo_runs_interval, args);
			on_combo_pulses_changed(combo_pulses, args);

			if(createdStatsWin) {
				statsWin.FillTreeView_stats(false, true);
			}

//			personAddModifyWin.Destroy();
		}
	}

	
	private void on_show_all_person_events_activate (object o, EventArgs args) {
		personShowAllEventsWin = PersonShowAllEventsWindow.Show(app1, currentSession.UniqueID, currentPerson);
	}
	
	
	private void on_delete_current_person_from_session_activate (object o, EventArgs args) {
		Log.WriteLine("delete current person from this session");
		ConfirmWindow confirmWin = ConfirmWindow.Show(
				Catalog.GetString("Are you sure you want to delete the current person and all his/her tests (jumps, runs, pulses, ...) from this session?\n(His/her personal data and tests in other sessions will remain intact)"), "", 
				Catalog.GetString("Current Person: ") + currentPerson.Name);

		confirmWin.Button_accept.Clicked += new EventHandler(on_delete_current_person_from_session_accepted);
	}
	
	private void on_delete_current_person_from_session_accepted (object o, EventArgs args) 
	{
		appbar2.Push( 1, Catalog.GetString("Deleted person and all his/her tests on this session") );
		SqlitePersonSession.DeletePersonFromSessionAndTests(
				currentSession.UniqueID.ToString(), currentPerson.UniqueID.ToString());
		
		resetAllTreeViews(true); //boolean means: "also persons"
		bool foundPersons = selectRowTreeView_persons(treeview_persons, treeview_persons_store, 0);
			
		if(createdStatsWin) {
			statsWin.FillTreeView_stats(false, true);
		}
		
		//if there are no persons
		if(!foundPersons) {
			sensitiveGuiNoPerson ();
			if(createdStatsWin) {
				statsWin.Hide();
			}
		}
	}


	/* ---------------------------------------------------------
	 * ----------------  SOME CALLBACKS ------------------------
	 *  --------------------------------------------------------
	 */

	private void on_menuitem_view_stats_activate(object o, EventArgs args) {
		statsWin = StatsWindow.Show(app1, currentSession, 
				prefsDigitsNumber, weightPercentPreferred, heightPreferred, 
				report, reportWin);
		createdStatsWin = true;
		statsWin.InitializeSession(currentSession);
	}
	
	//edit
	private void on_cut1_activate (object o, EventArgs args) {
	}
	
	private void on_copy1_activate (object o, EventArgs args) {
	}
	
	private void on_paste1_activate (object o, EventArgs args) {
	}


	private void on_preferences_activate (object o, EventArgs args) {
		PreferencesWindow myWin = PreferencesWindow.Show(
				prefsDigitsNumber, showHeight, showPower, showInitialSpeed, showAngle, showQIndex, showDjIndex, 
				askDeletion, weightPercentPreferred, heightPreferred, metersSecondsPreferred,
				//System.Threading.Thread.CurrentThread.CurrentUICulture.ToString(),
				SqlitePreferences.Select("language"),
				allowFinishRjAfterTime);
		myWin.Button_accept.Clicked += new EventHandler(on_preferences_accepted);
	}

	private void on_preferences_accepted (object o, EventArgs args) {
		prefsDigitsNumber = Convert.ToInt32 ( SqlitePreferences.Select("digitsNumber") ); 

		//string myPort = SqlitePreferences.Select("chronopicPort");

		//chronopicPort cannot change while chronopic is running.
		//user change the port, and the clicks on radiobutton on platform menu

		//if(myPort != chronopicPort && cpRunning) {
		//	string message = "";
		//	bool success = chronopicInit (myPort, out message);
		//}

		//chronopicPort = myPort;
	
		
		if ( SqlitePreferences.Select("askDeletion") == "True" ) 
			askDeletion = true;
		 else 
			askDeletion = false;
		
	
		if ( SqlitePreferences.Select("weightStatsPercent") == "True" ) 
			weightPercentPreferred = true;
		 else 
			weightPercentPreferred = false;
		

		if ( SqlitePreferences.Select("showHeight") == "True" ) 
			showHeight = true;
		 else 
			showHeight = false;
		
		if ( SqlitePreferences.Select("showPower") == "True" ) 
			showPower = true;
		 else 
			showPower = false;
		

		if ( SqlitePreferences.Select("showInitialSpeed") == "True" ) 
			showInitialSpeed = true;
		 else 
			showInitialSpeed = false;
		
		if ( SqlitePreferences.Select("showAngle") == "True" ) 
			showAngle = true;
		 else 
			showAngle = false;
		

		//update showQIndex or showDjIndex
		if ( SqlitePreferences.Select("showQIndex") == "True" ) 
			showQIndex = true;
		 else 
			showQIndex = false;
		
			
		if ( SqlitePreferences.Select("showDjIndex") == "True" ) 
			showDjIndex = true;
		 else 
			showDjIndex = false;
		
			
		//update heightPreferred
		if ( SqlitePreferences.Select("heightPreferred") == "True" ) 
			heightPreferred = true;
		 else 
			heightPreferred = false;
		

		//update metersSecondsPreferred
		if ( SqlitePreferences.Select("metersSecondsPreferred") == "True" ) 
			metersSecondsPreferred = true;
		 else 
			metersSecondsPreferred = false;
		

		//update allowFinish...
		if ( SqlitePreferences.Select("allowFinishRjAfterTime") == "True" ) 
			allowFinishRjAfterTime = true;
		else 
			allowFinishRjAfterTime = false;
		
		//change language works on windows. On Linux let's change the locale
		//if(Util.IsWindows()) 
		//	languageChange();
	

		try {
			if(createdStatsWin) {
				statsWin.PrefsDigitsNumber = prefsDigitsNumber;
				statsWin.WeightStatsPercent = weightPercentPreferred;
				statsWin.HeightPreferred = heightPreferred;

				statsWin.FillTreeView_stats(false, true);
			}

			//pass to report
			report.PrefsDigitsNumber = prefsDigitsNumber;
			report.HeightPreferred = heightPreferred;
			report.WeightStatsPercent = weightPercentPreferred;
			
			
			createTreeView_jumps (treeview_jumps);
			createTreeView_jumps_rj (treeview_jumps_rj);
			createTreeView_runs (treeview_runs);
			createTreeView_runs_interval (treeview_runs_interval);
			createTreeView_pulses(treeview_pulses);
			createTreeView_reaction_times(treeview_reaction_times);
			createTreeView_multi_chronopic(treeview_multi_chronopic);
			
			on_combo_jumps_changed(combo_jumps, args);
			on_combo_jumps_rj_changed(combo_jumps_rj, args);
			on_combo_runs_changed(combo_runs, args);
			on_combo_runs_interval_changed(combo_runs_interval, args);
			on_combo_pulses_changed(combo_pulses, args);

			//currently no combo_reaction_times
			treeview_reaction_times_storeReset();
			fillTreeView_reaction_times();

			//currently no combo_multi_chronopic
			treeview_multi_chronopic_storeReset();
			fillTreeView_multi_chronopic();
		}
		catch 
		{
		}
	}
	
	private void on_cancel_clicked (object o, EventArgs args) 
	{
		Console.WriteLine("cancel clicked one");

		//this will cancel jumps or runs
		currentEventExecute.Cancel = true;

		//unhide event buttons for next event
		sensitiveGuiEventDone();

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
		//	statsWin.ShowUpdateStatsButton();
	}
	
	private void on_cancel_multi_clicked (object o, EventArgs args) 
	{
		Console.WriteLine("cancel multi clicked one");

		//this will cancel jumps or runs
		currentEventExecute.Cancel = true;

		//unhide event buttons for next event
		sensitiveGuiEventDone();

		if(chronopicWin.Connected)
			checkCancelMultiTotally(o, args);
	}


	//if user doesn't touch the platform after pressing "cancel", sometimes it gets waiting a Read_event
	//now the event cancels ok, and next will be ok, also	
	private void checkCancelTotally (object o, EventArgs args) 
	{
		if(currentEventExecute.TotallyCancelled) 
			Log.WriteLine("totallyCancelled");
		else {
			Log.Write("NOT-totallyCancelled ");
			errorWin = ErrorWindow.Show(Catalog.GetString("Please, touch the contact platform for full cancelling.\nThen press button\n"));
			errorWin.Button_accept.Clicked += new EventHandler(checkCancelTotally);
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
//			Log.Write("NOT-totallyCancelled ");
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
		
		//unhide event buttons for next event
		sensitiveGuiEventDone();

		if(chronopicWin.Connected)
			checkFinishTotally(o, args);
		
		//let update stats
		if(createdStatsWin)
			statsWin.ShowUpdateStatsButton();
	}
		
	//mark to only get inside on_multi_chronopic_finished one time
	static bool multiFinishingByClickFinish;
	private void on_finish_multi_clicked (object o, EventArgs args) 
	{
		/*
		if(multiFinishingByClickFinish)
			return;
		else
			multiFinishingByClickFinish =  true;
			*/

		currentEventExecute.Finish = true;
		
		//unhide event buttons for next event
		Console.WriteLine("RR0");
		sensitiveGuiEventDone();

		//runA is not called for this, because it ends different
		//and there's a message on gui/eventExecute.cs for runA	
		Console.WriteLine("RR1");
		if(currentMultiChronopicType.Name != Constants.RunAnalysisName && chronopicWin.Connected) {
			checkFinishMultiTotally(o, args);
		}
		Console.WriteLine("RR2");
		
		//let update stats
		//if(createdStatsWin)
		//	statsWin.ShowUpdateStatsButton();
	}
		
	//if user doesn't touch the platform after pressing "finish", sometimes it gets waiting a Read_event
	//now the event finishes ok, and next will be ok
	//
	//not for multiChronopic:
	
	private void checkFinishTotally (object o, EventArgs args) 
	{
		if(currentEventExecute.TotallyFinished) 
			Log.WriteLine("totallyFinished");
		else {
			Log.Write("NOT-totallyFinished ");
			errorWin = ErrorWindow.Show(Catalog.GetString("Please, touch the contact platform for full finishing.\nThen press this button:\n"));
			errorWin.Button_accept.Clicked += new EventHandler(checkFinishTotally);
		}
	}

	//runA is not called for this, because it ends different
	//and there's a message on gui/eventExecute.cs for runA	
	private void checkFinishMultiTotally (object o, EventArgs args) 
	{
		bool needFinish1 = false;
		bool needFinish2 = false;
		bool needFinish3 = false;
		bool needFinish4 = false;
			
		Console.WriteLine("cfmt 0");
		needFinish1 = !currentEventExecute.TotallyFinishedMulti1;
		if(currentEventExecute.Chronopics > 1) {
			Console.WriteLine("cfmt 1");
			needFinish2 = !currentEventExecute.TotallyFinishedMulti2;
			if(currentEventExecute.Chronopics > 2) {
				Console.WriteLine("cfmt 2");
				needFinish3 = !currentEventExecute.TotallyFinishedMulti3;
				if(currentEventExecute.Chronopics > 3) {
					Console.WriteLine("cfmt 3");
					needFinish4 = !currentEventExecute.TotallyFinishedMulti4;
				}
			}
		}
		Console.WriteLine("cfmt 4");

		if(needFinish1 || needFinish2 || needFinish3 || needFinish4) {
//			Log.Write("NOT-totallyFinishled ");
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
		
			Console.WriteLine("cfmt 5");
			//try here because maybe solves problems in runAnalysis when seem to update the eventExecuteWindow at the same time as tries to show this errorWindow
				errorWin = ErrorWindow.Show(string.Format(
							Catalog.GetString("Please, touch the contact platform on Chronopic/s [{0}] for full finishing.") + 
							"\n" + Catalog.GetString("Then press this button:\n"), cancelStr));
				Console.WriteLine("cfmt 6");
				errorWin.Button_accept.Clicked += new EventHandler(checkFinishMultiTotally);
				Console.WriteLine("cfmt 7");
			//}
		} else {
			Log.WriteLine("totallyFinished");
			/*
			//call write here, because if done in execute/MultiChronopic, will be called n times if n chronopics are working
			currentEventExecute.MultiChronopicWrite(false);
			currentMultiChronopic = (MultiChronopic) currentEventExecute.EventDone;
Console.WriteLine("W");
		

			//if this multichronopic has more chronopics than other in session, then reload treeview, else simply add
			if(currentMultiChronopic.CPs() != SqliteMultiChronopic.MaxCPs(currentSession.UniqueID)) {
				treeview_multi_chronopic_storeReset();
				fillTreeView_multi_chronopic();
			} else
				myTreeViewMultiChronopic.Add(currentPerson.Name, currentMultiChronopic);
Console.WriteLine("X");
			
			//since 0.7.4.1 when test is done, treeview select it. action event button have to be shown 
			showHideActionEventButtons(true, Constants.MultiChronopicName); //show
		
			//unhide buttons for delete last test
			sensitiveGuiYesEvent();
			*/
		}
	}
	
	private void on_show_report_activate (object o, EventArgs args) {
		Log.WriteLine("open report window");
		reportWin = ReportWindow.Show(app1, report);
	}

	private void on_enter_notify (object o, Gtk.EnterNotifyEventArgs args) {
		Log.WriteLine("enter notify");
	}


	private void on_button_enter (object o, EventArgs args) {
		//jump simple
		if(o == (object) button_free || o == (object) menuitem_jump_free) {
			currentEventType = new JumpType("Free");
		} else if(o == (object) button_sj) {
			currentEventType = new JumpType("SJ");
		} else 	if(o == (object) button_sj_l) {
			currentEventType = new JumpType("SJl");
		} else 	if(o == (object) button_cmj) {
			currentEventType = new JumpType("CMJ");
		} else 	if(o == (object) button_cmj_l) {
			currentEventType = new JumpType("CMJl");
		} else 	if(o == (object) button_abk) {
			currentEventType = new JumpType("ABK");
//no abk_l button currently
//		} else 	if(o == (object) button_abk_l) {
//			currentEventType = new JumpType("ABKl");
		} else 	if(o == (object) button_dj) {
			currentEventType = new JumpType("DJ");
		} else 	if(o == (object) button_rocket) {
			currentEventType = new JumpType("Rocket");
		} else 	if(o == (object) button_take_off) {
			currentEventType = new JumpType(Constants.TakeOffName);
		//jumpRJ
		} else 	if(o == (object) button_rj_j) {
			currentEventType = new JumpType("RJ(j)");
		} else 	if(o == (object) button_rj_t) {
			currentEventType = new JumpType("RJ(t)");
		} else 	if(o == (object) button_rj_unlimited) {
			currentEventType = new JumpType("RJ(unlimited)");
		} else 	if(o == (object) button_rj_hexagon) {
			currentEventType = new JumpType("RJ(hexagon)");
		//run
		} else 	if(o == (object) button_run_custom) {
			currentEventType = new RunType("Custom");
		} else 	if(o == (object) button_run_20m) {
			currentEventType = new RunType("20m");
		} else 	if(o == (object) button_run_20m) {
			currentEventType = new RunType("100m");
		} else 	if(o == (object) button_run_100m) {
			currentEventType = new RunType("100m");
		} else 	if(o == (object) button_run_200m) {
			currentEventType = new RunType("200m");
		} else 	if(o == (object) button_run_400m) {
			currentEventType = new RunType("400m");
		} else 	if(o == (object) button_run_gesell) {
			currentEventType = new RunType("Gesell-DBT");
		} else 	if(o == (object) button_run_20yard) {
			currentEventType = new RunType("Agility-20Yard");
		} else 	if(o == (object) button_run_505) {
			currentEventType = new RunType("Agility-505");
		} else 	if(o == (object) button_run_illinois) {
			currentEventType = new RunType("Agility-Illinois");
		} else 	if(o == (object) button_run_margaria) {
			currentEventType = new RunType("Margaria");
		} else 	if(o == (object) button_run_shuttle) {
			currentEventType = new RunType("Agility-Shuttle-Run");
		} else 	if(o == (object) button_run_zigzag) {
			currentEventType = new RunType("Agility-ZigZag");
		//run interval
		} else 	if(o == (object) button_run_interval_by_laps) {
			currentEventType = new RunType("byLaps");
		} else 	if(o == (object) button_run_interval_by_time) {
			currentEventType = new RunType("byTime");
		} else 	if(o == (object) button_run_interval_unlimited) {
			currentEventType = new RunType("unlimited");
		} else 	if(o == (object) button_run_interval_mtgug) {
			currentEventType = new RunType("MTGUG");
		//reactionTime
		} else 	if(o == (object) button_reaction_time) {
			currentEventType = new ReactionTimeType("reactionTime");
		//pulse
		} else 	if(o == (object) button_pulse_free) {
			currentEventType = new PulseType("Free");
		} else 	if(o == (object) button_pulse_custom) {
			currentEventType = new PulseType("Custom");
		//multiChronopic
		} else 	if(o == (object) button_multi_chronopic_start) {
			currentEventType = new MultiChronopicType(Constants.MultiChronopicName);
		} else 	if(o == (object) button_run_analysis) {
			currentEventType = new MultiChronopicType(Constants.RunAnalysisName);
		}

		changeTestImage(currentEventType.Type.ToString(), currentEventType.Name, currentEventType.ImageFileName);
	}


	//changes the image about the text on the bottom left of main screen	
	private void changeTestImage(string eventTypeString, string eventName, string fileNameString) {
		label_image_test.Text = "<b>" + eventName + "</b>"; 
		label_image_test.UseMarkup = true; 

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
			else Log.WriteLine("Error on eventTypeHasLongDescription");

			if(myType.HasLongDescription)
				return true;
		}
		return false;
	}

	/* ---------------------------------------------------------
	 * ----------------  JUMPS EXECUTION (no RJ) ----------------
	 *  --------------------------------------------------------
	 */

	private void on_button_more_clicked (object o, EventArgs args) 
	{
		jumpsMoreWin = JumpsMoreWindow.Show(app1, true);
		jumpsMoreWin.Button_accept.Clicked += new EventHandler(on_more_jumps_accepted);
		jumpsMoreWin.Button_selected.Clicked += new EventHandler(on_more_jumps_draw_image_test);
	}
	
	private void on_more_jumps_draw_image_test (object o, EventArgs args) {
		currentEventType = new JumpType(jumpsMoreWin.SelectedEventName);
		changeTestImage(currentEventType.Type.ToString(), currentEventType.Name, currentEventType.ImageFileName);
	}
	
	private void on_button_last_clicked (object o, EventArgs args) 
	{
		//currentJumpType contains the last jump type
		if( ! currentJumpType.StartIn || currentJumpType.HasWeight) {
			on_jump_extra_activate(o, args);
		} else {
			on_normal_jump_activate(o, args);
		}
	}
	
	//used from the dialogue "jumps more"
	private void on_more_jumps_accepted (object o, EventArgs args) 
	{
		jumpsMoreWin.Button_accept.Clicked -= new EventHandler(on_more_jumps_accepted);
		
		currentJumpType = new JumpType(
				//jumpsMoreWin.SelectedJumpType,
				jumpsMoreWin.SelectedEventName, //type of jump
								//SelectedEventType would be: jump, or run, ...
				jumpsMoreWin.SelectedStartIn,
				jumpsMoreWin.SelectedExtraWeight,
				false,		//isRepetitive
				false,		//jumpsLimited (false, because is not repetitive)
				0,		//limitValue
				false,		//unlimited
				jumpsMoreWin.SelectedDescription,
				SqliteEvent.GraphLinkSelectFileName("jump", jumpsMoreWin.SelectedEventName)
				);

		//destroy the win for not having updating problems if a new jump type is created
		//jumpsMoreWin = null; //don't work
		jumpsMoreWin.Destroy(); //works ;)
		
		if( ! currentJumpType.StartIn || currentJumpType.HasWeight) {
			on_jump_extra_activate(o, args);
		} else {
			on_normal_jump_activate(o, args);
		}
	}
	
	//here comes the SJl, DJ and every jump that has weight or fall or both. Also the reactive jumps (for defining is limited value or weight or fall)
	private void on_jump_extra_activate (object o, EventArgs args) 
	{
		Log.WriteLine("jump extra");
		if(o == (object) button_sj_l || o == (object) sj_l) {
			currentJumpType = new JumpType("SJl");
		} else if(o == (object) button_dj || o == (object) dj) {
			currentJumpType = new JumpType("DJ");
		} else if(o == (object) button_cmj_l || o == (object) cmj_l) {
			currentJumpType = new JumpType("CMJl");
// currently no abk_l button or menu
//		} else if(o == (object) button_abk_l || o == (object) abk_l) {
//			currentJumpType = new JumpType("ABKl");
		} else {
		}
		
		jumpExtraWin = JumpExtraWindow.Show(app1, currentJumpType);
		if( currentJumpType.IsRepetitive ) {
			jumpExtraWin.Button_accept.Clicked += new EventHandler(on_rj_accepted);
		} else {
			jumpExtraWin.Button_accept.Clicked += new EventHandler(on_normal_jump_activate);
		}
	}

	
	//suitable for all jumps not repetitive
	private void on_normal_jump_activate (object o, EventArgs args) 
	{
		if(o == (object) button_free || o == (object) menuitem_jump_free) {
			currentJumpType = new JumpType("Free");
		}else if(o == (object) button_sj || o == (object) sj) {
			currentJumpType = new JumpType("SJ");
		} else if (o == (object) button_cmj || o == (object) cmj) {
			currentJumpType = new JumpType("CMJ");
		} else if (o == (object) button_abk || o == (object) abk) {
			currentJumpType = new JumpType("ABK");
		} else if (o == (object) button_rocket || o == (object) menuitem_jump_rocket) {
			currentJumpType = new JumpType("Rocket");
		} else if (o == (object) button_take_off || o == (object) menuitem_take_off) {
			currentJumpType = new JumpType(Constants.TakeOffName);
		} else {
		}
	
		//jumpType is a DJ?... with or without arms?
		if(currentJumpType.Name == "DJ") {
			if(jumpExtraWin.Arms)
				currentJumpType = new JumpType("DJa");
			else
				currentJumpType = new JumpType("DJna");
		}

		changeTestImage(EventType.Types.JUMP.ToString(), currentJumpType.Name, currentJumpType.ImageFileName);
			
		double jumpWeight = 0;
		if(currentJumpType.HasWeight) {
			if(jumpExtraWin.Option == "%") 
				jumpWeight = jumpExtraWin.Weight;
			else 
				jumpWeight = Util.WeightFromKgToPercent(jumpExtraWin.Weight, currentPersonSession.Weight);
		}
		double myFall = 0;
		bool arms = false;
		if(currentJumpType.Name == Constants.TakeOffName || currentJumpType.Name == Constants.TakeOffWeightName)
			myFall = 0;
		else if( ! currentJumpType.StartIn) {
			myFall = jumpExtraWin.Fall;
		}
		
			
		//used by cancel and finish
		//currentEventType = new JumpType();
		currentEventType = currentJumpType;
			
		//hide jumping buttons
		sensitiveGuiEventDoing();

		//change to page 0 of notebook if were in other
		notebook_change(0);
		
		//show the event doing window
		double myLimit = 3; //3 phases for show the Dj
		if( currentJumpType.StartIn || 
				currentJumpType.Name == Constants.TakeOffName || 
				currentJumpType.Name == Constants.TakeOffWeightName)
			myLimit = 2; //2 for normal jump (or take off)
			
		//don't let update until test finishes
		if(createdStatsWin)
			statsWin.HideUpdateStatsButton();

		eventExecuteWin = EventExecuteWindow.Show(
			Catalog.GetString("Execute Jump"), //windowTitle
			Catalog.GetString("Phases"),  	  //name of the different moments
			currentPerson.UniqueID, currentPerson.Name, 
			currentSession.UniqueID, 
			"jump", //tableName
			currentJumpType.Name, 
			prefsDigitsNumber, myLimit, chronopicWin.Connected);

		eventExecuteWin.ButtonCancel.Clicked += new EventHandler(on_cancel_clicked);
		eventExecuteWin.ButtonFinish.Clicked += new EventHandler(on_finish_clicked);

		//when user clicks on update the eventExecute window 
		//(for showing with his new confgured values: max, min and guides
		eventExecuteWin.ButtonUpdate.Clicked -= new EventHandler(on_update_clicked); //if we don't do this, on_update_clicked it's called 'n' times when 'n' events are don
		eventExecuteWin.ButtonUpdate.Clicked += new EventHandler(on_update_clicked);

		currentEventExecute = new JumpExecute(eventExecuteWin, currentPerson.UniqueID, currentPerson.Name, 
				currentSession.UniqueID, currentJumpType.Name, myFall, jumpWeight,
				chronopicWin.CP, appbar2, app1, prefsDigitsNumber, volumeOn);

		if (!chronopicWin.Connected) 
			currentEventExecute.SimulateInitValues(rand);
		
		if( currentJumpType.StartIn ) 
			currentEventExecute.Manage();
		 else 
			currentEventExecute.ManageFall();

		currentEventExecute.FakeButtonFinished.Clicked += new EventHandler(on_jump_finished);
	}	

	
	private void on_jump_finished (object o, EventArgs args)
	{
		currentEventExecute.FakeButtonFinished.Clicked -= new EventHandler(on_jump_finished);
		
		if ( ! currentEventExecute.Cancel ) {
			currentJump = (Jump) currentEventExecute.EventDone;

			if(weightPercentPreferred)
				myTreeViewJumps.Add(currentPerson.Name, currentJump);
			else {
				Jump myJump = new Jump();
				myJump = currentJump;
				myJump.Weight = Util.WeightFromPercentToKg(currentJump.Weight, currentPersonSession.Weight);
				myTreeViewJumps.Add(currentPerson.Name, myJump);
			}
			
			//since 0.7.4.1 when test is done, treeview select it. action event button have to be shown 
			showHideActionEventButtons(true, "Jump"); //show
		
			if(createdStatsWin) {
				//statsWin.FillTreeView_stats(false, false);
				statsWin.ShowUpdateStatsButton();
			}
		
			//unhide buttons for delete last jump
			sensitiveGuiYesEvent();
		}
		
		//unhide buttons that allow jumping
		sensitiveGuiEventDone();
	}


	/* ---------------------------------------------------------
	 * ----------------  JUMPS RJ EXECUTION  ------------------
	 *  --------------------------------------------------------
	 */
	
	private void on_button_more_rj_clicked (object o, EventArgs args) 
	{
		jumpsRjMoreWin = JumpsRjMoreWindow.Show(app1, true);
		jumpsRjMoreWin.Button_accept.Clicked += new EventHandler(on_more_jumps_rj_accepted);
		jumpsRjMoreWin.Button_selected.Clicked += new EventHandler(on_more_jumps_rj_draw_image_test);
	}

	private void on_more_jumps_rj_draw_image_test (object o, EventArgs args) {
		currentEventType = new JumpType(jumpsRjMoreWin.SelectedEventName);
		changeTestImage(currentEventType.Type.ToString(), currentEventType.Name, currentEventType.ImageFileName);
	}
	
	private void on_button_last_rj_clicked (object o, EventArgs args) 
	{
		//currentJumpType contains the last jump type
		if( ! currentJumpType.StartIn || currentJumpType.HasWeight || currentJumpType.FixedValue == 0) {
			on_jump_extra_activate(o, args);
		} else {
			on_rj_accepted(o, args);
		}
	}
	
	
	//used from the dialogue "jumps rj more"
	private void on_more_jumps_rj_accepted (object o, EventArgs args) 
	{
		jumpsRjMoreWin.Button_accept.Clicked -= new EventHandler(on_more_jumps_rj_accepted);

		currentJumpType = new JumpType(
				//jumpsRjMoreWin.SelectedJumpType,
				jumpsRjMoreWin.SelectedEventName,
				jumpsRjMoreWin.SelectedStartIn,
				jumpsRjMoreWin.SelectedExtraWeight,
				true,		//isRepetitive
				jumpsRjMoreWin.SelectedLimited,
				jumpsRjMoreWin.SelectedLimitedValue,
				jumpsRjMoreWin.SelectedUnlimited,
				jumpsRjMoreWin.SelectedDescription,
				SqliteEvent.GraphLinkSelectFileName("jumpRj", jumpsRjMoreWin.SelectedEventName)
				);

		//destroy the win for not having updating problems if a new jump type is created
		jumpsRjMoreWin.Destroy();
		
		if( ! currentJumpType.StartIn || currentJumpType.HasWeight || 
				(currentJumpType.FixedValue == 0 && ! currentJumpType.Unlimited) ) {
			on_jump_extra_activate(o, args);
		} else {
			on_rj_accepted(o, args);
		}
	}
	
	private void on_rj_activate (object o, EventArgs args) {
		if(o == (object) button_rj_j || o == (object) rj_j) 
		{
			currentJumpType = new JumpType("RJ(j)");
			jumpExtraWin = JumpExtraWindow.Show(app1, currentJumpType);
			jumpExtraWin.Button_accept.Clicked += new EventHandler(on_rj_accepted);
		} else if(o == (object) button_rj_t || o == (object) rj_t) 
		{
			currentJumpType = new JumpType("RJ(t)");
			jumpExtraWin = JumpExtraWindow.Show(app1, currentJumpType);
			jumpExtraWin.Button_accept.Clicked += new EventHandler(on_rj_accepted);
		} else if(o == (object) button_rj_unlimited || o == (object) rj_unlimited) 
		{
			currentJumpType = new JumpType("RJ(unlimited)");

			//in this jump type, don't ask for limit of jumps or seconds
			on_rj_accepted(o, args);
		} else if(o == (object) button_rj_hexagon || o == (object) rj_hexagon) 
		{
			currentJumpType = new JumpType("RJ(hexagon)");

			//in this jump type, don't ask for limit of jumps or seconds
			on_rj_accepted(o, args);
		}
	}
	private void on_rj_accepted (object o, EventArgs args) 
	{
		changeTestImage(EventType.Types.JUMP.ToString(), currentJumpType.Name, currentJumpType.ImageFileName);

		double myLimit = 0;
		
		//if it's a unlimited interval run, put -1 as limit value
		if(currentJumpType.Unlimited) {
			myLimit = -1;
		} else {
			if(currentJumpType.FixedValue > 0) {
				myLimit = currentJumpType.FixedValue;
			} else {
				myLimit = jumpExtraWin.Limited;
			}
		}

		double jumpWeight = 0;
		if(currentJumpType.HasWeight) {
			//jumpWeight = jumpExtraWin.Weight + jumpExtraWin.Option;
			if(jumpExtraWin.Option == "%") {
				jumpWeight = jumpExtraWin.Weight;
			} else {
				jumpWeight = Util.WeightFromKgToPercent(jumpExtraWin.Weight, currentPersonSession.Weight);
			}
		}
		double myFall = 0;
		if( ! currentJumpType.StartIn || currentJumpType.Name	== Constants.RunAnalysisName)
			myFall = jumpExtraWin.Fall;
			
		//used by cancel and finish
		//currentEventType = new JumpType();
		currentEventType = currentJumpType;
			
		//hide jumping buttons
		sensitiveGuiEventDoing();
	
		//change to page 1 of notebook if were in other
		notebook_change(1);
		
		//don't let update until test finishes
		if(createdStatsWin)
			statsWin.HideUpdateStatsButton();

		//show the event doing window
		eventExecuteWin = EventExecuteWindow.Show(
			Catalog.GetString("Execute Reactive Jump"), //windowTitle
			Catalog.GetString("Jumps"),  	  //name of the different moments
			currentPerson.UniqueID, currentPerson.Name, 
			currentSession.UniqueID, 
			"jumpRj", //tableName
			currentJumpType.Name, 
			prefsDigitsNumber, myLimit, chronopicWin.Connected);
		
		eventExecuteWin.ButtonCancel.Clicked += new EventHandler(on_cancel_clicked);
		eventExecuteWin.ButtonFinish.Clicked += new EventHandler(on_finish_clicked);
		
		//when user clicks on update the eventExecute window 
		//(for showing with his new configured values: max, min and guides
		eventExecuteWin.ButtonUpdate.Clicked -= new EventHandler(on_update_clicked); //if we don't do this, on_update_clicked it's called 'n' times when 'n' events are don
		eventExecuteWin.ButtonUpdate.Clicked += new EventHandler(on_update_clicked);
	
		currentEventExecute = new JumpRjExecute(eventExecuteWin, currentPerson.UniqueID, currentPerson.Name, 
				currentSession.UniqueID, currentJumpType.Name, myFall, jumpWeight, 
				myLimit, currentJumpType.JumpsLimited, 
				chronopicWin.CP, appbar2, app1, prefsDigitsNumber, allowFinishRjAfterTime, volumeOn, repetitiveConditionsWin);
		
		
		//suitable for limited by jump and time
		//simulated always simulate limited by jumps
		if(!chronopicWin.Connected) 
			currentEventExecute.SimulateInitValues(rand);
		
		currentEventExecute.Manage();
		currentEventExecute.FakeButtonFinished.Clicked += new EventHandler(on_jump_rj_finished);
	}
		
	private void on_jump_rj_finished (object o, EventArgs args) 
	{
		Log.WriteLine("ON JUMP RJ FINISHED");
		
		currentEventExecute.FakeButtonFinished.Clicked -= new EventHandler(on_jump_rj_finished);
		
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

			if(weightPercentPreferred)
				myTreeViewJumpsRj.Add(currentPerson.Name, currentJumpRj);
			else {
				JumpRj myJump = new JumpRj();
				myJump = currentJumpRj;
				myJump.Weight = Util.WeightFromPercentToKg(currentJumpRj.Weight, currentPersonSession.Weight);
				myTreeViewJumpsRj.Add(currentPerson.Name, myJump);
			}
			
			//since 0.7.4.1 when test is done, treeview select it. action event button have to be shown 
			showHideActionEventButtons(true, "JumpRj"); //show

			//currentEventExecute.StopThread();

			if(createdStatsWin) {
				//statsWin.FillTreeView_stats(false, false);
				statsWin.ShowUpdateStatsButton();
			}

			//unhide buttons for delete last jump
			sensitiveGuiYesEvent();

			//put correct time value in eventWindow (put the time from chronopic and not onTimer soft chronometer)
			eventExecuteWin.LabelTimeValue = Util.GetTotalTime(currentJumpRj.TcString, currentJumpRj.TvString);
			//possible deletion of last jump can make the jumps on event window be false
			eventExecuteWin.LabelEventValue = currentJumpRj.Jumps;
		}
		
		//delete the temp tables if exists
		Sqlite.DeleteTempEvents("tempJumpRj");


		//unhide buttons that allow jumping
		sensitiveGuiEventDone();
	}

	/* ---------------------------------------------------------
	 * ----------------  RUNS EXECUTION (no interval) ----------
	 *  --------------------------------------------------------
	 */
	
	private void on_button_run_more_clicked (object o, EventArgs args) 
	{
		runsMoreWin = RunsMoreWindow.Show(app1, true);
		runsMoreWin.Button_accept.Clicked += new EventHandler(on_more_runs_accepted);
		runsMoreWin.Button_selected.Clicked += new EventHandler(on_more_runs_draw_image_test);
	}
	
	private void on_more_runs_draw_image_test (object o, EventArgs args) {
		currentEventType = new RunType(runsMoreWin.SelectedEventName);
		changeTestImage(currentEventType.Type.ToString(), currentEventType.Name, currentEventType.ImageFileName);
	}
	
	
	private void on_button_run_last_clicked (object o, EventArgs args) 
	{
		Log.WriteLine("button run last");
		//currentRunType contains the last run type
		if(currentRunType.Distance == 0) {
			on_run_extra_activate(o, args);
		} else {
			on_normal_run_activate(o, args);
		}
	}
	
	//used from the dialogue "runs more"
	private void on_more_runs_accepted (object o, EventArgs args) 
	{
		runsMoreWin.Button_accept.Clicked -= new EventHandler(on_more_runs_accepted);
	
		currentRunType = new RunType(
				runsMoreWin.SelectedEventName,	//name
				false,				//hasIntervals
				runsMoreWin.SelectedDistance,	//distance
				false,				//tracksLimited (false, because has not intervals)
				0,				//fixedValue (0, because has not intervals)
				false,				//unlimited (false, because has not intervals)
				runsMoreWin.SelectedDescription,
				"", // distancesstring (deactivated now, TODO: activate)
				SqliteEvent.GraphLinkSelectFileName("run", runsMoreWin.SelectedEventName)
				);
		
				
		//destroy the win for not having updating problems if a new run type is created
		runsMoreWin.Destroy();
		
		if( currentRunType.Distance == 0 ) {
			on_run_extra_activate(o, args);
		} else {
			on_normal_run_activate(o, args);
		}
	}
	
	//here comes the unlimited runs (and every run with distance = 0 (undefined)
	private void on_run_extra_activate (object o, EventArgs args) 
	{
		Log.WriteLine("run extra");
	
		if(o == (object) button_run_custom || o == (object) menuitem_run_custom) {
			currentRunType = new RunType("Custom");
		} else if (o == (object) button_run_margaria || o == (object) menuitem_run_margaria) {
			currentRunType = new RunType("Margaria");
		}
		// add others...
		
		runExtraWin = RunExtraWindow.Show(app1, currentRunType);
		if( currentRunType.HasIntervals ) {
			runExtraWin.Button_accept.Clicked += new EventHandler(on_run_interval_accepted);
		} else {
			runExtraWin.Button_accept.Clicked += new EventHandler(on_normal_run_activate);
		}
	}

	//suitable for all runs not repetitive
	private void on_normal_run_activate (object o, EventArgs args) 
	{
		if (o == (object) button_run_20m || o == (object) menuitem_20m) {
			currentRunType = new RunType("20m");
		} else if (o == (object) button_run_100m || o == (object) menuitem_100m) {
			currentRunType = new RunType("100m");
		} else if (o == (object) button_run_200m || o == (object) menuitem_200m) {
			currentRunType = new RunType("200m");
		} else if (o == (object) button_run_400m || o == (object) menuitem_400m) {
			currentRunType = new RunType("400m");
		} else if (o == (object) button_run_gesell || o == (object) menuitem_run_gesell) {
			currentRunType = new RunType("Gesell-DBT");
		} else if (o == (object) button_run_20yard || o == (object) menuitem_run_20yard) {
			currentRunType = new RunType("Agility-20Yard");
		} else if (o == (object) button_run_505 || o == (object) menuitem_run_505) {
			currentRunType = new RunType("Agility-505");
		} else if (o == (object) button_run_illinois || o == (object) menuitem_run_illinois) {
			currentRunType = new RunType("Agility-Illinois");
		} else if (o == (object) button_run_shuttle || o == (object) menuitem_run_shuttle) {
			currentRunType = new RunType("Agility-Shuttle-Run");
		} else if (o == (object) button_run_zigzag || o == (object) menuitem_run_zigzag) {
			currentRunType = new RunType("Agility-ZigZag");
		} 
		// add others...
		
		changeTestImage(EventType.Types.RUN.ToString(), currentRunType.Name, currentRunType.ImageFileName);

		//if distance can be always different in this run,
		//show values selected in runExtraWin
		int myDistance = 0;		
		if(currentRunType.Distance == 0) {
			myDistance = runExtraWin.Distance;
		} else {
			myDistance = (int) currentRunType.Distance;
		}
		
		//used by cancel and finish
		//currentEventType = new RunType();
		currentEventType = currentRunType;
			
		//hide jumping (running) buttons
		sensitiveGuiEventDoing();
	
		//change to page 2 of notebook if were in other
		notebook_change(2);
			
		//show the event doing window
		
		/*
		double myLimit = 3; //3 phases for show the Dj
		if( currentJumpType.StartIn )
			myLimit = 2; //2 for normal jump
		*/
		
		double myLimit = 3; //same for startingIn than out (before)
		
		//don't let update until test finishes
		if(createdStatsWin)
			statsWin.HideUpdateStatsButton();

		eventExecuteWin = EventExecuteWindow.Show(
			Catalog.GetString("Execute Run"), //windowTitle
			Catalog.GetString("Phases"),  	  //name of the different moments
			currentPerson.UniqueID, currentPerson.Name, 
			currentSession.UniqueID, 
			"run", //tableName
			currentRunType.Name, 
			prefsDigitsNumber, myLimit, chronopicWin.Connected);
		
		eventExecuteWin.ButtonCancel.Clicked += new EventHandler(on_cancel_clicked);

		eventExecuteWin.ButtonFinish.Clicked += new EventHandler(on_finish_clicked);


		//when user clicks on update the eventExecute window 
		//(for showing with his new confgured values: max, min and guides
		eventExecuteWin.ButtonUpdate.Clicked -= new EventHandler(on_update_clicked); //if we don't do this, on_update_clicked it's called 'n' times when 'n' events are don
		eventExecuteWin.ButtonUpdate.Clicked += new EventHandler(on_update_clicked);


		currentEventExecute = new RunExecute(eventExecuteWin, currentPerson.UniqueID, currentSession.UniqueID, 
				currentRunType.Name, myDistance, 
				chronopicWin.CP, appbar2, app1, prefsDigitsNumber, metersSecondsPreferred, volumeOn);
		
		if (!chronopicWin.Connected) 
			currentEventExecute.SimulateInitValues(rand);
			
		currentEventExecute.Manage();
		currentEventExecute.FakeButtonFinished.Clicked += new EventHandler(on_run_finished);
	}
	
	private void on_run_finished (object o, EventArgs args)
	{
		currentEventExecute.FakeButtonFinished.Clicked -= new EventHandler(on_run_finished);
		
		if ( ! currentEventExecute.Cancel ) {
			currentRun = (Run) currentEventExecute.EventDone;
			currentRun.MetersSecondsPreferred = metersSecondsPreferred;

			myTreeViewRuns.Add(currentPerson.Name, currentRun);
			
			//since 0.7.4.1 when test is done, treeview select it. action event button have to be shown 
			showHideActionEventButtons(true, "Run"); //show
		
			if(createdStatsWin) {
				statsWin.ShowUpdateStatsButton();
			}
		
			//unhide buttons for delete last jump
			sensitiveGuiYesEvent();

			//put correct time value in eventWindow (put the time from chronopic and not onTimer soft chronometer)
			eventExecuteWin.LabelTimeValue = currentRun.Time;
		}
		
		//unhide buttons that allow jumping, running
		sensitiveGuiEventDone();
	}

	/* ---------------------------------------------------------
	 * ----------------  RUNS EXECUTION (interval) ----------
	 *  --------------------------------------------------------
	 */

	private void on_button_run_interval_more_clicked (object o, EventArgs args) 
	{
		runsIntervalMoreWin = RunsIntervalMoreWindow.Show(app1, true);
		runsIntervalMoreWin.Button_accept.Clicked += new EventHandler(on_more_runs_interval_accepted);
		runsIntervalMoreWin.Button_selected.Clicked += new EventHandler(on_more_runs_interval_draw_image_test);
	}
	
	private void on_more_runs_interval_draw_image_test (object o, EventArgs args) {
		currentEventType = new RunType(runsIntervalMoreWin.SelectedEventName);
		changeTestImage(currentEventType.Type.ToString(), currentEventType.Name, currentEventType.ImageFileName);
	}
	
	private void on_more_runs_interval_accepted (object o, EventArgs args) 
	{
		runsIntervalMoreWin.Button_accept.Clicked -= new EventHandler(on_more_runs_interval_accepted);
		
		Console.WriteLine(runsIntervalMoreWin.SelectedEventName);
		Console.WriteLine(runsIntervalMoreWin.SelectedDistance.ToString());
		Console.WriteLine(runsIntervalMoreWin.SelectedDistancesString);
		currentRunType = new RunType(
				runsIntervalMoreWin.SelectedEventName,	//name
				true,					//hasIntervals
				runsIntervalMoreWin.SelectedDistance,
				runsIntervalMoreWin.SelectedTracksLimited,
				runsIntervalMoreWin.SelectedLimitedValue,
				runsIntervalMoreWin.SelectedUnlimited,
				runsIntervalMoreWin.SelectedDescription,
				runsIntervalMoreWin.SelectedDistancesString,
				SqliteEvent.GraphLinkSelectFileName(Constants.RunIntervalTable, runsIntervalMoreWin.SelectedEventName)
				);

		bool unlimited = false;
		if(runsIntervalMoreWin.SelectedUnlimited)
			unlimited = true;

		//destroy the win for not having updating problems if a new runInterval type is created
		runsIntervalMoreWin.Destroy();
		
		//go to run extra if we need something to define
		if( currentRunType.Distance == 0 || 
				//(currentRunType.FixedValue == 0 && ! runsIntervalMoreWin.SelectedUnlimited) ) {
				(currentRunType.FixedValue == 0 && ! unlimited) ) {
			on_run_extra_activate(o, args);
		} else {
			on_run_interval_accepted(o, args);
		}
	}
	
	private void on_button_run_interval_last_clicked (object o, EventArgs args) 
	{
		//go to run extra if we need something to define
		if( currentRunType.Distance == 0 || 
				(currentRunType.FixedValue == 0 && ! currentRunType.Unlimited) ) {
			on_run_extra_activate(o, args);
		} else {
			on_run_interval_accepted(o, args);
		}
	}
	
	//interval runs clicked from user interface
	//(not suitable for the other runs we found in "more")
	private void on_run_interval_activate (object o, EventArgs args) 
	{
		if(o == (object) button_run_interval_by_laps || o == (object) menuitem_run_interval_by_laps) 
		{	
			currentRunType = new RunType("byLaps");
		} else if(o == (object) button_run_interval_by_time || o == (object) menuitem_run_interval_by_time) 
		{
			currentRunType = new RunType("byTime");
		} else if(o == (object) button_run_interval_unlimited || o == (object) menuitem_run_interval_unlimited) 
		{
			currentRunType = new RunType("unlimited");
		} else if(o == (object) button_run_interval_mtgug || o == (object) menuitem_run_interval_mtgug) 
		{
			currentRunType = new RunType("MTGUG");
		}
		
		if( currentRunType.Distance == 0 || 
				(currentRunType.FixedValue == 0 && ! currentRunType.Unlimited) ) {
			runExtraWin = RunExtraWindow.Show(app1, currentRunType);
			runExtraWin.Button_accept.Clicked += new EventHandler(on_run_interval_accepted);
		} else {
			on_run_interval_accepted(o, args);
		}
			
	}
	
	private void on_run_interval_accepted (object o, EventArgs args)
	{
		Log.WriteLine("run interval accepted");
		
		//if distance can be always different in this run,
		//show values selected in runExtraWin
		int distanceInterval = 0;		
		if(currentRunType.Distance == 0) {
			distanceInterval = runExtraWin.Distance;
		} else {
			distanceInterval = (int) currentRunType.Distance;
		}
		
		double myLimit = 0;
		//if it's a unlimited interval run, put -1 as limit value
		//if(o == (object) button_rj_unlimited || o == (object) rj_unlimited) {
		if(currentRunType.Unlimited) {
			myLimit = -1;
		} else {
			if(currentRunType.FixedValue > 0) {
				myLimit = currentRunType.FixedValue;
			} else {
				myLimit = runExtraWin.Limited;
			}
		}


		//used by cancel and finish
		//currentEventType = new RunType();
		currentEventType = currentRunType;
			
		//hide running buttons
		sensitiveGuiEventDoing();
		
		//change to page 3 of notebook if were in other
		notebook_change(3);
		
		//don't let update until test finishes
		if(createdStatsWin)
			statsWin.HideUpdateStatsButton();

		//show the event doing window
		eventExecuteWin = EventExecuteWindow.Show(
			Catalog.GetString("Execute Intervallic Run"), //windowTitle
			Catalog.GetString("Tracks"),  	  //name of the different moments
			currentPerson.UniqueID, currentPerson.Name, 
			currentSession.UniqueID, 
			Constants.RunIntervalTable, //tableName
			currentRunType.Name, 
			prefsDigitsNumber, myLimit, chronopicWin.Connected);

		eventExecuteWin.ButtonCancel.Clicked += new EventHandler(on_cancel_clicked);
		eventExecuteWin.ButtonFinish.Clicked += new EventHandler(on_finish_clicked);

		//when user clicks on update the eventExecute window 
		//(for showing with his new confgured values: max, min and guides
		eventExecuteWin.ButtonUpdate.Clicked -= new EventHandler(on_update_clicked); //if we don't do this, on_update_clicked it's called 'n' times when 'n' events are don
		eventExecuteWin.ButtonUpdate.Clicked += new EventHandler(on_update_clicked);
	
		currentEventExecute = new RunIntervalExecute(eventExecuteWin, currentPerson.UniqueID, currentSession.UniqueID, currentRunType.Name, 
				distanceInterval, myLimit, currentRunType.TracksLimited, 
				chronopicWin.CP, appbar2, app1, prefsDigitsNumber, metersSecondsPreferred, volumeOn, repetitiveConditionsWin);
		
		
		//suitable for limited by tracks and time
		if(!chronopicWin.Connected)
			currentEventExecute.SimulateInitValues(rand);
			
		currentEventExecute.Manage();
		currentEventExecute.FakeButtonFinished.Clicked += new EventHandler(on_run_interval_finished);
	}


	private void on_run_interval_finished (object o, EventArgs args) 
	{
		currentEventExecute.FakeButtonFinished.Clicked -= new EventHandler(on_run_interval_finished);
		
		if ( ! currentEventExecute.Cancel ) {
			currentRunInterval = (RunInterval) currentEventExecute.EventDone;
			currentRunInterval.MetersSecondsPreferred = metersSecondsPreferred;

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
				//statsWin.FillTreeView_stats(false, false);
				statsWin.ShowUpdateStatsButton();
			}

			//unhide buttons for delete last jump
			sensitiveGuiYesEvent();

			//put correct time value in eventWindow (put the time from chronopic and not onTimer soft chronometer)
			eventExecuteWin.LabelTimeValue = currentRunInterval.TimeTotal;
			//possible deletion of last run can make the runs on event window be false
			eventExecuteWin.LabelEventValue = currentRunInterval.Tracks;
		}
		
		//delete the temp tables if exists
		Sqlite.DeleteTempEvents("tempRunInterval");

		
		//unhide buttons that allow jumping, running
		sensitiveGuiEventDone();
	}

	/* ---------------------------------------------------------
	 * ----------------  REACTION TIMES EXECUTION --------------
	 *  --------------------------------------------------------
	 */

	
	//suitable for reaction times
	private void on_reaction_time_activate (object o, EventArgs args) 
	{
/*
		if(o == (object) button_free || o == (object) menuitem_jump_free) {
			currentJumpType = new JumpType("Free");
		}else if(o == (object) button_sj || o == (object) sj) {
			currentJumpType = new JumpType("SJ");
		} else if (o == (object) button_cmj || o == (object) cmj) {
			currentJumpType = new JumpType("CMJ");
		} else if (o == (object) button_abk || o == (object) abk) {
			currentJumpType = new JumpType("ABK");
		} else {
		}
			
*/			
		//used by cancel and finish
		currentEventType = new ReactionTimeType();
		//currentEventType = currentReactionTimeType;
			
		//hide jumping buttons
		sensitiveGuiEventDoing();

		//change to page 4 of notebook if were in other
		notebook_change(4);
		
		//show the event doing window
		double myLimit = 2;
			
		//don't let update until test finishes
		if(createdStatsWin)
			statsWin.HideUpdateStatsButton();

		eventExecuteWin = EventExecuteWindow.Show(
			Catalog.GetString("Execute Reaction Time"), //windowTitle
			Catalog.GetString("Phases"),  	  //name of the different moments
			currentPerson.UniqueID, currentPerson.Name, 
			currentSession.UniqueID, 
			"reactionTime", //tableName
			//currentJumpType.Name, 
			"", 
			prefsDigitsNumber, myLimit, chronopicWin.Connected);

		eventExecuteWin.ButtonCancel.Clicked += new EventHandler(on_cancel_clicked);
		eventExecuteWin.ButtonFinish.Clicked += new EventHandler(on_finish_clicked);

		//when user clicks on update the eventExecute window 
		//(for showing with his new confgured values: max, min and guides
		eventExecuteWin.ButtonUpdate.Clicked -= new EventHandler(on_update_clicked); //if we don't do this, on_update_clicked it's called 'n' times when 'n' events are don
		eventExecuteWin.ButtonUpdate.Clicked += new EventHandler(on_update_clicked);

		currentEventExecute = new ReactionTimeExecute(eventExecuteWin, currentPerson.UniqueID, currentPerson.Name, 
				currentSession.UniqueID, 
				//currentJumpType.Name, 
				chronopicWin.CP, appbar2, app1, prefsDigitsNumber, volumeOn);

		if (!chronopicWin.Connected) 
			currentEventExecute.SimulateInitValues(rand);
		
		currentEventExecute.Manage();

		currentEventExecute.FakeButtonFinished.Clicked += new EventHandler(on_reaction_time_finished);
	}	



	private void on_reaction_time_finished (object o, EventArgs args)
	{
		currentEventExecute.FakeButtonFinished.Clicked -= new EventHandler(on_reaction_time_finished);
		
		if ( ! currentEventExecute.Cancel ) {

			currentReactionTime = (ReactionTime) currentEventExecute.EventDone;
			
			myTreeViewReactionTimes.Add(currentPerson.Name, currentReactionTime);
			
			//since 0.7.4.1 when test is done, treeview select it. action event button have to be shown 
			showHideActionEventButtons(true, "ReactionTime"); //show
		
			if(createdStatsWin) {
				//statsWin.FillTreeView_stats(false, false);
				statsWin.ShowUpdateStatsButton();
			}
		
			//unhide buttons for delete last reaction time
			sensitiveGuiYesEvent();
		}
		
		//unhide buttons that allow jumping
		sensitiveGuiEventDone();
	}



	/* ---------------------------------------------------------
	 * ----------------  PULSES EXECUTION ----------------------
	 *  --------------------------------------------------------
	 */

	/*
	private void on_button_pulse_more_clicked (object o, EventArgs args) 
	{
	}
	*/
	
	private void on_more_pulse_accepted (object o, EventArgs args) 
	{
		/*
		runsIntervalMoreWin.Button_accept.Clicked -= new EventHandler(on_more_runs_interval_accepted);
		
		currentRunType = new RunType(
				runsIntervalMoreWin.SelectedRunType,	//name
				true,					//hasIntervals
				runsIntervalMoreWin.SelectedDistance,
				runsIntervalMoreWin.SelectedTracksLimited,
				runsIntervalMoreWin.SelectedLimitedValue,
				runsIntervalMoreWin.SelectedUnlimited
				);
				
		//go to run extra if we need something to define
		if( currentRunType.Distance == 0 || 
				(currentRunType.FixedValue == 0 && ! runsIntervalMoreWin.SelectedUnlimited) ) {
			on_run_extra_activate(o, args);
		} else {
			on_run_interval_accepted(o, args);
		}
		*/
	}

	/*	
	private void on_button_pulse_last_clicked (object o, EventArgs args) 
	{
	}
	*/
	
	private void on_button_pulse_free_activate (object o, EventArgs args) 
	{
		currentPulseType = new PulseType("Free");
		on_pulse_accepted(o, args);
	}
	
	//interval runs clicked from user interface
	//(not suitable for the other runs we found in "more")
	private void on_button_pulse_custom_activate (object o, EventArgs args) 
	{
		currentPulseType = new PulseType("Custom");
			
		pulseExtraWin = PulseExtraWindow.Show(app1, currentPulseType);
		pulseExtraWin.Button_accept.Clicked += new EventHandler(on_pulse_accepted);
	}
	
	private void on_pulse_accepted (object o, EventArgs args)
	{
		Log.WriteLine("pulse accepted");
	
		double pulseStep = 0;
		int totalPulses = 0;

		if(currentPulseType.Name == "Free") {
			pulseStep = currentPulseType.FixedPulse; // -1
			totalPulses = currentPulseType.TotalPulsesNum; //-1
		} else { //custom (info comes from Extra Window
			pulseStep = pulseExtraWin.PulseStep;
			totalPulses = pulseExtraWin.TotalPulses; //-1: unlimited; or 'n': limited by 'n' pulses
		}

		//used by cancel and finish
		//currentEventType = new PulseType();
		currentEventType = currentPulseType;
			
		//hide pulse buttons
		sensitiveGuiEventDoing();
		
		//change to page 5 of notebook if were in other
		notebook_change(5);
		
		//don't let update until test finishes
		if(createdStatsWin)
			statsWin.HideUpdateStatsButton();

		//show the event doing window
		eventExecuteWin = EventExecuteWindow.Show(
			Catalog.GetString("Execute Pulse"), //windowTitle
			Catalog.GetString("Pulses"),  	  //name of the different moments
			currentPerson.UniqueID, currentPerson.Name, 
			currentSession.UniqueID, 
			"pulse", //tableName
			currentPulseType.Name, 
			prefsDigitsNumber, totalPulses, chronopicWin.Connected);

		eventExecuteWin.ButtonCancel.Clicked += new EventHandler(on_cancel_clicked);
		eventExecuteWin.ButtonFinish.Clicked += new EventHandler(on_finish_clicked);
		
		//when user clicks on update the eventExecute window 
		//(for showing with his new confgured values: max, min and guides
		eventExecuteWin.ButtonUpdate.Clicked -= new EventHandler(on_update_clicked); //if we don't do this, on_update_clicked it's called 'n' times when 'n' events are don
		eventExecuteWin.ButtonUpdate.Clicked += new EventHandler(on_update_clicked);

		currentEventExecute = new PulseExecute(eventExecuteWin, currentPerson.UniqueID, currentPerson.Name, 
				currentSession.UniqueID, currentPulseType.Name, pulseStep, totalPulses, 
				chronopicWin.CP, appbar2, app1, prefsDigitsNumber, volumeOn);
		
		if(!chronopicWin.Connected)	
			currentEventExecute.SimulateInitValues(rand);
		
		currentEventExecute.Manage();
		currentEventExecute.FakeButtonFinished.Clicked += new EventHandler(on_pulse_finished);
	}

	private void on_pulse_finished (object o, EventArgs args) 
	{
		Log.WriteLine("pulse finished");
		
		currentEventExecute.FakeButtonFinished.Clicked -= new EventHandler(on_pulse_finished);
		
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
			showHideActionEventButtons(true, "Pulse"); //show
			
			if(createdStatsWin) {
				//statsWin.FillTreeView_stats(false, false);
				statsWin.ShowUpdateStatsButton();
			}
			
			//unhide buttons for delete last jump
			sensitiveGuiYesEvent();

			//put correct time value in eventWindow (put the time from chronopic and not onTimer soft chronometer)
			eventExecuteWin.LabelTimeValue = Util.GetTotalTime(currentPulse.TimesString);
		}
		
		//unhide buttons that allow jumping, running
		sensitiveGuiEventDone();
	}

	/* ---------------------------------------------------------
	 * ----------------  MULTI CHRONOPIC EXECUTION -------------
	 *  --------------------------------------------------------
	 */

	private void on_menuitem_chronopic_activate (object o, EventArgs args) {
		chronopicWin = ChronopicWindow.View();
		chronopicWin.FakeWindowDone.Clicked += new EventHandler(on_chronopic_window_done);
	}
	
	private void on_chronopic_window_done (object o, EventArgs args) {
		chronopicWin.FakeWindowDone.Clicked -= new EventHandler(on_chronopic_window_done);
		if(chronopicWin.NumConnected()>=2) {
			menuitem_multi_chronopic_start.Sensitive = true;
			menuitem_run_analysis.Sensitive = true;
			button_multi_chronopic_start.Sensitive = true;
			entry_run_analysis_distance.Sensitive = true;
			on_entry_run_analysis_distance_changed (o, args);
		} else {
			menuitem_multi_chronopic_start.Sensitive = false;
			menuitem_run_analysis.Sensitive = false;
			button_multi_chronopic_start.Sensitive = false;
			entry_run_analysis_distance.Sensitive = false;
			button_run_analysis.Sensitive = false;
		}
	}

	private void on_entry_run_analysis_distance_changed (object o, EventArgs args) {
		if(Util.IsNumber(entry_run_analysis_distance.Text, false) && entry_run_analysis_distance.Text != "0" &&
				chronopicWin.NumConnected()>=2) {
			menuitem_run_analysis.Sensitive = true;
			button_run_analysis.Sensitive = true;
		} else {
			menuitem_run_analysis.Sensitive = false;
			button_run_analysis.Sensitive = false;
		}
	}

	private void on_multi_chronopic_start_clicked (object o, EventArgs args) {
		Log.WriteLine("multi chronopic accepted");
		
		if(o == (object) button_multi_chronopic_start || o == (object) menuitem_multi_chronopic_start) 
			currentMultiChronopicType = new MultiChronopicType(Constants.MultiChronopicName);
		else if(o == (object) button_run_analysis || o == (object) menuitem_run_analysis)
			currentMultiChronopicType = new MultiChronopicType(Constants.RunAnalysisName);

		//used by cancel and finish
		currentEventType = new MultiChronopicType();
			
		//hide pulse buttons
		sensitiveGuiEventDoing();
		
		//change to page 6 of notebook if were in other
		notebook_change(6);
		
		//don't let update until test finishes
		if(createdStatsWin)
			statsWin.HideUpdateStatsButton();

		//show the event doing window
		eventExecuteWin = EventExecuteWindow.Show(
			Catalog.GetString("Execute Multi Chronopic"), //windowTitle
			Catalog.GetString("Changes"),  	  //name of the different moments
			currentPerson.UniqueID, currentPerson.Name, 
			currentSession.UniqueID, 
			Constants.MultiChronopicTable, //tableName
			currentMultiChronopicType.Name, 
			prefsDigitsNumber, -1, chronopicWin.Connected
			); //-1: unlimited pulses (or changes)

		eventExecuteWin.ButtonCancel.Clicked += new EventHandler(on_cancel_multi_clicked);
		multiFinishingByClickFinish = false;
		eventExecuteWin.ButtonFinish.Clicked += new EventHandler(on_finish_multi_clicked);
		
		//when user clicks on update the eventExecute window 
		//(for showing with his new confgured values: max, min and guides
		eventExecuteWin.ButtonUpdate.Clicked -= new EventHandler(on_update_clicked); //if we don't do this, on_update_clicked it's called 'n' times when 'n' events are don
		eventExecuteWin.ButtonUpdate.Clicked += new EventHandler(on_update_clicked);


		bool syncNeeded = false;
		if(currentMultiChronopicType.SyncNeeded && check_multi_sync.Active)
			syncNeeded = true;

		int numConnected = chronopicWin.NumConnected();

		if(numConnected == 1)
			currentEventExecute = new MultiChronopicExecute(
					eventExecuteWin, currentPerson.UniqueID, currentPerson.Name, 
					currentSession.UniqueID, currentMultiChronopicType.Name, 
					chronopicWin.CP, 
					syncNeeded, check_multi_delete_first.Active, 
					entry_run_analysis_distance.Text.ToString(),
					appbar2, app1);
		else if(numConnected == 2)
			currentEventExecute = new MultiChronopicExecute(
					eventExecuteWin, currentPerson.UniqueID, currentPerson.Name, 
					currentSession.UniqueID, currentMultiChronopicType.Name,  
					chronopicWin.CP, chronopicWin.CP2, 
					syncNeeded, check_multi_delete_first.Active, 
					entry_run_analysis_distance.Text.ToString(),
					appbar2, app1);
		else if(numConnected == 3)
			currentEventExecute = new MultiChronopicExecute(
					eventExecuteWin, currentPerson.UniqueID, currentPerson.Name, 
					currentSession.UniqueID, currentMultiChronopicType.Name,
					chronopicWin.CP, chronopicWin.CP2, chronopicWin.CP3, 
					syncNeeded, check_multi_delete_first.Active, 
					entry_run_analysis_distance.Text.ToString(),
					appbar2, app1);
		else if(numConnected == 4)
			currentEventExecute = new MultiChronopicExecute(
					eventExecuteWin, currentPerson.UniqueID, currentPerson.Name, 
					currentSession.UniqueID, currentMultiChronopicType.Name,
					chronopicWin.CP, chronopicWin.CP2, chronopicWin.CP3, chronopicWin.CP4,
					syncNeeded, check_multi_delete_first.Active, 
					entry_run_analysis_distance.Text.ToString(),
					appbar2, app1);

		//if(!chronopicWin.Connected)	
		//	currentEventExecute.SimulateInitValues(rand);


		//mark to only get inside on_multi_chronopic_finished one time
		multiFinishing = false;
		currentEventExecute.Manage();

		currentEventExecute.FakeButtonFinished.Clicked += new EventHandler(on_multi_chronopic_finished);
	}

	bool multiFinishing;
	private void on_multi_chronopic_finished (object o, EventArgs args) {
		if(multiFinishing)
			return;
		else
			multiFinishing = true;

		currentEventExecute.FakeButtonFinished.Clicked -= new EventHandler(on_multi_chronopic_finished);

		if(currentMultiChronopicType.Name == Constants.RunAnalysisName && ! currentEventExecute.MultiChronopicRunAUsedCP2()) 
			//new DialogMessage(Constants.MessageTypes.WARNING, 
			//		Catalog.GetString("This Run Analysis is not valid because there are no strides."));
			eventExecuteWin.RunANoStrides();
		else if ( ! currentEventExecute.Cancel ) {
Console.WriteLine("T");
			/*
			   on runAnalysis test, when cp1 ends, run ends,
			   but cp2 is still waiting event
			   with this will ask cp2 to press button
			   solves problem with threads at ending
			   */

			//on_finish_multi_clicked(o, args);
			//this produces also a crash:
			//new DialogMessage(Constants.MessageTypes.INFO, "Please, touch a platform now.");
Console.WriteLine("U");
			//call write here, because if done in execute/MultiChronopic, will be called n times if n chronopics are working
			currentEventExecute.MultiChronopicWrite(false);
Console.WriteLine("V");
			currentMultiChronopic = (MultiChronopic) currentEventExecute.EventDone;
Console.WriteLine("W");
			//this produces also a crash:
			//new DialogMessage(Constants.MessageTypes.INFO, "Please, touch a platform now.");

Console.WriteLine("W2");
			
			//if this multichronopic has more chronopics than other in session, then reload treeview, else simply add
			if(currentMultiChronopic.CPs() != SqliteMultiChronopic.MaxCPs(currentSession.UniqueID)) {
				treeview_multi_chronopic_storeReset();
				fillTreeView_multi_chronopic();
			} else
				myTreeViewMultiChronopic.Add(currentPerson.Name, currentMultiChronopic);
Console.WriteLine("X");
			
			//since 0.7.4.1 when test is done, treeview select it. action event button have to be shown 
			showHideActionEventButtons(true, Constants.MultiChronopicName); //show
		
			//unhide buttons for delete last test
			sensitiveGuiYesEvent();
		}
		
		//unhide buttons that allow doing another test
		Console.WriteLine("RR3");
		sensitiveGuiEventDone();
		Console.WriteLine("RR4");
	}
		


	/*
	 * update button is clicked on eventWindow, chronojump.cs delegate points here
	 */
	
	private void on_update_clicked (object o, EventArgs args) {
		Log.WriteLine("--On_update_clicked--");
		try {
			switch (currentEventType.Type) {
				case EventType.Types.JUMP:
					if(currentJumpType.IsRepetitive) 
						eventExecuteWin.PrepareJumpReactiveGraph(
								Util.GetLast(currentJumpRj.TvString), Util.GetLast(currentJumpRj.TcString),
								currentJumpRj.TvString, currentJumpRj.TcString, volumeOn, repetitiveConditionsWin);
					else 
						eventExecuteWin.PrepareJumpSimpleGraph(currentJump.Tv, currentJump.Tc);
					break;
				case EventType.Types.RUN:
					if(currentRunType.HasIntervals) {
							RunType runType = SqliteRunIntervalType.SelectAndReturnRunIntervalType(currentRunInterval.Type);
							double distanceTotal = Util.GetRunITotalDistance(currentRunInterval.DistanceInterval, 
									runType.DistancesString, currentRunInterval.Tracks);

							double distanceInterval = currentRunInterval.DistanceInterval;
							if(distanceInterval == -1) //variable distances
								distanceInterval = Util.GetRunIVariableDistancesStringRow(
										runType.DistancesString, (int) currentRunInterval.Tracks -1);

							eventExecuteWin.PrepareRunIntervalGraph(distanceInterval, 
								Util.GetLast(currentRunInterval.IntervalTimesString), 
								currentRunInterval.IntervalTimesString, 
								distanceTotal,
								runType.DistancesString,
								volumeOn, repetitiveConditionsWin);
					} else
						eventExecuteWin.PrepareRunSimpleGraph(currentRun.Time, currentRun.Speed);
					break;
				case EventType.Types.PULSE:
					eventExecuteWin.PreparePulseGraph(Util.GetLast(currentPulse.TimesString), currentPulse.TimesString);
					break;
				case EventType.Types.REACTIONTIME:
					eventExecuteWin.PrepareReactionTimeGraph(currentReactionTime.Time);
					break;
				case EventType.Types.MULTICHRONOPIC:
					eventExecuteWin.PrepareMultiChronopicGraph(
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
		notebook_change(0);
		Log.WriteLine("Edit selected jump (normal)");
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person (check also if it's not a individual RJ, the pass the parent RJ)
		if (myTreeViewJumps.EventSelectedID > 0) {
			//3.- obtain the data of the selected jump
			Jump myJump = SqliteJump.SelectJumpData( myTreeViewJumps.EventSelectedID );
			eventOldPerson = myJump.PersonID;
		
			//4.- edit this jump
			editJumpWin = EditJumpWindow.Show(app1, myJump, weightPercentPreferred, prefsDigitsNumber);
			editJumpWin.Button_accept.Clicked += new EventHandler(on_edit_selected_jump_accepted);
		}
	}
	
	private void on_edit_selected_jump_rj_clicked (object o, EventArgs args) {
		notebook_change(1);
		Log.WriteLine("Edit selected jump (RJ)");
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person (check also if it's not a individual RJ, the pass the parent RJ)
		if (myTreeViewJumpsRj.EventSelectedID > 0) {
			//3.- obtain the data of the selected jump
			JumpRj myJump = SqliteJumpRj.SelectJumpData( "jumpRj", myTreeViewJumpsRj.EventSelectedID );
			eventOldPerson = myJump.PersonID;
		
			//4.- edit this jump
			editJumpRjWin = EditJumpRjWindow.Show(app1, myJump, weightPercentPreferred, prefsDigitsNumber);
			editJumpRjWin.Button_accept.Clicked += new EventHandler(on_edit_selected_jump_rj_accepted);
		}
	}
	
	private void on_edit_selected_jump_accepted (object o, EventArgs args) {
		Log.WriteLine("edit selected jump accepted");
	
		Jump myJump = SqliteJump.SelectJumpData( myTreeViewJumps.EventSelectedID );

		//if person changed, fill treeview again, if not, only update it's line
		if(eventOldPerson == myJump.PersonID) {
			if(! weightPercentPreferred) {
				double personWeight = SqlitePersonSession.SelectAttribute(
						myJump.PersonID, currentSession.UniqueID, Constants.Weight);
				myJump.Weight = Util.WeightFromPercentToKg(myJump.Weight, personWeight);
			}
			myTreeViewJumps.Update(myJump);
		}
		else {
			treeview_jumps_storeReset();
			fillTreeView_jumps(UtilGtk.ComboGetActive(combo_jumps));
		}

		if(createdStatsWin) 
			statsWin.FillTreeView_stats(false, false);
	}
	
	private void on_edit_selected_jump_rj_accepted (object o, EventArgs args) {
		Log.WriteLine("edit selected jump RJ accepted");
	
		JumpRj myJump = SqliteJumpRj.SelectJumpData( "jumpRj", myTreeViewJumpsRj.EventSelectedID );
		
		//if person changed, fill treeview again, if not, only update it's line
		if(eventOldPerson == myJump.PersonID) {
			if(! weightPercentPreferred) {
				double personWeight = SqlitePersonSession.SelectAttribute(
						myJump.PersonID, currentSession.UniqueID, Constants.Weight);
				myJump.Weight = Util.WeightFromPercentToKg(myJump.Weight, personWeight);
			}
			myTreeViewJumpsRj.Update(myJump);
		}
		else {
			treeview_jumps_rj_storeReset();
			fillTreeView_jumps_rj(UtilGtk.ComboGetActive(combo_jumps_rj));
		}

		if(createdStatsWin) 
			statsWin.FillTreeView_stats(false, false);
	}
	
	private void on_edit_selected_run_clicked (object o, EventArgs args) {
		notebook_change(2);
		Log.WriteLine("Edit selected run (normal)");
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person (check also if it's not a individual RJ, the pass the parent RJ)
		if (myTreeViewRuns.EventSelectedID > 0) {
			//3.- obtain the data of the selected run
			Run myRun = SqliteRun.SelectRunData( myTreeViewRuns.EventSelectedID );
			myRun.MetersSecondsPreferred = metersSecondsPreferred;
			eventOldPerson = myRun.PersonID;
		
			//4.- edit this run
			editRunWin = EditRunWindow.Show(app1, myRun, prefsDigitsNumber, metersSecondsPreferred);
			editRunWin.Button_accept.Clicked += new EventHandler(on_edit_selected_run_accepted);
		}
	}
	
	private void on_edit_selected_run_interval_clicked (object o, EventArgs args) {
		notebook_change(3);
		Log.WriteLine("Edit selected run interval");
		//1.- check that there's a line selected
		//2.- check that this line is a run and not a person (check also if it's not a individual subrun, the pass the parent run)
		if (myTreeViewRunsInterval.EventSelectedID > 0) {
			//3.- obtain the data of the selected run
			RunInterval myRun = SqliteRunInterval.SelectRunData( Constants.RunIntervalTable, myTreeViewRunsInterval.EventSelectedID );
			eventOldPerson = myRun.PersonID;
		
			//4.- edit this run
			editRunIntervalWin = EditRunIntervalWindow.Show(app1, myRun, prefsDigitsNumber, metersSecondsPreferred);
			editRunIntervalWin.Button_accept.Clicked += new EventHandler(on_edit_selected_run_interval_accepted);
		}
	}
	
	private void on_edit_selected_run_accepted (object o, EventArgs args) {
		Log.WriteLine("edit selected run accepted");
		
		Run myRun = SqliteRun.SelectRunData( myTreeViewRuns.EventSelectedID );
		
		//if person changed, fill treeview again, if not, only update it's line
		if(eventOldPerson == myRun.PersonID)
			myTreeViewRuns.Update(myRun);
		else {
			treeview_runs_storeReset();
			fillTreeView_runs(UtilGtk.ComboGetActive(combo_runs));
		}
		
		if(createdStatsWin) 
			statsWin.FillTreeView_stats(false, false);
	}
	
	private void on_edit_selected_run_interval_accepted (object o, EventArgs args) {
		Log.WriteLine("edit selected run interval accepted");
		
		RunInterval myRun = SqliteRunInterval.SelectRunData( Constants.RunIntervalTable, myTreeViewRunsInterval.EventSelectedID );

		//if person changed, fill treeview again, if not, only update it's line
		if(eventOldPerson == myRun.PersonID)
			myTreeViewRunsInterval.Update(myRun);
		else {
			treeview_runs_interval_storeReset();
			fillTreeView_runs_interval(UtilGtk.ComboGetActive(combo_runs_interval));
		}
		
		if(createdStatsWin)
			statsWin.FillTreeView_stats(false, false);
	}

	private void on_edit_selected_reaction_time_clicked (object o, EventArgs args) {
		notebook_change(4);
		Log.WriteLine("Edit selected reaction time");
		//1.- check that there's a line selected
		//2.- check that this line is a event and not a person
		if (myTreeViewReactionTimes.EventSelectedID > 0) {
			//3.- obtain the data of the selected event
			ReactionTime myRT = SqliteReactionTime.SelectReactionTimeData( myTreeViewReactionTimes.EventSelectedID );
			eventOldPerson = myRT.PersonID;
		
			//4.- edit this event
			editReactionTimeWin = EditReactionTimeWindow.Show(app1, myRT, prefsDigitsNumber);
			editReactionTimeWin.Button_accept.Clicked += new EventHandler(on_edit_selected_reaction_time_accepted);
		}
	}
	
	private void on_edit_selected_reaction_time_accepted (object o, EventArgs args) {
		Log.WriteLine("edit selected reaction time accepted");
		
		ReactionTime myRT = SqliteReactionTime.SelectReactionTimeData( myTreeViewReactionTimes.EventSelectedID );

		//if person changed, fill treeview again, if not, only update it's line
		if(eventOldPerson == myRT.PersonID)
			myTreeViewReactionTimes.Update(myRT);
		else {
			treeview_reaction_times_storeReset();
			fillTreeView_reaction_times();
		}
	
		//if(createdStatsWin) {
		//	statsWin.FillTreeView_stats(false, false);
		//}
	}
	
	private void on_edit_selected_pulse_clicked (object o, EventArgs args) {
		notebook_change(5);
		Log.WriteLine("Edit selected pulse");
		//1.- check that there's a line selected
		//2.- check that this line is a event and not a person
		if (myTreeViewPulses.EventSelectedID > 0) {
			//3.- obtain the data of the selected event
			Pulse myPulse = SqlitePulse.SelectPulseData( myTreeViewPulses.EventSelectedID );
			eventOldPerson = myPulse.PersonID;
		
			//4.- edit this event
			editPulseWin = EditPulseWindow.Show(app1, myPulse, prefsDigitsNumber);
			editPulseWin.Button_accept.Clicked += new EventHandler(on_edit_selected_pulse_accepted);
		}
	}
	
	private void on_edit_selected_pulse_accepted (object o, EventArgs args) {
		Log.WriteLine("edit selected pulse accepted");
		
		Pulse myPulse = SqlitePulse.SelectPulseData( myTreeViewPulses.EventSelectedID );

		//if person changed, fill treeview again, if not, only update it's line
		if(eventOldPerson == myPulse.PersonID)
			myTreeViewPulses.Update(myPulse);
		else {
			treeview_pulses_storeReset();
			fillTreeView_pulses(UtilGtk.ComboGetActive(combo_pulses));
		}
	
		//if(createdStatsWin) {
		//	statsWin.FillTreeView_stats(false, false);
		//}
	}
	
	private void on_edit_selected_multi_chronopic_clicked (object o, EventArgs args) {
		notebook_change(6);
		Log.WriteLine("Edit selected multi chronopic");
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person (check also if it's not a individual RJ, the pass the parent RJ)
		if (myTreeViewMultiChronopic.EventSelectedID > 0) {
			//3.- obtain the data of the selected test
			MultiChronopic mc = SqliteMultiChronopic.SelectMultiChronopicData( myTreeViewMultiChronopic.EventSelectedID );
			eventOldPerson = mc.PersonID;
		
			//4.- edit this jump
			editMultiChronopicWin = EditMultiChronopicWindow.Show(app1, mc, prefsDigitsNumber);
			editMultiChronopicWin.Button_accept.Clicked += new EventHandler(on_edit_selected_multi_chronopic_accepted);
		}
	}

	private void on_edit_selected_multi_chronopic_accepted (object o, EventArgs args) {
		Log.WriteLine("edit selected multi chronopic accepted");
	
		MultiChronopic mc = SqliteMultiChronopic.SelectMultiChronopicData( myTreeViewMultiChronopic.EventSelectedID );
		
		//if person changed, fill treeview again, if not, only update it's line
		if(eventOldPerson == mc.PersonID) 
			myTreeViewMultiChronopic.Update(mc);
		else {
			treeview_multi_chronopic_storeReset();
			fillTreeView_multi_chronopic();
		}
	}
	
	/* ---------------------------------------------------------
	 * ----------------  EVENTS DELETE -------------------------
	 *  --------------------------------------------------------
	 */
	
	private void on_delete_selected_jump_clicked (object o, EventArgs args) {
		notebook_change(0);
		Log.WriteLine("delete selected jump (normal)");
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person
		Log.WriteLine(myTreeViewJumps.EventSelectedID.ToString());
		if (myTreeViewJumps.EventSelectedID > 0) {
			//3.- display confirmwindow of deletion 
			if (askDeletion) {
				confirmWinJumpRun = ConfirmWindowJumpRun.Show("Do you want to delete selected jump?", 
						"", "jump", myTreeViewJumps.EventSelectedID);
				confirmWinJumpRun.Button_accept.Clicked += new EventHandler(on_delete_selected_jump_accepted);
			} else {
				on_delete_selected_jump_accepted(o, args);
			}
		}
	}
	
	private void on_delete_selected_jump_rj_clicked (object o, EventArgs args) {
		notebook_change(1);
		Log.WriteLine("delete selected reactive jump");
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person (check also if it's not a individual RJ, the pass the parent RJ)
		if (myTreeViewJumpsRj.EventSelectedID > 0) {
			//3.- display confirmwindow of deletion 
			if (askDeletion) {
				confirmWinJumpRun = ConfirmWindowJumpRun.Show( Catalog.GetString("Do you want to delete selected jump?"), 
						 Catalog.GetString("Attention: Deleting a Reactive subjump will delete the whole jump"), 
						 "jump", myTreeViewJumpsRj.EventSelectedID);
				confirmWinJumpRun.Button_accept.Clicked += new EventHandler(on_delete_selected_jump_rj_accepted);
			} else {
				on_delete_selected_jump_rj_accepted(o, args);
			}
		}
	}
	
	private void on_delete_selected_jump_accepted (object o, EventArgs args) {
		Log.WriteLine("accept delete selected jump");
		
		SqliteJump.Delete( "jump", (myTreeViewJumps.EventSelectedID).ToString() );
		
		appbar2.Push( 1, Catalog.GetString ( "Deleted jump" ));
		myTreeViewJumps.DelEvent(myTreeViewJumps.EventSelectedID);
		showHideActionEventButtons(false, "Jump");

		if(createdStatsWin) {
			statsWin.FillTreeView_stats(false, false);
		}
	}

	private void on_delete_selected_jump_rj_accepted (object o, EventArgs args) {
		Log.WriteLine("accept delete selected jump");
		
		SqliteJump.Delete("jumpRj", myTreeViewJumpsRj.EventSelectedID.ToString());
		
		appbar2.Push( 1, Catalog.GetString ( "Deleted reactive jump" ));
		myTreeViewJumpsRj.DelEvent(myTreeViewJumpsRj.EventSelectedID);
		showHideActionEventButtons(false, "JumpRj");

		if(createdStatsWin) {
			statsWin.FillTreeView_stats(false, false);
		}
	}
	
	private void on_delete_selected_run_clicked (object o, EventArgs args) {
		notebook_change(2);
		Log.WriteLine("delete selected run (normal)");
		
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person
		if (myTreeViewRuns.EventSelectedID > 0) {
			//3.- display confirmwindow of deletion 
			if (askDeletion) {
				confirmWinJumpRun = ConfirmWindowJumpRun.Show("Do you want to delete selected run?", 
						"", "run", myTreeViewRuns.EventSelectedID);
				confirmWinJumpRun.Button_accept.Clicked += new EventHandler(on_delete_selected_run_accepted);
			} else {
				on_delete_selected_run_accepted(o, args);
			}
		}
	}
		
	
	private void on_delete_selected_run_interval_clicked (object o, EventArgs args) {
		notebook_change(3);
		Log.WriteLine("delete selected run interval");
		//1.- check that there's a line selected
		//2.- check that this line is a run and not a person (check also if it's a subrun, pass the parent run)
		if (myTreeViewRunsInterval.EventSelectedID > 0) {
			//3.- display confirmwindow of deletion 
			if (askDeletion) {
				confirmWinJumpRun = ConfirmWindowJumpRun.Show( Catalog.GetString("Do you want to delete selected run?"), 
						 Catalog.GetString("Attention: Deleting a Intervallic subrun will delete the whole run"), 
						 "run", myTreeViewJumpsRj.EventSelectedID);
				confirmWinJumpRun.Button_accept.Clicked += new EventHandler(on_delete_selected_run_interval_accepted);
			} else {
				on_delete_selected_run_interval_accepted(o, args);
			}
		}
	}

	private void on_delete_selected_run_accepted (object o, EventArgs args) {
		Log.WriteLine("accept delete selected run");
		
		SqliteRun.Delete( "run", (myTreeViewRuns.EventSelectedID).ToString() );
		
		appbar2.Push( 1, Catalog.GetString ( "Deleted selected run" ));
	
		myTreeViewRuns.DelEvent(myTreeViewRuns.EventSelectedID);
		showHideActionEventButtons(false, "Run");

		if(createdStatsWin) {
			statsWin.FillTreeView_stats(false, false);
		}
	}

	private void on_delete_selected_run_interval_accepted (object o, EventArgs args) {
		Log.WriteLine("accept delete selected run");
		
		SqliteRun.Delete( Constants.RunIntervalTable, (myTreeViewRunsInterval.EventSelectedID).ToString() );
		
		appbar2.Push( 1, Catalog.GetString ( "Deleted intervallic run" ));
	
		myTreeViewRunsInterval.DelEvent(myTreeViewRunsInterval.EventSelectedID);
		showHideActionEventButtons(false, "RunInterval");

		if(createdStatsWin) {
			statsWin.FillTreeView_stats(false, false);
		}
	}
	
	private void on_delete_selected_reaction_time_clicked (object o, EventArgs args) {
		notebook_change(4);
		Log.WriteLine("delete selected reaction time");
		
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person
		Log.WriteLine(myTreeViewReactionTimes.EventSelectedID.ToString());
		if (myTreeViewReactionTimes.EventSelectedID > 0) {
			//3.- display confirmwindow of deletion 
			if (askDeletion) {
				confirmWinJumpRun = ConfirmWindowJumpRun.Show("Do you want to delete selected test?", 
						"", "reactiontime", myTreeViewReactionTimes.EventSelectedID);
				confirmWinJumpRun.Button_accept.Clicked += new EventHandler(on_delete_selected_reaction_time_accepted);
			} else {
				on_delete_selected_reaction_time_accepted(o, args);
			}
		}
	}
		
	private void on_delete_selected_reaction_time_accepted (object o, EventArgs args) {
		Log.WriteLine("accept delete selected reaction time");
		
		SqliteJump.Delete( "reactiontime", (myTreeViewReactionTimes.EventSelectedID).ToString() );
		
		appbar2.Push( 1, Catalog.GetString ( "Deleted reaction time" ) );
		myTreeViewReactionTimes.DelEvent(myTreeViewReactionTimes.EventSelectedID);
		showHideActionEventButtons(false, "ReactionTime");

		/*
		if(createdStatsWin) {
			statsWin.FillTreeView_stats(false, false);
		}
		*/
	}

	private void on_delete_selected_pulse_clicked (object o, EventArgs args) {
		notebook_change(5);
		Log.WriteLine("delete selected pulse");
		
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person
		Log.WriteLine(myTreeViewPulses.EventSelectedID.ToString());
		if (myTreeViewPulses.EventSelectedID > 0) {
			//3.- display confirmwindow of deletion 
			if (askDeletion) {
				confirmWinJumpRun = ConfirmWindowJumpRun.Show("Do you want to delete selected test?", 
						"", "pulses", myTreeViewPulses.EventSelectedID);
				confirmWinJumpRun.Button_accept.Clicked += new EventHandler(on_delete_selected_pulse_accepted);
			} else {
				on_delete_selected_pulse_accepted(o, args);
			}
		}
	}
		
	private void on_delete_selected_pulse_accepted (object o, EventArgs args) {
		Log.WriteLine("accept delete selected pulse");
		
		SqliteJump.Delete( "pulse", (myTreeViewPulses.EventSelectedID).ToString() );
		
		appbar2.Push( 1, Catalog.GetString ( "Deleted pulse" ) );
		myTreeViewPulses.DelEvent(myTreeViewPulses.EventSelectedID);
		showHideActionEventButtons(false, "Pulse");

		/*
		if(createdStatsWin) {
			statsWin.FillTreeView_stats(false, false);
		}
		*/
	}

	private void on_delete_selected_multi_chronopic_clicked (object o, EventArgs args) {
		notebook_change(6);
		Log.WriteLine("delete selected multi chronopic");
		//1.- check that there's a line selected
		//2.- check that this line is a test and not a person (check also if it's not a individual mc, then pass the parent mc)
		if (myTreeViewMultiChronopic.EventSelectedID > 0) {
			//3.- display confirmwindow of deletion 
			if (askDeletion) {
				confirmWinJumpRun = ConfirmWindowJumpRun.Show( Catalog.GetString("Do you want to delete selected test?"), 
						"", "jump", myTreeViewMultiChronopic.EventSelectedID);
				confirmWinJumpRun.Button_accept.Clicked += new EventHandler(on_delete_selected_multi_chronopic_accepted);
			} else {
				on_delete_selected_multi_chronopic_accepted(o, args);
			}
		}
	}
	
	private void on_delete_selected_multi_chronopic_accepted (object o, EventArgs args) {
		Log.WriteLine("accept delete selected multi chronopic");
		
		SqliteMultiChronopic.Delete( (myTreeViewMultiChronopic.EventSelectedID).ToString() );
		
		appbar2.Push( 1, Catalog.GetString ( "Deleted multi chronopic" ));
	
		myTreeViewMultiChronopic.DelEvent(myTreeViewMultiChronopic.EventSelectedID);
		showHideActionEventButtons(false, Constants.MultiChronopicName);
	}
	



	/* ---------------------------------------------------------
	 * ----------------  EVENTS TYPE ADD -----------------------
	 *  --------------------------------------------------------
	 */

	
	private void on_jump_type_add_activate (object o, EventArgs args) {
		Log.WriteLine("Add new jump type");
			
		jumpTypeAddWin = JumpTypeAddWindow.Show(app1);
		jumpTypeAddWin.FakeButtonAccept.Clicked += new EventHandler(on_jump_type_add_accepted);
	}
	
	private void on_jump_type_add_accepted (object o, EventArgs args) {
		Log.WriteLine("ACCEPTED Add new jump type");
		if(jumpTypeAddWin.InsertedSimple) {
			UtilGtk.ComboUpdate(combo_jumps, SqliteJumpType.SelectJumpTypes(Constants.AllJumpsName, "", true), ""); //without filter, only select name
			new DialogMessage(Constants.MessageTypes.INFO, Catalog.GetString("Added simple jump."));
		} else {
			UtilGtk.ComboUpdate(combo_jumps_rj, SqliteJumpType.SelectJumpRjTypes(Constants.AllJumpsName, true), ""); //without filter, only select name
			new DialogMessage(Constants.MessageTypes.INFO, Catalog.GetString("Added reactive jump."));
		}
		combo_jumps.Active = 0;
		combo_jumps_rj.Active = 0;
	}

	private void on_run_type_add_activate (object o, EventArgs args) {
		Log.WriteLine("Add new run type");
			
		runTypeAddWin = RunTypeAddWindow.Show(app1);
		runTypeAddWin.FakeButtonAccept.Clicked += new EventHandler(on_run_type_add_accepted);
	}
	
	private void on_run_type_add_accepted (object o, EventArgs args) {
		Log.WriteLine("ACCEPTED Add new run type");
		if(runTypeAddWin.InsertedSimple) {
			UtilGtk.ComboUpdate(combo_runs, SqliteRunType.SelectRunTypes(Constants.AllRunsName, true), ""); //without filter, only select name
			new DialogMessage(Constants.MessageTypes.INFO, Catalog.GetString("Added simple run."));
		} else {
			UtilGtk.ComboUpdate(combo_runs_interval, SqliteRunIntervalType.SelectRunIntervalTypes(Constants.AllRunsName, true), ""); //without filter, only select name
			new DialogMessage(Constants.MessageTypes.INFO, Catalog.GetString("Added intervallic run."));
		}
		combo_runs.Active = 0;
		combo_runs_interval.Active = 0;
	}

	//reactiontime has no types

	private void on_pulse_type_add_activate (object o, EventArgs args) {
		Log.WriteLine("Add new pulse type");
	}
	
	private void on_pulse_type_add_accepted (object o, EventArgs args) {
		Log.WriteLine("ACCEPTED Add new pulse type");
	}

	/* ---------------------------------------------------------
	 * ----------------  EVENTS TYPE DELETE --------------------
	 *  --------------------------------------------------------
	 */

	private void on_jump_type_delete_simple_activate (object o, EventArgs args) {
		jumpsMoreWin = JumpsMoreWindow.Show(app1, false); //delete jump type
	}
	
	private void on_jump_type_delete_reactive_activate (object o, EventArgs args) {
		jumpsRjMoreWin = JumpsRjMoreWindow.Show(app1, false); //delete jump type
	}
	
	private void on_run_type_delete_simple_activate (object o, EventArgs args) {
		runsMoreWin = RunsMoreWindow.Show(app1, false); //delete run type
	}
	
	private void on_run_type_delete_intervallic_activate (object o, EventArgs args) {
		runsIntervalMoreWin = RunsIntervalMoreWindow.Show(app1, false); //delete run type
	}

	/* ---------------------------------------------------------
	 * ----------------  EVENTS REPAIR -------------------------
	 *  --------------------------------------------------------
	 */
	
	private void on_repair_selected_jump_rj_clicked (object o, EventArgs args) {
		notebook_change(1);
		Log.WriteLine("Repair selected subjump");
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person (check also if it's not a individual RJ, the pass the parent RJ)
		if (myTreeViewJumpsRj.EventSelectedID > 0) {
			//3.- obtain the data of the selected jump
			JumpRj myJump = SqliteJumpRj.SelectJumpData( "jumpRj", myTreeViewJumpsRj.EventSelectedID );
		
			//4.- edit this jump
			repairJumpRjWin = RepairJumpRjWindow.Show(app1, myJump, prefsDigitsNumber);
			repairJumpRjWin.Button_accept.Clicked += new EventHandler(on_repair_selected_jump_rj_accepted);
		}
	}
	
	private void on_repair_selected_jump_rj_accepted (object o, EventArgs args) {
		Log.WriteLine("Repair selected reactive jump accepted");
		
		treeview_jumps_rj_storeReset();
		fillTreeView_jumps_rj(UtilGtk.ComboGetActive(combo_jumps_rj));
		
		if(createdStatsWin) {
			statsWin.FillTreeView_stats(false, false);
		}
	}
	
	private void on_repair_selected_run_interval_clicked (object o, EventArgs args) {
		notebook_change(3);
		Log.WriteLine("Repair selected subrun");
		//1.- check that there's a line selected
		//2.- check that this line is a run and not a person 
		//(check also if it's not a individual run interval, then pass the parent run interval)
		if (myTreeViewRunsInterval.EventSelectedID > 0) {
			//3.- obtain the data of the selected run
			RunInterval myRun = SqliteRunInterval.SelectRunData( Constants.RunIntervalTable, myTreeViewRunsInterval.EventSelectedID );
		
			//4.- edit this run
			repairRunIntervalWin = RepairRunIntervalWindow.Show(app1, myRun, prefsDigitsNumber);
			repairRunIntervalWin.Button_accept.Clicked += new EventHandler(on_repair_selected_run_interval_accepted);
		}
	}
	
	private void on_repair_selected_run_interval_accepted (object o, EventArgs args) {
		Log.WriteLine("repair selected run interval accepted");
		
		treeview_runs_interval_storeReset();
		fillTreeView_runs_interval(UtilGtk.ComboGetActive(combo_runs_interval));
		
		if(createdStatsWin) {
			statsWin.FillTreeView_stats(false, false);
		}
	}

	private void on_repair_selected_pulse_clicked (object o, EventArgs args) {
		notebook_change(5);
		Log.WriteLine("Repair selected pulse");
		//1.- check that there's a line selected
		//2.- check that this line is a pulse and not a person 
		//(check also if it's not a individual pulse, then pass the parent pulse)
		if (myTreeViewPulses.EventSelectedID > 0) {
			//3.- obtain the data of the selected pulse
			Pulse myPulse = SqlitePulse.SelectPulseData( myTreeViewPulses.EventSelectedID );
		
			//4.- edit this pulse
			repairPulseWin = RepairPulseWindow.Show(app1, myPulse, prefsDigitsNumber);
			repairPulseWin.Button_accept.Clicked += new EventHandler(on_repair_selected_pulse_accepted);
		}
	}
	
	private void on_repair_selected_pulse_accepted (object o, EventArgs args) {
		Log.WriteLine("repair selected pulse accepted");
		
		treeview_pulses_storeReset();
		fillTreeView_pulses(UtilGtk.ComboGetActive(combo_pulses));
		
		/*
		if(createdStatsWin) {
			statsWin.FillTreeView_stats(false, false);
		}
		*/
	}

	private void on_repair_selected_multi_chronopic_clicked (object o, EventArgs args) {
		notebook_change(6);
		Log.WriteLine("Repair selected multichronopic");
	}
	

	/* ---------------------------------------------------------
	 * ----------------  SOME MORE CALLBACKS---------------------
	 *  --------------------------------------------------------
	 */
	
	//changed by chronojump when it's needed
	private void notebook_change(int desiredPage) {
		while(notebook.CurrentPage < desiredPage) 
			notebook.NextPage();
		while(notebook.CurrentPage > desiredPage) 
			notebook.PrevPage();
	}
	
	//changed by user clicking on notebook tabs
	private void on_notebook_change_by_user (object o, SwitchPageArgs args) {
		//show chronojump logo on down-left area
		changeTestImage("", "", "LOGO");
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
		System.Diagnostics.Process.Start(Path.GetFullPath(Util.GetManualDir())); 
	}

	private void on_menuitem_formulas_activate (object o, EventArgs args) {
		new DialogMessage(Constants.MessageTypes.INFO, "Here there will be bibliographic information about formulas and some notes.\n\nProbably this will be a window and not a dialog\n\nNote text is selectable");
	}

	private void on_about1_activate (object o, EventArgs args) {
		string translator_credits = Catalog.GetString ("translator-credits");
		//only print if exist (don't print 'translator-credits' word
		if(translator_credits == "translator-credits") 
			translator_credits = "";

		new About(progVersion, translator_credits);
	}

	private void on_checkbutton_volume_clicked(object o, EventArgs args) {
		if(volumeOn) {
			volumeOn = false;
			SqlitePreferences.Update("volumeOn", "False", false);
		} else {
			volumeOn = true;
			SqlitePreferences.Update("volumeOn", "True", false);
		}
		changeVolumeButton(volumeOn);

//		if(repetitiveConditionsWin != null)
			repetitiveConditionsWin.VolumeOn = volumeOn;

		//Log.WriteLine("VolumeOn: {0}", volumeOn.ToString());
	}
		
	private void changeVolumeButton(bool myVolume) {
		Pixbuf pixbuf;
		if(myVolume) 
			pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "audio-volume-high.png");
		else 
			pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "audio-volume-muted.png");
		image_volume.Pixbuf = pixbuf;
	}

	private void on_button_rj_bells_clicked(object o, EventArgs args) {
		repetitiveConditionsWin.View(true, volumeOn); //show jumps
	}

	private void on_button_time_bells_clicked(object o, EventArgs args) {
		repetitiveConditionsWin.View(false, volumeOn); //show runs
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
	}
	
	private void menuPersonSensitive(bool option)
	{
		menuitem_person_add_single.Sensitive = option;
		menuitem_person_add_multiple.Sensitive = option;
		menuitem_recuperate_person.Sensitive = option;
		menuitem_recuperate_persons_from_session.Sensitive = option;
	}
	
	private void menuPersonSelectedSensitive(bool option)
	{
		menuitem_edit_current_person.Sensitive = option;
		menuitem_show_all_person_events.Sensitive = option;
		menuitem_delete_current_person_from_session.Sensitive = option;
		button_edit_current_person.Sensitive = option;
		button_show_all_person_events.Sensitive = option;
	}

	private void menuJumpsSensitive(bool option)
	{
		menu_execute_simple_jumps1.Sensitive = option;
		menuitem_edit_selected_jump.Sensitive = option;
		menuitem_delete_selected_jump.Sensitive = option;
		menu_execute_repetitive_jumps1.Sensitive = option;
		menuitem_edit_selected_jump_rj.Sensitive = option;
		menuitem_repair_selected_jump_rj.Sensitive = option;
		menuitem_delete_selected_jump_rj.Sensitive = option;
		menuitem_jump_type_add.Sensitive = option;
		menuitem_jump_type_delete_simple.Sensitive = option;
		menuitem_jump_type_delete_reactive.Sensitive = option;
	}
	
	private void menuRunsSensitive(bool option)
	{
		menu_execute_simple_runs1.Sensitive = option;
		menuitem_edit_selected_run.Sensitive = option;
		menuitem_delete_selected_run.Sensitive = option;
		menu_execute_intervallic_runs1.Sensitive = option;
		menuitem_edit_selected_run_interval.Sensitive = option;
		menuitem_repair_selected_run_interval.Sensitive = option;
		menuitem_delete_selected_run_interval.Sensitive = option;
		menuitem_run_type_add.Sensitive = option;
		menuitem_run_type_delete_simple.Sensitive = option;
		menuitem_run_type_delete_intervallic.Sensitive = option;
	}
	
	private void menuOtherSensitive(bool option)
	{
		menuitem_reaction_time.Sensitive = option;
		menuitem_pulse_free.Sensitive = option;
		menuitem_pulse_custom.Sensitive = option;
		menuitem_multi_chronopic_start.Sensitive = option;
		menuitem_run_analysis.Sensitive = option;
	}
	
	private void menuToolsSensitive(bool option)
	{
		menuitem_view_stats.Sensitive = option;
		menuitem_report_window.Sensitive = option;
		menuitem_server_evaluator_data.Sensitive = option;
		menuitem_server_upload_session.Sensitive = option;
	}

	private void sensitiveGuiNoSession () 
	{
		menuitem_preferences.Sensitive = true;
		treeview_persons.Sensitive = false;
		
		//menuitems
		menuSessionSensitive(false);
		menuPersonSensitive(false);
		menuPersonSelectedSensitive(false);
		menuJumpsSensitive(false);
		menuRunsSensitive(false);
		menuOtherSensitive(false);
		menuToolsSensitive(false);
		
		vbox_image_test.Sensitive = false;
		frame_persons.Sensitive = false;
		button_recup_per.Sensitive = false;
		button_create_per.Sensitive = false;
		button_edit_current_person.Sensitive = false;
		
		//notebook
		notebook.Sensitive = false;
		
		button_last.Sensitive = false;
		button_rj_last.Sensitive=false;
		button_run_last.Sensitive=false;
		button_run_interval_last.Sensitive=false;
	}
	
	private void sensitiveGuiYesSession () {
		vbox_image_test.Sensitive = true;
		frame_persons.Sensitive = true;
		button_recup_per.Sensitive = true;
		button_create_per.Sensitive = true;
		
		menuSessionSensitive(true);
		menuPersonSensitive(true);
		menuToolsSensitive(true);
	}

	//only called by delete person functions (if we run out of persons)
	private void sensitiveGuiNoPerson () {
		notebook.Sensitive = false;
		treeview_persons.Sensitive = false;
		
		menuPersonSelectedSensitive(false);
		menuJumpsSensitive(false);
		menuRunsSensitive(false);
		menuOtherSensitive(false);
		menuToolsSensitive(false);
	}
	
	private void sensitiveGuiYesPerson () {
		notebook.Sensitive = true;
		treeview_persons.Sensitive = true;
		
		menuPersonSelectedSensitive(true);
		menuJumpsSensitive(true);
		menuRunsSensitive(true);
		menuOtherSensitive(true);
		menuToolsSensitive(true);
	
		//unsensitive edit, delete, repair events because no event is initially selected
		showHideActionEventButtons(false, "ALL");

		combo_jumps.Sensitive = true;
		combo_jumps_rj.Sensitive = true;
		combo_runs.Sensitive = true;
		combo_runs_interval.Sensitive = true;
		combo_pulses.Sensitive = true;
	}
	
	private void sensitiveGuiYesEvent () {
	}
	
	private void sensitiveGuiEventDoing () {
		//hbox
		hbox_jumps.Sensitive = false;
		hbox_jumps_rj.Sensitive = false;
		hbox_runs.Sensitive = false;
		hbox_runs_interval.Sensitive = false;
		hbox_pulses.Sensitive = false;
		
		//menu
		menuJumpsSensitive(false);
		menuRunsSensitive(false);
		menuOtherSensitive(false);
			
		hbox_multi_chronopic_buttons.Sensitive = false;
	}
   
	private void sensitiveGuiEventDone () {
		//hbox
		hbox_jumps.Sensitive = true;
		hbox_jumps_rj.Sensitive = true;
		hbox_runs.Sensitive = true;
		hbox_runs_interval.Sensitive = true;
		hbox_pulses.Sensitive = true;

		//allow repeat last jump or run (check also if it wasn't cancelled)
		if(! currentEventExecute.Cancel) {
			switch (currentEventType.Type) {
				case EventType.Types.JUMP:
					if(currentJumpType.IsRepetitive) {
						button_rj_last.Sensitive = true;
						button_last.Sensitive = false;
					} else {
						button_last.Sensitive = true;
						button_rj_last.Sensitive = false;
					}
					break;
				case EventType.Types.RUN:
					if(currentRunType.HasIntervals) {
						button_run_interval_last.Sensitive = true;
						button_run_last.Sensitive = false;
					} else {
						button_run_last.Sensitive = true;
						button_run_interval_last.Sensitive = false;
					}
					break;
				case EventType.Types.REACTIONTIME:
					Log.WriteLine("sensitiveGuiEventDone reaction time");
					break;
				case EventType.Types.PULSE:
					Log.WriteLine("sensitiveGuiEventDone pulse");
					break;
				case EventType.Types.MULTICHRONOPIC:
					Log.WriteLine("sensitiveGuiEventDone multichronopic");
					hbox_multi_chronopic_buttons.Sensitive = true;
					break;
				default:
					Log.WriteLine("sensitiveGuiEventDone default");
					break;
			}
		}
		
		//menu
		menuJumpsSensitive(true);
		menuRunsSensitive(true);
		menuOtherSensitive(true);
	}

	private void showHideActionEventButtons(bool show, string type) {
		bool success = false;
		if(type == "ALL" || type == "Jump") {
			menuitem_edit_selected_jump.Sensitive = show;
			menuitem_delete_selected_jump.Sensitive = show;
			button_edit_selected_jump.Sensitive = show;
			button_delete_selected_jump.Sensitive = show;
			success = true;
		} 
		if (type == "ALL" || type == "JumpRj") {
			menuitem_edit_selected_jump_rj.Sensitive = show;
			menuitem_delete_selected_jump_rj.Sensitive = show;
			button_edit_selected_jump_rj.Sensitive = show;
			button_delete_selected_jump_rj.Sensitive = show;
			button_repair_selected_jump_rj.Sensitive = show;
			menuitem_repair_selected_jump_rj.Sensitive = show;
			success = true;
		} 
		if (type == "ALL" || type == "Run") {
			menuitem_edit_selected_run.Sensitive = show;
			menuitem_delete_selected_run.Sensitive = show;
			button_edit_selected_run.Sensitive = show;
			button_delete_selected_run.Sensitive = show;
			success = true;
		} 
		if (type == "ALL" || type == "RunInterval") {
			menuitem_edit_selected_run_interval.Sensitive = show;
			menuitem_delete_selected_run_interval.Sensitive = show;
			button_edit_selected_run_interval.Sensitive = show;
			button_delete_selected_run_interval.Sensitive = show;
			button_repair_selected_run_interval.Sensitive = show;
			menuitem_repair_selected_run_interval.Sensitive = show;
			success = true;
		} 
		if (type == "ALL" || type == "ReactionTime") {
			button_edit_selected_reaction_time.Sensitive = show;
			button_delete_selected_reaction_time.Sensitive = show;
			success = true;
		} 
		if (type == "ALL" || type == "Pulse") {
			// menuitem_edit_selected_pulse.Sensitive = show;
			// menuitem_delete_selected_pulse.Sensitive = show;
			button_edit_selected_pulse.Sensitive = show;
			button_delete_selected_pulse.Sensitive = show;
			button_repair_selected_pulse.Sensitive = show;
			success = true;
		} 
		if (type == "ALL" || type == Constants.MultiChronopicName) {
			button_edit_selected_multi_chronopic.Sensitive = show;
			button_delete_selected_multi_chronopic.Sensitive = show;
			success = true;
		} 
		if (!success) {
			Log.WriteLine(string.Format("Error in showHideActionEventButtons, type: {0}", type));
		}
	}
	
	/*
	 * voluntary crash for testing purposes 
	 */

	private void on_debug_crash_activate (object o, EventArgs args) {
		bool voluntaryCrashAllowed = false;
		if(voluntaryCrashAllowed) {
			ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString("Done for testing purposes. Chronojump will exit badly"), "", "Are you sure you want to crash application?");
			confirmWin.Button_accept.Clicked += new EventHandler(crashing);
		} else {
			new DialogMessage(Constants.MessageTypes.INFO, "Currently disabled.");
		}
	}

	private void crashing (object o, EventArgs args) {
		string [] myString = new String [3];
		Console.WriteLine(myString[5]);
	}

	private void on_menuitem_server_activate (object o, EventArgs args) {
		Log.WriteLine("SERVER");
	}


}
