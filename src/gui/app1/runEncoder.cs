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
 * Copyright (C) 2018-2020   Xavier de Blas <xaviblas@gmail.com>
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
using System.Text.RegularExpressions; //Regex
using System.Diagnostics;  //Stopwatch
using Mono.Unix;


public partial class ChronoJumpWindow 
{
	//[Widget] Gtk.CheckMenuItem menuitem_check_race_encoder_capture_simulate;

	[Widget] Gtk.Button button_combo_run_encoder_exercise_capture_left;
	[Widget] Gtk.Button button_combo_run_encoder_exercise_capture_right;
	[Widget] Gtk.VBox vbox_run_encoder_capture_buttons;
	[Widget] Gtk.VBox vbox_run_encoder_capture_options;
	[Widget] Gtk.HBox hbox_combo_run_encoder_exercise;
	[Widget] Gtk.ComboBox combo_run_encoder_exercise;
	[Widget] Gtk.SpinButton race_analyzer_spinbutton_distance;
	[Widget] Gtk.SpinButton race_analyzer_spinbutton_temperature;
	[Widget] Gtk.ComboBox combo_race_analyzer_device;
	[Widget] Gtk.Image image_run_encoder_graph;
	[Widget] Gtk.Viewport viewport_run_encoder_graph;
	[Widget] Gtk.TreeView treeview_raceAnalyzer;
	[Widget] Gtk.Button button_raceAnalyzer_table_save;
	//[Widget] Gtk.Label label_race_analyzer_capture_speed;
	[Widget] Gtk.HBox hbox_race_analyzer_capture_tab_result_views;
	[Widget] Gtk.RadioButton radio_race_analyzer_capture_view_simple;
	[Widget] Gtk.RadioButton radio_race_analyzer_capture_view_complete;
	[Widget] Gtk.Alignment alignment_drawingarea_race_analyzer_capture_velocimeter_topleft;
	[Widget] Gtk.Alignment alignment_hbox_race_analyzer_capture_bottom;
	[Widget] Gtk.DrawingArea drawingarea_race_analyzer_capture_velocimeter_topleft;
	[Widget] Gtk.DrawingArea drawingarea_race_analyzer_capture_velocimeter_bottom;
	[Widget] Gtk.DrawingArea drawingarea_race_analyzer_capture_position_time;
	[Widget] Gtk.DrawingArea drawingarea_race_analyzer_capture_speed_time;
	[Widget] Gtk.DrawingArea drawingarea_race_analyzer_capture_accel_time;

	int race_analyzer_distance;
	int race_analyzer_temperature;
	RunEncoder.Devices race_analyzer_device;


	Thread runEncoderCaptureThread;
	static bool runEncoderProcessFinish;
	static bool runEncoderProcessCancel;
	static bool runEncoderProcessError;
        static string runEncoderPulseMessage = "";
	static bool runEncoderShouldShowCaptureGraphsWithData; //on change person this is false
	
	private RunEncoder currentRunEncoder;
	private RunEncoderExercise currentRunEncoderExercise;
	DateTime runEncoderTimeStartCapture;
	bool runEncoderCaptureSimulated;
	string runEncoderFirmwareVersion;

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


	private void initRunEncoder ()
	{
		followSignals = false;
		if(preferences.runEncoderCaptureDisplaySimple)
			radio_race_analyzer_capture_view_simple.Active = true;
		else
			radio_race_analyzer_capture_view_complete.Active = true;
		followSignals = true;

		manageRunEncoderCaptureViews();

		createRunEncoderExerciseCombo();
		createRunEncoderAnalyzeCombos();
		setRunEncoderAnalyzeWidgets();

		spinbutton_run_encoder_export_image_width.Value = preferences.exportGraphWidth;
		spinbutton_run_encoder_export_image_height.Value = preferences.exportGraphHeight;
	}

	private void manageRunEncoderCaptureViews()
	{
		if(radio_race_analyzer_capture_view_simple.Active)
		{
			alignment_drawingarea_race_analyzer_capture_velocimeter_topleft.Visible = true;
			alignment_hbox_race_analyzer_capture_bottom.Visible = false;
			cairoRadial = null;
			drawingarea_race_analyzer_capture_velocimeter_topleft.QueueDraw(); //will fire ExposeEvent
		} else {
			alignment_drawingarea_race_analyzer_capture_velocimeter_topleft.Visible = false;
			alignment_hbox_race_analyzer_capture_bottom.Visible = true;
			cairoRadial = null;
			drawingarea_race_analyzer_capture_velocimeter_bottom.QueueDraw(); //will fire ExposeEvent
		}

		/*
		   update the preferences variable
		   note as can be changed while capturing, it will be saved to SQL on exit
		   to not have problems with SQL while capturing
		   */
		preferences.runEncoderCaptureDisplaySimple = radio_race_analyzer_capture_view_simple.Active;
	}

	private void on_radio_race_analyzer_capture_view_clicked (object o, EventArgs args)
	{
		if(! followSignals)
			return;

		manageRunEncoderCaptureViews();
	}

	//no GTK here
	private bool runEncoderConnect()
	{
		LogB.Information(" RE connect 0 ");
		if(chronopicRegister.ConnectedOfType(ChronopicRegisterPort.Types.ARDUINO_RUN_ENCODER) == null)
		{
			runEncoderPulseMessage = runEncoderNotConnectedString;
			return false;
		}

		LogB.Information(" RE connect 1 ");
		runEncoderPortName = chronopicRegister.ConnectedOfType(ChronopicRegisterPort.Types.ARDUINO_RUN_ENCODER).Port;
		LogB.Information(" RE connect 2 ");
		if(runEncoderPortName == null || runEncoderPortName == "")
		{
			runEncoderPulseMessage = "Please, select port!";
			return false;
		}
		LogB.Information(" RE connect 3 ");
		runEncoderPulseMessage = "Connecting ...";

		portRE = new SerialPort(runEncoderPortName, 115200); //runEncoder
		LogB.Information(" RE connect 4: opening port...");

		try {
			portRE.Open();
		}
		catch (System.IO.IOException)
		{
			runEncoderPulseMessage = runEncoderNotConnectedString;
			return false;
		}

		LogB.Information(" RE connect 5: let arduino start");

		Thread.Sleep(3000); //sleep to let arduino start reading serial event

		LogB.Information(" RE connect 6: get version");

		runEncoderFirmwareVersion = runEncoderCheckVersionDo();
		LogB.Information("Version found: [" + runEncoderFirmwareVersion + "]");

		portREOpened = true;
		runEncoderPulseMessage = "Connected!";
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
		/*
		 * return(str.Remove(0,14));
		 * use a regex because with above line we can find problems like this:
		 * init string: ^@;Race_analyzer-0.3
		 */
		Match match = Regex.Match(str, @"Race_Analyzer-(\d+\.\d+)");
		if(match.Groups.Count == 1)
			return str = match.Value;
		else
			return "0.3"; //if there is a problem default to 0.3. 0.2 was the first that will be distributed and will be on binary. 0.3 has the byte of encoderOrRCA
	}


	//Attention: no GTK here!!
	private bool runEncoderSendCommand(string command, string displayMessage, string errorMessage)
	{
		if(displayMessage != "")
			runEncoderPulseMessage = displayMessage;

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

	private string runEncoderReceiveFeedback(string expected)
	{
		string str = "";
		do {
			Thread.Sleep(100); //sleep to let arduino start reading
			try {
				str = portRE.ReadLine();
			} catch {
				runEncoderProcessError = true;
				return "";
			}
			LogB.Information("runEncoder feedback string: " + str);
		}
		while(! str.Contains(expected));
		return str;
	}

	private void on_runs_encoder_capture_clicked ()
	{
		if(UtilGtk.ComboGetActive(combo_run_encoder_exercise) == "")
		{
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Need to create/select an exercise."));
			return;
		}
		if(currentPersonSession.Weight == 0)
		{
			new DialogMessage(Constants.MessageTypes.WARNING,
					Catalog.GetString("Error, weight of the person cannot be 0"));
			return;
		}

		/*
		if(currentPersonSession.Height == 0)
		{
			new DialogMessage(Constants.MessageTypes.WARNING,
					Catalog.GetString("Error, height of the person cannot be 0"));
			return;
		}
		allow to do the capture, but after show message: no height
		*/

		runEncoderPulseMessage = "";
		runEncoderButtonsSensitive(false);
		sensitiveLastTestButtons(false);

		//reset capture tab graphs
		if(cairoRadial != null)
			cairoRadial.ResetSpeedMax();

		//blank Cairo scatterplot graphs
		cairoGraphRaceAnalyzer_dt = null;
		cairoGraphRaceAnalyzer_st = null;
		cairoGraphRaceAnalyzer_at = null;
		cairoGraphRaceAnalyzerPoints_dt_l = new List<PointF>();
		cairoGraphRaceAnalyzerPoints_st_l = new List<PointF>();
		cairoGraphRaceAnalyzerPoints_at_l = new List<PointF>();

		updateRaceAnalyzerCapturePositionTime(true);
		updateRaceAnalyzerCaptureSpeedTime(true);
		updateRaceAnalyzerCaptureAccelTime(true);
		//if graphs are not updated with the line, use this that will fire ExposeEvent:
		//drawingarea_race_analyzer_capture_position_time.QueueDraw(); and the others


		if(chronopicRegister.NumConnectedOfType(ChronopicRegisterPort.Types.ARDUINO_RUN_ENCODER) == 0)
		{
			event_execute_label_message.Text = runEncoderNotConnectedString;
			runEncoderButtonsSensitive(true);
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
		textview_contacts_signal_comment.Buffer.Text = "";
		assignCurrentRunEncoderExercise();
		raceEncoderReadWidgets();
		button_contacts_exercise_close_and_recalculate.Sensitive = false;

		//remove stuff on analyze tab
		image_run_encoder_graph.Visible = false;
		treeview_raceAnalyzer = UtilGtk.RemoveColumns(treeview_raceAnalyzer);

		bool connected = runEncoderCapturePre4_GTK();
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

		//draw the capture graphs empty:
		//a) radial
		runEncoderShouldShowCaptureGraphsWithData = false;

		if(radio_race_analyzer_capture_view_simple.Active)
			drawingarea_race_analyzer_capture_velocimeter_topleft.QueueDraw(); //will fire ExposeEvent
		else
			drawingarea_race_analyzer_capture_velocimeter_bottom.QueueDraw(); //will fire ExposeEvent

		//b) scatterplots
		cairoGraphRaceAnalyzerPoints_dt_l = new List<PointF>();
		cairoGraphRaceAnalyzerPoints_st_l = new List<PointF>();
		cairoGraphRaceAnalyzerPoints_at_l = new List<PointF>();
		drawingarea_race_analyzer_capture_position_time.QueueDraw(); //will fire ExposeEvent
		drawingarea_race_analyzer_capture_speed_time.QueueDraw(); //will fire ExposeEvent
		drawingarea_race_analyzer_capture_accel_time.QueueDraw(); //will fire ExposeEvent

		button_contacts_exercise_close_and_recalculate.Sensitive = false;
		textview_contacts_signal_comment.Buffer.Text = "";

		//image_run_encoder_graph.Sensitive = false; //this is not useful at all
		image_run_encoder_graph.Visible = false;

		treeview_raceAnalyzer = UtilGtk.RemoveColumns(treeview_raceAnalyzer);
		button_raceAnalyzer_table_save.Sensitive = false;
		clearRaceAnalyzerTriggers();

		button_run_encoder_analyze_options_close_and_analyze.Sensitive = false;
		button_run_encoder_analyze_analyze.Sensitive = false;
		button_delete_last_test.Sensitive = false;
		button_run_encoder_image_save.Sensitive = false;

		if(radio_run_encoder_analyze_individual_current_session.Active)
		{
			if(currentPerson != null)
				label_run_encoder_export_data.Text = currentPerson.Name;
			else
				label_run_encoder_export_data.Text = "";
		}

		label_run_encoder_export_result.Text = "";
		button_run_encoder_export_result_open.Visible = false;
	}

	private void raceEncoderReadWidgets()
	{
		race_analyzer_distance = Convert.ToInt32(race_analyzer_spinbutton_distance.Value);
		race_analyzer_temperature = Convert.ToInt32(race_analyzer_spinbutton_temperature.Value);

		if(UtilGtk.ComboGetActive(combo_race_analyzer_device) == RunEncoder.DevicesStringMANUAL)
			race_analyzer_device = RunEncoder.Devices.MANUAL;
		else
			race_analyzer_device = RunEncoder.Devices.RESISTED;
	}

	private RunEncoder.Devices raceEncoderGetDevice()
	{
		if(UtilGtk.ComboGetActive(combo_race_analyzer_device) == RunEncoder.DevicesStringMANUAL)
			return RunEncoder.Devices.MANUAL;
		else
			return RunEncoder.Devices.RESISTED;
	}
	private void raceEncoderSetDevice(RunEncoder.Devices d)
	{
		if(d == RunEncoder.Devices.RESISTED)
			combo_race_analyzer_device.Active = UtilGtk.ComboMakeActive(combo_race_analyzer_device, RunEncoder.DevicesStringRESISTED);
		else
			combo_race_analyzer_device.Active = UtilGtk.ComboMakeActive(combo_race_analyzer_device, RunEncoder.DevicesStringMANUAL);
	}

	private void on_combo_race_analyzer_device_changed (object o, EventArgs args)
	{
		forceSensorImageTestChange();
	}

	private void forceSensorImageTestChange()
	{
		Pixbuf pixbuf; //main image
		if(UtilGtk.ComboGetActive(combo_race_analyzer_device) == RunEncoder.DevicesStringMANUAL)
			pixbuf = new Pixbuf (null, Util.GetImagePath(true) + "run-encoder-manual.png");
		else
			pixbuf = new Pixbuf (null, Util.GetImagePath(true) + "run-encoder-resisted.png");

		image_test.Pixbuf = pixbuf;
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
	private bool runEncoderCapturePre4_GTK()
	{
		/*
		if(! portREOpened)
			if(! runEncoderConnect()) //GTK
				return false;
				*/

		if(File.Exists(UtilEncoder.GetSprintEncoderImage()))
			Util.FileDelete(UtilEncoder.GetSprintEncoderImage());

		event_execute_label_message.Text = "Please, wait ...";
		captureEndedMessage = "";
		capturingRunEncoder = arduinoCaptureStatus.STARTING;

		button_execute_test.Sensitive = false;
		event_execute_button_cancel.Sensitive = true;

		//forceCaptureStartMark = false;

		runEncoderProcessFinish = false;
		runEncoderProcessCancel = false;
		runEncoderProcessError = false;

		//To know if USB has been disconnected
		usbDisconnectedCount = 0;
		usbDisconnectedLastTime = 0;
		contactsShowCaptureDoingButtons(true);

		//runEncoderCaptureSimulated = menuitem_check_race_encoder_capture_simulate.Active; //TODO: show this in some way on 2.0
		runEncoderCaptureSimulated = false; //note that on simulated we need to click finish not so late

		/*
		//initialize
		forceSensorValues = new ForceSensorValues();
		*/

		event_execute_ButtonFinish.Clicked -= new EventHandler(on_finish_clicked);
		event_execute_ButtonFinish.Clicked += new EventHandler(on_finish_clicked);
		
		event_execute_ButtonCancel.Clicked -= new EventHandler(on_cancel_clicked);
		event_execute_ButtonCancel.Clicked += new EventHandler(on_cancel_clicked);

		cairoGraphRaceAnalyzerPoints_dt_l = new List<PointF>();
		cairoGraphRaceAnalyzerPoints_st_l = new List<PointF>();
		cairoGraphRaceAnalyzerPoints_at_l = new List<PointF>();

		//RunEncoderCaptureGetSpeedAndDisplacement reCGSD = new RunEncoderCaptureGetSpeedAndDisplacement();
		reCGSD = new RunEncoderCaptureGetSpeedAndDisplacement();
		runEncoderShouldShowCaptureGraphsWithData = true;

		runEncoderCaptureThread = new Thread(new ThreadStart(runEncoderCaptureDo));
		GLib.Idle.Add (new GLib.IdleHandler (pulseGTKRunEncoderCapture));

		if(preferences.debugMode)
			LogB.Information("Debug mode active. Logs active while race analyzer capture");
		else
			LogB.Information("Debug mode inactive. Logs INactive while race analyzer capture");

		//mute logs if ! debug mode
		LogB.Mute = ! preferences.debugMode;

		LogB.ThreadStart();
		runEncoderCaptureThread.Start();
		return true;
	}

	string capturingMessage = "Capturing ...";
	static RunEncoderCaptureGetSpeedAndDisplacement reCGSD;

	//non GTK on this method
	private void runEncoderCaptureDo()
	{
		LogB.Information("runEncoderCaptureDo 0");

		if(! portREOpened)
			if(! runEncoderConnect()) //GTK
				return;

		lastChangedTime = 0;

                double versionDouble = Convert.ToDouble(Util.ChangeDecimalSeparator(runEncoderFirmwareVersion));
		if(versionDouble >= Convert.ToDouble(Util.ChangeDecimalSeparator("0.3")))
		{
			if(! runEncoderSendCommand(string.Format("set_pps:{0};", preferences.runEncoderPPS), "Sending pps", "Catched at set_pps"))
				return;

			//read confirmation data
			if(runEncoderReceiveFeedback("pps set to") == "")
				return;
		}

		string command = "start_capture:";
		if(runEncoderCaptureSimulated)
			command = "start_simulation:";

		if(! runEncoderSendCommand(command, "Initializing", "Catched run encoder capturing"))
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

		//it comes "Starting capture;PPS:100"
		string [] strStarting = str.Split(new char[] {';'});
		int pps = 999;
		if(strStarting.Length == 2 && strStarting[1].StartsWith("PPS:"))
		{
			string [] strPPS = strStarting[1].Split(new char[] {':'});
			if(strPPS.Length == 2 && Util.IsNumber(strPPS[1], false))
				pps = Convert.ToInt32(strPPS[1]);
		}

		runEncoderPulseMessage = capturingMessage;

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

		triggerListRunEncoder = new TriggerList();
		Stopwatch sw = new Stopwatch();

		//to measure accel (having 3 samples)
		//TODO: store on reCGSD
		double speedPre2 = -1;
		double timePre2 = -1;
		double speedPre = -1;
		double timePre = -1;

		int rowsCount = 0;
		while(! runEncoderProcessFinish && ! runEncoderProcessCancel && ! runEncoderProcessError)
		{
			/*
			 * The difference between forceSensor and runEncoder is:
			 * runEncoder is not always returning data
			 * if user press "finish" button, and they don't move the encoder,
			 * this will never end:
			 * //str = portRE.ReadLine();
			 * so use the methods:
			 * 	if (portRE.BytesToRead == 0) [binary]
			 * or
			 * 	readFromRunEncoderIfDataArrived(); [not binary]
			 * that allow to continue in the loop when there is no data
			 * and then the while above will end with the runEncoderProcessFinish condition
			 */

			if (portRE.BytesToRead == 0)
			{
				//runEncoder sends changes. If there is no movement in certain time, just show a 0 on screen and capture graph
				if(sw.ElapsedMilliseconds > 500)
				{
					reCGSD.RunEncoderCaptureSpeed = 0;
					sw.Stop();
				}

				continue;
			}

			//time (4 bytes: long at Arduino, uint at c-sharp), force (2 bytes: uint)
			List<int> binaryReaded = readBinaryRunEncoderValues();
			reCGSD.PassCapturedRow (binaryReaded);
			if(reCGSD.Calcule() && reCGSD.EncoderDisplacement != 0) //this 0s are triggers without displacement
			{
				//distance/time
				cairoGraphRaceAnalyzerPoints_dt_l.Add(new PointF(
							UtilAll.DivideSafe(reCGSD.Time, 1000000),
							reCGSD.RunEncoderCaptureDistance));
				//speed/time
				cairoGraphRaceAnalyzerPoints_st_l.Add(new PointF(
							UtilAll.DivideSafe(reCGSD.Time, 1000000),
							reCGSD.RunEncoderCaptureSpeed));

				speedPre2 = speedPre;
				timePre2 = timePre;
				speedPre = reCGSD.RunEncoderCaptureSpeed;
				timePre = UtilAll.DivideSafe(reCGSD.Time, 1000000);

				if(timePre2 > 0)
				{
					LogB.Information(string.Format("accel at capture is: {0} m/s",
								UtilAll.DivideSafe(reCGSD.RunEncoderCaptureSpeed - speedPre2,
									UtilAll.DivideSafe(reCGSD.Time, 1000000) - timePre2)));

					//accel/time
					cairoGraphRaceAnalyzerPoints_at_l.Add(new PointF(
								UtilAll.DivideSafe(reCGSD.Time, 1000000),
								UtilAll.DivideSafe(reCGSD.RunEncoderCaptureSpeed - speedPre2,
									UtilAll.DivideSafe(reCGSD.Time, 1000000) - timePre2)));
				}

				sw.Restart();
			}

			LogB.Information(string.Format("{0};{1};{2};{3};{4}", pps, reCGSD.EncoderDisplacement, reCGSD.Time, reCGSD.Force, reCGSD.EncoderOrRCA));

			if(reCGSD.EncoderDisplacement == 0)
				LogB.Information("It is a trigger and without displacement, do not write it to file");
			else
				writer.WriteLine(string.Format("{0};{1};{2}", reCGSD.EncoderDisplacement, reCGSD.Time, reCGSD.Force));

			if(reCGSD.EncoderOrRCA == 0)
				rowsCount ++; //note this is not used right now, and maybe it will need to be for all cases, not just for encoderOrRCA
			else {
				Trigger trigger;
				if(reCGSD.EncoderOrRCA == 1)
					trigger = new Trigger(Trigger.Modes.RACEANALYZER, reCGSD.Time, false);
				else //(reCGSD.EncoderOrRCA == 2)
					trigger = new Trigger(Trigger.Modes.RACEANALYZER, reCGSD.Time, true);

				if(! triggerListRunEncoder.NewSameTypeThanBefore(trigger) &&
						! triggerListRunEncoder.IsSpurious(trigger, TriggerList.Type3.BOTH, 50000))
					triggerListRunEncoder.Add(trigger);
			}
		}
		sw.Stop();

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

				/*
				 * we will discard everything at the moment finish is pressed
				 * to read correctly as txt the "Capture ended" string
				 * note this can make the rowsCount differ
				 */
			} catch {
				LogB.Information("Caught waiting end_capture feedback");
			}
			LogB.Information("waiting \"Capture ended\" string: " + str);
		}
		while(! str.Contains("Capture ended")); //TODO: read after ':' the number of "rows" sent
		LogB.Information("Success: received end_capture");

		string [] strEnded = str.Split(new char[] {':'});
		if(strEnded.Length == 2 && Util.IsNumber(strEnded[1], false))
		{
			LogB.Information(string.Format("Read {0} rows, sent {1} rows, match: {2}",
						rowsCount, strEnded[1], rowsCount == Convert.ToInt32(strEnded[1]) ));
		}

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
			lastRunEncoderFullPath = fileName;

			//raceEncoderCaptureRGraphDo(); moved to pulseGTKRunEncoderCapture () where currentRunEncoder is created

			capturingRunEncoder = arduinoCaptureStatus.COPIED_TO_TMP;
		}
	}

	//time (4 bytes: long at Arduino, uint at c-sharp), force (2 bytes: uint), encoder/RCA (1 byte: uint)
	private List<int> readBinaryRunEncoderValues()
        {
		LogB.Information("start reading binary data");
		//LogB.Debug("readed start mark Ok");
                List<int> dataRow = new List<int>();

		// 1) encoderDisplacement (2 bytes)
                int b0 = portRE.ReadByte(); //encoderDisplacement least significative
                int b1 = portRE.ReadByte(); //encoderDisplacement most significative
		LogB.Information(" 1 readed b0,b1");
		/*
		LogB.Information("bbbencDispl0:" + b0.ToString());
		LogB.Information("bbbencDispl1:" + b1.ToString());
		*/
		int readedNum = Convert.ToInt32(256 * b1 + b0);

		//care for negative values
		if(readedNum > 32768)
			readedNum = -1 * (65536 - readedNum);

		dataRow.Add(readedNum);

		// 2) read time, four bytes
                b0 = portRE.ReadByte(); //least significative
                b1 = portRE.ReadByte();
                int b2 = portRE.ReadByte();
                int b3 = portRE.ReadByte(); //most significative
		LogB.Information(" 2 readed b0-b3");

		/*
		LogB.Information("bbbtime0:" + b0.ToString());
		LogB.Information("bbbtime1:" + b1.ToString());
		LogB.Information("bbbtime2:" + b2.ToString());
		LogB.Information("bbbtime3:" + b3.ToString());

		LogB.Information("time: " +
                                (Math.Pow(256,3) * b3 +
                                Math.Pow(256,2) * b2 +
                                Math.Pow(256,1) * b1 +
                                Math.Pow(256,0) * b0).ToString());
		*/

                dataRow.Add(Convert.ToInt32(
                                Math.Pow(256,3) * b3 +
                                Math.Pow(256,2) * b2 +
                                Math.Pow(256,1) * b1 +
                                Math.Pow(256,0) * b0));

		// 3) read force, two bytes
		b0 = portRE.ReadByte(); //least significative
		b1 = portRE.ReadByte(); //most significative
		readedNum = Convert.ToInt32(256 * b1 + b0);
		LogB.Information(" 3 readed b0,b1");

		/*
		LogB.Information("bbbforce0:" + b0.ToString());
		LogB.Information("bbbforce1:" + b1.ToString());
		LogB.Information("force (not changed sign): " +
                                (Math.Pow(256,1) * b1 +
                                Math.Pow(256,0) * b0).ToString());
		*/

		/*
		 * cannot be negative, if it is (on resisted) will be changed to 0 on Arduino code
		//care for negative values
		if(readedNum > 32768)
			readedNum = -1 * (65536 - readedNum);
			*/

		dataRow.Add(readedNum);

		/*
		 * 4) byte for encoder or RCA
		 * 0 encoder data
		 * 1 RCA down (button is released)
		 * 2 RCA up (button is pressed)
		 */
		b0 = portRE.ReadByte();
		//LogB.Information("b0: " + b0.ToString());
		dataRow.Add(Convert.ToInt32(b0));

		LogB.Information("readed all binary data");

                return dataRow;
        }

	private void run_encoder_load ()
	{
		List<RunEncoder> data = SqliteRunEncoder.Select(false, -1, currentPerson.UniqueID, currentSession.UniqueID);

		ArrayList dataPrint = new ArrayList();
		int count = 1;
		foreach(RunEncoder re in data)
			dataPrint.Add(re.ToStringArray(false, count++));

		string [] columnsString = {
			Catalog.GetString("ID"),
			Catalog.GetString("Set"),
			Catalog.GetString("Exercise"),
			//Catalog.GetString("Device"),
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

		genericWin.SetTreeview(columnsString, false, dataPrint, new ArrayList(), GenericWindow.EditActions.EDITPLAYDELETE, true);

		//find all persons in current session
		ArrayList personsPre = SqlitePersonSession.SelectCurrentSessionPersons(currentSession.UniqueID,
				false); //means: do not returnPersonAndPSlist

		string [] persons = new String[personsPre.Count];
		count = 0;
	        foreach	(Person p in personsPre)
			persons[count++] = p.UniqueID.ToString() + ":" + p.Name;
		genericWin.SetComboValues(persons, currentPerson.UniqueID + ":" + currentPerson.Name);
		//genericWin.SetComboLabel(Catalog.GetString("Change the owner of selected set") +
		//		" (" + Catalog.GetString("code") + ":" + Catalog.GetString("name") + ")");
		genericWin.SetComboLabel(Catalog.GetString("Change person"));
		genericWin.ShowEditRow(false);

		//select row corresponding to current signal
		genericWin.SelectRowWithID(0, currentRunEncoder.UniqueID); //colNum, id

		genericWin.VideoColumn = 6;
		genericWin.CommentColumn = 7;

		genericWin.ShowButtonCancel(true);
		genericWin.SetButtonAcceptLabel(Catalog.GetString("Load"));
		genericWin.SetButtonCancelLabel(Catalog.GetString("Close"));
		genericWin.SetButtonAcceptSensitive(false);
		genericWin.Button_accept.Clicked += new EventHandler(on_run_encoder_load_accepted);
		genericWin.Button_row_play.Clicked += new EventHandler(on_run_encoder_load_signal_row_play);
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
		radio_run_encoder_analyze_individual_current_set.Active = true;

		RunEncoder re = (RunEncoder) SqliteRunEncoder.Select(false, uniqueID, currentPerson.UniqueID, currentSession.UniqueID)[0];
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

		List<string> contents = Util.ReadFileAsStringList(re.FullURL);
		LogB.Information("FullURL: " + re.FullURL);
		if(contents.Count < 3)
		{
			new DialogMessage(Constants.MessageTypes.WARNING, Constants.FileEmptyStr());
			return;
		}

		currentRunEncoder = re;
		lastRunEncoderFile = Util.RemoveExtension(re.Filename);
		lastRunEncoderFullPath = re.FullURL;

		combo_run_encoder_exercise.Active = UtilGtk.ComboMakeActive(combo_run_encoder_exercise, Catalog.GetString(re.ExerciseName));
		assignCurrentRunEncoderExercise();

		raceEncoderSetDevice(re.Device);
		raceEncoderSetDistanceAndTemp(re.Distance, re.Temperature);
///		textview_race_analyzer_comment.Buffer.Text = re.Comments;
		textview_contacts_signal_comment.Buffer.Text = re.Comments;

		raceEncoderReadWidgets(); //needed to be able to do R graph

		//triggers
		triggerListRunEncoder = new TriggerList(
				SqliteTrigger.Select(
					false, Trigger.Modes.RACEANALYZER,
					Convert.ToInt32(currentRunEncoder.UniqueID))
				);
		showRaceAnalyzerTriggers ();

		// ---- capture tab graphs start ---->

		int count = 0;
		reCGSD = new RunEncoderCaptureGetSpeedAndDisplacement();
		runEncoderShouldShowCaptureGraphsWithData = true;

		cairoGraphRaceAnalyzer_dt = null;
		cairoGraphRaceAnalyzer_st = null;
		cairoGraphRaceAnalyzer_at = null;
		cairoGraphRaceAnalyzerPoints_dt_l = new List<PointF>();
		cairoGraphRaceAnalyzerPoints_st_l = new List<PointF>();
		cairoGraphRaceAnalyzerPoints_at_l = new List<PointF>();

		//to measure accel (having 3 samples)
		//TODO: store on reCGSD
		double speedPre2 = -1;
		double timePre2 = -1;
		double speedPre = -1;
		double timePre = -1;

		foreach(string row in contents)
		{
			LogB.Information("row: " + row);
			if(count < 3)
			{
				count ++;
				continue;
			}

			if(reCGSD.PassLoadedRow (row))
				reCGSD.Calcule();

			//distance/time
			cairoGraphRaceAnalyzerPoints_dt_l.Add(new PointF(
						UtilAll.DivideSafe(reCGSD.Time, 1000000),
						reCGSD.RunEncoderCaptureDistance));
			//speed/time
			cairoGraphRaceAnalyzerPoints_st_l.Add(new PointF(
						UtilAll.DivideSafe(reCGSD.Time, 1000000),
						reCGSD.RunEncoderCaptureSpeed));

			speedPre2 = speedPre;
			timePre2 = timePre;
			speedPre = reCGSD.RunEncoderCaptureSpeed;
			timePre = UtilAll.DivideSafe(reCGSD.Time, 1000000);

			if(timePre2 > 0)
			{
				LogB.Information(string.Format("accel at load is: {0} m/s",
							UtilAll.DivideSafe(reCGSD.RunEncoderCaptureSpeed - speedPre2,
								UtilAll.DivideSafe(reCGSD.Time, 1000000) - timePre2)));

				//accel/time
				cairoGraphRaceAnalyzerPoints_at_l.Add(new PointF(
							UtilAll.DivideSafe(reCGSD.Time, 1000000),
							UtilAll.DivideSafe(reCGSD.RunEncoderCaptureSpeed - speedPre2,
								UtilAll.DivideSafe(reCGSD.Time, 1000000) - timePre2)));
			}

		}
		if(reCGSD.RunEncoderCaptureSpeedMax > 0)
		{
			if(cairoRadial == null)
			{
				if(radio_race_analyzer_capture_view_simple.Active)
					cairoRadial = new CairoRadial(drawingarea_race_analyzer_capture_velocimeter_topleft, preferences.fontType.ToString());
				else
					cairoRadial = new CairoRadial(drawingarea_race_analyzer_capture_velocimeter_bottom, preferences.fontType.ToString());
			}

			cairoRadial.GraphSpeedMaxAndDistance(reCGSD.RunEncoderCaptureSpeedMax, reCGSD.RunEncoderCaptureDistance);

			updateRaceAnalyzerCapturePositionTime(true);
			updateRaceAnalyzerCaptureSpeedTime(true);
			updateRaceAnalyzerCaptureAccelTime(true);
		}

		// <---- capture tab graphs end ----

		//on load do the R graph, but not on capture, to show on capture the label related to lack of person height
		//raceEncoderCopyToTempAndDoRGraph();
		//no do not do it automatically, just make user click on analyze button
		//also showing that graph while analyze tab has not shown first time is buggy

		button_contacts_exercise_close_and_recalculate.Sensitive = true;

		button_video_play_this_test_contacts.Sensitive = (re.VideoURL != "");
		sensitiveLastTestButtons(true);

		event_execute_label_message.Text = Catalog.GetString("Loaded:") + " " + Util.GetLastPartOfPath(re.Filename);
		image_run_encoder_graph.Visible = false;
		button_run_encoder_analyze_analyze.Sensitive = true;
		button_run_encoder_analyze_options_close_and_analyze.Sensitive = true;
		button_run_encoder_image_save.Sensitive = true;
	}

	protected void on_run_encoder_load_signal_row_play (object o, EventArgs args)
	{
		LogB.Information("row play at load signal");
		LogB.Information(genericWin.TreeviewSelectedUniqueID.ToString());
		playVideo(Util.GetVideoFileName(currentSession.UniqueID,
				Constants.TestTypes.RACEANALYZER, genericWin.TreeviewSelectedUniqueID));
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


	private void run_encoder_recalculate ()
	{
		if(! Util.FileExists(lastRunEncoderFullPath))
		{
			new DialogMessage(Constants.MessageTypes.WARNING, Constants.FileNotFoundStr());
			return;
		}

		assignCurrentRunEncoderExercise();

		raceEncoderReadWidgets();
		if(lastRunEncoderFullPath != null && lastRunEncoderFullPath != "")
			raceEncoderCopyToTempAndDoRGraph();

		event_execute_label_message.Text = "Recalculated.";
		radio_mode_contacts_analyze.Active = true;
		button_contacts_exercise_close_and_recalculate.Sensitive = true;

		//update SQL with exercise, device, distance, temperature, comments
		currentRunEncoder.ExerciseID = currentRunEncoderExercise.UniqueID;
		currentRunEncoder.ExerciseName = currentRunEncoderExercise.Name; //just in case
		currentRunEncoder.Device = raceEncoderGetDevice();
		currentRunEncoder.Distance = Convert.ToInt32(race_analyzer_spinbutton_distance.Value);
		currentRunEncoder.Temperature = Convert.ToInt32(race_analyzer_spinbutton_temperature.Value);
		//currentRunEncoder.Comments = UtilGtk.TextViewGetCommentValidSQL(textview_race_analyzer_comment);
		currentRunEncoder.Comments = UtilGtk.TextViewGetCommentValidSQL(textview_contacts_signal_comment);

		currentRunEncoder.UpdateSQL(false);
	}

	private void raceEncoderCopyToTempAndDoRGraph()
	{
		// 0) delete results file
		Util.FileDelete(RunEncoder.GetCSVResultsURL());

		// 1) copy file
		File.Copy(lastRunEncoderFullPath, RunEncoder.GetCSVFileName(), true); //can be overwritten

		// 2) create and open graph
		raceEncoderCaptureRGraphDo();

		Thread.Sleep (250); //Wait a bit to ensure is copied

		runEncoderAnalyzeOpenImage();
		notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.RACEENCODER);
		//radio_mode_contacts_analyze.Active = true;
		button_run_encoder_analyze_analyze.Sensitive = true;

		// 3) display table
		treeview_raceAnalyzer = UtilGtk.RemoveColumns(treeview_raceAnalyzer);

		string contents = Util.ReadFile(RunEncoder.GetCSVResultsURL(), false);

		//maybe captured data was too low or two different than an sprint.
		//then we have image but maybe we have no sprintResults.csv

		if(contents == null || contents == "")
			return;
		else {
			createTreeViewRaceEncoder(contents);
			button_raceAnalyzer_table_save.Sensitive = true;
		}
	}

	private void raceEncoderCaptureRGraphDo()
	{
		if(File.Exists(UtilEncoder.GetSprintEncoderImage()))
			Util.FileDelete(UtilEncoder.GetSprintEncoderImage());

		int imageWidth = UtilGtk.WidgetWidth(viewport_run_encoder_graph);
		int imageHeight = UtilGtk.WidgetHeight(viewport_run_encoder_graph);

		string title = Util.ChangeSpaceAndMinusForUnderscore(currentPerson.Name) + "-" +
			Util.ChangeSpaceAndMinusForUnderscore(UtilGtk.ComboGetActive(combo_run_encoder_exercise));

		//create graph
		RunEncoderGraph reg = new RunEncoderGraph(
				race_analyzer_distance,
				currentPersonSession.Weight,  	//TODO: can be more if extra weight
				currentPersonSession.Height,
				race_analyzer_temperature,
				race_analyzer_device,
				currentRunEncoderExercise, //TODO: do not let capture if there's no exercise
				title,
				currentRunEncoder.DateTimePublic,
				preferences.runEncoderMinAccel,
				Preferences.RunEncoderShouldPlotVariable(Preferences.RunEncoderPlotVariables.RAWACCEL),
				Preferences.RunEncoderShouldPlotVariable(Preferences.RunEncoderPlotVariables.FITTEDACCEL),
				Preferences.RunEncoderShouldPlotVariable(Preferences.RunEncoderPlotVariables.RAWFORCE),
				Preferences.RunEncoderShouldPlotVariable(Preferences.RunEncoderPlotVariables.FITTEDFORCE),
				Preferences.RunEncoderShouldPlotVariable(Preferences.RunEncoderPlotVariables.RAWPOWER),
				Preferences.RunEncoderShouldPlotVariable(Preferences.RunEncoderPlotVariables.FITTEDPOWER),
				triggerListRunEncoder);

		reg.CallR(imageWidth, imageHeight, true);

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

	private void createTreeViewRaceEncoder (string contents)
	{
		string [] columnsString = getTreeviewRaceAnalyzerHeaders();
		int count = 0;
		foreach(string column in columnsString)
			treeview_raceAnalyzer.AppendColumn (column, new CellRendererText(), "text", count++);

		TreeStore store = new TreeStore(
				typeof (string), typeof (string), typeof (string),
				typeof (string), typeof (string), typeof (string),
				typeof (string), typeof (string), typeof (string),
				typeof (string), typeof (string), typeof (string),
				typeof (string), typeof (string), typeof (string),
				typeof (string), typeof (string), typeof (string),
				typeof (string), typeof (string), typeof (string),
				typeof (string)
				);

		RunEncoderCSV recsv = readRunEncoderCSVContents (contents);
		store.AppendValues (recsv.ToTreeView());

		treeview_raceAnalyzer.Model = store;
		treeview_raceAnalyzer.Selection.Mode = SelectionMode.None;
                treeview_raceAnalyzer.HeadersVisible=true;
	}

	private string [] getTreeviewRaceAnalyzerHeaders ()
        {
                string [] headers = {
			"Mass\n\n(Kg)", "Height\n\n(m)", "Temperature\n\n(ºC)",
			"V (wind)\n\n(m/s)", "Ka\n\n", "K\nfitted\n(s^-1)",
			"Vmax\nfitted\n(m/s)", "Amax\nfitted\n(m/s^2)", "Fmax\nfitted\n(N)",
			"Fmax\nrel fitted\n(N/Kg)", "Sfv\nfitted\n", "Sfv\nrel fitted\n",
			"Sfv\nlm\n", "Sfv\nrel lm\n", "Pmax\nfitted\n(W)",
			"Pmax\nrel fitted\n(W/Kg)", "Time to pmax\nfitted\n(s)", "F0\n\n(N)",
			"F0\nrel\n(N/Kg)", "V0\n\n(m/s)", "Pmax\nlm\n(W)",
			"Pmax\nrel lm\n(W/Kg)"
		};
		return headers;
	}

	//right now it only returns one line
	private RunEncoderCSV readRunEncoderCSVContents (string contents)
	{
		RunEncoderCSV recsv = new RunEncoderCSV();
		string line;
		using (StringReader reader = new StringReader (contents))
		{
			line = reader.ReadLine ();      //headers
			//LogB.Information(line);
			do {
				line = reader.ReadLine ();
				LogB.Information(line);
				if (line == null)
					break;

				string [] cells = line.Split(new char[] {';'});

				recsv = new RunEncoderCSV(
						Convert.ToDouble(cells[0]), Convert.ToDouble(cells[1]), Convert.ToInt32(cells[2]),
						Convert.ToDouble(cells[3]), Convert.ToDouble(cells[4]), Convert.ToDouble(cells[5]),
						Convert.ToDouble(cells[6]), Convert.ToDouble(cells[7]), Convert.ToDouble(cells[8]),
						Convert.ToDouble(cells[9]), Convert.ToDouble(cells[10]), Convert.ToDouble(cells[11]),
						Convert.ToDouble(cells[12]), Convert.ToDouble(cells[13]), Convert.ToDouble(cells[14]),
						Convert.ToDouble(cells[15]), Convert.ToDouble(cells[16]), Convert.ToDouble(cells[17]),
						Convert.ToDouble(cells[18]), Convert.ToDouble(cells[19]), Convert.ToDouble(cells[20]),
						Convert.ToDouble(cells[21])
						);
			} while(true);
		}

		return recsv;
	}

	private void on_button_raceAnalyzer_table_save_clicked (object o, EventArgs args)
	{
		checkFile(Constants.CheckFileOp.RUNENCODER_SAVE_TABLE);
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
		LogB.Information(" re A ");
		if(runEncoderCaptureThread == null)
		{
			Thread.Sleep (25);
			return true;
		}

		LogB.Information(" re B ");
		//LogB.Information(capturingRunEncoder.ToString())
		if(! runEncoderCaptureThread.IsAlive || runEncoderProcessFinish || runEncoderProcessCancel || runEncoderProcessError) //capture ends
		{
			LogB.Information(" re C ");
			button_video_play_this_test_contacts.Sensitive = false;
			if(runEncoderProcessFinish)
			{
				LogB.Information(" re C finish");
				if(capturingRunEncoder != arduinoCaptureStatus.COPIED_TO_TMP)
				{
					Thread.Sleep (25); //Wait file is copied
					return true;
				}
				else
				{
					event_execute_label_message.Text = "Saved." + captureEndedMessage;
					if(currentPersonSession.Height == 0)
						event_execute_label_message.Text += " " + "Person height is 0!";
					if(runEncoderCaptureSimulated)
						event_execute_label_message.Text += " SIMULATED TEST!";

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
					triggerListRunEncoder.SQLInsert(currentRunEncoder.UniqueID);
					showRaceAnalyzerTriggers ();

					//stop camera
					if(webcamEnd (Constants.TestTypes.RACEANALYZER, currentRunEncoder.UniqueID))
					{
						//add the videoURL to SQL
						currentRunEncoder.VideoURL = Util.GetVideoFileName(currentSession.UniqueID,
								Constants.TestTypes.RACEANALYZER,
								currentRunEncoder.UniqueID);
						currentRunEncoder.UpdateSQL(false);
						label_video_feedback.Text = "";
						button_video_play_this_test_contacts.Sensitive = true;
					}

					Thread.Sleep (250); //Wait a bit to ensure is copied
					sensitiveLastTestButtons(true);
					contactsShowCaptureDoingButtons(false);

					//do not analyze after capture, to be able to show the message: no person height
					/*
					raceEncoderCaptureRGraphDo();

					runEncoderAnalyzeOpenImage();
					notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.RACEENCODER);
					radio_mode_contacts_analyze.Active = true;
					*/

					button_contacts_exercise_close_and_recalculate.Sensitive = true;
					button_run_encoder_analyze_options_close_and_analyze.Sensitive = true;
					button_run_encoder_analyze_analyze.Sensitive = true;
					button_run_encoder_image_save.Sensitive = true;
					button_delete_last_test.Sensitive = true;

					/*
					fscPoints.InitRealWidthHeight();
					forceSensorDoSignalGraphPlot();
					forceSensorDoRFDGraph();

					//if drawingarea has still not shown, don't paint graph because GC screen is not defined
					if(force_sensor_ai_drawingareaShown)
						forceSensorDoGraphAI();
					 */

					updateRaceAnalyzerCapturePositionTime(true);
					updateRaceAnalyzerCaptureSpeedTime(true);
					updateRaceAnalyzerCaptureAccelTime(true);
				}
				LogB.Information(" re C finish 2");
			} else if(runEncoderProcessCancel || runEncoderProcessError)
			{
				LogB.Information(" re C cancel ");
				//stop the camera (and do not save)
				webcamEnd (Constants.TestTypes.RACEANALYZER, -1);
				sensitiveLastTestButtons(false);
				contactsShowCaptureDoingButtons(false);
				button_run_encoder_analyze_options_close_and_analyze.Sensitive = false;
				button_run_encoder_analyze_analyze.Sensitive = false;
				button_run_encoder_image_save.Sensitive = false;
				button_delete_last_test.Sensitive = false;

				if(runEncoderProcessCancel)
					event_execute_label_message.Text = "Cancelled.";
				else
					event_execute_label_message.Text = runEncoderNotConnectedString;
				LogB.Information(" re C cancel 2");
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
			LogB.Information(" re D ");

			LogB.Mute = preferences.muteLogs;
			if(! preferences.muteLogs)
				LogB.Information("muteLogs INactive. Logs active active again");

			if(reCGSD != null)
				cairoRadial.GraphSpeedMaxAndDistance(reCGSD.RunEncoderCaptureSpeedMax, reCGSD.RunEncoderCaptureDistance);

			/*
			LogB.Information("cairoGraphRaceAnalyzerPoints_dt_l: ");
			foreach(PointF p in cairoGraphRaceAnalyzerPoints_dt_l)
				LogB.Information(p.ToString());

			LogB.Information("cairoGraphRaceAnalyzerPoints_st_l: ");
			foreach(PointF p in cairoGraphRaceAnalyzerPoints_st_l)
				LogB.Information(p.ToString());
			*/

			LogB.ThreadEnded(); 

			runEncoderButtonsSensitive(true);
			radio_run_encoder_analyze_individual_current_set.Active = true;

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
		else //capture continues
		{
			event_execute_label_message.Text = runEncoderPulseMessage;

			if(cairoRadial != null && reCGSD != null)
				cairoRadial.GraphSpeedAndDistance(reCGSD.RunEncoderCaptureSpeed, reCGSD.RunEncoderCaptureDistance);

			//TODO: activate again when there's a real time update (not repaint all) method
			//false: it will not be redrawn if there are no new points
			updateRaceAnalyzerCapturePositionTime(false);
			updateRaceAnalyzerCaptureSpeedTime(false);
			updateRaceAnalyzerCaptureAccelTime(false);

			if(runEncoderPulseMessage == capturingMessage)
				event_execute_button_finish.Sensitive = true;
		}


		LogB.Information(" re E ");
		/*
		if(forceCaptureStartMark)
		{
			event_execute_label_message.Text = "Capturing ...";
			forceCaptureStartMark = false;
		}
		*/
		LogB.Information(" re F ");

		if(capturingRunEncoder == arduinoCaptureStatus.CAPTURING)
		{
			LogB.Information(" re G ");


			LogB.Information(" re H2 ");
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

			LogB.Information(" re I ");

			LogB.Information(" re Q ");
		}
		LogB.Information(" re R ");

		Thread.Sleep (50);
		//LogB.Information(" RunEncoder:"+ runEncoderCaptureThread.ThreadState.ToString());
		return true;
	}

	void runEncoderButtonsSensitive(bool sensitive)
	{
		//runEncoder related buttons
		vbox_run_encoder_capture_buttons.Sensitive = sensitive;
		vbox_run_encoder_capture_options.Sensitive = sensitive;
		frame_contacts_exercise.Sensitive = sensitive;
		button_execute_test.Sensitive = sensitive;
		hbox_contacts_camera.Sensitive = sensitive;

		//other gui buttons
		menus_and_mode_sensitive(sensitive);

		hbox_contacts_sup_capture_analyze_two_buttons.Sensitive = sensitive;
		frame_persons.Sensitive = sensitive;
		hbox_top_person.Sensitive = sensitive;
		hbox_chronopics_and_more.Sensitive = sensitive;
	}

	void runEncoderAnalyzeOpenImage()
	{
		string imagePath = UtilEncoder.GetSprintEncoderImage();
		image_run_encoder_graph = UtilGtk.OpenImageSafe(
				imagePath,
				image_run_encoder_graph);

		//image_run_encoder_graph.Sensitive = true; //not useful
		image_run_encoder_graph.Visible = true;
	}

	/*
	 * unused on 2.0
	 *
	private void on_menuitem_race_encoder_open_folder_activate (object o, EventArgs args)
	{
		if(currentSession == null || currentSession.UniqueID == -1)
		{
			try {
				System.Diagnostics.Process.Start(RunEncoderGraph.GetDataDir(-1)); //also use Util.OpenFolder
			} catch {
				new DialogMessage(Constants.MessageTypes.WARNING,
						Catalog.GetString("Error. Cannot open directory.") + "\n\n" + RunEncoderGraph.GetDataDir(-1));
			}
			return;
		}

		string dataDir = RunEncoderGraph.GetDataDir(currentSession.UniqueID);
		if(dataDir != "")
		{
			try {
				System.Diagnostics.Process.Start(dataDir); //also use Util.OpenFolder
			} catch {
				new DialogMessage(Constants.MessageTypes.WARNING,
						Catalog.GetString("Error. Cannot open directory.") + "\n\n" + dataDir);
			}
		} else
			new DialogMessage(Constants.MessageTypes.WARNING, Constants.DirectoryCannotOpenStr());
	}
	*/

	// -------------------------------- exercise stuff --------------------


	string [] runEncoderComboExercisesString; //id:name (no translations, use user language)

	private void createRunEncoderExerciseCombo ()
	{
		//run_encoder_exercise

		combo_run_encoder_exercise = ComboBox.NewText ();
		fillRunEncoderExerciseCombo("");

		combo_run_encoder_exercise.Changed += new EventHandler (on_combo_run_encoder_exercise_changed);
		hbox_combo_run_encoder_exercise.PackStart(combo_run_encoder_exercise, true, true, 0);
		hbox_combo_run_encoder_exercise.ShowAll();
	}

	//left-right buttons on run_encoder combo exercise selection
	private void on_button_combo_run_encoder_exercise_capture_left_clicked (object o, EventArgs args)
	{
		contacts_exercise_left_button (combo_run_encoder_exercise,
				button_combo_run_encoder_exercise_capture_left,
				button_combo_run_encoder_exercise_capture_right);
	}
	private void on_button_combo_run_encoder_exercise_capture_right_clicked (object o, EventArgs args)
	{
		contacts_exercise_right_button (combo_run_encoder_exercise,
				button_combo_run_encoder_exercise_capture_left,
				button_combo_run_encoder_exercise_capture_right);
	}

	private void on_combo_run_encoder_exercise_changed(object o, EventArgs args)
	{
		ComboBox combo = o as ComboBox;
		if (o == null)
			return;

		//two combobox are linked ---->
		if(comboSelectContactsTopNoFollow)
			return;

		comboSelectContactsTopNoFollow = true;
		if (o == combo_run_encoder_exercise)
			combo_select_contacts_top.Active = combo_run_encoder_exercise.Active;
		else if (o == combo_select_contacts_top)
			combo_run_encoder_exercise.Active = combo_select_contacts_top.Active;
		comboSelectContactsTopNoFollow = false;
		//<---- two combobox are linked

		setLabelContactsExerciseSelected(Constants.Modes.RUNSENCODER);

		//sensitivity of left/right buttons
		button_combo_run_encoder_exercise_capture_left.Sensitive = (combo_run_encoder_exercise.Active > 0);
		button_combo_run_encoder_exercise_capture_right.Sensitive = ! UtilGtk.ComboSelectedIsLast(combo_run_encoder_exercise);
		button_combo_select_contacts_top_left.Sensitive = (combo_run_encoder_exercise.Active > 0);
		button_combo_select_contacts_top_right.Sensitive = ! UtilGtk.ComboSelectedIsLast(combo_run_encoder_exercise);
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

		//update also combo_select_contacts_top (but check do not crash on start)
		if(combo_select_contacts_top != null)
		{
			comboSelectContactsTopNoFollow = true;
			UtilGtk.ComboUpdate(combo_select_contacts_top,
					UtilGtk.ComboGetValues (combo_run_encoder_exercise), "");
			combo_select_contacts_top.Active = combo_run_encoder_exercise.Active;
			comboSelectContactsTopNoFollow = false;
		}
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
		ArrayList a3 = new ArrayList();

		//0 is the widgget to show; 1 is the editable; 2 id default value
		a1.Add(Constants.GenericWindowShow.ENTRY); a1.Add(true); a1.Add(ex.Name);
		bigArray.Add(a1);

		a2.Add(Constants.GenericWindowShow.ENTRY2); a2.Add(true); a2.Add(ex.Description);
		bigArray.Add(a2);

		a3.Add(Constants.GenericWindowShow.SPININT2); a3.Add(true); a3.Add("");
		bigArray.Add(a3);

		genericWin = GenericWindow.Show(Catalog.GetString("Exercise"), false,	//don't show now
				Catalog.GetString("Race analyzer exercise:"), bigArray);
		genericWin.LabelEntry2 = Catalog.GetString("Description");

		genericWin.ShowButtonCancel(false);
		genericWin.HideOnAccept = false;

		genericWin.uniqueID = ex.UniqueID;
		genericWin.LabelSpinInt2 = "Segments size (m)";
		genericWin.SetSpin2Range(1, 30);
		genericWin.SetSpin2Value(ex.SegmentMeters);

		genericWin.Button_accept.Clicked -= new EventHandler(on_button_run_encoder_exercise_edit_accepted);
		genericWin.Button_accept.Clicked += new EventHandler(on_button_run_encoder_exercise_edit_accepted);
		genericWin.ShowNow();
	}

	private void on_button_run_encoder_exercise_add_clicked (object o, EventArgs args)
	{
		ArrayList bigArray = new ArrayList();

		ArrayList a1 = new ArrayList();
		ArrayList a2 = new ArrayList();
		ArrayList a3 = new ArrayList();

		//0 is the widgget to show; 1 is the editable; 2 id default value
		a1.Add(Constants.GenericWindowShow.ENTRY); a1.Add(true); a1.Add("");
		bigArray.Add(a1);

		a2.Add(Constants.GenericWindowShow.ENTRY2); a2.Add(true); a2.Add("");
		bigArray.Add(a2);

		a3.Add(Constants.GenericWindowShow.SPININT2); a3.Add(true); a3.Add("");
		bigArray.Add(a3);

		genericWin = GenericWindow.Show(Catalog.GetString("Exercise"), false,	//don't show now
				Catalog.GetString("Write the name of the exercise:"), bigArray);
		genericWin.LabelEntry2 = Catalog.GetString("Description");

		genericWin.SetButtonAcceptLabel(Catalog.GetString("Add"));
		genericWin.HideOnAccept = false;

		genericWin.LabelSpinInt2 = "Segments size (m)";
		genericWin.SetSpin2Range(1, 30);
		genericWin.SetSpin2Value(RunEncoderExercise.SegmentMetersDefault);

		genericWin.Button_accept.Clicked -= new EventHandler(on_button_run_encoder_exercise_add_accepted);
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
		{
			genericWin.SetLabelError(Catalog.GetString("Error: Missing name of exercise."));
			return false;
		}
		else if (adding && Sqlite.Exists(false, Constants.RunEncoderExerciseTable, name))
		{
			genericWin.SetLabelError(string.Format(Catalog.GetString(
							"Error: An exercise named '{0}' already exists."), name));
			return false;
		}
		else if (! adding) //if we are editing
		{
			//if we edit, check that this name does not exists (on other exercise, on current editing exercise is obviously fine)
			int getIdOfThis = Sqlite.ExistsAndGetUniqueID(false, Constants.RunEncoderExerciseTable, name); //if not exists will be -1
			if(getIdOfThis != -1 && getIdOfThis != genericWin.uniqueID)
			{
				genericWin.SetLabelError(string.Format(Catalog.GetString(
								"Error: An exercise named '{0}' already exists."), name));

				return false;
			}
		}

		if(adding)
			SqliteRunEncoderExercise.Insert(false, -1, name,
					genericWin.Entry2Selected, genericWin.SpinInt2Selected);
		else {
			RunEncoderExercise ex = new RunEncoderExercise(genericWin.uniqueID, name,
					genericWin.Entry2Selected, genericWin.SpinInt2Selected);

			SqliteRunEncoderExercise.Update(false, ex);
		}

		fillRunEncoderExerciseCombo(name);
		LogB.Information("done");
		return true;
	}

	//based on: on_button_encoder_exercise_delete
	//maybe unify them on the future
	void on_button_run_encoder_exercise_delete_clicked (object o, EventArgs args)
	{
		if(UtilGtk.ComboGetActive(combo_run_encoder_exercise) == "")
		{
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Need to create/select an exercise."));
			return;
		}

		RunEncoderExercise ex = (RunEncoderExercise) SqliteRunEncoderExercise.Select (
                                false, getExerciseIDFromAnyCombo(combo_run_encoder_exercise, runEncoderComboExercisesString, false), false)[0];

		LogB.Information("selected exercise: " + ex.ToString());

		//1st find if there are sets with this exercise
		ArrayList array = SqliteRunEncoder.SelectRowsOfAnExercise(false, ex.UniqueID);

		if(array.Count > 0)
		{
			genericWin = GenericWindow.Show(Catalog.GetString("Exercise"),
					Catalog.GetString("Exercise name:"), Constants.GenericWindowShow.ENTRY, false);

			genericWin.EntrySelected = ex.Name;

			//just one button to exit and with ESC accelerator
			genericWin.ShowButtonAccept(false);
			genericWin.SetButtonCancelLabel(Catalog.GetString("Close"));

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
		} else {
			//runEncoder table has not records of this exercise. Delete exercise
			SqliteRunEncoderExercise.Delete(false, ex.UniqueID);

			fillRunEncoderExerciseCombo("");
			combo_run_encoder_exercise.Active = 0;

			new DialogMessage(Constants.MessageTypes.INFO, Catalog.GetString("Exercise deleted."));
		}
	}
	// -------------------------------- end of exercise stuff --------------------

	CairoRadial cairoRadial;
	private void on_drawingarea_race_analyzer_capture_velocimeter_expose_event (object o, ExposeEventArgs args)
	{
		Gtk.DrawingArea da;
		if (o == (object) drawingarea_race_analyzer_capture_velocimeter_topleft)
			da = drawingarea_race_analyzer_capture_velocimeter_topleft;
		else if (o == (object) drawingarea_race_analyzer_capture_velocimeter_bottom)
			da = drawingarea_race_analyzer_capture_velocimeter_bottom;
		else
			return;

		if(cairoRadial == null)
			cairoRadial = new CairoRadial(da, preferences.fontType.ToString());

		//when person or session changes
		if(! runEncoderShouldShowCaptureGraphsWithData)
		{
			cairoRadial.GraphBlank();
			return;
		}

		if(reCGSD == null)
		{
			cairoRadial.GraphBlank();
			return;
		}

		if(runEncoderCaptureThread != null && runEncoderCaptureThread.IsAlive)
			cairoRadial.GraphSpeedAndDistance(reCGSD.RunEncoderCaptureSpeed, reCGSD.RunEncoderCaptureDistance);
		else
			cairoRadial.GraphSpeedMaxAndDistance(reCGSD.RunEncoderCaptureSpeedMax, reCGSD.RunEncoderCaptureDistance);
	}

	CairoGraphRaceAnalyzer cairoGraphRaceAnalyzer_dt;
	static List<PointF> cairoGraphRaceAnalyzerPoints_dt_l; //distancetime
	private void on_drawingarea_race_analyzer_capture_position_time_expose_event (object o, ExposeEventArgs args)
	{
		updateRaceAnalyzerCapturePositionTime(true);
	}

	CairoGraphRaceAnalyzer cairoGraphRaceAnalyzer_st;
	static List<PointF> cairoGraphRaceAnalyzerPoints_st_l;	//speed/time
	private void on_drawingarea_race_analyzer_capture_speed_time_expose_event (object o, ExposeEventArgs args)
	{
		updateRaceAnalyzerCaptureSpeedTime(true);
	}

	CairoGraphRaceAnalyzer cairoGraphRaceAnalyzer_at;
	static List<PointF> cairoGraphRaceAnalyzerPoints_at_l;	//accel/time
	private void on_drawingarea_race_analyzer_capture_accel_time_expose_event (object o, ExposeEventArgs args)
	{
		updateRaceAnalyzerCaptureAccelTime(true);
	}

	private void updateRaceAnalyzerCapturePositionTime(bool forceRedraw)
	{
		if(radio_race_analyzer_capture_view_simple.Active)
			return;

		if(cairoGraphRaceAnalyzer_dt == null)
			cairoGraphRaceAnalyzer_dt = new CairoGraphRaceAnalyzer(
					drawingarea_race_analyzer_capture_position_time, "title",
					Catalog.GetString("Distance"), "m", false);

		cairoGraphRaceAnalyzer_dt.DoSendingList (preferences.fontType.ToString(),
				cairoGraphRaceAnalyzerPoints_dt_l, forceRedraw, CairoXY.PlotTypes.POINTSLINES);
	}
	private void updateRaceAnalyzerCaptureSpeedTime(bool forceRedraw)
	{
		if(cairoGraphRaceAnalyzer_st == null)
			cairoGraphRaceAnalyzer_st = new CairoGraphRaceAnalyzer(
					drawingarea_race_analyzer_capture_speed_time, "title",
					Catalog.GetString("Speed"), "m/s", true);

		cairoGraphRaceAnalyzer_st.DoSendingList (preferences.fontType.ToString(),
				cairoGraphRaceAnalyzerPoints_st_l, forceRedraw, CairoXY.PlotTypes.POINTSLINES);
	}
	private void updateRaceAnalyzerCaptureAccelTime(bool forceRedraw)
	{
		if(radio_race_analyzer_capture_view_simple.Active)
			return;

		if(cairoGraphRaceAnalyzer_at == null)
			cairoGraphRaceAnalyzer_at = new CairoGraphRaceAnalyzer(
					drawingarea_race_analyzer_capture_accel_time, "title",
					Catalog.GetString("Accel"), "m/s^2", false);

		cairoGraphRaceAnalyzer_at.DoSendingList (preferences.fontType.ToString(),
				cairoGraphRaceAnalyzerPoints_at_l, forceRedraw, CairoXY.PlotTypes.LINES);
	}
}
