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
 * Copyright (C) 2004-2026   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Data;
using System.Diagnostics; //Stopwatch
using Gtk;
using Mono.Unix;
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

public abstract class PrepareEventGraphTest
{
	public int selectedID; //-1 if none selected. If >= 0 then is the selected on treeview.
	public Event selectedEvent;

	// to show on X axis
	public enum OrderXEnum { Best, Last, Weight }; // Not capitals because it will be printed
	public OrderXEnum OrderX;

	protected int sessionID;

	protected int personID; //this will no longer be used
	protected int personIDForGraph;
	protected int currentPersonID; //personID will be -1 if all persons, but we need to know also who is the currentPerson
	protected string currentPersonName;
	protected bool allPersons;

	protected string type; //this will no longer be used
	protected string typeCurrent;
	protected string typeForGraph;

	protected double historicalExD;
	protected string historicalExStr;
	protected string historicalExUnits;

	protected Boxplot boxplotPerson;
	protected Boxplot boxplotSession;

	protected void initVariables () //add also sessionID, personID, ...
	{
		historicalExD = 0;
		historicalExStr = "";
		historicalExUnits = "";
	}

	//need to be private of each class, if public orprotected says: Inconsistent accessibility)
	//protected Sqlite.Orders_by orderBy;

	protected void getSelected ()
	{
		selectedEvent = null;
		if (selectedID >= 0)
			if (! selectEventFromList ())
				selectEventFromSQL ();
	}

	protected abstract bool selectEventFromList ();
	protected abstract void selectEventFromSQL ();

	protected void boxplotsDo (string param)
	{
		boxplotPerson = new Boxplot (boxplotSelectPerson (param));
		boxplotPerson.Do ();

		boxplotSession = new Boxplot (boxplotSelectSession (param));
		boxplotSession.Do ();
	}

	protected abstract List<double> boxplotSelectPerson (string param);
	protected abstract List<double> boxplotSelectSession (string param);

	protected void personHistoricalBest (string sqlSelect)
	{
		if (! personHistoricalBestHaveData ())
			return;

		SqliteStruct.DateTypeResult dtr = personHistoricalBestGetData (sqlSelect);

		if (dtr.date == "" || dtr.type == "")
			return;

		historicalExD = dtr.result;
		historicalExStr = string.Format (
				Catalog.GetString ("Best {0} achieved by {1}:"),
				dtr.type, currentPersonName) + " " +
			string.Format ("{0} {1} ({2})",
					Util.TrimDecimals (dtr.result, 2),
					historicalExUnits,
					UtilDate.GetDatetimePrint (UtilDate.FromFile (dtr.date)));
	}

	public virtual bool personHistoricalBestHaveData ()
	{
		return false;
	}
	public virtual SqliteStruct.DateTypeResult personHistoricalBestGetData (string sqlSelect)
	{
		return SqliteStruct.DateTypeResult.Init ();
	}

	// TODO: Type will disappear
	public string Type {
		get { return type; }
	}
	public string TypeForGraph {
		get { return typeForGraph; }
	}

	public Boxplot BoxplotPerson {
		get { return boxplotPerson; }
	}

	public Boxplot BoxplotSession {
		get { return boxplotSession; }
	}

	public double HistoricalExD {
		get { return historicalExD; }
	}
	public string HistoricalExStr {
		get { return historicalExStr; }
	}
}

public class PrepareEventGraphJumpSimple : PrepareEventGraphTest
{
	//sql data of previous jumps to plot graph and show stats at bottom
	public List<Jump> rowsAtSQL;
	
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
	public bool showHeights;
	private Sqlite.Orders_by orderBy;
	private bool exerciseAll;

	public PrepareEventGraphJumpSimple() {
	}

	//allPersons is for searching the jumps of current of allpersons
	//personIDForGraph we need to the personsMAX/AVG sql calls
	//type can be "" for all jumps, then write it under bar
	public PrepareEventGraphJumpSimple (
			double tv, double tc, int sessionID,
			int currentPersonID, string currentPersonName, bool allPersons,
			bool showHeights,
			bool showBest, int limit,
			string typeCurrent, int selectedID, bool exerciseAll)
	{
		this.sessionID = sessionID;
		this.currentPersonID = currentPersonID;
		this.currentPersonName = currentPersonName;
		this.allPersons = allPersons;
		this.typeCurrent = typeCurrent;
		this.tv = tv;
		this.tc = tc;
		this.showHeights = showHeights;
		this.selectedID = selectedID;
		this.exerciseAll = exerciseAll;

		initVariables ();

		Sqlite.Open(); // ----------------->

		personIDForGraph = currentPersonID;
		if (allPersons)
			personIDForGraph = -1;

		typeForGraph = typeCurrent;
		if (exerciseAll)
			typeForGraph = "";

		orderBy = Sqlite.Orders_by.BEST;
		OrderX = OrderXEnum.Best;
		if (! showBest)
		{
			orderBy = Sqlite.Orders_by.ID_ASC;
			OrderX = OrderXEnum.Last;
		}

		rowsAtSQL = SqliteJump.SelectJumps (true, sessionID, personIDForGraph, typeForGraph,
				orderBy, limit,
				allPersons, 	//show names on comments only if "all persons"
				false); 	//! onlyBestInSession

		// get the selectedJump to show it if it's not aready shown by the limit
		getSelected ();

		string param = "tv";
		historicalExUnits = "s";
		if (showHeights)
		{
			param = "100*4.9*(tv/2)*(tv/2)";
			historicalExUnits = "cm";
		}

		boxplotsDo (param);
		personHistoricalBest (param);

		Sqlite.Close(); // < -----------------
	}

	protected override bool selectEventFromList ()
	{
		foreach (Jump e in rowsAtSQL)
			if (e.UniqueID == selectedID)
			{
				selectedEvent = e;
				return true;
			}
		return false;
	}

	protected override void selectEventFromSQL ()
	{
		selectedEvent = SqliteJump.SelectJumpData (selectedID, true);
	}

	protected override List<double> boxplotSelectPerson (string param)
	{
		return SqliteJump.SelectJumps (true, param, sessionID, personIDForGraph, typeForGraph,
				Sqlite.Orders_by.BEST, 0, // no limit
				false); 	//! onlyBestInSession
	}
	protected override List<double> boxplotSelectSession (string param)
	{
		return SqliteJump.SelectJumps (true, param, sessionID, -1, typeForGraph,
				Sqlite.Orders_by.BEST, 0, // no limit
				false); 	//! onlyBestInSession
	}

	public override bool personHistoricalBestHaveData ()
	{
		if (typeCurrent == "")
			return false;

		SqliteBest sb = new SqliteBest ();
		return sb.HaveEventsInOtherSessions (true, sessionID, currentPersonID,
					Constants.JumpTable, typeCurrent, -1, Constants.JumpTypeTable);
	}
	public override SqliteStruct.DateTypeResult personHistoricalBestGetData (string param)
	{
		SqliteBest sb = new SqliteBest ();
		return sb.Select_MAX_EventsOfAType (true, -1, currentPersonID,
				Constants.JumpTable, typeCurrent, -1,
				Constants.JumpTypeTable, param);
	}

	~PrepareEventGraphJumpSimple() {}
}

public class PrepareEventGraphJumpReactive : PrepareEventGraphTest
{
	//sql data of previous jumps to plot graph and show stats at bottom
	public List<JumpRj> rowsAtSQL;


	public double personMAXAtSQL;
	public double sessionMAXAtSQL;

	public double personAVGAtSQL;
	public double sessionAVGAtSQL;

	public double personMINAtSQL;
	public double sessionMINAtSQL;
	public bool showHeights; //if showHeights graph falling height and jump height
	private Sqlite.Orders_by orderBy;

	public PrepareEventGraphJumpReactive () {
	}

	//allPersons is for searching the jumps of current of allpersons
	//personID we need to the personsMAX/AVG sql calls
	//type can be "" for all jumps, then write it under bar
	public PrepareEventGraphJumpReactive (
			int sessionID, int personID, bool allPersons,
			bool showHeights,
			Constants.ResultsSessionCriteria resultsSessionCriteria, int limit,
			string type, int selectedID)
	{
		// 1) assign variables
		this.sessionID = sessionID;
		this.personID = personID;
		this.allPersons = allPersons;
		this.type = type;
		this.selectedID = selectedID;
		this.showHeights = showHeights;

		Sqlite.Open(); // ----------------->

		int personIDTemp = personID;
		if(allPersons)
			personIDTemp = -1;

		orderBy = Sqlite.Orders_by.ID_ASC;
		OrderX = OrderXEnum.Last;
		string sqlRangeSelect = "";
		if (resultsSessionCriteria == Constants.ResultsSessionCriteria.LAST)
		{
			orderBy = Sqlite.Orders_by.ID_ASC;
			if (showHeights)
				sqlRangeSelect = "heightAvg";
			else
				sqlRangeSelect = "tvAvg";
		}
		else if (resultsSessionCriteria == Constants.ResultsSessionCriteria.BEST)
		{
			orderBy = Sqlite.Orders_by.BEST;
			OrderX = OrderXEnum.Best;
			sqlRangeSelect = "tvAvg";
		} else if (resultsSessionCriteria == Constants.ResultsSessionCriteria.BEST2)
		{
			orderBy = Sqlite.Orders_by.BEST2;
			OrderX = OrderXEnum.Best;
			//sqlRangeSelect = "tvAvg/tcAvg";
			sqlRangeSelect = "tvAvg"; //bars show tvAvg (not Q), so use this on Y
		} else // if (resultsSessionCriteria == Constants.ResultsSessionCriteria.BEST3)
		{
			orderBy = Sqlite.Orders_by.BEST3;
			OrderX = OrderXEnum.Best;
			sqlRangeSelect = "heightAvg";
		}

		LogB.Information (string.Format ("LIMIT: " + limit));
		rowsAtSQL = SqliteJumpRj.SelectJumps (true, sessionID, personIDTemp, type,
				orderBy, limit, allPersons); 	//show names on comments only if "all persons"

		getSelected ();

		boxplotsDo (sqlRangeSelect);

		Sqlite.Close(); // < -----------------
	}

	protected override bool selectEventFromList ()
	{
		foreach (JumpRj e in rowsAtSQL)
			if (e.UniqueID == selectedID)
			{
				selectedEvent = e;
				return true;
			}
		return false;
	}

	protected override void selectEventFromSQL ()
	{
		selectedEvent = SqliteJumpRj.SelectJumpData (Constants.JumpRjTable, selectedID, false, true);
	}

	protected override List<double> boxplotSelectPerson (string param)
	{
		return SqliteJumpRj.SelectJumps (true, param, sessionID, personID, type,
				Sqlite.Orders_by.BEST, 0); // no limit
	}
	protected override List<double> boxplotSelectSession (string param)
	{
		return SqliteJumpRj.SelectJumps (true, param, sessionID, -1, type,
				Sqlite.Orders_by.BEST, 0); // no limit
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

public class PrepareEventGraphRunSimple : PrepareEventGraphTest
{
	//sql data of previous runs to plot graph and show stats at bottom
	public List<Run> rowsAtSQL;

	public double personMAXAtSQLAllSessions;
	public double personMAXAtSQL;
	public double sessionMAXAtSQL;

	public double personAVGAtSQL;
	public double sessionAVGAtSQL;

	public double personMINAtSQL;
	public double sessionMINAtSQL;

	public double time;
	public double speed;
	private Sqlite.Orders_by orderBy;
	private bool exerciseAll;

	public PrepareEventGraphRunSimple() {
	}

	public PrepareEventGraphRunSimple (
			double time, double speed, int sessionID,
			int currentPersonID, string currentPersonName, bool allPersons,
			Constants.ResultsSessionCriteria resultsSessionCriteria, bool times,
			int limit,
			string typeCurrent, int selectedID, bool exerciseAll)
	{
		this.sessionID = sessionID;
		this.currentPersonID = currentPersonID;
		this.currentPersonName = currentPersonName;
		this.allPersons = allPersons;
		this.typeCurrent = typeCurrent;
		this.time = time;
		this.speed = speed;
		this.selectedID = selectedID;
		this.exerciseAll = exerciseAll;

		initVariables ();

		Sqlite.Open(); // ----------------->
		
		personIDForGraph = currentPersonID;
		if (allPersons)
			personIDForGraph = -1;

		typeForGraph = typeCurrent;
		if (exerciseAll)
			typeForGraph = "";

		orderBy = Sqlite.Orders_by.ID_ASC;
		OrderX = OrderXEnum.Last;
		if (resultsSessionCriteria == Constants.ResultsSessionCriteria.BEST)
		{
			orderBy = Sqlite.Orders_by.BEST;
			OrderX = OrderXEnum.Best;
		} else if (resultsSessionCriteria == Constants.ResultsSessionCriteria.BEST2) {
			orderBy = Sqlite.Orders_by.BEST2;
			OrderX = OrderXEnum.Best;
		}

		LogB.Information ("resultsSessionCriteria = " + resultsSessionCriteria.ToString ());

		//obtain data
		rowsAtSQL = SqliteRun.SelectRuns (true, sessionID, personIDForGraph, typeForGraph,
				orderBy, limit,
				allPersons, false); //show names on comments only if "all persons"

		// get the selectedEvent to show it if it's not aready shown by the limit
		getSelected ();

		string param = "distance/time";
		historicalExUnits = "m/s";
		if (times)
		{
			param = "time";
			historicalExUnits = "s";
		}

		// if ID_ASC, order correctly the boxplot
		if (orderBy == Sqlite.Orders_by.ID_ASC)
		{
			orderBy = Sqlite.Orders_by.BEST;
			if (times)
				orderBy = Sqlite.Orders_by.BEST2REV;
		}
		else if (orderBy == Sqlite.Orders_by.BEST2)
			orderBy = Sqlite.Orders_by.BEST2REV; //boxplot have to be selected asc

		boxplotsDo (param);
		personHistoricalBest (param);

		Sqlite.Close(); // < -----------------
	}

	protected override bool selectEventFromList ()
	{
		foreach (Run e in rowsAtSQL)
			if (e.UniqueID == selectedID)
			{
				selectedEvent = e;
				return true;
			}
		return false;
	}
	protected override void selectEventFromSQL ()
	{
		selectedEvent = SqliteRun.SelectRunData (selectedID, true);
	}

	// need to use orderBy to correctly order time for boxplot
	protected override List<double> boxplotSelectPerson (string param)
	{
		return SqliteRun.SelectRuns (true, param, sessionID, personIDForGraph, typeForGraph,
				orderBy, 0); // no limit
	}
	protected override List<double> boxplotSelectSession (string param)
	{
		return SqliteRun.SelectRuns (true, param, sessionID, -1, typeForGraph,
				orderBy, 0); // no limit
	}

	public override bool personHistoricalBestHaveData ()
	{
		if (typeCurrent == "")
			return false;

		SqliteBest sb = new SqliteBest ();
		return sb.HaveEventsInOtherSessions (true, sessionID, currentPersonID,
					Constants.RunTable, typeCurrent, -1, Constants.RunTypeTable);
	}
	public override SqliteStruct.DateTypeResult personHistoricalBestGetData (string param)
	{
		SqliteBest sb = new SqliteBest ();
		if (param == "time")
			return sb.Select_MIN_EventsOfAType (true, -1, currentPersonID,
					Constants.RunTable, typeCurrent, -1,
					Constants.RunTypeTable, param);
		else
			return sb.Select_MAX_EventsOfAType (true, -1, currentPersonID,
					Constants.RunTable, typeCurrent, -1,
					Constants.RunTypeTable, param);
	}

	~PrepareEventGraphRunSimple() {}
}

public class PrepareEventGraphRunInterval : PrepareEventGraphTest
{
	//sql data of previous jumps to plot graph and show stats at bottom
	public List<RunInterval> rowsAtSQL;

	public double personMAXAtSQL;
	public double sessionMAXAtSQL;

	public double personAVGAtSQL;
	public double sessionAVGAtSQL;

	public double personMINAtSQL;
	public double sessionMINAtSQL;
	private Sqlite.Orders_by orderBy;

	public PrepareEventGraphRunInterval () {
	}

	//allPersons is for searching the jumps of current of allpersons
	//personID we need to the personsMAX/AVG sql calls
	//type can be "" for all jumps, then write it under bar
	public PrepareEventGraphRunInterval (
			int sessionID, int personID, bool allPersons,
			Constants.ResultsSessionCriteria resultsSessionCriteria, bool times,
			int limit,
			string type, int selectedID)
	{
		// 1) assign variables
		this.sessionID = sessionID;
		this.personID = personID;
		this.allPersons = allPersons;
		this.type = type;
		this.selectedID = selectedID;

		Sqlite.Open(); // ----------------->

		int personIDTemp = personID;
		if(allPersons)
			personIDTemp = -1;

		orderBy = Sqlite.Orders_by.ID_ASC;
		OrderX = OrderXEnum.Last;
		if (resultsSessionCriteria == Constants.ResultsSessionCriteria.BEST)
		{
			orderBy = Sqlite.Orders_by.BEST;
			OrderX = OrderXEnum.Best;
		} else if (resultsSessionCriteria == Constants.ResultsSessionCriteria.BEST2) {
			orderBy = Sqlite.Orders_by.BEST2;
			OrderX = OrderXEnum.Best;
		}

		rowsAtSQL = SqliteRunInterval.SelectRuns (true, sessionID, personIDTemp, type,
				orderBy, limit, allPersons); 	//show names on comments only if "all persons"

		// get the selectedEvent to show it if it's not aready shown by the limit
		getSelected ();

		string sqlSelect = "distanceTotal/timeTotal";
		if (times)
			sqlSelect = "timeTotal";

		// if ID_ASC, order correctly the boxplot
		if (orderBy == Sqlite.Orders_by.ID_ASC)
		{
			orderBy = Sqlite.Orders_by.BEST;
			if (times)
				orderBy = Sqlite.Orders_by.BEST2REV;
		}
		if (orderBy == Sqlite.Orders_by.BEST2)
			orderBy = Sqlite.Orders_by.BEST2REV; //boxplot have to be selected asc

		boxplotsDo (sqlSelect);

		Sqlite.Close(); // < -----------------
	}

	protected override bool selectEventFromList ()
	{
		foreach (RunInterval e in rowsAtSQL)
			if (e.UniqueID == selectedID)
			{
				selectedEvent = e;
				return true;
			}
		return false;
	}
	protected override void selectEventFromSQL ()
	{
		selectedEvent = SqliteRunInterval.SelectRunData (Constants.RunIntervalTable, selectedID, false, true);
	}

	// need to use orderBy to correctly order time for boxplot
	protected override List<double> boxplotSelectPerson (string param)
	{
		return SqliteRunInterval.SelectRuns (true, param, sessionID, personID, type,
				orderBy, 0); // no limit
	}
	protected override List<double> boxplotSelectSession (string param)
	{
		return SqliteRunInterval.SelectRuns (true, param, sessionID, -1, type,
				orderBy, 0); // no limit
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

public class PrepareEventGraphRunEncoder : PrepareEventGraphTest
{
	//sql data of previous tests to plot graph and show stats at bottom
	public List<RunEncoder> rowsAtSQL;
	private Sqlite.Orders_by orderBy;
	private int exerciseID;
	private bool bestSecond;

	public bool exerciseAll; //all tests

	public PrepareEventGraphRunEncoder() {
	}

	public PrepareEventGraphRunEncoder (int sessionID, int personID, bool allPersons,
			Constants.ResultsSessionCriteria resultsSessionCriteria, bool bestSecond, int limit,
			int exerciseID, int selectedID, Constants.Modes mode, bool exerciseAll)
	{
		this.sessionID = sessionID;
		this.personID = personID;
		this.exerciseID = exerciseID;
		this.bestSecond = bestSecond;
		this.selectedID = selectedID;
		this.exerciseAll = exerciseAll;

		Sqlite.Open(); // ----------------->

		int personIDTemp = personID;
		if(allPersons)
			personIDTemp = -1;

		orderBy = Sqlite.Orders_by.ID_ASC;
		OrderX = OrderXEnum.Last;
		if (resultsSessionCriteria == Constants.ResultsSessionCriteria.BEST)
		{
			orderBy = Sqlite.Orders_by.BEST;
			OrderX = OrderXEnum.Best;
		} else if (resultsSessionCriteria == Constants.ResultsSessionCriteria.BEST2) {
			orderBy = Sqlite.Orders_by.BEST2;
			OrderX = OrderXEnum.Best;
		}

		rowsAtSQL = SqliteRunEncoder.Select (true, -1, personIDTemp, sessionID, exerciseID,
				orderBy, limit,
				allPersons//, 	//show names on comments only if "all persons"
				//false 	//! onlyBestInSession
				);

		// get the selectedEvent to show it if it's not aready shown by the limit
		getSelected ();

		string sqlSelect = "maxSpeed";
		if (bestSecond)
			sqlSelect = "maxAvgSpeed1s";

		boxplotsDo (sqlSelect);

		Sqlite.Close(); // < -----------------
	}

	protected override bool selectEventFromList ()
	{
		foreach (RunEncoder e in rowsAtSQL)
			if (e.UniqueID == selectedID)
			{
				selectedEvent = e;
				return true;
			}
		return false;
	}
	protected override void selectEventFromSQL ()
	{
		RunEncoder sel = SqliteRunEncoder.SelectData (selectedID, false, true);
		if (sel.UniqueID < 0)
			selectedEvent = null; //to manage problems at deleting and updating treeview/bars
		else
			selectedEvent = sel;

		selectedEvent = SqliteRunEncoder.SelectData (selectedID, false, true);
	}

	protected override List<double> boxplotSelectPerson (string param)
	{
		return SqliteRunEncoder.Select (true, param, -1, personID, sessionID, exerciseID,
				orderBy, 0);
	}
	protected override List<double> boxplotSelectSession (string param)
	{
		return SqliteRunEncoder.Select (true, param, -1, -1, sessionID, exerciseID,
				orderBy, 0);
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

public class PrepareEventGraphForceSensor : PrepareEventGraphTest
{
	//sql data of previous tests to plot graph and show stats at bottom
	public List<ForceSensor> rowsAtSQL;
	private Sqlite.Orders_by orderBy;
	private	int elastic;
	private int currentExerciseID; //selected on top
	private int exerciseIDForGraph; // will be currentExerciseID or -1 (if exerciseAll)
	private bool bestSecond;

	public bool exerciseAll; //all tests

	public PrepareEventGraphForceSensor() {
	}

	public PrepareEventGraphForceSensor (int sessionID, int currentPersonID, string currentPersonName, bool allPersons,
			Constants.ResultsSessionCriteria resultsSessionCriteria, bool bestSecond, int limit,
			int currentExerciseID, int selectedID, Constants.Modes mode, bool exerciseAll)
	{
		this.sessionID = sessionID;
		this.currentPersonID = currentPersonID;
		this.currentPersonName = currentPersonName;
		this.currentExerciseID = currentExerciseID;
		this.selectedID = selectedID;
		this.bestSecond = bestSecond;
		this.exerciseAll = exerciseAll;

		initVariables ();

		Sqlite.Open(); // ----------------->

		personIDForGraph = currentPersonID;
		if (allPersons)
			personIDForGraph = -1;

		exerciseIDForGraph = currentExerciseID;
		if (exerciseAll)
			exerciseIDForGraph = -1;

		// see ForceSensor.GetElasticIntFromMode ()
		elastic = -1;
		if (mode == Constants.Modes.FORCESENSORISOMETRIC)
			elastic = 0;
		else if (mode == Constants.Modes.FORCESENSORELASTIC)
			elastic = 1;

		orderBy = Sqlite.Orders_by.ID_ASC;
		OrderX = OrderXEnum.Last;
		if (resultsSessionCriteria == Constants.ResultsSessionCriteria.BEST)
		{
			orderBy = Sqlite.Orders_by.BEST;
			OrderX = OrderXEnum.Best;
		} else if (resultsSessionCriteria == Constants.ResultsSessionCriteria.BEST2) {
			orderBy = Sqlite.Orders_by.BEST2;
			OrderX = OrderXEnum.Best;
		}

		rowsAtSQL = SqliteForceSensor.Select (true, -1, personIDForGraph, sessionID, elastic, exerciseIDForGraph,
				orderBy, limit,
				allPersons//, 	//show names on comments only if "all persons"
				//false 	//! onlyBestInSession
				);
		//LogB.Information ("rowsAtSQL count: " + (rowsAtSQL.Count).ToString ());

		// get the selectedEvent to show it if it's not aready shown by the limit
		getSelected ();

		string sqlSelect = "maxForceRaw";
		if (bestSecond)
			sqlSelect = "maxAvgForce1s";

		historicalExUnits = "N";

		// if ID_ASC, order correctly the boxplot
		if (orderBy == Sqlite.Orders_by.ID_ASC)
		{
			orderBy = Sqlite.Orders_by.BEST;
			if (bestSecond)
				orderBy = Sqlite.Orders_by.BEST2;
		}

		boxplotsDo (sqlSelect);
		personHistoricalBest (sqlSelect);

		Sqlite.Close(); // < -----------------
	}

	protected override bool selectEventFromList ()
	{
		foreach (ForceSensor e in rowsAtSQL)
			if (e.UniqueID == selectedID)
			{
				selectedEvent = e;
				return true;
			}
		return false;
	}
	protected override void selectEventFromSQL ()
	{
		ForceSensor sel = SqliteForceSensor.SelectData (selectedID, false, true);
		if (sel.UniqueID < 0)
			selectedEvent = null; //to manage problems at deleting and updating treeview/bars
		else
			selectedEvent = sel;
	}

	protected override List<double> boxplotSelectPerson (string param)
	{
		return SqliteForceSensor.Select (true, param, -1, personIDForGraph, sessionID, elastic, exerciseIDForGraph,
				orderBy, 0);
	}
	protected override List<double> boxplotSelectSession (string param)
	{
		return SqliteForceSensor.Select (true, param, -1, -1, sessionID, elastic, exerciseIDForGraph,
				orderBy, 0);
	}

	public override bool personHistoricalBestHaveData ()
	{
		if (currentExerciseID < 0)
			return false;

		SqliteBest sb = new SqliteBest ();
		return sb.HaveEventsInOtherSessions (true, sessionID, currentPersonID,
					Constants.ForceSensorTable, "", currentExerciseID, Constants.ForceSensorExerciseTable);
	}
	public override SqliteStruct.DateTypeResult personHistoricalBestGetData (string sqlSelect)
	{
		SqliteBest sb = new SqliteBest ();
		return sb.Select_MAX_EventsOfAType (true, -1, currentPersonID,
				Constants.ForceSensorTable, "", currentExerciseID,
				Constants.ForceSensorExerciseTable, sqlSelect);
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

public class PrepareEventGraphEncoderSession : PrepareEventGraphTest
{
	public List<Event> selectedEvent_l; // if more modes use a list, move this to PrepareEventGraphTest

	//sql data of previous tests to plot graph and show stats at bottom
	public List<EncoderSQL> rowsAtSQL;
	public int selectedSetID; //-1 if none selected. If >= 0 then is the selected on treeview.
	public List<int> selectedRepID_l; //need to match with the bars, as the bars are going to be repetitions
	private Sqlite.Orders_by orderBy;

	public bool exerciseAll; //all tests
	private Constants.EncoderGI encoderGI;
	private Constants.EncoderVariablesCapture encoderVariablesCapture;
	private int exerciseID;

	public PrepareEventGraphEncoderSession () {
	}

	public PrepareEventGraphEncoderSession (int sessionID, int personID, bool allPersons,
			Constants.EncoderGI encoderGI,
			Constants.ResultsSessionCriteria resultsSessionCriteria,
			Constants.EncoderVariablesCapture encoderVariablesCapture, //used if resultsSessionCriteria == BEST
			int limit,
			int exerciseID, int selectedSetID, Constants.Modes mode, bool exerciseAll)
	{
		this.sessionID = sessionID;
		this.personID = personID;
		this.exerciseID = exerciseID;
		this.selectedSetID = selectedSetID;
		this.exerciseAll = exerciseAll;
		this.encoderGI = encoderGI;
		this.encoderVariablesCapture = encoderVariablesCapture;

		int personIDTemp = personID;
		if(allPersons)
			personIDTemp = -1;

		orderBy = Sqlite.Orders_by.ID_ASC;
		OrderX = OrderXEnum.Last;
		if (resultsSessionCriteria == Constants.ResultsSessionCriteria.BEST)
		{
			orderBy = Sqlite.Orders_by.BEST;
			OrderX = OrderXEnum.Best;
		} else if (resultsSessionCriteria == Constants.ResultsSessionCriteria.BEST2) {
			orderBy = Sqlite.Orders_by.BEST2;
			OrderX = OrderXEnum.Weight;
		}

		rowsAtSQL = SqliteEncoder.SelectList (false, -1, personIDTemp, sessionID, encoderGI,
				exerciseID, "curve", EncoderSQL.Eccons.ALL,
				"", 	//lateralityEnglish
				false, orderBy, encoderVariablesCapture, 	// onlyActive
				true, 	//orderRepsByPosInSet
				limit,
				allPersons//, 	//show names on comments only if "all persons"
				//false 	//! onlyBestInSession
				);
		//LogB.Information ("rowsAtSQL count: " + (rowsAtSQL.Count).ToString ());

		//select linkedReps (if any)
		selectedRepID_l = new List<int> ();
		selectedEvent_l = new List<Event> ();
		if (selectedSetID >= 0)
		{
			ArrayList linkedReps = SqliteEncoderSignalCurve.SelectSignalCurve (
					false, selectedSetID, -1, -1, -1);	//DBopened, signal, curve, msStart, msEnd

			foreach (EncoderSignalCurve esc in linkedReps)
				selectedRepID_l.Add (esc.curveID);
		}

		getSelected ();
		orderBy = Sqlite.Orders_by.BEST; 	//for boxplots use order_by.BEST
		boxplotsDo (Constants.GetEncoderVariablesCaptureAsSQLField (encoderVariablesCapture));
	}

	protected override bool selectEventFromList ()
	{
		// as on encoder some the reps selected could be shown by the filter and some other not. Better return false
		// and ensure select all of them on selectEventFromSQL ()
		return false;
	}
	protected override void selectEventFromSQL ()
	{
		if (selectedSetID < 0)
			return;

		SqliteEncoder sqliteEncoder = new SqliteEncoder ();
		List<List<EncoderSQL>> eSQL_ll = sqliteEncoder.SelectSetsAndRepsLList (false, personID, sessionID, encoderGI,
				exerciseID, selectedSetID);

		if (eSQL_ll.Count > 0 && eSQL_ll[0].Count > 1) // 1 because: 0 is the set, reps start at 1
			for (int i = 1; i < eSQL_ll[0].Count; i ++)
				selectedEvent_l.Add (eSQL_ll[0][i]);
	}

	protected override List<double> boxplotSelectPerson (string param)
	{
		return SqliteEncoder.SelectList (false, param, -1, personID, sessionID, encoderGI,
				exerciseID, "curve", EncoderSQL.Eccons.ALL,
				"", 	//lateralityEnglish
				false, orderBy, encoderVariablesCapture, 	// onlyActive
				false, 	//orderRepsByPosInSet
				0
				);
	}
	protected override List<double> boxplotSelectSession (string param)
	{
		return SqliteEncoder.SelectList (false, param, -1, -1, sessionID, encoderGI,
				exerciseID, "curve", EncoderSQL.Eccons.ALL,
				"", 	//lateralityEnglish
				false, orderBy, encoderVariablesCapture, 	// onlyActive
				false, 	//orderRepsByPosInSet
				0
				);
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
				else if (py >= pir.StartY && py <= pir.EndY) //encoder has Y, need to check it. Note also on BarPoints.POINTS the y is relevant
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
