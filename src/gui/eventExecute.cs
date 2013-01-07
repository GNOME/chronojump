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
 * Copyright (C) 2004-2011   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Glade;
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using Mono.Unix;
using Gdk; //for the EventMask
using LongoMatch.Gui;
using LongoMatch.Video.Capturer;
using LongoMatch.Video.Common;



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
	[Widget] Gtk.Label event_graph_label_graph_person;
	[Widget] Gtk.Label event_graph_label_graph_test;
	
	[Widget] Gtk.ProgressBar event_execute_progressbar_event;
	[Widget] Gtk.ProgressBar event_execute_progressbar_time;
	

	//currently gtk-sharp cannot display a label in a progressBar in activity mode (Pulse() not Fraction)
	//then we show the value in a label:
	[Widget] Gtk.Label event_execute_label_event_value;
	[Widget] Gtk.Label event_execute_label_time_value;
	
	[Widget] Gtk.Button event_execute_button_cancel;
	[Widget] Gtk.Button event_execute_button_finish;
	[Widget] Gtk.Button event_execute_button_update;
	
	
	[Widget] Gtk.Table event_execute_table_jump_simple;
	[Widget] Gtk.Table event_execute_table_jump_reactive;
	[Widget] Gtk.Table event_execute_table_run_simple;
	[Widget] Gtk.Table event_execute_table_run_interval;
	[Widget] Gtk.Table event_execute_table_pulse;
	[Widget] Gtk.Table event_execute_table_reaction_time;
	
	[Widget] Gtk.Table event_execute_table_jump_simple_values;
	[Widget] Gtk.Table event_execute_table_jump_reactive_values;
	[Widget] Gtk.Table event_execute_table_run_simple_values;
	[Widget] Gtk.Table event_execute_table_run_interval_values;
	[Widget] Gtk.Table event_execute_table_pulse_values;
	[Widget] Gtk.Table event_execute_table_reaction_time_values;

	[Widget] Gtk.HBox event_execute_hbox_jump_simple_titles;
	[Widget] Gtk.HBox event_execute_hbox_run_simple_titles;
	[Widget] Gtk.HBox event_execute_hbox_reaction_time_titles;

	//for the color change in the background of the cell label
	[Widget] Gtk.EventBox event_execute_eventbox_jump_simple_tc;
	[Widget] Gtk.EventBox event_execute_eventbox_jump_simple_tf;
	[Widget] Gtk.EventBox event_execute_eventbox_jump_reactive_tc;
	[Widget] Gtk.EventBox event_execute_eventbox_jump_reactive_tf;
	//[Widget] Gtk.EventBox event_execute_eventbox_jump_reactive_tf_tc;
	[Widget] Gtk.EventBox event_execute_eventbox_run_simple_time;
	[Widget] Gtk.EventBox event_execute_eventbox_run_simple_speed;
	[Widget] Gtk.EventBox event_execute_eventbox_run_interval_time;
	[Widget] Gtk.EventBox event_execute_eventbox_run_interval_speed;
	[Widget] Gtk.EventBox event_execute_eventbox_pulse_time;
	[Widget] Gtk.EventBox event_execute_eventbox_reaction_time_time;

	
	[Widget] Gtk.Label event_execute_label_jump_simple_tc_now;
	[Widget] Gtk.Label event_execute_label_jump_simple_tc_person;
	[Widget] Gtk.Label event_execute_label_jump_simple_tc_session;
	[Widget] Gtk.Label event_execute_label_jump_simple_tf_now;
	[Widget] Gtk.Label event_execute_label_jump_simple_tf_person;
	[Widget] Gtk.Label event_execute_label_jump_simple_tf_session;
	[Widget] Gtk.Label event_execute_label_jump_simple_height_now;
	[Widget] Gtk.Label event_execute_label_jump_simple_height_person;
	[Widget] Gtk.Label event_execute_label_jump_simple_height_session;

	[Widget] Gtk.Label event_execute_label_jump_reactive_tc_now;
	[Widget] Gtk.Label event_execute_label_jump_reactive_tc_avg;
	[Widget] Gtk.Label event_execute_label_jump_reactive_tf_now;
	[Widget] Gtk.Label event_execute_label_jump_reactive_tf_avg;
	[Widget] Gtk.Label event_execute_label_jump_reactive_tf_tc_now;
	[Widget] Gtk.Label event_execute_label_jump_reactive_tf_tc_avg;

	[Widget] Gtk.Label event_execute_label_run_simple_time_now;
	[Widget] Gtk.Label event_execute_label_run_simple_time_person;
	[Widget] Gtk.Label event_execute_label_run_simple_time_session;
	[Widget] Gtk.Label event_execute_label_run_simple_speed_now;
	[Widget] Gtk.Label event_execute_label_run_simple_speed_person;
	[Widget] Gtk.Label event_execute_label_run_simple_speed_session;

	[Widget] Gtk.Label event_execute_label_run_interval_time_now;
	[Widget] Gtk.Label event_execute_label_run_interval_time_avg;
	[Widget] Gtk.Label event_execute_label_run_interval_speed_now;
	[Widget] Gtk.Label event_execute_label_run_interval_speed_avg;
	
	[Widget] Gtk.Label event_execute_label_pulse_now;
	[Widget] Gtk.Label event_execute_label_pulse_avg;

	[Widget] Gtk.Label event_execute_label_reaction_time_now;
	[Widget] Gtk.Label event_execute_label_reaction_time_person;
	[Widget] Gtk.Label event_execute_label_reaction_time_session;

	[Widget] Gtk.Image event_execute_image_jump_reactive_tf_good;
	[Widget] Gtk.Image event_execute_image_jump_reactive_tf_bad;
	[Widget] Gtk.Image event_execute_image_jump_reactive_tc_good;
	[Widget] Gtk.Image event_execute_image_jump_reactive_tc_bad;
	[Widget] Gtk.Image event_execute_image_jump_reactive_tf_tc_good;
	[Widget] Gtk.Image event_execute_image_jump_reactive_tf_tc_bad;

	[Widget] Gtk.Image event_execute_image_run_interval_time_good;
	[Widget] Gtk.Image event_execute_image_run_interval_time_bad;
	
	[Widget] Gtk.Notebook notebook_results_data;
	
	[Widget] Gtk.DrawingArea event_execute_drawingarea;
	[Widget] Box event_execute_hbox_drawingarea;
	/*
	[Widget] Gtk.Box hbox_combo_graph_results_width;
	[Widget] Gtk.Box hbox_combo_graph_results_height;
	[Widget] Gtk.ComboBox combo_graph_results_width;
	[Widget] Gtk.ComboBox combo_graph_results_height;
	*/

	[Widget] Gtk.Alignment event_execute_alignment;
	//[Widget] Gtk.Alignment event_execute_alignment_drawingarea;
	//static Gdk.Pixmap event_execute_pixmap = null;
	Gdk.Pixmap event_execute_pixmap = null;
	
	//[Widget] Gtk.HBox hbox_capture;


	int event_execute_personID;	
	//int sessionID;
	//string event_execute_personName;	
	string event_execute_tableName;	
	string event_execute_eventType;	
	
	//double event_execute_limit;
	
	private enum phasesGraph {
		UNSTARTED, DOING, DONE
	}
	
	int event_execute_radio = 8; 		//radious of the circles
	int event_execute_arcSystemCorrection = 0; //on Windows circles are paint just one pixel left, fix it
	int event_execute_rightMargin = 30; 	//at the right we write text (on windows we change later)

	/*
	 * when click on destroy window, delete event is raised
	 * if event has ended, then it should normally close the window
	 * if has not ended, then it should cancel it before.
	 * on 0.7.5.2 and before, we always cancel, 
	 * and this produces and endless loop when event has ended, because there's nothing to cancel
	 */
	bool eventHasEnded;

	//ExecutingGraphData executingGraphData;

	
	//for writing text
	Pango.Layout layout;

	static EventGraphConfigureWindow eventGraphConfigureWin;
	
	private static string [] comboGraphResultsSize = {
		"100", "200", "300", "400", "500"
	};
	

	ExecutingGraphData event_execute_initializeVariables (
			int personID,
			string personName,
			string phasesName, 
			string tableName,
			string event_execute_eventType
			) 
	{
		eventExecuteHideAllTables();
		eventExecuteHideImages();
		
		if(Util.IsWindows()) {
			event_execute_rightMargin = 50;
			event_execute_arcSystemCorrection = 1;
		}

		event_execute_configureColors();
	
		event_graph_label_graph_person.Text = personName;
		event_graph_label_graph_test.Text = event_execute_eventType;
				
		event_execute_label_message.Text = "";

		//this.event_execute_personName.Text = event_execute_personName; 	//"Jumps" (rjInterval), "Runs" (runInterval), "Ticks" (pulses), 
		this.event_execute_label_phases_name.Text = phasesName; 	//"Jumps" (rjInterval), "Runs" (runInterval), "Ticks" (pulses), 
								//"Phases" (simple jumps, dj, simple runs)
		this.event_execute_personID = personID;
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

		event_execute_button_cancel.Sensitive = true;

		event_execute_clearDrawingArea();
		clearProgressBars();

	
		event_execute_eventbox_jump_simple_tc.ModifyBg(Gtk.StateType.Normal, UtilGtk.RED_PLOTS);
		event_execute_eventbox_jump_simple_tf.ModifyBg(Gtk.StateType.Normal, UtilGtk.BLUE_PLOTS);
		event_execute_eventbox_jump_reactive_tc.ModifyBg(Gtk.StateType.Normal, UtilGtk.RED_PLOTS);
		event_execute_eventbox_jump_reactive_tf.ModifyBg(Gtk.StateType.Normal, UtilGtk.BLUE_PLOTS);
		event_execute_eventbox_run_simple_time.ModifyBg(Gtk.StateType.Normal, UtilGtk.RED_PLOTS);
		event_execute_eventbox_run_simple_speed.ModifyBg(Gtk.StateType.Normal, UtilGtk.BLUE_PLOTS);
		event_execute_eventbox_run_interval_time.ModifyBg(Gtk.StateType.Normal, UtilGtk.RED_PLOTS);
		event_execute_eventbox_run_interval_speed.ModifyBg(Gtk.StateType.Normal, UtilGtk.BLUE_PLOTS);
		event_execute_eventbox_pulse_time.ModifyBg(Gtk.StateType.Normal, UtilGtk.BLUE_PLOTS); //only one serie in pulse, leave blue
		
		layout = new Pango.Layout (event_execute_drawingarea.PangoContext);
		layout.FontDescription = Pango.FontDescription.FromString ("Courier 7");
	
		eventHasEnded = false;
	
		if(videoOn)
			cameraRecordInitiate();
		
		ExecutingGraphData executingGraphData = new ExecutingGraphData(
				event_execute_button_cancel, event_execute_button_finish, 
				event_execute_label_message,  
				event_execute_label_event_value,  event_execute_label_time_value,
				event_execute_progressbar_event,  event_execute_progressbar_time);
		return executingGraphData;
	}
	
	private void eventExecutePutNonStandardIcons() {
		Pixbuf pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_bell_green.png");
		event_execute_image_jump_reactive_tf_good.Pixbuf = pixbuf;
		event_execute_image_jump_reactive_tc_good.Pixbuf = pixbuf;
		event_execute_image_jump_reactive_tf_tc_good.Pixbuf = pixbuf;
		event_execute_image_run_interval_time_good.Pixbuf = pixbuf;
		
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_bell_red.png");
		event_execute_image_jump_reactive_tf_bad.Pixbuf = pixbuf;
		event_execute_image_jump_reactive_tc_bad.Pixbuf = pixbuf;
		event_execute_image_jump_reactive_tf_tc_bad.Pixbuf = pixbuf;
		event_execute_image_run_interval_time_bad.Pixbuf = pixbuf;
	}

	private void eventExecuteHideImages() {
		event_execute_image_jump_reactive_tf_good.Hide();
		event_execute_image_jump_reactive_tf_bad.Hide();
		event_execute_image_jump_reactive_tc_good.Hide();
		event_execute_image_jump_reactive_tc_bad.Hide();
		event_execute_image_jump_reactive_tf_tc_good.Hide();
		event_execute_image_jump_reactive_tf_tc_bad.Hide();
		event_execute_image_run_interval_time_good.Hide();
		event_execute_image_run_interval_time_bad.Hide();
	}

	private void eventExecuteHideAllTables() {
		//hide simple jump info
		event_execute_hbox_jump_simple_titles.Hide();
		event_execute_table_jump_simple.Hide();
		event_execute_table_jump_simple_values.Hide();
		
		//hide reactive info
		event_execute_table_jump_reactive.Hide();
		event_execute_table_jump_reactive_values.Hide();
		
		//hide run simple info
		event_execute_hbox_run_simple_titles.Hide();
		event_execute_table_run_simple.Hide();
		event_execute_table_run_simple_values.Hide();
		
		//hide run interval info
		event_execute_table_run_interval.Hide();
		event_execute_table_run_interval_values.Hide();
		
		//hide pulse info
		event_execute_table_pulse.Hide();
		event_execute_table_pulse_values.Hide();
		
		//hide reaction time info
		event_execute_hbox_reaction_time_titles.Hide();
		event_execute_table_reaction_time.Hide();
		event_execute_table_reaction_time_values.Hide();
	}
	
	private void showJumpSimpleLabels() {
		//show simple jump info
		event_execute_hbox_jump_simple_titles.Show();
		event_execute_table_jump_simple.Show();
		event_execute_table_jump_simple_values.Show();

		//initializeLabels
		event_execute_label_jump_simple_tc_now.Text = "";
		event_execute_label_jump_simple_tc_person.Text = "";
		event_execute_label_jump_simple_tc_session.Text = "";
		event_execute_label_jump_simple_tf_now.Text = "";
		event_execute_label_jump_simple_tf_person.Text = "";
		event_execute_label_jump_simple_tf_session.Text = "";
		event_execute_label_jump_simple_height_now.Text = "";
		event_execute_label_jump_simple_height_person.Text = "";
		event_execute_label_jump_simple_height_session.Text = "";

		notebook_results_data.CurrentPage = 0;
	}
	
	
	private void showJumpReactiveLabels() {
		//show reactive info
		event_execute_table_jump_reactive.Show();
		event_execute_table_jump_reactive_values.Show();

		//initializeLabels
		event_execute_label_jump_reactive_tc_now.Text = "";
		event_execute_label_jump_reactive_tc_avg.Text = "";
		event_execute_label_jump_reactive_tf_now.Text = "";
		event_execute_label_jump_reactive_tf_avg.Text = "";
		event_execute_label_jump_reactive_tf_tc_now.Text = "";
		event_execute_label_jump_reactive_tf_tc_avg.Text = "";

		notebook_results_data.CurrentPage = 1;
	}
	
	private void showRunSimpleLabels() {
		//show run simple info
		event_execute_hbox_run_simple_titles.Show();
		event_execute_table_run_simple.Show();
		event_execute_table_run_simple_values.Show();
		
		//initializeLabels
		event_execute_label_run_simple_time_now.Text = "";
		event_execute_label_run_simple_time_person.Text = "";
		event_execute_label_run_simple_time_session.Text = "";
		event_execute_label_run_simple_speed_now.Text = "";
		event_execute_label_run_simple_speed_person.Text = "";
		event_execute_label_run_simple_speed_session.Text = "";

		notebook_results_data.CurrentPage = 2;
	}
		
	private void showRunIntervalLabels() {
		//show run interval info
		event_execute_table_run_interval.Show();
		event_execute_table_run_interval_values.Show();
		
		//initializeLabels
		event_execute_label_run_interval_time_now.Text = "";
		event_execute_label_run_interval_time_avg.Text = "";
		event_execute_label_run_interval_speed_now.Text = "";
		event_execute_label_run_interval_speed_avg.Text = "";

		notebook_results_data.CurrentPage = 3;
	}
	
	private void showReactionTimeLabels() {
		//show info
		event_execute_hbox_reaction_time_titles.Show();
		event_execute_table_reaction_time.Show();
		event_execute_table_reaction_time_values.Show();

		//initializeLabels
		event_execute_label_reaction_time_now.Text = "";
		event_execute_label_reaction_time_person.Text = "";
		event_execute_label_reaction_time_session.Text = "";

		notebook_results_data.CurrentPage = 4;
	}

	private void showPulseLabels() {
		//show pulse info
		event_execute_table_pulse.Show();
		event_execute_table_pulse_values.Show();

		//initializeLabels
		event_execute_label_pulse_now.Text = "";
		event_execute_label_pulse_avg.Text = "";

		notebook_results_data.CurrentPage = 5;
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
	
	//called for cleaning the graph of a event done before than the current
	private void event_execute_clearDrawingArea() 
	{
		if(event_execute_pixmap == null) 
			event_execute_pixmap = new Gdk.Pixmap (event_execute_drawingarea.GdkWindow, event_execute_drawingarea.Allocation.Width, event_execute_drawingarea.Allocation.Height, -1);
		
		event_execute_erasePaint(event_execute_drawingarea);
	}
	
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
	bool sizeChanged;
	public void on_event_execute_drawingarea_configure_event(object o, ConfigureEventArgs args)
	{
		Gdk.EventConfigure ev = args.Event;
		Gdk.Window window = ev.Window;

		Gdk.Rectangle allocation = event_execute_drawingarea.Allocation;
		
		if(event_execute_pixmap == null || sizeChanged || allocation.Width != allocationXOld) {
			event_execute_pixmap = new Gdk.Pixmap (window, allocation.Width, allocation.Height, -1);
		
			event_execute_erasePaint(event_execute_drawingarea);
			
			sizeChanged = false;
		}

		allocationXOld = allocation.Width;
	}
	
	public void on_event_execute_drawingarea_expose_event(object o, ExposeEventArgs args)
	{
		/* in some mono installations, configure_event is not called, but expose_event yes. 
		 * Do here the initialization
		 */
		
		Gdk.Rectangle allocation = event_execute_drawingarea.Allocation;
		if(event_execute_pixmap == null || sizeChanged || allocation.Width != allocationXOld) {
			event_execute_pixmap = new Gdk.Pixmap (event_execute_drawingarea.GdkWindow, allocation.Width, allocation.Height, -1);
			event_execute_erasePaint(event_execute_drawingarea);

			sizeChanged = false;
		}

		Gdk.Rectangle area = args.Event.Area;

		//sometimes this is called when pait is finished
		//don't let this erase win
		if(event_execute_pixmap != null) {
			args.Event.Window.DrawDrawable(event_execute_drawingarea.Style.WhiteGC, event_execute_pixmap,
				area.X, area.Y,
				area.X, area.Y,
				area.Width, area.Height);
		}
		
		allocationXOld = allocation.Width;
	}


	private void event_execute_erasePaint(Gtk.DrawingArea drawingarea) {
		event_execute_pixmap.DrawRectangle (drawingarea.Style.WhiteGC, true, 0, 0,
				drawingarea.Allocation.Width, drawingarea.Allocation.Height);
		
		// -- refresh
		drawingarea.QueueDraw();
	}

	CapturerBin capturer;
	//Gtk.Window capturerWindow;
	private void cameraRecordInitiate() 
	{
		capturer = new CapturerBin();
		CapturePropertiesStruct s = new CapturePropertiesStruct();

		s.OutputFile = Util.GetVideoTempFileName();

		s.VideoBitrate =  1000;
		s.CaptureSourceType = CaptureSourceType.Raw;
		s.Width = 360;
		s.Height = 288;

		capturer.CaptureProperties = s;
		capturer.Type = CapturerType.Live;
		capturer.Visible=true;
		
		//capturerWindow = new Gtk.Window("Capturer");
		//capturerWindow.Add(capturer);
		//capturerWindow.ShowAll();
		//capturerWindow.DeleteEvent += delegate(object sender, DeleteEventArgs e) {capturer.Close(); capturer.Dispose();};
		//hbox_capture.PackStart(capturer, true, true, 0);
		//hbox_capture.ShowAll();

		capturer.Run();
		capturer.ClickRec();
	}
	
	

	// simple and DJ jump	
	public void PrepareJumpSimpleGraph(double tv, double tc) 
	{
		//check graph properties window is not null (propably user has closed it with the DeleteEvent
		//then create it, but not show it
		if(eventGraphConfigureWin == null)
			eventGraphConfigureWin = EventGraphConfigureWindow.Show(false);

		
		//obtain data
		string []jumps = SqliteJump.SelectJumps(
				currentSession.UniqueID, event_execute_personID, "", event_execute_eventType);

		double tvPersonAVG = SqliteSession.SelectAVGEventsOfAType(
				currentSession.UniqueID, event_execute_personID, 
				event_execute_tableName, event_execute_eventType, "TV");
		double tvSessionAVG = SqliteSession.SelectAVGEventsOfAType(
				currentSession.UniqueID, -1, event_execute_tableName, event_execute_eventType, "TV");

		double tcPersonAVG = 0; 
		double tcSessionAVG = 0; 
		if(tc > 0) {
			tcPersonAVG = SqliteSession.SelectAVGEventsOfAType(
					currentSession.UniqueID, event_execute_personID, 
					event_execute_tableName, event_execute_eventType, "TC");
			tcSessionAVG = SqliteSession.SelectAVGEventsOfAType(
					currentSession.UniqueID, -1, event_execute_tableName, event_execute_eventType, "TC");
		}
		
		double maxValue = 0;
		double minValue = 0;
		int topMargin = 10; 
		int bottomMargin = 10; 

		//if max value of graph is automatic
		if(eventGraphConfigureWin.Max == -1) {
			maxValue = Util.GetMax(
					tv.ToString() + "=" + tvPersonAVG.ToString() + "=" + tvSessionAVG.ToString() + "=" +
					tc.ToString() + "=" + tcPersonAVG.ToString() + "=" + tcSessionAVG.ToString());
			foreach(string myStr in jumps) {
				string [] jump = myStr.Split(new char[] {':'});
				if(Convert.ToDouble(jump[5]) > maxValue)
					maxValue = Convert.ToDouble(jump[5]); //tf
				if(Convert.ToDouble(jump[6]) > maxValue)
					maxValue = Convert.ToDouble(jump[6]); //tc
			}
		} else {
			maxValue = eventGraphConfigureWin.Max;
			topMargin = 0;
		}
		
		//if min value of graph is automatic
		if(eventGraphConfigureWin.Min == -1) {
			string myString = tv.ToString() + "=" + tvPersonAVG.ToString() + "=" + tvSessionAVG.ToString();
			if(tc > 0)
				myString = myString + "=" + tc.ToString() + "=" + tcPersonAVG.ToString() + "=" + tcSessionAVG.ToString();
			minValue = Util.GetMin(myString);
			foreach(string myStr in jumps) {
				string [] jump = myStr.Split(new char[] {':'});
				if(Convert.ToDouble(jump[5]) < minValue)
					minValue = Convert.ToDouble(jump[5]); //tf
				if(Convert.ToDouble(jump[6]) < minValue)
					minValue = Convert.ToDouble(jump[6]); //tc
			}
		} else {
			minValue = eventGraphConfigureWin.Min;
			bottomMargin = 0;
		}
		
		//paint graph
		paintJumpSimple (event_execute_drawingarea, jumps, tv, tvPersonAVG, tvSessionAVG, 
				tc, tcPersonAVG, tcSessionAVG, maxValue, minValue, topMargin, bottomMargin);

		//printLabels
		printLabelsJumpSimple (tv, tvPersonAVG, tvSessionAVG, tc, tcPersonAVG, tcSessionAVG);
		
		// -- refresh
		event_execute_drawingarea.QueueDraw();
		
	}
	
	// Reactive jump 
	public void PrepareJumpReactiveGraph(double lastTv, double lastTc, string tvString, string tcString, 
			bool volumeOn, RepetitiveConditionsWindow repetitiveConditionsWin) {
		//check graph properties window is not null (propably user has closed it with the DeleteEvent
		//then create it, but not show it
		if(eventGraphConfigureWin == null)
			eventGraphConfigureWin = EventGraphConfigureWindow.Show(false);

Log.WriteLine("Preparing reactive A");

		//search MAX
		double maxValue = 0;
		int topMargin = 10;
		//if max value of graph is automatic
		if(eventGraphConfigureWin.Max == -1) 
			maxValue = Util.GetMax(tvString + "=" + tcString);
		else {
			maxValue = eventGraphConfigureWin.Max;
			topMargin = 0;
		}
	
		//search min
		double minValue = 1000;
		int bottomMargin = 10; 
		//if min value of graph is automatic
		if(eventGraphConfigureWin.Min == -1) 
			minValue = Util.GetMin(tvString + "=" + tcString);
		else {
			minValue = eventGraphConfigureWin.Min;
			bottomMargin = 10; 
		}		

		int jumps = Util.GetNumberOfJumps(tvString, true); 

		//paint graph
		paintJumpReactive (event_execute_drawingarea, lastTv, lastTc, tvString, tcString, Util.GetAverage(tvString), Util.GetAverage(tcString), 
				maxValue, minValue, jumps, topMargin, bottomMargin, 
				bestOrWorstTvTcIndex(true, tvString, tcString), bestOrWorstTvTcIndex(false, tvString, tcString), 
				volumeOn, repetitiveConditionsWin);
		
		// -- refresh
		event_execute_drawingarea.QueueDraw();
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
			

	// run simple
	public void PrepareRunSimpleGraph(double time, double speed) 
	{
		//check graph properties window is not null (propably user has closed it with the DeleteEvent
		//then create it, but not show it
		if(eventGraphConfigureWin == null)
			eventGraphConfigureWin = EventGraphConfigureWindow.Show(false);

		
		bool paintTime = false; //paint speed
		if(eventGraphConfigureWin.RunsTimeActive) 
			paintTime = true;
		
		//obtain data
		string [] runs = SqliteRun.SelectRuns(currentSession.UniqueID, event_execute_personID, event_execute_eventType);

		double timePersonAVG = SqliteSession.SelectAVGEventsOfAType(currentSession.UniqueID, event_execute_personID, event_execute_tableName, event_execute_eventType, "time");
		double timeSessionAVG = SqliteSession.SelectAVGEventsOfAType(currentSession.UniqueID, -1, event_execute_tableName, event_execute_eventType, "time");

		//double distancePersonAVG = SqliteSession.SelectAVGEventsOfAType(currentSession.UniqueID, event_execute_personID, event_execute_tableName, event_execute_eventType, "distance");
		//double distanceSessionAVG = SqliteSession.SelectAVGEventsOfAType(currentSession.UniqueID, -1, event_execute_tableName, event_execute_eventType, "distance");
		//better to know speed like:
		//SELECT AVG(distance/time) from run; than 
		//SELECT AVG(distance) / SELECT AVG(time) 
		//first is ok, because is the speed AVG
		//2nd is not good because it tries to do an AVG of all distances and times
		double speedPersonAVG = SqliteSession.SelectAVGEventsOfAType(currentSession.UniqueID, event_execute_personID, event_execute_tableName, event_execute_eventType, "distance/time");
		double speedSessionAVG = SqliteSession.SelectAVGEventsOfAType(currentSession.UniqueID, -1, event_execute_tableName, event_execute_eventType, "distance/time");

		double maxValue = 0;
		double minValue = 0;
		int topMargin = 10; 
		int bottomMargin = 10; 

		//if max value of graph is automatic
		if(eventGraphConfigureWin.Max == -1) {
			if(paintTime) {
				maxValue = Util.GetMax(time.ToString() + "=" + timePersonAVG.ToString() + "=" + timeSessionAVG.ToString());
				foreach(string myStr in runs) {
					string [] run = myStr.Split(new char[] {':'});
					if(Convert.ToDouble(run[6]) > maxValue)
						maxValue = Convert.ToDouble(run[6]); 
				}
			}
			else {						//paint speed
				maxValue = Util.GetMax(speed.ToString() + "=" + speedPersonAVG.ToString() + "=" + speedSessionAVG.ToString());
				foreach(string myStr in runs) {
					string [] run = myStr.Split(new char[] {':'});
					double mySpeed = Convert.ToDouble(Util.GetSpeed(run[5], run[6], true));
					if(mySpeed > maxValue)
						maxValue = mySpeed;
				}
			}
		} else {
			maxValue = eventGraphConfigureWin.Max;
			topMargin = 0;
		}
			
		//if min value of graph is automatic
		if(eventGraphConfigureWin.Min == -1) {
			if(paintTime) {
				minValue = Util.GetMin(time.ToString() + "=" + timePersonAVG.ToString() + "=" + timeSessionAVG.ToString());
				foreach(string myStr in runs) {
					string [] run = myStr.Split(new char[] {':'});
					if(Convert.ToDouble(run[6]) < minValue)
						minValue = Convert.ToDouble(run[6]); 
				}
			}
			else {
				minValue = Util.GetMin(speed.ToString() + "=" + speedPersonAVG.ToString() + "=" + speedSessionAVG.ToString());
				foreach(string myStr in runs) {
					string [] run = myStr.Split(new char[] {':'});
					double mySpeed = Convert.ToDouble(Util.GetSpeed(run[5], run[6], true));
					if(mySpeed < minValue)
						minValue = mySpeed;
				}
			}
		} else {
			minValue = eventGraphConfigureWin.Min;
			bottomMargin = 0;
		}
			
		
		//paint graph
		if(paintTime)
			paintRunSimple (event_execute_drawingarea, pen_rojo, runs, time, timePersonAVG, timeSessionAVG, maxValue, minValue, topMargin, bottomMargin);
		else						//paint speed
			paintRunSimple (event_execute_drawingarea, pen_azul_claro, runs, speed, speedPersonAVG, speedSessionAVG, maxValue, minValue, topMargin, bottomMargin);
		
		//printLabels
		printLabelsRunSimple (time, timePersonAVG, timeSessionAVG, speed, speedPersonAVG, speedSessionAVG);
		
		// -- refresh
		event_execute_drawingarea.QueueDraw();
	}
	
	// run interval
	// distanceTotal is passed because it can change in variable distances test
	public void PrepareRunIntervalGraph(double distance, double lastTime, string timesString, double distanceTotal, string distancesString,
			bool volumeOn, RepetitiveConditionsWindow repetitiveConditionsWin) {
		//check graph properties window is not null (propably user has closed it with the DeleteEvent
		//then create it, but not show it
		if(eventGraphConfigureWin == null)
			eventGraphConfigureWin = EventGraphConfigureWindow.Show(false);

		bool paintTime = false; //paint speed
		if(eventGraphConfigureWin.RunsTimeActive) 
			paintTime = true;

		//search MAX 
		double maxValue = 0;
		int topMargin = 10;
		//if max value of graph is automatic
		if(eventGraphConfigureWin.Max == -1) {
			if(paintTime)
				maxValue = Util.GetMax(timesString);
			else {
				if(distancesString == "")
					maxValue = distance / Util.GetMin(timesString); //getMin because is on the "denominador"
				else
					maxValue = Util.GetRunIVariableDistancesSpeeds(distancesString, timesString, true);
			}
		} else {
			maxValue = eventGraphConfigureWin.Max;
			topMargin = 0;
		}
			
		//search min
		double minValue = 1000;
		int bottomMargin = 10; 
		//if min value of graph is automatic
		if(eventGraphConfigureWin.Min == -1) { 
			if(paintTime)
				minValue = Util.GetMin(timesString);
			else {
				if(distancesString == "")
					minValue = distance / Util.GetMax(timesString); //getMax because is in the "denominador"
				else
					minValue = Util.GetRunIVariableDistancesSpeeds(distancesString, timesString, false);
			}
		} else {
			minValue = eventGraphConfigureWin.Min;
			bottomMargin = 10; 
		}		

		int tracks = Util.GetNumberOfJumps(timesString, true); 

		//paint graph
		paintRunInterval (event_execute_drawingarea, paintTime, distance, distanceTotal, distancesString,
				lastTime, timesString, Util.GetAverage(timesString), 
				maxValue, minValue, tracks, topMargin, bottomMargin,
				Util.GetPosMax(timesString), Util.GetPosMin(timesString),
				volumeOn, repetitiveConditionsWin);
		
		// -- refresh
		event_execute_drawingarea.QueueDraw();
	}


	// pulse 
	public void PreparePulseGraph(double lastTime, string timesString) { 
		//check graph properties window is not null (propably user has closed it with the DeleteEvent
		//then create it, but not show it
		if(eventGraphConfigureWin == null)
			eventGraphConfigureWin = EventGraphConfigureWindow.Show(false);

		//search MAX 
		double maxValue = 0;
		int topMargin = 10;
		//if max value of graph is automatic
		if(eventGraphConfigureWin.Max == -1) 
			maxValue = Util.GetMax(timesString);
		else {
			maxValue = eventGraphConfigureWin.Max;
			topMargin = 0;
		}
			
		//search MIN 
		double minValue = 1000;
		int bottomMargin = 10;
		//if min value of graph is automatic
		if(eventGraphConfigureWin.Min == -1) 
			minValue = Util.GetMin(timesString);
		else {
			minValue = eventGraphConfigureWin.Min;
			bottomMargin = 0;
		}
			
		int pulses = Util.GetNumberOfJumps(timesString, true); 

		//paint graph
		paintPulse (event_execute_drawingarea, lastTime, timesString, 
				Util.GetAverage(timesString), pulses, maxValue, minValue, topMargin, bottomMargin);
		
		// -- refresh
		event_execute_drawingarea.QueueDraw();
	}
	
	public void PrepareReactionTimeGraph(double time) 
	{
		//check graph properties window is not null (propably user has closed it with the DeleteEvent
		//then create it, but not show it
		if(eventGraphConfigureWin == null)
			eventGraphConfigureWin = EventGraphConfigureWindow.Show(false);

		
		//obtain data
		string [] rts = SqliteReactionTime.SelectReactionTimes(currentSession.UniqueID, event_execute_personID);

		double timePersonAVG = SqliteSession.SelectAVGEventsOfAType(
				currentSession.UniqueID, event_execute_personID, event_execute_tableName, event_execute_eventType, "time");
		double timeSessionAVG = SqliteSession.SelectAVGEventsOfAType(
				currentSession.UniqueID, -1, event_execute_tableName, event_execute_eventType, "time");

		double maxValue = 0;
		double minValue = 0;
		int topMargin = 10; 
		int bottomMargin = 10; 

		//if max value of graph is automatic
		if(eventGraphConfigureWin.Max == -1) {
			maxValue = Util.GetMax(
					time.ToString() + "=" + timePersonAVG.ToString() + "=" + timeSessionAVG.ToString());
		} else {
			maxValue = eventGraphConfigureWin.Max;
			topMargin = 0;
		}
		
		//if min value of graph is automatic
		if(eventGraphConfigureWin.Min == -1) {
			minValue = Util.GetMin(
					time.ToString() + "=" + timePersonAVG.ToString() + "=" + timeSessionAVG.ToString());
		} else {
			minValue = eventGraphConfigureWin.Min;
			bottomMargin = 0;
		}
		
		//paint graph (use simple jump method)
		paintJumpSimple (event_execute_drawingarea, rts, time, timePersonAVG, timeSessionAVG, 0, 0, 0, maxValue, minValue, topMargin, bottomMargin);

		printLabelsReactionTime (time, timePersonAVG, timeSessionAVG);
		
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
		int topMargin = 10;
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
		int bottomMargin = 10;
		//if min value of graph is automatic
		if(eventGraphConfigureWin.Min == -1) 
			minValue = 0;
		else {
			minValue = eventGraphConfigureWin.Min; //TODO
			bottomMargin = 0;
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
	

	private void printLabelsJumpSimple (double tvNow, double tvPerson, double tvSession, double tcNow, double tcPerson, double tcSession) {
		if(tcNow > 0) {
			event_execute_label_jump_simple_tc_now.Text = Util.TrimDecimals(tcNow.ToString(), prefsDigitsNumber);
			event_execute_label_jump_simple_tc_person.Text = Util.TrimDecimals(tcPerson.ToString(), prefsDigitsNumber);
			event_execute_label_jump_simple_tc_session.Text = Util.TrimDecimals(tcSession.ToString(), prefsDigitsNumber);
		} else {
			event_execute_label_jump_simple_tc_now.Text = "";
			event_execute_label_jump_simple_tc_person.Text = "";
			event_execute_label_jump_simple_tc_session.Text = "";
		}
		event_execute_label_jump_simple_tf_now.Text = Util.TrimDecimals(tvNow.ToString(), prefsDigitsNumber);
		event_execute_label_jump_simple_tf_person.Text = Util.TrimDecimals(tvPerson.ToString(), prefsDigitsNumber);
		event_execute_label_jump_simple_tf_session.Text = Util.TrimDecimals(tvSession.ToString(), prefsDigitsNumber);
		
		event_execute_label_jump_simple_height_now.Text = Util.TrimDecimals(
				Util.GetHeightInCentimeters(tvNow.ToString()) , prefsDigitsNumber);
		event_execute_label_jump_simple_height_person.Text = Util.TrimDecimals(
				Util.GetHeightInCentimeters(tvPerson.ToString()) , prefsDigitsNumber);
		event_execute_label_jump_simple_height_session.Text = Util.TrimDecimals(
				Util.GetHeightInCentimeters(tvSession.ToString()) , prefsDigitsNumber);
	}
	
	private void printLabelsRunSimple (double timeNow, double timePerson, double timeSession, double speedNow, double speedPerson, double speedSession) {
		event_execute_label_run_simple_time_now.Text = Util.TrimDecimals(timeNow.ToString(), prefsDigitsNumber);
		event_execute_label_run_simple_time_person.Text = Util.TrimDecimals(timePerson.ToString(), prefsDigitsNumber);
		event_execute_label_run_simple_time_session.Text = Util.TrimDecimals(timeSession.ToString(), prefsDigitsNumber);
		
		event_execute_label_run_simple_speed_now.Text = Util.TrimDecimals(speedNow.ToString(), prefsDigitsNumber);
		event_execute_label_run_simple_speed_person.Text = Util.TrimDecimals(speedPerson.ToString(), prefsDigitsNumber);
		event_execute_label_run_simple_speed_session.Text = Util.TrimDecimals(speedSession.ToString(), prefsDigitsNumber);
	}
	
	private void printLabelsReactionTime (double timeNow, double timePerson, double timeSession) {
		event_execute_label_reaction_time_now.Text = Util.TrimDecimals(timeNow.ToString(), prefsDigitsNumber);
		event_execute_label_reaction_time_person.Text = Util.TrimDecimals(timePerson.ToString(), prefsDigitsNumber);
		event_execute_label_reaction_time_session.Text = Util.TrimDecimals(timeSession.ToString(), prefsDigitsNumber);
	}
	

	private void paintJumpSimple (Gtk.DrawingArea drawingarea, string [] jumps, 
			double tvNow, double tvPerson, double tvSession, 
			double tcNow, double tcPerson, double tcSession,
			double maxValue, double minValue, int topMargin, int bottomMargin)
	{
		int ancho=drawingarea.Allocation.Width;
		int alto=drawingarea.Allocation.Height;
		
		event_execute_erasePaint(drawingarea);
		writeMarginsText(maxValue, minValue, alto);
		
		//check now here that we will have not division by zero problems
		if(maxValue - minValue > 0) {
			//calculate separation between series and bar width
			int tctfSep = 0; //separation between tc and tf
			int distanceBetweenCols = Convert.ToInt32((ancho-event_execute_rightMargin)*(1+.5)/jumps.Length) -
					Convert.ToInt32((ancho-event_execute_rightMargin)*(0+.5)/jumps.Length);
			if(tcNow > 0)
				tctfSep = Convert.ToInt32(.3*distanceBetweenCols);
			int barWidth = Convert.ToInt32(.3*distanceBetweenCols);
			int barDesplLeft = Convert.ToInt32(.5*barWidth);

			//paint first the average horizontal guides in order to be behind the bars
			if(tcNow > 0) {
				drawGuideOrAVG(pen_rojo, tcPerson, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
				drawGuideOrAVG(pen_rojo_discont, tcSession, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
			}
			if(tvNow > 0) {
				drawGuideOrAVG(pen_azul_claro, tvPerson, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
				drawGuideOrAVG(pen_azul_claro_discont, tvSession, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
			}

			//red for TC
			if(tcNow > 0) {
				int count = 0;
				foreach(string myStr in jumps) {
					string [] jump = myStr.Split(new char[] {':'});
					Rectangle rect = new Rectangle(
							Convert.ToInt32((ancho-event_execute_rightMargin)*(count+.5)/jumps.Length)-barDesplLeft, 
							calculatePaintHeight(Convert.ToDouble(jump[6]), alto, maxValue, minValue, 
								topMargin, bottomMargin),
							barWidth, alto
							);
					event_execute_pixmap.DrawRectangle(pen_rojo, true, rect);
					event_execute_pixmap.DrawRectangle(pen_negro, false, rect);
					count ++;
				}
			}
		
			//blue for TF
			//check it's not a take off
			if(tvNow > 0) {
				int count = 0;
				foreach(string myStr in jumps) {
					string [] jump = myStr.Split(new char[] {':'});
					//jump[5] is ok fo jump.tv and for reactionTime.time
					Rectangle rect = new Rectangle(
							Convert.ToInt32((ancho-event_execute_rightMargin)*(count+.5)/jumps.Length)-barDesplLeft +tctfSep, 
							calculatePaintHeight(Convert.ToDouble(jump[5]), alto, maxValue, minValue, 
								topMargin, bottomMargin),
							barWidth, alto
							);
					event_execute_pixmap.DrawRectangle(pen_azul_claro, true, rect);
					event_execute_pixmap.DrawRectangle(pen_negro, false, rect);
					count ++;
				}
		
				//write "last" to show last jump
				layout.SetMarkup(Catalog.GetString("Last"));
				event_execute_pixmap.DrawLayout (pen_gris, 
						Convert.ToInt32((ancho-event_execute_rightMargin)*(count-.5)/jumps.Length)-barDesplLeft + tctfSep, 
						0, layout);
			}
			
			//paint reference guide black and green if needed
			drawGuideOrAVG(pen_negro_discont, eventGraphConfigureWin.BlackGuide, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
			drawGuideOrAVG(pen_green_discont, eventGraphConfigureWin.GreenGuide, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
		}
	}

	private void paintRunSimple (Gtk.DrawingArea drawingarea, Gdk.GC myPen, string [] runs, 
			double now, double person, double session,
			double maxValue, double minValue, int topMargin, int bottomMargin)
	{
		int ancho=drawingarea.Allocation.Width;
		int alto=drawingarea.Allocation.Height;
		
		event_execute_erasePaint(drawingarea);
		writeMarginsText(maxValue, minValue, alto);
		
		//check now here that we will have not division by zero problems
		if(maxValue - minValue > 0) {
			//calculate bar width
			int distanceBetweenCols = Convert.ToInt32((ancho-event_execute_rightMargin)*(1+.5)/runs.Length) -
					Convert.ToInt32((ancho-event_execute_rightMargin)*(0+.5)/runs.Length);
			int barWidth = Convert.ToInt32(.3*distanceBetweenCols);
			int barDesplLeft = Convert.ToInt32(.5*barWidth);
			
			//paint reference guide black and green if needed
			drawGuideOrAVG(pen_negro_discont, eventGraphConfigureWin.BlackGuide, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
			drawGuideOrAVG(pen_green_discont, eventGraphConfigureWin.GreenGuide, alto, ancho, topMargin, bottomMargin, maxValue, minValue);

			//blue for TF
			if(now > 0) {
				//blue tf average discountinuos line	
				drawGuideOrAVG(pen_azul_claro, 	person, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
				drawGuideOrAVG(pen_azul_claro_discont, session, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
		
				int count = 0;
				foreach(string myStr in runs) {
					string [] run = myStr.Split(new char[] {':'});
					Rectangle rect = new Rectangle(
							Convert.ToInt32((ancho-event_execute_rightMargin)*(count+.5)/runs.Length)-barDesplLeft, 
							calculatePaintHeight(Convert.ToDouble(run[6]), alto, maxValue, minValue, 
								topMargin, bottomMargin),
							barWidth, alto
							);
					//TODO: do speed related
					event_execute_pixmap.DrawRectangle(pen_azul_claro, true, rect);
					event_execute_pixmap.DrawRectangle(pen_negro, false, rect);
					count ++;
				}
				
				//write "last" to show last jump
				layout.SetMarkup(Catalog.GetString("Last"));
				event_execute_pixmap.DrawLayout (pen_gris, 
						Convert.ToInt32((ancho-event_execute_rightMargin)*(count-.5)/runs.Length)-barDesplLeft, 
						0, layout);
			}
		}
	}

	
	private void paintJumpReactive (Gtk.DrawingArea drawingarea, double lastTv, double lastTc, string tvString, string tcString, 
			double avgTV, double avgTC, double maxValue, double minValue, int jumps, 
			int topMargin, int bottomMargin, int posMax, int posMin, bool volumeOn,
			RepetitiveConditionsWindow repetitiveConditionsWin)
	{
		//int topMargin = 10; 
		int ancho=drawingarea.Allocation.Width;
		int alto=drawingarea.Allocation.Height;

		
		event_execute_erasePaint(drawingarea);
		
		writeMarginsText(maxValue, minValue, alto);
		
		//check now here that we will have not division by zero problems
		if(maxValue - minValue > 0) {

			if(jumps > 1) {
				//blue tf average discountinuos line	
				drawGuideOrAVG(pen_azul_discont, avgTV, alto, ancho, topMargin, bottomMargin, maxValue, minValue);

				//red tc average discountinuos line	
				drawGuideOrAVG(pen_rojo_discont, avgTC, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
			}

			//paint reference guide black and green if needed
			drawGuideOrAVG(pen_negro_discont, eventGraphConfigureWin.BlackGuide, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
			drawGuideOrAVG(pen_green_discont, eventGraphConfigureWin.GreenGuide, alto, ancho, topMargin, bottomMargin, maxValue, minValue);

			
			//blue tf evolution	
			string [] myTVStringFull = tvString.Split(new char[] {'='});
			int count = 0;
			double oldValue = 0;
			double myTVDouble = 0;

			foreach (string myTV in myTVStringFull) {
				myTVDouble = Convert.ToDouble(myTV);
				if(myTVDouble < 0)
					myTVDouble = 0;

				if (count > 0) 
					event_execute_pixmap.DrawLine(pen_azul, //blue for TF
							Convert.ToInt32((ancho-event_execute_rightMargin)*(count-.5)/jumps), calculatePaintHeight(oldValue, alto, maxValue, minValue, topMargin, bottomMargin),
							Convert.ToInt32((ancho-event_execute_rightMargin)*(count+.5)/jumps), calculatePaintHeight(myTVDouble, alto, maxValue, minValue, topMargin, bottomMargin));

				//paint Y lines
				if(eventGraphConfigureWin.VerticalGrid) 
					event_execute_pixmap.DrawLine(pen_beige_discont, Convert.ToInt32((ancho - event_execute_rightMargin) *(count+.5)/jumps), topMargin, Convert.ToInt32((ancho - event_execute_rightMargin) *(count+.5)/jumps), alto-topMargin);

				oldValue = myTVDouble;
				count ++;
			}
			
			drawCircleAndWriteValue(pen_azul, myTVDouble, --count, jumps, ancho, alto, maxValue, minValue, topMargin, bottomMargin);

			//read tc evolution	
			string [] myTCStringFull = tcString.Split(new char[] {'='});
			count = 0;
			oldValue = 0;
			double myTCDouble = 0;

			foreach (string myTC in myTCStringFull) {
				myTCDouble = Convert.ToDouble(myTC);

				//if we are at second value (or more), and first was not a "-1"
				//-1 means here that first jump has not TC (started inside)
				if (count > 0 && oldValue != -1)  
					event_execute_pixmap.DrawLine(pen_rojo, //red for TC
							Convert.ToInt32((ancho-event_execute_rightMargin)*(count-.5)/jumps), calculatePaintHeight(oldValue, alto, maxValue, minValue, topMargin, bottomMargin),
							Convert.ToInt32((ancho-event_execute_rightMargin)*(count+.5)/jumps), calculatePaintHeight(myTCDouble, alto, maxValue, minValue, topMargin, bottomMargin));

				oldValue = myTCDouble;
				count ++;
			}
			
			drawCircleAndWriteValue(pen_rojo, myTCDouble, --count, jumps, ancho, alto, maxValue, minValue, topMargin, bottomMargin);
		

			//draw best tv/tc
			event_execute_pixmap.DrawLine(pen_brown_bold,
					Convert.ToInt32((ancho-event_execute_rightMargin)*(posMax+.5)/jumps), calculatePaintHeight(Convert.ToDouble(myTVStringFull[posMax]), alto, maxValue, minValue, topMargin, bottomMargin),
					Convert.ToInt32((ancho-event_execute_rightMargin)*(posMax+.5)/jumps), calculatePaintHeight(Convert.ToDouble(myTCStringFull[posMax]), alto, maxValue, minValue, topMargin, bottomMargin));
			//draw worst tv/tc
			event_execute_pixmap.DrawLine(pen_violet_bold,
					Convert.ToInt32((ancho-event_execute_rightMargin)*(posMin+.5)/jumps), calculatePaintHeight(Convert.ToDouble(myTVStringFull[posMin]), alto, maxValue, minValue, topMargin, bottomMargin),
					Convert.ToInt32((ancho-event_execute_rightMargin)*(posMin+.5)/jumps), calculatePaintHeight(Convert.ToDouble(myTCStringFull[posMin]), alto, maxValue, minValue, topMargin, bottomMargin));

			//bells & images
			event_execute_image_jump_reactive_tf_good.Hide();
			event_execute_image_jump_reactive_tf_bad.Hide();
			event_execute_image_jump_reactive_tc_good.Hide();
			event_execute_image_jump_reactive_tc_bad.Hide();
			event_execute_image_jump_reactive_tf_tc_good.Hide();
			event_execute_image_jump_reactive_tf_tc_bad.Hide();
			bool showTfGood = false;
			bool showTfBad = false;
			bool showTcGood = false;
			bool showTcBad = false;
			bool showTfTcGood = false;
			bool showTfTcBad = false;

			//sounds of best & worst
			if(count > 0) {
				if(repetitiveConditionsWin.TfTcBest && posMax == count)
					showTfTcGood = true;
				else if(repetitiveConditionsWin.TfTcWorst && posMin == count)
					showTfTcBad = true;
			}
				
			if(repetitiveConditionsWin.TfGreater && lastTv > repetitiveConditionsWin.TfGreaterValue) 
				showTfGood = true;
			if(repetitiveConditionsWin.TfLower && lastTv < repetitiveConditionsWin.TfLowerValue) 
				showTfBad = true;

			if(repetitiveConditionsWin.TcGreater && lastTc > repetitiveConditionsWin.TcGreaterValue) 
				showTcBad = true;
			if(repetitiveConditionsWin.TcLower && lastTc < repetitiveConditionsWin.TcLowerValue) 
				showTcGood = true;

			if(lastTc > 0 && repetitiveConditionsWin.TfTcGreater && lastTv/lastTc > repetitiveConditionsWin.TfTcGreaterValue) 
				showTfTcGood = true;
			if(lastTc > 0 && repetitiveConditionsWin.TfTcLower && lastTv/lastTc < repetitiveConditionsWin.TfTcLowerValue) 
				showTfTcGood = true;


			if(showTfGood || showTcGood || showTfTcGood)
				Util.PlaySound(Constants.SoundTypes.GOOD, volumeOn);
			if(showTfBad || showTcBad || showTfTcBad)
				Util.PlaySound(Constants.SoundTypes.BAD, volumeOn);

			if(showTfGood)
				event_execute_image_jump_reactive_tf_good.Show();
			if(showTfBad)
				event_execute_image_jump_reactive_tf_bad.Show();
			if(showTcGood)
				event_execute_image_jump_reactive_tc_good.Show();
			if(showTcBad)
				event_execute_image_jump_reactive_tc_bad.Show();
			if(showTfTcGood)
				event_execute_image_jump_reactive_tf_tc_good.Show();
			if(showTfTcBad)
				event_execute_image_jump_reactive_tf_tc_bad.Show();
		}

		/*
		 * these Log.writeLines are useful to don't "get the thread dead"
		 * without them , sometimes drawingarea is not painted
		 */
		event_execute_label_jump_reactive_tc_now.Text = Util.TrimDecimals(lastTc.ToString(), prefsDigitsNumber);
		event_execute_label_jump_reactive_tc_avg.Text = Util.TrimDecimals(avgTC.ToString(), prefsDigitsNumber);
		event_execute_label_jump_reactive_tf_now.Text = Util.TrimDecimals(lastTv.ToString(), prefsDigitsNumber);
		event_execute_label_jump_reactive_tf_avg.Text = Util.TrimDecimals(avgTV.ToString(), prefsDigitsNumber);
		if(lastTc > 0)
			event_execute_label_jump_reactive_tf_tc_now.Text = Util.TrimDecimals((lastTv/lastTc).ToString(), prefsDigitsNumber);
		else
			event_execute_label_jump_reactive_tf_tc_now.Text = "0";
		if(avgTC > 0)
			event_execute_label_jump_reactive_tf_tc_avg.Text = Util.TrimDecimals((avgTV/avgTC).ToString(), prefsDigitsNumber);
		else
			event_execute_label_jump_reactive_tf_tc_avg.Text = "0";
	}

	private void paintRunInterval (Gtk.DrawingArea drawingarea, bool paintTime, double distance, double distanceTotal, string distancesString, double lastTime, 
			string timesString, double avgTime, double maxValue, double minValue, int tracks, int topMargin, int bottomMargin, 
			int hightValuePosition, int lowValuePosition,
			bool volumeOn, RepetitiveConditionsWindow repetitiveConditionsWin)
	{
		//int topMargin = 10; 
		int ancho=drawingarea.Allocation.Width;
		int alto=drawingarea.Allocation.Height;
		
		
		event_execute_erasePaint(drawingarea);
		
		writeMarginsText(maxValue, minValue, alto);
		
		//check now here that we will have not division by zero problems
		if(maxValue - minValue > 0) {

			if(tracks > 1) {
				if(paintTime) 
					//red time average discountinuos line	
					drawGuideOrAVG(pen_rojo_discont, avgTime, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
				else 
					//blue speed average discountinuos line	
					drawGuideOrAVG(pen_azul_discont, distance/avgTime, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
			}

			//paint reference guide black and green if needed
			drawGuideOrAVG(pen_negro_discont, eventGraphConfigureWin.BlackGuide, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
			drawGuideOrAVG(pen_green_discont, eventGraphConfigureWin.GreenGuide, alto, ancho, topMargin, bottomMargin, maxValue, minValue);

			
			string [] myTimesStringFull = timesString.Split(new char[] {'='});
			int count = 0;
			double oldValue = 0;
			double myTimeDouble = 0;

			Gdk.GC myPen = pen_rojo; //default value
			double myValue = 0;

			foreach (string myTime in myTimesStringFull) {
				myTimeDouble = Convert.ToDouble(myTime);
				if(myTimeDouble < 0)
					myTimeDouble = 0;

				if(paintTime) {
					//red time evolution
					myPen = pen_rojo;
					myValue = myTimeDouble;
				} else {
					//blue speed evolution	
					myPen = pen_azul;

					//if distances are variable
					if(distancesString == "") 
						myValue = distance / myTimeDouble;
					else
						myValue = Util.GetRunIVariableDistancesStringRow(distancesString, count) / myTimeDouble;
				}

				if (count > 0) {
					event_execute_pixmap.DrawLine(myPen,
							Convert.ToInt32((ancho - event_execute_rightMargin) *(count-.5)/tracks), calculatePaintHeight(oldValue, alto, maxValue, minValue, topMargin, bottomMargin),
							Convert.ToInt32((ancho - event_execute_rightMargin) *(count+.5)/tracks), calculatePaintHeight(myValue, alto, maxValue, minValue, topMargin, bottomMargin));
				}
				
				//paint Y lines
				if(eventGraphConfigureWin.VerticalGrid) 
					event_execute_pixmap.DrawLine(pen_beige_discont, Convert.ToInt32((ancho - event_execute_rightMargin) *(count+.5)/tracks), topMargin, Convert.ToInt32((ancho - event_execute_rightMargin) *(count+.5)/tracks), alto-topMargin);

				oldValue = myValue;
				count ++;
			}
			
			drawCircleAndWriteValue(myPen, myValue, --count, tracks, ancho, alto, maxValue, minValue, topMargin, bottomMargin);
		

			//bells & images
			event_execute_image_run_interval_time_good.Hide();
			event_execute_image_run_interval_time_bad.Hide();
			bool showTimeGood = false;
			bool showTimeBad = false;
	
			//sounds of best & worst
			if(count > 0) {
				if(repetitiveConditionsWin.RunTimeBest && lowValuePosition == count) 
					showTimeGood = true;
				else if(repetitiveConditionsWin.RunTimeWorst && hightValuePosition == count) 
					showTimeBad = true;
			}

			if(repetitiveConditionsWin.RunTimeLower && lastTime < repetitiveConditionsWin.RunTimeLowerValue) 
				showTimeGood = true;
			if(repetitiveConditionsWin.RunTimeGreater && lastTime > repetitiveConditionsWin.RunTimeGreaterValue) 
				showTimeBad = true;

			if(showTimeGood) {
				Util.PlaySound(Constants.SoundTypes.GOOD, volumeOn);
				event_execute_image_run_interval_time_good.Show();
			}
			if(showTimeBad) {
				Util.PlaySound(Constants.SoundTypes.BAD, volumeOn);
				event_execute_image_run_interval_time_bad.Show();
			}
		}
		
		event_execute_label_run_interval_time_now.Text = Util.TrimDecimals(lastTime.ToString(), prefsDigitsNumber);
		event_execute_label_run_interval_time_avg.Text = Util.TrimDecimals(avgTime.ToString(), prefsDigitsNumber);
		event_execute_label_run_interval_speed_now.Text = Util.TrimDecimals((distance / lastTime).ToString(), prefsDigitsNumber);
		event_execute_label_run_interval_speed_avg.Text = Util.TrimDecimals((distanceTotal / Util.GetTotalTime(timesString)).ToString(), prefsDigitsNumber);
	}

	private void paintPulse (Gtk.DrawingArea drawingarea, double lastTime, string timesString, double avgTime, int pulses, 
			double maxValue, double minValue, int topMargin, int bottomMargin)
	{
		int ancho=drawingarea.Allocation.Width;
		int alto=drawingarea.Allocation.Height;
		
		
		event_execute_erasePaint(drawingarea);
		
		writeMarginsText(maxValue, minValue, alto);
		
		//check now here that we will have not division by zero problems
		if(maxValue - minValue > 0) {

			//blue time average discountinuos line	
			drawGuideOrAVG(pen_azul_discont, avgTime, alto, ancho, topMargin, bottomMargin, maxValue, minValue);

			//paint reference guide black and green if needed
			drawGuideOrAVG(pen_negro_discont, eventGraphConfigureWin.BlackGuide, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
			drawGuideOrAVG(pen_green_discont, eventGraphConfigureWin.GreenGuide, alto, ancho, topMargin, bottomMargin, maxValue, minValue);

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
		
			drawCircleAndWriteValue(pen_azul, myTimeDouble, --count, pulses, ancho, alto, maxValue, minValue, topMargin, bottomMargin);

		}
		
		event_execute_label_pulse_now.Text = Util.TrimDecimals(lastTime.ToString(), prefsDigitsNumber);
		event_execute_label_pulse_avg.Text = Util.TrimDecimals(avgTime.ToString(), prefsDigitsNumber);
	}

	double multiChronopicGetX(int ancho, double time, double timeOld, double timeTotal) {
		if(time < 0)
			time = 0;

		//Console.WriteLine("   timestamp {0}, ancho {1}, x {2}, timeold{3}, xOld{4}", 
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
		Console.WriteLine("total time: {0}", timeTotal);


		/*
		//TODO: done this now because we come here with only 1 string filled and this does problems
		if(timeTotal == 0) {
			event_execute_alignment.SetSizeRequest(700, 300);
			sizeChanged = true;
			return;
		}
		*/

		int ancho=drawingarea.Allocation.Width;

		event_execute_erasePaint(drawingarea);

		//writeMarginsText(maxValue, minValue, alto);
		writeCpNames();
		if(event_execute_eventType == Constants.RunAnalysisName)
			runAWritePlatformNames();


		//check now here that we will have not division by zero problems
		//if(maxValue - minValue <= 0) 
		//	return;

		Console.Write(" paint0 ");
		
		if(event_execute_eventType == Constants.RunAnalysisName)
			paintMultiChronopicRunAPhotocell (ancho, cp1StartedIn, cp1InStr, cp1OutStr, yCp1Out +10, yCp1Out);
		else
			paintMultiChronopicDefault (ancho, cp1StartedIn, cp1InStr, cp1OutStr, timeTotal, yCp1Out +10, yCp1Out);

		Console.Write(" paint1 ");
		paintMultiChronopicDefault (ancho, cp2StartedIn, cp2InStr, cp2OutStr, timeTotal, yCp2Out +10, yCp2Out);
		Console.Write(" paint2 ");
		paintMultiChronopicDefault (ancho, cp3StartedIn, cp3InStr, cp3OutStr, timeTotal, yCp3Out +10, yCp3Out);
		Console.Write(" paint3 ");
		paintMultiChronopicDefault (ancho, cp4StartedIn, cp4InStr, cp4OutStr, timeTotal, yCp4Out +10, yCp4Out);
		Console.Write(" paint4 ");

		Console.Write(" paint done ");
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

		Console.WriteLine("\n(A) cpInStr:*{0}*, cpOutStr:*{1}*", cpInStr, cpOutStr);

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
		Console.WriteLine("(C)");
		if(timeOld < timeTotal) { //this cp didn't received last event
			if(lastCpIsStart)
				event_execute_pixmap.DrawLine(penStartDiscont, Convert.ToInt32(xOld), heightStart, Convert.ToInt32(ancho-event_execute_rightMargin), heightStart);
			else
				event_execute_pixmap.DrawLine(penEndDiscont, Convert.ToInt32(xOld), heightEnd, Convert.ToInt32(ancho-event_execute_rightMargin), heightEnd);
		}
		Console.WriteLine("(D)");
	}

	private void paintMultiChronopicRunAPhotocell (int ancho, bool cpStartedIn, string cpInStr, string cpOutStr, int h1, int h2) 
	{
		/*
		if(Util.GetTotalTime(cpInStr + "=" + cpOutStr) == 0) 
			return;
		
		string [] cpIn = cpInStr.Split(new char[] {'='});
		string [] cpOut = cpOutStr.Split(new char[] {'='});
		if(cpOut.Length == 1) {
			layout.SetMarkup("at first");
			event_execute_pixmap.DrawLayout (pen_gris, 50, yCp1Out, layout);
		}
		if(cpIn.Length == 1) {
			layout.SetMarkup("middle");
			event_execute_pixmap.DrawLayout (pen_gris, 70, yCp1Out, layout);
		}
		if(cpOut.Length == 2) {
			layout.SetMarkup("arrived");
			event_execute_pixmap.DrawLayout (pen_gris, 200, yCp1Out, layout);
		}
		*/
	}


	private void drawCircleAndWriteValue (Gdk.GC myPen, double myValue, int count, int total, int ancho, int alto, 
			double maxValue, double minValue, int topMargin, int bottomMargin) {

		//write text
		layout.SetMarkup((Math.Round(myValue,3)).ToString());
		event_execute_pixmap.DrawLayout (myPen, ancho -event_execute_rightMargin, (int)calculatePaintHeight(myValue, alto, maxValue, minValue, topMargin, bottomMargin) -7, layout); //-7 for aligning (is baseline) (font is Courier 7)

		if(eventGraphConfigureWin.PaintCircle) {
			//put circle in last value
			event_execute_pixmap.DrawArc(myPen, true, Convert.ToInt32((ancho - event_execute_rightMargin) *(count+.5)/total) - event_execute_radio/2 + event_execute_arcSystemCorrection, calculatePaintHeight(myValue, alto, maxValue, minValue, topMargin, bottomMargin) - event_execute_radio/2, event_execute_radio, event_execute_radio, 0, 360*64);
		}
	}

		
	private void drawGuideOrAVG(Gdk.GC myPen, double guideHeight, int alto, int ancho, int topMargin, int bottomMargin, double maxValue, double minValue) 
	{
		if(guideHeight == -1)
			return; //return if checkbox guide is not checked
		else {
			event_execute_pixmap.DrawLine(myPen, 
					0, calculatePaintHeight(guideHeight, alto, maxValue, minValue, topMargin, bottomMargin),
					ancho - event_execute_rightMargin-2, calculatePaintHeight(guideHeight, alto, maxValue, minValue, topMargin, bottomMargin));
			//write textual data
			layout.SetMarkup((Math.Round(guideHeight,3)).ToString());
			event_execute_pixmap.DrawLayout (pen_gris, ancho -event_execute_rightMargin, (int)calculatePaintHeight(guideHeight, alto, maxValue, minValue, topMargin, bottomMargin) -7, layout); //-7 for aligning with Courier 7 font baseline
		}
	}

	//this calculates the Y of every point in the graph
	//the first "alto -" is because the graph comes from down to up, and we have to reverse
	private int calculatePaintHeight(double currentValue, int alto, double maxValue, double minValue, int topMargin, int bottomMargin) {
		return Convert.ToInt32(alto - bottomMargin - ((currentValue - minValue) * (alto - topMargin - bottomMargin) / (maxValue - minValue)));
	}

	private void writeMarginsText(double maxValue, double minValue, int alto) {
		//write margins textual data
		layout.SetMarkup((Math.Round(maxValue, 3)).ToString());
		event_execute_pixmap.DrawLayout (pen_gris, 0, 0, layout);
		//event_execute_pixmap.DrawLayout (pen_gris, 0, 3, layout); //y to 3 (not 0) probably this solves rando Pango problems where this is not written and interface gets "clumsy"
		layout.SetMarkup((Math.Round(minValue, 3)).ToString());
		event_execute_pixmap.DrawLayout (pen_gris, 0, alto -10, layout); //don't search Y using alto - bottomMargin, because bottomMargin can be 0, 
									//and text goes down from the baseline, and will not be seen
	}
		
	private void hideButtons() {
		event_execute_button_cancel.Sensitive = false;
		event_execute_button_finish.Sensitive = false;
	}

	private void writeCpNames() {
		layout.SetMarkup("cp1");
		event_execute_pixmap.DrawLayout (pen_gris, 0, yCp1Out -20, layout);
		layout.SetMarkup("cp2");
		event_execute_pixmap.DrawLayout (pen_gris, 0, yCp2Out -20, layout);
		layout.SetMarkup("cp3");
		event_execute_pixmap.DrawLayout (pen_gris, 0, yCp3Out -20, layout);
		layout.SetMarkup("cp4");
		event_execute_pixmap.DrawLayout (pen_gris, 0, yCp4Out -20, layout);
	}
	
	private void runAWritePlatformNames() {
		layout.SetMarkup(Catalog.GetString("Photocells"));
		event_execute_pixmap.DrawLayout (pen_gris, 20, yCp1Out -20, layout);
		layout.SetMarkup(Catalog.GetString("Platforms"));
		event_execute_pixmap.DrawLayout (pen_gris, 20, yCp2Out -20, layout);
	}

	private void on_event_execute_update_graph_in_progress_clicked(object o, EventArgs args) {
		switch (currentEventType.Type) {
			case EventType.Types.JUMP:
				if(thisJumpIsSimple) {
					PrepareJumpSimpleGraph(
							currentEventExecute.PrepareEventGraphJumpSimpleObject.tv, 
							currentEventExecute.PrepareEventGraphJumpSimpleObject.tc);
				} else {
					PrepareJumpReactiveGraph(
							currentEventExecute.PrepareEventGraphJumpReactiveObject.lastTv, 
							currentEventExecute.PrepareEventGraphJumpReactiveObject.lastTc,
							currentEventExecute.PrepareEventGraphJumpReactiveObject.tvString,
							currentEventExecute.PrepareEventGraphJumpReactiveObject.tcString,
							volumeOn, repetitiveConditionsWin);
				}
				break;
			case EventType.Types.RUN:
				if(thisRunIsSimple) {
					PrepareRunSimpleGraph(
							currentEventExecute.PrepareEventGraphRunSimpleObject.time,
							currentEventExecute.PrepareEventGraphRunSimpleObject.speed);
				} else {
					bool volumeOnHere = true;
					//do not play good or bad sounds at RSA because we need to hear the GO sound
					if(currentRunIntervalType.IsRSA)
						volumeOnHere = false;

					PrepareRunIntervalGraph(
							currentEventExecute.PrepareEventGraphRunIntervalObject.distance, 
							currentEventExecute.PrepareEventGraphRunIntervalObject.lastTime,
							currentEventExecute.PrepareEventGraphRunIntervalObject.timesString,
							currentEventExecute.PrepareEventGraphRunIntervalObject.distanceTotal,
							currentEventExecute.PrepareEventGraphRunIntervalObject.distancesString,
							volumeOnHere, repetitiveConditionsWin);
				}
				break;
			case EventType.Types.REACTIONTIME:
					PrepareReactionTimeGraph(
							currentEventExecute.PrepareEventGraphReactionTimeObject.time);
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
	
	private void on_event_execute_EventEnded(object o, EventArgs args) {
		hideButtons();
		eventHasEnded = true;
	
		if(videoOn) {	
			capturer.Stop();
			capturer.Close();
			capturer.Dispose();
		}
	}
	
	
	void on_event_execute_button_properties_clicked (object o, EventArgs args) {
		//now show the eventGraphConfigureWin
		eventGraphConfigureWin = EventGraphConfigureWindow.Show(true);
	}

	void on_event_execute_button_update_clicked (object o, EventArgs args) 
	{
		//event will be raised, and managed in chronojump.cs
		//see ButtonUpdate at end of class
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
	Gdk.GC pen_azul_claro; //tf, also speed and pulse; jump avg personTv. This for bars
	Gdk.GC pen_azul; //tf, also speed and pulse; jump avg personTv. This for lines
	Gdk.GC pen_rojo_discont; //avg tc in reactive; jump avg sessionTc 
	Gdk.GC pen_azul_claro_discont; //avg tf in reactive; jump avg sessionTv
	Gdk.GC pen_azul_discont; //avg tf in reactive; jump avg sessionTv
	Gdk.GC pen_negro; //borders of rectangle
	Gdk.GC pen_negro_discont; //guide
	Gdk.GC pen_green_discont; //guide
	Gdk.GC pen_gris; //textual data
	Gdk.GC pen_beige_discont; //Y cols
	Gdk.GC pen_brown_bold; //best tv/tc in rj;
	Gdk.GC pen_violet_bold; //worst tv/tc in rj
	//Gdk.GC pen_blanco;
	

	void event_execute_configureColors()
	{
		//Gdk.Color rojo = new Gdk.Color(0xff,0,0);
		//Gdk.Color azul  = new Gdk.Color(0,0,0xff);
		//Gdk.Color rojo = new Gdk.Color(238,0,0);
		//Gdk.Color azul  = new Gdk.Color(178,223,238);
		Gdk.Color negro = new Gdk.Color(0,0,0); 
		Gdk.Color green = new Gdk.Color(0,0xff,0);
		Gdk.Color gris = new Gdk.Color(0x66,0x66,0x66);
		Gdk.Color beige = new Gdk.Color(0x99,0x99,0x99);
		Gdk.Color brown = new Gdk.Color(0xd6,0x88,0x33);
		Gdk.Color violet = new Gdk.Color(0xc4,0x20,0xf3);
		//Gdk.Color blanco = new Gdk.Color(0xff,0xff,0xff);

		Gdk.Colormap colormap = Gdk.Colormap.System;
		colormap.AllocColor (ref UtilGtk.RED_PLOTS, true, true);
		colormap.AllocColor (ref UtilGtk.BLUE_PLOTS,true,true);
		colormap.AllocColor (ref UtilGtk.LIGHT_BLUE_PLOTS,true,true);
		colormap.AllocColor (ref negro,true,true);
		colormap.AllocColor (ref green,true,true);
		colormap.AllocColor (ref gris,true,true);
		colormap.AllocColor (ref beige,true,true);
		colormap.AllocColor (ref brown,true,true);
		colormap.AllocColor (ref violet,true,true);
		//colormap.AllocColor (ref blanco,true,true);

		//-- Configurar los contextos graficos (pinceles)
		pen_rojo = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_azul_claro = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_azul = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_rojo_discont = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_azul_claro_discont = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_azul_discont = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		//pen_negro = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		//pen_blanco= new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_negro = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_negro_discont = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_green_discont = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_gris = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_beige_discont = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_brown_bold = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_violet_bold = new Gdk.GC(event_execute_drawingarea.GdkWindow);

		
		pen_rojo.Foreground = UtilGtk.RED_PLOTS;
		pen_azul_claro.Foreground = UtilGtk.LIGHT_BLUE_PLOTS;
		pen_azul.Foreground = UtilGtk.BLUE_PLOTS;
		
		pen_rojo_discont.Foreground = UtilGtk.RED_PLOTS;
		pen_azul_claro_discont.Foreground = UtilGtk.LIGHT_BLUE_PLOTS;
		pen_azul_discont.Foreground = UtilGtk.BLUE_PLOTS;
		pen_rojo_discont.SetLineAttributes(1, Gdk.LineStyle.OnOffDash, Gdk.CapStyle.Butt, Gdk.JoinStyle.Round);
		pen_azul_claro_discont.SetLineAttributes(1, Gdk.LineStyle.OnOffDash, Gdk.CapStyle.Butt, Gdk.JoinStyle.Round);
		pen_azul_discont.SetLineAttributes(1, Gdk.LineStyle.OnOffDash, Gdk.CapStyle.Butt, Gdk.JoinStyle.Round);
		
		pen_negro.Foreground = negro;

		pen_negro_discont.Foreground = negro;
		pen_negro_discont.SetLineAttributes(1, Gdk.LineStyle.OnOffDash, Gdk.CapStyle.Butt, Gdk.JoinStyle.Round);
		pen_green_discont.Foreground = green;
		pen_green_discont.SetLineAttributes(1, Gdk.LineStyle.OnOffDash, Gdk.CapStyle.Butt, Gdk.JoinStyle.Round);
		
		pen_gris.Foreground = gris;

		pen_beige_discont.Foreground = beige;
		pen_beige_discont.SetLineAttributes(1, Gdk.LineStyle.OnOffDash, Gdk.CapStyle.Butt, Gdk.JoinStyle.Round);
		
		pen_brown_bold.Foreground = brown;
		pen_brown_bold.SetLineAttributes(2, Gdk.LineStyle.Solid, Gdk.CapStyle.Butt, Gdk.JoinStyle.Round);
		pen_violet_bold.Foreground = violet;
		pen_violet_bold.SetLineAttributes(2, Gdk.LineStyle.Solid, Gdk.CapStyle.Butt, Gdk.JoinStyle.Round);
	}
	void on_event_execute_button_cancel_clicked (object o, EventArgs args)
	{
		hideButtons();
		
		if(videoOn) {
			capturer.Stop();
			capturer.Close();
			capturer.Dispose();
			//it will lbe recorded on temp, but chronojump will not move it to chronojump/multimedia folders
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

	public Button event_execute_ButtonUpdate 
	{
		get { return event_execute_button_update; }
	}
	
}
