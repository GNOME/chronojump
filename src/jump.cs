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


public class Jump 
{
	protected int personID;
	protected int sessionID;
	protected int uniqueID;
	protected string type;
	protected double tv;
	protected double tc;
	protected int fall;
	protected string weight;
	protected string description;

	//for not checking always in database
	protected bool hasFall;

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

	//for raise a signal and manage it on chronojump.cs
	protected Gtk.Button falseButtonFinished;
	
	//for cancelling from chronojump.cs
	protected bool cancel;
	
	
	public Jump() {
	}

	//jump execution
	public Jump(int personID, int sessionID, string type, int fall, string weight,  
			Chronopic cp, Gtk.ProgressBar progressBar, Gnome.AppBar appbar, Gtk.Window app, 
			int pDN)
	{
		this.personID = personID;
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
		
		falseButtonFinished = new Gtk.Button();
	}
	
	//after inserting database (SQL)
	public Jump(int uniqueID, int personID, int sessionID, string type, double tv, double tc, int fall, string weight, string description)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.sessionID = sessionID;
		this.type = type;
		this.tv = tv;
		this.tc = tc;
		this.fall = fall;
		this.weight = weight;
		this.description = description;
	}

	public virtual void Simulate(Random rand)
	{
		if(hasFall) {
			tc = rand.NextDouble() * .4;
		}
		tv = rand.NextDouble() * .6;
		Console.WriteLine("TV: {0}", tv.ToString());
		write();
	}

	
	public virtual void Manage(object o, EventArgs args)
	{
		Chronopic.Respuesta respuesta;		//ok, error, or timeout in calling the platform
		Chronopic.Plataforma platformState;	//on (in platform), off (jumping), or unknow

		do {
			respuesta = cp.Read_platform(out platformState);
		} while (respuesta!=Chronopic.Respuesta.Ok);

		if (platformState==Chronopic.Plataforma.ON) {
			appbar.Push( Catalog.GetString("You are IN, JUMP when prepared!!") );

			loggedState = States.ON;

			//reset progressBar
			progressBar.Fraction = 0;

			//prepare jump for being cancelled if desired
			cancel = false;

			//start thread
			thread = new Thread(new ThreadStart(waitJump));
			GLib.Idle.Add (new GLib.IdleHandler (Pulse));
			thread.Start(); 
		} 
		else {
			ConfirmWindow confirmWin;		//for go up or down the platform, and for 
			confirmWin = ConfirmWindow.Show(app, 
					Catalog.GetString("You are OUT, come inside and press button"), "");

			//we call again this function
			confirmWin.Button_accept.Clicked += new EventHandler(Manage);
			
			//if confirmWin.Button_cancel is pressed retuen
			confirmWin.Button_cancel.Clicked += new EventHandler(cancel_jump);
		}
	}
	
	public void ManageFall(object o, EventArgs args)
	{
		Chronopic.Respuesta respuesta;		//ok, error, or timeout in calling the platform
		Chronopic.Plataforma platformState;	//on (in platform), off (jumping), or unknow

		do {
			respuesta = cp.Read_platform(out platformState);
		} while (respuesta!=Chronopic.Respuesta.Ok);

		if (platformState==Chronopic.Plataforma.OFF) {
			appbar.Push( Catalog.GetString("You are OUT, JUMP when prepared!!") );

			loggedState = States.OFF;

			//useful also for tracking the jump phases
			tc = 0;

			//reset progressBar
			progressBar.Fraction = 0;

			//prepare jump for being cancelled if desired
			cancel = false;

			//start thread
			thread = new Thread(new ThreadStart(waitJump));
			GLib.Idle.Add (new GLib.IdleHandler (Pulse));
			thread.Start(); 
		} 
		else {
			ConfirmWindow confirmWin;		//for go up or down the platform, and for 
			confirmWin = ConfirmWindow.Show(app, 
					Catalog.GetString("You are IN, please, go out and press button"), "");

			//we call again this function
			confirmWin.Button_accept.Clicked += new EventHandler(ManageFall);
			
			//if confirmWin.Button_cancel is pressed retuen
			confirmWin.Button_cancel.Clicked += new EventHandler(cancel_jump);
		}
	}
	
	
	protected virtual void waitJump ()
	{
		double timestamp;
		bool success = false;
		
		Chronopic.Respuesta respuesta;		//ok, error, or timeout in calling the platform
		Chronopic.Plataforma platformState;	//on (in platform), off (jumping), or unknow
		
		do {
			respuesta = cp.Read_event(out timestamp, out platformState);
			if (respuesta == Chronopic.Respuesta.Ok) {
				if (platformState == Chronopic.Plataforma.ON && loggedState == States.OFF) {
					//has landed
					loggedState = States.ON;
					
					if(hasFall && tc == 0) {
						//jump with fall, landed first time
						this.progressBar.Fraction = 0.33;
					} else {
						//jump with fall: second landed; or without fall first landing
						
						tv = timestamp / 1000;
						write ();

						success = true;
					}
				}
				else if (platformState == Chronopic.Plataforma.OFF && loggedState == States.ON) {
					//it's out, was inside (= has jumped)
					if(hasFall) {
						//record the TC
						tc = timestamp / 1000;
						
						progressBar.Fraction = 0.66;
					} else {
						progressBar.Fraction = 0.5;
					}

					//change the automata state
					loggedState = States.OFF;

				}
			}
		} while ( ! success && ! cancel );
	}
	
	protected bool Pulse ()
	{
		if (thread.IsAlive) {
			if(progressBar.Fraction == 1 || cancel) {
				Console.Write("dying");

				//event will be raised, and managed in chronojump.cs
				falseButtonFinished.Click();
				
				return false;
			}
			Thread.Sleep (150);
			Console.Write(thread.ThreadState);
			return true;
		}
		return false;
	}

	protected virtual void write()
	{
		string tcString = "";
		if(hasFall) {
			Console.WriteLine("TC: {0}", tc.ToString());
			tcString = " TC: " + Util.TrimDecimals( tc.ToString(), pDN ) ;
		} else {
			tc = 0;
		}
		Console.WriteLine("TV: {0}", tv.ToString());
		
		string myStringPush =   Catalog.GetString("Last jump: ") + JumperName + " " + 
			type + tcString + " TV:" + Util.TrimDecimals( tv.ToString(), pDN ) ;
		if(weight.Length > 0) {
			myStringPush = myStringPush + "(" + weight + ")";
		}
		appbar.Push( myStringPush );

		SqliteJump.Insert(personID, sessionID, 
				type, tv, tc, fall,  //type, tv, tc, fall
				weight, "", ""); //weight, limited, description
		
		//put max value in progressBar. This makes the thread in Pulse() stop
		progressBar.Fraction = 1;
	}
	
	//from confirm_window cancel button (thread has not started)
	private void cancel_jump(object o, EventArgs args)
	{
		cancel = true;
	
		//event will be raised, and managed in chronojump.cs
		falseButtonFinished.Click();
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
	
	public bool TypeHasWeight
	{
		get {
			return SqliteJumpType.HasWeight(type);
		}
	}
	
	public bool TypeHasFall
	{
		get {
			return SqliteJumpType.HasFall(type);
		}
	}
	
	public string Type
	{
		get
		{
			return type;
		}
		set
		{
			type = value;
		}
	}
	
	public double Tv
	{
		get
		{
			return tv;
		}
		set
		{
			tv = value;
		}
	}
	
	public double Tc
	{
		get
		{
			return tc;
		}
		set
		{
			tc = value;
		}
	}
	
	public int Fall
	{
		get
		{
			return fall;
		}
		set
		{
			fall = value;
		}
	}
	
	public string Weight
	{
		get
		{
			return weight;
		}
		set 
		{
			weight = value;
		}
	}
	
	public string Description
	{
		get
		{
			return description;
		}
		set 
		{
			description = value;
		}
	}
	
	public int UniqueID
	{
		get
		{
			return uniqueID;
		}
		set 
		{
			uniqueID = value;
		}
	}

	public int SessionID
	{
		get
		{
			return sessionID;
		}
	}

	public int PersonID
	{
		get
		{
			return personID;
		}
	}
		
	public string JumperName
	{
		get
		{
			return SqlitePerson.SelectJumperName(personID);
		}
	}

	~Jump() {}
	   
}

public class JumpRj : Jump
{
	string tvString;
	string tcString;
	int jumps; //total number of jumps
	double time; //time elapsed
	string limited; //the teorically values, eleven jumps: "11=J" (time recorded in "time"), 10 seconds: "10=T" (jumps recorded in jumps)
	double limitAsDouble;
	bool jumpsLimited;
	bool firstRjValue;
	private double tcCount;
	private double tvCount;

	//jump execution
	public JumpRj(int personID, int sessionID, string type, int fall, string weight, double limitAsDouble, bool jumpsLimited, 
			Chronopic cp, Gtk.ProgressBar progressBar, Gnome.AppBar appbar, Gtk.Window app, 
			int pDN)
	{
		this.personID = personID;
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
	
		if(TypeHasFall) {
			hasFall = true;
		} else {
			hasFall = false;
		}
		
		falseButtonFinished = new Gtk.Button();
	}
	
	//after inserting database (SQL)
	public JumpRj(int uniqueID, int personID, int sessionID, string type, string tvString, string tcString, int fall, string weight, string description, int jumps, double time, string limited)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
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

	public override void Simulate(Random rand)
	{
		tvString = "" ;
		tcString = "" ;
		string equalTc = "";
		string equalTv = "";
		bool nowTv = false;
		
		if( ! TypeHasFall ) {
			//is start in TV, write a "-1" in TC
			nowTv = true;
			tc = -1;
			tcString = tc.ToString();
			equalTc = "=";
		}
		for (double i=0 ; i < limitAsDouble ; i = i +.5) {
			//we insert the RJs as a TV and TC string of all jumps separated by '='
			if( nowTv ) {
				tv = rand.NextDouble() * .6;
				tvString = tvString + equalTv + tv.ToString();
				equalTv = "=";
				nowTv = false;
			} else {
				tc = rand.NextDouble() * .4;
				tcString = tcString + equalTc + tc.ToString();
				equalTc = "=";
				nowTv = true;
			}
		}

		if(nowTv) {
			//finished writing the TC, let's put a "-1" in the TV
			tv = -1;
			tvString = tvString + equalTv + tv.ToString();
		}
		write();
	}

	public override void Manage(object o, EventArgs args)
	{
		Chronopic.Respuesta respuesta;		//ok, error, or timeout in calling the platform
		Chronopic.Plataforma platformState;	//on (in platform), off (jumping), or unknow

		do {
			respuesta = cp.Read_platform(out platformState);
		} while (respuesta!=Chronopic.Respuesta.Ok);

		bool success = false;

		if (platformState==Chronopic.Plataforma.OFF && hasFall ) {
			appbar.Push( Catalog.GetString("You are OUT, JUMP when prepared!!") );
			success = true;
		} else if (platformState==Chronopic.Plataforma.ON && ! hasFall ) {
			appbar.Push( Catalog.GetString("You are IN, JUMP when prepared!!") );
			success = true;
		} else {
			string myMessage = Catalog.GetString("You are IN, please go out the platform, prepare for jump and press button");
			if (platformState==Chronopic.Plataforma.OFF ) {
				myMessage = Catalog.GetString("You are OUT, please put on the platform, prepare for jump and press button");
			}
			ConfirmWindow confirmWin;		//for go up or down the platform, and for 
			confirmWin = ConfirmWindow.Show(app, myMessage, "");

			//we call again this function
			confirmWin.Button_accept.Clicked += new EventHandler(Manage);
		}

		if(success) {
			//initialize strings of TCs and TVs
			tcString = "";
			tvString = "";
			tcCount = 0;
			tvCount = 0;
			firstRjValue = true;

			//if jump starts on TV, write a "-1" in TC
			if ( ! hasFall ) {
				double myTc = -1;
				tcString = myTc.ToString();
				tcCount = tcCount +.5;
			}

			//reset progressBar
			progressBar.Fraction = 0;

			//prepare jump for being cancelled if desired
			cancel = false;

			//start thread
			thread = new Thread(new ThreadStart(waitJump));
			GLib.Idle.Add (new GLib.IdleHandler (Pulse));
			thread.Start(); 
		}
	}

	protected override void waitJump ()
	{
		double timestamp;
		bool success = false;
		
		Chronopic.Respuesta respuesta;		//ok, error, or timeout in calling the platform
		Chronopic.Plataforma platformState;	//on (in platform), off (jumping), or unknow
	
		//limitAsDouble
		//jumpsLimited
		
		do {
			respuesta = cp.Read_event(out timestamp, out platformState);

			//update the progressBar if limit is time
			if ( ! jumpsLimited) {
				double myPb = getTotalTime (tcString, tvString) / limitAsDouble ;
				if(myPb > 1.0) { myPb = 1.0; }
				progressBar.Fraction = myPb; 
			}


			if (respuesta == Chronopic.Respuesta.Ok) {
				string equal = "";
				//check if reactive jump should finish
				if (jumpsLimited) {
					//change the progressBar percent
					progressBar.Fraction = (tcCount + tvCount) / limitAsDouble ;

					if(getNumberOfJumps(tcString) >= limitAsDouble && getNumberOfJumps(tvString) >= limitAsDouble)
					{
						//finished writing the TC, let's put a "-1" in the TV
						if (tcCount > tvCount) {
							if(tvCount > 0) { equal = "="; }
							tvString = tvString + equal + "-1";
						}

						write();

						success = true;
					}
				} else {
					//limited by time
					if (getTotalTime (tcString, tvString) >= limitAsDouble &&
							getNumberOfJumps(tcString) == getNumberOfJumps(tvString) ) 
					{
						//finished writing the TC, let's put a "-1" in the TV
						if (tcCount > tvCount) {
							if(tvCount > 0) { equal = "="; }
							tvString = tvString + equal + "-1";
						}

						write();

						success = true;
					}
				}

				if ( ! success) {
					//don't record the time until the first event
					if (firstRjValue) {
						firstRjValue = false;
					} else {
						//reactive jump has not finished... record the next jump
						if (tcCount == tvCount) {
							double myTc = timestamp/1000;
							//Console.WriteLine("TC: {0}", myTc);
							if(tcCount > 0) { equal = "="; }
							tcString = tcString + equal + myTc.ToString();
							tcCount = tcCount +.5;
						} else {
							//tcCount > tvCount 
							double myTv = timestamp/1000;
							//Console.WriteLine("TV: {0}", myTv);
							if(tvCount > 0) { equal = "="; }
							tvString = tvString + equal + myTv.ToString();
							tvCount = tvCount +.5;
						}
					}
				}
			}
		} while ( ! success && ! cancel );
	}
				

	protected override void write()
	{
		int jumps;
		string limitString = "";
		if(jumpsLimited) {
			limitString = limitAsDouble.ToString() + "J";
			jumps = (int) limitAsDouble;
		} else {
			limitString = limitAsDouble.ToString() + "T";
			string [] myStringFull = tcString.Split(new char[] {'='});
			jumps = myStringFull.Length;
		}
	

		SqliteJump.InsertRj(personID, sessionID, 
				type, Util.GetMax(tvString), Util.GetMax(tcString), 
				fall, weight, "", //fall, weight, description
				Util.GetAverage(tvString), Util.GetAverage(tcString),
				tvString, tcString,
				jumps, getTotalTime(tcString, tvString), limitString
				);

		string myStringPush =   Catalog.GetString("Last jump: ") + JumperName + " " + 
			type + " (" + limitString + ") " +
			" AVG TV: " + Util.TrimDecimals( Util.GetAverage (tvString).ToString(), pDN ) +
			" AVG TC: " + Util.TrimDecimals( Util.GetAverage (tcString).ToString(), pDN ) ;
		appbar.Push( myStringPush );
	
		//put max value in progressBar. This makes the thread in Pulse() stop
		progressBar.Fraction = 1;
	}
	
	private double getTotalTime (string stringTC, string stringTV)
	{
		if(stringTC.Length > 0 && stringTV.Length > 0) {
			string [] tc = stringTC.Split(new char[] {'='});
			string [] tv = stringTV.Split(new char[] {'='});

			double totalTime = 0;

			foreach (string jump in tc) {
				totalTime = totalTime + Convert.ToDouble(jump);
			}
			foreach (string jump in tv) {
				totalTime = totalTime + Convert.ToDouble(jump);
			}

			return totalTime ;
		} else {
			return 0;
		}
	}
	
	private int getNumberOfJumps(string myString)
	{
		if(myString.Length > 0) {
			string [] jumpsSeparated = myString.Split(new char[] {'='});
			int count = 0;
			foreach (string temp in jumpsSeparated) {
				count++;
			}
			if(count == 0) { count =1; }
			
			return count;
		} else { 
			return 0;
		}
	}
	
	public string Limited
	{
		get {
			return limited;
		}
	}
	
	public double TvMax
	{
		get
		{
			return Util.GetMax (tvString);
		}
	}
		
	public double TcMax
	{
		get
		{
			return Util.GetMax (tcString);
		}
	}
		
	public double TvAvg
	{
		get
		{
			return Util.GetAverage (tvString);
		}
	}
		
	public double TcAvg
	{
		get
		{
			return Util.GetAverage (tcString);
		}
	}
	
	public string TvString
	{
		get {
			return tvString;
		}
	}
		
	public string TcString
	{
		get {
			return tcString;
		}
	}
		
	~JumpRj() {}
}

