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
using System.Collections.Generic; //List
using System.Threading;
using System.IO.Ports;
using Mono.Unix;

public class RunExecute : EventExecute
{
	protected double distance;
	protected double time;
	protected bool startIn;
	
	protected Chronopic cp;
	//private Chronopic cp; //thi doesn't work
	protected bool metersSecondsPreferred;

	//used by the updateTimeProgressBar for display its time information
	//changes a bit on runSimple and runInterval
	//explained at each of the updateTimeProgressBar()
	//measureRectionTime will be PLATFORM_INI_YES_TIME
	protected enum runPhases {
		PRE_RUNNING, PLATFORM_INI_YES_TIME, PLATFORM_INI_NO_TIME, RUNNING, PLATFORM_END
	}
	protected static runPhases runPhase;
		
	protected Constants.DoubleContact checkDoubleContactMode;
	protected int checkDoubleContactTime;
	
	protected bool speedStartArrival; //if speedStartArrival then race time includes reaction time
	protected bool measureReactionTime;
	protected double reactionTimeMS; //reaction time in milliseconds

	//double contacts stuff
	protected double timestampDCFlightTimes; 	//sum of the flight times that happen in small time
	protected double timestampDCContactTimes; 	//sum of the contact times that happen in small time
	protected double timestampDCn; 			//number of flight times in double contacts period

	public RunExecute() {
	}

	//run execution
	public RunExecute(int personID, int sessionID, string type, double distance,   
			Chronopic cp, Gtk.Label event_execute_label_message, Gtk.Window app, 
			int pDN, bool metersSecondsPreferred,
			bool volumeOn, Preferences.GstreamerTypes gstreamer,
			double progressbarLimit, ExecutingGraphData egd,
			Constants.DoubleContact checkDoubleContactMode, int checkDoubleContactTime, 
			bool speedStartArrival, bool measureReactionTime
			)
	{
		this.personID = personID;
		this.sessionID = sessionID;
		this.type = type;
		this.distance = distance;
		
		this.cp = cp;
		this.event_execute_label_message = event_execute_label_message;
		this.app = app;

		this.pDN = pDN;
		this.metersSecondsPreferred = metersSecondsPreferred;
		this.volumeOn = volumeOn;
		this.gstreamer = gstreamer;
		this.progressbarLimit = progressbarLimit;
		this.egd = egd;
		this.checkDoubleContactMode = checkDoubleContactMode;
		this.checkDoubleContactTime = checkDoubleContactTime;
		this.speedStartArrival = speedStartArrival;	
		this.measureReactionTime = measureReactionTime;

		reactionTimeMS = 0;

		fakeButtonUpdateGraph = new Gtk.Button();
		fakeButtonThreadDyed = new Gtk.Button();

		simulated = false;
		
		needUpdateEventProgressBar = false;
		needUpdateGraph = false;
		
		//initialize eventDone as a Run	
		eventDone = new Run();
	}

	
	public override void SimulateInitValues(Random randSent)
	{
		LogB.Information("From execute/run.cs");

		rand = randSent; //we send the random, because if we create here, the values will be the same for each nbew instance
		simulated = true;
		simulatedTimeAccumulatedBefore = 0;
		simulatedTimeLast = 0;
		simulatedContactTimeMin = 0; //seconds
		simulatedContactTimeMax = 1; //seconds ('0' gives problems)
		simulatedFlightTimeMin = 3; //seconds
		simulatedFlightTimeMax = 4; //seconds

	}
	
	public override void Manage()
	{
		LogB.Debug("MANAGE!!!!");

		//boolean to know if chronopic has been disconnected	
		chronopicDisconnected = false;

		if (simulated) 
			platformState = Chronopic.Plataforma.ON;
		else
			platformState = chronopicInitialValue(cp);
		
		LogB.Debug("MANAGE(b)!!!!");

		//you can start ON or OFF the platform, 
		//we record always de TF (or time between we abandonate the platform since we arrive)
		if (platformState==Chronopic.Plataforma.ON) {
			feedbackMessage = Catalog.GetString("You are IN, RUN when prepared!");
			needShowFeedbackMessage = true; 
			Util.PlaySound(Constants.SoundTypes.CAN_START, volumeOn, gstreamer);

			loggedState = States.ON;
			startIn = true;
			runPhase = runPhases.PLATFORM_INI_NO_TIME;
		} else if (platformState==Chronopic.Plataforma.OFF) {
			feedbackMessage = Catalog.GetString("You are OUT, RUN when prepared!");
			needShowFeedbackMessage = true; 
			Util.PlaySound(Constants.SoundTypes.CAN_START, volumeOn, gstreamer);

			loggedState = States.OFF;
			startIn = false;
			runPhase = runPhases.PRE_RUNNING;
		}
		else { //UNKNOW (Chronopic disconnected, port changed, ...)
			chronopicHasBeenDisconnected();
			return;
		}

	
		if (simulated) {
			if(startIn) {
				//values of simulation will be the flightTime (between two platforms)
				//at the first time (and the only)
				//start running being on the platform
				simulatedCurrentTimeIntervalsAreContact = false;
			
				//in simulated mode, make the run start just when we arrive to waitEvent at the first time
				//mark now that we have abandoned platform:
				platformState = Chronopic.Plataforma.OFF;
			} else {
				//values of simulation will be the contactTime
				//at the first time, the second will be flightTime (between two platforms)
				//come with previous run ("salida lanzada")
				simulatedCurrentTimeIntervalsAreContact = true;
				
				//in simulated mode, make the run start just when we arrive to waitEvent at the first time
				//mark now that we have reached the platform:
				platformState = Chronopic.Plataforma.ON;
			}
		}

		//prepare jump for being cancelled if desired
		cancel = false;

		//start thread
		thread = new Thread(new ThreadStart(waitEvent));
		GLib.Idle.Add (new GLib.IdleHandler (PulseGTK));
			
		LogB.ThreadStart(); 
		thread.Start(); 
	}

	protected void timestampDCInitValues()
	{
		timestampDCFlightTimes = 0;
		timestampDCContactTimes = 0;
		timestampDCn = 0;
	}

	protected override void waitEvent ()
	{
		double timestamp = 0;
		double timestampFirstContact = 0; //used when runPhase == runPhases.PLATFORM_INI_YES_TIME;

		timestampDCInitValues();

		bool success = false;
		bool ok;

		//prepare variables to allow being cancelled or finished
		if(! simulated)
			Chronopic.InitCancelAndFinish();

		do {
			if(simulated)
				ok = true;
			else 
				ok = cp.Read_event(out timestamp, out platformState);
			
			//if (ok) {
			if (ok && !cancel) {
				//LogB.Information("timestamp:" + timestamp);
				if (platformState == Chronopic.Plataforma.ON && loggedState == States.OFF) {
					//has arrived
					loggedState = States.ON;
					
					if(runPhase == runPhases.PRE_RUNNING) {
						if(speedStartArrival || measureReactionTime) {
							runPhase = runPhases.PLATFORM_INI_YES_TIME;
							initializeTimer(); //timerCount = 0
						} else
							runPhase = runPhases.PLATFORM_INI_NO_TIME;
						
						updateProgressBar = new UpdateProgressBar (
								true, //isEvent
								true, //tracksLimited: percentageMode
								1 //just reached platform, phase 1/3
								);  
						needUpdateEventProgressBar = true;
					} else {
						//run finished: 
						//if started outside (behind platform) it's the second arrive
						//if started inside: it's the first arrive
						
						if(simulated)
							timestamp = simulatedTimeLast * 1000; //conversion to milliseconds

						//prevent double contact stuff
						if(checkDoubleContactMode != Constants.DoubleContact.NONE) {
							if(timestamp <= checkDoubleContactTime) {
								/*
								   when checking double contact
								   first time that timestamp < checkDoubleContactTime
								   and we arrived (it's a flight time)
								   record this time as timestampDCFlightTimes
								 */
								timestampDCn ++;
								timestampDCFlightTimes += timestamp;
							}
							else {
								if(timestampDCn > 0) {
									if(checkDoubleContactMode == 
											Constants.DoubleContact.FIRST) {
										/* user want first flight time,
										   then add all DC times*/
										timestamp += timestampDCFlightTimes + 
											timestampDCContactTimes;
									}
									else if(checkDoubleContactMode == 
											Constants.DoubleContact.LAST) {
										//user want last flight time, take that
										// It doesn't change the timestamp so this is the same as:
										// timestamp = timestamp;
									}
									else {	/* do the avg of all flights and contacts
										   then add to last timestamp */
										timestamp += 
											(timestampDCFlightTimes + 
											 timestampDCContactTimes) 
											/ timestampDCn;
									}
								}
								success = true;
							}
						}
						
						if(checkDoubleContactMode == Constants.DoubleContact.NONE)
							success = true;

						if(success) {
							runPhase = runPhases.PLATFORM_END;

							//add the first contact time if PLATFORM_INI_YES_TIME
							if(timestampFirstContact > 0)
							{
								if(measureReactionTime)
									reactionTimeMS = timestampFirstContact;

								// measuringReactionTime or not, if speedStartArrival, timestamp should include timpestampFirstContact
								if(speedStartArrival)
									timestamp += timestampFirstContact;
							}

							time = timestamp / 1000.0;
							write();

							//success = true;

							updateProgressBar = new UpdateProgressBar (
									true, //isEvent
									true, //percentageMode
									//percentageToPass
									3
									);  
							needUpdateEventProgressBar = true;
						}
					}
				}
				else if (platformState == Chronopic.Plataforma.OFF && loggedState == States.ON) {
					//it's out, was inside (= has abandoned platform)
					//don't record time
						
					//change the automata state
					loggedState = States.OFF;

					if(checkDoubleContactMode != Constants.DoubleContact.NONE && timestampDCn > 0)
						timestampDCContactTimes += timestamp; //TODO: print before here every timestampDCContactTime to understand better what's readed on Chronopic
					else {
						if(runPhase == runPhases.PLATFORM_INI_YES_TIME)
							timestampFirstContact = timestamp;
						else if(runPhase == runPhases.PLATFORM_INI_NO_TIME)
							initializeTimer(); //timerCount = 0

						//update event progressbar
						updateProgressBar = new UpdateProgressBar (
								true, //isEvent
								true, //percentageMode
								2 //normal run, phase 2/3
								);  
						needUpdateEventProgressBar = true;

						feedbackMessage = "";
						needShowFeedbackMessage = true; 

						runPhase = runPhases.RUNNING;
					}
				}
			}
		} while ( ! success && ! cancel );
	}
	
	protected override bool shouldFinishByTime() {
		return false; //this kind of events (simple runs) cannot be finished by time
	}
	
	protected override void updateTimeProgressBar() {
		/* 4 situations:
		 *   1- if we start out and have not arrived to platform, it should be a pulse with no time value on label:
		 *		case runPhases.PRE_RUNNING
		 *   2-  if we are on the platform, it should be a pulse
		 *   		a) if speedStartArrival (time starts at arriving at platform) || measureReactionTime
		 *   		then time starts and have to be time value on label:
		 *			case runPhases.PLATFORM_INI_YES_TIME
		 *   		b) if ! speedStartArrival (time starts at leaving platform)
		 *   		then time starts and do not have to be time value on label:
		 *			case runPhases.PLATFORM_INI_NO_TIME
		 *   3- if we leave the platform, it should be a pulse with timerCount on label:
		 *		case runPhases.RUNNING
		 *   4- if we arrive (finish), it should be a pulse with chronopic time on label:
		 *		case runPhases.PLATFORM_END.
		 *		Don't update time label here because later it will be overrided with the good data from Chronopic
		 *		and sometimes can happen in different order, and then bad data (timerCount) will be shown on label at the end of test
		 */
			
		if(runPhase == runPhases.PLATFORM_END) //see comment above
			return;
		
		double myTimeValue = 0;
		switch (runPhase) {
			case runPhases.PRE_RUNNING:
				myTimeValue = -1; //don't show nothing on label_timer 
				break;
			case runPhases.PLATFORM_INI_NO_TIME:
				myTimeValue = -1;
				break;
			case runPhases.PLATFORM_INI_YES_TIME:
				myTimeValue = timerCount; //show time from the timerCount
				break;
			case runPhases.RUNNING:
				myTimeValue = timerCount; //show time from the timerCount
				break;
		}
				
		
		//has no finished, but move progressbar time
		progressBarEventOrTimePreExecution(
				false, //isEvent false: time
				false, //activity mode
				myTimeValue
				); 
	}

	protected override void write()
	{
		LogB.Information(string.Format("TIME: {0}", time.ToString()));
		
		/*
		string myStringPush =   Catalog.GetString("Last run") + ": " + RunnerName + " " + 
			type + " " + Catalog.GetString("time") + ": " + Util.TrimDecimals( time.ToString(), pDN ) + 
			" " + Catalog.GetString("speed") + ": " + Util.TrimDecimals ( (distance/time).ToString(), pDN );
		*/
		if(simulated)
			feedbackMessage = Catalog.GetString(Constants.SimulatedMessage);
		else
			feedbackMessage = "";
		needShowFeedbackMessage = true; 


		string description = "";
		if(type == "Margaria") {
			// P = W * 9.8 * D / t   
			// W: person weight
			// D: distance between 3d and 9th stair
			double weight = SqlitePersonSession.SelectAttribute(false, personID, sessionID, Constants.Weight);
			double distanceMeters = distance / 1000;
			description = "P = " + Util.TrimDecimals ( (weight * 9.8 * distanceMeters / time).ToString(), pDN) + " (Watts)";
		} else if(type == "Gesell-DBT") 
			description = "0";

		if(measureReactionTime && reactionTimeMS > 0)
			description += descriptionAddReactionTime(reactionTimeMS, pDN, speedStartArrival);

		string table = Constants.RunTable;

		uniqueID = SqliteRun.Insert(false, table, "NULL", personID, sessionID, 
				type, distance, time, description, 
				Util.BoolToNegativeInt(simulated), 
				!startIn	//initialSpeed true if not startIn
				); 
		
		//define the created object
		eventDone = new Run(uniqueID, personID, sessionID, type, distance, time, description, Util.BoolToNegativeInt(simulated), !startIn); 

		//app1.PrepareRunSimpleGraph(time, distance/time);
		PrepareEventGraphRunSimpleObject = new PrepareEventGraphRunSimple(time, distance/time, sessionID, personID, table, type);
		needUpdateGraphType = eventType.RUN;
		needUpdateGraph = true;
		
		needEndEvent = true; //used for hiding some buttons on eventWindow
	}

	protected string reactionTimeIncludedStr = Catalog.GetString("Included on race time");
	protected string reactionTimeNotIncludedStr = Catalog.GetString("Not included on race time");

	protected string descriptionAddReactionTime(double rtimeMS, int ndec, bool speedStartArrival)
	{
		string str = "";
		if(speedStartArrival)
			str = reactionTimeIncludedStr;
		else
			str = reactionTimeNotIncludedStr;

		return Util.TrimDecimals(rtimeMS / 1000.0, ndec) + " ms (" + str + ")";
	}

	public string RunnerName
	{
		get { return SqlitePerson.SelectAttribute(personID, Constants.Name); }
	}

	~RunExecute() {}
	   
}

public class RunIntervalExecute : RunExecute
{
	double distanceTotal;
	double timeTotal;
	double distanceInterval;
	string intervalTimesString;
	double tracks; //double because if we limit by time (runType tracksLimited false), we do n.nn tracks
	string limited; //the teorically values, eleven runs: "11=R" (time recorded in "time"), 10 seconds: "10=T" (tracks recorded in tracks)
	double limitAsDouble;	//-1 for non limited (unlimited repetitive run until "finish" is clicked)
	bool tracksLimited;


	string distancesString; //if distances are variable (distanceInterval == -1), this is used
	double distanceIntervalFixed; //if distanceInterval == -1, then Fixed is the corresponding base on distancesString
	double lastTc;		//useful to know time on contact platform because intervalTimesString does not differentiate
							
	bool RSABellDone;

	//private Chronopic cp;
	private static RunExecuteInspector runEI;

	public RunIntervalExecute() {
	}

	//run execution
	public RunIntervalExecute(int personID, int sessionID, string type, double distanceInterval, double limitAsDouble, bool tracksLimited,  
			Chronopic cp, Gtk.Label event_execute_label_message, Gtk.Window app, int pDN, bool metersSecondsPreferred, 
			bool volumeOn, Preferences.GstreamerTypes gstreamer,
			RepetitiveConditionsWindow repetitiveConditionsWin,
			double progressbarLimit, ExecutingGraphData egd ,
			Constants.DoubleContact checkDoubleContactMode, int checkDoubleContactTime, 
			bool speedStartArrival, bool measureReactionTime
			)
	{
		this.personID = personID;
		this.sessionID = sessionID;
		this.type = type;
		this.distanceInterval = distanceInterval;
		this.limitAsDouble = limitAsDouble;
		this.tracksLimited = tracksLimited;

		//if distances are variable
		distancesString = "";
		if(distanceInterval == -1) {
			RunType runType = SqliteRunIntervalType.SelectAndReturnRunIntervalType(type, false);
			distancesString = runType.DistancesString;
		}


		if(tracksLimited) {
			this.limited = limitAsDouble.ToString() + "R"; //'R'uns (don't put 'T'racks for not confusing with 'T'ime)
		} else {
			this.limited = limitAsDouble.ToString() + "T";
			timeTotal = limitAsDouble;
		}
		
		
		this.cp = cp;
		this.event_execute_label_message = event_execute_label_message;
		this.app = app;

		this.metersSecondsPreferred = metersSecondsPreferred;
		this.pDN = pDN;
		this.volumeOn = volumeOn;
		this.gstreamer = gstreamer;
		this.repetitiveConditionsWin = repetitiveConditionsWin;
		this.progressbarLimit = progressbarLimit;
		this.egd = egd;
		this.checkDoubleContactMode = checkDoubleContactMode;
		this.checkDoubleContactTime = checkDoubleContactTime;
		this.speedStartArrival = speedStartArrival;	
		this.measureReactionTime = measureReactionTime;

		reactionTimeMS = 0;
		reactionTimeIncludedStr = Catalog.GetString("Included on race time of first track");
		reactionTimeNotIncludedStr = Catalog.GetString("Not included on race time of first track");

		fakeButtonUpdateGraph = new Gtk.Button();
		fakeButtonThreadDyed = new Gtk.Button();

		simulated = false;
		
		needUpdateEventProgressBar = false;
		needUpdateGraph = false;
		
		timesForSavingRepetitive = 1; //number of times that this repetive event needs for being recorded in temporal table

		//initialize eventDone as a RunInterval
		eventDone = new RunInterval();
	}


	protected override void waitEvent ()
	{
		double timestamp = 0;
		bool success = false;
		string equal = "";
		
		//initialize variables
		intervalTimesString = "";
		tracks = 0;
		bool ok;
		timerCount = 0;
		//bool initialized = false;
		int countForSavingTempTable = 0;
		lastTc = 0;
		distanceIntervalFixed = distanceInterval;

		timestampDCInitValues();
		
		//prepare variables to allow being cancelled or finished
		if(! simulated)
			Chronopic.InitCancelAndFinish();

		runEI = new RunExecuteInspector(
				RunExecuteInspector.Types.RUN_INTERVAL,
				speedStartArrival,
				checkDoubleContactMode,
				checkDoubleContactTime
				);
		runEI.ChangePhase(RunExecuteInspector.Phases.START);
		do {
			if(simulated) 
				ok = true;
			else 
				ok = cp.Read_event(out timestamp, out platformState);
		
	
			if (ok && !cancel && !finish) {
				if(distanceInterval == -1)
					distanceIntervalFixed = Util.GetRunIVariableDistancesStringRow(distancesString, (int) tracks);

				if (platformState == Chronopic.Plataforma.ON && loggedState == States.OFF) {
					//has arrived
					loggedState = States.ON;
					
					//show RSA count down only on air		
					needShowCountDown = false;
					
					//if we start out, and we arrive to the platform for the first time,
					//don't record nothing
					if(runPhase == runPhases.PRE_RUNNING) {
						if(speedStartArrival || measureReactionTime) {
							runPhase = runPhases.PLATFORM_INI_YES_TIME;
							//run starts
							initializeTimer(); //timerCount = 0

							runEI.ChangePhase(RunExecuteInspector.Phases.IN,
								"TimerStart");
						} else
						{
							runPhase = runPhases.PLATFORM_INI_NO_TIME;
							runEI.ChangePhase(RunExecuteInspector.Phases.IN,
								"No timer start until leave plaform");
						}

						feedbackMessage = "";
						needShowFeedbackMessage = true; 
					}
					else {
						runPhase = runPhases.RUNNING;
						//has arrived and not in the "running previous"
						
						//if interval run is "unlimited" not limited by tracks, nor time, 
						//then play with the progress bar until finish button is pressed
						
						if(simulated)
							timestamp = simulatedTimeLast * 1000; //conversion to milliseconds

						string runEIString = "";

						if(checkDoubleContactMode != Constants.DoubleContact.NONE) {
							if(timestamp <= checkDoubleContactTime) {
								/*
								   when checking double contact
								   first time that timestamp < checkDoubleContactTime
								   and we arrived (it's a flight time)
								   record this time as timestampDCFlightTimes
								 */
								timestampDCn ++;
								timestampDCFlightTimes += timestamp;
								runEI.ChangePhase(RunExecuteInspector.Phases.IN,
									string.Format("RUNNING, DOUBLECONTACT, timestamp: {0}, " +
										"has been added to timestampDCFLightTimes: {1}",
										Math.Round(timestamp/1000.0, 3),
										Math.Round(timestampDCFlightTimes/1000.0, 3)
										));

								continue;
							}
							else {
								if(timestampDCn > 0) {
									if(checkDoubleContactMode == 
											Constants.DoubleContact.FIRST) {
										/* user want first flight time,
										   then add all DC times*/

										double timestampTemp = timestamp;
										timestamp += timestampDCFlightTimes + 
											timestampDCContactTimes;

										runEIString = string.Format("RUNNING, DoubleContactMode.FIRST, timestamp was: {0} " +
												"added DCFLightTimes: {1} and DCContactTimes: {2}, " +
												"now timestamp is: {3}",
												Math.Round(timestampTemp/1000.0, 3), Math.Round(timestampDCFlightTimes/1000.0, 3),
												Math.Round(timestampDCContactTimes/1000.0, 3), Math.Round(timestamp/1000.0, 3));
									}
									else if(checkDoubleContactMode == 
											Constants.DoubleContact.LAST) {
										//user want last flight time, take that
										// so we don't need to change timestamp. Or we could do (it triggers a warning):
										// timestamp = timestamp;

										runEIString = string.Format("RUNNING, DoubleContactMode.LAST" +
												"do not change timestamp, timestamp is: {0}", timestamp/1000.0);
									}
									else {	/* do the avg of all flights and contacts
										   then add to last timestamp */
										double timestampTemp = timestamp;
										timestamp += 
											(timestampDCFlightTimes + 
											 timestampDCContactTimes) 
											/ timestampDCn;

										runEIString = string.Format("RUNNING, DoubleContactMode.AVERAGE, timestamp was: {0} " +
												"added (DCFLightTimes: {1} + DCContactTimes: {2}) / n: {3}, " +
												"now timestamp is: {4}",
												Math.Round(timestampTemp/1000.0, 3), Math.Round(timestampDCFlightTimes/1000.0, 3),
												Math.Round(timestampDCContactTimes/1000.0, 3), timestampDCn,
												Math.Round(timestamp/1000.0, 3));
									}

									//init values of timestampDC for next track
									timestampDCInitValues();
								}
							}
						}

						//note in double contacts mode timestamp can have added DCFlightTimes and DCContactTimes. So contact time is not only on lastTc
						double myRaceTime = lastTc + timestamp/1000.0;

						runEI.ChangePhase(RunExecuteInspector.Phases.IN, runEIString +
								string.Format("; timestamp: {0}; <b>trackTime: {1}</b>",
									Math.Round(timestamp/1000.0, 3), Math.Round(myRaceTime, 3)));

						LogB.Information(string.Format("RACE ({0}) TC: {1}; TV: {2}; TOTALTIME: {3}", tracks, lastTc, timestamp/1000.0, myRaceTime));

						if(intervalTimesString.Length > 0) { equal = "="; }
						intervalTimesString = intervalTimesString + equal + myRaceTime.ToString();
						updateTimerCountWithChronopicData(intervalTimesString);
						tracks ++;	

						//save temp table if needed
						countForSavingTempTable ++;
						if(countForSavingTempTable == timesForSavingRepetitive) {
								writeRunInterval(true); //tempTable
								countForSavingTempTable = 0;
						}
					
						if(limitAsDouble == -1) {
							//has arrived, unlimited
							updateProgressBar= new UpdateProgressBar (
									true, //isEvent
									true, //unlimited: activity mode
									tracks
									);  
							needUpdateEventProgressBar = true;
						}
						else {
							//has arrived, limited
							if (tracksLimited) {
								//has arrived, limited by tracks
								if(tracks >= limitAsDouble) 
								{
									runPhase = runPhases.PLATFORM_END;

									//finished
									writeRunInterval(false); //tempTable = false
									success = true;
								}
								//progressBarEventOrTimePreExecution(
								updateProgressBar= new UpdateProgressBar (
										true, //isEvent
										true, //tracksLimited: percentageMode
										tracks
										);  
								needUpdateEventProgressBar = true;
							} else {
								//has arrived, limited by time
								updateProgressBar= new UpdateProgressBar (
										true, //isEvent
										false, //timeLimited: activity mode
										tracks
										);  
								needUpdateEventProgressBar = true;
							}
						}

						distanceTotal = Util.GetRunITotalDistance(distanceInterval, distancesString, tracks);

						//update graph
						PrepareEventGraphRunIntervalObject = new PrepareEventGraphRunInterval(
								distanceIntervalFixed, myRaceTime, intervalTimesString, 
								distanceTotal, distancesString, startIn);

						needUpdateGraphType = eventType.RUNINTERVAL;
						needUpdateGraph = true;

						//put button_finish as sensitive when first jump is done (there's something recordable)
						if(tracks == 1)
							needSensitiveButtonFinish = true;
					}
				}
				else if (platformState == Chronopic.Plataforma.OFF && loggedState == States.ON) {
					//it's out, was inside (= has abandoned platform)

					lastTc = 0;
					if(runPhase == runPhases.PLATFORM_INI_NO_TIME) {
						//run starts
						initializeTimer();
						runEI.ChangePhase(RunExecuteInspector.Phases.OUT, "Timer start");

						feedbackMessage = "";
						needShowFeedbackMessage = true; 
					} else if(runPhase == runPhases.PLATFORM_INI_YES_TIME)
					{
						if(measureReactionTime)
							reactionTimeMS = timestamp;

						// measuringReactionTime or not, if speedStartArrival, 1st race time should include lastTc
						if(speedStartArrival)
						{
							lastTc = timestamp / 1000.0;
							runEI.ChangePhase(RunExecuteInspector.Phases.OUT,
								string.Format("SpeedStartArrival, tc = {0}", Math.Round(lastTc, 3)));
						}

						feedbackMessage = "";
						needShowFeedbackMessage = true; 
					} else {
						if(checkDoubleContactMode != Constants.DoubleContact.NONE && timestampDCn > 0)
						{
							timestampDCContactTimes += timestamp;
							runEI.ChangePhase(RunExecuteInspector.Phases.OUT,
								string.Format("RUNNING double contact, timestampDCContactTimes = {0}", Math.Round(timestampDCContactTimes/1000.0, 3)));
						}
						else {
							lastTc = timestamp / 1000.0;
							runEI.ChangePhase(RunExecuteInspector.Phases.OUT,
								string.Format("RUNNING, tc = {0}", Math.Round(lastTc, 3)));
						}
						
						
						//RSA
						double RSAseconds = Util.GetRunIVariableDistancesThisRowIsRSA(
								distancesString, Convert.ToInt32(tracks));
						if(RSAseconds > 0) {
							RSABellDone = false;
							needShowCountDown = true;
						} else {
							needShowCountDown = false;
							feedbackMessage = "";
							needShowFeedbackMessage = true;
						}

					}

						
					runPhase = runPhases.RUNNING;

					//change the automata state
					loggedState = States.OFF;
				}
			}
		} while ( ! success && ! cancel && ! finish );
		runEI.ChangePhase(RunExecuteInspector.Phases.END);

		if (finish) {
			runPhase = runPhases.PLATFORM_END;

			//write();
			//write only if there's a run at minimum
			if(Util.GetNumberOfJumps(intervalTimesString, false) >= 1) {
				writeRunInterval(false); //tempTable = false
			} else {
				//cancel a run if clicked finish before any events done, or ended by time without events
				cancel = true;
			}
		}
	}

	protected override string countDownMessage() {
		double waitSeconds = Util.GetRunIVariableDistancesThisRowIsRSA(distancesString, Convert.ToInt32(tracks))
			 - (timerCount - Util.GetTotalTime(intervalTimesString) - lastTc);

		if (waitSeconds < 0) {
		       	if(! RSABellDone) {
				Util.PlaySound(Constants.SoundTypes.GOOD, volumeOn, gstreamer);
				RSABellDone = true;
			}
			return Catalog.GetString("Go!");
		} else {
			return string.Format(Catalog.GetPluralString(
						"Wait 1 second.",
						"Wait {0} seconds.",
						Convert.ToInt32(Math.Ceiling(waitSeconds))),
					Math.Ceiling(waitSeconds));
		}

	}

	protected override bool shouldFinishByTime() {
		//check if it should finish now (time limited, not unlimited and time exceeded)
		//check that the run started
		//if( ! tracksLimited && limitAsDouble != -1 && timerCount > limitAsDouble 
		if( ! tracksLimited && limitAsDouble != -1 && Util.GetTotalTime(intervalTimesString) > limitAsDouble 
				&& !(runPhase == runPhases.PRE_RUNNING) 
				&& !(runPhase == runPhases.PLATFORM_INI_NO_TIME)
				&& !(runPhase == runPhases.PLATFORM_INI_YES_TIME)
				) 
			return true;
		else
			return false;
	}
	
	protected override void updateTimeProgressBar() {
		/* 4 situations:
		 *   1- if we start out and have not arrived to platform, it should be a pulse with no time value on label:
		 *		case runPhases.PRE_RUNNING
		 *   2-  if we are on the platform, it should be a pulse
		 *   		a) if speedStartArrival (time starts at arriving at platform) || measureReactionTime
		 *   		then time starts and have to be time value on label:
		 *			case runPhases.PLATFORM_INI_YES_TIME
		 *   		b) if ! speedStartArrival (time starts at leaving platform)
		 *   		then time starts and do not have to be time value on label:
		 *			case runPhases.PLATFORM_INI_NO_TIME
		 *   3- we are in the platform or outside at any time except 1,2 and 4. timerCount have to be shown, and progress should be Fraction or Pulse depending on if it's time limited or not:
		 *		case runPhases.RUNNING
		 *   4- if we arrive (finish), it should be a pulse with chronopic time on label:
		 *		case runPhases.PLATFORM_END
		 *		Don't update time label here because later it will be overrided with the good data from Chronopic
		 *		and sometimes can happen in different order, and then bad data (timerCount) will be shown on label at the end of test
		 */
		
		if(runPhase == runPhases.PLATFORM_END) //see comment above
			return;
		
		double myTimeValue = 0;
		bool percentageMode = true; //false is activity mode
		switch (runPhase) {
			case runPhases.PRE_RUNNING:
				percentageMode = false;
				myTimeValue = -1; //don't show nothing on label_timer 
				break;
			case runPhases.PLATFORM_INI_NO_TIME:
				percentageMode = false;
				myTimeValue = -1;
				break;
			case runPhases.PLATFORM_INI_YES_TIME:
				percentageMode = !tracksLimited;
				myTimeValue = timerCount; //show time from the timerCount
				break;
			case runPhases.RUNNING:
				percentageMode = !tracksLimited;
				myTimeValue = timerCount; //show time from the timerCount
				break;
		}
			
		if(! finish) 
			progressBarEventOrTimePreExecution(
					false, //isEvent false: time
					//!tracksLimited, //if tracksLimited: activity, if timeLimited: fraction
					percentageMode,
					myTimeValue
					); 
	}

	private void updateTimerCountWithChronopicData(string timesString) {
		//update timerCount, with the chronopic data
		timerCount =  Util.GetTotalTime(timesString);
	}
				
	protected void writeRunInterval(bool tempTable)
	{
		int tracksHere = 0; //different than globakl tracks variable
		string limitString = "";

		//if user clicked in finish earlier
		if(finish) {
			if(tracksLimited) {
				tracksHere = Util.GetNumberOfJumps(intervalTimesString, false);
				limitString = tracksHere.ToString() + "R";
			} else {
				//when we mark that run should finish by time, chronopic thread is probably capturing data
				//check if it captured more than date limit, and if it has done, delete last(s) run(s)
				if(limitAsDouble != -1) {
					bool eventPassed = Util.EventPassedFromMaxTime(intervalTimesString, limitAsDouble);
					while(eventPassed) {
						intervalTimesString = Util.DeleteLastSubEvent(intervalTimesString);

						//run limited by time that first subRun has arrived later than maximum for the whole run,
						//and DeleteLastSubEvent returns "-" as a mark
						if(intervalTimesString[0] == '-') {
							//this dialog can make crash the software because the non-gui part calls it
							//new DialogMessage(Constants.MessageTypes.WARNING, 
							//		Catalog.GetString("Race will not be recorded, 1st lap is out of time"));

							feedbackMessage = Catalog.GetString("Race will not be recorded. Out of time.");
							needShowFeedbackMessage = true; 
							LogB.Information("Race will not be recorded, 1st lap is out of time");
	
							//mark for not having problems with cancelled
							cancel = true;

							//end this piece of code
							return;
						} else {
							LogB.Information("Deleted one event out of time");
							eventPassed = Util.EventPassedFromMaxTime(intervalTimesString, limitAsDouble);
						}
					}
				}
				//tracksHere are defined here (and not before) because can change on "while(eventPassed)" before
				tracksHere = Util.GetNumberOfJumps(intervalTimesString, false);
				limitString = Util.GetTotalTime(intervalTimesString) + "T";
			}
		} else {
			if(tracksLimited) {
				limitString = limitAsDouble.ToString() + "R";
				tracksHere = (int) limitAsDouble;
			} else {
				limitString = limitAsDouble.ToString() + "T";
				string [] myStringFull = intervalTimesString.Split(new char[] {'='});
				tracksHere = myStringFull.Length;
			}
		}

		distanceTotal = Util.GetRunITotalDistance(distanceInterval, distancesString, tracksHere);
		timeTotal = Util.GetTotalTime(intervalTimesString); 
		

		string description = "";
		if(type == "MTGUG")
			description = "u u u u u u"; //undefined 6 items of questionnaire
		//note MTGUG will not have reaction time measurement to have description read correctly by the rest of the software
		else if(measureReactionTime && reactionTimeMS > 0)
			description += descriptionAddReactionTime(reactionTimeMS, pDN, speedStartArrival);



		if(tempTable)
			SqliteRunInterval.Insert(false, Constants.TempRunIntervalTable, "NULL", personID, sessionID, type, 
					distanceTotal, timeTotal,
					distanceInterval, intervalTimesString, tracksHere, 
					description,
					limitString,
					Util.BoolToNegativeInt(simulated),
					!startIn	//initialSpeed true if not startIn
					);
		else {
			uniqueID = SqliteRunInterval.Insert(false, Constants.RunIntervalTable, "NULL", personID, sessionID, type, 
					distanceTotal, timeTotal,
					distanceInterval, intervalTimesString, tracksHere, 
					description,
					limitString,
					Util.BoolToNegativeInt(simulated),
					!startIn
					);

			//define the created object
			eventDone = new RunInterval(uniqueID, personID, sessionID, type, distanceTotal, timeTotal, distanceInterval, intervalTimesString, tracksHere, description, limitString, Util.BoolToNegativeInt(simulated), !startIn); 

			/*
			string tempValuesString;
			if(tracksLimited) 
				tempValuesString = " (" + distanceIntervalFixed + "x" + tracksHere + "R), " + Catalog.GetString("Time") + ": " + Util.TrimDecimals( timeTotal.ToString(), pDN);
			else
				tempValuesString = " (" + distanceIntervalFixed + "x" + Util.TrimDecimals( timeTotal.ToString(), pDN) + "T), " + Catalog.GetString("Tracks") + ": " + tracksHere;


			string myStringPush =   Catalog.GetString("Last run") + ": " + RunnerName + ", " + 
				type + tempValuesString + ", " +
				Catalog.GetString("AVG Speed") + ": " + Util.TrimDecimals( 
						Util.GetSpeed(distanceTotal.ToString(),
							timeTotal.ToString(), metersSecondsPreferred )
						, pDN ) ;
			*/
			if(simulated)
				feedbackMessage = Catalog.GetString(Constants.SimulatedMessage);
			else
				feedbackMessage = "";
			needShowFeedbackMessage = true; 

			PrepareEventGraphRunIntervalObject = new PrepareEventGraphRunInterval(
					distanceIntervalFixed, Util.GetLast(intervalTimesString), 
					intervalTimesString, distanceTotal, distancesString, startIn);
			needUpdateGraphType = eventType.RUNINTERVAL;
			needUpdateGraph = true;

			needEndEvent = true; //used for hiding some buttons on eventWindow, and also for updateTimeProgressBar here
		}
	}

	public override string GetInspectorMessages()
	{
		return runEI.ToString();
	}
	
	/*
	public string IntervalTimesString
	{
		get { return intervalTimesString; }
		set { intervalTimesString = value; }
	}
	
	public double DistanceInterval
	{
		get { return distanceInterval; }
		set { distanceInterval = value; }
	}
		
	public double DistanceTotal
	{
		get { return distanceTotal; }
		set { distanceTotal = value; }
	}
		
	public double TimeTotal
	{
		get { return timeTotal; }
		set { timeTotal = value; }
	}
		
	public double Tracks
	{
		get { return tracks; }
		set { tracks = value; }
	}
		
	public string Limited
	{
		get { return limited; }
		set { limited = value; }
	}

	public bool TracksLimited
	{
		get { return tracksLimited; }
	}
		
	public override double Speed
	{
		get { 
			//if(metersSecondsPreferred) 
				return distanceTotal / timeTotal ; 
			// else 
			//	return (distanceTotal / timeTotal) * 3.6 ; 
		}
	}
	*/
		
	~RunIntervalExecute() {}
}

public class RunExecuteInspector
{
	public enum Types { RUN_SIMPLE, RUN_INTERVAL }
	private Types type;

	public enum Phases { START, IN, OUT, END }
	private DateTime dtStarted;
	private DateTime dtEnded;

	private bool speedStartArrival;
	Constants.DoubleContact checkDoubleContactMode;
	int checkDoubleContactTime;

	private List<InOut> listInOut;


	//constructor
	public RunExecuteInspector(Types type, bool speedStartArrival,
			Constants.DoubleContact checkDoubleContactMode, int checkDoubleContactTime)
	{
		this.type = type;
		this.speedStartArrival = speedStartArrival;
		this.checkDoubleContactMode = checkDoubleContactMode;
		this.checkDoubleContactTime = checkDoubleContactTime;

		listInOut = new List<InOut>();
	}

	//public methods

	public void ChangePhase(Phases phase)
	{
		ChangePhase(phase, "");
	}
	public void ChangePhase(Phases phase, string message)
	{
		DateTime dt = DateTime.Now;

		if(phase == Phases.START)
			dtStarted = dt;
		else if(phase == Phases.END)
			dtEnded = dt;
		else // (phase == Phases.IN || phases == Phases.OUT)
		{
			InOut inOut = new InOut(phase == Phases.IN, dt, message);
			listInOut.Add(inOut);
			//listInOut.Add(new InOut(phase == Phases.IN, dt, message));
		}
	}

	public override string ToString()
	{
		string report = "Report of race started at: " + dtStarted.ToShortTimeString();
		report += "\n" + "Type: " + type.ToString();
		report += "\n" + "SpeedStartArrival: " + speedStartArrival;
		report += "\n" + "CheckDoubleContactMode: " + checkDoubleContactMode;
		report += "\n" + "CheckDoubleContactTime: " + checkDoubleContactTime;

		report += "\n" + "Chronopic changes:";
		foreach(InOut inOut in listInOut)
		{
			report += inOut.ToString();
		}
		report += "\n\n";
		report += "\n" + "Ended: " + dtEnded.ToShortTimeString();

		return report;
	}
}
