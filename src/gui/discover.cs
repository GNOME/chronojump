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
 *  Copyright (C) 2016-2026   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List<T>
using Gdk;
using Gtk;
using Mono.Unix;
using System.Threading;
using System.Diagnostics;  //Stopwatch


public class DiscoverWindow
{
	//TODO instead of all these lists, have List<microDiscoverGui>
	//... tried but complicated. note the different enumeration between discover & the gui rows

	List<Gtk.ProgressBar> c1_progressbar_microNotDiscovered_l;
	List<Gtk.ProgressBar> c1_progressbar_microAlreadyDiscovered_l; // to be used on forget

	List<Gtk.Button> button_microNotDiscovered_l;
	List<Gtk.Label> label_microNotDiscovered_l; //to use labels for ---- and NC instead of buttons

	List<ChronopicRegisterPort> portAlreadyDiscovered_l;
	List<Gtk.Button> button_microAlreadyDiscovered_l;
	List<Gtk.Label> label_microAlreadyDiscovered_l; //to be able to Visible = false after ShowAll ()

	//to test debug a device
	List<Gtk.Button> buttonDebug_alreadyDiscovered_l;
	List<Gtk.Button> buttonDebug_notDiscovered_l;
	List<ChronopicRegisterPort> buttonDebug_alreadyDiscovered_crp_l;
	List<ChronopicRegisterPort> buttonDebug_notDiscovered_crp_l;

	//to forget a device
	List<Gtk.Button> buttonForget_alreadyDiscovered_l;
	List<Gtk.Button> buttonForget_notDiscovered_l;
	List<ChronopicRegisterPort> buttonForget_alreadyDiscovered_crp_l;
	List<ChronopicRegisterPort> buttonForget_notDiscovered_crp_l;

	//to manuallyAssign a device
	List<Gtk.Button> buttonManuallyAssign_alreadyDiscovered_l;
	List<Gtk.Button> buttonManuallyAssign_notDiscovered_l;
	List<ChronopicRegisterPort> buttonManuallyAssign_alreadyDiscovered_crp_l;
	List<ChronopicRegisterPort> buttonManuallyAssign_notDiscovered_crp_l;

	static bool discoverCloseAfterCancel; //is true when select useThis while reading other devices
	static Thread discoverThread;
	static MicroDiscover microDiscover;
	public Gtk.Button FakeButtonClose;
	ChronopicTestWindow chronopicTestWin;

	private Gtk.Window parentWin;
	private Constants.Modes current_mode;
	private ChronopicRegister chronopicRegister;
	private Gtk.Notebook notebook_micro_discover;
	private Gtk.VBox vbox_micro_discover_main;
	private Gtk.Button button_micro_discover_refresh;
	private Gtk.Image image_micro_discover_refresh;
	private Gtk.Box box_micro_discover_assign_manually;
	private Gtk.ButtonBox buttonbox_micro_discover_assign_manually;
	private Gtk.Grid grid_micro_discover;
	private Gtk.Box box_micro_discover_nc;
	private Gtk.Label label_micro_discover_nc_comment;
	private Gtk.Button button_micro_discover_cancel_close;
	private Gtk.Image image_button_micro_discover_cancel_close;
	private Gtk.Label label_button_micro_discover_cancel_close;
	private Gtk.Image image_button_micro_discover_assign_manually_cancel;
	private Gtk.CheckButton check_discover_advanced;
	private Gtk.Label label_discover_advanced;
	private Gtk.Image image_discover_advanced;
	private Gtk.Image image_discover_mode;
	private Gtk.Label label_micro_discover_connect_error;
	private bool bgShiftedIsDark;
	private Gtk.Label lAdvanced;
	private bool showAdvanced;
	private string useThisStr = Catalog.GetString ("Select!");
	private string debugThisStr = Catalog.GetString ("Test it!");
	private string forgetThisStr = Catalog.GetString ("Forget it!");
	private string forgottenStr = Catalog.GetString ("Forgotten");
	private string manuallyAssignThisStr = Catalog.GetString ("Assign manually");
	private string manuallyAssignedStr = Catalog.GetString ("Assigned manually: ");

	public enum Notebook_micro_discover_pages { ASK_BT_OR_USB, ASK_RACES, USB, BLUETOOTH, USB_ASSIGN_MANUALLY }
	private Gtk.Button button_manually_assign1;
	private Gtk.Button button_manually_assign2;
	ChronopicRegisterPort crpManuallyAssign;

	private ChronopicRegisterPort portSelected;

	public DiscoverWindow (Gtk.Window parentWin,
			UtilAll.OperatingSystems operatingSystem, Constants.Modes current_mode,
			ChronopicRegister chronopicRegister,
			Gtk.Notebook notebook_micro_discover,
			Gtk.VBox vbox_micro_discover_main,
			Gtk.Button button_micro_discover_refresh,
			Gtk.Image image_micro_discover_refresh,
			Gtk.Box box_micro_discover_assign_manually,
			Gtk.ButtonBox buttonbox_micro_discover_assign_manually,
			Gtk.Label label_micro_discover_not_found,
			Gtk.Grid grid_micro_discover,
			Gtk.Box box_micro_discover_nc,
			Gtk.Label label_micro_discover_nc_comment,
			Gtk.Button button_micro_discover_cancel_close,
			Gtk.Image image_button_micro_discover_cancel_close,
			Gtk.Label label_button_micro_discover_cancel_close,
			Gtk.Image image_button_micro_discover_assign_manually_cancel,
			bool showAdvanced, Gtk.CheckButton check_discover_advanced,
			Gtk.Label label_discover_advanced, Gtk.Image image_discover_advanced,
			string iconModeStr,
			Gtk.Label label_micro_discover_connect_error,
			bool bgShiftedIsDark
			)
	{
		this.parentWin = parentWin;
		this.current_mode = current_mode;
		this.chronopicRegister = chronopicRegister;
		this.notebook_micro_discover = notebook_micro_discover;
		this.vbox_micro_discover_main = vbox_micro_discover_main;
		this.button_micro_discover_refresh = button_micro_discover_refresh;
		this.image_micro_discover_refresh = image_micro_discover_refresh;
		this.box_micro_discover_assign_manually = box_micro_discover_assign_manually;
		this.buttonbox_micro_discover_assign_manually = buttonbox_micro_discover_assign_manually;
		this.grid_micro_discover = grid_micro_discover;
		this.box_micro_discover_nc = box_micro_discover_nc;
		this.label_micro_discover_nc_comment = label_micro_discover_nc_comment;
		this.button_micro_discover_cancel_close = button_micro_discover_cancel_close;
		this.image_button_micro_discover_cancel_close = image_button_micro_discover_cancel_close;
		this.label_button_micro_discover_cancel_close = label_button_micro_discover_cancel_close;
		this.image_button_micro_discover_assign_manually_cancel = image_button_micro_discover_assign_manually_cancel;
		this.showAdvanced = showAdvanced;
		this.check_discover_advanced = check_discover_advanced;
		this.label_discover_advanced = label_discover_advanced;
		this.image_discover_advanced = image_discover_advanced;
		this.label_micro_discover_connect_error = label_micro_discover_connect_error;
		this.bgShiftedIsDark = bgShiftedIsDark;

		// 1) set up gui

		vbox_micro_discover_main.Visible = true;

		// create manually assign buttons
		crpManuallyAssign = new ChronopicRegisterPort ("");
		button_manually_assign1 = new Gtk.Button ();
		button_manually_assign2 = new Gtk.Button ();
		button_manually_assign1.Visible = false;
		button_manually_assign2.Visible = false;
		button_manually_assign1.Clicked -= new EventHandler (on_button_manually_assign1_clicked);
		button_manually_assign1.Clicked += new EventHandler (on_button_manually_assign1_clicked);
		button_manually_assign2.Clicked -= new EventHandler (on_button_manually_assign2_clicked);
		button_manually_assign2.Clicked += new EventHandler (on_button_manually_assign2_clicked);
		UtilGtk.RemoveChildren (buttonbox_micro_discover_assign_manually);
		buttonbox_micro_discover_assign_manually.PackStart (button_manually_assign1, false, false, 0);
		buttonbox_micro_discover_assign_manually.PackStart (button_manually_assign2, false, false, 0);

		FakeButtonClose = new Gtk.Button();
		portSelected = new ChronopicRegisterPort ("");
		image_discover_mode = new Gtk.Image (Chronojump.MyPixbuf.Get (null, Util.GetImagePath(false) + iconModeStr));

		image_micro_discover_refresh.Pixbuf = Chronojump.MyPixbuf.Get (null, Util.GetImagePath(false) + "refresh_blue.png");
		if (showAdvanced) {
			label_discover_advanced.Text = Catalog.GetString ("Hide advanced");
			image_discover_advanced.Visible = false;
		} else {
			label_discover_advanced.Text = Catalog.GetString ("Show advanced");
			image_discover_advanced.Visible = true;
		}

		//ChronoDebug cDebug = new ChronoDebug("Discover " + current_mode.ToString());
		//cDebug.Start();

		// 2) get the serial numbers (and also the portName and type if saved on SQL)
		//chronopicRegisterUpdate (false);

		List<ChronopicRegisterPort> alreadyDiscovered_l = new List<ChronopicRegisterPort> ();
		List<ChronopicRegisterPort> notDiscovered_l = new List<ChronopicRegisterPort> ();
		foreach (ChronopicRegisterPort crp in chronopicRegister.Crpl.L)
                        if (crp.Port != "")
			{
				if (crp.Type != ChronopicRegisterPort.Types.UNKNOWN &&
						! chronopicRegister.SerialNumberIsNotUnique (crp.SerialNumber))
					alreadyDiscovered_l.Add (crp);
				else
					notDiscovered_l.Add (crp);
			}

		Image_cancel_close_isCancel ();

		image_button_micro_discover_assign_manually_cancel.Pixbuf =
			Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "image_cancel.png");

		if (alreadyDiscovered_l.Count > 0 || notDiscovered_l.Count > 0)
		{
			microDiscover = new MicroDiscover (notDiscovered_l);

			label_micro_discover_not_found.Visible = false;
			setup_grid_micro_discover_l (alreadyDiscovered_l, notDiscovered_l);

			if (current_mode == Constants.Modes.OTHER) // 4platforms
			{
				// do nothing, user will click on BLUETOOTH or USB
			}
			else if (operatingSystem == UtilAll.OperatingSystems.WINDOWS &&
					(current_mode == Constants.Modes.RUNSSIMPLE || current_mode == Constants.Modes.RUNSINTERVALLIC))
			{
				// do nothing, user will click on WICHRO or Old cabled photocells
			} else
				discoverStart ();
		} else {
			UtilGtk.RemoveChildren (grid_micro_discover);

			label_micro_discover_not_found.Text = Catalog.GetString ("Device not found.");
			label_micro_discover_not_found.Visible = true;
			Image_cancel_close_isClose ();
		}

		if (current_mode == Constants.Modes.OTHER)
		{
			/*
			 * Tere is a bug at least on my Lenovo T430u, ACM3 is detected but also an unexistant ACM0 (I am not using any hub).
			 * If I press select while is trying to detect this ACM0, then the cancel does not work ok, because capture then does not work (capture message is sent but arduino did not answer).
			 * If we wait untile ACM0 is detected (it will show NC), then press Select (ACM3), and capture works
			 */
			label_micro_discover_connect_error.Text = "On 4Platforms wait until process ends, and after press: Select!";
			label_micro_discover_connect_error.Visible = true;
		} else
			label_micro_discover_connect_error.Text = "";

		//cDebug.StopAndPrint();
	}

	// separated as in Windows Races it is called after user interaction
	private void discoverStart ()
	{
		discoverCloseAfterCancel = false;
		button_micro_discover_refresh.Sensitive = false;

		discoverThread = new Thread (new ThreadStart (discoverDo));
		GLib.Idle.Add (new GLib.IdleHandler (pulseDiscoverGTK));
		discoverThread.Start();
	}

	// Races on Windows 11 problem if detect first chronopic (9600) and then WICHRO (115200)
	// now checking what to detect, and if no success, tell user to unplug/plug usb before try the other type
	public void DetectWichro ()
	{
		if (microDiscover == null)
			return;

		microDiscover.RacesDevicesDetect = MicroDiscover.RacesDevices.WICHRO;
		discoverStart ();
	}
	public void DetectOldPhotocells ()
	{
		if (microDiscover == null)
			return;

		microDiscover.RacesDevicesDetect = MicroDiscover.RacesDevices.OLDPHOTOCELLS;
		discoverStart ();
	}

	/*
	public void DetectBluetooth ()
	{
	}
	*/
	public void DetectUSB ()
	{
		if (microDiscover == null)
			return;

		discoverStart ();
	}

	public void Image_cancel_close_isCancel ()
	{
		image_button_micro_discover_cancel_close.Pixbuf =
			Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "image_cancel.png");
		label_button_micro_discover_cancel_close.Text = Catalog.GetString("Cancel");
	}
	public void Image_cancel_close_isClose ()
	{
		image_button_micro_discover_cancel_close.Pixbuf =
			Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "image_close.png");
		label_button_micro_discover_cancel_close.Text = Catalog.GetString("Close");
	}

	private void setup_grid_micro_discover_l (
			List<ChronopicRegisterPort> alreadyDiscovered_l,
			List<ChronopicRegisterPort> notDiscovered_l)
	{
		// 1) delete widgets of previous calls
		UtilGtk.RemoveChildren (grid_micro_discover);

		grid_micro_discover.ColumnSpacing = 20;
		grid_micro_discover.RowSpacing = 14;

		// 2) create the lists of widgets to be able to access later
		c1_progressbar_microNotDiscovered_l = new List<Gtk.ProgressBar> ();
		c1_progressbar_microAlreadyDiscovered_l = new List<Gtk.ProgressBar> ();
		button_microNotDiscovered_l = new List<Gtk.Button> ();
		label_microNotDiscovered_l = new List<Gtk.Label> ();
		portAlreadyDiscovered_l = new List<ChronopicRegisterPort> ();
		button_microAlreadyDiscovered_l = new List<Gtk.Button> ();
		label_microAlreadyDiscovered_l = new List<Gtk.Label> ();

		buttonDebug_alreadyDiscovered_l = new List<Gtk.Button> ();
		buttonDebug_notDiscovered_l = new List<Gtk.Button> ();
		buttonDebug_alreadyDiscovered_crp_l = new List<ChronopicRegisterPort> ();
		buttonDebug_notDiscovered_crp_l = new List<ChronopicRegisterPort> ();

		buttonForget_alreadyDiscovered_l = new List<Gtk.Button> ();
		buttonForget_notDiscovered_l = new List<Gtk.Button> ();
		buttonForget_alreadyDiscovered_crp_l = new List<ChronopicRegisterPort> ();
		buttonForget_notDiscovered_crp_l = new List<ChronopicRegisterPort> ();

		buttonManuallyAssign_alreadyDiscovered_l = new List<Gtk.Button> ();
		buttonManuallyAssign_notDiscovered_l = new List<Gtk.Button> ();
		buttonManuallyAssign_alreadyDiscovered_crp_l = new List<ChronopicRegisterPort> ();
		buttonManuallyAssign_notDiscovered_crp_l = new List<ChronopicRegisterPort> ();

		// 3) create widgets, lists, attach to table and show all

		// 3a) create table header row
		Gtk.Label l0 = new Gtk.Label ("<b>" + Catalog.GetString ("Device") + "</b>");
		l0.UseMarkup = true;

		/*
		Gtk.Label l1 = new Gtk.Label ("<b>" + Catalog.GetString ("Compatibility with") + "</b>");
		l1.UseMarkup = true;
		Gtk.Box hbox_l1 = new Gtk.Box (Gtk.Orientation.Horizontal, 10);
		hbox_l1.PackStart (l1, false, false, 0);
		hbox_l1.PackStart (image_discover_mode, false, false, 0);
		//hbox_l1.Hexpand = true; //this does not work, so create a parent and expand:
		Gtk.Box hbox_l1_parent = new Gtk.Box (Gtk.Orientation.Horizontal, 0);
		hbox_l1_parent.PackStart (hbox_l1, true, false, 0);
		*/
		Gtk.Label lType = new Gtk.Label ("<b>" + Catalog.GetString ("Type") + "</b>");
		lType.UseMarkup = true;
		Gtk.Label lAction = new Gtk.Label ("<b>" + Catalog.GetString ("Action") + "</b>");
		lAction.UseMarkup = true;

		lAdvanced = new Gtk.Label ("<b>" + Catalog.GetString ("Advanced") + "</b>");
		lAdvanced.UseMarkup = true;

		grid_micro_discover.Attach (l0, 0, 0, 1, 1);
		//grid_micro_discover.Attach (hbox_l1_parent, 1, 0, 2, 1);
		grid_micro_discover.Attach (lType, 1, 0, 1, 1);
		grid_micro_discover.Attach (lAction, 2, 0, 1, 1);
		grid_micro_discover.Attach (lAdvanced, 3, 0, 1, 1);

		// 3b) create a row for each device
		for (int i = 0; i < alreadyDiscovered_l.Count; i ++)
			setup_row_micro_discover_l (alreadyDiscovered_l [i], i + 1, true);
		for (int i = 0; i < notDiscovered_l.Count; i ++)
			setup_row_micro_discover_l (notDiscovered_l [i], i + 1 + alreadyDiscovered_l.Count, false);

		grid_micro_discover.ShowAll();

		//hide any buttons with "NC"or "----"
		foreach (Button b in button_microAlreadyDiscovered_l)
			if (b.Label == Catalog.GetString ("NC") || b.Label == "----")
				b.Visible = false;
		foreach (Button b in button_microNotDiscovered_l)
			if (b.Label == Catalog.GetString ("NC") || b.Label == "----")
				b.Visible = false;
		foreach (Gtk.Label l in label_microAlreadyDiscovered_l)
			if (l.Text == useThisStr)
				l.Visible = false;

		lAdvanced.Visible = showAdvanced;

		foreach (Button b in buttonDebug_alreadyDiscovered_l)
			if (b.Label == "" || ! showAdvanced)
				b.Visible = false;
		foreach (Button b in buttonDebug_notDiscovered_l)
			if (b.Label == "" || ! showAdvanced)
				b.Visible = false;

		foreach (Button b in buttonForget_alreadyDiscovered_l)
			if (b.Label == "" || ! showAdvanced)
				b.Visible = false;
		foreach (Button b in buttonForget_notDiscovered_l)
			if (b.Label == "" || ! showAdvanced)
				b.Visible = false;

		foreach (Button b in buttonManuallyAssign_alreadyDiscovered_l)
			if (b.Label == "" || ! showAdvanced)
				b.Visible = false;
		foreach (Button b in buttonManuallyAssign_notDiscovered_l)
			if (b.Label == "" || ! showAdvanced)
				b.Visible = false;
	}

	/*
	 * | l (port/serialNum) | c1_pb (progressbar) | c2_box_b_or_label (label, bSelect) | c3i_advancedButtons (bDebug, bForget (if !magicNumber)) or manually assign
	 */
	private void setup_row_micro_discover_l (ChronopicRegisterPort crp, int i, bool alreadyDiscovered)
	{
		// ---- column 0
		string portNameShort = crp.Port;
		if (portNameShort.StartsWith ("/dev/"))
			portNameShort = portNameShort.Replace ("/dev/", "");

		Gtk.Label l = new Gtk.Label (string.Format("{0}\n{1}",
					portNameShort, Util.RemoveCenterCharsOnLongString (crp.SerialNumber, 12)));
		grid_micro_discover.Attach (l, 0, i, 1, 1);

		// ---- column 1
		Gtk.ProgressBar c1_pb = new Gtk.ProgressBar ();
		c1_pb.Text = "----"; //to have height
		c1_pb.SetSizeRequest (125, -1);
		c1_pb.ShowText = true;
		if (bgShiftedIsDark)
			c1_pb.Name = "lightCss";
		else
			c1_pb.Name = "darkCss";

		if (alreadyDiscovered)
		{
			c1_pb.Fraction = 1;
			c1_pb.Text = ChronopicRegisterPort.TypePrint (crp.Type);
			c1_progressbar_microAlreadyDiscovered_l.Add (c1_pb);
		} else
			c1_progressbar_microNotDiscovered_l.Add (c1_pb);

		grid_micro_discover.Attach (c1_pb, 1, i, 1, 1);

		// ---- column 2
		Gtk.Label label = new Gtk.Label (); //used on NC and ----
		Gtk.Button bSelect = new Gtk.Button (); //used on Select!
		Gtk.Box c2_box_b_or_label = new Gtk.Box (Gtk.Orientation.Horizontal, 0);

		// ---- column 3
		Gtk.Button bDebug = new Gtk.Button ();
		bDebug.Label = "";
		Gtk.Button bForget = new Gtk.Button ();
		bForget.Label = "";
		Gtk.Button bManuallyAssign = new Gtk.Button ();
		bManuallyAssign.Label = "";
		Gtk.Box c3_advancedButtons = new Gtk.Box (Gtk.Orientation.Horizontal, 6);

		if (alreadyDiscovered)
		{
			if (discoverMatchCurrentMode (crp.Type))
			{
				bSelect.Sensitive = true;
				label.Text = useThisStr;

				if (shouldHaveDebugAndForgetButtons (current_mode, crp.Type))
				{
					bDebug.Sensitive = true;
					bDebug.Label = debugThisStr;
					bDebug.Clicked -= new EventHandler (on_discover_debug_this_clicked); //needed. if not: called multiple times
					bDebug.Clicked += new EventHandler (on_discover_debug_this_clicked);
				}
			} else // crp.Type does not match current_mode
			{
				bSelect.Sensitive = false;
				label.Text = Catalog.GetString ("NC");

				box_micro_discover_nc.Visible = true;
				label_micro_discover_nc_comment.Visible = true;
			}

			// we can forget a device that has been assigned to current_mode or others
			bForget.Sensitive = true;
			bForget.Label = forgetThisStr;
			bForget.Clicked -= new EventHandler (on_discover_forget_this_clicked); //needed. if not: called multiple times
			bForget.Clicked += new EventHandler (on_discover_forget_this_clicked);

			//label_microAlreadyDiscovered_l.Add (label);
			bSelect.Label = label.Text;
			button_microAlreadyDiscovered_l.Add (bSelect);
			label_microAlreadyDiscovered_l.Add (label); //just to make not visible later
			portAlreadyDiscovered_l.Add (crp);
			bSelect.Clicked -= new EventHandler (on_discover_use_this_clicked); //needed. if not: called multiple times
			bSelect.Clicked += new EventHandler (on_discover_use_this_clicked);

			buttonDebug_alreadyDiscovered_l.Add (bDebug);
			buttonDebug_alreadyDiscovered_crp_l.Add (crp);

			buttonForget_alreadyDiscovered_l.Add (bForget);
			buttonForget_alreadyDiscovered_crp_l.Add (crp);

			buttonManuallyAssign_alreadyDiscovered_l.Add (bManuallyAssign);
			buttonManuallyAssign_alreadyDiscovered_crp_l.Add (crp);
		} else {
			bSelect.Sensitive = false;

			//b.Label = "----";
			label_microNotDiscovered_l.Add (label);
			label.Text = "----";
			bSelect.Label = label.Text;
			button_microNotDiscovered_l.Add (bSelect);

			buttonDebug_notDiscovered_l.Add (bDebug);
			buttonDebug_notDiscovered_crp_l.Add (crp);

			buttonForget_notDiscovered_l.Add (bForget);
			buttonForget_notDiscovered_crp_l.Add (crp);

			buttonManuallyAssign_notDiscovered_l.Add (bManuallyAssign);
			buttonManuallyAssign_notDiscovered_crp_l.Add (crp);
		}

		//c2_box_b_or_label has button and label, if compatible with mode will show button, if not, label
		c2_box_b_or_label.PackStart (label, false, false, 0);
		c2_box_b_or_label.PackStart (bSelect, false, false, 0);

		/* done after grid_micro_discover.ShowAll ();
		if (label.Text == "NC" || label.Text == "----")
		{
			label.Visible = true;
			bSelect.Visible = false;
		} else {
			label.Visible = false;
			bSelect.Visible = true;
		}
		*/

		grid_micro_discover.Attach (c2_box_b_or_label, 2, i, 1, 1);

		// c3 advanced
		c3_advancedButtons.PackStart (bDebug, false, false, 0);
		c3_advancedButtons.PackStart (bForget, false, false, 0);
		c3_advancedButtons.PackStart (bManuallyAssign, false, false, 0);
		grid_micro_discover.Attach (c3_advancedButtons, 3, i, 1, 1);
	}

	private bool shouldHaveDebugAndForgetButtons (Constants.Modes mode, ChronopicRegisterPort.Types crpType)
	{
		if (
				(current_mode == Constants.Modes.JUMPSSIMPLE || current_mode == Constants.Modes.JUMPSREACTIVE ||
				 current_mode == Constants.Modes.RUNSSIMPLE || current_mode == Constants.Modes.RUNSINTERVALLIC) &&
				crpType == ChronopicRegisterPort.Types.CONTACTS
		   )
			return true;

		if (
				(current_mode == Constants.Modes.RUNSSIMPLE || current_mode == Constants.Modes.RUNSINTERVALLIC) &&
				crpType == ChronopicRegisterPort.Types.RUN_WIRELESS
		   )
			return true;

		if (current_mode == Constants.Modes.RUNSENCODER && crpType == ChronopicRegisterPort.Types.ARDUINO_RUN_ENCODER)
			return true;

		if (Constants.ModeIsFORCESENSOR (current_mode) && crpType == ChronopicRegisterPort.Types.ARDUINO_FORCE)
			return true;

		if (Constants.ModeIsENCODER (current_mode) && crpType == ChronopicRegisterPort.Types.ENCODER)
			return true;

		return false;
	}

	public void ShowAdvanced (bool show)
	{
		showAdvanced = show;

		if (showAdvanced) {
			label_discover_advanced.Text = Catalog.GetString ("Hide advanced");
			image_discover_advanced.Visible = false;
		} else {
			label_discover_advanced.Text = Catalog.GetString ("Show advanced");
			image_discover_advanced.Visible = true;
		}

		//lAdvanced && buttonDebug_* is defined on setup_grid_micro_discover () if it is not called, it will be null
		if (lAdvanced == null || buttonDebug_alreadyDiscovered_l == null || buttonDebug_notDiscovered_l == null) // no need to change also for Forget lists
			return;

		lAdvanced.Visible = showAdvanced;

		for (int i = 0; i < buttonDebug_alreadyDiscovered_l.Count; i ++)
			if (buttonDebug_alreadyDiscovered_l[i].Label == debugThisStr)
				buttonDebug_alreadyDiscovered_l[i].Visible = showAdvanced;
		for (int i = 0; i < buttonDebug_notDiscovered_l.Count; i ++)
			if (buttonDebug_notDiscovered_l[i].Label == debugThisStr)
				buttonDebug_notDiscovered_l[i].Visible = showAdvanced;

		for (int i = 0; i < buttonForget_alreadyDiscovered_l.Count; i ++)
			if (buttonForget_alreadyDiscovered_l[i].Label == forgetThisStr)
				buttonForget_alreadyDiscovered_l[i].Visible = showAdvanced;
		for (int i = 0; i < buttonForget_notDiscovered_l.Count; i ++)
			if (buttonForget_notDiscovered_l[i].Label == forgetThisStr)
				buttonForget_notDiscovered_l[i].Visible = showAdvanced;

		for (int i = 0; i < buttonManuallyAssign_alreadyDiscovered_l.Count; i ++)
			if (buttonManuallyAssign_alreadyDiscovered_l[i].Label == manuallyAssignThisStr)
				buttonManuallyAssign_alreadyDiscovered_l[i].Visible = showAdvanced;
		for (int i = 0; i < buttonManuallyAssign_notDiscovered_l.Count; i ++)
			if (buttonManuallyAssign_notDiscovered_l[i].Label == manuallyAssignThisStr)
				buttonManuallyAssign_notDiscovered_l[i].Visible = showAdvanced;
	}

	private void discoverDo ()
	{
		microDiscover.DiscoverOneMode (current_mode);
	}
	private bool pulseDiscoverGTK ()
	{
		if(microDiscover == null)
		{
			Thread.Sleep (200);
			return true;
		}

		//gui updates while thread is alive
		for (int i = 0; i < c1_progressbar_microNotDiscovered_l.Count; i ++)
		{
			//progressbars
			Gtk.ProgressBar pb = c1_progressbar_microNotDiscovered_l[i];
			if (microDiscover.ProgressBar_l[i] == MicroDiscover.Status.NotStarted)
			{
				pb.Text = "----"; //to have height
				pb.Fraction = 0;
			} else if (pb.Text == forgottenStr || pb.Text.StartsWith (manuallyAssignedStr))
			{
				// do not assign text if text is already "Forgotten" because while detection will show e.g. Encoder again when we have hit Forgot
			} else if (microDiscover.ProgressBar_l[i] == MicroDiscover.Status.Done)
			{
				pb.Text = microDiscover.ProgressBar_l[i].ToString();
				pb.Fraction = 1;
				pb.Text = microDiscover.ProgressBar_l[i].ToString();
			} else {
				if (microDiscover.Cancel)
					pb.Text = Catalog.GetString("Cancelling");
				else
					pb.Text = microDiscover.ProgressBar_l[i].ToString();
				pb.Pulse ();
			}

			if (i < microDiscover.Discovered_l.Count)
			{
				if ((c1_progressbar_microNotDiscovered_l[i]).Text == forgottenStr ||
						(c1_progressbar_microNotDiscovered_l[i]).Text.StartsWith (manuallyAssignedStr))
				{
					// do not assign text if text is already "Forgotten" because while detection will show e.g. Encoder again when we have hit Forgot
				} else if (discoverMatchCurrentMode (microDiscover.Discovered_l[i]))
				{
					(c1_progressbar_microNotDiscovered_l[i]).Text = ChronopicRegisterPort.TypePrint(microDiscover.Discovered_l[i]);

					button_microNotDiscovered_l[i].Sensitive = true;
					button_microNotDiscovered_l[i].Label = useThisStr;
					button_microNotDiscovered_l[i].Clicked -= new EventHandler(on_discover_use_this_clicked); //needed. if not: called multiple times
					button_microNotDiscovered_l[i].Clicked += new EventHandler(on_discover_use_this_clicked);
					button_microNotDiscovered_l[i].Visible = true;
					label_microNotDiscovered_l[i].Visible = false;

					if (shouldHaveDebugAndForgetButtons (current_mode, microDiscover.Discovered_l[i]))
					{
						// debug
						buttonDebug_notDiscovered_crp_l[i].Type = microDiscover.Discovered_l[i];

						if (showAdvanced)
							buttonDebug_notDiscovered_l[i].Visible = true;
						buttonDebug_notDiscovered_l[i].Sensitive = true;
						buttonDebug_notDiscovered_l[i].Label = debugThisStr;
						buttonDebug_notDiscovered_l[i].Clicked -= new EventHandler (on_discover_debug_this_clicked); //needed. if not: called multiple times
						buttonDebug_notDiscovered_l[i].Clicked += new EventHandler (on_discover_debug_this_clicked);

						// forget
						if (! chronopicRegister.SerialNumberIsNotUnique (buttonForget_notDiscovered_crp_l[i].SerialNumber)) // A50285BI shoud have not a forget it as it is always forgotten
						{
							buttonForget_notDiscovered_crp_l[i].Type = microDiscover.Discovered_l[i];

							if (showAdvanced)
								buttonForget_notDiscovered_l[i].Visible = true;
							buttonForget_notDiscovered_l[i].Sensitive = true;
							buttonForget_notDiscovered_l[i].Label = forgetThisStr;
							buttonForget_notDiscovered_l[i].Clicked -= new EventHandler (on_discover_forget_this_clicked); //needed. if not: called multiple times
							buttonForget_notDiscovered_l[i].Clicked += new EventHandler (on_discover_forget_this_clicked);
						}
					}
				} else {
					button_microNotDiscovered_l[i].Visible = false;
					label_microNotDiscovered_l[i].Text = Catalog.GetString ("NC");
					label_microNotDiscovered_l[i].Visible = true;

					buttonManuallyAssign_notDiscovered_crp_l[i].Type = microDiscover.Discovered_l[i];

					// A50285BI cannot be manually assigned
					if (! chronopicRegister.SerialNumberIsNotUnique (buttonManuallyAssign_notDiscovered_crp_l[i].SerialNumber))
					{
						if (showAdvanced)
							buttonManuallyAssign_notDiscovered_l[i].Visible = true;
						buttonManuallyAssign_notDiscovered_l[i].Sensitive = true;
						buttonManuallyAssign_notDiscovered_l[i].Label = manuallyAssignThisStr;
						buttonManuallyAssign_notDiscovered_l[i].Clicked -= new EventHandler (on_discover_manuallyAssign_this_clicked); //needed. if not: called multiple times
						buttonManuallyAssign_notDiscovered_l[i].Clicked += new EventHandler (on_discover_manuallyAssign_this_clicked);

						box_micro_discover_nc.Visible = true;
						label_micro_discover_nc_comment.Visible = true;
					}
				}
			}

			//show label of busy ports (eg used by Arduino IDE)
			if (microDiscover.ConnectError_l != null && microDiscover.ConnectError_l.Count > 0)
			{
				label_micro_discover_connect_error.Text = Catalog.GetString ("Cannot connect to ports:" ) +
					UtilList.ListStringToString (microDiscover.ConnectError_l, ", ");
				label_micro_discover_connect_error.Visible = true;
			}

		}

		if(! discoverThread.IsAlive)
		{
			// 3) end this pulse
			LogB.Information("pulseDiscoverGTK ending here");
			LogB.ThreadEnded();

			for (int i = 0; i < c1_progressbar_microNotDiscovered_l.Count; i ++)
			{
				if (microDiscover.Cancel &&
						 microDiscover.ProgressBar_l[i] != MicroDiscover.Status.Done)
					(c1_progressbar_microNotDiscovered_l[i]).Text = Catalog.GetString("Cancelled");

				(c1_progressbar_microNotDiscovered_l[i]).Fraction = 1;

				if ( ! (i < microDiscover.Discovered_l.Count && discoverMatchCurrentMode (microDiscover.Discovered_l[i])) &&
						! c1_progressbar_microNotDiscovered_l[i].Text.StartsWith (manuallyAssignedStr)
				   )
					(c1_progressbar_microNotDiscovered_l[i]).Text = "";
			}

			Image_cancel_close_isClose ();

			if (discoverCloseAfterCancel)
			{
				//on_button_micro_discover_cancel_close_clicked (new object (), new EventArgs ());
				CancelCloseFromUser ();
			}

			button_micro_discover_refresh.Sensitive = true;

			return false;
		}

		Thread.Sleep (200);
		return true;
	}

	private bool discoverMatchCurrentMode (ChronopicRegisterPort.Types crpt)
	{
		LogB.Information(string.Format(
					"at discoverMatchCurrentMode current_mode: {0}, crpt: {1}",
					current_mode, crpt));

		if (
				(current_mode == Constants.Modes.JUMPSSIMPLE || current_mode == Constants.Modes.JUMPSREACTIVE) &&
				crpt == ChronopicRegisterPort.Types.CONTACTS )
			return true;
		else if (current_mode == Constants.Modes.JUMPSSIMPLE && crpt == ChronopicRegisterPort.Types.FOURPLATFORMS)
			return true;
		else if (
				(current_mode == Constants.Modes.RUNSSIMPLE || current_mode == Constants.Modes.RUNSINTERVALLIC) &&
				(crpt == ChronopicRegisterPort.Types.CONTACTS || crpt == ChronopicRegisterPort.Types.RUN_WIRELESS) )
			return true;
		else if (current_mode == Constants.Modes.WILIGHT && crpt == ChronopicRegisterPort.Types.RUN_WIRELESS)
			return true;
		else if (Constants.ModeIsFORCESENSOR (current_mode) && crpt == ChronopicRegisterPort.Types.ARDUINO_FORCE)
			return true;
		else if (current_mode == Constants.Modes.RUNSENCODER && crpt == ChronopicRegisterPort.Types.ARDUINO_RUN_ENCODER)
			return true;
		else if (
				(current_mode == Constants.Modes.POWERGRAVITATORY ||
				 current_mode == Constants.Modes.POWERINERTIAL) &&
				crpt == ChronopicRegisterPort.Types.ENCODER )
			return true;
		else if (current_mode == Constants.Modes.OTHER && crpt == ChronopicRegisterPort.Types.FOURPLATFORMS)
			return true;

		return false;
	}

	private void on_discover_use_this_clicked (object o, EventArgs args)
	{
		Button bPress = (Button) o;
		bool success = false;

		// 1) test the discovered by MicroDiscover
		//loop the list to know which button was
		for (int i = 0 ; i < button_microNotDiscovered_l.Count; i ++)
			if (button_microNotDiscovered_l[i] == bPress)
			{
				// update the list
				chronopicRegister.SetType (microDiscover.ToDiscover_l[i].SerialNumber,
						microDiscover.Discovered_l[i]);

				// update the SQL (since 20 oct 2023 it will only INSERT)
				if (SqliteChronopicRegister.Exists (false, microDiscover.ToDiscover_l[i].SerialNumber))
					SqliteChronopicRegister.Update (false,
							microDiscover.ToDiscover_l[i], microDiscover.Discovered_l[i]);
				else
					SqliteChronopicRegister.Insert (false,
							microDiscover.ToDiscover_l[i]);

				portSelected = microDiscover.ToDiscover_l[i];

				/* instead of connect, just do changes on gui in order to be used
				if(! portFSOpened)
				{
					*/ /*
					discoverThread = new Thread (new ThreadStart (forceSensorConnectDo));
					GLib.Idle.Add (new GLib.IdleHandler (pulseDiscoverGTK));
					discoverThread.Start();
					if(! forceSensorConnectDo ())
						LogB.Information("could'n connect");
						*/ /*
				} else
					on_button_micro_discover_cancel_close_clicked (new object (), new EventArgs ());
				*/

				success = true;
			}

		// 2) test the already discovered
		for (int i = 0 ; i < button_microAlreadyDiscovered_l.Count; i ++)
			if (button_microAlreadyDiscovered_l[i] == bPress)
			{
				portSelected = portAlreadyDiscovered_l[i];
				success = true;
			}

		if (success)
		{
			//if we are discovering, on_button_micro_discover_cancel_close_clicked will cancel
			//make discoverCloseAfterCancel = true to also close the window on pulse
			discoverCloseAfterCancel = discoverThread.IsAlive;

			//on_button_micro_discover_cancel_close_clicked (new object (), new EventArgs ());
			CancelCloseFromUser ();
		}
	}

	/*
	 * ---- debug this ---->
	 */

	private void on_discover_debug_this_clicked (object o, EventArgs args)
	{
		// TODO: hability to send a log
		Button bPress = (Button) o;

		// 1) test the discovered by MicroDiscover
		//loop the list to know which button was
		for (int i = 0 ; i < buttonDebug_alreadyDiscovered_l.Count; i ++)
			if (buttonDebug_alreadyDiscovered_l[i] == bPress)
				on_discover_debug_this_clicked_do (portAlreadyDiscovered_l[i], buttonDebug_alreadyDiscovered_l[i]);

		for (int i = 0 ; i < buttonDebug_notDiscovered_l.Count; i ++)
			if (buttonDebug_notDiscovered_l[i] == bPress)
				on_discover_debug_this_clicked_do (buttonDebug_notDiscovered_crp_l[i], buttonDebug_notDiscovered_l[i]);
	}

	private void on_discover_debug_this_clicked_do (ChronopicRegisterPort crp, Gtk.Button bDebug)
	{
		if (crp.Type != ChronopicRegisterPort.Types.CONTACTS &&
				crp.Type != ChronopicRegisterPort.Types.RUN_WIRELESS && //WICHRO
				crp.Type != ChronopicRegisterPort.Types.ARDUINO_RUN_ENCODER && //Race Analyzer
				crp.Type != ChronopicRegisterPort.Types.ARDUINO_FORCE &&
				crp.Type != ChronopicRegisterPort.Types.ENCODER)
			return;

		if (crp.Type == ChronopicRegisterPort.Types.CONTACTS)
		{
			chronopicTestWin = ChronopicTestWindow.Show (parentWin);
		}
		else if (crp.Type == ChronopicRegisterPort.Types.RUN_WIRELESS ||
				crp.Type == ChronopicRegisterPort.Types.ARDUINO_RUN_ENCODER ||
				crp.Type == ChronopicRegisterPort.Types.ARDUINO_FORCE)
		{
			//force sensor needs to wait 3s to start capturing
			bDebugCurrent = bDebug;
			crpCurrent = crp;
			dd = null;

			ddTotalSeconds = 5.99;
			if (crp.Type == ChronopicRegisterPort.Types.ARDUINO_RUN_ENCODER)
				ddTotalSeconds = 5.99 + 3; //capturing data
			if (crp.Type == ChronopicRegisterPort.Types.RUN_WIRELESS)
				ddTotalSeconds = 5.99 + 30; //30 s for discovering the terminals

			stopwatch = new Stopwatch ();
			stopwatch.Start ();

			grid_micro_discover.Sensitive = false;
			check_discover_advanced.Sensitive = false;
			button_micro_discover_cancel_close.Sensitive = false;

			if (crp.Type == ChronopicRegisterPort.Types.RUN_WIRELESS)
				debugThread = new Thread (new ThreadStart (debugWichro));
			else if (crp.Type == ChronopicRegisterPort.Types.ARDUINO_RUN_ENCODER)
				debugThread = new Thread (new ThreadStart (debugRaceAnalyzer));
			else //if (crp.Type == ChronopicRegisterPort.Types.ARDUINO_FORCE)
				debugThread = new Thread (new ThreadStart (debugForceSensor));

			button_micro_discover_refresh.Sensitive = false;
			GLib.Idle.Add (new GLib.IdleHandler (pulseDebugGTK));
			debugThread.Start();
		}
		else { //if (crp.Type == ChronopicRegisterPort.Types.ENCODER)
			dd = new DebugEncoder (crp);
			new DialogMessage (dd.Title, Constants.MessageTypes.INFO, 450, 400, dd.Str);
		}
	}

	/*
	 * <---- debug this ----
	 */
	/*
	 * ---- forget this ---->
	 */

	private void on_discover_forget_this_clicked (object o, EventArgs args)
	{
		Button bPress = (Button) o;

		// 1) test the discovered by MicroDiscover
		//loop the list to know which button was
		for (int i = 0 ; i < buttonForget_alreadyDiscovered_l.Count; i ++)
			if (buttonForget_alreadyDiscovered_l[i] == bPress)
				on_discover_forget_this_clicked_do (portAlreadyDiscovered_l[i], true); // alreadyDiscovered

		for (int i = 0 ; i < buttonForget_notDiscovered_l.Count; i ++)
			if (buttonForget_notDiscovered_l[i] == bPress)
				on_discover_forget_this_clicked_do (buttonForget_notDiscovered_crp_l[i], false); // not alreadyDiscovered
	}

	private void on_discover_forget_this_clicked_do (ChronopicRegisterPort crp, bool alreadyDiscovered)
	{
		// 1. forget the device on SQL
		if (SqliteChronopicRegister.Exists (false, crp.SerialNumber))
			SqliteChronopicRegister.Delete (false, crp);

		// 2. changes on gui
		if (alreadyDiscovered)
		{
			for (int i = 0; i < buttonForget_alreadyDiscovered_crp_l.Count; i ++)
				if (buttonForget_alreadyDiscovered_crp_l[i] == crp)
				{
					guiMarkAlreadyDiscoveredCrpAsForgotten (i);
					return;
				}
		}
		else {
			for (int i = 0; i < buttonForget_notDiscovered_crp_l.Count; i ++)
				if (buttonForget_notDiscovered_crp_l[i] == crp)
				{
					guiMarkNotDiscoveredCrpAsForgotten (i);
					return;
				}
		}
	}
	private void guiMarkAlreadyDiscoveredCrpAsForgotten (int i)
	{
		c1_progressbar_microAlreadyDiscovered_l[i].Text = forgottenStr;
		button_microAlreadyDiscovered_l[i].Sensitive = false;
		buttonDebug_alreadyDiscovered_l[i].Sensitive = false;
		buttonForget_alreadyDiscovered_l[i].Sensitive = false;
	}

	private void guiMarkNotDiscoveredCrpAsForgotten (int i)
	{
		c1_progressbar_microNotDiscovered_l[i].Text = forgottenStr;
		button_microNotDiscovered_l[i].Sensitive = false;
		buttonDebug_notDiscovered_l[i].Sensitive = false;
		buttonForget_notDiscovered_l[i].Sensitive = false;
	}

	/*
	 * <---- forget this ----
	 */
	/*
	 * ---- manuallyAssign ---->
	 */

	private void on_discover_manuallyAssign_this_clicked (object o, EventArgs args)
	{
		// 0. Exit if not implemented yet for this mode
		if (
				current_mode != Constants.Modes.JUMPSSIMPLE &&
				current_mode != Constants.Modes.JUMPSREACTIVE &&
				current_mode != Constants.Modes.RUNSSIMPLE &&
				current_mode != Constants.Modes.RUNSINTERVALLIC &&
				current_mode != Constants.Modes.RUNSENCODER &&
				! Constants.ModeIsFORCESENSOR (current_mode) &&
				! Constants.ModeIsENCODER (current_mode)
		      )
		{
			new DialogMessage ("TODO", Constants.MessageTypes.INFO, 450, 400, " TODO ");
			return;
		}

		Button bPress = (Button) o;

		// 1) test the discovered by MicroDiscover
		//loop the list to know which button was
		bool found = false;
		for (int i = 0 ; i < buttonManuallyAssign_notDiscovered_l.Count; i ++)
			if (buttonManuallyAssign_notDiscovered_l[i] == bPress)
			{
				crpManuallyAssign = buttonManuallyAssign_notDiscovered_crp_l[i];
				found = true;
			}

		if (! found)
			return;

		button_manually_assign1.Visible = true;
		button_manually_assign2.Visible = false;

		//TODO: fourPlatforms
		if (current_mode == Constants.Modes.JUMPSSIMPLE || current_mode == Constants.Modes.JUMPSREACTIVE)
			button_manually_assign1.Label = Catalog.GetString ("Chronopic");
		else if (current_mode == Constants.Modes.RUNSSIMPLE || current_mode == Constants.Modes.RUNSINTERVALLIC) {
			button_manually_assign1.Label =
				ChronopicRegisterPort.TypePrint (ChronopicRegisterPort.Types.RUN_WIRELESS); //WICHRO
			button_manually_assign2.Label =
				Catalog.GetString ("Old cabled photocells");
			button_manually_assign2.Visible = true;
		}
		else if (current_mode == Constants.Modes.RUNSENCODER)
			button_manually_assign1.Label =
				ChronopicRegisterPort.TypePrint (ChronopicRegisterPort.Types.ARDUINO_RUN_ENCODER);
		else if (Constants.ModeIsFORCESENSOR (current_mode))
			button_manually_assign1.Label =
				ChronopicRegisterPort.TypePrint (ChronopicRegisterPort.Types.ARDUINO_FORCE);
		else if (Constants.ModeIsENCODER (current_mode))
			button_manually_assign1.Label =
				ChronopicRegisterPort.TypePrint (ChronopicRegisterPort.Types.ENCODER);

		notebook_micro_discover.CurrentPage = Convert.ToInt32 (DiscoverWindow.Notebook_micro_discover_pages.USB_ASSIGN_MANUALLY);
	}

	private void on_button_manually_assign1_clicked (object o, EventArgs args)
	{
		if (current_mode == Constants.Modes.JUMPSSIMPLE || current_mode == Constants.Modes.JUMPSREACTIVE)
			crpManuallyAssign.Type = ChronopicRegisterPort.Types.CONTACTS;
		else if (current_mode == Constants.Modes.RUNSSIMPLE || current_mode == Constants.Modes.RUNSINTERVALLIC)
			crpManuallyAssign.Type = ChronopicRegisterPort.Types.RUN_WIRELESS;
		else if (current_mode == Constants.Modes.RUNSENCODER)
			crpManuallyAssign.Type = ChronopicRegisterPort.Types.ARDUINO_RUN_ENCODER;
		else if (Constants.ModeIsFORCESENSOR (current_mode))
			crpManuallyAssign.Type = ChronopicRegisterPort.Types.ARDUINO_FORCE;
		else if (Constants.ModeIsENCODER (current_mode))
			crpManuallyAssign.Type = ChronopicRegisterPort.Types.ENCODER;

		manually_assign_finish ();
	}

	private void on_button_manually_assign2_clicked (object o, EventArgs args)
	{
		//if (current_mode == Constants.Modes.RUNSSIMPLE || current_mode == Constants.Modes.RUNSINTERVALLIC)
			crpManuallyAssign.Type = ChronopicRegisterPort.Types.CONTACTS;

		manually_assign_finish ();
	}

	private void manually_assign_finish ()
	{
		//1.  manuallyAssign the device on SQL
		if (SqliteChronopicRegister.Exists (false, crpManuallyAssign.SerialNumber))
			SqliteChronopicRegister.Update (false, crpManuallyAssign, crpManuallyAssign.Type);
		else
			SqliteChronopicRegister.Insert (false, crpManuallyAssign);

		for (int i = 0; i < buttonManuallyAssign_notDiscovered_crp_l.Count; i ++)
			if (buttonManuallyAssign_notDiscovered_crp_l[i] == crpManuallyAssign)
				guiMarkNotDiscoveredCrpAsManuallyAssigned (i, crpManuallyAssign);

		notebook_micro_discover.CurrentPage = Convert.ToInt32 (DiscoverWindow.Notebook_micro_discover_pages.USB);
	}

	private void guiMarkNotDiscoveredCrpAsManuallyAssigned (int i, ChronopicRegisterPort crp)
	{
		c1_progressbar_microNotDiscovered_l[i].Text =
			manuallyAssignedStr + ChronopicRegisterPort.TypePrint (crp.Type);
		buttonManuallyAssign_notDiscovered_l[i].Sensitive = false;

		button_microNotDiscovered_l[i].Label = useThisStr;
		button_microNotDiscovered_l[i].Sensitive = true;
		button_microNotDiscovered_l[i].Visible = true;

		label_microNotDiscovered_l[i].Visible = false;

		// update Discovered_l to be used at: on_discover_use_this_clicked
		microDiscover.ToDiscover_l[i].Type = crp.Type;
		microDiscover.Discovered_l[i] = crp.Type;
		button_microNotDiscovered_l[i].Clicked -= new EventHandler (on_discover_use_this_clicked); //needed. if not: called multiple times
		button_microNotDiscovered_l[i].Clicked += new EventHandler (on_discover_use_this_clicked);
	}

	/*
	 * <---- manuallyAssign ----
	 */

	static Thread debugThread;
	private DebugDevices dd;
	private double ddTotalSeconds;
	private ChronopicRegisterPort crpCurrent;
	private Gtk.Button bDebugCurrent;
	private Stopwatch stopwatch;

	// Using a thread: when a DialogMessage window is shown after some time (arduino start)
	private void debugWichro ()
	{
		dd = new DebugWichro (crpCurrent);
	}
	private void debugRaceAnalyzer ()
	{
		dd = new DebugRaceAnalyzer (crpCurrent);
	}
	private void debugForceSensor ()
	{
		dd = new DebugForceSensor (crpCurrent);
	}
	private bool pulseDebugGTK ()
	{
		if (! debugThread.IsAlive)
		{
			new DialogMessage (dd.Title, Constants.MessageTypes.INFO, 450, 400, dd.Str);
			bDebugCurrent.Label = "Test it!";
			stopwatch.Stop ();

			grid_micro_discover.Sensitive = true;
			check_discover_advanced.Sensitive = true;
			button_micro_discover_cancel_close.Sensitive = true;
			button_micro_discover_refresh.Sensitive = true;

			return false;
		}

		int seconds = Convert.ToInt32 (ddTotalSeconds -stopwatch.Elapsed.TotalSeconds);
		if (seconds < 0)
		       seconds = 0;

		bDebugCurrent.Label = string.Format ("Please, wait {0} s.", seconds);

		Thread.Sleep (100);
		return true;
	}

	/*
	private bool pulseDiscoverConnectGTK ()
	{
		if(! discoverThread.IsAlive)
		{
			// 3) end this pulse
			LogB.Information("pulseDiscoverConnectGTK ending here");
			LogB.ThreadEnded();

			return false;
		}

		Thread.Sleep (200);
		return true;
	}
	*/

	//private void on_button_micro_discover_cancel_close_clicked (object o, EventArgs args)
	public void CancelCloseFromUser ()
	{
		if (discoverThread != null && discoverThread.IsAlive && microDiscover != null)
		{
			microDiscover.Cancel = true;
			//microDiscover.CancelWrite (); //does not work
		} else
			FakeButtonClose.Click ();
	}

	public ChronopicRegister ChronopicRegisterGet {
		get { return chronopicRegister; }
	}

	//the port that user clicked on "Select!"
	public ChronopicRegisterPort PortSelected {
		get { return portSelected; }
	}
}
