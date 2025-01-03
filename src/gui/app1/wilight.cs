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
 * Copyright (C) 2024-2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Diagnostics; //Stopwatch
using Gtk;

//TODO: note this dirty code is just for testing
public partial class ChronoJumpWindow 
{
	// at glade ---->
	Gtk.Box box_start_wilight;
	Gtk.ButtonBox buttonbox_wilight_test;
	Gtk.CheckButton check_wilight_very_verbose;
	Gtk.Label label_wilight_test_status;
	Gtk.TextView textview_wilight;
	// <---- at glade

	static Thread threadWilight;
	enum wilightActions { DISCOVER, SPEED, SEQUENCE };
	private wilightActions wilightAction;

	static TextBuffer tbWilight = new TextBuffer (new TextTagTable());

	private void wilightApp1Init ()
	{
		tbWilight.Text = "";
	}

	private void on_button_wilight_test_discover_clicked (object o, EventArgs args)
	{
		wilightAction = wilightActions.DISCOVER;
		wilightExecute ();
	}

	private void on_button_wilight_test_speed_clicked (object o, EventArgs args)
	{
		wilightAction = wilightActions.SPEED;
		wilightExecute ();
	}

	private void on_button_wilight_test_sequence_clicked (object o, EventArgs args)
	{
		wilightAction = wilightActions.SEQUENCE;
		wilightExecute ();
	}

	private void wilightExecute ()
	{
		buttonbox_wilight_test.Sensitive = false;
		label_wilight_test_status.Text = "Doing";
		tbWilight.Text = "";

		threadWilight = new Thread (new ThreadStart (wilightTest));
		GLib.Idle.Add (new GLib.IdleHandler (pulseWilight));

		LogB.ThreadStart();
		threadWilight.Start();
	}

	private bool wilightManageConnect (string portName)
	{
		if(wichroCapture != null && wichroCapture.PortOpened)
			wichroCapture.Disconnect();

		if(wichroCapture == null || wichroCapture.PortName != portName)
		{
			wichroCapture = new WichroCapture (portName);
		}

		wichroCapture.Reset ();

		if (! wichroCapture.CaptureStart ())
		{
			//chronopicDisconnected = true;
			wichroCapture.Disconnect ();
			//cancel = true; //problem reading line (capturing)
			Util.PlaySound (Constants.SoundTypes.BAD, preferences.volumeOn, preferences.gstreamer);
			LogB.Information ("cannot connect");
			return false;
		}

		System.Threading.Thread.Sleep (3000); //to be able to read the answer from get_version (coming on CaptureStart)
		wichroCapture.Flush (); //to be able to read later

		return true;
	}

	private void wilightTest ()
	{
		string portName = configChronojump.WilightPortURL;
		string commandsFile = configChronojump.WilightCommandsURL;
		int commandTimeMs = configChronojump.WilightCommandMs;

		/*
		//testing stuff
		LogB.Information ("wilightTest");
		WilightTest wtz = new WilightTest (commandsFile);
		bool finishedz = false;
		while (true)
		{
			if (finishedz) //finished here to have also time to answer to the last command
				return;
			LogB.Information (wtz.GetNext (out finishedz));
		}
		*/

		if (! wilightManageConnect (portName))
			return;

		if (wilightAction == wilightActions.DISCOVER)
		{
			discover ();
			wichroCapture.Stop(); //Should we do a disconnect here?
			return;
		}

		sendCommandAndTextview (WilightColors.AllOffCommand);
		System.Threading.Thread.Sleep (1000);

		if (wilightAction == wilightActions.SPEED)
		{
			testSpeed ();
			wichroCapture.Stop(); //Should we do a disconnect here?
		}
		else if (wilightAction == wilightActions.SEQUENCE)
		{
			testSequence (commandsFile);
			wichroCapture.Stop(); //Should we do a disconnect here?
		}

		System.Threading.Thread.Sleep (1000);
		sendCommandAndTextview (WilightColors.AllOffCommand);
	}

	private void discover ()
	{
		tbWilight.Text += "\n> local:discover;";
		System.Threading.Thread.Sleep (50);
		bool commandSendOk = wichroCapture.Discover ();

		if (! commandSendOk)
			LogB.Information ("Error on call to discover");
		else {
			LogB.Information ("discover called ok");
			tbWilight.Text += "\n< " + wichroCapture.discoverResponse;
		}
	}

	//TODO: send only to discovered terminals
	private void testSpeed ()
	{
		List<string> colorsAll_l = new List<string> ();
		colorsAll_l.Add (WilightColors.AllRedCommand);
		colorsAll_l.Add (WilightColors.AllGreenCommand);
		colorsAll_l.Add (WilightColors.AllBlueCommand);
		int sleepTime = 500;
		int sleepTimeMin = 100; //does not work very good at 50
		int countDownAtMaxSpeed = 10; //to execute 10 times at max speed
		bool done = false;

		while (! done)
		{
			foreach (string colorAllStr in colorsAll_l)
			{
				sendCommandAndTextview (colorAllStr);
				System.Threading.Thread.Sleep (sleepTime);
			}
			sleepTime -= 50;
			if (sleepTime < sleepTimeMin)
			{
				sleepTime = sleepTimeMin;
				countDownAtMaxSpeed --;
			}
			//LogB.Information ("sleepTime: " + sleepTime.ToString ());
			if (countDownAtMaxSpeed <= 0)
				done = true;
		}
	}

	//TODO: send only to discovered terminals
	private void testSequence (string commandsFile)
	{
		WilightTest wt = new WilightTest (commandsFile);
		List<int> expectedTerminals_l = new List<int> (); //expected response on this (or them)
		bool finished = false;

		while (true)
		{
			if (finished) //finished here to have also time to answer to the last command
				break;

			string command = wt.GetNext (out finished);
			if (command == "")
				continue;

			sendCommandAndTextview (command);
			expectedTerminals_l = wt.GetExpectedTerminals (command);

			bool readedOn = false;
			while (! readedOn)
			{
				if(! wichroCapture.CaptureSample())
				{
					LogB.Information ("Problem capturing sample");
					Util.PlaySound (Constants.SoundTypes.BAD, preferences.volumeOn, preferences.gstreamer);
					break;
				}

				if(wichroCapture.CanReadFromList ())
				{
					LogB.Information ("Can read");
					WichroEvent we = wichroCapture.WichroCaptureReadNext();
					if (we.status == Chronopic.Plataforma.ON &&
							UtilList.FoundInListInt (expectedTerminals_l, we.photocell))
					{
						tbWilight.Text += "\n< " + we.ToString ();

						//LogB.Information ("Is ON!");
						Util.PlaySound (Constants.SoundTypes.GOOD, preferences.volumeOn, preferences.gstreamer);
						readedOn = true;
					}
					else if (check_wilight_very_verbose.Active)
						tbWilight.Text += "\n< " + we.ToString ();
				}
				System.Threading.Thread.Sleep (20);
			}
		}
	}

	private bool pulseWilight ()
	{
		textview_wilight.Buffer = tbWilight;

		if (! threadWilight.IsAlive)
		{
			buttonbox_wilight_test.Sensitive = true;
			label_wilight_test_status.Text = "Done";
			return false;
		}

		Thread.Sleep (20);
		return true;
	}

	private void sendCommandAndTextview (string command)
	{
		wichroCapture.WilightSendCommand (command);
		tbWilight.Text += "\n> " + command;
	}

	private void connectWidgetsWilight (Gtk.Builder builder)
	{
		box_start_wilight = (Gtk.Box) builder.GetObject ("box_start_wilight");
		buttonbox_wilight_test = (Gtk.ButtonBox) builder.GetObject ("buttonbox_wilight_test");
		check_wilight_very_verbose = (Gtk.CheckButton) builder.GetObject ("check_wilight_very_verbose");
		label_wilight_test_status = (Gtk.Label) builder.GetObject ("label_wilight_test_status");
		textview_wilight = (Gtk.TextView) builder.GetObject ("textview_wilight");
	}
}

