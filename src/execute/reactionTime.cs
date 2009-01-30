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
	public ReactionTimeExecute(EventExecuteWindow eventExecuteWin, int personID, string personName, int sessionID,   
			Chronopic cp, Gtk.Statusbar appbar, Gtk.Window app, int pDN, bool volumeOn)
	{
		this.eventExecuteWin = eventExecuteWin;
		this.personID = personID;
		this.personName = personName;
		this.sessionID = sessionID;
		
		this.cp = cp;
		this.appbar = appbar;
		this.app = app;

		this.pDN = pDN;
		this.volumeOn = volumeOn;
	
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
		if (simulated) 
			platformState = Chronopic.Plataforma.ON;
		else
			platformState = chronopicInitialValue(cp);
		
		
		if (platformState==Chronopic.Plataforma.ON) {
			appbar.Push( 1,Catalog.GetString("You are IN, RELEASE when prepared!!") );

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
		else {
			ConfirmWindow confirmWin;		
			confirmWin = ConfirmWindow.Show( 
					Catalog.GetString("You are OUT, come inside and press the 'accept' button"), "");
			//System.Media.SystemSounds.Beep.Play();
			Util.PlaySound(Constants.SoundTypes.BAD, volumeOn);

			//we call again this function
			confirmWin.Button_accept.Clicked += new EventHandler(callAgainManage);
			
			//if confirmWin.Button_cancel is pressed retuen
			confirmWin.Button_cancel.Clicked += new EventHandler(cancel_event_before_start);
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
Log.Write("w1 ");				

					if(simulated)
						timestamp = simulatedTimeLast * 1000; //conversion to milliseconds

					Log.Write(string.Format("t1:{0}", timestamp));

					time = timestamp / 1000.0;
					write ();

					success = true;

					//update event progressbar
					double percentageToPass = 2; //has two phases

Log.Write("w5 ");			
					//eventExecuteWin.ProgressBarEventOrTimePreExecution(
					//don't do it, put a boolean value and let the PulseGTK do it
					updateProgressBar = new UpdateProgressBar (
							true, //isEvent
							true, //percentageMode
							percentageToPass
							);
					needUpdateEventProgressBar = true;
Log.Write("w6 ");				

					loggedState = States.ON;
				}
				else if (platformState == Chronopic.Plataforma.OFF && loggedState == States.ON) 
				{
			
					//it's out, was inside (= has released)
					
Log.Write("w9 ");				
					initializeTimer();
						
Log.Write("wa ");				
						
					//update event progressbar
					//eventExecuteWin.ProgressBarEventOrTimePreExecution(
					//don't do it, put a boolean value and let the PulseGTK do it
					updateProgressBar = new UpdateProgressBar (
							true, //isEvent
							true, //percentageMode
							1 //normal jump, phase 1/2
							);
					needUpdateEventProgressBar = true;
Log.Write("wb ");				

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
		eventExecuteWin.ProgressBarEventOrTimePreExecution(
				false, //isEvent false: time
				false, //activity mode
				-1	//don't want to show info on label
				); 
	}

	protected override void write()
	{
		string myStringPush =   
			personName + " " + 
			type + " " + Catalog.GetString("Time") + ": " + Util.TrimDecimals( time.ToString(), pDN ) ;
		
		appbar.Push( 1,myStringPush );

		uniqueID = SqliteReactionTime.Insert(
				false, Constants.ReactionTimeTable, 
				"NULL", personID, sessionID, "", //type
				time, "", Util.BoolToNegativeInt(simulated)); //time, description, simulated

		//define the created object
		eventDone = new ReactionTime(uniqueID, personID, sessionID, time, "", Util.BoolToNegativeInt(simulated)); 
		
		//event will be raised, and managed in chronojump.cs
		fakeButtonFinished.Click();
		
		//eventExecuteWin.PrepareJumpSimpleGraph(tv, tc);
		prepareEventGraphReactionTime = new PrepareEventGraphReactionTime(time);
		needUpdateGraphType = eventType.REACTIONTIME;
		needUpdateGraph = true;
		
		//eventExecuteWin.EventEnded();
		needEndEvent = true; //used for hiding some buttons on eventWindow
	}
	

	~ReactionTimeExecute() {}
	   
}

