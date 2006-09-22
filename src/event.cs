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
using System.Timers;


public class Event 
{
	protected int personID;
	protected string personName;
	protected int sessionID;
	protected int uniqueID;
	protected string type;
	protected string description;

	//for finishing earlier from chronojump.cs
	protected bool finish;
	
	protected Thread thread;
	//platform state variables
	protected enum States {
		ON,
		OFF
	}
	
	//better as private and don't inherit, don't know why
	//protected Chronopic cp;
	//private Chronopic cp;
	
	//a timer for controlling the time between events and update the progressbar
	//timer has a delegate that 10/s updates the time progressBar. 
	//It starts when the first event is detected
	protected System.Timers.Timer timerClock = new System.Timers.Timer();    
	protected double timerCount; // 10 times x second

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
	protected Gtk.ProgressBar progressBar;
	protected Gtk.Statusbar appbar;
	protected Gtk.Window app;
	protected int pDN;

	//for raise a signal and manage it on chronojump.cs
	protected Gtk.Button fakeButtonFinished;
	
	//for cancelling from chronojump.cs
	protected bool cancel;

	
	public Event() {
		simulated = false;
	}
	
	//public virtual void Simulate(Random randSent)
	public virtual void SimulateInitValues(Random randSent)
	{
		Console.WriteLine("From event.cs");

		//look at the override on jump.cs for a sample
		
		rand = randSent; //we send the random, because if we create here, the values will be the same for each nbew instance
		simulated = true;
		simulatedTimeAccumulatedBefore = 0;
		simulatedTimeLast = 0;
		//simulatedContactTimeMin = 0.2; //seconds
		//simulatedContactTimeMax = 0.3; //seconds
		//simulatedFlightTimeMin = 0.3; //seconds
		//simulatedFlightTimeMax = 0.6; //seconds
		simulatedContactTimeMin = 0; //seconds
		simulatedContactTimeMax = 0; //seconds
		simulatedFlightTimeMin = 0; //seconds
		simulatedFlightTimeMax = 0; //seconds
		simulatedCurrentTimeIntervalsAreContact = false;

		//Manage();
	}

	protected virtual Chronopic.Plataforma chronopicInitialValue(Chronopic cp)
	{
		Chronopic.Plataforma myPlatformState  = Chronopic.Plataforma.UNKNOW; //on (in platform), off (jumping), or unknow
		bool ok = false;
		Console.WriteLine("A1");

		do {
			Console.WriteLine("B");
			try {
				ok = cp.Read_platform(out myPlatformState);
			} catch {
				Console.WriteLine("Manage called after finishing constructor, do later");
			}
			Console.WriteLine("C");
		} while (! ok);

		return myPlatformState;
	}
	
	//public virtual void Manage(object o, EventArgs args)
	public virtual void Manage()
	{
	}
	
	//for calling it again after a confirmWindow says that you have to be in or out the platform
	//and press ok button
	//This method is for not having problems with the parameters of the delegate
	protected void callAgainManage(object o, EventArgs args) {
		Manage();
	}
	

	protected virtual void waitEvent () {
	}
	
	protected bool PulseGTK ()
	{
		//if (thread.IsAlive) {
			if(progressBar.Fraction == 1 || cancel) {
				Console.Write("dying");

				//event will be raised, and managed in chronojump.cs
				//fakeButtonFinished.Click();
				//Now called on write(), now work in mono1.1.6
				
				return false;
			}
			Thread.Sleep (150);
			Console.Write(thread.ThreadState);
			return true;
		//}
		//return false;
	}

	protected void initializeTimer () {
		//start onTimer for moving the time progressBar (activiy mode)	
		//also in simulated, allows to change platform state
		//this should be done only one time
		timerCount = 0;
		timerClock.Elapsed += new ElapsedEventHandler(onTimer);
		timerClock.Interval = 100; //10 times x second
		timerClock.Enabled = true;
	}
		
	//onTimer allow to update progressbar_time 10/s
	//also can change platform state in simulated mode
	protected void onTimer( Object source, ElapsedEventArgs e )
	{
		timerCount = timerCount + .1; //10 times x second
		
		if(simulated) {
			eventSimulatedShouldChangePlatform();
		}

		//check if it should finish by time
		if(shouldFinishByTime()) {
			updateProgressbarForFinish();
			finish = true;
		} 
		else 
			updateTimeProgressbar();
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
				Console.WriteLine("EXCEEDES MAX!");
				simulateChangePlatform();
		}
		
		//some random for finishing between timeMin and timeMax
		if(timerCount - simulatedTimeAccumulatedBefore >= timeMin)
		{
			double simulatedRange = timeMax - timeMin;
			//rand.NextDouble gives a value between 0 and 1
			//if we multiply by the (simulatedRange * 10 +1), then we will have 4 options if the range is ,4
			//check if the value is less than 1 (it's one change in four options) and if it's 1, then simulated the change platform
			//double dice = 0;
			double myRand = rand.NextDouble();
			double dice = myRand * (simulatedRange *10 +1);
			Console.WriteLine("rand: {0}, dice: {1}", myRand, dice);
			if (dice < 1)
			{
				simulateChangePlatform();
			}
		}
	}

	protected virtual void simulateChangePlatform() {
		Console.Write("Changing!");
		Console.WriteLine("timeLast: {0}, timerCount: {1}, timeAccumulated: {2}", simulatedTimeLast, timerCount, simulatedTimeAccumulatedBefore);

		simulatedTimeLast = timerCount - simulatedTimeAccumulatedBefore;
		simulatedTimeAccumulatedBefore = timerCount;

		//change the boolean who points to 'which are the MINs and the MAXs
		simulatedCurrentTimeIntervalsAreContact = ! simulatedCurrentTimeIntervalsAreContact;
		
		//wait 1/100 sec to change the plataformaState values, 
		//if not, sometimes, it waitEvent tries to manage data, when the simulatedTimeLast variable (just up) has not been asssigned yet
		//Thread.Sleep (10); 

		if(platformState == Chronopic.Plataforma.ON)
			platformState = Chronopic.Plataforma.OFF;
		else
			platformState = Chronopic.Plataforma.ON;

		Console.WriteLine("Changed!");
	}
			
	protected virtual bool shouldFinishByTime() {
		return true;
	}
	
	protected virtual void updateProgressbarForFinish() {
	}
	
	protected virtual void updateTimeProgressbar() {
	}
	
	protected virtual void write() {
	}
	
	//from confirm_window cancel button (thread has not started)
	//this is NOT called when a event has started and user click on "Cancel"
	protected void cancel_event(object o, EventArgs args)
	{
		//event will be raised, and managed in chronojump.cs
		fakeButtonFinished.Click();
		
		cancel = true;
	}
	
	public Gtk.Button FakeButtonFinished
	{
		get {
			return	fakeButtonFinished;
		}
	}

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
	
	public string Type
	{
		get { return type; }
		set { type = value; }
	}
	
	public string Description
	{
		get { return description; }
		set { description = value; }
	}
	
	public int UniqueID
	{
		get { return uniqueID; }
		set { uniqueID = value; }
	}

	public int SessionID
	{
		get { return sessionID; }
	}

	public int PersonID
	{
		get { return personID; }
	}
		
	public string PersonName
	{
		get { return personName; }
	}
	
	~Event() {}
	   
}
