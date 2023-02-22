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

public abstract class CairoGraphForceSensor : CairoXY
{
	protected int points_l_painted;

	protected int pathLineWidthInN;

	protected int minDisplayFNegative;
	protected int minDisplayFPositive;
	protected int rectangleN;
	protected int rectangleRange;
	protected List<PointF> points_l_interpolated_path;
	protected int interpolatedMin;
	protected int interpolatedMax;

	protected void initForceSensor (DrawingArea area, string title)
	{
		this.area = area;
		this.title = title;
		this.colorBackground = colorFromGdk(Config.ColorBackground); //but note if we are using system colors, this will not match

		points_l_painted = 0;

		//need to be small because graphHeight could be 100,
		//if margins are big then calculatePaintY could give us reverse results
		leftMargin = 40;
		rightMargin = 40;
		topMargin = 40;
		bottomMargin = 40;
		outerMargin = 40; //outerMargin has to be the same than topMargin & bottomMargin to have grid arrive to the margins
		innerMargin = 20;

		yVariable = forceStr;
		yUnits = "N";
		yRightVariable = distanceStr;
		yRightUnits = "m";

		xAtMaxY = 0;
		yAtMaxY = 0;
		xAtMinY = 0;
		yAtMinY = 0;

		gridNiceSeps = 7;
	}

	protected void fixMaximums ()
	{
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

		if (points_l_interpolated_path != null && points_l_interpolated_path.Count > 0)
		{
			if (interpolatedMin - pathLineWidthInN/2 < minY)
				minY = interpolatedMin - pathLineWidthInN/2;
			if (interpolatedMax + pathLineWidthInN/2 > absoluteMaxY)
				absoluteMaxY = interpolatedMax + pathLineWidthInN/2;
			//make the head of the worm fit inside the graph increasing rightMargin if needed
			if  (calculatePathWidth ()/2 > rightMargin)
				rightMargin = Convert.ToInt32 (Math.Ceiling (calculatePathWidth ()/2));
		}
	}

	protected double calculatePathWidth ()
	{
		return Math.Abs (calculatePaintY (pathLineWidthInN) - calculatePaintY (0));
	}

	//instead of use totalMargins, use leftMargin and rightMargin to allow feedback path head be inside the graph (not at extreme right)
	protected override double calculatePaintX (double realX)
	{
                return leftMargin + innerMargin + (realX - minX) * UtilAll.DivideSafe(
				graphWidth -(leftMargin + rightMargin) -2*innerMargin,
				absoluteMaxX - minX);
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

	protected void paintRectangle (int rectangleN, int rectangleRange)
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

	protected void paintTriggers (List<PointF> points_l, TriggerList triggerList)
	{
		g.LineWidth = 1;
		int bucleStartPoints = points_l.Count -2; //to start searching points for each trigger since last one

		foreach (Trigger trigger in triggerList.GetList())
		{
			for (int i = bucleStartPoints; i >= 0; i --)
			{
				/* This fixes crash when triggers are done after force capture end
				 * triggers are searched from last (right) to first (left).
				 * If right is just at the end, bucleStartPoints will be set at end, and i will fail at next iteration
				 */
				if (i +1 >= points_l.Count)
					continue;

				if (points_l[i].X <= trigger.Us)
				{
					int bestFit = i+1;
					if (MathUtil.PassedSampleIsCloserToCriteria (
								points_l[i].X, points_l[i+1].X, trigger.Us))
						bestFit = i;

					paintVerticalTriggerLine (g, trigger, timeUnits.MICROSECONDS,
							Util.TrimDecimals (points_l[bestFit].Y,1), textHeight -3);

					bucleStartPoints = bestFit;
					break;
				}
			}
		}
		g.SetSourceColor (black);
	}
	
	//this is painted after the 1st serie because this changes the mins and max to be used on calculatePaintY
	protected void paintAnotherSerie (List<PointF> p_l, int startAt, PlotTypes plotType, Cairo.Color color)
	{
		g.SetSourceColor (color);

		findPointMaximums (false, p_l);
		fixMaximums ();
		paintGrid (gridTypes.HORIZONTALLINESATRIGHT, true);
		paintAxisRight ();
		plotRealPoints (plotType, p_l, startAt, false); //fast (but the difference is very low)
	}

	protected override void writeTitle()
	{
	}
}

public class CairoGraphForceSensorSignal : CairoGraphForceSensor
{
	private int accuracySamplesGood;
	private int accuracySamplesBad;
	private Cairo.Color colorPathBlue = colorFromRGB (178,223,238);
	private int startAt;

	//regular constructor
	public CairoGraphForceSensorSignal (DrawingArea area, string title, int pathLineWidthInN)
	{
		initForceSensor (area, title);

		this.pathLineWidthInN = pathLineWidthInN;

		//doing = false;
		accuracySamplesGood = 0;
		accuracySamplesBad = 0;
	}

	//separated in two methods to ensure endGraphDisposing on any return of the other method
	public void DoSendingList (string font,
			List<PointF> points_l,
			List<PointF> pointsDispl_l,
			List<PointF> points_l_interpolated_path, int interpolatedMin, int interpolatedMax,
			bool capturing, bool showAccuracy, int showLastSeconds,
			int minDisplayFNegative, int minDisplayFPositive,
			int rectangleN, int rectangleRange,
			TriggerList triggerList,
			bool forceRedraw, PlotTypes plotType)
	{
		this.minDisplayFNegative = minDisplayFNegative;
		this.minDisplayFPositive = minDisplayFPositive;
		this.rectangleN = rectangleN;
		this.rectangleRange = rectangleRange;
		this.points_l_interpolated_path = points_l_interpolated_path;
		this.interpolatedMin = interpolatedMin;
		this.interpolatedMax = interpolatedMax;

		if (doSendingList (font, points_l,
					capturing, showAccuracy, showLastSeconds,
					triggerList,
					forceRedraw, plotType))
		{
			if (pointsDispl_l != null && pointsDispl_l.Count > 0)
				paintAnotherSerie (pointsDispl_l, startAt, plotType, bluePlots);

			endGraphDisposing(g, surface, area.GdkWindow);
		}
	}

	//similar to encoder method but calling configureTimeWindow and using minDisplayF(Negative/Positive)
	//return true if graph is inited (to dispose it)
	private bool doSendingList (string font,
			List<PointF> points_l,
			bool capturing, bool showAccuracy, int showLastSeconds,
			TriggerList triggerList,
			bool forceRedraw, PlotTypes plotType)
	{
		bool maxValuesChanged = false;

		if(points_l != null)
		{
			maxValuesChanged = findPointMaximums(false, points_l);
			//LogB.Information(string.Format("minY: {0}, maxY: {1}", minY, maxY));

			fixMaximums ();
		}

		bool graphInited = false;
		if( maxValuesChanged || forceRedraw ||
				(points_l != null && points_l.Count != points_l_painted)
				)
		{
			initGraph (font, 1, (maxValuesChanged || forceRedraw) );
			graphInited = true;
			points_l_painted = 0;
		}

		//do not draw axis at the moment (and it is not in 0Y right now)
		//if(maxValuesChanged || forceRedraw)
		//	paintAxis();

		//LogB.Information (string.Format ("doSendingList B points_l == null: {0}", (points_l == null)));
		//if (points_l != null)
		//	LogB.Information (string.Format ("doSendingList C points_l.Count: {0}", points_l.Count));

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
		pointsRadius = 1;

		startAt = 0;
		if (showLastSeconds > 0 && points_l.Count > 1)
			startAt = configureTimeWindow (points_l, showLastSeconds);

		// paint points and maybe interpolated path
		if(maxValuesChanged || forceRedraw || points_l.Count != points_l_painted)
		{
			if (rectangleRange > 0)
				paintRectangle (rectangleN, rectangleRange);

			if (points_l.Count > 2) //to ensure minX != maxX
				paintGrid(gridTypes.BOTH, true);

			paintAxis();

			bool accuracyNowIn = true;
			Cairo.Color colorHead = colorPathBlue;

			//calculate the accuracy on rectangle and path
			string accuracyText = "";
			if (showAccuracy &&
					(rectangleRange > 0 ||
					 (points_l_interpolated_path != null && points_l_interpolated_path.Count > 0)))
			{
				if (points_l[points_l.Count -1].X < 5000000)
				{
					accuracyText = string.Format ("Accuracy calculation starts in {0} s",
							Convert.ToInt32 (UtilAll.DivideSafe(5000000 - points_l[points_l.Count -1].X, 1000000)));
				}
				else {
					if (rectangleRange > 0)
					{
						accuracyNowIn =
							(rectangleN + rectangleRange/2 >= points_l[points_l.Count -1].Y &&
							rectangleN - rectangleRange/2 <= points_l[points_l.Count -1].Y);
					} else {
						//compare last point painted with circle at right
						double error = getDistance2D (calculatePaintX (points_l[points_l.Count -1].X),
								calculatePaintY (points_l[points_l.Count -1].Y),
								calculatePaintX (points_l_interpolated_path[points_l_interpolated_path.Count -1].X),
								calculatePaintY (points_l_interpolated_path[points_l_interpolated_path.Count -1].Y));

						if (error > calculatePathWidth ()/2)
							accuracyNowIn = false;
					}

					accuracyText = string.Format ("Accuracy {0} %", Util.TrimDecimals (100 * UtilAll.DivideSafe (accuracySamplesGood, (accuracySamplesGood + accuracySamplesBad)), 1));

					if (accuracyNowIn)
					{
						//avoid to change the results on a resize after capture
						if (capturing)
							accuracySamplesGood ++; //but need to check the rest of the sample points, not only last
					} else
					{
						colorHead = colorFromRGB (238, 0, 0);

						if (capturing)
							accuracySamplesBad ++; //but need to check the rest of the sample points, not only last
					}
				}
			}

			if (rectangleRange > 0 && showAccuracy)
			{
				g.SetSourceColor (black);

				g.SetFontSize (textHeight +4);
				printText (graphWidth/2, outerMargin, 0, textHeight +4,
						accuracyText, g, alignTypes.CENTER);
				g.SetFontSize (textHeight);

				g.LineWidth = 2;
			}
			else if (points_l_interpolated_path != null && points_l_interpolated_path.Count > 0)
			{
				g.LineWidth = calculatePathWidth ();
				g.SetSourceColor (colorPathBlue);

				//make the line start at startAt +1 (if possible) and draw cicle at left (startAt) to have nice rounding at left.
				int startAt_theLine = startAt;
				if (points_l.Count -1 > startAt)
					startAt_theLine = startAt +1;

				plotRealPoints(plotType, points_l_interpolated_path, startAt_theLine, false); //fast (but the difference is very low)

				//circle at left
				drawCircle (calculatePaintX (points_l_interpolated_path[startAt].X),
						calculatePaintY (points_l_interpolated_path[startAt].Y),
						g.LineWidth/2, colorPathBlue, true);

				//circle at right
				drawCircle (calculatePaintX (points_l_interpolated_path[points_l_interpolated_path.Count -1].X),
						calculatePaintY (points_l_interpolated_path[points_l_interpolated_path.Count -1].Y),
						g.LineWidth/2, colorHead, true);

				g.SetSourceColor (black);

				if (showAccuracy)
				{
					g.SetFontSize (textHeight +4);
					printText (graphWidth/2, outerMargin, 0, textHeight +4,
							accuracyText, g, alignTypes.CENTER);
					g.SetFontSize (textHeight);
				}

				g.LineWidth = 2;
			}

			plotRealPoints(plotType, points_l, startAt, false); //fast (but the difference is very low)

			if(calculatePaintX (xAtMaxY) > leftMargin)
				drawCircle (calculatePaintX (xAtMaxY), calculatePaintY (yAtMaxY), 8, red, false);

			if(calculatePaintX (xAtMinY) > leftMargin)
				drawCircle (calculatePaintX (xAtMinY), calculatePaintY (yAtMinY), 8, red, false);

			points_l_painted = points_l.Count;
		}

		// paint triggers
		if (points_l != null && points_l.Count > 3 && graphInited && triggerList != null && triggerList.Count() > 0)
			paintTriggers (points_l, triggerList);

		return true;
	}

	//for signals like forceSensor where points_l.X is time in microseconds and there is not a sample for each second (like encoder)
	private int configureTimeWindow (List<PointF> points_l, int seconds)
	{
//LogB.Information ("configureTimeWindow 0");
		//double firstTime = points_l[0].X; //micros

		/*
LogB.Information ("points_l.Count");
LogB.Information (points_l.Count.ToString());
LogB.Information ("points_l[points_l.Count-1].X");
LogB.Information (points_l[points_l.Count-1].X.ToString());
*/

		double lastTime = points_l[points_l.Count -1].X; //micros
		//LogB.Information (string.Format ("firstTime: {0}, lastTime: {1}, elapsed: {2}", firstTime, lastTime, lastTime - firstTime));

		absoluteMaxX = lastTime;
		if (absoluteMaxX < seconds * 1000000)
			absoluteMaxX = seconds * 1000000;

//LogB.Information ("configureTimeWindow 2");
		int startAt = PointF.FindSampleAtTimeToEnd (points_l, seconds * 1000000); //s to ms
//LogB.Information ("configureTimeWindow 3");
		minX = points_l[startAt].X;
//LogB.Information ("configureTimeWindow 4");

		return startAt;
	}

}

public class CairoGraphForceSensorAI : CairoGraphForceSensor
{
	private Cairo.Color colorGreen = colorFromRGB (0,200,0);
	private Cairo.Color colorBlue = colorFromRGB (0,0,200);
	private ForceSensorExercise exercise;
	private RepetitionMouseLimitsWithSamples repMouseLimits;
	private int startAt;

	//regular constructor
	public CairoGraphForceSensorAI (DrawingArea area, string title)
	{
		initForceSensor (area, title);
	}

	//separated in two methods to ensure endGraphDisposing on any return of the other method
	public RepetitionMouseLimitsWithSamples DoSendingList (
			string font,
			List<PointF> points_l,
			List<PointF> pointsDispl_l,
			int minDisplayFNegative, int minDisplayFPositive,
			int rectangleN, int rectangleRange,
			TriggerList triggerList,
			int hscaleSampleA, int hscaleSampleB, bool zoomed,
			int fMaxAvgSampleStart, int fMaxAvgSampleEnd, double fMaxAvgForce,
			ForceSensorExercise exercise, List<ForceSensorRepetition> reps_l,
			bool forceRedraw, PlotTypes plotType)
	{
		this.minDisplayFNegative = minDisplayFNegative;
		this.minDisplayFPositive = minDisplayFPositive;
		this.rectangleN = rectangleN;
		this.rectangleRange = rectangleRange;
		this.points_l_interpolated_path = new List<PointF> ();

		this.exercise = exercise;

		repMouseLimits = new RepetitionMouseLimitsWithSamples ();
		area.AddEvents((int) Gdk.EventMask.ButtonPressMask); //to have mouse clicks

		if (doSendingList (font, points_l,
					triggerList,
					hscaleSampleA, hscaleSampleB, zoomed,
					fMaxAvgSampleStart, fMaxAvgSampleEnd, fMaxAvgForce,
					exercise, reps_l,
					forceRedraw, plotType))
		{
			if (pointsDispl_l != null && pointsDispl_l.Count > 0)
				paintAnotherSerie (pointsDispl_l, startAt, plotType, bluePlots);

			endGraphDisposing(g, surface, area.GdkWindow);
		}

		return repMouseLimits;
	}

	//similar to encoder method but calling configureTimeWindow and using minDisplayF(Negative/Positive)
	//return true if graph is inited (to dispose it)
	private bool doSendingList (string font,
			List<PointF> points_l,
			TriggerList triggerList,
			int hscaleSampleA, int hscaleSampleB, bool zoomed,
			int fMaxAvgSampleStart, int fMaxAvgSampleEnd, double fMaxAvgForce,
			ForceSensorExercise exercise, List<ForceSensorRepetition> reps_l,
			bool forceRedraw, PlotTypes plotType)
	{
		bool maxValuesChanged = false;

		if(points_l != null)
		{
			maxValuesChanged = findPointMaximums(false, points_l);
			//LogB.Information(string.Format("minY: {0}, maxY: {1}", minY, maxY));

			fixMaximums ();
		}

		bool graphInited = false;
		if ( maxValuesChanged || forceRedraw ||
				(points_l != null && points_l.Count != points_l_painted) )
		{
			initGraph (font, 1, (maxValuesChanged || forceRedraw) );
			graphInited = true;
			points_l_painted = 0;
		}

		if (points_l == null || points_l.Count == 0)
		{
			if (! graphInited)
			{
				initGraph (font, 1, true);
				graphInited = true;
			}
			return graphInited;
		}

		//fix an eventual crash on g.LineWidth below
		if (g == null || ! graphInited)
			return false;

		//this try/catch is an extra precaution
		try {
			g.LineWidth = 1;
		} catch {
			LogB.Information("Catched on CairoGraphForceSensorSignal soSendingList() g.LineWidth");
			return graphInited;
		}
		pointsRadius = 1;
		startAt = 0;

		// paint points and maybe interpolated path
		if(maxValuesChanged || forceRedraw || points_l.Count != points_l_painted)
		{
			if (rectangleRange > 0)
				paintRectangle (rectangleN, rectangleRange);

			if (points_l.Count > 2) //to ensure minX != maxX
				paintGrid(gridTypes.BOTH, true);

			paintAxis();

			plotRealPoints(plotType, points_l, startAt, false); //fast (but the difference is very low)

			// paint the AB rectangle
			// hscales start at 1.
			LogB.Information (string.Format ("doSendingList hscales: hscaleSampleA: {0}, hscaleSampleB: {1}, points_l.Count: {2}",
						hscaleSampleA, hscaleSampleB, points_l.Count));

			// hscales start at 0
			if (hscaleSampleA >= 0 && hscaleSampleB >= 0 &&
					points_l.Count > hscaleSampleA && points_l.Count > hscaleSampleB)
				CairoUtil.PaintVerticalLinesAndRectangle (g, graphHeight,
						calculatePaintX (points_l[hscaleSampleA].X),
						calculatePaintX (points_l[hscaleSampleB].X),
						true, 15, 0);

			// paint the repetition lines and codes
			if (reps_l.Count > 0)
			{
				g.LineWidth = 1;
				g.SetSourceColor (colorBlue);

				// for RepetitionsShowTypes.BOTHSEPARATED to write correctly e or c
				int sepCount = 0;
				bool lastIsCon = true;
				double xgStart;
				double xgEnd;

				int iAll = 0;
				int iAccepted = 0;
				foreach (ForceSensorRepetition rep in reps_l)
				{
					// 0) manage sepCount before the continue's to show numbers correctly on BOTHSEPARATED
					if (exercise.RepetitionsShow == ForceSensorExercise.RepetitionsShowTypes.BOTHSEPARATED)
					{
						if (lastIsCon && rep.TypeShort() == "c")
							sepCount ++;
						else if (! lastIsCon)
							sepCount ++;

						lastIsCon = (rep.TypeShort() == "c");
					}

					// 1) if the rep does not overlap because on zoom ends before A, do not paint it
					//    | rep |  A    B
					if (zoomed && rep.sampleEnd <= 0)
					{
						iAll ++;
						continue;
					}
					// 2) if the rep does not overlap because on zoom starts after B, do not paint it
					//    A    B  | rep |
					else if (zoomed && rep.sampleStart >= points_l.Count)
					{
						iAll ++;
						continue;
					}

					bool arrowL = false;
					bool arrowR = false;

					// 3) rep starts before A, paint an arrow to A (and write text in the center of A and the end of rep
					// hscales:     A      B
					// rep:      |  rep  |
					// show:        <----|
					if (zoomed && rep.sampleStart < 0)
					{
						arrowL = true;
						xgStart = calculatePaintX (points_l[0].X);
					} else
						xgStart = calculatePaintX (points_l[rep.sampleStart].X);

					// 4) rep ends after B, paint an arrow to B (and write text in the center of the end of rep and B
					// hscales:     A      B
					// rep:            |  rep  |
					// show:           |--->
					LogB.Information (string.Format ("rep.sampleStart: {0}, rep.sampleEnd: {1}, points_l.Count: {2}",
								rep.sampleStart, rep.sampleEnd, points_l.Count));
					if (zoomed && rep.sampleEnd >= points_l.Count)
					{
						arrowR = true;
						xgEnd = calculatePaintX (points_l[points_l.Count -1].X);
					} else
						xgEnd = calculatePaintX (points_l[rep.sampleEnd].X);

					//display arrows if needed
					if (arrowL && arrowR)
					{
						plotArrowFree (g, colorBlue, 1, 8, false,
								(xgStart + xgEnd) /2, textHeight +6,
								xgStart, textHeight +6);
						plotArrowFree (g, colorBlue, 1, 8, false,
								(xgStart + xgEnd) /2, textHeight +6,
								xgEnd, textHeight +6);
					} else if (arrowL)
						plotArrowFree (g, colorBlue, 1, 8, false,
								xgEnd, textHeight +6,
								xgStart, textHeight +6);
					else if (arrowR)
						plotArrowFree (g, colorBlue, 1, 8, false,
								xgStart, textHeight +6,
								xgEnd, textHeight +6);
					else
						CairoUtil.PaintSegment (g,
								xgStart, textHeight +6,
								xgEnd, textHeight +6);

					// display left vertical line if does not overlap a previous right vertical line
					if (! arrowL && (iAccepted == 0 || (iAccepted > 0 && points_l[rep.sampleStart].X > points_l[reps_l[iAll-1].sampleEnd].X)))
						CairoUtil.PaintSegment (g,
								xgStart, textHeight +6,
								xgStart, graphHeight -outerMargin);

					// display right vertical line
					if (! arrowR)
						CairoUtil.PaintSegment (g,
								xgEnd, textHeight +6,
								xgEnd, graphHeight -outerMargin);

					writeRepetitionCode (iAll, rep.TypeShort(), sepCount,
							xgStart, xgEnd, rep.sampleStart > 0, true);

					//store x,y to select the repetition clicking
					repMouseLimits.Add (xgStart, xgEnd);
					repMouseLimits.AddSamples (rep.sampleStart, rep.sampleEnd);

					iAll ++;
					iAccepted ++;
				}
				g.SetSourceColor (black);
			}

			// paint the f max avg in x seconds
			g.LineWidth = 2;
			if ( points_l != null && fMaxAvgSampleEnd >= 0 && points_l.Count > fMaxAvgSampleEnd)
			{
				double yPx = calculatePaintY (fMaxAvgForce);

				CairoUtil.PaintSegment (g, colorGreen,
						calculatePaintX (points_l[fMaxAvgSampleStart].X), yPx,
						calculatePaintX (points_l[fMaxAvgSampleEnd].X), yPx);
				CairoUtil.PaintSegment (g, colorGreen,
						calculatePaintX (points_l[fMaxAvgSampleStart].X), yPx-10,
						calculatePaintX (points_l[fMaxAvgSampleStart].X), yPx+10);
				CairoUtil.PaintSegment (g, colorGreen,
						calculatePaintX (points_l[fMaxAvgSampleEnd].X), yPx-10,
						calculatePaintX (points_l[fMaxAvgSampleEnd].X), yPx+10);
			}

			// paint max, min circles
			if(calculatePaintX (xAtMaxY) > leftMargin)
				drawCircle (calculatePaintX (xAtMaxY), calculatePaintY (yAtMaxY), 8, red, false);

			if(calculatePaintX (xAtMinY) > leftMargin)
				drawCircle (calculatePaintX (xAtMinY), calculatePaintY (yAtMinY), 8, red, false);

			points_l_painted = points_l.Count;
		}

		// paint triggers
		if (points_l != null && points_l.Count > 3 && graphInited && triggerList != null && triggerList.Count() > 0)
			paintTriggers (points_l, triggerList);

		return true;
	}

	private void writeRepetitionCode (int number, string type, int sepCount,
			double xposRepStart, double xposRepEnd, bool endsAtLeft, bool endsAtRight)
	{
		//just be safe
		if (exercise == null)
			return;

		string text = "";
		if (exercise.RepetitionsShow == ForceSensorExercise.RepetitionsShowTypes.CONCENTRIC ||
				exercise.RepetitionsShow == ForceSensorExercise.RepetitionsShowTypes.BOTHTOGETHER)
			text = (number +1).ToString();
		else
			text = string.Format ("{0}{1}", sepCount, type);

		Cairo.TextExtents te;
		te = g.TextExtents (text);

		int xposNumber = Convert.ToInt32 ((xposRepStart + xposRepEnd)/2);

		printText (xposNumber, 6, 0, Convert.ToInt32 (te.Height), text, g, alignTypes.CENTER);
	}

}
