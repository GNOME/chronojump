/*
 * Copyright (C) 2014-2016  Xavier de Blas <xaviblas@gmail.com>
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

/*
 * This class provides all the functionality to connect using FTDI serialNumber
 * this started at 1.6.3 version
 */

using System.Collections.Generic; //List<T>
using System.Diagnostics; 	//for detect OS and for Process

public class ChronopicRegisterPort
{
	public string Port;
	public bool FTDI;
	public string SerialNumber;
	public enum Types { UNKNOWN, CONTACTS, ENCODER }
	public Types Type;

	//constructor when port is known (searching FTDI stuff on a serial port)
	public ChronopicRegisterPort (string port)
	{
		this.Port = port;
		this.FTDI = false;
		this.SerialNumber = "";
		this.Type = Types.UNKNOWN;
	}

	//constructor used on SqliteChronopicRegister.SelectAll()
	public ChronopicRegisterPort (string serialNumber, Types type)
	{
		this.Port = "";
		this.FTDI = true;
		this.SerialNumber = serialNumber;
		this.Type = type;
	}

	public override string ToString()
	{
		return "Port: " + Port + " ; FTDI: " + FTDI.ToString() +
			" ; SerialNumber: " + SerialNumber + " ; Type: " + Type.ToString();
	}
}

public class ChronopicRegisterPortList
{
	public List<ChronopicRegisterPort> L;

	//constructor
	public ChronopicRegisterPortList ()
	{
		this.L = SqliteChronopicRegister.SelectAll(false);
	}

	public void Print()
	{
		LogB.Information("Printing ChronopicRegisterPortList... ");
		foreach(ChronopicRegisterPort crp in L)
			LogB.Information(crp.ToString());

		LogB.Information("... Done!");
	}

	public bool Exists(ChronopicRegisterPort crp)
	{
		foreach(ChronopicRegisterPort c in L)
			if(c.SerialNumber == crp.SerialNumber)
				return true;

		return false;
	}

	public void Add (ChronopicRegisterPort crp)
	{
		//Add to SQL
		SqliteChronopicRegister.Insert(false, crp);

		//Add to list
		L.Add(crp);
	}

	public void Update (ChronopicRegisterPort crp, ChronopicRegisterPort.Types newType)
	{
		//Update SQL
		SqliteChronopicRegister.Update(false, crp, newType);

		//Update list
		foreach(ChronopicRegisterPort c in L) {
			if(c.SerialNumber == crp.SerialNumber) {
				c.Type = newType;
				break;
			}
		}
	}

	public void Delete (ChronopicRegisterPort crp)
	{
		//Delete from SQL
		SqliteChronopicRegister.Delete(false, crp);

		//Delete from list
		L.Remove(crp);
	}

}


public class ChronopicRegister 
{
	public ChronopicRegister () 
	{
		//1 print the registered ports on SQL
		ChronopicRegisterPortList crpl = new ChronopicRegisterPortList();
		crpl.Print();

		List<string> ports = getPorts(true);
		foreach(string p in ports) {
			LogB.Information(string.Format("ChronopicRegister for port: " + p));
			ChronopicRegisterPort crp = readFTDI(p);
			LogB.Information(crp.ToString());

			//2 add to registered list (add also on database)
			if(crp.FTDI && ! crpl.Exists(crp))
				crpl.Add(crp);
		}

		//1 print the registered ports on SQL
		crpl = new ChronopicRegisterPortList();
		crpl.Print();
	}

	private List<string> getPorts(bool debug) 
	{
		//TODO: move that method here
		List<string> l = new List<string>(ChronopicPorts.GetPorts());

		if(debug)
			foreach(string p in l)
				LogB.Information(string.Format("port: " + p));

		return l;
	}

	//read all information of one port
	private ChronopicRegisterPort readFTDI(string port) 
	{
		//if(UtilAll.GetOSEnum() == UtilAll.OperatingSystems.LINUX)
			return readFTDILinux(port);
		//else if(UtilAll.GetOSEnum() == UtilAll.OperatingSystems.MACOSX)
		//else // WINDOWS
	}
		
	private ChronopicRegisterPort readFTDILinux(string port) 
	{
		ChronopicRegisterPort crp = new ChronopicRegisterPort(port);

		/*
		 * old:
		 * /bin/udevadm info —name=/dev/ttyUSB0 |grep ID_SERIAL_SHORT|cut -d= -f2
		 * error on some systems:
		 * Unknown device, --name=, --path=, or absolute path in /dev/ or /sys expected.
		 */
		//new: udevadm info -p $(udevadm info -q path -n /dev/ttyUSB0)
		//TODO: find a way to call this in only one process

		ProcessStartInfo pinfo = new ProcessStartInfo();
		string pBin = "udevadm";
		pinfo.FileName=pBin;
		
		pinfo.CreateNoWindow = true;
		pinfo.UseShellExecute = false;
		pinfo.RedirectStandardError = true;
		pinfo.RedirectStandardOutput = true; 
	
		//1) get path	
		pinfo.Arguments = "info -q path -n " + port;
		LogB.Information("Arguments:", pinfo.Arguments);
		
		Process p = new Process();
		p.StartInfo = pinfo;
		p.Start();

		string path = p.StandardOutput.ReadToEnd();
		LogB.Information(path);
		string error = p.StandardError.ReadToEnd();
		
		p.WaitForExit();

		if(error != "") {
			LogB.Error(error);
			return crp;
		}
		
		//2) read FTDI info	
		pinfo.Arguments = "info -p " + path;
		LogB.Information("Arguments:", pinfo.Arguments);
		
		p = new Process();
		p.StartInfo = pinfo;
		p.Start();

		error = p.StandardError.ReadToEnd();

		while (! p.StandardOutput.EndOfStream)
		{
			string lineOut = p.StandardOutput.ReadLine();
			if (lineOut.Contains("ID_VENDOR=")) {
				string [] strFull = lineOut.Split(new char[] {'='});
				crp.FTDI = (strFull[1] == "FTDI");
			} else if (lineOut.Contains("ID_SERIAL_SHORT=")) {
				string [] strFull = lineOut.Split(new char[] {'='});
				crp.SerialNumber = strFull[1];
			}
		}
		p.WaitForExit();
		
		if(error != "")
			LogB.Error(error);
		
		return crp;
	}
}

