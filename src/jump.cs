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
using System.Text; //StringBuilder

using System.Threading;
using System.IO.Ports;
using Mono.Unix;

public class Jump : Event 
{
	protected double tv;
	protected double tc;
	protected int fall;
	protected double weight; //always write in % (not kg or %) then sqlite can do avgs

	//for not checking always in database
	protected bool hasFall;

	//better as private and don't inherit, don't know why
	//protected Chronopic cp;
	private Chronopic cp;
	
	public Jump() {
	}

	//jump execution
	public Jump(EventExecuteWindow eventExecuteWin, int personID, string personName, int sessionID, string type, int fall, double weight,  
			Chronopic cp, Gtk.ProgressBar progressBar, Gtk.Statusbar appbar, Gtk.Window app, 
			int pDN)
	{
		this.eventExecuteWin = eventExecuteWin;
		this.personID = personID;
		this.personName = personName;
		this.sessionID = sessionID;
		this.type = type;
		this.fall = fall;
		this.weight = weight;
		
		this.cp = cp;
		this.progressBar = progressBar;
		this.appbar = appbar;
		this.app = app;

		this.pDN = pDN;
	
		if(TypeHasFall) {
			hasFall = true;
		} else {
			hasFall = false;
		}
		
		fakeButtonFinished = new Gtk.Button();
		
		simulated = false;
	}
	
	//after inserting database (SQL)
	public Jump(int uniqueID, int personID, int sessionID, string type, double tv, double tc, int fall, double weight, string description)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.personName = SqlitePerson.SelectJumperName(personID);
		this.sessionID = sessionID;
		this.type = type;
		this.tv = tv;
		this.tc = tc;
		this.fall = fall;
		this.weight = weight;
		this.description = description;
	}
		
	public override void SimulateInitValues(Random randSent)
	{
		Console.WriteLine("From jump.cs");

		rand = randSent; //we send the random, because if we create here, the values will be the same for each nbew instance
		simulated = true;
		simulatedTimeAccumulatedBefore = 0;
		simulatedTimeLast = 0;
		simulatedContactTimeMin = 0.2; //seconds
		simulatedContactTimeMax = 0.3; //seconds
		simulatedFlightTimeMin = 0.3; //seconds
		simulatedFlightTimeMax = 0.6; //seconds

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
		if (simulated) 
			platformState = Chronopic.Plataforma.ON;
		else
			platformState = chronopicInitialValue(cp);
		
		
		if (platformState==Chronopic.Plataforma.ON) {
			appbar.Push( 1,Catalog.GetString("You are IN, JUMP when prepared!!") );

			loggedState = States.ON;

			//reset progressBar
			progressBar.Fraction = 0;
			progressBar.Text = "";

			//prepare jump for being cancelled if desired
			cancel = false;

			//in simulated mode, make the jump start just when we arrive to waitEvent at the first time
			//mark now that we have leaved platform:
			if (simulated)
				platformState = Chronopic.Plataforma.OFF;
			
			//start thread
			//Console.Write("Start thread");
			thread = new Thread(new ThreadStart(waitEvent));
			GLib.Idle.Add (new GLib.IdleHandler (PulseGTK));
			thread.Start(); 
		} 
		else {
			ConfirmWindow confirmWin;		
			confirmWin = ConfirmWindow.Show(app, 
					Catalog.GetString("You are OUT, come inside and press the 'accept' button"), "");

			//we call again this function
			confirmWin.Button_accept.Clicked += new EventHandler(callAgainManage);
			
			//if confirmWin.Button_cancel is pressed retuen
			confirmWin.Button_cancel.Clicked += new EventHandler(cancel_event);
		}
	}

	public void ManageFall()
	{
		if (simulated) 
			platformState = Chronopic.Plataforma.OFF;
		else
			platformState = chronopicInitialValue(cp);

		
		if (platformState==Chronopic.Plataforma.OFF) {
			appbar.Push( 1,Catalog.GetString("You are OUT, JUMP when prepared!!") );

			loggedState = States.OFF;

			//useful also for tracking the jump phases
			tc = 0;

			//reset progressBar
			progressBar.Fraction = 0;
			progressBar.Text = "";

			//prepare jump for being cancelled if desired
			cancel = false;

			//in simulated mode, make the jump start just when we arrive to waitEvent at the first time
			//mark now that we have arrived:
			if (simulated)
				platformState = Chronopic.Plataforma.ON;
			
			//start thread
			thread = new Thread(new ThreadStart(waitEvent));
			GLib.Idle.Add (new GLib.IdleHandler (PulseGTK));
			thread.Start(); 
		} 
		else {
			ConfirmWindow confirmWin;		
			confirmWin = ConfirmWindow.Show(app, 
					Catalog.GetString("You are IN, please leave the platform, and press the 'accept' button"), "");

			//we call again this function
			confirmWin.Button_accept.Clicked += new EventHandler(callAgainManageFall);
			
			//if confirmWin.Button_cancel is pressed retuen
			confirmWin.Button_cancel.Clicked += new EventHandler(cancel_event);
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
			
			if (ok) {
				if (platformState == Chronopic.Plataforma.ON && loggedState == States.OFF) 
				{
					//has landed
				
					if(hasFall && tc == 0) {
						//jump with fall, landed first time
						initializeTimer();

						//this.progressBar.Fraction = 0.33;
						eventExecuteWin.ProgressbarEventOrTimePreExecution(
								true, //isEvent
								true, //jumpsLimited: percentageMode
								1 //it's a drop: phase 1/3
								);  
					} else {
						//jump with fall: second landed; or without fall first landing
					
						if(simulated)
							timestamp = simulatedTimeLast * 1000; //conversion to milliseconds
						
						Console.Write("t1:{0}", timestamp);

						tv = timestamp / 1000;
						write ();

						success = true;
						
						//update event progressbar
						double percentageToPass = 2; //normal jump has two phases
						if(hasFall)
							percentageToPass = 3; //drop jump has three phases
							
						eventExecuteWin.ProgressbarEventOrTimePreExecution(
								true, //isEvent
								true, //percentageMode
								percentageToPass
								);  
					}
					
					loggedState = States.ON;
				}
				else if (platformState == Chronopic.Plataforma.OFF && loggedState == States.ON) 
				{
			
					//it's out, was inside (= has jumped)
					
					if(hasFall) {
						
						if(simulated)
							timestamp = simulatedTimeLast * 1000; //conversion to milliseconds
						
						Console.Write("t2:{0}", timestamp);
						
						//record the TC
						tc = timestamp / 1000;
						
						//update event progressbar
						eventExecuteWin.ProgressbarEventOrTimePreExecution(
								true, //isEvent
								true, //percentageMode
								2 //it's a drop jump: phase 2/3
								);  
					} else {
						initializeTimer();
						
						//update event progressbar
						eventExecuteWin.ProgressbarEventOrTimePreExecution(
								true, //isEvent
								true, //percentageMode
								1 //normal jump, phase 1/2
								);  
					}

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
		return false; //this kind of events (simple or Dj jumps) cannot be finished by time
	}
	
	protected override void updateTimeProgressbar() {
		//has no finished, but move progressbar time
		eventExecuteWin.ProgressbarEventOrTimePreExecution(
				false, //isEvent false: time
				false, //activity mode
				-1	//don't want to show info on label
				); 
	}

	protected override void write()
	{
		string tcString = "";
		if(hasFall) {
			//Console.WriteLine("TC: {0}", tc.ToString());
			tcString = " " + Catalog.GetString("TC") + ": " + Util.TrimDecimals( tc.ToString(), pDN ) ;
		} else {
			tc = 0;
		}
			
		//Console.WriteLine("TF: {0}", tv.ToString());
		//progressBar.Text = "tf: " + Util.TrimDecimals( tv.ToString(), pDN);
		
		string myStringPush =   
			//Catalog.GetString("Last jump: ") + 
			personName + " " + 
			type + tcString + " " + Catalog.GetString("TF") + ": " + Util.TrimDecimals( tv.ToString(), pDN ) ;
		if(weight > 0) {
			myStringPush = myStringPush + "(" + weight.ToString() + "%)";
		}
		appbar.Push( 1,myStringPush );

		uniqueID = SqliteJump.Insert(personID, sessionID, 
				type, tv, tc, fall,  //type, tv, tc, fall
				weight, "", ""); //weight, limited, description
		
		//event will be raised, and managed in chronojump.cs
		fakeButtonFinished.Click();
		
		//put max value in progressBar. This makes the thread in PulseGTK() stop
		progressBar.Fraction = 1;
		
		eventExecuteWin.EventEnded(tv, tc);
	}
	
	public bool TypeHasWeight
	{
		get { return SqliteJumpType.HasWeight(type); }
	}
	
	public virtual bool TypeHasFall
	{
		get { return SqliteJumpType.HasFall("jumpType", type); } //jumpType is the table name
	}
	
	public double Tv
	{
		get { return tv; }
		set { tv = value; }
	}
	
	public double Tc
	{
		get { return tc; }
		set { tc = value; }
	}
	
	public int Fall
	{
		get { return fall; }
		set { fall = value; }
	}
	
	public double Weight
	{
		get { return weight; }
		set { weight = value; }
	}

	
	/*
	public string JumperName
	{
		get { return SqlitePerson.SelectJumperName(personID); }
	}
	*/

	~Jump() {}
	   
}

public class JumpRj : Jump
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
	
	
	//jump execution
	public JumpRj(EventExecuteWindow eventExecuteWin, int personID, string personName, 
			int sessionID, string type, int fall, double weight, 
			double limitAsDouble, bool jumpsLimited, 
			Chronopic cp, Gtk.ProgressBar progressBar, Gtk.Statusbar appbar, Gtk.Window app, 
			int pDN)
	{
		this.eventExecuteWin = eventExecuteWin;
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
			this.limited = limitAsDouble.ToString() + "T";
		}
		
		this.cp = cp;
		this.progressBar = progressBar;
		this.appbar = appbar;
		this.app = app;

		this.pDN = pDN;
	
		//progressBar is used here only for put it to 1 when we want to stop the pulseGTK() (the thread)
		progressBar.Fraction = 0;
		
		if(TypeHasFall) { hasFall = true; } 
		else { hasFall = false; }
		
		fakeButtonFinished = new Gtk.Button();
	}
	
	public JumpRj() {
	}

	//after inserting database (SQL)
	public JumpRj(int uniqueID, int personID, int sessionID, string type, 
			string tvString, string tcString, int fall, double weight, 
			string description, int jumps, double time, string limited)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.personName = SqlitePerson.SelectJumperName(personID);
		this.sessionID = sessionID;
		this.type = type;
		this.tvString = tvString;
		this.tcString = tcString;
		this.fall = fall;
		this.weight = weight;
		this.description = description;
		this.jumps = jumps;
		this.time = time;
		this.limited = limited;
	}

	public override void Manage()
	{
		if (simulated)
			if(hasFall) 
				platformState = Chronopic.Plataforma.OFF;
			else 
				platformState = Chronopic.Plataforma.ON;
		else
			platformState = chronopicInitialValue(cp);

		if(platformState == Chronopic.Plataforma.OFF)
			loggedState = States.OFF;
		else
			loggedState = States.ON;

		
		bool success = false;

		if (platformState==Chronopic.Plataforma.OFF && hasFall ) {
			appbar.Push( 1,Catalog.GetString("You are OUT, JUMP when prepared!!") );
			success = true;
		} else if (platformState==Chronopic.Plataforma.ON && ! hasFall ) {
			appbar.Push( 1,Catalog.GetString("You are IN, JUMP when prepared!!") );
			success = true;
		} else {
			string myMessage = Catalog.GetString("You are IN, please leave the platform, and press the 'accept' button");
			if (platformState==Chronopic.Plataforma.OFF ) {
				myMessage = Catalog.GetString("You are OUT, please enter the platform, prepare for jump and press the 'accept' button");
			}
			ConfirmWindow confirmWin;		
			confirmWin = ConfirmWindow.Show(app, myMessage, "");

			//we call again this function
			confirmWin.Button_accept.Clicked += new EventHandler(callAgainManage);
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
			
			//prepare jump for being finished earlier if desired
			finish = false;

			//in simulated mode, make the jump start just when we arrive to waitEvent at the first time
			//mark now that the opposite as before:
			if (simulated)
				if(hasFall)
					platformState = Chronopic.Plataforma.ON;
				else 
					platformState = Chronopic.Plataforma.OFF;
			
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
		
		bool ok;
	
		do {
			if(simulated) 
				ok = true;
			else
				ok = cp.Read_event(out timestamp, out platformState);
			
			
			//if chronopic signal is Ok and state has changed
			if (ok && (
					(platformState == Chronopic.Plataforma.ON && loggedState == States.OFF) ||
					(platformState == Chronopic.Plataforma.OFF && loggedState == States.ON) ) ) {
				
			
				if(simulated)
					timestamp = simulatedTimeLast * 1000; //conversion to milliseconds

				Console.Write(Util.GetTotalTime(tcString, tvString));

				string equal = "";
			
				//while no finished time or jumps, continue recording events
				if ( ! success) {
					//don't record the time until the first event
					if (firstRjValue) {
						firstRjValue = false;

						//but start timer
						initializeTimer();
					} else {
						//reactive jump has not finished... record the next jump
						Console.WriteLine("tcCount: {0}, tvCount: {1}", tcCount, tvCount);
						if ( tcCount == tvCount )
						{
							lastTc = timestamp/1000;
							
//							jumpRjExecuteWin.ProgressbarTcCurrent = lastTc; 
							
							if(tcCount > 0) { equal = "="; }
							tcString = tcString + equal + lastTc.ToString();
							
//							jumpRjExecuteWin.ProgressbarTcAvg = Util.GetAverage(tcString); 
							
							tcCount = tcCount + 1;
						} else {
							//tcCount > tvCount 
							lastTv = timestamp/1000;
							
//							jumpRjExecuteWin.ProgressbarTvCurrent = lastTv; 
							//show the tv/tc except if it's the first jump starting inside
//							if(tc != -1)
//								jumpRjExecuteWin.ProgressbarTvTcCurrent = lastTv / lastTc; 
							
							if(tvCount > 0) { equal = "="; }
							tvString = tvString + equal + lastTv.ToString();
							
//							jumpRjExecuteWin.ProgressbarTvAvg = Util.GetAverage(tvString); 
							//show the tv/tc except if it's the first jump starting inside
//							if(tc != -1)
//								jumpRjExecuteWin.ProgressbarTvTcAvg = 
//									Util.GetAverage(tvString) / Util.GetAverage(tcString); 
				
							tvCount = tvCount + 1;
							
							//update event progressbar
							//jumpRjExecuteWin.ProgressbarEventOrTimePreExecution(
							eventExecuteWin.ProgressbarEventOrTimePreExecution(
									true, //isEvent
									jumpsLimited, //if jumpsLimited: do fraction; if time limited: do pulse
									tvCount
									);  
						}
					}
				}

				//check if reactive jump should finish
				if (jumpsLimited) {
					if(limitAsDouble != -1) {
						if(Util.GetNumberOfJumps(tvString, false) >= limitAsDouble)
						{
							write();
							success = true;
							
							//update event progressbar
							eventExecuteWin.ProgressbarEventOrTimePreExecution(
									true, //isEvent
									true, //percentageMode
									tvCount
									);  
						}
					}
				} else {
					//limited by time, if passed it, write
					if(success) {
						write();
					}
				}

				if(platformState == Chronopic.Plataforma.OFF)
					loggedState = States.OFF;
				else
					loggedState = States.ON;

			}
		} while ( ! success && ! cancel && ! finish );
	
		
		if (finish) {
			if(Util.GetNumberOfJumps(tcString, false) >= 1 && Util.GetNumberOfJumps(tvString, false) >= 1) {
				write();
			} else {
				//cancel a jump if clicked finish before any events done
				cancel = true;
			}
		}
		if(cancel || finish) {
			//event will be raised, and managed in chronojump.cs
			fakeButtonFinished.Click();
		}
	}

	protected override bool shouldFinishByTime() {
		//check if it should finish now (time limited, not unlimited and time exceeded)
		if( !jumpsLimited && limitAsDouble != -1 && timerCount > limitAsDouble)
			return true;
		else
			return false;
	}
	
	protected override void updateProgressbarForFinish() {
		eventExecuteWin.ProgressbarEventOrTimePreExecution(
				false, //isEvent false: time
				true, //percentageMode: it has finished, show bar at 100%
				limitAsDouble
				);  
	}

	protected override void updateTimeProgressbar() {
		//limited by jumps or time, but has no finished
		eventExecuteWin.ProgressbarEventOrTimePreExecution(
				false, //isEvent false: time
				!jumpsLimited, //if jumpsLimited: activity, if timeLimited: fraction
				timerCount
				); 
	}

				
	protected override void write()
	{
		int jumps;
		string limitString = "";

		//if user clicked in finish earlier
		if(finish) {
			jumps = Util.GetNumberOfJumps(tvString, false);

			//if user clicked finish and last event was tc, probably there are more TCs than TFs
			//if last event was tc, it has no sense, it should be deleted
			tcString = Util.DeleteLastTcIfNeeded(tcString, tvString);
					
			if(jumpsLimited) {
				limitString = jumps.ToString() + "J";
			} else {
				limitString = Util.GetTotalTime(tcString, tvString) + "T";
			}
		} else {
			if(jumpsLimited) {
				limitString = limitAsDouble.ToString() + "J";
				jumps = (int) limitAsDouble;
			} else {
				//if time finished and the last event was tc, probably there are more TCs than TFs
				//if last event was tc, it has no sense, it should be deleted
				tcString = Util.DeleteLastTcIfNeeded(tcString, tvString);
				
				limitString = limitAsDouble.ToString() + "T";
				string [] myStringFull = tcString.Split(new char[] {'='});
				jumps = myStringFull.Length;
			}
		}

		uniqueID = SqliteJump.InsertRj("NULL", personID, sessionID, 
				type, Util.GetMax(tvString), Util.GetMax(tcString), 
				fall, weight, "", //fall, weight, description
				Util.GetAverage(tvString), Util.GetAverage(tcString),
				tvString, tcString,
				jumps, Util.GetTotalTime(tcString, tvString), limitString
				);

		string myStringPush =   
			//Catalog.GetString("Last jump: ") + 
			personName + " " + 
			type + " (" + limitString + ") " +
			" " + Catalog.GetString("AVG TF") + ": " + Util.TrimDecimals( Util.GetAverage (tvString).ToString(), pDN ) +
			" " + Catalog.GetString("AVG TC") + ": " + Util.TrimDecimals( Util.GetAverage (tcString).ToString(), pDN ) ;
		appbar.Push( 1,myStringPush );
	
		//event will be raised, and managed in chronojump.cs
		fakeButtonFinished.Click();
		
		//put max value in progressBar. This makes the thread in PulseGTK() stop
		progressBar.Fraction = 1;
		
		
		eventExecuteWin.EventEnded(-1, -1);
	}


	public string Limited
	{
		get { return limited; }
		set { limited = value; }
	}
	
	public override bool TypeHasFall
	{
		get { return SqliteJumpType.HasFall("jumpRjType", type); } //jumpRjType is the table name
	}
	
	public double TvMax
	{
		get { return Util.GetMax (tvString); }
	}
		
	public double TcMax
	{
		get { return Util.GetMax (tcString); }
	}
		
	public double TvAvg
	{
		get { return Util.GetAverage (tvString); }
	}
		
	public double TcAvg
	{
		get { return Util.GetAverage (tcString); }
	}
	
	public string TvString
	{
		get { return tvString; }
		set { tvString = value; }
	}
		
	public string TcString
	{
		get { return tcString; }
		set { tcString = value; }
	}

	public int Jumps
	{
		get { return jumps; }
		set { jumps = value; }
	}
	
	public bool JumpsLimited
	{
		get { return jumpsLimited; }
	}
		
		
	~JumpRj() {}
}

