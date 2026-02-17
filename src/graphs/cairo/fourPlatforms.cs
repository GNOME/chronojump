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
 *  Copyright (C) 2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List
using Gtk;
using Cairo;

public class CairoGraphFourPlatforms : CairoXY
{
	//private bool horizontal;
	private int points_l_painted;
	private Constants.Modes mode;
	private List<List<PointF>> points_ll;
	private List<PointF> stepsBottom_l;
	private List<PointF> stepsTop_l;
	private List<IDName> idName_l;
	private int startAt;
	private int marginAfterInSeconds;
	private FourPlatformsCaptureManage.CaptureEnum fourPlatformsCaptureType;
	private bool capturing;
	private Cairo.Color colorBalls;
	private Cairo.Color colorSteps;

	public CairoGraphFourPlatforms (DrawingArea area)//, bool horizontal))
	{
		initFourPlatforms (area);//, bool horizontal);
	}

	private void initFourPlatforms (DrawingArea area)//, bool horizontal)
	{
		this.area = area;
		//this.horizontal = horizontal;
		this.colorBackground = colorFromRGBA(Config.ColorBackground); //but note if we are using system colors, this will not match

		points_l_painted = 0;

		//need to be small because graphHeight could be 100,
		//if margins are big then calculatePaintY could give us reverse results
		leftMargin = 40;
		//rightMargin = 40; //defined in subclasses
		topMargin = 40;
		bottomMargin = 40;

		innerMargin = 0;

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
			Constants.Modes mode,
			string title,
			List<List<PointF>> points_ll,
			List<PointF> stepsBottom_l, List<PointF> stepsTop_l,
			List<IDName> idName_l,
			FourPlatformsCaptureManage.CaptureEnum fourPlatformsCaptureType,
			bool capturing,
			bool videoShow, double videoPlayTimeInSeconds,
			int showLastSeconds,
			bool forceRedraw, PlotTypes plotType,
			Gdk.RGBA colorBalls, Gdk.RGBA colorSteps)
	{
		//LogB.Information (string.Format ("capturing: {0}, showLastSeconds: {1}", capturing, showLastSeconds));
		if (doSendingList (font,
					mode,
					title,
					points_ll,
					stepsBottom_l, stepsTop_l,
					idName_l,
					fourPlatformsCaptureType,
					capturing,
					videoShow, videoPlayTimeInSeconds,
					showLastSeconds,
					forceRedraw, plotType,
					colorFromRGBA (colorBalls), colorFromRGBA (colorSteps)))
			endGraphDisposing(g, surface, area.Window);
	}

	private bool doSendingList (string font,
			Constants.Modes mode,
			string title,
			List<List<PointF>> points_ll,
			List<PointF> stepsBottom_l, List<PointF> stepsTop_l,
			List<IDName> idName_l,
			FourPlatformsCaptureManage.CaptureEnum fourPlatformsCaptureType,
			bool capturing,
			bool videoShow, double videoPlayTimeInSeconds,
			int showLastSeconds,
			bool forceRedraw, PlotTypes plotType,
			Cairo.Color colorBalls, Cairo.Color colorSteps)
	{
		this.mode = mode;
		this.points_ll = points_ll;
		this.stepsBottom_l = stepsBottom_l;
		this.stepsTop_l = stepsTop_l;
		this.idName_l = idName_l;
		this.fourPlatformsCaptureType = fourPlatformsCaptureType;
		this.capturing = capturing;
		this.colorBalls = colorBalls;
		this.colorSteps = colorSteps;

		//force show all set when not capturing
		if (! capturing)
			showLastSeconds = -1;

		rightMargin = 40;

		//TODO: s'hauria de veure si anem actualitzant el graf mínim cada dècima de segon (fent scroll amb temps actual)
		bool maxValuesChanged = false;

		if(points_ll != null)
		{
			maxValuesChanged = findPointMaximums(false, points_ll[0], false);
			//LogB.Information(string.Format("minY: {0}, maxY: {1}", minY, maxY));

			//forced
			//minY = -4;
			//absoluteMaxY = +4;
			if (showLastSeconds > 0 && absoluteMaxX < showLastSeconds)
				absoluteMaxX = showLastSeconds;

			minY = 1 - .25;
			absoluteMaxY = 4 + .25;
		}

		bool graphInited = false;
		if( maxValuesChanged || forceRedraw ||
				(points_ll != null && points_ll[0].Count != points_l_painted)
				)
		{
			colorCairoBackground = new Cairo.Color (1, 1, 1, 1);

			initGraph (font, 1, (maxValuesChanged || forceRedraw) );
			graphInited = true;
			points_l_painted = 0;
		}

		//if( points_l == null || points_l.Count == 0)
		if( (points_ll == null || points_ll[0].Count == 0) && idName_l == null)
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

		pointsRadius = 8;

		startAt = 0;
		marginAfterInSeconds = 0;

		if (showLastSeconds > 0 && points_ll[0].Count > 1)
			startAt = configureTimeWindowHorizontal (points_ll[0], showLastSeconds, marginAfterInSeconds, 1); //data in s

		/*
		 * Note grid is having in account minX, but as we are having an scroll, things are not shown on the left, but grid starts always at 0, do do not call like this:
		paintGrid (gridTypes.VERTICALLINES, true, 0);//axisShiftToRight + 5);
		call like this:
		*/
		if (points_ll[0].Count > 0)
		{
			verticalGridLineUnits = " s";
			//gridNiceSeps = 20; // just to debug
			paintGridNiceAutoValues (g,
					points_ll[0][startAt].X,
					absoluteMaxX, minY, absoluteMaxY, gridNiceSeps, gridTypes.VERTICALLINES, 0, textHeight, true);
		}

		if (title != "")
			printText (graphWidth/2, 20, 0, textHeight, title, g, alignTypes.CENTER);

		//paint points
		if(maxValuesChanged || forceRedraw || points_ll[0].Count != points_l_painted)
			doPlot (plotType);
		else if (points_ll[0].Count == 0) //to plot just the names when there is no data
			doPlot (plotType);

		return true;
	}

	private void doPlot (PlotTypes plotType)
	{
		g.SetSourceColor (gray);
		for (int i = 1; i <= 4; i ++)
		{
			g.MoveTo (leftMargin, calculatePaintY (changeYIfNeeded (i)));
			g.LineTo (graphWidth - rightMargin, calculatePaintY (changeYIfNeeded (i)));
		}
		g.Stroke ();

		g.SetSourceColor (black);
		for (int i = 1; i <= 4; i ++)
		{
			//jumps simple
			if (mode == Constants.Modes.JUMPSSIMPLE)
			{
				if (idName_l != null && idName_l.Count == 4 && idName_l[i-1].UniqueID >= 0)
					printText (leftMargin/2, calculatePaintY (5 -i +.7), 0, textHeight +4,
							idName_l[i-1].Name, g, alignTypes.LEFT);
			} else //(mode == Constants.Modes.OTHER)
				printText (leftMargin/2, calculatePaintY (changeYIfNeeded (i)), 0, textHeight +4,
					i.ToString (), g, alignTypes.CENTER);

			g.Stroke (); //needed because if not the move to on printText makes after show a line to the following points
		}

		if (stepsBottom_l.Count > 0 && fourPlatformsCaptureType == FourPlatformsCaptureManage.CaptureEnum.FROMLOWTOHIGH) //area at bg
			paintStepsFromLowToHigh ();

		g.SetSourceColor (black);
		g.LineWidth = 2;
		if (points_ll[0].Count > 0)
			for (int i = 1; i <= 4; i ++)
			{
				if (mode == Constants.Modes.JUMPSSIMPLE)
					doPlotMarksJumpsSimple (i);
				else //(mode == Constants.Modes.OTHER)
					doPlotMarksOther (i,
							fourPlatformsCaptureType == FourPlatformsCaptureManage.CaptureEnum.DEFAULT ||
							fourPlatformsCaptureType == FourPlatformsCaptureManage.CaptureEnum.FROMLOWTOHIGH);
			}

		if (stepsBottom_l.Count > 0 && fourPlatformsCaptureType != FourPlatformsCaptureManage.CaptureEnum.FROMLOWTOHIGH)
			paintStepsOthers ();

		/*
		//debug with points_ll[0]
		plotRealPoints (plotType, points_ll[0], startAt, false); //fast (but the difference is very low)
		*/

		g.SetSourceColor (yellow);
		points_l_painted = points_ll[0].Count;
	}

	private void paintStepsFromLowToHigh ()
	{
		Cairo.Color colorLine_yellow = new Cairo.Color (0.906, 0.745, 0.098, 1); //Chronojump yellow
		Cairo.Color colorArea_yellowTransp = new Cairo.Color (0.9, 0.9, 0.01, .15);

		for (int j = 0 ; j < stepsBottom_l.Count && j < stepsTop_l.Count ; j ++)
		{
			//CairoUtil.PaintVerticalLinesAndRectangle (g, graphHeight,
			//based on CairoUtil.PaintVerticalLinesAndRectangle ...
			g.SetSourceColor (colorLine_yellow);
			g.MoveTo (calculatePaintX (stepsBottom_l[j].X), calculatePaintY (1));
			g.LineTo (calculatePaintX (stepsBottom_l[j].X), calculatePaintY (4) -2.5*pointsRadius);
			g.Stroke ();
			g.MoveTo (calculatePaintX (stepsTop_l[j].X), calculatePaintY (1));
			g.LineTo (calculatePaintX (stepsTop_l[j].X), calculatePaintY (4) -2.5*pointsRadius);
			g.Stroke ();

			// print the num
			printText (
					UtilAll.DivideSafe (
						calculatePaintX (stepsBottom_l[j].X) + calculatePaintX (stepsTop_l[j].X),
						2),
					calculatePaintY (4) -2.5*pointsRadius,
					0, textHeight, (j+1).ToString (), g, alignTypes.CENTER);

			g.SetSourceColor (colorArea_yellowTransp);
			if (stepsTop_l[j].X > stepsBottom_l[j].X)
			{
				g.Rectangle (calculatePaintX (stepsBottom_l[j].X),
						calculatePaintY (4),
						calculatePaintX (stepsTop_l[j].X) - calculatePaintX (stepsBottom_l[j].X),
						calculatePaintY (1) - calculatePaintY (4));
				g.Fill();
			}
		}
	}

	private void paintStepsOthers ()
	{
		g.SetSourceColor (colorSteps);
		for (int j = 0 ; j < stepsBottom_l.Count && j < stepsTop_l.Count ; j ++)
		{
			g.MoveTo (calculatePaintX (stepsBottom_l[j].X),
					calculatePaintY (changeYIfNeeded (stepsBottom_l[j].Y)));
			g.LineTo (calculatePaintX (stepsTop_l[j].X),
					calculatePaintY (changeYIfNeeded (stepsTop_l[j].Y)));
			g.Stroke ();
		}
	}

	private void doPlotMarksJumpsSimple (int i) //person (row)
	{
		if (points_ll[i].Count == 1)
		{
			drawCircle (g, calculatePaintX (points_ll[i][0].X),
					calculatePaintY (points_ll[i][0].Y),
					3, black, true);
			return;
		}

		for (int j = 0; j < points_ll[i].Count; j ++)
		{
			if (j == 0)
				g.MoveTo (calculatePaintX (points_ll[i][j].X), calculatePaintY (points_ll[i][j].Y));
			else {
				g.LineTo (calculatePaintX (points_ll[i][j].X), calculatePaintY (points_ll[i][j-1].Y));
				g.LineTo (calculatePaintX (points_ll[i][j].X), calculatePaintY (points_ll[i][j].Y));
			}
		}
		g.Stroke ();

		//2 draw the boxes (air), they will also overlap the tc line
	}

	private void doPlotMarksOther (int i, bool drawLine) //person (row)
	{
		for (int j = points_ll[i].Count -1; j >= 0 && points_ll[i][j].X >= points_ll[0][startAt].X ; j --)
		{
			/*
			LogB.Information (string.Format (
						"points doPlotMarksOther i: {0}, j: {1}, points_ll[i][j].X: {2}, points_ll[i][j].Y: {3}",
						i, j, points_ll[i][j].X, points_ll[i][j].Y));
			*/

			if (points_ll[i][j].Y > 5-i) 	//ON: filled
			{
				drawCircle (g, calculatePaintX (points_ll[i][j].X),
						calculatePaintY (changeYIfNeeded (i)),
						pointsRadius, colorSteps, colorBalls);

				continue;
			}

			if (points_ll[i][j].Y < 5-i) //if OFF, should be empty and draw the line to ON at left
			{
				drawCircle (g, calculatePaintX (points_ll[i][j].X),
						calculatePaintY (changeYIfNeeded(i)),
						pointsRadius, colorSteps, white);

				//double drawLineToX = calculatePaintX (0);
				double drawLineToX = calculatePaintX (points_ll[0][startAt].X) + pointsRadius;
				if (j -1 >= 0 && points_ll[i][j-1].Y > 5-i && //ON: filled
						points_ll[i][j-1].X >= points_ll[0][startAt].X)
				{
					drawCircle (g, calculatePaintX (points_ll[i][j-1].X),
							calculatePaintY (changeYIfNeeded(i)),
							pointsRadius, colorBalls, true);
					drawLineToX = calculatePaintX (points_ll[i][j-1].X) + pointsRadius;
				}

				if (drawLine && calculatePaintX (points_ll[i][j].X) - pointsRadius - drawLineToX > 0)
				{
					g.MoveTo (calculatePaintX (points_ll[i][j].X) - pointsRadius,
							calculatePaintY (changeYIfNeeded(i)));
					g.LineTo (drawLineToX, calculatePaintY (changeYIfNeeded(i)));
					g.Stroke ();
				}
			}
		}
	}

	private double changeYIfNeeded (double y)
	{
		if (mode != Constants.Modes.OTHER)
			return y;
		if (fourPlatformsCaptureType != FourPlatformsCaptureManage.CaptureEnum.FROMLOWTOHIGH)
			return y;

		// have bottom lines (1,2) closer
		if (y == 2)
			return y -.33;
		// have top lines (3,4) closer
		if (y == 3)
			return y +.33;
		return y;
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
