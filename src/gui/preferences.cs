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
using Gnome;
using GLib; //for Value
using System.Text; //StringBuilder
using System.Collections; //ArrayList


public class PreferencesWindow {
	
	[Widget] Gtk.Window preferences;
	[Widget] Gtk.SpinButton spinbutton_decimals;
	[Widget] Gtk.CheckButton checkbutton_height;
	[Widget] Gtk.CheckButton checkbutton_ask_deletion;
	[Widget] Gtk.RadioButton radiobutton_weight_stats_percent;
	[Widget] Gtk.RadioButton radiobutton_weight_stats_kgs;

	[Widget] Gtk.Button button_accept;
	
	static PreferencesWindow PreferencesWindowBox;
	Gtk.Window parent;

	
	PreferencesWindow (Gtk.Window parent) {
		Glade.XML gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "preferences", null);

		gladeXML.Autoconnect(this);
		this.parent = parent;
		
	}
	
	static public PreferencesWindow Show (Gtk.Window parent, int digitsNumber, bool showHeight, bool askDeletion, bool weightStatsPercent)
	{
		if (PreferencesWindowBox == null) {
			PreferencesWindowBox = new PreferencesWindow (parent);
		}
		PreferencesWindowBox.spinbutton_decimals.Value = digitsNumber;
	
		if(showHeight) { 
			PreferencesWindowBox.checkbutton_height.Active = true; 
		}
		else {
			PreferencesWindowBox.checkbutton_height.Active = false; 
		}

		if(askDeletion) { 
			PreferencesWindowBox.checkbutton_ask_deletion.Active = true; 
		}
		else {
			PreferencesWindowBox.checkbutton_ask_deletion.Active = false; 
		}

		if(weightStatsPercent) { 
			PreferencesWindowBox.radiobutton_weight_stats_percent.Active = true; 
		}
		else {
			PreferencesWindowBox.radiobutton_weight_stats_kgs.Active = true; 
		}

		PreferencesWindowBox.preferences.Show ();

		return PreferencesWindowBox;
	}
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		PreferencesWindowBox.preferences.Hide();
		PreferencesWindowBox = null;
	}
	
	void on_preferences_delete_event (object o, EventArgs args)
	{
		PreferencesWindowBox.preferences.Hide();
		PreferencesWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		SqlitePreferences.Update("digitsNumber", spinbutton_decimals.Value.ToString());
		SqlitePreferences.Update("showHeight", PreferencesWindowBox.checkbutton_height.Active.ToString());
		SqlitePreferences.Update("askDeletion", PreferencesWindowBox.checkbutton_ask_deletion.Active.ToString());
		SqlitePreferences.Update("weightStatsPercent", PreferencesWindowBox.radiobutton_weight_stats_percent.Active.ToString());
		
		PreferencesWindowBox.preferences.Hide();
		PreferencesWindowBox = null;
	}

	public Button Button_accept 
	{
		set {
			button_accept = value;	
		}
		get {
			return button_accept;
		}
	}

}
