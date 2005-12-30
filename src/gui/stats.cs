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
	[Widget] Gtk.Button button_add_to_report;
	
	[Widget] Gtk.TextView textview_enunciate;
	[Widget] Gtk.ScrolledWindow scrolledwindow_enunciate;
	[Widget] Gtk.CheckButton checkbutton_show_enunciate;
	
	[Widget] Gtk.Box hbox_mark_consecutives;
	[Widget] Gtk.CheckButton checkbutton_mark_consecutives;
	[Widget] Gtk.SpinButton spinbutton_mark_consecutives;
	
	[Widget] Gtk.Box hbox_combo_select_checkboxes;
	[Widget] Gtk.Combo combo_select_checkboxes;

	int prefsDigitsNumber;
	bool heightPreferred;
	//bool weightStatsPercent;
	
	bool statsAutomatic = true;
	bool statsColumnsToRemove = false;
	private Session currentSession;
	bool changingCombos = false;
	//selected sessions
	ArrayList selectedSessions;
	
	//private Stat myStat; 
	private StatType myStatType;
	
	//optimization
	private bool blockFillingTreeview;

	private static string [] comboStatsTypeOptions = {
		Catalog.GetString("Global"), 
		Catalog.GetString("Jumper"),
		Catalog.GetString("Simple"),
		Catalog.GetString("With TC"),
		Catalog.GetString("Reactive"),
	};
	
	private static string [] comboStatsSubTypeWithTCOptions = {
		Constants.DjIndexFormula,
		Constants.QIndexFormula
	};
	
	private static string [] comboStatsSubTypeReactiveOptions = {
		Catalog.GetString("Average Index"), 
		Catalog.GetString("POTENCY (Bosco)"), // 9.81^2*TV*TT / (4*jumps*(TT-TV))
		Catalog.GetString("Evolution") 
	};
	
	private static string [] comboStatsSubTypeSimpleOptions = {
		Catalog.GetString("No indexes"), 
		Constants.FvIndexFormula,
		Constants.IeIndexFormula, 
		Constants.IubIndexFormula
	};
		
	private static string [] comboCheckboxesOptions = {
		Catalog.GetString("All"),
		Catalog.GetString("None"),
		Catalog.GetString("Invert"),
		Catalog.GetString("Selected"),
		Catalog.GetString("Male"),
		Catalog.GetString("Female")
	};

	ArrayList sendSelectedSessions;
	
	Report report;
	ReportWindow reportWin;

	
	StatsWindow (Gtk.Window parent, Session currentSession, 
			//int prefsDigitsNumber, bool weightStatsPercent, bool heightPreferred, 
			int prefsDigitsNumber, bool heightPreferred, 
			Report report, ReportWindow reportWin)
	{
		Glade.XML gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "stats_window", null);

		gladeXML.Autoconnect(this);
		this.parent = parent;
		this.currentSession = currentSession;
		this.prefsDigitsNumber = prefsDigitsNumber;
		//this.weightStatsPercent = weightStatsPercent;
		this.heightPreferred = heightPreferred;

		this.report = report;
		this.reportWin= reportWin;

		//myStat = new Stat(); //create and instance of myStat
		myStatType = new StatType();

		//this doesn't allow treeview to be recreated many times (4)
		//in all the combos that are going to be created
		blockFillingTreeview = true;
		
		createComboSelectCheckboxes();

		createComboStatsType();
		createComboStatsSubType();
		createComboStatsApplyTo();
	
		// here doesn't do Ok the job, done later in Initialize
		//blockFillingTreeview = false;

		updateComboStats();
			
		
		//textview_enunciate.Hide();
		//scrolledwindow_enunciate.Hide();
			
		spinbutton_mark_consecutives.Sensitive = false;
		hbox_mark_consecutives.Hide();
	}
	

	static public StatsWindow Show (Gtk.Window parent, Session currentSession, 
			//int prefsDigitsNumber, bool weightStatsPercent, bool heightPreferred, 
			int prefsDigitsNumber, bool heightPreferred, 
			Report report, ReportWindow reportWin)
	{
		if (StatsWindowBox == null) {
			StatsWindowBox = new StatsWindow (parent, currentSession, 
					//prefsDigitsNumber, weightStatsPercent, heightPreferred, 
					prefsDigitsNumber, heightPreferred, 
					report, reportWin);
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

		//now will be the first time the treeview is updated
		blockFillingTreeview = false;
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

	private void createComboSelectCheckboxes() {
		combo_select_checkboxes = new Combo ();
		combo_select_checkboxes.PopdownStrings = comboCheckboxesOptions;
		
		//combo_select_checkboxes.DisableActivate ();
		combo_select_checkboxes.Entry.Changed += new EventHandler (on_combo_select_checkboxes_changed);

		hbox_combo_select_checkboxes.PackStart(combo_select_checkboxes, false, false, 0);
		hbox_combo_select_checkboxes.ShowAll();
		
		combo_select_checkboxes.Sensitive = true;
	}
	
	private void on_combo_select_checkboxes_changed(object o, EventArgs args) {
		string myText = combo_select_checkboxes.Entry.Text;
			
		if (myText != "" & myText != Catalog.GetString("Selected")) {
			try {
				//if selected 'male' or 'female', showSex and redo the treeview if needed
				if (myText == Catalog.GetString("Male") ||
						myText == Catalog.GetString("Female")) {
					if( ! checkbutton_stats_sex.Active) {
						//this will redo the treeview
						checkbutton_stats_sex.Active = true;
						//put another time the value Male or Female in combo_select_checkboxes
						combo_select_checkboxes.Entry.Text = myText;
					}
				}
				
				myStatType.MarkSelected(myText);
			} catch {
				Console.WriteLine("Do later!!");
			}
		}
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
				SqlitePersonSession.SelectCurrentSession(currentSession.UniqueID, false); //not reversed
			combo_stats_stat_apply_to.Sensitive = true;
		} 
		else if (combo_stats_stat_type.Entry.Text == Catalog.GetString("Simple") ) 
		{
			combo_stats_stat_subtype.PopdownStrings = comboStatsSubTypeSimpleOptions;
			combo_stats_stat_subtype.Sensitive = true;
			
			//by default show all simple nonTC jumps, but if combo_stats_subtype changed
			//updateComboStatsSubType() will do the work
			combo_stats_stat_apply_to.PopdownStrings = 
				SqliteJumpType.SelectJumpTypes(Constants.AllJumpsName, "nonTC", true); //only select name
			combo_stats_stat_apply_to.Sensitive = true;
		} 
		else if (combo_stats_stat_type.Entry.Text == Catalog.GetString("With TC") ) 
		{
			combo_stats_stat_subtype.PopdownStrings = comboStatsSubTypeWithTCOptions;
			combo_stats_stat_subtype.Sensitive = true;
			
			combo_stats_stat_apply_to.PopdownStrings = 
				SqliteJumpType.SelectJumpTypes(Constants.AllJumpsName, "TC", true); //only select name
			combo_stats_stat_apply_to.Sensitive = true;
		} 
		else if (combo_stats_stat_type.Entry.Text == Catalog.GetString("Reactive") ) 
		{
			combo_stats_stat_subtype.PopdownStrings = comboStatsSubTypeReactiveOptions;
			combo_stats_stat_subtype.Sensitive = true;
			
			combo_stats_stat_apply_to.PopdownStrings = 
				SqliteJumpType.SelectJumpRjTypes(Constants.AllJumpsName, true); //only select name
			combo_stats_stat_apply_to.Sensitive = true;
		}

		fillTreeView_stats(false);
	}

	private void updateComboStatsSubType() {
		if (combo_stats_stat_type.Entry.Text == Catalog.GetString("Simple") ) 
		{
			if(combo_stats_stat_subtype.Entry.Text == Catalog.GetString("No indexes")) {
				combo_stats_stat_apply_to.PopdownStrings = 
					SqliteJumpType.SelectJumpTypes(Constants.AllJumpsName, "nonTC", true); //only select name
				combo_stats_stat_apply_to.Sensitive = true;
			} else if (combo_stats_stat_subtype.Entry.Text == Constants.IeIndexFormula) {
				combo_stats_stat_apply_to.Entry.Text = "CMJ, SJ";
				combo_stats_stat_apply_to.Sensitive = false;
			} else if (combo_stats_stat_subtype.Entry.Text == Constants.IubIndexFormula) {
				combo_stats_stat_apply_to.Entry.Text = "ABK, CMJ";
				combo_stats_stat_apply_to.Sensitive = false;
			} else {
				//"F/V sj+(100%)/sj *100",	//fvIndexFormula
				combo_stats_stat_apply_to.Entry.Text = "SJ+(100%), SJ";
				combo_stats_stat_apply_to.Sensitive = false;
			}
		}  else if (combo_stats_stat_type.Entry.Text == Catalog.GetString("With TC") ) 
		{
			combo_stats_stat_apply_to.PopdownStrings = 
				SqliteJumpType.SelectJumpTypes(Constants.AllJumpsName, "TC", true); //only select name
			combo_stats_stat_apply_to.Sensitive = true;
		} 
		
		if (combo_stats_stat_subtype.Entry.Text == Catalog.GetString("Evolution") )  {
			hbox_mark_consecutives.Show();
		} else {
			hbox_mark_consecutives.Hide();
		}
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
		if(blockFillingTreeview) {
			return false;
		}
		
		Console.WriteLine("----------FILLING treeview stats---------------");
		
		string statisticType = combo_stats_stat_type.Entry.Text;
		string statisticSubType = combo_stats_stat_subtype.Entry.Text;
		string statisticApplyTo = combo_stats_stat_apply_to.Entry.Text;

		if(statsColumnsToRemove && !graph) {
			statsRemoveColumns();
		}
		statsColumnsToRemove = true;

		bool toReport = false; //all graphs are down for showing in window (not to file like report.cs)
		
		int statsJumpsType = 0;
		int limit = -1;
		if (radiobutton_stats_jumps_all.Active) {
			statsJumpsType = 0;
			limit = -1;
		} else if (radiobutton_stats_jumps_limit.Active) {
			statsJumpsType = 1;
			limit = Convert.ToInt32 ( spin_stats_jumps_limit.Value ); 
		} else if (radiobutton_stats_jumps_person_bests.Active) {
			statsJumpsType = 2;
			limit = Convert.ToInt32 ( spin_stats_jumps_person_bests.Value ); 
		} else {
			statsJumpsType = 3;
			limit = -1;
		}

		//we use sendSelectedSessions for not losing selectedSessions ArrayList 
		//everytime user cicles the sessions select radiobuttons
		sendSelectedSessions = new ArrayList(2);
		if (radiobutton_current_session.Active) {
			sendSelectedSessions.Add (currentSession.UniqueID + ":" + currentSession.Name + ":" + currentSession.Date); 
		} else if (radiobutton_selected_sessions.Active) {
			sendSelectedSessions = selectedSessions;
		}

		int rj_evolution_mark_consecutives = -1;
		if (combo_stats_stat_subtype.Entry.Text == Catalog.GetString("Evolution") &&
			checkbutton_mark_consecutives.Active ) {
			rj_evolution_mark_consecutives = Convert.ToInt32 ( spinbutton_mark_consecutives.Value ); 
		}

		ArrayList markedRows = new ArrayList();
		if(graph) {
			markedRows = myStatType.MarkedRows;
		}

		//if we change combo_type, subtype, or others, always, show button_graph & add_to_report,
		//if there's no data, they will be hided, later
		button_graph.Sensitive = true;
		button_add_to_report.Sensitive = true;
		
		myStatType = new StatType(
				statisticType,
				statisticSubType,
				statisticApplyTo,
				treeview_stats,
				sendSelectedSessions, 
				prefsDigitsNumber, 
				checkbutton_stats_sex.Active,  
				statsJumpsType,
				limit, 
				heightPreferred,
				//weightStatsPercent, 
				markedRows,
				rj_evolution_mark_consecutives,
				graph,
				toReport  //always false in this class
				);

		bool allFine = myStatType.ChooseStat();
	
		myStatType.FakeButtonRowCheckedUnchecked.Clicked += 
			new EventHandler(on_fake_button_row_checked_clicked);
		myStatType.FakeButtonRowsSelected.Clicked += 
			new EventHandler(on_fake_button_rows_selected_clicked);
		myStatType.FakeButtonNoRowsSelected.Clicked += 
			new EventHandler(on_fake_button_no_rows_selected_clicked);

		//useful for not showing button_graph & add_to_report when there are no rows
		try {
			if(myStatType.MarkedRows.Count == 0) {
				button_graph.Sensitive = false;
				button_add_to_report.Sensitive = false;
			}
		} catch {
			Console.WriteLine("Do markedRows stuff later");
		}

		//every time a stat is created, all rows should be checked (except AVG & SD)
		combo_select_checkboxes.Entry.Text = Catalog.GetString("All");

		//show enunciate of the stat in textview_enunciate
		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.SetText(myStatType.Enunciate);
		textview_enunciate.Buffer = tb;
		tb.SetText(myStatType.Enunciate);
		
		if(allFine) {
			return true;
		} else {
			return false;
		}
	}

	//changes the combo_select_checkboxes to "Selected" if any row in the treeview is checked or unchecked
	private void on_fake_button_row_checked_clicked (object o, EventArgs args) {
		Console.WriteLine("fakeButtonRowCheckedUnchecked in gui/stats.cs !!");

		combo_select_checkboxes.Entry.Text = Catalog.GetString("Selected");
	}
	
	private void on_fake_button_rows_selected_clicked (object o, EventArgs args) {
		Console.WriteLine("fakeButtonRowsSelected in gui/stats.cs !!");
		button_graph.Sensitive = true;
		button_add_to_report.Sensitive = true;
	}
	
	private void on_fake_button_no_rows_selected_clicked (object o, EventArgs args) {
		Console.WriteLine("fakeButtonNoRowsSelected in gui/stats.cs !!");
		button_graph.Sensitive = false;
		button_add_to_report.Sensitive = false;

		//put none in combo
		combo_select_checkboxes.Entry.Text = Catalog.GetString("None");
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

	
	private void on_checkbutton_mark_consecutives_clicked(object o, EventArgs args) {
		if(checkbutton_mark_consecutives.Active) {
			spinbutton_mark_consecutives.Sensitive = true;
		} else {
			spinbutton_mark_consecutives.Sensitive = false;
		}
		
		if (statsAutomatic) { 
			fillTreeView_stats(false);
		}
	}
	
	void on_spinbutton_mark_consecutives_changed (object o, EventArgs args) {
		if (statsAutomatic) { 
			fillTreeView_stats(false);
		}
	}

	
	private void update_stats_widgets_sensitiveness() {
		string statisticType = combo_stats_stat_type.Entry.Text;
		string statisticSubType = combo_stats_stat_subtype.Entry.Text;
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
					( statisticType == Catalog.GetString("Simple") && 
					 statisticSubType != Catalog.GetString("No indexes") ) ||
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
		
		updateComboStatsSubType();
		update_stats_widgets_sensitiveness();
		
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
	
	private void on_button_add_to_report_clicked (object o, EventArgs args) {
		Console.WriteLine("add to report window");

		string statisticType = combo_stats_stat_type.Entry.Text;
		string statisticSubType = combo_stats_stat_subtype.Entry.Text;
		if(statisticType == "" || statisticSubType == "") {
			//for an unknown reason, when we select an option in the combo stats, 
			//the on_combo_stats_stat_type_changed it's called two times? 
			//in the first the value of Entry.Text is "";
			return;
		} else {
			if (statisticSubType == Catalog.GetString("Evolution") &&
				checkbutton_mark_consecutives.Active ) {
				statisticSubType += "." + ( spinbutton_mark_consecutives.Value ).ToString(); 
			}
			
			string statisticApplyTo = combo_stats_stat_apply_to.Entry.Text;
			if(statisticApplyTo.Length == 0) {
				statisticApplyTo = "-";
			}
	
			string statsShowJumps = "";
			if (radiobutton_stats_jumps_all.Active) {
				statsShowJumps = Catalog.GetString("All");
			} else if (radiobutton_stats_jumps_limit.Active) {
				statsShowJumps = Catalog.GetString("Limit") + "." + 
					spin_stats_jumps_limit.Value.ToString(); 
			} else if (radiobutton_stats_jumps_person_bests.Active) {
				statsShowJumps = Catalog.GetString("Jumper's best") + "." + 
					spin_stats_jumps_person_bests.Value.ToString(); 
			} else {
				statsShowJumps = Catalog.GetString("Jumper's average");
			}

			bool showSex = false;
			if(checkbutton_stats_sex.Active) {
				showSex = true;
			}
			
			//create or show the report window
			reportWin = ReportWindow.Show(parent, report);
			//add current stat
			reportWin.Add(statisticType, statisticSubType, statisticApplyTo, 
					//sessionsAsAString, statsShowJumps, showSex.ToString());
					sendSelectedSessions, statsShowJumps, showSex.ToString(), 
					myStatType.MarkedRows);
					
		}
		
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

	/*
	public bool WeightStatsPercent
	{
		set {
			weightStatsPercent = value;
		}
	}
	*/

}

