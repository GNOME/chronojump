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
 * Copyright (C) 2004-2026   Xavier de Blas <xaviblas@gmail.com>
 */


using System;
using Gtk;
using Gdk;
//using Glade;
using System.IO.Ports;
using System.IO; //"File" things
using System.Collections; //ArrayList
using System.Collections.Generic; //List
using System.Diagnostics; //Process
using System.Threading;
using Mono.Unix;
	
public partial class ChronoJumpWindow 
{
	// at glade ---->
	//custom buttons
	Gtk.HBox hbox_encoder_analyze_signal_or_curves;
	Gtk.VBox vbox_start_window_main;
	Gtk.VBox vbox_start_window_sub;
	Gtk.Alignment alignment_start_window;
	Gtk.Alignment alignment_encoder_capture_options;
	Gtk.Button button_contacts_devices_networks_problems;

	//RFID
	Gtk.Label label_rfid_wait;

	Gtk.Label label_logout_seconds;

	//better raspberry controls
	Gtk.HBox hbox_sessions_raspberry;
	Gtk.HBox hbox_persons_raspberry;
	Gtk.Box hbox_encoder_capture_extra_mass_no_raspberry;
	Gtk.Box hbox_encoder_capture_extra_mass_raspberry;
	
	//config.EncoderNameAndCapture
	Gtk.HBox hbox_top_person;
	Gtk.Label label_top_person_name;
	Gtk.Image image_current_person;
	Gtk.Button button_contacts_person_change;
	Gtk.Button button_networks_contacts_guest;

	//encoder exercise stuff
	Gtk.Label label_encoder_exercise_encoder;
	//Gtk.VSeparator vseparator_encoder_exercise_encoder;
	Gtk.HBox hbox_encoder_exercise_encoder;
	Gtk.Button button_encoder_exercise_edit;
	Gtk.Button button_encoder_exercise_add;
	Gtk.Button button_encoder_exercise_delete;

	//encoder ...
	Gtk.CheckButton check_encoder_networks_upload;
	Gtk.Button button_encoder_monthly_change_current_session;
	Gtk.Button button_encoder_analyze_image_compujump_send_email;
	/*
	Gtk.Label label_RFID_disconnected;
	Gtk.Label label_chronopic_encoder;
	Gtk.Image image_chronopic_encoder_no;
	Gtk.Image image_chronopic_encoder_yes;
	*/
	Gtk.HBox hbox_encoder_disconnected;
	Gtk.HBox hbox_RFID_disconnected;
	Gtk.Label label_encoder_checked_error;

	//contacts
	Gtk.CheckButton check_contacts_networks_upload;

	//runsInterval
	Gtk.RadioButton radio_run_interval_compujump_5m;
	Gtk.RadioButton radio_run_interval_compujump_10m;
	Gtk.RadioButton radio_run_interval_compujump_15m;
	Gtk.RadioButton radio_run_interval_compujump_20m;

	//shown when menu is hidden
	//Gtk.HBox hbox_menu_and_preferences_outside_menu_contacts;
	//Gtk.HBox hbox_menu_and_preferences_outside_menu_encoder;
	//Gtk.Button button_menu_outside_menu;
	//Gtk.Button button_menu_outside_menu1;
	// <---- at glade

	//private enum linuxTypeEnum { NOTLINUX, LINUX, RASPBERRY, NETWORKS }
	private bool encoderUpdateTreeViewWhileCapturing = true;

	static Thread threadRFID;
	public RFID rfid;
	private static string capturedRFID; //current RFID in use
	private static bool shouldUpdateRFIDGui;
	private static bool shouldShowRFIDDisconnected;
	private static bool sendingJsonRFID; //if connection is very slow, this helps to show a Readed (RFID) message fast and then the connection will be done
	private static bool networksRunIntervalCanChangePersonSQLReady;
	private static DateTime startedRFIDWait; //just to display a message at the moment of try to wristband change while capturing
	private bool rfidProcessCancel;
	private bool rfidIsDifferent; //new rfid capture is different than previous
	private bool needManageCompujumpCapturingReset; //useful to manage wristbands while capture
	//private bool compujumpAutologout;
	//private static CompujumpAutologout compujumpAutologout;
	private CompujumpAutologout compujumpAutologout;

	private static RFIDWaitingAdminGuiObjects rfidWaitingAdminGuiObjects;

	RemoteTest remoteTest;
	static Thread threadRemoteTest;
	private static bool remoteTestCallNow;
	private static bool remoteTestCancelNow;
	private static string remoteTestTestName;
	RemotePersonNext remotePersonNext; //only one instance

	DialogPersonPopup dialogPersonPopup;
		
	Config configChronojump;
	bool maximizeWindowAtStartDone = false;

	private void configInitDoAtBoot ()
	{
		LogB.Information("readed config: " + configChronojump.PrintDefined ());

		if(configChronojump.CompujumpStationMode != Constants.Modes.UNDEFINED)
		{
			button_show_modes_contacts.Visible = false;

			//button_show_modes_encoder.Visible = false;
			//do not allow to change modes
			hbox_change_modes_encoder.Visible = false;

		}

		LogB.Information(string.Format("Compujump variables: {0}, {1}, {2}, {3}, {4}",
				configChronojump.Compujump,
				configChronojump.CompujumpServerURL != null,
				configChronojump.CompujumpServerURL != "",
				configChronojump.CompujumpStationID != -1,
				configChronojump.CompujumpStationMode != Constants.Modes.UNDEFINED));

		if(
				configChronojump.Compujump &&
				configChronojump.CompujumpServerURL != null &&
				configChronojump.CompujumpServerURL != "" &&
				configChronojump.CompujumpStationID != -1 &&
				configChronojump.CompujumpStationMode != Constants.Modes.UNDEFINED
				)
		{
			LogB.Information(configChronojump.Compujump.ToString());
			LogB.Information(configChronojump.CompujumpServerURL);

			//on compujump cannot add/edit persons, do it from server
			frame_persons_top.Visible = false;
			//button_contacts_person_change.Visible = false;
			//button_encoder_person_change.Visible = false;
			//TODO: don't allow edit person on person treeview

			//dont't show persons_bottom hbox where users can be edited, deleted if persons at lateral is selected on preferences
			vbox_persons_bottom.Visible = false;

			//do not show new products on networks
			button_menu_news.Visible = false;
			button_menu_news1.Visible = false;

			//don't allow to change encoderConfiguration
			label_encoder_exercise_encoder.Visible = false;
			hbox_encoder_exercise_encoder.Visible = false;

			//on networks do not show tooltip of button_encoder_capture_finish button
			//because on pressing capture, the position pressed on the screen corresponds to the finish button
			//and then tooltip appears during all capture, so hide this tooltip
			button_encoder_capture_finish.TooltipText = "";

			//do not allow to capture infinite (cont)
			preferences.encoderCaptureInfinite = false;

			//don't allow to edit exercises
			button_encoder_exercise_edit.Visible = false;
			button_encoder_exercise_add.Visible = false;
			button_encoder_exercise_delete.Visible = false;

			//on networks do not allow to change devices until preferences option is checked
			viewport_chronopics.Sensitive = false;
			button_encoder_devices_networks.Sensitive = false;

			//do not allow camera controls
			showWebcamCaptureContactsControls (false); //contacts
			notebook_video_contacts.Visible = false;
			hbox_video_encoder.Visible = false;

			check_contacts_networks_upload.Visible = true;
			check_encoder_networks_upload.Visible = true;

			//networks always without lateral person win
			//also this is important for seing label_rfid_encoder_wait
			SqlitePreferences.Update("personWinHide", "True", false);
			preferences.personWinHide = true;
			configChronojump.PersonWinHide = true;
			showPersonsOnTop(true);

			button_networks_contacts_guest.Visible = true;
			button_contacts_person_change.Visible = false;

			if(configChronojump.CompujumpStationMode != Constants.Modes.UNDEFINED)
			{
				//changeModeCheckRadios (configChronojump.CompujumpStationMode);
				//better do like this because radiobuttons are not set. TODO: remove radiobuttons checks
				if(configChronojump.CompujumpStationMode == Constants.Modes.JUMPSSIMPLE)
					on_button_selector_start_jumps_simple_clicked(new object (), new EventArgs());
				else if(configChronojump.CompujumpStationMode == Constants.Modes.JUMPSREACTIVE)
					on_button_selector_start_jumps_reactive_clicked(new object (), new EventArgs());
				else if(configChronojump.CompujumpStationMode == Constants.Modes.RUNSINTERVALLIC)
					on_button_selector_start_runs_intervallic_clicked(new object (), new EventArgs());
				else if(configChronojump.CompujumpStationMode == Constants.Modes.POWERGRAVITATORY)
					on_button_selector_start_encoder_gravitatory_clicked(new object (), new EventArgs());
				else //if(configChronojump.CompujumpStationMode == Constants.Modes.POWERINERTIAL)
					on_button_selector_start_encoder_inertial_clicked(new object (), new EventArgs());

				vbox_runs_interval.Visible = false;
				vbox_runs_interval_compujump.Visible = true;
			}

			Json.ChangeServerUrl(configChronojump.CompujumpServerURL);

			capturedRFID = "";
			shouldUpdateRFIDGui = false;
			rfidProcessCancel = false;
			networksRunIntervalCanChangePersonSQLReady = true;

			chronopicRegisterUpdate(false);
			if(chronopicRegister != null && chronopicRegister.GetRfidPortName() != "")
			{
				networksShowDeviceMissingRFID (false);
				rfid = new RFID(chronopicRegister.GetRfidPortName());
				rfid.FakeButtonChange.Clicked += new EventHandler(rfidChanged);
				rfid.FakeButtonReopenDialog.Clicked += new EventHandler(rfidReopenDialog);
				rfid.FakeButtonDisconnected.Clicked += new EventHandler(rfidDisconnected);
				rfid.FakeButtonAdminDetected.Clicked += new EventHandler(rfidAdminDetectedSendMail);

				sendingJsonRFID = false;
				threadRFID = new Thread (new ThreadStart (RFIDStart));
				GLib.Idle.Add (new GLib.IdleHandler (pulseRFID));

				LogB.ThreadStart();
				threadRFID.Start();
			} else
				networksShowDeviceMissingRFID (true);

			if(configChronojump.CompujumpAdminEmail != "")
				button_encoder_analyze_image_compujump_send_email.Visible = true;

			viewport_chronopics.Visible = true;
			viewport_chronopic_encoder.Visible = true;
		}
		else {
			viewport_chronopics.Visible = false;
			viewport_chronopic_encoder.Visible = false;
		}

		if (configChronojump.Compujump) {
			button_contacts_devices_networks.Visible = true;
			button_encoder_devices_networks.Visible = true;
			button_contacts_detect_small.Visible = false;
			button_threshold.Visible = false;
			//button_force_sensor_adjust.Visible = false;
			button_encoder_detect_small.Visible = false;
		} else {
			button_contacts_devices_networks.Visible = false;
			button_encoder_devices_networks.Visible = false;
			button_contacts_detect_small.Visible = true;
			button_threshold.Visible = true;
			//button_force_sensor_adjust.Visible = true;
			button_encoder_detect_small.Visible = true;
		}

		if(configChronojump.Raspberry)
		{
			//make easiers to use some treeviews
			hbox_sessions_raspberry.Visible = true;

			//on raspberry with VNC the main hpaned cannot be moved, so show this buttons
			hbox_persons_raspberry.Visible = true;
		}

		if(configChronojump.LowHeight)
		{
			vbox_menu_tiny_menu.Spacing = 14; //spacing 10 or 14 is the same. 20 makes window higher
			image_encoder_inertial_instructions.Visible = false;
		}

		if(configChronojump.GuiTest)
		{
			button_menu_guiTest.Visible = true;
			button_menu_guiTest1.Visible = true;
		}

		if(configChronojump.Exhibition)
		{
			exhibitionGuiAtStart(configChronojump.ExhibitionStationType);
			SqliteJson.UploadExhibitionTestsPending();
		}

		if (configChronojump.CopyToCloudFullPath != "")
		{
			app1s_box_copyToCloud1.Visible = true;
			app1s_alignment_copyToCloud.Visible = true;
		}

		//Cloud-view should not be able to capture, create new person, create new session
		//as everything is on tmp and will be deleted
		if (configChronojump.ReadFromCloudMainPath != "")
		{
			//contacts
			box_contacts_capture_cloud_view_disabled.Visible = true;
			button_contacts_detect.Visible = false;
			hbox_contacts_detect_and_execute.Visible = false;

			//encoder
			box_encoder_capture_cloud_view_disabled.Visible = true;
			button_encoder_detect.Visible = false;
			hbox_encoder_detect_and_execute.Visible = false;

			//session new
			box_session_new_cloud_view_disabled.Visible = true;
			button_session_new.Visible = false;

			//person new
			box_person_new_cloud_view_disabled.Visible = true;
			button_person_add_single.Visible = false;
			button_person_add_multiple.Visible = false;
		} else {
			//contacts
			box_contacts_capture_cloud_view_disabled.Visible = false;

			//encoder
			box_encoder_capture_cloud_view_disabled.Visible = false;

			//session new
			box_session_new_cloud_view_disabled.Visible = false;

			//person new
			box_person_new_cloud_view_disabled.Visible = false;
		}

		storedCloudDir = "";
		if (configChronojump.ReadFromCloudMainPath != "" || configChronojump.CanOpenExternalDB)
		{
			box_menu_database.Visible = true;
			check_menu_database1.Visible = true;
			box_copy_from_cloud_progressbars.Visible = (configChronojump.ReadFromCloudMainPath != "");
			image_cloud_view.Visible = (configChronojump.ReadFromCloudMainPath != "");

			// if directory on LastDBFullPath does not exists, update field
			if (configChronojump.LastDBFullPath != "" && ! Util.DirectoryExists (configChronojump.LastDBFullPath))
			{
				configChronojump.UpdateField ("LastDBFullPath", ""); 	//update file
				configChronojump.LastDBFullPath = "";			//update variable
			}

			// if LastDBFullPath exists, use it
			if (configChronojump.LastDBFullPath != "")
			{
				if (configChronojump.ReadFromCloudMainPath != "")
				{
					storedDBFilename = configChronojump.LastDBFullPath;
					databaseCloudCopyToTemp (true); //at boot
					return; //following code is not going to be executed, will be called when copying thread is finished
				}

				if (configChronojump.ReadFromCloudMainPath != "" || configChronojump.CanOpenExternalDB)
				{
					storedCloudDir = configChronojump.LastDBFullPath;
					databaseChange ();
				}
			} else {
				label_current_database.Text = "(default database)";
				label_current_database1.Text = "(default database)";
				button_database_reload.Sensitive = false;
			}
		}

		if (configChronojump.RemoteTestJumpSimpleFile != "" || configChronojump.RemoteTestRunIntervalFile != "")
		{
			remoteTest = new RemoteTest (current_mode,
					configChronojump.RemoteTestJumpSimpleFile,
					configChronojump.RemoteTestRunIntervalFile,
					configChronojump.RemoteTestCancelFile);

			//"Call" because it will be called by the main thread (pulseRemoteTestCheck)
			remoteTest.FakeButtonDo.Clicked += new EventHandler (remoteTestCall);
			remoteTest.FakeButtonCancel.Clicked += new EventHandler (remoteTestCancel);

			threadRemoteTest = new Thread (new ThreadStart (remoteTestCheckStart));
			GLib.Idle.Add (new GLib.IdleHandler (pulseRemoteTestCheck));

			LogB.ThreadStart();
			threadRemoteTest.Start();
		}

		if (configChronojump.RemotePersonNextFile != "")
		{
			box_remote_person_next.Visible = true;

			remotePersonNext = new RemotePersonNext (
					configChronojump.RemotePersonNextFile,
					new List<Gtk.Button> {
						button_remote_person_next1,
						button_remote_person_next2,
						button_remote_person_next3,
						button_remote_person_next4,
						button_remote_person_next5 });
		}

		configDo();
		ChronojumpWindowCont ();
	}
	private void RFIDStart()
	{
		startedRFIDWait = DateTime.MinValue;
		rfidWaitingAdminGuiObjects = new RFIDWaitingAdminGuiObjects();
		LogB.Information("networksRI: " + networksRunIntervalCanChangePersonSQLReady.ToString());

		needManageCompujumpCapturingReset = false;
		LogB.Information("RFID Start");
		rfid.Start();
	}
	private void rfidChanged(object sender, EventArgs e)
	{
		LogB.Information("at rfidChanged");
		/*
		 * TODO: only if we are not in the middle of capture, or in cont mode without repetitions
		 */
		if(currentSession == null)
			return;

		if(rfid.Captured != capturedRFID)
			LogB.Information("detected different RFID: " + rfid.Captured);

		if(isCompujumpCapturing ())
		{
			startedRFIDWait = DateTime.Now;
			LogB.Information("... but we are on the middle of capture");
			return;
		}

		if(rfid.Captured != capturedRFID)
		{
			capturedRFID = rfid.Captured;
			rfidIsDifferent = true;
			shouldUpdateRFIDGui = true;
		}
	}

	private void rfidReopenDialog(object sender, EventArgs e)
	{
		if(currentSession == null || rfid.Captured != capturedRFID)
			return;

		if(isCompujumpCapturing ())
		{
			startedRFIDWait = DateTime.Now;
			LogB.Information("... but we are on the middle of capture");
			return;
		}

		rfidIsDifferent = false;
		shouldUpdateRFIDGui = true; //this opens the task window
	}

	private void rfidAdminDetectedSendMail(object sender, EventArgs e)
	{
		checkFile(Constants.CheckFileOp.ENCODER_ANALYZE_SEND_IMAGE);
		compujumpSendEmail(Constants.CheckFileOp.ENCODER_ANALYZE_SEND_IMAGE);
	}

	private void rfidDisconnected(object sender, EventArgs e)
	{
		shouldShowRFIDDisconnected = true;
		rfidProcessCancel = true;
	}

	// ----remote test ---->
	private void remoteTestCheckStart () //no gui here
	{
		remoteTest.CheckFileCreation ();
	}
	private void remoteTestCall (object o, EventArgs e) //no gui here
	{
		remoteTestTestName = remoteTest.RemoteTestTestName;
		remoteTestCallNow = true;
	}
	private void remoteTestCancel (object o, EventArgs e) //no gui here
	{
		remoteTestCancelNow = true;
	}
	private bool pulseRemoteTestCheck ()
	{
		if(! threadRemoteTest.IsAlive)
		{
			LogB.ThreadEnding();
			LogB.ThreadEnded();
			return false;
		}

		if (currentSession == null || currentPerson == null)
			return true;

		//execute test if needed
		if (remoteTestCallNow &&
				(current_mode == Constants.Modes.JUMPSSIMPLE ||
				 current_mode == Constants.Modes.RUNSINTERVALLIC))
		{
			remoteTestCallNow = false;

			if (current_mode == Constants.Modes.JUMPSSIMPLE &&
					remoteTestTestName != "" &&
					comboSelectJumps.EntryExistsByName (remoteTestTestName))
				combo_select_jumps = comboSelectJumps.SelectByName (remoteTestTestName);
			else if (current_mode == Constants.Modes.RUNSINTERVALLIC &&
					remoteTestTestName != "" &&
					comboSelectRunsI.EntryExistsByName (remoteTestTestName))
				combo_select_runs_interval = comboSelectRunsI.SelectByName (remoteTestTestName);

			button_execute_test.Click ();
		}

		if (remoteTestCancelNow)
		{
			remoteTestCancelNow = false;
			event_execute_button_cancel.Click();
		}

		Thread.Sleep (100);
		return true;
	}
	// <---- remote test ----

	/*
	 * this controls what is going to be done at en the copying thread
	 * true: at chronojump boot
	 * false: at click on open database
	 */
	static bool databaseCloudCopyToTempModeAtBoot;

	private void databaseCloudCopyToTemp (bool atBoot)
	{
		databaseCloudCopyToTempModeAtBoot = atBoot;

		copyFromCloudToTemp_pre ();
	}

	// updateConfigFile only if selected a new db by user: on_button_database_change_clicked ()
	private void databaseChange ()
	{
		closeSession ();

		Sqlite.DisConnect ();

		//called from Util.GetLocalDataDir
		Config.LastDBFullPathStatic = configChronojump.LastDBFullPath;
		Sqlite.SetHome ();

		Sqlite.Connect ();

		//this updated if needed:
		Sqlite.DBIsOldEnum dbIsOldEnum = Sqlite.NeedToConvertToLastChronojumpDBVersion (); // //needed to update string currentVersion
		if (dbIsOldEnum == Sqlite.DBIsOldEnum.OLDDB)
			Sqlite.ConvertToLastChronojumpDBVersion();

		string databaseDirName = configChronojump.LastDBFullPath;
		if (storedCloudDir != "")
			databaseDirName = storedCloudDir;

		label_current_database.Text = "<b>" + Util.GetLastPartOfPath (databaseDirName) + "</b>";
		label_current_database1.Text = "<b>" + Util.GetLastPartOfPath (databaseDirName) + "</b>";
		label_current_database.UseMarkup = true;
		label_current_database1.UseMarkup = true;
		label_current_database.TooltipText = Util.GetLastPartOfPath (databaseDirName);
		label_current_database1.TooltipText = Util.GetLastPartOfPath (databaseDirName);

		loadPreferencesAtStartOrCloudViewChangeDB ();

		if (preferences.loadLastSessionAtStart && preferences.lastSessionID > 0)
			changeSessionAtStartOrCloudViewChangeDB ();

		if (preferences.loadLastModeAtStart &&
					preferences.lastMode != Constants.Modes.UNDEFINED)
			changeModeAtStartOrCloudViewChangeDB ();

		Config.UseSystemColor = preferences.colorBackgroundOsColor;
		doLabelsContrast(configChronojump.PersonWinHide);
		vbox_persons_bottom.Visible = preferences.personPhoto && ! check_menu_session.Active;
		UtilGtk.ApplyCSS (preferences.fontSizeAtGui);

		fillAllCombos ();
	}

	private void on_button_database_reload_clicked (object o, EventArgs args)
	{
		if (storedDBFilename != "")
			databaseChangeOrReload ();
	}

	private string storedCloudDir;
	private string storedDBFilename = ""; //used reload/change database from know where to copy to tmp

	private void on_button_database_change_select_clicked (object o, EventArgs args)
	{
		cloudReadFromShowFolders ();

		button_database_change_apply.Visible = true;

		button_database_change_apply.Sensitive = false;
		foreach (Gtk.RadioButton r in cloudReadFolder_l)
			if (r.Active && Util.GetLastPartOfPath (storedDBFilename) != r.Label)
				button_database_change_apply.Sensitive = true;
	}

	private void on_button_database_change_apply_clicked (object o, EventArgs args)
	{
		foreach (Gtk.RadioButton r in cloudReadFolder_l)
			if (r.Active)
			{
				//if we are not in defaultDB, copy /tmp/ChronojumpCloudRead to LastDBFullPath to ensure changes are copied. Also done on Chronojump exit
				if (storedDBFilename != "")
					Util.FileCopySafe (
							Path.Combine(Util.GetCloudReadTempDir (), "database", "chronojump.db"),
							Path.Combine(storedDBFilename, "database", "chronojump.db"),
							true); //overwrite

				storedDBFilename = Path.Combine (configChronojump.ReadFromCloudMainPath, r.Label);
				databaseChangeOrReload ();
			}
	}

	//Gtk.FileChooserNative database_fc;
	/*
	 * commented as now on cloudRead radio buttons are created.
	 * TODO: remember to have something working for ExternalDBDefaultPath
	 *
	private void on_button_database_change_clicked (object o, EventArgs args)
	{
		FileChooserAction action = FileChooserAction.SelectFolder;
		//mac arm64 crashes on SelectFolder, use Open. The problem in Open is it cannot select a folder that has contents. Only an empty folder
		if (UtilAll.IsMacSilicon ())
			action = FileChooserAction.Open;

		database_fc = new Gtk.FileChooserNative("Select folder:",
				app1,
				action,
				Catalog.GetString("Select"), Catalog.GetString("Cancel")
				);

		if (configChronojump.ReadFromCloudMainPath != "")
			database_fc.SetCurrentFolder (configChronojump.ReadFromCloudMainPath);
		else if (configChronojump.ExternalDBDefaultPath != "")
			database_fc.SetCurrentFolder (configChronojump.ExternalDBDefaultPath);

		if (database_fc.Run() == (int)ResponseType.Accept)
		{
			storedDBFilename = database_fc.Filename;
			databaseChangeOrReload ();
		}

		database_fc.Hide ();

		//Don't forget to call Destroy() or the FileChooserNative window won't get closed.
		database_fc.Destroy();
	}
	*/

	private void databaseChangeOrReload ()
	{
		// 1) check that there is a database/chronojump.db inside
		if ( ! File.Exists( System.IO.Path.Combine (storedDBFilename, "database", "chronojump.db")) )
		{
			new DialogMessage (Constants.MessageTypes.WARNING,
					"Error: Need to select a folder that has a \"database\" folder inside an a \"chronojump.db\" file inside.");
		} else {
			// 2) update config file (taking care of being default config file)
			configChronojump.UpdateFieldEnsuringDefaultConfigFile ("LastDBFullPath", storedDBFilename);

			// 3) reassign configChronojump.LastDBFullPath
			configChronojump.LastDBFullPath = storedDBFilename;
			storedCloudDir = "";
			treeview_persons_storeReset();

			// 4) if on cloud, copy to tmp, LastDBFullPath will be at temp (needed for Chronojump)
			if (configChronojump.ReadFromCloudMainPath != "")
			{
				storedCloudDir = storedDBFilename;
				databaseCloudCopyToTemp (false); //not at boot

				return; //following code is not going to be executed, will be called when copying thread is finished
			}

			// 5) change database
			databaseChange ();
		}
	}

	List<Gtk.RadioButton> cloudReadFolder_l;
	private void cloudReadFromShowFolders ()
	{
		cloudReadFolder_l = new List<Gtk.RadioButton> ();
		List<DirectoryInfo> dir_l = Util.GetCloudViewDatabases (configChronojump.ReadFromCloudMainPath);

		UtilGtk.RemoveChildren (box_database_manage_read);

		bool first = true; //used to assign the rest of the radios to the group of the 1st
		Gtk.RadioButton r = null;

		foreach (DirectoryInfo di in dir_l)
		{
			if (first) {
				r = new Gtk.RadioButton (di.Name);
				first = false;
			} else
				r = new Gtk.RadioButton (r, di.Name);

			if (di.Name == Util.GetLastPartOfPath (storedDBFilename))
				r.Active = true;

			r.Clicked -= new EventHandler (on_cloud_view_radio_clicked); //needed. if not: called multiple times
			r.Clicked += new EventHandler (on_cloud_view_radio_clicked);

			box_database_manage_read.PackStart (r, false, false, 0);
			cloudReadFolder_l.Add (r);
		}

		if (first) //no directory found
		{
			Gtk.Label l = new Gtk.Label ("\n\n" +
					Catalog.GetString ("Not found any database at path:") + "\n" +
				configChronojump.ReadFromCloudMainPath);
			box_database_manage_read.PackStart (l, false, false, 0);
		}

		box_database_manage_read.ShowAll ();

		if(! Config.UseSystemColor)
			UtilGtk.ContrastLabelsBox (Config.ColorBackgroundShiftedIsDark, box_database_manage_read);
	}

	private void on_cloud_view_radio_clicked (object o, EventArgs args)
	{
		if (! ((Gtk.RadioButton) o).Active)
			return;

		button_database_change_apply.Sensitive =
			(((Gtk.RadioButton) o).Label != Util.GetLastPartOfPath (storedDBFilename));
	}

	private void on_button_networks_guest_clicked (object sender, EventArgs e)
	{
		// 1) do not allow to click again the button
		button_networks_contacts_guest.Visible = false;

		// 2) reset logout counter
		compujumpAutologout = new CompujumpAutologout();

		// 3) create/load person. guest is id: -2, check if it exists. If it exists will return -2, if not: -1
		currentPerson = SqlitePerson.Select(false, -2);
		if(currentPerson.UniqueID == -1) //do not exists, create it
			currentPerson = new Person (true, -2, Catalog.GetString("Guest"), "-1", ""); //this true means insertPerson

		// 4) personSession insert if needed and assign
		currentPersonSession = SqlitePersonSession.Select(false, currentPerson.UniqueID, currentSession.UniqueID);
		if(currentPersonSession.UniqueID == -1)
			currentPersonSession = new PersonSession (
					currentPerson.UniqueID, currentSession.UniqueID,
					0, 0, //width, height
					Constants.SportUndefinedID,
					Constants.SpeciallityUndefinedID,
					Constants.LevelUndefinedID,
					"", 	//comments
					Constants.TrochanterToeUndefinedID,
					Constants.TrochanterFloorOnFlexionUndefinedID,
					false); //dbconOpened

		// 5) person active gui changes
		sensitiveGuiYesPerson();
		personChanged(); //GTK
		label_person_change();

		// 6) specific guest setup
		if (Constants.ModeIsENCODER (current_mode))
			configNetworksEncoderAsGuest (true);
		else
			configNetworksContactsAsGuest (true);
	}
	private void configNetworksContactsAsGuest (bool guest)
	{
		//on guest widgets are invisible, cleaner and easier than unsensitive because during capture/curves sensitivity changes on some buttons
		check_contacts_networks_upload.Active = ! guest;
		check_contacts_networks_upload.Visible = ! guest;

		//radio_mode_contacts_analyze_small.Visible = ! guest;
		button_menu_preferences.Visible = ! guest;
		button_contacts_delete_selected.Visible = ! guest;

		button_contacts_bells.Visible = ! guest;
		button_video_play_this_test.Visible = ! guest;
		button_contacts_repair_selected.Visible = ! guest;
	}
	private void configNetworksEncoderAsGuest(bool guest)
	{
		//on guest widgets are invisible, cleaner and easier than unsensitive because during capture/curves sensitivity changes on some buttons
		check_encoder_networks_upload.Active = ! guest;
		check_encoder_networks_upload.Visible = ! guest;

		radio_mode_contacts_analyze.Visible = ! guest;
		button_menu_preferences.Visible = ! guest;
		button_encoder_bells.Visible = ! guest;

		if(guest)
		{
			preferences.encoderCaptureMainVariable = Constants.EncoderVariablesCapture.MeanSpeed;
			preferences.encoderCaptureSecondaryVariable = Constants.EncoderVariablesCapture.RangeAbsolute;
		} else {
			preferences.encoderCaptureMainVariable = (Constants.EncoderVariablesCapture)
				Enum.Parse(typeof(Constants.EncoderVariablesCapture),
						SqlitePreferences.Select("encoderCaptureMainVariable", false));
			preferences.encoderCaptureSecondaryVariable = (Constants.EncoderVariablesCapture)
				Enum.Parse(typeof(Constants.EncoderVariablesCapture),
						SqlitePreferences.Select("encoderCaptureSecondaryVariable", false));
		}
	}

	private void configInitFromPreferences()
	{
		LogB.Information ("at configInitFromPreferences, configChronojump null? " + (configChronojump == null).ToString ());
		configChronojump.Maximized = preferences.maximized;
		configChronojump.PersonWinHide = preferences.personWinHide;
		configChronojump.PersonWinShrink = preferences.personWinShrink;
		configChronojump.EncoderCaptureShowOnlyBars = preferences.encoderCaptureShowOnlyBars;

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

		// config only change maximized status on start
		if( ! maximizeWindowAtStartDone )
		{
			maximizeOrNot (true);  //from config
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
			//---- analyze tab ----

			hbox_encoder_analyze_signal_or_curves.HeightRequest = 40;
			button_encoder_analyze.SetSizeRequest(120,40);
		}
		//restriction for configured Compujump clients
		//if(configChronojump.Compujump)
		//	spin_encoder_im_weights_n.Sensitive = false;
		
		//show only power
		if(configChronojump.OnlyEncoderGravitatory)
			on_button_selector_start_encoder_gravitatory_clicked(new object(), new EventArgs());
		else if(configChronojump.OnlyEncoderInertial)
			on_button_selector_start_encoder_inertial_clicked(new object(), new EventArgs());

		encoderUpdateTreeViewWhileCapturing = configChronojump.EncoderUpdateTreeViewWhileCapturing;

		showPersonsOnTop (configChronojump.PersonWinHide);
		person_search_compacted (configChronojump.PersonWinShrink);
		showPersonPhoto(preferences.personPhoto);

		if(currentSession == null && //this is going to be called one time because currentSession will change
			       ( configChronojump.SessionMode == Config.SessionModeEnum.UNIQUE || configChronojump.SessionMode == Config.SessionModeEnum.MONTHLY) )
		{
			//app1.Decorated = false;
			//hbox_menu_and_preferences_outside_menu_contacts.Visible = true;
			//hbox_menu_and_preferences_outside_menu_encoder.Visible = true;

			//hide session controls on UNIQUE and MONTHLY
			button_session_new.Visible = false;
			button_session_load.Visible = false;
			check_menu_session.Visible = false;
			check_menu_session1.Visible = false;

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
				if(needToCreateMonthlySession(yearMonthStr))
					createMonthlySession(yearMonthStr);
				else
					currentSession = SqliteSession.SelectByName(yearMonthStr);
			}
			
			/*
			   this will be done later (at end of ChronoJumpWindow method)
			   because on_load_session_accepted () manages many objects, most of them still not defined

			on_load_session_accepted();
			sensitiveGuiYesSession();
			*/
		}

		//TODO
		//RunScriptOnExit

		/*
		if(linuxType == linuxTypeEnum.NETWORKS) {
			//mostrar directament el power
			changeModeCheckRadios (Constants.Modes.POWER);
			
			//no mostrar menu
			
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
	}

	private void maximizeOrNot (bool fromConfig)
	{
		if (fromConfig)
		{
			if(configChronojump.Maximized == Preferences.MaximizedTypes.NO)
			{
				app1.Unmaximize();
				app1.Decorated = true;
			} else {
				app1.Decorated = UtilAll.IsWindows () || //always Decorated on Windows (fixes bug on 11 Pro)
					configChronojump.Maximized != Preferences.MaximizedTypes.YESUNDECORATED;
				app1.Maximize();
			}
		} else //fromPreferences
		{
			if (preferences.maximized == Preferences.MaximizedTypes.NO)
			{
				app1.Unmaximize();
				app1.Decorated = true;
			} else
			{
				app1.Decorated = UtilAll.IsWindows () || //always Decorated on Windows (fixes bug on 11 Pro)
					preferences.maximized == Preferences.MaximizedTypes.YES;
				app1.Maximize();
			}
		}
	}

	private bool needToCreateMonthlySession(string yearMonthStr)
	{
		LogB.Information("yearMonthStr: " + yearMonthStr);

		return(! Sqlite.Exists(false, Constants.SessionTable, yearMonthStr));
	}

	private void createMonthlySession(string yearMonthStr)
	{
		//this creates the session and inserts at DB
		currentSession = new Session(
				yearMonthStr, "", DateTime.Today,	//name, place, dateTime
				Constants.SportUndefinedID, Constants.SpeciallityUndefinedID, Constants.LevelUndefinedID,
				"", Constants.ServerUndefinedID); //comments, serverID

		setApp1Title(currentSession.Name, current_mode);

		//insert personSessions from last month
		string yearLastMonthStr = UtilDate.GetCurrentYearLastMonthStr();
		if(Sqlite.Exists(false, Constants.SessionTable, yearLastMonthStr))
		{
			Session s = SqliteSession.SelectByName(yearLastMonthStr);

			//import all persons from last session
			List<PersonSession> personSessions = SqlitePersonSession.SelectPersonSessionList(false, -1, s.UniqueID);

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
	}

	DialogMessage dialogMessageNotAtServer;
	private bool pulseRFID ()
	{
		if(shouldShowRFIDDisconnected)
		{
			//new DialogMessage(Constants.MessageTypes.WARNING, Constants.RFIDDisconnectedMessage());
			networksShowDeviceMissingRFID (true);

			if(dialogPersonPopup != null)
				dialogPersonPopup.DestroyDialog();

			shouldShowRFIDDisconnected = false;
		}

		if(! threadRFID.IsAlive || rfidProcessCancel)
		{
			label_rfid_wait.Visible = false;

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
			label_rfid_wait.Text = Catalog.GetString("Please, wait!");
		} else {
			label_rfid_wait.Visible = false;
		}

		if(isCompujumpCapturing ()) {
			Thread.Sleep (100);
			needManageCompujumpCapturingReset = true;
			return true;
		}
		else if (needManageCompujumpCapturingReset) {
			//reset lastRFID in order to be able to use that RFID after capture (if same wristband is used again)
			rfid.ResetLastRFID(); //maybe rfid has to be static or ensure this is done in the other thread
			needManageCompujumpCapturingReset = false;
		}

		//---- end of checking if we are on the middle of capture.

		//to send email with analyzed image to the admin
		if(rfidWaitingAdminGuiObjects != null)
		{
			if(rfidWaitingAdminGuiObjects.ShowGuiMessageSuccess)
			{
				new DialogMessage(Constants.MessageTypes.INFO,
						"Sent Email to: " + configChronojump.CompujumpAdminEmail);
				rfidWaitingAdminGuiObjects = new RFIDWaitingAdminGuiObjects();
			}
			if(rfidWaitingAdminGuiObjects.ShowGuiMessageFailed)
			{
				new DialogMessage(Constants.MessageTypes.WARNING, "Could not send email\nProbably need to install mailutils package.");
				rfidWaitingAdminGuiObjects = new RFIDWaitingAdminGuiObjects();
			}

			if(rfidWaitingAdminGuiObjects.Waiting)
			{
				LogB.Information("rest seconds: " + rfidWaitingAdminGuiObjects.RestSeconds().ToString());
				if(rfidWaitingAdminGuiObjects.RestSeconds() != -1)
				{
					label_rfid_wait.Text =
						string.Format(Catalog.GetString("Identify with admin ID wristband before {0} s."),
								rfidWaitingAdminGuiObjects.RestSeconds());
					label_rfid_wait.Visible = true;
				} else {
					rfidWaitingAdminGuiObjects = new RFIDWaitingAdminGuiObjects();
					label_rfid_wait.Text = "";
					label_rfid_wait.Visible = false;
					rfid.WaitingAdminStop(); //maybe rfid has to be static or ensure this is done in the other thread
				}
				Thread.Sleep (100);
				return true;
			}
		}


		if(! shouldUpdateRFIDGui) {
			Thread.Sleep (100);
			return true;
		}

		//show a Read message nice if the network is slow or there is any problem with the web services
		string str = Catalog.GetString ("The wristband has been read.") + "\n" +
			Catalog.GetString ("Connecting to server …");
		label_rfid_wait.Text = str;
		label_rfid_wait.Visible = true;

		shouldUpdateRFIDGui = false;

		//is we are on analyze, switch to capture
		if(! radio_mode_contacts_capture.Active)
			radio_mode_contacts_capture.Active = true;

		/*
		   don't allow sendJsonRFID to be called again until ended
		   Note RFID detection can send many cards (the same) per second
		   having that call on a timeout is to ensure the label_rfid_wait will be shown
		   */
		if (sendingJsonRFID)
		{
			Thread.Sleep (100);
			LogB.Information(" threadRFID:" + threadRFID.ThreadState.ToString());

			return false;
		} else {
			//don't allow to press guest button while connection is being done
			button_networks_contacts_guest.Visible = false;

			sendingJsonRFID = true;
			GLib.Timeout.Add (50, new GLib.TimeoutHandler (sendJsonRFID));
		}

		return true;
	}
	private bool sendJsonRFID ()
	{
		/*
		 * This method is shown on diagrams/processes/rfid-local-read.dia
		 */

		bool currentPersonWasNull = (currentPerson == null);
		bool pChangedShowTasks = false;
		JsonCompujump json = new JsonCompujump(configChronojump.CompujumpDjango);

		//select person by RFID
		Person pLocal = SqlitePerson.SelectByRFID(capturedRFID);
		Person pServer = json.GetPersonByRFID(capturedRFID);

		if(pLocal.UniqueID == -1)
		{
			LogB.Information("RFID person does not exist locally!!");

			if(! json.Connected) {
				LogB.Information("Cannot connect with server!");
				if(dialogMessageNotAtServer == null || ! dialogMessageNotAtServer.Visible)
				{
					NetworksCheckDevices ncd = new NetworksCheckDevices();
					dialogMessageNotAtServer = new DialogMessage(
							Constants.MessageTypes.WARNING,
							ncd.ToString() + "\n\n" +
							Constants.ServerDisconnectedMessage()
							); //GTK

					compujumpPersonLogoutDo();
				}
			}
			else if(pServer.UniqueID == -1) {
				LogB.Information("Person NOT found on server!");
				if(dialogMessageNotAtServer == null || ! dialogMessageNotAtServer.Visible)
				{
					dialogMessageNotAtServer = new DialogMessage(Constants.MessageTypes.WARNING, Constants.RFIDNotInServerMessage()); //GTK

					compujumpPersonLogoutDo();
				}
			}
			else {
				LogB.Information("Person found on server!");

				//personID exists at local DB?
				//check if this uniqueID already exists on local database (would mean RFID changed on server)
				pLocal = SqlitePerson.Select(false, pServer.UniqueID);

				if(! json.LastPersonJustInserted)
				{
					/*
					 * id exists locally, RFID has changed. Changed locally
					 * Note server don't allow having an rfid of a previous person. Must be historically unique.
					 */

					pLocal.Future1 = pServer.Future1;
					SqlitePerson.Update(pLocal);
				}

				string yearMonthStr = UtilDate.GetCurrentYearMonthStr();
				if(needToCreateMonthlySession(yearMonthStr))
					createMonthlySession(yearMonthStr);

				currentPerson = pLocal;
				insertAndAssignPersonSessionIfNeeded(json);

				if(json.LastPersonJustInserted)
				{
					compujumpDownloadImage (json, json.LastPersonByRFIDImageURL, currentPerson.UniqueID);
					person_added(); //GTK
				} else {
					personChanged(); //GTK
					label_person_change();
				}
				pChangedShowTasks = true;
			}
		}
		else if(json.Connected && pLocal.UniqueID != pServer.UniqueID)
		{
			LogB.Information("PersonID on client does not match personID on server for rfid: " + capturedRFID);
			/* previous to 2.1.3 was not accepted (rfids cannot be reassigned to a new person)
			if(dialogMessageNotAtServer == null || ! dialogMessageNotAtServer.Visible)
			{
				dialogMessageNotAtServer = new DialogMessage(Constants.MessageTypes.WARNING,
						string.Format("PersonID {0} on client does not match personID {1} on server for rfid: {2}",
							pLocal.UniqueID, pServer.UniqueID, capturedRFID)); //GTK

				compujumpPersonLogoutDo();
			}
			*/

			// 2.1.3 code, rfids can be reassigned to a new person
			// a wristband of a new player has been introduced, but this wristband we had previously locally assigned to another player

			// 1 delete rfid on previous person
			SqlitePerson.UpdateRFID (pLocal.UniqueID, "");

			// 2 create new person with the server rfid, personSession
			currentPerson = new Person (true, pServer.UniqueID, pServer.Name, pServer.Future1, json.LastPersonByRFIDImageURL);
			insertAndAssignPersonSessionIfNeeded(json);

			compujumpDownloadImage (json, json.LastPersonByRFIDImageURL, currentPerson.UniqueID);

			personChanged(); //GTK
			label_person_change();
			pChangedShowTasks = true;
		} else {
			LogB.Information("RFID person exists locally!!");

			//if image changed, download it
			if( pServer.UniqueID != -1 &&
					(pServer.LinkServerImage != pLocal.LinkServerImage ||
					! Util.FileExists(compujumpDownloadImageGetDest (pServer.LinkServerImage, pServer.UniqueID))) )
				compujumpDownloadImage (json, pServer.LinkServerImage, pServer.UniqueID);

			if(rfidIsDifferent || dialogPersonPopup == null || ! dialogPersonPopup.Visible)
			{
				string yearMonthStr = UtilDate.GetCurrentYearMonthStr();
				if(needToCreateMonthlySession(yearMonthStr))
					createMonthlySession(yearMonthStr);

				//update person if name changed
				//but only if we are connected
				if(pServer.Name != null && pServer.Name != "" && pServer.Name != pLocal.Name)
				{
					pLocal.Name = pServer.Name;
					SqlitePerson.UpdateName (pLocal.UniqueID, pLocal.Name);
				}

				currentPerson = pLocal;
				insertAndAssignPersonSessionIfNeeded(json);

				personChanged(); //GTK
				label_person_change();
				pChangedShowTasks = true;
			}
		}

		//----- Start upload temp tests

		//disabled on 2.1.3. being able now to reassign to another person a current rfid (wristband), is better to not store data when there is no network.
		//To avoid uploading problems if person changed.

		//select UploadTemp tests (have not been uploaded by network errors)

		/*
		Sqlite.Open(); // ---------------->

		List<UploadEncoderDataFullObject> listEncoderTemp = SqliteJson.SelectTempEncoder(true);
		List<UploadSprintDataObject> listSprintTemp = SqliteJson.SelectTempSprint(true);

		//Upload them
		TODO: disabled (2.1.1 aprox) until find why says that database is not open
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
		*/

		//----- End upload temp tests


		if(currentPerson != null && currentPersonWasNull)
			sensitiveGuiYesPerson();

		//it is not a guest remove guest changes
		if (Constants.ModeIsENCODER (current_mode))
			configNetworksEncoderAsGuest (false);
		else
			configNetworksContactsAsGuest (false);

		if(pChangedShowTasks)
		{
			compujumpAutologout = new CompujumpAutologout();

			button_networks_contacts_guest.Visible = false;
			button_contacts_person_change.Visible = true;

			/*TODO:
			int rowToSelect = myTreeViewPersons.FindRow(currentPerson.UniqueID);
			if(rowToSelect != -1)
				selectRowTreeView_persons(treeview_persons, rowToSelect);
			*/
			getTasksExercisesAndPopup();

			//load current session if MONTHLY and current session is not current month and currentPerson is not compumpAdminID
			compujumpPersonChangedShouldChangeSession();

			//not allow to change devices if person changed. If you want to change again, go to preferences/advanced networksAllowChangeDevices
			preferences.networksAllowChangeDevices = false;
			button_encoder_devices_networks.Sensitive = false;
		}

		//Wakeup screen if it's off
		Networks.WakeUpRaspberryIfNeeded();

		sendingJsonRFID = false;
		return false;
	}

	private void compujumpDownloadImage (JsonCompujump json, string url, int personID)
	{
		if(url == null || url == "")
			return;

		string image_dest = compujumpDownloadImageGetDest (url, personID);

		if(json.DownloadImage(url, personID))
			File.Copy(
					Path.Combine(Path.GetTempPath(), personID.ToString()),
					image_dest,
					true); //overwrite

		//find if there is an image with the other extension and delete it
		if(UtilMultimedia.GetImageType(url) == UtilMultimedia.ImageTypes.PNG)
			Util.FileDelete (Util.GetPhotoFileName(false, personID)); //our file is a png, delete the jpg if exists
		else
			Util.FileDelete (Util.GetPhotoPngFileName(false, personID)); //our file is a jpg, delete the png if exists
	}
	//get destination file of the image: multimedia/photos/uniqueID. (jpg or png)
	private string compujumpDownloadImageGetDest (string linkServerImage, int personID)
	{
		string image_dest = Util.GetPhotoFileName(false, personID);
		if(UtilMultimedia.GetImageType(linkServerImage) == UtilMultimedia.ImageTypes.PNG)
			image_dest = Util.GetPhotoPngFileName(false, personID);

		return image_dest;
	}

	//load current session if MONTHLY and current session is not current month and currentPerson is not compumpAdminID
	private void compujumpPersonChangedShouldChangeSession()
	{
		string yearMonthStr = UtilDate.GetCurrentYearMonthStr();
		if(
				configChronojump.SessionMode == Config.SessionModeEnum.MONTHLY &&
				currentSession.Name.ToLower() != yearMonthStr.ToLower() &&
				! configChronojump.CompujumpUserIsAdmin(currentPerson) )
		{
			currentSession = SqliteSession.SelectByName(yearMonthStr);
			on_load_session_accepted();
		}
	}

	private void insertAndAssignPersonSessionIfNeeded(JsonCompujump json)
	{
		PersonSession ps = SqlitePersonSession.Select(false, currentPerson.UniqueID, currentSession.UniqueID);
		if(ps.UniqueID == -1)
			currentPersonSession = new PersonSession (
					currentPerson.UniqueID, currentSession.UniqueID,
					json.LastPersonByRFIDHeight, json.LastPersonByRFIDWeight,
					Constants.SportUndefinedID,
					Constants.SpeciallityUndefinedID,
					Constants.LevelUndefinedID,
					"", 	//comments
					Constants.TrochanterToeUndefinedID,
					Constants.TrochanterFloorOnFlexionUndefinedID,
					false); //dbconOpened
		else {
			//update height if needed
			if(ps.Height != json.LastPersonByRFIDHeight)
			{
				ps.Height = json.LastPersonByRFIDHeight;
				SqlitePersonSession.UpdateAttribute (currentPerson.UniqueID, currentSession.UniqueID,
						"height", json.LastPersonByRFIDHeight);
			}

			//update weight if needed
			if(ps.Weight != json.LastPersonByRFIDWeight)
			{
				ps.Weight = json.LastPersonByRFIDWeight;
				SqlitePersonSession.UpdateAttribute (currentPerson.UniqueID, currentSession.UniqueID,
						"weight", json.LastPersonByRFIDWeight);
			}

			//assign currentPersonSession
			currentPersonSession = ps;
		}
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
		JsonCompujump json;

		if(configChronojump.CompujumpStationMode == Constants.Modes.POWERGRAVITATORY ||
				configChronojump.CompujumpStationMode == Constants.Modes.POWERINERTIAL)
		{
			json = new JsonCompujumpEncoder (configChronojump.CompujumpDjango);
		}
		else if(configChronojump.CompujumpStationMode == Constants.Modes.JUMPSSIMPLE ||
				configChronojump.CompujumpStationMode == Constants.Modes.JUMPSREACTIVE)
		{
			json = new JsonCompujumpJumps (configChronojump.CompujumpDjango);
		}
		else
			return;

		string tasksStr = json.GetTasksResponse (currentPerson.UniqueID, configChronojump.CompujumpStationID);

		List<Task> tasks = new List<Task>();

		//2) deserialize tasks
		if(tasksStr != "")
		{
			if(configChronojump.CompujumpStationMode == Constants.Modes.POWERGRAVITATORY ||
					configChronojump.CompujumpStationMode == Constants.Modes.POWERINERTIAL)
				tasks = new TaskDeserialize().DeserializeTaskEncoder(tasksStr);
		}

		// 1) get tasks
		// 3) get exercises and insert if needed (at the moment only for jumps and encoder)
		// 3a) jumps
		if(configChronojump.CompujumpStationMode == Constants.Modes.JUMPSSIMPLE ||
				configChronojump.CompujumpStationMode == Constants.Modes.JUMPSREACTIVE)
		{
			List<object> jumpsSimpleExercisesOnLocal = SqliteJumpType.SelectJumpTypesNew (false, "", "", false);
			List<object> jumpsRjExercisesOnLocal = SqliteJumpType.SelectJumpRjTypesNew ("", false);

			//List<SelectJumpTypes> exRemote_l = json.GetJumpStationExercises (configChronojump.CompujumpStationID);
			json.GetJumpStationExercises (configChronojump.CompujumpStationID);

			// 3a1) Jumps simple
			bool insertedJumps = false;
			foreach(SelectJumpTypes exRemote in json.JumpSimpleExercises_l)
			{
				bool found = false;
				foreach(SelectJumpTypes jLocal in jumpsSimpleExercisesOnLocal)
					if(jLocal.Id == exRemote.Id)
					{
						found = true;
						break;
					}

				if(! found)
				{
					SqliteJumpType.JumpTypeInsert (exRemote.Id, exRemote.ToSQLString (), false);
					insertedJumps = true;
				}
			}
			if (insertedJumps)
			{
				createComboSelectJumps (false);
				if (current_mode == Constants.Modes.JUMPSSIMPLE)
					createComboSelectContactsTop ();
			}

			// 3a2) Jumps Reactive
			insertedJumps = false;
			foreach(SelectJumpRjTypes exRemote in json.JumpRjExercises_l)
			{
				bool found = false;
				foreach(SelectJumpRjTypes jLocal in jumpsRjExercisesOnLocal)
					if(jLocal.Id == exRemote.Id)
					{
						found = true;
						break;
					}

				if(! found)
				{
					SqliteJumpType.JumpRjTypeInsert (exRemote.Id, exRemote.ToSQLString (), false);
					insertedJumps = true;
				}
			}
			if (insertedJumps)
			{
				createComboSelectJumpsRj (false);
				if (current_mode == Constants.Modes.JUMPSSIMPLE)
					createComboSelectContactsTop ();
			}
		}
		// 3b) encoder
		else if(configChronojump.CompujumpStationMode == Constants.Modes.POWERGRAVITATORY ||
				configChronojump.CompujumpStationMode == Constants.Modes.POWERINERTIAL)
		{
			Constants.EncoderGI type = Constants.EncoderGI.GRAVITATORY;
			if(configChronojump.CompujumpStationMode == Constants.Modes.POWERINERTIAL)
				type = Constants.EncoderGI.INERTIAL;

			List<EncoderExercise> encoderExercisesOnLocal_l = SqliteEncoderExercise.SelectEncoderExercises(false, -1, false, type);
			List<EncoderExercise> exRemote_list = json.GetEncoderStationExercises (configChronojump.CompujumpStationID, type);

			foreach (EncoderExercise exRemote in exRemote_list)
			{
				bool found = false;
				foreach (EncoderExercise exLocal in encoderExercisesOnLocal_l)
					if(exLocal.uniqueID == exRemote.uniqueID)
					{
						found = true;
						break;
					}

				if(! found)
				{
					SqliteEncoderExercise.InsertExercise(
							false, exRemote.uniqueID, exRemote.name, exRemote.percentBodyWeight,
							"", "", Util.ConvertToPoint(exRemote.Speed1RM), type);
					updateEncoderExercisesGui(exRemote.name);
				}
			}
		}

		//4) get other stationsCount
		List<StationCount> stationsCount = json.GetOtherStationsWithPendingTasks(currentPerson.UniqueID, configChronojump.CompujumpStationID);

		//5) check if there are active Internet devices
		NetworksCheckDevices ncd = new NetworksCheckDevices();

		//6) If disconnected, make check_encoder_networks_upload false and insensitive
		check_encoder_networks_upload.Sensitive = json.Connected;
		if(! json.Connected)
			check_encoder_networks_upload.Active = false;

		//7) show dialog
		showDialogPersonPopup(tasks, stationsCount, ncd.ToString(), json.Connected);
	}


	/*
	//just to debug dialog without networks
	public void ShowDialogPersonPopup()
	{
		currentPerson = SqlitePerson.Select(1);
		compujumpAutologout = new CompujumpAutologout();
		showDialogPersonPopup(new List<Task> (), new List<StationCount> (), "some net dev", true);
	}
	*/
	private void showDialogPersonPopup(List<Task> tasks, List<StationCount> stationsCount, string networkDevices, bool serverConnected)
	//private void showDialogPersonPopup(TaskList taskList, List<StationCount> stationsCount, string networkDevices, bool serverConnected)
	{
		if(dialogPersonPopup != null)
			dialogPersonPopup.DestroyDialog();

		if(dialogMessageNotAtServer != null && dialogMessageNotAtServer.Visible)
			dialogMessageNotAtServer.on_close_button_clicked(new object(), new EventArgs());

		dialogPersonPopup = new DialogPersonPopup(
				currentPerson.UniqueID, currentPerson.Name, capturedRFID, tasks, stationsCount,
				networkDevices, serverConnected, compujumpAutologout.Active,
				configChronojump.CompujumpDjango, configChronojump.CompujumpHideTaskDone);

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
		Task task = null;
		if(dialogPersonPopup != null)
		{
			task = dialogPersonPopup.TaskActive;
		}
		dialogPersonPopup.DestroyDialog();

		if(task == null)
			return;

		LogB.Information("Selected task from gui/networks.cs:" + task.ToString());
		if(configChronojump.CompujumpStationMode == Constants.Modes.RUNSINTERVALLIC)
			compujumpTaskStartRunInterval((TaskEncoder) task); //TODO: use taskRunInertial
		else
			compujumpTaskStartEncoder((TaskEncoder) task);
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

	private void compujumpTaskStartRunInterval(TaskEncoder task) //TODO: use taskRunInterval
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

	private void compujumpTaskStartEncoder(TaskEncoder task)
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
			feedbackWin.EncoderMeanSpeedHigherValue = task.Speed;
			feedbackWin.EncoderMeanSpeedHigher = true;
			feedbackWin.EncoderMeanSpeedLowerValue = task.Speed / 2;
			feedbackWin.EncoderMeanSpeedHigher = true;
			feedbackWin.Encoder_show_manual_feedback = true;
			feedbackWin.Notebook_encoder_conditions_page = 1; //speed
			pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "stock_bell_active.png");
		} else {
			feedbackWin.EncoderMeanSpeedHigher = false;
			feedbackWin.EncoderMeanSpeedLower = false;
			feedbackWin.Encoder_show_manual_feedback = false;
			pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "stock_bell_none.png");
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
		button_networks_contacts_guest.Visible = true;
		button_contacts_person_change.Visible = false;

		//not allow to change devices if person changed. If you want to change again, go to preferences/advanced networksAllowChangeDevices
		preferences.networksAllowChangeDevices = false;
		button_encoder_devices_networks.Sensitive = false;
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

	//no GTK here because can be called from rfidChanged()
	private void compujumpSendEmail(Constants.CheckFileOp op)
	{
		if(op == Constants.CheckFileOp.ENCODER_ANALYZE_SEND_IMAGE)
		{
			LogB.Information("Sending Email to: " + configChronojump.CompujumpAdminEmail);

			LogB.Information("Creating CSV ...");

			string tableFile = "/tmp/table.csv";
			//save the csv, but if does not work, leave tableFile empty so it will not be sent
			if(! on_button_encoder_save_table_file_selected (tableFile, false))
				tableFile = "";
			//another option would be use the txt: chronojump-last-encoder-analyze-table.txt

			LogB.Information("CSV created!");

			NetworksSendMail nsm = new NetworksSendMail ();
			if(nsm.Send(exportFileName, UtilEncoder.GetEncoderGraphTempFileName(),
						tableFile, configChronojump.CompujumpAdminEmail))
			{
				rfidWaitingAdminGuiObjects.ShowGuiMessageSuccess = true;
				LogB.Information("Sent!");
			} else {
				rfidWaitingAdminGuiObjects.ShowGuiMessageFailed = true;
				//no GTK here
				//new DialogMessage(Constants.MessageTypes.WARNING, nsm.ErrorStr);
				LogB.Information(nsm.ErrorStr);
			}

		}
		//other future send email send objects
	}

	private void networksShowDeviceMissingEncoder (bool missing)
	{
		if(missing) {
			notebook_sup.CurrentPage = Convert.ToInt32(notebook_sup_pages.NETWORKSPROBLEMS);
			hbox_RFID_disconnected.Visible = false;
			hbox_encoder_disconnected.Visible = true;
			label_encoder_checked_error.Visible = false;
			button_contacts_devices_networks_problems.Sensitive = preferences.networksAllowChangeDevices;
		}
		else {
			notebook_sup.CurrentPage = Convert.ToInt32(notebook_sup_pages.TESTS);
			hbox_RFID_disconnected.Visible = false;
			hbox_encoder_disconnected.Visible = false;
		}
	}

	private void networksShowDeviceMissingRFID (bool missing)
	{
		if(missing) {
			/*
			 * note rfid missing is more important than encoder missing,
			 * if both messages are active, then user can connect encoder, press check button
			 * and it will take to normal capture window (while rfid is not ok or Chronojump has to reboot)
			 * so do not show the encoder message
			 */
			notebook_sup.CurrentPage = Convert.ToInt32(notebook_sup_pages.NETWORKSPROBLEMS);
			hbox_RFID_disconnected.Visible = true;

			button_contacts_devices_networks_problems.Sensitive = preferences.networksAllowChangeDevices;
			hbox_encoder_disconnected.Visible = false;
		}
		else {
			notebook_sup.CurrentPage = Convert.ToInt32(notebook_sup_pages.TESTS);
			hbox_RFID_disconnected.Visible = false;
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

	private void connectWidgetsNetworks (Gtk.Builder builder)
	{
		//custom buttons
		hbox_encoder_analyze_signal_or_curves = (Gtk.HBox) builder.GetObject ("hbox_encoder_analyze_signal_or_curves");
		vbox_start_window_main = (Gtk.VBox) builder.GetObject ("vbox_start_window_main");
		vbox_start_window_sub = (Gtk.VBox) builder.GetObject ("vbox_start_window_sub");
		alignment_start_window = (Gtk.Alignment) builder.GetObject ("alignment_start_window");
		alignment_encoder_capture_options = (Gtk.Alignment) builder.GetObject ("alignment_encoder_capture_options");
		button_contacts_devices_networks_problems = (Gtk.Button) builder.GetObject ("button_contacts_devices_networks_problems");

		//RFID
		label_rfid_wait = (Gtk.Label) builder.GetObject ("label_rfid_wait");

		label_logout_seconds = (Gtk.Label) builder.GetObject ("label_logout_seconds");

		//better raspberry controls
		hbox_sessions_raspberry = (Gtk.HBox) builder.GetObject ("hbox_sessions_raspberry");
		hbox_persons_raspberry = (Gtk.HBox) builder.GetObject ("hbox_persons_raspberry");
		hbox_encoder_capture_extra_mass_no_raspberry = (Gtk.Box) builder.GetObject ("hbox_encoder_capture_extra_mass_no_raspberry");
		hbox_encoder_capture_extra_mass_raspberry = (Gtk.Box) builder.GetObject ("hbox_encoder_capture_extra_mass_raspberry");

		//config.EncoderNameAndCapture
		hbox_top_person = (Gtk.HBox) builder.GetObject ("hbox_top_person");
		label_top_person_name = (Gtk.Label) builder.GetObject ("label_top_person_name");
		image_current_person = (Gtk.Image) builder.GetObject ("image_current_person");
		button_contacts_person_change = (Gtk.Button) builder.GetObject ("button_contacts_person_change");
		button_networks_contacts_guest = (Gtk.Button) builder.GetObject ("button_networks_contacts_guest");

		//encoder exercise stuff
		label_encoder_exercise_encoder = (Gtk.Label) builder.GetObject ("label_encoder_exercise_encoder");
		//vseparator_encoder_exercise_encoder = (Gtk.VSeparator) builder.GetObject ("vseparator_encoder_exercise_encoder");
		hbox_encoder_exercise_encoder = (Gtk.HBox) builder.GetObject ("hbox_encoder_exercise_encoder");
		button_encoder_exercise_edit = (Gtk.Button) builder.GetObject ("button_encoder_exercise_edit");
		button_encoder_exercise_add = (Gtk.Button) builder.GetObject ("button_encoder_exercise_add");
		button_encoder_exercise_delete = (Gtk.Button) builder.GetObject ("button_encoder_exercise_delete");

		//encoder ...
		check_encoder_networks_upload = (Gtk.CheckButton) builder.GetObject ("check_encoder_networks_upload");
		button_encoder_monthly_change_current_session = (Gtk.Button) builder.GetObject ("button_encoder_monthly_change_current_session");
		button_encoder_analyze_image_compujump_send_email = (Gtk.Button) builder.GetObject ("button_encoder_analyze_image_compujump_send_email");
		/*
		   label_RFID_disconnected = (Gtk.Label) builder.GetObject ("label_RFID_disconnected");
		   label_chronopic_encoder = (Gtk.Label) builder.GetObject ("label_chronopic_encoder");
		   image_chronopic_encoder_no = (Gtk.Image) builder.GetObject ("image_chronopic_encoder_no");
		   image_chronopic_encoder_yes = (Gtk.Image) builder.GetObject ("image_chronopic_encoder_yes");
		   */
		hbox_encoder_disconnected = (Gtk.HBox) builder.GetObject ("hbox_encoder_disconnected");
		hbox_RFID_disconnected = (Gtk.HBox) builder.GetObject ("hbox_RFID_disconnected");
		label_encoder_checked_error = (Gtk.Label) builder.GetObject ("label_encoder_checked_error");

		//contacts
		check_contacts_networks_upload = (Gtk.CheckButton) builder.GetObject ("check_contacts_networks_upload");

		//runsInterval
		radio_run_interval_compujump_5m = (Gtk.RadioButton) builder.GetObject ("radio_run_interval_compujump_5m");
		radio_run_interval_compujump_10m = (Gtk.RadioButton) builder.GetObject ("radio_run_interval_compujump_10m");
		radio_run_interval_compujump_15m = (Gtk.RadioButton) builder.GetObject ("radio_run_interval_compujump_15m");
		radio_run_interval_compujump_20m = (Gtk.RadioButton) builder.GetObject ("radio_run_interval_compujump_20m");

		//shown when menu is hidden
		//hbox_menu_and_preferences_outside_menu_contacts = (Gtk.HBox) builder.GetObject ("hbox_menu_and_preferences_outside_menu_contacts");
		//hbox_menu_and_preferences_outside_menu_encoder = (Gtk.HBox) builder.GetObject ("hbox_menu_and_preferences_outside_menu_encoder");
		//button_menu_outside_menu = (Gtk.Button) builder.GetObject ("button_menu_outside_menu");
		//button_menu_outside_menu1 = (Gtk.Button) builder.GetObject ("button_menu_outside_menu1");
	}
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

//this class manages the waiting of admin wristband in tasks like sending an email with the analyze image
public class RFIDWaitingAdminGuiObjects
{
	public bool Waiting;
	public bool ShowGuiMessageSuccess;
	public bool ShowGuiMessageFailed;
	//public string AdminRFID;

	private static DateTime startedDT; //time to wait ID of the admin to have access to "higher" methods
	private const int secondsLimit = 10; //10 seconds to use the admin ID

	//constructor
	public RFIDWaitingAdminGuiObjects()
	{
		Waiting = false;
		ShowGuiMessageSuccess = false;
		ShowGuiMessageFailed = false;
		startedDT = DateTime.MinValue;
		LogB.Information("RFIDWaitingAdminGuiObjects constructed!");
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
