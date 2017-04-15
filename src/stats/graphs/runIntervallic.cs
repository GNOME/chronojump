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
using System.Drawing;
using System.Drawing.Imaging;
using Mono.Unix;


public class GraphRunIntervallic : StatRunIntervallic
{
	protected string operation;


	//numContinuous passed only for writing correct Enunciate	
	public GraphRunIntervallic  (StatTypeStruct myStatTypeStruct, int numContinuous)
	{
		isRunIntervalEvolution = true;

		completeConstruction (myStatTypeStruct, treeview);
		this.numContinuous = numContinuous;
		
		string sessionString = obtainSessionSqlString(sessions, "runInterval");
		//we need to know the reactive with more runs for prepare columns
		//later this value can be changed in stats/main/plotgraphgraphseries because 
		//there is possible to check the checked stats rows
		maxRuns = SqliteStat.ObtainMaxNumberOfRuns(sessionString);
		
		this.dataColumns = maxRuns + 1;	//for simplesession (speedavg, speed*runs, )
		
		//in X axe, we print the number of runs, not the person names
		//this should be equal to the number of runs
		//xAxisNames = new ArrayList(0);
		for(int i=0; i<maxRuns ; i++) {
			//xAxisNames.Add((i+1).ToString());
			CurrentGraphData.XAxisNames.Add((i+1).ToString());
		}
		
		if (statsJumpsType == 2) {
			this.operation = "MAX";
		} else {
			this.operation = "AVG";
		}

		CurrentGraphData.WindowTitle = Catalog.GetString("Chronojump graph");
		//title is shown on the graph except it's a report, then title will be on the html
		if(myStatTypeStruct.ToReport) {
			CurrentGraphData.GraphTitle = "";
		} else {
			//CurrentGraphData.GraphTitle = this.ToString();
			CurrentGraphData.GraphTitle = "";
		}
		
		
		CurrentGraphData.LabelLeft = translateYesNo("Speed") + "(m/s)";
		CurrentGraphData.LabelRight = "";
		CurrentGraphData.LabelBottom = translateYesNo("Races");
	}

	protected override void printData (string [] statValues) 
	{
		/*
		 * if one day i found the way of plotting different (single plots) i will use it for the numContinuous
		 */
		/*
		if(numContinuous != -1) {
			int bestPos = findBestContinuous(statValues, numContinuous);
			if(bestPos != -1) {
				statValues = markBestContinuous(statValues, numContinuous, bestPos);
			}
		}
		*/
		
		GraphSerie serieSpeed = new GraphSerie();
		serieSpeed.IsLeftAxis = true;

		int i = 0;
		foreach (string myValue in statValues) 
		{
			if(i==0) 
				serieSpeed.Title = myValue;
			else if(i >= 2)
				serieSpeed.SerieData.Add(myValue);
			//i==1 is the avg speed, is not used

			i++;
		}

		//add created series to GraphSeries ArrayList
		GraphSeries.Add(serieSpeed);
	}
}
