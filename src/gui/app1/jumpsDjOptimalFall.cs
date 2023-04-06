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
	// at glade ---->
	Gtk.DrawingArea drawingarea_jumps_dj_optimal_fall;
	Gtk.Image image_tab_jumps_dj_optimal_fall;
	Gtk.Image image_jumps_dj_optimal_fall_save;
	Gtk.HBox hbox_combo_select_jumps_dj_optimal_fall;
	Gtk.Button button_jumps_dj_optimal_fall_save_image;
	// <---- at glade

	JumpsDjOptimalFall jumpsDjOptimalFall;
	JumpsDjOptimalFallGraph jumpsDjOptimalFallGraph;
	CjComboSelectJumps comboSelectJumpsDjOptimalFall;
	
	Gtk.ComboBoxText combo_select_jumps_dj_optimal_fall;

	// combo (start)
	private void createComboSelectJumpsDjOptimalFall(bool create)
	{
		if(create)
		{
			comboSelectJumpsDjOptimalFall = new CjComboSelectJumps(combo_select_jumps_dj_optimal_fall, hbox_combo_select_jumps_dj_optimal_fall, true);
			combo_select_jumps_dj_optimal_fall = comboSelectJumpsDjOptimalFall.Combo;
			combo_select_jumps_dj_optimal_fall.Changed += new EventHandler (on_combo_select_jumps_dj_optimal_fall_changed);
		} else {
			comboSelectJumpsDjOptimalFall.Fill();
			combo_select_jumps_dj_optimal_fall = comboSelectJumpsDjOptimalFall.Combo;
		}
		combo_select_jumps_dj_optimal_fall.Sensitive = true;
	}
	private void on_combo_select_jumps_dj_optimal_fall_changed(object o, EventArgs args)
	{
		//ComboBoxText combo = o as ComboboxText;
		if (o == null)
			return;

		jumpsDjOptimalFallCalculate ();
		drawingarea_jumps_dj_optimal_fall.QueueDraw ();
	}
	// combo (end)

	private void jumpsDjOptimalFallCalculate ()
	{
		if(currentPerson == null || currentSession == null ||
				drawingarea_jumps_dj_optimal_fall == null || drawingarea_jumps_dj_optimal_fall.Window == null) //it happens at start on click on analyze
			return;

		if(jumpsDjOptimalFall == null)
			jumpsDjOptimalFall = new JumpsDjOptimalFall();

		string jumpType = UtilGtk.ComboGetActive(combo_select_jumps_dj_optimal_fall);
		jumpsDjOptimalFall.Calculate (currentPerson.UniqueID, currentSession.UniqueID, jumpType);
	}

	//called just by QueueDraw
	private void jumpsDjOptimalFallPlot ()
	{
		if(currentPerson == null || currentSession == null ||
				drawingarea_jumps_dj_optimal_fall == null || drawingarea_jumps_dj_optimal_fall.Window == null) //it happens at start on click on analyze
		{
			button_jumps_dj_optimal_fall_save_image.Sensitive = false;
			return;
		}

		if(jumpsDjOptimalFall == null)
			jumpsDjOptimalFallCalculate ();

		string jumpType = UtilGtk.ComboGetActive(combo_select_jumps_dj_optimal_fall);

		if(jumpsDjOptimalFall.Point_l.Count == 0)
		{
			//constructor for showing blank screen with a message
			new JumpsDjOptimalFallGraph(drawingarea_jumps_dj_optimal_fall, jumpType, preferences.fontType.ToString());
					//currentPerson.Name, jumpType, currentSession.DateShort);

			button_jumps_dj_optimal_fall_save_image.Sensitive = false;

		} else {
			//regular constructor
			jumpsDjOptimalFallGraph = new JumpsDjOptimalFallGraph(
					jumpsDjOptimalFall.Point_l,
					jumpsDjOptimalFall.Coefs,
					jumpsDjOptimalFall.ParaboleType, //model
					jumpsDjOptimalFall.XatMaxY, //model
					jumpsDjOptimalFall.GetMaxValue(),
					drawingarea_jumps_dj_optimal_fall,
					currentPerson.Name, jumpType, currentSession.DateShort,
					jumpsDjOptimalFall.MouseX,
					jumpsDjOptimalFall.MouseY);
			jumpsDjOptimalFallGraph.Do(preferences.fontType.ToString());

			button_jumps_dj_optimal_fall_save_image.Sensitive = true;
		}
	}
	private void on_drawingarea_jumps_dj_optimal_fall_cairo_draw (object o, Gtk.DrawnArgs args) 
	{
		jumpsDjOptimalFallPlot ();
		//data is calculated on switch page (at notebook_capture_analyze) or on change person
	}

	private void on_drawingarea_jumps_dj_optimal_fall_button_press_event (object o, ButtonPressEventArgs args)
	{
		//if there is no data and nothing to show, nothing to press, and also this is null
		if(jumpsDjOptimalFallGraph == null || jumpsDjOptimalFall.Point_l.Count == 0)
			return;

		LogB.Information("Button press done!");

		//redo the graph to delete previous rectangles of previous mouse clicks
		if (jumpsDjOptimalFall != null)
			jumpsDjOptimalFall.MouseSet (args.Event.X, args.Event.Y);

		drawingarea_jumps_dj_optimal_fall.QueueDraw ();
	}

	private void on_button_jumps_dj_optimal_fall_save_image_clicked (object o, EventArgs args)
	{
		checkFile(Constants.CheckFileOp.JUMPS_DJ_OPTIMAL_FALL_SAVE_IMAGE);
	}

	private void on_button_jumps_dj_optimal_fall_save_image_selected (string destination)
	{
		if(drawingarea_jumps_dj_optimal_fall == null)
			return;

		LogB.Information("Saving");
		CairoUtil.GetScreenshotFromDrawingArea (drawingarea_jumps_dj_optimal_fall, destination);
	}
	private void on_overwrite_file_jumps_dj_optimal_fall_save_image_accepted (object o, EventArgs args)
	{
		on_button_jumps_dj_optimal_fall_save_image_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), exportFileName);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}

	private void connectWidgetsJumpDjOptimalFall (Gtk.Builder builder)
	{
		drawingarea_jumps_dj_optimal_fall = (Gtk.DrawingArea) builder.GetObject ("drawingarea_jumps_dj_optimal_fall");
		image_tab_jumps_dj_optimal_fall = (Gtk.Image) builder.GetObject ("image_tab_jumps_dj_optimal_fall");
		image_jumps_dj_optimal_fall_save = (Gtk.Image) builder.GetObject ("image_jumps_dj_optimal_fall_save");
		hbox_combo_select_jumps_dj_optimal_fall = (Gtk.HBox) builder.GetObject ("hbox_combo_select_jumps_dj_optimal_fall");
		button_jumps_dj_optimal_fall_save_image = (Gtk.Button) builder.GetObject ("button_jumps_dj_optimal_fall_save_image");
	}
}
