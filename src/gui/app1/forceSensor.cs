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
 * Copyright (C) 2017-2018   Xavier de Blas <xaviblas@gmail.com>
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
using Mono.Unix;


public partial class ChronoJumpWindow 
{
	[Widget] Gtk.MenuItem menuitem_force_sensor_open_folder;
	[Widget] Gtk.MenuItem menuitem_force_sensor_check_version;

	//capture tab
	[Widget] Gtk.HBox hbox_force_capture_buttons;
	[Widget] Gtk.HBox hbox_combo_force_sensor_exercise;
	[Widget] Gtk.ComboBox combo_force_sensor_exercise;
	[Widget] Gtk.Button button_force_sensor_stiffness;
	[Widget] Gtk.Label label_button_force_sensor_stiffness;
	[Widget] Gtk.Image image_button_force_sensor_stiffness_problem;
	[Widget] Gtk.ComboBox combo_force_sensor_capture_options;
	[Widget] Gtk.RadioButton radio_force_sensor_laterality_both;
	[Widget] Gtk.RadioButton radio_force_sensor_laterality_l;
	[Widget] Gtk.RadioButton radio_force_sensor_laterality_r;
	[Widget] Gtk.HBox hbox_force_sensor_adjust_actions;
	[Widget] Gtk.Button button_force_sensor_tare;
	[Widget] Gtk.Button button_force_sensor_calibrate;
	[Widget] Gtk.Label label_force_sensor_value_max;
	[Widget] Gtk.Label label_force_sensor_value;
	[Widget] Gtk.Label label_force_sensor_value_min;
	//[Widget] Gtk.VScale vscale_force_sensor;
	[Widget] Gtk.SpinButton spin_force_sensor_calibration_kg_value;
	[Widget] Gtk.Button button_force_sensor_image_save_signal;
	[Widget] Gtk.DrawingArea force_capture_drawingarea;
	[Widget] Gtk.Button button_force_sensor_exercise_edit;
	[Widget] Gtk.Button button_force_sensor_exercise_delete;

	ForceSensorExerciseWindow forceSensorExerciseWin;
	ForceSensorElasticBandsWindow forceSensorElasticBandsWin;

	Gdk.Pixmap force_capture_pixmap = null;

	Thread forceCaptureThread;
	static bool forceProcessFinish;
	static bool forceProcessCancel;
	static bool forceProcessError;
	ForceSensorCapturePoints fscPoints;

	Thread forceOtherThread; //for messages on: capture, tare, calibrate
	static string forceSensorOtherMessage = "";
	static bool forceSensorOtherMessageShowSeconds;
	static DateTime forceSensorTimeStart;
	static string lastForceSensorFile = "";
	static string lastForceSensorFullPath = "";

	int usbDisconnectedCount;
	int usbDisconnectedLastTime;

	private ForceSensor currentForceSensor;
	private ForceSensorExercise currentForceSensorExercise;
	DateTime forceSensorTimeStartCapture;


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
	static bool redoingPoints; //don't draw while redoing points (adjusting screen)

	static bool forceCaptureStartMark; 	//Just needed to display Capturing message (with seconds)
	static ForceSensorValues forceSensorValues;

	string forceSensorPortName;
	SerialPort portFS; //Attention!! Don't reopen port because arduino makes reset and tare, calibration... is lost
	bool portFSOpened;
	bool forceSensorBinaryCapture;
	int forceSensorTopRectangleAtOperationStart; //operation can be capture, load

	Gdk.GC pen_black_force_capture;
	Gdk.GC pen_red_force_capture;
	Gdk.GC pen_white_force_capture;
	//Gdk.GC pen_yellow_force_capture;
	Gdk.GC pen_blue_light_force_capture;
	Gdk.GC pen_yellow_dark_force_capture;
	//Gdk.GC pen_orange_dark_force_capture;
	Gdk.GC pen_blue_dark_force_capture;
	Gdk.GC pen_gray_force_capture;
	Gdk.GC pen_gray_force_capture_discont;
	Pango.Layout layout_force_text;
	Gdk.Colormap colormapForce = Gdk.Colormap.System;

	string forceSensorNotConnectedString =
		Catalog.GetString("Force sensor is not detected!") + " " +
		Catalog.GetString("Plug cable and click on 'device' button.");


	private void initForceSensor ()
	{
		notebook_force_sensor_analyze.CurrentPage = 1; 	//start on 1: force_general_analysis
		createForceExerciseCombo();
		createComboForceSensorCaptureOptions();
		createForceAnalyzeCombos();
		setForceDurationRadios();
		setRFDValues();
		setImpulseValue();
	}


	private void force_graphs_init()
	{
		colormapForce = Gdk.Colormap.System;
		colormapForce.AllocColor (ref UtilGtk.BLACK,true,true);
		colormapForce.AllocColor (ref UtilGtk.GRAY,true,true);
		colormapForce.AllocColor (ref UtilGtk.RED_PLOTS,true,true);
		colormapForce.AllocColor (ref UtilGtk.WHITE,true,true);
		colormapForce.AllocColor (ref UtilGtk.YELLOW,true,true);
		colormapForce.AllocColor (ref UtilGtk.YELLOW_DARK,true,true);
		colormapForce.AllocColor (ref UtilGtk.ORANGE_DARK,true,true);
		colormapForce.AllocColor (ref UtilGtk.BLUE_DARK,true,true);
		colormapForce.AllocColor (ref UtilGtk.LIGHT_BLUE_PLOTS,true,true);

		pen_black_force_capture = new Gdk.GC(force_capture_drawingarea.GdkWindow);
		pen_black_force_capture.Foreground = UtilGtk.BLACK;
		//pen_black_force_capture.SetLineAttributes (2, Gdk.LineStyle.Solid, Gdk.CapStyle.NotLast, Gdk.JoinStyle.Miter);
		//this makes the lines less spiky:
		pen_black_force_capture.SetLineAttributes (preferences.forceSensorGraphsLineWidth, Gdk.LineStyle.Solid, Gdk.CapStyle.Round, Gdk.JoinStyle.Round);

		pen_red_force_capture = new Gdk.GC(force_capture_drawingarea.GdkWindow);
		pen_red_force_capture.Foreground = UtilGtk.RED_PLOTS;
		pen_red_force_capture.SetLineAttributes (2, Gdk.LineStyle.Solid, Gdk.CapStyle.NotLast, Gdk.JoinStyle.Miter);

		pen_white_force_capture = new Gdk.GC(force_capture_drawingarea.GdkWindow);
		pen_white_force_capture.Foreground = UtilGtk.WHITE;
		pen_white_force_capture.SetLineAttributes (2, Gdk.LineStyle.Solid, Gdk.CapStyle.NotLast, Gdk.JoinStyle.Miter);

		pen_blue_light_force_capture = new Gdk.GC(force_capture_drawingarea.GdkWindow);
		pen_blue_light_force_capture.Foreground = UtilGtk.LIGHT_BLUE_PLOTS;
		pen_blue_light_force_capture.SetLineAttributes (2, Gdk.LineStyle.Solid, Gdk.CapStyle.NotLast, Gdk.JoinStyle.Miter);

		//pen_yellow_force_capture = new Gdk.GC(force_capture_drawingarea.GdkWindow);
		//pen_yellow_force_capture.Foreground = UtilGtk.YELLOW;
		//pen_yellow_force_capture.SetLineAttributes (2, Gdk.LineStyle.Solid, Gdk.CapStyle.NotLast, Gdk.JoinStyle.Miter);

		pen_yellow_dark_force_capture = new Gdk.GC(force_capture_drawingarea.GdkWindow);
		pen_yellow_dark_force_capture.Foreground = UtilGtk.YELLOW_DARK;
		pen_yellow_dark_force_capture.SetLineAttributes (2, Gdk.LineStyle.Solid, Gdk.CapStyle.NotLast, Gdk.JoinStyle.Miter);

		//pen_orange_dark_force_capture = new Gdk.GC(force_capture_drawingarea.GdkWindow);
		//pen_orange_dark_force_capture.Foreground = UtilGtk.ORANGE_DARK;
		//pen_orange_dark_force_capture.SetLineAttributes (3, Gdk.LineStyle.OnOffDash, Gdk.CapStyle.NotLast, Gdk.JoinStyle.Miter);

		pen_blue_dark_force_capture = new Gdk.GC(force_capture_drawingarea.GdkWindow);
		pen_blue_dark_force_capture.Foreground = UtilGtk.BLUE_DARK;
		pen_blue_dark_force_capture.SetLineAttributes (3, Gdk.LineStyle.OnOffDash, Gdk.CapStyle.NotLast, Gdk.JoinStyle.Miter);

		pen_gray_force_capture_discont = new Gdk.GC(force_capture_drawingarea.GdkWindow);
		pen_gray_force_capture_discont.Foreground = UtilGtk.GRAY;
		pen_gray_force_capture_discont.SetLineAttributes (1, Gdk.LineStyle.OnOffDash, Gdk.CapStyle.NotLast, Gdk.JoinStyle.Miter);

		pen_gray_force_capture = new Gdk.GC(force_capture_drawingarea.GdkWindow);
		pen_gray_force_capture.Foreground = UtilGtk.GRAY;
		pen_gray_force_capture.SetLineAttributes (1, Gdk.LineStyle.Solid, Gdk.CapStyle.NotLast, Gdk.JoinStyle.Miter);

		layout_force_text = new Pango.Layout (force_capture_drawingarea.PangoContext);
		layout_force_text.FontDescription = Pango.FontDescription.FromString ("Courier 10");
	}


	//Attention: no GTK here!!
	private bool forceSensorConnect()
	{
		LogB.Information(" FS connect 0 ");
		if(chronopicRegister.ConnectedOfType(ChronopicRegisterPort.Types.ARDUINO_FORCE) == null)
		{
			forceSensorOtherMessage = forceSensorNotConnectedString;
			return false;
		}

		LogB.Information(" FS connect 1 ");
		forceSensorPortName = chronopicRegister.ConnectedOfType(ChronopicRegisterPort.Types.ARDUINO_FORCE).Port;
		LogB.Information(" FS connect 2 ");
		if(forceSensorPortName == null || forceSensorPortName == "")
		{
			forceSensorOtherMessage = "Please, select port!";
			return false;
		}
		LogB.Information(" FS connect 3 ");
		forceSensorOtherMessage = "Connecting ...";

		portFS = new SerialPort(forceSensorPortName, 115200); //forceSensor
		//portFS = new SerialPort(forceSensorPortName, 1000000); //forceSensor
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

		string version = forceSensorCheckVersionDo();
		LogB.Information("Version found: [" + version + "]");

		if(version == "0.1")
		{
			LogB.Information(" FS connect 6b, version 0.1: adjusting parameters...");

			//set_tare
			if(! forceSensorSendCommand("set_tare:" + preferences.forceSensorTare.ToString() + ";",
						"Setting previous tare ...", "Catched adjusting tare"))
				return false;

			//read confirmation data
			if(! forceSensorReceiveFeedback("Tare set"))
				return false;


			//set_calibration_factor
			if(! forceSensorSendCommand("set_calibration_factor:" + Util.ConvertToPoint(preferences.forceSensorCalibrationFactor) + ";",
						"Setting previous calibration factor ...", "Catched adjusting calibration factor"))
				return false;

			//read confirmation data
			if(! forceSensorReceiveFeedback("Calibration factor set"))
				return false;
		}

		bool forceSensorBinaryCapture = false;
                double versionDouble = Convert.ToDouble(Util.ChangeDecimalSeparator(version));
		if(versionDouble >= Convert.ToDouble(Util.ChangeDecimalSeparator("0.3"))) //from 0.3 versions can be binary
			forceSensorBinaryCapture = forceSensorCheckBinaryCapture();

		LogB.Information("forceSensorBinaryCapture = " + forceSensorBinaryCapture.ToString());

		portFSOpened = true;
		forceSensorOtherMessage = "Connected!";
		LogB.Information(" FS connect 7: connected and adjusted!");
		return true;
	}
	private void forceSensorDisconnect()
	{
		portFS.Close();
		portFSOpened = false;
		event_execute_label_message.Text = "Disconnected!";
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

	//use this method for other feedback, but beware some of the commands do a Trim on ReadLine
	private bool forceSensorReceiveFeedback(string expected)
	{
		string str = "";
		do {
			Thread.Sleep(100); //sleep to let arduino start reading
			try {
				str = portFS.ReadLine();
			} catch {
				forceSensorOtherMessage = "Disconnected";
				return false;
			}
			LogB.Information("init string: " + str);
		}
		while(! str.Contains(expected));
		return true;
	}

	enum forceSensorOtherModeEnum { TARE, CALIBRATE, CAPTURE_PRE, TARE_AND_CAPTURE_PRE, CHECK_VERSION }
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
		}

		if(chronopicRegister.NumConnectedOfType(ChronopicRegisterPort.Types.ARDUINO_FORCE) == 0)
		{
			event_execute_label_message.Text = forceSensorNotConnectedString;
			return;
		}

		capturingForce = arduinoCaptureStatus.STOP;
		forceSensorTimeStart = DateTime.Now;
		forceSensorOtherMessageShowSeconds = true;

		if(pen_black_force_capture == null)
			force_graphs_init();

		if(o == (object) button_force_sensor_tare)
		{
			hbox_force_sensor_adjust_actions.Sensitive = false;
			forceSensorOtherMode = forceSensorOtherModeEnum.TARE;
			forceOtherThread = new Thread(new ThreadStart(forceSensorTare));
		}
		else if(o == (object) button_force_sensor_calibrate)
		{
			hbox_force_sensor_adjust_actions.Sensitive = false;
			forceSensorOtherMode = forceSensorOtherModeEnum.CALIBRATE;
			forceOtherThread = new Thread(new ThreadStart(forceSensorCalibrate));
		}
		else if (o == (object) button_execute_test)
		{
			forceSensorButtonsSensitive(false);
			sensitiveLastTestButtons(false);
			contactsShowCaptureDoingButtons(true);
			image_force_sensor_graph.Sensitive = false; //unsensitivize the RFD image (can contain info of previous data)

			//textview_force_sensor_capture_comment.Buffer.Text = "";
			textview_contacts_signal_comment.Buffer.Text = "";

			if(currentForceSensorExercise.TareBeforeCapture)
			{
				forceSensorOtherMode = forceSensorOtherModeEnum.TARE_AND_CAPTURE_PRE;
				forceOtherThread = new Thread(new ThreadStart(forceSensorTareAndCapturePre_noGTK));
			} else {
				forceSensorOtherMode = forceSensorOtherModeEnum.CAPTURE_PRE;
				forceOtherThread = new Thread(new ThreadStart(forceSensorCapturePre_noGTK));
			}
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
		button_contacts_exercise.Sensitive = sensitive;
		button_execute_test.Sensitive = sensitive;
		button_force_sensor_analyze_load.Sensitive = sensitive;
		hbox_contacts_camera.Sensitive = sensitive;

		//other gui buttons
		main_menu.Sensitive = sensitive;
		hbox_contacts_sup_capture_analyze_two_buttons.Sensitive = sensitive;
		frame_persons.Sensitive = sensitive;
		hbox_top_person.Sensitive = sensitive;
		hbox_chronopics_and_more.Sensitive = sensitive;
	}

	private void forceSensorPersonChanged()
	{
		blankForceSensorInterface();
	}
	private void blankForceSensorInterface()
	{
		currentForceSensor = new ForceSensor();

		/*
		 * without this, on change person fsAI graph will blank
		 * but first makes no graph when resize,
		 * and second does not allow the graph to be done on going to RFD automatic and return
		 */
		fsAI = null;
		lastForceSensorFullPath = null;

		button_contacts_recalculate.Sensitive = false;
		textview_contacts_signal_comment.Buffer.Text = "";
		notebook_force_sensor_analyze.Sensitive = false;
		button_force_sensor_analyze_options.Sensitive = false;
		button_force_sensor_analyze_analyze.Sensitive = false;
		button_delete_last_test.Sensitive = false;
		if(force_capture_drawingarea != null && force_capture_pixmap != null)
			UtilGtk.ErasePaint(force_capture_drawingarea, force_capture_pixmap);
		if(force_sensor_ai_drawingarea != null && force_sensor_ai_pixmap != null)
			UtilGtk.ErasePaint(force_sensor_ai_drawingarea, force_sensor_ai_pixmap);

		label_force_sensor_value_max.Text = "";
		label_force_sensor_value.Text = "";
		label_force_sensor_value_min.Text = "";

		button_force_sensor_image_save_signal.Sensitive = false;
	}

	private bool pulseGTKForceSensorOther ()
	{
		string secondsStr = "";
		if(forceSensorOtherMessage != "")
		{
			if(forceSensorOtherMessageShowSeconds)
			{
				TimeSpan ts = DateTime.Now.Subtract(forceSensorTimeStart);
				double seconds = ts.TotalSeconds;
				secondsStr = " (" + Util.TrimDecimals(seconds, 0) + " s)";
			}
		}

		if(forceOtherThread.IsAlive)
		{
			/*
			if(forceSensorOtherMode == forceSensorOtherModeEnum.TARE ||
				forceSensorOtherMode == forceSensorOtherModeEnum.CALIBRATE)
				event_execute_label_message.Text = forceSensorOtherMessage + secondsStr;
			else
			*/
				event_execute_label_message.Text = forceSensorOtherMessage + secondsStr;
		}
		else
		{
			event_execute_label_message.Text = forceSensorOtherMessage;
			LogB.ThreadEnding();

			if(forceSensorOtherMode == forceSensorOtherModeEnum.TARE ||
				forceSensorOtherMode == forceSensorOtherModeEnum.CALIBRATE)
			{
				hbox_force_sensor_adjust_actions.Sensitive = true;
				return false;
			}
			else if(forceSensorOtherMode == forceSensorOtherModeEnum.CHECK_VERSION)
			{
				forceSensorButtonsSensitive(true);
				event_execute_label_message.Text = forceSensorOtherMessage;
			}
			else if(forceSensorOtherMode == forceSensorOtherModeEnum.TARE_AND_CAPTURE_PRE || forceSensorOtherMode == forceSensorOtherModeEnum.CAPTURE_PRE)
				forceSensorCapturePre2_GTK_cameraCall();

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

		// 1 send tare command
		if(! forceSensorSendCommand("tare:", "Taring ...", "Catched force taring"))
			return;

		// 2 read confirmation data
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
			// 3 get tare factor
			if (portFS.BytesToRead > 0)
				LogB.Information("PRE_get_tare bytes: " + portFS.ReadExisting());

			if(! forceSensorSendCommand("get_tare:", "Checking ...", "Catched at get_tare"))
				return;

			str = Util.ChangeDecimalSeparator(portFS.ReadLine().Trim());
		}

		// 4 update preferences and SQL with new tare
		if(Util.IsNumber(str, true))
			preferences.UpdateForceSensorTare(Convert.ToDouble(str));

		// 5 print message
		forceSensorOtherMessageShowSeconds = false;
		forceSensorOtherMessage = "Tared!";
	}

	//Attention: no GTK here!!
	private void forceSensorCalibrate()
	{
		// 0 connect if needed
		if(! portFSOpened)
			if(! forceSensorConnect())
				return;

		// 1 send calibrate command
		if(! forceSensorSendCommand("calibrate:" + Util.ConvertToPoint(spin_force_sensor_calibration_kg_value.Value) + ";",
					"Calibrating ...", "Catched force calibrating"))
			return;

		// 2 read confirmation data
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
			// 3 get calibration factor
			if (portFS.BytesToRead > 0)
				LogB.Information("PRE_get_calibrationfactor bytes: " + portFS.ReadExisting());

			if(! forceSensorSendCommand("get_calibration_factor:", "Checking ...", "Catched at get_calibration_factor"))
				return;

			// 4 update preferences and SQL with new calibration factor
			str = Util.ChangeDecimalSeparator(portFS.ReadLine().Trim());
		}

		if(Util.IsNumber(str, true))
			preferences.UpdateForceSensorCalibration(
					spin_force_sensor_calibration_kg_value.Value, Convert.ToDouble(str));

		// 5 print message
		forceSensorOtherMessageShowSeconds = false;
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
	private string forceSensorCheckVersionDo()
	{
		if (portFS.BytesToRead > 0)
			LogB.Information("check_version read possible bytes: " + portFS.ReadExisting());

		if(! forceSensorSendCommand("get_version:", "Checking version ...", "Catched checking version"))
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

		forceSensorOtherMessageShowSeconds = false;
		forceSensorOtherMessage = str;

		/*
		 * //return the version without "Force_Sensor-"
		 * return(str.Remove(0,13));
		 * //can be problematic, once detected:
		 * //init string: ^@;Force_Sensor-0.3
		 * //Version found: [r-0.3]
		 * //so now using portFS.ReadExisting() above, and also regex and not the 0,13 positions
		 */

		Match match = Regex.Match(str, @"Force_Sensor-(\d+\.\d+)");
		if(match.Groups.Count == 1)
			return str = match.Value;
		else
			return "0.3"; //if there is a problem default to 0.3
	}

	//Attention: no GTK here!!
	private bool forceSensorCheckBinaryCapture()
	{
		if(! forceSensorSendCommand("get_transmission_format:", "Checking transmission format ...", "Catched checking transmission format"))
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

		forceSensorOtherMessageShowSeconds = false;
		forceSensorOtherMessage = str;

		return (str.Contains("binary"));
	}

	//Attention: no GTK here!!
	private void forceSensorTareAndCapturePre_noGTK()
	{
		forceSensorTare();
		forceSensorCapturePre_noGTK();
	}
	//Attention: no GTK here!!
	private void forceSensorCapturePre_noGTK()
	{
		if(! portFSOpened)
			if(! forceSensorConnect())
				return;

		forceSensorOtherMessage = "Please, wait ...";
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
		button_force_sensor_image_save_signal.Sensitive = false;
		button_force_sensor_analyze_analyze.Sensitive = false;
		forceCaptureStartMark = false;
		//vscale_force_sensor.Value = 0;
		label_force_sensor_value_max.Text = "0 N";
		label_force_sensor_value.Text = "0 N";
		label_force_sensor_value_min.Text = "0 N";
		label_force_sensor_analyze.Text = "";
		label_force_sensor_analyze.Visible = false;

		forceProcessFinish = false;
		forceProcessCancel = false;
		forceProcessError = false;

		//To know if USB has been disconnected
		usbDisconnectedCount = 0;
		usbDisconnectedLastTime = 0;

		//initialize
		forceSensorValues = new ForceSensorValues();

		UtilGtk.ErasePaint(force_capture_drawingarea, force_capture_pixmap);
		fscPoints = new ForceSensorCapturePoints(
				ForceSensorCapturePoints.GraphTypes.FORCESIGNAL,
				force_capture_drawingarea.Allocation.Width,
				force_capture_drawingarea.Allocation.Height,
				preferences.forceSensorCaptureWidthSeconds
				);

		setForceSensorTopAtOperationStart();

		if(fscPoints.RealHeightG < forceSensorTopRectangleAtOperationStart)
			fscPoints.RealHeightG = forceSensorTopRectangleAtOperationStart;

		LogB.Information("RealHeight = " + fscPoints.RealHeightG.ToString());

		forcePaintHVLines(ForceSensorGraphs.CAPTURE, fscPoints.RealHeightG, ForceSensorCapturePoints.DefaultRealHeightGNeg, 10, false);
		//draw horizontal rectangle of feedback
		if(preferences.forceSensorCaptureFeedbackActive)
			forceSensorSignalPlotFeedbackRectangle(fscPoints, force_capture_drawingarea.Allocation.Width,
					force_capture_drawingarea, force_capture_pixmap, pen_blue_light_force_capture);


		event_execute_ButtonFinish.Clicked -= new EventHandler(on_finish_clicked);
		event_execute_ButtonFinish.Clicked += new EventHandler(on_finish_clicked);
		
		event_execute_ButtonCancel.Clicked -= new EventHandler(on_cancel_clicked);
		event_execute_ButtonCancel.Clicked += new EventHandler(on_cancel_clicked);

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
                                false, getExerciseIDFromAnyCombo(combo_force_sensor_exercise, forceSensorComboExercisesString, false), false)[0];
	}

	private void forceSensorCaptureDo()
	{
		lastChangedTime = 0;

		if(! forceSensorSendCommand("start_capture:", "Preparing capture...", "Catched force capturing"))
		{
			forceProcessError = true;
			return;
		}

		string str = "";

		do {
			Thread.Sleep(100); //sleep to let arduino start reading
			try {
				str = portFS.ReadLine();
			} catch {
				forceProcessError = true;
				return;
			}
			LogB.Information("init string: " + str);
		}
		while(! str.Contains("Starting capture"));

		forceCaptureStartMark = true;
		forceSensorTimeStart = DateTime.Now; //to have an active count of capture time
		forceSensorTimeStartCapture = forceSensorTimeStart; //to have same DateTime on filename and on sql datetime
		capturingForce = arduinoCaptureStatus.CAPTURING;
//		string captureComment = UtilGtk.TextViewGetCommentValidSQL(textview_force_sensor_capture_comment);
		string captureComment = UtilGtk.TextViewGetCommentValidSQL(textview_contacts_signal_comment);

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
		str = "";
		int firstTime = 0;
//		bool forceSensorBinary = forceSensorBinaryCapture();

		//LogB.Information("pre bucle");
		//LogB.Information(string.Format("forceProcessFinish: {0}, forceProcessCancel: {1}, forceProcessError: {2}", forceProcessFinish, forceProcessCancel, forceProcessError));
		while(! forceProcessFinish && ! forceProcessCancel && ! forceProcessError)
		{
			LogB.Information("at bucle");
			int time = 0;
			double force = 0;

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
				str = portFS.ReadLine();

				//check if there is one and only one ';'
				if( ! (str.Contains(";") && str.IndexOf(";") == str.LastIndexOf(";")) )
					continue;

				string [] strFull = str.Split(new char[] {';'});
				//LogB.Information("str: " + str);

				LogB.Information("time: " + strFull[0]);
				if(! Util.IsNumber(Util.ChangeDecimalSeparator(strFull[0]), true))
					continue;

				LogB.Information("force: " + strFull[1]);
				if(! Util.IsNumber(Util.ChangeDecimalSeparator(strFull[1]), true))
					continue;

				time = Convert.ToInt32(strFull[0]);
				force = Convert.ToDouble(Util.ChangeDecimalSeparator(strFull[1]));
			}

			//measurement does not start at 0 time. When we start receiving data, mark this as firstTime
			if(firstTime == 0)
				firstTime = time;

			//use this to have time starting at 0
			time -= firstTime;

			LogB.Information(string.Format("time: {0}, force: {1}", time, force));
			//forceCalculated have abs or inverted

			double forceCalculated = ForceSensor.CalculeForceResultantIfNeeded (force, forceSensorCaptureOption,
					currentForceSensorExercise, currentPersonSession.Weight);

			if(forceSensorCaptureOption != ForceSensor.CaptureOptions.NORMAL)
				LogB.Information(string.Format("with abs or inverted flag: time: {0}, force: {1}", time, forceCalculated));

			writer.WriteLine(time.ToString() + ";" + force.ToString()); //on file force is stored without flags

			forceSensorValues.TimeLast = time;
			forceSensorValues.ValueLast = forceCalculated;

			forceSensorValues.SetMaxMinIfNeeded(forceCalculated, time);

			fscPoints.Add(time, forceCalculated);
			fscPoints.NumCaptured ++;
			if(fscPoints.OutsideGraphChangeValues(preferences.forceSensorCaptureScroll))
			{
				redoingPoints = true;
				fscPoints.Redo();

				//mark meaning screen should be erased
				//but only applies when not in scroll
				//because scroll already erases screen all the time, paintHVLines and plot feedback rectangle
				if(! (preferences.forceSensorCaptureScroll && fscPoints.ScrollStartedAtCount > 0))
					fscPoints.NumPainted = -1;

				redoingPoints = false;
			}

			//changeSlideIfNeeded(time, force);
		}

		//LogB.Information(string.Format("forceProcessFinish: {0}, forceProcessCancel: {1}, forceProcessError: {2}", forceProcessFinish, forceProcessCancel, forceProcessError));
		LogB.Information("Calling end_capture");
		if(! forceSensorSendCommand("end_capture:", "Ending capture ...", "Catched ending capture"))
		{
			forceProcessError = true;
			capturingForce = arduinoCaptureStatus.STOP;
			Util.FileDelete(fileName);
			return;
		}

		LogB.Information("Waiting end_capture");
		do {
			Thread.Sleep(10);
			try {
				str = portFS.ReadLine();
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
		capturingForce = arduinoCaptureStatus.STOP;

		if(forceProcessCancel || forceProcessError)
			Util.FileDelete(fileName);
		else {
			//call graph
			File.Copy(fileName, UtilEncoder.GetmifCSVFileName(), true); //can be overwritten
			lastForceSensorFullPath = fileName;
			capturingForce = arduinoCaptureStatus.COPIED_TO_TMP;
		}
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
			button_video_play_this_test_contacts.Sensitive = false;
			if(forceProcessFinish)
			{
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
							currentForceSensorExercise.Name);

					currentForceSensor.UniqueID = currentForceSensor.InsertSQL(false);

					//stop camera
					if(webcamEnd (Constants.TestTypes.FORCESENSOR, currentForceSensor.UniqueID))
					{
						//add the videoURL to SQL
						currentForceSensor.VideoURL = Util.GetVideoFileName(currentSession.UniqueID,
								Constants.TestTypes.FORCESENSOR,
								currentForceSensor.UniqueID);
						currentForceSensor.UpdateSQL(false);
						label_video_feedback.Text = "";
						button_video_play_this_test_contacts.Sensitive = true;
					}

					Thread.Sleep (250); //Wait a bit to ensure is copied
					sensitiveLastTestButtons(true);
					contactsShowCaptureDoingButtons(false);

					fscPoints.InitRealWidthHeight(preferences.forceSensorCaptureWidthSeconds);

					forceSensorDoSignalGraphPlot();

					//do not calculate RFD until analyze button there is clicked
					//forceSensorDoRFDGraph();

					//if drawingarea has still not shown, don't paint graph because GC screen is not defined
					if(force_sensor_ai_drawingareaShown)
					{
						forceSensorZoomDefaultValues();
						forceSensorDoGraphAI();
					}
					button_contacts_recalculate.Sensitive = true;
					button_delete_last_test.Sensitive = true;
					force_capture_drawingarea.Sensitive = true;
					button_force_sensor_image_save_signal.Sensitive = true;
					notebook_force_sensor_analyze.Sensitive = true;
					button_force_sensor_analyze_options.Sensitive = true;
					button_force_sensor_analyze_analyze.Sensitive = true;

					if( configChronojump.Exhibition &&
							( configChronojump.ExhibitionStationType == ExhibitionTest.testTypes.FORCE_ROPE ||
							  configChronojump.ExhibitionStationType == ExhibitionTest.testTypes.FORCE_SHOT ) )
						SqliteJson.UploadExhibitionTest(getExhibitionTestFromGui(configChronojump.ExhibitionStationType, forceSensorValues.Max));

				}
			} else if(forceProcessCancel || forceProcessError)
			{
				//stop the camera (and do not save)
				webcamEnd (Constants.TestTypes.FORCESENSOR, -1);
				sensitiveLastTestButtons(false);
				contactsShowCaptureDoingButtons(false);

				if(forceProcessCancel)
					event_execute_label_message.Text = "Cancelled.";
				else
					event_execute_label_message.Text = forceSensorNotConnectedString;

				button_force_sensor_image_save_signal.Sensitive = false;
				button_force_sensor_analyze_analyze.Sensitive = false;
				button_force_sensor_image_save_rfd_auto.Sensitive = false;
				button_force_sensor_image_save_rfd_manual.Sensitive = false;
				button_contacts_recalculate.Sensitive = false;
				button_delete_last_test.Sensitive = false;
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
		if(forceCaptureStartMark)
		{
			event_execute_label_message.Text = "Capturing" +
					" (" + Util.TrimDecimals(DateTime.Now.Subtract(forceSensorTimeStart).TotalSeconds, 0) + " s)";
		}
LogB.Information(" fs F ");

		if(capturingForce == arduinoCaptureStatus.CAPTURING)
		{
LogB.Information(" fs G ");
			label_force_sensor_value_max.Text = string.Format("{0:0.##} N", forceSensorValues.Max);
			label_force_sensor_value_min.Text = string.Format("{0:0.##} N", forceSensorValues.Min);
			label_force_sensor_value.Text = string.Format("{0:0.##} N", forceSensorValues.ValueLast);


LogB.Information(" fs H ");
			//------------------- realtime graph -----------------
			if(redoingPoints || fscPoints == null || fscPoints.Points == null || force_capture_drawingarea == null)
				return true;

LogB.Information(" fs H2 ");
			if(usbDisconnectedLastTime == forceSensorValues.TimeLast)
			{
				usbDisconnectedCount ++;
				if(usbDisconnectedCount >= 20)
				{
					event_execute_label_message.Text = "Disconnected!";
					forceProcessError = true;
					return true;
				}
			}
			else
			{
				usbDisconnectedLastTime = forceSensorValues.TimeLast;
				usbDisconnectedCount = 0;
			}

LogB.Information(" fs I ");
			//mark meaning screen should be erased
			//but only applies when not in scroll
			//because scroll already erases screen all the time, paintHVLines and plot feedback rectangle
			if(fscPoints.NumPainted == -1)
			{
				UtilGtk.ErasePaint(force_capture_drawingarea, force_capture_pixmap);
				fscPoints.NumPainted = 0;

				forcePaintHVLines(ForceSensorGraphs.CAPTURE, fscPoints.RealHeightG, forceSensorValues.Min * 2, fscPoints.RealWidthG, false);
				//draw horizontal rectangle of feedback
				if(preferences.forceSensorCaptureFeedbackActive)
					forceSensorSignalPlotFeedbackRectangle(fscPoints, force_capture_drawingarea.Allocation.Width,
							force_capture_drawingarea, force_capture_pixmap, pen_blue_light_force_capture);

			}

LogB.Information(" fs J ");
			//use these integers and this List to not have errors by updating data on the other thread
			int numCaptured = fscPoints.NumCaptured;
			int numPainted = fscPoints.NumPainted;
			List<Gdk.Point> points = fscPoints.Points;

			int toDraw = numCaptured - numPainted;

			LogB.Information("points count: " + points.Count +
					"; NumCaptured: " + numCaptured + "; NumPainted: " + numPainted +
					"; toDraw: " + toDraw.ToString() );

			//fixes crash at the end
			if(toDraw == 0)
				return true;

			//note that scroll mode will call NOScroll method until scroll starts
			if(preferences.forceSensorCaptureScroll && fscPoints.ScrollStartedAtCount > 0)
				forceSensorCaptureDoRealtimeGraphScroll(numCaptured, numPainted, toDraw, points);
			else
				forceSensorCaptureDoRealtimeGraphNOScroll(numCaptured, numPainted, toDraw, points);

			force_capture_drawingarea.QueueDraw(); // -- refresh

			/*
			 * update fscPoints.NumPainted by only if there's no -1 mark
			 * because -1 mark should prevail until repainted the screen
			 */
			if(fscPoints.NumPainted != -1)
				fscPoints.NumPainted = numCaptured;

LogB.Information(" fs Q ");
		}
LogB.Information(" fs R ");

		Thread.Sleep (25);
		//LogB.Information(" ForceSensor:"+ forceCaptureThread.ThreadState.ToString());
		return true;
	}

	private void forceSensorCaptureDoRealtimeGraphNOScroll(int numCaptured, int numPainted, int toDraw, List<Gdk.Point> points)
	{
		LogB.Information(" Graph NO Scroll ");
		Gdk.Point [] paintPoints;
		if(numPainted > 0)
			paintPoints = new Gdk.Point[toDraw +1]; // if something has been painted, connected first point with previous points
		else
			paintPoints = new Gdk.Point[toDraw];

		//TODO: if maxForce or minForce changed
		//blank the screen and paint the HVLine of max and min

		LogB.Information(" fs N ");
		int jStart = 0;
		int iStart = 0;
		if(numPainted > 0)
		{
			// if something has been painted, connected first point with previous points
			LogB.Information(" fs N0 ");

			paintPoints[0] = points[numPainted -1];
			jStart = 1;
			iStart = numPainted;

			LogB.Information(" fs N2 ");
		}
		LogB.Information(" fs O ");

		//i is related to what has been captured: points
		//j is related to what is going to be painted: paintPoints
		for(int j = jStart, i = iStart ; i < numCaptured ; i ++, j++)
		{
			LogB.Information(" fs O1 ");
			if(points.Count > i) 	//extra check to avoid going outside of arrays
				paintPoints[j] = points[i];

			LogB.Information(" fs O2 ");
		}
		force_capture_pixmap.DrawLines(pen_black_force_capture, paintPoints);
		LogB.Information(" fs P ");
	}

	private void forceSensorCaptureDoRealtimeGraphScroll(int numCaptured, int numPainted, int toDraw, List<Gdk.Point> points)
	{
		LogB.Information(" Graph Scroll ");
		Gdk.Point [] paintPoints = new Gdk.Point[fscPoints.ScrollStartedAtCount];

		int jStart = 0;
		int iStart = numCaptured - fscPoints.ScrollStartedAtCount; //-toDraw

		// if something has been painted, connected first point with previous points
		LogB.Information(" fs N0 ");

		UtilGtk.ErasePaint(force_capture_drawingarea, force_capture_pixmap);
		if(preferences.forceSensorCaptureFeedbackActive)
			forceSensorSignalPlotFeedbackRectangle(fscPoints, force_capture_drawingarea.Allocation.Width,
					force_capture_drawingarea, force_capture_pixmap, pen_blue_light_force_capture);

		forcePaintHVLines(ForceSensorGraphs.CAPTURE,
				getForceSensorMaxForceIncludingRectangle(forceSensorValues.Max),
				forceSensorValues.Min,
				Convert.ToInt32(forceSensorValues.TimeLast),
				true);

		LogB.Information(" fs O ");

		//i is related to what has been captured: points
		//j is related to what is going to be painted: paintPoints
		for(int j = jStart, i = iStart ; i < numCaptured ; i ++, j++)
		{
			LogB.Information(" fs O0 ");
			if(points.Count > i && j < fscPoints.ScrollStartedAtCount) 	//extra check to avoid going outside of arrays
			{
				LogB.Information(string.Format("i: {0}; j: {1}, paintPointsLength: {2}",
							i, j, fscPoints.ScrollStartedAtCount));
				paintPoints[j] = points[i];
				LogB.Information(" fs O1 ");
				paintPoints[j].X = fscPoints.GetTimeInPx(Convert.ToInt32(fscPoints.GetTimeAtCount(j)));
			}
			LogB.Information(" fs O2 ");
		}
		force_capture_pixmap.DrawLines(pen_black_force_capture, paintPoints);
		LogB.Information(" fs P ");
	}

	int force_capture_allocationXOld;
	bool force_capture_sizeChanged;
	public void on_force_capture_drawingarea_configure_event(object o, ConfigureEventArgs args)
	{
		if(force_capture_drawingarea == null)
			return;

		Gdk.EventConfigure ev = args.Event;
		Gdk.Window window = ev.Window;

		Gdk.Rectangle allocation = force_capture_drawingarea.Allocation;

		if(force_capture_pixmap == null || force_capture_sizeChanged ||
				allocation.Width != force_capture_allocationXOld)
		{
			force_capture_pixmap = new Gdk.Pixmap (window, allocation.Width, allocation.Height, -1);

			if(forceCaptureThread != null && forceCaptureThread.IsAlive)
				fscPoints.NumPainted = -1; //mark meaning screen should be erased and start painting from the beginning
			else {
				if(fscPoints == null)
					UtilGtk.ErasePaint(force_capture_drawingarea, force_capture_pixmap);
				else {
					fscPoints.WidthG = allocation.Width;
					fscPoints.HeightG = allocation.Height;
					fscPoints.Redo();
					forceSensorDoSignalGraphPlot();
				}
			}

			force_capture_sizeChanged = false;
		}

		force_capture_allocationXOld = allocation.Width;
	}
	public void on_force_capture_drawingarea_expose_event(object o, ExposeEventArgs args)
	{
		if(force_capture_drawingarea == null)
			return;

		/* in some mono installations, configure_event is not called, but expose_event yes.
		 * Do here the initialization
		 */
		LogB.Debug("capture drawing area EXPOSE");

		Gdk.Rectangle allocation = force_capture_drawingarea.Allocation;
		if(force_capture_pixmap == null || force_capture_sizeChanged ||
				allocation.Width != force_capture_allocationXOld)
		{
			force_capture_pixmap = new Gdk.Pixmap (force_capture_drawingarea.GdkWindow,
					allocation.Width, allocation.Height, -1);

			if(forceCaptureThread != null && forceCaptureThread.IsAlive)
				fscPoints.NumPainted = -1; //mark meaning screen should be erased and start painting from the beginning
			else {
				if(fscPoints == null)
					UtilGtk.ErasePaint(force_capture_drawingarea, force_capture_pixmap);
				else {
					fscPoints.WidthG = allocation.Width;
					fscPoints.HeightG = allocation.Height;
					fscPoints.Redo();
					forceSensorDoSignalGraphPlot();
				}
			}

			force_capture_sizeChanged = false;
		}

		Gdk.Rectangle area = args.Event.Area;

		//sometimes this is called when paint is finished
		//don't let this erase win
		if(force_capture_pixmap != null) {
			args.Event.Window.DrawDrawable(force_capture_drawingarea.Style.WhiteGC, force_capture_pixmap,
				area.X, area.Y,
				area.X, area.Y,
				area.Width, area.Height);
		}

		force_capture_allocationXOld = allocation.Width;
	}

	//this is called when user clicks on load signal
	//very based on: on_encoder_load_signal_clicked () future have some inheritance
	private void force_sensor_load ()
	{
		List<ForceSensor> data = SqliteForceSensor.Select(false, -1, currentPerson.UniqueID, currentSession.UniqueID);

		ArrayList dataPrint = new ArrayList();
		int count = 1;
		foreach(ForceSensor fs in data)
			dataPrint.Add(fs.ToStringArray(count++));

		string [] columnsString = {
			Catalog.GetString("ID"),
			Catalog.GetString("Set"),
			Catalog.GetString("Exercise"),
			Catalog.GetString("Elastic") + " (N/m)",
			Catalog.GetString("Capture option"),
			Catalog.GetString("Laterality"),
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
		genericWin.SetComboLabel(Catalog.GetString("Change the owner of selected set") +
				" (" + Catalog.GetString("code") + ":" + Catalog.GetString("name") + ")");
		genericWin.ShowEditRow(false);

		//select row corresponding to current signal
		genericWin.SelectRowWithID(0, currentForceSensor.UniqueID); //colNum, id

		genericWin.VideoColumn = 7;
		genericWin.CommentColumn = 8;

		genericWin.ShowButtonCancel(true);
		genericWin.SetButtonAcceptLabel(Catalog.GetString("Load"));
		genericWin.SetButtonCancelLabel(Catalog.GetString("Close"));
		genericWin.SetButtonAcceptSensitive(false);
		genericWin.Button_accept.Clicked += new EventHandler(on_force_sensor_load_signal_accepted);
		genericWin.Button_row_play.Clicked += new EventHandler(on_force_sensor_load_signal_row_play);
		genericWin.Button_row_edit.Clicked += new EventHandler(on_force_sensor_load_signal_row_edit);
		genericWin.Button_row_edit_apply.Clicked += new EventHandler(on_force_sensor_load_signal_row_edit_apply);
		genericWin.Button_row_delete.Clicked += new EventHandler(on_force_sensor_load_signal_row_delete_prequestion);

		genericWin.ShowNow();
	}

	private void on_force_sensor_load_signal_accepted (object o, EventArgs args)
	{
		LogB.Information("on force sensor load signal accepted");
		genericWin.Button_accept.Clicked -= new EventHandler(on_force_sensor_load_signal_accepted);

		int uniqueID = genericWin.TreeviewSelectedRowID();

		genericWin.HideAndNull();

		ForceSensor fs = (ForceSensor) SqliteForceSensor.Select(false, uniqueID, currentPerson.UniqueID, currentSession.UniqueID)[0];
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

		currentForceSensor = fs;
		lastForceSensorFile = Util.RemoveExtension(fs.Filename);
		lastForceSensorFullPath = fs.FullURL;

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
			button_force_sensor_stiffness.Visible = true;

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
			button_force_sensor_stiffness.Visible = false;
		}

		forceSensorCopyTempAndDoGraphs(forceSensorGraphsEnum.SIGNAL);
		image_force_sensor_graph.Sensitive = false; //unsensitivize the RFD image (can contain info of previous data)

		button_video_play_this_test_contacts.Sensitive = (fs.VideoURL != "");
		sensitiveLastTestButtons(true);

		//if drawingarea has still not shown, don't paint graph because GC screen is not defined
		if(force_sensor_ai_drawingareaShown)
		{
			forceSensorZoomDefaultValues();
			forceSensorDoGraphAI();
		}
		//event_execute_label_message.Text = "Loaded: " + Util.GetLastPartOfPath(filechooser.Filename);
		button_contacts_recalculate.Sensitive = true;
		force_capture_drawingarea.Sensitive = true;
		notebook_force_sensor_analyze.Sensitive = true;
		button_force_sensor_analyze_options.Sensitive = true;
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
		ForceSensor fs = (ForceSensor) SqliteForceSensor.Select(true, setID, -1, -1)[0];

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
		string idName = genericWin.GetComboSelected;
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
			ForceSensor fs = (ForceSensor) SqliteForceSensor.Select(false, setID, -1, -1)[0];
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
		forceSensorDeleteTestDo(currentForceSensor);

		//empty currentForceSensor (assign -1)
		currentForceSensor = new ForceSensor();

		//empty forceSensor GUI
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
			image_force_sensor_graph.Sensitive = false; //unsensitivize the RFD image (can contain info of previous data)
		}

		//if drawingarea has still not shown, don't paint graph because GC screen is not defined
		if(force_sensor_ai_drawingareaShown)
		{
			forceSensorZoomDefaultValues();
			forceSensorDoGraphAI();
		}

		currentForceSensor.UpdateSQL(false);
	}

	private void on_button_force_sensor_analyze_analyze_clicked (object o, EventArgs args)
	{
		if(! Util.FileExists(lastForceSensorFullPath))
		{
			new DialogMessage(Constants.MessageTypes.WARNING, Constants.FileNotFoundStr());
			return;
		}

		if(lastForceSensorFullPath != null && lastForceSensorFullPath != "")
			forceSensorCopyTempAndDoGraphs(forceSensorGraphsEnum.RFD);
	}

	private enum forceSensorGraphsEnum { SIGNAL, RFD }
	private void forceSensorCopyTempAndDoGraphs(forceSensorGraphsEnum fsge)
	{
		File.Copy(lastForceSensorFullPath, UtilEncoder.GetmifCSVFileName(), true); //can be overwritten

		if(fsge == forceSensorGraphsEnum.SIGNAL)
			forceSensorDoSignalGraph();
		else //(fsge == forceSensorGraphsEnum.RFD)
			forceSensorDoRFDGraph();
	}

	void forceSensorDoRFDGraph()
	{
		string imagePath = UtilEncoder.GetmifTempFileName();
		Util.FileDelete(imagePath);
		image_force_sensor_graph.Sensitive = false;

		int duration = -1;
		if(radio_force_duration_seconds.Active)
			duration = Convert.ToInt32(spin_force_duration_seconds.Value);

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

		ForceSensorGraph fsg = new ForceSensorGraph(getForceSensorCaptureOptions(), rfdList, impulse,
				duration, Convert.ToInt32(spin_force_rfd_duration_percent.Value),
				title, exercise, currentForceSensor.DateTimePublic);

		int imageWidth = UtilGtk.WidgetWidth(viewport_force_sensor_graph);
		int imageHeight = UtilGtk.WidgetHeight(viewport_force_sensor_graph);
		if(imageWidth < 300)
			imageWidth = 300; //Not crash R with a png height of -1 or "figure margins too large"
		if(imageHeight < 300)
			imageHeight = 300; //Not crash R with a png height of -1 or "figure margins too large"

		bool success = fsg.CallR(imageWidth -5, imageHeight -5);

		if(! success)
		{
			label_force_sensor_analyze.Text = Catalog.GetString("Error doing graph.") + " " +
				Catalog.GetString("Probably not sustained force.");
			label_force_sensor_analyze.Visible = true;

			return;
		}
		label_force_sensor_analyze.Visible = false;
		label_force_sensor_analyze.Text = "";

		while ( ! Util.FileReadable(imagePath));

		image_force_sensor_graph = UtilGtk.OpenImageSafe(
				imagePath,
				image_force_sensor_graph);
		image_force_sensor_graph.Sensitive = true;
		button_force_sensor_image_save_rfd_auto.Sensitive = true;
	}

	void forceSensorDoSignalGraph()
	{
		forceSensorDoSignalGraphReadFile(getForceSensorCaptureOptions());
		forceSensorDoSignalGraphPlot();
	}
	void forceSensorDoSignalGraphReadFile(ForceSensor.CaptureOptions fsco)
	{
		fscPoints = new ForceSensorCapturePoints(
				ForceSensorCapturePoints.GraphTypes.FORCESIGNAL,
				force_capture_drawingarea.Allocation.Width,
				force_capture_drawingarea.Allocation.Height,
				preferences.forceSensorCaptureWidthSeconds
				);

		List<string> contents = Util.ReadFileAsStringList(UtilEncoder.GetmifCSVFileName());
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

				/*
				 * TODO: Make this work with decimals as comma and decimals as point
				 * to fix problems on importing data on different localised computer
				 */

				if(Util.IsNumber(strFull[0], false) && Util.IsNumber(strFull[1], true))
				{
					times.Add(Convert.ToInt32(strFull[0]));
					forces.Add(Convert.ToDouble(strFull[1]));
				}
			}
		}
		ForceSensorDynamics fsd;

		if(currentForceSensorExercise.ComputeAsElastic)
			fsd = new ForceSensorDynamicsElastic(
					times, forces, fsco, currentForceSensorExercise, currentPersonSession.Weight, currentForceSensor.Stiffness,
					preferences.forceSensorElasticEccMinDispl, preferences.forceSensorElasticConMinDispl, false);
		else
			fsd = new ForceSensorDynamicsNotElastic(
					times, forces, fsco, currentForceSensorExercise, currentPersonSession.Weight, currentForceSensor.Stiffness,
					preferences.forceSensorNotElasticEccMinForce, preferences.forceSensorNotElasticConMinForce);

		forces = fsd.GetForces();
		times.RemoveAt(0); //always (not-elastic and elastic) 1st has to be removed, because time is not ok there.
		if(fsd.CalculedElasticPSAP)
			times = times.GetRange(fsd.RemoveNValues +1, times.Count -2*fsd.RemoveNValues);
		int i = 0;
		foreach(int time in times)
		{
			fscPoints.Add(time, forces[i]);
			fscPoints.NumCaptured ++;

			forceSensorValues.TimeLast = time;
			forceSensorValues.ValueLast = forces[i];
			forceSensorValues.SetMaxMinIfNeeded(forces[i], time);

			i ++;
		}
	}
	void forceSensorDoSignalGraphPlot()
	{
		UtilGtk.ErasePaint(force_capture_drawingarea, force_capture_pixmap);

		if(pen_black_force_capture == null)
			force_graphs_init();

		/*
		 * redo the graph if last point time is greater than RealWidthG
		 * or if GetForceInPx(minForce) < 0
		 * or if getForceInPx(maxForce) > heightG
		 */

		setForceSensorTopAtOperationStart();
		if(fscPoints.OutsideGraphChangeValues(forceSensorValues.TimeLast,
					getForceSensorMaxForceIncludingRectangle(forceSensorValues.Max),
					forceSensorValues.Min, false))
			fscPoints.Redo();

		forcePaintHVLines(ForceSensorGraphs.CAPTURE,
				getForceSensorMaxForceIncludingRectangle(forceSensorValues.Max),
				forceSensorValues.Min, forceSensorValues.TimeLast,
				false);

		//draw horizontal rectangle of feedback
		if(preferences.forceSensorCaptureFeedbackActive)
			forceSensorSignalPlotFeedbackRectangle(fscPoints, force_capture_drawingarea.Allocation.Width,
					force_capture_drawingarea, force_capture_pixmap, pen_blue_light_force_capture);


		Gdk.Point [] paintPoints = new Gdk.Point[fscPoints.Points.Count];
		for(int i = 0; i < fscPoints.Points.Count; i ++)
			paintPoints[i] = fscPoints.Points[i];

		force_capture_pixmap.DrawLines(pen_black_force_capture, paintPoints);

		//draw rectangle in maxForce
		//force_capture_pixmap.DrawRectangle(pen_red_force_capture, false,
		//		new Gdk.Rectangle(fscPoints.GetTimeInPx(maxForceTime) -5, fscPoints.GetForceInPx(maxForce) -5, 10, 10));

		//draw circle in maxForce
		force_capture_pixmap.DrawArc(pen_red_force_capture, false,
				fscPoints.GetTimeInPx(forceSensorValues.TimeValueMax) -6,
				fscPoints.GetForceInPx(forceSensorValues.Max) -6,
				12, 12, 90 * 64, 360 * 64);

		force_capture_drawingarea.QueueDraw(); // -- refresh

		label_force_sensor_value.Text = string.Format("{0:0.##} N", forceSensorValues.ValueLast);
		label_force_sensor_value_max.Text = string.Format("{0:0.##} N", forceSensorValues.Max);
		label_force_sensor_value_min.Text = string.Format("{0:0.##} N", forceSensorValues.Min);
		button_force_sensor_image_save_signal.Sensitive = true;
		button_force_sensor_analyze_analyze.Sensitive = true;
	}

	private void setForceSensorTopAtOperationStart()
	{
		if(preferences.forceSensorCaptureFeedbackActive)
			forceSensorTopRectangleAtOperationStart = Convert.ToInt32(preferences.forceSensorCaptureFeedbackAt + preferences.forceSensorCaptureFeedbackRange /2);
		else
			forceSensorTopRectangleAtOperationStart = 0;
	}
	//This function calculates the max value between the sent force and the top of the feedback rectangle
	private int getForceSensorMaxForceIncludingRectangle(double forceValue)
	{
		if(forceValue > forceSensorTopRectangleAtOperationStart)
			return Convert.ToInt32(forceValue);
		else
			return forceSensorTopRectangleAtOperationStart;
	}

	//at analyze can show lines AB and rectangle. There the rectangle should be YELLOW_LIGHT
	private void forceSensorSignalPlotFeedbackRectangle(ForceSensorCapturePoints points, int maxX,
			Gtk.DrawingArea drawingarea, Gdk.Pixmap pixmap, Gdk.GC pen_rectangle)
	{
		//draw horizontal rectangle of feedback
		int fbkNValue = preferences.forceSensorCaptureFeedbackAt; //feedback Newtons value
		int fbkNRange = preferences.forceSensorCaptureFeedbackRange; //feedback Newtons range (height of the rectangle)

		int fbkGraphCenter = points.GetForceInPx(fbkNValue);
		int fbkGraphRectHeight = points.GetForceInPx(0) - points.GetForceInPx(fbkNRange);
		int fbkGraphRectHalfHeight = Convert.ToInt32( fbkGraphRectHeight /2);
		int fbkGraphTop = points.GetForceInPx(fbkNValue) - fbkGraphRectHalfHeight;

		Rectangle rect = new Rectangle(points.GetTimeInPx(0) +1, fbkGraphTop,
				maxX - points.GetTimeInPx(0) -1, fbkGraphRectHeight);

		pixmap.DrawRectangle(pen_rectangle, true, rect);

		pixmap.DrawLine(pen_blue_dark_force_capture,
				points.GetTimeInPx(0) +1, fbkGraphCenter, maxX -1, fbkGraphCenter);
	}

	private enum ForceSensorGraphs { CAPTURE, ANALYSIS_GENERAL }

	//at scroll we use outsideScreen but we need to have some values defined
	private void forcePaintHVLines(ForceSensorGraphs fsg, double maxForce, double minForce, int lastTime, bool scroll)
	{
		//1 horizontal lines

		forcePaintHLine(fsg, 0, true);

		double absoluteMaxForce = maxForce;
		if(Math.Abs(minForce) > absoluteMaxForce)
			absoluteMaxForce = Math.Abs(minForce);

		//show 5 steps positive, 5 negative (if possible)
		int temp = Convert.ToInt32(UtilAll.DivideSafe(absoluteMaxForce, 5.0));
		int step = temp;

		//to have values multiples than 10, 100 ...
		if(step <= 10)
			step = temp;
		else if(step <= 100)
			step = temp - (temp % 10);
		else if(step <= 1000)
			step = temp - (temp % 100);
		else if(step <= 10000)
			step = temp - (temp % 1000);
		else //if(step <= 100000)
			step = temp - (temp % 10000);

		//fix crash when no force
		if(step == 0)
			step = 1;

		for(int i = step; i <= absoluteMaxForce ; i += step)
		{
			if(maxForce >= i || ForceSensorCapturePoints.DefaultRealHeightG >= i)
			{
				forcePaintHLine(fsg, i, false);
			}
			if(minForce <= (i * -1) || (ForceSensorCapturePoints.DefaultRealHeightGNeg * -1) <= (i * -1))
			{
				forcePaintHLine(fsg, i *-1, false);
			}
		}

		if(scroll)
		{
			forcePaintHLine(fsg, Convert.ToInt32(maxForce), true);
			forcePaintHLine(fsg, Convert.ToInt32(minForce), true);
		}

		//2 vertical lines

		if(scroll)
			forcePaintTimeValue(fsg, 0, false, true); //only paint de 0 vertical line, but do not show label, and paint solid
		else {
			int lastTimeInSeconds = lastTime / 1000000; //from microseconds to seconds
			step = 1;
			if(lastTimeInSeconds > 10)
				step = 5;
			if(lastTimeInSeconds > 50)
				step = 10;
			if(lastTimeInSeconds > 100)
				step = 20;
			if(lastTimeInSeconds > 200)
				step = 40;
			if(lastTimeInSeconds > 400)
				step = 80;

			for(int i = 0; i <= lastTimeInSeconds ; i += step)
				forcePaintTimeValue(fsg, i, true, i == 0);
		}
	}

	private void forcePaintTimeValue(ForceSensorGraphs fsg, int time, bool showValue, bool solid)
	{
		if(fsg == ForceSensorGraphs.CAPTURE)
			forcePaintCaptureTimeValue(time, showValue, solid);
		else if(fsg == ForceSensorGraphs.ANALYSIS_GENERAL)
			forcePaintAnalyzeGeneralTimeValue(time, solid);
	}
	private void forcePaintCaptureTimeValue(int time, bool showValue, bool solid)
	{
		int xPx = fscPoints.GetTimeInPx(1000000 * time);
		int textWidth = 1;
		int textHeight = 1;

		if(showValue)
		{
			layout_force_text.SetMarkup(time.ToString() + "s");
			layout_force_text.GetPixelSize(out textWidth, out textHeight);
			force_capture_pixmap.DrawLayout (pen_gray_force_capture,
					xPx - textWidth/2, force_capture_drawingarea.Allocation.Height - textHeight, layout_force_text);
		}


		//draw vertical line
		if(solid)
		{
			layout_force_text.SetMarkup("Force (N)");
			layout_force_text.GetPixelSize(out textWidth, out textHeight);
			force_capture_pixmap.DrawLayout (pen_gray_force_capture,
					fscPoints.GetTimeInPx(0) - textWidth/2, 0, layout_force_text);

			force_capture_pixmap.DrawLine(pen_gray_force_capture,
					xPx, textHeight, xPx, force_capture_drawingarea.Allocation.Height - textHeight -6);

		} else
			force_capture_pixmap.DrawLine(pen_gray_force_capture_discont,
					xPx, 4, xPx, force_capture_drawingarea.Allocation.Height - textHeight -6);
	}

	private void forcePaintHLine(ForceSensorGraphs fsg, int yForce, bool solid)
	{
		if(fsg == ForceSensorGraphs.CAPTURE)
			forcePaintCaptureHLine(yForce, solid);
		else if(fsg == ForceSensorGraphs.ANALYSIS_GENERAL)
			forcePaintAnalyzeGeneralHLine(yForce, solid);
	}
	private void forcePaintCaptureHLine(int yForce, bool solid)
	{
		int yPx = fscPoints.GetForceInPx(yForce);

		//1) draw horizontal line
		if(solid)
			force_capture_pixmap.DrawLine(pen_gray_force_capture,
					fscPoints.GetTimeInPx(0), yPx, force_capture_drawingarea.Allocation.Width, yPx);
		else
			force_capture_pixmap.DrawLine(pen_gray_force_capture_discont,
					fscPoints.GetTimeInPx(0), yPx, force_capture_drawingarea.Allocation.Width, yPx);

		//2) write force label
		layout_force_text.SetMarkup(yForce.ToString());
		int textWidth = 1;
		int textHeight = 1;
		layout_force_text.GetPixelSize(out textWidth, out textHeight);

		//max, 0, min will be in black, the rest in gray
		if(solid)
			force_capture_pixmap.DrawLayout (pen_black_force_capture,
					fscPoints.GetTimeInPx(0) - textWidth -6, yPx - textHeight/2, layout_force_text);
		else
			force_capture_pixmap.DrawLayout (pen_gray_force_capture,
					fscPoints.GetTimeInPx(0) - textWidth -6, yPx - textHeight/2, layout_force_text);
	}

	private void on_radio_force_rfd_duration_toggled (object o, EventArgs args)
	{
		spin_force_duration_seconds.Sensitive = radio_force_duration_seconds.Active;
		hbox_force_rfd_duration_percent.Sensitive = radio_force_rfd_duration_percent.Active;
	}

	private void on_button_force_sensor_image_save_signal_clicked (object o, EventArgs args)
	{
		checkFile(Constants.CheckFileOp.FORCESENSOR_SAVE_IMAGE_SIGNAL);
	}
	private void on_button_force_sensor_image_save_rfd_auto_clicked (object o, EventArgs args)
	{
		checkFile(Constants.CheckFileOp.FORCESENSOR_SAVE_IMAGE_RFD_AUTO);
	}
	private void on_button_force_sensor_image_save_rfd_manual_clicked (object o, EventArgs args)
	{
		checkFile(Constants.CheckFileOp.FORCESENSOR_SAVE_IMAGE_RFD_MANUAL);
	}

	void on_button_forcesensor_save_image_signal_file_selected (string destination)
	{
		LogB.Information("CREATING PIXBUF");
		LogB.Information("force_capture_pixmap is null == " + (force_capture_pixmap == null));
		LogB.Information("colormapForce is null == " + (colormapForce == null));
		LogB.Information("force_capture_drawingarea is null == " + (force_capture_drawingarea == null));
		int pixmapW = 0;
		int pixmapH = 0;
		force_capture_pixmap.GetSize(out pixmapW, out pixmapH);
		Gdk.Pixbuf pixbuf = Pixbuf.FromDrawable(force_capture_pixmap, colormapForce,
				0, 0, 0, 0, pixmapW, pixmapH);

		LogB.Information("Saving");
		pixbuf.Save(destination,"png");
	}
	private void on_overwrite_file_forcesensor_save_image_signal_accepted(object o, EventArgs args)
	{
		on_button_forcesensor_save_image_signal_file_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), exportFileName);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}

	void on_button_forcesensor_save_image_rfd_auto_file_selected (string destination)
	{
		File.Copy(UtilEncoder.GetmifTempFileName(), destination, true);
	}
	private void on_overwrite_file_forcesensor_save_image_rfd_auto_accepted(object o, EventArgs args)
	{
		on_button_forcesensor_save_image_rfd_auto_file_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), exportFileName);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}

	void on_button_forcesensor_save_image_rfd_manual_file_selected (string destination)
	{
		LogB.Information("CREATING PIXBUF");
		LogB.Information("force_sensor_ai_pixmap is null == " + (force_sensor_ai_pixmap == null));
		LogB.Information("colormapForceAI is null == " + (colormapForceAI == null));
		LogB.Information("force_sensor_ai_drawingarea is null == " + (force_sensor_ai_drawingarea == null));
		int pixmapW = 0;
		int pixmapH = 0;
		force_sensor_ai_pixmap.GetSize(out pixmapW, out pixmapH);
		Gdk.Pixbuf pixbuf = Pixbuf.FromDrawable(force_sensor_ai_pixmap, colormapForceAI,
				0, 0, 0, 0, pixmapW, pixmapH);

		LogB.Information("Saving");
		pixbuf.Save(destination,"png");
	}
	private void on_overwrite_file_forcesensor_save_image_rfd_manual_accepted(object o, EventArgs args)
	{
		on_button_forcesensor_save_image_rfd_manual_file_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), exportFileName);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}


	private void on_menuitem_force_sensor_open_folder_activate (object o, EventArgs args)
	{
		if(currentSession == null || currentSession.UniqueID == -1)
		{
			try {
				System.Diagnostics.Process.Start(ForceSensorGraph.GetDataDir(-1));
			} catch {
				new DialogMessage(Constants.MessageTypes.WARNING,
						Catalog.GetString("Error. Cannot open directory.") + "\n\n" + ForceSensorGraph.GetDataDir(-1));
			}
			return;
		}

		string dataDir = ForceSensorGraph.GetDataDir(currentSession.UniqueID);
		if(dataDir != "")
		{
			try {
				System.Diagnostics.Process.Start(dataDir);
			} catch {
				new DialogMessage(Constants.MessageTypes.WARNING,
						Catalog.GetString("Error. Cannot open directory.") + "\n\n" + dataDir);
			}
		}
		else
			new DialogMessage(Constants.MessageTypes.WARNING, Constants.DirectoryCannotOpenStr());
	}

	void on_menuitem_forceSensor_session_overview_activate (object o, EventArgs args)
	{
		ForceSensorOverviewWindow.Show (app1, currentSession.UniqueID);
	}

	private void on_button_force_sensor_adjust_clicked (object o, EventArgs args)
	{
		button_force_sensor_adjust.Sensitive = false; //to not be called again

		//hbox_force_capture_buttons.Sensitive = false;
		notebook_contacts_execute_or_instructions.Sensitive = false;

		viewport_chronopics.Sensitive = false;
		notebook_contacts_capture_doing_wait.CurrentPage = 3;
		button_contacts_exercise.Sensitive = false;

		forceSensorCaptureAdjustSensitivity(false);
		event_execute_label_message.Text = Catalog.GetString("If you want to calibrate, please tare first.");
	}
	private void on_button_force_sensor_adjust_close_clicked (object o, EventArgs args)
	{
		button_force_sensor_adjust.Sensitive = true;

		//hbox_force_capture_buttons.Sensitive = true;
		notebook_contacts_execute_or_instructions.Sensitive = true;

		viewport_chronopics.Sensitive = true;
		notebook_contacts_capture_doing_wait.CurrentPage = 0;
		button_contacts_exercise.Sensitive = true;

		forceSensorCaptureAdjustSensitivity(true);
		event_execute_label_message.Text = "";
	}

	private void forceSensorCaptureAdjustSensitivity(bool s) //s for sensitive. When adjusting s = false
	{
		hbox_force_capture_buttons.Sensitive = s;

		button_activate_chronopics.Sensitive = s;
		image_test.Sensitive = s;
		button_execute_test.Sensitive = s;
		button_force_sensor_image_save_signal.Sensitive = s;

		main_menu.Sensitive = s;
		viewport_persons.Sensitive = s;
		hbox_contacts_sup_capture_analyze_two_buttons.Sensitive = s;
		hbox_top_person.Sensitive = s;
	}

	private void on_button_force_sensor_adjust_help_clicked (object o, EventArgs args)
	{
		new DialogMessage("Force sensor adjust data", Constants.MessageTypes.INFO,
				preferences.GetForceSensorAdjustString());
	}

	private void createComboForceSensorCaptureOptions()
	{
		UtilGtk.ComboUpdate(combo_force_sensor_capture_options, ForceSensor.CaptureOptionsList());
		combo_force_sensor_capture_options.Active = 0;
	}

	// -------------------------------- exercise stuff --------------------


	string [] forceSensorComboExercisesString; //id:name (no translations, use user language)

	private void createForceExerciseCombo ()
	{
		//force_sensor_exercise

		combo_force_sensor_exercise = ComboBox.NewText ();
		fillForceSensorExerciseCombo("");

		combo_force_sensor_exercise.Changed += new EventHandler (on_combo_force_sensor_exercise_changed);
		hbox_combo_force_sensor_exercise.PackStart(combo_force_sensor_exercise, true, true, 0);
		hbox_combo_force_sensor_exercise.ShowAll();

		//needed because the += EventHandler does not work on first fill of the combo
		on_combo_force_sensor_exercise_changed (new object (), new EventArgs ());

		combo_force_sensor_button_sensitive_exercise(UtilGtk.ComboGetActive(combo_force_sensor_exercise) != "");
	}

	private void on_combo_force_sensor_exercise_changed (object o, EventArgs args)
	{
		ArrayList array = SqliteForceSensorExercise.Select (
                                false, getExerciseIDFromAnyCombo(
					combo_force_sensor_exercise, forceSensorComboExercisesString, false), false );
		if(array.Count == 0)
		{
			label_button_force_sensor_stiffness.Text = "0";
			image_button_force_sensor_stiffness_problem.Visible = true;
			button_force_sensor_stiffness.Visible = false;
			combo_force_sensor_button_sensitive_exercise(false);
			return;
		}

		ForceSensorExercise fse = (ForceSensorExercise) array[0];
		if(fse.ComputeAsElastic)
		{
			List<ForceSensorElasticBand> list_fseb = SqliteForceSensorElasticBand.SelectAll(false, true); //not opened, onlyActive
			double stiffness = ForceSensorElasticBand.GetStiffnessOfActiveBands(list_fseb);

			setStiffnessButtonLabel(stiffness);
			button_force_sensor_stiffness.Visible = true;
		} else {
			label_button_force_sensor_stiffness.Text = "0";
			button_force_sensor_stiffness.Visible = false;
			image_button_force_sensor_stiffness_problem.Visible = false;
		}
		setLabelContactsExerciseSelected(Catalog.GetString(fse.Name));
		combo_force_sensor_button_sensitive_exercise(true);
	}

	private void fillForceSensorExerciseCombo(string name)
	{
		ArrayList forceSensorExercises = SqliteForceSensorExercise.Select (false, -1, false);
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
	}

	private void combo_force_sensor_button_sensitive_exercise(bool exerciseSelected)
	{
		button_force_sensor_exercise_edit.Sensitive = exerciseSelected;
		button_force_sensor_exercise_delete.Sensitive = exerciseSelected;
	}

	void on_button_force_sensor_exercise_edit_clicked (object o, EventArgs args)
	{
		if(UtilGtk.ComboGetActive(combo_force_sensor_exercise) == "")
		{
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Need to create/select an exercise."));
			return;
		}

		ForceSensorExercise ex = (ForceSensorExercise) SqliteForceSensorExercise.Select (
                                false, getExerciseIDFromAnyCombo(combo_force_sensor_exercise, forceSensorComboExercisesString, false), false)[0];

		LogB.Information("selected exercise: " + ex.ToString());

		forceSensorExerciseWin = ForceSensorExerciseWindow.ShowEdit (Catalog.GetString("Exercise"),
				Catalog.GetString("Force sensor exercise:"), ex,
				preferences.forceSensorElasticEccMinDispl, preferences.forceSensorElasticConMinDispl,
				preferences.forceSensorNotElasticEccMinForce, preferences.forceSensorNotElasticConMinForce);

		forceSensorExerciseWin.FakeButtonReadValues.Clicked += new EventHandler(on_button_force_sensor_exercise_edit_add_accepted);
	}

	void on_button_force_sensor_exercise_add_clicked (object o, EventArgs args)
	{
		forceSensorExerciseWin = ForceSensorExerciseWindow.ShowAdd (Catalog.GetString("Exercise"),
				Catalog.GetString("Force sensor exercise:"),
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

		forceSensorDoGraphAI();
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
                                false, getExerciseIDFromAnyCombo(combo_force_sensor_exercise, forceSensorComboExercisesString, false), false)[0];

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
		if(s == Catalog.GetString(Constants.ForceSensorLateralityBoth))
			radio_force_sensor_laterality_both.Active = true;
		else if(s == Catalog.GetString(Constants.ForceSensorLateralityLeft))
			radio_force_sensor_laterality_l.Active = true;
		else if(s == Catalog.GetString(Constants.ForceSensorLateralityRight))
			radio_force_sensor_laterality_r.Active = true;
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

}
