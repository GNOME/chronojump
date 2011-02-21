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
 * Copyright (C) 2004-2009   Xavier de Blas <xaviblas@gmail.com> 
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
	[Widget] Gtk.Label label_apply_to;
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
		
	[Widget] Gtk.Label label_subtraction_between;
	[Widget] Gtk.Box hbox_subtraction_between;
	[Widget] Gtk.Box hbox_subtraction_between_1;
	[Widget] Gtk.Box hbox_subtraction_between_2;
	[Widget] Gtk.ComboBox combo_subtraction_between_1;
	[Widget] Gtk.ComboBox combo_subtraction_between_2;
	
	[Widget] Gtk.Box hbox_combo_select_checkboxes;
	[Widget] Gtk.ComboBox combo_select_checkboxes;
	
	[Widget] Gtk.Image image_stats_win_graph;
	[Widget] Gtk.Image image_stats_win_report;
	[Widget] Gtk.Statusbar statusbar_stats;
	
	[Widget] Gtk.Box hbox_combo_graph_type;
	[Widget] Gtk.Label label_graph_var_x;
	[Widget] Gtk.Label label_graph_var_y;
	[Widget] Gtk.Box hbox_combo_graph_var_x;
	[Widget] Gtk.Box hbox_combo_graph_var_y;
	[Widget] Gtk.Box hbox_combo_graph_palette;
	[Widget] Gtk.ComboBox combo_graph_type;
	[Widget] Gtk.ComboBox combo_graph_var_x;
	[Widget] Gtk.ComboBox combo_graph_var_y;
	[Widget] Gtk.ComboBox combo_graph_palette;
	[Widget] Gtk.Label label_graph_options;
	[Widget] Gtk.CheckButton checkbutton_transposed;
	[Widget] Gtk.Box hbox_line;
	[Widget] Gtk.SpinButton spin_line;

	[Widget] Gtk.Box hbox_combo_graph_width;
	[Widget] Gtk.Box hbox_combo_graph_height;
	[Widget] Gtk.ComboBox combo_graph_width;
	[Widget] Gtk.ComboBox combo_graph_height;
	[Widget] Gtk.Label label_graph_legend;
	[Widget] Gtk.Box hbox_combo_graph_legend;
	[Widget] Gtk.ComboBox combo_graph_legend;
	
	[Widget] Gtk.CheckButton checkbutton_margins;
	[Widget] Gtk.Box hbox_graph_margins;
	[Widget] Gtk.SpinButton spin_graph_margin_b; //bottom
	[Widget] Gtk.SpinButton spin_graph_margin_l; //left
	[Widget] Gtk.SpinButton spin_graph_margin_t; //top
	[Widget] Gtk.SpinButton spin_graph_margin_r; //right

	int prefsDigitsNumber;
	bool heightPreferred;
	bool weightStatsPercent;
	
	//bool statsAutomatic = true;
	bool statsAutomatic = false;
	bool statsColumnsToRemove = false;
	private Session currentSession;
	//selected sessions
	ArrayList selectedSessions;
	
	//private Stat myStat; 
	private StatType myStatType;
	
	Gtk.TreeStore lastStore; 
	
	//optimization
	private bool blockFillingTreeview;

	private static string [] comboStatsTypeOptions = {
//		Constants.TypeSessionSummary, //deactivated until R is fully implemented
//		Constants.TypeJumperSummary,
		Constants.TypeJumpsSimple,
		Constants.TypeJumpsSimpleWithTC,
		Constants.TypeJumpsReactive,
		Constants.TypeRunsSimple,
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
		Catalog.GetString(Constants.SubtractionBetweenTests),
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
	
	private static string [] comboGraphSizeOptions = {
		"300", "400", "500", "600", "700", "800", "1000", "1100", "1200"
	};
	
	private static string [] comboGraphLegendOptions = {
		"bottomright",
		"bottom",
		"bottomleft",
		"left",
		"topleft",
		"top",
		"topright",
		"right",
		"center"
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
		
		UtilGtk.ResizeIfNeeded(stats_window);

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
		createComboStatsSubtractionBetweenTests();
	
		createComboGraphType();
		
		createComboGraphVars();
		showGraphXYStuff(false);

		createComboGraphPalette();
		createComboGraphSize();
		createComboGraphLegend();
	
		// here doesn't do Ok the job, done later in Initialize
		//blockFillingTreeview = false;

		updateComboStats();
			
		
		//textview_enunciate.Hide();
		//scrolledwindow_enunciate.Hide();
			
		spinbutton_mark_consecutives.Sensitive = false;
		hbox_mark_consecutives.Hide();
		hbox_graph_margins.Hide();
		
		//first graph type is boxplot, and it doesn't show transpose also colors are grey...
		on_combo_graph_type_changed(new object(), new EventArgs());
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
		//StatsWindowBox.button_stats.Sensitive = false;
		StatsWindowBox.button_stats.Visible = false;

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
	
	private void createComboStatsSubtractionBetweenTests() {
		combo_subtraction_between_1 = ComboBox.NewText ();
		combo_subtraction_between_1.Changed += new EventHandler (on_combo_subtraction_between_1_changed);
		hbox_subtraction_between_1.PackStart(combo_subtraction_between_1, true, true, 0);
		hbox_subtraction_between_1.ShowAll();
		combo_subtraction_between_1.Sensitive = true;
		
		combo_subtraction_between_2 = ComboBox.NewText ();
		combo_subtraction_between_2.Changed += new EventHandler (on_combo_subtraction_between_2_changed);
		hbox_subtraction_between_2.PackStart(combo_subtraction_between_2, true, true, 0);
		hbox_subtraction_between_2.ShowAll();
		combo_subtraction_between_2.Sensitive = true;
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
	
	private void createComboGraphType() {
		combo_graph_type = ComboBox.NewText ();
		UtilGtk.ComboUpdate(combo_graph_type, Constants.GraphTypes, "");
		combo_graph_type.Active=0;
		
		combo_graph_type.Changed += new EventHandler (on_combo_graph_type_changed);
		
		hbox_combo_graph_type.PackStart(combo_graph_type, true, true, 0);
		hbox_combo_graph_type.ShowAll();
		combo_graph_type.Sensitive = true;
	}

	private void createComboGraphVars() {
		combo_graph_var_x = ComboBox.NewText ();
		UtilGtk.ComboUpdate(combo_graph_var_x, UtilGtk.GetCols(treeview_stats, 2), ""); //2 for not getting the checkbox and the text column
		combo_graph_var_x.Active=0;
		hbox_combo_graph_var_x.PackStart(combo_graph_var_x, true, true, 0);
		hbox_combo_graph_var_x.ShowAll();
		combo_graph_var_x.Sensitive = true;
		
		combo_graph_var_y = ComboBox.NewText ();
		UtilGtk.ComboUpdate(combo_graph_var_y, UtilGtk.GetCols(treeview_stats, 2), "");
		combo_graph_var_y.Active=0;
		hbox_combo_graph_var_y.PackStart(combo_graph_var_y, true, true, 0);
		hbox_combo_graph_var_y.ShowAll();
		combo_graph_var_y.Sensitive = true;
	}
	
	private void createComboGraphPalette() {
		combo_graph_palette = ComboBox.NewText ();
		UtilGtk.ComboUpdate(combo_graph_palette, Constants.GraphPalettes, "");
		combo_graph_palette.Active=0;
		
		hbox_combo_graph_palette.PackStart(combo_graph_palette, true, true, 0);
		hbox_combo_graph_palette.ShowAll();
		combo_graph_palette.Sensitive = true;
	}

	private void createComboGraphSize() {
		combo_graph_width = ComboBox.NewText ();
		UtilGtk.ComboUpdate(combo_graph_width, comboGraphSizeOptions, "");
		combo_graph_width.Active=2; //500
		
		hbox_combo_graph_width.PackStart(combo_graph_width, true, true, 0);
		hbox_combo_graph_width.ShowAll();
		combo_graph_width.Sensitive = true;
		
		combo_graph_height = ComboBox.NewText ();
		UtilGtk.ComboUpdate(combo_graph_height, comboGraphSizeOptions, "");
		combo_graph_height.Active=2; //500
		
		hbox_combo_graph_height.PackStart(combo_graph_height, true, true, 0);
		hbox_combo_graph_height.ShowAll();
		combo_graph_height.Sensitive = true;
	}
	
	private void createComboGraphLegend() {
		combo_graph_legend = ComboBox.NewText ();
		UtilGtk.ComboUpdate(combo_graph_legend, comboGraphLegendOptions, "");
		combo_graph_legend.Active=0;
		
		hbox_combo_graph_legend.PackStart(combo_graph_legend, true, true, 0);
		hbox_combo_graph_legend.ShowAll();
		combo_graph_legend.Sensitive = true;
	}
	
	private void showTransposed(bool show) {
		checkbutton_transposed.Visible = show;
	}
	
	private void showLineWidth(bool show) {
		hbox_line.Visible = show;
	}

	private void showGraphXYStuff(bool show) {
		label_graph_var_x.Visible = show;
		label_graph_var_y.Visible = show;
		hbox_combo_graph_var_x.Visible = show;
		hbox_combo_graph_var_y.Visible = show;
	}

	private void on_combo_graph_type_changed(object o, EventArgs args) {
		if(UtilGtk.ComboGetActive(combo_graph_type) == Constants.GraphTypeXY)
			showGraphXYStuff(true);
		else 
			showGraphXYStuff(false);
		
		showLineWidth(true);

		if(
				UtilGtk.ComboGetActive(combo_graph_type) == Constants.GraphTypeDotchart ||
				UtilGtk.ComboGetActive(combo_graph_type) == Constants.GraphTypeBoxplot ||
				UtilGtk.ComboGetActive(combo_graph_type) == Constants.GraphTypeHistogram ||
				UtilGtk.ComboGetActive(combo_graph_type) == Constants.GraphTypeStripchart ||
				UtilGtk.ComboGetActive(combo_graph_type) == Constants.GraphTypeXY)
			showTransposed(false);
		else
			showTransposed(true);

		//boxplot and stripchart are black&white and have no legend
		if(
				UtilGtk.ComboGetActive(combo_graph_type) == Constants.GraphTypeBoxplot ||
				UtilGtk.ComboGetActive(combo_graph_type) == Constants.GraphTypeStripchart ||
				UtilGtk.ComboGetActive(combo_graph_type) == Constants.GraphTypeHistogram ||
				UtilGtk.ComboGetActive(combo_graph_type) == Constants.GraphTypeDotchart) {
			combo_graph_palette.Active = UtilGtk.ComboMakeActive(Constants.GraphPalettes, Constants.GraphPaletteBlack);
			combo_graph_palette.Sensitive = false;
			label_graph_legend.Visible = false;
			combo_graph_legend.Visible = false;
		}
		else {
			combo_graph_palette.Sensitive = true;
			label_graph_legend.Visible = true;
			combo_graph_legend.Visible = true;
		}
	}
	
	private void on_checkbutton_margins_clicked(object o, EventArgs args) {
		if(checkbutton_margins.Active)
			hbox_graph_margins.Visible = true;
		else
			hbox_graph_margins.Visible = false;
	}
	
	private void on_button_graph_margin_default_clicked(object o, EventArgs args) {
		spin_graph_margin_b.Value = 5;
		spin_graph_margin_l.Value = 4;
		spin_graph_margin_t.Value = 4;
		spin_graph_margin_r.Value = 2;
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
			
			ArrayList persons = SqlitePersonSession.SelectCurrentSessionPersons(currentSession.UniqueID);
			string [] personsStrings = new String[persons.Count];
			int i=0;
			foreach (Person person in persons) 
				personsStrings[i++] = person.IDAndName(":");

			UtilGtk.ComboUpdate(combo_stats_stat_apply_to, personsStrings, "");
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
		else if (UtilGtk.ComboGetActive(combo_stats_stat_type) == Constants.TypeRunsSimple ) 
		{
			UtilGtk.ComboUpdate(combo_stats_stat_subtype, comboStatsSubTypeSimpleOptions, "");
			combo_stats_stat_subtype.Sensitive = false;
			combo_stats_stat_subtype.Active = 0;
			
			UtilGtk.ComboUpdate(combo_stats_stat_apply_to, 
				SqliteRunType.SelectRunTypes(Constants.AllRunsName, true), ""); //only select name
			combo_stats_stat_apply_to.Sensitive = true;
			combo_stats_stat_apply_to.Active = 0;
		} 

		fillTreeView_stats(false);
	}

	private void updateComboStatsSubType() {
		subtraction_between_tests_show(false);
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
			} else if (UtilGtk.ComboGetActive(combo_stats_stat_subtype) == Catalog.GetString(Constants.SubtractionBetweenTests)) {
				UtilGtk.ComboUpdate(combo_subtraction_between_1, 
					SqliteJumpType.SelectJumpTypes("", "", true), ""); //only select name
				UtilGtk.ComboUpdate(combo_subtraction_between_2, 
					SqliteJumpType.SelectJumpTypes("", "", true), ""); //only select name
				subtraction_between_tests_show(true);
				combo_stats_stat_apply_to.Active = 0;
				combo_stats_stat_apply_to.Sensitive = true;
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
		
		/*
		   if is RjEvolution, show mark consecutives, graph only with lines and transposed
		   */
		if (UtilGtk.ComboGetActive(combo_stats_stat_subtype) == Catalog.GetString("Evolution") )  {
			hbox_mark_consecutives.Show();
			checkbutton_transposed.Active = true;
			checkbutton_transposed.Sensitive = false;
			UtilGtk.ComboUpdate(combo_graph_type, Util.StringToStringArray(Constants.GraphTypeLines), "");
			combo_graph_type.Active=0;
		} else {
			hbox_mark_consecutives.Hide();
			checkbutton_transposed.Active = false;
			checkbutton_transposed.Sensitive = true;
			UtilGtk.ComboUpdate(combo_graph_type, Constants.GraphTypes, "");
			combo_graph_type.Active=0;
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

	//creates a GraphROptions object	
	public GraphROptions fillGraphROptions() {
		//Dotchart plots col 2
		string varx = UtilGtk.ComboGetActive(combo_graph_var_x);
		if(UtilGtk.ComboGetActive(combo_graph_type) == Constants.GraphTypeDotchart)
			varx = UtilGtk.GetCol(treeview_stats, 2);

		return new GraphROptions(
				UtilGtk.ComboGetActive(combo_graph_type),
				varx,
				UtilGtk.ComboGetActive(combo_graph_var_y),
				UtilGtk.ComboGetActive(combo_graph_palette),
				checkbutton_transposed.Active,
				Convert.ToInt32(spin_line.Value), 
				Convert.ToInt32(UtilGtk.ComboGetActive(combo_graph_width)),
				Convert.ToInt32(UtilGtk.ComboGetActive(combo_graph_height)),
				UtilGtk.ComboGetActive(combo_graph_legend),
				Convert.ToInt32(spin_graph_margin_b.Value), 
				Convert.ToInt32(spin_graph_margin_l.Value), 
				Convert.ToInt32(spin_graph_margin_t.Value), 
				Convert.ToInt32(spin_graph_margin_r.Value) 
				);
	}

	private bool fillTreeView_stats (bool graph) 
	{
		if(blockFillingTreeview)
			return false;
		
		Log.WriteLine("----------FILLING treeview stats---------------");
	
		string statisticType = UtilGtk.ComboGetActive(combo_stats_stat_type);
		string statisticSubType = UtilGtk.ComboGetActive(combo_stats_stat_subtype);
		string statisticApplyTo = UtilGtk.ComboGetActive(combo_stats_stat_apply_to);
			
		if (UtilGtk.ComboGetActive(combo_stats_stat_subtype) == 
				Catalog.GetString(Constants.SubtractionBetweenTests))
			statisticApplyTo = UtilGtk.ComboGetActive(combo_subtraction_between_1) + 
				":" + UtilGtk.ComboGetActive(combo_subtraction_between_2);

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

		GraphROptions graphROptions = fillGraphROptions();
		
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
				graphROptions,
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
			UtilGtk.ComboUpdate(combo_graph_var_x, UtilGtk.GetCols(treeview_stats, 2), "");
			UtilGtk.ComboUpdate(combo_graph_var_y, UtilGtk.GetCols(treeview_stats, 2), "");
		}

		//every time a stat is created, all rows should be checked (except AVG & SD)
		//but not if we clicked graph
		if(! graph) {
			combo_select_checkboxes.Active = UtilGtk.ComboMakeActive(comboCheckboxesOptions, Catalog.GetString("All"));
			try { 
				combo_graph_var_x.Active=0;
				combo_graph_var_y.Active=0;
			} catch {} //maybe there's no data
		}
		

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
		//button_stats.Sensitive = false;
		button_stats.Visible = false;
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
		//button_stats.Sensitive = false;
		button_stats.Visible = false;
	}
	public void ShowUpdateStatsButton() {
		//button_stats.Sensitive = true;
		button_stats.Visible = true;
	}
				
	private void subtraction_between_tests_show(bool show) {
		if(show) {
			label_subtraction_between.Show();
			hbox_subtraction_between.Show();
			hbox_subtraction_between_1.Show();
			hbox_subtraction_between_2.Show();

			//subtraction doesn't uses the combo: apply to
			label_apply_to.Hide();
			hbox_combo_stats_stat_apply_to.Hide();
		} else {
			label_subtraction_between.Hide();
			hbox_subtraction_between.Hide();
			hbox_subtraction_between_1.Hide();
			hbox_subtraction_between_2.Hide();
	
			label_apply_to.Show();
			hbox_combo_stats_stat_apply_to.Show();
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
		if(blockFillingTreeview)
			return;

		//blank statusbar
		statusbar_stats.Push( 1, "");

		string statisticType = UtilGtk.ComboGetActive(combo_stats_stat_type);
		string statisticSubType = UtilGtk.ComboGetActive(combo_stats_stat_subtype);
			
		//for an unknown reason, when we select an option in the combo stats, 
		//the on_combo_stats_stat_type_changed it's called two times? 
		//in the first the value of Entry.Text is "";
		if(statisticType == "" || statisticSubType == "") 
			return;
			
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
	
	private void on_combo_subtraction_between_1_changed(object o, EventArgs args) {
		Log.WriteLine(UtilGtk.ComboGetActive(combo_subtraction_between_1));
		string myText = UtilGtk.ComboGetActive(combo_subtraction_between_1);
		string myText2 = UtilGtk.ComboGetActive(combo_subtraction_between_2);
		if (myText != "" && (myText2 != "") ) 
			fillTreeView_stats(false);
	}
	private void on_combo_subtraction_between_2_changed(object o, EventArgs args) {
		Log.WriteLine(UtilGtk.ComboGetActive(combo_subtraction_between_2));
		string myText = UtilGtk.ComboGetActive(combo_subtraction_between_1);
		string myText2 = UtilGtk.ComboGetActive(combo_subtraction_between_2);
		if (myText != "" && (myText2 != "") ) 
			fillTreeView_stats(false);
	}
	
	private void on_radiobuttons_stat_session_toggled (object o, EventArgs args)
	{
		if(o == (object) radiobutton_current_session) 
		{
			Log.WriteLine("current");
			button_stats_select_sessions.Sensitive = false;

			//single session can have all graph types
			combo_graph_type.Active = UtilGtk.ComboUpdate(
					combo_graph_type, Constants.GraphTypes, UtilGtk.ComboGetActive(combo_graph_type));
		} 
		else if (o == (object) radiobutton_selected_sessions ) 
		{
			Log.WriteLine("selected");
			button_stats_select_sessions.Sensitive = true;
			
			//multi session cannot have XY and dotchart
			string [] types = Util.DeleteString(Constants.GraphTypes, Constants.GraphTypeXY);
			types = Util.DeleteString(types, Constants.GraphTypeDotchart);
			combo_graph_type.Active = UtilGtk.ComboUpdate(
					combo_graph_type, types, UtilGtk.ComboGetActive(combo_graph_type));
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
					sendSelectedSessions, statsShowJumps, showSex.ToString(), 
					myStatType.MarkedRows, fillGraphROptions());
					
			
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

