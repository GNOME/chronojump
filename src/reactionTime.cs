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
 *  Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;

public class ReactionTime : Event 
{
	protected double time;

	public ReactionTime() {
	}

	//after inserting database (SQL)
	public ReactionTime(int uniqueID, int personID, int sessionID, string type, double time, string description, int simulated)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.sessionID = sessionID;
		this.type = type;
		this.time = time;
		this.description = description;
		this.simulated = simulated;
	}

	//used to select a event at SqliteReactionTime.SelectReactionTimeData and at Sqlite.convertTables
	public ReactionTime(string [] eventString) {
		this.uniqueID = Convert.ToInt32(eventString[0]);
		this.personID = Convert.ToInt32(eventString[1]);
		this.sessionID = Convert.ToInt32(eventString[2]);
		this.type = eventString[3].ToString();
		this.time = Convert.ToDouble(Util.ChangeDecimalSeparator(eventString[4]));
		this.description = eventString[5].ToString();
		this.simulated = Convert.ToInt32(eventString[6]);
	}

	public override int InsertAtDB (bool dbconOpened, string tableName) {
		return SqliteReactionTime.Insert(dbconOpened, tableName, 
				uniqueID.ToString(), 
				personID, sessionID, 
				type, time,
				description, simulated);
	}


	public double Time
	{
		get { return time; }
		set { time = value; }
	}
	
	~ReactionTime() {}
}
