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
 * Copyright (C) 2004-2016   Xavier de Blas <xaviblas@gmail.com> 
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
	
	[Widget] Gtk.Table event_execute_table_jump_reactive;
	[Widget] Gtk.Table event_execute_table_run_interval;
	[Widget] Gtk.Table event_execute_table_pulse;
	
	[Widget] Gtk.Table event_execute_table_jump_reactive_values;
	[Widget] Gtk.Table event_execute_table_run_interval_values;
	[Widget] Gtk.Table event_execute_table_pulse_values;
	
	[Widget] Gtk.HBox hbox_results_legend;

	//for the color change in the background of the cell label
	//[Widget] Gtk.EventBox event_execute_eventbox_jump_reactive_height;
	[Widget] Gtk.EventBox event_execute_eventbox_jump_reactive_tc;
	[Widget] Gtk.EventBox event_execute_eventbox_jump_reactive_tf;
	//[Widget] Gtk.EventBox event_execute_eventbox_jump_reactive_tf_tc;
	[Widget] Gtk.EventBox event_execute_eventbox_run_interval_time;
	[Widget] Gtk.EventBox event_execute_eventbox_run_interval_speed;
	[Widget] Gtk.EventBox event_execute_eventbox_pulse_time;

	[Widget] Gtk.Label event_execute_label_jump_reactive_height_now;
	[Widget] Gtk.Label event_execute_label_jump_reactive_height_avg;
	[Widget] Gtk.Label event_execute_label_jump_reactive_tf_now;
	[Widget] Gtk.Label event_execute_label_jump_reactive_tf_avg;
	[Widget] Gtk.Label event_execute_label_jump_reactive_tc_now;
	[Widget] Gtk.Label event_execute_label_jump_reactive_tc_avg;
	[Widget] Gtk.Label event_execute_label_jump_reactive_tf_tc_now;
	[Widget] Gtk.Label event_execute_label_jump_reactive_tf_tc_avg;

	[Widget] Gtk.Label event_execute_label_run_interval_time_now;
	[Widget] Gtk.Label event_execute_label_run_interval_time_avg;
	[Widget] Gtk.Label event_execute_label_run_interval_speed_now;
	[Widget] Gtk.Label event_execute_label_run_interval_speed_avg;
	
	[Widget] Gtk.Label event_execute_label_pulse_now;
	[Widget] Gtk.Label event_execute_label_pulse_avg;

	[Widget] Gtk.Image event_execute_image_jump_reactive_height_good;
	[Widget] Gtk.Image event_execute_image_jump_reactive_height_bad;
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
	

	string event_execute_label_simulated;
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
	int event_execute_rightMargin = 35; 	//at the right we write text (on windows we change later)

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
	Pango.Layout layoutSmall;
	Pango.Layout layoutMid;
	Pango.Layout layoutBig;

	static EventGraphConfigureWindow eventGraphConfigureWin;
	
	private static string [] comboGraphResultsSize = {
		"100", "200", "300", "400", "500"
	};
	

	ExecutingGraphData event_execute_initializeVariables (
			bool simulated,
			int personID,
			string personName,
			string phasesName, 
			string tableName,
			string event_execute_eventType
			) 
	{
		eventExecuteHideAllTables();
		eventExecuteHideImages();
		
		if(UtilAll.IsWindows()) {
			event_execute_rightMargin = 55;
			event_execute_arcSystemCorrection = 1;
		}

		event_execute_configureColors();
	
		event_execute_label_simulated = "";
		if(simulated) 
			event_execute_label_simulated = Catalog.GetString("Simulated");

		event_graph_label_graph_person.Text = "<b>" + personName + "</b>";
		event_graph_label_graph_person.UseMarkup = true; 
		event_graph_label_graph_test.Text = " - " + event_execute_eventType;
				
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

		UtilGtk.ClearDrawingArea(event_execute_drawingarea, event_execute_pixmap);

		clearProgressBars();

	
		//event_execute_eventbox_jump_reactive_height.ModifyBg(Gtk.StateType.Normal, UtilGtk.RED_PLOTS);
		event_execute_eventbox_jump_reactive_tc.ModifyBg(Gtk.StateType.Normal, UtilGtk.RED_PLOTS);
		event_execute_eventbox_jump_reactive_tf.ModifyBg(Gtk.StateType.Normal, UtilGtk.BLUE_PLOTS);
		event_execute_eventbox_run_interval_time.ModifyBg(Gtk.StateType.Normal, UtilGtk.RED_PLOTS);
		event_execute_eventbox_run_interval_speed.ModifyBg(Gtk.StateType.Normal, UtilGtk.BLUE_PLOTS);
		event_execute_eventbox_pulse_time.ModifyBg(Gtk.StateType.Normal, UtilGtk.BLUE_PLOTS); //only one serie in pulse, leave blue
		
		layoutSmall = new Pango.Layout (event_execute_drawingarea.PangoContext);
		layoutSmall.FontDescription = Pango.FontDescription.FromString ("Courier 7");
		
		layoutMid = new Pango.Layout (event_execute_drawingarea.PangoContext);
		layoutMid.FontDescription = Pango.FontDescription.FromString ("Courier 11");

		layoutBig = new Pango.Layout (event_execute_drawingarea.PangoContext);
		layoutBig.FontDescription = Pango.FontDescription.FromString ("Courier 14");
		//layoutBig.Alignment = Pango.Alignment.Center; //doesn't work, see GetPixelSize below
	
		eventHasEnded = false;

		checkbutton_video.Sensitive = false;
		if(preferences.videoOn) {
			capturer.ClickRec();
			//label_video_feedback.Text = Catalog.GetString("Recording");
			label_video_feedback.Text = "Rec.";
		}

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
		event_execute_image_jump_reactive_height_good.Pixbuf = pixbuf;
		event_execute_image_jump_reactive_tf_good.Pixbuf = pixbuf;
		event_execute_image_jump_reactive_tc_good.Pixbuf = pixbuf;
		event_execute_image_jump_reactive_tf_tc_good.Pixbuf = pixbuf;
		event_execute_image_run_interval_time_good.Pixbuf = pixbuf;
		
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_bell_red.png");
		event_execute_image_jump_reactive_height_bad.Pixbuf = pixbuf;
		event_execute_image_jump_reactive_tf_bad.Pixbuf = pixbuf;
		event_execute_image_jump_reactive_tc_bad.Pixbuf = pixbuf;
		event_execute_image_jump_reactive_tf_tc_bad.Pixbuf = pixbuf;
		event_execute_image_run_interval_time_bad.Pixbuf = pixbuf;
	}

	private void eventExecuteHideImages() {
		event_execute_image_jump_reactive_height_good.Hide();
		event_execute_image_jump_reactive_height_bad.Hide();
		event_execute_image_jump_reactive_tf_good.Hide();
		event_execute_image_jump_reactive_tf_bad.Hide();
		event_execute_image_jump_reactive_tc_good.Hide();
		event_execute_image_jump_reactive_tc_bad.Hide();
		event_execute_image_jump_reactive_tf_tc_good.Hide();
		event_execute_image_jump_reactive_tf_tc_bad.Hide();
		event_execute_image_run_interval_time_good.Hide();
		event_execute_image_run_interval_time_bad.Hide();
	}

	private void eventExecuteHideAllTables() 
	{
		//hide reactive info
		event_execute_table_jump_reactive.Hide();
		event_execute_table_jump_reactive_values.Hide();
		
		//hide run interval info
		event_execute_table_run_interval.Hide();
		event_execute_table_run_interval_values.Hide();
		
		//hide pulse info
		event_execute_table_pulse.Hide();
		event_execute_table_pulse_values.Hide();
	}
	
	private void showJumpSimpleLabels() 
	{
		hbox_results_legend.Visible = true;
		notebook_results_data.Visible = false;
	}
	
	
	private void showJumpReactiveLabels() 
	{
		hbox_results_legend.Visible = false;

		//show reactive info
		event_execute_table_jump_reactive.Show();
		event_execute_table_jump_reactive_values.Show();

		//initializeLabels
		event_execute_label_jump_reactive_height_now.Text = "";
		event_execute_label_jump_reactive_height_avg.Text = "";
		event_execute_label_jump_reactive_tf_now.Text = "";
		event_execute_label_jump_reactive_tf_avg.Text = "";
		event_execute_label_jump_reactive_tc_now.Text = "";
		event_execute_label_jump_reactive_tc_avg.Text = "";
		event_execute_label_jump_reactive_tf_tc_now.Text = "";
		event_execute_label_jump_reactive_tf_tc_avg.Text = "";

		notebook_results_data.Visible = true;
		notebook_results_data.CurrentPage = 0;
	}
	
	private void showRunSimpleLabels() 
	{
		hbox_results_legend.Visible = true;
		notebook_results_data.Visible = false;
	}
		
	private void showRunIntervalLabels() 
	{
		hbox_results_legend.Visible = false;

		//show run interval info
		event_execute_table_run_interval.Show();
		event_execute_table_run_interval_values.Show();
		
		//initializeLabels
		event_execute_label_run_interval_time_now.Text = "";
		event_execute_label_run_interval_time_avg.Text = "";
		event_execute_label_run_interval_speed_now.Text = "";
		event_execute_label_run_interval_speed_avg.Text = "";

		notebook_results_data.Visible = true;
		notebook_results_data.CurrentPage = 1;
	}
	
	private void showReactionTimeLabels() 
	{
		hbox_results_legend.Visible = true;
		notebook_results_data.Visible = false;
	}

	private void showPulseLabels() 
	{
		hbox_results_legend.Visible = false;

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
	bool sizeChanged;
	public void on_event_execute_drawingarea_configure_event(object o, ConfigureEventArgs args)
	{
		Gdk.EventConfigure ev = args.Event;
		Gdk.Window window = ev.Window;

		Gdk.Rectangle allocation = event_execute_drawingarea.Allocation;
		
		if(event_execute_pixmap == null || sizeChanged || allocation.Width != allocationXOld) {
			event_execute_pixmap = new Gdk.Pixmap (window, allocation.Width, allocation.Height, -1);
		
			UtilGtk.ErasePaint(event_execute_drawingarea, event_execute_pixmap);
			
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
			UtilGtk.ErasePaint(event_execute_drawingarea, event_execute_pixmap);

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

	

	// simple and DJ jump	
	public void PrepareJumpSimpleGraph(PrepareEventGraphJumpSimple eventGraph)
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
		if(eventGraphConfigureWin.Max == -1) {
			maxValue = eventGraph.sessionMAXAtSQL;

			//fix if there's a max tc that's higher than max tv
			foreach(string myStr in eventGraph.jumpsAtSQL) {
				string [] jump = myStr.Split(new char[] {':'});
				if(Convert.ToDouble(jump[6]) > maxValue)
					maxValue = Convert.ToDouble(jump[6]);
			}
		} else {
			maxValue = eventGraphConfigureWin.Max;
			topMargin = 0;
		}
		
		//if min value of graph is automatic
		/*
		if(eventGraphConfigureWin.Min == -1) {
			string myString = eventGraph.tv.ToString() + "=" + 
				eventGraph.tvPersonAVGAtSQL.ToString() + "=" + eventGraph.tvSessionAVGAtSQL.ToString();
			if(eventGraph.tc > 0)
				myString = myString + "=" + eventGraph.tc.ToString() + "=" + 
					eventGraph.tcPersonAVGAtSQL.ToString() + "=" + eventGraph.tcSessionAVGAtSQL.ToString();
			minValue = Util.GetMin(myString);
			foreach(string myStr in eventGraph.jumpsAtSQL) {
				string [] jump = myStr.Split(new char[] {':'});
				if(Convert.ToDouble(jump[5]) < minValue)
					minValue = Convert.ToDouble(jump[5]); //tf
				if(Convert.ToDouble(jump[6]) < minValue)
					minValue = Convert.ToDouble(jump[6]); //tc
			}
		} else {
		*/
			minValue = eventGraphConfigureWin.Min;
		//}
		
		//paint graph
		paintJumpSimple (event_execute_drawingarea, eventGraph, 
			       	maxValue, minValue, topMargin, bottomMargin);

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

		LogB.Debug("Preparing reactive A");

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
		int bottomMargin = 0; 
		//if min value of graph is automatic
		if(eventGraphConfigureWin.Min == -1) 
			minValue = Util.GetMin(tvString + "=" + tcString);
		else {
			minValue = eventGraphConfigureWin.Min;
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
	public void PrepareRunSimpleGraph(PrepareEventGraphRunSimple eventGraph)
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
		/*
		if(eventGraphConfigureWin.Min == -1) {
			minValue = Util.GetMin(eventGraph.speed.ToString() + "=" + 
					eventGraph.speedPersonAVGAtSQL.ToString() + "=" + eventGraph.speedSessionAVGAtSQL.ToString());
			foreach(string myStr in eventGraph.runsAtSQL) {
				string [] run = myStr.Split(new char[] {':'});
				double mySpeed = Convert.ToDouble(Util.GetSpeed(run[5], run[6], true));
				if(mySpeed < minValue)
					minValue = mySpeed;
			}
		} else {
		*/
			minValue = eventGraphConfigureWin.Min;
		//}
			
			paintRunSimple (event_execute_drawingarea, eventGraph,
					maxValue, minValue, topMargin, bottomMargin);
		
		
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

		//search MAX 
		double maxValue = 0;
		int topMargin = 20;
		//if max value of graph is automatic
		if(eventGraphConfigureWin.Max == -1) {
			if(distancesString == "")
				maxValue = distance / Util.GetMin(timesString); //getMin because is on the "denominador"
			else
				maxValue = Util.GetRunIVariableDistancesSpeeds(distancesString, timesString, true);
		} else {
			maxValue = eventGraphConfigureWin.Max;
			topMargin = 0;
		}
			
		//search min
		double minValue = 1000;
		int bottomMargin = 0; 
		//if min value of graph is automatic
		if(eventGraphConfigureWin.Min == -1) { 
			if(distancesString == "")
				minValue = distance / Util.GetMax(timesString); //getMax because is in the "denominador"
			else
				minValue = Util.GetRunIVariableDistancesSpeeds(distancesString, timesString, false);
		} else {
			minValue = eventGraphConfigureWin.Min;
		}		

		int tracks = Util.GetNumberOfJumps(timesString, true); 

		//paint graph
		paintRunInterval (event_execute_drawingarea, distance, distanceTotal, distancesString,
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
	
	public void PrepareReactionTimeGraph(PrepareEventGraphReactionTime eventGraph) 
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
				maxValue, minValue, topMargin, bottomMargin);

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
	//used on jumps reactive, runs interval
	private void plotSimulatedMessageIfNeededAtCenter(int ancho, int alto) {
		if(event_execute_label_simulated != "") {
			layoutBig.SetMarkup(event_execute_label_simulated);
			int lWidth = 1;
			int lHeight = 1;
			layoutBig.GetPixelSize(out lWidth, out lHeight); 
			event_execute_pixmap.DrawLayout (pen_black, 
					Convert.ToInt32(ancho/2 - lWidth/2), 
					Convert.ToInt32(alto/2 - lHeight/2), 
					layoutBig);
		}
	}
	
	private void paintJumpSimple (Gtk.DrawingArea drawingarea, PrepareEventGraphJumpSimple eventGraph, 
			double maxValue, double minValue, int topMargin, int bottomMargin)
	{
		int ancho=drawingarea.Allocation.Width;
		int alto=drawingarea.Allocation.Height;
		
		UtilGtk.ErasePaint(event_execute_drawingarea, event_execute_pixmap);
		//writeMarginsText(eventGraph.sessionMAXAtSQL, minValue, alto);

		//check now here that we will have not division by zero problems
		if(maxValue - minValue <= 0)
			return;

		//calculate separation between series and bar width
		int tctfSep = 0; //separation between tc and tf
		int distanceBetweenCols = Convert.ToInt32((ancho-event_execute_rightMargin)*(1+.5)/eventGraph.jumpsAtSQL.Length) -
			Convert.ToInt32((ancho-event_execute_rightMargin)*(0+.5)/eventGraph.jumpsAtSQL.Length);
		if(eventGraph.tc > 0)
			tctfSep = Convert.ToInt32(.3*distanceBetweenCols);
		int barWidth = Convert.ToInt32(.3*distanceBetweenCols);
		int barDesplLeft = Convert.ToInt32(.5*barWidth);

		//paint first the average horizontal guides in order to be behind the bars
		drawGuideOrAVG(pen_black_90, eventGraph.sessionMAXAtSQL, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
		drawGuideOrAVG(pen_black_discont, eventGraph.sessionAVGAtSQL, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
		
		drawGuideOrAVG(pen_yellow, eventGraph.personMAXAtSQL, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
		drawGuideOrAVG(pen_yellow_discont, eventGraph.personAVGAtSQL, alto, ancho, topMargin, bottomMargin, maxValue, minValue);

		bool animate;
		int x = 0;
		int y = 0;
		int count = eventGraph.jumpsAtSQL.Length;
		foreach(string myStr in eventGraph.jumpsAtSQL) 
		{
			string [] jump = myStr.Split(new char[] {':'});
		
			if(eventGraph.tc > 0) {
				//small layout when tc and tv and there are more than 4 jumps
				Pango.Layout layout = layoutMid;
				if(eventGraph.tv > 0 && eventGraph.jumpsAtSQL.Length > 4)
					layout = layoutSmall;
				
				//do not animate last tc, if tv is animated because then tc is not shown
				animate = true;
				if(eventGraph.tv >0)
					animate = false;

				x = Convert.ToInt32((ancho-event_execute_rightMargin)*(count-.5)/eventGraph.jumpsAtSQL.Length)-barDesplLeft;
				y = calculatePaintHeight(Convert.ToDouble(jump[6]), alto, maxValue, minValue, topMargin, bottomMargin);
				
				drawBar(x, y, barWidth, alto, pen_rojo, count == eventGraph.jumpsAtSQL.Length,
						jump[11] == "-1", Convert.ToDouble(jump[6]), layout, animate);
			
				if(eventGraph.tv > 0) {
					x = Convert.ToInt32((ancho-event_execute_rightMargin)*(count-.5)/eventGraph.jumpsAtSQL.Length)-barDesplLeft +tctfSep;
					y = calculatePaintHeight(Convert.ToDouble(jump[5]), alto, maxValue, minValue, topMargin, bottomMargin);
					
					drawBar(x, y, barWidth, alto, pen_azul_claro, count == eventGraph.jumpsAtSQL.Length,
							jump[11] == "-1", Convert.ToDouble(jump[5]), layout, true);
				}
			} else { //if only tv show height
				x = Convert.ToInt32((ancho-event_execute_rightMargin)*(count-.5)/eventGraph.jumpsAtSQL.Length)-barDesplLeft +tctfSep;
				y = calculatePaintHeight(Convert.ToDouble(Util.GetHeightInCentimeters(jump[5])), 
						alto, maxValue, minValue, topMargin, bottomMargin);

				drawBar(x, y, barWidth, alto, pen_azul_claro, count == eventGraph.jumpsAtSQL.Length,
						jump[11] == "-1", Convert.ToDouble(Util.GetHeightInCentimeters(jump[5])), layoutMid, true);
			}
			count --;
		}

		if(eventGraph.tc > 0)
			addUnitsToLabel("s");
		else
			addUnitsToLabel("cm");

		//paint reference guide black and green if needed
		//drawGuideOrAVG(pen_black_discont, eventGraphConfigureWin.BlackGuide, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
		//drawGuideOrAVG(pen_green_discont, eventGraphConfigureWin.GreenGuide, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
	}

	MovingBar movingBar;

	//TODO: if last tc and tf have to be painted, tc is not painted
	private void drawBar(int x, int y, int barWidth, int alto, Gdk.GC pen_bar_bg, 
			bool isLast, bool simulated, double result, Pango.Layout layout, bool animate)
	{
		if(isLast && animate) {
			timerBar = true;
			movingBar = new MovingBar(x, y + alto, barWidth, y, alto, pen_bar_bg, simulated, result, layout);
			GLib.Timeout.Add(1, new GLib.TimeoutHandler(OnTimerBar));
		}
		else {
			Rectangle rect = new Rectangle(x, y, barWidth, alto);
			event_execute_pixmap.DrawRectangle(pen_bar_bg, true, rect);
			event_execute_pixmap.DrawRectangle(pen_black, false, rect);
			
			if(simulated)
				plotSimulatedMessage(x + barWidth/2, alto, layout);

			plotResultOnBar(x + barWidth/2, y, alto, result, layout);
		}
	}

	bool timerBar = true;
	bool OnTimerBar() 
	{ 
		if (!timerBar) 
			return false;
		
		movingBar.Next();
		Rectangle rect = new Rectangle(movingBar.X, movingBar.Y, movingBar.Width, movingBar.Step);
		event_execute_pixmap.DrawRectangle(movingBar.Pen_bar_bg, true, rect);
		event_execute_drawingarea.QueueDrawArea(movingBar.X, movingBar.Y, movingBar.Width, movingBar.Step);
		
		if(movingBar.Y <= movingBar.YTop) {
			rect = new Rectangle(movingBar.X, movingBar.YTop, movingBar.Width, movingBar.AltoTop);
			event_execute_pixmap.DrawRectangle(pen_black, false, rect);
			
			if(movingBar.Simulated)
				plotSimulatedMessage(movingBar.X + movingBar.Width/2, movingBar.AltoTop, movingBar.Layout);

			plotResultOnBar(movingBar.X + movingBar.Width/2, movingBar.YTop, movingBar.AltoTop, movingBar.Result, movingBar.Layout);
			
			event_execute_drawingarea.QueueDraw();

			timerBar = false;
			return false;
		}

		return true;
	}      
	private void plotSimulatedMessage(int x, int alto, Pango.Layout layout) {
		layout.SetMarkup(event_execute_label_simulated);
		int lWidth = 1;
		int lHeight = 1;
		layout.GetPixelSize(out lWidth, out lHeight); 
		event_execute_pixmap.DrawLayout (pen_black, 
				Convert.ToInt32(x - lWidth/2), 
				alto - lHeight, 
				layout);
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
		int yStart = Convert.ToInt32((y+alto)/2);

		if( (yStart + 2*lHeight) > alto )
			yStart = alto - 2*lHeight;

		//draw rectangle behind
		Rectangle rect = new Rectangle(x - lWidth/2, yStart, lWidth, lHeight);
		event_execute_pixmap.DrawRectangle(pen_yellow_bg, true, rect);
		
		//write text
		event_execute_pixmap.DrawLayout (pen_black, 
				Convert.ToInt32(x - lWidth/2),
				yStart,
				layout);
	}

	private void addUnitsToLabel(string unit) {
		event_graph_label_graph_test.Text = event_graph_label_graph_test.Text + " (" + unit + ")";
	}

	private void paintRunSimple (Gtk.DrawingArea drawingarea, PrepareEventGraphRunSimple eventGraph,
			double maxValue, double minValue, int topMargin, int bottomMargin)
	{
		int ancho=drawingarea.Allocation.Width;
		int alto=drawingarea.Allocation.Height;
		
		UtilGtk.ErasePaint(event_execute_drawingarea, event_execute_pixmap);
		//writeMarginsText(maxValue, minValue, alto);
		
		//check now here that we will have not division by zero problems
		if(maxValue - minValue <= 0)
			return;
		
		//calculate bar width
		int distanceBetweenCols = Convert.ToInt32((ancho-event_execute_rightMargin)*(1+.5)/eventGraph.runsAtSQL.Length) -
			Convert.ToInt32((ancho-event_execute_rightMargin)*(0+.5)/eventGraph.runsAtSQL.Length);
		int barWidth = Convert.ToInt32(.3*distanceBetweenCols);
		int barDesplLeft = Convert.ToInt32(.5*barWidth);

		/*
		//paint reference guide black and green if needed
		drawGuideOrAVG(pen_black_discont, eventGraphConfigureWin.BlackGuide, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
		drawGuideOrAVG(pen_green_discont, eventGraphConfigureWin.GreenGuide, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
		*/

		//blue for TF
			
		drawGuideOrAVG(pen_black_90, eventGraph.sessionMAXAtSQL, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
		drawGuideOrAVG(pen_black_discont, eventGraph.sessionAVGAtSQL, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
		drawGuideOrAVG(pen_yellow, eventGraph.personMAXAtSQL, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
		drawGuideOrAVG(pen_yellow_discont, eventGraph.personAVGAtSQL, alto, ancho, topMargin, bottomMargin, maxValue, minValue);

		int x = 0;
		int y = 0;
		int count = eventGraph.runsAtSQL.Length;
		foreach(string myStr in eventGraph.runsAtSQL) {
			string [] run = myStr.Split(new char[] {':'});
			if(Convert.ToDouble(run[5]) > 0 && Convert.ToDouble(run[6]) > 0) {
				x = Convert.ToInt32((ancho-event_execute_rightMargin)*(count-.5)/eventGraph.runsAtSQL.Length)-barDesplLeft;
				y = calculatePaintHeight(Convert.ToDouble(run[5])/Convert.ToDouble(run[6]), alto, maxValue, minValue, 
						topMargin, bottomMargin);

				drawBar(x, y, barWidth, alto, pen_azul_claro, count == eventGraph.runsAtSQL.Length,
						run[8] == "-1", Convert.ToDouble(run[5])/Convert.ToDouble(run[6]), layoutMid, true);
			}

			count --;
		}
			
		addUnitsToLabel("m/s");
	}
	
	private void paintReactionTime (Gtk.DrawingArea drawingarea, PrepareEventGraphReactionTime eventGraph,
			double maxValue, double minValue, int topMargin, int bottomMargin)
	{
		int ancho=drawingarea.Allocation.Width;
		int alto=drawingarea.Allocation.Height;
		
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

		drawGuideOrAVG(pen_black_90, eventGraph.sessionMINAtSQL, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
		drawGuideOrAVG(pen_black_discont, eventGraph.sessionAVGAtSQL, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
		drawGuideOrAVG(pen_yellow, eventGraph.personMINAtSQL, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
		drawGuideOrAVG(pen_yellow_discont, eventGraph.personAVGAtSQL, alto, ancho, topMargin, bottomMargin, maxValue, minValue);

		int x = 0;
		int y = 0;
		int count = eventGraph.rtsAtSQL.Length;
		foreach(string myStr in eventGraph.rtsAtSQL) {
			string [] rts = myStr.Split(new char[] {':'});
			x = Convert.ToInt32((ancho-event_execute_rightMargin)*(count-.5)/eventGraph.rtsAtSQL.Length)-barDesplLeft;
			y = calculatePaintHeight(Convert.ToDouble(rts[5]), alto, maxValue, minValue, 
					topMargin, bottomMargin);

			drawBar(x, y, barWidth, alto, pen_azul_claro, count == eventGraph.rtsAtSQL.Length,
					rts[7] == "-1", Convert.ToDouble(rts[5]), layoutMid, true);

			count --;
		}
			
		addUnitsToLabel("s");
	}

	
	private void paintJumpReactive (Gtk.DrawingArea drawingarea, double lastTv, double lastTc, string tvString, string tcString, 
			double avgTV, double avgTC, double maxValue, double minValue, int jumps, 
			int topMargin, int bottomMargin, int posMax, int posMin, bool volumeOn,
			RepetitiveConditionsWindow repetitiveConditionsWin)
	{
		//int topMargin = 10; 
		int ancho=drawingarea.Allocation.Width;
		int alto=drawingarea.Allocation.Height;

		double lastHeight = Convert.ToDouble(Util.GetHeightInCentimeters(lastTv.ToString()));
		
		UtilGtk.ErasePaint(event_execute_drawingarea, event_execute_pixmap);
		
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
			drawGuideOrAVG(pen_black_discont, eventGraphConfigureWin.BlackGuide, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
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

			
			plotSimulatedMessageIfNeededAtCenter(ancho, alto);
			
			//bells & images
			event_execute_image_jump_reactive_height_good.Hide();
			event_execute_image_jump_reactive_height_bad.Hide();
			event_execute_image_jump_reactive_tf_good.Hide();
			event_execute_image_jump_reactive_tf_bad.Hide();
			event_execute_image_jump_reactive_tc_good.Hide();
			event_execute_image_jump_reactive_tc_bad.Hide();
			event_execute_image_jump_reactive_tf_tc_good.Hide();
			event_execute_image_jump_reactive_tf_tc_bad.Hide();
			bool showHeightGood = false;
			bool showHeightBad = false;
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
				
			if(repetitiveConditionsWin.HeightGreater && lastHeight > repetitiveConditionsWin.HeightGreaterValue) 
				showHeightGood = true;
			if(repetitiveConditionsWin.HeightLower && lastHeight < repetitiveConditionsWin.HeightLowerValue) 
				showHeightBad = true;

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


			if(showHeightGood || showTfGood || showTcGood || showTfTcGood)
				Util.PlaySound(Constants.SoundTypes.GOOD, volumeOn);
			if(showHeightBad || showTfBad || showTcBad || showTfTcBad)
				Util.PlaySound(Constants.SoundTypes.BAD, volumeOn);

			if(showHeightGood)
				event_execute_image_jump_reactive_height_good.Show();
			if(showHeightBad)
				event_execute_image_jump_reactive_height_bad.Show();
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

		//height
		event_execute_label_jump_reactive_height_now.Text = "<b>" + Util.TrimDecimals(lastHeight.ToString(), preferences.digitsNumber) + "</b>";
		event_execute_label_jump_reactive_height_now.UseMarkup = true; 
		event_execute_label_jump_reactive_height_avg.Text = Util.TrimDecimals(
				Util.GetHeightInCentimeters(avgTV.ToString()), preferences.digitsNumber);
		
		//TV
		event_execute_label_jump_reactive_tf_now.Text = "<b>" + Util.TrimDecimals(lastTv.ToString(), preferences.digitsNumber) + "</b>";
		event_execute_label_jump_reactive_tf_now.UseMarkup = true; 
		event_execute_label_jump_reactive_tf_avg.Text = Util.TrimDecimals(avgTV.ToString(), preferences.digitsNumber);
		
		//TC
		event_execute_label_jump_reactive_tc_now.Text = "<b>" + Util.TrimDecimals(lastTc.ToString(), preferences.digitsNumber) + "</b>";
		event_execute_label_jump_reactive_tc_now.UseMarkup = true; 
		event_execute_label_jump_reactive_tc_avg.Text = Util.TrimDecimals(avgTC.ToString(), preferences.digitsNumber);

		//TV / TC
		if(lastTc > 0) {
			event_execute_label_jump_reactive_tf_tc_now.Text = "<b>" + Util.TrimDecimals((lastTv/lastTc).ToString(), preferences.digitsNumber) + "</b>";
			event_execute_label_jump_reactive_tf_tc_now.UseMarkup = true; 
		} else
			event_execute_label_jump_reactive_tf_tc_now.Text = "0";
		if(avgTC > 0)
			event_execute_label_jump_reactive_tf_tc_avg.Text = Util.TrimDecimals((avgTV/avgTC).ToString(), preferences.digitsNumber);
		else
			event_execute_label_jump_reactive_tf_tc_avg.Text = "0";
	}

	private void paintRunInterval (Gtk.DrawingArea drawingarea, double distance, double distanceTotal, string distancesString, double lastTime, 
			string timesString, double avgTime, double maxValue, double minValue, int tracks, int topMargin, int bottomMargin, 
			int hightValuePosition, int lowValuePosition,
			bool volumeOn, RepetitiveConditionsWindow repetitiveConditionsWin)
	{
		//int topMargin = 10; 
		int ancho=drawingarea.Allocation.Width;
		int alto=drawingarea.Allocation.Height;
		
		
		UtilGtk.ErasePaint(event_execute_drawingarea, event_execute_pixmap);
		
		writeMarginsText(maxValue, minValue, alto);
		
		//check now here that we will have not division by zero problems
		if(maxValue - minValue > 0) {

			if(tracks > 1) {
				//blue speed average discountinuos line	
				drawGuideOrAVG(pen_azul_discont, distance/avgTime, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
			}

			//paint reference guide black and green if needed
			drawGuideOrAVG(pen_black_discont, eventGraphConfigureWin.BlackGuide, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
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

				//blue speed evolution	
				myPen = pen_azul;

				//if distances are variable
				if(distancesString == "") 
					myValue = distance / myTimeDouble;
				else
					myValue = Util.GetRunIVariableDistancesStringRow(distancesString, count) / myTimeDouble;

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
			
			plotSimulatedMessageIfNeededAtCenter(ancho, alto);
		}
		
		event_execute_label_run_interval_time_now.Text = "<b>" + Util.TrimDecimals(lastTime.ToString(), preferences.digitsNumber) + "</b>";
		event_execute_label_run_interval_time_now.UseMarkup = true; 

		event_execute_label_run_interval_time_avg.Text = Util.TrimDecimals(avgTime.ToString(), preferences.digitsNumber);
		
		event_execute_label_run_interval_speed_now.Text = "<b>" + Util.TrimDecimals((distance / lastTime).ToString(), preferences.digitsNumber) + "</b>";
		event_execute_label_run_interval_speed_now.UseMarkup = true; 
		
		event_execute_label_run_interval_speed_avg.Text = Util.TrimDecimals((distanceTotal / Util.GetTotalTime(timesString)).ToString(), preferences.digitsNumber);
	}

	private void paintPulse (Gtk.DrawingArea drawingarea, double lastTime, string timesString, double avgTime, int pulses, 
			double maxValue, double minValue, int topMargin, int bottomMargin)
	{
		int ancho=drawingarea.Allocation.Width;
		int alto=drawingarea.Allocation.Height;
		
		
		UtilGtk.ErasePaint(event_execute_drawingarea, event_execute_pixmap);
		
		writeMarginsText(maxValue, minValue, alto);
		
		//check now here that we will have not division by zero problems
		if(maxValue - minValue > 0) {

			//blue time average discountinuos line	
			drawGuideOrAVG(pen_azul_discont, avgTime, alto, ancho, topMargin, bottomMargin, maxValue, minValue);

			//paint reference guide black and green if needed
			drawGuideOrAVG(pen_black_discont, eventGraphConfigureWin.BlackGuide, alto, ancho, topMargin, bottomMargin, maxValue, minValue);
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


	private void drawCircleAndWriteValue (Gdk.GC myPen, double myValue, int count, int total, int ancho, int alto, 
			double maxValue, double minValue, int topMargin, int bottomMargin) {

		//write text
		layoutSmall.SetMarkup((Math.Round(myValue,3)).ToString());
		event_execute_pixmap.DrawLayout (myPen, ancho -event_execute_rightMargin, (int)calculatePaintHeight(myValue, alto, maxValue, minValue, topMargin, bottomMargin) -7, layoutSmall); //-7 for aligning (is baseline) (font is Courier 7)

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
					10, calculatePaintHeight(guideHeight, alto, maxValue, minValue, topMargin, bottomMargin),
					ancho - event_execute_rightMargin-2, calculatePaintHeight(guideHeight, alto, maxValue, minValue, topMargin, bottomMargin));
			//write textual data
			layoutSmall.SetMarkup((Math.Round(guideHeight,1)).ToString());
			event_execute_pixmap.DrawLayout (pen_gris, ancho -event_execute_rightMargin, (int)calculatePaintHeight(guideHeight, alto, maxValue, minValue, topMargin, bottomMargin) -7, layoutSmall); //-7 for aligning with Courier 7 font baseline
		}
	}

	//this calculates the Y of every point in the graph
	//the first "alto -" is because the graph comes from down to up, and we have to reverse
	private int calculatePaintHeight(double currentValue, int alto, double maxValue, double minValue, int topMargin, int bottomMargin) {
		return Convert.ToInt32(alto - bottomMargin - ((currentValue - minValue) * (alto - topMargin - bottomMargin) / (maxValue - minValue)));
	}

	private void writeMarginsText(double maxValue, double minValue, int alto) {
		//write margins textual data
		layoutSmall.SetMarkup((Math.Round(maxValue, 3)).ToString());
		event_execute_pixmap.DrawLayout (pen_gris, 0, 0, layoutSmall);
		//event_execute_pixmap.DrawLayout (pen_gris, 0, 3, layoutSmall); //y to 3 (not 0) probably this solves rando Pango problems where this is not written and interface gets "clumsy"
		layoutSmall.SetMarkup((Math.Round(minValue, 3)).ToString());
		event_execute_pixmap.DrawLayout (pen_gris, 0, alto -10, layoutSmall); //don't search Y using alto - bottomMargin, because bottomMargin can be 0, 
									//and text goes down from the baseline, and will not be seen
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

	private void on_event_execute_update_graph_in_progress_clicked(object o, EventArgs args) {
		switch (currentEventType.Type) {
			case EventType.Types.JUMP:
				if(thisJumpIsSimple) 
					PrepareJumpSimpleGraph(currentEventExecute.PrepareEventGraphJumpSimpleObject);
				else {
					PrepareJumpReactiveGraph(
							currentEventExecute.PrepareEventGraphJumpReactiveObject.lastTv, 
							currentEventExecute.PrepareEventGraphJumpReactiveObject.lastTc,
							currentEventExecute.PrepareEventGraphJumpReactiveObject.tvString,
							currentEventExecute.PrepareEventGraphJumpReactiveObject.tcString,
							preferences.volumeOn, repetitiveConditionsWin);
				}
				break;
			case EventType.Types.RUN:
				if(thisRunIsSimple)
					PrepareRunSimpleGraph(currentEventExecute.PrepareEventGraphRunSimpleObject);
				else {
					bool volumeOnHere = preferences.volumeOn;
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
					PrepareReactionTimeGraph(currentEventExecute.PrepareEventGraphReactionTimeObject);
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

		checkbutton_video.Sensitive = true;
		if(preferences.videoOn) {	
			label_video_feedback.Text = "";
			capturer.ClickStop();
			videoCapturePrepare(false); //if error, show message
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
	Gdk.GC pen_black; //borders of rectangle
	Gdk.GC pen_black_90; //max value on the top
	Gdk.GC pen_yellow; //person max
	Gdk.GC pen_yellow_discont; //person avg
	Gdk.GC pen_yellow_bg; //below person result bar
	Gdk.GC pen_black_discont; //guide
	Gdk.GC pen_black_bars; //big borders of rectangle (last event)
	Gdk.GC pen_green_discont; //guide
	Gdk.GC pen_gris; //textual data
	Gdk.GC pen_beige_discont; //Y cols
	Gdk.GC pen_brown_bold; //best tv/tc in rj;
	Gdk.GC pen_violet_bold; //worst tv/tc in rj
	//Gdk.GC pen_white;
	

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
		Gdk.Color green = new Gdk.Color(0,0xff,0);
		Gdk.Color gris = new Gdk.Color(0x66,0x66,0x66);
		Gdk.Color beige = new Gdk.Color(0x99,0x99,0x99);
		Gdk.Color brown = new Gdk.Color(0xd6,0x88,0x33);
		Gdk.Color violet = new Gdk.Color(0xc4,0x20,0xf3);
		//Gdk.Color white = new Gdk.Color(0xff,0xff,0xff);

		Gdk.Colormap colormap = Gdk.Colormap.System;
		colormap.AllocColor (ref UtilGtk.RED_PLOTS, true, true);
		colormap.AllocColor (ref UtilGtk.BLUE_PLOTS,true,true);
		colormap.AllocColor (ref UtilGtk.LIGHT_BLUE_PLOTS,true,true);
		colormap.AllocColor (ref black,true,true);
		colormap.AllocColor (ref yellow,true,true);
		colormap.AllocColor (ref yellow_bg,true,true);
		colormap.AllocColor (ref green,true,true);
		colormap.AllocColor (ref gris,true,true);
		colormap.AllocColor (ref beige,true,true);
		colormap.AllocColor (ref brown,true,true);
		colormap.AllocColor (ref violet,true,true);
		//colormap.AllocColor (ref white,true,true);

		//-- Configurar los contextos graficos (pinceles)
		pen_rojo = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_yellow = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_yellow_discont = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_yellow_bg = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_azul_claro = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_azul = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_rojo_discont = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_azul_claro_discont = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_azul_discont = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		//pen_white= new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_black = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_black_90 = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_black_discont = new Gdk.GC(event_execute_drawingarea.GdkWindow);
		pen_black_bars = new Gdk.GC(event_execute_drawingarea.GdkWindow);
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
		
		pen_black.Foreground = black;
		pen_black_90.Foreground = black_90;
		pen_yellow.Foreground = yellow;
		pen_yellow_discont.Foreground = yellow;
		pen_yellow_bg.Foreground = yellow_bg;
		pen_black_bars.Foreground = black;
		//pen_white.Foreground = white;

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
	}
	void on_event_execute_button_cancel_clicked (object o, EventArgs args)
	{
		hideButtons();
		
		checkbutton_video.Sensitive = true;
		if(preferences.videoOn) {
			//it will be recorded on temp, but chronojump will move it to chronojump/multimedia folders
			label_video_feedback.Text = "";
			capturer.ClickStop();
			videoCapturePrepare(false); //if error, show message
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
