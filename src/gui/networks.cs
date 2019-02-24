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
	[Widget] Gtk.Label label_rfid_wait;
	[Widget] Gtk.Label label_rfid_encoder_wait;

	[Widget] Gtk.Label label_logout_seconds;
	[Widget] Gtk.Label label_logout_seconds_encoder;
	
	//better raspberry controls
	[Widget] Gtk.Box hbox_encoder_capture_extra_mass_no_raspberry;
	[Widget] Gtk.Box hbox_encoder_capture_extra_mass_raspberry;
	[Widget] Gtk.HBox hbox_encoder_im_weights_n;
	
	//config.EncoderNameAndCapture
	[Widget] Gtk.Box hbox_top_person;
	[Widget] Gtk.Box hbox_top_person_encoder;
	[Widget] Gtk.Label label_top_person_name;
	[Widget] Gtk.Label label_top_encoder_person_name;
	[Widget] Gtk.Button button_image_current_person_zoom;
	[Widget] Gtk.Image image_current_person;
	[Widget] Gtk.Button button_contacts_person_change;
	[Widget] Gtk.Button button_encoder_person_change;
	[Widget] Gtk.VBox vbox_encoder_capture_1_or_cont;

	//encoder exercise stuff
	[Widget] Gtk.Label label_encoder_exercise_encoder;
	[Widget] Gtk.VSeparator vseparator_encoder_exercise_encoder;
	[Widget] Gtk.HBox hbox_encoder_exercise_encoder;
	[Widget] Gtk.Button button_encoder_exercise_edit;
	[Widget] Gtk.Button button_encoder_exercise_add;

	//config.EncoderCaptureShowOnlyBars
	[Widget] Gtk.Notebook notebook_encoder_capture_main;
	[Widget] Gtk.VBox vbox_treeview_encoder_at_second_page;

	//encoder ...
	[Widget] Gtk.Button button_encoder_monthly_change_current_session;
	[Widget] Gtk.Button button_encoder_analyze_image_compujump_send_email;

	//runsInterval
	[Widget] Gtk.HBox hbox_runs_interval_compujump;
	[Widget] Gtk.RadioButton radio_run_interval_compujump_5m;
	[Widget] Gtk.RadioButton radio_run_interval_compujump_10m;
	[Widget] Gtk.RadioButton radio_run_interval_compujump_15m;
	[Widget] Gtk.RadioButton radio_run_interval_compujump_20m;

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
	private static bool shouldShowRFIDDisconnected;
	private static bool updatingRFIDGuiStuff;
	private static bool networksRunIntervalCanChangePersonSQLReady;
	private static DateTime startedRFIDWait;
	private bool rfidProcessCancel;
	private bool rfidIsDifferent;
	//private bool compujumpAutologout;
	//private static CompujumpAutologout compujumpAutologout;
	private CompujumpAutologout compujumpAutologout;

	private static RFIDWaitingAdmin rfidWaitingAdmin;

	DialogPersonPopup dialogPersonPopup;
		
	Config configChronojump;
	bool maximizeWindowAtStartDone = false;

	private void configInitRead()
	{
		configChronojump.Read();
		if(
				configChronojump.Compujump &&
				configChronojump.CompujumpServerURL != null &&
				configChronojump.CompujumpServerURL != "" &&
				configChronojump.CompujumpStationID != -1 &&
				configChronojump.CompujumpStationMode != Constants.Menuitem_modes.UNDEFINED
				)
		{
			LogB.Information(configChronojump.Compujump.ToString());
			LogB.Information(configChronojump.CompujumpServerURL);

			//on compujump cannot add/edit persons, do it from server
			frame_persons_top.Visible = false;
			//button_contacts_person_change.Visible = false;
			//button_encoder_person_change.Visible = false;
			//TODO: don't allow edit person on person treeview

			//don't allow to change persons view options
			view_menuitem.Visible = false;

			//dont't show persons_bottom hbox where users can be edited, deleted if persons at lateral is selected on preferences
			vbox_persons_bottom.Visible = false;

			//don't allow to change encoderConfiguration
			label_encoder_exercise_encoder.Visible = false;
			vseparator_encoder_exercise_encoder.Visible = false;
			hbox_encoder_exercise_encoder.Visible = false;

			//don't show encoder 1-cont buttons
			vbox_encoder_capture_1_or_cont.Visible = false;

			//don't allow to edit exercises
			button_encoder_exercise_edit.Visible = false;
			button_encoder_exercise_add.Visible = false;

			if(configChronojump.CompujumpStationMode != Constants.Menuitem_modes.UNDEFINED)
			{
				//select_menuitem_mode_toggled(configChronojump.CompujumpStationMode);
				//better do like this because radiobuttons are not set. TODO: remove radiobuttons checks
				if(configChronojump.CompujumpStationMode == Constants.Menuitem_modes.RUNSINTERVALLIC)
					on_button_selector_start_runs_intervallic_clicked(new object (), new EventArgs());
				else if(configChronojump.CompujumpStationMode == Constants.Menuitem_modes.POWERGRAVITATORY)
					on_button_selector_start_encoder_gravitatory_clicked(new object (), new EventArgs());
				else //if(configChronojump.CompujumpStationMode == Constants.Menuitem_modes.POWERINERTIAL)
					on_button_selector_start_encoder_inertial_clicked(new object (), new EventArgs());

				hbox_runs_interval.Visible = false;
				hbox_runs_interval_compujump.Visible = true;

				menuitem_mode.Visible = false;
				button_menu_outside_menu.Visible = false;
				button_menu_outside_menu1.Visible = false;
			}

			Json.ChangeServerUrl(configChronojump.CompujumpServerURL);

			capturedRFID = "";
			updatingRFIDGuiStuff = false;
			shouldUpdateRFIDGui = false;
			rfidProcessCancel = false;
			networksRunIntervalCanChangePersonSQLReady = true;

			chronopicRegisterUpdate(false);
			if(chronopicRegister != null && chronopicRegister.GetRfidPortName() != "")
			{
				rfid = new RFID(chronopicRegister.GetRfidPortName());
				rfid.FakeButtonChange.Clicked += new EventHandler(rfidChanged);
				rfid.FakeButtonReopenDialog.Clicked += new EventHandler(rfidReopenDialog);
				rfid.FakeButtonDisconnected.Clicked += new EventHandler(rfidDisconnected);

				threadRFID = new Thread (new ThreadStart (RFIDStart));
				GLib.Idle.Add (new GLib.IdleHandler (pulseRFID));

				LogB.ThreadStart();
				threadRFID.Start();
			}

			if(configChronojump.CompujumpAdminEmail != "")
				button_encoder_analyze_image_compujump_send_email.Visible = true;
		}

		if(configChronojump.Exhibition)
			frame_exhibition.Visible = true;

		configDo();
	}
	private void RFIDStart()
	{
		startedRFIDWait = DateTime.MinValue;
		rfidWaitingAdmin = new RFIDWaitingAdmin();
		LogB.Information("networksRI: " + networksRunIntervalCanChangePersonSQLReady.ToString());

		LogB.Information("RFID Start");
		rfid.Start();
	}
	private void rfidChanged(object sender, EventArgs e)
	{
		LogB.Information("at rfidChanged");
		/*
		 * TODO: only if we are not in the middle of capture, or in cont mode without repetitions
		 */
		if(currentSession != null && rfid.Captured != capturedRFID)
		{
			LogB.Information("RFID changed to: " + rfid.Captured);

			if(isCompujumpCapturing ())
			{
				startedRFIDWait = DateTime.Now;
				LogB.Information("... but we are on the middle of capture");
			}
			else {
				if(rfidWaitingAdmin != null && rfidWaitingAdmin.Waiting)
				{
					LogB.Information("Checking if an email has to be sent");
					Person pLocal = SqlitePerson.SelectByRFID(rfid.Captured);
					if(configChronojump.CompujumpUserIsAdmin(pLocal.UniqueID))
						compujumpSendEmail(Constants.CheckFileOp.ENCODER_ANALYZE_SAVE_IMAGE);
					else
						LogB.Information("RFID should be the RFID of AdminID, not this: " + rfid.Captured);
				} else {
					capturedRFID = rfid.Captured;
					rfidIsDifferent = true;
					shouldUpdateRFIDGui = true;
				}
			}
		}
	}

	private void rfidReopenDialog(object sender, EventArgs e)
	{
		if(currentSession != null && rfid.Captured == capturedRFID)
		{
			if(isCompujumpCapturing ())
			{
				startedRFIDWait = DateTime.Now;
				LogB.Information("... but we are on the middle of capture");
			} else {
				rfidIsDifferent = false;
				shouldUpdateRFIDGui = true;
			}
		}
	}

	private void rfidDisconnected(object sender, EventArgs e)
	{
		shouldShowRFIDDisconnected = true;
		rfidProcessCancel = true;
	}

	private void configInitFromPreferences()
	{
		if(configChronojump == null)
			configChronojump = new Config();

		configChronojump.Maximized = preferences.maximized;
		configChronojump.PersonWinHide = preferences.personWinHide;
		configChronojump.EncoderCaptureShowOnlyBars = preferences.encoderCaptureShowOnlyBars;
		configChronojump.EncoderUpdateTreeViewWhileCapturing = ! preferences.encoderCaptureShowOnlyBars;

		configDo();
	}

	private void configDo()
	{
		LogB.Information("Is Compujump?:\n" + configChronojump.Compujump.ToString());
			
		/*
		 * TODO: do an else to any option
		 * is good to do the else here because user can import a configuration at any time 
		 * and things need to be restored to default position in glade
		 *
		 * But note this has to be executed only if it has changed!!
		 */

		//only change maximizez status on start
		if( ! maximizeWindowAtStartDone )
		{
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
			maximizeWindowAtStartDone = true;
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
		//restriction for configured Compujump clients
		//if(configChronojump.Compujump)
		//	hbox_encoder_im_weights_n.Sensitive = false;
		
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

		showPersonsOnTop(configChronojump.PersonWinHide);
		menuitem_view_persons_on_top.Active = configChronojump.PersonWinHide;

		showPersonPhoto(preferences.personPhoto);
		menuitem_view_persons_show_photo.Active = preferences.personPhoto;


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

		label_rfid_wait.Visible = false;
		label_rfid_encoder_wait.Visible = false;
	}

	DialogMessage dialogMessageNotAtServer;
	private bool pulseRFID ()
	{
		if(shouldShowRFIDDisconnected)
		{
			new DialogMessage(Constants.MessageTypes.WARNING, Constants.RFIDDisconnectedMessage);

			if(dialogPersonPopup != null)
				dialogPersonPopup.DestroyDialog();
		}

		if(! threadRFID.IsAlive || rfidProcessCancel)
		{
			label_rfid_wait.Visible = false;
			label_rfid_encoder_wait.Visible = false;

			LogB.ThreadEnding();
			LogB.ThreadEnded();
			return false;
		}

		/*
		 * if we are on the middle of an encoderCapture, just show a wait message
		 * to avoid problems of SQL on encoder capture stuff and on person login stuff
		 */
		TimeSpan span = DateTime.Now - startedRFIDWait;
		if(span.TotalSeconds < 2) {
			label_rfid_wait.Visible = true;
			label_rfid_encoder_wait.Visible = true;
		} else {
			label_rfid_wait.Visible = false;
			label_rfid_encoder_wait.Visible = false;
		}

		if(isCompujumpCapturing ()) {
			Thread.Sleep (100);
			return true;
		}

		//---- end of checking if we are on the middle of capture.

		//to send email with analyzed image to the admin
		if(rfidWaitingAdmin != null)
		{
			if(rfidWaitingAdmin.ShowGuiMessage)
			{
				new DialogMessage(Constants.MessageTypes.INFO,
						"Sent Email to: " + configChronojump.CompujumpAdminEmail);
				rfidWaitingAdmin = new RFIDWaitingAdmin();
			}

			if(rfidWaitingAdmin.Waiting)
			{
				if(rfidWaitingAdmin.RestSeconds() != -1) {
					label_rfid_encoder_wait.Text =
						string.Format("Please, identify with admin ID wristband before {0} seconds", rfidWaitingAdmin.RestSeconds());
					label_rfid_encoder_wait.Visible = true;
				} else {
					rfidWaitingAdmin = new RFIDWaitingAdmin();
					label_rfid_encoder_wait.Text = "";
					label_rfid_encoder_wait.Visible = false;
				}
				Thread.Sleep (100);
				return true;
			} else
				label_rfid_encoder_wait.Text = "";
		}


		//don't allow this method to be called again until ended
		//Note RFID detection can send many cards (the same) per second
		if(updatingRFIDGuiStuff) {
			Thread.Sleep (100);
			return true;
		}

		if(! shouldUpdateRFIDGui) {
			Thread.Sleep (100);
			return true;
		}

		shouldUpdateRFIDGui = false;
		updatingRFIDGuiStuff = true;

		//is we are on analyze, switch to capture
		if(! radio_mode_encoder_capture_small.Active)
			radio_mode_encoder_capture_small.Active = true;

		/*
		 * This method is shown on diagrams/processes/rfid-local-read.dia
		 */

		bool currentPersonWasNull = (currentPerson == null);
		bool pChangedShowTasks = false;
		Json json = new Json();

		//select person by RFID
		Person pLocal = SqlitePerson.SelectByRFID(capturedRFID);
		if(pLocal.UniqueID == -1)
		{
			LogB.Information("RFID person does not exist locally!!");

			Person pServer = json.GetPersonByRFID(capturedRFID);

			if(! json.Connected) {
				LogB.Information("Server is disconnected!");
				if(dialogMessageNotAtServer == null || ! dialogMessageNotAtServer.Visible)
				{
					dialogMessageNotAtServer = new DialogMessage(
							Constants.MessageTypes.WARNING,
							Constants.ServerDisconnectedMessage
							); //GTK

					compujumpPersonLogoutDo();
				}
			}
			else if(pServer.UniqueID == -1) {
				LogB.Information("Person NOT found on server!");
				if(dialogMessageNotAtServer == null || ! dialogMessageNotAtServer.Visible)
				{
					dialogMessageNotAtServer = new DialogMessage(Constants.MessageTypes.WARNING, Constants.RFIDNotInServerMessage); //GTK

					compujumpPersonLogoutDo();
				}
			}
			else {
				LogB.Information("Person found on server!");

				//personID exists at local DB?
				//check if this uniqueID already exists on local database (would mean RFID changed on server)
				pLocal = SqlitePerson.Select(false, pServer.UniqueID);

				if(! json.LastPersonWasInserted)
				{
					/*
					 * id exists locally, RFID has changed. Changed locally
					 * Note server don't allow having an rfid of a previous person. Must be historically unique.
					 */

					pLocal.Future1 = pServer.Future1;
					SqlitePerson.Update(pLocal);
				}

				currentPerson = pLocal;
				insertAndAssignPersonSessionIfNeeded(json);

				if(json.LastPersonWasInserted)
				{
					string imageFile = json.LastPersonByRFIDImageURL;
					if(imageFile != null && imageFile != "")
					{
						string image_dest = Util.GetPhotoFileName(false, currentPerson.UniqueID);
						if(UtilMultimedia.GetImageType(imageFile) == UtilMultimedia.ImageTypes.PNG)
							image_dest = Util.GetPhotoPngFileName(false, currentPerson.UniqueID);

						bool downloaded = json.DownloadImage(json.LastPersonByRFIDImageURL, currentPerson.UniqueID);
						if(downloaded)
							File.Copy(
									Path.Combine(Path.GetTempPath(), currentPerson.UniqueID.ToString()),
									image_dest,
									true); //overwrite
					}

					person_added(); //GTK
				} else {
					personChanged(); //GTK
					label_person_change();
				}
				pChangedShowTasks = true;
			}
		}
		else {
			LogB.Information("RFID person exists locally!!");
			if(rfidIsDifferent || dialogPersonPopup == null || ! dialogPersonPopup.Visible)
			{
				currentPerson = pLocal;
				insertAndAssignPersonSessionIfNeeded(json);

				personChanged(); //GTK
				label_person_change();
				pChangedShowTasks = true;
			}
		}

		//----- Start upload temp tests
		//select UploadTemp tests (have not been uploaded by network errors)

		Sqlite.Open(); // ---------------->

		List<UploadEncoderDataFullObject> listEncoderTemp = SqliteJson.SelectTempEncoder(true);
		List<UploadSprintDataObject> listSprintTemp = SqliteJson.SelectTempSprint(true);

		//Upload them
		if(listEncoderTemp.Count > 0)
		{
			foreach(UploadEncoderDataFullObject uedfo in listEncoderTemp)
			{
				bool success = json.UploadEncoderData(uedfo);
				LogB.Information(json.ResultMessage);
				if(success)
					SqliteJson.DeleteTempEncoder(true, uedfo.uniqueId); //delete the record
			}
		}
		if(listSprintTemp.Count > 0)
		{
			foreach(UploadSprintDataObject usdo in listSprintTemp)
			{
				bool success = json.UploadSprintData(usdo);
				LogB.Information(json.ResultMessage);
				if(success)
					SqliteJson.DeleteTempSprint(true, usdo.uniqueId); //delete the record
			}
		}

		Sqlite.Close(); // <----------------

		//----- End upload temp tests


		if(currentPerson != null && currentPersonWasNull)
			sensitiveGuiYesPerson();

		if(pChangedShowTasks)
		{
			compujumpAutologout = new CompujumpAutologout();

			/*TODO:
			int rowToSelect = myTreeViewPersons.FindRow(currentPerson.UniqueID);
			if(rowToSelect != -1)
				selectRowTreeView_persons(treeview_persons, rowToSelect);
			*/
			getTasksExercisesAndPopup();

			//load current session if MONTHLY and current session is not current month and currentPerson is not compumpAdminID
			compujumpPersonChangedShouldChangeSession();
		}

		//Wakeup screen if it's off
		Networks.WakeUpRaspberryIfNeeded();

		updatingRFIDGuiStuff = false;

		Thread.Sleep (100);
		//LogB.Information(" threadRFID:" + threadRFID.ThreadState.ToString());

		return true;
	}

	//load current session if MONTHLY and current session is not current month and currentPerson is not compumpAdminID
	private void compujumpPersonChangedShouldChangeSession()
	{
		string yearMonthStr = UtilDate.GetCurrentYearMonthStr();
		if(
				configChronojump.SessionMode == Config.SessionModeEnum.MONTHLY &&
				currentSession.Name != yearMonthStr &&
				! configChronojump.CompujumpUserIsAdmin(currentPerson) )
		{
			currentSession = SqliteSession.SelectByName(yearMonthStr);
			on_load_session_accepted();
		}
	}

	private void insertAndAssignPersonSessionIfNeeded(Json json)
	{
		PersonSession ps = SqlitePersonSession.Select(false, currentPerson.UniqueID, currentSession.UniqueID);
		if(ps.UniqueID == -1)
			currentPersonSession = new PersonSession (
					currentPerson.UniqueID, currentSession.UniqueID,
					json.LastPersonByRFIDHeight, json.LastPersonByRFIDWeight,
					Constants.SportUndefinedID,
					Constants.SpeciallityUndefinedID,
					Constants.LevelUndefinedID,
					"", false); //comments, dbconOpened
		else
			currentPersonSession = ps;
	}

	private void on_button_person_popup_clicked (object o, EventArgs args)
	{
		if(currentPerson == null)
			return;

		//update login time
		if(compujumpAutologout != null)
			compujumpAutologout.UpdateLoginTime();

		getTasksExercisesAndPopup();
	}

	private void getTasksExercisesAndPopup()
	{
		//1) get tasks
		Json json = new Json();
		List<Task> tasks = json.GetTasks(currentPerson.UniqueID, configChronojump.CompujumpStationID);

		//2) get exercises and insert if needed (only on encoder)
		if(configChronojump.CompujumpStationMode == Constants.Menuitem_modes.POWERGRAVITATORY ||
				configChronojump.CompujumpStationMode == Constants.Menuitem_modes.POWERINERTIAL)
		{
			ArrayList encoderExercisesOnLocal = SqliteEncoder.SelectEncoderExercises(false, -1, false);
			List<EncoderExercise> exRemote_list = json.GetStationExercises(configChronojump.CompujumpStationID);

			foreach(EncoderExercise exRemote in exRemote_list)
			{
				bool found = false;
				foreach(EncoderExercise exLocal in encoderExercisesOnLocal)
					if(exLocal.uniqueID == exRemote.uniqueID)
					{
						found = true;
						break;
					}

				if(! found)
				{
					SqliteEncoder.InsertExercise(
							false, exRemote.uniqueID, exRemote.name, exRemote.percentBodyWeight,
							"", "", ""); //ressitance, description, speed1RM
					updateEncoderExercisesGui(exRemote.name);
				}
			}
		}

		//3) get other stationsCount
		List<StationCount> stationsCount = json.GetOtherStationsWithPendingTasks(currentPerson.UniqueID, configChronojump.CompujumpStationID);

		//4) show dialog
		showDialogPersonPopup(tasks, stationsCount, json.Connected);
	}

	private void showDialogPersonPopup(List<Task> tasks, List<StationCount> stationsCount, bool serverConnected)
	{
		if(dialogPersonPopup != null)
			dialogPersonPopup.DestroyDialog();

		if(dialogMessageNotAtServer != null && dialogMessageNotAtServer.Visible)
			dialogMessageNotAtServer.on_close_button_clicked(new object(), new EventArgs());

		dialogPersonPopup = new DialogPersonPopup(
				currentPerson.UniqueID, currentPerson.Name, capturedRFID, tasks, stationsCount,
				serverConnected, compujumpAutologout.Active);

		dialogPersonPopup.Fake_button_start_task.Clicked -= new EventHandler(compujumpTaskStart);
		dialogPersonPopup.Fake_button_start_task.Clicked += new EventHandler(compujumpTaskStart);

		dialogPersonPopup.Fake_button_person_logout.Clicked -= new EventHandler(compujumpPersonLogout);
		dialogPersonPopup.Fake_button_person_logout.Clicked += new EventHandler(compujumpPersonLogout);

		dialogPersonPopup.Fake_button_person_autologout_changed.Clicked -= new EventHandler(compujumpPersonAutoLogoutChanged);
		dialogPersonPopup.Fake_button_person_autologout_changed.Clicked += new EventHandler(compujumpPersonAutoLogoutChanged);
	}

	private void compujumpTaskStart(object o, EventArgs args)
	{
		dialogPersonPopup.Fake_button_start_task.Clicked -= new EventHandler(compujumpTaskStart);
		Task task = new Task();
		if(dialogPersonPopup != null)
		{
			task = dialogPersonPopup.TaskActive;
		}
		dialogPersonPopup.DestroyDialog();
		LogB.Information("Selected task from gui/networks.cs:" + task.ToString());

		if(configChronojump.CompujumpStationMode == Constants.Menuitem_modes.RUNSINTERVALLIC)
			compujumpTaskStartRunInterval(task);
		else
			compujumpTaskStartEncoder(task);
	}

	private void on_run_interval_compujump_type_toggled(object o, EventArgs args)
	{
		RadioButton radio = o as RadioButton;
		if (o == null)
			return;

		if(radio == radio_run_interval_compujump_5m)
			combo_select_runs_interval.Active = 0;
		else if(radio == radio_run_interval_compujump_10m)
			combo_select_runs_interval.Active = 1;
		else if(radio == radio_run_interval_compujump_15m)
			combo_select_runs_interval.Active = 2;
		else //if(radio == radio_run_interval_compujump_20m)
			combo_select_runs_interval.Active = 3;
	}

	private void compujumpTaskStartRunInterval(Task task)
	{
		if(task.ExerciseName == "5 m" || task.ExerciseName == "05 m")
			radio_run_interval_compujump_5m.Active = true;
		else if(task.ExerciseName == "10 m")
			radio_run_interval_compujump_10m.Active = true;
		else if(task.ExerciseName == "15 m")
			radio_run_interval_compujump_15m.Active = true;
		else // if(task.ExerciseName == "20 m")
			radio_run_interval_compujump_20m.Active = true;

		on_button_execute_test_clicked(new object(), new EventArgs());
	}

	private void compujumpTaskStartEncoder(Task task)
	{
		combo_encoder_exercise_capture.Active = UtilGtk.ComboMakeActive(combo_encoder_exercise_capture, task.ExerciseName);

		//laterality
		if(task.Laterality == "RL")
			radio_encoder_laterality_both.Active = true;
		else if(task.Laterality == "R")
			radio_encoder_laterality_r.Active = true;
		else if(task.Laterality == "L")
			radio_encoder_laterality_l.Active = true;

		Pixbuf pixbuf;
		if(task.Load > 0)
			spin_encoder_extra_weight.Value = Convert.ToDouble(task.Load);

		if(task.Speed > 0) {
			repetitiveConditionsWin.EncoderMeanSpeedHigherValue = task.Speed;
			repetitiveConditionsWin.EncoderMeanSpeedHigher = true;
			repetitiveConditionsWin.EncoderMeanSpeedLowerValue = task.Speed / 2;
			repetitiveConditionsWin.EncoderMeanSpeedHigher = true;
			repetitiveConditionsWin.Encoder_show_manual_feedback = true;
			repetitiveConditionsWin.Notebook_encoder_conditions_page = 1; //speed
			pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_bell_active.png");
		} else {
			repetitiveConditionsWin.EncoderMeanSpeedHigher = false;
			repetitiveConditionsWin.EncoderMeanSpeedLower = false;
			repetitiveConditionsWin.Encoder_show_manual_feedback = false;
			pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_bell_none.png");
		}
		image_encoder_bell.Pixbuf = pixbuf;

		//start test
		on_button_encoder_capture_clicked (new object(), new EventArgs ());
	}

	private void compujumpPersonLogout(object o, EventArgs args)
	{
		compujumpPersonLogoutDo();
	}
	private void compujumpPersonLogoutDo()
	{
		if(dialogPersonPopup != null)
		{
			dialogPersonPopup.Fake_button_person_logout.Clicked -= new EventHandler(compujumpPersonLogout);
			dialogPersonPopup.DestroyDialog();
		}

		currentPerson = null;
		currentPersonSession = null;
		sensitiveGuiNoPerson ();
	}

	private void compujumpPersonAutoLogoutChanged(object o, EventArgs args)
	{
		compujumpAutologout.Active = dialogPersonPopup.Autologout;
	}

	//are we capturing runInterval or encoder?
	private bool isCompujumpCapturing ()
	{
		if(compujumpAutologout == null)
			return false;

		return(compujumpAutologout.IsCompujumpCapturing());
	}

	private void compujumpSendEmail(Constants.CheckFileOp op)
	{
		if(op == Constants.CheckFileOp.ENCODER_ANALYZE_SAVE_IMAGE)
		{
			LogB.Information("Send Email to: " + configChronojump.CompujumpAdminEmail);
			rfidWaitingAdmin.ShowGuiMessage = true;
		}
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

//Class for manage autologout on Compujump
//TODO: expand this class to manage better resttime and clearer
public class CompujumpAutologout
{
	public bool Active;
	public DateTime lastEncoderAnalyzeTime;

	private bool capturingRunInterval;
	private bool capturingEncoder;

	private DateTime loginTime;
	private DateTime lastRunIntervalTime;
	private DateTime lastEncoderCaptureTime;
	private int logoutMinutes = 3;

	public CompujumpAutologout ()
	{
		Active = true;
		loginTime = DateTime.Now;

		capturingRunInterval = false;
		capturingEncoder = false;

		lastRunIntervalTime = DateTime.MinValue;
		lastEncoderCaptureTime = DateTime.MinValue;
		lastEncoderAnalyzeTime = DateTime.MinValue;
	}

	public void UpdateLoginTime()
	{
		loginTime = DateTime.Now;
	}

	public void StartCapturingRunInterval()
	{
		capturingRunInterval = true;
	}
	public void EndCapturingRunInterval()
	{
		capturingRunInterval = false;
		lastRunIntervalTime = DateTime.Now;
	}

	public void StartCapturingEncoder()
	{
		capturingEncoder = true;
	}
	public void EndCapturingEncoder()
	{
		capturingEncoder = false;
		lastEncoderCaptureTime = DateTime.Now;
	}

	public void UpdateLastEncoderAnalyzeTime()
	{
		lastEncoderAnalyzeTime = DateTime.Now;
	}

	public bool IsCompujumpCapturing()
	{
		return (capturingRunInterval || capturingEncoder);
	}

	//mSinceLogin: time in 'm'inutes since login
	private double mSinceLogin()
	{
		return DateTime.Now.Subtract(loginTime).TotalMinutes;
	}
	private double mSinceRunInterval()
	{
		return DateTime.Now.Subtract(lastRunIntervalTime).TotalMinutes;
	}
	private double mSinceEncoderCapture()
	{
		return DateTime.Now.Subtract(lastEncoderCaptureTime).TotalMinutes;
	}
	private double mSinceEncoderAnalyze()
	{
		return DateTime.Now.Subtract(lastEncoderAnalyzeTime).TotalMinutes;
	}

	//decide if use has to be autologout now
	public bool ShouldILogoutNow()
			//bool moreThanThreeMinutesSinceLastCapture
	{
		if(IsCompujumpCapturing())
			return false;

		if(Active &&
				mSinceLogin() >= logoutMinutes &&
	//			moreThanThreeMinutesSinceLastCapture &&
				mSinceRunInterval() >= logoutMinutes &&
				mSinceEncoderCapture() >= logoutMinutes &&
				mSinceEncoderAnalyze() >= logoutMinutes
				)
			return true;

		return false;
	}

	private double sSinceLogin()
	{
		return DateTime.Now.Subtract(loginTime).TotalSeconds;
	}
	private double sSinceRunInterval()
	{
		return DateTime.Now.Subtract(lastRunIntervalTime).TotalSeconds;
	}
	private double sSinceEncoderCapture()
	{
		return DateTime.Now.Subtract(lastEncoderCaptureTime).TotalSeconds;
	}
	private double sSinceEncoderAnalyze()
	{
		return DateTime.Now.Subtract(lastEncoderAnalyzeTime).TotalSeconds;
	}
	public int RemainingSeconds ()
	{
		int logoutDefaultS = logoutMinutes * 60;
		double seconds = 0;

		if(sSinceLogin() > seconds)
			seconds = logoutDefaultS - sSinceLogin();

		if(lastRunIntervalTime > DateTime.MinValue && logoutDefaultS - sSinceRunInterval() > seconds)
			seconds = logoutDefaultS - sSinceRunInterval();

		if(lastEncoderCaptureTime > DateTime.MinValue && logoutDefaultS - sSinceEncoderCapture() > seconds)
			seconds = logoutDefaultS - sSinceEncoderCapture();

		if(lastEncoderAnalyzeTime > DateTime.MinValue && logoutDefaultS - sSinceEncoderAnalyze() > seconds)
			seconds = logoutDefaultS - sSinceEncoderAnalyze();

		return Convert.ToInt32(seconds);
	}

	//showAll is for debug, user will see only one value
	//TODO: separate between minutes and seconds and only display when remaining 10 seconds
	public string RemainingSecondsOld(bool showAll)
	{
		if(! Active)
			return "";

		double elapsed = logoutMinutes - mSinceLogin();

		string strAll = "Logout in (minutes): " + string.Format("login: {0}", Math.Round(elapsed, 2));
		double maxTime = elapsed;

		if(lastRunIntervalTime > DateTime.MinValue)
		{
			elapsed = logoutMinutes - mSinceRunInterval();
			strAll += string.Format("; lastRunI: {0}", Math.Round(elapsed, 2));
			if(elapsed > maxTime)
			       maxTime = elapsed;
		}

		if(lastEncoderCaptureTime > DateTime.MinValue)
		{
			elapsed = logoutMinutes - mSinceEncoderCapture();
			strAll += string.Format("; lastECapture {0}", Math.Round(elapsed, 2));
			if(elapsed > maxTime)
			       maxTime = elapsed;
		}

		if(lastEncoderAnalyzeTime > DateTime.MinValue)
		{
			elapsed = logoutMinutes - mSinceEncoderAnalyze();
			strAll += string.Format("; lastEAnalyze: {0}", Math.Round(elapsed, 2));
			if(elapsed > maxTime)
			       maxTime = elapsed;
		}

		if(showAll)
			return strAll;
		else
			return "Logout in\n" + Math.Round(maxTime, 2) + " min.";
	}
}

//this class manages the wating of admin wristband in tasks like sending an email with the analyze image
public class RFIDWaitingAdmin
{
	public bool Waiting;
	public bool ShowGuiMessage;

	private static DateTime startedDT; //time to wait ID of the admin to have access to "higher" methods
	private const int secondsLimit = 5; //5 seconds to use the admin ID

	//constructor
	public RFIDWaitingAdmin()
	{
		Waiting = false;
		ShowGuiMessage = false;
		startedDT = DateTime.MinValue;
		LogB.Information("RFIDWaitingAdmin constructed!");
	}

	public void Start()
	{
		startedDT = DateTime.Now; //start countdown
		Waiting = true;
	}

	//we have to capture before this time in seconds
	public int RestSeconds()
	{
		TimeSpan span = DateTime.Now - startedDT;
		if (Waiting && span.TotalSeconds < secondsLimit)
			return Convert.ToInt32(Math.Round(secondsLimit - span.TotalSeconds, 0));
		else
			return -1;
	}
}
