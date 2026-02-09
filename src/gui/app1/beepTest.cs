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
 * Copyright (C) 2026   Xavier de Blas <xaviblas@gmail.com>
 */


using System;
using Gtk;
//using Gdk;

public partial class ChronoJumpWindow
{
	// at glade ---->
	Gtk.Image image_mode_race_beepTest;
	Gtk.Image image_change_modes_contacts_runs_beepTest;
	Gtk.Box box_beepTest;
	Gtk.Box box_beepTest_type_and_options;
	Gtk.ComboBoxText combo_beepTest_type;
	Gtk.Box box_beepTest_start_at;
	Gtk.SpinButton spin_beepTest_start_at;
	Gtk.CheckButton check_beepTest_start8kmh;
	Gtk.Box box_beepTest_custom_options;
	Gtk.SpinButton spin_beepTest_custom_distM;
	Gtk.SpinButton spin_beepTest_custom_speed;
	Gtk.SpinButton spin_beepTest_custom_totalLaps;
	Gtk.CheckButton check_beepTest_custom_incremental;
	Gtk.Box box_beepTest_custom_incremental;
	Gtk.SpinButton spin_beepTest_custom_incremental;
	Gtk.Button button_beepTest_start;
	Gtk.Button button_beepTest_warn_selected;
	Gtk.Button button_beepTest_exempt_selected;
	Gtk.Button button_beepTest_finish_selected;
	Gtk.Button button_beepTest_finish_all;
	Gtk.Label label_beepTest_time;
	Gtk.Label label_beepTest_stage;
	Gtk.Label label_beepTest_lap;
	Gtk.Label label_beepTest_speed;
	Gtk.Image image_beepTest_runStatus;
	Gtk.Label label_beepTest_runStatus;
	Gtk.Label label_beepTest_runStatus_value;
	Gtk.TextView textview_beepTest;
	Gtk.TextView textview_beepTest_warn;
	// <---- at glade

	static BeepTestCM beepTestCM;
	static Thread threadBeepTest;
	TextBuffer tbBeepTest = new TextBuffer (new TextTagTable());
	TextBuffer tbBeepTestWarn = new TextBuffer (new TextTagTable());


	private void beepTestApp1Init ()
	{
		UtilGtk.ComboUpdate (combo_beepTest_type, BeepTestCM.TypesArray (), "");
		combo_beepTest_type.Active = 0;
		image_beepTest_runStatus.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "image_empty.png");
	}

	private void on_combo_beepTest_type_changed (object o, EventArgs args)
	{
		//Leger
		check_beepTest_start8kmh.Visible = false;
		box_beepTest_start_at.Visible = false;

		//YoYo Intermitent
		label_beepTest_runStatus.Visible = false;
		label_beepTest_runStatus_value.Visible = false;
		label_beepTest_runStatus_value.Text = "";

		//Custom
		box_beepTest_custom_options.Visible = false;

		string str = UtilGtk.ComboGetActive (combo_beepTest_type);

		if (str == BeepTestCM.Leger20Name || str == BeepTestCM.Leger15Name)
		{
			check_beepTest_start8kmh.Visible = true;
			box_beepTest_start_at.Visible = true;
		}
		else if (str == BeepTestCM.YYIE1Name || str == BeepTestCM.YYIE2Name ||
				str == BeepTestCM.YYIR1Name || str == BeepTestCM.YYIR2Name)
		{
			label_beepTest_runStatus.Visible = true;
			label_beepTest_runStatus_value.Visible = true;
		}
		else if (str == BeepTestCM.CustomName)
			box_beepTest_custom_options.Visible = true;

		radio_contacts_graph_currentTest.Label = str;

		//update the treeview
		pre_fillTreeView_resultsSession ();
	}

	private void on_check_beepTest_custom_incremental_clicked (object o, EventArgs args)
	{
		box_beepTest_custom_incremental.Visible = check_beepTest_custom_incremental.Active;
	}

	BeepTestRunners beepTestRunners;
	public void on_button_beepTest_start_clicked (object o, EventArgs args)
	{
		if (currentSession == null)
			return;

		//Create list and mark all as runners
		beepTestRunners = new BeepTestRunners (
			SqlitePersonSession.SelectCurrentSessionPersonsAsList (false, currentSession.UniqueID));

		//put status of all persons as Running (on treeview_persons)
		myTreeViewPersons.UpdateStatus (-1, RunnerStatus.StatusEnum.Running);

		box_beepTest_type_and_options.Sensitive = false;
		button_beepTest_start.Sensitive = false;
		button_beepTest_warn_selected.Sensitive = true;
		button_beepTest_exempt_selected.Sensitive = true;
		button_beepTest_finish_selected.Sensitive = true;
		button_beepTest_finish_all.Sensitive = true;

		double customIncrementalSpeed = Convert.ToDouble (spin_beepTest_custom_incremental.Value);
		if (! check_beepTest_custom_incremental.Active)
			customIncrementalSpeed = 0;

		beepTestCM = BeepTestCM.Factory (
				UtilGtk.ComboGetActive (combo_beepTest_type),
				Convert.ToInt32 (spin_beepTest_start_at.Value),
				check_beepTest_start8kmh.Active,
				Convert.ToInt32 (spin_beepTest_custom_distM.Value),
				Convert.ToDouble (spin_beepTest_custom_speed.Value),
				Convert.ToInt32 (spin_beepTest_custom_totalLaps.Value),
				customIncrementalSpeed
				);

		if (beepTestCM.HasVo2max)
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

		tbBeepTestWarn.Text = "";
                textview_beepTest_warn.Buffer = tbBeepTestWarn;

		threadBeepTest = new Thread (new ThreadStart (beepTestDo));
		GLib.Idle.Add (new GLib.IdleHandler (pulseBeepTest));

		threadBeepTest.Start();
	}

	public void on_button_beepTest_warn_selected_clicked (object o, EventArgs args)
	{
		if (! threadBeepTest.IsAlive)
			return;

		if (currentPerson == null)
			return;

		beepTestCM.WarningAdd (currentPerson.UniqueID, currentPerson.Name);

		//tbBeepTestWarn.Text += beepTest.WarningPrintPerson (currentPerson.UniqueID);
		tbBeepTestWarn.Text = beepTestCM.WarningPrintAll ();
                textview_beepTest_warn.Buffer = tbBeepTestWarn;
	}

	private void beepTestPrintResults (string personName,
			BeepTestStageManage.StageLapStatus slStatus, bool hasVo2Max)
	{

		//note 5 is "Stage" and " Lap " char lengths. Note on glade this textview is set as monospace
		if (hasVo2Max)
			tbBeepTest.Text += string.Format ("\n {0,5} | {1,5} | {2,5} | {3,6} | {4}",
					slStatus.stageName,
					string.Format ("{0}/{1}", slStatus.lap + 1, slStatus.lapsOfThisStage),
					Util.TrimDecimals (slStatus.speedKmh, 1),
					Util.TrimDecimals (beepTestCM.Vo2max (), 2),
					personName);
		else
			tbBeepTest.Text += string.Format ("\n {0,5} | {1,5} | {2,5} | {3}",
					slStatus.stageName,
					string.Format ("{0}/{1}", slStatus.lap + 1, slStatus.lapsOfThisStage),
					Util.TrimDecimals (slStatus.speedKmh, 1),
					personName);

                textview_beepTest.Buffer = tbBeepTest;
	}

	private void beepTestPersonChanged ()
	{
		if (beepTestRunners == null)
			return;

		button_beepTest_exempt_selected.Sensitive = beepTestRunners.IsRunning (currentPerson.UniqueID);
		button_beepTest_finish_selected.Sensitive = beepTestRunners.IsRunning (currentPerson.UniqueID);
	}

	public void on_button_beepTest_exempt_selected_clicked (object o, EventArgs args)
	{
		beepTestRunners.Exempt (currentPerson.UniqueID);
		myTreeViewPersons.UpdateStatus (currentPerson.UniqueID, RunnerStatus.StatusEnum.Exempt);

		button_beepTest_exempt_selected.Sensitive = false;
		button_beepTest_finish_selected.Sensitive = false;
	}

	public void on_button_beepTest_finish_selected_clicked (object o, EventArgs args)
	{
		if (! threadBeepTest.IsAlive)
			return;

		if (currentPerson == null)
			return;

		beepTestRunners.Finished (currentPerson.UniqueID);

		button_beepTest_exempt_selected.Sensitive = false;
		button_beepTest_finish_selected.Sensitive = false;

		BeepTestStageManage.StageLapStatus slStatus = beepTestCM.GetCurrentStageLapStatus ();
		beepTestFinishInsertPerson (currentPerson.UniqueID, currentPerson.Name, slStatus, beepTestCM.ExerciseID, beepTestCM.GetOptions);
		beepTestPrintResults (currentPerson.Name, slStatus, beepTestCM.HasVo2max);

		myTreeViewPersons.UpdateStatus (currentPerson.UniqueID, RunnerStatus.StatusEnum.Finished);

		restTime.AddOrModify(currentPerson.UniqueID, currentPerson.Name, true);
		updateRestTimes();

		if (! beepTestRunners.AnyoneRunning)
			beepTestCM.Finish ();
	}

	public void on_button_beepTest_finish_all_clicked (object o, EventArgs args)
	{
		if (! threadBeepTest.IsAlive)
			return;

		BeepTestStageManage.StageLapStatus slStatus = beepTestCM.GetCurrentStageLapStatus ();
		List<int> finishedNow_l = beepTestRunners.FinishAllRunners ();
		foreach (int i in finishedNow_l)
		{
			string personName = beepTestRunners.GetName (i);
			beepTestFinishInsertPerson (i, personName, slStatus, beepTestCM.ExerciseID, beepTestCM.GetOptions);
			beepTestPrintResults (personName, slStatus, beepTestCM.HasVo2max);

			myTreeViewPersons.UpdateStatus (i, RunnerStatus.StatusEnum.Finished);
		}

		beepTestCM.Finish ();
	}

	private void beepTestFinishInsertPerson (int personID, string personName, BeepTestStageManage.StageLapStatus slStatus, int exerciseID, string options)
	{
                BeepTest bt = new BeepTest (-1, personID, currentSession.UniqueID, exerciseID,
                                options, slStatus.stage, slStatus.lap, 0, slStatus.speedKmh,
                                UtilDate.ToFile (DateTime.Now), "", "");
                bt.InsertSQL (false);
		updatePersonTestsN (false);
		treeViewResultsSession.Add (personID, personName, bt, "");
	}

	private void beepTestDo ()
	{
		beepTestCM.Start ();
		while (! beepTestCM.Finished)
		{
		}
	}

	private bool pulseBeepTest ()
	{
		if (! threadBeepTest.IsAlive)
		{
			box_beepTest_type_and_options.Sensitive = true;
			button_beepTest_start.Sensitive = true;
			button_beepTest_warn_selected.Sensitive = false;
			button_beepTest_exempt_selected.Sensitive = false;
			button_beepTest_finish_selected.Sensitive = false;
			button_beepTest_finish_all.Sensitive = false;
			label_beepTest_runStatus_value.Text = "";
			image_beepTest_runStatus.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "image_empty.png");

			return false;
		}

		label_beepTest_time.Text = (beepTestCM.GetCurrentSeconds ()).ToString ();

		BeepTestStageManage.StageLapStatus slStatus = beepTestCM.GetCurrentStageLapStatus ();

		label_beepTest_stage.Text = slStatus.stageName;
		label_beepTest_lap.Text = string.Format ("{0} / {1}",
				slStatus.lap + 1, slStatus.lapsOfThisStage);
		label_beepTest_speed.Text = Util.TrimDecimals(slStatus.speedKmh, 1);

		if (slStatus.resting)
			label_beepTest_runStatus_value.Text = "Resting";
		else
			label_beepTest_runStatus_value.Text = "Running";

		if (beepTestCM.ShouldBeepNow == BeepTestCM.BeepNowEnum.STAGE)
			 Util.PlaySoundGstreamerFromFile (beepTestCM.GetSoundFileForStage (slStatus.stage, true),
					 preferences.volumeOn, preferences.gstreamer, 2);
		else if (beepTestCM.ShouldBeepNow == BeepTestCM.BeepNowEnum.LAP)
			 Util.PlaySoundGstreamerFromFile (beepTestCM.GetSoundFileForStage (slStatus.stage, false),
					 preferences.volumeOn, preferences.gstreamer, 1);

		if (beepTestCM.ShouldVoiceNow == BeepTestCM.VoiceNowEnum.STAGE)
			 Util.PlaySoundGstreamerFromFile (beepTestCM.GetVoiceFile (slStatus.stage, false),
					 preferences.volumeOn, preferences.gstreamer, 1);
		else if (beepTestCM.ShouldVoiceNow == BeepTestCM.VoiceNowEnum.MID)
			 Util.PlaySoundGstreamerFromFile (beepTestCM.GetVoiceFile (slStatus.stage, true),
					 preferences.volumeOn, preferences.gstreamer, 1);

		image_beepTest_runStatus.Pixbuf = beepTestCM.ImageLapStatus;

		Thread.Sleep (20);
		return true;
	}

	private void connectWidgetsBeepTest (Gtk.Builder builder)
	{
		image_mode_race_beepTest = (Gtk.Image) builder.GetObject ("image_mode_race_beepTest");
		image_change_modes_contacts_runs_beepTest = (Gtk.Image) builder.GetObject ("image_change_modes_contacts_runs_beepTest");
		box_beepTest = (Gtk.Box) builder.GetObject ("box_beepTest");
		box_beepTest_type_and_options = (Gtk.Box) builder.GetObject ("box_beepTest_type_and_options");
		combo_beepTest_type = (Gtk.ComboBoxText) builder.GetObject ("combo_beepTest_type");
		check_beepTest_start8kmh = (Gtk.CheckButton) builder.GetObject ("check_beepTest_start8kmh");
		box_beepTest_start_at = (Gtk.Box) builder.GetObject ("box_beepTest_start_at");
		spin_beepTest_start_at = (Gtk.SpinButton) builder.GetObject ("spin_beepTest_start_at");
		box_beepTest_custom_options = (Gtk.Box) builder.GetObject ("box_beepTest_custom_options");
		spin_beepTest_custom_distM = (Gtk.SpinButton) builder.GetObject ("spin_beepTest_custom_distM");
		spin_beepTest_custom_speed = (Gtk.SpinButton) builder.GetObject ("spin_beepTest_custom_speed");
		spin_beepTest_custom_totalLaps = (Gtk.SpinButton) builder.GetObject ("spin_beepTest_custom_totalLaps");
		check_beepTest_custom_incremental = (Gtk.CheckButton) builder.GetObject ("check_beepTest_custom_incremental");
		box_beepTest_custom_incremental = (Gtk.Box) builder.GetObject ("box_beepTest_custom_incremental");
		spin_beepTest_custom_incremental = (Gtk.SpinButton) builder.GetObject ("spin_beepTest_custom_incremental");
		button_beepTest_start = (Gtk.Button) builder.GetObject ("button_beepTest_start");
		button_beepTest_warn_selected = (Gtk.Button) builder.GetObject ("button_beepTest_warn_selected");
		button_beepTest_exempt_selected = (Gtk.Button) builder.GetObject ("button_beepTest_exempt_selected");
		button_beepTest_finish_selected = (Gtk.Button) builder.GetObject ("button_beepTest_finish_selected");
		button_beepTest_finish_all = (Gtk.Button) builder.GetObject ("button_beepTest_finish_all");
		label_beepTest_time = (Gtk.Label) builder.GetObject ("label_beepTest_time");
		label_beepTest_stage = (Gtk.Label) builder.GetObject ("label_beepTest_stage");
		label_beepTest_lap = (Gtk.Label) builder.GetObject ("label_beepTest_lap");
		label_beepTest_speed = (Gtk.Label) builder.GetObject ("label_beepTest_speed");
		image_beepTest_runStatus = (Gtk.Image) builder.GetObject ("image_beepTest_runStatus");
		label_beepTest_runStatus = (Gtk.Label) builder.GetObject ("label_beepTest_runStatus");
		label_beepTest_runStatus_value = (Gtk.Label) builder.GetObject ("label_beepTest_runStatus_value");
		textview_beepTest = (Gtk.TextView) builder.GetObject ("textview_beepTest");
		textview_beepTest_warn = (Gtk.TextView) builder.GetObject ("textview_beepTest_warn");
	}
}
