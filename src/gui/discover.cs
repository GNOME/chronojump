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
 *  Copyright (C) 2016-2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List<T>
using Gdk;
using Gtk;
using Mono.Unix;
using System.Threading;
using System.IO.Ports;


public class DiscoverWindow
{
	//TODO instead of 4 lists, have List<microDiscoveGui>
	List<Gtk.ProgressBar> progressbar_microNotDiscovered_l;
	List<Gtk.Button> button_microNotDiscovered_l;
	List<Gtk.Label> label_microNotDiscovered_l; //to use labels for ---- and NC instead of buttons

	List<ChronopicRegisterPort> portAlreadyDiscovered_l;
	List<Gtk.Button> button_microAlreadyDiscovered_l;
	List<Gtk.Label> label_microAlreadyDiscovered_l; //to be able to Visible = false after ShowAll ()
	List<Gtk.Button> button_debug_l; //to test debug a device

	static bool discoverCloseAfterCancel; //is true when select useThis while reading other devices
	static Thread discoverThread;
	static MicroDiscover microDiscover;
	public Gtk.Button FakeButtonClose;

	private Constants.Modes current_mode;
	private ChronopicRegister chronopicRegister;
	private Gtk.Grid grid_micro_discover;
	private Gtk.Box box_micro_discover_nc;
	private Gtk.Image image_button_micro_discover_cancel_close;
	private Gtk.Label label_button_micro_discover_cancel_close;
	private Gtk.Image image_discover_mode;
	private Gtk.Label label_micro_discover_connect_error;
	private string useThisStr = "Select!";

	private ChronopicRegisterPort portSelected;

	public DiscoverWindow (Constants.Modes current_mode, ChronopicRegister chronopicRegister,
			Gtk.Label label_micro_discover_not_found,
			Gtk.Grid grid_micro_discover,
			Gtk.Box box_micro_discover_nc,
			Gtk.Image image_button_micro_discover_cancel_close,
			Gtk.Label label_button_micro_discover_cancel_close,
			string iconModeStr,
			Gtk.Label label_micro_discover_connect_error
			)
	{
		this.current_mode = current_mode;
		this.chronopicRegister = chronopicRegister;
		this.grid_micro_discover = grid_micro_discover;
		this.box_micro_discover_nc = box_micro_discover_nc;
		this.image_button_micro_discover_cancel_close = image_button_micro_discover_cancel_close;
		this.label_button_micro_discover_cancel_close = label_button_micro_discover_cancel_close;
		this.label_micro_discover_connect_error = label_micro_discover_connect_error;

		// 1) set up gui

		FakeButtonClose = new Gtk.Button();
		portSelected = new ChronopicRegisterPort ("");
		image_discover_mode = new Gtk.Image (Chronojump.MyPixbuf.Get (null, Util.GetImagePath(false) + iconModeStr));

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

		image_button_micro_discover_cancel_close.Pixbuf =
				Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "image_cancel.png");
		label_button_micro_discover_cancel_close.Text = Catalog.GetString("Cancel");

		if (alreadyDiscovered_l.Count > 0 || notDiscovered_l.Count > 0)
		{
			microDiscover = new MicroDiscover (notDiscovered_l);

			label_micro_discover_not_found.Visible = false;
			setup_grid_micro_discover_l (alreadyDiscovered_l, notDiscovered_l);
			discoverCloseAfterCancel = false;

			discoverThread = new Thread (new ThreadStart (discoverDo));
			GLib.Idle.Add (new GLib.IdleHandler (pulseDiscoverGTK));
			discoverThread.Start();
		} else {
			UtilGtk.RemoveChildren (grid_micro_discover);

			label_micro_discover_not_found.Text = Catalog.GetString ("Device not found.");
			label_micro_discover_not_found.Visible = true;

			image_button_micro_discover_cancel_close.Pixbuf =
				Chronojump.MyPixbuf.Get (null, Util.GetImagePath (false) + "image_close.png");
			label_button_micro_discover_cancel_close.Text = Catalog.GetString("Close");
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



	private void setup_grid_micro_discover_l (
			List<ChronopicRegisterPort> alreadyDiscovered_l,
			List<ChronopicRegisterPort> notDiscovered_l)
	{
		// 1) delete widgets of previous calls
		UtilGtk.RemoveChildren (grid_micro_discover);

		grid_micro_discover.ColumnSpacing = 20;
		grid_micro_discover.RowSpacing = 14;

		// 2) create the lists of widgets to be able to access later
		progressbar_microNotDiscovered_l = new List<Gtk.ProgressBar> ();
		button_microNotDiscovered_l = new List<Gtk.Button> ();
		label_microNotDiscovered_l = new List<Gtk.Label> ();
		portAlreadyDiscovered_l = new List<ChronopicRegisterPort> ();
		button_microAlreadyDiscovered_l = new List<Gtk.Button> ();
		label_microAlreadyDiscovered_l = new List<Gtk.Label> ();
		button_debug_l = new List<Gtk.Button> ();

		// 3) create widgets, lists, attach to table and show all

		// 3a) create table header row
		Gtk.Label l0 = new Gtk.Label ("<b>" + Catalog.GetString ("Device") + "</b>");
		l0.UseMarkup = true;

		Gtk.Label l1 = new Gtk.Label ("<b>" + Catalog.GetString ("Compatibility with") + "</b>");
		l1.UseMarkup = true;
		Gtk.Box hbox_l1 = new Gtk.Box (Gtk.Orientation.Horizontal, 10);
		hbox_l1.PackStart (l1, false, false, 0);
		hbox_l1.PackStart (image_discover_mode, false, false, 0);
		//hbox_l1.Hexpand = true; //this does not work, so create a parent and expand:
		Gtk.Box hbox_l1_parent = new Gtk.Box (Gtk.Orientation.Horizontal, 0);
		hbox_l1_parent.PackStart (hbox_l1, true, false, 0);

		grid_micro_discover.Attach (l0, 0, 0, 1, 1);
		grid_micro_discover.Attach (hbox_l1_parent, 2, 0, 2, 1);

		// 3b) create a row for each device
		for (int i = 0; i < alreadyDiscovered_l.Count; i ++)
			setup_row_micro_discover_l (alreadyDiscovered_l [i], i + 1, true);
		for (int i = 0; i < notDiscovered_l.Count; i ++)
			setup_row_micro_discover_l (notDiscovered_l [i], i + 1 + alreadyDiscovered_l.Count, false);

		grid_micro_discover.ShowAll();

		//hide any buttons with "NC"or "----"
		foreach (Button b in button_microAlreadyDiscovered_l)
			if (b.Label == "NC" || b.Label == "----")
				b.Visible = false;
		foreach (Button b in button_microNotDiscovered_l)
			if (b.Label == "NC" || b.Label == "----")
				b.Visible = false;
		foreach (Gtk.Label l in label_microAlreadyDiscovered_l)
			if (l.Text == Catalog.GetString (useThisStr))
				l.Visible = false;
		foreach (Button b in button_debug_l)
			if (b.Label == "")
				b.Visible = false;
	}

	private void setup_row_micro_discover_l (ChronopicRegisterPort crp, int i, bool alreadyDiscovered)
	{
		string portNameShort = crp.Port;
		if (portNameShort.StartsWith ("/dev/"))
			portNameShort = portNameShort.Replace ("/dev/", "");

		Gtk.Label l = new Gtk.Label (string.Format("{0}\n{1}",
					portNameShort, Util.RemoveCenterCharsOnLongString (crp.SerialNumber, 12)));
		grid_micro_discover.Attach (l, 0, i, 1, 1);

		if (alreadyDiscovered)
		{
			Gtk.Label l2 = new Gtk.Label (ChronopicRegisterPort.TypePrint (crp.Type));
			grid_micro_discover.Attach (l2, 1, i, 1, 1);
		} else {
			Gtk.ProgressBar pb = new Gtk.ProgressBar ();
			pb.Text = "----"; //to have height
			pb.SetSizeRequest (125, -1);
			progressbar_microNotDiscovered_l.Add (pb);
			grid_micro_discover.Attach (pb, 1, i, 1, 1);
		}

		Gtk.Label label = new Gtk.Label (); //used on NC and ----
		Gtk.Button b = new Gtk.Button (); //used on Select!
		Gtk.Box box_b_label = new Gtk.Box (Gtk.Orientation.Horizontal, 0);
		Gtk.Button bDebug = new Gtk.Button ();
		bDebug.Label = "";

		if (alreadyDiscovered)
		{
			if (discoverMatchCurrentMode (crp.Type))
			{
				b.Sensitive = true;
				label.Text = Catalog.GetString (useThisStr);

				//TODO: work for more sensors
				if (
						(Constants.ModeIsFORCESENSOR (current_mode) && crp.Type == ChronopicRegisterPort.Types.ARDUINO_FORCE) ||
						(Constants.ModeIsENCODER (current_mode) && crp.Type == ChronopicRegisterPort.Types.ENCODER) )
				{
					bDebug.Sensitive = true;
					bDebug.Label = "Test it!";
					bDebug.Clicked -= new EventHandler (on_discover_debug_this_clicked); //needed. if not: called multiple times
					bDebug.Clicked += new EventHandler (on_discover_debug_this_clicked);
				}
			} else
			{
				b.Sensitive = false;
				label.Text = Catalog.GetString ("NC");
				box_micro_discover_nc.Visible = true;
			}

			//label_microAlreadyDiscovered_l.Add (label);
			b.Label = label.Text;
			button_microAlreadyDiscovered_l.Add (b);
			label_microAlreadyDiscovered_l.Add (label); //just to make not visible later
			portAlreadyDiscovered_l.Add (crp);
			b.Clicked -= new EventHandler (on_discover_use_this_clicked); //needed. if not: called multiple times
			b.Clicked += new EventHandler (on_discover_use_this_clicked);

			button_debug_l.Add (bDebug);
		} else {
			b.Sensitive = false;

			//b.Label = "----";
			label_microNotDiscovered_l.Add (label);
			label.Text = "----";
			b.Label = label.Text;
			button_microNotDiscovered_l.Add (b);
			button_debug_l.Add (bDebug);
		}

		box_b_label.PackStart (label, false, false, 0);
		box_b_label.PackStart (b, false, false, 0);
		box_b_label.PackStart (bDebug, false, false, 6);

		/* done after grid_micro_discover.ShowAll ();
		if (label.Text == "NC" || label.Text == "----")
		{
			label.Visible = true;
			b.Visible = false;
		} else {
			label.Visible = false;
			b.Visible = true;
		}
		*/

		grid_micro_discover.Attach (box_b_label, 2, i, 1, 1);
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
		for (int i = 0; i < progressbar_microNotDiscovered_l.Count; i ++)
		{
			//progressbars
			Gtk.ProgressBar pb = progressbar_microNotDiscovered_l[i];
			if (microDiscover.ProgressBar_l[i] == MicroDiscover.Status.NotStarted)
			{
				pb.Text = "----"; //to have height
				pb.Fraction = 0;
			} else if (microDiscover.ProgressBar_l[i] == MicroDiscover.Status.Done)
			{
				pb.Text = microDiscover.ProgressBar_l[i].ToString();
				pb.Fraction = 1;
			} else {
				if (microDiscover.Cancel)
					pb.Text = Catalog.GetString("Cancelling");
				else
					pb.Text = microDiscover.ProgressBar_l[i].ToString();
				pb.Pulse ();
			}

			if (i < microDiscover.Discovered_l.Count)
			{
				if (discoverMatchCurrentMode (microDiscover.Discovered_l[i]))
				{
					(progressbar_microNotDiscovered_l[i]).Text = ChronopicRegisterPort.TypePrint(microDiscover.Discovered_l[i]);
					button_microNotDiscovered_l[i].Sensitive = true;
					button_microNotDiscovered_l[i].Label = Catalog.GetString (useThisStr);
					button_microNotDiscovered_l[i].Clicked -= new EventHandler(on_discover_use_this_clicked); //needed. if not: called multiple times
					button_microNotDiscovered_l[i].Clicked += new EventHandler(on_discover_use_this_clicked);
					button_microNotDiscovered_l[i].Visible = true;
					label_microNotDiscovered_l[i].Visible = false;
				} else {
					//button_microNotDiscovered_l[i].Label = Catalog.GetString ("NC");
					button_microNotDiscovered_l[i].Visible = false;
					label_microNotDiscovered_l[i].Text = Catalog.GetString ("NC");
					label_microNotDiscovered_l[i].Visible = true;

					box_micro_discover_nc.Visible = true;
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

			for (int i = 0; i < progressbar_microNotDiscovered_l.Count; i ++)
			{
				if (microDiscover.Cancel &&
						 microDiscover.ProgressBar_l[i] != MicroDiscover.Status.Done)
					(progressbar_microNotDiscovered_l[i]).Text = Catalog.GetString("Cancelled");

				(progressbar_microNotDiscovered_l[i]).Fraction = 1;

				if ( ! (i < microDiscover.Discovered_l.Count &&
							discoverMatchCurrentMode (microDiscover.Discovered_l[i])) )
					(progressbar_microNotDiscovered_l[i]).Text = Catalog.GetString ("No");
			}

			image_button_micro_discover_cancel_close.Pixbuf =
				Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "image_close.png");
			label_button_micro_discover_cancel_close.Text = Catalog.GetString("Close");

			if (discoverCloseAfterCancel)
			{
				//on_button_micro_discover_cancel_close_clicked (new object (), new EventArgs ());
				CancelCloseFromUser ();
			}

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

	private void on_discover_debug_this_clicked (object o, EventArgs args)
	{
		/*
		 * TODO:
		 * - unsensitivize SELECT and CLOSE
		 * - show feedback graphically
		 * - work on debug mode
		 * - hability to send a log
		 */
		Button bPress = (Button) o;
		bool success = false;

		// 1) test the discovered by MicroDiscover
		//loop the list to know which button was
		for (int i = 0 ; i < button_debug_l.Count; i ++)
		{
			if (button_debug_l[i] != bPress)
				continue;

			LogB.Information ("Device to debug is: " + portAlreadyDiscovered_l[i].ToString ());
			//TODO: work for more sensors
			ChronopicRegisterPort crp = portAlreadyDiscovered_l[i];
			if (crp.Type == ChronopicRegisterPort.Types.ARDUINO_FORCE ||
					crp.Type == ChronopicRegisterPort.Types.ENCODER)
			{
				DebugDevices dd;
				if (crp.Type == ChronopicRegisterPort.Types.ARDUINO_FORCE)
					dd = new DebugForceSensor (crp);
				else //if (crp.Type == ChronopicRegisterPort.Types.ENCODER)
					dd = new DebugEncoder (crp);

				new DialogMessage (dd.Title, Constants.MessageTypes.INFO, 450, 400, dd.Str);
			}
		}
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

public abstract class DebugDevices
{
	protected ChronopicRegisterPort crp;
	protected string title;
	protected string str;
	protected SerialPort port;

	protected bool portCreate ()
	{
		str += "\n\n- Creating port …";

		try {
			port = new SerialPort (crp.Port, 115200);
		}
		catch (System.IO.IOException)
		{
			str += "\n- Problems creating port";
			return false;
		}

		str += "\n- Successfully created port";
		return true;
	}

	protected bool portOpen ()
	{
		str += "\n\n- Opening port …";
		try {
			port.Open();
		}
		catch (System.IO.IOException)
		{
			//forceSensorOtherMessage = forceSensorNotConnectedString;
			str += "\n- Problems opening port";
			return false;
		}
		str += "\n- Successfully opened port";
		return true;
	}

	/*
		Thread.Sleep (3000); //sleep to let arduino start reading serial event

		LogB.Information ("Have wait 3 s");
                //double firmwareVersion = forceSensorCheckVersionDo(); //TODO: it uses portFS
	*/

	protected virtual bool readSomeData ()
	{
		return true;
	}

	protected bool portClose ()
	{
		str += "\n\n- Closing port …";
		try {
			port.Close();
		} 
		catch (System.IO.IOException)
		{
			str += "\n- Problems closing port";
			return false;
		}
		str += "\n- Closed! All ok.";

		return true;
	}

	public string Title {
		get { return title; }
	}
	public string Str {
		get { return str; }
	}
}

public class DebugForceSensor : DebugDevices
{
	public DebugForceSensor (ChronopicRegisterPort crp)
	{
		this.crp = crp;
		title = "Testing Force Sensor";

		if (! portCreate ())
			return;

		if (! portOpen ())
			return;

		Thread.Sleep(3000); //sleep to let arduino start reading serial event
		LogB.Information ("Have wait 3 s");

		if (! getVersion ())
			return;

		if (! readSomeData ())
			return;

		portClose ();
	}

	// adapted from gui/app1/forceSensor.cs forceSensorCheckVersionDo ()
	private bool getVersion ()
	{
		str += "\n\n- Getting version …";

		// send message
		try {
			port.WriteLine ("get_version:");
		}
		catch (Exception ex)
		{
			if(ex is System.IO.IOException || ex is System.TimeoutException)
			{
				str += "\n- Failed at sending message. Error: " + ex.ToString ();
				return false;
			}
		}

		// get version
		string s = "";
		do {
			Thread.Sleep(100); //sleep to let arduino start reading
			try {
				s = port.ReadLine().Trim();
			} catch (Exception ex) {
				str += "\n- Failed at receiving message. Error: " + ex.ToString ();
				return false;
			}
		}
		while(! s.Contains("Force_Sensor-"));

		str += "\n- Version found is: " + s;
		return true;
	}

	// copied from gui/app1/forceSensor.cs
	protected override bool readSomeData ()
	{
		int samples = 10;
		str += string.Format ("\n\n- Capturing {0} samples …", samples);

		// send message
		try {
			port.WriteLine ("start_capture:");
		}
		catch (Exception ex)
		{
			if(ex is System.IO.IOException || ex is System.TimeoutException)
			{
				str += "\n- Failed at sending message. Error: " + ex.ToString ();
				return false;
			}
		}

		// receive confirmation
		string s = "";
		do {
			Thread.Sleep(100); //sleep to let arduino start reading
			try {
				s = port.ReadLine().Trim();
			} catch (Exception ex) {
				str += "\n- Failed at receiving message. Error: " + ex.ToString ();
				return false;
			}
		}
		while(! s.Contains("Starting capture"));

		// capture some data
		s = "";
		int count = 0;
		do {
			int time = 0;
			double force = 0;
			string triggerCode = "";
			s = port.ReadLine();
			if(! forceSensorProcessCapturedLine(s, out time, out force,
						false, out triggerCode)) //false: do not read triggers
				continue;

			count ++;
			//str += string.Format ("\n{0,12} us, {1,12} N", time, force);
			str += string.Format ("\n{0} us\t {1} N", time, force);
		} while (count < samples);

		// ending capture. Send message
		try {
			port.WriteLine ("end_capture:");
		}
		catch (Exception ex)
		{
			if(ex is System.IO.IOException || ex is System.TimeoutException)
			{
				str += "\n- Failed at sending message. Error: " + ex.ToString ();
				return false;
			}
		}

		// ending capture. Receive message
		int notValidCommandCount = 0;
		do {
			Thread.Sleep(10);
			try {
				s = port.ReadLine();
			} catch (Exception ex) {
				str += "\n- Failed at receiving message. Error: " + ex.ToString ();
			}

			//2023 Aug 3: sometimes Arduino looses some chars. It seems only happens with this command because Arduino will be busy capturing
			//instead of "end_capture:" arrived "end_cture:" (found 2 times) "end_capte:", "end_ture:", "end_caure:"
			if (s.Contains ("Not a valid command"))
			{
				notValidCommandCount ++;

				if (notValidCommandCount > 10)
				{
					str += "\n- NotValidCommandCount > 10";
					return false;
				}

				try {
					port.WriteLine ("end_capture:");
				} catch (Exception ex) {
					str += "\n- Failed at sending message. Error: " + ex.ToString ();
					return false;
				}
			}
		}
		while(! s.Contains("Capture ended"));

		return true;
	}

	// copied from gui/app1/forceSensor.cs
	private bool forceSensorProcessCapturedLine (string str,
			out int time, out double force,
			bool readTriggers, out string triggerCode)
	{
		time = 0;
		force = 0;
		triggerCode = "";

		//check if there is one and only one ';'
		if( ! (str.Contains(";") && str.IndexOf(";") == str.LastIndexOf(";")) )
			return false;

		string [] strFull = str.Split(new char[] {';'});

		if (! Util.IsNumber (Util.ChangeDecimalSeparator (strFull[0]), true))
			return false;

		if (Util.IsNumber (Util.ChangeDecimalSeparator (strFull[1]), true))
		{
		}
		else if (readTriggers)
		{
			time = Convert.ToInt32 (strFull[0]);
			triggerCode = strFull[1].Trim(); //now is coming from Arduino with an enter
			return true;
		} else
			return false;

		time = Convert.ToInt32 (strFull[0]);

		//bad tare or bad calibration or too much force
		if (Math.Abs (Convert.ToDouble(Util.ChangeDecimalSeparator(strFull[1]))) > 20000) // 20000 N (2000 Kg) Chronojump force sensors are up to 5000 but we have special version with 20000
		{
			str += string.Format ("\n- Error. Force too big: " + Util.ChangeDecimalSeparator  (strFull[1]));
			return false;
		}

		force = Convert.ToDouble (Util.ChangeDecimalSeparator (strFull[1]));

		return true;
	}

}

public class DebugEncoder : DebugDevices
{
	public DebugEncoder (ChronopicRegisterPort crp)
	{
		this.crp = crp;
		title = "Testing Encoder";

		if (! portCreate ())
			return;

		if (! portOpen ())
			return;

		if (! readSomeData ())
			return;

		portClose ();
	}

	protected override bool readSomeData ()
	{
		int samples = 50;
		str += string.Format ("\n\n- Capturing {0} samples …", samples);

		var buffer = new byte[1024];
		int countPrinted = 0;
		do {
			try {
				int bytesRead = port.Read (buffer, 0, buffer.Length);
				if (bytesRead == 0)
					continue;

				for (int j = 0; j < bytesRead && countPrinted < samples; j ++)
				{
					if (countPrinted % 10 == 0)
						str += "\n";

					str += string.Format ("{0,3} ", convertByte (Convert.ToInt32  (buffer[j])));
					countPrinted ++;
				}
			}
			catch (Exception ex)
			{
				if(ex is System.IO.IOException || ex is System.TimeoutException)
				{
					str += "\n- Failed at sending message. Error: " + ex.ToString ();
					return false;
				}
			}
		} while (countPrinted < samples);

		return true;
	}

	private int convertByte (int b)
	{
		if(b > 128)
			b = b - 256;

		return b;
	}

}
