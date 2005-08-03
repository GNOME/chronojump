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
using Gnome;
using System.Collections; //ArrayList


public class ChronoJump 
{
	[Widget] Gtk.Window app1;
	[Widget] Gnome.AppBar appbar2;
	[Widget] Gtk.TreeView treeview_jumps;
	[Widget] Gtk.TreeView treeview_jumps_rj;
	[Widget] Gtk.TreeView treeview_runs;
	[Widget] Gtk.TreeView treeview_runs_interval;
	[Widget] Gtk.Box hbox_combo_jumps;
	[Widget] Gtk.Box hbox_combo_jumps_rj;
	[Widget] Gtk.Box hbox_combo_runs;
	[Widget] Gtk.Box hbox_combo_person_current;
	[Widget] Gtk.Box hbox_jumps;
	[Widget] Gtk.Box hbox_jumps_rj;
	[Widget] Gtk.Combo combo_jumps;
	[Widget] Gtk.Combo combo_jumps_rj;
	[Widget] Gtk.Combo combo_runs;
	[Widget] Gtk.Combo combo_person_current;

	[Widget] Gtk.CheckButton checkbutton_sort_by_type;
	[Widget] Gtk.CheckButton checkbutton_sort_by_type_rj;
	[Widget] Gtk.MenuItem menuitem_edit_selected_jump;
	[Widget] Gtk.MenuItem menuitem_delete_selected_jump;
	[Widget] Gtk.Button button_edit_selected_jump;
	[Widget] Gtk.Button button_delete_selected_jump;
	[Widget] Gtk.MenuItem menuitem_edit_selected_jump_rj;
	[Widget] Gtk.MenuItem menuitem_delete_selected_jump_rj;
	[Widget] Gtk.Button button_edit_selected_jump_rj;
	[Widget] Gtk.Button button_delete_selected_jump_rj;
	[Widget] Gtk.MenuItem menuitem_edit_selected_run;
	[Widget] Gtk.MenuItem menuitem_delete_selected_run;
	[Widget] Gtk.Button button_edit_selected_run;
	[Widget] Gtk.Button button_delete_selected_run;

	//widgets for enable or disable
	[Widget] Gtk.Button button_new;
	[Widget] Gtk.Button button_open;
	[Widget] Gtk.Button button_recup_per;
	[Widget] Gtk.Button button_create_per;
	[Widget] Gtk.Label label_current_person;

	[Widget] Gtk.Button button_sj;
	[Widget] Gtk.Button button_sj_plus;
	[Widget] Gtk.Button button_cmj;
	[Widget] Gtk.Button button_abk;
	[Widget] Gtk.Button button_dj;
	[Widget] Gtk.Button button_more;
	[Widget] Gtk.Button button_rj_j;
	[Widget] Gtk.Button button_rj_t;
	[Widget] Gtk.Button button_run_free;
	[Widget] Gtk.Button button_run_20m;
	[Widget] Gtk.Button button_run_100m;
	[Widget] Gtk.Button button_run_200m;
	[Widget] Gtk.Button button_run_400m;
	[Widget] Gtk.Button button_run_1000m;
	[Widget] Gtk.Button button_run_2000m;
	
	[Widget] Gtk.Button button_last;
	[Widget] Gtk.Button button_rj_last;
	[Widget] Gtk.Button button_run_last;
	[Widget] Gtk.Button button_run_interval_last;
	[Widget] Gtk.Button button_last_delete;
	[Widget] Gtk.MenuItem preferences;
	[Widget] Gtk.MenuItem menuitem_export_csv;
	[Widget] Gtk.MenuItem menuitem_export_xml;
	[Widget] Gtk.MenuItem menuitem_recuperate_person;
	[Widget] Gtk.MenuItem create_person;
	[Widget] Gtk.MenuItem menuitem_edit_session;
	[Widget] Gtk.MenuItem menuitem_delete_session;
	[Widget] Gtk.MenuItem menuitem_recuperate_persons_from_session;
	[Widget] Gtk.MenuItem menu_persons;
	[Widget] Gtk.MenuItem menu_jumps;
	[Widget] Gtk.MenuItem menu_runs;
	[Widget] Gtk.MenuItem menu_view;
		
	[Widget] Gtk.MenuItem sj;
	[Widget] Gtk.MenuItem sj_plus;
	[Widget] Gtk.MenuItem cmj;
	[Widget] Gtk.MenuItem abk;
	[Widget] Gtk.MenuItem dj;
	[Widget] Gtk.MenuItem more_simple_jumps;
	[Widget] Gtk.MenuItem more_rj;
	[Widget] Gtk.MenuItem jump_type_add;
	[Widget] Gtk.MenuItem rj_j;
	[Widget] Gtk.MenuItem rj_t;
	[Widget] Gtk.MenuItem menuitem_run_free;
	[Widget] Gtk.MenuItem menuitem_20m;
	[Widget] Gtk.MenuItem menuitem_100m;
	[Widget] Gtk.MenuItem menuitem_200m;
	[Widget] Gtk.MenuItem menuitem_400m;
	[Widget] Gtk.MenuItem menuitem_1000m;
	[Widget] Gtk.MenuItem menuitem_2000m;

	[Widget] Gtk.Button button_edit_current_person;
	[Widget] Gtk.MenuItem menuitem_edit_current_person;
	[Widget] Gtk.MenuItem menuitem_delete_current_person_from_session;
	[Widget] Gtk.Button button_cancel;
	[Widget] Gtk.Button button_finish;
	
	[Widget] Gtk.RadioMenuItem menuitem_simulated;
	[Widget] Gtk.RadioMenuItem menuitem_serial_port;
	
	[Widget] Gtk.Notebook notebook;

	private Random rand;
	
	private static string [] authors = {"Xavier de Blas", "Juan Gonzalez"};
	private static string progversion = "0.2";
	private static string progname = "ChronoJump";
	
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
	//private TreeStore treeview_runs_interval_store;
	//private TreeViewRunsInterval myTreeViewRunsInterval;


	private static string allJumpsName = Catalog.GetString("All jumps");
	private static string allRunsName = Catalog.GetString("All runs");

	
	//preferences variables
	private static int prefsDigitsNumber;
	private static bool showHeight;
	private static bool showInitialSpeed;
	private static bool simulated;
	private static bool askDeletion;
	private static bool weightStatsPercent;
	private static bool heightPreferred;
	private static bool metersSecondsPreferred;

	private static Person currentPerson;
	private static Session currentSession;
	private static Jump currentJump;
	private static JumpRj currentJumpRj;
	private static Run currentRun;
	//private static RunInterval currentRunInterval;
	private static bool lastEventWasJump; //if last event was Jump (true) or Run (false)
	private static bool lastJumpIsReactive; //if last Jump is reactive or not
	private static bool lastRunIsInterval; //if last run is interval or not (obvious) 
	private static JumpType currentJumpType;
	private static RunType currentRunType;

	//windows needed
	SessionAddWindow sessionAddWin;
	SessionEditWindow sessionEditWin;
	SessionLoadWindow sessionLoadWin;
	PersonRecuperateWindow personRecuperateWin; 
	PersonAddWindow personAddWin; 
	PersonModifyWindow personModifyWin; 
	JumpsMoreWindow jumpsMoreWin;
	JumpsRjMoreWindow jumpsRjMoreWin;
	JumpExtraWindow jumpExtraWin; //for normal and repetitive jumps 
	EditJumpWindow editJumpWin;
	EditJumpRjWindow editJumpRjWin;
	JumpTypeAddWindow jumpTypeAddWin;
	
	RunExtraWindow runExtraWin; //for normal and intervaled runs 
	RunsMoreWindow runsMoreWin;
	//RunsIntervalMoreWindow runsIntervalMoreWin;
	//RunTypeAddWindow runTypeAddWin;
	EditRunWindow editRunWin;
	//EditRunRjWindow editRunRjWin;

	ConfirmWindowJumpRun confirmWinJumpRun;	//for deleting jumps and RJ jumps (and runs)
	StatsWindow statsWin;
	
	
	//Progress bar 
	[Widget] Gtk.Box hbox_progress_bar;
	Gtk.ProgressBar progressBar;

	//platform state variables
	enum States {
		ON,
		OFF
	}
	Chronopic.Plataforma platformState;	//on (in platform), off (jumping), or unknow
	Chronopic.Respuesta respuesta;		//ok, error, or timeout in calling the platform
	Chronopic cp;
	States loggedState;		//log of last state
	private bool firstRjValue;
	private double rjTcCount;
	private double rjTvCount;
	private string rjTcString;
	private string rjTvString;
	
	private bool createdStatsWin;


	public static void Main(string [] args) 
	{
		new ChronoJump(args);
	}

	public ChronoJump (string [] args) 
	{
		Catalog.Init ("chronojump", "./locale");

		
		Program program = new Program(progname, progversion, Modules.UI, args);

		Glade.XML gxml = new Glade.XML (null, "chronojump.glade", "app1", "chronojumpGlade");

		gxml.Autoconnect(this);

		Sqlite.Connect();
		
		//Chech if the DB file exists
		if (!Sqlite.CheckTables()) {
			Console.WriteLine ( Catalog.GetString ("no tables, creating ...") );
			Sqlite.CreateFile();
			Sqlite.CreateTables();
		} else { Console.WriteLine ( Catalog.GetString ("tables already created") ); }

		loadPreferences ();
		
		createTreeView_jumps(treeview_jumps);
		createTreeView_jumps_rj(treeview_jumps_rj);
		createTreeView_runs(treeview_runs);
		createTreeView_runs_interval(treeview_runs_interval);

		createComboJumps();
		createComboJumpsRj();
		createComboRuns();
		createComboSujetoCurrent();
		createdStatsWin = false;

		//We have no session, mark some widgets as ".Sensitive = false"
		sensitiveGuiNoSession();

		progressBar = new ProgressBar();
		hbox_progress_bar.PackStart(progressBar, true, true, 0);
		hbox_progress_bar.ShowAll();
		
		appbar2.Push ( Catalog.GetString ("Ready.") );

		rand = new Random(40);
				
		//init connecting with chronopic	
		chronopicInit();
				
		program.Run();
	}

	private void chronopicInit ()
	{
		Console.WriteLine ( Catalog.GetString ("starting connection with serial port") );
		Console.WriteLine ( Catalog.GetString ("if program crashes, write to xavi@xdeblas.com") );

		cp = new Chronopic("/dev/ttyS0");

		//-- Read initial state of platform
		respuesta=cp.Read_platform(out platformState);
		switch(respuesta) {
			case Chronopic.Respuesta.Error:
				Console.WriteLine(Catalog.GetString("Error comunicating with Chronopic"));
				break;
			case Chronopic.Respuesta.Timeout:
				Console.WriteLine(Catalog.GetString("Chronopic in not responding"));
				break;
			default:
				Console.WriteLine(Catalog.GetString("Chronopic OK"));
				break;
		}
    
		Console.Write(Catalog.GetString("Plataform state: "));
		Console.WriteLine("{0}", platformState);
	}
	
	private void loadPreferences () 
	{
		Console.WriteLine ("Chronojump database version file: {0}", 
				SqlitePreferences.Select("databaseVersion") );
		
		prefsDigitsNumber = Convert.ToInt32 ( SqlitePreferences.Select("digitsNumber") );
		

		if ( SqlitePreferences.Select("showHeight") == "True" ) {
			showHeight = true;
		} else {
			showHeight = false;
		}
			
		if ( SqlitePreferences.Select("showInitialSpeed") == "True" ) {
			showInitialSpeed = true;
		} else {
			showInitialSpeed = false;
		}
			
		if ( SqlitePreferences.Select("simulated") == "True" ) {
			simulated = true;
			menuitem_simulated.Active = true;
		} else {
			simulated = false;
			menuitem_serial_port.Active = true;
		}
		
		if ( SqlitePreferences.Select("askDeletion") == "True" ) {
			askDeletion = true;
		} else {
			askDeletion = false;
		}
		
		if ( SqlitePreferences.Select("weightStatsPercent") == "True" ) {
			weightStatsPercent = true;
		} else {
			weightStatsPercent = false;
		}
		
		if ( SqlitePreferences.Select("heightPreferred") == "True" ) {
			heightPreferred = true;
		} else {
			heightPreferred = false;
		}
		
		if ( SqlitePreferences.Select("metersSecondsPreferred") == "True" ) {
			metersSecondsPreferred = true;
		} else {
			metersSecondsPreferred = false;
		}
		
		Console.WriteLine ( Catalog.GetString ("Preferences loaded") );
	}

	/* ---------------------------------------------------------
	 * ----------------  TREEVIEW JUMPS ------------------------
	 *  --------------------------------------------------------
	 */

	private void createTreeView_jumps (Gtk.TreeView tv) {
		//myTreeViewJumps is a TreeViewJumps instance
		bool sortByType = false;
		if(checkbutton_sort_by_type.Active) {
			sortByType = true;
		}
		myTreeViewJumps = new TreeViewJumps( tv, sortByType, showHeight, showInitialSpeed, prefsDigitsNumber );
	}

	private void fillTreeView_jumps (Gtk.TreeView tv, TreeStore store, string filter) {
		string [] myJumps;
		
		if(checkbutton_sort_by_type.Active) {
			myJumps = SqliteJump.SelectAllNormalJumps(
					currentSession.UniqueID, "ordered_by_type");
		} else {
			myJumps = SqliteJump.SelectAllNormalJumps(
					currentSession.UniqueID, "ordered_by_time");
		}
		myTreeViewJumps.Fill(myJumps, filter);
	}

	private void on_checkbutton_sort_by_type_clicked(object o, EventArgs args) {
		string myText = combo_jumps.Entry.Text;
			
		treeview_jumps_storeReset();
		fillTreeView_jumps(treeview_jumps, treeview_jumps_store, myText);
		treeview_jumps.ExpandAll();
	}
	
	private void on_button_tv_collapse_clicked (object o, EventArgs args) {
		treeview_jumps.CollapseAll();
	}
	
	private void on_button_tv_expand_clicked (object o, EventArgs args) {
		treeview_jumps.ExpandAll();
	}
	
	private void treeview_jumps_storeReset() {
		myTreeViewJumps.RemoveColumns();
		bool sortByType = false;
		if(checkbutton_sort_by_type.Active) {
			sortByType = true;
		}
		myTreeViewJumps = new TreeViewJumps( treeview_jumps, sortByType, showHeight, showInitialSpeed, prefsDigitsNumber );
	}


	/* ---------------------------------------------------------
	 * ----------------  TREEVIEW JUMPS RJ ---------------------
	 *  --------------------------------------------------------
	 */

	private void createTreeView_jumps_rj (Gtk.TreeView tv) {
		myTreeViewJumpsRj = new TreeViewJumpsRj( tv, showHeight, showInitialSpeed, prefsDigitsNumber );
	}

	private void fillTreeView_jumps_rj (Gtk.TreeView tv, TreeStore store, string filter) {
		string [] myJumps;
		if(checkbutton_sort_by_type_rj.Active) {
			myJumps = SqliteJump.SelectAllRjJumps(
						currentSession.UniqueID, "ordered_by_type"); 
		} else {
			myJumps = SqliteJump.SelectAllRjJumps(
						currentSession.UniqueID, "ordered_by_time");
		}
		myTreeViewJumpsRj.Fill(myJumps, filter);
	}

	private void on_checkbutton_sort_by_type_rj_clicked(object o, EventArgs args) {
		string myText = combo_jumps_rj.Entry.Text;
			
		treeview_jumps_rj_storeReset();
		fillTreeView_jumps_rj(treeview_jumps_rj, treeview_jumps_rj_store, myText);
		myTreeViewJumpsRj.ExpandOptimal();
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
		myTreeViewJumpsRj = new TreeViewJumpsRj( treeview_jumps_rj, showHeight, showInitialSpeed, prefsDigitsNumber );
	}

	/* ---------------------------------------------------------
	 * ----------------  TREEVIEW RUNS -------------------------
	 *  --------------------------------------------------------
	 */

	private void createTreeView_runs (Gtk.TreeView tv) {
		//myTreeViewRuns is a TreeViewRuns instance
		bool sortByType = false;
		if(checkbutton_sort_by_type.Active) {
			sortByType = true;
		}
		myTreeViewRuns = new TreeViewRuns( tv, sortByType, prefsDigitsNumber );
	}

	private void fillTreeView_runs (Gtk.TreeView tv, TreeStore store, string filter) {
		string [] myRuns;
		
		if(checkbutton_sort_by_type.Active) {
			myRuns = SqliteRun.SelectAllNormalRuns(
					currentSession.UniqueID, "ordered_by_type");
		} else {
			myRuns = SqliteRun.SelectAllNormalRuns(
					currentSession.UniqueID, "ordered_by_time");
		}
		myTreeViewRuns.Fill(myRuns, filter);
	}
	
	private void on_checkbutton_sort_by_type_run_clicked(object o, EventArgs args) {
		string myText = combo_runs.Entry.Text;
			
		treeview_runs_storeReset();
		fillTreeView_runs(treeview_runs, treeview_runs_store, myText);
		treeview_runs.ExpandAll();
	}
	
	private void on_button_tv_run_collapse_clicked (object o, EventArgs args) {
		treeview_runs.CollapseAll();
	}
	
	private void on_button_tv_run_expand_clicked (object o, EventArgs args) {
		treeview_runs.ExpandAll();
	}
	
	private void treeview_runs_storeReset() {
		myTreeViewRuns.RemoveColumns();
		bool sortByType = false;
		if(checkbutton_sort_by_type.Active) {
			sortByType = true;
		}
		myTreeViewRuns = new TreeViewRuns( treeview_runs, sortByType, prefsDigitsNumber );
	}

	/* ---------------------------------------------------------
	 * ----------------  TREEVIEW RUN INTERVAL -----------------
	 *  --------------------------------------------------------
	 */

	private void createTreeView_runs_interval (Gtk.TreeView tv) {
		/*
		//myTreeViewRuns is a TreeViewRuns instance
		bool sortByType = false;
		if(checkbutton_sort_by_type.Active) {
			sortByType = true;
		}
		myTreeViewRuns = new TreeViewRuns( tv, sortByType, prefsDigitsNumber );
		*/
	}

	private void fillTreeView_runs_interval (Gtk.TreeView tv, TreeStore store, string filter) {
		/*
		string [] myRuns;
		
		if(checkbutton_sort_by_type.Active) {
			myRuns = SqliteRun.SelectAllNormalRuns(
					currentSession.UniqueID, "ordered_by_type");
		} else {
			myRuns = SqliteRun.SelectAllNormalRuns(
					currentSession.UniqueID, "ordered_by_time");
		}
		myTreeViewRuns.Fill(myRuns, filter);
		*/
	}
	
	private void on_checkbutton_sort_by_type_run_interval_clicked(object o, EventArgs args) {
		/*
		string myText = combo_runs.Entry.Text;
			
		treeview_runs_storeReset();
		fillTreeView_runs(treeview_runs, treeview_runs_store, myText);
		treeview_runs.ExpandAll();
		*/
	}
	
	private void on_button_tv_run_interval_collapse_clicked (object o, EventArgs args) {
		/*
		treeview_runs.CollapseAll();
		*/
	}
	
	private void on_button_tv_run_interval_optimal_clicked (object o, EventArgs args) {
	}
	
	private void on_button_tv_run_interval_expand_clicked (object o, EventArgs args) {
		/*
		treeview_runs.ExpandAll();
		*/
	}
	
	private void treeview_runs_interval_storeReset() {
		/*
		myTreeViewRuns.RemoveColumns();
		bool sortByType = false;
		if(checkbutton_sort_by_type.Active) {
			sortByType = true;
		}
		myTreeViewRuns = new TreeViewRuns( treeview_runs, sortByType, prefsDigitsNumber );
		*/
	}

	/* ---------------------------------------------------------
	 * ----------------  CREATE AND UPDATE COMBOS ---------------
	 *  --------------------------------------------------------
	 */
	private void createComboJumps() {
		combo_jumps = new Combo ();
		combo_jumps.PopdownStrings = 
			SqliteJumpType.SelectJumpTypes(allJumpsName, "", true); //without filter, only select name
		
		combo_jumps.DisableActivate ();
		combo_jumps.Entry.Changed += new EventHandler (on_combo_jumps_changed);

		hbox_combo_jumps.PackStart(combo_jumps, true, true, 0);
		hbox_combo_jumps.ShowAll();
		
		combo_jumps.Sensitive = false;
	}
	
	private void createComboJumpsRj() {
		combo_jumps_rj = new Combo ();
		combo_jumps_rj.PopdownStrings = SqliteJumpType.SelectJumpRjTypes(allJumpsName, true); //only select name
		
		combo_jumps_rj.DisableActivate ();
		combo_jumps_rj.Entry.Changed += new EventHandler (on_combo_jumps_rj_changed);

		hbox_combo_jumps_rj.PackStart(combo_jumps_rj, true, true, 0);
		hbox_combo_jumps_rj.ShowAll();
		
		combo_jumps_rj.Sensitive = false;
	}
	
	private void createComboRuns() {
		combo_runs = new Combo ();
		combo_runs.PopdownStrings = 
			SqliteRunType.SelectRunTypes(allRunsName, true); //without filter, only select name
		
		combo_runs.DisableActivate ();
		combo_runs.Entry.Changed += new EventHandler (on_combo_runs_changed);

		hbox_combo_runs.PackStart(combo_runs, true, true, 0);
		hbox_combo_runs.ShowAll();
		
		combo_runs.Sensitive = false;
	}

	private void createComboSujetoCurrent() {
		combo_person_current = new Combo ();
		
		combo_person_current.DisableActivate ();
		combo_person_current.Entry.Changed += new EventHandler (on_combo_person_current_changed);

		hbox_combo_person_current.PackStart(combo_person_current, true, true, 0);
		hbox_combo_person_current.ShowAll();

		combo_person_current.Sensitive = false;
		button_edit_current_person.Sensitive = false;
		menuitem_edit_current_person.Sensitive = false;
	}
		
	private void updateComboJumps() {
		combo_jumps.PopdownStrings = 
			SqliteJumpType.SelectJumpTypes(allJumpsName, "", true); //without filter, only select name
	}
	
	private void updateComboJumpsRj() {
		combo_jumps_rj.PopdownStrings = SqliteJumpType.SelectJumpRjTypes(allJumpsName, true); //only select name
	}

	private bool updateComboSujetoCurrent() {
		string [] jumpers = SqlitePersonSession.SelectCurrentSession(currentSession.UniqueID);
		combo_person_current.PopdownStrings = jumpers; 
		
		if(jumpers.Length > 0) {
			return true;
		} else {
			return false;
		}
	}
	
	//combosujeto does not show always the last person as currentperson
	//imagine when we edit a person and we change the name of him and then we accept,
	//the combosujeto need to select the person just edited, not the last created person as the SQL says
	private bool updateComboSujetoCurrent (string name) {
		string [] jumpers = SqlitePersonSession.SelectCurrentSession(currentSession.UniqueID);
		combo_person_current.PopdownStrings = jumpers; 
		
		foreach (string jumper in jumpers) {
			if (jumper == name) {
				combo_person_current.Entry.Text = jumper;
			}
		}
		
		if(jumpers.Length > 0) {
			return true;
		} else {
			return false;
		}
	}
	
	private void on_combo_jumps_changed(object o, EventArgs args) {
		string myText = combo_jumps.Entry.Text;

		//show the edit-delete selected jumps buttons:
		
		menuitem_edit_selected_jump.Sensitive = true;
		menuitem_delete_selected_jump.Sensitive = true;
		button_edit_selected_jump.Sensitive = true;
		button_delete_selected_jump.Sensitive = true;
		
		treeview_jumps_storeReset();
		fillTreeView_jumps(treeview_jumps, treeview_jumps_store, myText);
		
		if (myText == allJumpsName) {
			checkbutton_sort_by_type.Sensitive = true ;
		} else {
			checkbutton_sort_by_type.Sensitive = false ;
			//expand all rows if a jump filter is selected:
			treeview_jumps.ExpandAll();
		}
	}
	
	private void on_combo_jumps_rj_changed(object o, EventArgs args) {
		string myText = combo_jumps_rj.Entry.Text;

		//show the edit-delete selected jumps buttons:

		menuitem_edit_selected_jump_rj.Sensitive = true;
		menuitem_delete_selected_jump_rj.Sensitive = true;
		button_edit_selected_jump_rj.Sensitive = true;
		button_delete_selected_jump_rj.Sensitive = true;

		treeview_jumps_rj_storeReset();
		fillTreeView_jumps_rj(treeview_jumps, treeview_jumps_store, myText);

		if (myText == allJumpsName) {
			checkbutton_sort_by_type_rj.Sensitive = true ;
		} else {
			checkbutton_sort_by_type_rj.Sensitive = false ;
			//expand all rows if a jump filter is selected:
			myTreeViewJumpsRj.ExpandOptimal();
		}
	}

	private void on_combo_runs_changed(object o, EventArgs args) {
		string myText = combo_runs.Entry.Text;

		//show the edit-delete selected runs buttons:
		menuitem_edit_selected_run.Sensitive = true;
		menuitem_delete_selected_run.Sensitive = true;
		button_edit_selected_run.Sensitive = true;
		button_delete_selected_run.Sensitive = true;

		treeview_runs_storeReset();
		fillTreeView_runs(treeview_runs, treeview_runs_store, myText);

		if (myText == allRunsName) {
			checkbutton_sort_by_type.Sensitive = true ;
		} else {
			checkbutton_sort_by_type.Sensitive = false ;
			//expand all rows if a runfilter is selected:
			treeview_runs.ExpandAll();
		}
	}


	private void on_combo_person_current_changed(object o, EventArgs args) {
		string myText = combo_person_current.Entry.Text;
		if(myText != "" && myText.LastIndexOf(":") != -1) {
			//if people modify the values in the combo_person_current, and this valeus are not correct, 
			//let's update the combosujetocurrent
			
			if(SqlitePersonSession.PersonSelectExistsInSession(Util.FetchID(myText), currentSession.UniqueID)) 
			{
				currentPerson = SqlitePersonSession.PersonSelect(Util.FetchID(myText));
			}
		}
		//else {
		//	updateComboSujetoCurrent();
		//}
	}

	
	/* ---------------------------------------------------------
	 * ----------------  DELETE EVENT, QUIT  -----------------------
	 *  --------------------------------------------------------
	 */
	
	private void on_delete_event (object o, DeleteEventArgs args) {
		Console.WriteLine("Chao!");
    
		cp.Close();
		
		Application.Quit();
	}

	private void on_quit1_activate (object o, EventArgs args) {
		Console.WriteLine("Chao!");
    
		cp.Close();
		
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

			
			//load the jumps treeview
			treeview_jumps_storeReset();
			fillTreeView_jumps(treeview_jumps, treeview_jumps_store, allJumpsName);
			//load the jumps_rj treeview_rj
			treeview_jumps_rj_storeReset();
			fillTreeView_jumps_rj(treeview_jumps_rj, treeview_jumps_rj_store, allJumpsName);
			//load the runs treeview
			treeview_runs_storeReset();
			fillTreeView_runs(treeview_runs, treeview_runs_store, allRunsName);
			//load the runs_interval treeview
			//treeview_jumps_storeReset();
			//fillTreeView_jumps(treeview_jumps, treeview_jumps_store, allJumpsName);


			//show hidden widgets
			sensitiveGuiNoSession();
			sensitiveGuiYesSession();

			//for sure, jumpsExists is false, because we create a new session

			button_edit_current_person.Sensitive = false;
			menuitem_edit_current_person.Sensitive = false;
			menuitem_delete_current_person_from_session.Sensitive = false;
			//update combo sujeto current
			updateComboSujetoCurrent();
			combo_person_current.Sensitive = false;
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
		
		//load the treeview_jumps
		treeview_jumps_storeReset();
		fillTreeView_jumps(treeview_jumps, treeview_jumps_store, allJumpsName);
		//load the treeview_jumps_rj
		treeview_jumps_rj_storeReset();
		fillTreeView_jumps_rj(treeview_jumps_rj, treeview_jumps_rj_store, allJumpsName);
		//load the runs treeview
		treeview_runs_storeReset();
		fillTreeView_runs(treeview_runs, treeview_runs_store, allRunsName);
		//load the runs_interval treeview
		//treeview_jumps_storeReset();
		//fillTreeView_jumps(treeview_jumps, treeview_jumps_store, allJumpsName);
		

		//show hidden widgets
		sensitiveGuiNoSession();
		sensitiveGuiYesSession();
		
		button_edit_current_person.Sensitive = false;
		menuitem_edit_current_person.Sensitive = false;
		menuitem_delete_current_person_from_session.Sensitive = false;
		//update combo sujeto current
		bool myBool = updateComboSujetoCurrent();
		combo_person_current.Sensitive = false;
		
		//if there are persons
		if(myBool) {
			//activate the gui for persons in main window
			sensitiveGuiYesPerson();
		}
	}
	
	
	private void on_delete_session_activate (object o, EventArgs args) {
		Console.WriteLine("delete session");
		ConfirmWindow confirmWin = ConfirmWindow.Show(app1, Catalog.GetString("Are you sure you want to delete current session"), Catalog.GetString("and all it's jumps?"));
		confirmWin.Button_accept.Clicked += new EventHandler(on_delete_session_accepted);
	}
	
	private void on_delete_session_accepted (object o, EventArgs args) 
	{
		Console.WriteLine("session and jumps deleted");
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
		Console.WriteLine("recup. suj.");
		personRecuperateWin = PersonRecuperateWindow.Show(app1, currentSession.UniqueID);
		personRecuperateWin.Button_recuperate.Clicked += new EventHandler(on_recuperate_person_accepted);
	}

	private void on_recuperate_person_accepted (object o, EventArgs args) {
		currentPerson = personRecuperateWin.CurrentPerson;
		updateComboSujetoCurrent();
		sensitiveGuiYesPerson();
	}
		
	private void on_create_person_activate (object o, EventArgs args) {
		Console.WriteLine("nuevo suj.");
		personAddWin = PersonAddWindow.Show(app1, currentSession.UniqueID);
		personAddWin.Button_accept.Clicked += new EventHandler(on_new_person_accepted);
	}
	
	private void on_new_person_accepted (object o, EventArgs args) {
		if (personAddWin.CurrentPerson != null)
		{
			currentPerson = personAddWin.CurrentPerson;
			updateComboSujetoCurrent();
			sensitiveGuiYesPerson();
		}
	}
	
	private void on_edit_current_person_clicked (object o, EventArgs args) {
		Console.WriteLine("modify suj.");
		personModifyWin = PersonModifyWindow.Show(app1, currentSession.UniqueID, currentPerson.UniqueID);
		personModifyWin.Button_accept.Clicked += new EventHandler(on_edit_current_person_accepted);
	}
	
	private void on_edit_current_person_accepted (object o, EventArgs args) {
		if (personModifyWin.CurrentPerson != null)
		{
			currentPerson = personModifyWin.CurrentPerson;
			updateComboSujetoCurrent (currentPerson.UniqueID + ": " + currentPerson.Name);

			sensitiveGuiYesPerson();

			treeview_jumps_storeReset();
			string myText = combo_jumps.Entry.Text;
			fillTreeView_jumps(treeview_jumps, treeview_jumps_store, myText);
			//load the treeview_rj
			treeview_jumps_rj_storeReset();
			myText = combo_jumps_rj.Entry.Text;
			fillTreeView_jumps_rj(treeview_jumps_rj, treeview_jumps_rj_store, myText);

			if(createdStatsWin) {
				statsWin.FillTreeView_stats(false, true);
			}
		}
	}
	
	private void on_recuperate_persons_from_session_activate (object o, EventArgs args) {
		Console.WriteLine("recupuerate persons from other session (not implemented)");
		//personRecuperateWin = PersonRecuperateWindow.Show(app1, currentSession.UniqueID);
		//personRecuperateWin.Button_recuperate.Clicked += new EventHandler(on_recuperate_person_accepted);
	}
	
	
	private void on_delete_current_person_from_session_activate (object o, EventArgs args) {
		Console.WriteLine("delete current person from this session");
		ConfirmWindow confirmWin = ConfirmWindow.Show(app1, 
				Catalog.GetString("Are you sure you want to delete current person and all it's jumps from this session?\n(It's personal data and jumps in other sessions will remain intact)"), 
				Catalog.GetString("Current Person: ") + currentPerson.Name);

		confirmWin.Button_accept.Clicked += new EventHandler(on_delete_current_person_from_session_accepted);
	}
	
	private void on_delete_current_person_from_session_accepted (object o, EventArgs args) 
	{
		Console.WriteLine("current person and it's jumps deleted from this session");
		SqlitePersonSession.DeletePersonFromSessionAndJumps(
				currentSession.UniqueID.ToString(), currentPerson.UniqueID.ToString());
		
		treeview_jumps_storeReset();
		fillTreeView_jumps(treeview_jumps, treeview_jumps_store, allJumpsName);
		treeview_jumps_storeReset();
		fillTreeView_jumps_rj(treeview_jumps_rj, treeview_jumps_rj_store, allJumpsName);
			
		if(createdStatsWin) {
			statsWin.FillTreeView_stats(false, true);
		}
		
		bool myBool = updateComboSujetoCurrent();
		
		//if there are no persons
		if(!myBool) {
			combo_person_current.Sensitive = false;
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
		Console.WriteLine("Activated");
		statsWin = StatsWindow.Show(app1, currentSession, 
				//prefsDigitsNumber, weightStatsPercent, heightPreferred, metersSecondsPreferred);
				prefsDigitsNumber, weightStatsPercent, heightPreferred);
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

	void on_radiobutton_simulated_activate (object o, EventArgs args)
	{
		Console.WriteLine("simulated");
		simulated = true;
		SqlitePreferences.Update("simulated", simulated.ToString());
	}
	
	void on_radiobutton_serial_port_activate (object o, EventArgs args)
	{
		Console.WriteLine("serial port");
		simulated = false;
		SqlitePreferences.Update("simulated", simulated.ToString());
	}

	private void on_preferences_activate (object o, EventArgs args) {
		PreferencesWindow myWin = PreferencesWindow.Show(
				app1, prefsDigitsNumber, showHeight, showInitialSpeed, 
				askDeletion, weightStatsPercent, heightPreferred, metersSecondsPreferred);
		myWin.Button_accept.Clicked += new EventHandler(on_preferences_accepted);
	}

	private void on_preferences_accepted (object o, EventArgs args) {
		prefsDigitsNumber = Convert.ToInt32 ( SqlitePreferences.Select("digitsNumber") ); 
		
		if ( SqlitePreferences.Select("askDeletion") == "True" ) {
			askDeletion = true;
		} else {
			askDeletion = false;
		}
		
		if ( SqlitePreferences.Select("weightStatsPercent") == "True" ) {
			weightStatsPercent = true;
		} else {
			weightStatsPercent = false;
		}

		//update showHeight
		if ( SqlitePreferences.Select("showHeight") == "True" ) {
			showHeight = true;
		} else {
			showHeight = false;
		}

		//update showInitialSpeed
		if ( SqlitePreferences.Select("showInitialSpeed") == "True" ) {
			showInitialSpeed = true;
		} else {
			showInitialSpeed = false;
		}

		//update heightPreferred
		if ( SqlitePreferences.Select("heightPreferred") == "True" ) {
			heightPreferred = true;
		} else {
			heightPreferred = false;
		}

		//update metersSecondsPreferred
		if ( SqlitePreferences.Select("metersSecondsPreferred") == "True" ) {
			metersSecondsPreferred = true;
		} else {
			metersSecondsPreferred = false;
		}

		
		//... and recreate the treeview_jumps
		string myText = combo_jumps.Entry.Text;
		createTreeView_jumps (treeview_jumps);
		treeview_jumps_storeReset();
		fillTreeView_jumps(treeview_jumps, treeview_jumps_store, myText);
		
		//... and recreate the treeview_jumps_rj
		myText = combo_jumps.Entry.Text;
		createTreeView_jumps_rj (treeview_jumps_rj);
		treeview_jumps_rj_storeReset();
		fillTreeView_jumps_rj(treeview_jumps_rj, treeview_jumps_rj_store, myText);
		
		//... and recreate the treeview_runs
		myText = combo_runs.Entry.Text;
		createTreeView_runs (treeview_runs);
		treeview_runs_storeReset();
		fillTreeView_runs(treeview_runs, treeview_runs_store, myText);

		//... and recreate the treeview_runs_rj
		/*
		myText = combo_runs.Entry.Text;
		createTreeView_runs (treeview_runs);
		treeview_runs_storeReset();
		fillTreeView_runs(treeview_runs, treeview_runs_store, myText);
		*/

		
		if(createdStatsWin) {
			statsWin.PrefsDigitsNumber = prefsDigitsNumber;
			statsWin.WeightStatsPercent = weightStatsPercent;
			statsWin.HeightPreferred = heightPreferred;
			//statsWin.HeightPreferred = metersSecondsPreferred;
			
			statsWin.FillTreeView_stats(false, true);
		}
	}
	
	private void on_cancel_clicked (object o, EventArgs args) 
	{
		Console.WriteLine("Cancel");

		//this will cancel jumps or runs
		if (currentJumpType.IsRepetitive) {
			currentJumpRj.Cancel = true;
		} else {
			currentJump.Cancel = true;
		}
	}
		
	private void on_finish_clicked (object o, EventArgs args) 
	{
		Console.WriteLine("Finish (not implemented)");

		//this will finish jumps or runs
		currentJumpRj.Finish = true;
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
		currentJumpType = new JumpType(
				jumpsMoreWin.SelectedJumpType,
				jumpsMoreWin.SelectedStartIn,
				jumpsMoreWin.SelectedExtraWeight,
				false,		//isRepetitive
				false,		//jumpsLimited (false, because is not repetitive)
				0		//limitValue
				);
				
		if( ! currentJumpType.StartIn || currentJumpType.HasWeight) {
			on_jump_extra_activate(o, args);
		} else {
			on_normal_jump_activate(o, args);
		}
	}
	
	//here comes the SJ+, DJ and every jump that has weight or fall or both. Also the reactive jumps (for defining it's limited value or weight or fall)
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
			
		string myWeight = "";
		if(currentJumpType.HasWeight) {
			myWeight = jumpExtraWin.Weight + jumpExtraWin.Option;
		}
		int myFall = 0;
		if( ! currentJumpType.StartIn ) {
			myFall = jumpExtraWin.Fall;
		}
			
			
		//hide jumping buttons
		sensitiveGuiJumping();
		
		currentJump = new Jump(currentPerson.UniqueID, currentSession.UniqueID, currentJumpType.Name, myFall, myWeight,
				cp, progressBar, appbar2, app1, prefsDigitsNumber);
		
		if (simulated) {
			currentJump.Simulate(rand);
			on_jump_finished(o, args);
		}
		else {
			if( currentJumpType.StartIn ) {
				currentJump.Manage(o, args);
			} else {
				currentJump.ManageFall(o, args);
			}
			currentJump.FalseButtonFinished.Clicked += new EventHandler(on_jump_finished);
		}
	}	
	
	private void on_jump_finished (object o, EventArgs args)
	{
		currentJump.FalseButtonFinished.Clicked -= new EventHandler(on_jump_finished);
		
		if ( ! currentJump.Cancel ) {
			lastEventWasJump = true;
			lastJumpIsReactive = false;

			myTreeViewJumps.Add(currentPerson.Name, currentJump);
		
			if(createdStatsWin) {
				statsWin.FillTreeView_stats(false, false);
			}
		
			//change to page 0 of notebook if were in other
			while(notebook.CurrentPage > 0) {
				notebook.PrevPage();
			}
		}
		
		//unhide buttons that allow jumping
		sensitiveGuiJumped();
		//unhide buttons for delete last jump
		sensitiveGuiYesJump();
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
		currentJumpType = new JumpType(
				jumpsRjMoreWin.SelectedJumpType,
				jumpsRjMoreWin.SelectedStartIn,
				jumpsRjMoreWin.SelectedExtraWeight,
				true,		//isRepetitive
				jumpsRjMoreWin.SelectedLimited,
				jumpsRjMoreWin.SelectedLimitedValue
				);
		if( ! currentJumpType.StartIn || currentJumpType.HasWeight || currentJumpType.FixedValue == 0) {
			on_jump_extra_activate(o, args);
		} else {
			on_rj_accepted(o, args);
		}
	}
	
	private void on_rj_activate (object o, EventArgs args) {
		if(o == (object) button_rj_j || o == (object) rj_j) 
		{
			currentJumpType = new JumpType(
				"RJ(j)",
				false,		//startIn
				false,		//hasWeight
				true,		//isRepetitive
				true,		//jumpsLimited
				0		//limitValue
				);
			jumpExtraWin = JumpExtraWindow.Show(app1, currentJumpType);
			jumpExtraWin.Button_accept.Clicked += new EventHandler(on_rj_accepted);
		} else if(o == (object) button_rj_t || o == (object) rj_t) 
		{
			currentJumpType = new JumpType(
				"RJ(t)",
				false,		//startIn
				false,		//hasWeight
				true,		//isRepetitive
				false,		//jumpsLimited
				0		//limitValue
				);
			jumpExtraWin = JumpExtraWindow.Show(app1, currentJumpType);
			jumpExtraWin.Button_accept.Clicked += new EventHandler(on_rj_accepted);
		} else {
		}
	}

	private void on_rj_accepted (object o, EventArgs args) 
	{
		double myLimit;
		if(currentJumpType.FixedValue > 0) {
			myLimit = currentJumpType.FixedValue;
		} else {
			myLimit = jumpExtraWin.Limited;
		}

		string myWeight = "";
		if(currentJumpType.HasWeight) {
			myWeight = jumpExtraWin.Weight + jumpExtraWin.Option;
		}
		int myFall = 0;
		if( ! currentJumpType.StartIn ) {
			myFall = jumpExtraWin.Fall;
		}

		//hide jumping buttons
		sensitiveGuiJumping();
		
		currentJumpRj = new JumpRj(currentPerson.UniqueID, currentSession.UniqueID, currentJumpType.Name, myFall, myWeight, 
				myLimit, currentJumpType.JumpsLimited, 
				cp, progressBar, appbar2, app1, prefsDigitsNumber);
		
		
		//suitable for limited by jump and time
		//simulated always simulate limited by jumps
		if(simulated) {
			currentJumpRj.Simulate(rand);
			on_jump_rj_finished(o, args);
		}
		else {
			currentJumpRj.Manage(o, args);
			currentJumpRj.FalseButtonFinished.Clicked += new EventHandler(on_jump_rj_finished);
		}

	}
				
	private void on_jump_rj_finished (object o, EventArgs args) 
	{
		currentJumpRj.FalseButtonFinished.Clicked -= new EventHandler(on_jump_rj_finished);
		
		if ( ! currentJumpRj.Cancel ) {
			lastEventWasJump = true;
			lastJumpIsReactive = true;

			//if user clicked in finish earlier
			if(currentJumpRj.Finish) {
				currentJumpRj.Jumps = Util.GetNumberOfJumps(currentJumpRj.TvString);
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

			//change to page 1 of notebook if were in other
			if(notebook.CurrentPage == 0) {
				notebook.NextPage();
			}
			while(notebook.CurrentPage > 1) {
				notebook.PrevPage();
			}
		}
		
		//unhide buttons that allow jumping
		sensitiveGuiJumped();
		//unhide buttons for delete last jump
		sensitiveGuiYesJump();
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
		currentRunType = new RunType(
				runsMoreWin.SelectedRunType,	//name
				false,				//hasIntervals
				runsMoreWin.SelectedDistance,	//distance
				false,				//tracksLimited (false, because has not intervals)
				0				//fixedValue (0, because has not intervals)
				);
				
		if( currentRunType.Distance == 0 ) {
			on_run_extra_activate(o, args);
		} else {
			on_normal_run_activate(o, args);
		}
	}
	
	//here comes the free runs (and every run with distance = 0 (undefined)
	private void on_run_extra_activate (object o, EventArgs args) 
	{
		Console.WriteLine("run extra");
	
		if(o == (object) button_run_free || o == (object) menuitem_run_free) {
			currentRunType = new RunType("Free");
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
		
		//if distance can be always different in this jump,
		//show values selected in runExtraWin
		int myDistance = 0;		
		if(currentRunType.Distance == 0) {
			myDistance = runExtraWin.Distance;
		} else {
			myDistance = (int) currentRunType.Distance;
		}
		
		//hide jumping (running) buttons
		sensitiveGuiRunning();
	
		currentRun = new Run(currentPerson.UniqueID, currentSession.UniqueID, 
				currentRunType.Name, myDistance, 
				cp, progressBar, appbar2, app1, prefsDigitsNumber, metersSecondsPreferred);
		
		if (simulated) {
			currentRun.Simulate(rand);
			on_run_finished(o, args);
		}
		else {
			currentRun.Manage(o, args);
			currentRun.FalseButtonFinished.Clicked += new EventHandler(on_run_finished);
		}
	}
	
	private void on_run_finished (object o, EventArgs args)
	{
		currentRun.FalseButtonFinished.Clicked -= new EventHandler(on_run_finished);
		
		if ( ! currentRun.Cancel ) {
			lastEventWasJump = false;
			lastRunIsInterval = false;

			myTreeViewRuns.Add(currentPerson.Name, currentRun);
		
			if(createdStatsWin) {
				statsWin.FillTreeView_stats(false, false);
			}
		
			//change to page 2 of notebook if were in other
			while(notebook.CurrentPage < 2) {
				notebook.NextPage();
			}
			while(notebook.CurrentPage > 2) {
				notebook.PrevPage();
			}
		}
		
		//unhide buttons that allow jumping, running
		sensitiveGuiRunned();
		//unhide buttons for delete last jump
		sensitiveGuiYesJump();
	}


	/* ---------------------------------------------------------
	 * ----------------  RUNS EXECUTION (interval) ----------
	 *  --------------------------------------------------------
	 */

	//suitable for all runs repetitive
	private void on_normal_run_interval_activate (object o, EventArgs args) 
	{
	}
	
	private void on_button_run_interval_more_clicked (object o, EventArgs args) 
	{
	}
	
	private void on_button_run_interval_last_clicked (object o, EventArgs args) 
	{
		Console.WriteLine("button run interval last (not implemented)");
	}
	
	private void on_run_interval_extra_activate (object o, EventArgs args) 
	{
	}

	private void on_run_interval_accepted (object o, EventArgs args)
	{
		Console.WriteLine("run interval accepted (not implemented)");
	}


	/* ---------------------------------------------------------
	 * ----------------  JUMPS EDIT, DELETE, JUMP TYPE ADD -----
	 *  --------------------------------------------------------
	 */
	
	private void on_last_delete (object o, EventArgs args) {
		Console.WriteLine("delete last (jump or run)");
		
		string warningString = "";
		if(lastEventWasJump) {
			if (askDeletion) {
				int myID = myTreeViewJumps.JumpSelectedID;
				if(lastJumpIsReactive) {
					warningString = Catalog.GetString("Atention: Deleting a RJ subjump will delete all the RJ"); 
					myID = myTreeViewJumpsRj.JumpSelectedID;
				}
				confirmWinJumpRun = ConfirmWindowJumpRun.Show(app1, 
						Catalog.GetString("Do you want to delete selected jump?"), 
						warningString, "jump", myID);
				confirmWinJumpRun.Button_accept.Clicked += new EventHandler(on_last_jump_delete_accepted);
			} else {
				on_last_jump_delete_accepted(o, args);
			}
		} else {
			if (askDeletion) {
				int myID = myTreeViewRuns.RunSelectedID;
				if (lastRunIsInterval) {
					/*
					warningString = Catalog.GetString("Atention: Deleting a intervalic sub-run will delete all the run"); 
					myID = myTreeViewRunsInterval.JumpSelectedID;
					*/
				}
				confirmWinJumpRun = ConfirmWindowJumpRun.Show(app1, 
						Catalog.GetString("Do you want to delete selected run?"), 
						warningString, "run", myID);
				confirmWinJumpRun.Button_accept.Clicked += new EventHandler(on_last_run_delete_accepted);
			} else {
				on_last_run_delete_accepted(o, args);
			}
		}
	}

	private void on_last_jump_delete_accepted (object o, EventArgs args) {
		if(lastJumpIsReactive) {
			SqliteJump.RjDelete(currentJumpRj.UniqueID.ToString());
		} else {
			SqliteJump.Delete(currentJump.UniqueID.ToString());
		}
		button_last_delete.Sensitive = false ;

		appbar2.Push( Catalog.GetString("Last jump deleted") );

		if(lastJumpIsReactive) {
			myTreeViewJumpsRj.DelJump(currentJumpRj.UniqueID);
		} else {
			myTreeViewJumps.DelJump(currentJump.UniqueID);
		}

		if(createdStatsWin) {
			statsWin.FillTreeView_stats(false, false);
		}
	}

	private void on_last_run_delete_accepted (object o, EventArgs args) {
		if (lastRunIsInterval) {
			//SqliteRun.RjDelete(currentRunRj.UniqueID.ToString());
		} else {
			SqliteRun.Delete(currentRun.UniqueID.ToString());
		}
		button_last_delete.Sensitive = false ;

		appbar2.Push( Catalog.GetString("Last run deleted") );

		if (lastRunIsInterval) {
			//myTreeViewJumpsRj.DelJump(currentJumpRj.UniqueID);
		} else {
			myTreeViewRuns.DelRun(currentRun.UniqueID);
		}

		if(createdStatsWin) {
			statsWin.FillTreeView_stats(false, false);
		}
	}

	private void on_edit_selected_jump_clicked (object o, EventArgs args) {
		Console.WriteLine("Edit selected jump (normal)");
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person (check also if it's not a individual RJ, the pass the parent RJ)
		if (myTreeViewJumps.JumpSelectedID > 0) {
			//3.- obtain the data of the selected jump
			Jump myJump = SqliteJump.SelectNormalJumpData( myTreeViewJumps.JumpSelectedID );
			Console.WriteLine(myJump);
		
			//4.- edit this jump
			editJumpWin = EditJumpWindow.Show(app1, myJump);
			editJumpWin.Button_accept.Clicked += new EventHandler(on_edit_selected_jump_accepted);
		}
	}
	
	private void on_edit_selected_jump_rj_clicked (object o, EventArgs args) {
		Console.WriteLine("Edit selected jump (RJ)");
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person (check also if it's not a individual RJ, the pass the parent RJ)
		if (myTreeViewJumpsRj.JumpSelectedID > 0) {
			//3.- obtain the data of the selected jump
			JumpRj myJump = SqliteJump.SelectRjJumpData( myTreeViewJumpsRj.JumpSelectedID );
			Console.WriteLine(myJump);
		
			//4.- edit this jump
			editJumpRjWin = EditJumpRjWindow.Show(app1, myJump);
			editJumpRjWin.Button_accept.Clicked += new EventHandler(on_edit_selected_jump_rj_accepted);
		}
	}
	
	private void on_edit_selected_jump_accepted (object o, EventArgs args) {
		Console.WriteLine("edit selected jump accepted");
		
		treeview_jumps_storeReset();
		fillTreeView_jumps(treeview_jumps, treeview_jumps_store, combo_jumps.Entry.Text);
	
		if(createdStatsWin) {
			statsWin.FillTreeView_stats(false, false);
		}
	}
	
	private void on_edit_selected_jump_rj_accepted (object o, EventArgs args) {
		Console.WriteLine("edit selected jump RJ accepted");
		
		treeview_jumps_rj_storeReset();
		fillTreeView_jumps_rj(treeview_jumps_rj, treeview_jumps_rj_store, combo_jumps_rj.Entry.Text);
		
		if(createdStatsWin) {
			statsWin.FillTreeView_stats(false, false);
		}
	}
	
	private void on_delete_selected_jump_clicked (object o, EventArgs args) {
		Console.WriteLine("delete selected jump (normal)");
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person
		Console.WriteLine(myTreeViewJumps.JumpSelectedID.ToString());
		if (myTreeViewJumps.JumpSelectedID > 0) {
			//3.- display confirmwindow of deletion 
			if (askDeletion) {
				confirmWinJumpRun = ConfirmWindowJumpRun.Show(app1, "Do you want to delete selected jump?", 
						"", "jump", myTreeViewJumps.JumpSelectedID);
				confirmWinJumpRun.Button_accept.Clicked += new EventHandler(on_delete_selected_jump_accepted);
			} else {
				on_delete_selected_jump_accepted(o, args);
			}
		}
	}
	
	private void on_delete_selected_jump_rj_clicked (object o, EventArgs args) {
		Console.WriteLine("delete selected (RJ) jump");
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person (check also if it's not a individual RJ, the pass the parent RJ)
		if (myTreeViewJumpsRj.JumpSelectedID > 0) {
			//3.- display confirmwindow of deletion 
			if (askDeletion) {
				confirmWinJumpRun = ConfirmWindowJumpRun.Show(app1,  Catalog.GetString("Do you want to delete selected jump?"), 
						 Catalog.GetString("Atention: Deleting a RJ subjump will delete all the RJ"), 
						 "jump", myTreeViewJumpsRj.JumpSelectedID);
				confirmWinJumpRun.Button_accept.Clicked += new EventHandler(on_delete_selected_jump_rj_accepted);
			} else {
				on_delete_selected_jump_rj_accepted(o, args);
			}
		}
	}
	
	private void on_delete_selected_jump_accepted (object o, EventArgs args) {
		Console.WriteLine("accept delete selected jump");
		
		SqliteJump.Delete( (myTreeViewJumps.JumpSelectedID).ToString() );
		
		appbar2.Push( Catalog.GetString ( "Deleted jump: " ) + myTreeViewJumps.JumpSelectedID );
		myTreeViewJumps.DelJump(myTreeViewJumps.JumpSelectedID);

		if(createdStatsWin) {
			statsWin.FillTreeView_stats(false, false);
		}
	}

	private void on_delete_selected_jump_rj_accepted (object o, EventArgs args) {
		Console.WriteLine("accept delete selected jump");
		
		SqliteJump.RjDelete(myTreeViewJumpsRj.JumpSelectedID.ToString());
		
		appbar2.Push( Catalog.GetString ( "Deleted reactive jump: " ) + myTreeViewJumpsRj.JumpSelectedID );
		myTreeViewJumpsRj.DelJump(myTreeViewJumpsRj.JumpSelectedID);

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
	 * ----------------  RUNS (no interval) EDIT, DELETE, RUN TYPE ADD -----
	 *  --------------------------------------------------------------------
	 */
	
	private void on_edit_selected_run_clicked (object o, EventArgs args) {
		Console.WriteLine("Edit selected run (normal)");
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person (check also if it's not a individual RJ, the pass the parent RJ)
		if (myTreeViewRuns.RunSelectedID > 0) {
			//3.- obtain the data of the selected run
			Run myRun = SqliteRun.SelectNormalRunData( myTreeViewRuns.RunSelectedID );
			Console.WriteLine(myRun);
		
			//4.- edit this run
			editRunWin = EditRunWindow.Show(app1, myRun);
			editRunWin.Button_accept.Clicked += new EventHandler(on_edit_selected_run_accepted);
		}
	}
	
	/*
	private void on_edit_selected_jump_rj_clicked (object o, EventArgs args) {
		Console.WriteLine("Edit selected jump (RJ)");
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person (check also if it's not a individual RJ, the pass the parent RJ)
		if (myTreeViewJumpsRj.JumpSelectedID > 0) {
			//3.- obtain the data of the selected jump
			JumpRj myJump = SqliteJump.SelectRjJumpData( myTreeViewJumpsRj.JumpSelectedID );
			Console.WriteLine(myJump);
		
			//4.- edit this jump
			editJumpRjWin = EditJumpRjWindow.Show(app1, myJump);
			editJumpRjWin.Button_accept.Clicked += new EventHandler(on_edit_selected_jump_rj_accepted);
		}
	}
	*/
	
	private void on_edit_selected_run_accepted (object o, EventArgs args) {
		Console.WriteLine("edit selected run accepted");
		
		treeview_runs_storeReset();
		fillTreeView_runs(treeview_runs, treeview_runs_store, combo_runs.Entry.Text);
		
		if(createdStatsWin) {
			statsWin.FillTreeView_stats(false, false);
		}
	}
	
	/*
	private void on_edit_selected_jump_rj_accepted (object o, EventArgs args) {
		Console.WriteLine("edit selected jump RJ accepted");
		
		treeview_jumps_rj_storeReset();
		fillTreeView_jumps_rj(treeview_jumps_rj, treeview_jumps_rj_store, combo_jumps_rj.Entry.Text);
		
		if(statsAutomatic) {
			fillTreeView_stats(false);
		}
	}
	*/

	private void on_delete_selected_run_clicked (object o, EventArgs args) {
		Console.WriteLine("delete selected run (normal)");
		
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
	}
		
	
	/*
	private void on_delete_selected_jump_rj_clicked (object o, EventArgs args) {
		Console.WriteLine("delete selected (RJ) jump");
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person (check also if it's not a individual RJ, the pass the parent RJ)
		if (myTreeViewJumpsRj.JumpSelectedID > 0) {
			//3.- display confirmwindow of deletion 
			if (askDeletion) {
				confirmWinJumpRun = ConfirmWindowJumpRun.Show(app1,  Catalog.GetString("Do you want to delete selected jump?"), 
						 Catalog.GetString("Atention: Deleting a RJ subjump will delete all the RJ"), 
						 "jump", myTreeViewJumpsRj.JumpSelectedID);
				confirmWinJumpRun.Button_accept.Clicked += new EventHandler(on_delete_selected_jump_rj_accepted);
			} else {
				Console.WriteLine("accept delete selected jump");
				SqliteJump.RjDelete(myTreeViewJumpsRj.JumpSelectedID.ToString());
				myTreeViewJumpsRj.DelJump(myTreeViewJumpsRj.JumpSelectedID);

				if(statsAutomatic) {
					fillTreeView_stats(false);
				}
			}
		}
	}
	*/

	private void on_delete_selected_run_accepted (object o, EventArgs args) {
		Console.WriteLine("accept delete selected run");
		
		SqliteRun.Delete( (myTreeViewRuns.RunSelectedID).ToString() );
		
		appbar2.Push( Catalog.GetString ( "Deleted run: " ) + myTreeViewRuns.RunSelectedID );
	
		myTreeViewRuns.DelRun(myTreeViewRuns.RunSelectedID);

		if(createdStatsWin) {
			statsWin.FillTreeView_stats(false, false);
		}
	}

	/*
	private void on_delete_selected_jump_rj_accepted (object o, EventArgs args) {
		Console.WriteLine("accept delete selected jump");
		treeview_jumps_rj_storeReset();
		//fillTreeView_jumps_rj(treeview_jumps_rj, treeview_jumps_rj_store);
		//FIXME:---------------- WORKS?
		myTreeViewJumpsRj.DelJump(myTreeViewJumpsRj.JumpSelectedID);

		if(statsAutomatic) {
			fillTreeView_stats(false);
		}
	}
	*/
	
	private void on_run_type_add_activate (object o, EventArgs args) {
		Console.WriteLine("Add new run type");
			
		//runTypeAddWin = RunTypeAddWindow.Show(app1);
		//runTypeAddWin.Button_accept.Clicked += new EventHandler(on_run_type_add_accepted);
	}
	
	private void on_run_type_add_accepted (object o, EventArgs args) {
		Console.WriteLine("ACCEPTED Add new run type");
		//updateComboRuns();
		//updateComboJumpsRj();
	}

	/* ---------------------------------------------------------------------
	 * ----------------  RUNS (no interval) EDIT, DELETE, RUN TYPE ADD -----
	 *  --------------------------------------------------------------------
	 */
	
	private void on_edit_selected_run_interval_clicked (object o, EventArgs args) {
		Console.WriteLine("Edit selected run interval");
	}

	private void on_delete_selected_run_interval_clicked (object o, EventArgs args) {
		Console.WriteLine("delete selected run interval");
	}

	private void on_run_interval_type_add_activate (object o, EventArgs args) {
		Console.WriteLine("Add new run interval type");
	}
	

		
	/* ---------------------------------------------------------
	 * ----------------  SOME MORE CALLBACKS---------------------
	 *  --------------------------------------------------------
	 */
	
	
	//help
	private void on_about1_activate (object o, EventArgs args) {
		new Gnome.About (
				progname, progversion,
				"(C) 2005 Xavier de  Blas, Juan Gonzalez",
				"Vertical jump analysis with contact platform.",
				authors, null, null, null).Run();
	}

	/* ---------------------------------------------------------
	 * ----------------  SENSITIVE GUI METHODS-------------------
	 *  --------------------------------------------------------
	 */
	
	private void sensitiveGuiNoSession () 
	{
		//menuitems
		preferences.Sensitive = false;
		menuitem_export_csv.Sensitive = false;
		menuitem_export_xml.Sensitive = false;
		menuitem_recuperate_person.Sensitive = false;
		menuitem_recuperate_persons_from_session.Sensitive = false;
		create_person.Sensitive = false;
		combo_person_current.Sensitive = false;
		menuitem_edit_session.Sensitive = false;
		menuitem_delete_session.Sensitive = false;
		
		menu_persons.Sensitive = false;
		menu_jumps.Sensitive = false;
		menu_runs.Sensitive = false;
		menu_view.Sensitive = false;

		button_recup_per.Sensitive = false;
		button_create_per.Sensitive = false;
		label_current_person.Sensitive = false;
		button_edit_current_person.Sensitive = false;
		menuitem_delete_current_person_from_session.Sensitive = false;
		
		//notebook
		notebook.Sensitive = false;
		hbox_jumps.Sensitive = false;
		hbox_jumps_rj.Sensitive = false;
		
		button_last_delete.Sensitive = false;
		
		//other
		button_cancel.Sensitive = false;
		button_finish.Sensitive = false;
	}
	
	private void sensitiveGuiYesSession () {
		button_recup_per.Sensitive = true;
		button_create_per.Sensitive = true;
		label_current_person.Sensitive = true;
		
		preferences.Sensitive = true;
		menuitem_export_csv.Sensitive = true;
		menuitem_export_xml.Sensitive = false; //it's not coded yet
		menuitem_recuperate_person.Sensitive = true;
		menuitem_recuperate_persons_from_session.Sensitive = true;
		create_person.Sensitive = true;
		menuitem_edit_session.Sensitive = true;
		menuitem_delete_session.Sensitive = true;
		menu_persons.Sensitive = true;
	}

	//only called by delete person functions (if we run out of persons)
	private void sensitiveGuiNoPerson () {
		notebook.Sensitive = false;
		combo_person_current.Sensitive = false;
		button_edit_current_person.Sensitive = false;
		menuitem_edit_current_person.Sensitive = false;
		menuitem_delete_current_person_from_session.Sensitive = false;
		
		menu_jumps.Sensitive = false;
		menu_runs.Sensitive = false;
		menu_view.Sensitive = false;
		
		jump_type_add.Sensitive = false;
		button_last_delete.Sensitive = false;
		
		hbox_jumps.Sensitive = false;
		hbox_jumps_rj.Sensitive = false;
		//don't allow repeat last jump
		button_last.Sensitive = false;
		button_rj_last.Sensitive = false;
		button_run_last.Sensitive = false;
		button_run_interval_last.Sensitive = false;

		menuitem_edit_selected_jump.Sensitive = false;
		menuitem_delete_selected_jump.Sensitive = false;
		button_edit_selected_jump.Sensitive = false;
		button_delete_selected_jump.Sensitive = false;
		menuitem_edit_selected_jump_rj.Sensitive = false;
		menuitem_delete_selected_jump_rj.Sensitive = false;
		button_edit_selected_jump_rj.Sensitive = false;
		button_delete_selected_jump_rj.Sensitive = false;
		menuitem_edit_selected_run.Sensitive = false;
		menuitem_delete_selected_run.Sensitive = false;
		button_edit_selected_run.Sensitive = false;
		button_delete_selected_run.Sensitive = false;
		
		checkbutton_sort_by_type.Sensitive = false;
		combo_jumps.Sensitive = false;
		combo_jumps_rj.Sensitive = false;
		combo_runs.Sensitive = false;
	}
	
	private void sensitiveGuiYesPerson () {
		notebook.Sensitive = true;
		combo_person_current.Sensitive = true;
		button_edit_current_person.Sensitive = true;
		menuitem_edit_current_person.Sensitive = true;
		menuitem_delete_current_person_from_session.Sensitive = true;
		
		menu_jumps.Sensitive = true;
		menu_runs.Sensitive = true;
		menu_view.Sensitive = true;
		
		jump_type_add.Sensitive = true;
		button_last_delete.Sensitive = true;
		
		hbox_jumps.Sensitive = true;
		hbox_jumps_rj.Sensitive = true;
		//don't allow repeat last jump
		button_last.Sensitive = false;
		button_rj_last.Sensitive = false;
		button_run_last.Sensitive = false;
		button_run_interval_last.Sensitive = false;

		menuitem_edit_selected_jump.Sensitive = true;
		menuitem_delete_selected_jump.Sensitive = true;
		button_edit_selected_jump.Sensitive = true;
		button_delete_selected_jump.Sensitive = true;
		menuitem_edit_selected_jump_rj.Sensitive = true;
		menuitem_delete_selected_jump_rj.Sensitive = true;
		button_edit_selected_jump_rj.Sensitive = true;
		button_delete_selected_jump_rj.Sensitive = true;
		menuitem_edit_selected_run.Sensitive = true;
		menuitem_delete_selected_run.Sensitive = true;
		button_edit_selected_run.Sensitive = true;
		button_delete_selected_run.Sensitive = true;
		
		checkbutton_sort_by_type.Sensitive = true;
		combo_jumps.Sensitive = true;
		combo_jumps_rj.Sensitive = true;
		combo_runs.Sensitive = true;
	}
	
	private void sensitiveGuiYesJump () {
		button_last_delete.Sensitive = true;
		button_cancel.Sensitive = false;
		button_finish.Sensitive = false;
	}
	
	private void sensitiveGuiJumping () {
		//hbox
		hbox_jumps.Sensitive = false;
		hbox_jumps_rj.Sensitive = false;
		
		//menu
		menu_jumps.Sensitive = false;
		menu_runs.Sensitive = false;
		
		//cancel, finish jump, delete last
		button_cancel.Sensitive = true;
		if (currentJumpType.IsRepetitive) {
			button_finish.Sensitive = true;
		}
		button_last_delete.Sensitive = false;
	}
    
	private void sensitiveGuiRunning () {
		//hbox
		hbox_jumps.Sensitive = false;
		hbox_jumps_rj.Sensitive = false;
		
		//menu
		menu_jumps.Sensitive = false;
		menu_runs.Sensitive = false;
		
		//cancel, finish jump, delete last
		button_cancel.Sensitive = true;
		if (currentRunType.HasIntervals) {
			button_finish.Sensitive = true;
		}
		button_last_delete.Sensitive = false;
	}
    
	private void sensitiveGuiJumped () {
		Console.WriteLine("GuiJumped");
		//hbox
		hbox_jumps.Sensitive = true;
		hbox_jumps_rj.Sensitive = true;

		//allow repeat last jump (check also if it wasn't cancelled)
		if(currentJumpType.IsRepetitive) {
			if(! currentJumpRj.Cancel) {
				button_rj_last.Sensitive = true;
				button_last.Sensitive = false;
			}
		} else {
			if(! currentJump.Cancel) {
				button_last.Sensitive = true;
				button_rj_last.Sensitive = false;
			}
		}
		
		//menu
		menu_jumps.Sensitive = true;
		menu_runs.Sensitive = true;
		
		//cancel, finish jump
		button_cancel.Sensitive = false;
		button_finish.Sensitive = false;
	}
	
	private void sensitiveGuiRunned () {
		Console.WriteLine("GuiRunned");
		//hbox
		hbox_jumps.Sensitive = true;
		hbox_jumps_rj.Sensitive = true;

		//allow repeat last run (check also if it wasn't cancelled)
		if(currentRunType.HasIntervals) {
			/*
			if(! currentRunInterval.Cancel) {
				button_rj_last.Sensitive = true;
				button_last.Sensitive = false;
			}
			*/
		} else {
			if(! currentRun.Cancel) {
				button_run_last.Sensitive = true;
				button_run_interval_last.Sensitive = false;
			}
		}
		
		//menu
		menu_jumps.Sensitive = true;
		menu_runs.Sensitive = true;
		
		//cancel, finish jump
		button_cancel.Sensitive = false;
		button_finish.Sensitive = false;
	}

}
