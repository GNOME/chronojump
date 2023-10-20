/*
 * Copyright (C) 2016-2023  Xavier de Blas <xaviblas@gmail.com>
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

	//Note: if this changes, change also on execute/arduinoCapture.cs
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
			return "WICHRO";

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

	public void Add (ChronopicRegisterPort crp, bool insertSQL)
	{
		//Add to SQL
		if (insertSQL)
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

	public ChronopicRegister Do (bool compujump, bool showRunWireless, bool FTDIalways)
	{
		if(UtilAll.GetOSEnum() == UtilAll.OperatingSystems.LINUX)
			return new ChronopicRegisterLinux (compujump, showRunWireless, FTDIalways);
		else if(UtilAll.GetOSEnum() == UtilAll.OperatingSystems.MACOSX)
			return new ChronopicRegisterMac (compujump, showRunWireless);
		else // WINDOWS
			return new ChronopicRegisterWindows (compujump, showRunWireless);
	}
}

public abstract class ChronopicRegister
{
	protected ChronopicRegisterPortList crpl;
	public static string SerialNumberNotUnique = "A50285BI"; //A FTDI sadly not unique

	private bool compujump;

	protected void process (bool compujump, bool showRunWireless)
	{
		this.compujump = compujump;

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

		/*
		   special case for the massively repeated A50285BI
		   on compujump there is no problem as there will be rfid (maybe bad number),
		   but the rest of the devices (right now: contact platform, photocells or encoder) all Chronopic (ftdi ok).
		   Also special case for devices without SerialNumber, eg on Chromebook udevadm does not return the Serial id, so is returned as ""
		   */
		if ( (crp.SerialNumber == SerialNumberNotUnique || crp.SerialNumber == "") && ! compujump)
		{
			crpl.Add (crp, false); //only add to the current list
			return;
		}

		if (! crpl.Exists (crp))
		{
			//if is unknown just add to the viewing list. Else add also to the sql
			if (crp.Type == ChronopicRegisterPort.Types.UNKNOWN)
				crpl.Add (crp, false);
			else
				crpl.Add (crp, true);
		} else if(crpl.PortChanged (crp))
			crpl.UpdatePort (crp, crp.Port);
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

	public void SetType (string serialNumber, ChronopicRegisterPort.Types type)
	{
		for (int i = 0; i < crpl.L.Count; i ++)
			if (crpl.L[i].SerialNumber == serialNumber)
				crpl.L[i].Type = type;
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

	/* ---- selectedForMode ---->

	   This helps to solve if there is more than one for a mode.
	   In the future will handle also n forceSensors
	   */

	private List <selectedForMode> selectedForMode_l;
	public struct selectedForMode
	{
		public ChronopicRegisterPort crp;
		public Constants.Modes mode;

		public selectedForMode (ChronopicRegisterPort crp, Constants.Modes mode)
		{
			this.crp = crp;
			this.mode = mode;
		}

		public override string ToString () 	//debug
		{
			return string.Format ("crp: {0}; mode: {1}", crp, mode);
		}
	}

	public void SetSelectedForMode (ChronopicRegisterPort crp, Constants.Modes mode)
	{
		if (selectedForMode_l == null)
			selectedForMode_l = new List<selectedForMode> ();

		for (int i = 0; i < selectedForMode_l.Count ; i ++)
			if (selectedForMode_l[i].mode == mode)
			{
				//note structs cannot be changed, so change by a new one
				selectedForMode_l[i] = new selectedForMode (crp, mode);
				return;
			}

		//not found, add it
		selectedForMode_l.Add (new selectedForMode (crp, mode));
	}

	// in order to the device available without needing to click to device button
	public bool SetAnyCompatibleConnectedAsSelected (Constants.Modes mode)
	{
		if (mode == Constants.Modes.JUMPSSIMPLE || mode == Constants.Modes.JUMPSREACTIVE)
		{
			if (setAnyCompatibleConnectedAsSelectedDo (mode, ChronopicRegisterPort.Types.CONTACTS))
				return true;
		}
		else if (mode == Constants.Modes.RUNSSIMPLE || mode == Constants.Modes.RUNSINTERVALLIC)
		{
			if (setAnyCompatibleConnectedAsSelectedDo (mode, ChronopicRegisterPort.Types.CONTACTS))
				return true;
			//with the repeated FTDI number it will not work until selected at each Chronojump boot
			else if (setAnyCompatibleConnectedAsSelectedDo (mode, ChronopicRegisterPort.Types.RUN_WIRELESS))
				return true;
		}
		else if (Constants.ModeIsENCODER (mode))
		{
			if (setAnyCompatibleConnectedAsSelectedDo (mode, ChronopicRegisterPort.Types.ENCODER))
				return true;
		}
		//with the repeated FTDI number it will not work until selected at each Chronojump boot
		else if (Constants.ModeIsFORCESENSOR (mode))
		{
			if (setAnyCompatibleConnectedAsSelectedDo (mode, ChronopicRegisterPort.Types.ARDUINO_FORCE))
				return true;
		}
		//with the repeated FTDI number it will not work until selected at each Chronojump boot
		else if (mode == Constants.Modes.RUNSENCODER)
		{
			if (setAnyCompatibleConnectedAsSelectedDo (mode, ChronopicRegisterPort.Types.ARDUINO_RUN_ENCODER))
				return true;
		}

		return false;
	}
	private bool setAnyCompatibleConnectedAsSelectedDo (Constants.Modes mode, ChronopicRegisterPort.Types type)
	{
		foreach(ChronopicRegisterPort crp in crpl.L)
			if(crp.Type == type && crp.Port != "")
			{
				SetSelectedForMode (crp, mode);
				return true;
			}

		return false;
	}

	public ChronopicRegisterPort GetSelectedForMode (Constants.Modes mode)
	{
		if (selectedForMode_l == null)
			return new ChronopicRegisterPort ("");

		//first search strict to that mode, so if there is a chronopic for jumps and another for races, will select the appropriate
		foreach (selectedForMode sfm in selectedForMode_l)
			if (sfm.mode == mode)
				return sfm.crp;

		//if not found for that mode, find for equivalent mode, so a chronopic for jumps can be suitable for races
		foreach (selectedForMode sfm in selectedForMode_l)
		{
			if (
					(mode == Constants.Modes.JUMPSSIMPLE || mode == Constants.Modes.JUMPSREACTIVE ||
					 mode == Constants.Modes.RUNSSIMPLE || mode == Constants.Modes.RUNSINTERVALLIC)
					&&
					(sfm.mode == Constants.Modes.JUMPSSIMPLE || sfm.mode == Constants.Modes.JUMPSREACTIVE ||
					 sfm.mode == Constants.Modes.RUNSSIMPLE || sfm.mode == Constants.Modes.RUNSINTERVALLIC) )
				return sfm.crp;
			else if (Constants.ModeIsFORCESENSOR (mode) && Constants.ModeIsFORCESENSOR (sfm.mode))
				return sfm.crp;
			else if (Constants.ModeIsENCODER (mode) && Constants.ModeIsENCODER (sfm.mode))
				return sfm.crp;
		}

		return new ChronopicRegisterPort ("");
	}

	//to debug
	public void ListSelectedForAllModes ()
	{
		LogB.Information ("at ListSelectedForAllModes:");
		if (selectedForMode_l == null)
			return;

		foreach (selectedForMode sfm in selectedForMode_l)
			LogB.Information (sfm.ToString ());
	}

	// <---- end of selectedForMode ----


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

	/*
	 * unused
	 *
	public bool UnknownFound()
	{
		if(NumConnectedOfType(ChronopicRegisterPort.Types.UNKNOWN) > 0)
			return true;

		return false;
	}
	*/

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
	private bool FTDIalways;

	public ChronopicRegisterLinux (bool compujump, bool showRunWireless, bool FTDIalways)
	{
		this.FTDIalways = FTDIalways;

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
			if (lineOut.Contains ("ID_VENDOR=") || lineOut.Contains ("ID_USB_VENDOR=")) //note before 2020 may 3 udevadm returned the first and now the second
			{
				string [] strFull = lineOut.Split(new char[] {'='});
				crp.FTDI = (strFull[1] == "FTDI");
			}
			else if (lineOut.Contains ("ID_SERIAL_SHORT=") || lineOut.Contains ("ID_USB_SERIAL_SHORT=")) //note before 2020 may 3 udevadm returned the first and now the second
			{
				string [] strFull = lineOut.Split(new char[] {'='});
				crp.SerialNumber = strFull[1];
			}
		}
		if (FTDIalways)
			crp.FTDI = true;

		//on Linux now we can use ACMs, but we cannot detect if they are FTDI, so mark them as FTDI in order to be shown on Chronojump
		if (crp.Port.Contains ("ACM"))
			crp.FTDI = true;

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
