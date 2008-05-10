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
using Gdk;
using Glade;
using GLib; //for Value
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using Mono.Unix;


public class StatsWindow {
	
	[Widget] Gtk.Window stats_window;
	static StatsWindow StatsWindowBox;
	Gtk.Window parent;
	SessionSelectStatsWindow sessionSelectStatsWin;

	[Widget] Gtk.TreeView treeview_stats;
	[Widget] Gtk.Box hbox_combo_stats_stat_type;
	[Widget] Gtk.Box hbox_combo_stats_stat_subtype;
	[Widget] Gtk.Box hbox_combo_stats_stat_apply_to;
	[Widget] Gtk.ComboBox combo_stats_stat_type;
	[Widget] Gtk.ComboBox combo_stats_stat_subtype;
	[Widget] Gtk.ComboBox combo_stats_stat_apply_to;
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
	[Widget] Gtk.ComboBox combo_select_checkboxes;
	
	[Widget] Gtk.Image image_stats_win_graph;
	[Widget] Gtk.Image image_stats_win_report;
	[Widget] Gtk.Statusbar statusbar_stats;

	int prefsDigitsNumber;
	bool heightPreferred;
	bool weightStatsPercent;
	
	//bool statsAutomatic = true;
	bool statsAutomatic = false;
	bool statsColumnsToRemove = false;
	private Session currentSession;
	bool changingCombos = false;
	//selected sessions
	ArrayList selectedSessions;
	
	//private Stat myStat; 
	private StatType myStatType;
	
	Gtk.TreeStore lastStore; 
	
	//optimization
	private bool blockFillingTreeview;

	private static string [] comboStatsTypeOptions = {
		Constants.TypeSessionSummary,
		Constants.TypeJumperSummary,
		Constants.TypeJumpsSimple,
		Constants.TypeJumpsSimpleWithTC,
		Constants.TypeJumpsReactive,
	};
	
	private static string [] comboStatsSubTypeWithTCOptions = {
		Constants.DjIndexFormula,
		Constants.QIndexFormula
	};
	
	private static string [] comboStatsSubTypeReactiveOptions = {
		Catalog.GetString("Average Index"), 
		Constants.RJPotencyBoscoFormula,
		Catalog.GetString("Evolution"),
		Constants.RJAVGSDRjIndexName,
		Constants.RJAVGSDQIndexName
	};
	
	private static string [] comboStatsSubTypeSimpleOptions = {
		Catalog.GetString("No indexes"), 
		Constants.FvIndexFormula,
		Constants.IeIndexFormula, 
		Constants.IubIndexFormula,
		Constants.PotencyLewisFormula,
		Constants.PotencyHarmanFormula,
		Constants.PotencySayersSJFormula,
		Constants.PotencySayersCMJFormula,
		Constants.PotencyShettyFormula,
		Constants.PotencyCanavanFormula,
		//Constants.PotencyBahamondeFormula,
		Constants.PotencyLaraMaleApplicantsSCFormula,
		Constants.PotencyLaraFemaleEliteVoleiFormula,
		Constants.PotencyLaraFemaleMediumVoleiFormula,
		Constants.PotencyLaraFemaleSCStudentsFormula,
		Constants.PotencyLaraFemaleSedentaryFormula
	};
		

	private static string [] comboCheckboxesOptions = {
		Catalog.GetString("All"),
		Catalog.GetString("None"),
		Catalog.GetString("Invert"),
		Catalog.GetString("Selected"),
		Catalog.GetString("Male"),
		Catalog.GetString("Female")
	};
	
	//useful for removing users from the combo
	private static string [] comboCheckboxesOptionsWithoutPersons = comboCheckboxesOptions;

	ArrayList sendSelectedSessions;
	
	Report report;
	ReportWindow reportWin;

	
	StatsWindow (Gtk.Window parent, Session currentSession, 
			int prefsDigitsNumber, bool weightStatsPercent, bool heightPreferred, 
			Report report, ReportWindow reportWin)
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "stats_window", null);
		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(stats_window);

		this.currentSession = currentSession;
		this.prefsDigitsNumber = prefsDigitsNumber;
		this.weightStatsPercent = weightStatsPercent;
		this.heightPreferred = heightPreferred;

		this.report = report;
		this.reportWin= reportWin;

		//myStat = new Stat(); //create and instance of myStat
		myStatType = new StatType();

		//this doesn't allow treeview to be recreated many times (4)
		//in all the combos that are going to be created
		blockFillingTreeview = true;
		
		putNonStandardIcons();	

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
			int prefsDigitsNumber, bool weightStatsPercent, bool heightPreferred, 
			//int prefsDigitsNumber, bool heightPreferred, 
			Report report, ReportWindow reportWin)
	{
		if (StatsWindowBox == null) {
			StatsWindowBox = new StatsWindow (parent, currentSession, 
					prefsDigitsNumber, weightStatsPercent, heightPreferred, 
					//prefsDigitsNumber, heightPreferred, 
					report, reportWin);
		}
		
		//button update stats is unsensitive until a test finished
		StatsWindowBox.button_stats.Sensitive = false;

		StatsWindowBox.stats_window.Show ();
		
		return StatsWindowBox;
	}

	public void Hide()
	{
		StatsWindowBox.stats_window.Hide ();
	}
	
	private void putNonStandardIcons() {
		Pixbuf pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "gpm-statistics.png");
		image_stats_win_graph.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_task-assigned.png");
		image_stats_win_report.Pixbuf = pixbuf;
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
		combo_stats_stat_type = ComboBox.NewText ();
		UtilGtk.ComboUpdate(combo_stats_stat_type, comboStatsTypeOptions, "");
		combo_stats_stat_type.Active = 0;
		
		combo_stats_stat_type.Changed += new EventHandler (on_combo_stats_stat_type_changed);

		hbox_combo_stats_stat_type.PackStart(combo_stats_stat_type, true, true, 0);
		hbox_combo_stats_stat_type.ShowAll();
		combo_stats_stat_type.Sensitive = true;
	}
	
	private void createComboStatsSubType() {
		combo_stats_stat_subtype = ComboBox.NewText ();
		
		combo_stats_stat_subtype.Changed += new EventHandler (on_combo_stats_stat_subtype_changed);

		hbox_combo_stats_stat_subtype.PackStart(combo_stats_stat_subtype, true, true, 0);
		hbox_combo_stats_stat_subtype.ShowAll();
		combo_stats_stat_subtype.Sensitive = true;
	}

	private void createComboStatsApplyTo() {
		combo_stats_stat_apply_to = ComboBox.NewText ();
		
		combo_stats_stat_apply_to.Changed += new EventHandler (on_combo_stats_stat_apply_to_changed);

		hbox_combo_stats_stat_apply_to.PackStart(combo_stats_stat_apply_to, true, true, 0);
		hbox_combo_stats_stat_apply_to.ShowAll();
		combo_stats_stat_apply_to.Sensitive = true;
	}

	private void createComboSelectCheckboxes() {
		combo_select_checkboxes = ComboBox.NewText ();
		UtilGtk.ComboUpdate(combo_select_checkboxes, comboCheckboxesOptions, "");
		
		//combo_select_checkboxes.DisableActivate ();
		combo_select_checkboxes.Changed += new EventHandler (on_combo_select_checkboxes_changed);

		hbox_combo_select_checkboxes.PackStart(combo_select_checkboxes, true, true, 0);
		hbox_combo_select_checkboxes.ShowAll();
		combo_select_checkboxes.Sensitive = true;
	}
	
	private void on_combo_select_checkboxes_changed(object o, EventArgs args) {
		string myText = UtilGtk.ComboGetActive(combo_select_checkboxes);
			
		if (myText != "" & myText != Catalog.GetString("Selected")) {
			try {
				//if selected 'male' or 'female', showSex and redo the treeview if needed
				if (myText == Catalog.GetString("Male") ||
						myText == Catalog.GetString("Female")) {
					if( ! checkbutton_stats_sex.Active) {
						//this will redo the treeview
						checkbutton_stats_sex.Active = true;
						//put another time the value Male or Female in combo_select_checkboxes
						combo_select_checkboxes.Active = UtilGtk.ComboMakeActive(comboCheckboxesOptions, myText);
					}
				}
				
				myStatType.MarkSelected(myText);
				myStatType.CreateOrUpdateAVGAndSD();
			} catch {
				Log.WriteLine("Do later!!");
			}
		}
	}
	
	private string [] addPersonsToComboCheckBoxesOptions() {
		return Util.AddArrayString(comboCheckboxesOptionsWithoutPersons, Util.ArrayListToString(myStatType.PersonsWithData));
	}

	
	private void updateComboStats() {
		string [] nullOptions = { "-" };
		if(UtilGtk.ComboGetActive(combo_stats_stat_type) == Constants.TypeSessionSummary ) 
		{
			UtilGtk.ComboUpdate(combo_stats_stat_subtype, nullOptions, "");
			combo_stats_stat_subtype.Sensitive = false;
			
			UtilGtk.ComboUpdate(combo_stats_stat_apply_to, nullOptions, "");
			combo_stats_stat_apply_to.Sensitive = false;
		}
		else if(UtilGtk.ComboGetActive(combo_stats_stat_type) == Constants.TypeJumperSummary )
		{
			UtilGtk.ComboUpdate(combo_stats_stat_subtype, nullOptions, "");
			combo_stats_stat_subtype.Sensitive = false;
			
			UtilGtk.ComboUpdate(combo_stats_stat_apply_to,  
				SqlitePersonSession.SelectCurrentSession(currentSession.UniqueID, true, false), ""); //onlyIDAndName, not reversed
			combo_stats_stat_apply_to.Sensitive = true;
			combo_stats_stat_apply_to.Active = 0;
		} 
		else if (UtilGtk.ComboGetActive(combo_stats_stat_type) == Constants.TypeJumpsSimple ) 
		{
			UtilGtk.ComboUpdate(combo_stats_stat_subtype, comboStatsSubTypeSimpleOptions, "");
			combo_stats_stat_subtype.Sensitive = true;
			combo_stats_stat_subtype.Active = 0;
			
			//by default show all simple nonTC jumps, but if combo_stats_subtype changed
			//updateComboStatsSubType() will do the work
			UtilGtk.ComboUpdate(combo_stats_stat_apply_to, 
				SqliteJumpType.SelectJumpTypes(Constants.AllJumpsName, "nonTC", true), ""); //only select name
			combo_stats_stat_apply_to.Sensitive = true;
			combo_stats_stat_apply_to.Active = 0;
		} 
		else if (UtilGtk.ComboGetActive(combo_stats_stat_type) == Constants.TypeJumpsSimpleWithTC ) 
		{
			UtilGtk.ComboUpdate(combo_stats_stat_subtype, comboStatsSubTypeWithTCOptions, "");
			combo_stats_stat_subtype.Sensitive = true;
			combo_stats_stat_subtype.Active = 0;
			
			UtilGtk.ComboUpdate(combo_stats_stat_apply_to, 
				SqliteJumpType.SelectJumpTypes(Constants.AllJumpsName, "TC", true), ""); //only select name
			combo_stats_stat_apply_to.Sensitive = true;
			combo_stats_stat_apply_to.Active = 0;
		} 
		else if (UtilGtk.ComboGetActive(combo_stats_stat_type) == Constants.TypeJumpsReactive ) 
		{
			UtilGtk.ComboUpdate(combo_stats_stat_subtype, comboStatsSubTypeReactiveOptions, "");
			combo_stats_stat_subtype.Sensitive = true;
			combo_stats_stat_subtype.Active = 0;
			
			UtilGtk.ComboUpdate(combo_stats_stat_apply_to, 
				SqliteJumpType.SelectJumpRjTypes(Constants.AllJumpsName, true), ""); //only select name
			combo_stats_stat_apply_to.Sensitive = true;
			combo_stats_stat_apply_to.Active = 0;
		}

		fillTreeView_stats(false);
	}

	private void updateComboStatsSubType() {
		if (UtilGtk.ComboGetActive(combo_stats_stat_type) == Constants.TypeJumpsSimple ) 
		{
			if(UtilGtk.ComboGetActive(combo_stats_stat_subtype) == Catalog.GetString("No indexes")) {
				UtilGtk.ComboUpdate(combo_stats_stat_apply_to, 
					SqliteJumpType.SelectJumpTypes(Constants.AllJumpsName, "nonTC", true), ""); //only select name
				combo_stats_stat_apply_to.Sensitive = true;
				combo_stats_stat_apply_to.Active = 0;
			} else if (UtilGtk.ComboGetActive(combo_stats_stat_subtype) == Constants.IeIndexFormula) {
				UtilGtk.ComboUpdate(combo_stats_stat_apply_to, "CMJ, SJ");
				combo_stats_stat_apply_to.Active = 0;
				combo_stats_stat_apply_to.Sensitive = false;
			} else if (UtilGtk.ComboGetActive(combo_stats_stat_subtype) == Constants.IubIndexFormula) {
				UtilGtk.ComboUpdate(combo_stats_stat_apply_to, "ABK, CMJ");
				combo_stats_stat_apply_to.Active = 0;
				combo_stats_stat_apply_to.Sensitive = false;
			} else if (UtilGtk.ComboGetActive(combo_stats_stat_subtype) == Constants.FvIndexFormula) {
				//"F/V sj+(100%)/sj *100",	//fvIndexFormula
				UtilGtk.ComboUpdate(combo_stats_stat_apply_to, "SJl(100%), SJ");
				combo_stats_stat_apply_to.Active = 0;
				combo_stats_stat_apply_to.Sensitive = false;
			} else if (UtilGtk.ComboGetActive(combo_stats_stat_subtype) == Constants.PotencySayersSJFormula) {
				combo_stats_stat_apply_to.Active = 
					UtilGtk.ComboUpdate(combo_stats_stat_apply_to, 
							SqliteJumpType.SelectJumpTypes("", "nonTC", true), //only select name
							"SJ"); //default value
				combo_stats_stat_apply_to.Sensitive = true;
			} else {/*
				   this applies to all potency formulas (default is CMJ), except SayersSJ 
				Constants.PotencyLewisFormula,
				Constants.PotencyHarmanFormula,
				Constants.PotencySayersCMJFormula,
				Constants.PotencyShettyFormula,
				Constants.PotencyCanavanFormula,
				Constants.PotencyBahamondeFormula,
				Constants.PotencyLaraMaleApplicantsSCFormula,
				Constants.PotencyLaraFemaleEliteVoleiFormula,
				Constants.PotencyLaraFemaleMediumVoleiFormula,
				Constants.PotencyLaraFemaleSCStudentsFormula,
				Constants.PotencyLaraFemaleSedentaryFormula
				*/
				combo_stats_stat_apply_to.Active = 
					UtilGtk.ComboUpdate(combo_stats_stat_apply_to, 
							SqliteJumpType.SelectJumpTypes("", "nonTC", true), //only select name
							"CMJ"); //default value
				combo_stats_stat_apply_to.Sensitive = true;
			}
		}  else if (UtilGtk.ComboGetActive(combo_stats_stat_type) == Constants.TypeJumpsSimpleWithTC ) 
		{
			UtilGtk.ComboUpdate(combo_stats_stat_apply_to, 
				SqliteJumpType.SelectJumpTypes(Constants.AllJumpsName, "TC", true), ""); //only select name
			combo_stats_stat_apply_to.Sensitive = true;
			combo_stats_stat_apply_to.Active = 0;
		} 
		
		if (UtilGtk.ComboGetActive(combo_stats_stat_subtype) == Catalog.GetString("Evolution") )  {
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

		//show update stats button
		//ShowUpdateStatsButton();
	}

	private bool fillTreeView_stats (bool graph) 
	{
		if(blockFillingTreeview) {
			return false;
		}
		
		Log.WriteLine("----------FILLING treeview stats---------------");
	
		string statisticType = UtilGtk.ComboGetActive(combo_stats_stat_type);
		string statisticSubType = UtilGtk.ComboGetActive(combo_stats_stat_subtype);
		string statisticApplyTo = UtilGtk.ComboGetActive(combo_stats_stat_apply_to);

		if(statsColumnsToRemove && !graph) {
			statsRemoveColumns();
		}
		statsColumnsToRemove = true;

		bool toReport = false; //all graphs are done for be shown on window (not to file like report.cs)
		
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
		if (UtilGtk.ComboGetActive(combo_stats_stat_subtype) == Catalog.GetString("Evolution") &&
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
				weightStatsPercent, 
				markedRows,
				rj_evolution_mark_consecutives,
				graph,
				toReport  //always false in this class
				);

		//if we just made a graph, store is not made, 
		//and we cannot change the Male/female visualizations in the combo
		//with this we can assign a store to the graph (we assign the store of the last stat (not graph)
		//define lastStore before Choosing Stat
		if(! toReport) 
			if (graph)
				myStatType.LastStore = lastStore;
		
		
		bool allFine = myStatType.ChooseStat();
		
		
		//if we just made a graph, store is not made, 
		//and we cannot change the Male/female visualizations in the combo
		//with this we can assign a store to the graph (we assign the store of the last stat (not graph)
		//assign lastStore here
		if(! toReport)
			if(! graph)
				lastStore = myStatType.LastStore;
		

	
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
			Log.WriteLine("Do markedRows stuff later");
		}

		//show enunciate of the stat in textview_enunciate
		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.Text = myStatType.Enunciate;
		textview_enunciate.Buffer = tb;
		tb.Text = myStatType.Enunciate;
		
		//show/hide persons selector on comboCheckboxesOptions
		if(! graph) {
			if(UtilGtk.ComboGetActive(combo_stats_stat_type) != Constants.TypeSessionSummary &&
					UtilGtk.ComboGetActive(combo_stats_stat_type) != Constants.TypeJumperSummary) 
				comboCheckboxesOptions = addPersonsToComboCheckBoxesOptions();
			else
				comboCheckboxesOptions = comboCheckboxesOptionsWithoutPersons;

			UtilGtk.ComboUpdate(combo_select_checkboxes, comboCheckboxesOptions, "");
		}

		//every time a stat is created, all rows should be checked (except AVG & SD)
		//but not if we clicked graph
		if(! graph)
			combo_select_checkboxes.Active = UtilGtk.ComboMakeActive(comboCheckboxesOptions, Catalog.GetString("All"));
		

		if(allFine) {
			return true;
		} else {
			return false;
		}
	}

	//changes the combo_select_checkboxes to "Selected" if any row in the treeview is checked or unchecked
	private void on_fake_button_row_checked_clicked (object o, EventArgs args) {
		Log.WriteLine("fakeButtonRowCheckedUnchecked in gui/stats.cs !!");

		combo_select_checkboxes.Active = UtilGtk.ComboMakeActive(comboCheckboxesOptions, Catalog.GetString("Selected"));
	}
	
	private void on_fake_button_rows_selected_clicked (object o, EventArgs args) {
		Log.WriteLine("fakeButtonRowsSelected in gui/stats.cs !!");
		button_graph.Sensitive = true;
		button_add_to_report.Sensitive = true;
	}
	
	private void on_fake_button_no_rows_selected_clicked (object o, EventArgs args) {
		Log.WriteLine("fakeButtonNoRowsSelected in gui/stats.cs !!");
		button_graph.Sensitive = false;
		button_add_to_report.Sensitive = false;

		//put none in combo
		combo_select_checkboxes.Active = UtilGtk.ComboMakeActive(comboCheckboxesOptions, Catalog.GetString("None"));
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

		//after update stats it will be unsensitive until a new test is finished
		button_stats.Sensitive = false;
	}

	private void on_button_graph_clicked (object o, EventArgs args) {
		fillTreeView_stats(true);
	}

	
	/* this is disabled now, see ShowUpdateStatsButton, HideUpdateStatsButton */
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
	
	//allows to click on updateStatsButton (from chronojump.cs)
	//now checkbox of stats automatic is disabled
	//and user has to do it always by hand
	//workaround to bug ???????
	public void HideUpdateStatsButton() {
		button_stats.Sensitive = false;
	}
	public void ShowUpdateStatsButton() {
		button_stats.Sensitive = true;
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
		
		//if (statsAutomatic) { 
			fillTreeView_stats(false);
		//}
	}
	
	void on_spinbutton_mark_consecutives_changed (object o, EventArgs args) {
		//if (statsAutomatic) { 
			fillTreeView_stats(false);
		//}
	}

	
	private void update_stats_widgets_sensitiveness() {
		//blank statusbar
		statusbar_stats.Push( 1, "");

		string statisticType = UtilGtk.ComboGetActive(combo_stats_stat_type);
		string statisticSubType = UtilGtk.ComboGetActive(combo_stats_stat_subtype);
		if(statisticType == "" || statisticSubType == "") {
			//for an unknown reason, when we select an option in the combo stats, 
			//the on_combo_stats_stat_type_changed it's called two times? 
			//in the first the value of Entry.Text is "";
			return;
		} else {
			//some stats should not be showed as limited jumps
			if(statisticType == Constants.TypeJumpsReactive && 
				       ( statisticSubType == Catalog.GetString("Evolution") ||
					 statisticSubType == Constants.RJAVGSDRjIndexName ||
					 statisticSubType == Constants.RJAVGSDQIndexName)
					 ) {
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
			//in Potency formulas show only "all jumps" radiobutton
			else if(statisticType == Constants.TypeJumpsSimple && ( 
						statisticSubType == Constants.PotencyLewisFormula ||
						statisticSubType == Constants.PotencyHarmanFormula ||
						statisticSubType == Constants.PotencySayersSJFormula ||
						statisticSubType == Constants.PotencySayersCMJFormula ||
						statisticSubType == Constants.PotencyShettyFormula ||
						statisticSubType == Constants.PotencyCanavanFormula ||
						//statisticSubType == Constants.PotencyBahamondeFormula ||
						statisticSubType == Constants.PotencyLaraMaleApplicantsSCFormula ||
						statisticSubType == Constants.PotencyLaraFemaleEliteVoleiFormula ||
						statisticSubType == Constants.PotencyLaraFemaleMediumVoleiFormula ||
						statisticSubType == Constants.PotencyLaraFemaleSCStudentsFormula ||
						statisticSubType == Constants.PotencyLaraFemaleSedentaryFormula
						) ) {
				//change the radiobutton value
				if(radiobutton_stats_jumps_limit.Active || radiobutton_stats_jumps_person_average.Active ||
						radiobutton_stats_jumps_person_bests.Active) {
					radiobutton_stats_jumps_all.Active = true;
				}
				radiobutton_stats_jumps_all.Sensitive = true;
				//make no sensitive
				radiobutton_stats_jumps_limit.Sensitive = false;
				radiobutton_stats_jumps_person_bests.Sensitive = false;
				radiobutton_stats_jumps_person_average.Sensitive = false;
			}
			else if(statisticType == Constants.TypeSessionSummary || 
					statisticType == Constants.TypeJumperSummary || 
					( statisticType == Constants.TypeJumpsSimple && 
					 statisticSubType != Catalog.GetString("No indexes") ) ||
					(selectedSessions.Count > 1 && ! radiobutton_current_session.Active) )
			{
				//change the radiobutton value
				if(radiobutton_stats_jumps_all.Active || radiobutton_stats_jumps_limit.Active) {
					radiobutton_stats_jumps_person_bests.Active = true;
					spin_stats_jumps_person_bests.Sensitive = false; //in this jumps only show the '1' best value
				}
				radiobutton_stats_jumps_person_bests.Sensitive = true;
				radiobutton_stats_jumps_person_average.Sensitive = true;
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
		
		string myText = UtilGtk.ComboGetActive(combo_stats_stat_type);
		string myText2 = UtilGtk.ComboGetActive(combo_stats_stat_subtype);
		string myText3 = UtilGtk.ComboGetActive(combo_stats_stat_apply_to);
		if (myText != "" && (myText2 != "" || myText3 !="") ) {
			fillTreeView_stats(false);
		}
	}
	
	private void on_combo_stats_stat_apply_to_changed(object o, EventArgs args) {
			
		//for an unknown reason, when we select an option in the combo stats, 
		//the on_combo_stats_stat_type_changed it's called two times? 
		//in the first the value of Entry.Text is "";
		string myText = UtilGtk.ComboGetActive(combo_stats_stat_type);
		string myText2 = UtilGtk.ComboGetActive(combo_stats_stat_subtype);
		string myText3 = UtilGtk.ComboGetActive(combo_stats_stat_apply_to);
		if (myText != "" && (myText2 != "" || myText3 !="") ) {
			fillTreeView_stats(false);
		}
	}
	
	private void on_radiobuttons_stat_session_toggled (object o, EventArgs args)
	{
		if(o == (object) radiobutton_current_session) 
		{
			Log.WriteLine("current");
			button_stats_select_sessions.Sensitive = false;
		} 
		else if (o == (object) radiobutton_selected_sessions ) 
		{
			Log.WriteLine("selected");
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
		//if (statsAutomatic) { 
			fillTreeView_stats(false);
		//}
	}
	
	private void on_button_stats_select_sessions_clicked (object o, EventArgs args) {
		Log.WriteLine("select sessions for stats");
		sessionSelectStatsWin = SessionSelectStatsWindow.Show(stats_window, selectedSessions);
		sessionSelectStatsWin.Button_accept.Clicked += new EventHandler(on_stats_select_sessions_accepted);
	}
	
	private void on_stats_select_sessions_accepted (object o, EventArgs args) {
		Log.WriteLine("select sessions for stats accepted");
		
		if ((sessionSelectStatsWin.ArrayOfSelectedSessions[0]).ToString() != "-1") { 
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
		Log.WriteLine("add to report window");

		string statisticType = UtilGtk.ComboGetActive(combo_stats_stat_type);
		string statisticSubType = UtilGtk.ComboGetActive(combo_stats_stat_subtype);
		if(statisticType == "" && statisticSubType == "") {
			//for an unknown reason, when we select an option in the combo stats, 
			//the on_combo_stats_stat_type_changed it's called two times? 
			//in the first the value of Entry.Text is "";
			return;
		} else {
			if (statisticSubType == Catalog.GetString("Evolution") &&
				checkbutton_mark_consecutives.Active ) {
				statisticSubType += "." + ( spinbutton_mark_consecutives.Value ).ToString(); 
			}
			
			string statisticApplyTo = UtilGtk.ComboGetActive(combo_stats_stat_apply_to);
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
					
			
			statusbar_stats.Push( 1, Catalog.GetString("Successfully added") + " " + statisticType + "-" + statisticSubType + "-" + statisticApplyTo);
		}
		
	}

	void on_button_close_clicked (object o, EventArgs args)
	{
		StatsWindowBox.stats_window.Hide();
		StatsWindowBox = null;
	}
	
	void on_stats_window_delete_event (object o, DeleteEventArgs args)
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

