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
 * Xavier de Blas: 
 * http://www.xdeblas.com, http://www.deporteyciencia.com (parleblas)
 */

using System;
using System.Data;

using Mono.Unix;

public class Pulse : Event
{
	double fixedPulse;
	int totalPulsesNum;

	string timesString;
	int tracks;
	double contactTime;

	//used on treeviewPulse
	public Pulse() {
	}

	//after inserting database (SQL)
	public Pulse(int uniqueID, int personID, int sessionID, string type, double fixedPulse, 
			int totalPulsesNum, string timesString, string description)
	{
		this.uniqueID = uniqueID;
		this.personName = SqlitePerson.SelectJumperName(personID);
		this.personID = personID;
		this.sessionID = sessionID;
		this.type = type;
		this.fixedPulse = fixedPulse;
		this.totalPulsesNum = totalPulsesNum;
		this.timesString = timesString;
		this.description = description;
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
	
	public string TimesString
	{
		get { return timesString; }
		set { timesString = value; }
	}
	
	public double FixedPulse
	{
		get { return fixedPulse; }
		set { fixedPulse = value; }
	}
	
		
	~Pulse() {}
}

