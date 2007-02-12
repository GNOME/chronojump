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
 * Xavier de Blas: 
 * http://www.xdeblas.com, http://www.deporteyciencia.com (parleblas)
 */


using System;
using Gtk;
using Gdk;
using Glade;
using System.IO.Ports;
using Mono.Unix;

public class ChronoJump 
{
	[Widget] Gtk.Window app1;
	[Widget] Gtk.Statusbar appbar2;
	[Widget] Gtk.TreeView treeview_persons;
	[Widget] Gtk.TreeView treeview_jumps;
	[Widget] Gtk.TreeView treeview_jumps_rj;
	[Widget] Gtk.TreeView treeview_runs;
	[Widget] Gtk.TreeView treeview_runs_interval;
	[Widget] Gtk.TreeView treeview_pulses;
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
	[Widget] Gtk.Combo combo_jumps;
	[Widget] Gtk.Combo combo_jumps_rj;
	[Widget] Gtk.Combo combo_runs;
	[Widget] Gtk.Combo combo_runs_interval;
	[Widget] Gtk.Combo combo_pulses;

	[Widget] Gtk.MenuItem menuitem_edit_selected_jump;
	[Widget] Gtk.MenuItem menuitem_delete_selected_jump;
	[Widget] Gtk.Button button_edit_selected_jump;
	[Widget] Gtk.Button button_delete_selected_jump;
	[Widget] Gtk.MenuItem menuitem_edit_selected_jump_rj;
	[Widget] Gtk.MenuItem menuitem_delete_selected_jump_rj;
	[Widget] Gtk.Button button_edit_selected_jump_rj;
	[Widget] Gtk.Button button_delete_selected_jump_rj;
	[Widget] Gtk.Button button_repair_selected_reactive_jump;
	[Widget] Gtk.MenuItem menuitem_repair_selected_reactive_jump;
	
	[Widget] Gtk.MenuItem menuitem_edit_selected_run;
	[Widget] Gtk.MenuItem menuitem_delete_selected_run;
	[Widget] Gtk.Button button_edit_selected_run;
	[Widget] Gtk.Button button_delete_selected_run;
	[Widget] Gtk.MenuItem menuitem_edit_selected_run_interval;
	[Widget] Gtk.MenuItem menuitem_delete_selected_run_interval;
	[Widget] Gtk.Button button_edit_selected_run_interval;
	[Widget] Gtk.Button button_delete_selected_run_interval;

	//[Widget] Gtk.MenuItem menuitem_edit_selected_pulse;
	//[Widget] Gtk.MenuItem menuitem_delete_selected_pulse;
	[Widget] Gtk.Button button_edit_selected_pulse;
	[Widget] Gtk.Button button_delete_selected_pulse;

	//widgets for enable or disable
	[Widget] Gtk.Button button_new;
	[Widget] Gtk.Button button_open;
	[Widget] Gtk.Frame frame_persons;
	[Widget] Gtk.Button button_recup_per;
	[Widget] Gtk.Button button_create_per;

	[Widget] Gtk.Button button_sj;
	[Widget] Gtk.Button button_sj_plus;
	[Widget] Gtk.Button button_cmj;
	[Widget] Gtk.Button button_abk;
	[Widget] Gtk.Button button_dj;
	[Widget] Gtk.Button button_more;
	[Widget] Gtk.Button button_rj_j;
	[Widget] Gtk.Button button_rj_t;
	[Widget] Gtk.Button button_rj_unlimited;
	[Widget] Gtk.Button button_run_custom;
	[Widget] Gtk.Button button_run_20m;
	[Widget] Gtk.Button button_run_100m;
	[Widget] Gtk.Button button_run_200m;
	[Widget] Gtk.Button button_run_400m;
	[Widget] Gtk.Button button_run_1000m;
	[Widget] Gtk.Button button_run_2000m;
	[Widget] Gtk.Button button_run_interval_by_laps;
	[Widget] Gtk.Button button_run_interval_by_time;
	[Widget] Gtk.Button button_run_interval_unlimited;
	[Widget] Gtk.Button button_pulse_custom;
	[Widget] Gtk.Button button_pulse_more;
	[Widget] Gtk.Button button_pulse_last;
	
	[Widget] Gtk.Button button_last;
	[Widget] Gtk.Button button_rj_last;
	[Widget] Gtk.Button button_run_last;
	[Widget] Gtk.Button button_run_interval_last;
	[Widget] Gtk.Button button_last_delete;
	[Widget] Gtk.MenuItem menuitem_preferences;
	[Widget] Gtk.MenuItem menuitem_export_csv;
	[Widget] Gtk.MenuItem menuitem_export_xml;
	[Widget] Gtk.MenuItem menuitem_recuperate_person;
	[Widget] Gtk.MenuItem menuitem_person_add_single;
	[Widget] Gtk.MenuItem menuitem_person_add_multiple;
	[Widget] Gtk.MenuItem menuitem_edit_session;
	[Widget] Gtk.MenuItem menuitem_delete_session;
	[Widget] Gtk.MenuItem menuitem_recuperate_persons_from_session;
	[Widget] Gtk.MenuItem menu_persons;
	[Widget] Gtk.MenuItem menu_jumps;
	[Widget] Gtk.MenuItem menu_runs;
	[Widget] Gtk.MenuItem menu_pulses;
	[Widget] Gtk.MenuItem menu_view;
		
	[Widget] Gtk.MenuItem sj;
	[Widget] Gtk.MenuItem sj_plus;
	[Widget] Gtk.MenuItem cmj;
	[Widget] Gtk.MenuItem abk;
	[Widget] Gtk.MenuItem dj;
	[Widget] Gtk.MenuItem more_simple_jumps;
	[Widget] Gtk.MenuItem more_rj;
	[Widget] Gtk.MenuItem menuitem_jump_type_add;
	[Widget] Gtk.MenuItem rj_j;
	[Widget] Gtk.MenuItem rj_t;
	[Widget] Gtk.MenuItem rj_unlimited;
	[Widget] Gtk.MenuItem menuitem_run_custom;
	[Widget] Gtk.MenuItem menuitem_20m;
	[Widget] Gtk.MenuItem menuitem_100m;
	[Widget] Gtk.MenuItem menuitem_200m;
	[Widget] Gtk.MenuItem menuitem_400m;
	[Widget] Gtk.MenuItem menuitem_1000m;
	[Widget] Gtk.MenuItem menuitem_2000m;
	[Widget] Gtk.MenuItem menuitem_run_interval_by_laps;
	[Widget] Gtk.MenuItem menuitem_run_interval_by_time;
	[Widget] Gtk.MenuItem menuitem_run_interval_unlimited;

	[Widget] Gtk.Button button_edit_current_person;
	[Widget] Gtk.MenuItem menuitem_edit_current_person;
	[Widget] Gtk.MenuItem menuitem_delete_current_person_from_session;
	[Widget] Gtk.Button button_show_all_person_events;
	[Widget] Gtk.MenuItem show_all_person_events;
	
	[Widget] Gtk.RadioMenuItem menuitem_simulated;
	[Widget] Gtk.RadioMenuItem menuitem_chronopic;
	
	[Widget] Gtk.Notebook notebook;
		
	private Random rand;
	
	private static string [] authors = {"Xavier de Blas", "Juan Gonzalez"};
	private static string progversion = "0.5-rev4";
	private static string progname = "Chronojump";
	
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
	//pulses
	private TreeStore treeview_pulses_store;
	private TreeViewPulses myTreeViewPulses;

	//preferences variables
	private static string chronopicPort;
	private static int prefsDigitsNumber;
	private static bool showHeight;
	private static bool showInitialSpeed;
	private static bool showQIndex;
	private static bool showDjIndex;
	private static bool simulated;
	private static bool askDeletion;
	//private static bool weightStatsPercent;
	private static bool heightPreferred;
	private static bool metersSecondsPreferred;
	private static bool allowFinishRjAfterTime;

	private static Person currentPerson;
	private static Session currentSession;
	private static Jump currentJump;
	private static JumpRj currentJumpRj;
	private static Run currentRun;
	private static RunInterval currentRunInterval;
	private static Pulse currentPulse;
	
	private static EventExecute currentEventExecute;

	//Used by Cancel and Finish
	
	private enum eventType {
		JUMP, RUN, PULSE
	}
	
	private static eventType currentEventIs;
	private static eventType lastEventWas;

	private static bool lastJumpIsReactive; //if last Jump is reactive or not
	private static bool lastRunIsInterval; //if last run is interval or not (obvious) 
	private static JumpType currentJumpType;
	private static RunType currentRunType;
	private static PulseType currentPulseType;
	private static Report report;

	//windows needed
	SessionAddWindow sessionAddWin;
	SessionEditWindow sessionEditWin;
	SessionLoadWindow sessionLoadWin;
	PersonRecuperateWindow personRecuperateWin; 
	PersonsRecuperateFromOtherSessionWindow personsRecuperateFromOtherSessionWin; 
	PersonAddWindow personAddWin; 
	PersonAddMultipleWindow personAddMultipleWin; 
	PersonModifyWindow personModifyWin; 
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

	PulseExtraWindow pulseExtraWin;
	
	ConfirmWindowJumpRun confirmWinJumpRun;	//for deleting jumps and RJ jumps (and runs)
	ErrorWindow errorWin;
	StatsWindow statsWin;
	ReportWindow reportWin;
	
	static EventExecuteWindow eventExecuteWin;

	//platform state variables
	enum States {
		ON,
		OFF
	}
	Chronopic.Plataforma platformState;	//on (in platform), off (jumping), or unknow
	//Chronopic.Respuesta respuesta;		//ok, error, or timeout in calling the platform
	SerialPort sp;
	Chronopic cp;
	bool cpRunning;
	States loggedState;		//log of last state
	private bool firstRjValue;
	private double rjTcCount;
	private double rjTvCount;
	private string rjTcString;
	private string rjTvString;
	
	private bool createdStatsWin;
	
	private bool preferencesLoaded;

	//const int statusbarID = 1;

	public static void Main(string [] args) 
	{
		new ChronoJump(args);
	}

	public ChronoJump (string [] args) 
	{
		//works on Linux
		//Console.WriteLine("lang: {0}", System.Environment.GetEnvironmentVariable("LANG"));
		//Console.WriteLine("language: {0}", System.Environment.GetEnvironmentVariable("LANGUAGE"));
	

		/* SERVER COMMUNICATION TESTS */
		ChronojumpServer myServer = new ChronojumpServer();
		string [] myListDir = myServer.ListDirectory("/home");
		foreach (string myResult in myListDir) 
			Console.WriteLine(myResult);

		Console.WriteLine(myServer.ConnectDatabase());
		Console.WriteLine(myServer.SelectPersonName(3));
		/* END OF SERVER COMMUNICATION TESTS */

		
		Sqlite.Connect();

		//Chech if the DB file exists
		if (!Sqlite.CheckTables()) {
			Console.WriteLine ( Catalog.GetString ("no tables, creating ...") );
			Sqlite.CreateFile();
			Sqlite.CreateTables();
		} else {
			Sqlite.ConvertToLastVersion();
			Console.WriteLine ( Catalog.GetString ("tables already created") ); 
			//check for bad Rjs (activate if program crashes and you use it in the same db before v.0.41)
			//SqliteJump.FindBadRjs();
		}

		//backup the database
		Util.BackupDirCreateIfNeeded();

		Util.BackupDatabase();
		Console.WriteLine ("made a database backup"); //not compressed yet, it seems System.IO.Compression.DeflateStream and
								//System.IO.Compression.GZipStream are not in mono
		
		
		//start as "simulated" if we are on windows
		//(until we improve the Timeout on chronopic)
		//changed: do also in Linux, because there are some problems in the initialization of chronopic (for the radiobutton in the gtk menu)
		//if(Util.IsWindows()) 
			SqlitePreferences.Update("simulated", "True");

			
		//we need to connect sqlite to do the languageChange
		//change language works on windows. On Linux let's change the locale
		if(Util.IsWindows()) 
			languageChange();

		
		Catalog.Init ("chronojump", "./locale");

		Application.Init();

		Util.IsWindows();	//only as additional info here

		
		Glade.XML gxml;
		try {
			//linux
			gxml = Glade.XML.FromAssembly ("chronojump.glade", "app1", null);
		} catch {
			//windows
			gxml = Glade.XML.FromAssembly ("chronojump.glade.chronojump.glade", "app1", null);
		}

		gxml.Autoconnect(this);

		cpRunning = false;

		report = new Report(-1); //when a session is loaded or created, it will change the report.SessionID value
					//TODO: check what happens if a session it's deleted
					//i think report it's deactivated until a new session is created or loaded, 
					//but check what happens if report window is opened


		//preferencesLoaded is a fix to a gtk#-net-windows-bug where radiobuttons raise signals
		//at initialization of chronojump and gives problems if this signals are raised while preferences are loading
		preferencesLoaded = false;
		loadPreferences ();
		preferencesLoaded = true;

		createTreeView_persons(treeview_persons);
		createTreeView_jumps(treeview_jumps);
		createTreeView_jumps_rj(treeview_jumps_rj);
		createTreeView_runs(treeview_runs);
		createTreeView_runs_interval(treeview_runs_interval);
		createTreeView_pulses(treeview_pulses);

		createComboJumps();
		createComboJumpsRj();
		createComboRuns();
		createComboRunsInterval();
		createComboPulses();
		createdStatsWin = false;

		//We have no session, mark some widgets as ".Sensitive = false"
		sensitiveGuiNoSession();

		appbar2.Push ( 1, Catalog.GetString ("Ready.") );

		rand = new Random(40);
				
		
		if(simulated)
			new DialogMessage(Catalog.GetString("Starting Chronojump in Simulated mode, change platform to 'Chronopic' for real detection of events"));
		
		
		Application.Run();
	}

	private void chronopicInit (string myPort)
	{
		Console.WriteLine ( Catalog.GetString ("starting connection with chronopic") );
		Console.WriteLine ( Catalog.GetString ("if program crashes, write to xavi@xdeblas.com") );
		Console.WriteLine ( Catalog.GetString ("If you have previously used the modem via a serial port (in a linux session, and you selected serial port), chronojump will crash.") );
		//Console.WriteLine ( Catalog.GetString ("change variable using 'sqlite ~/.chronojump/chronojump.db' and") );
		//Console.WriteLine ( Catalog.GetString ("'update preferences set value=\"True\" where name=\"simulated\";'") );

	
		bool success = true;
		
		try {
			Console.WriteLine("chronopic port: {0}", myPort);
			sp = new SerialPort(myPort);
			sp.Open();
		
			//-- Create chronopic object, for accessing chronopic
			cp = new Chronopic(sp);

			//on windows, this check make a crash 
			//i think the problem is: as we don't really know the Timeout on Windows (.NET) and this variable is not defined on chronopic.cs
			//the Read_platform comes too much soon (when cp is not totally created), and this makes crash
			if ( ! Util.IsWindows()) {
				//-- Obtener el estado inicial de la plataforma
				bool ok=cp.Read_platform(out platformState);
				if (!ok) {
					//-- Si hay error terminar
					Console.WriteLine("Error: {0}",cp.Error);
					success = false;
				}
			}
		} catch {
			success = false;
		}
			
		if(success) {
			cpRunning = true;
			string myString = string.Format(Catalog.GetString("Connected to Chronopic on port: {0}"), myPort);
			appbar2.Push( 1, myString);
		}
		if(! success) {
			string myString = Catalog.GetString("Problems communicating to chronopic, changed platform to 'Simulated'");
			if(Util.IsWindows())
				myString += Catalog.GetString("\n\nOn Windows we recommend to close/open Chronojump after every unsuccessful port test.");
			new DialogMessage(myString);

			//Console.WriteLine("Problems communicating to chronopic, changed platform to 'Simulated'");
			//TODO: raise a error window
			
			//this will raise on_radiobutton_simulated_ativate and 
			//will put cpRunning to false, and simulated to true and cp.Close()
			menuitem_simulated.Active = true;
			cpRunning = false;
		}
	}
	
	private void loadPreferences () 
	{
		Console.WriteLine (Catalog.GetString("Chronojump database version file: {0}"), 
				SqlitePreferences.Select("databaseVersion") );
		
		chronopicPort = SqlitePreferences.Select("chronopicPort");
		
		prefsDigitsNumber = Convert.ToInt32 ( SqlitePreferences.Select("digitsNumber") );

	
		if ( SqlitePreferences.Select("allowFinishRjAfterTime") == "True" ) 
			allowFinishRjAfterTime = true;
		 else 
			allowFinishRjAfterTime = false;
		
			
		if ( SqlitePreferences.Select("showHeight") == "True" ) 
			showHeight = true;
		 else 
			showHeight = false;
		
			
		if ( SqlitePreferences.Select("showInitialSpeed") == "True" ) 
			showInitialSpeed = true;
		 else 
			showInitialSpeed = false;
		
		
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
			simulated = true;
			menuitem_simulated.Active = true;

			cpRunning = false;
		} else {
			simulated = false;
			menuitem_chronopic.Active = true;
			
			cpRunning = true;
		}
		
		if ( SqlitePreferences.Select("askDeletion") == "True" ) 
			askDeletion = true;
		 else 
			askDeletion = false;
		

		/*
		if ( SqlitePreferences.Select("weightStatsPercent") == "True" ) 
			weightStatsPercent = true;
		 else 
			weightStatsPercent = false;
		
		*/
		
		if ( SqlitePreferences.Select("heightPreferred") == "True" ) 
			heightPreferred = true;
		 else 
			heightPreferred = false;
		
		
		if ( SqlitePreferences.Select("metersSecondsPreferred") == "True" ) 
			metersSecondsPreferred = true;
		 else 
			metersSecondsPreferred = false;
		
	
		//change language works on windows. On Linux let's change the locale
		if(Util.IsWindows())
			languageChange();
			
		//pass to report
		report.PrefsDigitsNumber = prefsDigitsNumber;
		report.HeightPreferred = heightPreferred;
		//report.WeightStatsPercent = weightStatsPercent;
		report.Progversion = progversion;
		
		
		Console.WriteLine ( Catalog.GetString ("Preferences loaded") );
	}

	private void languageChange () {
		string myLanguage = SqlitePreferences.Select("language");
		if ( myLanguage != "0") {
			Console.WriteLine("myLanguage: {0}", myLanguage);
			System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(myLanguage);
			System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(myLanguage);
			//probably only works on newly created windows, if change, then say user has to restart
			Console.WriteLine ("Changed language to {0}", myLanguage );
		}
	}

	/* ---------------------------------------------------------
	 * ----------------  TREEVIEW PERSONS ----------------------
	 *  --------------------------------------------------------
	 */

	private void createTreeView_persons (Gtk.TreeView tv) {
		myTreeViewPersons = new TreeViewPersons( tv );
	}

	private void fillTreeView_persons () {
		string [] myPersons = SqlitePersonSession.SelectCurrentSession(currentSession.UniqueID, false); //not reversed

		if(myPersons.Length > 0) {
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
			string selectedID = (string) model.GetValue (iter, 1); //name, ID
			currentPerson = SqlitePersonSession.PersonSelect(selectedID);
			Console.WriteLine("CurrentPerson: id:{0}, name:{1}", currentPerson.UniqueID, currentPerson.Name);
			return true;
		} else {
			return false;
		}
	}
	
	private void treeview_persons_storeReset() {
		myTreeViewPersons.RemoveColumns();
		myTreeViewPersons = new TreeViewPersons(treeview_persons);
	}
	
	private void on_treeview_persons_cursor_changed (object o, EventArgs args) {
		TreeView tv = (TreeView) o;
		TreeModel model;
		TreeIter iter;

		// you get the iter and the model if something is selected
		if (tv.Selection.GetSelected (out model, out iter)) {
			string selectedID = (string) model.GetValue (iter, 1); //name, ID
		
			Console.WriteLine (selectedID);
			currentPerson = SqlitePersonSession.PersonSelect(selectedID);
			Console.WriteLine("CurrentPerson: id:{0}, name:{1}", currentPerson.UniqueID, currentPerson.Name);
		}
	}

	/* ---------------------------------------------------------
	 * ----------------  TREEVIEW JUMPS ------------------------
	 *  --------------------------------------------------------
	 */

	private void createTreeView_jumps (Gtk.TreeView tv) {
		//myTreeViewJumps is a TreeViewJumps instance
		myTreeViewJumps = new TreeViewJumps( tv, showHeight, showInitialSpeed, showQIndex, showDjIndex, prefsDigitsNumber );
	}

	private void fillTreeView_jumps (string filter) {
		string [] myJumps;
		
		myJumps = SqliteJump.SelectAllNormalJumps(currentSession.UniqueID);
		myTreeViewJumps.Fill(myJumps, filter);
	}

	private void on_button_tv_collapse_clicked (object o, EventArgs args) {
		treeview_jumps.CollapseAll();
	}
	
	private void on_button_tv_expand_clicked (object o, EventArgs args) {
		treeview_jumps.ExpandAll();
	}
	
	private void treeview_jumps_storeReset() {
		myTreeViewJumps.RemoveColumns();
		myTreeViewJumps = new TreeViewJumps( treeview_jumps, showHeight, showInitialSpeed, showQIndex, showDjIndex, prefsDigitsNumber );
	}

	private void on_treeview_jumps_cursor_changed (object o, EventArgs args) {
		// don't select if it's a person, 
		// is for not confusing with the person treeviews that controls who jumps
		if (myTreeViewJumps.EventSelectedID == 0) {
			Console.WriteLine("don't select");
			myTreeViewJumps.Unselect();
		}
	}

	/* ---------------------------------------------------------
	 * ----------------  TREEVIEW JUMPS RJ ---------------------
	 *  --------------------------------------------------------
	 */

	private void createTreeView_jumps_rj (Gtk.TreeView tv) {
		myTreeViewJumpsRj = new TreeViewJumpsRj( tv, showHeight, showInitialSpeed, showQIndex, showDjIndex, prefsDigitsNumber );
	}

	private void fillTreeView_jumps_rj (string filter) {
		string [] myJumps;
		myJumps = SqliteJump.SelectAllRjJumps(currentSession.UniqueID);
		myTreeViewJumpsRj.Fill(myJumps, filter);
	}

	private void on_button_tv_rj_collapse_clicked (object o, EventArgs args) {
		treeview_jumps_rj.CollapseAll();
	}
	
	private void on_button_tv_rj_optimal_clicked (object o, EventArgs args) {
		treeview_jumps_rj.CollapseAll();
		myTreeViewJumpsRj.ExpandOptimal();
	}
	
	private void on_button_tv_rj_expand_clicked (object o, EventArgs args) {
		treeview_jumps_rj.ExpandAll();
	}
	
	private void treeview_jumps_rj_storeReset() {
		myTreeViewJumpsRj.RemoveColumns();
		myTreeViewJumpsRj = new TreeViewJumpsRj( treeview_jumps_rj, showHeight, showInitialSpeed, showQIndex, showDjIndex, prefsDigitsNumber );
	}

	private void on_treeview_jumps_rj_cursor_changed (object o, EventArgs args) {
		// don't select if it's a person, 
		// is for not confusing with the person treeviews that controls who jumps
		if (myTreeViewJumpsRj.EventSelectedID == 0) {
			Console.WriteLine("don't select");
			myTreeViewJumpsRj.Unselect();
		}
	}

	/* ---------------------------------------------------------
	 * ----------------  TREEVIEW RUNS -------------------------
	 *  --------------------------------------------------------
	 */

	private void createTreeView_runs (Gtk.TreeView tv) {
		//myTreeViewRuns is a TreeViewRuns instance
		myTreeViewRuns = new TreeViewRuns( tv, prefsDigitsNumber, metersSecondsPreferred );
	}

	private void fillTreeView_runs (string filter) {
		string [] myRuns = SqliteRun.SelectAllNormalRuns(currentSession.UniqueID);
		myTreeViewRuns.Fill(myRuns, filter);
	}
	
	private void on_button_tv_run_collapse_clicked (object o, EventArgs args) {
		treeview_runs.CollapseAll();
	}
	
	private void on_button_tv_run_expand_clicked (object o, EventArgs args) {
		treeview_runs.ExpandAll();
	}
	
	private void treeview_runs_storeReset() {
		myTreeViewRuns.RemoveColumns();
		myTreeViewRuns = new TreeViewRuns( treeview_runs, prefsDigitsNumber, metersSecondsPreferred );
	}

	private void on_treeview_runs_cursor_changed (object o, EventArgs args) {
		// don't select if it's a person, 
		// is for not confusing with the person treeviews that controls who runs
		if (myTreeViewRuns.EventSelectedID == 0) {
			Console.WriteLine("don't select");
			myTreeViewRuns.Unselect();
		}
	}

	/* ---------------------------------------------------------
	 * ----------------  TREEVIEW RUN INTERVAL -----------------
	 *  --------------------------------------------------------
	 */

	private void createTreeView_runs_interval (Gtk.TreeView tv) {
		//myTreeViewRunsInterval is a TreeViewRunsInterval instance
		myTreeViewRunsInterval = new TreeViewRunsInterval( tv, prefsDigitsNumber, metersSecondsPreferred );
	}

	private void fillTreeView_runs_interval (string filter) {
		string [] myRuns = SqliteRun.SelectAllIntervalRuns(currentSession.UniqueID);
		myTreeViewRunsInterval.Fill(myRuns, filter);
	}
	
	private void on_button_tv_run_interval_collapse_clicked (object o, EventArgs args) {
		treeview_runs_interval.CollapseAll();
	}
	
	private void on_button_tv_run_interval_optimal_clicked (object o, EventArgs args) {
		treeview_runs_interval.CollapseAll();
		myTreeViewRunsInterval.ExpandOptimal();
	}
	
	private void on_button_tv_run_interval_expand_clicked (object o, EventArgs args) {
		treeview_runs_interval.ExpandAll();
	}
	
	private void treeview_runs_interval_storeReset() {
		myTreeViewRunsInterval.RemoveColumns();
		myTreeViewRunsInterval = new TreeViewRunsInterval( treeview_runs_interval,  
				prefsDigitsNumber, metersSecondsPreferred );
	}

	private void on_treeview_runs_interval_cursor_changed (object o, EventArgs args) {
		// don't select if it's a person, 
		// is for not confusing with the person treeviews that controls who runs
		if (myTreeViewRunsInterval.EventSelectedID == 0) {
			Console.WriteLine("don't select");
			myTreeViewRunsInterval.Unselect();
		}
	}

	/* ---------------------------------------------------------
	 * ----------------  TREEVIEW PULSES -----------------------
	 *  --------------------------------------------------------
	 */

	private void createTreeView_pulses (Gtk.TreeView tv) {
		//myTreeViewPulses is a TreeViewPulses instance
		myTreeViewPulses = new TreeViewPulses( tv, prefsDigitsNumber );
	}

	private void fillTreeView_pulses (string filter) {
		string [] myPulses = SqlitePulse.SelectAllPulses(currentSession.UniqueID);
		myTreeViewPulses.Fill(myPulses, filter);
	}
	
	private void on_button_tv_pulse_collapse_clicked (object o, EventArgs args) {
		treeview_pulses.CollapseAll();
	}
	
	private void on_button_tv_pulse_optimal_clicked (object o, EventArgs args) {
		treeview_pulses.CollapseAll();
		myTreeViewPulses.ExpandOptimal();
	}
	
	private void on_button_tv_pulse_expand_clicked (object o, EventArgs args) {
		treeview_pulses.ExpandAll();
	}
	
	private void treeview_pulses_storeReset() {
		myTreeViewPulses.RemoveColumns();
		myTreeViewPulses = new TreeViewPulses( treeview_pulses, prefsDigitsNumber );
	}

	private void on_treeview_pulses_cursor_changed (object o, EventArgs args) {
		// don't select if it's a person, 
		// is for not confusing with the person treeviews that controls who is executing
		if (myTreeViewPulses.EventSelectedID == 0) {
			Console.WriteLine("don't select");
			myTreeViewPulses.Unselect();
		}
	}

	/* ---------------------------------------------------------
	 * ----------------  CREATE AND UPDATE COMBOS ---------------
	 *  --------------------------------------------------------
	 */
	private void createComboJumps() {
		combo_jumps = new Combo ();
		combo_jumps.PopdownStrings = 
			SqliteJumpType.SelectJumpTypes(Constants.AllJumpsName, "", true); //without filter, only select name
		
		combo_jumps.DisableActivate ();
		combo_jumps.Entry.Changed += new EventHandler (on_combo_jumps_changed);

		hbox_combo_jumps.PackStart(combo_jumps, true, true, 0);
		hbox_combo_jumps.ShowAll();
		
		combo_jumps.Sensitive = false;
	}
	
	private void createComboJumpsRj() {
		combo_jumps_rj = new Combo ();
		combo_jumps_rj.PopdownStrings = SqliteJumpType.SelectJumpRjTypes(Constants.AllJumpsName, true); //only select name
		
		combo_jumps_rj.DisableActivate ();
		combo_jumps_rj.Entry.Changed += new EventHandler (on_combo_jumps_rj_changed);

		hbox_combo_jumps_rj.PackStart(combo_jumps_rj, true, true, 0);
		hbox_combo_jumps_rj.ShowAll();
		
		combo_jumps_rj.Sensitive = false;
	}
	
	private void createComboRuns() {
		combo_runs = new Combo ();
		combo_runs.PopdownStrings = 
			SqliteRunType.SelectRunTypes(Constants.AllRunsName, true); //without filter, only select name
		
		combo_runs.DisableActivate ();
		combo_runs.Entry.Changed += new EventHandler (on_combo_runs_changed);

		hbox_combo_runs.PackStart(combo_runs, true, true, 0);
		hbox_combo_runs.ShowAll();
		
		combo_runs.Sensitive = false;
	}

	private void createComboRunsInterval() {
		combo_runs_interval = new Combo ();
		combo_runs_interval.PopdownStrings = 
			SqliteRunType.SelectRunIntervalTypes(Constants.AllRunsName, true); //without filter, only select name
		
		combo_runs_interval.DisableActivate ();
		combo_runs_interval.Entry.Changed += new EventHandler (on_combo_runs_interval_changed);

		hbox_combo_runs_interval.PackStart(combo_runs_interval, true, true, 0);
		hbox_combo_runs_interval.ShowAll();
		
		combo_runs_interval.Sensitive = false;
	}
	
	private void createComboPulses() {
		combo_pulses = new Combo ();
		combo_pulses.PopdownStrings = 
			SqlitePulseType.SelectPulseTypes(Constants.AllPulsesName, true); //without filter, only select name
		
		combo_pulses.DisableActivate ();
		combo_pulses.Entry.Changed += new EventHandler (on_combo_pulses_changed);

		hbox_combo_pulses.PackStart(combo_pulses, true, true, 0);
		hbox_combo_pulses.ShowAll();
		
		combo_pulses.Sensitive = false;
	}


	private void updateComboJumps() {
		combo_jumps.PopdownStrings = 
			SqliteJumpType.SelectJumpTypes(Constants.AllJumpsName, "", true); //without filter, only select name
	}
	
	private void updateComboJumpsRj() {
		combo_jumps_rj.PopdownStrings = 
			SqliteJumpType.SelectJumpRjTypes(Constants.AllJumpsName, true); //only select name
	}
	
	private void updateComboRuns() {
		combo_runs.PopdownStrings = 
			SqliteRunType.SelectRunTypes(Constants.AllRunsName, true); //only select name
	}
	
	private void updateComboRunsInterval() {
		combo_runs_interval.PopdownStrings = 
			SqliteRunType.SelectRunIntervalTypes(Constants.AllRunsName, true); //only select name
	}
	
	private void updateComboPulses() {
		combo_runs.PopdownStrings = 
			SqlitePulseType.SelectPulseTypes(Constants.AllRunsName, true); //only select name
	}
	
	private void on_combo_jumps_changed(object o, EventArgs args) {
		string myText = combo_jumps.Entry.Text;

		//show the edit-delete selected jumps buttons:
		
		menuitem_edit_selected_jump.Sensitive = true;
		menuitem_delete_selected_jump.Sensitive = true;
		button_edit_selected_jump.Sensitive = true;
		button_delete_selected_jump.Sensitive = true;
		
		treeview_jumps_storeReset();
		fillTreeView_jumps(myText);
		
		//expand all rows if a jump filter is selected:
		if (myText != Constants.AllJumpsName)
			treeview_jumps.ExpandAll();
	}
	
	private void on_combo_jumps_rj_changed(object o, EventArgs args) {
		string myText = combo_jumps_rj.Entry.Text;

		//show the edit-delete selected jumps buttons:

		menuitem_edit_selected_jump_rj.Sensitive = true;
		menuitem_delete_selected_jump_rj.Sensitive = true;
		button_edit_selected_jump_rj.Sensitive = true;
		button_delete_selected_jump_rj.Sensitive = true;
		button_repair_selected_reactive_jump.Sensitive = true;
		menuitem_repair_selected_reactive_jump.Sensitive = true;

		treeview_jumps_rj_storeReset();
		fillTreeView_jumps_rj(myText);

		//expand all rows if a jump filter is selected:
		if (myText != Constants.AllJumpsName) 
			myTreeViewJumpsRj.ExpandOptimal();
	}

	private void on_combo_runs_changed(object o, EventArgs args) {
		string myText = combo_runs.Entry.Text;

		//show the edit-delete selected runs buttons:
		menuitem_edit_selected_run.Sensitive = true;
		menuitem_delete_selected_run.Sensitive = true;
		button_edit_selected_run.Sensitive = true;
		button_delete_selected_run.Sensitive = true;

		treeview_runs_storeReset();
		fillTreeView_runs(myText);

		//expand all rows if a runfilter is selected:
		if (myText != Constants.AllRunsName) 
			treeview_runs.ExpandAll();
	}

	private void on_combo_runs_interval_changed(object o, EventArgs args) {
		string myText = combo_runs_interval.Entry.Text;

		//show the edit-delete selected runs buttons:
		menuitem_edit_selected_run_interval.Sensitive = true;
		menuitem_delete_selected_run_interval.Sensitive = true;
		button_edit_selected_run_interval.Sensitive = true;
		button_delete_selected_run_interval.Sensitive = true;

		treeview_runs_interval_storeReset();
		fillTreeView_runs_interval(myText);

		//expand all rows if a runfilter is selected
		if (myText != Constants.AllRunsName) 
			myTreeViewRunsInterval.ExpandOptimal();
	}

	private void on_combo_pulses_changed(object o, EventArgs args) {
		string myText = combo_pulses.Entry.Text;

		//show the edit-delete selected runs buttons:
		//menuitem_edit_selected_run_interval.Sensitive = true;
		//menuitem_delete_selected_run_interval.Sensitive = true;
		button_edit_selected_pulse.Sensitive = true;
		button_delete_selected_pulse.Sensitive = true;

		treeview_pulses_storeReset();
		fillTreeView_pulses(myText);

		//expand all rows if a runfilter is selected
		if (myText != Constants.AllPulsesName) 
			myTreeViewPulses.ExpandOptimal();
	}

	
	/* ---------------------------------------------------------
	 * ----------------  DELETE EVENT, QUIT  -----------------------
	 *  --------------------------------------------------------
	 */
	
	private void on_delete_event (object o, DeleteEventArgs args) {
		Console.WriteLine("Bye!");
    
		if(simulated == false) {
			sp.Close();
		}
		
		Application.Quit();
	}

	private void on_quit1_activate (object o, EventArgs args) {
		Console.WriteLine("Bye!");
    
		if(simulated == false) {
			sp.Close();
		}
		
		Application.Quit();
	}
	
	/* ---------------------------------------------------------
	 * ----------------  SESSION NEW, LOAD, EXPORT, DELETE -----
	 *  --------------------------------------------------------
	 */
	

	private void on_new_activate (object o, EventArgs args) {
		Console.WriteLine("new session");
		sessionAddWin = SessionAddWindow.Show(app1);
		sessionAddWin.Button_accept.Clicked += new EventHandler(on_new_session_accepted);
	}
	
	private void on_new_session_accepted (object o, EventArgs args) {
		if(sessionAddWin.CurrentSession != null) 
		{
			currentSession = sessionAddWin.CurrentSession;
			app1.Title = progname + " - " + currentSession.Name;

			if(createdStatsWin) {
				statsWin.InitializeSession(currentSession);
			}
			
			//load the persons treeview
			treeview_persons_storeReset();
			fillTreeView_persons();
			
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


			//show hidden widgets
			sensitiveGuiNoSession();
			sensitiveGuiYesSession();

			//for sure, jumpsExists is false, because we create a new session

			button_edit_current_person.Sensitive = false;
			menuitem_edit_current_person.Sensitive = false;
			menuitem_delete_current_person_from_session.Sensitive = false;
			button_show_all_person_events.Sensitive = false;
			show_all_person_events.Sensitive = false;
		
			//update report
			report.SessionID = currentSession.UniqueID;
		}
	}
	
	private void on_edit_session_activate (object o, EventArgs args) {
		Console.WriteLine("edit session");
		sessionEditWin = SessionEditWindow.Show(app1, currentSession);
		sessionEditWin.Button_accept.Clicked += new EventHandler(on_edit_session_accepted);
	}
	
	private void on_edit_session_accepted (object o, EventArgs args) {
		if(sessionEditWin.CurrentSession != null) 
		{
			currentSession = sessionEditWin.CurrentSession;
			app1.Title = progname + " - " + currentSession.Name;

			if(createdStatsWin) {
				statsWin.InitializeSession(currentSession);
			}
		}
	}
	
	private void on_open_activate (object o, EventArgs args) {
		Console.WriteLine("open session");
		sessionLoadWin = SessionLoadWindow.Show(app1);
		sessionLoadWin.Button_accept.Clicked += new EventHandler(on_load_session_accepted);
	}
	
	private void on_load_session_accepted (object o, EventArgs args) {
		currentSession = sessionLoadWin.CurrentSession;
		app1.Title = progname + " - " + currentSession.Name;
		
		if(createdStatsWin) {
			statsWin.InitializeSession(currentSession);
		}
		
		//load the persons treeview (and try to select first)
		treeview_persons_storeReset();
		fillTreeView_persons();
		bool foundPersons = selectRowTreeView_persons(treeview_persons, treeview_persons_store, 0);
		
		//load the treeview_jumps
		treeview_jumps_storeReset();
		fillTreeView_jumps(Constants.AllJumpsName);
		
		//load the treeview_jumps_rj
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
		

		//show hidden widgets
		sensitiveGuiNoSession();
		sensitiveGuiYesSession();
		
		button_edit_current_person.Sensitive = false;
		menuitem_edit_current_person.Sensitive = false;
		menuitem_delete_current_person_from_session.Sensitive = false;
		button_show_all_person_events.Sensitive = false;
		show_all_person_events.Sensitive = false;

		//if there are persons
		if(foundPersons) {
			//activate the gui for persons in main window
			sensitiveGuiYesPerson();
		}

		//update report
		report.SessionID = currentSession.UniqueID;
	}
	
	
	private void on_delete_session_activate (object o, EventArgs args) {
		Console.WriteLine("delete session");
		ConfirmWindow confirmWin = ConfirmWindow.Show(app1, Catalog.GetString("Are you sure you want to delete the current session"), Catalog.GetString("and all the session events?"));
		confirmWin.Button_accept.Clicked += new EventHandler(on_delete_session_accepted);
	}
	
	private void on_delete_session_accepted (object o, EventArgs args) 
	{
		appbar2.Push( 1, Catalog.GetString("Deleted session and all its events") );
		SqliteSession.DeleteWithJumps(currentSession.UniqueID.ToString());
		
		sensitiveGuiNoSession();
		app1.Title = progname + "";
	}

	
	private void on_export_session_activate(object o, EventArgs args) {
		if (o == (object) menuitem_export_csv) {
			new ExportSessionCSV(currentSession, app1, appbar2);
		} else if (o == (object) menuitem_export_xml) {
			new ExportSessionXML(currentSession, app1, appbar2);
		} else {
			Console.WriteLine("Error exporting");
		}
	}

	
	/* ---------------------------------------------------------
	 * ----------------  PERSON RECUPERATE, LOAD, EDIT, DELETE -
	 *  --------------------------------------------------------
	 */
	
	private void on_recuperate_person_activate (object o, EventArgs args) {
		Console.WriteLine("recuperate person");
		personRecuperateWin = PersonRecuperateWindow.Show(app1, currentSession.UniqueID);
		personRecuperateWin.Button_recuperate.Clicked += new EventHandler(on_recuperate_person_accepted);
	}

	private void on_recuperate_person_accepted (object o, EventArgs args) {
		currentPerson = personRecuperateWin.CurrentPerson;
		
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
		Console.WriteLine("recuperate persons from other session");
		personsRecuperateFromOtherSessionWin = PersonsRecuperateFromOtherSessionWindow.Show(app1, currentSession.UniqueID);
		personsRecuperateFromOtherSessionWin.Button_recuperate.Clicked += new EventHandler(on_recuperate_persons_from_session_accepted);
	}
	
	private void on_recuperate_persons_from_session_accepted (object o, EventArgs args) {
		currentPerson = personsRecuperateFromOtherSessionWin.CurrentPerson;
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
		personAddWin = PersonAddWindow.Show(app1, currentSession.UniqueID);
		personAddWin.Button_accept.Clicked += new EventHandler(on_person_add_single_accepted);
	}
	
	private void on_person_add_single_accepted (object o, EventArgs args) {
		if (personAddWin.CurrentPerson != null)
		{
			currentPerson = personAddWin.CurrentPerson;
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
	
	private void on_person_add_multiple_activate (object o, EventArgs args) {
		personAddMultipleWin = PersonAddMultipleWindow.Show(app1, currentSession.UniqueID);
		personAddMultipleWin.Button_accept.Clicked += new EventHandler(on_person_add_multiple_accepted);
	}
	
	private void on_person_add_multiple_accepted (object o, EventArgs args) {
		if (personAddMultipleWin.CurrentPerson != null)
		{
			currentPerson = personAddMultipleWin.CurrentPerson;
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
		Console.WriteLine("modify person");
		personModifyWin = PersonModifyWindow.Show(app1, currentSession.UniqueID, currentPerson.UniqueID);
		personModifyWin.Button_accept.Clicked += new EventHandler(on_edit_current_person_accepted);
	}
	
	private void on_edit_current_person_accepted (object o, EventArgs args) {
		if (personModifyWin.CurrentPerson != null)
		{
			currentPerson = personModifyWin.CurrentPerson;
			treeview_persons_storeReset();
			fillTreeView_persons();
			int rowToSelect = findRowOfCurrentPerson(treeview_persons, treeview_persons_store, currentPerson);
			if(rowToSelect != -1) {
				selectRowTreeView_persons(treeview_persons,
						treeview_persons_store, 
						rowToSelect);
				sensitiveGuiYesPerson();
			}

			treeview_jumps_storeReset();
			string myText = combo_jumps.Entry.Text;
			fillTreeView_jumps(myText);
			
			//load the treeview_rj
			treeview_jumps_rj_storeReset();
			myText = combo_jumps_rj.Entry.Text;
			fillTreeView_jumps_rj(myText);

			if(createdStatsWin) {
				statsWin.FillTreeView_stats(false, true);
			}
		}
	}
	
	private void on_show_all_person_events_activate (object o, EventArgs args) {
		personShowAllEventsWin = PersonShowAllEventsWindow.Show(app1, currentSession.UniqueID, currentPerson);
	}
	
	
	private void on_delete_current_person_from_session_activate (object o, EventArgs args) {
		Console.WriteLine("delete current person from this session");
		ConfirmWindow confirmWin = ConfirmWindow.Show(app1, 
				Catalog.GetString("Are you sure you want to delete the current person and all his/her events (jumps, runs, pulses) from this session?\n(His/her personal data and events in other sessions will remain intact)"), 
				Catalog.GetString("Current Person: ") + currentPerson.Name);

		confirmWin.Button_accept.Clicked += new EventHandler(on_delete_current_person_from_session_accepted);
	}
	
	private void on_delete_current_person_from_session_accepted (object o, EventArgs args) 
	{
		appbar2.Push( 1, Catalog.GetString("Deleted person and all his/her events on this session") );
		SqlitePersonSession.DeletePersonFromSessionAndJumps(
				currentSession.UniqueID.ToString(), currentPerson.UniqueID.ToString());
		
		treeview_persons_storeReset();
		fillTreeView_persons();
		bool foundPersons = selectRowTreeView_persons(treeview_persons, treeview_persons_store, 0);
		
		treeview_jumps_storeReset();
		fillTreeView_jumps(Constants.AllJumpsName);
		
		treeview_jumps_rj_storeReset();
		fillTreeView_jumps_rj(Constants.AllJumpsName);
			
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
				//prefsDigitsNumber, weightStatsPercent, heightPreferred, 
				prefsDigitsNumber, heightPreferred, 
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

	void on_radiobutton_simulated (object o, EventArgs args)
	{
		Console.WriteLine("RAD - simul. cpRunning: {0}", cpRunning);
		if(menuitem_simulated.Active) {
			Console.WriteLine("RadioSimulated - ACTIVE");
			simulated = true;
			SqlitePreferences.Update("simulated", simulated.ToString());

			//close connection with chronopic if initialized
			if(cpRunning) {
				sp.Close();
			}
			cpRunning = false;
		}
		else
			Console.WriteLine("RadioSimulated - INACTIVE");
	}
	
	void on_radiobutton_chronopic (object o, EventArgs args)
	{
		Console.WriteLine("RAD - chrono. cpRunning: {0}", cpRunning);
		if(! preferencesLoaded)
			return;

		if(! menuitem_chronopic.Active) {
			Console.WriteLine("RadioChronopic - INACTIVE");
			return;
		}

		Console.WriteLine("RadioChronopic - ACTIVE");
	
		//on windows currently there's no timeout on init of chronopic
		//show this window, and start chronopic only when button_accept is clicjed
		if(Util.IsWindows()) {
			ConfirmWindow confirmWin = ConfirmWindow.Show(app1, Catalog.GetString("** Attention **: generate a event with the platform or with chronopic.\nIf you don't do it, Chronojump will crash.\n"), Catalog.GetString("If it crashes, try to close it and open again, then Chronojump will be configured as simulated, and you can change the port in the preferences window"));
			confirmWin.Button_accept.Clicked += new EventHandler(on_chronopic_accepted);
			confirmWin.Button_cancel.Clicked += new EventHandler(on_chronopic_cancelled);
		} else {
			simulated = false;
			SqlitePreferences.Update("simulated", simulated.ToString());

			//init connecting with chronopic	
			if(cpRunning == false) {
				chronopicInit(chronopicPort);
				//cpRunning = true;
			}
		}
	}

	private void on_chronopic_accepted (object o, EventArgs args) {
		simulated = false;
		SqlitePreferences.Update("simulated", simulated.ToString());
		
		//init connecting with chronopic	
		if(cpRunning == false) {
			chronopicInit(chronopicPort);
			//cpRunning = true;
		}
	}

	private void on_chronopic_cancelled (object o, EventArgs args) {
		menuitem_chronopic.Active = false;
		menuitem_simulated.Active = true;
	}
	

	private void on_preferences_activate (object o, EventArgs args) {
		PreferencesWindow myWin = PreferencesWindow.Show(
				app1, chronopicPort, prefsDigitsNumber, showHeight, showInitialSpeed, showQIndex, showDjIndex, 
				askDeletion, heightPreferred, metersSecondsPreferred,
				System.Threading.Thread.CurrentThread.CurrentUICulture.ToString(),
				allowFinishRjAfterTime);
		myWin.Button_accept.Clicked += new EventHandler(on_preferences_accepted);
	}

	private void on_preferences_accepted (object o, EventArgs args) {
		prefsDigitsNumber = Convert.ToInt32 ( SqlitePreferences.Select("digitsNumber") ); 

		string myPort = SqlitePreferences.Select("chronopicPort");
		if(myPort != chronopicPort && cpRunning) {
			chronopicInit (myPort);
		}
		chronopicPort = myPort;
	
		
		if ( SqlitePreferences.Select("askDeletion") == "True" ) 
			askDeletion = true;
		 else 
			askDeletion = false;
		
	
		/*
		if ( SqlitePreferences.Select("weightStatsPercent") == "True" ) 
			weightStatsPercent = true;
		 else 
			weightStatsPercent = false;
		
		*/

		//update showHeight
		if ( SqlitePreferences.Select("showHeight") == "True" ) 
			showHeight = true;
		 else 
			showHeight = false;
		

		//update showInitialSpeed
		if ( SqlitePreferences.Select("showInitialSpeed") == "True" ) 
			showInitialSpeed = true;
		 else 
			showInitialSpeed = false;
		

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
		if(Util.IsWindows()) 
			languageChange();
	

		//this will crash if currentSession is not created/loaded, then go to catch
		try {
			//... and recreate the treeview_jumps
			string myText = combo_jumps.Entry.Text;
			createTreeView_jumps (treeview_jumps);
			treeview_jumps_storeReset();
			fillTreeView_jumps(myText);

			//... and recreate the treeview_jumps_rj
			myText = combo_jumps.Entry.Text;
			createTreeView_jumps_rj (treeview_jumps_rj);
			treeview_jumps_rj_storeReset();
			fillTreeView_jumps_rj(myText);

			//... and recreate the treeview_runs
			myText = combo_runs.Entry.Text;
			createTreeView_runs (treeview_runs);
			treeview_runs_storeReset();
			fillTreeView_runs(myText);

			//... and recreate the treeview_runs_interval
			myText = combo_runs_interval.Entry.Text;
			createTreeView_runs_interval (treeview_runs_interval);
			treeview_runs_interval_storeReset();
			fillTreeView_runs_interval(myText);
		}
		catch 
		{
			if(createdStatsWin) {
				statsWin.PrefsDigitsNumber = prefsDigitsNumber;
				//statsWin.WeightStatsPercent = weightStatsPercent;
				statsWin.HeightPreferred = heightPreferred;

				statsWin.FillTreeView_stats(false, true);
			}

			//pass to report
			report.PrefsDigitsNumber = prefsDigitsNumber;
			report.HeightPreferred = heightPreferred;
			//report.WeightStatsPercent = weightStatsPercent;
		}
	}
	
	private void on_cancel_clicked (object o, EventArgs args) 
	{
		//this will cancel jumps or runs
		currentEventExecute.Cancel = true;

		//unhide event buttons for next event
		sensitiveGuiEventDone();
	}
		
	private void on_finish_clicked (object o, EventArgs args) 
	{
		currentEventExecute.Finish = true;
		
		//unhide event buttons for next event
		sensitiveGuiEventDone();
	}
		
	
	private void on_show_report_activate (object o, EventArgs args) {
		Console.WriteLine("open report window");
		reportWin = ReportWindow.Show(app1, report);
	}
	

	/* ---------------------------------------------------------
	 * ----------------  JUMPS EXECUTION (no RJ) ----------------
	 *  --------------------------------------------------------
	 */

	private void on_button_more_clicked (object o, EventArgs args) 
	{
		jumpsMoreWin = JumpsMoreWindow.Show(app1);
		jumpsMoreWin.Button_accept.Clicked += new EventHandler(on_more_jumps_accepted);
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
				jumpsMoreWin.SelectedJumpType,
				jumpsMoreWin.SelectedStartIn,
				jumpsMoreWin.SelectedExtraWeight,
				false,		//isRepetitive
				false,		//jumpsLimited (false, because is not repetitive)
				0,		//limitValue
				false		//unlimited
				);
				
		if( ! currentJumpType.StartIn || currentJumpType.HasWeight) {
			on_jump_extra_activate(o, args);
		} else {
			on_normal_jump_activate(o, args);
		}
	}
	
	//here comes the SJ+, DJ and every jump that has weight or fall or both. Also the reactive jumps (for defining is limited value or weight or fall)
	private void on_jump_extra_activate (object o, EventArgs args) 
	{
		Console.WriteLine("jump extra");
		if(o == (object) button_sj_plus || o == (object) sj_plus) {
			currentJumpType = new JumpType("SJ+");
		} else if(o == (object) button_dj || o == (object) dj) {
			currentJumpType = new JumpType("DJ");
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
		if(o == (object) button_sj || o == (object) sj) {
			currentJumpType = new JumpType("SJ");
		} else if (o == (object) button_cmj || o == (object) cmj) {
			currentJumpType = new JumpType("CMJ");
		} else if (o == (object) button_abk || o == (object) abk) {
			currentJumpType = new JumpType("ABK");
		} else {
		}
			
		double jumpWeight = 0;
		if(currentJumpType.HasWeight) {
			if(jumpExtraWin.Option == "%") {
				jumpWeight = jumpExtraWin.Weight;
			} else {
				jumpWeight = jumpExtraWin.Weight *100 / currentPerson.Weight;
			}
		}
		int myFall = 0;
		if( ! currentJumpType.StartIn ) {
			myFall = jumpExtraWin.Fall;
		}
			
		//used by cancel and finish
		currentEventIs = eventType.JUMP;
			
		//hide jumping buttons
		sensitiveGuiEventDoing();

		//change to page 0 of notebook if were in other
		notebook_change(0);
		
		//show the event doing window
		double myLimit = 3; //3 phases for show the Dj
		if( currentJumpType.StartIn )
			myLimit = 2; //2 for normal jump
			
		eventExecuteWin = EventExecuteWindow.Show(
			Catalog.GetString("Execute Jump"), //windowTitle
			Catalog.GetString("Phases"),  	  //name of the different moments
			currentPerson.UniqueID, currentPerson.Name, 
			currentSession.UniqueID, 
			"jump", //tableName
			currentJumpType.Name, 
			prefsDigitsNumber, myLimit, simulated);

		eventExecuteWin.ButtonCancel.Clicked += new EventHandler(on_cancel_clicked);
		eventExecuteWin.ButtonFinish.Clicked += new EventHandler(on_finish_clicked);

		//when user clicks on update the eventExecute window 
		//(for showing with his new confgured values: max, min and guides
		eventExecuteWin.ButtonUpdate.Clicked -= new EventHandler(on_update_clicked); //if we don't do this, on_update_clicked it's called 'n' times when 'n' events are don
		eventExecuteWin.ButtonUpdate.Clicked += new EventHandler(on_update_clicked);

		currentEventExecute = new JumpExecute(eventExecuteWin, currentPerson.UniqueID, currentPerson.Name, 
				currentSession.UniqueID, currentJumpType.Name, myFall, jumpWeight,
				cp, appbar2, app1, prefsDigitsNumber);

		if (simulated) 
			currentEventExecute.SimulateInitValues(rand);
		
		if( currentJumpType.StartIn ) 
			currentEventExecute.Manage();
		 else 
			currentEventExecute.ManageFall();

		currentEventExecute.FakeButtonFinished.Clicked += new EventHandler(on_jump_finished);
	}	

	/*
	 * update button is clicked on eventWindow, chronojump.cs delegate points here
	 */
	
	private void on_update_clicked (object o, EventArgs args) {
		Console.WriteLine("--On_update_clicked--");
		try {
			switch (currentEventIs) {
				case eventType.JUMP:
					if(currentJumpType.IsRepetitive) 
						eventExecuteWin.PrepareJumpReactiveGraph(
								Util.GetLast(currentJumpRj.TvString), Util.GetLast(currentJumpRj.TcString),
								currentJumpRj.TvString, currentJumpRj.TcString);
					else 
						eventExecuteWin.PrepareJumpSimpleGraph(currentJump.Tv, currentJump.Tc);
					break;
				case eventType.RUN:
					if(currentRunType.HasIntervals) 
						eventExecuteWin.PrepareRunIntervalGraph(currentRunInterval.DistanceInterval, 
								Util.GetLast(currentRunInterval.IntervalTimesString), currentRunInterval.IntervalTimesString);
					else
						eventExecuteWin.PrepareRunSimpleGraph(currentRun.Time, currentRun.Speed);
					break;
				case eventType.PULSE:
					eventExecuteWin.PreparePulseGraph(Util.GetLast(currentPulse.TimesString), currentPulse.TimesString);
					break;
			}
		}
		catch {
			errorWin = ErrorWindow.Show(app1, Catalog.GetString("Cannot update. Probably this event was deleted."));
		}
	
	}

	
	
	private void on_jump_finished (object o, EventArgs args)
	{
		currentEventExecute.FakeButtonFinished.Clicked -= new EventHandler(on_jump_finished);
		
		if ( ! currentEventExecute.Cancel ) {
			lastEventWas = eventType.JUMP;
			lastJumpIsReactive = false;

			currentJump = (Jump) currentEventExecute.EventDone;
			
			myTreeViewJumps.Add(currentPerson.Name, currentJump);
		
			if(createdStatsWin) {
				statsWin.FillTreeView_stats(false, false);
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
		jumpsRjMoreWin = JumpsRjMoreWindow.Show(app1);
		jumpsRjMoreWin.Button_accept.Clicked += new EventHandler(on_more_jumps_rj_accepted);
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
				jumpsRjMoreWin.SelectedJumpType,
				jumpsRjMoreWin.SelectedStartIn,
				jumpsRjMoreWin.SelectedExtraWeight,
				true,		//isRepetitive
				jumpsRjMoreWin.SelectedLimited,
				jumpsRjMoreWin.SelectedLimitedValue,
				jumpsRjMoreWin.SelectedUnlimited
				);
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
		}
	}
	private void on_rj_accepted (object o, EventArgs args) 
	{
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
				jumpWeight = jumpExtraWin.Weight *100 / currentPerson.Weight;
			}
		}
		int myFall = 0;
		if( ! currentJumpType.StartIn ) {
			myFall = jumpExtraWin.Fall;
		}

		//used by cancel and finish
		currentEventIs = eventType.JUMP;
			
		//hide jumping buttons
		sensitiveGuiEventDoing();
	
		//change to page 1 of notebook if were in other
		notebook_change(1);
		
		//show the event doing window
		eventExecuteWin = EventExecuteWindow.Show(
			Catalog.GetString("Execute Reactive Jump"), //windowTitle
			Catalog.GetString("Jumps"),  	  //name of the different moments
			currentPerson.UniqueID, currentPerson.Name, 
			currentSession.UniqueID, 
			"jumpRj", //tableName
			currentJumpType.Name, 
			prefsDigitsNumber, myLimit, simulated);
		
		eventExecuteWin.ButtonCancel.Clicked += new EventHandler(on_cancel_clicked);
		eventExecuteWin.ButtonFinish.Clicked += new EventHandler(on_finish_clicked);
		
		//when user clicks on update the eventExecute window 
		//(for showing with his new configured values: max, min and guides
		eventExecuteWin.ButtonUpdate.Clicked -= new EventHandler(on_update_clicked); //if we don't do this, on_update_clicked it's called 'n' times when 'n' events are don
		eventExecuteWin.ButtonUpdate.Clicked += new EventHandler(on_update_clicked);
	
		currentEventExecute = new JumpRjExecute(eventExecuteWin, currentPerson.UniqueID, currentPerson.Name, 
				currentSession.UniqueID, currentJumpType.Name, myFall, jumpWeight, 
				myLimit, currentJumpType.JumpsLimited, 
				cp, appbar2, app1, prefsDigitsNumber, allowFinishRjAfterTime);
		
		
		//suitable for limited by jump and time
		//simulated always simulate limited by jumps
		if(simulated) 
			currentEventExecute.SimulateInitValues(rand);
		
		currentEventExecute.Manage();
		currentEventExecute.FakeButtonFinished.Clicked += new EventHandler(on_jump_rj_finished);
	}
		
	private void on_jump_rj_finished (object o, EventArgs args) 
	{
		Console.WriteLine("ON JUMP RJ FINISHED");
		
		currentEventExecute.FakeButtonFinished.Clicked -= new EventHandler(on_jump_rj_finished);
		
		if ( ! currentEventExecute.Cancel ) {
			lastEventWas = eventType.JUMP;
			lastJumpIsReactive = true;

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

			myTreeViewJumpsRj.Add(currentPerson.Name, currentJumpRj);

			if(createdStatsWin) {
				statsWin.FillTreeView_stats(false, false);
			}

			//unhide buttons for delete last jump
			sensitiveGuiYesEvent();

			//put correct time value in eventWindow (put the time from chronopic and not onTimer soft chronometer)
			eventExecuteWin.LabelTimeValue = Util.GetTotalTime(currentJumpRj.TcString, currentJumpRj.TvString);
			//possible deletion of last jump can make the jumps on event window be false
			eventExecuteWin.LabelEventValue = currentJumpRj.Jumps;
		}
		
		//unhide buttons that allow jumping
		sensitiveGuiEventDone();
	}

	/* ---------------------------------------------------------
	 * ----------------  RUNS EXECUTION (no interval) ----------
	 *  --------------------------------------------------------
	 */
	
	private void on_button_run_more_clicked (object o, EventArgs args) 
	{
		runsMoreWin = RunsMoreWindow.Show(app1);
		runsMoreWin.Button_accept.Clicked += new EventHandler(on_more_runs_accepted);
	}
	
	private void on_button_run_last_clicked (object o, EventArgs args) 
	{
		Console.WriteLine("button run last");
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
				runsMoreWin.SelectedRunType,	//name
				false,				//hasIntervals
				runsMoreWin.SelectedDistance,	//distance
				false,				//tracksLimited (false, because has not intervals)
				0,				//fixedValue (0, because has not intervals)
				false				//unlimited (false, because has not intervals)
				);
				
		if( currentRunType.Distance == 0 ) {
			on_run_extra_activate(o, args);
		} else {
			on_normal_run_activate(o, args);
		}
	}
	
	//here comes the unlimited runs (and every run with distance = 0 (undefined)
	private void on_run_extra_activate (object o, EventArgs args) 
	{
		Console.WriteLine("run extra");
	
		if(o == (object) button_run_custom || o == (object) menuitem_run_custom) {
			currentRunType = new RunType("Custom");
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
		} else if (o == (object) button_run_1000m || o == (object) menuitem_1000m) {
			currentRunType = new RunType("1000m");
		} else if (o == (object) button_run_2000m || o == (object) menuitem_2000m) {
			currentRunType = new RunType("2000m");
		}
		// add others...
		
		//if distance can be always different in this run,
		//show values selected in runExtraWin
		int myDistance = 0;		
		if(currentRunType.Distance == 0) {
			myDistance = runExtraWin.Distance;
		} else {
			myDistance = (int) currentRunType.Distance;
		}
		
		//used by cancel and finish
		currentEventIs = eventType.RUN;
			
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
		
		eventExecuteWin = EventExecuteWindow.Show(
			Catalog.GetString("Execute Run"), //windowTitle
			Catalog.GetString("Phases"),  	  //name of the different moments
			currentPerson.UniqueID, currentPerson.Name, 
			currentSession.UniqueID, 
			"run", //tableName
			currentRunType.Name, 
			prefsDigitsNumber, myLimit, simulated);
		
		eventExecuteWin.ButtonCancel.Clicked += new EventHandler(on_cancel_clicked);

		eventExecuteWin.ButtonFinish.Clicked += new EventHandler(on_finish_clicked);


		//when user clicks on update the eventExecute window 
		//(for showing with his new confgured values: max, min and guides
		eventExecuteWin.ButtonUpdate.Clicked -= new EventHandler(on_update_clicked); //if we don't do this, on_update_clicked it's called 'n' times when 'n' events are don
		eventExecuteWin.ButtonUpdate.Clicked += new EventHandler(on_update_clicked);


		currentEventExecute = new RunExecute(eventExecuteWin, currentPerson.UniqueID, currentSession.UniqueID, 
				currentRunType.Name, myDistance, 
				cp, appbar2, app1, prefsDigitsNumber, metersSecondsPreferred);
		
		if (simulated) 
			currentEventExecute.SimulateInitValues(rand);
			
		currentEventExecute.Manage();
		currentEventExecute.FakeButtonFinished.Clicked += new EventHandler(on_run_finished);
	}
	
	private void on_run_finished (object o, EventArgs args)
	{
		currentEventExecute.FakeButtonFinished.Clicked -= new EventHandler(on_run_finished);
		
		if ( ! currentEventExecute.Cancel ) {
			lastEventWas = eventType.RUN;
			lastRunIsInterval = false;
			
			currentRun = (Run) currentEventExecute.EventDone;

			myTreeViewRuns.Add(currentPerson.Name, currentRun);
		
			if(createdStatsWin) {
				statsWin.FillTreeView_stats(false, false);
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
		runsIntervalMoreWin = RunsIntervalMoreWindow.Show(app1);
		runsIntervalMoreWin.Button_accept.Clicked += new EventHandler(on_more_runs_interval_accepted);
	}
	
	private void on_more_runs_interval_accepted (object o, EventArgs args) 
	{
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
	}
	
	private void on_button_run_interval_last_clicked (object o, EventArgs args) 
	{
		on_run_interval_activate(o, args);
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
		}
		
			
		runExtraWin = RunExtraWindow.Show(app1, currentRunType);
		runExtraWin.Button_accept.Clicked += new EventHandler(on_run_interval_accepted);
	}
	
	private void on_run_interval_accepted (object o, EventArgs args)
	{
		Console.WriteLine("run interval accepted");
		
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
		currentEventIs = eventType.RUN;
			
		//hide running buttons
		sensitiveGuiEventDoing();
		
		//change to page 3 of notebook if were in other
		notebook_change(3);
		
		//show the event doing window
		eventExecuteWin = EventExecuteWindow.Show(
			Catalog.GetString("Execute Intervallic Run"), //windowTitle
			Catalog.GetString("Tracks"),  	  //name of the different moments
			currentPerson.UniqueID, currentPerson.Name, 
			currentSession.UniqueID, 
			"runInterval", //tableName
			currentRunType.Name, 
			prefsDigitsNumber, myLimit, simulated);

		eventExecuteWin.ButtonCancel.Clicked += new EventHandler(on_cancel_clicked);
		eventExecuteWin.ButtonFinish.Clicked += new EventHandler(on_finish_clicked);

		//when user clicks on update the eventExecute window 
		//(for showing with his new confgured values: max, min and guides
		eventExecuteWin.ButtonUpdate.Clicked -= new EventHandler(on_update_clicked); //if we don't do this, on_update_clicked it's called 'n' times when 'n' events are don
		eventExecuteWin.ButtonUpdate.Clicked += new EventHandler(on_update_clicked);
	
		currentEventExecute = new RunIntervalExecute(eventExecuteWin, currentPerson.UniqueID, currentSession.UniqueID, currentRunType.Name, 
				distanceInterval, myLimit, currentRunType.TracksLimited, 
				cp, appbar2, app1, prefsDigitsNumber);
		
		
		//suitable for limited by tracks and time
		if(simulated)
			currentEventExecute.SimulateInitValues(rand);
			
		currentEventExecute.Manage();
		currentEventExecute.FakeButtonFinished.Clicked += new EventHandler(on_run_interval_finished);
	}

	private void on_run_interval_finished (object o, EventArgs args) 
	{
		currentEventExecute.FakeButtonFinished.Clicked -= new EventHandler(on_run_interval_finished);
		
		if ( ! currentEventExecute.Cancel ) {
			lastEventWas = eventType.RUN;
			lastRunIsInterval = true;

			currentRunInterval = (RunInterval) currentEventExecute.EventDone;

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

			if(createdStatsWin) {
				statsWin.FillTreeView_stats(false, false);
			}

			//unhide buttons for delete last jump
			sensitiveGuiYesEvent();

			//put correct time value in eventWindow (put the time from chronopic and not onTimer soft chronometer)
			eventExecuteWin.LabelTimeValue = currentRunInterval.TimeTotal;
			//possible deletion of last run can make the runs on event window be false
			eventExecuteWin.LabelEventValue = currentRunInterval.Tracks;
		}
		
		//unhide buttons that allow jumping, running
		sensitiveGuiEventDone();
	}


	/* ---------------------------------------------------------
	 * ----------------  PULSES EXECUTION ----------- ----------
	 *  --------------------------------------------------------
	 */

	private void on_button_pulse_more_clicked (object o, EventArgs args) 
	{
		appbar2.Push ( 1, "pulse more (NOT IMPLEMENTED YET)");
		/*
		runsIntervalMoreWin = RunsIntervalMoreWindow.Show(app1);
		runsIntervalMoreWin.Button_accept.Clicked += new EventHandler(on_more_runs_interval_accepted);
		*/
	}
	
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
	
	private void on_button_pulse_last_clicked (object o, EventArgs args) 
	{
		appbar2.Push ( 1, "pulse last (NOT IMPLEMENTED YET)");
		/*
		on_run_interval_activate(o, args);
		*/
	}
	
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
		Console.WriteLine("pulse accepted");
	
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
		currentEventIs = eventType.PULSE;
			
		//hide pulse buttons
		sensitiveGuiEventDoing();
		
		//change to page 4 of notebook if were in other
		notebook_change(4);
		
		//show the event doing window
		eventExecuteWin = EventExecuteWindow.Show(
			Catalog.GetString("Execute Pulse"), //windowTitle
			Catalog.GetString("Pulses"),  	  //name of the different moments
			currentPerson.UniqueID, currentPerson.Name, 
			currentSession.UniqueID, 
			"pulse", //tableName
			currentPulseType.Name, 
			prefsDigitsNumber, totalPulses, simulated);

		eventExecuteWin.ButtonCancel.Clicked += new EventHandler(on_cancel_clicked);
		eventExecuteWin.ButtonFinish.Clicked += new EventHandler(on_finish_clicked);
		
		//when user clicks on update the eventExecute window 
		//(for showing with his new confgured values: max, min and guides
		eventExecuteWin.ButtonUpdate.Clicked -= new EventHandler(on_update_clicked); //if we don't do this, on_update_clicked it's called 'n' times when 'n' events are don
		eventExecuteWin.ButtonUpdate.Clicked += new EventHandler(on_update_clicked);

		currentEventExecute = new PulseExecute(eventExecuteWin, currentPerson.UniqueID, currentPerson.Name, 
				currentSession.UniqueID, currentPulseType.Name, pulseStep, totalPulses, 
				cp, appbar2, app1, prefsDigitsNumber);
		
		if(simulated)	
			currentEventExecute.SimulateInitValues(rand);
		
		currentEventExecute.Manage();
		currentEventExecute.FakeButtonFinished.Clicked += new EventHandler(on_pulse_finished);
	}

	private void on_pulse_finished (object o, EventArgs args) 
	{
		Console.WriteLine("pulse finished");
		
		currentEventExecute.FakeButtonFinished.Clicked -= new EventHandler(on_pulse_finished);
		
		if ( ! currentEventExecute.Cancel ) {
			lastEventWas = eventType.PULSE;
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
			
			/*
			if(createdStatsWin) {
				statsWin.FillTreeView_stats(false, false);
			}
			*/
			
			//unhide buttons for delete last jump
			sensitiveGuiYesEvent();

			//put correct time value in eventWindow (put the time from chronopic and not onTimer soft chronometer)
			eventExecuteWin.LabelTimeValue = Util.GetTotalTime(currentPulse.TimesString);
		}
		
		//unhide buttons that allow jumping, running
		sensitiveGuiEventDone();
	}


	/* ---------------------------------------------------------
	 * ----------------  JUMPS EDIT, DELETE, JUMP TYPE ADD -----
	 *  --------------------------------------------------------
	 */
	
	private void notebook_change(int desiredPage) {
		while(notebook.CurrentPage < desiredPage) {
			notebook.NextPage();
		}
		while(notebook.CurrentPage > desiredPage) {
			notebook.PrevPage();
		}
	}
	
	private void on_last_delete (object o, EventArgs args) {
		Console.WriteLine("delete last event");
		
		string warningString = "";
		switch (lastEventWas) {
			case eventType.JUMP:
				if (askDeletion) {
					int myID = myTreeViewJumps.EventSelectedID;
					if(lastJumpIsReactive) {
						notebook_change(1);
						warningString = Catalog.GetString("Attention: Deleting a Reactive subjump will delete the whole jump"); 
						myID = myTreeViewJumpsRj.EventSelectedID;
					} else {
						notebook_change(0);
					}

					confirmWinJumpRun = ConfirmWindowJumpRun.Show(app1, 
							Catalog.GetString("Do you want to delete last jump?"), 
							warningString, "jump", myID);
					confirmWinJumpRun.Button_accept.Clicked += new EventHandler(on_last_jump_delete_accepted);
				} else {
					on_last_jump_delete_accepted(o, args);
				}
				break;
			case eventType.RUN:
				if (askDeletion) {
					int myID = myTreeViewRuns.EventSelectedID;
					if (lastRunIsInterval) {
						notebook_change(3);
						warningString = Catalog.GetString("Attention: Deleting a intervalic sub-run will delete the whole run"); 
						myID = myTreeViewRunsInterval.EventSelectedID;
					} else {
						notebook_change(2);
					}

					confirmWinJumpRun = ConfirmWindowJumpRun.Show(app1, 
							Catalog.GetString("Do you want to delete last run?"), 
							warningString, "run", myID);
					confirmWinJumpRun.Button_accept.Clicked += new EventHandler(on_last_run_delete_accepted);
				} else {
					on_last_run_delete_accepted(o, args);
				}
				break;
			case eventType.PULSE:
				if (askDeletion) {
					int myID = myTreeViewPulses.EventSelectedID;
					notebook_change(4);
					confirmWinJumpRun = ConfirmWindowJumpRun.Show(app1, 
							Catalog.GetString("Do you want to delete last pulse?"), 
							warningString, "pulse", myID);
					confirmWinJumpRun.Button_accept.Clicked += new EventHandler(on_last_pulse_delete_accepted);
				} else {
					on_last_pulse_delete_accepted(o, args);
				}
				break;
			default:
				Console.WriteLine("on_last_delete default"); 
				break;

		}
	}

	private void on_last_jump_delete_accepted (object o, EventArgs args) {
		if(lastJumpIsReactive) 
			SqliteJump.Delete("jumpRj", currentJumpRj.UniqueID.ToString());
		else 
			SqliteJump.Delete("jump", currentJump.UniqueID.ToString());
		
		button_last_delete.Sensitive = false ;

		appbar2.Push( 1, Catalog.GetString("Last jump deleted") );

		if(lastJumpIsReactive) 
			myTreeViewJumpsRj.DelEvent(currentJumpRj.UniqueID);
		else 
			myTreeViewJumps.DelEvent(currentJump.UniqueID);
		

		if(createdStatsWin) 
			statsWin.FillTreeView_stats(false, false);
	}

	private void on_last_run_delete_accepted (object o, EventArgs args) {
		if (lastRunIsInterval) 
			SqliteRun.Delete("runInterval", currentRunInterval.UniqueID.ToString());
		else 
			SqliteRun.Delete("run", currentRun.UniqueID.ToString());
		
		button_last_delete.Sensitive = false ;

		appbar2.Push( 1, Catalog.GetString("Last run deleted") );

		if (lastRunIsInterval) 
			myTreeViewRunsInterval.DelEvent(currentRunInterval.UniqueID);
		else 
			myTreeViewRuns.DelEvent(currentRun.UniqueID);

		if(createdStatsWin) 
			statsWin.FillTreeView_stats(false, false);
	}

	private void on_last_pulse_delete_accepted (object o, EventArgs args) {
		SqlitePulse.Delete(currentPulse.UniqueID.ToString());
		
		button_last_delete.Sensitive = false ;

		appbar2.Push( 1, Catalog.GetString("Last pulse deleted") );

		myTreeViewPulses.DelEvent(currentPulse.UniqueID);

		/*
		if(createdStatsWin) {
			statsWin.FillTreeView_stats(false, false);
		}
		*/
	}

	private void on_edit_selected_jump_clicked (object o, EventArgs args) {
		notebook_change(0);
		Console.WriteLine("Edit selected jump (normal)");
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person (check also if it's not a individual RJ, the pass the parent RJ)
		if (myTreeViewJumps.EventSelectedID > 0) {
			//3.- obtain the data of the selected jump
			Jump myJump = SqliteJump.SelectNormalJumpData( myTreeViewJumps.EventSelectedID );
		
			//4.- edit this jump
			editJumpWin = EditJumpWindow.Show(app1, myJump, prefsDigitsNumber);
			editJumpWin.Button_accept.Clicked += new EventHandler(on_edit_selected_jump_accepted);
		}
	}
	
	private void on_edit_selected_jump_rj_clicked (object o, EventArgs args) {
		notebook_change(1);
		Console.WriteLine("Edit selected jump (RJ)");
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person (check also if it's not a individual RJ, the pass the parent RJ)
		if (myTreeViewJumpsRj.EventSelectedID > 0) {
			//3.- obtain the data of the selected jump
			JumpRj myJump = SqliteJump.SelectRjJumpData( myTreeViewJumpsRj.EventSelectedID );
		
			//4.- edit this jump
			editJumpRjWin = EditJumpRjWindow.Show(app1, myJump, prefsDigitsNumber);
			editJumpRjWin.Button_accept.Clicked += new EventHandler(on_edit_selected_jump_rj_accepted);
		}
	}
	
	private void on_repair_selected_reactive_jump_clicked (object o, EventArgs args) {
		notebook_change(1);
		Console.WriteLine("Repair selected subjump");
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person (check also if it's not a individual RJ, the pass the parent RJ)
		if (myTreeViewJumpsRj.EventSelectedID > 0) {
			//3.- obtain the data of the selected jump
			JumpRj myJump = SqliteJump.SelectRjJumpData( myTreeViewJumpsRj.EventSelectedID );
		
			//4.- edit this jump
			repairJumpRjWin = RepairJumpRjWindow.Show(app1, myJump);
			repairJumpRjWin.Button_accept.Clicked += new EventHandler(on_repair_selected_reactive_jump_accepted);
		}
	}
	
	private void on_edit_selected_jump_accepted (object o, EventArgs args) {
		Console.WriteLine("edit selected jump accepted");
		
		treeview_jumps_storeReset();
		fillTreeView_jumps(combo_jumps.Entry.Text);
	
		if(createdStatsWin) {
			statsWin.FillTreeView_stats(false, false);
		}
	}
	
	private void on_edit_selected_jump_rj_accepted (object o, EventArgs args) {
		Console.WriteLine("edit selected jump RJ accepted");
		
		treeview_jumps_rj_storeReset();
		fillTreeView_jumps_rj(combo_jumps_rj.Entry.Text);
		
		if(createdStatsWin) {
			statsWin.FillTreeView_stats(false, false);
		}
	}
	
	private void on_repair_selected_reactive_jump_accepted (object o, EventArgs args) {
		Console.WriteLine("Repair selected reactive jump accepted");
		
		treeview_jumps_rj_storeReset();
		fillTreeView_jumps_rj(combo_jumps_rj.Entry.Text);
		
		if(createdStatsWin) {
			statsWin.FillTreeView_stats(false, false);
		}
	}
	
	private void on_delete_selected_jump_clicked (object o, EventArgs args) {
		notebook_change(0);
		Console.WriteLine("delete selected jump (normal)");
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person
		Console.WriteLine(myTreeViewJumps.EventSelectedID.ToString());
		if (myTreeViewJumps.EventSelectedID > 0) {
			//3.- display confirmwindow of deletion 
			if (askDeletion) {
				confirmWinJumpRun = ConfirmWindowJumpRun.Show(app1, "Do you want to delete selected jump?", 
						"", "jump", myTreeViewJumps.EventSelectedID);
				confirmWinJumpRun.Button_accept.Clicked += new EventHandler(on_delete_selected_jump_accepted);
			} else {
				on_delete_selected_jump_accepted(o, args);
			}
		}
	}
	
	private void on_delete_selected_jump_rj_clicked (object o, EventArgs args) {
		notebook_change(1);
		Console.WriteLine("delete selected reactive jump");
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person (check also if it's not a individual RJ, the pass the parent RJ)
		if (myTreeViewJumpsRj.EventSelectedID > 0) {
			//3.- display confirmwindow of deletion 
			if (askDeletion) {
				confirmWinJumpRun = ConfirmWindowJumpRun.Show(app1,  Catalog.GetString("Do you want to delete selected jump?"), 
						 Catalog.GetString("Attention: Deleting a Reactive subjump will delete the whole jump"), 
						 "jump", myTreeViewJumpsRj.EventSelectedID);
				confirmWinJumpRun.Button_accept.Clicked += new EventHandler(on_delete_selected_jump_rj_accepted);
			} else {
				on_delete_selected_jump_rj_accepted(o, args);
			}
		}
	}
	
	private void on_delete_selected_jump_accepted (object o, EventArgs args) {
		Console.WriteLine("accept delete selected jump");
		
		SqliteJump.Delete( "jump", (myTreeViewJumps.EventSelectedID).ToString() );
		
		appbar2.Push( 1, Catalog.GetString ( "Deleted jump: " ) + myTreeViewJumps.EventSelectedID );
		myTreeViewJumps.DelEvent(myTreeViewJumps.EventSelectedID);

		if(createdStatsWin) {
			statsWin.FillTreeView_stats(false, false);
		}
	}

	private void on_delete_selected_jump_rj_accepted (object o, EventArgs args) {
		Console.WriteLine("accept delete selected jump");
		
		SqliteJump.Delete("jumpRj", myTreeViewJumpsRj.EventSelectedID.ToString());
		
		appbar2.Push( 1, Catalog.GetString ( "Deleted reactive jump: " ) + myTreeViewJumpsRj.EventSelectedID );
		myTreeViewJumpsRj.DelEvent(myTreeViewJumpsRj.EventSelectedID);

		if(createdStatsWin) {
			statsWin.FillTreeView_stats(false, false);
		}
	}
	
	private void on_jump_type_add_activate (object o, EventArgs args) {
		Console.WriteLine("Add new jump type");
			
		jumpTypeAddWin = JumpTypeAddWindow.Show(app1);
		jumpTypeAddWin.Button_accept.Clicked += new EventHandler(on_jump_type_add_accepted);
	}
	
	private void on_jump_type_add_accepted (object o, EventArgs args) {
		Console.WriteLine("ACCEPTED Add new jump type");
		updateComboJumps();
		updateComboJumpsRj();
	}

	/* ---------------------------------------------------------------------
	 * ----------------  RUNS 	 EDIT, DELETE, RUN TYPE ADD ------------
	 *  --------------------------------------------------------------------
	 */
	
	private void on_edit_selected_run_clicked (object o, EventArgs args) {
		notebook_change(2);
		Console.WriteLine("Edit selected run (normal)");
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person (check also if it's not a individual RJ, the pass the parent RJ)
		if (myTreeViewRuns.EventSelectedID > 0) {
			//3.- obtain the data of the selected run
			Run myRun = SqliteRun.SelectNormalRunData( myTreeViewRuns.EventSelectedID );
			Console.WriteLine(myRun);
		
			//4.- edit this run
			editRunWin = EditRunWindow.Show(app1, myRun, prefsDigitsNumber);
			editRunWin.Button_accept.Clicked += new EventHandler(on_edit_selected_run_accepted);
		}
	}
	
	private void on_edit_selected_run_interval_clicked (object o, EventArgs args) {
		notebook_change(3);
		Console.WriteLine("Edit selected run interval");
		//1.- check that there's a line selected
		//2.- check that this line is a run and not a person (check also if it's not a individual subrun, the pass the parent run)
		if (myTreeViewRunsInterval.EventSelectedID > 0) {
			//3.- obtain the data of the selected run
			RunInterval myRun = SqliteRun.SelectIntervalRunData( myTreeViewRunsInterval.EventSelectedID );
			Console.WriteLine(myRun);
		
			//4.- edit this run
			editRunIntervalWin = EditRunIntervalWindow.Show(app1, myRun, prefsDigitsNumber);
			editRunIntervalWin.Button_accept.Clicked += new EventHandler(on_edit_selected_run_interval_accepted);
		}
	}
	
	private void on_repair_selected_run_interval_clicked (object o, EventArgs args) {
		notebook_change(3);
		Console.WriteLine("Repair selected subrun");
		//1.- check that there's a line selected
		//2.- check that this line is a run and not a person 
		//(check also if it's not a individual run interval, then pass the parent run interval)
		if (myTreeViewRunsInterval.EventSelectedID > 0) {
			//3.- obtain the data of the selected run
			RunInterval myRun = SqliteRun.SelectIntervalRunData( myTreeViewRunsInterval.EventSelectedID );
		
			//4.- edit this run
			repairRunIntervalWin = RepairRunIntervalWindow.Show(app1, myRun);
			repairRunIntervalWin.Button_accept.Clicked += new EventHandler(on_repair_selected_run_interval_accepted);
		}
	}
	
	private void on_edit_selected_run_accepted (object o, EventArgs args) {
		Console.WriteLine("edit selected run accepted");
		
		treeview_runs_storeReset();
		fillTreeView_runs(combo_runs.Entry.Text);
		
		if(createdStatsWin) {
			statsWin.FillTreeView_stats(false, false);
		}
	}
	
	private void on_edit_selected_run_interval_accepted (object o, EventArgs args) {
		Console.WriteLine("edit selected run interval accepted");
		
		treeview_runs_interval_storeReset();
		fillTreeView_runs_interval(combo_runs_interval.Entry.Text);
		
		if(createdStatsWin) {
			statsWin.FillTreeView_stats(false, false);
		}
	}

	private void on_repair_selected_run_interval_accepted (object o, EventArgs args) {
		Console.WriteLine("repair selected run interval accepted");
		
		treeview_runs_interval_storeReset();
		fillTreeView_runs_interval(combo_runs_interval.Entry.Text);
		
		if(createdStatsWin) {
			statsWin.FillTreeView_stats(false, false);
		}
	}

	private void on_delete_selected_run_clicked (object o, EventArgs args) {
		notebook_change(2);
		Console.WriteLine("delete selected run (normal)");
		
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person
		if (myTreeViewRuns.EventSelectedID > 0) {
			//3.- display confirmwindow of deletion 
			if (askDeletion) {
				confirmWinJumpRun = ConfirmWindowJumpRun.Show(app1, "Do you want to delete selected run?", 
						"", "run", myTreeViewRuns.EventSelectedID);
				confirmWinJumpRun.Button_accept.Clicked += new EventHandler(on_delete_selected_run_accepted);
			} else {
				on_delete_selected_run_accepted(o, args);
			}
		}
	}
		
	
	private void on_delete_selected_run_interval_clicked (object o, EventArgs args) {
		notebook_change(3);
		Console.WriteLine("delete selected run interval");
		//1.- check that there's a line selected
		//2.- check that this line is a run and not a person (check also if it's a subrun, pass the parent run)
		if (myTreeViewRunsInterval.EventSelectedID > 0) {
			//3.- display confirmwindow of deletion 
			if (askDeletion) {
				confirmWinJumpRun = ConfirmWindowJumpRun.Show(app1,  Catalog.GetString("Do you want to delete selected run?"), 
						 Catalog.GetString("Attention: Deleting a Intervallic subrun will delete the whole run"), 
						 "run", myTreeViewJumpsRj.EventSelectedID);
				confirmWinJumpRun.Button_accept.Clicked += new EventHandler(on_delete_selected_run_interval_accepted);
			} else {
				on_delete_selected_run_interval_accepted(o, args);
			}
		}
	}

	private void on_delete_selected_run_accepted (object o, EventArgs args) {
		Console.WriteLine("accept delete selected run");
		
		SqliteRun.Delete( "run", (myTreeViewRuns.EventSelectedID).ToString() );
		
		appbar2.Push( 1, Catalog.GetString ( "Deleted run: " ) + myTreeViewRuns.EventSelectedID );
	
		myTreeViewRuns.DelEvent(myTreeViewRuns.EventSelectedID);

		if(createdStatsWin) {
			statsWin.FillTreeView_stats(false, false);
		}
	}

	private void on_delete_selected_run_interval_accepted (object o, EventArgs args) {
		Console.WriteLine("accept delete selected run");
		
		SqliteRun.Delete( "runInterval", (myTreeViewRunsInterval.EventSelectedID).ToString() );
		
		appbar2.Push( 1, Catalog.GetString ( "Deleted intervallic run: " ) + myTreeViewRunsInterval.EventSelectedID );
	
		myTreeViewRunsInterval.DelEvent(myTreeViewRunsInterval.EventSelectedID);

		if(createdStatsWin) {
			statsWin.FillTreeView_stats(false, false);
		}
	}
	
	private void on_run_type_add_activate (object o, EventArgs args) {
		Console.WriteLine("Add new run type");
			
		runTypeAddWin = RunTypeAddWindow.Show(app1);
		runTypeAddWin.Button_accept.Clicked += new EventHandler(on_run_type_add_accepted);
	}
	
	private void on_run_type_add_accepted (object o, EventArgs args) {
		Console.WriteLine("ACCEPTED Add new run type");
		updateComboRuns();
		updateComboRunsInterval();
	}

	/* ---------------------------------------------------------------------
	 * ----------------  PULSE 	 EDIT, DELETE, PULSE TYPE ADD ------------
	 *  --------------------------------------------------------------------
	 */
	
	private void on_edit_selected_pulse_clicked (object o, EventArgs args) {
		notebook_change(5);
		Console.WriteLine("Edit selected pulse");
		appbar2.Push ( 1, "Edit selected pulse (NOT IMPLEMENTED YET)");
		/*
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person (check also if it's not a individual RJ, the pass the parent RJ)
		if (myTreeViewRuns.RunSelectedID > 0) {
			//3.- obtain the data of the selected run
			Run myRun = SqliteRun.SelectNormalRunData( myTreeViewRuns.RunSelectedID );
			Console.WriteLine(myRun);
		
			//4.- edit this run
			editRunWin = EditRunWindow.Show(app1, myRun, prefsDigitsNumber);
			editRunWin.Button_accept.Clicked += new EventHandler(on_edit_selected_run_accepted);
		}
		*/
	}
	
	private void on_repair_selected_pulse_clicked (object o, EventArgs args) {
		notebook_change(5);
		Console.WriteLine("Repair selected pulse");
		appbar2.Push ( 1, "repair selected pulse (NOT IMPLEMENTED YET)");
		/*
		//1.- check that there's a line selected
		//2.- check that this line is a run and not a person 
		//(check also if it's not a individual run interval, then pass the parent run interval)
		if (myTreeViewRunsInterval.RunSelectedID > 0) {
			//3.- obtain the data of the selected run
			RunInterval myRun = SqliteRun.SelectIntervalRunData( myTreeViewRunsInterval.RunSelectedID );
		
			//4.- edit this run
			repairRunIntervalWin = RepairRunIntervalWindow.Show(app1, myRun);
			repairRunIntervalWin.Button_accept.Clicked += new EventHandler(on_repair_selected_run_interval_accepted);
		}
		*/
	}
	
	private void on_edit_selected_pulse_accepted (object o, EventArgs args) {
		Console.WriteLine("edit selected pulse accepted");
		
		/*
		treeview_runs_storeReset();
		fillTreeView_runs(combo_runs.Entry.Text);
		
		if(createdStatsWin) {
			statsWin.FillTreeView_stats(false, false);
		}
		*/
	}
	
	private void on_repair_selected_pulse_accepted (object o, EventArgs args) {
		Console.WriteLine("repair selected pulse accepted");
		
		/*
		treeview_runs_interval_storeReset();
		fillTreeView_runs_interval(combo_runs_interval.Entry.Text);
		
		if(createdStatsWin) {
			statsWin.FillTreeView_stats(false, false);
		}
		*/
	}

	private void on_delete_selected_pulse_clicked (object o, EventArgs args) {
		notebook_change(5);
		Console.WriteLine("delete selected pulse");
		appbar2.Push ( 1, "delete selected pulse (NOT IMPLEMENTED YET)");
		
		/*
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person
		if (myTreeViewRuns.RunSelectedID > 0) {
			//3.- display confirmwindow of deletion 
			if (askDeletion) {
				confirmWinJumpRun = ConfirmWindowJumpRun.Show(app1, "Do you want to delete selected run?", 
						"", "run", myTreeViewRuns.RunSelectedID);
				confirmWinJumpRun.Button_accept.Clicked += new EventHandler(on_delete_selected_run_accepted);
			} else {
				on_delete_selected_run_accepted(o, args);
			}
		}
		*/
	}
		
	private void on_delete_selected_pulse_accepted (object o, EventArgs args) {
		Console.WriteLine("accept delete selected pulse");
		
		/*
		SqliteRun.Delete( "run", (myTreeViewRuns.RunSelectedID).ToString() );
		
		appbar2.Push( Catalog.GetString ( "Deleted run: " ) + myTreeViewRuns.RunSelectedID );
	
		myTreeViewRuns.DelRun(myTreeViewRuns.RunSelectedID);

		if(createdStatsWin) {
			statsWin.FillTreeView_stats(false, false);
		}
		*/
	}

	
	private void on_pulse_type_add_activate (object o, EventArgs args) {
		Console.WriteLine("Add new pulse type");
			
		/*
		runTypeAddWin = RunTypeAddWindow.Show(app1);
		runTypeAddWin.Button_accept.Clicked += new EventHandler(on_run_type_add_accepted);
		*/
	}
	
	private void on_pulse_type_add_accepted (object o, EventArgs args) {
		Console.WriteLine("ACCEPTED Add new pulse type");
		/*
		updateComboRuns();
		updateComboRunsInterval();
		*/
	}


		
	/* ---------------------------------------------------------
	 * ----------------  SOME MORE CALLBACKS---------------------
	 *  --------------------------------------------------------
	 */
	
	//help
	private void on_about1_activate (object o, EventArgs args) {
		string translator_credits = Catalog.GetString ("translator-credits");
		//only print if exist (don't print 'translator-credits' word
		if(translator_credits == "translator-credits") 
			translator_credits = "";

		new About(progversion, authors, translator_credits);
	}

	/* ---------------------------------------------------------
	 * ----------------  SENSITIVE GUI METHODS-------------------
	 *  --------------------------------------------------------
	 */
	
	private void sensitiveGuiNoSession () 
	{
		//menuitems
		menuitem_preferences.Sensitive = true;
		menuitem_export_csv.Sensitive = false;
		menuitem_export_xml.Sensitive = false;
		menuitem_recuperate_person.Sensitive = false;
		menuitem_recuperate_persons_from_session.Sensitive = false;
		menuitem_person_add_single.Sensitive = false;
		menuitem_person_add_multiple.Sensitive = false;
		treeview_persons.Sensitive = false;
		menuitem_edit_session.Sensitive = false;
		menuitem_delete_session.Sensitive = false;
		
		menu_persons.Sensitive = false;
		menu_jumps.Sensitive = false;
		menu_runs.Sensitive = false;
		menu_pulses.Sensitive = false;
		menu_view.Sensitive = false;

		frame_persons.Sensitive = false;
		button_recup_per.Sensitive = false;
		button_create_per.Sensitive = false;
		button_edit_current_person.Sensitive = false;
		menuitem_delete_current_person_from_session.Sensitive = false;
		button_show_all_person_events.Sensitive = false;
		show_all_person_events.Sensitive = false;
		
		//notebook
		notebook.Sensitive = false;
		//hbox_jumps.Sensitive = false;
		//hbox_jumps_rj.Sensitive = false;
		
		button_last_delete.Sensitive = false;
	}
	
	private void sensitiveGuiYesSession () {
		frame_persons.Sensitive = true;
		button_recup_per.Sensitive = true;
		button_create_per.Sensitive = true;
		
		menuitem_export_csv.Sensitive = true;
		menuitem_export_xml.Sensitive = false; //it's not coded yet
		menuitem_recuperate_person.Sensitive = true;
		menuitem_recuperate_persons_from_session.Sensitive = true;
		menuitem_person_add_single.Sensitive = true;
		menuitem_person_add_multiple.Sensitive = true;
		menuitem_edit_session.Sensitive = true;
		menuitem_delete_session.Sensitive = true;
		menu_persons.Sensitive = true;
	}

	//only called by delete person functions (if we run out of persons)
	private void sensitiveGuiNoPerson () {
		notebook.Sensitive = false;
		treeview_persons.Sensitive = false;
		
		button_edit_current_person.Sensitive = false;
		menuitem_edit_current_person.Sensitive = false;
		menuitem_delete_current_person_from_session.Sensitive = false;
		button_show_all_person_events.Sensitive = false;
		show_all_person_events.Sensitive = false;
		
		menu_jumps.Sensitive = false;
		menu_runs.Sensitive = false;
		menu_pulses.Sensitive = false;
		menu_view.Sensitive = false;
		
		//menuitem_jump_type_add.Sensitive = false;
		button_last_delete.Sensitive = false;
	}
	
	private void sensitiveGuiYesPerson () {
		notebook.Sensitive = true;
		treeview_persons.Sensitive = true;
		button_edit_current_person.Sensitive = true;
		menuitem_edit_current_person.Sensitive = true;
		menuitem_delete_current_person_from_session.Sensitive = true;
		button_show_all_person_events.Sensitive = true;
		show_all_person_events.Sensitive = true;
		
		menu_jumps.Sensitive = true;
		menu_runs.Sensitive = true;
		menu_pulses.Sensitive = true;
		menu_view.Sensitive = true;
		
		combo_jumps.Sensitive = true;
		combo_jumps_rj.Sensitive = true;
		combo_runs.Sensitive = true;
		combo_runs_interval.Sensitive = true;
		combo_pulses.Sensitive = true;
	}
	
	private void sensitiveGuiYesEvent () {
		button_last_delete.Sensitive = true;
	}
	
	private void sensitiveGuiEventDoing () {
		//hbox
		hbox_jumps.Sensitive = false;
		hbox_jumps_rj.Sensitive = false;
		hbox_runs.Sensitive = false;
		hbox_runs_interval.Sensitive = false;
		hbox_pulses.Sensitive = false;
		
		//menu
		menu_jumps.Sensitive = false;
		menu_runs.Sensitive = false;
		menu_pulses.Sensitive = false;
		
		//cancel, delete last, finish
		button_last_delete.Sensitive = false;
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
			switch (currentEventIs) {
				case eventType.JUMP:
					if(currentJumpType.IsRepetitive) {
						//if(! currentJumpRj.Cancel) {
						button_rj_last.Sensitive = true;
						button_last.Sensitive = false;
						//}
					} else {
						//if(! currentJump.Cancel) {
						button_last.Sensitive = true;
						button_rj_last.Sensitive = false;
						//}
					}
					break;
				case eventType.RUN:
					if(currentRunType.HasIntervals) {
						//if(! currentRunInterval.Cancel) {
						button_run_interval_last.Sensitive = true;
						button_run_last.Sensitive = false;
						//}
					} else {
						//if(! currentRun.Cancel) {
						button_run_last.Sensitive = true;
						button_run_interval_last.Sensitive = false;
						//}
					}
					break;
				case eventType.PULSE:
					Console.WriteLine("sensitiveGuiEventDone pulse");
					break;
				default:
					Console.WriteLine("sensitiveGuiEventDone default");
					break;
			}
		}
		
		//menu
		menu_jumps.Sensitive = true;
		menu_runs.Sensitive = true;
		menu_pulses.Sensitive = true;
	}

}
