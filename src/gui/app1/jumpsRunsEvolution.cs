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
 * Copyright (C) 2004-2021   Xavier de Blas <xaviblas@gmail.com>
 */


using System;
using Gtk;
using Glade;
using Mono.Unix;

public partial class ChronoJumpWindow 
{
	/*
	   ---- jumpsEvolution ----
	   */

	[Widget] Gtk.DrawingArea drawingarea_jumps_evolution;
	[Widget] Gtk.Image image_tab_jumps_evolution;
	[Widget] Gtk.Image image_jumps_evolution_save;
	[Widget] Gtk.HBox hbox_combo_select_jumps_evolution;
	[Widget] Gtk.ComboBox combo_select_jumps_evolution;
	[Widget] Gtk.Button button_jumps_evolution_save_image;
	[Widget] Gtk.CheckButton check_jumps_evolution_only_best_in_session;

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
		ComboBox combo = o as ComboBox;
		if (o == null)
			return;

		jumpsEvolutionDo(true);
	}
	// combo (end)

	private void on_check_jumps_evolution_only_best_in_session_clicked (object o, EventArgs args)
	{
		jumpsEvolutionDo(true);

		SqlitePreferences.Update(SqlitePreferences.JumpsEvolutionOnlyBestInSession,
				check_jumps_evolution_only_best_in_session.Active, false);
	}

	private void jumpsEvolutionDo (bool calculateData)
	{
		// 1) exit, if problems
		if(currentPerson == null || currentSession == null ||
				drawingarea_jumps_evolution == null || drawingarea_jumps_evolution.GdkWindow == null) //it happens at start on click on analyze
		{
			button_jumps_evolution_save_image.Sensitive = false;
			return;
		}

		// 2) create jumpsEvolution, if needed
		if(jumpsEvolution == null) {
			jumpsEvolution = new JumpsEvolution();
			calculateData = true;
		}

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

		// 5) calculateData
		if(calculateData)
			jumpsEvolution.Calculate(currentPerson.UniqueID, jumpType, check_jumps_evolution_only_best_in_session.Active);

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
					jumpsEvolution.Slope,
					jumpsEvolution.Intercept,
					drawingarea_jumps_evolution,
					currentPerson.Name, jumpType, currentSession.DateShort);
			jumpsEvolutionGraph.Do(preferences.fontType.ToString());

			button_jumps_evolution_save_image.Sensitive = true;
		}
	}
	private void on_drawingarea_jumps_evolution_expose_event (object o, ExposeEventArgs args) 
	{
		//needed to have mouse clicks at: on_drawingarea_jumps_evolution_button_press_event ()
//		drawingarea_jumps_evolution.AddEvents((int) (Gdk.EventMask.ButtonPressMask | Gdk.EventMask.ButtonReleaseMask));
		drawingarea_jumps_evolution.AddEvents((int) Gdk.EventMask.ButtonPressMask);

		jumpsEvolutionDo(false); //do not calculate data
		//data is calculated on switch page (at notebook_capture_analyze) or on change person
	}

	private void on_drawingarea_jumps_evolution_button_press_event (object o, ButtonPressEventArgs args)
	{
		//if there is no data and nothing to show, nothing to press, and also this is null
		if(jumpsEvolutionGraph == null)
			return;

		LogB.Information("Button press done!");

		//redo the graph to delete previous rectangles of previous mouse clicks
		jumpsEvolutionGraph.Do(preferences.fontType.ToString());
		LogB.Information(string.Format("Mouse X: {0}; Mouse Y: {1}", args.Event.X, args.Event.Y));
		jumpsEvolutionGraph.CalculateAndWriteRealXY(args.Event.X, args.Event.Y);
	}

	private void on_button_jumps_evolution_save_image_clicked (object o, EventArgs args)
	{
		checkFile(Constants.CheckFileOp.JUMPS_EVOLUTION_SAVE_IMAGE);
	}

	private void on_button_jumps_evolution_save_image_selected (string destination)
	{
		if(drawingarea_jumps_evolution == null)
			return;

		Gdk.Pixbuf pixbuf = Gdk.Pixbuf.FromDrawable(drawingarea_jumps_evolution.GdkWindow, Gdk.Colormap.System,
				0, 0, 0, 0,
				UtilGtk.WidgetWidth(drawingarea_jumps_evolution),
				UtilGtk.WidgetHeight(drawingarea_jumps_evolution) );

		LogB.Information("Saving");
		pixbuf.Save(destination,"png");
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

	[Widget] Gtk.DrawingArea drawingarea_runs_evolution;
	[Widget] Gtk.Image image_tab_runs_evolution;
	[Widget] Gtk.Image image_runs_evolution_save;
	[Widget] Gtk.Image image_runs_evolution_analyze_image_save;
	[Widget] Gtk.HBox hbox_combo_select_runs_evolution;
	[Widget] Gtk.ComboBox combo_select_runs_evolution;
	[Widget] Gtk.HBox hbox_combo_select_runs_evolution_distance;
	[Widget] Gtk.ComboBox combo_select_runs_evolution_distance;
	[Widget] Gtk.Button button_runs_evolution_save_image;
	[Widget] Gtk.CheckButton check_runs_evolution_only_best_in_session;
	[Widget] Gtk.CheckButton check_runs_evolution_show_time;

	RunsEvolution runsEvolution;
	RunsEvolutionGraph runsEvolutionGraph;
	CjComboSelectRuns comboSelectRunsEvolution;

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
		ComboBox combo = o as ComboBox;
		if (o == null)
			return;

		runsEvolutionDo(true, true);
	}
	// combo (end)

	// combo (start)
	private bool combo_select_runs_evolution_distance_follow_signals;
	private void createComboSelectRunsEvolutionDistance()
	{
		combo_select_runs_evolution_distance = ComboBox.NewText();
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

		ComboBox combo = o as ComboBox;
		if (o == null)
			return;

		runsEvolutionDo(false, true);
	}
	// combo (end)

	private void on_check_runs_evolution_only_best_in_session_clicked (object o, EventArgs args)
	{
		runsEvolutionDo(false, true);

		SqlitePreferences.Update(SqlitePreferences.RunsEvolutionOnlyBestInSession,
				check_runs_evolution_only_best_in_session.Active, false);
	}
	private void on_check_runs_evolution_show_time_clicked (object o, EventArgs args)
	{
		runsEvolutionDo(false, true);

		SqlitePreferences.Update(SqlitePreferences.RunsEvolutionShowTime,
				check_runs_evolution_show_time.Active, false);
	}

	//if exerciseChanged, distances can change
	private void runsEvolutionDo (bool exerciseChanged, bool calculateData)
	{
		LogB.Information("runsEvolutionDo, calculateData: " + calculateData.ToString());
		if(currentPerson == null || currentSession == null ||
				drawingarea_runs_evolution == null || drawingarea_runs_evolution.GdkWindow == null) //it happens at start on click on analyze
		{
			button_runs_evolution_save_image.Sensitive = false;
			LogB.Information("runsEvolutionDo: exit early");
			return;
		}

		bool runsEvolutionJustCreated = false;
		if(runsEvolution == null) {
			runsEvolution = new RunsEvolution();
			calculateData = true;
			runsEvolutionJustCreated = true;
		}

		string runType = comboSelectRunsEvolution.GetSelectedNameEnglish();

		if(calculateData)
		{
			runsEvolution.PassParameters(check_runs_evolution_show_time.Active, preferences.metersSecondsPreferred);

			// 1 get distance on the combo
			double distanceAtCombo = -1;
			if(exerciseChanged)
				distanceAtCombo = -1; //changing exercise will always select ----
			else if(combo_select_runs_evolution_distance != null && Util.IsNumber(UtilGtk.ComboGetActive(combo_select_runs_evolution_distance), true))
				distanceAtCombo = Convert.ToDouble(UtilGtk.ComboGetActive(combo_select_runs_evolution_distance));

			// 2 calculate (using distance)
			runsEvolution.distanceAtCombo = distanceAtCombo;
			runsEvolution.Calculate(currentPerson.UniqueID, runType, check_runs_evolution_only_best_in_session.Active);

			// 3 modify the distances combo, but only if exercise change or on creation of runsEvolution (first expose_event)
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
					runsEvolution.Slope,
					runsEvolution.Intercept,
					drawingarea_runs_evolution,
					currentPerson.Name, runType, currentSession.DateShort,
					check_runs_evolution_show_time.Active,
					preferences.metersSecondsPreferred);
			runsEvolutionGraph.Do(preferences.fontType.ToString());

			button_runs_evolution_save_image.Sensitive = true;
		}
		LogB.Information("runsEvolutionDo: ended!");
	}
	private void on_drawingarea_runs_evolution_expose_event (object o, ExposeEventArgs args)
	{
		//needed to have mouse clicks at: on_drawingarea_runs_evolution_button_press_event ()
//		drawingarea_runs_evolution.AddEvents((int) (Gdk.EventMask.ButtonPressMask | Gdk.EventMask.ButtonReleaseMask));
		drawingarea_runs_evolution.AddEvents((int) Gdk.EventMask.ButtonPressMask);

		runsEvolutionDo(false, false); //do not calculate data
		//data is calculated on switch page (at notebook_capture_analyze) or on change person
	}

	private void on_drawingarea_runs_evolution_button_press_event (object o, ButtonPressEventArgs args)
	{
		//if there is no data and nothing to show, nothing to press, and also this is null
		if(runsEvolutionGraph == null)
			return;

		LogB.Information("Button press done!");

		//redo the graph to delete previous rectangles of previous mouse clicks
		runsEvolutionGraph.Do(preferences.fontType.ToString());
		LogB.Information(string.Format("Mouse X: {0}; Mouse Y: {1}", args.Event.X, args.Event.Y));
		runsEvolutionGraph.CalculateAndWriteRealXY(args.Event.X, args.Event.Y);
	}

	private void on_button_runs_evolution_save_image_clicked (object o, EventArgs args)
	{
		checkFile(Constants.CheckFileOp.RUNS_EVOLUTION_SAVE_IMAGE);
	}

	private void on_button_runs_evolution_save_image_selected (string destination)
	{
		if(drawingarea_runs_evolution == null)
			return;

		Gdk.Pixbuf pixbuf = Gdk.Pixbuf.FromDrawable(drawingarea_runs_evolution.GdkWindow, Gdk.Colormap.System,
				0, 0, 0, 0,
				UtilGtk.WidgetWidth(drawingarea_runs_evolution),
				UtilGtk.WidgetHeight(drawingarea_runs_evolution) );

		LogB.Information("Saving");
		pixbuf.Save(destination,"png");
	}
	private void on_overwrite_file_runs_evolution_save_image_accepted (object o, EventArgs args)
	{
		on_button_runs_evolution_save_image_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), exportFileName);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}

}
