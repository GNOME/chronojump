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
 * Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Glade;
using System.IO; //"File" things
using System.IO.Ports;
using Mono.Unix;

public class HelpPorts
{
	[Widget] Gtk.Dialog dialog_help_ports;
	[Widget] Gtk.Label label_info;
	[Widget] Gtk.Label label_detected;
	[Widget] Gtk.Label label_help_info;
	[Widget] Gtk.Label label_help_manual;
	[Widget] Gtk.Button button_check_port;
	[Widget] Gtk.Button button_force_port;

	public HelpPorts ()
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "dialog_help_ports.glade", "dialog_help_ports", "chronojump");
		gladeXML.Autoconnect(this);

		//put an icon to window
		UtilGtk.IconWindow(dialog_help_ports);
	
		string messageInfo;
		string messageDetected = "";
		
		
		if(UtilAll.IsWindows()) {
			messageInfo = Constants.PortNamesWindowsStr();

			/*
			 * autodetection disabled on Linux and windows because mono doesn't allow it
			string jumpLine = "";
			foreach (string s in SerialPort.GetPortNames()) {
				messageDetected += jumpLine + s;
				jumpLine = "\n";
			}
			*/
		
			messageDetected = string.Format(Catalog.GetString("Auto-Detection currently disabled"));
		} else {
			messageInfo = Constants.PortNamesLinuxStr();
			messageDetected = UtilAll.DetectPortsLinux(true); //formatting
			button_check_port.Hide();
			button_force_port.Hide();
		}
		
		label_info.Text = messageInfo;
		label_info.UseMarkup = true;
		label_detected.Text = messageDetected;
		label_detected.UseMarkup = true;
	
		if(UtilAll.IsWindows()) {
			label_help_info.Text = 
				Constants.FindDriverWindowsStr() + "\n\n" +
				Catalog.GetString("Ports above COM4 may not work.") + "\n" + 
				Catalog.GetString("If you want a safer port, press:") + "\n" +
				Catalog.GetString("Force Chronopic to port <b>COM1 - COM4</b>'.");
		} else
			label_help_info.Text = Constants.FindDriverOthersStr();

		label_help_info.UseMarkup = true;

		label_help_manual.Text = 
			Catalog.GetString("More information on <b>Chronojump Manual</b>") + "\n" +
			"<i>" + Path.GetFullPath(Util.GetManualDir()) + "</i>\n\n" + 
			Catalog.GetString("Newer versions will be on this site:") +"\n" + 
			"<i>http://chronojump.org/documents.html</i>";
		label_help_manual.UseMarkup = true;
	}
	
	private void on_button_check_port_clicked (object o, EventArgs args)
	{
		new DialogMessage(Constants.MessageTypes.HELP,
				Catalog.GetString("Check Chronopic port") + "\n\n" +
				"1 " + Catalog.GetString("Click with the right button on <i>MyPC</i> icon at desktop or Start Menu.") + "\n" +
				"  " + Catalog.GetString("Note <i>MyPC</i> can be called <i>System</i> in Windows Vista.") + "\n" +
				"  " + Catalog.GetString("Note also, you maybe have a <i>MyPC</i> icon that has no <i>properties</i> because can be a link to the real <i>MyPC</i> icon, if you cannot see <i>Properties</i>, please use another MyPC icon.") + "\n" +
				"2 " + Catalog.GetString("Select <i>properties</i> (last option).") +  "\n" +
				"3 " + Catalog.GetString("Go to <i>hardware</i>.") +  "\n" +
				"4 " + Catalog.GetString("Select <i>administrate dispositives</i>. It's first button.") +  "\n" +
				"5 " + Catalog.GetString("Click on the '+' at left of COM and LPT ports.") +  "\n" +
				"6 " + Catalog.GetString("The port name will be what it's written like COM? on the USB-serial line.") +  "\n" + "  " + Catalog.GetString("Eg: if it's written COM7, then you should write COM7 at Chronopic window.") + "\n\n" +
				Catalog.GetString("If it doesn't work, try to force to COM1 - COM4, as it's explained on parent window.")
				);
	}

	private void on_button_force_port_clicked (object o, EventArgs args)
	{
		new DialogMessage(Constants.MessageTypes.HELP,
				Catalog.GetString("Force Chronopic port to COM1 - COM4") + "\n\n" +
				"1 " + Catalog.GetString("Find the port as explained at <i>Check Chronopic port</i>.") + "\n" +
				"2 " + Catalog.GetString("At the line where port is shown right click and select <i>properties</i> (last option).") +  "\n" +
				"3 " + Catalog.GetString("Go to <i>Port configurations</i>.") +  "\n" +
				"4 " + Catalog.GetString("Go to <i>Advanced options</i>.") +  "\n" +
				"5 " + Catalog.GetString("Select COM1, COM2, COM3 or COM4 on the list shown on that window.") + "\n" +
				Catalog.GetString("If COM1 - COM4 are <i>used</i>, then select unused ports below 10.") + "\n" + Catalog.GetString("If doesn't work, try to select the COM1 - COM4 (normally they are not really <i>used</i>).")
				);
	}

	public void on_button_close_clicked (object obj, EventArgs args) {
		dialog_help_ports.Destroy ();
	}

	private void on_delete_event (object o, DeleteEventArgs args) {
		dialog_help_ports.Destroy ();
	}
}

