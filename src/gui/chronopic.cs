/*
 * This file is part of ChronoJump
 *
 * Chronojump is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or   
 *    (at your option) any later version.
 *    
 * Chronojump is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
 *    GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 * Copyright (C) 2004-2009   Xavier de Blas <xaviblas@gmail.com> 
 */


using System;
using Gtk;
using Gdk;
using Glade;
using System.IO.Ports;
using Mono.Unix;
using System.Threading;
using System.IO; //"File" things


public class ChronopicWindow 
{
	[Widget] Gtk.Window chronopic_window;
	static ChronopicWindow ChronopicWindowBox;
	ChronopicConnection chronopicConnectionWin;


	[Widget] Gtk.Image image_cp1_yes;
	[Widget] Gtk.Image image_cp1_no;
	[Widget] Gtk.Image image_cp2_yes;
	[Widget] Gtk.Image image_cp2_no;
	[Widget] Gtk.Image image_cp3_yes;
	[Widget] Gtk.Image image_cp3_no;
	[Widget] Gtk.Image image_cp4_yes;
	[Widget] Gtk.Image image_cp4_no;
				
	//[Widget] Gtk.Entry entry_multi_chronopic_cp2;
	
	[Widget] Gtk.ComboBox combo_linux1;
	[Widget] Gtk.ComboBox combo_windows1;
	string [] comboWindowsOptions;

	//chronopic connection thread
	Thread thread;
	bool needUpdateChronopicWin;
	bool updateChronopicWinValuesState;
	string updateChronopicWinValuesMessage;
	[Widget] Gtk.Button fakeChronopicButton; //raised when chronopic detection ended
	

	//preferences variables
	private static string chronopicPort;
	
	//platform state variables
	enum States {
		ON,
		OFF
	}
	bool connected;
	int currentCp; //1 to 4

	//cp1	
	Chronopic cp;
	SerialPort sp;
	Chronopic.Plataforma platformState;	//on (in platform), off (jumping), or unknow
	
	//cp2	
	Chronopic cp2;
	SerialPort sp2;
	Chronopic.Plataforma platformState2;

	//cp3	
	Chronopic cp3;
	SerialPort sp3;
	Chronopic.Plataforma platformState3;

	//cp4	
	Chronopic cp4;
	SerialPort sp4;
	Chronopic.Plataforma platformState4;

	States loggedState;		//log of last state


	
	public ChronopicWindow()
	{
		Glade.XML gxml;
		gxml = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "chronopic_window", null);
		gxml.Autoconnect(this);

		//put an icon to window
		UtilGtk.IconWindow(chronopic_window);
		
		if(Util.IsWindows()) 
			combo_linux1.Hide();
		else
			combo_windows1.Hide();

		connected = false;
		image_cp1_yes.Hide();
		image_cp2_yes.Hide();
		image_cp3_yes.Hide();
		image_cp4_yes.Hide();
		
		currentCp = 1; //TODO: change
		
		chronopicPort = SqlitePreferences.Select("chronopicPort");
	
		if(chronopicPort != Constants.ChronopicDefaultPortWindows && 
				(chronopicPort != Constants.ChronopicDefaultPortLinux && File.Exists(chronopicPort))
		  ) {
			ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString("Do you want to connect to Chronopic now?"), "", "");
			confirmWin.Button_accept.Clicked += new EventHandler(chronopicAtStart);
		}
	}
	
	static public ChronopicWindow Create ()
	{
		if (ChronopicWindowBox == null) {
			ChronopicWindowBox = new ChronopicWindow ();
		}
		
		//don't show until View is called
		//ChronopicWindowBox.chronopic_window.Show ();
		
		return ChronopicWindowBox;
	}

	static public ChronopicWindow View ()
	{
		if (ChronopicWindowBox == null) {
			ChronopicWindowBox = new ChronopicWindow ();
		}
		
		ChronopicWindowBox.comboStuff();
		
		ChronopicWindowBox.chronopic_window.Show();
		
		return ChronopicWindowBox;
	}

	private void comboStuff() {
		//combo port stuff
		comboWindowsOptions = new string[257];
		int count = 0;
		for (int i=1; i <= 257; i ++)
			comboWindowsOptions[i-1] = "COM" + i;
		
		chronopicPort = SqlitePreferences.Select("chronopicPort");

		if(Util.IsWindows()) {
			UtilGtk.ComboUpdate(combo_windows1, comboWindowsOptions, comboWindowsOptions[0]);
			
			if(chronopicPort.Length > 0)
				combo_windows1.Active = UtilGtk.ComboMakeActive(comboWindowsOptions, chronopicPort);
			else
				combo_windows1.Active = 0; //first option
		} else {
			string [] usbSerial = Directory.GetFiles("/dev/", "ttyUSB*");
			string [] serial = Directory.GetFiles("/dev/", "ttyS*");
			string [] all = Util.AddArrayString(usbSerial, serial);
			string [] def = Util.StringToStringArray(Constants.ChronopicDefaultPortLinux);
			string [] allWithDef = Util.AddArrayString(def, all);
	
			UtilGtk.ComboUpdate(combo_linux1, allWithDef, Constants.ChronopicDefaultPortLinux);
			
			if(chronopicPort.Length > 0)
				combo_linux1.Active = UtilGtk.ComboMakeActive(allWithDef, chronopicPort);
			else 
				combo_linux1.Active = 0; //first option
		}
	}

	private void chronopicAtStart(object o, EventArgs args) {
		//make active menuitem chronopic, and this
		//will raise other things
//		menuitem_chronopic.Active = true;
		Log.WriteLine("CP AT START from gui/chronopic.cs");
	}


	protected bool PulseGTK ()
	{
		if(needUpdateChronopicWin || ! thread.IsAlive) {
			fakeChronopicButton.Click();
			Log.Write("dying");
			return false;
		}
		//need to do this, if not it crashes because chronopicConnectionWin gets died by thread ending
		ChronopicConnection chronopicConnectionWin = ChronopicConnection.Show();
		chronopicConnectionWin.Pulse();
		
		Thread.Sleep (50);
		Log.Write(thread.ThreadState.ToString());
		return true;
	}
			
	private void updateChronopicWin(bool state, string message) {
		Log.WriteLine("-----------------");

		//need to do this, if not it crashes because chronopicConnectionWin gets died by thread ending
		chronopicConnectionWin = ChronopicConnection.Show();

		Log.WriteLine("+++++++++++++++++");
		if(state)
			chronopicConnectionWin.Connected(message);
		else
			chronopicConnectionWin.Disconnected(message);
		
		needUpdateChronopicWin = false;
	}

	//chronopic init should not touch  gtk, for the threads
	private Chronopic chronopicInit (Chronopic myCp, out SerialPort mySp, Chronopic.Plataforma myPS, string myPort, out string returnString, out bool success) 
	{
		Log.WriteLine ( Catalog.GetString ("starting connection with chronopic") );
		if(Util.IsWindows())
			Log.WriteLine ( Catalog.GetString ("If you have previously used the modem via a serial port (in a GNU/Linux session, and you selected serial port), Chronojump will crash.") );

		success = true;
		
		Log.WriteLine("+++++++++++++++++ 1 ++++++++++++++++");		
		Log.WriteLine(string.Format("chronopic port: {0}", myPort));
		mySp = new SerialPort(myPort);
		try {
			mySp.Open();
			Log.WriteLine("+++++++++++++++++ 2 ++++++++++++++++");		
			//-- Create chronopic object, for accessing chronopic
			myCp = new Chronopic(mySp);

			Log.WriteLine("+++++++++++++++++ 3 ++++++++++++++++");		
			//on windows, this check make a crash 
			//i think the problem is: as we don't really know the Timeout on Windows (.NET) and this variable is not defined on chronopic.cs
			//the Read_platform comes too much soon (when cp is not totally created), and this makes crash

			//-- Obtener el estado inicial de la plataforma

			bool ok=false;
			Log.WriteLine("+++++++++++++++++ 4 ++++++++++++++++");		
			do {
				Log.WriteLine("+++++++++++++++++ 5 ++++++++++++++++");		
				ok=myCp.Read_platform(out myPS);
				Log.WriteLine("+++++++++++++++++ 6 ++++++++++++++++");		
			} while(!ok);
			Log.WriteLine("+++++++++++++++++ 7 ++++++++++++++++");		
			if (!ok) {
				//-- Si hay error terminar
				Log.WriteLine(string.Format("Error: {0}", myCp.Error));
				success = false;
			}
		} catch {
			success = false;
		}
			
		returnString = "";
		if(success) {
			if(currentCp == 1)
				connected = true;
			returnString = string.Format(Catalog.GetString("<b>Connected</b> to Chronopic on port: {0}"), myPort);
			//appbar2.Push( 1, returnString);
		}
		if(! success) {
			returnString = Catalog.GetString("Problems communicating to chronopic.");
			if(currentCp == 1) 
				returnString += " " + Catalog.GetString("Changed platform to 'Simulated'");
			if(Util.IsWindows()) {
				returnString += Catalog.GetString("\n\nOn Windows we recommend to remove and connect USB or serial cable from the computer after every unsuccessful port test.");
				returnString += Catalog.GetString("\n... And after cancelling Chronopic detection.");
				returnString += Catalog.GetString("\n\n... Later, when you close Chronojump it will probably get frozen. If this happens, let's press CTRL+C on the black screen.");
			}

			//this will raise on_radiobutton_simulated_ativate and 
			//will put cpRunning to false, and simulated to true and cp.Close()
			if(currentCp == 1) {
//				menuitem_simulated.Active = true;
				connected = false;
			}
		}
		return myCp;
	}
	

	private void on_button_connect_cp1_clicked (object o, EventArgs args) {
		/*
		if(image_cp2_no.Visible)
			currentCp = 2;
		else if(image_cp3_no.Visible)
			currentCp = 3;
		else if(image_cp4_no.Visible)
			currentCp = 4;
			*/

		if(Util.IsWindows())
			chronopicPort = UtilGtk.ComboGetActive(combo_windows1);
		else
			chronopicPort = UtilGtk.ComboGetActive(combo_linux1);
		prepareChronopicConnection();
	}
	
	private void on_button_help_clicked (object o, EventArgs args) {
		Log.WriteLine("HELP");
		new HelpPorts();
	}


	/*
	private void createComboMultiChronopic() 
	{
		table_multi_chronopic_buttons.Sensitive = false;
		menuitem_multi_chronopic_start.Sensitive = false;
		menuitem_run_analysis.Sensitive = false;
		button_connect_cp.Sensitive = false;
		image_cp1_yes.Hide();
		image_cp2_yes.Hide();
		image_cp3_yes.Hide();
		image_cp4_yes.Hide();

		if(Util.IsWindows()) {
			combo_windows1.Sensitive = false;
			combo_linux1.Hide();
			string [] comboWindowsOptions = new string[257];
			for (int count = 0, i=1; i <= 257; i ++)
				comboWindowsOptions[i-1] = "COM" + i;

			UtilGtk.ComboUpdate(combo_windows1, comboWindowsOptions, comboWindowsOptions[0]);
			combo_windows1.Changed += new EventHandler (on_combo_multi_chronopic_changed);
		} else {
			combo_linux1.Sensitive = false;
			combo_windows1.Hide();
			UtilGtk.ComboUpdate(combo_linux1, Constants.ComboPortLinuxOptions, Constants.ComboPortLinuxOptions[0]);
			combo_linux1.Active = 0; //first option
			combo_linux1.Changed += new EventHandler (on_combo_multi_chronopic_changed);
		}
	}


	private void on_combo_multi_chronopic_changed(object o, EventArgs args) {
		ComboBox combo = o as ComboBox;
		if (o == null)
			return;
		
		bool portOk = true;
		if(UtilGtk.ComboGetActive(combo) == Constants.ChronopicDefaultPortWindows ||
				UtilGtk.ComboGetActive(combo) == Constants.ChronopicDefaultPortLinux) 
			portOk = false;

		if (o == combo_linux1 || o == combo_windows1) 
			button_connect_cp.Sensitive = portOk;
	}
	*/


	public void SerialPortsClose() {
		Console.WriteLine("Closing sp");
		sp.Close();
		/*

//		image_cp1_no.Show();
//		image_cp1_yes.Hide();
		//close connection with other chronopics on multiChronopic
		if(image_cp2_yes.Visible) {
			Console.WriteLine("Closing sp2");
			sp2.Close();
//			image_cp2_no.Show();
//			image_cp2_yes.Hide();
		}
		if(image_cp3_yes.Visible) {
			Console.WriteLine("Closing sp3");
			sp3.Close();
//			image_cp3_no.Show();
//			image_cp3_yes.Hide();
		}
		if(image_cp4_yes.Visible) {
			Console.WriteLine("Closing sp4");
			sp4.Close();
//			image_cp4_no.Show();
//			image_cp4_yes.Hide();
		}
		Console.WriteLine("Closed all");
		*/
	}

	/*
	void on_radiobutton_simulated (object o, EventArgs args)
	{
		Log.WriteLine(string.Format("RAD - simul. cpRunning: {0}", cpRunning));
		if(menuitem_simulated.Active) {
			Log.WriteLine("RadioSimulated - ACTIVE");
			simulated = true;
			SqlitePreferences.Update("simulated", simulated.ToString(), false);

			//close connection with chronopic if initialized
			if(cpRunning) {
				serialPortsClose();

				table_multi_chronopic_buttons.Sensitive = false;
				combo_windows1.Sensitive = false;
				combo_linux1.Sensitive = false;
		
				//regenerate combos (maybe some ports have been deleted on using before going to simulated)
				if(Util.IsWindows()) {
					string [] comboWindowsOptions = new string[257];
					for (int count = 0, i=1; i <= 257; i ++)
						comboWindowsOptions[i-1] = "COM" + i;
					UtilGtk.ComboUpdate(combo_windows1, comboWindowsOptions, comboWindowsOptions[0]);
				} else {
					UtilGtk.ComboUpdate(combo_linux1, Constants.ComboPortLinuxOptions, Constants.ComboPortLinuxOptions[0]);
					combo_linux1.Active = 0; //first option
				}
			}
			Log.WriteLine("cpclosed");
			cpRunning = false;
		}
		else
			Log.WriteLine("RadioSimulated - INACTIVE");
		
		Log.WriteLine("all done");
	}
	
	void on_radiobutton_chronopic (object o, EventArgs args)
	{
		Log.WriteLine(string.Format("RAD - chrono. cpRunning: {0}", cpRunning));
		if(! preferencesLoaded)
			return;

		if(! menuitem_chronopic.Active) {
			appbar2.Push( 1, Catalog.GetString("Changed to simulated mode"));
			Log.WriteLine("RadioChronopic - INACTIVE");
			return;
		}

		if(chronopicPort == Constants.ChronopicDefaultPortWindows ||
				chronopicPort == Constants.ChronopicDefaultPortLinux) {
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("You need to configurate the Chronopic port at preferences."));
			menuitem_simulated.Active = true;
			return;
		}

		Log.WriteLine("RadioChronopic - ACTIVE");
	
		currentCp = 1;
		prepareChronopicConnection();
	}
	*/

	void prepareChronopicConnection() {
		ChronopicConnection chronopicConnectionWin = ChronopicConnection.Show();
		chronopicConnectionWin.LabelFeedBackReset();

		chronopicConnectionWin.Button_cancel.Clicked += new EventHandler(on_chronopic_cancelled);
		
		fakeChronopicButton = new Gtk.Button();
		fakeChronopicButton.Clicked += new EventHandler(on_chronopic_detection_ended);

		thread = new Thread(new ThreadStart(waitChronopicStart));
		GLib.Idle.Add (new GLib.IdleHandler (PulseGTK));
		thread.Start(); 
	}
	
	protected void waitChronopicStart () 
	{
		if(currentCp == 1) {
		//	simulated = false;
		//	SqlitePreferences.Update("simulated", simulated.ToString(), false);
			if(connected)
				return;
		}

		string message = "";
		string myPort = "";
		bool success = false;
			
		//if(currentCp == 1) 
			myPort = chronopicPort;
		/*
		else {
			if(Util.IsWindows()) 
				myPort = UtilGtk.ComboGetActive(combo_windows1);
			else
				myPort = UtilGtk.ComboGetActive(combo_linux1);
		}
		*/

		if(currentCp == 1) {
			cp = chronopicInit(cp, out sp, platformState, myPort, out message, out success);
			if(success) {
				image_cp1_no.Hide();
				image_cp1_yes.Show();
			} else {
				image_cp1_no.Show();
				image_cp1_yes.Hide();
			}
		}
		else if(currentCp == 2) {
			cp2 = chronopicInit(cp2, out sp2, platformState2, myPort, out message, out success);
			if(success) {
				image_cp2_no.Hide();
				image_cp2_yes.Show();
			} 
		}
		else if(currentCp == 3) {
			cp3 = chronopicInit(cp3, out sp3, platformState3, myPort, out message, out success);
			if(success) {
				image_cp3_no.Hide();
				image_cp3_yes.Show();
			} 
		}
		else if(currentCp == 4) {
			cp4 = chronopicInit(cp4, out sp4, platformState4, myPort, out message, out success);
			if(success) {
				image_cp4_no.Hide();
				image_cp4_yes.Show();
				//button_connect_cp.Sensitive = false;
			} 
		}
		

		Log.WriteLine(string.Format("wait_chronopic_start {0}", message));
			
		if(success) {
			updateChronopicWinValuesState= true; //connected
			updateChronopicWinValuesMessage= message;
			
		/*	
			if(currentCp >= 2) {
//				table_multi_chronopic_buttons.Sensitive = true;
				if(Util.IsNumber(entry_run_analysis_distance.Text, false)) {
//					menuitem_multi_chronopic_start.Sensitive = true;
//					menuitem_run_analysis.Sensitive = true;
//					button_run_analysis.Sensitive = true;
				} else {
//					menuitem_multi_chronopic_start.Sensitive = false;
//					menuitem_run_analysis.Sensitive = false;
//					button_run_analysis.Sensitive = false;
				}
			}
	
			//disallow selection of that port for other chronopics
			//and change sensitiveness of combo port 
			if(Util.IsWindows()) {
				UtilGtk.ComboDelThisValue(combo_windows1, myPort);
				combo_windows1.Active = 0; //first option
				if(currentCp < 4)
					combo_windows1.Sensitive = true;
				else
					combo_windows1.Sensitive = false;
			} else {
				UtilGtk.ComboDelThisValue(combo_linux1, myPort);
				combo_linux1.Active = 0; //first option
				if(currentCp < 4)
					combo_linux1.Sensitive = true;
				else
					combo_linux1.Sensitive = false;
			}
			*/
		} else {
			updateChronopicWinValuesState= false; //disconnected
			updateChronopicWinValuesMessage= message;
		}
		needUpdateChronopicWin = true;
	}

	private void on_chronopic_detection_ended(object o, EventArgs args) {
		updateChronopicWin(updateChronopicWinValuesState, updateChronopicWinValuesMessage);
	}


	private void on_chronopic_cancelled (object o, EventArgs args) {
		Log.WriteLine("cancelled-----");
		
		//kill the chronopicInit function that is waiting event 
		thread.Abort();
		
		//menuitem_chronopic.Active = false;
		//menuitem_simulated.Active = true;
				
		updateChronopicWinValuesState= false; //disconnected
		updateChronopicWinValuesMessage= Catalog.GetString("Cancelled by user");
		needUpdateChronopicWin = true;
			
	}
	
	//private void on_chronopic_closed (object o, EventArgs args) {
	//}

	void on_button_close_clicked (object o, EventArgs args)
	{
		Log.WriteLine("CLOSE");
		ChronopicWindowBox.chronopic_window.Hide();
//		ChronopicWindowBox = null;
	}

	void on_delete_event (object o, DeleteEventArgs args)
	{
		ChronopicWindowBox.chronopic_window.Hide();
		ChronopicWindowBox = null;
	}



	public Chronopic CP {
		get { return cp; }
	}

	public Chronopic CP2 {
		get { return cp2; }
	}

	public Chronopic CP3 {
		get { return cp3; }
	}

	public Chronopic CP4 {
		get { return cp4; }
	}
	
	public bool Connected {
		get { return connected; }
	}

}
