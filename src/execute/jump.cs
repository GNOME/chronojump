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

public class JumpExecute : EventExecute
{
	protected double personWeight;
	protected double tv;
	protected double tc;
	protected double fall;
	protected double weight; //always write in % (not kg or %) then sqlite can do avgs

	//for not checking always in database
	protected bool hasFall;

	//better as private and don't inherit, don't know why
	//protected Chronopic cp;
	private Chronopic cp;
	
	//used by the updateTimeProgressBar for display its time information
	//copied from execute/run.cs 
	protected enum jumpPhases {
		PERSON_OUT_BUT_NEED_TO_START_IN, // when should start in and person is out
		PERSON_IN_BUT_NEED_TO_START_OUT, // when should start out and person is in
		PRE_OR_DOING,
		PLATFORM_END
	}
	protected static jumpPhases jumpPhase;
	protected static JumpChangeImage jumpChangeImage;

	private int angle = -1;
	//private bool avoidGraph;
	//private bool heightPreferred;
	protected bool metersSecondsPreferred;
	protected Gtk.Image image_jump_execute_air;
	protected Gtk.Image image_jump_execute_land;
	protected bool upload;
	protected int uploadStationId;
	protected bool django;
	private bool jsonUploadNeedsButton;
	private string jsonUploadTestScript;

	public JumpExecute() {
	}

	//jump execution
	public JumpExecute (
			int personID, string personName, double personWeight,
			int sessionID, int typeID, string type, double fall, double weight,
			Chronopic cp, int pDN,
			bool volumeOn, Preferences.GstreamerTypes gstreamer,
			double progressbarLimit, ExecutingGraphData egd, string description,
			//bool avoidGraph, //on configChronojump.Exhibition do not show graph because it gets too slow with big database
			//bool heightPreferred,
			bool metersSecondsPreferred,
			int graphLimit, bool graphAllTypes, bool graphAllPersons,
			Gtk.Image image_jump_execute_air, Gtk.Image image_jump_execute_land,
			bool upload, int uploadStationId, bool django, //upload: configChronojump.Compujump && upload (contacts) button active
			bool cameraRecording,
			bool jsonUploadNeedsButton,
			string jsonUploadTestScript
			)
	{
		this.personID = personID;
		this.personName = personName;
		this.personWeight = personWeight; //for Stiffness at upload on compujump
		this.sessionID = sessionID;
		this.typeID = typeID;
		this.type = type;
		this.fall = fall; //-1 means has to be calculated with a previous jump
		this.weight = weight;
		
		this.cp = cp;

		this.pDN = pDN;
		this.volumeOn = volumeOn;
		this.gstreamer = gstreamer;
		this.progressbarLimit = progressbarLimit;
		this.egd = egd;
		this.description = description;
		//this.avoidGraph = avoidGraph;
		//this.heightPreferred = heightPreferred;
		this.metersSecondsPreferred = metersSecondsPreferred;
		this.graphLimit = graphLimit;
		this.graphAllTypes = graphAllTypes;
		this.graphAllPersons = graphAllPersons;
		this.image_jump_execute_air = image_jump_execute_air;
		this.image_jump_execute_land = image_jump_execute_land;
		this.upload = upload;
		this.uploadStationId = uploadStationId;
		this.django = django;
		this.cameraRecording = cameraRecording;
		this.jsonUploadNeedsButton = jsonUploadNeedsButton;
		this.jsonUploadTestScript = jsonUploadTestScript;

		if(TypeHasFall) {
			hasFall = true;
		} else {
			hasFall = false;
		}

		fakeButtonUpdateGraph = new Gtk.Button();
		fakeButtonCameraStopIfNeeded = new Gtk.Button ();
		fakeButtonThreadDyed = new Gtk.Button();

		simulated = false;
			
		needUpdateEventProgressBar = false;
		needUpdateGraph = false;
	
		//initialize eventDone as a Jump		
		eventDone = new Jump();
	}

	public override void SimulateInitValues(Random randSent)
	{
		LogB.Information ("From execute/jump.cs");

		rand = randSent; //we send the random, because if we create here, the values will be the same for each nbew instance
		simulated = true;
		simulatedTimeAccumulatedBefore = 0;
		simulatedTimeLast = 0;
		simulatedContactTimeMin = 0.2; //seconds
		simulatedContactTimeMax = 0.37; //seconds
		simulatedFlightTimeMin = 0.4; //seconds
		simulatedFlightTimeMax = 0.7; //seconds

		if(hasFall) {
			//values of simulation will be the contactTime
			//at the first time, the second will be flightTime
			simulatedCurrentTimeIntervalsAreContact = true;
		} else {
			//values of simulation will be the flightTime
			//at the first time (and the only)
			simulatedCurrentTimeIntervalsAreContact = false;
		}
	}
	
	public override bool Manage()
	{
		LogB.Information("Jumps Manage!");
		//boolean to know if chronopic has been disconnected	
		chronopicDisconnected = false;
		jumpChangeImage = new JumpChangeImage();

		if (simulated) 
			platformState = Chronopic.Plataforma.ON;
		else
			platformState = chronopicInitialValue(cp);
		
		
		//UNKNOW (Chronopic disconnected, port changed, ...)
		if (platformState != Chronopic.Plataforma.ON &&
					platformState != Chronopic.Plataforma.OFF)
		{
			jumpChangeImageDo (platformState);
			chronopicHasBeenDisconnected();

			return false;
		}

		jumpChangeImageDo (platformState); //done here before thread starts

		if (platformState == Chronopic.Plataforma.ON)
		{
			feedbackMessage = Catalog.GetString ("You are IN, JUMP when prepared!");
			needShowFeedbackMessage = true; 
			Util.PlaySound(Constants.SoundTypes.CAN_START, volumeOn, gstreamer);

			loggedState = States.ON;
			jumpPhase = jumpPhases.PRE_OR_DOING;
	
			//in simulated mode, make the jump start just when we arrive to waitEvent at the first time
			//mark now that we have leaved platform:
			if (simulated)
				platformState = Chronopic.Plataforma.OFF;
		} 
		else if (platformState == Chronopic.Plataforma.OFF)
		{
			feedbackMessage = Catalog.GetString ("You are OUT, please enter the platform, and then jump when prepared!");
			needShowFeedbackMessage = true;
			loggedState = States.OFF;
			jumpPhase = jumpPhases.PERSON_OUT_BUT_NEED_TO_START_IN;
		}

		cancel = false; 	//prepare jump for being cancelled if desired

		thread = new Thread(new ThreadStart(waitEvent)); 	//start thread
		GLib.Idle.Add (new GLib.IdleHandler (PulseGTK));

		LogB.ThreadStart();
		thread.Start();

		return true;
	}

	public override bool ManageFall()
	{
		LogB.Information ("Jumps ManageFall!, fall: ", fall.ToString ());
		//boolean to know if chronopic has been disconnected	
		chronopicDisconnected = false;
		jumpChangeImage = new JumpChangeImage();

		if (simulated) {
			if(fall != -1)
				platformState = Chronopic.Plataforma.OFF;
			else
				platformState = Chronopic.Plataforma.ON;
		}
		else
			platformState = chronopicInitialValue(cp);



		if (platformState != Chronopic.Plataforma.OFF &&
				platformState != Chronopic.Plataforma.ON) 
		{
			//UNKNOW (Chronopic disconnected, port changed, ...)
			jumpChangeImageDo (platformState);

			chronopicHasBeenDisconnected();
			return false;
		}
		
		jumpChangeImageDo (platformState);

		//useful also for tracking the jump phases
		tc = 0;

		//if we are outside
		//or we are inside, but with fall == -1 (calculate fall using a previous jump (start inside))
		if (
				( platformState == Chronopic.Plataforma.OFF && fall != -1 ) ||
				( platformState == Chronopic.Plataforma.ON  && fall == -1 )
				) 
		{
			if(fall != -1) {
				feedbackMessage = Catalog.GetString ("You are OUT, JUMP when prepared!");
				loggedState = States.OFF;
			} else {
				feedbackMessage = Catalog.GetString ("You are IN, JUMP when prepared!");
				loggedState = States.ON;
			}

			Util.PlaySound(Constants.SoundTypes.CAN_START, volumeOn, gstreamer);
			jumpPhase = jumpPhases.PRE_OR_DOING;

			//in simulated mode, make the jump start just when we arrive to waitEvent at the first time
			if (simulated) {
				if(fall != -1)
					platformState = Chronopic.Plataforma.ON; //mark now that we have arrived:
				else
					platformState = Chronopic.Plataforma.OFF; //mark now that we have jumped
			}
		} 
		else  {
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
		}

		needShowFeedbackMessage = true;

		cancel = false; 	//prepare jump for being cancelled if desired

		thread = new Thread(new ThreadStart(waitEvent)); 	//start thread
		GLib.Idle.Add (new GLib.IdleHandler (PulseGTK));

		LogB.ThreadStart();
		thread.Start();

		return true;
	}
	
	//for calling it again after a confirmWindow says that you have to be in or out the platform
	//and press ok button
	//This method is for not having problems with the parameters of the delegate
	private void callAgainManageFall(object o, EventArgs args) {
		ManageFall();
	}

	//before thread start, to set the image
	protected void jumpChangeImageDo (Chronopic.Plataforma plat)
	{
		if (plat == Chronopic.Plataforma.OFF)
			jumpChangeImage.Current = JumpChangeImage.Types.AIR;
		else if (plat == Chronopic.Plataforma.ON)
			jumpChangeImage.Current = JumpChangeImage.Types.LAND;
		else
			jumpChangeImage.Current = JumpChangeImage.Types.NONE;

		jumpChangeImageIfNeeded ();
	}

	protected override void jumpChangeImageIfNeeded ()
	{
		if(! jumpChangeImage.ShouldBeChanged())
			return;

		if (jumpChangeImage.Current == JumpChangeImage.Types.AIR)
		{
			image_jump_execute_air.Visible = true;
			image_jump_execute_land.Visible = false;
		} else if (jumpChangeImage.Current == JumpChangeImage.Types.LAND)
		{
			image_jump_execute_air.Visible = false;
			image_jump_execute_land.Visible = true;
		} else
		{ //UNKNOW (Chronopic disconnected, port changed, ...)
			image_jump_execute_air.Visible = false;
			image_jump_execute_land.Visible = false;
		}
	}

	protected override void jumpChangeImageForceHide()
	{
		image_jump_execute_air.Visible = false;
		image_jump_execute_land.Visible = false;
	}

	protected override void waitEvent ()
	{
		double timestamp = 0;
		bool success = false;
		
		bool ok;
		int phase = 0;

		//prepare variables to allow being cancelled or finished
		if(! simulated)
			Chronopic.InitCancelAndFinish();

		do {
			if(simulated)
				ok = true;
			else
			{
				LogB.Information("calling Read_event");
				ok = cp.Read_event(out timestamp, out platformState);
				LogB.Information("Read_event done!");
			}


			/*
			 *           \()/            \()/
			 *            \/              \/
			 *   _()_     /\     _()_     /\     _()_
			 *    \/              \/              \/
			 * ___/\______________/\______________/\___ 
			 *
			 *  GraphA  graphB  graphC  graphD  graphE
			 *  unused  jumps   lands   jumps   lands
			 *
			 *    ______start_______             end 
			 *
			 *    DJ      DJ      SJ
			 * hasFall  hasFall
			 * fall -1
			 *
			 */
			
			//if (ok) 
			if (ok && ! cancel)
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

				if (platformState == Chronopic.Plataforma.ON && loggedState == States.OFF) 
				{
					//has landed
					jumpChangeImage.Current = JumpChangeImage.Types.LAND;

					if(hasFall && tc == 0) 
					{
						//**** graphC **** 

						if(fall == -1) {
							if(simulated)
								timestamp = simulatedTimeLast * 1000; //conversion to milliseconds

							//calculate the fall height using flight time
							double tvPreJump = timestamp / 1000.0;
							fall = Convert.ToDouble(Util.GetHeightInCentimeters(tvPreJump.ToString()));
						}

						//jump with fall, landed first time
						initializeTimer();

						//app1.ProgressBarEventOrTimePreExecution(
						//don't do it, put a boolean value and let the PulseGTK do it
						updateProgressBar = new UpdateProgressBar (
								true, //isEvent
								true, //jumpsLimited: percentageMode
								++phase
								);
						needUpdateEventProgressBar = true;
		
						feedbackMessage = "";
						needShowFeedbackMessage = true;

					} else {
						//**** graphE **** jump with fall: second landed; or without fall first landing
					
						if(simulated)
							timestamp = simulatedTimeLast * 1000; //conversion to milliseconds
						
						LogB.Information(string.Format("t1:{0}", timestamp));

						tv = timestamp / 1000.0;
					
						jumpPhase = jumpPhases.PLATFORM_END;
						
						write();

						success = true;
						
						//app1.ProgressBarEventOrTimePreExecution(
						//don't do it, put a boolean value and let the PulseGTK do it
						updateProgressBar = new UpdateProgressBar (
								true, //isEvent
								true, //percentageMode
								++phase
								);
						needUpdateEventProgressBar = true;
					}
					loggedState = States.ON;
				}
				else if (platformState == Chronopic.Plataforma.OFF && loggedState == States.ON) 
				{
					//it's out, was inside (= has jumped)
					jumpChangeImage.Current = JumpChangeImage.Types.AIR;
				
					//fall != -1 because if it was == -1, it will change once touching floor for the first time	
					if(hasFall && fall != -1) {
						//**** graphD **** 

						if(simulated)
							timestamp = simulatedTimeLast * 1000; //conversion to milliseconds
						
						LogB.Information(string.Format("t2:{0}", timestamp));
						
						//record the TC
						tc = timestamp / 1000.0;
						
						//takeOff jump (only one TC)
						//if(fixedValue == 0.5) 
						if(type == Constants.TakeOffName || type == Constants.TakeOffWeightName) {
							tv = 0;
						
							jumpPhase = jumpPhases.PLATFORM_END;
							
							write();
							success = true;
						}

						//update event progressbar
						//app1.ProgressBarEventOrTimePreExecution(
						//don't do it, put a boolean value and let the PulseGTK do it
						updateProgressBar = new UpdateProgressBar (
								true, //isEvent
								true, //percentageMode
								++phase
								);
						needUpdateEventProgressBar = true;
					} else {
						//**** graphD (if simple jump) ****
						//**** graphB (if hasFall and fall == -1) **** 

						initializeTimer();
						
						//update event progressbar
						//app1.ProgressBarEventOrTimePreExecution(
						//don't do it, put a boolean value and let the PulseGTK do it
						updateProgressBar = new UpdateProgressBar (
								true, //isEvent
								true, //percentageMode
								++phase
								);
						needUpdateEventProgressBar = true;
						
						feedbackMessage = "";
						needShowFeedbackMessage = true; 
					}

					//change the automata state
					loggedState = States.OFF;
				}
			}
		} while ( ! success && ! cancel );

		LogB.Information("Exited waitEvent main bucle");
	}
	
	protected override bool shouldFinishByTime() {
		return false; //this kind of events (simple or Dj jumps) cannot be finished by time
	}
	
	protected override void write()
	{
		// string tcString = "";
		if(hasFall) {
			//Log.WriteLine("TC: {0}", tc.ToString());
			// tcString = " " + Catalog.GetString("TC") + ": " + Util.TrimDecimals( tc.ToString(), pDN ) ;
		} else {
			tc = 0;
		}

		
		/*	
		string myStringPush =   
			personName + " " + 
			type + tcString + " " + Catalog.GetString("TF") + ": " + Util.TrimDecimals( tv.ToString(), pDN ) ;
		if(weight > 0) {
			myStringPush = myStringPush + "(" + weight.ToString() + "%)";
		}
		*/
		if(simulated)
			feedbackMessage = Catalog.GetString(Constants.SimulatedMessage());
		else
			feedbackMessage = "";

		string table = Constants.JumpTable;
		string datetime = UtilDate.ToFile(DateTime.Now);

		uniqueID = SqliteJump.Insert(false, table, "NULL", personID, sessionID, 
				type, tv, tc, fall,  //type, tv, tc, fall
				weight, description, angle, Util.BoolToNegativeInt(simulated),
				datetime);

		//define the created object
		eventDone = new Jump(uniqueID, personID, sessionID, type, tv, tc, fall, 
				weight, description, angle, Util.BoolToNegativeInt(simulated), datetime);

		// upload with json but not to networks
		if (! jsonUploadNeedsButton)
		{
			if (jsonUploadTestScript != "")
				JsonUploadTestScriptDo ();
		}

		if(upload) //networks
		{
			UploadJumpSimpleDataObject uj = new UploadJumpSimpleDataObject (
					uploadStationId, (Jump) eventDone, typeID, personWeight, metersSecondsPreferred);
			JsonCompujump js = new JsonCompujump (django);
			if( ! js.UploadJumpData (uj, Constants.Modes.JUMPSSIMPLE) )
			{
				LogB.Error (js.ResultMessage);

				/*
				   feedbackMessage will be shown on a DialogMessage to not being erased by updateGraphJumpsSimple -> event_execute_initializeVariables
				   the dialog cannot be called here to avoid gtk crash
				   */
				feedbackMessageOnDialog = true;
				feedbackMessage = js.ResultMessage;

				//since 2.1.3 do not store in Temp, if there are network errors, it is not going to be uploaded later, because wristbands can be re-assigned
				//SqliteJson.InsertTempSprint(false, usdo); //insert only if couldn't be uploaded
			}
		}
		needShowFeedbackMessage = true;

		/* 2.2.2 do not do the graph here because PrepareEventGraphJumpSimple has an SQL call with a reader
		   and updateGraph can be also called by gtk thread and also call PrepareEventGraphJumpSimple,
		   so SQL can be tried to open again, but the problem is in reader that if both run at same time it will crash (seen a log on 2.2.1)
		   Note on_jump_finished (main thread) also calls updateGraphJumpsSimple(); so graph will be updated at end
		   Note also the PrepareEventGraphJumpReactiveRealtimeCaptureObject has no SQL calls, and the PrepareEventGraphJumpReactive is not called while capture

		if(! avoidGraph)
		{
			if(graphAllTypes)
				type = "";

			PrepareEventGraphJumpSimpleObject = new PrepareEventGraphJumpSimple(
					tv, tc, sessionID,
					personID, graphAllPersons, graphLimit,
					table, type, heightPreferred);

			needUpdateGraphType = eventType.JUMP;
			needUpdateGraph = true;
		}
		*/
		
		needEndEvent = true; //used for hiding some buttons on eventWindow
	}

	public void JsonUploadTestScriptDo ()
	{
		Person p = SqlitePerson.Select (false, personID);
		writeJsonDataThisTest (p);
		System.Threading.Thread.Sleep(250);
		ExecuteProcess.run (jsonUploadTestScript, false, false);
	}
	private void writeJsonDataThisTest (Person p)
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
			"\"No\":\"(" + p.ClubID + ")\",\n" +
			"\"Photo\":\"" + description + "\",\n" +
			"\"Test\":\"Jump\",\n" +
			"\"TestType\":\"" + type + "\",\n" +
			"\"JumpHeightCm\":" + Util.ConvertToPoint (Util.TrimDecimals (
						Util.GetHeightInCm (tv), 2)) + "\n" +
			"}";

		TextWriter writer = File.CreateText("/tmp/chronojump_json_jump_1_test.txt");
		writer.Write(jsonStr);
		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();
	}

	protected override void updateTimeProgressBar() {
		if(jumpPhase == jumpPhases.PLATFORM_END)
			return;

		//until it has not landed for first time, show a pulse with no values
		progressBarEventOrTimePreExecution(
				false, //isEvent false: time
				false, //activity mode
				-1	//don't want to show info on label
				); 
	}
	
	public virtual bool TypeHasWeight
	{
		get { return SqliteJumpType.HasWeight("jumpType", type); }
	}
	
	public virtual bool TypeHasFall
	{
		get { return SqliteJumpType.HasFall("jumpType", type); } //jumpType is the table name
	}
	
	/*
	public string JumperName
	{
		get { return SqlitePerson.SelectJumperName(personID); }
	}
	*/

	~JumpExecute() {}
	   
}
