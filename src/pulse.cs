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

public class Pulse : Event
{
	double fixedPulse;
	int totalPulsesNum;

	string timesString;
	int tracks;
	double contactTime;
	bool firstPulse;

	//better as private and don't inherit, don't know why
	//protected Chronopic cp;
	private Chronopic cp;

	
	//used on treeviewPulse
	public Pulse() {
	}
	
	//execution
	public Pulse(EventExecuteWindow eventExecuteWin, int personID, string personName, int sessionID, string type, double fixedPulse, int totalPulsesNum,  
			Chronopic cp, Gtk.ProgressBar progressBar, Gtk.Statusbar appbar, Gtk.Window app, 
			int pDN)
	{
		this.eventExecuteWin = eventExecuteWin;
		this.personID = personID;
		this.personName = personName;
		this.sessionID = sessionID;
		this.type = type;
		this.fixedPulse = fixedPulse;
		this.totalPulsesNum = totalPulsesNum;
		
	
		this.cp = cp;
		this.progressBar = progressBar;
		this.appbar = appbar;
		this.app = app;

		this.pDN = pDN;
	
		fakeButtonFinished = new Gtk.Button();

		simulated = false;
	}
	
	
	//after inserting database (SQL)
	public Pulse(int uniqueID, int personID, int sessionID, string type, double fixedPulse, 
			int totalPulsesNum, string timesString, string description)
	{
		this.uniqueID = uniqueID;
		this.personName = SqlitePerson.SelectJumperName(personID);
		this.personID = personID;
		this.sessionID = sessionID;
		this.type = type;
		this.fixedPulse = fixedPulse;
		this.totalPulsesNum = totalPulsesNum;
		this.timesString = timesString;
		this.description = description;
	}


	public override void SimulateInitValues(Random randSent)
	{
		Console.WriteLine("From pulse.cs");

		rand = randSent; //we send the random, because if we create here, the values will be the same for each nbew instance
		simulated = true;
		simulatedTimeAccumulatedBefore = 0;
		simulatedTimeLast = 0;
		simulatedContactTimeMin = 0; //seconds
		simulatedContactTimeMax = .2; //seconds ('0' gives problems)
		simulatedFlightTimeMin = 0.8; //seconds
		simulatedFlightTimeMax = 1.3; //seconds

		//values of simulation will be the contactTime at the first time 
		//(next flight, contact, next flight...)
		//tc+tv will be registered
		simulatedCurrentTimeIntervalsAreContact = true;
	}
	
	
	public override void Manage()
	{
		bool success = false;
		
		
		if (simulated) 
			platformState = Chronopic.Plataforma.OFF;
		else
			platformState = chronopicInitialValue(cp);
		

		//you should start OFF (outside) the platform 
		//we record always de TC+TF (or time between we pulse platform and we pulse again)
		//we don't care about the time between the get in and the get out the platform
		if (platformState==Chronopic.Plataforma.ON) {
			string myMessage = Catalog.GetString("You are IN, please leave the platform, prepare for start, and press the 'accept' button!!");

			ConfirmWindow confirmWin;		
			confirmWin = ConfirmWindow.Show(app, myMessage, "");

			//we call again this function
			confirmWin.Button_accept.Clicked += new EventHandler(callAgainManage);
		} else {
			appbar.Push( 1, Catalog.GetString("You are OUT, start when prepared!!") );

			loggedState = States.OFF;

			success = true;
		}

		if(success) {
			//initialize variables
			timesString = "";
			tracks = 0;
			firstPulse = true;

			//reset progressBar
			progressBar.Fraction = 0;
			progressBar.Text = "";

			//prepare jump for being cancelled if desired
			cancel = false;

			//prepare jump for being finished earlier if desired
			finish = false;

			//in simulated mode, make the event start just when we arrive to waitEvent at the first time
			//mark now that we have landed:
			if (simulated)
				platformState = Chronopic.Plataforma.ON;
			
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
			string equal = "";

			bool ok;

			do {
					if(simulated) 
							ok = true;
					else 
							ok = cp.Read_event(out timestamp, out platformState);


					if (ok) {
							if (platformState == Chronopic.Plataforma.ON && loggedState == States.OFF) {
									//has arrived

									//if we arrive to the platform for the first time, don't record anything
									if (firstPulse) {
											firstPulse = false;
											initializeTimer();
									} else {
											//is not the first pulse
											if(totalPulsesNum == -1) {
													//if is "unlimited", 
													//then play with the progress bar until finish button is pressed
													if(simulated)
															timestamp = simulatedTimeLast * 1000; //conversion to milliseconds
													if(timesString.Length > 0) { equal = "="; }
													timesString = timesString + equal + (contactTime/1000 + timestamp/1000).ToString();
													tracks ++;	

													//update event progressbar
													eventExecuteWin.ProgressbarEventOrTimePreExecution(
																	true, //isEvent
																	false, //activityMode
																	tracks
																	);  
											}
											else {
													//is not the first pulse, and it's limited by tracks (ticks)
													tracks ++;	

													if(simulated)
															timestamp = simulatedTimeLast * 1000; //conversion to milliseconds
													if(timesString.Length > 0) { equal = "="; }
													timesString = timesString + equal + (contactTime/1000 + timestamp/1000).ToString();

													if(tracks >= totalPulsesNum) 
													{
															//finished
															write();
															success = true;
													}

													//update event progressbar
													eventExecuteWin.ProgressbarEventOrTimePreExecution(
																	true, //isEvent
																	true, //PercentageMode
																	tracks
																	);  
											}
									}

									//change the automata state
									loggedState = States.ON;
							}
							else if (platformState == Chronopic.Plataforma.OFF && loggedState == States.ON) {
									//it's out, was inside (= has abandoned platform)
									//don't record time
									if(simulated)
											timestamp = simulatedTimeLast * 1000; //conversion to milliseconds
									contactTime = timestamp;

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

	//now pulses are not thought for being able to finish by time
	protected override bool shouldFinishByTime() {
			return false;
	}
	
	protected override void updateProgressbarForFinish() {
		eventExecuteWin.ProgressbarEventOrTimePreExecution(
				false, //isEvent false: time
				true, //percentageMode: it has finished, show bar at 100%
				totalPulsesNum
				);  
	}

	protected override void updateTimeProgressbar() {
		//limited by jumps or time, but has no finished
		eventExecuteWin.ProgressbarEventOrTimePreExecution(
				false, //isEvent false: time
				false, //activiyMode
				timerCount
				); 
	}


	protected override void write()
	{
		int totalPulsesNum = 0;

		totalPulsesNum = Util.GetNumberOfJumps(timesString, false);

		uniqueID = SqlitePulse.Insert(personID, sessionID, type, 
				fixedPulse, totalPulsesNum, timesString, 
				"" 					//description
				);

		string myStringPush =   Catalog.GetString("Last pulse") + ": " + personName + " " + type ;
		appbar.Push( 1, myStringPush );
				
	
		//event will be raised, and managed in chronojump.cs
		fakeButtonFinished.Click();
		
		//put max value in progressBar. This makes the thread in PulseGTK() stop
		progressBar.Fraction = 1;
		
		eventExecuteWin.EventEnded(-1, -1);
	}

	
	//called from treeViewPulse
	public double GetErrorAverage(bool relative)
	{
		double pulseToComparate = Convert.ToDouble(Util.GetAverage(timesString));
		string [] myStringFull = timesString.Split(new char[] {'='});
		string myErrors = "";
		string separatorString = "";
		double error = 0;
		
		foreach (string myPulse in myStringFull) {
			if(relative)
				error = (Convert.ToDouble(myPulse) - pulseToComparate) *100 / pulseToComparate; 
			else
				error = Convert.ToDouble(myPulse) - pulseToComparate;

			//all the values should be positive
			if (error < 0)
				error = error * -1;
			
			myErrors += separatorString + error.ToString();
			separatorString = "=";
		}
		return Util.GetAverage(myErrors);
	}

	public string TimesString
	{
		get { return timesString; }
		set { timesString = value; }
	}
	
	public double FixedPulse
	{
		get { return fixedPulse; }
		set { fixedPulse = value; }
	}
	
		
		
	~Pulse() {}
}

