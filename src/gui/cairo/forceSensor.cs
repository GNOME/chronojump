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
 *  Copyright (C) 2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List
using Gtk;
using Cairo;


public class CairoGraphForceSensorSignal : CairoXY
{
	private int points_list_painted;
	private int pathLineWidthInN;
	private int accuracySamplesGood;
	private int accuracySamplesBad;

	//regular constructor
	public CairoGraphForceSensorSignal (DrawingArea area, string title, int pathLineWidthInN)
	{
		this.area = area;
		this.title = title;
		this.pathLineWidthInN = pathLineWidthInN;
		this.colorBackground = colorFromGdk(Config.ColorBackground); //but note if we are using system colors, this will not match
		
		//doing = false;
		points_list_painted = 0;
		accuracySamplesGood = 0;
		accuracySamplesBad = 0;

		//need to be small because graphHeight could be 100,
		//if margins are big then calculatePaintY could give us reverse results
		leftMargin = 30;
		topMargin = 30;
		bottomMargin = 30;
		outerMargin = 30; //outerMargin has to be the same than topMargin & bottomMargin to have grid arrive to the margins
		innerMargin = 10;

		yVariable = forceStr;
		yUnits = "N";

		xAtMaxY = 0;
		yAtMaxY = 0;
		xAtMinY = 0;
		yAtMinY = 0;

		gridNiceSeps = 7;
	}

	//separated in two methods to ensure endGraphDisposing on any return of the other method
	public void DoSendingList (string font,
			List<PointF> points_list,
			List<PointF> points_list_interpolated_path, int interpolatedMin, int interpolatedMax,
			bool capturing,
			int showLastSeconds,
			int minDisplayFNegative, int minDisplayFPositive,
			int rectangleN, int rectangleRange,
			bool forceRedraw, PlotTypes plotType)
	{
		if(doSendingList (font, points_list,
					points_list_interpolated_path, interpolatedMin, interpolatedMax,
					capturing,
					showLastSeconds,
					minDisplayFNegative, minDisplayFPositive,
					rectangleN, rectangleRange,
					forceRedraw, plotType))
			endGraphDisposing(g, surface, area.GdkWindow);
	}

	//similar to encoder method but calling configureTimeWindow and using minDisplayF(Negative/Positive)
	//return true if graph is inited (to dispose it)
	private bool doSendingList (string font,
			List<PointF> points_list,
			List<PointF> points_list_interpolated_path, int interpolatedMin, int interpolatedMax,
			bool capturing,
			int showLastSeconds,
			int minDisplayFNegative, int minDisplayFPositive,
			int rectangleN, int rectangleRange,
			bool forceRedraw, PlotTypes plotType)
	{
		bool maxValuesChanged = false;

		if(points_list != null)
		{
			maxValuesChanged = findPointMaximums(false, points_list);
			//LogB.Information(string.Format("minY: {0}, maxY: {1}", minY, maxY));

			if (minY > minDisplayFNegative)
				minY = minDisplayFNegative;
			if (absoluteMaxY < minDisplayFPositive)
				absoluteMaxY = minDisplayFPositive;

			if (rectangleRange > 0)
			{
				if (rectangleN < 0 && rectangleN - rectangleRange/2 < minY)
					minY = rectangleN - rectangleRange/2;
				if (rectangleN > 0 && rectangleN + rectangleRange/2 > absoluteMaxY)
					absoluteMaxY = rectangleN + rectangleRange/2;
			}

			if (points_list_interpolated_path != null && points_list_interpolated_path.Count > 0)
			{
				if (interpolatedMin - pathLineWidthInN/2 < minY)
					minY = interpolatedMin - pathLineWidthInN/2;
				if (interpolatedMax + pathLineWidthInN/2 > absoluteMaxY)
					absoluteMaxY = interpolatedMax + pathLineWidthInN/2;
			}
		}

		bool graphInited = false;
		if( maxValuesChanged || forceRedraw ||
				(points_list != null && points_list.Count != points_list_painted)
				)
		{
			initGraph (font, 1, (maxValuesChanged || forceRedraw) );
			graphInited = true;
			points_list_painted = 0;
		}

		//do not draw axis at the moment (and it is not in 0Y right now)
		//if(maxValuesChanged || forceRedraw)
		//	paintAxis();

		if( points_list == null || points_list.Count == 0)
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
		pointsRadius = 1;

		int startAt = 0;
		if (showLastSeconds > 0 && points_list.Count > 1)
			startAt = configureTimeWindow (points_list, showLastSeconds);

		if(maxValuesChanged || forceRedraw || points_list.Count != points_list_painted)
		{
			if (rectangleRange > 0)
				paintRectangle (rectangleN, rectangleRange);

			if (points_list.Count > 2) //to ensure minX != maxX
				paintGrid(gridTypes.BOTH, true);

			paintAxis();

			if (points_list_interpolated_path != null && points_list_interpolated_path.Count > 0)
			{
				Cairo.Color colorPathBlue = colorFromRGB (178,223,238);
				g.LineWidth = calculatePathWidth ();
				g.SetSourceColor (colorPathBlue);

				//make the line start at startAt +1 (if possible) and draw cicle at left (startAt) to have nice rounding at left.
				int startAt_theLine = startAt;
				if (points_list.Count -1 > startAt)
					startAt_theLine = startAt +1;

				plotRealPoints(plotType, points_list_interpolated_path, startAt_theLine, true); //fast (but the difference is very low)

				//circle at left
				drawCircle (calculatePaintX (points_list_interpolated_path[startAt].X),
						calculatePaintY (points_list_interpolated_path[startAt].Y),
						g.LineWidth/2, colorPathBlue, true);

				Cairo.Color colorHead = colorPathBlue;

				string accuracyText = "";
				if (points_list[points_list.Count -1].X < 5000000)
				{
					accuracyText = string.Format ("Accuracy calculation starts in {0} s",
							Convert.ToInt32 (UtilAll.DivideSafe(5000000 - points_list[points_list.Count -1].X, 1000000)));
				}
				else {
					//compare last point painted with circle at right
					double error = getDistance2D (calculatePaintX (points_list[points_list.Count -1].X),
							calculatePaintY (points_list[points_list.Count -1].Y),
							calculatePaintX (points_list_interpolated_path[points_list_interpolated_path.Count -1].X),
							calculatePaintY (points_list_interpolated_path[points_list_interpolated_path.Count -1].Y));

					accuracyText = string.Format ("Accuracy {0} %", Util.TrimDecimals (100 * UtilAll.DivideSafe (accuracySamplesGood, (accuracySamplesGood + accuracySamplesBad)), 1));

					if (error > g.LineWidth/2)
					{
						colorHead = colorFromRGB (238, 0, 0);

						//avoid to change the results on a resize after capture
						if (capturing)
							accuracySamplesBad ++; //but need to check the rest of the sample points, not only last
					} else
						if (capturing)
							accuracySamplesGood ++; //but need to check the rest of the sample points, not only last
				}

				//circle at right
				drawCircle (calculatePaintX (points_list_interpolated_path[points_list_interpolated_path.Count -1].X),
						calculatePaintY (points_list_interpolated_path[points_list_interpolated_path.Count -1].Y),
						g.LineWidth/2, colorHead, true);

				g.SetSourceColor (black);

				g.SetFontSize (textHeight +4);
				printText (graphWidth/2, outerMargin, 0, textHeight +4,
						accuracyText, g, alignTypes.CENTER);
				g.SetFontSize (textHeight);

				g.LineWidth = 2;
			}

			plotRealPoints(plotType, points_list, startAt, true); //fast (but the difference is very low)

			if(calculatePaintX (xAtMaxY) > leftMargin)
				drawCircle (calculatePaintX (xAtMaxY), calculatePaintY (yAtMaxY), 8, red, false);

			if(calculatePaintX (xAtMinY) > leftMargin)
				drawCircle (calculatePaintX (xAtMinY), calculatePaintY (yAtMinY), 8, red, false);

			points_list_painted = points_list.Count;
		}

		return true;
	}

	private double calculatePathWidth ()
	{
		return Math.Abs (calculatePaintY (pathLineWidthInN) - calculatePaintY (0));
	}

	//for signals like forceSensor where points_list.X is time in microseconds and there is not a sample for each second (like encoder)
	private int configureTimeWindow (List<PointF> points_list, int seconds)
	{
		//double firstTime = points_list[0].X; //micros
		double lastTime = points_list[points_list.Count -1].X; //micros
		//LogB.Information (string.Format ("firstTime: {0}, lastTime: {1}, elapsed: {2}", firstTime, lastTime, lastTime - firstTime));

		absoluteMaxX = lastTime;
		if (absoluteMaxX < seconds * 1000000)
			absoluteMaxX = seconds * 1000000;

		int startAt = PointF.FindSampleAtTimeToEnd (points_list, seconds * 1000000); //s to ms
		minX = points_list[startAt].X;

		return startAt;
	}

	private void paintRectangle (int rectangleN, int rectangleRange)
	{
		// 1) paint the light blue rectangle
		//no need to be transparent as the rectangle is un the bottom layer
		//g.SetSourceRGBA(0.6, 0.8, 1, .5); //light blue
		g.SetSourceRGB(0.6, 0.8, 1); //light blue

		g.Rectangle (leftMargin +innerMargin,
				calculatePaintY (rectangleN +rectangleRange/2),
				graphWidth -rightMargin -leftMargin -innerMargin,
				calculatePaintY (rectangleN -rectangleRange/2) - calculatePaintY (rectangleN +rectangleRange/2));
		g.Fill();

		// 2) paint the dark blue center line
		g.SetSourceRGB(0.3, 0.3, 1); //dark blue
		g.Save ();
		g.LineWidth = 3;
		g.MoveTo (leftMargin +innerMargin, calculatePaintY (rectangleN));
		g.LineTo (graphWidth -rightMargin, calculatePaintY (rectangleN));
		g.Stroke();
		g.Restore();

		g.SetSourceRGB(0, 0, 0);
	}

	protected override void paintVerticalGridLine(Cairo.Context g, int xtemp, string text, int fontH)
	{
		if(fontH < 1)
			fontH = 1;

		g.MoveTo(xtemp, topMargin);
		g.LineTo(xtemp, graphHeight - bottomMargin);

		if (Util.IsNumber (text, false))
		{
			double micros = Convert.ToDouble (text);
			text = string.Format ("{0}s", UtilAll.DivideSafe (micros, 1000000));
		}
		printText(xtemp, graphHeight -bottomMargin/2, 0, fontH, text, g, alignTypes.CENTER);
	}
	protected override void writeTitle()
	{
	}
}
