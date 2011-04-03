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

public class JumpExecute : EventExecute
{
	protected double tv;
	protected double tc;
	protected double fall;
	protected double weight; //always write in % (not kg or %) then sqlite can do avgs

	//for not checking always in database
	protected bool hasFall;

	//better as private and don't inherit, don't know why
	//protected Chronopic cp;
	private Chronopic cp;

	private Jump jumpDone;

	private int angle = -1;
	
	public JumpExecute() {
	}

	//jump execution
	public JumpExecute(int personID, string personName, int sessionID, string type, double fall, double weight,  
			Chronopic cp, Gtk.TextView event_execute_textview_message, Gtk.Window app, int pDN, bool volumeOn,
			double progressbarLimit, ExecutingGraphData egd 
			)
	{
		this.personID = personID;
		this.personName = personName;
		this.sessionID = sessionID;
		this.type = type;
		this.fall = fall;
		this.weight = weight;
		
		this.cp = cp;
		this.event_execute_textview_message = event_execute_textview_message;
		this.app = app;

		this.pDN = pDN;
		this.volumeOn = volumeOn;
		this.progressbarLimit = progressbarLimit;
		this.egd = egd;
	
		if(TypeHasFall) {
			hasFall = true;
		} else {
			hasFall = false;
		}
		
		fakeButtonUpdateGraph = new Gtk.Button();
		fakeButtonEventEnded = new Gtk.Button();
		fakeButtonFinished = new Gtk.Button();
		
		simulated = false;
			
		needUpdateEventProgressBar = false;
		needUpdateGraph = false;
		

		//initialize eventDone as a Jump		
		eventDone = new Jump();
	}
	
	public override void SimulateInitValues(Random randSent)
	{
		Log.WriteLine("From execute/jump.cs");

		rand = randSent; //we send the random, because if we create here, the values will be the same for each nbew instance
		simulated = true;
		simulatedTimeAccumulatedBefore = 0;
		simulatedTimeLast = 0;
		simulatedContactTimeMin = 0.2; //seconds
		simulatedContactTimeMax = 0.37; //seconds
		simulatedFlightTimeMin = 0.4; //seconds
		simulatedFlightTimeMax = 0.7; //seconds

		if(hasFall) {
			//values of simulation will be the contactTime
			//at the first time, the second will be flightTime
			simulatedCurrentTimeIntervalsAreContact = true;
		} else {
			//values of simulation will be the flightTime
			//at the first time (and the only)
			simulatedCurrentTimeIntervalsAreContact = false;
		}
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
			feedbackMessage = Catalog.GetString("You are IN, JUMP when prepared!");
			needShowFeedbackMessage = true; 
			Util.PlaySound(Constants.SoundTypes.CAN_START, volumeOn);

			loggedState = States.ON;

			//prepare jump for being cancelled if desired
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

			Util.PlaySound(Constants.SoundTypes.BAD, volumeOn);

			//we call again this function
			confirmWin.Button_accept.Clicked += new EventHandler(callAgainManage);
			
			//if confirmWin.Button_cancel is pressed return
			confirmWin.Button_cancel.Clicked += new EventHandler(cancel_event_before_start);
		}
		else { //UNKNOW (Chronopic disconnected, port changed, ...)
			chronopicHasBeenDisconnected();
		}
	}

	public override void ManageFall()
	{
		//boolean to know if chronopic has been disconnected	
		chronopicDisconnected = false;

		if (simulated) 
			platformState = Chronopic.Plataforma.OFF;
		else
			platformState = chronopicInitialValue(cp);

		
		if (platformState==Chronopic.Plataforma.OFF) {
			feedbackMessage = Catalog.GetString("You are OUT, JUMP when prepared!");
			needShowFeedbackMessage = true; 
			Util.PlaySound(Constants.SoundTypes.CAN_START, volumeOn);

			loggedState = States.OFF;

			//useful also for tracking the jump phases
			tc = 0;

			//prepare jump for being cancelled if desired
			cancel = false;
			totallyCancelled = false;

			//in simulated mode, make the jump start just when we arrive to waitEvent at the first time
			//mark now that we have arrived:
			if (simulated)
				platformState = Chronopic.Plataforma.ON;
			
			//start thread
			thread = new Thread(new ThreadStart(waitEvent));
			GLib.Idle.Add (new GLib.IdleHandler (PulseGTK));
			thread.Start(); 
		} 
		else if (platformState==Chronopic.Plataforma.ON) {
			ConfirmWindow confirmWin;		
			confirmWin = ConfirmWindow.Show( 
					Catalog.GetString("You are IN, please leave the platform, and press the 'accept' button"), "", "");
			Util.PlaySound(Constants.SoundTypes.BAD, volumeOn);

			//we call again this function
			confirmWin.Button_accept.Clicked += new EventHandler(callAgainManageFall);
			
			//if confirmWin.Button_cancel is pressed return
			confirmWin.Button_cancel.Clicked += new EventHandler(cancel_event_before_start);
		}
		else { //UNKNOW (Chronopic disconnected, port changed, ...)
			chronopicHasBeenDisconnected();
		}
	}
	
	//for calling it again after a confirmWindow says that you have to be in or out the platform
	//and press ok button
	//This method is for not having problems with the parameters of the delegate
	private void callAgainManageFall(object o, EventArgs args) {
		ManageFall();
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
					if(hasFall && tc == 0) {
						//jump with fall, landed first time
						initializeTimer();

						//app1.ProgressBarEventOrTimePreExecution(
						//don't do it, put a boolean value and let the PulseGTK do it
						updateProgressBar = new UpdateProgressBar (
								true, //isEvent
								true, //jumpsLimited: percentageMode
								1 //it's a drop: phase 1/3
								);
						needUpdateEventProgressBar = true;
		
						feedbackMessage = "";
						needShowFeedbackMessage = true; 
					} else {
						//jump with fall: second landed; or without fall first landing
					
						if(simulated)
							timestamp = simulatedTimeLast * 1000; //conversion to milliseconds
						
						Log.Write(string.Format("t1:{0}", timestamp));

						tv = timestamp / 1000.0;
						write ();

						success = true;
						
						//update event progressbar
						double percentageToPass = 2; //normal jump has two phases
						if(hasFall)
							percentageToPass = 3; //drop jump has three phases
							
						//app1.ProgressBarEventOrTimePreExecution(
						//don't do it, put a boolean value and let the PulseGTK do it
						updateProgressBar = new UpdateProgressBar (
								true, //isEvent
								true, //percentageMode
								percentageToPass
								);
						needUpdateEventProgressBar = true;
					}
					
					loggedState = States.ON;
				}
				else if (platformState == Chronopic.Plataforma.OFF && loggedState == States.ON) 
				{
			
					//it's out, was inside (= has jumped)
					
					if(hasFall) {
						
						if(simulated)
							timestamp = simulatedTimeLast * 1000; //conversion to milliseconds
						
						Log.Write(string.Format("t2:{0}", timestamp));
						
						//record the TC
						tc = timestamp / 1000.0;
						
						//takeOff jump (only one TC)
						//if(fixedValue == 0.5) {
						if(type == Constants.TakeOffName || type == Constants.TakeOffWeightName) {
							tv = 0;
							write ();
							success = true;
						}

						//update event progressbar
						//app1.ProgressBarEventOrTimePreExecution(
						//don't do it, put a boolean value and let the PulseGTK do it
						updateProgressBar = new UpdateProgressBar (
								true, //isEvent
								true, //percentageMode
								2 //it's a drop jump: phase 2/3
								);
						needUpdateEventProgressBar = true;
					} else {
						initializeTimer();
						
						//update event progressbar
						//app1.ProgressBarEventOrTimePreExecution(
						//don't do it, put a boolean value and let the PulseGTK do it
						updateProgressBar = new UpdateProgressBar (
								true, //isEvent
								true, //percentageMode
								1 //normal jump, phase 1/2
								);
						needUpdateEventProgressBar = true;
						
						feedbackMessage = "";
						needShowFeedbackMessage = true; 
					}

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
		return false; //this kind of events (simple or Dj jumps) cannot be finished by time
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
		string tcString = "";
		if(hasFall) {
			//Log.WriteLine("TC: {0}", tc.ToString());
			tcString = " " + Catalog.GetString("TC") + ": " + Util.TrimDecimals( tc.ToString(), pDN ) ;
		} else {
			tc = 0;
		}
		
		/*	
		string myStringPush =   
			personName + " " + 
			type + tcString + " " + Catalog.GetString("TF") + ": " + Util.TrimDecimals( tv.ToString(), pDN ) ;
		if(weight > 0) {
			myStringPush = myStringPush + "(" + weight.ToString() + "%)";
		}
		*/
		if(simulated)
			feedbackMessage = Catalog.GetString(Constants.SimulatedMessage);
		else
			feedbackMessage = "";
		needShowFeedbackMessage = true; 

		uniqueID = SqliteJump.Insert(false, Constants.JumpTable, "NULL", personID, sessionID, 
				type, tv, tc, fall,  //type, tv, tc, fall
				weight, "", angle, Util.BoolToNegativeInt(simulated)); //weight, description, simulated

		//define the created object
		eventDone = new Jump(uniqueID, personID, sessionID, type, tv, tc, fall, 
				weight, "", angle, Util.BoolToNegativeInt(simulated)); 
		
		//event will be raised, and managed in chronojump.cs
		fakeButtonFinished.Click();
		
		PrepareEventGraphJumpSimpleObject = new PrepareEventGraphJumpSimple(tv, tc);
		needUpdateGraphType = eventType.JUMP;
		needUpdateGraph = true;
		
		needEndEvent = true; //used for hiding some buttons on eventWindow
	}
	
/*
	public Jump JumpDone {
		get { return jumpDone; }
	}
*/
	public virtual bool TypeHasWeight
	{
		get { return SqliteJumpType.HasWeight("jumpType", type); }
	}
	
	public virtual bool TypeHasFall
	{
		get { return SqliteJumpType.HasFall("jumpType", type); } //jumpType is the table name
	}
	
	/*
	public string JumperName
	{
		get { return SqlitePerson.SelectJumperName(personID); }
	}
	*/

	~JumpExecute() {}
	   
}

public class JumpRjExecute : JumpExecute
{
	string tvString;
	string tcString;
	int jumps; //total number of jumps
	double time; //time elapsed
	string limited; //the teorically values, eleven jumps: "11=J" (time recorded in "time"), 10 seconds: "10=T" (jumps recorded in jumps)
	double limitAsDouble;	//-1 for non limited (unlimited repetitive jump until "finish" is clicked)
	bool jumpsLimited;
	bool firstRjValue;
	private double tcCount;
	private double tvCount;
	private double lastTc;
	private double lastTv;
	
	//better as private and don't inherit, don't know why
	private Chronopic cp;

	//this records a jump when time has finished (if jumper was in the air)
	private bool allowFinishAfterTime;
	//this will be a flag for finishing if allowFinishAfterTime is true
	private bool shouldFinishAtNextFall = true;

	private string angleString = "-1";
	
	public JumpRjExecute() {
	}

	//jump execution
	public JumpRjExecute(int personID, string personName, 
			int sessionID, string type, double fall, double weight, 
			double limitAsDouble, bool jumpsLimited, 
			Chronopic cp, Gtk.TextView event_execute_textview_message, Gtk.Window app, int pDN, bool allowFinishAfterTime, 
			bool volumeOn, RepetitiveConditionsWindow repetitiveConditionsWin,
			double progressbarLimit, ExecutingGraphData egd 
			)
	{
		this.personID = personID;
		this.personName = personName;
		this.sessionID = sessionID;
		this.type = type;
		this.fall = fall;
		this.weight = weight;
		this.limitAsDouble = limitAsDouble;

		this.jumpsLimited = jumpsLimited;
		if(jumpsLimited) {
			this.limited = limitAsDouble.ToString() + "J";
		} else {
			//this.limited = limitAsDouble.ToString() + "T"; define later, because it can be higher if allowFinishRjAfterTime is defined
		}
		
		this.cp = cp;
		this.event_execute_textview_message = event_execute_textview_message;
		this.app = app;

		this.pDN = pDN;
		this.allowFinishAfterTime = allowFinishAfterTime;
		this.volumeOn = volumeOn;
		this.repetitiveConditionsWin = repetitiveConditionsWin;
		this.progressbarLimit = progressbarLimit;
		this.egd = egd;
	
		if(TypeHasFall) { hasFall = true; } 
		else { hasFall = false; }
		
		fakeButtonUpdateGraph = new Gtk.Button();
		fakeButtonEventEnded = new Gtk.Button();
		fakeButtonFinished = new Gtk.Button();
		
		simulated = false;
			
		needUpdateEventProgressBar = false;
		needUpdateGraph = false;
		
		timesForSavingRepetitive = 10; //number of times that this repetive event needs for being recorded in temporal table

		//initialize eventDone as a JumpRj	
		eventDone = new JumpRj();
	}

	public override void Manage()
	{
		//boolean to know if chronopic has been disconnected	
		chronopicDisconnected = false;

		if (simulated)
			if(hasFall) 
				platformState = Chronopic.Plataforma.OFF;
			else 
				platformState = Chronopic.Plataforma.ON;
		else
			platformState = chronopicInitialValue(cp);

		if(platformState == Chronopic.Plataforma.OFF)
			loggedState = States.OFF;
		else if(platformState == Chronopic.Plataforma.ON)
			loggedState = States.ON;
		else { //UNKNOW (Chronopic disconnected, port changed, ...)
			chronopicHasBeenDisconnected();
			return;
		}

		
		bool success = false;

		if (platformState==Chronopic.Plataforma.OFF && hasFall ) {
			feedbackMessage = Catalog.GetString("You are OUT, JUMP when prepared!");
			needShowFeedbackMessage = true; 
			Util.PlaySound(Constants.SoundTypes.CAN_START, volumeOn);
			success = true;
		} else if (platformState==Chronopic.Plataforma.ON && ! hasFall ) {
			feedbackMessage = Catalog.GetString("You are IN, JUMP when prepared!");
			needShowFeedbackMessage = true; 
			Util.PlaySound(Constants.SoundTypes.CAN_START, volumeOn);
			success = true;
		} else {
			string myMessage = Catalog.GetString("You are IN, please leave the platform, and press the 'accept' button");
			if (platformState==Chronopic.Plataforma.OFF ) {
				myMessage = Catalog.GetString("You are OUT, please enter the platform, prepare for jump and press the 'accept' button");
			}
			ConfirmWindow confirmWin;		
			confirmWin = ConfirmWindow.Show(myMessage, "","");
			Util.PlaySound(Constants.SoundTypes.BAD, volumeOn);

			//we call again this function
			confirmWin.Button_accept.Clicked += new EventHandler(callAgainManage);
			//if confirmWin.Button_cancel is pressed return
			confirmWin.Button_cancel.Clicked += new EventHandler(cancel_event_before_start);
		}

		if(success) {
			//initialize strings of TCs and TFs
			tcString = "";
			tvString = "";
			tcCount = 0;
			tvCount = 0;
			firstRjValue = true;

			//if jump starts on TF, write a "-1" in TC
			if ( ! hasFall ) {
				double myTc = -1;
				tcString = myTc.ToString();
				tcCount = 1;
			}

			//prepare jump for being cancelled if desired
			cancel = false;
			totallyCancelled = false;
			
			//prepare jump for being finished earlier if desired
			finish = false;
			totallyFinished = false;
			
			//in simulated mode, make the jump start just when we arrive to waitEvent at the first time
			//mark now that the opposite as before:
			if (simulated) {
				if(hasFall)
					platformState = Chronopic.Plataforma.ON;
				else 
					platformState = Chronopic.Plataforma.OFF;
			}
			
			//start thread
			thread = new Thread(new ThreadStart(waitEvent));
			GLib.Idle.Add (new GLib.IdleHandler (PulseGTK));
			thread.Start(); 
		}
	}
	
	protected override void waitEvent ()
	{
		double timestamp = 0;
		bool success = false;
				
		shouldFinishAtNextFall = false;
		
		bool ok;

		int countForSavingTempTable = 0;
	
		do {
			if(simulated) 
				ok = true;
			else
				ok = cp.Read_event(out timestamp, out platformState);
			
			
			//if chronopic signal is Ok and state has changed
			if (ok && (
					(platformState == Chronopic.Plataforma.ON && loggedState == States.OFF) ||
					(platformState == Chronopic.Plataforma.OFF && loggedState == States.ON) ) 
						&& !cancel && !finish) {
				
			
				if(simulated)
					timestamp = simulatedTimeLast * 1000; //conversion to milliseconds

				Log.Write(Util.GetTotalTime(tcString, tvString).ToString());


					
				
				string equal = "";
				
				//while no finished time or jumps, continue recording events
				if ( ! success) {
					//don't record the time until the first event
					if (firstRjValue) {
						firstRjValue = false;

						//but start timer
						initializeTimer();
						
						feedbackMessage = "";
						needShowFeedbackMessage = true; 
					} else {
						//reactive jump has not finished... record the next jump
						Log.WriteLine(string.Format("tcCount: {0}, tvCount: {1}", tcCount, tvCount));
						if ( tcCount == tvCount )
						{
							lastTc = timestamp/1000.0;
							
							if(tcCount > 0) { equal = "="; }
							tcString = tcString + equal + lastTc.ToString();

							updateTimerCountWithChronopicData(tcString, tvString);
							
							tcCount = tcCount + 1;
						} else {
							//tcCount > tvCount 
							lastTv = timestamp/1000.0;
							
							if(tvCount > 0) { equal = "="; }
							tvString = tvString + equal + lastTv.ToString();
							
							updateTimerCountWithChronopicData(tcString, tvString);							
							tvCount = tvCount + 1;
							
							//update event progressbar
							//app1.ProgressBarEventOrTimePreExecution(
							updateProgressBar= new UpdateProgressBar (
									true, //isEvent
									jumpsLimited, //if jumpsLimited: do fraction; if time limited: do pulse
									tvCount
									);  
							needUpdateEventProgressBar = true;
							
							//update graph
							PrepareEventGraphJumpReactiveObject = new PrepareEventGraphJumpReactive(lastTv, lastTc, tvString, tcString);
							needUpdateGraphType = eventType.JUMPREACTIVE;
							needUpdateGraph = true;

							//put button_finish as sensitive when first jump is done (there's something recordable)
							if(tvCount == 1)
								needSensitiveButtonFinish = true;

							//save temp table if needed
							countForSavingTempTable ++;
							if(countForSavingTempTable == timesForSavingRepetitive) {
								writeRj(true); //tempTable
								countForSavingTempTable = 0;
							}

						}
					}
				}
			
				//if we finish by time, and allowFinishAfterTime == true, when time passed, if the jumper is jumping
				//if flags the shouldFinishAtNextFall that will finish when he arrives to the platform
				if(shouldFinishAtNextFall && platformState == Chronopic.Plataforma.ON && loggedState == States.OFF)
					finish = true;

				
				//check if reactive jump should finish
				if (jumpsLimited) {
					if(limitAsDouble != -1) {
						if(Util.GetNumberOfJumps(tvString, false) >= limitAsDouble)
						{
							writeRj(false); //tempTable
							success = true;
						
							//update event progressbar
							//app1.ProgressBarEventOrTimePreExecution(
							updateProgressBar= new UpdateProgressBar (
									true, //isEvent
									true, //percentageMode
									tvCount
									);  
							needUpdateEventProgressBar = true;
							
							//update graph
							PrepareEventGraphJumpReactiveObject = new PrepareEventGraphJumpReactive(lastTv, lastTc, tvString, tcString);
							needUpdateGraphType = eventType.JUMPREACTIVE;
							needUpdateGraph = true;
						}
					}
				} 
				else {
					//limited by time, if passed it, write
					if(success) {
						//write();
						//write only if there's a jump at minimum
						if(Util.GetNumberOfJumps(tcString, false) >= 1 && Util.GetNumberOfJumps(tvString, false) >= 1) {
							writeRj(false); //tempTable
						} else {
							//cancel a jump if clicked finish before any events done
							cancel = true;
						}
					}
				}

/*
				EndingConditionsJumpRj conditions = new EndingConditionsJumpRj();
				if(! conditionsOk(tv,tc)) {
					finish = true;
					posar MARCA de que les conditions no estan be i després mostrar quines
*/

				if(platformState == Chronopic.Plataforma.OFF)
					loggedState = States.OFF;
				else
					loggedState = States.ON;

			}
		} while ( ! success && ! cancel && ! finish );
	
		
		if (finish) {
			//write only if there's a jump at minimum
			if(Util.GetNumberOfJumps(tcString, false) >= 1 && Util.GetNumberOfJumps(tvString, false) >= 1) {
				writeRj(false); //tempTable
				
				totallyFinished = true;
			} else {
				//cancel a jump if clicked finish before any events done
				cancel = true;
			}
		}
		//if(cancel || finish) {
		if(cancel) {
			//event will be raised, and managed in chronojump.cs
			fakeButtonFinished.Click();
			
			totallyCancelled = true;
		}
	}

	protected override bool shouldFinishByTime() {
		//check if it should finish now (time limited, not unlimited and time exceeded)
		//check also that rj has started (!firstRjValue)

		//if( !jumpsLimited && limitAsDouble != -1 && timerCount > limitAsDouble && !firstRjValue)
		if( !jumpsLimited && limitAsDouble != -1 && Util.GetTotalTime(tcString, tvString) > limitAsDouble && !firstRjValue)
		{
			//limited by Time, we are jumping and time passed
			if ( tcCount == tvCount ) {
				//if we are on floor
				return true;
			} else {
				//we are on air
				if(allowFinishAfterTime) {
					Log.Write("ALLOW!!");
					//allow to finish later, return false, and waitEvent (looking at shouldFinishAtNextFall)
					//will finishJump when he falls 
					shouldFinishAtNextFall = true;
					return false;
				} else {
					//we are at air, but ! shouldFinishAfterTime, then finish now discarding current jump
					return true;
				}
			}
		}
		else
			//we haven't finished, return false
			return false;
	}
	
	protected override void updateProgressBarForFinish() {
		progressBarEventOrTimePreExecution(
				false, //isEvent false: time
				true, //percentageMode: it has finished, show bar at 100%
				//limitAsDouble
				Util.GetTotalTime(tcString, tvString)
				);  
	}

	protected override void updateTimeProgressBar() {
		//limited by jumps or time or unlimited, but has no finished

		if(firstRjValue)  
			//until it has not landed for first time, show a pulse with no values
			progressBarEventOrTimePreExecution(
					false, //isEvent false: time
					false, //activity mode
					-1	//don't want to show info on label
					); 
		else
			//after show a progressBar with time value
			progressBarEventOrTimePreExecution(
					false, //isEvent false: time
					!jumpsLimited, //if jumpsLimited: activity, if timeLimited: fraction
					timerCount
					); 
	}


	private void updateTimerCountWithChronopicData(string tcString, string tvString) {
		//update timerCount, with the chronopic data
		//but in the first jump probably one is zero and then GetTotalTime returns a 0
		Log.WriteLine(string.Format("///I timerCount: {0} tcString+tvString: {1} ///", timerCount, Util.GetTotalTime(tcString) + Util.GetTotalTime(tvString)));
		if(tvString.Length == 0) 
			timerCount =  Util.GetTotalTime(tcString);
		else if (tcString.Length == 0) 
			timerCount =  Util.GetTotalTime(tvString);
		else 
			timerCount =  Util.GetTotalTime(tcString, tvString);
	}
				
				
	protected void writeRj(bool tempTable)
	{
		Log.WriteLine("----------WRITING----------");
		int jumps;
		string limitString = "";
		string description = "";

		//if user clicked in finish earlier
		//or toggled with shouldFinishAtNextTime
		if(finish) {
			//if user clicked finish and last event was tc, probably there are more TCs than TFs
			//if last event was tc, it has no sense, it should be deleted
			tcString = Util.DeleteLastTcIfNeeded(tcString, tvString);

			//when we mark that jump should finish by time, chronopic thread is probably capturing data
			//check if it captured more than date limit, and if it has done, delete last(s) jump(s)
			//also have in mind that allowFinishAfterTime exist
			bool deletedEvent = false;
			if( ! jumpsLimited && limitAsDouble != -1) {
				bool eventPassed = Util.EventPassedFromMaxTime(tcString, tvString, limitAsDouble, allowFinishAfterTime);
				while(eventPassed) {
					tcString = Util.DeleteLastSubEvent(tcString);
					tvString = Util.DeleteLastSubEvent(tvString);
					Log.WriteLine("Deleted one event out of time");
					eventPassed = Util.EventPassedFromMaxTime(tcString, tvString, limitAsDouble, allowFinishAfterTime);
					deletedEvent = true;
				}
			}
			if(deletedEvent) {
				//update graph if a event was deleted
				PrepareEventGraphJumpReactiveObject = new PrepareEventGraphJumpReactive(Util.GetLast(tvString), Util.GetLast(tcString), tvString, tcString);
				needUpdateGraphType = eventType.JUMPREACTIVE;
				needUpdateGraph = true;



				//try to fix this:
				//http://mail.gnome.org/archives/chronojump-list/2007-June/msg00013.html
							updateProgressBar= new UpdateProgressBar (
									true, //isEvent
									jumpsLimited, //if jumpsLimited: do fraction; if time limited: do pulse
									 Util.GetNumberOfJumps(tvString, false)
									);  
							needUpdateEventProgressBar = true;
				//and this:
				//http://mail.gnome.org/archives/chronojump-list/2007-June/msg00017.html
							updateTimerCountWithChronopicData(tcString, tvString);							
							
			}

			jumps = Util.GetNumberOfJumps(tvString, false);

					
			if(jumpsLimited) {
				limitString = jumps.ToString() + "J";
			} else {
				limitString = Util.GetTotalTime(tcString, tvString) + "T";
				limited = limitString; //define limited because it's checked in treeviewJump, and possibly it's not the initial defined time (specially when allowFinishRjAfterTime is true)
				//leave the initial selected time into description/comments:
				description = string.Format(Catalog.GetString("Initially selected {0} seconds"), limitAsDouble.ToString());
			}
		} else {
			if(jumpsLimited) {
				limitString = limitAsDouble.ToString() + "J";
				jumps = (int) limitAsDouble;
			} else {
				//if time finished and the last event was tc, probably there are more TCs than TFs
				//if last event was tc, it has no sense, it should be deleted
				//this is not aplicable in tempTable
				if(! tempTable)
					tcString = Util.DeleteLastTcIfNeeded(tcString, tvString);
				
				//limitString = limitAsDouble.ToString() + "T";
				limitString = Util.GetTotalTime(tcString, tvString) + "T";
				limited = limitString; //define limited because it's checked in treeviewJump, and possibly it's not the initial defined time (specially when allowFinishRjAfterTime is true)
				
				//leave the initial selected time into description/comments:
				description = string.Format(Catalog.GetString("Initially selected {0} seconds"), limitAsDouble.ToString());

				string [] myStringFull = tcString.Split(new char[] {'='});
				jumps = myStringFull.Length;
			}
		}

		if(type == Constants.RunAnalysisName) {
			//double speed = (fall /10) / Util.GetTotalTime(tcString, tvString);
	
	/*		
	 *		Josep Ma Padullés test
	 *
			string tcStringWithoutFirst = Util.DeleteFirstSubEvent(tcString);
			string tvStringWithoutFirst = Util.DeleteFirstSubEvent(tvString);
		
			double averagePlatformTimes = ( Util.GetAverage(tcStringWithoutFirst) + Util.GetAverage(tvStringWithoutFirst) ) / 2;
			double freq = 1 / averagePlatformTimes;

			//amplitud
			double range = speed / freq;
			
			//don't put "=" because can appear problems in different parts of the code
			description = 
				Catalog.GetString ("AVG speed") + "->" + Util.TrimDecimals(speed.ToString(), pDN) + "m/s, " +
				Catalog.GetString ("AVG frequencies") + "->" + Util.TrimDecimals(freq.ToString(), pDN) + "Hz, " +
				Catalog.GetString ("AVG range") + "->" + Util.TrimDecimals(range.ToString(), pDN) + "m.";
				*/
		}


		if(tempTable) 
			SqliteJumpRj.Insert(false, Constants.TempJumpRjTable, "NULL", personID, sessionID, 
					type, Util.GetMax(tvString), Util.GetMax(tcString), 
					fall, weight, description,
					Util.GetAverage(tvString), Util.GetAverage(tcString),
					tvString, tcString,
					jumps, Util.GetTotalTime(tcString, tvString), limitString, angleString, Util.BoolToNegativeInt(simulated)
					);
		else {
			uniqueID = SqliteJumpRj.Insert(false, Constants.JumpRjTable, "NULL", personID, sessionID, 
					type, Util.GetMax(tvString), Util.GetMax(tcString), 
					fall, weight, description,
					Util.GetAverage(tvString), Util.GetAverage(tcString),
					tvString, tcString,
					jumps, Util.GetTotalTime(tcString, tvString), limitString, angleString, Util.BoolToNegativeInt(simulated)
					);

			//define the created object
			eventDone = new JumpRj(uniqueID, personID, sessionID, type, tvString, tcString, fall, weight, description, jumps, Util.GetTotalTime(tcString, tvString), limitString, angleString, Util.BoolToNegativeInt(simulated)); 


			//event will be raised, and managed in chronojump.cs
			/*
			string myStringPush =   
				//Catalog.GetString("Last jump: ") + 
				personName + " " + 
				type + " (" + limitString + ") " +
				" " + Catalog.GetString("AVG TF") + ": " + Util.TrimDecimals( Util.GetAverage (tvString).ToString(), pDN ) +
				" " + Catalog.GetString("AVG TC") + ": " + Util.TrimDecimals( Util.GetAverage (tcString).ToString(), pDN ) ;
			*/
			if(simulated)
				feedbackMessage = Catalog.GetString(Constants.SimulatedMessage);
			else
				feedbackMessage = "";
			needShowFeedbackMessage = true; 
		

			//event will be raised, and managed in chronojump.cs
			fakeButtonFinished.Click();

			needEndEvent = true; //used for hiding some buttons on eventWindow, and also for updateTimeProgressBar here
		}
	}

	public override bool TypeHasWeight
	{
		get { return SqliteJumpType.HasWeight("jumpRjType", type); }
	}
	

	public override bool TypeHasFall
	{
		get { return SqliteJumpType.HasFall("jumpRjType", type); } //jumpRjType is the table name
	}


	~JumpRjExecute() {}
}
