/*
 * Copyright (C) 2005  Juan Gonzalez Gomez
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

using System;
using System.IO.Ports;

public class Chronopic2016
{
	public Chronopic2016()
	{
		//this is constructed only one time
	}

	//used on contacts
	private Chronopic cp;
	private SerialPort sp;
	private Chronopic.Plataforma platformState;
	
	private string lastConnectedRealPort = "";
	private string lastConnectedRealSerialNumber = "";
	private ChronopicRegisterPort.Types lastConnectedRealType = ChronopicRegisterPort.Types.UNKNOWN;

	public bool IsLastConnectedReal(ChronopicRegisterPort crp)
	{
		LogB.Information("lastConnectedRealPort");
		LogB.Information(lastConnectedRealPort);
		LogB.Information("lastConnectedRealSerialNumber");
		LogB.Information(lastConnectedRealSerialNumber);
		LogB.Information("lastConnectedRealType");
		LogB.Information(lastConnectedRealType.ToString());
		crp.ToString();

		if(lastConnectedRealPort != "" && lastConnectedRealSerialNumber != "" && 
				lastConnectedRealType == ChronopicRegisterPort.Types.CONTACTS &&
				crp.Port == lastConnectedRealPort &&
				crp.SerialNumber == lastConnectedRealSerialNumber)
			return true;

		return false;
	}
	public bool ConnectContactsReal(ChronopicRegisterPort crp)
	{
		string message = "";
		bool success = false;

		sp = new SerialPort(crp.Port);
		ChronopicInit chronopicInit = new ChronopicInit();
		bool connected = chronopicInit.Do(1, out cp, out sp, platformState, crp.Port, out message, out success);

		if(connected) {
			lastConnectedRealPort = crp.Port;
			lastConnectedRealSerialNumber = crp.SerialNumber;
			lastConnectedRealType = ChronopicRegisterPort.Types.CONTACTS;
		}

		return connected;
	}

	//store a boolean in order to read info faster
	public bool StoredCanCaptureContacts;

	//called from gui/chronojump.cs
	//done here because sending the SP is problematic on windows
	public string CheckAuto (out bool isChronopicAuto)
	{
		ChronopicAuto ca = new ChronopicAutoCheck();

		string str = ca.Read(sp);

		isChronopicAuto = ca.IsChronopicAuto;

		return str;
	}


	// ----- change multitest firmware START ----->

	private bool previousMultitestFirmwareDefined = false;
	private Constants.Menuitem_modes previousMultitestFirmware;

	//change debounce time automatically on change menuitem mode (if multitest firmware)
	//return values:
	//-1 error
	//0 don't need to change
	//10 or 50 the change value
	public int ChangeMultitestFirmwareMaybe(Constants.Menuitem_modes m)
	{

		LogB.Information("ChangeMultitestFirmwareMaybe (A)");

		//---- 1 if don't need to change, return
		if(previousMultitestFirmwareDefined &&
				! Constants.Menuitem_mode_multitest_should_change(previousMultitestFirmware, m))
		{
			LogB.Information("don't need to change multitest firmware");
			return 0;
		}


		//bool ok = cp.Read_platform(out platformState);
		//seems better to have a new platformState:
		Chronopic.Plataforma ps;
		bool ok = cp.Read_platform(out ps);
		if(! ok) {
			LogB.Information("Chronopic has been disconnected");
			//createChronopicWindow(true, "");
			//chronopicWin.Connected = false;
			return -1;
		}


		/*
		 * method 1. Unused
		try {
			ChronopicAuto ca = new ChronopicAutoCheck();
			//problems with windows using this:
			string chronopicVersion = ca.Read(chronopicWin.SP);
			LogB.Debug("version: " + chronopicVersion);
		} catch {
			LogB.Information("Could not read from Chronopic with method 1");
			return;
		}*/

		//---- 4 try to communicate with multitest firmware (return if cannot connect)

		LogB.Information("ChangeMultitestFirmwareMaybe (B)");
		bool isChronopicAuto = false;
		try {
			string result = CheckAuto(out isChronopicAuto);
			LogB.Debug("version: " + result);
		} catch {
			LogB.Information("Could not read from Chronopic with method 2");
			return -1;
		}

		//---- 5 change 10 <-> 50 ms
		int returnValue = -1;

		LogB.Information("ChangeMultitestFirmwareMaybe (C)");
		if(isChronopicAuto) {
			int debounceChange = 50;
			if(m == Constants.Menuitem_modes.RUNSSIMPLE || m == Constants.Menuitem_modes.RUNSINTERVALLIC)
				debounceChange = 10;

			int msChanged = changeMultitestFirmwareDo(debounceChange);
			if(msChanged == 50)
				returnValue = 50;
			else if(msChanged == 10)
				returnValue = 10;
		}

		previousMultitestFirmwareDefined = true;
		previousMultitestFirmware = m;

		LogB.Information("ChangeMultitestFirmwareMaybe (D)");

		return returnValue;
	}

	private int changeMultitestFirmwareDo (int debounceChange)
	{
		LogB.Information("ChangeMultitestFirmwareDo");
		try {
			//write change
			ChronopicAuto ca = new ChronopicAutoChangeDebounce();
			ca.Write(sp, debounceChange);

			//read if ok
			string ms = "";
			bool success = false;
			int tryNum = 7; //try to connect seven times
			do {
				ca = new ChronopicAutoCheckDebounce();
				ms = ca.Read(sp);

				if(ms.Length == 0)
					LogB.Error("multitest firmware. ms is null");
				else if(ms[0] == '-') //is negative
					LogB.Error("multitest firmware. ms = " + ms);
				else
					success = true;
				tryNum --;
			} while (! success && tryNum > 0);

			LogB.Debug("multitest firmware. ms = " + ms);

			if(ms == "50 ms")
				return 50;
			else if(ms == "10 ms")
				return 10;
		} catch {
			LogB.Error("Could not change debounce");
		}

		return -1;
	}

	// <----- change multitest firmware END

	public void SerialPortsCloseIfNeeded() {
		if(sp != null && sp.IsOpen) {
			LogB.Information("Closing sp");
			sp.Close();
		}
	}

	public Chronopic CP
	{
		get { return cp; }
	}

	public SerialPort SP
	{
		get { return sp; }
	}
}
