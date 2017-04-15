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


public class GraphJumpSimpleSubtraction : StatJumpSimpleSubtraction
{
	protected string operation;

	//for simplesession
	GraphSerie serieResultPercent;
	//GraphSerie serieResult; //don't plot the result, plot the resultPercent
	GraphSerie serieJump1;
	GraphSerie serieJump2;

	public GraphJumpSimpleSubtraction  (StatTypeStruct myStatTypeStruct)
	{
		completeConstruction (myStatTypeStruct, treeview);
		
		this.dataColumns = 4; //for Simplesession (resultPercent, result, test1, test2)
		
		string [] applyTos = myStatTypeStruct.StatisticApplyTo.Split(new char[] {','});
		test1 = applyTos[0];
		test2 = applyTos[1];
		
		columnsString[0] = translateYesNo("Jumper");
		columnsString[1] = translateYesNo("ResultPercent");
		columnsString[2] = test1;
		columnsString[3] = test2;
		
		if (statsJumpsType == 2) {
			this.operation = "MAX";
		} else {
			this.operation = "AVG";
		}

		CurrentGraphData.WindowTitle = Catalog.GetString("Chronojump graph");
		//title is shown on the graph except it's a report, then title will be on the html
		if(myStatTypeStruct.ToReport) 
			CurrentGraphData.GraphTitle = "";
		else 
			CurrentGraphData.GraphTitle = string.Format(Catalog.GetString("Subtraction between {0} {1} and {0} {2}"), operation, test1, test2);
		
		
		if(sessions.Count == 1) {
			//four series, the four columns
			serieResultPercent = new GraphSerie();
			serieJump1 = new GraphSerie();
			serieJump2 = new GraphSerie();
				
			serieResultPercent.Title = translateYesNo("Result") + " %";
			serieJump1.Title = test1;
			serieJump2.Title = test2;
		
			serieResultPercent.IsLeftAxis = true;
			serieJump1.IsLeftAxis = false;
			serieJump2.IsLeftAxis = false;

			CurrentGraphData.LabelLeft = translateYesNo("Result") + " %"; 
			CurrentGraphData.LabelRight = 
				test1 + " " + translateYesNo("TF") + "(s), " + 
				test2 + " " + translateYesNo("TF") + "(s)";
		} else {
			for(int i=0; i < sessions.Count ; i++) {
				string [] stringFullResults = sessions[i].ToString().Split(new char[] {':'});
				CurrentGraphData.XAxisNames.Add(stringFullResults[1].ToString());
			}
			CurrentGraphData.LabelLeft = translateYesNo("Result") + " %";
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
						serieResultPercent.Avg = Convert.ToDouble(myValue);
					else
						serieResultPercent.SerieData.Add(myValue);
				//2 is result and is not plotted
				} else if(i == 3) {
					if(foundAVG)
						serieJump1.Avg = Convert.ToDouble(myValue);
					else
						serieJump1.SerieData.Add(myValue);
				} else if(i == 4) {
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
							GraphSeries.Add(serieResultPercent);
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
