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
 * Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Glade;
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using Mono.Unix;

using System.Threading;


using Gdk; //for the EventMask



public class EventGraphConfigureWindow 
{
	[Widget] Gtk.Window event_graph_configure;
	
	[Widget] Gtk.CheckButton checkbutton_max_auto;
	[Widget] Gtk.CheckButton checkbutton_min_auto;
	[Widget] Gtk.CheckButton checkbutton_show_black_guide;
	[Widget] Gtk.CheckButton checkbutton_show_green_guide;
	
	[Widget] Gtk.SpinButton spinbutton_max;
	[Widget] Gtk.SpinButton spinbutton_min;
	[Widget] Gtk.SpinButton spinbutton_black_guide;
	[Widget] Gtk.SpinButton spinbutton_green_guide;
	
	[Widget] Gtk.CheckButton checkbutton_show_vertical_grid;
	
	static EventGraphConfigureWindow EventGraphConfigureWindowBox;
		
	EventGraphConfigureWindow () {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "event_graph_configure.glade", "event_graph_configure", "chronojump");
		gladeXML.Autoconnect(this);
		
		//put an icon to window
		UtilGtk.IconWindow(event_graph_configure);
	}

	//bool reallyShow
	//we create this window on start of event_execute widget for having the graph execute values defined
	//but we don't want to show until user clicks on "properties" on the event_execute widget
	static public EventGraphConfigureWindow Show (bool reallyShow)
	{
		if (EventGraphConfigureWindowBox == null) {
			EventGraphConfigureWindowBox = new EventGraphConfigureWindow (); 
			EventGraphConfigureWindowBox.initializeWidgets(); 
		}
		
		if(reallyShow)
			EventGraphConfigureWindowBox.event_graph_configure.Show ();
		else
			EventGraphConfigureWindowBox.event_graph_configure.Hide ();
		
		return EventGraphConfigureWindowBox;
	}
	
	void initializeWidgets ()
	{
		checkbutton_max_auto.Active = true;
		checkbutton_min_auto.Active = false;
		
		checkbutton_show_black_guide.Active = false;
		checkbutton_show_green_guide.Active = false;
			
		checkbutton_show_vertical_grid.Active = true;
		spinbutton_black_guide.Sensitive = false;
		spinbutton_green_guide.Sensitive = false;
	}

	void on_checkbutton_max_auto_clicked (object o, EventArgs args) {
		if(checkbutton_max_auto.Active)
			spinbutton_max.Sensitive = false;
		else
			spinbutton_max.Sensitive = true;
	}
	
	void on_checkbutton_min_auto_clicked (object o, EventArgs args) {
		if(checkbutton_min_auto.Active)
			spinbutton_min.Sensitive = false;
		else
			spinbutton_min.Sensitive = true;
	}
	
	void on_checkbutton_show_black_guide_clicked (object o, EventArgs args) {
		if(checkbutton_show_black_guide.Active)
			spinbutton_black_guide.Sensitive = true;
		else
			spinbutton_black_guide.Sensitive = false;
	}
	
	void on_checkbutton_show_green_guide_clicked (object o, EventArgs args) {
		if(checkbutton_show_green_guide.Active)
			spinbutton_green_guide.Sensitive = true;
		else
			spinbutton_green_guide.Sensitive = false;
	}
	
		
	/*
	void on_button_help_clicked (object o, EventArgs args)
	{
		Log.WriteLine("help Clicked");
	
		new DialogMessage(Constants.MessageTypes.HELP, Catalog.GetString("This window allows to change the graph options. \nFirst, you can adjust the Y parameters\nSecond, put guides\n"));
	}
	*/

	void on_button_close_clicked (object o, EventArgs args)
	{
		EventGraphConfigureWindowBox.event_graph_configure.Hide();
		//EventGraphConfigureWindowBox = null;
	}

	void on_delete_event (object o, DeleteEventArgs args)
	{
		EventGraphConfigureWindowBox.event_graph_configure.Hide();
		EventGraphConfigureWindowBox = null;
	}

	public double Max {
		get {
			if(checkbutton_max_auto.Active)
				return -1;
			else
				return Convert.ToDouble(spinbutton_max.Value);
		}
	}

	public double Min {
		get {
			if(checkbutton_min_auto.Active)
				return -1;
			else
				return Convert.ToDouble(spinbutton_min.Value);
		}
	}

	public double BlackGuide {
		get {
			if(checkbutton_show_black_guide.Active)
				return Convert.ToDouble(spinbutton_black_guide.Value);
			else
				return -1;
		}
	}

	public double GreenGuide {
		get {
			if(checkbutton_show_green_guide.Active)
				return Convert.ToDouble(spinbutton_green_guide.Value);
			else
				return -1;
		}
	}
	
	public bool VerticalGrid {
		get {
			return (checkbutton_show_vertical_grid.Active);
		}
	}
}

