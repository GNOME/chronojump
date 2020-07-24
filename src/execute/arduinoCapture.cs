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
	protected int readedPos; //position already readed from list

	// public stuff ---->

	public abstract bool CaptureStart();
	public abstract bool CaptureLine();
	public abstract bool Stop();
	public abstract bool CanRead();

	//have methods for get objects on each of the derived classes
	public abstract List<PhotocellWirelessEvent> PhotocellWirelssCaptureGetList();
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
				return false;
			}
			//throw;
		}
		return true;
	}

	protected bool readLine (out string str)
	{
		str = "";
		try {
			if (port.BytesToRead > 0)
			{
				str = port.ReadLine();
			}
		} catch (System.IO.IOException)
		{
			LogB.Information("Catched reading!");
			return false;
		}
		return true;
	}

	protected abstract void emptyList();
}

public class PhotocellWirelessCapture: ArduinoCapture
{
	private List<PhotocellWirelessEvent> list = new List<PhotocellWirelessEvent>();

	public PhotocellWirelessCapture (string portName)
	{
		this.bauds = 115200;
		initialize(portName, bauds);
	}

	public override bool CaptureStart()
	{
		LogB.Information(string.Format("arduinoCapture portName: {0}, bauds: {1}", portName, bauds));
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

		Thread.Sleep(3000); //sleep to let arduino start reading serial event

		if (! sendCommand("start_capture:", "Catched at start_capture:"))
			return false;

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

		string str = "";
		//empty any pending port read to be able to read correctly the Capture ended message
		if (port.BytesToRead > 0)
		{
			str = port.ReadExisting();
			LogB.Information("At Stop, readed: " + str);
		}

		if (! sendCommand("end_capture:", "Catched at end_capture:"))
			return false;

		do {
			Thread.Sleep(25);
			if (port.BytesToRead > 0)
			{
				try {
					str = port.ReadLine();
				} catch {
					LogB.Information("Catched waiting end_capture feedback");
				}
			}
			//LogB.Information("waiting \"Capture ended\" string: " + str);
		}
		while(! str.Contains("Capture ended"));

		LogB.Information("AT Capture: STOPPED");

		port.Close();

		return true;
	}

	public override bool CanRead()
	{
		return (list.Count > readedPos);
	}

	public override List<PhotocellWirelessEvent> PhotocellWirelssCaptureGetList()
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

		//LogB.Information("No trim str" + str);

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
				(strFull[2] != "O" && strFull[2] != "I")
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
		if(status == "O")
			this.status = Chronopic.Plataforma.OFF;
		else //(status == "I")
			this.status = Chronopic.Plataforma.ON;
	}

	public override string ToString()
	{
		return (string.Format("{0};{1};{2}", photocell, timeMs, status));
	}
}
