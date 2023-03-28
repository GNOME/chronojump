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
using Gdk;
using Gtk;

public class DialogMessage
{
	Gtk.Dialog dialog_message;
	Gtk.ScrolledWindow scrolledwindow_by_software;
	Gtk.Label label_message;

	Gtk.Image image_warning;
	Gtk.Image image_info;
	Gtk.Image image_help;
	Gtk.Image image_inspect;

	Gtk.Button button_go;

	public bool Visible;
	private string button_go_link = "";

	public DialogMessage (Constants.MessageTypes type, string message)
	{
		initialize("", type, message, false);
	}
	public DialogMessage (string title, Constants.MessageTypes type, string message)
	{
		initialize(title, type, message, false);
	}
	public DialogMessage (string title, Constants.MessageTypes type, string message, bool showScrolledWinBar)
	{
		initialize(title, type, message, showScrolledWinBar);
	}
	//special caller to show stiffness formula or others
	public DialogMessage (Constants.MessageTypes type, string message, string objectToShow)
	{
		initialize("", type, message, false);
		if(objectToShow == "button_go_r_mac")
		{
			button_go_link = Constants.RmacDownload;
			button_go.Show();
		}
	}

	private void initialize(string title, Constants.MessageTypes type, string message, bool showScrolledWinBar)
	{
		LogB.Information("Dialog message: " + message);

		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "dialog_message.glade", "dialog_message", "chronojump");
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "dialog_message.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);

		button_go.Visible = false;
		button_go_link = "";

		Visible = true;

		//put an icon to window
		UtilGtk.IconWindow(dialog_message);

		//manage window color
		if(! Config.UseSystemColor)
		{
			UtilGtk.DialogColor(dialog_message, Config.ColorBackground);
			UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundShiftedIsDark, label_message);
		}
	
		//with this, user doesn't see a moving/changing creation window
		//if uncommented, then does weird bug in windows not showing dialog as its correct size until window is moves
		//dialog_message.Hide();	

		if(title != "")
			dialog_message.Title = title;

		label_message.Text = message; 
		label_message.UseMarkup = true; 

		image_warning.Hide();
		image_info.Hide();
		image_help.Hide();
		image_inspect.Hide();

		switch (type) {
			case Constants.MessageTypes.WARNING:
				image_warning.Show();
			break;
			case Constants.MessageTypes.INFO:
				image_info.Show();
			break;
			case Constants.MessageTypes.HELP:
				image_help.Show();
			break;
			case Constants.MessageTypes.INSPECT:
				Pixbuf pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_test_inspect.png");
				image_inspect.Pixbuf = pixbuf;
				image_inspect.Show();
			break;
		}

		if(showScrolledWinBar)
		{
			dialog_message.HeightRequest = 450;
			scrolledwindow_by_software.SetPolicy(PolicyType.Never, PolicyType.Automatic);
		}
		else
			scrolledwindow_by_software.SetPolicy(PolicyType.Never, PolicyType.Never);

		label_message.Show();
		dialog_message.Show();

		//if(UtilAll.GetOSEnum() == UtilAll.OperatingSystems.MACOSX)
		//	GLib.Timeout.Add(200, new GLib.TimeoutHandler(resizeDialogForMac));
	}

	/*
	   on some macs, the label is not shown correctly, seems not a problem of frame or scrollerwin
	   after resizing a bit (by user) is shown correctly, so resize here.
	   */
	/*
	 * deactivated on gtk3
	 *
	private bool resizeDialogForMac ()
	{
		dialog_message.SetSizeRequest(
				dialog_message.SizeRequest().Width + 3,
				dialog_message.SizeRequest().Height + 3);

		return false;
	}
	*/

	public void on_button_go_clicked (object obj, EventArgs args)
	{
		LogB.Information("Opening browser (r mac install) to: " + button_go_link);
		if(! Util.OpenURL (button_go_link))
			label_message.Text = Constants.WebsiteNotFoundStr();
	}

	public void on_close_button_clicked (object obj, EventArgs args)
	{
		Visible = false;
		dialog_message.Destroy ();
	}

	private void on_delete_event (object o, DeleteEventArgs args)
	{
		Visible = false;
		dialog_message.Destroy ();
	}

	private void connectWidgets (Gtk.Builder builder)
	{
		dialog_message = (Gtk.Dialog) builder.GetObject ("dialog_message");
		scrolledwindow_by_software = (Gtk.ScrolledWindow) builder.GetObject ("scrolledwindow_by_software");
		label_message = (Gtk.Label) builder.GetObject ("label_message");
		image_warning = (Gtk.Image) builder.GetObject ("image_warning");
		image_info = (Gtk.Image) builder.GetObject ("image_info");
		image_help = (Gtk.Image) builder.GetObject ("image_help");
		image_inspect = (Gtk.Image) builder.GetObject ("image_inspect");
		button_go = (Gtk.Button) builder.GetObject ("button_go");
	}
}
