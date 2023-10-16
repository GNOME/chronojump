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
 * Copyright (C) 2017-2023   Xavier de Blas <xaviblas@gmail.com>
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
using Mono.Unix;
using Kinovea.Filtering;


public partial class ChronoJumpWindow 
{
	// at glade ---->
	//capture tab
	Gtk.Button button_combo_force_sensor_exercise_capture_left;
	Gtk.Button button_combo_force_sensor_exercise_capture_right;
	Gtk.HBox hbox_force_capture_buttons;
	Gtk.Entry force_sensor_exercise_filter;
	Gtk.HBox hbox_combo_force_sensor_exercise;
	Gtk.Frame frame_force_sensor_elastic;
	Gtk.Button button_stiffness_detect;
	Gtk.Label label_button_force_sensor_stiffness;
	Gtk.Image image_button_force_sensor_stiffness_problem;
	Gtk.RadioButton radio_force_sensor_laterality_both;
	Gtk.RadioButton radio_force_sensor_laterality_l;
	Gtk.RadioButton radio_force_sensor_laterality_r;
	Gtk.VBox vbox_force_sensor_adjust_actions;
	Gtk.Button button_force_sensor_tare;
	Gtk.Button button_force_sensor_calibrate;
	Gtk.Label label_force_sensor_value_max;
	Gtk.Label label_force_sensor_value;
	Gtk.Label label_force_sensor_value_min;
	Gtk.Label label_force_sensor_value_best_second;
	Gtk.Label label_force_sensor_value_rfd;
	//Gtk.VScale vscale_force_sensor;
	Gtk.SpinButton spin_force_sensor_calibration_kg_value;
	Gtk.Box box_force_sensor_capture_magnitudes;
	Gtk.CheckButton check_force_sensor_capture_show_distance;
	Gtk.CheckButton check_force_sensor_capture_show_speed;
	Gtk.CheckButton check_force_sensor_capture_show_power;
	Gtk.Image image_force_sensor_capture_show_distance;
	Gtk.Image image_force_sensor_capture_show_speed;
	Gtk.Image image_force_sensor_capture_show_power;
	Gtk.Button button_force_sensor_image_save_signal;
	Gtk.DrawingArea force_capture_drawingarea_cairo;
	Gtk.Button button_force_sensor_exercise_edit;
	Gtk.Button button_force_sensor_exercise_delete;
	Gtk.Label force_sensor_adjust_label_message;
	Gtk.Box box_combo_force_sensor_capture_options;
	// <---- at glade

	Gtk.ComboBoxText combo_force_sensor_capture_options;

	ForceSensorExerciseWindow forceSensorExerciseWin;
	ForceSensorElasticBandsWindow forceSensorElasticBandsWin;

	Gtk.ComboBoxText combo_force_sensor_exercise;

	Thread forceCaptureThread;
	static bool forceProcessFinish;
	static bool forceProcessCancel;
	static bool forceProcessKill; //when user closes program while capturing (do not call arduino and wait for its response)
	static bool forceProcessError;

	Thread forceOtherThread; //for messages on: capture, tare, calibrate
	static string forceSensorOtherMessage = "";

	enum secondsEnum { NO, ASC, DESC };
	static secondsEnum forceSensorOtherMessageShowSeconds;
	static double forceSensorOtherMessageShowSecondsInit; //for DESC

	static DateTime forceSensorTimeStart;
	static string lastForceSensorFile = "";
	static string lastForceSensorFullPath = "";
	static string lastForceSensorFullPath_CD = "";

	int usbDisconnectedCount;
	int usbDisconnectedLastTime;

	private ForceSensor currentForceSensor;
	private ForceSensor currentForceSensor_CD; //only for analyze cd (when 2 sets), only for know laterality, and datetime
	private ForceSensorExercise currentForceSensorExercise;
	private ForceSensorExercise currentForceSensorExercise_CD; //only for analyze cd (when 2sets)
	DateTime forceSensorTimeStartCapture;
	private string forceSensorFirmwareVersion;

	int forceSensorStiffMinCm;
	int forceSensorStiffMaxCm;

	//non GTK on this method

	/*
	 * arduinoCaptureStatus:
	 * STOP is when is not used
	 * STARTING is while is waiting forceSensor to start capturing
	 * CAPTURING is when data is arriving
	 * COPIED_TO_TMP means data is on tmp and graph can be called
	 */
	enum arduinoCaptureStatus { STOP, STARTING, CAPTURING, COPIED_TO_TMP }
	static arduinoCaptureStatus capturingForce = arduinoCaptureStatus.STOP;

	static bool forceCaptureStartMark; 	//Just needed to display Capturing message (with seconds)
	static bool forceTooBigMark;
	static double forceTooBigValue;
	static ForceSensorValues forceSensorValues;

	SerialPort portFS; //Attention!! Don't reopen port because arduino makes reset and tare, calibration... is lost
	bool portFSOpened;
	bool forceSensorBinaryCapture;

	//for a second forceSensor
	string forceSensorPortName_B = "/dev/ttyUSB1"; //TODO: hardcoded
	SerialPort portFS_B; //Attention!! Don't reopen port because arduino makes reset and tare, calibration... is lost
	bool portFSOpened_B;

	List<PointF> interpolate_l; 	//interpolated path

	string forceSensorNotConnectedString =
		Catalog.GetString("Force sensor is not detected!") + " " +
		Catalog.GetString("Plug cable and click on 'device' button.");


	TreeviewFSAnalyze tvFS_AB;
	TreeviewFSAnalyze tvFS_CD;
	TreeviewFSAnalyzeOther tvFS_other;

	private void initForceSensor ()
	{
		//notebook_ai_top.CurrentPage = Convert.ToInt32(notebook_ai_top_pages.CURRENTSETSIGNAL);
		//change radio and will change also notebook:
		radio_signal_analyze_current_set.Active = true;

		createComboForceSensorCaptureOptions();
		createForceExerciseCombo();
		UtilGtk.ViewportColor (viewport_radio_ai_ab, UtilGtk.Colors.YELLOW_LIGHT);
		UtilGtk.ViewportColor (viewport_radio_ai_cd, UtilGtk.Colors.GREEN_LIGHT);
		UtilGtk.ViewportColor (viewport_ai_hscales, UtilGtk.Colors.YELLOW_LIGHT);
		createForceAnalyzeCombos();
		setForceDurationRadios();
		setRFDValues();
		setImpulseValue();
		setForceSensorAnalyzeABSliderIncrements();

		aiButtonsHscaleZoomSensitiveness();
		if (tvFS_other == null)
			tvFS_other = new TreeviewFSAnalyzeOther (treeview_force_sensor_ai_other);
		setForceSensorAnalyzeMaxAVGInWindow();

		spinbutton_ai_export_image_width.Value = preferences.exportGraphWidth;
		spinbutton_ai_export_image_height.Value = preferences.exportGraphHeight;
	}

	//Attention: no GTK here!!
	private bool forceSensorConnect()
	{
		LogB.Information(" FS connect 0 ");
		if (chronopicRegister.GetSelectedForMode (current_mode).Port == "")
		{
			forceSensorOtherMessage = forceSensorNotConnectedString;
			return false;
		}

		LogB.Information(" FS connect 3 ");

		return forceSensorConnectDo ();
	}

	private bool forceSensorConnectDo()
	{
		forceSensorOtherMessage = "Connecting ...";

		portFS = new SerialPort (chronopicRegister.GetSelectedForMode (current_mode).Port, 115200);
		LogB.Information(" FS connect 4: opening port...");

		try {
			portFS.Open();
		}
		catch (System.IO.IOException)
		{
			forceSensorOtherMessage = forceSensorNotConnectedString;
			return false;
		}

		LogB.Information(" FS connect 5: let arduino start");

		Thread.Sleep(3000); //sleep to let arduino start reading serial event

		LogB.Information(" FS connect 6: get version");

		forceSensorFirmwareVersion = forceSensorCheckVersionDo();
		LogB.Information("Version found: [" + forceSensorFirmwareVersion + "]");

		if(forceSensorFirmwareVersion == "0.1")
		{
			LogB.Information(" FS connect 6b, version 0.1: adjusting parameters...");

			//set_tare
			if(! forceSensorSendCommand("set_tare:" + preferences.forceSensorTare.ToString() + ";",
						"Setting previous tare ...", "Catched adjusting tare"))
				return false;

			//read confirmation data
			if(forceSensorReceiveFeedback("Tare set") == "")
				return false;


			//set_calibration_factor
			if(! forceSensorSendCommand("set_calibration_factor:" + Util.ConvertToPoint(preferences.forceSensorCalibrationFactor) + ";",
						"Setting previous calibration factor ...", "Catched adjusting calibration factor"))
				return false;

			//read confirmation data
			if(forceSensorReceiveFeedback("Calibration factor set") == "")
				return false;
		}

		forceSensorBinaryCapture = false;
                double versionDouble = Convert.ToDouble(Util.ChangeDecimalSeparator(forceSensorFirmwareVersion));
		if(versionDouble >= Convert.ToDouble(Util.ChangeDecimalSeparator("0.3"))) //from 0.3 versions can be binary
			forceSensorBinaryCapture = forceSensorCheckBinaryCapture();

		LogB.Information("forceSensorBinaryCapture = " + forceSensorBinaryCapture.ToString());

		portFSOpened = true;
		forceSensorOtherMessage = "Connected!";
		LogB.Information(" FS connect 7: connected and adjusted!");
		return true;
	}
	private bool forceSensorConnect_Port_B()
	{
		portFS_B = new SerialPort(forceSensorPortName_B, 115200); //forceSensor

		try {
			portFS_B.Open();
		}
		catch (System.IO.IOException)
		{
			return false;
		}

		Thread.Sleep(3000); //sleep to let arduino start reading serial event

		portFSOpened_B = true;
		return true;
	}

	//this is called on change mode
	private void forceSensorDisconnect()
	{
		portFS.Close();
		portFSOpened = false;
		//event_execute_label_message.Text = "Disconnected!";
		LogB.Information("PortFS Disconnected!");
	}

	//Attention: no GTK here!!
	private bool forceSensorSendCommand(string command, string displayMessage, string errorMessage)
	{
		forceSensorOtherMessage = displayMessage;

		try {
			LogB.Information("Force sensor command |" + command + "|");
			portFS.WriteLine(command);
			forceSensorTimeStart = DateTime.Now;
		}
		catch (Exception ex)
		{
			LogB.Information ("exception: " + ex.ToString ());
			if(ex is System.IO.IOException || ex is System.TimeoutException)
			{
				LogB.Information(errorMessage);
//portFS.Close(); //TODO: seguir investigant
				portFSOpened = false;
				return false;
			}
			//throw;
		}
		return true;
	}

	//Attention: no GTK here!!
	private bool forceSensorSendCommand_B(string command, string displayMessage, string errorMessage)
	{
		try {
			LogB.Information("Force sensor command |" + command + "|");
			portFS_B.WriteLine(command);
		}
		catch (Exception ex)
		{
			if(ex is System.IO.IOException || ex is System.TimeoutException)
			{
				LogB.Information(errorMessage);
//portFS_B.Close(); //TODO: seguir investigant
				portFSOpened_B = false;
				return false;
			}
			//throw;
		}
		return true;
	}

	//PORT_B is used when there are two devices (and have to be in sync)
	private enum forceSensorPortEnum { PORT_A, PORT_B };

	private string forceSensorReceiveFeedback(string expected)
	{
		return forceSensorReceiveFeedback(expected, forceSensorPortEnum.PORT_A);
	}
	//use this method for other feedback, but beware some of the commands do a Trim on ReadLine
	private string forceSensorReceiveFeedback(string expected, forceSensorPortEnum myPort)
	{
		string str = "";
		do {
			Thread.Sleep(100); //sleep to let arduino start reading
			try {
				if(myPort == forceSensorPortEnum.PORT_A)
					str = portFS.ReadLine();
				else
					str = portFS_B.ReadLine();
			} catch {
				forceSensorOtherMessage = "Disconnected";
				return "";
			}
			LogB.Information("init string: " + str);
		}
		while(! str.Contains(expected));
		return str;
	}

	enum forceSensorOtherModeEnum { TARE, CALIBRATE, CAPTURE_PRE, TARE_AND_CAPTURE_PRE, STIFFNESS_DETECT, CHECK_VERSION }
	static forceSensorOtherModeEnum forceSensorOtherMode;

	//buttons: tare, calibrate, check version and capture (via on_button_execute_test_cicked) come here
	private void on_buttons_force_sensor_clicked(object o, EventArgs args)
	{
		if (o == (object) button_execute_test)
		{
			if(UtilGtk.ComboGetActive(combo_force_sensor_exercise) == "")
			{
				new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Need to create/select an exercise."));
				return;
			}

			assignCurrentForceSensorExercise();

			if(currentForceSensorExercise.ComputeAsElastic)
			{
				List<ForceSensorElasticBand> list_fseb = SqliteForceSensorElasticBand.SelectAll(false, true); //not opened, onlyActive
				if(ForceSensorElasticBand.GetStiffnessOfActiveBands(list_fseb) == 0 || image_button_force_sensor_stiffness_problem.Visible)
				{
					new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Need to configure fixture to know stiffness of this elastic exercise."));
					return;
				}
			}
			box_force_sensor_capture_magnitudes.Visible = currentForceSensorExercise.ComputeAsElastic;
			box_force_sensor_analyze_magnitudes.Visible = currentForceSensorExercise.ComputeAsElastic;
		}

		if (! Config.SimulatedCapture && chronopicRegister.GetSelectedForMode (current_mode).Port == "")
		{
			event_execute_label_message.Text = forceSensorNotConnectedString;
			return;
		}

		capturingForce = arduinoCaptureStatus.STOP;
		forceSensorTimeStart = DateTime.Now;
		forceSensorOtherMessageShowSeconds = secondsEnum.ASC;

		if(o == (object) button_force_sensor_tare)
		{
			vbox_force_sensor_adjust_actions.Sensitive = false;
			forceSensorOtherMode = forceSensorOtherModeEnum.TARE;
			forceOtherThread = new Thread(new ThreadStart(forceSensorTare));
		}
		else if(o == (object) button_force_sensor_calibrate)
		{
			vbox_force_sensor_adjust_actions.Sensitive = false;
			forceSensorOtherMode = forceSensorOtherModeEnum.CALIBRATE;
			forceOtherThread = new Thread(new ThreadStart(forceSensorCalibrate));
		}
		else if (o == (object) button_execute_test)
		{
			//notebook_ai_top.CurrentPage = Convert.ToInt32(notebook_ai_top_pages.CURRENTSETSIGNAL);
			//change radio and will change also notebook:
			radio_signal_analyze_current_set.Active = true;

			forceSensorButtonsSensitive(false);
			sensitiveLastTestButtons(false);
			contactsShowCaptureDoingButtons(true);
			image_ai_model_graph.Sensitive = false; //unsensitivize the RFD image (can contain info of previous data)

			//textview_force_sensor_capture_comment.Buffer.Text = "";
			textview_contacts_signal_comment.Buffer.Text = "";

			if(currentForceSensorExercise.TareBeforeCaptureAndForceResultant)
			{
				forceSensorOtherMode = forceSensorOtherModeEnum.TARE_AND_CAPTURE_PRE;
				//forceOtherThread = new Thread(new ThreadStart(forceSensorTareAndCapturePre_noGTK)); //unused see comments on that method
				forceOtherThread = new Thread(new ThreadStart(forceSensorCapturePre_noGTK));
			} else {
				forceSensorOtherMode = forceSensorOtherModeEnum.CAPTURE_PRE;
				forceOtherThread = new Thread(new ThreadStart(forceSensorCapturePre_noGTK));
			}
		}
		else if (o == (object) button_stiffness_detect)
		{
			box_contacts_capture_top.Sensitive = false;
			forceSensorButtonsSensitive(false);
			forceSensorOtherMode = forceSensorOtherModeEnum.STIFFNESS_DETECT;
			forceOtherThread = new Thread(new ThreadStart(forceSensorDetectStiffness));
		}
		else { //if (o == (object) button_check_version)
			forceSensorButtonsSensitive(false);
			forceSensorOtherMode = forceSensorOtherModeEnum.CHECK_VERSION;
			forceOtherThread = new Thread(new ThreadStart(forceSensorCheckVersionPre));
		}

		GLib.Idle.Add (new GLib.IdleHandler (pulseGTKForceSensorOther));

		LogB.ThreadStart();
		forceOtherThread.Start();
	}

	void forceSensorButtonsSensitive(bool sensitive)
	{
		//force related buttons
		hbox_force_capture_buttons.Sensitive = sensitive;
		frame_contacts_exercise.Sensitive = sensitive;
		button_execute_test.Sensitive = sensitive;
		button_signal_analyze_load_ab.Sensitive = sensitive;
		button_signal_analyze_load_cd.Sensitive = sensitive;
		if (! sensitive )
			button_ai_move_cd_pre.Sensitive = false;
		else
			button_ai_move_cd_pre_set_sensitivity ();

		hbox_contacts_camera.Sensitive = sensitive;

		//other gui buttons
		hbox_contacts_sup_capture_analyze_two_buttons.Sensitive = sensitive;
		frame_persons.Sensitive = sensitive;
		menus_and_mode_sensitive(sensitive);

		hbox_top_person.Sensitive = sensitive;
		hbox_chronopics_and_more.Sensitive = sensitive;
		button_force_sensor_adjust.Sensitive = sensitive;
	}

	private void on_button_force_sensor_stiffness_detect_clicked (object o, EventArgs args)
	{
		forceSensorStiffMinCm = 0;
		forceSensorStiffMaxCm = 0;

		ArrayList bigArray = new ArrayList();
		ArrayList a1 = new ArrayList();
		ArrayList a2 = new ArrayList();

		//0 is the widgget to show; 1 is the editable; 2 id default value
		a1.Add(Constants.GenericWindowShow.SPININT2); a1.Add(true); a1.Add("");
		bigArray.Add(a1);

		a2.Add(Constants.GenericWindowShow.SPININT3); a2.Add(true); a2.Add("");
		bigArray.Add(a2);

		genericWin = GenericWindow.Show("", true,	//show now
				Catalog.GetString("Detect stiffness of one band/tube") + "\n\n" +
				"Eschema of the band/tube:" + "\n" +
				"0------------------d-----A----------B---\n\n" +
				Catalog.GetString("Legend:") + "\n" +
				"0-d: " + Catalog.GetString("Length without tension") + "\n" +
				"d-A: " + Catalog.GetString("Minimum working distance") + "\n" +
				"d-B: " + Catalog.GetString("Maximum working distance") + "\n",
				bigArray);

		genericWin.LabelSpinInt2 = "d-A (cm)";
		genericWin.LabelSpinInt3 = "d-B (cm)";
		genericWin.SetSpin2Range(0, 30000);
		genericWin.SetSpin3Range(0, 30000);
		genericWin.SetButtonAcceptLabel(Catalog.GetString("Start"));
		//genericWin.SetSizeRequest(300, -1);
		genericWin.Button_accept.Clicked += new EventHandler(force_sensor_stiffness_detect_start);
	}
	private void force_sensor_stiffness_detect_start (object o, EventArgs args)
	{
		genericWin.Button_accept.Clicked -= new EventHandler(force_sensor_stiffness_detect_start);

		forceSensorStiffMinCm = genericWin.SpinInt2Selected;
		forceSensorStiffMaxCm = genericWin.SpinInt3Selected;
		genericWin.HideAndNull();

		if(forceSensorStiffMinCm == forceSensorStiffMaxCm)
		{
			new DialogMessage(Constants.MessageTypes.WARNING, "Error: Distances cannot be the same");
			return;
		}

		on_button_contacts_exercise_close_clicked (o, args);
		on_buttons_force_sensor_clicked (button_stiffness_detect, new EventArgs ());
	}

	private void forceSensorPersonChanged()
	{
		blankForceSensorInterface();
	}
	private void blankForceSensorInterface()
	{
		currentForceSensor = new ForceSensor();

		blankAIInterface ();

		/*
		 * without this, on change person fsAI graph will blank
		 * but first makes no graph when resize,
		 * and second does not allow the graph to be done on going to RFD automatic and return
		 */
		fsAI_AB = null;
		fsAI_CD = null;
		lastForceSensorFullPath = null;
		lastForceSensorFullPath_CD = null;

		button_contacts_exercise_close_and_recalculate.Sensitive = false;
		textview_contacts_signal_comment.Buffer.Text = "";
		hbox_force_general_analysis.Sensitive = false;
		button_ai_model.Sensitive = false;
		button_contacts_delete_selected.Sensitive = false;

		// if on RFD model graph shown, go back to signal
		if (notebook_ai_top.CurrentPage ==
				Convert.ToInt32 (notebook_ai_top_pages.CURRENTSETMODEL))
			notebook_ai_top.CurrentPage =
				Convert.ToInt32 (notebook_ai_top_pages.CURRENTSETSIGNAL);

		// if on zoom, exit
		if (check_ai_zoom.Active)
		{
			check_ai_zoom.Active = false;
			image_force_sensor_ai_zoom.Visible = true;
			image_force_sensor_ai_zoom_out.Visible = false;
		}

		// erase cairo graphs ---->
		// ... at capture tab
		cairoGraphForceSensorSignal = null;
		spCairoFE = new SignalPointsCairoForceElastic ();
		spCairoFE_CD = new SignalPointsCairoForceElastic ();
		paintPointsInterpolateCairo_l = new List<PointF>();
		force_capture_drawingarea_cairo.QueueDraw ();

		// ... at analyze tab
		spCairoFEZoom = new SignalPointsCairoForceElastic ();
		spCairoFEZoom_CD = new SignalPointsCairoForceElastic ();
		ai_drawingarea_cairo.QueueDraw ();

		// <---- end of erase cairo graphs

		radiosAiSensitivity (true); //because maybe zoom was in
		aiButtonsHscaleZoomSensitiveness();

		label_force_sensor_value_max.Text = "";
		label_force_sensor_value.Text = "";
		label_force_sensor_value_min.Text = "";
		label_force_sensor_value_best_second.Text = "";
		label_force_sensor_value_rfd.Text = "";

		if (radio_ai_export_individual_current_session.Active)
		{
			if(currentPerson != null)
				label_ai_export_person.Text = currentPerson.Name;
			else
				label_ai_export_person.Text = "";
		}

		button_force_sensor_image_save_signal.Sensitive = false;

		label_ai_export_result.Text = "";
		button_ai_export_result_open.Visible = false;
		event_execute_label_message.Text = "";
		box_force_sensor_capture_magnitudes.Visible = false;
		box_force_sensor_analyze_magnitudes.Visible = false;
	}

	private bool pulseGTKForceSensorOther ()
	{
		string secondsStr = "";
		if(forceSensorOtherMessage != "")
		{
			if(forceSensorOtherMessageShowSeconds == secondsEnum.ASC)
			{
				TimeSpan ts = DateTime.Now.Subtract(forceSensorTimeStart);
				double seconds = ts.TotalSeconds;
				secondsStr = " (" + Util.TrimDecimals(seconds, 0) + " s)";
			}
			else if(forceSensorOtherMessageShowSeconds == secondsEnum.DESC)
			{
				TimeSpan ts = DateTime.Now.Subtract(forceSensorTimeStart);
				double seconds = forceSensorOtherMessageShowSecondsInit - ts.TotalSeconds;
				if(seconds < 0)
					seconds = 0;
				secondsStr = " (" + Util.TrimDecimals(seconds, 0) + " s)";
			}
		}

		if(forceOtherThread.IsAlive)
		{
			if(forceSensorOtherMode == forceSensorOtherModeEnum.TARE ||
					forceSensorOtherMode == forceSensorOtherModeEnum.CALIBRATE)
			{
				force_sensor_adjust_label_message.Text = forceSensorOtherMessage + secondsStr;
				force_sensor_adjust_label_message.UseMarkup = true;
			} else
			{
				event_execute_label_message.Text = forceSensorOtherMessage + secondsStr;
				event_execute_label_message.UseMarkup = true;
			}

			if(forceSensorOtherMode == forceSensorOtherModeEnum.STIFFNESS_DETECT &&
					forceSensorValues != null)
			{
				label_force_sensor_value_max.Text = string.Format("{0:0.##}", forceSensorValues.Max);
				label_force_sensor_value_min.Text = string.Format("{0:0.##}", forceSensorValues.Min);
				label_force_sensor_value.Text = string.Format("{0:0.##}", forceSensorValues.ValueLast);
				label_force_sensor_value_best_second.Text = string.Format("{0:0.##}", forceSensorValues.BestSecond);
				label_force_sensor_value_rfd.Text = string.Format("{0:0.##}", forceSensorValues.BestRFD);
			}
		}
		else
		{
			LogB.ThreadEnding();

			if(forceSensorOtherMode == forceSensorOtherModeEnum.TARE ||
				forceSensorOtherMode == forceSensorOtherModeEnum.CALIBRATE)
			{
				force_sensor_adjust_label_message.Text = forceSensorOtherMessage;
				vbox_force_sensor_adjust_actions.Sensitive = true;
				return false;
			}
			else if(forceSensorOtherMode == forceSensorOtherModeEnum.STIFFNESS_DETECT)
			{
				forceSensorButtonsSensitive(true);
				box_contacts_capture_top.Sensitive = true;
				event_execute_label_message.Text = forceSensorOtherMessage;
			}
			else if(forceSensorOtherMode == forceSensorOtherModeEnum.CHECK_VERSION)
			{
				forceSensorButtonsSensitive(true);
				event_execute_label_message.Text = forceSensorOtherMessage;
			}
			else if(forceSensorOtherMode == forceSensorOtherModeEnum.TARE_AND_CAPTURE_PRE || forceSensorOtherMode == forceSensorOtherModeEnum.CAPTURE_PRE)
			{
				event_execute_label_message.Text = forceSensorOtherMessage;
				forceSensorCapturePre2_GTK_cameraCall();
			}

			return false;
		}

		//LogB.Information(" ForceSensor:"+ forceOtherThread.ThreadState.ToString());
		Thread.Sleep (25);
		return true;
	}

	//Attention: no GTK here!!
	private void forceSensorTare()
	{
		// 0 connect if needed
		if(! portFSOpened)
			if(! forceSensorConnect())
				return;

		// 1 countdown before tare
		forceSensorTimeStart = DateTime.Now;
		forceSensorOtherMessageShowSecondsInit = 2.999;
		forceSensorOtherMessageShowSeconds = secondsEnum.DESC;
		forceSensorOtherMessage = Catalog.GetString ("Taring will start in …");
		do {
			/* wait for countdown to end
			LogB.Information (string.Format ("countdown taring: {0}",
						forceSensorOtherMessageShowSecondsInit - DateTime.Now.Subtract(forceSensorTimeStart).TotalSeconds));
			 */
		} while (forceSensorOtherMessageShowSecondsInit - DateTime.Now.Subtract(forceSensorTimeStart).TotalSeconds > 0);

		// 2 send tare command
		forceSensorOtherMessageShowSeconds = secondsEnum.ASC;
		if(! forceSensorSendCommand("tare:", Catalog.GetString ("Taring …"), "Catched force taring"))
			return;

		// 3 read confirmation data
		string str = "";
		do {
			Thread.Sleep(100); //sleep to let arduino start reading
			try {
				str = portFS.ReadLine();
			} catch {
				forceSensorOtherMessage = "Disconnected";
				return;
			}
			LogB.Information("init string: " + str);
		}
		while(! str.Contains("Taring OK"));

		//from Force_Sensor-0.4 at tare returns this: Taring OK:(\d+)
		Match match = Regex.Match(str, @"Taring OK:(\d+)");
		if(match.Groups.Count == 2)
		{
			LogB.Information("matched OK:" + match.Groups[1].ToString());
			str = Util.ChangeDecimalSeparator(match.Groups[1].ToString());
		} else {
			LogB.Information("not matched OK");
			// 4 get tare factor
			if (portFS.BytesToRead > 0)
				LogB.Information("PRE_get_tare bytes: " + portFS.ReadExisting());

			if(! forceSensorSendCommand("get_tare:", Catalog.GetString ("Checking …"), "Catched at get_tare"))
				return;

			str = Util.ChangeDecimalSeparator(portFS.ReadLine().Trim());
		}

		// 5 update preferences and SQL with new tare
		if(Util.IsNumber(str, true))
			preferences.UpdateForceSensorTare(Convert.ToDouble(str));

		// 6 print message
		forceSensorOtherMessageShowSeconds = secondsEnum.NO;
		forceSensorOtherMessage = "Tared!";
	}

	//Attention: no GTK here!!
	private void forceSensorCalibrate()
	{
		// 0 connect if needed
		if(! portFSOpened)
			if(! forceSensorConnect())
				return;

		// 1 countdown before calibrate
		forceSensorTimeStart = DateTime.Now;
		forceSensorOtherMessageShowSecondsInit = 2.999;
		forceSensorOtherMessageShowSeconds = secondsEnum.DESC;
		forceSensorOtherMessage = Catalog.GetString ("Calibrating will start in …");
		do {
			/* wait for countdown to end
			LogB.Information (string.Format ("countdown taring: {0}",
						forceSensorOtherMessageShowSecondsInit - DateTime.Now.Subtract(forceSensorTimeStart).TotalSeconds));
			*/
		} while (forceSensorOtherMessageShowSecondsInit - DateTime.Now.Subtract(forceSensorTimeStart).TotalSeconds > 0);

		// 2 send calibrate command
		forceSensorOtherMessageShowSeconds = secondsEnum.ASC;
		if(! forceSensorSendCommand("calibrate:" + Util.ConvertToPoint(spin_force_sensor_calibration_kg_value.Value) + ";",
					Catalog.GetString ("Calibrating …"), "Catched force calibrating"))
			return;

		// 3 read confirmation data
		string str = "";
		do {
			Thread.Sleep(100); //sleep to let arduino start reading
			try {
				str = portFS.ReadLine();
			} catch {
				forceSensorOtherMessage = "Disconnected";
				return;
			}
			LogB.Information("init string: " + str);
		}
		while(! str.Contains("Calibrating OK"));

		//from Force_Sensor-0.4 at calibrate returns this: Calibrating OK:(\d+\.\d+)
		Match match = Regex.Match(str, @"Calibrating OK:(\d+\.\d+)");
		if(match.Groups.Count == 2)
		{
			LogB.Information("matched OK:" + match.Groups[1].ToString());
			str = Util.ChangeDecimalSeparator(match.Groups[1].ToString());
		} else {
			LogB.Information("not matched OK");
			// 4 get calibration factor
			if (portFS.BytesToRead > 0)
				LogB.Information("PRE_get_calibrationfactor bytes: " + portFS.ReadExisting());

			if(! forceSensorSendCommand("get_calibration_factor:", Catalog.GetString ("Checking …"), "Catched at get_calibration_factor"))
				return;

			// 5 update preferences and SQL with new calibration factor
			str = Util.ChangeDecimalSeparator(portFS.ReadLine().Trim());
		}

		if(Util.IsNumber(str, true))
			preferences.UpdateForceSensorCalibration(
					spin_force_sensor_calibration_kg_value.Value, Convert.ToDouble(str));

		// 6 print message
		forceSensorOtherMessageShowSeconds = secondsEnum.NO;
		forceSensorOtherMessage = "Calibrated!";
	}

	//Attention: no GTK here!!
	private void forceSensorCheckVersionPre()
	{
		if(! portFSOpened)
			if(! forceSensorConnect())
				return;

		forceSensorOtherMessage = Catalog.GetString("Version of the firmware:") + " " + forceSensorCheckVersionDo();
	}

	//Attention: no GTK here!!
	//we do not pass the port because there are problems passing ports
	private string forceSensorCheckVersionDo()
	{
		if (portFS.BytesToRead > 0)
			LogB.Information("check_version read possible bytes: " + portFS.ReadExisting());

		if(! forceSensorSendCommand("get_version:", Catalog.GetString ("Checking version …"), "Catched checking version"))
			return "";

		string str = "";
		do {
			Thread.Sleep(100); //sleep to let arduino start reading
			try {
				str = portFS.ReadLine().Trim();
			} catch {
				forceSensorOtherMessage = "Disconnected";
				return "";
			}
			LogB.Information("init string: " + str);
		}
		while(! str.Contains("Force_Sensor-"));

		forceSensorOtherMessageShowSeconds = secondsEnum.NO;
		forceSensorOtherMessage = str;

		return forceSensorCheckVersionMatch(str);
	}

	private string forceSensorCheckVersionDo_Port_B()
	{
		if (portFS_B.BytesToRead > 0)
			LogB.Information("Port_B check_version read possible bytes: " + portFS_B.ReadExisting());

		if(! forceSensorSendCommand_B("get_version:", Catalog.GetString ("Checking version …"), "Catched checking version"))
			return "";

		string str = forceSensorReceiveFeedback("Force_Sensor-", forceSensorPortEnum.PORT_B);
		if(str == "")
			return str;

		return forceSensorCheckVersionMatch(str.Trim());
	}

	private string forceSensorCheckVersionMatch(string str)
	{
		/*
		 * //return the version without "Force_Sensor-"
		 * return(str.Remove(0,13));
		 * //can be problematic, once detected:
		 * //init string: ^@;Force_Sensor-0.3
		 * //Version found: [r-0.3]
		 * //so now using portFS.ReadExisting() above, and also regex and not the 0,13 positions
		 */

		Match match = Regex.Match(str, @"Force_Sensor-(\d+\.\d+)");
		if(match.Groups.Count == 2)
		{
			LogB.Information("match: " + match.Groups[1].Value);
			return str = match.Groups[1].Value;
		}
		else
			return "0.3"; //if there is a problem default to 0.3
	}

	//Attention: no GTK here!!
	private void forceSensorDetectStiffness()
	{
		// 0 connect if needed
		if(! portFSOpened)
			if(! forceSensorConnect())
				return;

		forceSensorOtherMessageShowSeconds = secondsEnum.NO;

		double forceAtMin = forceSensorDetectStiffnessDo (forceSensorStiffMinCm, "A");
		//LogB.Information("forceAtMin: " + forceAtMin.ToString());
		if(forceAtMin < 0)
		{
			forceSensorOtherMessage = "Error. Force is lower than 0.";
			return;
		}

		double forceAtMax = forceSensorDetectStiffnessDo (forceSensorStiffMaxCm, "B");
		//LogB.Information("forceAtMax: " + forceAtMax.ToString());
		if(forceAtMax < 0)
		{
			forceSensorOtherMessage = "Error. Force is lower than 0.";
			return;
		}
		if(forceAtMin >= forceAtMax)
		{
			forceSensorOtherMessage = "Error. Force in second situation has to be higher.";
			return;
		}

		forceSensorOtherMessage = string.Format("Stiffness: {0} N/m", Math.Round(
					(forceAtMax-forceAtMin)/(forceSensorStiffMaxCm/100.0 - forceSensorStiffMinCm/100.0), 3));
	}
	//Attention: no GTK here!!
	private double forceSensorDetectStiffnessDo (int distanceCm, string letter)
	{
		// 1 send tare command
		if(! forceSensorSendCommand("start_capture:", Catalog.GetString ("Preparing capture …"), "Catched force capturing"))
			return -1;

		// 2 read confirmation data
		string str = "";
		do {
			Thread.Sleep(100); //sleep to let arduino start reading
			try {
				str = portFS.ReadLine();
			} catch {
				forceSensorOtherMessage = "Disconnected";
				return -1;
			}
			LogB.Information("init string: " + str);
		}
		while(! str.Contains("Starting capture"));

		//forceSensorOtherMessage = string.Format("Please pull the band/tube to {0} cm from its length without tension. You have 10 seconds.", distanceCm);
		forceSensorOtherMessage = string.Format("0-------d---A---B--\t\tPull to <b>{0}</b> \t(d-{0} = {1} cm). \t", letter, distanceCm);

		forceSensorOtherMessageShowSecondsInit = 10.999;
		forceSensorOtherMessageShowSeconds = secondsEnum.DESC;

		forceSensorValues = new ForceSensorValues();
		label_force_sensor_value_max.Text = "0";
		label_force_sensor_value.Text = "0";
		label_force_sensor_value_min.Text = "0";
		label_force_sensor_value_best_second.Text = "0";
		label_force_sensor_value_rfd.Text = "0";

		int count = 0;
		do {
			str = portFS.ReadLine();

			int time;
			double force;
			string triggerCode;
			if(! forceSensorProcessCapturedLine(str, out time, out force,
						false, out triggerCode)) //false: do not read triggers
				continue;

			forceSensorValues.TimeLast = time;
			forceSensorValues.ValueLast = force;
			forceSensorValues.SetMaxMinIfNeeded(force, time);

			count ++;
		} while (forceSensorValues.TimeLast < 10000000 && count < 1000);
		//if there is a problem on getting time, it will end at 1000 count

		forceSensorOtherMessageShowSeconds = secondsEnum.NO;
		LogB.Information("timeLast: " + forceSensorValues.TimeLast.ToString());

		LogB.Information("Calling end_capture");
		if(! forceSensorSendCommand("end_capture:", Catalog.GetString ("Ending capture …"), "Catched ending capture"))
			return -1;

		LogB.Information("Waiting end_capture");
		int notValidCommandCount = 0;
		do {
			Thread.Sleep(10);
			try {
				str = portFS.ReadLine();
			} catch {
				LogB.Information("Catched waiting end_capture feedback");
			}
			LogB.Information("waiting \"Capture ended\" string: " + str);

			//See comment "2023 Aug 3" on this file
			if (str.Contains ("Not a valid command"))
			{
				LogB.Information ("Not a valid commmand");
				notValidCommandCount ++;

				if (notValidCommandCount > 10 || ! forceSensorSendCommand("end_capture:", Catalog.GetString ("Ending capture …"), "Catched ending capture"))
					return -1;
			}
		}
		while(! str.Contains("Capture ended"));
		LogB.Information("Success: received end_capture");

		// 5 print message
		forceSensorOtherMessageShowSeconds = secondsEnum.NO;
		forceSensorOtherMessage = string.Format("max force detected: {0}", forceSensorValues.Max);

		return(forceSensorValues.Max);
	}

	//Attention: no GTK here!!
	private bool forceSensorCheckBinaryCapture()
	{
		if(! forceSensorSendCommand("get_transmission_format:", Catalog.GetString ("Checking transmission format …"), "Catched checking transmission format"))
			return false;

		string str = "";
		do {
			Thread.Sleep(100); //sleep to let arduino start reading
			try {
				str = portFS.ReadLine().Trim();
			} catch {
				forceSensorOtherMessage = "Disconnected";
				return false;
			}
			LogB.Information("init string: " + str);
		}
		while(! (str.Contains("binary") || str.Contains("text") || str.Contains("Not a valid command")) );

		forceSensorOtherMessageShowSeconds = secondsEnum.NO;
		forceSensorOtherMessage = str;

		return (str.Contains("binary"));
	}

	/*
	//Attention: no GTK here!!
	/*
	 tare+capture will not call Arduino to tare, because it would store the tare value there
	 and affect posterior normal captures
	private void forceSensorTareAndCapturePre_noGTK()
	{
		forceSensorTare();
		forceSensorCapturePre_noGTK();
	}
	*/
	//Attention: no GTK here!!
	private void forceSensorCapturePre_noGTK()
	{
		if(!Config.SimulatedCapture && ! portFSOpened)
			if(! forceSensorConnect())
				return;

		if(currentForceSensorExercise.TareBeforeCaptureAndForceResultant)
			forceSensorOtherMessage = Catalog.GetString ("The tare will soon begin");
		else
			forceSensorOtherMessage = Catalog.GetString ("Please, wait …");

		capturingForce = arduinoCaptureStatus.STARTING;
	}

	private void forceSensorCapturePre2_GTK_cameraCall()
	{
		on_button_execute_test_acceptedPre_start_camera(
				ChronoJumpWindow.WebcamStartedTestStart.FORCESENSOR);
	}

	private void forceSensorCapturePre3_GTK_cameraCalled()
	{
		button_execute_test.Sensitive = false;
		event_execute_button_finish.Sensitive = true;
		event_execute_button_cancel.Sensitive = true;
		fullscreen_button_fullscreen_contacts.Sensitive = true;
		button_force_sensor_image_save_signal.Sensitive = false;
		button_ai_model.Sensitive = false;
		forceCaptureStartMark = false;
		forceTooBigMark = false;
		forceTooBigValue = 0;
		//vscale_force_sensor.Value = 0;
		label_force_sensor_value_max.Text = "0";
		label_force_sensor_value.Text = "0";
		label_force_sensor_value_min.Text = "0";
		label_force_sensor_value_best_second.Text = "0";
		label_force_sensor_value_rfd.Text = "0";
		label_model_analyze.Text = "";
		label_model_analyze.Visible = false;

		forceProcessFinish = false;
		forceProcessCancel = false;
		forceProcessKill = false;
		forceProcessError = false;

		//To know if USB has been disconnected
		usbDisconnectedCount = 0;
		usbDisconnectedLastTime = 0;

		//initialize
		forceSensorValues = new ForceSensorValues();

		webcamStatusEnumSetStart ();

		//blank Cairo scatterplot graphs
		cairoGraphForceSensorSignal = null;
		spCairoFE = new SignalPointsCairoForceElastic ();
		paintPointsInterpolateCairo_l = new List<PointF>();

		event_execute_ButtonFinish.Clicked -= new EventHandler(on_finish_clicked);
		event_execute_ButtonFinish.Clicked += new EventHandler(on_finish_clicked);
		
		event_execute_ButtonCancel.Clicked -= new EventHandler(on_cancel_clicked);
		event_execute_ButtonCancel.Clicked += new EventHandler(on_cancel_clicked);

		if (fullscreenLastCapture)
			fullscreen_button_fullscreen_contacts.Click ();

		cairoGraphForceSensorSignalPointsShowAccuracy = true;

		forceCaptureThread = new Thread(new ThreadStart(forceSensorCaptureDo));
		GLib.Idle.Add (new GLib.IdleHandler (pulseGTKForceSensorCapture));

		if(preferences.debugMode)
			LogB.Information("Debug mode active. Logs active while force sensor capture");
		else
			LogB.Information("Debug mode inactive. Logs INactive while force sensor capture");

		//mute logs if ! debug mode
		LogB.Mute = ! preferences.debugMode;

		LogB.ThreadStart();
		forceCaptureThread.Start();
	}

	private bool readBinaryRowMark()
	{
		if(portFS.ReadByte() != 255)
			return false;

		LogB.Debug("reading mark... 255,");
		for(int j = 0; j < 3; j ++)
			if(portFS.ReadByte() != 255)
				return false;

		return true;
	}

	private List<int> readBinaryForceValues()
	{
		LogB.Debug("readed start mark Ok");
		List<int> dataRow = new List<int>();

		//read time, four bytes
		int t0 = portFS.ReadByte(); //least significative
		int t1 = portFS.ReadByte();
		int t2 = portFS.ReadByte();
		int t3 = portFS.ReadByte(); //most significative
		dataRow.Add(Convert.ToInt32(
				Math.Pow(256,3) * t3 +
				Math.Pow(256,2) * t2 +
				Math.Pow(256,1) * t1 +
				Math.Pow(256,0) * t0)); //TODO: note this should be an UInt32

		//read data, four sensors, 2 byte each
		for(int i = 0; i < 4; i ++)
		{
			int b0 = portFS.ReadByte(); //least significative
			int b1 = portFS.ReadByte(); //most significative

			int readedNum = 256 * b1 + b0;
			//care for negative values
			if(readedNum > 32768)
				readedNum = -1 * (65536 - readedNum);

			dataRow.Add(readedNum);
			//LogB.Information(string.Format("b0: {0}, b1: {1}, readedNum: {2}", b0, b1, readedNum));
		}

		return dataRow;
		//printDataRow(dataRow);
	}

	private void assignCurrentForceSensorExercise()
	{
		currentForceSensorExercise = (ForceSensorExercise) SqliteForceSensorExercise.Select (
                                false, getExerciseIDFromAnyCombo(combo_force_sensor_exercise, forceSensorComboExercisesString, false), -1, false, "")[0];
	}

	private Questionnaire questionnaire;
	private Asteroids asteroids;

	private void forceSensorCaptureDo()
	{
		//precaution
		if (currentSession == null || currentPerson == null)
			return;

		lastChangedTime = 0;

		if(! Config.SimulatedCapture && ! forceSensorSendCommand("start_capture:", Catalog.GetString ("Preparing capture …"), "Catched force capturing"))
		{
			LogB.Information("fs Error 1");
			forceProcessError = true;
			return;
		}

		string str = "";

		do {
			Thread.Sleep(100); //sleep to let arduino start reading
			try {
				if (Config.SimulatedCapture)
					str = "Starting capture";
				else
					str = portFS.ReadLine();
			} catch {
				LogB.Information("fs Error 2");
				forceProcessError = true;
				return;
			}
			LogB.Information("init string: " + str);
		}
		while(! str.Contains("Starting capture"));

		forceSensorTimeStart = DateTime.Now; //to have an active count of capture time
		forceSensorTimeStartCapture = forceSensorTimeStart; //to have same DateTime on filename and on sql datetime
		capturingForce = arduinoCaptureStatus.CAPTURING;
		//string captureComment = UtilGtk.TextViewGetCommentValidSQL(textview_force_sensor_capture_comment);
		//string captureComment = UtilGtk.TextViewGetCommentValidSQL(textview_contacts_signal_comment);

		Util.CreateForceSensorSessionDirIfNeeded (currentSession.UniqueID);

		//done at on_buttons_force_sensor_clicked()
		//assignCurrentForceSensorExercise();

		string fileNamePre = currentPerson.UniqueID + "_" + currentPerson.Name + "_" + UtilDate.ToFile(forceSensorTimeStartCapture);

		ForceSensor.CaptureOptions forceSensorCaptureOption = getForceSensorCaptureOptions();


		//fileName to save the csv
		string fileName = Util.GetForceSensorSessionDir(currentSession.UniqueID) + Path.DirectorySeparatorChar + fileNamePre + ".csv";

		//lastForceSensorFile to save the images
		lastForceSensorFile = fileNamePre;

		TextWriter writer = File.CreateText(fileName);
		writer.WriteLine("Time (micros);Force(N)");

		triggerListForceSensor = new TriggerList();

		if(preferences.forceSensorCaptureFeedbackActive == Preferences.ForceSensorCaptureFeedbackActiveEnum.PATH)
			createForceSensorCaptureInterpolateSignal();
		else
			interpolate_l = null;

		str = "";
//		bool forceSensorBinary = forceSensorBinaryCapture();

		bool readTriggers = false;

		if (! Config.SimulatedCapture)
		{
			double versionDouble = Convert.ToDouble(Util.ChangeDecimalSeparator(forceSensorFirmwareVersion));
			if(versionDouble >= Convert.ToDouble(Util.ChangeDecimalSeparator("0.5"))) //from 0.5 versions have trigger
				readTriggers = true;

			LogB.Information("forceSensor versionDouble: " + versionDouble.ToString());
			//LogB.Information("> 0.5" + (versionDouble >= Convert.ToDouble(Util.ChangeDecimalSeparator("0.5"))).ToString());
		}

		/*
		   tare+capture does a tare here in the software
		   to not call tare function on Arduino and store the tare value there
		   that will affect other normal captures
		   */
		double forceTared = 0;
		if (! Config.SimulatedCapture && currentForceSensorExercise.TareBeforeCaptureAndForceResultant)
		{
			/*
			 * do not do this, sound ends really late
			Util.PlaySound (Constants.SoundTypes.CAN_START, preferences.volumeOn, preferences.gstreamer);
			Thread.Sleep (2000); //to allow sound to be played
			*/

			forceSensorOtherMessage = Catalog.GetString ("Taring; …");
			LogB.Information("Taring starts");
			int taringSample = 0;
			int taringSamplesTotal = 50;
			double taringSum = 0;
			while(! forceProcessFinish && ! forceProcessCancel && ! forceProcessKill && ! forceProcessError &&
					taringSample < taringSamplesTotal)
			{
				int time = 0;
				double force = 0;
				string triggerCode = "";

				//non-binary capture. TODO: implement binary capture if needed in the future
				str = portFS.ReadLine();
				if(! forceSensorProcessCapturedLine(str, out time, out force,
							false, out triggerCode)) //false: do not read triggers
					continue;
				else {
					taringSum += force;
					taringSample ++;
				}
			}

			if(taringSample > 0)
			{
				/*
				   In positive a 10Kg tare, will do on a 40Kg force: force = 40 -(10) = 30 Kg
				   In negative a -10Kg tare, will do on a -40Kg force: force = -40 -(-10) = -30 Kg
				   */
				forceTared = UtilAll.DivideSafe(taringSum, taringSamplesTotal);
			}
		}

		str = "";
		int firstTime = 0;
		forceCaptureStartMark = true;
		Util.PlaySound (Constants.SoundTypes.CAN_START, preferences.volumeOn, preferences.gstreamer);

		//simulated stuff
		int simulatedSamples = 0;
		double simulatedForce = 0;
		bool simulatedForceGoingPositive = true; //to have random values with less probable change of sign (less spikes)
		Random simulatedForceRandom = new Random ();

		//questionnaire
		if (preferences.forceSensorCaptureFeedbackActive == Preferences.ForceSensorCaptureFeedbackActiveEnum.QUESTIONNAIRE)
			questionnaire = new Questionnaire (preferences.forceSensorFeedbackQuestionnaireN,
					preferences.forceSensorFeedbackQuestionnaireFile);
		else
			questionnaire = null;

		asteroids = new Asteroids (
				preferences.forceSensorFeedbackAsteroidsMax,
				preferences.forceSensorFeedbackAsteroidsMin,
				preferences.forceSensorFeedbackAsteroidsDark,
				preferences.forceSensorFeedbackAsteroidsFrequency,
				preferences.forceSensorFeedbackShotsFrequency,
				true, -1); //micros, recordingTime

		//LogB.Information("pre bucle");
		//LogB.Information(string.Format("forceProcessFinish: {0}, forceProcessCancel: {1}, forceProcessError: {2}", forceProcessFinish, forceProcessCancel, forceProcessError));
		while(! forceProcessFinish && ! forceProcessCancel && ! forceProcessKill && ! forceProcessError)
		{
			LogB.Information("at bucle");
			int time = 0;
			double force = 0;
			string triggerCode = "";

			if(forceSensorBinaryCapture)
			{
				if(! readBinaryRowMark())
					continue;
				LogB.Information("at bucle2");

				List<int> binaryReaded = readBinaryForceValues();
				time = binaryReaded[0];
				force = binaryReaded[1]; //note right now we are only reading 1st sensor
			}
			else {
				if (Config.SimulatedCapture)
				{
					Thread.Sleep(6); //6.25 ms 	// 160 Hz -> 1/160 -> 0,00625
					time = Convert.ToInt32 (6.25 * 1000 * simulatedSamples ++);
					double forceChange = (simulatedForceRandom.NextDouble () *3) -1.5;
					if (simulatedForceGoingPositive)
						forceChange += .5;
					else
						forceChange -= .5;

					simulatedForce += forceChange;
					force = simulatedForce;
					simulatedForceGoingPositive = (forceChange >= 0);
				} else {
					str = portFS.ReadLine();
					//LogB.Information ("forceSensor captured str: " + str);
					if(! forceSensorProcessCapturedLine(str, out time, out force,
								readTriggers, out triggerCode))
						continue;
				}
			}

			if(currentForceSensorExercise.TareBeforeCaptureAndForceResultant && forceTared != 0)
			{
				LogB.Information(string.Format("forceTared: {0}, force: {1}, forceFixed: {2}",
							forceTared, force, force - forceTared));
				force -= forceTared;
			}

			//measurement does not start at 0 time. When we start receiving data, mark this as firstTime
			if(firstTime == 0)
				firstTime = time;

			//use this to have time starting at 0
			time -= firstTime;


			LogB.Information("at bucle4");
			LogB.Information("at bucle5");
			//if RCA or button at the moment just print it here (now that time has been corrected using firstTime)
			if( readTriggers && (triggerCode == "r" || triggerCode == "R") )
			{
				LogB.Information(string.Format("At: {0}, triggerCode: {1}",
							time.ToString(), ForceSensor.ReadTrigger(triggerCode)));

				Trigger trigger;
				if(triggerCode == "r")
					trigger = new Trigger(Trigger.Modes.FORCESENSOR, time, false);
				else //if(triggerCode == "R")
					trigger = new Trigger(Trigger.Modes.FORCESENSOR, time, true);

				if(! triggerListForceSensor.NewSameTypeThanBefore(trigger) &&
						! triggerListForceSensor.IsSpurious(trigger, TriggerList.Type3.BOTH, 50000))
					triggerListForceSensor.Add(trigger);

				continue;
			}

			LogB.Information("at bucle6");
			//LogB.Information(string.Format("time: {0}, force: {1}", time, force));
			//forceCalculated have abs or inverted
			//this has to be after readTriggers, because if this "sample" is a trigger we do not have force
			double forceCalculated = ForceSensor.CalculeForceResultantIfNeeded (force, forceSensorCaptureOption,
					currentForceSensorExercise, currentPersonSession.Weight);

			//if(forceSensorCaptureOption != ForceSensor.CaptureOptions.NORMAL)
			//	LogB.Information(string.Format("with abs or inverted flag: time: {0}, force: {1}", time, forceCalculated));

			//force decimal is . since 2.0.3 Before was culture specific.
			writer.WriteLine(time.ToString() + ";" + Util.ConvertToPoint(force)); //on file force is stored without flags

			forceSensorValues.TimeLast = time;
			forceSensorValues.ValueLast = forceCalculated;

			forceSensorValues.SetMaxMinIfNeeded(forceCalculated, time);

			//done in two phases in order to avoid having last element empty
			PointF pNow =  new PointF (time, forceCalculated);
			spCairoFE.Force_l.Add (pNow);


			//LogB.Information (string.Format ("paintPointsInterpolateCairo_l null: {0}, interpolate_l null: {1}",
			//			(paintPointsInterpolateCairo_l == null), (interpolate_l == null) ));

			if (interpolate_l != null)
			{
				int currentYpos = 0;
				if (paintPointsInterpolateCairo_l == null)
					paintPointsInterpolateCairo_l = new List<PointF>();
				else if (paintPointsInterpolateCairo_l.Count > 0)
					currentYpos = paintPointsInterpolateCairo_l.Count -1;

				if (currentYpos >= interpolate_l.Count)
					currentYpos = currentYpos % interpolate_l.Count;

				//LogB.Information (string.Format ("paintPointsInterpolateCairo_l.Count: {0}, interpolate_l.Count: {1}",
				//			paintPointsInterpolateCairo_l.Count, interpolate_l.Count ));

				paintPointsInterpolateCairo_l.Add (new PointF (
							time, interpolate_l[currentYpos].Y));
			}

			LogB.Information("at bucle8");

			//changeSlideIfNeeded(time, force);
		}

		if(forceProcessKill)
			LogB.Information("User killed the software");
		else {
			//LogB.Information(string.Format("forceProcessFinish: {0}, forceProcessCancel: {1}, forceProcessError: {2}", forceProcessFinish, forceProcessCancel, forceProcessError));
			LogB.Information("Calling end_capture");
			if (! Config.SimulatedCapture && ! forceSensorSendCommand("end_capture:", Catalog.GetString ("Ending capture …"), "Catched ending capture"))
			{
				forceProcessError = true; LogB.Information("fs Error 3");
				capturingForce = arduinoCaptureStatus.STOP;
				Util.FileDelete(fileName);
				return;
			}

			LogB.Information("Waiting end_capture");
			int notValidCommandCount = 0;
			do {
				Thread.Sleep(10);
				try {
					if (Config.SimulatedCapture)
						str = "Capture ended";
					else
						str = portFS.ReadLine();
				} catch {
					LogB.Information("Catched waiting end_capture feedback");
				}
				LogB.Information("waiting \"Capture ended\" string: " + str);

				//2023 Aug 3: sometimes Arduino looses some chars. It seems only happens with this command because Arduino will be busy capturing
				//instead of "end_capture:" arrived "end_cture:" (found 2 times) "end_capte:", "end_ture:", "end_caure:"
				if (str.Contains ("Not a valid command"))
				{
					LogB.Information ("Not a valid commmand");
					notValidCommandCount ++;

					if (notValidCommandCount > 10 ||
							(! Config.SimulatedCapture && ! forceSensorSendCommand("end_capture:", Catalog.GetString ("Ending capture …"), "Catched ending capture")))
					{
						forceProcessError = true; LogB.Information("fs Error 3b");
						capturingForce = arduinoCaptureStatus.STOP;
						Util.FileDelete(fileName);
						return;
					}
				}
			}
			while(! str.Contains("Capture ended"));
			LogB.Information("Success: received end_capture");
		}

		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();

		capturingForce = arduinoCaptureStatus.STOP;

		if(forceProcessCancel || forceProcessKill || forceProcessError)
			Util.FileDelete(fileName);
		else {
			forceSensorValues.BestSecond = getMaxAvgForce1s ();
			//forceSensorValues.BestRFD = getBestAvgRFD ();

			//call graph
			File.Copy(fileName, UtilEncoder.GetmifCSVFileName(), true); //can be overwritten
			lastForceSensorFullPath = fileName;
			capturingForce = arduinoCaptureStatus.COPIED_TO_TMP;
		}
	}

	private double getMaxAvgForce1s ()
	{
		double maxAvgForce1s = -1; //default value
		GetMaxAvgInWindow miw = new GetMaxAvgInWindow (spCairoFE.Force_l,
				0, spCairoFE.Force_l.Count -1, 1); //1s

		if (miw.Error == "")
			maxAvgForce1s = miw.Max;

		return maxAvgForce1s;
	}

	private bool forceSensorProcessCapturedLine (string str,
			out int time, out double force,
			bool readTriggers, out string triggerCode)
	{
		time = 0;
		force = 0;
		triggerCode = "";

		//LogB.Information("strA: " + str);
		//check if there is one and only one ';'
		if( ! (str.Contains(";") && str.IndexOf(";") == str.LastIndexOf(";")) )
			return false;

		string [] strFull = str.Split(new char[] {';'});
		//LogB.Information("strB: " + str);

		//LogB.Information("time: " + strFull[0]);
		if(! Util.IsNumber(Util.ChangeDecimalSeparator(strFull[0]), true))
			return false;

		if(Util.IsNumber(Util.ChangeDecimalSeparator(strFull[1]), true))
		{
			//LogB.Information("force: " + strFull[1]);
		}
		else if(readTriggers)
		{
			time = Convert.ToInt32(strFull[0]);
			triggerCode = strFull[1].Trim(); //now is coming from Arduino with an enter
			return true;
		} else
			return false;

		time = Convert.ToInt32(strFull[0]);

		//bad tare or bad calibration or too much force
		if (Math.Abs (Convert.ToDouble(Util.ChangeDecimalSeparator(strFull[1]))) > 20000) // 20000 N (2000 Kg) Chronojump force sensors are up to 5000 but we have special version with 20000
		{
			LogB.Information ("Error. Force too big: " + strFull[1]);
			forceTooBigMark = true;
			forceTooBigValue = Convert.ToDouble (Util.ChangeDecimalSeparator(strFull[1]));
			forceProcessError = true;

			return false;
		}

		force = Convert.ToDouble(Util.ChangeDecimalSeparator(strFull[1]));

		return true;
	}

	private bool pulseGTKForceSensorCapture ()
	{
LogB.Information(" fs A ");
		if(forceCaptureThread == null)
		{
			Thread.Sleep (25);
			return true;
		}

LogB.Information(" fs B ");
		//LogB.Information(capturingForce.ToString())
		if(! forceCaptureThread.IsAlive || forceProcessFinish || forceProcessCancel || forceProcessError)
		{
LogB.Information(" fs C ");
			LogB.Information(string.Format(
						"! forceCaptureThread.IsAlive: {0}, forceProcessFinish: {1}," +
						"forceProcessCancel: {2}, forceProcessError: {3}",
						! forceCaptureThread.IsAlive, forceProcessFinish, forceProcessCancel, forceProcessError));

			button_video_play_this_test_contacts.Sensitive = false;
			if(forceProcessFinish)
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

				if(capturingForce != arduinoCaptureStatus.COPIED_TO_TMP)
				{
					Thread.Sleep (25); //Wait file is copied
					return true;
				}
				else
				{
					event_execute_label_message.Text = "Saved.";

					double stiffness;
					string stiffnessString;
					getStiffnessAndStiffnessStringFromSQL(out stiffness, out stiffnessString);

					currentForceSensor = new ForceSensor(-1, currentPerson.UniqueID, currentSession.UniqueID,
							currentForceSensorExercise.UniqueID, getForceSensorCaptureOptions(),
							ForceSensor.AngleUndefined, getLaterality(false),
							Util.GetLastPartOfPath(lastForceSensorFile + ".csv"), //filename
							Util.MakeURLrelative(Util.GetForceSensorSessionDir(currentSession.UniqueID)), //url
							UtilDate.ToFile(forceSensorTimeStartCapture),
							"", //on capture cannot store comment (comment has to be written after),
							"", //videoURL
							stiffness, stiffnessString,
							forceSensorValues.Max,
							forceSensorValues.BestSecond,
							currentForceSensorExercise.Name);

					currentForceSensor.UniqueID = currentForceSensor.InsertSQL(false);
					triggerListForceSensor.SQLInsert(currentForceSensor.UniqueID);
					//showForceSensorTriggers (); TODO until know where to put it

					if (radio_ai_2sets.Active)
						radio_ai_cd.Sensitive = true;

					if (webcamStatusEnum == WebcamStatusEnum.STOPPED)
					{
						bool success = webcamEndingSaveFile (Constants.TestTypes.FORCESENSOR, currentForceSensor.UniqueID);
						if (success)
						{
							//add the videoURL to SQL
							currentForceSensor.VideoURL = Util.GetVideoFileName(currentSession.UniqueID,
									Constants.TestTypes.FORCESENSOR,
									currentForceSensor.UniqueID);
							currentForceSensor.UpdateSQL(false);
						}
						webcamRestoreGui (success);
					}

					Thread.Sleep (250); //Wait a bit to ensure is copied
					sensitiveLastTestButtons(true);
					contactsShowCaptureDoingButtons(false);

					forceSensorDoSignalGraphPlot ();

					//do not calculate RFD until analyze button there is clicked
					//forceSensorDoRFDGraph();

					forceSensorZoomDefaultValues();
					forceSensorPrepareGraphAI ();

					hbox_force_sensor_analyze_ai_sliders_and_buttons.Sensitive = true;

					button_contacts_exercise_close_and_recalculate.Sensitive = true;
					button_contacts_delete_selected.Sensitive = true;
					button_force_sensor_image_save_signal.Sensitive = true;
					hbox_force_general_analysis.Sensitive = true;
					button_ai_model_options_close_and_analyze.Sensitive = true;
					button_ai_model.Sensitive = true;

					if( configChronojump.Exhibition &&
							( configChronojump.ExhibitionStationType == ExhibitionTest.testTypes.FORCE_ROPE ||
							  configChronojump.ExhibitionStationType == ExhibitionTest.testTypes.FORCE_SHOT ) )
						SqliteJson.UploadExhibitionTest(getExhibitionTestFromGui(configChronojump.ExhibitionStationType, forceSensorValues.Max));

					//on resultant (projected) recalculate at end (to manage correctly speed and accel smoothed that affects body mass
					if(currentForceSensorExercise.ComputeAsElastic)
					{
						force_sensor_recalculate();
						event_execute_label_message.Text = "Converted to exerted force.";
					}
				}
			} else if(forceProcessCancel || forceProcessError)
			{
				if (forceProcessError)
					Util.PlaySound (Constants.SoundTypes.BAD, preferences.volumeOn, preferences.gstreamer);

				//stop the camera (and do not save)
				if (webcamStatusEnum == WebcamStatusEnum.RECORDING)
				{
					webcamEndingRecordingCancel ();
					webcamRestoreGui (false);
				}

				sensitiveLastTestButtons(false);
				contactsShowCaptureDoingButtons(false);

				if (forceTooBigMark)
					event_execute_label_message.Text = string.Format (
							Catalog.GetString ("Error. Force too big: {0} N"), forceTooBigValue) + " " +
						Catalog.GetString ("Bad calibration or too much force for this sensor.") +
						" " + Catalog.GetString ("Stopped.");
				else if (forceProcessCancel)
					event_execute_label_message.Text = "Cancelled.";
				else {
					event_execute_label_message.Text = forceSensorNotConnectedString;
					button_detect_show_hide (true); // show the detect big button
				}

				button_force_sensor_image_save_signal.Sensitive = false;
				button_ai_model.Sensitive = false;
				button_ai_model_save_image.Sensitive = false;
				button_force_sensor_image_save_rfd_manual.Sensitive = false;
				button_contacts_exercise_close_and_recalculate.Sensitive = false;
				button_contacts_delete_selected.Sensitive = false;
			}
			else
					event_execute_label_message.Text = "";

			LogB.ThreadEnding();

			/*
			 * ensure forceCaptureThread is ended:
			 * called: portFS.WriteLine("end_capture:");
			 * and received feedback from device
			 */
			while(forceCaptureThread.IsAlive)
				Thread.Sleep (250);
LogB.Information(" fs D ");

			// if captured in vertical, make null the graph to be drawn again (horizontal with axis ok)
			if (! preferences.signalDirectionHorizontal)
				cairoGraphForceSensorSignal = null;

			//1) unMute logs if preferences.muteLogs == false
			LogB.Mute = preferences.muteLogs;
			if(! preferences.muteLogs)
				LogB.Information("muteLogs INactive. Logs active active again");

			LogB.ThreadEnded(); 

			forceSensorButtonsSensitive(true);

			//finish, cancel: sensitive = false
			hideButtons();

			restTime.AddOrModify(currentPerson.UniqueID, currentPerson.Name, true);
			updateRestTimes();

			return false;
		}

LogB.Information(" fs E ");

		if (forceCaptureStartMark)
		{
			string str = "Capturing" + " (" + Util.TrimDecimals(DateTime.Now.Subtract(forceSensorTimeStart).TotalSeconds, 0) + " s)";
			if (notebook_start.CurrentPage == Convert.ToInt32 (notebook_start_pages.FULLSCREENCAPTURE))
			{
				fullscreen_label_message.Text = str;
				fullscreen_label_message.UseMarkup = true;
			} else {
				event_execute_label_message.Text = str;
				event_execute_label_message.UseMarkup = true;
			}
		}

LogB.Information(" fs F ");

		if(capturingForce == arduinoCaptureStatus.CAPTURING)
		{
LogB.Information(" fs G ");
			//do this if we are not on tare and TareBeforeCapture
			if( ! (currentForceSensorExercise.TareBeforeCaptureAndForceResultant && ! forceCaptureStartMark) )
			{

				label_force_sensor_value_max.Text = string.Format("{0:0.##}", forceSensorValues.Max);
				label_force_sensor_value_min.Text = string.Format("{0:0.##}", forceSensorValues.Min);
				label_force_sensor_value.Text = string.Format("{0:0.##}", forceSensorValues.ValueLast);
				label_force_sensor_value_best_second.Text = string.Format("{0:0.##}", forceSensorValues.BestSecond);
				label_force_sensor_value_rfd.Text = string.Format("{0:0.##}", forceSensorValues.BestRFD);


				LogB.Information(" fs H ");
				//------------------- realtime graph -----------------
				//if(redoingPoints || fscPoints == null || fscPoints.Points == null || force_capture_drawingarea == null)
				//	if (force_capture_drawingarea == null)
				//	return true;
			}

LogB.Information(" fs H2 ");
			if(usbDisconnectedLastTime == forceSensorValues.TimeLast)
			{
				usbDisconnectedCount ++;

				/* this was 20 for some years, but some electronics are slower on start sending data
				   and then Disconnected happens. So changed to 1000 it makes the Disconnected check
				   do not make fail the capture, and just if disconnected, the message appears few seconds later.

				   Changed also to 1000 while we wait first point and 40 while we have data.
				   Then if disconnected while capture error message and thread end is fast
				 */
				int disconnectedThreshold = 1000;
				if (spCairoFE.Force_l.Count > 20)
					disconnectedThreshold = 40;

				if(usbDisconnectedCount >= disconnectedThreshold)
				{
					event_execute_label_message.Text = "Disconnected!";
					forceProcessError = true;
					LogB.Information("fs Error 4." +
						string.Format(" captured {0} samples", spCairoFE.Force_l.Count));
					return true;
				}
			}
			else
			{
				usbDisconnectedLastTime = forceSensorValues.TimeLast;
				usbDisconnectedCount = 0;
			}

			//if taring at TareBeforeCapture, just show message
			if( currentForceSensorExercise.TareBeforeCaptureAndForceResultant && ! forceCaptureStartMark)
			{
				event_execute_label_message.Text = forceSensorOtherMessage; //"Taring ..."
				Thread.Sleep(25);
				return true;
			}

LogB.Information(" fs I ");
LogB.Information(" fs J ");

			if(configChronojump.LowCPU)
				event_execute_label_message.Text = "Disabled real time graph on this device";
			else {
				//updateForceSensorCaptureSignalCairo (false); //TODO: if this is commented, then RAM does not increase
				//much better like the folowing lines, but note that memory and CPU usage also increase. When Chronojump is closed all memory is returned. Check where is the leak. Leak happens in X11 (not in Wayland)
				if (notebook_start.CurrentPage == Convert.ToInt32 (notebook_start_pages.FULLSCREENCAPTURE))
					fullscreen_capture_drawingarea_cairo.QueueDraw ();
				else
					force_capture_drawingarea_cairo.QueueDraw ();
			}
LogB.Information(" fs Q ");
		}
LogB.Information(" fs R ");


		Thread.Sleep (25);
		//LogB.Information(" ForceSensor:"+ forceCaptureThread.ThreadState.ToString());
		return true;
	}

	private string [] getForceSensorLoadColumnsString ()
	{
		int all = 10;
		if (current_mode == Constants.Modes.FORCESENSORELASTIC)
			all = 11;

		string [] colStr = new String [all];
		int i = 0;
		colStr [i++] = Catalog.GetString("ID");
		colStr [i++] = Catalog.GetString("Set");
		colStr [i++] = Catalog.GetString("Exercise");
		if (current_mode == Constants.Modes.FORCESENSORELASTIC)
			colStr [i++] = Catalog.GetString("Elastic") + " (N/m)";
		colStr [i++] = Catalog.GetString("Capture option");
		colStr [i++] = Catalog.GetString("Laterality");
		colStr [i++] = Catalog.GetString("Max force") + " (" + Catalog.GetString("Raw data") + ") (N)";
		colStr [i++] = string.Format(Catalog.GetString("Max AVG Force in {0} s"), 1) + " (N)";
		colStr [i++] = Catalog.GetString("Date");
		colStr [i++] = Catalog.GetString("Video");
		colStr [i++] = Catalog.GetString("Comment");

		return colStr;
	}

	private ArrayList getForceSensorLoadSetsDataPrint (int personID, int sessionID)
	{
		int elastic = ForceSensor.GetElasticIntFromMode (current_mode);
		List<ForceSensor> data = SqliteForceSensor.Select(false, -1, personID, sessionID, elastic);

		ArrayList dataPrint = new ArrayList();
		int count = 1;
		foreach (ForceSensor fs in data)
			dataPrint.Add (fs.ToStringArray (count++, current_mode));

		return dataPrint;
	}

	//this is called when user clicks on load signal
	//very based on: on_encoder_load_signal_clicked () future have some inheritance
	private void force_sensor_load (bool canChoosePersonAndSession)
	{
		string [] colStr = getForceSensorLoadColumnsString ();
		ArrayList dataPrint = getForceSensorLoadSetsDataPrint (currentPerson.UniqueID, currentSession.UniqueID);

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
		//}
		*/

		string title = string.Format (Catalog.GetString ("Select set of athlete {0} on this session."), currentPerson.Name);

		if (canChoosePersonAndSession)
		{
			ArrayList a3 = new ArrayList ();
			a3.Add (Constants.GenericWindowShow.GRIDPERSONSESSION); a3.Add (true); a3.Add ("");
			bigArray.Add (a3);

			title = Catalog.GetString ("Select set to compare");
		}

		genericWin = GenericWindow.Show (Catalog.GetString("Load"), false, title , bigArray);

		if (canChoosePersonAndSession)
		{
			genericWin.SetGridPersonSession (currentPerson, currentSession);

			//do not allow to edit when can change person/session
			genericWin.SetTreeview (colStr, false, dataPrint, new ArrayList(), GenericWindow.EditActions.NONE, true);

			genericWin.FakeButtonNeedUpdateTreeView.Clicked -= new EventHandler (on_force_sensor_load_signal_update_treeview);
			genericWin.FakeButtonNeedUpdateTreeView.Clicked += new EventHandler (on_force_sensor_load_signal_update_treeview);
		} else {
			genericWin.SetTreeview (colStr, false, dataPrint, new ArrayList(), GenericWindow.EditActions.EDITPLAYDELETE, true);

			//find all persons in current session
			ArrayList personsPre = SqlitePersonSession.SelectCurrentSessionPersons(currentSession.UniqueID,
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
		if(currentForceSensor != null)
			genericWin.SelectRowWithID(0, currentForceSensor.UniqueID); //colNum, id

		genericWin.VideoColumn = 7;
		genericWin.CommentColumn = 8;

		genericWin.ShowButtonCancel(true);
		genericWin.SetButtonAcceptLabel(Catalog.GetString("Load"));
		genericWin.SetButtonCancelLabel(Catalog.GetString("Close"));
		genericWin.SetButtonAcceptSensitive(false);
		genericWin.Button_accept.Clicked += new EventHandler(on_force_sensor_load_signal_accepted);

		if (! canChoosePersonAndSession)
		{
			genericWin.Button_row_play.Clicked += new EventHandler(on_force_sensor_load_signal_row_play);
			genericWin.Button_row_edit.Clicked += new EventHandler(on_force_sensor_load_signal_row_edit);
			genericWin.Button_row_edit_apply.Clicked += new EventHandler(on_force_sensor_load_signal_row_edit_apply);
			genericWin.Button_row_delete.Clicked += new EventHandler(on_force_sensor_load_signal_row_delete_prequestion);
		}

		genericWin.ShowNow();
	}

	private void on_force_sensor_load_signal_accepted (object o, EventArgs args)
	{
		LogB.Information("on force sensor load signal accepted");
		genericWin.Button_accept.Clicked -= new EventHandler(on_force_sensor_load_signal_accepted);

		int uniqueID = genericWin.TreeviewSelectedRowID();

		int elastic = ForceSensor.GetElasticIntFromMode (current_mode);

		int personID = currentPerson.UniqueID;
		int sessionID = currentSession.UniqueID;
		if (genericWin.UseGridPersonSession)
		{
			personID = genericWin.GetPersonIDFromGui ();
			sessionID = genericWin.GetSessionIDFromGui ();
		}

		genericWin.HideAndNull();

		ForceSensor fs = (ForceSensor) SqliteForceSensor.Select (false, uniqueID, personID, sessionID, elastic)[0];
		if(fs == null)
		{
			new DialogMessage(Constants.MessageTypes.WARNING, Constants.FileNotFoundStr());
			return;
		}

		if(! Util.FileExists(fs.FullURL))
		{
			new DialogMessage(Constants.MessageTypes.WARNING, Constants.FileNotFoundStr());
			return;
		}
		List<string> contents = Util.ReadFileAsStringList(fs.FullURL);
		if(contents.Count < 3)
		{
			new DialogMessage(Constants.MessageTypes.WARNING, Constants.FileEmptyStr());
			return;
		}

		// trying on _cd to only update the graph
		if (radio_ai_2sets.Active && radio_ai_cd.Active)
		{
			lastForceSensorFullPath_CD = fs.FullURL;
			LogB.Information ("lastForceSensorFullPath_CD is: " + lastForceSensorFullPath_CD);

			currentForceSensor_CD = fs;
			currentForceSensorExercise_CD = (ForceSensorExercise) SqliteForceSensorExercise.Select (
                                false, fs.ExerciseID, -1, false, "")[0];

			//TODO: maybe need to wait to ensure is copied
			File.Copy (lastForceSensorFullPath_CD, UtilEncoder.GetmifCSVFileName_CD (), true); //can be overwritten
			forceSensorDoSignalGraphReadFile (false, fs.CaptureOption); //cd

			signalSuperpose2SetsCDPersonName = "";
			if (personID != currentPerson.UniqueID)
				signalSuperpose2SetsCDPersonName = SqlitePerson.SelectAttribute (personID, "name");

			forceSensorPrepareGraphAI ();
			updateForceSensorAICairo (true);

			button_ai_move_cd_pre_set_sensitivity ();

			return;
		}

		currentForceSensor = fs;
		lastForceSensorFile = Util.RemoveExtension(fs.Filename);
		lastForceSensorFullPath = fs.FullURL;
		LogB.Information ("lastForceSensorFullPath is: " + lastForceSensorFullPath);
		LogB.Information("lastForceSensorFullPath: " + lastForceSensorFullPath);

		if (radio_ai_2sets.Active)
			radio_ai_cd.Sensitive = true;

		combo_force_sensor_exercise.Active = UtilGtk.ComboMakeActive(combo_force_sensor_exercise, fs.ExerciseName);
		setForceSensorCaptureOptions(fs.CaptureOption);

		setLaterality(fs.Laterality);
		//textview_force_sensor_capture_comment.Buffer.Text = fs.Comments;
		textview_contacts_signal_comment.Buffer.Text = fs.Comments;

		assignCurrentForceSensorExercise();

		// stiffness 1: change button_force_sensor_stiffness
		image_button_force_sensor_stiffness_problem.Visible = false;
		if(currentForceSensorExercise.ComputeAsElastic)
		{
			setStiffnessButtonLabel(fs.Stiffness);
			frame_force_sensor_elastic.Visible = true;

			// stiffness 2: update elastic bands table
			if(! ForceSensorElasticBand.UpdateBandsStatusToSqlite (
						SqliteForceSensorElasticBand.SelectAll(false, false), fs.StiffnessString, fs.Stiffness) )
			{
				//TODO: improve this message and apply any needed change on ForceSensorElasticBandsWindow
				//when is definitive mark to be translated with Catalog
				new DialogMessage(Constants.MessageTypes.WARNING,
						Catalog.GetString("Loaded set used elastic bands removed from database or with changed values.") + "\n\n" +
						Catalog.GetString("Stiffness calculation is correct but stiffness configuration window will not be able to match elastic bands and total stiffness."));

				image_button_force_sensor_stiffness_problem.Visible = true;
			}
		} else
		{
			label_button_force_sensor_stiffness.Text = "0";
			frame_force_sensor_elastic.Visible = false;
		}
		box_force_sensor_capture_magnitudes.Visible = currentForceSensorExercise.ComputeAsElastic;
		box_force_sensor_analyze_magnitudes.Visible = currentForceSensorExercise.ComputeAsElastic;

		//triggers
		triggerListForceSensor = new TriggerList(
				SqliteTrigger.Select(
					false, Trigger.Modes.FORCESENSOR,
					Convert.ToInt32(currentForceSensor.UniqueID))
				);
		//showForceSensorTriggers (); TODO until know where to put it

		/*
		   do not interpolate signal at load
		if(preferences.forceSensorCaptureFeedbackActive == Preferences.ForceSensorCaptureFeedbackActiveEnum.PATH)
			createForceSensorCaptureInterpolateSignal();
		else
		*/
			interpolate_l = null;

		cairoGraphForceSensorSignalPointsShowAccuracy = false;
		forceSensorCopyTempAndDoGraphs(forceSensorGraphsEnum.SIGNAL);
		//image_ai_model_graph.Sensitive = false; //unsensitivize the RFD image (can contain info of previous data)
		notebook_ai_top.CurrentPage = Convert.ToInt32(notebook_ai_top_pages.CURRENTSETSIGNAL);

		button_video_play_this_test_contacts.Sensitive = (fs.VideoURL != "");
		sensitiveLastTestButtons(true);

		forceSensorZoomDefaultValues();
		forceSensorPrepareGraphAI ();
		updateForceSensorAICairo (true);

		//event_execute_label_message.Text = "Loaded: " + Util.GetLastPartOfPath(filechooser.Filename);
		event_execute_label_message.Text = Catalog.GetString("Loaded:") + " " + lastForceSensorFile;

		hbox_force_sensor_analyze_ai_sliders_and_buttons.Sensitive = true;

		button_contacts_exercise_close_and_recalculate.Sensitive = true;
		hbox_force_general_analysis.Sensitive = true;
		button_ai_model_options_close_and_analyze.Sensitive = true;

		//notebook_ai_top.CurrentPage = Convert.ToInt32(notebook_ai_top_pages.CURRENTSETSIGNAL);
		//change radio and will change also notebook:
		radio_signal_analyze_current_set.Active = true;
	}

	private void on_force_sensor_load_signal_update_treeview (object o, EventArgs args)
	{
		LogB.Information ("on_force_sensor_load_signal_update_treeview");

		string [] colStr = getForceSensorLoadColumnsString ();
		ArrayList dataPrint = getForceSensorLoadSetsDataPrint (
				genericWin.GetPersonIDFromGui (), genericWin.GetSessionIDFromGui ());

		genericWin.SetTreeview (colStr, false, dataPrint, new ArrayList(), GenericWindow.EditActions.NONE, true);
	}

	protected void on_force_sensor_load_signal_row_play (object o, EventArgs args)
	{
		LogB.Information("row play at load signal");
		LogB.Information(genericWin.TreeviewSelectedUniqueID.ToString());
		playVideo(Util.GetVideoFileName(currentSession.UniqueID,
				Constants.TestTypes.FORCESENSOR, genericWin.TreeviewSelectedUniqueID));
	}

	protected void on_force_sensor_load_signal_row_edit (object o, EventArgs args) {
		LogB.Information("row edit at load signal");
		LogB.Information(genericWin.TreeviewSelectedUniqueID.ToString());
		genericWin.ShowEditRow(true);
	}

	protected void on_force_sensor_load_signal_row_edit_apply (object o, EventArgs args)
	{
		LogB.Information("row edit apply at load signal. Opening db:");

		Sqlite.Open();

		//1) select set
		int setID = genericWin.TreeviewSelectedUniqueID;
		int elastic = ForceSensor.GetElasticIntFromMode (current_mode);
		ForceSensor fs = (ForceSensor) SqliteForceSensor.Select(true, setID, -1, -1, elastic)[0];

		//2) if changed comment, update SQL, and update treeview
		//first remove conflictive characters
		string comment = Util.RemoveTildeAndColonAndDot(genericWin.EntryEditRow);
		if(comment != fs.Comments)
		{
			fs.Comments = comment;
			fs.UpdateSQLJustComments(true);

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
			ForceSensor fsChangedPerson = fs.ChangePerson(idName);
			fsChangedPerson.UpdateSQL(true);
			genericWin.RemoveSelectedRow();
			genericWin.SetButtonAcceptSensitive(false);
		}

		genericWin.ShowEditRow(false);
		genericWin.SensitiveEditDeleteIfSelected();

		//remove signal from gui just in case the edited signal is the same we have loaded
		//removeSignalFromGuiBecauseDeletedOrCancelled();
		blankForceSensorInterface();

		Sqlite.Close();
	}

	private void createForceSensorCaptureInterpolateSignal()
	{
		/*
		//create random forces from 1200 to 2400, 4000 ms aprox, 4 points (4000/1000)
		//between each of the points interpolation will happen
		InterpolateSignal interpolateS = new InterpolateSignal(1200, 2400, 4000, 1000);
		*/

		//need at least 3 masters
		if(preferences.forceSensorFeedbackPathMasters < 3)
		{
			interpolate_l = null;
			return;
		}

		/*
		3rd param on InterpolateSignal is maxx.
		points = maxx / step , so: maxx = points * step
		*/
		int maxx = preferences.forceSensorFeedbackPathMasters *
			preferences.forceSensorFeedbackPathMasterSeconds;

		InterpolateSignal interpolateS = new InterpolateSignal(
				preferences.forceSensorFeedbackPathMin,
				preferences.forceSensorFeedbackPathMax,
				maxx * 1000,
				preferences.forceSensorFeedbackPathMasterSeconds * 1000
				);

		interpolate_l = interpolateS.GetCubicInterpolated();

		/*
		LogB.Information("interpolate_l: ");
		for(int i=0; i < interpolate_l.Count; i++)
			LogB.Information(interpolate_l[i].ToString());
			*/
	}

	// ----start of forceSensorDeleteTest stuff -------

	protected void on_force_sensor_load_signal_row_delete_prequestion (object o, EventArgs args)
	{
		if(preferences.askDeletion) {
			ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString(
						"Are you sure you want to delete this set?"), "", "");
			confirmWin.Button_accept.Clicked += new EventHandler(on_force_sensor_load_signal_row_delete);
		} else
			on_force_sensor_load_signal_row_delete (o, args);
	}
	protected void on_force_sensor_load_signal_row_delete (object o, EventArgs args)
	{
		LogB.Information("row delete at load set");

		int setID = genericWin.TreeviewSelectedUniqueID;
		LogB.Information(setID.ToString());

		//if it's current set use the delete set from the gui interface that updates gui
		if(currentForceSensor != null && setID == Convert.ToInt32(currentForceSensor.UniqueID))
			force_sensor_delete_current_test_accepted(o, args);
		else {
			int elastic = ForceSensor.GetElasticIntFromMode (current_mode);
			ForceSensor fs = (ForceSensor) SqliteForceSensor.Select(false, setID, -1, -1, elastic)[0];
			forceSensorDeleteTestDo(fs);

			//genericWin selected row is deleted, unsensitive the "load" button
			genericWin.SetButtonAcceptSensitive(false);
		}
		genericWin.Delete_row_accepted();
	}

	private void force_sensor_delete_current_test_pre_question()
	{
		//solve possible gui problems
		if(currentForceSensor == null || currentForceSensor.UniqueID == -1)
		{
			new DialogMessage(Constants.MessageTypes.WARNING, "Test does not exists. Cannot be deleted");
			return;
		}

		if(preferences.askDeletion) {
			ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString(
						"Are you sure you want to delete this set?"), "", "");
			confirmWin.Button_accept.Clicked += new EventHandler(force_sensor_delete_current_test_accepted);
		} else
			force_sensor_delete_current_test_accepted(new object(), new EventArgs());
	}
	private void force_sensor_delete_current_test_accepted(object o, EventArgs args)
	{
		forceSensorDeleteTestDo (currentForceSensor);

		//empty forceSensor GUI (this also assigns -1 to currentForceSensor)
		blankForceSensorInterface();
	}

	private void forceSensorDeleteTestDo(ForceSensor fs)
	{
		//int uniqueID = currentForceSensor.UniqueID;
		SqliteForceSensor.DeleteSQLAndFiles (false, fs); //deletes also the .csv
	}

	// ---- end of forceSensorDeleteTest stuff -------


	private void force_sensor_recalculate ()
	{
		if(! Util.FileExists(lastForceSensorFullPath))
		{
			new DialogMessage(Constants.MessageTypes.WARNING, Constants.FileNotFoundStr());
			return;
		}

		//getForceSensorCaptureOptions is called on doing the graphs
		//recalculate graphs will be different if exercise changed, so need to know the exercise
		assignCurrentForceSensorExercise();

		if(currentForceSensorExercise.ComputeAsElastic)
		{
			List<ForceSensorElasticBand> list_fseb = SqliteForceSensorElasticBand.SelectAll(false, true); //not opened, onlyActive
			if(ForceSensorElasticBand.GetStiffnessOfActiveBands(list_fseb) == 0 || image_button_force_sensor_stiffness_problem.Visible)
			{
				new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Need to configure fixture to know stiffness of this elastic exercise."));
				return;
			}
		}
		box_force_sensor_capture_magnitudes.Visible = currentForceSensorExercise.ComputeAsElastic;
		box_force_sensor_analyze_magnitudes.Visible = currentForceSensorExercise.ComputeAsElastic;

		//update SQL with exercise, captureOptions, laterality, comments
		currentForceSensor.ExerciseID = currentForceSensorExercise.UniqueID;
		currentForceSensor.ExerciseName = currentForceSensorExercise.Name; //just in case
		currentForceSensor.CaptureOption = getForceSensorCaptureOptions();
		currentForceSensor.Laterality = getLaterality(false);
		//currentForceSensor.Comments = UtilGtk.TextViewGetCommentValidSQL(textview_force_sensor_capture_comment);
		currentForceSensor.Comments = UtilGtk.TextViewGetCommentValidSQL(textview_contacts_signal_comment);


		double stiffness;
		string stiffnessString;
		getStiffnessAndStiffnessStringFromSQL(out stiffness, out stiffnessString);
		currentForceSensor.Stiffness = stiffness;
		currentForceSensor.StiffnessString = stiffnessString;

		if(lastForceSensorFullPath != null && lastForceSensorFullPath != "")
		{
			forceSensorCopyTempAndDoGraphs(forceSensorGraphsEnum.SIGNAL);
			image_ai_model_graph.Sensitive = false; //unsensitivize the RFD image (can contain info of previous data)
		}

		forceSensorZoomDefaultValues();
		forceSensorPrepareGraphAI ();

		//to update maxAvgForce in 1s and fmax need to have fscPoints changed according to CaptureOption. So do it here
		currentForceSensor.MaxForceRaw = forceSensorValues.Max;

		forceSensorValues.BestSecond = getMaxAvgForce1s ();
		currentForceSensor.MaxAvgForce1s = forceSensorValues.BestSecond;
		//forceSensorValues.BestRFD = getBestAvgRFD ();

		currentForceSensor.UpdateSQL(false);

		//notebook_ai_top.CurrentPage = Convert.ToInt32(notebook_ai_top_pages.CURRENTSETSIGNAL);
		//change radio and will change also notebook:
		radio_signal_analyze_current_set.Active = true;
	}

	private enum forceSensorGraphsEnum { SIGNAL, RFD }
	private void forceSensorCopyTempAndDoGraphs(forceSensorGraphsEnum fsge)
	{
		LogB.Information(string.Format("at forceSensorCopyTempAndDoGraphs(), lastForceSensorFullPath: {0}, UtilEncoder.GetmifCSVFileName(): {1}",
				lastForceSensorFullPath, UtilEncoder.GetmifCSVFileName()));

		File.Copy(lastForceSensorFullPath, UtilEncoder.GetmifCSVFileName(), true); //can be overwritten

		if(fsge == forceSensorGraphsEnum.SIGNAL)
			forceSensorDoSignalGraph ();
		else //(fsge == forceSensorGraphsEnum.RFD)
			forceSensorDoRFDGraph();
	}

	void forceSensorDoRFDGraph()
	{
		string imagePath = UtilEncoder.GetmifTempFileName();
		Util.FileDelete(imagePath);
		image_ai_model_graph.Sensitive = false;

		double duration = -1;
		if(radio_force_duration_seconds.Active)
			duration = Convert.ToDouble(spin_force_duration_seconds.Value);

		//string title = lastForceSensorFile;
		string title = currentPerson.Name;
		string exercise = currentForceSensorExercise.Name;
		if (UtilAll.IsWindows()) {
			title = Util.ConvertToUnicode(title);
			exercise = Util.ConvertToUnicode(exercise);
		}

		if (title == null || title == "")
			title = "unnamed";
		//else
		//	title = Util.RemoveChar(title, '_');

		int sampleA;
		int sampleB;
		if (radio_ai_ab.Active)
		{
			sampleA = Convert.ToInt32(hscale_ai_a.Value);
			sampleB = Convert.ToInt32(hscale_ai_b.Value);
		} else {
			sampleA = Convert.ToInt32(hscale_ai_c.Value);
			sampleB = Convert.ToInt32(hscale_ai_d.Value);
		}
		if (AiVars.zoomApplied)
		{
			if (radio_ai_ab.Active)
			{
				sampleA += AiVars.a_beforeZoom;
				sampleB += AiVars.b_beforeZoom;
			} else {
				sampleA += AiVars.c_beforeZoom;
				sampleB += AiVars.d_beforeZoom;
			}
		}


		/*
		 * (*) check if decimal is point
		 * before 2.0.3 decimal point of forces was culture specific. From 2.0.3 is .
		 * read this file to see which is the decimal point
		 */

		ForceSensorGraph fsg = new ForceSensorGraph(
				rfdList, impulse,
				duration, Convert.ToInt32(spin_force_rfd_duration_percent.Value),
				preferences.forceSensorStartEndOptimized,
				Util.CSVDecimalColumnIsPoint(UtilEncoder.GetmifCSVFileName(), 1), 	//decimalIsPointAtFile (read)
				preferences.CSVExportDecimalSeparatorChar, 				//decimalIsPointAtExport (write)
				new ForceSensorGraphAB(
					getForceSensorCaptureOptions(),
					sampleA, sampleB,
					title, exercise,
					currentForceSensor.DatePublic,
					currentForceSensor.TimePublic,
					triggerListForceSensor)
				);

		int imageWidth = UtilGtk.WidgetWidth (viewport_ai_model_graph);
		int imageHeight = UtilGtk.WidgetHeight (viewport_ai_model_graph);
		if(imageWidth < 300)
			imageWidth = 300; //Not crash R with a png height of -1 or "figure margins too large"
		if(imageHeight < 300)
			imageHeight = 300; //Not crash R with a png height of -1 or "figure margins too large"

		bool success = fsg.CallR(imageWidth -5, imageHeight -5, true);

		if(! success)
		{
			label_model_analyze.Text = Catalog.GetString("Error doing graph.") + " " +
				Catalog.GetString("Probably not sustained force.");
			label_model_analyze.Visible = true;

			image_ai_model_graph.Visible = false;
			button_ai_model_save_image.Sensitive = false;

			return;
		}
		label_model_analyze.Visible = false;
		label_model_analyze.Text = "";

		while ( ! Util.FileReadable(imagePath));

		image_ai_model_graph = UtilGtk.OpenImageSafe(
				imagePath,
				image_ai_model_graph);
		image_ai_model_graph.Sensitive = true;
		image_ai_model_graph.Visible = true;
		button_ai_model_save_image.Sensitive = true;
	}

	void forceSensorDoSignalGraph ()
	{
		forceSensorDoSignalGraphReadFile (true, getForceSensorCaptureOptions());
		forceSensorDoSignalGraphPlot ();
	}
	void forceSensorDoSignalGraphReadFile (bool ab, ForceSensor.CaptureOptions fsco)
	{
		if (ab)
			spCairoFE = new SignalPointsCairoForceElastic ();
		else
			spCairoFE_CD = new SignalPointsCairoForceElastic ();

		//LogB.Information("at forceSensorDoSignalGraphReadFile(), filename: " + UtilEncoder.GetmifCSVFileName());
		List<string> contents;
		if (ab)
			contents = Util.ReadFileAsStringList(UtilEncoder.GetmifCSVFileName ());
		else //cd
			contents = Util.ReadFileAsStringList(UtilEncoder.GetmifCSVFileName_CD ());

		bool headersRow = true;

		//initialize
		forceSensorValues = new ForceSensorValues();

		List<int> times = new List<int>();
		List<double> forces = new List<double>();

		foreach(string str in contents)
		{
			if(headersRow)
				headersRow = false;
			else {
				string [] strFull = str.Split(new char[] {';'});
				if(strFull.Length != 2)
					continue;

				//this can take forces recorded as , or as . because before 2.0.3 forces decimal was locale specific.
				//since 2.0.3 forces are recorded with .

				if(Util.IsNumber(strFull[0], false) && Util.IsNumber(Util.ChangeDecimalSeparator(strFull[1]), true))
				{
					times.Add(Convert.ToInt32(strFull[0]));
					forces.Add(Convert.ToDouble(Util.ChangeDecimalSeparator(strFull[1])));
				}
			}
		}
		ForceSensorDynamics fsd;

		//LogB.Information(string.Format("size of times: {0}", times.Count));
		//LogB.Information(string.Format("size of forces: {0}", forces.Count));

		ForceSensorExercise fsex;
		if (ab)
			fsex = currentForceSensorExercise;
		else
			fsex = currentForceSensorExercise_CD;

		if (fsex.ComputeAsElastic)
			fsd = new ForceSensorDynamicsElastic (
					times, forces, fsco, fsex, currentPersonSession.Weight, currentForceSensor.Stiffness,
					preferences.forceSensorElasticEccMinDispl, preferences.forceSensorElasticConMinDispl, false);
		else
			fsd = new ForceSensorDynamicsNotElastic (
					times, forces, fsco, fsex, currentPersonSession.Weight, currentForceSensor.Stiffness,
					preferences.forceSensorNotElasticEccMinForce, preferences.forceSensorNotElasticConMinForce);

		forces = fsd.GetForces();
		times.RemoveAt(0); //always (not-elastic and elastic) 1st has to be removed, because time is not ok there.
		List<double> position_l = new List<double> ();
		List<double> speed_l = new List<double> ();
		List<double> accel_l = new List<double> ();
		List<double> power_l = new List<double> ();
		if(fsd.CalculedElasticPSAP)
		{
			times = times.GetRange(fsd.RemoveNValues +1, times.Count -2*fsd.RemoveNValues);
			position_l = fsd.GetPositions();
			speed_l = fsd.GetSpeeds();
			accel_l = fsd.GetAccels();
			power_l = fsd.GetPowers();
		}
		int i = 0;
		foreach(int time in times)
		{
			if (ab)
			{
				spCairoFE.Force_l.Add (new PointF (time, forces[i]));
				if(fsd.CalculedElasticPSAP)
				{
					spCairoFE.Displ_l.Add (new PointF (time, position_l[i]));
					spCairoFE.Speed_l.Add (new PointF (time, speed_l[i]));
					spCairoFE.Accel_l.Add (new PointF (time, accel_l[i]));
					spCairoFE.Power_l.Add (new PointF (time, power_l[i]));
				}
			} else {
				spCairoFE_CD.Force_l.Add (new PointF (time, forces[i]));
				if(fsd.CalculedElasticPSAP)
				{
					spCairoFE_CD.Displ_l.Add (new PointF (time, position_l[i]));
					spCairoFE_CD.Speed_l.Add (new PointF (time, speed_l[i]));
					spCairoFE_CD.Accel_l.Add (new PointF (time, accel_l[i]));
					spCairoFE_CD.Power_l.Add (new PointF (time, power_l[i]));
				}
			}

			forceSensorValues.TimeLast = time;
			forceSensorValues.ValueLast = forces[i];
			forceSensorValues.SetMaxMinIfNeeded(forces[i], time);

			i ++;
		}

		forceSensorValues.BestSecond = getMaxAvgForce1s ();
		//forceSensorValues.BestRFD = getBestAvgRFD ();
	}

	CairoGraphForceSensorSignal cairoGraphForceSensorSignal;
	SignalPointsCairoForceElastic spCairoFE;
	SignalPointsCairoForceElastic spCairoFEZoom;
	SignalPointsCairoForceElastic spCairoFE_CD;
	SignalPointsCairoForceElastic spCairoFEZoom_CD;
	bool cairoGraphForceSensorSignalPointsShowAccuracy;

	bool fsMagnitudesSignalsNoFollow;
	private void on_check_force_sensor_capture_show_magnitudes (object o, EventArgs args)
	{
		if (fsMagnitudesSignalsNoFollow)
			return;

		force_capture_drawingarea_cairo.QueueDraw ();

		//sync with analyze magnitudes
		fsMagnitudesSignalsNoFollow = true;

		if (check_force_sensor_analyze_show_distance.Active != check_force_sensor_capture_show_distance.Active)
			check_force_sensor_analyze_show_distance.Active = check_force_sensor_capture_show_distance.Active;
		if (check_force_sensor_analyze_show_speed.Active != check_force_sensor_capture_show_speed.Active)
			check_force_sensor_analyze_show_speed.Active = check_force_sensor_capture_show_speed.Active;
		if (check_force_sensor_analyze_show_power.Active != check_force_sensor_capture_show_power.Active)
			check_force_sensor_analyze_show_power.Active = check_force_sensor_capture_show_power.Active;

		fsMagnitudesSignalsNoFollow = false;
	}

	public void on_force_capture_drawingarea_cairo_draw (object o, Gtk.DrawnArgs args)
	{
		updateForceSensorCaptureSignalCairo (true);
	}

	private void updateForceSensorCaptureSignalCairo (bool forceRedraw)
	{
		bool capturing = (forceCaptureThread != null && forceCaptureThread.IsAlive);

		bool cairoDrawHorizontal = true;
		if (! preferences.signalDirectionHorizontal && capturing)
			cairoDrawHorizontal = false;

		//LogB.Information ("updateForceSensorCaptureSignalCairo 0");
		if (cairoGraphForceSensorSignal == null || fullScreenChange == fullScreenChangeEnum.CHANGETOFULL)
		{
			Gtk.DrawingArea da = force_capture_drawingarea_cairo;
			if (fullScreenChange == fullScreenChangeEnum.CHANGETOFULL)
			{
				da = fullscreen_capture_drawingarea_cairo;
				fullScreenChange = fullScreenChangeEnum.DONOTHING;
			}

			if (preferences.forceSensorCaptureFeedbackActive ==
					Preferences.ForceSensorCaptureFeedbackActiveEnum.ASTEROIDS)
				cairoGraphForceSensorSignal = new CairoGraphForceSensorSignalAsteroids (
						da, "title", cairoDrawHorizontal);
			else if (preferences.forceSensorCaptureFeedbackActive ==
					Preferences.ForceSensorCaptureFeedbackActiveEnum.QUESTIONNAIRE)
				cairoGraphForceSensorSignal = new CairoGraphForceSensorSignalQuestionnaire (
						da, "title");
			else
				cairoGraphForceSensorSignal = new CairoGraphForceSensorSignal (
						da, "title",
						preferences.forceSensorFeedbackPathLineWidth,
						cairoDrawHorizontal);
		}
		else if (! capturing &&
				(cairoGraphForceSensorSignal.GetType ().Equals (typeof (CairoGraphForceSensorSignalAsteroids)) ||
				 cairoGraphForceSensorSignal.GetType ().Equals (typeof (CairoGraphForceSensorSignalQuestionnaire))))
		{
			//while ! capture do not show asteroids/questionnaire
			cairoGraphForceSensorSignal = new CairoGraphForceSensorSignal (
					force_capture_drawingarea_cairo, "title",
					preferences.forceSensorFeedbackPathLineWidth,
					cairoDrawHorizontal);
		}

		//LogB.Information ("updateForceSensorCaptureSignalCairo 1");

		int showLastSeconds = -1; //show all signal
		if (forceCaptureThread != null && forceCaptureThread.IsAlive)
			showLastSeconds = 10; //TODO: make this configurable from GUI

		//LogB.Information ("updateForceSensorCaptureSignalCairo 2");
		int rectangleN = 0;
		int rectangleRange = 0;
		if(preferences.forceSensorCaptureFeedbackActive == Preferences.ForceSensorCaptureFeedbackActiveEnum.RECTANGLE)
		{
			rectangleN = preferences.forceSensorCaptureFeedbackAt;
			rectangleRange = preferences.forceSensorCaptureFeedbackRange;
		}
		//LogB.Information ("updateForceSensorCaptureSignalCairo 3");

		/*
		LogB.Information (string.Format ("cairoGraphForceSensorSignalPoints_l == null: {0}, paintPointsInterpolateCairo_l == null: {1}",
					(cairoGraphForceSensorSignalPoints_l == null),
					(paintPointsInterpolateCairo_l == null) ));
		*/
		if (spCairoFE == null)
			spCairoFE = new SignalPointsCairoForceElastic ();
		if (paintPointsInterpolateCairo_l == null)
			paintPointsInterpolateCairo_l = new List<PointF> ();

		//create copys to not have problem on updating data that is being graph in other thread (even using static variables)
		List<PointF> paintPointsInterpolateCairo_l_copy = new List<PointF>();

		//TODO: think if is better to decide de startAt here and not in Cairo to not be able to copy a growing list that is not used at all

		//create a copy
		int pointsToCopy = spCairoFE.Force_l.Count;
		SignalPointsCairoForceElastic spCairoFECopy = new SignalPointsCairoForceElastic (
				spCairoFE, 0, pointsToCopy -1, cairoDrawHorizontal);

		//create inerpolate_l_copy for path, but not on load
		if (interpolate_l != null && preferences.forceSensorCaptureFeedbackActive == Preferences.ForceSensorCaptureFeedbackActiveEnum.PATH)
			for (int i = 0; i < pointsToCopy; i ++)
				paintPointsInterpolateCairo_l_copy.Add (paintPointsInterpolateCairo_l[i]);

		TriggerList triggerListForceSensor_copy = new TriggerList ();
		if (triggerListForceSensor != null && triggerListForceSensor.Count() > 0)
		{
			pointsToCopy = triggerListForceSensor.Count ();
			for (int i = 0; i < pointsToCopy; i ++)
				triggerListForceSensor_copy.Add (triggerListForceSensor.GetTrigger (i));
		}

		//minimum Y display from 0 to +25
		int minY = 0;
		int maxY = +25;
		if (spCairoFECopy.Displ_l != null && spCairoFECopy.Displ_l.Count > 0)
		{
			minY = 0;
			maxY = 0;
		}

		GetMaxAvgInWindow gmiw = new GetMaxAvgInWindow (spCairoFECopy.Force_l,
				0, spCairoFECopy.Force_l.Count -1, 1); //1s
		if (spCairoFECopy.Force_l.Count > 0 && gmiw.Error == "")
		{
			label_force_sensor_value_best_second.Text = string.Format("{0:0.##}", gmiw.Max);
			if (forceSensorValues != null)
				forceSensorValues.BestSecond = gmiw.Max;
		}

		GetBestRFDInWindow briw = new GetBestRFDInWindow (spCairoFECopy.Force_l,
				0, spCairoFECopy.Force_l.Count -1, 0.05); //50 ms
		if (spCairoFECopy.Force_l.Count > 0 && briw.Error == "")
		{
			label_force_sensor_value_rfd.Text = string.Format("{0:0.##}", briw.Max);
			if (forceSensorValues != null)
				forceSensorValues.BestRFD = briw.Max;
		}

		if (capturing && preferences.forceSensorCaptureFeedbackActive == Preferences.ForceSensorCaptureFeedbackActiveEnum.ASTEROIDS)
			cairoGraphForceSensorSignal.PassAsteroids = asteroids;
		else if (capturing && preferences.forceSensorCaptureFeedbackActive == Preferences.ForceSensorCaptureFeedbackActiveEnum.QUESTIONNAIRE)
		{
			cairoGraphForceSensorSignal.PassQuestionnaire = questionnaire;
			cairoGraphForceSensorSignal.QuestionnaireMinY = preferences.forceSensorFeedbackQuestionnaireMin;
			cairoGraphForceSensorSignal.QuestionnaireMaxY = preferences.forceSensorFeedbackQuestionnaireMax;
		}

		if (fullScreenChange != fullScreenChangeEnum.DONOTHING)
		{
			if (fullScreenChange == fullScreenChangeEnum.CHANGETOFULL)
				cairoGraphForceSensorSignal.ChangeDrawingArea (fullscreen_capture_drawingarea_cairo);
			else //if (fullScreenChange == fullScreenChangeEnum.CHANGETONORMAL)
				cairoGraphForceSensorSignal.ChangeDrawingArea (force_capture_drawingarea_cairo);

			fullScreenChange = fullScreenChangeEnum.DONOTHING;
		}

		//butterworth (see comments on butterworth/Sample/Program.cs
		ChronoDebug cDebug = new ChronoDebug ("Butterworth time:");
		cDebug.Start();

		//List<PointF> butterTrajAutomatic_l = new List<PointF> ();
		List<PointF> butterTrajA_l = new List<PointF> ();
		//double trajAutomaticXCutoff = 0;
		double trajACutoff = 15;

		if (spCairoFECopy.Force_l.Count > 0 &&
				preferences.forceSensorCaptureFeedbackActive ==
				Preferences.ForceSensorCaptureFeedbackActiveEnum.NO) //right now only calculate butterworth on NO feedback
		{
			List<PointF> pForButter_l = spCairoFECopy.Force_l;
			if (! cairoDrawHorizontal)
				pForButter_l = spCairoFECopy.ForcePaintHoriz_l;

			List<TimedPoint> samples = new List<TimedPoint>();
			foreach (PointF point in pForButter_l)
				samples.Add (new TimedPoint((float) point.Y, 0, (long) point.X));

			double fps = UtilAll.DivideSafe (pForButter_l.Count, PointF.Last (pForButter_l).X/1000000 - pForButter_l[0].X/1000000);
			//FilteredTrajectory trajAutomatic = new FilteredTrajectory();
			FilteredTrajectory trajA = new FilteredTrajectory();
			//trajAutomatic.Initialize(samples, fps, -1);
			//trajAutomaticXCutoff = trajAutomatic.XCutoff;
			trajA.Initialize(samples, fps, trajACutoff);
			//LogB.Information (string.Format ("butterworth: samples: {0}, fps: {1}, cutoff: {2}",
			//			pForButter_l.Count, fps, traj.XCutoffIndex));

			//for (int i = 0; i < trajAutomatic.Times.Length; i ++)
			for (int i = 0; i < trajA.Times.Length; i ++)
			{
				//the -10, +5 , +10 are to be able to see the diff now as fast debug on screen
				if (cairoDrawHorizontal)
				{
					//butterTrajAutomatic_l.Add (new PointF (trajAutomatic.Times[i], trajAutomatic.Xs[i] - 10));
					butterTrajA_l.Add (new PointF (trajA.Times[i], trajA.Xs[i]));
				} else {
					//butterTrajAutomatic_l.Add (new PointF (trajAutomatic.Xs[i] - 10, trajAutomatic.Times[i]));
					butterTrajA_l.Add (new PointF (trajA.Xs[i], trajA.Times[i]));
				}
			}
		}
		cDebug.StopAndPrint();

		double videoTime = 0;
		if (webcamPlay != null && webcamPlay.PlayVideoGetSecond > 0)
		{
			videoTime = webcamPlay.PlayVideoGetSecond -diffVideoVsSignal;
				/*
				+ 0.010 //creating the graph is 10ms aprox
				+ 0.020; //20 more for getting the sterrHandler from ffplay and processing it
				*/

			//TODO: calculate an average of previous samples
			//maybe is better not have this +0.010, +0.020 because if video is paused we want current videoTime,
			//so what we have done is  have pulseWebcamPlayGTK sleep just 10 ms instead of 25
			//LogB.Information (string.Format ("forceSensor videoTime: {0}, webcamPlay.PlayVideoGetSecond: {1}, -diffVideoVsSignal: {2}",
			//			videoTime, webcamPlay.PlayVideoGetSecond, diffVideoVsSignal));

			//LogB.Information ("videoFrames", videoFrames);
			//LogB.Information ("spCairoFECopy.Force_l.Count", spCairoFECopy.Force_l.Count);
		}

		//LogB.Information ("updateForceSensorCaptureSignalCairo 4");
		cairoGraphForceSensorSignal.DoSendingList (
				preferences.fontType.ToString(),
				spCairoFECopy,
				//butterTrajAutomatic_l, trajAutomaticXCutoff,
				butterTrajA_l, //trajACutoff,
				check_force_sensor_capture_show_distance.Active,
				check_force_sensor_capture_show_speed.Active,
				check_force_sensor_capture_show_power.Active,
				paintPointsInterpolateCairo_l_copy, preferences.forceSensorFeedbackPathMin, preferences.forceSensorFeedbackPathMax,
				capturing, videoTime,
				cairoGraphForceSensorSignalPointsShowAccuracy,
				showLastSeconds,
				minY, maxY,
				rectangleN, rectangleRange,
				gmiw,
				briw,
				triggerListForceSensor_copy,
				forceRedraw, CairoXY.PlotTypes.LINES);

		//LogB.Information ("updateForceSensorCaptureSignalCairo 5");
		if (currentForceSensor.UniqueID >= 0 && forceSensorValues != null)
		{
			label_force_sensor_value.Text = string.Format("{0:0.##}", forceSensorValues.ValueLast);
			label_force_sensor_value_max.Text = string.Format("{0:0.##}", forceSensorValues.Max);
			label_force_sensor_value_min.Text = string.Format("{0:0.##}", forceSensorValues.Min);
			label_force_sensor_value_best_second.Text = string.Format("{0:0.##}", forceSensorValues.BestSecond);
			label_force_sensor_value_rfd.Text = string.Format("{0:0.##}", forceSensorValues.BestRFD);
		}

		if (currentForceSensor.UniqueID >= 0)
		{
			//LogB.Information ("updateForceSensorCaptureSignalCairo 6");
			button_force_sensor_image_save_signal.Sensitive = true;
			button_ai_model.Sensitive = true;
		}
	}

	private void forceSensorDoSignalGraphPlot ()
	{
		force_capture_drawingarea_cairo.QueueDraw ();

		label_force_sensor_value.Text = string.Format("{0:0.##}", forceSensorValues.ValueLast);
		label_force_sensor_value_max.Text = string.Format("{0:0.##}", forceSensorValues.Max);
		label_force_sensor_value_min.Text = string.Format("{0:0.##}", forceSensorValues.Min);
		label_force_sensor_value_best_second.Text = string.Format("{0:0.##}", forceSensorValues.BestSecond);
		label_force_sensor_value_best_second.Text = string.Format("{0:0.##}", forceSensorValues.BestRFD);
		button_force_sensor_image_save_signal.Sensitive = true;
		button_ai_model.Sensitive = true;
	}

	static List<PointF> paintPointsInterpolateCairo_l;

	private enum ForceSensorGraphs { CAPTURE, ANALYSIS_GENERAL }

	private void on_radio_force_rfd_duration_toggled (object o, EventArgs args)
	{
		spin_force_duration_seconds.Sensitive = radio_force_duration_seconds.Active;
		hbox_force_rfd_duration_percent.Sensitive = radio_force_rfd_duration_percent.Active;
	}

	private void on_button_force_sensor_image_save_signal_clicked (object o, EventArgs args)
	{
		checkFile(Constants.CheckFileOp.FORCESENSOR_SAVE_IMAGE_SIGNAL);
	}
	private void on_button_force_sensor_image_save_model_clicked (object o, EventArgs args)
	{
		checkFile(Constants.CheckFileOp.FORCESENSOR_SAVE_IMAGE_MODEL);
	}
	private void on_button_force_sensor_image_save_rfd_manual_clicked (object o, EventArgs args)
	{
		checkFile(Constants.CheckFileOp.FORCESENSOR_SAVE_IMAGE_RFD_MANUAL);
	}

	void on_button_forcesensor_save_image_signal_file_selected (string destination)
	{
		try {
			if (force_capture_drawingarea_cairo == null)
				return;

			LogB.Information ("Saving");
			CairoUtil.GetScreenshotFromDrawingArea (force_capture_drawingarea_cairo, destination);
		} catch {
			string myString = string.Format (
					Catalog.GetString ("Cannot save file {0} "), destination);
			new DialogMessage (Constants.MessageTypes.WARNING, myString);
		}
	}
	private void on_overwrite_file_forcesensor_save_image_signal_accepted(object o, EventArgs args)
	{
		on_button_forcesensor_save_image_signal_file_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), exportFileName);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}

	void on_button_forcesensor_save_image_rfd_model_file_selected (string destination)
	{
		File.Copy(UtilEncoder.GetmifTempFileName(), destination, true);
	}
	private void on_overwrite_file_forcesensor_save_image_rfd_model_accepted(object o, EventArgs args)
	{
		on_button_forcesensor_save_image_rfd_model_file_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), exportFileName);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}

	void on_button_forcesensor_save_image_rfd_manual_file_selected (string destination)
	{
		try {
			if (ai_drawingarea_cairo == null)
				return;

			LogB.Information ("Saving");
			CairoUtil.GetScreenshotFromDrawingArea (ai_drawingarea_cairo, destination);
		} catch {
			string myString = string.Format (
					Catalog.GetString ("Cannot save file {0} "), destination);
			new DialogMessage (Constants.MessageTypes.WARNING, myString);
		}
	}
	private void on_overwrite_file_forcesensor_save_image_rfd_manual_accepted(object o, EventArgs args)
	{
		on_button_forcesensor_save_image_rfd_manual_file_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), exportFileName);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}


	private void on_button_force_sensor_adjust_clicked (object o, EventArgs args)
	{
		button_force_sensor_adjust.Sensitive = false; //to not be called again

		notebook_contacts_capture_doing_wait.Sensitive = false;
		notebook_contacts_execute_or.CurrentPage = Convert.ToInt32(notebook_contacts_execute_or_pages.FORCESENSORADJUST);

		viewport_chronopics.Sensitive = false;
		frame_contacts_exercise.Sensitive = false;

		forceSensorCaptureAdjustSensitivity(false);
		force_sensor_adjust_label_message.Text = Catalog.GetString("If you want to calibrate, please tare first.");
	}
	private void on_button_force_sensor_adjust_close_clicked (object o, EventArgs args)
	{
		button_force_sensor_adjust.Sensitive = true;

		notebook_contacts_capture_doing_wait.Sensitive = true;
		notebook_contacts_execute_or.CurrentPage = Convert.ToInt32(notebook_contacts_execute_or_pages.EXECUTE);

		viewport_chronopics.Sensitive = true;
		frame_contacts_exercise.Sensitive = true;

		forceSensorCaptureAdjustSensitivity(true);
		event_execute_label_message.Text = "";
	}

	private void forceSensorCaptureAdjustSensitivity(bool s) //s for sensitive. When adjusting s = false
	{
		hbox_force_capture_buttons.Sensitive = s;

		button_contacts_devices_networks.Sensitive = s;
		image_test.Sensitive = s;
		button_execute_test.Sensitive = (s && currentPerson != null && currentPerson.UniqueID > 0 && currentSession != null);
		button_force_sensor_image_save_signal.Sensitive = s;

		menus_and_mode_sensitive(s);
		hbox_contacts_sup_capture_analyze_two_buttons.Sensitive = s;
		hbox_top_person.Sensitive = s;
	}

	private void on_button_force_sensor_adjust_help_clicked (object o, EventArgs args)
	{
		new DialogMessage("Force sensor adjust data", Constants.MessageTypes.INFO,
				preferences.GetForceSensorAdjustString());
	}

	private void on_button_force_sensor_sync_clicked (object o, EventArgs args)
	{
		LogB.Information("on_button_force_sensor_sync_clicked 0");
		//port A check connection
		if(! portFSOpened)
			if(! forceSensorConnect())
			{
				new DialogMessage(Constants.MessageTypes.WARNING, "Cannot connect port A");
				return;
			}

		LogB.Information("on_button_force_sensor_sync_clicked 1");
		string versionStr = forceSensorCheckVersionDo();
                double versionDouble = Convert.ToDouble(Util.ChangeDecimalSeparator(versionStr));
		if(versionDouble < Convert.ToDouble(Util.ChangeDecimalSeparator("0.6")))
		{
			new DialogMessage(Constants.MessageTypes.WARNING, "Force version A == " + versionStr);
			return;
		}

		LogB.Information("on_button_force_sensor_sync_clicked 2");
		//port B check connection
		if(! portFSOpened_B)
			if(! forceSensorConnect_Port_B())
			{
				new DialogMessage(Constants.MessageTypes.WARNING, "Cannot connect port B");
				return;
			}

		LogB.Information("on_button_force_sensor_sync_clicked 3");
		versionStr = forceSensorCheckVersionDo_Port_B();
                versionDouble = Convert.ToDouble(Util.ChangeDecimalSeparator(versionStr));
		if(versionDouble < Convert.ToDouble(Util.ChangeDecimalSeparator("0.6")))
		{
			new DialogMessage(Constants.MessageTypes.WARNING, "Force version B == " + versionStr);
			return;
		}

		LogB.Information("on_button_force_sensor_sync_clicked 4");
		new DialogMessage(Constants.MessageTypes.WARNING, "Success! Both dispositives with >= 0.6");
	}

	private void createComboForceSensorCaptureOptions()
	{
		combo_force_sensor_capture_options = UtilGtk.CreateComboBoxText (
				box_combo_force_sensor_capture_options,
				ForceSensor.CaptureOptionsList (),
				"");
	}

	// -------------------------------- exercise stuff --------------------


	string [] forceSensorComboExercisesString; //id:name (no translations, use user language)

	private void on_force_sensor_exercise_filter_changed (object o, EventArgs args)
	{
		updateForceExerciseCombo ();
	}

	//called on initForceSensor (just one time)
	private void createForceExerciseCombo ()
	{
		LogB.Information("createForceExerciseCombo start");
		//force_sensor_exercise

		combo_force_sensor_exercise = new ComboBoxText ();
		fillForceSensorExerciseCombo("");

		combo_force_sensor_exercise.Changed += new EventHandler (on_combo_force_sensor_exercise_changed);
		hbox_combo_force_sensor_exercise.PackStart(combo_force_sensor_exercise, true, true, 0);
		hbox_combo_force_sensor_exercise.ShowAll();

		//needed because the += EventHandler does not work on first fill of the combo
		on_combo_force_sensor_exercise_changed (new object (), new EventArgs ());

		combo_force_sensor_button_sensitive_exercise(UtilGtk.ComboGetActive(combo_force_sensor_exercise) != "");
		LogB.Information("createForceExerciseCombo end");
	}

	//called on change mode, can be from isometric to elastic
	private void updateForceExerciseCombo ()
	{
		LogB.Information("updateForceExerciseCombo start");
		combo_force_sensor_exercise.Changed -= new EventHandler (on_combo_force_sensor_exercise_changed);
		UtilGtk.ComboDelAll(combo_force_sensor_exercise);
		combo_force_sensor_exercise.Changed += new EventHandler (on_combo_force_sensor_exercise_changed);
		LogB.Information("updateForceExerciseCombo going to fill");
		fillForceSensorExerciseCombo("");
		LogB.Information("updateForceExerciseCombo end");

		//needed because the += EventHandler does not work on first fill of the combo
		on_combo_force_sensor_exercise_changed (new object (), new EventArgs ());
	}

	//left-right buttons on force_sensor combo exercise selection
	private void on_button_combo_force_sensor_exercise_capture_left_clicked (object o, EventArgs args)
	{
		contacts_exercise_left_button (combo_force_sensor_exercise,
				button_combo_force_sensor_exercise_capture_left,
				button_combo_force_sensor_exercise_capture_right);
	}
	private void on_button_combo_force_sensor_exercise_capture_right_clicked (object o, EventArgs args)
	{
		contacts_exercise_right_button (combo_force_sensor_exercise,
				button_combo_force_sensor_exercise_capture_left,
				button_combo_force_sensor_exercise_capture_right);
	}

	private void on_combo_force_sensor_exercise_changed (object o, EventArgs args)
	{
		//ComboBoxText combo = o as ComboboxText;
		if (o == null)
			return;

		//two combobox are linked ---->
		if(comboSelectContactsTopNoFollow)
			return;

		comboSelectContactsTopNoFollow = true;
		if (o == combo_force_sensor_exercise)
			combo_select_contacts_top.Active = combo_force_sensor_exercise.Active;
		else if (o == combo_select_contacts_top)
			combo_force_sensor_exercise.Active = combo_select_contacts_top.Active;
		comboSelectContactsTopNoFollow = false;
		//<---- two combobox are linked

		int exID = getExerciseIDFromAnyCombo (combo_force_sensor_exercise, forceSensorComboExercisesString, false);
		ArrayList array = SqliteForceSensorExercise.Select (false, exID, -1, false, "");

		if(array.Count == 0 || exID < 0)
		{
			label_button_force_sensor_stiffness.Text = "0";
			image_button_force_sensor_stiffness_problem.Visible = true;

			frame_force_sensor_elastic.Visible = false;

			setLabelContactsExerciseSelected(current_mode);
			combo_force_sensor_button_sensitive_exercise(false);
			return;
		}

		ForceSensorExercise fse = (ForceSensorExercise) array[0];
		if(fse.ComputeAsElastic)
		{
			List<ForceSensorElasticBand> list_fseb = SqliteForceSensorElasticBand.SelectAll(false, true); //not opened, onlyActive
			double stiffness = ForceSensorElasticBand.GetStiffnessOfActiveBands(list_fseb);

			setStiffnessButtonLabel(stiffness);
			frame_force_sensor_elastic.Visible = true;
		} else {
			label_button_force_sensor_stiffness.Text = "0";
			frame_force_sensor_elastic.Visible = false;
			image_button_force_sensor_stiffness_problem.Visible = false;
		}

		//sensitivity of left/right buttons
		button_combo_force_sensor_exercise_capture_left.Sensitive = (combo_force_sensor_exercise.Active > 0);
		button_combo_force_sensor_exercise_capture_right.Sensitive = ! UtilGtk.ComboSelectedIsLast(combo_force_sensor_exercise);
		button_combo_select_contacts_top_left.Sensitive = (combo_force_sensor_exercise.Active > 0);
		button_combo_select_contacts_top_right.Sensitive = ! UtilGtk.ComboSelectedIsLast(combo_force_sensor_exercise);

		setLabelContactsExerciseSelected(Catalog.GetString(fse.Name));
		changeTestImage (fse.UniqueID);

		combo_force_sensor_button_sensitive_exercise(true);

		if(fse.ForceResultant) {
			/*
			   setForceSensorCaptureOptions(ForceSensor.CaptureOptions.ABS);
			   combo_force_sensor_capture_options.Sensitive = false;
			   better to hide it instead of making it unsensitive to not force it to ABS and then have ABS by default in raw exercises
			   */
			//combo_force_sensor_capture_options.Visible = false;
			//2.2.0:
			combo_force_sensor_capture_options.Visible = true;
		} else {
			//combo_force_sensor_capture_options.Sensitive = true;
			combo_force_sensor_capture_options.Visible = true;
		}
	}

	private void fillForceSensorExerciseCombo (string name)
	{
		int elastic = ForceSensor.GetElasticIntFromMode (current_mode);

		ArrayList forceSensorExercises = SqliteForceSensorExercise.Select (false, -1, elastic, false,
				force_sensor_exercise_filter.Text.ToString ());
		if(forceSensorExercises.Count == 0)
		{
			forceSensorComboExercisesString = new String [0];
			UtilGtk.ComboUpdate(combo_force_sensor_exercise, new String[0], "");

			return;
		}

		forceSensorComboExercisesString = new String [forceSensorExercises.Count];
		string [] exerciseNamesToCombo = new String [forceSensorExercises.Count];
		int i =0;
		foreach(ForceSensorExercise ex in forceSensorExercises)
		{
			exerciseNamesToCombo[i] = ex.Name;
			forceSensorComboExercisesString[i] = ex.UniqueID + ":" + ex.Name;
			i++;
		}

		UtilGtk.ComboUpdate(combo_force_sensor_exercise, exerciseNamesToCombo, "");
		if(name == "")
			combo_force_sensor_exercise.Active = 0;
		else
			combo_force_sensor_exercise.Active = UtilGtk.ComboMakeActive(combo_force_sensor_exercise, name);

		//update also combo_select_contacts_top (but check do not crash on start)
		//we need the 2nd check because without is, on import if we are on other mode, top combo could have been updated with this mode exercises
		if(combo_select_contacts_top != null && Constants.ModeIsFORCESENSOR (current_mode))
		{
			comboSelectContactsTopNoFollow = true;
			UtilGtk.ComboUpdate(combo_select_contacts_top,
					UtilGtk.ComboGetValues (combo_force_sensor_exercise), "");
			combo_select_contacts_top.Active = combo_force_sensor_exercise.Active;
			comboSelectContactsTopNoFollow = false;
		}
	}

	private void combo_force_sensor_button_sensitive_exercise(bool exerciseSelected)
	{
		button_force_sensor_exercise_edit.Sensitive = exerciseSelected;
		button_force_sensor_exercise_delete.Sensitive = exerciseSelected;
		button_image_test_add_edit.Sensitive = exerciseSelected;
	}

	void on_button_force_sensor_exercise_edit_clicked (object o, EventArgs args)
	{
		if(UtilGtk.ComboGetActive(combo_force_sensor_exercise) == "")
		{
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Need to create/select an exercise."));
			return;
		}

		ForceSensorExercise ex = (ForceSensorExercise) SqliteForceSensorExercise.Select (
                                false, getExerciseIDFromAnyCombo(combo_force_sensor_exercise, forceSensorComboExercisesString, false), -1, false, "")[0];

		LogB.Information("selected exercise: " + ex.ToString());

		string subtitle = Catalog.GetString ("Isometric exercise");
		if (current_mode == Constants.Modes.FORCESENSORELASTIC)
			subtitle = Catalog.GetString ("Elastic exercise");

		forceSensorExerciseWin = ForceSensorExerciseWindow.ShowEdit (current_mode,
				Catalog.GetString("Exercise"),
				subtitle, ex,
				preferences.forceSensorElasticEccMinDispl, preferences.forceSensorElasticConMinDispl,
				preferences.forceSensorNotElasticEccMinForce, preferences.forceSensorNotElasticConMinForce);

		forceSensorExerciseWin.FakeButtonReadValues.Clicked += new EventHandler(on_button_force_sensor_exercise_edit_add_accepted);
	}

	void on_button_force_sensor_exercise_add_clicked (object o, EventArgs args)
	{
		string subtitle = Catalog.GetString ("Isometric exercise");
		if (current_mode == Constants.Modes.FORCESENSORELASTIC)
			subtitle = Catalog.GetString ("Elastic exercise");

		forceSensorExerciseWin = ForceSensorExerciseWindow.ShowAdd (current_mode,
				Catalog.GetString("Exercise"),
				subtitle,
				preferences.forceSensorElasticEccMinDispl, preferences.forceSensorElasticConMinDispl,
				preferences.forceSensorNotElasticEccMinForce, preferences.forceSensorNotElasticConMinForce);

		forceSensorExerciseWin.FakeButtonReadValues.Clicked += new EventHandler(on_button_force_sensor_exercise_edit_add_accepted);
	}

	void on_button_force_sensor_exercise_edit_oldTODO_accepted (object o, EventArgs args)
	{
		//TODO: cridat des del delete
	}

	void on_button_force_sensor_exercise_edit_add_accepted (object o, EventArgs args)
	{
		forceSensorExerciseWin.FakeButtonReadValues.Clicked -= new EventHandler(on_button_force_sensor_exercise_edit_add_accepted);

		if(forceSensorExerciseWin.Success)
			fillForceSensorExerciseCombo(forceSensorExerciseWin.Exercise.Name);

		currentForceSensorExercise = forceSensorExerciseWin.Exercise;

		forceSensorExerciseWin.HideAndNull();

		forceSensorPrepareGraphAI ();
	}

	//based on: on_button_encoder_exercise_delete
	//maybe unify them on the future
	void on_button_force_sensor_exercise_delete_clicked (object o, EventArgs args)
	{
		if(UtilGtk.ComboGetActive(combo_force_sensor_exercise) == "")
		{
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Need to create/select an exercise."));
			return;
		}

		ForceSensorExercise ex = (ForceSensorExercise) SqliteForceSensorExercise.Select (
                                false, getExerciseIDFromAnyCombo(combo_force_sensor_exercise, forceSensorComboExercisesString, false), -1, false, "")[0];

		//1st find if there are sets with this exercise
		ArrayList array = SqliteForceSensor.SelectRowsOfAnExercise(false, ex.UniqueID);

		if(array.Count > 0)
		{
			genericWin = GenericWindow.Show(Catalog.GetString("Delete exercise"),
					Catalog.GetString("Exercise name:"), Constants.GenericWindowShow.ENTRY, false);

			genericWin.EntrySelected = ex.Name;

			//just one button to exit and with ESC accelerator
			genericWin.ShowButtonAccept(false);
			genericWin.SetButtonCancelLabel(Catalog.GetString("Close"));

			//there are some records of this exercise on forceSensor table, do not delete
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
			//forceSensor table has not records of this exercise. Delete exercise
			SqliteForceSensorExercise.Delete(false, ex.UniqueID);
			//delete the image (if any)
			ExerciseImage ei = new ExerciseImage (current_mode, ex.UniqueID);
			ei.DeleteImage ();

			fillForceSensorExerciseCombo("");
			combo_force_sensor_exercise.Active = 0;

			new DialogMessage(Constants.MessageTypes.INFO, Catalog.GetString("Exercise deleted."));
		}
	}

	// -------------------------------- end of exercise stuff --------------------

	// -------------------------------- elastic band stuff -----------------------

	private void getStiffnessAndStiffnessStringFromSQL(out double stiffness, out string stiffnessString)
	{
		stiffness = -1;
		stiffnessString = "";

		if(currentForceSensorExercise.ComputeAsElastic)
		{
			List<ForceSensorElasticBand> list_fseb = SqliteForceSensorElasticBand.SelectAll(false, true); //not opened, onlyActive
			stiffness = ForceSensorElasticBand.GetStiffnessOfActiveBands(list_fseb);
			stiffnessString = ForceSensorElasticBand.GetIDsOfActiveBands(list_fseb);
		}
	}

	private void on_button_force_sensor_stiffness_clicked (object o, EventArgs args)
	{
		//done like this to be able to call on_elastic_bands_win_stiffness_changed before the Show is done, because in the Show is where the stiffness will change
		forceSensorElasticBandsWin = new ForceSensorElasticBandsWindow();
		forceSensorElasticBandsWin.FakeButton_stiffness_changed.Clicked -= new EventHandler(on_elastic_bands_win_stiffness_changed);
		forceSensorElasticBandsWin.FakeButton_stiffness_changed.Clicked += new EventHandler(on_elastic_bands_win_stiffness_changed);

		forceSensorElasticBandsWin.Show(
				Catalog.GetString("Stiffness configuration"), Catalog.GetString("Configure attached elastic bands/tubes"));
	}

	private void on_elastic_bands_win_stiffness_changed(object o, EventArgs args)
	{
		setStiffnessButtonLabel (forceSensorElasticBandsWin.TotalStiffness);
	}

	private void setStiffnessButtonLabel (double stiffness)
	{
		if(stiffness <= 0)
		{
			label_button_force_sensor_stiffness.Text = Catalog.GetString("Configure bands/tubes");
			image_button_force_sensor_stiffness_problem.Visible = true;
		} else {
			label_button_force_sensor_stiffness.Text = Catalog.GetString("Stiffness:") + " " +
				stiffness.ToString() + " N/m";
			image_button_force_sensor_stiffness_problem.Visible = false;
		}
	}


	// -------------------------------- end of elastic band stuff ----------------


	// -------------------------------- options, laterality and comment stuff -------------

	//note: Standard capture, Absolute values, Inverted values are:
	//- on glade: app1 combo_force_sensor_capture_options
	//- on ForceSensorCaptureOptionsString...
	private ForceSensor.CaptureOptions getForceSensorCaptureOptions()
	{
		string option = UtilGtk.ComboGetActive(combo_force_sensor_capture_options);
		if(option == ForceSensor.CaptureOptionsStringABS())
			return ForceSensor.CaptureOptions.ABS;
		else if(option == ForceSensor.CaptureOptionsStringINVERTED())
			return ForceSensor.CaptureOptions.INVERTED;
		else //ForceSensor.CaptureOptionsStringNORMAL()
			return ForceSensor.CaptureOptions.NORMAL;
	}
	private void setForceSensorCaptureOptions(ForceSensor.CaptureOptions co)
	{
		LogB.Information("setForceSensorCaptureOptions " + co.ToString());
		combo_force_sensor_capture_options.Active = UtilGtk.ComboMakeActive(
				combo_force_sensor_capture_options,
				Catalog.GetString(ForceSensor.GetCaptureOptionsString(co)));
	}

	private void on_radio_force_sensor_laterality_toggled (object o, EventArgs args)
	{
		//setLabelContactsExerciseSelectedOptionsForceSensor();
		setForceSensorLateralityPixbuf();
	}

	private string getLaterality(bool translated)
	{
		string lat = "";
		if(radio_force_sensor_laterality_both.Active)
			lat = Constants.ForceSensorLateralityBoth;
		else if(radio_force_sensor_laterality_l.Active)
			lat = Constants.ForceSensorLateralityLeft;
		else //if(radio_force_sensor_laterality_r.Active)
			lat = Constants.ForceSensorLateralityRight;

		if(translated)
			return Catalog.GetString(lat);

		return lat;
	}
	private void setLaterality(string s)
	{
		if(s == Constants.ForceSensorLateralityLeft)
			radio_force_sensor_laterality_l.Active = true;
		else if(s == Constants.ForceSensorLateralityRight)
			radio_force_sensor_laterality_r.Active = true;
		else //if(s == Constants.ForceSensorLateralityBoth)
			radio_force_sensor_laterality_both.Active = true;

		setForceSensorLateralityPixbuf();
	}

	private void setForceSensorLateralityPixbuf()
	{
		Pixbuf pixbuf;
		if(radio_force_sensor_laterality_r.Active)
			pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "laterality-right.png");
		else if(radio_force_sensor_laterality_l.Active)
			pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "laterality-left.png");
		else
			pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "laterality-both.png");

		image_top_laterality_contacts.Pixbuf = pixbuf;
	}
	// -------------------------------- end of options, laterality and comment stuff ------

	// ------------------------------------------------ slides stuff for presentations

	double lastChangedTime; //changeSlideCode
	private void changeSlideIfNeeded(int time, double force)
	{
		if(force > 75) {
			//changeSlide if one second or more elapsed since last change
			if(time - lastChangedTime >= 1000000)
			{
				changeSlide(true);
				lastChangedTime = time;
			}
		}
		if(force < -75) {
			//changeSlide if one second or more elapsed since last change
			if(time - lastChangedTime >= 1000000)
			{
				changeSlide(false);
				lastChangedTime = time;
			}
		}
	}
	private bool changeSlide(bool next)
	{
		string executable = "";
		if(next)
			executable = "pathTo/testing-stuff/" + "slideNext.sh";
			//executable = "/home/xavier/informatica/progs_meus/chronojump/chronojump/testing-stuff/" + "slideNext.sh";
		else
			executable = "pathTo/testing-stuff/" + "slidePrior.sh";
			//executable = "/home/xavier/informatica/progs_meus/chronojump/chronojump/testing-stuff/" + "slidePrior.sh";
		List<string> parameters = new List<string>();

		LogB.Information("\nCalling slide ----->");

		ExecuteProcess.Result execute_result = ExecuteProcess.run (executable, parameters, true, true);

		LogB.Information("\n<------ Done calling slide");
		return execute_result.success;
	}

	// ------------------------------------------------ end of slides stuff for presentations

	private void connectWidgetsForceSensor (Gtk.Builder builder)
	{
		//capture tab
		button_combo_force_sensor_exercise_capture_left = (Gtk.Button) builder.GetObject ("button_combo_force_sensor_exercise_capture_left");
		button_combo_force_sensor_exercise_capture_right = (Gtk.Button) builder.GetObject ("button_combo_force_sensor_exercise_capture_right");
		hbox_force_capture_buttons = (Gtk.HBox) builder.GetObject ("hbox_force_capture_buttons");
		force_sensor_exercise_filter = (Gtk.Entry) builder.GetObject ("force_sensor_exercise_filter");
		hbox_combo_force_sensor_exercise = (Gtk.HBox) builder.GetObject ("hbox_combo_force_sensor_exercise");
		frame_force_sensor_elastic = (Gtk.Frame) builder.GetObject ("frame_force_sensor_elastic");
		button_stiffness_detect = (Gtk.Button) builder.GetObject ("button_stiffness_detect");
		label_button_force_sensor_stiffness = (Gtk.Label) builder.GetObject ("label_button_force_sensor_stiffness");
		image_button_force_sensor_stiffness_problem = (Gtk.Image) builder.GetObject ("image_button_force_sensor_stiffness_problem");
		radio_force_sensor_laterality_both = (Gtk.RadioButton) builder.GetObject ("radio_force_sensor_laterality_both");
		radio_force_sensor_laterality_l = (Gtk.RadioButton) builder.GetObject ("radio_force_sensor_laterality_l");
		radio_force_sensor_laterality_r = (Gtk.RadioButton) builder.GetObject ("radio_force_sensor_laterality_r");
		vbox_force_sensor_adjust_actions = (Gtk.VBox) builder.GetObject ("vbox_force_sensor_adjust_actions");
		button_force_sensor_tare = (Gtk.Button) builder.GetObject ("button_force_sensor_tare");
		button_force_sensor_calibrate = (Gtk.Button) builder.GetObject ("button_force_sensor_calibrate");
		label_force_sensor_value_max = (Gtk.Label) builder.GetObject ("label_force_sensor_value_max");
		label_force_sensor_value = (Gtk.Label) builder.GetObject ("label_force_sensor_value");
		label_force_sensor_value_min = (Gtk.Label) builder.GetObject ("label_force_sensor_value_min");
		label_force_sensor_value_best_second = (Gtk.Label) builder.GetObject ("label_force_sensor_value_best_second");
		label_force_sensor_value_rfd = (Gtk.Label) builder.GetObject ("label_force_sensor_value_rfd");
		//vscale_force_sensor = (Gtk.VScale) builder.GetObject ("vscale_force_sensor");
		spin_force_sensor_calibration_kg_value = (Gtk.SpinButton) builder.GetObject ("spin_force_sensor_calibration_kg_value");
		box_force_sensor_capture_magnitudes = (Gtk.Box) builder.GetObject ("box_force_sensor_capture_magnitudes");
		check_force_sensor_capture_show_distance = (Gtk.CheckButton) builder.GetObject ("check_force_sensor_capture_show_distance");
		check_force_sensor_capture_show_speed = (Gtk.CheckButton) builder.GetObject ("check_force_sensor_capture_show_speed");
		check_force_sensor_capture_show_power = (Gtk.CheckButton) builder.GetObject ("check_force_sensor_capture_show_power");
		image_force_sensor_capture_show_distance = (Gtk.Image) builder.GetObject ("image_force_sensor_capture_show_distance");
		image_force_sensor_capture_show_speed = (Gtk.Image) builder.GetObject ("image_force_sensor_capture_show_speed");
		image_force_sensor_capture_show_power = (Gtk.Image) builder.GetObject ("image_force_sensor_capture_show_power");
		button_force_sensor_image_save_signal = (Gtk.Button) builder.GetObject ("button_force_sensor_image_save_signal");
		force_capture_drawingarea_cairo = (Gtk.DrawingArea) builder.GetObject ("force_capture_drawingarea_cairo");
		button_force_sensor_exercise_edit = (Gtk.Button) builder.GetObject ("button_force_sensor_exercise_edit");
		button_force_sensor_exercise_delete = (Gtk.Button) builder.GetObject ("button_force_sensor_exercise_delete");
		force_sensor_adjust_label_message = (Gtk.Label) builder.GetObject ("force_sensor_adjust_label_message");
		box_combo_force_sensor_capture_options = (Gtk.Box) builder.GetObject ("box_combo_force_sensor_capture_options");
	}
}
