
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
 *  Copyright (C) 2022   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List
using Gtk;
using Cairo;


public class CairoGraphRaceAnalyzer : CairoXY
{
	/*
	   at load if 1st value of triggerList is lower than minAccel,
	   shift points and triggerList to sample before of 1st trigger
	   */
	private bool setExists; //only used to show no data on load (at speed/time)
//	private bool load;

	int points_l_painted;
	private bool isSprint;
	private bool plotMaxMark;
	private RunEncoderSegmentCalcs segmentCalcs;

	//plotSegmentBars will plot power, force or accel according to mainVariable
	private bool plotSegmentBars;
	private FeedbackWindow.RunsEncoderMainVariableTypes mainVariable;

	private bool useListOfDoublesOnY;

	//to avoid to have new data on PassData while the for is working on plotRealPoints
//	static bool doing;
	//regular constructor
	public CairoGraphRaceAnalyzer (
//			bool load,
			bool setExists,
			DrawingArea area, string title,
			string yVariable, string yUnits,
			bool isSprint, bool plotMaxMark,
			RunEncoderSegmentCalcs segmentCalcs,
			bool plotSegmentBars, FeedbackWindow.RunsEncoderMainVariableTypes mainVariable,
			bool useListOfDoublesOnY) //for pos/time graph
	{
		this.setExists = setExists;
//		this.load = load;
		this.area = area;
		this.title = title;
		this.colorBackground = colorFromRGBA(Config.ColorBackground); //but note if we are using system colors, this will not match

		xVariable = timeStr;
		this.yVariable = yVariable;
		xUnits = "s";
		this.yUnits = yUnits;
		this.isSprint = isSprint;
		this.plotMaxMark = plotMaxMark;
		this.segmentCalcs = segmentCalcs;
		this.plotSegmentBars = plotSegmentBars;
		this.mainVariable = mainVariable;
		this.useListOfDoublesOnY = useListOfDoublesOnY;
		
//		doing = false;
		points_l_painted = 0;
	}

	public void Reset()
	{
		minX = 1000000;
		maxX = 0;
		minY = 1000000;
		maxY = 0;
		xAtMaxY = 0;
		yAtMaxY = 0;
		absoluteMaxX = 0;
		absoluteMaxY = 0;

		points_l_painted = 0;
	}

	//separated in two methods to ensure endGraphDisposing on any return of the other method
	public void DoSendingList (string font,
			List<PointF> points_l,
			List<PointF> pointsCD_l,
			List<string> subtitleWithSetsInfo_l,
			bool forceRedraw,
			double videoPlayTimeInSeconds,
			PlotTypes plotType, bool blackLine, int smoothLineWindow,
			TriggerList triggerList, int timeAtEnoughAccelOrTrigger0,
			int timeAtEnoughAccelMark, double minAccel,
			int hscaleSampleA, int hscaleSampleB,
			int hscaleSampleC, int hscaleSampleD)

	{
		if (doSendingList (font, points_l, pointsCD_l, subtitleWithSetsInfo_l,
					forceRedraw, videoPlayTimeInSeconds, plotType, blackLine, smoothLineWindow,
					triggerList, timeAtEnoughAccelOrTrigger0, timeAtEnoughAccelMark, minAccel,
					hscaleSampleA, hscaleSampleB,
					hscaleSampleC, hscaleSampleD))

			endGraphDisposing(g, surface, area.Window);
	}

	//return true if graph is inited (to dispose it)
	private bool doSendingList (string font,
			List<PointF> points_l,
			List<PointF> pointsCD_l,
			List<string> subtitleWithSetsInfo_l,
			bool forceRedraw,
			double videoPlayTimeInSeconds,
			PlotTypes plotType, bool blackLine, int smoothLineWindow,
			TriggerList triggerList, int timeAtEnoughAccelOrTrigger0,
			int timeAtEnoughAccelMark, double minAccel, //timeAtEnoughAccelMark: only for capture (just to display mark), minAccel is the value at preferences
			int hscaleSampleA, int hscaleSampleB,
			int hscaleSampleC, int hscaleSampleD)
	{
		// 1) init graph

		bool dataExists = false;
		if (points_l != null)
			dataExists = true;
		else if (points_l == null && (pointsCD_l != null && pointsCD_l.Count > 0))
		{
			// on superpose, can have data of the CD but not of the AB (because set ended)
			// just create an empty points_l
			points_l = new List<PointF> ();
			dataExists = true;
		}

		bool twoSets = false;
		if (pointsCD_l != null && pointsCD_l.Count > 0)
			twoSets = true;
		else
			pointsCD_l = points_l;

		bool maxValuesChanged = false;

		if (dataExists)
		{
			maxValuesChanged = findPointMaximums(false, points_l);

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
		if(maxValuesChanged || forceRedraw || points_l.Count != points_l_painted)
		{
			initGraph( font, 1, (maxValuesChanged || forceRedraw) );
			graphInited = true;
			points_l_painted = 0;
		}
		if(points_l == null || points_l.Count == 0)
		{
			if(! graphInited)
			{
				initGraph(font, 1, true);
				graphInited = true;
			}

			paintAxis();
			if (setExists && (points_l == null || points_l.Count == 0))
			{
				printText(graphWidth/2, graphHeight/2,
						0, textHeight-3,
						string.Format ("No data. Minimum acceleration is {0} m/s^2.", minAccel),
						g, alignTypes.CENTER);
				printText(graphWidth/2, graphHeight/2 + 16,
						0, textHeight-3,
						"Maybe is too high. Change it on preferences.",
						g, alignTypes.CENTER);
			}

			return graphInited;
		}

		// 2) paint grid and write vaules on grid lines
		if(maxValuesChanged || forceRedraw)
		{
			if(segmentCalcs == null || segmentCalcs.Count == 0)
			{
				// do not show vertical grid lines if we do not pass any distance mark. Show only horizontal.
				paintGrid (gridTypes.HORIZONTALLINES, true, 0);
			}
			else {
				//horizontal
				if(segmentCalcs.Count > 0 && useListOfDoublesOnY)
				{
					g.LineWidth = 1;
					g.Save();
					g.SetDash(new double[]{1, 2}, 0);
					for(int i = 0 ; i < segmentCalcs.Count ; i ++)
					{
						double yValue = segmentCalcs.Dist_l[i];
						paintHorizontalGridLine (g, Convert.ToInt32(calculatePaintY(yValue)), yValue.ToString(), textHeight -3, false, 0);
					}
					g.Stroke ();
					g.Restore();
				} else //maybe we have not arrived to any segment
					paintGrid (gridTypes.HORIZONTALLINES, true, 0);

				//vertical
				if(segmentCalcs.Count > 0)
				{
					g.Save();
					g.SetDash(new double[]{1, 2}, 0);
					for(int i = 0 ; i < segmentCalcs.Count ; i ++)
					{
						string xTextTop = segmentCalcs.Dist_l[i].ToString();

						//seconds
						string xTextBottom = Util.TrimDecimals(segmentCalcs.TimeEnd_l[i]/1000000.0, 1).ToString();
						double xGraph = calculatePaintX(segmentCalcs.TimeEnd_l[i]/1000000.0);

						if(useListOfDoublesOnY)
							paintVerticalGridLine(g, Convert.ToInt32(xGraph), xTextBottom, textHeight-3);
						else
							paintVerticalGridLineTopBottom (g, Convert.ToInt32(xGraph), xTextTop, xTextBottom, textHeight-3);

						//LogB.Information (segmentCalcs.ToString ());  //debug
					}
					g.Stroke ();
					g.Restore();
					if(! useListOfDoublesOnY)
					{
						g.MoveTo (graphWidth -rightMargin, topMargin);
						g.LineTo (leftMargin, topMargin);
						g.Stroke();
						printXAxisTopText();
					}

					//graph the segmentCalcs
					LogB.Information("dist ; time(s) ; speedCont ; accels ; forces ; powers");

					if(plotSegmentBars)
					{
						List<double> data_l = new List<double> ();
						string mainVariableStr;
						string unitsStr;
						if(mainVariable == FeedbackWindow.RunsEncoderMainVariableTypes.POWER)
						{
							data_l = segmentCalcs.Power_l;
							mainVariableStr = powerStr;
							unitsStr = "W";
						}
						else if(mainVariable == FeedbackWindow.RunsEncoderMainVariableTypes.FORCE)
						{
							data_l = segmentCalcs.Force_l;
							mainVariableStr = forceStr;
							unitsStr = "N";
						} else //if(mainVariable == FeedbackWindow.RunsEncoderMainVariableTypes.ACCELERATION)
						{
							data_l = segmentCalcs.Accel_l;
							mainVariableStr = accelStr;
							unitsStr = "m/s^2";
						}

						g.SetSourceColor (colorFromRGB (190,190,190));
						double powerPropAt0 = MathUtil.GetProportion (0, data_l, true);

						//draw Y0 line
						g.SetSourceColor (colorFromRGB (66,66,66));
						g.MoveTo (leftMargin, calculatePaintYProportion (powerPropAt0));
						g.LineTo (calculatePaintX (points_l[points_l.Count -1].X), calculatePaintYProportion (powerPropAt0));
						g.Stroke ();
						g.SetSourceColor (black);
						printText(calculatePaintX (points_l[points_l.Count -1].X), calculatePaintYProportion (powerPropAt0) - .66*textHeight,
								0, textHeight-3, mainVariableStr, g, alignTypes.LEFT);
						printText(calculatePaintX (points_l[points_l.Count -1].X), calculatePaintYProportion (powerPropAt0) + .66*textHeight,
								0, textHeight-3, "0 " + unitsStr, g, alignTypes.LEFT);

						for(int i = 0 ; i < segmentCalcs.Count ; i ++)
						{
							// debug
							/*
							   LogB.Information(string.Format("dist: {0} ; timeStart {1} ; timeEnd: {2} ; speed: {3} : accel: {4} ; force: {5}: power: {6}",
										   segmentCalcs.Dist_l[i], segmentCalcs.TimeStart_l[i]/1000000.0, segmentCalcs.TimeEnd_l[i]/1000000.0,
							   segmentCalcs.SpeedCont_l[i], segmentCalcs.Accel_l[i], segmentCalcs.Force_l[i], segmentCalcs.Power_l[i] ));
							   */

							double powerProp = MathUtil.GetProportion (data_l[i], data_l, true);
							double xStart = calculatePaintX (segmentCalcs.TimeStart_l[i]/1000000.0);
							double xEnd = calculatePaintX (segmentCalcs.TimeEnd_l[i]/1000000.0);

							g.SetSourceColor (colorFromRGB (190,190,190));
							g.Rectangle (xStart, //x
									calculatePaintYProportion (powerPropAt0), //y
									xEnd - xStart, //width
									calculatePaintYProportion (powerProp) - calculatePaintYProportion (powerPropAt0) );

							g.FillPreserve(); //fill preparing for border later
							g.SetSourceColor (colorFromRGB (66,66,66));
							g.Stroke(); //rectangle border

							int textPadding = 1;
							if(data_l[i] < 0)
								textPadding = -1;

							g.SetSourceColor (black);
							printText((xStart + xEnd) / 2, calculatePaintYProportion (powerProp) - (textHeight) * textPadding,
									0, textHeight-3, Math.Round(data_l[i],1).ToString()// + " " + unitsStr
									, g, alignTypes.CENTER);
						}
					}
					g.Stroke ();
					g.SetSourceColor (black);
				}
				//else //maybe we have not arrived to any segment
				//	paintGridNiceAutoValues (g, minX, absoluteMaxX, minY, absoluteMaxY, 5, gridTypes.VERTICALLINES, textHeight-3);
				// 	do not show grid if we do not pass any distance mark
			}

			paintAxis();
		}

		// 3) paint points, paint smooth line, paint maximum mark, paint timeAtEnoughAccelMark on capture
		pointsRadius = 1;
		if( graphInited && points_l != null &&
				(maxValuesChanged || forceRedraw || points_l.Count != points_l_painted) )
		{
			// 3.a) paint points
			if(! blackLine)
				g.SetSourceColor (gray99);

			plotRealPoints(plotType, points_l, points_l_painted, false); //not fast. TODO: maybe use fast if is really faster
			//plotRealPoints(PlotTypes.POINTSLINES, points_l, points_l_painted, false); //not fast. TODO: maybe use fast if is really faster //to debug

			if (twoSets)
			{
				if (points_l.Count > 0)
					printText (calculatePaintX (PointF.Last (points_l).X) + 5,
							calculatePaintY (PointF.Last (points_l).Y),
							0, textHeight, "AB", g, alignTypes.LEFT);

				g.SetSourceColor (grayDark);
				plotRealPoints (plotType, pointsCD_l, points_l_painted, false); //fast (but the difference is very low)
				printText (calculatePaintX (PointF.Last (pointsCD_l).X) + 5,
						calculatePaintY (PointF.Last (pointsCD_l).Y),
						0, textHeight, "CD", g, alignTypes.LEFT);

				g.SetSourceColor (black);
			}

			if(! blackLine)
				g.SetSourceRGB (0,0,0);

			points_l_painted = points_l.Count;

			// 3.b) paint smooth line
			if(smoothLineWindow > 0 && points_l.Count > smoothLineWindow)
			{
				MovingAverage mAverageSmoothLine = new MovingAverage (points_l, smoothLineWindow);
				mAverageSmoothLine.Calculate ();
				g.SetSourceColor (bluePlots);
				plotRealPoints(plotType, mAverageSmoothLine.MovingAverage_l, 0, false); //not fast. TODO: maybe use fast if is really faster
				g.SetSourceRGB (0,0,0);
			}

			// 3.c) paint maximum mark
			if (plotMaxMark)
			{
				if (points_l.Count > 1)
					plotMaxMarkDo (points_l, smoothLineWindow, xAtMaxY, yAtMaxY);
				if (twoSets && pointsCD_l.Count > 1)
				{
					double xAtMaxYCD = 0;
					double yAtMaxYCD = 0;
					for (int i = 0; i < pointsCD_l.Count; i ++)
					{
						if (pointsCD_l[i].Y > yAtMaxYCD)
						{
							xAtMaxYCD = pointsCD_l[i].X;
							yAtMaxYCD = pointsCD_l[i].Y;
						}
					}

					plotMaxMarkDo (pointsCD_l, smoothLineWindow, xAtMaxYCD, yAtMaxYCD);
				}
			}

			// TODO: move this to xy.cs to share between forceSensor and raceAnalyzer
			// hscales start at 0
			if (hscaleSampleA >= 0 && hscaleSampleB >= 0 &&
					points_l.Count > hscaleSampleA && points_l.Count > hscaleSampleB)
				CairoUtil.PaintVerticalLinesAndRectangle (g, graphHeight,
						"A", calculatePaintX (points_l[hscaleSampleA].X),
						"B", calculatePaintX (points_l[hscaleSampleB].X),
						true, 15, 0, yellow, yellowTransp);

			if (hscaleSampleC >= 0 && hscaleSampleD >= 0 &&
					pointsCD_l.Count > hscaleSampleC && pointsCD_l.Count > hscaleSampleD
					&& (hscaleSampleC != hscaleSampleA || hscaleSampleD != hscaleSampleB))
				CairoUtil.PaintVerticalLinesAndRectangle (g, graphHeight,
						"C", calculatePaintX (pointsCD_l[hscaleSampleC].X),
						"D", calculatePaintX (pointsCD_l[hscaleSampleD].X),
						true, 15, 0, green, greenTransp);

			if (timeAtEnoughAccelMark >= 0 || timeAtEnoughAccelOrTrigger0 >= 0)
			{
				//on load we have to shift if trigger0 has been first
				if (timeAtEnoughAccelOrTrigger0 > 0 && timeAtEnoughAccelMark > timeAtEnoughAccelOrTrigger0)
					timeAtEnoughAccelMark -= timeAtEnoughAccelOrTrigger0;

				double xTimeAtEnoughAccelMark = calculatePaintX (timeAtEnoughAccelMark/1000000.0);
				g.LineWidth = 1;

				/*
				//line top/bottom and text at bottom, but they are confused with triggers
				g.SetSourceColor(red);
				g.MoveTo (xTimeAtEnoughAccelMark, topMargin);
				g.LineTo (xTimeAtEnoughAccelMark, graphHeight - bottomMargin);
				g.Stroke ();
				printText(xTimeAtEnoughAccelMark, graphHeight -bottomMargin*3/4,
						0, textHeight-3, string.Format("a >= {0} m/s^2", minAccel), g, alignTypes.LEFT);
				 */
				//above the graph with short top line
				g.SetSourceColor(black);
				g.Save();
				g.SetDash(new double[]{1, 2}, 0);
				g.MoveTo (xTimeAtEnoughAccelMark, topMargin * 2/3);
				g.LineTo (xTimeAtEnoughAccelMark, graphHeight -bottomMargin);
				g.Stroke ();
				g.Restore ();
				printText(xTimeAtEnoughAccelMark+2, topMargin * 3/4,
						0, textHeight-3, string.Format("a >= {0} m/s^2 at {1} s",
							Math.Round (minAccel, 3),
							Math.Round (timeAtEnoughAccelMark/1000000.0, 3)),
						g, alignTypes.LEFT);
			}

			if (subtitleWithSetsInfo_l.Count > 0)
				paintSignalSubtitles (subtitleWithSetsInfo_l);

		}

		// 5) paint triggers
		if(graphInited && triggerList != null && triggerList.Count() > 0)
		{
			g.LineWidth = 1;
			foreach(Trigger trigger in triggerList.GetList())
			{
				//create a new trigger to not modify the original list that will be used for pos/time, speed/time, accel/time
				Trigger triggerModified = new Trigger (trigger.Mode, trigger.Us, trigger.InOut);

				LogB.Information("trigger.Us: " + trigger.Us.ToString());
				LogB.Information("timeAtEnoughAccelOrTrigger0: " + timeAtEnoughAccelOrTrigger0.ToString());

				if (timeAtEnoughAccelOrTrigger0 > 0)
					triggerModified.Us -= timeAtEnoughAccelOrTrigger0;

				LogB.Information("triggerModified.Us fixed: " + triggerModified.Us.ToString());

				paintVerticalTriggerLine (g, triggerModified, timeUnits.SECONDS, "", textHeight -3);
			}
		}

		//show the play time mark if it fits the graph. Note that because of the stopAfter 2s, we do the <= abolsuteMaxX check
		if (videoPlayTimeInSeconds > 0 && videoPlayTimeInSeconds <= absoluteMaxX)
		{
			//printText (graphWidth - rightMargin/2, topMargin,
			//		0, textHeight +4, Util.TrimDecimals (videoPlayTimeInSeconds, 2), g, alignTypes.CENTER);
			g.MoveTo (calculatePaintX (videoPlayTimeInSeconds), topMargin);
			g.LineTo (calculatePaintX (videoPlayTimeInSeconds), graphHeight - bottomMargin);
			g.Stroke ();
		}

		//doing = false;
		return graphInited;
	}

	protected override void printYAxisText()
	{
		printText(2, Convert.ToInt32 (leftMargin/2), 0, textHeight-3, getYAxisLabel(), g, alignTypes.LEFT);
	}
	protected override void printXAxisText()
	{
		printText(graphWidth, graphHeight -bottomMargin/2,
				0, textHeight-3, getXAxisLabel() + " ", g, alignTypes.RIGHT);
	}
	private void printXAxisTopText()
	{
		printText(graphWidth, topMargin/2,
				0, textHeight-3, getAxisLabel(distanceStr, "m") + "  ", g, alignTypes.RIGHT);
	}

	private void plotMaxMarkDo (List<PointF> p_l, int smoothLineWindow, double xAtMaxY, double yAtMaxY)
	{
		if(isSprint) //on sprint plot an arrow from top speed (with moving average) to the right
		{
			double graphX = 0;
			double graphY = 0;
			if (smoothLineWindow >= 3)
			{
				MovingAverage mAverage = new MovingAverage (p_l, smoothLineWindow);
				mAverage.Calculate ();
				PointF pMaxY = mAverage.GetMaxY ();
				graphX = pMaxY.X;
				graphY = pMaxY.Y;
			} else {
				graphX = xAtMaxY;
				graphY = yAtMaxY;
			}
			plotArrowPassingRealPoints (g, colorFromRGB (255,0,0),
					graphX, graphY, p_l[p_l.Count -1].X, graphY, true, false, 0);
		}
		else  //if no sprint just plot a circle on max value
		{
			double graphX = xAtMaxY;
			double graphY = yAtMaxY;
			bool useMovingAverage = false;
			if(useMovingAverage)
			{
				MovingAverage mAverage = new MovingAverage(p_l, 5);
				mAverage.Calculate();
				PointF pMaxY = mAverage.GetMaxY();

				graphX = pMaxY.X;
				graphY = pMaxY.Y;
			}
			graphX = calculatePaintX (graphX);
			graphY = calculatePaintY (graphY);
			drawCircle (graphX, graphY, 8, red, false);
		}
	}

	/*
	protected override void paintVerticalGridLine(Cairo.Context g, int xtemp, string text, int fontH)
	{
		g.MoveTo(xtemp, graphHeight - outerMargin);
		g.LineTo(xtemp, outerMargin);
		printText(xtemp, graphHeight -Convert.ToInt32(.75 * outerMargin), 0, fontH, text, g, alignTypes.CENTER); //TODO: this only for raceAnalyzer
	}
	*/

	private void paintVerticalGridLineTopBottom (Cairo.Context g, int xtemp, string textTop, string textBottom, int fontH)
	{
		if(fontH < 1)
			fontH = 1;

		g.MoveTo(xtemp, topMargin);
		g.LineTo(xtemp, graphHeight - bottomMargin);

		printText(xtemp, topMargin/2, 0, fontH, textTop, g, alignTypes.CENTER);
		printText(xtemp, graphHeight- bottomMargin/2, 0, fontH, textBottom, g, alignTypes.CENTER);
		//LogB.Information("pvgl fontH: " + fontH.ToString());
	}
	protected override void writeTitle()
	{
	}

}
