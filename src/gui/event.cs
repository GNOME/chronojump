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



//--------------------------------------------------------
//---------------- EVENT EXECUTE WIDGET ----------------
//--------------------------------------------------------

public class EventExecuteWindow 
{
	[Widget] Gtk.Window event_execute;
	
	[Widget] Gtk.Label label_person;
	[Widget] Gtk.Label label_event_type;
	[Widget] Gtk.Label label_sub_event_name;
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

	int pDN;
	double limit;
	//private bool simulated;

	/*
	bool jumpsLimited;
	static bool showTvTc = false;
	*/
	
	static EventExecuteWindow EventExecuteWindowBox;
		
	EventExecuteWindow () {
		Glade.XML gladeXML;
		try {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "event_execute", null);
		} catch {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade.chronojump.glade", "event_execute", null);
		}

		gladeXML.Autoconnect(this);
		
		//in first rj jump in a session, always doesn't show the tv/tc
		//showTvTc = false;
	}

	static public EventExecuteWindow Show (string windowTitle, string eventName, string personName, string eventType, int pDN, double limit, bool simulated)
	{
		if (EventExecuteWindowBox == null) {
			EventExecuteWindowBox = new EventExecuteWindow (); 
		}
		
		//initialize global inherited variables
		EventExecuteWindowBox.initializeVariables (windowTitle, eventName, personName, eventType, pDN, limit, simulated);
		//initialize specific variables
		//EventExecuteWindowBox.initializeSpecificVariables (limit);
	
		EventExecuteWindowBox.event_execute.Show ();

		return EventExecuteWindowBox;
	}

	//protected void initializeVariables (string personName, string eventType, int pDN) 
	void initializeVariables (string windowTitle, string eventName, string personName, string eventType, int pDN, double limit, bool simulated) 
	{
		event_execute.Title = windowTitle;
		this.label_sub_event_name.Text = eventName; //"Jumps", "Runs" or "Ticks"
		this.label_person.Text = personName;
		this.label_event_type.Text = eventType;
		this.pDN = pDN;
		this.limit = limit;

		if(simulated)
			label_simulated.Show();
		else
			label_simulated.Hide();
			

		button_finish.Sensitive = true;
		button_cancel.Sensitive = true;
		button_close.Sensitive = false;
	}
	
	public virtual void EventEndedHideButtons() {
		button_cancel.Sensitive = false;
		button_close.Sensitive = true;
	}
	
	void on_finish_clicked (object o, EventArgs args)
	{
		//event will be raised, and managed in chronojump.cs
	}

	void on_button_cancel_clicked (object o, EventArgs args)
	{
		//event will be raised, and managed in chronojump.cs
		EventEndedHideButtons();
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
		
		//JumpRjExecuteWindowBox.jump_rj_execute.Hide();
		//JumpRjExecuteWindowBox = null;
		EventExecuteWindowBox.event_execute.Hide();
		EventExecuteWindowBox = null;
	}
	
	public void ProgressbarEventOrTimePreExecution (bool isEvent, bool percentageMode, double events) 
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

	public Button ButtonCancel 
	{
		get { return button_cancel; }
	}
	
	public Button ButtonFinish 
	{
		get { return button_finish; }
	}
}

