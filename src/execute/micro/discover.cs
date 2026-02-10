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
 * Copyright (C) 2022-2025  Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List<T>
using System.Diagnostics;  //Stopwatch
using System.IO.Ports;
using System.Threading;

//inspired on RFID.cs (unique class that reads arduino separated of gui


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
	private string fourPlatformsStr = "4Platforms-";

	public enum Status { NotStarted, Connecting, Detecting, Done };
	private List<Status> progressBar_l; //progressBars status

	private List<ChronopicRegisterPort> toDiscover_l;

	public enum RacesDevices { ALL, WICHRO, OLDPHOTOCELLS };
	private RacesDevices racesDevicesDetect;

	//devices discovered compatible with current mode
	private static List<ChronopicRegisterPort.Types> discovered_l;
	
	private static List<string> connectError_l;

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
		racesDevicesDetect = RacesDevices.ALL;

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
		LogB.Information("At DiscoverOneMode with mode: " + mode.ToString ());
		discovered_l = new List<ChronopicRegisterPort.Types> ();
		connectError_l = new List<string> (); //cannot connect, maybe device already opened by another program (like Arduino IDE)




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
				} else
					connectError_l.Add (micro.PortName);

				if (! success && mode == Constants.Modes.JUMPSSIMPLE)
				{
					micro.ClosePort ();
					micro.Bauds = 115200;
					LogB.Information("calling discover FourPlatforms from jumps simple mode");
					if (connectAndSleep ())
					{
						flush(); //after connect
						success = discoverFourPlatforms ();
					}
				}
			}
			else if(mode == Constants.Modes.RUNSSIMPLE || mode == Constants.Modes.RUNSINTERVALLIC)
			{
				//if we need to test low speed and high speed, better try first low
				//a 9600 device can get saturated if 115200 connection is done, will need to remove usb

				if (racesDevicesDetect != RacesDevices.WICHRO)
				{
					micro.Bauds = 9600;
					if(connectAndSleep ())
					{
						flush(); //after connect
						LogB.Information("calling discoverMultitest");
						success = discoverMultitest ();
						LogB.Information("ended discoverMultitest");
					} else
						connectError_l.Add (micro.PortName);
				}

				if (racesDevicesDetect != RacesDevices.OLDPHOTOCELLS && ! success)
				{
					if (racesDevicesDetect == RacesDevices.ALL)
						micro.ClosePort ();

					micro.Bauds = 115200;
					if (! micro.PortName.ToLower().Contains("acm"))
					{
						LogB.Information("connectAndSleep again");
						if (connectAndSleep ())
						{
							LogB.Information("calling discoverWichro");
							flush(); //after connect
							success = discoverWichro ();
							LogB.Information("ended discoverWichro");
						} else
							connectError_l.Add (micro.PortName);
					} else // see CreateSerialPort comment on WILIGHT
						connectNotSleep ();
				}
				LogB.Information("success: " + success.ToString());
			}
			else if(mode == Constants.Modes.WILIGHT) //same as WICHRO (but without trying multitest before)
			{
				LogB.Information("At DiscoverOneMode WILIGHT");
				micro.Bauds = 115200;
				if (! micro.PortName.ToLower().Contains("acm"))
				{
					LogB.Information("connectAndSleep again");
					if (connectAndSleep ())
					{
						LogB.Information("calling discoverWichro");
						flush(); //after connect
						success = discoverWichro ();
						LogB.Information("ended discoverWichro");
					} else
						connectError_l.Add (micro.PortName);
				} else {
					//need to CreateSerialPort, because if not the ClosePort below will crash
					// no need now, there is a try/catch there
					connectNotSleep ();
					//yes, because if not it crashes after on a missing list
				}
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
					else if (mode == Constants.Modes.OTHER)
					{
						LogB.Information("calling discover FourPlatforms");
						success = discoverFourPlatforms ();
					}
				} else
					connectError_l.Add (micro.PortName);
			}

			try {
				micro.ClosePort (); //close even connect failed?
			} catch {
				LogB.Information ("Catched closing port at DiscoverOneMode");
			}

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

		if(getVersion ("get_version:", responseExpected_l, false, 2500, true)) // 2000 -> 2500 as seems is better for Every
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

		if(getVersion ("get_version:", responseExpected_l, false, 2000, true))
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

	//WICHRO and WILIGHT
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
				LogB.Information("Discover found this WICHRO (or WILIGHT) device: " + micro.Response);
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

		if(getVersion ("J", responseExpected_l, true, 2000, true))
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

		LogB.Information ("sw.Elapsed.TotalMilliseconds = " + sw.Elapsed.TotalMilliseconds.ToString ());
		if (success)
			LogB.Information ("Discover found this Multitest device: " + micro.Response);
		if (cancel)
			LogB.Information ("Cancelled");

		LogB.Information("done");

		flush(); //empty the port for future use
		return success;
	}

	private bool discoverFourPlatforms ()
	{
		bool success = false;
		List<string> responseExpected_l = new List<string>();
		responseExpected_l.Add(fourPlatformsStr);

		Thread.Sleep(1500); //wait 1500 ms after open to be able to receive commands
		if(cancel)
			return false;

		if(getVersion ("get_version:", responseExpected_l, false, 2000, true))
		{
			LogB.Information("Discover found this 4Platforms device: " + micro.Response);
			if(micro.Response.Contains(fourPlatformsStr))
			{
				micro.Discovered = ChronopicRegisterPort.Types.FOURPLATFORMS;
				success = true;
			}
		}

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

	public List<string> ConnectError_l {
		get { return connectError_l; }
	}

	public RacesDevices RacesDevicesDetect {
		set { racesDevicesDetect = value; }
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
