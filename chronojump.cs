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
	[Widget] Gtk.Box hbox_combo_stats2;
	[Widget] Gtk.Box hbox_combo_person_current;
	[Widget] Gtk.Label label_current_jumper;
	[Widget] Gtk.Combo combo_jumps;
	[Widget] Gtk.Combo combo_stats2;
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
	bool statsSex = false;


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
	[Widget] Gtk.RadioButton radiobutton_jumpers;
	[Widget] Gtk.RadioButton radiobutton_jumps;
	[Widget] Gtk.RadioButton radiobutton_max;
	[Widget] Gtk.RadioButton radiobutton_avg;
	[Widget] Gtk.SpinButton spinbutton_jumps_num;
	[Widget] Gtk.TextView textview_enunciate;
	[Widget] Gtk.ScrolledWindow scrolledwindow_enunciate;

	[Widget] Gtk.RadioButton radiobutton_simulated_bar;
	[Widget] Gtk.RadioButton radiobutton_serial_bar;

	private Random rand;
	
	private static string [] authors = {"Xavier de Blas", "Juan Gonzalez"};
	private static string progversion = "0.1";
	private static string progname = "ChronoJump";
	
	//normal jumps
	private TreeStore treeview_jumps_store;
	private static Gtk.TreeViewColumn col_name;
	private static Gtk.TreeViewColumn col_tv;
	private static Gtk.TreeViewColumn col_height;
	private static Gtk.TreeViewColumn col_tc;

	//rj jumps
	private TreeStore treeview_jumps_rj_store;
	private static Gtk.TreeViewColumn rj_col_name;
	private static Gtk.TreeViewColumn rj_col_tv;
	private static Gtk.TreeViewColumn rj_col_height;
	private static Gtk.TreeViewColumn rj_col_tc;


	private static string allJumpsName = "All jumps";
	private static string [] comboJumpsOptions = {allJumpsName, "SJ", "SJ+", "CMJ", "ABK", "DJ"};
	private static string [] comboStatsOptions = {"Global", "SJ", "SJ+", "CMJ", "ABK", 
		"DJ (TV)", 
		"DJ Index ((tv-tc)/tc)*100", 
		"RJ Average Index", 
		"IE ((cmj-sj)/sj)*100",
		"IUB ((abk-cmj)/cmj)*100"
	};

	//preferences variables
	private static int prefsDigitsNumber ;
	private static bool showHeight ;
	private static bool simulated ;
	private static bool askDeletion ;

	//treeviews collapsed array of iters
	private ArrayList myArrayOfStringItersCollapsed;
	private ArrayList myArrayOfStringItersRjCollapsed;
	
	//currentPerson currentSession currentJump
	private static Person currentPerson;
	private static Session currentSession;
	private static Jump currentJump;

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
	ConfirmWindow confirmWin;
	ConfirmWindowPlatform confirmWinPlatform;

	//timers
	private static int rjTimer;
	private static int rjTimerLimited;
	private static System.Timers.Timer timerClockJump;    

	//platform state variables
    	private int serial_fd=0;
	private bool platformState;
    	private int estadoInicial;
	private double tcDjJump;
	private bool firstRjValue;
	private string rjTCString;
	private string rjTVString;

	//selected jumps
	private int jumpSelected = 0;
	private int jumpRjSelected = 0;

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
		

		myArrayOfStringItersCollapsed = new ArrayList(2);
		myArrayOfStringItersRjCollapsed = new ArrayList(2);
		
		createTreeView_jumps(treeview_jumps);
		createTreeView_jumps_rj(treeview_jumps_rj);

		createComboJumps();
		createComboStats();
		createComboSujetoCurrent();

		myStat = new Stat(); //create and instance of myStat

		//We have no session, mark some widgets as ".Sensitive = false"
		sensitiveGuiNoSession();

		//appbar2.Push("Ready.");
		appbar2.Push ( Catalog.GetString ("Ready.") );

		rand = new Random(40);
		
		Console.WriteLine ( Catalog.GetString ("starting connection with serial port") );
		Console.WriteLine ( Catalog.GetString ("if program chrashes, disable next line, and work always in 'simulated' mode") );
		//serial_fd = Serial.Open("/dev/ttyS0");
		
		program.Run();
	}

	private void loadPreferences () 
	{
		prefsDigitsNumber = Convert.ToInt32 ( Sqlite.PreferencesSelect("digitsNumber") ); 
		if ( Sqlite.PreferencesSelect("showHeight") == "True" ) {
			showHeight = true;
		} else {
			showHeight = false;
		}
			
		if ( Sqlite.PreferencesSelect("simulated") == "True" ) {
			simulated = true;
			radiobutton_simulated_bar.Active = true;
		} else {
			simulated = false;
			radiobutton_serial_bar.Active = true;
		}
		
		if ( Sqlite.PreferencesSelect("askDeletion") == "True" ) {
			askDeletion = true;
		} else {
			askDeletion = false;
		}
		
		Console.WriteLine ( Catalog.GetString ("Preferences loaded") );
	}

	/* ---------------------------------------------------------
	 * ----------------  TREEVIEW JUMPS ------------------------
	 *  --------------------------------------------------------
	 */

	private void createTreeView_jumps (Gtk.TreeView tv) {
		tv.HeadersVisible=true;
		int count = 0;
	
		col_name = tv.AppendColumn ( Catalog.GetString("Name"), new CellRendererText(), "text", count ++);
		col_tv = tv.AppendColumn ("TV", new CellRendererText(), "text", count ++);
		if (showHeight) {
			col_height = tv.AppendColumn ( Catalog.GetString("height"), new CellRendererText(), "text", count ++);
		}
		col_tc = tv.AppendColumn ("TC", new CellRendererText(), "text", count ++);
		//tv.AppendColumn ("Fall", new CellRendererText(), "text", 4);
		
	}

	private void removeTreeView_jumps (bool seenHeight) {
		treeview_jumps.RemoveColumn (col_name);
		treeview_jumps.RemoveColumn (col_tv);
		if (seenHeight) {
			treeview_jumps.RemoveColumn (col_height);
		}
		treeview_jumps.RemoveColumn (col_tc);
	}

	private void fillTreeView_jumps (Gtk.TreeView tv, TreeStore store, string filter) {
		TreeIter iter = new TreeIter();

		string tempJumper = ":"; //one value that's not possible
	
		string [] myJumps;
		
		if(sortJumpsByType) {
			myJumps = Sqlite.SelectAllNormalJumps(currentSession.UniqueID, "ordered_by_type"); //returns a string of values separated by ':'
		}
		else {
			myJumps = Sqlite.SelectAllNormalJumps(currentSession.UniqueID, "ordered_by_time"); //returns a string of values separated by ':'
		}


		string myType ;
		string myTypeComplet ;
			
		foreach (string jump in myJumps) {
			string [] myStringFull = jump.Split(new char[] {':'});

			//show always the names of jumpers ...
			if(tempJumper != myStringFull[0])
			{
				iter = store.AppendValues (myStringFull[0]);
				tempJumper = myStringFull[0];
			}

			//... but if we selected one type of jump and this it's not the type, don't show
			if(filter == allJumpsName || filter == myStringFull[4]) {

				myType = myStringFull[4];
				myTypeComplet = myType;
				//SJ+ weight and RJ limited, are in fall column
				if (myType == "DJ") {
					myTypeComplet = myType + "(" + myStringFull[7] + ")"; //fall
				} else if (myType == "SJ+") {
					myTypeComplet = myType + "(" + myStringFull[8] + ")"; //weight
				}
				
				if (showHeight) {
					store.AppendValues (iter,
						myTypeComplet,
						trimDecimals( myStringFull[5].ToString() ),
						trimDecimals( obtainHeight( myStringFull[5].ToString() ) ),
						trimDecimals( myStringFull[6].ToString() )
						, myStringFull[1] //jumpUniqueID (not shown) 
						);
				} else {
					store.AppendValues (iter, 
						myTypeComplet, 
						trimDecimals( myStringFull[5].ToString() ),
						trimDecimals( myStringFull[6].ToString() )
						, myStringFull[1] //jumpUniqueID (not shown) 
						);
				}
			}
		}	

		//now we expand what it's NOT marked for collapsing. Found no other way of doing this
		string myStringTreeView;
		TreeIter iterTreeView = new TreeIter();
		bool modelNotEmpty = tv.Model.GetIterFirst ( out iterTreeView ) ;
		bool found;

		do {
			if(!modelNotEmpty) {
				return;
			}
			
			myStringTreeView = tv.Model.GetStringFromIter ( iterTreeView ) ;

			found = false;
			foreach (string myStringArray in myArrayOfStringItersCollapsed) {
				if (myStringArray != null) {

					//if iterTreeView it's in the contracted Array list of iters
					if ( myStringArray == myStringTreeView ) { 
						found = true;
						goto finishForeach;
					}
				}
			}

finishForeach:

			if(!found) {
				tv.ExpandRow( tv.Model.GetPath ( iterTreeView ) , true);
			}

		} while (tv.Model.IterNext (ref iterTreeView));

	}

	/*
	private void expandCurrentJumperIfNeeded ()
	{
		foreach (string myStringArray in myArrayOfStringItersCollapsed) {
			if (myStringArray != null) {
				//if iterTreeView it's in the contracted Array list of iters
				Console.WriteLine("{0}:::{1}", myStringArray, currentPerson.Name);
				if ( myStringArray == currentPerson.Name ) { 
					myArrayOfStringItersCollapsed.Remove ( myStringArray );
				}
			}
		}
	}
	*/
	
	private void on_treeview_jumps_row_collapsed (object o, RowCollapsedArgs args)
	{
		string value = (string) treeview_jumps.Model.GetValue (args.Iter, 0);
		Console.WriteLine ("collapsed: {0}", value);
		
		//put this iter in the row collapsed iters array, but check first if it's not already there.
		
		bool found = false;
		
		foreach (string myString in myArrayOfStringItersCollapsed) {
			if (myString == treeview_jumps.Model.GetStringFromIter (args.Iter) ) {
				found = true;
			}
		}
		if(!found) {
			myArrayOfStringItersCollapsed.Add ( treeview_jumps.Model.GetStringFromIter (args.Iter) );
		}
		
		foreach (string myString in myArrayOfStringItersCollapsed) {
		}
	}
	
	private void on_treeview_jumps_row_expanded (object o, RowExpandedArgs args)
	{
		string value = (string) treeview_jumps.Model.GetValue (args.Iter, 0);
		
		myArrayOfStringItersCollapsed.Remove ( treeview_jumps.Model.GetStringFromIter (args.Iter) );
	}

	
	//puts a value in private member selected
	private void on_treeview_jumps_cursor_changed (object o, EventArgs args)
	{
		TreeView tv = (TreeView) o;
		TreeModel model;
		TreeIter iter;

		// you get the iter and the model if something is selected
		if (tv.Selection.GetSelected (out model, out iter)) {
			if(showHeight) {
				jumpSelected = Convert.ToInt32 ( model.GetValue (iter, 4) );
			} else {
				jumpSelected = Convert.ToInt32 ( model.GetValue (iter, 3) );
			}
		} else {
			jumpSelected = 0;
		}

	}
	
	private void treeview_jumps_storeReset() {
		if (showHeight) {
			//name, tv, alt, tc, jumpUniqueID
			//if it's a person, jumpUniqueID is "". 
			treeview_jumps_store = new TreeStore(typeof (string), typeof (string), typeof (string), typeof (string), typeof(string));
		} else {
			//name, tv, tc, jumpUniqueID
			//if it's a person, jumpUniqueID is "".
			treeview_jumps_store = new TreeStore(typeof (string), typeof (string), typeof (string), typeof (string));
		}
		
		treeview_jumps.Model = treeview_jumps_store;
	}


	/* ---------------------------------------------------------
	 * ----------------  TREEVIEW JUMPS RJ ---------------------
	 *  --------------------------------------------------------
	 */

	private void createTreeView_jumps_rj (Gtk.TreeView tv) {
		tv.HeadersVisible=true;
		int count = 0;
	
		rj_col_name = tv.AppendColumn (Catalog.GetString("Name"), new CellRendererText(), "text", count ++);
		rj_col_tv = tv.AppendColumn ("TV", new CellRendererText(), "text", count ++);
		if (showHeight) {
			rj_col_height = tv.AppendColumn (Catalog.GetString("height"), new CellRendererText(), "text", count ++);
		}
		rj_col_tc = tv.AppendColumn ("TC", new CellRendererText(), "text", count ++);
		//tv.AppendColumn ("Fall", new CellRendererText(), "text", 4);
		
	}

	private void removeTreeView_jumps_rj (bool seenHeight) {
		treeview_jumps_rj.RemoveColumn (rj_col_name);
		treeview_jumps_rj.RemoveColumn (rj_col_tv);
		if (seenHeight) {
			treeview_jumps_rj.RemoveColumn (rj_col_height);
		}
		treeview_jumps_rj.RemoveColumn (rj_col_tc);
	}

	//no filter in treeview_jumps_rj
	private void fillTreeView_jumps_rj (Gtk.TreeView tv, TreeStore store) {
		TreeIter iter = new TreeIter();
		TreeIter iterDeep = new TreeIter(); //only for RJ

		string tempJumper = ":"; //one value that's not possible
	
		string [] myJumps;
		
		myJumps = Sqlite.SelectAllRjJumps(currentSession.UniqueID); //returns a string of values separated by ':'


		string myType ;
		string myTypeComplet ;
			
		foreach (string jump in myJumps) {
			string [] myStringFull = jump.Split(new char[] {':'});

			//show always the names of jumpers ...
			if(tempJumper != myStringFull[0])
			{
				iter = store.AppendValues (myStringFull[0]);
				tempJumper = myStringFull[0];
			}


			myType = myStringFull[4];
			myTypeComplet = myType + "(" + myStringFull[16] + ") AVG: "; //limited

			if (showHeight) {
				iterDeep = store.AppendValues (iter,
						myTypeComplet,
						trimDecimals( myStringFull[10].ToString() ), //tvAvg
						trimDecimals( obtainHeight( myStringFull[10].ToString() ) ), //height(tvAvg)
						trimDecimals( myStringFull[11].ToString() ), //tcAvg
						myStringFull[1] //jumpUniqueID (not shown) 
						);
			} else {
				iterDeep = store.AppendValues (iter, 
						myTypeComplet, 
						trimDecimals( myStringFull[10].ToString() ), //tvAvg
						trimDecimals( myStringFull[11].ToString() ), //tcAvg
						myStringFull[1] //jumpUniqueID (not shown) 
						);
			}
			//if it's an RJ, we should make a deeper tree with all the jumps
			//the info above it's average

			string [] rjTvs = myStringFull[12].Split(new char[] {'='});
			string [] rjTcs = myStringFull[13].Split(new char[] {'='});
			int count = 0;
			foreach (string myTv in rjTvs) 
			{
				if (showHeight) {
					store.AppendValues (iterDeep, 
							(count+1).ToString(), 
							trimDecimals(myTv), 
							trimDecimals(obtainHeight(myTv)),
							trimDecimals(rjTcs[count]), 
							myStringFull[1] //jumpUniqueID 
							);
				} else {
					store.AppendValues (iterDeep, 
							(count+1).ToString(), 
							trimDecimals(myTv), 
							trimDecimals(rjTcs[count]),
							myStringFull[1] //jumpUniqueID 
							);
				}
				count ++;
			}
		}

		//now we expand what it's NOT marked for collapsing. Found no other way of doing this
		string myStringTreeView;
		TreeIter iterTreeView = new TreeIter();
		bool modelNotEmpty = tv.Model.GetIterFirst ( out iterTreeView ) ;
		bool found;

		do {
			if(!modelNotEmpty) {
				return;
			}
			
			myStringTreeView = tv.Model.GetStringFromIter ( iterTreeView ) ;

			found = false;
			foreach (string myStringArray in myArrayOfStringItersRjCollapsed) {
				if (myStringArray != null) {

					//if iterTreeView it's in the contracted Array list of iters
					if ( myStringArray == myStringTreeView ) { 
						found = true;
						goto finishForeach;
					}
				}
			}

finishForeach:

			if(!found) {
				tv.ExpandRow( tv.Model.GetPath ( iterTreeView ) , true);
			}

			//all the deeper iters (RJs) should be always collapsed
			if( tv.Model.IterHasChild (iterTreeView) ) {
				int children = tv.Model.IterNChildren(iterTreeView) ;
				//Console.WriteLine("HasChild, children: {0}", children );
				tv.Model.IterNthChild ( out iterTreeView, iterTreeView, 0 );
				for (int i=0; i < children ; i++) {
					string myStringTreeView2 = tv.Model.GetStringFromIter ( iterTreeView ) ;
					//Console.WriteLine("stringtreeview2: {0}, has child: {1}", 
							//myStringTreeView2, tv.Model.IterNChildren(iterTreeView) );
					if ( tv.Model.IterHasChild (iterTreeView) ) {
						tv.CollapseRow(tv.Model.GetPath (iterTreeView) );
						//Console.WriteLine("collapsed");
					}
				
					tv.Model.IterNext (ref iterTreeView);
				}
				tv.Model.IterParent ( out iterTreeView, iterTreeView );
			}
		} while (tv.Model.IterNext (ref iterTreeView));

//escape:

	}

	
	private void on_treeview_jumps_rj_row_collapsed (object o, RowCollapsedArgs args)
	{
		string value = (string) treeview_jumps_rj.Model.GetValue (args.Iter, 0);
		
		//put this iter in the row collapsed iters array, but check first if it's not already there.
		
		bool found = false;
		
		foreach (string myString in myArrayOfStringItersRjCollapsed) {
			if (myString == treeview_jumps_rj.Model.GetStringFromIter (args.Iter) ) {
				found = true;
			}
		}
		if(!found) {
			myArrayOfStringItersRjCollapsed.Add ( treeview_jumps_rj.Model.GetStringFromIter (args.Iter) );
		}
		
		foreach (string myString in myArrayOfStringItersRjCollapsed) {
		}
	}
	
	private void on_treeview_jumps_rj_row_expanded (object o, RowExpandedArgs args)
	{
		string value = (string) treeview_jumps_rj.Model.GetValue (args.Iter, 0);
		Console.WriteLine ("expanded: {0}", value);
		
		myArrayOfStringItersRjCollapsed.Remove ( treeview_jumps_rj.Model.GetStringFromIter (args.Iter) );
	}

	
	//puts a value in private member selected
	private void on_treeview_jumps_rj_cursor_changed (object o, EventArgs args)
	{
		TreeView tv = (TreeView) o;
		TreeModel model;
		TreeIter iter;

		// you get the iter and the model if something is selected
		if (tv.Selection.GetSelected (out model, out iter)) {
			if(showHeight) {
				jumpRjSelected = Convert.ToInt32 ( model.GetValue (iter, 4) );
			} else {
				jumpRjSelected = Convert.ToInt32 ( model.GetValue (iter, 3) );
			}
		} else {
			jumpRjSelected = 0;
		}

	}
	
	private void treeview_jumps_rj_storeReset() {
		if (showHeight) {
			//name, tv, alt, tc, jumpUniqueID
			//if it's a person, jumpUniqueID is "". If it's a RJsubjump, jumpUniqueID equals the RJ jump uniqueID
			treeview_jumps_rj_store = new TreeStore(typeof (string), typeof (string), typeof (string), typeof (string), typeof(string));
		} else {
			//name, tv, tc, jumpUniqueID
			//if it's a person, jumpUniqueID is "". If it's a RJsubjump, jumpUniqueID equals the RJ jump uniqueID
			treeview_jumps_rj_store = new TreeStore(typeof (string), typeof (string), typeof (string), typeof (string));
		}
		
		treeview_jumps_rj.Model = treeview_jumps_rj_store;
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
	
	private void fillTreeView_stats () {
		
		string myText = combo_stats2.Entry.Text;
		string [] fullTitle = myText.Split(new char[] {' '});

		if(myStat.SessionName != "") {
			myStat.RemoveHeaders();
		}
	
		int limit;
		if (radiobutton_jumps.Active) {
			limit = Convert.ToInt32 ( spinbutton_jumps_num.Value ); 
		} else {
			limit = 0;
		}
	
		if(myText == "SJ" || myText == "SJ+" || 
				myText == "CMJ" || myText == "ABK")
		{
			myStat = new StatSjCmjAbk (treeview_stats, currentSession.UniqueID, 
					currentSession.Name, prefsDigitsNumber, fullTitle[0], statsSex, 
					radiobutton_max.Active, //show MAX or AVG
					limit);
			myStat.prepareData();

		}	
		else if(myText == "DJ (TV)")
		{
			myStat = new StatDjTv(treeview_stats, currentSession.UniqueID, 
					currentSession.Name, prefsDigitsNumber, statsSex,
					radiobutton_max.Active, //show MAX or AVG
					limit);
			myStat.prepareData();
		}	
		else if(myText == "DJ Index ((tv-tc)/tc)*100")
		{
			myStat = new StatDjIndex(treeview_stats, currentSession.UniqueID, 
					currentSession.Name, prefsDigitsNumber, statsSex,
					radiobutton_max.Active, //show MAX or AVG
					limit);
			myStat.prepareData();
		}
		else if(myText == "RJ Average Index")
		{
			myStat = new StatRjIndex(treeview_stats, currentSession.UniqueID, 
					currentSession.Name, prefsDigitsNumber, statsSex,
					radiobutton_max.Active, //show MAX or AVG
					limit);
			myStat.prepareData();
		}	
		else if(myText == "IE ((cmj-sj)/sj)*100")
		{
			myStat = new StatIE(treeview_stats, currentSession.UniqueID, 
					currentSession.Name, prefsDigitsNumber, statsSex, 
					radiobutton_max.Active //show MAX or AVG
					);
			myStat.prepareData();
		}
		else if(myText == "IUB ((abk-cmj)/cmj)*100")
		{
			myStat = new StatIUB(treeview_stats, currentSession.UniqueID, 
					currentSession.Name, prefsDigitsNumber, statsSex, 
					radiobutton_max.Active //show MAX or AVG
					);
			myStat.prepareData();
		}
		
		else if ( myText == "Global") {
			myStat = new StatGlobal(treeview_stats, currentSession.UniqueID, 
						currentSession.Name, prefsDigitsNumber, statsSex,  
						radiobutton_max.Active //show MAX or AVG
						);
			myStat.prepareData();
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
		fillTreeView_stats();

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
		combo_stats2 = new Combo ();
		combo_stats2.PopdownStrings = comboStatsOptions;
		
		combo_stats2.DisableActivate ();
		combo_stats2.Entry.Changed += new EventHandler (on_combo_stats2_changed);

		hbox_combo_stats2.PackStart(combo_stats2, true, true, 0);
		hbox_combo_stats2.ShowAll();
		
		combo_stats2.Sensitive = false;
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
		string [] jumpers = Sqlite.PersonSessionSelectCurrentSession(currentSession.UniqueID);
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
		string [] jumpers = Sqlite.PersonSessionSelectCurrentSession(currentSession.UniqueID);
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

		if (myText == allJumpsName) {
			checkbutton_sort_by_type.Sensitive = true ;
		} else {
			checkbutton_sort_by_type.Sensitive = false ;
		}
		
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
	}

	private void on_combo_person_current_changed(object o, EventArgs args) {
		string myText = combo_person_current.Entry.Text;
		if(myText != "") {
			//if people modify the values in the combo_person_current, and this valeus are not correct, 
			//let's update the combosujetocurrent
			if(Sqlite.PersonSelectExistsInSession(fetchID(myText), currentSession.UniqueID)) {
				currentPerson = Sqlite.PersonSelect(fetchID(myText));
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
	
	private void on_checkbutton_sort_by_type_clicked(object o, EventArgs args) {
		if (sortJumpsByType) { sortJumpsByType = false; }
		else { sortJumpsByType = true; }
		
		string myText = combo_jumps.Entry.Text;
			
		treeview_jumps_storeReset();
		fillTreeView_jumps(treeview_jumps, treeview_jumps_store, myText);
	}
	
	private void on_checkbutton_stats_always_clicked(object o, EventArgs args) {
		if (statsAutomatic) { 
			statsAutomatic = false; 
			button_stats.Sensitive = true;
		}
		else { 
			statsAutomatic = true; 
			button_stats.Sensitive = false;
			fillTreeView_stats();
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
	
	private void on_combo_stats2_changed(object o, EventArgs args) {
		string myText = combo_stats2.Entry.Text;
		
		//for an unknown reason, when we select an option in the combo stats, 
		//the on_combo_stats2_changed it's called two times? 
		//in the first the value of Entry.Text is "";
		if(myText != "")
			fillTreeView_stats();

		//some stats should not be showed as limited jumps
		if(myText == "Global" || myText == "IE ((cmj-sj)/sj)*100" || myText == "IUB ((abk-cmj)/cmj)*100") 
		{
			//change the radiobutton value
			if(radiobutton_jumps.Active) {
				radiobutton_jumpers.Active = true;
			}
			//make no sensitive
			radiobutton_jumps.Sensitive = false;
		} else {
			radiobutton_jumps.Sensitive = true;
		}
		
	}
	
	private void on_checkbutton_stats_sex_clicked(object o, EventArgs args)
	{
		if (statsSex) { statsSex = false; }
		else { statsSex = true; }
		
		if (statsAutomatic) { 
			fillTreeView_stats();
		}
	}

	void on_radiobutton_jumpers_toggled (object o, EventArgs args)
	{
		spinbutton_jumps_num.Sensitive = false;
		radiobutton_max.Sensitive = true;
		radiobutton_avg.Sensitive = true;
		checkbutton_stats_sex.Sensitive = true;
		if (statsAutomatic) { 
			fillTreeView_stats();
		}
	}
	
	void on_radiobutton_jumps_toggled (object o, EventArgs args)
	{
		spinbutton_jumps_num.Sensitive = true;
		radiobutton_max.Sensitive = false;
		radiobutton_avg.Sensitive = false;
		checkbutton_stats_sex.Sensitive = false;
		if (statsAutomatic) { 
			fillTreeView_stats();
		}
	}
	
	void on_radiobutton_max_toggled (object o, EventArgs args)
	{
		if (statsAutomatic) { 
			fillTreeView_stats();
		}
	}
	
	void on_radiobutton_avg_toggled (object o, EventArgs args)
	{
		if (statsAutomatic) { 
			fillTreeView_stats();
		}
	}
	
	void on_spinbutton_jumps_num_changed (object o, EventArgs args)
	{
		if (statsAutomatic) { 
			fillTreeView_stats();
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
	 * ----------------  SESSION NEW AND LOAD---------------
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

			//clear the arrayList if contracted iters
			myArrayOfStringItersCollapsed = new ArrayList(2);

			//load the treeview
			treeview_jumps_storeReset();
			fillTreeView_jumps(treeview_jumps,treeview_jumps_store,allJumpsName);
			//load the treeview_rj
			treeview_jumps_rj_storeReset();
			fillTreeView_jumps_rj(treeview_jumps_rj, treeview_jumps_rj_store);

			fillTreeView_stats();

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
		
		//clear the arrayList if contracted iters
		myArrayOfStringItersCollapsed = new ArrayList(2);
		
		//load the treeview_jumps
		treeview_jumps_storeReset();
		fillTreeView_jumps(treeview_jumps,treeview_jumps_store,allJumpsName);
		//load the treeview_jumps_rj
		treeview_jumps_rj_storeReset();
		fillTreeView_jumps_rj(treeview_jumps_rj,treeview_jumps_rj_store);
		
		//everytime we load a session, we put stats to "Global" 
		//(if this session has no jumps, it crashes in the others statistics)
		combo_stats2.Entry.Text = comboStatsOptions[0];
		fillTreeView_stats();

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

			if(statsAutomatic) {
				fillTreeView_stats();
			}
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
	
	void on_radiobutton_simulated_toggled (object o, EventArgs args)
	{
		simulated = true;
		Sqlite.PreferencesUpdate("simulated", simulated.ToString());
	}
	
	void on_radiobutton_serial_toggled (object o, EventArgs args)
	{
		simulated = false;
		Sqlite.PreferencesUpdate("simulated", simulated.ToString());
	}
	
	private void on_preferences_activate (object o, EventArgs args) {
		PreferencesWindow myWin = PreferencesWindow.Show(app1, prefsDigitsNumber, showHeight, simulated, askDeletion);
		myWin.Button_accept.Clicked += new EventHandler(on_preferences_accepted);
	}

	private void on_preferences_accepted (object o, EventArgs args) {
		//now simulated-serial it's a radiobutton in the menu
		/*
		if ( Sqlite.PreferencesSelect("simulated") == "True" ) {
			simulated = true;
		} else {
			simulated = false;
		}
		*/
		
		prefsDigitsNumber = Convert.ToInt32 ( Sqlite.PreferencesSelect("digitsNumber") ); 
		if ( Sqlite.PreferencesSelect("askDeletion") == "True" ) {
			askDeletion = true;
		} else {
			askDeletion = false;
		}

		//...remove corresponding headers...
		removeTreeView_jumps (showHeight);
		removeTreeView_jumps_rj (showHeight);
		
		//update showHeight
		if ( Sqlite.PreferencesSelect("showHeight") == "True" ) {
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
			fillTreeView_stats();
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

				confirmWinPlatform = ConfirmWindowPlatform.Show(app1,  Catalog.GetString("You are OUT, come inside and press button"), "");

				//we call again this function
				confirmWinPlatform.Button_accept.Clicked += new EventHandler(on_normal_jump_activate);

				Console.WriteLine( Catalog.GetString("You are IN, JUMP when prepared!!") );
				//appbar2.Push( "Estas dentro, cuando quieras SALTA!!" );

			}
		}

		//check if the row of this jumper it's contracted, if it's, mark for expanding
		//FIXME: do the same in the other jumps
		//expandCurrentJumperIfNeeded ();
	}


	//writes the non-simulated normal jump (sj, sj+, cmj, abk) to the DB, and updates treeviews and stats
	private void writeNormalJump (double myTV) 
	{
		string myType = lastRealJumpType;

		string myWeight = "";
		if (myType == "SJ+") { myWeight = sjPlusWin.Weight + sjPlusWin.Option ;
		}

		currentJump = Sqlite.JumpInsert(currentPerson.UniqueID, currentSession.UniqueID, 
				myType, myTV, 0, 0,  //type, tv, tc, fall
				myWeight, "", ""); //weight, limited, description

		sensitiveGuiYesJump();
		string myText = combo_jumps.Entry.Text;
		treeview_jumps_storeReset();
		fillTreeView_jumps(treeview_jumps,treeview_jumps_store,myText);
		if(statsAutomatic) {
			fillTreeView_stats();
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

				confirmWinPlatform = ConfirmWindowPlatform.Show(app1,  Catalog.GetString("You are IN, please go out the platform, prepare for jump and press button"), "");

				//we call again this function
				confirmWinPlatform.Button_accept.Clicked += new EventHandler(on_dj_fall_accepted);
			}
		}

		//check if the row of this jumper it's contracted, if it's, mark for expanding
		//FIXME: do the same in the other jumps
		//expandCurrentJumperIfNeeded ();
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

		currentJump = Sqlite.JumpInsert(currentPerson.UniqueID, currentSession.UniqueID, 
				"DJ", myTV, myTC, myFall, //type, tv, tc, fall
				"", "", ""); //weight, limited, description

		sensitiveGuiYesJump();
		string myText = combo_jumps.Entry.Text;
		treeview_jumps_storeReset();
		fillTreeView_jumps(treeview_jumps,treeview_jumps_store,myText);
		if(statsAutomatic) {
			fillTreeView_stats();
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

				confirmWinPlatform = ConfirmWindowPlatform.Show(app1,  Catalog.GetString("You are IN, please go out the platform, prepare for jump and press button"), "");

				//we call again this function
				confirmWinPlatform.Button_accept.Clicked += new EventHandler(on_rj_accepted);
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
		
		currentJump = Sqlite.JumpInsertRj(currentPerson.UniqueID, currentSession.UniqueID, 
				"RJ", getMax(myTVString), getMax(myTCString), 
				0, "", "", //fall, weight, description
				getAverage(myTVString), getAverage(myTCString),
				myTVString, myTCString,
				jumps, getTotalTime(myTCString, myTVString), limited
				);
		
		
		sensitiveGuiYesJump();
		treeview_jumps_rj_storeReset();
		fillTreeView_jumps_rj(treeview_jumps_rj, treeview_jumps_rj_store);
		if(statsAutomatic) {
			fillTreeView_stats();
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
		if(currentJump.Type == "RJ") {
			Sqlite.JumpRjDelete(currentJump.UniqueID.ToString());
		} else {
			Sqlite.JumpDelete(currentJump.UniqueID.ToString());
		}
		menuitem_last_jump_delete.Sensitive = false ;
		button_last_jump_delete.Sensitive = false ;
		
		appbar2.Push( Catalog.GetString("Last jump deleted") );
		
		if(currentJump.Type == "RJ") {
			treeview_jumps_rj_storeReset();
			fillTreeView_jumps_rj(treeview_jumps_rj,treeview_jumps_rj_store);
		} else {
			string myText = combo_jumps.Entry.Text;
			treeview_jumps_storeReset();
			fillTreeView_jumps(treeview_jumps,treeview_jumps_store,myText);
		}
		
		if(statsAutomatic) {
			fillTreeView_stats();
		}
	}

	private void on_edit_selected_jump_clicked (object o, EventArgs args) {
		Console.WriteLine("Edit selected jump (normal)");
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person (check also if it's not a individual RJ, the pass the parent RJ)
		if (jumpSelected > 0) {
			//3.- obtain the data of the selected jump
			Jump myJump = Sqlite.SelectNormalJumpData( jumpSelected );
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
		if (jumpRjSelected > 0) {
			//3.- obtain the data of the selected jump
			Jump myJump = Sqlite.SelectRjJumpData( jumpRjSelected );
			Console.WriteLine(myJump);
		
			//4.- edit this jump
			editJumpWin = EditJumpWindow.Show(app1, myJump);
			editJumpWin.Button_accept.Clicked += new EventHandler(on_edit_selected_jump_rj_accepted);
		}
	}
	
	private void on_edit_selected_jump_accepted (object o, EventArgs args) {
		Console.WriteLine("edit selected jump accepted");
		
		treeview_jumps_storeReset();
		fillTreeView_jumps(treeview_jumps, treeview_jumps_store, combo_jumps.Entry.Text);
		
		if(statsAutomatic) {
			fillTreeView_stats();
		}
	}
	
	private void on_edit_selected_jump_rj_accepted (object o, EventArgs args) {
		Console.WriteLine("edit selected jump RJ accepted");
		
		treeview_jumps_rj_storeReset();
		fillTreeView_jumps_rj(treeview_jumps_rj, treeview_jumps_rj_store);
		
		if(statsAutomatic) {
			fillTreeView_stats();
		}
	}
	
	private void on_delete_selected_jump_clicked (object o, EventArgs args) {
		Console.WriteLine("delete selected jump (normal)");
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person
		if (jumpSelected > 0) {
			//3.- display confirmwindow of deletion 
			if (askDeletion) {
				bool isRj = false;
				confirmWin = ConfirmWindow.Show(app1, "Do you want to delete selected jump?", 
						"", "jump", jumpSelected, isRj);
				confirmWin.Button_accept.Clicked += new EventHandler(on_delete_selected_jump_accepted);
			} else {
				Console.WriteLine("accept delete selected jump");
				Sqlite.JumpDelete(jumpSelected.ToString());
				treeview_jumps_storeReset();
				fillTreeView_jumps(treeview_jumps, treeview_jumps_store, combo_jumps.Entry.Text);

				if(statsAutomatic) {
					fillTreeView_stats();
				}
			}
		}
	}
	
	private void on_delete_selected_jump_rj_clicked (object o, EventArgs args) {
		Console.WriteLine("delete selected (RJ) jump");
		//1.- check that there's a line selected
		//2.- check that this line is a jump and not a person (check also if it's not a individual RJ, the pass the parent RJ)
		if (jumpRjSelected > 0) {
			//3.- display confirmwindow of deletion 
			if (askDeletion) {
				bool isRj = true;
				confirmWin = ConfirmWindow.Show(app1,  Catalog.GetString("Do you want to delete selected jump?"), 
						 Catalog.GetString("Atention: Deleting a RJ subjump will delete all the RJ"), 
						 "jump", jumpRjSelected, isRj);
				confirmWin.Button_accept.Clicked += new EventHandler(on_delete_selected_jump_rj_accepted);
			} else {
				Console.WriteLine("accept delete selected jump");
				Sqlite.JumpRjDelete(jumpRjSelected.ToString());
				treeview_jumps_rj_storeReset();
				fillTreeView_jumps_rj(treeview_jumps_rj, treeview_jumps_rj_store);

				if(statsAutomatic) {
					fillTreeView_stats();
				}
			}
		}
	}
	
	private void on_delete_selected_jump_accepted (object o, EventArgs args) {
		Console.WriteLine("accept delete selected jump");
		treeview_jumps_storeReset();
		fillTreeView_jumps(treeview_jumps, treeview_jumps_store, combo_jumps.Entry.Text);

		if(statsAutomatic) {
			fillTreeView_stats();
		}
	}

	private void on_delete_selected_jump_rj_accepted (object o, EventArgs args) {
		Console.WriteLine("accept delete selected jump");
		treeview_jumps_rj_storeReset();
		fillTreeView_jumps_rj(treeview_jumps_rj, treeview_jumps_rj_store);

		if(statsAutomatic) {
			fillTreeView_stats();
		}
	}

	/* ---------------------------------------------------------
	 * ----------------  SOME MORE CALLBACKS---------------------
	 *  --------------------------------------------------------
	 */
	
	private void on_ind_elasticidad_activate (object o, EventArgs args) {
		Console.WriteLine("I. elast.");
	}
	
	private void on_ind_utiliz_brazos_activate (object o, EventArgs args) {
		Console.WriteLine("I. Ut. brazos");
	}

	//stats
	private void on_intrasesion_activate (object o, EventArgs args) {
		Console.WriteLine("Graf. Intrasesion");
	}
	
	private void on_intersesion_activate (object o, EventArgs args) {
		Console.WriteLine("Graf. Intersesion");
	}
	
	
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
		ind_elasticidad.Sensitive = false ;
		ind_utiliz_brazos.Sensitive = false ;

		intrasesion.Sensitive = false ;
		intersesion.Sensitive = false ;
	
		checkbutton_sort_by_type.Sensitive = false ;
		menuitem_edit_selected_jump.Sensitive = false;
		menuitem_delete_selected_jump.Sensitive = false;
		button_edit_selected_jump.Sensitive = false;
		button_delete_selected_jump.Sensitive = false;
		menuitem_edit_selected_jump_rj.Sensitive = false;
		menuitem_delete_selected_jump_rj.Sensitive = false;
		button_edit_selected_jump_rj.Sensitive = false;
		button_delete_selected_jump_rj.Sensitive = false;
		
		combo_stats2.Sensitive = false;
		checkbutton_stats_sex.Sensitive = false;
		checkbutton_stats_always.Sensitive = false;
		checkbutton_show_enunciate.Sensitive = false;
		radiobutton_jumpers.Sensitive = false;
		radiobutton_jumps.Sensitive = false;
		spinbutton_jumps_num.Sensitive = false;
		radiobutton_max.Sensitive = false;
		radiobutton_avg.Sensitive = false;
	}
	
	private void sensitiveGuiYesSession () {
		button_recup_per.Sensitive = true ;
		button_create_per.Sensitive = true ;
		
		preferences.Sensitive = true ;
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
		combo_stats2.Sensitive = true;
		checkbutton_stats_sex.Sensitive = true;
		checkbutton_stats_always.Sensitive = true;
		checkbutton_show_enunciate.Sensitive = true;
		radiobutton_jumpers.Sensitive = true;
		//radiobutton_jumps.Sensitive = true; //not activated because it starts with the "Global" stat, where radiobutton_jumps has no sense
		radiobutton_max.Sensitive = true;
		radiobutton_avg.Sensitive = true;
	
		
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
