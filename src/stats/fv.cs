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


public class StatFv : StatJumpIndexes
{
	public StatFv () {
	}

	public StatFv (StatTypeStruct myStatTypeStruct, Gtk.TreeView treeview, string indexType)
	{
		completeConstruction (myStatTypeStruct, treeview);

		this.dataColumns = 3;	//for simplesession (FV, cmj, sj)
		this.indexType = indexType; // "FV"

		jump1="SJl";
		jump2="SJ";
		
		if(sessions.Count > 1) {
			store = getStore(sessions.Count +3); //+3 (for jumper, the AVG horizontal and SD horizontal)
		} else {
			store = getStore(dataColumns +1); //jumper, FV, cmj, sj
		}
		
		columnsString[0] = Catalog.GetString("Jumper");
		columnsString[1] = indexType;
		columnsString[2] = jump1 + " (" + Catalog.GetString("height") + ")";
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
						SqliteStat.Fv(sessionString, multisession, "AVG(", ")", jump1, jump2, showSex), 
						true, sessions.Count);
			} else {
				processDataSimpleSession ( cleanDontWanted (
							SqliteStat.Fv(sessionString, multisession, "AVG(", ")", jump1, jump2, showSex), 
							statsJumpsType, limit),
						true, dataColumns);
			}
		} else {
			//if more than on session, show only the avg or max of each jump/jumper
			//FIXME: indexes max value have two possibilities:
			//max jump1, max jump2 (seems more real)
			//max jump1, min jump2 (index goes greater)
			if(multisession) {
				processDataMultiSession ( SqliteStat.Fv(sessionString, multisession, "MAX(", ")", jump1, jump2, showSex),  
						true, sessions.Count);
			} else {
				processDataSimpleSession ( SqliteStat.Fv(sessionString, multisession, "MAX(", ")", jump1, jump2, showSex), 
						true, dataColumns);
			}
		}
	}
		
	public override string ToString () 
	{
		string selectedValuesString = "";
		
		/* this options are not possible in this index
		 *
		if(statsJumpsType == 0) { //all jumps
			selectedValuesString = allValuesString; 
		} else if(statsJumpsType == 1) { //limit
			selectedValuesString = string.Format(Catalog.GetString("First {0} values"), limit); 
		*/
		
		if(statsJumpsType == 2) { //best of each jumper
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

		return string.Format(Catalog.GetString("{0} in Index FV [SJl(100%)/SJ *100] on {1}"), selectedValuesString, mySessionString);
	}
}
