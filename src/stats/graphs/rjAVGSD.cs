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

public class GraphRjAVGSD : StatRjAVGSD
{
	protected string operation;
	private Random myRand = new Random();
	private int countSeriesGraphColors = 0;

	//for simplesession
	GraphSerie serieAVG;
	GraphSerie serieSD;

	public GraphRjAVGSD (StatTypeStruct myStatTypeStruct, string indexType) 
	{
		this.indexType = indexType;

		completeConstruction (myStatTypeStruct, treeview);

		this.dataColumns = 2; //for Simplesession (avg, sd)

		//no average for this stat
		//if (statsJumpsType == 2) {
		this.operation = "MAX";
		/*
		   } else {
		   this.operation = "AVG";
		   }
		   */

		CurrentGraphData.WindowTitle = Catalog.GetString("ChronoJump graph");
		//title is shown on the graph except it's a report, then title will be on the html
		if(myStatTypeStruct.ToReport) {
			CurrentGraphData.GraphTitle = "";
		} else {
			//CurrentGraphData.GraphTitle = this.ToString();
			CurrentGraphData.GraphTitle = "";
		}


		serieAVG = new GraphSerie();
		serieSD = new GraphSerie();

		serieAVG.Title = Catalog.GetString("Average");
		serieSD.Title = Catalog.GetString("SD");

		serieAVG.IsLeftAxis = true;
		serieSD.IsLeftAxis = true;

		serieAVG.SerieMarker = new Marker (Marker.MarkerType.FilledCircle, 
				6, new Pen (Color.FromName("Red"), 2.0F));
		serieSD.SerieMarker = new Marker (Marker.MarkerType.Cross1, 
				6, new Pen (Color.FromName("Black"), 2.0F));

		//for the line between markers
		serieAVG.SerieColor = Color.FromName("Red");
		serieSD.SerieColor = Color.FromName("Black");

		CurrentGraphData.LabelLeft = Catalog.GetString("seconds");
		CurrentGraphData.LabelRight = "";
	}

	protected override void printData (string [] statValues) 
	{
		//values are recorded for calculating later AVG and SD
		recordStatValues(statValues);

		//only simpleSession
			
		int i = 0;
		//we need to save this transposed
		foreach (string myValue in statValues) 
		{
			if(i == 0) 
				CurrentGraphData.XAxisNames.Add(myValue);
			else if(i == 1) 
				serieAVG.SerieData.Add(myValue);
			else if(i == 2)
				serieSD.SerieData.Add(myValue);

			if (i == 2)
				i = 0;
			else
				i++;
		}
				
		//add created series to GraphSeries ArrayList
		//check don't do it two times
		if(GraphSeries.Count == 0) {
			GraphSeries.Add(serieAVG);
			GraphSeries.Add(serieSD);
		}
	}
	
	protected override int plotGraphGraphSeries (IPlotSurface2D plot, int xtics, ArrayList allSeries)
	{
		double[] lineData = new double[ xtics ];

		Marker m = serieAVG.SerieMarker;
		Marker mSDUp = new Marker (Marker.MarkerType.TriangleUp, 
				6, new Pen (Color.FromName("Black"), 1.0F));
		Marker mSDDown = new Marker (Marker.MarkerType.TriangleDown, 
				6, new Pen (Color.FromName("Black"), 1.0F));

		PointPlot pp;
		LinePlot lp;
		PointPlot ppSDUp;
		PointPlot ppSDDown;

		pp = new PointPlot( m );
		pp.Label = serieAVG.Title; 
		lp = new LinePlot();
		lp.Label = serieAVG.Title; 
		lp.Color = serieAVG.SerieColor;
		ppSDUp = new PointPlot( mSDUp );
		ppSDDown = new PointPlot( mSDDown );
		ppSDUp.Label = serieSD.Title; 

		//left margin
		lineData[0] = double.NaN;

		int added=1;
		int counter=0;
		foreach (string myValue in serieAVG.SerieData) 
		{
			//in single session lineData should contain all rows from stats except unchecked
			if(sessions.Count == 1 && ! acceptCheckedData(counter) ) {
				counter ++;
				continue;
			}

			if(myValue == "-") 
				lineData[added++] = double.NaN;
			else 
				lineData[added++] = Convert.ToDouble(myValue);

			counter++;
		}

		//right margin
		lineData[added] = double.NaN;

		lp.DataSource = lineData;
		pp.OrdinateData = lineData;
		ppSDUp.OrdinateData = convertLineDataToSDData(lineData, serieSD.SerieData, true);
		ppSDDown.OrdinateData = convertLineDataToSDData(lineData, serieSD.SerieData, false);

		pp.AbscissaData = new StartStep( 0, 1 ); //ini 0, step 1 (ini 0 because in lineData we start with blank value)
		ppSDUp.AbscissaData = new StartStep( 0, 1 );
		ppSDDown.AbscissaData = new StartStep( 0, 1 );
		pp.ShowInLegend = false;
		ppSDUp.ShowInLegend = true; //only need to show it one time
		ppSDDown.ShowInLegend = false;

		plot.Add( lp );
		plot.Add( pp );
		plot.Add( ppSDUp );
		plot.Add( ppSDDown );

		int acceptedSerie = 1;

		return acceptedSerie; //for knowing if a serie was accepted, and then createAxisGraphSeries
	}

	private double[] convertLineDataToSDData(double[] lineData, ArrayList serieSDData, bool up) {
		double[] lineDataReturn = new double[ lineData.Length ];
		int i = 1;
		for(i=1; i < lineData.Length -1; i ++) {
			if(lineData[i] != double.NaN) {
				int j = 0;
				foreach(string mySD in serieSDData) {
					if(j+1 == i) {
						if(up)
							lineDataReturn[i] = lineData[i] + Convert.ToDouble(mySD);
						else
							lineDataReturn[i] = lineData[i] - Convert.ToDouble(mySD);
						break;
					}
					j++;
				}
			} else
				lineDataReturn[i] = double.NaN;
		}
		lineDataReturn[i] = double.NaN;

		return lineDataReturn;
	}
}
