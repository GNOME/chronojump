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
 * Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using System.Text; //StringBuilder

using System.Threading;
using System.IO.Ports;
using Mono.Unix;

public class ReactionTimeExecute : EventExecute
{
	protected double time;

	//better as private and don't inherit, don't know why
	//protected Chronopic cp;
	private Chronopic cp;

	public ReactionTimeExecute() {
	}

	//reactionTime execution
	public ReactionTimeExecute(int personID, string personName, int sessionID, string type,
			Chronopic cp, int pDN,
			bool volumeOn, Preferences.GstreamerTypes gstreamer,
			double progressbarLimit, ExecutingGraphData egd, string description,
			bool cameraRecording
			)
	{
		this.personID = personID;
		this.personName = personName;
		this.sessionID = sessionID;
		this.type = type;
		
		this.cp = cp;

		this.pDN = pDN;
		this.volumeOn = volumeOn;
		this.gstreamer = gstreamer;
		this.progressbarLimit = progressbarLimit;
		this.egd = egd;
		this.description = description;
		this.cameraRecording = cameraRecording;
	
		fakeButtonUpdateGraph = new Gtk.Button();
		fakeButtonCameraStopIfNeeded = new Gtk.Button ();
		fakeButtonThreadDyed = new Gtk.Button();
				
		FakeButtonReactionTimeStart = new Gtk.Button();
		
		simulated = false;
			
		needUpdateEventProgressBar = false;
		needUpdateGraph = false;
		

		//initialize eventDone as a ReactionTime	
		eventDone = new ReactionTime();

		//updateProgressBar = new UpdateProgressBar();
	}
	
	public override void SimulateInitValues(Random randSent)
	{
		LogB.Information("From execute/reactionTime.cs");

		rand = randSent; //we send the random, because if we create here, the values will be the same for each nbew instance
		simulated = true;
		simulatedTimeAccumulatedBefore = 0;
		simulatedTimeLast = 0;
		simulatedFlightTimeMin = 0.1; //seconds
		simulatedFlightTimeMax = 0.4; //seconds

		//values of simulation will be the flightTime
		//at the first time (and the only)
		simulatedCurrentTimeIntervalsAreContact = false;
	}
	
	public override bool Manage()
	{
		//boolean to know if chronopic has been disconnected	
		chronopicDisconnected = false;

		LogB.Error("at Manage!");

		if (simulated) {
			if(StartIn)
				platformState = Chronopic.Plataforma.ON;
			else
				platformState = Chronopic.Plataforma.OFF;
		} else {
			platformState = chronopicInitialValue(cp);
		}
			
		bool canStart = false;
		if ( 
				(StartIn && platformState == Chronopic.Plataforma.ON) ||
				(! StartIn && platformState == Chronopic.Plataforma.OFF) )
			canStart = true;
			
		
		if (canStart) {
			feedbackMessage = Catalog.GetString("You are IN, RELEASE when prepared!"); //TODO: change this
			needShowFeedbackMessage = true; 
			Util.PlaySound(Constants.SoundTypes.CAN_START, volumeOn, gstreamer);

			if(StartIn)
				loggedState = States.ON;
			else
				loggedState = States.OFF;

			//prepare reactionTime for being cancelled if desired
			cancel = false;

			//in simulated mode, make the jump start just when we arrive to waitEvent at the first time
			//mark now that we have leaved platform:
			if (simulated) {
				if(StartIn)
					platformState = Chronopic.Plataforma.OFF;
				else
					platformState = Chronopic.Plataforma.ON;
			}
	
			//if discriminative, will fire the buttons	
			FakeButtonReactionTimeStart.Click();
		} 
		else if (! canStart && (platformState == Chronopic.Plataforma.ON || platformState == Chronopic.Plataforma.OFF) )
		{
			ConfirmWindow confirmWin;		
			confirmWin = ConfirmWindow.Show( 
					Catalog.GetString("You are OUT, come inside and press the 'accept' button"), "", ""); //TODO:change this
			//System.Media.SystemSounds.Beep.Play();
			Util.PlaySound(Constants.SoundTypes.BAD, volumeOn, gstreamer);

			//we call again this function
			confirmWin.Button_accept.Clicked += new EventHandler(callAgainManage);
			
			//if confirmWin.Button_cancel is pressed retuen
			confirmWin.Button_cancel.Clicked += new EventHandler(cancel_event_before_start);
		}
		else { //UNKNOW (Chronopic disconnected, port changed, ...) platformStart == some error
			chronopicHasBeenDisconnected();
			return false;
		}
		return true;
	}

	public override void Manage2() 
	{
		//start thread
		//Log.Write("Start thread");
		thread = new Thread(new ThreadStart(waitEvent));
		GLib.Idle.Add (new GLib.IdleHandler (PulseGTK));

		LogB.ThreadStart(); 
		thread.Start(); 
	}

	protected override void waitEvent ()
	{
		double timestamp = 0;
		bool success = false;
		
		bool ok;
	
		//prepare variables to allow being cancelled or finished
		if(! simulated)
			Chronopic.InitCancelAndFinish();

		LogB.Information("Inside waitEvent");	
		do {
			if(simulated)
				ok = true;
			else 
				ok = cp.Read_event(out timestamp, out platformState);
		
			LogB.Information("Inside do");	
			LogB.Information("cancel == ");	
			LogB.Information(cancel.ToString());	
			LogB.Information("ok == ");	
			LogB.Information(ok.ToString());	
			
			//if (ok) {
			if (ok && !cancel) {
				LogB.Information("ok!");	
				if (platformState == Chronopic.Plataforma.ON && loggedState == States.OFF) 
				{
					//LogB.Information("condition guai! hem entrat!");	
					//has landed
					if(simulated)
						timestamp = simulatedTimeLast * 1000; //conversion to milliseconds

					LogB.Information(string.Format("t1:{0}", timestamp));

					time = timestamp / 1000.0;
					write();

					success = true;

					//update event progressbar
					double percentageToPass = 2; //has two phases

					//progressBarEventOrTimePreExecution(
					//don't do it, put a boolean value and let the PulseGTK do it
					updateProgressBar = new UpdateProgressBar (
							true, //isEvent
							true, //percentageMode
							percentageToPass
							);
					needUpdateEventProgressBar = true;

					loggedState = States.ON;
				}
				else if (platformState == Chronopic.Plataforma.OFF && loggedState == States.ON) 
				{
					//LogB.Information("condition hem sortit");	
			
					//it's out, was inside (= has released)
					
					initializeTimer();
						
					feedbackMessage = "";
					needShowFeedbackMessage = true; 
						
					//update event progressbar
					//progressBarEventOrTimePreExecution(
					//don't do it, put a boolean value and let the PulseGTK do it
					updateProgressBar = new UpdateProgressBar (
							true, //isEvent
							true, //percentageMode
							1 //simple jump, phase 1/2
							);
					needUpdateEventProgressBar = true;

					//change the automata state
					loggedState = States.OFF;

				}
			}
//Log.WriteLine("PREEXIT");
		} while ( ! success && ! cancel );
//Log.WriteLine("EXIT");
	}
	
	protected override bool shouldFinishByTime() {
		return false; //this kind of events cannot be finished by time
	}
	
	protected override void updateTimeProgressBar() {
		//has no finished, but move progressbar time
		progressBarEventOrTimePreExecution(
				false, //isEvent false: time
				false, //activity mode
				-1	//don't want to show info on label
				); 
	}

	protected override void write()
	{
		/*
		string myStringPush =   
			personName + " " + 
			type + " " + Catalog.GetString("Time") + ": " + Util.TrimDecimals( time.ToString(), pDN ) ;
		*/
		
		if(simulated)
			feedbackMessage = Constants.SimulatedMessage();
		else
			feedbackMessage = "";
		needShowFeedbackMessage = true; 
		
		string table = Constants.ReactionTimeTable;

		uniqueID = SqliteReactionTime.Insert(
				false, table, 
				"NULL", personID, sessionID, type,
				time, description, Util.BoolToNegativeInt(simulated));

		//define the created object
		eventDone = new ReactionTime(uniqueID, personID, sessionID, type, time, description, Util.BoolToNegativeInt(simulated)); 
		
		//app1.PrepareJumpSimpleGraph(tv, tc);
		PrepareEventGraphReactionTimeObject = new PrepareEventGraphReactionTime(time, sessionID, personID, table, type);
		needUpdateGraphType = eventType.REACTIONTIME;
		needUpdateGraph = true;
		
		needEndEvent = true; //used for hiding some buttons on eventWindow
	}
	

	~ReactionTimeExecute() {}
	   
}

