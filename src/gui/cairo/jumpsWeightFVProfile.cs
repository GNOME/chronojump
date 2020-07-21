
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


public class JumpsWeightFVProfileGraph : CairoXY
{
	public enum ErrorAtStart { NEEDLEGPARAMS, BADLEGPARAMS, NEEDJUMPS }
	private double pmax;
	private bool showFullGraph;

	//constructor when there are no points
	public JumpsWeightFVProfileGraph (DrawingArea area, ErrorAtStart error)//, string title, string jumpType, string date)
	{
		this.area = area;

		initGraph();

		string message = "";
		if(error == ErrorAtStart.NEEDLEGPARAMS)
			message = "Need to fill person's leg parameters.";
		else if(error == ErrorAtStart.BADLEGPARAMS)
			message = "Person's leg parameters are incorrect.";
		else //if(error == ErrorAtStart.NEEDJUMPS)
			message = "Need to execute jumps SJl and/or SJ.";

		g.SetFontSize(16);
		printText(area.Allocation.Width /2, area.Allocation.Height /2, 24, textHeight, message, g, true);

		endGraph();
	}

	//regular constructor
	public JumpsWeightFVProfileGraph (
			List<PointF> point_l, double slope, double intercept,
			DrawingArea area, string title, //string jumpType,
			string date, bool showFullGraph)
	{
		this.point_l = point_l;
		this.slope = slope;
		this.intercept = intercept;
		this.area = area;
		this.title = title;
		//this.jumpType = jumpType;
		this.date = date;
		this.showFullGraph = showFullGraph;

		outerMargins = 50; //blank space outside the axis
		if(showFullGraph)
			innerMargins = 0;

		xVariable = "Speed";
		yVariable = "Force";
		xUnits = "m/s";
		yUnits = "N";

		f0 = intercept;
		v0 = -f0 / slope;
		pmax = (f0 * v0) /4;
	}

	public override void Do()
	{
		LogB.Information("at JumpsWeightFVProfileGraph.Do");
		initGraph();

		if(showFullGraph)
			findPointMaximums(true);
		else
			findPointMaximums(false);

		//findAbsoluteMaximums();
		paintAxisAndGrid(gridTypes.BOTH);

		plotPredictedLine(predictedLineTypes.STRAIGHT);
		plotRealPoints();

		writeTitle();

		endGraph();
	}

	protected override void writeTitle()
	{
		int ypos = -6;

		writeTextAtRight(ypos++, title, true);
		writeTextAtRight(ypos++, "FV Profile", false);
		writeTextAtRight(ypos++, date, false);

		ypos++;
		writeTextAtRight(ypos++, string.Format("F0: {0} N", Math.Round(f0,2)), false);
		writeTextAtRight(ypos++, string.Format("V0: {0} m/s", Math.Round(v0,2)), false);
		writeTextAtRight(ypos++, string.Format("Pmax: {0} W", Math.Round(pmax,1)), false);
//		writeTextAtRight(ypos++, "Imbalance: ?", false);
	}
}
