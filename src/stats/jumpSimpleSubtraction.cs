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


public class StatJumpSimpleSubtraction : Stat
{
	protected string test1;
	protected string test2;
	protected string [] columnsString = new String[5];
	
	//if this is not present i have problems like (No overload for method `xxx' takes `0' arguments) with some inherited classes
	public StatJumpSimpleSubtraction () 
	{
		this.showSex = false;
		this.statsJumpsType = 0;
		this.limit = 0;
	}

	public StatJumpSimpleSubtraction (StatTypeStruct myStatTypeStruct, Gtk.TreeView treeview)
	{
		completeConstruction (myStatTypeStruct, treeview);
		
		this.dataColumns = 4;	//for simplesession (result %, result, test1, test2)


		string [] applyTos = myStatTypeStruct.StatisticApplyTo.Split(new char[] {','});
		test1 = applyTos[0];
		test2 = applyTos[1];
		
		if(sessions.Count > 1) {
			store = getStore(sessions.Count +3); //+3 (for jumper, the AVG horizontal and SD horizontal)
		} else {
			store = getStore(dataColumns +1); //jumper, result %, result, test1, test2
		}
		
		columnsString[0] = Catalog.GetString("Jumper");
		columnsString[1] = Catalog.GetString("Result") + " %";
		columnsString[2] = Catalog.GetString("Result");
		columnsString[3] = test1;
		columnsString[4] = test2;
		
		if(toReport) {
			reportString = prepareHeadersReport(columnsString);
		} else {
			treeview.Model = store;
			prepareHeaders(columnsString);
		}
	}
	
	public override void PrepareData() 
	{
		string sessionString = obtainSessionSqlStringTwoTests(sessions);
		bool multisession = false;
		if(sessions.Count > 1) {
			multisession = true;
		}

		if(statsJumpsType == 3) { //avg of each jumper
			if(multisession) {
				processDataMultiSession ( 
						SqliteStat.JumpSimpleSubtraction(sessionString, multisession, "AVG(", ")", test1, test2, showSex), 
						true, sessions.Count);
			} else {
				processDataSimpleSession ( cleanDontWanted (
							SqliteStat.JumpSimpleSubtraction(sessionString, multisession, "AVG(", ")", test1, test2, showSex), 
							statsJumpsType, limit),
						true, dataColumns);
			}
		} else {
			//if more than on session, show only the avg or max of each jump/jumper
			//some of this options are never called becase we don't allow radiobutton all and limit (only avg and best)
			if(multisession) {
				processDataMultiSession ( SqliteStat.JumpSimpleSubtraction(sessionString, multisession, "MAX(", ")", test1, test2, showSex),  
						true, sessions.Count);
			} else {
				processDataSimpleSession ( SqliteStat.JumpSimpleSubtraction(sessionString, multisession, "MAX(", ")", test1, test2, showSex), 
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

		return string.Format(Catalog.GetString("{0} in test {1} - test {2} on {3}"), selectedValuesString, test1, test2, mySessionString);
	}
}
