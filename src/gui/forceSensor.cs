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
 * Copyright (C) 2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.IO;
using System.IO.Ports;
using System.Threading;
using Gtk;
using Gdk;
using Glade;
using System.Text; //StringBuilder
using System.Collections.Generic; //List<T>
using Mono.Unix;

//struct with relevant data used on various functions and threads
public class ForceSensorValues
{
	public int TimeLast; //store last time
	public int TimeForceMax; //store time of max force
	public double ForceLast; //store last force
	public double ForceMax; //store max force
	public double ForceMin; //store min force

	public ForceSensorValues()
	{
		TimeLast = 0;
		TimeForceMax = 0;
		ForceLast = 0;
		ForceMax = 0;
		ForceMin = 10000;
	}

	public void SetMaxMinIfNeeded(double force, int time)
	{
		if(force > ForceMax)
		{
			ForceMax = force;
			TimeForceMax = time;
		}
		if(force < ForceMin)
			ForceMin = force;
	}
}

public partial class ChronoJumpWindow 
{
	[Widget] Gtk.HBox hbox_force_buttons;
	[Widget] Gtk.Button button_force_sensor_tare;
	[Widget] Gtk.Button button_force_sensor_calibrate;
	[Widget] Gtk.Label label_force_sensor_value_max;
	[Widget] Gtk.Label label_force_sensor_value;
	[Widget] Gtk.Label label_force_sensor_value_min;
	//[Widget] Gtk.VScale vscale_force_sensor;
	[Widget] Gtk.Viewport viewport_force_sensor_graph;
	[Widget] Gtk.Image image_force_sensor_graph;
	[Widget] Gtk.SpinButton spin_force_sensor_calibration_kg_value;
	[Widget] Gtk.Button button_force_sensor_check_version;
	[Widget] Gtk.Button button_force_sensor_image_save_signal;
	[Widget] Gtk.Button button_force_sensor_image_save_rfd;
	[Widget] Gtk.DrawingArea force_capture_drawingarea;
	Gdk.Pixmap force_capture_pixmap = null;
	
	Thread forceCaptureThread;
	static bool forceProcessFinish;
	static bool forceProcessCancel;
	ForceSensorCapturePoints fscPoints;

	Thread forceOtherThread; //for messages on: capture, tare, calibrate
	static string forceSensorOtherMessage = "";
	static bool forceSensorOtherMessageShowSeconds;
	static DateTime forceSensorTimeStart;
	static string lastForceSensorFile = "";

	/*
	 * forceStatus:
	 * STOP is when is not used
	 * STARTING is while is waiting forceSensor to start capturing
	 * CAPTURING is when data is arriving
	 * COPIED_TO_TMP means data is on tmp and graph can be called
	 */
	enum forceStatus { STOP, STARTING, CAPTURING, COPIED_TO_TMP }
	static forceStatus capturingForce = forceStatus.STOP;
	static bool redoingPoints; //don't draw while redoing points (adjusting screen)

	static bool forceCaptureStartMark; 	//Just needed to display "Capturing message"
	static ForceSensorValues forceSensorValues;

	string forceSensorPortName;
	SerialPort portFS; //Attention!! Don't reopen port because arduino makes reset and tare, calibration... is lost
	bool portFSOpened;

	Gdk.GC pen_black_force_capture;
	Gdk.GC pen_red_force_capture;
	Gdk.GC pen_gray_force_capture;
	Pango.Layout layout_force_text;
	Gdk.Colormap colormapForce = Gdk.Colormap.System;


	private void force_graphs_init()
	{
		colormapForce = Gdk.Colormap.System;
		colormapForce.AllocColor (ref UtilGtk.BLACK,true,true);
		colormapForce.AllocColor (ref UtilGtk.GRAY,true,true);
		colormapForce.AllocColor (ref UtilGtk.RED_PLOTS,true,true);

		pen_black_force_capture = new Gdk.GC(force_capture_drawingarea.GdkWindow);
		pen_black_force_capture.Foreground = UtilGtk.BLACK;
		//pen_black_force_capture.SetLineAttributes (2, Gdk.LineStyle.Solid, Gdk.CapStyle.NotLast, Gdk.JoinStyle.Miter);
		//this makes the lines less spiky:
		pen_black_force_capture.SetLineAttributes (2, Gdk.LineStyle.Solid, Gdk.CapStyle.Round, Gdk.JoinStyle.Round);

		pen_red_force_capture = new Gdk.GC(force_capture_drawingarea.GdkWindow);
		pen_red_force_capture.Foreground = UtilGtk.RED_PLOTS;
		pen_red_force_capture.SetLineAttributes (2, Gdk.LineStyle.Solid, Gdk.CapStyle.NotLast, Gdk.JoinStyle.Miter);

		pen_gray_force_capture = new Gdk.GC(force_capture_drawingarea.GdkWindow);
		pen_gray_force_capture.Foreground = UtilGtk.GRAY;
		pen_gray_force_capture.SetLineAttributes (1, Gdk.LineStyle.OnOffDash, Gdk.CapStyle.NotLast, Gdk.JoinStyle.Miter);

		layout_force_text = new Pango.Layout (force_capture_drawingarea.PangoContext);
		layout_force_text.FontDescription = Pango.FontDescription.FromString ("Courier 10");
	}


	//Attention: no GTK here!!
	private bool forceSensorConnect()
	{
		LogB.Information(" FS connect 0 ");
		if(chronopicRegister.ConnectedOfType(ChronopicRegisterPort.Types.ARDUINO_FORCE) == null)
		{
			forceSensorOtherMessage = "Force sensor is not connected!";
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
		LogB.Information(" FS connect 4 ");

		try {
			portFS.Open();
		}
		catch (System.IO.IOException)
		{
			return false;
		}

		//set adjust parameters
		if(! forceSensorSendCommand("set_tare:" + preferences.forceSensorTare.ToString() + ";",
					"Connecting ...", "Catched adjusting tare"))
			return false;

		if(! forceSensorSendCommand("set_calibration_factor:" + preferences.forceSensorCalibrationFactor.ToString() + ";",
					"Connecting ...", "Catched adjusting calibration factor"))
			return false;

		portFSOpened = true;
		Thread.Sleep(2500); //sleep to let arduino start reading
		forceSensorOtherMessage = "Connected!";
		LogB.Information(" FS connect 5 ");
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
			portFS.WriteLine(command);
			forceSensorTimeStart = DateTime.Now;
		}
		catch (Exception ex)
		{
			if(ex is System.IO.IOException || ex is System.TimeoutException)
			{
				LogB.Information(errorMessage);
				portFSOpened = false;
				return false;
			}
			//throw;
		}
		return true;
	}

	enum forceSensorOtherModeEnum { TARE, CALIBRATE, CAPTURE_PRE, CHECK_VERSION }
	static forceSensorOtherModeEnum forceSensorOtherMode;

	//buttons: tare, calibrate, check version and capture (via on_button_execute_test_cicked) come here
	private void on_buttons_force_sensor_clicked(object o, EventArgs args)
	{
		if(chronopicRegister.NumConnectedOfType(ChronopicRegisterPort.Types.ARDUINO_FORCE) == 0)
		{
			new DialogMessage(Constants.MessageTypes.WARNING, "Sensor not found.");
			return;
		}

		capturingForce = forceStatus.STOP;
		forceSensorButtonsSensitive(false);
		forceSensorTimeStart = DateTime.Now;
		forceSensorOtherMessageShowSeconds = true;

		if(pen_black_force_capture == null)
			force_graphs_init();

		if(o == (object) button_force_sensor_tare)
		{
			forceSensorOtherMode = forceSensorOtherModeEnum.TARE;
			forceOtherThread = new Thread(new ThreadStart(forceSensorTare));
		}
		else if(o == (object) button_force_sensor_calibrate)
		{
			forceSensorOtherMode = forceSensorOtherModeEnum.CALIBRATE;
			forceOtherThread = new Thread(new ThreadStart(forceSensorCalibrate));
		}
		else if (o == (object) button_execute_test)
		{
			forceSensorOtherMode = forceSensorOtherModeEnum.CAPTURE_PRE;
			forceOtherThread = new Thread(new ThreadStart(forceSensorCapturePre));
		}
		else { //if (o == (object) button_check_version)
			forceSensorOtherMode = forceSensorOtherModeEnum.CHECK_VERSION;
			forceOtherThread = new Thread(new ThreadStart(forceSensorCheckVersion));
		}

		GLib.Idle.Add (new GLib.IdleHandler (pulseGTKForceSensorOther));

		LogB.ThreadStart();
		forceOtherThread.Start();
	}

	void forceSensorButtonsSensitive(bool sensitive)
	{
		hbox_force_buttons.Sensitive = sensitive;
		button_execute_test.Sensitive = sensitive;
	}

	private bool pulseGTKForceSensorOther ()
	{
		if(forceSensorOtherMessage != "")
		{
			string secondsStr = "";
			if(forceSensorOtherMessageShowSeconds)
			{
				TimeSpan ts = DateTime.Now.Subtract(forceSensorTimeStart);
				double seconds = ts.TotalSeconds;
				secondsStr = " (" + Util.TrimDecimals(seconds, 0) + " s)";

			}
			event_execute_label_message.Text = forceSensorOtherMessage + secondsStr;
		}

		if(! forceOtherThread.IsAlive)
		{
			LogB.ThreadEnding();

			if(
					forceSensorOtherMode == forceSensorOtherModeEnum.TARE ||
					forceSensorOtherMode == forceSensorOtherModeEnum.CALIBRATE ||
					forceSensorOtherMode == forceSensorOtherModeEnum.CHECK_VERSION)
				forceSensorButtonsSensitive(true);
			else //if(forceSensorOtherMode == forceSensorOtherModeEnum.CAPTURE_PRE)
				forceSensorCapturePre2();

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

		// 3 get tare factor
		if (portFS.BytesToRead > 0)
			LogB.Information("PRE_get_tare bytes: " + portFS.ReadExisting());

		if(! forceSensorSendCommand("get_tare:", "Checking ...", "Catched at get_tare"))
			return;

		// 4 update preferences and SQL with new tare
		str = Util.ChangeDecimalSeparator(portFS.ReadLine().Trim());
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
		if(! forceSensorSendCommand("calibrate:" + spin_force_sensor_calibration_kg_value.Value.ToString() + ";",
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

		// 3 get calibration factor
		if (portFS.BytesToRead > 0)
			LogB.Information("PRE_get_calibrationfactor bytes: " + portFS.ReadExisting());

		if(! forceSensorSendCommand("get_calibration_factor:", "Checking ...", "Catched at get_calibration_factor"))
			return;

		// 4 update preferences and SQL with new calibration factor
		str = Util.ChangeDecimalSeparator(portFS.ReadLine().Trim());
		if(Util.IsNumber(str, true))
			preferences.UpdateForceSensorCalibration(
					spin_force_sensor_calibration_kg_value.Value, Convert.ToDouble(str));

		// 5 print message
		forceSensorOtherMessageShowSeconds = false;
		forceSensorOtherMessage = "Calibrated!";
	}

	//Attention: no GTK here!!
	private void forceSensorCheckVersion()
	{
		if(! portFSOpened)
			if(! forceSensorConnect())
				return;

		if(! forceSensorSendCommand("get_version:", "Checking version ...", "Catched checking version"))
			return;

		string str = "";
		do {
			Thread.Sleep(100); //sleep to let arduino start reading
			try {
				str = portFS.ReadLine().Trim();
			} catch {
				forceSensorOtherMessage = "Disconnected";
				return;
			}
			LogB.Information("init string: " + str);
		}
		while(! str.Contains("Force_Sensor-"));

		forceSensorOtherMessageShowSeconds = false;
		forceSensorOtherMessage = str;
	}

	//Attention: no GTK here!!
	private void forceSensorCapturePre()
	{
		if(! portFSOpened)
			if(! forceSensorConnect())
				return;

		forceSensorOtherMessage = "Please, wait ...";
		capturingForce = forceStatus.STARTING;
	}

	private void forceSensorCapturePre2()
	{
		button_execute_test.Sensitive = false;
		event_execute_button_finish.Sensitive = true;
		event_execute_button_cancel.Sensitive = true;
		forceCaptureStartMark = false;
		//vscale_force_sensor.Value = 0;
		label_force_sensor_value_max.Text = "0";
		label_force_sensor_value.Text = "0";
		label_force_sensor_value_min.Text = "0";

		forceProcessFinish = false;
		forceProcessCancel = false;

		//initialize
		forceSensorValues = new ForceSensorValues();

		UtilGtk.ErasePaint(force_capture_drawingarea, force_capture_pixmap);
		fscPoints = new ForceSensorCapturePoints(
				force_capture_drawingarea.Allocation.Width,
				force_capture_drawingarea.Allocation.Height
				);
		forcePaintHVLines(fscPoints.RealHeightG, -1 * fscPoints.RealHeightG, 10);

		event_execute_ButtonFinish.Clicked -= new EventHandler(on_finish_clicked);
		event_execute_ButtonFinish.Clicked += new EventHandler(on_finish_clicked);
		
		event_execute_ButtonCancel.Clicked -= new EventHandler(on_cancel_clicked);
		event_execute_ButtonCancel.Clicked += new EventHandler(on_cancel_clicked);

		forceCaptureThread = new Thread(new ThreadStart(forceSensorCaptureDo));
		GLib.Idle.Add (new GLib.IdleHandler (pulseGTKForceSensorCapture));

		LogB.ThreadStart();
		forceCaptureThread.Start();
	}

	//non GTK on this method
	private void forceSensorCaptureDo()
	{
		lastChangedTime = 0;

		if(! forceSensorSendCommand("start_capture:", "", "Catched force capturing"))
		{
			forceProcessCancel = true;
			return;
		}

		string str = "";
		do {
			Thread.Sleep(100); //sleep to let arduino start reading
			try {
				str = portFS.ReadLine();
			} catch {
				forceProcessCancel = true;
				return;
			}
			LogB.Information("init string: " + str);
		}
		while(! str.Contains("Starting capture"));

		forceCaptureStartMark = true;
		capturingForce = forceStatus.CAPTURING;

		Util.CreateForceSensorSessionDirIfNeeded (currentSession.UniqueID);
		string fileName = Util.GetForceSensorSessionDir(currentSession.UniqueID) + Path.DirectorySeparatorChar +
			currentPerson.Name + "_" + UtilDate.ToFile(DateTime.Now) + ".csv";

		TextWriter writer = File.CreateText(fileName);
		writer.WriteLine("Time (s);Force(N)");
		str = "";
		int firstTime = 0;

		while(! forceProcessFinish && ! forceProcessCancel)
		{
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

			int time = Convert.ToInt32(strFull[0]);

			//measurement does not start at 0 time. When we start receiving data, mark this as firstTime
			if(firstTime == 0)
				firstTime = time;

			//use this to have time starting at 0
			time -= firstTime;

			double force = Convert.ToDouble(Util.ChangeDecimalSeparator(strFull[1]));
			writer.WriteLine(time.ToString() + ";" + force.ToString());

			forceSensorValues.TimeLast = time;
			forceSensorValues.ForceLast = force;

			forceSensorValues.SetMaxMinIfNeeded(force, time);

			fscPoints.Add(time, force);
			fscPoints.NumCaptured ++;
			if(fscPoints.OutsideGraph())
			{
				redoingPoints = true;
				fscPoints.Redo();
				fscPoints.NumPainted = -1;
				redoingPoints = false;
			}

			//changeSlideIfNeeded(time, force);
		}
		portFS.WriteLine("end_capture:");

		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();
		capturingForce = forceStatus.STOP;

		//port.Close();

		if(forceProcessCancel)
			Util.FileDelete(fileName);
		else {
			//call graph
			File.Copy(fileName, UtilEncoder.GetmifCSVFileName(), true); //can be overwritten
			capturingForce = forceStatus.COPIED_TO_TMP;
		}
	}

	private bool pulseGTKForceSensorCapture ()
	{
LogB.Information(" fc A ");
		if(forceCaptureThread == null)
		{
			Thread.Sleep (25);
			return true;
		}

LogB.Information(" fc B ");
		//LogB.Information(capturingForce.ToString())
		if(! forceCaptureThread.IsAlive || forceProcessFinish || forceProcessCancel)
		{
LogB.Information(" fc C ");
			LogB.ThreadEnding();

			if(forceProcessFinish)
			{
				if(capturingForce != forceStatus.COPIED_TO_TMP)
				{
					Thread.Sleep (25); //Wait file is copied
					return true;
				}
				else
				{
					event_execute_label_message.Text = "Saved.";
					Thread.Sleep (250); //Wait a bit to ensure is copied

					fscPoints.InitRealWidthHeight();
					forceSensorDoSignalGraphPlot();
					forceSensorDoRFDGraph();
				}
			} else if(forceProcessCancel)
			{
				event_execute_label_message.Text = "Cancelled.";
				button_force_sensor_image_save_signal.Sensitive = false;
				button_force_sensor_image_save_rfd.Sensitive = false;
			}
			else
				event_execute_label_message.Text = "";

LogB.Information(" fc D ");
			LogB.ThreadEnded(); 

			forceSensorButtonsSensitive(true);
			button_force_sensor_image_save_signal.Sensitive = true;

			//finish, cancel: sensitive = false
			hideButtons();

			return false;
		}

LogB.Information(" fc E ");
		if(forceCaptureStartMark)
		{
			event_execute_label_message.Text = "Capturing ...";
			forceCaptureStartMark = false;
		}
LogB.Information(" fc F ");

		if(capturingForce == forceStatus.CAPTURING)
		{
LogB.Information(" fc G ");
			//------------------- vscale -----------------
			/*
			//A) resize vscale if needed
			int upper = Convert.ToInt32(vscale_force_sensor.Adjustment.Upper);
			int lower = Convert.ToInt32(vscale_force_sensor.Adjustment.Lower);
			bool changed = false;

			if(forceSensorLastCaptured > upper)
			{
				upper = Convert.ToInt32(forceSensorLastCaptured * 2);
				changed = true;
			}
			if(forceSensorLastCaptured < lower)
			{
				lower = Convert.ToInt32(forceSensorLastCaptured * 2); //good for negative values
				changed = true;
			}
			if(changed)
				vscale_force_sensor.SetRange(lower, upper);

			//B) change the value
			vscale_force_sensor.Value = forceSensorLastCaptured;
			*/
			label_force_sensor_value.Text = forceSensorValues.ForceLast.ToString();
			label_force_sensor_value_max.Text = forceSensorValues.ForceMax.ToString();
			label_force_sensor_value_min.Text = forceSensorValues.ForceMin.ToString();


LogB.Information(" fc H ");
			//------------------- realtime graph -----------------
			if(redoingPoints || fscPoints == null || fscPoints.Points == null || force_capture_drawingarea == null)
				return true;

LogB.Information(" fc I ");
			//mark meaning screen should be erased
			if(fscPoints.NumPainted == -1) {
				UtilGtk.ErasePaint(force_capture_drawingarea, force_capture_pixmap);
				forcePaintHVLines(forceSensorValues.ForceMax, forceSensorValues.ForceMin, fscPoints.RealWidthG);
				fscPoints.NumPainted = 0;
			}

LogB.Information(" fc J ");
			//use these integers and this List to not have errors by updating data on the other thread
			int numCaptured = fscPoints.NumCaptured;
			int numPainted = fscPoints.NumPainted;
			List<Gdk.Point> points = fscPoints.Points;

LogB.Information(" fc K ");
			int toDraw = numCaptured - numPainted;

			LogB.Information("points count: " + points.Count +
					"; NumCaptured: " + numCaptured + "; NumPainted: " + numPainted +
					"; toDraw: " + toDraw.ToString() );

LogB.Information(" fc L ");
			//fixes crash at the end
			if(toDraw == 0)
				return true;

LogB.Information(" fc M ");
			Gdk.Point [] paintPoints;
			if(numPainted > 0)
				paintPoints = new Gdk.Point[toDraw +1]; // if something has been painted, connected first point with previous points
			else
				paintPoints = new Gdk.Point[toDraw];

LogB.Information(" fc N ");
			int jStart = 0;
			int iStart = 0;
			if(numPainted > 0)
			{
				// if something has been painted, connected first point with previous points
				paintPoints[0] = points[numPainted -1];
				jStart = 1;
				iStart = numPainted;
				//LogB.Information("X: " + paintPoints[0].X.ToString() + "; Y: " + paintPoints[0].Y.ToString());
			}
LogB.Information(" fc O ");
			for(int j=jStart, i = iStart ; i < numCaptured ; i ++, j++)
			{
				if(points.Count > i) 	//extra check to avoid going outside of arrays
					paintPoints[j] = points[i];
				//LogB.Information("X: " + paintPoints[j].X.ToString() + "; Y: " + paintPoints[j].Y.ToString());
			}
LogB.Information(" fc P ");
			force_capture_pixmap.DrawLines(pen_black_force_capture, paintPoints);
			force_capture_drawingarea.QueueDraw(); // -- refresh

			/*
			 * update fscPoints.NumPainted by only if there's no -1 mark
			 * because -1 mark should prevail until repainted the screen
			 */
			if(fscPoints.NumPainted != -1)
				fscPoints.NumPainted = numCaptured;

LogB.Information(" fc Q ");
		}
LogB.Information(" fc R ");

		Thread.Sleep (25);
		//LogB.Information(" ForceSensor:"+ forceCaptureThread.ThreadState.ToString());
		return true;
	}

	int force_capture_allocationXOld;
	bool force_capture_sizeChanged;
	public void on_force_capture_drawingarea_configure_event(object o, ConfigureEventArgs args)
	{
//		if(force_capture_drawingarea == null)
//			return;

		Gdk.EventConfigure ev = args.Event;
		Gdk.Window window = ev.Window;

		Gdk.Rectangle allocation = force_capture_drawingarea.Allocation;

		if(force_capture_pixmap == null || force_capture_sizeChanged ||
				allocation.Width != force_capture_allocationXOld)
		{
			force_capture_pixmap = new Gdk.Pixmap (window, allocation.Width, allocation.Height, -1);

			if(forceSensorOtherMode == forceSensorOtherModeEnum.CAPTURE_PRE)
//			if(forceCaptureThread != null) //&& capturingCsharp == encoderCaptureProcess.CAPTURING)
				fscPoints.NumPainted = -1; //mark meaning screen should be erased and start painting from the beginning
			else
				UtilGtk.ErasePaint(force_capture_drawingarea, force_capture_pixmap);

			force_capture_sizeChanged = false;
		}

		force_capture_allocationXOld = allocation.Width;
	}
	public void on_force_capture_drawingarea_expose_event(object o, ExposeEventArgs args)
	{
//		if(force_capture_drawingarea == null)
//			return;

		/* in some mono installations, configure_event is not called, but expose_event yes.
		 * Do here the initialization
		 */
		//LogB.Debug("EXPOSE");

		Gdk.Rectangle allocation = force_capture_drawingarea.Allocation;
		if(force_capture_pixmap == null || force_capture_sizeChanged ||
				allocation.Width != force_capture_allocationXOld) {
			force_capture_pixmap = new Gdk.Pixmap (force_capture_drawingarea.GdkWindow,
					allocation.Width, allocation.Height, -1);

			if(forceSensorOtherMode == forceSensorOtherModeEnum.CAPTURE_PRE)
			//if(forceCaptureThread != null) //&& capturingCsharp == encoderCaptureProcess.CAPTURING)
				fscPoints.NumPainted = -1; //mark meaning screen should be erased and start painting from the beginning
			else
				UtilGtk.ErasePaint(force_capture_drawingarea, force_capture_pixmap);

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


	private void on_button_force_sensor_load_clicked (object o, EventArgs args)
	{
		Gtk.FileChooserDialog filechooser = new Gtk.FileChooserDialog ("Choose file",
		                                                               app1, FileChooserAction.Open,
		                                                               "Cancel",ResponseType.Cancel,
		                                                               "Choose",ResponseType.Accept);
		string dataDir = ForceSensorGraph.GetDataDir(currentSession.UniqueID);
		filechooser.SetCurrentFolder(dataDir);

		FileFilter file_filter = new FileFilter();
		file_filter.AddPattern ("*.csv");

		lastForceSensorFile = "";
		if (filechooser.Run () == (int)ResponseType.Accept)
		{
			lastForceSensorFile = Util.RemoveExtension(Util.GetLastPartOfPath(filechooser.Filename));
			File.Copy(filechooser.Filename, UtilEncoder.GetmifCSVFileName(), true); //can be overwritten

			forceSensorDoSignalGraph();
			forceSensorDoRFDGraph();
		}
		filechooser.Destroy ();
	}

	void forceSensorDoRFDGraph()
	{
		string imagePath = UtilEncoder.GetmifTempFileName();
		Util.FileDelete(imagePath);
		image_force_sensor_graph.Sensitive = false;

		ForceSensorGraph fsg = new ForceSensorGraph(rfdList, impulse);
		bool success = fsg.CallR(
				viewport_force_sensor_graph.Allocation.Width -5,
				viewport_force_sensor_graph.Allocation.Height -5);

		if(! success)
		{
			event_execute_label_message.Text = "Error doing RFD graph.";
			return;
		}

		while ( ! Util.FileReadable(imagePath));

		image_force_sensor_graph = UtilGtk.OpenImageSafe(
				imagePath,
				image_force_sensor_graph);
		image_force_sensor_graph.Sensitive = true;
		button_force_sensor_image_save_rfd.Sensitive = true;
	}

	void forceSensorDoSignalGraph()
	{
		forceSensorDoSignalGraphReadFile();
		forceSensorDoSignalGraphPlot();
	}
	void forceSensorDoSignalGraphReadFile()
	{
		fscPoints = new ForceSensorCapturePoints(
				force_capture_drawingarea.Allocation.Width,
				force_capture_drawingarea.Allocation.Height
				);

		List<string> contents = Util.ReadFileAsStringList(UtilEncoder.GetmifCSVFileName());
		bool headersRow = true;

		//initialize
		forceSensorValues = new ForceSensorValues();

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
					int time = Convert.ToInt32(strFull[0]);
					double force = Convert.ToDouble(strFull[1]);

					fscPoints.Add(time, force);
					fscPoints.NumCaptured ++;

					forceSensorValues.TimeLast = time;
					forceSensorValues.ForceLast = force;
					forceSensorValues.SetMaxMinIfNeeded(force, time);
				}
			}
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
		if(fscPoints.OutsideGraph(forceSensorValues.TimeLast, forceSensorValues.ForceMax, forceSensorValues.ForceMin))
			fscPoints.Redo();

		forcePaintHVLines(forceSensorValues.ForceMax, forceSensorValues.ForceMin, forceSensorValues.TimeLast);

		Gdk.Point [] paintPoints = new Gdk.Point[fscPoints.Points.Count];
		for(int i = 0; i < fscPoints.Points.Count; i ++)
			paintPoints[i] = fscPoints.Points[i];

		force_capture_pixmap.DrawLines(pen_black_force_capture, paintPoints);

		//draw rectangle in maxForce
		//force_capture_pixmap.DrawRectangle(pen_red_force_capture, false,
		//		new Gdk.Rectangle(fscPoints.GetTimeInPx(maxForceTime) -5, fscPoints.GetForceInPx(maxForce) -5, 10, 10));

		//draw circle in maxForce
		force_capture_pixmap.DrawArc(pen_red_force_capture, false,
				fscPoints.GetTimeInPx(forceSensorValues.TimeForceMax) -6,
				fscPoints.GetForceInPx(forceSensorValues.ForceMax) -6,
				12, 12, 90 * 64, 360 * 64);

		force_capture_drawingarea.QueueDraw(); // -- refresh

		label_force_sensor_value.Text = forceSensorValues.ForceLast.ToString();
		label_force_sensor_value_max.Text = forceSensorValues.ForceMax.ToString();
		label_force_sensor_value_min.Text = forceSensorValues.ForceMin.ToString();
		button_force_sensor_image_save_signal.Sensitive = true;
	}

	private void forcePaintHVLines(double maxForce, double minForce, int lastTime)
	{
		forcePaintHLine(0);
		double absoluteMaxForce = maxForce;
		if(Math.Abs(minForce) > absoluteMaxForce)
			absoluteMaxForce = Math.Abs(minForce);

		int step = 100;
		if(absoluteMaxForce < 200)
			step = 50;
		if(absoluteMaxForce < 100)
			step = 25;

		for(int i = step; i <= absoluteMaxForce ; i += step)
		{
			forcePaintHLine(i);
			forcePaintHLine(i *-1);
		}

		int lastTimeInSeconds = lastTime / 1000000; //from microseconds to seconds
		step = 1;
		if(lastTimeInSeconds > 10)
			step = 5;
		if(lastTimeInSeconds > 50)
			step = 10;
		if(lastTimeInSeconds > 100)
			step = 20;

		for(int i = 0; i <= lastTimeInSeconds ; i += step)
			forcePaintTimeValue(i);
	}

	private void forcePaintTimeValue(int time)
	{
		int xPx = fscPoints.GetTimeInPx(1000000 * time);

		layout_force_text.SetMarkup(time.ToString() + "s");
		int textWidth = 1;
		int textHeight = 1;
		layout_force_text.GetPixelSize(out textWidth, out textHeight);
		force_capture_pixmap.DrawLayout (pen_gray_force_capture,
				xPx - textWidth/2, force_capture_drawingarea.Allocation.Height - textHeight, layout_force_text);

		//draw vertical line
		force_capture_pixmap.DrawLine(pen_gray_force_capture,
				xPx, 4, xPx, force_capture_drawingarea.Allocation.Height - textHeight -4);
	}

	private void forcePaintHLine(int yForce)
	{
		int yPx = fscPoints.GetForceInPx(yForce);
		//draw horizontal line
		force_capture_pixmap.DrawLine(pen_gray_force_capture,
				fscPoints.GetTimeInPx(0), yPx, force_capture_drawingarea.Allocation.Width, yPx);

		layout_force_text.SetMarkup(yForce.ToString());
		int textWidth = 1;
		int textHeight = 1;
		layout_force_text.GetPixelSize(out textWidth, out textHeight);
		force_capture_pixmap.DrawLayout (pen_gray_force_capture,
				fscPoints.GetTimeInPx(0) - textWidth -4, yPx - textHeight/2, layout_force_text);
	}

	private void on_button_force_sensor_image_save_signal_clicked (object o, EventArgs args)
	{
		checkFile(Constants.CheckFileOp.FORCESENSOR_SAVE_IMAGE_SIGNAL);
	}
	private void on_button_force_sensor_image_save_rfd_clicked (object o, EventArgs args)
	{
		checkFile(Constants.CheckFileOp.FORCESENSOR_SAVE_IMAGE_RFD);
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

	void on_button_forcesensor_save_image_rfd_file_selected (string destination)
	{
		File.Copy(UtilEncoder.GetmifTempFileName(), destination, true);
	}
	private void on_overwrite_file_forcesensor_save_image_rfd_accepted(object o, EventArgs args)
	{
		on_button_forcesensor_save_image_rfd_file_selected (exportFileName);

		string myString = string.Format(Catalog.GetString("Saved to {0}"), exportFileName);
		new DialogMessage(Constants.MessageTypes.INFO, myString);
	}


	private void on_button_force_sensor_data_folder_clicked	(object o, EventArgs args)
	{
		string dataDir = ForceSensorGraph.GetDataDir(currentSession.UniqueID);
		if(dataDir != "")
			System.Diagnostics.Process.Start(dataDir);
		else
			new DialogMessage(Constants.MessageTypes.WARNING, Constants.DirectoryCannotOpen);
	}

	private void showHideForceSensorControls(bool modeForceSensor)
	{
		hbox_capture_phases_time_record.Visible = ! modeForceSensor;
		hbox_options_top.Visible = ! modeForceSensor;
		notebook_options_top.Visible = ! modeForceSensor;

		button_threshold.Visible = ! modeForceSensor;
		button_force_sensor_check_version.Visible = modeForceSensor;
	}

	private void on_button_force_sensor_adjust_help_clicked (object o, EventArgs args)
	{
		new DialogMessage("Force sensor adjust data", Constants.MessageTypes.INFO, preferences.GetForceSensorAdjustString());
	}

	double lastChangedTime; //changeSlideCode
	private void changeSlideIfNeeded(int time, double force)
	{
		if(force > 100) {
			//changeSlide if one second or more elapsed since last change
			if(time - lastChangedTime >= 1)
			{
				changeSlide(true);
				lastChangedTime = time;
			}
		}
		if(force < -100) {
			//changeSlide if one second or more elapsed since last change
			if(time - lastChangedTime >= 1)
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

		ExecuteProcess.Result execute_result = ExecuteProcess.run (executable, parameters);

		LogB.Information("\n<------ Done calling slide");
		return execute_result.success;
	}

}
