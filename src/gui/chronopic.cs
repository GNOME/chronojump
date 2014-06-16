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
 * Copyright (C) 2004-2014   Xavier de Blas <xaviblas@gmail.com> 
 */


using System;
using Gtk;
using Gdk;
using Glade;
using System.IO.Ports;
using Mono.Unix;
using System.Threading;
using System.IO; //"File" things
using System.Collections; //ArrayList

public class ChronopicPortData
{
	public int Num;
	public string Port; //port filename
	public bool Connected;
	
	public ChronopicPortData(int num, string port, bool connected) {
		this.Num = num;
		this.Port = port;
		this.Connected = connected;
	}
}

public class ChronopicWindow 
{
	[Widget] Gtk.Window chronopic_window;
	static ChronopicWindow ChronopicWindowBox;
	//ChronopicConnection chronopicConnectionWin;

	[Widget] Gtk.Notebook notebook_main;
	//[Widget] Gtk.Image image_contact_modular;
	//[Widget] Gtk.Image image_infrared;
	
	[Widget] Gtk.Label label_connect_contacts;
	[Widget] Gtk.Label label_connect_encoder;

	[Widget] Gtk.Image image_cp1_yes;
	[Widget] Gtk.Image image_cp1_no;
	[Widget] Gtk.Image image_cp2_yes;
	[Widget] Gtk.Image image_cp2_no;
	[Widget] Gtk.Image image_cp3_yes;
	[Widget] Gtk.Image image_cp3_no;
	[Widget] Gtk.Image image_cp4_yes;
	[Widget] Gtk.Image image_cp4_no;
				
	//[Widget] Gtk.Entry entry_multi_chronopic_cp2;
	
	string [] comboWindowsOptions;
	[Widget] Gtk.ComboBox combo_windows1;
	[Widget] Gtk.ComboBox combo_windows2;
	[Widget] Gtk.ComboBox combo_windows3;
	[Widget] Gtk.ComboBox combo_windows4;
	[Widget] Gtk.ComboBox combo_linux1;
	[Widget] Gtk.ComboBox combo_linux2;
	[Widget] Gtk.ComboBox combo_linux3;
	[Widget] Gtk.ComboBox combo_linux4;
	[Widget] Gtk.ComboBox combo_windows_encoder;
	[Widget] Gtk.ComboBox combo_linux_encoder;
	
	[Widget] Gtk.Button button_connect_cp1;
	[Widget] Gtk.Button button_connect_cp2;
	[Widget] Gtk.Button button_connect_cp3;
	[Widget] Gtk.Button button_connect_cp4;
	
	[Widget] Gtk.CheckButton checkbutton_multi_show;
	[Widget] Gtk.Table table_multi_chronopic;
	//[Widget] Gtk.Button button_reload;
	
	//frame_connections
	[Widget] Gtk.Frame frame_connection;
	[Widget] Gtk.Label label_title;
	[Widget] Gtk.ProgressBar progressbar;
	[Widget] Gtk.Button button_cancel;
	
	[Widget] Gtk.Image chronopic_image;
	[Widget] Gtk.TextView textview_ports_found_explanation;

	//chronopic connection thread
	Thread thread;
	bool needUpdateChronopicWin;
	bool updateChronopicWinValuesState;
	string updateChronopicWinValuesMessage;
	//Gtk.Button fakeButtonCancelled;

	[Widget] Gtk.Button fakeConnectionButton; //raised when chronopic detection ended
	[Widget] Gtk.Button fakeWindowDone; //raised when chronopic detection ended
	//[Widget] Gtk.Button fakeWindowReload; //raised when asked to reload

	bool isWindows;	

	//ArrayList of ChronopicPortData
	static ArrayList cpd;
	static string encoderPort;
	
	//platform state variables
	enum States {
		ON,
		OFF
	}
	bool connected;
	string type;	//"contacts" or "encoder"
	bool volumeOn;
	int currentCp; //1 to 4
	bool cancelledByUser;
		
	//in order to cancel before close window
	static bool connecting;

	//cp1	
	Chronopic cp;
	SerialPort sp;
	Chronopic.Plataforma platformState;	//on (in platform), off (jumping), or unknow
	
	//cp2	
	Chronopic cp2;
	Chronopic.Plataforma platformState2;

	//cp3	
	Chronopic cp3;
	Chronopic.Plataforma platformState3;

	//cp4	
	Chronopic cp4;
	Chronopic.Plataforma platformState4;

	States loggedState;		//log of last state
	

	public ChronopicWindow(ArrayList myCpd)
	{
		Glade.XML gxml;
		gxml = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "chronopic_window", "chronojump");
		gxml.Autoconnect(this);

		cpd = myCpd;
			
		UtilGtk.IconWindow(chronopic_window);

		if(UtilAll.IsWindows())
			isWindows = true;
		else
			isWindows = false;

		setDefaultValues();		
		
		//Pixbuf pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "chronopic_128.png");
		//chronopic_image.Pixbuf = pixbuf;
		/*
		Pixbuf pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(true) + Constants.FileNameContactPlatformModular);
		image_contact_modular.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(true) + Constants.FileNameInfrared);
		image_infrared.Pixbuf = pixbuf;
		*/

		/*
		if(chronopicPort1 != Constants.ChronopicDefaultPortWindows && 
				(chronopicPort1 != Constants.ChronopicDefaultPortLinux && File.Exists(chronopicPort1))
		  ) {
			ConfirmWindow confirmWin = ConfirmWindow.Show(Catalog.GetString("Do you want to connect to Chronopic now?"), "", "");
			confirmWin.Button_accept.Clicked += new EventHandler(chronopicAtStart);
		}
		*/
	}
	
	//recreate is used when a Chronopic was disconnected
	//port names com from gui/chronojump.cs to this method (myCpd)
	static public ChronopicWindow Create (ArrayList myCpd, string myEncoderPort, bool recreate, bool volumeOn)
	{
		if (ChronopicWindowBox != null && recreate) {
			ChronopicWindowBox.chronopic_window.Hide();
		}
		if (ChronopicWindowBox == null || recreate) {
			ChronopicWindowBox = new ChronopicWindow (myCpd);
		}
		
		//don't show until View is called
		//ChronopicWindowBox.chronopic_window.Show ();
		
		ChronopicWindowBox.volumeOn = volumeOn;
		encoderPort = myEncoderPort;

		ChronopicWindowBox.fakeWindowDone = new Gtk.Button();
		//ChronopicWindowBox.fakeWindowReload = new Gtk.Button();
		
		return ChronopicWindowBox;
	}

	static public ChronopicWindow View (string type, bool volumeOn)
	{
		if (ChronopicWindowBox == null) {
			ChronopicWindowBox = new ChronopicWindow (cpd);
		} 
		
		ChronopicWindowBox.type = type;
		
		if(type == "contacts")
			ChronopicWindowBox.notebook_main.CurrentPage = 0;
		else
			ChronopicWindowBox.notebook_main.CurrentPage = 1;
		
		ChronopicWindowBox.volumeOn = volumeOn;
		ChronopicWindowBox.checkChronopicDisconnected();
		ChronopicWindowBox.createCombos();

		//ports info comes from gui/chronojump.cs to Create mehod
		ChronopicWindowBox.info();

		ChronopicWindowBox.chronopic_window.Show();
		ChronopicWindowBox.chronopic_window.Present();
Log.WriteLine("bbb");
	
		return ChronopicWindowBox;
	}

	private void setDefaultValues() {
		
		label_connect_contacts.Text = "<b>" + label_connect_contacts.Text + "</b>";
		label_connect_encoder.Text = "<b>" + label_connect_encoder.Text + "</b>";
		label_connect_contacts.UseMarkup = true;
		label_connect_encoder.UseMarkup = true;
		
		
		checkbutton_multi_show.Active = false;
		table_multi_chronopic.Visible = false;

		if(isWindows) {
			combo_linux1.Hide();
			combo_linux2.Hide();
			combo_linux3.Hide();
			combo_linux4.Hide();
			combo_linux_encoder.Hide();
				
			combo_windows2.Sensitive = false;
			combo_windows3.Sensitive = false;
			combo_windows4.Sensitive = false;
		} else {
			combo_windows1.Hide();
			combo_windows2.Hide();
			combo_windows3.Hide();
			combo_windows4.Hide();
			combo_windows_encoder.Hide();
				
			combo_linux2.Sensitive = false;
			combo_linux3.Sensitive = false;
			combo_linux4.Sensitive = false;
		}
			
		button_connect_cp1.Sensitive = false;
		button_connect_cp2.Sensitive = false;
		button_connect_cp3.Sensitive = false;
		button_connect_cp4.Sensitive = false;

		connected = false;
		image_cp1_yes.Hide();
		image_cp2_yes.Hide();
		image_cp3_yes.Hide();
		image_cp4_yes.Hide();
		
		//encoderPort = "";
		//fakeButtonCancelled = new Gtk.Button();
	}
	
	//check if user has disconnected chronopic or port has changed
	private void checkChronopicDisconnected() {
		foreach(ChronopicPortData a in cpd) {
			Chronopic myCP;
			Chronopic.Plataforma myPS;
			if(a.Num == 1) {
				myCP = cp;
				myPS = platformState;
			} else if(a.Num == 2) {
				myCP = cp2;
				myPS = platformState2;
			} else if(a.Num == 3) {
				myCP = cp3;
				myPS = platformState3;
			} else { //if(a.Num == 4) {
				myCP = cp4;
				myPS = platformState4;
			}
			
			int errorCount = 0;
			bool ok = false;	
			if(a.Connected) {
				//try {
					ok = myCP.Read_platform(out myPS);
				//} catch { 
				//	Log.WriteLine("catch at 1"); 
				//}
				if(!ok) {
					Log.WriteLine("false at 1");
					errorCount ++;
				}
			}
			if(errorCount > 0) {
				ArrayList myCPD = new ArrayList();
				for(int i=1; i<=4;i++) {
					ChronopicPortData b = new ChronopicPortData(i,"",false);
					myCPD.Add(b);
				}
				Create (myCPD, encoderPort, true, volumeOn);
				new DialogMessage(Constants.MessageTypes.WARNING, 
						Catalog.GetString("One or more Chronopics have been disconnected.") + "\n" + 
						Catalog.GetString("Please connect again, and configure on Chronopic window."));
			}
		}
	}
	
	private void createCombos() {
		if(isWindows)
			ChronopicWindowBox.createComboWindows();
		else
			ChronopicWindowBox.createComboLinux();
	}

	//private void createComboWindows(string myPort, Gtk.ComboBox myCombo) {
	private void createComboWindows() {
		//combo port stuff
		comboWindowsOptions = new string[32];
		//for (int i=1; i <= 32; i ++)
			//comboWindowsOptions[i-1] = "COM" + i;
		comboWindowsOptions = SerialPort.GetPortNames();

		string [] def = Util.StringToStringArray(Constants.ChronopicDefaultPortWindows);
		string [] allWithDef = Util.AddArrayString(def, comboWindowsOptions);
	
		UtilGtk.ComboUpdate(combo_windows1, allWithDef, Constants.ChronopicDefaultPortWindows);
		UtilGtk.ComboUpdate(combo_windows2, allWithDef, Constants.ChronopicDefaultPortWindows);
		UtilGtk.ComboUpdate(combo_windows3, allWithDef, Constants.ChronopicDefaultPortWindows);
		UtilGtk.ComboUpdate(combo_windows4, allWithDef, Constants.ChronopicDefaultPortWindows);
		
		foreach(ChronopicPortData a in cpd) {
			if(a.Num == 1) {
				combo_windows1.Active = UtilGtk.ComboMakeActive(comboWindowsOptions, a.Port);
				combo_windows1.Changed += new EventHandler (on_combo_changed);
				if(a.Connected) {
					UtilGtk.ComboDelThisValue(combo_windows2, a.Port);
					UtilGtk.ComboDelThisValue(combo_windows3, a.Port);
					UtilGtk.ComboDelThisValue(combo_windows4, a.Port);
				}
			} else if(a.Num == 2) {
				combo_windows2.Active = UtilGtk.ComboMakeActive(comboWindowsOptions, a.Port);
				combo_windows2.Changed += new EventHandler (on_combo_changed);
				if(a.Connected) {
					UtilGtk.ComboDelThisValue(combo_windows3, a.Port);
					UtilGtk.ComboDelThisValue(combo_windows4, a.Port);
				}
			} else if(a.Num == 3) {
				combo_windows3.Active = UtilGtk.ComboMakeActive(comboWindowsOptions, a.Port);
				combo_windows3.Changed += new EventHandler (on_combo_changed);
				if(a.Connected)
					UtilGtk.ComboDelThisValue(combo_windows4, a.Port);
			} else { //4
				combo_windows4.Active = UtilGtk.ComboMakeActive(comboWindowsOptions, a.Port);
				combo_windows4.Changed += new EventHandler (on_combo_changed);
			}
		}
		
		//encoder
		//this reduces the callbacks of combo change
		combo_windows_encoder.Sensitive = false;

		UtilGtk.ComboUpdate(combo_windows_encoder, allWithDef, encoderPort);

		combo_windows_encoder.Changed += new EventHandler (on_combo_changed);
			
		combo_windows_encoder.Active = UtilGtk.ComboMakeActive(allWithDef, encoderPort);

		combo_windows_encoder.Sensitive = true;
	}

	private void createComboLinux() {
		//string [] serial = Directory.GetFiles("/dev/", "ttyS*");
		string [] usbSerial = Directory.GetFiles("/dev/", "ttyUSB*");
		string [] usbSerialMac = Directory.GetFiles("/dev/", "tty.usbserial*");
		string [] all = Util.AddArrayString(usbSerial, usbSerialMac);
		
		string [] def = Util.StringToStringArray(Constants.ChronopicDefaultPortLinux);
		
		string [] allWithDef = Util.AddArrayString(def, all);

		UtilGtk.ComboUpdate(combo_linux1, allWithDef, Constants.ChronopicDefaultPortLinux);
		UtilGtk.ComboUpdate(combo_linux2, allWithDef, Constants.ChronopicDefaultPortLinux);
		UtilGtk.ComboUpdate(combo_linux3, allWithDef, Constants.ChronopicDefaultPortLinux);
		UtilGtk.ComboUpdate(combo_linux4, allWithDef, Constants.ChronopicDefaultPortLinux);
		
		foreach(ChronopicPortData a in cpd) {
			if(a.Num == 1) {
				combo_linux1.Active = UtilGtk.ComboMakeActive(combo_linux1, a.Port);
				combo_linux1.Changed += new EventHandler (on_combo_changed);
				if(a.Connected) {
					UtilGtk.ComboDelThisValue(combo_linux2, a.Port);
					UtilGtk.ComboDelThisValue(combo_linux3, a.Port);
					UtilGtk.ComboDelThisValue(combo_linux4, a.Port);
				}
			} else if(a.Num == 2) {
				combo_linux2.Active = UtilGtk.ComboMakeActive(combo_linux2, a.Port);
				combo_linux2.Changed += new EventHandler (on_combo_changed);
				if(a.Connected) {
					UtilGtk.ComboDelThisValue(combo_linux3, a.Port);
					UtilGtk.ComboDelThisValue(combo_linux4, a.Port);
				}
			} else if(a.Num == 3) {
				combo_linux3.Active = UtilGtk.ComboMakeActive(combo_linux3, a.Port);
				combo_linux3.Changed += new EventHandler (on_combo_changed);
				if(a.Connected)
					UtilGtk.ComboDelThisValue(combo_linux4, a.Port);
			} else { //4
				combo_linux4.Active = UtilGtk.ComboMakeActive(combo_linux4, a.Port);
				combo_linux4.Changed += new EventHandler (on_combo_changed);
			}
		}
		
		//encoder
		//this reduces the callbacks of combo change
		combo_linux_encoder.Sensitive = false;

		UtilGtk.ComboUpdate(combo_linux_encoder, allWithDef, encoderPort);

		combo_linux_encoder.Changed += new EventHandler (on_combo_changed);

		combo_linux_encoder.Active = UtilGtk.ComboMakeActive(allWithDef, encoderPort);

		combo_linux_encoder.Sensitive = true;
	}
	
	private void on_combo_changed(object o, EventArgs args) {
		ComboBox combo = o as ComboBox;
		if (o == null)
			return;

		//combo is not sensitive when it has been connected
		//this helps to have button_connect with correct sensitiveness after close window
		//also help to not have lots of callbacks coming here about encoder combos
		if(! combo.Sensitive)
			return;

		bool portOk = true;
		if(UtilGtk.ComboGetActive(combo) == Constants.ChronopicDefaultPortWindows ||
				UtilGtk.ComboGetActive(combo) == Constants.ChronopicDefaultPortLinux) 
			portOk = false;

		if (o == combo_linux1 || o == combo_windows1) 
			button_connect_cp1.Sensitive = portOk;
		else if (o == combo_linux2 || o == combo_windows2) 
			button_connect_cp2.Sensitive = portOk;
		else if (o == combo_linux3 || o == combo_windows3) 
			button_connect_cp3.Sensitive = portOk;
		else if (o == combo_linux4 || o == combo_windows4) 
			button_connect_cp4.Sensitive = portOk;
		else if (o == combo_windows_encoder) {
			combo_windows_encoder.Changed -= new EventHandler (on_combo_changed);
			encoderPort = UtilGtk.ComboGetActive(combo);
		} else if (o == combo_linux_encoder) {
			combo_linux_encoder.Changed -= new EventHandler (on_combo_changed);
			encoderPort = UtilGtk.ComboGetActive(combo);
		}
	}
	
	private void info() {
		textview_ports_found_explanation.Buffer = UtilGtk.TextViewPrint(
				Catalog.GetString("If you just plugged Chronopic cable and expected port is not listed, close and open again this window.") + "\n" + 
				Catalog.GetString("If you have problems connecting with Chronopic, press help button.")  
				//saferPorts
				);
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
			fakeConnectionButton.Click();
			pulseEnd();
			Log.Write("dying");
			return false;
		}
		//need to do this, if not it crashes because chronopicConnectionWin gets died by thread ending
		//chronopicConnectionWin = ChronopicConnection.Show();
		//chronopicConnectionWin.Pulse();
		progressbar.Pulse();
		
		Thread.Sleep (50);
		Log.Write(thread.ThreadState.ToString());
		return true;
	}

	private void pulseEnd() {
		button_cancel.Sensitive = false;
		connecting = false;
	}
			
	private void updateChronopicWin(bool state, string message) {
		Log.WriteLine("updateChronopicWin-1");

		//need to do this, if not it crashes because chronopicConnectionWin gets died by thread ending
		//chronopicConnectionWin = ChronopicConnection.Show();

		Log.WriteLine("updateChronopicWin-2");
		if(state) {
			//chronopicConnectionWin.Connected(message);
			sensitivityConnected(message);
			progressbar.Fraction = 1.0;
		}
		else
			//chronopicConnectionWin.Disconnected(message);
			sensitivityDisconnected(message);
		
		needUpdateChronopicWin = false;
	}
	
	private void sensitivityConnected(string message) {
		Log.WriteLine("CONNECTED!!");
		label_title.Text = message;
		label_title.UseMarkup = true;
		button_cancel.Sensitive = false;
	}

	private void sensitivityDisconnected(string message) {
		Log.WriteLine("DISCONNECTED!!");
		label_title.Text = message;
		button_cancel.Sensitive = false;
	}
		
	private void on_button_help_ports_clicked (object o, EventArgs args) {
		new HelpPorts();
	}

	//chronopic init should not touch  gtk, for the threads
	//private Chronopic chronopicInit (out Chronopic myCp, out SerialPort mySp, Chronopic.Plataforma myPS, string myPort, out string returnString, out bool success) 
	private void chronopicInit (out Chronopic myCp, out SerialPort mySp, Chronopic.Plataforma myPS, string myPort, out string returnString, out bool success) 
	{
		Log.WriteLine ( Catalog.GetString ("starting connection with chronopic") );

		success = true;
		
		Log.WriteLine("chronopicInit-1");		
		Log.WriteLine(string.Format("chronopic port: {0}", myPort));
		mySp = new SerialPort(myPort);
		try {
			mySp.Open();
			Log.WriteLine("chronopicInit-2");		
			//-- Create chronopic object, for accessing chronopic
			myCp = new Chronopic(mySp);
			
			Log.WriteLine("chronopicInit-2.1");		
			myCp.Flush();
			
			//if myCp has been cancelled
			if(myCp.AbortFlush) {
				Log.WriteLine("chronopicInit-2.2 cancelled");
				success = false;
				myCp = new Chronopic(); //fake constructor
			} else {
				Log.WriteLine("chronopicInit-3");		
				//on windows, this check make a crash 
				//i think the problem is: as we don't really know the Timeout on Windows (.NET) and this variable is not defined on chronopic.cs
				//the Read_platform comes too much soon (when cp is not totally created), and this makes crash

				//-- Obtener el estado inicial de la plataforma

				bool ok=false;
				Log.WriteLine("chronopicInit-4");		
				do {
					Log.WriteLine("chronopicInit-5");		
					ok=myCp.Read_platform(out myPS);
					Log.WriteLine("chronopicInit-6");		
				} while(! ok && ! cancelledByUser);
				Log.WriteLine("chronopicInit-7");		
				if (!ok) {
					//-- Si hay error terminar
					Log.WriteLine(string.Format("Error: {0}", myCp.Error));
					success = false;
				}
			}
		} catch {
			Log.WriteLine("chronopicInit-2.a catched");
			success = false;
			myCp = new Chronopic(); //fake constructor
		}
			
		returnString = "";
		if(success) {
			if(currentCp == 1)
				connected = true;
			returnString = string.Format(Catalog.GetString("<b>Connected</b> to Chronopic on port: {0}"), myPort);
		}
		if(! success) {
			returnString = Catalog.GetString("Problems communicating to chronopic.");
			if(currentCp == 1) {
				returnString += " " + Catalog.GetString("Changed platform to 'Simulated'");
				returnString += Catalog.GetString("\n\nWe recommend to remove and connect USB cable.");
			}

			//this will raise on_radiobutton_simulated_ativate and 
			//will put cpRunning to false, and simulated to true and cp.Close()
			if(currentCp == 1) {
				connected = false;
			}
		}
	}
	
	private void on_checkbutton_multi_show_clicked(object o, EventArgs args) {
		table_multi_chronopic.Visible = checkbutton_multi_show.Active;
	}

	private void on_button_connect_cp_clicked (object o, EventArgs args) {
		if (o == null)
			return;

		if(isWindows){
			if (o == button_connect_cp1) 
				((ChronopicPortData) cpd[0]).Port = UtilGtk.ComboGetActive(combo_windows1);
			else if (o == button_connect_cp2) 
				((ChronopicPortData) cpd[1]).Port = UtilGtk.ComboGetActive(combo_windows2);
			else if (o == button_connect_cp3) 
				((ChronopicPortData) cpd[2]).Port = UtilGtk.ComboGetActive(combo_windows3);
			else if (o == button_connect_cp4) 
				((ChronopicPortData) cpd[3]).Port = UtilGtk.ComboGetActive(combo_windows4);
		}
		else {
			if (o == button_connect_cp1) 
				((ChronopicPortData) cpd[0]).Port = UtilGtk.ComboGetActive(combo_linux1);
			else if (o == button_connect_cp2) 
				((ChronopicPortData) cpd[1]).Port = UtilGtk.ComboGetActive(combo_linux2);
			else if (o == button_connect_cp3) 
				((ChronopicPortData) cpd[2]).Port = UtilGtk.ComboGetActive(combo_linux3);
			else if (o == button_connect_cp4) 
				((ChronopicPortData) cpd[3]).Port = UtilGtk.ComboGetActive(combo_linux4);
		}
		
		if (o == button_connect_cp1) 
			currentCp = 1;
		else if (o == button_connect_cp2) 
			currentCp = 2;
		else if (o == button_connect_cp3) 
			currentCp = 3;
		else // if (o == button_connect_cp4) 
			currentCp = 4;

		prepareChronopicConnection();
	}
	
	private void on_button_help_clicked (object o, EventArgs args) {
		Log.WriteLine("HELP");
		new HelpPorts();
	}
	
	public void SerialPortsClose() {
		Console.WriteLine("Closing sp");
		sp.Close();
	}


	void prepareChronopicConnection() {
		frame_connection.Visible = true;
		
		label_title.Text = Catalog.GetString("Please touch the platform or click Chronopic <i>TEST</i> button");
		label_title.UseMarkup = true;
			
		button_cancel.Sensitive = true;
		cancelledByUser = false;
		
		fakeConnectionButton = new Gtk.Button();
		fakeConnectionButton.Clicked += new EventHandler(on_chronopic_detection_ended);

		connecting = true;
		needUpdateChronopicWin = false;
		thread = new Thread(new ThreadStart(waitChronopicStart));
		GLib.Idle.Add (new GLib.IdleHandler (PulseGTK));
		thread.Start(); 
	}

	static Chronopic cpDoing;	
	protected void waitChronopicStart () 
	{
		if(currentCp == 1) {
		//	simulated = false;
		//	SqlitePreferences.Update("simulated", simulated.ToString(), false);
			if(connected)
				return;
		}
	
		SerialPort sp2;
		SerialPort sp3;
		SerialPort sp4;

		string message = "";
		string myPort = "";
		bool success = false;
			
		if(currentCp == 1) {
			myPort = ((ChronopicPortData) cpd[0]).Port;
			cpDoing = cp;
			chronopicInit(out cpDoing, out sp, platformState, myPort, out message, out success);
			cp = cpDoing;
			if(success) {
				((ChronopicPortData) cpd[0]).Connected=true;
				button_connect_cp1.Sensitive = false;
				image_cp1_no.Hide();
				image_cp1_yes.Show();
			
				if(isWindows) {
					combo_windows1.Sensitive = false;
					combo_windows2.Sensitive = true;
					UtilGtk.ComboDelThisValue(combo_windows2, myPort);
					combo_windows2.Active = 0; //first option
					UtilGtk.ComboDelThisValue(combo_windows3, myPort);
					combo_windows3.Active = 0;
					UtilGtk.ComboDelThisValue(combo_windows4, myPort);
					combo_windows4.Active = 0;
				} else {
					combo_linux1.Sensitive = false;
					combo_linux2.Sensitive = true;
					UtilGtk.ComboDelThisValue(combo_linux2, myPort);
					combo_linux2.Active = 0; //first option
					UtilGtk.ComboDelThisValue(combo_linux3, myPort);
					combo_linux3.Active = 0;
					UtilGtk.ComboDelThisValue(combo_linux4, myPort);
					combo_linux4.Active = 0;
				}
			}
		}
		else if(currentCp == 2) {
			myPort = ((ChronopicPortData) cpd[1]).Port;
			cpDoing = cp2;
			chronopicInit(out cpDoing, out sp2, platformState2, myPort, out message, out success);
			cp2 = cpDoing;
			if(success) {
				((ChronopicPortData) cpd[1]).Connected=true;
				button_connect_cp2.Sensitive = false;
				image_cp2_no.Hide();
				image_cp2_yes.Show();
			
				if(isWindows) {
					combo_windows2.Sensitive = false;
					combo_windows3.Sensitive = true;
					UtilGtk.ComboDelThisValue(combo_windows3, myPort);
					combo_windows3.Active = 0;
					UtilGtk.ComboDelThisValue(combo_windows4, myPort);
					combo_windows4.Active = 0;
				} else {
					combo_linux2.Sensitive = false;
					combo_linux3.Sensitive = true;
					UtilGtk.ComboDelThisValue(combo_linux3, myPort);
					combo_linux3.Active = 0;
					UtilGtk.ComboDelThisValue(combo_linux4, myPort);
					combo_linux4.Active = 0;
				}
			}
		}
		else if(currentCp == 3) {
			myPort = ((ChronopicPortData) cpd[2]).Port;
			cpDoing = cp3;
			chronopicInit(out cpDoing, out sp3, platformState3, myPort, out message, out success);
			cp3 = cpDoing;
			if(success) {
				((ChronopicPortData) cpd[2]).Connected=true;
				button_connect_cp3.Sensitive = false;
				image_cp3_no.Hide();
				image_cp3_yes.Show();
			
				if(isWindows) {
					combo_windows3.Sensitive = false;
					combo_windows4.Sensitive = true;
					UtilGtk.ComboDelThisValue(combo_windows4, myPort);
					combo_windows4.Active = 0;
				} else {
					combo_linux3.Sensitive = false;
					combo_linux4.Sensitive = true;
					UtilGtk.ComboDelThisValue(combo_linux4, myPort);
					combo_linux4.Active = 0;
				}
			}
		}
		else if(currentCp == 4) {
			myPort = ((ChronopicPortData) cpd[3]).Port;
			cpDoing = cp4;
			chronopicInit(out cpDoing, out sp4, platformState4, myPort, out message, out success);
			cp4 = cpDoing;
			if(success) {
				((ChronopicPortData) cpd[3]).Connected=true;
				button_connect_cp4.Sensitive = false;
				image_cp4_no.Hide();
				image_cp4_yes.Show();
			
				if(isWindows) 
					combo_windows4.Sensitive = false;
				else 
					combo_linux4.Sensitive = false;
			}
		}
		

		Log.WriteLine(string.Format("wait_chronopic_start {0}", message));
			
		if(success) {
			Util.PlaySound(Constants.SoundTypes.GOOD, volumeOn);
			updateChronopicWinValuesState= true; //connected
			updateChronopicWinValuesMessage= message;
		} else {
			Util.PlaySound(Constants.SoundTypes.BAD, volumeOn);
			updateChronopicWinValuesState= false; //disconnected
			updateChronopicWinValuesMessage= message;
		}

		foreach(ChronopicPortData a in cpd)
			Log.WriteLine(a.Num + ", " + a.Port + ", " + a.Connected);

		fakeWindowDone.Click();
		needUpdateChronopicWin = true;
	}

	private void on_chronopic_detection_ended(object o, EventArgs args) {
		updateChronopicWin(updateChronopicWinValuesState, updateChronopicWinValuesMessage);
	}


	private void on_button_cancel_clicked (object o, EventArgs args) {
		Log.WriteLine("cancelled-----");
		//fakeButtonCancelled.Click(); //just to show message of crashing on windows exiting
		
		button_cancel.Sensitive = false;
		
		cpDoing.AbortFlush = true;
		cancelledByUser = true;

		//kill the chronopicInit function that is waiting event 
		//thread.Abort();
		//http://stackoverflow.com/questions/2853072/thread-does-not-abort-on-application-closing
		//Log.Write(thread.ThreadState.ToString());
		//thread.IsBackground = true;
		
		//try to solve windows problems when a chronopic detection was cancelled
		//Log.Write(thread.ThreadState.ToString());
		//thread.Join(1000);
		//Log.Write(thread.ThreadState.ToString());

		
		updateChronopicWinValuesState= false; //disconnected
		updateChronopicWinValuesMessage= Catalog.GetString("Cancelled by user");
		needUpdateChronopicWin = true;
	}
	
	void on_button_close_clicked (object o, EventArgs args)
	{
		if(connecting)
			button_cancel.Click();

		Log.WriteLine("CLOSE");
		fakeWindowDone.Click();
		ChronopicWindowBox.chronopic_window.Hide();
	}

	void on_delete_event (object o, DeleteEventArgs args)
	{
		//nice: this makes windows no destroyed, then it works like button_close
		fakeWindowDone.Click();
		
		if(connecting)
			button_cancel.Click();

		args.RetVal = true;
		ChronopicWindowBox.chronopic_window.Hide();
	}

	public bool IsConnected(int numCP) {
		//int count = 1;
		//foreach(ChronopicPortData a in cpd) 
		//	Log.WriteLine(a.Num + ", " + a.Port + ", " + a.Connected);
		return ((ChronopicPortData) cpd[numCP]).Connected;
	}

	public int NumConnected() {
		int count = 0;
		foreach(ChronopicPortData a in cpd) 
			if(a.Connected)
				count ++;
		return count;
	}

	public string GetEncoderPort() {
		if(isWindows)
			return UtilGtk.ComboGetActive(combo_windows_encoder);
		else
			return UtilGtk.ComboGetActive(combo_linux_encoder);
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

	//connected to a Chronopic	
	public bool Connected {
		get { return connected; }
	}
	
	public Button FakeWindowDone {
		get { return fakeWindowDone; }
	}

	//public Gtk.Button FakeButtonCancelled {
	//	get { return fakeButtonCancelled; }
	//}

}
