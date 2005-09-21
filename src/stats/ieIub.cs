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


public class StatIeIub : Stat
{
	protected string indexType;
	protected string jump1;
	protected string jump2;
	protected string [] columnsString = new String[4];
	
	//if this is not present i have problems like (No overload for method `xxx' takes `0' arguments) with some inherited classes
	public StatIeIub () 
	{
		this.showSex = false;
		this.statsJumpsType = 0;
		this.limit = 0;
	}

	public StatIeIub (StatTypeStruct myStatTypeStruct, Gtk.TreeView treeview, string indexType)
	{
		completeConstruction (myStatTypeStruct, treeview);
		
		this.dataColumns = 3;	//for simplesession (IE, cmj, sj)
		this.indexType = indexType; //"IE" or "IUB" or "FV"

		if(indexType == "IE") {
			jump1="CMJ";
			jump2="SJ";
		} else { //IUB
			jump1="ABK";
			jump2="CMJ";
		}
		
		if(sessions.Count > 1) {
			store = getStore(sessions.Count +3); //+3 (for jumper, the AVG horizontal and SD horizontal)
		} else {
			store = getStore(dataColumns +1); //jumper, IE, cmj, sj
		}
		
		columnsString[0] = Catalog.GetString("Jumper");
		columnsString[1] = indexType;
		columnsString[2] = jump1;
		columnsString[3] = jump2;
		
		if(toReport) {
			reportString = prepareHeadersReport(columnsString);
		} else {
			treeview.Model = store;
			prepareHeaders(columnsString);
		}
	}
	
	//session string must be different for indexes
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
		string sessionString = obtainSessionSqlStringIndexes(sessions);
		bool multisession = false;
		if(sessions.Count > 1) {
			multisession = true;
		}

		if(statsJumpsType == 3) { //avg of each jumper
			if(multisession) {
				processDataMultiSession ( 
						SqliteStat.IeIub(sessionString, multisession, "AVG(", ")", jump1, jump2, showSex), 
						true, sessions.Count);
			} else {
				processDataSimpleSession ( cleanDontWanted (
							SqliteStat.IeIub(sessionString, multisession, "AVG(", ")", jump1, jump2, showSex), 
							statsJumpsType, limit),
						true, dataColumns);
			}
		} else {
			//if more than on session, show only the avg or max of each jump/jumper
			//some of this options are never called becase we don't allow radiobutton all and limit (only avg and best)
			if(multisession) {
				processDataMultiSession ( SqliteStat.IeIub(sessionString, multisession, "MAX(", ")", jump1, jump2, showSex),  
						true, sessions.Count);
			} else {
				processDataSimpleSession ( SqliteStat.IeIub(sessionString, multisession, "MAX(", ")", jump1, jump2, showSex), 
						true, dataColumns);
			}
		}
	}
		
	public override string ToString () 
	{
		string selectedValuesString = "";
		if(statsJumpsType == 0) { //all jumps
			selectedValuesString = allValuesString; 
		} else if(statsJumpsType == 1) { //limit
			selectedValuesString = string.Format(Catalog.GetString("First {0} values"), limit); 
		} else if(statsJumpsType == 2) { //best of each jumper
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

		string indexString = "IE [(cmj-sj)/sj * 100]";
		if(indexType == "IUB") {
			indexString = "IUB [(abk-cmj)/cmj * 100]";
		}
		return string.Format(Catalog.GetString("{0} in index {1} on {2}"), selectedValuesString, indexString, mySessionString);
	}
}
