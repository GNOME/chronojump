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
 */

using System;
using System.Data;
using Gtk;
using System.Collections; //ArrayList

//using NPlot.Gtk;
//using NPlot;
using System.Drawing;
using System.Drawing.Imaging;
using Mono.Unix;


public class GraphIeIub : StatIeIub
{
	protected string operation;
	private Random myRand = new Random();
	private int countSeriesGraphColors = 0;

	//for simplesession
	GraphSerie serieIndex;
	GraphSerie serieJump1;
	GraphSerie serieJump2;

	public GraphIeIub  (StatTypeStruct myStatTypeStruct, string indexType)
	{
		completeConstruction (myStatTypeStruct, treeview);
		
		this.dataColumns = 3; //for Simplesession (index, jump1, jump2)
		
		this.indexType = indexType; //"IE" or "IUB"
		if(indexType == "IE") {
			jump1="CMJ";
			jump2="SJ";
		} else { //IUB
			jump1="ABK";
			jump2="CMJ";
		}
		
		columnsString[0] = "Jumper";
		columnsString[1] = indexType;
		columnsString[2] = jump1;
		columnsString[3] = jump2;
		
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
			//CurrentGraphData.GraphTitle = this.ToString();
			CurrentGraphData.GraphTitle = "";
		}
		
		
		if(sessions.Count == 1) {
			//four series, the four columns
			serieIndex = new GraphSerie();
			serieJump1 = new GraphSerie();
			serieJump2 = new GraphSerie();
				
			serieIndex.Title = Catalog.GetString("Index");
			serieJump1.Title = jump1;
			serieJump2.Title = jump2;
			
			serieIndex.IsLeftAxis = false;
			serieJump1.IsLeftAxis = true;
			serieJump2.IsLeftAxis = true;

			/*
			serieIndex.SerieMarker = new Marker (Marker.MarkerType.Square, 
					6, new Pen (Color.FromName("Red"), 2.0F));
			//serieJump1.SerieMarker = new Marker (Marker.MarkerType.TriangleUp, 
			serieJump1.SerieMarker = new Marker (Marker.MarkerType.Cross1, 
					6, new Pen (Color.FromName("Green"), 2.0F));
			//serieJump2.SerieMarker = new Marker (Marker.MarkerType.TriangleUp, 
			serieJump2.SerieMarker = new Marker (Marker.MarkerType.Cross1, 
					6, new Pen (Color.FromName("Blue"), 2.0F));
					*/
		
			//for the line between markers
			serieIndex.SerieColor = Color.FromName("Red");
			serieJump1.SerieColor = Color.FromName("Green");
			serieJump2.SerieColor = Color.FromName("Blue");
		
			CurrentGraphData.LabelLeft = 
				jump1 + " " + Catalog.GetString("TF") + "(s), " + 
				jump2 + " " + Catalog.GetString("TF") + "(s)";
			CurrentGraphData.LabelRight = Catalog.GetString("Index") + "(%)";
		} else {
			for(int i=0; i < sessions.Count ; i++) {
				string [] stringFullResults = sessions[i].ToString().Split(new char[] {':'});
				CurrentGraphData.XAxisNames.Add(stringFullResults[1].ToString());
			}
			CurrentGraphData.LabelLeft = Catalog.GetString("Index") + "(%)";
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
						serieJump1.Avg = Convert.ToDouble(myValue);
					else
						serieJump1.SerieData.Add(myValue);
				} else if(i == 3) {
					if(foundAVG)
						serieJump2.Avg = Convert.ToDouble(myValue);
					else
						serieJump2.SerieData.Add(myValue);
				}

				if(foundAVG && i == dataColumns) {
					//add created series to GraphSeries ArrayList
					//check don't do it two times
					if(GraphSeries.Count == 0) {
							GraphSeries.Add(serieJump1);
							GraphSeries.Add(serieJump2);
							GraphSeries.Add(serieIndex);
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
		
//			mySerie.SerieMarker = new Marker (Marker.MarkerType.Cross1, 
//					6, new Pen (myColor, 2.0F));
			
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
