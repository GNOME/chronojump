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
 *  Copyright (C) 2024   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List
using Gtk;
using Cairo;

public class CairoGraphFourPlatforms : CairoXY
{
	//private bool horizontal;
	private int points_l_painted;
	private List<PointF> points_l; //if butterworth, this will be it
	private int startAt;
	private int marginAfterInSeconds;
	private bool capturing;

	public CairoGraphFourPlatforms (DrawingArea area, string title)//, bool horizontal))
	{
		initFourPlatforms (area, title);//, bool horizontal);
	}

	private void initFourPlatforms (DrawingArea area, string title)//, bool horizontal)
	{
		this.area = area;
		this.title = title;
		//this.horizontal = horizontal;
		this.colorBackground = colorFromRGBA(Config.ColorBackground); //but note if we are using system colors, this will not match

		points_l_painted = 0;

		//need to be small because graphHeight could be 100,
		//if margins are big then calculatePaintY could give us reverse results
		leftMargin = 40;
		//rightMargin = 40; //defined in subclasses
		topMargin = 40;
		bottomMargin = 40;

		innerMargin = 20;

		yVariable = "";
		yUnits = "";

		xAtMaxY = 0;
		yAtMaxY = 0;
		xAtMinY = 0;
		yAtMinY = 0;

		gridNiceSeps = 7;
	}

	//separated in two methods to ensure endGraphDisposing on any return of the other method
	public void DoSendingList (string font,
			List<PointF> points_l,
			bool capturing,
			bool videoShow, double videoPlayTimeInSeconds,
			int showLastSeconds,
			bool forceRedraw, PlotTypes plotType)
	{
		if (doSendingList (font,
					points_l,
					capturing,
					videoShow, videoPlayTimeInSeconds,
					showLastSeconds,
					forceRedraw, plotType))
			endGraphDisposing(g, surface, area.Window);
	}

	private bool doSendingList (string font,
			List<PointF> points_l,
			bool capturing,
			bool videoShow, double videoPlayTimeInSeconds,
			int showLastSeconds,
			bool forceRedraw, PlotTypes plotType)
	{
		this.points_l = points_l;
		this.capturing = capturing;

		rightMargin = 40;

		bool maxValuesChanged = false;

		if(points_l != null)
		{
			maxValuesChanged = findPointMaximums(false, points_l);
			//LogB.Information(string.Format("minY: {0}, maxY: {1}", minY, maxY));

			//forced
			minY = -4;
			absoluteMaxY = +4;
		}

		bool graphInited = false;
		if( maxValuesChanged || forceRedraw ||
				(points_l != null && points_l.Count != points_l_painted)
				)
		{
			colorCairoBackground = new Cairo.Color (1, 1, 1, 1);

			initGraph (font, 1, (maxValuesChanged || forceRedraw) );
			graphInited = true;
			points_l_painted = 0;
		}

		if( points_l == null || points_l.Count == 0)
		{
			if (! graphInited)
			{
				initGraph (font, 1, true);
				graphInited = true;
			}
			return graphInited;
		}

		//fix an eventual crash on g.LineWidth below
		if(g == null || ! graphInited)
			return false;

		//this try/catch is an extra precaution
		try {
			g.LineWidth = 1;
		} catch {
			LogB.Information("Catched on CairoGraphForceSensorSignal soSendingList() g.LineWidth");
			return graphInited;
		}
		pointsRadius = 10;

		startAt = 0;
		marginAfterInSeconds = 0;

		//marginAfterInSeconds = 3;
		if (showLastSeconds > 0 && points_l.Count > 1)
		{
			//if (horizontal)
				startAt = configureTimeWindowHorizontal (points_l, showLastSeconds, marginAfterInSeconds, 10000); //10 s
			//else
			//	startAt = configureTimeWindowVertical (points_l, showLastSeconds, marginAfterInSeconds, 10000);
		}

		// paint points and maybe interpolated path
		if(maxValuesChanged || forceRedraw || points_l.Count != points_l_painted)
			doPlot (plotType);

		return true;
	}

	private void doPlot (PlotTypes plotType)
	{
		g.SetSourceColor (white);

		if (points_l.Count > 0)
		{
			g.SetSourceColor (gray);
			for (int i = -4; i <= 4; i ++)
			{
				if (i == 0)
					continue;

				g.MoveTo (leftMargin, calculatePaintY (i));
				g.LineTo (graphWidth - rightMargin, calculatePaintY (i));
				g.Stroke ();
				printText (leftMargin-4, calculatePaintY (i), 0, textHeight +4,
						i.ToString (), g, alignTypes.RIGHT);
			}
		}

		g.SetSourceColor (black);
		plotRealPoints (plotType, points_l, startAt, false); //fast (but the difference is very low)

		points_l_painted = points_l.Count;
	}

	protected override void writeTitle()
	{
	}

	//instead of use totalMargins, use leftMargin and rightMargin to allow feedback path head be inside the graph (not at extreme right)
	protected override double calculatePaintX (double realX)
	{
                return leftMargin + innerMargin + (realX - minX) * UtilAll.DivideSafe(
				graphWidth -(leftMargin + rightMargin) -2*innerMargin,
				absoluteMaxX - minX);
        }
}
