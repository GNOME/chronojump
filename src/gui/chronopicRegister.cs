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
 *  Copyright (C) 2016-2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List<T>
using Gdk;
using Gtk;
using Mono.Unix;
using System.Threading;


public class TypePix
{
	public ChronopicRegisterPort.Types Type;
	public Pixbuf Pix;

	public TypePix(ChronopicRegisterPort.Types type, Pixbuf pix)
	{
		Type = type;
		Pix = pix;
	}
}

public static class TypePixList
{
	public static List<TypePix> l;

	//one for each type
	static TypePixList()
	{
		l = new List<TypePix>();

		l.Add(new TypePix(ChronopicRegisterPort.Types.UNKNOWN, new Pixbuf (null, Util.GetImagePath(false) + "board-unknown.png")));
		l.Add(new TypePix(ChronopicRegisterPort.Types.CONTACTS, new Pixbuf (null, Util.GetImagePath(false) + "board-jump-run.png")));
		l.Add(new TypePix(ChronopicRegisterPort.Types.ARDUINO_RUN_ENCODER, new Pixbuf (null, Util.GetImagePath(false) + "board-arduino-run-encoder.png")));
		l.Add(new TypePix(ChronopicRegisterPort.Types.ARDUINO_FORCE, new Pixbuf (null, Util.GetImagePath(false) + "board-arduino-force.png")));
		l.Add(new TypePix(ChronopicRegisterPort.Types.ENCODER, new Pixbuf (null, Util.GetImagePath(false) + "board-encoder.png")));
		l.Add(new TypePix(ChronopicRegisterPort.Types.ARDUINO_RFID, new Pixbuf (null, Util.GetImagePath(false) + "board-arduino-rfid.png")));
		//l.Add(new TypePix(ChronopicRegisterPort.Types.ACCELEROMETER, new Pixbuf (null, Util.GetImagePath(false) + "board-accelerometer.png")));
		l.Add(new TypePix(ChronopicRegisterPort.Types.RUN_WIRELESS, new Pixbuf (null, Util.GetImagePath(false) + "board-run-wireless.png")));
	}

	//used to hide new unfinished devices on chronojump release
	public static void RemovePix(ChronopicRegisterPort.Types type)
	{
		int i = 0;
		int pos = -1;
		foreach(TypePix tp in l)
		{
			if(tp.Type == type)
			{
				pos = i;
				break;
			}

			i ++;
		}

		if(pos >= 0)
			l.RemoveAt(pos);
	}

	public static Pixbuf GetPix(ChronopicRegisterPort.Types type)
	{
		foreach(TypePix tp in l)
		{
			if(tp.Type == type)
				return tp.Pix;
		}

		return l[0].Pix; //return first value if there are problems
	}

	public static TypePix GetPixPrevNext(ChronopicRegisterPort.Types type, string direction)
	{
		int currentPos = 0;
		int nextPos = 0;
		foreach(TypePix tp in l)
		{
			if(tp.Type == type)
			{
				if(direction == "LEFT")
				{
					nextPos = currentPos -1;
					if(nextPos < 0)
						nextPos = 0;
				} else
				{
					nextPos = currentPos +1;
					if(nextPos > l.Count -1)
						nextPos = l.Count -1;
				}

				return(l[nextPos]);
			}
			currentPos ++;
		}

		return(l[0]); //if there are problems, return UNKNOWN value
	}
}

public class DiscoverWindow
{
	//TODO instead of 4 lists, have List<microDiscoveGui>
	List<Gtk.ProgressBar> progressbar_microNotDiscovered_l;
	List<Gtk.Button> button_microNotDiscovered_l;
	List<Gtk.Label> label_microNotDiscovered_l; //to use labels for ---- and NC instead of buttons

	List<ChronopicRegisterPort> portAlreadyDiscovered_l;
	List<Gtk.Button> button_microAlreadyDiscovered_l;
	List<Gtk.Label> label_microAlreadyDiscovered_l; //to be able to Visible = false after ShowAll ()

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
	private string useThisStr = "Use this!";

	private ChronopicRegisterPort portSelected;

	public DiscoverWindow (Constants.Modes current_mode, ChronopicRegister chronopicRegister,
			Gtk.Label label_micro_discover_not_found,
			Gtk.Grid grid_micro_discover,
			Gtk.Box box_micro_discover_nc,
			Gtk.Image image_button_micro_discover_cancel_close,
			Gtk.Label label_button_micro_discover_cancel_close,
			string iconModeStr
			)
	{
		this.current_mode = current_mode;
		this.chronopicRegister = chronopicRegister;
		this.grid_micro_discover = grid_micro_discover;
		this.box_micro_discover_nc = box_micro_discover_nc;
		this.image_button_micro_discover_cancel_close = image_button_micro_discover_cancel_close;
		this.label_button_micro_discover_cancel_close = label_button_micro_discover_cancel_close;

		// 1) set up gui

		FakeButtonClose = new Gtk.Button();
		portSelected = new ChronopicRegisterPort ("");
		image_discover_mode = new Gtk.Image (new Pixbuf (null, Util.GetImagePath(false) + iconModeStr));

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
						crp.SerialNumber != ChronopicRegister.SerialNumberNotUnique)
					alreadyDiscovered_l.Add (crp);
				else
					notDiscovered_l.Add (crp);
			}

		image_button_micro_discover_cancel_close.Pixbuf =
				new Pixbuf (null, Util.GetImagePath(false) + "image_cancel.png");
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
				new Pixbuf (null, Util.GetImagePath (false) + "image_close.png");
			label_button_micro_discover_cancel_close.Text = Catalog.GetString("Close");
		}

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

		// 3) create widgets, lists, attach to table and show all

		// 3a) create table header row
		Gtk.Label l0 = new Gtk.Label ("<b>" + Catalog.GetString ("Device") + "</b>");
		l0.UseMarkup = true;

		Gtk.Label l1 = new Gtk.Label ("<b>" + Catalog.GetString ("Compatibility with") + "</b>");
		l1.UseMarkup = true;
		Gtk.HBox hbox_l1 = new Gtk.HBox (false, 10);
		hbox_l1.PackStart (l1, false, false, 0);
		hbox_l1.PackStart (image_discover_mode, false, false, 0);
		//hbox_l1.Hexpand = true; //this does not work, so create a parent and expand:
		Gtk.HBox hbox_l1_parent = new Gtk.HBox (false, 0);
		hbox_l1_parent.PackStart (hbox_l1, true, false, 0);

		grid_micro_discover.Attach (l0, 0, 0, 1, 1);
		grid_micro_discover.Attach (hbox_l1_parent, 1, 0, 2, 1);

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
		Gtk.Button b = new Gtk.Button (); //used on Use this!
		Gtk.Box box_b_label = new Gtk.Box (Gtk.Orientation.Horizontal, 0);

		if (alreadyDiscovered)
		{
			if (discoverMatchCurrentMode (crp.Type))
			{
				b.Sensitive = true;
				label.Text = Catalog.GetString (useThisStr);
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
		} else {
			b.Sensitive = false;

			//b.Label = "----";
			label_microNotDiscovered_l.Add (label);
			label.Text = "----";
			b.Label = label.Text;
			button_microNotDiscovered_l.Add (b);
		}

		box_b_label.PackStart (label, false, false, 0);
		box_b_label.PackStart (b, false, false, 0);

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
				new Pixbuf (null, Util.GetImagePath(false) + "image_close.png");
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
		else if (
				(current_mode == Constants.Modes.RUNSSIMPLE || current_mode == Constants.Modes.RUNSINTERVALLIC) &&
				(crpt == ChronopicRegisterPort.Types.CONTACTS || crpt == ChronopicRegisterPort.Types.RUN_WIRELESS) )
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

	//the port that user clicked on "Use this!"
	public ChronopicRegisterPort PortSelected {
		get { return portSelected; }
	}
}

public class ChronopicRegisterWindow
{
	Gtk.Window chronopic_register_win;
	Gtk.VBox vbox_top;
	Gtk.VBox vbox_main;
	private List<ChronopicRegisterPort> listConnected;
	public Gtk.Button FakeButtonCloseSerialPort;

	public ChronopicRegisterWindow(Gtk.Window app1, List<ChronopicRegisterPort> listAll, bool compujump, bool showRunWireless)
	{
		// 1) remove undesired Pixs from list
		if(! compujump)
			TypePixList.RemovePix(ChronopicRegisterPort.Types.ARDUINO_RFID);

		if(! showRunWireless)
			TypePixList.RemovePix(ChronopicRegisterPort.Types.RUN_WIRELESS);

		// 2) create window

		createWindow(app1);
		UtilGtk.IconWindow(chronopic_register_win); //put an icon to window

		//manage window color
		if(! Config.UseSystemColor)
			UtilGtk.WindowColor(chronopic_register_win, Config.ColorBackground);

		listConnected = new List<ChronopicRegisterPort>();

		// 3)create listConnected with connected chronopics

		int connectedCount = 0;
		int unknownCount = 0;
		foreach(ChronopicRegisterPort crp in listAll)
		{
			if(crp.Port != "")
			{
				listConnected.Add(crp);
				connectedCount ++;

				if(crp.Type == ChronopicRegisterPort.Types.UNKNOWN)
					unknownCount ++;
			}
		}

		createVBoxsViewportAndFrame();
		createContent(connectedCount, unknownCount);
		createButtons();
	}

	public void Show()
	{
		chronopic_register_win.ShowAll();
		list_labels_selected_show(); //hide some label_selected if they are UNKNOWN
	}

	private void createWindow(Gtk.Window app1)
	{
		chronopic_register_win = new Gtk.Window (Catalog.GetString("Chronojump devices"));
		//chronopic_register_win.AllowGrow = false;
		chronopic_register_win.Modal = true;
		chronopic_register_win.TransientFor = app1;
		chronopic_register_win.WindowPosition = Gtk.WindowPosition.CenterOnParent;
		chronopic_register_win.TypeHint = WindowTypeHint.Dialog;

		chronopic_register_win.DeleteEvent += on_delete_event;

		/* Sets the border width of the window. */
		chronopic_register_win.BorderWidth= 20;
	}

	private void createVBoxsViewportAndFrame()
	{
		vbox_main = new Gtk.VBox(false, 12);
		Gtk.Viewport viewport = new Gtk.Viewport(null, null);
		Gtk.Frame frame = new Gtk.Frame();
		frame.LabelXalign = 0;
		frame.LabelYalign = 1;
		frame.ShadowType = ShadowType.In;

		vbox_main.BorderWidth = 8;
		viewport.Add(vbox_main);

		viewport.BorderWidth = 4;
		frame.Add(viewport);

		vbox_top = new Gtk.VBox(false, 20);
		vbox_top.Add(frame);

		chronopic_register_win.Add(vbox_top);
	}

	Gtk.Grid grid_main;

	private List<Gtk.Image> list_images;
	private List<Gtk.Label> list_labels_selected;
	private List<Gtk.Label> list_labels_type;
	private List<Gtk.Button> list_buttons_left;
	private List<Gtk.Button> list_buttons_right;

	private void createGrid ()
	{
		int rows = listConnected.Count;

		Gtk.Label label_device_title = new Gtk.Label("<b>" + Catalog.GetString("Device") + "</b>");
		Gtk.Label label_type_title = new Gtk.Label("<b>" + Catalog.GetString("Type") + "</b>");

		label_device_title.UseMarkup = true;
		label_type_title.UseMarkup = true;

		label_device_title.Show();
		label_type_title.Show();

		grid_main = new Gtk.Grid();
		grid_main.ColumnSpacing = 20;
		grid_main.RowSpacing = 12;

		grid_main.Attach (label_device_title, 1, 0, 1, 1);
		grid_main.Attach (label_type_title, 2, 0, 1, 1);

		list_buttons_left = new List<Gtk.Button>();
		list_images = new List<Gtk.Image>();
		list_labels_selected = new List<Gtk.Label>();
		list_labels_type = new List<Gtk.Label>();
		list_buttons_right = new List<Gtk.Button>();

		for (int count=1; count <= rows; count ++)
		{
			string deviceStr = listConnected[count -1].SerialNumber + "\n\n" + listConnected[count -1].Port;
			Gtk.Label label_device = new Gtk.Label(deviceStr);
			grid_main.Attach (label_device, 1, count, 1, 1);
			label_device.Show();

			Gtk.HBox hbox_type = new Gtk.HBox(false, 6);
			Button button_left = UtilGtk.CreateArrowButton(ArrowType.Left, ShadowType.In, 50, -1, UtilGtk.ArrowEnum.BACKWARD);
			button_left.Sensitive = (listConnected[count-1].Type != TypePixList.l[0].Type);
			button_left.CanFocus = false;
			button_left.IsFocus = false;
			button_left.Clicked += on_button_left_clicked;
			//hbox_type.Add(button_left);
			hbox_type.PackStart(button_left, true, false, 1);

			//create image
			Pixbuf pixbuf = TypePixList.GetPix(listConnected[count-1].Type);
			Gtk.Image image = new Gtk.Image();
			image.Pixbuf = pixbuf;
			hbox_type.Add(image);
			hbox_type.PackStart(image, false, false, 1);

			UtilGtk.ArrowEnum arrowEnum = UtilGtk.ArrowEnum.FORWARD;
			if(ChronopicRegisterPort.TypePrint(listConnected[count-1].Type) == ChronopicRegisterPort.TypePrint(ChronopicRegisterPort.Types.UNKNOWN))
				arrowEnum = UtilGtk.ArrowEnum.FORWARD_EMPHASIS;

			Button button_right = UtilGtk.CreateArrowButton(ArrowType.Right, ShadowType.In, 50, -1, arrowEnum);

			button_right.CanFocus = false;
			button_right.IsFocus = false;
			button_right.Clicked += on_button_right_clicked;
			button_right.Sensitive = (listConnected[count-1].Type != TypePixList.l[TypePixList.l.Count -1].Type);
			hbox_type.PackStart(button_right, true, false, 1);

			Gtk.VBox vbox = new Gtk.VBox(false, 2);
			vbox.Add(hbox_type);

			Gtk.HBox hbox_label_to_align = new Gtk.HBox(false, 0);
			Gtk.HBox hbox_label = new Gtk.HBox(false, 6);
			Gtk.Label label_selected = new Gtk.Label("<b>" + Catalog.GetString("Selected:") + "</b> ");
			label_selected.UseMarkup = true;
			label_selected.Visible = false; //but this will be visible by the chronopic_register_win.ShowAll();
			hbox_label.PackStart(label_selected, false, false, 1);
			Gtk.Label label_type = new Gtk.Label(ChronopicRegisterPort.TypePrint(listConnected[count-1].Type));
			hbox_label.PackStart(label_type, false, false, 1);
			hbox_label_to_align.PackStart(hbox_label, true, false, 1);

			//vbox.Add(label_type);
			vbox.Add(hbox_label_to_align);

			grid_main.Attach (vbox, 2, count, 1, 1);

			list_buttons_left.Add(button_left);
			list_images.Add(image);
			list_labels_selected.Add(label_selected);
			list_labels_type.Add(label_type);
			list_buttons_right.Add(button_right);
		}
		grid_main.Show();
	}

	private void list_labels_selected_show()
	{
		int rows = listConnected.Count;
		for (int count=1; count <= rows; count ++)
		{
			list_labels_selected[count-1].Visible = (ChronopicRegisterPort.TypePrint(listConnected[count-1].Type) != ChronopicRegisterPort.TypePrint(ChronopicRegisterPort.Types.UNKNOWN));
		}
	}

	private void createContent(int connectedCount, int unknownCount)
	{
		//create top hbox
		Gtk.HBox hbox = new Gtk.HBox(false, 12);

		Pixbuf pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_chronopic_connect_big.png");
		//hbox image
		Gtk.Image image = new Gtk.Image();
		image.Pixbuf = pixbuf;
		hbox.Add(image);

		//hbox label
		Gtk.Label label = new Gtk.Label();
		label.Text = writeLabel(connectedCount, unknownCount);
		hbox.Add(label);
		vbox_main.Add(hbox);

		//table
		if(connectedCount > 0)
		{
			createGrid();
			Gtk.VBox vboxTV = new Gtk.VBox(false, 10);
			vboxTV.Add(grid_main);
			vbox_main.Add(vboxTV);
		}
	}

	private string writeLabel(int connectedCount, int unknownCount)
	{
		if(connectedCount > 0)
		{
			string str = string.Format(Catalog.GetPluralString(
						"Found 1 device.",
						"Found {0} devices.",
						connectedCount),
					connectedCount);

			if(unknownCount > 0)
			{
				str += "\n\n";
				str += string.Format(Catalog.GetPluralString(
							"One device is not configured. Please, configure it clicking the arrows.",
							"{0} devices are not configured. Please, configure them clicking the arrows.",
							unknownCount),
						unknownCount) + "\n";
			}
			return str;
		}

		return Catalog.GetString("Device not found") + "\n\n" + Catalog.GetString("Connect and reopen device window.");
	}

	private void createButtons()
	{
		/*
		 * not needed on new versions of OSX
		 * problem seems to be fixed
		 *
		Frame frame = new Frame();
		frame.ShadowType = ShadowType.In;
		frame.LabelXalign = 0f;
		frame.LabelYalign = 0.5f;

		frame.Label = Catalog.GetString("There is a known problem on macOS:");
		Gtk.Label label_macOS = new Gtk.Label(
				Catalog.GetString("If Chronopic is disconnected after jumps or runs execution,\nthat port will be blocked until restart of machine."));

		Gtk.VBox vbox_m = new Gtk.VBox();
		vbox_m.PackStart(label_macOS, false, false, 8);

		Gtk.HBox hbox_m = new Gtk.HBox();
		hbox_m.PackStart(vbox_m, false, false, 8);

		if( UtilAll.GetOSEnum() == UtilAll.OperatingSystems.MACOSX)
		{
			frame.Add(hbox_m);
			vbox_main.Add(frame);
		}
		*/

		FakeButtonCloseSerialPort = new Gtk.Button();
		Gtk.Button button_close_serial_port = new Gtk.Button("Close serial port (debug)");
		button_close_serial_port.Clicked += new EventHandler(on_button_close_serial_port_clicked);

		//---- button close start --->
		Gtk.Button button_close = new Gtk.Button(Catalog.GetString("Close"));
		button_close.CanFocus = true;
		button_close.IsFocus = true;
		button_close.Clicked += new EventHandler(on_button_close_clicked);

		Gtk.AccelGroup ag = new Gtk.AccelGroup (); //button can be called clicking Escape key
		chronopic_register_win.AddAccelGroup (ag);

		button_close.AddAccelerator
			("activate", ag, new Gtk.AccelKey
			 (Gdk.Key.Escape, Gdk.ModifierType.None,
			  Gtk.AccelFlags.Visible));
		//<---- button close end

		//add buttons to containers
		Gtk.HButtonBox hbox = new Gtk.HButtonBox ();
		//hbox.Add(button_close_serial_port);

		hbox.Add(button_close);

		vbox_top.Add(hbox);
	}

	private void on_button_left_clicked(object o, EventArgs args)
	{
		Button buttonClicked = o as Button;
		if (o == null)
			return;

		int count = 0;
		foreach(Gtk.Button button in list_buttons_left)
		{
			if(button == buttonClicked)
			{
				TypePix tp = TypePixList.GetPixPrevNext(listConnected[count].Type, "LEFT");
				listConnected[count].Type = tp.Type;
				list_images[count].Pixbuf = tp.Pix;
				list_labels_selected[count].Visible = (ChronopicRegisterPort.TypePrint(listConnected[count].Type) != ChronopicRegisterPort.TypePrint(ChronopicRegisterPort.Types.UNKNOWN));
				list_labels_type[count].Text = ChronopicRegisterPort.TypePrint(listConnected[count].Type);

				buttons_sensitivity(button, list_buttons_right[count], tp.Type);
				updateSQL(listConnected[count].SerialNumber, tp.Type);
			}
			count ++;
		}
	}
	private void on_button_right_clicked(object o, EventArgs args)
	{
		Button buttonClicked = o as Button;
		if (o == null)
			return;

		int count = 0;
		foreach(Gtk.Button button in list_buttons_right)
		{
			if(button == buttonClicked)
			{
				TypePix tp = TypePixList.GetPixPrevNext(listConnected[count].Type, "RIGHT");
				listConnected[count].Type = tp.Type;
				list_images[count].Pixbuf = tp.Pix;
				list_labels_selected[count].Visible = (ChronopicRegisterPort.TypePrint(listConnected[count].Type) != ChronopicRegisterPort.TypePrint(ChronopicRegisterPort.Types.UNKNOWN));
				list_labels_type[count].Text = ChronopicRegisterPort.TypePrint(listConnected[count].Type);

				buttons_sensitivity(list_buttons_left[count], button, tp.Type);
				updateSQL(listConnected[count].SerialNumber, tp.Type);
			}
			count ++;
		}
	}

	private void buttons_sensitivity(Gtk.Button left, Gtk.Button right, ChronopicRegisterPort.Types type)
	{
		left.Sensitive = (type != TypePixList.l[0].Type);
		right.Sensitive = (type != TypePixList.l[TypePixList.l.Count -1].Type);
		//LogB.Information("count + tplcount " + count + "," + TypePixList.l.Count);

		Pixbuf pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameArrowForward);
		//show red image on button right if UNKNOWN
		if(type == TypePixList.l[0].Type)
			pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameArrowForwardEmphasis);

		Gtk.Image image = (Gtk.Image) right.Child;
		image.Pixbuf = pixbuf;
	}

	private void updateSQL(string serialNumber, ChronopicRegisterPort.Types type)
	{
		//store on SQL
		SqliteChronopicRegister.Update(false, new ChronopicRegisterPort(serialNumber, type), type);
	}
	
	private void on_button_close_serial_port_clicked(object o, EventArgs args)
	{
		//try first to see if a sp is opened on a cp but that /ttyusbserial does not exists
		FakeButtonCloseSerialPort.Click();
	}

	private void on_button_close_clicked(object o, EventArgs args)
	{
		chronopic_register_win.Hide();
		chronopic_register_win = null;
	}

	private void on_delete_event (object o, DeleteEventArgs args)
	{
		LogB.Information("calling on_delete_event on ChronopicRegisterWindow");

		args.RetVal = true;

		on_button_close_clicked(new object(), new EventArgs());
	}
}

