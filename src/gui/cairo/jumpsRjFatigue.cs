
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
 *  Copyright (C) 2020   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Collections.Generic; //List
using Gtk;
using Cairo;


public class JumpsRjFatigueGraph : CairoXY
{
	private JumpsRjFatigue.Statistic statistic;

	private int divideIn;

	//constructor when there are no points
	public JumpsRjFatigueGraph (DrawingArea area, string jumpType, string font)//, string title, string jumpType, string date)
	{
		this.area = area;

		initGraph(font, .8);

		g.SetFontSize(16);
		//printText(area.Allocation.Width /2, area.Allocation.Height /2, 24, textHeight,
		//		string.Format("Need to execute jumps: {0}.", jumpType), g, true);

		endGraphDisposing(g);
	}
	public JumpsRjFatigueGraph (
			List<PointF> point_l, double slope, double intercept,
			DrawingArea area, string title, string jumpType, string date,
			JumpsRjFatigue.Statistic statistic, int divideIn)
	{
		this.point_l = point_l;
		this.slope = slope;
		this.intercept = intercept;
		this.area = area;
		this.title = title;
		this.jumpType = jumpType;
		this.date = date;
		this.divideIn = divideIn;
		this.colorBackground = colorFromGdk(Config.ColorBackground); //but note if we are using system colors, this will not match

		xVariable = countStr;
		xUnits = "";

		if(statistic == JumpsRjFatigue.Statistic.HEIGHTS)
		{
			yVariable = heightStr;
			yUnits = "cm";
		} else if(statistic == JumpsRjFatigue.Statistic.Q)
		{
			yVariable = tfStr + "/" + tcStr;
			yUnits = "";
		} else //if(statistic == JumpsRjFatigue.Statistic.RSI)
		{
			yVariable = heightStr + "/" + tcStr;
			yUnits = "m/s";
		}
	}

	public override void Do(string font)
	{
		LogB.Information("at JumpsRjFatigueGraph.Do");
		initGraph(font, .8);

                findPointMaximums(false);
                //findAbsoluteMaximums();
		paintGrid(gridTypes.HORIZONTALLINES, true);
		paintGrid(gridTypes.VERTICALLINES, false);
		paintAxis();

		g.Color = red;
		plotPredictedLine(predictedLineTypes.STRAIGHT, predictedLineCrossMargins.TOUCH);
		//g.Color = black;
		g.Color = gray99;
		divideAndPlotAverage(divideIn);
		g.Color = black;

		plotRealPoints(PlotTypes.LINES);

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

	protected override void writeTitle()
	{
		int ypos = -6;

		writeTextAtRight(ypos++, title, true);
		writeTextAtRight(ypos++, jumpTypeStr + " " + jumpType, false);
		writeTextAtRight(ypos++, date, false);
	}

	private void divideAndPlotAverage(int parts)
	{
		if(point_l.Count < parts * 2)
			return;

		List<List<PointF>> l_l = SplitList(point_l, Convert.ToInt32(Math.Floor(point_l.Count / (1.0 * parts))));
		int done = 0;
		foreach(List<PointF> l in l_l)
		{
			if(done >= parts)
				break; //to fix i SplitList returned more chunks than wanted

			double sum = 0;
			foreach(PointF p in l)
				sum += p.Y;
			paintHorizSegment (l[0].X, l[l.Count -1].X, sum / l.Count);
			done ++;
		}
	}

	private void paintHorizSegment (double xa, double xb, double y)
	{
		double xap = calculatePaintX(xa);
		double xbp = calculatePaintX(xb);
		double yp = calculatePaintY(y);

		g.MoveTo(xap, yp);
		g.LineTo(xbp, yp);

		//paint also small y marks
		g.MoveTo(xap, yp - pointsRadius);
		g.LineTo(xap, yp + pointsRadius);
		g.MoveTo(xbp, yp - pointsRadius);
		g.LineTo(xbp, yp + pointsRadius);

		printText(
				graphWidth - outerMargin,
				Convert.ToInt32(yp),
				0, textHeight, Util.TrimDecimals(y, 2), g, alignTypes.CENTER);

		g.Stroke ();
	}

	//https://stackoverflow.com/a/11463800
	public static List<List<PointF>> SplitList(List<PointF> listMain, int nSize)
	{
		var l_l = new List<List<PointF>>();

		for (int i = 0; i < listMain.Count; i += nSize)
			l_l.Add(listMain.GetRange(i, Math.Min(nSize, listMain.Count - i)));

		return l_l;
	}

}
