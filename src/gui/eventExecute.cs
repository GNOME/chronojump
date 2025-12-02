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

	Gtk.Box box_resultsSession_jumpVar;
	Gtk.Box box_resultsSession_runVar;
	Gtk.Box box_resultsSession_raVar;
	Gtk.Box box_resultsSession_forceVar;
	Gtk.Label label_resultsSession_encoder_saved_repetitions;
	Gtk.RadioButton radio_runI_realtime_speeds;
	Gtk.RadioButton radio_runI_realtime_times;
	Gtk.Image image_runI_realtime_speeds;
	Gtk.Image image_runI_realtime_times;
	Gtk.RadioButton radio_resultsSession_jump_heights;
	Gtk.RadioButton radio_resultsSession_jump_times;
	Gtk.Image image_resultsSession_jump_heights;
	Gtk.Image image_resultsSession_jump_times;
	Gtk.RadioButton radio_resultsSession_run_speeds;
	Gtk.RadioButton radio_resultsSession_run_times;
	Gtk.Image image_resultsSession_run_speeds;
	Gtk.Image image_resultsSession_run_times;
	Gtk.RadioButton radio_resultsSession_ra_speeds;
	Gtk.RadioButton radio_resultsSession_ra_best_second;
	Gtk.Image image_resultsSession_ra_speeds;
	Gtk.Image image_resultsSession_ra_best_second;
	Gtk.RadioButton radio_resultsSession_force_max;
	Gtk.RadioButton radio_resultsSession_force_best_second;
	Gtk.Image image_resultsSession_force_max;
	Gtk.Image image_resultsSession_force_best_second;
	Gtk.RadioButton radio_resultsSession_bars;
	Gtk.RadioButton radio_resultsSession_points;
	Gtk.Image image_resultsSession_bars;
	Gtk.Image image_resultsSession_points;
	Gtk.Box box_radio_resultsSession_bestLast;
	Gtk.Label label_resultsSession_last;
	Gtk.RadioButton radio_resultsSession_best;
	Gtk.RadioButton radio_resultsSession_best2;
	Gtk.RadioButton radio_resultsSession_last;
	Gtk.Box box_resultsSession_limit;
	Gtk.Image image_resultsSession_limit;
	Gtk.SpinButton spin_resultsSession_limit;
	Gtk.Box box_resultsSession_bestLast;
	Gtk.Box box_contacts_graph_exercise;
	Gtk.Box box_contacts_graph_show_graph_table;
	Gtk.RadioButton radio_contacts_graph_currentTest;
	Gtk.RadioButton radio_contacts_graph_allTests;
	//Gtk.RadioButton radio_contacts_results_personCurrent;
	Gtk.RadioButton radio_contacts_results_personAll;
	Gtk.Image image_radio_contacts_results_personCurrent;
	Gtk.Image image_radio_contacts_results_personAll;
	
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
		box_resultsSession_bestLast.Visible = true;

//		align_check_vbox_contacts_graph_legend.Visible = true;
		//vbox_contacts_graph_legend.Visible = false;

		notebook_results_data.Visible = false;
	}
	
	
	private void showJumpReactiveLabels() 
	{
		box_resultsSession_bestLast.Visible = true;

//		align_check_vbox_contacts_graph_legend.Visible = false;
//		vbox_contacts_graph_legend.Visible = false;

		notebook_results_data.Visible = false;
	}
	
	private void showRunSimpleLabels() 
	{
		box_resultsSession_bestLast.Visible = true;

//		align_check_vbox_contacts_graph_legend.Visible = true;
		//vbox_contacts_graph_legend.Visible = false;

		notebook_results_data.Visible = false;
	}
		
	private void showRunIntervalLabels() 
	{
		box_resultsSession_bestLast.Visible = true;

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
						true, CairoXY.PlotTypes.POINTSFILL,
						Config.ColorBackgroundShifted, Config.ColorBackground
						);
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

		Gdk.EventButton eventButton = args.Event;

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

		// show edit, repair?, delete
		if (eventButton.Button == 3)
			treeviewResultsContextMenu (
					(current_mode == Constants.Modes.JUMPSREACTIVE || current_mode == Constants.Modes.RUNSINTERVALLIC), //hasRepair
					"");
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

		// B) Paint cairo graph
		cairoPaintBarsPre.ShowPersonNames = radio_contacts_results_personAll.Active;
		cairoPaintBarsPre.UseHeights = eventGraph.showHeights;

		cairoPaintBarsPre.Paint();
	}

	private string getCurrentTestTypeForThisMode ()
	{
		if (current_mode == Constants.Modes.UNDEFINED)
			return "";

		switch (current_mode)
		{
			case Constants.Modes.JUMPSSIMPLE :
				if (currentJumpType != null)
					return currentJumpType.Name;
				else
					return "";
			case Constants.Modes.JUMPSREACTIVE :
				if (currentJumpRjType != null)
					return currentJumpRjType.Name;
				else
					return "";
			case Constants.Modes.RUNSSIMPLE :
				if (currentRunType != null)
					return currentRunType.Name;
				else
					return "";
			case Constants.Modes.RUNSINTERVALLIC :
				if (currentJumpType != null)
					return currentRunIntervalType.Name;
				else
					return "";
			case Constants.Modes.OTHER : // fourPlatforms
				return fourPlatformsCaptureType.ToString ();
		}

		return "";
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

	private void on_radio_runI_realtime_speeds_times_toggled (object o, EventArgs args)
	{
		// 2) redo graph
		drawingarea_results_realtime.QueueDraw ();
	}

	private void on_check_runI_realtime_rel_abs_toggled (object o, EventArgs args)
	{
		// 1) change icon
		if(check_runI_realtime_rel_abs.Active)
			image_check_runI_realtime_rel_abs.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "bar_relative.png");
		else
			image_check_runI_realtime_rel_abs.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "bar_absolute.png");

		// 2) redo graph
		drawingarea_results_realtime.QueueDraw ();
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
		cairoPaintBarsPre.RunsTimes = radio_resultsSession_run_times.Active;
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
		cairoPaintBarsPre.RunsTimes = radio_resultsSession_run_times.Active;
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
				radio_runI_realtime_times.Active, preferences.metersSecondsPreferred,
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

	// only jumpRj
	private void on_radio_resultsSession_jumpVar_toggled (object o, EventArgs args)
	{
		// change on preferences object and DB
		preferences.heightPreferred = Preferences.PreferencesChange (
				false, "heightPreferred",
				preferences.heightPreferred,
				radio_resultsSession_jump_heights.Active);

		resultsSession_bestLast_controls ();
		updateGraphResultsSessionByMode ();

		if (current_mode == Constants.Modes.JUMPSSIMPLE)
			((TreeViewJumps) treeViewResultsSession).BarsAreDistance = radio_resultsSession_jump_heights.Active;
		else if (current_mode == Constants.Modes.JUMPSREACTIVE)
			((TreeViewJumpsRj) treeViewResultsSession).BarsAreDistance = radio_resultsSession_jump_heights.Active;
		treeViewResultsSession.ResultsInBarsRowChanged ();
	}
	private void on_radio_resultsSession_runVar_toggled (object o, EventArgs args)
	{
		resultsSession_bestLast_controls ();
		updateGraphResultsSessionByMode ();

		if (current_mode == Constants.Modes.RUNSSIMPLE)
			((TreeViewRuns) treeViewResultsSession).BarsAreSpeeds = radio_resultsSession_run_speeds.Active;
		else if (current_mode == Constants.Modes.RUNSINTERVALLIC)
			((TreeViewRunsInterval) treeViewResultsSession).BarsAreSpeeds = radio_resultsSession_run_speeds.Active;
		treeViewResultsSession.ResultsInBarsRowChanged ();
	}
	private void on_radio_resultsSession_raVar_toggled (object o, EventArgs args)
	{
		resultsSession_bestLast_controls ();
		updateGraphResultsSessionByMode ();

		((TreeViewRunEncoder) treeViewResultsSession).BarsAreSpeeds = radio_resultsSession_ra_speeds.Active;
		treeViewResultsSession.ResultsInBarsRowChanged ();
	}
	private void on_radio_resultsSession_forceVar_toggled (object o, EventArgs args)
	{
		resultsSession_bestLast_controls ();
		updateGraphResultsSessionByMode ();
	}

	private void on_radio_resultsSession_bestLast_toggled (object o, EventArgs args)
	{
		updateGraphResultsSessionByMode ();
	}

	private void resultsSession_bestLast_controls ()
	{
		Constants.Modes m = current_mode;
		
		if (m == Constants.Modes.BEEPTEST || m == Constants.Modes.WILIGHT ||
				m == Constants.Modes.OTHER) // OTHER is FOURPLATFORMS
		{	
			box_radio_resultsSession_bestLast.Visible = false;
			label_resultsSession_last.Visible = true;
			return;
		}

		box_radio_resultsSession_bestLast.Visible = true;
		label_resultsSession_last.Visible = false;

		if (m == Constants.Modes.JUMPSSIMPLE || m == Constants.Modes.JUMPSREACTIVE)
			resultsSession_bestLast_controls_jumps (m);
		else if (m == Constants.Modes.RUNSSIMPLE || m == Constants.Modes.RUNSINTERVALLIC || m == Constants.Modes.RUNSENCODER)
			resultsSession_bestLast_controls_races (m);
		else if (Constants.ModeIsFORCESENSOR (m))
			resultsSession_bestLast_controls_forceSensor (m);
		else if (Constants.ModeIsENCODER (m))
			resultsSession_bestLast_controls_encoder (m);

		if (radio_resultsSession_best2.Active && ! radio_resultsSession_best2.Visible)
			radio_resultsSession_best.Active = true;

		UtilGtk.ContrastLabelsBox (Config.ColorBackgroundShiftedIsDark, box_radio_resultsSession_bestLast);
	}

	private void resultsSession_bestLast_controls_jumps (Constants.Modes m)
	{
		if ( m == Constants.Modes.JUMPSSIMPLE ||
				(m == Constants.Modes.JUMPSREACTIVE && radio_resultsSession_jump_heights.Active))
		{
			if (radio_resultsSession_jump_heights.Active)
				radio_resultsSession_best.Label = Catalog.GetString ("Jump height");
			else
				radio_resultsSession_best.Label = Catalog.GetString ("Flight time");

			radio_resultsSession_best2.Visible = false;
		}
		else // (m == Constants.Modes.JUMPSREACTIVE && radio_resultsSession_times.Active)
		{
			radio_resultsSession_best.Label = Catalog.GetString ("Flight time");
			radio_resultsSession_best2.Label = "FT/CT";
			radio_resultsSession_best2.Visible = true;
		}
	}

	private void resultsSession_bestLast_controls_races (Constants.Modes m)
	{
		if (m == Constants.Modes.RUNSSIMPLE || m == Constants.Modes.RUNSINTERVALLIC)
		{
			if (radio_resultsSession_run_speeds.Active)
				radio_resultsSession_best.Label = Catalog.GetString ("Speed");
			else
				radio_resultsSession_best.Label = Catalog.GetString ("Time");

			radio_resultsSession_best2.Visible = false;
		} else { 	// m == Constants.Modes.RUNSENCODER
			if (radio_resultsSession_ra_speeds.Active)
				radio_resultsSession_best.Label = Catalog.GetString ("Max speed");
			else
				radio_resultsSession_best.Label = Catalog.GetString ("Best second");

			radio_resultsSession_best2.Visible = false;
		}
	}

	private void resultsSession_bestLast_controls_forceSensor (Constants.Modes m)
	{
		if (radio_resultsSession_force_max.Active)
			radio_resultsSession_best.Label = Catalog.GetString ("Max force");
		else
			radio_resultsSession_best.Label = Catalog.GetString ("Best second");

		radio_resultsSession_best2.Visible = false;
	}

	private void resultsSession_bestLast_controls_encoder (Constants.Modes m)
	{
		radio_resultsSession_best.Label = Catalog.GetString (preferences.encoderCaptureMainVariable.ToString ());
		if (m == Constants.Modes.POWERGRAVITATORY)
		{
			radio_resultsSession_best2.Label = Catalog.GetString ("Extra weight");
			radio_resultsSession_best2.Visible = true;
		} else { // (m == Constants.Modes.POWERINERTIAL)
			radio_resultsSession_best2.Visible = false;
		}
	}


	// used on forceSensor
	private Constants.ResultsSessionCriteria get_radio_resultsSession_criteria ()
	{
		if (radio_resultsSession_last.Active)
			return Constants.ResultsSessionCriteria.LAST;

		if (current_mode == Constants.Modes.JUMPSREACTIVE && radio_resultsSession_jump_heights.Active)
			return Constants.ResultsSessionCriteria.BEST3;

		if (radio_resultsSession_best.Active)
			return Constants.ResultsSessionCriteria.BEST;

		// if (radio_resultsSession_best2.Active)
		return Constants.ResultsSessionCriteria.BEST2;
	}

	private void on_spin_resultsSession_limit_value_changed (object o, EventArgs args)
	{
		updateGraphResultsSessionByMode ();
	}

	private void on_radio_resultsSession_bars_points_toggled (object o, EventArgs args)
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

					if (dialogResult != null && dialogResult.Visible)
					{
						if (currentJumpRjType.JumpsLimited)
							dialogResult.UpdateLabelResult (currentEventExecute.GetDialogResultString ());
					}
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

					if (dialogResult != null && dialogResult.Visible)
					{
						if (currentRunIntervalType.TracksLimited)
							dialogResult.UpdateLabelResult (currentEventExecute.GetDialogResultString ());
					}
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

		box_resultsSession_jumpVar = (Gtk.Box) builder.GetObject ("box_resultsSession_jumpVar");
		box_resultsSession_runVar = (Gtk.Box) builder.GetObject ("box_resultsSession_runVar");
		box_resultsSession_raVar = (Gtk.Box) builder.GetObject ("box_resultsSession_raVar");
		box_resultsSession_forceVar = (Gtk.Box) builder.GetObject ("box_resultsSession_forceVar");
		label_resultsSession_encoder_saved_repetitions = (Gtk.Label) builder.GetObject ("label_resultsSession_encoder_saved_repetitions");
		radio_runI_realtime_speeds = (Gtk.RadioButton) builder.GetObject ("radio_runI_realtime_speeds");
		radio_runI_realtime_times = (Gtk.RadioButton) builder.GetObject ("radio_runI_realtime_times");
		image_runI_realtime_speeds = (Gtk.Image) builder.GetObject ("image_runI_realtime_speeds");
		image_runI_realtime_times = (Gtk.Image) builder.GetObject ("image_runI_realtime_times");
		radio_resultsSession_jump_heights = (Gtk.RadioButton) builder.GetObject ("radio_resultsSession_jump_heights");
		radio_resultsSession_jump_times = (Gtk.RadioButton) builder.GetObject ("radio_resultsSession_jump_times");
		image_resultsSession_jump_heights = (Gtk.Image) builder.GetObject ("image_resultsSession_jump_heights");
		image_resultsSession_jump_times = (Gtk.Image) builder.GetObject ("image_resultsSession_jump_times");
		radio_resultsSession_run_speeds = (Gtk.RadioButton) builder.GetObject ("radio_resultsSession_run_speeds");
		radio_resultsSession_run_times = (Gtk.RadioButton) builder.GetObject ("radio_resultsSession_run_times");
		image_resultsSession_run_speeds = (Gtk.Image) builder.GetObject ("image_resultsSession_run_speeds");
		image_resultsSession_run_times = (Gtk.Image) builder.GetObject ("image_resultsSession_run_times");
		radio_resultsSession_ra_speeds = (Gtk.RadioButton) builder.GetObject ("radio_resultsSession_ra_speeds");
		radio_resultsSession_ra_best_second = (Gtk.RadioButton) builder.GetObject ("radio_resultsSession_ra_best_second");
		image_resultsSession_ra_speeds = (Gtk.Image) builder.GetObject ("image_resultsSession_ra_speeds");
		image_resultsSession_ra_best_second = (Gtk.Image) builder.GetObject ("image_resultsSession_ra_best_second");
		radio_resultsSession_force_max = (Gtk.RadioButton) builder.GetObject ("radio_resultsSession_force_max");
		radio_resultsSession_force_best_second = (Gtk.RadioButton) builder.GetObject ("radio_resultsSession_force_best_second");
		image_resultsSession_force_max = (Gtk.Image) builder.GetObject ("image_resultsSession_force_max");
		image_resultsSession_force_best_second = (Gtk.Image) builder.GetObject ("image_resultsSession_force_best_second");
		radio_resultsSession_bars = (Gtk.RadioButton) builder.GetObject ("radio_resultsSession_bars");
		radio_resultsSession_points = (Gtk.RadioButton) builder.GetObject ("radio_resultsSession_points");
		image_resultsSession_bars = (Gtk.Image) builder.GetObject ("image_resultsSession_bars");
		image_resultsSession_points = (Gtk.Image) builder.GetObject ("image_resultsSession_points");
		box_radio_resultsSession_bestLast = (Gtk.Box) builder.GetObject ("box_radio_resultsSession_bestLast");
		label_resultsSession_last = (Gtk.Label) builder.GetObject ("label_resultsSession_last");
		radio_resultsSession_best = (Gtk.RadioButton) builder.GetObject ("radio_resultsSession_best");
		radio_resultsSession_best2 = (Gtk.RadioButton) builder.GetObject ("radio_resultsSession_best2");
		radio_resultsSession_last = (Gtk.RadioButton) builder.GetObject ("radio_resultsSession_last");
		box_resultsSession_limit = (Gtk.Box) builder.GetObject ("box_resultsSession_limit");
		image_resultsSession_limit = (Gtk.Image) builder.GetObject ("image_resultsSession_limit");
		spin_resultsSession_limit = (Gtk.SpinButton) builder.GetObject ("spin_resultsSession_limit");
		box_contacts_graph_exercise = (Gtk.Box) builder.GetObject ("box_contacts_graph_exercise");
		box_contacts_graph_show_graph_table = (Gtk.Box) builder.GetObject ("box_contacts_graph_show_graph_table");
		box_resultsSession_bestLast = (Gtk.Box) builder.GetObject ("box_resultsSession_bestLast");
		radio_contacts_graph_currentTest = (Gtk.RadioButton) builder.GetObject ("radio_contacts_graph_currentTest");
		radio_contacts_graph_allTests = (Gtk.RadioButton) builder.GetObject ("radio_contacts_graph_allTests");
		//radio_contacts_results_personCurrent = (Gtk.RadioButton) builder.GetObject ("radio_contacts_results_personCurrent");
		radio_contacts_results_personAll = (Gtk.RadioButton) builder.GetObject ("radio_contacts_results_personAll");
		image_radio_contacts_results_personCurrent = (Gtk.Image) builder.GetObject ("image_radio_contacts_results_personCurrent");
		image_radio_contacts_results_personAll = (Gtk.Image) builder.GetObject ("image_radio_contacts_results_personAll");

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
