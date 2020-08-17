
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
	//constructor when there are no points
	public JumpsRjFatigueGraph (DrawingArea area, string jumpType)//, string title, string jumpType, string date)
	{
		this.area = area;

		initGraph();

		g.SetFontSize(16);
		//printText(area.Allocation.Width /2, area.Allocation.Height /2, 24, textHeight,
		//		string.Format("Need to execute jumps: {0}.", jumpType), g, true);

		endGraph();
	}
	public JumpsRjFatigueGraph (
			List<PointF> point_l, double slope, double intercept,
			DrawingArea area, string title, string jumpType, string date, bool heights)
	{
		this.point_l = point_l;
		this.slope = slope;
		this.intercept = intercept;
		this.area = area;
		this.title = title;
		this.jumpType = jumpType;
		this.date = date;

		xVariable = countStr;
		xUnits = "";

		if(heights) {
			yVariable = heightStr;
			yUnits = "cm";
		} else {
			yVariable = tfStr + "/" + tcStr;
			yUnits = "";
		}
	}

	public override void Do()
	{
		LogB.Information("at JumpsRjFatigueGraph.Do");
		initGraph();

                findPointMaximums(false);
                //findAbsoluteMaximums();
		paintGrid(gridTypes.HORIZONTALLINES, true);
		paintGrid(gridTypes.VERTICALLINES, false);
		paintAxis();

		g.Color = red;
		plotPredictedLine(predictedLineTypes.STRAIGHT, predictedLineCrossMargins.TOUCH);
		g.Color = black;

		divideInTwoAndPlotAverage();

		plotRealPoints(true);

		writeTitle();

		endGraph();
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

	private void divideInTwoAndPlotAverage()
	{
		List<PointF> point_l_start = new List<PointF>();
		List<PointF> point_l_end = new List<PointF>();
		double sumIni = 0;
		double sumEnd = 0;

		int i = 0;
		foreach(PointF point in point_l)
		{
			if(i < Math.Floor(point_l.Count / 2.0))
			{
				point_l_start.Add(point);
				sumIni += point.Y;
				//LogB.Information(string.Format("Added to point_l_start: {0}", point));
			}
			if(point_l.Count - i -1 < Math.Floor(point_l.Count / 2.0))
			{
				point_l_end.Add(point);
				sumEnd += point.Y;
				//LogB.Information(string.Format("Added to point_l_end: {0}", point));
			}

			i ++;
		}

		if(point_l_start.Count == 0 || point_l_end.Count == 0)
			return;

		double avgIni = sumIni / point_l_start.Count;
		double avgEnd = sumEnd / point_l_end.Count;

		g.MoveTo(calculatePaintX(point_l_start[0].X), calculatePaintY(avgIni));
		g.LineTo(calculatePaintX(point_l_start[point_l_start.Count -1].X), calculatePaintY(avgIni));
		printText(
				//Convert.ToInt32(calculatePaintX(point_l_start[0].X)) -2,
				graphWidth - outerMargins,
				Convert.ToInt32(calculatePaintY(avgIni)),
				0, textHeight, Util.TrimDecimals(avgIni, 2), g, alignTypes.CENTER);

		g.MoveTo(calculatePaintX(point_l_end[0].X), calculatePaintY(avgEnd));
		g.LineTo(calculatePaintX(point_l_end[point_l_end.Count -1].X), calculatePaintY(avgEnd));
		printText(
				//Convert.ToInt32(calculatePaintX(point_l_end[point_l_end.Count -1].X)) +2,
				graphWidth - outerMargins,
				Convert.ToInt32(calculatePaintY(avgEnd)),
				0, textHeight, Util.TrimDecimals(avgEnd, 2), g, alignTypes.CENTER);

		g.Stroke ();
	}
}
