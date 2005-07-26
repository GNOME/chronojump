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
	[Widget] Gtk.TreeView treeview_stats;
	[Widget] Gtk.Box hbox_combo_jumps;
	[Widget] Gtk.Box hbox_combo_jumps_rj;
	[Widget] Gtk.Box hbox_combo_runs;
	[Widget] Gtk.Box hbox_combo_stats_stat_name;
	[Widget] Gtk.Box hbox_combo_stats_stat_name2;
	[Widget] Gtk.Box hbox_combo_person_current;
	[Widget] Gtk.Box vbox_jumps;
	[Widget] Gtk.Box vbox_jumps_rj;
	[Widget] Gtk.Combo combo_jumps;
	[Widget] Gtk.Combo combo_jumps_rj;
	[Widget] Gtk.Combo combo_runs;
	[Widget] Gtk.Combo combo_stats_stat_name;
	[Widget] Gtk.Combo combo_stats_stat_name2;
	[Widget] Gtk.Combo combo_person_current;

	[Widget] Gtk.CheckButton checkbutton_sort_by_type;
	[Widget] Gtk.CheckButton checkbutton_sort_by_type_rj;
	[Widget] Gtk.CheckButton checkbutton_stats_sex;
	[Widget] Gtk.CheckButton checkbutton_stats_always;
	[Widget] Gtk.CheckButton checkbutton_show_enunciate;
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
	bool statsAutomatic = true;
	bool statsColumnsToRemove = false;

	//widgets for enable or disable
	[Widget] Gtk.Button button_new;
	[Widget] Gtk.Button button_open;
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
	
	[Widget] Gtk.Button button_last_delete;
	[Widget] Gtk.Button button_stats;
	[Widget] Gtk.MenuItem preferences;
	[Widget] Gtk.MenuItem menuitem_export_csv;
	[Widget] Gtk.MenuItem menuitem_export_xml;
	[Widget] Gtk.MenuItem recuperate_person;
	[Widget] Gtk.MenuItem create_person;
			
	[Widget] Gtk.MenuItem menu_jumps;
	[Widget] Gtk.MenuItem menu_runs;

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

	[Widget] Gtk.Button button_edit_current_person;
	[Widget] Gtk.MenuItem menuitem_edit_current_person;
	[Widget] Gtk.Button button_cancel;
	[Widget] Gtk.Button button_finish;
	[Widget] Gtk.RadioButton radiobutton_current_session;
	[Widget] Gtk.RadioButton radiobutton_selected_sessions;
	[Widget] Gtk.Button button_stats_select_sessions;
	[Widget] Gtk.RadioButton radiobutton_stats_jumps_all;
	[Widget] Gtk.RadioButton radiobutton_stats_jumps_limit;
	[Widget] Gtk.SpinButton spin_stats_jumps_limit;
	[Widget] Gtk.RadioButton radiobutton_stats_jumps_person_bests;
	[Widget] Gtk.SpinButton spin_stats_jumps_person_bests;
	[Widget] Gtk.RadioButton radiobutton_stats_jumps_person_average;
	[Widget] Gtk.Button button_graph;
	
	
	[Widget] Gtk.TextView textview_enunciate;
	[Widget] Gtk.ScrolledWindow scrolledwindow_enunciate;

	[Widget] Gtk.RadioMenuItem menuitem_simulated;
	[Widget] Gtk.RadioMenuItem menuitem_serial_port;
	
	[Widget] Gtk.Frame frame_jumpers;
	[Widget] Gtk.Frame frame_stats;
	[Widget] Gtk.Notebook notebook;

	private Random rand;
	
	private static string [] authors = {"Xavier de Blas", "Juan Gonzalez"};
	private static string progversion = "0.1";
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
	private static string [] comboStatsOptions = {
		Catalog.GetString("Global"), 
		Catalog.GetString("Jumper"),
		Catalog.GetString("Simple"),
		Catalog.GetString("With TC"),
		Catalog.GetString("Reactive"),
		Catalog.GetString("Indexes")
	};
	
	private static string [] comboStats2ReactiveOptions = {
		Catalog.GetString("RJ Average Index"), 
		Catalog.GetString("POTENCY (Bosco)"), // 9.81^2*TV*TT / (4*jumps*(TT-TV))
		Catalog.GetString("RJ Evolution") 
	};
	
	private static string [] comboStats2IndexesOptions = {
		Catalog.GetString("IE (cmj-sj)*100/sj"), 
		Catalog.GetString("IUB (abk-cmj)*100/cmj")
	};

	//preferences variables
	private static int prefsDigitsNumber;
	private static bool showHeight;
	private static bool showInitialSpeed;
	private static bool simulated;
	private static bool askDeletion;
	private static bool weightStatsPercent;
	private static bool heightPreferred;

	//currentPerson currentSession currentJump
	private static Person currentPerson;
	private static Session currentSession;
	private static Jump currentJump;
	private static JumpRj currentJumpRj;
	private static bool lastEventWasJump; //if last event was Jump (true) or Run (false)
	private static bool lastJumpIsReactive; //if last Jump is reactive or not
	private static JumpType currentJumpType;

	//windows needed
	SessionAddWindow sessionAddWin;
	SessionLoadWindow sessionLoadWin;
	SessionSelectStatsWindow sessionSelectStatsWin;
	PersonRecuperateWindow personRecuperateWin; 
	PersonAddWindow personAddWin; 
	PersonModifyWindow personModifyWin; 
	JumpsMoreWindow jumpsMoreWin;
	JumpsRjMoreWindow jumpsRjMoreWin;
	JumpExtraWindow jumpExtraWin; //for normal and repetitive jumps 
	EditJumpWindow editJumpWin;
	EditJumpRjWindow editJumpRjWin;
	JumpTypeAddWindow jumpTypeAddWin;
	//RunTypeAddWindow runTypeAddWin;
	ConfirmWindow confirmWin;		//for go up or down the platform, and for 
						//export in a not-newly-created file
	ConfirmWindowJump confirmWinJump;	//for deleting jumps and RJ jumps
		
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

	//selected sessions
	ArrayList selectedSessions;

	//useful for deleting headers of lastStat just before making a new Stat
	private Stat myStat; 

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
		createComboStats();
		createComboStats2();
		createComboSujetoCurrent();

		myStat = new Stat(); //create and instance of myStat
		
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
	 * ----------------  TREEVIEW STATS ------------------------
	 *  --------------------------------------------------------
	 */
	
	private void statsRemoveColumns() {
		Gtk.TreeViewColumn [] myColumns = treeview_stats.Columns;
		foreach (Gtk.TreeViewColumn column in myColumns) {
			treeview_stats.RemoveColumn (column);
		}
	}

	private void fillTreeView_stats (bool graph) 
	{
		string category = combo_stats_stat_name.Entry.Text;
		string statistic = combo_stats_stat_name2.Entry.Text;

		if(statsColumnsToRemove && !graph) {
			statsRemoveColumns();
		}
		statsColumnsToRemove = true;
	
		int statsJumpsType = 0;
		int limit = -1;
		if (radiobutton_stats_jumps_all.Active) {
			statsJumpsType = 0;
			limit = -1; //FIXME: this changed form 0 to -1, check problems in stats.cs
		} else if (radiobutton_stats_jumps_limit.Active) {
			statsJumpsType = 1;
			limit = Convert.ToInt32 ( spin_stats_jumps_limit.Value ); 
		} else if (radiobutton_stats_jumps_person_bests.Active) {
			statsJumpsType = 2;
			limit = Convert.ToInt32 ( spin_stats_jumps_person_bests.Value ); 
		} else {
			statsJumpsType = 3;
			limit = -1; //FIXME: this changed form 0 to -1, check problems in stats.cs
		}

		//we use sendSelectedSessions for not losing selectedSessions ArrayList 
		//everytime user cicles the sessions select radiobuttons
		ArrayList sendSelectedSessions = new ArrayList(2);
		if (radiobutton_current_session.Active) {
			sendSelectedSessions.Add (currentSession.UniqueID + ":" + currentSession.Name + ":" + currentSession.Date); 
		} else if (radiobutton_selected_sessions.Active) {
			sendSelectedSessions = selectedSessions;
		}

		
		if ( category == Catalog.GetString("Global") ) {
			int jumperID = -1; //all jumpers
			string jumperName = ""; //all jumpers
			if(graph) {
				myStat = new GraphGlobal(
						sendSelectedSessions, 
						jumperID, jumperName, 
						prefsDigitsNumber, checkbutton_stats_sex.Active,  
						statsJumpsType, heightPreferred 
						);
				myStat.PrepareData();
				myStat.CreateGraph();
			} else {
				myStat = new StatGlobal(treeview_stats, 
						sendSelectedSessions, 
						jumperID, jumperName, 
						prefsDigitsNumber, checkbutton_stats_sex.Active,  
						statsJumpsType, heightPreferred 
						);
				myStat.PrepareData();
			}
		}
		else if (category == Catalog.GetString("Jumper"))
		{
			int jumperID = Convert.ToInt32(fetchID(statistic));
			string jumperName = fetchName(statistic);
			if(graph) {
				myStat = new GraphGlobal(
						sendSelectedSessions, 
						jumperID, jumperName, 
						prefsDigitsNumber, checkbutton_stats_sex.Active,  
						statsJumpsType, heightPreferred 
						);
				myStat.PrepareData();
				myStat.CreateGraph();
			}
			else {
				myStat = new StatGlobal(treeview_stats, 
						sendSelectedSessions, 
						jumperID, jumperName, 
						prefsDigitsNumber, checkbutton_stats_sex.Active,  
						statsJumpsType, heightPreferred  
						);
				myStat.PrepareData();
			}
		}
		else if(category == Catalog.GetString("Simple"))
		{
			JumpType myType = new JumpType(statistic);
			if(myType.HasWeight) {
				if(graph) {
					myStat = new GraphSjCmjAbkPlus ( 
							sendSelectedSessions, 
							prefsDigitsNumber, statistic, 
							checkbutton_stats_sex.Active, 
							statsJumpsType,
							limit,
							weightStatsPercent, 
							heightPreferred 
							);
					myStat.PrepareData();
					myStat.CreateGraph();
				} else {
					myStat = new StatSjCmjAbkPlus (treeview_stats, 
							sendSelectedSessions, 
							prefsDigitsNumber, statistic, 
							checkbutton_stats_sex.Active, 
							statsJumpsType,
							limit,
							weightStatsPercent, 
							heightPreferred
							);
					myStat.PrepareData();
				}
			} else {
				if(graph) {
					myStat = new GraphSjCmjAbk ( 
							sendSelectedSessions, 
							prefsDigitsNumber, statistic, 
							checkbutton_stats_sex.Active, 
							statsJumpsType,
							limit,
							heightPreferred
							);
					myStat.PrepareData();
					myStat.CreateGraph();
				} else {
					myStat = new StatSjCmjAbk (treeview_stats, 
							sendSelectedSessions, 
							prefsDigitsNumber, statistic, 
							checkbutton_stats_sex.Active, 
							statsJumpsType,
							limit,
							heightPreferred
							);
					myStat.PrepareData();
				}
			}
		}
		else if(category == Catalog.GetString("With TC"))
		{
			if(graph) {
				myStat = new GraphDjIndex ( 
						sendSelectedSessions, 
						prefsDigitsNumber, statistic, 
						checkbutton_stats_sex.Active, 
						statsJumpsType,
						limit//,
						//heightPreferred
						);
				myStat.PrepareData();
				myStat.CreateGraph();
			} else {
				myStat = new StatDjIndex(treeview_stats, 
						sendSelectedSessions, 
						prefsDigitsNumber, statistic, 
						checkbutton_stats_sex.Active,
						statsJumpsType,
						limit//, 
						//heightPreferred
						);
				myStat.PrepareData();
			}
		}
		else if(category == Catalog.GetString("Reactive")) {
			if(statistic == Catalog.GetString("RJ Average Index"))
			{
				if(graph) {
					myStat = new GraphRjIndex ( 
							sendSelectedSessions, 
							prefsDigitsNumber, checkbutton_stats_sex.Active, 
							statsJumpsType,
							limit);
					myStat.PrepareData();
					myStat.CreateGraph();
				} else {
					myStat = new StatRjIndex(treeview_stats, 
							sendSelectedSessions, 
							prefsDigitsNumber, checkbutton_stats_sex.Active,
							statsJumpsType,
							limit);
					myStat.PrepareData();
				}
			}	
			else if(statistic == Catalog.GetString("POTENCY (Bosco)"))
			{
				if(graph) {
					myStat = new GraphRjPotencyBosco ( 
							sendSelectedSessions, 
							prefsDigitsNumber, checkbutton_stats_sex.Active, 
							statsJumpsType,
							limit);
					myStat.PrepareData();
					myStat.CreateGraph();
				} else {
					myStat = new StatRjPotencyBosco(treeview_stats, 
							sendSelectedSessions, 
							prefsDigitsNumber, checkbutton_stats_sex.Active, 
							statsJumpsType,
							limit);
					myStat.PrepareData();
				}
			}
			else if(statistic == Catalog.GetString("RJ Evolution"))
			{
				if(graph) {
					myStat = new GraphRjEvolution ( 
							sendSelectedSessions, 
							prefsDigitsNumber, checkbutton_stats_sex.Active, 
							statsJumpsType,
							limit);
					myStat.PrepareData();
					myStat.CreateGraph();
				} else {
					myStat = new StatRjEvolution(treeview_stats, 
							sendSelectedSessions, 
							prefsDigitsNumber, checkbutton_stats_sex.Active, 
							statsJumpsType,
							limit);
					myStat.PrepareData();
				}
			}
		}
		else if(category == Catalog.GetString("Indexes")) {
			if(statistic == Catalog.GetString("IE (cmj-sj)*100/sj"))
			{
				if(graph) {
					myStat = new GraphIeIub ( 
							sendSelectedSessions, 
							"IE",
							prefsDigitsNumber, checkbutton_stats_sex.Active, 
							statsJumpsType,
							limit);
					myStat.PrepareData();
					myStat.CreateGraph();
				} else {
					myStat = new StatIeIub(treeview_stats, 
							sendSelectedSessions,
							"IE", 
							prefsDigitsNumber, checkbutton_stats_sex.Active, 
							statsJumpsType,
							limit);
					myStat.PrepareData();
				}
			}
			else if(statistic == Catalog.GetString("IUB (abk-cmj)*100/cmj"))
			{
				if(graph) {
					myStat = new GraphIeIub ( 
							sendSelectedSessions, 
							"IUB",
							prefsDigitsNumber, checkbutton_stats_sex.Active, 
							statsJumpsType,
							limit);
					myStat.PrepareData();
					myStat.CreateGraph();
				} else {
					myStat = new StatIeIub(treeview_stats, 
							sendSelectedSessions,
							"IUB", 
							prefsDigitsNumber, checkbutton_stats_sex.Active, 
							statsJumpsType,
							limit);
					myStat.PrepareData();
				}
			}
		}

		//show enunciate of the stat in textview_enunciate
		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.SetText(myStat.ToString());
		textview_enunciate.Buffer = tb;
	}

	private void on_button_stats_clicked (object o, EventArgs args) {
		fillTreeView_stats(false);
	}

	private void on_button_graph_clicked (object o, EventArgs args) {
		fillTreeView_stats(true);
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
	
	private void createComboStats() {
		combo_stats_stat_name = new Combo ();
		combo_stats_stat_name.PopdownStrings = comboStatsOptions;
		
		combo_stats_stat_name.DisableActivate ();
		combo_stats_stat_name.Entry.Changed += new EventHandler (on_combo_stats_stat_name_changed);

		hbox_combo_stats_stat_name.PackStart(combo_stats_stat_name, false, false, 0);
		hbox_combo_stats_stat_name.ShowAll();
		
		combo_stats_stat_name.Sensitive = false;
	}
	
	private void createComboStats2() {
		combo_stats_stat_name2 = new Combo ();
		//combo_stats_stat_name2.PopdownStrings = comboStatsOptions;
		
		combo_stats_stat_name2.DisableActivate ();
		combo_stats_stat_name2.Entry.Changed += new EventHandler (on_combo_stats_stat_name2_changed);

		hbox_combo_stats_stat_name2.PackStart(combo_stats_stat_name2, false, false, 0);
		hbox_combo_stats_stat_name2.ShowAll();
		
		combo_stats_stat_name2.Sensitive = false;
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

	private void updateComboStats2() {
		if(combo_stats_stat_name.Entry.Text == Catalog.GetString("Global") ) 
		{
			string [] nullOptions = { "-" };
			combo_stats_stat_name2.PopdownStrings = nullOptions;
			combo_stats_stat_name2.Sensitive = false;
		}
		else if(combo_stats_stat_name.Entry.Text == Catalog.GetString("Jumper") )
		{
			combo_stats_stat_name2.PopdownStrings = 
				SqlitePersonSession.SelectCurrentSession(currentSession.UniqueID);
			combo_stats_stat_name2.Sensitive = true;
		} else if (combo_stats_stat_name.Entry.Text == Catalog.GetString("Simple") ) 
		{
			combo_stats_stat_name2.PopdownStrings = 
				SqliteJumpType.SelectJumpTypes("", "nonTC", true); //only select name
			combo_stats_stat_name2.Sensitive = true;
		} else if (combo_stats_stat_name.Entry.Text == Catalog.GetString("With TC") ) 
		{
			combo_stats_stat_name2.PopdownStrings = 
				SqliteJumpType.SelectJumpTypes("", "TC", true); //only select name
			combo_stats_stat_name2.Sensitive = true;
		} else if (combo_stats_stat_name.Entry.Text == Catalog.GetString("Reactive") ) 
		{
			combo_stats_stat_name2.PopdownStrings = comboStats2ReactiveOptions;
			combo_stats_stat_name2.Sensitive = true;
		} else if (combo_stats_stat_name.Entry.Text == Catalog.GetString("Indexes") ) 
		{
			combo_stats_stat_name2.PopdownStrings = comboStats2IndexesOptions;
			combo_stats_stat_name2.Sensitive = true;
		}

		fillTreeView_stats(false);
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
			if(myText != "") {
			//if people modify the values in the combo_person_current, and this valeus are not correct, 
			//let's update the combosujetocurrent
			if(SqlitePersonSession.PersonSelectExistsInSession(fetchID(myText), currentSession.UniqueID)) 
			{
				currentPerson = SqlitePersonSession.PersonSelect(fetchID(myText));

				//if stats "jumper" are selected, and stats are automatic, 
				//update stats when current person change
				if(combo_stats_stat_name.Entry.Text == "Jumper" && statsAutomatic) {
					fillTreeView_stats(false);
				}
			}
			else {
				//bool myBool = updateComboSujetoCurrent();
				updateComboSujetoCurrent();
			}
		}
	}

	private string fetchID (string text)
	{
		string [] myStringFull = text.Split(new char[] {':'});
		return myStringFull[0];
	}
	
	private string fetchName (string text)
	{
		//"id: name" (return only name)
		bool found = false;
		int i;
		for (i=0; ! found ; i++) {
			if(text[i] == ' ') {
				found = true;
			}
		}
		return text.Substring(i);
	}
	
	
	/* ---------------------------------------------------------
	 * ----------------  STATS CALLBACKS--------------------
	 *  --------------------------------------------------------
	 */
	
	private void on_checkbutton_stats_always_clicked(object o, EventArgs args) {
		if (statsAutomatic) { 
			statsAutomatic = false; 
			button_stats.Sensitive = true;
			Console.WriteLine("stats AUTO");
		}
		else { 
			statsAutomatic = true; 
			button_stats.Sensitive = false;
			fillTreeView_stats(false);
			Console.WriteLine("stats NO AUTO");
		}
		
	}
	
	private void on_checkbutton_show_enunciate_clicked(object o, EventArgs args) {
		if (checkbutton_show_enunciate.Active) {
			textview_enunciate.Show();
			scrolledwindow_enunciate.Show();
		} else {
			textview_enunciate.Hide();
			scrolledwindow_enunciate.Hide();
		}
	}

	private void update_stats_widgets_sensitiveness() {
		string category = combo_stats_stat_name.Entry.Text;
		string statistic = combo_stats_stat_name2.Entry.Text;
		if(category == "" || statistic == "") {
			//for an unknown reason, when we select an option in the combo stats, 
			//the on_combo_stats_stat_name_changed it's called two times? 
			//in the first the value of Entry.Text is "";
			return;
		} else {
			//some stats should not be showed as limited jumps
			if(category == Catalog.GetString("Reactive") && statistic == Catalog.GetString("RJ Evolution") ) {
				//don't allow RJ Evolution and multisession
				radiobutton_current_session.Active = true;
				radiobutton_selected_sessions.Sensitive = false;
				//has no sense to study the AVG of rj tv tc evolution string
				//nota fair to make avg of each subjump, 
				//specially when some RJs have more jumps than others
				if(radiobutton_stats_jumps_person_average.Active) {
					radiobutton_stats_jumps_person_bests.Active = true;
				}
				radiobutton_stats_jumps_person_average.Sensitive = false;
			}
			else if(category == Catalog.GetString("Global") || category == Catalog.GetString("Jumper") || 
					category == Catalog.GetString("Indexes") || 
					(selectedSessions.Count > 1 && ! radiobutton_current_session.Active) )
			{
				//change the radiobutton value
				if(radiobutton_stats_jumps_all.Active || radiobutton_stats_jumps_limit.Active) {
					radiobutton_stats_jumps_person_bests.Active = true;
					spin_stats_jumps_person_bests.Sensitive = false; //in this jumps only show the '1' best value
				}
				//make no sensitive
				radiobutton_stats_jumps_all.Sensitive = false;
				radiobutton_stats_jumps_limit.Sensitive = false;
				spin_stats_jumps_person_bests.Sensitive = false;
				
				//Not RJ Evolution (put selected_sessions_radiobutton visible, and person_average)
				radiobutton_selected_sessions.Sensitive = true;
				radiobutton_stats_jumps_person_average.Sensitive = true;
			} else {
				radiobutton_stats_jumps_all.Sensitive = true;
				radiobutton_stats_jumps_limit.Sensitive = true;
				if(radiobutton_stats_jumps_person_bests.Active) {
					spin_stats_jumps_person_bests.Sensitive = true;
				}
				//Not RJ Evolution (put selected_sessions_radiobutton visible, and person_average)
				radiobutton_selected_sessions.Sensitive = true;
				radiobutton_stats_jumps_person_average.Sensitive = true;
			}
		}
	}
	
	private void on_combo_stats_stat_name_changed(object o, EventArgs args) {
		//update combo stat2, there change the treeviewstats (with the combostats2 values changed)
		updateComboStats2();
		
		//update_stats_widgets_sensitiveness();
	}
	
	private void on_combo_stats_stat_name2_changed(object o, EventArgs args) {
		//there's no need of this:
		update_stats_widgets_sensitiveness();
			
		//for an unknown reason, when we select an option in the combo stats, 
		//the on_combo_stats_stat_name_changed it's called two times? 
		//in the first the value of Entry.Text is "";
		string myText = combo_stats_stat_name.Entry.Text;
		string myText2 = combo_stats_stat_name2.Entry.Text;
		if(myText != "" && myText2 != "")
			fillTreeView_stats(false);
	}
	
	private void on_radiobuttons_stat_session_toggled (object o, EventArgs args)
	{
		if(o == (object) radiobutton_current_session) 
		{
			Console.WriteLine("current");
			button_stats_select_sessions.Sensitive = false;
		} 
		else if (o == (object) radiobutton_selected_sessions ) 
		{
			Console.WriteLine("selected");
			button_stats_select_sessions.Sensitive = true;
		}
		update_stats_widgets_sensitiveness();
		
		//if(statsAutomatic) {
			fillTreeView_stats(false);
		//}
	}
	
	private void on_checkbutton_stats_sex_clicked(object o, EventArgs args)
	{
		//if (statsAutomatic) { 
			fillTreeView_stats(false);
		//}
	}

	void on_radiobutton_stats_jumps_clicked (object o, EventArgs args)
	{
		if (radiobutton_stats_jumps_all.Active) {
			spin_stats_jumps_limit.Sensitive = false;
			spin_stats_jumps_person_bests.Sensitive = false;
		} 
		else if (radiobutton_stats_jumps_limit.Active) {
			spin_stats_jumps_limit.Sensitive = true;
			spin_stats_jumps_person_bests.Sensitive = false;
		} 
		else if (radiobutton_stats_jumps_person_bests.Active) {
			spin_stats_jumps_limit.Sensitive = false;
			spin_stats_jumps_person_bests.Sensitive = true;
		} 
		else if (radiobutton_stats_jumps_person_average.Active) {
			spin_stats_jumps_limit.Sensitive = false;
			spin_stats_jumps_person_bests.Sensitive = false;
		}
	
		update_stats_widgets_sensitiveness();
		//if (statsAutomatic) { 
			fillTreeView_stats(false);
		//}
	}
	
	void on_spinbutton_stats_jumps_changed (object o, EventArgs args)
	{
		if (statsAutomatic) { 
			fillTreeView_stats(false);
		}
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
	 * ----------------  SESSION NEW, LOAD AND EXPORT ----------
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

			//put value in selectedSessions
			selectedSessions = new ArrayList(2);
			selectedSessions.Add(currentSession.UniqueID + ":" + currentSession.Name + ":" + currentSession.Date);
			
			//load the treeview
			treeview_jumps_storeReset();
			fillTreeView_jumps(treeview_jumps, treeview_jumps_store, allJumpsName);
			//load the treeview_rj
			treeview_jumps_rj_storeReset();
			fillTreeView_jumps_rj(treeview_jumps_rj, treeview_jumps_rj_store, allJumpsName);

			fillTreeView_stats(false);

			//show hidden widgets
			sensitiveGuiNoSession();
			sensitiveGuiYesSession();

			//for sure, jumpsExists is false, because we create a new session

			button_edit_current_person.Sensitive = false;
			menuitem_edit_current_person.Sensitive = false;
			//update combo sujeto current
			//bool myBool = updateComboSujetoCurrent();
			updateComboSujetoCurrent();
			combo_person_current.Sensitive = false;
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
		
		//put value in selectedSessions
		selectedSessions = new ArrayList(2);
		selectedSessions.Add(currentSession.UniqueID + ":" + currentSession.Name + ":" + currentSession.Date);
		
		//load the treeview_jumps
		treeview_jumps_storeReset();
		fillTreeView_jumps(treeview_jumps, treeview_jumps_store, allJumpsName);
		//load the treeview_jumps_rj
		treeview_jumps_rj_storeReset();
		fillTreeView_jumps_rj(treeview_jumps_rj, treeview_jumps_rj_store, allJumpsName);
		
		//everytime we load a session, we put stats to "Global" 
		//(if this session has no jumps, it crashes in the others statistics)
		combo_stats_stat_name.Entry.Text = comboStatsOptions[0];
		fillTreeView_stats(false);

		//show hidden widgets
		sensitiveGuiNoSession();
		sensitiveGuiYesSession();
		
		button_edit_current_person.Sensitive = false;
		menuitem_edit_current_person.Sensitive = false;
		//update combo sujeto current
		bool myBool = updateComboSujetoCurrent();
		combo_person_current.Sensitive = false;
		
		if(myBool) {
			sensitiveGuiYesPerson();
		}
	}
	
	private void on_button_stats_select_sessions_clicked (object o, EventArgs args) {
		Console.WriteLine("select sessions for stats");
		sessionSelectStatsWin = SessionSelectStatsWindow.Show(app1, selectedSessions);
		sessionSelectStatsWin.Button_accept.Clicked += new EventHandler(on_stats_select_sessions_accepted);
	}
	
	private void on_stats_select_sessions_accepted (object o, EventArgs args) {
		Console.WriteLine("select sessions for stats accepted");
		if (sessionSelectStatsWin.ArrayOfSelectedSessions[0] != "-1") { 
			//there are sessionsSelected, put them in selectedSessions ArrayList
			selectedSessions = sessionSelectStatsWin.ArrayOfSelectedSessions;
		} else {
			//there are NO sessionsSelected, put currentSession in selectedSession ArrayList
			selectedSessions = new ArrayList(2);
			selectedSessions.Add(currentSession.UniqueID + ":" + currentSession.Name + ":" + currentSession.Date);
		}

		update_stats_widgets_sensitiveness();
		//if(statsAutomatic) {
			fillTreeView_stats(false);
		//}
	}

	private void on_export_session_activate(object o, EventArgs args) {
		if (o == (object) menuitem_export_csv) {
			//ExportSessionCSV myExport = new ExportSessionCSV(currentSession, app1, appbar2);
			new ExportSessionCSV(currentSession, app1, appbar2);
		} else if (o == (object) menuitem_export_xml) {
			//ExportSessionXML myExport = new ExportSessionXML(currentSession, app1, appbar2);
			new ExportSessionXML(currentSession, app1, appbar2);
		} else {
			Console.WriteLine("Error exporting");
		}
	}

	
	/* ---------------------------------------------------------
	 * ----------------  PERSON RECUPERATE, LOAD, EDIT------
	 *  --------------------------------------------------------
	 */
	
	private void on_recuperate_person_activate (object o, EventArgs args) {
		Console.WriteLine("recup. suj.");
		personRecuperateWin = PersonRecuperateWindow.Show(app1, currentSession.UniqueID);
		personRecuperateWin.Button_recuperate.Clicked += new EventHandler(on_recuperate_person_accepted);
	}

	private void on_recuperate_person_accepted (object o, EventArgs args) {
		currentPerson = personRecuperateWin.CurrentPerson;
		//bool myBool = updateComboSujetoCurrent();
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
			//bool myBool = updateComboSujetoCurrent();
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
			//bool myBool = updateComboSujetoCurrent (currentPerson.UniqueID + ": " + currentPerson.Name);
			updateComboSujetoCurrent (currentPerson.UniqueID + ": " + currentPerson.Name);

			sensitiveGuiYesPerson();

			treeview_jumps_storeReset();
			string myText = combo_jumps.Entry.Text;
			fillTreeView_jumps(treeview_jumps, treeview_jumps_store, myText);
			//load the treeview_rj
			treeview_jumps_rj_storeReset();
			myText = combo_jumps_rj.Entry.Text;
			fillTreeView_jumps_rj(treeview_jumps_rj, treeview_jumps_rj_store, myText);

			//if(statsAutomatic) {
				fillTreeView_stats(false);
			//}
		}
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
				askDeletion, weightStatsPercent, heightPreferred);
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

		if(statsAutomatic) {
			fillTreeView_stats(false);
		}
			
	}
	//view
		
	
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
		Console.WriteLine("button last (not implemented)");
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
		
		//unhide buttons that allow jumping
		sensitiveGuiJumped();
		//unhide buttons for delete last jump
		sensitiveGuiYesJump();
		
		if ( ! currentJump.Cancel ) {
			lastEventWasJump = true;
			lastJumpIsReactive = false;

			myTreeViewJumps.Add(currentPerson.Name, currentJump);
		
			if(statsAutomatic) {
				fillTreeView_stats(false);
			}
		
			//change to page 0 of notebook if were in other
			while(notebook.CurrentPage > 0) {
				notebook.PrevPage();
			}
		}
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
		Console.WriteLine("button last rj (not implemented)");
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
		
		//unhide buttons that allow jumping
		sensitiveGuiJumped();
		//unhide buttons for delete last jump
		sensitiveGuiYesJump();
		
		if ( ! currentJumpRj.Cancel ) {
			lastEventWasJump = true;
			lastJumpIsReactive = true;

			myTreeViewJumpsRj.Add(currentPerson.Name, currentJumpRj);

			if(statsAutomatic) {
				fillTreeView_stats(false);
			}

			//change to page 1 of notebook if were in other
			if(notebook.CurrentPage == 0) {
				notebook.NextPage();
			}
			while(notebook.CurrentPage > 1) {
				notebook.PrevPage();
			}
		}
	}

	
	/* ---------------------------------------------------------
	 * ----------------  RUNS EXECUTION (no interval) ----------
	 *  --------------------------------------------------------
	 */
	
	//suitable for all runs not repetitive
	private void on_normal_run_activate (object o, EventArgs args) 
	{
	}
	
	private void on_button_run_more_clicked (object o, EventArgs args) 
	{
	}
	
	private void on_button_run_last_clicked (object o, EventArgs args) 
	{
		Console.WriteLine("button run last (not implemented)");
	}
	
	private void on_run_extra_activate (object o, EventArgs args) 
	{
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



	/* ---------------------------------------------------------
	 * ----------------  JUMPS EDIT, DELETE, JUMP TYPE ADD -----
	 *  --------------------------------------------------------
	 */
	
	private void on_last_delete (object o, EventArgs args) {
		Console.WriteLine("delete last (jump or run)");
		
		if(lastEventWasJump) {
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

			if(statsAutomatic) {
				fillTreeView_stats(false);
			}
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
		
		if(statsAutomatic) {
			fillTreeView_stats(false);
		}
	}
	
	private void on_edit_selected_jump_rj_accepted (object o, EventArgs args) {
		Console.WriteLine("edit selected jump RJ accepted");
		
		treeview_jumps_rj_storeReset();
		fillTreeView_jumps_rj(treeview_jumps_rj, treeview_jumps_rj_store, combo_jumps_rj.Entry.Text);
		
		if(statsAutomatic) {
			fillTreeView_stats(false);
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
				bool isRj = false;
				confirmWinJump = ConfirmWindowJump.Show(app1, "Do you want to delete selected jump?", 
						"", "jump", myTreeViewJumps.JumpSelectedID, isRj);
				confirmWinJump.Button_accept.Clicked += new EventHandler(on_delete_selected_jump_accepted);
			} else {
				Console.WriteLine("accept delete selected jump");
				SqliteJump.Delete(
						(myTreeViewJumps.JumpSelectedID).ToString()
						);
				appbar2.Push( Catalog.GetString ( "Deleted jump: " ) + myTreeViewJumps.JumpSelectedID );
				myTreeViewJumps.DelJump(myTreeViewJumps.JumpSelectedID);
				
				if(statsAutomatic) {
					fillTreeView_stats(false);
				}
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
				bool isRj = true;
				confirmWinJump = ConfirmWindowJump.Show(app1,  Catalog.GetString("Do you want to delete selected jump?"), 
						 Catalog.GetString("Atention: Deleting a RJ subjump will delete all the RJ"), 
						 "jump", myTreeViewJumpsRj.JumpSelectedID, isRj);
				confirmWinJump.Button_accept.Clicked += new EventHandler(on_delete_selected_jump_rj_accepted);
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
	
	private void on_delete_selected_jump_accepted (object o, EventArgs args) {
		Console.WriteLine("accept delete selected jump");
		appbar2.Push( Catalog.GetString ( "Deleted jump: " ) + myTreeViewJumps.JumpSelectedID );
		myTreeViewJumps.DelJump(myTreeViewJumps.JumpSelectedID);

		if(statsAutomatic) {
			fillTreeView_stats(false);
		}
	}

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
		/*
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
		*/
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
	/*
	private void on_edit_selected_jump_accepted (object o, EventArgs args) {
		Console.WriteLine("edit selected jump accepted");
		
		treeview_jumps_storeReset();
		fillTreeView_jumps(treeview_jumps, treeview_jumps_store, combo_jumps.Entry.Text);
		
		if(statsAutomatic) {
			fillTreeView_stats(false);
		}
	}
	
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
		/*
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person
		if (myTreeViewJumps.JumpSelectedID > 0) {
			//3.- display confirmwindow of deletion 
			if (askDeletion) {
				bool isRj = false;
				confirmWinJump = ConfirmWindowJump.Show(app1, "Do you want to delete selected jump?", 
						"", "jump", myTreeViewJumps.JumpSelectedID, isRj);
				confirmWinJump.Button_accept.Clicked += new EventHandler(on_delete_selected_jump_accepted);
			} else {
				Console.WriteLine("accept delete selected jump");
				SqliteJump.Delete(
						(myTreeViewJumps.JumpSelectedID).ToString()
						);
				appbar2.Push( Catalog.GetString ( "Deleted jump: " ) + myTreeViewJumps.JumpSelectedID );
				myTreeViewJumps.DelJump(myTreeViewJumps.JumpSelectedID);
				
				if(statsAutomatic) {
					fillTreeView_stats(false);
				}
			}
		}
		*/
	}
	
	/*
	private void on_delete_selected_jump_rj_clicked (object o, EventArgs args) {
		Console.WriteLine("delete selected (RJ) jump");
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person (check also if it's not a individual RJ, the pass the parent RJ)
		if (myTreeViewJumpsRj.JumpSelectedID > 0) {
			//3.- display confirmwindow of deletion 
			if (askDeletion) {
				bool isRj = true;
				confirmWinJump = ConfirmWindowJump.Show(app1,  Catalog.GetString("Do you want to delete selected jump?"), 
						 Catalog.GetString("Atention: Deleting a RJ subjump will delete all the RJ"), 
						 "jump", myTreeViewJumpsRj.JumpSelectedID, isRj);
				confirmWinJump.Button_accept.Clicked += new EventHandler(on_delete_selected_jump_rj_accepted);
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
	
	private void on_delete_selected_jump_accepted (object o, EventArgs args) {
		Console.WriteLine("accept delete selected jump");
		appbar2.Push( Catalog.GetString ( "Deleted jump: " ) + myTreeViewJumps.JumpSelectedID );
		myTreeViewJumps.DelJump(myTreeViewJumps.JumpSelectedID);

		if(statsAutomatic) {
			fillTreeView_stats(false);
		}
	}

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
		preferences.Sensitive = false ;
		menuitem_export_csv.Sensitive = false;
		menuitem_export_xml.Sensitive = false;
		recuperate_person.Sensitive = false ;
		create_person.Sensitive = false ;
		
		menu_jumps.Sensitive = false;
		menu_runs.Sensitive = false;

		//frame_jumpers
		frame_jumpers.Sensitive = false;
		button_recup_per.Sensitive = false ;
		button_create_per.Sensitive = false ;
		
		//notebook
		notebook.Sensitive = false;
		vbox_jumps.Sensitive = false;
		vbox_jumps_rj.Sensitive = false;
		
		button_last_delete.Sensitive = false ;
		button_stats.Sensitive = false;
	
		//frame_stats
		frame_stats.Sensitive = false;
		checkbutton_sort_by_type.Sensitive = false ;
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
		
		combo_stats_stat_name.Sensitive = false;
		button_stats_select_sessions.Sensitive = false;
		checkbutton_stats_sex.Sensitive = false;
		checkbutton_stats_always.Sensitive = false;
		checkbutton_show_enunciate.Sensitive = false;
		radiobutton_current_session.Sensitive = false;
		radiobutton_selected_sessions.Sensitive = false;
		radiobutton_stats_jumps_all.Sensitive = false;
		radiobutton_stats_jumps_limit.Sensitive = false;
		spin_stats_jumps_limit.Sensitive = false;
		radiobutton_stats_jumps_person_bests.Sensitive = false;
		spin_stats_jumps_person_bests.Sensitive = false;
		radiobutton_stats_jumps_person_average.Sensitive = false;
		button_graph.Sensitive = false;
		
		//other
		button_cancel.Sensitive = false ;
		button_finish.Sensitive = false ;
	}
	
	private void sensitiveGuiYesSession () {
		frame_jumpers.Sensitive = true;
		button_recup_per.Sensitive = true ;
		button_create_per.Sensitive = true ;
		
		preferences.Sensitive = true ;
		menuitem_export_csv.Sensitive = true;
		menuitem_export_xml.Sensitive = false; //it's not coded yet
		recuperate_person.Sensitive = true ;
		create_person.Sensitive = true ;
	}

	private void sensitiveGuiYesPerson () {
		notebook.Sensitive = true;
		combo_person_current.Sensitive = true;
		button_edit_current_person.Sensitive = true;
		menuitem_edit_current_person.Sensitive = true;
		
		menu_jumps.Sensitive = true;
		menu_runs.Sensitive = true;
		
		jump_type_add.Sensitive = true;
		button_last_delete.Sensitive = true ;
		
		vbox_jumps.Sensitive = true;
		vbox_jumps_rj.Sensitive = true;

		frame_stats.Sensitive = true;
		if(statsAutomatic) {
			button_stats.Sensitive = false;
			checkbutton_stats_always.Active = true;
		}
		combo_stats_stat_name.Sensitive = true;
		checkbutton_stats_sex.Sensitive = true;
		checkbutton_stats_always.Sensitive = true;
		checkbutton_show_enunciate.Sensitive = true;
		radiobutton_current_session.Sensitive = true;
		radiobutton_current_session.Active = true;
		radiobutton_selected_sessions.Sensitive = true;
		
		
		radiobutton_stats_jumps_person_bests.Sensitive = true;
		radiobutton_stats_jumps_person_bests.Active = true;
		radiobutton_stats_jumps_person_average.Sensitive = true;
		//not activated because it starts with the "Global" stat, where some radiobutton_jumps has no sense:
		radiobutton_stats_jumps_all.Sensitive = false;
		radiobutton_stats_jumps_limit.Sensitive = false;
		//in global stat, there's no option of "two three or more bests jumps"
		spin_stats_jumps_person_bests.Sensitive = false;
	
		
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
		
		checkbutton_sort_by_type.Sensitive = true ;
		combo_jumps.Sensitive = true;
		combo_jumps_rj.Sensitive = true;
		combo_runs.Sensitive = true;
		button_graph.Sensitive = true;
	}
	
	private void sensitiveGuiYesJump () {
		button_last_delete.Sensitive = true ;
		button_cancel.Sensitive = true ;
		button_finish.Sensitive = true ;
	}
	
	private void sensitiveGuiJumping () {
		//vbox
		vbox_jumps.Sensitive = false;
		vbox_jumps_rj.Sensitive = false;
		
		//menu
		menu_jumps.Sensitive = false;
		menu_runs.Sensitive = false;
		
		//cancel, finish jump
		button_cancel.Sensitive = true ;
		button_finish.Sensitive = true ;
	}
    
	private void sensitiveGuiJumped () {
		//vbox
		vbox_jumps.Sensitive = true;
		vbox_jumps_rj.Sensitive = true;
		
		//menu
		menu_jumps.Sensitive = true;
		menu_runs.Sensitive = true;
		
		//cancel, finish jump
		button_cancel.Sensitive = false ;
		button_finish.Sensitive = false ;
	}
}
