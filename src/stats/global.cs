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


public class StatGlobal : Stat
{
	protected int personID;
	protected string personName;
	protected string operation;
	
	public StatGlobal ()
	{
	}

	public StatGlobal (Gtk.TreeView treeview, ArrayList sessions, int personID, string personName, int newPrefsDigitsNumber, bool showSex, int statsJumpsType, bool heightPreferred) 
	{
		if(sessions.Count > 1) {
			store = getStore(sessions.Count +3); //+3 (for the statName, the AVG horizontal and SD horizontal
		} else {
			store = getStore(sessions.Count +1);
		}
		treeview.Model = store;
		
		this.personID = personID;
		this.personName = personName;
		this.heightPreferred = heightPreferred;
		
		completeConstruction (treeview, sessions, newPrefsDigitsNumber, showSex, statsJumpsType);
		string [] columnsString = { Catalog.GetString("Jump"), Catalog.GetString("Value") };
		prepareHeaders(columnsString);
		
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
				
		processDataMultiSession ( SqliteStat.GlobalNormal(sessionString, operation, showSex, 
					personID, heightPreferred), 
				true, sessions.Count );
		
		//currently disabled GlobalIndexes in stats global
		//only for showing less info in global.
		//If enable another time, remember to create a GlobalIndexes for IndexQ, FV an others
		/*
		processDataMultiSession ( SqliteStat.GlobalOthers("DjIndex", "(100*((tv-tc)/tc))", "jump", "DJ", 
					sessionString, operation, showSex, personID),
				false, sessions.Count );
		processDataMultiSession ( SqliteStat.GlobalOthers("RjIndex", "(100*((tvavg-tcavg)/tcavg))", "jumpRj", "RJ", 
					sessionString, operation, showSex, personID),
				false, sessions.Count );
		processDataMultiSession ( SqliteStat.GlobalOthers("RjPotency", 
					"(9.81*9.81 * tvavg*jumps * time / (4*jumps*(time - tvavg*jumps)) )", "jumpRj", "RJ", 
					sessionString, operation, showSex, personID),
				false, sessions.Count );
		
		//session string must be different for indexes
		sessionString = obtainSessionSqlStringIndexes(sessions);
		
		processDataMultiSession ( SqliteStat.GlobalIndexes("IE", "CMJ", "SJ", 
					sessionString, operation, showSex, personID),
				false, sessions.Count );
		processDataMultiSession ( SqliteStat.GlobalIndexes("IUB", "ABK", "CMJ", 
					sessionString, operation, showSex, personID),
				false, sessions.Count );
		*/
	}

	public override string ToString () 
	{
		/*
		string personString = "";
		if (personID != -1) {
			personString = Catalog.GetString(" of '") + personName + Catalog.GetString("' jumper");
		}
			
		string showSexString = "";
		if (this.showSex) { showSexString = " " + Catalog.GetString("sorted by sex"); }
		
		string inSessions = Catalog.GetString(" in sessions: ");
		for (int i=0; i < sessions.Count ;i++) {
			string [] str = sessions[i].ToString().Split(new char[] {':'});
			inSessions = inSessions + "'" + str[1] + "' (" + str[2] + ")";
			if(i + 1 < sessions.Count) {
				inSessions = inSessions + ", ";
			}
		}
	
		if ( this.operation == "MAX" ) { 
			return Catalog.GetString("MAX values of some jumps and statistics") + personString + inSessions + showSexString + "."  ; 
		} else {
			return Catalog.GetString("AVG values of some jumps and statistics") + personString + inSessions + showSexString + "."  ; 
		}
		*/

		return "pending";
	}
}
