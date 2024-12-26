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
			wichroCapture.WilightSendCommand (wt.DefaultStatusCommand);

			System.Threading.Thread.Sleep (1000);
		
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

					wichroCapture.WilightSendCommand (command);
					stopwatch.Restart ();
				}

				//bool readed = false;
				//int readed = 0;
				//do {
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
						Util.PlaySound (Constants.SoundTypes.GOOD, preferences.volumeOn, preferences.gstreamer);
						//readed ++;
					}
				//} while (readed < 2);
				//System.Threading.Thread.Sleep (1000);
			}

			wichroCapture.Stop(); //Should we do a disconnect here?
		}
	}
}

