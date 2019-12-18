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
 * Copyright (C) 2004-2019   Xavier de Blas <xaviblas@gmail.com> 
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

	JumpsDjOptimalFall jumpsDjOptimalFall;

	private void jumpsDjOptimalFallDo (bool calculateData)
	{
		if(currentPerson == null || currentSession == null)
			return;
		
		if(jumpsDjOptimalFall == null) {
			jumpsDjOptimalFall = new JumpsDjOptimalFall();
			calculateData = true;
		}

		if(calculateData)
			jumpsDjOptimalFall.Calculate(currentPerson.UniqueID, currentSession.UniqueID);

		if(jumpsDjOptimalFall.Point_l.Count > 0)
			JumpsDjOptimalFallGraph.Do(
					jumpsDjOptimalFall.Point_l,
					jumpsDjOptimalFall.Coefs,
					jumpsDjOptimalFall.XatMaxY, //model
					jumpsDjOptimalFall.GetMaxValue(),
					drawingarea_jumps_dj_optimal_fall,
					currentPerson.Name, currentSession.DateShort);
		//TODO: if not, just blank screen
	}
	private void on_drawingarea_jumps_dj_optimal_fall_expose_event (object o, ExposeEventArgs args) 
	{
		jumpsDjOptimalFallDo(false); //do not calculate data
		//data is calculated on switch page (at notebook_capture_analyze) or on change person
	}

	private void on_button_jumps_dj_optimal_fall_save_image_clicked (object o, EventArgs args)
	{
		checkFile(Constants.CheckFileOp.JUMPS_PROFILE_SAVE_IMAGE);
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
