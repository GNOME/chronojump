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
 *  Copyright (C) 2023-2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List
using Gtk;
using Cairo;

public abstract class CairoGraphForceSensor : CairoXY
{
	protected bool horizontal;
	protected int points_l_painted;

	protected int pathLineWidthInN;

	protected int minDisplayFNegative;
	protected int minDisplayFPositive;
	protected int rectangleN;
	protected int rectangleRange;
	protected List<PointF> points_l_interpolated_path;
	protected List<PointF> points_l_interpolated_path_further;
	protected int interpolatedMin;
	protected int interpolatedMax;
	//protected bool oneSerie; //on elastic is false: more than 1 serie


	protected void initForceSensor (DrawingArea area, string title, bool horizontal)
	{
		this.area = area;
		this.title = title;
		this.horizontal = horizontal;
		this.colorBackground = colorFromRGBA(Config.ColorBackground); //but note if we are using system colors, this will not match

		points_l_painted = 0;

		//need to be small because graphHeight could be 100,
		//if margins are big then calculatePaintY could give us reverse results
		leftMargin = 40;
		//rightMargin = 40; //defined in subclasses
		topMargin = 40;
		bottomMargin = 40;

		innerMargin = 20;

		if (horizontal)
		{
			yVariable = forceStr;
			yUnits = "N";
		} else {
			xVariable = forceStr;
			xUnits = "N";
		}

		xAtMaxY = 0;
		yAtMaxY = 0;
		xAtMinY = 0;
		yAtMinY = 0;

		gridNiceSeps = 7;
	}

	protected void fixMaximums ()
	{
		if (horizontal)
		{
			if (minY > minDisplayFNegative)
				minY = minDisplayFNegative;
			if (absoluteMaxY < minDisplayFPositive)
				absoluteMaxY = minDisplayFPositive;
		} else {
			if (minX > minDisplayFNegative)
				minX = minDisplayFNegative;
			if (absoluteMaxX < minDisplayFPositive)
				absoluteMaxX = minDisplayFPositive;
		}

		if (rectangleRange > 0) //TODO: fix this for vertical
		{
			if (rectangleN < 0 && rectangleN - rectangleRange/2 < minY)
				minY = rectangleN - rectangleRange/2;
			if (rectangleN > 0 && rectangleN + rectangleRange/2 > absoluteMaxY)
				absoluteMaxY = rectangleN + rectangleRange/2;
		}

		//TODO: fix this for vertical
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

		if (asteroids != null)
		{
			if (horizontal)
			{
				if (asteroids.MinY < minY)
					minY = asteroids.MinY;
				if (asteroids.MaxY > absoluteMaxY)
					absoluteMaxY = asteroids.MaxY;
			} else {
				if (asteroids.MinY < minX)
					minX = asteroids.MinY;
				if (asteroids.MaxY > absoluteMaxX)
					absoluteMaxX = asteroids.MaxY;
			}
		}
		//questionnaire is only done at CairoGraphForceSensorSignal
	}

	//TODO: fix this for vertical
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

	protected override void paintHorizontalGridLine (Cairo.Context g, int ytemp, string text, int fontH, bool atRight, int shiftRight, bool showText)
	{
		if (atRight) //atRight do not write the line
		{
			//g.MoveTo(leftMargin, ytemp);
			//g.LineTo(graphWidth - rightMargin, ytemp);
			//g.SetDash(new double[] {10,5}, 0);

			if (showText)
				printText (graphWidth -rightMargin + shiftRight, ytemp, 0, fontH, text, g, alignTypes.LEFT);

			return;
		}

		g.MoveTo(leftMargin, ytemp);
		g.LineTo(graphWidth - rightMargin, ytemp);

		if (! showText)
			return;
		if (! horizontal && Util.IsNumber (text, false))
		{
			double micros = Convert.ToDouble (text);
			text = string.Format ("{0}s", UtilAll.DivideSafe (micros, 1000000));
		}
		printText (leftMargin/2, ytemp, 0, fontH, text, g, alignTypes.CENTER);
	}
	protected override void paintVerticalGridLine(Cairo.Context g, int xtemp, string text, int fontH, bool showText)
	{
		if(fontH < 1)
			fontH = 1;

		g.MoveTo(xtemp, topMargin);
		g.LineTo(xtemp, graphHeight - bottomMargin);

		if (! showText)
			return;
		if (horizontal && Util.IsNumber (text, false))
		{
			double micros = Convert.ToDouble (text);
			text = string.Format ("{0}s", UtilAll.DivideSafe (micros, 1000000));
		}
		printText(xtemp, graphHeight -bottomMargin/2, 0, fontH, text, g, alignTypes.CENTER);
	}

	//TODO: fix this for vertical
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

	//TODO: fix this for vertical
	protected void paintTriggers (List<PointF> points_l, TriggerList triggerList)
	{
		g.LineWidth = 1;
		int bucleStartPoints = points_l.Count -2; //to start searching points for each trigger since last one

		foreach (Trigger trigger in triggerList.GetListReversed())
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
	
	//TODO: fix this for vertical
	//this is painted after the 1st serie because this changes the mins and max to be used on calculatePaintY
	protected void paintAnotherSerie (List<PointF> p_l, int startAt, PlotTypes plotType, Cairo.Color color, int axisShiftToRight, bool axisLabelTop, string variable, string units)
	{
		g.SetSourceColor (color);

		findPointMaximums (false, p_l);
		fixMaximums ();
		paintGrid (gridTypes.HORIZONTALLINESATRIGHT, true, axisShiftToRight + 5);
		paintAxisRight (axisShiftToRight, axisLabelTop, variable, units);
		plotRealPoints (plotType, p_l, startAt, false); //fast (but the difference is very low)
	}

	//TODO: fix this for vertical
	protected void bsiwPlot (List<PointF> points_l, GetBestStabilityInWindow bsiw)
	{
		//to not show "S" on capture tab. TODO: check better GetBestStabilityInWindow.dataTooShort()
		if (bsiw.MaxSampleStart == 0 && bsiw.MaxSampleEnd == 0)
			return;

		double x1 = calculatePaintX (points_l[bsiw.MaxSampleStart].X);
		double y1 = calculatePaintY (points_l[bsiw.MaxSampleStart].Y);
		double x2 = calculatePaintX (points_l[bsiw.MaxSampleEnd].X);
		double y2 = calculatePaintY (points_l[bsiw.MaxSampleEnd].Y);
		g.LineWidth = 2;

		//do the segment horizontal at bottom
		double bsiwY = y1;
		if (y2 > y1)
			bsiwY = y2;
		bsiwY += 10; //10 px below lowest y
		double bsiwX = UtilAll.DivideSafe (x1 + x2, 2);

		printText (bsiwX, bsiwY, 0, textHeight, "S", g, alignTypes.CENTER);
		CairoUtil.PaintSegment (g, x1, y1+2, x1, bsiwY);
		if (bsiwX -5 > x1)
			CairoUtil.PaintSegment (g, x1, bsiwY, bsiwX -5, bsiwY);
		CairoUtil.PaintSegment (g, x2, y2+2, x2, bsiwY);
		if (bsiwX +5 < x2)
			CairoUtil.PaintSegment (g, x2, bsiwY, bsiwX +5, bsiwY);
	}

	protected override void writeTitle()
	{
	}
}

public class CairoGraphForceSensorSignal : CairoGraphForceSensor
{
	private List<PointF> raw_l;
	private List<PointF> unfiltered_l;
	protected List<PointF> points_l; //if butterworth, this will be it

	protected int startAt;
	protected int marginAfterInSeconds;
	protected bool capturing;

	//questionnaire
	protected Questionnaire questionnaire;
	protected int questionnaireMinY;
	protected int questionnaireMaxY;

	private bool showAccuracy;
	private int accuracySamplesGood;
	private int accuracySamplesBad;
	private Cairo.Color colorPathBlue = colorFromRGB (178,223,238);
	//private Cairo.Color colorPathBlueLight = colorFromRGB (207,239,250);
	private Cairo.Color colorPathBlueLight = colorFromRGB (192,236,246);
	private GetMaxAvgInWindow miw;
	private GetBestRFDInWindow briw;
	private GetBestStabilityInWindow bsiw;

	private bool accuracyNowIn;
	private Cairo.Color colorHead;

	//constructor to inherit
	public CairoGraphForceSensorSignal ()
	{
	}

	//regular constructor
	public CairoGraphForceSensorSignal (DrawingArea area, string title, int pathLineWidthInN, bool horizontal)
	{
		initForceSensor (area, title, horizontal);

		this.pathLineWidthInN = pathLineWidthInN;

		//doing = false;
		accuracySamplesGood = 0;
		accuracySamplesBad = 0;
	}

	//separated in two methods to ensure endGraphDisposing on any return of the other method
	public void DoSendingList (string font,
			SignalPointsCairoForceElastic spCairoFE_raw,
			SignalPointsCairoForceElastic spCairoFE_unfiltered,	//only used if butterworth
			SignalPointsCairoForceElastic spCairoFE,	//spCairoFE to plot
			bool showDistance, bool showSpeed, bool showPower,
			List<PointF> points_l_interpolated_path, int interpolatedMin, int interpolatedMax,
			List<PointF> points_l_interpolated_path_further,
			bool capturing, bool videoShow, double videoPlayTimeInSeconds,
			bool showAccuracy, int showLastSeconds,
			int minDisplayFNegative, int minDisplayFPositive,
			int rectangleN, int rectangleRange,
			GetMaxAvgInWindow miw, GetBestRFDInWindow briw, GetBestStabilityInWindow bsiw,
			TriggerList triggerList,
			bool forceRedraw, PlotTypes plotType)
	{
		if (spCairoFE_unfiltered != null)
			this.unfiltered_l = spCairoFE_unfiltered.Force_l;
		else
			this.unfiltered_l = new List <PointF> ();

		if (spCairoFE_raw != null)
			this.raw_l = spCairoFE_raw.Force_l;
		else
			this.raw_l = new List <PointF> ();

		this.points_l = spCairoFE.Force_l;

		this.capturing = capturing;
		this.showAccuracy = showAccuracy;
		this.minDisplayFNegative = minDisplayFNegative;
		this.minDisplayFPositive = minDisplayFPositive;
		this.rectangleN = rectangleN;
		this.rectangleRange = rectangleRange;
		this.points_l_interpolated_path = points_l_interpolated_path;
		this.points_l_interpolated_path_further = points_l_interpolated_path_further;
		this.interpolatedMin = interpolatedMin;
		this.interpolatedMax = interpolatedMax;
		this.miw = miw;
		this.briw = briw;
		this.bsiw = bsiw;

		/*
		this.oneSerie = ( (pointsDispl_l == null || pointsDispl_l.Count == 0) &&
				(pointsSpeed_l == null || pointsSpeed_l.Count == 0) &&
				(pointsPower_l == null || pointsPower_l.Count == 0) );
				*/

		rightMargin = 40;
		if (spCairoFE.Displ_l != null && spCairoFE.Displ_l.Count > 0)
			rightMargin = Util.BoolToInt (showDistance) * 50 +
				Util.BoolToInt (showSpeed) * 50 +
				Util.BoolToInt (showPower) * 50;

		if (doSendingList (font, videoShow, videoPlayTimeInSeconds, showLastSeconds, triggerList, forceRedraw, plotType))
		{
			int atX = 0;
			bool atTop = true;
			if (showDistance && spCairoFE.Displ_l != null && spCairoFE.Displ_l.Count > 0)
			{
				paintAnotherSerie (spCairoFE.Displ_l, startAt, plotType, bluePlots, (atX ++)*50,
						atTop, distanceStr, "m");
				atTop = ! atTop;
			}

			if (showSpeed && spCairoFE.Speed_l != null && spCairoFE.Speed_l.Count > 0)
			{
				paintAnotherSerie (spCairoFE.Speed_l, startAt, plotType, green, (atX ++)*50,
						atTop, speedStr, "m/s");
				atTop = ! atTop;
			}

			if (showPower && spCairoFE.Power_l != null && spCairoFE.Power_l.Count > 0)
			{
				paintAnotherSerie (spCairoFE.Power_l, startAt, plotType, red, (atX ++)*50,
						atTop, powerStr, "W");
				atTop = ! atTop;
			}

			endGraphDisposing(g, surface, area.Window);
		}
	}

	//similar to encoder method but calling configureTimeWindow and using minDisplayF(Negative/Positive)
	//return true if graph is inited (to dispose it)
	private bool doSendingList (string font, bool videoShow, double videoPlayTimeInSeconds, int showLastSeconds,
			TriggerList triggerList, bool forceRedraw, PlotTypes plotType)
	{
		bool maxValuesChanged = false;

		if(points_l != null)
		{
			maxValuesChanged = findPointMaximums(false, points_l);
			//LogB.Information(string.Format("minY: {0}, maxY: {1}", minY, maxY));

			fixMaximums ();

			//also for unfiltered
			if (unfiltered_l.Count > 0)
			{
				double unfilteredMinY, unfilteredMaxY;
				PointF.GetMaxMinY (unfiltered_l, out unfilteredMinY, out unfilteredMaxY);
				if (unfilteredMinY < minY)
					minY = unfilteredMinY;
				if (unfilteredMaxY > absoluteMaxY)
					absoluteMaxY = unfilteredMaxY;
			}

			// if vertical do have X in the center (at least at start)
			if (! horizontal)
			{
				if (minX > -25)
					minX = -25;
				if (maxX < 25)
					maxX = 25;
				if (absoluteMaxX < 25)
					absoluteMaxX = 25;
			}

			if (questionnaire != null)
			{
				if (questionnaireMinY < minY)
					minY = questionnaireMinY;
				if (questionnaireMaxY > absoluteMaxY)
					absoluteMaxY = questionnaireMaxY;
			} else if (asteroids != null)
			{
				if (horizontal)
				{
					if (asteroids.MinY < minY)
						minY = asteroids.MinY;
					if (asteroids.MaxY > absoluteMaxY)
						absoluteMaxY = asteroids.MaxY;
				} else {
					if (asteroids.MinY < minY)
						minY = asteroids.MinY;
					if (asteroids.MaxY > absoluteMaxY)
						absoluteMaxY = asteroids.MaxY;
				}
			}
		}

		bool graphInited = false;
		if( maxValuesChanged || forceRedraw ||
				(points_l != null && points_l.Count != points_l_painted)
				)
		{
			if (asteroids != null && asteroids.Dark)
				colorCairoBackground = new Cairo.Color (.005, .005, .05, 1);
			else
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

		if (calculatePaintY (10) > calculatePaintY (0))
		{
			printText (graphWidth/2, graphHeight/2, 0, textHeight,
					Constants.GraphNeedMoreHeight (), g, alignTypes.CENTER);
			return true;
		}

		pointsRadius = 1;

		startAt = 0;
		marginAfterInSeconds = 0;

		//on worm, have it on 3 s
		if (showAccuracy && points_l_interpolated_path != null && points_l_interpolated_path.Count > 0 && showLastSeconds >= 10)
			marginAfterInSeconds = 3;
		if ( (asteroids != null || questionnaire != null) && showLastSeconds > 3) //this works also for asteroids
			marginAfterInSeconds = Convert.ToInt32 (.80 * showLastSeconds); //show blue ball left 20% of image (to have time/space to answer)

		if (showLastSeconds > 0 && points_l.Count > 1)
		{
			if (horizontal)
				startAt = configureTimeWindowHorizontal (points_l, showLastSeconds, marginAfterInSeconds, 1000000);
			else
				startAt = configureTimeWindowVertical (points_l, showLastSeconds, marginAfterInSeconds, 1000000);
		}

		// paint points and maybe interpolated path
		if(maxValuesChanged || forceRedraw || points_l.Count != points_l_painted)
			doPlot (plotType);

		// paint triggers
		if (points_l != null && points_l.Count > 3 && graphInited && triggerList != null && triggerList.Count() > 0)
			paintTriggers (points_l, triggerList);


		if (videoShow)
		{
			//videoPlayTimeInSeconds
			//printText (graphWidth - rightMargin/2, topMargin,
			//		0, textHeight +4, Util.TrimDecimals (videoPlayTimeInSeconds, 2), g, alignTypes.CENTER);
			g.MoveTo (calculatePaintX (videoPlayTimeInSeconds * 1000000), topMargin);
			g.LineTo (calculatePaintX (videoPlayTimeInSeconds * 1000000), graphHeight - bottomMargin);
			g.Stroke ();
		}

		return true;
	}

	private void doPlot (PlotTypes plotType)
	{
		if (rectangleRange > 0)
			paintRectangle (rectangleN, rectangleRange);

		if (asteroids != null && asteroids.Dark)
			g.SetSourceColor (white);

		if (points_l.Count > 2) //to ensure minX != maxX
		{
			//on asteroids show the grid in gray to allow stars be shown
			if (asteroids != null)
				g.SetSourceColor (gray);

			paintGrid (gridTypes.BOTH, true, 0);

			if (asteroids != null)
			{
				g.SetSourceColor (white);
				if (asteroids.Dark)
					g.SetSourceColor (white);
				else
					g.SetSourceColor (black);
			}
		}

		paintAxis();
		g.SetSourceColor (black);

		accuracyNowIn = true;
		colorHead = colorPathBlue;

		//calculate the accuracy on rectangle and path
		string accuracyText = "";
		if (showAccuracy &&
				(rectangleRange > 0 ||
				 (points_l_interpolated_path != null && points_l_interpolated_path.Count > 0)))
			accuracyText = accuracyCalcule (points_l);

		plotSpecific (); //right now only asteroids and questionnaire

		if (rectangleRange > 0 && showAccuracy)
			accuracyRectanglePlot (accuracyText);
		else if (points_l_interpolated_path != null && points_l_interpolated_path.Count > 0)
		{
			if (points_l_interpolated_path_further.Count > 0) //only is filled while capture
			{
				g.SetSourceColor (colorPathBlueLight);
				g.LineWidth = calculatePathWidth ();
				for (int i = 0; i < points_l_interpolated_path_further.Count; i ++)
				{
					if (i == 0)
						g.MoveTo (calculatePaintX (points_l_interpolated_path_further[i].X),
								calculatePaintY (points_l_interpolated_path_further[i].Y));

					if (i + 1 < points_l_interpolated_path_further.Count)
						g.LineTo (calculatePaintX (points_l_interpolated_path_further[i+1].X),
								calculatePaintY (points_l_interpolated_path_further[i+1].Y));
				}
				g.Stroke();
			}

			accuracyPathPlot (accuracyText,
					points_l.Count, points_l_interpolated_path, plotType);
		}

		if (questionnaire == null && asteroids == null)
		{
			if (points_l_interpolated_path == null || points_l_interpolated_path.Count == 0)
			{
				//raw
				g.SetSourceColor (caramel);
				if (raw_l.Count > 0)
					plotRealPoints(plotType, raw_l, startAt, false); //fast (but the difference is very low)

				//unfiltered
				g.SetSourceColor (brown);
				if (unfiltered_l.Count > 0)
					plotRealPoints(plotType, unfiltered_l, startAt, false); //fast (but the difference is very low)
			}

			//points_l
			if (! (points_l_interpolated_path != null && capturing)) //while worm capture do not show this stats
			{
				if (miw.Error == "" && miw.Max > 0) // > 0 to not show miw at 0ms on negative signal
					paintMaxAvgInWindow (miw.MaxSampleStart, miw.MaxSampleEnd, miw.Max, points_l);

				if (briw.Error == "")
					briwPlot (points_l);

				if (bsiw.Error == "")
					bsiwPlot (points_l, bsiw);

				if(calculatePaintX (xAtMaxY) > leftMargin)
					drawCircle (g, calculatePaintX (xAtMaxY), calculatePaintY (yAtMaxY), 8, red, false);

				if(calculatePaintX (xAtMinY) > leftMargin)
					drawCircle (g, calculatePaintX (xAtMinY), calculatePaintY (yAtMinY), 8, red, false);
			}

			g.LineWidth = 2;
			g.SetSourceColor (black);
			plotRealPoints(plotType, points_l, startAt, false); //fast (but the difference is very low)
		}

		points_l_painted = points_l.Count;
	}

	//TODO: fix this for vertical
	private string accuracyCalcule (List<PointF> points_l)
	{
		string str;
		if (points_l[points_l.Count -1].X < 5000000)
		{
			str = string.Format ("Accuracy calculation starts in {0} s",
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
				double error = CairoUtil.GetDistance2D (calculatePaintX (points_l[points_l.Count -1].X),
						calculatePaintY (points_l[points_l.Count -1].Y),
						calculatePaintX (points_l_interpolated_path[points_l_interpolated_path.Count -1].X),
						calculatePaintY (points_l_interpolated_path[points_l_interpolated_path.Count -1].Y));

				if (error > calculatePathWidth ()/2)
					accuracyNowIn = false;
			}

			str = string.Format ("Accuracy {0} %", Util.TrimDecimals (100 * UtilAll.DivideSafe (accuracySamplesGood, (accuracySamplesGood + accuracySamplesBad)), 1));

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

		return str;
	}

	protected virtual void plotSpecific ()
	{
		//do nothing
	}

	//TODO: fix this for vertical
	private void briwPlot (List<PointF> points_l)
	{
		g.LineWidth = 2;
		drawCircle (g, calculatePaintX (points_l[briw.MaxSampleStart].X), calculatePaintY (points_l[briw.MaxSampleStart].Y), 8, black, false);
		drawCircle (g, calculatePaintX (points_l[briw.MaxSampleEnd].X), calculatePaintY (points_l[briw.MaxSampleEnd].Y), 8, black, false);

		List<PointF> briwP_l = new List<PointF> ();
		briwP_l.Add (new PointF (points_l[briw.MaxSampleStart].X, points_l[briw.MaxSampleStart].Y));
		briwP_l.Add (new PointF (points_l[briw.MaxSampleEnd].X, points_l[briw.MaxSampleEnd].Y));
		preparePredictedLine (briwP_l);
	}

	private void accuracyRectanglePlot (string accuracyText)
	{
		g.SetSourceColor (black);

		g.SetFontSize (textHeight +4);
		printText (graphWidth/2, leftMargin, 0, textHeight +4,
				accuracyText, g, alignTypes.CENTER);
		g.SetFontSize (textHeight);

		g.LineWidth = 2;
	}

	//TODO: fix this for vertical
	private void accuracyPathPlot (string accuracyText,
			int points_lCount,  List<PointF> points_l_interpolated_path, PlotTypes plotType)
	{
		g.LineWidth = calculatePathWidth ();
		g.SetSourceColor (colorPathBlue);

		//make the line start at startAt +1 (if possible) and draw cicle at left (startAt) to have nice rounding at left.
		int startAt_theLine = startAt;
		if (points_lCount -1 > startAt)
			startAt_theLine = startAt +1;

		plotRealPoints(plotType, points_l_interpolated_path, startAt_theLine, false); //fast (but the difference is very low)

		//circle at left
		drawCircle (g, calculatePaintX (points_l_interpolated_path[startAt].X),
				calculatePaintY (points_l_interpolated_path[startAt].Y),
				g.LineWidth/2, colorPathBlue, true);

		//circle at right
		drawCircle (g, calculatePaintX (points_l_interpolated_path[points_l_interpolated_path.Count -1].X),
				calculatePaintY (points_l_interpolated_path[points_l_interpolated_path.Count -1].Y),
				g.LineWidth/2, colorHead, true);

		g.SetSourceColor (black);

		if (showAccuracy)
		{
			g.SetFontSize (textHeight +4);
			printText (graphWidth/2, leftMargin, 0, textHeight +4,
					accuracyText, g, alignTypes.CENTER);
			g.SetFontSize (textHeight);
		}
	}

	protected void crashedPaintOutRectangle ()
	{
		g.SetSourceColor (red);
		g.LineWidth = 20;
		g.Rectangle (0, 0, graphWidth, graphHeight);
		g.Stroke();
		g.LineWidth = 1;
	}

	public Asteroids PassAsteroids {
		set { asteroids = value; }
	}

	public Questionnaire PassQuestionnaire {
		set { questionnaire = value; }
	}
	public int QuestionnaireMinY {
		set { questionnaireMinY = value; }
	}
	public int QuestionnaireMaxY {
		set { questionnaireMaxY = value; }
	}
}

public class CairoGraphForceSensorAI : CairoGraphForceSensor
{
	//private Cairo.Color colorGreen = colorFromRGB (0,200,0);
	//private Cairo.Color colorBlue = colorFromRGB (0,0,200);
	private ForceSensorExercise exercise;
	private RepetitionMouseLimitsWithSamples repMouseLimits;
	private int startAt;

	//regular constructor
	public CairoGraphForceSensorAI (DrawingArea area, string title)
	{
		initForceSensor (area, title, true);
	}

	//separated in two methods to ensure endGraphDisposing on any return of the other method
	public RepetitionMouseLimitsWithSamples DoSendingList (
			string font,
			SignalPointsCairoForceElastic spCairoFE,
			SignalPointsCairoForceElastic spCairoFE_CD,
			List<string> subtitleWithSetsInfo_l, bool radio_cd_active,
			bool showDistance, bool showSpeed, bool showPower,
			int minDisplayFNegative, int minDisplayFPositive,
			int rectangleN, int rectangleRange,
			List<GetBestRFDInWindow> briw_l, List<GetBestStabilityInWindow> bsiw_l,
			TriggerList triggerList,
			int hscaleSampleA, int hscaleSampleB,
			int hscaleSampleC, int hscaleSampleD,
			bool zoomed,
			List<GetMaxAvgInWindow> gmaiw_l,
			ForceSensorExercise exercise, List<ForceSensorRepetition> reps_l,
			bool forceRedraw, PlotTypes plotType)
	{
		this.minDisplayFNegative = minDisplayFNegative;
		this.minDisplayFPositive = minDisplayFPositive;
		this.rectangleN = rectangleN;
		this.rectangleRange = rectangleRange;
		this.points_l_interpolated_path = new List<PointF> ();

		this.exercise = exercise;
		/*
		this.oneSerie = ( (pointsDispl_l == null || pointsDispl_l.Count == 0) &&
				(pointsSpeed_l == null || pointsSpeed_l.Count == 0) &&
				(pointsPower_l == null || pointsPower_l.Count == 0) );
				*/

		repMouseLimits = new RepetitionMouseLimitsWithSamples ();
		area.AddEvents((int) Gdk.EventMask.ButtonPressMask); //to have mouse clicks

		rightMargin = 40;
		if (spCairoFE.Displ_l != null && spCairoFE.Displ_l.Count > 0)
			rightMargin = Util.BoolToInt (showDistance) * 50 +
				Util.BoolToInt (showSpeed) * 50 +
				Util.BoolToInt (showPower) * 50;

		if (doSendingList (font, spCairoFE.Force_l,
					spCairoFE_CD,
					subtitleWithSetsInfo_l, radio_cd_active,
					triggerList,
					hscaleSampleA, hscaleSampleB,
					hscaleSampleC, hscaleSampleD,
					zoomed,
					briw_l, bsiw_l,
					gmaiw_l,
					exercise, reps_l,
					forceRedraw, plotType))
		{
			int atX = 0;
			bool atTop = true;
			if (showDistance && spCairoFE.Displ_l != null && spCairoFE.Displ_l.Count > 0)
			{
				paintAnotherSerie (spCairoFE.Displ_l, startAt, plotType, bluePlots, (atX ++)*50,
						atTop, distanceStr, "m");
				atTop = ! atTop;
			}

			if (showSpeed && spCairoFE.Speed_l != null && spCairoFE.Speed_l.Count > 0)
			{
				paintAnotherSerie (spCairoFE.Speed_l, startAt, plotType, green, (atX ++)*50,
						atTop, speedStr, "m/s");
				atTop = ! atTop;
			}

			if (showPower && spCairoFE.Power_l != null && spCairoFE.Power_l.Count > 0)
			{
				paintAnotherSerie (spCairoFE.Power_l, startAt, plotType, red, (atX ++)*50,
						atTop, powerStr, "W");
				atTop = ! atTop;
			}

			endGraphDisposing(g, surface, area.Window);
		}

		return repMouseLimits;
	}

	//similar to encoder method but calling configureTimeWindow and using minDisplayF(Negative/Positive)
	//return true if graph is inited (to dispose it)
	private bool doSendingList (string font,
			List<PointF> points_l,
			SignalPointsCairoForceElastic spCairoFE_CD,
			List<string> subtitleWithSetsInfo_l, bool radio_cd_active,
			TriggerList triggerList,
			int hscaleSampleA, int hscaleSampleB,
			int hscaleSampleC, int hscaleSampleD,
			bool zoomed,
			List<GetBestRFDInWindow> briw_l, List<GetBestStabilityInWindow> bsiw_l,
			List<GetMaxAvgInWindow> gmaiw_l,
			ForceSensorExercise exercise, List<ForceSensorRepetition> reps_l,
			bool forceRedraw, PlotTypes plotType)
	{
		//debug
		if (spCairoFE_CD != null)
			LogB.Information (string.Format ("\nAt CairoGraphForceSensorAI doSendingList" +
						"points_l.Count: {0}, spCairoFE_CD.Force_l.Count: {1} " +
						"hscaleSampleA : {2}, hscaleSampleB : {3}, " +
						"hscaleSampleC : {4}, hscaleSampleD : {5}, " +
						"exercise : {6}, reps_l.Count : {7}",
						points_l.Count, spCairoFE_CD.Force_l.Count,
						hscaleSampleA, hscaleSampleB,
						hscaleSampleC, hscaleSampleD,
						exercise, reps_l.Count));

		bool dataExists = false;
		if (points_l != null)
			dataExists = true;
		else if (points_l == null && (spCairoFE_CD != null && spCairoFE_CD.Force_l.Count > 0))
		{
			// on superpose, can have data of the CD but not of the AB (because set ended)
			// just create an empty points_l
			points_l = new List<PointF> ();
			dataExists = true;
		}

		bool twoSets = false;
		List <PointF> pointsCD_l = points_l;
		if (spCairoFE_CD != null && spCairoFE_CD.Force_l.Count > 0)
		{
			twoSets = true;
			pointsCD_l = spCairoFE_CD.Force_l;
		}

		bool maxValuesChanged = false;
		if (dataExists)
		{
			maxValuesChanged = findPointMaximums(false, points_l);
			//LogB.Information(string.Format("minY: {0}, maxY: {1}", minY, maxY));

			fixMaximums ();

			if (twoSets)
			{
				// for superpose when CD starts after AB ends (so points_l.Count == 0)
				if (points_l.Count == 0)
					minX = pointsCD_l[0].X;

				foreach (PointF p in pointsCD_l)
				{
					if(p.X < minX)
						minX = p.X;
					if(p.X > absoluteMaxX)
						absoluteMaxX = p.X;
					if(p.Y < minY)
						minY = p.Y;
					if(p.Y > absoluteMaxY)
						absoluteMaxY = p.Y;
				}
			}
		}

		bool graphInited = false;
		if ( maxValuesChanged || forceRedraw ||
				(dataExists && points_l.Count != points_l_painted) )
		{
			initGraph (font, 1, (maxValuesChanged || forceRedraw) );
			graphInited = true;
			points_l_painted = 0;
		}

		if (! dataExists)
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
				paintGrid(gridTypes.BOTH, true, 0);

			paintAxis();

			if (points_l.Count > 0)
				plotRealPoints (plotType, points_l, startAt, false); //fast (but the difference is very low)

			if (twoSets)
			{
				if (points_l.Count > 0)
					printText (calculatePaintX (PointF.Last (points_l).X) + 5,
							calculatePaintY (PointF.Last (points_l).Y),
							0, textHeight, "AB", g, alignTypes.LEFT);

				g.SetSourceColor (grayDark);
				plotRealPoints (plotType, pointsCD_l, startAt, false); //fast (but the difference is very low)
				printText (calculatePaintX (PointF.Last (pointsCD_l).X) + 5,
						calculatePaintY (PointF.Last (pointsCD_l).Y),
						0, textHeight, "CD", g, alignTypes.LEFT);

				g.SetSourceColor (black);
			}


			// paint the AB rectangle
			// hscales start at 1.
			LogB.Information (string.Format ("doSendingList hscales: hscaleSampleA: {0}, hscaleSampleB: {1}, points_l.Count: {2}",
						hscaleSampleA, hscaleSampleB, points_l.Count));

			// hscales start at 0
			if (hscaleSampleA >= 0 && hscaleSampleB >= 0 &&
					points_l.Count > hscaleSampleA && points_l.Count > hscaleSampleB)
				CairoUtil.PaintVerticalLinesAndRectangle (g, graphHeight,
						"A", calculatePaintX (points_l[hscaleSampleA].X),
						"B", calculatePaintX (points_l[hscaleSampleB].X),
						true, 15, 0, yellow, yellowTransp);

			// paint the CD rectangle
			if (hscaleSampleC >= 0 && hscaleSampleD >= 0 &&
					pointsCD_l.Count > hscaleSampleC && pointsCD_l.Count > hscaleSampleD
					&& (hscaleSampleC != hscaleSampleA || hscaleSampleD != hscaleSampleB))
				CairoUtil.PaintVerticalLinesAndRectangle (g, graphHeight,
						"C", calculatePaintX (pointsCD_l[hscaleSampleC].X),
						"D", calculatePaintX (pointsCD_l[hscaleSampleD].X),
						true, 15, 0, green, greenTransp);

			// paint the repetition lines and codes
			if (reps_l.Count > 0)
			{
				List<PointF> pointsForReps_l = points_l;
				if (twoSets && radio_cd_active)
					pointsForReps_l = pointsCD_l;

				g.LineWidth = 1;

				Cairo.Color colorReps = grayDark;
				if (twoSets && radio_cd_active)
					colorReps = green;
				else if (twoSets && ! radio_cd_active)
					colorReps = yellow;

				g.SetSourceColor (colorReps);

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
					else if (zoomed && rep.sampleStart >= pointsForReps_l.Count)
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
					/*
					LogB.Information (string.Format ("pointsForReps_l.Count: {0}",
								pointsForReps_l.Count));
					LogB.Information (string.Format ("rep.sampleStart: {0}",
								rep.sampleStart));
								*/

					if (zoomed && rep.sampleStart < 0)
					{
						arrowL = true;
						xgStart = calculatePaintX (pointsForReps_l[0].X);
					} else
						xgStart = calculatePaintX (pointsForReps_l[rep.sampleStart].X);

					// 4) rep ends after B, paint an arrow to B (and write text in the center of the end of rep and B
					// hscales:     A      B
					// rep:            |  rep  |
					// show:           |--->
					LogB.Information (string.Format ("rep.sampleStart: {0}, rep.sampleEnd: {1}, pointsForReps_l.Count: {2}",
								rep.sampleStart, rep.sampleEnd, pointsForReps_l.Count));
					if (zoomed && rep.sampleEnd >= pointsForReps_l.Count)
					{
						arrowR = true;
						xgEnd = calculatePaintX (pointsForReps_l[pointsForReps_l.Count -1].X);
					} else {
						//fix potential crash
						if (rep.sampleEnd < 0 || rep.sampleEnd >= pointsForReps_l.Count)
							continue;

						xgEnd = calculatePaintX (pointsForReps_l[rep.sampleEnd].X);
					}

					//display arrows if needed
					if (arrowL && arrowR)
					{
						plotArrowFree (g, colorReps, 1, 8, false,
								(xgStart + xgEnd) /2, textHeight +6,
								xgStart, textHeight +6);
						plotArrowFree (g, colorReps, 1, 8, false,
								(xgStart + xgEnd) /2, textHeight +6,
								xgEnd, textHeight +6);
					} else if (arrowL)
						plotArrowFree (g, colorReps, 1, 8, false,
								xgEnd, textHeight +6,
								xgStart, textHeight +6);
					else if (arrowR)
						plotArrowFree (g, colorReps, 1, 8, false,
								xgStart, textHeight +6,
								xgEnd, textHeight +6);
					else
						CairoUtil.PaintSegment (g,
								xgStart, textHeight +6,
								xgEnd, textHeight +6);

					// display left vertical line if does not overlap a previous right vertical line
					if (! arrowL && (iAccepted == 0 || (iAccepted > 0 && pointsForReps_l[rep.sampleStart].X > pointsForReps_l[reps_l[iAll-1].sampleEnd].X)))
						CairoUtil.PaintSegment (g,
								xgStart, textHeight +6,
								xgStart, graphHeight -bottomMargin);

					// display right vertical line
					if (! arrowR)
						CairoUtil.PaintSegment (g,
								xgEnd, textHeight +6,
								xgEnd, graphHeight -bottomMargin);

					writeRepetitionCode (iAll, rep.TypeShort(), sepCount,
							xgStart, xgEnd, rep.sampleStart > 0, true);

					//store x,y to select the repetition clicking
					//here it only matters the x. For this reason both y's == -1
					repMouseLimits.Add (xgStart, -1, xgEnd, -1);
					repMouseLimits.AddSamples (rep.sampleStart, rep.sampleEnd);

					iAll ++;
					iAccepted ++;
				}
				g.SetSourceColor (black);
			}

			// paint the f max avg in x seconds
			paintGmaiw (points_l, gmaiw_l[0]);
			if (gmaiw_l.Count > 1)
				paintGmaiw (pointsCD_l, gmaiw_l[1]);

			// paint the f max avg in x seconds
			paintBriw (points_l, briw_l[0]);
			if (briw_l.Count > 1)
				paintBriw (pointsCD_l, briw_l[1]);

			if (bsiw_l[0].Error == "")
				bsiwPlot (points_l, bsiw_l[0]);
			if (bsiw_l.Count > 1 && bsiw_l[1].Error == "")
				bsiwPlot (pointsCD_l, bsiw_l[1]);

			g.LineWidth = 2;

			if (subtitleWithSetsInfo_l.Count > 0)
				paintSignalSubtitles (subtitleWithSetsInfo_l);

			// paint max, min circles
			if (points_l.Count > 0 && calculatePaintX (xAtMaxY) > leftMargin)
				drawCircle (g, calculatePaintX (xAtMaxY), calculatePaintY (yAtMaxY), 8, yellow, false);

			if (points_l.Count > 0 && calculatePaintX (xAtMinY) > leftMargin)
				drawCircle (g, calculatePaintX (xAtMinY), calculatePaintY (yAtMinY), 8, yellow, false);

			if (twoSets && pointsCD_l.Count > 0)
			{
				PointF maxCD = PointF.GetMaxYAndItsX (pointsCD_l);
				PointF minCD = PointF.GetMinYAndItsX (pointsCD_l);

				if (calculatePaintX (maxCD.X) > leftMargin)
					drawCircle (g, calculatePaintX (maxCD.X), calculatePaintY (maxCD.Y), 8, green, false);
				if (calculatePaintX (minCD.X) > leftMargin)
					drawCircle (g, calculatePaintX (minCD.X), calculatePaintY (minCD.Y), 8, green, false);
			}

			points_l_painted = points_l.Count;
		}

		// paint triggers
		if (points_l != null && points_l.Count > 3 && graphInited && triggerList != null && triggerList.Count() > 0)
			paintTriggers (points_l, triggerList);

		return true;
	}

	private void paintGmaiw (List<PointF> p_l, GetMaxAvgInWindow gmaiw)
	{
		if ( p_l != null && gmaiw.MaxSampleEnd >= 0 && p_l.Count > gmaiw.MaxSampleEnd)
			paintMaxAvgInWindow (gmaiw.MaxSampleStart, gmaiw.MaxSampleEnd, gmaiw.Max, p_l);
	}

	private void paintBriw (List<PointF> p_l, GetBestRFDInWindow briw)
	{
		if (briw.Error != "")
			return;

		g.LineWidth = 2;
		drawCircle (g, calculatePaintX (p_l[briw.MaxSampleStart].X), calculatePaintY (p_l[briw.MaxSampleStart].Y), 8, black, false);
		drawCircle (g, calculatePaintX (p_l[briw.MaxSampleEnd].X), calculatePaintY (p_l[briw.MaxSampleEnd].Y), 8, black, false);

		List<PointF> briwP_l = new List<PointF> ();
		briwP_l.Add (new PointF (p_l[briw.MaxSampleStart].X, p_l[briw.MaxSampleStart].Y));
		briwP_l.Add (new PointF (p_l[briw.MaxSampleEnd].X, p_l[briw.MaxSampleEnd].Y));
		preparePredictedLine (briwP_l);
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

