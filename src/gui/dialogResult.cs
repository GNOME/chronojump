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
 * Copyright (C) 2025   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gdk;
using Gtk;

public class DialogResult
{
	// at glade ---->
	Gtk.Dialog dialog_result;
	Gtk.Label label_result;
	// <---- at glade

	public DialogResult (string title, int width, int height, string resultStr)
	{
		initialize (title, width, height, resultStr);
	}

	private void initialize (string title, int width, int height, string resultStr)
	{
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "dialog_result.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);

		//put an icon to window
		UtilGtk.IconWindow (dialog_result);

		label_result.Name = "huge_monospace";

		//manage window color
		if(! Config.UseSystemColor)
		{
			UtilGtk.DialogColor(dialog_result, Config.ColorBackground);
			UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundShiftedIsDark, label_result);
		}
	
		//with this, user doesn't see a moving/changing creation window
		//if uncommented, then does weird bug in windows not showing dialog as its correct size until window is moves
		//dialog_result.Hide();	

		if(title != "")
			dialog_result.Title = title;

		if (width > 0 && height > 0)
		{
			dialog_result.WidthRequest = width;
			dialog_result.HeightRequest = height;
		}

		label_result.Text = resultStr;

		dialog_result.Show();
	}

	public void on_close_button_clicked (object obj, EventArgs args)
	{
		//Visible = false;
		dialog_result.Destroy ();
	}

	private void on_delete_event (object o, DeleteEventArgs args)
	{
		//Visible = false;
		dialog_result.Destroy ();
	}

	private void connectWidgets (Gtk.Builder builder)
	{
		dialog_result = (Gtk.Dialog) builder.GetObject ("dialog_result");
		label_result = (Gtk.Label) builder.GetObject ("label_result");
	}
}
