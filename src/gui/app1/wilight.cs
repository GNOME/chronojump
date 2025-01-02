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

//TODO: note this dirty code is just for testing
public partial class ChronoJumpWindow 
{
	// at glade ---->
	Gtk.Box box_wilight_test;
	Gtk.ButtonBox buttonbox_wilight_test;
	Gtk.Label label_wilight_test_status;
	// <---- at glade

	static Thread threadWilight;
	enum wilightTestTypes { SPEED, SEQUENCE };
	private wilightTestTypes wilightTestType;

	private void on_button_wilight_test_speed_clicked (object o, EventArgs args)
	{
		wilightTestType = wilightTestTypes.SPEED;
		wilightExecute ();
	}

	private void on_button_wilight_test_sequence_clicked (object o, EventArgs args)
	{
		wilightTestType = wilightTestTypes.SEQUENCE;
		wilightExecute ();
	}

	private void wilightExecute ()
	{
		buttonbox_wilight_test.Sensitive = false;
		label_wilight_test_status.Text = "";

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
			wichroCapture = new WichroCapture (portName);

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

		//System.Threading.Thread.Sleep (1000);

		wichroCapture.WilightSendCommand (WilightColors.AllOffCommand);
		if (wilightTestType == wilightTestTypes.SPEED)
		{
			testSpeed ();
			wichroCapture.Stop(); //Should we do a disconnect here?
		}
		else if (wilightTestType == wilightTestTypes.SEQUENCE)
		{
			testSequence (commandsFile);
			wichroCapture.Stop(); //Should we do a disconnect here?
		}
		wichroCapture.WilightSendCommand (WilightColors.AllOffCommand);
	}

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
				//LogB.Information ("\n\n");
				wichroCapture.WilightSendCommand (colorAllStr);
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

	private void testSequence (string commandsFile)
	{
		System.Threading.Thread.Sleep (2500); //to be able to read later
		wichroCapture.Flush (); //to be able to read later

		WilightTest wt = new WilightTest (commandsFile);
		bool finished = false;

		while (true)
		{
			if (finished) //finished here to have also time to answer to the last command
				break;

			string command = wt.GetNext (out finished);
			if (command == "")
				continue;

			wichroCapture.WilightSendCommand (command);

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
					WichroEvent we = wichroCapture.WichroCaptureReadNext();
					if (we.status == Chronopic.Plataforma.ON)
					{
						//LogB.Information ("Is ON!");
						Util.PlaySound (Constants.SoundTypes.GOOD, preferences.volumeOn, preferences.gstreamer);
						readedOn = true;
					}
				}
				System.Threading.Thread.Sleep (20);
			}
		}
	}

	private bool pulseWilight ()
	{
		if (! threadWilight.IsAlive)
		{
			buttonbox_wilight_test.Sensitive = true;
			label_wilight_test_status.Text = "Done";
			return false;
		}
		Thread.Sleep (50);
		return true;
	}

	private void connectWidgetsWilight (Gtk.Builder builder)
	{
		box_wilight_test = (Gtk.Box) builder.GetObject ("box_wilight_test");
		buttonbox_wilight_test = (Gtk.ButtonBox) builder.GetObject ("buttonbox_wilight_test");
		label_wilight_test_status = (Gtk.Label) builder.GetObject ("label_wilight_test_status");
	}
}

