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


public class GraphFv : StatFv
{
	protected string operation;

	//for simplesession
	GraphSerie serieIndex;
	GraphSerie serieJump1;
	GraphSerie serieJump2;

	public GraphFv  (StatTypeStruct myStatTypeStruct, string indexType)
	{
		completeConstruction (myStatTypeStruct, treeview);
		
		this.dataColumns = 3; //for Simplesession (index, jump1, jump2)
		//this.jumpType = jumpType;
		//this.limit = limit;
		
		jump1="SJl";
		jump2="SJ";
	
		//completeConstruction (treeview, sessions, newPrefsDigitsNumber, showSex, statsJumpsType);
	
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
			CurrentGraphData.GraphTitle = Constants.FvIndexFormula;
		}
		
		if(sessions.Count == 1) {
			//four series, the four columns
			serieIndex = new GraphSerie();
			serieJump1 = new GraphSerie();
			serieJump2 = new GraphSerie();
				
			serieIndex.Title = indexType;
			serieJump1.Title = jump1 + " (" + translateYesNo("height") + ")";
			serieJump2.Title = jump2 + " (" + translateYesNo("height") + ")";
			
			serieIndex.IsLeftAxis = false;
			serieJump1.IsLeftAxis = true;
			serieJump2.IsLeftAxis = true;

			//this index is measured in height of CdG (not in tv)
			CurrentGraphData.LabelLeft = 
				jump1 + " " + translateYesNo("Height") + "(cm), " + 
				jump2 + " " + translateYesNo("Height") + "(cm)";
			CurrentGraphData.LabelRight = translateYesNo("Index") + "(%)";
		} else {
			for(int i=0; i < sessions.Count ; i++) {
				string [] stringFullResults = sessions[i].ToString().Split(new char[] {':'});
				CurrentGraphData.XAxisNames.Add(stringFullResults[1].ToString());
			}
			CurrentGraphData.LabelLeft = translateYesNo("Index") + "(%)";
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
						foundAVG =  true;
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
		
			int i=0;
			foreach (string myValue in statValues) {
				if( myValue == Catalog.GetString("AVG") || myValue == Catalog.GetString("SD") ) {
					return;
				}
				if(i == 0) {
					mySerie.Title = myValue;
				} else {
					mySerie.SerieData.Add(myValue);
				}
				i++;
			}
			GraphSeries.Add(mySerie);
		}
	}
}
