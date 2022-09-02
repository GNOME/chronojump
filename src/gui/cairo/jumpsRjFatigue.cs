
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


public class JumpsRjFatigueGraph : CairoXY
{
	private List<double> timesAccu_l;
	private int divideIn;

	//constructor when there are no points
	public JumpsRjFatigueGraph (DrawingArea area, string jumpType, string font)//, string title, string jumpType, string date)
	{
		this.area = area;

		initGraph(font, .8);

		g.SetFontSize(16);
		//printText(area.Allocation.Width /2, area.Allocation.Height /2, 24, textHeight,
		//		string.Format("Need to execute jumps: {0}.", jumpType), g, true);

		endGraphDisposing(g, surface, area.GdkWindow);
	}
	public JumpsRjFatigueGraph (
			List<double> timesAccu_l,
			List<PointF> point_l, double slope, double intercept,
			DrawingArea area, string title, string jumpType, string date,
			JumpsRjFatigue.Statistic statistic, int divideIn)
	{
		this.timesAccu_l = timesAccu_l;
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

		area.AddEvents((int) Gdk.EventMask.ButtonPressMask); //to have mouse clicks
		mouseX = -1;
		mouseY = -1;
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

		g.SetSourceColor(red);

		if(point_l.Count > 1)
			plotPredictedLine(predictedLineTypes.STRAIGHT, predictedLineCrossMargins.TOUCH);
		//g.SetSourceColor(black);
		g.SetSourceColor(gray99);
		divideAndPlotAverage (divideIn, true);
		g.SetSourceColor(black);

		plotRealPoints(PlotTypes.POINTSLINES);

		writeTitle();
		addClickableMark(g);

		if(mouseX >= 0 && mouseY >= 0)
			calculateAndWriteRealXY ();

		endGraphDisposing(g, surface, area.GdkWindow);
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

	private void divideAndPlotAverage (int parts, bool byTime) //byTime or byJumps
	{
		if(point_l.Count < parts * 2)
			return;

		List<List<PointF>> l_l = new List<List<PointF>> ();
		if (byTime)
			l_l = divideAndPlotAverageByTime (parts);
		else
			l_l = divideAndPlotAverageByJumps (parts);

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

	private List<List<PointF>> divideAndPlotAverageByTime (int parts)
	{
		return SplitListByTime (timesAccu_l, point_l, parts);
	}

	private List<List<PointF>> divideAndPlotAverageByJumps (int parts)
	{
		return SplitListByJumps (point_l, Convert.ToInt32(Math.Floor(point_l.Count / (1.0 * parts))));
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

	public static List<List<PointF>> SplitListByTime (List<double> timesAccu_l, List<PointF> p_l, int parts)
	{
		var l_l = new List<List<PointF>>();

		if (p_l.Count <= 1)
			return l_l;

		double partRange = timesAccu_l[timesAccu_l.Count -1] / (1.0 * parts);

		for (int p = 0; p < parts; p ++)
		{
			int start = -1;
			int count = 0;
			int j;
			for (j = 0; j < timesAccu_l.Count ; j ++)
			{
				LogB.Information (string.Format ("p: {0}, j: {1}, timesAccu_l[j]: {2}, partRange: {3}", p, j, timesAccu_l[j], partRange));
				if ( timesAccu_l[j] >= partRange * p && (
							timesAccu_l[j] < partRange * (p+1) ||
							p == parts -1) ) //to have also the last value
				{
					if (start < 0)
						start = j;
					count ++;
				}
			}

			if (start >= 0 && count > 0)
			{
				LogB.Information (string.Format ("p: {0}, j: {1}, start: {2}, count: {3}", p, j, start, count));
				l_l.Add (p_l.GetRange (start, count));
			}
		}

		return l_l;
	}

	//https://stackoverflow.com/a/11463800
	public static List<List<PointF>> SplitListByJumps (List<PointF> p_l, int nSize)
	{
		var l_l = new List<List<PointF>>();

		for (int i = 0; i < p_l.Count; i += nSize)
			l_l.Add (p_l.GetRange (i, Math.Min (nSize, p_l.Count - i)));

		return l_l;
	}

}
