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
 * Copyright (C) 2019-2020   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Gtk;
using Glade;
using System.IO; //"File" things
using System.Diagnostics;  //Stopwatch
using System.Threading;
using Mono.Unix;

public partial class ChronoJumpWindow 
{
	//[Widget] Gtk.Notebook notebook_last_test_buttons; page1: delete, play, inspect, page2: progressbar_video_generating
	[Widget] Gtk.ProgressBar progressbar_video_generating;
	[Widget] Gtk.HBox hbox_contacts_camera;
	[Widget] Gtk.CheckButton checkbutton_video_contacts;
	[Widget] Gtk.CheckButton checkbutton_video_encoder;
	//[Widget] Gtk.HBox hbox_video_contacts;
	[Widget] Gtk.Notebook notebook_video_contacts;
	[Widget] Gtk.HBox hbox_video_contacts_no_capturing;
	[Widget] Gtk.HBox hbox_video_contacts_capturing;
	[Widget] Gtk.HBox hbox_video_encoder;
	[Widget] Gtk.HBox hbox_video_encoder_no_capturing;
	[Widget] Gtk.HBox hbox_video_encoder_capturing;
	[Widget] Gtk.Label label_video_feedback;
	[Widget] Gtk.Label label_video_encoder_feedback;
	[Widget] Gtk.Button button_video_contacts_preview;
	[Widget] Gtk.Button button_video_encoder_preview;
	//[Widget] Gtk.Label label_video;
	[Widget] Gtk.Image image_video_contacts_yes;
	[Widget] Gtk.Image image_video_contacts_yes1;
	[Widget] Gtk.Image image_video_contacts_no;
	[Widget] Gtk.Image image_video_encoder_yes;
	[Widget] Gtk.Image image_video_encoder_yes1;
	[Widget] Gtk.Image image_video_encoder_no;
	[Widget] Gtk.Label label_video_contacts_tests_will_be_filmed;
	[Widget] Gtk.Label label_video_encoder_tests_will_be_filmed;
	[Widget] Gtk.Button button_video_play_this_test_contacts;
	[Widget] Gtk.Button button_video_play_this_test_encoder;
	[Widget] Gtk.ProgressBar pulsebar_webcam;


	private enum WebcamEncoderFileStarted { NEEDTOCHECK, RECORDSTARTED, NOCAMERA }
	private WebcamEncoderFileStarted webcamEncoderFileStarted;
	private WebcamEndParams webcamEndParams;


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

		string errorMessage = "";
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

			if(current_mode == Constants.Modes.FORCESENSOR)
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

	/*
	 * in the past we pass here an string, and an option was ALL
	 * now we use Modes an UNDEFINED will work as ALL
	 */
	private void button_video_play_selected_test(Constants.Modes m)
	{
		if(m == Constants.Modes.JUMPSSIMPLE || m == Constants.Modes.UNDEFINED)
			button_video_play_selected_jump.Sensitive =
				(myTreeViewJumps.EventSelectedID > 0 &&
				 File.Exists(Util.GetVideoFileName(
						 currentSession.UniqueID,
						 Constants.TestTypes.JUMP,
						 myTreeViewJumps.EventSelectedID)));

		if(m == Constants.Modes.JUMPSREACTIVE || m == Constants.Modes.UNDEFINED)
			button_video_play_selected_jump_rj.Sensitive =
				(myTreeViewJumpsRj.EventSelectedID > 0
				 && File.Exists(Util.GetVideoFileName(
						 currentSession.UniqueID,
						 Constants.TestTypes.JUMP_RJ,
						 myTreeViewJumpsRj.EventSelectedID)));

		if(m == Constants.Modes.RUNSSIMPLE || m == Constants.Modes.UNDEFINED)
			button_video_play_selected_run.Sensitive =
				(myTreeViewRuns.EventSelectedID > 0 &&
				 File.Exists(Util.GetVideoFileName(
						 currentSession.UniqueID,
						 Constants.TestTypes.RUN,
						 myTreeViewRuns.EventSelectedID)));

		if(m == Constants.Modes.RUNSINTERVALLIC || m == Constants.Modes.UNDEFINED)
			button_video_play_selected_run_interval.Sensitive =
				(myTreeViewRunsInterval.EventSelectedID > 0
				 && File.Exists(Util.GetVideoFileName(
						 currentSession.UniqueID,
						 Constants.TestTypes.RUN_I,
						 myTreeViewRunsInterval.EventSelectedID)));


		if(m == Constants.Modes.RT || m == Constants.Modes.UNDEFINED)
			button_video_play_selected_reaction_time.Sensitive =
				(myTreeViewReactionTimes.EventSelectedID > 0 &&
				 File.Exists(Util.GetVideoFileName(
						 currentSession.UniqueID,
						 Constants.TestTypes.RT,
						 myTreeViewReactionTimes.EventSelectedID)));

		if(m == Constants.Modes.OTHER || m == Constants.Modes.UNDEFINED)
		{
			button_video_play_selected_pulse.Sensitive =
				(myTreeViewPulses.EventSelectedID > 0 &&
				 File.Exists(Util.GetVideoFileName(
						 currentSession.UniqueID,
						 Constants.TestTypes.PULSE,
						 myTreeViewPulses.EventSelectedID)));

			button_video_play_selected_multi_chronopic.Sensitive =
				(myTreeViewMultiChronopic.EventSelectedID > 0
				 && File.Exists(Util.GetVideoFileName(
						 currentSession.UniqueID,
						 Constants.TestTypes.MULTICHRONOPIC,
						 myTreeViewMultiChronopic.EventSelectedID)));
		}
	}


	//can pass a -1 uniqueID if test is cancelled
	//returns false if not ended (maybe because did not started)
	private bool webcamEnd (Constants.TestTypes testType, int uniqueID)
	{
		//on contacts tests, we have ReallyStarted. No need to stop camera because it is not recording
		if(testType != Constants.TestTypes.ENCODER && ! webcamManage.ReallyStarted)
			return false;

		WebcamManage.GuiContactsEncoder guiContactsEncoder = WebcamManage.GuiContactsEncoder.CONTACTS;
		if(testType == Constants.TestTypes.ENCODER)
		{
			guiContactsEncoder = WebcamManage.GuiContactsEncoder.ENCODER;
			label_video_encoder_feedback.Text = "";

			hbox_video_encoder.Sensitive = true;
			hbox_video_encoder_no_capturing.Visible = true;
			hbox_video_encoder_capturing.Visible = false;
		}

		if(! preferences.videoOn || webcamManage == null)
			return false;

		Webcam.Result result = webcamManage.RecordEnd (1);

		if(! result.success)
		{
			new DialogMessage(Constants.MessageTypes.WARNING, result.error);
			return false;
		}

		webcamEndParams = new WebcamEndParams(1, currentSession.UniqueID, testType, uniqueID, guiContactsEncoder);

		//on encoder do not have a delayed call to not have problems with CopyTempVideo on src/gui/encoder.cs
		//also on encoder exercise ends when movement has really finished
		if(testType == Constants.TestTypes.ENCODER)
		{
			LogB.Information("Encoder, immediate call to webcamEndDo()");
			webcamEndDo();
		} else {
			if(preferences.videoStopAfter == 0)
				webcamEndDo();
			else {
				//call it later to be able to have some video on a short test like a jump.
				LogB.Information(string.Format("Preparing to call webcamEndDo() in {0} s", preferences.videoStopAfter));

				//notebook_last_test_buttons.CurrentPage = 1;
				//hbox_video_contacts_no_capturing.Visible = false;
				notebook_video_contacts.CurrentPage = 2;
				progressbar_video_generating.Text = Catalog.GetString("Ending video");
				//progressbar_video_generating.Visible = true;

				//GLib.Timeout.Add(Convert.ToUInt32(preferences.videoStopAfter * 1000), new GLib.TimeoutHandler(webcamEndDo));
				//do not done the above method because now we call webcamEndDo to update the progressbar, until preferences.videoStopAfter end
				swWebcamStop = new Stopwatch();
				swWebcamStop.Start();
				GLib.Timeout.Add(50, new GLib.TimeoutHandler(webcamEndDo));
			}
		}

		return true; //really ended
	}

	private bool webcamEndDo()
	{
		//note on encoder swWebcamStop is null because the video ends when encoder ends. so do not show the progressbar finishing the video
		if(swWebcamStart != null || swWebcamStop != null)
		{
			if(swWebcamStop.Elapsed.TotalSeconds < preferences.videoStopAfter)
			{
				//progressbar_video_generating.Pulse();
				progressbar_video_generating.Fraction = UtilAll.DivideSafeFraction(swWebcamStop.Elapsed.TotalMilliseconds, preferences.videoStopAfter * 1000);
				return true;
			}

			swWebcamStart.Stop();
			progressbar_video_generating.Fraction = 1;
		}
		LogB.Information("Called webcamEndDo() ending the pulse");
		Webcam.Result resultExit = webcamManage.ExitAndFinish (webcamEndParams.camera, webcamEndParams.sessionID,
				webcamEndParams.testType, webcamEndParams.uniqueID, webcamEndParams.guiContactsEncoder);

		if(webcamEndParams.uniqueID != -1 && ! resultExit.success)
			new DialogMessage(Constants.MessageTypes.WARNING, resultExit.error);

		button_video_contacts_preview_visible (webcamEndParams.guiContactsEncoder, true);
		LogB.Information(string.Format("calling button_video_play_this_test_contacts_sensitive {0}-{1}-{2}",
					webcamEndParams.guiContactsEncoder, webcamManage.ReallyStarted, resultExit.success));
		button_video_play_this_test_contacts_sensitive (webcamEndParams.guiContactsEncoder, webcamManage.ReallyStarted && resultExit.success);
		button_video_play_selected_test(current_mode);

		sensitiveGuiEventDone();
		//notebook_last_test_buttons.CurrentPage = 0;
		//progressbar_video_generating.Visible = false;
		//hbox_video_contacts_no_capturing.Visible = true;
		notebook_video_contacts.CurrentPage = 0;

		return false; //do not call this Timeout routine again
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

			if(notebook_sup.CurrentPage == Convert.ToInt32(notebook_sup_pages.CONTACTS)) {
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

		button_video_contacts_preview.Visible = myVideo;
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
		playPreview();
	}
	private void on_button_video_encoder_preview_clicked (object o, EventArgs args)
	{
		playPreview();
	}
	private void playPreview ()
	{
		//constructor for playpreview
		webcamPlay = new WebcamFfmpeg (Webcam.Action.PLAYPREVIEW, UtilAll.GetOSEnum(), preferences.videoDevice,
				preferences.videoDevicePixelFormat, preferences.videoDeviceResolution, preferences.videoDeviceFramerate);
		Webcam.Result result = webcamPlay.PlayPreviewNoBackground ();
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
	private void playVideo (string fileName)
	{
		//constructor for playpreview
		webcamPlay = new WebcamFfmpeg (Webcam.Action.PLAYFILE, UtilAll.GetOSEnum(), "", "", "", "");
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
	}


	private void on_button_video_play_this_test_contacts_clicked (object o, EventArgs args)
	{
		if(current_mode == Constants.Modes.FORCESENSOR)
		{
			if(currentForceSensor == null || currentForceSensor.UniqueID == -1)
				new DialogMessage(Constants.MessageTypes.WARNING, "Sorry, file not found");
			else
				playVideo(Util.GetVideoFileName(currentSession.UniqueID, Constants.TestTypes.FORCESENSOR, currentForceSensor.UniqueID));

			return;
		}
		else if(current_mode == Constants.Modes.RUNSENCODER)
		{
			if(currentRunEncoder == null || currentRunEncoder.UniqueID == -1)
				new DialogMessage(Constants.MessageTypes.WARNING, "Sorry, file not found");
			else
				playVideo(Util.GetVideoFileName(currentSession.UniqueID, Constants.TestTypes.RACEANALYZER, currentRunEncoder.UniqueID));

			return;
		}

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
