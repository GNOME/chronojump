/*
 * Copyright (C) 2016-2017  Xavier de Blas <xaviblas@gmail.com>
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
 * this started at 1.7.0 version
 */

using System;
using System.Collections.Generic; //List<T>
using System.Diagnostics; 	//for detect OS and for Process
using System.IO.Ports;
using FTD2XX_NET;
using Mono.Unix;


public class ChronopicRegisterPort
{
	public string Port;
	public bool FTDI;
	public string SerialNumber;
	public enum Types { UNKNOWN, CONTACTS, ENCODER, ARDUINO_RFID, ARDUINO_FORCE, ARDUINO_RUN_ENCODER, ACCELEROMETER, RUN_WIRELESS }
	public Types Type;

	public bool ConnectedReal; 	//if connexion has been done by ChronopicInit.Do

	//constructor when port is known (searching FTDI stuff on a serial port)
	public ChronopicRegisterPort (string port)
	{
		this.Port = port;
		this.FTDI = false;
		this.SerialNumber = "";
		this.Type = Types.UNKNOWN;
		ConnectedReal = false;
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

	public static string TypePrint(Types typeStatic)
	{
		if(typeStatic == Types.UNKNOWN)
			//return Catalog.GetString("Not configured");
			return Catalog.GetString("Click on right arrow!");
		else if(typeStatic == Types.CONTACTS)
			return Catalog.GetString("Jumps/Races");
		else if(typeStatic == Types.ENCODER)
			return Catalog.GetString("Encoder");
		else if(typeStatic == Types.ARDUINO_RFID)
			return "RFID";
		else if(typeStatic == Types.ARDUINO_FORCE)
			return Catalog.GetString("Force sensor");
		else if(typeStatic == Types.ARDUINO_RUN_ENCODER)
			return "Race encoder";
		else if(typeStatic == Types.ACCELEROMETER)
			return "Accelerometer";
		else if(typeStatic == Types.RUN_WIRELESS)
			return "Races (wireless)";

		return Catalog.GetString("Unknown");
	}
}

public class ChronopicRegisterPortList
{
	public List<ChronopicRegisterPort> L;

	//constructor
	public ChronopicRegisterPortList (bool showArduinoRFID, bool showRunWireless)
	{
		this.L = SqliteChronopicRegister.SelectAll(false, showArduinoRFID, showRunWireless);
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

	//only call this if Exists
	public bool PortChanged(ChronopicRegisterPort crp)
	{
		foreach(ChronopicRegisterPort c in L) {
			if(c.SerialNumber == crp.SerialNumber) {
				if(c.Port == crp.Port)
					return false;

				return true;
			}
		}

		return true;
	}

	public void UpdateType (ChronopicRegisterPort crp, ChronopicRegisterPort.Types newType)
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
	public void UpdatePort (ChronopicRegisterPort crp, string newPort)
	{
		foreach(ChronopicRegisterPort c in L) {
			if(c.SerialNumber == crp.SerialNumber) {
				c.Port = newPort;
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

public class ChronopicRegisterSelectOS
{
	public ChronopicRegisterSelectOS() {
	}

	public ChronopicRegister Do(bool compujump, bool showRunWireless)
	{
		if(UtilAll.GetOSEnum() == UtilAll.OperatingSystems.LINUX)
			return new ChronopicRegisterLinux(compujump, showRunWireless);
		else if(UtilAll.GetOSEnum() == UtilAll.OperatingSystems.MACOSX)
			return new ChronopicRegisterMac(compujump, showRunWireless);
		else // WINDOWS
			return new ChronopicRegisterWindows(compujump, showRunWireless);
	}
}

public abstract class ChronopicRegister
{
	protected ChronopicRegisterPortList crpl;

	protected void process(bool compujump, bool showRunWireless)
	{
		//1 print the registered ports on SQL
		crpl = new ChronopicRegisterPortList(compujump, showRunWireless); //compujump means: showArduinoRFID
		crpl.Print();

		//2 create list
		createList();

		//3 print the registered ports on SQL (debug)
		crpl.Print();
	}

	//used on Linux and Mac
	protected virtual void createList()
	{
		List<string> ports = getPorts(true);
		foreach(string p in ports)
		{
			LogB.Information(string.Format("ChronopicRegister for port: " + p));

			ChronopicRegisterPort crp = new ChronopicRegisterPort(p);
			crp = readFTDI(crp);

			LogB.Information(crp.ToString());

			//2 add/update registered list
			registerAddOrUpdate(crp);
		}
	}

	//used on Linux and Mac
	protected List<string> getPorts(bool debug)
	{
		//TODO: move that method here
		List<string> l = new List<string>(ChronopicPorts.GetPorts());

		if(debug)
			foreach(string p in l)
				LogB.Information(string.Format("port: " + p));

		return l;
	}
	
	protected void registerAddOrUpdate(ChronopicRegisterPort crp)
	{
		if(! crp.FTDI)
			return;

		if (! crpl.Exists(crp))
			crpl.Add(crp);
		else if(crpl.PortChanged(crp))
			crpl.UpdatePort(crp, crp.Port);
	}

	//unused
	protected virtual ChronopicRegisterPort readFTDI(ChronopicRegisterPort crp)
	{
		return crp;
	}

	public int NumConnectedOfType(ChronopicRegisterPort.Types type)
	{
		int count = 0;
		foreach(ChronopicRegisterPort crp in crpl.L)
			if(crp.Type == type && crp.Port != "")
				count ++;

		return count;
	}

	//returns first found (should be only one if called NumConnectedOfType and returned value was 1
	public ChronopicRegisterPort ConnectedOfType(ChronopicRegisterPort.Types type)
	{
		foreach (ChronopicRegisterPort crp in crpl.L)
		{
			if (crp.Type == type && crp.Port != "")
			{
				return crp;
			}
		}

		return null;
	}

	//multichronopic
	public List<ChronopicRegisterPort> GetTwoContactsConnected()
	{
		//create a list for two contacts crpl
		List<ChronopicRegisterPort> l = new List<ChronopicRegisterPort>();
		int count = 0;

		foreach (ChronopicRegisterPort crp in crpl.L)
		{
			if (crp.Type == ChronopicRegisterPort.Types.CONTACTS && crp.Port != "")
			{
				l.Add(crp);
				count ++;
			}
		}

		if(count == 2)
			return l;
		else
			return null;
	}

	public bool UnknownFound()
	{
		if(NumConnectedOfType(ChronopicRegisterPort.Types.UNKNOWN) > 0)
			return true;

		return false;
	}

	public string GetRfidPortName()
	{
		foreach (ChronopicRegisterPort crp in crpl.L)
		{
			if (crp.Type == ChronopicRegisterPort.Types.ARDUINO_RFID && crp.Port != "")
				return crp.Port;
		}

		return "";
	}

	public ChronopicRegisterPortList Crpl
	{
		get { return crpl; }
	}
}

public class ChronopicRegisterLinux : ChronopicRegister
{
	public ChronopicRegisterLinux (bool compujump, bool showRunWireless)
	{
		process(compujump, showRunWireless);
	}

	protected override ChronopicRegisterPort readFTDI(ChronopicRegisterPort crp) 
	{
		/*
		 * old:
		 * /bin/udevadm info —name=/dev/ttyUSB0 |grep ID_SERIAL_SHORT|cut -d= -f2
		 * error on some systems:
		 * Unknown device, --name=, --path=, or absolute path in /dev/ or /sys expected.
		 */
		//new: udevadm info -p $(udevadm info -q path -n /dev/ttyUSB0)
		//TODO: find a way to call this in only one process

		//1) get path
		List<string> parameters = new List<string> { "info", "-q", "path", "-n", crp.Port };
		ExecuteProcess.Result result = ExecuteProcess.runShowErrorIfNotStarted ("udevadm", parameters);
		string path = result.stdout;

		if (result.stderr != "") {
			return crp;
		}

		//2) read FTDI info	
		parameters = new List<string> { "info", "-p", path };
		result = ExecuteProcess.runShowErrorIfNotStarted ("udevadm", parameters);

		if (result.stderr != "") {
			return crp;
		}

		foreach (string lineOut in result.stdout.Split('\n'))
		{
			if (lineOut.Contains("ID_VENDOR=")) {
				string [] strFull = lineOut.Split(new char[] {'='});
				crp.FTDI = (strFull[1] == "FTDI");
			} else if (lineOut.Contains("ID_SERIAL_SHORT=")) {
				string [] strFull = lineOut.Split(new char[] {'='});
				crp.SerialNumber = strFull[1];
			}
		}
		return crp;
	}
}


public class ChronopicRegisterMac : ChronopicRegister
{
	public ChronopicRegisterMac (bool compujump, bool showRunWireless)
	{
		process(compujump, showRunWireless);
	}

	protected override ChronopicRegisterPort readFTDI(ChronopicRegisterPort crp)
	{
		//TODO: 1) check if it's FTDI
		crp.FTDI = true;

		//2) read SerialNumber
		//eg crp.Port = "/dev/tty.usbserialA123456F";
		string chunk = "usbserial";
		int pos = crp.Port.LastIndexOf(chunk) + chunk.Length;
		crp.SerialNumber = crp.Port.Substring(pos); //eg. A123456F

		return crp;
	}
}

public class ChronopicRegisterWindows : ChronopicRegister
{
	FTDI ftdiDeviceWin;

	public ChronopicRegisterWindows (bool compujump, bool showRunWireless)
	{
		process(compujump, showRunWireless);
	}

	protected override void createList()
	{
		// Create new instance of the FTDI device class
		//TODO: check that is created only once?
		ftdiDeviceWin = new FTDI();

		uint numDevices = getFTDIdevicesWindows();
		if(numDevices > 0)
			createListDo(numDevices);
	}

	private uint getFTDIdevicesWindows()
	{
		//based on: http://www.ftdichip.com/Support/SoftwareExamples/CodeExamples/CSharp/EEPROM.zip

		//UInt32 ftdiDeviceCount = 0;
		uint ftdiDeviceCount = 0;
		FTDI.FT_STATUS ftStatus = FTDI.FT_STATUS.FT_OK;

		// Determine the number of FTDI devices connected to the machine
		ftStatus = ftdiDeviceWin.GetNumberOfDevices(ref ftdiDeviceCount);
		// Check status
		if (ftStatus != FTDI.FT_STATUS.FT_OK) {
			LogB.Error("FTDI GetNumberOfDevices failed");
			return 0;
		}
		if (ftdiDeviceCount == 0) {
			LogB.Information("FTDI GetNumberOfDevices 0");
			return 0;
		}

		return ftdiDeviceCount;
	}

	private void createListDo(uint ftdiDeviceCount)
	{
		FTDI.FT_STATUS ftStatus = FTDI.FT_STATUS.FT_OK;

		// Allocate storage for device info list
		FTDI.FT_DEVICE_INFO_NODE[] ftdiDeviceList = new FTDI.FT_DEVICE_INFO_NODE[ftdiDeviceCount];

		// Populate our device list
		ftStatus = ftdiDeviceWin.GetDeviceList(ftdiDeviceList);

		if (ftStatus == FTDI.FT_STATUS.FT_OK)
		{
			for (uint i = 0; i < ftdiDeviceCount; i++)
			{
				LogB.Information(String.Format("Device Index: " + i.ToString()));
				LogB.Information(String.Format("Flags: " + String.Format("{0:x}", ftdiDeviceList[i].Flags)));
				LogB.Information(String.Format("Type: " + ftdiDeviceList[i].Type.ToString()));
				LogB.Information(String.Format("ID: " + String.Format("{0:x}", ftdiDeviceList[i].ID)));
				LogB.Information(String.Format("Location ID: " + String.Format("{0:x}", ftdiDeviceList[i].LocId)));
				LogB.Information(String.Format("Serial Number: " + ftdiDeviceList[i].SerialNumber.ToString()));
				LogB.Information(String.Format("Description: " + ftdiDeviceList[i].Description.ToString()));

				string port = getComPort(ftdiDeviceList[i]);
				ChronopicRegisterPort crp = new ChronopicRegisterPort(port);
				crp.FTDI = true;
				crp.SerialNumber = ftdiDeviceList[i].SerialNumber.ToString();
				crp.Type = ChronopicRegisterPort.Types.UNKNOWN;
				
				LogB.Information(string.Format("crp: " + crp.ToString()));

				registerAddOrUpdate(crp);
			}
		}
	}

	private string getComPort(FTDI.FT_DEVICE_INFO_NODE node)
	{
		string comport = "";
		//http://stackoverflow.com/questions/2279646/finding-usb-serial-ports-from-a-net-application-under-windows-7
		if (ftdiDeviceWin.OpenByLocation(node.LocId) == FTDI.FT_STATUS.FT_OK)
		{
			try {
				ftdiDeviceWin.GetCOMPort(out comport);
			}
			finally {
				ftdiDeviceWin.Close();
			}
		}

		return comport;
	}
}
