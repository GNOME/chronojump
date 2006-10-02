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
//using Gnome;
//using GLib; //for Value
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
	
	/*
	[Widget] Gtk.ProgressBar progressbar_tv_current;
	[Widget] Gtk.ProgressBar progressbar_tc_current;
	[Widget] Gtk.ProgressBar progressbar_tv_tc_current_1up;
	[Widget] Gtk.ProgressBar progressbar_tv_tc_current_0;
	[Widget] Gtk.ProgressBar progressbar_tv_avg;
	[Widget] Gtk.ProgressBar progressbar_tc_avg;
	[Widget] Gtk.ProgressBar progressbar_tv_tc_avg_1up;
	[Widget] Gtk.ProgressBar progressbar_tv_tc_avg_0;

	*/
	
	[Widget] Gtk.ProgressBar progressbar_event;
	[Widget] Gtk.ProgressBar progressbar_time;
	

	//currently gtk-sharp cannot display a label in a progressBar in activity mode (Pulse() not Fraction)
	//then we show the value in a label:
	[Widget] Gtk.Label label_event_value;
	[Widget] Gtk.Label label_time_value;
	
	/*
	[Widget] Gtk.CheckButton checkbutton_show_tv_tc;
	[Widget] Gtk.Box hbox_tv_tc;
	[Widget] Gtk.Box vbox_tv_tc;
	[Widget] Gtk.Label label_tv_tc;
	*/
	
	[Widget] Gtk.Button button_cancel;
	[Widget] Gtk.Button button_finish;
	[Widget] Gtk.Button button_close;
	

	[Widget] Gtk.HBox hbox_drawingarea;


	[Widget] Gtk.DrawingArea drawingarea;
	static Gdk.Pixmap pixmap = null;


	int personID;	
	int sessionID;	
	string tableName;	
	string eventType;	
	
	int pDN;
	double limit;
	
	private enum phasesGraph {
		UNSTARTED, DOING, DONE
	}
	private phasesGraph graphProgress;

	
	static EventExecuteWindow EventExecuteWindowBox;
		
	EventExecuteWindow () {
		Glade.XML gladeXML;
		try {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "event_execute", null);
		} catch {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade.chronojump.glade", "event_execute", null);
		}

		gladeXML.Autoconnect(this);
		
		/* afegit a saco pq sembla que el galde falla en alguns monos (juanfer)
		 */

		/*
		drawingarea = new Gtk.DrawingArea ();
		drawingarea.SetSizeRequest (200, 200);
		hbox_drawingarea.PackStart(drawingarea, false, false, 0);
		hbox_drawingarea.ShowAll();

		drawingarea.ExposeEvent += new ExposeEventHandler (on_drawingarea_expose_event);
		drawingarea.ConfigureEvent += new ConfigureEventHandler (on_drawingarea_configure_event);
		*/
		
	}

	static public EventExecuteWindow Show (string windowTitle, string phasesName, int personID, string personName, int sessionID, 
			string tableName, string eventType, int pDN, double limit, bool simulated)
	{
		if (EventExecuteWindowBox == null) {
			EventExecuteWindowBox = new EventExecuteWindow (); 
		}
		
		EventExecuteWindowBox.initializeVariables (windowTitle, phasesName, personID, personName, sessionID, 
				tableName, eventType, pDN, limit, simulated);

		EventExecuteWindowBox.event_execute.Show ();

		return EventExecuteWindowBox;
	}

	void initializeVariables (string windowTitle, string phasesName, int personID, string personName, int sessionID,
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
			
		button_cancel.Sensitive = true;
		button_close.Sensitive = false;

		graphProgress = phasesGraph.UNSTARTED; 
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
	


	[Widget] Gtk.Label label_tc_person;
	[Widget] Gtk.Label label_tv_session;
	[Widget] Gtk.Label label_tc_session;
	[Widget] Gtk.Label label_tv_now;
	[Widget] Gtk.Label label_tc_now;
	[Widget] Gtk.Label label_tv_person;


	[Widget] Gtk.Label label_tc;
	[Widget] Gtk.Label label_tf;
	[Widget] Gtk.Table table_simple_jump_values;

	
	private void paintJumpSimple (Gtk.DrawingArea drawingarea, 
			double tvNow, double tvPerson, double tvSession, 
			double tcNow, double tcPerson, double tcSession)
	{
		//TEMPORARY, for only make graph of normal jump events
		if(tvNow == -1) 
			return;

		
		double topMargin = 10; 
		int ancho=drawingarea.Allocation.Width;
		int alto=drawingarea.Allocation.Height;
		
		
		Console.Write(" paint1 ");
		
		//change in a near future ;)
		double maxValue = tvNow;
		if(tvPerson > maxValue) maxValue = tvPerson;
		if(tvSession > maxValue) maxValue = tvSession;
		if(tcNow > maxValue) maxValue = tcNow;
		if(tcPerson > maxValue) maxValue = tcPerson;
		if(tcSession > maxValue) maxValue = tcSession;
		
		
		Console.WriteLine("{0}, {1}, {2}", tcNow, tcPerson, tcSession);
		Console.WriteLine("{0}, {1}, {2}", tvNow, tvPerson, tvSession);
		Console.WriteLine("maxValue: {0}", maxValue);
		
		erasePaint(drawingarea);
		
		
		pixmap.DrawLine(drawingarea.Style.BlackGC, ancho*1/6, alto, ancho*1/6, Convert.ToInt32(alto - (tcNow * (alto - topMargin) / maxValue)));
		pixmap.DrawLine(drawingarea.Style.BlackGC, ancho*3/6, alto, ancho*3/6, Convert.ToInt32(alto - (tcPerson * (alto - topMargin) / maxValue)));
		pixmap.DrawLine(drawingarea.Style.BlackGC, ancho*5/6, alto, ancho*5/6, Convert.ToInt32(alto - (tcSession * (alto - topMargin) / maxValue)));
		
		pixmap.DrawLine(drawingarea.Style.BlackGC, ancho*1/6 +10, alto, ancho*1/6 +10, Convert.ToInt32(alto - (tvNow * (alto - topMargin) / maxValue)));
		pixmap.DrawLine(drawingarea.Style.BlackGC, ancho*3/6 +10, alto, ancho*3/6 +10, Convert.ToInt32(alto - (tvPerson * (alto - topMargin) / maxValue)));
		pixmap.DrawLine(drawingarea.Style.BlackGC, ancho*5/6 +10, alto, ancho*5/6 +10, Convert.ToInt32(alto - (tvSession * (alto - topMargin) / maxValue)));
		
		
		Console.Write(" paint2 ");

		label_tc_now.Text = Util.TrimDecimals(tcNow.ToString(), pDN);
		label_tc_person.Text = Util.TrimDecimals(tcPerson.ToString(), pDN);
		label_tc_session.Text = Util.TrimDecimals(tcSession.ToString(), pDN);
		label_tv_now.Text = Util.TrimDecimals(tvNow.ToString(), pDN);
		label_tv_person.Text = Util.TrimDecimals(tvPerson.ToString(), pDN);
		label_tv_session.Text = Util.TrimDecimals(tvSession.ToString(), pDN);
			
		graphProgress = phasesGraph.DONE; 
	}

	
	private void paintJumpReactive (Gtk.DrawingArea drawingarea, string tvString, string tcString, 
			double avgTV, double avgTC, double maxValue, int jumps)
	{
	/*
		//TEMPORARY, for only make graph of normal jump events
		if(tvNow == -1) 
			return;
	*/

		
		double topMargin = 10; 
		int ancho=drawingarea.Allocation.Width;
		int alto=drawingarea.Allocation.Height;
		
		
		Console.Write(" paint1 reactive 1");
		
		/*
		//change in a near future ;)
		double maxValue = tvNow;
		if(tvPerson > maxValue) maxValue = tvPerson;
		if(tvSession > maxValue) maxValue = tvSession;
		if(tcNow > maxValue) maxValue = tcNow;
		if(tcPerson > maxValue) maxValue = tcPerson;
		if(tcSession > maxValue) maxValue = tcSession;
		*/
		/*
		Console.WriteLine("{0}, {1}, {2}", tcNow, tcPerson, tcSession);
		Console.WriteLine("{0}, {1}, {2}", tvNow, tvPerson, tvSession);
		Console.WriteLine("maxValue: {0}", maxValue);
		*/
		
		erasePaint(drawingarea);
		
		
		string [] myTVStringFull = tvString.Split(new char[] {'='});
		int count = 0;
		double oldValue = 0;
		double myTVDouble = 0;
		foreach (string myTV in myTVStringFull) {
			myTVDouble = Convert.ToDouble(myTV);
			if(myTVDouble < 0)
				myTVDouble = 0;
			
			if (count > 0)
				pixmap.DrawLine(drawingarea.Style.BlackGC, 
						Convert.ToInt32(ancho*(count-.5)/jumps) , Convert.ToInt32(alto - (oldValue * (alto - topMargin) / maxValue)),
						Convert.ToInt32(ancho*(count+.5)/jumps), Convert.ToInt32(alto - (myTVDouble * (alto - topMargin) / maxValue)));
			
			oldValue = myTVDouble;
			count ++;
		}

		string [] myTCStringFull = tcString.Split(new char[] {'='});
		count = 0;
		oldValue = 0;
		double myTCDouble = 0;
		foreach (string myTC in myTCStringFull) {
			myTCDouble = Convert.ToDouble(myTC);
			if(myTCDouble < 0)
				myTCDouble = 0;
			
			if (count > 0)
				pixmap.DrawLine(drawingarea.Style.BlackGC, 
						Convert.ToInt32(ancho*(count-.5)/jumps) , Convert.ToInt32(alto - (oldValue * (alto - topMargin) / maxValue)),
						Convert.ToInt32(ancho*(count+.5)/jumps), Convert.ToInt32(alto - (myTCDouble * (alto - topMargin) / maxValue)));
			
			oldValue = myTCDouble;
			count ++;
		}

	
		/*
		pixmap.DrawLine(drawingarea.Style.BlackGC, ancho*1/6, alto, ancho*1/6, Convert.ToInt32(alto - (tcNow * (alto - topMargin) / maxValue)));
		pixmap.DrawLine(drawingarea.Style.BlackGC, ancho*3/6, alto, ancho*3/6, Convert.ToInt32(alto - (tcPerson * (alto - topMargin) / maxValue)));
		pixmap.DrawLine(drawingarea.Style.BlackGC, ancho*5/6, alto, ancho*5/6, Convert.ToInt32(alto - (tcSession * (alto - topMargin) / maxValue)));
		
		pixmap.DrawLine(drawingarea.Style.BlackGC, ancho*1/6 +10, alto, ancho*1/6 +10, Convert.ToInt32(alto - (tvNow * (alto - topMargin) / maxValue)));
		pixmap.DrawLine(drawingarea.Style.BlackGC, ancho*3/6 +10, alto, ancho*3/6 +10, Convert.ToInt32(alto - (tvPerson * (alto - topMargin) / maxValue)));
		pixmap.DrawLine(drawingarea.Style.BlackGC, ancho*5/6 +10, alto, ancho*5/6 +10, Convert.ToInt32(alto - (tvSession * (alto - topMargin) / maxValue)));
		*/
		
		Console.Write(" paint reactive 2 ");

		/*
		label_tc_now.Text = Util.TrimDecimals(tcNow.ToString(), pDN);
		label_tc_person.Text = Util.TrimDecimals(tcPerson.ToString(), pDN);
		label_tc_session.Text = Util.TrimDecimals(tcSession.ToString(), pDN);
		label_tv_now.Text = Util.TrimDecimals(tvNow.ToString(), pDN);
		label_tv_person.Text = Util.TrimDecimals(tvPerson.ToString(), pDN);
		label_tv_session.Text = Util.TrimDecimals(tvSession.ToString(), pDN);
		*/
			
		graphProgress = phasesGraph.DONE; 
	}



	private void hideButtons() {
		button_cancel.Sensitive = false;
		button_close.Sensitive = true;
		button_finish.Sensitive = false;
	}


	// simple and DJ jump	
	public void EventEnded(double tv, double tc) {
		hideButtons();
		prepareGraph(tv, tc);
	}
	
	// Reactive jump 
	public void EventEnded(string tvString, string tcString) {
		hideButtons();
		prepareGraph(tvString, tcString);
	}
	
	// simple and DJ jump	
	private void prepareGraph(double tv, double tc) {
		Console.Write("k1");
		
		//obtain data
		double tvPersonAVG = SqliteJump.SelectAllEventsOfAType(sessionID, personID, tableName, eventType, "TV");
		double tvSessionAVG = SqliteJump.SelectAllEventsOfAType(sessionID, -1, tableName, eventType, "TV");

		double tcPersonAVG = 0; 
		double tcSessionAVG = 0; 
		if(tc > 0) {
			tcPersonAVG = SqliteJump.SelectAllEventsOfAType(sessionID, personID, tableName, eventType, "TC");
			tcSessionAVG = SqliteJump.SelectAllEventsOfAType(sessionID, -1, tableName, eventType, "TC");
		}
		
		//paint graph
		paintJumpSimple (drawingarea, tv, tvPersonAVG, tvSessionAVG, tc, tcPersonAVG, tcSessionAVG);
		
		Console.Write("k2");
		
		// -- refresh
		drawingarea.QueueDraw();
		
		Console.Write("k3");
		
		label_tc.Show();
		label_tf.Show();
		table_simple_jump_values.Show();
	}
	
	// Reactive jump 
	private void prepareGraph(string tvString, string tcString) {
		label_tc.Hide();
		label_tf.Hide();
		table_simple_jump_values.Hide();
		
		Console.Write("l1");

		//search AVGs and MAXs
		double maxValue = Util.GetMax(tvString);
		double maxTC = Util.GetMax(tcString);
		if(maxTC > maxValue)
			maxValue = maxTC;
		int jumps = Util.GetNumberOfJumps(tvString, true); 

		//paint graph
		paintJumpReactive (drawingarea, tvString, tcString, Util.GetAverage(tvString), Util.GetAverage(tcString), maxValue, jumps);
		
		Console.Write("l2");
		
		// -- refresh
		drawingarea.QueueDraw();
		
		Console.Write("l3");
	}

	
	void on_finish_clicked (object o, EventArgs args)
	{
		//event will be raised, and managed in chronojump.cs
		//see ButtonFinish at end of file
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

	
/* some tests and code	

	//projecte cubevirtual de juan gonzalez
	
	Gdk.GC pen_rojo;
	Gdk.GC pen_azul;
	Gdk.GC pen_negro;
	Gdk.GC pen_blanco;
	
	protected void paintSomething()
	{
	}

	void configurar_colores()
	{
		//-- Configurar los colores
		Gdk.Color rojo = new Gdk.Color(0xff,0,0);
		Gdk.Color azul  = new Gdk.Color(0,0,0xff);
		Gdk.Color negro = new Gdk.Color(0,0,0);
		Gdk.Color blanco = new Gdk.Color(0xff,0xff,0xff);

		Gdk.Colormap colormap = Gdk.Colormap.System;
		colormap.AllocColor (ref rojo, true, true);
		colormap.AllocColor (ref azul,true,true);
		colormap.AllocColor (ref negro,true,true);
		colormap.AllocColor (ref blanco,true,true);

		//-- Configurar los contextos graficos (pinceles)
		pen_rojo = new Gdk.GC(drawingarea.GdkWindow);
		pen_azul = new Gdk.GC(drawingarea.GdkWindow);
		pen_negro = new Gdk.GC(drawingarea.GdkWindow);
		pen_blanco= new Gdk.GC(drawingarea.GdkWindow);

		pen_rojo.Foreground = rojo;
		pen_azul.Foreground = azul;
	}
*/

	
	//old code
	 
	/*
	public double ProgressbarPreSet(Gtk.Progressbar progressbar, double myValue) 
	{
	}
		
	public double ProgressbarSet(Gtk.Progressbar progressbar, double myValue) 
	{
		progressbar.Text = Util.TrimDecimals(myValue.ToString(), pDN);
		if(myValue > 1.0) myValue = 1.0;
		else if(myValue < 0) myValue = 0;
		progressbar.Fraction = myValue;
	}
	*/

	/*
	public double ProgressbarTvTcCurrent
	{
		set { 
			if(value > 1) {
				progressbar_tv_tc_current_1up.Text = Util.TrimDecimals(value.ToString(), pDN);
				if(value > 4.0) value = 4.0;
				else if(value <= 0) value = 0.01; //fix the div by 0 bug
				progressbar_tv_tc_current_1up.Fraction = value/4;
				progressbar_tv_tc_current_0.Fraction = 1;
				progressbar_tv_tc_current_0.Text = "";
			} else {
				progressbar_tv_tc_current_1up.Fraction = 0;
				progressbar_tv_tc_current_1up.Text = "";
				progressbar_tv_tc_current_0.Text = Util.TrimDecimals(value.ToString(), pDN);
				if(value > 1.0) value = 1.0;
				else if(value < 0) value = 0;
				progressbar_tv_tc_current_0.Fraction = value;
			}
		}
	}

	public double ProgressbarTvTcAvg
	{
		set { 
			if(value > 0) {
				progressbar_tv_tc_avg_1up.Text = Util.TrimDecimals(value.ToString(), pDN);
				if(value > 4.0) value = 4.0;
				else if(value <= 0) value = 0.01; //fix the div by 0 bug
				progressbar_tv_tc_avg_1up.Fraction = value/4;
				progressbar_tv_tc_avg_0.Fraction = 1;
				progressbar_tv_tc_avg_0.Text = "";
			} else {
				progressbar_tv_tc_avg_1up.Fraction = 0;
				progressbar_tv_tc_avg_1up.Text = "";
				progressbar_tv_tc_avg_0.Text = Util.TrimDecimals(value.ToString(), pDN);
				if(value > 1.0) value = 1.0;
				else if(value < 0) value = 0;
				progressbar_tv_tc_avg_0.Fraction = value;
			}
		}
	}
	*/

}
