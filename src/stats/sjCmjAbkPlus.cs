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
 * Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using Gtk;
using System.Collections; //ArrayList
using Mono.Unix;


//no sj+
public class StatSjCmjAbkPlus : Stat
{
	//protected bool percent = false; //show weight in simplesession as a percent of body weight or kg
	
	//if this is not present i have problems like (No overload for method `xxx' takes `0' arguments) with some inherited classes
	
	protected bool weightPercentPreferred;

	public StatSjCmjAbkPlus () 
	{
		this.showSex = false;
		this.statsJumpsType = 0;
		this.limit = 0;
	}

	 public StatSjCmjAbkPlus (StatTypeStruct myStatTypeStruct, Gtk.TreeView treeview) 
	{
		completeConstruction (myStatTypeStruct, treeview);
		
		this.dataColumns = 3;	//for simplesession (height, tv, weight)
		
		if(sessions.Count > 1) {
			store = getStore(sessions.Count +3); //+3 (for jumper, the AVG horizontal and SD horizontal)
		} else {
			store = getStore(dataColumns +1); //jumper, height, TF, weight (this col has characters '%' and 'Kg') solved in sqlite
		}
		
		weightPercentPreferred = myStatTypeStruct.preferences.weightStatsPercent;
		string weightName = Catalog.GetString("Extra weight");
		if(weightPercentPreferred)
			weightName += " %";
		else
			weightName += " Kg";

		string [] columnsString = { Catalog.GetString("Jumper"), Catalog.GetString("Height"), 
			Catalog.GetString("TF"), weightName };
		/*
		if(! percent) {
			columnsString[3] = Catalog.GetString("Weight Kg");
		}
		*/
		
		if(toReport) {
			reportString = prepareHeadersReport(columnsString);
		} else {
			treeview.Model = store;
			prepareHeaders(columnsString);
		}
	}

	
	public override void PrepareData() 
	{
		string sessionString = obtainSessionSqlString(sessions, "jump");
		bool multisession = false;
		if(sessions.Count > 1) {
			multisession = true;
		}

		if(statsJumpsType == 3) { //avg of each jumper
			if(multisession) {
				string operation = "AVG";
				processDataMultiSession ( 
						SqliteStat.SjCmjAbkPlus(sessionString, multisession, 
							//operation, jumpType, showSex, percent, heightPreferred), 
							operation, jumpType, showSex, heightPreferred, weightPercentPreferred), 
						true, sessions.Count);
			} else {
				string operation = "AVG";
				processDataSimpleSession ( cleanDontWanted (
							SqliteStat.SjCmjAbkPlus(sessionString, multisession, 
								//operation, jumpType, showSex, percent, heightPreferred), 
								operation, jumpType, showSex, heightPreferred, weightPercentPreferred), 
							statsJumpsType, limit),
						true, dataColumns);
			}
		} else {
			//if more than on session, show only the avg or max of each jump/jumper
			if(multisession) {
				string operation = "MAX";
				processDataMultiSession ( SqliteStat.SjCmjAbkPlus(sessionString, multisession, 
							//operation, jumpType, showSex, percent, heightPreferred),  
							operation, jumpType, showSex, heightPreferred, weightPercentPreferred),  
						true, sessions.Count);
			} else {
				string operation = ""; //no need of "MAX", there's an order by jump.tv desc
							//and clenaDontWanted will do his work
				processDataSimpleSession ( cleanDontWanted (
							SqliteStat.SjCmjAbkPlus(sessionString, multisession, 
								//operation, jumpType, showSex, percent, heightPreferred), 
								operation, jumpType, showSex, heightPreferred, weightPercentPreferred), 
							statsJumpsType, limit),
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
			selectedValuesString = string.Format(Catalog.GetPluralString(
						"First value", "First {0} values", limit), limit);
		} else if(statsJumpsType == 2) { //best of each jumper
			selectedValuesString = string.Format(Catalog.GetPluralString(
						"Max value of each person", "Max {0} values of each person", limit), limit);
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

		return string.Format(Catalog.GetString("{0} in {1} on {2}"), selectedValuesString, jumpType, mySessionString);
	}

}

