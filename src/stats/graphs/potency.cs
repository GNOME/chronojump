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
using Mono.Unix;

public class GraphPotency : StatPotency
{
	protected string operation;
	private Random myRand = new Random();
	private int countSeriesGraphColors = 0;

	//for simplesession
	GraphSerie serieIndex;
	GraphSerie seriePersonWeight;
	GraphSerie serieExtraWeight;
	GraphSerie serieHeight;

	public GraphPotency (StatTypeStruct myStatTypeStruct, string indexType)
	{
		this.indexType = indexType;
		completeConstruction (myStatTypeStruct, treeview);
		
		this.dataColumns = 4; //for Simplesession (index, personWeight, extraWeight, height)
		
		/*
		if (statsJumpsType == 2) {
			this.operation = "MAX";
		} else {
			this.operation = "AVG";
		}
		*/
		this.operation = "";

		CurrentGraphData.WindowTitle = Catalog.GetString("ChronoJump graph");
		//title is shown on the graph except it's a report, then title will be on the html
		if(myStatTypeStruct.ToReport) {
			CurrentGraphData.GraphTitle = "";
		} else {
			//CurrentGraphData.GraphTitle = this.ToString();
			CurrentGraphData.GraphTitle = "";
		}
		
		if(sessions.Count == 1) {
			//four series, the four columns
			serieIndex = new GraphSerie();
			seriePersonWeight = new GraphSerie();
			serieExtraWeight = new GraphSerie();
			serieHeight = new GraphSerie();
				
			serieIndex.Title = Catalog.GetString("Peak Power");
			seriePersonWeight.Title = Catalog.GetString("Person's Weight");
			serieExtraWeight.Title = Catalog.GetString("Extra Weight") + " (Kg)";
			serieHeight.Title = Catalog.GetString("Height");
			
			serieIndex.IsLeftAxis = true;
			seriePersonWeight.IsLeftAxis = false;
			serieExtraWeight.IsLeftAxis = false;
			serieHeight.IsLeftAxis = false;

			serieIndex.SerieMarker = new Marker (Marker.MarkerType.FilledCircle, 
					6, new Pen (Color.FromName("Red"), 2.0F));
			seriePersonWeight.SerieMarker = new Marker (Marker.MarkerType.Cross1, 
					6, new Pen (Color.FromName("Green"), 2.0F));
			serieExtraWeight.SerieMarker = new Marker (Marker.MarkerType.Cross1, 
					6, new Pen (Color.FromName("Blue"), 2.0F));
			serieHeight.SerieMarker = new Marker (Marker.MarkerType.Cross1, 
					6, new Pen (Color.FromName("Black"), 2.0F));
		
			//for the line between markers
			serieIndex.SerieColor = Color.FromName("Red");
			seriePersonWeight.SerieColor = Color.FromName("Green");
			serieExtraWeight.SerieColor = Color.FromName("Blue");
			serieHeight.SerieColor = Color.FromName("Black");
		
			CurrentGraphData.LabelLeft = Catalog.GetString("watts");
			CurrentGraphData.LabelRight = "kg, cm";
		} else {
			for(int i=0; i < sessions.Count ; i++) {
				string [] stringFullResults = sessions[i].ToString().Split(new char[] {':'});
				CurrentGraphData.XAxisNames.Add(stringFullResults[1].ToString());
			}
			CurrentGraphData.LabelLeft = Catalog.GetString("watts");
			CurrentGraphData.LabelRight = "";
		}
	}

	protected override void printData (string [] statValues) 
	{
		//values are recorded for calculating later AVG and SD
		recordStatValues(statValues);

		if(sessions.Count == 1) {
			int i = 0;
			bool foundAVG = false;
			//we need to save this transposed
			foreach (string myValue in statValues) 
			{
				if(i == 0) {
					//don't plot AVG and SD rows
					if( myValue == Catalog.GetString("AVG")) 
						foundAVG = true;
					else
						CurrentGraphData.XAxisNames.Add(myValue);
				} else if(i == 1) {
					if(foundAVG)
						serieIndex.Avg = Convert.ToDouble(myValue);
					else
						serieIndex.SerieData.Add(myValue);
				} else if(i == 2) {
					if(foundAVG)
						seriePersonWeight.Avg = Convert.ToDouble(myValue);
					else
						seriePersonWeight.SerieData.Add(myValue);
				} else if(i == 3) {
					if(foundAVG)
						serieExtraWeight.Avg = Convert.ToDouble(myValue);
					else
						serieExtraWeight.SerieData.Add(myValue);
				} else if(i == 4) {
					if(foundAVG)
						serieHeight.Avg = Convert.ToDouble(myValue);
					else
						serieHeight.SerieData.Add(myValue);
				}

				if(foundAVG && i == dataColumns) {
					//add created series to GraphSeries ArrayList
					//check don't do it two times
					if(GraphSeries.Count == 0) {
						GraphSeries.Add(serieIndex);
						GraphSeries.Add(seriePersonWeight);
						GraphSeries.Add(serieExtraWeight);
						GraphSeries.Add(serieHeight);
					}
					return;
				}

				i++;
			}
		} else {
			GraphSerie mySerie = new GraphSerie();
			mySerie.IsLeftAxis = true;
		
			//color code
			Color myColor = new Color();
			if(countSeriesGraphColors > Constants.Colors.Length) {
				int myR = myRand.Next(255 - 40); //not 255 for not being so light colors
				int myG = myRand.Next(255 - 40);
				int myB = myRand.Next(255 - 40);
				myColor = Color.FromArgb(myR, myG, myB);
			} else {
				myColor = Color.FromName(Constants.Colors[countSeriesGraphColors]);
				countSeriesGraphColors ++;
			}
			
			mySerie.SerieMarker = new Marker (Marker.MarkerType.Cross1, 
					6, new Pen (myColor, 2.0F));
			
			mySerie.SerieColor = myColor;
			
			
			int i=0;
			foreach (string myValue in statValues) {
				if( myValue == Catalog.GetString("SD") ) 
					return;
				
				if(i == 0) 
					mySerie.Title = myValue;
				else if( i == sessions.Count + 1 ) { //eg, for 2 sessions: [(0)person name, (1)sess1, (2)sess2, (3)AVG]
					if(myValue != "-")
						mySerie.Avg = Convert.ToDouble(myValue);
				} else 
					mySerie.SerieData.Add(myValue);
				
				i++;
			}
			GraphSeries.Add(mySerie);
		}
	}
}
