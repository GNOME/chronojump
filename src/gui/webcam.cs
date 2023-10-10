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
 * Copyright (C) 2019-2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Gtk;
//using Glade;
using System.IO; //"File" things
using System.Diagnostics;  //Stopwatch
using System.Collections.Generic; //List<T>
using System.Threading;
using Mono.Unix;

public partial class ChronoJumpWindow 
{
	// at glade ---->
	//Gtk.Notebook notebook_last_test_buttons; page1: delete, play, inspect, page2: progressbar_video_generating
	Gtk.Label label_video_generating;
	Gtk.ProgressBar progressbar_video_generating;
	Gtk.HBox hbox_contacts_camera;
	Gtk.CheckButton checkbutton_video_contacts;
	Gtk.CheckButton checkbutton_video_encoder;
	//Gtk.HBox hbox_video_contacts;
	Gtk.Notebook notebook_video_contacts;
	//Gtk.HBox hbox_video_contacts_no_capturing;
	//Gtk.HBox hbox_video_contacts_capturing;
	Gtk.HBox hbox_video_encoder;
	Gtk.HBox hbox_video_encoder_no_capturing;
	Gtk.HBox hbox_video_encoder_capturing;
	Gtk.Label label_video_feedback;
	Gtk.Label label_video_encoder_feedback;
	Gtk.Button button_video_contacts_preview;
	Gtk.Button button_video_encoder_preview;
	Gtk.Alignment align_video_contacts_preview;
	Gtk.Alignment align_video_encoder_preview;
	//Gtk.Label label_video;
	Gtk.Image image_video_contacts_yes;
	Gtk.Image image_video_contacts_yes1;
	Gtk.Image image_video_contacts_no;
	Gtk.Image image_video_encoder_yes;
	Gtk.Image image_video_encoder_yes1;
	Gtk.Image image_video_encoder_no;
	Gtk.Label label_video_contacts_tests_will_be_filmed;
	Gtk.Label label_video_encoder_tests_will_be_filmed;
	Gtk.Button button_video_play_this_test_contacts;
	Gtk.Button button_video_play_this_test_encoder;
	Gtk.Spinner spinner_video_play_this_test_contacts;
	Gtk.Spinner spinner_video_play_this_test_encoder;
	Gtk.Spinner spinner_video_preview_this_test_contacts;
	Gtk.Spinner spinner_video_preview_this_test_encoder;
	Gtk.ProgressBar pulsebar_webcam;
	// <---- at glade


	private enum WebcamEncoderFileStarted { NEEDTOCHECK, RECORDSTARTED, NOCAMERA }
	private WebcamEncoderFileStarted webcamEncoderFileStarted;
	private WebcamEndParams webcamEndParams;

	private enum WebcamStatusEnum { NOCAMERA, RECORDING, STOPPING, STOPPED, SAVED }
	private WebcamStatusEnum webcamStatusEnum;


	//should be visible on all contacts, but right now hide it on force sensor and runEncoder
	//but we need database stuff first
	public void showWebcamCaptureContactsControls (bool show)
	{
		hbox_contacts_camera.Visible = show;
		button_video_play_this_test_contacts.Visible = show;
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
	 * maybe use on /tmp/chronojump-video0 /tmp/chronojump-video1 …
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

	//private bool webcamStart (WebcamManage.GuiContactsEncoder guiContactsEncoder, int ncams, bool waitUntilRecording)
	private bool webcamStart (WebcamManage.GuiContactsEncoder guiContactsEncoder, int ncams)//, bool waitUntilRecording)
	{
		bool waitUntilRecording = true; //only applies to contacts, right now
		if(guiContactsEncoder == WebcamManage.GuiContactsEncoder.ENCODER)
			waitUntilRecording = false;

		if(guiContactsEncoder == WebcamManage.GuiContactsEncoder.ENCODER)
			hbox_video_encoder.Sensitive = false;

		if(! preferences.videoOn || webcamManage == null)
			return false;

		if(guiContactsEncoder == WebcamManage.GuiContactsEncoder.ENCODER)
		{
			hbox_video_encoder_no_capturing.Visible = false;
			hbox_video_encoder_capturing.Visible = true;
		}

		button_video_contacts_preview_visible (guiContactsEncoder, false);

		//string errorMessage = "";
		if(ncams == 1)
		{
			if(! webcamManage.RecordPrepare(preferences.videoDevice, preferences.videoDevicePixelFormat,
						preferences.videoDeviceResolution, preferences.videoDeviceFramerate).success)
				return false;

			if(! webcamManage.RecordStart(1))
				return false;

			label_video_feedback_text (guiContactsEncoder, Catalog.GetString("Preparing camera"));
		}
		else if(ncams == 2)
		{
			if(! webcamManage.RecordPrepare(preferences.videoDevice, "/dev/video1", preferences.videoDeviceResolution, preferences.videoDeviceFramerate).success)
				return false;

			if(! webcamManage.RecordStart(2))
				return false;

			label_video_feedback_text (guiContactsEncoder, Catalog.GetString("Preparing camera"));
		}

		if(waitUntilRecording)
		{
			//to not allow to click two times on execute test while camera is starting: unsensitive button now
			button_execute_test.Sensitive = false;

			webcamStartThreadBeforeTestStatus = statusEnum.NOT_STARTED;
			webcamStartThread = new Thread (new ThreadStart (webcamStartThreadBeforeTest));
			GLib.Idle.Add (new GLib.IdleHandler (pulseWebcamGTK));
			webcamStartThread.Start();
		}

		return true;
	}

	Thread webcamStartThread; //TODO: remember to stop/kill this on Chronojump exit
	private enum statusEnum { NOT_STARTED, STARTING, FAILURE, SUCCESS };
	static statusEnum webcamStartThreadBeforeTestStatus;
	static Stopwatch swWebcamStart;
	static Stopwatch swWebcamStop;

	private bool webcamStatusEnumSetStart ()
	{
		if (
				(Constants.ModeIsENCODER (current_mode) && checkbutton_video_encoder.Active) ||
				(! Constants.ModeIsENCODER (current_mode) && checkbutton_video_contacts.Active) )
		{
			webcamStatusEnum = WebcamStatusEnum.RECORDING;
			return true;
		} else {
			webcamStatusEnum = WebcamStatusEnum.NOCAMERA;
			return false;
		}
	}

	//Attention: no GTK here
	private void webcamStartThreadBeforeTest()
	{
		bool problems = false;
		swWebcamStart = new Stopwatch();
		swWebcamStart.Start();
		do {
			System.Threading.Thread.Sleep(100);
			if(swWebcamStart.Elapsed.TotalSeconds >= 10)
				problems = true;
		} while(! WebcamManage.RecordingFileStarted() && ! problems);
		swWebcamStart.Stop();

		if(problems) {
			LogB.Information("Problems starting camera.");
			webcamStartThreadBeforeTestStatus = statusEnum.FAILURE;
		} else
			webcamStartThreadBeforeTestStatus = statusEnum.SUCCESS;
	}
	private bool pulseWebcamGTK ()
	{
		if(webcamStartThreadBeforeTestStatus == statusEnum.NOT_STARTED)
		{
			label_video_feedback_text (WebcamManage.GuiContactsEncoder.CONTACTS, Catalog.GetString("Initializing camera."));
			pulsebar_webcam.Visible = true;
			webcamStartThreadBeforeTestStatus = statusEnum.STARTING;
		}

		if ( ! webcamStartThread.IsAlive )
		{
			pulsebar_webcam.Visible = false;
			if(webcamStartThreadBeforeTestStatus == statusEnum.FAILURE)
				label_video_feedback_text (WebcamManage.GuiContactsEncoder.CONTACTS, Catalog.GetString("Problems starting camera."));
			else if(webcamStartThreadBeforeTestStatus == statusEnum.SUCCESS)
			{
				webcamManage.ReallyStarted = true;
				label_video_feedback_text (WebcamManage.GuiContactsEncoder.CONTACTS, Catalog.GetString("Recording …"));
				notebook_video_contacts.CurrentPage = 1;
			}

			if(Constants.ModeIsFORCESENSOR (current_mode))
				forceSensorCapturePre3_GTK_cameraCalled();
			else if(current_mode == Constants.Modes.RUNSENCODER)
				runEncoderCapturePre3_GTK_cameraCalled();
			else
				on_button_execute_test_accepted ();

			LogB.ThreadEnded();
			return false;
		}

		pulsebar_webcam.Fraction = UtilAll.DivideSafeFraction(swWebcamStart.Elapsed.TotalSeconds, 10);
		Thread.Sleep (50);
		//LogB.Debug(webcamStartThread.ThreadState.ToString());
		return true;
	}

	private void button_video_contacts_preview_visible (WebcamManage.GuiContactsEncoder guiContactsEncoder, bool visible)
	{
		if(guiContactsEncoder == WebcamManage.GuiContactsEncoder.CONTACTS)
			button_video_contacts_preview.Visible = visible;
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
	private void button_video_play_this_test_contacts_sensitive (WebcamManage.GuiContactsEncoder guiContactsEncoder, bool s)
	{
		LogB.Information("button_video_play_this_test_contacts_sensitive: " + s.ToString());
		if(guiContactsEncoder == WebcamManage.GuiContactsEncoder.CONTACTS)
			button_video_play_this_test_contacts.Sensitive = s;
		else
			//button_video_encoder_play_this_test_contacts.Sensitive = s;
			button_video_play_this_test_encoder.Sensitive = s; //TODO:jugar amb la sensitivitat de aixo quan hi ha o no signalUniqueID 
	}

	private void button_video_play_selected_test (Constants.Modes m)
	{
		if (m == Constants.Modes.JUMPSSIMPLE)
			button_video_play_this_test_contacts.Sensitive =
				(myTreeViewJumps.EventSelectedID > 0 &&
				 File.Exists(Util.GetVideoFileName(
						 currentSession.UniqueID,
						 Constants.TestTypes.JUMP,
						 myTreeViewJumps.EventSelectedID)));

		else if (m == Constants.Modes.JUMPSREACTIVE)
			button_video_play_this_test_contacts.Sensitive =
				(myTreeViewJumpsRj.EventSelectedID > 0
				 && File.Exists(Util.GetVideoFileName(
						 currentSession.UniqueID,
						 Constants.TestTypes.JUMP_RJ,
						 myTreeViewJumpsRj.EventSelectedID)));

		else if (m == Constants.Modes.RUNSSIMPLE)
			button_video_play_this_test_contacts.Sensitive =
				(myTreeViewRuns.EventSelectedID > 0 &&
				 File.Exists(Util.GetVideoFileName(
						 currentSession.UniqueID,
						 Constants.TestTypes.RUN,
						 myTreeViewRuns.EventSelectedID)));

		else if (m == Constants.Modes.RUNSINTERVALLIC)
			button_video_play_this_test_contacts.Sensitive =
				(myTreeViewRunsInterval.EventSelectedID > 0
				 && File.Exists(Util.GetVideoFileName(
						 currentSession.UniqueID,
						 Constants.TestTypes.RUN_I,
						 myTreeViewRunsInterval.EventSelectedID)));

		/* unused
		else if (m == Constants.Modes.RT)
			button_video_play_this_test_contacts.Sensitive =
				(myTreeViewReactionTimes.EventSelectedID > 0 &&
				 File.Exists(Util.GetVideoFileName(
						 currentSession.UniqueID,
						 Constants.TestTypes.RT,
						 myTreeViewReactionTimes.EventSelectedID)));

		else if (m == Constants.Modes.OTHER)
		{
			button_video_play_this_test_contacts.Sensitive =
				(myTreeViewPulses.EventSelectedID > 0 &&
				 File.Exists(Util.GetVideoFileName(
						 currentSession.UniqueID,
						 Constants.TestTypes.PULSE,
						 myTreeViewPulses.EventSelectedID)));

			button_video_play_this_test_contacts.Sensitive =
				(myTreeViewMultiChronopic.EventSelectedID > 0
				 && File.Exists(Util.GetVideoFileName(
						 currentSession.UniqueID,
						 Constants.TestTypes.MULTICHRONOPIC,
						 myTreeViewMultiChronopic.EventSelectedID)));
		}
		*/
	}

	public void webcamEncoderEnd ()
	{
		if (webcamEncoderFileStarted == WebcamEncoderFileStarted.RECORDSTARTED)
		{
			webcamManage.RecordingStop ();
			webcamStatusEnum = WebcamStatusEnum.STOPPED;
		}

		webcamRestoreGui (! encoderProcessCancel);
	}

	private void webcamEndingRecordingCancel ()
	{
		if(! preferences.videoOn || webcamManage == null)
			return;

		webcamManage.RecordingStop ();
		webcamStatusEnum = WebcamStatusEnum.STOPPED;
	}

	//can pass a -1 uniqueID if test is cancelled
	//returns false if not ended (maybe because did not started)
	private bool webcamEndingRecordingStop ()
	{
		//on contacts tests, we have ReallyStarted. No need to stop camera because it is not recording
		if(! Constants.ModeIsENCODER (current_mode) && ! webcamManage.ReallyStarted)
			return false;

		if(! preferences.videoOn || webcamManage == null)
			return false;

		/* unused on ffmpeg
		Webcam.Result result = webcamManage.RecordEnd (1);

		if(! result.success)
		{
			new DialogMessage(Constants.MessageTypes.WARNING, result.error);
			return false;
		}
		*/

		webcamStatusEnum = WebcamStatusEnum.STOPPING;

		//on encoder do not have a delayed call to not have problems with CopyTempVideo on src/gui/encoder.cs
		//also on encoder exercise ends when movement has really finished
		if (! Constants.ModeIsENCODER (current_mode) && preferences.videoStopAfter > 0)
		{
			//call it later to be able to have some video on a short test like a jump.
			//LogB.Information(string.Format("Preparing to call webcamEndDo() in {0} s", preferences.videoStopAfter));

			//notebook_last_test_buttons.CurrentPage = 1;
			//hbox_video_contacts_no_capturing.Visible = false;
			notebook_video_contacts.CurrentPage = 2;
			label_video_generating.Text = Catalog.GetString("Ending video");
			//progressbar_video_generating.Visible = true;

			swWebcamStop = new Stopwatch();
			swWebcamStop.Start();
		}

		return true;
	}

	//this bool means: has stopped. Do not call me again
	private bool webcamEndingRecordingStopDo ()
	{
		//stop camera
		if(swWebcamStart != null || swWebcamStop != null)
		{
			if(swWebcamStop.Elapsed.TotalSeconds < preferences.videoStopAfter)
			{
				//progressbar_video_generating.Pulse();
				progressbar_video_generating.Fraction = UtilAll.DivideSafeFraction (
						swWebcamStop.Elapsed.TotalMilliseconds, preferences.videoStopAfter * 1000);
				Thread.Sleep (50);
				return false;
			}

			swWebcamStop.Stop();
			progressbar_video_generating.Fraction = 1;

			//LogB.Information("Called webcamEndingRecordingStopDo () ending the pulse");
			webcamManage.RecordingStop ();
			webcamStatusEnum = WebcamStatusEnum.STOPPED;
		}

		return true;
	}

	private WebcamManage.GuiContactsEncoder getGuiContactsEncoder ()
	{
		if (Constants.ModeIsENCODER (current_mode))
			return WebcamManage.GuiContactsEncoder.ENCODER;
		else
			return WebcamManage.GuiContactsEncoder.CONTACTS;
	}

	public string EventEndedSaveVideoFile (Constants.TestTypes testType, int uniqueID)
	{
		bool savedVideo = webcamEndingSaveFile (testType, uniqueID);
		webcamRestoreGui (savedVideo);

		if (savedVideo)
			return string.Format ("{0}-{1}", testType, uniqueID); //no need the extension here
		else
			return "";
	}

	public bool webcamEndingSaveFile (Constants.TestTypes testType, int uniqueID)
	{
		webcamEndParams = new WebcamEndParams (1, currentSession.UniqueID, testType, uniqueID, getGuiContactsEncoder ());

		LogB.Information ("pre SaveFile");
		Webcam.Result resultExit = webcamManage.SaveFile (webcamEndParams.camera, webcamEndParams.sessionID,
				webcamEndParams.testType, webcamEndParams.uniqueID, webcamEndParams.guiContactsEncoder);
		LogB.Information ("post SaveFile");

		if(webcamEndParams.uniqueID != -1 && ! resultExit.success)
			new DialogMessage(Constants.MessageTypes.WARNING, resultExit.error);

		button_video_contacts_preview_visible (webcamEndParams.guiContactsEncoder, true);
		LogB.Information(string.Format("calling button_video_play_this_test_contacts_sensitive {0}-{1}-{2}",
					webcamEndParams.guiContactsEncoder, webcamManage.ReallyStarted, resultExit.success));
		webcamStatusEnum = WebcamStatusEnum.SAVED;

		return resultExit.success;
	}

	public void webcamRestoreGui (bool saved)
	{
		if (Constants.ModeIsENCODER (current_mode))
		{
			label_video_encoder_feedback.Text = "";
			hbox_video_encoder.Sensitive = true;
			hbox_video_encoder_no_capturing.Visible = true;
			hbox_video_encoder_capturing.Visible = false;
		} else {
			label_video_feedback.Text = "";
			button_video_play_this_test_contacts_sensitive (
					webcamEndParams.guiContactsEncoder, webcamManage.ReallyStarted && saved);

			button_video_play_selected_test (current_mode);

			//notebook_last_test_buttons.CurrentPage = 0;
			//progressbar_video_generating.Visible = false;
			//hbox_video_contacts_no_capturing.Visible = true;
			notebook_video_contacts.CurrentPage = 0;
		}

		sensitiveGuiEventDone();
	}

	//to be able to pass data to webcamEndDo
	public struct WebcamEndParams
	{
		public int camera;
		public int sessionID;
		public Constants.TestTypes testType;
		public int uniqueID;
		public WebcamManage.GuiContactsEncoder guiContactsEncoder;

		public WebcamEndParams (int camera, int sessionID, Constants.TestTypes testType, int uniqueID, WebcamManage.GuiContactsEncoder guiContactsEncoder)
		{
			this.camera = camera;
			this.sessionID = sessionID;
			this.testType = testType;
			this.uniqueID = uniqueID;
			this.guiContactsEncoder = guiContactsEncoder;
		}
	}

	/*
	 * Unused right now.
	 * if use again in the future, note ExitAndFinish now is RecordingStop and the SaveFile
	 *
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

		//button_video_play_this_test_contacts.Sensitive = false;
		//button_video_play_this_test_contacts_sensitive (guiContactsEncoder, false);

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

		//button_video_play_this_test_contacts.Sensitive = (uniqueID != -1 && errorMessage == "");
		button_video_play_this_test_contacts_sensitive (guiContactsEncoder, (uniqueID != -1 && errorMessage == ""));
		button_video_play_selected_test(current_mode);
	}
	*/

	private void on_button_camera_stop_at_boot_clicked (object o, EventArgs args)
	{
		if(ExecuteProcess.KillExternalProcess (WebcamFfmpeg.GetExecutableCapture(UtilAll.GetOSEnum())))
			hbox_message_camera_at_boot.Visible = false;
	}

	/*
	 * videoOn
	 */

	//at what tab of notebook_sup there's the video_capture
	private int video_capture_notebook_sup = Convert.ToInt32(notebook_sup_pages.CONTACTS);

	//changed by user clicking on notebook tabs
	private void on_notebook_sup_switch_page (object o, SwitchPageArgs args) {
		if(
				(notebook_sup.CurrentPage == Convert.ToInt32(notebook_sup_pages.CONTACTS) &&
				 video_capture_notebook_sup == Convert.ToInt32(notebook_sup_pages.ENCODER))
				||
				(notebook_sup.CurrentPage == Convert.ToInt32(notebook_sup_pages.ENCODER) &&
				 video_capture_notebook_sup == Convert.ToInt32(notebook_sup_pages.CONTACTS)))
		{
			//first stop showing video
			bool wasActive = false;
			if(checkbutton_video_contacts.Active) {
				wasActive = true;
				checkbutton_video_contacts.Active = false;
			}

			if(wasActive)
				checkbutton_video_contacts.Active = true;

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

		//checkbutton_video_contacts and checkbutton_video_encoder are synchronized
		if(checkbutton_video_contacts.Active)
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
		image_video_contacts_yes.Visible = myVideo;
		image_video_contacts_no.Visible = ! myVideo;

		image_video_encoder_yes.Visible = myVideo;
		image_video_encoder_no.Visible = ! myVideo;

		align_video_contacts_preview.Visible = myVideo;
		button_video_contacts_preview.Visible = myVideo;

		align_video_encoder_preview.Visible = myVideo;
		button_video_encoder_preview.Visible = myVideo;
	}

	private void on_checkbutton_video_contacts_clicked(object o, EventArgs args)
	{
		if(checkbutton_video_contacts.Active)
		{
			if(! preferences.IsVideoConfigured())
			{
				new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Video device is not configured. Check Preferences / Multimedia."));
				checkbutton_video_contacts.Active = false;
				return;
			}

			preferences.videoOn = true;
			SqlitePreferences.Update("videoOn", "True", false);

			//this allows to see the label during 500 ms
			//hbox_video_contacts_no_capturing.Visible = false;
			notebook_video_contacts.CurrentPage = 1;
			label_video_contacts_tests_will_be_filmed.Visible = true;
			GLib.Timeout.Add(1000, new GLib.TimeoutHandler(checkbutton_video_contacts_active_end));
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
			if(! preferences.IsVideoConfigured())
			{
				new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Video device is not configured. Check Preferences / Multimedia."));
				checkbutton_video_encoder.Active = false;
				return;
			}

			preferences.videoOn = true;
			SqlitePreferences.Update("videoOn", "True", false);

			//this allows to see the label during 500 ms
			hbox_video_encoder_no_capturing.Visible = false;
			label_video_encoder_tests_will_be_filmed.Visible = true;
			GLib.Timeout.Add(1000, new GLib.TimeoutHandler(checkbutton_video_encoder_active_end));
		} else {
			preferences.videoOn = false;
			SqlitePreferences.Update("videoOn", "False", false);
		}
		//change contacts checkbox but don't raise the signal
		checkbutton_video_contacts.Clicked -= new EventHandler(on_checkbutton_video_contacts_clicked);
		checkbutton_video_contacts.Active = preferences.videoOn;
		checkbutton_video_contacts.Clicked += new EventHandler(on_checkbutton_video_contacts_clicked);

		changeVideoButtons(preferences.videoOn);

		//will start on record
		videoCapturePrepare(true); //if error, show message
	}

	private bool checkbutton_video_contacts_active_end()
	{
		//hbox_video_contacts_no_capturing.Visible = true;
		notebook_video_contacts.CurrentPage = 0;
		label_video_contacts_tests_will_be_filmed.Visible = false;

		return false; //do not call this again
	}

	private bool checkbutton_video_encoder_active_end()
	{
		hbox_video_encoder_no_capturing.Visible = true;
		label_video_encoder_tests_will_be_filmed.Visible = false;

		return false; //do not call this again
	}

	/* ---------------------------------------------------------
	 * ----------------  EVENTS PLAY VIDEO ---------------------
	 *  --------------------------------------------------------
	 */

	//TODO: manage different playVideo. Playing is very different than capturing, separate it.
	Webcam webcamPlay;

	private void on_button_video_contacts_preview_clicked (object o, EventArgs args)
	{
		if (webcamPlay != null && webcamPlayThread != null && webcamPlayThread.IsAlive)
			return;

		//widgets changes
		button_video_contacts_preview.Visible = false;
		spinner_video_preview_this_test_contacts.Visible = true;
		spinner_video_preview_this_test_contacts.Start ();

		webcamPlayThread = new Thread (new ThreadStart (playPreview));
		GLib.Idle.Add (new GLib.IdleHandler (pulseWebcamPreviewGTK));
		webcamPlayThread.Start();
		//playPreview();
	}

	private void on_button_video_encoder_preview_clicked (object o, EventArgs args)
	{
		if (webcamPlay != null && webcamPlayThread != null && webcamPlayThread.IsAlive)
			return;

		//widgets changes
		button_video_encoder_preview.Visible = false;
		spinner_video_preview_this_test_encoder.Visible = true;
		spinner_video_preview_this_test_encoder.Start ();

		webcamPlayThread = new Thread (new ThreadStart (playPreview));
		GLib.Idle.Add (new GLib.IdleHandler (pulseWebcamPreviewGTK));
		webcamPlayThread.Start();
		//playPreview();
	}
	private void playPreview ()
	{
		//constructor for playpreview
		webcamPlay = new WebcamFfmpeg (Webcam.Action.PLAYPREVIEW, UtilAll.GetOSEnum(), preferences.videoDevice,
				preferences.videoDevicePixelFormat, preferences.videoDeviceResolution, preferences.videoDeviceFramerate);
		//Webcam.Result result = webcamPlay.PlayPreviewNoBackground ();
		webcamPlay.PlayPreviewNoBackground ();
	}

	/*
	private void on_button_video_debug_clicked (object o, EventArgs args)
	{
		string executable = "debug";
		if(UtilAll.GetOSEnum() == UtilAll.OperatingSystems.WINDOWS)
			executable = System.IO.Path.Combine(Util.GetPrefixDir(), "bin/debug.bat");

		LogB.Information("Calling debug: " + executable);
		ExecuteProcess.Result execute_result = ExecuteProcess.run (executable, true, true);
		LogB.Information("Called debug.");
	}
	*/

	//Not used on encoder
	private bool playVideo (string fileName)
	{
		//constructor for playpreview
		webcamPlay = new WebcamFfmpeg (Webcam.Action.PLAYFILE, UtilAll.GetOSEnum(), "", "", "", "");
		//Webcam.Result result = webcamPlay.PlayFile (fileName);
		Webcam.Result result = webcamPlay.PlayFile (fileName);

		/*
		 * TODO: reimplement this with ffmpeg
		 *
		if(File.Exists(fileName)) {
			LogB.Information("Play video starting …");
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
				LogB.Information("Play video playing …");
				player.Play();
			}
			return true;
		}
		*/
		return result.success;
	}

	Thread webcamPlayThread;
	private double diffVideoVsSignal;
	private double videoFrames;

	private void on_button_video_play_this_test_contacts_clicked (object o, EventArgs args)
	{
		if (webcamPlay != null && webcamPlayThread != null && webcamPlayThread.IsAlive)
			return;

		if (Constants.ModeIsFORCESENSOR (current_mode) &&
				(currentForceSensor == null || currentForceSensor.UniqueID == -1))
		{
			new DialogMessage(Constants.MessageTypes.WARNING, "Sorry, file not found");
			return;
		}
		else if (current_mode == Constants.Modes.RUNSENCODER &&
				(currentRunEncoder == null || currentRunEncoder.UniqueID == -1))
		{
			new DialogMessage(Constants.MessageTypes.WARNING, "Sorry, file not found");
			return;
		}

		// widgets changes
		checkbutton_video_contacts.Visible = false;
		button_video_play_this_test_contacts.Visible = false;
		spinner_video_play_this_test_contacts.Visible = true;
		spinner_video_play_this_test_contacts.Start ();

		webcamPlayThread = new Thread (new ThreadStart (webcamPlayThreadDo));
		GLib.Idle.Add (new GLib.IdleHandler (pulseWebcamPlayGTK));
		webcamPlayThread.Start();
	}

	private void on_button_video_play_this_test_encoder_clicked (object o, EventArgs args)
	{
		if (webcamPlay != null && webcamPlayThread != null && webcamPlayThread.IsAlive)
			return;

		// widgets changes
		checkbutton_video_encoder.Visible = false;
		button_video_play_this_test_encoder.Visible = false;
		spinner_video_play_this_test_encoder.Visible = true;
		spinner_video_play_this_test_encoder.Start ();

		if(encoderConfigurationCurrent.has_inertia)
			eCapture = new EncoderCaptureInertial();
		else
			eCapture = new EncoderCaptureGravitatory();

		cairoGraphEncoderSignal = null;
		cairoGraphEncoderSignalPoints_l = new List<PointF>();
		cairoGraphEncoderSignalInertialPoints_l = new List<PointF>();

		eCapture.LoadFromFile (encoderConfigurationCurrent.has_inertia, preferences.signalDirectionHorizontal);
		eCapture.PointsPainted = -1;
		if(encoderConfigurationCurrent.has_inertia) {
			updateEncoderCaptureGraphPaintData (UpdateEncoderPaintModes.INERTIAL);
			//updateEncoderCaptureSignalCairo (true, false); //inertial, forceRedraw
		} else {
			updateEncoderCaptureGraphPaintData (UpdateEncoderPaintModes.GRAVITATORY);
			//updateEncoderCaptureSignalCairo (false, false);
		}
		//eCapture.PointsPainted = 0;
		//encoder_capture_signal_drawingarea_cairo.QueueDraw (); //aixo no hauria de caldre aqui pq ja es deu fer al thread de sota

		// show the signal realtime cairo graph (not the R generated)
		notebook_encoder_capture.CurrentPage = 0; //TODO: return to show the Page 1 at end

		webcamPlayThread = new Thread (new ThreadStart (webcamPlayThreadDo));
		GLib.Idle.Add (new GLib.IdleHandler (pulseWebcamPlayGTK));
		webcamPlayThread.Start();
	}

	private void webcamPlayThreadDo ()
	{
		// 1) get signal total time (s)
		double signalTotalTime = signalTotalTimeCalculate ();

		// 2) get video duration
		int sessionID = currentSession.UniqueID;
		Constants.TestTypes testType = Constants.TestTypes.FORCESENSOR;
		int id = -1;

		if (Constants.ModeIsFORCESENSOR (current_mode))
		{
			testType = Constants.TestTypes.FORCESENSOR;
			id = currentForceSensor.UniqueID;
		}
		else if(Constants.ModeIsENCODER (current_mode))
		{
			testType = Constants.TestTypes.ENCODER;
			id = Convert.ToInt32 (encoderSignalUniqueID);
		}
		else if(current_mode == Constants.Modes.RUNSENCODER)
		{
			testType = Constants.TestTypes.RACEANALYZER;
			id = currentRunEncoder.UniqueID;
		}
		else if (current_mode == Constants.Modes.JUMPSSIMPLE) {
			testType = Constants.TestTypes.JUMP;
			id = myTreeViewJumps.EventSelectedID;
		}
		else if (current_mode == Constants.Modes.JUMPSREACTIVE) {
			testType = Constants.TestTypes.JUMP_RJ;
			id = myTreeViewJumpsRj.EventSelectedID;
		}
		else if (current_mode == Constants.Modes.RUNSSIMPLE) {
			testType = Constants.TestTypes.RUN;
			id = myTreeViewRuns.EventSelectedID;
		}
		else if (current_mode == Constants.Modes.RUNSINTERVALLIC) {
			testType = Constants.TestTypes.RUN_I;
			id = myTreeViewRunsInterval.EventSelectedID;
		}

		if (id < 0)
			return;

		webcamPlay = new WebcamFfmpeg (Webcam.Action.PLAYFILE, UtilAll.GetOSEnum(), "", "", "", "");
		double videoDuration = webcamPlay.FindVideoDuration (Util.GetVideoFileName (sessionID, testType, id));

		// 3) get diff video vs signal
		if (videoDuration < 0)
		{
			new DialogMessage (Constants.MessageTypes.WARNING, Webcam.ProgramFfprobeNotInstalled);
			diffVideoVsSignal = 0;
		} else {
			if (Constants.ModeIsENCODER (current_mode)) //encoder video capture ends at signal end
				diffVideoVsSignal = videoDuration -signalTotalTime;
			else
				diffVideoVsSignal = videoDuration -preferences.videoStopAfter -signalTotalTime;

			LogB.Information (string.Format ("signalTotalTime: {0}, videoDuration: {1}, diffVideoVsSignal: {2}",
						signalTotalTime, videoDuration, diffVideoVsSignal));
		}

		//unused right now
		//videoFrames = webcamPlay.FindVideoFrames (Util.GetVideoFileName (sessionID, testType, id));
		//LogB.Information ("videoFrames", videoFrames);

		// 4) play video
		if (playVideo (Util.GetVideoFileName (sessionID, testType, id)))
			do {
			} while (webcamPlay != null && webcamPlay.PlayVideoGetSecond >= 0);
	}

	private double signalTotalTimeCalculate ()
	{
		double signalTotalTime = 0;
		if (Constants.ModeIsFORCESENSOR (current_mode))
		{
			//using spCairoFE instead of fsAI_AB to avoid problems with zoom
			if (spCairoFE != null && spCairoFE.Force_l != null && spCairoFE.Force_l.Count > 0)
				signalTotalTime = UtilAll.DivideSafe (PointF.Last (spCairoFE.Force_l).X, 1000000);
		}
		else if (Constants.ModeIsENCODER (current_mode))
		{
			signalTotalTime = UtilAll.DivideSafe (cairoGraphEncoderSignalPoints_l.Count, 1000); // 1 KHz
		}
		else if(current_mode == Constants.Modes.RUNSENCODER)
		{
			//using cairoGraphRaceAnalyzerPoints_st_l instead of raAI_AB.P_l to avoid problems with zoom
			if (cairoGraphRaceAnalyzerPoints_st_l != null && cairoGraphRaceAnalyzerPoints_st_l.Count > 0)
				signalTotalTime = PointF.Last (cairoGraphRaceAnalyzerPoints_st_l).X
					- cairoGraphRaceAnalyzerPoints_st_l[0].X; //consider that the beginning use to be negative part (until a >= 10)
		}
		else if (current_mode == Constants.Modes.JUMPSREACTIVE && selectedJumpRj != null)
			signalTotalTime = selectedJumpRj.TvSum + selectedJumpRj.TcSumCaringForStartIn;
		else if (current_mode == Constants.Modes.RUNSINTERVALLIC && selectedRunInterval != null)
			signalTotalTime = selectedRunInterval.TimeTotal;

		// LogB.Information ("signalTotalTime", signalTotalTime);
		return signalTotalTime;
	}

	private bool pulseWebcamPreviewGTK ()
	{
		if (! webcamPlayThread.IsAlive)
		{
			//widgets changes
			if (Constants.ModeIsENCODER (current_mode)) {
				button_video_encoder_preview.Visible = true;
				spinner_video_preview_this_test_encoder.Stop ();
				spinner_video_preview_this_test_encoder.Visible = false;
			} else {
				button_video_contacts_preview.Visible = true;
				spinner_video_preview_this_test_contacts.Stop ();
				spinner_video_preview_this_test_contacts.Visible = false;
			}

			return false;
		}

		Thread.Sleep (10);
		//LogB.Debug(webcamPlayThread.ThreadState.ToString());
		return true;
	}

	private bool pulseWebcamPlayGTK ()
	{
		if (! webcamPlayThread.IsAlive)
		{
			if (Constants.ModeIsENCODER (current_mode)) {
				checkbutton_video_encoder.Visible = true;
				button_video_play_this_test_encoder.Visible = true;
				spinner_video_play_this_test_encoder.Stop ();
				spinner_video_play_this_test_encoder.Visible = false;
			} else {
				checkbutton_video_contacts.Visible = true;
				button_video_play_this_test_contacts.Visible = true;
				spinner_video_play_this_test_contacts.Stop ();
				spinner_video_play_this_test_contacts.Visible = false;
			}

			return false;
		}

		if (webcamPlay != null && webcamPlay.PlayVideoGetSecond > 0)
		{
			if (Constants.ModeIsENCODER (current_mode)) {
				spinner_video_play_this_test_encoder.Stop ();
				spinner_video_play_this_test_encoder.Visible = false;
			} else {
				spinner_video_play_this_test_contacts.Stop ();
				spinner_video_play_this_test_contacts.Visible = false;
			}

			/*
			event_execute_label_message.Text = string.Format ("video s: {0} force s: {1}",
					webcamPlay.PlayVideoGetSecond,
					webcamPlay.PlayVideoGetSecond - diffVideoVsSignal);
			*/

			if (Constants.ModeIsFORCESENSOR (current_mode))
				force_capture_drawingarea_cairo.QueueDraw ();
			else if(current_mode == Constants.Modes.RUNSENCODER)
				drawingarea_race_analyzer_capture_speed_time.QueueDraw ();
			else if (Constants.ModeIsENCODER (current_mode))
			{
				encoder_capture_curves_bars_drawingarea_cairo.QueueDraw ();
				encoder_capture_signal_drawingarea_cairo.QueueDraw ();
			}
			else if (current_mode == Constants.Modes.JUMPSREACTIVE ||
					current_mode == Constants.Modes.RUNSINTERVALLIC)
				event_execute_drawingarea_realtime_capture_cairo.QueueDraw ();

			spinner_video_play_this_test_contacts.Visible = true;
		}

		Thread.Sleep (10);
		//LogB.Debug(webcamPlayThread.ThreadState.ToString());
		return true;
	}

	private void connectWidgetsWebcam (Gtk.Builder builder)
	{
		//notebook_last_test_buttons = (Gtk.Notebook) builder.GetObject ("notebook_last_test_buttons"); page1: delete, play, inspect, page2: progressbar_video_generating
		label_video_generating = (Gtk.Label) builder.GetObject ("label_video_generating");
		progressbar_video_generating = (Gtk.ProgressBar) builder.GetObject ("progressbar_video_generating");
		hbox_contacts_camera = (Gtk.HBox) builder.GetObject ("hbox_contacts_camera");
		checkbutton_video_contacts = (Gtk.CheckButton) builder.GetObject ("checkbutton_video_contacts");
		checkbutton_video_encoder = (Gtk.CheckButton) builder.GetObject ("checkbutton_video_encoder");
		//hbox_video_contacts = (Gtk.HBox) builder.GetObject ("hbox_video_contacts");
		notebook_video_contacts = (Gtk.Notebook) builder.GetObject ("notebook_video_contacts");
		//hbox_video_contacts_no_capturing = (Gtk.HBox) builder.GetObject ("hbox_video_contacts_no_capturing");
		//hbox_video_contacts_capturing = (Gtk.HBox) builder.GetObject ("hbox_video_contacts_capturing");
		hbox_video_encoder = (Gtk.HBox) builder.GetObject ("hbox_video_encoder");
		hbox_video_encoder_no_capturing = (Gtk.HBox) builder.GetObject ("hbox_video_encoder_no_capturing");
		hbox_video_encoder_capturing = (Gtk.HBox) builder.GetObject ("hbox_video_encoder_capturing");
		label_video_feedback = (Gtk.Label) builder.GetObject ("label_video_feedback");
		label_video_encoder_feedback = (Gtk.Label) builder.GetObject ("label_video_encoder_feedback");
		button_video_contacts_preview = (Gtk.Button) builder.GetObject ("button_video_contacts_preview");
		button_video_encoder_preview = (Gtk.Button) builder.GetObject ("button_video_encoder_preview");
		align_video_contacts_preview = (Gtk.Alignment) builder.GetObject ("align_video_contacts_preview");
		align_video_encoder_preview = (Gtk.Alignment) builder.GetObject ("align_video_encoder_preview");
		//label_video = (Gtk.Label) builder.GetObject ("label_video");
		image_video_contacts_yes = (Gtk.Image) builder.GetObject ("image_video_contacts_yes");
		image_video_contacts_yes1 = (Gtk.Image) builder.GetObject ("image_video_contacts_yes1");
		image_video_contacts_no = (Gtk.Image) builder.GetObject ("image_video_contacts_no");
		image_video_encoder_yes = (Gtk.Image) builder.GetObject ("image_video_encoder_yes");
		image_video_encoder_yes1 = (Gtk.Image) builder.GetObject ("image_video_encoder_yes1");
		image_video_encoder_no = (Gtk.Image) builder.GetObject ("image_video_encoder_no");
		label_video_contacts_tests_will_be_filmed = (Gtk.Label) builder.GetObject ("label_video_contacts_tests_will_be_filmed");
		label_video_encoder_tests_will_be_filmed = (Gtk.Label) builder.GetObject ("label_video_encoder_tests_will_be_filmed");
		button_video_play_this_test_contacts = (Gtk.Button) builder.GetObject ("button_video_play_this_test_contacts");
		button_video_play_this_test_encoder = (Gtk.Button) builder.GetObject ("button_video_play_this_test_encoder");
		spinner_video_play_this_test_contacts = (Gtk.Spinner) builder.GetObject ("spinner_video_play_this_test_contacts");
		spinner_video_play_this_test_encoder = (Gtk.Spinner) builder.GetObject ("spinner_video_play_this_test_encoder");
		spinner_video_preview_this_test_contacts = (Gtk.Spinner) builder.GetObject ("spinner_video_preview_this_test_contacts");
		spinner_video_preview_this_test_encoder = (Gtk.Spinner) builder.GetObject ("spinner_video_preview_this_test_encoder");
		pulsebar_webcam = (Gtk.ProgressBar) builder.GetObject ("pulsebar_webcam");
	}
}
