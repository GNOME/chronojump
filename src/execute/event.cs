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
 */

using System;
using System.Data;
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
		JUMP, JUMPREACTIVE, RUN, RUNINTERVAL, PULSE, REACTIONTIME, MULTICHRONOPIC
	}
	protected eventType needUpdateGraphType;
	
	protected string syncMessage;
	protected bool needShowSyncMessage;

	//instances with the info to create
	protected PrepareEventGraphJumpSimple prepareEventGraphJumpSimple; 
	protected PrepareEventGraphJumpReactive prepareEventGraphJumpReactive;
	protected PrepareEventGraphRunSimple prepareEventGraphRunSimple;
	protected PrepareEventGraphRunInterval prepareEventGraphRunInterval;
	protected PrepareEventGraphPulse prepareEventGraphPulse;
	protected PrepareEventGraphReactionTime prepareEventGraphReactionTime;
	protected PrepareEventGraphMultiChronopic prepareEventGraphMultiChronopic;
	
	protected bool needEndEvent;
	
	protected bool volumeOn;
	protected RepetitiveConditionsWindow repetitiveConditionsWin;


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
	
	protected int timesForSavingRepetitive; //number of times that this repetive event needs for being recorded in temporal table

	//for raise a signal and manage it on chronojump.cs
	protected Gtk.Button fakeButtonFinished;
	
	//for cancelling from chronojump.cs
	protected bool cancel;

	//cancel doesn't finish until platform is touched (after pressing cancel button)
	//this variable controls that platform has been touched
	//if not, it will shown a popup from gui/chronojump.cs (on_cancel_clicked)	
	protected bool totallyCancelled;

	//for finishing earlier from chronojump.cs
	protected bool finish;
	protected bool totallyFinished;
	
	// multi Chronopic stuff
	protected int chronopics; 
	protected bool totallyFinishedMulti1;
	protected bool totallyFinishedMulti2;
	protected bool totallyFinishedMulti3;
	protected bool totallyFinishedMulti4;
	protected bool totallyCancelledMulti1;
	protected bool totallyCancelledMulti2;
	protected bool totallyCancelledMulti3;
	protected bool totallyCancelledMulti4;
	


	protected EventExecuteWindow eventExecuteWin;
	
	protected Event eventDone;
	
	public EventExecute() {
		simulated = false;
		eventDone = new Event();
	}
	
	//public virtual void Simulate(Random randSent)
	public virtual void SimulateInitValues(Random randSent)
	{
		Log.WriteLine("From event.cs");

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

	protected Chronopic.Plataforma chronopicInitialValue(Chronopic cp)
	{
		Chronopic.Plataforma myPlatformState  = Chronopic.Plataforma.UNKNOW; //on (in platform), off (jumping), or unknow
		bool ok = false;

		do {
			try {
				ok = cp.Read_platform(out myPlatformState);
			} catch {
				Log.WriteLine("Manage called after finishing constructor, do later");
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
			Log.Write("dying");
			return false;
		}
	
		Thread.Sleep (50);
		//Log.Write(thread.ThreadState.ToString());
		return true;
	}

	public void StopThread() {
		/*
		Log.WriteLine("----------ABORTING----------");
		thread.Abort();
		Log.WriteLine("----------ABORTED-----------");
		*/
	}

	protected void initializeTimer () {
		//put onTimer count to 0 for moving the time progressBar (activiy mode)	
		//also in simulated, allows to change platform state
		timerCount = 0;
	}
		
	//onTimer allow to update progressbar_time every 50 milliseconds
	//also can change platform state in simulated mode
	//protected void onTimer( Object source, ElapsedEventArgs e )
	protected virtual void onTimer( )
	{
		timerCount = timerCount + .05; //0,05 segons == 50 milliseconds, time between each call of onTimer
		
		/* this will be good for not continue counting the time on eventWindow when event has finished
		 * this will help to sync chronopic data with the timerCount data
		 * later also, copy the value of the chronopic to the timerCount label
		 */

		//updateTimeProgressBar();
		if(needEndEvent) {
			eventExecuteWin.EventEnded();
			//needEndEvent = false;
			if(needUpdateGraphType == eventType.MULTICHRONOPIC && type == Constants.RunAnalysisName && finish) 
				eventExecuteWin.RunATouchPlatform();
		} else 
			updateTimeProgressBar();
		
		
		if(simulated) {
			eventSimulatedShouldChangePlatform();
		}

		if(needUpdateEventProgressBar) {
			//update event progressbar
			eventExecuteWin.ProgressBarEventOrTimePreExecution(
					updateProgressBar.IsEvent,
					updateProgressBar.PercentageMode,
					updateProgressBar.ValueToShow
					);  

			needUpdateEventProgressBar = false;
		}
		
	Console.WriteLine("pulse update graph");	
		if(needUpdateGraph) {
	Console.WriteLine("pulse update graph 2");	
			//solve problems when runAnalysis ended and tries to paint window
			if(needUpdateGraphType == eventType.MULTICHRONOPIC && type == Constants.RunAnalysisName && finish) 
				Console.WriteLine("is MC, RA, finished!");	
			else if(needUpdateGraphType == eventType.MULTICHRONOPIC && type == Constants.RunAnalysisName && ! finish) {
				Console.WriteLine("is MC, RA, NOT finished!");	
				updateGraph();
			} else
				updateGraph();
	
			Console.WriteLine("pulse update graph 3");	
			needUpdateGraph = false;
		}
	Console.WriteLine("pulse update graph 4");	
		
		if(needSensitiveButtonFinish) {
			eventExecuteWin.ButtonFinishMakeSensitive();
			needSensitiveButtonFinish = false;
		}
		
		if(needShowSyncMessage) {
			eventExecuteWin.ShowSyncMessage(syncMessage);
			needShowSyncMessage = false;
		}
		
		
		//check if it should finish by time
		if(shouldFinishByTime()) {
			finish = true;
			updateProgressBarForFinish();
		} 
	Console.WriteLine("pulse done");	
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
				Log.WriteLine("EXCEEDES MAX!");
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
			Log.WriteLine(string.Format("rand: {0}, dice: {1}", myRand, dice));
			if (dice < 1)
			{
				simulateChangePlatform();
			}
		}
	}

	protected void simulateChangePlatform() {
		Log.Write("Changing!");
		Log.WriteLine(string.Format("PRE timeLast: {0}, timerCount: {1}, timeAccumulated: {2}", simulatedTimeLast, timerCount, simulatedTimeAccumulatedBefore));

		simulatedTimeLast = timerCount - simulatedTimeAccumulatedBefore;
		//simulatedTimeAccumulatedBefore = timerCount;
		simulatedTimeAccumulatedBefore = Math.Round(timerCount,2);
		Log.WriteLine(string.Format("POST: timeLast: {0}, timerCount: {1}, timeAccumulated: {2}", simulatedTimeLast, timerCount, simulatedTimeAccumulatedBefore));

		//change the boolean who points to 'which are the MINs and the MAXs
		simulatedCurrentTimeIntervalsAreContact = ! simulatedCurrentTimeIntervalsAreContact;
		
		if(platformState == Chronopic.Plataforma.ON)
			platformState = Chronopic.Plataforma.OFF;
		else
			platformState = Chronopic.Plataforma.ON;

		Log.WriteLine("Changed!");
	}
			
	private void updateGraph() {
		switch(needUpdateGraphType) {
			case eventType.JUMP:
				Log.Write("update graph: JUMP");
				eventExecuteWin.PrepareJumpSimpleGraph(
						prepareEventGraphJumpSimple.tv, 
						prepareEventGraphJumpSimple.tc);
				break;
			case eventType.JUMPREACTIVE:
				Log.Write("update graph: JUMPREACTIVE");
				eventExecuteWin.PrepareJumpReactiveGraph(
						prepareEventGraphJumpReactive.lastTv, 
						prepareEventGraphJumpReactive.lastTc,
						prepareEventGraphJumpReactive.tvString,
						prepareEventGraphJumpReactive.tcString,
						volumeOn, repetitiveConditionsWin);
				break;
			case eventType.RUN:
				Log.Write("update graph: RUN");
				eventExecuteWin.PrepareRunSimpleGraph(
						prepareEventGraphRunSimple.time, 
						prepareEventGraphRunSimple.speed);
				break;
			case eventType.RUNINTERVAL:
				Log.Write("update graph: RUNINTERVAL");
				eventExecuteWin.PrepareRunIntervalGraph(
						prepareEventGraphRunInterval.distance, 
						prepareEventGraphRunInterval.lastTime,
						prepareEventGraphRunInterval.timesString,
						prepareEventGraphRunInterval.distanceTotal,
						prepareEventGraphRunInterval.distancesString,
						volumeOn, repetitiveConditionsWin);
				break;
			case eventType.PULSE:
				Log.Write("update graph: PULSE");
				eventExecuteWin.PreparePulseGraph(
						prepareEventGraphPulse.lastTime, 
						prepareEventGraphPulse.timesString);
				break;
			case eventType.REACTIONTIME:
				Log.Write("update graph: REACTIONTIME");
				eventExecuteWin.PrepareReactionTimeGraph(
						prepareEventGraphReactionTime.time); 
				break;
			case eventType.MULTICHRONOPIC:
				Log.Write("update graph: MULTICHRONOPIC");
				eventExecuteWin.PrepareMultiChronopicGraph(
						//prepareEventGraphMultiChronopic.timestamp, 
						prepareEventGraphMultiChronopic.cp1StartedIn, 
						prepareEventGraphMultiChronopic.cp2StartedIn, 
						prepareEventGraphMultiChronopic.cp3StartedIn, 
						prepareEventGraphMultiChronopic.cp4StartedIn, 
						prepareEventGraphMultiChronopic.cp1InStr, 
						prepareEventGraphMultiChronopic.cp1OutStr,
						prepareEventGraphMultiChronopic.cp2InStr, 
						prepareEventGraphMultiChronopic.cp2OutStr,
						prepareEventGraphMultiChronopic.cp3InStr, 
						prepareEventGraphMultiChronopic.cp3OutStr,
						prepareEventGraphMultiChronopic.cp4InStr, 
						prepareEventGraphMultiChronopic.cp4OutStr
						);
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


	protected virtual void goodEvent() {
		Util.PlaySound(Constants.SoundTypes.GOOD, volumeOn);
	} 
	
	protected virtual void badEvent() {
		Util.PlaySound(Constants.SoundTypes.BAD, volumeOn);
	} 
	
	//from confirm_window cancel button (thread has not started)
	//this is NOT called when a event has started and user click on "Cancel"
	protected void cancel_event_before_start(object o, EventArgs args)
	{
		cancel = true;
		totallyCancelled = true;
		eventExecuteWin.EventEnded();
		
		//event will be raised, and managed in chronojump.cs
		fakeButtonFinished.Click();
	}
	
	public virtual bool MultiChronopicRunAUsedCP2() {
		return false;
	}
	public virtual void MultiChronopicWrite(bool tempTable) {
		Console.WriteLine("at event.cs");
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

	public bool TotallyFinished
	{
		get { return totallyFinished; }
		set { totallyFinished = value; }
	}

	public Event EventDone {
		get { return eventDone; }
	}
	
	// multi Chronopic stuff
	public int Chronopics { get { return chronopics; } }

	public bool TotallyFinishedMulti1 { get { return totallyFinishedMulti1; } }
	public bool TotallyFinishedMulti2 { get { return totallyFinishedMulti2; } }
	public bool TotallyFinishedMulti3 { get { return totallyFinishedMulti3; } }
	public bool TotallyFinishedMulti4 { get { return totallyFinishedMulti4; } }
	
	public bool TotallyCancelledMulti1 { get { return totallyCancelledMulti1; } }
	public bool TotallyCancelledMulti2 { get { return totallyCancelledMulti2; } }
	public bool TotallyCancelledMulti3 { get { return totallyCancelledMulti3; } }
	public bool TotallyCancelledMulti4 { get { return totallyCancelledMulti4; } }


	~EventExecute() {}
	   
}

