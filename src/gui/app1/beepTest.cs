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
using Gtk;
//using Gdk;

public partial class ChronoJumpWindow
{
	// at glade ---->
	Gtk.Label label_beepTest_time;
	Gtk.Label label_beepTest_stage;
	Gtk.Label label_beepTest_track;
	// <---- at glade

	static CourseNavette courseNavette;
	static Thread courseNavetteThread;

	//TODO: need to play with sensitivity of start button
	//TODO: need to check if thread is running
	//TODO: need to have a button to stop
	//TODO: need to auto stop on Chronojump end
	public void on_button_beepTest_start_clicked (object o, EventArgs args)
	{
		courseNavette = new CourseNavette ();

		courseNavetteThread = new Thread (new ThreadStart (courseNavetteDo));
		GLib.Idle.Add (new GLib.IdleHandler (pulseCourseNavette));

		courseNavetteThread.Start();
	}

	private void courseNavetteDo ()
	{
		courseNavette.Start ();
		while (true)
		{
		}
	}

	private bool pulseCourseNavette ()
	{
		if (! courseNavetteThread.IsAlive)
		{
			//TODO
			return false;
		}

		label_beepTest_time.Text = (courseNavette.GetCurrentSeconds ()).ToString ();

		IntInt stageAndTrack = courseNavette.GetCurrentStageAndTrack ();
		label_beepTest_stage.Text = (stageAndTrack.a + 1).ToString ();
		label_beepTest_track.Text = (stageAndTrack.b + 1).ToString ();

		Thread.Sleep (250);
		return true;
	}

	private void connectWidgetsBeepTest (Gtk.Builder builder)
	{
		label_beepTest_time = (Gtk.Label) builder.GetObject ("label_beepTest_time");
		label_beepTest_stage = (Gtk.Label) builder.GetObject ("label_beepTest_stage");
		label_beepTest_track = (Gtk.Label) builder.GetObject ("label_beepTest_track");
	}
}
