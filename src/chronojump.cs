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



public class ChronoJump {
	[Widget] Gtk.Window app1;
	[Widget] Gnome.AppBar appbar2;
	[Widget] Gtk.TreeView treeview_jumps;
	[Widget] Gtk.TreeView treeview_jumps_rj;
	[Widget] Gtk.TreeView treeview_stats;
	[Widget] Gtk.Box hbox_combo_jumps;
	[Widget] Gtk.Box hbox_combo_stats_stat_name;
	[Widget] Gtk.Box hbox_combo_person_current;
	[Widget] Gtk.Label label_current_jumper;
	[Widget] Gtk.Combo combo_jumps;
	[Widget] Gtk.Combo combo_stats_stat_name;
	[Widget] Gtk.Combo combo_person_current;

	[Widget] Gtk.CheckButton checkbutton_sort_by_type;
	[Widget] Gtk.CheckButton checkbutton_stats_sex;
	[Widget] Gtk.CheckButton checkbutton_stats_always;
	[Widget] Gtk.CheckButton checkbutton_show_enunciate;
	bool sortJumpsByType = false;
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
	[Widget] Gtk.Button button_rj;
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
	[Widget] Gtk.MenuItem rj;
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
	
	
	[Widget] Gtk.TextView textview_enunciate;
	[Widget] Gtk.ScrolledWindow scrolledwindow_enunciate;

	[Widget] Gtk.RadioMenuItem menuitem_simulated;
	[Widget] Gtk.RadioMenuItem menuitem_serial_port;

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


	private static string allJumpsName = "All jumps";
	private static string [] comboJumpsOptions = {allJumpsName, "SJ", "SJ+", "CMJ", "ABK", "DJ"};
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
	private static bool lastJumpIsRj; //if last Jump is an Rj or not

	//windows needed
	SessionAddWindow sessionAddWin;
	SessionLoadWindow sessionLoadWin;
	PersonRecuperateWindow personRecuperateWin; 
	PersonAddWindow personAddWin; 
	PersonModifyWindow personModifyWin; 
	SjPlusWindow sjPlusWin; 
	DjFallWindow djFallWin;
	RjWindow rjWin;
	EditJumpWindow editJumpWin;
	EditJumpRjWindow editJumpRjWin;
	ConfirmWindow confirmWin;		//for go up or down the platform, and for 
						//export in a not-newly-created file
	ConfirmWindowJump confirmWinJump;	//for deleting jumps and RJ jumps
	SessionSelectStatsWindow sessionSelectStatsWin;

	//timers
	private static System.Timers.Timer timerClockJump;    

	//platform state variables
    	private int serial_fd=0;
	private bool platformState;
    	private int estadoInicial;
	private double tcDjJump;
	private bool firstRjValue;
	private string rjTCString;
	private string rjTVString;

	//selected sessions
	ArrayList selectedSessions;

	//useful for deleting headers of lastStat just before making a new Stat
	private Stat myStat;  

	string lastRealJumpType;
	
	
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
		createComboStats();
		createComboSujetoCurrent();

		myStat = new Stat(); //create and instance of myStat
		
		//We have no session, mark some widgets as ".Sensitive = false"
		sensitiveGuiNoSession();

		appbar2.Push ( Catalog.GetString ("Ready.") );

		rand = new Random(40);
		
		Console.WriteLine ( Catalog.GetString ("starting connection with serial port") );
		Console.WriteLine ( Catalog.GetString ("if program crashes, disable next line, and work always in 'simulated' mode") );
		serial_fd = Serial.Open("/dev/ttyS0");
		
		program.Run();
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
			menuitem_serial_port.Active = false;
		} else {
			simulated = false;
			menuitem_serial_port.Active = true;
			menuitem_simulated.Active = false;
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
		myTreeViewJumps = new TreeViewJumps( tv, sortJumpsByType, showHeight, prefsDigitsNumber );
	}

	private void fillTreeView_jumps (Gtk.TreeView tv, TreeStore store, string filter) {
		string [] myJumps;
		
		if(sortJumpsByType) {
			myJumps = SqliteJump.SelectAllNormalJumps(
					currentSession.UniqueID, "ordered_by_type"); //returns a string of values separated by ':'
		}
		else {
			myJumps = SqliteJump.SelectAllNormalJumps(
					currentSession.UniqueID, "ordered_by_time"); //returns a string of values separated by ':'
		}
		myTreeViewJumps.Fill(myJumps, filter);
	}
	
	private void on_checkbutton_sort_by_type_clicked(object o, EventArgs args) {
		if (sortJumpsByType) { sortJumpsByType = false; }
		else { sortJumpsByType = true; }
		
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
		myTreeViewJumps = new TreeViewJumps( treeview_jumps, sortJumpsByType, showHeight, prefsDigitsNumber );
	}


	/* ---------------------------------------------------------
	 * ----------------  TREEVIEW JUMPS RJ ---------------------
	 *  --------------------------------------------------------
	 */

	private void createTreeView_jumps_rj (Gtk.TreeView tv) {
		myTreeViewJumpsRj = new TreeViewJumpsRj( tv, showHeight, prefsDigitsNumber );
	}

	//no filter in treeview_jumps_rj
	private void fillTreeView_jumps_rj (Gtk.TreeView tv, TreeStore store) {
		string [] myJumps = SqliteJump.SelectAllRjJumps(
					currentSession.UniqueID); //returns a string of values separated by ':'
		myTreeViewJumpsRj.Fill(myJumps, "none");
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
	
	private static string obtainHeight (string time) {
		// s = 4.9 * (tv/2)exp2
		double myValue = 4.9 * ( Convert.ToDouble(time) / 2 ) * (Convert.ToDouble(time) / 2 ) ;

		return myValue.ToString();
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

	
	private static string trimDecimals (string time) {
		//the +2 is a workarround for not counting the two first characters: "0."
		//this will not work with the fall
		return time.Length > prefsDigitsNumber + 2 ? 
			time.Substring( 0, prefsDigitsNumber + 2 ) : 
				time;
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
		combo_jumps.PopdownStrings = comboJumpsOptions;
		
		combo_jumps.DisableActivate ();
		combo_jumps.Entry.Changed += new EventHandler (on_combo_jumps_changed);

		hbox_combo_jumps.PackStart(combo_jumps, true, true, 0);
		hbox_combo_jumps.ShowAll();
		
		combo_jumps.Sensitive = false;
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
    
		Serial.Close(serial_fd);
		
		Application.Quit();
	}

	private void on_quit1_activate (object o, EventArgs args) {
		Console.WriteLine("Chao!");
    
		Serial.Close(serial_fd);
		
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
			fillTreeView_jumps(treeview_jumps,treeview_jumps_store,allJumpsName);
			//load the treeview_rj
			treeview_jumps_rj_storeReset();
			fillTreeView_jumps_rj(treeview_jumps_rj, treeview_jumps_rj_store);

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
		fillTreeView_jumps(treeview_jumps,treeview_jumps_store,allJumpsName);
		//load the treeview_jumps_rj
		treeview_jumps_rj_storeReset();
		fillTreeView_jumps_rj(treeview_jumps_rj,treeview_jumps_rj_store);
		
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
			fillTreeView_jumps(treeview_jumps,treeview_jumps_store,myText);
			//load the treeview_rj
			treeview_jumps_rj_storeReset();
			fillTreeView_jumps_rj(treeview_jumps_rj, treeview_jumps_rj_store);

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
				app1, prefsDigitsNumber, showHeight, simulated, askDeletion, weightStatsPercent);
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
		createTreeView_jumps_rj (treeview_jumps_rj);
		treeview_jumps_rj_storeReset();
		fillTreeView_jumps_rj(treeview_jumps_rj, treeview_jumps_rj_store);

		if(statsAutomatic) {
			fillTreeView_stats(false);
		}
			
	}
	//view
		

	/* ---------------------------------------------------------
	 * ----------------  JUMPS EXECUTION ------------------------
	 *  --------------------------------------------------------
	 */

	private void on_cancel_jump_clicked (object o, EventArgs args) 
	{
		//FIXME: do something here
		Console.WriteLine("Cancel Jump");
	}
		
	//suitable for sj, sj+, cmj and abk
	private void on_normal_jump_activate (object o, EventArgs args) 
	{

		string myType;

		if(o == (object) button_sj || o == (object) sj) {
			myType = "SJ";
		} else if (o == (object) button_cmj || o == (object) cmj) {
			myType = "CMJ";
		} else if (o == (object) button_abk || o == (object) abk) {
			myType = "ABK";
		} else {
			//if we call the on_normal_jump_activate from the same function:
			//on_normal_jump_activate, we cannot now tje object button,
			//but we have recorded in the first time the value of this in lastRealJumpType
			//suitable also for the SJ+
			myType = lastRealJumpType;
		}
		//suitable for writing jump type in writeNormalJump, specially when calling from onTimerNormalJump
		lastRealJumpType = myType;

		if (simulated) {
			//random value
			double myTV = rand.NextDouble() * .6;
			Console.WriteLine("TV: {0}", myTV.ToString());

			//write the Jump
			writeNormalJump (myTV);
		}
		else {
			//initialize this ok, and delete buffer
			int ok = 0;
			do {
				//FIXME: change the name of this method to english
				ok = Serial.Estado(serial_fd, out estadoInicial);
				Console.WriteLine("okey1: {0}" ,ok.ToString());
			} while (ok != 1) ;

			if (estadoInicial == 1) {
				Console.WriteLine( Catalog.GetString("You are IN, JUMP when prepared!!") );
				//appbar2.Push( "Estas dentro, cuando quieras SALTA!!" );

				platformState = true;

				//delete serial buffer
				Serial.Flush(serial_fd);

				timerClockJump = new System.Timers.Timer();    
				timerClockJump.Elapsed += new ElapsedEventHandler(OnTimerNormalJump);
				timerClockJump.Interval = 100; //one decisecond
				timerClockJump.Enabled = true;
			} 
			else {
				Console.WriteLine( Catalog.GetString("You are OUT, please come inside the platform") );

				confirmWin = ConfirmWindow.Show(app1,  Catalog.GetString("You are OUT, come inside and press button"), "");

				//we call again this function
				confirmWin.Button_accept.Clicked += new EventHandler(on_normal_jump_activate);

				Console.WriteLine( Catalog.GetString("You are IN, JUMP when prepared!!") );
				//appbar2.Push( "Estas dentro, cuando quieras SALTA!!" );

			}
		}
	}


	//writes the non-simulated normal jump (sj, sj+, cmj, abk) to the DB, and updates treeviews and stats
	private void writeNormalJump (double myTV) 
	{
		string myType = lastRealJumpType;

		string myWeight = "";
		if (myType == "SJ+") { myWeight = sjPlusWin.Weight + sjPlusWin.Option ;
		}

		currentJump = SqliteJump.Insert(currentPerson.UniqueID, currentSession.UniqueID, 
				myType, myTV, 0, 0,  //type, tv, tc, fall
				myWeight, "", ""); //weight, limited, description
		lastJumpIsRj = false;

		sensitiveGuiYesJump();
		string myText = combo_jumps.Entry.Text;
		myTreeViewJumps.Add(currentPerson.Name, currentJump);
		
		if(statsAutomatic) {
			fillTreeView_stats(false);
		}

		string myStringPush =   Catalog.GetString("Last jump: ") + currentPerson.Name + " " + myType + " TV:" +
			trimDecimals( myTV.ToString() ) ;
		if (myType == "SJ+") { myStringPush = myStringPush + "(" + myWeight + ")";
		}
		appbar2.Push( myStringPush );
	}


	public void OnTimerNormalJump( System.Object source, ElapsedEventArgs e )
	{
		//t0 header of frame (always an ascii X): 88
		//t1 state of platform (0 free (outside), 1 someone inside)
		//t2, t3 time (more significative, and less significative) 
		//time = t2 * 256 + t3 (in decimals of milliseconds) 0.1 miliseconds
		//
		int t0,t1,t2,t3;
		int ok = Serial.Read(serial_fd, out t0, out t1, out t2, out t3);
		if (ok==1) {
			Console.WriteLine("trama: {0} {1} {2}", t0, t1, realTime(t2, t3) );

			//we were in and now regist out
			if(platformState && t1 == 0) {
				Console.WriteLine("Changed from {0}, to {1} (out)", platformState, t1);
				platformState = false;
			} 
			//we were out and now register in
			else if(!platformState && t1 == 1) {
				Console.WriteLine("Changed from {0}, to {1} (in) ", platformState, t1);
				timerClockJump.Elapsed -= new ElapsedEventHandler(OnTimerNormalJump);
				timerClockJump.Enabled = false;
				Console.WriteLine("------------Timer event should be killed----------");


				//write the Jump
				writeNormalJump (realTime(t2,t3));
			}
			else { 
				Console.WriteLine("NOT Changed {0} - {1}", platformState, t1);
			}

		}
	}

	//suitable for sj with extra weight
	private void on_sj_plus_activate (object o, EventArgs args) 
	{
		Console.WriteLine("sj+");
		//button_sj_plus.Sensitive = false;

		sjPlusWin = SjPlusWindow.Show(app1);
		sjPlusWin.Button_accept.Clicked += new EventHandler(on_sj_plus_accepted);
	}

	private void on_sj_plus_accepted (object o, EventArgs args) {
		lastRealJumpType = "SJ+";
		on_normal_jump_activate (o, args);
	}

	private void on_dj_activate (object o, EventArgs args) 
	{
		Console.WriteLine("dj");

		djFallWin = DjFallWindow.Show(app1);

		djFallWin.Button_accept.Clicked += new EventHandler(on_dj_fall_accepted);
	}

	private void on_dj_fall_accepted (object o, EventArgs args) 
	{
		if (simulated) {
			//random values
			double myTV = rand.NextDouble() * .6;
			Console.WriteLine("TV: {0}", myTV.ToString());
			double myTC = rand.NextDouble() * .4;
			Console.WriteLine("TC: {0}", myTC.ToString());

			//write the Jump
			writeDjJump (myTC, myTV);
		}
		else {
			//initialize this ok, and delete buffer
			int ok = 0;
			do {
				//FIXME: change the name of this method to english
				ok = Serial.Estado(serial_fd, out estadoInicial);
				Console.WriteLine("okey1: {0}" ,ok.ToString());
			} while (ok != 1) ;

			if (estadoInicial == 0) {
				Console.WriteLine( Catalog.GetString("You are OUT, JUMP when prepared!!") );
				//appbar2.Push( "Estas fuera, cuando quieras SALTA!!" );

				platformState = false;
				tcDjJump = 0;

				//delete serial buffer
				Serial.Flush(serial_fd);

				timerClockJump = new System.Timers.Timer();    
				timerClockJump.Elapsed += new ElapsedEventHandler(OnTimerDjJump);
				timerClockJump.Interval = 100; //one decisecond
				timerClockJump.Enabled = true;
			} 
			else {
				Console.WriteLine( Catalog.GetString("You are IN, please go out the platform") );

				confirmWin = ConfirmWindow.Show(app1,  Catalog.GetString("You are IN, please go out the platform, prepare for jump and press button"), "");

				//we call again this function
				confirmWin.Button_accept.Clicked += new EventHandler(on_dj_fall_accepted);
			}
		}
	}

	public void OnTimerDjJump( System.Object source, ElapsedEventArgs e )
	{
		//t0 header of frame (always an ascii X): 88
		//t1 state of platform (0 free (outside), 1 someone inside)
		//t2, t3 time (more significative, and less significative) 
		//time = t2 * 256 + t3 (in decimals of milliseconds) 0.1 miliseconds
		//
		int t0,t1,t2,t3;
		int ok = Serial.Read(serial_fd, out t0, out t1, out t2, out t3);
		if (ok==1) {
			Console.WriteLine("trama: {0} {1} {2}", t0, t1, realTime(t2, t3) );

			//we were out and now regist in
			if(!platformState && t1 == 1 && tcDjJump == 0) {
				Console.WriteLine("Changed from {0}, to {1} (in)", platformState, t1);
				platformState = true;
			} 
			//we were in and now regist out
			else if(platformState && t1 == 0) {
				Console.WriteLine("Changed from {0}, to {1} (out)", platformState, t1);
				platformState = false;

				//record the TC in a temp variable
				tcDjJump = realTime(t2,t3);
			} 
			//we were out and now register in
			else if(!platformState && t1 == 1 && tcDjJump != 0) {
				Console.WriteLine("Changed from {0}, to {1} (in) ", platformState, t1);
				timerClockJump.Elapsed -= new ElapsedEventHandler(OnTimerDjJump);
				timerClockJump.Enabled = false;
				Console.WriteLine("------------Timer event should be killed----------");


				//write the Jump
				writeDjJump (tcDjJump, realTime(t2,t3));
			}
			else { 
				Console.WriteLine("NOT Changed {0} - {1}", platformState, t1);
			}

		}
	}

	private void writeDjJump (double myTC, double myTV) 
	{
		int myFall = djFallWin.Fall;

		currentJump = SqliteJump.Insert(currentPerson.UniqueID, currentSession.UniqueID, 
				"DJ", myTV, myTC, myFall, //type, tv, tc, fall
				"", "", ""); //weight, limited, description
		lastJumpIsRj = false;

		sensitiveGuiYesJump();
		string myText = combo_jumps.Entry.Text;
		
		myTreeViewJumps.Add(currentPerson.Name, currentJump);
		if(statsAutomatic) {
			fillTreeView_stats(false);
		}

		string myStringPush =   Catalog.GetString("Last jump: ") + currentPerson.Name + " DJ TV:" +
			trimDecimals( myTV.ToString() ) ;
		appbar2.Push( myStringPush );
	}


	private void on_rj_activate (object o, EventArgs args) {
		Console.WriteLine("RJ");

		rjWin = RjWindow.Show(app1);

		rjWin.Button_accept.Clicked += new EventHandler(on_rj_accepted);
	}

	private void on_rj_accepted (object o, EventArgs args) {
		Console.WriteLine( Catalog.GetString("RJ accepted, limited: {0} {1}"), rjWin.Limited.ToString(), rjWin.Option);

		double myTv;
		double myTc;
		string myTvString = "" ;
		string myTcString = "" ;
		string equal = "";
		string limited = rjWin.Limited.ToString() + rjWin.Option; 

		//suitable for limited by jump and time
		if(simulated) {
			for (int i=0 ; i < rjWin.Limited ; i ++) {
				if(i>0) { equal = "="; }
				//we insert the RJs as a TV and TC string of all jumps separated by '='
				myTc = rand.NextDouble() * .4;
				myTcString = myTcString + equal + myTc.ToString();
				myTv = rand.NextDouble() * .6;
				myTvString = myTvString + equal + myTv.ToString();
			}

			writeRjJump (myTcString, myTvString);
		}
		else {
			//initialize this ok, and delete buffer
			int ok = 0;
			do {
				//FIXME: change the name of this method to english
				ok = Serial.Estado(serial_fd, out estadoInicial);
				Console.WriteLine("okey1: {0}" ,ok.ToString());
			} while (ok != 1) ;

			//initialize strings of TCs and TVs
			rjTCString = "";
			rjTVString = "";

			//you have to start outside the platform
			if (estadoInicial == 1) {
				Console.WriteLine( Catalog.GetString("You are IN, please go out the platform") );
				platformState = true;

				confirmWin = ConfirmWindow.Show(app1,  Catalog.GetString("You are IN, please go out the platform, prepare for jump and press button"), "");

				//we call again this function
				confirmWin.Button_accept.Clicked += new EventHandler(on_rj_accepted);
			} else {
				Console.WriteLine( Catalog.GetString("You are OUT, JUMP when prepared!!") );
				platformState = false;
			}

			firstRjValue = true;

			//delete serial buffer
			Serial.Flush(serial_fd);

			timerClockJump = new System.Timers.Timer();    
			timerClockJump.Elapsed += new ElapsedEventHandler(OnTimerRjJump);
			timerClockJump.Interval = 100; //one decisecond
			timerClockJump.Enabled = true;
		}
	}
				
	public void OnTimerRjJump( System.Object source, ElapsedEventArgs e )
	{
		 //t0 header of frame (always an ascii X): 88
		 //t1 state of platform (0 free (outside), 1 someone inside)
		 //t2, t3 time (more significative, and less significative) 
		 //time = t2 * 256 + t3 (in decimals of milliseconds) 0.1 miliseconds
		 //
	
		if (rjWin.Option == "J") {
			int jumps = Convert.ToInt32(rjWin.Limited.ToString());
			if(getNumberOfJumps(rjTCString) >= jumps)
			{
				Console.WriteLine("------------Timer event should be killed----------");
				timerClockJump.Elapsed -= new ElapsedEventHandler(OnTimerRjJump);
				timerClockJump.Enabled = false;

				//write the jump (but sleep a little bit for ensuring the clock eventhandler it's killed)
				//sleep (1);
				writeRjJump (rjTCString, rjTVString);
				return;
			}
		} else {
			//limited by time
			int limitTime = Convert.ToInt32(rjWin.Limited.ToString());
			//check if we passed the time, but only finish when the number of TC equal the number of TV
			if (getTotalTime (rjTCString, rjTVString) >= limitTime &&
					getNumberOfJumps(rjTCString) == getNumberOfJumps(rjTVString) ) 
			{
				Console.WriteLine("------------Timer event should be killed----------");
				timerClockJump.Elapsed -= new ElapsedEventHandler(OnTimerRjJump);
				timerClockJump.Enabled = false;

				//write the jump
				writeRjJump (rjTCString, rjTVString);
				return;
			}
		}
	
		int t0,t1,t2,t3;
		int ok = Serial.Read(serial_fd, out t0, out t1, out t2, out t3);
		if (ok==1) {
			Console.WriteLine("trama: {0} {1} {2}", t0, t1, realTime(t2, t3) );
			string myString = "prova " + t0 + ", " + t1 + ", " + realTime(t2, t3) ;
			appbar2.Push( myString );

			//----------------
			//first value inside or outside, we forget it
			//if it was inside, it shows how many time passed until he jumped (we fon't need it now,
			//but we need in the future for doing test of reaction velocity
			//if it was outside it show time elapsed until get inside (same as above)
			//----------------

			//if(platformState && t1 == 0 && firstRjValue) { }
			if(firstRjValue) {
				Console.WriteLine("Changed from {0}, to {1} (out)", platformState, t1);
				firstRjValue = false;
			} 
			//we were out and now regist in
			else if(!firstRjValue && !platformState && t1 == 1) {
				Console.WriteLine("Changed from {0}, to {1} (in)", platformState, t1);
				platformState = true;

				//record the TV in a temp variable
				if(rjTVString.Length > 0) {
					rjTVString = rjTVString + "=" + realTime(t2,t3);
				} else {
					rjTVString = realTime(t2,t3).ToString();
				}
			} 
			//we were out and now register out
			else if(!firstRjValue && platformState && t1 == 0) {
				Console.WriteLine("Changed from {0}, to {1} (out) ", platformState, t1);
				platformState = false;

				//record the TC in a temp variable
				if(rjTCString.Length > 0) {
					rjTCString = rjTCString + "=" + realTime(t2,t3);
				} else {
					rjTCString = realTime(t2,t3).ToString();
				}
			}
			else { 
				Console.WriteLine("NOT Changed {0} - {1}", platformState, t1);
			}

		}
	}

	private void writeRjJump (string myTCString, string myTVString) 
	{
		string limited = rjWin.Limited.ToString() + rjWin.Option; 
		int jumps = Convert.ToInt32(rjWin.Limited.ToString());
		
		currentJumpRj = SqliteJump.InsertRj(currentPerson.UniqueID, currentSession.UniqueID, 
				"RJ", getMax(myTVString), getMax(myTCString), 
				0, "", "", //fall, weight, description
				getAverage(myTVString), getAverage(myTCString),
				myTVString, myTCString,
				jumps, getTotalTime(myTCString, myTVString), limited
				);
		lastJumpIsRj = true;
		
		sensitiveGuiYesJump();
		
		myTreeViewJumpsRj.Add(currentPerson.Name, currentJumpRj);

		if(statsAutomatic) {
			fillTreeView_stats(false);
		}

		string myStringPush =   Catalog.GetString("Last jump: ") + currentPerson.Name + " RJ (" + limited + ") " +
			" AVG TV: " + trimDecimals( getAverage (myTVString).ToString() ) +
			" AVG TC: " + trimDecimals( getAverage (myTCString).ToString() ) ;
		appbar2.Push( myStringPush );
	}
	
	
	private static double realTime(int timeh, int timel) 
	{
		return (double) ( timeh * 256 + timel ) /10000 ;
	}

	private static double getMax (string values)
	{
		string [] myStringFull = values.Split(new char[] {'='});
		double max = 0;
		foreach (string jump in myStringFull) {
			if ( Convert.ToDouble(jump) > max ) {
				max = Convert.ToDouble(jump);
			}
		}
		return max ; 
	}
	
	private static double getAverage (string values)
	{
		string [] myStringFull = values.Split(new char[] {'='});
		double myAverage = 0;
		double myCount = 0;
		foreach (string jump in myStringFull) {
			myAverage = myAverage + Convert.ToDouble(jump);
			myCount ++;
		}
		return myAverage / myCount ; 
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
	 * ----------------  JUMPS EDIT, DELETE----------------------
	 *  --------------------------------------------------------
	 */
	
	private void on_last_jump_delete (object o, EventArgs args) {
		Console.WriteLine("delete last");
		
		if(lastJumpIsRj) {
			SqliteJump.RjDelete(currentJumpRj.UniqueID.ToString());
		} else {
			SqliteJump.Delete(currentJump.UniqueID.ToString());
		}
		menuitem_last_jump_delete.Sensitive = false ;
		button_last_jump_delete.Sensitive = false ;
		
		appbar2.Push( Catalog.GetString("Last jump deleted") );
		
		if(lastJumpIsRj) {
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
		fillTreeView_jumps_rj(treeview_jumps_rj, treeview_jumps_rj_store);
		
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
				treeview_jumps_rj_storeReset();
				fillTreeView_jumps_rj(treeview_jumps_rj, treeview_jumps_rj_store);

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
		fillTreeView_jumps_rj(treeview_jumps_rj, treeview_jumps_rj_store);

		if(statsAutomatic) {
			fillTreeView_stats(false);
		}
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
				"Vertical jump analysis with contact plataform.",
				authors, null, null, null).Run();
	}

	/* ---------------------------------------------------------
	 * ----------------  SENSITIVE GUI METHODS-------------------
	 *  --------------------------------------------------------
	 */
	
	private void sensitiveGuiNoSession () {
		button_recup_per.Sensitive = false ;
		button_create_per.Sensitive = false ;
		button_sj.Sensitive = false ;
		button_sj_plus.Sensitive = false ;
		button_cmj.Sensitive = false ;
		button_abk.Sensitive = false ;
		button_dj.Sensitive = false ;
		button_rj.Sensitive = false ;
		button_last_jump_delete.Sensitive = false ;
		button_stats.Sensitive = false;
		preferences.Sensitive = false ;
		menuitem_export_csv.Sensitive = false;
		menuitem_export_xml.Sensitive = false;
		recuperate_person.Sensitive = false ;
		create_person.Sensitive = false ;

		button_cancel_jump.Sensitive = false ;
		menuitem_cancel_jump.Sensitive = false ;
	
		sj.Sensitive = false ;
		sj_plus.Sensitive = false ;
		cmj.Sensitive = false ;
		abk.Sensitive = false ;
		dj.Sensitive = false ;
		rj.Sensitive = false ;
		menuitem_last_jump_delete.Sensitive = false ;
	
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
	}
	
	private void sensitiveGuiYesSession () {
		button_recup_per.Sensitive = true ;
		button_create_per.Sensitive = true ;
		
		preferences.Sensitive = true ;
		menuitem_export_csv.Sensitive = true;
		menuitem_export_xml.Sensitive = false; //it's not coded yet
		recuperate_person.Sensitive = true ;
		create_person.Sensitive = true ;
	}

	private void sensitiveGuiYesPerson () {
		combo_person_current.Sensitive = true;
		button_edit_current_person.Sensitive = true;
		menuitem_edit_current_person.Sensitive = true;
		
		sj.Sensitive = true ;
		sj_plus.Sensitive = true ;
		cmj.Sensitive = true ;
		abk.Sensitive = true ;
		dj.Sensitive = true ;
		rj.Sensitive = true ;
		
		button_sj_plus.Sensitive = true ;
		button_sj.Sensitive = true ;
		button_cmj.Sensitive = true ;
		button_abk.Sensitive = true ;
		button_dj.Sensitive = true ;
		button_rj.Sensitive = true ;

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
	
		
		combo_jumps.Entry.Text = comboJumpsOptions[0];
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
		label_current_jumper.Sensitive = true;
	}
	
	private void sensitiveGuiYesJump () {
		button_last_jump_delete.Sensitive = true ;
		menuitem_last_jump_delete.Sensitive = true ;
		
		button_cancel_jump.Sensitive = true ;
		menuitem_cancel_jump.Sensitive = true ;
	}
    
}
