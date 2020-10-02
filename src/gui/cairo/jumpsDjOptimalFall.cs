
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


public class JumpsDjOptimalFallGraph : CairoXY
{
	//constructor when there are no points
	public JumpsDjOptimalFallGraph (DrawingArea area, string jumpType, string font)//, string title, string jumpType, string date)
	{
		this.area = area;

		initGraph(font);

		g.SetFontSize(16);
		printText(area.Allocation.Width /2, area.Allocation.Height /2, 24, textHeight,
				needToExecuteJumpsStr + " " + jumpType + ".", g, alignTypes.CENTER);

		endGraphDisposing();
	}

	//regular constructor
	public JumpsDjOptimalFallGraph (
			List<PointF> point_l, double[] coefs,
			LeastSquaresParabole.ParaboleTypes paraboleType,
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
		this.colorBackground = colorFromGdk(Config.ColorBackground); //but note if we are using system colors, this will not match

		xVariable = fallStr;
		yVariable = heightStr;
		xUnits = "cm";
		yUnits = "cm";

		//needed to have mouse clicks at: on_drawingarea_jumps_weight_fv_profile_button_press_event ()
//		area.AddEvents((int) (Gdk.EventMask.ButtonPressMask | Gdk.EventMask.ButtonReleaseMask));
		area.AddEvents((int) Gdk.EventMask.ButtonPressMask);
	}

	public override void Do()
	{
		LogB.Information("at JumpsDjOptimalFallGraph.Do");
		initGraph(font);

                findPointMaximums(false);
                findAbsoluteMaximums();
		paintGrid(gridTypes.BOTH, true);
		paintAxis();

		LogB.Information(string.Format("coef length:{0}", coefs.Length));
		if(coefs.Length == 3)
			plotPredictedLine(predictedLineTypes.PARABOLE, predictedLineCrossMargins.TOUCH);

		plotRealPoints(false);

		if(coefs.Length == 3)
		{
			if(paraboleType == LeastSquaresParabole.ParaboleTypes.CONVEX)
			{
				plotPredictedMaxPoint();
				writeTextPredictedPoint();
				predictedPointDone = true;
			}
			else
				writeTextConcaveParabole();
		} else {
			writeTextNeed3PointsWithDifferentFall();
		}
		writeTitle();

		endGraphDisposing();
	}

	protected override void writeTitle()
	{
		int ypos = -6;

		writeTextAtRight(ypos++, title, true);
		writeTextAtRight(ypos++, optimalFallHeightStr, false);
		writeTextAtRight(ypos++, jumpTypeStr + " " + jumpType, false);
		writeTextAtRight(ypos++, date, false);
	}

}
