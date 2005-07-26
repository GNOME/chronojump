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


public class GraphGlobal : StatGlobal
{
	private Random myRand = new Random();

	//for simplesession
	GraphSerie serieIndex;
	GraphSerie serieTv;

	
	public GraphGlobal (ArrayList sessions, int personID, string personName, int newPrefsDigitsNumber, bool showSex, int statsJumpsType, bool heightPreferred) 
	{
		this.dataColumns = 1; //for Simplesession
		this.personID = personID;
		this.personName = personName;
		this.heightPreferred = heightPreferred;
		
		completeConstruction (treeview, sessions, newPrefsDigitsNumber, showSex, statsJumpsType);
	
		if (statsJumpsType == 2) {
			this.operation = "MAX";
		} else {
			this.operation = "AVG";
		}

		CurrentGraphData.WindowTitle = Catalog.GetString("ChronoJump graph");
		string mySessions = "single session";
		if(sessions.Count > 1) {
			mySessions = "multiple sessions";
		}
		CurrentGraphData.GraphTitle = "Global " + operation + 
			Catalog.GetString(" values chart of ") + mySessions;
		
		
		if(sessions.Count == 1) {
			//four series, the four columns
			serieIndex = new GraphSerie();
			serieTv = new GraphSerie();
				
			serieIndex.Title = Catalog.GetString("Index");
			if(heightPreferred) {
				serieTv.Title = Catalog.GetString("Height");
			} else {
				serieTv.Title = "TV";
			}
			
			serieIndex.IsLeftAxis = false;
			serieTv.IsLeftAxis = true;

			serieIndex.SerieMarker = new Marker (Marker.MarkerType.FilledCircle, 
					6, new Pen (Color.FromName("Red"), 2.0F));
			//serieTv.SerieMarker = new Marker (Marker.MarkerType.TriangleUp, 
			serieTv.SerieMarker = new Marker (Marker.MarkerType.Cross1, 
					6, new Pen (Color.FromName("LightBlue"), 2.0F));
		
			//for the line between markers
			serieIndex.SerieColor = Color.FromName("Red");
			serieTv.SerieColor = Color.FromName("LightBlue");
		
			if(heightPreferred) {
				CurrentGraphData.LabelLeft = Catalog.GetString("centimeters");
			} else {
				CurrentGraphData.LabelLeft = Catalog.GetString("seconds");
			}
			CurrentGraphData.LabelRight = Catalog.GetString("%");
		} else {
			for(int i=0; i < sessions.Count ; i++) {
				string [] stringFullResults = sessions[i].ToString().Split(new char[] {':'});
				CurrentGraphData.XAxisNames.Add(stringFullResults[1].ToString());
			}
			if(heightPreferred) {
				CurrentGraphData.LabelLeft = Catalog.GetString("centimeters");
			} else {
				CurrentGraphData.LabelLeft = Catalog.GetString("seconds");
			}
			CurrentGraphData.LabelRight = Catalog.GetString("%");
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
						//good moment for adding created series to GraphSeries ArrayList
						//check don't do it two times
						if(GraphSeries.Count == 0) {
							GraphSeries.Add(serieIndex);
							GraphSeries.Add(serieTv);
						}
						return;
					}
					CurrentGraphData.XAxisNames.Add(myValue);

					//record the statistic (stripping of sex)
					string [] stringFullResults = myValue.Split(new char[] {'.'});
					myValueBefore = stringFullResults[0];
				} else { 
					if(myValueBefore == "DjIndex" || myValueBefore == "RjIndex" || 
							myValueBefore == "RjPotency" || myValueBefore == "IE" || 
							myValueBefore == "IUB") {
						serieIndex.SerieData.Add(myValue);
						serieTv.SerieData.Add("-");
					} else {
						serieTv.SerieData.Add(myValue);
						serieIndex.SerieData.Add("-");
					}
				}
				i++;
			}
		} else {
			GraphSerie mySerie = new GraphSerie();
		
			int myR = myRand.Next(255);
			int myG = myRand.Next(255);
			int myB = myRand.Next(255);
			
			
			mySerie.SerieColor = Color.FromArgb(myR, myG, myB);
			
			int i=0;
			foreach (string myValue in statValues) {
				if( myValue == Catalog.GetString("AVG") || myValue == Catalog.GetString("SD") ) {
					return;
				}
				if(i == 0) {
					mySerie.Title = myValue;
					//strip of sex
					string [] stringFullResults = myValue.Split(new char[] {'.'});
					string valueNoSex = stringFullResults[0];
					if(valueNoSex == "DjIndex" || valueNoSex == "RjIndex" || 
							valueNoSex == "RjPotency" || valueNoSex == "IE" || 
							valueNoSex == "IUB") {
						mySerie.IsLeftAxis = false;
						mySerie.SerieMarker = new Marker (Marker.MarkerType.FilledCircle, 
								6, new Pen (Color.FromArgb(myR, myG, myB), 2.0F));
					} else {
						mySerie.IsLeftAxis = true;
						//mySerie.SerieMarker = new Marker (Marker.MarkerType.TriangleUp, 
						mySerie.SerieMarker = new Marker (Marker.MarkerType.Cross1, 
								6, new Pen (Color.FromArgb(myR, myG, myB), 2.0F));
					}
				} else {
					mySerie.SerieData.Add(myValue);
				}
				i++;
			}
			GraphSeries.Add(mySerie);
		}
	}
}
