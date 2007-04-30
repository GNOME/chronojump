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
using Gtk;
using System.Collections; //ArrayList
using Mono.Unix;


public class StatGlobal : Stat
{
	protected int personID;
	protected string personName;
	protected string operation;
	
	public StatGlobal ()
	{
	}

	public StatGlobal (StatTypeStruct myStatTypeStruct, Gtk.TreeView treeview, int personID, string personName) 
	{
		completeConstruction (myStatTypeStruct, treeview);
		
		if(sessions.Count > 1) {
			store = getStore(sessions.Count +3); //+3 (for the statName, the AVG horizontal and SD horizontal
		} else {
			store = getStore(sessions.Count +1);
		}
		
		this.personID = personID;
		this.personName = personName;
		//this.heightPreferred = heightPreferred;
		
		string [] columnsString = { Catalog.GetString("Jump"), Catalog.GetString("Value") };
		
		if(toReport) {
			reportString = prepareHeadersReport(columnsString);
		} else {
			treeview.Model = store;
			prepareHeaders(columnsString);
		}
		
		if (statsJumpsType == 2) {
			this.operation = "MAX";
		} else {
			this.operation = "AVG";
		}
	}

	protected string obtainSessionSqlStringIndexes(ArrayList sessions)
	{
		string newStr = "WHERE (";
		for (int i=0; i < sessions.Count; i++) {
			string [] stringFullResults = sessions[i].ToString().Split(new char[] {':'});
			newStr = newStr + " (j1.sessionID == " + stringFullResults[0] + 
				" AND j2.sessionID == " + stringFullResults[0] + ")";
			if (i+1 < sessions.Count) {
				newStr = newStr + " OR ";
			}
		}
		newStr = newStr + ") ";
		return newStr;		
	}
	

	public override void PrepareData() 
	{
		string sessionString = obtainSessionSqlString(sessions);
				
		//it's better (nicer, cleaner, easier) to leave all the AVGSD as 'false'
		processDataMultiSession ( SqliteStat.GlobalNormal(sessionString, operation, showSex, 
					personID, heightPreferred), 
				false, sessions.Count );
		
		//session string must be different for indexes
		sessionString = obtainSessionSqlStringIndexes(sessions);
		
		processDataMultiSession ( SqliteStat.GlobalIndexes("IE", "CMJ", "SJ", 
					sessionString, operation, showSex, personID),
				false, sessions.Count );
		processDataMultiSession ( SqliteStat.GlobalIndexes("IUB", "ABK", "CMJ", 
					sessionString, operation, showSex, personID),
				false, sessions.Count );
		processDataMultiSession ( SqliteStat.GlobalIndexes("FV", "SJl", "SJ", 
					sessionString, operation, showSex, personID),
				false, sessions.Count );
		
		sessionString = obtainSessionSqlString(sessions);
		processDataMultiSession ( SqliteStat.GlobalOthers("DjIndex", "(100*((tv-tc)/tc))", "jump", 
					sessionString, operation, showSex, personID),
				false, sessions.Count );
		processDataMultiSession ( SqliteStat.GlobalOthers("IndexQ", "(tv/tc)", "jump", 
					sessionString, operation, showSex, personID),
				false, sessions.Count );
		processDataMultiSession ( SqliteStat.GlobalOthers("RjIndex", "(100*((tvavg-tcavg)/tcavg))", "jumpRj", 
					sessionString, operation, showSex, personID),
				false, sessions.Count );
		processDataMultiSession ( SqliteStat.GlobalOthers("RjPotency", 
					"(9.81*9.81 * tvavg*jumps * time / (4*jumps*(time - tvavg*jumps)) )", "jumpRj",
					sessionString, operation, showSex, personID),
				false, sessions.Count );
	
	}

	public override string ToString () 
	{
		string selectedValuesString = "";
		
		/* this options are not possible in this statistic
		 * 
		if(statsJumpsType == 0) { //all jumps
			selectedValuesString = allValuesString; 
		} else if(statsJumpsType == 1) { //limit
			selectedValuesString = string.Format(Catalog.GetString("First {0} values"), limit); 
		*/
		
		if(statsJumpsType == 2) { //best of each jumper
			selectedValuesString = string.Format(Catalog.GetString("Max {0} values of each jumper"), limit);
		} else if(statsJumpsType == 3) { //avg of each jumper
			selectedValuesString = avgValuesString; 
		}  

		string mySessionString = "";
		if(sessions.Count > 1) {
			mySessionString =  Catalog.GetString (" various sessions "); 
		} else {
			string [] strFull = sessions[0].ToString().Split(new char[] {':'});
			mySessionString =  Catalog.GetString (" session ") + 
				strFull[0] + "(" + strFull[2] + ")";
		}

		//if is a stat of a concrete jumper, show it in enunciate
		string myPersonString = "";
		if (personID != -1) {
			myPersonString = string.Format(Catalog.GetString(" for person {0}({1})"), personName, personID);
		}
		
		return string.Format(Catalog.GetString("{0} in some jumps and statistics on {1}{2}"), selectedValuesString, mySessionString, myPersonString);
	}
}
