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
 * Copyright (C) 2022-2023  Xavier de Blas <xaviblas@gmail.com>
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
		LogB.Information (string.Format ("micro[{0}].OpenPort () ...", this.ToString () ));
		port.Open ();
		LogB.Information("... opened!");
	}

	public void ClosePort ()
	{
		LogB.Information (string.Format ("micro[{0}].ClosePort () ...", this.ToString () ));
		port.Close ();
		opened = false;
		LogB.Information("... closed!");
	}

	public bool BytesToRead ()
	{
		return (port.BytesToRead > 0);
	}

	//used on Chronopic
	public int ReadByte ()
	{
		return (port.ReadByte ());
	}

	public string ReadLine ()
	{
		return (port.ReadLine ());
	}

	public string ReadExisting ()
	{
		return (port.ReadExisting ());
	}

	//used on Chronopic
	public void Write (string command)
	{
		port.Write (command);
	}

	public void WriteLine (string command)
	{
		port.WriteLine (command);
	}

	/*
	 * this does not work
	public void WriteCancel ()
	{
		port.DiscardOutBuffer();
	}
	*/

	public override string ToString ()
	{
		return string.Format ("portName: {0}, bauds: {1}", portName, bauds);
	}

	public bool IsOpen ()
	{
		return port.IsOpen;
	}

	public string PortName {
		get { return portName; }
	}

	public int Bauds {
		set { bauds = value; }
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
	protected static bool cancel;

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

		//forceSensor need this, cannot send the get_version before this seconds
		if(doSleep)
			Thread.Sleep(2000); //sleep to let arduino start reading serial event
		/*
		   This does not work, because it says IsOpen in 100-200 ms, but it is not ready to receive the get_version
		   maybe a solution will be to send the get_version each 100 ms
		   */
		LogB.Information("waiting to be opened");
		do {
			Thread.Sleep(100);
			LogB.Information("waiting to be opened");
		} while ( ! micro.IsOpen () );

		LogB.Information("truly opened!, but maybe too early to get_version");

		return true;
	}

	//false could mean a error on sending command or not received the expected response
	protected bool getVersion (string getVersionStr, List<string> responseExpected_l,
			bool cleanAllZeros, int waitResponseTime)
	{
		if(! sendCommand (getVersionStr, "error getting version")) //note this is for Wichro
			return false;

		return waitResponse (responseExpected_l, cleanAllZeros, waitResponseTime);
	}

	/*
	   repeat the command n times
	   this allows to call it many times while the port is being really opened after connect
	   */
	protected bool getVersionNTimes (string getVersionStr, List<string> responseExpected_l,
			bool cleanAllZeros, int times, int waitLimitMs)
	{
		bool success = false;
		int count = 0;

		do {
			if(! sendCommand (getVersionStr, "error getting version")) //note this is for Wichro
				return false;

			success = waitResponse (responseExpected_l, cleanAllZeros, waitLimitMs);
			count ++;
			if (count >= times)
				break;

			if(! success)
				Thread.Sleep(100);

		} while (! success);

		return success;
	}

	protected bool sendCommand (string command, string errorMessage)
	{
		try {
			LogB.Information("micro sendCommand: |" + command + "|");
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

	//cleanAllZeros is used for encoder
	protected bool waitResponse (List<string> responseExpected_l, bool cleanAllZeros, int waitResponseMs)
	{
		string str = "";
		bool success = false;
		Stopwatch sw = new Stopwatch();
		sw.Start();
		LogB.Information("starting waitResponse");
		do {
			Thread.Sleep(25);
			if (micro.BytesToRead ())
			{
				try {
					//use this because if 9600 call an old Wichro that has no comm at this speed, will answer things and maybe never a line
					string received = micro.ReadExisting();
					if (cleanAllZeros)
						received = Util.RemoveChar (received, '0', false);

					str += received; //The += is because maybe it receives part of the string
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
		while(! (success || cancel || sw.Elapsed.TotalMilliseconds > waitResponseMs) );
		LogB.Information("ended waitResponse");

		return (success);
	}

	protected bool flush ()
	{
		LogB.Information ("micro not opened on flush");
		if (! micro.Opened)
			return false;

		string str = "";

		bool bytesToRead = false;
		try {
			bytesToRead = micro.BytesToRead ();
		}
		catch (System.IO.IOException)
		{
			LogB.Information ("Catched on flush");
			return false;
		}

		if (bytesToRead)
			str = micro.ReadExisting ();

		LogB.Information(string.Format("flushed: |{0}|", str));
		return true;
	}

	public bool Cancel {
		get { return cancel; }
		set { cancel = value; }
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
		cancel = false;
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
			if(! getVersion ("local:get_version;", responseExpected_l, false, 2000))
				return false;
		}

		micro.Opened = true;

		//LogB.Information(string.Format("arduinoCapture portName: {0}, bauds: {1}", portName, bauds));

		//empty the port before new capture
		/*
		 * note a detected device if usb cable gets disconnected, then micro.Opened above is true,
		 * so previous to 22 may 2023 comes here and crashes. Now flush has a try/catch and returns a boolean,
		 * and CaptureStart return also is managed on execute/run.cs
		 */
		if (! flush())
		{
			LogB.Information ("device has been disconnected");
			micro.ClosePort ();
			return false;
		}


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

		/*
		 * if at CaptureStart device is disconnected,
		 * micro gets closed there and here it shoud not readLine
		 */
		if (! micro.Opened)
			return false;

		if(! readLine (out str))
		{
			micro.ClosePort ();
			return false;
		}

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
/*
   Right now not using the MicroDiscover as a way to discover all the devices at Chronojump start,
   what we will do is discover the needed device of for each mode before capture.
   So device button will replace capture button. User will click on discover and then capture will be done
   For Chronopic multitest and encoder will be different because FTDI works properly (with different IDs).
   For Races with photocells user can choose between wichro and chronopic multitest
   */
public class MicroDiscover : MicroComms
{
	private List<Micro> micro_l;
	private List<MicroDiscoverManage> microDiscoverManage_l;

	//115200
	private string forceSensorStr = "Force_Sensor-";
	private string raceAnalyzerStr = "Race_Analyzer-";
	private string wichroStr = "Wifi-Controller-"; //Will be used for Wichro and Quick, then user will decide. "local:get_channel;" to know the channel
	private string encoderStr = "J"; //for encoder send a J and receive a J

	public enum Status { NotStarted, Connecting, Detecting, Done };
	private List<Status> progressBar_l; //progressBars status

	private List<ChronopicRegisterPort> toDiscover_l;

	//devices discovered compatible with current mode
	private static List<ChronopicRegisterPort.Types> discovered_l;

	//9600
	//private string rfidStr = "YES Chronojump RFID";
	//Chronopic multitest will send a J (9600)

	//1st trying a list of just one port
	public MicroDiscover (List<ChronopicRegisterPort> toDiscover_l)
	{
		this.toDiscover_l = toDiscover_l;

		micro_l = new List<Micro> ();
		microDiscoverManage_l = new List<MicroDiscoverManage> ();
		progressBar_l = new List<Status> ();

		cancel = false;

		foreach (ChronopicRegisterPort crp in toDiscover_l)
		{
			micro_l.Add (new Micro (crp.Port, 115200)); //for multitest will be changed below
			microDiscoverManage_l.Add (new MicroDiscoverManage (crp.Port));
			progressBar_l.Add (Status.NotStarted);
		}
	}

	//mode is forceSensor, runsEncoder, ...
	public void DiscoverOneMode (Constants.Modes mode)
	{
		discovered_l = new List<ChronopicRegisterPort.Types> ();
		bool success;
		for (int i = 0; i < micro_l.Count ; i ++)
		{
			micro = micro_l[i]; //micro is the protected variable

			progressBar_l[i] = Status.Connecting;
			success = false;

			LogB.Information("\nDiscover loop, port: " + micro.PortName);
			progressBar_l[i] = Status.Detecting;

			//flush(); //after connect
			if(mode == Constants.Modes.JUMPSSIMPLE || mode == Constants.Modes.JUMPSREACTIVE)
			{
				micro.Bauds = 9600;
				if(connectAndSleep ())
				{
					flush(); //after connect
					LogB.Information("calling discoverMultitest");
					success = discoverMultitest ();
					LogB.Information("ended discoverMultitest");
				}
			}
			else if(mode == Constants.Modes.RUNSSIMPLE || mode == Constants.Modes.RUNSINTERVALLIC)
			{
				//if we need to test low speed and high speed, better try first low
				//a 9600 device can get saturated if 115200 connection is done, will need to remove usb
				micro.Bauds = 9600;
				if(connectAndSleep ())
				{
					flush(); //after connect
					LogB.Information("calling discoverMultitest");
					success = discoverMultitest ();
					LogB.Information("ended discoverMultitest");
				}
				if (! success)
				{
					micro.ClosePort ();
					micro.Bauds = 115200;
					LogB.Information("connectAndSleep again");
					if (! micro.PortName.ToLower().Contains("acm") && connectAndSleep ())
					{
						LogB.Information("calling discoverWichro");
						success = discoverWichro ();
						LogB.Information("ended discoverWichro");
					}
				}
				LogB.Information("success: " + success.ToString());
			}
			else {
				if(connectAndSleep ())
				{
					flush(); //after connect

					if(Constants.ModeIsFORCESENSOR (mode))
					{
						LogB.Information("calling discoverForceSensor");
						success = discoverForceSensor ();
					}
					else if(mode == Constants.Modes.RUNSENCODER)
					{
						LogB.Information("calling discoverRaceAnalyzer");
						success = discoverRaceAnalyzer ();
					}
					else if (Constants.ModeIsENCODER (mode))
					{
						LogB.Information("calling discoverEncoder");
						success = discoverEncoder ();
					}
				}
			}

			micro.ClosePort (); //close even connect failed?

			//add to list only the relevant, eg in races will be Wichro (and maybe Chronopic multitest)
			if(success)
				discovered_l.Add(micro.Discovered);
			else {
				micro.Discovered = ChronopicRegisterPort.Types.UNKNOWN;
				discovered_l.Add(ChronopicRegisterPort.Types.UNKNOWN);
			}

			progressBar_l[i] = Status.Done;

			if(cancel)
				break;
		}
	}

	/*
	   these methods Discover all the devices,
	   just use method above to discover devices of each mode

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
				flush(); //after connect
				if (! discoverDo115200 ())
					if (! discoverOldWichros ())
					{
						//try at 9600
						micro.ClosePort ();
						micro.Bauds = 9600;
						if(connectAndSleep ())
						{
							flush(); //after connect
							discoverDo9600 ();
						}
					}
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
			{
				microDiscoverManage_l[i].Discovered = ChronopicRegisterPort.Types.UNKNOWN;
				micro.ClosePort (); //close even connect failed?
				continue;
			}

			//wait the ms since connect
			while (! microDiscoverManage_l[i].PassedMsSinceConnect (2000))
				;

			//TODO: right now have to wait at each getVersion, improve it
			micro = micro_l[i]; //micro is the protected variable
			flush();
			if (! discoverDo115200 ())
				if (! discoverOldWichros ())
				{
					//try at 9600
					micro.ClosePort ();
					micro.Bauds = 9600;
					if(connectAndSleep ())
					{
						flush(); //after connect
						discoverDo9600 ();
					}
				}

			microDiscoverManage_l[i].Discovered = micro.Discovered;

			micro.ClosePort ();
			discovered_l.Add(microDiscoverManage_l[i].ResultStr ());
		}

		return discovered_l;
	}
	*/

	private bool connectAndSleep ()
	{
		return portConnect(true);
	}
	private bool connectNotSleep () // not sequential
	{
		return portConnect(false);
	}

	private bool discoverForceSensor ()
	{
		bool success = false;
		List<string> responseExpected_l = new List<string>();
		responseExpected_l.Add(forceSensorStr);

		//if(getVersionDuringNTime ("get_version:", responseExpected_l, false, 4000))
		Thread.Sleep(1500); //force sensor wait 1500 ms after open to be able to receive commands
		if(cancel)
			return false;

		if(getVersion ("get_version:", responseExpected_l, false, 2000))
		{
			LogB.Information("Discover found this ForceSensor device: " + micro.Response);
			if(micro.Response.Contains(forceSensorStr))
			{
				micro.Discovered = ChronopicRegisterPort.Types.ARDUINO_FORCE;
				success = true;
			}
		}

		flush(); //empty the port for future use
		return success;
	}

	private bool discoverRaceAnalyzer ()
	{
		bool success = false;
		List<string> responseExpected_l = new List<string>();
		responseExpected_l.Add(raceAnalyzerStr);

		Thread.Sleep(1500); //raceAnalyzer wait 1500 ms after open to be able to receive commands
		if(cancel)
			return false;

		if(getVersion ("get_version:", responseExpected_l, false, 2000))
		{
			LogB.Information("Discover found this RaceAnalyzer device: " + micro.Response);
			if(micro.Response.Contains(raceAnalyzerStr))
			{
				micro.Discovered = ChronopicRegisterPort.Types.ARDUINO_RUN_ENCODER;
				success = true;
			}
		}

		flush(); //empty the port for future use
		return success;
	}

	private bool discoverWichro ()
	{
		bool success = false;
		List<string> responseExpected_l = new List<string>();
		responseExpected_l.Add(wichroStr);

		List<string> commands_l = new List<string> { "get_version:", "local:get_version;" };
		foreach (string command in commands_l)
		{
			if(getVersionNTimes (command, responseExpected_l, false, 2, 200))
			{
				LogB.Information("Discover found this WICHRO device: " + micro.Response);
				if(micro.Response.Contains(wichroStr))
				{
					micro.Discovered = ChronopicRegisterPort.Types.RUN_WIRELESS;
					success = true;
					break;
				}
			}
		}

		flush(); //empty the port for future use
		return success;
	}

	private bool discoverEncoder ()
	{
		bool success = false;
		List<string> responseExpected_l = new List<string>();
		responseExpected_l.Add (encoderStr);
		if(cancel)
			return false;

		if(getVersion ("J", responseExpected_l, true, 2000))
		{
			LogB.Information("Discover found this Encoder device: " + micro.Response);
			if(micro.Response.Contains(encoderStr))
			{
				micro.Discovered = ChronopicRegisterPort.Types.ENCODER;
				success = true;
			}
		}

		flush(); //empty the port for future use
		return success;
	}

	private bool discoverMultitest ()
	{
		bool success = false;

		LogB.Information("Going to write a J");
		micro.Write("J");
		LogB.Information("Going to read a J");

		Stopwatch sw = new Stopwatch();
		sw.Start();
		do {
			Thread.Sleep(25);
			if (micro.BytesToRead () && (char) micro.ReadByte() == 'J')
			{
				micro.Discovered = ChronopicRegisterPort.Types.CONTACTS;
				success = true;
			}
		}
		while(! (success || cancel || sw.Elapsed.TotalMilliseconds > 1000) );

		if (success)
			LogB.Information("Discover found this Multitest device: " + micro.Response);

		LogB.Information("done");

		flush(); //empty the port for future use
		return success;
	}

	/*
	   these methods Discover all the devices,
	   just use methods above to discover devices of each mode

	// check with common get_version (any device except the first Wichros)
	private bool discoverDo115200 ()
	{
		bool success = false;
		List<string> responseExpected_l = new List<string>();
		responseExpected_l.Add(forceSensorStr);
		responseExpected_l.Add(raceAnalyzerStr);
		responseExpected_l.Add(wichroStr);

		if(getVersionDuringNTime ("get_version:", responseExpected_l, false, 2000))
		{
			LogB.Information("Discover found this device: " + micro.Response);
			if(micro.Response.Contains(forceSensorStr))
			{
				micro.Discovered = ChronopicRegisterPort.Types.ARDUINO_FORCE;
				success = true;
			}
			else if(micro.Response.Contains(raceAnalyzerStr))
			{
				micro.Discovered = ChronopicRegisterPort.Types.ARDUINO_RUN_ENCODER;
				success = true;
			}
			else if(micro.Response.Contains(wichroStr))
			{
				micro.Discovered = ChronopicRegisterPort.Types.RUN_WIRELESS;
				success = true;
			}
		}

		if(! success)  //try encoder (send J, receive J).
		{
			responseExpected_l = new List<string>();
			responseExpected_l.Add(encoderStr);
			//TODO: cleanAllZeros must clean all digits, and then read if the only char (maybe repeated) is a J, becuase like its now could detect other device that sends any error message with J (and other chars). Take care because encoder seems to return also version (eg 1.1)
			if(getVersionDuringNTime ("J", responseExpected_l, true, 2000))
			{
				micro.Discovered = ChronopicRegisterPort.Types.ENCODER;
				success = true;
			}
		}

		flush(); //empty the port for future use
		return success;
	}

	// check if it is an old Wichro (has different get_version command)
	private bool discoverOldWichros ()
	{
		bool success = false;
		List<string> responseExpected_l = new List<string>();
		responseExpected_l.Add(wichroStr);

		if(getVersionDuringNTime ("local:get_version;", responseExpected_l, false, 2000))
		{
			micro.Discovered = ChronopicRegisterPort.Types.RUN_WIRELESS;
			success = true;
		}

		flush(); //empty the port for future use
		return success;
	}

	//for RFID and Chronopic multitest
	private bool discoverDo9600 ()
	{
		bool success = false;

		// 1) try if it is an RFID
		List<string> responseExpected_l = new List<string>();
		responseExpected_l.Add(rfidStr);

		if(getVersionDuringNTime ("Chronojump RFID", responseExpected_l, false, 2000))
		{
			LogB.Information("Discover found this device: " + micro.Response);
			if(micro.Response.Contains(rfidStr))
			{
				micro.Discovered = ChronopicRegisterPort.Types.ARDUINO_RFID;
				success = true;
			}

		}
		flush(); //empty the port for future use
		if(success)
			return true;

		// 2) try if it is a Chronopic multitest. Not working, also tried to do this first without success.
		LogB.Information("Going to write a J");
		micro.Write("J");
		LogB.Information("Going to read a J");
		if ( (char) micro.ReadByte() == 'J')
		{
			micro.Discovered = ChronopicRegisterPort.Types.CONTACTS;
			success = true;
		}
		LogB.Information("done");

		flush(); //empty the port for future use
		return success;
	}
	*/

	/* does not work
	public void CancelWrite ()
	{
		micro.WriteCancel ();
	}
	*/

	public List<Status> ProgressBar_l {
		get { return progressBar_l; }
	}

	public List<ChronopicRegisterPort> ToDiscover_l {
		get { return toDiscover_l; }
	}

	public List<ChronopicRegisterPort.Types> Discovered_l {
		get { return discovered_l; }
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
