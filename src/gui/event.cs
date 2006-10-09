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
 * Xavier de Blas: 
 * http://www.xdeblas.com, http://www.deporteyciencia.com (parleblas)
 */

using System;
using Gtk;
using Glade;
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using Mono.Unix;

using System.Threading;


using Gdk; //for the EventMask


//--------------------------------------------------------
//---------------- EVENT EXECUTE WIDGET ----------------
//--------------------------------------------------------

public class EventExecuteWindow 
{
	[Widget] Gtk.Window event_execute;
	
	[Widget] Gtk.Label label_person;
	[Widget] Gtk.Label label_event_type;
	[Widget] Gtk.Label label_phases_name;
	[Widget] Gtk.Label label_simulated;
	
	[Widget] Gtk.ProgressBar progressbar_event;
	[Widget] Gtk.ProgressBar progressbar_time;
	

	//currently gtk-sharp cannot display a label in a progressBar in activity mode (Pulse() not Fraction)
	//then we show the value in a label:
	[Widget] Gtk.Label label_event_value;
	[Widget] Gtk.Label label_time_value;
	
	[Widget] Gtk.Button button_cancel;
	[Widget] Gtk.Button button_finish;
	[Widget] Gtk.Button button_update;
	[Widget] Gtk.Button button_close;

	
	[Widget] Gtk.VBox vbox_simple_jump;
	[Widget] Gtk.VBox vbox_reactive_jump;
	[Widget] Gtk.VBox vbox_run_simple;
	//interval
	//pulse
	
	[Widget] Gtk.Table table_simple_jump_values;
	[Widget] Gtk.Table table_reactive_jump_values;
	[Widget] Gtk.Table table_run_simple_values;
	//interval
	//pulse


	[Widget] Gtk.Label label_jump_simple_tc_now;
	[Widget] Gtk.Label label_jump_simple_tc_person;
	[Widget] Gtk.Label label_jump_simple_tc_session;
	[Widget] Gtk.Label label_jump_simple_tf_now;
	[Widget] Gtk.Label label_jump_simple_tf_person;
	[Widget] Gtk.Label label_jump_simple_tf_session;

	[Widget] Gtk.Label label_jump_reactive_tc_now;
	[Widget] Gtk.Label label_jump_reactive_tc_avg;
	[Widget] Gtk.Label label_jump_reactive_tf_now;
	[Widget] Gtk.Label label_jump_reactive_tf_avg;

	[Widget] Gtk.Label label_run_simple_time_now;
	[Widget] Gtk.Label label_run_simple_time_person;
	[Widget] Gtk.Label label_run_simple_time_session;
	[Widget] Gtk.Label label_run_simple_speed_now;
	[Widget] Gtk.Label label_run_simple_speed_person;
	[Widget] Gtk.Label label_run_simple_speed_session;

	[Widget] Gtk.DrawingArea drawingarea;
	static Gdk.Pixmap pixmap = null;


	int personID;	
	int sessionID;	
	string tableName;	
	string eventType;	
	private string lastEventWas;
	
	int pDN;
	double limit;
	
	private enum phasesGraph {
		UNSTARTED, DOING, DONE
	}
	private phasesGraph graphProgress;
	

	static EventGraphConfigureWindow eventGraphConfigureWin;
	
	static EventExecuteWindow EventExecuteWindowBox;
		
	EventExecuteWindow () {
		Glade.XML gladeXML;
		try {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "event_execute", null);
		} catch {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade.chronojump.glade", "event_execute", null);
		}

		gladeXML.Autoconnect(this);
		
		configureColors();
	}

	static public EventExecuteWindow Show (
			string windowTitle, string phasesName, int personID, string personName, int sessionID, 
			string tableName, string eventType, int pDN, double limit, bool simulated)
	{
		if (EventExecuteWindowBox == null) {
			EventExecuteWindowBox = new EventExecuteWindow (); 
		}

		//create the properties window if doesnt' exists, but do not show
		if(eventGraphConfigureWin == null)
			eventGraphConfigureWin = EventGraphConfigureWindow.Show(false);

		
		EventExecuteWindowBox.initializeVariables (
				windowTitle, phasesName, personID, personName, sessionID, 
				tableName, eventType, pDN, limit, simulated);

		EventExecuteWindowBox.event_execute.Show ();

		return EventExecuteWindowBox;
	}

	void initializeVariables (
			string windowTitle, string phasesName, int personID, string personName, int sessionID,
			string tableName, string eventType, int pDN, double limit, bool simulated) 
	{
		event_execute.Title = windowTitle;
		this.label_phases_name.Text = phasesName; 	//"Jumps" (rjInterval), "Runs" (runInterval), "Ticks" (pulses), 
								//"Phases" (simple jumps, dj, simple runs)
		this.personID = personID;
		this.label_person.Text = personName;
		this.tableName = tableName;
		this.sessionID = sessionID;

		this.eventType = eventType;
		this.label_event_type.Text = this.eventType;
		this.pDN = pDN;
		this.limit = limit;

		if(simulated)
			label_simulated.Show();
		else
			label_simulated.Hide();
			

		//allow to finish earlier if the event has subevents
		if(tableName == "jumpRj" || tableName == "runInterval" || tableName == "pulse")
			button_finish.Sensitive = true;
		else
			button_finish.Sensitive = false;

		if(tableName == "jump")
			showJumpSimpleLabels();
		else if (tableName == "jumpRj")
			showJumpReactiveLabels();
		else if (tableName == "run")
			showRunSimpleLabels();

		//for the "update" button
		lastEventWas = tableName;
			
		
		button_cancel.Sensitive = true;
		button_close.Sensitive = false;

		clearDrawingArea();
		clearProgressBars();

		graphProgress = phasesGraph.UNSTARTED; 
	}

	
	private void showJumpSimpleLabels() {
		//hide reactive info
		vbox_reactive_jump.Hide();
		table_reactive_jump_values.Hide();
		//hide run simple info
		vbox_run_simple.Hide();
		table_run_simple_values.Hide();
		
		//show simple jump info
		vbox_simple_jump.Show();
		table_simple_jump_values.Show();

		//initializeLabels
		label_jump_simple_tc_now.Text = "";
		label_jump_simple_tc_person.Text = "";
		label_jump_simple_tc_session.Text = "";
		label_jump_simple_tf_now.Text = "";
		label_jump_simple_tf_person.Text = "";
		label_jump_simple_tf_session.Text = "";
	}
	
	
	private void showJumpReactiveLabels() {
		//hide simple jump info
		vbox_simple_jump.Hide();
		table_simple_jump_values.Hide();
		//hide run simple info
		vbox_run_simple.Hide();
		table_run_simple_values.Hide();
		
		//show reactive info
		vbox_reactive_jump.Show();
		table_reactive_jump_values.Show();

		//initializeLabels
		label_jump_reactive_tc_now.Text = "";
		label_jump_reactive_tc_avg.Text = "";
		label_jump_reactive_tf_now.Text = "";
		label_jump_reactive_tf_avg.Text = "";
	}
	
	private void showRunSimpleLabels() {
		//hide simple jump info
		vbox_simple_jump.Hide();
		table_simple_jump_values.Hide();
		//hide reactive info
		vbox_reactive_jump.Hide();
		table_reactive_jump_values.Hide();
		
		//show run simple info
		vbox_run_simple.Show();
		table_run_simple_values.Show();
		
		//initializeLabels
		label_run_simple_time_now.Text = "";
		label_run_simple_time_person.Text = "";
		label_run_simple_time_session.Text = "";
		label_run_simple_speed_now.Text = "";
		label_run_simple_speed_person.Text = "";
		label_run_simple_speed_session.Text = "";
	}
		
	//called for cleaning the graph of a event done before than the current
	private void clearDrawingArea() 
	{
		if(pixmap == null) 
			pixmap = new Gdk.Pixmap (drawingarea.GdkWindow, drawingarea.Allocation.Width, drawingarea.Allocation.Height, -1);
		
		erasePaint(drawingarea);
	}
	
	private void clearProgressBars() 
	{
		progressbar_event.Fraction = 0;
		progressbar_event.Text = "";
		progressbar_time.Fraction = 0;
		progressbar_time.Text = "";
	
		//clear also the close labels
		label_event_value.Text = "";
		label_time_value.Text = "";
	}

	public void on_drawingarea_configure_event(object o, ConfigureEventArgs args)
	{
		Console.Write("A");
		Gdk.EventConfigure ev = args.Event;
		Gdk.Window window = ev.Window;
	

		Console.Write("B1");
		
		Gdk.Rectangle allocation = drawingarea.Allocation;
		
		Console.Write("B2");
	
		if(pixmap == null) {
			pixmap = new Gdk.Pixmap (window, allocation.Width, allocation.Height, -1);
		
			Console.Write("B3");
		
			erasePaint(drawingarea);
			
			graphProgress = phasesGraph.DOING; 
		}
	}
	
	public void on_drawingarea_expose_event(object o, ExposeEventArgs args)
	{
		/* in some mono installations, configure_event is not called, but expose_event yes. 
		 * Do here the initialization
		 */
		
		Console.Write("C");
		
		if(pixmap == null) {
			Console.Write("T1");
			
			Gdk.Rectangle allocation = drawingarea.Allocation;
			//pixmap = new Gdk.Pixmap (args.Event.Window, allocation.Width, allocation.Height, -1);
			pixmap = new Gdk.Pixmap (drawingarea.GdkWindow, allocation.Width, allocation.Height, -1);
			erasePaint(drawingarea);

			Console.Write("T2");
		
			graphProgress = phasesGraph.DOING; 
		}

			
		Console.Write("D");
		
		Gdk.Rectangle area = args.Event.Area;

		Console.Write("E1");

		//sometimes this is called when pait is finished
		//don't let this erase win
		//if(graphProgress != phasesGraph.DONE) {
		if(pixmap != null) {
			Console.Write("E2");

			args.Event.Window.DrawDrawable(drawingarea.Style.WhiteGC, pixmap,
				area.X, area.Y,
				area.X, area.Y,
				area.Width, area.Height);
		}
		
		Console.Write("E3");
	}


	private void erasePaint(Gtk.DrawingArea drawingarea) {
		pixmap.DrawRectangle (drawingarea.Style.WhiteGC, true, 0, 0,
				drawingarea.Allocation.Width, drawingarea.Allocation.Height);
	}
	

	// simple and DJ jump	
	public void PrepareJumpSimpleGraph(double tv, double tc) 
	{
		//check graph properties window is not null (propably user has closed it with the DeleteEvent
		//then create it, but not show it
		if(eventGraphConfigureWin == null)
			eventGraphConfigureWin = EventGraphConfigureWindow.Show(false);

		
		Console.Write("k1");
		
		//obtain data
		double tvPersonAVG = SqliteSession.SelectAllEventsOfAType(sessionID, personID, tableName, eventType, "TV");
		double tvSessionAVG = SqliteSession.SelectAllEventsOfAType(sessionID, -1, tableName, eventType, "TV");

		double tcPersonAVG = 0; 
		double tcSessionAVG = 0; 
		if(tc > 0) {
			tcPersonAVG = SqliteSession.SelectAllEventsOfAType(sessionID, personID, tableName, eventType, "TC");
			tcSessionAVG = SqliteSession.SelectAllEventsOfAType(sessionID, -1, tableName, eventType, "TC");
		}
		
		//paint graph
		paintJumpSimpleOrRunSimple (drawingarea, tv, tvPersonAVG, tvSessionAVG, tc, tcPersonAVG, tcSessionAVG);

		//printLabels
		printLabelsJumpSimple (tv, tvPersonAVG, tvSessionAVG, tc, tcPersonAVG, tcSessionAVG);
		
		Console.Write("k2");
		
		// -- refresh
		drawingarea.QueueDraw();
		
		Console.Write("k3");
		
	}
	
	// Reactive jump 
	public void PrepareJumpReactiveGraph(double lastTv, double lastTc, string tvString, string tcString) {
		//check graph properties window is not null (propably user has closed it with the DeleteEvent
		//then create it, but not show it
		if(eventGraphConfigureWin == null)
			eventGraphConfigureWin = EventGraphConfigureWindow.Show(false);

		Console.Write("l1");

		//search AVGs and MAXs
		double maxValue = Util.GetMax(tvString);
		double maxTC = Util.GetMax(tcString);
		if(maxTC > maxValue)
			maxValue = maxTC;
		int jumps = Util.GetNumberOfJumps(tvString, true); 

		//paint graph
		paintJumpReactive (drawingarea, lastTv, lastTc, tvString, tcString, Util.GetAverage(tvString), Util.GetAverage(tcString), maxValue, jumps);
		
		Console.Write("l2");
		
		// -- refresh
		drawingarea.QueueDraw();
		
		Console.Write("l3");
	}

	// run simple
	public void PrepareRunSimpleGraph(double time, double speed) 
	{
		//check graph properties window is not null (propably user has closed it with the DeleteEvent
		//then create it, but not show it
		if(eventGraphConfigureWin == null)
			eventGraphConfigureWin = EventGraphConfigureWindow.Show(false);

		
		Console.Write("k1");
		
		//obtain data
		double timePersonAVG = SqliteSession.SelectAllEventsOfAType(sessionID, personID, tableName, eventType, "time");
		double timeSessionAVG = SqliteSession.SelectAllEventsOfAType(sessionID, -1, tableName, eventType, "time");
		double distancePersonAVG = SqliteSession.SelectAllEventsOfAType(sessionID, personID, tableName, eventType, "distance");
		double distanceSessionAVG = SqliteSession.SelectAllEventsOfAType(sessionID, -1, tableName, eventType, "distance");

		//paint graph
		paintJumpSimpleOrRunSimple (drawingarea, time, timePersonAVG, timeSessionAVG, speed, distancePersonAVG / timePersonAVG, distanceSessionAVG /timeSessionAVG);
		
		//printLabels
		printLabelsRunSimple (time, timePersonAVG, timeSessionAVG, speed, distancePersonAVG / timePersonAVG, distanceSessionAVG /timeSessionAVG);
		
		Console.Write("k2");
		
		// -- refresh
		drawingarea.QueueDraw();
		
		Console.Write("k3");
		
	}
	
	// run interval
	public void PrepareRunIntervalGraph() 
	{
	}

	// pulse 
	public void PreparePulseGraph() 
	{
	}

	private void printLabelsJumpSimple (double tvNow, double tvPerson, double tvSession, double tcNow, double tcPerson, double tcSession) {
		if(tcNow > 0) {
			label_jump_simple_tc_now.Text = Util.TrimDecimals(tcNow.ToString(), pDN);
			label_jump_simple_tc_person.Text = Util.TrimDecimals(tcPerson.ToString(), pDN);
			label_jump_simple_tc_session.Text = Util.TrimDecimals(tcSession.ToString(), pDN);
		} else {
			label_jump_simple_tc_now.Text = "";
			label_jump_simple_tc_person.Text = "";
			label_jump_simple_tc_session.Text = "";
		}
		label_jump_simple_tf_now.Text = Util.TrimDecimals(tvNow.ToString(), pDN);
		label_jump_simple_tf_person.Text = Util.TrimDecimals(tvPerson.ToString(), pDN);
		label_jump_simple_tf_session.Text = Util.TrimDecimals(tvSession.ToString(), pDN);
	}
	
	private void printLabelsRunSimple (double timeNow, double timePerson, double timeSession, double speedNow, double speedPerson, double speedSession) {
		label_run_simple_time_now.Text = Util.TrimDecimals(timeNow.ToString(), pDN);
		label_run_simple_time_person.Text = Util.TrimDecimals(timePerson.ToString(), pDN);
		label_run_simple_time_session.Text = Util.TrimDecimals(timeSession.ToString(), pDN);
		
		label_run_simple_speed_now.Text = Util.TrimDecimals(speedNow.ToString(), pDN);
		label_run_simple_speed_person.Text = Util.TrimDecimals(speedPerson.ToString(), pDN);
		label_run_simple_speed_session.Text = Util.TrimDecimals(speedSession.ToString(), pDN);
	}
	
	/*
	private void printLabelsJumpReactive (tvNow, tvPersonAVG, tvSessionAVG, tcNow, tcPersonAVG, tcSessionAVG);
		if(tcNow > 0) {
			label_jump_simple_tc_now.Text = Util.TrimDecimals(tcNow.ToString(), pDN);
			label_jump_simple_tc_person.Text = Util.TrimDecimals(tcPerson.ToString(), pDN);
			label_jump_simple_tc_session.Text = Util.TrimDecimals(tcSession.ToString(), pDN);
		} else {
			label_jump_simple_tc_now.Text = "";
			label_jump_simple_tc_person.Text = "";
			label_jump_simple_tc_session.Text = "";
		}
		label_jump_simple_tf_now.Text = Util.TrimDecimals(tvNow.ToString(), pDN);
		label_jump_simple_tf_person.Text = Util.TrimDecimals(tvPerson.ToString(), pDN);
		label_jump_simple_tf_session.Text = Util.TrimDecimals(tvSession.ToString(), pDN);
	}
		*/

	//if it's a run, tc means time, and tf(tv) means speed
	//simply works
	private void paintJumpSimpleOrRunSimple (Gtk.DrawingArea drawingarea, 
			double tvNow, double tvPerson, double tvSession, 
			double tcNow, double tcPerson, double tcSession)
	{
		//TEMPORARY, for only make graph of normal jump events
		if(tvNow == -1) 
			return;

		
		int ancho=drawingarea.Allocation.Width;
		int alto=drawingarea.Allocation.Height;
		
		
		Console.Write(" paint1 ");
		
		double maxValue = 0;
		double minValue = 0;
		int topMargin = 10; 
		//double bottomMargin = 10; 

		//if max value of graph is automatic
		if(eventGraphConfigureWin.Max == -1) {
			maxValue = Util.GetMax(
					tvNow.ToString() + "=" + tvPerson.ToString() + "=" + tvSession.ToString() + "=" +
					tcNow.ToString() + "=" + tcPerson.ToString() + "=" + tcSession.ToString());
		} else {
			maxValue = eventGraphConfigureWin.Max;
			topMargin = 0;
		}
		
		//if min value of graph is automatic
		/*
		if(eventGraphConfigureWin.Min == -1) {
			if(tcNow == 0)
				//if has not tc, get minimum in tv values
				minValue = Util.GetMin(
						tvNow.ToString() + "=" + tvPerson.ToString() + "=" + tvSession.ToString());
			else
				//if has tc, get minimum in all values
				minValue = Util.GetMin(
						tvNow.ToString() + "=" + tvPerson.ToString() + "=" + tvSession.ToString() + "=" +
						tcNow.ToString() + "=" + tcPerson.ToString() + "=" + tcSession.ToString());
		} else {
			minValue = eventGraphConfigureWin.Min;
			bottomMargin = 0;
		}
		*/
		minValue = eventGraphConfigureWin.Min;
		
		
		Console.WriteLine("{0}, {1}, {2}", tcNow, tcPerson, tcSession);
		Console.WriteLine("{0}, {1}, {2}", tvNow, tvPerson, tvSession);
		Console.WriteLine("maxValue: {0}", maxValue);
		Console.WriteLine("minValue: {0}", minValue);

		
		erasePaint(drawingarea);
		
		Console.Write(" paint7 ");
	
		//check now here that we will have not division by zero problems
		if(maxValue - minValue > 0) {
			//red for TC
			pixmap.DrawLine(pen_rojo, ancho*1/6, alto, ancho*1/6, Convert.ToInt32(alto - ((tcNow - minValue) * (alto - topMargin) / (maxValue - minValue))));
			pixmap.DrawLine(pen_rojo, ancho*3/6, alto, ancho*3/6, Convert.ToInt32(alto - ((tcPerson - minValue) * (alto - topMargin) / (maxValue - minValue))));
			pixmap.DrawLine(pen_rojo, ancho*5/6, alto, ancho*5/6, Convert.ToInt32(alto - ((tcSession - minValue) * (alto - topMargin) / (maxValue - minValue))));
		
			//blue for TF
			pixmap.DrawLine(pen_azul, ancho*1/6 +10, alto, ancho*1/6 +10, Convert.ToInt32(alto - ((tvNow - minValue) * (alto - topMargin) / (maxValue - minValue))));
			pixmap.DrawLine(pen_azul, ancho*3/6 +10, alto, ancho*3/6 +10, Convert.ToInt32(alto - ((tvPerson - minValue) * (alto - topMargin) / (maxValue - minValue))));
			pixmap.DrawLine(pen_azul, ancho*5/6 +10, alto, ancho*5/6 +10, Convert.ToInt32(alto - ((tvSession - minValue) * (alto - topMargin) / (maxValue - minValue))));

	
			//paint reference guide black and green if needed
			drawGuideOrAVG(pen_negro_discont, eventGraphConfigureWin.BlackGuide, alto, ancho, topMargin, maxValue, minValue);
			drawGuideOrAVG(pen_green_discont, eventGraphConfigureWin.GreenGuide, alto, ancho, topMargin, maxValue, minValue);
		}
		
	
		
		Console.Write(" paint2 ");

		/*
		if(tcNow > 0) {
			label_jump_simple_tc_now.Text = Util.TrimDecimals(tcNow.ToString(), pDN);
			label_jump_simple_tc_person.Text = Util.TrimDecimals(tcPerson.ToString(), pDN);
			label_jump_simple_tc_session.Text = Util.TrimDecimals(tcSession.ToString(), pDN);
		} else {
			label_jump_simple_tc_now.Text = "";
			label_jump_simple_tc_person.Text = "";
			label_jump_simple_tc_session.Text = "";
		}
		label_jump_simple_tf_now.Text = Util.TrimDecimals(tvNow.ToString(), pDN);
		label_jump_simple_tf_person.Text = Util.TrimDecimals(tvPerson.ToString(), pDN);
		label_jump_simple_tf_session.Text = Util.TrimDecimals(tvSession.ToString(), pDN);
		*/
			
		graphProgress = phasesGraph.DONE; 
	}

	
	private void paintJumpReactive (Gtk.DrawingArea drawingarea, double lastTv, double lastTc, string tvString, string tcString, 
			double avgTV, double avgTC, double maxValue, int jumps)
	{
		int topMargin = 10; 
		int ancho=drawingarea.Allocation.Width;
		int alto=drawingarea.Allocation.Height;
		
		
		Console.Write(" paint1 reactive 1");
		
		erasePaint(drawingarea);
		
		double minValue = eventGraphConfigureWin.Min;
		
		//check now here that we will have not division by zero problems
		if(maxValue - minValue > 0) {

			//blue tf average discountinuos line	
			//pixmap.DrawLine(pen_azul_discont, 
			//		0, Convert.ToInt32(alto - (avgTV * (alto - topMargin) / maxValue)),
			//		ancho, Convert.ToInt32(alto - (avgTV * (alto - topMargin) / maxValue)));
			drawGuideOrAVG(pen_azul_discont, avgTV, alto, ancho, topMargin, maxValue, minValue);


			//red tc average discountinuos line	
			//pixmap.DrawLine(pen_rojo_discont, 
			//		0, Convert.ToInt32(alto - (avgTC * (alto - topMargin) / maxValue)),
			//		ancho, Convert.ToInt32(alto - (avgTC * (alto - topMargin) / maxValue)));
			drawGuideOrAVG(pen_rojo_discont, avgTC, alto, ancho, topMargin, maxValue, minValue);


			//paint reference guide black and green if needed
			drawGuideOrAVG(pen_negro_discont, eventGraphConfigureWin.BlackGuide, alto, ancho, topMargin, maxValue, minValue);
			drawGuideOrAVG(pen_green_discont, eventGraphConfigureWin.GreenGuide, alto, ancho, topMargin, maxValue, minValue);

			
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
					pixmap.DrawLine(pen_azul, //blue for TF
							Convert.ToInt32(ancho*(count-.5)/jumps), Convert.ToInt32(alto - (oldValue * (alto - topMargin) / maxValue)),
							Convert.ToInt32(ancho*(count+.5)/jumps), Convert.ToInt32(alto - (myTVDouble * (alto - topMargin) / maxValue)));

				oldValue = myTVDouble;
				count ++;
			}

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
					pixmap.DrawLine(pen_rojo, //red for TC
							Convert.ToInt32(ancho*(count-.5)/jumps), Convert.ToInt32(alto - (oldValue * (alto - topMargin) / maxValue)),
							Convert.ToInt32(ancho*(count+.5)/jumps), Convert.ToInt32(alto - (myTCDouble * (alto - topMargin) / maxValue)));

				oldValue = myTCDouble;
				count ++;
			}
		}
		
		Console.Write(" paint reactive 2 ");

		label_jump_reactive_tc_now.Text = Util.TrimDecimals(lastTc.ToString(), pDN);
		label_jump_reactive_tc_avg.Text = Util.TrimDecimals(avgTC.ToString(), pDN);
		label_jump_reactive_tf_now.Text = Util.TrimDecimals(lastTv.ToString(), pDN);
		label_jump_reactive_tf_avg.Text = Util.TrimDecimals(avgTV.ToString(), pDN);
		
		graphProgress = phasesGraph.DONE; 
	}

	private void drawGuideOrAVG(Gdk.GC myPen, double guideHeight, int alto, int ancho, int topMargin, double maxValue, double minValue) 
	{
		if(guideHeight == -1)
			return; //return if checkbox guide is not checked
		else
			pixmap.DrawLine(myPen, 
					0, Convert.ToInt32(alto - ((guideHeight - minValue) * (alto - topMargin) / (maxValue - minValue))),
					ancho, Convert.ToInt32(alto - ((guideHeight - minValue) * (alto - topMargin) / (maxValue - minValue))));
	}


	private void hideButtons() {
		button_cancel.Sensitive = false;
		button_close.Sensitive = true;
		button_finish.Sensitive = false;
	}


	public void EventEnded() {
		hideButtons();
	}
	
	
	
	void on_button_properties_clicked (object o, EventArgs args) {
		//now show the eventGraphConfigureWin
		eventGraphConfigureWin = EventGraphConfigureWindow.Show(true);
	}

	void on_button_update_clicked (object o, EventArgs args) 
	{
		//event will be raised, and managed in chronojump.cs
		//see ButtonUpdate at end of class
	}
	
		
	void on_finish_clicked (object o, EventArgs args)
	{
		//event will be raised, and managed in chronojump.cs
		//see ButtonFinish at end of class
	}
			
	void on_button_help_clicked (object o, EventArgs args)
	{
		new DialogHelp(Catalog.GetString("This window shows the execution of an event. In the graph, you may see:\n-\"Now\": shows the data of the current event.\n-\"Person AVG\": shows the average of the current person executing this type of event on this session.\n-\"Session AVG\": shows the Average of all persons executing this type of event on this session.\n(For more statistics data, you may use the statistics window).\n\nAt the bottom you may see the evolution of the event, and you may finish it (depending on the type of event), or even cancel it."));
	}

	void on_button_cancel_clicked (object o, EventArgs args)
	{
		//event will be raised, and managed in chronojump.cs
		hideButtons();
	}
		
	void on_button_close_clicked (object o, EventArgs args)
	{
		EventExecuteWindowBox.event_execute.Hide();
		EventExecuteWindowBox = null;
	}
	
	void on_delete_event (object o, DeleteEventArgs args)
	{
		//if there's an event doing, simulate a cancel
		//if there's not, simulate also
		button_cancel.Click();
		
		EventExecuteWindowBox.event_execute.Hide();
		EventExecuteWindowBox = null;
	}
	
	public void ProgressBarEventOrTimePreExecution (bool isEvent, bool percentageMode, double events) 
	{
		if (isEvent) 
			progressbarEventOrTimeExecution (progressbar_event, percentageMode, label_event_value, events);
		else
			progressbarEventOrTimeExecution (progressbar_time, percentageMode, label_time_value, events);
	}

	private void progressbarEventOrTimeExecution (Gtk.ProgressBar progressbar, bool percentageMode, Gtk.Label label_value, double events)
	{
		if(limit == -1) {	//unlimited event (until 'finish' is clicked)
			progressbar.Pulse();
			label_value.Text = events.ToString();
		} else {
			if(percentageMode) {
				double myFraction = events / limit;

				if(myFraction > 1)
					myFraction = 1;
				else if(myFraction < 0)
					myFraction = 0;

				//Console.Write("{0}-{1}", limit, myFraction);
				progressbar.Fraction = myFraction;
				progressbar.Text = Util.TrimDecimals(events.ToString(), 1) + " / " + limit.ToString();
			} else {
				//activity mode
				progressbar.Pulse();

				//pass -1 in events in activity mode if don't want to use this label
				if(events != -1)
					label_value.Text = Util.TrimDecimals(events.ToString(), 1);
			}
		}
	}

	
	public Button ButtonCancel 
	{
		get { return button_cancel; }
	}
	
	public Button ButtonFinish 
	{
		get { return button_finish; }
	}

	public Button ButtonUpdate 
	{
		get { return button_update; }
	}
	
	
	//projecte cubevirtual de juan gonzalez
	
	Gdk.GC pen_rojo; //tc
	Gdk.GC pen_azul; //tf
	Gdk.GC pen_rojo_discont; //avg tc in reactive
	Gdk.GC pen_azul_discont; //avg tf in reactive
	Gdk.GC pen_negro_discont; //guide
	Gdk.GC pen_green_discont; //guide
	//Gdk.GC pen_blanco;
	

	void configureColors()
	{
		Gdk.Color rojo = new Gdk.Color(0xff,0,0);
		Gdk.Color azul  = new Gdk.Color(0,0,0xff);
		Gdk.Color negro = new Gdk.Color(0,0,0);
		Gdk.Color green = new Gdk.Color(0,0xff,0);
		//Gdk.Color blanco = new Gdk.Color(0xff,0xff,0xff);

		Gdk.Colormap colormap = Gdk.Colormap.System;
		colormap.AllocColor (ref rojo, true, true);
		colormap.AllocColor (ref azul,true,true);
		colormap.AllocColor (ref negro,true,true);
		colormap.AllocColor (ref green,true,true);
		//colormap.AllocColor (ref blanco,true,true);

		//-- Configurar los contextos graficos (pinceles)
		pen_rojo = new Gdk.GC(drawingarea.GdkWindow);
		pen_azul = new Gdk.GC(drawingarea.GdkWindow);
		pen_rojo_discont = new Gdk.GC(drawingarea.GdkWindow);
		pen_azul_discont = new Gdk.GC(drawingarea.GdkWindow);
		//pen_negro = new Gdk.GC(drawingarea.GdkWindow);
		//pen_blanco= new Gdk.GC(drawingarea.GdkWindow);
		pen_negro_discont = new Gdk.GC(drawingarea.GdkWindow);
		pen_green_discont = new Gdk.GC(drawingarea.GdkWindow);

		pen_rojo.Foreground = rojo;
		pen_azul.Foreground = azul;
		
		pen_rojo_discont.Foreground = rojo;
		pen_azul_discont.Foreground = azul;
		pen_rojo_discont.SetLineAttributes(1, Gdk.LineStyle.OnOffDash, Gdk.CapStyle.Butt, Gdk.JoinStyle.Round);
		pen_azul_discont.SetLineAttributes(1, Gdk.LineStyle.OnOffDash, Gdk.CapStyle.Butt, Gdk.JoinStyle.Round);
		
		pen_negro_discont.Foreground = negro;
		pen_negro_discont.SetLineAttributes(1, Gdk.LineStyle.OnOffDash, Gdk.CapStyle.Butt, Gdk.JoinStyle.Round);
		pen_green_discont.Foreground = green;
		pen_green_discont.SetLineAttributes(1, Gdk.LineStyle.OnOffDash, Gdk.CapStyle.Butt, Gdk.JoinStyle.Round);
	}
}


//--------------------------------------------------------
//---------------- EVENT GRAPH CONFIGURE WIDGET ----------------
//--------------------------------------------------------


public class EventGraphConfigureWindow 
{
	[Widget] Gtk.Window event_graph_configure;
	
	[Widget] Gtk.Button button_finish;
	[Widget] Gtk.Button button_close;

	[Widget] Gtk.CheckButton checkbutton_max_auto;
	//[Widget] Gtk.CheckButton checkbutton_min_auto;
	[Widget] Gtk.CheckButton checkbutton_show_black_guide;
	[Widget] Gtk.CheckButton checkbutton_show_green_guide;
	
	[Widget] Gtk.SpinButton spinbutton_max;
	[Widget] Gtk.SpinButton spinbutton_min;
	[Widget] Gtk.SpinButton spinbutton_black_guide;
	[Widget] Gtk.SpinButton spinbutton_green_guide;

	
	static EventGraphConfigureWindow EventGraphConfigureWindowBox;
		
	EventGraphConfigureWindow () {
		Glade.XML gladeXML;
		try {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "event_graph_configure", null);
		} catch {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade.chronojump.glade", "event_graph_configure", null);
		}

		gladeXML.Autoconnect(this);
	}

	//bool reallyShow
	//we create this window on start of event_execute widget for having the graph execute values defined
	//but we don't want to show until user clicks on "properties" on the event_execute widget
	static public EventGraphConfigureWindow Show (bool reallyShow)
	{
		if (EventGraphConfigureWindowBox == null) {
			EventGraphConfigureWindowBox = new EventGraphConfigureWindow (); 
			EventGraphConfigureWindowBox.initializeWidgets(); 
		}
		
		if(reallyShow)
			EventGraphConfigureWindowBox.event_graph_configure.Show ();
		else
			EventGraphConfigureWindowBox.event_graph_configure.Hide ();
		
		return EventGraphConfigureWindowBox;
	}
	
	void initializeWidgets ()
	{
		checkbutton_max_auto.Active = true;
		//checkbutton_min_auto.Active = false;
		
		checkbutton_show_black_guide.Active = false;
		checkbutton_show_green_guide.Active = false;
			
		spinbutton_black_guide.Sensitive = false;
		spinbutton_green_guide.Sensitive = false;
	}

	void on_checkbutton_max_auto_clicked (object o, EventArgs args) {
		if(checkbutton_max_auto.Active)
			spinbutton_max.Sensitive = false;
		else
			spinbutton_max.Sensitive = true;
	}
	
	/*
	void on_checkbutton_min_auto_clicked (object o, EventArgs args) {
		Console.WriteLine("ch_min_auto Clicked");
		if(checkbutton_min_auto.Active)
			spinbutton_min.Sensitive = false;
		else
			spinbutton_min.Sensitive = true;
	}
	*/
	
	void on_checkbutton_show_black_guide_clicked (object o, EventArgs args) {
		if(checkbutton_show_black_guide.Active)
			spinbutton_black_guide.Sensitive = true;
		else
			spinbutton_black_guide.Sensitive = false;
	}
	
	void on_checkbutton_show_green_guide_clicked (object o, EventArgs args) {
		if(checkbutton_show_green_guide.Active)
			spinbutton_green_guide.Sensitive = true;
		else
			spinbutton_green_guide.Sensitive = false;
	}
	
		
	void on_button_help_clicked (object o, EventArgs args)
	{
		Console.WriteLine("help Clicked");
		/*
		new DialogHelp(Catalog.GetString("This window shows the execution of an event. In the graph, you may see:\n-\"Now\": shows the data of the current event.\n-\"Person AVG\": shows the average of the current person executing this type of event on this session.\n-\"Session AVG\": shows the Average of all persons executing this type of event on this session.\n(For more statistics data, you may use the statistics window).\n\nAt the bottom you may see the evolution of the event, and you may finish it (depending on the type of event), or even cancel it."));
		*/
	}

	void on_button_close_clicked (object o, EventArgs args)
	{
		EventGraphConfigureWindowBox.event_graph_configure.Hide();
		//EventGraphConfigureWindowBox = null;
	}

	void on_delete_event (object o, DeleteEventArgs args)
	{
		EventGraphConfigureWindowBox.event_graph_configure.Hide();
		EventGraphConfigureWindowBox = null;
	}

	public double Max {
		get {
			if(checkbutton_max_auto.Active)
				return -1;
			else
				return Convert.ToDouble(spinbutton_max.Value);
		}
	}

	public double Min {
		get {
			/*
			if(checkbutton_min_auto.Active)
				return -1;
			else
			*/
				return Convert.ToDouble(spinbutton_min.Value);
		}
	}

	public double BlackGuide {
		get {
			if(checkbutton_show_black_guide.Active)
				return Convert.ToDouble(spinbutton_black_guide.Value);
			else
				return -1;
		}
	}

	public double GreenGuide {
		get {
			if(checkbutton_show_green_guide.Active)
				return Convert.ToDouble(spinbutton_green_guide.Value);
			else
				return -1;
		}
	}

}

