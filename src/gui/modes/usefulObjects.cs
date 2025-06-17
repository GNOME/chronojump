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
using System.Diagnostics; //Stopwatch
using Gtk;
using System.Collections; //ArrayList
using System.Collections.Generic; //List<T>

//this file has classes to allow to pass gui objectes easily
public class ExecutingGraphData
{
	public Gtk.Button Button_cancel;
	public Gtk.Button Button_finish;
	public Gtk.Label Label_message;
	public Gtk.Label Label_event_value;
	public Gtk.Label Label_time_value;
	public Gtk.Label Label_video_feedback;
	public Gtk.ProgressBar Progressbar_event;
	public Gtk.ProgressBar Progressbar_time;
	
	public ExecutingGraphData(
			Gtk.Button Button_cancel, Gtk.Button Button_finish, 
			Gtk.Label Label_message,
			Gtk.Label Label_event_value, Gtk.Label Label_time_value,
			Gtk.Label Label_video_feedback,
			Gtk.ProgressBar Progressbar_event, Gtk.ProgressBar Progressbar_time) 
	{
		this.Button_cancel =  Button_cancel;
		this.Button_finish =  Button_finish;
		this.Label_message =  Label_message;
		this.Label_event_value =  Label_event_value;
		this.Label_time_value =  Label_time_value;
		this.Label_video_feedback = Label_video_feedback;
		this.Progressbar_event =  Progressbar_event;
		this.Progressbar_time =  Progressbar_time;
	}

	public ExecutingGraphData() {
	}
}	

public class PrepareEventGraphJumpSimple
{
	//sql data of previous jumps to plot graph and show stats at bottom
	public List<Jump> jumpsAtSQL;
	
	public double personMAXAtSQLAllSessions;
	public double personMAXAtSQL;
	public double sessionMAXAtSQL;

	public double personAVGAtSQL;
	public double sessionAVGAtSQL;

	public double personMINAtSQL;
	public double sessionMINAtSQL;

	//current data
	public double tv;
	public double tc;
	public string type; //jumpType (useful to know if "all jumps" (type == "")
	public bool djShowHeights; //if djShowHeights and is a dj, graph falling height and jump height
	public int selectedID; //-1 if none selected. If >= 0 then is the selected on treeview.

	public PrepareEventGraphJumpSimple() {
	}

	//allPersons is for searching the jumps of current of allpersons
	//personID we need to the personsMAX/AVG sql calls
	//type can be "" for all jumps, then write it under bar
	public PrepareEventGraphJumpSimple (double tv, double tc, int sessionID,
			int personID, bool allPersons, bool showBest, int limit,
			string table, string type, bool djShowHeights, int selectedID)
	{
		int personIDTemp = personID;
		if(allPersons)
			personIDTemp = -1;

		Sqlite.Orders_by orderBy = Sqlite.Orders_by.BEST;
		if (! showBest)
			orderBy = Sqlite.Orders_by.ID_ASC;

		jumpsAtSQL = SqliteJump.SelectJumps (sessionID, personIDTemp, type,
				orderBy, limit,
				allPersons, 	//show names on comments only if "all persons"
				false); 	//! onlyBestInSession

		Sqlite.Open();


		string sqlSelect = "";
		//if it is a concrete jump type, then check if showHeights or times
		if(type != "") {
			if(tv > 0) {
				if(tc <= 0)
					sqlSelect = "100*4.9*(TV/2)*(TV/2)";
				else {
					if(djShowHeights)
						sqlSelect = "100*4.9*(TV/2)*(TV/2)";
					else
						sqlSelect = "TV"; //if tc is higher than tv it will be fixed on PrepareJumpSimpleGraph
				}
			} else
				sqlSelect = "TC";
		} else {
			//if there are different types, always use heights to be able to do comparisons between different jump types
			sqlSelect = "100*4.9*(TV/2)*(TV/2)";
		}

		personMAXAtSQLAllSessions = SqliteSession.SelectMAXEventsOfAType(true, -1, personID, table, type, sqlSelect);

		List<double> personStats = SqliteSession.Select_MAX_AVG_MIN_EventsOfAType(
				true, sessionID, personID, table, type, sqlSelect);
		personMAXAtSQL = personStats[0];
		personAVGAtSQL = personStats[1];
		personMINAtSQL = personStats[2];

		List<double> sessionStats = SqliteSession.Select_MAX_AVG_MIN_EventsOfAType(
				true, sessionID, -1, table, type, sqlSelect);
		sessionMAXAtSQL = sessionStats[0];
		sessionAVGAtSQL = sessionStats[1];
		sessionMINAtSQL = sessionStats[2];
	
		//end of select data from SQL to update graph	
			
		this.tv = tv;
		this.tc = tc;
		this.type = type;
		this.djShowHeights = djShowHeights;
		this.selectedID = selectedID;
		
		Sqlite.Close();
	}

	~PrepareEventGraphJumpSimple() {}
}

public class PrepareEventGraphJumpReactive
{
	//sql data of previous jumps to plot graph and show stats at bottom
	public List<JumpRj> jumpsAtSQL;
	public string type; //jumpType (useful to know if "all jumps" (type == "")

	public double personMAXAtSQL;
	public double sessionMAXAtSQL;

	public double personAVGAtSQL;
	public double sessionAVGAtSQL;

	public double personMINAtSQL;
	public double sessionMINAtSQL;
	public bool showHeights; //if showHeights graph falling height and jump height
	public int selectedID; //-1 if none selected. If >= 0 then is the selected on treeview.

	public PrepareEventGraphJumpReactive () {
	}

	//allPersons is for searching the jumps of current of allpersons
	//personID we need to the personsMAX/AVG sql calls
	//type can be "" for all jumps, then write it under bar
	public PrepareEventGraphJumpReactive (
			int sessionID, int personID, bool allPersons, int limit, string type, bool showHeights, int selectedID)
	{
		// 1) assign variables
		this.type = type;
		this.selectedID = selectedID;
		this.showHeights = showHeights;

		Sqlite.Open(); // ----------------->

		int personIDTemp = personID;
		if(allPersons)
			personIDTemp = -1;

		jumpsAtSQL = SqliteJumpRj.SelectJumps (true, sessionID, personIDTemp, type,
				Sqlite.Orders_by.ID_ASC, limit, allPersons); 	//show names on comments only if "all persons"


		//as height is quadratic vs tv, we need to calculate height of each of the subjumps, cannot do it directly from sql (as its an string)
		if (showHeights)
		{
			List<JumpRj> jumpsAtSQLWithoutLimit = SqliteJumpRj.SelectJumps (true, sessionID, personIDTemp, type,
					Sqlite.Orders_by.ID_ASC, 0, allPersons); 	//show names on comments only if "all persons"

			//note falls should be also counted, but all falls are just heights except the last one.
			//TODO: and we need to add first fall (selected from the software) (if is > 0)
			List<double> personHeights_l = new List<double> ();
			List<double> sessionHeights_l = new List<double> ();

			foreach (JumpRj jumpRj in jumpsAtSQLWithoutLimit)
			{
				double heightAvg = UtilList.GetAverage (jumpRj.HeightList);
				if (jumpRj.PersonID == personIDTemp)
					personHeights_l.Add (heightAvg);

				sessionHeights_l.Add (heightAvg);
			}

			personMAXAtSQL = UtilList.GetMax (personHeights_l);
			personAVGAtSQL = UtilList.GetAverage (personHeights_l);
			personMINAtSQL = UtilList.GetMin (personHeights_l);

			sessionMAXAtSQL = UtilList.GetMax (sessionHeights_l);
			sessionAVGAtSQL = UtilList.GetAverage (sessionHeights_l);
			sessionMINAtSQL = UtilList.GetMin (sessionHeights_l);
		}
		else
		{
			// sum of each subjump
			//string sqlSelect = "tvAvg*jumps";
			// avg of each subjump
			string sqlSelect = "tvAvg";

			string table = Constants.JumpRjTable;

			List<double> personStats = SqliteSession.Select_MAX_AVG_MIN_EventsOfAType(
					true, sessionID, personID, table, type, sqlSelect);
			personMAXAtSQL = personStats[0];
			personAVGAtSQL = personStats[1];
			personMINAtSQL = personStats[2];

			List<double> sessionStats = SqliteSession.Select_MAX_AVG_MIN_EventsOfAType(
					true, sessionID, -1, table, type, sqlSelect);
			sessionMAXAtSQL = sessionStats[0];
			sessionAVGAtSQL = sessionStats[1];
			sessionMINAtSQL = sessionStats[2];
		}

		Sqlite.Close(); // < -----------------
	}

	~PrepareEventGraphJumpReactive () {}
}

public class PrepareEventGraphJumpReactiveRealtimeCapture
{
	public double lastTv;
	public double lastTc;
	public string tvString;
	public string tcString;
	public string type;

	public PrepareEventGraphJumpReactiveRealtimeCapture () {
	}

	public PrepareEventGraphJumpReactiveRealtimeCapture (double lastTv, double lastTc, string tvString, string tcString, string type) {
		this.lastTv = lastTv;
		this.lastTc = lastTc;
		this.tvString = tvString;
		this.tcString = tcString;
		this.type = type;
	}

	~PrepareEventGraphJumpReactiveRealtimeCapture () {}
}

public class PrepareEventGraphRunSimple
{
	//sql data of previous runs to plot graph and show stats at bottom
	public List<Run> runsAtSQL;
	
	public double personMAXAtSQLAllSessions;
	public double personMAXAtSQL;
	public double sessionMAXAtSQL;

	public double personAVGAtSQL;
	public double sessionAVGAtSQL;

	public double personMINAtSQL;
	public double sessionMINAtSQL;

	public double time;
	public double speed;
	public string type; //jumpType (useful to know if "all jumps" (type == "")
	public int selectedID; //-1 if none selected. If >= 0 then is the selected on treeview.

	public PrepareEventGraphRunSimple() {
	}

	public PrepareEventGraphRunSimple(double time, double speed, int sessionID,
			int personID, bool allPersons, bool showBest, int limit,
			string table, string type, int selectedID)
	{
		Sqlite.Open();
		
		int personIDTemp = personID;
		if(allPersons)
			personIDTemp = -1;

		Sqlite.Orders_by orderBy = Sqlite.Orders_by.BEST;
		if (! showBest)
			orderBy = Sqlite.Orders_by.ID_ASC;

		//obtain data
		runsAtSQL = SqliteRun.SelectRuns (true, sessionID, personIDTemp, type,
				orderBy, limit,
				allPersons, false); //show names on comments only if "all persons"

		
		string sqlSelect = "distance/time";
		//better to know speed like:
		//SELECT AVG(distance/time) from run; than 
		//SELECT AVG(distance) / SELECT AVG(time) 
		//first is ok, because is the speed AVG
		//2nd is not good because it tries to do an AVG of all distances and times
		
		personMAXAtSQLAllSessions = SqliteSession.SelectMAXEventsOfAType(true, -1, personID, table, type, sqlSelect); //right now, used only on the not-cairo solution

		List<double> personStats = SqliteSession.Select_MAX_AVG_MIN_EventsOfAType(
				true, sessionID, personID, table, type, sqlSelect);
		personMAXAtSQL = personStats[0];
		personAVGAtSQL = personStats[1];
		personMINAtSQL = personStats[2];

		List<double> sessionStats = SqliteSession.Select_MAX_AVG_MIN_EventsOfAType(
				true, sessionID, -1, table, type, sqlSelect);
		sessionMAXAtSQL = sessionStats[0];
		sessionAVGAtSQL = sessionStats[1];
		sessionMINAtSQL = sessionStats[2];

		this.time = time;
		this.speed = speed;
		this.type = type;
		this.selectedID = selectedID;
		
		Sqlite.Close();
	}

	~PrepareEventGraphRunSimple() {}
}

public class PrepareEventGraphRunInterval
{
	//sql data of previous jumps to plot graph and show stats at bottom
	public List<RunInterval> runsAtSQL;
	public string type; //jumpType (useful to know if "all jumps" (type == "")

	public double personMAXAtSQL;
	public double sessionMAXAtSQL;

	public double personAVGAtSQL;
	public double sessionAVGAtSQL;

	public double personMINAtSQL;
	public double sessionMINAtSQL;
	public int selectedID; //-1 if none selected. If >= 0 then is the selected on treeview.

	public PrepareEventGraphRunInterval () {
	}

	//allPersons is for searching the jumps of current of allpersons
	//personID we need to the personsMAX/AVG sql calls
	//type can be "" for all jumps, then write it under bar
	public PrepareEventGraphRunInterval (
			int sessionID, int personID, bool allPersons, bool showBest, int limit,
			string type, int selectedID)
	{
		// 1) assign variables
		this.type = type;
		this.selectedID = selectedID;

		Sqlite.Open(); // ----------------->

		int personIDTemp = personID;
		if(allPersons)
			personIDTemp = -1;

		Sqlite.Orders_by orderBy = Sqlite.Orders_by.BEST;
		if (! showBest)
			orderBy = Sqlite.Orders_by.ID_ASC;

		runsAtSQL = SqliteRunInterval.SelectRuns (true, sessionID, personIDTemp, type,
				orderBy, limit, allPersons); 	//show names on comments only if "all persons"

		string sqlSelect = "distanceTotal/timeTotal";
		string table = Constants.RunIntervalTable;

		List<double> personStats = SqliteSession.Select_MAX_AVG_MIN_EventsOfAType(
				true, sessionID, personID, table, type, sqlSelect);
		personMAXAtSQL = personStats[0];
		personAVGAtSQL = personStats[1];
		personMINAtSQL = personStats[2];

		List<double> sessionStats = SqliteSession.Select_MAX_AVG_MIN_EventsOfAType(
				true, sessionID, -1, table, type, sqlSelect);
		sessionMAXAtSQL = sessionStats[0];
		sessionAVGAtSQL = sessionStats[1];
		sessionMINAtSQL = sessionStats[2];

		Sqlite.Close(); // < -----------------
	}

	~PrepareEventGraphRunInterval () {}
}

public class PrepareEventGraphRunIntervalRealtimeCapture
{
	public string type;
	public string timesString;
	public double distanceInterval; //we pass this because it's dificult to calculate in runs with variable distances
	public string distancesString; //we pass this because it's dificult to calculate in runs with variable distances
	public List<int> photocell_l; //for Wichro
	public bool startIn;
	public bool finished;

	public PrepareEventGraphRunIntervalRealtimeCapture() {
	}

	public PrepareEventGraphRunIntervalRealtimeCapture (string type,
			string timesString,
			double distanceInterval, string distancesString,
			List<int> photocell_l,
			bool startIn, bool finished)
	{
		this.type = type;
		this.timesString = timesString;
		this.distanceInterval = distanceInterval;
		this.distancesString = distancesString;
		this.photocell_l = photocell_l;
		this.startIn = startIn;
		this.finished = finished;
	}

	~PrepareEventGraphRunIntervalRealtimeCapture() {}
}

public class PrepareEventGraphRunEncoder
{
	//sql data of previous tests to plot graph and show stats at bottom
	public List<RunEncoder> rowsAtSQL;
	public int selectedID; //-1 if none selected. If >= 0 then is the selected on treeview.

	public bool exerciseAll; //all tests

	public PrepareEventGraphRunEncoder() {
	}

	public PrepareEventGraphRunEncoder (int sessionID, int personID, bool allPersons,
			Constants.ResultsSessionCriteria resultsSessionCriteria, int limit,
			int exerciseID, int selectedID, Constants.Modes mode, bool exerciseAll)
	{
		this.selectedID = selectedID;
		this.exerciseAll = exerciseAll;

		int personIDTemp = personID;
		if(allPersons)
			personIDTemp = -1;

		Sqlite.Orders_by orderBy = Sqlite.Orders_by.ID_ASC;
		if (resultsSessionCriteria == Constants.ResultsSessionCriteria.BEST)
			orderBy = Sqlite.Orders_by.BEST;
		else if (resultsSessionCriteria == Constants.ResultsSessionCriteria.BEST2)
			orderBy = Sqlite.Orders_by.BEST2;

		rowsAtSQL = SqliteRunEncoder.Select (false, -1, personIDTemp, sessionID, exerciseID,
				orderBy, limit,
				allPersons//, 	//show names on comments only if "all persons"
				//false 	//! onlyBestInSession
				);
		//LogB.Information ("rowsAtSQL count: " + (rowsAtSQL.Count).ToString ());

		this.selectedID = selectedID;
	}

	~PrepareEventGraphRunEncoder() {}
}

public class PrepareEventGraphWilight
{
	//sql data of previous jumps to plot graph and show stats at bottom
	public List<Wilight> rowsAtSQL;
	public int selectedID; //-1 if none selected. If >= 0 then is the selected on treeview.

	//public double lastTime;
	//public string timesString;
	public double time;

	public PrepareEventGraphWilight() {
	}

	public PrepareEventGraphWilight (double time, int sessionID, int personID, bool allPersons, int limit,
			int selectedID)
	{
		this.time = time;
		this.selectedID = selectedID;

		int personIDTemp = personID;
		if(allPersons)
			personIDTemp = -1;

		rowsAtSQL = SqliteWilight.Select (false, sessionID, personIDTemp, //type,
				Sqlite.Orders_by.ID_ASC, limit,
				allPersons//, 	//show names on comments only if "all persons"
				//false 	//! onlyBestInSession
				);
		LogB.Information ("rowsAtSQL count: " + (rowsAtSQL.Count).ToString ());
	}

	~PrepareEventGraphWilight() {}
}

public class PrepareEventGraphFourPlatforms
{
	//sql data of previous jumps to plot graph and show stats at bottom
	public List<FourPlatforms> rowsAtSQL;
	public int selectedID; //-1 if none selected. If >= 0 then is the selected on treeview.

	//public double lastTime;
	//public string timesString;
	public double time;

	public PrepareEventGraphFourPlatforms() {
	}

	public PrepareEventGraphFourPlatforms (double time, int sessionID, int personID, bool allPersons, int limit,
			int selectedID)
	{
		this.time = time;
		this.selectedID = selectedID;

		int personIDTemp = personID;
		if(allPersons)
			personIDTemp = -1;

		rowsAtSQL = SqliteFourPlatforms.Select (false, sessionID, personIDTemp, //type,
				Sqlite.Orders_by.ID_ASC, limit,
				allPersons//, 	//show names on comments only if "all persons"
				//false 	//! onlyBestInSession
				);
		LogB.Information ("rowsAtSQL count: " + (rowsAtSQL.Count).ToString ());
	}

	~PrepareEventGraphFourPlatforms() {}
}

public class PrepareEventGraphForceSensor
{
	//sql data of previous tests to plot graph and show stats at bottom
	public List<ForceSensor> rowsAtSQL;
	public int selectedID; //-1 if none selected. If >= 0 then is the selected on treeview.

	public bool exerciseAll; //all tests

	public PrepareEventGraphForceSensor() {
	}

	public PrepareEventGraphForceSensor (int sessionID, int personID, bool allPersons,
			Constants.ResultsSessionCriteria resultsSessionCriteria, int limit,
			int exerciseID, int selectedID, Constants.Modes mode, bool exerciseAll)
	{
		this.selectedID = selectedID;
		this.exerciseAll = exerciseAll;

		int personIDTemp = personID;
		if(allPersons)
			personIDTemp = -1;

		// see ForceSensor.GetElasticIntFromMode ()
		int elastic = -1;
		if (mode == Constants.Modes.FORCESENSORISOMETRIC)
			elastic = 0;
		else if (mode == Constants.Modes.FORCESENSORELASTIC)
			elastic = 1;

		Sqlite.Orders_by orderBy = Sqlite.Orders_by.ID_ASC;
		if (resultsSessionCriteria == Constants.ResultsSessionCriteria.BEST)
			orderBy = Sqlite.Orders_by.BEST;
		else if (resultsSessionCriteria == Constants.ResultsSessionCriteria.BEST2)
			orderBy = Sqlite.Orders_by.BEST2;

		rowsAtSQL = SqliteForceSensor.Select (false, -1, personIDTemp, sessionID, elastic, exerciseID,
				orderBy, limit,
				allPersons//, 	//show names on comments only if "all persons"
				//false 	//! onlyBestInSession
				);
		//LogB.Information ("rowsAtSQL count: " + (rowsAtSQL.Count).ToString ());

		this.selectedID = selectedID;
	}

	~PrepareEventGraphForceSensor() {}
}

public class PrepareEventGraphEncoderCurrent
{
	public string mainVariable;
	public double mainVariableHigher;
	public double mainVariableLower;
	public string secondaryVariable;
	public bool showLoss;
	public bool capturing;
	public string eccon;
	public double massDisplaced;
	public FeedbackEncoder feedback;
	public bool hasInertia;
	public bool playSoundsFromFile;
	public List<EncoderBarsData> encoderBarsData_l;
	public Gtk.ListStore encoderCaptureListStore;
	public bool relativeToSet;
	public double maxPowerSpeedForceIntersession; //it will be one of these 3
	public string maxPowerSpeedForceIntersessionDate;
	public int discardFirstN;
	public int showNRepetitions;
	public bool volumeOn;
	public Preferences.GstreamerTypes gstreamer;

	public PrepareEventGraphEncoderCurrent () {
	}

	public PrepareEventGraphEncoderCurrent (
			string mainVariable, double mainVariableHigher, double mainVariableLower,
			string secondaryVariable, bool showLoss,
			bool capturing, string eccon, double massDisplaced,
			FeedbackEncoder feedback,
			bool hasInertia, bool playSoundsFromFile,
			List<EncoderBarsData> encoderBarsData_l, Gtk.ListStore encoderCaptureListStore,
			bool relativeToSet,
			double maxPowerSpeedForceIntersession, string maxPowerSpeedForceIntersessionDate,
			int discardFirstN, int showNRepetitions, bool volumeOn, Preferences.GstreamerTypes gstreamer)

	{
		this.mainVariable = mainVariable;
		this.mainVariableHigher = mainVariableHigher;
		this.mainVariableLower = mainVariableLower;
		this.secondaryVariable = secondaryVariable;
		this.showLoss = showLoss;
		this.capturing = capturing;
		this.eccon = eccon;
		this.massDisplaced = massDisplaced;
		this.feedback = feedback;
		this.hasInertia = hasInertia;
		this.playSoundsFromFile = playSoundsFromFile;
		this.encoderBarsData_l = encoderBarsData_l;
		this.encoderCaptureListStore = encoderCaptureListStore;
		this.relativeToSet = relativeToSet;
		this.maxPowerSpeedForceIntersession = maxPowerSpeedForceIntersession;
		this.maxPowerSpeedForceIntersessionDate = maxPowerSpeedForceIntersessionDate;
		this.discardFirstN = discardFirstN;
		this.showNRepetitions = showNRepetitions;
		this.volumeOn = volumeOn;
		this.gstreamer = gstreamer;
	}

	~PrepareEventGraphEncoderCurrent () {}
}

public class PrepareEventGraphEncoderSession
{
	//sql data of previous tests to plot graph and show stats at bottom
	public List<EncoderSQL> rowsAtSQL;
	public int selectedSetID; //-1 if none selected. If >= 0 then is the selected on treeview.
	public List<int> selectedRepID_l; //need to match with the bars, as the bars are going to be repetitions

	public bool exerciseAll; //all tests

	public PrepareEventGraphEncoderSession () {
	}

	public PrepareEventGraphEncoderSession (int sessionID, int personID, bool allPersons,
			Constants.EncoderGI encoderGI,
			bool showBest,
			int limit,
			int exerciseID, int selectedSetID, Constants.Modes mode, bool exerciseAll)
	{
		this.selectedSetID = selectedSetID;
		this.exerciseAll = exerciseAll;

		int personIDTemp = personID;
		if(allPersons)
			personIDTemp = -1;

		Sqlite.Orders_by orderBy = Sqlite.Orders_by.BEST;
		if (! showBest)
			orderBy = Sqlite.Orders_by.ID_ASC;

		rowsAtSQL = SqliteEncoder.SelectList (false, -1, personIDTemp, sessionID, encoderGI,
				exerciseID, "curve", EncoderSQL.Eccons.ALL,
				"", 	//lateralityEnglish
				false, orderBy, 	// onlyActive, orderIDascendent
				true, 	//orderRepsByPosInSet
				limit,
				allPersons//, 	//show names on comments only if "all persons"
				//false 	//! onlyBestInSession
				);
		//LogB.Information ("rowsAtSQL count: " + (rowsAtSQL.Count).ToString ());

		//select linkedReps (if any)
		selectedRepID_l = new List<int> ();
		if (selectedSetID >= 0)
		{
			ArrayList linkedReps = SqliteEncoderSignalCurve.SelectSignalCurve (
					false, selectedSetID, -1, -1, -1);	//DBopened, signal, curve, msStart, msEnd

			foreach (EncoderSignalCurve esc in linkedReps)
				selectedRepID_l.Add (esc.curveID);
		}
	}

	~PrepareEventGraphEncoderSession() {}
}

public class UpdateProgressBar {
	public bool IsEvent;
	public bool PercentageMode;
	public double ValueToShow;

	public UpdateProgressBar() {
	}

	public UpdateProgressBar(bool isEvent, bool percentageMode, double valueToShow) {
		this.IsEvent = isEvent;
		this.PercentageMode = percentageMode;
		this.ValueToShow = valueToShow;
	}

	~UpdateProgressBar() {}
}

//start window buttons
public class MovingStartButton
{
	public bool Moving;

	private double pos;
	private double speed;
	private int end;
	public enum Dirs { R, L }
	private Dirs dir;


	public MovingStartButton(int start, int end, Dirs dir)
	{
		pos = start;
		this.end = end;
		this.dir = dir;
		Moving = true;
	}
	
	public bool Next()
	{
		if(dir == Dirs.R) {
			if( pos >= end )
				Moving = false;
			else {
				speed = Math.Ceiling(Math.Abs(end-pos)/25.0);
				pos += speed;
			}
		} else {
			if( pos <= end )
				Moving = false;
			else {
				speed = Math.Ceiling(Math.Abs(end-pos)/25.0);
				pos -= speed;
			}
		}

		//LogB.Information("pos: " + pos + "; speed: " + speed);
		return true;
	}

	public int Pos {
		get { return Convert.ToInt32(pos); }
	}
	public int Speed {
		get { return Convert.ToInt32(speed); }
	}
}

//to store the rectangle size of every encoder or forceSensor capture repetition
//in order to be saved or not on clicking screen
//note every rep will be c or ec
public class RepetitionMouseLimits
{
//	TODO: make all the sample stuff inherited

	protected List<PointInRectangle> list;
	protected int current;

	public RepetitionMouseLimits()
	{
		list = new List<PointInRectangle>();
		current = 0;
	}

	public void Add (double startX, double startY, double endX, double endY)
	{
		PointInRectangle p = new PointInRectangle (current ++, startX, startY, endX, endY);
		list.Add(p);
		//LogB.Information("Mouse added: " + p.ToString());
	}

	//used on CairoBars because bars go from right to left, so we force the pos here
	public void AddInPos (int pos, double startX, double startY, double endX, double endY)
	{
		PointInRectangle p = new PointInRectangle (pos, startX, startY, endX, endY);
		list.Add(p);
		//LogB.Information("Mouse added: " + p.ToString());
	}

	public int FindBarInPixel (double px, double py)
	{
		foreach (PointInRectangle pir in list)
			if (px >= pir.StartX && px <= pir.EndX)
			{
				if (pir.StartY < 0 && pir.EndY < 0) //forceSensor does not have Y, so both are -1, only check X
					return pir.Id;
				else if (py >= pir.StartY && py <= pir.EndY) //encoder has Y, need to check it
					return pir.Id;
			}

		return -1;
	}

	/*
	public double GetStartOfARep(int rep)
	{
		return ((PointInRectangle) list[rep]).Start;
	}
	public double GetEndOfARep(int rep)
	{
		return ((PointInRectangle) list[rep]).End;
	}
	*/

	//to debug
	public int Count ()
	{
		return list.Count;
	}
}
//used on graphs/cairo/forceSensor.cs CairoGraphForceSensorAI
public class RepetitionMouseLimitsWithSamples : RepetitionMouseLimits
{
	private List<int> sampleStart_l;
	private List<int> sampleEnd_l;

	public RepetitionMouseLimitsWithSamples ()
	{
		list = new List<PointInRectangle>();
		current = 0;

		sampleStart_l = new List<int>();
		sampleEnd_l = new List<int>();
	}

	public void AddSamples (int sampleStart, int sampleEnd)
	{
		sampleStart_l.Add (sampleStart);
		sampleEnd_l.Add (sampleEnd);
	}

	public int GetSampleStartOfARep (int rep)
	{
		return (sampleStart_l[rep]);
	}
	public int GetSampleEndOfARep (int rep)
	{
		return (sampleEnd_l[rep]);
	}
}

public class Blink
{
	private DateTime timeStart;

	public enum StatusEnum { NOTSTARTED, RUNNING, ENDED };
	public StatusEnum Status;

	//constructor
	public Blink ()
	{
		Status = StatusEnum.NOTSTARTED;
	}

	public void Start ()
	{
		timeStart = DateTime.Now;
		Status = StatusEnum.RUNNING;
	}

	public void End ()
	{
		Status = StatusEnum.ENDED;
	}

	//to show somthing like the red icon of capturing (blinking)
	public bool IsOn
	{
		get {
			TimeSpan ts = DateTime.Now.Subtract (timeStart);
			return (Util.IsEven (Convert.ToInt32 (ts.TotalSeconds)));
		}
	}
}

public class BlinkImage : Blink
{
	public Gtk.Image imageOff;
	public Gtk.Image imageOn;

	//constructor
	//TODO: assign color (tare, calibrate, detect stiffness: blue, capture: red)
	public BlinkImage (Gtk.Image imageOff, Gtk.Image imageOn)
	{
		Status = StatusEnum.NOTSTARTED;

		this.imageOff = imageOff;
		this.imageOn = imageOn;
	}

	public Gtk.Image ImageOff {
		get { return imageOff; }
	}
	public Gtk.Image ImageOn {
		get { return imageOn; }
	}
}
