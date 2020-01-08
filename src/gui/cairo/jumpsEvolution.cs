
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
 *  Copyright (C) 2004-2020   Xavier de Blas <xaviblas@gmail.com> 
 *  Copyright (C) 2004-2020   Jordi Rodeiro <jordirodeiro@gmail.com> 
 */

using System;
using System.Collections.Generic; //List
using Gtk;
using Cairo;


public class JumpsEvolutionGraph : CairoXY
{
	//constructor when there are no points
	public JumpsEvolutionGraph (DrawingArea area, string jumpType)//, string title, string jumpType, string date)
	{
		this.area = area;

		initGraph();

		g.SetFontSize(16);
		printText(area.Allocation.Width /2, area.Allocation.Height /2, 24, textHeight,
				string.Format("Need to execute jumps: {0}.", jumpType), g, true);

		endGraph();
	}

	//regular constructor
	public JumpsEvolutionGraph (
			List<Point> point_l, double[] coefs,
			LeastSquares.ParaboleTypes paraboleType,
			double xAtMMaxY, //x at Model MaxY
			double pointsMaxValue,
			DrawingArea area,
			string title, string jumpType, string date)
	{
		this.point_l = point_l;
		this.coefs = coefs;
		this.paraboleType = paraboleType;
		this.xAtMMaxY = xAtMMaxY;
		this.pointsMaxValue = pointsMaxValue;
		this.area = area;
		this.title = title;
		this.jumpType = jumpType;
		this.date = date;

		axisYLabel = "Height (cm)";
		axisXLabel = "Date (double)";
	}

	public override void Do()
	{
		LogB.Information("at JumpsEvolutionGraph.Do");
		initGraph();

                findPointMaximums();
                findAbsoluteMaximums();
		paintAxisAndGrid();

		LogB.Information(string.Format("coef length:{0}", coefs.Length));
		if(coefs.Length == 3)
			plotPredictedLine();

		plotRealPoints();

		if(coefs.Length == 3)
		{
			if(paraboleType == LeastSquares.ParaboleTypes.CONVEX)
			{
				plotPredictedMaxPoint();
				writeTextPredictedPoint();
			}
			else
				writeTextConcaveParabole();
		} else {
			writeTextNeed3PointsWithDifferentFall();
		}
		writeTitle();

		endGraph();
	}

	//here X is year, add half a year
	protected override void separateMinXMaxX()
	{
		minX -= .5;
		maxX += .5;
	}

	protected override void writeTitle()
	{
		writeTextAtRight(-5, title, true);
		//writeTextAtRight(-4, "Optimal fall height", false);
		writeTextAtRight(-3, "Jumptype: " + jumpType, false);
		writeTextAtRight(-2, date, false);
	}

}
