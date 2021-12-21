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
 * Copyright (C) 2017-2020  Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List<T>
using System.Diagnostics;  //Stopwatch
using System.IO.Ports;
using System.Threading;
using Gtk;


public class RFID
{
	public string Captured;

	private bool waitingAdmin;
	private string adminRFID;

	private bool stop;
	//public event EventHandler ChangedEvent; //raised when change RFID vaues
	private SerialPort port;
	private string portName;
	private Gtk.Button fakeButtonChange;
	private Gtk.Button fakeButtonAdminDetected;
	private Gtk.Button fakeButtonReopenDialog;
	private Gtk.Button fakeButtonDisconnected;

	private string lastRFID;

	public RFID(string portName)
	{
		this.portName = portName;
		stop = false;
		fakeButtonChange = new Button();
		fakeButtonAdminDetected = new Button();
		fakeButtonReopenDialog = new Button();
		fakeButtonDisconnected = new Button();

		waitingAdmin = false;
		adminRFID = "";
	}
	
	public void Start()
	{
		/*
		 * don't use getPorts that list all the ports
		 * use chronopicRegister.Rfid portName
		 */
		//List<string> l = getPorts(false);
		
		lastRFID = "";
		string str = "";
		DateTime dtWaitingLastTimeAdminDetected = new DateTime(); //to be able to detect other person after the AdminDetected

		LogB.Information("portName: " + portName);
		port = new SerialPort(portName, 9600); //for the rfid
		port.Open();

		LogB.Information("AT RFID.cs");
		while(! stop)
		{
			Thread.Sleep(200);
			//str = port.ReadLine(); //don't use this because gets waiting some stop signal
			str = "";
			try {
				if (port.BytesToRead > 0)
					str = port.ReadExisting();
			} catch (System.IO.IOException) {
				LogB.Information("Catched reading RFID!");
				fakeButtonDisconnected.Click();
				return;
			}

			if(str == "")
				continue;

			LogB.Information("No trim str" + str);

			//get only the first line and trim it
			if(str.IndexOf(Environment.NewLine) > 0)
				str = str.Substring(0, str.IndexOf(Environment.NewLine)).Trim();

			LogB.Information("Yes one line and trim str" + str);

			//this first line should have a 's' and 'e' (mark of 's'tart and 'e'nd of rfid)
			if( ! (str.IndexOf('s') == 0 && str[str.Length -1] == 'e') )
				continue;

			str = str.Substring(1, str.Length -2); //remove the 's' and 'e'

			if((DateTime.Now - dtWaitingLastTimeAdminDetected).TotalSeconds <= 3)
				continue;

			if(waitingAdmin && adminRFID != "")
			{
				if(str == adminRFID)
				{
					dtWaitingLastTimeAdminDetected = DateTime.Now;

					fakeButtonAdminDetected.Click(); 	//fire special signal

					waitingAdmin = false;
				}
			}
			else
			{
				if(str != lastRFID)
				{
					Captured = str;

					fakeButtonChange.Click(); 		//Firing the event

					lastRFID = str;
				} else
					fakeButtonReopenDialog.Click(); 	//Firing the event
			}
		}
		LogB.Information("AT RFID.cs: STOPPED");
		port.Close();
	}

	public void Stop()
	{
		stop = true;
	}

	public void WaitingAdminStart(string adminRFID)
	{
		waitingAdmin = true;
		this.adminRFID = adminRFID;
	}
	//used when time passed to have waitingAdmin as false. If this is not done, it will not capture other rfids.
	public void WaitingAdminStop()
	{
		waitingAdmin = false;
	}

	//reset lastRFID in order to be able to use that RFID after capture (if same wristband is used again)
	public void ResetLastRFID()
	{
		lastRFID = "";
	}

	private bool findRFIDPort(List<string> l)
	{
		foreach(string p in l)
		{
			LogB.Information("portName: " + p);
			port = new SerialPort(p, 9600); //for the rfid
			port.Open();
			Thread.Sleep(3000); //sleep to let arduino start reading
			
			if (port.BytesToRead > 0){
				LogB.Information("portReading: " + port.ReadExisting());
			}
			//send welcome message
			port.WriteLine("Chronojump RFID");
		
			string str = port.ReadLine();
			LogB.Information("str: " + str);
			if(str.StartsWith("YES Chronojump RFID"))
			{
				LogB.Information("Arduino RFID found on port: " + p);
				return(true);
			}
			else
				LogB.Information("Arduino RFID NOT found on port: " + p);

			port.Close();
		}
		return(false);
	}

	/*
	private List<string> getPorts(bool debug)
	{
		//TODO: move that method here
		List<string> l = new List<string>(ChronopicPorts.GetPorts()); //usb-serial ports

		if(debug)
			foreach(string p in l)
				LogB.Information(string.Format("port: " + p));
		
		return l;
	}
	*/
	
	public Gtk.Button FakeButtonChange
	{
		get { return fakeButtonChange; }
	}

	public Gtk.Button FakeButtonAdminDetected
	{
		get { return fakeButtonAdminDetected; }
	}

	public Gtk.Button FakeButtonReopenDialog
	{
		get { return fakeButtonReopenDialog; }
	}

	public Gtk.Button FakeButtonDisconnected
	{
		get { return fakeButtonDisconnected; }
	}
}
