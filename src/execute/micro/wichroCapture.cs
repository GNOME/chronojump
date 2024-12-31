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
 * Copyright (C) 2022-2024  Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List<T>
using System.Diagnostics;  //Stopwatch
using System.IO.Ports;
using System.Threading;

public class WichroCapture: ArduinoCapture
{
	private string portName;
	private List<WichroEvent> list = new List<WichroEvent>();

	//constructor
	public WichroCapture (string portName)
	{
		this.portName = portName;

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
		LogB.Information ("At wichroCapture, CaptureStart() Micro: " + micro.ToString ());
		LogB.Information("portOpened: " + micro.Opened);
		// 0 connect if needed

		List<string> responseExpected_l = new List<string>();
		responseExpected_l.Add("Wifi-Controller");
		if(! micro.Opened)
		{
			if(! portConnect (true))
				return false;
			if(! getVersion ("local:get_version;", responseExpected_l, false, 2000, true))
				return false;
		} else {
			//we check just to see if device has been disconnected
			if(! getVersion ("local:get_version;", responseExpected_l, false, 2000, true))
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

		//LogB.Information("bucle capture call process line");
		WichroEvent we = new WichroEvent();
		if(! processLine (str, out we))
			return true;

		list.Add(we);
		LogB.Information("bucle capture list added: " + we.ToString());
		return true;
	}

	public override string CaptureEchoLine ()
	{
		string str = "";

		/*
		 * if at CaptureStart device is disconnected,
		 * micro gets closed there and here it shoud not readLine
		 */
		if (! micro.Opened)
		{
			//return false;
			return "";
		}

		if(! readLine (out str))
		{
			micro.ClosePort ();
			//return false;
			return "";
		}

		//LogB.Information("echo readed: " + str);
		//return true;
		return str;
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

	public bool SensorOnce (int terminal)
	{
		//1 is the sensorOnce command
		return sendCommand (string.Format ("{0}:1;", terminal), "Error doing sensorOnce");
	}
	public bool SensorAll (int terminal) //if used sensorOnce, put terminal as before when capture ends
	{
		//256 is the sensorAll command
		return sendCommand (string.Format ("{0}:256;", terminal), "Error doing sensorAll");
	}

	public bool WilightSendCommand (string str)
	{
		return sendCommand (str, "Error sending: " + str);
	}

	public override bool CanReadFromList()
	{
		return (list.Count > readedPos);
	}

	public List<WichroEvent> WichroCaptureGetList()
	{
		return list;
	}

	public WichroEvent WichroCaptureReadNext()
	{
		return list[readedPos++];
	}

	// protected stuff ---->

	protected override void emptyList()
	{
		list = new List<WichroEvent>();
	}

	// private stuff ---->

	/*
	 * Line example: 5;215;O
	 * this event means: At photocell 5, 2015 ms, status is Off
	 * could be O or I
	 */
	private bool processLine (string str, out WichroEvent we)
	{
		//LogB.Information(string.Format("at processLine, str: |{0}|", str));
		we = new WichroEvent();

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

		we = new WichroEvent(Convert.ToInt32(strFull[0]),
				Convert.ToInt32(strFull[1]), strFull[2]);

		return true;
	}

	public string PortName {
		get { return portName; }
	}
}

public class WichroEvent
{
	public int photocell;
	public int timeMs;
	public Chronopic.Plataforma status; // like run with chronopic

	public WichroEvent()
	{
		this.photocell = -1;
		this.timeMs = 0;
		this.status = Chronopic.Plataforma.UNKNOW;
	}

	public WichroEvent(int photocell, int timeMs, string status)
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
