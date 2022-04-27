/*
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
 *
 * Copyright (C) 2022  Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List<T>
using System.Diagnostics;  //Stopwatch
using System.IO.Ports;
using System.Threading;

//inspired on RFID.cs (unique class that reads arduino separated of gui

//ArduinoCommunications
public abstract class ArduinoComms
{
	public static SerialPort ArduinoPort; //on Windows we cannot pass the SerialPort to another class, so use this.
	public static bool PortOpened;

	protected string portName;
	protected int bauds;
//	protected SerialPort port;
//	protected bool portOpened;

	protected string response; //the response on waitResponse () if is what expected

	protected bool portConnect ()
	{
		//port = new SerialPort(portName, bauds);
		ArduinoPort = new SerialPort(portName, bauds);

		try {
			ArduinoPort.Open();
		}
		catch (System.IO.IOException)
		{
			LogB.Information("Error: could not open port");
			return false;
		}

		LogB.Information("port successfully opened");

		//TODO: Val, caldria que quedés clar a la interficie que estem esperant aquest temps, a veure com ho fa el sensor de força, ...
		//just print on gui something like "please, wait, ..."
		//
		Thread.Sleep(3000); //sleep to let arduino start reading serial event

		return true;
	}

	protected bool getVersion (string getVersionStr, List<string> responseExpected_l)
	{
		if(! sendCommand(getVersionStr, "error getting version")) //note this is for Wichro
			return false;

		return waitResponse(responseExpected_l);
	}

	protected bool sendCommand (string command, string errorMessage)
	{
		try {
			LogB.Information("arduinocapture sendCommand: |" + command + "|");
			ArduinoPort.WriteLine(command);
		}
		catch (Exception ex)
		{
			if(ex is System.IO.IOException || ex is System.TimeoutException)
			{
				LogB.Information("error: " + errorMessage);
				ArduinoPort.Close();
				//portOpened = false;
				PortOpened = false;
				return false;
			}
			//throw;
		}
		return true;
	}

	protected bool waitResponse (List<string> responseExpected_l)
	{
		string str = "";
		bool success = false;
		int waitLimitMs = 2000; //wait 2s //don't wait 1s because response will not come
		Stopwatch sw = new Stopwatch();
		sw.Start();
		LogB.Information("starting waitResponse");
		do {
			Thread.Sleep(25);
			if (ArduinoPort.BytesToRead > 0)
			{
				try {
					//str = ArduinoPort.ReadLine();
					//use this because if 9600 call an old Wichro that has no comm at this speed, will answer things and maybe never a line
					str += ArduinoPort.ReadExisting(); //The += is because maybe it receives part of the string
				} catch {
					if(responseExpected_l.Count == 1)
						LogB.Information(string.Format("Catched waiting: |{0}|", responseExpected_l[0]));
					else if(responseExpected_l.Count > 1)
					{
						LogB.Information("Catched waiting any of:");
						foreach(string expected in responseExpected_l)
							LogB.Information("- " + expected);

					}
					return false;
				}
				LogB.Information(string.Format("received: |{0}|", str));
			}

			foreach(string expected in responseExpected_l)
				if(str.Contains(expected))
				{
					success = true;
					response = str;
				}
		}
		while(! (success || sw.Elapsed.TotalMilliseconds > waitLimitMs) );
		LogB.Information("ended waitResponse");

		return (success);
	}

	protected void flush ()
	{
		string str = "";
		if (ArduinoPort.BytesToRead > 0)
			str = ArduinoPort.ReadExisting();

		LogB.Information(string.Format("flushed: |{0}|", str));
	}
}

public abstract class ArduinoCapture : ArduinoComms
{
	protected int readedPos; //position already readed from list

	// public stuff ---->

	public abstract bool CaptureStart();
	public abstract bool CaptureLine();
	public abstract bool Stop();
	public abstract bool CanRead();

	//have methods for get objects on each of the derived classes
	public abstract List<PhotocellWirelessEvent> PhotocellWirelessCaptureGetList();
	public abstract PhotocellWirelessEvent PhotocellWirelessCaptureReadNext();

	public int ReadedPos
	{
		get { return readedPos; }
	}

	// protected stuff ---->

	protected void initialize (string portName, int bauds)
	{
		this.portName = portName;
		this.bauds = bauds;
		readedPos = 0;
		response = "";

		emptyList();
	}


	protected bool readLine (out string str)
	{
		str = "";
		try {
			if (ArduinoPort.BytesToRead > 0)
			{
				str = ArduinoPort.ReadLine();
				LogB.Information(string.Format("at readLine BytesToRead>0, readed:|{0}|", str));
			}
		} catch (System.IO.IOException)
		{
			LogB.Information("Catched reading!");
			return false;
		}
		return true;
	}

	protected abstract void emptyList();

	public void Disconnect()
	{
		ArduinoPort.Close();
		//portOpened = false;
		PortOpened = false;
	}

	/*
	public bool PortOpened
	{
		get { return portOpened; }
	}
	*/
}

public class PhotocellWirelessCapture: ArduinoCapture
{
	private List<PhotocellWirelessEvent> list = new List<PhotocellWirelessEvent>();

	//constructor
	public PhotocellWirelessCapture (string portName)
	{
		Reset(portName);
	}

	//after a first capture, put variales to zero
	public void Reset (string portName)
	{
		this.bauds = 115200;
		initialize(portName, bauds);
	}

	public override bool CaptureStart()
	{
		LogB.Information("portOpened: " + ArduinoCapture.PortOpened.ToString());
		// 0 connect if needed
		if(! ArduinoCapture.PortOpened)
		{
			List<string> responseExpected_l = new List<string>();
			responseExpected_l.Add("Wifi-Controller");

			if(! portConnect ())
				return false;
			if(! getVersion ("local:get_version;", responseExpected_l))
				return false;
		}

		ArduinoCapture.PortOpened = true;

		LogB.Information(string.Format("arduinoCapture portName: {0}, bauds: {1}", portName, bauds));

		//empty the port before new capture
		flush();

		/*
		   disabled start_capture
		if (! sendCommand("start_capture:", "Catched at start_capture:"))
			return false;
		waitResponse("Starting capture");
		*/

		return true;
	}

	//if true: continue capturing; if false: error, end	
	public override bool CaptureLine ()
	{
		string str = "";

		if(! readLine (out str))
			return false;

		//LogB.Information("bucle capture call process line");
		PhotocellWirelessEvent pwe = new PhotocellWirelessEvent();
		if(! processLine (str, out pwe))
			return true;

		list.Add(pwe);
		LogB.Information("bucle capture list added: " + pwe.ToString());
		return true;
	}

	public override bool Stop()
	{
		LogB.Information("AT Capture: STOPPING");

		//empty any pending port read to be able to read correctly the Capture ended message
		flush();

		/*
		   disabled end_capture
		if (! sendCommand("end_capture:", "Catched at end_capture:"))
			return false;

		waitResponse("Capture ended");
		*/
		LogB.Information("AT Capture: STOPPED");

		/*
		ArduinoPort.Close();
		ArduinoCapture.PortOpened = false;
		*/

		return true;
	}

	public override bool CanRead()
	{
		return (list.Count > readedPos);
	}

	public override List<PhotocellWirelessEvent> PhotocellWirelessCaptureGetList()
	{
		return list;
	}
	
	public override PhotocellWirelessEvent PhotocellWirelessCaptureReadNext()
	{
		return list[readedPos++];
	}
	
	// protected stuff ---->

	protected override void emptyList()
	{
		list = new List<PhotocellWirelessEvent>();
	}

	// private stuff ---->

	/*
	 * Line example: 5;215;O
	 * this event means: At photocell 5, 2015 ms, status is Off
	 * could be O or I
	 */
	private bool processLine (string str, out PhotocellWirelessEvent pwe)
	{
		//LogB.Information(string.Format("at processLine, str: |{0}|", str));
		pwe = new PhotocellWirelessEvent();

		if(str == "")
			return false;

		LogB.Information("No trim str" + str);

		//get only the first line
		if(str.IndexOf(Environment.NewLine) > 0)
			str = str.Substring(0, str.IndexOf(Environment.NewLine));

		//Trim str
		str = str.Trim();

		LogB.Information(string.Format("Yes one line and trim str: |{0}|", str));

		string [] strFull = str.Split(new char[] {';'});
		if( strFull.Length != 3 ||
				! Util.IsNumber(strFull[0], false) ||
				! Util.IsNumber(strFull[1], false) ||
				//(strFull[2] != "O" && strFull[2] != "I")
				(strFull[2] != "0" && strFull[2] != "1")
				)
			return false;

		pwe = new PhotocellWirelessEvent(Convert.ToInt32(strFull[0]),
				Convert.ToInt32(strFull[1]), strFull[2]);

		return true;
	}
	
}

public class PhotocellWirelessEvent
{
	public int photocell;
	public int timeMs;
	public Chronopic.Plataforma status; // like run with chronopic

	public PhotocellWirelessEvent()
	{
		this.photocell = -1;
		this.timeMs = 0;
		this.status = Chronopic.Plataforma.UNKNOW;
	}

	public PhotocellWirelessEvent(int photocell, int timeMs, string status)
	{
		this.photocell = photocell;
		this.timeMs = timeMs;
		if(status == "1")
			this.status = Chronopic.Plataforma.OFF;
		else //(status == "0")
			this.status = Chronopic.Plataforma.ON;
	}

	public override string ToString()
	{
		return (string.Format("{0};{1};{2}", photocell, timeMs, status));
	}
}

//New firmwares enable communication at 9600 (event devices working at higher speeds) to get the version (contains the product)
public class ArduinoDiscover : ArduinoComms
{
	protected ChronopicRegisterPort.Types discovered;

	public ArduinoDiscover (string portName)
	{
		this.portName = portName;
		discovered = ChronopicRegisterPort.Types.UNKNOWN;
	}

	public ChronopicRegisterPort.Types Discover ()
	{
		if(! connect ())
			return discovered;

		discoverDo ();
		if(discovered == ChronopicRegisterPort.Types.UNKNOWN)
			discoverOldWichros ();

		ArduinoPort.Close();
		return discovered;
	}

	private bool connect ()
	{
		this.bauds = 9600;
		return portConnect();
	}

	// check with common get_version (any device except the first Wichros)
	private void discoverDo ()
	{
		string forceSensorStr = "Force_Sensor-";
		string raceAnalyzerStr = "Race_Analyzer-";
		string wichroStr = "Wichro";

		List<string> responseExpected_l = new List<string>();
		responseExpected_l.Add(forceSensorStr);
		responseExpected_l.Add(raceAnalyzerStr);
		responseExpected_l.Add(wichroStr);

		if(getVersion ("get_version:", responseExpected_l))
		{
			LogB.Information("Discover found this device: " + response);
			if(response.Contains(forceSensorStr))
				discovered = ChronopicRegisterPort.Types.ARDUINO_FORCE;
			else if(response.Contains(raceAnalyzerStr))
				discovered = ChronopicRegisterPort.Types.ARDUINO_RUN_ENCODER;
			else if(response.Contains(wichroStr))
				discovered = ChronopicRegisterPort.Types.RUN_WIRELESS;
		}
		flush(); //empty the port for future use
	}

	// check if it is an old Wichro (has different get_version command)
	private void discoverOldWichros ()
	{
		List<string> responseExpected_l = new List<string>();
		responseExpected_l.Add("Wichro");

		if(getVersion ("local:get_version;", responseExpected_l))
			discovered = ChronopicRegisterPort.Types.RUN_WIRELESS;

		flush(); //empty the port for future use
	}
}
