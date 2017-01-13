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


public class StatJumpIndexes : Stat
{
	protected string indexType;
	protected string jump1;
	protected string jump2;
	protected string [] columnsString = new String[4];
	
	//if this is not present i have problems like (No overload for method `xxx' takes `0' arguments) with some inherited classes
	public StatJumpIndexes () 
	{
		this.showSex = false;
		this.statsJumpsType = 0;
		this.limit = 0;
	}

	public StatJumpIndexes (StatTypeStruct myStatTypeStruct, Gtk.TreeView treeview, string indexType)
	{
		completeConstruction (myStatTypeStruct, treeview);
		
		this.dataColumns = 3;	//for simplesession (IE, cmj, sj)
		this.indexType = indexType; //"IE", Constants.ArmsUseIndexName, "IRna", "IRa"
		if(indexType == "IE") {
			jump1="CMJ";
			jump2="SJ";
		} else if(indexType == Constants.ArmsUseIndexName) {
			jump1="ABK";
			jump2="CMJ";
		} else if(indexType == "IRna") { //reactivity no arms
			jump1="DJna";
			jump2="CMJ";
		} else { //IRa //reactivity with arms
			jump1="DJa";
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
		if(useHeightsOnJumpIndexes)
			columnsString[2] = jump1 + " (" + Catalog.GetString("height") + ")";

		columnsString[3] = jump2;
		if(useHeightsOnJumpIndexes)
			columnsString[3] = jump2 + " (" + Catalog.GetString("height") + ")";
		
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
						SqliteStat.JumpIndexes(sessionString, multisession, "AVG(", ")", jump1, jump2, showSex, useHeightsOnJumpIndexes),
						true, sessions.Count);
			} else {
				processDataSimpleSession ( cleanDontWanted (
							SqliteStat.JumpIndexes(sessionString, multisession, "AVG(", ")", jump1, jump2, showSex, useHeightsOnJumpIndexes),
							statsJumpsType, limit),
						true, dataColumns);
			}
		} else {
			//if more than on session, show only the avg or max of each jump/jumper
			//some of this options are never called becase we don't allow radiobutton all and limit (only avg and best)
			if(multisession) {
				processDataMultiSession ( SqliteStat.JumpIndexes(sessionString, multisession, "MAX(", ")", jump1, jump2, showSex, useHeightsOnJumpIndexes),
						true, sessions.Count);
			} else {
				processDataSimpleSession ( SqliteStat.JumpIndexes(sessionString, multisession, "MAX(", ")", jump1, jump2, showSex, useHeightsOnJumpIndexes),
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

		string indexString = "IE [(cmj-sj)/sj * 100]";
		if(indexType == Constants.ArmsUseIndexName) {
			indexString = Constants.ArmsUseIndexName + " [(abk-cmj)/cmj * 100]";
		} else if(indexType == "IRna") {
			indexString = "IRna [(djna-cmj)/cmj * 100]";
		} else if(indexType == "IRa") {
			indexString = "IRa [(dja-cmj)/cmj * 100]";
		}
		return string.Format(Catalog.GetString("{0} in index {1} on {2}"), selectedValuesString, indexString, mySessionString);
	}
}
