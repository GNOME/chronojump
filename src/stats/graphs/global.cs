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
	protected ArrayList by1Values;
	protected ArrayList by100Values;
	
	public GraphGlobal (ArrayList sessions, int personID, string personName, int newPrefsDigitsNumber, bool showSex, int statsJumpsType) 
	{
		this.sessionName = "";

		by1Values = new ArrayList(2); 
		by100Values = new ArrayList(2);
		this.dataColumns = 1; //for Simplesession
		
		this.personID = personID;
		this.personName = personName;
		
		completeConstruction (treeview, sessions, newPrefsDigitsNumber, showSex, statsJumpsType);
		
		if (statsJumpsType == 2) {
			this.operation = "MAX";
		} else {
			this.operation = "AVG";
		}
	}

	protected override void printData (string [] statValues) 
	{
		//add jump to by1Values (as a string separated by ':'
		string myReturn = "";
		int i=0;
		bool by100 = false;
		foreach (string myValue in statValues) {
			if(i == 0) {
				//don't plot AVG and SD rows
				if( myValue == Catalog.GetString("AVG") || myValue == Catalog.GetString("SD") ) {
					return;
				}
				
				if(myValue == "DjIndex" || myValue == "RjIndex" || myValue == "RjPotency" || 
						myValue == "IE" || myValue == "IUB") {
					by100 = true;
				}
			}
			if(i > 0) {
				myReturn = myReturn + ":";
			}
			myReturn = myReturn + myValue;
			i++;
		}
		if(by100) {
			by100Values.Add(myReturn);
		} else {
			by1Values.Add(myReturn);
		}
	}
	
	//private static void createGraph() 
	//public static override void CreateGraph () { 
	public override void CreateGraph () 
	{
		Gtk.Window w = new Window ("Chronojump graph");
		w.SetDefaultSize (400, 300);
		NPlot.Gtk.PlotSurface2D plot = new NPlot.Gtk.PlotSurface2D ();
		
		plot.Clear();
		
		if(sessions.Count > 1) {
			plot.Title = Catalog.GetString("Global values chart of multiple sessions");
			plotGraphMultisession (plot, by1Values, false);
			plotGraphMultisession (plot, by100Values, true);
			//this has to be always the last:
			createAxisMultisession (plot);
		} else {
			plot.Title = Catalog.GetString("Global values chart of single session");
			int columnsPlotted = 0;
			columnsPlotted = plotGraphSimplesessionJumps (plot, by1Values);
			columnsPlotted = plotGraphSimplesessionIndexes (plot, by100Values, columnsPlotted);

			//this has to be always the last:
			ArrayList allValues = new ArrayList(2); 
			for(int i=0; i < by1Values.Count ; i++) {
				allValues.Add(by1Values[i]);
			}
			for(int i=0; i < by100Values.Count ; i++) {
				allValues.Add(by100Values[i]);
			}
			createAxisSimplesession (plot, allValues);
		}
		writeLegend(plot);
		plot.Add( new Grid() );
		plot.Show ();
		w.Add (plot);
		w.ShowAll ();
	}

	private int plotGraphSimplesessionJumps(IPlotSurface2D plot, ArrayList myValues)
	{
		int xtics = myValues.Count;

		HistogramPlot hp = new HistogramPlot();
		hp.BaseWidth = 0.4f;

		double[] myData = new double[xtics];

		hp.Label = "TV (seconds)";

		int count=0;
		for(int i=0; i < myValues.Count ; i++) {
			string [] jump = myValues[i].ToString().Split(new char[] {':'});
			
			int j=0;
			foreach (string myValue in jump) 
			{
				if(j>0) {
					myData[count] = Convert.ToDouble(myValue);
					Console.WriteLine("count {0}, myData {1}", count, myData[count]);
					count ++;
				}
				j++;
			}
		}
		hp.DataSource = myData;
		hp.BrushOrientation = HistogramPlot.BrushOrientations.HorizontalFadeIn;
		hp.Filled = true;
			
		hp.Pen = Pens.Red;
		plot.Add(hp);
		
		//return the number of plotted bars 
		return count;
	}

	//values can be < 0
	private int plotGraphSimplesessionIndexes(IPlotSurface2D plot, ArrayList myValues, int cPlotted)
	{
		PointPlot pp = new PointPlot();

		double[] myData = new double[myValues.Count];

		pp.Label = "indexes (%)";

		int count = 0;
		for(int i=0; i < myValues.Count ; i++) {
			string [] jump = myValues[i].ToString().Split(new char[] {':'});
			
			int j=0;
			foreach (string myValue in jump) 
			{
				if(j>0) {
					myData[count] = Convert.ToDouble(myValue);
					Console.WriteLine("count {0}, myData {1}", count, myData[count]);
					count ++;
				}
				j++;
			}
		}
		
		pp.AbscissaData = new StartStep( cPlotted, 1 ); //ini cplotted, step 1
		pp.OrdinateData = myData;
		pp.Marker.DropLine = true;
		pp.Marker.Pen = Pens.Blue;
		pp.Marker.Filled = true;
		
		plot.Add( pp, NPlot.PlotSurface2D.XAxisPosition.Bottom, NPlot.PlotSurface2D.YAxisPosition.Right );
		
		//return the number of plotted bars 
		return count;
	}

	//private static void plotGraph(IPlotSurface2D plotSurface)
	private void plotGraphMultisession(IPlotSurface2D plot, ArrayList myValues, bool by100)
	{
		for(int i=0; i < myValues.Count ; i++) {
			string [] jump = myValues[i].ToString().Split(new char[] {':'});
			
			int xtics;
			xtics = sessions.Count +2; //each session (+2 for not having values in the left-right edges)
			
			double[] lineData = new double[xtics];
	
			System.Drawing.Color [] colorArray = { Color.Green, Color.Blue, Color.Red, Color.Chocolate,
				Color.Magenta, Color.Cyan, Color.Chartreuse };
			
			Marker m;
			if(! by100) {
				m = new Marker(Marker.MarkerType.Cross1,6,new Pen(colorArray[i],2.0F));
			} else {
				m = new Marker(Marker.MarkerType.Square, 10, new Pen(colorArray[i],2.0F));
			}
			PointPlot pp = new PointPlot( m );
									
			LinePlot lp = new LinePlot();
			lp.Color = colorArray[i];
			
			int j=0;
			//left margin
			lineData[0] = double.NaN;
		
			foreach (string myValue in jump) 
			{
				if(j==0) {
					//titol de la serie esta a by1Values[0]
					lp.Label = myValue;
					pp.Label = myValue;
				} else {
					if(myValue == "-") {
						lineData[j] = double.NaN;
					} else {
						lineData[j] = Convert.ToDouble(myValue);
					}
				}
				j++;
				//don't graph AVG and SD right cols in multisession
				if ( j > xtics -2 ) {
					break;
				}
			}
			//right margin
			lineData[j] = double.NaN;
			
			lp.DataSource = lineData;
			pp.OrdinateData = lineData;

			pp.AbscissaData = new StartStep( 0, 1 ); //ini 0, step 1 (ini 0 because in lineData we start with blank value)
			lp.ShowInLegend = false;
				
			if(! by100) {
				plot.Add( lp );
				plot.Add( pp );
			} else {
				plot.Add( lp, NPlot.PlotSurface2D.XAxisPosition.Bottom, NPlot.PlotSurface2D.YAxisPosition.Right );
				plot.Add( pp, NPlot.PlotSurface2D.XAxisPosition.Bottom, NPlot.PlotSurface2D.YAxisPosition.Right );
			}
		}
	}

	private void createAxisSimplesession (IPlotSurface2D plot, ArrayList myValues)
	{
		LabelAxis la = new LabelAxis( plot.XAxis1 );
		int count =0;
		for (int i=0; i < myValues.Count; i++) {
			string [] stringFullResults = myValues[i].ToString().Split(new char[] {':'});
			la.AddLabel( stringFullResults[0], count++ );
		}
		la.WorldMin = -.3f;
		la.WorldMax = myValues.Count + .3f;
		la.Label = Catalog.GetString("Jumps and Indexes");
		plot.XAxis1 = la;
		plot.XAxis1.LargeTickSize = 0.0f;
		plot.XAxis1.TicksLabelAngle = 35.0f;
	
		LinearAxis ly1 = (LinearAxis)plot.YAxis1;
		//ly1.WorldMin = 0.0f;
		ly1.Label = Catalog.GetString("Jumps TV (seconds)");
		
		LinearAxis ly2 = (LinearAxis)plot.YAxis2;
		//ly2.WorldMin = -100.0f;
		ly2.Label = Catalog.GetString("Indexes (%)");
	}
	
	private void createAxisMultisession (IPlotSurface2D plot)
	{
		LabelAxis la = new LabelAxis( plot.XAxis1 );
		for (int i=0; i < sessions.Count; i++) {
			string [] stringFullResults = sessions[i].ToString().Split(new char[] {':'});
			la.AddLabel( stringFullResults[1], i+1 );
		}
		la.WorldMin = 0.7f;
		la.WorldMax = sessions.Count + .3f;
		la.Label = Catalog.GetString("Sessions");
		plot.XAxis1 = la;
		plot.XAxis1.LargeTickSize = 0.0f;
		plot.XAxis1.TicksLabelAngle = 35.0f;
						 
		LinearAxis ly1 = (LinearAxis)plot.YAxis1;
		//ly1.WorldMin = 0.0f;
		ly1.Label = Catalog.GetString("Jumps TV (seconds)");
								
		LinearAxis ly2 = (LinearAxis)plot.YAxis2;
		ly2.Label = Catalog.GetString("Indexes (%)");
	}

	private void writeLegend(IPlotSurface2D plot)
	{	
		plot.Legend = new Legend();
		plot.Legend.XOffset = +30;
	}
}
