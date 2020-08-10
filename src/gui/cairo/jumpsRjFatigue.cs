
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

		xVariable = "Count";
		xUnits = "";

		if(heights) {
			yVariable = "Height";
			yUnits = "cm";
		} else {
			yVariable = "TF/TC";
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

		plotPredictedLine(predictedLineTypes.STRAIGHT, predictedLineCrossMargins.TOUCH);
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
		writeTextAtRight(ypos++, "Jumptype: " + jumpType, false);
		writeTextAtRight(ypos++, date, false);
	}

}
