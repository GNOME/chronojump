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
	[Widget] Gtk.Notebook notebook;
	[Widget] Gtk.Button button_cancel;
	[Widget] Gtk.Button button_next;

	//tab 0
	[Widget] Gtk.RadioButton radio_contacts;
	[Widget] Gtk.RadioButton radio_encoder;
	[Widget] Gtk.RadioButton radio_both;
	
	//tab 1
	[Widget] Gtk.Label label_unplug;
	
	//tab 2
	[Widget] Gtk.TextView textview_detected_unplugged;
	[Widget] Gtk.Frame frame_detection_contacts;
	[Widget] Gtk.Frame frame_detection_encoder;

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
		notebook.CurrentPage = 0;
		button_next.Label = "Next";
	}

	void on_button_next_clicked (object o, EventArgs args)
	{
		//from page 0 to page 1 show unplug message:
		if(notebook.CurrentPage == 0) {
			int numCPs = 1;
			if(radio_both.Active)
				numCPs = 2;

			label_unplug.Text = Catalog.GetPluralString(
					"Please unplug Chronopic USB cable.", 
					"Please unplug Chronopic USB cables.", numCPs);
		}

		//from page 1 to page 2 detect devices:
		if(notebook.CurrentPage == 1) 
		{
			//1 detected serial ports
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
			if(messageDetected == "")
				messageDetected = "None";

			textview_detected_unplugged.Buffer.Text = messageDetected;

			//2 other widgets
			frame_detection_contacts.Visible = (radio_contacts.Active || radio_both.Active);
			frame_detection_encoder.Visible = (radio_encoder.Active || radio_both.Active);
			button_next.Label = "Finish"; //page 2 is last page, "Next" is changed to "finish"
		}
		
		//page 2 is last page, exit
		if(notebook.CurrentPage == 2)
			on_button_cancel_clicked(o, args); //TODO
	

		//change the page		
		notebook.CurrentPage ++;
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

