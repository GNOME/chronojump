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
using System.Collections.Generic; //List

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
