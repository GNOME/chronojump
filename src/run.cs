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

public class Run 
{
	protected int personID;
	protected int sessionID;
	protected int uniqueID;
	protected string type;
	protected double distance;
	protected double time;
	protected string description;

	//for not checking always in database
	protected bool startIn;
	
	protected Thread thread;
	//platform state variables
	protected enum States {
		ON,
		OFF
	}
	
	protected Chronopic cp;
	protected States loggedState;		//log of last state
	protected Gtk.ProgressBar progressBar;
	protected Gnome.AppBar appbar;
	protected Gtk.Window app;
	protected int pDN;
	protected bool metersSecondsPreferred;

	//for raise a signal and manage it on chronojump.cs
	protected Gtk.Button falseButtonFinished;
	
	//for cancelling from chronojump.cs
	protected bool cancel;
	
	
	public Run() {
	}

	//run execution
	public Run(int personID, int sessionID, string type, double distance,   
			Chronopic cp, Gtk.ProgressBar progressBar, Gnome.AppBar appbar, Gtk.Window app, 
			int pDN, bool metersSecondsPreferred)
	{
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
		
		falseButtonFinished = new Gtk.Button();
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

	public virtual void Simulate(Random rand)
	{
		time = rand.NextDouble() * 15;
		Console.WriteLine("time: {0}", time.ToString());
		write();
	}

	public virtual void Manage(object o, EventArgs args)
	{
		Chronopic.Respuesta respuesta;		//ok, error, or timeout in calling the platform
		Chronopic.Plataforma platformState;	//on (in platform), off (jumping), or unknow

		do {
			respuesta = cp.Read_platform(out platformState);
		} while (respuesta!=Chronopic.Respuesta.Ok);


		//you can start ON or OFF the platform, 
		//we record always de TV (or time between we abandonate the platform since we arrive)
		if (platformState==Chronopic.Plataforma.ON) {
			appbar.Push( Catalog.GetString("You are OUT, RUN when prepared!!") );

			loggedState = States.ON;
			startIn = false;
		} else {
			appbar.Push( Catalog.GetString("You are IN, RUN when prepared!!") );

			loggedState = States.OFF;
			startIn = true;
		}

		//reset progressBar
		progressBar.Fraction = 0;

		//prepare jump for being cancelled if desired
		cancel = false;

		//start thread
		thread = new Thread(new ThreadStart(waitRun));
		GLib.Idle.Add (new GLib.IdleHandler (Pulse));
		thread.Start(); 
	}
	
	protected virtual void waitRun ()
	{
		double timestamp;
		bool success = false;
		
		Chronopic.Respuesta respuesta;		//ok, error, or timeout in calling the platform
		Chronopic.Plataforma platformState;	//on (in platform), off (jumping), or unknow
	
		//we allow start from the platform or outside
		bool arrived = false; 
		
		do {
			respuesta = cp.Read_event(out timestamp, out platformState);
			if (respuesta == Chronopic.Respuesta.Ok) {
				if (platformState == Chronopic.Plataforma.ON && loggedState == States.OFF) {
					//has arrived
					loggedState = States.ON;
					
					if( ! startIn && ! arrived ) {
						//run started out (behind the platform) and it's the first arrive
						this.progressBar.Fraction = 0.20;
						arrived = true;
					} else {
						//run finished: 
						//if started outside (behind platform) it's the second arrive
						//if started inside: it's the first arrive
						
						time = timestamp / 1000;
						write ();

						success = true;
					}
				}
				else if (platformState == Chronopic.Plataforma.OFF && loggedState == States.ON) {
					//it's out, was inside (= has abandoned platform)
					//don't record time
						progressBar.Fraction = 0.5;

					//change the automata state
					loggedState = States.OFF;

				}
			}
		} while ( ! success && ! cancel );

		if(cancel) {
			//event will be raised, and managed in chronojump.cs
			falseButtonFinished.Click();
		}
	}
	
	protected bool Pulse ()
	{
		//if (thread.IsAlive) {
			if(progressBar.Fraction == 1 || cancel) {
				Console.Write("dying");

				//event will be raised, and managed in chronojump.cs
				//falseButtonFinished.Click();
				//Now called on write(), now work in mono1.1.6
				
				return false;
			}
			Thread.Sleep (150);
			Console.Write(thread.ThreadState);
			return true;
		//}
		//return false;
	}

	protected virtual void write()
	{
		Console.WriteLine("TIME: {0}", time.ToString());
		
		string myStringPush =   Catalog.GetString("Last run: ") + RunnerName + " " + 
			type + " time:" + Util.TrimDecimals( time.ToString(), pDN ) + 
			" speed:" + Util.TrimDecimals ( (distance/time).ToString(), pDN );
		appbar.Push( myStringPush );

		uniqueID = SqliteRun.Insert(personID, sessionID, 
				type, distance, time, ""); //type, distance, time, description
		
		//event will be raised, and managed in chronojump.cs
		falseButtonFinished.Click();
		
		//put max value in progressBar. This makes the thread in Pulse() stop
		progressBar.Fraction = 1;
	}
	

	public Gtk.Button FalseButtonFinished
	{
		get {
			return	falseButtonFinished;
		}
	}

	//called from chronojump.cs for cancelling jumps
	public bool Cancel
	{
		get {
			return cancel;
		}
		set {
			cancel = value;
		}
	}
	
	
	public string Type
	{
		get { return type; }
		set { type = value; }
	}
	
	public double Speed
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
	int tracks;


	public RunInterval(int uniqueID, int personID, int sessionID, string type, double distanceTotal, double timeTotal, double distanceInterval, string intervalTimesString, int tracks, string description)
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
	}

	public string IntervalTimesString
	{
		get {
			return intervalTimesString;
		}
	}
		
		
	~RunInterval() {}
}

