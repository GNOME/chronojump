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
 * Copyright (C) 2004-2026   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Gtk;
using System.Collections.Generic; //List<T>
using System.Threading;
using Mono.Unix;
using System.Diagnostics; 	//for detect OS and for Process, and for Stopwatch


public partial class ChronoJumpWindow 
{
	Thread encoderThread;
	Thread encoderThreadBG;

	/* 
	 * ---- encoderThreadStart ---->
	 */

	private void encoderThreadStart (encoderActions action)
	{
		encoderProcessCancel = false;

		if(action == encoderActions.CAPTURE_BG)
		{
			encoderThreadStart_CAPTUREBG ();
			return;
		}

		else if(action == encoderActions.CAPTURE || action == encoderActions.CAPTURE_IM)
		{
			//event_execute_label_message.Text = Catalog.GetString("Please, wait.");
			LogB.Information("encoderThreadStart begins");
				
			if(action == encoderActions.CAPTURE) {
				runEncoderCaptureNoRDotNetInitialize();
			}

			//don't need to be false because ItemToggled is deactivated during capture
			treeview_encoder_capture_curves.Sensitive = true;

			/*
			//on continuous mode do not erase bars at beginning of capture in order to see last bars
			if(action == encoderActions.CAPTURE && preferences.encoderCaptureInfinite) {
				if(encoderGraphDoPlot != null)
					encoderGraphDoPlot.ShowMessage("Previous set", true, false);

				cairoPaintBarsPreCurrent.ShowMessage (Catalog.GetString("Previous set"), true, false);
			}
			if want to show this, then need to not call the ErasePaint, encoderGraphDoPlot.ShowMessage, cairoPaintBarsPreCurrent stuff below
			*/

			//eccaCreated = false;

			if(action == encoderActions.CAPTURE)
			{
				if (! encoderThreadStart_CAPTURE ())
					return; //to avoid encoderThread.Start() without encoderThread instantiated
			}
			else { //action == encoderActions.CAPTURE_IM)
				encoderThreadStart_CAPTUREIM ();
			}

			encoderShowCaptureDoingButtons(true);

			LogB.Information("encoderThreadStart middle");
			encoderButtonsSensitive(encoderSensEnum.PROCESSINGCAPTURE);

			LogB.ThreadStart();

			//mute logs to improve stability (encoder inertial test only works with muted log)
			LogB.Mute = ! encoderRProcCapture.Debug;

			encoderThread.Start();
		}
		else if(
				action == encoderActions.RECALCULATE ||
				action == encoderActions.LOAD ||
				action == encoderActions.CURVES_AC)	//this does not run a pulseGTK
		{
			encoderThreadStart_RECALCULATE_LOAD_CURVESAC (action);
		}
		else { //encoderActions.ANALYZE
			encoderThreadStart_ANALYZE ();
		}
	}

	private void encoderThreadStart_CAPTUREBG ()
	{
		shownWaitAtInertialCapture = false;
		calledCaptureInertial = false;
		timeCalibrated = DateTime.Now;

		if (Config.SimulatedCapture)
			eCaptureInertialBG = new EncoderCaptureInertialBackground("");
		else
			eCaptureInertialBG = new EncoderCaptureInertialBackground(
					chronopicRegister.ConnectedOfType(ChronopicRegisterPort.Types.ENCODER).Port);

		encoderThreadBG = new Thread(new ThreadStart(encoderDoCaptureBG));
		GLib.Idle.Add (new GLib.IdleHandler (pulseGTKEncoderCaptureBG));

		LogB.ThreadStart();

		//mute logs to improve stability (encoder inertial test only works with muted log)
		LogB.Mute = ! encoderRProcCapture.Debug;

		encoderThreadBG.Start();
	}

	private bool encoderThreadStart_CAPTURE ()
	{
		webcamManage = new WebcamManage();
		if(webcamStart (WebcamManage.GuiContactsEncoder.ENCODER, 1))
			webcamEncoderFileStarted = WebcamEncoderFileStarted.NEEDTOCHECK;
		else
			webcamEncoderFileStarted = WebcamEncoderFileStarted.NOCAMERA;

		//remove treeview columns
		if( ! preferences.encoderCaptureInfinite || firstSetOfCont )
			treeviewEncoderCaptureRemoveColumns();

		cairoPaintBarsPreCurrent = new CairoPaintBarsPreEncoderCurrent (
				encoder_capture_curves_bars_drawingarea_cairo,
				preferences.fontTypeToGraph());//, "--capturing--");

		cairoPaintBarsPreCurrent.ShowMessage (
				encoder_capture_curves_bars_drawingarea_cairo,
				preferences.fontTypeToGraph(),
				Catalog.GetString("Capturing") + " …");

		encoderCaptureStringR = new List<string>();
		encoderCaptureStringR.Add(
				",series,exercise,mass,start,width,height," +
				"meanSpeed,maxSpeed,maxSpeedT,rvd," +
				"meanPower,peakPower,peakPowerT,pp_ppt," +
				"meanForce, maxForce, maxForceT, maxForce_maxForceT," +
				"workJ, impulse");

		string filename = UtilEncoder.GetEncoderCaptureTempFileName();
		if(File.Exists(filename))
			File.Delete(filename);

		encoderCaptureReadedLines = 0;
		deleteAllCapturedCurveFiles();

		capturingCsharp = encoderCaptureProcess.CAPTURING;
		if(compujumpAutologout != null)
			compujumpAutologout.StartCapturingEncoder();


		captureCurvesBarsData_l = new List<EncoderBarsData> ();

		needToRefreshTreeviewCapture = false;

		if(current_mode == Constants.Modes.POWERINERTIAL)
		{
			eCapture = new EncoderCaptureInertial();
		} else
			eCapture = new EncoderCaptureGravitatory();

		eCapture.FakeFinishByTime.Clicked -= new EventHandler (on_encoder_capture_finish_by_time);
		eCapture.FakeFinishByTime.Clicked += new EventHandler (on_encoder_capture_finish_by_time);

		if(preferences.encoderCaptureInfinite)
			encoderProcessFinishContMode = false; //will be true when finish button is pressed

		string portName = "";
		if (! Config.SimulatedCapture)
			portName = chronopicRegister.ConnectedOfType(ChronopicRegisterPort.Types.ENCODER).Port;


		//TODO: provar bé i implementar a més parts del software

		bool success = false;
		string errorStr = Catalog.GetString ("Sorry, cannot start capture.");
		if ( (csharpOrR == EncoderCapture.CsharpOrR.R || csharpOrR == EncoderCapture.CsharpOrR.BOTH) &&
				! Util.CanRunRscript ())
			errorStr = string.Format (Catalog.GetString ("Sorry, {0} software is not installed."),
					Util.GetRscriptBin ()) + "\n" + Constants.CheckChronojumpSoftwareWebsiteStr ();
		else
			success = eCapture.InitGlobal (
					preferences.encoderCaptureTime,
					preferences.encoderCaptureInactivityEndTime,
					preferences.encoderCaptureInfinite,
					findEcconFromCaptureGui (true), //so ecc-con will always be ecS
					portName,
					(current_mode == Constants.Modes.POWERINERTIAL && eCaptureInertialBG != null),
					encoderConfigurationNewCapture.IsInverted (),
					//configChronojump.EncoderCaptureShowOnlyBars,
					false, //false to show all, and let user change this at any moment
					Config.SimulatedCapture,
					csharpOrR);

		if(! success)
		{
			new DialogMessage (Constants.MessageTypes.WARNING, 450, 300, errorStr);

			// 1) sensitivize again
			sensitiveGuiEventDone(); //senstivize again

			// 2) show the detect big button
			button_detect_show_hide (true);

			// 3) erase cairo barplot (remove the Capturing...)
			cairoPaintBarsPreCurrent = new CairoPaintBarsPreEncoderCurrent (
					encoder_capture_curves_bars_drawingarea_cairo,
					preferences.fontTypeToGraph());
			prepareEventGraphEncoderCurrent = null; //to avoid is repainted again, and sound be repeated;

			// 4)
			encoder_pulsebar_capture.Fraction = 1;
			fullscreen_capture_progressbar.Fraction = 1;

			return false;
		}

		if(current_mode == Constants.Modes.POWERINERTIAL && eCaptureInertialBG != null)
		{
			eCaptureInertialBG.StoreData = true;
			eCapture.InitCalibrated(eCaptureInertialBG.AngleNow);

			if (Config.SimulatedCapture)
				eCaptureInertialBG.SimulatedReset();
		}

		/*
		 * initialize DateTime for rhythm
		 * also variable eccon_ec gravitatory mode is e -> c, inertial is c -> e
		 */
		if(encoderRhythm.ActiveRhythm) {
			encoderRhythmExecute = new EncoderRhythmExecuteHasRhythm (encoderRhythm, current_mode == Constants.Modes.POWERGRAVITATORY);
			label_rhythm.Text = Catalog.GetString("Rhythm");
			encoder_pulsebar_rhythm_eccon.Visible = true;
		} else if(encoderRhythm.UseClusters()) {
			encoderRhythmExecute = new EncoderRhythmExecuteJustClusters (encoderRhythm, current_mode == Constants.Modes.POWERGRAVITATORY);
			label_rhythm.Text = Catalog.GetString("Clusters");
			encoder_pulsebar_rhythm_eccon.Visible = false;
		}

		//triggers only work on gravitatory, concentric
		Preferences.TriggerTypes reallyCutByTriggers = Preferences.TriggerTypes.NO_TRIGGERS;

		if(preferences.encoderCaptureCutByTriggers != Preferences.TriggerTypes.NO_TRIGGERS &&
				currentEncoderGI == Constants.EncoderGI.GRAVITATORY && eCapture.Eccon == "c")
		{
			reallyCutByTriggers = preferences.encoderCaptureCutByTriggers;
			vbox_capturing_with_triggers.Visible = true;
		}

		box_encoder_capture_rhythm.Visible = (encoderRhythm.ActiveRhythm || encoderRhythm.UseClusters());
		encoderRProcCapture.CutByTriggers = reallyCutByTriggers;
		encoderClusterRestActive = false;
		encoderClusterLastRestSoundWasOnRep = -1; //to know which rep we are resting, to not repeat a rest in the same rep

		//to know if there are connection problems between chronopic and encoder
		encoderCaptureStopwatch = new Stopwatch();
		encoderCaptureStopwatch.Start();

		if (fullscreenLastCapture)
			fullscreen_button_fullscreen_encoder.Click ();

		button_video_play_this_test.Sensitive = false;
		blinkCapture = new BlinkImage (image_no_capturing_encoder, image_capturing_encoder);

		encoderThread = new Thread(new ThreadStart(encoderDoCaptureCsharp));
		GLib.Idle.Add (new GLib.IdleHandler (pulseGTKEncoderCaptureAndCurvesAC));

		return true;
	}

	private void encoderThreadStart_CAPTUREIM ()
	{
		eCapture = new EncoderCaptureIMCalc();
		bool success = eCapture.InitGlobal(
				preferences.encoderCaptureTimeIM, //two minutes max capture
				EncoderCaptureIMCalc.InactivityEndTime, //3 seconds
				false,
				findEcconFromCaptureGui (true),
				chronopicRegister.ConnectedOfType(ChronopicRegisterPort.Types.ENCODER).Port,
				false,
				false,
				false,
				false,
				EncoderCapture.CsharpOrR.R);
		if(! success)
		{
			new DialogMessage(Constants.MessageTypes.WARNING,
					Catalog.GetString("Sorry, cannot start capture."));
			return;
		}

		encoderRProcCapture.CutByTriggers = Preferences.TriggerTypes.NO_TRIGGERS; //do not cutByTriggers on inertial, yet.

		encoderCaptureStopwatch = new Stopwatch();
		encoderCaptureStopwatch.Start();

		encoderThread = new Thread(new ThreadStart(encoderDoCaptureCsharpIM));
		GLib.Idle.Add (new GLib.IdleHandler (pulseGTKEncoderCaptureIM));
	}

	private void encoderThreadStart_RECALCULATE_LOAD_CURVESAC (encoderActions action)
	{
		if(action == encoderActions.RECALCULATE || action == encoderActions.LOAD)
		{
			//______ 1) prepareEncoderGraphs

			//image_encoder_width = UtilGtk.WidgetWidth(viewport_image_encoder_capture)-5;
			//make graph half width of Chronojump window
			//but if video is disabled, then make it wider because thegraph will be much taller
			//if(configChronojump.UseVideo)
			//	image_encoder_width = Convert.ToInt32(UtilGtk.WidgetWidth(app1) / 2);
			//else
			image_encoder_width = Convert.ToInt32(UtilGtk.WidgetWidth(app1));

			if(image_encoder_width < 100)
				image_encoder_width = 100; //Not crash R with a png height of -1 or "figure margins too large"

			//-2 to accomadate the width slider without needing a height slider
			image_encoder_height = Convert.ToInt32(UtilGtk.WidgetHeight(app1));
			if(image_encoder_height < 100)
				image_encoder_height = 100; //Not crash R with a png height of -1 or "figure margins too large"

			LogB.Information("at load");

			//_______ 2) run stuff

			//don't need because ItemToggled is deactivated during capture
			//treeview_encoder_capture_curves.Sensitive = false;


			if(action == encoderActions.RECALCULATE)
			{
				encoderThread = new Thread(new ThreadStart(encoderDoCurvesGraphR_recalculate));
				GLib.Idle.Add (new GLib.IdleHandler (pulseGTKEncoderRecalculate));
			} else // action == encoderActions.LOAD
			{
				//capture tab
				box_set_loading.Visible = true;
				spinner_set_loading.Start ();

				//analyze tab
				label_encoder_load_signal_at_analyze.Visible = false;
				//encoder_pulsebar_load_signal_at_analyze.SetSizeRequest (
				//	label_encoder_load_signal_at_analyze.SizeRequest().Width, -1);
				encoder_pulsebar_load_signal_at_analyze.Fraction = 0;
				encoder_pulsebar_load_signal_at_analyze.Visible = true;

				encoderThread = new Thread(new ThreadStart(encoderDoCurvesGraphR_load));
				GLib.Idle.Add (new GLib.IdleHandler (pulseGTKEncoderLoad));
			}
			encoderButtonsSensitive(encoderSensEnum.PROCESSINGR);

			LogB.ThreadStart();
			encoderThread.Start(); 
		} else { //CURVES_AC
			 //______ 1) prepareEncoderGraphs
			 //don't call directly to prepareEncoderGraphs() here because it's called from a Non-GTK thread

			 //_______ 2) run stuff
			 //this does not run a pulseGTK
			encoderDoCurvesGraphR_curvesAC();
			encoderButtonsSensitive(encoderSensEnum.PROCESSINGR);
		}
	}

	private void encoderThreadStart_ANALYZE ()
	{
		//the -5 is because image is inside (is smaller than) viewport
		image_encoder_width = UtilGtk.WidgetWidth(scrolledwindow_image_encoder_analyze)-5;
		if(image_encoder_width < 100)
			image_encoder_width = 100; //Not crash R with a png height of -1 or "figure margins too large"

		image_encoder_height = UtilGtk.WidgetHeight(scrolledwindow_image_encoder_analyze)-5;
		if(image_encoder_height < 100)
			image_encoder_height = 100; //Not crash R with a png height of -1 or "figure margins too large"

		if(encoderSelectedAnalysis == "single" || encoderSelectedAnalysis == "singleAllSet")
			image_encoder_height -= UtilGtk.WidgetHeight(grid_encoder_analyze_instant); //to allow hslides and table

		encoder_pulsebar_analyze.Text = Catalog.GetString("Please, wait.");
		encoderRProcAnalyze.status = EncoderRProc.Status.WAITING;

		encoderRProcAnalyze.CrossValidate = checkbutton_crossvalidate.Active;
		encoderRProcAnalyze.SeparateSessionInDays = check_encoder_separate_session_in_days.Active;

		encoderThread = new Thread(new ThreadStart(encoderDoAnalyze));
		GLib.Idle.Add (new GLib.IdleHandler (pulseGTKEncoderAnalyze));

		encoderButtonsSensitive(encoderSensEnum.PROCESSINGR);
		treeview_encoder_analyze_curves.Sensitive = false;
		button_encoder_analyze_image_save.Sensitive = false;
		button_encoder_analyze_image_compujump_send_email.Sensitive = false;
		button_encoder_analyze_AB_save.Sensitive = false;
		button_encoder_analyze_table_save.Sensitive = false;
		button_encoder_analyze_1RM_save.Visible = false;

		LogB.ThreadStart();
		encoderThread.Start();
	}

	/* 
	 * <---- encoderThreadStart ----
	 */


	/* 
	 * ---- pulseGTK ---->
	 */

	bool shownWaitAtInertialCapture;
	bool calledCaptureInertial;
	DateTime timeCalibrated;
	private bool pulseGTKEncoderCaptureBG ()
	{
		if(! encoderThreadBG.IsAlive) {
			return false;
		}

		if(! shownWaitAtInertialCapture)
		{
			button_encoder_inertial_calibrate.Sensitive = false;
			button_encoder_inertial_calibrate_close.Sensitive = false;
			label_wait.Text = string.Format("Exercise will start in {0} seconds.", 3);
			shownWaitAtInertialCapture = true;
		}

		if(! calledCaptureInertial)
		{
			int elapsed = Convert.ToInt32(DateTime.Now.Subtract(timeCalibrated).TotalSeconds);
			if(elapsed > 3)
			{
				calledCaptureInertial = true;
				on_button_encoder_capture_clicked_do (true);
			} else
				label_wait.Text = string.Format("Exercise will start in {0} seconds.", 3 - elapsed);
		}

		if (eCaptureInertialBG == null)
			return false;

		int newValue = eCaptureInertialBG.AngleNow;
		if(eCaptureInertialBG.Phase == EncoderCaptureInertialBackground.Phases.ATCALIBRATEDPOINT)
		{
			image_encoder_capture_inertial_ecc.Visible = false;
			image_encoder_capture_inertial_con.Visible = false;
		}
		else if(eCaptureInertialBG.Phase == EncoderCaptureInertialBackground.Phases.CON)
		{
			image_encoder_capture_inertial_ecc.Visible = false;
			image_encoder_capture_inertial_con.Visible = true;
		}
		else if(eCaptureInertialBG.Phase == EncoderCaptureInertialBackground.Phases.ECC)
		{
			image_encoder_capture_inertial_ecc.Visible = true;
			image_encoder_capture_inertial_con.Visible = false;
		}
		/*
		else if(eCaptureInertialBG.Phase == EncoderCaptureInertialBackground.Phases.NOTMOVED)
		{
			//do not change nothing, show labels like before
		}
		*/

		//resize vscale if needed
		//0 is at the graphical top. abs(+-100) is on the bottom, but is called adjustment Upper
		int upper = Convert.ToInt32(vscale_encoder_capture_inertial_angle_now.Adjustment.Upper);
		if(Math.Abs(newValue) > upper)
			vscale_encoder_capture_inertial_angle_now.SetRange(0, upper *2);

		//update vscale value
		vscale_encoder_capture_inertial_angle_now.Value = Math.Abs(newValue);
		label_encoder_capture_inertial_angle_now.Text = newValue.ToString();


		Thread.Sleep (50);

		//don't plot info here because this is working all the time
		//LogB.Information(" CapBG:"+ encoderThreadBG.ThreadState.ToString());

		if(newValue < -100000 || newValue > 100000)
		{
			LogB.Information("Encoder seems to be disconnected");
			stopCapturingInertialBG();
		}

		return true;
	}

	private bool pulseGTKEncoderCaptureAndCurvesAC ()
	{
		//TODO: test this if this is needed:
		//if on inertia and already showing instructions, hide them
		if(vbox_encoder_bars_table_and_save_reps.Visible == false)
		{
			vbox_encoder_bars_table_and_save_reps.Visible = true;
			vbox_inertial_instructions.Visible = false;
		}

		if(! encoderThread.IsAlive || encoderProcessCancel)
		{
			LogB.Information("End from capture"); 

			if (blinkCapture != null)
				blinkCapture.End ();
			showHideBlinkIcon (blinkCapture, false);

			LogB.ThreadEnding();

			if(eCaptureInertialBG != null)
				eCaptureInertialBG.StoreData = false;

			finishPulsebar(encoderActions.CURVES_AC);

			vbox_capturing_with_triggers.Visible = false;

			if(encoderProcessCancel) {
				//stop video and will NOT be stored
				LogB.Information("call to webcamEncoderEnd");
				webcamEncoderEnd ();

				if(compujumpAutologout != null)
					compujumpAutologout.EndCapturingEncoder();
			}

			LogB.ThreadEnded(); 
			return false;
		}

		if(capturingCsharp == encoderCaptureProcess.CAPTURING) 
		{
			updatePulsebar(encoderActions.CAPTURE); //activity on pulsebar

			if (csharpOrR == EncoderCapture.CsharpOrR.CSHARP)
			{
				// >=  because encoderCaptureStringR has a row of titles
				if (encoderRProcCapture.CsharpMethodRepetitions_al.Count >= encoderCaptureStringR.Count)
					readingCurveFromRCont (UtilList.GetLast (encoderRProcCapture.CsharpMethodRepetitions_al));
			} else
				readingCurveFromR();

			if(current_mode == Constants.Modes.POWERINERTIAL) {
				updateEncoderCaptureGraphPaintData (UpdateEncoderPaintModes.INERTIAL);
				//updateEncoderCaptureSignalCairo (true, false); //inertial, forceRedraw
			} else {
				updateEncoderCaptureGraphPaintData (UpdateEncoderPaintModes.GRAVITATORY);
				//updateEncoderCaptureSignalCairo (false, false);
			}
			encoder_capture_signal_drawingarea_cairo.QueueDraw ();

			if (blinkCapture.Status == Blink.StatusEnum.NOTSTARTED)
				blinkCapture.Start ();
			showHideBlinkIcon (blinkCapture, true);

			if(needToRefreshTreeviewCapture) 
			{
				if(encoderRhythmExecute != null && ! encoderRhythmExecute.FirstPhaseDone)
				{
					bool upOrDown = true;
					string myEccon = findEcconFromCaptureGui (false);
					if (myEccon == "c")
						upOrDown = true;
					else if (myEccon == "ec" || myEccon == "ecS")
						upOrDown = false;
					else // (myEccon == "ce" || myEccon == "ceS")
						upOrDown = true;

					LogB.Information(encoderRhythm.ToString());
					encoderRhythmExecute.FirstPhaseDo(upOrDown);
				}

				//LogB.Error("HERE YES");
				//LogB.Error(Util.ListStringToString (encoderCaptureStringR));

				treeviewEncoderCaptureRemoveColumns();
				eCapture.Ecca.curvesAccepted = createTreeViewEncoderCapture(encoderCaptureStringR);

				//if(plotCurvesBars) {}
				string mainVariable = Constants.GetEncoderVariablesCapture(preferences.encoderCaptureMainVariable);
				double mainVariableHigher = feedbackWin.GetMainVariableHigher(mainVariable);
				double mainVariableLower = feedbackWin.GetMainVariableLower(mainVariable);
				string secondaryVariable = Constants.GetEncoderVariablesCapture(preferences.encoderCaptureSecondaryVariable);
				if(! preferences.encoderCaptureSecondaryVariableShow)
					secondaryVariable = "";
				//TODO:
				//captureCurvesBarsData_l.Add(new EncoderBarsData(meanSpeed, maxSpeed, meanPower, peakPower));
				//captureCurvesBarsData_l.Add(new EncoderBarsData(20, 39, 10, 40));

				//Cairo
				prepareEventGraphEncoderCurrent = new PrepareEventGraphEncoderCurrent (
						mainVariable, mainVariableHigher, mainVariableLower,
						secondaryVariable, preferences.encoderCaptureShowLoss,
						true, //capturing
						findEcconFromCaptureGui (true),
						findMassFromGui (Constants.MassType.DISPLACED),
						feedbackEncoder,
						current_mode == Constants.Modes.POWERINERTIAL,
						configChronojump.PlaySoundsFromFile,
						captureCurvesBarsData_l,
						encoderCaptureListStore,
						preferences.encoderCaptureMainVariableThisSetOrHistorical,
						encoderMaxIntersessionForCapture,
						preferences.encoderCaptureInertialDiscardFirstN,
						preferences.encoderCaptureShowNRepetitions,
						preferences.volumeOn,
						preferences.gstreamer);

				if (notebook_start.CurrentPage == Convert.ToInt32 (notebook_start_pages.FULLSCREENCAPTURE))
					fullscreen_capture_drawingarea_cairo.QueueDraw ();
				else
					encoder_capture_curves_bars_drawingarea_cairo.QueueDraw ();

				needToRefreshTreeviewCapture = false;
			}

			if(webcamEncoderFileStarted == WebcamEncoderFileStarted.NEEDTOCHECK)
				if(WebcamManage.RecordingFileStarted ())
				{
					webcamEncoderFileStarted = WebcamEncoderFileStarted.RECORDSTARTED;
					label_video_encoder_feedback.Text = "Recording video.";
				}

			if(encoderRhythm.ActiveRhythm || encoderRhythm.UseClusters())
				updatePulsebarRhythm();

			//changed trying to fix crash of nuell 27/may/2016
			//LogB.Debug(" Cap:", encoderThread.ThreadState.ToString());
			//LogB.Information(" Cap:" + encoderThread.ThreadState.ToString());
		}
		else if(capturingCsharp == encoderCaptureProcess.STOPPING)
		{
			if (blinkCapture != null)
				blinkCapture.End ();
			showHideBlinkIcon (blinkCapture, false);

			//stop video		
			webcamEncoderEnd (); //this will end but file will be copied later (when we have encoderSignalUniqueID)

			//don't allow to press cancel or finish
			button_encoder_capture_cancel.Sensitive = false;
			button_encoder_capture_finish.Sensitive = false;
			button_encoder_capture_finish_cont.Sensitive = false;
			fullscreen_button_fullscreen_encoder.Sensitive = false;

			capturingCsharp = encoderCaptureProcess.STOPPED;

			if(compujumpAutologout != null)
				compujumpAutologout.EndCapturingEncoder();
		} else {	//STOPPED	
			LogB.Debug("at pulseGTKEncoderCaptureAndCurvesAC stopped");
			//do curves, capturingCsharp has ended
			updatePulsebar (encoderActions.CURVES_AC); //activity on pulsebar
			//LogB.Debug(" Cur:", encoderThread.ThreadState.ToString());
			LogB.Information(" Cur:" + encoderThread.ThreadState.ToString());

			if(compujumpAutologout != null)
				compujumpAutologout.EndCapturingEncoder();
		}
			
		//Thread.Sleep (50);
		Thread.Sleep (25); //better for asteroids

		return true;
	}
	
	private bool pulseGTKEncoderCaptureIM ()
	{
		if(! encoderThread.IsAlive || encoderProcessCancel) {
			LogB.ThreadEnding(); 
			finishPulsebar(encoderActions.CAPTURE_IM);
			
			LogB.ThreadEnded(); 
			return false;
		}
		updatePulsebar(encoderActions.CAPTURE_IM); //activity on pulsebar
		updateEncoderCaptureGraphPaintData (UpdateEncoderPaintModes.CALCULE_IM);

		Thread.Sleep (25);
		//LogB.Debug(" CapIM:", encoderThread.ThreadState.ToString());
		LogB.Information(" CapIM:"+ encoderThread.ThreadState.ToString());
		return true;
	}
	

	private bool pulseGTKEncoderRecalculate ()
	{
		if(! encoderThread.IsAlive || encoderProcessCancel)
		{
			LogB.Information("End from recalculate");
			LogB.ThreadEnding(); 
			if(encoderProcessCancel)
				encoderRProcAnalyze.CancelRScript = true;

			finishPulsebar (encoderActions.RECALCULATE);
			
			LogB.ThreadEnded(); 
			return false;
		}
		updatePulsebar (encoderActions.RECALCULATE); //activity on pulsebar
		Thread.Sleep (50);
		//LogB.Debug(" Recalculate:", encoderThread.ThreadState.ToString());
		LogB.Information(" Recalculate:" + encoderThread.ThreadState.ToString());
		return true;
	}

	private bool pulseGTKEncoderLoad ()
	{
		if(! encoderThread.IsAlive || encoderProcessCancel) {
			LogB.ThreadEnding(); 
			if(encoderProcessCancel){
				encoderRProcAnalyze.CancelRScript = true;
			}

			//capture tab
			spinner_set_loading.Stop ();
			box_set_loading.Visible = false;

			//analyze tab
			label_encoder_load_signal_at_analyze.Visible = true;
			encoder_pulsebar_load_signal_at_analyze.Visible = false;

			finishPulsebar(encoderActions.LOAD);
			
			LogB.ThreadEnded(); 
			return false;
		}
		updatePulsebar(encoderActions.LOAD); //activity on pulsebar

		Thread.Sleep (50);
		//LogB.Debug(" L:", encoderThread.ThreadState.ToString());
		LogB.Information(" L:" + encoderThread.ThreadState.ToString());
		return true;
	}
	
	private bool pulseGTKEncoderAnalyze ()
	{
		if( encoderRProcAnalyze.status == EncoderRProc.Status.DONE || ! encoderThread.IsAlive || encoderProcessCancel) {
			LogB.ThreadEnding(); 
			if(encoderProcessCancel){
				encoderRProcAnalyze.CancelRScript = true;
			}

			finishPulsebar(encoderActions.ANALYZE);
			
			LogB.ThreadEnded(); 
			return false;
		}
		updatePulsebar(encoderActions.ANALYZE); //activity on pulsebar
		Thread.Sleep (50);
		//LogB.Debug(" A:", encoderThread.ThreadState.ToString());
		LogB.Information(" A:" + encoderThread.ThreadState.ToString());
		return true;
	}
	
	private void updatePulsebar (encoderActions action) 
	{
		if(action == encoderActions.CAPTURE && preferences.encoderCaptureInfinite) {
			encoder_countdown_label.Text = "";
			encoder_pulsebar_capture.Pulse();
			fullscreen_label_message.Text = "";
			fullscreen_capture_progressbar.Pulse();
			return;
		}

		if(action == encoderActions.CAPTURE || action == encoderActions.CAPTURE_IM) 
		{
			int selectedTime = preferences.encoderCaptureTime;
			if(action == encoderActions.CAPTURE_IM)
				selectedTime = preferences.encoderCaptureTimeIM;

			encoder_pulsebar_capture.Fraction = UtilAll.DivideSafeFraction(
					(selectedTime - eCapture.Countdown), selectedTime);
			encoder_countdown_label.Text = eCapture.Countdown + " s";
			fullscreen_capture_progressbar.Fraction = UtilAll.DivideSafeFraction(
					(selectedTime - eCapture.Countdown), selectedTime);
			fullscreen_label_message.Text = eCapture.Countdown + " s";

			if(encoderCaptureStopwatch.Elapsed.TotalSeconds >= 3 && eCapture.Countdown == preferences.encoderCaptureTime)
			{
				//event_execute_label_message.Text = "Chronopic seems not properly connected to encoder");
				event_execute_label_message.Text = "Plug encoder into Chronopic"; //TODO: improve this and finish capture with problems
				fullscreen_label_message.Text = "Plug encoder into Chronopic"; //TODO: improve this and finish capture with problems
			}

			return;
		}

		try {
			encoder_countdown_label.Text = "";
			string contents = Catalog.GetString("Please, wait.");
			double fraction = -1;
			/*
			if(Util.FileExists(UtilEncoder.GetEncoderStatusTempFileName())) {
				contents = Util.ReadFile(UtilEncoder.GetEncoderStatusTempFileName(), true);
				//contents is:
				//(1/5) Starting R
				//(5/5) R tasks done

				//-48: ascii 0 char
				if(System.Char.IsDigit(contents[1]) && System.Char.IsDigit(contents[3]))
					fraction = UtilAll.DivideSafeFraction(
							Convert.ToInt32(contents[1]-48), Convert.ToInt32(contents[3]-48) );
			}
			*/

			if(Util.FileExists(UtilEncoder.GetEncoderStatusTempBaseFileName() + "6.txt")) 
			{
				fraction = 6;
				contents = Catalog.GetString("R tasks done");
			}
			else if(Util.FileExists(UtilEncoder.GetEncoderStatusTempBaseFileName() + "5.txt")) 
			{
				fraction = 5;
				contents = "Smoothing done";
			} else if(Util.FileExists(UtilEncoder.GetEncoderStatusTempBaseFileName() + "4.txt")) 
			{
				fraction = 4;
				if(encoderRProcAnalyze.CurvesReaded > 0)
					contents = encoderRProcAnalyze.CurvesReaded.ToString();
				else
					contents = Catalog.GetString("Repetitions processed");
			} else if(Util.FileExists(UtilEncoder.GetEncoderStatusTempBaseFileName() + "3.txt")) 
			{
				fraction = 3;
				if(encoderRProcAnalyze.CurvesReaded > 0)
					contents = encoderRProcAnalyze.CurvesReaded.ToString();
				else
					contents = Catalog.GetString("Starting process");
			} else if(Util.FileExists(UtilEncoder.GetEncoderStatusTempBaseFileName() + "2.txt")) 
			{
				fraction = 2;
				contents = Catalog.GetString("Loading libraries");
			} else if(Util.FileExists(UtilEncoder.GetEncoderStatusTempBaseFileName() + "1.txt")) 
			{
				fraction = 1;
				contents = Catalog.GetString("Starting R");
			}

			if(action == encoderActions.CURVES_AC)
			{
				if(fraction == -1)
					encoder_pulsebar_capture.Pulse();
				else
					encoder_pulsebar_capture.Fraction = UtilAll.DivideSafeFraction(fraction, 6);

				event_execute_label_message.Text = contents;
			}
			else if(action == encoderActions.LOAD)
			{
				if(fraction <= 0)
					encoder_pulsebar_load_signal_at_analyze.Pulse();
				else
					encoder_pulsebar_load_signal_at_analyze.Fraction = UtilAll.DivideSafeFraction(fraction, 6);
			} else {
				if(fraction == -1)
					encoder_pulsebar_analyze.Pulse();
				else
					encoder_pulsebar_analyze.Fraction = UtilAll.DivideSafeFraction(fraction, 6);

				encoder_pulsebar_analyze.Text = contents;
			}
		} catch {
			//UtilEncoder.GetEncoderStatusTempBaseFileName() 1,2,3,4,5 is deleted at the end of the process
			//this can make crash updatePulsebar sometimes
			LogB.Warning("catched at updatePulsebar");
		}
	}

	bool encoderClusterRestActive;
	int encoderClusterLastRestSoundWasOnRep; //to know which rep we are resting, to not repeat a rest in the same rep

	private void updatePulsebarRhythm()
	{
		if(encoderRhythm.ActiveRhythm)
		{
			if(! encoderRhythmExecute.FirstPhaseDone)
			{
				box_encoder_capture_rhythm_doing.Visible = true;
				box_encoder_capture_rhythm_rest.Visible = false;
				encoder_pulsebar_rhythm_eccon.Fraction = 0;
				label_rhythm_rep.Text = "...";
				return;
			} else {
				if(encoderRhythmExecute.CurrentPhase != encoderRhythmExecute.LastPhase)// &&
						//uncomment to avoid sound at start of rest
						//encoderRhythmExecute.CurrentPhase != EncoderRhythmExecute.Phases.RESTREP)
					{
						Util.PlaySound(Constants.SoundTypes.CAN_START, preferences.volumeOn, preferences.gstreamer);
						LogB.Information(encoderRhythmExecute.CurrentPhase.ToString());
					}

				encoderRhythmExecute.LastPhase = encoderRhythmExecute.CurrentPhase;
			}

			encoderRhythmExecute.CalculateFractionsAndText();

			if (encoderRhythmExecute.TextRest == "")
			{
				box_encoder_capture_rhythm_doing.Visible = true;
				box_encoder_capture_rhythm_rest.Visible = false;
				encoder_pulsebar_rhythm_eccon.Fraction = encoderRhythmExecute.Fraction;
				label_rhythm_rep.Text = encoderRhythmExecute.TextRepetition;
			} else {
				box_encoder_capture_rhythm_doing.Visible = false;
				box_encoder_capture_rhythm_rest.Visible = true;
				label_encoder_rhythm_rest.Text = encoderRhythmExecute.TextRest;
			}
		}
		else if(encoderRhythm.UseClusters())
		{
			// 1) check if first phase has been done
			//just for show cluster rest (so on feedback gui, rhythm will be unactive but cluster rest active)
			if(! encoderRhythmExecute.FirstPhaseDone)
			{
				encoder_pulsebar_rhythm_eccon.Fraction = 0;
				box_encoder_capture_rhythm_doing.Visible = true;
				box_encoder_capture_rhythm_rest.Visible = false;
				return;
			}

			// 2) check if showRest has to be shown

			int repsDone = eCapture.Ecca.curvesAccepted;
			bool showRest = false;

			if( radio_encoder_eccon_concentric.Active && repsDone % encoderRhythm.RepsCluster == 0 &&
					(! encoderRhythm.RestAfterEcc || eCapture.DirectionCompleted == 1) )
				showRest = true;
			else if(repsDone > 1 && radio_encoder_eccon_eccentric_concentric.Active)
			{
				if(! encoderRhythm.RestAfterEcc && repsDone % (2 * encoderRhythm.RepsCluster) == 0)
					showRest = true;
				else if(encoderRhythm.RestAfterEcc &&
						repsDone >= 2 && //to avoid 0/x crash
						(repsDone -1) % (2 * encoderRhythm.RepsCluster) == 0)
					showRest = true;
			}

			// 3) if showRest have to be show, check that is not already shown on this rep
			//    if all ok, play sound, start rest
			if(showRest)
			{
				if(! encoderRhythmExecute.ClusterRestDoing() && encoderClusterLastRestSoundWasOnRep != repsDone)
				{
					encoderRhythmExecute.ClusterRestStart();
					box_encoder_capture_rhythm_doing.Visible = false;
					box_encoder_capture_rhythm_rest.Visible = true;
					Util.PlaySound(Constants.SoundTypes.CAN_START, preferences.volumeOn, preferences.gstreamer);
					encoderClusterLastRestSoundWasOnRep = repsDone;
					encoderClusterRestActive = true;
				}
			}

			// 4) if rest is active, see if we have to end it or not

			if(encoderClusterRestActive)
			{
				if (encoderRhythmExecute.ClusterRestSecondsStr() == "")
				{
					encoderClusterRestActive = false;
					Util.PlaySound(Constants.SoundTypes.CAN_START, preferences.volumeOn, preferences.gstreamer);
					encoderRhythmExecute.ClusterRestStop();
					box_encoder_capture_rhythm_doing.Visible = true;
					box_encoder_capture_rhythm_rest.Visible = false;
				} else {
					string restStr = encoderRhythmExecute.ClusterRestSecondsStr();
					label_encoder_rhythm_rest.Text = restStr;
					box_encoder_capture_rhythm_doing.Visible = false;
					box_encoder_capture_rhythm_rest.Visible = true;
				}
			}
		}
	}

	/* 
	 * <---- pulseGTK ----
	 */

	/* 
	 * ---- finishPulsebar ---->
	 */

	bool captureContWithCurves = true;
	private void finishPulsebar (encoderActions action)
	{
		if(
				action == encoderActions.CAPTURE || 
				action == encoderActions.CAPTURE_IM || 
				action == encoderActions.RECALCULATE ||
				action == encoderActions.CURVES_AC || 
				action == encoderActions.LOAD )
		{
			LogB.Information ("ffffffinishPulsebarrrrr action: " + action.ToString ());
		
			//save video will be later at encoderSaveSignalOrCurve, because there encoderSignalUniqueID will be known
			
			if(encoderProcessCancel || encoderProcessProblems)
			{
				encoder_pulsebar_capture.Fraction = 1;
				fullscreen_capture_progressbar.Fraction = 1;
			
				if(encoderProcessProblems) {
					new DialogMessage(Constants.MessageTypes.WARNING, 
							Catalog.GetString("Sorry. Error doing graph.") + 
							"\n" + Catalog.GetString("Maybe R is not installed.") + 
							"\n" + Catalog.GetString("Please, install it from here:") +
							"\n\n" + Constants.RmacDownload);
					encoderProcessProblems = false;
				} else {
					if(action == encoderActions.CAPTURE_IM)
						encoder_configuration_win.Button_encoder_capture_inertial_do_ended(0,"Cancelled");
					else {
						event_execute_label_message.Text = Catalog.GetString("Cancelled");
						fullscreen_label_message.Text = Catalog.GetString("Cancelled");
					}
				}
			}
			else if(action == encoderActions.CAPTURE && encoderProcessFinish)
			{
				event_execute_label_message.Text = Catalog.GetString("Finished");
				fullscreen_label_message.Text = Catalog.GetString("Finished");
				updateEncoderAnalyzeExercisesPre ();
			} 
			else if (action == encoderActions.CURVES_AC || action == encoderActions.LOAD || action == encoderActions.RECALCULATE)
			{
				//variables for plotting curves bars graph
				string mainVariable = Constants.GetEncoderVariablesCapture(preferences.encoderCaptureMainVariable);
				double mainVariableHigher = feedbackWin.GetMainVariableHigher(mainVariable);
				double mainVariableLower = feedbackWin.GetMainVariableLower(mainVariable);
				string secondaryVariable = Constants.GetEncoderVariablesCapture(preferences.encoderCaptureSecondaryVariable);
				if(! preferences.encoderCaptureSecondaryVariableShow)
					secondaryVariable = "";

				if(action == encoderActions.CURVES_AC && preferences.encoderCaptureInfinite && ! captureContWithCurves)
				{
					//will use captureCurvesBarsData_l (created on capture)
					LogB.Information("at fff with captureCurvesBarsData_l =");
					LogB.Information(captureCurvesBarsData_l.Count.ToString());
				} else {
					List<string> contents = Util.ReadFileAsStringList(UtilEncoder.GetEncoderCurvesTempFileName(), "");

					encoderUpdateTreeViewCapture(contents); //this updates encoderCaptureCurves

					captureCurvesBarsData_l = new List<EncoderBarsData> ();
					foreach (EncoderCurve curve in encoderCaptureCurves)
						//TODO: add here also the Start and Duration needed for video, maybe better be an standard class in order to not have crashes for trying to access limits on an array (when start and duration is implemented)
						captureCurvesBarsData_l.Add (new EncoderBarsData (
									Convert.ToDouble(curve.Start),
									Convert.ToDouble(curve.Duration),
									Convert.ToDouble(curve.Height),
									Convert.ToDouble(curve.MeanSpeed),
									Convert.ToDouble(curve.MaxSpeed),
									Convert.ToDouble(curve.MeanForce),
									Convert.ToDouble(curve.MaxForce),
									Convert.ToDouble(curve.MeanPower),
									Convert.ToDouble(curve.PeakPower),
									Convert.ToDouble(curve.WorkJ),
									Convert.ToDouble(curve.Impulse)
									));
				}


				string eccon = "";
				double displacedMass = 0;
				int exerciseID = 0;
				EncoderConfiguration encoderConfiguration = new EncoderConfiguration ();
				string laterality = "RL";
				double extraWeight = 0;
				if (action == encoderActions.LOAD || action == encoderActions.RECALCULATE)
				{
					exerciseID = currentEncoderSQLSet.exerciseID;
					eccon = findEcconFromCurrentSet (true);
					displacedMass = findDisplacedMassFromSQL ();
					encoderConfiguration = currentEncoderSQLSet.encoderConfiguration;
					laterality = currentEncoderSQLSet.Laterality;
					extraWeight = currentEncoderSQLSet.extraWeightD;
				}
				else if (action == encoderActions.CURVES_AC)
				{
					exerciseID = getExerciseIDFromEncoderCombo(exerciseCombos.CAPTURE);
					eccon = findEcconFromCaptureGui (true);	//force ecS (ecc-conc separated)
					displacedMass = findMassFromGui (Constants.MassType.DISPLACED);
					encoderConfiguration = encoderConfigurationNewCapture;
					laterality = getLateralityFromGui (true);
					extraWeight = Convert.ToDouble (spin_encoder_extra_weight.Value);
				}

				//Cairo
				prepareEventGraphEncoderCurrent = new PrepareEventGraphEncoderCurrent (
						mainVariable, mainVariableHigher, mainVariableLower,
						secondaryVariable, preferences.encoderCaptureShowLoss,
						false, //not capturing
						eccon,
						displacedMass,
						feedbackEncoder,
						encoderConfiguration.has_inertia,
						configChronojump.PlaySoundsFromFile,
						captureCurvesBarsData_l,
						encoderCaptureListStore,
						preferences.encoderCaptureMainVariableThisSetOrHistorical,
						encoderMaxIntersessionForCapture,
						preferences.encoderCaptureInertialDiscardFirstN,
						preferences.encoderCaptureShowNRepetitions,
						preferences.volumeOn,
						preferences.gstreamer);

				//no need in fullscreen because it will be closed
				encoder_capture_curves_bars_drawingarea_cairo.QueueDraw ();

				//autosave signal (but not in load)
				if(action == encoderActions.RECALCULATE || action == encoderActions.CURVES_AC)
				{
					bool needToAutoSaveCurve = false;
					if(
							encoderSignalUniqueID == -1 &&	//if we just captured
							(preferences.encoderAutoSaveCurve == Constants.EncoderAutoSaveCurve.ALL ||
							preferences.encoderAutoSaveCurve == Constants.EncoderAutoSaveCurve.BEST ||
							preferences.encoderAutoSaveCurve == Constants.EncoderAutoSaveCurve.BESTN ||
							preferences.encoderAutoSaveCurve == Constants.EncoderAutoSaveCurve.BESTNCONSECUTIVE ||
							preferences.encoderAutoSaveCurve == Constants.EncoderAutoSaveCurve.FROM4TOPENULTIMATE) )
						needToAutoSaveCurve = true;


					string encoderSaveResult = encoderSaveSignalOrCurve(false, "signal", 0); //this updates encoderSignalUniqueID
					event_execute_label_message.Text = encoderSaveResult;
					fullscreen_label_message.Text = encoderSaveResult;

					if(needToAutoSaveCurve)
						encoderCaptureSaveCurvesAllNoneBest(preferences.encoderAutoSaveCurve,
								Constants.GetEncoderVariablesCapture(preferences.encoderCaptureMainVariable));

					if(action == encoderActions.CURVES_AC)
					{
						SqliteEncoder se = new SqliteEncoder ();
						treeViewResultsSession.AddEncoder (currentPerson.UniqueID, currentPerson.Name,
								se.SelectSetsAndRepsLList (
									false, currentPerson.UniqueID, currentSession.UniqueID,
									currentEncoderGI, currentEncoderSQLSet.exerciseID, encoderSignalUniqueID),
								"");

						//1) unMute logs if preferences.muteLogs == false
						LogB.Mute = preferences.muteLogs;

						//1) save the triggers now that we have an encoderSignalUniqueID
						eCapture.SaveTriggers(encoderSignalUniqueID); //dbcon is closed
						showEncoderAnalyzeTriggersAndTab();

						if(encoderRProcCapture.CutByTriggers != Preferences.TriggerTypes.NO_TRIGGERS &&
								! eCapture.MinimumOneTriggersOn())
							new DialogMessage(
									"Chronojump",
									Constants.MessageTypes.WARNING,
									"Not found enought triggers to cut repetitions." + "\n\n" +
									"Repetitions have been cut automatically.");

						//2) send the json to server
						//check if encoderCaptureCurves > 0
						//(this is the case of a capture without repetitions or can have on ending cont mode)

						if(configChronojump.Compujump && check_encoder_networks_upload.Active && encoderCaptureCurves.Count > 0)
						{
							uploadEncoderDataObjectIfPossible();
						}
						else if(configChronojump.Exhibition &&
								configChronojump.ExhibitionStationType == ExhibitionTest.testTypes.INERTIAL &&
								encoderCaptureCurves.Count > 0)
						{
							UploadEncoderDataObject uo = new UploadEncoderDataObject(
									encoderCaptureCurves, currentEncoderSQLSet.eccon);
							SqliteJson.UploadExhibitionTest(getExhibitionTestFromGui(ExhibitionTest.testTypes.INERTIAL, Convert.ToDouble(uo.pmeanByPowerAsDouble)));

						}
						encoderLoadToPaintData ();
					}

				} else { //action == encoderActions.LOAD
					event_execute_label_message.Text = "";
				}

				if (action == encoderActions.CURVES_AC || action == encoderActions.RECALCULATE)
					updateGraphEncoderSessionBars();

				/*
				 * if we captured, but encoderSignalUniqueID has not been changed on encoderSaveSignalOrCurve
				 * because there are no curves (problems detecting, or minimal height so big
				 * then do not continue
				 * because with a value of -1 there will be problems in 
				 * SqliteEncoder.Select(false, Convert.ToInt32(encoderSignalUniqueID), …)
				 */
				LogB.Information(" encoderSignalUniqueID:" + encoderSignalUniqueID.ToString ());
				if (encoderSignalUniqueID >= 0)
				{
					/*
					 * (0) open Sqlite
					 * (1) manageCurvesOfThisSignal
					 * (2) update meanPower on SQL encoder
					 * (3) close Sqlite
					 */

					Sqlite.Open();

					manageCurvesOfThisSignal();

					//update meanPower on SQL encoder
					findAndMarkSavedCurves(true, true); //SQL opened; update curve SQL records (like future1: meanPower, 2 and 3)
					
					Sqlite.Close();

					if (action == encoderActions.LOAD || action == encoderActions.RECALCULATE) // see: diagrams/processes/person_results_changes.dia
					{
						SqliteEncoder se = new SqliteEncoder ();
						treeview_results_session_cursor_changed_block = true; //to block cursor_change on store.Remove ()

						// note this is needed on load because graph.R code maybe decides that there are more or less reps depending on smoothing or other factors
						treeViewResultsSession.UpdateReps (
								se.SelectSetsAndRepsLList (
									false, currentPerson.UniqueID, currentSession.UniqueID,
									currentEncoderGI, currentEncoderSQLSet.exerciseID, encoderSignalUniqueID)
								);
						treeview_results_session_cursor_changed_block = false;

						encoderLoadToPaintData (); // done now (after findAndMarkSavedCurves)
					}
				}
			}

			if(action == encoderActions.CAPTURE_IM && ! encoderProcessCancel && ! encoderProcessProblems) 
			{
				string imResultText = Util.ChangeDecimalSeparator(
						Util.ReadFile(UtilEncoder.GetEncoderSpecialDataTempFileName(), true) );
				LogB.Information("imResultText = |" + imResultText + "|");

				if(imResultText == "NA" || imResultText == "-1" || imResultText == "")
					encoder_configuration_win.Button_encoder_capture_inertial_do_ended (0, "Error capturing. Maybe more oscillations are needed.");
				else {
					//script calculates kg*m^2 -> GUI needs kg*cm^2
					encoder_configuration_win.Button_encoder_capture_inertial_do_ended (
							Convert.ToDouble(imResultText) * 10000.0, Catalog.GetString("Finished"));
				}

				encoderButtonsSensitive(encoderSensEnum.DONENOSIGNAL);
			} else {
				encoderButtonsSensitive(encoderSensEnumStored);

				//an user has one active concentric curve
				//signal of this curve is loaded
				//user change to ecc-con and recalculate
				//then that concentrinc curve disappears
				//button_encoder_analyze have to be unsensitive because there are no curves:
				button_encoder_analyze_sensitiveness();

				//LogB.Debug(Enum.Parse(typeof(encoderActions), action.ToString()).ToString());
				//LogB.Debug(encoderProcessCancel.ToString());
						
				if(encoderProcessCancel)
					removeSignalFromGuiBecauseDeletedOrCancelled();
			}
			
			encoder_pulsebar_capture.Fraction = 1;
			fullscreen_capture_progressbar.Fraction = 1;
			//analyze_image_save only has not to be sensitive now because capture graph will be saved
			image_encoder_analyze.Sensitive = false;
			vbox_encoder_analyze_instant.Visible = false; //play with Visible instead of Sensitive because with Sensitive the pixmap is fully shown

			button_encoder_analyze_image_save.Sensitive = false;
			button_encoder_analyze_image_compujump_send_email.Sensitive = false;
			button_encoder_analyze_AB_save.Sensitive = false;
			button_encoder_analyze_table_save.Sensitive = false;
			button_encoder_analyze_1RM_save.Visible = false;
		
			encoderShowCaptureDoingButtons(false);

			if(action == encoderActions.CURVES_AC)
			{
				restTime.AddOrModify(currentPerson.UniqueID, currentPerson.Name, true);
				updateRestTimes();
			}

			//on inertial, check after capture if string was not fully extended and was corrected
			if(current_mode == Constants.Modes.POWERINERTIAL &&
					action == encoderActions.CURVES_AC && 
					Util.FileExists(UtilEncoder.GetEncoderSpecialDataTempFileName())) 
			{
				string str = Util.ReadFile(UtilEncoder.GetEncoderSpecialDataTempFileName(), true);
				if(str != null && str == "SIGNAL CORRECTED")
					new DialogMessage(Constants.MessageTypes.WARNING, 
						Catalog.GetString("Set corrected. string was not fully extended at the beginning."));
			}

			if( encoderRhythm != null &&
					! encoderRhythm.ActiveRhythm && encoderRhythm.UseClusters() &&
					encoderRhythmExecute != null && encoderRhythmExecute.ClusterRestDoing() )
				encoderRhythmExecute.ClusterRestStop ();

			if(action == encoderActions.RECALCULATE || action == encoderActions.CURVES_AC || action == encoderActions.LOAD)
				sensitiveGuiEventDone();

		} else { //ANALYZE
			if(encoderProcessCancel) {
				encoder_pulsebar_analyze.Text = Catalog.GetString("Cancelled");
			} else {
				if(compujumpAutologout != null)
					compujumpAutologout.UpdateLastEncoderAnalyzeTime();

				//TODO pensar en si s'ha de fer 1er amb mida petita i despres amb gran (en el zoom),
				//o si es una sola i fa alguna edicio
				
				if(encoderSelectedAnalysis == "single" || encoderSelectedAnalysis == "singleAllSet")
				{
					drawingarea_encoder_analyze_cairo_pixbuf = UtilGtk.OpenPixbufSafe(
							UtilEncoder.GetEncoderGraphTempFileName(),
							drawingarea_encoder_analyze_cairo_pixbuf);

					drawingarea_encoder_analyze_instant.QueueDraw(); //will fire ExposeEvent

					vbox_encoder_analyze_instant.Visible = true;

					button_encoder_analyze_AB_save.Visible = checkbutton_encoder_analyze_b.Active;

					notebook_encoder_analyze.CurrentPage = 1;
				} else {
					//maybe image is still not readable
					image_encoder_analyze = UtilGtk.OpenImageSafe(
							UtilEncoder.GetEncoderGraphTempFileName(),
							image_encoder_analyze);
					
					button_encoder_analyze_AB_save.Visible = false;

					notebook_encoder_analyze.CurrentPage = 0;
				}

				encoder_pulsebar_analyze.Text = "";

				string contents = Util.ReadFile(UtilEncoder.GetEncoderAnalyzeTableTempFileName(), false);
				if (contents != null && contents != "") {
					if(radiobutton_encoder_analyze_neuromuscular_profile.Active) {
						treeviewEncoderAnalyzeRemoveColumns();
						encoderAnalyzeListStore = new Gtk.ListStore (typeof (EncoderCurve));
						createTreeViewEncoderAnalyzeNeuromuscular(contents);
					} else if(
							radiobutton_encoder_analyze_1RM.Active &&
							Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_analyze_1RM),
								encoderAnalyze1RMTranslation) == "1RM Indirect") {
						treeviewEncoderAnalyzeRemoveColumns();
						encoderAnalyzeListStore = new Gtk.ListStore (typeof (List<double>));
					} else {
						treeviewEncoderAnalyzeRemoveColumns();
						encoderAnalyzeListStore = new Gtk.ListStore (typeof (EncoderCurve));
						createTreeViewEncoderAnalyze(contents, current_mode);
					}
				}

				if(encoderSelectedAnalysis == "single" || encoderSelectedAnalysis == "singleAllSet") {
					eai = new EncoderAnalyzeInstant();
					eai.ReadArrayFile(UtilEncoder.GetEncoderInstantDataTempFileName());
					eai.ReadGraphParams(UtilEncoder.GetEncoderSpecialDataTempFileName());

					//ranges should have max value the number of the lines of csv file minus the header
					hscale_encoder_analyze_a.SetRange(1, eai.speed.Count);
					hscale_encoder_analyze_b.SetRange(1, eai.speed.Count);

					/*
					   will update alll the a, b, range labels
					   range is done two times but note this label will disappear in the future
					   and some treeview will be used
					   */
					on_hscale_encoder_analyze_a_value_changed (new object (), new EventArgs ());
					if (checkbutton_encoder_analyze_b.Active)
						on_hscale_encoder_analyze_b_value_changed (new object (), new EventArgs ());
					//eai.PrintDebug();
				}

				encoderLastAnalysis = encoderSendedAnalysis;
			}

			button_encoder_analyze.Visible = true;
			hbox_encoder_analyze_progress.Visible = false;
			button_encoder_analyze_cancel.Sensitive = false;

			encoder_pulsebar_analyze.Fraction = 1;
			encoderButtonsSensitive(encoderSensEnumStored);
			image_encoder_analyze.Sensitive = true;
			treeview_encoder_analyze_curves.Sensitive = true;

			button_encoder_analyze_image_save.Sensitive = true;
			button_encoder_analyze_image_compujump_send_email.Sensitive = true;
			button_encoder_analyze_AB_save.Sensitive = true;
			button_encoder_analyze_table_save.Sensitive = true;
			
			string my1RMName = Util.FindOnArray(':',1,0,UtilGtk.ComboGetActive(combo_encoder_analyze_1RM),
						encoderAnalyze1RMTranslation);
			button_encoder_analyze_1RM_save.Visible = 
				(radiobutton_encoder_analyze_1RM.Active &&
				(my1RMName == "1RM Bench Press" || my1RMName == "1RM Squat" || my1RMName == "1RM Deadlift" ||
				 my1RMName == "1RM Any exercise" || my1RMName == "1RM Indirect") );
			/*
			 * TODO: currently disabled because 
			 * on_button_encoder_analyze_1RM_save_clicked () reads getExerciseNameFromEncoderTable()
			 * and encoderAnalyzeListStore is not created because "1RM Indirect" 
			 * currently prints no data on OutputData1
			 *
			 * Solution will be to print data there with a new format 
			 * (new columns) like neuromuscular has done
			 */
		}

		treeview_encoder_capture_curves.Sensitive = true;

		//delete the status filenames
		Util.FileDelete(UtilEncoder.GetEncoderStatusTempBaseFileName() + "1.txt");
		Util.FileDelete(UtilEncoder.GetEncoderStatusTempBaseFileName() + "2.txt");
		Util.FileDelete(UtilEncoder.GetEncoderStatusTempBaseFileName() + "3.txt");
		Util.FileDelete(UtilEncoder.GetEncoderStatusTempBaseFileName() + "4.txt");
		Util.FileDelete(UtilEncoder.GetEncoderStatusTempBaseFileName() + "5.txt");
		Util.FileDelete(UtilEncoder.GetEncoderStatusTempBaseFileName() + "6.txt");
			
		if(action == encoderActions.CURVES_AC && preferences.encoderCaptureInfinite && ! encoderProcessFinishContMode)
			on_button_encoder_capture_clicked_do (false);

		//for chronojumpWindowTests
		LogB.Information("finishPulseBar DONE: " + action.ToString());
		if(
				action == encoderActions.LOAD ||	//load 
				action == encoderActions.RECALCULATE ||	//recalculate
				action == encoderActions.CURVES_AC) 	//curves after capture
			chronojumpWindowTestsNext();
	}

	/* 
	 * <---- finishPulsebar ----
	 */

}
