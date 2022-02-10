
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
 *  Copyright (C) 2022   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List
using Gtk;
using Cairo;


public class CairoGraphRaceAnalyzer : CairoXY
{
	int points_list_painted;
	private bool plotHorizArrowFromMaxY;
	private int verticalGridSep;
	private TwoListsOfDoubles verticalLinesUs_2l;

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
			bool plotHorizArrowFromMaxY,
			int verticalGridSep, //-1 if auto
			TwoListsOfDoubles verticalLinesUs_2l)
	{
		this.area = area;
		this.title = title;
		this.colorBackground = colorFromGdk(Config.ColorBackground); //but note if we are using system colors, this will not match

		xVariable = timeStr;
		this.yVariable = yVariable;
		xUnits = "s";
		this.yUnits = yUnits;
		this.plotHorizArrowFromMaxY = plotHorizArrowFromMaxY;
		this.verticalGridSep = verticalGridSep;
		this.verticalLinesUs_2l = verticalLinesUs_2l;
		
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
			if(verticalGridSep < 0 && verticalLinesUs_2l.Count() == 0)
				paintGrid(gridTypes.BOTH, true);
			else {
				//horizontal
				if(verticalGridSep <= 0)
					paintGrid(gridTypes.HORIZONTALLINES, true);
				else
					paintGridInt (g, minX, absoluteMaxX, minY, absoluteMaxY, verticalGridSep, gridTypes.HORIZONTALLINES, textHeight);

				//vertical
				if(verticalLinesUs_2l.Count() == 0)
					paintGridNiceAutoValues (g, minX, absoluteMaxX, minY, absoluteMaxY, 5, gridTypes.VERTICALLINES, textHeight);
				else {
					for(int i = 0 ; i < verticalLinesUs_2l.Count() ; i ++)
					{
						string xTextTop = verticalLinesUs_2l.GetFromFirst(i).ToString();

						//seconds
						string xTextBottom = Util.TrimDecimals(verticalLinesUs_2l.GetFromSecond(i)/1000000.0, 1).ToString();
						double xGraph = calculatePaintX(verticalLinesUs_2l.GetFromSecond(i)/1000000.0);

						g.Save();
						g.SetDash(new double[]{1, 2}, 0);
						if(verticalGridSep > 0)
							paintVerticalGridLine(g, Convert.ToInt32(xGraph), xTextBottom, textHeight-3);
						else
							paintVerticalGridLineTopBottom (g, Convert.ToInt32(xGraph), xTextTop, xTextBottom, textHeight-3);
						g.Stroke ();
						g.Restore();
					}
				}
				printXAxisTopText();
			}

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
		printText(graphWidth - outerMargin, graphHeight -Convert.ToInt32(.25 * outerMargin),
				0, textHeight, getXAxisLabel(), g, alignTypes.RIGHT);
	}
	private void printXAxisTopText()
	{
		printText(graphWidth - outerMargin, Convert.ToInt32(.5 * outerMargin),
				0, textHeight, getAxisLabel(distanceStr, "m"), g, alignTypes.RIGHT);
	}

	/*
	protected override void paintVerticalGridLine(Cairo.Context g, int xtemp, string text, int fontH)
	{
		g.MoveTo(xtemp, graphHeight - outerMargin);
		g.LineTo(xtemp, outerMargin);
		printText(xtemp, graphHeight -Convert.ToInt32(.75 * outerMargin), 0, fontH, text, g, alignTypes.CENTER); //TODO: this only for raceAnalyzer
	}
	*/

	private void paintVerticalGridLineTopBottom (Cairo.Context g, int xtemp, string textTop, string textBottom, int fontH)
	{
		if(fontH < 1)
			fontH = 1;

		g.MoveTo(xtemp, topMargin);
		g.LineTo(xtemp, graphHeight - bottomMargin);

		printText(xtemp, Convert.ToInt32(.8*topMargin), 0, fontH, textTop, g, alignTypes.CENTER);
		printText(xtemp, graphHeight-(bottomMargin/2), 0, fontH, textBottom, g, alignTypes.CENTER);
		//LogB.Information("pvgl fontH: " + fontH.ToString());
	}
	protected override void writeTitle()
	{
	}

}
