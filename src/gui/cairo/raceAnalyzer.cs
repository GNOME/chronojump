
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
 *  Copyright (C) 2021   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Collections.Generic; //List
using Gtk;
using Cairo;


public class CairoGraphRaceAnalyzer : CairoXY
{
	int points_list_painted;
	private bool plotHorizArrowFromMaxY;

	/*
	//constructor when there are no points
	public CairoGraphRaceAnalyzer (DrawingArea area, string jumpType, string font)//, string title, string jumpType, string date)
	{
		this.area = area;

		initGraph(font);

		g.SetFontSize(16);
		printText(area.Allocation.Width /2, area.Allocation.Height /2, 24, textHeight,
				needToExecuteJumpsStr + " " + jumpType + ".", g, alignTypes.CENTER);

		endGraphDisposing(g);
	}
	*/

	//to avoid to have new data on PassData while the for is working on plotRealPoints
//	static bool doing;
	//regular constructor
	public CairoGraphRaceAnalyzer (
			DrawingArea area, string title,
			string yVariable, string yUnits,
			bool plotHorizArrowFromMaxY)
	{
		this.area = area;
		this.title = title;
		this.colorBackground = colorFromGdk(Config.ColorBackground); //but note if we are using system colors, this will not match

		xVariable = timeStr;
		this.yVariable = yVariable;
		xUnits = "s";
		this.yUnits = yUnits;
		this.plotHorizArrowFromMaxY = plotHorizArrowFromMaxY;
		
//		doing = false;
		points_list_painted = 0;
	}

	public void Reset()
	{
		minX = 1000000;
		maxX = 0;
		minY = 1000000;
		maxY = 0;
		absoluteMaxX = 0;
		absoluteMaxY = 0;

		points_list_painted = 0;
	}

	public override void DoSendingList (string font, List<PointF> points_list, bool forceRedraw, PlotTypes plotType)
	{
		LogB.Information("at RaceAnalyzerGraph.Do");

		bool initGraphDone = false;
		bool maxValuesChanged = false;
		if(points_list != null)
			maxValuesChanged = findPointMaximums(false, points_list);

		if(maxValuesChanged || forceRedraw || points_list.Count != points_list_painted)
		{
			initGraph( font, 1, (maxValuesChanged || forceRedraw) );
			initGraphDone = true;
			points_list_painted = 0;
		}

		if(maxValuesChanged || forceRedraw)
		{
			paintGrid(gridTypes.BOTH, true);
			paintAxis();
		}

		pointsRadius = 1;
		if( points_list != null &&
				(maxValuesChanged || forceRedraw || points_list.Count != points_list_painted) )
		{
			plotRealPoints(plotType, points_list, points_list_painted);
			points_list_painted = points_list.Count;

			if(plotHorizArrowFromMaxY && points_list.Count > 1)
			{
				MovingAverage mAverage = new MovingAverage(points_list, 5);
				mAverage.Calculate();
				PointF pMaxY = mAverage.GetMaxY();
				plotArrowPassingRealPoints (g, colorFromRGB(255,0,0),
						pMaxY.X, pMaxY.Y, points_list[points_list.Count -1].X, pMaxY.Y, true, false, 0);
			}
		}

		if(initGraphDone)
			endGraphDisposing(g);

		//doing = false;
	}

	protected override void printXAxisText()
	{
		printText(graphWidth - outerMargin, graphHeight -Convert.ToInt32(.25 * outerMargin), 0, textHeight, getXAxisLabel(), g, alignTypes.CENTER);
	}
	protected override void paintVerticalGridLine(Cairo.Context g, int xtemp, string text, int fontH)
	{
		g.MoveTo(xtemp, graphHeight - outerMargin);
		g.LineTo(xtemp, outerMargin);
		printText(xtemp, graphHeight -Convert.ToInt32(.75 * outerMargin), 0, fontH, text, g, alignTypes.CENTER); //TODO: this only for raceAnalyzer
	}

	protected override void writeTitle()
	{
	}

}
