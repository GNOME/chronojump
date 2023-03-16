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
 * Copyright (C) 2020-2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Gtk;
using Gdk;
//using Glade;

public class DialogShortcuts
{
	Gtk.Dialog dialog_shortcuts;
	Gtk.Image image_button_close;
	Gtk.Notebook notebook;

	//images on capture tab
	Gtk.Image image_enc_grav_1;
	Gtk.Image image_enc_inert_1;
	Gtk.Image image_fs_1;
	Gtk.Image image_ra_1;
	Gtk.Image image_enc_grav_2;
	Gtk.Image image_enc_inert_2;
	Gtk.Image image_fs_2;
	Gtk.Image image_ra_2;
	Gtk.Image image_enc_grav_3;
	Gtk.Image image_enc_inert_3;
	Gtk.Image image_fs_3;
	Gtk.Image image_ra_3;
	Gtk.Image image_enc_grav_4;
	Gtk.Image image_enc_inert_4;
	Gtk.Image image_enc_grav_5;
	Gtk.Image image_enc_inert_5;

	//images on analyze tab
	Gtk.Image image_jump_1;
	Gtk.Image image_run_1;
	Gtk.Image image_jump_2;
	Gtk.Image image_run_2;
	Gtk.Image image_jump_3;
	Gtk.Image image_run_3;
	Gtk.Image image_jump_4;
	Gtk.Image image_run_4;
	Gtk.Image image_fs_4;


	public DialogShortcuts (bool isMac)
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "dialog_shortcuts.glade", "dialog_shortcuts", "chronojump");
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "dialog_shortcuts.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);

		//put an icon to window
		UtilGtk.IconWindow(dialog_shortcuts);

		//manage window color
		if(! Config.UseSystemColor)
		{
			UtilGtk.DialogColor(dialog_shortcuts, Config.ColorBackground);
			UtilGtk.WidgetColor (notebook, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundShiftedIsDark, notebook);
		}

		/*
		 * on 2.0 mac will also use ctrl until we find the way to use command
		 *
		if(isMac)
		{
			label_ctrl.Text = "Command";
			label_ctrl1.Text = "Command";
			label_ctrl2.Text = "Command";
			label_ctrl3.Text = "Command";
			label_ctrl4.Text = "Command";
			label_ctrl5.Text = "Command";
			label_enter.Text = "Return";
		}
		*/

		putIcons();
	}

	private void putIcons()
	{
		Pixbuf pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_cancel.png");
		image_button_close.Pixbuf = pixbuf;

		//capture tab
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_weight.png"); //encoder gravitatory
		image_enc_grav_1.Pixbuf = pixbuf;
		image_enc_grav_2.Pixbuf = pixbuf;
		image_enc_grav_3.Pixbuf = pixbuf;
		image_enc_grav_4.Pixbuf = pixbuf;
		image_enc_grav_5.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_inertia.png");
		image_enc_inert_1.Pixbuf = pixbuf;
		image_enc_inert_2.Pixbuf = pixbuf;
		image_enc_inert_3.Pixbuf = pixbuf;
		image_enc_inert_4.Pixbuf = pixbuf;
		image_enc_inert_5.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "force_sensor_icon.png");
		image_fs_1.Pixbuf = pixbuf;
		image_fs_2.Pixbuf = pixbuf;
		image_fs_3.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "race_encoder_icon.png");
		image_ra_1.Pixbuf = pixbuf;
		image_ra_2.Pixbuf = pixbuf;
		image_ra_3.Pixbuf = pixbuf;

		//analyze tab
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_jump.png");
		image_jump_1.Pixbuf = pixbuf;
		image_jump_2.Pixbuf = pixbuf;
		image_jump_3.Pixbuf = pixbuf;
		image_jump_4.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_run.png");
		image_run_1.Pixbuf = pixbuf;
		image_run_2.Pixbuf = pixbuf;
		image_run_3.Pixbuf = pixbuf;
		image_run_4.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "force_sensor_icon.png");
		image_fs_4.Pixbuf = pixbuf;
	}

	public void on_button_close_clicked (object obj, EventArgs args) {
		dialog_shortcuts.Destroy ();
	}

	private void on_delete_event (object o, DeleteEventArgs args) {
		dialog_shortcuts.Destroy ();
	}

	private void connectWidgets (Gtk.Builder builder)
	{
		dialog_shortcuts = (Gtk.Dialog) builder.GetObject ("dialog_shortcuts");
		image_button_close = (Gtk.Image) builder.GetObject ("image_button_close");
		notebook = (Gtk.Notebook) builder.GetObject ("notebook");
		image_enc_grav_1 = (Gtk.Image) builder.GetObject ("image_enc_grav_1");
		image_enc_inert_1 = (Gtk.Image) builder.GetObject ("image_enc_inert_1");
		image_fs_1 = (Gtk.Image) builder.GetObject ("image_fs_1");
		image_ra_1 = (Gtk.Image) builder.GetObject ("image_ra_1");
		image_enc_grav_2 = (Gtk.Image) builder.GetObject ("image_enc_grav_2");
		image_enc_inert_2 = (Gtk.Image) builder.GetObject ("image_enc_inert_2");
		image_fs_2 = (Gtk.Image) builder.GetObject ("image_fs_2");
		image_ra_2 = (Gtk.Image) builder.GetObject ("image_ra_2");
		image_enc_grav_3 = (Gtk.Image) builder.GetObject ("image_enc_grav_3");
		image_enc_inert_3 = (Gtk.Image) builder.GetObject ("image_enc_inert_3");
		image_fs_3 = (Gtk.Image) builder.GetObject ("image_fs_3");
		image_ra_3 = (Gtk.Image) builder.GetObject ("image_ra_3");
		image_enc_grav_4 = (Gtk.Image) builder.GetObject ("image_enc_grav_4");
		image_enc_inert_4 = (Gtk.Image) builder.GetObject ("image_enc_inert_4");
		image_enc_grav_5 = (Gtk.Image) builder.GetObject ("image_enc_grav_5");
		image_enc_inert_5 = (Gtk.Image) builder.GetObject ("image_enc_inert_5");
		image_jump_1 = (Gtk.Image) builder.GetObject ("image_jump_1");
		image_run_1 = (Gtk.Image) builder.GetObject ("image_run_1");
		image_jump_2 = (Gtk.Image) builder.GetObject ("image_jump_2");
		image_run_2 = (Gtk.Image) builder.GetObject ("image_run_2");
		image_jump_3 = (Gtk.Image) builder.GetObject ("image_jump_3");
		image_run_3 = (Gtk.Image) builder.GetObject ("image_run_3");
		image_jump_4 = (Gtk.Image) builder.GetObject ("image_jump_4");
		image_run_4 = (Gtk.Image) builder.GetObject ("image_run_4");
		image_fs_4 = (Gtk.Image) builder.GetObject ("image_fs_4");
	}
} 
