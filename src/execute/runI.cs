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
 * Copyright (C) 2004-2024   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Data;
using System.Collections.Generic; //List
using System.Threading;
using System.IO.Ports;
using Mono.Unix;

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
			Chronopic cp, WichroCapture wichroCapture,
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
			bool cameraRecording, int sensorOnceA, int sensorOnceB,
			bool jsonUploadNeedsButton,
			string jsonUploadTestScript,
			string jsonUploadRankingScript
			)
	{
		jsonDataRankingTitle = "RankingSprint";
		jsonDataRankingFile = "/tmp/chronojump_json_sprint_ranking.txt";

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
		this.wichroCapture = wichroCapture;
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
		this.sensorOnceA = sensorOnceA;
		this.sensorOnceB = sensorOnceB;
		this.jsonUploadNeedsButton = jsonUploadNeedsButton;
		this.jsonUploadTestScript = jsonUploadTestScript;
		this.jsonUploadRankingScript = jsonUploadRankingScript;

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

	//contacts_insert_test_button_do, this inserts and later it can be uploaded with button
	public RunIntervalExecute(int personID, int sessionID, string type,
			double distanceInterval, double timeTrack1, double timeTrack2,
			string jsonUploadTestScript, string jsonUploadRankingScript)
	{
		jsonDataRankingTitle = "RankingSprint";
		jsonDataRankingFile = "/tmp/chronojump_json_sprint_ranking.txt";

		this.personID = personID;
		this.sessionID = sessionID;
		this.type = type;
		this.distanceInterval = distanceInterval;
		this.jsonUploadTestScript = jsonUploadTestScript;
		this.jsonUploadRankingScript = jsonUploadRankingScript;

		double distanceTotal = distanceInterval * 2;
		string datetime = UtilDate.ToFile(DateTime.Now);
		timeTotal = timeTrack1 + timeTrack2;

		uniqueID = SqliteRunInterval.Insert(false, Constants.RunIntervalTable, "NULL", personID, sessionID, type,
				distanceTotal, timeTotal,
				distanceInterval, timeTrack1 + "=" + timeTrack2, 2,
				"",
				"2R", 0, true,
				datetime, new List<int>()
				);

		eventDone = new RunInterval (uniqueID, personID, sessionID, type, distanceTotal, timeTotal, distanceInterval, timeTrack1 + "=" + timeTrack2,
				2, "", "2R", 0, true, datetime, new List<int>());
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

			if (! jsonUploadNeedsButton)
			{
				if (jsonUploadTestScript != "")
					JsonUploadTestScriptDo ();
				if (jsonUploadRankingScript != "")
					JsonUploadRankingScriptDo ();
			}

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

	public override void JsonUploadTestScriptDo ()
	{
		double maxSpeed = 0;
		if(distanceInterval == -1)
			maxSpeed = Util.GetRunIVariableDistancesSpeeds (distancesString, intervalTimesString, true);
		else {
			List<double> timeList = ((RunInterval) eventDone).TimeList;
			int count = 0;
			foreach (double time in timeList)
			{
				if (count == 0 || UtilAll.DivideSafe (distanceInterval, time) > maxSpeed)
					maxSpeed = UtilAll.DivideSafe (distanceInterval, time);

				count ++;
			}
		}

		Person p = SqlitePerson.Select (false, personID);
		writeJsonDataThisTest (p, maxSpeed);
		System.Threading.Thread.Sleep(250);
		ExecuteProcess.run (jsonUploadTestScript, false, false);
	}

	public override void JsonUploadRankingScriptDo ()
	{
		writeJsonDataRanking (SqliteRunInterval.GetPersonsRanking (sessionID));
		System.Threading.Thread.Sleep(250);
		ExecuteProcess.run (jsonUploadRankingScript, false, false);
	}

	private void writeJsonDataThisTest (Person p, double maxSpeed)
	{
		/*
		 * fix problems managing url on person description as : are trimmed by some part of chronojump
		 * and copyiing pasting url maybe there's space or enter before
		 */
		string description = p.Description;
		description = description.Replace ("\nhttps", "https");
		description = description.Replace (" https", "https");
		description = description.Replace ("https //", "https://");

		string jsonStr =
			"{\n" +
			"\"Name\":\"" + p.Name + "\",\n" +
			"\"No\":\"(" + p.Future2 + ")\",\n" +
			"\"Photo\":\"" + description + "\",\n" +
			"\"Test\":\"Speed\",\n" +
			"\"Time\":" + Util.ConvertToPoint (Util.TrimDecimals (timeTotal, 2)) + ",\n" +
			"\"MaxSpeed\":" + Util.ConvertToPoint (Util.TrimDecimals (3.6 * maxSpeed, 2)) + "\n" +
			"}";

		TextWriter writer = File.CreateText("/tmp/chronojump_json_sprint_1_test.txt");
		writer.Write(jsonStr);
		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();
	}

	public override string GetDialogResultString ()
	{
		if (type.ToLower () == "forestals")
		{
			if (tracks < 1)
				return string.Format ("Bloc 0\n____");
			else if (tracks < 9)
				return string.Format ("Bloc 1\n{0} / 8", tracks);
			else if (tracks < 19)
				return string.Format ("Bloc 2\n{0} / 10", tracks -8);
			else if (tracks < 31)
				return string.Format ("Bloc 3\n{0} / 12", tracks -18);
			else
				return string.Format ("Fí de\nla prova");
		} else
			return string.Format ("{0} / {1}\n{2} s",
					tracks, limitAsDouble,
					Util.TrimDecimals (Util.GetTotalTime (intervalTimesString), 2)
					);
	}

	~RunIntervalExecute() {}
}
