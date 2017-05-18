/*
 * Copyright (C) 2017  Xavier de Blas <xaviblas@gmail.com>
 *
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
 */

using System;
using System.Collections.Generic; //List<T>
using System.IO.Ports;
using System.Threading;
using Gtk;


public class RFID
{
	public string Captured;
	private bool stop;
	//public event EventHandler ChangedEvent; //raised when change RFID vaues
	private SerialPort port;
	private string portName;
	private Gtk.Button fakeButtonChange;

	public RFID(string portName)
	{
		this.portName = portName;
		stop = false;
		fakeButtonChange = new Button();
	}
	
	public void Start()
	{
		/*
		 * don't use getPorts that list all the ports
		 * use chronopicRegister.Rfid portName
		 */
		//List<string> l = getPorts(false);
		//LogB.Information("getPorts");
		List<string> l = new List<string>();
		l.Add(portName);
		
		string lastRFID = "";
		string str = "";
		if(findRFIDPort(l))
		{
			//don't need to open port because it's still opened
			//port.Open();
			while(! stop)
			{
				LogB.Information("AT RFID.cs");
				//str = port.ReadLine(); //don't use this because gets waiting some stop signal
				if (port.BytesToRead > 0)
				{
					str = port.ReadExisting();
					LogB.Information("No trim str" + str);

					//get only the first line and trim it
					if(str.IndexOf(Environment.NewLine) > 0)
						str = str.Substring(0, str.IndexOf(Environment.NewLine)).Trim();
					
					LogB.Information("Yes one line and trim str" + str);
	
					//this first line should have a ';' (mark of end of rfid)	
					if(str.IndexOf(";") > 0)
					{
						str = str.Substring(0, str.IndexOf(";"));

						if(str != lastRFID)
						{
							Captured = str;

							//Firing the event
							fakeButtonChange.Click();
							/*
							   EventHandler handler = ChangedEvent;
							   if (handler != null)
							   handler(this, new EventArgs());
							   */
							lastRFID = str;
						}
					}
				}
				Thread.Sleep(100);
			}
			LogB.Information("AT RFID.cs: STOPPED");
			port.Close();
		}
	}

	public void Stop()
	{
		stop = true;
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

	private List<string> getPorts(bool debug)
	{
		//TODO: move that method here
		List<string> l = new List<string>(ChronopicPorts.GetPorts()); //usb-serial ports

		if(debug)
			foreach(string p in l)
				LogB.Information(string.Format("port: " + p));
		
		return l;
	}
	
	public Gtk.Button FakeButtonChange
	{
		get { return fakeButtonChange; }
	}
}
