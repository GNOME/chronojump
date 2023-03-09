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
using Gtk;
//using Glade;
//using Gnome;
using GLib; //for Value
using System.Text; //StringBuilder
using System.Collections; //ArrayList

public class ErrorWindow
{
	Gtk.Window error_window;
	Gtk.Label label1;
	Gtk.Button button_accept;
	

	static ErrorWindow ErrorWindowBox;
	
	public ErrorWindow (string text1)
	{
		LogB.Information("At error window2");
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "error_window.glade", "error_window", "chronojump");
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "error_window.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);
		
		//put an icon to window
		UtilGtk.IconWindow(error_window);
		
		label1.Text = text1;
		label1.UseMarkup = true;
	}

	static public ErrorWindow Show (string text1)
	{
		LogB.Information("At error window");
		if (ErrorWindowBox == null) {
			ErrorWindowBox = new ErrorWindow(text1);
		}
		
		ErrorWindowBox.error_window.Show();
		
		return ErrorWindowBox;
	}
	
	void on_delete_window_event (object o, DeleteEventArgs args)
	{
		/*
		ErrorWindowBox.error_window.Hide();
		ErrorWindowBox = null;
		*/
		
		//need this for continue execution on chronojump startup. See src/chronojump.cs
		button_accept.Click();
	}

	public Button Button_accept {
		get { return button_accept; }
	}

	protected void on_button_accept_clicked (object o, EventArgs args)
	{
		ErrorWindowBox.error_window.Hide();
		ErrorWindowBox = null;
	}

	public void HideAndNull() {
		if(ErrorWindowBox != null) {
			ErrorWindowBox.error_window.Hide();
			ErrorWindowBox = null;
		}
	}

	private void connectWidgets (Gtk.Builder builder)
	{
		error_window = (Gtk.Window) builder.GetObject ("error_window");
		label1 = (Gtk.Label) builder.GetObject ("label1");
		button_accept = (Gtk.Button) builder.GetObject ("button_accept");
	}

	~ErrorWindow() {}
	
}

