/*
curses trams graph de sessió podria ser 2H (totaltime, maxSpeed)
*/


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
 * Copyright (C) 2004-2021   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Gtk;
using Glade;
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
	
//	[Widget] Gtk.Label event_execute_label_person;
//	[Widget] Gtk.Label event_execute_label_event_type;
	[Widget] Gtk.Label event_execute_label_phases_name;
	[Widget] Gtk.Label event_execute_label_message;
	[Widget] Gtk.Label event_graph_label_graph_test;


	[Widget] Gtk.SpinButton spin_contacts_graph_last_limit;
	[Widget] Gtk.VBox vbox_contacts_simple_graph_controls;
	[Widget] Gtk.RadioButton radio_contacts_graph_currentTest;
	[Widget] Gtk.RadioButton radio_contacts_graph_allTests;
	[Widget] Gtk.RadioButton radio_contacts_graph_currentPerson;
	[Widget] Gtk.RadioButton radio_contacts_graph_allPersons;
	[Widget] Gtk.Image image_radio_contacts_graph_currentPerson;
	[Widget] Gtk.Image image_radio_contacts_graph_allPersons;
	[Widget] Gtk.CheckButton check_run_show_time;
	
	[Widget] Gtk.ProgressBar event_execute_progressbar_event;
	[Widget] Gtk.ProgressBar event_execute_progressbar_time;
	

	//currently gtk-sharp cannot display a label in a progressBar in activity mode (Pulse() not Fraction)
	//then we show the value in a label:
	[Widget] Gtk.Label event_execute_label_event_value;
	[Widget] Gtk.Label event_execute_label_time_value;
	
	[Widget] Gtk.Button event_execute_button_cancel;
	[Widget] Gtk.Button event_execute_button_finish;

	[Widget] Gtk.Table event_execute_table_pulse;
	[Widget] Gtk.Table event_execute_table_pulse_values;
	
//	[Widget] Gtk.Alignment align_check_vbox_contacts_graph_legend;
//	[Widget] Gtk.CheckButton check_vbox_contacts_graph_legend;
//	[Widget] Gtk.VBox vbox_contacts_graph_legend;

	//for the color change in the background of the cell label
	[Widget] Gtk.EventBox event_execute_eventbox_pulse_time;
	[Widget] Gtk.Label event_execute_label_pulse_now;
	[Widget] Gtk.Label event_execute_label_pulse_avg;

	[Widget] Gtk.Notebook notebook_results_data;

	[Widget] Gtk.DrawingArea event_execute_drawingarea;
	[Widget] Gtk.HBox hbox_drawingarea_realtime_capture_cairo;
	[Widget] Gtk.DrawingArea event_execute_drawingarea_realtime_capture_cairo;
	[Widget] Gtk.DrawingArea event_execute_drawingarea_cairo;
	[Widget] Gtk.VBox vbox_event_execute_drawingarea_run_interval_realtime_capture_cairo;
	[Widget] Gtk.CheckButton check_runI_realtime_rel_abs;
	[Widget] Gtk.Image image_check_runI_realtime_rel_abs;
	[Widget] Gtk.DrawingArea event_execute_drawingarea_run_simple_double_contacts;
	[Widget] Gtk.Label label_run_simple_double_contacts;
	/*
	[Widget] Gtk.Box hbox_combo_graph_results_width;
	[Widget] Gtk.Box hbox_combo_graph_results_height;
	[Widget] Gtk.ComboBox combo_graph_results_width;
	[Widget] Gtk.ComboBox combo_graph_results_height;
	*/

	//[Widget] Gtk.Alignment event_execute_alignment_drawingarea;
	//static Gdk.Pixmap event_execute_pixmap = null;
	Gdk.Pixmap event_execute_pixmap = null;
	Gdk.Pixmap event_execute_run_simple_double_contacts_pixmap = null;
	

	string event_execute_label_simulated;
	//int sessionID;
	//string event_execute_personName;	
	string event_execute_tableName;	
	string event_execute_eventType;	
	
	//double event_execute_limit;
	
	private enum phasesGraph {
		UNSTARTED, DOING, DONE
	}
	
	int event_execute_rightMargin = 35; 	//at the right we write text (on windows we change later)

	/*
	 * when click on destroy window, delete event is raised
	 * if event has ended, then it should normally close the window
	 * if has not ended, then it should cancel it before.
	 * on 0.7.5.2 and before, we always cancel, 
	 * and this produces and endless loop when event has ended, because there's nothing to cancel
	 */

	//ExecutingGraphData executingGraphData;

	
	//for writing text
	Pango.Layout layoutSmall;
	Pango.Layout layoutSmallMid;
	Pango.Layout layoutMid;
	Pango.Layout layoutBig;
	Pango.Layout layoutMid_run_simple;

	static EventGraphConfigureWindow eventGraphConfigureWin;
	
	/* Used only by commented code
	   private static string [] comboGraphResultsSize = {
		"100", "200", "300", "400", "500"
	};
	*/

	//Cairo stuff (migrating to GTK3)
	//TODO: make all inherit from a generic: PrepareEventGraphContacts
	//PrepareEventGraphJumpSimple eventGraphJumpsCairoStored;
	//PrepareEventGraphRunSimple eventGraphRunsCairoStored;
	//string cairoTitleStored;

	//we need both working to be able to correctly expose_event on jumpRj, runI
	CairoPaintBarsPre cairoPaintBarsPre;
	CairoPaintBarsPre cairoPaintBarsPreRealTime;
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

		if(UtilAll.IsWindows()) {
			event_execute_rightMargin = 55;
		}

		event_execute_configureColors();
	
		event_execute_label_simulated = "";
		if(simulated) 
			event_execute_label_simulated = "(" + Catalog.GetString("Simulated") + ")";

		event_graph_label_graph_test.Text = "<b>" + event_execute_eventType + "</b>";
		event_graph_label_graph_test.UseMarkup = true;
		event_execute_label_message.Text = "";

		//this.event_execute_personName.Text = event_execute_personName; 	//"Jumps" (rjInterval), "Runs" (runInterval), "Ticks" (pulses), 
		this.event_execute_label_phases_name.Text = phasesName; 	//"Jumps" (rjInterval), "Runs" (runInterval), "Ticks" (pulses), 
								//"Phases" (simple jumps, dj, simple runs)
		this.event_execute_tableName = tableName;

		this.event_execute_eventType = event_execute_eventType;

		//finish not sensitive for all events. 
		//Later reactive, interval and pulse will sensitive it when a subevent is done
		event_execute_button_finish.Sensitive = false;

		if(event_execute_tableName == Constants.JumpTable) {
			showJumpSimpleLabels();
		} else if(event_execute_tableName == Constants.JumpRjTable) {
			showJumpReactiveLabels();
		} else if(event_execute_tableName == Constants.RunTable) {
			showRunSimpleLabels();
		} else if(event_execute_tableName == Constants.RunIntervalTable) {
			showRunIntervalLabels();
		} else if(event_execute_tableName == Constants.ReactionTimeTable) {
			showReactionTimeLabels(); 
		} else if(event_execute_tableName == Constants.PulseTable) {
			showPulseLabels();
		}

		UtilGtk.ClearDrawingArea(event_execute_drawingarea, event_execute_pixmap);

		clearProgressBars();
	
		event_execute_eventbox_pulse_time.ModifyBg(Gtk.StateType.Normal, UtilGtk.BLUE_PLOTS); //only one serie in pulse, leave blue
		
		layoutSmall = new Pango.Layout (event_execute_drawingarea.PangoContext);
		layoutSmall.FontDescription = Pango.FontDescription.FromString (preferences.GetFontTypeWithSize(7));
		
		layoutSmallMid = new Pango.Layout (event_execute_drawingarea.PangoContext);
		layoutSmallMid.FontDescription = Pango.FontDescription.FromString (preferences.GetFontTypeWithSize(9));

		layoutMid = new Pango.Layout (event_execute_drawingarea.PangoContext);
		layoutMid.FontDescription = Pango.FontDescription.FromString (preferences.GetFontTypeWithSize(11));

		layoutBig = new Pango.Layout (event_execute_drawingarea.PangoContext);
		layoutBig.FontDescription = Pango.FontDescription.FromString (preferences.GetFontTypeWithSize(14));
		//layoutBig.Alignment = Pango.Alignment.Center; //doesn't work, see GetPixelSize below
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
		event_execute_table_pulse.Hide();
		event_execute_table_pulse_values.Hide();
	}
	
	private void showJumpSimpleLabels() 
	{
		event_graph_label_graph_test.Visible = false;
		vbox_contacts_simple_graph_controls.Visible = true;
		check_run_show_time.Visible = false;

//		align_check_vbox_contacts_graph_legend.Visible = true;
		//vbox_contacts_graph_legend.Visible = false;

		notebook_results_data.Visible = false;
	}
	
	
	private void showJumpReactiveLabels() 
	{
		event_graph_label_graph_test.Visible = false;
		vbox_contacts_simple_graph_controls.Visible = true;
		check_run_show_time.Visible = false;

//		align_check_vbox_contacts_graph_legend.Visible = false;
//		vbox_contacts_graph_legend.Visible = false;

		notebook_results_data.Visible = false;
	}
	
	private void showRunSimpleLabels() 
	{
		event_graph_label_graph_test.Visible = false;
		vbox_contacts_simple_graph_controls.Visible = true;
		check_run_show_time.Visible = true;

//		align_check_vbox_contacts_graph_legend.Visible = true;
		//vbox_contacts_graph_legend.Visible = false;

		notebook_results_data.Visible = false;
	}
		
	private void showRunIntervalLabels() 
	{
		event_graph_label_graph_test.Visible = false;
		vbox_contacts_simple_graph_controls.Visible = true;
		check_run_show_time.Visible = true;

//		align_check_vbox_contacts_graph_legend.Visible = false;
//		vbox_contacts_graph_legend.Visible = false;
	}
	
	private void showReactionTimeLabels() 
	{
		event_graph_label_graph_test.Visible = true;
		vbox_contacts_simple_graph_controls.Visible = false;

//		align_check_vbox_contacts_graph_legend.Visible = true;
		//vbox_contacts_graph_legend.Visible = false;

		notebook_results_data.Visible = false;
	}

	private void showPulseLabels() 
	{
		event_graph_label_graph_test.Visible = true;
		vbox_contacts_simple_graph_controls.Visible = false;

//		align_check_vbox_contacts_graph_legend.Visible = false;
		//vbox_contacts_graph_legend.Visible = false;

		//show pulse info
		event_execute_table_pulse.Show();
		event_execute_table_pulse_values.Show();

		//initializeLabels
		event_execute_label_pulse_now.Text = "";
		event_execute_label_pulse_avg.Text = "";

		notebook_results_data.Visible = true;
		notebook_results_data.CurrentPage = 2;
	}
	
	/*
	private void eventExecuteCreateComboGraphResultsSize() {
		combo_graph_results_width = ComboBox.NewText ();
		UtilGtk.ComboUpdate(combo_graph_results_width, comboGraphResultsSize, "");
		combo_graph_results_width.Active=2; //300
		
		hbox_combo_graph_results_width.PackStart(combo_graph_results_width, true, true, 0);
		hbox_combo_graph_results_width.ShowAll();
		combo_graph_results_width.Sensitive = true;
		
		combo_graph_results_width.Changed += new EventHandler (on_combo_graph_results_changed);
		
		combo_graph_results_height = ComboBox.NewText ();
		UtilGtk.ComboUpdate(combo_graph_results_height, comboGraphResultsSize, "");
		combo_graph_results_height.Active=1; //200
		
		hbox_combo_graph_results_height.PackStart(combo_graph_results_height, true, true, 0);
		hbox_combo_graph_results_height.ShowAll();
		combo_graph_results_height.Sensitive = true;
		
		combo_graph_results_height.Changed += new EventHandler (on_combo_graph_results_changed);
	}
	*/

	/*	
	private void on_combo_graph_results_changed(object o, EventArgs args) {
		//event_execute_drawingarea.Size(
		event_execute_alignment_drawingarea.SetSizeRequest(
				Convert.ToInt32(UtilGtk.ComboGetActive(combo_graph_results_width)),
				Convert.ToInt32(UtilGtk.ComboGetActive(combo_graph_results_height)));
		sizeChanged = true;
	}
	*/
	
	//reactive, interval, pulse events, put flag needSensitiveButtonFinish to true when started
	//event.cs (Pulse.GTK) calls this method:
	//public void ButtonFinishMakeSensitive() {
	//	event_execute_button_finish.Sensitive = true;
	//}

	/*
	public void ShowSyncMessage(string str) {
		event_execute_textview_sync_message.Text = str;
	}
	*/
		
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

	int allocationXOld;
	int allocationYOld;
	bool sizeChanged;
	public void on_event_execute_drawingarea_configure_event(object o, ConfigureEventArgs args)
	{
		LogB.Information("CONFIGURE START");
		Gdk.EventConfigure ev = args.Event;
		Gdk.Window window = ev.Window;

		Gdk.Rectangle allocation = event_execute_drawingarea.Allocation;
		
		if(event_execute_pixmap == null || sizeChanged ||
				allocation.Width != allocationXOld || allocation.Height != allocationYOld)
		{
			event_execute_pixmap = new Gdk.Pixmap (window, allocation.Width, allocation.Height, -1);
		
			UtilGtk.ErasePaint(event_execute_drawingarea, event_execute_pixmap);
			
			sizeChanged = true;
		}

		allocationXOld = allocation.Width;
		allocationYOld = allocation.Height;
		LogB.Information("CONFIGURE END");
	}

	public void on_event_execute_drawingarea_expose_event(object o, ExposeEventArgs args)
	{
		//LogB.Information("EXPOSE START");
		Gdk.Rectangle allocation = event_execute_drawingarea.Allocation;

		/* in some mono installations, configure_event is not called, but expose_event yes. 
		 * Do here the initialization
		 */
		
		/*
		if(event_execute_pixmap == null || sizeChanged ||
				allocation.Width != allocationXOld || allocation.Height != allocationYOld)
		{
			event_execute_pixmap = new Gdk.Pixmap (event_execute_drawingarea.GdkWindow, allocation.Width, allocation.Height, -1);
			UtilGtk.ErasePaint(event_execute_drawingarea, event_execute_pixmap);

			sizeChanged = false;
		}
		*/

		Gdk.Rectangle area = args.Event.Area;

		//sometimes this is called when paint is finished
		//don't let this erase win
		if(event_execute_pixmap != null) {
			args.Event.Window.DrawDrawable(event_execute_drawingarea.Style.WhiteGC, event_execute_pixmap,
				area.X, area.Y,
				area.X, area.Y,
				area.Width, area.Height);
		}

		//Note this does SQL calls at graph resize, eg. on_extra_window_jumps_test_changed: PrepareEventGraphJumpSimple
		if(sizeChanged)
		{
			//LogB.Information("caring for resize screen and correctly update event_execute_drawingarea");
			/*if(current_mode == Constants.Modes.JUMPSSIMPLE)
				on_extra_window_jumps_test_changed(o, new EventArgs ());
			else if(current_mode == Constants.Modes.JUMPSREACTIVE)
				on_extra_window_jumps_rj_test_changed(o, new EventArgs());
			else if(current_mode == Constants.Modes.RUNSSIMPLE)
				on_extra_window_runs_test_changed(o, new EventArgs());
			else if(current_mode == Constants.Modes.RUNSINTERVALLIC)
				on_extra_window_runs_interval_test_changed(o, new EventArgs());
			else */if(current_mode == Constants.Modes.RT)
				on_extra_window_reaction_times_test_changed(o, new EventArgs());
			else if(current_mode == Constants.Modes.OTHER) {
				if(radio_mode_pulses_small.Active)
					on_extra_window_pulses_test_changed(o, new EventArgs());
				else
					on_extra_window_multichronopic_test_changed(new object(), new EventArgs());
			}
			sizeChanged = false;
		}

		allocationXOld = allocation.Width;
		allocationYOld = allocation.Height;
		//LogB.Information("EXPOSE END");
	}

	//realtime capture graph for jumpRj and runInterval
	public void on_event_execute_drawingarea_realtime_capture_cairo_expose_event (object o, ExposeEventArgs args)
	{
		//right now only for jump reactive
		if(current_mode != Constants.Modes.JUMPSREACTIVE &&
				current_mode != Constants.Modes.RUNSINTERVALLIC)
			return;

		if(current_mode == Constants.Modes.JUMPSREACTIVE)
		{
			if(currentEventExecute == null || currentEventExecute.PrepareEventGraphJumpReactiveRealtimeCaptureObject == null)
				return;

			PrepareJumpReactiveRealtimeCaptureGraph (
					currentEventExecute.PrepareEventGraphJumpReactiveRealtimeCaptureObject.lastTv,
					currentEventExecute.PrepareEventGraphJumpReactiveRealtimeCaptureObject.lastTc,
					currentEventExecute.PrepareEventGraphJumpReactiveRealtimeCaptureObject.tvString,
					currentEventExecute.PrepareEventGraphJumpReactiveRealtimeCaptureObject.tcString,
					currentEventExecute.PrepareEventGraphJumpReactiveRealtimeCaptureObject.type,
					preferences.volumeOn, preferences.gstreamer, repetitiveConditionsWin);
		} else if (current_mode == Constants.Modes.RUNSINTERVALLIC)
		{
			if(currentEventExecute == null || currentEventExecute.PrepareEventGraphRunIntervalRealtimeCaptureObject == null)
				return;

			PrepareRunIntervalRealtimeCaptureGraph(
					currentEventExecute.PrepareEventGraphRunIntervalRealtimeCaptureObject.distance,
					currentEventExecute.PrepareEventGraphRunIntervalRealtimeCaptureObject.lastTime,
					currentEventExecute.PrepareEventGraphRunIntervalRealtimeCaptureObject.timesString,
					currentEventExecute.PrepareEventGraphRunIntervalRealtimeCaptureObject.distanceTotal,
					currentEventExecute.PrepareEventGraphRunIntervalRealtimeCaptureObject.distancesString,
					currentEventExecute.PrepareEventGraphRunIntervalRealtimeCaptureObject.type
					);
		}
	}

	//barplot of tests in session
	public void on_event_execute_drawingarea_cairo_expose_event(object o, ExposeEventArgs args)
	{
		//right now only for jumps/runs simple
		if(current_mode != Constants.Modes.JUMPSSIMPLE &&
				current_mode != Constants.Modes.JUMPSREACTIVE &&
				current_mode != Constants.Modes.RUNSSIMPLE &&
				current_mode != Constants.Modes.RUNSINTERVALLIC)
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
	}

	public void on_event_execute_drawingarea_run_simple_double_contacts_expose_event (object o, ExposeEventArgs args)
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

	// simple and DJ jump	
	public void PrepareJumpSimpleGraph(PrepareEventGraphJumpSimple eventGraph, bool animate)
	{
		/*
		 * if not dj show heights
		 * and it is a single jump type, and it has tc, tv (it is a dj or similar)
		 * then show tc, tf
		 */
		bool useHeights = true;
		if(! eventGraph.djShowHeights &&
				eventGraph.type != "" && //it is a concrete type, not all jumps
				eventGraph.jumpsAtSQL.Count > 0 &&
				eventGraph.jumpsAtSQL[0].Tc > 0 &&
				eventGraph.jumpsAtSQL[0].Tv > 0
				)
			useHeights = false;

		// B) Paint cairo graph
		cairoPaintBarsPre.ShowPersonNames = radio_contacts_graph_allPersons.Active;
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
		cairoPaintBarsPre.ShowPersonNames = radio_contacts_graph_allPersons.Active;
		//cairoPaintBarsPre.UseHeights = useHeights;

		cairoPaintBarsPre.Paint();
	}

	private void on_check_runI_realtime_rel_abs_toggled (object o, EventArgs args)
	{
		// 1) change icon
		if(check_runI_realtime_rel_abs.Active)
			image_check_runI_realtime_rel_abs.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "bar_relative.png");
		else
			image_check_runI_realtime_rel_abs.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "bar_absolute.png");

		// 2) redo graph if possible
		if(currentEventExecute == null || currentEventExecute.PrepareEventGraphRunIntervalRealtimeCaptureObject == null)
			return;

		PrepareRunIntervalRealtimeCaptureGraph(
				currentEventExecute.PrepareEventGraphRunIntervalRealtimeCaptureObject.distance,
				currentEventExecute.PrepareEventGraphRunIntervalRealtimeCaptureObject.lastTime,
				currentEventExecute.PrepareEventGraphRunIntervalRealtimeCaptureObject.timesString,
				currentEventExecute.PrepareEventGraphRunIntervalRealtimeCaptureObject.distanceTotal,
				currentEventExecute.PrepareEventGraphRunIntervalRealtimeCaptureObject.distancesString,
				currentEventExecute.PrepareEventGraphRunIntervalRealtimeCaptureObject.type
				);
	}

	// Reactive jump 
	public void blankJumpReactiveRealtimeCaptureGraph ()
	{
		//constructor for showing a blank graph
		cairoPaintBarsPreRealTime = new CairoPaintBarsPreJumpReactiveRealtimeCapture(
				event_execute_drawingarea_realtime_capture_cairo, preferences.fontType.ToString());
	}

	public void PrepareJumpReactiveRealtimeCaptureGraph (double lastTv, double lastTc, string tvString, string tcString, string type,
			bool volumeOn, Preferences.GstreamerTypes gstreamer, RepetitiveConditionsWindow repetitiveConditionsWin)
	{
		if(currentPerson == null)
			return;

		cairoPaintBarsPreRealTime = new CairoPaintBarsPreJumpReactiveRealtimeCapture(
				event_execute_drawingarea_realtime_capture_cairo, preferences.fontType.ToString(), current_mode,
				currentPerson.Name, type, preferences.digitsNumber,// preferences.heightPreferred,
				lastTv, lastTc, tvString, tcString);

		// B) Paint cairo graph
		//cairoPaintBarsPreRealTime.UseHeights = useHeights;

		cairoPaintBarsPreRealTime.Paint();
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
		cairoPaintBarsPre.ShowPersonNames = radio_contacts_graph_allPersons.Active;
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
		cairoPaintBarsPre.ShowPersonNames = radio_contacts_graph_allPersons.Active;
		cairoPaintBarsPre.RunsShowTime = check_run_show_time.Active;
		cairoPaintBarsPre.Paint();
	}

	public void blankRunIntervalRealtimeCaptureGraph ()
	{
		//constructor for showing a blank graph
		cairoPaintBarsPreRealTime = new CairoPaintBarsPreRunIntervalRealtimeCapture(
				event_execute_drawingarea_realtime_capture_cairo, preferences.fontType.ToString());
	}

	public void PrepareRunIntervalRealtimeCaptureGraph (double distance, double lastTime, string timesString, double distanceTotal, string distancesString, string type)
	{
		if(currentPerson == null)
			return;

		//discard RSA (at the moment)
		if( currentEventExecute.PrepareEventGraphRunIntervalRealtimeCaptureObject.distancesString.Contains("R") )
			return;

		cairoPaintBarsPreRealTime = new CairoPaintBarsPreRunIntervalRealtimeCapture(
				event_execute_drawingarea_realtime_capture_cairo, preferences.fontType.ToString(), current_mode,
				currentPerson.Name, type, preferences.digitsNumber,// preferences.heightPreferred,
				check_runI_realtime_rel_abs.Active,
				distance, lastTime, timesString, distancesString);

		// B) Paint cairo graph
		//cairoPaintBarsPreRealTime.UseHeights = useHeights;

		cairoPaintBarsPreRealTime.Paint();
	}

	// pulse 
	public void PreparePulseGraph(double lastTime, string timesString) { 
		//check graph properties window is not null (propably user has closed it with the DeleteEvent
		//then create it, but not show it
		if(eventGraphConfigureWin == null)
			eventGraphConfigureWin = EventGraphConfigureWindow.Show(false);

		//search MAX 
		double maxValue = 0;
		int topMargin = 20;
		//if max value of graph is automatic
		if(eventGraphConfigureWin.Max == -1) 
			maxValue = Util.GetMax(timesString);
		else {
			maxValue = eventGraphConfigureWin.Max;
			topMargin = 0;
		}
			
		//search MIN 
		double minValue = 1000;
		int bottomMargin = 0;
		//if min value of graph is automatic
		if(eventGraphConfigureWin.Min == -1) 
			minValue = Util.GetMin(timesString);
		else {
			minValue = eventGraphConfigureWin.Min;
		}
			
		int pulses = Util.GetNumberOfJumps(timesString, true); 

		//paint graph
		paintPulse (event_execute_drawingarea, lastTime, timesString, 
				Util.GetAverage(timesString), pulses, maxValue, minValue, topMargin, bottomMargin);
		
		// -- refresh
		event_execute_drawingarea.QueueDraw();
	}
	
	public void PrepareReactionTimeGraph(PrepareEventGraphReactionTime eventGraph, bool animate) 
	{
		//check graph properties window is not null (propably user has closed it with the DeleteEvent
		//then create it, but not show it
		if(eventGraphConfigureWin == null)
			eventGraphConfigureWin = EventGraphConfigureWindow.Show(false);


		double maxValue = 0;
		double minValue = 0;
		int topMargin = 20; 
		int bottomMargin = 0; 

		//if max value of graph is automatic
		if(eventGraphConfigureWin.Max == -1)
			maxValue = eventGraph.sessionMAXAtSQL;
		else {
			maxValue = eventGraphConfigureWin.Max;
			topMargin = 0;
		}
		
		//if min value of graph is automatic
		if(eventGraphConfigureWin.Min == -1)
			minValue = eventGraph.sessionMINAtSQL;
		else
			minValue = eventGraphConfigureWin.Min;
		
		//paint graph
		paintReactionTime (event_execute_drawingarea, eventGraph,
				maxValue, minValue, topMargin, bottomMargin, animate);

		// -- refresh
		event_execute_drawingarea.QueueDraw();
	}
	
	// multi chronopic 
	public void PrepareMultiChronopicGraph(
			//double timestamp, 
			bool cp1StartedIn, bool cp2StartedIn, bool cp3StartedIn, bool cp4StartedIn,
			string cp1InStr, string cp1OutStr, string cp2InStr, string cp2OutStr, 
			string cp3InStr, string cp3OutStr, string cp4InStr, string cp4OutStr) { 
		//check graph properties window is not null (propably user has closed it with the DeleteEvent
		//then create it, but not show it
		if(eventGraphConfigureWin == null)
			eventGraphConfigureWin = EventGraphConfigureWindow.Show(false);

		//search MAX 
		double maxValue = 0;
		int topMargin = 20;
		//if max value of graph is automatic
		/*
		if(eventGraphConfigureWin.Max == -1) 
			//maxValue = timestamp; //TODO: delete this, is not used here
		else {
			//maxValue = eventGraphConfigureWin.Max; //TODO
			topMargin = 0;
		}
		*/
		if(eventGraphConfigureWin.Max != -1) 
			topMargin = 0;
			
		//search MIN 
		double minValue = 1000;
		int bottomMargin = 0;
		//if min value of graph is automatic
		if(eventGraphConfigureWin.Min == -1) 
			minValue = 0;
		else {
			minValue = eventGraphConfigureWin.Min; //TODO
		}
			
		/*
		int cols = Util.GetNumberOfJumps(
				cp1InString + "=" + cp2InString + "=" + cp3InString + "=" + cp4InString, true); 
				*/

		//paint graph
		paintMultiChronopic (event_execute_drawingarea, 
				//timestamp, 
				cp1StartedIn, cp2StartedIn, cp3StartedIn, cp4StartedIn,
				cp1InStr, cp1OutStr, cp2InStr, cp2OutStr, cp3InStr, cp3OutStr, cp4InStr, cp4OutStr, 
				maxValue, minValue, topMargin, bottomMargin);
		
		// -- refresh
		event_execute_drawingarea.QueueDraw();
	}
	
	
	/*
	//used on simple tests
	private void plotSimulatedMessageIfNeededAtLast(int x, int alto) {
		if(event_execute_label_simulated != "") {
			layoutBig.SetMarkup(event_execute_label_simulated);
			int lWidth = 1;
			int lHeight = 1;
			layoutBig.GetPixelSize(out lWidth, out lHeight); 
			event_execute_pixmap.DrawLayout (pen_black, 
					Convert.ToInt32(x - lWidth/2), 
					//Convert.ToInt32(alto/2 - lHeight/2), 
					//10,
					alto - lHeight, 
					layoutBig);
		}
	}
	*/
	//used on jumps reactive, runs interval
	private void plotSimulatedMessageIfNeededAtCenter(int ancho, int alto) {
		if(event_execute_label_simulated != "") {
			layoutBig.SetMarkup(event_execute_label_simulated);
			int lWidth = 1;
			int lHeight = 1;
			layoutBig.GetPixelSize(out lWidth, out lHeight); 
			event_execute_pixmap.DrawLayout (pen_gris,
					Convert.ToInt32(ancho/2 - lWidth/2), 
					Convert.ToInt32(alto/2 - lHeight/2), 
					layoutBig);
		}
	}

	MovingBar movingBar;

	private void drawBar(int x, int y, int barWidth, int alto, int bottomMargin, Gdk.GC pen_bar_bg,
			bool isLast, double result, Pango.Layout layout, bool animate)
	{
		if(isLast && animate) {
			timerBar = true;
			movingBar = new MovingBar(x, alto - bottomMargin, barWidth, y, alto - bottomMargin,
					pen_bar_bg, result, 250, layout);
			movingBar.Start();
			GLib.Timeout.Add(10, new GLib.TimeoutHandler(OnTimerBar));
		}
		else {
			Rectangle rect = new Rectangle(x, y, barWidth, alto-bottomMargin-y-1);
			//LogB.Information(string.Format("drawBar rect y: {0}, height: {1}", y, alto-bottomMargin-y-1));

			event_execute_pixmap.DrawRectangle(pen_bar_bg, true, rect);
			event_execute_pixmap.DrawRectangle(pen_black, false, rect);

			plotResultOnBar(x + barWidth/2, y, alto - bottomMargin, result, layout);
		}
	}

	bool timerBar = true;
	bool OnTimerBar() 
	{ 
		if (!timerBar) 
			return false;

		int yNew = movingBar.NextByDuration();
		Rectangle rect = new Rectangle(movingBar.X, yNew, movingBar.Width, movingBar.YStart - yNew);

		//paint the 0 line
		event_execute_pixmap.DrawLine(pen_black_90,
				movingBar.X, movingBar.AltoTop -1,
				movingBar.X + movingBar.Width, movingBar.AltoTop -1);

		event_execute_pixmap.DrawRectangle(movingBar.Pen_bar_bg, true, rect);
		event_execute_drawingarea.QueueDrawArea(movingBar.X, yNew, movingBar.Width, movingBar.YStart - yNew);

		if(movingBar.Y <= movingBar.YTop)
		{
			rect = new Rectangle(movingBar.X, movingBar.YTop, movingBar.Width, movingBar.AltoTop-movingBar.YTop -1);
			event_execute_pixmap.DrawRectangle(pen_black, false, rect);

			//LogB.Information(string.Format("movinBar: Y: {0}, YTop: {1}, AltoTop: {2}",
			//			movingBar.Y, movingBar.YTop, movingBar.AltoTop));

			plotResultOnBar(movingBar.X + movingBar.Width/2, movingBar.YTop, movingBar.AltoTop, movingBar.Result, movingBar.Layout);
			
			event_execute_drawingarea.QueueDraw();

			timerBar = false;
			return false;
		}

		return true;
	}

	private void paintSimpleAxis(int ancho, int alto, int topMargin, int bottomMargin, Pango.Layout layout, string text)
	{
		// 1) paint the 0 X line
		event_execute_pixmap.DrawLine(pen_black_90,
				10, alto -bottomMargin -1,
				ancho -10, alto -bottomMargin -1);
		// 2) paint the 0 Y line
		event_execute_pixmap.DrawLine(pen_black_90,
				10, alto -bottomMargin -1,
				10, topMargin - 5); //-5 to be a bit upper than the horizontal guides
		// 3) write units
		layout.SetMarkup(text);

		int lWidth = 1;
		int lHeight = 1;
		layout.GetPixelSize(out lWidth, out lHeight);
		event_execute_pixmap.DrawLayout (pen_gris, Convert.ToInt32(10 - .5 * lWidth),
				Convert.ToInt32(.5 * topMargin -7), layout); //-7 for aligning with font 7 baseline
	}

	private void plotSimulatedMessage(int xcenter, int y, Pango.Layout layout)
	{
		layout.SetMarkup(event_execute_label_simulated);
		int lWidth = 1;
		int lHeight = 1;
		layout.GetPixelSize(out lWidth, out lHeight); 
		event_execute_pixmap.DrawLayout (pen_gris,
				Convert.ToInt32(xcenter - lWidth/2),
				y - lHeight,
				layout);
	}

	private int calculateMaxRowsForText (List<Event> events, int longestWordSize, bool allJumps, bool runsPrintTime)
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

			if(runsPrintTime)
				rows ++;

			if(rows > maxRows)
				maxRows = rows;
		}

		return maxRows;
	}

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

	//person name or test type, or both
	//this can separate name with spaces on rows
	private void plotTextBelowBar(int x, int y, int alto,
			string secondResult, 	//time on runSimple
			string jumpType,
			string personName,
			Pango.Layout layout, int longestWordSize, int maxRowsForText)
	{
		// 1) to get the height of the font
		layout.SetMarkup(personName);
		int lWidth = 1;
		int lHeight = 1;
		layout.GetPixelSize(out lWidth, out lHeight);

		int row = 1;

		if(secondResult != "")
		{
			plotTextBelowBarDoRow (x,
					Convert.ToInt32(alto - (maxRowsForText -row +1) * lHeight),
					secondResult, layout, pen_black);
			row ++;
		}

		//if have to print jump type, print it first in one row
		if(jumpType != "")
		{
			plotTextBelowBarDoRow (x,
					Convert.ToInt32(alto - (maxRowsForText -row +1) * lHeight),
					jumpType, layout, pen_azul);
			row ++;
		}

		// 2) separate in rows and send it to plotTextBelowBarDoRow()
		//    packing small words if they fit in a row using wordsAccu (accumulated)

		string wordsAccu = "";
		string [] words = personName.Split(new char[] {' '});

		foreach(string word in words)
		{
			if(wordsAccu == "")
				wordsAccu = word;
			else if( (wordsAccu + " " + word).Length <= longestWordSize )
				wordsAccu += " " + word;
			else {
				plotTextBelowBarDoRow (x,
						Convert.ToInt32(alto - (maxRowsForText -row +1) * lHeight),
						wordsAccu, layout, pen_black);

				wordsAccu = word;
				row ++;
			}
		}
		plotTextBelowBarDoRow (x,
				Convert.ToInt32(alto - (maxRowsForText -row +1) * lHeight),
				wordsAccu, layout, pen_black);
	}
	private void plotTextBelowBarDoRow (int x, int y, string text, Pango.Layout layout, Gdk.GC pen)
	{
		//just to get the width of every row
		layout.SetMarkup(text);
		int lWidth = 1;
		int lHeight = 1;
		layout.GetPixelSize(out lWidth, out lHeight);

		//write text
		event_execute_pixmap.DrawLayout (pen, Convert.ToInt32(x - lWidth/2), y, layout);
	}

	private void plotResultOnBar(int x, int y, int alto, double result, Pango.Layout layout)
	{
		layout.SetMarkup(Util.TrimDecimals(result,2));
		int lWidth = 1;
		int lHeight = 1;
		layout.GetPixelSize(out lWidth, out lHeight); 

		/*
		 * text and surrounding rect are in the middle of bar
		 * if bar is so small, then text and rect will not be fully shown
		 * for this reason, show rect and bar in a higher position
		 * use 2*lHeight in order to accomodate "Simulated" message below
		 */
		int yStart = Convert.ToInt32((y+alto)/2) - lHeight/2;

		if( (yStart + lHeight) > alto )
			yStart = alto - lHeight;

		//draw rectangle behind
		Rectangle rect = new Rectangle(x - lWidth/2, yStart, lWidth, lHeight);
		event_execute_pixmap.DrawRectangle(pen_yellow_bg, true, rect);
		
		//write text
		event_execute_pixmap.DrawLayout (pen_black, 
				Convert.ToInt32(x - lWidth/2),
				yStart,
				layout);
	}

	private void addUnitsToLabel(string unit)
	{
		event_graph_label_graph_test.Text = "<b>" + event_graph_label_graph_test.Text + " </b>(" + unit + ")";
		event_graph_label_graph_test.UseMarkup = true;

		//no because looks ugly
		//radio_contacts_graph_currentTest.Label = radio_contacts_graph_currentTest.Label + " (" + unit + ")";
	}

	private void addLegend (Gdk.GC pen1, string text1, Gdk.GC pen2, string text2, Pango.Layout layout, int ancho, int topMargin, bool horizontal)
	{
		int marginOut = 2;
		int marginIn = 2;
		int boxSize = 9; //the colored rectangle drawn

		layout.SetMarkup(text1);
		int lWidth1 = 1;
		int lHeight1 = 1;
		layout.GetPixelSize(out lWidth1, out lHeight1);

		layout.SetMarkup(text2);
		int lWidth2 = 1;
		int lHeight2 = 1;
		layout.GetPixelSize(out lWidth2, out lHeight2);

		if (horizontal)
			addLegendH (pen1, text1, pen2, text2, layout, ancho, topMargin, marginOut, marginIn, boxSize, lWidth1, lHeight1, lWidth2, lHeight2);
		else
			addLegendV (pen1, text1, pen2, text2, layout, ancho, topMargin, marginOut, marginIn, boxSize, lWidth1, lHeight1, lWidth2, lHeight2);
	}

	private void addLegendH (Gdk.GC pen1, string text1, Gdk.GC pen2, string text2, Pango.Layout layout, int ancho, int topMargin,
			int marginOut, int marginIn, int boxSize, int lWidth1, int lHeight1, int lWidth2, int lHeight2)
	{
		int horizVarSep = 6 * marginIn; //horizontal separation between variables
		int totalWidth = lWidth1 + lWidth2 + 2 * boxSize + 2 * marginIn + horizVarSep; //8 * marginIn (2 are the margins, and 6 the separation between variables)
		int marginLeft = Convert.ToInt32(.5 * ancho - .5 * totalWidth);

		int maxHeight = lHeight1;
		if(lHeight2 > maxHeight)
			maxHeight = lHeight2;
		int totalHeight = 2 * marginIn + maxHeight;
		int marginTop = Convert.ToInt32(.5 * topMargin - .5 * totalHeight);

		//A rectangle drawn filled is 1 pixel smaller in both dimensions than a rectangle outlined.
		//Rectangle rect = new Rectangle(marginLeft + marginOut -1, marginTop + marginOut -1,  totalWidth +1, totalHeight +1);
		//event_execute_pixmap.DrawRectangle(pen_yellow_bg, false, rect);
		Rectangle rect = new Rectangle(marginLeft + marginOut, marginTop + marginOut,  totalWidth, totalHeight);
		event_execute_pixmap.DrawRectangle(pen_white, true, rect); //filled

		//box of text1 && text2
		rect = new Rectangle(marginLeft + marginOut + marginIn, marginTop + marginOut + marginIn + 2, boxSize, boxSize); //+2 to align similar to text
		event_execute_pixmap.DrawRectangle(pen1, true, rect); //filled

		rect = new Rectangle(marginLeft + marginOut + marginIn + boxSize + marginIn + lWidth1 + horizVarSep, marginTop + marginOut + marginIn + 2, boxSize, boxSize);
		event_execute_pixmap.DrawRectangle(pen2, true, rect); //filled

		//write text
		layout.SetMarkup(text1);
		event_execute_pixmap.DrawLayout (pen_black, marginLeft + marginOut + 2 * marginIn + boxSize, marginTop + marginOut + marginIn, layout);
		layout.SetMarkup(text2);
		event_execute_pixmap.DrawLayout (pen_black, marginLeft + marginOut + marginIn + boxSize + marginIn + lWidth1 + horizVarSep + boxSize + marginIn, marginTop + marginOut + marginIn, layout);
	}

	private void addLegendV (Gdk.GC pen1, string text1, Gdk.GC pen2, string text2, Pango.Layout layout, int ancho, int topMargin,
			int marginOut, int marginIn, int boxSize, int lWidth1, int lHeight1, int lWidth2, int lHeight2)
	{
		int maxWidth = lWidth1;
		if(lWidth2 > maxWidth)
			maxWidth = lWidth2;
		maxWidth += 2 * marginIn + boxSize;
		int marginLeft = Convert.ToInt32(.5 * ancho - .5 * maxWidth);

		int totalHeight = lHeight1 + lHeight2 + 4 * marginIn;
		int marginTop = Convert.ToInt32(.5 * topMargin - .5 * totalHeight);
		//TODO: need to implement marginTop below

		//A rectangle drawn filled is 1 pixel smaller in both dimensions than a rectangle outlined.
		Rectangle rect = new Rectangle(marginLeft + marginOut -1, marginOut -1,  maxWidth +1, totalHeight +1);
		event_execute_pixmap.DrawRectangle(pen_yellow_bg, false, rect);
		rect = new Rectangle(marginLeft + marginOut, marginOut,  maxWidth, totalHeight);
		event_execute_pixmap.DrawRectangle(pen_white, true, rect); //filled

		//box of text1 && text2
		rect = new Rectangle(marginLeft + marginOut + marginIn, marginOut + marginIn + 2, boxSize, boxSize); //+2 to align similar to text
		event_execute_pixmap.DrawRectangle(pen1, true, rect); //filled

		rect = new Rectangle(marginLeft + marginOut + marginIn, 2 * (marginOut + marginIn) + 2 + lHeight1, boxSize, boxSize);
		event_execute_pixmap.DrawRectangle(pen2, true, rect); //filled

		//write text
		layout.SetMarkup(text1);
		event_execute_pixmap.DrawLayout (pen_black, marginLeft + marginOut + 2 * marginIn + boxSize, marginOut + marginIn, layout);
		layout.SetMarkup(text2);
		event_execute_pixmap.DrawLayout (pen_black, marginLeft + marginOut + 2 * marginIn + boxSize, 2 * (marginOut + marginIn) + lHeight1, layout);
	}

	private void paintReactionTime (Gtk.DrawingArea drawingarea, PrepareEventGraphReactionTime eventGraph,
			double maxValue, double minValue, int topMargin, int bottomMargin, bool animate)
	{
		int ancho=drawingarea.Allocation.Width;
		int alto=drawingarea.Allocation.Height;
		addUnitsToLabel("s");
		
		//fix problem on show graph at Chronojump start
		if(event_execute_drawingarea == null || event_execute_pixmap == null)
			return;

		UtilGtk.ErasePaint(event_execute_drawingarea, event_execute_pixmap);
		//writeMarginsText(maxValue, minValue, alto);
		
		//check now here that we will have not division by zero problems
		if(maxValue - minValue <= 0)
			return;
		
		//calculate bar width
		int distanceBetweenCols = Convert.ToInt32((ancho-event_execute_rightMargin)*(1+.5)/eventGraph.rtsAtSQL.Length) -
			Convert.ToInt32((ancho-event_execute_rightMargin)*(0+.5)/eventGraph.rtsAtSQL.Length);
		int barWidth = Convert.ToInt32(.3*distanceBetweenCols);
		int barDesplLeft = Convert.ToInt32(.5*barWidth);

		/*
		//paint reference guide black and green if needed
		drawGuideOrAVG(pen_black_discont, eventGraphConfigureWin.BlackGuide, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
		drawGuideOrAVG(pen_green_discont, eventGraphConfigureWin.GreenGuide, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
		*/

		drawGuideOrAVG(pen_black_90, eventGraph.sessionMINAtSQL, alto, ancho, topMargin, bottomMargin, maxValue, minValue, guideWidthEnum.FULL);
		drawGuideOrAVG(pen_black_discont, eventGraph.sessionAVGAtSQL, alto, ancho, topMargin, bottomMargin, maxValue, minValue, guideWidthEnum.FULL);
		drawGuideOrAVG(pen_yellow, eventGraph.personMINAtSQL, alto, ancho, topMargin, bottomMargin, maxValue, minValue, guideWidthEnum.FULL);
		drawGuideOrAVG(pen_yellow_discont, eventGraph.personAVGAtSQL, alto, ancho, topMargin, bottomMargin, maxValue, minValue, guideWidthEnum.FULL);

		//paint the 0 line
		event_execute_pixmap.DrawLine(pen_black_90,
				10, alto -bottomMargin -1,
				ancho -10, alto -bottomMargin -1);

		int x = 0;
		int y = 0;
		int count = eventGraph.rtsAtSQL.Length;
		foreach(string myStr in eventGraph.rtsAtSQL) {
			string [] rts = myStr.Split(new char[] {':'});
			x = Convert.ToInt32((ancho-event_execute_rightMargin)*(count-.5)/eventGraph.rtsAtSQL.Length)-barDesplLeft;
			y = calculatePaintHeight(Convert.ToDouble(rts[5]), alto, maxValue, minValue, 
					topMargin, bottomMargin);

			drawBar(x, y, barWidth, alto, bottomMargin, pen_background, count == eventGraph.rtsAtSQL.Length,
					Convert.ToDouble(rts[5]), layoutMid, animate);//, "", layoutMid, 0, animate);

			count --;
		}
	}


	private void paintPulse (Gtk.DrawingArea drawingarea, double lastTime, string timesString, double avgTime, int pulses, 
			double maxValue, double minValue, int topMargin, int bottomMargin)
	{
		int ancho=drawingarea.Allocation.Width;
		int alto=drawingarea.Allocation.Height;
		
		//fix problem on show graph at Chronojump start
		if(event_execute_drawingarea == null || event_execute_pixmap == null)
			return;
		
		UtilGtk.ErasePaint(event_execute_drawingarea, event_execute_pixmap);
		
		writeMarginsText(maxValue, minValue, alto, "");
		
		//check now here that we will have not division by zero problems
		if(maxValue - minValue > 0) {

			//blue time average discountinuos line	
			drawGuideOrAVG(pen_azul_discont, avgTime, alto, ancho, topMargin, bottomMargin, maxValue, minValue, guideWidthEnum.FULL);

			//paint reference guide black and green if needed
			drawGuideOrAVG(pen_black_discont, eventGraphConfigureWin.BlackGuide, alto, ancho, topMargin, bottomMargin, maxValue, minValue, guideWidthEnum.FULL);
			drawGuideOrAVG(pen_green_discont, eventGraphConfigureWin.GreenGuide, alto, ancho, topMargin, bottomMargin, maxValue, minValue, guideWidthEnum.FULL);

			//blue time evolution	
			string [] myTimesStringFull = timesString.Split(new char[] {'='});
			int count = 0;
			double oldValue = 0;
			double myTimeDouble = 0;

			foreach (string myTime in myTimesStringFull) {
				myTimeDouble = Convert.ToDouble(myTime);
				if(myTimeDouble < 0)
					myTimeDouble = 0;

				if (count > 0) {
					event_execute_pixmap.DrawLine(pen_azul, //blue for time
							Convert.ToInt32((ancho-event_execute_rightMargin)*(count-.5)/pulses), calculatePaintHeight(oldValue, alto, maxValue, minValue, topMargin, bottomMargin),
							Convert.ToInt32((ancho-event_execute_rightMargin)*(count+.5)/pulses), calculatePaintHeight(myTimeDouble, alto, maxValue, minValue, topMargin, bottomMargin));
				}

				//paint Y lines
				if(eventGraphConfigureWin.VerticalGrid) 
					event_execute_pixmap.DrawLine(pen_beige_discont, Convert.ToInt32((ancho - event_execute_rightMargin) *(count+.5)/pulses), topMargin, Convert.ToInt32((ancho - event_execute_rightMargin) *(count+.5)/pulses), alto-topMargin);
				
				
				oldValue = myTimeDouble;
				count ++;
			}
		
			writeValue(pen_azul, myTimeDouble, --count, pulses, ancho, alto, maxValue, minValue, topMargin, bottomMargin);
			
			plotSimulatedMessageIfNeededAtCenter(ancho, alto);
		}
		
		event_execute_label_pulse_now.Text = "<b>" + Util.TrimDecimals(lastTime.ToString(), preferences.digitsNumber) + "</b>";
		event_execute_label_pulse_now.UseMarkup = true; 
		
		event_execute_label_pulse_avg.Text = Util.TrimDecimals(avgTime.ToString(), preferences.digitsNumber);
	}

	double multiChronopicGetX(int ancho, double time, double timeOld, double timeTotal) {
		if(time < 0)
			time = 0;

		//LogB.Information("   timestamp {0}, ancho {1}, x {2}, timeold{3}, xOld{4}", 
		//timestamp, ancho, Util.TrimDecimals(x,1), timeOld, Util.TrimDecimals(xOld,1));

		return ( ancho * ( (timeOld + time) / timeTotal) ) -event_execute_rightMargin;
	}

	int yCp1Out = 20;
	int yCp2Out = 75;
	int yCp3Out = 130;
	int yCp4Out = 185;

	//TODO: fix this method
	private void paintMultiChronopic (Gtk.DrawingArea drawingarea, 
			//double timestamp, 
			bool cp1StartedIn, bool cp2StartedIn, bool cp3StartedIn, bool cp4StartedIn,
			string cp1InStr, string cp1OutStr, string cp2InStr, string cp2OutStr, 
			string cp3InStr, string cp3OutStr, string cp4InStr, string cp4OutStr, 
			double maxValue, double minValue, int topMargin, int bottomMargin)
	{
		double timeTotal1 = Util.GetTotalTime(cp1InStr + "=" + cp1OutStr);
		double timeTotal2 = Util.GetTotalTime(cp2InStr + "=" + cp2OutStr);
		double timeTotal3 = Util.GetTotalTime(cp3InStr + "=" + cp3OutStr);
		double timeTotal4 = Util.GetTotalTime(cp4InStr + "=" + cp4OutStr);
		double timeTotal = 0;


		if(event_execute_eventType == Constants.RunAnalysisName)
			timeTotal = timeTotal2;
		else {
			timeTotal = timeTotal1;
			if(timeTotal2 > timeTotal)
				timeTotal = timeTotal2;
			if(timeTotal3 > timeTotal)
				timeTotal = timeTotal3;
			if(timeTotal4 > timeTotal)
				timeTotal = timeTotal4;
		}
		LogB.Information("total time: {0}" + timeTotal);


		/*
		//TODO: done this now because we come here with only 1 string filled and this does problems
		if(timeTotal == 0) {
			event_execute_alignment.SetSizeRequest(700, 300);
			sizeChanged = true;
			return;
		}
		*/

		int ancho=drawingarea.Allocation.Width;

		UtilGtk.ErasePaint(event_execute_drawingarea, event_execute_pixmap);

		//writeMarginsText(maxValue, minValue, alto);
		writeCpNames();
		if(event_execute_eventType == Constants.RunAnalysisName)
			runAWritePlatformNames();


		//check now here that we will have not division by zero problems
		//if(maxValue - minValue <= 0) 
		//	return;

		LogB.Debug(" paint0 ");
		
		if(event_execute_eventType == Constants.RunAnalysisName)
			paintMultiChronopicRunAPhotocell (ancho, cp1StartedIn, cp1InStr, cp1OutStr, yCp1Out +10, yCp1Out);
		else
			paintMultiChronopicDefault (ancho, cp1StartedIn, cp1InStr, cp1OutStr, timeTotal, yCp1Out +10, yCp1Out);

		LogB.Debug(" paint1 ");
		paintMultiChronopicDefault (ancho, cp2StartedIn, cp2InStr, cp2OutStr, timeTotal, yCp2Out +10, yCp2Out);
		LogB.Debug(" paint2 ");
		paintMultiChronopicDefault (ancho, cp3StartedIn, cp3InStr, cp3OutStr, timeTotal, yCp3Out +10, yCp3Out);
		LogB.Debug(" paint3 ");
		paintMultiChronopicDefault (ancho, cp4StartedIn, cp4InStr, cp4OutStr, timeTotal, yCp4Out +10, yCp4Out);
		LogB.Debug(" paint4 ");

		LogB.Information(" paint done ");
	}

	private void paintMultiChronopicDefault (int ancho, bool cpStartedIn, string cpInStr, string cpOutStr, double timeTotal, int h1, int h2) 
	{
		if(Util.GetTotalTime(cpInStr + "=" + cpOutStr) == 0) 
			return;

		int ticks;
		int heightStart;
		int heightEnd;
		Gdk.GC penStart;
		Gdk.GC penEnd;
		Gdk.GC penStartDiscont;
		Gdk.GC penEndDiscont;
		string [] cpStart;
		string [] cpEnd;
		
		if(cpStartedIn) {
			cpStart = cpInStr.Split(new char[] {'='});
			cpEnd =   cpOutStr.Split(new char[] {'='});
			penStart = pen_rojo;
			penEnd = pen_azul;
			penStartDiscont = pen_rojo_discont;
			penEndDiscont = pen_azul_discont;
			heightStart = h1;
			heightEnd = h2;
		}
		else {
			cpStart = cpOutStr.Split(new char[] {'='});
			cpEnd =   cpInStr.Split(new char[] {'='});
			penStart = pen_azul;
			penEnd = pen_rojo;
			penStartDiscont = pen_azul_discont;
			penEndDiscont = pen_rojo_discont;
			heightStart = h2;
			heightEnd = h1;
		}
		ticks = cpStart.Length;
		double timeOld = 0;
		double xOld = 0;
		bool lastCpIsStart = true;

		LogB.Information("(A) cpInStr:*" + cpInStr + "*, cpOutStr:*" + cpOutStr + "*");

		for(int i=0; i < ticks; i++) { 
			if(cpStart.Length > i) {
				try {
					double x = multiChronopicGetX(ancho, Convert.ToDouble(cpStart[i]), timeOld, timeTotal);
					event_execute_pixmap.DrawLine(penStart, Convert.ToInt32(xOld), heightStart, Convert.ToInt32(x), heightStart);
					timeOld += Convert.ToDouble(cpStart[i]);
					xOld = x;
					lastCpIsStart = true;
				} catch {
					//solve problems if a string is empty or with old value
					//sometimes happens at runAnalysis when there's only cp1
				}
			}

			if(cpEnd.Length > i) {
				try {
					double x = multiChronopicGetX(ancho, Convert.ToDouble(cpEnd[i]), timeOld, timeTotal);
					event_execute_pixmap.DrawLine(penEnd, Convert.ToInt32(xOld), heightEnd, Convert.ToInt32(x), heightEnd);
					timeOld += Convert.ToDouble(cpEnd[i]);
					xOld = x;
					lastCpIsStart = false;
				} catch {
					//solve problems if a string is empty or with old value
					//sometimes happens at runAnalysis when there's only cp1
				}
			}
		}
		
		/*
		   the chronopic that received last event, it's painted and arrives at right end of graph
		   following code allows to paint line also on other chronopics
		   in order to show all updated four cps after any cp change
		   */
		LogB.Debug("(C)");
		if(timeOld < timeTotal) { //this cp didn't received last event
			if(lastCpIsStart)
				event_execute_pixmap.DrawLine(penStartDiscont, Convert.ToInt32(xOld), heightStart, Convert.ToInt32(ancho-event_execute_rightMargin), heightStart);
			else
				event_execute_pixmap.DrawLine(penEndDiscont, Convert.ToInt32(xOld), heightEnd, Convert.ToInt32(ancho-event_execute_rightMargin), heightEnd);
		}
		LogB.Debug("(D)");
	}

	private void paintMultiChronopicRunAPhotocell (int ancho, bool cpStartedIn, string cpInStr, string cpOutStr, int h1, int h2) 
	{
		/*
		if(Util.GetTotalTime(cpInStr + "=" + cpOutStr) == 0) 
			return;
		
		string [] cpIn = cpInStr.Split(new char[] {'='});
		string [] cpOut = cpOutStr.Split(new char[] {'='});
		if(cpOut.Length == 1) {
			layoutSmall.SetMarkup("at first");
			event_execute_pixmap.DrawLayout (pen_gris, 50, yCp1Out, layoutSmall);
		}
		if(cpIn.Length == 1) {
			layoutSmall.SetMarkup("middle");
			event_execute_pixmap.DrawLayout (pen_gris, 70, yCp1Out, layoutSmall);
		}
		if(cpOut.Length == 2) {
			layoutSmall.SetMarkup("arrived");
			event_execute_pixmap.DrawLayout (pen_gris, 200, yCp1Out, layoutSmall);
		}
		*/
	}


	private void writeValue (Gdk.GC myPen, double myValue, int count, int total, int ancho, int alto, 
			double maxValue, double minValue, int topMargin, int bottomMargin) 
	{
		//write text
		layoutSmall.SetMarkup((Math.Round(myValue,3)).ToString());
		event_execute_pixmap.DrawLayout (myPen, ancho -event_execute_rightMargin, (int)calculatePaintHeight(myValue, alto, maxValue, minValue, topMargin, bottomMargin) -7, layoutSmall); //-7 for aligning (is baseline) (font size is 7)
	}

	enum guideWidthEnum { FULL, LEFT, RIGHT }
	private void drawGuideOrAVG(Gdk.GC myPen, double guideHeight, int alto, int ancho, int topMargin, int bottomMargin, double maxValue, double minValue, guideWidthEnum guideWidth ) 
	{
		if(guideHeight == -1)
			return; //return if checkbox guide is not checked
		else {
			int xLeft;
			int xRight;
			if(guideWidth == guideWidthEnum.FULL) {
				xLeft = 10;
				xRight = ancho - event_execute_rightMargin-2;
			}
			else if(guideWidth == guideWidthEnum.LEFT) {
				xLeft = 10;
				xRight = Convert.ToInt32((ancho - event_execute_rightMargin-2) / 2);
			}
			else {	// RIGHT
				xLeft = Convert.ToInt32((ancho - event_execute_rightMargin-2) / 2);
				xRight = ancho - event_execute_rightMargin-2;
			}

			event_execute_pixmap.DrawLine(myPen, 
					xLeft, calculatePaintHeight(guideHeight, alto, maxValue, minValue, topMargin, bottomMargin),
					xRight, calculatePaintHeight(guideHeight, alto, maxValue, minValue, topMargin, bottomMargin));
			//write textual data
			layoutSmall.SetMarkup((Math.Round(guideHeight,1)).ToString());
			event_execute_pixmap.DrawLayout (pen_gris, ancho -event_execute_rightMargin, (int)calculatePaintHeight(guideHeight, alto, maxValue, minValue, topMargin, bottomMargin) -7, layoutSmall); //-7 for aligning with font 7 baseline
		}
	}

	//this calculates the Y of every point in the graph
	//the first "alto -" is because the graph comes from down to up, and we have to reverse
	private int calculatePaintHeight(double currentValue, int alto, double maxValue, double minValue, int topMargin, int bottomMargin) {
		return Convert.ToInt32(alto - bottomMargin - ((currentValue - minValue) * (alto - topMargin - bottomMargin) / (maxValue - minValue)));
	}

	private void writeMarginsText(double maxValue, double minValue, int alto, string units)
	{
		//write margins textual data
		layoutSmall.SetMarkup((Math.Round(maxValue, 3)).ToString() + units);
		event_execute_pixmap.DrawLayout (pen_gris, 0, 0, layoutSmall);
		//event_execute_pixmap.DrawLayout (pen_gris, 0, 3, layoutSmall); //y to 3 (not 0) probably this solves rando Pango problems where this is not written and interface gets "clumsy"
		
		int y = alto - 10;
		layoutSmall.SetMarkup((Math.Round(minValue, 3)).ToString() + units);
		event_execute_pixmap.DrawLayout (pen_gris, 0, y, layoutSmall); //don't search Y using alto - bottomMargin, because bottomMargin can be 0, 
									//and text goes down from the baseline, and will not be seen
	}
	
	//better than above method
	private void writeMarginBottom(double minValue, int alto, string units, int bottomMargin, int ancho)
	{
		int y = alto - (bottomMargin + 10/2);
		layoutSmall.SetMarkup((Math.Round(minValue, 3)).ToString() + units);
		event_execute_pixmap.DrawLayout (pen_gris, 0, y, layoutSmall);
		
		//draw a line below
		event_execute_pixmap.DrawLine(pen_gris, 
				event_execute_rightMargin, alto-bottomMargin, 
				ancho - event_execute_rightMargin-2, alto-bottomMargin);
	}
		
	private void hideButtons() {
		event_execute_button_cancel.Sensitive = false;
		event_execute_button_finish.Sensitive = false;
	}

	private void writeCpNames() {
		layoutSmall.SetMarkup("cp1");
		event_execute_pixmap.DrawLayout (pen_gris, 0, yCp1Out -20, layoutSmall);
		layoutSmall.SetMarkup("cp2");
		event_execute_pixmap.DrawLayout (pen_gris, 0, yCp2Out -20, layoutSmall);
		layoutSmall.SetMarkup("cp3");
		event_execute_pixmap.DrawLayout (pen_gris, 0, yCp3Out -20, layoutSmall);
		layoutSmall.SetMarkup("cp4");
		event_execute_pixmap.DrawLayout (pen_gris, 0, yCp4Out -20, layoutSmall);
	}
	
	private void runAWritePlatformNames() {
		layoutSmall.SetMarkup(Catalog.GetString("Photocells"));
		event_execute_pixmap.DrawLayout (pen_gris, 20, yCp1Out -20, layoutSmall);
		layoutSmall.SetMarkup(Catalog.GetString("Platforms"));
		event_execute_pixmap.DrawLayout (pen_gris, 20, yCp2Out -20, layoutSmall);
	}

	// ---- test simple controls ----->

	private void on_spin_contacts_graph_last_limit_value_changed (object o, EventArgs args)
	{
		if(current_mode == Constants.Modes.JUMPSSIMPLE)
			updateGraphJumpsSimple ();
		else if(current_mode == Constants.Modes.JUMPSREACTIVE)
			updateGraphJumpsReactive ();
		else if(current_mode == Constants.Modes.RUNSSIMPLE)
			updateGraphRunsSimple ();
		else if(current_mode == Constants.Modes.RUNSINTERVALLIC)
			updateGraphRunsInterval ();
	}

	private void on_radio_contacts_graph_test_toggled (object o, EventArgs args)
	{
		if(current_mode == Constants.Modes.JUMPSSIMPLE)
		{
			updateGraphJumpsSimple ();
			pre_fillTreeView_jumps(false);
		}
		else if(current_mode == Constants.Modes.JUMPSREACTIVE)
		{
			updateGraphJumpsReactive ();
			pre_fillTreeView_jumps_rj(false);
		}
		else if(current_mode == Constants.Modes.RUNSSIMPLE)
		{
			updateGraphRunsSimple ();
			pre_fillTreeView_runs(false);
		}
		else if(current_mode == Constants.Modes.RUNSINTERVALLIC)
		{
			updateGraphRunsInterval ();
			pre_fillTreeView_runs_interval(false);
		}
	}

	private void on_radio_contacts_graph_person_toggled (object o, EventArgs args)
	{
		if(current_mode == Constants.Modes.JUMPSSIMPLE)
			updateGraphJumpsSimple ();
		else if(current_mode == Constants.Modes.JUMPSREACTIVE)
			updateGraphJumpsReactive ();
		else if(current_mode == Constants.Modes.RUNSSIMPLE)
			updateGraphRunsSimple ();
		else if(current_mode == Constants.Modes.RUNSINTERVALLIC)
			updateGraphRunsInterval ();
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
							preferences.volumeOn, preferences.gstreamer, repetitiveConditionsWin);
				}
				break;
			case EventType.Types.RUN:
				if(thisRunIsSimple)
					PrepareRunSimpleGraph(currentEventExecute.PrepareEventGraphRunSimpleObject, animate);
				else {
					bool volumeOnHere = preferences.volumeOn;
					//do not play good or bad sounds at RSA because we need to hear the GO sound
					if(currentRunIntervalType.IsRSA)
						volumeOnHere = false;

					PrepareRunIntervalRealtimeCaptureGraph(
							currentEventExecute.PrepareEventGraphRunIntervalRealtimeCaptureObject.distance,
							currentEventExecute.PrepareEventGraphRunIntervalRealtimeCaptureObject.lastTime,
							currentEventExecute.PrepareEventGraphRunIntervalRealtimeCaptureObject.timesString,
							currentEventExecute.PrepareEventGraphRunIntervalRealtimeCaptureObject.distanceTotal,
							currentEventExecute.PrepareEventGraphRunIntervalRealtimeCaptureObject.distancesString,
							currentEventExecute.PrepareEventGraphRunIntervalRealtimeCaptureObject.type
							);
				}
				break;
			case EventType.Types.REACTIONTIME:
					PrepareReactionTimeGraph(currentEventExecute.PrepareEventGraphReactionTimeObject, animate);
				break;
			case EventType.Types.PULSE:
					PreparePulseGraph(
							currentEventExecute.PrepareEventGraphPulseObject.lastTime,
							currentEventExecute.PrepareEventGraphPulseObject.timesString);
				break;
			case EventType.Types.MULTICHRONOPIC:
					PrepareMultiChronopicGraph(
							currentEventExecute.PrepareEventGraphMultiChronopicObject.cp1StartedIn,
							currentEventExecute.PrepareEventGraphMultiChronopicObject.cp2StartedIn,
							currentEventExecute.PrepareEventGraphMultiChronopicObject.cp3StartedIn,
							currentEventExecute.PrepareEventGraphMultiChronopicObject.cp4StartedIn,
							currentEventExecute.PrepareEventGraphMultiChronopicObject.cp1InStr,
							currentEventExecute.PrepareEventGraphMultiChronopicObject.cp1OutStr,
							currentEventExecute.PrepareEventGraphMultiChronopicObject.cp2InStr,
							currentEventExecute.PrepareEventGraphMultiChronopicObject.cp2OutStr, 
							currentEventExecute.PrepareEventGraphMultiChronopicObject.cp3InStr,
							currentEventExecute.PrepareEventGraphMultiChronopicObject.cp3OutStr,
							currentEventExecute.PrepareEventGraphMultiChronopicObject.cp4InStr,
							currentEventExecute.PrepareEventGraphMultiChronopicObject.cp4OutStr);
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
	

	//projecte cubevirtual de juan gonzalez
	
	Gdk.GC pen_rojo; //tc, also time; jump avg personTc
	//Gdk.GC pen_azul_claro; //tf, also speed and pulse; jump avg personTv. This for bars
	Gdk.GC pen_azul; //tf, also speed and pulse; jump avg personTv. This for lines
	Gdk.GC pen_rojo_discont; //avg tc in reactive; jump avg sessionTc 
	//Gdk.GC pen_azul_claro_discont; //avg tf in reactive; jump avg sessionTv
	Gdk.GC pen_azul_discont; //avg tf in reactive; jump avg sessionTv
	Gdk.GC pen_black; //borders of rectangle
	Gdk.GC pen_black_90; //max value on the top
	Gdk.GC pen_yellow; //person max
	Gdk.GC pen_yellow_discont; //person avg
	Gdk.GC pen_yellow_bg; //below person result bar
	Gdk.GC pen_magenta; //person max all sessions
	Gdk.GC pen_black_discont; //guide
	Gdk.GC pen_black_bars; //big borders of rectangle (last event)
	Gdk.GC pen_green_discont; //guide
	Gdk.GC pen_gris; //textual data
	Gdk.GC pen_beige_discont; //Y cols
	Gdk.GC pen_brown_bold; //best tv/tc in rj;
	Gdk.GC pen_violet_bold; //worst tv/tc in rj
	Gdk.GC pen_white;
	Gdk.GC pen_background;
	Gdk.GC pen_background_shifted; //bit darker or lighter than background depending on if background is dark
	

	void event_execute_configureColors()
	{
		//Gdk.Color rojo = new Gdk.Color(0xff,0,0);
		//Gdk.Color azul  = new Gdk.Color(0,0,0xff);
		//Gdk.Color rojo = new Gdk.Color(238,0,0);
		//Gdk.Color azul  = new Gdk.Color(178,223,238);
		Gdk.Color black = new Gdk.Color(0,0,0); 
		Gdk.Color black_90 = new Gdk.Color(0x33,0x33,0x33); 
		Gdk.Color yellow = new Gdk.Color(0xff,0xcc,0x01);
		Gdk.Color yellow_bg = new Gdk.Color(0xff,0xee,0x66);
		Gdk.Color magenta = new Gdk.Color(0xff,0x39,0xff);
		Gdk.Color green = new Gdk.Color(0,0xff,0);
		Gdk.Color gris = new Gdk.Color(0x66,0x66,0x66);
		Gdk.Color beige = new Gdk.Color(0x99,0x99,0x99);
		Gdk.Color brown = new Gdk.Color(0xd6,0x88,0x33);
		Gdk.Color violet = new Gdk.Color(0xc4,0x20,0xf3);
		Gdk.Color white = new Gdk.Color(0xff,0xff,0xff);
		Gdk.Color colorBackground = UtilGtk.ColorParse(preferences.colorBackgroundString); //but note if we are using system colors, this will not match
		Gdk.Color colorBackgroundShifted = UtilGtk.GetColorShifted (colorBackground,
				! UtilGtk.ColorIsDark(colorBackground));

		Gdk.Colormap colormap = Gdk.Colormap.System;
		colormap.AllocColor (ref UtilGtk.RED_PLOTS, true, true);
		colormap.AllocColor (ref UtilGtk.BLUE_PLOTS,true,true);
		colormap.AllocColor (ref UtilGtk.LIGHT_BLUE_PLOTS,true,true);
		colormap.AllocColor (ref black,true,true);
		colormap.AllocColor (ref yellow,true,true);
		colormap.AllocColor (ref yellow_bg,true,true);
		colormap.AllocColor (ref magenta,true,true);
		colormap.AllocColor (ref green,true,true);
		colormap.AllocColor (ref gris,true,true);
		colormap.AllocColor (ref beige,true,true);
		colormap.AllocColor (ref brown,true,true);
		colormap.AllocColor (ref violet,true,true);
		colormap.AllocColor (ref white,true,true);
		colormap.AllocColor (ref colorBackground,true,true);
		colormap.AllocColor (ref colorBackgroundShifted,true,true);

		//-- Configurar los contextos graficos (pinceles)
		pen_rojo = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_yellow = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_yellow_discont = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_yellow_bg = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_magenta = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		//pen_azul_claro = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_azul = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_rojo_discont = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		//pen_azul_claro_discont = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_azul_discont = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_white= new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_black = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_black_90 = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_black_discont = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_black_bars = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_green_discont = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_gris = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_beige_discont = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_brown_bold = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_violet_bold = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_background = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_background_shifted = new Gdk.GC(event_execute_drawingarea.GdkWindow);

		pen_rojo.Foreground = UtilGtk.RED_PLOTS;
		//pen_azul_claro.Foreground = UtilGtk.LIGHT_BLUE_PLOTS;
		pen_azul.Foreground = UtilGtk.BLUE_PLOTS;
		
		pen_rojo_discont.Foreground = UtilGtk.RED_PLOTS;
		//pen_azul_claro_discont.Foreground = UtilGtk.LIGHT_BLUE_PLOTS;
		pen_azul_discont.Foreground = UtilGtk.BLUE_PLOTS;
		pen_rojo_discont.SetLineAttributes(1, Gdk.LineStyle.OnOffDash, Gdk.CapStyle.Butt, Gdk.JoinStyle.Round);
		//pen_azul_claro_discont.SetLineAttributes(1, Gdk.LineStyle.OnOffDash, Gdk.CapStyle.Butt, Gdk.JoinStyle.Round);
		pen_azul_discont.SetLineAttributes(1, Gdk.LineStyle.OnOffDash, Gdk.CapStyle.Butt, Gdk.JoinStyle.Round);
		
		pen_black.Foreground = black;
		pen_black_90.Foreground = black_90;
		pen_yellow.Foreground = yellow;
		pen_yellow_discont.Foreground = yellow;
		pen_yellow_bg.Foreground = yellow_bg;
		pen_magenta.Foreground = magenta;
		pen_black_bars.Foreground = black;
		pen_white.Foreground = white;

		pen_black_discont.Foreground = black;
		pen_black_discont.SetLineAttributes(1, Gdk.LineStyle.OnOffDash, Gdk.CapStyle.Butt, Gdk.JoinStyle.Miter);
		pen_green_discont.Foreground = green;
		pen_green_discont.SetLineAttributes(1, Gdk.LineStyle.OnOffDash, Gdk.CapStyle.Butt, Gdk.JoinStyle.Round);
		
		pen_yellow_discont.SetLineAttributes(1, Gdk.LineStyle.OnOffDash, Gdk.CapStyle.Butt, Gdk.JoinStyle.Round);
		
		pen_gris.Foreground = gris;

		pen_beige_discont.Foreground = beige;
		pen_beige_discont.SetLineAttributes(1, Gdk.LineStyle.OnOffDash, Gdk.CapStyle.Butt, Gdk.JoinStyle.Round);
		
		pen_black_bars.SetLineAttributes(3, Gdk.LineStyle.OnOffDash, Gdk.CapStyle.Butt, Gdk.JoinStyle.Round);

		pen_brown_bold.Foreground = brown;
		pen_brown_bold.SetLineAttributes(2, Gdk.LineStyle.Solid, Gdk.CapStyle.Butt, Gdk.JoinStyle.Round);
		pen_violet_bold.Foreground = violet;
		pen_violet_bold.SetLineAttributes(2, Gdk.LineStyle.Solid, Gdk.CapStyle.Butt, Gdk.JoinStyle.Round);

		pen_background.Foreground = colorBackground;
		pen_background_shifted.Foreground = colorBackgroundShifted;
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

	protected DrawingArea darea;
	protected string fontStr;
	protected Constants.Modes mode;
	protected string personName;
	protected string testName;
	protected string title;
	protected int pDN; //preferences.digitsNumber

	protected void initialize (DrawingArea darea, string fontStr, Constants.Modes mode,
			string personName, string testName, int pDN)
	{
		this.darea = darea;
		this.fontStr = fontStr;
		this.mode = mode;
		this.personName = personName;
		this.testName = testName;
		this.pDN = pDN;
	}

	public bool ModeMatches (Constants.Modes mode)
	{
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
			new CairoBars1Series (darea, fontStr, "");
		} catch {
			LogB.Information("saved crash at with cairo paint (blank screen)");
		}
	}

	public void Paint ()
	{
		if(darea == null || darea.GdkWindow == null) //at start program, this can fail
			return;

		if(! storeCreated())
			return;

		if(! haveDataToPlot())
		{
			try {
				new CairoBars1Series (darea, fontStr, testsNotFound());
			} catch {
				LogB.Information("saved crash at with cairo paint");
			}
			return;
		}

		paintSpecific();
	}

	protected virtual string testsNotFound ()
	{
		if(personName != "")
		{
			if(testName != "")
				return string.Format(Catalog.GetString("{0} has not run any {1} test in this session."),
						personName, testName);
			else
				return string.Format(Catalog.GetString("{0} has not run any test in this session."),
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
			bool allJumps, bool thereIsASimulated, bool runsPrintTime)
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

			if(runsPrintTime)
				rows ++;

			if(rows > maxRows)
				maxRows = rows;
		}
		//LogB.Information("maxRows: " + maxRows.ToString());

		return maxRows;
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
			if(allTypes && ev.Type.Length > longestWordSize)
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

		LogB.Information("longestWord: " + longestWord);
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

		bool newLineDone = false;
		foreach(string word in words)
		{
			if(wordsAccu == "")
				wordsAccu = word;
			else if( (wordsAccu + " " + word).Length <= longestWordSize )
				wordsAccu += " " + word;
			else {
				str += vertSep + wordsAccu;
				vertSep = "\n";
				newLineDone = true;
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

		CairoBars cb;
		if(showBarA && showBarB) //Dja, Djna
			cb = new CairoBarsNHSeries (darea);
		else if (showBarA) //takeOff, takeOffWeight
			cb = new CairoBars1Series (darea);
		else //rest of the jumps: sj, cmj, ..
			cb = new CairoBars1Series (darea);

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

		cb.GraphInit(fontStr, ! ShowPersonNames, true); //usePersonGuides, useGroupGuides

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

		//manage bottom text font/spacing of rows
		string longestWord = findLongestWordCairo (events,
				eventGraphJumpsStored.type == "",
				"",
				"(" + Catalog.GetString("Simulated") + ")"); // condition for "all runs"
		int fontHeightForBottomNames = cb.GetFontForBottomNames (events, longestWord);

		int maxRowsForText = calculateMaxRowsForTextCairo (events, longestWord.Length,
				eventGraphJumpsStored.type == "", thereIsASimulated, false);
		int bottomMargin = cb.GetBottomMarginForText (maxRowsForText, fontHeightForBottomNames);


		List<PointF> pointA_l = new List<PointF>();
		List<PointF> pointB_l = new List<PointF>();
		List<string> names_l = new List<string>();

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
		}

		cb.PassGuidesData (new CairoBarsGuideManage(
					! ShowPersonNames, true, //usePersonGuides, useGroupGuides
					eventGraphJumpsStored.sessionMAXAtSQL,
					eventGraphJumpsStored.sessionAVGAtSQL,
					eventGraphJumpsStored.sessionMINAtSQL,
					eventGraphJumpsStored.personMAXAtSQLAllSessions,
					eventGraphJumpsStored.personMAXAtSQL,
					eventGraphJumpsStored.personAVGAtSQL,
					eventGraphJumpsStored.personMINAtSQL));

		if(showBarA && showBarB) //Dja, Djna
		{
			List<List<PointF>> pointSecondary_l = new List<List<PointF>>();
			pointSecondary_l.Add(pointA_l);
			cb.PassPointSecondaryList(pointSecondary_l);

			cb.GraphDo (pointA_l, pointB_l, names_l,
					fontHeightForBottomNames, bottomMargin, title);
		} else if (showBarA) //takeOff, takeOffWeight
			cb.GraphDo (pointA_l, new List<PointF>(), names_l,
					fontHeightForBottomNames, bottomMargin, title);
		else //rest of the jumps: sj, cmj, ..
			cb.GraphDo (pointB_l, new List<PointF>(), names_l,
					fontHeightForBottomNames, bottomMargin, title);
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
		CairoBars cb = new CairoBarsNHSeries (darea);

		cb.YVariable = Catalog.GetString("Time");
		cb.YUnits = "s";
		cb.VariableSerieA = Catalog.GetString("Contact time") + " (" + Catalog.GetString("AVG") + ") ";
		cb.VariableSerieB = Catalog.GetString("Flight time") + " (" + Catalog.GetString("AVG") + ") ";

		cb.GraphInit(fontStr, ! ShowPersonNames, true); //usePersonGuides, useGroupGuides

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

		//manage bottom text font/spacing of rows
		string longestWord = findLongestWordCairo (events,
				//eventGraphJumpsRjStored.type == "",
				true,
				" - 99", //thinking on 99 jumps
				"(" + Catalog.GetString("Simulated") + ")"); // condition for "all runs"
		int fontHeightForBottomNames = cb.GetFontForBottomNames (events, longestWord);

		/*
		int maxRowsForText = calculateMaxRowsForTextCairo (events, longestWord.Length,
				eventGraphJumpsRjStored.type == "", thereIsASimulated, false);
				*/
		//TYPE A: on jumpRj show always jump type to show at the side the number of jumps. If change here, change it below (TYPEB)
		int maxRowsForText = calculateMaxRowsForTextCairo (events, longestWord.Length,
				true, thereIsASimulated, false);

		int bottomMargin = cb.GetBottomMarginForText (maxRowsForText, fontHeightForBottomNames);


		//List<PointF> pointA0_l = new List<PointF>();
		List<PointF> pointA1_l = new List<PointF>();

		List<PointF> pointB_l = new List<PointF>();
		List<string> names_l = new List<string>();

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
		}

		cb.PassGuidesData (new CairoBarsGuideManage(
					! ShowPersonNames, true, //usePersonGuides, useGroupGuides
					eventGraphJumpsRjStored.sessionMAXAtSQL,
					eventGraphJumpsRjStored.sessionAVGAtSQL,
					eventGraphJumpsRjStored.sessionMINAtSQL,
					0,
					eventGraphJumpsRjStored.personMAXAtSQL,
					eventGraphJumpsRjStored.personAVGAtSQL,
					eventGraphJumpsRjStored.personMINAtSQL
					));

		List<List<PointF>> pointSecondary_l = new List<List<PointF>>();
		//pointSecondary_l.Add(pointA0_l);
		pointSecondary_l.Add(pointA1_l);
		cb.PassPointSecondaryList(pointSecondary_l);

		cb.GraphDo (pointA1_l, pointB_l, names_l,
				fontHeightForBottomNames, bottomMargin, title);
	}
}

public class CairoPaintBarsPreRunSimple : CairoPaintBarsPre
{
	public CairoPaintBarsPreRunSimple (DrawingArea darea, string fontStr, Constants.Modes mode, string personName, string testName, int pDN)
	{
		initialize (darea, fontStr, mode, personName, testName, pDN);
		this.title = generateTitle();
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
		CairoBars1Series cb = new CairoBars1Series (darea);

		cb.YVariable = Catalog.GetString("Speed");
		cb.YUnits = "m/s";

		cb.GraphInit(fontStr, ! ShowPersonNames, true); //usePersonGuides, useGroupGuides

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

		//manage bottom text font/spacing of rows
		string longestWord = findLongestWordCairo (events,
				eventGraphRunsStored.type == "",
				"",
				"(" + Catalog.GetString("Simulated") + ")"); // condition for "all runs"
		int fontHeightForBottomNames = cb.GetFontForBottomNames (events, longestWord);

		int maxRowsForText = calculateMaxRowsForTextCairo (events, longestWord.Length,
				eventGraphRunsStored.type == "", thereIsASimulated, RunsShowTime);
		int bottomMargin = cb.GetBottomMarginForText (maxRowsForText, fontHeightForBottomNames);

		//LogB.Information(string.Format("fontHeightForBottomNames: {0}, bottomMargin: {1}", fontHeightForBottomNames, bottomMargin));

		List<PointF> point_l = new List<PointF>();
		List<string> names_l = new List<string>();

		int countToDraw = eventGraphRunsStored.runsAtSQL.Count;
		foreach(Run run in eventGraphRunsStored.runsAtSQL)
		{
			// 1) Add data
			point_l.Add(new PointF(countToDraw --, run.Distance/run.Time));

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
		}

		cb.PassGuidesData (new CairoBarsGuideManage(
					! ShowPersonNames, true, //usePersonGuides, useGroupGuides
					eventGraphRunsStored.sessionMAXAtSQL,
					eventGraphRunsStored.sessionAVGAtSQL,
					eventGraphRunsStored.sessionMINAtSQL,
					eventGraphRunsStored.personMAXAtSQLAllSessions,
					eventGraphRunsStored.personMAXAtSQL,
					eventGraphRunsStored.personAVGAtSQL,
					eventGraphRunsStored.personMINAtSQL));

		cb.GraphDo(point_l, new List<PointF>(), names_l,
				fontHeightForBottomNames, bottomMargin, title);
	}
}

public class CairoPaintBarsPreRunInterval : CairoPaintBarsPre
{
	public CairoPaintBarsPreRunInterval (DrawingArea darea, string fontStr, Constants.Modes mode, string personName, string testName, int pDN)
	{
		initialize (darea, fontStr, mode, personName, testName, pDN);
		this.title = generateTitle();
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
		CairoBars1Series cb = new CairoBars1Series (darea);

		cb.YVariable = Catalog.GetString("Speed");
		cb.YUnits = "m/s";

		cb.GraphInit(fontStr, ! ShowPersonNames, true); //usePersonGuides, useGroupGuides

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

		//manage bottom text font/spacing of rows
		string longestWord = findLongestWordCairo (events,
				eventGraphRunsIntervalStored.type == "",
				" - 99", //thinking on 99 tracks
				"(" + Catalog.GetString("Simulated") + ")"); // condition for "all runs"
		int fontHeightForBottomNames = cb.GetFontForBottomNames (events, longestWord);

		/*
		int maxRowsForText = calculateMaxRowsForTextCairo (events, longestWord.Length,
				eventGraphRunsIntervalStored.type == "", thereIsASimulated, RunsShowTime);
				*/
		//TYPE A: on runI show always run type to show at the side the number of tracks. If change here, change it below (TYPEB)
		int maxRowsForText = calculateMaxRowsForTextCairo (events, longestWord.Length,
				true, thereIsASimulated, RunsShowTime);

		int bottomMargin = cb.GetBottomMarginForText (maxRowsForText, fontHeightForBottomNames);

		//LogB.Information(string.Format("fontHeightForBottomNames: {0}, bottomMargin: {1}", fontHeightForBottomNames, bottomMargin));

		List<PointF> point_l = new List<PointF>();
		List<string> names_l = new List<string>();

		int countToDraw = eventGraphRunsIntervalStored.runsAtSQL.Count;
		foreach(RunInterval runI in eventGraphRunsIntervalStored.runsAtSQL)
		{
			// 1) Add data
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
		}

		cb.PassGuidesData (new CairoBarsGuideManage(
					! ShowPersonNames, true, //usePersonGuides, useGroupGuides
					eventGraphRunsIntervalStored.sessionMAXAtSQL,
					eventGraphRunsIntervalStored.sessionAVGAtSQL,
					eventGraphRunsIntervalStored.sessionMINAtSQL,
					0,
					eventGraphRunsIntervalStored.personMAXAtSQL,
					eventGraphRunsIntervalStored.personAVGAtSQL,
					eventGraphRunsIntervalStored.personMINAtSQL));

		cb.GraphDo(point_l, new List<PointF>(), names_l,
				fontHeightForBottomNames, bottomMargin, title);
	}
}

//realtime jump reactive capture
public class CairoPaintBarsPreJumpReactiveRealtimeCapture : CairoPaintBarsPre
{
	private double lastTv;
	private double lastTc;
	private List<double> tv_l;
	private List<double> tc_l;

	//just blank the screen
	public CairoPaintBarsPreJumpReactiveRealtimeCapture (DrawingArea darea, string fontStr)
	{
		blankScreen(darea, fontStr);
	}

	public CairoPaintBarsPreJumpReactiveRealtimeCapture (DrawingArea darea, string fontStr,
			Constants.Modes mode, string personName, string testName, int pDN,// bool heightPreferred,
			double lastTv, double lastTc, string tvString, string tcString)
	{
		initialize (darea, fontStr, mode, personName, testName, pDN);
		this.title = Catalog.GetString("Last test:") + " " + generateTitle();

		this.lastTv = lastTv;
		this.lastTc = lastTc;

		tv_l = new List<double>();
		tc_l = new List<double>();

		string [] tvFull = tvString.Split(new char[] {'='});
		string [] tcFull = tcString.Split(new char[] {'='});
		if(tvFull.Length != tcFull.Length)
			return;

		foreach(string tv in tvFull)
			if(Util.IsNumber(tv, true))
				tv_l.Add(Convert.ToDouble(tv));
		foreach(string tc in tcFull)
			if(Util.IsNumber(tc, true))
				tc_l.Add(Convert.ToDouble(tc));
	}

	/*
	public override void StoreEventGraphJumpReactiveCapture (PrepareEventGraphJumpReactiveRealtimeCapture eventGraph)
	{
		this.eventGraphJumpReactiveCapture = eventGraph;
	}
	*/

	protected override bool storeCreated ()
	{
		return (tv_l.Count == tc_l.Count && tv_l.Count > 0);
	}

	protected override bool haveDataToPlot()
	{
		return (tv_l.Count == tc_l.Count && tv_l.Count > 0);
	}

	protected override void paintSpecific()
	{
		//extra check
		if(tv_l.Count != tc_l.Count)
			return;

		CairoBars cb = new CairoBarsNHSeries (darea);

		cb.YVariable = Catalog.GetString("Time");
		cb.YUnits = "s";

		cb.VariableSerieA = Catalog.GetString("Contact time");
		cb.VariableSerieB = Catalog.GetString("Flight time");

		cb.GraphInit(fontStr, true, false); //usePersonGuides, useGroupGuides

		List<PointF> pointA_l = new List<PointF>();
		List<PointF> pointB_l = new List<PointF>();
		List<string> names_l = new List<string>();

		//statistics for tv
		double max = 0;
		double sum = 0; //for tv_l avg
		double min = 1000;

		for(int i = tv_l.Count -1; i >= 0; i --)
		{
			double tc = Convert.ToDouble(tc_l[i]);
			double tv = Convert.ToDouble(tv_l[i]);

			pointA_l.Add(new PointF(i+1, tc));
			pointB_l.Add(new PointF(i+1, tv));
			names_l.Add((i+1).ToString());

			//get max (only of tv)
			if(tv > max)
				max = tv;

			//get avg (only of tv)
			sum += Convert.ToDouble(tv);

			//get min (only of tv)
			if(tv < min)
				min = tv;
		}

		cb.PassGuidesData (new CairoBarsGuideManage(
					true, false, //usePersonGuides, useGroupGuides
					0,
					0,
					0,
					0,
					max,
					sum / tv_l.Count,
					min));

		List<List<PointF>> pointSecondary_l = new List<List<PointF>>();
		pointSecondary_l.Add(pointA_l);
		cb.PassPointSecondaryList(pointSecondary_l);

		cb.GraphDo (pointA_l, pointB_l, names_l,
				14, 8, title);
	}
}

//TODO: care for R (RSAs) on distancesString
public class CairoPaintBarsPreRunIntervalRealtimeCapture : CairoPaintBarsPre
{
	private double lastDistance;
	private double lastTime;
	private bool isRelative; //related to names: distance and time

	private List<double> distance_l;
	private List<double> time_l;
	private List<double> speed_l;

	//just blank the screen
	public CairoPaintBarsPreRunIntervalRealtimeCapture (DrawingArea darea, string fontStr)
	{
		blankScreen(darea, fontStr);
	}

	public CairoPaintBarsPreRunIntervalRealtimeCapture (DrawingArea darea, string fontStr,
			Constants.Modes mode, string personName, string testName, int pDN,// bool heightPreferred,
			bool isRelative,
			double lastDistance, double lastTime, string timesString, string distancesString)
	{
		initialize (darea, fontStr, mode, personName, testName, pDN);
		this.title = Catalog.GetString("Last test:") + " " + generateTitle();

		this.lastDistance = lastDistance;
		this.lastTime = lastTime;
		this.isRelative = isRelative;

		distance_l = new List<double>();
		time_l = new List<double>();
		speed_l = new List<double>();

		string [] timeFull = timesString.Split(new char[] {'='});
		foreach(string t in timeFull)
			if(Util.IsNumber(t, true))
			{
				double tDouble = Convert.ToDouble(t);
				if(tDouble < 0)
					time_l.Add(0);
				else
					time_l.Add(tDouble);
			}

		int count = 0;
		foreach (double time in time_l)
		{
			double distance = lastDistance;
			if(distancesString != "") //if distances are variable
				distance = Util.GetRunIVariableDistancesStringRow(distancesString, count);

			distance_l.Add(distance);
			speed_l.Add(distance / time);
			count ++;
		}

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

		CairoBars cb = new CairoBars1Series (darea);

		cb.YVariable = Catalog.GetString("Speed");
		cb.YUnits = "m/s";

		cb.GraphInit(fontStr, true, false); //usePersonGuides, useGroupGuides

		List<PointF> point_l = new List<PointF>();
		List<string> names_l = new List<string>();

		//statistics for speed
		double max = 0;
		double sum = 0; //for speed_l avg
		double min = 1000;

		//for absolute data. Absolute is from the beginning.
		double distanceTotal = 0;
		double timeTotal = 0;
		for(int i = 0; i < time_l.Count; i ++)
		{
			distanceTotal += distance_l[i];
			timeTotal += time_l[i];
		}
		double distanceAccumulated = distanceTotal;
		double timeAccumulated = timeTotal;

		for(int i = time_l.Count -1; i >= 0; i --)
		{
			double time = Convert.ToDouble(time_l[i]);
			double speed = Convert.ToDouble(speed_l[i]);

			point_l.Add(new PointF(i+1, speed));

			if(isRelative)
				names_l.Add(string.Format("{0} m\n{1} s",
							distance_l[i], Util.TrimDecimals(time,2)));
			else {
				names_l.Add(string.Format("{0} m\n{1} s",
							distanceAccumulated, Util.TrimDecimals(timeAccumulated,2)));
				distanceAccumulated -= distance_l[i];
				timeAccumulated -= time_l[i];
			}

			if(speed > max) 	//get max
				max = speed;

			sum += speed;		//get avg

			if(speed < min)		//get min
				min = speed;
		}

		cb.PassGuidesData (new CairoBarsGuideManage(
					true, false, //usePersonGuides, useGroupGuides
					0, 0, 0, 0,
					max,
					sum / speed_l.Count,
					min));

		cb.GraphDo (point_l, new List<PointF>(), names_l,
				14, 22, title); //22 because there are two rows
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
		if(darea == null || darea.GdkWindow == null) //at start program, this can fail
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
