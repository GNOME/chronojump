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
 *  Copyright (C) 2004-2009   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;

using System.Threading;
using Mono.Unix;

public class Run : Event 
{
	protected double distance;
	protected double time;

	//for not checking always in database
	protected bool startIn;
	
	
	//protected Chronopic cp;
	protected bool metersSecondsPreferred;

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
	public Run(int uniqueID, int personID, int sessionID, string type, double distance, double time, string description, int simulated)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.sessionID = sessionID;
		this.type = type;
		this.distance = distance;
		this.time = time;
		this.description = description;
		this.simulated = simulated;
	}

	//used to select a run at SqliteRun.SelectNormalRunData and at Sqlite.convertTables
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
	}
	

	public override int InsertAtDB (bool dbconOpened, string tableName) {
		return SqliteRun.Insert(dbconOpened, tableName, 
				uniqueID.ToString(), 
				personID, sessionID, 
				type, distance, time, 
				description, simulated);
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
	double limitAsDouble;	//-1 for non limited (unlimited repetitive run until "finish" is clicked)
	bool tracksLimited;
	

	public RunInterval() {
	}
	
	//after inserting database (SQL)
	public RunInterval(int uniqueID, int personID, int sessionID, string type, double distanceTotal, double timeTotal, double distanceInterval, string intervalTimesString, double tracks, string description, string limited, int simulated)
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
	}
	

	public override int InsertAtDB (bool dbconOpened, string tableName) {
		return SqliteRunInterval.Insert(dbconOpened, tableName, 
				uniqueID.ToString(), 
				personID, sessionID, 
				type, distanceTotal, timeTotal, 
				distanceInterval, intervalTimesString,
				tracks, description, 
				limited, simulated);
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
	
	public bool TracksLimited
	{
		get { return tracksLimited; }
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
	
		
		
	~RunInterval() {}
}

