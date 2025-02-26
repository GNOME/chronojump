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
	private Constants.Modes mode;
	private FourPlatformsCapture fpc;
	private bool finish;
	private bool cancel;
	private DateTime timeOfLastCapture; //to show correctly the scroll even with no new data
	//private bool error;
	private List<IDName> idName_l;

	//private List<PointF> points_l;
	private List<List<PointF>> points_ll; //[0] will have all and helps to configureTimeWindow (graphical info)
	private List<List<double>> timesOn_ll; //[0] will have all and helps to configureTimeWindow (time info to sql)
	private List<List<double>> timesOff_ll; //[0] will have all and helps to configureTimeWindow (time info to sql)

	public FourPlatformsCaptureManage (
			Constants.Modes mode,
			FourPlatformsCapture fpc,
			ref List<List<PointF>> points_ll,
			List<IDName> idName_l
			)
	{
		this.mode = mode;
		this.fpc = fpc;
		this.points_ll = points_ll;
		this.idName_l = idName_l;

		timesOn_ll = new List<List<double>>();
		timesOff_ll = new List<List<double>>();

		for (int i = 0; i < 4; i ++)
		{
			timesOn_ll.Add (new List<double> ());
			timesOff_ll.Add (new List<double> ());
		}
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

		List<double> timeAccu_l = new List<double> (); //double to use PointF
		for (int i = 0; i <= 3 ; i ++)
			timeAccu_l.Add (0);

		while (! finish && ! cancel)// && ! error)
		{
			if(! fpc.CaptureSample ())
				cancel = true; //problem reading line (capturing)

			if (fpc.CanReadFromList ())
			{
				FourPlatformsEvent fpe = fpc.FourPlatformsCaptureReadNext();
				LogB.Information("fpe: " + fpe.ToString());

				fpe.Time *= -1; //first buttons prototype board has the buttons behaviour opposite than the final board

				int timeNow = fpe.Time; //millis

				//int button = fpe.Button + 1; //from 0-3 to 1-4
				//have button as positive or negative and put timeNow as positive
				if (timeNow < 0)
					timeNow = Math.Abs (timeNow);

				timeAccu_l[fpe.Button] += timeNow;

				int y = fpe.Button + 1; //1 - 4
				double ySign;

				if (mode == Constants.Modes.JUMPSSIMPLE)
				{
					ySign = 0;
					if (fpe.Time < 0)
						ySign = .4;
				} else { //(mode == Constants.Modes.OTHER)
					ySign = .2;
					if (fpe.Time < 0)
						ySign = -.2;
				}

				if (fpe.Time < 0)
					timesOff_ll[fpe.Button].Add (UtilAll.DivideSafe (timeAccu_l[fpe.Button], 1000)); //0-3 each of the sensors
				else
					timesOn_ll[fpe.Button].Add (UtilAll.DivideSafe (timeAccu_l[fpe.Button], 1000)); //0-3 each of the sensors

				//LogB.Information ("fpe.Button: " + fpe.Button);
				//LogB.Information ("y: " + y);
				//points_ll[0].Add (new PointF (timeAccu_l[fpe.Button], y+ySign)); //0 has all
				//in seconds
				points_ll[0].Add (new PointF (UtilAll.DivideSafe (timeAccu_l[fpe.Button], 1000), .1)); //0 has all //to debug
				points_ll[y].Add (new PointF (UtilAll.DivideSafe (timeAccu_l[fpe.Button], 1000), 5-y+ySign)); //1-4 each of the sensors
				timeOfLastCapture = DateTime.Now;
			}
		}
		LogB.Information ("calling Stop");
		fpc.Stop ();
	}

	public DateTime TimeOfLastCapture {
		get { return timeOfLastCapture; }
	}

	public List<List<double>> TimesOn_ll {
		get { return timesOn_ll; }
	}
	public List<List<double>> TimesOff_ll {
		get { return timesOff_ll; }
	}
	public List<IDName> IDName_l {
		get { return idName_l; }
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
	// at glade ---->
	Gtk.Box box_start_fourPlatforms;
	Gtk.Box box_fourPlatforms;
	// <---- at glade


	Thread fourPlatformsCaptureThread;
	static bool fourPlatformsProcessFinish;
	static bool fourPlatformsProcessCancel;
        static string fourPlatformsPulseMessage = "";
	//static bool fourPlatformsProcessError;

	static arduinoCaptureStatus capturingFourPlatforms = arduinoCaptureStatus.STOP;

	CairoGraphFourPlatforms cairoGraphFourPlatforms;
	//static List<PointF> cairoGraphFourPlatformsPoints_l;
	//for making 4 lines each for a sensor, and being continuous (1) or empty (0)
	//5 PointF lists, 0: have all (to configureTimeWindow), 1-3: each for one button. x=accumulated time, y is on (1) off (0),
	static List<List<PointF>> cairoGraphFourPlatformsPoints_ll;

	static FourPlatformsCaptureManage fpcm;
	FourPlatformsCapture fpc;

	//methods used on discoverWin closed, person changed, and Chronojump start (changeMode)
	private void showHideFourPlatformsJumpsDrawingArea ()
	{
		ChronopicRegisterPort crp = chronopicRegister.GetSelectedForMode (current_mode);
		if (crp.Port != "" && crp.Type == ChronopicRegisterPort.Types.FOURPLATFORMS)
		{
			updateFourPlatformsJumpsPersonNames ();
			align_drawingarea_realtime_capture_cairo.Visible = true;
		} else
			align_drawingarea_realtime_capture_cairo.Visible = false;
	}
	private void updateFourPlatformsJumpsPersonNames ()
	{
		cairoGraphFourPlatformsPoints_ll = new List<List<PointF>>();
		cairoGraphFourPlatformsPoints_ll.Add (new List<PointF>());
		fpcm = new FourPlatformsCaptureManage (current_mode, null, ref cairoGraphFourPlatformsPoints_ll, getSelectedPersonAndNext3 ());
		drawingarea_results_realtime.QueueDraw ();

	}

	private void on_four_platforms_capture_clicked ()
	{
		capturingFourPlatforms = arduinoCaptureStatus.STARTING;

		//blank Cairo scatterplot graphs
		cairoGraphFourPlatforms = null;
		cairoGraphFourPlatformsPoints_ll = new List<List<PointF>>();
		cairoGraphFourPlatformsPoints_ll.Add (new List<PointF>()); //all buttons
		for (int i = 0; i < 4; i ++)
			cairoGraphFourPlatformsPoints_ll.Add (new List<PointF>()); //button 1

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

		blinkCapture = new BlinkImage (image_no_capturing, image_capturing);

		fourPlatformsCaptureThread = new Thread (new ThreadStart (fourPlatformsCaptureDo));
		GLib.Idle.Add (new GLib.IdleHandler (pulseGTKFourPlatformsCapture));

		//mute logs if ! debug mode
		LogB.Mute = ! preferences.debugMode;

		LogB.ThreadStart();
		fourPlatformsCaptureThread.Start();
		//return true;
	}

	private List<IDName> getSelectedPersonAndNext3 ()
	{
		int currentPersonRow = myTreeViewPersons.FindRow (currentPerson.UniqueID);
		List<IDName> p_l = new List<IDName> ();
		for (int i = currentPersonRow; i < currentPersonRow + 4; i ++)
			p_l.Add (myTreeViewPersons.GetPersonByRow (i));

		return p_l;
	}

	private void fourPlatformsCaptureDo ()
	{
		fourPlatformsPulseMessage = "Please wait";

		if (fpc == null ||
				fpc.PortName != chronopicRegister.GetSelectedForMode (current_mode).Port)
			fpc = new FourPlatformsCapture (
					chronopicRegister.GetSelectedForMode (current_mode).Port);

		fpcm = new FourPlatformsCaptureManage (current_mode, fpc, ref cairoGraphFourPlatformsPoints_ll, getSelectedPersonAndNext3 ());

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

				fourPlatformsInsertToSQL ();
			}

			blinkCapture.End ();
			capturingFourPlatforms = arduinoCaptureStatus.STOP;
			showHideBlinkIcon (blinkCapture, false);

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

			drawingarea_results_realtime.QueueDraw ();

			if (current_mode == Constants.Modes.JUMPSSIMPLE)
			{
				pre_fillTreeView_jumps (false);
				updateGraphJumpsSimple();
			}

			return false;
		} else {
			if (capturingFourPlatforms == arduinoCaptureStatus.CAPTURING)
			{
				if (blinkCapture.Status == Blink.StatusEnum.NOTSTARTED)
					blinkCapture.Start (); //TODO: but note here is still connecting
				showHideBlinkIcon (blinkCapture, true);

				drawingarea_results_realtime.QueueDraw ();

				if(fourPlatformsPulseMessage == capturingMessage)
					event_execute_button_finish.Sensitive = true;
			}
		}

		Thread.Sleep (50);
		//LogB.Information("FourPlatforms:"+ fourPlatformsCaptureThread.ThreadState.ToString());
		return true;
	}

	private void fourPlatformsInsertToSQL ()
	{
		if (current_mode == Constants.Modes.JUMPSSIMPLE)
			fourPlatformsInsertToSQLJumpSimple ();
		else if (current_mode == Constants.Modes.OTHER)
			fourPlatformsInsertToSQLOther ();
	}
	private void fourPlatformsInsertToSQLJumpSimple ()
	{
		double firstFall = 0;
		bool hasFall = currentJumpType.HasFall (configChronojump.Compujump);

		if (hasFall)
			firstFall = (double) extra_window_jumps_spinbutton_fall.Value; //note will be the fall of first jump, if there are more the height of each jump will be used

		SqliteFourPlatformsJumpsSimple sfpjs = new SqliteFourPlatformsJumpsSimple (hasFall);

		sfpjs.Insert (
				getSelectedPersonAndNext3 (), currentSession.UniqueID,
				currentJumpType.Name,
				fpcm.TimesOff_ll, fpcm.TimesOn_ll, firstFall,  //type, tv, tc, fall
				0, "", -1, false, //weight: TODO
				UtilDate.ToFile(DateTime.Now));
	}
	private void fourPlatformsInsertToSQLOther ()
	{
		string insertString = "(NULL, " +
			currentPerson.UniqueID + ", " +
			currentSession.UniqueID + ", " +
			"0, '" + //exerciseID
			UtilDate.ToFile (DateTime.Now) + "', '" +
			Util.ConvertToPoint (UtilList.ListDoubleToString (fpcm.TimesOn_ll[0], 3, "="))  + "', '" +
			Util.ConvertToPoint (UtilList.ListDoubleToString (fpcm.TimesOff_ll[0], 3, "=")) + "', '" +
			Util.ConvertToPoint (UtilList.ListDoubleToString (fpcm.TimesOn_ll[1], 3, "="))  + "', '" +
			Util.ConvertToPoint (UtilList.ListDoubleToString (fpcm.TimesOff_ll[1], 3, "=")) + "', '" +
			Util.ConvertToPoint (UtilList.ListDoubleToString (fpcm.TimesOn_ll[2], 3, "="))  + "', '" +
			Util.ConvertToPoint (UtilList.ListDoubleToString (fpcm.TimesOff_ll[2], 3, "=")) + "', '" +
			Util.ConvertToPoint (UtilList.ListDoubleToString (fpcm.TimesOn_ll[3], 3, "="))  + "', '" +
			Util.ConvertToPoint (UtilList.ListDoubleToString (fpcm.TimesOff_ll[3], 3, "=")) + "', " +
			"'', '', 0)"; //comments, videoURL, totalTime

		SqliteFourPlatforms.Insert (false, insertString);
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

	private void connectWidgetsFourPlatforms (Gtk.Builder builder)
	{
		box_start_fourPlatforms = (Gtk.Box) builder.GetObject ("box_start_fourPlatforms");
		box_fourPlatforms = (Gtk.Box) builder.GetObject ("box_fourPlatforms");
	}
}

