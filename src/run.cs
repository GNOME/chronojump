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
using Mono.Unix;

public class Run : Event 
{
	protected double distance;
	protected double time;

	//for not checking always in database
	protected bool startIn;
	
	
	protected Chronopic cp;
	protected bool metersSecondsPreferred;

	
	public Run() {
	}

	//run execution
	public Run(EventExecuteWindow eventExecuteWin, int personID, int sessionID, string type, double distance,   
			Chronopic cp, Gtk.ProgressBar progressBar, Gtk.Statusbar appbar, Gtk.Window app, 
			int pDN, bool metersSecondsPreferred)
	{
		this.eventExecuteWin = eventExecuteWin;
		this.personID = personID;
		this.sessionID = sessionID;
		this.type = type;
		this.distance = distance;
		
		this.cp = cp;
		this.progressBar = progressBar;
		this.appbar = appbar;
		this.app = app;

		this.pDN = pDN;
		this.metersSecondsPreferred = metersSecondsPreferred;
		
		fakeButtonFinished = new Gtk.Button();

		simulated = false;
	}
	
	//after inserting database (SQL)
	public Run(int uniqueID, int personID, int sessionID, string type, double distance, double time, string description)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.sessionID = sessionID;
		this.type = type;
		this.distance = distance;
		this.time = time;
		this.description = description;
	}

	public override void SimulateInitValues(Random randSent)
	{
		Console.WriteLine("From run.cs");

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
		if (simulated) 
			platformState = Chronopic.Plataforma.ON;
		else
			platformState = chronopicInitialValue(cp);
		
		//you can start ON or OFF the platform, 
		//we record always de TF (or time between we abandonate the platform since we arrive)
		if (platformState==Chronopic.Plataforma.ON) {
			appbar.Push( 1,Catalog.GetString("You are IN, RUN when prepared!!") );

			loggedState = States.ON;
			startIn = true;
		} else {
			appbar.Push( 1,Catalog.GetString("You are OUT, RUN when prepared!!") );

			loggedState = States.OFF;
			startIn = false;
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

		//reset progressBar
		progressBar.Fraction = 0;

		//prepare jump for being cancelled if desired
		cancel = false;

		//start thread
		thread = new Thread(new ThreadStart(waitEvent));
		GLib.Idle.Add (new GLib.IdleHandler (PulseGTK));
		thread.Start(); 
	}
	
	protected override void waitEvent ()
	{
		double timestamp = 0;
		bool success = false;
		
		bool ok;
	
		//we allow start from the platform or outside
		bool arrived = false; 
		
		do {
			if(simulated)
				ok = true;
			else 
				ok = cp.Read_event(out timestamp, out platformState);
			
			if (ok) {
				if (platformState == Chronopic.Plataforma.ON && loggedState == States.OFF) {
					//has arrived
					loggedState = States.ON;
					
					if( ! startIn && ! arrived ) {
						arrived = true;
						
						initializeTimer();

						eventExecuteWin.ProgressBarEventOrTimePreExecution(
								true, //isEvent
								true, //tracksLimited: percentageMode
								1 //just reached platform, phase 1/3
								);  
					} else {
						//run finished: 
						//if started outside (behind platform) it's the second arrive
						//if started inside: it's the first arrive
						
						if(simulated)
							timestamp = simulatedTimeLast * 1000; //conversion to milliseconds
						
						time = timestamp / 1000;
						write ();

						success = true;
						
						eventExecuteWin.ProgressBarEventOrTimePreExecution(
								true, //isEvent
								true, //percentageMode
								//percentageToPass
								3
								);  
					}
				}
				else if (platformState == Chronopic.Plataforma.OFF && loggedState == States.ON) {
					//it's out, was inside (= has abandoned platform)
					//don't record time
						
					initializeTimer();

					//update event progressbar
					eventExecuteWin.ProgressBarEventOrTimePreExecution(
							true, //isEvent
							true, //percentageMode
							2 //normal run, phase 2/3
							);  
					
					//change the automata state
					loggedState = States.OFF;

				}
			}
		} while ( ! success && ! cancel );

		if(cancel) {
			//event will be raised, and managed in chronojump.cs
			fakeButtonFinished.Click();
		}
	}
	
	protected override bool shouldFinishByTime() {
		return false; //this kind of events (simple runs) cannot be finished by time
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
		Console.WriteLine("TIME: {0}", time.ToString());
		
		string myStringPush =   Catalog.GetString("Last run") + ": " + RunnerName + " " + 
			type + " " + Catalog.GetString("time") + ": " + Util.TrimDecimals( time.ToString(), pDN ) + 
			" " + Catalog.GetString("speed") + ": " + Util.TrimDecimals ( (distance/time).ToString(), pDN );
		appbar.Push( 1,myStringPush );

		uniqueID = SqliteRun.Insert(personID, sessionID, 
				type, distance, time, ""); //type, distance, time, description
		
		//event will be raised, and managed in chronojump.cs
		fakeButtonFinished.Click();
		
		//put max value in progressBar. This makes the thread in PulseGTK() stop
		//progressBar.Fraction = 1;
		
		//eventExecuteWin.EventEnded(-1, -1);
		eventExecuteWin.PrepareRunSimpleGraph(time, distance/time);
		eventExecuteWin.EventEnded();
	}
	

	
	public virtual double Speed
	{
		get { 
			if(metersSecondsPreferred) {
				return distance / time ; 
			} else {
				return (distance / time) * 3.6 ; 
			}
		}
	}
	
	public double Distance
	{
		get { return distance; }
		set { distance = value; }
	}
	
	public double Time
	{
		get { return time; }
		set { time = value; }
	}
	
	public string RunnerName
	{
		get { return SqlitePerson.SelectJumperName(personID); }
	}

	~Run() {}
	   
}

public class RunInterval : Run
{
	double distanceTotal;
	double timeTotal;
	double distanceInterval;
	string intervalTimesString;
	double tracks; //double because if we limit by time (runType tracksLimited false), we do n.nn tracks
	string limited; //the teorically values, eleven runs: "11=R" (time recorded in "time"), 10 seconds: "10=T" (tracks recorded in tracks)
	double limitAsDouble;	//-1 for non limited (unlimited repetitive run until "finish" is clicked)
	bool tracksLimited;
	bool firstIntervalValue;

	

	public RunInterval() {
	}

	//run execution
	public RunInterval(EventExecuteWindow eventExecuteWin, int personID, int sessionID, string type, double distanceInterval, double limitAsDouble, bool tracksLimited,  
			Chronopic cp, Gtk.ProgressBar progressBar, Gtk.Statusbar appbar, Gtk.Window app, 
			int pDN)
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
		this.progressBar = progressBar;
		this.appbar = appbar;
		this.app = app;

		this.pDN = pDN;
	
		fakeButtonFinished = new Gtk.Button();

		simulated = false;
	}
	
	
	//after inserting database (SQL)
	public RunInterval(int uniqueID, int personID, int sessionID, string type, double distanceTotal, double timeTotal, double distanceInterval, string intervalTimesString, double tracks, string description, string limited)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.sessionID = sessionID;
		this.type = type;
		this.distanceTotal = distanceTotal;
		this.timeTotal = timeTotal;
		this.distanceInterval = distanceInterval;
		this.intervalTimesString = intervalTimesString;
		this.tracks = tracks;
		this.description = description;
		this.limited = limited;
	}

	protected override void waitEvent ()
	{
		double timestamp = 0;
		bool success = false;
		string equal = "";
		
		
		//initialize variables
		intervalTimesString = "";
		tracks = 0;
		firstIntervalValue = true;
		
		bool ok;

		timerCount = 0;
		
		do {

			if(simulated) 
				ok = true;
			else
				ok = cp.Read_event(out timestamp, out platformState);
			
			if (ok) {
				if (platformState == Chronopic.Plataforma.ON && loggedState == States.OFF) {
					//has arrived
					loggedState = States.ON;
					
					//if we start out, and we arrive to the platform for the first time, don't record nothing
					if (firstIntervalValue && ! startIn) {
						firstIntervalValue = false;
					} else {
						//has arrived and not in the "running previous"
						
						//if interval run is "unlimited" not limited by tracks, nor time, 
						//then play with the progress bar until finish button is pressed
						if(limitAsDouble == -1) {
							
							if(simulated)
								timestamp = simulatedTimeLast * 1000; //conversion to milliseconds
							
							if(intervalTimesString.Length > 0) { equal = "="; }
							intervalTimesString = intervalTimesString + equal + (timestamp/1000).ToString();
							tracks ++;	
								
							eventExecuteWin.ProgressBarEventOrTimePreExecution(
									true, //isEvent
									true, //unlimited: activity mode
									tracks
									);  
						}
						else {
							//has arrived, limited
							if (tracksLimited) {
								//has arrived, limited by tracks
								tracks ++;	

								if(simulated)
									timestamp = simulatedTimeLast * 1000; //conversion to milliseconds

								if(intervalTimesString.Length > 0) { equal = "="; }
								intervalTimesString = intervalTimesString + equal + (timestamp/1000).ToString();

								if(tracks >= limitAsDouble) 
								{
									//finished
									write();
									success = true;
								}
								
								eventExecuteWin.ProgressBarEventOrTimePreExecution(
										true, //isEvent
										true, //tracksLimited: percentageMode
										tracks
										);  

							} else {
								//has arrived, limited by time
								
								if(simulated)
									timestamp = simulatedTimeLast * 1000; //conversion to milliseconds
								
								if(success)
									write();
								else {
									if(intervalTimesString.Length > 0) { equal = "="; }
									intervalTimesString = intervalTimesString + equal + (timestamp/1000).ToString();
									tracks ++;	
								}
								
								eventExecuteWin.ProgressBarEventOrTimePreExecution(
										true, //isEvent
										false, //timeLimited: activity mode
										tracks
										);  
							}
						}
					}
				}
				else if (platformState == Chronopic.Plataforma.OFF && loggedState == States.ON) {
					//it's out, was inside (= has abandoned platform)
					//don't record time
					//progressBar.Fraction = progressBar.Fraction + 0.1;
				
					//count the contact times when limited by time
					//normally these are despreciable in runs, but if
					//someone uses this for other application, we should record

					//change the automata state
					loggedState = States.OFF;

				}
			}
		} while ( ! success && ! cancel && ! finish );

		if (finish) {
			write();
		}
		if(cancel || finish) {
			//event will be raised, and managed in chronojump.cs
			fakeButtonFinished.Click();
		}
	}
	
	protected override bool shouldFinishByTime() {
		//check if it should finish now (time limited, not unlimited and time exceeded)
		if( ! tracksLimited && limitAsDouble != -1 && timerCount > limitAsDouble)
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
		//limited by jumps or time, but has no finished
		eventExecuteWin.ProgressBarEventOrTimePreExecution(
				false, //isEvent false: time
				!tracksLimited, //if tracksLimited: activity, if timeLimited: fraction
				timerCount
				); 
	}


	protected override void write()
	{
		int tracks = 0;
		string limitString = "";

		//if user clicked in finish earlier
		if(finish) {
			tracks = Util.GetNumberOfJumps(intervalTimesString, false);
			if(tracksLimited) {
				limitString = tracks.ToString() + "R";
			} else {
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
			
		uniqueID = SqliteRun.InsertInterval("NULL", personID, sessionID, type, 
				distanceTotal, timeTotal,
				distanceInterval, intervalTimesString, tracks, 
				"", 					//description
				limitString
				);

		string myStringPush =   Catalog.GetString("Last run") + ": " + RunnerName + " " + 
			type + " (" + limitString + ") " +
			Catalog.GetString("AVG Speed") + ": " + Util.TrimDecimals( 
					Util.GetSpeed(distanceTotal.ToString(),
						timeTotal.ToString() )
					, pDN ) ;
		appbar.Push( 1,myStringPush );
				
	
		//event will be raised, and managed in chronojump.cs
		fakeButtonFinished.Click();
		
		//put max value in progressBar. This makes the thread in PulseGTK() stop
		progressBar.Fraction = 1;
		
		//eventExecuteWin.EventEnded(-1, -1);
		eventExecuteWin.EventEnded();
		
	}


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
			/*} else {
				return (distanceTotal / timeTotal) * 3.6 ; 
			}
			*/
		}
	}
	
		
		
	~RunInterval() {}
}

