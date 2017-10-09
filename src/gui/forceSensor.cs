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
using Glade;
using System.Text; //StringBuilder
using System.Collections.Generic; //List<T>

public partial class ChronoJumpWindow 
{
	[Widget] Gtk.Button button_force_sensor_tare;
	[Widget] Gtk.Button button_force_sensor_calibrate;
	[Widget] Gtk.Label label_force_sensor_value_max;
	[Widget] Gtk.Label label_force_sensor_value;
	[Widget] Gtk.Label label_force_sensor_value_min;
	[Widget] Gtk.VScale vscale_force_sensor;
	[Widget] Gtk.Viewport viewport_force_sensor_graph;
	[Widget] Gtk.Image image_force_sensor_graph;
	[Widget] Gtk.SpinButton spin_force_sensor_calibration_kg_value;
	
	Thread forceCaptureThread;
	static bool forceProcessFinish;
	static bool forceProcessCancel;

	Thread forceOtherThread; //for messages on: capture, tare, calibrate
	static string forceSensorOtherMessage = "";
	static bool forceSensorOtherMessageShowSeconds;
	static DateTime forceSensorTimeStart;

	/*
	 * forceStatus:
	 * STOP is when is not used
	 * STARTING is while is waiting forceSensor to start capturing
	 * CAPTURING is when data is arriving
	 * COPIED_TO_TMP means data is on tmp and graph can be called
	 */
	enum forceStatus { STOP, STARTING, CAPTURING, COPIED_TO_TMP }
	static forceStatus capturingForce = forceStatus.STOP;

	static bool forceCaptureStartMark; 	//Just needed to display "Capturing message"
	static double forceSensorLast; 		//Needed to display value and move vscale

	string forceSensorPortName;
	SerialPort portFS;
	bool portFSOpened;

	//Don't reopen port because arduino makes reset and tare, calibration... is lost

	private bool forceSensorConnect()
	{
		LogB.Information(" FS connect 0 ");
		if(chronopicRegister.ConnectedOfType(ChronopicRegisterPort.Types.ARDUINO_FORCE) == null)
		{
			new DialogMessage(Constants.MessageTypes.WARNING, "Force sensor is not connected!");
			return false;
		}

		LogB.Information(" FS connect 1 ");
		forceSensorPortName = chronopicRegister.ConnectedOfType(ChronopicRegisterPort.Types.ARDUINO_FORCE).Port;
		LogB.Information(" FS connect 2 ");
		if(forceSensorPortName == null || forceSensorPortName == "")
		{
			new DialogMessage(Constants.MessageTypes.WARNING, "Please, select port");
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

	enum forceSensorOtherModeEnum { TARE, CALIBRATE, CAPTURE_PRE }
	static forceSensorOtherModeEnum forceSensorOtherMode;

	private void on_buttons_force_sensor_clicked(object o, EventArgs args)
	{
		capturingForce = forceStatus.STOP;
		forceSensorButtonsSensitive(false);
		forceSensorTimeStart = DateTime.Now;
		forceSensorOtherMessageShowSeconds = true;

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
		else { //if (o == (object) button_execute_test)
			forceSensorOtherMode = forceSensorOtherModeEnum.CAPTURE_PRE;
			forceOtherThread = new Thread(new ThreadStart(forceSensorCapturePre));
		}

		GLib.Idle.Add (new GLib.IdleHandler (pulseGTKForceSensorOther));

		LogB.ThreadStart();
		forceOtherThread.Start();
	}

	void forceSensorButtonsSensitive(bool sensitive)
	{
		button_force_sensor_tare.Sensitive = sensitive;
		button_force_sensor_calibrate.Sensitive = sensitive;
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

			if(forceSensorOtherMode == forceSensorOtherModeEnum.TARE || forceSensorOtherMode == forceSensorOtherModeEnum.CALIBRATE)
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
		if(! portFSOpened)
		{
			if(! forceSensorConnect())
				return;
		}

		if(! forceSensorSendCommand("tare:", "Taring ...", "Catched force taring"))
			return;

		string str = "";
		do {
			Thread.Sleep(100); //sleep to let arduino start reading
			str = portFS.ReadLine();
			LogB.Information("init string: " + str);
		}
		while(! str.Contains("Taring OK"));

		forceSensorOtherMessageShowSeconds = false;
		forceSensorOtherMessage = "Tared!";
	}

	//Attention: no GTK here!!
	private void forceSensorCalibrate()
	{
		if(! portFSOpened)
		{
			if(! forceSensorConnect())
				return;
		}

		if(! forceSensorSendCommand("calibrate:" + spin_force_sensor_calibration_kg_value.Value.ToString() + ";",
				"Calibrating ...", "Catched force calibrating"))
			return;

		string str = "";
		do {
			Thread.Sleep(100); //sleep to let arduino start reading
			str = portFS.ReadLine();
			LogB.Information("init string: " + str);
		}
		while(! str.Contains("Calibrating OK"));

		forceSensorOtherMessageShowSeconds = false;
		forceSensorOtherMessage = "Calibrated!";
	}

	//Attention: no GTK here!!
	private void forceSensorCapturePre()
	{
		if(! portFSOpened)
		{
			if(! forceSensorConnect())
				return;
		}

		forceSensorOtherMessage = "Please, wait ...";
		capturingForce = forceStatus.STARTING;
	}

	private void forceSensorCapturePre2()
	{
		button_execute_test.Sensitive = false;
		event_execute_button_finish.Sensitive = true;
		event_execute_button_cancel.Sensitive = true;
		forceCaptureStartMark = false;
		vscale_force_sensor.Value = 0;
		label_force_sensor_value_max.Text = "0";
		label_force_sensor_value.Text = "0";
		label_force_sensor_value_min.Text = "0";

		forceProcessFinish = false;
		forceProcessCancel = false;
		forceSensorLast = 0;
		
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
			str = portFS.ReadLine();
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
		double firstTime = 0;
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

			//needed to store double in user locale
			double time = Convert.ToDouble(Util.ChangeDecimalSeparator(strFull[0]));

			//measurement does not start at 0 time. When we start receiving data, mark this as firstTime
			if(firstTime == 0)
				firstTime = time;

			//use this to have time starting at 0
			time -= firstTime;

			double force = Convert.ToDouble(Util.ChangeDecimalSeparator(strFull[1]));

			writer.WriteLine(time.ToString() + ";" + force.ToString());
			forceSensorLast = force;

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
			File.Copy(fileName,
					Path.GetTempPath() + Path.DirectorySeparatorChar + "cj_mif_Data.csv",
					true); //can be overwritten
			capturingForce = forceStatus.COPIED_TO_TMP;
		}
	}
	
	private bool pulseGTKForceSensorCapture ()
	{
		//LogB.Information(capturingForce.ToString())
		if(! forceCaptureThread.IsAlive || forceProcessFinish || forceProcessCancel)
		{
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
					forceSensorDoGraph();
				}
			} else if(forceProcessCancel)
				event_execute_label_message.Text = "Cancelled.";
			else
				event_execute_label_message.Text = "";

			LogB.ThreadEnded(); 

			forceSensorButtonsSensitive(true);

			return false;
		}

		if(forceCaptureStartMark)
		{
			event_execute_label_message.Text = "Capturing ...";
			forceCaptureStartMark = false;
		}

		if(capturingForce == forceStatus.CAPTURING)
		{
			//A) resize vscale if needed
			int upper = Convert.ToInt32(vscale_force_sensor.Adjustment.Upper);
			int lower = Convert.ToInt32(vscale_force_sensor.Adjustment.Lower);
			bool changed = false;

			if(forceSensorLast > upper)
			{
				upper = Convert.ToInt32(forceSensorLast * 2);
				changed = true;
			}
			if(forceSensorLast < lower)
			{
				lower = Convert.ToInt32(forceSensorLast * 2); //good for negative values
				changed = true;
			}
			if(changed)
				vscale_force_sensor.SetRange(lower, upper);

			//B) change the value
			vscale_force_sensor.Value = forceSensorLast;
			label_force_sensor_value.Text = forceSensorLast.ToString();
			if(forceSensorLast > Convert.ToDouble(label_force_sensor_value_max.Text))
				label_force_sensor_value_max.Text = forceSensorLast.ToString();
			if(forceSensorLast < Convert.ToDouble(label_force_sensor_value_min.Text))
				label_force_sensor_value_min.Text = forceSensorLast.ToString();
		}

		Thread.Sleep (25);
//		LogB.Information(" ForceSensor:"+ forceCaptureThread.ThreadState.ToString());
		return true;
	}

	private void on_button_force_sensor_graph_clicked (object o, EventArgs args)
	{
		Gtk.FileChooserDialog filechooser = new Gtk.FileChooserDialog ("Choose file",
		                                                               app1, FileChooserAction.Open,
		                                                               "Cancel",ResponseType.Cancel,
		                                                               "Choose",ResponseType.Accept);
		string dataDir = ForceSensorGraph.GetDataDir(currentSession.UniqueID);
		filechooser.SetCurrentFolder(dataDir);

		FileFilter file_filter = new FileFilter();
		file_filter.AddPattern ("*.csv");

		if (filechooser.Run () == (int)ResponseType.Accept)
		{
			File.Copy(filechooser.Filename,
					Path.GetTempPath() + Path.DirectorySeparatorChar + "cj_mif_Data.csv",
					true); //can be overwritten

			forceSensorDoGraph();
		}
		filechooser.Destroy ();
	}

	void forceSensorDoGraph()
	{
		string imagePath = Path.GetTempPath() + Path.DirectorySeparatorChar + "cj_mif_Graph.png";
		Util.FileDelete(imagePath);
		image_force_sensor_graph.Sensitive = false;

		ForceSensorGraph fsg = new ForceSensorGraph(rfdList, impulse);
		bool success = fsg.CallR(
				viewport_force_sensor_graph.Allocation.Width -5,
				viewport_force_sensor_graph.Allocation.Height -5);

		if(! success)
		{
			new DialogMessage(Constants.MessageTypes.WARNING, "Error doing graph.");
			return;
		}

		while ( ! Util.FileReadable(imagePath));

		image_force_sensor_graph = UtilGtk.OpenImageSafe(
				imagePath,
				image_force_sensor_graph);
		image_force_sensor_graph.Sensitive = true;
	}

	private void on_button_force_sensor_data_folder_clicked	(object o, EventArgs args)
	{
		string dataDir = ForceSensorGraph.GetDataDir(currentSession.UniqueID);
		if(dataDir != "")
			System.Diagnostics.Process.Start(dataDir);
		else
			new DialogMessage(Constants.MessageTypes.WARNING, Constants.DirectoryCannotOpen);
	}


	double lastChangedTime; //changeSlideCode
	private void changeSlideIfNeeded(double time, double force)
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
