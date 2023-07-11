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
	//protected bool oneSerie; //on elastic is false: more than 1 serie


	protected void initForceSensor (DrawingArea area, string title)
	{
		this.area = area;
		this.title = title;
		this.colorBackground = colorFromRGBA(Config.ColorBackground); //but note if we are using system colors, this will not match

		points_l_painted = 0;

		//need to be small because graphHeight could be 100,
		//if margins are big then calculatePaintY could give us reverse results
		leftMargin = 40;
		//rightMargin = 40; //defined in subclasses
		topMargin = 40;
		bottomMargin = 40;

		innerMargin = 20;

		yVariable = forceStr;
		yUnits = "N";

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

		//questionnaire is only done at CairoGraphForceSensorSignal
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

	protected void paintMaxAvgInWindow (int start, int end, double force, List<PointF> points_l)
	{
		/* unused, maybe show in other way
		if (oneSerie)
			g.LineWidth = 2;
		else */
			g.LineWidth = 4;

		double yPx = calculatePaintY (force);

		CairoUtil.PaintSegment (g, black,
				calculatePaintX (points_l[start].X), yPx,
				calculatePaintX (points_l[end].X), yPx);
		CairoUtil.PaintSegment (g, black,
				calculatePaintX (points_l[start].X), yPx-10,
				calculatePaintX (points_l[start].X), yPx+10);
		CairoUtil.PaintSegment (g, black,
				calculatePaintX (points_l[end].X), yPx-10,
				calculatePaintX (points_l[end].X), yPx+10);
	}

	protected override void writeTitle()
	{
	}
}

public class CairoGraphForceSensorSignal : CairoGraphForceSensor
{
	protected List<PointF> points_l;
	protected int startAt;
	protected int marginRightInSeconds;
	protected bool capturing;

	//questionnaire
	protected Questionnaire questionnaire;
	protected int questionnaireMinY;
	protected int questionnaireMaxY;

	private bool showAccuracy;
	private int accuracySamplesGood;
	private int accuracySamplesBad;
	private Cairo.Color colorPathBlue = colorFromRGB (178,223,238);
	private GetMaxAvgInWindow miw;
	private GetBestRFDInWindow briw;

	private bool accuracyNowIn;
	private Cairo.Color colorHead;

	//constructor to inherit
	public CairoGraphForceSensorSignal ()
	{
	}

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
			SignalPointsCairoForceElastic spCairoFE,
			bool showDistance, bool showSpeed, bool showPower,
			List<PointF> points_l_interpolated_path, int interpolatedMin, int interpolatedMax,
			bool capturing, bool showAccuracy, int showLastSeconds,
			int minDisplayFNegative, int minDisplayFPositive,
			int rectangleN, int rectangleRange,
			GetMaxAvgInWindow miw,
			GetBestRFDInWindow briw,
			TriggerList triggerList,
			bool forceRedraw, PlotTypes plotType)
	{
		this.points_l = spCairoFE.Force_l;
		this.capturing = capturing;
		this.showAccuracy = showAccuracy;
		this.minDisplayFNegative = minDisplayFNegative;
		this.minDisplayFPositive = minDisplayFPositive;
		this.rectangleN = rectangleN;
		this.rectangleRange = rectangleRange;
		this.points_l_interpolated_path = points_l_interpolated_path;
		this.interpolatedMin = interpolatedMin;
		this.interpolatedMax = interpolatedMax;
		this.miw = miw;
		this.briw = briw;

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

		if (doSendingList (font, showLastSeconds, triggerList, forceRedraw, plotType))
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
	private bool doSendingList (string font, int showLastSeconds,
			TriggerList triggerList, bool forceRedraw, PlotTypes plotType)
	{
		bool maxValuesChanged = false;

		if(points_l != null)
		{
			maxValuesChanged = findPointMaximums(false, points_l);
			//LogB.Information(string.Format("minY: {0}, maxY: {1}", minY, maxY));

			fixMaximums ();

			if (questionnaire != null)
			{
				if (questionnaireMinY < minY)
					minY = questionnaireMinY;
				if (questionnaireMaxY > absoluteMaxY)
					absoluteMaxY = questionnaireMaxY;
			} else if (asteroids != null)
			{
				if (asteroids.MinY < minY)
					minY = asteroids.MinY;
				if (asteroids.MaxY > absoluteMaxY)
					absoluteMaxY = asteroids.MaxY;
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
		pointsRadius = 1;

		startAt = 0;
		marginRightInSeconds = 0;

		//on worm, have it on 3 s
		/*
		if (showAccuracy && points_l_interpolated_path != null && points_l_interpolated_path.Count > 0 && showLastSeconds >= 10)
			marginRightInSeconds = 3; //TODO: or a 1/3 of showLastSeconds TODO: on worm first we need to fix interpolatedPath to be 3s longer
			*/
		if ( (asteroids != null || questionnaire != null) && showLastSeconds > 3) //this works also for asteroids
			marginRightInSeconds = Convert.ToInt32 (.66 * showLastSeconds); //show in left third of image (to have time/space to answer)

		if (showLastSeconds > 0 && points_l.Count > 1)
			startAt = configureTimeWindow (points_l, showLastSeconds, marginRightInSeconds, 1000000);

		// paint points and maybe interpolated path
		if(maxValuesChanged || forceRedraw || points_l.Count != points_l_painted)
			doPlot (plotType);

		// paint triggers
		if (points_l != null && points_l.Count > 3 && graphInited && triggerList != null && triggerList.Count() > 0)
			paintTriggers (points_l, triggerList);

		return true;
	}

	private void doPlot (PlotTypes plotType)
	{
		if (rectangleRange > 0)
			paintRectangle (rectangleN, rectangleRange);

		if (asteroids != null && asteroids.Dark)
			g.SetSourceColor (white);

		if (points_l.Count > 2) //to ensure minX != maxX
			paintGrid (gridTypes.BOTH, true, 0);

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
			accuracyPathPlot (accuracyText,
					points_l.Count, points_l_interpolated_path, plotType);

		if (questionnaire == null && asteroids == null)
		{
			if (miw.Error == "")
				paintMaxAvgInWindow (miw.MaxSampleStart, miw.MaxSampleEnd, miw.Max, points_l);

			if (briw.Error == "")
				briwPlot (points_l);

			plotRealPoints(plotType, points_l, startAt, false); //fast (but the difference is very low)

			if(calculatePaintX (xAtMaxY) > leftMargin)
				drawCircle (calculatePaintX (xAtMaxY), calculatePaintY (yAtMaxY), 8, red, false);

			if(calculatePaintX (xAtMinY) > leftMargin)
				drawCircle (calculatePaintX (xAtMinY), calculatePaintY (yAtMinY), 8, red, false);
		}

		points_l_painted = points_l.Count;
	}

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

	private void briwPlot (List<PointF> points_l)
	{
		g.LineWidth = 2;
		drawCircle (calculatePaintX (points_l[briw.MaxSampleStart].X), calculatePaintY (points_l[briw.MaxSampleStart].Y), 8, black, false);
		drawCircle (calculatePaintX (points_l[briw.MaxSampleEnd].X), calculatePaintY (points_l[briw.MaxSampleEnd].Y), 8, black, false);

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

public class CairoGraphForceSensorSignalAsteroids : CairoGraphForceSensorSignal
{
	private double lastShot;
	private double lastPointUp; //each s 1 point up
	private int multiplier;

	public CairoGraphForceSensorSignalAsteroids (DrawingArea area, string title)
	{
		initForceSensor (area, title);
		multiplier = 1000000; //forceSensor

		lastShot = 0;
		lastPointUp = 0; //each s 1 point up
	}

	protected override void plotSpecific ()
	{
		if (! capturing)
			return;

		asteroidsPlot (points_l[points_l.Count -1], startAt, multiplier,
				//marginRightInSeconds, points_l, horizontal,
				marginRightInSeconds, points_l, true,
				ref lastShot, ref lastPointUp);
	}
}

public class CairoGraphForceSensorSignalQuestionnaire : CairoGraphForceSensorSignal
{
	public CairoGraphForceSensorSignalQuestionnaire (DrawingArea area, string title)
	{
		initForceSensor (area, title);
	}

	protected override void plotSpecific ()
	{
		questionnairePlot (points_l[points_l.Count -1]);

		drawCircle (calculatePaintX (points_l[points_l.Count -1].X),
				calculatePaintY (points_l[points_l.Count -1].Y), 6, bluePlots, true);
	}

	private void questionnairePlot (PointF lastPoint)
	{
		int textHeightHere = textHeight + 8;
		g.SetFontSize (textHeightHere);
		g.SetSourceRGB(0, 0, 0); //black

		// 1) manage finish
		// 10 questions, do not plot more than 100 seconds
		// or (if less than 10 questions, do not plot more than those)
		if (lastPoint.X / 1000000 >= 100 || lastPoint.X / 1000000 >= questionnaire.N * 10)
		{
			printText ((graphWidth -getMargins (Directions.LR))/2 +getMargins (Directions.L),
					.33 * graphHeight, 0, textHeight +4,
					string.Format ("Finished! {0} points", questionnaire.Points), g, alignTypes.CENTER);
			g.SetFontSize (textHeight);

			return;
		}

		// 2) get bars position
		QRectangleManage rectangleM = new QRectangleManage ();

		double barRange = (graphHeight - topMargin - bottomMargin) /30;
		List <double> y_l = new List<double> ();
		for (int i = 0; i < 6; i ++)
			y_l.Add (topMargin + (i * ((graphHeight -topMargin - bottomMargin)/5)));

		QuestionAnswers qa = questionnaire.GetQAByMicros (lastPoint.X);

		// 3) write the question (ensure it horizontally fits)
		string questionText = string.Format ("({0}/{1}) {2}",
				questionnaire.GetQNumByMicros (lastPoint.X), questionnaire.N, qa.question);

		int textHeightReduced = textHeightHere;
		if (! textFits (questionText, g, graphWidth -leftMargin -rightMargin -70)) //75 for the messages "Force (N)" and "Points: xx" message
			textHeightReduced = findFontThatFits (textHeightHere, questionText, g, graphWidth -leftMargin -rightMargin -75);
		if (textHeightReduced >= 2)
		{
			g.SetFontSize (textHeightReduced);
			printText (graphWidth/2 -leftMargin, topMargin/2, 0, textHeightReduced,
					questionText, g, alignTypes.CENTER);
		}
		g.SetFontSize (textHeightHere);

		// 4) write the answers (ensure they horizontally fit)
		List<string> answers_l = qa.TopBottom_l;
		double answerX = questionnaire.GetAnswerXrel (lastPoint.X) *
			(graphWidth - leftMargin - rightMargin) + leftMargin;
		for (int i = 0; i < 5; i ++)
		{
			string text = "NSNC";
			if (i > 0)
				text = answers_l[i-1];

			textHeightReduced = textHeightHere;
			if (! textRightAlignedFitsOnLeft (answerX - barRange, text, g, leftMargin))
				textHeightReduced = findFontThatFitsOnLeft (textHeightHere, answerX - barRange, text, g, leftMargin);

			if (textHeightReduced >= 4)
			{
				g.SetFontSize (textHeightReduced);
				printText (answerX - barRange, (y_l[i] + y_l[i+1])/2, 0, textHeight +4,
						text, g, alignTypes.RIGHT);
			}
			g.SetFontSize (textHeightHere);
		}

		// 5) plot horizontal bars
		g.SetSourceRGB(1, 0, 0); //red
		double lineLeftX = questionnaire.GetLineStartXrel (lastPoint.X) *
			(graphWidth - leftMargin - rightMargin) + leftMargin;
		for (int i = 1; i < 5; i ++)
		{
			g.Rectangle (lineLeftX, y_l[i] - barRange/2, answerX - lineLeftX, barRange);
			g.Fill();

			rectangleM.Add (new QRectangle (false, lineLeftX, y_l[i] - barRange/2, answerX - lineLeftX, barRange));
		}

		// 6) plot vertical bars
		List<Cairo.Color> answerColor_l = questionnaire.GetAnswerColor (lastPoint.X, qa);
		for (int i = 1; i < 5; i ++)
		{
			g.SetSourceColor (answerColor_l[i]);
			g.Rectangle (answerX -barRange/2, y_l[i] + barRange/2, barRange/2, y_l[i+1] - y_l[i] - barRange);
			g.Fill();

			if (answerColor_l[i].R == Questionnaire.red.R &&
					answerColor_l[i].G == Questionnaire.red.G &&
					answerColor_l[i].B == Questionnaire.red.B)
				rectangleM.Add (new QRectangle (false, answerX -barRange/2, y_l[i] + barRange/2, barRange/2, y_l[i+1] - y_l[i] - barRange));
			else
				rectangleM.Add (new QRectangle (true, answerX -barRange/2, y_l[i] + barRange/2, barRange/2, y_l[i+1] - y_l[i] - barRange));
		}

		g.SetSourceRGB(0, 0, 0); //black

		// 7 manage crash and points
		if (rectangleM.IsRed (
					calculatePaintX (lastPoint.X),
					calculatePaintY (lastPoint.Y) ))
		{
			crashedPaintOutRectangle ();
			questionnaire.Points --;
		}
		else if (rectangleM.IsGreen (
					calculatePaintX (lastPoint.X),
					calculatePaintY (lastPoint.Y) ))
			questionnaire.Points ++;

		g.SetFontSize (textHeightHere);
		printText (graphWidth -rightMargin, topMargin/2, 0, textHeight +4,
				"Points: " + questionnaire.Points.ToString (), g, alignTypes.RIGHT);
		g.SetFontSize (textHeight);
	}


}

public class CairoGraphForceSensorAI : CairoGraphForceSensor
{
	//private Cairo.Color colorGreen = colorFromRGB (0,200,0);
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
			SignalPointsCairoForceElastic spCairoFE,
			bool showDistance, bool showSpeed, bool showPower,
			int minDisplayFNegative, int minDisplayFPositive,
			int rectangleN, int rectangleRange,
			List<GetBestRFDInWindow> briw_l,
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
					triggerList,
					hscaleSampleA, hscaleSampleB,
					hscaleSampleC, hscaleSampleD,
					zoomed,
					briw_l,
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
			TriggerList triggerList,
			int hscaleSampleA, int hscaleSampleB,
			int hscaleSampleC, int hscaleSampleD,
			bool zoomed,
			List<GetBestRFDInWindow> briw_l,
			List<GetMaxAvgInWindow> gmaiw_l,
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
				paintGrid(gridTypes.BOTH, true, 0);

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
						"A", calculatePaintX (points_l[hscaleSampleA].X),
						"B", calculatePaintX (points_l[hscaleSampleB].X),
						true, 15, 0, yellow, yellowTransp);

			// paint the CD rectangle
			if (hscaleSampleC >= 0 && hscaleSampleD >= 0 &&
					points_l.Count > hscaleSampleC && points_l.Count > hscaleSampleD
					&& (hscaleSampleC != hscaleSampleA || hscaleSampleD != hscaleSampleB))
				CairoUtil.PaintVerticalLinesAndRectangle (g, graphHeight,
						"C", calculatePaintX (points_l[hscaleSampleC].X),
						"D", calculatePaintX (points_l[hscaleSampleD].X),
						true, 15, 0, green, greenTransp);

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
								xgStart, graphHeight -bottomMargin);

					// display right vertical line
					if (! arrowR)
						CairoUtil.PaintSegment (g,
								xgEnd, textHeight +6,
								xgEnd, graphHeight -bottomMargin);

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
			foreach (GetMaxAvgInWindow gmaiw in gmaiw_l)
			{
				LogB.Information ("gmaiw: " + gmaiw.ToString ());
				if ( points_l != null && gmaiw.MaxSampleEnd >= 0 && points_l.Count > gmaiw.MaxSampleEnd)
					paintMaxAvgInWindow (gmaiw.MaxSampleStart, gmaiw.MaxSampleEnd, gmaiw.Max, points_l);
			}

			foreach (GetBestRFDInWindow briw in briw_l)
			{
				if (briw.Error == "")
				{
					g.LineWidth = 2;
					drawCircle (calculatePaintX (points_l[briw.MaxSampleStart].X), calculatePaintY (points_l[briw.MaxSampleStart].Y), 8, black, false);
					drawCircle (calculatePaintX (points_l[briw.MaxSampleEnd].X), calculatePaintY (points_l[briw.MaxSampleEnd].Y), 8, black, false);

					List<PointF> briwP_l = new List<PointF> ();
					briwP_l.Add (new PointF (points_l[briw.MaxSampleStart].X, points_l[briw.MaxSampleStart].Y));
					briwP_l.Add (new PointF (points_l[briw.MaxSampleEnd].X, points_l[briw.MaxSampleEnd].Y));
					preparePredictedLine (briwP_l);
				}
			}

			g.LineWidth = 2;

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

public class Questionnaire
{
	public int Points;

	public static Cairo.Color red = new Cairo.Color (1, 0, 0, 1);
	private Cairo.Color green = new Cairo.Color (0, 1, 0, 1);
	private Cairo.Color transp = new Cairo.Color (0, 0, 0, 0);

	private int n; //number of questions

	private List<QuestionAnswers> qa_l = new List<QuestionAnswers> () {
		new QuestionAnswers ("Year of 1st Chronojump version", "2004", "2008", "2012", "2016"),
		new QuestionAnswers ("Name of wireless photocells", "WICHRO", "RUN+", "PhotoProto", "TopGear"),
		new QuestionAnswers ("Chronojump products are made in ...", "Barcelona", "Helsinki", "New York", "Munich"),
		new QuestionAnswers ("Which jumps are used to calculate elasticity?", "CMJ / SJ", "CMJ / ABK", "ABK / DJ", "SJ / ABK"),
		new QuestionAnswers ("Name of the microcontroller for jumps and races with photocells?", "Chronopic", "Clock fast", "Arduino", "Double step"),
		new QuestionAnswers ("Race to measure left and right turns?", "3L3R", "Margaria", "505", "100 m hurdles"),
		new QuestionAnswers ("Biggest contact platform", "A1", "A2", "A3", "A4"),
		new QuestionAnswers ("Name of 1st version on 2031", "3.1.0", "2.31", "2.3.1", "23.1"),
		new QuestionAnswers ("Max distance of Race Analyzer", "100 m", "5 m", "10 m", "30 m"),
		new QuestionAnswers ("Sample frequency of our encoder", "1 kHz", "1 Hz", "10 Hz", "100 Hz")
	};
	private List<QuestionAnswers> qaRandom_l;

	public Questionnaire (int n, string filename)
	{
		this.n = n;

		// read questions file if needed
		if (filename != null && filename != "")
		{
			List<QuestionAnswers> qaLoading_l = getQuestionsFromFile (filename);
			if (qaLoading_l != null)
				qa_l = qaLoading_l;
		}

		// randomize questions
		qaRandom_l = Util.ListRandomize (qa_l);

		// if questions < n -> set n; if questions > n -> cut to n
		if (qaRandom_l.Count < n)
			n = qaRandom_l.Count;
		else if (qaRandom_l.Count > n)
			qaRandom_l = Util.ListGetFirstN (qaRandom_l, n);

		Points = 0;
	}

	public bool FileIsOk (string filename)
	{
		return (getQuestionsFromFile (filename) != null);
	}

	private List<QuestionAnswers> getQuestionsFromFile (string filename)
	{
		List<List<string>> rows_ll = UtilCSV.ReadAsListListString (filename, ';');
		if (rows_ll.Count == 0 || rows_ll[0].Count != 5)
		{
			rows_ll = UtilCSV.ReadAsListListString (filename, ',');

			if (rows_ll.Count == 0 || rows_ll[0].Count != 5)
				return null;
		}

		List<QuestionAnswers> list = new List<QuestionAnswers> ();
		foreach (List<string> row_l in rows_ll)
			list.Add (new QuestionAnswers(row_l[0], row_l[1], row_l[2], row_l[3], row_l[4]));

		return list;
	}

	public QuestionAnswers GetQAByMicros (double micros)
	{
		double seconds = micros / 1000000;

		//TODO: fix this
		if (seconds > 100)
			seconds = 0;

		if (seconds < 10)
			return qaRandom_l[0];
		else
			return qaRandom_l[Convert.ToInt32(Math.Floor(seconds/10))];
	}

	//just to track the number of questions
	public int GetQNumByMicros (double micros)
	{
		double seconds = micros / 1000000;

		//TODO: fix this
		if (seconds > 100)
			seconds = 0;

		if (seconds < 10)
			return 0 + 1;
		else
			return Convert.ToInt32(Math.Floor(seconds/10)) + 1;
	}

	public double GetLineStartXrel (double micros)
	{
		double xrel = GetAnswerXrel (micros) -.3;
		if (xrel < 0)
			xrel = 0;

		return xrel;
	}
	public double GetAnswerXrel (double micros)
	{
		double seconds = micros / 1000000;

		//TODO: fix this
		if (seconds > 100)
			seconds = 0;

		if (seconds < 10)
			return 1 - seconds/10;
		else
			return 1 - (seconds % 10)/10;
	}

	public List<Cairo.Color> GetAnswerColor (double micros, QuestionAnswers qa)
	{
		double xrel = GetAnswerXrel (micros) -.3;
		if (xrel < 0)
			xrel = 0;

		if (xrel > .3)
			return new List<Cairo.Color> { transp, transp, transp, transp, transp };

		List<Cairo.Color> color_l = new List<Cairo.Color> ();
		color_l.Add (transp);
		foreach (string answer in qa.TopBottom_l)
		{
			if (qa.AnswerIsCorrect (answer))
				color_l.Add (green);
			else
				color_l.Add (red);
		}
		return color_l;
	}

	public int N {
		get { return n; }
	}
}

public class QuestionAnswers
{
	public List<string> TopBottom_l;
	public int CorrectPos;
	public string question;

	private string aCorrect;

	public QuestionAnswers (string question, string aCorrect, string aBad1, string aBad2, string aBad3)
	{
		this.question = question;
		this.aCorrect = aCorrect;

		TopBottom_l = new List<string> () { aCorrect, aBad1, aBad2, aBad3 };
		TopBottom_l = Util.ListRandomize (TopBottom_l);
		CorrectPos = Util.FindOnListString (TopBottom_l, aCorrect);
	}

	public bool AnswerIsCorrect (string answer)
	{
		return (answer == aCorrect);
	}
}

public class QRectangleManage
{
	private List<QRectangle> rectangle_l;

	public QRectangleManage ()
	{
		rectangle_l = new List<QRectangle> ();
	}

	public void Add (QRectangle r)
	{
		rectangle_l.Add (r);
	}

	public bool IsRed (double x, double y)
	{
		foreach (QRectangle r in rectangle_l)
			if (! r.good && r.x <= x && r.x2 >= x &&
					r.y <= y && r.y2 >= y)
				return true;

		return false;
	}

	public bool IsGreen (double x, double y)
	{
		foreach (QRectangle r in rectangle_l)
			if (r.good && r.x <= x && r.x2 >= x &&
					r.y <= y && r.y2 >= y)
				return true;

		return false;
	}
}
public class QRectangle
{
	public bool good; //true: green, false: red
	public double x;
	public double y;
	public double x2;
	public double y2;

	public QRectangle (bool good, double x, double y, double width, double height)
	{
		this.good = good;
		this.x = x;
		this.y = y;
		this.x2 = x + width;
		this.y2 = y + height;
	}
}

public class Asteroids
{
	public int Points;
	public bool Dark;
	public int MaxY;
	public int MinY;
	public int ShotsFrequency;

	private List<Asteroid> asteroid_l;
	private List<Shot> shot_l;
	private Random random = new Random();
	private double lastCrash; //to paint ship in red for half second

	private const int playerRadius = 6;
	private bool micros;
	private int multiplier;
	private Cairo.Color bluePlots = new Cairo.Color (0, 0, .78, 1);
	private Cairo.Color gray = new Cairo.Color (.5, .5, .5, 1);
	//private Cairo.Color white = new Cairo.Color (1, 1, 1, 1);
	private Cairo.Color yellow = new Cairo.Color (0.906, 0.745, 0.098, 1);
	private Cairo.Color redDark = new Cairo.Color (0.55, 0, 0, 1);

	public Asteroids (int maxY, int minY, bool Dark, int asteroidsFrequency, int shotsFrequency, bool micros)
	{
		this.Dark = Dark;
		this.MaxY = maxY;
		this.MinY = minY;
		this.ShotsFrequency = shotsFrequency;
		this.micros = micros;

		if (micros)
			multiplier = 1000000;
		else
			multiplier = 1000;

		Points = 0;
		lastCrash = -1; //to not start in red
		asteroid_l = new List<Asteroid> ();
		shot_l = new List<Shot> ();
		int plotSeconds = 100;

		for (int i = 0; i < asteroidsFrequency * plotSeconds; i ++)
		{
			int xStart = random.Next (7*multiplier, 100*multiplier);
			int usLife = random.Next (3*multiplier/10, 15*multiplier);
			asteroid_l.Add (new Asteroid (
						xStart, random.Next (minY, maxY), // y (force)
						usLife, random.Next (minY, maxY), // y (force)
						random.Next (20, 100), // size
						createAsteroidColor (),
						micros
						));
		}

		/*
		//debug with just one
		asteroid_l.Add (new Asteroid (10 * multiplier, -50, 5 * multiplier, +50,
					50, createAsteroidColor (), micros));
					*/

	}

	public List<Asteroid> GetAllAsteroidsPaintable (double startAtPointX, int marginRightInSeconds)
	{
		List<Asteroid> aPaintable_l = new List<Asteroid> ();
		foreach (Asteroid a in asteroid_l)
			if (a.NeedToShow (startAtPointX, marginRightInSeconds))
				aPaintable_l.Add (a);

		return aPaintable_l;
	}

	public bool DoesAsteroidCrashedWithPlayer (double asteroidX, double asteroidY, int asteroidSize, double playerX, double playerY)
	{
		return (CairoUtil.GetDistance2D (asteroidX, asteroidY, playerX, playerY) < asteroidSize + playerRadius);
	}

	public void AsteroidCrashedWithPlayerSetTime (double timeNow)
	{
		lastCrash = timeNow;
	}

	public void Shot (PointF p)
	{
		shot_l.Add (new Shot (p, micros));
	}

	public List<Shot> GetAllShotsPaintable (double timeNow)
	{
		List<Shot> sPaintable_l = new List<Shot> ();
		foreach (Shot s in shot_l)
			if (s.NeedToShow (timeNow))
				sPaintable_l.Add (s);

		return sPaintable_l;
	}

	public void PaintShip (double x, double y, double timeNow, Context g)
	{
		Cairo.Color playerColor = bluePlots;
		if (Dark)
			playerColor = yellow;

		//after a crash show ship half red for .5 seconds
		if (lastCrash > 0 && timeNow - lastCrash < .5*multiplier)
			playerColor = redDark;

		CairoUtil.DrawCircle (g, x, y, playerRadius, playerColor, true);
	}

	public void PaintShot (Shot s, double sx, double sy, double timeNow, Context g)
	{
		Cairo.Color color = bluePlots;
		if (Dark)
			color = yellow;
		if (s.LifeIsEnding (timeNow))
			color = gray;

		g.Save ();
		g.LineWidth = 2;
		g.SetSourceColor (color);
		g.MoveTo (sx -3, sy);
		g.LineTo (sx +3, sy);
		g.Stroke ();
		g.Restore ();

		//drawCircle (sx, sy, s.Size, color, true);
	}

	public bool ShotCrashedWithAsteroid (double sx, double sy, int size,
			List<Asteroid> asteroid_l, List<Point3F> asteroidXYZ_l)
	{
		for (int i = 0; i < asteroidXYZ_l.Count; i ++)
		{
			Point3F aXYZ = asteroidXYZ_l[i];
			if (CairoUtil.GetDistance2D (aXYZ.X, aXYZ.Y, sx, sy) < aXYZ.Z + size)
			{
				asteroid_l[i].Alive = false;
				return true;
			}
		}

		return false;
	}

	private Cairo.Color createAsteroidColor ()
	{
		Cairo.Color color;
		do {
			color = new Cairo.Color (random.NextDouble (), random.NextDouble (), random.NextDouble (),
					(double) random.Next (5,10)/10); //alpha: .5-1
		} while (Dark == CairoUtil.ColorIsDark (color));

		return color;
	}

}

public class Asteroid
{
	private int xStart; //time. When screen right is this time it will start
	private int yStart; //force
	private int usLife; //time
	private int yEnd; //force
	private int size;
	private Cairo.Color color;
	private bool alive;
	private int multiplier;

	public Asteroid (int xStart, int yStart, int usLife, int yEnd, int size, Cairo.Color color, bool micros)
	{
		this.xStart = xStart;
		this.yStart = yStart;
		this.usLife = usLife;
		this.yEnd = yEnd;
		this.size = size;
		this.color = color;

		if (micros)
			multiplier = 1000000;
		else
			multiplier = 1000;

		this.alive = true;
	}

	public bool NeedToShow (double graphUsStart, int graphSecondsAtRight)
	{
		LogB.Information ("NeedToShow 0");
		if (! alive)
			return false;

		int graphUsAtRight = graphSecondsAtRight * multiplier;
		double graphUsTotalAtRight = graphUsStart + graphUsAtRight;

		// the 3000000 is for having a bit of margin to easily consider radius
		// to not have asteroids appear/disappear on sides when center arrives to that limits
		if (xStart - 3*multiplier > graphUsTotalAtRight)
		{
			LogB.Information ("NeedToShow 1");
			return false;
		} if (xStart + usLife + 3*multiplier < graphUsTotalAtRight)
		{
			LogB.Information ("NeedToShow 2");
			LogB.Information (string.Format (
						"xStart: {0}, usLife: {1}, multiplier: {2}, graphUsTotalAtRight: {3}",
						xStart, usLife, multiplier, graphUsTotalAtRight));
			return false;
		}

		LogB.Information ("NeedToShow 3");
		return true;
	}

	public double GetTimeNowProportion (double graphUsStart, int graphSecondsAtRight)
	{
		int graphUsAtRight = graphSecondsAtRight * multiplier;
		double graphUsTotalAtRight = graphUsStart + graphUsAtRight;

		return UtilAll.DivideSafe (graphUsTotalAtRight - xStart, usLife);
	}

	public double GetYNow (double graphUsStart, int graphSecondsAtRight)
	{
		double lifeProportion = GetTimeNowProportion (graphUsStart, graphSecondsAtRight);
		return lifeProportion * (yEnd - yStart) + yStart;
	}

	public void Destroy ()
	{
		alive = false;
	}

	public override string ToString ()
	{
		return string.Format ("({0},{1}) ({2},{3}) size: {4} color: ({5},{6},{7} {8})",
				xStart, yStart, xStart+usLife, yEnd, size, color.R, color.G, color.B, color.A);
	}

	public int Size {
		get { return size; }
	}

	public Cairo.Color Color {
		get { return color; }
	}
	public bool Alive {
		set { alive = value; }
	}
}

public class Shot
{
	private const int life = 3; //will not arrive to end of screen (and not kill something that still we have not seen)
	private const int speed = 3; //relative to ship //note if this change, life will need to change
	private const int size = 2;
	private int multiplier;

	private int xStart; //time when started
	private int yStart;
	private bool alive;

	public Shot (PointF p, bool micros)
	{
		this.xStart = Convert.ToInt32 (p.X); //TODO: to the right of the "ship"
		this.yStart = Convert.ToInt32 (p.Y);
		this.alive = true;

		if (micros)
			multiplier = 1000000;
		else
			multiplier = 1000;
	}

	public bool NeedToShow (double timeNow)
	{
		if (! alive)
			return false;

		if (timeNow - xStart > multiplier * life)
		       return false;

		return true;
	}

	public double GetXNow (double timeNow)
	{
		return xStart + speed * (timeNow - xStart);
	}

	// will be shown on gray
	public bool LifeIsEnding (double timeNow)
	{
		if (timeNow - xStart > multiplier * .75 * life && timeNow - xStart <= multiplier * life)
		       return true;

		return false;
	}

	public int Ystart {
		get { return yStart; }
	}
	public int Size {
		get { return size; }
	}
	public bool Alive {
		set { alive = value; }
	}
}
