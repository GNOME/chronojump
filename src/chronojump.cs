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
using System.Timers; 

using System.Threading;


public class ChronoJump 
{
	[Widget] Gtk.Window app1;
	[Widget] Gnome.AppBar appbar2;
	[Widget] Gtk.TreeView treeview_jumps;
	[Widget] Gtk.TreeView treeview_jumps_rj;
	[Widget] Gtk.TreeView treeview_stats;
	[Widget] Gtk.Box hbox_combo_jumps;
	[Widget] Gtk.Box hbox_combo_jumps_rj;
	[Widget] Gtk.Box hbox_combo_stats_stat_name;
	[Widget] Gtk.Box hbox_combo_person_current;
	[Widget] Gtk.Box vbox_jumps;
	[Widget] Gtk.Box vbox_jumps_rj;
	[Widget] Gtk.Label label_current_jumper;
	[Widget] Gtk.Combo combo_jumps;
	[Widget] Gtk.Combo combo_jumps_rj;
	[Widget] Gtk.Combo combo_stats_stat_name;
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
	bool statsAutomatic = false;
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
	[Widget] Gtk.Button button_last_jump_delete;
	[Widget] Gtk.Button button_stats;
	[Widget] Gtk.MenuItem preferences;
	[Widget] Gtk.MenuItem menuitem_export_csv;
	[Widget] Gtk.MenuItem menuitem_export_xml;
	[Widget] Gtk.MenuItem recuperate_person;
	[Widget] Gtk.MenuItem create_person;
	[Widget] Gtk.MenuItem sj;
	[Widget] Gtk.MenuItem sj_plus;
	[Widget] Gtk.MenuItem cmj;
	[Widget] Gtk.MenuItem abk;
	[Widget] Gtk.MenuItem dj;
	[Widget] Gtk.MenuItem more;
	[Widget] Gtk.MenuItem more_rj;
	[Widget] Gtk.MenuItem jump_type_add;
	[Widget] Gtk.MenuItem rj_j;
	[Widget] Gtk.MenuItem rj_t;
	[Widget] Gtk.MenuItem menuitem_last_jump_delete;
	[Widget] Gtk.MenuItem ind_elasticidad;
	[Widget] Gtk.MenuItem ind_utiliz_brazos;
	[Widget] Gtk.MenuItem intrasesion;
	[Widget] Gtk.MenuItem intersesion;
	
	[Widget] Gtk.Button button_edit_current_person;
	[Widget] Gtk.MenuItem menuitem_edit_current_person;
	[Widget] Gtk.Button button_cancel_jump;
	[Widget] Gtk.MenuItem menuitem_cancel_jump;
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
	[Widget] Gtk.Notebook notebook_jumps;

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


	private static string allJumpsName = Catalog.GetString("All jumps");
	private static string [] comboStatsOptions = {
		"Global", 
		"Jumper", 
		"SJ", "SJ+", "CMJ", "ABK", 
		"DJ (TV)", 
		"DJ Index (tv-tc)*100/tc", 
		"RJ Average Index", 
		"POTENCY (Aguado)", // 9.81^2*TV*TT / (4*jumps*(TT-TV))
		"IE (cmj-sj)*100/sj",
		"IUB (abk-cmj)*100/cmj"
	};

	//preferences variables
	private static int prefsDigitsNumber;
	private static bool showHeight;
	private static bool simulated;
	private static bool askDeletion;
	private static bool weightStatsPercent;

	//currentPerson currentSession currentJump
	private static Person currentPerson;
	private static Session currentSession;
	private static Jump currentJump;
	private static JumpRj currentJumpRj;
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
	ConfirmWindow confirmWin;		//for go up or down the platform, and for 
						//export in a not-newly-created file
	ConfirmWindowJump confirmWinJump;	//for deleting jumps and RJ jumps
		
	//Thread and progress bar 
	private Thread thread;
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
	private double tcDjJump;	//used for log the jumps with previous fall
	private bool firstRjValue;
	private double rjTcCount;
	private double rjTvCount;
	private string rjTcString;
	private string rjTvString;

	//selected sessions
	ArrayList selectedSessions;

	//useful for deleting headers of lastStat just before making a new Stat
	private Stat myStat; 

	private bool cancellingJump;

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

		createComboJumps();
		createComboJumpsRj();
		createComboStats();
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
		myTreeViewJumps = new TreeViewJumps( tv, sortByType, showHeight, prefsDigitsNumber );
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
		myTreeViewJumps = new TreeViewJumps( treeview_jumps, sortByType, showHeight, prefsDigitsNumber );
	}


	/* ---------------------------------------------------------
	 * ----------------  TREEVIEW JUMPS RJ ---------------------
	 *  --------------------------------------------------------
	 */

	private void createTreeView_jumps_rj (Gtk.TreeView tv) {
		myTreeViewJumpsRj = new TreeViewJumpsRj( tv, showHeight, prefsDigitsNumber );
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
		myTreeViewJumpsRj = new TreeViewJumpsRj( treeview_jumps_rj, showHeight, prefsDigitsNumber );
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

	private void fillTreeView_stats (bool graph) {
		
		string myText = combo_stats_stat_name.Entry.Text;
		string [] fullTitle = myText.Split(new char[] {' '});

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

		
		if ( myText == "Global" || myText == "Jumper")
		{
			int jumperID = -1; //all jumpers
			string jumperName = ""; //all jumpers
			if (myText == "Jumper") {
				jumperID = currentPerson.UniqueID;
				jumperName = currentPerson.Name;
			}
	
			if (myText == "Global") {
				if(graph) {
					myStat = new GraphGlobal(
							sendSelectedSessions, 
							jumperID, jumperName, 
							prefsDigitsNumber, checkbutton_stats_sex.Active,  
							statsJumpsType 
							);
					myStat.PrepareData();
					myStat.CreateGraph();
				} else {
					myStat = new StatGlobal(treeview_stats, 
							sendSelectedSessions, 
							jumperID, jumperName, 
							prefsDigitsNumber, checkbutton_stats_sex.Active,  
							statsJumpsType 
							);
					myStat.PrepareData();
				}
			} else {
				if(graph) {
					myStat = new GraphGlobal(
						sendSelectedSessions, 
						jumperID, jumperName, 
						prefsDigitsNumber, checkbutton_stats_sex.Active,  
						statsJumpsType 
						);
					myStat.PrepareData();
					myStat.CreateGraph();
				}
				else {
					myStat = new StatGlobal(treeview_stats, 
						sendSelectedSessions, 
						jumperID, jumperName, 
						prefsDigitsNumber, checkbutton_stats_sex.Active,  
						statsJumpsType 
						);
					myStat.PrepareData();
				}
			}
		}
		else if(myText == "SJ" || myText == "CMJ" || myText == "ABK")
		{
			if(graph) {
				myStat = new GraphSjCmjAbk ( 
						sendSelectedSessions, 
						prefsDigitsNumber, fullTitle[0], checkbutton_stats_sex.Active, 
						statsJumpsType,
						limit);
				myStat.PrepareData();
				myStat.CreateGraph();
			} else {
				myStat = new StatSjCmjAbk (treeview_stats, 
						sendSelectedSessions, 
						prefsDigitsNumber, fullTitle[0], checkbutton_stats_sex.Active, 
						statsJumpsType,
						limit);
				myStat.PrepareData();
			}
		}
		else if(myText == "SJ+" || myText == "CMJ+" || myText == "ABK+")
		{
			if(graph) {
			} else {
				myStat = new StatSjCmjAbkPlus (treeview_stats, 
						sendSelectedSessions, 
						prefsDigitsNumber, fullTitle[0], checkbutton_stats_sex.Active, 
						statsJumpsType,
						limit,
						weightStatsPercent
						);
				myStat.PrepareData();
			}
		}
		else if(myText == "DJ (TV)")
		{
			if(graph) {
			} else {
				myStat = new StatDj(treeview_stats, 
						sendSelectedSessions, 
						prefsDigitsNumber, checkbutton_stats_sex.Active,
						statsJumpsType,
						limit);
				myStat.PrepareData();
			}
		}
		else if(myText == "DJ Index (tv-tc)*100/tc")
		{
			if(graph) {
				myStat = new GraphDjIndex ( 
						sendSelectedSessions, 
						prefsDigitsNumber, checkbutton_stats_sex.Active, 
						statsJumpsType,
						limit);
				myStat.PrepareData();
				myStat.CreateGraph();
			} else {
				myStat = new StatDjIndex(treeview_stats, 
						sendSelectedSessions, 
						prefsDigitsNumber, checkbutton_stats_sex.Active,
						statsJumpsType,
						limit);
				myStat.PrepareData();
			}
		}
		else if(myText == "RJ Average Index")
		{
			if(graph) {
			} else {
				myStat = new StatRjIndex(treeview_stats, 
						sendSelectedSessions, 
						prefsDigitsNumber, checkbutton_stats_sex.Active,
						statsJumpsType,
						limit);
				myStat.PrepareData();
			}
		}	
		else if(myText == "POTENCY (Aguado)") // 9.81^2*TV*TT / (4*jumps*(TT-TV))
		{
			if(graph) {
			} else {
				myStat = new StatRjPotencyAguado(treeview_stats, 
						sendSelectedSessions, 
						prefsDigitsNumber, checkbutton_stats_sex.Active, 
						statsJumpsType,
						limit);
				myStat.PrepareData();
			}
		}
		else if(myText == "IE (cmj-sj)*100/sj")
		{
			if(graph) {
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
		else if(myText == "IUB (abk-cmj)*100/cmj")
		{
			if(graph) {
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
		combo_jumps.PopdownStrings = SqliteJumpType.SelectJumpTypes(allJumpsName, true); //only select name
		
		combo_jumps.DisableActivate ();
		combo_jumps.Entry.Changed += new EventHandler (on_combo_jumps_changed);

		hbox_combo_jumps.PackStart(combo_jumps, true, true, 0);
		hbox_combo_jumps.ShowAll();
		
		combo_jumps.Sensitive = false;
		label_current_jumper.Sensitive = false;
	}
	
	private void createComboJumpsRj() {
		combo_jumps_rj = new Combo ();
		combo_jumps_rj.PopdownStrings = SqliteJumpType.SelectJumpRjTypes(allJumpsName, true); //only select name
		
		combo_jumps_rj.DisableActivate ();
		combo_jumps_rj.Entry.Changed += new EventHandler (on_combo_jumps_rj_changed);

		hbox_combo_jumps_rj.PackStart(combo_jumps_rj, true, true, 0);
		hbox_combo_jumps_rj.ShowAll();
		
		combo_jumps_rj.Sensitive = false;
		label_current_jumper.Sensitive = false;
	}
	
	private void createComboStats() {
		combo_stats_stat_name = new Combo ();
		combo_stats_stat_name.PopdownStrings = comboStatsOptions;
		
		combo_stats_stat_name.DisableActivate ();
		combo_stats_stat_name.Entry.Changed += new EventHandler (on_combo_stats_stat_name_changed);

		hbox_combo_stats_stat_name.PackStart(combo_stats_stat_name, false, true, 0);
		hbox_combo_stats_stat_name.ShowAll();
		
		combo_stats_stat_name.Sensitive = false;
		checkbutton_stats_sex.Sensitive = false;
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
		combo_jumps.PopdownStrings = SqliteJumpType.SelectJumpTypes(allJumpsName, true); //only select name
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
	
	//combosujeto does not to show always the last person as currentperson
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
		menuitem_edit_selected_jump_rj.Sensitive = true;
		menuitem_delete_selected_jump_rj.Sensitive = true;
		button_edit_selected_jump_rj.Sensitive = true;
		button_delete_selected_jump_rj.Sensitive = true;
		
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
		menuitem_edit_selected_jump.Sensitive = true;
		menuitem_delete_selected_jump.Sensitive = true;
		button_edit_selected_jump.Sensitive = true;
		button_delete_selected_jump.Sensitive = true;
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
				bool myBool = updateComboSujetoCurrent();
			}
		}
	}

	private string fetchID (string text)
	{
		string [] myStringFull = text.Split(new char[] {':'});
		return myStringFull[0];
	}
	
	
	/* ---------------------------------------------------------
	 * ----------------  STATS CALLBACKS--------------------
	 *  --------------------------------------------------------
	 */
	
	private void on_checkbutton_stats_always_clicked(object o, EventArgs args) {
		if (statsAutomatic) { 
			statsAutomatic = false; 
			button_stats.Sensitive = true;
		}
		else { 
			statsAutomatic = true; 
			button_stats.Sensitive = false;
			fillTreeView_stats(false);
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
		string myText = combo_stats_stat_name.Entry.Text;
		
		//some stats should not be showed as limited jumps
		if(myText == "Global" || myText == "Jumper" || myText == "IE (cmj-sj)*100/sj" || myText == "IUB (abk-cmj)*100/cmj"
				|| (selectedSessions.Count > 1 && ! radiobutton_current_session.Active) )
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
		} else {
			radiobutton_stats_jumps_all.Sensitive = true;
			radiobutton_stats_jumps_limit.Sensitive = true;
			if(radiobutton_stats_jumps_person_bests.Active) {
				spin_stats_jumps_person_bests.Sensitive = true;
			}
		}
	}
	
	private void on_combo_stats_stat_name_changed(object o, EventArgs args) {
		update_stats_widgets_sensitiveness();
			
		//for an unknown reason, when we select an option in the combo stats, 
		//the on_combo_stats_stat_name_changed it's called two times? 
		//in the first the value of Entry.Text is "";
		string myText = combo_stats_stat_name.Entry.Text;
		if(myText != "")
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
			bool myBool = updateComboSujetoCurrent();
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
			ExportSessionCSV myExport = new ExportSessionCSV(currentSession, app1, appbar2);
		} else if (o == (object) menuitem_export_xml) {
			ExportSessionXML myExport = new ExportSessionXML(currentSession, app1, appbar2);
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
		bool myBool = updateComboSujetoCurrent();
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
			bool myBool = updateComboSujetoCurrent();
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
			bool myBool = updateComboSujetoCurrent (currentPerson.UniqueID + ": " + currentPerson.Name);

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
				app1, prefsDigitsNumber, showHeight, askDeletion, weightStatsPercent);
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
		

	/* ---------------------------------------------------------
	 * ----------------  JUMPS EXECUTION (no RJ) ----------------
	 *  --------------------------------------------------------
	 */

	private void on_cancel_jump_clicked (object o, EventArgs args) 
	{
		Console.WriteLine("Cancel Jump");

		//this will cancel jumps
		cancellingJump = true;
		progressBar.Fraction = 1.0;
	}
		
	private void on_button_more_clicked (object o, EventArgs args) 
	{
		jumpsMoreWin = JumpsMoreWindow.Show(app1);
		jumpsMoreWin.Button_accept.Clicked += new EventHandler(on_more_jumps_accepted);
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
		} else if( ! currentJumpType.StartIn ) {
			jumpExtraWin.Button_accept.Clicked += new EventHandler(on_jump_extra_fall_accepted);
		} else {
			jumpExtraWin.Button_accept.Clicked += new EventHandler(on_jump_extra_not_fall_accepted);
		}
	}

	private void on_jump_extra_not_fall_accepted (object o, EventArgs args) {
		on_normal_jump_activate (o, args);
	}
	
	private void on_jump_extra_fall_accepted (object o, EventArgs args) {
		on_jump_fall_accepted (o, args);
	}

	
	//suitable for sj, sj+, cmj, cmj+, abk, abk+, ...
	//all jumps that startIn and are not repetitive
	private void on_normal_jump_activate (object o, EventArgs args) 
	{
		
		string myType;

		if(o == (object) button_sj || o == (object) sj) {
			currentJumpType = new JumpType("SJ");
		} else if (o == (object) button_cmj || o == (object) cmj) {
			currentJumpType = new JumpType("CMJ");
		} else if (o == (object) button_abk || o == (object) abk) {
			currentJumpType = new JumpType("ABK");
		} else {
		}

		if (simulated) {
			//random value
			double myTV = rand.NextDouble() * .6;
			Console.WriteLine("TV: {0}", myTV.ToString());

			//write the Jump
			writeNormalJump (myTV);
		}
		else {
			do {
				respuesta = cp.Read_platform(out platformState);
			} while (respuesta!=Chronopic.Respuesta.Ok);
      
    			if (platformState==Chronopic.Plataforma.ON) {
				appbar2.Push( Catalog.GetString("You are IN, JUMP when prepared!!") );

				loggedState = States.ON;
		
				//reset progressBar
				progressBar.Fraction = 0;
	
				//hide jumping buttons
				sensitiveGuiJumping();

				//prepare jump for being cancelled if desired
				cancellingJump = false;
				
				//start thread
				thread = new Thread(new ThreadStart(waitJump));
				GLib.Idle.Add (new GLib.IdleHandler (Pulse));
				thread.Start(); 
			} 
			else {
				confirmWin = ConfirmWindow.Show(app1, 
						Catalog.GetString("You are OUT, come inside and press button"), "");

				//we call again this function
				confirmWin.Button_accept.Clicked += new EventHandler(on_normal_jump_activate);
			}
		}
	}

	private void waitJump ()
	{
		double timestamp;
		bool success = false;
		do {
			respuesta = cp.Read_event(out timestamp, out platformState);
			if (respuesta == Chronopic.Respuesta.Ok) {
				if (platformState == Chronopic.Plataforma.ON && loggedState == States.OFF) {
					//it's inside, was out (= has landed)

					//change the automata state (not needed in this jump)
					loggedState = States.ON;

					//write the jump in seconds
					writeNormalJump (timestamp/1000);

					success = true;
				}
				else if (platformState == Chronopic.Plataforma.OFF && loggedState == States.ON) {
					//it's out, was inside (= has jumped)
					//don't write nothing here

					//change the automata state
					loggedState = States.OFF;

					//change the progressBar percent
					progressBar.Fraction = 0.5;
				}
			}
		} while ( ! success && ! cancellingJump );
	}
	
	private bool Pulse ()
	{
		if (thread.IsAlive) {
			if(progressBar.Fraction == 1) {
				Console.Write("dying");
				sensitiveGuiJumped();
				return false;
			}
			Thread.Sleep (150);
			//Console.Write(thread.ThreadState);
			return true;
		}
		return false;
	}


	//writes the non-simulated normal jump (all non repetitve jumps without startIn (without fall))
	//sj, sj+, cmj, cmj+, abk, abk+, ...
	private void writeNormalJump (double myTV) 
	{
		string myWeight = "";
		if(currentJumpType.HasWeight) {
			myWeight = jumpExtraWin.Weight + jumpExtraWin.Option;
		}

		currentJump = SqliteJump.Insert(currentPerson.UniqueID, currentSession.UniqueID, 
				currentJumpType.Name, myTV, 0, 0,  //type, tv, tc, fall
				myWeight, "", ""); //weight, limited, description
		lastJumpIsReactive = false;

		sensitiveGuiYesJump();
		myTreeViewJumps.Add(currentPerson.Name, currentJump);
		
		if(statsAutomatic) {
			fillTreeView_stats(false);
		}

		string myStringPush =   Catalog.GetString("Last jump: ") + currentPerson.Name + " " + 
			currentJumpType.Name + " TV:" + Util.TrimDecimals( myTV.ToString(), prefsDigitsNumber ) ;
		if(currentJumpType.HasWeight) {
			myStringPush = myStringPush + "(" + myWeight + ")";
		}
		appbar2.Push( myStringPush );

		//change to page 0 of notebook if we were on 1
		if(notebook_jumps.CurrentPage == 1) {
			notebook_jumps.PrevPage();
		}
				
		//put max value in progressBar. This makes the thread in Pulse() stop
		this.progressBar.Fraction = 1;
	}

	/* ---------------------------------------------------------
	 * ----------------  JUMPS WITH PREVIOUS FALL  EXECUTION  --
	 *  --------------------------------------------------------
	 */
	private void on_jump_fall_accepted (object o, EventArgs args) 
	{
		if (simulated) {
			//random values
			double myTV = rand.NextDouble() * .6;
			Console.WriteLine("TV: {0}", myTV.ToString());
			double myTC = rand.NextDouble() * .4;
			Console.WriteLine("TC: {0}", myTC.ToString());

			//write the Jump
			writeJumpFall (myTC, myTV);
		}
		else {
			do {
				respuesta = cp.Read_platform(out platformState);
			} while (respuesta!=Chronopic.Respuesta.Ok);
      
    			if (platformState==Chronopic.Plataforma.OFF) {
				appbar2.Push( Catalog.GetString("You are OUT, JUMP when prepared!!") );

				loggedState = States.OFF;
				tcDjJump = 0;	//useful for tracking the evolution of this jump
				
				//reset progressBar
				this.progressBar.Fraction = 0;
			
				//hide jumping buttons
				sensitiveGuiJumping();

				//prepare jump for being cancelled if desired
				cancellingJump = false;
				
				//start thread
				thread = new Thread(new ThreadStart(waitJumpFall));
				GLib.Idle.Add (new GLib.IdleHandler (Pulse));
				thread.Start(); 
			} 
			else {
				confirmWin = ConfirmWindow.Show(app1, 
						Catalog.GetString("You are IN, please go out and press button"), "");

				//we call again this function
				confirmWin.Button_accept.Clicked += new EventHandler(on_jump_fall_accepted);
			}
		}
	}

	public void waitJumpFall ()
	{
		double timestamp;
		bool success = false;
		do {
			respuesta = cp.Read_event(out timestamp, out platformState);
			if (respuesta == Chronopic.Respuesta.Ok) {
				if (platformState == Chronopic.Plataforma.ON && loggedState == States.OFF
						&& tcDjJump == 0) {
					//it's inside, was out (= has landed), first time
					loggedState = States.ON;

					//dont' write nothing here

					//change the progressBar percent
					this.progressBar.Fraction = 0.33;
				} 
				else if (platformState == Chronopic.Plataforma.OFF && loggedState == States.ON) {
					//it's out, was inside (= has jumped)
					//record the TC
					tcDjJump = timestamp;

					//change the state
					loggedState = States.OFF;

					//change the progressBar percent
					this.progressBar.Fraction = 0.66;
				}
				else if (platformState == Chronopic.Plataforma.ON && loggedState == States.OFF
						&& tcDjJump != 0) {
					//it's inside, was out (= has landed), second time
					loggedState = States.ON;

					//write the jump in seconds
					writeJumpFall (tcDjJump/1000, timestamp/1000);

					//change the automata state (not needed in this jump)
					loggedState = States.ON;

					//change the progressBar percent
					//this.progressBar.Fraction = 1;

					success = true;
				}
			}
		} while ( ! success && ! cancellingJump );
	}

	private void writeJumpFall (double myTC, double myTV) 
	{
		int myFall = jumpExtraWin.Fall;
		string myWeight = "";
		if(currentJumpType.HasWeight) {
			myWeight = jumpExtraWin.Weight + jumpExtraWin.Option;
		}

		currentJump = SqliteJump.Insert(currentPerson.UniqueID, currentSession.UniqueID, 
				currentJumpType.Name, myTV, myTC, myFall, //type, tv, tc, fall
				myWeight, "", ""); //weight, limited, description
		lastJumpIsReactive = false;

		sensitiveGuiYesJump();
		
		myTreeViewJumps.Add(currentPerson.Name, currentJump);
		
		if(statsAutomatic) {
			fillTreeView_stats(false);
		}

		string myStringPush =   Catalog.GetString("Last jump: ") + currentPerson.Name + " " + 
			currentJumpType.Name + " TV:" + Util.TrimDecimals( myTV.ToString(), prefsDigitsNumber ) ;
		myStringPush = myStringPush + " TC:" + Util.TrimDecimals( myTC.ToString(), prefsDigitsNumber );
		if (myWeight != "") { 
			myStringPush = myStringPush + "(" + myWeight + ")";
		}
		appbar2.Push( myStringPush );
		
		//change to page 0 of notebook if we were on 1
		if(notebook_jumps.CurrentPage == 1) {
			notebook_jumps.PrevPage();
		}
				
		//put max value in progressBar. This makes the thread in Pulse() stop
		this.progressBar.Fraction = 1;
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
		double myTv;
		double myTc;
		double myLimit;
		if(currentJumpType.FixedValue > 0) {
			myLimit = currentJumpType.FixedValue;
		} else {
			myLimit = jumpExtraWin.Limited;
		}

		//suitable for limited by jump and time
		//simulated always simulate limited by jumps
		if(simulated) {
			string myTvString = "" ;
			string myTcString = "" ;
			string equalTc = "";
			string equalTv = "";
			bool nowTv = false;
			if( currentJumpType.StartIn ) {
				//is start in TV, write a "-1" in TC
				nowTv = true;
				myTc = -1;
				myTcString = myTc.ToString();
				equalTc = "=";
			}
			for (double i=0 ; i < myLimit ; i = i +.5) {
				//we insert the RJs as a TV and TC string of all jumps separated by '='
				if( nowTv ) {
					myTv = rand.NextDouble() * .6;
					myTvString = myTvString + equalTv + myTv.ToString();
					equalTv = "=";
					nowTv = false;
				} else {
					myTc = rand.NextDouble() * .4;
					myTcString = myTcString + equalTv + myTc.ToString();
					equalTc = "=";
					nowTv = true;
				}
			}

			if(nowTv) {
				//finished writing the TC, let's put a "-1" in the TV
				myTv = -1;
				myTvString = myTvString + equalTv + myTv.ToString();
			}
			writeJumpReactive (myTcString, myTvString);
		}
		else {
			do {
				respuesta = cp.Read_platform(out platformState);
			} while (respuesta!=Chronopic.Respuesta.Ok);

			bool success = false;
      
    			if (platformState==Chronopic.Plataforma.ON && currentJumpType.StartIn ) {
				//Console.WriteLine( Catalog.GetString("You are IN, JUMP when prepared!!") );
				appbar2.Push( Catalog.GetString("You are IN, JUMP when prepared!!") );
				success = true;
			} else if (platformState==Chronopic.Plataforma.OFF && ! currentJumpType.StartIn ) {
				//Console.WriteLine( Catalog.GetString("You are OUT, JUMP when prepared!!") );
				appbar2.Push( Catalog.GetString("You are OUT, JUMP when prepared!!") );
				success = true;
			} else {
				string myMessage = Catalog.GetString("You are IN, please go out the platform, prepare for jump and press button");
				if (platformState==Chronopic.Plataforma.OFF ) {
					myMessage = Catalog.GetString("You are OUT, please put on the platform, prepare for jump and press button");
				}
				confirmWin = ConfirmWindow.Show(app1, myMessage, "");

				//we call again this function
				confirmWin.Button_accept.Clicked += new EventHandler(on_rj_accepted);
			}
				
			if(success) {
				//initialize strings of TCs and TVs
				rjTcString = "";
				rjTvString = "";
				rjTcCount = 0;
				rjTvCount = 0;
				firstRjValue = true;
				
				//if jump starts on TV, write a "-1" in TC
    				if ( currentJumpType.StartIn ) {
					myTc = -1;
					rjTcString = myTc.ToString();
					rjTcCount = rjTcCount +.5;
				}

				//reset progressBar
				this.progressBar.Fraction = 0;
				
				//hide jumping buttons
				sensitiveGuiJumping();
				
				//prepare jump for being cancelled if desired
				cancellingJump = false;
				
				//start thread
				thread = new Thread(new ThreadStart(waitJumpReactive));
				GLib.Idle.Add (new GLib.IdleHandler (Pulse));
				thread.Start(); 
			}
		}
	}
				
	public void waitJumpReactive()
	{
		double myLimit;
		if(currentJumpType.FixedValue >0) {
			myLimit = currentJumpType.FixedValue;
		} else {
			myLimit = jumpExtraWin.Limited;
		}

		double timestamp;
		bool success = false;
		do {
			respuesta = cp.Read_event(out timestamp, out platformState);

			//update the progressBar if limit is time
			if ( ! currentJumpType.JumpsLimited) {
				double myPb = getTotalTime (rjTcString, rjTvString) / myLimit ;
				if(myPb > 1.0) { myPb = 1.0; }
				this.progressBar.Fraction = myPb; 
			}
				
			if (respuesta == Chronopic.Respuesta.Ok) {
				string equal = "";
				//check if reactive jump should finish
				if (currentJumpType.JumpsLimited) {
					//change the progressBar percent
					Console.WriteLine("rjTcCount: {0} rjTvCount: {1} bar: {2} limit: {3}", 
							rjTcCount, rjTvCount, (rjTcCount + rjTvCount) / myLimit, myLimit ) ;
					this.progressBar.Fraction = (rjTcCount + rjTvCount) / myLimit ;

					if(getNumberOfJumps(rjTcString) >= myLimit && getNumberOfJumps(rjTvString) >= myLimit)
					{
						//finished writing the TC, let's put a "-1" in the TV
						if (rjTcCount > rjTvCount) {
							if(rjTvCount > 0) { equal = "="; }
							rjTvString = rjTvString + equal + "-1";
						}

						writeJumpReactive (rjTcString, rjTvString);

						success = true;
					}
				} else {
					//limited by time
					if (getTotalTime (rjTcString, rjTvString) >= myLimit &&
							getNumberOfJumps(rjTcString) == getNumberOfJumps(rjTvString) ) 
					{
						//finished writing the TC, let's put a "-1" in the TV
						if (rjTcCount > rjTvCount) {
							if(rjTvCount > 0) { equal = "="; }
							rjTvString = rjTvString + equal + "-1";
						}

						writeJumpReactive (rjTcString, rjTvString);

						success = true;
					}
				}

				if ( ! success) {
					//don't record the time until the first event
					if (firstRjValue) {
						firstRjValue = false;
					} else {
						//reactive jump has not finished... record the next jump
						if (rjTcCount == rjTvCount) {
							double myTc = timestamp/1000;
							Console.WriteLine("TC: {0}", myTc);
							if(rjTcCount > 0) { equal = "="; }
							rjTcString = rjTcString + equal + myTc.ToString();
							rjTcCount = rjTcCount +.5;
						} else {
							//rjTcCount > rjTvCount 
							double myTv = timestamp/1000;
							Console.WriteLine("TV: {0}", myTv);
							if(rjTvCount > 0) { equal = "="; }
							rjTvString = rjTvString + equal + myTv.ToString();
							rjTvCount = rjTvCount +.5;
						}
					}
				}
			}
		} while ( ! success && ! cancellingJump );
	}

	private void writeJumpReactive (string myTCString, string myTVString) 
	{
		double myLimit = 0;
		if(currentJumpType.FixedValue >0) {
			myLimit = currentJumpType.FixedValue;
		} else {
			myLimit = jumpExtraWin.Limited;
		}

		int jumps;
		string limitString = "";
		if(currentJumpType.JumpsLimited) {
			limitString = myLimit.ToString() + "J";
			jumps = (int) myLimit;
		} else {
			limitString = myLimit.ToString() + "T";
			jumps = (int) rjTcCount;
		}
		
		string myWeightString;
		if(currentJumpType.HasWeight) {
			myWeightString = jumpExtraWin.Weight + jumpExtraWin.Option;
		} else {
			myWeightString = "";
		}
		int myFall = 0;
		if( ! currentJumpType.StartIn) {
			myFall = jumpExtraWin.Fall;
		}

		currentJumpRj = SqliteJump.InsertRj(currentPerson.UniqueID, currentSession.UniqueID, 
				currentJumpType.Name, Util.GetMax(myTVString), Util.GetMax(myTCString), 
				myFall, myWeightString, "", //fall, weight, description
				Util.GetAverage(myTVString), Util.GetAverage(myTCString),
				myTVString, myTCString,
				jumps, getTotalTime(myTCString, myTVString), limitString
				);
		lastJumpIsReactive = true;

		sensitiveGuiYesJump();

		myTreeViewJumpsRj.Add(currentPerson.Name, currentJumpRj);

		if(statsAutomatic) {
			fillTreeView_stats(false);
		}

		string myStringPush =   Catalog.GetString("Last jump: ") + currentPerson.Name + " " + 
			currentJumpType.Name + " (" + limitString + ") " +
			" AVG TV: " + Util.TrimDecimals( Util.GetAverage (myTVString).ToString(), prefsDigitsNumber ) +
			" AVG TC: " + Util.TrimDecimals( Util.GetAverage (myTCString).ToString(), prefsDigitsNumber ) ;
		appbar2.Push( myStringPush );
	
		//change to page 1 of notebook if we were on 0
		if(notebook_jumps.CurrentPage == 0) {
			notebook_jumps.NextPage();
		}
				
		//put max value in progressBar. This makes the thread in Pulse() stop
		this.progressBar.Fraction = 1;
	}
	
	private static double getTotalTime (string stringTC, string stringTV)
	{
		if(stringTC.Length > 0 && stringTV.Length > 0) {
			string [] tc = stringTC.Split(new char[] {'='});
			string [] tv = stringTV.Split(new char[] {'='});

			double totalTime = 0;

			foreach (string jump in tc) {
				totalTime = totalTime + Convert.ToDouble(jump);
			}
			foreach (string jump in tv) {
				totalTime = totalTime + Convert.ToDouble(jump);
			}

			return totalTime ;
		} else {
			return 0;
		}
	}
	
	private static int getNumberOfJumps(string myString)
	{
		//FIXME: make this method nicer and less buggier
		if(myString.Length > 0) {
			string [] jumpsSeparated = myString.Split(new char[] {'='});
			int count = 0;
			foreach (string temp in jumpsSeparated) {
				count++;
			}
			if(count == 0) { count =1; }
			
			return count;
		} else { 
			return 0;
		}
	}
	
	/* ---------------------------------------------------------
	 * ----------------  JUMPS EDIT, DELETE, JUMP TYPE ADD -----
	 *  --------------------------------------------------------
	 */
	
	private void on_last_jump_delete (object o, EventArgs args) {
		Console.WriteLine("delete last");
		
		if(lastJumpIsReactive) {
			SqliteJump.RjDelete(currentJumpRj.UniqueID.ToString());
		} else {
			SqliteJump.Delete(currentJump.UniqueID.ToString());
		}
		menuitem_last_jump_delete.Sensitive = false ;
		button_last_jump_delete.Sensitive = false ;
		
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

	/* ---------------------------------------------------------
	 * ----------------  SOME MORE CALLBACKS---------------------
	 *  --------------------------------------------------------
	 */
	
	
	//help
	private void on_about1_activate (object o, EventArgs args) {
		new Gnome.About (
				progname, progversion,
				"(C) 2004 Xavier de  Blas, Juan Gonzalez",
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
		sj.Sensitive = false ;
		sj_plus.Sensitive = false ;
		cmj.Sensitive = false ;
		abk.Sensitive = false ;
		dj.Sensitive = false ;
		rj_j.Sensitive = false ;
		rj_t.Sensitive = false ;
		more.Sensitive = false;
		more_rj.Sensitive = false;
		jump_type_add.Sensitive = false;
		menuitem_last_jump_delete.Sensitive = false ;
		menuitem_cancel_jump.Sensitive = false ;

		//frame_jumpers
		frame_jumpers.Sensitive = false;
		button_recup_per.Sensitive = false ;
		button_create_per.Sensitive = false ;
		
		//notebook_jumps
		notebook_jumps.Sensitive = false;
		button_sj.Sensitive = false ;
		button_sj_plus.Sensitive = false ;
		button_cmj.Sensitive = false ;
		button_abk.Sensitive = false ;
		button_dj.Sensitive = false ;
		button_more.Sensitive = false ;
		button_rj_j.Sensitive = false ;
		button_rj_t.Sensitive = false ;
		button_last_jump_delete.Sensitive = false ;
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
		button_cancel_jump.Sensitive = false ;
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
		notebook_jumps.Sensitive = true;
		combo_person_current.Sensitive = true;
		button_edit_current_person.Sensitive = true;
		menuitem_edit_current_person.Sensitive = true;
		
		sj.Sensitive = true ;
		sj_plus.Sensitive = true ;
		cmj.Sensitive = true ;
		abk.Sensitive = true ;
		dj.Sensitive = true ;
		more.Sensitive = true;
		more_rj.Sensitive = true;
		rj_j.Sensitive = true ;
		rj_t.Sensitive = true ;
		jump_type_add.Sensitive = true;
		menuitem_last_jump_delete.Sensitive = true ;
		button_last_jump_delete.Sensitive = true ;
		
		button_sj_plus.Sensitive = true ;
		button_sj.Sensitive = true ;
		button_cmj.Sensitive = true ;
		button_abk.Sensitive = true ;
		button_dj.Sensitive = true ;
		button_more.Sensitive = true ;
		button_rj_j.Sensitive = true ;
		button_rj_t.Sensitive = true ;

		frame_stats.Sensitive = true;
		if(!statsAutomatic) {
			button_stats.Sensitive = true;
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
		
		checkbutton_sort_by_type.Sensitive = true ;
		combo_jumps.Sensitive = true;
		combo_jumps_rj.Sensitive = true;
		label_current_jumper.Sensitive = true;
		button_graph.Sensitive = true;
	}
	
	private void sensitiveGuiYesJump () {
		button_last_jump_delete.Sensitive = true ;
		menuitem_last_jump_delete.Sensitive = true ;
		
		button_cancel_jump.Sensitive = true ;
		menuitem_cancel_jump.Sensitive = true ;
	}
	
	private void sensitiveGuiJumping () {
		//vbox
		vbox_jumps.Sensitive = false;
		vbox_jumps_rj.Sensitive = false;
		
		//menu
		sj.Sensitive = false ;
		sj_plus.Sensitive = false ;
		cmj.Sensitive = false ;
		abk.Sensitive = false ;
		dj.Sensitive = false ;
		more.Sensitive = false;
		more_rj.Sensitive = false;
		rj_j.Sensitive = false ;
		rj_t.Sensitive = false ;
		
		//cancel jump
		button_cancel_jump.Sensitive = true ;
		menuitem_cancel_jump.Sensitive = true ;
	}
    
	private void sensitiveGuiJumped () {
		//vbox
		vbox_jumps.Sensitive = true;
		vbox_jumps_rj.Sensitive = true;
		
		//menu
		sj.Sensitive = true ;
		sj_plus.Sensitive = true ;
		cmj.Sensitive = true ;
		abk.Sensitive = true ;
		dj.Sensitive = true ;
		more.Sensitive = true;
		more_rj.Sensitive = true;
		rj_j.Sensitive = true ;
		rj_t.Sensitive = true ;
		
		//cancel jump
		button_cancel_jump.Sensitive = true ;
		menuitem_cancel_jump.Sensitive = true ;
	}
    
}
