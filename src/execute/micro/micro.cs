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

	// just to debug
	public int NumBytesToRead ()
	{
		return (port.BytesToRead);
	}

	public bool BytesToReadAtLeast (int n)
	{
		return (port.BytesToRead >= n);
		/*
		int toRead = port.BytesToRead;
		LogB.Information ("port.BytesToRead: " + toRead.ToString ());
		return (toRead >= n);
		*/
	}

	//used on Chronopic
	public int ReadByte ()
	{
		return (port.ReadByte ());
	}

	byte[] buffer; // var ?
	public void BufferInit ()
	{
		buffer = new byte[1024];
		//buffer = new byte[8192];
		//buffer = new byte[32768];
		//buffer = new byte[12];
	}
	public int ReadWithBuffer (int start, int end)
	{
		if (end == -1)
			return port.Read (buffer, 0, buffer.Length);
		else
			return port.Read (buffer, 0, end);
	}
	public int GetBufferAtPos (int pos)
	{
		return buffer[pos];
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
		get { return bauds; }
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
		//catch (System.IO.IOException)
		catch (Exception ex)
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

	//returning false could mean a error on sending command or not received the expected response
	//waitResponseReadExisting is the the default method, but on encoderPTL we need to ReadLine
	protected bool getVersion (string getVersionStr, List<string> responseExpected_l,
			bool cleanAllZeros, int waitResponseTime, bool waitResponseReadExisting
			)
	{
		if(! sendCommand (getVersionStr, "error getting version")) //note this is for Wichro
			return false;

		return waitResponse (responseExpected_l, cleanAllZeros, waitResponseTime, waitResponseReadExisting);
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

			success = waitResponse (responseExpected_l, cleanAllZeros, waitLimitMs, true);
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
			} else {
				LogB.Information("error: " + errorMessage);
				LogB.Information("not managed exception");
				return false;
			}
			//throw;
		}
		LogB.Information("success sending command");
		return true;
	}

	//cleanAllZeros is used for encoder
	//this call when there is only one expected string
	protected bool waitResponse (string responseExpected, bool cleanAllZeros, int waitResponseMs, bool waitResponseReadExisting)
	{
		List<string> responseExpected_l = new List<string> ();
		responseExpected_l.Add (responseExpected);
	
		return waitResponse (responseExpected_l, cleanAllZeros, waitResponseMs, waitResponseReadExisting);
	}

	protected bool waitResponse (List<string> responseExpected_l, bool cleanAllZeros, int waitResponseMs, bool waitResponseReadExisting)
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
					string received = "";
					if (waitResponseReadExisting)
						received = micro.ReadExisting();
					else
						received = micro.ReadLine();

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
		LogB.Information ("checking if micro opened on flush");
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
