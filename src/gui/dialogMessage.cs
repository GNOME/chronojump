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
 * Copyright (C) 2004-2016   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Glade;

public class DialogMessage
{
	[Widget] Gtk.Dialog dialog_message;
	[Widget] Gtk.Label label_message;

	[Widget] Gtk.Image image_warning;
	[Widget] Gtk.Image image_info;
	[Widget] Gtk.Image image_help;
		
	[Widget] Gtk.Box hbox_stiffness_formula;

	public DialogMessage (string title, Constants.MessageTypes type, string message)
	{
		initialize(title, type, message);
	}
	public DialogMessage (Constants.MessageTypes type, string message)
	{
		initialize("", type, message);
	}
	//special caller to show stiffness formula or others
	public DialogMessage (Constants.MessageTypes type, string message, string objectToShow)
	{
		initialize("", type, message);
		if(objectToShow == "hbox_stiffness_formula")
			hbox_stiffness_formula.Show();
	}


	private void initialize(string title, Constants.MessageTypes type, string message)
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "dialog_message", "chronojump");
		gladeXML.Autoconnect(this);
		
		//put an icon to window
		UtilGtk.IconWindow(dialog_message);
	
		//with this, user doesn't see a moving/changing creation window
		//if uncommented, then does weird bug in windows not showing dialog as its correct size until window is moves
		//dialog_message.Hide();	

		if(title != "")
			dialog_message.Title = title;

		label_message.Text = message; 
		label_message.UseMarkup = true; 

		switch (type) {
			case Constants.MessageTypes.WARNING:
				image_warning.Show();
				image_info.Hide();
				image_help.Hide();
			break;
			case Constants.MessageTypes.INFO:
				image_warning.Hide();
				image_info.Show();
				image_help.Hide();
			break;
			case Constants.MessageTypes.HELP:
				image_warning.Hide();
				image_info.Hide();
				image_help.Show();
			break;
		}

		label_message.Show();	
		dialog_message.Show();	
	}

	public void on_close_button_clicked (object obj, EventArgs args) {
		dialog_message.Destroy ();
	}

	private void on_delete_event (object o, DeleteEventArgs args) {
		dialog_message.Destroy ();
	}
}
