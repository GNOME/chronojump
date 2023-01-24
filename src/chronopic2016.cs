/*
 * Copyright (C) 2005  Juan Gonzalez Gomez
 * Copyright (C) 2014-2017  Xavier de Blas <xaviblas@gmail.com>
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
using System.Threading;
using Gtk;
using Mono.Unix;

public class Chronopic2016
{
	public Chronopic2016()
	{
		//this is constructed only one time

		FakeButtonContactsRealDone = new Gtk.Button();

		WindowOpened = false;
	}

	public bool WindowOpened;

	//used on contacts
	private Chronopic cp;
	private SerialPort sp;
	private Chronopic.Plataforma platformState = Chronopic.Plataforma.OFF;

	//multichronopic
	private Chronopic cp2;
	private SerialPort sp2;
	private int cpDoing; //2 is for the second chronopic on multichronopic

	//to check if cp changed
	private string lastConnectedRealPort = "";
	private string lastConnectedRealSerialNumber = "";
	private ChronopicRegisterPort.Types lastConnectedRealType = ChronopicRegisterPort.Types.UNKNOWN;

	//to check if cp2 changed
	private string lastConnectedRealPort2 = "";
	private string lastConnectedRealSerialNumber2 = "";
	private ChronopicRegisterPort.Types lastConnectedRealType2 = ChronopicRegisterPort.Types.UNKNOWN;


	// -----ConnectContactsReal START ----->

	//used to pass crp to connectContactsRealThread
	private ChronopicRegisterPort crpConnectContactsRealThread;
	private ChronopicInit chronopicInit;

	public bool SuccededConnectContactsRealThread;
	public Gtk.Button FakeButtonContactsRealDone;

	private void on_button_cancel_clicked(object o, EventArgs args)
	{
		if(cpDoing == 1)
			cp.AbortFlush = true;
		else //(cpDoing == 2)
			cp2.AbortFlush = true;

		if(chronopicInit != null)
			chronopicInit.CancelledByUser = true;
	}
	private void on_delete_event (object o, DeleteEventArgs args)
	{
		LogB.Information("calling on_delete_event on Chronopic2016");

		args.RetVal = true;

		on_button_cancel_clicked(new object(), new EventArgs());
	}

	public void ConnectContactsReal(Gtk.Window app1, ChronopicRegisterPort crp,
			int cpCount, string labelStr) //cpCount 2 is for 2nd chronopic on multichronopic
	{
		crpConnectContactsRealThread = crp;

		cpDoing = cpCount;

		connectContactsRealDo();
	}

	private void connectContactsRealDo()
	{
		ChronopicRegisterPort crp = crpConnectContactsRealThread;

		string message = "";
		bool success = false;
		bool connected = false;

		if(cpDoing == 1)
		{
			LogB.Information("connectContactsRealDo() 1");
			sp = new SerialPort(crp.Port);
			chronopicInit = new ChronopicInit();
			connected = chronopicInit.Do(1, out cp, out sp,
					platformState, crp.Port, out message, out success);
		} else //(cpDoing == 2)
		{
			LogB.Information("connectContactsRealDo() 2");
			sp2 = new SerialPort(crp.Port);
			chronopicInit = new ChronopicInit();
			connected = chronopicInit.Do(2, out cp2, out sp2,
					platformState, crp.Port, out message, out success);
		}

		LogB.Information("Ended chronopicInit.Do()");

		if(connected)
			assignLastConnectedVariables(crp);

		SuccededConnectContactsRealThread = connected;
		connectContactsRealEnd();
	}

	private void connectContactsRealEnd()
	{
		if(SuccededConnectContactsRealThread)
			LogB.Information("Success at Connecting real!");
		else
			LogB.Warning("Failure at Connecting real!");

		FakeButtonContactsRealDone.Click();
	}

	public bool IsLastConnectedReal(ChronopicRegisterPort crp, int cpCount)
	{
		if(cpCount == 1 &&
				lastConnectedRealPort != "" && lastConnectedRealSerialNumber != "" &&
				lastConnectedRealType == ChronopicRegisterPort.Types.CONTACTS &&
				crp.Port == lastConnectedRealPort &&
				crp.SerialNumber == lastConnectedRealSerialNumber)
				return true;
		else if(cpCount == 2 &&
				lastConnectedRealPort2 != "" && lastConnectedRealSerialNumber2 != "" &&
				lastConnectedRealType2 == ChronopicRegisterPort.Types.CONTACTS &&
				crp.Port == lastConnectedRealPort2 &&
				crp.SerialNumber == lastConnectedRealSerialNumber2)
				return true;

		return false;
	}

	private void assignLastConnectedVariables(ChronopicRegisterPort crp)
	{
		if(cpDoing == 1) {
			lastConnectedRealPort = crp.Port;
			lastConnectedRealSerialNumber = crp.SerialNumber;
			lastConnectedRealType = ChronopicRegisterPort.Types.CONTACTS;
		} else { //2
			lastConnectedRealPort2 = crp.Port;
			lastConnectedRealSerialNumber2 = crp.SerialNumber;
			lastConnectedRealType2 = ChronopicRegisterPort.Types.CONTACTS;
		}
	}

	/*
	//check if last connected real port exists on getFiles()
	public bool WindowsLastConnectedRealExists()
	{
		if(lastConnectedRealPort == null || lastConnectedRealPort == "")
			return false;

		foreach(string port in ChronopicPorts.GetPorts())
			if(port == lastConnectedRealPort)
				return true;

		return false;
	}
	*/

	public bool StoredCanCaptureContacts; //store a boolean in order to read info faster
	public bool StoredWireless; //store a boolean in order to read info faster

	//<-----ConnectContactsReal END -----


	//called from gui/chronojump.cs
	//done here because sending the SP is problematic on windows
	public string CheckAuto (out bool isChronopicAuto)
	{
		ChronopicAuto ca = new ChronopicAutoCheck();

		string str = "";
	        if(cpDoing == 1)
			str = ca.Read(sp);
		else
			str = ca.Read(sp2);

		isChronopicAuto = ca.IsChronopicAuto;

		return str;
	}



	// ----- change multitest firmware START ----->

	//change debounce time automatically on change menuitem mode (if multitest firmware)
	public bool ChangeMultitestFirmwarePre(int thresholdValue, int cpCount)
	{
		LogB.Information("ChangeMultitestFirmwareMaybe (A)");

		if(cp == null) {
			LogB.Information("Chronopic has been disconnected, cp == null");
			return false;
		}

		cpDoing = cpCount;

		//---- 1
		//bool ok = cp.Read_platform(out platformState);
		//seems better to have a new platformState:
		Chronopic.Plataforma ps;
		bool ok;
	        if(cpDoing == 1)
			ok = cp.Read_platform(out ps);
		else
			ok = cp2.Read_platform(out ps);

		if(! ok) {
			LogB.Information("Chronopic has been disconnected");
			//createChronopicWindow(true, "");
			//chronopicWin.Connected = false;
			return false;
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

		//---- 2 try to communicate with multitest firmware (return if cannot connect)

		LogB.Information("ChangeMultitestFirmwareMaybe (B)");
		bool isChronopicAuto = false;
		try {
			string result = CheckAuto(out isChronopicAuto);
			LogB.Debug("version: " + result);
		} catch {
			LogB.Information("Could not read from Chronopic with method 2");
			return false;
		}

		//---- 3 change debounce time
		LogB.Information("ChangeMultitestFirmwareMaybe (C)");
		if(isChronopicAuto)
		{
			bool changedOk = changeMultitestFirmwareDo(thresholdValue);
			if(! changedOk)
				return false;
		}

		LogB.Information("ChangeMultitestFirmwareMaybe (D)");

		return true;
	}

	private bool changeMultitestFirmwareDo (int debounceChange)
	{
		LogB.Information("ChangeMultitestFirmwareDo");
		try {
			ChronopicAuto ca = new ChronopicAutoChangeDebounce();
			//write change
			if(cpDoing == 1)
				ca.Write(sp, debounceChange);
			else
				ca.Write(sp2, debounceChange);

			string ms = "";
			bool success = false;
			int tryNum = 30; //try to connect 30 times. Linux is ok with 1, but... Windows and Mac users get what they bought
			do {
				//read if ok
				ca = new ChronopicAutoCheckDebounce();

				if(cpDoing == 1)
					ms = ca.Read(sp); //ms wil be eg. "50 ms"
				else
					ms = ca.Read(sp2); //ms wil be eg. "50 ms"
				LogB.Information("ChronopicAutoCheckDebounce: " + ms);

				if(ms.Length == 0)
					LogB.Error("multitest firmware. ms is null");
				else if(ms[0] == '-') //is negative
					LogB.Error("multitest firmware. ms = " + ms);
				else if(debounceChange.ToString() + " ms" == ms)
					success = true;

				tryNum --;
			} while (! success && tryNum > 0);

			LogB.Information("multitest firmware CHANGED to ms = " + ms);

			if(success)
				return true;
		} catch {
			LogB.Error("Could not change debounce");
		}

		return false;
	}

	//public method to access from guiTests.cs
	public bool TestsChangeMultitestFirmwareDo (int debounceChange)
	{
		return changeMultitestFirmwareDo(debounceChange);
	}

	// <----- change multitest firmware END

	//nullify only when exit software
	public void SerialPortsCloseIfNeeded(bool nullify)
	{
		if(sp != null && sp.IsOpen) {
			LogB.Information("Closing sp");
			sp.Close();

			LogB.Information("Flushing cp to see if helps on OSX port busy");
			cp.FlushByTimeOut();

			if(nullify) {
				LogB.Information("Disposing cp to see if helps on OSX port busy");
				cp = null;
			}
		}

		if(sp2 != null && sp2.IsOpen) {
			LogB.Information("Closing sp2");
			sp2.Close();

			LogB.Information("Flushing cp2 to see if helps on OSX port busy");
			cp2.FlushByTimeOut();

			if(nullify) {
				LogB.Information("Disposing cp2 to see if helps on OSX port busy");
				cp2 = null;
			}
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

	//multichronopic
	public Chronopic CP2
	{
		get { return cp2; }
	}

	//connectContactsRealDo() uses 1 or 2 cpDoing. This has to be known on gui/chronojump.cs
	//to call cp2016.ChangeMultitestFirmwarePre with 1 or 2
	public int CpDoing
	{
		get { return cpDoing; }
	}
}
