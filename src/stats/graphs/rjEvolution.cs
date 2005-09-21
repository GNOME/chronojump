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

using NPlot.Gtk;
using NPlot;
using System.Drawing;
using System.Drawing.Imaging;


public class GraphRjEvolution : StatRjEvolution
{
	protected string operation;
	private Random myRand = new Random();

	public GraphRjEvolution  (StatTypeStruct myStatTypeStruct)
	{
		completeConstruction (myStatTypeStruct, treeview);
		
		string sessionString = obtainSessionSqlString(sessions);
		//we need to know the reactive with more jumps for prepare columns
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

		CurrentGraphData.WindowTitle = Catalog.GetString("ChronoJump graph");
		//title is shown on the graph except it's a report, then title will be on the html
		if(myStatTypeStruct.ToReport) {
			CurrentGraphData.GraphTitle = "";
		} else {
			CurrentGraphData.GraphTitle = this.ToString();
		}
		
		
		CurrentGraphData.LabelLeft = Catalog.GetString("seconds");
		CurrentGraphData.LabelRight = "";
	}

	protected override void printData (string [] statValues) 
	{
		GraphSerie serieTc = new GraphSerie();
		GraphSerie serieTv = new GraphSerie();

		serieTc.IsLeftAxis = true;
		serieTv.IsLeftAxis = true;

		int myR = myRand.Next(255);
		int myG = myRand.Next(255);
		int myB = myRand.Next(255);
		
		//serieTc.SerieMarker = new Marker (Marker.MarkerType.TriangleDown, 
		serieTc.SerieMarker = new Marker (Marker.MarkerType.Cross1, 
				6, new Pen (Color.FromArgb(myR, myG, myB), 2.0F));
		//serieTv.SerieMarker = new Marker (Marker.MarkerType.TriangleUp, 
		serieTv.SerieMarker = new Marker (Marker.MarkerType.Cross1, 
				6, new Pen (Color.FromArgb(myR, myG, myB), 2.0F));
		
		//for the line between markers
		serieTc.SerieColor = Color.FromArgb(myR, myG, myB);
		serieTv.SerieColor = Color.FromArgb(myR, myG, myB);
		
		int i = 0;
		foreach (string myValue in statValues) 
		{
			if(i==0) {
				serieTc.Title = myValue + " TC";
				serieTv.Title = myValue + " TV";

			} else if(isTC(i)) {
				serieTc.SerieData.Add(myValue);
			} else if(isTV(i)) {
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
			if (i*2 +2 == col) { //TC cols: 2, 4, 6, 8, ...
				return true;
			}
		}
		return false;
	}
	
	private bool isTV(int col) {
		for (int i=0; i < maxJumps ; i++) {
			if (i*2 +3 == col) { //TV cols: 3, 5, 6, 9, ...
				return true;
			}
		}
		return false;
	}
}
