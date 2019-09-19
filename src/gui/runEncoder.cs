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
 * Copyright (C) 2018   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using Gtk;
using Gdk;
using Glade;
using System.Text; //StringBuilder
using System.Collections;
using System.Collections.Generic; //List<T>
using Mono.Unix;


public partial class ChronoJumpWindow 
{
	[Widget] Gtk.MenuItem menuitem_race_encoder_open_folder;
	[Widget] Gtk.CheckMenuItem menuitem_check_race_encoder_capture_simulate;

	[Widget] Gtk.HBox hbox_combo_run_encoder_exercise;
	[Widget] Gtk.ComboBox combo_run_encoder_exercise;
	[Widget] Gtk.SpinButton race_analyzer_spinbutton_distance;
	[Widget] Gtk.SpinButton race_analyzer_spinbutton_temperature;
	[Widget] Gtk.SpinButton race_analyzer_spinbutton_graph_width;
	[Widget] Gtk.SpinButton race_analyzer_spinbutton_graph_height;
	[Widget] Gtk.TextView textview_race_analyzer_comment;
	[Widget] Gtk.HBox hbox_race_analyzer_device;
	[Widget] Gtk.VBox vbox_run_encoder_width_height;
	[Widget] Gtk.RadioButton race_analyzer_radio_device_manual;
	[Widget] Gtk.RadioButton race_analyzer_radio_device_other; //resisted
	[Widget] Gtk.Image image_race_encoder_graph;
	[Widget] Gtk.Button button_run_encoder_recalculate;
	[Widget] Gtk.Button button_race_analyzer_save_comment;

	int race_analyzer_distance;
	int race_analyzer_temperature;
	int race_analyzer_graph_width;
	int race_analyzer_graph_height;
	RunEncoder.Devices race_analyzer_device;


	Thread runEncoderCaptureThread;
	static bool runEncoderProcessFinish;
	static bool runEncoderProcessCancel;
	static bool runEncoderProcessError;
	
	private RunEncoder currentRunEncoder;
	private RunEncoderExercise currentRunEncoderExercise;
	DateTime runEncoderTimeStartCapture;
	bool runEncoderCaptureSimulated;

	static string lastRunEncoderFile = "";
	static string lastRunEncoderFullPath = "";

	//int usbDisconnectedCount;
	//int usbDisconnectedLastTime;
	static arduinoCaptureStatus capturingRunEncoder = arduinoCaptureStatus.STOP;
	/*
	static bool redoingPoints; //don't draw while redoing points (adjusting screen)

	static bool forceCaptureStartMark; 	//Just needed to display "Capturing message"
	static ForceSensorValues forceSensorValues;

	*/
	static string captureEndedMessage;
	string runEncoderPortName;
	SerialPort portRE; //Attention!! Don't reopen port because arduino makes reset
	bool portREOpened;

	string runEncoderNotConnectedString =
		//Catalog.GetString("Run encoder sensor is not detected!") + " " +
		"Run encoder sensor is not detected!" + " " +
		Catalog.GetString("Plug cable and click on 'device' button.");


	//this can use GTK (forceSensor not because it's managed by non-gtk thread)
	private bool runEncoderConnect()
	{
		LogB.Information(" RE connect 0 ");
		if(chronopicRegister.ConnectedOfType(ChronopicRegisterPort.Types.ARDUINO_RUN_ENCODER) == null)
		{
			event_execute_label_message.Text = runEncoderNotConnectedString;
			return false;
		}

		LogB.Information(" RE connect 1 ");
		runEncoderPortName = chronopicRegister.ConnectedOfType(ChronopicRegisterPort.Types.ARDUINO_RUN_ENCODER).Port;
		LogB.Information(" RE connect 2 ");
		if(runEncoderPortName == null || runEncoderPortName == "")
		{
			event_execute_label_message.Text = "Please, select port!";
			return false;
		}
		LogB.Information(" RE connect 3 ");
		event_execute_label_message.Text = "Connecting ...";

		portRE = new SerialPort(runEncoderPortName, 1000000); //runEncoder
		LogB.Information(" RE connect 4: opening port...");

		try {
			portRE.Open();
		}
		catch (System.IO.IOException)
		{
			event_execute_label_message.Text = runEncoderNotConnectedString;
			return false;
		}

		LogB.Information(" RE connect 5: let arduino start");

		Thread.Sleep(3000); //sleep to let arduino start reading serial event

		LogB.Information(" RE connect 6: get version");

		string version = runEncoderCheckVersionDo();
		LogB.Information("Version found: [" + version + "]");

		portREOpened = true;
		event_execute_label_message.Text = "Connected!";
		LogB.Information(" RE connect 7: connected and adjusted!");
		return true;
	}
	private void runEncoderDisconnect()
	{
		portRE.Close();
		portREOpened = false;
		event_execute_label_message.Text = "Disconnected!";
	}

	private string runEncoderCheckVersionDo()
	{
		if(! runEncoderSendCommand("get_version:", "Checking version ...", "Catched checking version"))
			return "";

		string str = "";
		do {
			Thread.Sleep(100); //sleep to let arduino start reading
			try {
				str = portRE.ReadLine().Trim();
			} catch {
				//forceSensorOtherMessage = "Disconnected";
				LogB.Information("catched! checking version");
				return "";
			}
			LogB.Information("init string: " + str);
		}
		while(! str.Contains("Race_Analyzer-"));

		//forceSensorOtherMessageShowSeconds = false;
		//forceSensorOtherMessage = str;

		//return the version without "Race_Analyzer-"
		return(str.Remove(0,14));
	}


	//Attention: no GTK here!!
	private bool runEncoderSendCommand(string command, string displayMessage, string errorMessage)
	{
		//forceSensorOtherMessage = displayMessage;

		try {
			LogB.Information("Run Encoder command |" + command + "|");
			portRE.WriteLine(command);
		}
		catch (Exception ex)
		{
			if(ex is System.IO.IOException || ex is System.TimeoutException)
			{
				LogB.Information(errorMessage);
				portREOpened = false;
				return false;
			}
			//throw;
		}

		return true;
	}

	private void on_runs_encoder_capture_clicked ()
	{
		if(chronopicRegister.NumConnectedOfType(ChronopicRegisterPort.Types.ARDUINO_RUN_ENCODER) == 0)
		{
			event_execute_label_message.Text = runEncoderNotConnectedString;
			return;
		}

		if(currentPersonSession.Weight == 0)
		{
			new DialogMessage(Constants.MessageTypes.WARNING,
					Catalog.GetString("Error, weight of the person cannot be 0"));
			return;
		}

		if(currentPersonSession.Height == 0)
		{
			new DialogMessage(Constants.MessageTypes.WARNING,
					Catalog.GetString("Error, height of the person cannot be 0"));
			return;
		}

		runEncoderCapturePre2_GTK_cameraCall();
	}

	private void runEncoderCapturePre2_GTK_cameraCall()
	{
		on_button_execute_test_acceptedPre_start_camera(
				ChronoJumpWindow.WebcamStartedTestStart.RUNENCODER);
	}

	private void runEncoderCapturePre3_GTK_cameraCalled()
	{
		textview_race_analyzer_comment.Buffer.Text = "";
		assignCurrentRunEncoderExercise();
		raceEncoderReadWidgets();
		runEncoderButtonsSensitive(false);
		button_run_encoder_recalculate.Sensitive = false;
		button_race_analyzer_save_comment.Sensitive = false;

		bool connected = runEncoderCapturePre();
		if(! connected)
			runEncoderButtonsSensitive(true);
	}

	private void runEncoderPersonChanged()
	{
		blankRunEncoderInterface();
	}
	private void blankRunEncoderInterface()
	{
		currentRunEncoder = new RunEncoder();

		button_run_encoder_recalculate.Sensitive = false;
		textview_race_analyzer_comment.Buffer.Text = "";
		button_race_analyzer_save_comment.Sensitive = false;

		button_delete_last_test.Sensitive = false;
	}

	private void initRunEncoder ()
	{
		createRunEncoderExerciseCombo();
	}

	private void raceEncoderReadWidgets()
	{
		race_analyzer_distance = Convert.ToInt32(race_analyzer_spinbutton_distance.Value);
		race_analyzer_temperature = Convert.ToInt32(race_analyzer_spinbutton_temperature.Value);
		race_analyzer_graph_width = Convert.ToInt32(race_analyzer_spinbutton_graph_width.Value);
		race_analyzer_graph_height = Convert.ToInt32(race_analyzer_spinbutton_graph_height.Value);

		if(race_analyzer_radio_device_manual.Active)
			race_analyzer_device = RunEncoder.Devices.MANUAL;
		else
			race_analyzer_device = RunEncoder.Devices.RESISTED;
	}

	private RunEncoder.Devices raceEncoderGetDevice()
	{
		if(race_analyzer_radio_device_manual.Active)
			return RunEncoder.Devices.MANUAL;
		else
			return RunEncoder.Devices.RESISTED;
	}
	private void raceEncoderSetDevice(RunEncoder.Devices d)
	{
		if(d == RunEncoder.Devices.RESISTED)
			race_analyzer_radio_device_other.Active = true;
		else
			race_analyzer_radio_device_manual.Active = true;
	}

	private void raceEncoderSetDistanceAndTemp(int distance, int temp)
	{
		race_analyzer_spinbutton_distance.Value = distance;
		race_analyzer_spinbutton_temperature.Value = temp;
	}

	private void assignCurrentRunEncoderExercise()
	{
		currentRunEncoderExercise = (RunEncoderExercise) SqliteRunEncoderExercise.Select (
                                false, getExerciseIDFromAnyCombo(combo_run_encoder_exercise, runEncoderComboExercisesString, false), false)[0];
	}

	//TODO: do all this with an "other" thread like in force sensor to allow connecting messages to be displayed
	private bool runEncoderCapturePre()
	{
		if(! portREOpened)
			if(! runEncoderConnect())
				return false;

		if(File.Exists(UtilEncoder.GetSprintEncoderImage()))
			Util.FileDelete(UtilEncoder.GetSprintEncoderImage());

		event_execute_label_message.Text = "Please, wait ...";
		captureEndedMessage = "";
		capturingRunEncoder = arduinoCaptureStatus.STARTING;

		button_execute_test.Sensitive = false;
		event_execute_button_finish.Sensitive = true;
		event_execute_button_cancel.Sensitive = true;

		//forceCaptureStartMark = false;

		runEncoderProcessFinish = false;
		runEncoderProcessCancel = false;
		runEncoderProcessError = false;

		//To know if USB has been disconnected
		usbDisconnectedCount = 0;
		usbDisconnectedLastTime = 0;

		runEncoderCaptureSimulated = menuitem_check_race_encoder_capture_simulate.Active;

		/*
		//initialize
		forceSensorValues = new ForceSensorValues();
		*/

		event_execute_ButtonFinish.Clicked -= new EventHandler(on_finish_clicked);
		event_execute_ButtonFinish.Clicked += new EventHandler(on_finish_clicked);
		
		event_execute_ButtonCancel.Clicked -= new EventHandler(on_cancel_clicked);
		event_execute_ButtonCancel.Clicked += new EventHandler(on_cancel_clicked);

		event_execute_label_message.Text = "Capturing ...";

		runEncoderCaptureThread = new Thread(new ThreadStart(runEncoderCaptureDo));
		GLib.Idle.Add (new GLib.IdleHandler (pulseGTKRunEncoderCapture));

		LogB.ThreadStart();
		runEncoderCaptureThread.Start();
		return true;
	}

	//non GTK on this method
	private void runEncoderCaptureDo()
	{
		LogB.Information("runEncoderCaptureDo 0");
		lastChangedTime = 0;

		string command = "start_capture:";
		if(runEncoderCaptureSimulated)
			command = "start_simulation:";

		if(! runEncoderSendCommand(command, "", "Catched run encoder capturing"))
		{
			runEncoderProcessError = true;
			return;
		}

		string str = "";
		LogB.Information("runEncoderCaptureDo 1");
		do {
			Thread.Sleep(100); //sleep to let arduino start reading
			try {
				str = portRE.ReadLine();
			} catch {
				runEncoderProcessError = true;
				return;
			}

			LogB.Information("init string: " + str);
		}
		while(! str.Contains("Starting capture"));

		//forceCaptureStartMark = true;
		capturingRunEncoder = arduinoCaptureStatus.CAPTURING;

		Util.CreateRunEncoderSessionDirIfNeeded (currentSession.UniqueID);

		runEncoderTimeStartCapture = DateTime.Now; //to have an active count of capture time
		string idNameDate = currentPerson.UniqueID + "_" + currentPerson.Name + "_" + UtilDate.ToFile(runEncoderTimeStartCapture);

		//fileName to save the csv
		string fileName = Util.GetRunEncoderSessionDir(currentSession.UniqueID) + Path.DirectorySeparatorChar + idNameDate + ".csv";

		//lastRunEncoderFile to save the images
		lastRunEncoderFile = idNameDate;


		TextWriter writer = File.CreateText(fileName);
		writer.WriteLine("Pulses;Time(useconds);Force(N)");
		str = "";
		int firstTime = 0;

		while(! runEncoderProcessFinish && ! runEncoderProcessCancel && ! runEncoderProcessError)
		{
			//LogB.Information(string.Format("finish conditions: {0}-{1}-{2}",
			//			runEncoderProcessFinish, runEncoderProcessCancel, runEncoderProcessError));

			/*
			 * The difference between forceSensor and runEncoder is:
			 * runEncoder is not always returning data
			 * if user press "finish" button, and they don't move the encoder,
			 * this will never end:
			 * //str = portRE.ReadLine();
			 * so use the following method that allows to return a "" when there no data
			 * and then the while above will end with the runEncoderProcessFinish condition
			 */
			str = readFromRunEncoderIfDataArrived();
			if(! checkRunEncoderCaptureLineIsOk(str))
				continue;

			/*
			int time = Convert.ToInt32(strFull[0]);

			//measurement does not start at 0 time. When we start receiving data, mark this as firstTime
			if(firstTime == 0)
				firstTime = time;

			//use this to have time starting at 0
			time -= firstTime;

			double force = Convert.ToDouble(Util.ChangeDecimalSeparator(strFull[1]));
			*/

			string [] strFull = str.Split(new char[] {';'});
			writer.WriteLine(string.Format("{0};{1};{2}",
						Convert.ToInt32(strFull[0]), //pulse
						Convert.ToInt32(strFull[1]), //time
						Convert.ToInt32(strFull[2]) //force
						));
		}

		LogB.Information(string.Format("FINISHED WITH conditions: {0}-{1}-{2}",
						runEncoderProcessFinish, runEncoderProcessCancel, runEncoderProcessError));
		LogB.Information("Calling end_capture");
		if(! runEncoderSendCommand("end_capture:", "Ending capture ...", "Catched ending capture"))
		{
			runEncoderProcessError = true;
			capturingRunEncoder = arduinoCaptureStatus.STOP;
			Util.FileDelete(fileName);
			return;
		}

		LogB.Information("Waiting end_capture");
		do {
			Thread.Sleep(10);
			try {
				str = portRE.ReadLine();

				if(checkRunEncoderCaptureLineIsOk(str))
				{
					LogB.Information("Processing last received line");
					string [] strFull = str.Split(new char[] {';'});
					writer.WriteLine(string.Format("{0};{1};{2}",
								Convert.ToInt32(strFull[0]), //pulse
								Convert.ToInt32(strFull[1]), //time
								Convert.ToInt32(strFull[2]) //force
								));
				}
			} catch {
				LogB.Information("Catched waiting end_capture feedback");
			}
			LogB.Information("waiting \"Capture ended\" string: " + str);
		}
		while(! str.Contains("Capture ended"));
		LogB.Information("Success: received end_capture");

		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();
		capturingRunEncoder = arduinoCaptureStatus.STOP;

		//port.Close();

		if(runEncoderProcessCancel || runEncoderProcessError)
			Util.FileDelete(fileName);
		else {
			//call graph. Prepare data
			File.Copy(fileName, RunEncoder.GetCSVFileName(), true); //can be overwritten
			//lastRunEncoderFullPath = fileName;

			raceEncoderCaptureGraphDo();

			capturingRunEncoder = arduinoCaptureStatus.COPIED_TO_TMP;
		}
	}

	private void on_button_run_encoder_load_clicked (object o, EventArgs args)
	{
		ArrayList data = SqliteRunEncoder.Select(false, -1, currentPerson.UniqueID, currentSession.UniqueID);

		ArrayList dataPrint = new ArrayList();
		int count = 1;
		foreach(RunEncoder re in data)
			dataPrint.Add(re.ToStringArray(count++));

		string [] columnsString = {
			Catalog.GetString("ID"),
			Catalog.GetString("Set"),
			Catalog.GetString("Exercise"),
			Catalog.GetString("Device"),
			Catalog.GetString("Distance"),
			Catalog.GetString("Date"),
			Catalog.GetString("Video"),
			Catalog.GetString("Comment")
		};

		ArrayList bigArray = new ArrayList();
		ArrayList a1 = new ArrayList();
		ArrayList a2 = new ArrayList();
		//0 is the widgget to show; 1 is the editable; 2 id default value
		a1.Add(Constants.GenericWindowShow.TREEVIEW); a1.Add(true); a1.Add("");
		bigArray.Add(a1);

		a2.Add(Constants.GenericWindowShow.COMBO); a2.Add(true); a2.Add("");
		bigArray.Add(a2);

		genericWin = GenericWindow.Show(Catalog.GetString("Load"), false,	//don't show now
				string.Format(Catalog.GetString("Select set of athlete {0} on this session."),
					currentPerson.Name)
					+ "\n" +
				Catalog.GetString("If you want to edit or delete a row, right click on it.")
				, bigArray);

		genericWin.SetTreeview(columnsString, false, dataPrint, new ArrayList(), GenericWindow.EditActions.EDITDELETE, true);

		//find all persons in current session
		ArrayList personsPre = SqlitePersonSession.SelectCurrentSessionPersons(currentSession.UniqueID,
				false); //means: do not returnPersonAndPSlist

		string [] persons = new String[personsPre.Count];
		count = 0;
	        foreach	(Person p in personsPre)
			persons[count++] = p.UniqueID.ToString() + ":" + p.Name;
		genericWin.SetComboValues(persons, currentPerson.UniqueID + ":" + currentPerson.Name);
		genericWin.SetComboLabel(Catalog.GetString("Change the owner of selected set") +
				" (" + Catalog.GetString("code") + ":" + Catalog.GetString("name") + ")");
		genericWin.ShowEditRow(false);

		//select row corresponding to current signal
		genericWin.SelectRowWithID(0, currentRunEncoder.UniqueID); //colNum, id

		genericWin.CommentColumn = 7;

		genericWin.ShowButtonCancel(true);
		genericWin.SetButtonAcceptLabel(Catalog.GetString("Load"));
		genericWin.SetButtonCancelLabel(Catalog.GetString("Close"));
		genericWin.SetButtonAcceptSensitive(false);
		genericWin.Button_accept.Clicked += new EventHandler(on_run_encoder_load_accepted);
		genericWin.Button_row_edit.Clicked += new EventHandler(on_run_encoder_load_signal_row_edit);
		genericWin.Button_row_edit_apply.Clicked += new EventHandler(on_run_encoder_load_signal_row_edit_apply);
		genericWin.Button_row_delete.Clicked += new EventHandler(on_run_encoder_load_signal_row_delete_prequestion);

		genericWin.ShowNow();
	}

	private void on_run_encoder_load_accepted (object o, EventArgs args)
	{
		LogB.Information("on run encoder load accepted");
		genericWin.Button_accept.Clicked -= new EventHandler(on_run_encoder_load_accepted);

		int uniqueID = genericWin.TreeviewSelectedRowID();

		genericWin.HideAndNull();

		ArrayList data = SqliteRunEncoder.Select(false, uniqueID, currentPerson.UniqueID, currentSession.UniqueID);
		RunEncoder re = (RunEncoder) data[0];
		if(re == null)
		{
			new DialogMessage(Constants.MessageTypes.WARNING, Constants.FileNotFoundStr());
			return;
		}

		if(! Util.FileExists(re.FullURL))
		{
			new DialogMessage(Constants.MessageTypes.WARNING, Constants.FileNotFoundStr());
			return;
		}

		currentRunEncoder = re;
		lastRunEncoderFile = Util.RemoveExtension(re.Filename);
		lastRunEncoderFullPath = re.FullURL;

		combo_run_encoder_exercise.Active = UtilGtk.ComboMakeActive(combo_run_encoder_exercise, Catalog.GetString(re.ExerciseName));
		assignCurrentRunEncoderExercise();

		raceEncoderSetDevice(re.Device);
		raceEncoderSetDistanceAndTemp(re.Distance, re.Temperature);
		textview_race_analyzer_comment.Buffer.Text = re.Comments;

		raceEncoderReadWidgets(); //needed to be able to do R graph

		raceEncoderCopyTempAndDoGraphs();

		button_run_encoder_recalculate.Sensitive = true;
		button_race_analyzer_save_comment.Sensitive = true;

		button_video_play_this_test.Sensitive = (re.VideoURL != "");
		sensitiveLastTestButtons(true);

		event_execute_label_message.Text = "Loaded: " + Util.GetLastPartOfPath(re.Filename);
	}

	protected void on_run_encoder_load_signal_row_edit (object o, EventArgs args) {
		LogB.Information("row edit at load signal");
		LogB.Information(genericWin.TreeviewSelectedUniqueID.ToString());
		genericWin.ShowEditRow(true);
	}

	protected void on_run_encoder_load_signal_row_edit_apply (object o, EventArgs args)
	{
		LogB.Information("row edit apply at load signal. Opening db:");

		Sqlite.Open();

		//1) select set
		int setID = genericWin.TreeviewSelectedUniqueID;
		RunEncoder re = (RunEncoder) SqliteRunEncoder.Select(true, setID, -1, -1)[0];

		//2) if changed comment, update SQL, and update treeview
		//first remove conflictive characters
		string comment = Util.RemoveTildeAndColonAndDot(genericWin.EntryEditRow);
		if(comment != re.Comments)
		{
			re.Comments = comment;
			re.UpdateSQLJustComments(true);

			//update treeview
			genericWin.on_edit_selected_done_update_treeview();
		}

		//3) change the session param and the url of signal and curves (if any)
		string idName = genericWin.GetComboSelected;
		LogB.Information("new person: " + idName);
		int newPersonID = Util.FetchID(idName);
		if(newPersonID != currentPerson.UniqueID)
		{
			//change stuff on signal
			RunEncoder reChangedPerson = re.ChangePerson(idName);
			reChangedPerson.UpdateSQL(true);
			genericWin.RemoveSelectedRow();
			genericWin.SetButtonAcceptSensitive(false);
		}

		genericWin.ShowEditRow(false);
		genericWin.SensitiveEditDeleteIfSelected();

		//remove signal from gui just in case the edited signal is the same we have loaded
		//removeSignalFromGuiBecauseDeletedOrCancelled();
		blankRunEncoderInterface();

		Sqlite.Close();
	}

	// ----start of runEncoderDeleteTest stuff -------

	protected void on_run_encoder_load_signal_row_delete_prequestion (object o, EventArgs args)
	{
		if(preferences.askDeletion) {
			ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString(
						"Are you sure you want to delete this set?"), "", "");
			confirmWin.Button_accept.Clicked += new EventHandler(on_run_encoder_load_signal_row_delete);
		} else
			on_run_encoder_load_signal_row_delete (o, args);
	}

	protected void on_run_encoder_load_signal_row_delete (object o, EventArgs args)
	{
		LogB.Information("row delete at load set");

		int setID = genericWin.TreeviewSelectedUniqueID;
		LogB.Information(setID.ToString());

		//if it's current set use the delete set from the gui interface that updates gui
		if(currentRunEncoder != null && setID == Convert.ToInt32(currentRunEncoder.UniqueID))
			run_encoder_delete_current_test_accepted(o, args);
		else {
			RunEncoder re = (RunEncoder) SqliteRunEncoder.Select(false, setID, -1, -1)[0];
			runEncoderDeleteTestDo(re);

			//genericWin selected row is deleted, unsensitive the "load" button
			genericWin.SetButtonAcceptSensitive(false);
		}
		genericWin.Delete_row_accepted();
	}

	private void run_encoder_delete_current_test_pre_question()
	{
		//solve possible gui problems
		if(currentRunEncoder == null || currentRunEncoder.UniqueID == -1)
		{
			new DialogMessage(Constants.MessageTypes.WARNING, "Test does not exists. Cannot be deleted");
			return;
		}

		if(preferences.askDeletion) {
			ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString(
						"Are you sure you want to delete this set?"), "", "");
			confirmWin.Button_accept.Clicked += new EventHandler(run_encoder_delete_current_test_accepted);
		} else
			run_encoder_delete_current_test_accepted(new object(), new EventArgs());
	}
	private void run_encoder_delete_current_test_accepted(object o, EventArgs args)
	{
		runEncoderDeleteTestDo(currentRunEncoder);

		//empty currentRunEncoder (assign -1)
		currentRunEncoder = new RunEncoder();

		//empty GUI
		blankRunEncoderInterface();
	}

	private void runEncoderDeleteTestDo(RunEncoder re)
	{
		//int uniqueID = currentRunEncoder.UniqueID;
		SqliteRunEncoder.DeleteSQLAndFiles (false, re); //deletes also the .csv
	}

	// --- end of runEncoderDeleteTest stuff -------


	private void on_button_run_encoder_recalculate_clicked (object o, EventArgs args)
	{
		if(! Util.FileExists(lastRunEncoderFullPath))
		{
			new DialogMessage(Constants.MessageTypes.WARNING, Constants.FileNotFoundStr());
			return;
		}

		assignCurrentRunEncoderExercise();

		raceEncoderReadWidgets();
		if(lastRunEncoderFullPath != null && lastRunEncoderFullPath != "")
			raceEncoderCopyTempAndDoGraphs();

		button_run_encoder_recalculate.Sensitive = false; //to not be called two times

		event_execute_label_message.Text = "Recalculated.";

		radio_mode_contacts_analyze.Active = true;
		button_run_encoder_recalculate.Sensitive = true;

		//update SQL with exercise, device, distance, temperature, comments
		currentRunEncoder.ExerciseID = currentRunEncoderExercise.UniqueID;
		currentRunEncoder.ExerciseName = currentRunEncoderExercise.Name; //just in case
		currentRunEncoder.Device = raceEncoderGetDevice();
		currentRunEncoder.Distance = Convert.ToInt32(race_analyzer_spinbutton_distance.Value);
		currentRunEncoder.Temperature = Convert.ToInt32(race_analyzer_spinbutton_temperature.Value);
		currentRunEncoder.Comments = UtilGtk.TextViewGetCommentValidSQL(textview_race_analyzer_comment);

		currentRunEncoder.UpdateSQL(false);
	}

	private void on_button_race_analyzer_save_comment_clicked (object o, EventArgs args)
	{
		currentRunEncoder.Comments = UtilGtk.TextViewGetCommentValidSQL(textview_race_analyzer_comment);
		currentRunEncoder.UpdateSQLJustComments(false);
	}

	private void raceEncoderCopyTempAndDoGraphs()
	{
		File.Copy(lastRunEncoderFullPath, RunEncoder.GetCSVFileName(), true); //can be overwritten

		raceEncoderCaptureGraphDo();

		Thread.Sleep (250); //Wait a bit to ensure is copied

		runEncoderAnalyzeOpenImage();
		notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.RACEENCODER);
		radio_mode_contacts_analyze.Active = true;
	}

	private void raceEncoderCaptureGraphDo()
	{
		//create graph
		RunEncoderGraph reg = new RunEncoderGraph(
				race_analyzer_distance,
				currentPersonSession.Weight,  	//TODO: can be more if extra weight
				currentPersonSession.Height,
				race_analyzer_temperature,
				race_analyzer_device);

		reg.CallR(race_analyzer_graph_width, race_analyzer_graph_height);

		DateTime runEncoderGraphStarted = DateTime.Now;
		//TODO: check better if png is saved and have a cancel button

		while(! File.Exists(UtilEncoder.GetSprintEncoderImage()) && DateTime.Now.Subtract(runEncoderGraphStarted).TotalSeconds < 5)
			Thread.Sleep(500);

		captureEndedMessage = "Data on raceAnalyzer folder";
		if(File.Exists(UtilEncoder.GetSprintEncoderImage()))
		{
			LogB.Information("File exists on png, trying to copy");
			try {
				File.Copy(UtilEncoder.GetSprintEncoderImage(),
						Util.GetRunEncoderSessionDir(currentSession.UniqueID) + Path.DirectorySeparatorChar +
						lastRunEncoderFile + 	//idNameDate
						".png",
						true); //can be overwritten
				captureEndedMessage += " (png too)";
			} catch {
				LogB.Information("Couldn't copy the file");
				captureEndedMessage += " (Created png but only on tmp folder, could not copy file)";
			}
		} else {
			LogB.Information("File does not exist on png (after 5 seconds)");
			captureEndedMessage += " (png not created, problem doing the graph)";
		}
	}

	private string readFromRunEncoderIfDataArrived()
	{
		string str = "";
		if (portRE.BytesToRead > 0)
			str = portRE.ReadLine();
			//LogB.Information("PRE_get_calibrationfactor bytes: " + portRE.ReadExisting());

		return str;
	}

	private bool checkRunEncoderCaptureLineIsOk(string str)
	{
		//LogB.Information("str: " + str); //TODO: remove this log
		if(str == "")
			return false;

		//check if there is one and only one ';'
		//if( ! (str.Contains(";") && str.IndexOf(";") == str.LastIndexOf(";")) )

		string [] strFull = str.Split(new char[] {';'});
		LogB.Information("captured str: " + str);

		if(strFull.Length != 3)
			return false;

		LogB.Information("pulses: " + strFull[0]);
		if(! Util.IsNumber(strFull[0], false))
			return false;

		LogB.Information("time microseconds: " + strFull[1]);
		if(! Util.IsNumber(strFull[1], false))
			return false;

		LogB.Information("force avg (N): " + strFull[1]);
		if(! Util.IsNumber(strFull[2], false))
			return false;

		return true;
	}

	private bool pulseGTKRunEncoderCapture ()
	{
LogB.Information(" fc A ");
		if(runEncoderCaptureThread == null)
		{
			Thread.Sleep (25);
			return true;
		}

LogB.Information(" fc B ");
		//LogB.Information(capturingRunEncoder.ToString())
		if(! runEncoderCaptureThread.IsAlive || runEncoderProcessFinish || runEncoderProcessCancel || runEncoderProcessError)
		{
LogB.Information(" fc C ");
			button_video_play_this_test.Sensitive = false;
			if(runEncoderProcessFinish)
			{
LogB.Information(" fc C finish");
				if(capturingRunEncoder != arduinoCaptureStatus.COPIED_TO_TMP)
				{
					Thread.Sleep (25); //Wait file is copied
					return true;
				}
				else
				{
					event_execute_label_message.Text = "Saved." + captureEndedMessage;

					currentRunEncoder = new RunEncoder(-1, currentPerson.UniqueID, currentSession.UniqueID,
							currentRunEncoderExercise.UniqueID, raceEncoderGetDevice(),
							Convert.ToInt32(race_analyzer_spinbutton_distance.Value),
							Convert.ToInt32(race_analyzer_spinbutton_temperature.Value),
							Util.GetLastPartOfPath(lastRunEncoderFile + ".csv"), //filename
							Util.MakeURLrelative(Util.GetRunEncoderSessionDir(currentSession.UniqueID)), //url
							UtilDate.ToFile(runEncoderTimeStartCapture),
							"", //on capture cannot store comment (comment has to be written after),
							"", //videoURL
							currentRunEncoderExercise.Name);

					currentRunEncoder.UniqueID = currentRunEncoder.InsertSQL(false);

					//stop camera
					if(webcamEnd (Constants.TestTypes.RACEANALYZER, currentRunEncoder.UniqueID))
					{
						//add the videoURL to SQL
						currentRunEncoder.VideoURL = Util.GetVideoFileName(currentSession.UniqueID,
								Constants.TestTypes.RACEANALYZER,
								currentRunEncoder.UniqueID);
						currentRunEncoder.UpdateSQL(false);
						label_video_feedback.Text = "";
						button_video_play_this_test.Sensitive = true;
					}

					Thread.Sleep (250); //Wait a bit to ensure is copied
					sensitiveLastTestButtons(true);

					runEncoderAnalyzeOpenImage();
					notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.RACEENCODER);
					radio_mode_contacts_analyze.Active = true;
					button_run_encoder_recalculate.Sensitive = true;
					button_race_analyzer_save_comment.Sensitive = true;
					button_delete_last_test.Sensitive = true;

					/*
					fscPoints.InitRealWidthHeight();
					forceSensorDoSignalGraphPlot();
					forceSensorDoRFDGraph();

					//if drawingarea has still not shown, don't paint graph because GC screen is not defined
					if(force_sensor_ai_drawingareaShown)
						forceSensorDoGraphAI();
					*/
				}
LogB.Information(" fc C finish 2");
			} else if(runEncoderProcessCancel || runEncoderProcessError)
			{
LogB.Information(" fc C cancel ");
				//stop the camera (and do not save)
				webcamEnd (Constants.TestTypes.RACEANALYZER, -1);
				sensitiveLastTestButtons(false);
				button_delete_last_test.Sensitive = false;

				if(runEncoderProcessCancel)
					event_execute_label_message.Text = "Cancelled.";
				else
					event_execute_label_message.Text = runEncoderNotConnectedString;
LogB.Information(" fc C cancel 2");
			}
			else
				event_execute_label_message.Text = "";

			LogB.ThreadEnding();

			/*
			 * ensure runEncoderCaptureThread is ended:
			 * called: portRE.WriteLine("end_capture:");
			 * and received feedback from device
			 */
			while(runEncoderCaptureThread.IsAlive)
				Thread.Sleep (250);
LogB.Information(" fc D ");

			LogB.ThreadEnded(); 

			runEncoderButtonsSensitive(true);
			/*
			button_force_sensor_image_save_signal.Sensitive = true;
			button_force_sensor_analyze_recalculate.Sensitive = true;
			*/

			//finish, cancel: sensitive = false
			hideButtons();

			restTime.AddOrModify(currentPerson.UniqueID, currentPerson.Name, true);
			updateRestTimes();

			return false;
		}

LogB.Information(" fc E ");
/*
		if(forceCaptureStartMark)
		{
			event_execute_label_message.Text = "Capturing ...";
			forceCaptureStartMark = false;
		}
		*/
LogB.Information(" fc F ");

		if(capturingRunEncoder == arduinoCaptureStatus.CAPTURING)
		{
LogB.Information(" fc G ");


LogB.Information(" fc H2 ");
/*
			if(usbDisconnectedLastTime == forceSensorValues.TimeLast)
			{
				usbDisconnectedCount ++;
				if(usbDisconnectedCount >= 20)
				{
					event_execute_label_message.Text = "Disconnected!";
					runEncoderProcessError = true;
					return true;
				}
			}
			else
			{
				usbDisconnectedLastTime = forceSensorValues.TimeLast;
				usbDisconnectedCount = 0;
			}
			*/

LogB.Information(" fc I ");

LogB.Information(" fc Q ");
		}
LogB.Information(" fc R ");

		Thread.Sleep (25);
		//LogB.Information(" RunEncoder:"+ runEncoderCaptureThread.ThreadState.ToString());
		return true;
	}

	void runEncoderButtonsSensitive(bool sensitive)
	{
		button_execute_test.Sensitive = sensitive;

		//other gui buttons
		main_menu.Sensitive = sensitive;
		hbox_contacts_sup_capture_analyze_two_buttons.Sensitive = sensitive;
		frame_persons.Sensitive = sensitive;
		hbox_top_person.Sensitive = sensitive;
		hbox_chronopics_and_more.Sensitive = sensitive;
	}

	void runEncoderAnalyzeOpenImage()
	{
		string imagePath = UtilEncoder.GetSprintEncoderImage();
		image_race_encoder_graph = UtilGtk.OpenImageSafe(
				imagePath,
				image_race_encoder_graph);
	}

	private void on_menuitem_race_encoder_open_folder_activate (object o, EventArgs args)
	{
		if(currentSession == null || currentSession.UniqueID == -1)
		{
			System.Diagnostics.Process.Start(RunEncoderGraph.GetDataDir(-1));
			return;
		}

		string dataDir = RunEncoderGraph.GetDataDir(currentSession.UniqueID);
		if(dataDir != "")
			System.Diagnostics.Process.Start(dataDir);
		else
			new DialogMessage(Constants.MessageTypes.WARNING, Constants.DirectoryCannotOpenStr());
	}

	// -------------------------------- exercise stuff --------------------


	string [] runEncoderComboExercisesString; //id:name (no translations, use user language)

	private void createRunEncoderExerciseCombo ()
	{
		//run_encoder_exercise

		combo_run_encoder_exercise = ComboBox.NewText ();
		fillRunEncoderExerciseCombo("");

//		combo_run_encoder_exercise.Changed += new EventHandler (on_combo_run_encoder_exercise_changed);
		hbox_combo_run_encoder_exercise.PackStart(combo_run_encoder_exercise, true, true, 0);
		hbox_combo_run_encoder_exercise.ShowAll();
	}

	private void fillRunEncoderExerciseCombo(string name)
	{
		ArrayList runEncoderExercises = SqliteRunEncoderExercise.Select (false, -1, false);
		if(runEncoderExercises.Count == 0)
		{
			runEncoderComboExercisesString = new String [0];
			UtilGtk.ComboUpdate(combo_run_encoder_exercise, new String[0], "");

			return;
		}

		runEncoderComboExercisesString = new String [runEncoderExercises.Count];
		string [] exerciseNamesToCombo = new String [runEncoderExercises.Count];
		int i =0;
		foreach(RunEncoderExercise ex in runEncoderExercises)
		{
			exerciseNamesToCombo[i] = ex.Name;
			runEncoderComboExercisesString[i] = ex.UniqueID + ":" + ex.Name;
			i++;
		}

		UtilGtk.ComboUpdate(combo_run_encoder_exercise, exerciseNamesToCombo, "");
		if(name == "")
			combo_run_encoder_exercise.Active = 0;
		else
			combo_run_encoder_exercise.Active = UtilGtk.ComboMakeActive(combo_run_encoder_exercise, name);
	}

	//info is now info and edit (all values can be changed), and detete (there's delete button)
	void on_button_run_encoder_exercise_edit_clicked (object o, EventArgs args)
	{
		if(UtilGtk.ComboGetActive(combo_run_encoder_exercise) == "")
		{
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Need to create/select an exercise."));
			return;
		}

		RunEncoderExercise ex = (RunEncoderExercise) SqliteRunEncoderExercise.Select (
                                false, getExerciseIDFromAnyCombo(combo_run_encoder_exercise, runEncoderComboExercisesString, false), false)[0];

		LogB.Information("selected exercise: " + ex.ToString());

		ArrayList bigArray = new ArrayList();

		ArrayList a1 = new ArrayList();
		ArrayList a2 = new ArrayList();

		//0 is the widgget to show; 1 is the editable; 2 id default value
		a1.Add(Constants.GenericWindowShow.ENTRY); a1.Add(true); a1.Add(ex.Name); //name can be changed (opposite to encoder), because we use always the uniqueID
		bigArray.Add(a1);

		a2.Add(Constants.GenericWindowShow.ENTRY2); a2.Add(true); a2.Add(ex.Description);
		bigArray.Add(a2);

		genericWin = GenericWindow.Show(Catalog.GetString("Exercise"), false,	//don't show now
				Catalog.GetString("Force sensor exercise:"), bigArray);
		genericWin.LabelEntry2 = Catalog.GetString("Description");

		genericWin.ShowButtonCancel(false);

		genericWin.ShowButtonDelete(true);
		genericWin.Button_delete.Clicked += new EventHandler(on_button_run_encoder_exercise_delete);

		genericWin.nameUntranslated = ex.Name;
		genericWin.uniqueID = ex.UniqueID;

		genericWin.Button_accept.Clicked += new EventHandler(on_button_run_encoder_exercise_edit_accepted);
		genericWin.ShowNow();
	}

	private void on_button_run_encoder_exercise_add_clicked (object o, EventArgs args)
	{
		ArrayList bigArray = new ArrayList();

		ArrayList a1 = new ArrayList();
		ArrayList a2 = new ArrayList();

		//0 is the widgget to show; 1 is the editable; 2 id default value
		a1.Add(Constants.GenericWindowShow.ENTRY); a1.Add(true); a1.Add("");
		bigArray.Add(a1);

		a2.Add(Constants.GenericWindowShow.ENTRY2); a2.Add(true); a2.Add("");
		bigArray.Add(a2);

		genericWin = GenericWindow.Show(Catalog.GetString("Exercise"), false,	//don't show now
				Catalog.GetString("Write the name of the exercise:"), bigArray);
		genericWin.LabelEntry2 = Catalog.GetString("Description");

		genericWin.SetButtonAcceptLabel(Catalog.GetString("Add"));

		genericWin.HideOnAccept = false;

		genericWin.Button_accept.Clicked += new EventHandler(on_button_run_encoder_exercise_add_accepted);
		genericWin.ShowNow();
	}

	void on_button_run_encoder_exercise_edit_accepted (object o, EventArgs args)
	{
		if(run_encoder_exercise_do_add_or_edit(false))
		{
			genericWin.Button_accept.Clicked -= new EventHandler(on_button_run_encoder_exercise_edit_accepted);
			genericWin.HideAndNull();
		}
	}
	void on_button_run_encoder_exercise_add_accepted (object o, EventArgs args)
	{
		if(run_encoder_exercise_do_add_or_edit(true))
		{
			genericWin.Button_accept.Clicked -= new EventHandler(on_button_run_encoder_exercise_add_accepted);
			genericWin.HideAndNull();
		}
	}

	bool run_encoder_exercise_do_add_or_edit (bool adding)
	{
		string name = Util.MakeValidSQLAndFileName(Util.RemoveTildeAndColonAndDot(genericWin.EntrySelected));
		name = Util.RemoveChar(name, '"');

		if(adding)
			LogB.Information("run_encoder_exercise_do - Trying to insert: " + name);
		else
			LogB.Information("run_encoder_exercise_do - Trying to edit: " + name);

		if(name == "")
			genericWin.SetLabelError(Catalog.GetString("Error: Missing name of exercise."));
		else if (adding && Sqlite.Exists(false, Constants.RunEncoderExerciseTable, name))
			genericWin.SetLabelError(string.Format(Catalog.GetString(
							"Error: An exercise named '{0}' already exists."), name));
		else {
			if(adding)
				SqliteRunEncoderExercise.Insert(false, -1, name, genericWin.Entry2Selected);
			else {
				RunEncoderExercise ex = new RunEncoderExercise(genericWin.uniqueID, name, genericWin.Entry2Selected);
				SqliteRunEncoderExercise.Update(false, ex);
			}

			fillRunEncoderExerciseCombo(name);

			LogB.Information("done");
			return true;
		}

		return false;
	}

	//based on: on_button_encoder_exercise_delete
	//maybe unify them on the future
	void on_button_run_encoder_exercise_delete (object o, EventArgs args)
	{
		int exerciseID = genericWin.uniqueID;

		//1st find if there are sets with this exercise
		ArrayList array = SqliteRunEncoder.SelectRowsOfAnExercise(false, exerciseID);

		if(array.Count > 0) {
			//there are some records of this exercise on encoder table, do not delete
			genericWin.SetTextview(
					Catalog.GetString("Sorry, this exercise cannot be deleted until these tests are deleted:"));

			ArrayList nonSensitiveRows = new ArrayList();
			for(int i=0; i < array.Count; i ++)
				nonSensitiveRows.Add(i);

			genericWin.SetTreeview(
					new string [] {
					"count",	//not shown, unused
					Catalog.GetString("Sets"), Catalog.GetString("Person"),
					Catalog.GetString("Session"), Catalog.GetString("Date") },
					false, array, nonSensitiveRows, GenericWindow.EditActions.NONE, false);

			genericWin.ShowTextview();
			genericWin.ShowTreeview();
			genericWin.ShowButtonDelete(false);
			genericWin.DeletingExerciseHideSomeWidgets();

			genericWin.Button_accept.Clicked -= new EventHandler(on_button_run_encoder_exercise_edit_accepted);
			genericWin.Button_accept.Clicked += new EventHandler(on_button_run_encoder_exercise_do_not_delete);
		} else {
			//runEncoder table has not records of this exercise. Delete exercise
			SqliteRunEncoderExercise.Delete(false, exerciseID);

			genericWin.HideAndNull();

			fillRunEncoderExerciseCombo("");
			combo_run_encoder_exercise.Active = 0;

			new DialogMessage(Constants.MessageTypes.INFO, Catalog.GetString("Exercise deleted."));
		}
	}

	//accept does not save changes, just closes window
	void on_button_run_encoder_exercise_do_not_delete (object o, EventArgs args) {
		genericWin.Button_accept.Clicked -= new EventHandler(on_button_run_encoder_exercise_do_not_delete);
		genericWin.HideAndNull();
	}
	// -------------------------------- end of exercise stuff --------------------

}
