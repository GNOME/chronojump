
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
 * Copyright (C) 2020   Xavier de Blas <xaviblas@gmail.com> 
 */


using System;
using Gtk;
using Glade;
//using Mono.Unix;
using System.Collections.Generic; //List<T> 

public partial class ChronoJumpWindow 
{
	[Widget] Gtk.DrawingArea drawingarea_jumps_rj_fatigue;
	[Widget] Gtk.Button button_jumps_rj_fatigue_save_image;
	[Widget] Gtk.Image image_jumps_rj_fatigue_save;
	[Widget] Gtk.Image image_jumps_rj_fatigue_image_save;
	[Widget] Gtk.HBox hbox_combo_select_jumps_rj_fatigue;
	[Widget] Gtk.ComboBox combo_select_jumps_rj_fatigue;

	JumpsRjFatigue jumpsRjFatigue;
	JumpsRjFatigueGraph jumpsRjFatigueGraph;
	CjComboSelectJumpsRj comboSelectJumpsRjFatigue;

	/* OLD, first test
	private void on_jumps_rj_analyze_fatigue_clicked (object o, EventArgs args)
	{
		if(currentPerson == null || currentSession == null)
			return;

		List<JumpRj> l = SqliteJumpRj.SelectJumps(false, currentSession.UniqueID, currentPerson.UniqueID, "");

		LogB.Information("printing list of jumps...");
		foreach(JumpRj jrj in l)
			LogB.Information(jrj.ToString());
	}
	*/

	// combo (start)
	private void createComboSelectJumpsRjFatigue(bool create)
	{
		if(create)
		{
			comboSelectJumpsRjFatigue = new CjComboSelectJumpsRj(combo_select_jumps_rj_fatigue, hbox_combo_select_jumps_rj_fatigue);
			combo_select_jumps_rj_fatigue = comboSelectJumpsRjFatigue.Combo;
			combo_select_jumps_rj_fatigue.Changed += new EventHandler (on_combo_select_jumps_rj_fatigue_changed);
		} else {
			comboSelectJumpsRjFatigue.Fill();
			combo_select_jumps_rj_fatigue = comboSelectJumpsRjFatigue.Combo;
		}
		combo_select_jumps_rj_fatigue.Sensitive = true;
	}
	private void on_combo_select_jumps_rj_fatigue_changed(object o, EventArgs args)
	{
		ComboBox combo = o as ComboBox;
		if (o == null)
			return;

		jumpsRjFatigueDo(true);
	}
	// combo (end)

	private void jumpsRjFatigueDo (bool calculateData)
	{
		if(currentPerson == null || currentSession == null ||
				drawingarea_jumps_rj_fatigue == null || drawingarea_jumps_rj_fatigue.GdkWindow == null) //it happens at start on click on analyze
		{
			button_jumps_rj_fatigue_save_image.Sensitive = false;
			return;
		}

		if(jumpsRjFatigue == null) {
			jumpsRjFatigue = new JumpsRjFatigue();
			calculateData = true;
		}

//		string jumpType = comboSelectJumpsRjFatigue.GetSelectedNameEnglish();
		string jumpType = "";

		if(calculateData)
			jumpsRjFatigue.Calculate(currentSession.UniqueID, currentPerson.UniqueID, jumpType);

		if(jumpsRjFatigue.Point_l.Count == 0)
		{
			//constructor for showing blank screen with a message
			new JumpsRjFatigueGraph(drawingarea_jumps_rj_fatigue, jumpType);
					//currentPerson.Name, jumpType, currentSession.DateShort);

			button_jumps_rj_fatigue_save_image.Sensitive = false;

		} else {
			//regular constructor
			jumpsRjFatigueGraph = new JumpsRjFatigueGraph(
					jumpsRjFatigue.Point_l,
					jumpsRjFatigue.Slope,
					jumpsRjFatigue.Intercept,
					drawingarea_jumps_rj_fatigue,
					currentPerson.Name, jumpType, currentSession.DateShort);
			jumpsRjFatigueGraph.Do();

			button_jumps_rj_fatigue_save_image.Sensitive = true;
		}
	}

	private void on_drawingarea_jumps_rj_fatigue_expose_event (object o, ExposeEventArgs args)
	{
		//needed to have mouse clicks at: on_drawingarea_jumps_rj_fatigue_button_press_event ()
		drawingarea_jumps_rj_fatigue.AddEvents((int) (Gdk.EventMask.ButtonPressMask | Gdk.EventMask.ButtonReleaseMask));

		jumpsRjFatigueDo(false); //do not calculate data
		//data is calculated on switch page (at notebook_capture_analyze) or on change person
	}

	private void on_drawingarea_jumps_rj_fatigue_button_press_event (object o, ButtonPressEventArgs args)
	{
		/*
		//if there is no data and nothing to show, nothing to press, and also this is null
		if(jumpsRjFatigueGraph == null)
			return;

		LogB.Information("Button press done!");

		//redo the graph to delete previous rectangles of previous mouse clicks
		jumpsRjFatigueGraph.Do();
		LogB.Information(string.Format("Mouse X: {0}; Mouse Y: {1}", args.Event.X, args.Event.Y));
		jumpsRjFatigueGraph.CalculateAndWriteRealXY(args.Event.X, args.Event.Y);
		*/
	}



}
