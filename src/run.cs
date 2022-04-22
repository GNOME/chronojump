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
 *  Copyright (C) 2004-2022   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Data;
using System.IO; 		//for detect OS //TextWriter
using System.Collections.Generic; //List
using System.Threading;
using Mono.Unix;

public class Run : Event 
{
	protected double distance;
	protected double time;

	//for not checking always in database
	protected bool startIn;
	
	protected bool initialSpeed;
	
	//protected Chronopic cp;
	protected bool metersSecondsPreferred;
	protected string datetime;

/*
	//used by the updateTimeProgressBar for display its time information
	//changes a bit on runSimple and runInterval
	//explained at each of the updateTimeProgressBar() 
	protected enum runPhases {
		PRE_RUNNING, PLATFORM_INI, RUNNING, PLATFORM_END
	}
	protected runPhases runPhase;
*/		
	
	public Run() {
	}

	//after inserting database (SQL)
	public Run(int uniqueID, int personID, int sessionID, string type, double distance, double time, string description, int simulated, bool initialSpeed, string datetime)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.sessionID = sessionID;
		this.type = type;
		this.distance = distance;
		this.time = time;
		this.description = description;
		this.simulated = simulated;
		this.initialSpeed = initialSpeed;
		this.datetime = datetime;
	}

	//used to select a run at SqliteRun.SelectRunData and at Sqlite.convertTables
	public Run(string [] eventString)
	{
		this.uniqueID = Convert.ToInt32(eventString[0]);
		this.personID = Convert.ToInt32(eventString[1]);
		this.sessionID = Convert.ToInt32(eventString[2]);
		this.type = eventString[3].ToString();
		this.distance = Convert.ToDouble(Util.ChangeDecimalSeparator(eventString[4]));
		this.time = Convert.ToDouble(Util.ChangeDecimalSeparator(eventString[5]));
		this.description = eventString[6].ToString();
		this.simulated = Convert.ToInt32(eventString[7]);
		this.initialSpeed = Util.IntToBool(Convert.ToInt32(eventString[8]));
		this.datetime = eventString[9];
	}
	
	public static List<Event> RunListToEventList(List<Run> runs)
	{
		List<Event> events = new List<Event>();
		foreach(Run run in runs)
			events.Add((Event) run);

		return events;
	}


	public override int InsertAtDB (bool dbconOpened, string tableName) {
		return SqliteRun.Insert(dbconOpened, tableName, 
				uniqueID.ToString(), 
				personID, sessionID, 
				type, distance, time, 
				description, simulated, initialSpeed, datetime);
	}

	public override string ToString() {
		return uniqueID + ":" + personID + ":" + sessionID + ":" + type + ":" + distance + ":" + time + ":" + datetime + ":" + description + ":" + simulated + ":" + initialSpeed;
	}
	
	public virtual double Speed
	{
		get { 
			if(metersSecondsPreferred) {
				return distance / time ; 
			} else {
				return (distance / time) * 3.6 ; 
			}
		}
	}
	
	public double Distance
	{
		get { return distance; }
		set { distance = value; }
	}
	
	public double Time
	{
		get { return time; }
		set { time = value; }
	}

	public bool MetersSecondsPreferred {
		set { metersSecondsPreferred = value; }
	}
	
	public bool InitialSpeed
	{
		get { return initialSpeed; }
		set { initialSpeed = value; }
	}

	public string Datetime {
		get { return datetime; }
		set { datetime = value; }
	}

	
	~Run() {}
	   
}

public class RunInterval : Run
{
	double distanceTotal;
	double timeTotal;
	double distanceInterval;
	string intervalTimesString;
	double tracks; //double because if we limit by time (runType tracksLimited false), we do n.nn tracks
	string limited; //the teorically values, eleven runs: "11=R" (time recorded in "time"), 10 seconds: "10=T" (tracks recorded in tracks)
	private List<int> photocell_l;
	

	public RunInterval() {
	}
	
	//after inserting database (SQL)
	public RunInterval(int uniqueID, int personID, int sessionID, string type, double distanceTotal, double timeTotal, double distanceInterval, string intervalTimesString, double tracks, string description, string limited, int simulated, bool initialSpeed, string datetime, List<int> photocell_l)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.sessionID = sessionID;
		this.type = type;
		this.distanceTotal = distanceTotal;
		this.timeTotal = timeTotal;
		this.distanceInterval = distanceInterval;
		this.intervalTimesString = intervalTimesString;
		this.tracks = tracks;
		this.description = description;
		this.limited = limited;
		this.simulated = simulated;
		this.initialSpeed = initialSpeed;
		this.datetime = datetime;
		this.photocell_l = photocell_l;
	}

	//used to select a run at SqliteRun.SelectIntervalRunData and at Sqlite.convertTables
	public RunInterval(string [] eventString)
	{
		this.uniqueID = Convert.ToInt32(eventString[0]);
		this.personID = Convert.ToInt32(eventString[1]);
		this.sessionID = Convert.ToInt32(eventString[2]);
		this.type = eventString[3].ToString();
		this.distanceTotal = Convert.ToDouble(Util.ChangeDecimalSeparator(eventString[4]));
		this.timeTotal = Convert.ToDouble(Util.ChangeDecimalSeparator(eventString[5]));
		this.distanceInterval = Convert.ToDouble(Util.ChangeDecimalSeparator(eventString[6]));
		this.intervalTimesString = Util.ChangeDecimalSeparator(eventString[7]);
		this.tracks = Convert.ToDouble(Util.ChangeDecimalSeparator(eventString[8]));
		this.description = eventString[9].ToString();
		this.limited = eventString[10].ToString();
		this.simulated = Convert.ToInt32(eventString[11]);
		this.initialSpeed = Util.IntToBool(Convert.ToInt32(eventString[12]));
		this.datetime = eventString[13];
		this.photocell_l = Util.SQLStringToListInt(eventString[14], ";");
	}

	public static List<Event> RunIntervalListToEventList(List<RunInterval> runsI)
	{
		List<Event> events = new List<Event>();
		foreach(RunInterval runI in runsI)
			events.Add((Event) runI);

		return events;
	}


	public override int InsertAtDB (bool dbconOpened, string tableName) {
		return SqliteRunInterval.Insert(dbconOpened, tableName, 
				uniqueID.ToString(), 
				personID, sessionID, 
				type, distanceTotal, timeTotal, 
				distanceInterval, intervalTimesString,
				tracks, description, 
				limited, simulated, initialSpeed, datetime, photocell_l);
	}

	//to debug
	public override string ToString()
	{
		return string.Format("uniqueID: {0}, personID: {1}, sessionID: {2}, " +
				"type: {3}, distanceTotal: {4}, timeTotal: {5}, " +
				"distanceInterval: {6}, intervalTimesString: {7}, " +
				"tracks: {8}, description: {9}, " +
				"limited: {10}, simulated: {11}, initialSpeed: {12}, " +
				"datetime: {13}, photocell_l: {14}",
				uniqueID, personID, sessionID,
				type, distanceTotal, timeTotal,
				distanceInterval, intervalTimesString,
				tracks, description,
				limited, simulated, initialSpeed, datetime, photocell_l);
	}

	//this discards RSA
	public static string GetSprintPositions(double distanceInterval, string intervalTimesString, string distancesString)
	{
		string positions = "";
		string [] intervalTimesSplit = intervalTimesString.Split(new char[] {'='});
		if(! distancesString.Contains("R") ) 	//discard RSA
		{
			string sep = "";
			for(int i=0; i < intervalTimesSplit.Length; i ++)
			{
				positions += sep + Util.GetRunITotalDistance(distanceInterval, distancesString, i+1);
				sep = ";";
			}

			//format positions
			positions = Util.ChangeChars(positions, "-", ";");
		}
		return positions;
	}

	public static string GetSplitTimes(string intervalTimesString, int prefsDigitsNumber)
	{
		string [] intervalTimesSplit = intervalTimesString.Split(new char[] {'='});

		//manage accumulated time
		double timeAccumulated = 0;
		string splitTimes = "";
		string sep = "";
		foreach(string time in intervalTimesSplit)
		{
			double timeD = Convert.ToDouble(time);
			timeAccumulated += timeD;
			splitTimes += sep + Util.TrimDecimals(timeAccumulated, prefsDigitsNumber);
			sep = ";";
		}

		return splitTimes;
	}

	private List<double> timeList
	{
		get {
			List<double> l = new List<double>();
			string [] strFull = intervalTimesString.Split(new char[] {'='});
			foreach(string str in strFull)
			{
				if(Util.IsNumber(Util.ChangeDecimalSeparator(str), true))
					l.Add(Convert.ToDouble(Util.ChangeDecimalSeparator(str)));
			}
			return l;
		}
	}
	public double TimeLast
	{
		get {
			if(timeList == null || timeList.Count == 0)
				return 0;
			else
				return timeList[timeList.Count -1];
		}
	}

	public static string GetCSVInputMulti() {
		return Path.Combine(Path.GetTempPath(), "sprintInputMulti.csv");
	}

	public static string GetCSVResultsFileName() {
		return "sprintResults.csv";
	}
	public static string GetCSVResultsURL() {
		return Path.Combine(Path.GetTempPath(), GetCSVResultsFileName());
	}

	public string IntervalTimesString
	{
		get { return intervalTimesString; }
		set { intervalTimesString = value; }
	}
	
	public double DistanceInterval
	{
		get { return distanceInterval; }
		set { distanceInterval = value; }
	}
		
	public double DistanceTotal
	{
		get { return distanceTotal; }
		set { distanceTotal = value; }
	}
		
	public double TimeTotal
	{
		get { return timeTotal; }
		set { timeTotal = value; }
	}
		
	public double Tracks
	{
		get { return tracks; }
		set { tracks = value; }
	}
	
	public string Limited
	{
		get { return limited; }
		set { limited = value; }
	}

	public bool StartIn
	{
		get { return startIn; }
	}
		
	public override double Speed
	{
		get { 
			//if(metersSecondsPreferred) {
				return distanceTotal / timeTotal ; 
			/*} else {
				return (distanceTotal / timeTotal) * 3.6 ; 
			}
			*/
		}
	}

	public List<int> Photocell_l
	{
		get { return photocell_l; }
	}
	/*
	public string Photocell_l_str
	{
		get { return Util.ListIntToSQLString (photocell_l, ";"); }
	}
	*/
		
	~RunInterval() {}
}

