
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
 *  Copyright (C) 2004-2021   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List
using Gtk;
using Cairo;


public abstract class EvolutionGraph : CairoXY
{
	public override void Do (string font)
	{
		LogB.Information("at Jumps or runs EvolutionGraph.Do");
		initGraph(font, .8);

                findPointMaximums(false);
                //findAbsoluteMaximums();
		paintGrid(gridTypes.HORIZONTALLINES, true);
		paintAxis();
		paintGridDatetime();

		plotPredictedLine(predictedLineTypes.STRAIGHT, predictedLineCrossMargins.TOUCH);
		plotRealPoints(PlotTypes.POINTS);

		writeTitle();

		endGraphDisposing(g);
	}

	//here X is year, add/subtract third of a year
	protected override void separateMinXMaxXIfNeeded()
	{
		if(minX == maxX || maxX - minX < .1) //<.1 means that maybe we will not see any vertical bar on grid, enlarge it
		{
			minX -= .1;
			maxX += .1;
		}
	}

	//a bit recursive function ;)
	protected void paintGridDatetime()
	{
		g.Save();
		g.SetDash(new double[]{1, 2}, 0);

		bool paintMonths = (Convert.ToInt32(Math.Floor(maxX)) - Convert.ToInt32(Math.Floor(minX)) < 3); //paint months if few years
		//or if years are very separated (high X resolution)
		if(! paintMonths)
		{
			int year = Convert.ToInt32(Math.Floor(minX)) -1;
			int xtemp1 = Convert.ToInt32(calculatePaintX(year));
			int xtemp2 = Convert.ToInt32(calculatePaintX(year + 1));
			if(xtemp2 - xtemp1 > 500)
				paintMonths = true;

		}

		//-1 to start on previous year to see last months (if fit into graph)
		for(int year = Convert.ToInt32(Math.Floor(minX)) -1; year <= Convert.ToInt32(Math.Floor(maxX)); year ++)
		{
			int xtemp = Convert.ToInt32(calculatePaintX(year));
			if( ! (xtemp < outerMargin || xtemp > graphWidth - outerMargin) )
			{
				if(paintMonths)
					paintVerticalGridLine(g, xtemp, string.Format("{0} {1}", year, UtilDate.GetMonthName(0, true)), textHeight);
				else
					paintVerticalGridLine(g, xtemp, year.ToString(), textHeight);
			}

			if(! paintMonths)
				continue;

			int monthStep = 3;
			//1 get de distance between 1 month and the next one
			int xtemp1 = Convert.ToInt32(calculatePaintX(year + 1/12.0));
			int xtemp2 = Convert.ToInt32(calculatePaintX(year + 2/12.0));
			if(xtemp2 - xtemp1 > 100)
				monthStep = 1;

			for(int month = monthStep; month <= 12-monthStep; month += monthStep)
			{
				LogB.Information(string.Format("year-month: {0}-{1}", year, month));
				xtemp = Convert.ToInt32(calculatePaintX(year + month/12.0));
				if(xtemp < outerMargin || xtemp > graphWidth - outerMargin)
					continue;

				paintVerticalGridLine(g, xtemp, string.Format("{0} {1}", year, UtilDate.GetMonthName(month, true)), textHeight);
			}
		}

		g.Stroke ();
		g.Restore();
	}
}

public class JumpsEvolutionGraph : EvolutionGraph
{
	public enum Error { NEEDJUMP, TESTINCOMPATIBLE }

	//constructor when there are no points
	public JumpsEvolutionGraph (DrawingArea area, Error error, string jumpType, string font)//, string title, string jumpType, string date)
	{
		this.area = area;

		initGraph(font, .8);

		g.SetFontSize(16);

		string errorMessage = "";
		if(error == Error.NEEDJUMP)
			errorMessage = needToExecuteJumpsStr + " " + jumpType + ".";
		else //(error == Error.TESTINCOMPATIBLE)
			errorMessage = Constants.GraphCannot(jumpType);

		printText(area.Allocation.Width /2, area.Allocation.Height /2, 24, textHeight,
				errorMessage, g, alignTypes.CENTER);

		endGraphDisposing(g);
	}

	//regular constructor
	public JumpsEvolutionGraph (
			List<PointF> point_l, double slope, double intercept,
			DrawingArea area, string title, string jumpType, string date)
	{
		this.point_l = point_l;
		this.slope = slope;
		this.intercept = intercept;
		this.area = area;
		this.title = title;
		this.jumpType = jumpType;
		this.date = date;
		this.colorBackground = colorFromGdk(Config.ColorBackground); //but note if we are using system colors, this will not match

		xVariable = dateStr;
		yVariable = heightStr;
		xUnits = "";
		yUnits = "cm";
	}

	protected override void writeTitle()
	{
		int ypos = -6;

		writeTextAtRight(ypos++, title, true);
		writeTextAtRight(ypos++, jumpTypeStr + " " + jumpType, false);
		writeTextAtRight(ypos++, date, false);
	}
}

public class RunsEvolutionGraph : EvolutionGraph
{
	//constructor when there are no points
	public RunsEvolutionGraph (DrawingArea area, string runType, string font)//, string title, string runType, string date)
	{
		this.area = area;

		initGraph(font, .8);

		g.SetFontSize(16);
		printText(area.Allocation.Width /2, area.Allocation.Height /2, 24, textHeight,
				needToExecuteRunsStr + " " + runType + ".", g, alignTypes.CENTER);

		endGraphDisposing(g);
	}

	//regular constructor
	public RunsEvolutionGraph (
			List<PointF> point_l, double slope, double intercept,
			DrawingArea area, string title, string runType, string date,
			bool showTime, bool metersSecondsPreferred)
	{
		this.point_l = point_l;
		this.slope = slope;
		this.intercept = intercept;
		this.area = area;
		this.title = title;
		this.runType = runType;
		this.date = date;
		this.colorBackground = colorFromGdk(Config.ColorBackground); //but note if we are using system colors, this will not match

		xVariable = dateStr;
		if(showTime)
			yVariable = timeStr;
		else
			yVariable = speedStr;

		xUnits = "";
		if(showTime)
			yUnits = "s";
		else {
			if(metersSecondsPreferred)
				yUnits = "m/s";
			else
				yUnits = "km/h";
		}
	}

	protected override void writeTitle()
	{
		int ypos = -6;

		writeTextAtRight(ypos++, title, true);
		writeTextAtRight(ypos++, runTypeStr + " " + runType, false);
		writeTextAtRight(ypos++, date, false);
	}
}
