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
	Gtk.Image image_mode_race_beepTest;
	Gtk.Image image_change_modes_contacts_runs_beepTest;
	Gtk.Box box_beepTest_type_and_options;
	Gtk.RadioButton radio_beepTest_leger20m;
	Gtk.RadioButton radio_beepTest_leger15m;
	Gtk.RadioButton radio_beepTest_pacer15m;
	Gtk.RadioButton radio_beepTest_yyie1;
	Gtk.RadioButton radio_beepTest_pacer20m;
	Gtk.RadioButton radio_beepTest_constant;
	Gtk.Box box_beepTest_start_at;
	Gtk.SpinButton spin_beepTest_start_at;
	Gtk.CheckButton check_beepTest_start8kmh;
	Gtk.Box box_beepTest_constant_options;
	Gtk.SpinButton spin_beepTest_constant_distM;
	Gtk.SpinButton spin_beepTest_constant_speed;
	Gtk.SpinButton spin_beepTest_constant_totalLaps;
	Gtk.Button button_beepTest_start;
	Gtk.Button button_beepTest_finish_selected;
	Gtk.Button button_beepTest_finish_all;
	Gtk.Label label_beepTest_time;
	Gtk.Label label_beepTest_stage;
	Gtk.Label label_beepTest_lap;
	Gtk.Label label_beepTest_speed;
	Gtk.Label label_beepTest_runStatus;
	Gtk.Label label_beepTest_runStatus_value;
	Gtk.TextView textview_beepTest;
	// <---- at glade

	static BeepTest beepTest;
	static Thread threadBeepTest;
	TextBuffer tbBeepTest = new TextBuffer (new TextTagTable());

	private void on_radio_beepTest_toggled (object o, EventArgs args)
	{
		check_beepTest_start8kmh.Visible = (radio_beepTest_leger20m.Active || radio_beepTest_leger15m.Active);
		box_beepTest_start_at.Visible = (radio_beepTest_leger20m.Active || radio_beepTest_leger15m.Active);

		label_beepTest_runStatus.Visible = (radio_beepTest_yyie1.Active);
		label_beepTest_runStatus_value.Visible = (radio_beepTest_yyie1.Active);
		label_beepTest_runStatus_value.Text = "";

		box_beepTest_constant_options.Visible = radio_beepTest_constant.Active;
	}

	public void on_button_beepTest_start_clicked (object o, EventArgs args)
	{
		box_beepTest_type_and_options.Sensitive = false;
		button_beepTest_start.Sensitive = false;
		button_beepTest_finish_selected.Sensitive = true;
		button_beepTest_finish_all.Sensitive = true;

		if (radio_beepTest_leger20m.Active)
			beepTest = new BeepTestLeger20m (Convert.ToInt32 (spin_beepTest_start_at.Value), check_beepTest_start8kmh.Active);
		else if (radio_beepTest_leger15m.Active)
			beepTest = new BeepTestLeger15m (Convert.ToInt32 (spin_beepTest_start_at.Value), check_beepTest_start8kmh.Active);
		else if (radio_beepTest_yyie1.Active)
			beepTest = new BeepTestYYIE1 ();
		/*
		else if (radio_beepTest_pacer15m.Active)
			beepTest = new Pacer15m ();
		else if (radio_beepTest_pacer20m.Active)
			beepTest = new Pacer20m ();
			*/
		else if (radio_beepTest_constant.Active)
			beepTest = new BeepTestConstantSpeed (
					Convert.ToInt32 (spin_beepTest_constant_distM.Value),
					Convert.ToDouble (spin_beepTest_constant_speed.Value),
					Convert.ToInt32 (spin_beepTest_constant_totalLaps.Value));

		if (beepTest.HasVo2max)
		{
			tbBeepTest.Text =
				" Stage |  Lap  | Speed | VO2max | Name " +
				"\n" +
				" ----- | ----- | ----- | ------ | ---- ";
		} else {
			tbBeepTest.Text =
				" Stage |  Lap  | Speed | Name " +
				"\n" +
				" ----- | ----- | ----- | ---- ";
		}
                textview_beepTest.Buffer = tbBeepTest;

		threadBeepTest = new Thread (new ThreadStart (beepTestDo));
		GLib.Idle.Add (new GLib.IdleHandler (pulseBeepTest));

		threadBeepTest.Start();
	}

	private void beepTestPrintResults (bool allPersons, bool hasVo2Max)
	{
		string personName = currentPerson.Name;
		if (allPersons)
			personName = "(Rest of the runners)";

		BeepTestStageManage.StageLapStatus slStatus = beepTest.GetCurrentStageLapStatus ();

		//note 5 is "Stage" and " Lap " char lengths. Note on glade this textview is set as monospace
		if (hasVo2Max)
			tbBeepTest.Text += string.Format ("\n {0,5} | {1,5} | {2,5} | {3,6} | {4}",
					slStatus.stage + 1,
					string.Format ("{0}/{1}", slStatus.lap + 1, slStatus.lapsOfThisStage),
					Util.TrimDecimals (slStatus.speedKmh, 1),
					Util.TrimDecimals (beepTest.Vo2max (slStatus.speedKmh), 2),
					personName);
		else
			tbBeepTest.Text += string.Format ("\n {0,5} | {1,5} | {2,5} | {3}",
					slStatus.stage + 1,
					string.Format ("{0}/{1}", slStatus.lap + 1, slStatus.lapsOfThisStage),
					Util.TrimDecimals (slStatus.speedKmh, 1),
					personName);

                textview_beepTest.Buffer = tbBeepTest;
	}

	public void on_button_beepTest_finish_selected_clicked (object o, EventArgs args)
	{
		if (! threadBeepTest.IsAlive)
			return;

		if (currentPerson == null)
			return;

		beepTestPrintResults (false, beepTest.HasVo2max);

		restTime.AddOrModify(currentPerson.UniqueID, currentPerson.Name, true);
		updateRestTimes();
	}

	public void on_button_beepTest_finish_all_clicked (object o, EventArgs args)
	{
		if (! threadBeepTest.IsAlive)
			return;

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
			box_beepTest_type_and_options.Sensitive = true;
			button_beepTest_start.Sensitive = true;
			button_beepTest_finish_selected.Sensitive = false;
			button_beepTest_finish_all.Sensitive = false;
			label_beepTest_runStatus_value.Text = "";

			beepTestPrintResults (true, beepTest.HasVo2max);
			return false;
		}

		label_beepTest_time.Text = (beepTest.GetCurrentSeconds ()).ToString ();

		BeepTestStageManage.StageLapStatus slStatus = beepTest.GetCurrentStageLapStatus ();
		label_beepTest_stage.Text = (slStatus.stage + 1).ToString ();
		label_beepTest_lap.Text = string.Format ("{0} / {1}",
				slStatus.lap + 1, slStatus.lapsOfThisStage);
		label_beepTest_speed.Text = Util.TrimDecimals(slStatus.speedKmh, 1);

		if (slStatus.resting)
			label_beepTest_runStatus_value.Text = "Resting";
		else
			label_beepTest_runStatus_value.Text = "Running";

		if (beepTest.ShouldBeepNow == BeepTest.BeepNowEnum.STAGE)
			 Util.PlaySoundGstreamerFromFile (beepTest.GetSoundFileForStage (slStatus.stage, true),
					 preferences.volumeOn, preferences.gstreamer, 2);
		else if (beepTest.ShouldBeepNow == BeepTest.BeepNowEnum.LAP)
			 Util.PlaySoundGstreamerFromFile (beepTest.GetSoundFileForStage (slStatus.stage, false),
					 preferences.volumeOn, preferences.gstreamer, 1);

		Thread.Sleep (100);
		return true;
	}

	private void connectWidgetsBeepTest (Gtk.Builder builder)
	{
		image_mode_race_beepTest = (Gtk.Image) builder.GetObject ("image_mode_race_beepTest");
		image_change_modes_contacts_runs_beepTest = (Gtk.Image) builder.GetObject ("image_change_modes_contacts_runs_beepTest");
		box_beepTest_type_and_options = (Gtk.Box) builder.GetObject ("box_beepTest_type_and_options");
		radio_beepTest_leger20m = (Gtk.RadioButton) builder.GetObject ("radio_beepTest_leger20m");
		radio_beepTest_leger15m = (Gtk.RadioButton) builder.GetObject ("radio_beepTest_leger15m");
		radio_beepTest_yyie1 = (Gtk.RadioButton) builder.GetObject ("radio_beepTest_yyie1");
		radio_beepTest_pacer15m = (Gtk.RadioButton) builder.GetObject ("radio_beepTest_pacer15m");
		radio_beepTest_pacer20m = (Gtk.RadioButton) builder.GetObject ("radio_beepTest_pacer20m");
		radio_beepTest_constant = (Gtk.RadioButton) builder.GetObject ("radio_beepTest_constant");
		check_beepTest_start8kmh = (Gtk.CheckButton) builder.GetObject ("check_beepTest_start8kmh");
		box_beepTest_start_at = (Gtk.Box) builder.GetObject ("box_beepTest_start_at");
		spin_beepTest_start_at = (Gtk.SpinButton) builder.GetObject ("spin_beepTest_start_at");
		box_beepTest_constant_options = (Gtk.Box) builder.GetObject ("box_beepTest_constant_options");
		spin_beepTest_constant_distM = (Gtk.SpinButton) builder.GetObject ("spin_beepTest_constant_distM");
		spin_beepTest_constant_speed = (Gtk.SpinButton) builder.GetObject ("spin_beepTest_constant_speed");
		spin_beepTest_constant_totalLaps = (Gtk.SpinButton) builder.GetObject ("spin_beepTest_constant_totalLaps");
		button_beepTest_start = (Gtk.Button) builder.GetObject ("button_beepTest_start");
		button_beepTest_finish_selected = (Gtk.Button) builder.GetObject ("button_beepTest_finish_selected");
		button_beepTest_finish_all = (Gtk.Button) builder.GetObject ("button_beepTest_finish_all");
		label_beepTest_time = (Gtk.Label) builder.GetObject ("label_beepTest_time");
		label_beepTest_stage = (Gtk.Label) builder.GetObject ("label_beepTest_stage");
		label_beepTest_lap = (Gtk.Label) builder.GetObject ("label_beepTest_lap");
		label_beepTest_speed = (Gtk.Label) builder.GetObject ("label_beepTest_speed");
		label_beepTest_runStatus = (Gtk.Label) builder.GetObject ("label_beepTest_runStatus");
		label_beepTest_runStatus_value = (Gtk.Label) builder.GetObject ("label_beepTest_runStatus_value");
		textview_beepTest = (Gtk.TextView) builder.GetObject ("textview_beepTest");
	}
}
