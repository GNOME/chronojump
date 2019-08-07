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


public class GraphGlobal : StatGlobal
{
	//for simplesession
	GraphSerie serieIndex;
	GraphSerie serieTv;

	public GraphGlobal (StatTypeStruct myStatTypeStruct, int personID, string personName) 
	{
		completeConstruction (myStatTypeStruct, treeview);
		
		this.dataColumns = 1; //for Simplesession
		this.personID = personID;
		this.personName = personName;
		//this.heightPreferred = heightPreferred;
		
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
		
		
		if(sessions.Count == 1) {
			//four series, the four columns
			serieIndex = new GraphSerie();
			serieTv = new GraphSerie();
	
			serieIndex.Title = translateYesNo("Value");
			if(heightPreferred) {
				serieTv.Title = translateYesNo("Height");
			} else {
				serieTv.Title = translateYesNo("TF");
			}
			
			serieIndex.IsLeftAxis = false;
			serieTv.IsLeftAxis = true;

			if(heightPreferred) {
				CurrentGraphData.LabelLeft = translateYesNo("Height") + "(cm)";
			} else {
				CurrentGraphData.LabelLeft = translateYesNo("TF") + "(s)";
			}
			CurrentGraphData.LabelRight = translateYesNo("Index") + "(%)";
		} else {
			for(int i=0; i < sessions.Count ; i++) {
				string [] stringFullResults = sessions[i].ToString().Split(new char[] {':'});
				CurrentGraphData.XAxisNames.Add(stringFullResults[1].ToString());
			}
			if(heightPreferred) {
				CurrentGraphData.LabelLeft = translateYesNo("Height") + "(cm)";
			} else {
				CurrentGraphData.LabelLeft = translateYesNo("TF") + "(s)";
			}
			//CurrentGraphData.LabelRight = "";
			CurrentGraphData.LabelRight = translateYesNo("Index") + "(%)";
		}
	}

	protected override void printData (string [] statValues) 
	{
		if(sessions.Count == 1) {
			int i = 0;
			//we need to save this transposed
			string myValueBefore = "";
			foreach (string myValue in statValues) 
			{
				if(i == 0) {
					
					//don't plot AVG and SD rows
					if( myValue == Catalog.GetString("AVG") || myValue == Catalog.GetString("SD") ) {
						return;
					}

					//global is nicer and cleaner  without the AVG and SD
					if(GraphSeries.Count == 0) {
						GraphSeries.Add(serieTv);
						GraphSeries.Add(serieIndex);
					}
					
					/*
					//don't plot AVG and SD rows
					if( myValue == Catalog.GetString("AVG") || myValue == Catalog.GetString("SD") ) {
						//good moment for adding created series to GraphSeries ArrayList
						//check don't do it two times
						if(GraphSeries.Count == 0) {
							GraphSeries.Add(serieIndex);
							GraphSeries.Add(serieTv);
						}
						return;
					}
					*/

					if(myValue.StartsWith("IndexQ")) {
						CurrentGraphData.XAxisNames.Add(myValue.Replace("IndexQ", "IndexQ *10"));
					} else if(myValue == "FV") {
						CurrentGraphData.XAxisNames.Add(myValue.Replace("FV", "FV *10"));
					} else {
						CurrentGraphData.XAxisNames.Add(myValue);
					}

					//record the statistic (stripping of sex)
					string [] stringFullResults = myValue.Split(new char[] {'.'});
					myValueBefore = stringFullResults[0];
				} else { 
					if(myValueBefore.StartsWith("DjIndex") ||  
							myValueBefore.StartsWith("RjIndex") || myValueBefore.StartsWith(Constants.RJPotencyBoscoNameStr()) ||
							myValueBefore == "IE" || myValueBefore == Constants.ArmsUseIndexName) {
						serieIndex.SerieData.Add(myValue);
						//serieTv.SerieData.Add("-");
					} else if(myValueBefore.StartsWith("IndexQ") || myValueBefore == "FV") {
						serieIndex.SerieData.Add( (
									Convert.ToDouble(myValue) *10).ToString() );
						//serieTv.SerieData.Add("-");
					} else {
						serieTv.SerieData.Add(myValue);
						//serieIndex.SerieData.Add("-");
					}
				}
				i++;
			}
		} else {
			GraphSerie mySerie = new GraphSerie();

			int i=0;
			string myValueBefore = "";
			foreach (string myValue in statValues) {
				if( myValue == Catalog.GetString("AVG") || myValue == Catalog.GetString("SD") ) {
					return;
				}
				if(i == 0) {
					//strip of sex
					string [] stringFullResults = myValue.Split(new char[] {'.'});
					string valueNoSex = stringFullResults[0];

					if(valueNoSex.StartsWith("DjIndex") || valueNoSex.StartsWith("IndexQ") || 
							valueNoSex.StartsWith("RjIndex") || valueNoSex.StartsWith(Constants.RJPotencyBoscoNameStr()) ||
							valueNoSex == "IE" || valueNoSex == Constants.ArmsUseIndexName || valueNoSex == "FV" ) {
						mySerie.IsLeftAxis = false;
					} else {
						mySerie.IsLeftAxis = true;
					}
					myValueBefore = valueNoSex; //for changing later indexQ for indexQ*10
					mySerie.Title = myValue;
				} else {
					if(myValueBefore.StartsWith("IndexQ")) {
						if(myValue == "-") {
							mySerie.SerieData.Add(myValue);
						} else {
							mySerie.SerieData.Add( (
										Convert.ToDouble(myValue) *10).ToString() );
						}
						mySerie.Title = myValueBefore.Replace("IndexQ", "IndexQ *10");
					} else if(myValueBefore == "FV") {
						if(myValue == "-") {
							mySerie.SerieData.Add(myValue);
						} else {
							mySerie.SerieData.Add( (
										Convert.ToDouble(myValue) *10).ToString() );
						}
						mySerie.Title = myValueBefore.Replace("FV", "FV *10");
					} else {
						mySerie.SerieData.Add(myValue);
					}
				}
				i++;
			}
			GraphSeries.Add(mySerie);
		}
	}
}
