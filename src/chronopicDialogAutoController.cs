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
 * Copyright (C) 2004-2015   Xavier de Blas <xaviblas@gmail.com> 
 */


using System.Threading;
using System;
using Gtk;

public class ChronopicDialogAutoController
{
	ChronopicDialogAuto cp_dialog_auto;
	Thread thread;
	
	private static bool cancel;
	public string Detected;
	
	public Gtk.Button FakeButtonDone;
	
	
	public ChronopicDialogAutoController ()
	{
		cp_dialog_auto = new ChronopicDialogAuto();
		cp_dialog_auto.button_cancel.Clicked += new EventHandler(on_button_cancel_clicked);
		FakeButtonDone = new Gtk.Button();
	}
	
	public void Detect(string mode)
	{
		if(mode == "ENCODER") {
			LogB.Information("Detecting encoder... ");
			
			cancel = false;
			Detected = "";
		
			thread = new Thread(new ThreadStart(detectEncoder));
			GLib.Idle.Add (new GLib.IdleHandler (PulseGTK));

			LogB.ThreadStart();
			thread.Start(); 
		}
	}

	private void detectEncoder()
	{
		/*
		 * testing a fault in drivers
		int count = 0;
		bool crash = true;
		while(crash) {
			count ++;
			if(count >= 1000) {
				LogB.Debug(" at detectEncoder\n ");
				count = 0;
			}
		}
		*/

		ChronopicAutoDetect cad = 
			new ChronopicAutoDetect(ChronopicAutoDetect.ChronopicType.ENCODER);

		Detected = cad.Detected;
	}
	
	private bool PulseGTK ()
	{
		if(cancel || ! thread.IsAlive) {
			LogB.ThreadEnding();

			if(cancel)
				thread.Abort();
			
			FakeButtonDone.Click();
			
			LogB.ThreadEnded();
			return false;
		}

		cp_dialog_auto.progressbar.Pulse();
		
		Thread.Sleep (50);
		LogB.Debug(thread.ThreadState.ToString());
		return true;
	}
	
	private void on_button_cancel_clicked (object o, EventArgs args)
	{
		cp_dialog_auto.button_cancel.Clicked -= new EventHandler(on_button_cancel_clicked);

		Detected = "Cancelled";
		cancel = true;
	}

	~ChronopicDialogAutoController() {}
}
