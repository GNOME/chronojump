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
//using Gdk;
//using Glade;
using Mono.Unix;

public partial class ChronoJumpWindow 
{
	Gtk.DrawingArea drawingarea_jumps_profile;
	Gtk.Image image_tab_jumps_profile;
	Gtk.Image image_jumps_profile_save;
	Gtk.Button button_jumps_profile_save_image;

	Gtk.Notebook notebook_jumps_profile;
	Gtk.HBox hbox_jumps_profile_jumps_done;
	Gtk.Image image_jumps_profile_sj_yes;
	Gtk.Image image_jumps_profile_sj_no;
	Gtk.Image image_jumps_profile_sjl_yes;
	Gtk.Image image_jumps_profile_sjl_no;
	Gtk.Image image_jumps_profile_cmj_yes;
	Gtk.Image image_jumps_profile_cmj_no;
	Gtk.Image image_jumps_profile_abk_yes;
	Gtk.Image image_jumps_profile_abk_no;
	Gtk.Image image_jumps_profile_dja_yes;
	Gtk.Image image_jumps_profile_dja_no;
	Gtk.Image image_jumps_profile_sjl_help;


	JumpsProfile jumpsProfile;

	private void jumpsProfileCalculate ()
	{
		if(currentPerson == null || currentSession == null ||
				drawingarea_jumps_profile == null || drawingarea_jumps_profile.Window == null) //it happens at start on click on analyze
			return;

		if(jumpsProfile == null)
			jumpsProfile = new JumpsProfile();

		//jumpsProfile.MouseReset ();

		jumpsProfile.Calculate(currentPerson.UniqueID, currentSession.UniqueID);

		if(jumpsProfile.AllJumpsDone)
		{
			hbox_jumps_profile_jumps_done.Visible = false;
			//button_jumps_profile_save_image.Sensitive = true;
		} else {
			hbox_jumps_profile_jumps_done.Visible = true;
			JumpsProfileGraph.ShowDoneJumps (jumpsProfile.JumpsDone,
					image_jumps_profile_sj_yes, image_jumps_profile_sj_no,
					image_jumps_profile_sjl_yes, image_jumps_profile_sjl_no,
					image_jumps_profile_cmj_yes, image_jumps_profile_cmj_no,
					image_jumps_profile_abk_yes, image_jumps_profile_abk_no,
					image_jumps_profile_dja_yes, image_jumps_profile_dja_no
					);
			//button_jumps_profile_save_image.Sensitive = false;
		}
		button_jumps_profile_save_image.Sensitive = true; //allow to save image without having all 5 indexes
	}

	//called just by QueueDraw
	private void jumpsProfilePlot ()
	{
		if(currentPerson == null || currentSession == null ||
				drawingarea_jumps_profile == null || drawingarea_jumps_profile.Window == null) //it happens at start on click on analyze
		{
			button_jumps_profile_save_image.Sensitive = false;
			return;
		}

		if(jumpsProfile == null)
			jumpsProfileCalculate ();

		JumpsProfileGraph.Do (jumpsProfile.JumpsDone, jumpsProfile.ErrorSJl,
				jumpsProfile.GetIndexes(), drawingarea_jumps_profile,
				currentPerson.Name, currentSession.DateShort, preferences.fontType.ToString());
	}
	private void on_drawingarea_jumps_profile_cairo_draw (object o, Gtk.DrawnArgs args) 
	{
		jumpsProfilePlot ();
		//data is calculated on switch page (at notebook_capture_analyze) or on change person
	}

	private void on_button_jumps_profile_save_image_clicked (object o, EventArgs args)
	{
		checkFile(Constants.CheckFileOp.JUMPS_PROFILE_SAVE_IMAGE);
	}

	private void on_button_jumps_profile_save_image_selected (string destination)
	{
		if(drawingarea_jumps_profile == null)
			return;

		LogB.Information("Saving");
		CairoUtil.GetScreenshotFromDrawingArea (drawingarea_jumps_profile, destination);
	}
	private void on_overwrite_file_jumps_profile_save_image_accepted (object o, EventArgs args)
	{
		on_button_jumps_profile_save_image_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), exportFileName);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}

	private void on_button_jumps_profile_sjl_help_clicked (object o, EventArgs args)
	{
		new DialogMessage (Constants.MessageTypes.INFO, Catalog.GetString ("Need at least one SJl jump with 100% body weight or two SJl jumps with different weights."));
	}

	private void connectWidgetsJumpsProfile (Gtk.Builder builder)
	{
		drawingarea_jumps_profile = (Gtk.DrawingArea) builder.GetObject ("drawingarea_jumps_profile");
		image_tab_jumps_profile = (Gtk.Image) builder.GetObject ("image_tab_jumps_profile");
		image_jumps_profile_save = (Gtk.Image) builder.GetObject ("image_jumps_profile_save");
		button_jumps_profile_save_image = (Gtk.Button) builder.GetObject ("button_jumps_profile_save_image");

		notebook_jumps_profile = (Gtk.Notebook) builder.GetObject ("notebook_jumps_profile");
		hbox_jumps_profile_jumps_done = (Gtk.HBox) builder.GetObject ("hbox_jumps_profile_jumps_done");
		image_jumps_profile_sj_yes = (Gtk.Image) builder.GetObject ("image_jumps_profile_sj_yes");
		image_jumps_profile_sj_no = (Gtk.Image) builder.GetObject ("image_jumps_profile_sj_no");
		image_jumps_profile_sjl_yes = (Gtk.Image) builder.GetObject ("image_jumps_profile_sjl_yes");
		image_jumps_profile_sjl_no = (Gtk.Image) builder.GetObject ("image_jumps_profile_sjl_no");
		image_jumps_profile_cmj_yes = (Gtk.Image) builder.GetObject ("image_jumps_profile_cmj_yes");
		image_jumps_profile_cmj_no = (Gtk.Image) builder.GetObject ("image_jumps_profile_cmj_no");
		image_jumps_profile_abk_yes = (Gtk.Image) builder.GetObject ("image_jumps_profile_abk_yes");
		image_jumps_profile_abk_no = (Gtk.Image) builder.GetObject ("image_jumps_profile_abk_no");
		image_jumps_profile_dja_yes = (Gtk.Image) builder.GetObject ("image_jumps_profile_dja_yes");
		image_jumps_profile_dja_no = (Gtk.Image) builder.GetObject ("image_jumps_profile_dja_no");
		image_jumps_profile_sjl_help = (Gtk.Image) builder.GetObject ("image_jumps_profile_sjl_help");
	}
}
