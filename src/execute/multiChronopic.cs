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
 */

using System;
using System.Data;
using System.Text; //StringBuilder

using System.Threading;
using System.IO.Ports;
using Mono.Unix;

public class MultiChronopicExecute : EventExecute
{
	private Chronopic cp;
	string cp1InStr;
	string cp1OutStr;
	bool cp1StartedIn;

	//2nd Chronopic stuff
	protected Thread thread2;
	private Chronopic cp2;
	private Chronopic.Plataforma platformState2;
	protected States loggedState2;
	string cp2InStr;
	string cp2OutStr;
	bool cp2StartedIn;
	
	//3rd Chronopic stuff
	protected Thread thread3;
	private Chronopic cp3;
	private Chronopic.Plataforma platformState3;
	protected States loggedState3;
	string cp3InStr;
	string cp3OutStr;
	bool cp3StartedIn;
	
	//4th Chronopic stuff
	protected Thread thread4;
	private Chronopic cp4;
	private Chronopic.Plataforma platformState4;
	protected States loggedState4;
	string cp4InStr;
	string cp4OutStr;
	bool cp4StartedIn;
	

	static bool firstValue;
	int chronopics; 
	
	public MultiChronopicExecute() {
	}

	//execution
	public MultiChronopicExecute(EventExecuteWindow eventExecuteWin, int personID, string personName, int sessionID, 
			Chronopic cp, Gtk.Statusbar appbar, Gtk.Window app) {
		this.eventExecuteWin = eventExecuteWin;
		this.personID = personID;
		this.personName = personName;
		this.sessionID = sessionID;
		
		this.cp = cp;
		
		this.appbar = appbar;
		this.app = app;
	
		chronopics = 1; 
		initValues();	
	}
	
	public MultiChronopicExecute(EventExecuteWindow eventExecuteWin, int personID, string personName, int sessionID, 
			Chronopic cp, Chronopic cp2, Gtk.Statusbar appbar, Gtk.Window app) {
		this.eventExecuteWin = eventExecuteWin;
		this.personID = personID;
		this.personName = personName;
		this.sessionID = sessionID;
		
		this.cp = cp;
		this.cp2 = cp2;
		
		this.appbar = appbar;
		this.app = app;
	
		chronopics = 2; 
		initValues();	
	}
	
	public MultiChronopicExecute(EventExecuteWindow eventExecuteWin, int personID, string personName, int sessionID, 
			Chronopic cp, Chronopic cp2, Chronopic cp3, Gtk.Statusbar appbar, Gtk.Window app) {
		this.eventExecuteWin = eventExecuteWin;
		this.personID = personID;
		this.personName = personName;
		this.sessionID = sessionID;
		
		this.cp = cp;
		this.cp2 = cp2;
		this.cp3 = cp3;
		
		this.appbar = appbar;
		this.app = app;
	
		chronopics = 3; 
		initValues();	
	}

	public MultiChronopicExecute(EventExecuteWindow eventExecuteWin, int personID, string personName, int sessionID, 
			Chronopic cp, Chronopic cp2, Chronopic cp3, Chronopic cp4, Gtk.Statusbar appbar, Gtk.Window app) {
		this.eventExecuteWin = eventExecuteWin;
		this.personID = personID;
		this.personName = personName;
		this.sessionID = sessionID;
		
		this.cp = cp;
		this.cp2 = cp2;
		this.cp3 = cp3;
		this.cp4 = cp4;
		
		this.appbar = appbar;
		this.app = app;
	
		chronopics = 4; 
		initValues();	
	}


	private void initValues() {
		fakeButtonFinished = new Gtk.Button();
		simulated = false;
	}
	
	public override void SimulateInitValues(Random randSent)
	{
	}

	/*
	//onTimer allow to update progressbar_time every 50 milliseconds
	//also can change platform state in simulated mode
	//protected void onTimer( Object source, ElapsedEventArgs e )
	protected override void onTimer( )
	{
		timerCount = timerCount + .05; //0,05 segons == 50 milliseconds, time between each call of onTimer
	}
	*/
			

	public override void Manage()
	{
		if(chronopics > 0) {
			platformState = chronopicInitialValue(cp);
		
			if (platformState==Chronopic.Plataforma.ON) {
				loggedState = States.ON;
				cp1StartedIn = true;
			} else {
				loggedState = States.OFF;
				cp1StartedIn = false;
			}
		
			if(chronopics > 1) {
				platformState2 = chronopicInitialValue(cp2);

				if (platformState2==Chronopic.Plataforma.ON) {
					loggedState2 = States.ON;
					cp2StartedIn = true;
				} else {
					loggedState2 = States.OFF;
					cp2StartedIn = false;
				}

				if(chronopics > 2) {
					platformState3 = chronopicInitialValue(cp3);

					if (platformState3==Chronopic.Plataforma.ON) {
						loggedState3 = States.ON;
						cp3StartedIn = true;
					} else {
						loggedState3 = States.OFF;
						cp3StartedIn = false;
					}

					if(chronopics > 3) {
						platformState4 = chronopicInitialValue(cp4);

						if (platformState4==Chronopic.Plataforma.ON) {
							loggedState4 = States.ON;
							cp4StartedIn = true;
						} else {
							loggedState4 = States.OFF;
							cp4StartedIn = false;
						}
					}
				}
			}
		}

		firstValue = true;

		//start thread
		if(chronopics > 0) {
			thread = new Thread(new ThreadStart(waitEventPre));
			if(chronopics > 1) {
				thread2 = new Thread(new ThreadStart(waitEventPre2));
				if(chronopics > 2) {
					thread3 = new Thread(new ThreadStart(waitEventPre3));
					if(chronopics > 3) {
						thread4 = new Thread(new ThreadStart(waitEventPre4));
					}
				}
			}
		}

		GLib.Idle.Add (new GLib.IdleHandler (PulseGTK));

		if(chronopics > 0) {
			thread.Start(); 
			if(chronopics > 1) {
				thread2.Start(); 
				if(chronopics > 2) {
					thread3.Start(); 
					if(chronopics > 4) {
						thread4.Start(); 
					}
				}
			}
		}

	}

	protected void waitEventPre () { waitEvent(cp, platformState, loggedState, out cp1InStr, out cp1OutStr, 1); }
	
	protected void waitEventPre2 () { waitEvent(cp2, platformState2, loggedState2, out cp2InStr, out cp2OutStr, 2); }
	
	protected void waitEventPre3 () { waitEvent(cp3, platformState3, loggedState3, out cp3InStr, out cp3OutStr, 3); }
	
	protected void waitEventPre4 () { waitEvent(cp4, platformState4, loggedState4, out cp4InStr, out cp4OutStr, 4); }
	
	
	protected void waitEvent (Chronopic myCP, Chronopic.Plataforma myPS, States myLS, out string inStr, out string outStr, int cpNum)
	{
		double timestamp = 0;
		bool success = false;
		bool ok;
		string inEqual = "";
		string outEqual = "";
		
		inStr = ""; outStr = "";

		do {
			ok = myCP.Read_event(out timestamp, out myPS);
			
			//if chronopic signal is Ok and state has changed
			if (ok && (
					(myPS == Chronopic.Plataforma.ON && myLS == States.OFF) ||
					(myPS == Chronopic.Plataforma.OFF && myLS == States.ON) ) 
						&& !cancel && !finish) {
				
				//while no finished time or jumps, continue recording events
				if ( ! success) {
					//don't record the time until the first event of the first Chronopic
					if (firstValue) {
						firstValue = false;
						initializeTimer();
					} else {
						needSensitiveButtonFinish = true;

						if(myPS == Chronopic.Plataforma.ON && myLS == States.OFF) {
							double lastOut = timestamp/1000.0;
							Log.WriteLine(cpNum.ToString() + " landed: " + lastOut.ToString());
							outStr = outStr + outEqual + lastOut.ToString();
							outEqual = "="; 
						}
						else if(myPS == Chronopic.Plataforma.OFF && myLS == States.ON) {
							double lastIn = timestamp/1000.0;
							Log.WriteLine(cpNum.ToString() + " jumped: " + lastIn.ToString());
							inStr = inStr + inEqual + lastIn.ToString();
							inEqual = "="; 
						}

						prepareEventGraphMultiChronopic = new PrepareEventGraphMultiChronopic(timestamp/1000.0, 
								cp1StartedIn, cp2StartedIn, cp3StartedIn, cp4StartedIn,
								cp1InStr, cp1OutStr, cp2InStr, cp2OutStr, cp3InStr, cp3OutStr, cp4InStr, cp4OutStr);
						needUpdateGraphType = eventType.MULTICHRONOPIC;
						needUpdateGraph = true;
					}

					updateProgressBar = new UpdateProgressBar (
							true, //isEvent
							false, //means activity mode
							-1 //don't show text
							);
					needUpdateEventProgressBar = true;

				}

				if(myPS == Chronopic.Plataforma.OFF)
					myLS = States.OFF;
				else
					myLS = States.ON;

			}
		} while ( ! success && ! cancel && ! finish );
	
		if (finish) {
			write();
			totallyFinished = true;
		}
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
		/*
		//has no finished, but move progressbar time
		eventExecuteWin.ProgressBarEventOrTimePreExecution(
				false, //isEvent false: time
				false, //activity mode
				-1	//don't want to show info on label
				); 
				*/
	}

	protected override void write()
	{
		Log.WriteLine("----------WRITING----------");
		Console.WriteLine("cp1 In:" + cp1InStr);
		Console.WriteLine("cp1 Out:" + cp1OutStr + "\n");
		Console.WriteLine("cp2 In:" + cp2InStr);
		Console.WriteLine("cp2 Out:" + cp2OutStr + "\n");
		Console.WriteLine("cp3 In:" + cp3InStr);
		Console.WriteLine("cp3 Out:" + cp3OutStr + "\n");
		Console.WriteLine("cp4 In:" + cp4InStr);
		Console.WriteLine("cp4 Out:" + cp4OutStr + "\n");
		
		//event will be raised, and managed in chronojump.cs
		fakeButtonFinished.Click();
		
		needEndEvent = true; //used for hiding some buttons on eventWindow
	}
	

	~MultiChronopicExecute() {}
	   
}

