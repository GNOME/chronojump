
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
	public enum ErrorAtStart { NEEDLEGPARAMS, BADLEGPARAMS, NEEDJUMPS, NEEDJUMPSX, F0NOTPOSITIVE, V0NOTPOSITIVE }
	private bool showFullGraph;
	private double pmax;
	public double fvprofile90;
	public bool needDevelopForce;
	private int imbalance;

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
		else if(error == ErrorAtStart.NEEDJUMPS)
			message = "Need to execute jumps SJl and/or SJ.";
		else if(error == ErrorAtStart.NEEDJUMPSX)
			message = "Need to execute jumps with different weights.";
		else if(error == ErrorAtStart.F0NOTPOSITIVE)
			message = "F0 is not > 0.";
		else //if(error == ErrorAtStart.V0NOTPOSITIVE)
			message = "V0 is not > 0.";

		g.SetFontSize(16);
		printText(area.Allocation.Width /2, area.Allocation.Height /2, 24, textHeight, message, g, true);

		endGraph();
	}

	//regular constructor
	public JumpsWeightFVProfileGraph (
			JumpsWeightFVProfile jwp,
			DrawingArea area, string title, //string jumpType,
			string date, bool showFullGraph)
	{
		this.point_l = jwp.Point_l;
		this.slope = jwp.Slope;
		this.intercept = jwp.Intercept;
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

		f0 = jwp.F0;
		v0 = jwp.V0;
		pmax = jwp.Pmax;
		fvprofile90 = jwp.FvProfileFor90();
		needDevelopForce = jwp.NeedDevelopForce();
		imbalance = jwp.Imbalance();

		LogB.Information(string.Format("Imbalance: {0}", imbalance));
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

		if(showFullGraph)
			plotPredictedLine(predictedLineTypes.STRAIGHT, predictedLineCrossMargins.CROSS);
		else
			plotPredictedLine(predictedLineTypes.STRAIGHT, predictedLineCrossMargins.DONOTTOUCH);

		plotRealPoints();

		writeTitle();

		endGraph();
	}

	protected override void writeTitle()
	{
		int ypos = -8;

		writeTextAtRight(ypos++, title, true);
		writeTextAtRight(ypos++, "FV Profile", false);
		writeTextAtRight(ypos++, date, false);

		ypos++;
		writeTextAtRight(ypos++, string.Format("F0: {0} N", Math.Round(f0,2)), false);
		writeTextAtRight(ypos++, string.Format("V0: {0} m/s", Math.Round(v0,2)), false);
		writeTextAtRight(ypos++, string.Format("Pmax: {0} W", Math.Round(pmax,1)), false);

		writeTextAtRight(ypos++, "Samozino & col. 2008-13:", false);
		writeTextAtRight(ypos++, string.Format("- Profile (90º): {0} %", Math.Round(fvprofile90,0)), false);
		if(needDevelopForce)
			writeTextAtRight(ypos++, "- Need to develop force", false);
		else
			writeTextAtRight(ypos++, "- Need to develop speed", false);
		writeTextAtRight(ypos++, string.Format("- Imbalance: {0} %", imbalance), false);
	}

	protected override void writeSelectedValues(int line, PointF pClosest)
	{
		writeTextAtRight(line, "Selected:", false);

		List<KeyDouble> l_keydouble = pClosest.l_keydouble;
		/*
		foreach(KeyDouble kd in l_keydouble)
			LogB.Information(kd.ToString());
			*/

		double heightCm = (double) l_keydouble[0].D;
		double extraWeight = (double) l_keydouble[1].D;

		writeTextAtRight(line +1, string.Format("- Height: {0} cm", Util.TrimDecimals(heightCm, 2)), false);
		writeTextAtRight(line +2, string.Format("- Extra weight: {0} Kg", Util.TrimDecimals(extraWeight, 2)), false);
		writeTextAtRight(line +3, string.Format("- {0}: {1} {2}", xVariable, Util.TrimDecimals(pClosest.X, 2), xUnits), false);
		writeTextAtRight(line +4, string.Format("- {0}: {1} {2}", yVariable, Util.TrimDecimals(pClosest.Y, 2), yUnits), false);
	}

}
