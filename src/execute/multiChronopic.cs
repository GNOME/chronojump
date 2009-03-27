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

	//2nd Chronopic stuff
	protected Thread thread2;
	private Chronopic cp2;
	private Chronopic.Plataforma platformState2;
	protected States loggedState2;
	
	//3rd Chronopic stuff
	protected Thread thread3;
	private Chronopic cp3;
	private Chronopic.Plataforma platformState3;
	protected States loggedState3;
	
	//4th Chronopic stuff
	protected Thread thread4;
	private Chronopic cp4;
	private Chronopic.Plataforma platformState4;
	protected States loggedState4;
	

	static bool firstValue = true;
	int chronopics; 
	

	public MultiChronopicExecute() {
	}

	//execution
	public MultiChronopicExecute(Chronopic cp, Gtk.Statusbar appbar, Gtk.Window app) {
		this.cp = cp;
		this.appbar = appbar;
		this.app = app;
	
		chronopics = 1; 
		initValues();	
	}
	
	public MultiChronopicExecute(Chronopic cp, Chronopic cp2, Gtk.Statusbar appbar, Gtk.Window app) {
		this.cp = cp;
		this.cp2 = cp2;
		this.appbar = appbar;
		this.app = app;
	
		chronopics = 2; 
		initValues();	
	}
	
	public MultiChronopicExecute(Chronopic cp, Chronopic cp2, Chronopic cp3, Gtk.Statusbar appbar, Gtk.Window app) {
		this.cp = cp;
		this.cp2 = cp2;
		this.cp3 = cp3;
		this.appbar = appbar;
		this.app = app;
	
		chronopics = 3; 
		initValues();	
	}

	public MultiChronopicExecute(Chronopic cp, Chronopic cp2, Chronopic cp3, Chronopic cp4, Gtk.Statusbar appbar, Gtk.Window app) {
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

	//onTimer allow to update progressbar_time every 50 milliseconds
	//also can change platform state in simulated mode
	//protected void onTimer( Object source, ElapsedEventArgs e )
	protected override void onTimer( )
	{
		timerCount = timerCount + .05; //0,05 segons == 50 milliseconds, time between each call of onTimer
	}
			

	public override void Manage()
	{
		if(chronopics > 0) {
			platformState = chronopicInitialValue(cp);
		
			string cpStr = "";
			if (platformState==Chronopic.Plataforma.ON) {
				cpStr = "cp1" + " " + "IN";
				loggedState = States.ON;
			} else {
				cpStr = "cp1" + " " + "OUT";
				loggedState = States.OFF;
			}
			appbar.Push( 1, cpStr);
		
			if(chronopics > 1) {
				platformState2 = chronopicInitialValue(cp2);
		
				string cp2Str = "";
				if (platformState2==Chronopic.Plataforma.ON) {
					cp2Str = "cp2" + " " + "IN";
					loggedState2 = States.ON;
				} else {
					cp2Str = "cp2" + " " + "OUT";
					loggedState2 = States.OFF;
				}
				appbar.Push( 1, cpStr + " / " + cp2Str);

				if(chronopics > 2) {
					platformState3 = chronopicInitialValue(cp3);

					string cp3Str = "";
					if (platformState3==Chronopic.Plataforma.ON) {
						cp3Str = "cp3" + " " + "IN";
						loggedState3 = States.ON;
					} else {
						cp3Str = "cp3" + " " + "OUT";
						loggedState3 = States.OFF;
					}
					appbar.Push( 1, cpStr + " / " + cp2Str + "/" + cp3Str);

					if(chronopics > 3) {
						platformState4 = chronopicInitialValue(cp4);

						string cp4Str = "";
						if (platformState4==Chronopic.Plataforma.ON) {
							cp4Str = "cp4" + " " + "IN";
							loggedState4 = States.ON;
						} else {
							cp4Str = "cp4" + " " + "OUT";
							loggedState4 = States.OFF;
						}
						appbar.Push( 1, cpStr + " / " + cp2Str + "/" + cp3Str + "/" + cp4Str);
					}
				}
			}
		}

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

	protected void waitEventPre () { waitEvent(cp, platformState, loggedState, "cp1"); }
	
	protected void waitEventPre2 () { waitEvent(cp2, platformState2, loggedState2, "cp2"); }
	
	protected void waitEventPre3 () { waitEvent(cp3, platformState3, loggedState3, "cp3"); }
	
	protected void waitEventPre4 () { waitEvent(cp4, platformState4, loggedState4, "cp4"); }
	
	
	protected void waitEvent (Chronopic myCP, Chronopic.Plataforma myPS, States myLS, string cpStr)
	{
		double timestamp = 0;
		bool success = false;
		bool ok;

		do {
			ok = myCP.Read_event(out timestamp, out myPS);
			
			//if chronopic signal is Ok and state has changed
			if (ok && (
					(myPS == Chronopic.Plataforma.ON && myLS == States.OFF) ||
					(myPS == Chronopic.Plataforma.OFF && myLS == States.ON) ) 
						&& !cancel && !finish) {
				
				//while no finished time or jumps, continue recording events
				if ( ! success) {
					//don't record the time until the first event
					if (firstValue) {
						firstValue = false;
						initializeTimer();
					} else 
						needSensitiveButtonFinish = true;
						
					if(myPS == Chronopic.Plataforma.ON && myLS == States.OFF)
						Log.WriteLine(cpStr + " landed");
					else if(myPS == Chronopic.Plataforma.OFF && myLS == States.ON)
						Log.WriteLine(cpStr + " jumped");

				}

				if(myPS == Chronopic.Plataforma.OFF)
					myLS = States.OFF;
				else
					myLS = States.ON;

			}
		} while ( ! success && ! cancel && ! finish );
	
		if (finish) {
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
	}
	

	~MultiChronopicExecute() {}
	   
}

