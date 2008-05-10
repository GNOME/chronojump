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
//using System.Text; //StringBuilder


public class GenericWindow
{
	[Widget] Gtk.Window generic_window;
	[Widget] Gtk.Label label_header;
	[Widget] Gtk.Label label_generic_name;
	[Widget] Gtk.Entry entry_value;
	[Widget] Gtk.SpinButton spinbutton_value;
	[Widget] Gtk.Button button_accept;

	static GenericWindow GenericWindowBox;
	
	public GenericWindow ()
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "generic_window", null);
		gladeXML.Autoconnect(this);
		
		//put an icon to window
		UtilGtk.IconWindow(generic_window);
	}

	static public GenericWindow Show (string textHeader, bool showEntry, bool showSpin)
	{
		if (GenericWindowBox == null) {
			GenericWindowBox = new GenericWindow();
		}
		GenericWindowBox.showHideWidgets(showEntry, showSpin);
		GenericWindowBox.label_header.Text = textHeader;

		GenericWindowBox.generic_window.Show ();
		
		return GenericWindowBox;
	}
	
	void showHideWidgets(bool showEntry, bool showSpin) {
		if(showEntry)
			entry_value.Show();
		else
			entry_value.Hide();
		
		if(showSpin)
			spinbutton_value.Show();
		else
			spinbutton_value.Hide();
	}
	
	public void SetSpinRange(double min, double max) {
		spinbutton_value.SetRange(min, max);
	}


	protected void on_button_cancel_clicked (object o, EventArgs args)
	{
		GenericWindowBox.generic_window.Hide();
		GenericWindowBox = null;
	}
	
	protected void on_delete_event (object o, DeleteEventArgs args)
	{
		GenericWindowBox.generic_window.Hide();
		GenericWindowBox = null;
	}

	protected void on_button_accept_clicked (object o, EventArgs args)
	{
		GenericWindowBox.generic_window.Hide();
		//GenericWindowBox = null;
	}
	
	public Button Button_accept {
		set { button_accept = value; }
		get { return button_accept; }
	}

	public string EntrySelected {
		get { return entry_value.Text.ToString(); }
	}

	public int SpinSelected {
		get { return (int) spinbutton_value.Value; }
	}
	

	~GenericWindow() {}
	
}

