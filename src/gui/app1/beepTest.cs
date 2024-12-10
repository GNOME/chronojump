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
	Gtk.Grid grid_beepTest_type;
	Gtk.RadioButton radio_beepTest_courseNavette;
	Gtk.RadioButton radio_beepTest_pacer15m;
	Gtk.RadioButton radio_beepTest_pacer20m;
	Gtk.Button button_beepTest_start;
	Gtk.Button button_beepTest_finish_selected;
	Gtk.Button button_beepTest_finish_all;
	Gtk.Label label_beepTest_time;
	Gtk.Label label_beepTest_stage;
	Gtk.Label label_beepTest_track;
	Gtk.TextView textview_beepTest;
	// <---- at glade

	static BeepTest beepTest;
	static Thread threadBeepTest;
	TextBuffer tbBeepTest = new TextBuffer (new TextTagTable());

	public void on_button_beepTest_start_clicked (object o, EventArgs args)
	{
		grid_beepTest_type.Sensitive = false;
		button_beepTest_start.Sensitive = false;
		button_beepTest_finish_selected.Sensitive = true;
		button_beepTest_finish_all.Sensitive = true;

                tbBeepTest.Text = "Stage | Track | Name";
                textview_beepTest.Buffer = tbBeepTest;

		if (radio_beepTest_courseNavette.Active)
			beepTest = new CourseNavette ();
		else if (radio_beepTest_pacer15m.Active)
			beepTest = new Pacer15m ();
		else if (radio_beepTest_pacer20m.Active)
			beepTest = new Pacer20m ();

		threadBeepTest = new Thread (new ThreadStart (beepTestDo));
		GLib.Idle.Add (new GLib.IdleHandler (pulseBeepTest));

		threadBeepTest.Start();
	}
	
	public void on_button_beepTest_finish_selected_clicked (object o, EventArgs args)
	{
		if (! threadBeepTest.IsAlive)
			return;

		if (currentPerson == null)
			return;

		BeepTestStageList.StageTrack stageTrack = beepTest.GetCurrentStageAndTrack ();

                tbBeepTest.Text += string.Format ("\n{0,5} | {1,5} | {2}", //note 5 is Stage and Track char lengths. Note on glade this textview is set as monospace
				stageTrack.stage + 1,
				string.Format ("{0}/{1}", stageTrack.track + 1, stageTrack.tracksOfThisStage),
				currentPerson.Name);
                textview_beepTest.Buffer = tbBeepTest;

		restTime.AddOrModify(currentPerson.UniqueID, currentPerson.Name, true);
		updateRestTimes();
	}

	public void on_button_beepTest_finish_all_clicked (object o, EventArgs args)
	{
		if (! threadBeepTest.IsAlive)
			return;

		BeepTestStageList.StageTrack stageTrack = beepTest.GetCurrentStageAndTrack ();

                tbBeepTest.Text += string.Format ("\n{0,5} | {1,5} | {2}", //note 5 is Stage and Track char lengths. Note on glade this textview is set as monospace
				stageTrack.stage + 1,
				string.Format ("{0}/{1}", stageTrack.track + 1, stageTrack.tracksOfThisStage),
				"(Rest of the runners)");
                textview_beepTest.Buffer = tbBeepTest;

		beepTest.Finish ();
	}

	private void beepTestDo ()
	{
		beepTest.Start ();
		while (! beepTest.Finished)
		{
		}
	}

	private bool pulseBeepTest ()
	{
		if (! threadBeepTest.IsAlive)
		{
			grid_beepTest_type.Sensitive = true;
			button_beepTest_start.Sensitive = true;
			button_beepTest_finish_selected.Sensitive = false;
			button_beepTest_finish_all.Sensitive = false;
			return false;
		}

		label_beepTest_time.Text = (beepTest.GetCurrentSeconds ()).ToString ();

		BeepTestStageList.StageTrack stageTrack = beepTest.GetCurrentStageAndTrack ();
		label_beepTest_stage.Text = (stageTrack.stage + 1).ToString ();
		label_beepTest_track.Text = string.Format ("{0} / {1}",
				stageTrack.track + 1, stageTrack.tracksOfThisStage);

		if (beepTest.ShouldBeepNow) //TODO: change tones with https://superuser.com/questions/1118826/change-tone-pitch-for-file-audio (or have them created before)
			 Util.PlaySound(Constants.SoundTypes.CAN_START, preferences.volumeOn, preferences.gstreamer);

		Thread.Sleep (250);
		return true;
	}

	private void connectWidgetsBeepTest (Gtk.Builder builder)
	{
		grid_beepTest_type = (Gtk.Grid) builder.GetObject ("grid_beepTest_type");
		radio_beepTest_courseNavette = (Gtk.RadioButton) builder.GetObject ("radio_beepTest_courseNavette");
		radio_beepTest_pacer15m = (Gtk.RadioButton) builder.GetObject ("radio_beepTest_pacer15m");
		radio_beepTest_pacer20m = (Gtk.RadioButton) builder.GetObject ("radio_beepTest_pacer20m");
		button_beepTest_start = (Gtk.Button) builder.GetObject ("button_beepTest_start");
		button_beepTest_finish_selected = (Gtk.Button) builder.GetObject ("button_beepTest_finish_selected");
		button_beepTest_finish_all = (Gtk.Button) builder.GetObject ("button_beepTest_finish_all");
		label_beepTest_time = (Gtk.Label) builder.GetObject ("label_beepTest_time");
		label_beepTest_stage = (Gtk.Label) builder.GetObject ("label_beepTest_stage");
		label_beepTest_track = (Gtk.Label) builder.GetObject ("label_beepTest_track");
		textview_beepTest = (Gtk.TextView) builder.GetObject ("textview_beepTest");
	}
}
