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

/*
   this class is used on the following classes,
   and ArduinoDiscover uses a List of this class to detect all elements without loose time for each one (connect and get_version)
   */
public class Arduino
{
	private SerialPort port; //static?

	private string portName;
	private int bauds;

	private string response;
	private bool opened;
	private ChronopicRegisterPort.Types discovered;

	/*
	TODO:
	//to control if enough time passed since Connect, get_version
	private DateTime connectStarted;
	private DateTime getVersionStarted;
	*/

	//constructor
	public Arduino (string portName, int bauds)
	{
		this.portName = portName;
		this.bauds = bauds;

		response = "";
		opened = false;
		discovered = ChronopicRegisterPort.Types.UNKNOWN;
		/*
		connectStarted = new DateTime(1900,1,1);
		getVersionStarted = new DateTime(1900,1,1);
		*/
	}

	public void CreateSerialPort ()
	{
		port = new SerialPort (portName, bauds);
	}

	public void OpenPort ()
	{
		port.Open ();
	}

	public void ClosePort ()
	{
		port.Close ();
		opened = false;
	}

	public bool BytesToRead ()
	{
		return (port.BytesToRead > 0);
	}

	public string ReadLine ()
	{
		return (port.ReadLine ());
	}

	public string ReadExisting ()
	{
		return (port.ReadExisting ());
	}

	public void WriteLine (string command)
	{
		port.WriteLine (command);
	}

	public string PortName {
		get { return portName; }
	}

	public bool Opened {
		set { opened = value; }
		get { return opened; }
	}

	public string Response {
		set { response = value; }
		get { return response; }
	}

	public ChronopicRegisterPort.Types Discovered {
		set { discovered = value; }
		get { return discovered; }
	}
}

//ArduinoCommunications
public abstract class ArduinoComms
{
	protected Arduino arduino;

	protected bool portConnect ()
	{
		arduino.CreateSerialPort ();

		try {
			arduino.OpenPort ();
			arduino.Opened = true;
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
		Thread.Sleep(2000); //sleep to let arduino start reading serial event
		//TODO: instead of sleep, it will be an StopWatch to not send the get_version until stopwatch ended

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
			arduino.WriteLine (command);
		}
		catch (Exception ex)
		{
			if(ex is System.IO.IOException || ex is System.TimeoutException)
			{
				LogB.Information("error: " + errorMessage);
				arduino.ClosePort();
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
			if (arduino.BytesToRead ())
			{
				try {
					//use this because if 9600 call an old Wichro that has no comm at this speed, will answer things and maybe never a line
					str += arduino.ReadExisting(); //The += is because maybe it receives part of the string
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
					arduino.Response = str;
				}
		}
		while(! (success || sw.Elapsed.TotalMilliseconds > waitLimitMs) );
		LogB.Information("ended waitResponse");

		return (success);
	}

	protected void flush ()
	{
		string str = "";
		if (arduino.BytesToRead ())
			str = arduino.ReadExisting ();

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

	protected void initialize ()
	{
		readedPos = 0;
		arduino.Response = "";

		emptyList();
	}

	protected bool readLine (out string str)
	{
		str = "";
		try {
			if (arduino.BytesToRead ())
			{
				str = arduino.ReadLine();
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
		arduino.ClosePort ();
	}

	public bool PortOpened
	{
		get { return arduino.Opened; }
	}
}

public class PhotocellWirelessCapture: ArduinoCapture
{
	private List<PhotocellWirelessEvent> list = new List<PhotocellWirelessEvent>();

	//constructor
	public PhotocellWirelessCapture (string portName)
	{
		arduino = new Arduino (portName, 115200);
		Reset ();
	}

	//after a first capture, put variales to zero
	public void Reset ()
	{
		initialize ();
	}

	public override bool CaptureStart()
	{
		LogB.Information("portOpened: " + arduino.Opened);
		// 0 connect if needed
		if(! arduino.Opened)
		{
			List<string> responseExpected_l = new List<string>();
			responseExpected_l.Add("Wifi-Controller");

			if(! portConnect ())
				return false;
			if(! getVersion ("local:get_version;", responseExpected_l))
				return false;
		}

		arduino.Opened = true;

		//LogB.Information(string.Format("arduinoCapture portName: {0}, bauds: {1}", portName, bauds));

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
	private List<Arduino> arduino_l;

	private string forceSensorStr = "Force_Sensor-";
	private string raceAnalyzerStr = "Race_Analyzer-";
	private string wichroStr = "Wifi-Controller-"; //Will be used for Wichro and Quick, then user will decide. "local:get_channel;" to know the channel

	//1st trying a list of just one port
	public ArduinoDiscover (List<string> portName_l)
	{
		arduino_l = new List<Arduino> ();

		foreach (string portName in portName_l)
			arduino_l.Add(new Arduino (portName, 115200));
	}

	//public List<ChronopicRegisterPort.Types> Discover ()
	public List<string> Discover () // TODO: return as an object
	{
		List<string> discovered_l = new List<string> ();
		foreach (Arduino ard in arduino_l)
		{
			arduino = ard; //arduino is the protected variable

			LogB.Information("Discover loop, port: " + arduino.PortName);
			if(connect ())
			{
				flush();
				discoverDo ();
				if(arduino.Discovered == ChronopicRegisterPort.Types.UNKNOWN)
					discoverOldWichros ();
			} else
				arduino.Discovered = ChronopicRegisterPort.Types.UNKNOWN;

			arduino.ClosePort (); //close even connect failed?

			discovered_l.Add(string.Format("{0} {1}", arduino.PortName, arduino.Discovered));
		}

		return discovered_l;
	}

	private bool connect ()
	{
		return portConnect();
	}

	// check with common get_version (any device except the first Wichros)
	private void discoverDo ()
	{
		List<string> responseExpected_l = new List<string>();
		responseExpected_l.Add(forceSensorStr);
		responseExpected_l.Add(raceAnalyzerStr);
		responseExpected_l.Add(wichroStr);

		if(getVersion ("get_version:", responseExpected_l))
		{
			LogB.Information("Discover found this device: " + arduino.Response);
			if(arduino.Response.Contains(forceSensorStr))
				arduino.Discovered = ChronopicRegisterPort.Types.ARDUINO_FORCE;
			else if(arduino.Response.Contains(raceAnalyzerStr))
				arduino.Discovered = ChronopicRegisterPort.Types.ARDUINO_RUN_ENCODER;
			else if(arduino.Response.Contains(wichroStr))
				arduino.Discovered = ChronopicRegisterPort.Types.RUN_WIRELESS;
		}
		flush(); //empty the port for future use
	}

	// check if it is an old Wichro (has different get_version command)
	private void discoverOldWichros ()
	{
		List<string> responseExpected_l = new List<string>();
		responseExpected_l.Add(wichroStr);

		if(getVersion ("local:get_version;", responseExpected_l))
			arduino.Discovered = ChronopicRegisterPort.Types.RUN_WIRELESS;

		flush(); //empty the port for future use
	}
}
