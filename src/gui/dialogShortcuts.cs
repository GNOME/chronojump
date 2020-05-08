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

public class DialogShortcuts
{
	[Widget] Gtk.Dialog dialog_shortcuts;
	[Widget] Gtk.Label label_ctrl;
	[Widget] Gtk.Label label_ctrl1;
	[Widget] Gtk.Label label_ctrl2;
	[Widget] Gtk.Label label_ctrl3;
	[Widget] Gtk.Label label_ctrl4;
	[Widget] Gtk.Label label_ctrl5;
	[Widget] Gtk.Label label_enter;

	public DialogShortcuts (bool isMac)
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "dialog_shortcuts.glade", "dialog_shortcuts", "chronojump");
		gladeXML.Autoconnect(this);

		//put an icon to window
		UtilGtk.IconWindow(dialog_shortcuts);

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
	}

	public void on_button_close_clicked (object obj, EventArgs args) {
		dialog_shortcuts.Destroy ();
	}

	private void on_delete_event (object o, DeleteEventArgs args) {
		dialog_shortcuts.Destroy ();
	}
} 
