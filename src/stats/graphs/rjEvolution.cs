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


public class GraphRjEvolution : StatRjEvolution
{
	protected string operation;


	//numContinuous passed only for writing correct Enunciate	
	public GraphRjEvolution  (StatTypeStruct myStatTypeStruct, int numContinuous)
	{
		isRjEvolution = true;

		completeConstruction (myStatTypeStruct, treeview);
		this.numContinuous = numContinuous;
		
		string sessionString = obtainSessionSqlString(sessions, "jumpRj");
		//we need to know the reactive with more jumps for prepare columns
		//later this value can be changed in stats/main/plotgraphgraphseries because 
		//there is possible to check the checked stats rows
		maxJumps = SqliteStat.ObtainMaxNumberOfJumps(sessionString);
		
		this.dataColumns = maxJumps*2 + 2;	//for simplesession (index, (tv , tc)*jumps, fall)
		
		//in X axe, we print the number of jumps, not the jumper names
		//this should be equal to the number of jumps
		//xAxisNames = new ArrayList(0);
		for(int i=0; i<maxJumps ; i++) {
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
		
		
		CurrentGraphData.LabelLeft = translateYesNo("Time") + "(s)";
		CurrentGraphData.LabelRight = "";
		CurrentGraphData.LabelBottom = translateYesNo("Jumps");
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
		
		GraphSerie serieTc = new GraphSerie();
		GraphSerie serieTv = new GraphSerie();

		serieTc.IsLeftAxis = true;
		serieTv.IsLeftAxis = true;

		int i = 0;
		foreach (string myValue in statValues) 
		{
			if(i==0) {
				serieTc.Title = myValue + " " + Catalog.GetString("TC");
				serieTv.Title = myValue + " " + Catalog.GetString("TF");

			} else if(isTC(i)) {
				serieTc.SerieData.Add(myValue);
			} else if(isTF(i)) {
				serieTv.SerieData.Add(myValue);
			}
			i++;
		}

		//add created series to GraphSeries ArrayList
		GraphSeries.Add(serieTc);
		GraphSeries.Add(serieTv);
	}

	private bool isTC(int col) {
		for (int i=0; i < maxJumps ; i++) {
			if (i*2 +3 == col) { //TC cols: 3, 5, 7, 9, ...
				return true;
			}
		}
		return false;
	}
	
	private bool isTF(int col) {
		for (int i=0; i < maxJumps ; i++) {
			if (i*2 +4 == col) { //TF cols: 4, 6, 8, 10, ...
				return true;
			}
		}
		return false;
	}
}
