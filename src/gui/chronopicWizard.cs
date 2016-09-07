/*
 * This file is part of ChronoJump
 *
 * ChronoJump is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or   
 *    (at your option) any later version.
 *    
 * ChronoJump is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
 *    GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 *  Copyright (C) 2004-2016   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.IO;   //for Path
using System.IO.Ports;
using Gtk;
using Glade;
using Mono.Unix;

public class ChronopicWizardWindow
{
	[Widget] Gtk.Window chronopic_wizard_win;
	[Widget] Gtk.Notebook notebook_main;
	[Widget] Gtk.Button button_cancel;
	[Widget] Gtk.Button button_next;

	//tab 0 start
	[Widget] Gtk.RadioButton radio_contacts;
	[Widget] Gtk.RadioButton radio_encoder;
	[Widget] Gtk.RadioButton radio_both;
	
	//tab 1 unplug
	[Widget] Gtk.Label label_unplug;
	[Widget] Gtk.Notebook notebook_unplugged;
	[Widget] Gtk.Button button_done_unplugged;
	[Widget] Gtk.TextView textview_detected_unplugged;
	
	//tab 2 contacts
	[Widget] Gtk.Frame frame_detection_contacts;
	[Widget] Gtk.Notebook notebook_contacts;
	[Widget] Gtk.Button button_done_contacts;
	[Widget] Gtk.ProgressBar progressbar_contacts;
	[Widget] Gtk.TextView textview_detected_contacts;

	//tab 3 encoder
	[Widget] Gtk.Frame frame_detection_encoder;
	[Widget] Gtk.Notebook notebook_encoder;
	[Widget] Gtk.Button button_done_encoder;
	[Widget] Gtk.ProgressBar progressbar_encoder;
	[Widget] Gtk.TextView textview_detected_encoder;

	static ChronopicWizardWindow ChronopicWizardWindowBox;

	public string portContacts;
	public string portEncoder;

	ChronopicWizardWindow() 
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "chronopic_wizard_win", "chronojump");
		gladeXML.Autoconnect(this);
		
		//put an icon to window
		UtilGtk.IconWindow(chronopic_wizard_win);
	}
	static public ChronopicWizardWindow Show ()
	{
		if (ChronopicWizardWindowBox == null) {
			ChronopicWizardWindowBox = new ChronopicWizardWindow ();
		}
		
		ChronopicWizardWindowBox.initialize();
		
		ChronopicWizardWindowBox.chronopic_wizard_win.Show ();
		return ChronopicWizardWindowBox;
	}

	void initialize() 
	{
		notebook_main.CurrentPage = 0;
		notebook_unplugged.CurrentPage = 0;
		notebook_contacts.CurrentPage = 0;
		notebook_encoder.CurrentPage = 0;
		
		button_done_contacts.Sensitive = true;
		progressbar_contacts.Text = "";
		
		button_done_encoder.Sensitive = true;
		progressbar_encoder.Text = "";

		button_next.Label = Catalog.GetString("Next");
		button_next.Sensitive = true;
	}

	string detectPorts()
	{
		string [] ports = {""};
		if(UtilAll.IsWindows())
			ports = SerialPort.GetPortNames();
		else
			ports = Directory.GetFiles("/dev/", "ttyUSB*");

		string messageDetected = "";
		string sep = "";
		foreach (string port in ports) {
			messageDetected += sep + port;
			sep = "\n";
		}

		return messageDetected;
	}
	
	bool progressbarContinue = true;
	int progressbarCount;
	
	bool progressbarContactsDo() 
	{ 
		LogB.Information("progressbarContactsDo");
		if (! progressbarContinue) 
			return false;
	
		//will be each 500 ms (because progressbarContactsDo is called each 50 ms)
		if(progressbarCount >= 10) 
		{
			LogB.Information("Wizard detecting contacts");
			
			progressbarCount = 0;
			string detected = detectPorts();
			if(detected != "") {
				progressbarContinue = false;
				detected_contacts(detected);
				return false;
			}
		} else
			progressbarCount ++;

		progressbar_contacts.Pulse();
		return true;
	}      
	
	bool progressbarEncoderDo() 
	{ 
		LogB.Information("progressbarEncoderDo");
		if (! progressbarContinue) 
			return false;
	
		//will be each 500 ms (because progressbarEncoderDo is called each 50 ms)
		if(progressbarCount >= 10) 
		{
			LogB.Information("Wizard detecting encoder");
			
			progressbarCount = 0;
			string detected = detectPorts();
			if(detected != "") {
				progressbarContinue = false;
				detected_encoder(detected);
				return false;
			}
		} else
			progressbarCount ++;

		progressbar_encoder.Pulse();
		return true;
	}      


	void on_button_done_unplugged_clicked (object o, EventArgs args)
	{
		string str = detectPorts();
		if(str == "")
			str = "Correct! Nothing detected";
		textview_detected_unplugged.Buffer.Text = str;
		notebook_unplugged.CurrentPage = 1;
		button_next.Sensitive = true;
	}
	
	void on_button_done_contacts_clicked (object o, EventArgs args)
	{
		button_done_contacts.Sensitive = false;
		progressbar_contacts.Text = "Detecting";
		progressbarContinue = true;
		progressbarCount = 0;
		GLib.Timeout.Add(50, new GLib.TimeoutHandler(progressbarContactsDo)); //each 50 ms
	}
	void detected_contacts (string detected)
	{
		progressbarContinue = false;
		textview_detected_contacts.Buffer.Text = detected;
		notebook_contacts.CurrentPage = 1;
		button_next.Sensitive = true;
	}
	
	void on_button_done_encoder_clicked (object o, EventArgs args)
	{
		button_done_encoder.Sensitive = false;
		progressbar_encoder.Text = "Detecting";
		progressbarContinue = true;
		progressbarCount = 0;
		GLib.Timeout.Add(50, new GLib.TimeoutHandler(progressbarEncoderDo)); //each 50 ms
	}
	void detected_encoder (string detected)
	{
		progressbarContinue = false;
		textview_detected_encoder.Buffer.Text = detected;
		notebook_encoder.CurrentPage = 1;
		button_next.Label = Catalog.GetString("Finish");
		button_next.Sensitive = true;
	}
	

	void on_button_next_clicked (object o, EventArgs args)
	{
		int advancePages = 1;

		if(notebook_main.CurrentPage == 0) 
		{
			//from page 0 to page 1 show unplug message
			int numCPs = 1;
			if(radio_both.Active)
				numCPs = 2;

			label_unplug.Text = Catalog.GetPluralString(
					"Please unplug Chronopic USB cable.", 
					"Please unplug Chronopic USB cables.", numCPs);

			button_next.Sensitive = false; //unsensitive until click on Done
		}
		else if(notebook_main.CurrentPage == 1) 
		{
			//from page 1 to page 2 detect contacts
			button_next.Sensitive = false; //unsensitive until click on Done

			if(radio_contacts.Active) {
				//if there will be no encoder, rename Next to Finish
				button_next.Label = Catalog.GetString("Finish");
			}
			else if(radio_encoder.Active) {
				//if there will be no contacts, jump to encoder page
				advancePages = 2;
			}
		}
		else if(notebook_main.CurrentPage == 2) 
		{
			//from page 2 to page 3 detect encoder
			//exit if there's no encoder
			if(radio_contacts.Active)
				on_button_cancel_clicked(o, args); //TODO
			else {
				button_next.Sensitive = false; //unsensitive until click on Done
				
				//rename Next to Finish
				button_next.Label = Catalog.GetString("Finish");
			}
		}
		else if(notebook_main.CurrentPage == 3) {
			//exiting form encoder page
			on_button_cancel_clicked(o, args); //TODO
		}

		//change the page		
		notebook_main.CurrentPage += advancePages;
	}

	void on_button_cancel_clicked (object o, EventArgs args)
	{
		ChronopicWizardWindowBox.chronopic_wizard_win.Hide();
		ChronopicWizardWindowBox = null;
	}
	
	void on_delete_event (object o, DeleteEventArgs args)
	{
		/*
		 * copied from preferences
		 *
		//do not hide/exit if copyiing
		if (thread != null && thread.IsAlive)
			args.RetVal = true;
		else {
		*/
			ChronopicWizardWindowBox.chronopic_wizard_win.Hide();
			ChronopicWizardWindowBox = null;
			/*
		}
		*/
	}
	

}

