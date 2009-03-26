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
	//better as private and don't inherit, don't know why
	//protected Chronopic cp;
	private Chronopic cp;

	//2nd Chronopic stuff
	protected Thread thread2;
	//SerialPort sp2;
	private Chronopic cp2;
	private Chronopic.Plataforma platformState2;
	protected States loggedState2;
	//private string port2;
	
	//3rd Chronopic stuff
	protected Thread thread3;
	//SerialPort sp3;
	private Chronopic cp3;
	private Chronopic.Plataforma platformState3;
	protected States loggedState3;
	//private string port3;
	
	//4th Chronopic stuff
	protected Thread thread4;
	//SerialPort sp4;
	private Chronopic cp4;
	private Chronopic.Plataforma platformState4;
	protected States loggedState4;
	//private string port4;
	

	static bool firstValue = true;

	

	public MultiChronopicExecute() {
	}

	//execution
	//public MultiChronopicExecute(Chronopic cp, Gtk.Statusbar appbar, Gtk.Window app, string port2)
	public MultiChronopicExecute(Chronopic cp, Gtk.Statusbar appbar, Gtk.Window app)
	{
		this.cp = cp;
		this.appbar = appbar;
		this.app = app;
		//this.port2 = port2;
		
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
		
		/* this will be good for not continue counting the time on eventWindow when event has finished
		 * this will help to sync chronopic data with the timerCount data
		 * later also, copy the value of the chronopic to the timerCount label
		 */

		/*
		//updateTimeProgressBar();
		if(needEndEvent) {
			eventExecuteWin.EventEnded();
			//needEndEvent = false;
		} else 
			updateTimeProgressBar();
		
		
		if(simulated) {
			eventSimulatedShouldChangePlatform();
		}

		if(needUpdateEventProgressBar) {
			//update event progressbar
			eventExecuteWin.ProgressBarEventOrTimePreExecution(
					updateProgressBar.IsEvent,
					updateProgressBar.PercentageMode,
					updateProgressBar.ValueToShow
					);  

			needUpdateEventProgressBar = false;
		}
		
	
		if(needUpdateGraph) {
			updateGraph();
			needUpdateGraph = false;
		}
		
		if(needSensitiveButtonFinish) {
			eventExecuteWin.ButtonFinishMakeSensitive();
			needSensitiveButtonFinish = false;
		}
		
		//check if it should finish by time
		if(shouldFinishByTime()) {
			finish = true;
			updateProgressBarForFinish();
		} 
		//else 
		//	updateTimeProgressBar();
		*/
	}
			
/*
	//private bool connectChronopic2(string myPort) 
	private bool connectOtherChronopics(Chronopic myCp, SerialPort mySp, Chronopic.Plataforma myPS, string myPort) 
	{
		bool success = false;
		try {
			Log.WriteLine(string.Format("chronopic port: {0}", myPort));
			mySp = new SerialPort(myPort);
			mySp.Open();
			//-- Create chronopic object, for accessing chronopic
			myCp = new Chronopic(mySp);

			//-- Obtener el estado inicial de la plataforma
			bool ok=false;
			do
				ok=myCp.Read_platform(out myPS);
			while(!ok);
			if (!ok) {
				//-- Si hay error terminar
				Log.WriteLine(string.Format("Error: {0}", myCp.Error));
				success = false;
			}
		} catch {
			success = false;
		}
		return success;
	}
*/

	public override void Manage()
	{
		/*
		connectOtherChronopics(cp2, sp2, platformState2, port2);
		//connectOtherChronopics(cp3, sp3, platformState3, port3);
		//connectOtherChronopics(cp4, sp4, platformState4, port4);
		*/

		platformState = chronopicInitialValue(cp);
		platformState2 = chronopicInitialValue(cp2);
		//platformState3 = chronopicInitialValue(cp3);
		//platformState4 = chronopicInitialValue(cp4);
		
		string cpStr = "";
		string cp2Str = "";
		//3
		//4
		
		if (platformState==Chronopic.Plataforma.ON) {
			cpStr = "cp1" + " " + "IN";
			loggedState = States.ON;
		} else {
			cpStr = "cp1" + " " + "OUT";
			loggedState = States.OFF;
		}
			
		if (platformState2==Chronopic.Plataforma.ON) {
			cp2Str = "cp2" + " " + "IN";
			loggedState2 = States.ON;
		} else {
			cp2Str = "cp2" + " " + "OUT";
			loggedState2 = States.OFF;
		}
		//3
		//4
			
		appbar.Push( 1, cpStr + " / " + cp2Str);

		//start thread
		//Log.Write("Start thread");
		thread = new Thread(new ThreadStart(waitEvent));
		thread2 = new Thread(new ThreadStart(waitEvent2));
		//3
		//4

		GLib.Idle.Add (new GLib.IdleHandler (PulseGTK));

		thread.Start(); 
		thread2.Start(); 
		//3
		//4
	}

	
	protected override void waitEvent ()
	{
		double timestamp = 0;
		bool success = false;
		bool ok;

		do {
			ok = cp.Read_event(out timestamp, out platformState);
			
			//if chronopic signal is Ok and state has changed
			if (ok && (
					(platformState == Chronopic.Plataforma.ON && loggedState == States.OFF) ||
					(platformState == Chronopic.Plataforma.OFF && loggedState == States.ON) ) 
						&& !cancel && !finish) {
				
				//while no finished time or jumps, continue recording events
				if ( ! success) {
					//don't record the time until the first event
					if (firstValue) {
						firstValue = false;

						//but start timer
						initializeTimer();
					} else {
						if(platformState == Chronopic.Plataforma.ON && loggedState == States.OFF)
							Log.WriteLine("cp1 landed");
						else if(platformState == Chronopic.Plataforma.OFF && loggedState == States.ON)
							Log.WriteLine("cp1 jumped");
								
						needSensitiveButtonFinish = true;
					}
				}

				if(platformState == Chronopic.Plataforma.OFF)
					loggedState = States.OFF;
				else
					loggedState = States.ON;

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
		
	private void waitEvent2 ()
	{
		double timestamp = 0;
		bool success = false;
		bool ok;

		do {
			ok = cp2.Read_event(out timestamp, out platformState2);
			
			//if chronopic signal is Ok and state has changed
			if (ok && (
					(platformState2 == Chronopic.Plataforma.ON && loggedState2 == States.OFF) ||
					(platformState2 == Chronopic.Plataforma.OFF && loggedState2 == States.ON) ) 
						&& !cancel && !finish) {
				
				//while no finished time or jumps, continue recording events
				if ( ! success) {
					//don't record the time until the first event
					if (firstValue) {
						firstValue = false;

						//but start timer
						initializeTimer();
					} else {
						if(platformState2 == Chronopic.Plataforma.ON && loggedState2 == States.OFF)
							Log.WriteLine("cp2 landed");
						else if(platformState2 == Chronopic.Plataforma.OFF && loggedState2 == States.ON)
							Log.WriteLine("cp2 jumped");
								
						needSensitiveButtonFinish = true;
					}
				}

				if(platformState2 == Chronopic.Plataforma.OFF)
					loggedState2 = States.OFF;
				else
					loggedState2 = States.ON;

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

