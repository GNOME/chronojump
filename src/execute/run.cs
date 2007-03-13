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

using System.Threading;
using System.IO.Ports;
using Mono.Unix;

public class RunExecute : EventExecute
{
	protected double distance;
	protected double time;

	//for not checking always in database
	protected bool startIn;
	
	
	protected Chronopic cp;
	//private Chronopic cp; //thi doesn't work
	protected bool metersSecondsPreferred;

	//used by the updateTimeProgressBar for display its time information
	//changes a bit on runSimple and runInterval
	//explained at each of the updateTimeProgressBar() 
	protected enum runPhases {
		PRE_RUNNING, PLATFORM_INI, RUNNING, PLATFORM_END
	}
	protected runPhases runPhase;
		
	
	public RunExecute() {
	}

	//run execution
	public RunExecute(EventExecuteWindow eventExecuteWin, int personID, int sessionID, string type, double distance,   
			Chronopic cp, Gtk.Statusbar appbar, Gtk.Window app, int pDN, bool metersSecondsPreferred)
	{
		this.eventExecuteWin = eventExecuteWin;
		this.personID = personID;
		this.sessionID = sessionID;
		this.type = type;
		this.distance = distance;
		
		this.cp = cp;
		this.appbar = appbar;
		this.app = app;

		this.pDN = pDN;
		this.metersSecondsPreferred = metersSecondsPreferred;
		
		fakeButtonFinished = new Gtk.Button();

		simulated = false;
		
		needUpdateEventProgressBar = false;
		needUpdateGraph = false;
		
		//initialize eventDone as a Run	
		eventDone = new Run();
	}

	
	public override void SimulateInitValues(Random randSent)
	{
		Console.WriteLine("From execute/run.cs");

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
Console.WriteLine("MANAGE!!!!");
		if (simulated) 
			platformState = Chronopic.Plataforma.ON;
		else
			platformState = chronopicInitialValue(cp);

Console.WriteLine("MANAGE(b)!!!!");
		
		//you can start ON or OFF the platform, 
		//we record always de TF (or time between we abandonate the platform since we arrive)
		if (platformState==Chronopic.Plataforma.ON) {
			appbar.Push( 1,Catalog.GetString("You are IN, RUN when prepared!!") );

			loggedState = States.ON;
			startIn = true;
			runPhase = runPhases.PLATFORM_INI;
		} else {
			appbar.Push( 1,Catalog.GetString("You are OUT, RUN when prepared!!") );

			loggedState = States.OFF;
			startIn = false;
			runPhase = runPhases.PRE_RUNNING;
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
		totallyCancelled = false;

Console.WriteLine("MANAGE(2)!!!!");
		//start thread
		thread = new Thread(new ThreadStart(waitEvent));
		GLib.Idle.Add (new GLib.IdleHandler (PulseGTK));
		thread.Start(); 
Console.WriteLine("MANAGE(3)!!!!");
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
				if (platformState == Chronopic.Plataforma.ON && loggedState == States.OFF) {
					//has arrived
					loggedState = States.ON;
					
					if(runPhase == runPhases.PRE_RUNNING) {
						runPhase = runPhases.PLATFORM_INI;
						
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
						
						time = timestamp / 1000;
						write();

						success = true;
						
						updateProgressBar = new UpdateProgressBar (
								true, //isEvent
								true, //percentageMode
								//percentageToPass
								3
								);  
						needUpdateEventProgressBar = true;
						
						runPhase = runPhases.PLATFORM_END;
					}
				}
				else if (platformState == Chronopic.Plataforma.OFF && loggedState == States.ON) {
					//it's out, was inside (= has abandoned platform)
					//don't record time
						
					initializeTimer();

					//update event progressbar
					updateProgressBar = new UpdateProgressBar (
							true, //isEvent
							true, //percentageMode
							2 //normal run, phase 2/3
							);  
					needUpdateEventProgressBar = true;
					
					//change the automata state
					loggedState = States.OFF;
						
					runPhase = runPhases.RUNNING;
				}
			}
		} while ( ! success && ! cancel );

		if(cancel) {
			//event will be raised, and managed in chronojump.cs
			fakeButtonFinished.Click();

			totallyCancelled = true;
		}
	}
	
	protected override bool shouldFinishByTime() {
		return false; //this kind of events (simple runs) cannot be finished by time
	}
	
	protected override void updateTimeProgressBar() {
		/* 4 situations:
		 *   1- if we start out and have not arrived to platform, it should be a pulse with no time value on label
			case runPhases.PRE_RUNNING:
		 *   2-  if we are on the platform, it should be a pulse with no time value on label
			case runPhases.PLATFORM_INI:
		 *   3- if we leave the platform, it should be a pulse with timerCount on label
			case runPhases.RUNNING:
		 *   4- if we arrive (finish), it should be a pulse with chronopic time on label
			case runPhases.PLATFORM_END:
		 */
		
		double myTimeValue = 0;
		switch (runPhase) {
			case runPhases.PRE_RUNNING:
				myTimeValue = -1; //don't show nothing on label_timer 
				break;
			case runPhases.PLATFORM_INI:
				myTimeValue = -1;
				break;
			case runPhases.RUNNING:
				myTimeValue = timerCount; //show time from the timerCount
				break;
			case runPhases.PLATFORM_END:
				myTimeValue = timerCount; //show time from the timerCount
				//but chronojump.cs will update info soon with chronopic value
				break;
		}
				
		
		//has no finished, but move progressbar time
		eventExecuteWin.ProgressBarEventOrTimePreExecution(
				false, //isEvent false: time
				false, //activity mode
				myTimeValue
				); 
	}

	protected override void write()
	{
		Console.WriteLine("TIME: {0}", time.ToString());
		
		string myStringPush =   Catalog.GetString("Last run") + ": " + RunnerName + " " + 
			type + " " + Catalog.GetString("time") + ": " + Util.TrimDecimals( time.ToString(), pDN ) + 
			" " + Catalog.GetString("speed") + ": " + Util.TrimDecimals ( (distance/time).ToString(), pDN );
		appbar.Push( 1,myStringPush );

		uniqueID = SqliteRun.Insert(personID, sessionID, 
				type, distance, time, ""); //type, distance, time, description
		
		//define the created object
		eventDone = new Run(uniqueID, personID, sessionID, type, distance, time, ""); 
		
		
		//event will be raised, and managed in chronojump.cs
		fakeButtonFinished.Click();
		
		//eventExecuteWin.PrepareRunSimpleGraph(time, distance/time);
		prepareEventGraphRunSimple = new PrepareEventGraphRunSimple(time, distance/time);
		needUpdateGraphType = eventType.RUN;
		needUpdateGraph = true;
		
		//eventExecuteWin.EventEnded();
		needEndEvent = true; //used for hiding some buttons on eventWindow
	}

	
	public string RunnerName
	{
		get { return SqlitePerson.SelectJumperName(personID); }
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
	
	//private Chronopic cp;

	public RunIntervalExecute() {
	}

	//run execution
	public RunIntervalExecute(EventExecuteWindow eventExecuteWin, int personID, int sessionID, string type, double distanceInterval, double limitAsDouble, bool tracksLimited,  
			Chronopic cp, Gtk.Statusbar appbar, Gtk.Window app, int pDN)
	{
		this.eventExecuteWin = eventExecuteWin;
		this.personID = personID;
		this.sessionID = sessionID;
		this.type = type;
		this.distanceInterval = distanceInterval;
		this.limitAsDouble = limitAsDouble;
		this.tracksLimited = tracksLimited;

		if(tracksLimited) {
			this.limited = limitAsDouble.ToString() + "R"; //'R'uns (don't put 'T'racks for not confusing with 'T'ime)
		} else {
			this.limited = limitAsDouble.ToString() + "T";
			timeTotal = limitAsDouble;
		}
		
		
		this.cp = cp;
		this.appbar = appbar;
		this.app = app;

		this.pDN = pDN;
	
		fakeButtonFinished = new Gtk.Button();

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
	
		double lastTc = 0;
		
		do {

			if(simulated) 
				ok = true;
			else 
				ok = cp.Read_event(out timestamp, out platformState);
		
	
			if (ok && !cancel && !finish) {
				if (platformState == Chronopic.Plataforma.ON && loggedState == States.OFF) {
					//has arrived
					loggedState = States.ON;
					
					//if we start out, and we arrive to the platform for the first time, don't record nothing
					if(runPhase == runPhases.PRE_RUNNING) {
						runPhase = runPhases.RUNNING;
						//run starts
						initializeTimer();
					}
					else {
						runPhase = runPhases.RUNNING;
						//has arrived and not in the "running previous"
						
						//if interval run is "unlimited" not limited by tracks, nor time, 
						//then play with the progress bar until finish button is pressed
						if(limitAsDouble == -1) {
							
							if(simulated)
								timestamp = simulatedTimeLast * 1000; //conversion to milliseconds
							
							double myRaceTime = lastTc + timestamp/1000;
							
							if(intervalTimesString.Length > 0) { equal = "="; }
							//intervalTimesString = intervalTimesString + equal + (timestamp/1000).ToString();
							intervalTimesString = intervalTimesString + equal + myRaceTime.ToString();
							updateTimerCountWithChronopicData(intervalTimesString);
							tracks ++;	
								
							//eventExecuteWin.ProgressBarEventOrTimePreExecution(
							updateProgressBar= new UpdateProgressBar (
									true, //isEvent
									true, //unlimited: activity mode
									tracks
									);  
							needUpdateEventProgressBar = true;
							
							//update graph
							//eventExecuteWin.PrepareRunIntervalGraph(distanceInterval, timestamp/1000, intervalTimesString);
							//prepareEventGraphRunInterval = new PrepareEventGraphRunInterval(distanceInterval, timestamp/1000, intervalTimesString);
							prepareEventGraphRunInterval = new PrepareEventGraphRunInterval(distanceInterval, myRaceTime, intervalTimesString);
							needUpdateGraphType = eventType.RUNINTERVAL;
							needUpdateGraph = true;
							
							
							//put button_finish as sensitive when first jump is done (there's something recordable)
							if(tracks == 1)
								needSensitiveButtonFinish = true;
						}
						else {
							//has arrived, limited
							if (tracksLimited) {
								//has arrived, limited by tracks
								tracks ++;	

								if(simulated)
									timestamp = simulatedTimeLast * 1000; //conversion to milliseconds

								double myRaceTime = lastTc + timestamp/1000;
								if(intervalTimesString.Length > 0) { equal = "="; }
								//intervalTimesString = intervalTimesString + equal + (timestamp/1000).ToString();
								intervalTimesString = intervalTimesString + equal + myRaceTime.ToString();
								updateTimerCountWithChronopicData(intervalTimesString);

								if(tracks >= limitAsDouble) 
								{
									//finished
									writeRunInterval(false); //tempTable = false
									success = true;
									runPhase = runPhases.PLATFORM_END;
								}
								
								//eventExecuteWin.ProgressBarEventOrTimePreExecution(
								updateProgressBar= new UpdateProgressBar (
										true, //isEvent
										true, //tracksLimited: percentageMode
										tracks
										);  
								needUpdateEventProgressBar = true;
							
								//update graph
								//eventExecuteWin.PrepareRunIntervalGraph(distanceInterval, timestamp/1000, intervalTimesString);
								//prepareEventGraphRunInterval = new PrepareEventGraphRunInterval(distanceInterval, timestamp/1000, intervalTimesString);
								prepareEventGraphRunInterval = new PrepareEventGraphRunInterval(distanceInterval, myRaceTime, intervalTimesString);
								needUpdateGraphType = eventType.RUNINTERVAL;
								needUpdateGraph = true;

								//put button_finish as sensitive when first jump is done (there's something recordable)
								if(tracks == 1)
									needSensitiveButtonFinish = true;
							} else {
								//has arrived, limited by time
								runPhase = runPhases.RUNNING;
								
								if(simulated)
									timestamp = simulatedTimeLast * 1000; //conversion to milliseconds
								
								double myRaceTime = lastTc + timestamp/1000;
								if(success) {
									//write();
									//write only if there's a run at minimum
									if(Util.GetNumberOfJumps(intervalTimesString, false) >= 1) {
										writeRunInterval(false); //tempTable = false
									} else {
										//cancel a run if clicked finish before any events done, or ended by time without events
										cancel = true;
									}

									runPhase = runPhases.PLATFORM_END;
								}
								else {
									if(intervalTimesString.Length > 0) { equal = "="; }
									//intervalTimesString = intervalTimesString + equal + (timestamp/1000).ToString();
									intervalTimesString = intervalTimesString + equal + myRaceTime.ToString();
									updateTimerCountWithChronopicData(intervalTimesString);
									tracks ++;	
								}
								
								//eventExecuteWin.ProgressBarEventOrTimePreExecution(
								updateProgressBar= new UpdateProgressBar (
										true, //isEvent
										false, //timeLimited: activity mode
										tracks
										);  
								needUpdateEventProgressBar = true;
							
								//update graph
								//eventExecuteWin.PrepareRunIntervalGraph(distanceInterval, timestamp/1000, intervalTimesString);
								//prepareEventGraphRunInterval = new PrepareEventGraphRunInterval(distanceInterval, timestamp/1000, intervalTimesString);
								prepareEventGraphRunInterval = new PrepareEventGraphRunInterval(distanceInterval, myRaceTime, intervalTimesString);
								needUpdateGraphType = eventType.RUNINTERVAL;
								needUpdateGraph = true;

								//put button_finish as sensitive when first jump is done (there's something recordable)
								if(tracks == 1)
									needSensitiveButtonFinish = true;
							}
						}
						

						//save temp table if needed
						countForSavingTempTable ++;
						if(countForSavingTempTable == timesForSavingRepetitive) {
							writeRunInterval(true); //tempTable
							countForSavingTempTable = 0;
						}
	
					}
				}
				else if (platformState == Chronopic.Plataforma.OFF && loggedState == States.ON) {
					//it's out, was inside (= has abandoned platform)
		
					if(runPhase == runPhases.PLATFORM_INI) {
						//run starts
						initializeTimer();
						lastTc = 0;
					} else
						lastTc = timestamp/1000;
						
					runPhase = runPhases.RUNNING;

					//change the automata state
					loggedState = States.OFF;

				}
			}
		} while ( ! success && ! cancel && ! finish );

		if (finish) {
			//write();
			//write only if there's a run at minimum
			if(Util.GetNumberOfJumps(intervalTimesString, false) >= 1) {
				writeRunInterval(false); //tempTable = false
			
				totallyFinished = true;
			} else {
				//cancel a run if clicked finish before any events done, or ended by time without events
				cancel = true;
			}

			runPhase = runPhases.PLATFORM_END;
		}
		if(cancel || finish) {
			//event will be raised, and managed in chronojump.cs
			fakeButtonFinished.Click();
			
			totallyCancelled = true;
		}
	}
	
	protected override bool shouldFinishByTime() {
		//check if it should finish now (time limited, not unlimited and time exceeded)
		//check that the run started
		//if( ! tracksLimited && limitAsDouble != -1 && timerCount > limitAsDouble 
		if( ! tracksLimited && limitAsDouble != -1 && Util.GetTotalTime(intervalTimesString) > limitAsDouble 
				&& !(runPhase == runPhases.PRE_RUNNING) && !(runPhase == runPhases.PLATFORM_INI)) 
			return true;
		else
			return false;
	}
	
	protected override void updateProgressBarForFinish() {
		eventExecuteWin.ProgressBarEventOrTimePreExecution(
				false, //isEvent false: time
				true, //percentageMode: it has finished, show bar at 100%
				limitAsDouble
				);  
	}

	protected override void updateTimeProgressBar() {
		/* 4 situations:
		 *   1- if we start out and have not arrived to platform, it should be a pulse with no time value on label 
		 *   	case runPhases.PRE_RUNNING:
		 *   2- we started in, and we haven't leaved the platform, a pulse but with no time value on label
		 *   	case runPhases.PLATFORM_INI:
		 *   3- we are in the platform or outside at any time except 1,2 and 4. timerCount have to be shown, and progress should be Fraction or Pulse depending on if ot's time limited or not
		 *   	case runPhases.RUNNING:
		 *  4.- we have arrived (or jump finished at any time)
		 *   	case runPhases.PLATFORM_END:
		 */
		
		double myTimeValue = 0;
		bool percentageMode = true; //false is activity mode
		switch (runPhase) {
			case runPhases.PRE_RUNNING:
				percentageMode = false;
				myTimeValue = -1; //don't show nothing on label_timer 
				break;
			case runPhases.PLATFORM_INI:
				percentageMode = false;
				myTimeValue = -1;
				break;
			case runPhases.RUNNING:
				percentageMode = !tracksLimited;
				myTimeValue = timerCount; //show time from the timerCount
				break;
			case runPhases.PLATFORM_END:
				percentageMode = !tracksLimited;
				myTimeValue = timerCount; //show time from the timerCount
				//but chronojump.cs will update info soon with chronopic value
				break;
		}
				
		eventExecuteWin.ProgressBarEventOrTimePreExecution(
				false, //isEvent false: time
				//!tracksLimited, //if tracksLimited: activity, if timeLimited: fraction
				percentageMode,
				myTimeValue
				); 
	}

	private void updateTimerCountWithChronopicData(string timesString) {
		//update timerCount, with the chronopic data
		//Console.WriteLine("///I timerCount: {0} tcString+tvString: {1} ///", timerCount, Util.GetTotalTime(tcString) + Util.GetTotalTime(tvString));
		timerCount =  Util.GetTotalTime(timesString);
	}
				

	protected void writeRunInterval(bool tempTable)
	{
		int tracks = 0;
		string limitString = "";

		//if user clicked in finish earlier
		if(finish) {
			if(tracksLimited) {
				tracks = Util.GetNumberOfJumps(intervalTimesString, false);
				limitString = tracks.ToString() + "R";
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
							new DialogMessage(Catalog.GetString("Run will not be recorded, 1st subrun is out of time"));
	
							//mark for not having problems with cancelled
							cancel = true;
							//event will be raised, and managed in chronojump.cs
							fakeButtonFinished.Click();

							//end this piece of code
							return;
						} else {
							Console.WriteLine("Deleted one event out of time");
							eventPassed = Util.EventPassedFromMaxTime(intervalTimesString, limitAsDouble);
						}
					}
				}
				//tracks are defined here (and not before) because can change on "while(eventPassed)" before
				tracks = Util.GetNumberOfJumps(intervalTimesString, false);
				limitString = Util.GetTotalTime(intervalTimesString) + "T";
			}
		} else {
			if(tracksLimited) {
				limitString = limitAsDouble.ToString() + "R";
				tracks = (int) limitAsDouble;
			} else {
				limitString = limitAsDouble.ToString() + "T";
				string [] myStringFull = intervalTimesString.Split(new char[] {'='});
				tracks = myStringFull.Length;
			}
		}

		distanceTotal = tracks * distanceInterval;
		timeTotal = Util.GetTotalTime(intervalTimesString); 
		
	
		if(tempTable)
			{
			SqliteRun.InsertInterval("tempRunInterval", "NULL", personID, sessionID, type, 
					distanceTotal, timeTotal,
					distanceInterval, intervalTimesString, tracks, 
					"", 					//description
					limitString
					);
			}

		else {
			uniqueID = SqliteRun.InsertInterval("runInterval", "NULL", personID, sessionID, type, 
					distanceTotal, timeTotal,
					distanceInterval, intervalTimesString, tracks, 
					"", 					//description
					limitString
					);

			//define the created object
			eventDone = new RunInterval(uniqueID, personID, sessionID, type, distanceTotal, timeTotal, distanceInterval, intervalTimesString, tracks, "", limitString); 


			string myStringPush =   Catalog.GetString("Last run") + ": " + RunnerName + " " + 
				type + " (" + limitString + ") " +
				Catalog.GetString("AVG Speed") + ": " + Util.TrimDecimals( 
						Util.GetSpeed(distanceTotal.ToString(),
							timeTotal.ToString(), metersSecondsPreferred )
						, pDN ) ;
			appbar.Push( 1,myStringPush );


			//event will be raised, and managed in chronojump.cs
			fakeButtonFinished.Click();

			//eventExecuteWin.PrepareRunIntervalGraph(distanceInterval, Util.GetLast(intervalTimesString), intervalTimesString);
			prepareEventGraphRunInterval = new PrepareEventGraphRunInterval(distanceInterval, Util.GetLast(intervalTimesString), intervalTimesString);
			needUpdateGraphType = eventType.RUNINTERVAL;
			needUpdateGraph = true;

			//eventExecuteWin.EventEnded();
			needEndEvent = true; //used for hiding some buttons on eventWindow, and also for updateTimeProgressBar here
		}		
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
			//if(metersSecondsPreferred) {
				return distanceTotal / timeTotal ; 
			// else {
			//	return (distanceTotal / timeTotal) * 3.6 ; 
			//}
		}
	}
*/
	
		
		
	~RunIntervalExecute() {}
}

