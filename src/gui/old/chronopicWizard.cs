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
//using Mono.Unix;
using System.Collections;

public class ChronopicWizardWindow
{
	[Widget] Gtk.Window chronopic_wizard_win;
	[Widget] Gtk.Notebook notebook_main;
	[Widget] Gtk.Button button_next;

	//tab 0 start
	[Widget] Gtk.RadioButton radio_start_contacts;
	[Widget] Gtk.RadioButton radio_start_encoder;
	[Widget] Gtk.RadioButton radio_start_both;
	
	//tab 1 unplug
	[Widget] Gtk.Label label_unplug;
	[Widget] Gtk.Notebook notebook_unplugged;
	[Widget] Gtk.TextView textview_detected_unplugged;
	
	//tab 2 contacts
	[Widget] Gtk.Notebook notebook_contacts;
	[Widget] Gtk.Button button_done_contacts;
	[Widget] Gtk.HBox hbox_detection_contacts;
	[Widget] Gtk.ProgressBar progressbar_contacts;
	[Widget] Gtk.RadioButton radio_contacts1;
	[Widget] Gtk.RadioButton radio_contacts2;
	[Widget] Gtk.RadioButton radio_contacts3;
	[Widget] Gtk.RadioButton radio_contacts4;
	[Widget] Gtk.RadioButton radio_contacts5;

	//tab 3 encoder
	[Widget] Gtk.Notebook notebook_encoder;
	[Widget] Gtk.Button button_done_encoder;
	[Widget] Gtk.HBox hbox_detection_encoder;
	[Widget] Gtk.ProgressBar progressbar_encoder;
	[Widget] Gtk.RadioButton radio_encoder1;
	[Widget] Gtk.RadioButton radio_encoder2;
	[Widget] Gtk.RadioButton radio_encoder3;
	[Widget] Gtk.RadioButton radio_encoder4;
	[Widget] Gtk.RadioButton radio_encoder5;

	static ChronopicWizardWindow ChronopicWizardWindowBox;

	public Gtk.Button FakeButtonChronopicWizardFinished;
	public string PortContacts;
	public string PortEncoder;

	private ArrayList portsAlreadyDetected;

	ChronopicWizardWindow() 
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronopic_wizard_win.glade", "chronopic_wizard_win", "chronojump");
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
		FakeButtonChronopicWizardFinished = new Gtk.Button();
		PortContacts = "";
		PortEncoder = "";

		portsAlreadyDetected = new ArrayList(0);

		notebook_main.CurrentPage = 0;
		notebook_unplugged.CurrentPage = 0;
		notebook_contacts.CurrentPage = 0;
		notebook_encoder.CurrentPage = 0;
		
		button_done_contacts.Sensitive = true;
		hbox_detection_contacts.Sensitive = false;
		progressbar_contacts.Text = "";
		
		radio_contacts1.Visible = false;
		radio_contacts2.Visible = false;
		radio_contacts3.Visible = false;
		radio_contacts4.Visible = false;
		radio_contacts5.Visible = false;
		
		button_done_encoder.Sensitive = true;
		hbox_detection_encoder.Sensitive = false;
		progressbar_encoder.Text = "";
		
		radio_encoder1.Visible = false;
		radio_encoder2.Visible = false;
		radio_encoder3.Visible = false;
		radio_encoder4.Visible = false;
		radio_encoder5.Visible = false;

		//button_next.Label = Catalog.GetString("Next");
		button_next.Label = "Next";
		button_next.Sensitive = true;
	}

	ArrayList detectPorts()
	{
		//detect ports
		string [] ports = ChronopicPorts.GetPorts();

		//get only new ports
		ArrayList portsNew = new ArrayList(0);
		foreach (string pNew in ports) 
			if(! Util.FoundInArrayList(portsAlreadyDetected, pNew)) 
			{
				portsAlreadyDetected.Add(pNew);
				portsNew.Add(pNew);
			}

		return portsNew;
	}

	void assignPorts (ArrayList ports, Gtk.RadioButton radio1, Gtk.RadioButton radio2, 
			Gtk.RadioButton radio3, Gtk.RadioButton radio4, Gtk.RadioButton radio5)
	{
		int count = 1;
		foreach (string port in ports) {
			switch(count) {
				case 1:
					radio1.Label = port;
					radio1.Visible = true;
					break;
				case 2:
					radio2.Label = port;
					radio2.Visible = true;
					break;
				case 3:
					radio3.Label = port;
					radio3.Visible = true;
					break;
				case 4:
					radio4.Label = port;
					radio4.Visible = true;
					break;
				case 5:
					radio5.Label = port;
					radio5.Visible = true;
					break;
			}
			count ++;
		}
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
			ArrayList newPorts = detectPorts();
			if(newPorts.Count > 0) {
				progressbarContinue = false;
				detectedContacts(newPorts);
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
			ArrayList newPorts = detectPorts();
			if(newPorts.Count > 0) {
				progressbarContinue = false;
				detectedEncoder(newPorts);
				return false;
			}
		} else
			progressbarCount ++;

		progressbar_encoder.Pulse();
		return true;
	}      

	//gui for contacts detection ----

	void on_button_done_unplugged_clicked (object o, EventArgs args)
	{
		ArrayList newPorts = detectPorts();
		
		string messageDetected = "";
		string sep = "";
		foreach (string port in newPorts) {
			messageDetected += sep + port;
			sep = "\n";
		}
		if(messageDetected == "")
			messageDetected = "None";
		
		textview_detected_unplugged.Buffer.Text = messageDetected;
		
		notebook_unplugged.CurrentPage = 1;
		button_next.Sensitive = true;
	}
	
	void on_button_done_contacts_clicked (object o, EventArgs args)
	{
		button_done_contacts.Sensitive = false;
		hbox_detection_contacts.Sensitive = true;
		//progressbar_contacts.Text = Catalog.GetString("Detecting");
		progressbar_contacts.Text = "Detecting";
		progressbarContinue = true;
		progressbarCount = 0;
		GLib.Timeout.Add(50, new GLib.TimeoutHandler(progressbarContactsDo)); //each 50 ms
	}
	void detectedContacts (ArrayList newPorts)
	{
		progressbarContinue = false;
		hbox_detection_contacts.Sensitive = false;
		assignPorts(newPorts, radio_contacts1, radio_contacts2, 
				radio_contacts3, radio_contacts4, radio_contacts5);
		notebook_contacts.CurrentPage = 1;
		button_next.Sensitive = true;
	}
	void on_button_cancel_contacts_clicked (object o, EventArgs args)
	{
		progressbarContinue = false;
		//progressbar_contacts.Text = Catalog.GetString("Cancelled");
		progressbar_contacts.Text = "Cancelled";
		hbox_detection_contacts.Sensitive = false;
		button_done_contacts.Sensitive = true;
	}
	
	//gui for encoder detection ----
	
	void on_button_done_encoder_clicked (object o, EventArgs args)
	{
		button_done_encoder.Sensitive = false;
		hbox_detection_encoder.Sensitive = true;
		//progressbar_encoder.Text = Catalog.GetString("Detecting");
		progressbar_encoder.Text = "Detecting";
		progressbarContinue = true;
		progressbarCount = 0;
		GLib.Timeout.Add(50, new GLib.TimeoutHandler(progressbarEncoderDo)); //each 50 ms
	}
	void detectedEncoder (ArrayList newPorts)
	{
		progressbarContinue = false;
		hbox_detection_encoder.Sensitive = false;
		assignPorts(newPorts, radio_encoder1, radio_encoder2, 
				radio_encoder3, radio_encoder4, radio_encoder5);
		notebook_encoder.CurrentPage = 1;
		button_next.Sensitive = true;
		
		//button_next.Label = Catalog.GetString("Finish");
		button_next.Label = "Finish";
	}
	void on_button_cancel_encoder_clicked (object o, EventArgs args)
	{
		progressbarContinue = false;
		//progressbar_encoder.Text = Catalog.GetString("Cancelled");
		progressbar_encoder.Text = "Cancelled";
		hbox_detection_encoder.Sensitive = false;
		button_done_encoder.Sensitive = true;
	}
	

	void on_button_next_clicked (object o, EventArgs args)
	{
		int advancePages = 1;

		if(notebook_main.CurrentPage == 0) 
		{
			//from page 0 to page 1 show unplug message
			/*
			int numCPs = 1;
			if(radio_start_both.Active)
				numCPs = 2;

			label_unplug.Text = Catalog.GetPluralString(
					"Please, unplug Chronopic USB cable.", 
					"Please, unplug Chronopic USB cables.", numCPs);
			*/
			label_unplug.Text = "Please, unplug Chronopic USB cable/s.";

			button_next.Sensitive = false; //unsensitive until click on Done
		}
		else if(notebook_main.CurrentPage == 1) 
		{
			//from page 1 to page 2 detect contacts
			button_next.Sensitive = false; //unsensitive until click on Done

			if(radio_start_contacts.Active) {
				//if there will be no encoder, rename Next to Finish
				//button_next.Label = Catalog.GetString("Finish");
				button_next.Label = "Finish";
			}
			else if(radio_start_encoder.Active) {
				//if there will be no contacts, jump to encoder page
				advancePages = 2;
			}
		}
		else if(notebook_main.CurrentPage == 2) 
		{
			//from page 2 to page 3 detect encoder
			//exit if there's no encoder
			if(radio_start_contacts.Active)
				finishWizard();
			else {
				button_next.Sensitive = false; //unsensitive until click on Done
				
				//rename Next to Finish
				//button_next.Label = Catalog.GetString("Finish");
				button_next.Label = "Finish";
			}
		}
		else if(notebook_main.CurrentPage == 3) {
			finishWizard();
		}

		//change the page		
		notebook_main.CurrentPage += advancePages;
	}
			
	private void finishWizard()
	{
		if(radio_start_contacts.Active || radio_start_both.Active)
			PortContacts = readSelectedRadioButton(radio_contacts1, radio_contacts2,
					radio_contacts3, radio_contacts4, radio_contacts5);
		
		if(radio_start_encoder.Active || radio_start_both.Active)
			PortEncoder = readSelectedRadioButton(radio_encoder1, radio_encoder2,
					radio_encoder3, radio_encoder4, radio_encoder5);
		
		//exiting using finish (next) button
		FakeButtonChronopicWizardFinished.Click();
	}
			
	private string readSelectedRadioButton(Gtk.RadioButton radio1, Gtk.RadioButton radio2, 
			Gtk.RadioButton radio3, Gtk.RadioButton radio4, Gtk.RadioButton radio5) 
	{
		if(radio1.Active)
			return radio1.Label;
		else if(radio2.Active)
			return radio2.Label;
		else if(radio3.Active)
			return radio3.Label;
		else if(radio4.Active)
			return radio4.Label;
		else //if(radio5.Active)
			return radio5.Label;
	}

	public void HideAndNull () 
	{
		ChronopicWizardWindowBox.chronopic_wizard_win.Hide();
		ChronopicWizardWindowBox = null;
	}

	void on_button_cancel_clicked (object o, EventArgs args)
	{
		HideAndNull();
	}
	
	void on_delete_event (object o, DeleteEventArgs args)
	{
		progressbarContinue = false;
		HideAndNull();
	}
}

