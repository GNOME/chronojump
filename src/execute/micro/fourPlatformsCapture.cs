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
 * Copyright (C) 2024  Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List<T>
using System.Diagnostics;  //Stopwatch
using System.IO.Ports;
using System.Threading;
using System.Text.RegularExpressions; //Regex

public class FourPlatformsCapture: ArduinoCapture
{
	private List<FourPlatformsEvent> list = new List<FourPlatformsEvent>();
	private int bauds = 115200;
	private string firmwareVersion;
	private string portName;

	//constructor
	public FourPlatformsCapture (string portName)
	{
		this.portName = portName;

		cancel = false;

		if (micro == null || micro.PortName != portName || micro.Bauds != bauds)
			micro = new Micro (portName, bauds);

		Reset ();
	}

	//after a first capture, put variales to zero
	public void Reset ()
	{
		initialize ();
	}

	public override bool CaptureStart()
	{
		LogB.Information("CaptureStart, micro.Opened: " + micro.Opened);
		// 0 connect if needed
		List<string> responseExpected_l = new List<string>();
		if(! micro.Opened)
		{
			responseExpected_l.Add("4Platforms-");

			if(! portConnect (true))
				return false;
			
			Thread.Sleep(1000); //need to sleep 1s more 

			if(! getVersion ("get_version:", responseExpected_l, false, 2000, false))
				return false;

			LogB.Information ("response: |" + micro.Response + "|");
			firmwareVersion = micro.Response;

			Match match = Regex.Match (firmwareVersion, @"4Platforms-(\d+\.\d+)");
			if(match.Groups.Count == 2)
				firmwareVersion = match.Groups[1].ToString();
		}
		micro.Opened = true;

		LogB.Information ("version: |" + firmwareVersion + "|");
		double versionDouble = Convert.ToDouble(Util.ChangeDecimalSeparator(firmwareVersion));

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
		if(! sendCommand ("start_capture:", "Catched 4Platforms capturing"))
		//if(! sendCommand ("start_simulation:", "Catched 4Platforms capturing"))
		{
			return false;
		}

		responseExpected_l = new List<string>();
		responseExpected_l.Add ("Starting capture");
	
		//return waitResponse (responseExpected_l, true, 2000);
		//return waitResponse (responseExpected_l, false, 2000);
		bool responseOk = waitResponse ("Starting capture", false, 2000, false);

		return responseOk;
		*/
		return true;
	}

	//if true: continue capturing; if false: error, end
	public override bool CaptureSample ()
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

		if (str != "")
		{
			FourPlatformsEvent fpe = new FourPlatformsEvent (str);
			list.Add (fpe);
		}

		return true;
	}

	public override bool Stop()
	{
		LogB.Information("AT Capture: STOPPING");

		//empty any pending port read to be able to read correctly the Capture ended message
		flush();

		/*
		if (! sendCommand("end_capture:", "Catched at end_capture:"))
			return false;

		if (waitResponse ("Capture ended", false, 4000, false))
			LogB.Information("AT Capture: STOPPED");
		else
		{
			LogB.Information("AT Capture: cannot stop, going to Disconnect");
			Disconnect ();
		//}
		*/

		LogB.Information("Stop, micro.Opened: " + micro.Opened);

		return true;
	}

	public override bool CanReadFromList ()
	{
		return (list.Count > readedPos);
	}

	public List<FourPlatformsEvent> FourPlatformsCaptureGetList()
	{
		return list;
	}

	public FourPlatformsEvent FourPlatformsCaptureReadNext()
	{
		return list[readedPos++];
	}

	// protected stuff ---->

	protected override void emptyList()
	{
		list = new List<FourPlatformsEvent>();
	}

	public string PortName {
		get { return portName; }
	}
}

public class FourPlatformsEvent
{
	public int Button; //Button -1 means error reading
	public int Time; //always is time since last event. Negative is a button off, Positive on.

	public FourPlatformsEvent (string str)
	{
                str = str.Trim(); 	//Trim str (to remove newline char)
		//LogB.Information ("str: |" + str + "|");

		string [] strFull = str.Split(new char[] {':'});
		if (strFull.Length != 2)
			Button = -1;
		else if (! (Util.IsNumber (strFull[0], false) && Util.IsNumber (strFull[1], false)))
			Button = -1;
		else {
			this.Button = Convert.ToInt32 (strFull[0]);
			this.Time = Convert.ToInt32 (strFull[1]);
		}
	}

	public override string ToString()
	{
		return (string.Format("{0};{1}", Button, Time));
	}
}
