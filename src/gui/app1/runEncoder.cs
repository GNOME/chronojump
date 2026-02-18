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
 * Copyright (C) 2018-2026   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using Gtk;
using Gdk;
//using Glade;
using System.Text; //StringBuilder
using System.Collections;
using System.Collections.Generic; //List<T>
using System.Text.RegularExpressions; //Regex
using System.Diagnostics;  //Stopwatch
using Mono.Unix;


public partial class ChronoJumpWindow 
{
	private bool debugForceTest = false;

	// at glade ---->
	//Gtk.CheckMenuItem menuitem_check_race_encoder_capture_simulate;

	Gtk.Button button_combo_run_encoder_exercise_capture_left;
	Gtk.Button button_combo_run_encoder_exercise_capture_right;
	Gtk.VBox vbox_run_encoder_capture_buttons;
	Gtk.VBox vbox_run_encoder_capture_options;
	Gtk.HBox hbox_combo_run_encoder_exercise;
	Gtk.SpinButton race_analyzer_spinbutton_distance;
	Gtk.SpinButton race_analyzer_spinbutton_angle;
	Gtk.SpinButton race_analyzer_spinbutton_temperature;
	Gtk.TreeView treeview_raceAnalyzer;
	Gtk.Button button_raceAnalyzer_table_save;
	//Gtk.Label label_race_analyzer_capture_speed;
	Gtk.Grid grid_race_analyzer_capture_tab_result_views;
	Gtk.RadioButton radio_race_analyzer_capture_view_simple;
	Gtk.RadioButton radio_race_analyzer_capture_view_complete;
	//Gtk.Alignment alignment_drawingarea_race_analyzer_capture_velocimeter_topleft;
	Gtk.Alignment alignment_hbox_race_analyzer_capture_bottom;
	//Gtk.DrawingArea drawingarea_race_analyzer_capture_velocimeter_topleft;
	Gtk.DrawingArea drawingarea_race_analyzer_capture_velocimeter_bottom;
	Gtk.DrawingArea drawingarea_race_analyzer_capture_position_time;
	Gtk.DrawingArea drawingarea_race_analyzer_capture_speed_time;
	Gtk.DrawingArea drawingarea_race_analyzer_capture_accel_time;
	Gtk.VBox vbox_race_analyzer_capture_graphs;
	Gtk.RadioButton radio_race_analyzer_capture_graph_starts_0;
	Gtk.RadioButton radio_race_analyzer_capture_graph_starts_grav;
	Gtk.CheckButton check_race_analyzer_capture_smooth_graphs;
	Gtk.HScale hscale_race_analyzer_capture_smooth_graphs;
	Gtk.Label label_race_analyzer_capture_smooth_graphs;

	Gtk.Frame frame_run_encoder_exercise;
	Gtk.Entry entry_run_encoder_exercise_name;
	Gtk.Entry entry_run_encoder_exercise_description;
	Gtk.CheckButton check_run_encoder_exercise_is_sprint;
	Gtk.SpinButton spin_run_encoder_exercise_angle_default;
	Gtk.CheckButton check_run_encoder_exercise_fixed_size;
	Gtk.HBox hbox_run_encoder_exercise_fixed_segments_size;
	Gtk.HBox hbox_run_encoder_exercise_notfixed_segment_num;
	Gtk.SpinButton	spin_race_encoder_exercise_f_segment_size_cm; //f: fixed
	Gtk.SpinButton spin_race_encoder_exercise_v_segments_num; //v: variable
	Gtk.Frame frame_run_encoder_exercise_notfixed_segments;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_0; //v: variable
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_1;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_2;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_3;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_4;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_5;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_6;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_7;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_8;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_9;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_10;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_11;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_12;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_13;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_14;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_15;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_16;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_17;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_18;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_19;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_20;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_21;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_22;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_23;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_24;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_25;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_26;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_27;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_28;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_29;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_30;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_31;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_32;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_33;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_34;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_35;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_36;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_37;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_38;
	Gtk.SpinButton spin_race_encoder_exercise_v_segment_size_cm_39;
	Gtk.Box box_combo_race_analyzer_device;
	// <---- at glade

	Gtk.ComboBoxText combo_race_analyzer_device;

	int race_analyzer_distance;
	//int race_analyzer_angle;
	int race_analyzer_temperature;
	RunEncoder.Devices race_analyzer_device;

	Gtk.ComboBoxText combo_run_encoder_exercise;

	Thread runEncoderCaptureThread;
	static bool runEncoderProcessFinish;
	static bool runEncoderProcessCancel;
	static bool runEncoderProcessError;
	static int runEncoderTotalTime;
        static string runEncoderPulseMessage = "";
	static bool runEncoderShouldShowCaptureGraphsWithData; //on change person this is false
	
	private RunEncoder currentRunEncoder;
	private RunEncoder currentRunEncoder_CD = null; //only for analyze cd (when 2 sets)

	private RunEncoderExercise currentRunEncoderExercise;
	private RunEncoderExercise currentRunEncoderExercise_CD; //only for analyze cd (when 2 sets)

	DateTime runEncoderTimeStartCapture;
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

		check_race_analyzer_capture_smooth_graphs.Active = false;
		hscale_race_analyzer_capture_smooth_graphs.Visible = false;
		label_race_analyzer_capture_smooth_graphs.Text = "";

		createRaceAnalyzerDeviceCombo ();
		createRunEncoderExerciseCombo();
		createRunEncoderAnalyzeCombos();
		setRunEncoderAnalyzeWidgets();

		aiButtonsHscaleZoomSensitiveness();
		updateGraphRunEncoderBars ();
	}

	private void manageRunEncoderCaptureViews()
	{
		if(radio_race_analyzer_capture_view_simple.Active)
		{
			//alignment_drawingarea_race_analyzer_capture_velocimeter_topleft.Visible = true;
			alignment_hbox_race_analyzer_capture_bottom.Visible = false;
			cairoRadial = null;
			//drawingarea_race_analyzer_capture_velocimeter_topleft.QueueDraw(); //will fire ExposeEvent
		} else {
			//alignment_drawingarea_race_analyzer_capture_velocimeter_topleft.Visible = false;
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
		if (chronopicRegister.GetSelectedForMode (current_mode).Port == "")
		{
			runEncoderPulseMessage = runEncoderNotConnectedString;
			return false;
		}

		LogB.Information(" RE connect 1 ");
		runEncoderPulseMessage = "Connecting ...";

		portRE = new SerialPort (chronopicRegister.GetSelectedForMode (current_mode).Port, 115200);
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
		//event_execute_label_message.Text = "Disconnected!";
		LogB.Information("runEncoder portRE Disconnected!");
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
			LogB.Information(string.Format("init string: |{0}|", str));
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
		if(match.Groups.Count == 2)
			return str = match.Groups[1].ToString();
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
		sensitiveSelectedTestButtons (false);

		//reset capture tab graphs
		if(cairoRadial != null)
			cairoRadial.ResetSpeedMax();

		//blank Cairo scatterplot graphs
		cairoGraphRaceAnalyzer_dt = null;
		cairoGraphRaceAnalyzer_st = null;
		cairoGraphRaceAnalyzer_at = null;
		cairoGraphRaceAnalyzerPoints_dt_l = new List<PointF>();
		cairoGraphRaceAnalyzerPoints_st_l = new List<PointF>();
		cairoGraphRaceAnalyzerPoints_st_Zoom_l = new List<PointF>();
		cairoGraphRaceAnalyzerPoints_st_CD_l = new List<PointF>();
		//cairoGraphRaceAnalyzerPoints_st_CD_l_timeShifted = false; //unused
		cairoGraphRaceAnalyzerPoints_st_Zoom_CD_l = new List<PointF>();
		cairoGraphRaceAnalyzerPoints_at_l = new List<PointF>();

		//to not show old vertical segments info on a capture that maybe cannot be done by lack of device
		if(currentRunEncoderExercise != null)
			reCGSD = new RunEncoderCaptureGetSpeedAndDisplacement(
					currentRunEncoderExercise.SegmentCm, currentRunEncoderExercise.SegmentVariableCm,
					currentPersonSession.Weight, //but note if person changes (but graph will be hopefully erased), this will change also take care on exports
					Convert.ToInt32(race_analyzer_spinbutton_angle.Value));

		drawingarea_race_analyzer_capture_position_time.QueueDraw ();
		drawingarea_race_analyzer_capture_speed_time.QueueDraw ();
		drawingarea_race_analyzer_capture_accel_time.QueueDraw ();

		if (chronopicRegister.GetSelectedForMode (current_mode).Port == "")
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
		assignCurrentRunEncoderExerciseFromCombo ();
		raceEncoderReadWidgets();

		//remove stuff on analyze tab
		image_ai_model_graph.Visible = false;
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
		currentRunEncoder_CD = null;

		blankAIInterface ();

		//draw the capture graphs empty:
		//a) radial
		runEncoderShouldShowCaptureGraphsWithData = false;

		/*
		if(radio_race_analyzer_capture_view_simple.Active)
			drawingarea_race_analyzer_capture_velocimeter_topleft.QueueDraw(); //will fire ExposeEvent
		else */
			drawingarea_race_analyzer_capture_velocimeter_bottom.QueueDraw(); //will fire ExposeEvent

		//b) scatterplots
		cairoGraphRaceAnalyzerPoints_dt_l = new List<PointF>();
		cairoGraphRaceAnalyzerPoints_st_l = new List<PointF>();
		cairoGraphRaceAnalyzerPoints_st_Zoom_l = new List<PointF>();
		cairoGraphRaceAnalyzerPoints_st_CD_l = new List<PointF>();
		//cairoGraphRaceAnalyzerPoints_st_CD_l_timeShifted = false; //unused
		cairoGraphRaceAnalyzerPoints_st_Zoom_CD_l = new List<PointF>();
		cairoGraphRaceAnalyzerPoints_at_l = new List<PointF>();
		drawingarea_race_analyzer_capture_position_time.QueueDraw(); //will fire ExposeEvent
		drawingarea_race_analyzer_capture_speed_time.QueueDraw(); //will fire ExposeEvent
		drawingarea_race_analyzer_capture_accel_time.QueueDraw(); //will fire ExposeEvent

		radiosAiSensitivity (true); //because maybe zoom was in
		aiButtonsHscaleZoomSensitiveness();

		//image_ai_model_graph.Sensitive = false; //this is not useful at all
		image_ai_model_graph.Visible = false;

		treeview_raceAnalyzer = UtilGtk.RemoveColumns(treeview_raceAnalyzer);
		button_raceAnalyzer_table_save.Sensitive = false;
		triggerListRunEncoder = new TriggerList();
		clearRaceAnalyzerTriggersFromTextView();

		check_ai_chained_hscales.Sensitive = false;
		button_ai_ab_options.Sensitive = false;
		button_ai_model.Sensitive = false;
		sensitiveSelectedTestButtons (false);
		button_ai_model_save_image.Sensitive = false;
		button_video_play_this_test.Sensitive = false;

		if (radio_ai_export_individual_current_session.Active)
		{
			if(currentPerson != null)
				label_ai_export_person.Text = currentPerson.Name;
			else
				label_ai_export_person.Text = "";
		}

		label_run_encoder_export_discarded.Text = "";
		button_ai_export_result_open.Visible = false;

		updateGraphRunEncoderBars ();
	}

	private void raceEncoderReadWidgets()
	{
		race_analyzer_distance = Convert.ToInt32(race_analyzer_spinbutton_distance.Value);
		//race_analyzer_angle = Convert.ToInt32(race_analyzer_spinbutton_angle.Value);
		race_analyzer_temperature = Convert.ToInt32(race_analyzer_spinbutton_temperature.Value);

		if(UtilGtk.ComboGetActive(combo_race_analyzer_device) == RunEncoder.DevicesStringMANUAL)
			race_analyzer_device = RunEncoder.Devices.MANUAL;
		else
			race_analyzer_device = RunEncoder.Devices.RESISTED;
	}

	private RunEncoder.Devices raceEncoderGetDeviceFromGui ()
	{
		if(UtilGtk.ComboGetActive(combo_race_analyzer_device) == RunEncoder.DevicesStringMANUAL)
			return RunEncoder.Devices.MANUAL;
		else
			return RunEncoder.Devices.RESISTED;
	}

	private void on_combo_race_analyzer_device_changed (object o, EventArgs args)
	{
		runEncoderImageTestChange();
	}

	private void runEncoderImageTestChange()
	{
		Pixbuf pixbuf; //main image
		if(UtilGtk.ComboGetActive(combo_race_analyzer_device) == RunEncoder.DevicesStringMANUAL)
			pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(true) + "run-encoder-manual.png");
		else
			pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(true) + "run-encoder-resisted.png");

		image_test.Pixbuf = pixbuf;
	}

	// used on capture
	private void assignCurrentRunEncoderExerciseFromCombo ()
	{
		currentRunEncoderExercise = SqliteRunEncoderExercise.Select (
                                false, getExerciseIDFromAnyCombo(combo_run_encoder_exercise, runEncoderComboExercisesString, false))[0];
	}
	// used on load, edit
	private void assignCurrentRunEncoderExerciseFromSQL ()
	{
		currentRunEncoderExercise = SqliteRunEncoderExercise.Select (
				false, currentRunEncoder.ExerciseID)[0];
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

		webcamStatusEnumSetStart ();

		//forceCaptureStartMark = false;

		runEncoderProcessFinish = false;
		runEncoderProcessCancel = false;
		runEncoderProcessError = false;

		//To know if USB has been disconnected
		usbDisconnectedCount = 0;
		usbDisconnectedLastTime = 0;
		contactsShowCaptureDoingButtons(true);

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
		cairoGraphRaceAnalyzerPoints_st_Zoom_l = new List<PointF>();
		cairoGraphRaceAnalyzerPoints_st_CD_l = new List<PointF>();
		//cairoGraphRaceAnalyzerPoints_st_CD_l_timeShifted = false; //unused
		cairoGraphRaceAnalyzerPoints_st_Zoom_CD_l = new List<PointF>();
		cairoGraphRaceAnalyzerPoints_at_l = new List<PointF>();

		//RunEncoderCaptureGetSpeedAndDisplacement reCGSD = new RunEncoderCaptureGetSpeedAndDisplacement();
		reCGSD = new RunEncoderCaptureGetSpeedAndDisplacement(
				currentRunEncoderExercise.SegmentCm, currentRunEncoderExercise.SegmentVariableCm,
				currentPersonSession.Weight, //but note if person changes (but graph will be hopefully erased), this will change also take care on exports
				Convert.ToInt32(race_analyzer_spinbutton_angle.Value));
		runEncoderShouldShowCaptureGraphsWithData = true;

		blinkCapture = new BlinkImage (image_no_capturing, image_capturing);

		if (configChronojump.EncoderPT)
		{
			//do not need to show velocimeter
			drawingarea_race_analyzer_capture_velocimeter_bottom.Visible = false;

			runEncoderCaptureThread = new Thread(new ThreadStart(runEncoderAsLinearEncoderCaptureDo));
			GLib.Idle.Add (new GLib.IdleHandler (pulseGTKRunEncoderAsLinearEncoderCapture));
		} else {
			runEncoderTotalTime = 0;
			runEncoderShiftedMicros = 0;

			runEncoderCaptureThread = new Thread(new ThreadStart(runEncoderCaptureDo));
			GLib.Idle.Add (new GLib.IdleHandler (pulseGTKRunEncoderCapture));
		}

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
	static RunEncoderCaptureGetSpeedAndDisplacement reCGSD_CD;

	/*
	 * runEncoderAsLinearEncoder start ------------>
	 */

	EncoderPTCaptureManage eptcm;
	EncoderPTCapture eptc;

	private void runEncoderAsLinearEncoderCaptureDo()
	{
		runEncoderPulseMessage = "Capture as linear encoder... please wait";
		LogB.Information("eptcm start");

		if (eptc == null || eptc.PortName != chronopicRegister.GetSelectedForMode (current_mode).Port)
			eptc = new EncoderPTCapture (
					chronopicRegister.GetSelectedForMode (current_mode).Port,
					preferences.runEncoderPPS);
		else if (eptc.RunEncoderPPS != preferences.runEncoderPPS)
			eptc.RunEncoderPPS = preferences.runEncoderPPS;

		//need to pass the ref every capture because every capture we do:
		//cairo...Points_xx_l = new List<PointF> ()
		eptcm = new EncoderPTCaptureManage (
				eptc,
				ref cairoGraphRaceAnalyzerPoints_dt_l,
				ref cairoGraphRaceAnalyzerPoints_st_l,
				ref cairoGraphRaceAnalyzerPoints_at_l
				);

		LogB.Information("eptcm start do");
		if (eptcm.Init ())
		{
			capturingRunEncoder = arduinoCaptureStatus.CAPTURING;
			runEncoderPulseMessage = capturingMessage;
			eptcm.Capture ();

			/*
			LogB.Information("eptcm end");
			runEncoderPulseMessage = "Done! check log";

			runEncoderProcessCancel = true;
			//capturingRunEncoder = arduinoCaptureStatus.STOP;
			*/
		}
	}

	//most of this code comes from pulseGTKRunEncoderCapture ()
	private bool pulseGTKRunEncoderAsLinearEncoderCapture ()
	{
		if(runEncoderCaptureThread == null)
		{
			Thread.Sleep (25);
			return true;
		}

		event_execute_label_message.Text = runEncoderPulseMessage;
		if(! runEncoderCaptureThread.IsAlive || runEncoderProcessFinish || runEncoderProcessCancel || runEncoderProcessError) //capture ends
		{
			if (runEncoderProcessCancel && eptcm != null)
			{
				event_execute_label_message.Text = "Cancelled.";
				eptcm.Cancel = true;
			}

			blinkCapture.End ();
			showHideBlinkIcon (blinkCapture, false);

			sensitiveSelectedTestButtons (false);
			contactsShowCaptureDoingButtons(false);
			button_ai_model_options_close_and_analyze.Sensitive = false;
			check_ai_chained_hscales.Sensitive = false;
			button_ai_ab_options.Sensitive = false;
			button_ai_model.Sensitive = false;
			button_ai_model_save_image.Sensitive = false;

			LogB.ThreadEnding();
			LogB.Mute = preferences.muteLogs;
			if(! preferences.muteLogs)
				LogB.Information("muteLogs INactive. Logs active active again");
			LogB.ThreadEnded();

			runEncoderButtonsSensitive(true);
			radio_signal_analyze_current_set.Active = true;
			hideButtons();

			drawingarea_race_analyzer_capture_position_time.QueueDraw ();
			drawingarea_race_analyzer_capture_speed_time.QueueDraw ();
			drawingarea_race_analyzer_capture_accel_time.QueueDraw ();

			return false;
		} else {
			if (capturingRunEncoder == arduinoCaptureStatus.CAPTURING)
			{
				if (blinkCapture.Status == Blink.StatusEnum.NOTSTARTED)
					blinkCapture.Start (); //TODO: but note here is still connecting
				showHideBlinkIcon (blinkCapture, true);

				drawingarea_race_analyzer_capture_position_time.QueueDraw ();
				drawingarea_race_analyzer_capture_speed_time.QueueDraw ();
				drawingarea_race_analyzer_capture_accel_time.QueueDraw ();
			}
		}

		Thread.Sleep (50);
		//LogB.Information("RunEncoderAsLinearEncoder:"+ runEncoderCaptureThread.ThreadState.ToString());
		return true;
	}

	/*
	 * <---------- runEncoderAsLinearEncoder end
	 */



	//non GTK on this method
	private void runEncoderCaptureDo()
	{
		RunEncoderCaptureGetSpeedAndDisplacementTest recgsdt = new RunEncoderCaptureGetSpeedAndDisplacementTest ();

		LogB.Information("runEncoderCaptureDo 0");

		if(! portREOpened)
			if(! runEncoderConnect()) //GTK
			{
				runEncoderProcessError = true;
				return;
			}

		lastChangedTime = 0;

                double versionDouble = Convert.ToDouble(Util.ChangeDecimalSeparator(runEncoderFirmwareVersion));
		if(versionDouble >= Convert.ToDouble(Util.ChangeDecimalSeparator("0.3")))
		{
			if(! runEncoderSendCommand(string.Format("set_pps:{0};", preferences.runEncoderPPS), "Sending pps", "Catched at set_pps"))
			{
				runEncoderProcessError = true;
				return;
			}

			//read confirmation data
			if(runEncoderReceiveFeedback("pps set to") == "")
			{
				runEncoderProcessError = true;
				return;
			}
		}

		string command = "start_capture:";
		if (Config.SimulatedCapture)
			command = "start_simulation:"; //needs the Arduino

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

		Util.PlaySound (Constants.SoundTypes.CAN_START, preferences.volumeOn, preferences.gstreamer);

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
		bool enoughAccelFound = false; //accel has been > preferences.runEncoderMinAccel (default 10ms^2)

		int rowsCount = 0;
		int bytesToRead = 0;
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

			try {
				bytesToRead = portRE.BytesToRead;
			} catch {
				LogB.Information ("catched on raceAnalyzer BytesToRead. Disconnected on the middle of the capture");
				runEncoderProcessError = true;
				continue; //to go to exit bucle in order to close writer and delete file
			}

			// readBinaryRunEncoder9Bytes will read 9 bytes
			if (portRE.BytesToRead < 9)
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
			List<int> binaryReaded;

			if (debugForceTest)
			{
				if (! recgsdt.ExistsMoreSamples ())
				{
					runEncoderProcessFinish = true;
					continue;
				}
				else
					binaryReaded = recgsdt.GetNextSample ();
			}
			else
				binaryReaded = readBinaryRunEncoder9Bytes ();

			reCGSD.PassCapturedRow (binaryReaded);
			if(reCGSD.Calcule (true) && reCGSD.EncoderDisplacement != 0) //this 0s are triggers without displacement
			{
				//distance/time
				cairoGraphRaceAnalyzerPoints_dt_l.Add(new PointF(
							UtilAll.DivideSafe(reCGSD.Time, 1000000),
							reCGSD.RunEncoderCaptureDistance));
				//speed/time
				cairoGraphRaceAnalyzerPoints_st_l.Add(new PointF(
							UtilAll.DivideSafe(reCGSD.Time, 1000000),
							reCGSD.RunEncoderCaptureSpeed));
				cairoGraphRaceAnalyzerPoints_st_CD_l.Add(new PointF(
							UtilAll.DivideSafe(reCGSD.Time, 1000000),
							reCGSD.RunEncoderCaptureSpeed));

				speedPre2 = speedPre;
				timePre2 = timePre;
				speedPre = reCGSD.RunEncoderCaptureSpeed;
				timePre = UtilAll.DivideSafe(reCGSD.Time, 1000000);

				if(timePre2 > 0)
				{
					double accel = UtilAll.DivideSafe(reCGSD.RunEncoderCaptureSpeed - speedPre2,
								UtilAll.DivideSafe(reCGSD.Time, 1000000) - timePre2);

					if (accel >= preferences.runEncoderMinAccel && ! enoughAccelFound)
					{
						enoughAccelFound = true;

						//at load to shift times to the left
						//at capture to draw a vertical line
						reCGSD.SetTimeAtEnoughAccelMark (binaryReaded, reCGSD.RunEncoderCaptureSpeed);
						runEncoderShiftedMicros = binaryReaded[2]; //time
					}

					//accel/time
					cairoGraphRaceAnalyzerPoints_at_l.Add(new PointF(
								UtilAll.DivideSafe(reCGSD.Time, 1000000),
								accel));
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
		if (reCGSD != null)
			reCGSD.CalculeBestSecond ();

		sw.Stop();

		LogB.Information(string.Format("FINISHED WITH conditions: {0}-{1}-{2}",
						runEncoderProcessFinish, runEncoderProcessCancel, runEncoderProcessError));

		if (! runEncoderProcessError)
		{
			LogB.Information("Calling end_capture");
			if(! runEncoderSendCommand("end_capture:", "Ending capture ...", "Catched ending capture"))
			{
				runEncoderProcessError = true;
				capturingRunEncoder = arduinoCaptureStatus.STOP;
				Util.FileDelete(fileName);
				return;
			}

			LogB.Information("Waiting end_capture");
			int notValidCommandCount = 0;
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

				//See comment on gui/app1/forceSensor.cs "2023 Aug 3"
				if (str.Contains ("Not a valid command"))
				{
					LogB.Information ("Not a valid commmand");
					notValidCommandCount ++;

					if (notValidCommandCount > 10 || ! runEncoderSendCommand("end_capture:", "Ending capture ...", "Catched ending capture"))
					{
						runEncoderProcessError = true;
						capturingRunEncoder = arduinoCaptureStatus.STOP;
						Util.FileDelete(fileName);
						return;
					}
				}
			}
			while(! str.Contains("Capture ended"));
			LogB.Information("Success: received end_capture");

			string [] strEnded = str.Split(new char[] {':'});
			if(strEnded.Length == 3 && Util.IsNumber(strEnded[2], false))
			{
				LogB.Information (string.Format ("runEncoderTotalTime: {0}", strEnded[2]));
				runEncoderTotalTime = Convert.ToInt32 (strEnded[2]);
			}
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

	// this code is also on gui/discover.cs we are not unifying on src/runEncoder.cs to not pass serial port that is (or was) a problem on Windows
	// time (4 bytes: long at Arduino, uint at c-sharp), force (2 bytes: uint), encoder/RCA (1 byte: uint)
	private List<int> readBinaryRunEncoder9Bytes ()
        {
		LogB.Information("start reading binary data");
		//LogB.Debug("readed start mark Ok");
                List<int> dataRow = new List<int>();

		var buffer = new byte[1024];
		int bytesRead = 0;
		try {
			bytesRead = portRE.Read (buffer, 0, 9);
		}
		catch (Exception ex)
		{
			if(ex is System.IO.IOException || ex is System.TimeoutException)
				LogB.Information ("catched on readBinaryRunEncoder9Bytes portRE.Read ()");

			return dataRow;
		}

		int count = 0;

		// 1) encoderDisplacement (2 bytes)
                int b0 = buffer[count ++]; //encoderDisplacement least significative
                int b1 = buffer[count ++]; //encoderDisplacement most significative
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
                b0 = buffer[count ++]; //least significative
                b1 = buffer[count ++];
                int b2 = buffer[count ++];
                int b3 = buffer[count ++]; //most significative
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
		b0 = buffer[count ++]; //least significative
		b1 = buffer[count ++]; //most significative
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
		b0 = buffer[count ++];
		//LogB.Information("b0: " + b0.ToString());
		dataRow.Add(Convert.ToInt32(b0));

		LogB.Information("readed all binary data");

                return dataRow;
        }

	private string [] getRunEncoderLoadColumnsString ()
	{
		return  new string [] {
			Catalog.GetString("ID"),
				Catalog.GetString("Set"),
				Catalog.GetString("Exercise"),
				//Catalog.GetString("Device"),
				Catalog.GetString("Distance"),
				Catalog.GetString("Date"),
				Catalog.GetString("Video"),
				Catalog.GetString("Comment")
		};
	}

	private ArrayList getRunEncoderLoadSetsDataPrint (int personID, int sessionID)
	{
		List<RunEncoder> data = SqliteRunEncoder.Select (false, -1, personID, sessionID);

		ArrayList dataPrint = new ArrayList();
		int count = 1;
		foreach(RunEncoder re in data)
			dataPrint.Add(re.ToStringArray(false, count++));

		return dataPrint;
	}

	// canChoosePersonAndSession is also CD
	private void run_encoder_load (bool canChoosePersonAndSession)
	{
		finishPlayVideoIfRunning ();

		string [] colStr = getRunEncoderLoadColumnsString ();
		ArrayList dataPrint = getRunEncoderLoadSetsDataPrint (currentPerson.UniqueID, currentSession.UniqueID);

		ArrayList bigArray = new ArrayList();
		ArrayList a1 = new ArrayList();
		//0 is the widgget to show; 1 is the editable; 2 id default value
		a1.Add(Constants.GenericWindowShow.TREEVIEW); a1.Add(true); a1.Add("");
		bigArray.Add(a1);

		/* this actually does not do nothing
		//if (! canChoosePersonAndSession) //do not allow to edit when can change person/session
		//{
			ArrayList a2 = new ArrayList();
			a2.Add(Constants.GenericWindowShow.COMBO); a2.Add(true); a2.Add("");
			bigArray.Add(a2);
		*/

		string title = string.Format (Catalog.GetString ("Select set of athlete {0} on this session."), currentPerson.Name);

		if (canChoosePersonAndSession)
		{
			ArrayList a3 = new ArrayList ();
			a3.Add (Constants.GenericWindowShow.GRIDSESSIONPERSON); a3.Add (true); a3.Add ("");
			bigArray.Add (a3);

			title = Catalog.GetString ("Select set to compare");
		}

		genericWin = GenericWindow.Show (Catalog.GetString("Load"), false, title, bigArray);

		if (canChoosePersonAndSession)
		{
			genericWin.SetGridSessionPerson (currentPerson, currentSession, current_mode);

			//do not allow to edit when can change person/session
			genericWin.SetTreeview (colStr, false, dataPrint, new ArrayList(), GenericWindow.EditActions.NONE, true);

			genericWin.FakeButtonNeedUpdateTreeView.Clicked -= new EventHandler (on_run_encoder_load_signal_update_treeview);
			genericWin.FakeButtonNeedUpdateTreeView.Clicked += new EventHandler (on_run_encoder_load_signal_update_treeview);
		} else {
			genericWin.SetTreeview (colStr, false, dataPrint, new ArrayList(), GenericWindow.EditActions.EDITPLAYDELETE, true);

			//find all persons in current session
			ArrayList personsPre = SqlitePersonSession.SelectCurrentSessionPersons (
					currentSession.UniqueID,
					true, 	// ifAllSessionsGetLastOfEachPerson
					false); //means: do not returnPersonAndPSlist

			string [] persons = new String[personsPre.Count];
			int count = 0;
			foreach	(Person p in personsPre)
				persons[count++] = p.UniqueID.ToString() + ":" + p.Name;
			genericWin.SetComboEditValues (persons, currentPerson.UniqueID + ":" + currentPerson.Name);
			//genericWin.SetComboLabel(Catalog.GetString("Change the owner of selected set") +
			//		" (" + Catalog.GetString("code") + ":" + Catalog.GetString("name") + ")");
			genericWin.SetComboLabel(Catalog.GetString("Change person"));
			genericWin.ShowEditRow(false);
		}

		//select row corresponding to current signal
		if(currentRunEncoder != null)
			genericWin.SelectRowWithID(0, currentRunEncoder.UniqueID); //colNum, id

		genericWin.VideoColumn = 5;
		genericWin.CommentColumn = 6;

		genericWin.ShowButtonCancel(true);
		genericWin.SetButtonAcceptLabel(Catalog.GetString("Load"));
		genericWin.SetButtonCancelLabel(Catalog.GetString("Close"));
		genericWin.SetButtonAcceptSensitive(false);

		if (canChoosePersonAndSession)
			genericWin.Button_accept.Clicked += new EventHandler(on_run_encoder_load_cd_accepted); //gets person and session from genericWin (and uniqueID)
		else
		{
			genericWin.Button_accept.Clicked += new EventHandler(on_run_encoder_load_accepted);

			genericWin.Button_row_play.Clicked += new EventHandler(on_run_encoder_load_signal_row_play);
			genericWin.Button_row_edit.Clicked += new EventHandler(on_run_encoder_load_signal_row_edit);
			genericWin.Button_row_edit_apply.Clicked += new EventHandler(on_run_encoder_load_signal_row_edit_apply);
			genericWin.Button_row_delete.Clicked += new EventHandler(on_run_encoder_load_signal_row_delete_prequestion);
		}

		genericWin.ShowNow();
	}

	private void on_run_encoder_load_signal_update_treeview (object o, EventArgs args)
	{
		LogB.Information ("on_run_encoder_load_signal_update_treeview");

		string [] colStr = getRunEncoderLoadColumnsString ();
		ArrayList dataPrint = getRunEncoderLoadSetsDataPrint (
				genericWin.GetPersonIDFromGui (), genericWin.GetSessionIDFromGui ());

		genericWin.SetTreeview (colStr, false, dataPrint, new ArrayList(), GenericWindow.EditActions.NONE, true);
	}

	private void on_run_encoder_load_accepted (object o, EventArgs args)
	{
		LogB.Information("on_run_encoder_load_accepted");
		genericWin.Button_accept.Clicked -= new EventHandler(on_run_encoder_load_accepted);

		on_run_encoder_load_set_pre (false);
	}
	private void on_run_encoder_load_cd_accepted (object o, EventArgs args)
	{
		LogB.Information("on_run_encoder_load_cd_accepted");
		genericWin.Button_accept.Clicked -= new EventHandler(on_run_encoder_load_cd_accepted);

		on_run_encoder_load_set_pre (true);
	}
	private void on_run_encoder_load_set_pre (bool getPersonSessionFromGenericWin)
	{
		int uniqueID = genericWin.TreeviewSelectedRowID();
		radio_signal_analyze_current_set.Active = true;
		run_encoder_load_set (uniqueID, getPersonSessionFromGenericWin);
	}

	//this is also called from recalculate
	private void run_encoder_load_set (int uniqueID, bool getPersonSessionFromGenericWin)
	{
		if (uniqueID < 0)
			return;

		int personID = currentPerson.UniqueID;
		int sessionID = currentSession.UniqueID;

		if (genericWin != null && getPersonSessionFromGenericWin)
		{
//			if (genericWin.UseGridSessionPerson)
//			{
				personID = genericWin.GetPersonIDFromGui ();
				sessionID = genericWin.GetSessionIDFromGui ();
				//genericWin.UseGridSessionPerson = false;
//			}

			genericWin.HideAndNull();
		}

		//LogB.Information (string.Format ("run_encoder_load_set uniqueID: {0}, personID: {1}, sessionID: {2}",
		//			uniqueID, personID, sessionID));

		if (! getPersonSessionFromGenericWin) //is ab, need to update capture tab
			selectResultsSessionId (uniqueID, true);
		else //is cd
			runEncoderLoadSetDo (uniqueID, personID, sessionID, getPersonSessionFromGenericWin);
	}

	// at the moment we are not using Lambda like on forceSensorLoadSignalAcceptedDo ()
	// because we return an string here. Also loading Race Analyzer set is fast
	private void runEncoderLoadSetDo (int uniqueID, int personID, int sessionID, bool getPersonSessionFromGenericWin)
	{
		RunEncoder re = (RunEncoder) SqliteRunEncoder.Select (false, uniqueID, personID, sessionID)[0];

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

		List<string> contents_l = Util.ReadFileAsStringList(re.FullURL, "");
		LogB.Information("FullURL: " + re.FullURL);

		if (debugForceTest)
		{
			RunEncoderCaptureGetSpeedAndDisplacementTest recgsdt = new RunEncoderCaptureGetSpeedAndDisplacementTest ();
			contents_l = recgsdt.TestData_l;
		}

		if(contents_l.Count < 3)
		{
			new DialogMessage(Constants.MessageTypes.WARNING, Constants.FileEmptyStr());
			return;
		}

		//if start at 0 add a time 0 row (if it really does not start at 0)
		if (radio_race_analyzer_capture_graph_starts_0.Active)
		{
			string firstDataLine = contents_l[1]; //contents_l[0] are headers
			string [] strFull = firstDataLine.Split(new char[] {';'});
			if (Util.IsNumber (strFull[1], false) && Convert.ToInt32 (strFull[1]) > 0)
				contents_l.Insert (1, "strFull[0];0;strFull[2]");
		}

		signalSuperpose_2SetsCDPersonName = "";
		// trying on _cd to only update the graph
		//if (radio_ai_2sets.Active && radio_ai_cd.Active)
		if (genericWin != null && getPersonSessionFromGenericWin)
		{
			cairoGraphRaceAnalyzerPoints_st_CD_l = new List<PointF>();
			//cairoGraphRaceAnalyzerPoints_st_CD_l_timeShifted = false; //unused

			currentRunEncoder_CD = re;
			currentRunEncoderExercise_CD = SqliteRunEncoderExercise.Select (false, re.ExerciseID)[0];

			reCGSD_CD = run_encoder_load_set_reCGSD (contents_l, true, //two sets
					currentRunEncoderExercise_CD.SegmentCm,
					currentRunEncoderExercise_CD.SegmentVariableCm,
					SqlitePersonSession.SelectAttribute (false, re.PersonID, re.SessionID, Constants.Weight),
					re.Angle);

			if (personID != currentPerson.UniqueID)
				signalSuperpose_2SetsCDPersonName = SqlitePerson.SelectAttribute (personID, "name");

			if (reCGSD_CD.RunEncoderCaptureSpeedMax > 0)
				drawingarea_race_analyzer_capture_speed_time.QueueDraw ();

			runEncoderPrepareGraphAI ();

			button_ai_move_cd_pre_set_sensitivity ();

			return;
		} else {
			currentRunEncoder = re;
			lastRunEncoderFile = Util.RemoveExtension(re.Filename);
			lastRunEncoderFullPath = re.FullURL;

			assignCurrentRunEncoderExerciseFromSQL ();
			raceEncoderReadWidgets(); //needed to be able to do R graph

			//triggers
			triggerListRunEncoder = new TriggerList(
					SqliteTrigger.Select(
						false, Trigger.Modes.RACEANALYZER,
						Convert.ToInt32(currentRunEncoder.UniqueID))
					);
			showRaceAnalyzerTriggers ();

			// ---- capture tab graphs start ---->

			runEncoderShouldShowCaptureGraphsWithData = true;

			cairoGraphRaceAnalyzer_dt = null;
			cairoGraphRaceAnalyzer_st = null;
			cairoGraphRaceAnalyzer_at = null;
			cairoGraphRaceAnalyzerPoints_dt_l = new List<PointF>();
			cairoGraphRaceAnalyzerPoints_st_l = new List<PointF>();
			cairoGraphRaceAnalyzerPoints_st_Zoom_l = new List<PointF>();
			cairoGraphRaceAnalyzerPoints_st_CD_l = new List<PointF>();
			//cairoGraphRaceAnalyzerPoints_st_CD_l_timeShifted = false; //unused
			cairoGraphRaceAnalyzerPoints_st_Zoom_CD_l = new List<PointF>();
			cairoGraphRaceAnalyzerPoints_at_l = new List<PointF>();

			reCGSD = run_encoder_load_set_reCGSD (contents_l, false,
					currentRunEncoderExercise.SegmentCm, currentRunEncoderExercise.SegmentVariableCm,
					currentPersonSession.Weight, currentRunEncoder.Angle);

			if(reCGSD.RunEncoderCaptureSpeedMax > 0)
			{
				if(cairoRadial == null)
				{
					/*
					   if(radio_race_analyzer_capture_view_simple.Active)
					   cairoRadial = new CairoRadial(drawingarea_race_analyzer_capture_velocimeter_topleft, preferences.fontTypeToGraph());
					   else */
					cairoRadial = new CairoRadial(drawingarea_race_analyzer_capture_velocimeter_bottom, preferences.fontTypeToGraph());
				}

				cairoRadial.GraphSpeedMaxAndDistance(reCGSD.RunEncoderCaptureSpeedMax, reCGSD.RunEncoderCaptureDistance);

				drawingarea_race_analyzer_capture_position_time.QueueDraw ();
				drawingarea_race_analyzer_capture_speed_time.QueueDraw ();
				drawingarea_race_analyzer_capture_accel_time.QueueDraw ();
			}

			runEncoderPrepareGraphAI ();

			/*
			//debug reCGSD.SegmentCalcs
			if (reCGSD != null && reCGSD.SegmentCalcs != null)
			LogB.Information (reCGSD.SegmentCalcs.ToString ());
			*/

			// <---- capture tab graphs end ----

			//on load do the R graph, but not on capture, to show on capture the label related to lack of person height
			//raceEncoderCopyToTempAndDoRGraph();
			//no do not do it automatically, just make user click on analyze button
			//also showing that graph while analyze tab has not shown first time is buggy

			button_video_play_this_test.Sensitive = (re.VideoURL != "");
			sensitiveSelectedTestButtons (true);

			image_ai_model_graph.Visible = false;
			check_ai_chained_hscales.Sensitive = true;
			button_ai_ab_options.Sensitive = true;
			button_ai_model.Sensitive = true;
			button_ai_model_options_close_and_analyze.Sensitive = true;
			button_ai_model_save_image.Sensitive = true;

			if (radio_ai_2sets.Active)
				radio_ai_cd.Sensitive = true;
		}
	}

	private int runEncoderShiftedMicros;
	private RunEncoderCaptureGetSpeedAndDisplacement run_encoder_load_set_reCGSD (
			List<string> contents_l, bool twoSets,
			int segmentCm, List<int> segmentVariableCm, double personWeight, int angle)
	{
		RunEncoderCaptureGetSpeedAndDisplacement my_reCGSD = new RunEncoderCaptureGetSpeedAndDisplacement (
			segmentCm, segmentVariableCm, personWeight, angle);

		//to measure accel (having 3 samples)
		//TODO: store on reCGSD
		double speedPre2 = -1;
		double timePre2 = -1;
		double speedPre = -1;
		double timePre = -1;
		double accel = -1;
		bool enoughAccelFound = false; //accel has been > preferences.runEncoderMinAccel (default 10ms^2)
		runEncoderShiftedMicros = 0;
		bool signalShifted = false; //shifted on trigger0 or accel >= minAccel, whatever is first
		//do not shift (mark already shifted) if radio: starts at 0s
		if (radio_race_analyzer_capture_graph_starts_0.Active)
			signalShifted = true;

		string rowPre = "";

		bool firstRow = true;
		int firstTime = 0;
		//store data on cairoGraphRaceAnalyzerPoints_dt_l, ...st_l, ...at_l
		foreach(string row in contents_l)
		{
			if (firstRow) //is this useful at all? because the timePre will be also -1 on the 4th row
			{
				firstRow = false;

				string [] timeArray = row.Split(new char[] {';'});
				if (timeArray.Length > 0 && Util.IsNumber (timeArray[1], false))
					firstTime = Convert.ToInt32 (timeArray[1]);

				//LogB.Information (string.Format ("at firstRow, row: {0}, firstTime: {1}",
				//			row, firstTime));

				continue;
			}

			if (my_reCGSD.PassLoadedRow (row))
				my_reCGSD.Calcule (false);

			speedPre2 = speedPre;
			timePre2 = timePre;
			speedPre = my_reCGSD.RunEncoderCaptureSpeed;
			timePre = UtilAll.DivideSafe (my_reCGSD.Time, 1000000);

			/*
			LogB.Information ( string.Format ("my_reCGSD.RunEncoderCaptureSpeed: {0}, speedPre2: {1}," +
						"UtilAll.DivideSafe(my_reCGSD.Time, 1000000): {2}, timePre2: {3}",
						my_reCGSD.RunEncoderCaptureSpeed, speedPre2,
						UtilAll.DivideSafe(my_reCGSD.Time, 1000000), timePre2));
						*/

			if(speedPre2 > 0)
			{
				accel = UtilAll.DivideSafe(my_reCGSD.RunEncoderCaptureSpeed - speedPre2,
								UtilAll.DivideSafe(my_reCGSD.Time, 1000000) - timePre2);

				//LogB.Information (string.Format ("accel: {0}", accel));

				int timeNow = 0;
				string [] cells = row.Split(new char[] {';'});
				timeNow = Convert.ToInt32(cells[1]);

				if (! signalShifted)
				{
					/*
					   pass when the first trigger is done,
					   because first time will be on first trigger or when accel >= minAccel
					   whatever is first.
					   We need timeAtTrigger0 and timeAtEnoughAccel,
					   because if timeAtTrigger0 is first, display also timeAtEnougAccel
					 */
					bool shiftNow = false;
					int shiftTo = 0;

					if (! twoSets)
					{
						if (triggerListRunEncoder != null && triggerListRunEncoder.Count() > 0 &&
								triggerListRunEncoder.GetList()[0].Us < timeNow)
						{
							//my_reCGSD.SetTimeAtTrigger0 (triggerListRunEncoder.GetList()[0].Us);
							shiftTo = triggerListRunEncoder.GetList()[0].Us;
							shiftNow = true;
						}
					}

					if (accel >= preferences.runEncoderMinAccel && ! enoughAccelFound)
					{
						if (cells.Length == 3 && Util.IsNumber(cells[0], false) && Util.IsNumber(cells[1], false))
						{
							shiftTo = timeNow;
							shiftNow = true;
							enoughAccelFound = true;
						}
					}

					if (shiftNow)
					{
						//recreate rcCGSD object since now
						my_reCGSD = new RunEncoderCaptureGetSpeedAndDisplacement(
							segmentCm, segmentVariableCm, personWeight, angle);

						my_reCGSD.SetTimeAtEnoughAccelOrTrigger0 (shiftTo);

						LogB.Information ("load_set shiftNow with row: " + row);
						string [] timeArray = row.Split(new char[] {';'});
						if (timeArray.Length > 0 && Util.IsNumber (timeArray[1], false))
							runEncoderShiftedMicros = Convert.ToInt32 (timeArray[1]) - firstTime;

						if (my_reCGSD.PassLoadedRow (row))
							my_reCGSD.CalculeSpeedAt0Shifted (rowPre, row);
						//LogB.Information(string.Format("after row runEncoderCaptureSpeed: {0}", my_reCGSD.RunEncoderCaptureSpeed));

						signalShifted = true;
						timePre = 0;
					}
				} else {
					/*
					   if first was trigger then accel >= minAccel,
					   we need to use the my_reCGSD.SetTimeAtEnoughAccelMark () to show a line
					   */
					if (accel >= preferences.runEncoderMinAccel && ! enoughAccelFound)
					{
						my_reCGSD.SetTimeAtEnoughAccelMark (timeNow);
						enoughAccelFound = true;
					}
				}
			}

			if(signalShifted)
			{
				cairoGraphRaceAnalyzerPoints_st_CD_l.Add(new PointF(
							UtilAll.DivideSafe(my_reCGSD.Time, 1000000),
							my_reCGSD.RunEncoderCaptureSpeed));

				if (! twoSets)
				{
					cairoGraphRaceAnalyzerPoints_dt_l.Add(new PointF(
								UtilAll.DivideSafe(my_reCGSD.Time, 1000000),
								my_reCGSD.RunEncoderCaptureDistance));

					cairoGraphRaceAnalyzerPoints_st_l.Add(new PointF(
								UtilAll.DivideSafe(my_reCGSD.Time, 1000000),
								my_reCGSD.RunEncoderCaptureSpeed));

					cairoGraphRaceAnalyzerPoints_at_l.Add(new PointF(
								UtilAll.DivideSafe(my_reCGSD.Time, 1000000),
								accel));
				}
			}

			rowPre = row;
		}

		my_reCGSD.CalculeBestSecond ();

		if (getSmoothFrom_gui_at_race_analyzer_capture_smooth_graphs () > 0)
			my_reCGSD.SegmentsRedoWithSmoothing (
					getSmoothFrom_gui_at_race_analyzer_capture_smooth_graphs ());

		return my_reCGSD;
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
		string description = Util.RemoveTildeAndColonAndDot (genericWin.EntryEditRow);
		if(description != re.Description)
		{
			re.Description = description;
			re.UpdateSQLJustComments(true);

			//update treeview
			genericWin.on_edit_selected_done_update_treeview();
		}

		//3) change the session param and the url of signal and curves (if any)
		string idName = genericWin.GetComboEditSelected;
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

		pre_fillTreeView_resultsSession ();
	}

	private void runEncoderDeleteTestDo(RunEncoder re)
	{
		//int uniqueID = currentRunEncoder.UniqueID;
		SqliteRunEncoder.DeleteSQLAndFiles (false, re); //deletes also the .csv
	}

	// --- end of runEncoderDeleteTest stuff -------

	private void run_encoder_recalculate ()
	{
		// 1) check file is ok
		if (! Util.FileExists (lastRunEncoderFullPath)) //maybe in the future use currentRunEncoder.FullURL
		{
			new DialogMessage(Constants.MessageTypes.WARNING, Constants.FileNotFoundStr());
			return;
		}

		// 2) assign currentRunEncoderExercise & other variables
		assignCurrentRunEncoderExerciseFromSQL ();
		currentRunEncoder.ExerciseName = currentRunEncoderExercise.Name; //just in case

		// 3) update MaxSpeed, MaxAvgSpeed1s
		currentRunEncoder.MaxSpeed = 0;
		currentRunEncoder.MaxAvgSpeed1s = 0;

		List<string> contents_l = Util.ReadFileAsStringList (currentRunEncoder.FullURL, "");
		reCGSD = run_encoder_load_set_reCGSD (contents_l, false,
				currentRunEncoderExercise.SegmentCm, currentRunEncoderExercise.SegmentVariableCm,
				currentPersonSession.Weight, currentRunEncoder.Angle);

		if (reCGSD.RunEncoderCaptureSpeedMax > 0)
		{
			currentRunEncoder.MaxSpeed = reCGSD.RunEncoderCaptureSpeedMax;
			if (reCGSD.Miw.Error == "")
				currentRunEncoder.MaxAvgSpeed1s = reCGSD.Miw.Max;
		}

		// 4) update graphs and treeview
		currentRunEncoder.UpdateSQL(false);
		int id = currentRunEncoder.UniqueID; //need to assign to a variable because will change on pre_fillTreeView_resultsSession

		run_encoder_load_set (currentRunEncoder.UniqueID, false);

		pre_fillTreeView_resultsSession ();
		selectResultsSessionId (id, true);
	}

	private void on_radio_race_analyzer_capture_graph_starts_clicked (object o, EventArgs args)
	{
		if (currentRunEncoder != null)
			run_encoder_load_set (currentRunEncoder.UniqueID, false);
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
		//notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.RACEENCODER);
		//radio_mode_contacts_analyze.Active = true;
		check_ai_chained_hscales.Sensitive = true;
		button_ai_ab_options.Sensitive = true;
		button_ai_model.Sensitive = true;

		// 3) display table
		treeview_raceAnalyzer = UtilGtk.RemoveColumns(treeview_raceAnalyzer);

		string contents = Util.ReadFile(RunEncoder.GetCSVResultsURL(), false);

		/*
		   maybe captured data was too low or two different than an sprint.
		   Then we have image but maybe we have no sprintResults.csv
		   Length < 10 is written because on a model too short R can just return ""
		   */
		if(contents == null || contents == "" || contents.Length < 10)
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

		int imageWidth = UtilGtk.WidgetWidth (viewport_ai_model_graph);
		int imageHeight = UtilGtk.WidgetHeight (viewport_ai_model_graph);

		//create graph
		RunEncoderGraph reg = new RunEncoderGraph(
				race_analyzer_distance,
				//race_analyzer_angle, 		//TODO: unused
				currentPersonSession.Weight,  	//TODO: can be more if extra weight
				currentPersonSession.Height,
				race_analyzer_temperature,
				race_analyzer_device,
				currentRunEncoderExercise, //TODO: do not let capture if there's no exercise
				Util.ChangeSpaceAndMinusForUnderscore(currentPerson.Name),
				Util.ChangeSpaceAndMinusForUnderscore(UtilGtk.ComboGetActive(combo_run_encoder_exercise)),
				currentRunEncoder.DateTimePublic,
				preferences.runEncoderMinAccel,
				Preferences.RunEncoderShouldPlotVariable(Preferences.RunEncoderPlotVariables.RAWACCEL),
				Preferences.RunEncoderShouldPlotVariable(Preferences.RunEncoderPlotVariables.FITTEDACCEL),
				Preferences.RunEncoderShouldPlotVariable(Preferences.RunEncoderPlotVariables.RAWFORCE),
				Preferences.RunEncoderShouldPlotVariable(Preferences.RunEncoderPlotVariables.FITTEDFORCE),
				Preferences.RunEncoderShouldPlotVariable(Preferences.RunEncoderPlotVariables.RAWPOWER),
				Preferences.RunEncoderShouldPlotVariable(Preferences.RunEncoderPlotVariables.FITTEDPOWER),
				triggerListRunEncoder);

		reg.CallR (imageWidth, imageHeight, true, out string errorStr);

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
		// 1) read the contents of the CSV
		RunEncoderCSV recsv = readRunEncoderCSVContents (contents);

		// 2) Add the columns to the treeview
		string [] columnsString = getTreeviewRaceAnalyzerHeaders(contents);
		int count = 0;
		foreach(string column in columnsString)
			treeview_raceAnalyzer.AppendColumn (column, new CellRendererText(), "text", count++);

		// 3) Add the TreeStore
		Type [] types = new Type [columnsString.Length];
		for (int i=0; i < columnsString.Length; i++) {
			types[i] = typeof (string);
		}
		TreeStore store = new TreeStore(types);

		store.AppendValues (recsv.ToTreeView());

		// 4) Assing model to store and other tweaks
		treeview_raceAnalyzer.Model = store;
		treeview_raceAnalyzer.Selection.Mode = SelectionMode.None;
                treeview_raceAnalyzer.HeadersVisible=true;
	}

	private string [] getTreeviewRaceAnalyzerHeaders (string contents)
        {
		// 1) check how many dist columns we should add
		List<string> dist_l = new List<string> ();
		using (StringReader reader = new StringReader (contents))
		{
			string line = reader.ReadLine ();      //headers
			LogB.Information(line);
			if (line != null)
			{
				string [] cells = line.Split(new char[] {','});
				dist_l = new List<string> ();
				for (int i = 26; i < cells.Length; i ++) //Attention!: take care with this 26 if in the future add more columns before dist/times
				{
					//each string comes as "X0Y25.5m_Speed" convert to 0-25.5 m\nSpeed or 0-25,5 m/nSpeed
					//since 2023 Jun 8 it comes as: "X0to25.5m_Speed" convert to 0-25.5 m\nSpeed or 0-25,5 m/nSpeed
					//We need the X because R cannot start column name with a number
					//We need the "to" because if we use a -, R converts the - to .
					//We use a "to" because when R prints the spreadsheet directly is better that than eg. an Y
					string temp = Util.RemoveChar (cells[i], '"', false);
					temp = Util.RemoveChar (temp, 'X', false); //X is needed because R cannot start column name with a number
					temp = Util.ChangeChars (temp, "to", "-");
					temp = Util.ChangeDecimalSeparator (temp);
					temp = Util.ChangeChars (temp, "_", "\n");

					dist_l.Add (temp);
				}
			}
		}

		// 2) prepare the headers
                string [] headers = {
			"Mass\n\n(kg)", "Height\n\n(m)", "Temperature\n\n(ºC)",
			"V (wind)\n\n(m/s)", "Ka\n\n", "K\nfitted\n(s^-1)",
			"Vmax\nfitted\n(m/s)", "Amax\nfitted\n(m/s^2)", "Fmax\nfitted\n(N)",
			"Fmax\nrel fitted\n(N/kg)", "Sfv\nfitted\n", "Sfv\nrel fitted\n",
			"Sfv\nlm\n", "Sfv\nrel lm\n", "Pmax\nfitted\n(W)",
			"Pmax\nrel fitted\n(W/kg)", "Time to pmax\nfitted\n(s)", "F0\n\n(N)",
			"F0\nrel\n(N/kg)", "V0\n\n(m/s)", "Pmax\nlm\n(W)",
			"Pmax\nrel lm\n(W/kg)",
			"Vmax\nraw\n(m/s)", "Amax\nraw\n(m/s^2)", "Fmax\nraw\n(N)", "Pmax\nraw\n(W)"
		};

		// 3) add the dists to the headers
		headers = Util.AddToArrayString (headers, dist_l);

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
			do {
				line = reader.ReadLine ();
				LogB.Information(line);
				if (line == null)
					break;

				string [] cells = line.Split(new char[] {','});

				// get the times (total columns can be different each time)
				List<double> time_l = new List<double> ();
				for (int i = 26; i < cells.Length; i ++) //Attention! take care with this 26 if in the future add more columns before dist/times
					time_l.Add (Convert.ToDouble (Util.CDS (cells[i])));

				recsv = new RunEncoderCSV (
						Convert.ToDouble (Util.CDS (cells[0])),
						Convert.ToDouble (Util.CDS (cells[1])),
						Convert.ToInt32 (cells[2]),
						Convert.ToDouble (Util.CDS (cells[3])),
						Convert.ToDouble (Util.CDS (cells[4])),
						Convert.ToDouble (Util.CDS (cells[5])),
						Convert.ToDouble (Util.CDS (cells[6])),
						Convert.ToDouble (Util.CDS (cells[7])),
						Convert.ToDouble (Util.CDS (cells[8])),
						Convert.ToDouble (Util.CDS (cells[9])),
						Convert.ToDouble (Util.CDS (cells[10])),
						Convert.ToDouble (Util.CDS (cells[11])),
						Convert.ToDouble (Util.CDS (cells[12])),
						Convert.ToDouble (Util.CDS (cells[13])),
						Convert.ToDouble (Util.CDS (cells[14])),
						Convert.ToDouble (Util.CDS (cells[15])),
						Convert.ToDouble (Util.CDS (cells[16])),
						Convert.ToDouble (Util.CDS (cells[17])),
						Convert.ToDouble (Util.CDS (cells[18])),
						Convert.ToDouble (Util.CDS (cells[19])),
						Convert.ToDouble (Util.CDS (cells[20])),
						Convert.ToDouble (Util.CDS (cells[21])),
						Convert.ToDouble (Util.CDS (cells[22])),
						Convert.ToDouble (Util.CDS (cells[23])), //vmax raw, amax raw
						Convert.ToDouble (Util.CDS (cells[24])),
						Convert.ToDouble (Util.CDS (cells[25])), //fmax raw, pmax raw
						time_l
						);
			} while(true);
		}

		return recsv;
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
			blinkCapture.End ();
			showHideBlinkIcon (blinkCapture, false);

			button_video_play_this_test.Sensitive = false;

			if(runEncoderProcessFinish)
			{
				if (webcamStatusEnum == WebcamStatusEnum.RECORDING)
				{
					//LogB.Information ("webcam will end now (gtk thread) at: " +
					//		DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff"));

					webcamEndingRecordingStop ();

					Thread.Sleep (50); //Wait
					return true;
				}
				else if (webcamStatusEnum == WebcamStatusEnum.STOPPING)
				{
					bool success = webcamEndingRecordingStopDo ();
					if (! success)
						return true;
				}

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
					if (Config.SimulatedCapture)
						event_execute_label_message.Text += " SIMULATED TEST!";

					double maxSpeed = 0;
					double maxAvgSpeed1s = 0;
					if (reCGSD != null && reCGSD.RunEncoderCaptureSpeedMax > 0)
					{
						maxSpeed = reCGSD.RunEncoderCaptureSpeedMax;
						if (reCGSD.Miw.Error == "")
							maxAvgSpeed1s = reCGSD.Miw.Max;
					}

					currentRunEncoder = new RunEncoder(-1, currentPerson.UniqueID, currentSession.UniqueID,
							currentRunEncoderExercise.UniqueID, raceEncoderGetDeviceFromGui (),
							Convert.ToInt32(race_analyzer_spinbutton_distance.Value),
							Convert.ToInt32(race_analyzer_spinbutton_temperature.Value),
							Util.GetLastPartOfPath(lastRunEncoderFile + ".csv"), //filename
							Util.MakeURLrelative(Util.GetRunEncoderSessionDir(currentSession.UniqueID)), //url
							UtilDate.ToFile(runEncoderTimeStartCapture),
							"", //on capture cannot store comment (comment has to be written after),
							"", //videoURL
							Convert.ToInt32(race_analyzer_spinbutton_angle.Value),
							runEncoderTotalTime,
							maxSpeed, maxAvgSpeed1s,
							currentRunEncoderExercise.Name);

					currentRunEncoder.UniqueID = currentRunEncoder.InsertSQL(false);
					triggerListRunEncoder.SQLInsert(currentRunEncoder.UniqueID);
					showRaceAnalyzerTriggers ();
					updatePersonTestsN (false);

					if (radio_ai_2sets.Active)
						radio_ai_cd.Sensitive = true;

					string videoStr = "";
					if (webcamStatusEnum == WebcamStatusEnum.STOPPED)
					{
						bool success = webcamEndingSaveFile (Constants.TestTypes.RACEANALYZER, currentRunEncoder.UniqueID);
						if (success)
						{
							//add the videoURL to SQL
							currentRunEncoder.VideoURL = Util.GetVideoFileName(currentSession.UniqueID,
									Constants.TestTypes.RACEANALYZER,
									currentRunEncoder.UniqueID);
							currentRunEncoder.UpdateSQL(false);
							videoStr = string.Format ("{0}-{1}", Constants.TestTypes.RACEANALYZER,
									currentRunEncoder.UniqueID);
						}
						webcamRestoreGui (success);
					}

					updateGraphRunEncoderBars();
					treeViewResultsSession.Add (currentPerson.UniqueID, currentPerson.Name, currentRunEncoder, videoStr);
					Thread.Sleep (250); //Wait a bit to ensure is copied
					sensitiveSelectedTestButtons (true);
					contactsShowCaptureDoingButtons(false);

					//do not analyze after capture, to be able to show the message: no person height
					/*
					raceEncoderCaptureRGraphDo();

					runEncoderAnalyzeOpenImage();
					notebook_analyze.CurrentPage = Convert.ToInt32(notebook_analyze_pages.RACEENCODER);
					radio_mode_contacts_analyze.Active = true;
					*/

					button_ai_model_options_close_and_analyze.Sensitive = true;
					check_ai_chained_hscales.Sensitive = true;
					button_ai_ab_options.Sensitive = true;
					button_ai_model.Sensitive = true;
					button_ai_model_save_image.Sensitive = true;

					drawingarea_race_analyzer_capture_position_time.QueueDraw ();
					drawingarea_race_analyzer_capture_speed_time.QueueDraw ();
					drawingarea_race_analyzer_capture_accel_time.QueueDraw ();

					runEncoderPrepareGraphAI ();
				}
				LogB.Information(" re C finish 2");
			} else if(runEncoderProcessCancel || runEncoderProcessError)
			{
				LogB.Information(" re C cancel ");

				if (runEncoderProcessError)
					Util.PlaySound (Constants.SoundTypes.BAD, preferences.volumeOn, preferences.gstreamer);

				//stop the camera (and do not save)
				if (webcamStatusEnum == WebcamStatusEnum.RECORDING)
				{
					webcamEndingRecordingCancel ();
					webcamRestoreGui (false);
				}

				sensitiveSelectedTestButtons (false);
				contactsShowCaptureDoingButtons(false);
				button_ai_model_options_close_and_analyze.Sensitive = false;
				check_ai_chained_hscales.Sensitive = false;
				button_ai_ab_options.Sensitive = false;
				button_ai_model.Sensitive = false;
				button_ai_model_save_image.Sensitive = false;
				sensitiveSelectedTestButtons (false);

				if(runEncoderProcessCancel)
					event_execute_label_message.Text = "Cancelled.";
				else {
					event_execute_label_message.Text = runEncoderNotConnectedString;
					button_detect_show_hide (true); // show the detect big button
				}

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

			if(reCGSD != null && cairoRadial != null)
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
			radio_signal_analyze_current_set.Active = true;

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
			{
				//cairoRadial.GraphSpeedAndDistance(reCGSD.RunEncoderCaptureSpeed, reCGSD.RunEncoderCaptureDistance);
				/*
				if(radio_race_analyzer_capture_view_simple.Active)
					drawingarea_race_analyzer_capture_velocimeter_topleft.QueueDraw ();
				else */
					drawingarea_race_analyzer_capture_velocimeter_bottom.QueueDraw ();
			}

			//TODO: activate again when there's a real time update (not repaint all) method
			//false: it will not be redrawn if there are no new points
			/*
			updateRaceAnalyzerCapturePositionTime(false);
			updateRaceAnalyzerCaptureSpeedTime(false);
			updateRaceAnalyzerCaptureAccelTime(false);
			*/
			//TODO: this should be false
			drawingarea_race_analyzer_capture_position_time.QueueDraw ();
			drawingarea_race_analyzer_capture_speed_time.QueueDraw ();
			drawingarea_race_analyzer_capture_accel_time.QueueDraw ();

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

			if (blinkCapture.Status == Blink.StatusEnum.NOTSTARTED)
				blinkCapture.Start ();
			showHideBlinkIcon (blinkCapture, true);

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
		image_ai_model_graph = UtilGtk.OpenImageSafe(
				imagePath,
				image_ai_model_graph);

		//image_ai_model_graph.Sensitive = true; //not useful
		image_ai_model_graph.Visible = true;
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

	private void createRaceAnalyzerDeviceCombo ()
	{
		combo_race_analyzer_device = UtilGtk.CreateComboBoxText (
				box_combo_race_analyzer_device,
				new List<string> { RunEncoder.DevicesStringMANUAL, RunEncoder.DevicesStringRESISTED },
				RunEncoder.DevicesStringMANUAL);

		combo_race_analyzer_device.Changed += new EventHandler (on_combo_race_analyzer_device_changed);
	}

	// -------------------------------- exercise stuff --------------------


	string [] runEncoderComboExercisesString; //id:name (no translations, use user language)

	private void createRunEncoderExerciseCombo ()
	{
		//run_encoder_exercise

		combo_run_encoder_exercise = new ComboBoxText ();
		fillRunEncoderExerciseCombo("");

		combo_run_encoder_exercise.Changed += new EventHandler (on_combo_run_encoder_exercise_changed);
		hbox_combo_run_encoder_exercise.PackStart(combo_run_encoder_exercise, true, true, 0);
		hbox_combo_run_encoder_exercise.ShowAll();

		//needed because the += EventHandler does not work on first fill of the combo
		on_combo_run_encoder_exercise_changed (new object (), new EventArgs ());
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
		//ComboBoxText combo = o as ComboboxText;
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

		RunEncoderExercise exTemp = SqliteRunEncoderExercise.Select (
                                false, getExerciseIDFromAnyCombo(combo_run_encoder_exercise, runEncoderComboExercisesString, false))[0];
		race_analyzer_spinbutton_angle.Value = exTemp.AngleDefault;

		//sensitivity of left/right buttons
		button_combo_run_encoder_exercise_capture_left.Sensitive = (combo_run_encoder_exercise.Active > 0);
		button_combo_run_encoder_exercise_capture_right.Sensitive = ! UtilGtk.ComboSelectedIsLast(combo_run_encoder_exercise);
		button_combo_select_contacts_top_left.Sensitive = (combo_run_encoder_exercise.Active > 0);
		button_combo_select_contacts_top_right.Sensitive = ! UtilGtk.ComboSelectedIsLast(combo_run_encoder_exercise);

		// unselect in order to use combo value on PrepareEventGraph type
		if (treeViewResultsSession != null)
			treeViewResultsSession.Unselect ();

		radio_contacts_graph_currentTest.Label = exTemp.Name;
                //update the treeview
                pre_fillTreeView_resultsSession ();
	}

	private void fillRunEncoderExerciseCombo(string name)
	{
		List<RunEncoderExercise> runEncoderExercises_l = SqliteRunEncoderExercise.Select (false, -1);
		if(runEncoderExercises_l.Count == 0)
		{
			runEncoderComboExercisesString = new String [0];
			UtilGtk.ComboUpdate(combo_run_encoder_exercise, new String[0], "");

			return;
		}

		runEncoderComboExercisesString = new String [runEncoderExercises_l.Count];
		string [] exerciseNamesToCombo = new String [runEncoderExercises_l.Count];
		int i =0;
		foreach(RunEncoderExercise ex in runEncoderExercises_l)
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
		//we need the 2nd check because without is, on import if we are on other mode, top combo could have been updated with this mode exercises
		if(combo_select_contacts_top != null && current_mode == Constants.Modes.RUNSENCODER)
		{
			comboSelectContactsTopNoFollow = true;
			UtilGtk.ComboUpdate(combo_select_contacts_top,
					UtilGtk.ComboGetValues (combo_run_encoder_exercise), "");
			combo_select_contacts_top.Active = combo_run_encoder_exercise.Active;
			comboSelectContactsTopNoFollow = false;
		}
	}

	void on_button_run_encoder_exercise_edit_clicked (object o, EventArgs args)
	{
		if(UtilGtk.ComboGetActive(combo_run_encoder_exercise) == "")
		{
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Need to create/select an exercise."));
			return;
		}

		RunEncoderExercise ex = SqliteRunEncoderExercise.Select (
                                false, getExerciseIDFromAnyCombo(combo_run_encoder_exercise, runEncoderComboExercisesString, false))[0];
		LogB.Information("selected exercise: " + ex.ToString());

		show_contacts_exercise_add_edit (false);
		entry_run_encoder_exercise_name.Text = ex.Name;
		entry_run_encoder_exercise_description.Text = ex.Description;
		check_run_encoder_exercise_is_sprint.Active = ex.IsSprint;
		spin_run_encoder_exercise_angle_default.Value = ex.AngleDefault;

		if(v_segments_size_cm_l == null)
			spin_race_encoder_exercise_v_segment_size_cm_create_list ();

		if(ex.SegmentCm < 0)
		{
			check_run_encoder_exercise_fixed_size.Active = false;
			spin_race_encoder_exercise_v_segments_num.Value = ex.SegmentVariableCm.Count;

			int i = 0;
			foreach(int cm in ex.SegmentVariableCm)
			{
				( (Gtk.SpinButton) v_segments_size_cm_l[i]).Value = ex.SegmentVariableCm[i];
				i ++;
			}
		} else {
			check_run_encoder_exercise_fixed_size.Active = true;
			spin_race_encoder_exercise_f_segment_size_cm.Value = ex.SegmentCm;
		}
		//force managing:
		on_check_run_encoder_exercise_fixed_size_toggled (new object (), new EventArgs ());
	}

	void on_button_run_encoder_exercise_add_clicked (object o, EventArgs args)
	{
		show_contacts_exercise_add_edit (true);

		entry_run_encoder_exercise_name.Text = "";
		entry_run_encoder_exercise_description.Text = "";
		check_run_encoder_exercise_is_sprint.Active = true;
		spin_run_encoder_exercise_angle_default.Value = 0;
		spin_race_encoder_exercise_f_segment_size_cm.Value = RunEncoderExercise.SegmentCmDefault;
		spin_race_encoder_exercise_v_segments_num.Value = 2;

		if(v_segments_size_cm_l == null)
			spin_race_encoder_exercise_v_segment_size_cm_create_list ();

		spin_race_encoder_exercise_v_segment_size_cm_reset_list (); //put default values;

		check_run_encoder_exercise_fixed_size.Active = true;
		//force managing:
		on_check_run_encoder_exercise_fixed_size_toggled (new object (), new EventArgs ());
	}

	private void on_button_run_encoder_exercise_is_sprint_help_clicked (object o, EventArgs args)
	{
		new DialogMessage(Constants.MessageTypes.INFO,
				Catalog.GetString("In a sprint exercise, maximum acceleration is performed at the beginning and maximum sustained speed at a later time."));
		return;
	}

	private void on_button_run_encoder_exercise_angle_default_help_clicked (object o, EventArgs args)
	{
		new DialogMessage(Constants.MessageTypes.INFO,
				Catalog.GetString("Default angle of this exercise.") + "\n" +
				Catalog.GetString("90 means go vertically up.") + "\n" +
				Catalog.GetString("-90 means go vertically down.") + "\n" +
				string.Format(Catalog.GetString("Possible range goes from {0} to {1}."), -90, 90));
		return;
	}
	private void on_check_run_encoder_exercise_fixed_size_toggled (object o, EventArgs args)
	{
		if(check_run_encoder_exercise_fixed_size.Active)
		{
			hbox_run_encoder_exercise_fixed_segments_size.Visible = true;
			hbox_run_encoder_exercise_notfixed_segment_num.Visible = false;
			frame_run_encoder_exercise_notfixed_segments.Visible = false;
		} else {
			hbox_run_encoder_exercise_fixed_segments_size.Visible = false;
			hbox_run_encoder_exercise_notfixed_segment_num.Visible = true;
			frame_run_encoder_exercise_notfixed_segments.Visible = true;

			spin_race_encoder_exercise_v_segment_size_cm_show_needed ();
		}
	}

	private void on_spin_race_encoder_exercise_v_segments_num_value_changed (object o, EventArgs args)
	{
		spin_race_encoder_exercise_v_segment_size_cm_show_needed ();
	}

	List<Gtk.SpinButton> v_segments_size_cm_l;
	private void spin_race_encoder_exercise_v_segment_size_cm_create_list ()
	{
		v_segments_size_cm_l = new List<Gtk.SpinButton>();

		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_0);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_1);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_2);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_3);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_4);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_5);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_6);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_7);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_8);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_9);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_10);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_11);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_12);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_13);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_14);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_15);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_16);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_17);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_18);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_19);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_20);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_21);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_22);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_23);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_24);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_25);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_26);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_27);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_28);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_29);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_30);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_31);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_32);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_33);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_34);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_35);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_36);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_37);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_38);
		v_segments_size_cm_l.Add(spin_race_encoder_exercise_v_segment_size_cm_39);
	}

	private void spin_race_encoder_exercise_v_segment_size_cm_reset_list ()
	{
		foreach(Gtk.SpinButton sb in v_segments_size_cm_l)
			sb.Value = 100;
	}

	private void spin_race_encoder_exercise_v_segment_size_cm_show_needed ()
	{
		int toShow = Convert.ToInt32(spin_race_encoder_exercise_v_segments_num.Value);

		int count = 0;
		foreach(Gtk.SpinButton sb in v_segments_size_cm_l)
		{
			sb.Visible = (count < toShow);
			count ++;
		}
	}

	private bool run_encoder_exercise_do_add_or_edit (bool adding)
	{
		string name = Util.MakeValidSQLAndFileName(Util.RemoveTildeAndColonAndDot(entry_run_encoder_exercise_name.Text));
		name = Util.RemoveChar(name, '"');
		label_contacts_exercise_error.Text = "";

		if(adding)
			LogB.Information("run_encoder_exercise_do - Trying to insert: " + name);
		else
			LogB.Information("run_encoder_exercise_do - Trying to edit: " + name);

		if(name == "")
		{
			label_contacts_exercise_error.Text = Catalog.GetString("Error: Missing name of exercise.");
			return false;
		}
		else if (adding && Sqlite.Exists(false, Constants.RunEncoderExerciseTable, name))
		{
			label_contacts_exercise_error.Text = string.Format(Catalog.GetString(
							"Error: An exercise named '{0}' already exists."), name);
			return false;
		}
		else if (! adding) //if we are editing
		{
			//if we edit, check that this name does not exists (on other exercise, on current editing exercise is obviously fine)
			int getIdOfThis = Sqlite.ExistsAndGetUniqueID(false, Constants.RunEncoderExerciseTable, name); //if not exists will be -1
			if(getIdOfThis != -1 && getIdOfThis != getExerciseIDFromAnyCombo(combo_run_encoder_exercise, runEncoderComboExercisesString, false))
			{
				label_contacts_exercise_error.Text = string.Format(Catalog.GetString(
								"Error: An exercise named '{0}' already exists."), name);

				return false;
			}
		}

		int segmentCm = Convert.ToInt32(spin_race_encoder_exercise_f_segment_size_cm.Value);
		List<int> segmentVariableCm = new List<int>();
		if(! check_run_encoder_exercise_fixed_size.Active)
		{
			segmentCm = -1;

			int i = 0;
			foreach(Gtk.SpinButton sb in v_segments_size_cm_l)
			{
				if(i < spin_race_encoder_exercise_v_segments_num.Value)
					segmentVariableCm.Add(Convert.ToInt32(sb.Value));
				i ++;
			}
		}

		if(adding)
		{
			RunEncoderExercise ex = new RunEncoderExercise (
					-1, name, entry_run_encoder_exercise_description.Text,
					segmentCm, segmentVariableCm, check_run_encoder_exercise_is_sprint.Active,
					Convert.ToInt32(spin_run_encoder_exercise_angle_default.Value));
			ex.InsertSQL (false);
			currentRunEncoderExercise = ex;
		} else {
			RunEncoderExercise ex = new RunEncoderExercise(
					getExerciseIDFromAnyCombo(combo_run_encoder_exercise, runEncoderComboExercisesString, false),
					name, entry_run_encoder_exercise_description.Text,
					segmentCm, segmentVariableCm, check_run_encoder_exercise_is_sprint.Active,
					Convert.ToInt32(spin_run_encoder_exercise_angle_default.Value));

			SqliteRunEncoderExercise.Update(false, ex);
			currentRunEncoderExercise = ex;
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

		RunEncoderExercise ex = SqliteRunEncoderExercise.Select (
                                false, getExerciseIDFromAnyCombo(combo_run_encoder_exercise, runEncoderComboExercisesString, false))[0];

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
	private void on_drawingarea_race_analyzer_capture_velocimeter_cairo_draw (object o, Gtk.DrawnArgs args)
	{
		Gtk.DrawingArea da;
		/*
		if (o == (object) drawingarea_race_analyzer_capture_velocimeter_topleft)
			da = drawingarea_race_analyzer_capture_velocimeter_topleft;
		else */ if (o == (object) drawingarea_race_analyzer_capture_velocimeter_bottom)
			da = drawingarea_race_analyzer_capture_velocimeter_bottom;
		else
			return;

		if(cairoRadial == null)
			cairoRadial = new CairoRadial(da, preferences.fontTypeToGraph());

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
	private void on_drawingarea_race_analyzer_capture_position_time_cairo_draw (object o, Gtk.DrawnArgs args)
	{
		updateRaceAnalyzerCapturePositionTime(true);
	}

	CairoGraphRaceAnalyzer cairoGraphRaceAnalyzer_st;
	static List<PointF> cairoGraphRaceAnalyzerPoints_st_l;		//speed/time
	static List<PointF> cairoGraphRaceAnalyzerPoints_st_Zoom_l;	//speed/time
	static List<PointF> cairoGraphRaceAnalyzerPoints_st_CD_l;	//speed/time (signal can be same or other)
	//private bool cairoGraphRaceAnalyzerPoints_st_CD_l_timeShifted = false; //unused
	static List<PointF> cairoGraphRaceAnalyzerPoints_st_Zoom_CD_l;	//speed/time
	private void on_drawingarea_race_analyzer_capture_speed_time_cairo_draw (object o, Gtk.DrawnArgs args)
	{
		updateRaceAnalyzerCaptureSpeedTime(true);
	}

	CairoGraphRaceAnalyzer cairoGraphRaceAnalyzer_at;
	static List<PointF> cairoGraphRaceAnalyzerPoints_at_l;	//accel/time
	private void on_drawingarea_race_analyzer_capture_accel_time_cairo_draw (object o, Gtk.DrawnArgs args)
	{
		updateRaceAnalyzerCaptureAccelTime(true);
	}

	private void updateRaceAnalyzerCapturePositionTime(bool forceRedraw)
	{
		if(radio_race_analyzer_capture_view_simple.Active)
			return;

		bool isSprint = false;
		if(currentRunEncoderExercise != null && currentRunEncoderExercise.IsSprint)
			isSprint = true;

		RunEncoderSegmentCalcs segmentCalcs = new RunEncoderSegmentCalcs ();
		if(currentRunEncoderExercise != null && //currentRunEncoderExercise.SegmentCm > 0 &&
				reCGSD != null && reCGSD.SegmentCalcs != null)
			segmentCalcs = reCGSD.SegmentCalcs;

		int timeAtEnoughAccel = 0;
		int timeAtEnoughAccelMark = 0;
		GetMaxAvgInWindow miw = new GetMaxAvgInWindow ();
		if (reCGSD != null)
		{
			timeAtEnoughAccel = reCGSD.TimeAtEnoughAccelOrTrigger0; //to shift time at load, sent here to sync/delete triggers
			timeAtEnoughAccelMark = reCGSD.TimeAtEnoughAccelMark; //to show mark at capture
			miw = reCGSD.Miw;
		}

		if(cairoGraphRaceAnalyzer_dt == null || forceRedraw)
			cairoGraphRaceAnalyzer_dt = new CairoGraphRaceAnalyzer(
					false,
					drawingarea_race_analyzer_capture_position_time, "title",
					Catalog.GetString("Distance"), "m",
					isSprint, false, false,
					segmentCalcs,
					false, preferences.runEncoderCaptureShow,
					true);

		int smoothGui = getSmoothFrom_gui_at_race_analyzer_capture_smooth_graphs ();
		cairoGraphRaceAnalyzer_dt.DoSendingList (preferences.fontTypeToGraph(),
				cairoGraphRaceAnalyzerPoints_dt_l,
				null,
				new List<string> (),
				forceRedraw, 0,
				CairoXY.PlotTypes.LINES, smoothGui == 0, smoothGui, miw,
				triggerListRunEncoder, timeAtEnoughAccel,
				timeAtEnoughAccelMark, preferences.runEncoderMinAccel,
				-1, -1, -1, -1);
	}
	private void updateRaceAnalyzerCaptureSpeedTime(bool forceRedraw)
	{
		bool isSprint = false;
		if(currentRunEncoderExercise != null && currentRunEncoderExercise.IsSprint)
			isSprint = true;

		RunEncoderSegmentCalcs segmentCalcs = new RunEncoderSegmentCalcs ();

		/*
		LogB.Information (string.Format ("currentRunEncoderExercise != null: {0}, reCGSD != null: {1}",
					(currentRunEncoderExercise != null), (reCGSD != null)));
		if (reCGSD != null)
			LogB.Information (string.Format ("reCGSD.SegmentCalcs != null: {0}", (reCGSD.SegmentCalcs != null)));
			*/

		if(currentRunEncoderExercise != null && //currentRunEncoderExercise.SegmentCm > 0 &&
				reCGSD != null && reCGSD.SegmentCalcs != null)
			segmentCalcs = reCGSD.SegmentCalcs;

		int timeAtEnoughAccel = 0;
		int timeAtEnoughAccelMark = 0;
		GetMaxAvgInWindow miw = new GetMaxAvgInWindow ();
		if (reCGSD != null)
		{
			timeAtEnoughAccel = reCGSD.TimeAtEnoughAccelOrTrigger0; //to shift time at load, sent here to sync/delete triggers
			timeAtEnoughAccelMark = reCGSD.TimeAtEnoughAccelMark; //to show mark at capture
			miw = reCGSD.Miw;
		}

		// get distinct CD data && subtitleWithSetsInfo if needed
		List<PointF> cairoGraphSend_CD = null;
		List<string> subtitleWithSetsInfo_l = new List<string> ();
		if (notebook_capture_analyze.CurrentPage == 1 && radio_ai_2sets.Active)
		{
			cairoGraphSend_CD = cairoGraphRaceAnalyzerPoints_st_CD_l;
			if (AiVars.zoomApplied)
				cairoGraphSend_CD = cairoGraphRaceAnalyzerPoints_st_Zoom_CD_l;

			if (currentRunEncoder != null && currentRunEncoderExercise != null &&
					currentRunEncoder_CD != null && currentRunEncoderExercise_CD != null)
			{
				string abPersonName = "";
				string cdPersonName = "";
				if (signalSuperpose_2SetsCDPersonName != "")
				{
					abPersonName = currentPerson.Name + ", ";
					cdPersonName = signalSuperpose_2SetsCDPersonName + ", ";
				}

				subtitleWithSetsInfo_l.Add (string.Format ("AB: {0}{1}, {2}",
							abPersonName,
							currentRunEncoderExercise.Name,
							currentRunEncoder.DateTimePublic));

				subtitleWithSetsInfo_l.Add (string.Format ("CD: {0}{1}, {2}",
							cdPersonName,
							currentRunEncoderExercise_CD.Name,
							currentRunEncoder_CD.DateTimePublic));
			}
		}

		Gtk.DrawingArea da;
		bool plotSegmentBarsOnSpeed = false;
		if (notebook_capture_analyze.CurrentPage == 0)
			da = drawingarea_race_analyzer_capture_speed_time;
		else {
			da = ai_drawingarea_cairo;
			plotSegmentBarsOnSpeed = true; //only show on analyze
		}

		if(cairoGraphRaceAnalyzer_st == null || forceRedraw)
			cairoGraphRaceAnalyzer_st = new CairoGraphRaceAnalyzer(
					(currentRunEncoder != null && currentRunEncoder.UniqueID >= 0 && (runEncoderCaptureThread == null || ! runEncoderCaptureThread.IsAlive)),
					da, "title",
					Catalog.GetString("Speed"), "m/s",
					isSprint, true, true,
					segmentCalcs,
					plotSegmentBarsOnSpeed, preferences.runEncoderCaptureShow,
					false);

		int smoothGui = getSmoothFrom_gui_at_race_analyzer_capture_smooth_graphs ();

		int hscaleA = -1, hscaleB = -1;
		int hscaleC = -1, hscaleD = -1;

		List<PointF> sendPoints_l = cairoGraphRaceAnalyzerPoints_st_l;

		if (notebook_capture_analyze.CurrentPage == 1)
		{
			hscaleA = Convert.ToInt32 (hscale_ai_a.Value);
			hscaleB = Convert.ToInt32 (hscale_ai_b.Value);
			hscaleC = Convert.ToInt32 (hscale_ai_c.Value);
			hscaleD = Convert.ToInt32 (hscale_ai_d.Value);

			if (AiVars.zoomApplied)
				sendPoints_l = cairoGraphRaceAnalyzerPoints_st_Zoom_l;
		}

		double videoTime = 0;
		if (webcamPlay != null && webcamPlay.PlayVideoGetSecond > 0)
		{
			videoTime = webcamPlay.PlayVideoGetSecond -diffVideoVsSignal;
			if (sendPoints_l.Count > 0)
			{
				//TODO: think if this is needed. In show at 0s as it starts at 0 should not be needed
				if (runEncoderShiftedMicros == 0)
					videoTime += sendPoints_l[0].X; //TODO: això segurament no s'hauria de tenir en compte quan es fa shift o potser s'hauria de usar el runEncoderShiftedMicros
				else
					videoTime -= UtilAll.DivideSafe (runEncoderShiftedMicros, 1000000.0);
			}
			//maybe at end there are no pulses. So to sync need TotalTime and last point
			if (currentRunEncoder != null && sendPoints_l.Count > 0)
				videoTime += UtilAll.DivideSafe (currentRunEncoder.TotalTime, 1000000.0) -
					PointF.Last (sendPoints_l).X;

			//LogB.Information (string.Format ("raceAnalyzer videoTime: {0}, webcamPlay.PlayVideoGetSecond: {1}, -diffVideoVsSignal: {2}",
			//			videoTime, webcamPlay.PlayVideoGetSecond, diffVideoVsSignal));
		}

		cairoGraphRaceAnalyzer_st.DoSendingList (preferences.fontTypeToGraph(),
				sendPoints_l,
				cairoGraphSend_CD,
				subtitleWithSetsInfo_l,
				forceRedraw, videoTime,
				CairoXY.PlotTypes.LINES, smoothGui == 0, smoothGui, miw,
				triggerListRunEncoder, timeAtEnoughAccel,
				timeAtEnoughAccelMark, preferences.runEncoderMinAccel,
				hscaleA, hscaleB, hscaleC, hscaleD);
	}
	private void updateRaceAnalyzerCaptureAccelTime(bool forceRedraw)
	{
		if(radio_race_analyzer_capture_view_simple.Active)
			return;

		bool isSprint = false;
		if(currentRunEncoderExercise != null && currentRunEncoderExercise.IsSprint)
			isSprint = true;

		RunEncoderSegmentCalcs segmentCalcs = new RunEncoderSegmentCalcs ();
		if(currentRunEncoderExercise != null && //currentRunEncoderExercise.SegmentCm > 0 &&
				reCGSD != null && reCGSD.SegmentCalcs != null)
			segmentCalcs = reCGSD.SegmentCalcs;

		int timeAtEnoughAccel = 0;
		int timeAtEnoughAccelMark = 0;
		GetMaxAvgInWindow miw = new GetMaxAvgInWindow ();
		if (reCGSD != null)
		{
			timeAtEnoughAccel = reCGSD.TimeAtEnoughAccelOrTrigger0; //to shift time at load, sent here to sync/delete triggers
			timeAtEnoughAccelMark = reCGSD.TimeAtEnoughAccelMark; //to show mark at capture
			miw = reCGSD.Miw;
		}

		if(cairoGraphRaceAnalyzer_at == null || forceRedraw)
			cairoGraphRaceAnalyzer_at = new CairoGraphRaceAnalyzer(
					false,
					drawingarea_race_analyzer_capture_accel_time, "title",
					Catalog.GetString("Accel"), "m/s^2",
					isSprint, false, false,
					segmentCalcs,
					false, preferences.runEncoderCaptureShow,
					false);

		cairoGraphRaceAnalyzer_at.DoSendingList (preferences.fontTypeToGraph(),
				cairoGraphRaceAnalyzerPoints_at_l,
				null,
				new List<string> (),
				forceRedraw, 0,
				CairoXY.PlotTypes.LINES, false,
				getSmoothFrom_gui_at_race_analyzer_capture_smooth_graphs (), miw,
				triggerListRunEncoder, timeAtEnoughAccel,
				timeAtEnoughAccelMark, preferences.runEncoderMinAccel,
				-1, -1, -1, -1);
	}

	private void on_check_race_analyzer_capture_smooth_graphs_clicked (object o, EventArgs args)
	{
		int smooth = getSmoothFrom_gui_at_race_analyzer_capture_smooth_graphs ();
		if(check_race_analyzer_capture_smooth_graphs.Active)
		{
			hscale_race_analyzer_capture_smooth_graphs.Visible = true;
			label_race_analyzer_capture_smooth_graphs.Visible = true;

			if(smooth == 0)
				label_race_analyzer_capture_smooth_graphs.Text = "";
			else
				label_race_analyzer_capture_smooth_graphs.Text = smooth.ToString();
		} else {
			hscale_race_analyzer_capture_smooth_graphs.Visible = false;
			label_race_analyzer_capture_smooth_graphs.Visible = false;
		}

		if (reCGSD != null)
			reCGSD.SegmentsRedoWithSmoothing (smooth);

		drawingarea_race_analyzer_capture_position_time.QueueDraw ();
		drawingarea_race_analyzer_capture_speed_time.QueueDraw ();
		drawingarea_race_analyzer_capture_accel_time.QueueDraw ();
	}

	private int getSmoothFrom_gui_at_race_analyzer_capture_smooth_graphs ()
	{
		if(! check_race_analyzer_capture_smooth_graphs.Active)
			return 0;

		//if 1,2,3,4,5,6,7  return 3,5,7,9,11,13,15
		return Convert.ToInt32(hscale_race_analyzer_capture_smooth_graphs.Value)*2 +1;
	}
	private void on_hscale_race_analyzer_capture_smooth_graphs_value_changed (object o, EventArgs args)
	{
		int smooth = getSmoothFrom_gui_at_race_analyzer_capture_smooth_graphs ();
		if(smooth == 0)
			label_race_analyzer_capture_smooth_graphs.Text = "";
		else
			label_race_analyzer_capture_smooth_graphs.Text = smooth.ToString();

		if (reCGSD != null)
			reCGSD.SegmentsRedoWithSmoothing (smooth);

		drawingarea_race_analyzer_capture_position_time.QueueDraw ();
		drawingarea_race_analyzer_capture_speed_time.QueueDraw ();
		drawingarea_race_analyzer_capture_accel_time.QueueDraw ();
	}

	private void on_button_race_analyzer_capture_save_image_clicked (object o, EventArgs args)
	{
		checkFile(Constants.CheckFileOp.RUNENCODER_CAPTURE_SAVE_IMAGE);
	}
	private void on_button_run_encoder_capture_image_save_selected (string destination)
	{
		try {
			LogB.Information("Saving");
			CairoUtil.GetScreenshotFromVBox (vbox_race_analyzer_capture_graphs, destination);
		} catch {
			string myString = string.Format(
					Catalog.GetString("Cannot save file {0} "), destination);
			new DialogMessage(Constants.MessageTypes.WARNING, myString);
		}
	}
	private void on_overwrite_file_runencoder_capture_image_save_accepted(object o, EventArgs args)
	{
		on_button_run_encoder_capture_image_save_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), exportFileName);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}

	private void updateGraphRunEncoderBars ()
	{
		LogB.Information (string.Format ("currentPerson == null: {0},  currentSession == null: {1}",
					currentPerson == null, currentSession == null));
		if(currentPerson == null || currentSession == null)
			return;

		// 1. prepare eventGraph object needed for the graph

		//initalizeVariables if not done before
		event_execute_initializeVariables(
			(! cp2016.StoredCanCaptureContacts && ! cp2016.StoredWireless), //is simulated
			currentPerson.UniqueID,
			currentPerson.Name,
			"", //Catalog.GetString("Phases"),  	  //name of the different moments
			Constants.RunEncoderTable, //tableName
			"" //type
			);

		int selectedID = -1;
		int exerciseID; //if test is selected on resultsSession will be this. If not will be the combo on gui for next test

		if (treeViewResultsSession != null && treeViewResultsSession.EventSelectedID >= 0)
		{
			selectedID = treeViewResultsSession.EventSelectedID;
			exerciseID = SqliteRunEncoder.SelectData (selectedID, false, false).ExerciseID;
		} else
			exerciseID = getExerciseIDFromAnyCombo (combo_run_encoder_exercise, runEncoderComboExercisesString, false);

		Constants.ResultsSessionCriteria resultsSessionCriteria;
		if (radio_resultsSession_last.Active)
			resultsSessionCriteria = Constants.ResultsSessionCriteria.LAST;
		else {
			if (radio_resultsSession_ra_speeds.Active)
				resultsSessionCriteria = Constants.ResultsSessionCriteria.BEST;
			else
				resultsSessionCriteria = Constants.ResultsSessionCriteria.BEST2;
		}

		PrepareEventGraphRunEncoder eventGraph = new PrepareEventGraphRunEncoder (
				currentSession.UniqueID,
				currentPerson.UniqueID, currentPerson.Name, radio_contacts_results_personAll.Active,
				//get_radio_resultsSession_criteria (),
				resultsSessionCriteria, radio_resultsSession_ra_best_second.Active,
				-1 * Convert.ToInt32 (spin_resultsSession_limit.Value), //negative: end limit
				//Constants.RunEncoderTable, typeTemp,
				exerciseID, selectedID, current_mode, radio_contacts_graph_allTests.Active);

		//if(eventGraph.personMAXAtSQLAllSessions > 0 || eventGraph.runsAtSQL.Count > 0)
		//	PrepareRunSimpleGraph(eventGraph, false); //don't animate

		// 2. Do the graph

		string typeTemp = "";
		if(! radio_contacts_graph_allTests.Active)
			typeTemp = UtilGtk.ComboGetActive (combo_run_encoder_exercise);

		string personStr = "";
		if(! radio_contacts_results_personAll.Active)
			personStr = currentPerson.Name;

		//LogB.Information("drawingarea_results_session == null: ",
		//	(drawingarea_results_session == null).ToString());

		cairoPaintBarsPre = new CairoPaintBarsPreRunEncoder (
				drawingarea_results_session, preferences.fontTypeToGraph(), current_mode,
				personStr,
				typeTemp,
				preferences.digitsNumber,
				currentPerson.UniqueID,
				radio_resultsSession_ra_best_second.Active,
				radio_resultsSession_bars.Active);

		cairoPaintBarsPre.StoreEventGraphRunEncoder (eventGraph);
		drawingarea_results_session.QueueDraw ();
	}

	private void connectWidgetsRunEncoder (Gtk.Builder builder)
	{
		//menuitem_check_race_encoder_capture_simulate = (Gtk.CheckMenuItem) builder.GetObject ("menuitem_check_race_encoder_capture_simulate");

		button_combo_run_encoder_exercise_capture_left = (Gtk.Button) builder.GetObject ("button_combo_run_encoder_exercise_capture_left");
		button_combo_run_encoder_exercise_capture_right = (Gtk.Button) builder.GetObject ("button_combo_run_encoder_exercise_capture_right");
		vbox_run_encoder_capture_buttons = (Gtk.VBox) builder.GetObject ("vbox_run_encoder_capture_buttons");
		vbox_run_encoder_capture_options = (Gtk.VBox) builder.GetObject ("vbox_run_encoder_capture_options");
		hbox_combo_run_encoder_exercise = (Gtk.HBox) builder.GetObject ("hbox_combo_run_encoder_exercise");
		race_analyzer_spinbutton_distance = (Gtk.SpinButton) builder.GetObject ("race_analyzer_spinbutton_distance");
		race_analyzer_spinbutton_angle = (Gtk.SpinButton) builder.GetObject ("race_analyzer_spinbutton_angle");
		race_analyzer_spinbutton_temperature = (Gtk.SpinButton) builder.GetObject ("race_analyzer_spinbutton_temperature");
		treeview_raceAnalyzer = (Gtk.TreeView) builder.GetObject ("treeview_raceAnalyzer");
		button_raceAnalyzer_table_save = (Gtk.Button) builder.GetObject ("button_raceAnalyzer_table_save");
		//label_race_analyzer_capture_speed = (Gtk.Label) builder.GetObject ("label_race_analyzer_capture_speed");
		grid_race_analyzer_capture_tab_result_views = (Gtk.Grid) builder.GetObject ("grid_race_analyzer_capture_tab_result_views");
		radio_race_analyzer_capture_view_simple = (Gtk.RadioButton) builder.GetObject ("radio_race_analyzer_capture_view_simple");
		radio_race_analyzer_capture_view_complete = (Gtk.RadioButton) builder.GetObject ("radio_race_analyzer_capture_view_complete");
		//alignment_drawingarea_race_analyzer_capture_velocimeter_topleft = (Gtk.Alignment) builder.GetObject ("alignment_drawingarea_race_analyzer_capture_velocimeter_topleft");
		alignment_hbox_race_analyzer_capture_bottom = (Gtk.Alignment) builder.GetObject ("alignment_hbox_race_analyzer_capture_bottom");
		//drawingarea_race_analyzer_capture_velocimeter_topleft = (Gtk.DrawingArea) builder.GetObject ("drawingarea_race_analyzer_capture_velocimeter_topleft");
		drawingarea_race_analyzer_capture_velocimeter_bottom = (Gtk.DrawingArea) builder.GetObject ("drawingarea_race_analyzer_capture_velocimeter_bottom");
		drawingarea_race_analyzer_capture_position_time = (Gtk.DrawingArea) builder.GetObject ("drawingarea_race_analyzer_capture_position_time");
		drawingarea_race_analyzer_capture_speed_time = (Gtk.DrawingArea) builder.GetObject ("drawingarea_race_analyzer_capture_speed_time");
		drawingarea_race_analyzer_capture_accel_time = (Gtk.DrawingArea) builder.GetObject ("drawingarea_race_analyzer_capture_accel_time");
		vbox_race_analyzer_capture_graphs = (Gtk.VBox) builder.GetObject ("vbox_race_analyzer_capture_graphs");
		check_race_analyzer_capture_smooth_graphs = (Gtk.CheckButton) builder.GetObject ("check_race_analyzer_capture_smooth_graphs");
		hscale_race_analyzer_capture_smooth_graphs = (Gtk.HScale) builder.GetObject ("hscale_race_analyzer_capture_smooth_graphs");
		radio_race_analyzer_capture_graph_starts_0 = (Gtk.RadioButton) builder.GetObject ("radio_race_analyzer_capture_graph_starts_0");
		radio_race_analyzer_capture_graph_starts_grav = (Gtk.RadioButton) builder.GetObject ("radio_race_analyzer_capture_graph_starts_grav");
		label_race_analyzer_capture_smooth_graphs = (Gtk.Label) builder.GetObject ("label_race_analyzer_capture_smooth_graphs");

		frame_run_encoder_exercise = (Gtk.Frame) builder.GetObject ("frame_run_encoder_exercise");
		entry_run_encoder_exercise_name = (Gtk.Entry) builder.GetObject ("entry_run_encoder_exercise_name");
		entry_run_encoder_exercise_description = (Gtk.Entry) builder.GetObject ("entry_run_encoder_exercise_description");
		check_run_encoder_exercise_is_sprint = (Gtk.CheckButton) builder.GetObject ("check_run_encoder_exercise_is_sprint");
		spin_run_encoder_exercise_angle_default = (Gtk.SpinButton) builder.GetObject ("spin_run_encoder_exercise_angle_default");
		check_run_encoder_exercise_fixed_size = (Gtk.CheckButton) builder.GetObject ("check_run_encoder_exercise_fixed_size");
		hbox_run_encoder_exercise_fixed_segments_size = (Gtk.HBox) builder.GetObject ("hbox_run_encoder_exercise_fixed_segments_size");
		hbox_run_encoder_exercise_notfixed_segment_num = (Gtk.HBox) builder.GetObject ("hbox_run_encoder_exercise_notfixed_segment_num");
		spin_race_encoder_exercise_f_segment_size_cm = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_f_segment_size_cm"); //f: fixed
		spin_race_encoder_exercise_v_segments_num = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segments_num"); //v: variable
		frame_run_encoder_exercise_notfixed_segments = (Gtk.Frame) builder.GetObject ("frame_run_encoder_exercise_notfixed_segments");
		spin_race_encoder_exercise_v_segment_size_cm_0 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_0"); //v: variable
		spin_race_encoder_exercise_v_segment_size_cm_1 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_1");
		spin_race_encoder_exercise_v_segment_size_cm_2 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_2");
		spin_race_encoder_exercise_v_segment_size_cm_3 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_3");
		spin_race_encoder_exercise_v_segment_size_cm_4 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_4");
		spin_race_encoder_exercise_v_segment_size_cm_5 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_5");
		spin_race_encoder_exercise_v_segment_size_cm_6 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_6");
		spin_race_encoder_exercise_v_segment_size_cm_7 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_7");
		spin_race_encoder_exercise_v_segment_size_cm_8 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_8");
		spin_race_encoder_exercise_v_segment_size_cm_9 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_9");
		spin_race_encoder_exercise_v_segment_size_cm_10 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_10");
		spin_race_encoder_exercise_v_segment_size_cm_11 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_11");
		spin_race_encoder_exercise_v_segment_size_cm_12 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_12");
		spin_race_encoder_exercise_v_segment_size_cm_13 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_13");
		spin_race_encoder_exercise_v_segment_size_cm_14 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_14");
		spin_race_encoder_exercise_v_segment_size_cm_15 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_15");
		spin_race_encoder_exercise_v_segment_size_cm_16 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_16");
		spin_race_encoder_exercise_v_segment_size_cm_17 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_17");
		spin_race_encoder_exercise_v_segment_size_cm_18 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_18");
		spin_race_encoder_exercise_v_segment_size_cm_19 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_19");
		spin_race_encoder_exercise_v_segment_size_cm_20 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_20");
		spin_race_encoder_exercise_v_segment_size_cm_21 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_21");
		spin_race_encoder_exercise_v_segment_size_cm_22 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_22");
		spin_race_encoder_exercise_v_segment_size_cm_23 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_23");
		spin_race_encoder_exercise_v_segment_size_cm_24 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_24");
		spin_race_encoder_exercise_v_segment_size_cm_25 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_25");
		spin_race_encoder_exercise_v_segment_size_cm_26 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_26");
		spin_race_encoder_exercise_v_segment_size_cm_27 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_27");
		spin_race_encoder_exercise_v_segment_size_cm_28 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_28");
		spin_race_encoder_exercise_v_segment_size_cm_29 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_29");
		spin_race_encoder_exercise_v_segment_size_cm_30 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_30");
		spin_race_encoder_exercise_v_segment_size_cm_31 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_31");
		spin_race_encoder_exercise_v_segment_size_cm_32 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_32");
		spin_race_encoder_exercise_v_segment_size_cm_33 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_33");
		spin_race_encoder_exercise_v_segment_size_cm_34 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_34");
		spin_race_encoder_exercise_v_segment_size_cm_35 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_35");
		spin_race_encoder_exercise_v_segment_size_cm_36 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_36");
		spin_race_encoder_exercise_v_segment_size_cm_37 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_37");
		spin_race_encoder_exercise_v_segment_size_cm_38 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_38");
		spin_race_encoder_exercise_v_segment_size_cm_39 = (Gtk.SpinButton) builder.GetObject ("spin_race_encoder_exercise_v_segment_size_cm_39");
		box_combo_race_analyzer_device = (Gtk.Box) builder.GetObject ("box_combo_race_analyzer_device");
	}
}
