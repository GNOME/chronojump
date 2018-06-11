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
using System.Collections.Generic; //List<T>
using Mono.Unix;


public partial class ChronoJumpWindow 
{
	Thread runEncoderCaptureThread;
	static bool runEncoderProcessFinish;
	static bool runEncoderProcessCancel;
	static bool runEncoderProcessError;
	
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

		portRE = new SerialPort(runEncoderPortName, 115200); //runEncoder
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

		/*
		string version = runEncoderCheckVersionDo();
		LogB.Information("Version found: [" + version + "]");

		if(version == "0.1")
		{
			LogB.Information(" RE connect 6b, version 0.1: adjusting parameters...");
		}
		*/

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

		runEncoderButtonsSensitive(false);
		bool connected = runEncoderCapturePre();
		if(! connected)
			runEncoderButtonsSensitive(true);
	}

	//TODO: do all this with an "other" thread like in force sensor to allow connecting messages to be displayed
	private bool runEncoderCapturePre()
	{
		if(! portREOpened)
			if(! runEncoderConnect())
				return false;

		event_execute_label_message.Text = "Please, wait ...";
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

		if(! runEncoderSendCommand("start_capture:", "", "Catched run encoder capturing"))
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

		string nameDate = currentPerson.Name + "_" + UtilDate.ToFile(DateTime.Now);

		//fileName to save the csv
		string fileName = Util.GetRunEncoderSessionDir(currentSession.UniqueID) + Path.DirectorySeparatorChar + nameDate + ".csv";

		//lastRunEncoderFile to save the images
		lastRunEncoderFile = nameDate;


		TextWriter writer = File.CreateText(fileName);
		writer.WriteLine("Pulses;Time(useconds)");
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
			//LogB.Information("str: " + str); //TODO: remove this log
			if(str == "")
				continue;

			//check if there is one and only one ';'
			if( ! (str.Contains(";") && str.IndexOf(";") == str.LastIndexOf(";")) )
				continue;

			string [] strFull = str.Split(new char[] {';'});
			//LogB.Information("str: " + str);

			LogB.Information("pulses: " + strFull[0]);
			if(! Util.IsNumber(strFull[0], false))
				continue;

			LogB.Information("time microseconds: " + strFull[1]);
			if(! Util.IsNumber(strFull[1], false))
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
			int pulse = Convert.ToInt32(strFull[0]);
			int time = Convert.ToInt32(strFull[1]);
			writer.WriteLine(pulse.ToString() + ";" + time.ToString());
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
			File.Copy(fileName, UtilEncoder.GetRunEncoderCSVFileName(), true); //can be overwritten
			lastRunEncoderFullPath = fileName;
			capturingRunEncoder = arduinoCaptureStatus.COPIED_TO_TMP;

			//create graph
			RunEncoderGraph reg = new RunEncoderGraph(
					 30, 				//TODO: 30 hardcoded
					 currentPersonSession.Weight,  	//TODO: can be more if extra weight
					 currentPersonSession.Height,
					 25); 				//TODO: hardcoded
			reg.CallR(1699, 768); 				//TODO: hardcoded

			//TODO: check better if png is saved and have a cancel button
			while(! File.Exists(UtilEncoder.GetSprintEncoderImage()))
			{
				Thread.Sleep(500);
				try {
					File.Copy(UtilEncoder.GetSprintEncoderImage(),
							Util.GetRunEncoderSessionDir(currentSession.UniqueID) + Path.DirectorySeparatorChar + 
							lastRunEncoderFile + 	//nameDate
							".png",
							true); //can be overwritten
				} catch {
					LogB.Information("Couldn't copy the file");
				}
			}
		}
	}

	private string readFromRunEncoderIfDataArrived()
	{
		string str = "";
		if (portRE.BytesToRead > 0)
			str = portRE.ReadLine();
			//LogB.Information("PRE_get_calibrationfactor bytes: " + portFS.ReadExisting());

		return str;
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
			if(runEncoderProcessFinish)
			{
LogB.Information(" fc C finish");
/*
				if(capturingRunEncoder != arduinoCaptureStatus.COPIED_TO_TMP)
				{
					Thread.Sleep (25); //Wait file is copied
					return true;
				}
				else
				{
*/
					event_execute_label_message.Text = "Saved.";
					Thread.Sleep (250); //Wait a bit to ensure is copied

					/*
					fscPoints.InitRealWidthHeight();
					forceSensorDoSignalGraphPlot();
					forceSensorDoRFDGraph();

					//if drawingarea has still not shown, don't paint graph because GC screen is not defined
					if(force_sensor_ai_drawingareaShown)
						forceSensorDoGraphAI();
					*/
//				}
LogB.Information(" fc C finish 2");
			} else if(runEncoderProcessCancel || runEncoderProcessError)
			{
LogB.Information(" fc C cancel ");
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

}
