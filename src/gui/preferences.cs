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
 * Copyright (C) 2004-2014   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.IO;
using Gdk;
using Gtk;
using Glade;
//using Gnome;
//using GLib; //for Value
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using Mono.Unix;
using System.Threading;

public class PreferencesWindow {
	
	[Widget] Gtk.Window preferences;

	[Widget] Gtk.Label label_database;
	[Widget] Gtk.Label label_database_temp;
	
	[Widget] Gtk.Button button_db_folder_open;

	//this three are unneded because cannot be unchecked
	//[Widget] Gtk.CheckButton check_backup_sessions;
	//[Widget] Gtk.CheckButton check_backup_persons;
	//[Widget] Gtk.CheckButton check_backup_contact_tests;
	[Widget] Gtk.CheckButton check_backup_encoder_tests;
	[Widget] Gtk.CheckButton check_backup_multimedia;
	
	[Widget] Gtk.Button button_db_backup;
	[Widget] Gtk.Box hbox_backup_doing;
	[Widget] Gtk.ProgressBar pulsebar;

	[Widget] Gtk.ComboBox combo_decimals;
	[Widget] Gtk.CheckButton checkbutton_height;
	[Widget] Gtk.CheckButton checkbutton_power;
	[Widget] Gtk.CheckButton checkbutton_initial_speed;
	[Widget] Gtk.CheckButton checkbutton_angle;
	
	[Widget] Gtk.Button button_help_power;
	
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
	
	[Widget] Gtk.CheckButton checkbutton_encoder_propulsive;
	[Widget] Gtk.SpinButton spin_encoder_smooth_con;
	[Widget] Gtk.Label label_encoder_con;
			
	[Widget] Gtk.RadioButton radio_encoder_1RM_nonweighted;
	[Widget] Gtk.RadioButton radio_encoder_1RM_weighted;
	[Widget] Gtk.RadioButton radio_encoder_1RM_weighted2;
	[Widget] Gtk.RadioButton radio_encoder_1RM_weighted3;
			
	[Widget] Gtk.RadioButton radio_export_latin;
	[Widget] Gtk.RadioButton radio_export_non_latin;
	
	[Widget] Gtk.RadioButton radio_graphs_translate;
	[Widget] Gtk.RadioButton radio_graphs_no_translate;

//	[Widget] Gtk.Box hbox_language_row;
//	[Widget] Gtk.Box hbox_combo_language;
//	[Widget] Gtk.ComboBox combo_language;

	[Widget] Gtk.Box hbox_combo_camera;
	[Widget] Gtk.ComboBox combo_camera;

	[Widget] Gtk.Button button_accept;
	[Widget] Gtk.Button button_cancel;
	
	static PreferencesWindow PreferencesWindowBox;
	
	private Thread thread;

	//language when window is called. If changes, then change data in sql and show 
	//dialogMessage
	//private string languageIni;


	PreferencesWindow () {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "preferences", "chronojump");
		gladeXML.Autoconnect(this);
		
		//put an icon to window
		UtilGtk.IconWindow(preferences);

		label_database.Visible = false;
		label_database_temp.Visible = false;

		//database and log files stuff
		label_database.Text = Util.GetDatabaseDir() + System.IO.Path.DirectorySeparatorChar  + "chronojump.db";
		label_database_temp.Text = Util.GetDatabaseTempDir() + System.IO.Path.DirectorySeparatorChar  + "chronojump.db";
	}
	
	static public PreferencesWindow Show (int digitsNumber, bool showHeight, bool showPower,  
			bool showInitialSpeed, bool showAngle, bool showQIndex, bool showDjIndex,
			bool askDeletion, bool weightStatsPercent, bool heightPreferred, bool metersSecondsPreferred, 
			string language, bool encoderPropulsive, double encoderSmoothCon,
			string [] videoDevices, int videoDeviceNum, string encoder1RMMethod,
			string CSVExportDecimalSeparator, bool RGraphsTranslate)
	{
		if (PreferencesWindowBox == null) {
			PreferencesWindowBox = new PreferencesWindow ();
		}

		//PreferencesWindowBox.languageIni = language;
		//if(UtilAll.IsWindows())
		//	PreferencesWindowBox.createComboLanguage(language);
		//else 
			PreferencesWindowBox.hideLanguageStuff();

			PreferencesWindowBox.createComboCamera(videoDevices, videoDeviceNum);
		
		string [] decs = {"1", "2", "3"};
		PreferencesWindowBox.combo_decimals.Active = UtilGtk.ComboMakeActive(decs, digitsNumber.ToString());

		if(showHeight) 
			PreferencesWindowBox.checkbutton_height.Active = true; 
		else 
			PreferencesWindowBox.checkbutton_height.Active = false; 
		
		if(showPower) {
			PreferencesWindowBox.checkbutton_power.Active = true; 
			PreferencesWindowBox.button_help_power.Sensitive = true;
		} else {
			PreferencesWindowBox.checkbutton_power.Active = false; 
			PreferencesWindowBox.button_help_power.Sensitive = false;
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


		if(CSVExportDecimalSeparator == "COMMA")
			PreferencesWindowBox.radio_export_latin.Active = true; 
		else
			PreferencesWindowBox.radio_export_non_latin.Active = true; 

	
		//encoder	
		PreferencesWindowBox.checkbutton_encoder_propulsive.Active = encoderPropulsive;
		PreferencesWindowBox.spin_encoder_smooth_con.Value = encoderSmoothCon;

		if(encoder1RMMethod == Constants.Encoder1RMMethod.NONWEIGHTED.ToString())
			PreferencesWindowBox.radio_encoder_1RM_nonweighted.Active = true;
		else if(encoder1RMMethod == Constants.Encoder1RMMethod.WEIGHTED.ToString())
			PreferencesWindowBox.radio_encoder_1RM_weighted.Active = true;
		else if(encoder1RMMethod == Constants.Encoder1RMMethod.WEIGHTED2.ToString())
			PreferencesWindowBox.radio_encoder_1RM_weighted2.Active = true;
		else //(encoder1RMMethod == Constants.Encoder1RMMethod.WEIGHTED3.ToString())
			PreferencesWindowBox.radio_encoder_1RM_weighted3.Active = true;
		/*
		switch(encoder1RMMethod) {
			case Constants.Encoder1RMMethod.NONWEIGHTED.ToString():
				PreferencesWindowBox.radio_encoder_1RM_nonweighted.Active = true;
				break;
			case Constants.Encoder1RMMethod.WEIGHTED.ToString():
				PreferencesWindowBox.radio_encoder_1RM_weighted.Active = true;
				break;
			case Constants.Encoder1RMMethod.WEIGHTED2.ToString():
				PreferencesWindowBox.radio_encoder_1RM_weighted2.Active = true;
				break;
			case Constants.Encoder1RMMethod.WEIGHTED3.ToString():
				PreferencesWindowBox.radio_encoder_1RM_weighted3.Active = true;
				break;
		}
		*/

		//done here and not in glade to be shown with the decimal point of user language	
		PreferencesWindowBox.label_encoder_con.Text = (0.7).ToString();
		
		if(RGraphsTranslate)
			PreferencesWindowBox.radio_graphs_translate.Active = true;
		else
			PreferencesWindowBox.radio_graphs_no_translate.Active = true;

		PreferencesWindowBox.preferences.Show ();
		return PreferencesWindowBox;
	}
	
	private void createComboCamera(string [] devices, int current) {
		combo_camera = ComboBox.NewText ();

		if(devices.Length == 0) {
			devices = Util.StringToStringArray(Constants.CameraNotFound);
			current = 0;
		}
		
		UtilGtk.ComboUpdate(combo_camera, devices, "");
		hbox_combo_camera.PackStart(combo_camera, true, true, 0);
		hbox_combo_camera.ShowAll();
		combo_camera.Active = UtilGtk.ComboMakeActive(devices, devices[current]);
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
		
		//if(UtilAll.IsWindows())
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
		button_help_power.Sensitive = checkbutton_power.Active;
	}
	private void on_button_help_power_clicked (object o, EventArgs args) {
		new DialogMessage(Constants.MessageTypes.INFO, 
				Catalog.GetString("On jumps results tab, power is calculated depending on jump type:") + 
				"\n\n" +
				Catalog.GetString("Jumps with TC & TF: Bosco Relative Power (W/Kg)") + 
				"\n" +
				Catalog.GetString("P = 24.6 * (Total time + Flight time) / Contact time") + 
				"\n\n" +
				Catalog.GetString("Jumps without TC: Lewis Peak Power 1974 (W)") + 
				"\n" +
				Catalog.GetString("P = SQRT(4.9) * 9.8 * (body weight+extra weight) * SQRT(jump height in meters)") + 
				"\n\n" +
				Catalog.GetString("If you want to use other formulas, go to Statistics."));
	}
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		PreferencesWindowBox.preferences.Hide();
		PreferencesWindowBox = null;
	}
	
	void on_preferences_delete_event (object o, DeleteEventArgs args)
	{
		//do not hide/exit if copyiing
		if (thread != null && thread.IsAlive)
			args.RetVal = true;
		else {
			PreferencesWindowBox.preferences.Hide();
			PreferencesWindowBox = null;
		}
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
			new DialogMessage(Constants.MessageTypes.WARNING, Constants.DatabaseNotFound);
	}

	void on_check_backup_multimedia_clicked(object o, EventArgs args) {
		check_backup_encoder_tests.Active = check_backup_multimedia.Active;
	}	
	void on_check_backup_encoder_tests_clicked(object o, EventArgs args) {
		check_backup_multimedia.Active = check_backup_encoder_tests.Active;
	}	
	
	
	void on_button_logs_folder_open_clicked (object o, EventArgs args)
	{
		string dir = Util.GetLogsDir();
		Log.WriteLine(dir);
		
		if( ! new System.IO.DirectoryInfo(dir).Exists) {
			try {
				Directory.CreateDirectory (dir);
			} catch {
				new DialogMessage(Constants.MessageTypes.WARNING, 
						Catalog.GetString("Cannot create directory.") + "\n\n" + dir);
				return;
			}
		}
		
		try {
			System.Diagnostics.Process.Start(dir); 
		}
		catch {
			new DialogMessage(Constants.MessageTypes.WARNING, 
					Constants.DirectoryCannotOpen + "\n\n" + dir);
		}
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
			//if encoder_tests or multimedia, then copy the folder. If not checked, then copy only the db file
			if(check_backup_encoder_tests.Active || check_backup_multimedia.Active)
				fileCopy = fc.Filename + Path.DirectorySeparatorChar + "chronojump";
			else
				fileCopy = fc.Filename + Path.DirectorySeparatorChar + "chronojump_copy.db";

			try {
				fc.Hide ();
			
				bool exists = false;
				if(check_backup_encoder_tests.Active || check_backup_multimedia.Active) {
					if(Directory.Exists(fileCopy)) {
						Log.WriteLine(string.Format("Directory {0} exists, created at {1}", 
									fileCopy, Directory.GetCreationTime(fileCopy)));
						exists = true;
					}
				} else {
					if (File.Exists(fileCopy)) {
						Log.WriteLine(string.Format("File {0} exists with attributes {1}, created at {2}", 
									fileCopy, File.GetAttributes(fileCopy), File.GetCreationTime(fileCopy)));
						exists = true;
					}
				}

				if(exists) {
					Log.WriteLine("Overwrite...");
					ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString("Are you sure you want to overwrite: "), "", fileCopy);
					confirmWin.Button_accept.Clicked += new EventHandler(on_overwrite_file_accepted);
				} else {
					//if encoder_tests or multimedia, then copy the folder. If not checked, then copy only the db file
					if(check_backup_encoder_tests.Active || check_backup_multimedia.Active) 
					{
						thread = new Thread(new ThreadStart(copyRecursive));
						GLib.Idle.Add (new GLib.IdleHandler (PulseGTK));
		
						backup_doing_sensitive_start_end(true);	
						thread.Start(); 
					} else {
						File.Copy(fileDB, fileCopy);
					
						string myString = string.Format(Catalog.GetString("Copied to {0}"), fileCopy);
						new DialogMessage(Constants.MessageTypes.INFO, myString);
					}
				}
			} 
			catch {
				string myString = string.Format(Catalog.GetString("Cannot copy to {0} "), fileCopy);
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
			//if encoder_tests or multimedia, then copy the folder. If not checked, then copy only the db file
			if(check_backup_encoder_tests.Active || check_backup_multimedia.Active) {
				Directory.Delete(fileCopy, true);
				thread = new Thread(new ThreadStart(copyRecursive));
				GLib.Idle.Add (new GLib.IdleHandler (PulseGTK));
		
				backup_doing_sensitive_start_end(true);	
				thread.Start(); 
			} else {
				File.Delete(fileCopy);
				File.Copy(fileDB, fileCopy);
						
				fc.Hide ();
				string myString = string.Format(Catalog.GetString("Copied to {0}"), fileCopy);
				new DialogMessage(Constants.MessageTypes.INFO, myString);
			}
		} catch {
			string myString = string.Format(Catalog.GetString("Cannot copy to {0} "), fileCopy);
			new DialogMessage(Constants.MessageTypes.WARNING, myString);
		}
	}
	
	private void copyRecursive() {
		Util.CopyFilesRecursively(new DirectoryInfo(Util.GetParentDir()), new DirectoryInfo(fileCopy));
	}
	
	private bool PulseGTK ()
	{
		if ( ! thread.IsAlive ) {
			Log.Write("dying");
			
			endPulse();

			return false;
		}
	
		pulsebar.Pulse();
		Thread.Sleep (50);
		//Log.Write(thread.ThreadState.ToString());
		return true;
	}

	private void endPulse() {
		pulsebar.Fraction = 1;
		backup_doing_sensitive_start_end(false);
		fc.Hide ();
		string myString = string.Format(Catalog.GetString("Copied to {0}"), fileCopy);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}
	
	private void backup_doing_sensitive_start_end(bool start) 
	{
		hbox_backup_doing.Visible = start;
	
		button_db_backup.Sensitive = ! start;
		button_db_folder_open.Sensitive = ! start;
		
		button_cancel.Sensitive = ! start;
		button_accept.Sensitive = ! start;
	}


	void on_button_accept_clicked (object o, EventArgs args)
	{
		Sqlite.Open();

		SqlitePreferences.Update("digitsNumber", UtilGtk.ComboGetActive(combo_decimals), true);
		SqlitePreferences.Update("showHeight", PreferencesWindowBox.checkbutton_height.Active.ToString(), true);
		SqlitePreferences.Update("showPower", PreferencesWindowBox.checkbutton_power.Active.ToString(), true);
		SqlitePreferences.Update("showInitialSpeed", PreferencesWindowBox.checkbutton_initial_speed.Active.ToString(), true);
		SqlitePreferences.Update("showAngle", PreferencesWindowBox.checkbutton_angle.Active.ToString(), true);
		
		if(PreferencesWindowBox.checkbutton_show_tv_tc_index.Active) {
			SqlitePreferences.Update("showQIndex", PreferencesWindowBox.radiobutton_show_q_index.Active.ToString(), true);
			SqlitePreferences.Update("showDjIndex", PreferencesWindowBox.radiobutton_show_dj_index.Active.ToString(), true);
		} else {
			SqlitePreferences.Update("showQIndex", "False", true);
			SqlitePreferences.Update("showDjIndex", "False", true);
		}
		
		
		SqlitePreferences.Update("askDeletion", PreferencesWindowBox.checkbutton_ask_deletion.Active.ToString(), true);
		SqlitePreferences.Update("weightStatsPercent", PreferencesWindowBox.radio_weight_percent.Active.ToString(), true);
		SqlitePreferences.Update("heightPreferred", PreferencesWindowBox.radio_elevation_height.Active.ToString(), true);
		SqlitePreferences.Update("metersSecondsPreferred", PreferencesWindowBox.radio_speed_ms.Active.ToString(), true);
		
		SqlitePreferences.Update("encoderPropulsive", 
				PreferencesWindowBox.checkbutton_encoder_propulsive.Active.ToString(), true);
		SqlitePreferences.Update("encoderSmoothCon", Util.ConvertToPoint( 
				(double) PreferencesWindowBox.spin_encoder_smooth_con.Value), true);
		SqlitePreferences.Update("videoDevice", UtilGtk.ComboGetActivePos(combo_camera).ToString(), true);
		
		if(PreferencesWindowBox.radio_export_latin.Active)
			SqlitePreferences.Update("CSVExportDecimalSeparator","COMMA", true); 
		else
			SqlitePreferences.Update("CSVExportDecimalSeparator","POINT", true); 
		
		SqlitePreferences.Update("RGraphsTranslate", 
				PreferencesWindowBox.radio_graphs_translate.Active.ToString(), true);
	
		string encoder1RMMethod = "";	
		if(PreferencesWindowBox.radio_encoder_1RM_nonweighted.Active)
			encoder1RMMethod = Constants.Encoder1RMMethod.NONWEIGHTED.ToString();
		else if(PreferencesWindowBox.radio_encoder_1RM_weighted.Active)
			encoder1RMMethod = Constants.Encoder1RMMethod.WEIGHTED.ToString();
		else if(PreferencesWindowBox.radio_encoder_1RM_weighted2.Active)
			encoder1RMMethod = Constants.Encoder1RMMethod.WEIGHTED2.ToString();
		else // (PreferencesWindowBox.radio_encoder_1RM_weighted3.Active)
			encoder1RMMethod = Constants.Encoder1RMMethod.WEIGHTED3.ToString();
		SqlitePreferences.Update("encoder1RMMethod", encoder1RMMethod, true);
	
		Sqlite.Close();
		
		/*
		if(UtilAll.IsWindows()) {
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
