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
 * Xavier de Blas: 
 * http://www.xdeblas.com, http://www.deporteyciencia.com (parleblas)
 */

using System;
using Gtk;
using Glade;
//using Gnome;
using GLib; //for Value
using System.Text; //StringBuilder
using System.Collections; //ArrayList


public class ErrorWindow
{
	[Widget] Gtk.Window error_window;
	[Widget] Gtk.Label label1;
	[Widget] Gtk.Button button_accept;

	string table;
	static ErrorWindow ErrorWindowBox;
	
	public ErrorWindow (string text1)
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "error_window", null);
		gladeXML.Autoconnect(this);
		
		label1.Text = text1;
	}

	static public ErrorWindow Show (string text1)
	{
		if (ErrorWindowBox == null) {
			ErrorWindowBox = new ErrorWindow(text1);
		}
		ErrorWindowBox.error_window.Show ();
		
		return ErrorWindowBox;
	}
	
	void on_delete_window_event (object o, DeleteEventArgs args)
	{
		ErrorWindowBox.error_window.Hide();
		ErrorWindowBox = null;
	}
	

	protected void on_button_accept_clicked (object o, EventArgs args)
	{
		ErrorWindowBox.error_window.Hide();
		ErrorWindowBox = null;
	}

	public Button Button_accept {
		get { return button_accept; }
	} 	

	~ErrorWindow() {}
	
}

