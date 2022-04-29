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
   and MicroDiscover uses a List of this class to detect all elements without loose time for each one (connect and get_version)
   */
public class Micro
{
	private SerialPort port; //static?

	private string portName;
	private int bauds;

	private string response;
	private bool opened;
	private ChronopicRegisterPort.Types discovered;

	//constructor
	public Micro (string portName, int bauds)
	{
		this.portName = portName;
		this.bauds = bauds;

		response = "";
		opened = false;
		discovered = ChronopicRegisterPort.Types.UNKNOWN;
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
public abstract class MicroComms
{
	protected Micro micro;

	protected bool portConnect (bool doSleep)
	{
		micro.CreateSerialPort ();

		try {
			micro.OpenPort ();
			micro.Opened = true;
		}
		catch (System.IO.IOException)
		{
			LogB.Information("Error: could not open port");
			return false;
		}

		LogB.Information("port successfully opened");

		if(doSleep)
			Thread.Sleep(2000); //sleep to let arduino start reading serial event

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
			micro.WriteLine (command);
		}
		catch (Exception ex)
		{
			if(ex is System.IO.IOException || ex is System.TimeoutException)
			{
				LogB.Information("error: " + errorMessage);
				micro.ClosePort();
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
			if (micro.BytesToRead ())
			{
				try {
					//use this because if 9600 call an old Wichro that has no comm at this speed, will answer things and maybe never a line
					str += micro.ReadExisting(); //The += is because maybe it receives part of the string
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
					micro.Response = str;
				}
		}
		while(! (success || sw.Elapsed.TotalMilliseconds > waitLimitMs) );
		LogB.Information("ended waitResponse");

		return (success);
	}

	protected void flush ()
	{
		string str = "";
		if (micro.BytesToRead ())
			str = micro.ReadExisting ();

		LogB.Information(string.Format("flushed: |{0}|", str));
	}
}

public abstract class ArduinoCapture : MicroComms
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
		micro.Response = "";

		emptyList();
	}

	protected bool readLine (out string str)
	{
		str = "";
		try {
			if (micro.BytesToRead ())
			{
				str = micro.ReadLine();
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
		micro.ClosePort ();
	}

	public bool PortOpened
	{
		get { return micro.Opened; }
	}
}

public class PhotocellWirelessCapture: ArduinoCapture
{
	private List<PhotocellWirelessEvent> list = new List<PhotocellWirelessEvent>();

	//constructor
	public PhotocellWirelessCapture (string portName)
	{
		micro = new Micro (portName, 115200);
		Reset ();
	}

	//after a first capture, put variales to zero
	public void Reset ()
	{
		initialize ();
	}

	public override bool CaptureStart()
	{
		LogB.Information("portOpened: " + micro.Opened);
		// 0 connect if needed
		if(! micro.Opened)
		{
			List<string> responseExpected_l = new List<string>();
			responseExpected_l.Add("Wifi-Controller");

			if(! portConnect (true))
				return false;
			if(! getVersion ("local:get_version;", responseExpected_l))
				return false;
		}

		micro.Opened = true;

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
public class MicroDiscover : MicroComms
{
	private List<Micro> micro_l;
	private List<MicroDiscoverManage> microDiscoverManage_l;

	private string forceSensorStr = "Force_Sensor-";
	private string raceAnalyzerStr = "Race_Analyzer-";
	private string wichroStr = "Wifi-Controller-"; //Will be used for Wichro and Quick, then user will decide. "local:get_channel;" to know the channel

	//1st trying a list of just one port
	public MicroDiscover (List<string> portName_l)
	{
		micro_l = new List<Micro> ();
		microDiscoverManage_l = new List<MicroDiscoverManage> ();

		foreach (string portName in portName_l)
		{
			micro_l.Add(new Micro (portName, 115200));
			microDiscoverManage_l.Add(new MicroDiscoverManage (portName));
		}
	}

	//public List<ChronopicRegisterPort.Types> Discover ()
	public List<string> Discover () // TODO: return as an object
	{
		List<string> discovered_l = new List<string> ();
		foreach (Micro ard in micro_l)
		{
			micro = ard; //micro is the protected variable

			LogB.Information("Discover loop, port: " + micro.PortName);
			if(connectAndSleep ())
			{
				flush();
				discoverDo ();
				if(micro.Discovered == ChronopicRegisterPort.Types.UNKNOWN)
					discoverOldWichros ();
			} else
				micro.Discovered = ChronopicRegisterPort.Types.UNKNOWN;

			micro.ClosePort (); //close even connect failed?

			discovered_l.Add(string.Format("{0} {1}", micro.PortName, micro.Discovered));
		}

		return discovered_l;
	}

	//Calls first all the connects, then the get_version
	//has stopwatch to know time passed and is not waiting after each connect
	public List<string> DiscoverNotSequential ()
	{
		List<string> discovered_l = new List<string> ();

		//connect
		for (int i = 0; i < micro_l.Count; i ++)
		{
			micro = micro_l[i]; //micro is the protected variable
			microDiscoverManage_l[i].ConnectCalled (connectNotSleep ());
		}

		//get Version when connect time passed
		for (int i = 0; i < micro_l.Count; i ++)
		{
			if(! microDiscoverManage_l[i].ConnectOk)
				microDiscoverManage_l[i].Discovered = ChronopicRegisterPort.Types.UNKNOWN;
			else
			{
				//wait the ms since connect
				while (! microDiscoverManage_l[i].PassedMsSinceConnect (2000))
					;

				//TODO: right now have to wait at each getVersion, improve it
				micro = micro_l[i]; //micro is the protected variable
				flush();
				discoverDo ();
				if(micro.Discovered == ChronopicRegisterPort.Types.UNKNOWN)
					discoverOldWichros ();

				microDiscoverManage_l[i].Discovered = micro.Discovered;
			}

			micro.ClosePort (); //close even connect failed?
			discovered_l.Add(microDiscoverManage_l[i].ResultStr ());
		}

		return discovered_l;
	}

	private bool connectAndSleep ()
	{
		return portConnect(true);
	}
	private bool connectNotSleep () // not sequential
	{
		return portConnect(false);
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
			LogB.Information("Discover found this device: " + micro.Response);
			if(micro.Response.Contains(forceSensorStr))
				micro.Discovered = ChronopicRegisterPort.Types.ARDUINO_FORCE;
			else if(micro.Response.Contains(raceAnalyzerStr))
				micro.Discovered = ChronopicRegisterPort.Types.ARDUINO_RUN_ENCODER;
			else if(micro.Response.Contains(wichroStr))
				micro.Discovered = ChronopicRegisterPort.Types.RUN_WIRELESS;
		}
		flush(); //empty the port for future use
	}

	// check if it is an old Wichro (has different get_version command)
	private void discoverOldWichros ()
	{
		List<string> responseExpected_l = new List<string>();
		responseExpected_l.Add(wichroStr);

		if(getVersion ("local:get_version;", responseExpected_l))
			micro.Discovered = ChronopicRegisterPort.Types.RUN_WIRELESS;

		flush(); //empty the port for future use
	}
}

/*
   class to store the connection success on not sequential
   this could be all on Micro class, but maybe better have it separated as this is used only for Discover
   */
public class MicroDiscoverManage
{
	private string portName;
	private DateTime connectStarted;
	//private DateTime getVersionStarted;
	private ChronopicRegisterPort.Types discovered;
	private bool connectOk;

	public MicroDiscoverManage (string portName)
	{
		this.portName = portName;

		connectStarted = new DateTime(1900,1,1);
		//getVersionStarted = new DateTime(1900,1,1);
		connectOk = false;
	}

	public void ConnectCalled (bool ok)
	{
		connectOk = ok;
		connectStarted = DateTime.Now;
	}

	public bool PassedMsSinceConnect (int ms)
	{
		TimeSpan span = DateTime.Now - connectStarted;
		return (span.TotalMilliseconds >= ms);
	}

	public string ResultStr ()
	{
		return (string.Format("{0} {1}", portName, discovered));
	}

	public bool ConnectOk {
		get { return connectOk; }
	}

	public string PortName {
		get { return portName; }
	}

	public ChronopicRegisterPort.Types Discovered {
		set { discovered = value; }
		get { return discovered; }
	}

}
