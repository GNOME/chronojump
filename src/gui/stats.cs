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
	[Widget] Gtk.Box hbox_combo_stats_stat_type;
	[Widget] Gtk.Box hbox_combo_stats_stat_subtype;
	[Widget] Gtk.Box hbox_combo_stats_stat_apply_to;
	[Widget] Gtk.Combo combo_stats_stat_type;
	[Widget] Gtk.Combo combo_stats_stat_subtype;
	[Widget] Gtk.Combo combo_stats_stat_apply_to;
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
	bool changingCombos = false;
	//selected sessions
	ArrayList selectedSessions;
	
	//useful for deleting headers of lastStat just before making a new Stat
	private Stat myStat; 
	
	private string allJumpsName = Catalog.GetString("All jumps");
	
	private static string [] comboStatsTypeOptions = {
		Catalog.GetString("Global"), 
		Catalog.GetString("Jumper"),
		Catalog.GetString("Simple"),
		Catalog.GetString("With TC"),
		Catalog.GetString("Reactive"),
		Catalog.GetString("Indexes")
	};
	
	private static string [] comboStatsSubTypeWithTCOptions = {
		Catalog.GetString("Dj Index") + " ((tv-tc)*100/tc)",
		Catalog.GetString("Q index") + " (tv/tc)" 
	};
	
	private static string [] comboStatsSubTypeReactiveOptions = {
		Catalog.GetString("Average Index"), 
		Catalog.GetString("POTENCY (Bosco)"), // 9.81^2*TV*TT / (4*jumps*(TT-TV))
		Catalog.GetString("Evolution") 
	};
	
	private static string [] comboStatsSubTypeIndexesOptions = {
		"IE (cmj-sj)*100/sj", 
		"IUB (abk-cmj)*100/cmj"
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
		
		createComboStatsType();
		createComboStatsSubType();
		createComboStatsApplyTo();
		
		updateComboStats();
			
		textview_enunciate.Hide();
		scrolledwindow_enunciate.Hide();
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

	public void Hide()
	{
		StatsWindowBox.stats_window.Hide ();
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

	private void createComboStatsType() {
		combo_stats_stat_type = new Combo ();
		combo_stats_stat_type.PopdownStrings = comboStatsTypeOptions;
		
		//combo_stats_stat_type.DisableActivate ();
		combo_stats_stat_type.Entry.Changed += new EventHandler (on_combo_stats_stat_type_changed);

		hbox_combo_stats_stat_type.PackStart(combo_stats_stat_type, false, false, 0);
		hbox_combo_stats_stat_type.ShowAll();
		
		combo_stats_stat_type.Sensitive = true;
	}
	
	private void createComboStatsSubType() {
		combo_stats_stat_subtype = new Combo ();
		
		//combo_stats_stat_subtype.DisableActivate ();
		combo_stats_stat_subtype.Entry.Changed += new EventHandler (on_combo_stats_stat_subtype_changed);

		hbox_combo_stats_stat_subtype.PackStart(combo_stats_stat_subtype, false, false, 0);
		hbox_combo_stats_stat_subtype.ShowAll();
		
		combo_stats_stat_subtype.Sensitive = true;
	}

	private void createComboStatsApplyTo() {
		combo_stats_stat_apply_to = new Combo ();
		
		//combo_stats_stat_apply_to.DisableActivate ();
		combo_stats_stat_apply_to.Entry.Changed += new EventHandler (on_combo_stats_stat_apply_to_changed);

		hbox_combo_stats_stat_apply_to.PackStart(combo_stats_stat_apply_to, false, false, 0);
		hbox_combo_stats_stat_apply_to.ShowAll();
		
		combo_stats_stat_apply_to.Sensitive = true;
	}

	private void updateComboStats() {
		string [] nullOptions = { "-" };
		if(combo_stats_stat_type.Entry.Text == Catalog.GetString("Global") ) 
		{
			combo_stats_stat_subtype.PopdownStrings = nullOptions;
			combo_stats_stat_subtype.Sensitive = false;
			
			combo_stats_stat_apply_to.PopdownStrings = nullOptions;
			combo_stats_stat_apply_to.Sensitive = false;
		}
		else if(combo_stats_stat_type.Entry.Text == Catalog.GetString("Jumper") )
		{
			combo_stats_stat_subtype.PopdownStrings = nullOptions;
			combo_stats_stat_subtype.Sensitive = false;
			
			combo_stats_stat_apply_to.PopdownStrings = 
				SqlitePersonSession.SelectCurrentSession(currentSession.UniqueID);
			combo_stats_stat_apply_to.Sensitive = true;
		} else if (combo_stats_stat_type.Entry.Text == Catalog.GetString("Simple") ) 
		{
			combo_stats_stat_subtype.PopdownStrings = nullOptions;
			combo_stats_stat_subtype.Sensitive = false;
			
			combo_stats_stat_apply_to.PopdownStrings = 
				SqliteJumpType.SelectJumpTypes(allJumpsName, "nonTC", true); //only select name
			combo_stats_stat_apply_to.Sensitive = true;
		} else if (combo_stats_stat_type.Entry.Text == Catalog.GetString("With TC") ) 
		{
			combo_stats_stat_subtype.PopdownStrings = comboStatsSubTypeWithTCOptions;
			combo_stats_stat_subtype.Sensitive = true;
			
			combo_stats_stat_apply_to.PopdownStrings = 
				SqliteJumpType.SelectJumpTypes(allJumpsName, "TC", true); //only select name
			combo_stats_stat_apply_to.Sensitive = true;
		} else if (combo_stats_stat_type.Entry.Text == Catalog.GetString("Reactive") ) 
		{
			combo_stats_stat_subtype.PopdownStrings = comboStatsSubTypeReactiveOptions;
			combo_stats_stat_subtype.Sensitive = true;
			
			combo_stats_stat_apply_to.PopdownStrings = 
				SqliteJumpType.SelectJumpRjTypes(allJumpsName, true); //only select name
			combo_stats_stat_apply_to.Sensitive = true;
		} else if (combo_stats_stat_type.Entry.Text == Catalog.GetString("Indexes") ) 
		{
			combo_stats_stat_subtype.PopdownStrings = comboStatsSubTypeIndexesOptions;
			combo_stats_stat_subtype.Sensitive = true;
			
			combo_stats_stat_apply_to.PopdownStrings = nullOptions;
			combo_stats_stat_apply_to.Sensitive = false;
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

	//private void fillTreeView_stats (bool graph) 
	private bool fillTreeView_stats (bool graph) 
	{
		string statisticType = combo_stats_stat_type.Entry.Text;
		string statisticSubType = combo_stats_stat_subtype.Entry.Text;
		string statisticApplyTo = combo_stats_stat_apply_to.Entry.Text;

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

		
		if ( statisticType == Catalog.GetString("Global") ) {
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
		else if (statisticType == Catalog.GetString("Jumper"))
		{
			if(statisticApplyTo.Length == 0) {
				Console.WriteLine("Jumper-ret");
				return false;
			}
			int jumperID = Convert.ToInt32(Util.FetchID(statisticApplyTo));
			if(jumperID == -1) {
				return false;
			}
			
			string jumperName = Util.FetchName(statisticApplyTo);
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
		else if(statisticType == Catalog.GetString("Simple"))
		{
			if(statisticApplyTo.Length == 0) {
				Console.WriteLine("Simple-ret");
				return false;
			}
			JumpType myType = new JumpType(statisticApplyTo);

			//manage all weight jumps and the "All jumps" (simple)
			if(myType.HasWeight || 
					statisticApplyTo == allJumpsName) 
			{
				if(graph) {
					myStat = new GraphSjCmjAbkPlus ( 
							sendSelectedSessions, 
							prefsDigitsNumber, statisticApplyTo, 
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
							prefsDigitsNumber, statisticApplyTo, 
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
							prefsDigitsNumber, statisticApplyTo, 
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
							prefsDigitsNumber, statisticApplyTo, 
							checkbutton_stats_sex.Active, 
							statsJumpsType,
							limit,
							heightPreferred
							);
					myStat.PrepareData();
				}
			}
		}
		else if(statisticType == Catalog.GetString("With TC"))
		{
			if(statisticApplyTo.Length == 0) {
				Console.WriteLine("WithTC-ret");
				return false;
			}
			
			if(statisticSubType == Catalog.GetString("Dj Index") + " ((tv-tc)*100/tc)")
			{
				if(graph) {
					myStat = new GraphDjIndex ( 
							sendSelectedSessions, 
							prefsDigitsNumber, statisticApplyTo, 
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
							prefsDigitsNumber, statisticApplyTo, 
							checkbutton_stats_sex.Active,
							statsJumpsType,
							limit//, 
							//heightPreferred
							);
					myStat.PrepareData();
				}
			} else if(statisticSubType == Catalog.GetString("Q index") + " (tv/tc)")
			{
				if(graph) {
					myStat = new GraphDjQ ( 
							sendSelectedSessions, 
							prefsDigitsNumber, statisticApplyTo, 
							checkbutton_stats_sex.Active, 
							statsJumpsType,
							limit//,
							//heightPreferred
							);
					myStat.PrepareData();
					myStat.CreateGraph();
				} else {
					myStat = new StatDjQ(treeview_stats, 
							sendSelectedSessions, 
							prefsDigitsNumber, statisticApplyTo, 
							checkbutton_stats_sex.Active,
							statsJumpsType,
							limit//, 
							//heightPreferred
							);
					myStat.PrepareData();
				}
			}
		}
		else if(statisticType == Catalog.GetString("Reactive")) {
			if(statisticSubType == Catalog.GetString("Average Index"))
			{
				if(graph) {
					myStat = new GraphRjIndex ( 
							sendSelectedSessions, 
							prefsDigitsNumber, 
							statisticApplyTo, 
							checkbutton_stats_sex.Active, 
							statsJumpsType,
							limit);
					myStat.PrepareData();
					myStat.CreateGraph();
				} else {
					myStat = new StatRjIndex(treeview_stats, 
							sendSelectedSessions, 
							prefsDigitsNumber, 
							statisticApplyTo, 
							checkbutton_stats_sex.Active,
							statsJumpsType,
							limit);
					myStat.PrepareData();
				}
			}	
			else if(statisticSubType == Catalog.GetString("POTENCY (Bosco)"))
			{
				if(graph) {
					myStat = new GraphRjPotencyBosco ( 
							sendSelectedSessions, 
							prefsDigitsNumber, 
							statisticApplyTo, 
							checkbutton_stats_sex.Active, 
							statsJumpsType,
							limit);
					myStat.PrepareData();
					myStat.CreateGraph();
				} else {
					myStat = new StatRjPotencyBosco(treeview_stats, 
							sendSelectedSessions, 
							prefsDigitsNumber, 
							statisticApplyTo, 
							checkbutton_stats_sex.Active, 
							statsJumpsType,
							limit);
					myStat.PrepareData();
				}
			}
			else if(statisticSubType == Catalog.GetString("Evolution"))
			{
				if(graph) {
					myStat = new GraphRjEvolution ( 
							sendSelectedSessions, 
							prefsDigitsNumber, 
							statisticApplyTo, 
							checkbutton_stats_sex.Active, 
							statsJumpsType,
							limit);
					myStat.PrepareData();
					myStat.CreateGraph();
				} else {
					myStat = new StatRjEvolution(treeview_stats, 
							sendSelectedSessions, 
							prefsDigitsNumber, 
							statisticApplyTo, 
							checkbutton_stats_sex.Active, 
							statsJumpsType,
							limit);
					myStat.PrepareData();
				}
			}
		}
		else if(statisticType == Catalog.GetString("Indexes")) {
			if(statisticSubType == "IE (cmj-sj)*100/sj")
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
			else if(statisticSubType == "IUB (abk-cmj)*100/cmj")
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

		//all was fine
		return true;
	}

	
	//called from chronojump.cs for showing or hiding some widgets
	//when a person is created or loaded 
	public void Widgets(bool person)
	{
		if(person) {
			combo_stats_stat_type.Sensitive = true;
			combo_stats_stat_subtype.Sensitive = true;
			combo_stats_stat_apply_to.Sensitive = true;
		} else {
			combo_stats_stat_type.Sensitive = false;
			combo_stats_stat_subtype.Sensitive = false;
			combo_stats_stat_apply_to.Sensitive = false;
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
		string statisticType = combo_stats_stat_type.Entry.Text;
		string statisticSubType = combo_stats_stat_subtype.Entry.Text;
		//string statisticApplyTo = combo_stats_stat_apply_to.Entry.Text;
		if(statisticType == "" || statisticSubType == "") {
			//for an unknown reason, when we select an option in the combo stats, 
			//the on_combo_stats_stat_type_changed it's called two times? 
			//in the first the value of Entry.Text is "";
			return;
		} else {
			//some stats should not be showed as limited jumps
			if(statisticType == Catalog.GetString("Reactive") && 
					statisticSubType == Catalog.GetString("Evolution") ) {
				//don't allow Evolution be multisession
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
			else if(statisticType == Catalog.GetString("Global") || 
					statisticType == Catalog.GetString("Jumper") || 
					statisticType == Catalog.GetString("Indexes") || 
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

	private void on_combo_stats_stat_type_changed(object o, EventArgs args) {
		//update combo stats_subtype, there change the treeviewstats (with the combostats_subtype values changed)
		
		updateComboStats();
		
		update_stats_widgets_sensitiveness();
	}
	
	private void on_combo_stats_stat_subtype_changed(object o, EventArgs args) {
			
		//for an unknown reason, when we select an option in the combo stats, 
		//the on_combo_stats_stat_type_changed it's called two times? 
		//in the first the value of Entry.Text is "";
		
		string myText = combo_stats_stat_type.Entry.Text;
		string myText2 = combo_stats_stat_subtype.Entry.Text;
		string myText3 = combo_stats_stat_apply_to.Entry.Text;
		if (myText != "" && (myText2 != "" || myText3 !="") ) {
			fillTreeView_stats(false);
		}
	}
	
	private void on_combo_stats_stat_apply_to_changed(object o, EventArgs args) {
			
		//for an unknown reason, when we select an option in the combo stats, 
		//the on_combo_stats_stat_type_changed it's called two times? 
		//in the first the value of Entry.Text is "";
		string myText = combo_stats_stat_type.Entry.Text;
		string myText2 = combo_stats_stat_subtype.Entry.Text;
		string myText3 = combo_stats_stat_apply_to.Entry.Text;
		if (myText != "" && (myText2 != "" || myText3 !="") ) {
			fillTreeView_stats(false);
		}
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

