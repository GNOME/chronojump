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
 * Copyright (C) 2004-2026   Xavier de Blas <xaviblas@gmail.com>
 */

using Mono.Unix;

// ----------------- discover / detect devices --------->
public partial class ChronoJumpWindow
{
	// at glade ---->
	//detect devices
	Gtk.Box vbox_micro_discover;
	Gtk.Label label_micro_discover_title;
	Gtk.Label label_micro_discover_not_found;
	Gtk.Frame frame_micro_discover;
	Gtk.Notebook notebook_micro_discover;
	Gtk.VBox vbox_micro_discover_main;
	Gtk.Box	box_micro_discover_assign_manually;
	Gtk.ButtonBox buttonbox_micro_discover_assign_manually;
	Gtk.Button button_micro_discover_refresh;
	Gtk.Image image_micro_discover_refresh;
	Gtk.Box box_micro_discover_races_choose;
	Gtk.Grid grid_micro_discover;
	Gtk.Box box_micro_discover_nc;
	Gtk.Label label_micro_discover_nc_current_mode;
	Gtk.Label label_micro_discover_connect_error;
	Gtk.Label label_micro_discover_nc_comment;
	Gtk.Box hbox_contacts_detect_and_execute;
	Gtk.Box hbox_encoder_detect_and_execute;
	Gtk.Button button_contacts_detect;
	Gtk.Button button_encoder_detect;
	Gtk.Button button_contacts_detect_small;
	Gtk.Button button_encoder_detect_small;
	Gtk.CheckButton check_discover_advanced;
	Gtk.Label label_discover_advanced;
	Gtk.Image image_discover_advanced;
	Gtk.EventBox eventbox_button_micro_discover_cancel_close;
	Gtk.Button button_micro_discover_cancel_close;
	Gtk.Image image_button_micro_discover_cancel_close;
	Gtk.Label label_button_micro_discover_cancel_close;
	Gtk.Image image_button_micro_discover_assign_manually_cancel;
	//Gtk.Image image_micro_discover_mode;

	//bluetooth
	Gtk.Image image_bluetooth;
	Gtk.Image image_usb;
	Gtk.Entry entry_bluetooth_url;
	Gtk.Button button_bluetooth_start;
	Gtk.Button button_bluetooth_end;
	Gtk.TextView textview_bluetooth;
	Gtk.RadioButton radio_bluetooth_mode_scan;
	Gtk.RadioButton radio_bluetooth_mode_scan_connect;
	Gtk.RadioButton radio_bluetooth_value_all;
	Gtk.RadioButton radio_bluetooth_value_chronojump;
	Gtk.RadioButton radio_bluetooth_value_chronopic4;
	Gtk.RadioButton radio_bluetooth_value_this_device;
	Gtk.Box box_bluetooth_value_this_device;
	Gtk.Entry entry_bluetooth_value_this_device;
	// <---- at glade

	BluetoothCapture bluetoothCapture;

	//also manages if networks or not, on networks do not show
	private void button_detect_show_hide (bool show)
	{
		if (configChronojump.Compujump)
			return;

		// Cloud-view cannot capture
		if (configChronojump.ReadFromCloudMainPath != "")
		{
			button_contacts_detect.Visible = false;
			hbox_contacts_detect_and_execute.Visible = false;
			button_encoder_detect.Visible = false;
			hbox_encoder_detect_and_execute.Visible = false;
			return;
		}

		button_contacts_detect.Visible = show;
		hbox_contacts_detect_and_execute.Visible = ! show;

		button_encoder_detect.Visible = show;
		hbox_encoder_detect_and_execute.Visible = ! show;
	}

	DiscoverWindow discoverWin;
	private void on_button_detect_clicked (object o, EventArgs args)
	{
		app1s_notebook_sup_entered_from = notebook_sup.CurrentPage;
		detect_devices_do ();
	}

	private void detect_devices_do ()
	{
		notebook_sup.CurrentPage = Convert.ToInt32 (notebook_sup_pages.MICRODISCOVER);
		event_execute_label_message.Text = "";
		menus_and_mode_sensitive (false);

		if(encoderThreadBG != null && encoderThreadBG.IsAlive)
		{
			stopCapturingInertialBG();

			//to have time on Windows to really have sp port closed and be able to read on chronopicRegister and/or discoverWin
			System.Threading.Thread.Sleep (1000);
		}

		if (Constants.ModeIsFORCESENSOR (current_mode) && portFSOpened)
			forceSensorDisconnect ();
		else if (current_mode == Constants.Modes.RUNSENCODER && portREOpened)
			runEncoderDisconnect ();

		chronopicRegisterUpdate (false);

		label_micro_discover_title.Text = string.Format (Catalog.GetString (
					"Detect devices compatible with: <b>{0}</b>"), Constants.ModePrint (current_mode));
		label_micro_discover_title.UseMarkup = true;
		box_micro_discover_nc.Visible = false;
		label_micro_discover_nc_current_mode.Text = Constants.ModePrint (current_mode);
		label_micro_discover_connect_error.Visible = false;
		label_micro_discover_nc_comment.Text = "";
		label_micro_discover_nc_comment.Visible = false;

		if (
				operatingSystem == UtilAll.OperatingSystems.WINDOWS &&
				(current_mode == Constants.Modes.RUNSSIMPLE || current_mode == Constants.Modes.RUNSINTERVALLIC)
				) // TODO: do it also on 4platforms to ask bluetooth
			notebook_micro_discover.CurrentPage = Convert.ToInt32 (DiscoverWindow.Notebook_micro_discover_pages.ASK_RACES);
		else if (current_mode == Constants.Modes.OTHER) // 4platforms
			notebook_micro_discover.CurrentPage = Convert.ToInt32 (DiscoverWindow.Notebook_micro_discover_pages.ASK_BT_OR_USB);
		else
			notebook_micro_discover.CurrentPage = Convert.ToInt32 (DiscoverWindow.Notebook_micro_discover_pages.USB);

		discoverWin = new DiscoverWindow (app1,
				operatingSystem, current_mode,
				chronopicRegister,
				notebook_micro_discover,
				vbox_micro_discover_main,
				button_micro_discover_refresh,
				image_micro_discover_refresh,
				box_micro_discover_assign_manually,
				buttonbox_micro_discover_assign_manually,
				label_micro_discover_not_found,
				grid_micro_discover,
				box_micro_discover_nc,
				label_micro_discover_nc_comment,
				button_micro_discover_cancel_close,
				image_button_micro_discover_cancel_close,
				label_button_micro_discover_cancel_close,
				image_button_micro_discover_assign_manually_cancel,
				check_discover_advanced.Active, check_discover_advanced,
				label_discover_advanced, image_discover_advanced,
				Constants.ModeIcon (current_mode),
				label_micro_discover_connect_error,
				Config.ColorBackgroundShiftedIsDark
				);

		if(! Config.UseSystemColor)
			UtilGtk.ContrastLabelsGrid (Config.ColorBackgroundShiftedIsDark, grid_micro_discover);

		discoverWin.FakeButtonClose.Clicked -= new EventHandler (on_discoverWindow_closed);
		discoverWin.FakeButtonClose.Clicked += new EventHandler (on_discoverWindow_closed);
	}

	private void on_button_micro_discover_refresh_clicked (object o, EventArgs args)
	{
		detect_devices_do ();
	}

	private void on_button_discover_detect_bluetooth_clicked (object o, EventArgs args)
	{
		entry_bluetooth_url.Text = BluetoothLE.GetScriptURL ();

		notebook_micro_discover.CurrentPage = Convert.ToInt32 (DiscoverWindow.Notebook_micro_discover_pages.BLUETOOTH);
	}
	private void on_button_discover_detect_usb_clicked (object o, EventArgs args)
	{
		notebook_micro_discover.CurrentPage = Convert.ToInt32 (DiscoverWindow.Notebook_micro_discover_pages.USB);

		if (discoverWin != null)
			discoverWin.DetectWichro ();
	}

	// ---- bluetooth callbacks ---->
	// TODO: adaptant tot això a bluetoothCapture.cs
	// TODO: move most of this to src/discover.cs
	static Thread discoverBluetoothThread;

	private void on_radio_bluetooth_value_toggled (object o, EventArgs args)
	{
		entry_bluetooth_value_this_device.Sensitive = radio_bluetooth_value_this_device.Active;
	}

	private void on_button_bluetooth_start_clicked (object o, EventArgs args)
	{
		/*
		if(! File.Exists (entry_bluetooth_url.Text))
		{
			LogB.Information ("Error. Bluetooth start file not found: " + entry_bluetooth_url.Text);
			tbBluetooth.Text = Catalog.GetString ("Error. File not found.");
			textview_bluetooth.Buffer = tbBluetooth;
			bluetoothSensitiveDoing (false);

			return;
		}
		*/

		bluetoothSensitiveDoing (true);

		//bluetoothReading = true;
		textview_bluetooth.Name = "fontSize9";
		//tbBluetoothText = "";
		tbBluetooth.Text = "Starting communication... ";
		textview_bluetooth.Buffer = tbBluetooth;

		discoverBluetoothThread = new Thread (new ThreadStart (bluetoothDo));
		GLib.Idle.Add (new GLib.IdleHandler (pulseBluetooth));

		LogB.ThreadStart();
		discoverBluetoothThread.Start();
	}

	private void bluetoothSensitiveDoing (bool doing)
	{
		entry_bluetooth_url.Sensitive = ! doing;
		button_bluetooth_start.Sensitive = ! doing;
		button_bluetooth_end.Sensitive = doing;
	}

	private void bluetoothDo ()
	{
		//Start BluetoothLE service
		BluetoothLE.SetProcess (entry_bluetooth_url.Text);

		string mode = "SCAN";
		if (radio_bluetooth_mode_scan_connect.Active)
			mode = "CONNECT";

		string val = "ALL";
		if (radio_bluetooth_value_chronojump.Active)
			val = "CJ";
		else if (radio_bluetooth_value_chronopic4.Active)
			val = "CP4";
		else if (radio_bluetooth_value_this_device.Active && entry_bluetooth_value_this_device.Text != "")
			val = entry_bluetooth_value_this_device.Text;

		//BluetoothLE.Start (mode, val);
		bluetoothCapture.Start (entry_bluetooth_url.Text, mode, val);
	}

	// by GTK thread
	private bool pulseBluetooth ()
	{
		/*
		if (needToUpdateTextViewBluetooth)
		{
			tbBluetooth.Text = tbBluetoothText;
			textview_bluetooth.Buffer = tbBluetooth;
			UtilGtk.TextViewScrollToEnd (textview_bluetooth);
			needToUpdateTextViewBluetooth = false;
		}
		*/
		//if (! bluetoothReading)
		//	return false;
		if (bluetoothCapture != null && bluetoothCapture.BluetoothReading &&
				bluetoothCapture.Bm_l.CanReadFromList ())
		{
			string currentCommand = bluetoothCapture.Bm_l.ReadNext ();
			//LogB.Information ("currentCommand: " + currentCommand);
			if (currentCommand.Contains (BluetoothLE.ConnectedName))
				discoverWin.Image_cancel_close_isClose ();

			tbBluetooth.Text += currentCommand;
			textview_bluetooth.Buffer = tbBluetooth;
			UtilGtk.TextViewScrollToEnd (textview_bluetooth);
		}

		//LogB.Debug (" \npulseBluetooth:" + discoverBluetoothThread.ThreadState.ToString());
		Thread.Sleep (50);
		return true;
	}

	private void on_button_bluetooth_end_clicked (object o, EventArgs args)
	{
		//if (bluetoothReading)
		//{
			bluetooth_stop ();
			bluetoothSensitiveDoing (false);
		//}
	}

	private void bluetooth_stop ()
	{
		//Stop the BluetoothLE service if it was started
		BluetoothLE.Stop();
		//bluetoothReading = false;
	}

	/*
	private void bluetooth_textview_update (string str)
	{
		tbBluetoothText += str;
		needToUpdateTextViewBluetooth = true;
	}
	*/

	// <---- bluetooth callbacks ----

	// ---- races ---->
	private void on_button_discover_detect_wichro_clicked (object o, EventArgs args)
	{
		if (discoverWin != null)
		{
			notebook_micro_discover.CurrentPage = Convert.ToInt32 (DiscoverWindow.Notebook_micro_discover_pages.USB);
			label_micro_discover_nc_current_mode.Text = "WICHRO";

			//TODO: only show this if no WICHRO is found
			label_micro_discover_nc_comment.Text = Catalog.GetString ("Before trying again, better unplug/plug USB");
			discoverWin.DetectWichro ();
		}
	}
	private void on_button_discover_detect_old_photocells_clicked (object o, EventArgs args)
	{
		if (discoverWin != null)
		{
			notebook_micro_discover.CurrentPage = Convert.ToInt32 (DiscoverWindow.Notebook_micro_discover_pages.USB);
			label_micro_discover_nc_current_mode.Text = Catalog.GetString ("Old cabled photocells");

			//TODO: only show this if no "old photocells" is found
			label_micro_discover_nc_comment.Text = Catalog.GetString ("Before trying again, better unplug/plug USB");
			discoverWin.DetectOldPhotocells ();
		}
	}
	// <---- races ----

	private void on_check_discover_advanced_toggled (object o, EventArgs args)
	{
		if (discoverWin != null)
			discoverWin.ShowAdvanced (check_discover_advanced.Active);
	}

	private void on_button_micro_discover_assign_manually_cancel_clicked (object o, EventArgs args)
	{
		if (discoverWin != null)
			notebook_micro_discover.CurrentPage = Convert.ToInt32 (DiscoverWindow.Notebook_micro_discover_pages.USB);
	}

	private void on_button_micro_discover_cancel_close_clicked (object o, EventArgs args)
	{
		if (discoverWin != null)
		{
			discoverWin.CancelCloseFromUser ();
			button_detect_show_hide (true); //as closed without use this, then show the big button again
		}
	}

	private void on_discoverWindow_closed (object o, EventArgs args)
	{
		chronopicRegister = discoverWin.ChronopicRegisterGet;

		//if(discoverWin.PortSelected != "")
		if(discoverWin.PortSelected.Port != "")
		{
			chronopicRegister.SetSelectedForMode (discoverWin.PortSelected, current_mode);
			button_detect_show_hide (false);

			//do not show the threshold on WICHRO
			//if ( chronopicRegister.NumConnectedOfType (ChronopicRegisterPort.Types.RUN_WIRELESS) == 1)
			if (current_mode == Constants.Modes.RUNSSIMPLE || current_mode == Constants.Modes.RUNSINTERVALLIC)
				button_threshold.Visible = (discoverWin.PortSelected.Type != ChronopicRegisterPort.Types.RUN_WIRELESS);
			else if (current_mode == Constants.Modes.WILIGHT)
				entry_wilight_port.Text = discoverWin.PortSelected.Port;
			else if (current_mode == Constants.Modes.OTHER) //FOURPLATFORMS
				entry_fourPlatforms_port.Text = discoverWin.PortSelected.Port;

			// close portFSOpened after discover to ensure do a forceSensorConnect()
			if (Constants.ModeIsFORCESENSOR (current_mode) && portFSOpened)
				portFSOpened = false;
			// same for runEncoder
			else if (current_mode == Constants.Modes.RUNSENCODER && portREOpened)
				portREOpened = false;

			if (current_mode == Constants.Modes.JUMPSSIMPLE)
				showHideFourPlatformsJumpsDrawingArea ();
		}

		notebook_sup.CurrentPage = app1s_notebook_sup_entered_from;
		menus_and_mode_sensitive (true);
	}
		
	private void connectWidgetsDiscover (Gtk.Builder builder)
	{
		//detect devices
		vbox_micro_discover = (Gtk.Box) builder.GetObject ("vbox_micro_discover");
		label_micro_discover_title = (Gtk.Label) builder.GetObject ("label_micro_discover_title");
		label_micro_discover_not_found = (Gtk.Label) builder.GetObject ("label_micro_discover_not_found");
		frame_micro_discover = (Gtk.Frame) builder.GetObject ("frame_micro_discover");
		notebook_micro_discover = (Gtk.Notebook) builder.GetObject ("notebook_micro_discover");
		vbox_micro_discover_main = (Gtk.VBox) builder.GetObject ("vbox_micro_discover_main");
		button_micro_discover_refresh = (Gtk.Button) builder.GetObject ("button_micro_discover_refresh");
		image_micro_discover_refresh = (Gtk.Image) builder.GetObject ("image_micro_discover_refresh");
		box_micro_discover_assign_manually = (Gtk.Box) builder.GetObject ("box_micro_discover_assign_manually");
		buttonbox_micro_discover_assign_manually = (Gtk.ButtonBox) builder.GetObject ("buttonbox_micro_discover_assign_manually");
		box_micro_discover_races_choose = (Gtk.Box) builder.GetObject ("box_micro_discover_races_choose");
		grid_micro_discover = (Gtk.Grid) builder.GetObject ("grid_micro_discover");
		box_micro_discover_nc = (Gtk.Box) builder.GetObject ("box_micro_discover_nc");
		label_micro_discover_nc_current_mode = (Gtk.Label) builder.GetObject ("label_micro_discover_nc_current_mode");
		label_micro_discover_connect_error = (Gtk.Label) builder.GetObject ("label_micro_discover_connect_error");
		label_micro_discover_nc_comment = (Gtk.Label) builder.GetObject ("label_micro_discover_nc_comment");
		hbox_contacts_detect_and_execute = (Gtk.Box) builder.GetObject ("hbox_contacts_detect_and_execute");
		hbox_encoder_detect_and_execute = (Gtk.Box) builder.GetObject ("hbox_encoder_detect_and_execute");
		button_contacts_detect = (Gtk.Button) builder.GetObject ("button_contacts_detect");
		button_encoder_detect = (Gtk.Button) builder.GetObject ("button_encoder_detect");
		button_contacts_detect_small = (Gtk.Button) builder.GetObject ("button_contacts_detect_small");
		button_encoder_detect_small = (Gtk.Button) builder.GetObject ("button_encoder_detect_small");
		eventbox_button_micro_discover_cancel_close = (Gtk.EventBox) builder.GetObject ("eventbox_button_micro_discover_cancel_close");
		check_discover_advanced = (Gtk.CheckButton) builder.GetObject ("check_discover_advanced");
		label_discover_advanced = (Gtk.Label) builder.GetObject ("label_discover_advanced");
		image_discover_advanced = (Gtk.Image) builder.GetObject ("image_discover_advanced");
		button_micro_discover_cancel_close = (Gtk.Button) builder.GetObject ("button_micro_discover_cancel_close");
		image_button_micro_discover_cancel_close = (Gtk.Image) builder.GetObject ("image_button_micro_discover_cancel_close");
		label_button_micro_discover_cancel_close = (Gtk.Label) builder.GetObject ("label_button_micro_discover_cancel_close");
		image_button_micro_discover_assign_manually_cancel = (Gtk.Image) builder.GetObject ("image_button_micro_discover_assign_manually_cancel");
		//image_micro_discover_mode = (Gtk.Image) builder.GetObject ("image_micro_discover_mode");

		//bluetooth
		image_bluetooth = (Gtk.Image) builder.GetObject ("image_bluetooth");
		image_usb = (Gtk.Image) builder.GetObject ("image_usb");
		entry_bluetooth_url = (Gtk.Entry) builder.GetObject ("entry_bluetooth_url");
		button_bluetooth_start = (Gtk.Button) builder.GetObject ("button_bluetooth_start");
		button_bluetooth_end = (Gtk.Button) builder.GetObject ("button_bluetooth_end");
		textview_bluetooth = (Gtk.TextView) builder.GetObject ("textview_bluetooth");
		radio_bluetooth_mode_scan = (Gtk.RadioButton) builder.GetObject ("radio_bluetooth_mode_scan");
		radio_bluetooth_mode_scan_connect = (Gtk.RadioButton) builder.GetObject ("radio_bluetooth_mode_scan_connect");
		radio_bluetooth_value_all = (Gtk.RadioButton) builder.GetObject ("radio_bluetooth_value_all");
		radio_bluetooth_value_chronojump = (Gtk.RadioButton) builder.GetObject ("radio_bluetooth_value_chronojump");
		radio_bluetooth_value_chronopic4 = (Gtk.RadioButton) builder.GetObject ("radio_bluetooth_value_chronopic4");
		radio_bluetooth_value_this_device = (Gtk.RadioButton) builder.GetObject ("radio_bluetooth_value_this_device");
		box_bluetooth_value_this_device = (Gtk.Box) builder.GetObject ("box_bluetooth_value_this_device");
		entry_bluetooth_value_this_device = (Gtk.Entry) builder.GetObject ("entry_bluetooth_value_this_device");
	}
}
