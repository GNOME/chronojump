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
 * Copyright (C) 2004-2023   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Gdk;
//using Glade;
using GLib; //for Value
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using Mono.Unix;


public partial class ChronoJumpWindow
{
	// at glade ---->
	Gtk.Notebook notebook_stats_sup;
	Gtk.Notebook notebook_stats;
	Gtk.Box vbox_stats;
	Gtk.TreeView treeview_stats;
	Gtk.Box vbox_stats_type;
	Gtk.Box hbox_combo_stats_stat_type;
	Gtk.Box hbox_combo_stats_stat_subtype;
	Gtk.Box hbox_combo_stats_stat_apply_to;
	Gtk.Box hbox_mark_and_enunciate;
	Gtk.Frame frame_graph_and_report;
	Gtk.Label label_apply_to;
	Gtk.CheckButton checkbutton_stats_sex;
	Gtk.Button button_stats;
	
	Gtk.RadioButton radiobutton_current_session;
	Gtk.RadioButton radiobutton_selected_sessions;
	Gtk.Button button_stats_select_sessions;
	Gtk.RadioButton radiobutton_stats_jumps_all;
	Gtk.RadioButton radiobutton_stats_jumps_limit;
	Gtk.SpinButton spin_stats_jumps_limit;
	Gtk.RadioButton radiobutton_stats_jumps_person_bests;
	Gtk.SpinButton spin_stats_jumps_person_bests;
	Gtk.RadioButton radiobutton_stats_jumps_person_average;
	Gtk.Button button_graph;
	Gtk.Button button_add_to_report;
	
	Gtk.Label label_enunciate;
	Gtk.ScrolledWindow scrolledwindow_enunciate;
	Gtk.CheckButton checkbutton_show_enunciate;
	
	Gtk.Notebook notebook_stats_win_options;
	Gtk.CheckButton checkbutton_mark_consecutives;
	Gtk.SpinButton spinbutton_mark_consecutives;
	Gtk.Box hbox_subtraction_between_1;
	Gtk.Box hbox_subtraction_between_2;
	
	Gtk.Box hbox_combo_select_checkboxes;
	
	Gtk.Image image_stats_win_graph;
	Gtk.Image image_stats_win_graph1;
	Gtk.Image image_stats_win_graph3;
	Gtk.Image image_stats_win_report_open;
		
	Gtk.Box hbox_graph_options;
	
	Gtk.Box hbox_combo_graph_type;
	Gtk.Box hbox_stats_variables;
	Gtk.Box hbox_combo_graph_var_x;
	Gtk.Box hbox_combo_graph_var_y;
	Gtk.Box hbox_combo_graph_palette;
	Gtk.CheckButton checkbutton_transposed;
	Gtk.Label label_line;
	Gtk.SpinButton spin_line;

	Gtk.Box hbox_combo_graph_width;
	Gtk.Box hbox_combo_graph_height;
	Gtk.Label label_graph_legend;
	Gtk.Box hbox_combo_graph_legend;
	
	Gtk.SpinButton spin_graph_margin_b; //bottom
	Gtk.SpinButton spin_graph_margin_l; //left
	Gtk.SpinButton spin_graph_margin_t; //top
	Gtk.SpinButton spin_graph_margin_r; //right
	Gtk.SpinButton spin_graph_x_cex_axis; //font size of x axis
	Gtk.Label label_stats_x_axis;
	// <---- at glade

	Gtk.ComboBoxText combo_stats_stat_type;
	Gtk.ComboBoxText combo_stats_stat_subtype;
	Gtk.ComboBoxText combo_stats_stat_apply_to;
	Gtk.ComboBoxText combo_subtraction_between_1;
	Gtk.ComboBoxText combo_subtraction_between_2;
	Gtk.ComboBoxText combo_select_checkboxes;
	Gtk.ComboBoxText combo_graph_type;
	Gtk.ComboBoxText combo_graph_var_x;
	Gtk.ComboBoxText combo_graph_var_y;
	Gtk.ComboBoxText combo_graph_palette;
	Gtk.ComboBoxText combo_graph_width;
	Gtk.ComboBoxText combo_graph_height;
	Gtk.ComboBoxText combo_graph_legend;

	SessionSelectStatsWindow sessionSelectStatsWin;

	bool statsColumnsToRemove = false;
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
		Constants.TypeJumpsSimpleStr(),
		Constants.TypeJumpsSimpleWithTCStr(),
		Constants.TypeJumpsReactiveStr(),
		Constants.TypeRunsSimpleStr(),
		Constants.TypeRunsIntervallicStr(),
	};
	
	private void stats_win_change_test_type(int testPage) {
		//no statistics for reactionTime, pulse and multichronopic
		//show a label with this info
		if(testPage >= 4) 
			notebook_stats_sup.CurrentPage = 1;
		else {
			notebook_stats_sup.CurrentPage = 0;

			combo_stats_stat_type.Changed -= new EventHandler (on_combo_stats_stat_type_changed);

			bool sensitive = false;
			bool showType = false;
			if(testPage == 0) {
				//on jumps show jumpsSimple and JumpsSimpleWithTC
				string [] str = new string [2];
				str[0] = comboStatsTypeOptions[0];
				str[1] = comboStatsTypeOptions[1];
				sensitive = true;
				showType = true;
				UtilGtk.ComboUpdate(combo_stats_stat_type, str, "");
			}
			else if(testPage == 1)
				UtilGtk.ComboUpdate(combo_stats_stat_type, comboStatsTypeOptions[2]);
			else if(testPage == 2)
				UtilGtk.ComboUpdate(combo_stats_stat_type, comboStatsTypeOptions[3]);
			else if(testPage == 3)
				UtilGtk.ComboUpdate(combo_stats_stat_type, comboStatsTypeOptions[4]);
			
			combo_stats_stat_type.Changed += new EventHandler (on_combo_stats_stat_type_changed);

			combo_stats_stat_type.Active = 0;
			combo_stats_stat_type.Sensitive = sensitive;
			vbox_stats_type.Visible = showType;
		}
	}
	
	private static string [] comboStatsSubTypeWithTCOptions = {
		Constants.DjIndexFormula,
		Constants.QIndexFormula,
		Constants.DjPowerFormula
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
		//Constants.ChronojumpProfileStr(),
		Constants.FvIndexFormula,
		Constants.IeIndexFormula, 
		Constants.ArmsUseIndexFormula,
		Constants.IRnaIndexFormula,
		Constants.IRaIndexFormula,
		Catalog.GetString(Constants.SubtractionBetweenTests),
		Constants.PotencyLewisFormulaShortStr(),
		Constants.PotencyHarmanFormulaShortStr(),
		Constants.PotencySayersSJFormulaShortStr(),
		Constants.PotencySayersCMJFormulaShortStr(),
		Constants.PotencyShettyFormulaShortStr(),
		Constants.PotencyCanavanFormulaShortStr(),
		//Constants.PotencyBahamondeFormulaShort,
		Constants.PotencyLaraMaleApplicantsSCFormulaShortStr(),
		Constants.PotencyLaraFemaleEliteVoleiFormulaShortStr(),
		Constants.PotencyLaraFemaleMediumVoleiFormulaShortStr(),
		Constants.PotencyLaraFemaleSCStudentsFormulaShortStr(),
		Constants.PotencyLaraFemaleSedentaryFormulaShortStr()
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
	
	private void stats_win_create() {
		//myStat = new Stat(); //create and instance of myStat
		myStatType = new StatType();

		//this doesn't allow treeview to be recreated many times (4)
		//in all the combos that are going to be created
		blockFillingTreeview = true;
		
		stats_win_putNonStandardIcons();	

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
			
		notebook_stats_win_options.Hide();	
		checkbutton_mark_consecutives.Active = false;
		spinbutton_mark_consecutives.Sensitive = false;
		
		//first graph type is boxplot, and it doesn't show transpose also colors are grey...
		on_combo_graph_type_changed(new object(), new EventArgs());
		
		//button update stats is unsensitive until a test finished
		button_stats.Visible = true;
		button_stats.Sensitive = false;
	}
	
	private void stats_win_hide()
	{
		//StatsWindowBox.stats_window.Hide ();
		vbox_stats.Hide();
	}
	
	private void stats_win_putNonStandardIcons() {
		Pixbuf pixbuf;
		//pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "gpm-statistics.png");
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_analyze.png");
		image_stats_win_graph.Pixbuf = pixbuf;
		image_stats_win_graph1.Pixbuf = pixbuf;
		image_stats_win_graph3.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "report_view.png");
		image_stats_win_report_open.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "spreadsheet.png");
	}

	
	private void stats_win_initializeSession() 
	{
		//currentSession = newCurrentSession;

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
		combo_stats_stat_type = new ComboBoxText ();
				
		string [] str = new string [2];
		str[0] = comboStatsTypeOptions[0];
		str[1] = comboStatsTypeOptions[1];
		UtilGtk.ComboUpdate(combo_stats_stat_type, str, "");
		combo_stats_stat_type.Active = 0;
		
		combo_stats_stat_type.Changed += new EventHandler (on_combo_stats_stat_type_changed);

		hbox_combo_stats_stat_type.PackStart(combo_stats_stat_type, true, true, 0);
		hbox_combo_stats_stat_type.ShowAll();
		combo_stats_stat_type.Sensitive = true;
	}
	
	private void createComboStatsSubType() {
		combo_stats_stat_subtype = new ComboBoxText ();
		
		combo_stats_stat_subtype.Changed += new EventHandler (on_combo_stats_stat_subtype_changed);

		hbox_combo_stats_stat_subtype.PackStart(combo_stats_stat_subtype, true, true, 0);
		hbox_combo_stats_stat_subtype.ShowAll();
		combo_stats_stat_subtype.Sensitive = true;
	}

	private void createComboStatsApplyTo() {
		combo_stats_stat_apply_to = new ComboBoxText ();
		
		combo_stats_stat_apply_to.Changed += new EventHandler (on_combo_stats_stat_apply_to_changed);

		hbox_combo_stats_stat_apply_to.PackStart(combo_stats_stat_apply_to, true, true, 0);
		hbox_combo_stats_stat_apply_to.ShowAll();
		combo_stats_stat_apply_to.Sensitive = true;
	}
	
	private void createComboStatsSubtractionBetweenTests() {
		combo_subtraction_between_1 = new ComboBoxText ();
		combo_subtraction_between_1.Changed += new EventHandler (on_combo_subtraction_between_1_or_2_changed);
		hbox_subtraction_between_1.PackStart(combo_subtraction_between_1, true, true, 0);
		hbox_subtraction_between_1.ShowAll();
		combo_subtraction_between_1.Sensitive = true;
		
		combo_subtraction_between_2 = new ComboBoxText ();
		combo_subtraction_between_2.Changed += new EventHandler (on_combo_subtraction_between_1_or_2_changed);
		hbox_subtraction_between_2.PackStart(combo_subtraction_between_2, true, true, 0);
		hbox_subtraction_between_2.ShowAll();
		combo_subtraction_between_2.Sensitive = true;
	}

	private void createComboSelectCheckboxes() {
		combo_select_checkboxes = new ComboBoxText ();
		UtilGtk.ComboUpdate(combo_select_checkboxes, comboCheckboxesOptions, "");
		
		//combo_select_checkboxes.DisableActivate ();
		combo_select_checkboxes.Changed += new EventHandler (on_combo_select_checkboxes_changed);

		hbox_combo_select_checkboxes.PackStart(combo_select_checkboxes, false, false, 0);
		hbox_combo_select_checkboxes.ShowAll();
		combo_select_checkboxes.Sensitive = true;
	}
	
	private void createComboGraphType() {
		combo_graph_type = new ComboBoxText ();
		UtilGtk.ComboUpdate(combo_graph_type, Constants.GraphTypes, "");
		combo_graph_type.Active=0;
		
		combo_graph_type.Changed += new EventHandler (on_combo_graph_type_changed);
		
		hbox_combo_graph_type.PackStart(combo_graph_type, true, true, 0);
		hbox_combo_graph_type.ShowAll();
		combo_graph_type.Sensitive = true;
	}

	private void createComboGraphVars() {
		combo_graph_var_x = new ComboBoxText ();
		UtilGtk.ComboUpdate(combo_graph_var_x, UtilGtk.GetCols(treeview_stats, 2), ""); //2 for not getting the checkbox and the text column
		combo_graph_var_x.Active=0;
		hbox_combo_graph_var_x.PackStart(combo_graph_var_x, true, true, 0);
		hbox_combo_graph_var_x.ShowAll();
		combo_graph_var_x.Sensitive = true;
		
		combo_graph_var_y = new ComboBoxText ();
		UtilGtk.ComboUpdate(combo_graph_var_y, UtilGtk.GetCols(treeview_stats, 2), "");
		combo_graph_var_y.Active=0;
		hbox_combo_graph_var_y.PackStart(combo_graph_var_y, true, true, 0);
		hbox_combo_graph_var_y.ShowAll();
		combo_graph_var_y.Sensitive = true;
	}
	
	private void createComboGraphPalette() {
		combo_graph_palette = new ComboBoxText ();
		UtilGtk.ComboUpdate(combo_graph_palette, Constants.GraphPalettes, "");
		combo_graph_palette.Active=0;
		
		hbox_combo_graph_palette.PackStart(combo_graph_palette, true, true, 0);
		hbox_combo_graph_palette.ShowAll();
		combo_graph_palette.Sensitive = true;
	}

	private void createComboGraphSize() {
		combo_graph_width = new ComboBoxText ();
		UtilGtk.ComboUpdate(combo_graph_width, comboGraphSizeOptions, "");
		combo_graph_width.Active=2; //500
		
		hbox_combo_graph_width.PackStart(combo_graph_width, true, true, 0);
		hbox_combo_graph_width.ShowAll();
		combo_graph_width.Sensitive = true;
		
		combo_graph_height = new ComboBoxText ();
		UtilGtk.ComboUpdate(combo_graph_height, comboGraphSizeOptions, "");
		combo_graph_height.Active=2; //500
		
		hbox_combo_graph_height.PackStart(combo_graph_height, true, true, 0);
		hbox_combo_graph_height.ShowAll();
		combo_graph_height.Sensitive = true;
	}
	
	private void createComboGraphLegend() {
		combo_graph_legend = new ComboBoxText ();
		UtilGtk.ComboUpdate(combo_graph_legend, comboGraphLegendOptions, "");
		combo_graph_legend.Active=0;
		
		hbox_combo_graph_legend.PackStart(combo_graph_legend, true, true, 0);
		hbox_combo_graph_legend.ShowAll();
		combo_graph_legend.Sensitive = true;
	}
	
	private void showTransposed(bool show) {
		checkbutton_transposed.Visible = show;
		checkbutton_transposed.Active = true;
	}
	
	private void showLineWidth(bool show) {
		label_line.Visible = show;
		spin_line.Visible = show;
	}

	private void showGraphXYStuff(bool show) {
		hbox_stats_variables.Visible = show;
	}
			
	private void showXAxisOptions(bool show) {
		label_stats_x_axis.Visible = show;
		spin_graph_x_cex_axis.Visible = show;
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
			combo_graph_palette.Active = UtilGtk.ComboMakeActive(Constants.GraphPalettes, Constants.GraphPaletteBlackStr());
			combo_graph_palette.Sensitive = false;
			label_graph_legend.Visible = false;
			combo_graph_legend.Visible = false;
		}
		else {
			combo_graph_palette.Sensitive = true;
			label_graph_legend.Visible = true;
			combo_graph_legend.Visible = true;
		}
		
		showXAxisOptions(false);

		//barplot and lines can have font size on X axis
		if(UtilGtk.ComboGetActive(combo_graph_type) == Constants.GraphTypeBarplot ||
				UtilGtk.ComboGetActive(combo_graph_type) == Constants.GraphTypeLines)
			showXAxisOptions(true);
	}
	
	private void on_button_graph_margin_default_clicked(object o, EventArgs args) {
		spin_graph_margin_b.Value = 5;
		spin_graph_margin_l.Value = 4;
		spin_graph_margin_t.Value = 4;
		spin_graph_margin_r.Value = 2;
		spin_graph_x_cex_axis.Value = 0.8;
	}

	private void on_combo_select_checkboxes_changed(object o, EventArgs args)
	{
		string myText = UtilGtk.ComboGetActive(combo_select_checkboxes);
			
		if (myText != "" & myText != Catalog.GetString("Selected")) {
			try {
				//if selected 'male' or 'female', showSex and redo the treeview if needed
				if (myText == Catalog.GetString("Male") ||
						myText == Catalog.GetString("Female")) {
					if( ! checkbutton_stats_sex.Active) {
						//this will redo the treeview
						checkbutton_stats_sex.Active = true;
						fillTreeView_stats(false);
						//put another time the value Male or Female in combo_select_checkboxes
						combo_select_checkboxes.Active = UtilGtk.ComboMakeActive(comboCheckboxesOptions, myText);
					}
				}
				
				myStatType.MarkSelected(myText);
				myStatType.CreateOrUpdateAVGAndSD();
			} catch {
				LogB.Warning("Do later!!");
			}
		}
	}
	
	private string [] addPersonsToComboCheckBoxesOptions() {
		return Util.AddArrayString(comboCheckboxesOptionsWithoutPersons, Util.ArrayListToString(myStatType.PersonsWithData));
	}

	private void showUpdateStatsAndHideData(bool show)
	{
		//as this is called by cancel and finish, check that we are on first thread to not have GTK problems
		if(UtilGtk.CanTouchGTK())
		{
			button_stats.Sensitive = show;
			treeview_stats.Sensitive = ! show;
			hbox_mark_and_enunciate.Sensitive = ! show;
			frame_graph_and_report.Sensitive = ! show;
		}
	}

	private void updateComboStats() {
		string [] nullOptions = { "-" };
		
		if(UtilGtk.ComboGetActive(combo_stats_stat_type) == Constants.TypeSessionSummaryStr() )
		{
			UtilGtk.ComboUpdate(combo_stats_stat_subtype, nullOptions, "");
			combo_stats_stat_subtype.Sensitive = false;
			
			UtilGtk.ComboUpdate(combo_stats_stat_apply_to, nullOptions, "");
			combo_stats_stat_apply_to.Sensitive = false;
		}
		else if(UtilGtk.ComboGetActive(combo_stats_stat_type) == Constants.TypeJumperSummaryStr() )
		{
			UtilGtk.ComboUpdate(combo_stats_stat_subtype, nullOptions, "");
			combo_stats_stat_subtype.Sensitive = false;
			
			ArrayList persons = SqlitePersonSession.SelectCurrentSessionPersons(
					currentSession.UniqueID,
					false); //means: do not returnPersonAndPSlist

			string [] personsStrings = new String[persons.Count];
			int i=0;
			foreach (Person person in persons) 
				personsStrings[i++] = person.IDAndName(":");

			UtilGtk.ComboUpdate(combo_stats_stat_apply_to, personsStrings, "");
			combo_stats_stat_apply_to.Sensitive = true;
			combo_stats_stat_apply_to.Active = 0;
		} 
		else if (UtilGtk.ComboGetActive(combo_stats_stat_type) == Constants.TypeJumpsSimpleStr() )
		{
			combo_stats_stat_subtype.Changed -= new EventHandler (on_combo_stats_stat_subtype_changed);
			UtilGtk.ComboUpdate(combo_stats_stat_subtype, comboStatsSubTypeSimpleOptions, "");
			combo_stats_stat_subtype.Changed += new EventHandler (on_combo_stats_stat_subtype_changed);

			combo_stats_stat_subtype.Sensitive = true;
			combo_stats_stat_subtype.Active = 0;
			
			//by default show all simple nonTC jumps, but if combo_stats_subtype changed
			//updateComboStatsSubType() will do the work
			UtilGtk.ComboUpdate(combo_stats_stat_apply_to,
					SqliteJumpType.SelectJumpTypes(false, Constants.AllJumpsNameStr(), "nonTC", true), ""); //only select name

			combo_stats_stat_apply_to.Sensitive = true;
			combo_stats_stat_apply_to.Active = 0;
		} 
		else if (UtilGtk.ComboGetActive(combo_stats_stat_type) == Constants.TypeJumpsSimpleWithTCStr() )
		{
			combo_stats_stat_subtype.Changed -= new EventHandler (on_combo_stats_stat_subtype_changed);
			UtilGtk.ComboUpdate(combo_stats_stat_subtype, comboStatsSubTypeWithTCOptions, "");
			combo_stats_stat_subtype.Changed += new EventHandler (on_combo_stats_stat_subtype_changed);

			combo_stats_stat_subtype.Sensitive = true;
			combo_stats_stat_subtype.Active = 0;
			
			UtilGtk.ComboUpdate(combo_stats_stat_apply_to, 
				SqliteJumpType.SelectJumpTypes(false, Constants.AllJumpsNameStr(), "TC", true), ""); //only select name
			combo_stats_stat_apply_to.Sensitive = true;
			combo_stats_stat_apply_to.Active = 0;
		} 
		else if (UtilGtk.ComboGetActive(combo_stats_stat_type) == Constants.TypeJumpsReactiveStr() )
		{
			combo_stats_stat_subtype.Changed -= new EventHandler (on_combo_stats_stat_subtype_changed);
			UtilGtk.ComboUpdate(combo_stats_stat_subtype, comboStatsSubTypeReactiveOptions, "");
			combo_stats_stat_subtype.Changed += new EventHandler (on_combo_stats_stat_subtype_changed);
			
			combo_stats_stat_subtype.Sensitive = true;
			combo_stats_stat_subtype.Active = 0;
			
			UtilGtk.ComboUpdate(combo_stats_stat_apply_to, 
				SqliteJumpType.SelectJumpRjTypes(Constants.AllJumpsNameStr(), true), ""); //only select name
			combo_stats_stat_apply_to.Sensitive = true;
			combo_stats_stat_apply_to.Active = 0;
		}
		else if (UtilGtk.ComboGetActive(combo_stats_stat_type) == Constants.TypeRunsSimpleStr() )
		{
			combo_stats_stat_subtype.Changed -= new EventHandler (on_combo_stats_stat_subtype_changed);
			UtilGtk.ComboUpdate(combo_stats_stat_subtype, comboStatsSubTypeSimpleOptions, "");
			combo_stats_stat_subtype.Changed += new EventHandler (on_combo_stats_stat_subtype_changed);
			
			combo_stats_stat_subtype.Sensitive = false;
			combo_stats_stat_subtype.Active = 0;
			
			UtilGtk.ComboUpdate(combo_stats_stat_apply_to, 
				SqliteRunType.SelectRunTypes(Constants.AllRunsNameStr(), true), ""); //only select name
			combo_stats_stat_apply_to.Sensitive = true;
			combo_stats_stat_apply_to.Active = 0;
		} 
		else if (UtilGtk.ComboGetActive(combo_stats_stat_type) == Constants.TypeRunsIntervallicStr() )
		{
			combo_stats_stat_subtype.Changed -= new EventHandler (on_combo_stats_stat_subtype_changed);
			UtilGtk.ComboUpdate(combo_stats_stat_subtype, comboStatsSubTypeSimpleOptions, "");
			combo_stats_stat_subtype.Changed += new EventHandler (on_combo_stats_stat_subtype_changed);
			
			combo_stats_stat_subtype.Sensitive = false;
			combo_stats_stat_subtype.Active = 0;
			
			UtilGtk.ComboUpdate(combo_stats_stat_apply_to, 
				SqliteRunIntervalType.SelectRunIntervalTypes(Constants.AllRunsNameStr(), true), ""); //only select name
			combo_stats_stat_apply_to.Sensitive = true;
			combo_stats_stat_apply_to.Active = 0;
		} 

		showUpdateStatsAndHideData(true);
	}
		
	private void updateComboStatsSubType() {
		bool showMarkConsecutives = false;
		bool showSubtractionBetweenTests = false;
		label_apply_to.Visible = true;
		combo_stats_stat_apply_to.Visible = true;
		//subtraction_between_tests_show(false);
		if (UtilGtk.ComboGetActive(combo_stats_stat_type) == Constants.TypeJumpsSimpleStr() )
		{
			if(UtilGtk.ComboGetActive(combo_stats_stat_subtype) == Catalog.GetString("No indexes")) {
				UtilGtk.ComboUpdate(combo_stats_stat_apply_to, 
					SqliteJumpType.SelectJumpTypes(false, Constants.AllJumpsNameStr(), "nonTC", true), ""); //only select name
				combo_stats_stat_apply_to.Sensitive = true;
				combo_stats_stat_apply_to.Active = 0;
			} else if (UtilGtk.ComboGetActive(combo_stats_stat_subtype) == Constants.ChronojumpProfileStr()) {
				UtilGtk.ComboUpdate(combo_stats_stat_apply_to, "SJ, SJl 100%, CMJ, ABK, DJa");
				combo_stats_stat_apply_to.Active = 0;
				combo_stats_stat_apply_to.Sensitive = false;
			} else if (UtilGtk.ComboGetActive(combo_stats_stat_subtype) == Constants.IeIndexFormula) {
				UtilGtk.ComboUpdate(combo_stats_stat_apply_to, "CMJ, SJ");
				combo_stats_stat_apply_to.Active = 0;
				combo_stats_stat_apply_to.Sensitive = false;
			} else if (UtilGtk.ComboGetActive(combo_stats_stat_subtype) == Constants.ArmsUseIndexFormula) {
				UtilGtk.ComboUpdate(combo_stats_stat_apply_to, "ABK, CMJ");
				combo_stats_stat_apply_to.Active = 0;
				combo_stats_stat_apply_to.Sensitive = false;
			} else if (UtilGtk.ComboGetActive(combo_stats_stat_subtype) == Constants.IRnaIndexFormula) {
				UtilGtk.ComboUpdate(combo_stats_stat_apply_to, "DJna, CMJ");
				combo_stats_stat_apply_to.Active = 0;
				combo_stats_stat_apply_to.Sensitive = false;
			} else if (UtilGtk.ComboGetActive(combo_stats_stat_subtype) == Constants.IRaIndexFormula) {
				UtilGtk.ComboUpdate(combo_stats_stat_apply_to, "DJa, CMJ");
				combo_stats_stat_apply_to.Active = 0;
				combo_stats_stat_apply_to.Sensitive = false;
			} else if (UtilGtk.ComboGetActive(combo_stats_stat_subtype) == Constants.FvIndexFormula) {
				//"F/V sj+(100%)/sj *100",	//fvIndexFormula
				UtilGtk.ComboUpdate(combo_stats_stat_apply_to, "SJl(100%), SJ");
				combo_stats_stat_apply_to.Active = 0;
				combo_stats_stat_apply_to.Sensitive = false;
			} else if (UtilGtk.ComboGetActive(combo_stats_stat_subtype) == Catalog.GetString(Constants.SubtractionBetweenTests)) {
				UtilGtk.ComboUpdate(combo_subtraction_between_1, 
					SqliteJumpType.SelectJumpTypes(false, "", "", true), ""); //only select name
				UtilGtk.ComboUpdate(combo_subtraction_between_2, 
					SqliteJumpType.SelectJumpTypes(false, "", "", true), ""); //only select name
				//subtraction_between_tests_show(true);
				showSubtractionBetweenTests = true;
				label_apply_to.Visible = false;
				combo_stats_stat_apply_to.Visible = false;
				combo_stats_stat_apply_to.Active = 0;
			} else if (UtilGtk.ComboGetActive(combo_stats_stat_subtype) == Constants.PotencySayersSJFormulaShortStr()) {
				combo_stats_stat_apply_to.Active = 
					UtilGtk.ComboUpdate(combo_stats_stat_apply_to, 
							SqliteJumpType.SelectJumpTypes(false, "", "nonTC", true), //only select name
							"SJ"); //default value
				combo_stats_stat_apply_to.Sensitive = false;
			} else {/*
				   this applies to all potency formulas (default is CMJ), except SayersSJ 
				Constants.PotencyLewisFormulaShort,
				Constants.PotencyHarmanFormulaShort,
				Constants.PotencySayersCMJFormulaShort,
				Constants.PotencyShettyFormulaShort,
				Constants.PotencyCanavanFormulaShort,
				Constants.PotencyBahamondeFormulaShort,
				Constants.PotencyLaraMaleApplicantsSCFormulaShort,
				Constants.PotencyLaraFemaleEliteVoleiFormulaShort,
				Constants.PotencyLaraFemaleMediumVoleiFormulaShort,
				Constants.PotencyLaraFemaleSCStudentsFormulaShort,
				Constants.PotencyLaraFemaleSedentaryFormulaShort
				*/
				combo_stats_stat_apply_to.Active = 
					UtilGtk.ComboUpdate(combo_stats_stat_apply_to, 
							SqliteJumpType.SelectJumpTypes(false, "", "nonTC", true), //only select name
							"CMJ"); //default value
				combo_stats_stat_apply_to.Sensitive = false;
			}
		}  
		else if (UtilGtk.ComboGetActive(combo_stats_stat_type) == Constants.TypeJumpsSimpleWithTCStr() )
		{
			UtilGtk.ComboUpdate(combo_stats_stat_apply_to, 
				SqliteJumpType.SelectJumpTypes(false, Constants.AllJumpsNameStr(), "TC", true), ""); //only select name
			combo_stats_stat_apply_to.Sensitive = true;
			combo_stats_stat_apply_to.Active = 0;
		} 
		
		/*
		   if is RjEvolution, or runIntervallic show mark consecutives, graph only with lines and transposed
		   */
		if ( UtilGtk.ComboGetActive(combo_stats_stat_subtype) == Catalog.GetString("Evolution") ||
				UtilGtk.ComboGetActive(combo_stats_stat_type) == Constants.TypeRunsIntervallicStr() ) {
			//hbox_mark_consecutives.Show();
			showMarkConsecutives = true;
			checkbutton_transposed.Active = true;
			checkbutton_transposed.Sensitive = false;
			UtilGtk.ComboUpdate(combo_graph_type, Util.StringToStringArray(Constants.GraphTypeLines), "");
			combo_graph_type.Active=0;
		} else if (radiobutton_selected_sessions.Active) {
			//on multi session use only barplot and lines
			combo_graph_type.Active = UtilGtk.ComboUpdate(combo_graph_type, Constants.GraphTypesMultisession, Constants.GraphTypeLines);
		} else {
			//hbox_mark_consecutives.Hide();
			checkbutton_transposed.Active = false;
			checkbutton_transposed.Sensitive = true;
			UtilGtk.ComboUpdate(combo_graph_type, Constants.GraphTypes, "");
			combo_graph_type.Active=0;
		}
		
		if(showMarkConsecutives || showSubtractionBetweenTests) {
			notebook_stats_win_options.Show();
			if(showSubtractionBetweenTests)
				notebook_stats_win_options.CurrentPage = 0;
			else
				notebook_stats_win_options.CurrentPage = 1;
		} else
			notebook_stats_win_options.Hide();
	}
	
	//way of accessing from chronojump.cs
	private void stats_win_fillTreeView_stats (bool graph, bool force) 
	{
		showUpdateStatsAndHideData(true);
	}

	//creates a GraphROptions object	
	private GraphROptions fillGraphROptions() {
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
				Convert.ToInt32(spin_graph_margin_r.Value),
				Convert.ToDouble(spin_graph_x_cex_axis.Value)
				);
	}

	private bool fillTreeView_stats (bool graph) 
	{
		if(blockFillingTreeview)
			return false;
		
		LogB.Information("----------FILLING treeview stats---------------");
	
		string statisticType = UtilGtk.ComboGetActive(combo_stats_stat_type);
		string statisticSubType = UtilGtk.ComboGetActive(combo_stats_stat_subtype);
		
		string statisticApplyTo = "";
		if (UtilGtk.ComboGetActive(combo_stats_stat_subtype) == 
				Catalog.GetString(Constants.SubtractionBetweenTests)) 
			statisticApplyTo = UtilGtk.ComboGetActive(combo_subtraction_between_1) + "," +
				UtilGtk.ComboGetActive(combo_subtraction_between_2);
		else 
			statisticApplyTo = UtilGtk.ComboGetActive(combo_stats_stat_apply_to);
		


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

		//the mark best jumps or runs is only on rjEvolution and runInterval
		//runInterval has only one stat subtype and is like rjEvolution
		int evolution_mark_consecutives = -1;
		if (
				( UtilGtk.ComboGetActive(combo_stats_stat_subtype) == Catalog.GetString("Evolution") ||
				UtilGtk.ComboGetActive(combo_stats_stat_type) == Constants.TypeRunsIntervallicStr() ) &&
			checkbutton_mark_consecutives.Active ) {
			evolution_mark_consecutives = Convert.ToInt32 ( spinbutton_mark_consecutives.Value ); 
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
				checkbutton_stats_sex.Active,  
				statsJumpsType,
				limit, 
				markedRows,
				evolution_mark_consecutives,
				graphROptions,
				graph,
				toReport,  //always false in this class
				preferences
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
			LogB.Warning("Do markedRows stuff later");
		}

		label_enunciate.Text = myStatType.Enunciate;
		
		//show/hide persons selector on comboCheckboxesOptions
		if(! graph) {
			if(UtilGtk.ComboGetActive(combo_stats_stat_type) != Constants.TypeSessionSummaryStr() &&
					UtilGtk.ComboGetActive(combo_stats_stat_type) != Constants.TypeJumperSummaryStr())
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
		
		showUpdateStatsAndHideData(false);

		if(allFine) {
			return true;
		} else {
			return false;
		}
	}

	//changes the combo_select_checkboxes to "Selected" if any row in the treeview is checked or unchecked
	private void on_fake_button_row_checked_clicked (object o, EventArgs args) {
		LogB.Information("fakeButtonRowCheckedUnchecked in gui/stats.cs !!");

		combo_select_checkboxes.Active = UtilGtk.ComboMakeActive(comboCheckboxesOptions, Catalog.GetString("Selected"));
	}
	
	private void on_fake_button_rows_selected_clicked (object o, EventArgs args) {
		LogB.Information("fakeButtonRowsSelected in gui/stats.cs !!");
		button_graph.Sensitive = true;
		button_add_to_report.Sensitive = true;
	}
	
	private void on_fake_button_no_rows_selected_clicked (object o, EventArgs args) {
		LogB.Information("fakeButtonNoRowsSelected in gui/stats.cs !!");
		button_graph.Sensitive = false;
		button_add_to_report.Sensitive = false;

		//put none in combo
		combo_select_checkboxes.Active = UtilGtk.ComboMakeActive(comboCheckboxesOptions, Catalog.GetString("None"));
	}
	

	
	/* ---------------------------------------------------------
	 * ----------------  STATS CALLBACKS--------------------
	 *  --------------------------------------------------------
	 */

	private void on_button_stats_graph_options_clicked (object o, EventArgs args) {
		hbox_graph_options.Visible = ! hbox_graph_options.Visible;
	}

	private void on_button_stats_clicked (object o, EventArgs args) {
		fillTreeView_stats(false);
	}

	private void on_button_graph_clicked (object o, EventArgs args) {
		fillTreeView_stats(true);
	}

	
	private void on_checkbutton_show_enunciate_clicked(object o, EventArgs args) {
		if (checkbutton_show_enunciate.Active) {
			label_enunciate.Show();
			scrolledwindow_enunciate.Show();
		} else {
			label_enunciate.Hide();
			scrolledwindow_enunciate.Hide();
		}
	}

	
	private void on_checkbutton_mark_consecutives_clicked(object o, EventArgs args) {
		if(checkbutton_mark_consecutives.Active) {
			spinbutton_mark_consecutives.Sensitive = true;
		} else {
			spinbutton_mark_consecutives.Sensitive = false;
		}
		
		showUpdateStatsAndHideData(true);
	}
	
	void on_spinbutton_mark_consecutives_changed (object o, EventArgs args) {
		showUpdateStatsAndHideData(true);
	}

	
	private void update_stats_widgets_sensitiveness() {
		if(blockFillingTreeview)
			return;

		//blank statusbar
		//appbar2.Push( 1, "");

		string statisticType = UtilGtk.ComboGetActive(combo_stats_stat_type);
		string statisticSubType = UtilGtk.ComboGetActive(combo_stats_stat_subtype);
			
		//for an unknown reason, when we select an option in the combo stats, 
		//the on_combo_stats_stat_type_changed it's called two times? 
		//in the first the value of Entry.Text is "";
		if(statisticType == "" || statisticSubType == "") 
			return;
			
		//some stats should not be showed as limited jumps
		if( (statisticType == Constants.TypeJumpsReactiveStr() &&
				( statisticSubType == Catalog.GetString("Evolution") ||
				  statisticSubType == Constants.RJAVGSDRjIndexName ||
				  statisticSubType == Constants.RJAVGSDQIndexName) ) 
				|| statisticType == Constants.TypeRunsIntervallicStr() )
		{
			//don't allow Evolution be multisession
			radiobutton_current_session.Active = true;
			radiobutton_selected_sessions.Sensitive = false;
			
			radiobutton_stats_jumps_all.Sensitive = true;
			radiobutton_stats_jumps_limit.Sensitive = true;
			//has no sense to study the AVG of rj tv tc evolution string
			//not fair to make avg of each subjump, 
			//specially when some RJs have more jumps than others
			//TODO: check this for runInterval
			if(radiobutton_stats_jumps_person_average.Active) {
				radiobutton_stats_jumps_person_bests.Active = true;
			}
			radiobutton_stats_jumps_person_average.Sensitive = false;
		}
		//in Potency formulas show only "all jumps" radiobutton
		else if(statisticType == Constants.TypeJumpsSimpleStr() && (
					statisticSubType == Constants.PotencyLewisFormulaShortStr() ||
					statisticSubType == Constants.PotencyHarmanFormulaShortStr() ||
					statisticSubType == Constants.PotencySayersSJFormulaShortStr() ||
					statisticSubType == Constants.PotencySayersCMJFormulaShortStr() ||
					statisticSubType == Constants.PotencyShettyFormulaShortStr() ||
					statisticSubType == Constants.PotencyCanavanFormulaShortStr() ||
					//statisticSubType == Constants.PotencyBahamondeFormulaShort ||
					statisticSubType == Constants.PotencyLaraMaleApplicantsSCFormulaShortStr() ||
					statisticSubType == Constants.PotencyLaraFemaleEliteVoleiFormulaShortStr() ||
					statisticSubType == Constants.PotencyLaraFemaleMediumVoleiFormulaShortStr() ||
					statisticSubType == Constants.PotencyLaraFemaleSCStudentsFormulaShortStr() ||
					statisticSubType == Constants.PotencyLaraFemaleSedentaryFormulaShortStr()
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
		else if(statisticType == Constants.TypeJumpsSimpleStr() && statisticSubType == Constants.ChronojumpProfileStr()) {
			//on Chronojump profile only best jumps are used
			radiobutton_stats_jumps_person_bests.Active = true;
			//make no sensitive
			spin_stats_jumps_person_bests.Sensitive = false;
			radiobutton_stats_jumps_all.Sensitive = false;
			radiobutton_stats_jumps_limit.Sensitive = false;
			radiobutton_stats_jumps_person_average.Sensitive = false;
		}
		else if(statisticType == Constants.TypeSessionSummaryStr() || 
				statisticType == Constants.TypeJumperSummaryStr() ||
				( statisticType == Constants.TypeJumpsSimpleStr() &&
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
			showUpdateStatsAndHideData(true);
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
			showUpdateStatsAndHideData(true);
		}
	}
	
	private void on_combo_subtraction_between_1_or_2_changed(object o, EventArgs args) {
		string myText = UtilGtk.ComboGetActive(combo_subtraction_between_1);
		string myText2 = UtilGtk.ComboGetActive(combo_subtraction_between_2);
		if (myText != "" && myText2 != "") {
			showUpdateStatsAndHideData(true);
		}
	}
	
	private void on_radiobuttons_stat_session_toggled (object o, EventArgs args)
	{
		if(o == (object) radiobutton_current_session) 
		{
			LogB.Information("current");
			button_stats_select_sessions.Sensitive = false;

			//single session can have all graph types
			combo_graph_type.Active = UtilGtk.ComboUpdate(
					combo_graph_type, Constants.GraphTypes, UtilGtk.ComboGetActive(combo_graph_type));
			//except
			if ( UtilGtk.ComboGetActive(combo_stats_stat_subtype) == Catalog.GetString("Evolution") ||
					UtilGtk.ComboGetActive(combo_stats_stat_type) == Constants.TypeRunsIntervallicStr() ) {
				notebook_stats_win_options.Show();
				checkbutton_transposed.Active = true;
				checkbutton_transposed.Sensitive = false;
				UtilGtk.ComboUpdate(combo_graph_type, Util.StringToStringArray(Constants.GraphTypeLines), "");
				combo_graph_type.Active=0;
			} else
				notebook_stats_win_options.Hide();
		} 
		else if (o == (object) radiobutton_selected_sessions ) 
		{
			LogB.Information("selected");
			button_stats_select_sessions.Sensitive = true;
			
			//multi session use only barplot and lines
			combo_graph_type.Active = UtilGtk.ComboUpdate(combo_graph_type, Constants.GraphTypesMultisession, Constants.GraphTypeLines);
		}
		update_stats_widgets_sensitiveness();
		showUpdateStatsAndHideData(true);
	}
	
	private void on_checkbutton_stats_sex_clicked(object o, EventArgs args)
	{
		showUpdateStatsAndHideData(true);
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
		
		showUpdateStatsAndHideData(true);
	}
	
	void on_spinbutton_stats_jumps_changed (object o, EventArgs args)
	{
		showUpdateStatsAndHideData(true);
	}
	
	private void on_button_stats_select_sessions_clicked (object o, EventArgs args) {
		LogB.Information("select sessions for stats");
		sessionSelectStatsWin = SessionSelectStatsWindow.Show(app1, selectedSessions);
		sessionSelectStatsWin.Button_accept.Clicked += new EventHandler(on_stats_select_sessions_accepted);
	}
	
	private void on_stats_select_sessions_accepted (object o, EventArgs args) {
		LogB.Information("select sessions for stats accepted");
		
		if ((sessionSelectStatsWin.ArrayOfSelectedSessions[0]).ToString() != "-1") { 
			//there are sessionsSelected, put them in selectedSessions ArrayList
			selectedSessions = sessionSelectStatsWin.ArrayOfSelectedSessions;
		} else {
			//there are NO sessionsSelected, put currentSession in selectedSession ArrayList
			selectedSessions = new ArrayList(2);
			selectedSessions.Add(currentSession.UniqueID + ":" + currentSession.Name + ":" + currentSession.Date);
		}

		update_stats_widgets_sensitiveness();

		showUpdateStatsAndHideData(true);
	}
	
	private void on_button_add_to_report_clicked (object o, EventArgs args) {
		LogB.Information("add to report window");

		string statisticType = UtilGtk.ComboGetActive(combo_stats_stat_type);
		string statisticSubType = UtilGtk.ComboGetActive(combo_stats_stat_subtype);
		if(statisticType == "" && statisticSubType == "") {
			//for an unknown reason, when we select an option in the combo stats, 
			//the on_combo_stats_stat_type_changed it's called two times? 
			//in the first the value of Entry.Text is "";
			return;
		} else {
			if ( ( statisticSubType == Catalog.GetString("Evolution") ||
					statisticType == Constants.TypeRunsIntervallicStr() ) &&
				checkbutton_mark_consecutives.Active ) {
				statisticSubType += "." + ( spinbutton_mark_consecutives.Value ).ToString(); 
			}
			
			string statisticApplyTo = "";
			if (UtilGtk.ComboGetActive(combo_stats_stat_subtype) == 
					Catalog.GetString(Constants.SubtractionBetweenTests)) 
				statisticApplyTo = UtilGtk.ComboGetActive(combo_subtraction_between_1) + "," +
					UtilGtk.ComboGetActive(combo_subtraction_between_2);
			else 
				statisticApplyTo = UtilGtk.ComboGetActive(combo_stats_stat_apply_to);
		
			if(statisticApplyTo.Length == 0) 
				statisticApplyTo = "-";
	
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
			reportWin = ReportWindow.Show(app1, report);
			//add current stat
			reportWin.Add(statisticType, statisticSubType, statisticApplyTo, 
					sendSelectedSessions, statsShowJumps, showSex.ToString(), 
					myStatType.MarkedRows, fillGraphROptions());
					
			
			//appbar2.Push( 1, Catalog.GetString("Successfully added") + " " + statisticType + "-" + statisticSubType + "-" + statisticApplyTo);
		}
		
	}
	
	
	private void on_show_report_clicked (object o, EventArgs args) {
		reportWin = ReportWindow.Show(app1, report);
	}

	private void connectWidgetsStats (Gtk.Builder builder)
	{
		notebook_stats_sup = (Gtk.Notebook) builder.GetObject ("notebook_stats_sup");
		notebook_stats = (Gtk.Notebook) builder.GetObject ("notebook_stats");
		vbox_stats = (Gtk.Box) builder.GetObject ("vbox_stats");
		treeview_stats = (Gtk.TreeView) builder.GetObject ("treeview_stats");
		vbox_stats_type = (Gtk.Box) builder.GetObject ("vbox_stats_type");
		hbox_combo_stats_stat_type = (Gtk.Box) builder.GetObject ("hbox_combo_stats_stat_type");
		hbox_combo_stats_stat_subtype = (Gtk.Box) builder.GetObject ("hbox_combo_stats_stat_subtype");
		hbox_combo_stats_stat_apply_to = (Gtk.Box) builder.GetObject ("hbox_combo_stats_stat_apply_to");
		hbox_mark_and_enunciate = (Gtk.Box) builder.GetObject ("hbox_mark_and_enunciate");
		frame_graph_and_report = (Gtk.Frame) builder.GetObject ("frame_graph_and_report");
		label_apply_to = (Gtk.Label) builder.GetObject ("label_apply_to");
		checkbutton_stats_sex = (Gtk.CheckButton) builder.GetObject ("checkbutton_stats_sex");
		button_stats = (Gtk.Button) builder.GetObject ("button_stats");

		radiobutton_current_session = (Gtk.RadioButton) builder.GetObject ("radiobutton_current_session");
		radiobutton_selected_sessions = (Gtk.RadioButton) builder.GetObject ("radiobutton_selected_sessions");
		button_stats_select_sessions = (Gtk.Button) builder.GetObject ("button_stats_select_sessions");
		radiobutton_stats_jumps_all = (Gtk.RadioButton) builder.GetObject ("radiobutton_stats_jumps_all");
		radiobutton_stats_jumps_limit = (Gtk.RadioButton) builder.GetObject ("radiobutton_stats_jumps_limit");
		spin_stats_jumps_limit = (Gtk.SpinButton) builder.GetObject ("spin_stats_jumps_limit");
		radiobutton_stats_jumps_person_bests = (Gtk.RadioButton) builder.GetObject ("radiobutton_stats_jumps_person_bests");
		spin_stats_jumps_person_bests = (Gtk.SpinButton) builder.GetObject ("spin_stats_jumps_person_bests");
		radiobutton_stats_jumps_person_average = (Gtk.RadioButton) builder.GetObject ("radiobutton_stats_jumps_person_average");
		button_graph = (Gtk.Button) builder.GetObject ("button_graph");
		button_add_to_report = (Gtk.Button) builder.GetObject ("button_add_to_report");

		label_enunciate = (Gtk.Label) builder.GetObject ("label_enunciate");
		scrolledwindow_enunciate = (Gtk.ScrolledWindow) builder.GetObject ("scrolledwindow_enunciate");
		checkbutton_show_enunciate = (Gtk.CheckButton) builder.GetObject ("checkbutton_show_enunciate");

		notebook_stats_win_options = (Gtk.Notebook) builder.GetObject ("notebook_stats_win_options");
		checkbutton_mark_consecutives = (Gtk.CheckButton) builder.GetObject ("checkbutton_mark_consecutives");
		spinbutton_mark_consecutives = (Gtk.SpinButton) builder.GetObject ("spinbutton_mark_consecutives");
		hbox_subtraction_between_1 = (Gtk.Box) builder.GetObject ("hbox_subtraction_between_1");
		hbox_subtraction_between_2 = (Gtk.Box) builder.GetObject ("hbox_subtraction_between_2");

		hbox_combo_select_checkboxes = (Gtk.Box) builder.GetObject ("hbox_combo_select_checkboxes");

		image_stats_win_graph = (Gtk.Image) builder.GetObject ("image_stats_win_graph");
		image_stats_win_graph1 = (Gtk.Image) builder.GetObject ("image_stats_win_graph1");
		image_stats_win_graph3 = (Gtk.Image) builder.GetObject ("image_stats_win_graph3");
		image_stats_win_report_open = (Gtk.Image) builder.GetObject ("image_stats_win_report_open");

		hbox_graph_options = (Gtk.Box) builder.GetObject ("hbox_graph_options");

		hbox_combo_graph_type = (Gtk.Box) builder.GetObject ("hbox_combo_graph_type");
		hbox_stats_variables = (Gtk.Box) builder.GetObject ("hbox_stats_variables");
		hbox_combo_graph_var_x = (Gtk.Box) builder.GetObject ("hbox_combo_graph_var_x");
		hbox_combo_graph_var_y = (Gtk.Box) builder.GetObject ("hbox_combo_graph_var_y");
		hbox_combo_graph_palette = (Gtk.Box) builder.GetObject ("hbox_combo_graph_palette");
		checkbutton_transposed = (Gtk.CheckButton) builder.GetObject ("checkbutton_transposed");
		label_line = (Gtk.Label) builder.GetObject ("label_line");
		spin_line = (Gtk.SpinButton) builder.GetObject ("spin_line");

		hbox_combo_graph_width = (Gtk.Box) builder.GetObject ("hbox_combo_graph_width");
		hbox_combo_graph_height = (Gtk.Box) builder.GetObject ("hbox_combo_graph_height");
		label_graph_legend = (Gtk.Label) builder.GetObject ("label_graph_legend");
		hbox_combo_graph_legend = (Gtk.Box) builder.GetObject ("hbox_combo_graph_legend");

		spin_graph_margin_b = (Gtk.SpinButton) builder.GetObject ("spin_graph_margin_b"); //bottom
		spin_graph_margin_l = (Gtk.SpinButton) builder.GetObject ("spin_graph_margin_l"); //left
		spin_graph_margin_t = (Gtk.SpinButton) builder.GetObject ("spin_graph_margin_t"); //top
		spin_graph_margin_r = (Gtk.SpinButton) builder.GetObject ("spin_graph_margin_r"); //right
		spin_graph_x_cex_axis = (Gtk.SpinButton) builder.GetObject ("spin_graph_x_cex_axis"); //font size of x axis
		label_stats_x_axis = (Gtk.Label) builder.GetObject ("label_stats_x_axis");
	}
}

