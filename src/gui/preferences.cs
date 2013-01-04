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
 * Copyright (C) 2004-2011   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.IO;
using Gdk;
using Gtk;
using Glade;
//using Gnome;
using GLib; //for Value
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using Mono.Unix;

public class PreferencesWindow {
	
	[Widget] Gtk.Window preferences;

	[Widget] Gtk.Label label_database;
	[Widget] Gtk.Label label_database_temp;
	[Widget] Gtk.Label label_logs;

	[Widget] Gtk.ComboBox combo_decimals;
	[Widget] Gtk.CheckButton checkbutton_height;
	[Widget] Gtk.CheckButton checkbutton_power;
	[Widget] Gtk.CheckButton checkbutton_initial_speed;
	[Widget] Gtk.CheckButton checkbutton_angle;
	
	[Widget] Gtk.CheckButton checkbutton_show_tv_tc_index;
	[Widget] Gtk.Box hbox_indexes;
	[Widget] Gtk.RadioButton radiobutton_show_q_index;
	[Widget] Gtk.RadioButton radiobutton_show_dj_index;
	
	[Widget] Gtk.CheckButton checkbutton_ask_deletion;

	[Widget] Gtk.RadioButton radio_elevation_height;
	[Widget] Gtk.RadioButton radio_elevation_tf;
	[Widget] Gtk.RadioButton radio_speed_ms;
	[Widget] Gtk.RadioButton radio_speed_km;
	[Widget] Gtk.RadioButton radio_weight_percent;
	[Widget] Gtk.RadioButton radio_weight_kg;
	[Widget] Gtk.TextView textview_power;

	[Widget] Gtk.CheckButton checkbutton_volume;
	[Widget] Gtk.CheckButton checkbutton_video;
	[Widget] Gtk.Image image_volume;
	[Widget] Gtk.Label label_volume;
	[Widget] Gtk.Image image_video_yes;
	[Widget] Gtk.Image image_video_no;
	[Widget] Gtk.Label label_video;
//	[Widget] Gtk.Label label_video_experimental;

//	[Widget] Gtk.Box hbox_language_row;
//	[Widget] Gtk.Box hbox_combo_language;
//	[Widget] Gtk.ComboBox combo_language;

	[Widget] Gtk.Button button_accept;
	
	static PreferencesWindow PreferencesWindowBox;

	//language when window is called. If changes, then change data in sql and show 
	//dialogMessage
	//private string languageIni;


	PreferencesWindow () {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "preferences", null);
		gladeXML.Autoconnect(this);
		
		//put an icon to window
		UtilGtk.IconWindow(preferences);

		label_database.Visible = false;
		label_database_temp.Visible = false;

		//database and log files stuff
		label_database.Text = Util.GetDatabaseDir() + System.IO.Path.DirectorySeparatorChar  + "chronojump.db";
		label_database_temp.Text = Util.GetDatabaseTempDir() + System.IO.Path.DirectorySeparatorChar  + "chronojump.db";
		//label_logs.Text = Log.GetDir();
	}
	
	static public PreferencesWindow Show (int digitsNumber, bool showHeight, bool showPower,  
			bool showInitialSpeed, bool showAngle, bool showQIndex, bool showDjIndex,
			bool askDeletion, bool weightStatsPercent, bool heightPreferred, bool metersSecondsPreferred, 
			string language, bool volumeOn, bool videoOn)
	{
		if (PreferencesWindowBox == null) {
			PreferencesWindowBox = new PreferencesWindow ();
		}

		//PreferencesWindowBox.languageIni = language;
		//if(Util.IsWindows())
		//	PreferencesWindowBox.createComboLanguage(language);
		//else 
			PreferencesWindowBox.hideLanguageStuff();
		
		string [] decs = {"1", "2", "3"};
		PreferencesWindowBox.combo_decimals.Active = UtilGtk.ComboMakeActive(decs, digitsNumber.ToString());

		if(showHeight) 
			PreferencesWindowBox.checkbutton_height.Active = true; 
		else 
			PreferencesWindowBox.checkbutton_height.Active = false; 
		
		if(showPower) {
			PreferencesWindowBox.checkbutton_power.Active = true; 
			PreferencesWindowBox.textview_power.Show();
		} else {
			PreferencesWindowBox.checkbutton_power.Active = false; 
			PreferencesWindowBox.textview_power.Hide();
		}
		
		if(showInitialSpeed)  
			PreferencesWindowBox.checkbutton_initial_speed.Active = true; 
		else 
			PreferencesWindowBox.checkbutton_initial_speed.Active = false; 
		
		if(showAngle)  
			PreferencesWindowBox.checkbutton_angle.Active = true; 
		else 
			PreferencesWindowBox.checkbutton_angle.Active = false; 
		

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
			PreferencesWindowBox.radio_weight_percent.Active = true; 
		else 
			PreferencesWindowBox.radio_weight_kg.Active = true; 
		

		if(heightPreferred)  
			PreferencesWindowBox.radio_elevation_height.Active = true; 
		else 
			PreferencesWindowBox.radio_elevation_tf.Active = true; 
		

		if(metersSecondsPreferred)  
			PreferencesWindowBox.radio_speed_ms.Active = true; 
		else 
			PreferencesWindowBox.radio_speed_km.Active = true; 
		
		UtilGtk.ColorsCheck(PreferencesWindowBox.checkbutton_volume);
		PreferencesWindowBox.checkbutton_volume.Active = volumeOn;
		PreferencesWindowBox.on_checkbutton_volume_clicked(new object(), new EventArgs());
		
		UtilGtk.ColorsCheck(PreferencesWindowBox.checkbutton_video);
		PreferencesWindowBox.checkbutton_video.Active = videoOn;
		PreferencesWindowBox.on_checkbutton_video_clicked(new object(), new EventArgs());

		PreferencesWindowBox.preferences.Show ();
		return PreferencesWindowBox;
	}
	
	private void changeVolumeButton(bool myVolume) {
		Pixbuf pixbuf;
		if(myVolume) {
			pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "audio-volume-high.png");
			label_volume.Text = Catalog.GetString("Sound activated.");
		} else {
			pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "audio-volume-muted.png");
			label_volume.Text = Catalog.GetString("No Sound.");
		}
		image_volume.Pixbuf = pixbuf;
	}
	
	private void on_checkbutton_volume_clicked(object o, EventArgs args) {
		if(checkbutton_volume.Active) {
			SqlitePreferences.Update("volumeOn", "True", false);
			label_volume.Text = Catalog.GetString("Sound activated.");
		} else {
			SqlitePreferences.Update("volumeOn", "False", false);
			label_volume.Text = Catalog.GetString("No Sound.");
		}
		changeVolumeButton(checkbutton_volume.Active);
	}

	private void changeVideoButton(bool myVideo) {
		image_video_yes.Visible = myVideo;
		image_video_no.Visible = ! myVideo;
		
		if(myVideo) {	
			label_video.Text = Catalog.GetString("Enabled test video recording and person's snapshots.");
			//label_video_experimental.Visible = true;
		}
		else {
			label_video.Text = Catalog.GetString("Disabled video and snapshots.");
			//label_video_experimental.Visible = false;
		}
	}
	
	private void on_checkbutton_video_clicked(object o, EventArgs args) {
		if(checkbutton_video.Active) {
			SqlitePreferences.Update("videoOn", "True", false);
			label_video.Text = Catalog.GetString("Enabled test video recording and person's snapshots.");
		} else {
			SqlitePreferences.Update("videoOn", "False", false);
			label_video.Text = Catalog.GetString("Disabled video and snapshots.");
		}
		changeVideoButton(checkbutton_video.Active);
	}

	
	private void createComboLanguage(string myLanguageCode) {
		/*
		combo_language = ComboBox.NewText ();
		UtilGtk.ComboUpdate(combo_language, Util.GetLanguagesNames(), "");
		
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
			*/
	}
			
	private void hideLanguageStuff() {
		//hbox_language_row.Hide();
	}
	
	private void on_checkbutton_show_tv_tc_index_clicked (object o, EventArgs args) {
		if(checkbutton_show_tv_tc_index.Active)
			hbox_indexes.Show();
		else
			hbox_indexes.Hide();
	}
		
	private void on_checkbutton_power_clicked (object o, EventArgs args) {
		if(checkbutton_power.Active)
			textview_power.Show();
		else
			textview_power.Hide();
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
	
	void on_button_db_folder_open_clicked (object o, EventArgs args)
	{
		System.IO.FileInfo file1 = new System.IO.FileInfo(label_database.Text); //potser cal una arrobar abans (a windows)
		System.IO.FileInfo file2 = new System.IO.FileInfo(label_database_temp.Text); //potser cal una arrobar abans (a windows)

		if(file1.Exists)
			System.Diagnostics.Process.Start(Util.GetDatabaseDir()); 
		else if(file2.Exists)
			System.Diagnostics.Process.Start(Util.GetDatabaseTempDir()); 
		else
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Error. Cannot find database."));
	}
	
	string fileDB;
	string fileCopy;
	Gtk.FileChooserDialog fc;
	void on_button_db_backup_clicked (object o, EventArgs args)
	{
		System.IO.FileInfo file1 = new System.IO.FileInfo(label_database.Text); //potser cal una arrobar abans (a windows)
		System.IO.FileInfo file2 = new System.IO.FileInfo(label_database_temp.Text); //potser cal una arrobar abans (a windows)
		fileDB = "";

		long length1 = 0;
		if(file1.Exists)
			length1 = file1.Length;
		long length2 = 0;
		if(file2.Exists)
			length2 = file2.Length;
		
		if(length1 == 0 && length2 == 0) 
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Error. Cannot find database."));
		else if(length1 > length2)
			fileDB = label_database.Text;
		else
			fileDB = label_database_temp.Text;

		fc = new Gtk.FileChooserDialog(Catalog.GetString("Copy database to:"),
				preferences,
				FileChooserAction.SelectFolder,
				Catalog.GetString("Cancel"),ResponseType.Cancel,
				Catalog.GetString("Copy"),ResponseType.Accept
				);

		if (fc.Run() == (int)ResponseType.Accept) 
		{
			fileCopy = fc.Filename + Path.DirectorySeparatorChar + "chronojump_copy.db";
			try {
				fc.Hide ();
				if (File.Exists(fileCopy)) {
					Log.WriteLine(string.Format("File {0} exists with attributes {1}, created at {2}", 
								fileCopy, File.GetAttributes(fileCopy), File.GetCreationTime(fileCopy)));
					Log.WriteLine("Overwrite...");
					ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString("Are you sure you want to overwrite file: "), "", fileCopy);
					confirmWin.Button_accept.Clicked += new EventHandler(on_overwrite_file_accepted);
				} else {
					File.Copy(fileDB, fileCopy);
					string myString = string.Format(Catalog.GetString("Copied to {0}"), fileCopy);
					new DialogMessage(Constants.MessageTypes.INFO, myString);
				}
			} 
			catch {
				string myString = string.Format(Catalog.GetString("Cannot copy to file {0} "), fileCopy);
				new DialogMessage(Constants.MessageTypes.WARNING, myString);
			}
		}
		else {
			fc.Hide ();
			return ;
		}
		
		//Don't forget to call Destroy() or the FileChooserDialog window won't get closed.
		fc.Destroy();
		
	}
	
	private void on_overwrite_file_accepted(object o, EventArgs args)
	{
		try {
			File.Delete(fileCopy);
			File.Copy(fileDB, fileCopy);
			fc.Hide ();
			string myString = string.Format(Catalog.GetString("Copied to {0}"), fileCopy);
			new DialogMessage(Constants.MessageTypes.INFO, myString);
		} catch {
			string myString = string.Format(Catalog.GetString("Cannot copy to file {0} "), fileCopy);
			new DialogMessage(Constants.MessageTypes.WARNING, myString);
		}
	}
		

	void on_button_accept_clicked (object o, EventArgs args)
	{
		/* the falses are for the dbcon that is not opened */
		SqlitePreferences.Update("digitsNumber", UtilGtk.ComboGetActive(combo_decimals), false);
		SqlitePreferences.Update("showHeight", PreferencesWindowBox.checkbutton_height.Active.ToString(), false);
		SqlitePreferences.Update("showPower", PreferencesWindowBox.checkbutton_power.Active.ToString(), false);
		SqlitePreferences.Update("showInitialSpeed", PreferencesWindowBox.checkbutton_initial_speed.Active.ToString(), false);
		SqlitePreferences.Update("showAngle", PreferencesWindowBox.checkbutton_angle.Active.ToString(), false);
		
		if(PreferencesWindowBox.checkbutton_show_tv_tc_index.Active) {
			SqlitePreferences.Update("showQIndex", PreferencesWindowBox.radiobutton_show_q_index.Active.ToString(), false);
			SqlitePreferences.Update("showDjIndex", PreferencesWindowBox.radiobutton_show_dj_index.Active.ToString(), false);
		} else {
			SqlitePreferences.Update("showQIndex", "False", false);
			SqlitePreferences.Update("showDjIndex", "False", false);
		}
		
		
		SqlitePreferences.Update("askDeletion", PreferencesWindowBox.checkbutton_ask_deletion.Active.ToString(), false);
		SqlitePreferences.Update("weightStatsPercent", PreferencesWindowBox.radio_weight_percent.Active.ToString(), false);
		SqlitePreferences.Update("heightPreferred", PreferencesWindowBox.radio_elevation_height.Active.ToString(), false);
		SqlitePreferences.Update("metersSecondsPreferred", PreferencesWindowBox.radio_speed_ms.Active.ToString(), false);
		
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

	public Button Button_accept 
	{
		set { button_accept = value; }
		get { return button_accept;  }
	}

}
