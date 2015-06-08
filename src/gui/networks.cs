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
	
	//better raspberry controls
	[Widget] Gtk.Entry entry_raspberry_extra_weight;
	[Widget] Gtk.Box hbox_encoder_capture_extra_mass_no_raspberry;
	[Widget] Gtk.Box hbox_encoder_capture_extra_mass_raspberry;

	private bool useVideo = true;
	private enum linuxTypeEnum { NOTLINUX, LINUX, RASPBERRY, NETWORKS }
	private linuxTypeEnum linuxType;


	private void configInit() 
	{
		//trying new Config class
		Config config = new Config();
		config.Read();
		LogB.Information("Config:\n" + config.ToString());

		if(config.Maximized)
			app1.Maximize();
		if(config.CustomButtons) {
			hbox_encoder_capture_extra_mass_no_raspberry.Visible = false;
			hbox_encoder_capture_extra_mass_raspberry.Visible = true;
		}
		if(! config.UseVideo) {
			useVideo = false;
			alignment_video_encoder.Visible = false;
		}

		//TODO
		//AutodetectPort
		//RunScriptOnExit

		/*
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
		*/
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

public class Config
{
	public enum AutodetectPortEnum { ACTIVE, DISCARDFIRST, INACTIVE }

	public bool Maximized;
	public bool CustomButtons;
	public bool UseVideo;
	public AutodetectPortEnum AutodetectPort;
	public string RunScriptOnExit;

	public Config()
	{
		Maximized = false;
		CustomButtons = false;
		UseVideo = true;
		AutodetectPort = AutodetectPortEnum.ACTIVE;
		RunScriptOnExit = "";
	}

	public void Read()
	{
		string contents = Util.ReadFile(UtilAll.GetConfigFileName(), false);
		if (contents != null && contents != "") 
		{
			string line;
			using (StringReader reader = new StringReader (contents)) {
				do {
					line = reader.ReadLine ();

					if (line == null)
						break;
					if (line == "" || line[0] == '#')
						continue;

					string [] parts = line.Split(new char[] {'='});
					if(parts.Length != 2)
						continue;

					if(parts[0] == "Maximized" && Util.StringToBool(parts[1]))
						Maximized = true;
					else if(parts[0] == "CustomButtons" && Util.StringToBool(parts[1]))
						CustomButtons = true;
					else if(parts[0] == "UseVideo" && ! Util.StringToBool(parts[1]))
						UseVideo = false;
					else if(parts[0] == "AutodetectPort" && Enum.IsDefined(typeof(AutodetectPortEnum), parts[1]))
						AutodetectPort = (AutodetectPortEnum) 
							Enum.Parse(typeof(AutodetectPortEnum), parts[1]);
					else if(parts[0] == "RunScriptOnExit" && parts[1] != "")
						RunScriptOnExit = parts[1];
				} while(true);
			}
		}
	}

	public override string ToString() {
		return(
				"Maximized = " + Maximized.ToString() + "\n" +
				"CustomButtons = " + CustomButtons.ToString() + "\n" +
				"UseVideo = " + UseVideo.ToString() + "\n" +
				"AutodetectPort = " + AutodetectPort.ToString() + "\n" +
				"RunScriptOnExit = " + RunScriptOnExit.ToString() + "\n"
		      );
	}

	~Config() {}
}
