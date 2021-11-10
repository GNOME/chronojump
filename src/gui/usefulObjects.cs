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
 * Copyright (C) 2004-2021   Xavier de Blas <xaviblas@gmail.com>
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

	public PrepareEventGraphJumpSimple() {
	}

	//allPersons is for searching the jumps of current of allpersons
	//personID we need to the personsMAX/AVG sql calls
	//type can be "" for all jumps, then write it under bar
	public PrepareEventGraphJumpSimple(double tv, double tc, int sessionID,
			int personID, bool allPersons, int limit,
			string table, string type, bool djShowHeights)
	{
		int personIDTemp = personID;
		if(allPersons)
			personIDTemp = -1;

		jumpsAtSQL = SqliteJump.SelectJumps (sessionID, personIDTemp, type,
				Sqlite.Orders_by.ID_DESC, limit,
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

	public PrepareEventGraphJumpReactive () {
	}

	//allPersons is for searching the jumps of current of allpersons
	//personID we need to the personsMAX/AVG sql calls
	//type can be "" for all jumps, then write it under bar
	public PrepareEventGraphJumpReactive (
			int sessionID, int personID, bool allPersons, int limit, string type)
	{
		// 1) assign variables
		this.type = type;

		Sqlite.Open(); // ----------------->

		int personIDTemp = personID;
		if(allPersons)
			personIDTemp = -1;

		jumpsAtSQL = SqliteJumpRj.SelectJumps (true, sessionID, personIDTemp, type,
				Sqlite.Orders_by.ID_DESC, limit, allPersons); 	//show names on comments only if "all persons"

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

public class PrepareEventGraphRunSimple {
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

	public PrepareEventGraphRunSimple() {
	}

	public PrepareEventGraphRunSimple(double time, double speed, int sessionID,
			int personID, bool allPersons, int limit,
			string table, string type)
	{
		Sqlite.Open();
		
		int personIDTemp = personID;
		if(allPersons)
			personIDTemp = -1;

		//obtain data
		runsAtSQL = SqliteRun.SelectRuns (true, sessionID, personIDTemp, type,
				Sqlite.Orders_by.ID_DESC, limit,
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

	public PrepareEventGraphRunInterval () {
	}

	//allPersons is for searching the jumps of current of allpersons
	//personID we need to the personsMAX/AVG sql calls
	//type can be "" for all jumps, then write it under bar
	public PrepareEventGraphRunInterval (
			int sessionID, int personID, bool allPersons, int limit, string type)
	{
		// 1) assign variables
		this.type = type;

		Sqlite.Open(); // ----------------->

		int personIDTemp = personID;
		if(allPersons)
			personIDTemp = -1;

		runsAtSQL = SqliteRunInterval.SelectRuns (true, sessionID, personIDTemp, type,
				Sqlite.Orders_by.ID_DESC, limit, allPersons); 	//show names on comments only if "all persons"

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
	public double distance;
	public double lastTime;
	public string timesString;
	public double distanceTotal; //we pass this because it's dificult to calculate in runs with variable distances
	public string distancesString; //we pass this because it's dificult to calculate in runs with variable distances
	public bool startIn;
	public bool finished;

	public PrepareEventGraphRunIntervalRealtimeCapture() {
	}

	public PrepareEventGraphRunIntervalRealtimeCapture (string type, double distance, double lastTime, string timesString,
			double distanceTotal, string distancesString, bool startIn, bool finished)
	{
		this.type = type;
		this.distance = distance;
		this.lastTime = lastTime;
		this.timesString = timesString;
		this.distanceTotal = distanceTotal;
		this.distancesString = distancesString;
		this.startIn = startIn;
		this.finished = finished;
	}

	~PrepareEventGraphRunIntervalRealtimeCapture() {}
}

public class PrepareEventGraphPulse {
	public double lastTime;
	public string timesString;

	public PrepareEventGraphPulse() {
	}

	public PrepareEventGraphPulse(double lastTime, string timesString) {
		this.lastTime = lastTime;
		this.timesString = timesString;
	}

	~PrepareEventGraphPulse() {}
}

public class PrepareEventGraphReactionTime {
	//sql data of previous rts to plot graph and show stats at bottom
	public string [] rtsAtSQL;
	public double personMAXAtSQL;
	public double sessionMAXAtSQL;
	public double personMINAtSQL;
	public double sessionMINAtSQL;
	public double personAVGAtSQL;
	public double sessionAVGAtSQL;

	public double time;

	public PrepareEventGraphReactionTime() {
	}

	public PrepareEventGraphReactionTime(double time, int sessionID, int personID, string table, string type) 
	{
		Sqlite.Open();

		//obtain data
		rtsAtSQL = SqliteReactionTime.SelectReactionTimes(true, sessionID, personID, type,
				Sqlite.Orders_by.ID_DESC, 10); //select only last 10
		
		personMAXAtSQL = SqliteSession.SelectMAXEventsOfAType(
				true, sessionID, personID, table, type, "time");
		sessionMAXAtSQL = SqliteSession.SelectMAXEventsOfAType(
				true, sessionID, -1, table, type, "time");
		
		personMINAtSQL = SqliteSession.SelectMINEventsOfAType(
				true, sessionID, personID, table, type, "time");
		sessionMINAtSQL = SqliteSession.SelectMINEventsOfAType(
				true, sessionID, -1, table, type, "time");

		personAVGAtSQL = SqliteSession.SelectAVGEventsOfAType(
				true, sessionID, personID, table, type, "time");
		sessionAVGAtSQL = SqliteSession.SelectAVGEventsOfAType(
				true, sessionID, -1, table, type, "time");
		
		Sqlite.Close();
	
		this.time = time;
	}

	~PrepareEventGraphReactionTime() {}
}

public class PrepareEventGraphMultiChronopic {
	//public double timestamp;
	public string cp1InStr;
	public string cp1OutStr;
	public string cp2InStr;
	public string cp2OutStr;
	public string cp3InStr;
	public string cp3OutStr;
	public string cp4InStr;
	public string cp4OutStr;
	public bool cp1StartedIn;
	public bool cp2StartedIn;
	public bool cp3StartedIn;
	public bool cp4StartedIn;

	public PrepareEventGraphMultiChronopic() {
	}

	public PrepareEventGraphMultiChronopic(
			//double timestamp, 
			bool cp1StartedIn, bool cp2StartedIn, bool cp3StartedIn, bool cp4StartedIn,
			string cp1InStr, string cp1OutStr, string cp2InStr, string cp2OutStr, 
			string cp3InStr, string cp3OutStr, string cp4InStr, string cp4OutStr) {
		//this.timestamp = timestamp;
		this.cp1StartedIn = cp1StartedIn; 
		this.cp2StartedIn = cp2StartedIn; 
		this.cp3StartedIn = cp3StartedIn; 
		this.cp4StartedIn = cp4StartedIn;
		this.cp1InStr = cp1InStr;
		this.cp1OutStr = cp1OutStr;
		this.cp2InStr = cp2InStr;
		this.cp2OutStr = cp2OutStr;
		this.cp3InStr = cp3InStr;
		this.cp3OutStr = cp3OutStr;
		this.cp4InStr = cp4InStr;
		this.cp4OutStr = cp4OutStr;
	}

	~PrepareEventGraphMultiChronopic() {}
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

//for animate last jump, run, pulse on a bar graph
public class MovingBar 
{
	public int X;
	public int Y;  //current value on animation
	/*
	 * y will go from (y + alto -speed) to (ytop)
	 * eg a vertical bar that goes from the bottom to the top, will go from:
	 * YTop 37, Y 380
	 * to
	 * YTop 37, Y 37
	 * so it will be decreasing
	 */
	
	public int Width;

	public int YTop; //stored y value
	public int AltoTop; //stored alto value
	
	public Gdk.GC Pen_bar_bg;
	public double Result;
	public int Step;
	public int DurationMS; //Step will depend on current time and durationMS (milliseconds)
	public Pango.Layout Layout;

	public MovingBar(int x, int y, int width, int yTop, int altoTop, 
			Gdk.GC pen_bar_bg, double result, int durationMS, Pango.Layout layout)
	{
		this.X = x;
		this.Y = y;
		this.Width = width;
		this.YTop = yTop;
		this.AltoTop = altoTop;
		this.Pen_bar_bg = pen_bar_bg;
		this.Result = result;
		this.DurationMS = durationMS;
		this.Layout = layout;
	} 

	/*
	public void Next() {
		Step = Convert.ToInt32(Math.Ceiling((Y-YTop)/100.0));
		Y = Y - Step;
	}
	*/

	Stopwatch sw;
	public int YStart;

	public void Start()
	{
		YStart = Y;
		sw = new Stopwatch();
		sw.Start();
	}

	//creates new Y and also returns it
	public int NextByDuration()
	{
		long elapsedMS = sw.ElapsedMilliseconds;
		if(elapsedMS >= DurationMS)
			Y = YTop;
		else {
			/*
			LogB.Information(string.Format("YStart: {0}, YTop: {1}, elapsed: {2}, duration: {3}, y will be: {4}",
						YStart, YTop, sw.ElapsedMilliseconds, DurationMS,
						YStart - Convert.ToInt32( UtilAll.DivideSafe(elapsedMS, DurationMS) * (YStart - YTop) )));
			*/
			Y = YStart - Convert.ToInt32( UtilAll.DivideSafe(elapsedMS, DurationMS) * (YStart - YTop) );
		}
		return Y;
	}

	~MovingBar() {}
}

//to store the xStart and xEnd of every encoder or forceSensor capture reptition
//in order to be saved or not on clicking screen
//note every rep will be c or ec
public class RepetitionMouseLimits
{
	private List<PointStartEnd> list;
	private int current;

	public RepetitionMouseLimits()
	{
		list = new List<PointStartEnd>();
		current = 0;
	}

	public void Add (double start, double end)
	{
		PointStartEnd p = new PointStartEnd(current ++, start, end);
		list.Add(p);
		LogB.Information("Added: " + p.ToString());
	}

	public int FindBarInPixel (double pixel)
	{
		foreach(PointStartEnd p in list)
			if(pixel >= p.Start && pixel <= p.End)
				return p.Id;

		return -1;
	}

	public double GetStartOfARep(int rep)
	{
		return ((PointStartEnd) list[rep]).Start;
	}
	public double GetEndOfARep(int rep)
	{
		return ((PointStartEnd) list[rep]).End;
	}
}
