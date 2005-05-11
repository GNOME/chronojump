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


public class GraphSjCmjAbk : StatSjCmjAbk
{
	protected string operation;
	private Random myRand = new Random();

	//for simplesession
	GraphSerie serieTv;
	
	public GraphSjCmjAbk (ArrayList sessions, int newPrefsDigitsNumber, string jumpType, bool showSex, int statsJumpsType, int limit) 
	{
		this.dataColumns = 1; //for Simplesession
		this.jumpType = jumpType;
		this.limit = limit;
		
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
		CurrentGraphData.GraphTitle = jumpType + " " + operation + 
			Catalog.GetString(" values chart of ") + mySessions;
		
		
		if(sessions.Count == 1) {
			//four series, the four columns
			serieTv = new GraphSerie();
				
			serieTv.Title = "TV";
			
			serieTv.IsLeftAxis = true;

			//serieTv.SerieMarker = new Marker (Marker.MarkerType.TriangleUp, 
			serieTv.SerieMarker = new Marker (Marker.MarkerType.Cross1, 
					6, new Pen (Color.FromName("LightBlue"), 2.0F));
		
			//for the line between markers
			serieTv.SerieColor = Color.FromName("LightBlue");
		
			CurrentGraphData.LabelLeft = Catalog.GetString("seconds");
			CurrentGraphData.LabelRight = "";
		} else {
			for(int i=0; i < sessions.Count ; i++) {
				string [] stringFullResults = sessions[i].ToString().Split(new char[] {':'});
				CurrentGraphData.XAxisNames.Add(stringFullResults[1].ToString());
			}
			CurrentGraphData.LabelLeft = Catalog.GetString("seconds");
			CurrentGraphData.LabelRight = "";
		}
	}

	protected override void printData (string [] statValues) 
	{
		if(sessions.Count == 1) {
			int i=0;
			//we need to save this transposed
			foreach (string myValue in statValues) {
				if(i == 0) {
					//don't plot AVG and SD rows
					if( myValue == Catalog.GetString("AVG") || myValue == Catalog.GetString("SD") ) {
						//good moment for adding created series to GraphSeries ArrayList
						//check don't do it two times
						if(GraphSeries.Count == 0) {
							GraphSeries.Add(serieTv);
						}

						return;
					}
					CurrentGraphData.XAxisNames.Add(myValue);
				} else if(i == 1) {
					serieTv.SerieData.Add(myValue);
				} 
				i++;
			}
		} else {
			GraphSerie mySerie = new GraphSerie();
			mySerie.IsLeftAxis = true;

			int myR = myRand.Next(255);
			int myG = myRand.Next(255);
			int myB = myRand.Next(255);

			//mySerie.SerieMarker = new Marker (Marker.MarkerType.TriangleUp, 
			mySerie.SerieMarker = new Marker (Marker.MarkerType.Cross1, 
					6, new Pen (Color.FromArgb(myR, myG, myB), 2.0F));

			mySerie.SerieColor = Color.FromArgb(myR, myG, myB);

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
