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
 * Copyright (C) 2004-2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Data;
using System.Collections.Generic; //List
using System.Threading;
using System.IO.Ports;
using Mono.Unix;

public class RunExecute : EventExecute
{
	protected double distance;
	protected double trackTime;
	protected bool startIn;
	
	protected Chronopic cp;
	//private Chronopic cp; //this doesn't work

	protected string wirelessPort; //"" if not using wireless
	protected int wirelessBauds; //0 if not using wireless
	protected bool wireless;

	protected bool metersSecondsPreferred;

	//used by the updateTimeProgressBar for display its time information
	//changes a bit on runSimple and runInterval
	//explained at each of the updateTimeProgressBar()
	//measureRectionTime will be PLATFORM_INI_YES_TIME
	//START_WIRELESS_UNKNOWN is because we do not know initial status on photocells
	protected enum runPhases {
		START_WIRELESS_UNKNOWN, PRE_RUNNING, PLATFORM_INI_YES_TIME, PLATFORM_INI_NO_TIME, RUNNING, PLATFORM_END
	}
	protected static runPhases runPhase;
		
	protected Constants.DoubleContact checkDoubleContactMode;
	protected int checkDoubleContactTime;
	
	protected bool speedStart; 	  //if we started before the contact
	protected bool speedStartArrival; //(preferences) if true then race time includes reaction time
	protected bool measureReactionTime;
	protected double reactionTimeMS; //reaction time in milliseconds

	//double contacts stuff
	protected double timestampDCFlightTimes; 	//sum of the flight times that happen in small time
	protected double timestampDCContactTimes; 	//sum of the contact times that happen in small time
	protected double timestampDCn; 			//number of flight times in double contacts period
	protected static double lastTc;			//useful to know time on contact platform because intervalTimesString does not differentiate
	protected static double lastTf; 		//used when no double contacts mode
	protected static DateTime timerLastTf; 		//this two will be protected and in runSimpleExecute

	//static because they are used on both threads at the same time
	protected static RunExecuteInspector runEI;
	protected static RunDoubleContact runDC;
	protected static RunChangeImage runChangeImage;
	protected static bool success;
	protected RunExecuteInspector.Types runEIType;
	protected static List<int> photocell_l; //for Wichro

	protected Gtk.Image image_run_execute_running;
	protected Gtk.Image image_run_execute_photocell_icon;
	protected Gtk.Label label_run_execute_photocell_code;

	protected PhotocellWirelessCapture photocellWirelessCapture;

//	protected bool firstTrackDone;

	public RunExecute() {
	}

	//run execution
	//if wireless: string wirelessPort, wirelessBauds will be used instead of Chronopic cp
	public RunExecute(int personID, int sessionID, string type, double distance,   
			Chronopic cp, PhotocellWirelessCapture photocellWirelessCapture,
			string wirelessPort, int wirelessBauds,
			int pDN, bool metersSecondsPreferred,
			bool volumeOn, Preferences.GstreamerTypes gstreamer,
			double progressbarLimit, ExecutingGraphData egd,
			Constants.DoubleContact checkDoubleContactMode, int checkDoubleContactTime, 
			bool speedStartArrival, bool measureReactionTime,
			Gtk.Image image_run_execute_running,
			Gtk.Image image_run_execute_photocell_icon,
			Gtk.Label label_run_execute_photocell_code,
			int graphLimit, bool graphAllTypes, bool graphAllPersons,
			bool cameraRecording
			)
	{
		this.personID = personID;
		this.sessionID = sessionID;
		this.type = type;
		this.distance = distance;
		
		this.cp = cp;
		this.photocellWirelessCapture = photocellWirelessCapture;
		this.wirelessPort = wirelessPort;
		this.wirelessBauds = wirelessBauds;
		wireless = (wirelessPort != "" && wirelessBauds > 0);
		LogB.Information(string.Format("This is a run simple capture with wireless?: {0}", wireless));

		this.pDN = pDN;
		this.metersSecondsPreferred = metersSecondsPreferred;
		this.volumeOn = volumeOn;
		this.gstreamer = gstreamer;
		this.progressbarLimit = progressbarLimit;
		this.egd = egd;
		this.checkDoubleContactMode = checkDoubleContactMode;
		this.checkDoubleContactTime = checkDoubleContactTime;
		this.speedStartArrival = speedStartArrival;	
		this.measureReactionTime = measureReactionTime;
		this.image_run_execute_running = image_run_execute_running;
		this.image_run_execute_photocell_icon = image_run_execute_photocell_icon;
		this.label_run_execute_photocell_code = label_run_execute_photocell_code;
		this.graphLimit = graphLimit;
		this.graphAllTypes = graphAllTypes;
		this.graphAllPersons = graphAllPersons;
		this.cameraRecording = cameraRecording;

		reactionTimeMS = 0;

		fakeButtonUpdateGraph = new Gtk.Button();
		fakeButtonCameraStopIfNeeded = new Gtk.Button ();
		fakeButtonThreadDyed = new Gtk.Button();

		simulated = false;
		
		needUpdateEventProgressBar = false;
		needUpdateGraph = false;
		needCallTrackDone = false;
		needCheckIfTrackEnded = false;
		runEIType = RunExecuteInspector.Types.RUN_SIMPLE;
		photocell_l = new List<int>();
		
		//initialize eventDone as a Run	
		eventDone = new Run();
	}

	
	public override void SimulateInitValues(Random randSent)
	{
		LogB.Information("From execute/run.cs");

		rand = randSent; //we send the random, because if we create here, the values will be the same for each nbew instance
		simulated = true;
		simulatedTimeAccumulatedBefore = 0;
		simulatedTimeLast = 0;
		simulatedContactTimeMin = 0; //seconds
		simulatedContactTimeMax = 1; //seconds ('0' gives problems)
		simulatedFlightTimeMin = 3; //seconds
		simulatedFlightTimeMax = 4; //seconds

	}
	
	public override bool Manage()
	{
		LogB.Debug("MANAGE!!!!");

		//boolean to know if chronopic has been disconnected	
		chronopicDisconnected = false;
		runChangeImage = new RunChangeImage();

		//if(wireless)
		//	manageIniWireless();
		//else
		if(! wireless)
			if (! manageIniNotWireless())
			{
				chronopicDisconnected = true;
				return false;
			}

		//prepare jump for being cancelled if desired
		cancel = false;

		//start thread
		thread = new Thread(new ThreadStart(waitEvent));
		GLib.Idle.Add (new GLib.IdleHandler (PulseGTK));

		LogB.ThreadStart();
		thread.Start();
		return true;
	}

	private void manageIniWireless()
	{
		/*
		 * on wireless we cannot know if on start we are in contact or not
		 * so we cannot set at the moment startIn, loggedState, runPhase
		 */
		LogB.Debug("MANAGE(wireless)!!!!");

		feedbackMessage = Catalog.GetString("RUN when prepared!");
		needShowFeedbackMessage = true;
		Util.PlaySound(Constants.SoundTypes.CAN_START, volumeOn, gstreamer);


		//runPhase = runPhases.START_WIRELESS_UNKNOWN; //done before

		/*
		//provem de forçar a que comencem en off, a veure si va be
		platformState = Chronopic.Plataforma.OFF;
		if (platformState==Chronopic.Plataforma.OFF) {
			feedbackMessage = Catalog.GetString("You are OUT, RUN when prepared!");
			needShowFeedbackMessage = true;
			Util.PlaySound(Constants.SoundTypes.CAN_START, volumeOn, gstreamer);

			loggedState = States.OFF;
			startIn = false;
			runPhase = runPhases.PRE_RUNNING;
			runChangeImage.Current = RunChangeImage.Types.RUNNING;
		}
		*/
	}

	private bool manageIniNotWireless()
	{
		if (simulated)
			platformState = Chronopic.Plataforma.ON;
		else
			platformState = chronopicInitialValue(cp);

		LogB.Debug("MANAGE(Not wireless)!!!!");

		//you can start ON or OFF the platform,
		//we record always de TF (or time between we abandonate the platform since we arrive)
		if (platformState==Chronopic.Plataforma.ON) {
			feedbackMessage = Catalog.GetString("You are IN, RUN when prepared!");
			needShowFeedbackMessage = true;
			Util.PlaySound(Constants.SoundTypes.CAN_START, volumeOn, gstreamer);

			loggedState = States.ON;
			startIn = true;
			runPhase = runPhases.PLATFORM_INI_NO_TIME;
			runChangeImage.Current = RunChangeImage.Types.PHOTOCELL;
		} else if (platformState==Chronopic.Plataforma.OFF) {
			feedbackMessage = Catalog.GetString("You are OUT, RUN when prepared!");
			needShowFeedbackMessage = true;
			Util.PlaySound(Constants.SoundTypes.CAN_START, volumeOn, gstreamer);

			loggedState = States.OFF;
			startIn = false;
			runPhase = runPhases.PRE_RUNNING;
			runChangeImage.Current = RunChangeImage.Types.RUNNING;
		}
		else { //UNKNOW (Chronopic disconnected, port changed, ...)
			chronopicHasBeenDisconnected();
			return false;
		}

		if (simulated) {
			if(startIn) {
				//values of simulation will be the flightTime (between two platforms)
				//at the first time (and the only)
				//start running being on the platform
				simulatedCurrentTimeIntervalsAreContact = false;

				//in simulated mode, make the run start just when we arrive to waitEvent at the first time
				//mark now that we have abandoned platform:
				platformState = Chronopic.Plataforma.OFF;
			} else {
				//values of simulation will be the contactTime
				//at the first time, the second will be flightTime (between two platforms)
				//come with previous run ("salida lanzada")
				simulatedCurrentTimeIntervalsAreContact = true;

				//in simulated mode, make the run start just when we arrive to waitEvent at the first time
				//mark now that we have reached the platform:
				platformState = Chronopic.Plataforma.ON;
			}
		}

		return true;
	}

	protected void timestampDCInitValues()
	{
		timestampDCFlightTimes = 0;
		timestampDCContactTimes = 0;
		timestampDCn = 0;
	}

	protected override void waitEvent ()
	{
		success = false;
		timerCount = 0;
		lastTc = 0;
		int photocell = -1; //-1 is valid for not inalambric and for start capture on inalambrics
		//firstTrackDone = false;

		double timestamp = 0;
		double timestampAccumulated = 0; //for wireless (because instead of split times, it comes absolute time)
		bool ok;

		timestampDCInitValues();
		timerLastTf = DateTime.MinValue;

		//prepare variables to allow being cancelled or finished
		if(! simulated)
			Chronopic.InitCancelAndFinish(); //ok for wireless

		runEI = new RunExecuteInspector(
				runEIType,
				speedStartArrival,
				checkDoubleContactMode,
				checkDoubleContactTime
				);
		runEI.ChangePhase(photocell, RunExecuteInspector.Phases.START);

		//initialize runDC
		runDC = new RunDoubleContact(
				checkDoubleContactMode,
				checkDoubleContactTime,
				speedStartArrival
				);
		runPTL = new RunPhaseTimeList(
				checkDoubleContactMode,
				checkDoubleContactTime
				);

		//PhotocellWirelessCapture pwc = null;
		if(wireless)
		{
			LogB.Information("going to call photocellWirelessCapture.CaptureStart ()");
			feedbackMessage = Catalog.GetString("Please, wait!");
			needShowFeedbackMessage = true;
			runPhase = runPhases.START_WIRELESS_UNKNOWN;
			//photocellWirelessCapture = new PhotocellWirelessCapture(wirelessPort);
			photocellWirelessCapture.Reset ();
			if (! photocellWirelessCapture.CaptureStart ())
			{
				chronopicDisconnected = true;
				cancel = true; //problem reading line (capturing)
			} else
				manageIniWireless();
		}

		bool firstFromChronopicReceived = false;
		bool exitWaitEventBucle = false;
		do {
			if (wireless)
			{
				if(! photocellWirelessCapture.CaptureLine())
				{
					chronopicDisconnected = true;
					cancel = true; //problem reading line (capturing)
				}

				ok = false;
				if(photocellWirelessCapture.CanRead())
				{
					LogB.Information("waitEvent 3");
					PhotocellWirelessEvent pwe = photocellWirelessCapture.PhotocellWirelessCaptureReadNext();
					LogB.Information("wait_event pwe: " + pwe.ToString());

					photocell = pwe.photocell;

					/*
					//to debug photocell assignement on Wichro.
					Random randDelete = new Random();
					photocell = randDelete.Next(0,10);
					*/

					timestamp = pwe.timeMs - timestampAccumulated; //photocell does not send splittime, sends absolute time
					timestampAccumulated += timestamp;

					platformState = pwe.status;

					ok = true;
					LogB.Information("waitEvent 4");
				}
			}
			else if(simulated)
				ok = true;
			else 
				ok = cp.Read_event(out timestamp, out platformState);

			if (ok && ! cancel && ! finish)
			{
//				LogB.Information("waitEvent 7");
				if( ! firstFromChronopicReceived )
				{
					speedStart = has_arrived();
					runDC.SpeedStart = has_arrived();
					runPTL.SpeedStart = has_arrived();
					firstFromChronopicReceived = true;
				}

				//LogB.Information("timestamp:" + timestamp);
				if (has_arrived()) // timestamp is tf
				{
					LogB.Information("has arrived");
					loggedState = States.ON;
					runChangeImage.Current = RunChangeImage.Types.PHOTOCELL;
					if(wireless)
						runChangeImage.Photocell = photocell;

					onlyInterval_NeedShowCountDownFalse();

					//it has confirmed that first phase is PRE_RUNNING
					if(wireless && runPhase == runPhases.START_WIRELESS_UNKNOWN)
					{
						runPhase = runPhases.PRE_RUNNING;
						startIn = false;
					}

					if(runPhase == runPhases.PRE_RUNNING)
					{
						if(speedStartArrival || measureReactionTime) {
							runPhase = runPhases.PLATFORM_INI_YES_TIME;
							//run starts
							LogB.Information("\ninitializeTimer at has_arrived");
							initializeTimer(); //timerCount = 0
							runEI.ChangePhase(photocell, RunExecuteInspector.Phases.IN,
								"TimerStart");
						} else {
							runPhase = runPhases.PLATFORM_INI_NO_TIME;
							runEI.ChangePhase(photocell, RunExecuteInspector.Phases.IN,
								"No timer start until leave plaform");
						}

						feedbackMessage = "";
						needShowFeedbackMessage = true;
					} else {
						/*
						 * on simple runs, run MAYBE finished:
						 * if started outside (behind platform) it's the second arrive
						 * if started inside: it's the first arrive
						 * MAYBE finished because: with new 1.8.1 code, we wait checkTime * 1,5 to see if there are more contacts
						 */
						runPhase = runPhases.RUNNING;
						
						if(simulated)
							timestamp = simulatedTimeLast * 1000; //conversion to milliseconds

						//prevent double contact stuff
						if(runDC.UseDoubleContacts())
						{
							runDC.DoneTF (timestamp, photocell);
							timerLastTf = DateTime.Now;
							needCheckIfTrackEnded = true;
						} else
						{
							lastTf = timestamp;
							//trackDone();
							needCallTrackDone = true;
						}

						runEI.ChangePhase(photocell, RunExecuteInspector.Phases.IN,
								string.Format("Arrived (preparing track) timestamp: {0}", Math.Round(timestamp, 3)));

						runPTL.AddTF(photocell, timestamp);
					}
				}
				else if (has_lifted()) // timestamp is tc
				{
					LogB.Information("has lifted");
					loggedState = States.OFF;
					runChangeImage.Current = RunChangeImage.Types.RUNNING;

					lastTc = 0;

					//it has confirmed that first phase is PLATFORM_INI_YES/NO_TIME
					if(wireless && runPhase == runPhases.START_WIRELESS_UNKNOWN)
					{
						/*
						   on wireless if we are still on unknown, change to INI_NO_TIME
						   as we cannot know how much time we were on the platform, so INI_YES_TIME will be not aplicable
						   */
						runPhase = runPhases.PLATFORM_INI_NO_TIME;
						startIn = true;
					}

					if(runPhase == runPhases.PLATFORM_INI_NO_TIME)
					{
						//run starts
						LogB.Information("\ninitializeTimer at has_lifted");
						initializeTimer(); //timerCount = 0
						runEI.ChangePhase(photocell, RunExecuteInspector.Phases.OUT, "Timer start");

						/*
						 * Stored the TC because we need it to decide
						 * if time will starts after this contact or
						 * other double contacts that will come just after
						 * depending on biggest tc
						 */
						/*
						 * but until 23 may 2023 this timestamp is also finally accounted on first track (in photocells, not in Wichro),
						 * when start in contact and when start before but contact time has to be discarded
						 * so better store 0
						 *
						runDC.DoneTC(timestamp, false, photocell);
						runPTL.AddTC(photocell, timestamp);
						*/
						runDC.DoneTC(0, false, photocell);
						runPTL.AddTC(photocell, 0);

						feedbackMessage = "";
						needShowFeedbackMessage = true;
					}
					else if(runPhase == runPhases.PLATFORM_INI_YES_TIME)
					{
						if(measureReactionTime)
							reactionTimeMS = timestamp;

						if(speedStartArrival)
						{
							lastTc = timestamp / 1000.0;
							runEI.ChangePhase(photocell, RunExecuteInspector.Phases.OUT,
								string.Format("SpeedStartArrival, tc = {0}", Math.Round(lastTc, 3)));
							runDC.DoneTC(timestamp, true, photocell);
							runPTL.AddTC(photocell, timestamp);
						}

						feedbackMessage = "";
						needShowFeedbackMessage = true; 
					} else {
						runEI.ChangePhase(photocell, RunExecuteInspector.Phases.OUT,
								string.Format("SpeedStartArrival, timestamp = {0}", timestamp));

						if(runDC.UseDoubleContacts())
							runDC.DoneTC(timestamp, true, photocell);
						else
							lastTc = timestamp / 1000.0;

						runPTL.AddTC(photocell, timestamp);
					}

					runPhase = runPhases.RUNNING;
				}
			}

			exitWaitEventBucle = false;
			if(success || cancel || finish)
			{
				exitWaitEventBucle = waitToExitWaitEventBucle();
				runChangeImage.Current = RunChangeImage.Types.NONE;
			}

		} while ( ! exitWaitEventBucle );

		if(wireless)
			photocellWirelessCapture.Stop(); //Should we do a disconnect here?

		onlyInterval_FinishWaitEventWrite();
	}

	protected bool has_arrived()
	{
		if(wireless && runPhase == runPhases.START_WIRELESS_UNKNOWN)
			return (platformState == Chronopic.Plataforma.ON);
		else
			return (platformState == Chronopic.Plataforma.ON && loggedState == States.OFF);
	}
	protected bool has_lifted()
	{
		if(wireless && runPhase == runPhases.START_WIRELESS_UNKNOWN)
			return (platformState == Chronopic.Plataforma.OFF);
		else
			return (platformState == Chronopic.Plataforma.OFF && loggedState == States.ON);
	}
	
	/* only run interval functions */

	protected virtual void onlyInterval_NeedShowCountDownFalse()
	{
	}
	protected virtual void onlyInterval_SetRSAVariables()
	{
	}
	protected virtual void onlyInterval_FinishWaitEventWrite()
	{
	}

	/* end of only run interval functions */

	protected bool waitToExitWaitEventBucle()
	{
		if(runDC.UseDoubleContacts())
		{
			LogB.Information(string.Format("success: {0}, cancel: {1}, finish: {2}", success, cancel, finish));

			//to avoid get hung waiting needCheckIfTrackEnded
			DateTime started = DateTime.Now;
			TimeSpan span = DateTime.Now - started;

			while(needCheckIfTrackEnded && span.TotalMilliseconds < checkDoubleContactTime * 1.5)
			{
				LogB.Information("WAITING 100 MS TO EXIT BUCLE");
				//TODO: checks what happens with cancel... in the pulse thread, will change this boolean? needCheckIfTrackEnded
				//TODO: also think on what happens if needCheckIfTrackEnded never gets negative
				Thread.Sleep(100);
				span = DateTime.Now - started;
			}
			if(! needCheckIfTrackEnded)
				LogB.Information("Exited 100 MS BUCLE because needCheckIfTrackEnded is false");
			else
				LogB.Information("Exited 100 MS BUCLE because time exceded");

			return true;
		}
		else
			return true;
	}

	protected override void runChangeImageIfNeeded()
	{
		if(! runChangeImage.ShouldBeChanged())
			return;

		if(runChangeImage.Current == RunChangeImage.Types.RUNNING)
		{
			image_run_execute_running.Visible = true;
			image_run_execute_photocell_icon.Visible = false;
			label_run_execute_photocell_code.Visible = false;
		}
		else if(runChangeImage.Current == RunChangeImage.Types.PHOTOCELL)
		{
			image_run_execute_running.Visible = false;
			image_run_execute_photocell_icon.Visible = true;
			if(runChangeImage.Photocell >= 0)
			{
				label_run_execute_photocell_code.Text = runChangeImage.Photocell.ToString();
				label_run_execute_photocell_code.Visible = true;
			}
		} else
		{
			image_run_execute_running.Visible = false;
			image_run_execute_photocell_icon.Visible = false;
			label_run_execute_photocell_code.Visible = false;
		}
	}

	protected override void runChangeImageForceHide()
	{
		image_run_execute_running.Visible = false;
		image_run_execute_photocell_icon.Visible = false;
		label_run_execute_photocell_code.Visible = false;
	}

	protected override void updateRunPhaseInfoManage()
	{
		//check if it's defined at beginning of race
		if(runDC != null)
			runDC.UpdateList();
	}

	protected override bool lastTfCheckTimeEnded()
	{
		//LogB.Information("In lastTfCheckTimeEnded()");
		TimeSpan span = DateTime.Now - timerLastTf;
		if(span.TotalMilliseconds > checkDoubleContactTime * 1.5)
		{
			timerLastTf = DateTime.Now;
			LogB.Information("lastTfCheckTimeEnded: success");
			return true;
		}
		//LogB.Information("... ended NOT success");
		return false;
	}


	//big change in 1.8.1: this is called from GTK thread
	//so don't write to SQL here
	//and use static variables where needed
	protected override bool trackDone()
	{
		LogB.Information("In trackDone() A");

		runDC.TrackDoneHasToBeCalledAgain = false;
		if(success)
			return true;

		LogB.Information("In trackDone() B");
		//double myTrackTime = 0;
		if(runDC.UseDoubleContacts())
		{
			if(! runDC.FirstTrackDone)
			{
				//TODO: take care because maybe more data can come than just the start, eg:
				//
				//_ __ _   (300ms < > 450ms)      _ _ _        (450ms...)
				//
				//all above line can be processed at once
				//so IsStartDoubleContact will be false
				//because all the tc,tf are processed and tf in the middle is greater than 300ms
				//so start will be marked in the first _
				//when should be done in the second _
				if(runDC.IsStartDoubleContact())
				{
					int posOfBiggestTC = 0;
					if(speedStart)
						posOfBiggestTC = runDC.GetPosOfBiggestTC(false);

					if(speedStart && ! speedStartArrival) 		//speed start and leaving
						runPTL.FirstRPIs = posOfBiggestTC +1;
					else if(speedStart && speedStartArrival) 	//speed start and arrival
						runPTL.FirstRPIs = posOfBiggestTC;
					else 						//no speed start (measure on leaving first contact)
						runPTL.FirstRPIs = posOfBiggestTC +1;

					runDC.UpdateStartPos(posOfBiggestTC);
					return true;
				}
				else
				{
					//if leaving: start pos will be on first TF
					if( runPTL.FirstRPIs == 0 && (! speedStart || (speedStart && ! speedStartArrival)) )
					{
						runPTL.FirstRPIs = 1;
						runDC.UpdateStartPos(1);
					}
				}
			}

			//store because runDC.GetTrackTimeInSecondsAndUpdateStartPos() will change it
			int photocellAtStartPos = runDC.GetPhotocellAtStartPos();

			trackTime = runDC.GetTrackTimeInSecondsAndUpdateStartPos(); //will come in seconds

			//add photocell to photocell_l
			if(trackTime != 0)
			{
				if(photocell_l.Count == 0)
				{
					photocell_l.Add(photocellAtStartPos);
					LogB.Information("photocell_l.Add at start photocell_l: " + photocellAtStartPos.ToString());
				}
				photocell_l.Add(runDC.GetPhotocellAtStartPos());
				LogB.Information("photocell_l.Add photocell_l: " + runDC.GetPhotocellAtStartPos().ToString());
			}

			runDC.FirstTrackDone = true;
		}
		else {
			//note in double contacts mode timestamp can have added
			//DCFlightTimes and DCContactTimes.
			//So contact time is not only on lastTc
			trackTime = lastTc + lastTf/1000.0;
		}

		LogB.Information("trackTime: " + trackTime.ToString());
		//solve possible problems of bad copied data between threads on start
		if(trackTime == 0)
			return false; //helps to fix display of a contact time bigger than double contact time * 1.5

		//maybe this -1 on wireless should be an static variable: lastPhotocell, that can replace the photocell used on waitEvent 
		runEI.ChangePhase(-1, RunExecuteInspector.Phases.IN,
				string.Format("; timestamp: {0}; <b>trackTime: {1}</b>",
					Math.Round(lastTf/1000.0, 3), Math.Round(trackTime, 3)));

		trackDoneRunSpecificStuff();

		/*
		 * searching GetPosOfBiggestTC maybe two different tracks found
		 * so call this method again to process the other
		 */
		if(! success && runDC.TrackDoneHasToBeCalledAgain)
			trackDone();

		return true;
	}

	protected virtual void trackDoneRunSpecificStuff ()
	{
		LogB.Information(string.Format("RACE (simple) TC: {0}; TV: {1}; TOTALTIME: {2}", lastTc, lastTf/1000.0, trackTime));

		/*
		double time = timestamp / 1000.0;

		runEI.ChangePhase(RunExecuteInspector.Phases.IN, runEIString +
				string.Format("; timestamp: {0}; <b>trackTime: {1}</b>",
					Math.Round(timestamp/1000.0, 3), Math.Round(time, 3)));
					*/

		write();
		success = true;

		//as we will be on waitEvent do { ok = cp.Read_event ... }
		//call this to end Read_cambio called by Read_event
		Chronopic.FinishDo();

		updateProgressBar = new UpdateProgressBar (
				true, //isEvent
				true, //percentageMode
				//percentageToPass
				3
				);
		needUpdateEventProgressBar = true;
	}

	protected override bool shouldFinishByTime() {
		return false; //this kind of events (simple runs) cannot be finished by time
	}
	
	protected override void updateTimeProgressBar() {
		/* 4 situations:
		 *   1- if we start out and have not arrived to platform, it should be a pulse with no time value on label:
		 *		case runPhases.PRE_RUNNING
		 *   2-  if we are on the platform, it should be a pulse
		 *   		a) if speedStartArrival (time starts at arriving at platform) || measureReactionTime
		 *   		then time starts and have to be time value on label:
		 *			case runPhases.PLATFORM_INI_YES_TIME
		 *   		b) if ! speedStartArrival (time starts at leaving platform)
		 *   		then time starts and do not have to be time value on label:
		 *			case runPhases.PLATFORM_INI_NO_TIME
		 *   3- if we leave the platform, it should be a pulse with timerCount on label:
		 *		case runPhases.RUNNING
		 *   4- if we arrive (finish), it should be a pulse with chronopic time on label:
		 *		case runPhases.PLATFORM_END.
		 *		Don't update time label here because later it will be overrided with the good data from Chronopic
		 *		and sometimes can happen in different order, and then bad data (timerCount) will be shown on label at the end of test
		 */
			
		if(runPhase == runPhases.PLATFORM_END) //see comment above
			return;
		
		double myTimeValue = 0;
		switch (runPhase) {
			case runPhases.START_WIRELESS_UNKNOWN:
				myTimeValue = -1; //don't show nothing on label_timer
				break;
			case runPhases.PRE_RUNNING:
				myTimeValue = -1; //don't show nothing on label_timer
				break;
			case runPhases.PLATFORM_INI_NO_TIME:
				myTimeValue = -1;
				break;
			case runPhases.PLATFORM_INI_YES_TIME:
				myTimeValue = timerCount; //show time from the timerCount
				break;
			case runPhases.RUNNING:
				myTimeValue = timerCount; //show time from the timerCount
				break;
		}

		//has no finished, but move progressbar time
		progressBarEventOrTimePreExecution(
				false, //isEvent false: time
				false, //activity mode
				myTimeValue
				); 
	}

	protected override void write()
	{
		LogB.Information(string.Format("tracktime: {0}", trackTime.ToString()));
		
		/*
		string myStringPush =   Catalog.GetString("Last run") + ": " + RunnerName + " " + 
			type + " " + Catalog.GetString("time") + ": " + Util.TrimDecimals( time.ToString(), pDN ) + 
			" " + Catalog.GetString("speed") + ": " + Util.TrimDecimals ( (distance/time).ToString(), pDN );
		*/
		if(simulated)
			feedbackMessage = Catalog.GetString(Constants.SimulatedMessage());
		else
			feedbackMessage = "";
		needShowFeedbackMessage = true; 


		string description = "";
		if(type == "Margaria") {
			// P = W * 9.8 * D / t   
			// W: person weight
			// D: distance between 3d and 9th stair
			double weight = SqlitePersonSession.SelectAttribute(false, personID, sessionID, Constants.Weight);
			double distanceMeters = distance / 1000;
			description = "P = " + Util.TrimDecimals ( (weight * 9.8 * distanceMeters / trackTime).ToString(), pDN) + " (Watts)";
		} else if(type == "Gesell-DBT") 
			description = "0";

		if(measureReactionTime && reactionTimeMS > 0)
			description += descriptionAddReactionTime(reactionTimeMS, pDN, speedStartArrival);

		string table = Constants.RunTable;
		string datetime = UtilDate.ToFile(DateTime.Now);

		uniqueID = SqliteRun.Insert(false, table, "NULL", personID, sessionID, 
				type, distance, trackTime, description,
				Util.BoolToNegativeInt(simulated), 
				!startIn,	//initialSpeed true if not startIn
				datetime
				); 

		//define the created object
		eventDone = new Run(uniqueID, personID, sessionID, type, distance, trackTime, description,
				Util.BoolToNegativeInt(simulated), !startIn, datetime);

		if(graphAllTypes)
			type = "";

		/* 2.2.2 do not do the graph here because PrepareEventGraphRunSimple has an SQL call with a reader
		   and updateGraph can be also called by gtk thread and also call PrepareEventGraphRunSimple,
		   so SQL can be tried to open again, but the problem is in reader that if both run at same time it will crash (seen a log on 2.2.1)
		   Note on_run_finished (main thread) also calls updateGraphRunsSimple(); so graph will be updated at end
		   Note also the PrepareEventGraphRunIntervalRealtimeCaptureObject has no SQL calls, and the PrepareEventGraphRunInterval is not called while capture

		//app1.PrepareRunSimpleGraph(time, distance/time);
		PrepareEventGraphRunSimpleObject = new PrepareEventGraphRunSimple(
				trackTime, distance/trackTime, sessionID,
				personID, graphAllPersons, graphLimit,
				table, type);

		needUpdateGraphType = eventType.RUN;
		needUpdateGraph = true;
		*/
		
		needEndEvent = true; //used for hiding some buttons on eventWindow
	}

	protected string reactionTimeIncludedStr = Catalog.GetString("Included on race time");
	protected string reactionTimeNotIncludedStr = Catalog.GetString("Not included on race time");

	protected string descriptionAddReactionTime(double rtimeMS, int ndec, bool speedStartArrival)
	{
		string str = "";
		if(speedStartArrival)
			str = reactionTimeIncludedStr;
		else
			str = reactionTimeNotIncludedStr;

		return Util.TrimDecimals(rtimeMS / 1000.0, ndec) + " ms (" + str + ")";
	}

	public override string GetInspectorMessages()
	{
		if(runPTL != null)
			return runEI.ToString() + "\n\n" + runPTL.ToString() + "\n\n" + runPTL.InListForPaintingToString();
		else
			return runEI.ToString();
	}

	public string RunnerName
	{
		get { return SqlitePerson.SelectAttribute(personID, Constants.Name); }
	}

	~RunExecute() {}
	   
}

public class RunIntervalExecute : RunExecute
{
	double timeTotal;
	double distanceInterval;

	//commented because it was assigned but never used
	//string limited; //the teorically values, eleven runs: "11R" (time recorded in "time"), 10 seconds: "10T" (tracks recorded in tracks)
	double limitAsDouble;	//-1 for non limited (unlimited repetitive run until "finish" is clicked)
	bool tracksLimited;

	string distancesString; //if distances are variable (distanceInterval == -1), this is used

	//static because they are used on both threads at the same time
	static double tracks; //double because if we limit by time (runType tracksLimited false), we do n.nn tracks
	static string intervalTimesString;
	//since trackDone is called by PulseGTK (onTimer)
							
	private bool RSABellDone;
	private string equal;
	private FeedbackRunsInterval feedbackRunsI;
	//private int countForSavingTempTable;

	//private Chronopic cp;

	public RunIntervalExecute() {
	}

	//run execution
	public RunIntervalExecute(int personID, int sessionID, string type, double distanceInterval, double limitAsDouble, bool tracksLimited,  
			Chronopic cp, PhotocellWirelessCapture photocellWirelessCapture,
			string wirelessPort, int wirelessBauds,
			int pDN, bool metersSecondsPreferred,
			bool volumeOn, Preferences.GstreamerTypes gstreamer,
			FeedbackRunsInterval feedbackRunsI,
			double progressbarLimit, ExecutingGraphData egd ,
			Constants.DoubleContact checkDoubleContactMode, int checkDoubleContactTime, 
			bool speedStartArrival, bool measureReactionTime,
			Gtk.Image image_run_execute_running,
			Gtk.Image image_run_execute_photocell_icon,
			Gtk.Label label_run_execute_photocell_code,
			bool cameraRecording
			)
	{
		this.personID = personID;
		this.sessionID = sessionID;
		this.type = type;
		this.distanceInterval = distanceInterval;
		this.limitAsDouble = limitAsDouble;
		this.tracksLimited = tracksLimited;

		//if distances are variable
		distancesString = "";
		if(distanceInterval == -1) {
			RunType runType = SqliteRunIntervalType.SelectAndReturnRunIntervalType(type, false);
			distancesString = runType.DistancesString;
		}


		if(tracksLimited) {
			//commented because it was assigned but never used
			//this.limited = limitAsDouble.ToString() + "R"; //'R'uns (don't put 'T'racks for not confusing with 'T'ime)
		} else {
			//commented because it was assigned but never used
			//this.limited = limitAsDouble.ToString() + "T";
			timeTotal = limitAsDouble;
		}
		
		
		this.cp = cp;
		this.photocellWirelessCapture = photocellWirelessCapture;
		this.wirelessPort = wirelessPort;
		this.wirelessBauds = wirelessBauds;
		wireless = (wirelessPort != "" && wirelessBauds > 0);
		LogB.Information(string.Format("This is a run interval capture with wireless?: {0}", wireless));

		this.metersSecondsPreferred = metersSecondsPreferred;
		this.pDN = pDN;
		this.volumeOn = volumeOn;
		this.gstreamer = gstreamer;
		this.feedbackRunsI = feedbackRunsI;
		this.progressbarLimit = progressbarLimit;
		this.egd = egd;
		this.checkDoubleContactMode = checkDoubleContactMode;
		this.checkDoubleContactTime = checkDoubleContactTime;
		this.speedStartArrival = speedStartArrival;	
		this.measureReactionTime = measureReactionTime;
		this.image_run_execute_running = image_run_execute_running;
		this.image_run_execute_photocell_icon = image_run_execute_photocell_icon;
		this.label_run_execute_photocell_code = label_run_execute_photocell_code;
		this.cameraRecording = cameraRecording;

		reactionTimeMS = 0;
		reactionTimeIncludedStr = Catalog.GetString("Included on race time of first track");
		reactionTimeNotIncludedStr = Catalog.GetString("Not included on race time of first track");

		fakeButtonUpdateGraph = new Gtk.Button();
		fakeButtonCameraStopIfNeeded = new Gtk.Button ();
		fakeButtonThreadDyed = new Gtk.Button();

		simulated = false;
		
		needUpdateEventProgressBar = false;
		needUpdateGraph = false;
		needCallTrackDone = false;
		needCheckIfTrackEnded = false;
		runEIType = RunExecuteInspector.Types.RUN_INTERVAL;

		timesForSavingRepetitive = 1; //number of times that this repetive event needs for being recorded in temporal table

		//initialize variables
		equal = "";
		intervalTimesString = "";
		tracks = 0;
		//countForSavingTempTable = 0;
		finishByTimeReturnedTrueAtThisCapture = false;

		photocell_l = new List<int>();

		//initialize eventDone as a RunInterval
		eventDone = new RunInterval();
	}


	/* only run interval functions */

	protected override void onlyInterval_NeedShowCountDownFalse()
	{
		//show RSA count down only on air
		needShowCountDown = false;
	}

	protected override void onlyInterval_SetRSAVariables()
	{
		double RSAseconds = Util.GetRunIVariableDistancesThisRowIsRSA(
				distancesString, Convert.ToInt32(tracks));
		if(RSAseconds > 0) {
			RSABellDone = false;
			needShowCountDown = true;
		} else {
			needShowCountDown = false;
			feedbackMessage = "";
			needShowFeedbackMessage = true;
		}
	}

	//TODO: is this needed at all with new 1.8.1 code?
	protected override void onlyInterval_FinishWaitEventWrite()
	{
		if (finish)
		{
			runPhase = runPhases.PLATFORM_END;

			//write();
			//write only if there's a run at minimum
			if(Util.GetNumberOfJumps(intervalTimesString, false) >= 1) {
				writeRunInterval(false); //tempTable = false
			} else {
				//cancel a run if clicked finish before any events done, or ended by time without events
				cancel = true;
			}
		}
	}

	/* end of only run interval functions */


	protected override void trackDoneRunSpecificStuff ()
	{
		LogB.Information(string.Format("RACE TRACK ({0}) TC: {1}; TV: {2}; TOTALTIME: {3}", tracks, lastTc, lastTf/1000.0, trackTime));

		if(intervalTimesString.Length > 0) { equal = "="; }
		intervalTimesString = intervalTimesString + equal + trackTime.ToString();
		updateTimerCountWithChronopicData(intervalTimesString);
		tracks ++;

		/*
		 * Attention:
		 * don't do this because we are on GTK thread right now
		 * and here we are touching SQL
		 *
		//save temp table if needed
		countForSavingTempTable ++;
		if(countForSavingTempTable == timesForSavingRepetitive) {
			writeRunInterval(true); //tempTable
			countForSavingTempTable = 0;
		}
		*/

		if(limitAsDouble == -1) {
			//has arrived, unlimited
			updateProgressBar= new UpdateProgressBar (
					true, //isEvent
					true, //unlimited: activity mode
					tracks
					);
			needUpdateEventProgressBar = true;
		}
		else {
			//has arrived, limited
			if (tracksLimited) {
				//has arrived, limited by tracks
				if(tracks >= limitAsDouble) 
				{
					runPhase = runPhases.PLATFORM_END;

					//finished
					writeRunInterval(false); //tempTable = false
					success = true;

					//as we will be on waitEvent do { ok = cp.Read_event ... }
					//call this to end Read_cambio called by Read_event
			                Chronopic.FinishDo();
				}
				//progressBarEventOrTimePreExecution(
				updateProgressBar= new UpdateProgressBar (
						true, //isEvent
						true, //tracksLimited: percentageMode
						tracks
						);
				needUpdateEventProgressBar = true;
			} else {
				//has arrived, limited by time
				updateProgressBar= new UpdateProgressBar (
						true, //isEvent
						false, //timeLimited: activity mode
						tracks
						);
				needUpdateEventProgressBar = true;
			}
		}

		double distancePre = Util.GetRunITotalDistance (distanceInterval, distancesString, tracks -1);
		double distancePost = Util.GetRunITotalDistance (distanceInterval, distancesString, tracks);
		double distance = distancePost - distancePre;
		//LogB.Information (string.Format ("distancePre: {0}, distancePost: {1}, distance : {2}, speed: {3}, time: {4}",
		//	distancePre, distancePost, distance, distance/trackTime,trackTime));

		if (feedbackRunsI.Green (UtilAll.DivideSafe (distance, trackTime), trackTime))
			Util.PlaySound (Constants.SoundTypes.GOOD, volumeOn, gstreamer);
		else if (feedbackRunsI.Red (UtilAll.DivideSafe (distance, trackTime), trackTime))
			Util.PlaySound (Constants.SoundTypes.BAD, volumeOn, gstreamer);

		//update graph
		PrepareEventGraphRunIntervalRealtimeCaptureObject = new PrepareEventGraphRunIntervalRealtimeCapture (
				type, intervalTimesString, distanceInterval, distancesString,
				photocell_l, startIn, success);

		needUpdateGraphType = eventType.RUNINTERVAL;
		needUpdateGraph = true;
		//fakeButtonUpdateGraph.Click();

		//put button_finish as sensitive when first jump is done (there's something recordable)
		if(tracks == 1)
			needSensitiveButtonFinish = true;

		onlyInterval_SetRSAVariables();
	}


	protected override string countDownMessage()
	{
		double waitSeconds = Util.GetRunIVariableDistancesThisRowIsRSA(distancesString, Convert.ToInt32(tracks))
			 - (timerCount - Util.GetTotalTime(intervalTimesString) - lastTc);

		if (waitSeconds < 0) {
		       	if(! RSABellDone) {
				Util.PlaySound(Constants.SoundTypes.GOOD, volumeOn, gstreamer);
				RSABellDone = true;
			}
			return Catalog.GetString("Go!");
		} else {
			return string.Format(Catalog.GetPluralString(
						"Wait 1 second.",
						"Wait {0} seconds.",
						Convert.ToInt32(Math.Ceiling(waitSeconds))),
					Math.Ceiling(waitSeconds));
		}

	}

	static bool finishByTimeReturnedTrueAtThisCapture;
	protected override bool shouldFinishByTime()
	{
		//do not call FinishDo n times while waiting catchedTimeOut there
		if(finishByTimeReturnedTrueAtThisCapture)
			return false;

		//check if it should finish now (time limited, not unlimited and time exceeded)
		//check that the run started
		//if( ! tracksLimited && limitAsDouble != -1 && timerCount > limitAsDouble 
		if( ! tracksLimited && limitAsDouble != -1
				&& !(runPhase == runPhases.START_WIRELESS_UNKNOWN)
				&& !(runPhase == runPhases.PRE_RUNNING) 
				&& !(runPhase == runPhases.PLATFORM_INI_NO_TIME)
				&& !(runPhase == runPhases.PLATFORM_INI_YES_TIME)
				&& timerLastTf > DateTime.MinValue
				&& (Util.GetTotalTime(intervalTimesString) + (DateTime.Now - timerLastTf).TotalSeconds) > limitAsDouble
				) 
		{
			LogB.Information("shouldFinishByTime finishes Chronopic calling FinishDo");
			//as we will be on waitEvent do { ok = cp.Read_event ... }
			//call this to end Read_cambio called by Read_event
			Chronopic.FinishDo();

			finishByTimeReturnedTrueAtThisCapture = true;

			return true;
		}
		else
			return false;
	}

	protected override void updateTimeProgressBar() {
		/* 4 situations:
		 *   1- if we start out and have not arrived to platform, it should be a pulse with no time value on label:
		 *		case runPhases.PRE_RUNNING
		 *   2-  if we are on the platform, it should be a pulse
		 *   		a) if speedStartArrival (time starts at arriving at platform) || measureReactionTime
		 *   		then time starts and have to be time value on label:
		 *			case runPhases.PLATFORM_INI_YES_TIME
		 *   		b) if ! speedStartArrival (time starts at leaving platform)
		 *   		then time starts and do not have to be time value on label:
		 *			case runPhases.PLATFORM_INI_NO_TIME
		 *   3- we are in the platform or outside at any time except 1,2 and 4. timerCount have to be shown, and progress should be Fraction or Pulse depending on if it's time limited or not:
		 *		case runPhases.RUNNING
		 *   4- if we arrive (finish), it should be a pulse with chronopic time on label:
		 *		case runPhases.PLATFORM_END
		 *		Don't update time label here because later it will be overrided with the good data from Chronopic
		 *		and sometimes can happen in different order, and then bad data (timerCount) will be shown on label at the end of test
		 */
		
		if(runPhase == runPhases.PLATFORM_END) //see comment above
			return;
		
		double myTimeValue = 0;
		bool percentageMode = true; //false is activity mode
		switch (runPhase) {
			case runPhases.START_WIRELESS_UNKNOWN:
				percentageMode = false;
				myTimeValue = -1; //don't show nothing on label_timer
				break;
			case runPhases.PRE_RUNNING:
				percentageMode = false;
				myTimeValue = -1; //don't show nothing on label_timer
				break;
			case runPhases.PLATFORM_INI_NO_TIME:
				percentageMode = false;
				myTimeValue = -1;
				break;
			case runPhases.PLATFORM_INI_YES_TIME:
				percentageMode = !tracksLimited;
				myTimeValue = timerCount; //show time from the timerCount
				break;
			case runPhases.RUNNING:
				percentageMode = !tracksLimited;
				myTimeValue = timerCount; //show time from the timerCount
				break;
		}
		if(! finish) 
			progressBarEventOrTimePreExecution(
					false, //isEvent false: time
					//!tracksLimited, //if tracksLimited: activity, if timeLimited: fraction
					percentageMode,
					myTimeValue
					); 
	}

	private void updateTimerCountWithChronopicData(string timesString) {
		//update timerCount, with the chronopic data
		timerCount =  Util.GetTotalTime(timesString);
	}
				
	protected void writeRunInterval(bool tempTable)
	{
		int tracksHere = 0; //different than global tracks variable
		string limitString = "";

		//if user clicked in finish earlier
		if(finish) {
			if(tracksLimited) {
				tracksHere = Util.GetNumberOfJumps(intervalTimesString, false);
				limitString = tracksHere.ToString() + "R";
			} else {
				//when we mark that run should finish by time, chronopic thread is probably capturing data
				//check if it captured more than date limit, and if it has done, delete last(s) run(s)
				if(limitAsDouble != -1) {
					bool eventPassed = Util.EventPassedFromMaxTime(intervalTimesString, limitAsDouble);
					while(eventPassed) {
						intervalTimesString = Util.DeleteLastSubEvent(intervalTimesString);

						//run limited by time that first subRun has arrived later than maximum for the whole run,
						//and DeleteLastSubEvent returns "-" as a mark
						if(intervalTimesString[0] == '-') {
							//this dialog can make crash the software because the non-gui part calls it
							//new DialogMessage(Constants.MessageTypes.WARNING, 
							//		Catalog.GetString("Race will not be recorded, 1st lap is out of time"));

							feedbackMessage = Catalog.GetString("Race will not be recorded. Out of time.");
							needShowFeedbackMessage = true; 
							LogB.Information("Race will not be recorded, 1st lap is out of time");
	
							//mark for not having problems with cancelled
							cancel = true;

							//end this piece of code
							return;
						} else {
							LogB.Information("Deleted one event out of time");
							eventPassed = Util.EventPassedFromMaxTime(intervalTimesString, limitAsDouble);
						}
					}
				}
				//tracksHere are defined here (and not before) because can change on "while(eventPassed)" before
				tracksHere = Util.GetNumberOfJumps(intervalTimesString, false);
				limitString = Util.GetTotalTime(intervalTimesString) + "T";
			}
		} else {
			if(tracksLimited) {
				limitString = limitAsDouble.ToString() + "R";
				tracksHere = (int) limitAsDouble;
			} else {
				limitString = limitAsDouble.ToString() + "T";
				string [] myStringFull = intervalTimesString.Split(new char[] {'='});
				tracksHere = myStringFull.Length;
			}
		}

		double distanceTotal = Util.GetRunITotalDistance(distanceInterval, distancesString, tracksHere);
		timeTotal = Util.GetTotalTime(intervalTimesString); 
		

		string description = "";
		if(type == "MTGUG")
			description = "u u u u u u"; //undefined 6 items of questionnaire
		//note MTGUG will not have reaction time measurement to have description read correctly by the rest of the software
		else if(measureReactionTime && reactionTimeMS > 0)
			description += descriptionAddReactionTime(reactionTimeMS, pDN, speedStartArrival);

		string datetime = UtilDate.ToFile(DateTime.Now);

		if(tempTable)
			SqliteRunInterval.Insert(false, Constants.TempRunIntervalTable, "NULL", personID, sessionID, type, 
					distanceTotal, timeTotal,
					distanceInterval, intervalTimesString, tracksHere, 
					description,
					limitString,
					Util.BoolToNegativeInt(simulated),
					!startIn,	//initialSpeed true if not startIn
					datetime,
					photocell_l
					);
		else {
			uniqueID = SqliteRunInterval.Insert(false, Constants.RunIntervalTable, "NULL", personID, sessionID, type, 
					distanceTotal, timeTotal,
					distanceInterval, intervalTimesString, tracksHere, 
					description,
					limitString,
					Util.BoolToNegativeInt(simulated),
					!startIn,
					datetime,
					photocell_l
					);

			//define the created object
			eventDone = new RunInterval(uniqueID, personID, sessionID, type, distanceTotal, timeTotal, distanceInterval, intervalTimesString,
					tracksHere, description, limitString, Util.BoolToNegativeInt(simulated), !startIn, datetime, photocell_l);

			if(simulated)
				feedbackMessage = Catalog.GetString(Constants.SimulatedMessage());
			else
				feedbackMessage = "";
			needShowFeedbackMessage = true; 

			PrepareEventGraphRunIntervalRealtimeCaptureObject = new PrepareEventGraphRunIntervalRealtimeCapture (
					type, intervalTimesString, distanceInterval, distancesString,
					photocell_l, startIn, success);

			needUpdateGraphType = eventType.RUNINTERVAL;
			needUpdateGraph = true;

			needEndEvent = true; //used for hiding some buttons on eventWindow, and also for updateTimeProgressBar here
		}
	}

	~RunIntervalExecute() {}
}
