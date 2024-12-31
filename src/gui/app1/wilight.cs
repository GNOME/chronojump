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
 * Copyright (C) 2024   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Diagnostics; //Stopwatch

//TODO: note this dirty code is just for testing, thread is needed...
public partial class ChronoJumpWindow 
{
	// at glade ---->
	Gtk.ButtonBox buttonbox_wilight_test;
	// <---- at glade

	private void on_button_wilight_test_clicked (object o, EventArgs args)
	{
		wilightTest (configChronojump.WilightPortURL, configChronojump.WilightCommandsURL, configChronojump.WilightCommandMs);
	}

	private void wilightTest (string portName, string commandsFile, int commandTimeMs)
	{
		wichroCapture = new WichroCapture (portName);
		wichroCapture.Reset ();
		if (! wichroCapture.CaptureStart ())
		{
			//chronopicDisconnected = true;
			wichroCapture.Disconnect ();
			//cancel = true; //problem reading line (capturing)
			Util.PlaySound (Constants.SoundTypes.BAD, preferences.volumeOn, preferences.gstreamer);
			LogB.Information ("cannot connect");
		} else
		{
			System.Threading.Thread.Sleep (1000);

			WilightTest wt = new WilightTest (commandsFile);

			//needed to set the default status
			wichroCapture.WilightSendCommand (wt.AllOffCommand);
			System.Threading.Thread.Sleep (1000);
			/*
			wichroCapture.Flush (); //to be able to read later
			System.Threading.Thread.Sleep (1000); //to be able to read later
			*/

			List<int> color_l = new List<int> ();
			color_l.Add (128);
			color_l.Add (64);
			color_l.Add (32);

			List<string> colorsAll_l = new List<string> ();
			colorsAll_l.Add (wt.AllRedCommand);
			colorsAll_l.Add (wt.AllGreenCommand);
			colorsAll_l.Add (wt.AllBlueCommand);
			int sleepTime = 500;
			while (true)
			{
				foreach (string colorAllStr in colorsAll_l)
				{
					//LogB.Information ("\n\n");
					wichroCapture.WilightSendCommand (colorAllStr);

					/*
					//System.Threading.Thread.Sleep (5); //not enough
					//System.Threading.Thread.Sleep (50);

					if (wichroCapture.BytesToRead ())
					{
						string receivedStr = wichroCapture.CaptureEchoLine ();
						LogB.Information ("received: " + receivedStr);
						receivedStr = receivedStr.Trim ();
						LogB.Information ("received2: |" + receivedStr + "|");

						if (colorAllStr == receivedStr)
							Util.PlaySound (Constants.SoundTypes.GOOD, preferences.volumeOn, preferences.gstreamer);
						else
							Util.PlaySound (Constants.SoundTypes.BAD, preferences.volumeOn, preferences.gstreamer);
					}
					*/

					System.Threading.Thread.Sleep (sleepTime);
					sleepTime -= 10;
					if (sleepTime < 100) // arrive to 50 ms is problematic
						sleepTime = 100;
				}
			}

			//needed to set the default status
			wichroCapture.WilightSendCommand (wt.AllOffCommand);
			System.Threading.Thread.Sleep (500);
		
			Stopwatch stopwatch = new Stopwatch ();
			stopwatch.Start ();
			bool finished = false;

			if (commandTimeMs < 0)
				commandTimeMs = 2000;

			while (true)
			{
				if (stopwatch.ElapsedMilliseconds >= commandTimeMs)
				{
					if (finished) //finished here to have also time to answer to the last command
						break;

					string command = wt.GetNext (out finished);
					if (command == "")
						continue;

					wichroCapture.WilightSendCommand (command);
					stopwatch.Restart ();
				}

				//TODO: readed should check that terminal
				bool readedOn = false;
				//int readed = 0;
//				do {
					LogB.Information ("at do while");
					if(! wichroCapture.CaptureSample())
					{
						LogB.Information ("Problem capturing sample");
						Util.PlaySound (Constants.SoundTypes.BAD, preferences.volumeOn, preferences.gstreamer);
						break;
					}

					if(wichroCapture.CanReadFromList ())
					{
						WichroEvent we = wichroCapture.WichroCaptureReadNext();
						LogB.Information ("Readed!: " + we.ToString ());
						if (we.status == Chronopic.Plataforma.ON)
						{
							LogB.Information ("Is ON!");
							Util.PlaySound (Constants.SoundTypes.GOOD, preferences.volumeOn, preferences.gstreamer);
							readedOn = true;
						}
						//readed ++;
					}
					System.Threading.Thread.Sleep (1000);
//				} while (! readedOn);
				//} while (readed < 2);
				//System.Threading.Thread.Sleep (1000);
			}

			wichroCapture.Stop(); //Should we do a disconnect here?
		}
	}
		
	private void connectWidgetsWilight (Gtk.Builder builder)
	{
		buttonbox_wilight_test = (Gtk.ButtonBox) builder.GetObject ("buttonbox_wilight_test");
	}
}

