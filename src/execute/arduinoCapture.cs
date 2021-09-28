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
 * Copyright (C) 2020  Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List<T>
using System.Diagnostics;  //Stopwatch
using System.IO.Ports;
using System.Threading;

//inspired on RFID.cs (unique class that reads arduino separated of gui

public abstract class ArduinoCapture
{
	protected string portName;
	protected int bauds;
	protected SerialPort port;
	protected bool portOpened;
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

		emptyList();
	}

	protected bool portConnect()
	{
		port = new SerialPort(portName, bauds);

		try {
			port.Open();
		}
		catch (System.IO.IOException)
		{
			LogB.Information("Error: could not open port");
			return false;
		}

		LogB.Information("port successfully opened");

		//TODO: Val, caldria que quedés clar a la interficie que estem esperant aquest temps, a veure com ho fa el sensor de força, ...
		//just print on gui somthing like "please, wait, ..."
		//
		Thread.Sleep(3000); //sleep to let arduino start reading serial event

		if(! sendCommand("local:get_version;", "error getting version"))
			return false;
		waitResponse("Wifi-Controller");

		return true;
	}

	protected bool sendCommand (string command, string errorMessage)
	{
		try {
			LogB.Information("arduinocapture sendCommand: |" + command + "|");
			port.WriteLine(command);
		}
		catch (Exception ex)
		{
			if(ex is System.IO.IOException || ex is System.TimeoutException)
			{
				LogB.Information("error: " + errorMessage);
				port.Close();
				portOpened = false;
				return false;
			}
			//throw;
		}
		return true;
	}

	protected void waitResponse (string expected)
	{
		string str = "";
		do {
			Thread.Sleep(25);
			if (port.BytesToRead > 0)
			{
				try {
					str = port.ReadLine();
				} catch {
					LogB.Information(string.Format("Catched waiting: |{0}|", expected));
				}
				//LogB.Information(string.Format("waiting \"{0}\", received: {1}", expected, str));
			}
		}
		while(! str.Contains(expected));
		LogB.Information("waitResponse success: " + str);
	}

	protected bool readLine (out string str)
	{
		str = "";
		try {
			if (port.BytesToRead > 0)
			{
				str = port.ReadLine();
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
		port.Close();
		portOpened = false;
	}

	public bool PortOpened
	{
		get { return portOpened; }
	}
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
		LogB.Information("portOpened: " + portOpened.ToString());
		// 0 connect if needed
		if(! portOpened)
			if(! portConnect())
				return false;

		portOpened = true;

		LogB.Information(string.Format("arduinoCapture portName: {0}, bauds: {1}", portName, bauds));

		//empty the port after new capture
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

	private void flush ()
	{
		string str = "";
		if (port.BytesToRead > 0)
			str = port.ReadExisting();

		LogB.Information(string.Format("flushed: |{0}|", str));
	}

	public override bool Stop()
	{
		LogB.Information("AT Capture: STOPPING");

		string str = "";
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
		port.Close();
		portOpened = false;
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
