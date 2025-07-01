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
 * Copyright (C) 2004-2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Data;
using System.Text; //StringBuilder

using System.Threading;
using System.IO.Ports;
using Mono.Unix;

public class JumpRjExecute : JumpExecute
{
	string tvString;
	string tcString;

	//commented because it was assigned but never used
	//string limited; //the teorically values, eleven jumps: "11=J" (time recorded in "time"), 10 seconds: "10=T" (jumps recorded in jumps)
	double limitAsDouble;	//-1 for non limited (unlimited repetitive jump until "finish" is clicked)
	bool jumpsLimited;
	bool firstRjValue;
	private double tcCount;
	private double tvCount;
	private double lastTc;
	private double lastTv;
	
	//better as private and don't inherit, don't know why
	private Chronopic cp;

	//this records a jump when time has finished (if jumper was in the air)
	private bool allowFinishAfterTime;
	//this will be a flag for finishing if allowFinishAfterTime is true
	private bool shouldFinishAtNextFall = true;
	private FeedbackJumpsRj feedbackJumpsRj;

	private string angleString = "-1";
	
	public JumpRjExecute() {
	}

	//jump execution
	public JumpRjExecute(int personID, string personName, double personWeight,
			int sessionID, int typeID, string type, double fall, double weight,
			double limitAsDouble, bool jumpsLimited, 
			Chronopic cp, int pDN, bool allowFinishAfterTime,
			bool volumeOn, Preferences.GstreamerTypes gstreamer,
			bool metersSecondsPreferred, FeedbackJumpsRj feedbackJumpsRj,
			double progressbarLimit, ExecutingGraphData egd,
			Gtk.Image image_jump_execute_air, Gtk.Image image_jump_execute_land,
			bool upload, int uploadStationId, bool django, //upload: configChronojump.Compujump && upload (contacts) button active
			bool cameraRecording
			)
	{
		this.personID = personID;
		this.personName = personName;
		this.personWeight = personWeight; //for Stiffness at upload on compujump
		this.sessionID = sessionID;
		this.type = type;
		this.typeID = typeID;
		this.fall = fall;
		this.weight = weight;
		this.limitAsDouble = limitAsDouble;

		this.jumpsLimited = jumpsLimited;
		/*
		commented because it was assigned but never used
		if(jumpsLimited) {
			this.limited = limitAsDouble.ToString() + "J";
		} else {
			//this.limited = limitAsDouble.ToString() + "T"; define later, because it can be higher if allowFinishRjAfterTime is defined
		}
		*/
		
		this.cp = cp;

		this.pDN = pDN;
		this.allowFinishAfterTime = allowFinishAfterTime;
		this.volumeOn = volumeOn;
		this.gstreamer = gstreamer;
		this.metersSecondsPreferred = metersSecondsPreferred;
		this.feedbackJumpsRj = feedbackJumpsRj;
		this.progressbarLimit = progressbarLimit;
		this.egd = egd;
	
		if(TypeHasFall) { hasFall = true; } 
		else { hasFall = false; }

		this.image_jump_execute_air = image_jump_execute_air;
		this.image_jump_execute_land = image_jump_execute_land;
		this.upload = upload;
		this.uploadStationId = uploadStationId;
		this.django = django;
		this.cameraRecording = cameraRecording;
		
		fakeButtonUpdateGraph = new Gtk.Button();
		fakeButtonCameraStopIfNeeded = new Gtk.Button ();
		fakeButtonThreadDyed = new Gtk.Button();
		
		simulated = false;
			
		needUpdateEventProgressBar = false;
		needUpdateGraph = false;
		
		timesForSavingRepetitive = 10; //number of times that this repetive event needs for being recorded in temporal table

		//initialize eventDone as a JumpRj	
		eventDone = new JumpRj();
	}

	public override bool Manage()
	{
		//boolean to know if chronopic has been disconnected	
		chronopicDisconnected = false;
		jumpChangeImage = new JumpChangeImage();

		if (simulated)
			if(hasFall) 
				platformState = Chronopic.Plataforma.OFF;
			else 
				platformState = Chronopic.Plataforma.ON;
		else
			platformState = chronopicInitialValue(cp);

		if(platformState == Chronopic.Plataforma.OFF)
			loggedState = States.OFF;
		else if(platformState == Chronopic.Plataforma.ON)
			loggedState = States.ON;
		else { //UNKNOW (Chronopic disconnected, port changed, ...)
			chronopicHasBeenDisconnected();
			return false;
		}

		jumpChangeImageDo (platformState);

		bool success = false;

		if (platformState==Chronopic.Plataforma.OFF && hasFall )
		{
			feedbackMessage = Catalog.GetString("You are OUT, JUMP when prepared!");
			Util.PlaySound(Constants.SoundTypes.CAN_START, volumeOn, gstreamer);
			jumpPhase = jumpPhases.PRE_OR_DOING;
			success = true;
		} else if (platformState==Chronopic.Plataforma.ON && ! hasFall )
		{
			feedbackMessage = Catalog.GetString("You are IN, JUMP when prepared!");
			Util.PlaySound(Constants.SoundTypes.CAN_START, volumeOn, gstreamer);
			jumpPhase = jumpPhases.PRE_OR_DOING;
			success = true;
		} else {
			if (platformState == Chronopic.Plataforma.OFF)
			{
				feedbackMessage = Catalog.GetString ("You are OUT, please enter the platform, and then jump when prepared!");
				loggedState = States.OFF;
				jumpPhase = jumpPhases.PERSON_OUT_BUT_NEED_TO_START_IN;
			}
			else // (platformState == Chronopic.Plataforma.ON)
			{
				feedbackMessage = Catalog.GetString ("You are IN, please leave the platform, and then jump when prepared!");
				loggedState = States.ON;
				jumpPhase = jumpPhases.PERSON_IN_BUT_NEED_TO_START_OUT;
			}
			success = true;
		}
		needShowFeedbackMessage = true;

		if(success) {
			//initialize strings of TCs and TFs
			tcString = "";
			tvString = "";
			tcCount = 0;
			tvCount = 0;
			firstRjValue = true;

			//if jump starts on TF, write a "-1" in TC
			if ( ! hasFall ) {
				double myTc = -1;
				tcString = myTc.ToString();
				tcCount = 1;
			}

			//prepare jump for being cancelled if desired
			cancel = false;
			
			//prepare jump for being finished earlier if desired
			finish = false;
			
			
			//in simulated mode, make the jump start just when we arrive to waitEvent at the first time
			//mark now that the opposite as before:
			if (simulated) {
				if(hasFall)
					platformState = Chronopic.Plataforma.ON;
				else 
					platformState = Chronopic.Plataforma.OFF;
			}
			
			//start thread
			thread = new Thread(new ThreadStart(waitEvent));
			GLib.Idle.Add (new GLib.IdleHandler (PulseGTK));
			
			LogB.ThreadStart();
			thread.Start();
		}
		return true;
	}
	
	protected override void waitEvent ()
	{
		double timestamp = 0;
		bool success = false;
				
		shouldFinishAtNextFall = false;
		
		bool ok;

		int countForSavingTempTable = 0;
	
		//prepare variables to allow being cancelled or finished
		if(! simulated)
			Chronopic.InitCancelAndFinish();

		do {
			if(simulated) 
				ok = true;
			else
				ok = cp.Read_event(out timestamp, out platformState);

			if (platformState == Chronopic.Plataforma.OFF)
				jumpChangeImage.Current = JumpChangeImage.Types.AIR;
			else if (platformState == Chronopic.Plataforma.ON)
				jumpChangeImage.Current = JumpChangeImage.Types.LAND;

			//if chronopic signal is Ok and state has changed
			if (ok && ! cancel && ! finish)
			{
				if (jumpPhase == jumpPhases.PERSON_OUT_BUT_NEED_TO_START_IN)
				{
					if (platformState == Chronopic.Plataforma.OFF)
						continue;

					else if (platformState == Chronopic.Plataforma.ON)
					{
						jumpChangeImageDo (platformState);
						feedbackMessage = Catalog.GetString ("You are IN, JUMP when prepared!");
						needShowFeedbackMessage = true;
						Util.PlaySound(Constants.SoundTypes.CAN_START, volumeOn, gstreamer);
						loggedState = States.ON;

						jumpPhase = jumpPhases.PRE_OR_DOING;
					}
				}
				else if (jumpPhase == jumpPhases.PERSON_IN_BUT_NEED_TO_START_OUT)
				{
					if (platformState == Chronopic.Plataforma.ON)
						continue;

					else if (platformState == Chronopic.Plataforma.OFF)
					{
						jumpChangeImageDo (platformState);
						feedbackMessage = Catalog.GetString ("You are OUT, JUMP when prepared!");
						needShowFeedbackMessage = true;
						Util.PlaySound(Constants.SoundTypes.CAN_START, volumeOn, gstreamer);
						loggedState = States.OFF;

						jumpPhase = jumpPhases.PRE_OR_DOING;
					}
				}

				if ( (platformState == Chronopic.Plataforma.ON && loggedState == States.OFF) ||
						(platformState == Chronopic.Plataforma.OFF && loggedState == States.ON) ) 
				{
					if(simulated)
						timestamp = simulatedTimeLast * 1000; //conversion to milliseconds

					LogB.Information(Util.GetTotalTime(tcString, tvString).ToString());

					string equal = "";

					//while no finished time or jumps, continue recording events
					if ( ! success) {
						//don't record the time until the first event
						if (firstRjValue) {
							firstRjValue = false;

							//but start timer
							initializeTimer();

							feedbackMessage = "";
							needShowFeedbackMessage = true; 
						} else {
							//reactive jump has not finished... record the next jump
							LogB.Information(string.Format("tcCount: {0}, tvCount: {1}", tcCount, tvCount));
							if ( tcCount == tvCount )
							{
								lastTc = timestamp/1000.0;

								if (feedbackJumpsRj.TcGreen (lastTc))
									Util.PlaySound(Constants.SoundTypes.GOOD, volumeOn, gstreamer);
								else if (feedbackJumpsRj.TcRed (lastTc))
									Util.PlaySound(Constants.SoundTypes.BAD, volumeOn, gstreamer);

								if(tcCount > 0) { equal = "="; }
								tcString = tcString + equal + lastTc.ToString();

								updateTimerCountWithChronopicData(tcString, tvString);

								tcCount = tcCount + 1;
							} else {
								//tcCount > tvCount 
								lastTv = timestamp/1000.0;

								if (feedbackJumpsRj.TvGreen (lastTv))
									Util.PlaySound(Constants.SoundTypes.GOOD, volumeOn, gstreamer);
								else if (feedbackJumpsRj.TvRed (lastTv))
									Util.PlaySound(Constants.SoundTypes.BAD, volumeOn, gstreamer);

								if(tvCount > 0) { equal = "="; }
								tvString = tvString + equal + lastTv.ToString();

								updateTimerCountWithChronopicData(tcString, tvString);							
								tvCount = tvCount + 1;

								//update event progressbar
								//app1.ProgressBarEventOrTimePreExecution(
								updateProgressBar= new UpdateProgressBar (
										true, //isEvent
										jumpsLimited, //if jumpsLimited: do fraction; if time limited: do pulse
										tvCount
										);  
								needUpdateEventProgressBar = true;

								//update graph
								PrepareEventGraphJumpReactiveRealtimeCaptureObject = new PrepareEventGraphJumpReactiveRealtimeCapture(lastTv, lastTc, tvString, tcString, type);
								needUpdateGraphType = eventType.JUMPREACTIVE;
								needUpdateGraph = true;

								//put button_finish as sensitive when first jump is done (there's something recordable)
								if(tvCount == 1)
									needSensitiveButtonFinish = true;

								//save temp table if needed
								countForSavingTempTable ++;
								if(countForSavingTempTable == timesForSavingRepetitive) {
									writeRj(true); //tempTable
									countForSavingTempTable = 0;
								}

							}
						}
					}

					//if we finish by time, and allowFinishAfterTime == true, when time passed, if the jumper is jumping
					//if flags the shouldFinishAtNextFall that will finish when he arrives to the platform
					if(shouldFinishAtNextFall && platformState == Chronopic.Plataforma.ON && loggedState == States.OFF)
						finish = true;


					//check if reactive jump should finish
					if (jumpsLimited) {
						if(limitAsDouble != -1) {
							if(Util.GetNumberOfJumps(tvString, false) >= limitAsDouble)
							{
								jumpPhase = jumpPhases.PLATFORM_END;

								writeRj(false); //tempTable
								success = true;

								//update event progressbar
								//app1.ProgressBarEventOrTimePreExecution(
								updateProgressBar= new UpdateProgressBar (
										true, //isEvent
										true, //percentageMode
										tvCount
										);  
								needUpdateEventProgressBar = true;

								//update graph
								PrepareEventGraphJumpReactiveRealtimeCaptureObject = new PrepareEventGraphJumpReactiveRealtimeCapture(lastTv, lastTc, tvString, tcString, type);
								needUpdateGraphType = eventType.JUMPREACTIVE;
								needUpdateGraph = true;
							}
						}
					} 

					if (platformState == Chronopic.Plataforma.OFF)
						loggedState = States.OFF;
					else
						loggedState = States.ON;
				}
			}
		} while ( ! success && ! cancel && ! finish );
		
		if (finish) {
			//write only if there's a jump at minimum
			if(Util.GetNumberOfJumps(tcString, false) >= 1 && Util.GetNumberOfJumps(tvString, false) >= 1) {
				jumpPhase = jumpPhases.PLATFORM_END;

				writeRj(false); //tempTable
			} else {
				//cancel a jump if clicked finish before any events done
				cancel = true;
			}
		}
	}

	protected override bool shouldFinishByTime() {
		//check if it should finish now (time limited, not unlimited and time exceeded)
		//check also that rj has started (!firstRjValue)

		//if( !jumpsLimited && limitAsDouble != -1 && timerCount > limitAsDouble && !firstRjValue)
		if( !jumpsLimited && limitAsDouble != -1 && Util.GetTotalTime(tcString, tvString) > limitAsDouble && !firstRjValue)
		{
			//limited by Time, we are jumping and time passed
			if ( tcCount == tvCount ) {
				//if we are on floor
				return true;
			} else {
				//we are on air
				if(allowFinishAfterTime) {
					LogB.Information("ALLOW!!");
					//allow to finish later, return false, and waitEvent (looking at shouldFinishAtNextFall)
					//will finishJump when he falls 
					shouldFinishAtNextFall = true;
					return false;
				} else {
					//we are at air, but ! shouldFinishAfterTime, then finish now discarding current jump
					return true;
				}
			}
		}
		else
			//we haven't finished, return false
			return false;
	}
	
	protected override void updateProgressBarForFinish() {
		/*
		progressBarEventOrTimePreExecution(
				false, //isEvent false: time
				true, //percentageMode: it has finished, show bar at 100%
				//limitAsDouble
				Util.GetTotalTime(tcString, tvString)
				);  
				*/
	}

	protected override void updateTimeProgressBar() {
		//limited by jumps or time or unlimited, but has no finished
		
		if(jumpPhase == jumpPhases.PLATFORM_END)
			return;


		if(firstRjValue)  
			//until it has not landed for first time, show a pulse with no values
			progressBarEventOrTimePreExecution(
					false, //isEvent false: time
					false, //activity mode
					-1	//don't want to show info on label
					); 
		else
			//after show a progressBar with time value
			if(! finish) //this finish happens when user clicks 'finish' or it finished by time. The above PLATFORM_END is used when test end by done tracks
				progressBarEventOrTimePreExecution(
						false, //isEvent false: time
						!jumpsLimited, //if jumpsLimited: activity, if timeLimited: fraction
						timerCount
						); 
	}


	private void updateTimerCountWithChronopicData(string tcString, string tvString) {
		//update timerCount, with the chronopic data
		//but in the first jump probably one is zero and then GetTotalTime returns a 0
		LogB.Information(string.Format("///I timerCount: {0} tcString+tvString: {1} ///", timerCount, Util.GetTotalTime(tcString) + Util.GetTotalTime(tvString)));
		if(tvString.Length == 0) 
			timerCount =  Util.GetTotalTime(tcString);
		else if (tcString.Length == 0) 
			timerCount =  Util.GetTotalTime(tvString);
		else 
			timerCount =  Util.GetTotalTime(tcString, tvString);
	}
				
				
	protected void writeRj(bool tempTable)
	{
		LogB.Information("----------WRITING----------");
		int jumps;
		string limitString = "";
		string description = "";

		//if user clicked in finish earlier
		//or toggled with shouldFinishAtNextTime
		if(finish) {
			//if user clicked finish and last event was tc, probably there are more TCs than TFs
			//if last event was tc, it has no sense, it should be deleted
			tcString = Util.DeleteLastTcIfNeeded(tcString, tvString);

			//when we mark that jump should finish by time, chronopic thread is probably capturing data
			//check if it captured more than date limit, and if it has done, delete last(s) jump(s)
			//also have in mind that allowFinishAfterTime exist
			bool deletedEvent = false;
			if( ! jumpsLimited && limitAsDouble != -1) {
				bool eventPassed = Util.EventPassedFromMaxTime(tcString, tvString, limitAsDouble, allowFinishAfterTime);
				while(eventPassed) {
					tcString = Util.DeleteLastSubEvent(tcString);
					tvString = Util.DeleteLastSubEvent(tvString);
					LogB.Information("Deleted one event out of time");
					eventPassed = Util.EventPassedFromMaxTime(tcString, tvString, limitAsDouble, allowFinishAfterTime);
					deletedEvent = true;
				}
			}
			if(deletedEvent) {
				//update graph if a event was deleted
				PrepareEventGraphJumpReactiveRealtimeCaptureObject = new PrepareEventGraphJumpReactiveRealtimeCapture(Util.GetLast(tvString), Util.GetLast(tcString), tvString, tcString, type);
				needUpdateGraphType = eventType.JUMPREACTIVE;
				needUpdateGraph = true;



				//try to fix this:
				//http://mail.gnome.org/archives/chronojump-list/2007-June/msg00013.html
							updateProgressBar= new UpdateProgressBar (
									true, //isEvent
									jumpsLimited, //if jumpsLimited: do fraction; if time limited: do pulse
									 Util.GetNumberOfJumps(tvString, false)
									);  
							needUpdateEventProgressBar = true;
				//and this:
				//http://mail.gnome.org/archives/chronojump-list/2007-June/msg00017.html
							updateTimerCountWithChronopicData(tcString, tvString);							
							
			}

			jumps = Util.GetNumberOfJumps(tvString, false);

					
			if(jumpsLimited) {
				limitString = jumps.ToString() + "J";
			} else {
				limitString = Util.GetTotalTime(tcString, tvString) + "T";
				//commented because it was assigned but never used
				//limited = limitString; //define limited because it's checked in treeviewJump, and possibly it's not the initial defined time (specially when allowFinishRjAfterTime is true)

				//leave the initial selected time into description/comments:
				description = string.Format(Catalog.GetString("Initially selected {0} seconds"), limitAsDouble.ToString());
			}
		} else {
			if(jumpsLimited) {
				limitString = limitAsDouble.ToString() + "J";
				jumps = (int) limitAsDouble;
			} else {
				//if time finished and the last event was tc, probably there are more TCs than TFs
				//if last event was tc, it has no sense, it should be deleted
				//this is not aplicable in tempTable
				if(! tempTable)
					tcString = Util.DeleteLastTcIfNeeded(tcString, tvString);
				
				//limitString = limitAsDouble.ToString() + "T";
				limitString = Util.GetTotalTime(tcString, tvString) + "T";

				//commented because it was assigned but never used
				//limited = limitString; //define limited because it's checked in treeviewJump, and possibly it's not the initial defined time (specially when allowFinishRjAfterTime is true)

				//leave the initial selected time into description/comments:
				description = string.Format(Catalog.GetString("Initially selected {0} seconds"), limitAsDouble.ToString());

				string [] myStringFull = tcString.Split(new char[] {'='});
				jumps = myStringFull.Length;
			}
		}

		if(type == Constants.RunAnalysisName) {
			//double speed = (fall /10) / Util.GetTotalTime(tcString, tvString);
	
	/*		
	 *		Josep Ma Padullés test
	 *
			string tcStringWithoutFirst = Util.DeleteFirstSubEvent(tcString);
			string tvStringWithoutFirst = Util.DeleteFirstSubEvent(tvString);
		
			double averagePlatformTimes = ( Util.GetAverage(tcStringWithoutFirst) + Util.GetAverage(tvStringWithoutFirst) ) / 2;
			double freq = 1 / averagePlatformTimes;

			//amplitud
			double range = speed / freq;
			
			//don't put "=" because can appear problems in different parts of the code
			description = 
				Catalog.GetString ("AVG speed") + "->" + Util.TrimDecimals(speed.ToString(), pDN) + "m/s, " +
				Catalog.GetString ("AVG frequencies") + "->" + Util.TrimDecimals(freq.ToString(), pDN) + "Hz, " +
				Catalog.GetString ("AVG range") + "->" + Util.TrimDecimals(range.ToString(), pDN) + "m.";
				*/
		}

		string datetime = UtilDate.ToFile(DateTime.Now);

		if(tempTable) 
			SqliteJumpRj.Insert(false, Constants.TempJumpRjTable, "NULL", personID, sessionID, 
					type, Util.GetMax(tvString), Util.GetMax(tcString), 
					fall, weight, description,
					Util.GetAverage(tvString), Util.GetAverage(tcString),
					tvString, tcString,
					jumps, Util.GetTotalTime(tcString, tvString), limitString, angleString, Util.BoolToNegativeInt(simulated),
					datetime,
					UtilList.GetAverage (JumpRj.HeightListFromTvString (tvString)));
		else {
			if(simulated)
				feedbackMessage = Catalog.GetString(Constants.SimulatedMessage());
			else
				feedbackMessage = "";

			uniqueID = SqliteJumpRj.Insert(false, Constants.JumpRjTable, "NULL", personID, sessionID, 
					type, Util.GetMax(tvString), Util.GetMax(tcString), 
					fall, weight, description,
					Util.GetAverage(tvString), Util.GetAverage(tcString),
					tvString, tcString,
					jumps, Util.GetTotalTime(tcString, tvString), limitString, angleString, Util.BoolToNegativeInt(simulated),
					datetime,
					UtilList.GetAverage (JumpRj.HeightListFromTvString (tvString)));

			//define the created object
			eventDone = new JumpRj(uniqueID, personID, sessionID, type, tvString, tcString, fall, weight, description, jumps, Util.GetTotalTime(tcString, tvString), limitString, angleString, Util.BoolToNegativeInt(simulated), datetime);

			if(upload)
			{
				/* debug
				LogB.Information ("upload will start");
				LogB.Information (string.Format (
							"uploadStationId: {0}, (JumpRj) eventDone {1}, typeID {2}, personWeight {3}, metersSecondsPreferred {4}",
							uploadStationId, (JumpRj) eventDone, typeID, personWeight, metersSecondsPreferred));
				 */

				UploadJumpReactiveDataObject uj = new UploadJumpReactiveDataObject (
						uploadStationId, (JumpRj) eventDone, typeID, personWeight, metersSecondsPreferred);
				LogB.Information ("uj: " + uj.ToString ());

				JsonCompujump js = new JsonCompujump (django);
				if( ! js.UploadJumpData (uj, Constants.Modes.JUMPSREACTIVE) )
				{
					LogB.Error (js.ResultMessage);

					feedbackMessageOnDialog = true;
					feedbackMessage = js.ResultMessage;
				}
			}

			needShowFeedbackMessage = true; 

			needEndEvent = true; //used for hiding some buttons on eventWindow, and also for updateTimeProgressBar here
		}
	}

	public override bool TypeHasWeight
	{
		get { return SqliteJumpType.HasWeight("jumpRjType", type); }
	}
	

	public override bool TypeHasFall
	{
		get { return SqliteJumpType.HasFall("jumpRjType", type); } //jumpRjType is the table name
	}


	~JumpRjExecute() {}
}
