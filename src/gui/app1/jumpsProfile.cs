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
//using Gdk;
using Glade;
using Mono.Unix;

public partial class ChronoJumpWindow 
{
	[Widget] Gtk.DrawingArea drawingarea_jumps_profile;
	[Widget] Gtk.Image image_tab_jumps_profile;
	[Widget] Gtk.Image image_jumps_profile_save;
	[Widget] Gtk.Button button_jumps_profile_save_image;

	[Widget] Gtk.HBox hbox_jumps_profile_jumps_done;
	[Widget] Gtk.Image image_jumps_profile_sj_yes;
	[Widget] Gtk.Image image_jumps_profile_sj_no;
	[Widget] Gtk.Image image_jumps_profile_sjl_yes;
	[Widget] Gtk.Image image_jumps_profile_sjl_no;
	[Widget] Gtk.Image image_jumps_profile_cmj_yes;
	[Widget] Gtk.Image image_jumps_profile_cmj_no;
	[Widget] Gtk.Image image_jumps_profile_abk_yes;
	[Widget] Gtk.Image image_jumps_profile_abk_no;
	[Widget] Gtk.Image image_jumps_profile_dja_yes;
	[Widget] Gtk.Image image_jumps_profile_dja_no;


	JumpsProfile jumpsProfile;

	private void jumpsProfileDo (bool calculateData)
	{
		if(currentPerson == null || currentSession == null ||
				drawingarea_jumps_profile == null || drawingarea_jumps_profile.GdkWindow == null) //it happens at start on click on analyze
		{
			button_jumps_profile_save_image.Sensitive = false;
			return;
		}
		
		if(jumpsProfile == null) {
			jumpsProfile = new JumpsProfile();
			calculateData = true;
		}

		if(calculateData)
		{
			jumpsProfile.Calculate(currentPerson.UniqueID, currentSession.UniqueID);

			if(jumpsProfile.AllJumpsDone)
			{
				hbox_jumps_profile_jumps_done.Visible = false;
				//button_jumps_profile_save_image.Sensitive = true;
			} else {
				hbox_jumps_profile_jumps_done.Visible = true;
				JumpsProfileGraph.ShowDoneJumps(jumpsProfile.JumpsDone,
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

		JumpsProfileGraph.Do(jumpsProfile.GetIndexes(), drawingarea_jumps_profile,
				currentPerson.Name, currentSession.DateShort, preferences.fontType.ToString());
	}
	private void on_drawingarea_jumps_profile_expose_event (object o, ExposeEventArgs args) 
	{
		jumpsProfileDo(false); //do not calculate data
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

		Gdk.Pixbuf pixbuf = Gdk.Pixbuf.FromDrawable(drawingarea_jumps_profile.GdkWindow, Gdk.Colormap.System,
				0, 0, 0, 0,
				UtilGtk.WidgetWidth(drawingarea_jumps_profile),
				UtilGtk.WidgetHeight(drawingarea_jumps_profile) );

		LogB.Information("Saving");
		pixbuf.Save(destination,"png");
	}
	private void on_overwrite_file_jumps_profile_save_image_accepted (object o, EventArgs args)
	{
		on_button_jumps_profile_save_image_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), exportFileName);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}

}
