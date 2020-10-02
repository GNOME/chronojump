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
using Mono.Unix;


public class JumpsWeightFVProfileGraph : CairoXY
{
	private bool showFullGraph;
	private double pmax;
	public double fvprofile90;
	public bool needDevelopForce;
	private int imbalance;

	public enum ErrorAtStart { ALLOK, NEEDLEGPARAMS, BADLEGPARAMS, NEEDJUMPS, NEEDJUMPSX, F0ANDV0NOTPOSITIVE, F0NOTPOSITIVE, V0NOTPOSITIVE }
	private ErrorAtStart errorMessage;

	//constructor when there are no points
	public JumpsWeightFVProfileGraph (DrawingArea area, ErrorAtStart errorMessage, string font)//, string title, string jumpType, string date)
	{
		this.area = area;
		this.errorMessage = errorMessage;

		initGraph(font);

		plotError();

		endGraphDisposing();
	}

	//regular constructor
	public JumpsWeightFVProfileGraph (
			JumpsWeightFVProfile jwp,
			DrawingArea area, string title, //string jumpType,
			string date, bool showFullGraph,
			ErrorAtStart errorMessage) //errorMessage, can make the graph but show the error
	{
		this.point_l = jwp.Point_l_relative;

		//this.slope = jwp.Slope;
		//relative:
		this.slope = jwp.SfvRel;

		//this.intercept = jwp.Intercept;
		//relative:
		this.intercept = jwp.F0Rel;

		this.area = area;
		this.title = title;
		//this.jumpType = jumpType;
		this.date = date;
		this.showFullGraph = showFullGraph;
		this.errorMessage = errorMessage;
		this.colorBackground = colorFromGdk(Config.ColorBackground); //but note if we are using system colors, this will not match

		outerMargins = 50; //blank space outside the axis
		if(showFullGraph)
			innerMargins = 0;

		xVariable = speedStr;
		yVariable = forceStr;
		xUnits = "m/s";
		yUnits = "N/Kg";

		f0 = jwp.F0;
		f0Rel = jwp.F0Rel;
		v0 = jwp.V0;
		pmax = jwp.Pmax;
		fvprofile90 = jwp.FvProfileFor90();
		needDevelopForce = jwp.NeedDevelopForce();
		imbalance = jwp.Imbalance();
		f0Opt = jwp.F0Opt;
		v0Opt = jwp.V0Opt;

		LogB.Information(string.Format("pmaxRel: {0}", jwp.PmaxRel));
		LogB.Information(string.Format("z: {0}", jwp.Z));
		LogB.Information(string.Format("sfvOpt: {0}", jwp.SfvOpt));
		LogB.Information(string.Format("f0Opt: {0}, v0Opt: {1}", f0Opt, v0Opt));
		LogB.Information(string.Format("Imbalance: {0}", imbalance));
	}

	private void plotError()
	{
		string message = "";
		if(errorMessage == ErrorAtStart.NEEDLEGPARAMS)
			message = Catalog.GetString("Need to fill person's leg parameters.");
		else if(errorMessage == ErrorAtStart.BADLEGPARAMS)
			message = Catalog.GetString("Person's leg parameters are incorrect.");
		else if(errorMessage == ErrorAtStart.NEEDJUMPS)
			message = Catalog.GetString("Need to execute jumps SJl and/or SJ.");
		else if(errorMessage == ErrorAtStart.NEEDJUMPSX)
			message = Catalog.GetString("Need to execute jumps with different weights.");
		else if(errorMessage == ErrorAtStart.F0ANDV0NOTPOSITIVE)
			message = Catalog.GetString("F0 and V0 are not > 0.");
		else if(errorMessage == ErrorAtStart.F0NOTPOSITIVE)
			message = Catalog.GetString("F0 is not > 0.");
		else if(errorMessage == ErrorAtStart.V0NOTPOSITIVE)
			message = Catalog.GetString("V0 is not > 0.");

		g.SetFontSize(16);
		printText(area.Allocation.Width /2, area.Allocation.Height /2, 24, textHeight, message, g, alignTypes.CENTER);
	}

	public override void Do()
	{
		LogB.Information("at JumpsWeightFVProfileGraph.Do");
		initGraph(font);

		if(showFullGraph)
			findPointMaximums(true);
		else
			findPointMaximums(false);

		//findAbsoluteMaximums();
		paintGrid(gridTypes.BOTH, true);
		paintAxis();

		if(showFullGraph)
			plotPredictedLine(predictedLineTypes.STRAIGHT, predictedLineCrossMargins.CROSS);
		else
			plotPredictedLine(predictedLineTypes.STRAIGHT, predictedLineCrossMargins.DONOTTOUCH);

		if(errorMessage == ErrorAtStart.ALLOK)
		{
			plotAlternativeLineWithRealPoints (0, f0Opt, v0Opt, 0, showFullGraph);

			if(showFullGraph)
			{
				if(f0Opt > f0Rel)
					plotArrow (0, f0Rel, 0, f0Opt, false, 12);
				if(v0Opt > v0)
					plotArrow (v0, 0, v0Opt, 0, true, 12);
			}
		}

		plotRealPoints(false);

		writeTitle();

		if(errorMessage != ErrorAtStart.ALLOK)
			plotError();

		endGraphDisposing();
	}

	protected override void writeTitle()
	{
		int ypos = -8;

		writeTextAtRight(ypos++, title, true);
		writeTextAtRight(ypos++, "FV Profile", false);
		writeTextAtRight(ypos++, date, false);

		ypos++;
		writeTextAtRight(ypos++, string.Format("F0: {0} N", Math.Round(f0Rel,2)), false);
		writeTextAtRight(ypos++, string.Format("V0: {0} m/s", Math.Round(v0,2)), false);
		writeTextAtRight(ypos++, string.Format("Pmax: {0} W", Math.Round(pmax,1)), false);

		writeTextAtRight(ypos++, "Samozino & col. 2008-13:", false);
		writeTextAtRight(ypos++, string.Format("- Profile (90º): {0} %", Math.Round(fvprofile90,0)), false);
		writeTextAtRight(ypos++, string.Format("- Imbalance: {0} %", imbalance), false);

		g.Color = red;
		if(needDevelopForce)
			writeTextAtRight(ypos++, "- " + Catalog.GetString("Need to develop force"), false);
		else
			writeTextAtRight(ypos++, "- " + Catalog.GetString("Need to develop speed"), false);
		g.SetSourceRGB(0, 0, 0);
	}

	protected override void writeSelectedValues(int line, PointF pClosest)
	{
		double lineVertSpacing = 1;

		// first check if it fits at right, if does not fit reduce lineVertSpacing
		if(Convert.ToInt32(graphHeight/2) + textHeight*2*(line+4) > graphHeight - outerMargins)
		{
			lineVertSpacing = .5;
		}

		g.Color = bluePlots;
		writeTextAtRight(line, "Selected:", false);
		g.SetSourceRGB(0, 0, 0);

		List<KeyDouble> l_keydouble = pClosest.l_keydouble;
		/*
		foreach(KeyDouble kd in l_keydouble)
			LogB.Information(kd.ToString());
			*/

		double heightCm = (double) l_keydouble[0].D;
		double extraWeight = (double) l_keydouble[1].D;

		writeTextAtRight(line + lineVertSpacing, "- " + heightStr + string.Format(" : {0} cm", Util.TrimDecimals(heightCm, 2)), false);
		writeTextAtRight(line + 2*lineVertSpacing, "- " + extraWeightStr + string.Format(" : {0} Kg", Util.TrimDecimals(extraWeight, 2)), false);
		writeTextAtRight(line + 3*lineVertSpacing, string.Format("- {0}: {1} {2}", xVariable, Util.TrimDecimals(pClosest.X, 2), xUnits), false);
		writeTextAtRight(line + 4*lineVertSpacing, string.Format("- {0}: {1} {2}", yVariable, Util.TrimDecimals(pClosest.Y, 2), yUnits), false);
	}
}
