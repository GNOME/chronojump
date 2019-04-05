/*
 * This file is part of ChronoJump
 *
 * ChronoJump is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or   
 *    (at your option) any later version.
 *    
 * ChronoJump is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
 *    GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *   Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 * Copyright (C) 2019   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Gtk;
using Glade;

public partial class ChronoJumpWindow 
{
	[Widget] Gtk.VSeparator vseparator_force_sensor_camera_space;
	[Widget] Gtk.VBox vbox_contacts_camera;
	[Widget] Gtk.CheckButton checkbutton_video;
	[Widget] Gtk.CheckButton checkbutton_video_encoder;
	[Widget] Gtk.HBox hbox_video_capture;
	[Widget] Gtk.HBox hbox_video_encoder;
	[Widget] Gtk.HBox hbox_video_encoder_no_capturing;
	[Widget] Gtk.HBox hbox_video_encoder_capturing;
	[Widget] Gtk.Label label_video_feedback;
	[Widget] Gtk.Label label_video_encoder_feedback;
	[Widget] Gtk.Button button_video_preview;
	[Widget] Gtk.Button button_video_encoder_preview;
	//[Widget] Gtk.Label label_video;
	[Widget] Gtk.Image image_video_yes;
	[Widget] Gtk.Image image_video_no;
	[Widget] Gtk.Image image_video_encoder_yes;
	[Widget] Gtk.Image image_video_encoder_no;
	[Widget] Gtk.Button button_video_play_this_test;
	[Widget] Gtk.Button button_video_play_this_test_encoder;


	private enum WebcamEncoderFileStarted { NEEDTOCHECK, RECORDSTARTED, NOCAMERA }
	private WebcamEncoderFileStarted webcamEncoderFileStarted;

	//should be visible on all contacts, but right now hide it on force sensor and runEncoder
	//but we need database stuff first
	public void showWebcamCapture (bool show)
	{
		vseparator_force_sensor_camera_space.Visible = false; //extra space before camera on force sensor
		vbox_contacts_camera.Visible = show;
		button_video_play_this_test.Visible = show;
	}

	/* ---------------------------------------------------------
	 * ----------------  Webcam manage on execution ------------
	 *  --------------------------------------------------------
	 */

	/*
	 * TODO:
	 * if there are two cameras
	 * have two webcam and call: webcamRecordStart and webcamRecordEnd two times
	 * take care pngs of 2n camera have to be in different area
	 * maybe use on /tmp/chronojump-video0 /tmp/chronojump-video1 ...
	 * and at the end merge both mp4s with:
	 *
	 * ffmpeg \
	 *  -i RUN-12.mp4 \
	 *  -i RUN-11.mp4 \
	 *  -filter_complex '[0:v]pad=iw*2:ih[int];[int][1:v]overlay=W/2:0[vid]' \
	 *  -map [vid] \
	 *  -c:v libx264 \
	 *  -crf 23 \
	 *  -preset veryfast \
	 *  output.mp4
	 *
	 *  https://unix.stackexchange.com/questions/233832/merge-two-video-clips-into-one-placing-them-next-to-each-other
	 *  2nd solution merges audios
	 *  3a solucio diu que la primera perd molts frames
	 */

	WebcamManage webcamManage;

	private bool webcamStart (WebcamManage.GuiContactsEncoder guiContactsEncoder, int ncams)
	{
		if(guiContactsEncoder == WebcamManage.GuiContactsEncoder.ENCODER)
			hbox_video_encoder.Sensitive = false;

		if(! preferences.videoOn || webcamManage == null)
			return false;

		if(guiContactsEncoder == WebcamManage.GuiContactsEncoder.ENCODER)
		{
			hbox_video_encoder_no_capturing.Visible = false;
			hbox_video_encoder_capturing.Visible = true;
		}

		button_video_preview_visibile (guiContactsEncoder, false);

		string errorMessage = "";
		if(ncams == 1 && webcamManage.RecordPrepare(preferences.videoDevice, preferences.videoDeviceResolution, preferences.videoDeviceFramerate).success)
		{
			webcamManage.RecordStart(1);
			//label_video_feedback.Text = "Preparing camera";
			label_video_feedback_text (guiContactsEncoder, "Preparing camera");
		}
		else if(ncams == 2 && webcamManage.RecordPrepare(preferences.videoDevice, "/dev/video1", preferences.videoDeviceResolution, preferences.videoDeviceFramerate).success)
		{
			webcamManage.RecordStart(2);
			//label_video_feedback.Text = "Preparing camera";
			label_video_feedback_text (guiContactsEncoder, "Preparing camera");
		}
		//TODO depending on errorMessage:
		//new DialogMessage(Constants.MessageTypes.WARNING, result.error);
		//button_video_play_this_test.Sensitive = false;

		return true;
	}

	private void button_video_preview_visibile (WebcamManage.GuiContactsEncoder guiContactsEncoder, bool visible)
	{
		if(guiContactsEncoder == WebcamManage.GuiContactsEncoder.CONTACTS)
			button_video_preview.Visible = visible;
		else
			button_video_encoder_preview.Visible = visible;
	}
	private void label_video_feedback_text (WebcamManage.GuiContactsEncoder guiContactsEncoder, string text)
	{
		if(guiContactsEncoder == WebcamManage.GuiContactsEncoder.CONTACTS)
			label_video_feedback.Text = text;
		else
			label_video_encoder_feedback.Text = text;
	}
	private void button_video_play_this_test_sensitive (WebcamManage.GuiContactsEncoder guiContactsEncoder, bool s)
	{
		if(guiContactsEncoder == WebcamManage.GuiContactsEncoder.CONTACTS)
			button_video_play_this_test.Sensitive = s;
		else
			//button_video_encoder_play_this_test.Sensitive = s;
			button_video_play_this_test_encoder.Sensitive = s; //TODO:jugar amb la sensitivitat de aixo quan hi ha o no signalUniqueID 
	}


	//can pass a -1 uniqueID if test is cancelled
	private void webcamEnd (Constants.TestTypes testType, int uniqueID)
	{
		WebcamManage.GuiContactsEncoder guiContactsEncoder = WebcamManage.GuiContactsEncoder.CONTACTS;
		if(testType == Constants.TestTypes.ENCODER)
		{
			guiContactsEncoder = WebcamManage.GuiContactsEncoder.ENCODER;
			label_video_encoder_feedback.Text = "";

			hbox_video_encoder.Sensitive = true;
			hbox_video_encoder_no_capturing.Visible = true;
			hbox_video_encoder_capturing.Visible = false;
		}

		//button_video_play_this_test.Sensitive = false;
		button_video_play_this_test_sensitive (guiContactsEncoder, false);

		if(! preferences.videoOn || webcamManage == null)
			return;

		Webcam.Result result = webcamManage.RecordEnd (1);
		if(result.success)
		{
			Webcam.Result resultExit = webcamManage.ExitAndFinish (1, currentSession.UniqueID, testType, uniqueID, guiContactsEncoder);
			if(uniqueID != -1 && ! resultExit.success)
				new DialogMessage(Constants.MessageTypes.WARNING, resultExit.error);
		}
		else
			new DialogMessage(Constants.MessageTypes.WARNING, result.error);

		//button_video_play_this_test.Sensitive = result.success;
		button_video_play_this_test_sensitive (guiContactsEncoder, result.success);
	}

	//do this to start them at the "same moment"
	//can pass a -1 uniqueID if test is cancelled
	private void webcamEndTwoCams (Constants.TestTypes testType, int uniqueID)
	{
		WebcamManage.GuiContactsEncoder guiContactsEncoder = WebcamManage.GuiContactsEncoder.CONTACTS;
		if(testType == Constants.TestTypes.ENCODER)
		{
			guiContactsEncoder = WebcamManage.GuiContactsEncoder.ENCODER;
			label_video_encoder_feedback.Text = "";
		}

		//button_video_play_this_test.Sensitive = false;
		button_video_play_this_test_sensitive (guiContactsEncoder, false);

		if(! preferences.videoOn || webcamManage == null)
			return;

		Webcam.Result result1 = webcamManage.RecordEnd (1);
		Webcam.Result result2 = webcamManage.RecordEnd (2);

		string errorMessage = "";
		if(result1.success)
		{
			Webcam.Result result1Exit = webcamManage.ExitAndFinish (1, currentSession.UniqueID, testType, uniqueID, guiContactsEncoder);
			if(uniqueID != -1 && ! result1Exit.success)
				errorMessage += result1Exit.error + "\n";
		}
		else
			errorMessage += result1.error + "\n";

		if(result2.success)
		{
			Webcam.Result result2Exit = webcamManage.ExitAndFinish (2, currentSession.UniqueID, testType, -1 * uniqueID, guiContactsEncoder);
			if(uniqueID != -1 && ! result2Exit.success)
				errorMessage += result2Exit.error + "\n";
		}
		else
			errorMessage += result2.error + "\n";

		if(errorMessage != "")
			new DialogMessage(Constants.MessageTypes.WARNING, errorMessage);

		//button_video_play_this_test.Sensitive = (uniqueID != -1 && errorMessage == "");
		button_video_play_this_test_sensitive (guiContactsEncoder, (uniqueID != -1 && errorMessage == ""));
	}


	/*
	 * videoOn
	 */

	//at what tab of notebook_sup there's the video_capture
	private int video_capture_notebook_sup = 0;

	//changed by user clicking on notebook tabs
	private void on_notebook_sup_switch_page (object o, SwitchPageArgs args) {
		if(
				(notebook_sup.CurrentPage == 0 && video_capture_notebook_sup == 1) ||
				(notebook_sup.CurrentPage == 1 && video_capture_notebook_sup == 0))
		{
			//first stop showing video
			bool wasActive = false;
			if(checkbutton_video.Active) {
				wasActive = true;
				checkbutton_video.Active = false;
			}

			if(notebook_sup.CurrentPage == 0) {
				/*
				 * TODO:
				//remove video capture from encoder tab
				viewport_video_capture_encoder.Remove(capturer);
				//add in contacts tab
				hbox_video_capture.PackStart(capturer, true, true, 0);
				*/
			} else {
				/*
				 * TODO:
				//remove video capture from contacts tab
				hbox_video_capture.Remove(capturer);
				//add in encoder tab

				//switch to capture tab
				radiobutton_video_encoder_capture.Active = true;

				//sometimes it seems is not removed and then cannot be added again
				//just add if not exists
				//maybe this error was because before we were not doing the:
				//radiobutton_video_encoder_capture.Active = true;
				if(viewport_video_capture_encoder.Child == null)
					viewport_video_capture_encoder.Add(capturer);
					*/
			}

			if(wasActive)
				checkbutton_video.Active = true;

			video_capture_notebook_sup = notebook_sup.CurrentPage;
		}
	}

	//CapturerBin capturer;
	private void videoCaptureInitialize()
	{
		/*
		 * TODO: reimplement this with ffmpeg
		 *
		capturer = new CapturerBin();

		hbox_video_capture.PackStart(capturer, true, true, 0);

		videoCapturePrepare(false); //if error, show message
		*/
	}

	//int videoDeviceNum = 0;
	private void videoCapturePrepare(bool showErrorMessage) {
		/*
		 * TODO: reimplement this with ffmpeg
		 *
		LogB.Information("videoCapturePPPPPPPPPPPPPPPPPrepare");
		List<LongoMatch.Video.Utils.Device> devices = LongoMatch.Video.Utils.Device.ListVideoDevices();
		if(devices.Count == 0) {
			if(showErrorMessage)
				new DialogMessage(Constants.MessageTypes.WARNING, Constants.CameraNotFound);
			return;
		}


		CapturePropertiesStruct s = new CapturePropertiesStruct();

		s.OutputFile = Util.GetVideoTempFileName();

		s.VideoBitrate =  1000;
		s.AudioBitrate =  128;
		s.CaptureSourceType = CaptureSourceType.System;
		s.Width = 360;
		s.Height = 288;

		foreach(LongoMatch.Video.Utils.Device dev in devices){
			LogB.Information(dev.ID.ToString());
			LogB.Information(dev.IDProperty.ToString());
			LogB.Information(dev.DeviceType.ToString());
		}

		s.DeviceID = devices[videoDeviceNum].ID;


		capturer.CaptureProperties = s;

		//checkbutton_video and checkbutton_video_encoder are synchronized
		if(checkbutton_video.Active)
			capturer.Type = CapturerType.Live;
		else
			capturer.Type = CapturerType.Fake;
		capturer.Visible=true;

		try {
			capturer.Stop();
		} catch {}
		LogB.Information("videoCapturePPPPPPPPPPPPPPPPPrepare done !");
		capturer.Run();
		*/
	}


	private void changeVideoButtons(bool myVideo)
	{
		image_video_yes.Visible = myVideo;
		image_video_no.Visible = ! myVideo;

		image_video_encoder_yes.Visible = myVideo;
		image_video_encoder_no.Visible = ! myVideo;

		button_video_preview.Visible = myVideo;
		button_video_encoder_preview.Visible = myVideo;
	}

	private void on_checkbutton_video_clicked(object o, EventArgs args)
	{
		if(checkbutton_video.Active)
		{
			if(preferences.videoDevice == "") //on mac can be "0"... || preferences.videoDevice == "0")
			{
				new DialogMessage(Constants.MessageTypes.WARNING, "Video device is not configured. Check Preferences / Multimedia.");
				checkbutton_video.Active = false;
				return;
			}

			preferences.videoOn = true;
			SqlitePreferences.Update("videoOn", "True", false);
		} else {
			preferences.videoOn = false;
			SqlitePreferences.Update("videoOn", "False", false);
		}
		//change encoder checkbox but don't raise the signal
		checkbutton_video_encoder.Clicked -= new EventHandler(on_checkbutton_video_encoder_clicked);
		checkbutton_video_encoder.Active = preferences.videoOn;
		checkbutton_video_encoder.Clicked += new EventHandler(on_checkbutton_video_encoder_clicked);

		changeVideoButtons(preferences.videoOn);

		videoCapturePrepare(true); //if error, show message
	}

	private void on_checkbutton_video_encoder_clicked(object o, EventArgs args)
	{
		if(checkbutton_video_encoder.Active)
		{
			if(preferences.videoDevice == "") //on mac can be "0"... || preferences.videoDevice == "0")
			{
				new DialogMessage(Constants.MessageTypes.WARNING, "Video device is not configured. Check Preferences / Multimedia.");
				checkbutton_video_encoder.Active = false;
				return;
			}

			preferences.videoOn = true;
			SqlitePreferences.Update("videoOn", "True", false);
		} else {
			preferences.videoOn = false;
			SqlitePreferences.Update("videoOn", "False", false);
		}
		//change contacts checkbox but don't raise the signal
		checkbutton_video.Clicked -= new EventHandler(on_checkbutton_video_clicked);
		checkbutton_video.Active = preferences.videoOn;
		checkbutton_video.Clicked += new EventHandler(on_checkbutton_video_clicked);

		changeVideoButtons(preferences.videoOn);

		//will start on record
		videoCapturePrepare(true); //if error, show message
	}

	/* ---------------------------------------------------------
	 * ----------------  EVENTS PLAY VIDEO ---------------------
	 *  --------------------------------------------------------
	 */

	//TODO: manage different playVideo. Playing is very different than capturing, separate it.
	Webcam webcamPlay;

	private void on_button_video_preview_clicked (object o, EventArgs args)
	{
		playPreview();
	}
	private void on_button_video_encoder_preview_clicked (object o, EventArgs args)
	{
		playPreview();
	}
	private void playPreview ()
	{
		//constructor for playpreview
		webcamPlay = new WebcamFfmpeg (Webcam.Action.PLAYPREVIEW, UtilAll.GetOSEnum(), preferences.videoDevice, preferences.videoDeviceResolution, preferences.videoDeviceFramerate);
		Webcam.Result result = webcamPlay.PlayPreviewNoBackground ();
	}

	private void on_button_video_debug_clicked (object o, EventArgs args)
	{
		string executable = "debug";
		if(UtilAll.GetOSEnum() == UtilAll.OperatingSystems.WINDOWS)
			executable = System.IO.Path.Combine(Util.GetPrefixDir(), "bin/debug.bat");

		LogB.Information("Calling debug: " + executable);
		ExecuteProcess.Result execute_result = ExecuteProcess.run (executable, true, true);
		LogB.Information("Called debug.");
	}

	//Not used on encoder
	private void playVideo (string fileName)
	{
		//constructor for playpreview
		webcamPlay = new WebcamFfmpeg (Webcam.Action.PLAYFILE, UtilAll.GetOSEnum(), "", "", "");
		Webcam.Result result = webcamPlay.PlayFile (fileName);

		/*
		 * TODO: reimplement this with ffmpeg
		 *
		if(File.Exists(fileName)) {
			LogB.Information("Play video starting...");
			PlayerBin player = new PlayerBin();
			player.Open(fileName);

			//without these lines works also but has less functionalities (speed, go to ms)
			Gtk.Window d = new Gtk.Window(Catalog.GetString("Playing video"));
			d.Add(player);
			d.Modal = true;
			d.SetDefaultSize(500,400);
			d.ShowAll();
			d.DeleteEvent += delegate(object sender, DeleteEventArgs e) {player.Close(); player.Dispose();};

			if(play) {
				LogB.Information("Play video playing...");
				player.Play();
			}
			return true;
		}
		*/
	}


	private void on_video_play_last_test_clicked (object o, EventArgs args)
	{
		Constants.TestTypes type = Constants.TestTypes.JUMP;
		int id = 0;
		switch (currentEventType.Type) {
			case EventType.Types.JUMP:
				if(lastJumpIsSimple) {
					type = Constants.TestTypes.JUMP;
					id = currentJump.UniqueID;
				}
				else {
					type = Constants.TestTypes.JUMP_RJ;
					id = currentJumpRj.UniqueID;
				} break;
			case EventType.Types.RUN:
				if(lastRunIsSimple) {
					type = Constants.TestTypes.RUN;
					id = currentRun.UniqueID;
				} else {
					type = Constants.TestTypes.RUN_I;
					id = currentRunInterval.UniqueID;
				}
				break;
			case EventType.Types.PULSE:
				type = Constants.TestTypes.PULSE;
				id = currentPulse.UniqueID;
				break;
			case EventType.Types.REACTIONTIME:
				type = Constants.TestTypes.RT;
				id = currentReactionTime.UniqueID;
				break;
			case EventType.Types.MULTICHRONOPIC:
				type = Constants.TestTypes.MULTICHRONOPIC;
				id = currentMultiChronopic.UniqueID;
				break;
		}

		playVideo(Util.GetVideoFileName(currentSession.UniqueID, type, id));
	}

	private void on_video_play_selected_jump_clicked (object o, EventArgs args) {
		if (myTreeViewJumps.EventSelectedID > 0)
			playVideo(Util.GetVideoFileName(currentSession.UniqueID,
						Constants.TestTypes.JUMP,
						myTreeViewJumps.EventSelectedID));
	}

	private void on_video_play_selected_jump_rj_clicked (object o, EventArgs args) {
		if (myTreeViewJumpsRj.EventSelectedID > 0)
			playVideo(Util.GetVideoFileName(currentSession.UniqueID,
						Constants.TestTypes.JUMP_RJ,
						myTreeViewJumpsRj.EventSelectedID));
	}

	private void on_video_play_selected_run_clicked (object o, EventArgs args) {
		if (myTreeViewRuns.EventSelectedID > 0)
			playVideo(Util.GetVideoFileName(currentSession.UniqueID,
						Constants.TestTypes.RUN,
						myTreeViewRuns.EventSelectedID));
	}

	private void on_video_play_selected_run_interval_clicked (object o, EventArgs args) {
		if (myTreeViewRunsInterval.EventSelectedID > 0)
			playVideo(Util.GetVideoFileName(currentSession.UniqueID,
						Constants.TestTypes.RUN_I,
						myTreeViewRunsInterval.EventSelectedID));
	}

	private void on_video_play_selected_reaction_time_clicked (object o, EventArgs args) {
		if (myTreeViewReactionTimes.EventSelectedID > 0)
			playVideo(Util.GetVideoFileName(currentSession.UniqueID,
						Constants.TestTypes.RT,
						myTreeViewReactionTimes.EventSelectedID));
	}

	private void on_video_play_selected_pulse_clicked (object o, EventArgs args) {
		if (myTreeViewPulses.EventSelectedID > 0)
			playVideo(Util.GetVideoFileName(currentSession.UniqueID,
						Constants.TestTypes.PULSE,
						myTreeViewPulses.EventSelectedID));
	}

	private void on_video_play_selected_multi_chronopic_clicked (object o, EventArgs args) {
		if (myTreeViewMultiChronopic.EventSelectedID > 0)
			playVideo(Util.GetVideoFileName(currentSession.UniqueID,
						Constants.TestTypes.MULTICHRONOPIC,
						myTreeViewMultiChronopic.EventSelectedID));
	}

	private void on_button_video_play_this_test_encoder_clicked (object o, EventArgs args)
	{
		playVideo(Util.GetVideoFileName(currentSession.UniqueID,
				Constants.TestTypes.ENCODER, Convert.ToInt32(encoderSignalUniqueID)));
	}

}
