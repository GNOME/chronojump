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
 * Copyright (C) 2004-2020   Xavier de Blas <xaviblas@gmail.com> 
 */


using System;
using Gtk;
using Glade;
using Mono.Unix;

public partial class ChronoJumpWindow 
{
	[Widget] Gtk.DrawingArea drawingarea_jumps_dj_optimal_fall;
	[Widget] Gtk.Image image_tab_jumps_dj_optimal_fall;
	[Widget] Gtk.Image image_jumps_dj_optimal_fall_save;
	[Widget] Gtk.HBox hbox_combo_select_jumps_dj_optimal_fall;
	[Widget] Gtk.ComboBox combo_select_jumps_dj_optimal_fall;
	[Widget] Gtk.Button button_jumps_dj_optimal_fall_save_image;

	JumpsDjOptimalFall jumpsDjOptimalFall;
	JumpsDjOptimalFallGraph jumpsDjOptimalFallGraph;
	CjComboSelectJumps comboSelectJumpsDjOptimalFall;

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
		ComboBox combo = o as ComboBox;
		if (o == null)
			return;

		jumpsDjOptimalFallDo(true);
	}
	// combo (end)

	private void jumpsDjOptimalFallDo (bool calculateData)
	{
		if(currentPerson == null || currentSession == null ||
				drawingarea_jumps_dj_optimal_fall == null || drawingarea_jumps_dj_optimal_fall.GdkWindow == null) //it happens at start on click on analyze
		{
			button_jumps_dj_optimal_fall_save_image.Sensitive = false;
			return;
		}

		if(jumpsDjOptimalFall == null) {
			jumpsDjOptimalFall = new JumpsDjOptimalFall();
			calculateData = true;
		}

		string jumpType = UtilGtk.ComboGetActive(combo_select_jumps_dj_optimal_fall);

		if(calculateData)
			jumpsDjOptimalFall.Calculate(currentPerson.UniqueID, currentSession.UniqueID, jumpType);

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
					currentPerson.Name, jumpType, currentSession.DateShort);
			jumpsDjOptimalFallGraph.Do(preferences.fontType.ToString());

			button_jumps_dj_optimal_fall_save_image.Sensitive = true;
		}
	}
	private void on_drawingarea_jumps_dj_optimal_fall_expose_event (object o, ExposeEventArgs args) 
	{
		/*
		   moved to creation:

		//needed to have mouse clicks at: on_drawingarea_jumps_weight_fv_profile_button_press_event ()
		drawingarea_jumps_dj_optimal_fall.AddEvents((int) (Gdk.EventMask.ButtonPressMask | Gdk.EventMask.ButtonReleaseMask));
		*/

		jumpsDjOptimalFallDo(false); //do not calculate data
		//data is calculated on switch page (at notebook_capture_analyze) or on change person
	}

	private void on_drawingarea_jumps_dj_optimal_fall_button_press_event (object o, ButtonPressEventArgs args)
	{
		//if there is no data and nothing to show, nothing to press, and also this is null
		if(jumpsDjOptimalFallGraph == null)
			return;

		LogB.Information("Button press done!");

		//redo the graph to delete previous rectangles of previous mouse clicks
		jumpsDjOptimalFallGraph.Do(preferences.fontType.ToString());
		LogB.Information(string.Format("Mouse X: {0}; Mouse Y: {1}", args.Event.X, args.Event.Y));
		jumpsDjOptimalFallGraph.CalculateAndWriteRealXY(args.Event.X, args.Event.Y);
	}

	private void on_button_jumps_dj_optimal_fall_save_image_clicked (object o, EventArgs args)
	{
		checkFile(Constants.CheckFileOp.JUMPS_DJ_OPTIMAL_FALL_SAVE_IMAGE);
	}

	private void on_button_jumps_dj_optimal_fall_save_image_selected (string destination)
	{
		if(drawingarea_jumps_dj_optimal_fall == null)
			return;

		Gdk.Pixbuf pixbuf = Gdk.Pixbuf.FromDrawable(drawingarea_jumps_dj_optimal_fall.GdkWindow, Gdk.Colormap.System,
				0, 0, 0, 0,
				UtilGtk.WidgetWidth(drawingarea_jumps_dj_optimal_fall),
				UtilGtk.WidgetHeight(drawingarea_jumps_dj_optimal_fall) );

		LogB.Information("Saving");
		pixbuf.Save(destination,"png");
	}
	private void on_overwrite_file_jumps_dj_optimal_fall_save_image_accepted (object o, EventArgs args)
	{
		on_button_jumps_dj_optimal_fall_save_image_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), exportFileName);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}

}
