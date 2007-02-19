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
using System.Data;
using Mono.Data.SqliteClient;
using System.Text; //StringBuilder

using System.Threading;
using System.IO.Ports;


public class EventExecute 
{
	protected int personID;
	protected string personName;
	protected int sessionID;
	protected int uniqueID;
	protected string type;
	protected string description;

	//for finishing earlier from chronojump.cs
	protected bool finish;
	
	protected Thread thread;
	//platform state variables
	protected enum States {
		ON,
		OFF
	}

	//don't make the waitEvent update the progressBars, just flag this variable
	//and make the PulseGTK do it
	protected bool needUpdateEventProgressBar;
	protected UpdateProgressBar updateProgressBar; //instance with the info to update
	
	//also for the sensitive of finish button on jumpsReactive, runsInterval and pulses
	protected bool needSensitiveButtonFinish;
	//also for the graph creation	
	protected bool needUpdateGraph;
	protected enum eventType {
		JUMP, JUMPREACTIVE, RUN, RUNINTERVAL, PULSE
	}
	protected eventType needUpdateGraphType;
	
	protected PrepareEventGraphJumpSimple prepareEventGraphJumpSimple; //instance with the info to create
	protected PrepareEventGraphJumpReactive prepareEventGraphJumpReactive; //instance with the info to create
	protected PrepareEventGraphRunSimple prepareEventGraphRunSimple; //instance with the info to create
	protected PrepareEventGraphRunInterval prepareEventGraphRunInterval; //instance with the info to create
	protected PrepareEventGraphPulse prepareEventGraphPulse; //instance with the info to create
	
	protected bool needEndEvent;



	//better as private and don't inherit, don't know why
	//protected Chronopic cp;
	//private Chronopic cp;
	
	//a timer for controlling the time between events and update the progressbar
	//timer has a delegate that updates the time progressBar. 
	//It starts when the first event is detected
	//protected System.Timers.Timer timerClock = new System.Timers.Timer();    
	protected double timerCount; // every 50 milliseconds: 

	protected Random rand;
	protected bool simulated;
	protected double simulatedTimeAccumulatedBefore; //the time that passed since we started recording time
	protected double simulatedTimeLast; //time in last simulated change
	protected double simulatedContactTimeMin; //minimum time we accept for a new wimulated change in platform
	protected double simulatedContactTimeMax; //max time we accept for a new wimulated change in platform
	protected double simulatedFlightTimeMin; //minimum time we accept for a new wimulated change in platform
	protected double simulatedFlightTimeMax; //max time we accept for a new wimulated change in platform
	protected bool simulatedCurrentTimeIntervalsAreContact; //boolean that says if we are lloking to contact or to flight
								//changes every time changes platformState

	protected Chronopic.Plataforma platformState;
	
	protected States loggedState;		//log of last state
	//protected Gtk.ProgressBar progressBar;
	protected Gtk.Statusbar appbar;
	protected Gtk.Window app;
	protected int pDN;

	//for raise a signal and manage it on chronojump.cs
	protected Gtk.Button fakeButtonFinished;
	
	//for cancelling from chronojump.cs
	protected bool cancel;

	//cancel doesn't finish until platform is touched (after pressing cancel button)
	//this variable controls that platform has been touched
	//if not, it wiil shown a popup from chronojump.cs (on_cancel_clicked)	
	protected bool totallyCancelled;



	protected EventExecuteWindow eventExecuteWin;
	
	protected Event eventDone;
	
	public EventExecute() {
		simulated = false;
		eventDone = new Event();
	}
	
	//public virtual void Simulate(Random randSent)
	public virtual void SimulateInitValues(Random randSent)
	{
		Console.WriteLine("From event.cs");

		//look at the override on jump.cs for a sample
		
		rand = randSent; //we send the random, because if we create here, the values will be the same for each nbew instance
		simulated = true;
		simulatedTimeAccumulatedBefore = 0;
		simulatedTimeLast = 0;
		simulatedContactTimeMin = 0; //seconds
		simulatedContactTimeMax = 0; //seconds
		simulatedFlightTimeMin = 0; //seconds
		simulatedFlightTimeMax = 0; //seconds
		simulatedCurrentTimeIntervalsAreContact = false;
	}

	//protected virtual Chronopic.Plataforma chronopicInitialValue(Chronopic cp)
	protected Chronopic.Plataforma chronopicInitialValue(Chronopic cp)
	{
		Chronopic.Plataforma myPlatformState  = Chronopic.Plataforma.UNKNOW; //on (in platform), off (jumping), or unknow
		bool ok = false;
		Console.WriteLine("A1");


int conta=0;



		do {
			Console.WriteLine("B");
			try {
				ok = cp.Read_platform(out myPlatformState);
			} catch {
				Console.WriteLine("Manage called after finishing constructor, do later");
			}
			Console.WriteLine("C");



conta++;
if(conta >3) {
	Console.WriteLine("Exceeded!");
	return myPlatformState;
}





		} while (! ok);

		return myPlatformState;
	}
	
	//public virtual void Manage(object o, EventArgs args)
	public virtual void Manage()
	{
	}
	
	//for calling it again after a confirmWindow says that you have to be in or out the platform
	//and press ok button
	//This method is for not having problems with the parameters of the delegate
	protected void callAgainManage(object o, EventArgs args) {
		Manage();
	}
	
	public virtual void ManageFall()
	{
	}
	

	protected virtual void waitEvent () {
	}
	
	protected bool PulseGTK ()
	{
		onTimer();


		//thread is (in jump, as an example), started in Manage:
		//thread = new Thread(new ThreadStart(waitEvent));
		//GLib.Idle.Add (new GLib.IdleHandler (PulseGTK));
		//thread.Start(); 
		//
		//when waitEvent it's done (with success, for example)
		//then thread is dead

		if ( ! thread.IsAlive || cancel) {
			//if(progressBar.Fraction == 1 || cancel) {
				Console.Write("dying");

				//event will be raised, and managed in chronojump.cs
				//fakeButtonFinished.Click();
				//Now called on write(), now work in mono1.1.6
				
				return false;
			}
			Thread.Sleep (50);
			Console.Write(thread.ThreadState);
			return true;
		//}
		//return false;
	}

	protected void initializeTimer () {
		//put onTimer count to 0 for moving the time progressBar (activiy mode)	
		//also in simulated, allows to change platform state
		timerCount = 0;
	}
		
	//onTimer allow to update progressbar_time every 50 milliseconds
	//also can change platform state in simulated mode
	//protected void onTimer( Object source, ElapsedEventArgs e )
	protected void onTimer( )
	{
		timerCount = timerCount + .05; //0,05 segons == 50 milliseconds, time between each call of onTimer
		
		/* this will be good for not continue counting the time on eventWindow when event has finished
		 * this will help to sync chronopic data with the timerCount data
		 * later also, copy the value of the chronopic to the timerCount label
		 */
		if(needEndEvent) {
			eventExecuteWin.EventEnded();
			//needEndEvent = false;
		} else 
			updateTimeProgressBar();
		
		
		if(simulated) {
			eventSimulatedShouldChangePlatform();
		}

		if(needUpdateEventProgressBar) {
Console.Write("wwa ");				
			//update event progressbar
			eventExecuteWin.ProgressBarEventOrTimePreExecution(
					updateProgressBar.IsEvent,
					updateProgressBar.PercentageMode,
					updateProgressBar.ValueToShow
					);  

			needUpdateEventProgressBar = false;
Console.Write("wwb ");				
		}
		
	
		if(needUpdateGraph) {
			updateGraph();
			needUpdateGraph = false;
		}
		
		if(needSensitiveButtonFinish) {
			eventExecuteWin.ButtonFinishMakeSensitive();
			needSensitiveButtonFinish = false;
		}
		
		//check if it should finish by time
		if(shouldFinishByTime()) {
			finish = true;
			updateProgressBarForFinish();
		} 
		//else 
		//	updateTimeProgressBar();
	}
			
	//check if we should simulate an arriving or leaving the platform depending on random time values
	protected virtual void eventSimulatedShouldChangePlatform() 
	{
		double timeMax = 0;
		double timeMin = 0;
		if(simulatedCurrentTimeIntervalsAreContact) {
			timeMax = simulatedContactTimeMax;
			timeMin = simulatedContactTimeMin;
		} else {
			timeMax = simulatedFlightTimeMax;
			timeMin = simulatedFlightTimeMin;
		}
		
		//if the time is too much, finish
		if(timerCount - simulatedTimeAccumulatedBefore > timeMax) {
				Console.WriteLine("EXCEEDES MAX!");
				simulateChangePlatform();
		}
		
		//some random for finishing between timeMin and timeMax
		if(timerCount - simulatedTimeAccumulatedBefore >= timeMin)
		{
			double simulatedRange = timeMax - timeMin;
			//rand.NextDouble gives a value between 0 and 1
			//if we multiply by the (simulatedRange * 10 +1), then we will have 4 options if the range is ,4
			//check if the value is less than 1 (it's one change in four options) and if it's 1, then simulated the change platform
			//double dice = 0;
			double myRand = rand.NextDouble();
			double dice = myRand * (simulatedRange *10 +1);
			Console.WriteLine("rand: {0}, dice: {1}", myRand, dice);
			if (dice < 1)
			{
				simulateChangePlatform();
			}
		}
	}

	protected void simulateChangePlatform() {
		Console.Write("Changing!");
		Console.WriteLine("PRE timeLast: {0}, timerCount: {1}, timeAccumulated: {2}", simulatedTimeLast, timerCount, simulatedTimeAccumulatedBefore);

		simulatedTimeLast = timerCount - simulatedTimeAccumulatedBefore;
		//simulatedTimeAccumulatedBefore = timerCount;
		simulatedTimeAccumulatedBefore = Math.Round(timerCount,2);
		Console.WriteLine("POST: timeLast: {0}, timerCount: {1}, timeAccumulated: {2}", simulatedTimeLast, timerCount, simulatedTimeAccumulatedBefore);

		//change the boolean who points to 'which are the MINs and the MAXs
		simulatedCurrentTimeIntervalsAreContact = ! simulatedCurrentTimeIntervalsAreContact;
		
		if(platformState == Chronopic.Plataforma.ON)
			platformState = Chronopic.Plataforma.OFF;
		else
			platformState = Chronopic.Plataforma.ON;

		Console.WriteLine("Changed!");
	}
			
	private void updateGraph() {
		switch(needUpdateGraphType) {
			case eventType.JUMP:
				Console.Write("update graph: JUMP");
				eventExecuteWin.PrepareJumpSimpleGraph(
						prepareEventGraphJumpSimple.tv, 
						prepareEventGraphJumpSimple.tc);
				break;
			case eventType.JUMPREACTIVE:
				Console.Write("update graph: JUMPREACTIVE");
				eventExecuteWin.PrepareJumpReactiveGraph(
						prepareEventGraphJumpReactive.lastTv, 
						prepareEventGraphJumpReactive.lastTc,
						prepareEventGraphJumpReactive.tvString,
						prepareEventGraphJumpReactive.tcString);
				break;
			case eventType.RUN:
				Console.Write("update graph: RUN");
				eventExecuteWin.PrepareRunSimpleGraph(
						prepareEventGraphRunSimple.time, 
						prepareEventGraphRunSimple.speed);
				break;
			case eventType.RUNINTERVAL:
				Console.Write("update graph: RUNINTERVAL");
				eventExecuteWin.PrepareRunIntervalGraph(
						prepareEventGraphRunInterval.distance, 
						prepareEventGraphRunInterval.lastTime,
						prepareEventGraphRunInterval.timesString);
				break;
			case eventType.PULSE:
				Console.Write("update graph: PULSE");
				eventExecuteWin.PreparePulseGraph(
						prepareEventGraphPulse.lastTime, 
						prepareEventGraphPulse.timesString);
				break;
		}
	}
	
	protected virtual bool shouldFinishByTime() {
		return true;
	}

	//called by the GTK loop (can call eventExecuteWin directly
	protected virtual void updateProgressBarForFinish() {
	}
	
	//called by the GTK loop (can call eventExecuteWin directly
	protected virtual void updateTimeProgressBar() {
	}
	
	protected virtual void write() {
	}
	
	//from confirm_window cancel button (thread has not started)
	//this is NOT called when a event has started and user click on "Cancel"
	protected void cancel_event(object o, EventArgs args)
	{
		cancel = true;
		
		//event will be raised, and managed in chronojump.cs
		fakeButtonFinished.Click();
	}
	
	public Gtk.Button FakeButtonFinished
	{
		get {
			return	fakeButtonFinished;
		}
	}

	//called from chronojump.cs for finishing events earlier
	public bool Finish
	{
		get { return finish; }
		set { finish = value; }
	}
	
	//called from chronojump.cs for cancelling events
	public bool Cancel
	{
		get { return cancel; }
		set { cancel = value; }
	}

	public bool TotallyCancelled
	{
		get { return totallyCancelled; }
		set { totallyCancelled = value; }
	}

	public Event EventDone {
		get { return eventDone; }
	}
	
	~EventExecute() {}
	   
}

