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
using Mono.Unix;

public class PreferencesWindow {
	
	[Widget] Gtk.Window preferences;
	[Widget] Gtk.Entry entry_chronopic;
	[Widget] Gtk.SpinButton spinbutton_decimals;
	[Widget] Gtk.CheckButton checkbutton_height;
	[Widget] Gtk.CheckButton checkbutton_initial_speed;
	
	[Widget] Gtk.CheckButton checkbutton_allow_finish_rj_after_time;
	[Widget] Gtk.CheckButton checkbutton_show_tv_tc_index;
	[Widget] Gtk.Box hbox_indexes;
	[Widget] Gtk.RadioButton radiobutton_show_q_index;
	[Widget] Gtk.RadioButton radiobutton_show_dj_index;
	
	[Widget] Gtk.CheckButton checkbutton_ask_deletion;
	[Widget] Gtk.CheckButton checkbutton_height_preferred;
	[Widget] Gtk.CheckButton checkbutton_meters_seconds_preferred;
	[Widget] Gtk.CheckButton checkbutton_percent_kg_preferred;
	[Widget] Gtk.Box hbox_language_row;
	[Widget] Gtk.Box hbox_combo_language;
	[Widget] Gtk.ComboBox combo_language;
	[Widget] Gtk.Separator hseparator_language;

	[Widget] Gtk.Button button_accept;
	
	static PreferencesWindow PreferencesWindowBox;
	Gtk.Window parent;

	//language when window is called. If changes, then change data in sql and show 
	//dialogMessage
	private string languageIni;

		
	PreferencesWindow (Gtk.Window parent, string entryChronopic) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "preferences", null);
		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		if(entryChronopic.Length > 0) {
			entry_chronopic.Text = entryChronopic;
		}
		
	}
	
	//static public PreferencesWindow Show (Gtk.Window parent, int digitsNumber, bool showHeight, bool showInitialSpeed, bool askDeletion, bool weightStatsPercent, bool heightPreferred, bool metersSecondsPreferred)
	static public PreferencesWindow Show (Gtk.Window parent, string entryChronopic, int digitsNumber, bool showHeight, 
			bool showInitialSpeed, bool showQIndex, bool showDjIndex,
			//bool askDeletion, bool heightPreferred, bool metersSecondsPreferred, string culture, bool allowFinishRjAfterTime)
			bool askDeletion, bool weightStatsPercent, bool heightPreferred, bool metersSecondsPreferred, string language, bool allowFinishRjAfterTime)
	{
		if (PreferencesWindowBox == null) {
			PreferencesWindowBox = new PreferencesWindow (parent, entryChronopic);
		}


		PreferencesWindowBox.languageIni = language;
		//if(Util.IsWindows())
		//	PreferencesWindowBox.createComboLanguage(language);
		//else 
			PreferencesWindowBox.hideLanguageStuff();
		
		PreferencesWindowBox.spinbutton_decimals.Value = digitsNumber;

		
		if(allowFinishRjAfterTime)
			PreferencesWindowBox.checkbutton_allow_finish_rj_after_time.Active = true; 
		else
			PreferencesWindowBox.checkbutton_allow_finish_rj_after_time.Active = false; 
			
		
		if(showHeight) 
			PreferencesWindowBox.checkbutton_height.Active = true; 
		
		else 
			PreferencesWindowBox.checkbutton_height.Active = false; 
		

		if(showInitialSpeed)  
			PreferencesWindowBox.checkbutton_initial_speed.Active = true; 
		
		else 
			PreferencesWindowBox.checkbutton_initial_speed.Active = false; 
		

		if(showQIndex || showDjIndex) { 
			PreferencesWindowBox.checkbutton_show_tv_tc_index.Active = true; 
			if(showQIndex) {
				PreferencesWindowBox.radiobutton_show_q_index.Active = true; 
				PreferencesWindowBox.radiobutton_show_dj_index.Active = false; 
			} else {
				PreferencesWindowBox.radiobutton_show_q_index.Active = false; 
				PreferencesWindowBox.radiobutton_show_dj_index.Active = true; 
			}
		}
		else {
			PreferencesWindowBox.checkbutton_show_tv_tc_index.Active = false; 
			PreferencesWindowBox.hbox_indexes.Hide();
		}

		if(askDeletion)  
			PreferencesWindowBox.checkbutton_ask_deletion.Active = true; 
		
		else 
			PreferencesWindowBox.checkbutton_ask_deletion.Active = false; 
		

		if(weightStatsPercent)  
			PreferencesWindowBox.checkbutton_percent_kg_preferred.Active = true; 
		
		else 
			PreferencesWindowBox.checkbutton_percent_kg_preferred.Active = false; 
		

		if(heightPreferred)  
			PreferencesWindowBox.checkbutton_height_preferred.Active = true; 
		
		else 
			PreferencesWindowBox.checkbutton_height_preferred.Active = false; 
		

		if(metersSecondsPreferred)  
			PreferencesWindowBox.checkbutton_meters_seconds_preferred.Active = true; 
		
		else 
			PreferencesWindowBox.checkbutton_meters_seconds_preferred.Active = false; 
		

		PreferencesWindowBox.preferences.Show ();

		return PreferencesWindowBox;
	}
	
	private void createComboLanguage(string myLanguageCode) {
		combo_language = ComboBox.NewText ();
		UtilGtk.ComboUpdate(combo_language, Util.GetLanguagesNames());
		
		//combo_language.Entry.Changed += new EventHandler (on_combo_language_changed);

		hbox_combo_language.PackStart(combo_language, false, false, 0);
		hbox_combo_language.ShowAll();
		
		bool found = false;
		int count = 0;
		foreach (string lang in Constants.Languages) {
			if (myLanguageCode == Util.GetLanguageCode(lang)) {
				combo_language.Active = count;
				found = true;
			}
			count ++;
		}
		if(!found)
			combo_language.Active = UtilGtk.ComboMakeActive(Constants.Languages, Util.GetLanguageName(Constants.LanguageDefault));
		
		//if(Util.IsWindows())
		//	combo_language.Sensitive = true;
		//else 
			combo_language.Sensitive = false;
	}
			
	private void hideLanguageStuff() {
		hbox_language_row.Hide();
		hseparator_language.Hide();
	}
	
	private void on_checkbutton_show_tv_tc_index_clicked (object o, EventArgs args) {
		if(checkbutton_show_tv_tc_index.Active)
			hbox_indexes.Show();
		else
			hbox_indexes.Hide();
	}
		
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		PreferencesWindowBox.preferences.Hide();
		PreferencesWindowBox = null;
	}
	
	void on_preferences_delete_event (object o, DeleteEventArgs args)
	{
		PreferencesWindowBox.preferences.Hide();
		PreferencesWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		SqlitePreferences.Update("chronopicPort", entry_chronopic.Text.ToString());
		SqlitePreferences.Update("digitsNumber", spinbutton_decimals.Value.ToString());
		SqlitePreferences.Update("showHeight", PreferencesWindowBox.checkbutton_height.Active.ToString());
		SqlitePreferences.Update("showInitialSpeed", PreferencesWindowBox.checkbutton_initial_speed.Active.ToString());
		SqlitePreferences.Update("allowFinishRjAfterTime", PreferencesWindowBox.checkbutton_allow_finish_rj_after_time.Active.ToString());
		
		if(PreferencesWindowBox.checkbutton_show_tv_tc_index.Active) {
			SqlitePreferences.Update("showQIndex", PreferencesWindowBox.radiobutton_show_q_index.Active.ToString());
			SqlitePreferences.Update("showDjIndex", PreferencesWindowBox.radiobutton_show_dj_index.Active.ToString());
		} else {
			SqlitePreferences.Update("showQIndex", "False");
			SqlitePreferences.Update("showDjIndex", "False");
		}
		
		
		SqlitePreferences.Update("askDeletion", PreferencesWindowBox.checkbutton_ask_deletion.Active.ToString());
		SqlitePreferences.Update("weightStatsPercent", PreferencesWindowBox.checkbutton_percent_kg_preferred.Active.ToString());
		SqlitePreferences.Update("heightPreferred", PreferencesWindowBox.checkbutton_height_preferred.Active.ToString());
		SqlitePreferences.Update("metersSecondsPreferred", PreferencesWindowBox.checkbutton_meters_seconds_preferred.Active.ToString());
		
		/*
		if(Util.IsWindows()) {
			//if language has changed
			if(UtilGtk.ComboGetActive(PreferencesWindowBox.combo_language) != languageIni) {
				string myLanguage = SqlitePreferences.Select("language");
				if ( myLanguage != null && myLanguage != "" && myLanguage != "0") {
					//if language exists in sqlite preferences update it
					SqlitePreferences.Update("language", Util.GetLanguageCodeFromName(UtilGtk.ComboGetActive(PreferencesWindowBox.combo_language)));
				} else {
					//else: create it
					SqlitePreferences.Insert("language", Util.GetLanguageCodeFromName(UtilGtk.ComboGetActive(PreferencesWindowBox.combo_language)));
				}

				new DialogMessage(Catalog.GetString("Restart Chronojump to operate completely on your language."), true);
			}
		}
		*/

		PreferencesWindowBox.preferences.Hide();
		PreferencesWindowBox = null;
	}

	private void on_button_help_clicked (object o, EventArgs args) {
		new HelpPorts();
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
