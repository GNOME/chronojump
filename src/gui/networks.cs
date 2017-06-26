/*
 * This file is part of ChronoJump
 *
 * Chronojump is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or   
 *    (at your option) any later version.
 *    
 * Chronojump is distributed in the hope that it will be useful,
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
using Gdk;
using Glade;
using System.IO.Ports;
using System.IO; //"File" things
using System.Collections; //ArrayList
using System.Collections.Generic; //List
using System.Diagnostics; //Process
using System.Threading;
	
public partial class ChronoJumpWindow 
{
	//custom buttons
	[Widget] Gtk.HBox hbox_encoder_analyze_signal_or_curves;
	[Widget] Gtk.VBox vbox_start_window_main;
	[Widget] Gtk.VBox vbox_start_window_sub;
	[Widget] Gtk.Alignment alignment_start_window;
	[Widget] Gtk.Alignment alignment_encoder_capture_options;
			
	//RFID
	[Widget] Gtk.Label label_rfid_contacts;
	[Widget] Gtk.Label label_rfid_encoder;
	
	//better raspberry controls
	[Widget] Gtk.Entry entry_raspberry_extra_weight;
	[Widget] Gtk.Box hbox_encoder_capture_extra_mass_no_raspberry;
	[Widget] Gtk.Box hbox_encoder_capture_extra_mass_raspberry;
	[Widget] Gtk.HBox hbox_encoder_im_weights_n;
	
	//config.EncoderNameAndCapture
	[Widget] Gtk.Box hbox_top_person;
	[Widget] Gtk.Box hbox_top_person_encoder;
	[Widget] Gtk.Label label_top_person_name;
	[Widget] Gtk.Label label_top_encoder_person_name;
	[Widget] Gtk.Button button_contacts_person_change;
	[Widget] Gtk.Button button_encoder_person_change;

	//encoder exercise stuff
	[Widget] Gtk.Button button_encoder_exercise_edit;
	[Widget] Gtk.Button button_encoder_exercise_add;

	//config.EncoderCaptureShowOnlyBars
	[Widget] Gtk.Notebook notebook_encoder_capture_main;
	[Widget] Gtk.VBox vbox_treeview_encoder_at_second_page;

	//shown when menu is hidden
	[Widget] Gtk.HBox hbox_menu_and_preferences_outside_menu_contacts;
	[Widget] Gtk.HBox hbox_menu_and_preferences_outside_menu_encoder;
	[Widget] Gtk.Button button_menu_outside_menu;
	[Widget] Gtk.Button button_menu_outside_menu1;

	private enum linuxTypeEnum { NOTLINUX, LINUX, RASPBERRY, NETWORKS }
	private bool encoderUpdateTreeViewWhileCapturing = true;

	static Thread threadRFID;
	public RFID rfid;
	private static string capturedRFID;
	private static bool shouldUpdateRFIDGui;
	private static bool updatingRFIDGuiStuff;
	private bool rfidProcessCancel;

	DialogPersonPopup dialogPersonPopup;
		
	Config configChronojump;
	private void configInitRead()
	{
		configChronojump.Read();
		if(
				configChronojump.Compujump &&
				configChronojump.CompujumpServerURL != null &&
				configChronojump.CompujumpServerURL != "")
		{
			LogB.Information(configChronojump.Compujump.ToString());
			LogB.Information(configChronojump.CompujumpServerURL);

			//on compujump cannot add/edit persons, do it from server
			frame_persons_top.Visible = false;
			//button_contacts_person_change.Visible = false;
			//button_encoder_person_change.Visible = false;
			//TODO: don't allow edit person on person treeview

			button_encoder_exercise_edit.Visible = false;
			button_encoder_exercise_add.Visible = false;

			if(configChronojump.CompujumpStationMode != Constants.Menuitem_modes.UNDEFINED)
			{
				select_menuitem_mode_toggled(configChronojump.CompujumpStationMode);
				menuitem_mode.Visible = false;
				button_menu_outside_menu.Visible = false;
				button_menu_outside_menu1.Visible = false;
			}

			Json.ChangeServerUrl(configChronojump.CompujumpServerURL);

			capturedRFID = "";
			updatingRFIDGuiStuff = false;
			shouldUpdateRFIDGui = false;
			rfidProcessCancel = false;

			chronopicRegisterUpdate(false);
			if(chronopicRegister != null && chronopicRegister.GetRfidPortName() != "")
			{
				rfid = new RFID(chronopicRegister.GetRfidPortName());
				rfid.FakeButtonChange.Clicked += new EventHandler(rfidChanged);

				threadRFID = new Thread (new ThreadStart (RFIDStart));
				GLib.Idle.Add (new GLib.IdleHandler (pulseRFID));

				LogB.ThreadStart();
				threadRFID.Start();
			}
		}

		configDo();
	}
	private void RFIDStart()
	{
		LogB.Information("RFID Start");
		rfid.Start();
		//rfid.ChangedEvent += new EventHandler(this.rfidChanged);
	}
	private void rfidChanged(object sender, EventArgs e)
	{
		/*
		 * TODO: only if we are not in the middle of capture, or in cont mode without repetitions
		 */
		if(rfid.Captured != capturedRFID && currentSession != null)
		{
			LogB.Information("RFID changed to: " + rfid.Captured);

			capturedRFID = rfid.Captured;
			shouldUpdateRFIDGui = true;
		} else
			LogB.Information("RFID doesn't change");
	}

	private void configInitFromPreferences()
	{
		configChronojump = new Config();

		configChronojump.Maximized = preferences.maximized;
		configChronojump.PersonWinHide = preferences.personWinHide;
		configChronojump.EncoderCaptureShowOnlyBars = preferences.encoderCaptureShowOnlyBars;

		configDo();
	}

	private void configDo()
	{
		LogB.Information("Config:\n" + configChronojump.ToString());
			
		/*
		 * TODO: do an else to any option
		 * is good to do the else here because user can import a configuration at any time 
		 * and things need to be restored to default position in glade
		 *
		 * But note this has to be executed only if it has changed!!
		 */

		if(configChronojump.Maximized == Preferences.MaximizedTypes.NO)
		{
			app1.Unmaximize();
			app1.Decorated = true;
			button_start_quit.Visible = false;
		} else {
			app1.Decorated = (configChronojump.Maximized != Preferences.MaximizedTypes.YESUNDECORATED);
			app1.Maximize();
			button_start_quit.Visible = ! app1.Decorated;
		}

		if(configChronojump.CustomButtons)
		{
			//---- start window ----

			vbox_start_window_main.Spacing = 0;
			vbox_start_window_sub.Spacing = 0;
			alignment_start_window.TopPadding = 0;
			alignment_start_window.BottomPadding = 2;
			alignment_encoder_capture_options.TopPadding = 0;
			alignment_encoder_capture_options.BottomPadding = 0;
			
			//---- capture tab ----
			
			hbox_encoder_capture_extra_mass_no_raspberry.Visible = false;
			hbox_encoder_capture_extra_mass_raspberry.Visible = true;
		
			button_encoder_select.HeightRequest = 40;
			//this will make all encoder capture controls taller	
			button_encoder_capture.SetSizeRequest(125,60);

			spin_encoder_im_weights_n.Visible = false;
			hbox_encoder_im_weights_n.Visible = true;
			
			//---- analyze tab ----

			hbox_encoder_analyze_signal_or_curves.HeightRequest = 40;
			button_encoder_analyze.SetSizeRequest(120,40);
		}
		if(! configChronojump.UseVideo) {
			alignment_video_encoder.Visible = false;
		}
		
		//show only power
		if(configChronojump.OnlyEncoderGravitatory)
			on_button_selector_start_encoder_gravitatory_clicked(new object(), new EventArgs());
		else if(configChronojump.OnlyEncoderInertial)
			on_button_selector_start_encoder_inertial_clicked(new object(), new EventArgs());
		
		if(configChronojump.EncoderCaptureShowOnlyBars)
		{
			//attention: this makes encoder_capture_signal_drawingarea == null
			vpaned_encoder_capture_video_and_set_graph.Visible = false;
			
			vpaned_encoder_main.Remove(alignment_treeview_encoder_capture_curves);
			vbox_treeview_encoder_at_second_page.PackStart(alignment_treeview_encoder_capture_curves);
			notebook_encoder_capture_main.ShowTabs = true;
		} else {
			/*
			 * is good to do the else here because user can import a configuration at any time 
			 * and things need to be restored to default position in glade
			 *
			 * But note this has to be executed only if it has changed!!
			 */
			/*
			notebook_encoder_capture_main.ShowTabs = false;
			vbox_treeview_encoder_at_second_page.Remove(alignment_treeview_encoder_capture_curves);
			vpaned_encoder_main.PackStart(alignment_treeview_encoder_capture_curves);
			*/

			//this change needs chronojump reload
		}
		
		encoderUpdateTreeViewWhileCapturing = configChronojump.EncoderUpdateTreeViewWhileCapturing;
		
		if(configChronojump.PersonWinHide)
		{
			notebook_session_person.Visible = false;
			hbox_top_person.Visible = true;
			hbox_top_person_encoder.Visible = true;
		} else {
			notebook_session_person.Visible = true;
			hbox_top_person.Visible = false;
			hbox_top_person_encoder.Visible = false;
		}
		
		if(configChronojump.EncoderAnalyzeHide) {
			hbox_encoder_sup_capture_analyze_two_buttons.Visible = false;
		}

		if(currentSession == null && //this is going to be called one time because currentSession will change
			       ( configChronojump.SessionMode == Config.SessionModeEnum.UNIQUE || configChronojump.SessionMode == Config.SessionModeEnum.MONTHLY) )
		{
			//main_menu.Visible = false;
			//app1.Decorated = false;
			hbox_menu_and_preferences_outside_menu_contacts.Visible = true;
			hbox_menu_and_preferences_outside_menu_encoder.Visible = true;

			if(configChronojump.SessionMode == Config.SessionModeEnum.UNIQUE)
			{
				if(! Sqlite.Exists(false, Constants.SessionTable, "session")) {
					//this creates the session and inserts at DB
					currentSession = new Session(
							"session", "", DateTime.Today,	//name, place, dateTime
							Constants.SportUndefinedID, Constants.SpeciallityUndefinedID, Constants.LevelUndefinedID,
							"", Constants.ServerUndefinedID); //comments, serverID
				} else
					currentSession = SqliteSession.SelectByName("session");
			} else {
				//configChronojump.SessionMode == Config.SessionModeEnum.MONTHLY

				string yearMonthStr = UtilDate.GetCurrentYearMonthStr();
				LogB.Information("yearMonthStr: " + yearMonthStr);
				if(! Sqlite.Exists(false, Constants.SessionTable, yearMonthStr))
				{
					//this creates the session and inserts at DB
					currentSession = new Session(
							yearMonthStr, "", DateTime.Today,	//name, place, dateTime
							Constants.SportUndefinedID, Constants.SpeciallityUndefinedID, Constants.LevelUndefinedID,
							"", Constants.ServerUndefinedID); //comments, serverID

					//insert personSessions from last month
					string yearLastMonthStr = UtilDate.GetCurrentYearLastMonthStr();
					if(Sqlite.Exists(false, Constants.SessionTable, yearLastMonthStr))
					{
						Session s = SqliteSession.SelectByName(yearLastMonthStr);

						//import all persons from last session
						List<PersonSession> personSessions = SqlitePersonSession.SelectPersonSessionList(s.UniqueID);

						//convert all personSessions to currentSession
						//and nullify UniqueID in order to be inserted incrementally by SQL
						foreach(PersonSession ps in personSessions)
						{
							ps.UniqueID = -1;
							ps.SessionID = currentSession.UniqueID;
						}


						//insert personSessions using a transaction
						new SqlitePersonSessionTransaction(personSessions);
					}
				} else
					currentSession = SqliteSession.SelectByName(yearMonthStr);
			}
			
			on_load_session_accepted();
			sensitiveGuiYesSession();
		}

		//TODO
		//RunScriptOnExit

		/*
		if(linuxType == linuxTypeEnum.NETWORKS) {
			//mostrar directament el power
			select_menuitem_mode_toggled(Constants.Menuitem_modes.POWER);
			
			//no mostrar menu
			main_menu.Visible = false;
			
			//no mostrar persones
			//vbox_persons.Visible = false;
			//TODO: rfid can be here, also machine, maybe weight, other features
			//time, gym, ...

			//show rfid
			label_rfid.Visible = true;

			//to test display, just make sensitive the top controls, but beware there's no session yet and no person
			notebook_sup.Sensitive = true;
			hbox_encoder_sup_capture_analyze.Sensitive = true;
			notebook_encoder_sup.Sensitive = false;
		}
		*/

		//label_rfid_contacts.Visible = (UtilAll.GetOSEnum() == UtilAll.OperatingSystems.LINUX);
		//label_rfid_encoder.Visible = (UtilAll.GetOSEnum() == UtilAll.OperatingSystems.LINUX);
		label_rfid_contacts.Visible = false;
		label_rfid_encoder.Visible = false;
	}

	private bool pulseRFID ()
	{
		if(! threadRFID.IsAlive || rfidProcessCancel)
		{
			LogB.ThreadEnding();
			LogB.ThreadEnded();
			return false;
		}

		//don't allow this method to be called again until ended
		//Note RFID detection can send many cards (the same) per second
		if(updatingRFIDGuiStuff)
			return true;

		if(! shouldUpdateRFIDGui)
			return true;

		shouldUpdateRFIDGui = false;
		updatingRFIDGuiStuff = true;

		//TODO: this pulseRFID need only the GTK stuff, not the rest
		label_rfid_contacts.Text = capturedRFID; //GTK
		label_rfid_encoder.Text = capturedRFID; //GTK

		Person p = SqlitePerson.SelectByRFID(capturedRFID);

		bool currentPersonWasNull = (currentPerson == null);
		bool pChanged = false;
		Json json = new Json();
		if(p.UniqueID == -1)
		{
			LogB.Information("RFID person does not exist!!");

			p = json.GetPersonByRFID(capturedRFID);
			if(p.UniqueID == -1) {
				LogB.Information("Person NOT found on server!");
				new DialogMessage(Constants.MessageTypes.WARNING,
						"Aquesta pulsera o jugador no es troba identificada al servidor"); //GTK
			}
			else {
				LogB.Information("Person found on server!");

				currentPerson = p;
				currentPersonSession = new PersonSession (
						currentPerson.UniqueID, currentSession.UniqueID, 
						json.LastPersonByRFIDHeight, json.LastPersonByRFIDWeight,
						Constants.SportUndefinedID, 
						Constants.SpeciallityUndefinedID, 
						Constants.LevelUndefinedID,
						"", false); //comments, dbconOpened

				if(json.LastPersonByRFIDImageURL != "")
				{
					bool downloaded = json.DownloadImage(json.LastPersonByRFIDImageURL, currentPerson.UniqueID);
					if(downloaded)
						File.Copy(
								Path.Combine(Path.GetTempPath(), currentPerson.UniqueID.ToString()),
								Util.GetPhotoFileName(false, currentPerson.UniqueID),
								true); //overwrite
				}

				person_added(); //GTK
				pChanged = true;
			}
		}
		else {
			LogB.Information("RFID person exists!!");
			currentPerson = p;

			PersonSession ps = SqlitePersonSession.Select(false, p.UniqueID, currentSession.UniqueID);
			if(ps == null)
				currentPersonSession = new PersonSession (
						p.UniqueID, currentSession.UniqueID,
						json.LastPersonByRFIDHeight, json.LastPersonByRFIDWeight,
						Constants.SportUndefinedID,
						Constants.SpeciallityUndefinedID,
						Constants.LevelUndefinedID,
						"", false); //comments, dbconOpened
			else
				currentPersonSession = ps;

			personChanged(); //GTK
			label_person_change();
			pChanged = true;
		}

		if(currentPersonWasNull)
			sensitiveGuiYesPerson();

		if(pChanged)
		{
			/*TODO:
			int rowToSelect = myTreeViewPersons.FindRow(currentPerson.UniqueID);
			if(rowToSelect != -1)
				selectRowTreeView_persons(treeview_persons, rowToSelect);
			*/

			List<Task> tasks = json.GetTasks(currentPerson.UniqueID);
			showDialogPersonPopup(tasks);
		}

		updatingRFIDGuiStuff = false;

		Thread.Sleep (100);
		LogB.Information(" threadRFID:" + threadRFID.ThreadState.ToString());

		return true;
	}

	private void on_button_person_popup_clicked (object o, EventArgs args)
	{
		if(currentPerson == null)
			return;

		Json json = new Json();
		List<Task> tasks = json.GetTasks(currentPerson.UniqueID);
		showDialogPersonPopup(tasks);
	}

	private void showDialogPersonPopup(List<Task> tasks)
	{
		if(dialogPersonPopup != null)
			dialogPersonPopup.DestroyDialog();

		dialogPersonPopup = new DialogPersonPopup(
				currentPerson.UniqueID, currentPerson.Name, capturedRFID, tasks);
	}


	/*
	 *
	 * This code uses a watcher to see changes on a filename
	 * is thought to be run on a raspberry connected to rfid
	 * with a Python program reading rfid and changing that file.
	 *
	 * Now is not used anymore because new code connects by USB to an Arduino that has the RFID
	 */

	/*
	static string updatingRFIDGuiStuffNewRFID;

	private void rfid_test() {
		Networks networks = new Networks();
		networks.Test();
	}

	void on_button_rfid_read_clicked (object o, EventArgs args)
	{
		string filePath = Util.GetRFIDCapturedFile();

		if(Util.FileExists(filePath))
			label_rfid.Text = Util.ReadFile(filePath, true);
	}

	//Process processRFIDcapture;
	void on_button_rfid_start_clicked (object o, EventArgs args)
	{
		rfid_start();
	}

	private void rfid_start()
	{
		string script_path = Util.GetRFIDCaptureScript();

		if(! File.Exists(script_path))
		{
			LogB.Debug ("ExecuteProcess does not exist parameter: " + script_path);
			label_rfid.Text = "Error starting rfid capture";
			return;
		}

		string filePath = Util.GetRFIDCapturedFile();
		Util.FileDelete(filePath);

		//create a Timeout function to show changes on process
		updatingRFIDGuiStuffNewRFID = "";
		updatingRFIDGuiStuff = true;
		GLib.Timeout.Add(1000, new GLib.TimeoutHandler(updateRFIDGuiStuff));

		// ---- start process ----
		//
		// on Windows will be different, but at the moment RFID is only supported on Linux (Raspberrys)
		// On Linux and OSX we execute Python and we pass the path to the script as a first argument

		string executable = "python";         // TODO: check if ReadChronojump.py works on Python 2 and Python 3

		List<string> parameters = new List <string> ();
		// first argument of the Python: the path to the script
		parameters.Insert (0, script_path);


		processRFIDcapture = new Process();
		bool calledOk = ExecuteProcess.RunAtBackground (processRFIDcapture, executable, parameters);
		if(calledOk) {
			button_rfid_start.Sensitive = false;
			label_rfid.Text = "...";
		} else
			label_rfid.Text = "Error starting rfid capture";

		// ----- process is launched

		//create a new FileSystemWatcher and set its properties.
		FileSystemWatcher watcher = new FileSystemWatcher();
		watcher.Path = Path.GetDirectoryName(filePath);
		watcher.Filter = Path.GetFileName(filePath);

		//add event handlers.
		watcher.Changed += new FileSystemEventHandler(rfid_watcher_changed);

		//start watching
		watcher.EnableRaisingEvents = true;

		//also perform an initial search
		rfid_read();
	}

	private void rfid_watcher_changed(object source, FileSystemEventArgs e)
	{
		LogB.Information("WATCHER");
		rfid_read();
		rfid_reading = false;

		//if compujump, wakeup screen if it's off
		//if(configChronojump.Compujump == true)
		//	Networks.WakeUpRaspberryIfNeeded();
	}

	bool rfid_reading = false;
	private void rfid_read()
	{
		//to avoid being called too continuously by watcher
		if(rfid_reading)
			return;

		rfid_reading = true;

		string filePath = Util.GetRFIDCapturedFile();

		LogB.Information("Changed file: " +  filePath);

		if(Util.FileExists(filePath))
		{
			string rfid = Util.ReadFile(filePath, true);
			if(rfid != "")
			{
				updatingRFIDGuiStuffNewRFID = rfid;
			}
		}
	}
	*/
}
