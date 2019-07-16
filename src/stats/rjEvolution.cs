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


public class StatRjEvolution : Stat
{
	protected int maxJumps;
	protected string [] columnsString;

	//if this is not present i have problems like (No overload for method `xxx' takes `0' arguments) with some inherited classes
	public StatRjEvolution () 
	{
		this.showSex = false;
		this.statsJumpsType = 0;
		this.limit = 0;
	}

	public StatRjEvolution (StatTypeStruct myStatTypeStruct, int numContinuous, Gtk.TreeView treeview) 
	{
		completeConstruction (myStatTypeStruct, treeview);

		this.numContinuous = numContinuous;
		
		string sessionString = obtainSessionSqlString(sessions, "jumpRj");

		//we need to know the reactive with more jumps for prepare columns
		maxJumps = SqliteStat.ObtainMaxNumberOfJumps(sessionString);
		
		this.dataColumns = maxJumps*2 + 2;	//for simplesession (index, fall, (tv , tc)*jumps)

		//only simplesession
		store = getStore(dataColumns +1); //jumper, datacolumns 
		string [] columnsString;
	      
	       //in report, show only 5 TCs and 5 TFs every row, 
	       //if there are more jumps to show, let's cut them
		if(toReport && maxJumps > 5 ) {
			columnsString = new String[14]; //jumper, index, fall, count, 5 tc+tv
		} else {
			columnsString =	new String[dataColumns +1];
		}
		columnsString[0] = Catalog.GetString("Jumper");
		columnsString[1] = Catalog.GetString("Index");
		columnsString[2] = Catalog.GetString("Fall");

		if(toReport && maxJumps > 5) {
			columnsString[3] = Catalog.GetString("Count");
			for(int i=0; i < maxJumps && i < 5; i++) {
				columnsString[(i+1)*2 +2] = Catalog.GetString("TC"); //cols: 4, 6, 8, ...
				columnsString[(i+1)*2 +3] = Catalog.GetString("TF"); //cols: 5, 7, 9, ...
			}
		} else {
			for(int i=0; i < maxJumps; i++) {
				columnsString[(i+1)*2 +1] = Catalog.GetString("TC") + (i+1).ToString(); //cols: 3, 5, 7, ...
				columnsString[(i+1)*2 +2] = Catalog.GetString("TF") + (i+1).ToString(); //cols: 4, 6, 8, ...
			}
		}
		
		if(toReport) {
			reportString = prepareHeadersReport(columnsString);
		} else {
			treeview.Model = store;
			prepareHeaders(columnsString);
		}
	}

		
	protected int findBestContinuous(string [] statValues, int numContinuous)
	{
		//if numContinuous is 3, we check the best consecutive three tc,tv values
		int bestPos=-1;	//position where the three best pair values start
				//will return -1 if less tha three jumps
		double bestCount=-10000;	//best value found of 3 pairs

		//read all values in pairs tc,tv
		//start in pos 3 because first is name, second is index, third fall
		//end in Length-numContinuous*2 because we should not count only the last tc,tv pair or the last two, only the last three
		for ( int i=3; i <= statValues.Length -numContinuous*2 ; i=i+2 ) 
		{
			double myCount = 0;
			bool jumpFinished = false;
			//read the n consecutive values 
			for (int j=i; j < i + numContinuous*2 ; j=j+2 )
			{
				if( statValues[j] == "-" || statValues[j+1] == "-" ) {
					jumpFinished = true;
					break;
				}
				double tc = Convert.ToDouble(statValues[i]);
				double tv = Convert.ToDouble(statValues[i+1]);
				myCount += (tv * 100) / tc;
			}
			
			//LogB.Information("i{0}, myCount{1}, bestCount{2}", i, myCount, bestCount);
			//if found a max, record it
			if(myCount > bestCount && !jumpFinished) {
				bestCount = myCount;
				bestPos = i;
				//LogB.Information("best i{0}", i);
			}
		}
		return bestPos;
	}

	protected string [] markBestContinuous(string [] statValues, int numContinuous, int bestPos) {
		if(toReport) {
			for ( int i=0; i < statValues.Length ; i=i+2 ) {

				if(i >= bestPos && i < bestPos+numContinuous*2) {
					//LogB.Information("i{0}, bp{1}, svi{2}, svi+1{3}", i, bestPos, statValues[i], statValues[i+1]);
					statValues[i] = "<font color=\"red\">" + statValues[i] + "</font>";
					statValues[i+1] = "<font color=\"red\">" + statValues[i+1] + "</font>";
				}
			}
		} else {
			// this marks the first and the last with '[' and ']'
			statValues[bestPos] = "[" + statValues[bestPos];
			statValues[bestPos + (numContinuous*2) -1] = statValues[bestPos + (numContinuous*2) -1] + "]";
		}
		
		return statValues;
	}
	
	//for stripping off unchecked rows in report
	//private int rowsPassedToReport = 1;
	private int rowsPassedToReport = 0;
	
	protected override void printData (string [] statValues) 
	{
		if(numContinuous != -1) {
			int bestPos = findBestContinuous(statValues, numContinuous);
			if(bestPos != -1) {
				statValues = markBestContinuous(statValues, numContinuous, bestPos);
			}
		}
		
		if(toReport) {
			bool allowedRow = false;
			for(int i=0; i < markedRows.Count; i++) {
				if(Convert.ToInt32(markedRows[i]) == rowsPassedToReport) {
					allowedRow = true;
					break;
				}
			}
			if(allowedRow) {
				reportString += "<TR>";
				//in report, if there are more than 5 jumps, break the row
				if(maxJumps > 5) {
					//show 5 jumps in a row (every jump has 2 cols: TC + TF)
					int countCols = -3; //jumper, index , fall, count (from -3 to 0)
					int countRows = 0;
					for (int i=0; i < statValues.Length ; i++) 
					{
						//if a jump is shorter than the others, 
						//there's no need of filling rows with '-' in every cell
						if(countCols >= 1 && statValues[i] == "-") {
							break;
						}	
						
						//when countCols is 0, and countRows is 0 we should print the first 'Count'
						if(countCols == 0 && countRows == 0) {
							reportString += "<TD>1-5</TD>";
						}
						
						//change line
						if(countCols >= 10) {
							reportString += "</TR><TR><TD>&nbsp;</TD><TD>&nbsp;</TD><TD>&nbsp;</TD>";
							countRows ++;
							countCols = 0;
							reportString += "<TD>" + (countRows*5 + 1) + "-" + 
								(countRows*5 +5) + "</TD>";
						}
						reportString += "<TD>" + statValues[i] + "</TD>";
						countCols ++;
					}
				} else {
					for (int i=0; i < statValues.Length ; i++) {
						reportString += "<TD>" + statValues[i] + "</TD>";
					}
				}
				reportString += "</TR>\n";
			}
			rowsPassedToReport ++;
		} else {
			//iter = store.AppendValues (statValues); 
			iter = new TreeIter();
			
			//iter = store.Append (iter);	//doesn't work
			//store.Append (out iter);	//add new row and make iter point to it
			iter = store.AppendNode ();
		
			//addAllNoneIfNeeded(statValues.Length);
		
			TreePath myPath = store.GetPath(iter); 
			
			if(statValues[0] != Catalog.GetString("AVG") && statValues[0] != Catalog.GetString("SD")) {
				store.SetValue(iter, 0, true);	//first col is true if it's not AVG or SD
				markedRows.Add(myPath.ToString());
				//LogB.Information("FROM PRINTDATA (EVOLUTION) Added to markedRows row:{0}", myPath.ToString());
			}
			
			for(int i=0; i < statValues.Length; i++) {
				store.SetValue(iter, i+1, statValues[i]);
			}
		}
	}

	
	
	public override void PrepareData() 
	{
		string sessionString = obtainSessionSqlString(sessions, "jumpRj");
		//only simplesession
		bool multisession = false;

		//we send maxJumps for make all the results of same length (fill it with '-'s)
		//
		// cannot be avg in this statistic
		
		string operation = ""; //no need of "MAX", there's an order by (index) desc
		//and cleanDontWanted will do his work
		processDataSimpleSession ( cleanDontWanted (
					SqliteStat.RjEvolution(sessionString, multisession, 
						operation, jumpType, showSex, maxJumps), 
					statsJumpsType, limit),
				false, dataColumns); //don't print AVG and SD at end of row (has no sense)
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
		} 
		/* this option is not possible in this statistic
		 * 
		else if(statsJumpsType == 3) { //avg of each jumper
			selectedValuesString = avgValuesString; 
		}  
		*/

		string [] strFull = sessions[0].ToString().Split(new char[] {':'});
		string mySessionString =  Catalog.GetString (" session ") + 
			strFull[0] + "(" + strFull[2] + ")";
		
		string bestResalted = "";
		if(numContinuous != -1) {
			bestResalted = string.Format(
					Catalog.GetPluralString(
						" (best jump marked using [tf/tc *100])",
						" (best {0} consecutive jumps marked using [tf/tc *100])",
						numContinuous),
					numContinuous);
		}

		return string.Format(Catalog.GetString("{0} in Rj Evolution applied to {1} on {2}{3}"), selectedValuesString, jumpType, mySessionString, bestResalted) +
			". " + Catalog.GetString("Index is [(tfavg-tcavg)/tcavg *100]");
	}
}


