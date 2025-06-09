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
 * Copyright (C) 2004-2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Gtk;
//using Glade;
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using System.Collections.Generic; //List
using Mono.Unix;
using Gdk; //for the EventMask



//--------------------------------------------------------
//---------------- this WIDGET ---------------------------
//---------------- is included in main gui ---------------
//---------------- since 0.9.3 ---------------------------
//--------------------------------------------------------


public partial class ChronoJumpWindow 
{
	Gtk.Box box_event_execute_label_message;
	Gtk.Label event_execute_label_phases_name;
	Gtk.Label event_execute_label_message;
	Gtk.Image image_no_capturing;
	Gtk.Image image_capturing;
	Gtk.Image image_capturing_blue; //tare+capture
	Gtk.Image image_no_capturing_encoder;
	Gtk.Image image_capturing_encoder;
	Gtk.Image image_force_sensor_adjust_no_capturing;
	Gtk.Image image_force_sensor_adjust_capturing;

	Gtk.SpinButton spin_contacts_graph_last_limit;
	Gtk.Box box_contacts_simple_graph_controls;
	Gtk.Box box_contacts_graph_exercise;
	Gtk.Box box_contacts_graph_show_graph_table;
	Gtk.RadioButton radio_contacts_graph_currentTest;
	Gtk.RadioButton radio_contacts_graph_allTests;
	//Gtk.RadioButton radio_contacts_results_personCurrent;
	Gtk.RadioButton radio_contacts_results_personAll;
	Gtk.Image image_radio_contacts_results_personCurrent;
	Gtk.Image image_radio_contacts_results_personAll;
	Gtk.CheckButton check_run_show_time;
	
	Gtk.ProgressBar event_execute_progressbar_event;
	Gtk.ProgressBar event_execute_progressbar_time;
	

	//currently gtk-sharp cannot display a label in a progressBar in activity mode (Pulse() not Fraction)
	//then we show the value in a label:
	Gtk.Label event_execute_label_event_value;
	Gtk.Label event_execute_label_time_value;
	
	Gtk.Button event_execute_button_cancel;
	Gtk.Button event_execute_button_finish;

	//removed on gtk3 migration as pulses are not used since some years
	//Gtk.Table event_execute_table_pulse;
	//Gtk.Table event_execute_table_pulse_values;
	
//	Gtk.Alignment align_check_vbox_contacts_graph_legend;
//	Gtk.CheckButton check_vbox_contacts_graph_legend;
//	Gtk.VBox vbox_contacts_graph_legend;

	//for the color change in the background of the cell label
	//Gtk.EventBox event_execute_eventbox_pulse_time;
	//Gtk.Label event_execute_label_pulse_now;
	//Gtk.Label event_execute_label_pulse_avg;

	Gtk.Notebook notebook_results_data;

	Gtk.Box box_capture_current;
	Gtk.Box box_capture_current_forceSensor;
	Gtk.HBox hbox_capture_current_runEncoder;
	Gtk.Alignment align_drawingarea_realtime_capture_cairo;
	Gtk.DrawingArea drawingarea_results_realtime;
	Gtk.DrawingArea drawingarea_results_session;
	Gtk.VBox vbox_event_execute_drawingarea_run_interval_realtime_capture_cairo;
	Gtk.CheckButton check_runI_realtime_rel_abs;
	Gtk.Image image_check_runI_realtime_rel_abs;
	Gtk.DrawingArea drawingarea_run_simple_double_contacts;
	Gtk.Label label_run_simple_double_contacts;
	/*
	Gtk.Box hbox_combo_graph_results_width;
	Gtk.Box hbox_combo_graph_results_height;
	Gtk.ComboBoxText combo_graph_results_width;
	Gtk.ComboBoxText combo_graph_results_height;
	*/

	string event_execute_label_simulated;
	//int sessionID;
	//string event_execute_personName;	
	string event_execute_tableName;	
	//string event_execute_eventType;	
	
	//double event_execute_limit;
	
	private enum phasesGraph {
		UNSTARTED, DOING, DONE
	}
	
	//we need both working to be able to correctly expose_event (draw) on jumpRj, runI
	CairoPaintBarsPre cairoPaintBarsPre;  //used for session results: treeviewResults
	CairoPaintBarsPre cairoPaintBarsPreCurrent; //used for current set: jumpRj/runI capture, encoder
	CairoManageRunDoubleContacts cairoManageRunDoubleContacts;


	private void event_execute_initializeVariables (
			bool simulated,
			int personID,
			string personName,
			string phasesName, 
			string tableName,
			string event_execute_eventType
			) 
	{
		eventExecuteHideAllTables();

		event_execute_label_simulated = "";
		if(simulated) 
			event_execute_label_simulated = "(" + Catalog.GetString("Simulated") + ")";

		event_execute_label_message.Text = "";

		//this.event_execute_personName.Text = event_execute_personName; 	//"Jumps" (rjInterval), "Runs" (runInterval), "Ticks" (pulses), 
		this.event_execute_label_phases_name.Text = phasesName; 	//"Jumps" (rjInterval), "Runs" (runInterval), "Ticks" (pulses), 
								//"Phases" (simple jumps, dj, simple runs)
		this.event_execute_tableName = tableName;

		//this.event_execute_eventType = event_execute_eventType;

		//finish not sensitive for all events. 
		//Later reactive, interval and pulse will sensitive it when a subevent is done
		event_execute_button_finish.Sensitive = false;
		fullscreen_button_fullscreen_contacts.Sensitive = false;

		if(event_execute_tableName == Constants.JumpTable) {
			showJumpSimpleLabels();
		} else if(event_execute_tableName == Constants.JumpRjTable) {
			showJumpReactiveLabels();
		} else if(event_execute_tableName == Constants.RunTable) {
			showRunSimpleLabels();
		} else if(event_execute_tableName == Constants.RunIntervalTable) {
			showRunIntervalLabels();
		}

		clearProgressBars();
	
		//event_execute_eventbox_pulse_time.OverrideBackgroundColor (Gtk.StateFlags.Normal,
		//		UtilGtk.GetRGBA (UtilGtk.Colors.BLUE_PLOTS)); //only one serie in pulse, leave blue
	}
	private ExecutingGraphData event_execute_prepareForTest () 
	{
		checkbutton_video_contacts.Sensitive = false;

		ExecutingGraphData executingGraphData = new ExecutingGraphData(
				event_execute_button_cancel, event_execute_button_finish, 
				event_execute_label_message,  
				event_execute_label_event_value,  event_execute_label_time_value,
				label_video_feedback,
				event_execute_progressbar_event,  event_execute_progressbar_time);
		
		return executingGraphData;
	}

	private void eventExecutePutNonStandardIcons() {
	}

	private void eventExecuteHideImages() {
	}

	private void eventExecuteHideAllTables() 
	{
		//hide pulse info
		//event_execute_table_pulse.Hide();
		//event_execute_table_pulse_values.Hide();
	}
	
	private void showJumpSimpleLabels() 
	{
		box_contacts_simple_graph_controls.Visible = true;
		check_run_show_time.Visible = false;

//		align_check_vbox_contacts_graph_legend.Visible = true;
		//vbox_contacts_graph_legend.Visible = false;

		notebook_results_data.Visible = false;
	}
	
	
	private void showJumpReactiveLabels() 
	{
		box_contacts_simple_graph_controls.Visible = true;
		check_run_show_time.Visible = false;

//		align_check_vbox_contacts_graph_legend.Visible = false;
//		vbox_contacts_graph_legend.Visible = false;

		notebook_results_data.Visible = false;
	}
	
	private void showRunSimpleLabels() 
	{
		box_contacts_simple_graph_controls.Visible = true;
		check_run_show_time.Visible = true;

//		align_check_vbox_contacts_graph_legend.Visible = true;
		//vbox_contacts_graph_legend.Visible = false;

		notebook_results_data.Visible = false;
	}
		
	private void showRunIntervalLabels() 
	{
		box_contacts_simple_graph_controls.Visible = true;
		check_run_show_time.Visible = true;

//		align_check_vbox_contacts_graph_legend.Visible = false;
//		vbox_contacts_graph_legend.Visible = false;
	}
	
	private void clearProgressBars() 
	{
		event_execute_progressbar_event.Fraction = 0;
		event_execute_progressbar_event.Text = "";
		event_execute_progressbar_time.Fraction = 0;
		event_execute_progressbar_time.Text = "";
	
		//clear also the close labels
		event_execute_label_event_value.Text = "";
		event_execute_label_time_value.Text = "";
	}

	//realtime capture graph for jumpRj and runInterval
	public void on_drawingarea_results_realtime_draw (object o, Gtk.DrawnArgs args)
	{
		//right now only for jump simple (fourPlatforms), reactive, runsI, other (fourplatforms)
		if(current_mode != Constants.Modes.JUMPSSIMPLE &&
				current_mode != Constants.Modes.JUMPSREACTIVE &&
				current_mode != Constants.Modes.RUNSINTERVALLIC &&
				current_mode != Constants.Modes.WILIGHT &&
				current_mode != Constants.Modes.OTHER)
			return;

		if(current_mode == Constants.Modes.JUMPSREACTIVE)
		{
			if(currentEventExecute != null && currentEventExecute.IsThreadRunning())
			{
				if(currentEventExecute.PrepareEventGraphJumpReactiveRealtimeCaptureObject != null)
					PrepareJumpReactiveRealtimeCaptureGraph(
							currentEventExecute.PrepareEventGraphJumpReactiveRealtimeCaptureObject.lastTv,
							currentEventExecute.PrepareEventGraphJumpReactiveRealtimeCaptureObject.lastTc,
							currentEventExecute.PrepareEventGraphJumpReactiveRealtimeCaptureObject.tvString,
							currentEventExecute.PrepareEventGraphJumpReactiveRealtimeCaptureObject.tcString,
							currentEventExecute.PrepareEventGraphJumpReactiveRealtimeCaptureObject.type,
							currentPerson.Name,
							preferences.volumeOn, preferences.gstreamer, feedbackJumpsRj,
							preferences.heightPreferred);
			}
			else if(selectedJumpRj != null)
				PrepareJumpReactiveRealtimeCaptureGraph (selectedJumpRj.tvLast, selectedJumpRj.tcLast,
						selectedJumpRj.TvString, selectedJumpRj.TcString,
						selectedJumpRj.Type, selectedJumpRj.Description, //Description is person.Name
						preferences.volumeOn, preferences.gstreamer, feedbackJumpsRj,
						preferences.heightPreferred);
		} else if (current_mode == Constants.Modes.RUNSINTERVALLIC)
		{
			if(currentEventExecute != null && currentEventExecute.IsThreadRunning())
			{
				if(currentEventExecute.PrepareEventGraphRunIntervalRealtimeCaptureObject != null)
					PrepareRunIntervalRealtimeCaptureGraph(
							currentEventExecute.PrepareEventGraphRunIntervalRealtimeCaptureObject.timesString,
							currentEventExecute.PrepareEventGraphRunIntervalRealtimeCaptureObject.distanceInterval,
							currentEventExecute.PrepareEventGraphRunIntervalRealtimeCaptureObject.distancesString,
							currentEventExecute.PrepareEventGraphRunIntervalRealtimeCaptureObject.photocell_l,
							currentEventExecute.PrepareEventGraphRunIntervalRealtimeCaptureObject.type,
							currentPerson.Name, feedbackRunsI);
			}
			else if(selectedRunInterval != null)
			{
				LogB.Information("selectedRunInterval: " + selectedRunInterval.ToString());
				PrepareRunIntervalRealtimeCaptureGraph(
						selectedRunInterval.IntervalTimesString,
						selectedRunInterval.DistanceInterval,
						selectedRunIntervalType.DistancesString,
						selectedRunInterval.Photocell_l,
						selectedRunInterval.Type, selectedRunInterval.Description, feedbackRunsI); //Description is person.Name
			}
		} else if(current_mode == Constants.Modes.JUMPSSIMPLE || current_mode == Constants.Modes.OTHER)
		{
			if(cairoGraphFourPlatforms == null)// || forceRedraw)
				cairoGraphFourPlatforms = new CairoGraphFourPlatforms (drawingarea_results_realtime);

			List<IDName> idName_l = new List<IDName> ();
			if (fpcm != null)
				idName_l = fpcm.IDName_l;

			if (cairoGraphFourPlatformsPoints_ll != null &&
					cairoGraphFourPlatformsStepsBottom_l != null &&
					cairoGraphFourPlatformsStepsTop_l != null)
			{
				FourPlatformsCaptureManage.CaptureEnum fpct = FourPlatformsCaptureManage.CaptureEnum.DEFAULT;
				if (capturingFourPlatforms == arduinoCaptureStatus.CAPTURING)
					fpct = fourPlatformsCaptureType;
				else if (currentFourPlatforms != null)
					fpct = currentFourPlatforms.GetCaptureEnum ();

				cairoGraphFourPlatforms.DoSendingList (
						preferences.fontTypeToGraph(),
						current_mode,
						fpct.ToString (),
						cairoGraphFourPlatformsPoints_ll,
						cairoGraphFourPlatformsStepsBottom_l,
						cairoGraphFourPlatformsStepsTop_l,
						idName_l,
						fpct,
						capturingFourPlatforms == arduinoCaptureStatus.CAPTURING,
						false, 0,
						10, //but if no capturing it will be -1 (all set)
						true, CairoXY.PlotTypes.POINTSFILL);
			}
		} else if(current_mode == Constants.Modes.WILIGHT)
		{
			if(cairoGraphWilight == null)// || forceRedraw)
				cairoGraphWilight = new CairoGraphWilight (
						drawingarea_results_realtime, "title");

			WilightCommandToTerminals wctt = new WilightCommandToTerminals (
					currentWilightCommand, wilightTerminalLayout);
			cairoGraphWilight.DoSendingList (preferences.fontTypeToGraph(), wctt.Do (), true);
		}
	}

	//barplot of tests in session
	public void on_drawingarea_results_session_draw (object o, Gtk.DrawnArgs args)
	{
		drawingarea_results_session.AddEvents((int) Gdk.EventMask.ButtonPressMask);

		//right now only for jumps/runs simple
		if (current_mode != Constants.Modes.JUMPSSIMPLE &&
				current_mode != Constants.Modes.JUMPSREACTIVE &&
				current_mode != Constants.Modes.RUNSSIMPLE &&
				current_mode != Constants.Modes.RUNSINTERVALLIC &&
				current_mode != Constants.Modes.RUNSENCODER &&
				current_mode != Constants.Modes.WILIGHT &&
				current_mode != Constants.Modes.OTHER && //FOURPLATFORMS
				! Constants.ModeIsFORCESENSOR (current_mode) &&
				! Constants.ModeIsENCODER (current_mode))
			return;

		//if object not defined or not defined fo this mode, return
		if(cairoPaintBarsPre == null || ! cairoPaintBarsPre.ModeMatches (current_mode))
			return;

		//cairoPaintBarsPre.Prepare();
		if(current_mode == Constants.Modes.JUMPSSIMPLE)
			PrepareJumpSimpleGraph (cairoPaintBarsPre.eventGraphJumpsStored, false);
		else if(current_mode == Constants.Modes.JUMPSREACTIVE)
			PrepareJumpReactiveGraph (cairoPaintBarsPre.eventGraphJumpsRjStored, false);
		else if (current_mode == Constants.Modes.RUNSSIMPLE)
			PrepareRunSimpleGraph (cairoPaintBarsPre.eventGraphRunsStored, false);
		else if (current_mode == Constants.Modes.RUNSINTERVALLIC)
			PrepareRunIntervalGraph (cairoPaintBarsPre.eventGraphRunsIntervalStored, false);
		else if (
				current_mode == Constants.Modes.RUNSENCODER ||
				current_mode == Constants.Modes.WILIGHT ||
				current_mode == Constants.Modes.OTHER || //FOURPLATFORMS
				Constants.ModeIsFORCESENSOR (current_mode) ||
				Constants.ModeIsENCODER (current_mode))
			PrepareResultsSessionGraph ();
	}

	public void on_drawingarea_run_simple_double_contacts_cairo_draw (object o, Gtk.DrawnArgs args)
	{
		if(current_mode != Constants.Modes.RUNSSIMPLE &&
				current_mode != Constants.Modes.RUNSINTERVALLIC)
			return;

		//if object not defined or not defined fo this mode, return
		if(cairoManageRunDoubleContacts == null)
			return;

		if (current_mode == Constants.Modes.RUNSSIMPLE)
			PrepareRunDoubleContactsGraph (true);
		else if (current_mode == Constants.Modes.RUNSINTERVALLIC)
			PrepareRunDoubleContactsGraph (false);
	}

	// Important! see: diagrams/processes/person_results_changes.dia
	private void on_drawingarea_results_session_button_press_event (object o, ButtonPressEventArgs args)
	{
		LogB.Information("on_drawingarea_results_session_button_press_event");
		if (
				current_mode != Constants.Modes.JUMPSSIMPLE &&
				current_mode != Constants.Modes.JUMPSREACTIVE &&
				current_mode != Constants.Modes.RUNSSIMPLE &&
				current_mode != Constants.Modes.RUNSINTERVALLIC &&
				current_mode != Constants.Modes.RUNSENCODER &&
				current_mode != Constants.Modes.WILIGHT &&
				current_mode != Constants.Modes.OTHER && //FOURPLATFORMS
				! Constants.ModeIsFORCESENSOR (current_mode) &&
				! Constants.ModeIsENCODER (current_mode))
			return;

		if(cairoPaintBarsPre == null)
			return;

		//int bar = cairoPaintBarsPre.FindBarInPixel(args.Event.X);
		//LogB.Information("Bar: " + bar.ToString());
		int id = cairoPaintBarsPre.FindBarIdInPixel (args.Event.X, args.Event.Y);
		LogB.Information("id: " + id.ToString());

		if(id < 0)
			return;

		if (treeViewResultsSession == null)
			return;

		if (current_mode == Constants.Modes.RUNSSIMPLE)
			button_inspect_last_test_run_simple.Sensitive = false;

		// on encoder the bars are reps, but the treeview selection is going to be a set
		if (Constants.ModeIsENCODER (current_mode))
		{
			ArrayList array = SqliteEncoderSignalCurve.SelectSignalCurve (false, -1, id, -1, -1);
			if (array.Count > 0)
				id = ((EncoderSignalCurve) array[0]).signalID;
		}

		selectResultsSessionId (id, true);
	}

	// simple and DJ jump	
	public void PrepareJumpSimpleGraph (PrepareEventGraphJumpSimple eventGraph, bool animate)
	{
		/*
		 * if not dj show heights
		 * and it is a single jump type, and it has tc, tv (it is a dj or similar)
		 * then show tc, tf
		 */
		if (eventGraph == null)
			return;

		bool useHeights = true;
		if(! eventGraph.djShowHeights &&
				eventGraph.type != "" && //it is a concrete type, not all jumps
				eventGraph.jumpsAtSQL.Count > 0 &&
				eventGraph.jumpsAtSQL[0].Tc > 0 &&
				eventGraph.jumpsAtSQL[0].Tv > 0
				)
			useHeights = false;

		// B) Paint cairo graph
		cairoPaintBarsPre.ShowPersonNames = radio_contacts_results_personAll.Active;
		cairoPaintBarsPre.UseHeights = useHeights;

		cairoPaintBarsPre.Paint();
	}

	private void on_button_person_max_all_sessions_info_clicked(object o, EventArgs args) 
	{
		/*
		string [] str;
		string testName;
		if(current_mode == Constants.Modes.JUMPSSIMPLE) {
			str = SqliteJump.SelectTestMaxStuff(currentPerson.UniqueID, currentJumpType); 
			testName = currentJumpType.Name;
		}
		else if(current_mode == Constants.Modes.RUNSSIMPLE) {
			str = SqliteRun.SelectTestMaxStuff(currentPerson.UniqueID, currentRunType); 
			testName = currentRunType.Name;
		} else
			return;

		if(str[2] == "" || str[2] == "0")
			new DialogMessage(Constants.MessageTypes.WARNING, Catalog.GetString("Missing data."));
		else {
			string message = string.Format(Catalog.GetString("Best {0} test of person {1} is {2}\nDone at session {3} ({4})"),
					testName, currentPerson.Name, 
					Util.TrimDecimals(Util.ChangeDecimalSeparator(str[2]), 3), 
					str[1], str[0]);
			if(str[3] == "-1")
				message += "\n" + Catalog.GetString("Simulated");

			new DialogMessage(Constants.MessageTypes.INFO, message);
		}
		*/
	}

	private void on_check_vbox_contacts_graph_legend_clicked (object o, EventArgs args)
	{
		/*
		LogB.Information("on_check_vbox_contacts_graph_legend_clicked (), check active: " +
				check_vbox_contacts_graph_legend.Active.ToString());

		if(check_vbox_contacts_graph_legend.Active)
			vbox_contacts_graph_legend.Visible = true;
		else
			vbox_contacts_graph_legend.Visible = false;
			*/
	}

	public void PrepareJumpReactiveGraph (PrepareEventGraphJumpReactive eventGraph, bool animate)
	{
		// Paint cairo graph
		cairoPaintBarsPre.ShowPersonNames = radio_contacts_results_personAll.Active;

		bool useHeights = false;
		if (eventGraph != null && eventGraph.showHeights)
			useHeights = true;

		cairoPaintBarsPre.UseHeights = useHeights;

		cairoPaintBarsPre.Paint();
	}

	private void on_check_runI_realtime_rel_abs_toggled (object o, EventArgs args)
	{
		// 1) change icon
		if(check_runI_realtime_rel_abs.Active)
			image_check_runI_realtime_rel_abs.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "bar_relative.png");
		else
			image_check_runI_realtime_rel_abs.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "bar_absolute.png");

		// 2) redo graph
		on_drawingarea_results_realtime_draw (new object(), new Gtk.DrawnArgs());
	}

	// Reactive jump 
	public void blankJumpReactiveRealtimeCaptureGraph ()
	{
		//constructor for showing a blank graph
		cairoPaintBarsPreCurrent = new CairoPaintBarsPreJumpReactiveRealtimeCapture(
				drawingarea_results_realtime, preferences.fontTypeToGraph());
	}

	public void PrepareJumpReactiveRealtimeCaptureGraph (double lastTv, double lastTc, string tvString, string tcString,
			string type, string personName,
			bool volumeOn, Preferences.GstreamerTypes gstreamer, FeedbackJumpsRj feedbackJumpsRj,
			bool useHeights)
	{
		if(currentPerson == null)
			return;

		bool isLastCaptured = false;
		if(currentEventExecute != null && currentEventExecute.IsThreadRunning()) //during the capture
			isLastCaptured = true;
		else if(currentJumpRj != null && selectedJumpRj != null &&
				currentJumpRj.UniqueID == selectedJumpRj.UniqueID) //selected == last captured
			isLastCaptured = true;

		double videoTime = 0;
		if (webcamPlay != null && webcamPlay.PlayVideoGetSecond > 0)
			videoTime = webcamPlay.PlayVideoGetSecond -diffVideoVsSignal;

		cairoPaintBarsPreCurrent = new CairoPaintBarsPreJumpReactiveRealtimeCapture(
				drawingarea_results_realtime, preferences.fontTypeToGraph(), current_mode,
				personName, type, preferences.digitsNumber,// preferences.heightPreferred,
				//lastTv, lastTc,
				tvString, tcString, isLastCaptured, feedbackJumpsRj, videoTime);

		cairoPaintBarsPreCurrent.UseHeights = useHeights;

		// B) Paint cairo graph
		cairoPaintBarsPreCurrent.Paint();
	}
	
	//identify which subjump is the best or the worst in tv/tc index	
	private int bestOrWorstTvTcIndex(bool isBest, string tvString, string tcString) 
	{
		string [] myTVStringFull = tvString.Split(new char[] {'='});
		string [] myTCStringFull = tcString.Split(new char[] {'='});
		double myTVDouble = 0;
		double myTCDouble = 0;
		double maxTvTc = 0;
		double minTvTc = 100000;
		int count = 0;
		int posSelected = 0;

		foreach (string myTV in myTVStringFull) {
			myTVDouble = Convert.ToDouble(myTV);
			myTCDouble = Convert.ToDouble(myTCStringFull[count]);
			if(myTCDouble > 0) {
				if(isBest) {
					if(myTVDouble / myTCDouble > maxTvTc) {
						maxTvTc = myTVDouble / myTCDouble;
						posSelected = count;
					}
				}
				else {
					if(myTVDouble / myTCDouble < minTvTc) {
						minTvTc = myTVDouble / myTCDouble;
						posSelected = count;
					}
				}
			}

			count ++;
		}
		return posSelected; 
	}
			

	/*
	// run simple
	// called from srg/gui/run updateGraphRunsSimple ()
	public void PrepareRunSimpleGraph(PrepareEventGraphRunSimple eventGraph, bool animate)
	{
		PrepareRunSimpleGraph(eventGraph, animate, null);
	}
	*/

	//standard call
	public void PrepareRunSimpleGraph(PrepareEventGraphRunSimple eventGraph, bool animate)
	{
		LogB.Information("cairoPaintBarsPre == null: ", (cairoPaintBarsPre == null).ToString());

		// Paint cairo graph
		cairoPaintBarsPre.ShowPersonNames = radio_contacts_results_personAll.Active;
		cairoPaintBarsPre.RunsShowTime = check_run_show_time.Active;
		cairoPaintBarsPre.Paint();
	}
	public void PrepareRunDoubleContactsGraph(bool simple)
	{
		LogB.Information("cairoManageRunDoubleContacts == null: ", (cairoManageRunDoubleContacts == null).ToString());

		// prepare runPTL stuff

		RunPhaseTimeList runPTL = currentEventExecute.RunPTL;
		//LogB.Information(string.Format("runPTL is null: {0}", (runPTL == null)));

		if(runPTL == null || ! runPTL.UseDoubleContacts())
			return;

		if(currentEventExecute == null)
			return;

		double timeTotal = 0;
		string intervalTimesString = "";
		if(simple)
		{
			if (currentEventExecute.PrepareEventGraphRunSimpleObject == null)
				return;

			timeTotal = currentEventExecute.PrepareEventGraphRunSimpleObject.time; //TODO: check problems on deleting last test, or changing mode
		} else {
			if (currentEventExecute.PrepareEventGraphRunIntervalRealtimeCaptureObject == null)
				return;

			timeTotal = Util.GetTotalTime(currentEventExecute.PrepareEventGraphRunIntervalRealtimeCaptureObject.timesString);
			intervalTimesString = currentEventExecute.PrepareEventGraphRunIntervalRealtimeCaptureObject.timesString;
		}

		// Paint cairo graph
		cairoManageRunDoubleContacts.Paint(currentEventExecute, runPTL, timeTotal, intervalTimesString);
	}

	public void PrepareRunIntervalGraph(PrepareEventGraphRunInterval eventGraph, bool animate)
	{
		// Paint cairo graph
		cairoPaintBarsPre.ShowPersonNames = radio_contacts_results_personAll.Active;
		cairoPaintBarsPre.RunsShowTime = check_run_show_time.Active;
		cairoPaintBarsPre.Paint();
	}

	public void blankRunIntervalRealtimeCaptureGraph ()
	{
		//constructor for showing a blank graph
		cairoPaintBarsPreCurrent = new CairoPaintBarsPreRunIntervalRealtimeCapture(
				drawingarea_results_realtime, preferences.fontTypeToGraph());
	}

	public void PrepareRunIntervalRealtimeCaptureGraph (string timesString,
			double distanceInterval, string distancesString,
			List<int> photocell_l, string type, string personName, FeedbackRunsInterval feedbackRunsI)
	{
		if(currentPerson == null)
			return;

		bool isLastCaptured = false;
		if(currentEventExecute != null && currentEventExecute.IsThreadRunning()) //during the capture
			isLastCaptured = true;
		else if(currentRunInterval != null && selectedRunInterval != null &&
				currentRunInterval.UniqueID == selectedRunInterval.UniqueID) //selected == last captured
			isLastCaptured = true;

		double videoTime = 0;
		if (webcamPlay != null && webcamPlay.PlayVideoGetSecond > 0)
			videoTime = webcamPlay.PlayVideoGetSecond -diffVideoVsSignal;

		cairoPaintBarsPreCurrent = new CairoPaintBarsPreRunIntervalRealtimeCapture(
				drawingarea_results_realtime, preferences.fontTypeToGraph(), current_mode,
				personName, type, preferences.digitsNumber,// preferences.heightPreferred,
				preferences.metersSecondsPreferred,
				check_runI_realtime_rel_abs.Active,
				timesString, distanceInterval, distancesString,
				photocell_l, isLastCaptured, feedbackRunsI, videoTime);

		// B) Paint cairo graph
		//cairoPaintBarsPreCurrent.UseHeights = useHeights;

		cairoPaintBarsPreCurrent.Paint();
	}

	public void PrepareResultsSessionGraph ()
	{
		// Paint cairo graph
		cairoPaintBarsPre.ShowPersonNames = radio_contacts_results_personAll.Active;
		cairoPaintBarsPre.Paint();
	}

	private int calculateMaxRowsForText (List<Event> events, int longestWordSize, bool allJumps, bool secondDataRow)
	{
		int maxRows = 0;

		foreach(Event ev in events)
		{
			int rows = 0;
			if(allJumps) 			//to write the jump type (1st the jump type because it's only one row)
				rows ++;

			//try to pack small words if they fit in a row using wordsAccu (accumulated)
			string wordsAccu = "";
			string [] words = ev.Description.Split(new char[] {' '});

			foreach(string word in words)
			{
				if(wordsAccu == "")
					wordsAccu = word;
				else if( (wordsAccu + " " + word).Length <= longestWordSize )
					wordsAccu += " " + word;
				else {
					wordsAccu = word;
					rows ++;
				}
			}
			if(wordsAccu != "")
				rows ++;

			if(ev.Simulated == -1) //to write simulated at bottom
				rows ++;

			if(secondDataRow)
				rows ++;

			if(rows > maxRows)
				maxRows = rows;
		}

		return maxRows;
	}

	/*
	 * commented old Pango code (pre Cairo)
	 *
	private int calculateBottomMarginForText (int maxRows, Pango.Layout layout)
	{

		layout.SetMarkup("a");
		int lWidth = 1;
		int lHeight = 1;
		layout.GetPixelSize(out lWidth, out lHeight);

		return lHeight * maxRows;
	}

	//TODO: need to add personName here
	private int findLongestWordSize (List<Event> events, bool allTypes)
	{
		int longestWordSize = 0;

		foreach(Event ev in events)
		{
			string [] textArray = ev.Description.Split(new char[] {' '});
			foreach(string text in textArray)
				if(text.Length > longestWordSize)
					longestWordSize = text.Length;

			//note jump type will be in one line
			//if(ev.Description.Length > longestWordSize)
			//		longestWordSize = ev.Description.Length;

			//TODO: check it in local user language (Catalog)
			if(allTypes && ev.Type.Length > longestWordSize)
				longestWordSize = ev.Type.Length;

			if(ev.Simulated == -1 && event_execute_label_simulated.Length > longestWordSize)
				longestWordSize = event_execute_label_simulated.Length;
		}

		return longestWordSize;
	}

	private Pango.Layout calculateLayoutFontForText (List<Event> events, int longestWordSize, Pango.Layout layout, int ancho)
	{
		// 1) set marginBetweenTexts to 1.1 character
		layout.SetMarkup("a");
		int lWidth = 1;
		int lHeight = 1;
		layout.GetPixelSize(out lWidth, out lHeight);
		int marginBetweenTexts = Convert.ToInt32(1.1 * lWidth);

		// 2) create the longestWord to find its width
		string longestWord = new string('*', longestWordSize);
		layout.SetMarkup(longestWord);
		lWidth = 1;
		lHeight = 1;
		layout.GetPixelSize(out lWidth, out lHeight);

		// 3) if longestWord * jumps.Count does not fit, iterate to find correct font size
		if(events.Count * (lWidth + marginBetweenTexts) > ancho)
		{
			int i = 1;
			do {
				layout.FontDescription.Size -= Convert.ToInt32(Pango.Scale.PangoScale);
				if(layout.FontDescription.Size / Pango.Scale.PangoScale < 1)
					break;

				layout.SetMarkup(longestWord);
				layout.GetPixelSize(out lWidth, out lHeight);

				i ++;
			} while (events.Count * (lWidth + marginBetweenTexts) > ancho);
		}

		return layout;
	}
	*/

	private void hideButtons() {
		event_execute_button_cancel.Sensitive = false;
		event_execute_button_finish.Sensitive = false;
		fullscreen_button_fullscreen_contacts.Sensitive = false;
	}


	// ---- test simple controls ----->

	private void updateGraphResultsSessionByMode ()
	{
		if (current_mode == Constants.Modes.JUMPSSIMPLE)
			updateGraphJumpsSimple ();
		else if (current_mode == Constants.Modes.JUMPSREACTIVE)
			updateGraphJumpsReactive ();
		else if (current_mode == Constants.Modes.RUNSSIMPLE)
			updateGraphRunsSimple ();
		else if (current_mode == Constants.Modes.RUNSINTERVALLIC)
			updateGraphRunsInterval ();
		else if (current_mode == Constants.Modes.RUNSENCODER)
			updateGraphRunEncoderBars ();
		else if (Constants.ModeIsFORCESENSOR (current_mode))
			updateGraphForceSensorBars ();
		else if (Constants.ModeIsENCODER (current_mode))
			updateGraphEncoderSessionBars ();
		else if (current_mode == Constants.Modes.WILIGHT)
			updateGraphWilightBars ();
		else if (current_mode == Constants.Modes.OTHER)
			updateGraphFourPlatformsBars ();
	}

	private void on_spin_contacts_graph_last_limit_value_changed (object o, EventArgs args)
	{
		updateGraphResultsSessionByMode ();
	}

	private void on_radio_contacts_graph_test_toggled (object o, EventArgs args)
	{
		updateGraphResultsSessionByMode ();

		pre_fillTreeView_resultsSession ();
	}

	private void on_radio_contacts_results_person_toggled (object o, EventArgs args)
	{
		updateGraphResultsSessionByMode ();

		pre_fillTreeView_resultsSession ();
	}

	private void on_check_run_show_time_toggled (object o, EventArgs args)
	{
		if(current_mode == Constants.Modes.RUNSSIMPLE)
			updateGraphRunsSimple ();
		else if(current_mode == Constants.Modes.RUNSINTERVALLIC)
			updateGraphRunsInterval ();
	}

	// <---- end of test simple controls -----

	private void on_event_execute_update_graph_in_progress_clicked(object o, EventArgs args)
	{
		bool animate = true;
		if(UtilAll.GetOSEnum() == UtilAll.OperatingSystems.MACOSX)
			animate = false;

		switch (currentEventType.Type) {
			case EventType.Types.JUMP:
				if(thisJumpIsSimple) 
					PrepareJumpSimpleGraph(currentEventExecute.PrepareEventGraphJumpSimpleObject, animate);
				else {
					PrepareJumpReactiveRealtimeCaptureGraph(
							currentEventExecute.PrepareEventGraphJumpReactiveRealtimeCaptureObject.lastTv, 
							currentEventExecute.PrepareEventGraphJumpReactiveRealtimeCaptureObject.lastTc,
							currentEventExecute.PrepareEventGraphJumpReactiveRealtimeCaptureObject.tvString,
							currentEventExecute.PrepareEventGraphJumpReactiveRealtimeCaptureObject.tcString,
							currentEventExecute.PrepareEventGraphJumpReactiveRealtimeCaptureObject.type,
							currentPerson.Name,
							preferences.volumeOn, preferences.gstreamer, feedbackJumpsRj,
							preferences.heightPreferred);

					drawingarea_results_realtime.QueueDraw ();
				}
				break;
			case EventType.Types.RUN:
				if(thisRunIsSimple)
					PrepareRunSimpleGraph(currentEventExecute.PrepareEventGraphRunSimpleObject, animate); //add here the photocells string on wichro (-1 strings on ! wichro)
				else {
					/*
					bool volumeOnHere = preferences.volumeOn;
					//do not play good or bad sounds at RSA because we need to hear the GO sound
					if(currentRunIntervalType.IsRSA)
						volumeOnHere = false;
						*/

					PrepareRunIntervalRealtimeCaptureGraph(
							currentEventExecute.PrepareEventGraphRunIntervalRealtimeCaptureObject.timesString,
							currentEventExecute.PrepareEventGraphRunIntervalRealtimeCaptureObject.distanceInterval,
							currentEventExecute.PrepareEventGraphRunIntervalRealtimeCaptureObject.distancesString,
							currentEventExecute.PrepareEventGraphRunIntervalRealtimeCaptureObject.photocell_l,
							currentEventExecute.PrepareEventGraphRunIntervalRealtimeCaptureObject.type,
							currentPerson.Name, feedbackRunsI);
					drawingarea_results_realtime.QueueDraw ();
				}
				break;
		}
	}
	
	private void on_event_execute_EventEnded()
	{
		hideButtons();

		checkbutton_video_contacts.Sensitive = true;
		if(preferences.videoOn) {	
			label_video_feedback.Text = "";
			button_video_contacts_preview.Visible = true;
			//capturer.ClickStop();
			//videoCapturePrepare(false); //if error, show message
		}
	}

	void on_event_execute_finish_clicked (object o, EventArgs args)
	{
		//event will be raised, and managed in chronojump.cs
		//see ButtonFinish at end of class
	}
	
	void on_event_execute_button_help_clicked (object o, EventArgs args)
	{
	}
	
	void on_event_execute_button_cancel_clicked (object o, EventArgs args)
	{
		hideButtons();
		
		checkbutton_video_contacts.Sensitive = true;
		if(preferences.videoOn) {
			//it will be recorded on temp, but chronojump will move it to chronojump/multimedia folders
			label_video_feedback.Text = "";
			button_video_contacts_preview.Visible = true;
			//capturer.ClickStop();
			//videoCapturePrepare(false); //if error, show message
		}
	}
	
	//when event finishes, we should put in the label_time, the correct totalTime, that comes from chronopic
	//label_time shows a updating value from a software chrono: onTimer, this is not exact and is now
	//replaced with the chronopic timer
	public double event_execute_LabelTimeValue 
	{
		set { 
			event_execute_label_time_value.Text = Math.Round(value,3).ToString();
		
			//also put progressBar text to "" because probably doesn't mach labe_time
			event_execute_progressbar_time.Fraction = 1; 
			event_execute_progressbar_time.Text = ""; 
		}
	}
	//same as LabelTimeValue	
	public double event_execute_LabelEventValue 
	{
		set { event_execute_label_event_value.Text = value.ToString(); }
	}
		
	
	public Button event_execute_ButtonCancel 
	{
		get { return event_execute_button_cancel; }
	}
	
	public Button event_execute_ButtonFinish 
	{
		get { return event_execute_button_finish; }
	}
	

	private void connectWidgetsEventExecute (Gtk.Builder builder)
	{
		box_event_execute_label_message = (Gtk.Box) builder.GetObject ("box_event_execute_label_message");
		event_execute_label_phases_name = (Gtk.Label) builder.GetObject ("event_execute_label_phases_name");
		event_execute_label_message = (Gtk.Label) builder.GetObject ("event_execute_label_message");
		image_no_capturing = (Gtk.Image) builder.GetObject ("image_no_capturing");
		image_capturing = (Gtk.Image) builder.GetObject ("image_capturing");
		image_capturing_blue = (Gtk.Image) builder.GetObject ("image_capturing_blue");
		image_no_capturing_encoder = (Gtk.Image) builder.GetObject ("image_no_capturing_encoder");
		image_capturing_encoder = (Gtk.Image) builder.GetObject ("image_capturing_encoder");
		image_force_sensor_adjust_no_capturing = (Gtk.Image) builder.GetObject ("image_force_sensor_adjust_no_capturing");
		image_force_sensor_adjust_capturing = (Gtk.Image) builder.GetObject ("image_force_sensor_adjust_capturing");

		spin_contacts_graph_last_limit = (Gtk.SpinButton) builder.GetObject ("spin_contacts_graph_last_limit");
		box_contacts_graph_exercise = (Gtk.Box) builder.GetObject ("box_contacts_graph_exercise");
		box_contacts_graph_show_graph_table = (Gtk.Box) builder.GetObject ("box_contacts_graph_show_graph_table");
		box_contacts_simple_graph_controls = (Gtk.Box) builder.GetObject ("box_contacts_simple_graph_controls");
		radio_contacts_graph_currentTest = (Gtk.RadioButton) builder.GetObject ("radio_contacts_graph_currentTest");
		radio_contacts_graph_allTests = (Gtk.RadioButton) builder.GetObject ("radio_contacts_graph_allTests");
		//radio_contacts_results_personCurrent = (Gtk.RadioButton) builder.GetObject ("radio_contacts_results_personCurrent");
		radio_contacts_results_personAll = (Gtk.RadioButton) builder.GetObject ("radio_contacts_results_personAll");
		image_radio_contacts_results_personCurrent = (Gtk.Image) builder.GetObject ("image_radio_contacts_results_personCurrent");
		image_radio_contacts_results_personAll = (Gtk.Image) builder.GetObject ("image_radio_contacts_results_personAll");
		check_run_show_time = (Gtk.CheckButton) builder.GetObject ("check_run_show_time");

		event_execute_progressbar_event = (Gtk.ProgressBar) builder.GetObject ("event_execute_progressbar_event");
		event_execute_progressbar_time = (Gtk.ProgressBar) builder.GetObject ("event_execute_progressbar_time");


		//currently gtk-sharp cannot display a label in a progressBar in activity mode (Pulse() not Fraction)
		//then we show the value in a label:
		event_execute_label_event_value = (Gtk.Label) builder.GetObject ("event_execute_label_event_value");
		event_execute_label_time_value = (Gtk.Label) builder.GetObject ("event_execute_label_time_value");

		event_execute_button_cancel = (Gtk.Button) builder.GetObject ("event_execute_button_cancel");
		event_execute_button_finish = (Gtk.Button) builder.GetObject ("event_execute_button_finish");

		//event_execute_table_pulse = (Gtk.Table) builder.GetObject ("event_execute_table_pulse");
		//event_execute_table_pulse_values = (Gtk.Table) builder.GetObject ("event_execute_table_pulse_values");

		//	align_check_vbox_contacts_graph_legend = (Gtk.Alignment) builder.GetObject ("align_check_vbox_contacts_graph_legend");
		//	check_vbox_contacts_graph_legend = (Gtk.CheckButton) builder.GetObject ("check_vbox_contacts_graph_legend");
		//	vbox_contacts_graph_legend = (Gtk.VBox) builder.GetObject ("vbox_contacts_graph_legend");

		//for the color change in the background of the cell label
		//event_execute_eventbox_pulse_time = (Gtk.EventBox) builder.GetObject ("event_execute_eventbox_pulse_time");
		//event_execute_label_pulse_now = (Gtk.Label) builder.GetObject ("event_execute_label_pulse_now");
		//event_execute_label_pulse_avg = (Gtk.Label) builder.GetObject ("event_execute_label_pulse_avg");

		notebook_results_data = (Gtk.Notebook) builder.GetObject ("notebook_results_data");

		box_capture_current = (Gtk.Box) builder.GetObject ("box_capture_current");
		box_capture_current_forceSensor = (Gtk.Box) builder.GetObject ("box_capture_current_forceSensor");
		hbox_capture_current_runEncoder = (Gtk.HBox) builder.GetObject ("hbox_capture_current_runEncoder");
		align_drawingarea_realtime_capture_cairo = (Gtk.Alignment) builder.GetObject ("align_drawingarea_realtime_capture_cairo");
		drawingarea_results_realtime = (Gtk.DrawingArea) builder.GetObject ("drawingarea_results_realtime");
		drawingarea_results_session = (Gtk.DrawingArea) builder.GetObject ("drawingarea_results_session");
		vbox_event_execute_drawingarea_run_interval_realtime_capture_cairo = (Gtk.VBox) builder.GetObject ("vbox_event_execute_drawingarea_run_interval_realtime_capture_cairo");
		check_runI_realtime_rel_abs = (Gtk.CheckButton) builder.GetObject ("check_runI_realtime_rel_abs");
		image_check_runI_realtime_rel_abs = (Gtk.Image) builder.GetObject ("image_check_runI_realtime_rel_abs");
		drawingarea_run_simple_double_contacts = (Gtk.DrawingArea) builder.GetObject ("drawingarea_run_simple_double_contacts");
		label_run_simple_double_contacts = (Gtk.Label) builder.GetObject ("label_run_simple_double_contacts");
		/*
		   hbox_combo_graph_results_width = (Gtk.Box) builder.GetObject ("hbox_combo_graph_results_width");
		   hbox_combo_graph_results_height = (Gtk.Box) builder.GetObject ("hbox_combo_graph_results_height");
		   combo_graph_results_width = (Gtk.ComboBoxText) builder.GetObject ("combo_graph_results_width");
		   combo_graph_results_height = (Gtk.ComboBoxText) builder.GetObject ("combo_graph_results_height");
		   */
	}
}

//to prepare data before calling cairo method
public abstract class CairoPaintBarsPre
{
	public bool ShowPersonNames; //to hide desc if not ShowPersonNames (because in ShowPersonNames, desc is name, but we do not want to see a comment there)

	//jump simple
	public PrepareEventGraphJumpSimple eventGraphJumpsStored;
	public bool UseHeights;

	//jump reactive
	public PrepareEventGraphJumpReactive eventGraphJumpsRjStored;

	//run simple
	public PrepareEventGraphRunSimple eventGraphRunsStored;
	public bool RunsShowTime;

	//run interval
	public PrepareEventGraphRunInterval eventGraphRunsIntervalStored;
	//runEncoder
	public PrepareEventGraphRunEncoder eventGraphRunEncoderStored;
	//wilight
	public PrepareEventGraphWilight eventGraphWilightStored;
	//wilight
	public PrepareEventGraphFourPlatforms eventGraphFourPlatformsStored;
	//forceSensor
	public PrepareEventGraphForceSensor eventGraphForceSensorStored;
	//encoder
	public PrepareEventGraphEncoderCurrent eventGraphEncoderCurrentStored;
	public PrepareEventGraphEncoderSession eventGraphEncoderSessionStored;

	protected CairoBars cb;
	protected DrawingArea darea;
	protected string fontStr;
	protected Constants.Modes mode;
	protected string personName;
	protected string testName;
	protected string title;
	protected int pDN; //preferences.digitsNumber
	//protected string messageNoStoreCreated;
	protected double videoTime;
	protected string screenshotURL;

	protected void initialize (DrawingArea darea, string fontStr, Constants.Modes mode,
			string personName, string testName, int pDN)
	{
		this.darea = darea;
		this.fontStr = fontStr;
		this.mode = mode;
		this.personName = personName;
		this.testName = testName;
		this.pDN = pDN;
		this.screenshotURL = "";
	}

	// to debug
	public override string ToString ()
	{
		return string.Format(
				"mode: {0}, personName: {1}, testName: {2}, pDN: {3}",
				mode, personName, testName, pDN);
	}

	public bool ModeMatches (Constants.Modes mode)
	{
		LogB.Information(string.Format("ModeMatches. This mode: {0}, checking against: {1}, are equal: {2}",
					this.mode, mode, (this.mode == mode)));

		return (this.mode == mode);
	}

	public virtual void StoreEventGraphJumps (PrepareEventGraphJumpSimple eventGraph)
	{
	}
	public virtual void StoreEventGraphJumpsRj (PrepareEventGraphJumpReactive eventGraph)
	{
	}
	public virtual void StoreEventGraphRuns (PrepareEventGraphRunSimple eventGraph)
	{
	}
	public virtual void StoreEventGraphRunsInterval (PrepareEventGraphRunInterval eventGraph)
	{
	}
	public virtual void StoreEventGraphRunEncoder (PrepareEventGraphRunEncoder eventGraph)
	{
	}
	public virtual void StoreEventGraphWilight (PrepareEventGraphWilight eventGraph)
	{
	}
	public virtual void StoreEventGraphFourPlatforms (PrepareEventGraphFourPlatforms eventGraph)
	{
	}
	public virtual void StoreEventGraphForceSensor (PrepareEventGraphForceSensor eventGraph)
	{
	}
	public virtual void StoreEventGraphEncoderCurrent (PrepareEventGraphEncoderCurrent eventGraph)
	{
	}
	public virtual void StoreEventGraphEncoderSession (PrepareEventGraphEncoderSession eventGraph)
	{
	}

	public virtual void ShowMessage (DrawingArea darea, string fontTypeStr, string message)
	{
	}

	/*
	public void Prepare ()
	{
		if(mode == Constants.Modes.JUMPSSIMPLE)
			PrepareJumpSimpleGraph(eventGraphJumpsStored, false);
		else if(current_mode == Constants.Modes.RUNSSIMPLE)
			PrepareRunSimpleGraph(eventGraphRunsStored, false);
	}
	*/

	//used at start capture on realtime tests (jumpRj, runI)
	protected void blankScreen (DrawingArea darea, string fontStr)
	{
		try {
			new CairoBars1Series (darea, CairoBars.Type.NORMAL, fontStr, "");
		} catch {
			LogB.Information("Saved crash at with cairo paint (blank screen)");
		}
	}

	public void Paint ()
	{
		if(darea == null || darea.Window == null) //at start program, this can fail
			return;

		if(! storeCreated())
		{
			try {
				new CairoBars1Series (darea, CairoBars.Type.NORMAL, fontStr, ""); //messageNoStoreCreated);
			} catch {
				LogB.Information("saved crash at with cairo paint at !storeCreated");
			}
			return;
		}

		if(! haveDataToPlot())
		{
			try {
				new CairoBars1Series (darea, CairoBars.Type.NORMAL, fontStr, testsNotFound());
			} catch {
				LogB.Information("saved crash at with cairo paint at !haveDataToPlot");
			}
			return;
		}

		paintSpecific ();
		//darea.QueueDraw (); this makes the memory increase a lot! Just call queue when it is needed!
	}

	protected void passDataForScreenshotIfNeeded ()
	{
		if (cb != null && screenshotURL != null && screenshotURL != "")
			cb.ScreenshotURL = screenshotURL;
	}

	protected virtual string testsNotFound ()
	{
		if(personName != "")
		{
			if(testName != "")
				return string.Format(Catalog.GetString("{0} has not made any {1} test in this session."),
						personName, testName);
			else
				return string.Format(Catalog.GetString("{0} has not made any test in this session."),
						personName);
		} else {
			if(testName != "")
				return string.Format(Catalog.GetString("There are no {0} tests in this session."),
						testName);
			else
				return Catalog.GetString("No tests in this session.");
		}
	}

	protected abstract bool storeCreated ();
	protected abstract bool haveDataToPlot ();
	protected abstract void paintSpecific();

	protected string generateTitle ()
	{
		string titleStr = "";
		string sep = "";

		if(personName != "")
		{
			titleStr = personName;
			sep = " - ";
		}

		if(testName != "")
			titleStr += sep + testName;

		return titleStr;
	}


	//TODO: this is repeated on this file, think also if move it to gui/cairo/bars.cs
	protected int calculateMaxRowsForTextCairo (List<Event> events, int longestWordSize,
			bool allJumps, bool thereIsASimulated, bool secondDataRow)
	{
		int maxRows = 0;

		//LogB.Information("calculateMaxRowsForText");
		foreach(Event ev in events)
		{
			//LogB.Information("Event: " + ev.ToString());
			int rows = 0;
			if(allJumps) 			//to write the jump type (1st the jump type because it's only one row)
				rows ++;

			//try to pack small words if they fit in a row using wordsAccu (accumulated)
			string wordsAccu = "";
			string [] words = ev.Description.Split(new char[] {' '});

			foreach(string word in words)
			{
				if(wordsAccu == "")
					wordsAccu = word;
				else if( (wordsAccu + " " + word).Length <= longestWordSize )
					wordsAccu += " " + word;
				else {
					wordsAccu = word;
					rows ++;
				}
			}
			if(wordsAccu != "")
				rows ++;

			//if(ev.Simulated == -1) //to write simulated at bottom
			if(thereIsASimulated) //if a event has two lines but not simulated, it has to reserve a line for other events (maybe of 1 line with simulated)
				rows ++;

			if(secondDataRow)
				rows ++;

			if(rows > maxRows)
				maxRows = rows;
		}
		//LogB.Information("maxRows: " + maxRows.ToString());

		return maxRows;
	}

	protected string longestWord;
	protected int fontHeightForBottomNames;
	protected int maxRowsForText;
	protected int bottomMargin;

	//manage bottom text font/spacing of rows
	protected void calculateBottomParams (List<Event> events, bool allTypes, string addToType, string simulatedLabel, bool thereIsASimulated, bool secondDataRow)
	{
		longestWord = findLongestWordCairo (events, allTypes, addToType, simulatedLabel);
		fontHeightForBottomNames = cb.GetFontForBottomNames (events, longestWord);

		maxRowsForText = calculateMaxRowsForTextCairo (events, longestWord.Length,
				allTypes, thereIsASimulated, secondDataRow);
		bottomMargin = cb.GetBottomMarginForText (maxRowsForText, fontHeightForBottomNames);

		//LogB.Information(string.Format("fontHeightForBottomNames: {0}, bottomMargin: {1}", fontHeightForBottomNames, bottomMargin));
	}

	//TODO: need to add personName here
	protected string findLongestWordCairo (List<Event> events, bool allTypes, string addToType, string simulatedLabel)
	{
		int longestWordSize = 0;
		string longestWord = ""; //debug

		foreach(Event ev in events)
		{
			string [] textArray = ev.Description.Split(new char[] {' '});
			foreach(string text in textArray)
			{
				if(text.Length > longestWordSize)
				{
					longestWordSize = text.Length;
					longestWord = text;
				}
			}

			//note jump type will be in one line
			//TODO: check it in local user language (Catalog)
			if(allTypes && ev.Type != null && ev.Type.Length > longestWordSize)
			{
				longestWordSize = ev.Type.Length + addToType.Length;
				longestWord = ev.Type + addToType;
			}

			if(ev.Simulated == -1 && simulatedLabel.Length > longestWordSize)
			{
				longestWordSize = simulatedLabel.Length;
				longestWord = simulatedLabel;
			}
		}

		//LogB.Information("longestWord: " + longestWord);
		//return longestWordSize;
		return longestWord;
	}

	//person name or test type, or both
	//this can separate name with spaces on rows
	protected string createTextBelowBar(
			string secondResult, 	//time on runSimple
			string jumpType,
			string personName,
			bool thereIsASimulated, bool thisIsSimulated,
			int longestWordSize, int maxRowsForText)
	{
		string str = "";
		string vertSep = "";
		int rows = 0;

		if(secondResult != "")
		{
			str += vertSep + secondResult;
			vertSep = "\n";
			rows ++;
		}

		//if have to print jump type, print it first in one row
		if(jumpType != "")
		{
			str += vertSep + jumpType;
			vertSep = "\n";
			rows ++;
		}

		//method 1
		// 2) separate person name in rows and send it to plotTextBelowBarDoRow()
		//    packing small words if they fit in a row using wordsAccu (accumulated)

		string wordsAccu = "";
		string [] words = personName.Split(new char[] {' '});

		//bool newLineDone = false;
		foreach(string word in words)
		{
			if(wordsAccu == "")
				wordsAccu = word;
			else if( (wordsAccu + " " + word).Length <= longestWordSize )
				wordsAccu += " " + word;
			else {
				str += vertSep + wordsAccu;
				vertSep = "\n";
				//newLineDone = true;
				wordsAccu = word;
				rows ++;
			}
		}
		if(wordsAccu != "")
		{
			str += vertSep + wordsAccu;
			vertSep = "\n";
			rows ++;
		}

		/* method 2, two lines for name
		if(personName != "")
		{
			//separate in two lines
			string [] words = personName.Split(new char[] {' '});
			string firstLine;
			string secondLine;
			string space;
			int minLengthOfMaxRow = 1000;
			int bestCombination = 0;
			for(int i = 1; i < words.Length; i ++)
			{
				firstLine = "";
				space = "";
				for(int j = 0; j < i; j ++)
				{
					firstLine += space + words[j];
					space = " ";
				}

				secondLine = "";
				space = "";
				for(int j = i; j < words.Length; j ++)
				{
					secondLine += space + words[j];
					space = " ";
				}

				LogB.Information(string.Format("i: {0}, firstLine: {1}, length: {2}, secondLine: {3}, length: {4}",
							i, firstLine, firstLine.Length, secondLine, secondLine.Length));

				int maxOfThisCombination = firstLine.Length;
				if(secondLine.Length > maxOfThisCombination)
					maxOfThisCombination = secondLine.Length;

				if(maxOfThisCombination < minLengthOfMaxRow)
				{
					minLengthOfMaxRow = maxOfThisCombination;
					bestCombination = i;
				}
			}

			str += vertSep;
			vertSep = "\n";
			space = "";
			for(int i = 0; i < bestCombination; i ++)
			{
				str += space + words[i];
				space = " ";
			}
			str += vertSep;
			space = "";
			for(int i = bestCombination; i < words.Length; i ++)
			{
				str += space + words[i];
				space = " ";
			}
		}
		*/

		if(thereIsASimulated)
		{
			while(rows +1 < maxRowsForText)
			{
				str += "\n";
				rows ++;
			}

			str += "\n";
			if(thisIsSimulated)
				str += "(" + Catalog.GetString("Simulated") + ")"; //TODO: improve this to ensure it is last row
		} else {
			while(rows < maxRowsForText)
			{
				str += "\n";
				rows ++;
			}
		}

		return str;
	}

	public int FindBarInPixel (double px, double py)
	{
		LogB.Information(string.Format("FindBarInPixel cb == null: {0}, px: {1}, py: {2}", (cb == null), px, py));
		if(cb == null)
			return -1;

		return cb.FindBarInPixel (px, py);
	}
	public int FindBarIdInPixel (double px, double py)
	{
		LogB.Information(string.Format("FindBarIdInPixel cb == null: {0}, px: {1}, py: {2}", (cb == null), px, py));
		if(cb == null)
			return -1;

		return cb.FindBarIdInPixel (px, py);
	}

	public string ScreenshotURL
	{
		set { screenshotURL = value; }
	}
}

public class CairoPaintBarsPreJumpSimple : CairoPaintBarsPre
{
	public CairoPaintBarsPreJumpSimple (DrawingArea darea, string fontStr, Constants.Modes mode, string personName, string testName, int pDN)
	{
		initialize (darea, fontStr, mode, personName, testName, pDN);
		this.title = generateTitle();
	}

	public override void StoreEventGraphJumps (PrepareEventGraphJumpSimple eventGraph)
	{
		this.eventGraphJumpsStored = eventGraph;
	}

	protected override bool storeCreated ()
	{
		return (eventGraphJumpsStored != null);
	}

	protected override bool haveDataToPlot()
	{
		return (eventGraphJumpsStored.jumpsAtSQL.Count > 0);
	}

	protected override void paintSpecific()
	{
		/*
		 * check if one bar has to be shown or two
		 * this is important when we are showing multitests
		 */
		bool showBarA = false; //tc or fall
		bool showBarB = false; //tv or height
		foreach(Jump jump in eventGraphJumpsStored.jumpsAtSQL)
		{
			if(jump.Fall > 0 || jump.Tc > 0) //jump.Tc to include takeOff, takeOffWeiht
				showBarA = true;
			if(jump.Tv > 0)
				showBarB = true;

			//if both found do not need to search more
			if(showBarA && showBarB)
				break;
		}
		//takeOff, takeOff weights show times (Tc)
		if(showBarA && ! showBarB)
			UseHeights = false;

		if(showBarA && showBarB) //Dja, Djna
			cb = new CairoBarsNHSeries (darea, CairoBars.Type.NORMAL, true, true, true, true);
		else if (showBarA) //takeOff, takeOffWeight
			cb = new CairoBars1Series (darea, CairoBars.Type.NORMAL, true, true, true);
		else //rest of the jumps: sj, cmj, ..
			cb = new CairoBars1Series (darea, CairoBars.Type.NORMAL, true, true, true);

		if(UseHeights) {
			cb.YVariable = Catalog.GetString("Height");
			cb.YUnits = "cm";
			if(showBarA && showBarB) //Dja, Djna
			{
				cb.VariableSerieA = Catalog.GetString("Falling height");
				cb.VariableSerieB = Catalog.GetString("Jump height");
			}
		} else {
			cb.YVariable = Catalog.GetString("Time");
			cb.YUnits = "s";
			if(showBarA && showBarB) //Dja, Djna
			{
				cb.VariableSerieA = Catalog.GetString("Contact time");
				cb.VariableSerieB = Catalog.GetString("Flight time");
			}
		}

		//cb.GraphInit(fontStr, ! ShowPersonNames, true); //usePersonGuides, useGroupGuides
		cb.GraphInit(fontStr, true, true); //usePersonGuides, useGroupGuides

		List<Event> events = Jump.JumpListToEventList(eventGraphJumpsStored.jumpsAtSQL);

		//find if there is a simulated
		bool thereIsASimulated = false;
		for(int i=0 ; i < eventGraphJumpsStored.jumpsAtSQL.Count; i++)
		{
			if(eventGraphJumpsStored.jumpsAtSQL[i].Simulated == -1)
				thereIsASimulated = true;

			if(! ShowPersonNames)
				eventGraphJumpsStored.jumpsAtSQL[i].Description = ""; //to avoid showing description
		}

		calculateBottomParams (events, eventGraphJumpsStored.type == "", "",
				"(" + Catalog.GetString("Simulated") + ")", thereIsASimulated, false);

		List<PointF> pointA_l = new List<PointF>();
		List<PointF> pointB_l = new List<PointF>();
		List<string> names_l = new List<string>();
		List<int> id_l = new List<int>(); //the uniqueIDs for knowing them on bar selection

		int countToDraw = eventGraphJumpsStored.jumpsAtSQL.Count;
		foreach(Jump jump in eventGraphJumpsStored.jumpsAtSQL)
		{
			LogB.Information("jump: " + jump.ToString());
			// 1) Add data
			double valueA = jump.Fall;
			double valueB = Util.GetHeightInCentimeters(jump.Tv); //jump height
			if(! UseHeights) {
				valueA = jump.Tc;
				valueB = jump.Tv;
			}

			pointA_l.Add(new PointF(countToDraw, valueA));
			pointB_l.Add(new PointF(countToDraw, valueB));
			countToDraw --;

			// 2) Add bottom names
			//names_l.Add(Catalog.GetString(jump.Type));
			string typeRowString = "";
			if (eventGraphJumpsStored.type == "") //if "all runs" show run.Type
				typeRowString = jump.Type;

			names_l.Add(createTextBelowBar(
						"",
						typeRowString,
						jump.Description,
						thereIsASimulated, (jump.Simulated == -1),
						longestWord.Length, maxRowsForText));

			id_l.Add(jump.UniqueID);
			if(showBarA && showBarB) //there are jumps like Dja, Djna
				id_l.Add(jump.UniqueID);

			if (eventGraphJumpsStored.selectedID == jump.UniqueID)
				cb.SelectedPos = eventGraphJumpsStored.jumpsAtSQL.Count -countToDraw -1;
		}

		cb.Id_l = id_l;

		cb.PassGuidesData (new CairoBarsGuideManage(
					//! ShowPersonNames, true, //usePersonGuides, useGroupGuides
					true, true, //usePersonGuides, useGroupGuides
					eventGraphJumpsStored.sessionMAXAtSQL,
					eventGraphJumpsStored.sessionAVGAtSQL,
					eventGraphJumpsStored.sessionMINAtSQL,
					eventGraphJumpsStored.personMAXAtSQLAllSessions,
					eventGraphJumpsStored.personMAXAtSQL,
					eventGraphJumpsStored.personAVGAtSQL,
					eventGraphJumpsStored.personMINAtSQL));

		if(showBarA && showBarB) //Dja, Djna
		{
			List<List<PointF>> barsSecondary_ll = new List<List<PointF>>();
			barsSecondary_ll.Add(pointA_l);

			cb.PassData2Series (pointB_l, barsSecondary_ll, false,
					new List<Cairo.Color>(), new List<Cairo.Color>(), names_l,
					"", false,
					-1, fontHeightForBottomNames, bottomMargin, title,
					new List<int> (), new List<int> ());
		} else if (showBarA) //takeOff, takeOffWeight
			cb.PassData1Serie (pointA_l,
					new List<Cairo.Color>(), names_l,
					-1, fontHeightForBottomNames, bottomMargin, title,
					new List<int> (), new List<int> ());
		else //rest of the jumps: sj, cmj, ..
			cb.PassData1Serie (pointB_l,
					new List<Cairo.Color>(), names_l,
					-1, fontHeightForBottomNames, bottomMargin, title,
					new List<int> (), new List<int> ());

		passDataForScreenshotIfNeeded ();

		cb.GraphDo();
	}
}

public class CairoPaintBarsPreJumpReactive : CairoPaintBarsPre
{
	public CairoPaintBarsPreJumpReactive (DrawingArea darea, string fontStr, Constants.Modes mode, string personName, string testName, int pDN)
	{
		initialize (darea, fontStr, mode, personName, testName, pDN);
		this.title = generateTitle();
	}

	public override void StoreEventGraphJumpsRj (PrepareEventGraphJumpReactive eventGraph)
	{
		this.eventGraphJumpsRjStored = eventGraph;
	}

	protected override bool storeCreated ()
	{
		return (eventGraphJumpsRjStored != null);
	}

	protected override bool haveDataToPlot()
	{
		return (eventGraphJumpsRjStored.jumpsAtSQL.Count > 0);
	}

	protected override void paintSpecific()
	{
		cb = new CairoBarsNHSeries (darea, CairoBars.Type.NORMAL, true, true, true, true);

		if(UseHeights) {
			cb.YVariable = Catalog.GetString("Height");
			cb.YUnits = "cm";
			cb.VariableSerieA = Catalog.GetString("Falling height") + " (" + Catalog.GetString("AVG") + ") ";
			cb.VariableSerieB = Catalog.GetString("Jump height") + " (" + Catalog.GetString("AVG") + ") ";
		} else {
			cb.YVariable = Catalog.GetString("Time");
			cb.YUnits = "s";
			cb.VariableSerieA = Catalog.GetString("Contact time") + " (" + Catalog.GetString("AVG") + ") ";
			cb.VariableSerieB = Catalog.GetString("Flight time") + " (" + Catalog.GetString("AVG") + ") ";
		}

		//cb.GraphInit(fontStr, ! ShowPersonNames, true); //usePersonGuides, useGroupGuides
		cb.GraphInit(fontStr, true, true); //usePersonGuides, useGroupGuides

		List<Event> events = JumpRj.JumpListToEventList(eventGraphJumpsRjStored.jumpsAtSQL);

		//find if there is a simulated
		bool thereIsASimulated = false;
		for(int i=0 ; i < eventGraphJumpsRjStored.jumpsAtSQL.Count; i++)
		{
			if(eventGraphJumpsRjStored.jumpsAtSQL[i].Simulated == -1)
				thereIsASimulated = true;

			if(! ShowPersonNames)
				eventGraphJumpsRjStored.jumpsAtSQL[i].Description = ""; //to avoid showing description
		}

		calculateBottomParams (events, true, " - 99", //thinking on 99 jumps
				"(" + Catalog.GetString("Simulated") + ")", thereIsASimulated, false);


		//List<PointF> pointA0_l = new List<PointF>();
		List<PointF> pointA1_l = new List<PointF>();

		List<PointF> pointB_l = new List<PointF>();
		List<string> names_l = new List<string>();
		List<int> id_l = new List<int>(); //the uniqueIDs for knowing them on bar selection

		int countToDraw = eventGraphJumpsRjStored.jumpsAtSQL.Count;
		foreach(JumpRj jump in eventGraphJumpsRjStored.jumpsAtSQL)
		{
			LogB.Information("jump: " + jump.ToString());
			// 1) Add data
			//sum of the subjumps
			//double valueA = jump.TcSumCaringForStartIn;
			//double valueB = jump.TvSum;

			//avg of the subjumps
			double valueA = jump.TcAvg; //this cares for the -1 on start in. Does not count it.
			double valueB = jump.TvAvg;
			if(UseHeights) {
				valueA = UtilList.GetAverage (jump.FallList);
				valueB = UtilList.GetAverage (jump.HeightList);
			}

			//pointA0_l.Add(new PointF(countToDraw, jump.Jumps));
			pointA1_l.Add(new PointF(countToDraw, valueA));

			pointB_l.Add(new PointF(countToDraw, valueB));
			countToDraw --;

			// 2) Add bottom names
			/*
			string typeRowString = "";
			if (eventGraphJumpsRjStored.type == "") //if "all runs" show run.Type
				typeRowString = jump.Type;
				*/
			//TYPE B: on jumpRj show always jump type to show at the side the number of jumps. If change here, change it above (TYPEA)
			string typeRowString = string.Format("{0} - {1}", jump.Type, jump.Jumps);

			names_l.Add(createTextBelowBar(
						"",
						typeRowString,
						jump.Description,
						thereIsASimulated, (jump.Simulated == -1),
						longestWord.Length, maxRowsForText));

			//add uniqueID two times, one for the each serie
			id_l.Add(jump.UniqueID);
			id_l.Add(jump.UniqueID);

			if (eventGraphJumpsRjStored.selectedID == jump.UniqueID)
				cb.SelectedPos = eventGraphJumpsRjStored.jumpsAtSQL.Count -countToDraw -1;
		}

		cb.Id_l = id_l;

		cb.PassGuidesData (new CairoBarsGuideManage(
					//! ShowPersonNames, true, //usePersonGuides, useGroupGuides
					true, true, //usePersonGuides, useGroupGuides
					eventGraphJumpsRjStored.sessionMAXAtSQL,
					eventGraphJumpsRjStored.sessionAVGAtSQL,
					eventGraphJumpsRjStored.sessionMINAtSQL,
					0,
					eventGraphJumpsRjStored.personMAXAtSQL,
					eventGraphJumpsRjStored.personAVGAtSQL,
					eventGraphJumpsRjStored.personMINAtSQL
					));

		List<List<PointF>> barsSecondary_ll = new List<List<PointF>>();
		barsSecondary_ll.Add(pointA1_l);

		cb.PassData2Series (pointB_l, barsSecondary_ll, false,
				new List<Cairo.Color>(), new List<Cairo.Color>(), names_l,
				"", false,
				-1, fontHeightForBottomNames, bottomMargin, title,
				new List<int> (), new List<int> ());

		passDataForScreenshotIfNeeded ();

		cb.GraphDo();
	}
}

public class CairoPaintBarsPreRunSimple : CairoPaintBarsPre
{
	private bool metersSecondsPreferred;

	public CairoPaintBarsPreRunSimple (DrawingArea darea, string fontStr, Constants.Modes mode, string personName, string testName, int pDN, bool metersSecondsPreferred)
	{
		initialize (darea, fontStr, mode, personName, testName, pDN);
		this.title = generateTitle();
		this.metersSecondsPreferred = metersSecondsPreferred;
	}

	public override void StoreEventGraphRuns (PrepareEventGraphRunSimple eventGraph)
	{
		this.eventGraphRunsStored = eventGraph;
	}

	protected override bool storeCreated ()
	{
		return (eventGraphRunsStored != null);
	}

	protected override bool haveDataToPlot()
	{
		return (eventGraphRunsStored.runsAtSQL.Count > 0);
	}

	protected override void paintSpecific()
	{
		cb = new CairoBars1Series (darea, CairoBars.Type.NORMAL, true, true, true);

		cb.YVariable = Catalog.GetString("Speed");
		if (metersSecondsPreferred)
			cb.YUnits = "m/s";
		else
			cb.YUnits = "Km/h";

		//cb.GraphInit(fontStr, ! ShowPersonNames, true); //usePersonGuides, useGroupGuides
		cb.GraphInit(fontStr, true, true); //usePersonGuides, useGroupGuides

		List<Event> events = Run.RunListToEventList(eventGraphRunsStored.runsAtSQL);

		//find if there is a simulated
		bool thereIsASimulated = false;
		for(int i=0 ; i < eventGraphRunsStored.runsAtSQL.Count; i++)
		{
			if(eventGraphRunsStored.runsAtSQL[i].Simulated == -1)
				thereIsASimulated = true;

			if(! ShowPersonNames)
				eventGraphRunsStored.runsAtSQL[i].Description = ""; //to avoid showing description
		}

		calculateBottomParams (events, eventGraphRunsStored.type == "", "",
				"(" + Catalog.GetString("Simulated") + ")", thereIsASimulated, RunsShowTime);

		List<PointF> point_l = new List<PointF>();
		List<string> names_l = new List<string>();
		List<int> id_l = new List<int>(); //the uniqueIDs for knowing them on bar selection

		int countToDraw = eventGraphRunsStored.runsAtSQL.Count;
		foreach(Run run in eventGraphRunsStored.runsAtSQL)
		{
			// 1) Add data
			run.MetersSecondsPreferred = metersSecondsPreferred;
			point_l.Add(new PointF(countToDraw --, run.Speed));

			// 2) Add bottom names
			string typeRowString = "";
			if (eventGraphRunsStored.type == "") //if "all runs" show run.Type
				typeRowString = run.Type;

			string timeString = "";
			if(RunsShowTime)
				timeString = string.Format("{0} s", Util.TrimDecimals(run.Time, pDN));

			names_l.Add(createTextBelowBar(
						timeString,
						typeRowString,
						run.Description,
						thereIsASimulated, (run.Simulated == -1),
						longestWord.Length, maxRowsForText));

			id_l.Add(run.UniqueID);

			if (eventGraphRunsStored.selectedID == run.UniqueID)
				cb.SelectedPos = eventGraphRunsStored.runsAtSQL.Count -countToDraw -1;
		}

		cb.Id_l = id_l;

		cb.PassGuidesData (new CairoBarsGuideManage(
					//! ShowPersonNames, true, //usePersonGuides, useGroupGuides
					true, true, //usePersonGuides, useGroupGuides
					eventGraphRunsStored.sessionMAXAtSQL,
					eventGraphRunsStored.sessionAVGAtSQL,
					eventGraphRunsStored.sessionMINAtSQL,
					eventGraphRunsStored.personMAXAtSQLAllSessions,
					eventGraphRunsStored.personMAXAtSQL,
					eventGraphRunsStored.personAVGAtSQL,
					eventGraphRunsStored.personMINAtSQL));

		cb.PassData1Serie (point_l,
				new List<Cairo.Color>(), names_l,
				-1, fontHeightForBottomNames, bottomMargin, title,
				new List<int> (), new List<int> ());

		passDataForScreenshotIfNeeded ();

		cb.GraphDo();
	}
}

public class CairoPaintBarsPreRunInterval : CairoPaintBarsPre
{
	private bool metersSecondsPreferred;

	public CairoPaintBarsPreRunInterval (DrawingArea darea, string fontStr, Constants.Modes mode, string personName, string testName, int pDN, bool metersSecondsPreferred)
	{
		initialize (darea, fontStr, mode, personName, testName, pDN);
		this.title = generateTitle();
		this.metersSecondsPreferred = metersSecondsPreferred;
	}

	public override void StoreEventGraphRunsInterval (PrepareEventGraphRunInterval eventGraph)
	{
		this.eventGraphRunsIntervalStored = eventGraph;
	}

	protected override bool storeCreated ()
	{
		return (eventGraphRunsIntervalStored != null);
	}

	protected override bool haveDataToPlot()
	{
		return (eventGraphRunsIntervalStored.runsAtSQL.Count > 0);
	}

	protected override void paintSpecific()
	{
		cb = new CairoBars1Series (darea, CairoBars.Type.NORMAL, true, true, true);

		cb.YVariable = Catalog.GetString("Speed");
		if (metersSecondsPreferred)
			cb.YUnits = "m/s";
		else
			cb.YUnits = "Km/h";

		//cb.GraphInit(fontStr, ! ShowPersonNames, true); //usePersonGuides, useGroupGuides
		cb.GraphInit(fontStr, true, true); //usePersonGuides, useGroupGuides

		List<Event> events = RunInterval.RunIntervalListToEventList (eventGraphRunsIntervalStored.runsAtSQL);

		//find if there is a simulated
		bool thereIsASimulated = false;
		for(int i=0 ; i < eventGraphRunsIntervalStored.runsAtSQL.Count; i++)
		{
			if(eventGraphRunsIntervalStored.runsAtSQL[i].Simulated == -1)
				thereIsASimulated = true;

			if(! ShowPersonNames)
				eventGraphRunsIntervalStored.runsAtSQL[i].Description = ""; //to avoid showing description
		}

		calculateBottomParams (events, true, " - 99", //thinking on 99 tracks
				"(" + Catalog.GetString("Simulated") + ")", thereIsASimulated, RunsShowTime);

		List<PointF> point_l = new List<PointF>();
		List<string> names_l = new List<string>();
		List<int> id_l = new List<int>(); //the uniqueIDs for knowing them on bar selection

		int countToDraw = eventGraphRunsIntervalStored.runsAtSQL.Count;
		foreach(RunInterval runI in eventGraphRunsIntervalStored.runsAtSQL)
		{
			// 1) Add data
			runI.MetersSecondsPreferred = metersSecondsPreferred;
			point_l.Add(new PointF(countToDraw --, runI.Speed));

			// 2) Add bottom names
			/*
			string typeRowString = "";
			if (eventGraphRunsIntervalStored.type == "") //if "all runs" show run.Type
				typeRowString = runI.Type;
				*/
			//TYPE B: on runI show always run type to show at the side the number of tracks. If change here, change it above (TYPEA)
			string typeRowString = string.Format("{0} - {1}", runI.Type, runI.Tracks);

			string timeString = "";
			if(RunsShowTime)
				timeString = string.Format("{0} s", Util.TrimDecimals(runI.TimeTotal, pDN));

			names_l.Add(createTextBelowBar(
						timeString,
						typeRowString,
						runI.Description,
						thereIsASimulated, (runI.Simulated == -1),
						longestWord.Length, maxRowsForText));

			id_l.Add(runI.UniqueID);

			if (eventGraphRunsIntervalStored.selectedID == runI.UniqueID)
				cb.SelectedPos = eventGraphRunsIntervalStored.runsAtSQL.Count -countToDraw -1;
		}

		cb.Id_l = id_l;

		cb.PassGuidesData (new CairoBarsGuideManage(
					//! ShowPersonNames, true, //usePersonGuides, useGroupGuides
					true, true, //usePersonGuides, useGroupGuides
					eventGraphRunsIntervalStored.sessionMAXAtSQL,
					eventGraphRunsIntervalStored.sessionAVGAtSQL,
					eventGraphRunsIntervalStored.sessionMINAtSQL,
					0,
					eventGraphRunsIntervalStored.personMAXAtSQL,
					eventGraphRunsIntervalStored.personAVGAtSQL,
					eventGraphRunsIntervalStored.personMINAtSQL));

		cb.PassData1Serie (point_l,
				new List<Cairo.Color>(), names_l,
				-1, fontHeightForBottomNames, bottomMargin, title,
				new List<int> (), new List<int> ());

		passDataForScreenshotIfNeeded ();

		cb.GraphDo();
	}
}

//realtime jump reactive capture
public class CairoPaintBarsPreJumpReactiveRealtimeCapture : CairoPaintBarsPre
{
	//private double lastTv;
	//private double lastTc;
	private List<double> secondary_l; //bar of the left
	private List<double> main_l; //bar of the right
	private List<Cairo.Color> colorMain_l;
	private List<Cairo.Color> colorSecondary_l;
	private FeedbackJumpsRj feedbackJumpsRj;

	// these are lists because on Runs best speed and best time can be sent,
	// and in the future maybe there are other criterias eg. for encoder
	private List<int> best_l;
	private List<int> worst_l;

	//just blank the screen
	public CairoPaintBarsPreJumpReactiveRealtimeCapture (DrawingArea darea, string fontStr)
	{
		blankScreen(darea, fontStr);
	}

	//isLastCaptured: if what we are showing is currentJumpRj then true, if is a selection from treeview and id != currentJumpRj then is false (meaning selected)

	public CairoPaintBarsPreJumpReactiveRealtimeCapture (DrawingArea darea, string fontStr,
			Constants.Modes mode, string personName, string testName, int pDN,// bool heightPreferred,
			//double lastTv, double lastTc,
			string tvString, string tcString, bool isLastCaptured,
			FeedbackJumpsRj feedbackJumpsRj, double videoTime)
	{
		initialize (darea, fontStr, mode, personName, testName, pDN);
		this.feedbackJumpsRj = feedbackJumpsRj;
		this.videoTime = videoTime;

		if(isLastCaptured)
			this.title = Catalog.GetString("Last test:") + " " + generateTitle();
		else
			this.title = Catalog.GetString("Viewing:") + " " + generateTitle();

		//this.lastTv = lastTv;
		//this.lastTc = lastTc;

		secondary_l = new List<double>();
		main_l = new List<double>();

		if (UseHeights)
		{
			List<double> tv_l = JumpRj.HeightListFromTvString (tvString);
			foreach(double d in tv_l)
				main_l.Add (d);
		} else {
			string [] tvFull = tvString.Split(new char[] {'='});
			string [] tcFull = tcString.Split(new char[] {'='});
			if(tvFull.Length != tcFull.Length)
				return;

			foreach(string tv in tvFull)
				if(Util.IsNumber(tv, true))
					main_l.Add(Convert.ToDouble(tv));
			foreach(string tc in tcFull)
				if(Util.IsNumber(tc, true))
					secondary_l.Add(Convert.ToDouble(tc));
		}

		if (! UseHeights)
		{
			if (feedbackJumpsRj.EmphasizeBestTvTc)
				best_l = getBestWorstTimesList (true);
			else
				best_l = new List<int> ();

			if (feedbackJumpsRj.EmphasizeWorstTvTc)
				worst_l = getBestWorstTimesList (false);
			else
				worst_l = new List<int> ();
		}

		colorMain_l = new List<Cairo.Color>();
		colorSecondary_l = new List<Cairo.Color>();
	}

	private List<int> getBestWorstTimesList (bool best)
	{
		int jump = -1;
		double jumpValue = 0;
		for (int i = 0; i < secondary_l.Count; i ++)
			if (secondary_l[i] > 0 &&
					( jump == -1 ||
					  (best && main_l[i] / secondary_l[i] > jumpValue) ||
					  (! best && main_l[i] / secondary_l[i] < jumpValue) ) )
			{
				jumpValue = main_l[i] / secondary_l[i];
				jump = i;
			}

		List<int> l = new List<int> ();
		if (jump >= 0)
			l.Add (jump);

		return l;
	}

	/*
	public override void StoreEventGraphJumpReactiveCapture (PrepareEventGraphJumpReactiveRealtimeCapture eventGraph)
	{
		this.eventGraphJumpReactiveCapture = eventGraph;
	}
	*/

	protected override bool storeCreated ()
	{
		return (main_l.Count == secondary_l.Count && main_l.Count > 0);
	}

	protected override bool haveDataToPlot()
	{
		return (main_l.Count == secondary_l.Count && main_l.Count > 0);
	}

	protected override void paintSpecific()
	{
		//extra check
		if(main_l.Count != secondary_l.Count)
			return;

		if (UseHeights)
			cb = new CairoBars1Series (darea, CairoBars.Type.NORMAL, false, true, true);
		else
			cb = new CairoBarsNHSeries (darea, CairoBars.Type.NORMAL, true, false, true, true);

		if(UseHeights) {
			cb.YVariable = Catalog.GetString("Height");
			cb.YUnits = "cm";
			cb.VariableSerieB = Catalog.GetString("Jump height");
		} else {
			cb.YVariable = Catalog.GetString("Time");
			cb.YUnits = "s";

			cb.VariableSerieA = Catalog.GetString("Contact time");
			cb.VariableSerieB = Catalog.GetString("Flight time");
		}

		cb.GraphInit(fontStr, true, false); //usePersonGuides, useGroupGuides

		List<PointF> pointA_l = new List<PointF>();
		List<PointF> pointB_l = new List<PointF>();
		List<string> names_l = new List<string>();

		//statistics for tv
		double max = 0;
		double sum = 0; //for main_l avg
		double min = 1000;

		for(int i = 0; i < main_l.Count; i ++)
		{
			double a = 0;
			double b = 0;

			if (UseHeights)
				b = Util.GetHeightInCm (Convert.ToDouble(main_l[i]));
			else {
				a = Convert.ToDouble(secondary_l[i]);
				b = Convert.ToDouble(main_l[i]);
			}

			pointA_l.Add(new PointF(i+1, a));
			pointB_l.Add(new PointF(i+1, b));
			names_l.Add((i+1).ToString());

			//get max (only of tv)
			if(b > max)
				max = b;

			//get avg (only of tv)
			sum += Convert.ToDouble(b);

			//get min (only of tv)
			if(b < min)
				min = b;

			if (UseHeights)
				colorMain_l.Add (feedbackJumpsRj.AssignColorMainByHeight (b));
			else {
				colorMain_l.Add (feedbackJumpsRj.AssignColorMain (b));
				colorSecondary_l.Add (feedbackJumpsRj.AssignColorSecondary (a));
			}
		}

		cb.PassGuidesData (new CairoBarsGuideManage(
					true, false, //usePersonGuides, useGroupGuides
					0,
					0,
					0,
					0,
					max,
					sum / main_l.Count,
					min));

		if (UseHeights)
			cb.PassData1Serie (pointB_l,
					colorMain_l, names_l,
					-1, 14, 8,
					title, best_l, worst_l);
		else {
			List<List<PointF>> barsSecondary_ll = new List<List<PointF>>();
			barsSecondary_ll.Add(pointA_l);
			cb.PassData2Series (pointB_l, barsSecondary_ll, false,
					colorMain_l, colorSecondary_l, names_l,
					"", false,
					-1, 14, 8, title, best_l, worst_l);
		}

		if (videoTime > 0)
			cb.VideoPlayTimeInSeconds = videoTime;

		passDataForScreenshotIfNeeded ();

		cb.GraphDo();
	}
}

public class CairoPaintBarsPreRunIntervalRealtimeCapture : CairoPaintBarsPre
{
	private bool metersSecondsPreferred;
	private bool isRelative; //related to names: distance and time
	//private bool ifRSAstartRest; //on RSA if rest starts, this is true and graph do not need to be updated.
					//but if it is last one then should be painted
					//better manage it different

	private List<double> distance_l;
	private List<double> time_l;
	private List<double> speed_l;
	private List<int> photocell_l;
	private List<Cairo.Color> colorMain_l;
	private FeedbackRunsInterval feedbackRunsI;

	// these are lists because on Runs best speed and best time can be sent,
	// and in the future maybe there are other criterias eg. for encoder
	private List<int> best_l;
	private List<int> worst_l;

	//just blank the screen
	public CairoPaintBarsPreRunIntervalRealtimeCapture (DrawingArea darea, string fontStr)
	{
		blankScreen(darea, fontStr);
	}

	public CairoPaintBarsPreRunIntervalRealtimeCapture (DrawingArea darea, string fontStr,
			Constants.Modes mode, string personName, string testName, int pDN,
			bool metersSecondsPreferred,
			bool isRelative,
			string timesString,
			double distanceInterval, //know each track distance according to this or distancesString
			string distancesString,
			List<int> photocell_l, bool isLastCaptured, FeedbackRunsInterval feedbackRunsI, double videoTime)
	{
		initialize (darea, fontStr, mode, personName, testName, pDN);
		this.metersSecondsPreferred = metersSecondsPreferred;
		this.feedbackRunsI = feedbackRunsI;
		this.videoTime = videoTime;

		if(isLastCaptured)
			this.title = Catalog.GetString("Last test:") + " " + generateTitle();
		else
			this.title = generateTitle();

		this.isRelative = isRelative;

		distance_l = new List<double>();
		time_l = new List<double>();
		speed_l = new List<double>();
		this.photocell_l = photocell_l;

		string [] timeFull = timesString.Split(new char[] {'='});
		int count = 0;
		foreach(string t in timeFull)
		{
			if(distancesString != null && distancesString != "") //if distances are variable
			{
				//this will return a 0 on Rest period on RSA
				distanceInterval = Util.GetRunIVariableDistancesStringRow(distancesString, count);
			}

			//ifRSAstartRest = true;
			if(distanceInterval > 0  //is not RSA rest period
					&&
				Util.IsNumber(t, true))
			{
				double tDouble = Convert.ToDouble(t);
				double time = 0;
				if(tDouble < 0)
					time = 0;
				else
					time = tDouble;

				time_l.Add(time);
				distance_l.Add(distanceInterval);
				if (metersSecondsPreferred)
					speed_l.Add(distanceInterval / time);
				else
					speed_l.Add(3.6 * distanceInterval / time);
				//ifRSAstartRest = false;
			}
			count ++;
		}

		/*
		//debug
		LogB.Information("distances:");
		foreach (double distance in distance_l)
			LogB.Information(distance.ToString());
		LogB.Information("times:");
		foreach (double time in time_l)
			LogB.Information(time.ToString());
		LogB.Information("speeds:");
		foreach (double speed in speed_l)
			LogB.Information(speed.ToString());
		*/

		best_l = new List<int> ();
		if (feedbackRunsI.EmphasizeBestSpeed)
			best_l = getBestWorstList (best_l, speed_l, true);
		if (feedbackRunsI.EmphasizeBestTime)
			best_l = getBestWorstList (best_l, time_l, false);

		worst_l = new List<int> ();
		if (feedbackRunsI.EmphasizeWorstSpeed)
			worst_l = getBestWorstList (worst_l, speed_l, false);
		if (feedbackRunsI.EmphasizeWorstTime)
			worst_l = getBestWorstList (worst_l, time_l, true);

		colorMain_l = new List<Cairo.Color>();
	}

	private List<int> getBestWorstList (List<int> return_l, List<double> find_l, bool higher)
	{
		int run = -1;
		double runValue = 0;
		for (int i = 0; i < find_l.Count; i ++)
			if (find_l[i] > 0 &&
					( run == -1 ||
					  (higher && find_l[i] > runValue) ||
					  (! higher && find_l[i] < runValue) ) )
			{
				runValue = find_l[i];
				run = i;
			}

		if (run >= 0)
			return_l.Add (run);

		return return_l;
	}
	/*
	public override void StoreEventGraphJumpReactiveCapture (PrepareEventGraphJumpReactiveRealtimeCapture eventGraph)
	{
		this.eventGraphJumpReactiveCapture = eventGraph;
	}
	*/

	protected override bool storeCreated ()
	{
		return (speed_l.Count == time_l.Count && speed_l.Count > 0);
	}

	protected override bool haveDataToPlot()
	{
		return (speed_l.Count == time_l.Count && speed_l.Count > 0);
	}

	protected override void paintSpecific()
	{
		//extra check
		if(speed_l.Count != time_l.Count)
			return;

		//if(ifRSAstartRest)
		//	return;

		cb = new CairoBars1Series (darea, CairoBars.Type.NORMAL, false, true, true);

		cb.YVariable = Catalog.GetString("Speed");
		if (metersSecondsPreferred)
			cb.YUnits = "m/s";
		else
			cb.YUnits = "Km/h";

		cb.GraphInit(fontStr, true, false); //usePersonGuides, useGroupGuides

		List<PointF> point_l = new List<PointF>();
		List<string> names_l = new List<string>();

		//statistics for speed
		double max = 0;
		double min = 1000;

		//for absolute data. Absolute is from the beginning.
		double distanceTotal = 0;
		double timeTotal = 0;
		for(int i = 0; i < time_l.Count; i ++)
		{
			distanceTotal += distance_l[i];
			timeTotal += time_l[i];
		}
		double distanceAccumulated = 0;
		double timeAccumulated = 0;

		for(int i = 0; i < time_l.Count; i ++)
		{
			double time = Convert.ToDouble(time_l[i]);
			double speed = Convert.ToDouble(speed_l[i]);

			point_l.Add(new PointF(i+1, speed));

			if(isRelative)
				names_l.Add(string.Format("{0} m\n{1} s",
							distance_l[i], Util.TrimDecimals(time,2)));
			else {
				distanceAccumulated += distance_l[i];
				timeAccumulated += time_l[i];
				names_l.Add(string.Format("{0} m\n{1} s",
							distanceAccumulated, Util.TrimDecimals(timeAccumulated,2)));
			}

			if(speed > max) 	//get max
				max = speed;

			if(speed < min)		//get min
				min = speed;

			colorMain_l.Add (feedbackRunsI.AssignColorMain (speed, time));
		}

		cb.PassGuidesData (new CairoBarsGuideManage(
					true, false, //usePersonGuides, useGroupGuides
					0, 0, 0, 0,
					max,
					UtilAll.DivideSafe(distanceTotal, timeTotal),
					min));
		/*
		   if(photocell_l.Count > 0)
			cb.InBarNums_l = photocell_l;
		 */
		if(photocell_l.Count > 0)
			cb.EdgeBarNums_l = photocell_l;

		cb.SpaceBetweenBars = false;

		cb.PassData1Serie (point_l,
				colorMain_l, names_l,
				-1, 14, 22, title, //22 because there are two rows
				best_l, worst_l);

		if (videoTime > 0)
		{
			cb.VideoPlayTimeInSeconds = videoTime;

			//cb.VideoPlayTimes_l = time_l; //VideoPlayTimes is accumulative)
			cb.VideoPlayTimes_l = UtilList.Cumsum (time_l);
		}

		passDataForScreenshotIfNeeded ();

		cb.GraphDo();
	}
}

public class CairoManageRunDoubleContacts
{
	private DrawingArea darea;
	private string fontStr;

	public CairoManageRunDoubleContacts (DrawingArea darea, string fontStr)
	{
		this.darea = darea;
		this.fontStr = fontStr;
	}

	public void Paint (EventExecute currentEventExecute, RunPhaseTimeList runPTL, double timeTotal,
			string intervalTimesString) //"" on runSimple
	{
		if(darea == null || darea.Window == null) //at start program, this can fail
			return;

		// 1) get data
		List<RunPhaseTimeListObject> runPTLInListForPainting = runPTL.InListForPainting();

		double negativePTLTime = getRunSRunINegativePTLTime(runPTLInListForPainting);
		double timeTotalWithExtraPTL = getRunSRunITimeTotalWithExtraPTLTime (timeTotal, runPTLInListForPainting, negativePTLTime);

		LogB.Information(string.Format("timeTotal: {0}, negativePTLTime: {1}, timeTotalWithExtraPTL: {2}",
					timeTotal, negativePTLTime, timeTotalWithExtraPTL));

		// 2) graph
		CairoRunDoubleContacts crdc;
		if(intervalTimesString == "") //runSimple
			crdc = new CairoRunDoubleContacts (darea, fontStr);
		else
			crdc = new CairoRunIntervalDoubleContacts (darea, fontStr, intervalTimesString);

		crdc.GraphDo (runPTLInListForPainting,
				timeTotal, timeTotalWithExtraPTL, negativePTLTime);
	}

	private double getRunSRunINegativePTLTime (List<RunPhaseTimeListObject> runPTLInListForPainting)
	{
		return 0;

		/*
		 * inactive because when user stays lot of time (in contact) before the test, with this code the graph will have almost this contact status
		 * having above return 0; fixed this: https://gitlab.gnome.org/GNOME/chronojump/issues/110
		 *
		if(runPTLInListForPainting.Count > 0)
		{
			//get first TC start value
			RunPhaseTimeListObject rptlfp = (RunPhaseTimeListObject) runPTLInListForPainting[0];
			double firstValue = rptlfp.tcStart;

			if(firstValue < 0)
				return Math.Abs(firstValue);
		}

		return 0;
		*/
	}

	private double getRunSRunITimeTotalWithExtraPTLTime (double timeTotal, List<RunPhaseTimeListObject> runPTLInListForPainting, double negativePTLTime)
	{
		double timeTotalWithExtraPTL = timeTotal;
		if(runPTLInListForPainting.Count > 0)
		{
			//get last TC end value
			RunPhaseTimeListObject rptlfp = (RunPhaseTimeListObject) runPTLInListForPainting[runPTLInListForPainting.Count -1];
			timeTotalWithExtraPTL = rptlfp.tcEnd;
		}

		return timeTotalWithExtraPTL + negativePTLTime;
	}
}

public class CairoPaintBarsPreRunEncoder : CairoPaintBarsPre
{
	public CairoPaintBarsPreRunEncoder (DrawingArea darea, string fontStr, Constants.Modes mode, string personName, string testName, int pDN)
	{
		LogB.Information ("CairoPaintBarsPreRunEncoder constructor");
		initialize (darea, fontStr, mode, personName, testName, pDN);
		this.title = generateTitle();
	}

	public override void StoreEventGraphRunEncoder (PrepareEventGraphRunEncoder eventGraph)
	{
		this.eventGraphRunEncoderStored = eventGraph;
	}

	protected override bool storeCreated ()
	{
		return (eventGraphRunEncoderStored != null);
	}

	protected override bool haveDataToPlot()
	{
		return (eventGraphRunEncoderStored.rowsAtSQL.Count > 0);
	}

	protected override void paintSpecific()
	{
		LogB.Information ("CairoPaintBarsPreRunEncoder paintSpecific");
		/*
		 * check if one bar has to be shown or two
		 * this is important when we are showing multitests
		 */
		cb = new CairoBarsNHSeries (darea, CairoBars.Type.NORMAL, true, true, true, true);

		cb.YVariable = Catalog.GetString("Speed");
		cb.YUnits = "m/s";
		cb.VariableSerieA = Catalog.GetString("Max speed");
		cb.VariableSerieB = Catalog.GetString("Best second");

		//cb.GraphInit(fontStr, ! ShowPersonNames, true); //usePersonGuides, useGroupGuides
		cb.GraphInit(fontStr, true, true); //usePersonGuides, useGroupGuides

		List<Event> events = RunEncoder.RunEncoderListToEventList (eventGraphRunEncoderStored.rowsAtSQL);

		for (int i=0 ; i < eventGraphRunEncoderStored.rowsAtSQL.Count; i++)
			if(! ShowPersonNames)
				eventGraphRunEncoderStored.rowsAtSQL[i].Description = ""; //to avoid showing description

		//findLongestWordCairo uses Type (from Event) but RunEncoder has ExerciseName
		for (int i = 0; i < events.Count; i ++)
			events[i].Type = ((RunEncoder) events[i]).ExerciseName;

		calculateBottomParams (events, eventGraphRunEncoderStored.exerciseAll, "",
				"", false, false);

		List<PointF> pointA_l = new List<PointF>();
		List<PointF> pointB_l = new List<PointF>();
		List<string> names_l = new List<string>();
		List<int> id_l = new List<int>(); //the uniqueIDs for knowing them on bar selection

		int countToDraw = eventGraphRunEncoderStored.rowsAtSQL.Count;
		foreach (RunEncoder re in eventGraphRunEncoderStored.rowsAtSQL)
		{
			//LogB.Information("forceSensor: " + re.ToString());
			// 1) Add data
			pointA_l.Add (new PointF (countToDraw, re.MaxSpeed));
			pointB_l.Add (new PointF (countToDraw, re.MaxAvgSpeed1s));
			countToDraw --;

			// 2) Add bottom names
			string typeRowString = "";
			if (eventGraphRunEncoderStored.exerciseAll) //if "all tests" show type
				typeRowString = re.ExerciseName; //TODO: check this param is filled

			names_l.Add (createTextBelowBar(
						"",
						typeRowString,
						re.Description,
						false, false,
						longestWord.Length, maxRowsForText));

			id_l.Add (re.UniqueID);
			id_l.Add (re.UniqueID);

			if (eventGraphRunEncoderStored.selectedID == re.UniqueID)
				cb.SelectedPos = eventGraphRunEncoderStored.rowsAtSQL.Count -countToDraw -1;
		}

		cb.Id_l = id_l;

		/*
		cb.PassGuidesData (new CairoBarsGuideManage(
					//! ShowPersonNames, true, //usePersonGuides, useGroupGuides
					true, true, //usePersonGuides, useGroupGuides
					eventGraphJumpsStored.sessionMAXAtSQL,
					eventGraphJumpsStored.sessionAVGAtSQL,
					eventGraphJumpsStored.sessionMINAtSQL,
					eventGraphJumpsStored.personMAXAtSQLAllSessions,
					eventGraphJumpsStored.personMAXAtSQL,
					eventGraphJumpsStored.personAVGAtSQL,
					eventGraphJumpsStored.personMINAtSQL));
		*/

		List<List<PointF>> barsSecondary_ll = new List<List<PointF>>();
		barsSecondary_ll.Add(pointA_l);

		cb.PassData2Series (pointB_l, barsSecondary_ll, false,
				new List<Cairo.Color>(), new List<Cairo.Color>(), names_l,
				"", false,
				-1, fontHeightForBottomNames, bottomMargin, title,
				new List<int> (), new List<int> ());

		passDataForScreenshotIfNeeded ();

		cb.GraphDo();
	}
}

public class CairoPaintBarsWilight : CairoPaintBarsPre
{
	public CairoPaintBarsWilight (DrawingArea darea, string fontStr, Constants.Modes mode, string personName, string testName, int pDN)
	{
		initialize (darea, fontStr, mode, personName, testName, pDN);
		this.title = generateTitle();
	}

	public override void StoreEventGraphWilight (PrepareEventGraphWilight eventGraph)
	{
		this.eventGraphWilightStored = eventGraph;
	}

	protected override bool storeCreated ()
	{
		return (eventGraphWilightStored != null);
	}

	protected override bool haveDataToPlot()
	{
		return (eventGraphWilightStored.rowsAtSQL.Count > 0);
	}

	protected override void paintSpecific()
	{
		cb = new CairoBars1Series (darea, CairoBars.Type.NORMAL, true, true, true);

		cb.YVariable = Catalog.GetString("Time");
		cb.YUnits = "s";

		//cb.GraphInit(fontStr, ! ShowPersonNames, true); //usePersonGuides, useGroupGuides
		cb.GraphInit(fontStr, true, true); //usePersonGuides, useGroupGuides

		List<Event> events = Wilight.WilightListToEventList (eventGraphWilightStored.rowsAtSQL);

		List<PointF> point_l = new List<PointF>();
		List<string> names_l = new List<string>();
		List<int> id_l = new List<int>(); //the uniqueIDs for knowing them on bar selection

		calculateBottomParams (events, true, "", "", false, false);

		int countToDraw = eventGraphWilightStored.rowsAtSQL.Count;
		foreach (Wilight wilight in eventGraphWilightStored.rowsAtSQL)
		{
			// 1) Add data
			point_l.Add(new PointF(countToDraw --, UtilAll.DivideSafe (wilight.TotalMs, 1000)));

			// 2) Add bottom names
			string typeRowString = "";
			//if (eventGraphWilightStored.type == "")
			//	typeRowString = jump.Type;

			names_l.Add (createTextBelowBar(
						"",
						typeRowString,
						wilight.Description, //person name
						false, false,
						longestWord.Length, maxRowsForText));

			id_l.Add (wilight.UniqueID);

			if (eventGraphWilightStored.selectedID == wilight.UniqueID)
				cb.SelectedPos = eventGraphWilightStored.rowsAtSQL.Count -countToDraw -1;
		}
		cb.Id_l = id_l;

		cb.PassData1Serie (point_l,
				new List<Cairo.Color>(), names_l,
				-1, fontHeightForBottomNames, bottomMargin, title,
				new List<int> (), new List<int> ());

		passDataForScreenshotIfNeeded ();

		cb.GraphDo();
	}
}

//copied from CairoPaintBarsWilight. TODO: unify them
public class CairoPaintBarsFourPlatforms : CairoPaintBarsPre
{
	public CairoPaintBarsFourPlatforms (DrawingArea darea, string fontStr, Constants.Modes mode, string personName, string testName, int pDN)
	{
		initialize (darea, fontStr, mode, personName, testName, pDN);
		this.title = generateTitle();
	}

	public override void StoreEventGraphFourPlatforms (PrepareEventGraphFourPlatforms eventGraph)
	{
		this.eventGraphFourPlatformsStored = eventGraph;
	}

	protected override bool storeCreated ()
	{
		return (eventGraphFourPlatformsStored != null);
	}

	protected override bool haveDataToPlot()
	{
		return (eventGraphFourPlatformsStored.rowsAtSQL.Count > 0);
	}

	protected override void paintSpecific()
	{
		cb = new CairoBars1Series (darea, CairoBars.Type.NORMAL, true, true, true);

		cb.YVariable = Catalog.GetString("Time");
		cb.YUnits = "s";

		//cb.GraphInit(fontStr, ! ShowPersonNames, true); //usePersonGuides, useGroupGuides
		cb.GraphInit(fontStr, true, true); //usePersonGuides, useGroupGuides

		List<Event> events = FourPlatforms.FourPlatformsListToEventList (eventGraphFourPlatformsStored.rowsAtSQL);

		List<PointF> point_l = new List<PointF>();
		List<string> names_l = new List<string>();
		List<int> id_l = new List<int>(); //the uniqueIDs for knowing them on bar selection

		calculateBottomParams (events, true, "", "", false, false);

		int countToDraw = eventGraphFourPlatformsStored.rowsAtSQL.Count;
		foreach (FourPlatforms fp in eventGraphFourPlatformsStored.rowsAtSQL)
		{
			// 1) Add data
			//point_l.Add(new PointF(countToDraw --, UtilAll.DivideSafe (fp.TotalMs, 1000)));
			point_l.Add (new PointF(countToDraw --, fp.TotalTime));

			// 2) Add bottom names
			string typeRowString = "";
			//if (eventGraphFourPlatformsStored.type == "")
			//	typeRowString = jump.Type;

			names_l.Add (createTextBelowBar(
						"",
						typeRowString,
						fp.Description, //person name
						false, false,
						longestWord.Length, maxRowsForText));

			id_l.Add (fp.UniqueID);

			if (eventGraphFourPlatformsStored.selectedID == fp.UniqueID)
				cb.SelectedPos = eventGraphFourPlatformsStored.rowsAtSQL.Count -countToDraw -1;
		}
		cb.Id_l = id_l;

		cb.PassData1Serie (point_l,
				new List<Cairo.Color>(), names_l,
				-1, fontHeightForBottomNames, bottomMargin, title,
				new List<int> (), new List<int> ());

		passDataForScreenshotIfNeeded ();

		cb.GraphDo();
	}
}

public class CairoPaintBarsPreForceSensor : CairoPaintBarsPre
{
	public CairoPaintBarsPreForceSensor (DrawingArea darea, string fontStr, Constants.Modes mode, string personName, string testName, int pDN)
	{
		LogB.Information ("CairoPaintBarsPreForceSensor constructor");
		initialize (darea, fontStr, mode, personName, testName, pDN);
		this.title = generateTitle();
	}

	public override void StoreEventGraphForceSensor (PrepareEventGraphForceSensor eventGraph)
	{
		this.eventGraphForceSensorStored = eventGraph;
	}

	protected override bool storeCreated ()
	{
		return (eventGraphForceSensorStored != null);
	}

	protected override bool haveDataToPlot()
	{
		return (eventGraphForceSensorStored.rowsAtSQL.Count > 0);
	}

	protected override void paintSpecific()
	{
		LogB.Information ("CairoPaintBarsPreForceSensor paintSpecific");
		/*
		 * check if one bar has to be shown or two
		 * this is important when we are showing multitests
		 */
		cb = new CairoBarsNHSeries (darea, CairoBars.Type.NORMAL, true, true, true, true);

		cb.YVariable = Catalog.GetString("Force");
		cb.YUnits = "N";
		cb.VariableSerieA = Catalog.GetString("Max force");
		cb.VariableSerieB = Catalog.GetString("Best second");

		//cb.GraphInit(fontStr, ! ShowPersonNames, true); //usePersonGuides, useGroupGuides
		cb.GraphInit(fontStr, true, true); //usePersonGuides, useGroupGuides

		List<Event> events = ForceSensor.ForceSensorListToEventList (eventGraphForceSensorStored.rowsAtSQL);

		for (int i=0 ; i < eventGraphForceSensorStored.rowsAtSQL.Count; i++)
			if(! ShowPersonNames)
				eventGraphForceSensorStored.rowsAtSQL[i].Description = ""; //to avoid showing description

		//findLongestWordCairo uses Type (from Event) but ForceSensor has ExerciseName
		for (int i = 0; i < events.Count; i ++)
			events[i].Type = ((ForceSensor) events[i]).ExerciseName;

		calculateBottomParams (events, eventGraphForceSensorStored.exerciseAll, "",
				"", false, false);

		List<PointF> pointA_l = new List<PointF>();
		List<PointF> pointB_l = new List<PointF>();
		List<string> names_l = new List<string>();
		List<int> id_l = new List<int>(); //the uniqueIDs for knowing them on bar selection

		int countToDraw = eventGraphForceSensorStored.rowsAtSQL.Count;
		foreach (ForceSensor fs in eventGraphForceSensorStored.rowsAtSQL)
		{
			//LogB.Information("forceSensor: " + fs.ToString());
			// 1) Add data
			pointA_l.Add (new PointF (countToDraw, fs.MaxForceRaw));
			pointB_l.Add (new PointF (countToDraw, fs.MaxAvgForce1s));
			countToDraw --;

			// 2) Add bottom names
			string typeRowString = "";
			if (eventGraphForceSensorStored.exerciseAll) //if "all tests" show type
				typeRowString = fs.ExerciseName; //TODO: check this param is filled

			names_l.Add (createTextBelowBar(
						"",
						typeRowString,
						fs.Description,
						false, false,
						longestWord.Length, maxRowsForText));

			id_l.Add (fs.UniqueID);
			id_l.Add (fs.UniqueID);

			if (eventGraphForceSensorStored.selectedID == fs.UniqueID)
				cb.SelectedPos = eventGraphForceSensorStored.rowsAtSQL.Count -countToDraw -1;
		}

		cb.Id_l = id_l;

		/*
		cb.PassGuidesData (new CairoBarsGuideManage(
					//! ShowPersonNames, true, //usePersonGuides, useGroupGuides
					true, true, //usePersonGuides, useGroupGuides
					eventGraphJumpsStored.sessionMAXAtSQL,
					eventGraphJumpsStored.sessionAVGAtSQL,
					eventGraphJumpsStored.sessionMINAtSQL,
					eventGraphJumpsStored.personMAXAtSQLAllSessions,
					eventGraphJumpsStored.personMAXAtSQL,
					eventGraphJumpsStored.personAVGAtSQL,
					eventGraphJumpsStored.personMINAtSQL));
		*/

		List<List<PointF>> barsSecondary_ll = new List<List<PointF>>();
		barsSecondary_ll.Add(pointA_l);

		cb.PassData2Series (pointB_l, barsSecondary_ll, false,
				new List<Cairo.Color>(), new List<Cairo.Color>(), names_l,
				"", false,
				-1, fontHeightForBottomNames, bottomMargin, title,
				new List<int> (), new List<int> ());

		passDataForScreenshotIfNeeded ();

		cb.GraphDo();
	}
}

public class CairoPaintBarsPreEncoderCurrent : CairoPaintBarsPre
{
	// without this, while capturing, if screen is minimized/maximized, or any redraw sounds are played again!
	// This is emptied at capture start
	public static List<int> RepetitionsPlayed_l = new List<int> ();

	private PrepareEventGraphEncoderCurrent pegbe;
	private Preferences preferences;

	//copied from gui/encoderGraphObjects (using ArrayList)
	private ArrayList data; //data is related to mainVariable (barplot)
	private List<double> lineData_l; //related to secondary variable (by default range (mm))
	private List<double> dataStart_l; //used on video (in seconds)
	private List<double> dataDuration_l; //used on video (in seconds)
	private ArrayList dataRangeOfMovement; //ROM, need it to discard last rep for loss. Is not the same as lineData_l because maybe user selected another variable as secondary. only checks con.
	private ArrayList dataWorkJ;
	private ArrayList dataImpulse;
	private CairoBarsArrow cairoBarsArrow;

	private double countValid;
	private double sumValid;
	private double sumSaved;
	private int countSaved;
	private double maxThisSetValidAndCon;
	private double minThisSetValidAndCon;
	//we need the position to draw the loss line and maybe to manage that the min should be after the max (for being real loss)
	private int maxThisSetValidAndConPos;
	private int minThisSetValidAndConPos;
	double workTotal; //can be J or Kcal (shown in cal)
	double impulseTotal;

	private List<PointF> barA_l; //data is related to mainVariable (barplot)
	private List<PointF> barB_l; //data is related to mainVariable (barplot)
	private List<Cairo.Color> colorMain_l;
	private List<Cairo.Color> colorSecondary_l;
	private List<string> names_l;
	private List<int> saved_l; //saved repetitions
	private List<CairoBarsArrow> eccOverload_l;

	//used on encoder when !relativeToSet
	private double maxAbsoluteForCalc;
	private double maxThisSetForCalc;
	private double maxThisSetSaved; //cb.MaxIntersession will be the greatest of pegbe.maxPowerSpeedForceIntersession and best saved repetition in this set

	private string units;
	private string titleStr;
	private string lossStr;
	private string workStr;
	private string impulseStr;

	private bool noMassAndNeeded;

	//just blank the screen
	public CairoPaintBarsPreEncoderCurrent (DrawingArea darea, string fontStr)
	{
		blankScreen(darea, fontStr);
	}

	//isLastCaptured: if what we are showing is currentJumpRj then true, if is a selection from treeview and id != currentJumpRj then is false (meaning selected)

	public CairoPaintBarsPreEncoderCurrent (Preferences preferences, DrawingArea darea, string fontStr,
			string personName, string testName, int pDN,
			PrepareEventGraphEncoderCurrent pegbe, double videoTime)
	{
		this.pegbe = pegbe;
		this.videoTime = videoTime;

		NewPreferences (preferences);
		//messageNoStoreCreated = " no criteria ";

		initialize (darea, fontStr, mode, personName, testName, pDN);

		if (noMassAndNeededCheck ())
		{
			noMassAndNeeded = true;
			return;
		}

		//calcule all graph stuff
		fillArraysDiscardingReps ();
		fillVariableListsForGraph ();
		prepareTitle ();
		prepareLossArrow ();
	}

	private bool noMassAndNeededCheck ()
	{
		if (! pegbe.hasInertia && pegbe.massDisplaced < 0.00001 &&
				(pegbe.mainVariable != Constants.Range && pegbe.mainVariable != Constants.RangeAbsolute &&
				 pegbe.mainVariable != Constants.MeanSpeed && pegbe.mainVariable != Constants.MaxSpeed) )
				 return true;

		return false;
	}

	public override void ShowMessage (DrawingArea darea, string fontTypeStr, string message)
	{
		if(darea == null)
			return;

		this.darea = darea;
		cb = new CairoBars1Series (darea, CairoBars.Type.ENCODER, fontTypeStr, message);
	}

	protected override bool storeCreated ()
	{
		return (pegbe != null && pegbe.encoderBarsData_l.Count > 0);
	}

	protected override bool haveDataToPlot()
	{
		return (pegbe != null && pegbe.encoderBarsData_l.Count > 0);
	}

	protected override void paintSpecific()
	{
		if (noMassAndNeeded)
			ShowMessage (darea, preferences.fontTypeToGraph(),
					Catalog.GetString("Main variable:") + " " + Catalog.GetString(pegbe.mainVariable) + "\n\n" +
					Catalog.GetString("The bars are not shown because the displaced mass is 0."));
		else
			paintSpecificDo ();
	}

	//preferences can change
	public void NewPreferences (Preferences preferences)
	{
		this.preferences = preferences;

		pegbe.discardFirstN = preferences.encoderCaptureInertialDiscardFirstN;
		pegbe.showNRepetitions = preferences.encoderCaptureShowNRepetitions;
	}

	private void fillArraysDiscardingReps () //copied from gui/encoderGraphObjects fillDataVariables()
	{
		data = new ArrayList (pegbe.encoderBarsData_l.Count); //data is related to mainVariable (barplot)
		lineData_l = new List<double>(); //lineData_l is related to secondary variable (by default range)
		dataStart_l = new List<double> ();
		dataDuration_l = new List<double> ();
		dataRangeOfMovement = new ArrayList (pegbe.encoderBarsData_l.Count);
		dataWorkJ = new ArrayList (pegbe.encoderBarsData_l.Count);
		dataImpulse = new ArrayList (pegbe.encoderBarsData_l.Count);
		bool lastIsEcc = false;
		int count = 0;

		//discard repetitions according to pegbe.showNRepetitions
		foreach(EncoderBarsData ebd in pegbe.encoderBarsData_l)
		{
			//LogB.Information(string.Format("count: {0}, value: {1}", count, ebd.GetValue(pegbe.mainVariable)));
			//when capture ended, show all repetitions
			if(pegbe.showNRepetitions == -1 || ! pegbe.capturing)
			{
				data.Add(ebd.GetValue(pegbe.mainVariable));
				if(pegbe.secondaryVariable != "")
					lineData_l.Add(ebd.GetValue(pegbe.secondaryVariable));
				dataStart_l.Add (UtilAll.DivideSafe (ebd.GetValue (Constants.Start), 1000));
				dataDuration_l.Add (UtilAll.DivideSafe (ebd.GetValue (Constants.Duration), 1000));
				dataRangeOfMovement.Add(ebd.GetValue(Constants.RangeAbsolute));
				dataWorkJ.Add(ebd.GetValue(Constants.WorkJ));
				dataImpulse.Add(ebd.GetValue(Constants.Impulse));
			}
			else {
				if(pegbe.eccon == "c" && ( pegbe.encoderBarsData_l.Count <= pegbe.showNRepetitions || 	//total repetitions are less than show repetitions threshold ||
						count >= pegbe.encoderBarsData_l.Count - pegbe.showNRepetitions ) ) 	//count is from the last group of reps (reps that have to be shown)
				{
					data.Add(ebd.GetValue(pegbe.mainVariable));
					if(pegbe.secondaryVariable != "")
						lineData_l.Add(ebd.GetValue(pegbe.secondaryVariable));
					dataStart_l.Add (UtilAll.DivideSafe (ebd.GetValue (Constants.Start), 1000));
					dataDuration_l.Add (UtilAll.DivideSafe (ebd.GetValue (Constants.Duration), 1000));
					dataRangeOfMovement.Add(ebd.GetValue(Constants.RangeAbsolute));
					dataWorkJ.Add(ebd.GetValue(Constants.WorkJ));
					dataImpulse.Add(ebd.GetValue(Constants.Impulse));
				}
				else if(pegbe.eccon != "c" && (
						pegbe.encoderBarsData_l.Count <= 2 * pegbe.showNRepetitions ||
						count >= pegbe.encoderBarsData_l.Count - 2 * pegbe.showNRepetitions) )
				{
					if(! Util.IsEven(count +1))  	//if it is "impar"
					{
						LogB.Information("added ecc");
						data.Add(ebd.GetValue(pegbe.mainVariable));
						if(pegbe.secondaryVariable != "")
							lineData_l.Add(ebd.GetValue(pegbe.secondaryVariable));
						dataStart_l.Add (UtilAll.DivideSafe (ebd.GetValue (Constants.Start), 1000));
						dataDuration_l.Add (UtilAll.DivideSafe (ebd.GetValue (Constants.Duration), 1000));
						dataRangeOfMovement.Add(ebd.GetValue(Constants.RangeAbsolute));
						dataWorkJ.Add(ebd.GetValue(Constants.WorkJ));
						dataImpulse.Add(ebd.GetValue(Constants.Impulse));
						lastIsEcc = true;
					} else {  			//it is "par"
						if(lastIsEcc)
						{
							data.Add(ebd.GetValue(pegbe.mainVariable));
							if(pegbe.secondaryVariable != "")
								lineData_l.Add(ebd.GetValue(pegbe.secondaryVariable));
							dataStart_l.Add (UtilAll.DivideSafe (ebd.GetValue (Constants.Start), 1000));
							dataDuration_l.Add (UtilAll.DivideSafe (ebd.GetValue (Constants.Duration), 1000));
							dataRangeOfMovement.Add(ebd.GetValue(Constants.RangeAbsolute));
							dataWorkJ.Add(ebd.GetValue(Constants.WorkJ));
							dataImpulse.Add(ebd.GetValue(Constants.Impulse));
							LogB.Information("added con");
							lastIsEcc = false;
						}
					}
				}
			}
			//LogB.Information("data workJ: " + dataWorkJ[count].ToString());
			count ++;
		}
	}

	private void fillVariableListsForGraph ()
	{
		barA_l = new List<PointF>(); //data is related to mainVariable (barplot)
		barB_l = new List<PointF>(); //data is related to mainVariable (barplot)

		colorMain_l = new List<Cairo.Color>();
		colorSecondary_l = new List<Cairo.Color>();
		names_l = new List<string>();
		saved_l = new List<int>();

		//Gdk colors from (soon deleted) encoderGraphDoPlot()
		RGBA colorPhase = new RGBA ();

		//final color of the bar
		Cairo.Color colorBar = new Cairo.Color();

		int count = 0;

		//Get max min avg values of this set
		double maxThisSetForGraph = -100000;
		maxThisSetForCalc = -100000;
		maxThisSetSaved = -100000;
		double minThisSet = 100000;
		/*
		 * if ! Preferences.EncoderPhasesEnum.BOTH, eg: ECC, we can graph max CON (that maybe is the highest value) , but for calculations we want only the max ECC value, so:
		 * maxThisSetForGraph will be to plot the margins,
		 * maxThisSetForCalc will be to calculate feedback (% of max)
		 */

		//only used for loss. For loss only con phase is used
		maxThisSetValidAndCon = maxThisSetForCalc;
		minThisSetValidAndCon = minThisSet;
		//we need the position to draw the loss line and maybe to manage that the min should be after the max (for being real loss)
		maxThisSetValidAndConPos = 0;
		minThisSetValidAndConPos = 0;

		//know not-discarded phases
		countValid = 0;
		sumValid = 0;
		sumSaved = 0;
		countSaved = 0;
		workTotal = 0; //can be J or Kcal (shown in cal)
		impulseTotal = 0;

		foreach(double d in data)
		{
			if(d > maxThisSetForGraph)
				maxThisSetForGraph = d;

			if(pegbe.eccon == "c" ||
					preferences.encoderCaptureFeedbackEccon == Preferences.EncoderPhasesEnum.BOTH ||
					preferences.encoderCaptureFeedbackEccon == Preferences.EncoderPhasesEnum.ECC && ! Util.IsEven(count +1) || //odd (impar)
					preferences.encoderCaptureFeedbackEccon == Preferences.EncoderPhasesEnum.CON && Util.IsEven(count +1) ) //even (par)
			{
				if(d > maxThisSetForCalc)
					maxThisSetForCalc = d;
			}

			if(d < minThisSet)
				minThisSet = d;

			if( pegbe.hasInertia && pegbe.discardFirstN > 0 &&
					  ((pegbe.eccon == "c" && count < pegbe.discardFirstN) || (pegbe.eccon != "c" && count < pegbe.discardFirstN * 2)) )
				LogB.Information("Discarded phase");
			else if(pegbe.eccon == "c" ||
					preferences.encoderCaptureFeedbackEccon == Preferences.EncoderPhasesEnum.BOTH ||
					preferences.encoderCaptureFeedbackEccon == Preferences.EncoderPhasesEnum.ECC && ! Util.IsEven(count +1) || //odd (impar)
					preferences.encoderCaptureFeedbackEccon == Preferences.EncoderPhasesEnum.CON && Util.IsEven(count +1) )	//even (par)
			{
				countValid ++;
				sumValid += d;
				bool needChangeMin = false;

				if(pegbe.eccon == "c" || Util.IsEven(count +1)) //par
				{
					if(d > maxThisSetValidAndCon) {
						maxThisSetValidAndCon = d;
						maxThisSetValidAndConPos = count;

						//min rep has to be after max
						needChangeMin = true;
					}
					if(needChangeMin || (d < minThisSetValidAndCon &&
								Convert.ToDouble(dataRangeOfMovement[count]) >= .7 * Convert.ToDouble(dataRangeOfMovement[maxThisSetValidAndConPos])
								//ROM of this rep cannot be lower than 70% of ROM of best rep (helps to filter when you leave the weight on the bar...)
							    ) ) {
						minThisSetValidAndCon = d;
						minThisSetValidAndConPos = count;
					}
				}
			}

			count ++;
		}

		maxAbsoluteForCalc = maxThisSetForCalc;
		//can be on meanPower, meanSpeed, meanForce
		if(! pegbe.relativeToSet)
		{
			//relative to historical of this person

			/*
			 *
			 * if there's a set captured but without repetitions saved, maxPowerSpeedForceIntersession will be 0
			 * and current set (loaded or captured) will have a power that will be out of the graph
			 * for this reason use maxAbsolute or maxThisSet, whatever is higher
			 *
			 * if ! relativeToSet, then Preferences.EncoderPhasesEnum.BOTH, so maxAbsoluteForCalc == maxAbsoluteForGraph
			 */
			if(pegbe.maxPowerSpeedForceIntersession > maxAbsoluteForCalc)
			{
				maxAbsoluteForCalc = pegbe.maxPowerSpeedForceIntersession;
				//maxAbsoluteForGraph = maxPowerSpeedForceIntersession;
			}
		}

		LogB.Information("maxAbsoluteForCalc = " + maxAbsoluteForCalc.ToString());
		pegbe.feedback.ResetBestSetValue(FeedbackEncoder.BestSetValueEnum.CAPTURE_MAIN_VARIABLE);
		pegbe.feedback.UpdateBestSetValue(
				FeedbackEncoder.BestSetValueEnum.CAPTURE_MAIN_VARIABLE, maxAbsoluteForCalc);


		//to show saved curves on DoPlot
		TreeIter iter;
		bool iterOk = pegbe.encoderCaptureListStore.GetIterFirst(out iter);

		//for eccentricOverload
		eccOverload_l = new List<CairoBarsArrow>();
		double concentricPreValue = -1;

		//discard repetitions according to pegbe.showNRepetitions
		//int countToDraw = pegbe.encoderBarsData_l.Count;
		//foreach(EncoderBarsData ebd in pegbe.encoderBarsData_l)
		//for (int count = 0; count < pegbe.encoderBarsData_l.Count; count ++)
//		int countNames = 0;

		//we used data because this array has only the reps not discarded by showNRepetitions
		for (count = 0; count < data.Count ; count ++)
		{
			double mainVariableValue = Convert.ToDouble(data[count]);

			// 1) get phase (for color)
			Preferences.EncoderPhasesEnum phaseEnum = Preferences.EncoderPhasesEnum.BOTH; // (eccon == "c")
			if (pegbe.eccon == "ec" || pegbe.eccon == "ecS") {
				bool isEven = Util.IsEven(count +1); //TODO: check this (as for is reversed)
				if(isEven)
					phaseEnum = Preferences.EncoderPhasesEnum.CON;
				else
					phaseEnum = Preferences.EncoderPhasesEnum.ECC;
			}

			// 2) manage colors for bars. select pen color for bars and sounds
			string myColor = pegbe.feedback.AssignColorAutomatic(
					FeedbackEncoder.BestSetValueEnum.CAPTURE_MAIN_VARIABLE, mainVariableValue, phaseEnum);

			bool discarded = false;
			if(pegbe.hasInertia) {
				if(pegbe.eccon == "c" && pegbe.discardFirstN > 0 && count < pegbe.discardFirstN)
					discarded = true;
				else if(pegbe.eccon != "c" && pegbe.discardFirstN > 0 && count < pegbe.discardFirstN * 2)
					discarded = true;
			}

			if ( ! discarded && ( myColor == UtilGtk.ColorGood || (pegbe.mainVariableHigher != -1 && mainVariableValue >= pegbe.mainVariableHigher) ) )
			{
				colorPhase = UtilGtk.GetRGBA (UtilGtk.Colors.GREEN_PLOTS);
				//play sound if value is high, volumeOn == true, is last value, capturing
				if (pegbe.volumeOn && count == data.Count -1 && pegbe.capturing && ! UtilList.FoundInListInt (RepetitionsPlayed_l, count))
				{
					Util.PlaySound (Constants.SoundTypes.GOOD, preferences.volumeOn, preferences.gstreamer);
					RepetitionsPlayed_l.Add (count);
				}
			}
			else if ( ! discarded && ( myColor == UtilGtk.ColorBad || (pegbe.mainVariableLower != -1 && mainVariableValue <= pegbe.mainVariableLower) ) )
			{
				colorPhase = UtilGtk.GetRGBA (UtilGtk.Colors.RED_PLOTS);
				//play sound if value is low, volumeOn == true, is last value, capturing
				if (pegbe.volumeOn && count == data.Count -1 && pegbe.capturing && ! UtilList.FoundInListInt (RepetitionsPlayed_l, count))
				{
					Util.PlaySound (Constants.SoundTypes.BAD, pegbe.volumeOn, pegbe.gstreamer);
					RepetitionsPlayed_l.Add (count);
				}
			}
			else if(myColor == UtilGtk.ColorGray)
			{
				/*
				 * on ecS when feedback is only in the opposite phase,
				 * AssignColorAutomatic will return ColorGray
				 * this helps to distinguins the phase that we want
				 */
				colorPhase = UtilGtk.GetRGBA (UtilGtk.Colors.GRAY);
			}
			else
				colorPhase = UtilGtk.GetRGBA (UtilGtk.Colors.BLUE_LIGHT);

			//know if ecc or con to paint with dark or light pen
			if (pegbe.eccon == "ec" || pegbe.eccon == "ecS")
			{
				//bool isEven = Util.IsEven(count +1);

				//on inertial if discardFirstN , they have to be gray
				if( pegbe.hasInertia && pegbe.discardFirstN > 0 &&
						((pegbe.eccon == "c" && count < pegbe.discardFirstN) || (pegbe.eccon != "c" && count < pegbe.discardFirstN * 2)) )
					colorBar = CairoGeneric.colorFromRGBA (UtilGtk.GetRGBA (UtilGtk.Colors.GRAY));
				else {
					colorBar = CairoGeneric.colorFromRGBA (colorPhase);
				}
			} else {
				if( pegbe.hasInertia && pegbe.discardFirstN > 0 &&
						((pegbe.eccon == "c" && count < pegbe.discardFirstN) || (pegbe.eccon != "c" && count < pegbe.discardFirstN * 2)) )
					colorBar = CairoGeneric.colorFromRGBA (UtilGtk.GetRGBA (UtilGtk.Colors.GRAY));
				else
					colorBar = CairoGeneric.colorFromRGBA (colorPhase);
			}

			// 3) add data in barA_l, barB_l, names_l and color lists
			if(pegbe.eccon == "c")
			{
				barA_l.Add(new PointF(count +1, mainVariableValue));
				colorMain_l.Add(colorBar);
				names_l.Add((pegbe.encoderBarsData_l.Count -data.Count +(count+1)).ToString());
			} else
			{
				if(! Util.IsEven(count +1))  	//if it is "impar"
				{
					barA_l.Add(new PointF(UtilAll.DivideSafe(count+1,2), mainVariableValue));
					colorSecondary_l.Add(colorBar);
					names_l.Add((UtilAll.DivideSafe(pegbe.encoderBarsData_l.Count -data.Count +count,2)+1).ToString());
				} else {// "par"
					barB_l.Add(new PointF(UtilAll.DivideSafe(count+1,2), mainVariableValue));
					colorMain_l.Add(colorBar);
				}
			}

			// 4) eccentric overload
			//draw green arrow eccentric overload on inertial only if ecc > con
			if (pegbe.hasInertia && preferences.encoderCaptureInertialEccOverloadMode !=
					Preferences.encoderCaptureEccOverloadModes.NOT_SHOW &&
					(pegbe.eccon == "ec" || pegbe.eccon == "ecS"))
			{
				bool isEven = Util.IsEven(count +1);
				if(isEven)
					concentricPreValue = mainVariableValue;
				else if(concentricPreValue >= 0 && mainVariableValue > concentricPreValue)
					eccOverload_l.Add (new CairoBarsArrow(count-1, concentricPreValue, count, mainVariableValue));
			}

			// 5) create saved list: saved_l and add to sumSaved and countSaved for title generation
			if( iterOk && ((EncoderCurve) pegbe.encoderCaptureListStore.GetValue (iter, 0)).Record )
			{
				if(pegbe.eccon == "c" ||
						preferences.encoderCaptureFeedbackEccon == Preferences.EncoderPhasesEnum.BOTH ||
						preferences.encoderCaptureFeedbackEccon == Preferences.EncoderPhasesEnum.ECC && ! Util.IsEven(count +1) || //odd (impar)
						preferences.encoderCaptureFeedbackEccon == Preferences.EncoderPhasesEnum.CON && Util.IsEven(count +1) ) //even (par)
				{
					sumSaved += mainVariableValue;
					countSaved ++;

					if (mainVariableValue > maxThisSetSaved)
						maxThisSetSaved = mainVariableValue;
				}

				if(pegbe.eccon == "c")
					saved_l.Add(count);
				else if(phaseEnum == Preferences.EncoderPhasesEnum.CON)
					saved_l.Add(Convert.ToInt32(Math.Floor(UtilAll.DivideSafe(count, 2))));
			}

			// 6) work and impulse
			if(dataWorkJ.Count > 0)
			{
				if(preferences.encoderWorkKcal)
					workTotal += Convert.ToDouble(dataWorkJ[count]) * 0.000239006;
				else
					workTotal += Convert.ToDouble(dataWorkJ[count]);
			}

			if(dataImpulse.Count > 0)
				impulseTotal += Convert.ToDouble(dataImpulse[count]);

			iterOk = pegbe.encoderCaptureListStore.IterNext (ref iter);
		}

		//if !c && is "impar" (uneven), add a null to B
		if (pegbe.eccon != "c" && ! Util.IsEven(pegbe.encoderBarsData_l.Count))
		{
			barB_l.Add(null);
			colorMain_l.Add(CairoGeneric.colorFromRGBA (UtilGtk.GetRGBA (UtilGtk.Colors.GRAY))); //this color will not be shown is just to match barB_l with colorMain_l
		}
	}

	private void prepareTitle ()
	{
		units = "";
		int decimals;
		if(pegbe.mainVariable == Constants.MeanSpeed || pegbe.mainVariable == Constants.MaxSpeed) {
			units = "m/s";
			decimals = 2;
		} else if(pegbe.mainVariable == Constants.MeanForce || pegbe.mainVariable == Constants.MaxForce) {
			units = "N";
			decimals = 1;
		}
		else { //powers
			units =  "W";
			decimals = 1;
		}

		//LogB.Information(string.Format("sumValid: {0}, countValid: {1}, div: {2}", sumValid, countValid, sumValid / countValid));
		//LogB.Information(string.Format("sumSaved: {0}, countSaved: {1}, div: {2}", sumSaved, countSaved, sumSaved / countSaved));

		//add avg and avg of saved values
		titleStr = Catalog.GetString (pegbe.mainVariable) + " [X: " +
			Util.TrimDecimals( (sumValid / countValid), decimals) +
			" " + units + "; ";

		if(countSaved > 0)
			titleStr += "X" + Catalog.GetString("saved") + ": " +
				Util.TrimDecimals( (sumSaved / countSaved), decimals) +
				" " + units;

		lossStr = "";

		//do not show lossStr on Preferences.EncoderPhasesEnum.ECC
		if( pegbe.showLoss && (pegbe.eccon == "c" || preferences.encoderCaptureFeedbackEccon != Preferences.EncoderPhasesEnum.ECC) )
		{
			titleStr += "; ";
			lossStr = "Loss: ";
			if(pegbe.eccon != "c")
				lossStr = "Loss (con): "; //on ecc/con use only con for loss calculation

			if(maxThisSetValidAndCon > 0)
			{
				lossStr += Util.TrimDecimals(
						100.0 * (maxThisSetValidAndCon - minThisSetValidAndCon) / maxThisSetValidAndCon, decimals) + "%";
				//LogB.Information(string.Format("Loss at plot: {0}", 100.0 * (maxThisSetValidAndCon - minThisSetValidAndCon) / maxThisSetValidAndCon));
			}
		}

		//work and impulse are in separate string variables because maybe we will select to show one or the other
		//work
		workStr = "]    " + Catalog.GetString("Work") + ": " + Util.TrimDecimals(workTotal, decimals);
		if(preferences.encoderWorkKcal)
			workStr += " Kcal";
		else
			workStr += " J";

		//impulse
		impulseStr = "    " + Catalog.GetString("Impulse") + ": " + Util.TrimDecimals(impulseTotal, decimals) + " N*s";
	}

	private void prepareLossArrow ()
	{
		cairoBarsArrow = null;
		if(pegbe.showLoss && (pegbe.eccon == "c" || preferences.encoderCaptureFeedbackEccon != Preferences.EncoderPhasesEnum.ECC) )
		{
			if(maxThisSetValidAndCon > 0 && maxThisSetValidAndConPos < minThisSetValidAndConPos)
				cairoBarsArrow = new CairoBarsArrow(maxThisSetValidAndConPos, maxThisSetValidAndCon,
						minThisSetValidAndConPos, minThisSetValidAndCon);
		}
	}

	private void paintSpecificDo ()
	{
		if(pegbe.eccon == "c")
			cb = new CairoBars1Series (darea, CairoBars.Type.ENCODER, ! pegbe.capturing, false, false);
		else
			cb = new CairoBarsNHSeries (darea, CairoBars.Type.ENCODER, false, ! pegbe.capturing, false, false);

		//LogB.Information("data_l.Count: " + data_l.Count.ToString());
		//cb.GraphInit(fontStr, true, false); //usePersonGuides, useGroupGuides
		cb.GraphInit(fontStr, false, false); //usePersonGuides, useGroupGuides

		int decs;
		if(pegbe.mainVariable == Constants.MeanSpeed || pegbe.mainVariable == Constants.MaxSpeed)
			decs = 2;
		else if(pegbe.mainVariable == Constants.MeanForce || pegbe.mainVariable == Constants.MaxForce)
			decs = 0;
		else //powers
			decs = 0;
		cb.Decs = decs;

		if(cairoBarsArrow != null)
			cb.PassArrowData (cairoBarsArrow);

		if(lineData_l.Count > 0)
		{
			if (! preferences.encoderCaptureSecondaryVariableYAxisCustom)
				cb.Cbsld = new CairoBarsSecondaryLineData (
						lineData_l,
						-1,
						-1,
						pegbe.secondaryVariable);
			else
				cb.Cbsld = new CairoBarsSecondaryLineData (
						lineData_l,
						preferences.encoderCaptureSecondaryVariableYAxisCustomMax,
						preferences.encoderCaptureSecondaryVariableYAxisCustomMin,
						pegbe.secondaryVariable);
		}

		if(eccOverload_l != null && eccOverload_l.Count > 0)
		{
			cb.EccOverload_l = eccOverload_l;
			if (preferences.encoderCaptureInertialEccOverloadMode ==
					Preferences.encoderCaptureEccOverloadModes.SHOW_LINE_AND_PERCENT)
				cb.EccOverloadWriteValue = true;
		}

		if(saved_l.Count > 0)
			cb.Saved_l = saved_l;

		if(! pegbe.relativeToSet)
		{
			if (maxThisSetSaved >= pegbe.maxPowerSpeedForceIntersession)
			{
				cb.MaxIntersession = maxThisSetSaved;
				cb.MaxIntersessionValueStr = "";//Util.TrimDecimals (maxThisSetSaved, decs) + " " + units;
				cb.MaxIntersessionDate = "";
			} else {
				cb.MaxIntersession = pegbe.maxPowerSpeedForceIntersession;
				cb.MaxIntersessionValueStr = Util.TrimDecimals(pegbe.maxPowerSpeedForceIntersession, decs) + " " + units;
				cb.MaxIntersessionDate = pegbe.maxPowerSpeedForceIntersessionDate;
			}
			cb.MaxIntersessionEcconCriteria = preferences.GetEncoderRepetitionCriteria (pegbe.hasInertia);
		}

		//this should be passed before PassData1Serie && PassData2Series
		cb.SetEncoderTitle (titleStr, lossStr, workStr, impulseStr);

		if(pegbe.eccon == "c")
			cb.PassData1Serie (barA_l,
					colorMain_l, names_l,
					preferences.encoderCaptureBarplotFontSize, 14, 8, "",
					new List<int> (), new List<int> ());
		else {
			List<List<PointF>> barsSecondary_ll = new List<List<PointF>>();
			barsSecondary_ll.Add(barA_l);

			cb.PassData2Series (barB_l, barsSecondary_ll, false,
					colorMain_l, colorSecondary_l, names_l,
					"Ecc",// "Con",
					false,
					preferences.encoderCaptureBarplotFontSize, 14, 8, "",
					new List<int> (), new List<int> ());
		}

		if (videoTime > 0)
		{
			cb.VideoPlayTimeInSeconds = videoTime;
			cb.VideoPlayTimes_l = dataStart_l;

			//TODO: used dataStart_l and dataDuration_l
		}

		passDataForScreenshotIfNeeded ();

		cb.GraphDo();
	}
}

public class CairoPaintBarsPreEncoderSession : CairoPaintBarsPre
{
	private bool showPersonName;

	public CairoPaintBarsPreEncoderSession (DrawingArea darea, string fontStr, Constants.Modes mode, string personName, string testName, int pDN, bool showPersonName)
	{
		initialize (darea, fontStr, mode, personName, testName, pDN);

		this.title = generateTitle();
		this.showPersonName = showPersonName;
	}

	public override void StoreEventGraphEncoderSession (PrepareEventGraphEncoderSession eventGraph)
	{
		this.eventGraphEncoderSessionStored = eventGraph;
	}

	protected override bool storeCreated ()
	{
		return (eventGraphEncoderSessionStored != null);
	}

	protected override bool haveDataToPlot()
	{
		return (eventGraphEncoderSessionStored.rowsAtSQL.Count > 0);
	}

	protected override void paintSpecific()
	{
		cb = new CairoBars1Series (darea, CairoBars.Type.NORMAL, true, true, true);

		cb.YVariable = Catalog.GetString("Mean Power");
		cb.YUnits = "W";

		//cb.GraphInit(fontStr, ! ShowPersonNames, true); //usePersonGuides, useGroupGuides
		cb.GraphInit(fontStr, true, true); //usePersonGuides, useGroupGuides

		List<Event> events = EncoderSQL.EncoderSQLListToEventList (eventGraphEncoderSessionStored.rowsAtSQL);

		List<PointF> point_l = new List<PointF>();
		List<string> names_l = new List<string>();
		List<int> id_l = new List<int>(); //the uniqueIDs for knowing them on bar selection

		calculateBottomParams (events, true, "", "", false, eventGraphEncoderSessionStored.exerciseAll);

		int countToDraw = eventGraphEncoderSessionStored.rowsAtSQL.Count;
		foreach (EncoderSQL eSQL in eventGraphEncoderSessionStored.rowsAtSQL)
		{
			// 1) Add data
			//point_l.Add(new PointF(countToDraw --, UtilAll.DivideSafe (fp.TotalMs, 1000)));
			point_l.Add (new PointF(countToDraw --, eSQL.meanPowerD)); //TODO: or meanSpeed

			// 2) Add bottom names
			string typeRowString = "";
			if (eventGraphEncoderSessionStored.exerciseAll) //if "all tests" show type
				typeRowString = eSQL.exerciseName;// + "\n" + string.Format ("{0} Kg", eSQL.extraWeight);
			//if (eventGraphEncoderSessionStored.type == "")
			//	typeRowString = jump.Type;

			names_l.Add (createTextBelowBar(
						string.Format ("{0} Kg", Util.TrimDecimals (eSQL.extraWeightD, 2)),
						typeRowString,
						eSQL.Description, //person name
						false, false,
						longestWord.Length, maxRowsForText));

			id_l.Add (eSQL.UniqueID);

			//if (eventGraphEncoderSessionStored.selectedID == eSQL.UniqueID)
			//	cb.SelectedPos = eventGraphEncoderSessionStored.rowsAtSQL.Count -countToDraw -1;
			if (UtilList.FoundInListInt (eventGraphEncoderSessionStored.selectedRepID_l, eSQL.UniqueID))
				cb.SelectedPos_l.Add (eventGraphEncoderSessionStored.rowsAtSQL.Count -countToDraw -1);
		}
		cb.Id_l = id_l;

		cb.PassData1Serie (point_l,
				new List<Cairo.Color>(), names_l,
				-1, fontHeightForBottomNames, bottomMargin, title,
				new List<int> (), new List<int> ());

		passDataForScreenshotIfNeeded ();

		cb.GraphDo();
	}
}
