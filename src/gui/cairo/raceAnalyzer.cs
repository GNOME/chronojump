
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
	int points_list_painted;
	private bool isSprint;
	private bool plotMaxMark;
	private RunEncoderSegmentCalcs segmentCalcs;
	private bool plotPowerBars;
	private bool useListOfDoublesOnY;

	//to avoid to have new data on PassData while the for is working on plotRealPoints
//	static bool doing;
	//regular constructor
	public CairoGraphRaceAnalyzer (
			DrawingArea area, string title,
			string yVariable, string yUnits,
			bool isSprint, bool plotMaxMark,
			RunEncoderSegmentCalcs segmentCalcs,
			bool plotPowerBars,
			bool useListOfDoublesOnY) //for pos/time graph
	{
		this.area = area;
		this.title = title;
		this.colorBackground = colorFromGdk(Config.ColorBackground); //but note if we are using system colors, this will not match

		xVariable = timeStr;
		this.yVariable = yVariable;
		xUnits = "s";
		this.yUnits = yUnits;
		this.isSprint = isSprint;
		this.plotMaxMark = plotMaxMark;
		this.segmentCalcs = segmentCalcs;
		this.plotPowerBars = plotPowerBars;
		this.useListOfDoublesOnY = useListOfDoublesOnY;
		
//		doing = false;
		points_list_painted = 0;
	}

	public void Reset()
	{
		minX = 1000000;
		maxX = 0;
		minY = 1000000;
		maxY = 0;
		xAtMaxY = 0;
		absoluteMaxX = 0;
		absoluteMaxY = 0;

		points_list_painted = 0;
	}

	//separated in two methods to ensure endGraphDisposing on any return of the other method
	public void DoSendingList (string font, List<PointF> points_list, TriggerList triggerList, bool forceRedraw, PlotTypes plotType, int smoothLineWindow)
	{
		if(doSendingList (font, points_list, triggerList, forceRedraw, plotType, smoothLineWindow))
			endGraphDisposing(g, surface, area.GdkWindow);
	}

	//return true if graph is inited (to dispose it)
	private bool doSendingList (string font, List<PointF> points_list, TriggerList triggerList, bool forceRedraw, PlotTypes plotType, int smoothLineWindow)
	{
		// 1) init graph

		bool maxValuesChanged = false;
		if(points_list != null)
			maxValuesChanged = findPointMaximums(false, points_list);

		bool graphInited = false;
		if(maxValuesChanged || forceRedraw || points_list.Count != points_list_painted)
		{
			initGraph( font, 1, (maxValuesChanged || forceRedraw) );
			graphInited = true;
			points_list_painted = 0;
		}
		if(points_list == null || points_list.Count == 0)
		{
			if(! graphInited)
			{
				initGraph(font, 1, true);
				graphInited = true;
			}

			paintAxis();
			return graphInited;
		}

		// 2) paint grid and write vaules on grid lines
		if(maxValuesChanged || forceRedraw)
		{
			if(segmentCalcs.Count == 0)
			{
				// do not show vertical grid lines if we do not pass any distance mark. Show only horizontal.
				paintGrid(gridTypes.HORIZONTALLINES, true);
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
						paintHorizontalGridLine(g, Convert.ToInt32(calculatePaintY(yValue)), yValue.ToString(), textHeight -3);
					}
					g.Stroke ();
					g.Restore();
				} else //maybe we have not arrived to any segment
					paintGrid(gridTypes.HORIZONTALLINES, true);

				//vertical
				if(segmentCalcs.Count > 0)
				{
					g.Save();
					g.SetDash(new double[]{1, 2}, 0);
					for(int i = 0 ; i < segmentCalcs.Count ; i ++)
					{
						string xTextTop = segmentCalcs.Dist_l[i].ToString();

						//seconds
						string xTextBottom = Util.TrimDecimals(segmentCalcs.Time_l[i]/1000000.0, 1).ToString();
						double xGraph = calculatePaintX(segmentCalcs.Time_l[i]/1000000.0);

						if(useListOfDoublesOnY)
							paintVerticalGridLine(g, Convert.ToInt32(xGraph), xTextBottom, textHeight-3);
						else
							paintVerticalGridLineTopBottom (g, Convert.ToInt32(xGraph), xTextTop, xTextBottom, textHeight-3);
					}
					g.Stroke ();
					g.Restore();
					if(! useListOfDoublesOnY)
					{
						g.MoveTo(graphWidth - outerMargin, outerMargin);
						g.LineTo(outerMargin, outerMargin);
						g.Stroke();
						printXAxisTopText();
					}

					//graph the segmentCalcs
					LogB.Information("dist ; time(s) ; speedCont ; accels ; forces ; powers");

					if(plotPowerBars)
					{
						g.SetSourceColor (colorFromRGB (66,66,66));
						double powerPropAt0 = MathUtil.GetProportion (0, segmentCalcs.Power_l);

						//draw Y0 line
						g.MoveTo (outerMargin, calculatePaintYProportion (powerPropAt0));
						g.LineTo (calculatePaintX (points_list[points_list.Count -1].X), calculatePaintYProportion (powerPropAt0));
						g.Stroke ();
						printText(calculatePaintX (points_list[points_list.Count -1].X), calculatePaintYProportion (powerPropAt0),
								0, textHeight-3, " 0 " + powerstr, g, alignTypes.LEFT);

						for(int i = 0 ; i < segmentCalcs.Count ; i ++)
						{
							/* debug
							   LogB.Information(string.Format("{0} ; {1} ; {2} ; {3} : {4} ; {5}", segmentCalcs.Dist_l[i], segmentCalcs.Time_l[i]/1000000.0,
							   segmentCalcs.SpeedCont_l[i], segmentCalcs.Accel_l[i], segmentCalcs.Force_l[i], segmentCalcs.Power_l[i] ));
							 */

							double powerProp = MathUtil.GetProportion (segmentCalcs.Power_l[i], segmentCalcs.Power_l);
							double xStart = calculatePaintX (points_list[0].X);
							if(i > 0)
								xStart = calculatePaintX (segmentCalcs.Time_l[i-1]/1000000.0);
							double xEnd = calculatePaintX (segmentCalcs.Time_l[i]/1000000.0);

							g.Rectangle (xStart, //x
									calculatePaintYProportion (powerPropAt0), //y
									xEnd - xStart, //width
									calculatePaintYProportion (powerProp) - calculatePaintYProportion (powerPropAt0) );
							g.Fill();

							int textPadding = 1;
							if(segmentCalcs.Power_l[i] < 0)
								textPadding = -1;
							printText((xStart + xEnd) / 2, calculatePaintYProportion (powerProp) - (textHeight) * textPadding,
									0, textHeight-3, Math.Round(segmentCalcs.Power_l[i],1).ToString() + " W", g, alignTypes.CENTER);
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

		// 3) paint points, paint smooth line, paint maximum mark
		pointsRadius = 1;
		if( points_list != null &&
				(maxValuesChanged || forceRedraw || points_list.Count != points_list_painted) )
		{
			// 3.a) paint points
			plotRealPoints(plotType, points_list, points_list_painted, false); //not fast. TODO: maybe use fast if is really faster
			points_list_painted = points_list.Count;

			// 3.b) paint smooth line
			if(smoothLineWindow > 0 && points_list.Count > 5)
			{
				MovingAverage mAverageSmoothLine = new MovingAverage (points_list, smoothLineWindow);
				mAverageSmoothLine.Calculate ();
				g.SetSourceColor (bluePlots);
				plotRealPoints(plotType, mAverageSmoothLine.MovingAverage_l, 0, false); //not fast. TODO: maybe use fast if is really faster
				g.SetSourceRGB (0,0,0);
			}

			// 3.c) paint maximum mark
			if(plotMaxMark && points_list.Count > 1)
			{
				if(isSprint) //on sprint plot an arrow from top speed (with moving average) to the right
				{
					MovingAverage mAverage = new MovingAverage(points_list, 5);
					mAverage.Calculate();
					PointF pMaxY = mAverage.GetMaxY();

					plotArrowPassingRealPoints (g, colorFromRGB(255,0,0),
							pMaxY.X, pMaxY.Y, points_list[points_list.Count -1].X, pMaxY.Y, true, false, 0);
				}
				else  //if no sprint just plot a circle on max value
				{
					double graphX = xAtMaxY;
					double graphY = maxY;
					bool useMovingAverage = false;
					if(useMovingAverage)
					{
						MovingAverage mAverage = new MovingAverage(points_list, 5);
						mAverage.Calculate();
						PointF pMaxY = mAverage.GetMaxY();

						graphX = pMaxY.X;
						graphY = pMaxY.Y;
					}
					graphX = calculatePaintX (graphX);
					graphY = calculatePaintY (graphY);

					g.MoveTo(graphX +8, graphY);
					g.Arc(graphX, graphY, 8.0, 0.0, 2.0 * Math.PI); //full circle
					g.SetSourceColor(red);
					g.Stroke();
				}
			}
		}

		// 4) paint triggers
		if(triggerList != null && triggerList.Count() > 0)
			foreach(Trigger trigger in triggerList.GetList())
				paintVerticalTriggerLine(g, trigger, textHeight -3);

		//doing = false;
		return graphInited;
	}

	protected override void printYAxisText()
	{
		printText(2, Convert.ToInt32(outerMargin/2), 0, textHeight-3, getYAxisLabel(), g, alignTypes.LEFT);
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
