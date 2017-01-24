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
 * Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */


using System;
using Gtk;
using Gdk;
using Glade;
using System.IO.Ports;
using System.IO; //"File" things
using System.Collections; //ArrayList
using System.Collections.Generic; //List
using System.Diagnostics; //Process
	
public partial class ChronoJumpWindow 
{
	//custom buttons
	[Widget] Gtk.HBox hbox_encoder_analyze_signal_or_curves;
			
	//RFID
	[Widget] Gtk.HBox hbox_rfid;
	[Widget] Gtk.Button button_rfid_start;
	[Widget] Gtk.Label label_rfid;
	
	//better raspberry controls
	[Widget] Gtk.Entry entry_raspberry_extra_weight;
	[Widget] Gtk.Box hbox_encoder_capture_extra_mass_no_raspberry;
	[Widget] Gtk.Box hbox_encoder_capture_extra_mass_raspberry;
	[Widget] Gtk.HBox hbox_encoder_im_weights_n;
	
	//config.EncoderNameAndCapture
	[Widget] Gtk.Box hbox_encoder_person;
	[Widget] Gtk.Label label_encoder_person_name;
	[Widget] Gtk.Button button_encoder_person_change;

	//config.EncoderCaptureShowOnlyBars
	[Widget] Gtk.Notebook notebook_encoder_capture_main;
	[Widget] Gtk.VBox vbox_treeview_encoder_at_second_page;
	
	//shown when menu is hidden
	[Widget] Gtk.Button button_preferences_not_menu;


	//variables used on gui/chronojump.cs
	private bool useVideo = true;
	private bool sessionIsUnique = false;

	private enum linuxTypeEnum { NOTLINUX, LINUX, RASPBERRY, NETWORKS }
	private bool encoderUpdateTreeViewWhileCapturing = true;
		
	private void configInit() 
	{
		//trying new Config class
		Config config = new Config();
		config.Read();
		LogB.Information("Config:\n" + config.ToString());
			
		/*
		 * TODO: do an else to any option
		 * is good to do the else here because user can import a configuration at any time 
		 * and things need to be restored to default position in glade
		 *
		 * But note this has to be executed only if it has changed!!
		 */

		if(config.Maximized)
			app1.Maximize();
		if(config.CustomButtons) {
			
			//---- capture tab ----
			
			hbox_encoder_capture_extra_mass_no_raspberry.Visible = false;
			hbox_encoder_capture_extra_mass_raspberry.Visible = true;
		
			button_encoder_select.HeightRequest = 40;
			//this will make all encoder capture controls taller	
			button_encoder_capture.SetSizeRequest(125,60);

			spin_encoder_im_weights_n.Visible = false;
			hbox_encoder_im_weights_n.Visible = true;
			
			//---- analyze tab ----

			hbox_encoder_analyze_signal_or_curves.HeightRequest = 40;
			button_encoder_analyze.SetSizeRequest(120,40);
		}
		if(! config.UseVideo) {
			useVideo = false;
			alignment_video_encoder.Visible = false;
		}
		
		//show only power
		if(config.OnlyEncoderGravitatory)
			select_menuitem_mode_toggled(Constants.Menuitem_modes.POWERGRAVITATORY);
		else if(config.OnlyEncoderInertial)
			select_menuitem_mode_toggled(Constants.Menuitem_modes.POWERINERTIAL);
		
		if(config.EncoderCaptureShowOnlyBars)
		{
			//attention: this makes encoder_capture_signal_drawingarea == null
			vpaned_encoder_capture_video_and_set_graph.Visible = false;
			
			vpaned_encoder_main.Remove(alignment_treeview_encoder_capture_curves);
			vbox_treeview_encoder_at_second_page.PackStart(alignment_treeview_encoder_capture_curves);
			notebook_encoder_capture_main.ShowTabs = true;
		} else {
			/*
			 * is good to do the else here because user can import a configuration at any time 
			 * and things need to be restored to default position in glade
			 *
			 * But note this has to be executed only if it has changed!!
			 */
			/*
			notebook_encoder_capture_main.ShowTabs = false;
			vbox_treeview_encoder_at_second_page.Remove(alignment_treeview_encoder_capture_curves);
			vpaned_encoder_main.PackStart(alignment_treeview_encoder_capture_curves);
			*/
		}
		
		encoderUpdateTreeViewWhileCapturing = config.EncoderUpdateTreeViewWhileCapturing;
		
		if(config.PersonWinHide) {
			//vbox_persons.Visible = false;
			notebook_session_person.Visible = false;
			hbox_encoder_person.Visible = true;
		}
		
		if(config.EncoderAnalyzeHide) {
			hbox_encoder_sup_capture_analyze_two_buttons.Visible = false;
		}

		if(config.SessionMode == Config.SessionModeEnum.UNIQUE)	
		{
			sessionIsUnique = true;
			main_menu.Visible = false;
			button_preferences_not_menu.Visible = true;

			if(! Sqlite.Exists(false, Constants.SessionTable, "session")) {
				//this creates the session and inserts at DB
				currentSession = new Session(
						"session", "", DateTime.Today,	//name, place, dateTime
						Constants.SportUndefinedID, Constants.SpeciallityUndefinedID, Constants.LevelUndefinedID,
						"", Constants.ServerUndefinedID); //comments, serverID
			} else
				currentSession = SqliteSession.SelectByName("session");
			
			on_load_session_accepted();
		}

		//TODO
		//RunScriptOnExit

		/*
		if(linuxType == linuxTypeEnum.NETWORKS) {
			//mostrar directament el power
			select_menuitem_mode_toggled(Constants.Menuitem_modes.POWER);
			
			//no mostrar menu
			main_menu.Visible = false;
			
			//no mostrar persones
			//vbox_persons.Visible = false;
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

		hbox_rfid.Visible = (UtilAll.GetOSEnum() == UtilAll.OperatingSystems.LINUX);
	}

	//rfid
	private void rfid_test() {
		Networks networks = new Networks();
		networks.Test();
	}
	void on_button_rfid_read_clicked (object o, EventArgs args)
	{
		string filePath = Util.GetRFIDCapturedFile();

		if(Util.FileExists(filePath))
			label_rfid.Text = Util.ReadFile(filePath, true);
	}

	Process processRFIDcapture;
	void on_button_rfid_start_clicked (object o, EventArgs args)
	{
		string script_path = Util.GetRFIDCaptureScript();

		if(! File.Exists(script_path))
		{
			LogB.Debug ("ExecuteProcess does not exist parameter: " + script_path);
			label_rfid.Text = "Error starting rfid capture";
			return;
		}

		string filePath = Util.GetRFIDCapturedFile();
		Util.FileDelete(filePath);


		// ---- start process ----
		//
		// on Windows will be different, but at the moment RFID is only supported on Linux (Raspberrys)
		// On Linux and OSX we execute Python and we pass the path to the script as a first argument

		string executable = "python";         // TODO: check if ReadChronojump.py works on Python 2 and Python 3

		List<string> parameters = new List <string> ();
		// first argument of the Python: the path to the script
		parameters.Insert (0, script_path);


		processRFIDcapture = new Process();
		bool calledOk = ExecuteProcess.RunAtBackground (processRFIDcapture, executable, parameters);
		if(calledOk) {
			button_rfid_start.Sensitive = false;
			label_rfid.Text = "...";
		} else
			label_rfid.Text = "Error starting rfid capture";

		// ----- process is launched

		//create a new FileSystemWatcher and set its properties.
		FileSystemWatcher watcher = new FileSystemWatcher();
		watcher.Path = Path.GetDirectoryName(filePath);
		watcher.Filter = Path.GetFileName(filePath);

		//add event handlers.
		watcher.Changed += new FileSystemEventHandler(rfid_watcher_changed);

		//start watching
		watcher.EnableRaisingEvents = true;

		//also perform an initial search
		rfid_read();
	}

	private void rfid_watcher_changed(object source, FileSystemEventArgs e)
	{
		rfid_read();
	}

	private void rfid_read()
	{
		string filePath = Util.GetRFIDCapturedFile();

		LogB.Information("Changed file: " +  filePath);

		if(Util.FileExists(filePath))
			label_rfid.Text = Util.ReadFile(filePath, true);
	}

}

