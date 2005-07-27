/*
 * This file is part of ChronoJump
 *
 * ChronoJump is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or   
 *    (at your option) any later version.
 *    
 * ChronoJump is distributed in the hope that it will be useful,
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
using Glade;
using Gnome;
using GLib; //for Value
using System.Text; //StringBuilder
using System.Collections; //ArrayList


public class StatsWindow {
	
	[Widget] Gtk.Window stats_window;
	static StatsWindow StatsWindowBox;
	Gtk.Window parent;
	SessionSelectStatsWindow sessionSelectStatsWin;

	[Widget] Gtk.TreeView treeview_stats;
	[Widget] Gtk.Box hbox_combo_stats_stat_name;
	[Widget] Gtk.Box hbox_combo_stats_stat_name2;
	[Widget] Gtk.Combo combo_stats_stat_name;
	[Widget] Gtk.Combo combo_stats_stat_name2;
	[Widget] Gtk.CheckButton checkbutton_stats_sex;
	[Widget] Gtk.CheckButton checkbutton_stats_always;
	[Widget] Gtk.Button button_stats;
	
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
	[Widget] Gtk.CheckButton checkbutton_show_enunciate;

	int prefsDigitsNumber;
	bool heightPreferred;
	bool weightStatsPercent;
	
	bool statsAutomatic = true;
	bool statsColumnsToRemove = false;
	private Session currentSession;
	//selected sessions
	ArrayList selectedSessions;
	
	//useful for deleting headers of lastStat just before making a new Stat
	private Stat myStat; 
	
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

	
	StatsWindow (Gtk.Window parent, Session currentSession, 
			int prefsDigitsNumber, bool weightStatsPercent, bool heightPreferred)
	{
		Glade.XML gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "stats_window", null);

		gladeXML.Autoconnect(this);
		this.parent = parent;
		this.currentSession = currentSession;
		this.prefsDigitsNumber = prefsDigitsNumber;
		this.weightStatsPercent = weightStatsPercent;
		this.heightPreferred = heightPreferred;

		myStat = new Stat(); //create and instance of myStat
		
		createComboStats();
		createComboStats2();
	}
	

	static public StatsWindow Show (Gtk.Window parent, Session currentSession, 
			int prefsDigitsNumber, bool weightStatsPercent, bool heightPreferred)
	{
		if (StatsWindowBox == null) {
			StatsWindowBox = new StatsWindow (parent, currentSession, 
					prefsDigitsNumber, weightStatsPercent, heightPreferred);
		}
		StatsWindowBox.stats_window.Show ();
		
		return StatsWindowBox;
	}

	public void InitializeSession(Session newCurrentSession) 
	{
		currentSession = newCurrentSession;

		selectedSessions = new ArrayList(2);
		selectedSessions.Add(currentSession.UniqueID + ":" + currentSession.Name + ":" + currentSession.Date);
		fillTreeView_stats(false);
	}
	
	private void statsRemoveColumns() {
		Gtk.TreeViewColumn [] myColumns = treeview_stats.Columns;
		foreach (Gtk.TreeViewColumn column in myColumns) {
			treeview_stats.RemoveColumn (column);
		}
	}

	private void createComboStats() {
		combo_stats_stat_name = new Combo ();
		combo_stats_stat_name.PopdownStrings = comboStatsOptions;
		
		combo_stats_stat_name.DisableActivate ();
		combo_stats_stat_name.Entry.Changed += new EventHandler (on_combo_stats_stat_name_changed);

		hbox_combo_stats_stat_name.PackStart(combo_stats_stat_name, false, false, 0);
		hbox_combo_stats_stat_name.ShowAll();
		
		combo_stats_stat_name.Sensitive = true;
	}
	
	private void createComboStats2() {
		combo_stats_stat_name2 = new Combo ();
		
		combo_stats_stat_name2.DisableActivate ();
		combo_stats_stat_name2.Entry.Changed += new EventHandler (on_combo_stats_stat_name2_changed);

		hbox_combo_stats_stat_name2.PackStart(combo_stats_stat_name2, false, false, 0);
		hbox_combo_stats_stat_name2.ShowAll();
		
		combo_stats_stat_name2.Sensitive = true;
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

	//way of accessing from chronojump.cs
	public void FillTreeView_stats (bool graph, bool force) 
	{
		//ask for statsAutomatic, because chronojump.cs doesn't know this
		if(statsAutomatic || force) {
			fillTreeView_stats(graph);
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
			int jumperID = Convert.ToInt32(Util.FetchID(statistic));
			string jumperName = Util.FetchName(statistic);
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

	
	//called from chronojump.cs for showing or hiding some widgets
	//when a person is created or loaded 
	public void Widgets(bool person)
	{
		if(person) {
			combo_stats_stat_name.Sensitive = true;
			combo_stats_stat_name2.Sensitive = true;
		} else {
			combo_stats_stat_name.Sensitive = false;
			combo_stats_stat_name2.Sensitive = false;
		}
	}
	
	
	
	/* ---------------------------------------------------------
	 * ----------------  STATS CALLBACKS--------------------
	 *  --------------------------------------------------------
	 */

	private void on_button_stats_clicked (object o, EventArgs args) {
		fillTreeView_stats(false);
	}

	private void on_button_graph_clicked (object o, EventArgs args) {
		fillTreeView_stats(true);
	}

	
	
	private void on_checkbutton_stats_always_clicked(object o, EventArgs args) 
	{
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
		fillTreeView_stats(false);
	}
	
	private void on_checkbutton_stats_sex_clicked(object o, EventArgs args)
	{
		fillTreeView_stats(false);
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
		fillTreeView_stats(false);
	}
	
	void on_spinbutton_stats_jumps_changed (object o, EventArgs args)
	{
		if (statsAutomatic) { 
			fillTreeView_stats(false);
		}
	}

	
	
	private void on_button_stats_select_sessions_clicked (object o, EventArgs args) {
		Console.WriteLine("select sessions for stats");
		//sessionSelectStatsWin = SessionSelectStatsWindow.Show(app1, selectedSessions);
		sessionSelectStatsWin = SessionSelectStatsWindow.Show(stats_window, selectedSessions);
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
		fillTreeView_stats(false);
	}
	
	void on_button_close_clicked (object o, EventArgs args)
	{
		StatsWindowBox.stats_window.Hide();
		StatsWindowBox = null;
	}
	
	void on_stats_window_delete_event (object o, EventArgs args)
	{
		StatsWindowBox.stats_window.Hide();
		StatsWindowBox = null;
	}
	
	
	public int PrefsDigitsNumber 
	{
		set {
			prefsDigitsNumber = value;
		}
	}
	
	public bool HeightPreferred 
	{
		set {
			heightPreferred = value;
		}
	}
	
	public bool WeightStatsPercent
	{
		set {
			weightStatsPercent = value;
		}
	}

}

