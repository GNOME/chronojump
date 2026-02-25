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
//using Glade;
using System.Diagnostics;  //Stopwatch
using System.Text; //StringBuilder
using System.Threading;


public partial class ChronoJumpWindow 
{
	// at glade ---->
	Gtk.Box box_start_fourPlatforms;
	Gtk.Grid grid_fourPlatforms;
	Gtk.Entry entry_fourPlatforms_port;
	Gtk.Button button_four_platforms_capture_default;
	Gtk.SpinButton spin_four_platforms_capture_n;
	Gtk.Button button_four_platforms_capture_1_2;
	Gtk.Button button_four_platforms_capture_1_3;
	Gtk.Button button_four_platforms_capture_1_4;
	Gtk.Button button_four_platforms_capture_low_high;
	Gtk.Box box_fourPlatforms_capture_buttons;
	Gtk.Box box_fourPlatforms_cancel_finish;
	Gtk.Button button_fourPlatforms_test_finish;
	Gtk.Button button_fourPlatforms_test_cancel;

	//bluetooth
	Gtk.TextView textview_fourPlatforms_bluetooth;
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
	static List<PointF> cairoGraphFourPlatformsStepsBottom_l;
	static List<PointF> cairoGraphFourPlatformsStepsTop_l;

	static FourPlatformsCaptureManage fpcm;
	FourPlatformsCapture fpc;
	private bool fourPlatformsNeedCallApplyCSSExternalWindow;

	TextBuffer tbBluetooth = new TextBuffer (new TextTagTable());

	private FourPlatformsCaptureManage.CaptureEnum fourPlatformsCaptureType;
	private bool fourPlatformsCaptureTwiceDo = false;
	private Stopwatch fourPlatformsCaptureTwiceSw;

	private void fourPlatformsInit1Time ()
	{
		button_four_platforms_capture_default.Label =
			FourPlatformsCaptureManage.CaptureEnumStr (FourPlatformsCaptureManage.CaptureEnum.DEFAULT);
		button_four_platforms_capture_1_2.Label =
			FourPlatformsCaptureManage.CaptureEnumStr (FourPlatformsCaptureManage.CaptureEnum.FROM1TO2);
		button_four_platforms_capture_1_3.Label =
			FourPlatformsCaptureManage.CaptureEnumStr (FourPlatformsCaptureManage.CaptureEnum.FROM1TO3);
		button_four_platforms_capture_1_4.Label =
			FourPlatformsCaptureManage.CaptureEnumStr (FourPlatformsCaptureManage.CaptureEnum.FROM1TO4);
		button_four_platforms_capture_low_high.Label =
			FourPlatformsCaptureManage.CaptureEnumStr (FourPlatformsCaptureManage.CaptureEnum.FROMLOWTOHIGH);

		tbBluetooth.Text = "";
	}

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

		cairoGraphFourPlatformsStepsBottom_l = new List<PointF>();
		cairoGraphFourPlatformsStepsTop_l = new List<PointF>();

		fpcm = new FourPlatformsCaptureManage (current_mode,
				FourPlatformsCaptureManage.CaptureEnum.DEFAULT,
				Convert.ToInt32 (spin_four_platforms_capture_n.Value),
				false,
				new BluetoothDataList (),
				null,
				ref cairoGraphFourPlatformsPoints_ll,
				ref cairoGraphFourPlatformsStepsBottom_l,
				ref cairoGraphFourPlatformsStepsTop_l,
				getSelectedPersonAndNext3 ());
		drawingarea_results_realtime.QueueDraw ();
	}

	private void blankFourPlatformsGraphs ()
	{
		currentFourPlatforms = null;

		//blank Cairo scatterplot graphs
		cairoGraphFourPlatforms = null;
		cairoGraphFourPlatformsPoints_ll = new List<List<PointF>>();
		cairoGraphFourPlatformsPoints_ll.Add (new List<PointF>()); //all buttons
		for (int i = 0; i < 4; i ++)
			cairoGraphFourPlatformsPoints_ll.Add (new List<PointF>()); //button 1

		cairoGraphFourPlatformsStepsBottom_l = new List<PointF>();
		cairoGraphFourPlatformsStepsTop_l = new List<PointF>();
	}

	//note this is used by modes: JUMPSSIMPLE and OTHER (FOURPLATFORMS)
	private void on_four_platforms_capture_clicked (object o)
	{
		fourPlatformsCaptureType = FourPlatformsCaptureManage.CaptureEnum.DEFAULT;
		Gtk.Button b = o as Gtk.Button;
		if (b == button_four_platforms_capture_1_2)
			fourPlatformsCaptureType = FourPlatformsCaptureManage.CaptureEnum.FROM1TO2;
		else if (b == button_four_platforms_capture_1_3)
			fourPlatformsCaptureType = FourPlatformsCaptureManage.CaptureEnum.FROM1TO3;
		else if (b == button_four_platforms_capture_1_4)
			fourPlatformsCaptureType = FourPlatformsCaptureManage.CaptureEnum.FROM1TO4;
		else if (b == button_four_platforms_capture_low_high)
			fourPlatformsCaptureType = FourPlatformsCaptureManage.CaptureEnum.FROMLOWTOHIGH;

		fourPlatformsCaptureTwiceDo = true;
		on_four_platforms_capture_clicked_do ();
	}

	private void on_four_platforms_capture_clicked_do ()
	{
		if (current_mode == Constants.Modes.OTHER)
		{
			box_fourPlatforms_capture_buttons.Sensitive = false;
			box_fourPlatforms_cancel_finish.Sensitive = true;
			//tests 1_2 1_3 1_4 have no finish button as it needs to count 15 (to save correctly from the 1st to the 15th). It will finish automatically
			//button_fourPlatforms_test_finish.Visible = (b == button_four_platforms_capture_default); commented to allow finish work if desired
			button_fourPlatforms_test_finish.Visible = true;

			if (dialogResult != null && dialogResult.Visible)
				dialog_result_set_labels ();
		}

		sensitiveSelectedTestButtons (false);

		capturingFourPlatforms = arduinoCaptureStatus.STARTING;
		LogB.Information ("capturingFourPlatforms: STARTING");

		blankFourPlatformsGraphs ();

		fourPlatformsPulseMessage = "";
		fourPlatformsButtonsSensitive (false);

		button_execute_test.Sensitive = false;
		event_execute_button_cancel.Sensitive = true;

		fourPlatformsProcessFinish = false;
		fourPlatformsProcessCancel = false;
		//fourPlatformsProcessError = false;

		contactsShowCaptureDoingButtons(true);

		//FourPlatforms on other mode has special buttons for finish and cancel with their callbacks
		//but when used on jump mode neeed to use on_finish/cancel_clicked
		if (current_mode == Constants.Modes.JUMPSSIMPLE)
		{
			event_execute_ButtonFinish.Clicked -= new EventHandler (on_finish_clicked);
			event_execute_ButtonFinish.Clicked += new EventHandler (on_finish_clicked);

			event_execute_ButtonCancel.Clicked -= new EventHandler (on_cancel_clicked);
			event_execute_ButtonCancel.Clicked += new EventHandler (on_cancel_clicked);
		}

		blinkCapture = new BlinkImage (image_no_capturing, image_capturing);

		fourPlatformsCaptureThread = new Thread (new ThreadStart (fourPlatformsCaptureDo));
		GLib.Idle.Add (new GLib.IdleHandler (pulseGTKFourPlatformsCapture));

		//mute logs if ! debug mode
		LogB.Mute = ! preferences.debugMode;

		LogB.ThreadStart();
		fourPlatformsCaptureThread.Start();

		//return true; 
	}

	// called on Timeout after a capture depending on config options
	private bool on_four_platforms_capture_clicked_again ()
	{
		fourPlatformsCaptureTwiceDo = false; //to not be called again
		box_fourPlatforms_capture_buttons.Sensitive = false;

		// display countdown and return true
		if (fourPlatformsCaptureTwiceSw.ElapsedMilliseconds <= 9000) // 9s because this is called for the first time 1s later
		{
			event_execute_label_message.Text = string.Format ("Calling again in {0} s",
					Convert.ToInt32 (10 -fourPlatformsCaptureTwiceSw.ElapsedMilliseconds/1000));
			Thread.Sleep (100);
			return true;
		}

		// countdown ended, call capture, and return false
		on_four_platforms_capture_clicked_do ();
		return false;
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

		bool bluetoothUse = false;
		if (bluetoothCapture != null && bluetoothCapture.BluetoothReading)
		{
			bluetoothUse = true;
			fpc = null;
		}
		else if (fpc == null ||
				fpc.PortName != chronopicRegister.GetSelectedForMode (current_mode).Port)
			fpc = new FourPlatformsCapture (
					chronopicRegister.GetSelectedForMode (current_mode).Port);

		fpcm = new FourPlatformsCaptureManage (current_mode,
				fourPlatformsCaptureType,
				Convert.ToInt32 (spin_four_platforms_capture_n.Value),
				bluetoothUse,
				bluetoothCapture.Bd_l, 	// the growing list of data
				fpc,
				ref cairoGraphFourPlatformsPoints_ll,
				ref cairoGraphFourPlatformsStepsBottom_l,
				ref cairoGraphFourPlatformsStepsTop_l,
				getSelectedPersonAndNext3 ());

		if (fpcm.Init ())
		{
			fourPlatformsPulseMessage = capturingMessage;

			LogB.Information ("--- capturingFourPlatforms after init A: " + capturingFourPlatforms.ToString ());
			if (capturingFourPlatforms != arduinoCaptureStatus.STOP) // to fix cancelled just when arduino was starting
			{
				capturingFourPlatforms = arduinoCaptureStatus.CAPTURING;
				fpcm.Capture ();
			}
			LogB.Information ("--- capturingFourPlatforms after init B: " + capturingFourPlatforms.ToString ());
		}
	}

	private void on_button_fourPlatforms_test_finish_clicked (object o, EventArgs args)
	{
		on_finish_clicked_2_other ();

		box_fourPlatforms_capture_buttons.Sensitive = true;
		box_fourPlatforms_cancel_finish.Sensitive = false;
	}
	private void on_button_fourPlatforms_test_cancel_clicked (object o, EventArgs args)
	{
		on_cancel_clicked_2_other ();

		blankFourPlatformsGraphs ();
		box_fourPlatforms_capture_buttons.Sensitive = true;
		box_fourPlatforms_cancel_finish.Sensitive = false;
	}

	private bool pulseGTKFourPlatformsCapture ()
	{
		if(fourPlatformsCaptureThread == null)
		{
			Thread.Sleep (25);
			return true;
		}

		event_execute_label_message.Text = fourPlatformsPulseMessage;
		if (fpcm != null)
		{
			event_execute_label_message.Text += string.Format (" ({0})", fpcm.StepsCompleted);
			if (dialogResult != null)
				dialogResult.UpdateLabelResult (string.Format ("{0} / {1}", fpcm.StepsCompleted, fpcm.StepsTotal));
		}

		if(! fourPlatformsCaptureThread.IsAlive || fourPlatformsProcessFinish || fourPlatformsProcessCancel // || fourPlatformsProcessError) //capture ends
			|| (fpcm != null && fpcm.Finish))
		{
			if (fourPlatformsProcessCancel && fpcm != null)
			{
				event_execute_label_message.Text = "Cancelled.";
				fpcm.Cancel = true;
			}

			//needed to really finish capture and be able to capture a second time
			//this is finish from button
			if (fourPlatformsProcessFinish && fpcm != null)
			{
				event_execute_label_message.Text = "Finished.";
				fpcm.Finish = true;

				if (current_mode == Constants.Modes.JUMPSSIMPLE)
				{
					//insert jumps and get the list of jumps
					List<Jump> jump_l = fourPlatformsInsertToSQLJumpSimple ();

					//get the list of 4 persons
					List<IDName> idName_l = getSelectedPersonAndNext3 ();

					// update treeview using the person names
					foreach (Jump jump in jump_l)
						foreach (IDName idName in idName_l)
							if (idName.UniqueID == jump.PersonID)
								treeViewResultsSession.Add (idName.UniqueID, idName.Name, jump, "");
				}
				else //if (current_mode == Constants.Modes.OTHER)
				{
					fourPlatformsInsertToSQLOther ();
					treeViewResultsSession.Add (currentPerson.UniqueID, currentPerson.Name, currentFourPlatforms, "");
				}

				sensitiveSelectedTestButtons (true);
			}
			//this is finish from arrive to stepsTotal steps
			else if (fpcm != null && fpcm.Finish)
			{
				event_execute_label_message.Text = "Finished.";

				if (current_mode == Constants.Modes.JUMPSSIMPLE)
					fourPlatformsInsertToSQLJumpSimple ();
				else //if (current_mode == Constants.Modes.OTHER)
					fourPlatformsInsertToSQLOther ();

				treeViewResultsSession.Add (currentPerson.UniqueID, currentPerson.Name, currentFourPlatforms, "");

				sensitiveSelectedTestButtons (true);
				box_fourPlatforms_capture_buttons.Sensitive = true;
				box_fourPlatforms_cancel_finish.Sensitive = false;
			}

			blinkCapture.End ();
			capturingFourPlatforms = arduinoCaptureStatus.STOP;
			LogB.Information ("capturingFourPlatforms: STOP");
			showHideBlinkIcon (blinkCapture, false);

			contactsShowCaptureDoingButtons(false);

			LogB.ThreadEnding();
			LogB.Mute = preferences.muteLogs;
			if(! preferences.muteLogs)
				LogB.Information("muteLogs INactive. Logs active active again");
			LogB.ThreadEnded();

			fourPlatformsButtonsSensitive (true);
			hideButtons();

			drawingarea_results_realtime.QueueDraw ();

			if (current_mode == Constants.Modes.JUMPSSIMPLE)
				updateGraphJumpsSimple();
			else //if (current_mode == Constants.Modes.OTHER) //FOURPLATFORMS
				updateGraphFourPlatformsBars ();

			//repeat capture 3s after
			if (configChronojump.FourPlatformsCaptureTwice && fourPlatformsCaptureTwiceDo)
			{
				fourPlatformsCaptureTwiceSw = new Stopwatch ();
				fourPlatformsCaptureTwiceSw.Start ();

				GLib.Timeout.Add (1000, new GLib.TimeoutHandler (on_four_platforms_capture_clicked_again));
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

				if (fourPlatformsNeedCallApplyCSSExternalWindow)
				{
					fourPlatformsNeedCallApplyCSSExternalWindow = false;
					UtilGtk.ApplyCSSExternalWindow ();
				}
			}
		}

		// note textview will not be updated until capture started
		// we can fix this having eg. a FakePulseButton on bluetoothCapture
		if (bluetoothCapture != null && bluetoothCapture.BluetoothReading &&
				bluetoothCapture.Bm_l.CanReadFromList ())
		{
			tbBluetooth.Text += bluetoothCapture.Bm_l.ReadNext ();
			textview_fourPlatforms_bluetooth.Buffer = tbBluetooth;
			UtilGtk.TextViewScrollToEnd (textview_fourPlatforms_bluetooth);
		}

		Thread.Sleep (50);
		//LogB.Information("FourPlatforms:"+ fourPlatformsCaptureThread.ThreadState.ToString());
		return true;
	}

	private List<Jump> fourPlatformsInsertToSQLJumpSimple ()
	{
		double firstFall = 0;
		bool hasFall = currentJumpType.HasFall (configChronojump.Compujump);

		if (hasFall)
			firstFall = (double) extra_window_jumps_spinbutton_fall.Value; //note will be the fall of first jump, if there are more the height of each jump will be used

		SqliteFourPlatformsJumpsSimple sfpjs = new SqliteFourPlatformsJumpsSimple (hasFall);

		return sfpjs.Insert (
				getSelectedPersonAndNext3 (), currentSession.UniqueID,
				currentJumpType.Name,
				fpcm.TimesOff_ll, fpcm.TimesOn_ll, firstFall,  //type, tv, tc, fall
				0, "", -1, false, //weight: TODO
				UtilDate.ToFile(DateTime.Now));
	}
	private void fourPlatformsInsertToSQLOther ()
	{
		int exerciseID = 0;
		if (fourPlatformsCaptureType == FourPlatformsCaptureManage.CaptureEnum.FROM1TO2)
			exerciseID = 1;
		else if (fourPlatformsCaptureType == FourPlatformsCaptureManage.CaptureEnum.FROM1TO3)
			exerciseID = 2;
		else if (fourPlatformsCaptureType == FourPlatformsCaptureManage.CaptureEnum.FROM1TO4)
			exerciseID = 3;
		else if (fourPlatformsCaptureType == FourPlatformsCaptureManage.CaptureEnum.FROMLOWTOHIGH)
			exerciseID = 4;

		currentFourPlatforms = new FourPlatforms (
				-1,
				currentPerson.UniqueID,
				currentSession.UniqueID,
				exerciseID,
				fpcm.TimesOn_ll[0], fpcm.TimesOff_ll[0],
				fpcm.TimesOn_ll[1], fpcm.TimesOff_ll[1],
				fpcm.TimesOn_ll[2], fpcm.TimesOff_ll[2],
				fpcm.TimesOn_ll[3], fpcm.TimesOff_ll[3],
				UtilDate.ToFile (DateTime.Now),
				"", //description
				"", //videoURL,
				fpcm.TimeEnd - fpcm.TimeStart //totalTime
				);

		currentFourPlatforms.InsertSQL (false);
		updatePersonTestsN (false);
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

	private void updateGraphFourPlatformsBars ()
	{
		if(currentPerson == null || currentSession == null)
			return;

		//intializeVariables if not done before
		event_execute_initializeVariables(
			(! cp2016.StoredCanCaptureContacts && ! cp2016.StoredWireless), //is simulated
			currentPerson.UniqueID,
			currentPerson.Name,
			"", //Catalog.GetString("Phases"),  	  //name of the different moments
			Constants.FourPlatformsTable, //tableName
			"" //type
			);

		/*
		string typeTemp = currentEventType.Name;
		if(radio_contacts_graph_allTests.Active)
			typeTemp = "";
			*/
		string typeTemp = "";

		int selectedID = -1;
		if (treeViewResultsSession != null && treeViewResultsSession.EventSelectedID >= 0)
			selectedID = treeViewResultsSession.EventSelectedID;

		PrepareEventGraphFourPlatforms eventGraph = new PrepareEventGraphFourPlatforms(
				1, //unused?
				currentSession.UniqueID,
				currentPerson.UniqueID, radio_contacts_results_personAll.Active,
				-1 * Convert.ToInt32 (spin_resultsSession_limit.Value), //negative: end limit
				//Constants.WiightTable, typeTemp,
				selectedID);

		//if(eventGraph.personMAXAtSQLAllSessions > 0 || eventGraph.runsAtSQL.Count > 0)
		//	PrepareRunSimpleGraph(eventGraph, false); //don't animate

		string personStr = "";
		if(! radio_contacts_results_personAll.Active)
			personStr = currentPerson.Name;

		LogB.Information("drawingarea_results_session == null: ",
			(drawingarea_results_session == null).ToString());

		cairoPaintBarsPre = new CairoPaintBarsFourPlatforms (
				drawingarea_results_session, preferences.fontTypeToGraph(), current_mode,
				personStr, typeTemp, preferences.digitsNumber, currentPerson.UniqueID, radio_resultsSession_bars.Active);

		cairoPaintBarsPre.StoreEventGraphFourPlatforms (eventGraph);
		//PrepareRunSimpleGraph(cairoPaintBarsPre.eventGraphRunsStored, false); //do not need, draw event will graph it:
		drawingarea_results_session.QueueDraw ();
	}

	private void connectWidgetsFourPlatforms (Gtk.Builder builder)
	{
		box_start_fourPlatforms = (Gtk.Box) builder.GetObject ("box_start_fourPlatforms");
		grid_fourPlatforms = (Gtk.Grid) builder.GetObject ("grid_fourPlatforms");
		entry_fourPlatforms_port = (Gtk.Entry) builder.GetObject ("entry_fourPlatforms_port");
		button_four_platforms_capture_default = (Gtk.Button) builder.GetObject ("button_four_platforms_capture_default");
		spin_four_platforms_capture_n = (Gtk.SpinButton) builder.GetObject ("spin_four_platforms_capture_n");
		button_four_platforms_capture_1_2 = (Gtk.Button) builder.GetObject ("button_four_platforms_capture_1_2");
		button_four_platforms_capture_1_3 = (Gtk.Button) builder.GetObject ("button_four_platforms_capture_1_3");
		button_four_platforms_capture_1_4 = (Gtk.Button) builder.GetObject ("button_four_platforms_capture_1_4");
		button_four_platforms_capture_low_high = (Gtk.Button) builder.GetObject ("button_four_platforms_capture_low_high");
		box_fourPlatforms_capture_buttons = (Gtk.Box) builder.GetObject ("box_fourPlatforms_capture_buttons");
		box_fourPlatforms_cancel_finish = (Gtk.Box) builder.GetObject ("box_fourPlatforms_cancel_finish");
		button_fourPlatforms_test_finish = (Gtk.Button) builder.GetObject ("button_fourPlatforms_test_finish");
		button_fourPlatforms_test_cancel = (Gtk.Button) builder.GetObject ("button_fourPlatforms_test_cancel");

		//bluetooth
		textview_fourPlatforms_bluetooth = (Gtk.TextView) builder.GetObject ("textview_fourPlatforms_bluetooth");
	}
}

