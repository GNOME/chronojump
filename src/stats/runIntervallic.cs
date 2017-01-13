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


public class StatRunIntervallic : Stat
{
	protected int maxRuns;

	//if this is not present i have problems like (No overload for method `xxx' takes `0' arguments) with some inherited classes
	public StatRunIntervallic () 
	{
		this.showSex = false;
		this.statsJumpsType = 0;
		this.limit = 0;
	}

	public StatRunIntervallic (StatTypeStruct myStatTypeStruct, int numContinuous, Gtk.TreeView treeview) 
	{
		completeConstruction (myStatTypeStruct, treeview);

		this.numContinuous = numContinuous;
		
		string sessionString = obtainSessionSqlString(sessions, "runInterval");

		//we need to know the run with more tracks for prepare columns
		maxRuns = SqliteStat.ObtainMaxNumberOfRuns(sessionString);
		
		this.dataColumns = maxRuns +1;	//for simplesession (avg speed, speed of each track)

		//only simplesession
		store = getStore(dataColumns +1); //jumper, datacolumns 
		string [] columnsString;
	      
	       //in report, show only 10 tracks every row, 
	       //if there are more tracks to show, let's cut them
		if(toReport && maxRuns > 10 ) {
			columnsString = new String[13]; //person, index, count, 10 tracks
		} else {
			columnsString =	new String[dataColumns +1];
		}
		columnsString[0] = Catalog.GetString("Person");
		columnsString[1] = Catalog.GetString("Speed");

		if(toReport && maxRuns > 10) {
			columnsString[2] = Catalog.GetString("Count");
			for(int i=0; i < maxRuns && i < 10; i++) {
				columnsString[i+3] = Catalog.GetString("Speed") + (i+1).ToString(); //cols: 3, 4, 5, ...
			}
		} else {
			for(int i=0; i < maxRuns; i++) {
				columnsString[i+2] = Catalog.GetString("Speed") + (i+1).ToString(); //cols: 2, 3, 4, ...
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
		//if numContinuous is 3, we check the best consecutive three time values
		int bestPos=-1;	//position where the three best group values start
				//will return -1 if less than three runs
		double bestCount=-10000;	//best value found of 3 pairs

		//start in pos 2 because first is name, second is speed
		for ( int i=2; i <= statValues.Length -numContinuous ; i=i+1 ) 
		{
			double myCount = 0;
			bool runFinished = false;
			//read the n consecutive values 
			for (int j=i; j < i + numContinuous ; j=j+1 )
			{
				if( statValues[j] == "-") {
					runFinished = true;
					break;
				}
				double speed = Convert.ToDouble(statValues[i]);
				myCount += speed;
			}
			
			//if found a max, record it
			if(myCount > bestCount && !runFinished) {
				bestCount = myCount;
				bestPos = i;
			}
		}
		return bestPos;
	}

	protected string [] markBestContinuous(string [] statValues, int numContinuous, int bestPos) {
		if(toReport) {
			for ( int i=0; i < statValues.Length ; i=i+1 ) {

				if(i >= bestPos && i < bestPos+numContinuous) {
					statValues[i] = "<font color=\"red\">" + statValues[i] + "</font>";
				}
			}
		} else {
			// this marks the first and the last with '[' and ']'
			statValues[bestPos] = "[" + statValues[bestPos];
			statValues[bestPos + numContinuous -1] = statValues[bestPos + numContinuous -1] + "]";
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
				//in report, if there are more than 10 runs, break the row
				if(maxRuns > 10) {
					//show 10 runs in a row (every run has 1 col: speed)
					int countCols = -2; //person, speed, count (from -2 to 0)
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
							reportString += "<TD>1-10</TD>";
						}
						
						//change line
						if(countCols >= 10) {
							reportString += "</TR><TR><TD>&nbsp;</TD><TD>&nbsp;</TD>";
							countRows ++;
							countCols = 0;
							reportString += "<TD>" + (countRows*10 + 1) + "-" + 
								(countRows*10 +10) + "</TD>";
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
			}
			
			for(int i=0; i < statValues.Length; i++) {
				store.SetValue(iter, i+1, statValues[i]);
			}
		}
	}

	
	
	public override void PrepareData() 
	{
		string sessionString = obtainSessionSqlString(sessions, "runInterval");
		//only simplesession
		bool multisession = false;

		//we send maxRuns for make all the results of same length (fill it with '-'s)
		//
		// cannot be avg in this statistic
		
		string operation = ""; //no need of "MAX", there's an order by (index) desc
		//and cleanDontWanted will do his work
		processDataSimpleSession ( cleanDontWanted (
					SqliteStat.RunInterval(sessionString, multisession, 
						operation, jumpType, showSex, maxRuns), 
					statsJumpsType, limit),
				false, dataColumns); //TODO: maybe in future do avg and sd of speeds 
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
						" (best race marked)",
						" (best {0} consecutive laps marked)",
						numContinuous),
					numContinuous);
		}

		return string.Format(Catalog.GetString("{0} in Intervallic races applied to {1} on {2}{3}"), selectedValuesString, jumpType, mySessionString, bestResalted);
	}
}


