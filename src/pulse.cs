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

public class Pulse : Event
{
	double fixedPulse;
	int totalPulsesNum;

	string timesString;
	int tracks;
	double contactTime;
	bool firstPulse;

	//for finishing earlier from chronojump.cs
	private bool finish;
	
	//better as private and don't inherit, don't know why
	//protected Chronopic cp;
	private Chronopic cp;

	
	//used on treeviewPulse
	public Pulse() {
	}
	
	//execution
	public Pulse(int personID, string personName, int sessionID, string type, double fixedPulse, int totalPulsesNum,  
			Chronopic cp, Gtk.ProgressBar progressBar, Gtk.Statusbar appbar, Gtk.Window app, 
			int pDN)
	{
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

	public override void Simulate(Random rand)
	{
		double intervalTime;
		timesString = "";
		string equalSymbol = "";
		
		//if it's a unlimited pulse and it's simulated, put random value in totalPulsesNum
		if(totalPulsesNum == -1) {
			totalPulsesNum = Convert.ToInt32(rand.NextDouble() * 7) +10; //+10 for not allowing being 0
		}
		
		for (double i=0 ; i < totalPulsesNum ; i++) {
			intervalTime = rand.NextDouble() * 15;
			timesString = timesString + equalSymbol + intervalTime.ToString();
			equalSymbol = "=";
		}
		
		write();
	}

	public override void Manage(object o, EventArgs args)
	{
		Chronopic.Plataforma platformState = chronopicInitialValue(cp);

		
		bool success = false;

		//you should start OFF (outside) the platform 
		//we record always de TC+TV (or time between we pulse platform and we pulse again)
		//we don't care about the time between the get in and the get out the platform
		if (platformState==Chronopic.Plataforma.ON) {
			string myMessage = Catalog.GetString("You are IN, please leave the platform, prepare for start, and press the 'accept' button!!");

			ConfirmWindow confirmWin;		
			confirmWin = ConfirmWindow.Show(app, myMessage, "");

			//we call again this function
			confirmWin.Button_accept.Clicked += new EventHandler(Manage);
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

			//start thread
			thread = new Thread(new ThreadStart(waitEvent));
			GLib.Idle.Add (new GLib.IdleHandler (PulseGTK));
			thread.Start(); 
		}
	}
	
	protected override void waitEvent ()
	{
		double timestamp;
		bool success = false;
		string equal = "";
		double pbUnlimited = 0;
		double myPb = 0;
		
		Chronopic.Plataforma platformState;	//on (in platform), off (jumping), or unknow
		bool ok;

		
		do {
			ok = cp.Read_event(out timestamp, out platformState);
			//if (respuesta == Chronopic.Respuesta.Ok) {
			if (ok) {
				Console.WriteLine("P1");
				if (platformState == Chronopic.Plataforma.ON && loggedState == States.OFF) {
					//has arrived
					loggedState = States.ON;
					
					//if we arrive to the platform for the first time, don't record anything
					if (firstPulse) {
						firstPulse = false;
					} else {
						//if is "unlimited", 
						//then play with the progress bar until finish button is pressed
						if(totalPulsesNum == -1) {
							pbUnlimited += 0.19;
							if(pbUnlimited >= 1.0) { pbUnlimited = 0; }
							progressBar.Fraction = pbUnlimited; 

							if(timesString.Length > 0) { equal = "="; }
							timesString = timesString + equal + (contactTime/1000 + timestamp/1000).ToString();
							tracks ++;	
						}
						else {
							tracks ++;	
							myPb = (double) tracks / totalPulsesNum ;
							if(myPb >= 1.0) { myPb = 0.99; }
							progressBar.Fraction = myPb; 

							if(timesString.Length > 0) { equal = "="; }
							timesString = timesString + equal + (contactTime/1000 + timestamp/1000).ToString();

							if(tracks >= totalPulsesNum) 
							{
								//finished
								write();
								success = true;
							}
						}
					}
				}
				else if (platformState == Chronopic.Plataforma.OFF && loggedState == States.ON) {
					//it's out, was inside (= has abandoned platform)
					//don't record time
					//progressBar.Fraction = progressBar.Fraction + 0.1;
				
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
	

	protected override void write()
	{
		int totalPulsesNum = 0;

		//if user clicked in finish earlier
		//if(finish) {
			totalPulsesNum = Util.GetNumberOfJumps(timesString, false);
		/*
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
		*/

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
	
	//called from chronojump.cs for finishing jumps earlier
	public bool Finish
	{
		get { return finish; }
		set { finish = value; }
	}
	

	public string TimesString
	{
		get { return timesString; }
		set { timesString = value; }
	}
	
	/*
	public int Tracks
	{
		get { return tracks; }
		set { tracks = value; }
	}
	*/
	
	public double FixedPulse
	{
		get { return fixedPulse; }
		set { fixedPulse = value; }
	}
	
		
		
	~Pulse() {}
}

