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
 * Copyright (C) 2004-2024   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
//using Glade;
using System.Text; //StringBuilder
using System.Threading;


public class FourPlatformsCaptureManage
{
	private FourPlatformsCapture fpc;
	private bool finish;
	private bool cancel;
	//private bool error;

	//private List<PointF> points_l;
	
	public FourPlatformsCaptureManage (
			FourPlatformsCapture fpc//,
			//ref List<PointF> points_l
			)
	{
		this.fpc = fpc;
		//this.points_l = points_l;
	}

	public bool Init ()
	{
		finish = false;
		cancel = false;
		//error = false;

		fpc.Reset ();
		if (! fpc.CaptureStart ())
			return false;

		return true;
	}

	public void Capture ()
	{
		finish = false;
		while (! finish && ! cancel)// && ! error)
		{
			if(! fpc.CaptureSample ())
				cancel = true; //problem reading line (capturing)

			if (fpc.CanReadFromList ())
			{
				FourPlatformsEvent fpe = fpc.FourPlatformsCaptureReadNext();
				LogB.Information("fpe: " + fpe.ToString());
			}
		}
		LogB.Information ("calling Stop");
		fpc.Stop ();
	}

	public bool Finish {
		set { finish = value; }
	}
	public bool Cancel {
		set { cancel = value; }
	}
}

public partial class ChronoJumpWindow 
{
	Thread fourPlatformsCaptureThread;
	static bool fourPlatformsProcessFinish;
	static bool fourPlatformsProcessCancel;
        static string fourPlatformsPulseMessage = "";
	//static bool fourPlatformsProcessError;

	static arduinoCaptureStatus capturingFourPlatforms = arduinoCaptureStatus.STOP;

	static FourPlatformsCaptureManage fpcm;
	FourPlatformsCapture fpc;

	private void on_four_platforms_capture_clicked ()
	{
		capturingFourPlatforms = arduinoCaptureStatus.STARTING;

		fourPlatformsPulseMessage = "";
		fourPlatformsButtonsSensitive (false);

		button_execute_test.Sensitive = false;
		event_execute_button_cancel.Sensitive = true;

		fourPlatformsProcessFinish = false;
		fourPlatformsProcessCancel = false;
		//fourPlatformsProcessError = false;

		contactsShowCaptureDoingButtons(true);

		event_execute_ButtonFinish.Clicked -= new EventHandler(on_finish_clicked);
		event_execute_ButtonFinish.Clicked += new EventHandler(on_finish_clicked);

		event_execute_ButtonCancel.Clicked -= new EventHandler(on_cancel_clicked);
		event_execute_ButtonCancel.Clicked += new EventHandler(on_cancel_clicked);

		blinkCapture = new Blink ();

		fourPlatformsCaptureThread = new Thread (new ThreadStart (fourPlatformsCaptureDo));
		GLib.Idle.Add (new GLib.IdleHandler (pulseGTKFourPlatformsCapture));

		//mute logs if ! debug mode
		LogB.Mute = ! preferences.debugMode;

		LogB.ThreadStart();
		fourPlatformsCaptureThread.Start();
		//return true;
	}

	private void fourPlatformsCaptureDo ()
	{
		fourPlatformsPulseMessage = "Please wait";

		if (fpc == null ||
				fpc.PortName != chronopicRegister.GetSelectedForMode (current_mode).Port)
			fpc = new FourPlatformsCapture (
					chronopicRegister.GetSelectedForMode (current_mode).Port);

		fpcm = new FourPlatformsCaptureManage (
				fpc//,
				//points_l,
				);

		if (fpcm.Init ())
		{
			capturingFourPlatforms = arduinoCaptureStatus.CAPTURING;
			fourPlatformsPulseMessage = capturingMessage;
			fpcm.Capture ();
		}
	}

	private bool pulseGTKFourPlatformsCapture ()
	{
		if(fourPlatformsCaptureThread == null)
		{
			Thread.Sleep (25);
			return true;
		}

		event_execute_label_message.Text = fourPlatformsPulseMessage;
		if(! fourPlatformsCaptureThread.IsAlive || fourPlatformsProcessFinish || fourPlatformsProcessCancel)// || fourPlatformsProcessError) //capture ends
		{
			if (fourPlatformsProcessCancel && fpcm != null)
			{
				event_execute_label_message.Text = "Cancelled.";
				fpcm.Cancel = true;
			}

			//needed to really finish capture and be able  to capture a second time
			if (fourPlatformsProcessFinish && fpcm != null)
			{
				event_execute_label_message.Text = "Finished.";
				fpcm.Finish = true;
			}

			blinkCapture.End ();
			showHideCaptureIcon (false);

			sensitiveLastTestButtons(false);
			contactsShowCaptureDoingButtons(false);
			button_contacts_delete_selected.Sensitive = false;

			LogB.ThreadEnding();
			LogB.Mute = preferences.muteLogs;
			if(! preferences.muteLogs)
				LogB.Information("muteLogs INactive. Logs active active again");
			LogB.ThreadEnded();

			fourPlatformsButtonsSensitive (true);
			hideButtons();

			//drawingarea_race_analyzer_capture_position_time.QueueDraw ();

			return false;
		} else {
			if (capturingFourPlatforms == arduinoCaptureStatus.CAPTURING)
			{
				if (blinkCapture.Status == Blink.StatusEnum.NOTSTARTED)
					blinkCapture.Start (); //TODO: but note here is still connecting
				showHideCaptureIcon (true);

				/*
				drawingarea_race_analyzer_capture_position_time.QueueDraw ();
				drawingarea_race_analyzer_capture_speed_time.QueueDraw ();
				drawingarea_race_analyzer_capture_accel_time.QueueDraw ();
				*/

				if(fourPlatformsPulseMessage == capturingMessage)
					event_execute_button_finish.Sensitive = true;
			}
		}

		Thread.Sleep (50);
		//LogB.Information("FourPlatforms:"+ fourPlatformsCaptureThread.ThreadState.ToString());
		return true;
	}

	private void fourPlatformsButtonsSensitive (bool sensitive)
	{
		//runEncoder related buttons
		//vbox_run_encoder_capture_buttons.Sensitive = sensitive;
		//vbox_run_encoder_capture_options.Sensitive = sensitive;
		frame_contacts_exercise.Sensitive = sensitive;
		button_execute_test.Sensitive = sensitive;
		hbox_contacts_camera.Sensitive = sensitive;

		//other gui buttons
		menus_and_mode_sensitive(sensitive);

		hbox_contacts_sup_capture_analyze_two_buttons.Sensitive = sensitive;
		frame_persons.Sensitive = sensitive;
		hbox_top_person.Sensitive = sensitive;
		hbox_chronopics_and_more.Sensitive = sensitive;
	}
}

