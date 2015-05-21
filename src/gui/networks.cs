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
 * Copyright (C) 2004-2015   Xavier de Blas <xaviblas@gmail.com> 
 */


using System;
using Gtk;
using Gdk;
using Glade;
using System.IO.Ports;
using System.IO; //"File" things
using System.Collections; //ArrayList
using System.Collections.Generic; //List
	
public partial class ChronoJumpWindow 
{
	//RFID
	[Widget] Gtk.Box hbox_rfid;
	[Widget] Gtk.Label label_rfid;

	private enum linuxTypeEnum { NOTLINUX, LINUX, RASPBERRY, NETWORKS }
	private linuxTypeEnum linuxType;
	
	//networks is a raspberry connected to the server	
	private static linuxTypeEnum getLinuxTypes() 
	{
		if(UtilAll.GetOSEnum() == UtilAll.OperatingSystems.LINUX) 
		{
			if(Util.FileExists(Util.GetChronojumpNetworksFile()))
				return(linuxTypeEnum.NETWORKS);
			else if(Util.FileExists(Util.GetChronojumpRaspberryFile()))
				return(linuxTypeEnum.RASPBERRY);
			else
				return(linuxTypeEnum.LINUX);
		}
		return(linuxTypeEnum.NOTLINUX);
	}

	
	private void raspberryOrNetworksInit() {
		linuxType = getLinuxTypes();
	
		if(linuxType == linuxTypeEnum.RASPBERRY || linuxType == linuxTypeEnum.NETWORKS) {
			alignment_video_encoder.Visible = false;
			//TODO: put the no connect video here.
		}

		if(linuxType == linuxTypeEnum.NETWORKS) {
			//mostrar directament el power
			select_menuitem_mode_toggled(menuitem_modes.POWER);
			
			//no mostrar menu
			main_menu.Visible = false;
			
			//no mostrar persones
			vbox_persons.Visible = false;
			//TODO: rfid can be here, also machine, maybe weight, other features
			//time, gym, ...

			//show rfid
			hbox_rfid.Visible = true;

			//to test display, just make sensitive the top controls, but beware there's no session yet and no person
			notebook_sup.Sensitive = true;
			hbox_encoder_sup_capture_analyze.Sensitive = true;
			notebook_encoder_sup.Sensitive = false;
		}

		//si es rapsberry o si es networks cal desactivar camera
		
		//maximize now, at the end of tweaks
		if(linuxType == linuxTypeEnum.RASPBERRY || linuxType == linuxTypeEnum.NETWORKS)
			app1.Maximize();
	}

	//rfid
	private void rfid_test() {
		Networks networks = new Networks();
		networks.Test();
	}
	void on_button_rfid_read_clicked (object o, EventArgs args) {
		string file = "/tmp/chronojump_rfid.txt";

		if(Util.FileExists(file))
			label_rfid.Text = Util.ReadFile(file, true);
	}

}
