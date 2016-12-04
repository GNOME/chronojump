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
using System.Threading;
using Gtk;

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
	private Chronopic.Plataforma platformState;
	
	private string lastConnectedRealPort = "";
	private string lastConnectedRealSerialNumber = "";
	private ChronopicRegisterPort.Types lastConnectedRealType = ChronopicRegisterPort.Types.UNKNOWN;


	// -----ConnectContactsReal START ----->

	Gtk.Window chronopic_contacts_real_win;
	Gtk.ProgressBar progressbar;

	private Thread connectContactsRealThread;
	//used to pass crp to connectContactsRealThread
	private ChronopicRegisterPort crpConnectContactsRealThread;
	private ChronopicInit chronopicInit;

	public bool SuccededConnectContactsRealThread;
	public Gtk.Button FakeButtonContactsRealDone;

	private void createGui(Gtk.Window app1, string labelStr)
	{
		chronopic_contacts_real_win = new Window ("Chronopic connection");
		chronopic_contacts_real_win.AllowGrow = false;
		chronopic_contacts_real_win.Modal = true;
		chronopic_contacts_real_win.TransientFor = app1;
		chronopic_contacts_real_win.BorderWidth= 20;

		chronopic_contacts_real_win.DeleteEvent += on_delete_event;

		Gtk.VBox vbox_main = new Gtk.VBox(false, 20);
		chronopic_contacts_real_win.Add(vbox_main);

		LogB.Information("Connecting real (starting connection)");
		LogB.Information("Press test button on Chronopic");

		Gtk.Label label = new Gtk.Label();
		label.Text = labelStr;
		vbox_main.Add(label);

		progressbar = new Gtk.ProgressBar();
		vbox_main.Add(progressbar);

		Gtk.Button button_cancel = new Gtk.Button("Cancel");
		button_cancel.Clicked += new EventHandler(on_button_cancel_clicked);
		Gtk.HButtonBox hbox = new Gtk.HButtonBox ();
		hbox.Add(button_cancel);
		vbox_main.Add(hbox);

		chronopic_contacts_real_win.ShowAll();
	}

	private void on_button_cancel_clicked(object o, EventArgs args)
	{
		cp.AbortFlush = true;
		chronopicInit.CancelledByUser = true;
	}
	private void on_delete_event (object o, DeleteEventArgs args)
	{
		LogB.Information("calling on_delete_event");

		args.RetVal = true;

		on_button_cancel_clicked(new object(), new EventArgs());
	}

	public void ConnectContactsReal(Gtk.Window app1, ChronopicRegisterPort crp, string labelStr)
	{
		createGui(app1, labelStr);

		crpConnectContactsRealThread = crp;

		connectContactsRealThread = new Thread (new ThreadStart (connectContactsRealDo));
		GLib.Idle.Add (new GLib.IdleHandler (pulseConnectContactsReal));

		LogB.ThreadStart();
		connectContactsRealThread.Start();
	}

	private void connectContactsRealDo()
	{
		ChronopicRegisterPort crp = crpConnectContactsRealThread;

		string message = "";
		bool success = false;

		sp = new SerialPort(crp.Port);
		chronopicInit = new ChronopicInit();
		bool connected = chronopicInit.Do(1, out cp, out sp,
				platformState, crp.Port, out message, out success);

		LogB.Information("Ended chronopicInit.Do()");

		if(connected) {
			lastConnectedRealPort = crp.Port;
			lastConnectedRealSerialNumber = crp.SerialNumber;
			lastConnectedRealType = ChronopicRegisterPort.Types.CONTACTS;
		}

		SuccededConnectContactsRealThread = connected;
	}

	bool pulseConnectContactsReal()
	{
		if(! connectContactsRealThread.IsAlive)
		{
			progressbar.Fraction = 1.0;
			LogB.ThreadEnding();
			connectContactsRealEnd();
			LogB.ThreadEnded();

			return false;
		}

		progressbar.Pulse();
		Thread.Sleep (50);
		return true;
	}

	private void connectContactsRealEnd()
	{
		if(SuccededConnectContactsRealThread)
			LogB.Information("Success at Connecting real!");
		else
			LogB.Warning("Failure at Connecting real!");

		hideAndNull();

		FakeButtonContactsRealDone.Click();
	}
	private void hideAndNull()
	{
		chronopic_contacts_real_win.Hide();
		chronopic_contacts_real_win = null;
	}


	public bool IsLastConnectedReal(ChronopicRegisterPort crp)
	{
		LogB.Information(string.Format(
					"lastConnectedReal (port:{0}, serialNumber:{1}, type:{2})",
					lastConnectedRealPort, lastConnectedRealSerialNumber,
					lastConnectedRealType.ToString()));
		LogB.Information(crp.ToString());

		if(lastConnectedRealPort != "" && lastConnectedRealSerialNumber != "" &&
				lastConnectedRealType == ChronopicRegisterPort.Types.CONTACTS &&
				crp.Port == lastConnectedRealPort &&
				crp.SerialNumber == lastConnectedRealSerialNumber)
			return true;

		return false;
	}

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

	public bool StoredCanCaptureContacts; //store a boolean in order to read info faster

	//<-----ConnectContactsReal END -----


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
			ChronopicAuto ca = new ChronopicAutoChangeDebounce();
			//write change
			ca.Write(sp, debounceChange);

			string ms = "";
			bool success = false;
			int tryNum = 10; //try to connect ten times
			do {
				//read if ok
				ca = new ChronopicAutoCheckDebounce();
				ms = ca.Read(sp);
				LogB.Information("ChronopicAutoCheckDebounce: " + ms);

				if(ms.Length == 0)
					LogB.Error("multitest firmware. ms is null");
				else if(ms[0] == '-') //is negative
					LogB.Error("multitest firmware. ms = " + ms);
				else if(debounceChange == 50 && ms == "50 ms")
					success = true;
				else if(debounceChange == 10 && ms == "10 ms")
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

	//public method to access from guiTests.cs
	public int TestsChangeMultitestFirmwareDo (int debounceChange)
	{
		return changeMultitestFirmwareDo(debounceChange);
	}

	// <----- change multitest firmware END

	public void SerialPortsCloseIfNeeded()
	{
		if(sp != null && sp.IsOpen) {
			LogB.Information("Closing sp");
			sp.Close();

			LogB.Information("Disposing cp to see if helps on OSX port busy");
			//cp = null;
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
