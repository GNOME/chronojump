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
 * Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
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
	
	string vars; //distance

	bool syncFirst;	
	bool deleteFirst;	
	private enum syncStates { NOTHING, CONTACTED, DONE } //done == released

	static bool firstValue;
	
	
	public MultiChronopicExecute() {
	}

	//execution
	public MultiChronopicExecute(int personID, string personName, int sessionID, string type, 
			Chronopic cp, bool syncFirst, bool deleteFirst, string vars,
			ExecutingGraphData egd
			) {
		this.personID = personID;
		this.personName = personName;
		this.sessionID = sessionID;
		this.type = type;
		
		this.cp = cp;
		this.syncFirst = syncFirst;
		this.deleteFirst = deleteFirst;
		this.vars = vars;
		this.egd = egd;
	
		chronopics = 1; 
		initValues();	
	}
	
	public MultiChronopicExecute(int personID, string personName, int sessionID, string type, 
			Chronopic cp, Chronopic cp2, bool syncFirst, bool deleteFirst, string vars,
			ExecutingGraphData egd, bool cameraRecording
			) {
		this.personID = personID;
		this.personName = personName;
		this.sessionID = sessionID;
		this.type = type;
		
		this.cp = cp;
		this.cp2 = cp2;
		this.syncFirst = syncFirst;
		this.deleteFirst = deleteFirst;
		this.vars = vars;
		this.egd = egd;
		this.cameraRecording = cameraRecording;
	
		chronopics = 2; 
		initValues();	
	}
	
	public MultiChronopicExecute(int personID, string personName, int sessionID, string type, 
			Chronopic cp, Chronopic cp2, Chronopic cp3, bool syncFirst, bool deleteFirst, string vars, 
			ExecutingGraphData egd
 
			) {
		this.personID = personID;
		this.personName = personName;
		this.sessionID = sessionID;
		this.type = type;
		
		this.cp = cp;
		this.cp2 = cp2;
		this.cp3 = cp3;
		this.syncFirst = syncFirst;
		this.deleteFirst = deleteFirst;
		this.vars = vars;
		this.egd = egd;
	
		chronopics = 3; 
		initValues();	
	}

	public MultiChronopicExecute(int personID, string personName, int sessionID, string type,
			Chronopic cp, Chronopic cp2, Chronopic cp3, Chronopic cp4, bool syncFirst, bool deleteFirst, string vars, 
			ExecutingGraphData egd

			) {
		this.personID = personID;
		this.personName = personName;
		this.sessionID = sessionID;
		this.type = type;
		
		this.cp = cp;
		this.cp2 = cp2;
		this.cp3 = cp3;
		this.cp4 = cp4;
		this.syncFirst = syncFirst;
		this.deleteFirst = deleteFirst;
		this.vars = vars;
		this.egd = egd;
	
		chronopics = 4; 
		initValues();	
	}


	private void initValues()
	{
		fakeButtonUpdateGraph = new Gtk.Button();
		fakeButtonCameraStopIfNeeded = new Gtk.Button ();
		fakeButtonThreadDyed = new Gtk.Button();
		simulated = false;

		cp1InStr = "";
		cp1OutStr = "";
		cp2InStr = "";
		cp2OutStr = "";
		cp3InStr = "";
		cp3OutStr = "";
		cp4InStr = "";
		cp4OutStr = "";
		
		if(type == Constants.RunAnalysisName) {
			syncFirst = false;
			deleteFirst = false;
		}

		//initialize eventDone as a mc
		eventDone = new MultiChronopic();
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
			

	public override bool Manage()
	{
		//boolean to know if chronopic has been disconnected	
		chronopicDisconnected = false;
		
		if(chronopics > 0) {
			platformState = chronopicInitialValue(cp);
		
			if (platformState==Chronopic.Plataforma.ON) {
				loggedState = States.ON;
				cp1StartedIn = true;
			} else if (platformState==Chronopic.Plataforma.OFF) {
				loggedState = States.OFF;
				cp1StartedIn = false;
			} else { //UNKNOW (Chronopic disconnected, port changed, ...)
				chronopicHasBeenDisconnected();
				return false;
			}
		
			//prepare jump for being cancelled if desired
			cancel = false;

			//prepare jump for being finished earlier if desired
			finish = false;
		
			if(chronopics > 1) {
				platformState2 = chronopicInitialValue(cp2);

				if (platformState2==Chronopic.Plataforma.ON) {
					loggedState2 = States.ON;
					cp2StartedIn = true;
				} else if (platformState2==Chronopic.Plataforma.OFF) {
					loggedState2 = States.OFF;
					cp2StartedIn = false;
				} else { //UNKNOW (Chronopic disconnected, port changed, ...)
					chronopicHasBeenDisconnected();
					return false;
				}
			
				if(chronopics > 2) {
					platformState3 = chronopicInitialValue(cp3);

					if (platformState3==Chronopic.Plataforma.ON) {
						loggedState3 = States.ON;
						cp3StartedIn = true;
					} else if (platformState3==Chronopic.Plataforma.OFF) {
						loggedState3 = States.OFF;
						cp3StartedIn = false;
					} else { //UNKNOW (Chronopic disconnected, port changed, ...)
						chronopicHasBeenDisconnected();
						return false;
					}

					if(chronopics > 3) {
						platformState4 = chronopicInitialValue(cp4);

						if (platformState4==Chronopic.Plataforma.ON) {
							loggedState4 = States.ON;
							cp4StartedIn = true;
						} else if (platformState4==Chronopic.Plataforma.OFF) {
							loggedState4 = States.OFF;
							cp4StartedIn = false;
						} else { //UNKNOW (Chronopic disconnected, port changed, ...)
							chronopicHasBeenDisconnected();
							return false;
						}
					}
				}
			}
		}

		string platformsProblems = "";
		if(type == Constants.RunAnalysisName) {
			string sep = "";
			if(platformState==Chronopic.Plataforma.ON) {
				platformsProblems = Catalog.GetString("Photocells");
				sep = ", ";
			}
			if(platformState2==Chronopic.Plataforma.ON)
				platformsProblems += sep + Catalog.GetString("Platform");
		}


		if(platformsProblems.Length > 0) {
			ConfirmWindow confirmWin;		
			confirmWin = ConfirmWindow.Show( 
					string.Format(Catalog.GetString("There's contact in {0}. Please leave."), platformsProblems),
					"", "");

			Util.PlaySound(Constants.SoundTypes.BAD, volumeOn, gstreamer);

			//we call again this function
			confirmWin.Button_accept.Clicked += new EventHandler(callAgainManage);

			//if confirmWin.Button_cancel is pressed retuen
			confirmWin.Button_cancel.Clicked += new EventHandler(cancel_event_before_start);
		} else {
			firstValue = true;
			//writingStarted = false;

			//start thread
			if(chronopics > 0) {
				//prepare variables to allow being cancelled or finished
				if(! simulated)
					Chronopic.InitCancelAndFinish();

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
				LogB.ThreadStart(); 
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
		return true;
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
		int runAnalysisTcCount = 0;
		int runAnalysisTfCount = 0;

		bool isFirstOut = true;
		bool isFirstIn = true;

		syncStates syncing = syncStates.DONE;
		if(syncFirst) {
			syncing = syncStates.NOTHING;
			feedbackMessage = Catalog.GetString("Press and maintain Test button in all Chronopics simultaneously.");
			needShowFeedbackMessage = true;
		}

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
					//this is only executed on the first chronopic that receives a change
					if (firstValue) {
						firstValue = false;
						initializeTimer(); //this is for first Chronopic and only for simulated
						
						feedbackMessage = "";
						needShowFeedbackMessage = true; 
					}
							
					if(syncing == syncStates.NOTHING && myPS == Chronopic.Plataforma.ON && myLS == States.OFF) {
						syncing = syncStates.CONTACTED;
						feedbackMessage = Catalog.GetString("Release Test button in all Chronopics simultaneously.");
						needShowFeedbackMessage = true;
					}
					else if (syncing == syncStates.CONTACTED && myPS == Chronopic.Plataforma.OFF && myLS == States.ON) {
						syncing = syncStates.DONE;
						feedbackMessage = Catalog.GetString("Synchronization done.") + "\n" + 
							Catalog.GetString("Test starts now.");
						needShowFeedbackMessage = true;
					}
					else {
						if(type != Constants.RunAnalysisName)
							needSensitiveButtonFinish = true;

						if(myPS == Chronopic.Plataforma.ON && myLS == States.OFF) {
							//this is for runAnalysis, delete first tf on 2nd cp (jump cp)
							if(cpNum == 2 && type == Constants.RunAnalysisName && runAnalysisTfCount == 0)
								runAnalysisTfCount ++;
							//this is for multiChronopic, not for runAnalysis
							else if(deleteFirst && isFirstOut)
								isFirstOut = false;
							else {
								double lastOut = timestamp/1000.0;
								LogB.Information(cpNum.ToString() + " landed: " + lastOut.ToString());
								outStr = outStr + outEqual + lastOut.ToString();
								outEqual = "="; 
							}
							/*
							   if it's a runAnalysis, 
							   should end when arrive at 2n photocell (controlled by cp1)
							   */
							if(cpNum == 1 && type == Constants.RunAnalysisName) {
								runAnalysisTcCount ++;
								if(runAnalysisTcCount >= 2) {
									success = true;
									//better call also finish
									//then all cps know about ending
									finish = true; 
								}
							}
						}
						else if(myPS == Chronopic.Plataforma.OFF && myLS == States.ON) {
							//this is for multiChronopic, not for runAnalysis
							if(deleteFirst && isFirstIn)
								isFirstIn = false;
							else {
								double lastIn = timestamp/1000.0;
								LogB.Information(cpNum.ToString() + " jumped: " + lastIn.ToString());
								inStr = inStr + inEqual + lastIn.ToString();
								inEqual = "="; 
							}
						}

						PrepareEventGraphMultiChronopicObject = new PrepareEventGraphMultiChronopic(
								//timestamp/1000.0, 
								cp1StartedIn, cp2StartedIn, cp3StartedIn, cp4StartedIn,
								cp1InStr, cp1OutStr, cp2InStr, cp2OutStr, 
								cp3InStr, cp3OutStr, cp4InStr, cp4OutStr);
						needUpdateGraphType = eventType.MULTICHRONOPIC;
						needUpdateGraph = true;


						updateProgressBar = new UpdateProgressBar (
								true, //isEvent
								false, //means activity mode
								-1 //don't show text
								);
						needUpdateEventProgressBar = true;
					}
				}

				if(myPS == Chronopic.Plataforma.OFF)
					myLS = States.OFF;
				else
					myLS = States.ON;

			}
		} while ( ! success && ! cancel && ! finish );
	
		if (finish) {
			finishThisCp();

			//call write on gui/chronojump.cs, because if done in execute/MultiChronopic, 
			//will be called n times if n chronopics are working
			//write(false); //tempTable
		}
	}
	
	private void finishThisCp () {
		needEndEvent = true;
	}

	protected override bool shouldFinishByTime() {
		return false; //this kind of events (simple or Dj jumps) cannot be finished by time
	}
	
	protected override void updateTimeProgressBar() {
		//has no finished, but move progressbar time
		progressBarEventOrTimePreExecution(
				false, //isEvent false: time
				false, //activity mode
				-1	//don't want to show info on label
				); 
	}
	
	public override bool MultiChronopicRunAUsedCP2() {
		if (cp2InStr.Length == 0 || cp2OutStr.Length == 0)
			return false;
		return true;
	}

	/*
	maybe we come here four times, one for any chronopic,
	best is to put one bool in order to only let on get inside
	*/
	//bool writingStarted;

	public override void MultiChronopicWrite(bool tempTable)
	{
		LogB.Information("----------WRITING A----------");
	//	if(writingStarted)
	//		return;
	//	else
	//		writingStarted = true; //only one execution can "get in"
		LogB.Information("----------WRITING B----------");

		LogB.Information("cp1 In:" + cp1InStr);
		LogB.Information("cp1 Out:" + cp1OutStr + "\n");
		LogB.Information("cp2 In:" + cp2InStr);
		LogB.Information("cp2 Out:" + cp2OutStr + "\n");
		LogB.Information("cp3 In:" + cp3InStr);
		LogB.Information("cp3 Out:" + cp3OutStr + "\n");
		LogB.Information("cp4 In:" + cp4InStr);
		LogB.Information("cp4 Out:" + cp4OutStr + "\n");
		

		/*
		   if on run analysis arrive to 2nd platform while we are flying, then
		   there are more TCs than TFs
		   if last event was tc, it has no sense, it should be deleted
		*/
		if(type == Constants.RunAnalysisName) 
			cp2InStr = Util.DeleteLastTcIfNeeded(cp2InStr, cp2OutStr);
	

		if(tempTable) //TODO
			uniqueID = SqliteMultiChronopic.Insert(false, Constants.TempMultiChronopicTable, "NULL", 
					personID, sessionID, type,  
					Util.BoolToInt(cp1StartedIn), Util.BoolToInt(cp2StartedIn), 
					Util.BoolToInt(cp3StartedIn), Util.BoolToInt(cp4StartedIn),
					cp1InStr, cp1OutStr, cp2InStr, cp2OutStr,
					cp3InStr, cp3OutStr, cp4InStr, cp4OutStr,
					vars, //distance
					description, Util.BoolToNegativeInt(simulated)
					);
		else {
			uniqueID = SqliteMultiChronopic.Insert(false, Constants.MultiChronopicTable, "NULL", 
					personID, sessionID, type,  
					Util.BoolToInt(cp1StartedIn), Util.BoolToInt(cp2StartedIn), 
					Util.BoolToInt(cp3StartedIn), Util.BoolToInt(cp4StartedIn),
					cp1InStr, cp1OutStr, cp2InStr, cp2OutStr,
					cp3InStr, cp3OutStr, cp4InStr, cp4OutStr,
					vars, //distance
					description, Util.BoolToNegativeInt(simulated)
					);

			//define the created object
			eventDone = new MultiChronopic(uniqueID, personID, sessionID, type, 
					Util.BoolToInt(cp1StartedIn), Util.BoolToInt(cp2StartedIn), 
					Util.BoolToInt(cp3StartedIn), Util.BoolToInt(cp4StartedIn),
					cp1InStr, cp1OutStr, cp2InStr, cp2OutStr,
					cp3InStr, cp3OutStr, cp4InStr, cp4OutStr,
					vars, //distance
					description, Util.BoolToNegativeInt(simulated)); 
		}
	}
	

	~MultiChronopicExecute() {}
	   
}

