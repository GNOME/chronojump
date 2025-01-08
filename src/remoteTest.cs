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
	private string cancelFile;
	private string remoteTestTestName;
	private bool remoteTestDoing; //to just send the signal one time
	private bool stop; //to end thread on Chronojump exit

	private Gtk.Button fakeButtonDo; //send the signal to start capture
	private Gtk.Button fakeButtonCancel; //send the signal to cancel

	//constructor
	public RemoteTest (Constants.Modes current_mode, string jumpSimpleFile, string runIntervalFile, string cancelFile)
	{
		this.current_mode = current_mode;
		this.jumpSimpleFile = jumpSimpleFile;
		this.runIntervalFile = runIntervalFile;
		this.cancelFile = cancelFile;

		remoteTestTestName = "";
		remoteTestDoing = false;
		stop = false;
	
		fakeButtonDo = new Button ();
		fakeButtonCancel = new Button ();
	}

	//all the time running
	public void CheckFileCreation ()
	{
		while (! stop)
		{
			Thread.Sleep (500);
			if (remoteTestDoing)
			{
				if (Util.FileExists (cancelFile))
				{
					remoteTestDoing = false;

					//sleep to avoid problems on deleting cancel file created by another program
					Thread.Sleep (100);

					Util.FileDelete (cancelFile);
					if (Util.FileExists (jumpSimpleFile))
						Util.FileDelete (jumpSimpleFile);
					if (Util.FileExists (runIntervalFile))
						Util.FileDelete (runIntervalFile);

					fakeButtonCancel.Click ();
				}
			}
			else
			{
				/*
				 * maybe cancel is being send n times, even before the capture,
				 * or even the file exists when Chronojump is opened,
				 * so if we are not doing test, delete it
				 */
				if (Util.FileExists (cancelFile))
					Util.FileDelete (cancelFile);

				// jump simple test
				if (current_mode == Constants.Modes.JUMPSSIMPLE &&
						Util.FileExists (jumpSimpleFile) &&
						! File.Exists (jumpSimpleFile + "Done")) //just be careful
				{
					remoteTestDoing = true;
					remoteTestTestName = "";
					if (Util.FileReadable (jumpSimpleFile))
						remoteTestTestName = Util.ReadFile (jumpSimpleFile, true);

					fakeButtonDo.Click ();
				}
				// run interval test
				else if (current_mode == Constants.Modes.RUNSINTERVALLIC &&
						Util.FileExists (runIntervalFile) &&
						! File.Exists (runIntervalFile + "Done")) //just be careful
				{
					remoteTestDoing = true;
					remoteTestTestName = "";
					if (Util.FileReadable (runIntervalFile))
						remoteTestTestName = Util.ReadFile (runIntervalFile, true);
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

	public Gtk.Button FakeButtonCancel
	{
		get { return fakeButtonCancel; }
	}

	public string RemoteTestTestName
	{
		get { return remoteTestTestName; }
	}

}
