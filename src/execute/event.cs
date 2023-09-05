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
 * Copyright (C) 2004-2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Data;
using System.Text; //StringBuilder
using Gtk;
using System.Threading;
using System.IO.Ports;
using Mono.Unix;


public class EventExecute 
{
	protected int personID;
	protected string personName;
	protected int sessionID;
	protected int uniqueID;
	protected int typeID; //to upload to networks
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
	
	protected bool needCallTrackDone;
	protected bool needCheckIfTrackEnded; //Races with double contacts wait some ms to see if other contact appears
	protected bool needShowCountDown;	//RSA

	//protected string syncMessage;
	//protected bool needShowSyncMessage;
	protected string feedbackMessage;
	protected bool needShowFeedbackMessage;
	protected bool feedbackMessageOnDialog;

	//instances with the info to create
	public PrepareEventGraphJumpSimple PrepareEventGraphJumpSimpleObject; 
	public PrepareEventGraphJumpReactiveRealtimeCapture PrepareEventGraphJumpReactiveRealtimeCaptureObject;
	public PrepareEventGraphRunSimple PrepareEventGraphRunSimpleObject;
	public PrepareEventGraphRunIntervalRealtimeCapture PrepareEventGraphRunIntervalRealtimeCaptureObject;
	public PrepareEventGraphPulse PrepareEventGraphPulseObject;
	public PrepareEventGraphReactionTime PrepareEventGraphReactionTimeObject;
	public PrepareEventGraphMultiChronopic PrepareEventGraphMultiChronopicObject;
	
	protected bool needEndEvent;
	
	protected bool volumeOn;
	protected Preferences.GstreamerTypes gstreamer;
	protected int graphLimit;
	protected bool graphAllTypes;
	protected bool graphAllPersons;
	protected double progressbarLimit;

	protected ExecutingGraphData egd;
	protected Gdk.Color colorBackground;

	//for runs
	public RunPhaseTimeList runPTL;


	//better as private and don't inherit, don't know why
	//protected Chronopic cp;
	//private Chronopic cp;
	
	//a timer for controlling the time between events and update the progressbar
	//timer has a delegate that updates the time progressBar. 
	//It starts when the first event is detected
	//protected System.Timers.Timer timerClock = new System.Timers.Timer();    
	protected double timerCount; // every 50 milliseconds:
	protected DateTime timerStart; // used as timestamp to count better 

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
	protected int pDN;
	
	protected int timesForSavingRepetitive; //number of times that this repetive event needs for being recorded in temporal table

	//for raise a signal and manage it on chronojump.cs
	protected Gtk.Button fakeButtonUpdateGraph;
	//this should be a safer way, because will be called when thread has dyed, then will be the last action in the GTK thread.
	//suitable for calling sensitiveGuiEventDone without problems
	//sensitiveGuiEventDone causes problems on changing (or even reading) properties of gtk stuff outside of gtk thread

	protected Gtk.Button fakeButtonCameraStopIfNeeded;
	protected Gtk.Button fakeButtonThreadDyed;

	//for cancelling from chronojump.cs
	protected bool cancel;

	//for finishing earlier from chronojump.cs
	protected bool finish;
	
	//if chronopic is disconnected by user, port changes, ...
	protected static bool chronopicDisconnected;
	
	// multi Chronopic stuff
	protected int chronopics; 

	protected bool cameraRecording;

	//for reaction time	
	//on animation lights and discriminative should be false
	public bool StartIn = true;
	public Gtk.Button FakeButtonReactionTimeStart;

	//protected EventExecuteWindow eventExecuteWin;
	//protected ChronoJumpWindow app1;
	
	protected Event eventDone;
	
	public EventExecute() {
		simulated = false;
		eventDone = new Event();
	}
	
	//public virtual void Simulate(Random randSent)
	public virtual void SimulateInitValues(Random randSent)
	{
		LogB.Information("From event.cs");

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
		int timeWait = 50; //wait 50ms between calls to Read_platform
		int timeLimit = 1000;
		int count = 0; 

		do {
			try {
				ok = cp.Read_platform(out myPlatformState);
			} catch {
				LogB.Warning("Manage called after finishing constructor, do later");
			}
			Thread.Sleep(timeWait); //wait 50ms
			count += timeWait;
		} while (! ok && count < timeLimit);

		return myPlatformState;
	}
	
	//public virtual void Manage(object o, EventArgs args)
	public virtual bool Manage()
	{
		return true;
	}
	
	//for calling it again after a confirmWindow says that you have to be in or out the platform
	//and press ok button
	//This method is for not having problems with the parameters of the delegate
	protected void callAgainManage(object o, EventArgs args) {
		Manage();
	}
	
	public virtual bool ManageFall()
	{
		return true;
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

		if ( ! thread.IsAlive || cancel)
		{
			//to correctly show the progressbar need to call this until is done
			if (cameraRecording && ! cancel)
			{
				//LogB.Information ("PulseGTK fakeButtonCameraStopIfNeeded.Click");
				fakeButtonCameraStopIfNeeded.Click ();
				return true;
			}

			LogB.ThreadEnding();

			//don't show any of the change image icons
			if (this.GetType ().Equals (typeof (JumpExecute)) ||
					this.GetType ().Equals (typeof (JumpRjExecute)))
				jumpChangeImageForceHide();
			else if (this.GetType ().Equals (typeof (RunExecute)) ||
					this.GetType ().Equals (typeof (RunIntervalExecute)))
				runChangeImageForceHide();

			if (chronopicDisconnected)
				chronopicHasBeenDisconnected ();

			fakeButtonThreadDyed.Click();

			LogB.ThreadEnded(); 
			return false;
		}
	
		Thread.Sleep (50);
		//Thread.Sleep (25);
		//LogB.Debug(thread.ThreadState.ToString());
		return true;
	}

	//used on Chronojump exit (if user wants to close program while it is capturing
	public bool IsThreadRunning() {
		if (thread != null && thread.IsAlive)
			return true;

		return false;
	}

	public void ThreadAbort() {
		thread.Abort();
	}


	protected void initializeTimer () {
		//put onTimer count to 0 for moving the time progressBar (activiy mode)	
		//also in simulated, allows to change platform state
		timerCount = 0;
		timerStart = DateTime.UtcNow;
	}
		
	//onTimer allow to update progressbar_time every 50 milliseconds
	//also can change platform state in simulated mode
	//protected void onTimer( Object source, ElapsedEventArgs e )
	protected virtual void onTimer( )
	{
		//this is not ok because not always is called every 50 milliseconds and then timer gets outdated
		//timerCount = timerCount + .05; //0,05 segons == 50 milliseconds, time between each call of onTimer
	
		//used Utc.Now because is lot faster:
		//http://stackoverflow.com/questions/28637/is-datetime-now-the-best-way-to-measure-a-functions-performance	
		DateTime timerNow = DateTime.UtcNow;
		TimeSpan ts = timerNow.Subtract(timerStart);
		timerCount = ts.TotalSeconds; 
		
		/* this will be good for not continue counting the time on eventWindow when event has finished
		 * this will help to sync chronopic data with the timerCount data
		 * later also, copy the value of the chronopic to the timerCount label
		 */

		//updateTimeProgressBar();
		if(needEndEvent) {
			//app1.EventEnded();
			//needEndEvent = false;
			if(needUpdateGraphType == eventType.MULTICHRONOPIC && type == Constants.RunAnalysisName && finish) 
				//app1.RunATouchPlatform();
				//fakeButtonRunATouchPlatform.Click();
				runATouchPlatform();
		} else 
			updateTimeProgressBar();
		
		
		if(simulated) {
			eventSimulatedShouldChangePlatform();
		}

		if(needUpdateEventProgressBar) {
			//update event progressbar
			//app1.ProgressBarEventOrTimePreExecution(
			progressBarEventOrTimePreExecution(
					updateProgressBar.IsEvent,
					updateProgressBar.PercentageMode,
					updateProgressBar.ValueToShow
					);  

			needUpdateEventProgressBar = false;
		}
		
		if(needUpdateGraph) {
			//solve problems when runAnalysis ended and tries to paint window
			if(needUpdateGraphType == eventType.MULTICHRONOPIC && type == Constants.RunAnalysisName && finish) 
				LogB.Information("is MC, RA, finished!");	
			else if(needUpdateGraphType == eventType.MULTICHRONOPIC && type == Constants.RunAnalysisName && ! finish) {
				LogB.Information("is MC, RA, NOT finished!");	
				updateGraph();
			} else
				updateGraph();
	
			needUpdateGraph = false;
		}
		
		if(needSensitiveButtonFinish) {
			//ButtonFinishMakeSensitive();
			egd.Button_finish.Sensitive = true;
			needSensitiveButtonFinish = false;
		}

		// jump specific --------------------------------->
		if (this.GetType ().Equals (typeof (JumpExecute)) ||
				this.GetType ().Equals (typeof (JumpRjExecute)))
			jumpChangeImageIfNeeded ();

		// races specific --------------------------------->

		//TODO: pass mode and only do what related to mode

		if (this.GetType ().Equals (typeof (RunExecute)) ||
				this.GetType ().Equals (typeof (RunIntervalExecute)))
		{
			runChangeImageIfNeeded ();

			updateRunPhaseInfoManage();

			//Race track with DoubleContacts mode NONE
			if(needCallTrackDone)
			{
				trackDone();
				needCallTrackDone = false;
			}
			//Race track with DoubleContacts mode != NONE
			//LogB.Information("needCheckIfTrackEnded: " + needCheckIfTrackEnded.ToString());
			if(needCheckIfTrackEnded && lastTfCheckTimeEnded())
			{
				if(trackDone())
				{
					//LogB.Information("needCheckIfTrackEnded changing to false");
					needCheckIfTrackEnded = false;
				} else {
					//LogB.Information("needCheckIfTrackEnded continue true, trackDone() will be called again");
					//this helps to fix when contact time display when it is bigger than double contact time * 1.5
				}
			}

			//RSA
			if(needShowCountDown)
			{
				feedbackMessage = countDownMessage();
				UtilGtk.PrintLabelWithTooltip(egd.Label_message, feedbackMessage);
			}
		}

		// <-------------------------- end of races specific

		if(! needShowCountDown && needShowFeedbackMessage)
		{
			if (feedbackMessageOnDialog)
			{
				new DialogMessage(Constants.MessageTypes.WARNING, feedbackMessage);
				feedbackMessageOnDialog = false;
			}
			UtilGtk.PrintLabelWithTooltip(egd.Label_message, feedbackMessage);
			needShowFeedbackMessage = false;
		}
		
		//check if it should finish by time
		if(shouldFinishByTime()) {
			finish = true;
			updateProgressBarForFinish();
		} 
	}
	
	// races specific --------------------------------->

	private void runATouchPlatform() {
		UtilGtk.PrintLabelWithTooltip(egd.Label_message, feedbackMessage);
	}

	public void RunANoStrides() {
		UtilGtk.PrintLabelWithTooltip(egd.Label_message, feedbackMessage);
	}

	protected virtual bool lastTfCheckTimeEnded()
	{
		return true;
	}

	protected virtual void jumpChangeImageIfNeeded()
	{
	}
	protected virtual void runChangeImageIfNeeded()
	{
	}

	protected virtual void jumpChangeImageForceHide()
	{
	}
	protected virtual void runChangeImageForceHide()
	{
	}

	protected virtual void updateRunPhaseInfoManage()
	{
	}

	protected virtual bool trackDone()
	{
		return true;
	}

	// <-------------------------- end of races specific

	protected void progressBarEventOrTimePreExecution (bool isEvent, bool percentageMode, double events) 
	{
		if (isEvent) 
			progressbarEventOrTimeExecution (egd.Progressbar_event, percentageMode, egd.Label_event_value, events);
		else
			progressbarEventOrTimeExecution (egd.Progressbar_time, percentageMode, egd.Label_time_value, events);
	}

	private void progressbarEventOrTimeExecution (Gtk.ProgressBar progressbar, bool percentageMode, Gtk.Label label_value, double events)
	{
		if(progressbarLimit == -1) {	//unlimited event (until 'finish' is clicked)
			progressbar.Pulse();
			//label_value.Text = events.ToString();
			if(events != -1)
				label_value.Text = Math.Round(events,3).ToString();
		} else {
			if(percentageMode) {
				double myFraction = events / progressbarLimit *1.0;

				if(myFraction > 1)
					myFraction = 1;
				else if(myFraction < 0)
					myFraction = 0;

				progressbar.Fraction = myFraction;
				//progressbar.Text = Util.TrimDecimals(events.ToString(), 1) + " / " + progressbarLimit.ToString();
				if(events == -1) //we don't want to display nothing
					//progressbar.Text = "";
					label_value.Text = "";
				else
					label_value.Text = Math.Round(events,3).ToString();
			} else {
				//activity mode
				progressbar.Pulse();

				//pass -1 in events in activity mode if don't want to use this label
				if(events != -1) {
					//label_value.Text = Util.TrimDecimals(events.ToString(), 1);
					label_value.Text = Math.Round(events,3).ToString();
				}
			}
		}
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
				simulateChangePlatform();
		}
		
		//some random for finishing between timeMin and timeMax
		if(timerCount - simulatedTimeAccumulatedBefore >= timeMin)
		{
			double simulatedRange = timeMax - timeMin;
			//rand.NextDouble gives a value between 0 and 1
			//if we multiply by the (simulatedRange * 10 +1), then we will have 4 options if the range is ,4
			//check if the value is less than 1 (it's one change in four options) and if it's 1, then simulated the change platform
			double myRand = rand.NextDouble();
			double dice = myRand * (simulatedRange *10 +1);
			if (dice < 1)
			{
				simulateChangePlatform();
			}
		}
	}

	protected void simulateChangePlatform() {
		LogB.Information("Changing!");
		LogB.Information(string.Format("PRE timeLast: {0}, timerCount: {1}, timeAccumulated: {2}", simulatedTimeLast, timerCount, simulatedTimeAccumulatedBefore));

		simulatedTimeLast = timerCount - simulatedTimeAccumulatedBefore;
		//simulatedTimeAccumulatedBefore = timerCount;
		simulatedTimeAccumulatedBefore = Math.Round(timerCount,2);
		LogB.Information(string.Format("POST: timeLast: {0}, timerCount: {1}, timeAccumulated: {2}", simulatedTimeLast, timerCount, simulatedTimeAccumulatedBefore));

		//change the boolean who points to 'which are the MINs and the MAXs
		simulatedCurrentTimeIntervalsAreContact = ! simulatedCurrentTimeIntervalsAreContact;
		
		if(platformState == Chronopic.Plataforma.ON)
			platformState = Chronopic.Plataforma.OFF;
		else
			platformState = Chronopic.Plataforma.ON;

		LogB.Information("Changed!");
	}
			
	private void updateGraph() {
		fakeButtonUpdateGraph.Click();
	}
	
	protected virtual string countDownMessage() {
		return "";
	}
	
	protected virtual bool shouldFinishByTime() {
		return true;
	}

	//called by the GTK loop (can call app1 directly
	protected virtual void updateProgressBarForFinish() {
	}
	
	//called by the GTK loop (can call app1 directly
	protected virtual void updateTimeProgressBar() {
	}
	
	protected virtual void write() {
	}

	protected virtual void goodEvent() {
		Util.PlaySound(Constants.SoundTypes.GOOD, volumeOn, gstreamer);
	} 
	
	protected virtual void badEvent() {
		Util.PlaySound(Constants.SoundTypes.BAD, volumeOn, gstreamer);
	} 
	
	public virtual void Manage2() {
	}

	public virtual string GetInspectorMessages()
	{
		return "";
	}

	public RunPhaseTimeList RunPTL
	{
		get { return runPTL; }
	}

	//from confirm_window cancel button (thread has not started)
	//this is NOT called when a event has started and user click on "Cancel"
	protected void cancel_event_before_start (object o, EventArgs args)
	{
		cancel = true;
			
		//event will be raised, and managed in chronojump.cs
		//calls sensitiveGuiEventDone()
		fakeButtonThreadDyed.Click();
	}

	protected void chronopicHasBeenDisconnected ()
	{
		chronopicDisconnected = true;
		/*
		ErrorWindow errorWin;		
		errorWin = ErrorWindow.Show( 
				Catalog.GetString("Chronopic seems disconnected."));
				*/
		new DialogMessage (Constants.MessageTypes.WARNING, Catalog.GetString("Chronopic seems disconnected."));

		Util.PlaySound(Constants.SoundTypes.BAD, volumeOn, gstreamer);
		//errorWin.Button_accept.Clicked += new EventHandler(cancel_event_before_start);
		cancel_event_before_start (new object (), new EventArgs ());
	}
	
	public virtual bool MultiChronopicRunAUsedCP2() {
		return false;
	}
	public virtual void MultiChronopicWrite(bool tempTable) {
		LogB.Information("at event.cs");
	}

	public Gtk.Button FakeButtonUpdateGraph {
		get { return fakeButtonUpdateGraph; }
	}

	public Gtk.Button FakeButtonCameraStopIfNeeded {
		get { return fakeButtonCameraStopIfNeeded; }
	}

	public Gtk.Button FakeButtonThreadDyed {
		get { return fakeButtonThreadDyed; }
	}

	//public Gtk.Button FakeButtonRunATouchPlatform {
	//	get { return fakeButtonRunATouchPlatform; }
	//}

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

	public bool ChronopicDisconnected
	{
		get { return chronopicDisconnected; }
	}

	public bool CameraRecording
	{
		set { cameraRecording = value; }
	}

	public Event EventDone {
		get { return eventDone; }
	}

	// multi Chronopic stuff
	public int Chronopics { get { return chronopics; } }


	~EventExecute() {}
	   
}

//currently only used for photocells
public class PhaseTime
{
	private int photocell; //on not inalambric is -1
	private bool contactIn;
	private double duration;

	public PhaseTime (int photocell, bool contactIn, double duration)
	{
		this.photocell = photocell;
		this.contactIn = contactIn;
		this.duration = duration;
	}

	public override string ToString()
	{
		string strMode = "IN (TC)";
		if(! contactIn)
			strMode = "OUT (TF)";

		string photocellStr = "";
		if(photocell >= 0)
			photocellStr = string.Format(" - {0}", photocell);

		//TODO: use a printf mode to have always same digits
		return "\n" + Math.Round(UtilAll.DivideSafe(duration, 1000.0), 3) + photocellStr + " - " + strMode;
	}

	public int Photocell
	{
		get { return photocell; }
	}
	public bool IsContact
	{
		get { return contactIn; }
	}
	public double Duration
	{
		get { return duration; }
	}
}

//currently only used for photocells
public class InOut
{
	private int photocell; // -1 on regular photocells (all the same)
	private bool contactIn;
	private DateTime dt;
	private string message;

	public InOut (int photocell, bool contactIn, DateTime dt, string message)
	{
		this.photocell = photocell;
		this.contactIn = contactIn;
		this.dt = dt;
		this.message = message;
	}

	public override string ToString()
	{
		string str = "\n- ";
		if(photocell >= 0)
			str += string.Format("[{0}] ", photocell);

		if(contactIn)
			str += "IN / ";
		else
			str += "OUT / ";

		str += dt.ToShortTimeString();
		if(message != "")
			str += " / " + message;

		return str;
	}
}
