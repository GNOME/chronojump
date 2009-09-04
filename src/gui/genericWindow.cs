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
 * Copyright (C) 2004-2009   Xavier de Blas <xaviblas@gmail.com> 
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
	[Widget] Gtk.Entry entry;
	[Widget] Gtk.SpinButton spinbutton;
	[Widget] Gtk.ScrolledWindow scrolled_window;
	[Widget] Gtk.TextView textview;
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

	static public GenericWindow Show (string textHeader, Constants.GenericWindowShow stuff)
	{
		if (GenericWindowBox == null) {
			GenericWindowBox = new GenericWindow();
		}
		GenericWindowBox.showHideWidgets(stuff);
		GenericWindowBox.label_header.Text = textHeader;

		GenericWindowBox.generic_window.Show ();
		
		return GenericWindowBox;
	}
	
	void showHideWidgets(Constants.GenericWindowShow stuff) {
		entry.Hide();
		spinbutton.Hide();
		scrolled_window.Hide();

		if(stuff == Constants.GenericWindowShow.ENTRY)
			entry.Show();
		else if(stuff == Constants.GenericWindowShow.SPIN)
			spinbutton.Show();
		else //if(stuff == Constants.GenericWindowShow.TEXTVIEW)
			scrolled_window.Show();
	}
	
	public void SetSpinRange(double min, double max) {
		spinbutton.SetRange(min, max);
	}
	
	public void SetTextview(string str) {
		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.Text = str;
		textview.Buffer = tb;
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
		get { return entry.Text.ToString(); }
	}

	public int SpinSelected {
		get { return (int) spinbutton.Value; }
	}
	
	public string TextviewSelected {
		get { return Util.RemoveTab(textview.Buffer.Text); }
	}
	

	~GenericWindow() {}
	
}

