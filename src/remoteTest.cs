/*
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
 *
 * Copyright (C) 2024  Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Threading;
using Gtk;

public class RemoteTest
{
	private Constants.Modes current_mode;
	private string jumpSimpleFile;
	private string runIntervalFile;
	private bool remoteTestDoing; //to just send the signal one time
	private bool stop; //to end thread on Chronojump exit

	private Gtk.Button fakeButtonDo; //send the signal to start capture

	//constructor
	public RemoteTest (Constants.Modes current_mode, string jumpSimpleFile, string runIntervalFile)
	{
		this.current_mode = current_mode;
		this.jumpSimpleFile = jumpSimpleFile;
		this.runIntervalFile = runIntervalFile;

		remoteTestDoing = false;
		stop = false;
	
		fakeButtonDo = new Button ();
	}

	//all the time running
	public void CheckFileCreation ()
	{
		while (! stop)
		{
			Thread.Sleep (500);
			if (! remoteTestDoing)
			{
				if (current_mode == Constants.Modes.JUMPSSIMPLE &&
						Util.FileExists (jumpSimpleFile) &&
						! File.Exists (jumpSimpleFile + "Done")) //just be careful
				{
					remoteTestDoing = true;
					fakeButtonDo.Click ();
				}
				else if (current_mode == Constants.Modes.RUNSINTERVALLIC &&
						Util.FileExists (runIntervalFile) &&
						! File.Exists (runIntervalFile + "Done")) //just be careful
				{
					remoteTestDoing = true;
					fakeButtonDo.Click ();
				}
			}
		}
	}

	//to show mark of Done and to be able to capture again
	public void Captured (string file)
	{
		if (Util.FileExists (file))
		{
                        File.Create (file + "Done");
			remoteTestDoing = false;
		}
	}

	//when Chronojump exits
	public void Stop()
	{
		stop = true;
	}

	public Constants.Modes Current_mode
	{
		set { current_mode = value; }
	}

	public Gtk.Button FakeButtonDo
	{
		get { return fakeButtonDo; }
	}

}
