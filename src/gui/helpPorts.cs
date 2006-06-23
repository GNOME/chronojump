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
 * Xavier de Blas: 
 * http://www.xdeblas.com, http://www.deporteyciencia.com (parleblas)
 */

using System;
using Gtk;
using Glade;
using System.IO.Ports;
using Mono.Unix;

public class HelpPorts
{
	[Widget] Gtk.Dialog dialog_help_ports;
	[Widget] Gtk.TextView textview_info;
	[Widget] Gtk.TextView textview_detected;

	public HelpPorts ()
	{
		Glade.XML gladeXML;
		try {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "dialog_help_ports", null);
		} catch {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade.chronojump.glade", "dialog_help_ports", null);
		}
		
		gladeXML.Autoconnect(this);
	
		string messageInfo;
		string messageDetected = "";
		
		if(Util.IsWindows()) {
			messageInfo = Constants.PortNamesWindows;

			string jumpLine = "";
			foreach (string s in SerialPort.GetPortNames()) {
				messageDetected += jumpLine + s;
				jumpLine = "\n";
			}
		} else {
			messageInfo = Constants.PortNamesLinux;
				
			messageDetected = string.Format(Catalog.GetString("Auto-Detection currently disabled on GNU/Linux"));
		}
			
		TextBuffer tb1 = new TextBuffer (new TextTagTable());
		tb1.SetText(messageInfo);
		textview_info.Buffer = tb1;
		
		TextBuffer tb2 = new TextBuffer (new TextTagTable());
		tb2.SetText(messageDetected);
		textview_detected.Buffer = tb2;
	}
				

	public void on_button_close_clicked (object obj, EventArgs args) {
		dialog_help_ports.Destroy ();
	}

	private void on_delete_event (object o, DeleteEventArgs args) {
		dialog_help_ports.Destroy ();
	}
}

