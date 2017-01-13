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

using Mono.Unix;

public class Pulse : Event
{
	double fixedPulse;
	int totalPulsesNum;

	string timesString;

	//used on treeviewPulse
	public Pulse() {
	}

	//after inserting database (SQL)
	public Pulse(int uniqueID, int personID, int sessionID, string type, double fixedPulse, 
			int totalPulsesNum, string timesString, string description, int simulated)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.sessionID = sessionID;
		this.type = type;
		this.fixedPulse = fixedPulse;
		this.totalPulsesNum = totalPulsesNum;
		this.timesString = timesString;
		this.description = description;
		this.simulated = simulated;
	}

	//used to select a event at SqlitePulse.SelectPulseData and at Sqlite.convertTables
	public Pulse(string [] eventString) {
		this.uniqueID = Convert.ToInt32(eventString[0]);
		this.personID = Convert.ToInt32(eventString[1]);
		this.sessionID = Convert.ToInt32(eventString[2]);
		this.type = eventString[3].ToString();
		this.fixedPulse = Convert.ToDouble(Util.ChangeDecimalSeparator(eventString[4]));
		this.totalPulsesNum = Convert.ToInt32(eventString[5]);
		this.timesString = eventString[6].ToString();
		this.description = eventString[7].ToString();
		this.simulated = Convert.ToInt32(eventString[8]);
	}

	public override int InsertAtDB (bool dbconOpened, string tableName) {
		return SqlitePulse.Insert(dbconOpened, tableName, 
				uniqueID.ToString(), 
				personID, sessionID, 
				type, fixedPulse, totalPulsesNum, timesString,
				description, simulated);
	}

	
	//called from treeViewPulse
	public double GetErrorAverage(bool relative)
	{
		double pulseToComparate = Convert.ToDouble(Util.GetAverage(timesString));
		string [] myStringFull = timesString.Split(new char[] {'='});
		string myErrors = "";
		string separatorString = "";
		double error = 0;
		
		foreach (string myPulse in myStringFull) {
			if(relative)
				error = (Convert.ToDouble(myPulse) - pulseToComparate) *100 / pulseToComparate; 
			else
				error = Convert.ToDouble(myPulse) - pulseToComparate;

			//all the values should be positive
			if (error < 0)
				error = error * -1;
			
			myErrors += separatorString + error.ToString();
			separatorString = "=";
		}
		return Util.GetAverage(myErrors);
	}
	
	public double FixedPulse {
		get { return fixedPulse; }
		set { fixedPulse = value; }
	}
	
	public int TotalPulsesNum {
		set { totalPulsesNum = value; }
	}
	
	public string TimesString {
		get { return timesString; }
		set { timesString = value; }
	}
	
		
	~Pulse() {}
}

