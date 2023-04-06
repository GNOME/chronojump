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
 * Copyright (C) 2004-2023   Xavier de Blas <xaviblas@gmail.com>
 */


using System;
using Gtk;
//using Glade;
using Mono.Unix;

public partial class ChronoJumpWindow 
{
	/*
	   ---- jumpsEvolution ----
	   */

	// at glade ---->
	Gtk.DrawingArea drawingarea_jumps_evolution;
	Gtk.Image image_tab_jumps_evolution;
	Gtk.Image image_jumps_evolution_save;
	Gtk.HBox hbox_combo_select_jumps_evolution;
	Gtk.Button button_jumps_evolution_save_image;
	Gtk.CheckButton check_jumps_evolution_only_best_in_session;

	Gtk.DrawingArea drawingarea_runs_evolution;
	Gtk.Image image_tab_runs_evolution;
	Gtk.Image image_runs_evolution_save;
	Gtk.Image image_runs_evolution_analyze_image_save;
	Gtk.HBox hbox_combo_select_runs_evolution;
	Gtk.HBox hbox_combo_select_runs_evolution_distance;
	Gtk.Button button_runs_evolution_save_image;
	Gtk.CheckButton check_runs_evolution_only_best_in_session;
	Gtk.CheckButton check_runs_evolution_show_time;
	// <---- at glade

	Gtk.ComboBoxText combo_select_jumps_evolution;

	JumpsEvolution jumpsEvolution;
	JumpsEvolutionGraph jumpsEvolutionGraph;
	CjComboSelectJumps comboSelectJumpsEvolution;

	// combo (start)
	private void createComboSelectJumpsEvolution(bool create)
	{
		if(create)
		{
			comboSelectJumpsEvolution = new CjComboSelectJumps(combo_select_jumps_evolution, hbox_combo_select_jumps_evolution, false);
			combo_select_jumps_evolution = comboSelectJumpsEvolution.Combo;
			combo_select_jumps_evolution.Changed += new EventHandler (on_combo_select_jumps_evolution_changed);
		} else {
			comboSelectJumpsEvolution.Fill();
			combo_select_jumps_evolution = comboSelectJumpsEvolution.Combo;
		}
		combo_select_jumps_evolution.Sensitive = true;
	}
	private void on_combo_select_jumps_evolution_changed(object o, EventArgs args)
	{
		//ComboBoxText combo = o as ComboboxText;
		if (o == null)
			return;

		jumpsEvolutionCalculate ();
		drawingarea_jumps_evolution.QueueDraw ();
	}
	// combo (end)

	private void on_check_jumps_evolution_only_best_in_session_clicked (object o, EventArgs args)
	{
		jumpsEvolutionCalculate ();
		drawingarea_jumps_evolution.QueueDraw ();

		SqlitePreferences.Update(SqlitePreferences.JumpsEvolutionOnlyBestInSession,
				check_jumps_evolution_only_best_in_session.Active, false);
	}

	private void jumpsEvolutionCalculate ()
	{
		// 1) exit, if problems
		if(currentPerson == null || currentSession == null ||
				drawingarea_jumps_evolution == null || drawingarea_jumps_evolution.Window == null) //it happens at start on click on analyze
			return;

		// 2) create jumpsEvolution, if needed
		if(jumpsEvolution == null)
			jumpsEvolution = new JumpsEvolution();

		string jumpType = comboSelectJumpsEvolution.GetSelectedNameEnglish();

		jumpsEvolution.MouseReset ();
		jumpsEvolution.Calculate(currentPerson.UniqueID, jumpType, check_jumps_evolution_only_best_in_session.Active);
	}

	//called just by QueueDraw
	private void jumpsEvolutionPlot ()
	{
		// 1) exit, if problems
		if(currentPerson == null || currentSession == null ||
				drawingarea_jumps_evolution == null || drawingarea_jumps_evolution.Window == null) //it happens at start on click on analyze
		{
			button_jumps_evolution_save_image.Sensitive = false;
			return;
		}

		// 2) create jumpsEvolution, if needed
		if(jumpsEvolution == null)
			jumpsEvolutionCalculate ();

		// 3) get jump type
		string jumpType = comboSelectJumpsEvolution.GetSelectedNameEnglish();

		// 4) exit if test incompatible (takeOff and takeOffWeight have no flight time, so this graph crashes)
		if(jumpType == Constants.TakeOffName || jumpType == Constants.TakeOffWeightName)
		{
			new JumpsEvolutionGraph(drawingarea_jumps_evolution,
					JumpsEvolutionGraph.Error.TESTINCOMPATIBLE, jumpType, preferences.fontType.ToString());

			button_jumps_evolution_save_image.Sensitive = false;
			return;
		}

		// 6) exit if no points, or do the graph
		if(jumpsEvolution.Point_l.Count == 0)
		{
			//constructor for showing blank screen with a message
			new JumpsEvolutionGraph(drawingarea_jumps_evolution,
					JumpsEvolutionGraph.Error.NEEDJUMP, jumpType, preferences.fontType.ToString());
					//currentPerson.Name, jumpType, currentSession.DateShort);

			button_jumps_evolution_save_image.Sensitive = false;

		} else {
			//regular constructor
			jumpsEvolutionGraph = new JumpsEvolutionGraph(
					jumpsEvolution.Point_l,
					jumpsEvolution.Dates_l,
					jumpsEvolution.Slope,
					jumpsEvolution.Intercept,
					drawingarea_jumps_evolution,
					currentPerson.Name, jumpType, currentSession.DateShort,
					jumpsEvolution.MouseX,
					jumpsEvolution.MouseY);
			jumpsEvolutionGraph.Do(preferences.fontType.ToString());

			button_jumps_evolution_save_image.Sensitive = true;
		}
	}
	private void on_drawingarea_jumps_evolution_draw (object o, Gtk.DrawnArgs args) 
	{
		jumpsEvolutionPlot ();
		//data is calculated on switch page (at notebook_capture_analyze) or on change person
	}

	private void on_drawingarea_jumps_evolution_button_press_event (object o, ButtonPressEventArgs args)
	{
		//if there is no data and nothing to show, nothing to press, and also this is null
		// without jumpsEvolution.Point_l.Count == 0 condition, it will show graph/data on mouse (with data of previous person/graph)
		if(jumpsEvolutionGraph == null || jumpsEvolution.Point_l.Count == 0)
			return;

		LogB.Information("Button press done!");

		//redo the graph to delete previous rectangles of previous mouse clicks
		if (jumpsEvolution != null)
			jumpsEvolution.MouseSet (args.Event.X, args.Event.Y);

		drawingarea_jumps_evolution.QueueDraw ();
	}

	private void on_button_jumps_evolution_save_image_clicked (object o, EventArgs args)
	{
		checkFile(Constants.CheckFileOp.JUMPS_EVOLUTION_SAVE_IMAGE);
	}

	private void on_button_jumps_evolution_save_image_selected (string destination)
	{
		if(drawingarea_jumps_evolution == null)
			return;

		LogB.Information("Saving");
		CairoUtil.GetScreenshotFromDrawingArea (drawingarea_jumps_evolution, destination);
	}
	private void on_overwrite_file_jumps_evolution_save_image_accepted (object o, EventArgs args)
	{
		on_button_jumps_evolution_save_image_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), exportFileName);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}


	/*
	   ---- runsEvolution ----
	   */

	RunsEvolution runsEvolution;
	RunsEvolutionGraph runsEvolutionGraph;
	CjComboSelectRuns comboSelectRunsEvolution;
	Gtk.ComboBoxText combo_select_runs_evolution;
	Gtk.ComboBoxText combo_select_runs_evolution_distance;

	// combo (start)
	private void createComboSelectRunsEvolution(bool create)
	{
		if(create)
		{
			comboSelectRunsEvolution = new CjComboSelectRuns(combo_select_runs_evolution, hbox_combo_select_runs_evolution);
			combo_select_runs_evolution = comboSelectRunsEvolution.Combo;
			combo_select_runs_evolution.Changed += new EventHandler (on_combo_select_runs_evolution_changed);
		} else {
			comboSelectRunsEvolution.Fill();
			combo_select_runs_evolution = comboSelectRunsEvolution.Combo;
		}
		combo_select_runs_evolution.Sensitive = true;
	}
	private void on_combo_select_runs_evolution_changed(object o, EventArgs args)
	{
		//ComboBoxText combo = o as ComboboxText;
		if (o == null)
			return;

		runsEvolutionCalculate (true);
		drawingarea_runs_evolution.QueueDraw ();
	}
	// combo (end)

	// combo (start)
	private bool combo_select_runs_evolution_distance_follow_signals;
	private void createComboSelectRunsEvolutionDistance()
	{
		combo_select_runs_evolution_distance = new ComboBoxText();
		//UtilGtk.ComboUpdate (combo_select_runs_evolution_distance, Catalog.GetString("All"));
		//combo_select_runs_evolution_distance.Active = 0;
		hbox_combo_select_runs_evolution_distance.PackStart(combo_select_runs_evolution_distance, true, true, 0);

		combo_select_runs_evolution_distance_follow_signals = false;
		combo_select_runs_evolution_distance.Changed += new EventHandler (on_combo_select_runs_evolution_distance_changed);
		combo_select_runs_evolution_distance_follow_signals = true;

		combo_select_runs_evolution_distance.Sensitive = true;
		combo_select_runs_evolution_distance.Visible = true;
		hbox_combo_select_runs_evolution_distance.Visible = true;
	}
	private void on_combo_select_runs_evolution_distance_changed(object o, EventArgs args)
	{
		if(! combo_select_runs_evolution_distance_follow_signals)
			return;

		//ComboBoxText combo = o as ComboboxText;
		if (o == null)
			return;

		runsEvolutionCalculate (false);
		drawingarea_runs_evolution.QueueDraw ();
	}
	// combo (end)

	private void on_check_runs_evolution_only_best_in_session_clicked (object o, EventArgs args)
	{
		runsEvolutionCalculate (false);
		drawingarea_runs_evolution.QueueDraw ();

		SqlitePreferences.Update(SqlitePreferences.RunsEvolutionOnlyBestInSession,
				check_runs_evolution_only_best_in_session.Active, false);
	}
	private void on_check_runs_evolution_show_time_clicked (object o, EventArgs args)
	{
		runsEvolutionCalculate (false);
		drawingarea_runs_evolution.QueueDraw ();

		SqlitePreferences.Update(SqlitePreferences.RunsEvolutionShowTime,
				check_runs_evolution_show_time.Active, false);
	}

	//if exerciseChanged, distances can change
	private void runsEvolutionCalculate (bool exerciseChanged)
	{
		if(currentPerson == null || currentSession == null ||
				drawingarea_runs_evolution == null || drawingarea_runs_evolution.Window == null) //it happens at start on click on analyze
			return;

		bool runsEvolutionJustCreated = false;
		if(runsEvolution == null)
		{
			runsEvolution = new RunsEvolution();
			runsEvolutionJustCreated = true;
		}

		runsEvolution.PassParameters(check_runs_evolution_show_time.Active, preferences.metersSecondsPreferred);

		string runType = comboSelectRunsEvolution.GetSelectedNameEnglish();

		// 1 get distance on the combo
		double distanceAtCombo = -1;
		if(exerciseChanged)
			distanceAtCombo = -1; //changing exercise will always select ----
		else if(combo_select_runs_evolution_distance != null && Util.IsNumber(UtilGtk.ComboGetActive(combo_select_runs_evolution_distance), true))
			distanceAtCombo = Convert.ToDouble(UtilGtk.ComboGetActive(combo_select_runs_evolution_distance));

		// 2 calculate (using distance)
		runsEvolution.distanceAtCombo = distanceAtCombo;
		runsEvolution.MouseReset ();
		runsEvolution.Calculate(currentPerson.UniqueID, runType, check_runs_evolution_only_best_in_session.Active);

		// 3 modify the distances combo, but only if exercise change or on creation of runsEvolution (first expose_event (draw))
		if(exerciseChanged || runsEvolutionJustCreated)
		{
			if(runsEvolution.distance_l.Count > 0)
			{
				if(runsEvolution.distance_l.Count > 1)
					runsEvolution.distance_l.Insert(0, "----");

				combo_select_runs_evolution_distance_follow_signals = false;

				UtilGtk.ComboUpdate(combo_select_runs_evolution_distance, runsEvolution.distance_l);

				combo_select_runs_evolution_distance.Active = 0;
				combo_select_runs_evolution_distance.Visible = true;

				combo_select_runs_evolution_distance_follow_signals = true;
			} else
				combo_select_runs_evolution_distance.Visible = false;
		}
	}

	//called just by QueueDraw
	private void runsEvolutionPlot ()
	{
		LogB.Information("runsEvolutionPlot");
		if(currentPerson == null || currentSession == null ||
				drawingarea_runs_evolution == null || drawingarea_runs_evolution.Window == null) //it happens at start on click on analyze
		{
			button_runs_evolution_save_image.Sensitive = false;
			LogB.Information("runsEvolutionPlot: exit early");
			return;
		}

		if(runsEvolution == null)
			runsEvolutionCalculate (true);

		string runType = comboSelectRunsEvolution.GetSelectedNameEnglish();

		if(runsEvolution.Point_l.Count == 0)
		{
			//constructor for showing blank screen with a message
			new RunsEvolutionGraph(drawingarea_runs_evolution, runType, preferences.fontType.ToString());
					//currentPerson.Name, runType, currentSession.DateShort);

			button_runs_evolution_save_image.Sensitive = false;
		} else {
			//regular constructor
			runsEvolutionGraph = new RunsEvolutionGraph(
					runsEvolution.Point_l,
					runsEvolution.Dates_l,
					runsEvolution.Slope,
					runsEvolution.Intercept,
					drawingarea_runs_evolution,
					currentPerson.Name, runType, currentSession.DateShort,
					check_runs_evolution_show_time.Active,
					preferences.metersSecondsPreferred,
					runsEvolution.MouseX,
					runsEvolution.MouseY);
			runsEvolutionGraph.Do(preferences.fontType.ToString());

			button_runs_evolution_save_image.Sensitive = true;
		}
		LogB.Information("runsEvolutionPlot: ended!");
	}
	private void on_drawingarea_runs_evolution_draw (object o, Gtk.DrawnArgs args)
	{
		runsEvolutionPlot ();
		//data is calculated on switch page (at notebook_capture_analyze) or on change person
	}

	private void on_drawingarea_runs_evolution_button_press_event (object o, ButtonPressEventArgs args)
	{
		//if there is no data and nothing to show, nothing to press, and also this is null
		// without runsEvolution.Point_l.Count == 0 condition, it will show graph/data on mouse (with data of previous person/graph)
		if(runsEvolutionGraph == null | runsEvolution.Point_l.Count == 0)
			return;

		LogB.Information("Button press done!");

		//redo the graph to delete previous rectangles of previous mouse clicks
		if (runsEvolution != null)
			runsEvolution.MouseSet (args.Event.X, args.Event.Y);

		drawingarea_runs_evolution.QueueDraw ();
	}

	private void on_button_runs_evolution_save_image_clicked (object o, EventArgs args)
	{
		checkFile(Constants.CheckFileOp.RUNS_EVOLUTION_SAVE_IMAGE);
	}

	private void on_button_runs_evolution_save_image_selected (string destination)
	{
		if(drawingarea_runs_evolution == null)
			return;

		LogB.Information("Saving");
		CairoUtil.GetScreenshotFromDrawingArea (drawingarea_runs_evolution, destination);
	}
	private void on_overwrite_file_runs_evolution_save_image_accepted (object o, EventArgs args)
	{
		on_button_runs_evolution_save_image_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), exportFileName);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}

	private void connectWidgetsJumpsRunsEvolution (Gtk.Builder builder)
	{
		drawingarea_jumps_evolution = (Gtk.DrawingArea) builder.GetObject ("drawingarea_jumps_evolution");
		image_tab_jumps_evolution = (Gtk.Image) builder.GetObject ("image_tab_jumps_evolution");
		image_jumps_evolution_save = (Gtk.Image) builder.GetObject ("image_jumps_evolution_save");
		hbox_combo_select_jumps_evolution = (Gtk.HBox) builder.GetObject ("hbox_combo_select_jumps_evolution");
		button_jumps_evolution_save_image = (Gtk.Button) builder.GetObject ("button_jumps_evolution_save_image");
		check_jumps_evolution_only_best_in_session = (Gtk.CheckButton) builder.GetObject ("check_jumps_evolution_only_best_in_session");

		drawingarea_runs_evolution = (Gtk.DrawingArea) builder.GetObject ("drawingarea_runs_evolution");
		image_tab_runs_evolution = (Gtk.Image) builder.GetObject ("image_tab_runs_evolution");
		image_runs_evolution_save = (Gtk.Image) builder.GetObject ("image_runs_evolution_save");
		image_runs_evolution_analyze_image_save = (Gtk.Image) builder.GetObject ("image_runs_evolution_analyze_image_save");
		hbox_combo_select_runs_evolution = (Gtk.HBox) builder.GetObject ("hbox_combo_select_runs_evolution");
		hbox_combo_select_runs_evolution_distance = (Gtk.HBox) builder.GetObject ("hbox_combo_select_runs_evolution_distance");
		button_runs_evolution_save_image = (Gtk.Button) builder.GetObject ("button_runs_evolution_save_image");
		check_runs_evolution_only_best_in_session = (Gtk.CheckButton) builder.GetObject ("check_runs_evolution_only_best_in_session");
		check_runs_evolution_show_time = (Gtk.CheckButton) builder.GetObject ("check_runs_evolution_show_time");
	}
}
