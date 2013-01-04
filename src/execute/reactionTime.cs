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

	private ReactionTime reactionTimeDone;
	
	public ReactionTimeExecute() {
	}

	//reactionTime execution
	public ReactionTimeExecute(int personID, string personName, int sessionID,   
			Chronopic cp, Gtk.TextView event_execute_textview_message, Gtk.Window app, int pDN, bool volumeOn,
			double progressbarLimit, ExecutingGraphData egd, Gtk.Image image_simulated_warning
			)
	{
		this.personID = personID;
		this.personName = personName;
		this.sessionID = sessionID;
		
		this.cp = cp;
		this.event_execute_textview_message = event_execute_textview_message;
		this.app = app;

		this.pDN = pDN;
		this.volumeOn = volumeOn;
		this.progressbarLimit = progressbarLimit;
		this.egd = egd;
		this.image_simulated_warning = image_simulated_warning;	
	
		fakeButtonUpdateGraph = new Gtk.Button();
		fakeButtonEventEnded = new Gtk.Button();
		fakeButtonFinished = new Gtk.Button();
		
		simulated = false;
			
		needUpdateEventProgressBar = false;
		needUpdateGraph = false;
		

		//initialize eventDone as a ReactionTime	
		eventDone = new ReactionTime();

		//updateProgressBar = new UpdateProgressBar();
	}
	
	public override void SimulateInitValues(Random randSent)
	{
		Log.WriteLine("From execute/reactionTime.cs");

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
	
	public override void Manage()
	{
		//boolean to know if chronopic has been disconnected	
		chronopicDisconnected = false;

		if (simulated) 
			platformState = Chronopic.Plataforma.ON;
		else
			platformState = chronopicInitialValue(cp);
		
		
		if (platformState==Chronopic.Plataforma.ON) {
			feedbackMessage = Catalog.GetString("You are IN, RELEASE when prepared!");
			needShowFeedbackMessage = true; 
			Util.PlaySound(Constants.SoundTypes.CAN_START, volumeOn);

			loggedState = States.ON;

			//prepare reactionTime for being cancelled if desired
			cancel = false;
			totallyCancelled = false;

			//in simulated mode, make the jump start just when we arrive to waitEvent at the first time
			//mark now that we have leaved platform:
			if (simulated)
				platformState = Chronopic.Plataforma.OFF;
			
			//start thread
			//Log.Write("Start thread");
			thread = new Thread(new ThreadStart(waitEvent));
			GLib.Idle.Add (new GLib.IdleHandler (PulseGTK));
			thread.Start(); 
		} 
		else if (platformState==Chronopic.Plataforma.OFF) {
			ConfirmWindow confirmWin;		
			confirmWin = ConfirmWindow.Show( 
					Catalog.GetString("You are OUT, come inside and press the 'accept' button"), "", "");
			//System.Media.SystemSounds.Beep.Play();
			Util.PlaySound(Constants.SoundTypes.BAD, volumeOn);

			//we call again this function
			confirmWin.Button_accept.Clicked += new EventHandler(callAgainManage);
			
			//if confirmWin.Button_cancel is pressed retuen
			confirmWin.Button_cancel.Clicked += new EventHandler(cancel_event_before_start);
		}
		else { //UNKNOW (Chronopic disconnected, port changed, ...)
			chronopicHasBeenDisconnected();
		}
	}

	
	protected override void waitEvent ()
	{
		double timestamp = 0;
		bool success = false;
		
		bool ok;
		
		do {
			if(simulated)
				ok = true;
			else 
				ok = cp.Read_event(out timestamp, out platformState);
			
			//if (ok) {
			if (ok && !cancel) {
				if (platformState == Chronopic.Plataforma.ON && loggedState == States.OFF) 
				{
					//has landed
					if(simulated)
						timestamp = simulatedTimeLast * 1000; //conversion to milliseconds

					Log.Write(string.Format("t1:{0}", timestamp));

					time = timestamp / 1000.0;
					write ();

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
							1 //normal jump, phase 1/2
							);
					needUpdateEventProgressBar = true;

					//change the automata state
					loggedState = States.OFF;

				}
			}
//Log.WriteLine("PREEXIT");
		} while ( ! success && ! cancel );
//Log.WriteLine("EXIT");
		
		if(cancel) {
			//event will be raised, and managed in chronojump.cs
			fakeButtonFinished.Click();

			totallyCancelled = true;
		}
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
			feedbackMessage = Catalog.GetString(Constants.SimulatedMessage);
		else
			feedbackMessage = "";
		needShowFeedbackMessage = true; 

		uniqueID = SqliteReactionTime.Insert(
				false, Constants.ReactionTimeTable, 
				"NULL", personID, sessionID, "", //type
				time, "", Util.BoolToNegativeInt(simulated)); //time, description, simulated

		//define the created object
		eventDone = new ReactionTime(uniqueID, personID, sessionID, time, "", Util.BoolToNegativeInt(simulated)); 
		
		//event will be raised, and managed in chronojump.cs
		fakeButtonFinished.Click();
		
		//app1.PrepareJumpSimpleGraph(tv, tc);
		PrepareEventGraphReactionTimeObject = new PrepareEventGraphReactionTime(time);
		needUpdateGraphType = eventType.REACTIONTIME;
		needUpdateGraph = true;
		
		needEndEvent = true; //used for hiding some buttons on eventWindow
	}
	

	~ReactionTimeExecute() {}
	   
}

