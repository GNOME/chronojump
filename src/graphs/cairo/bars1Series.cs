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
 *  Copyright (C) 2004-2026   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List
using Gdk;
using Gtk;
using Cairo;


public class CairoBars1Series : CairoBars
{
	private List<PointF> barMain_l;
	private List<Cairo.Color> colorMain_l;
	private List<string> names_l;
	private List<List<double>> barMainIntervals_l; // on jumpR, runI each or the tracks

	//constructors when there are no points
	public CairoBars1Series (DrawingArea area, Type type, string font, string message)
	{
		new CairoBars1Series (area, type, font, message, 0, "");
	}
	public CairoBars1Series (DrawingArea area, Type type, string font, string message, double historicalD, string historicalStr)
	{
		this.area = area;
		this.type = type;

		LogB.Information("constructor without points, area is null:" + (area == null).ToString());
		LogB.Information("constructor without points, area.Window is null:" + (area.Window == null).ToString());
		initGraph(font, 1); //.8 to have title at right

		if(message != "")
			writeMessageAtCenter(message);

		if (historicalStr != "")
			printBestPersonExHistorical (historicalD, historicalStr);

		endGraphDisposing(g, surface, area.Window);
	}

	//regular constructor
	public CairoBars1Series (DrawingArea area, Type type, MouseClickable clickable, bool paintAxis, bool paintGrid)
	{
		this.area = area;
		this.type = type;
		this.clickable = clickable;
		this.paintAxis = paintAxis;
		this.paintGrid = paintGrid;

		this.colorSerieA = colorFromRGBA (Config.ColorBackground); //but note if we are using system colors, this will not match
		spaceBetweenBars = true;
	}

	protected override void topMarginSet ()
	{
		if(type == Type.ENCODER)
			topMargin = 20;
		else
			topMargin = 40;
	}

	protected override void findMaximums()
	{
		bool first = true;
		foreach(PointF p in barMain_l)
		{
			if(p.Y > maxY)
				maxY = p.Y;
			if(p.Y < minY)
				minY = p.Y;

			if (first || p.Y > maxYForBoxplotShadow)
				maxYForBoxplotShadow = p.Y;
			if (first || p.Y < minYForBoxplotShadow)
				minYForBoxplotShadow = p.Y;

			first = false;
		}

		if(cairoBarsGuideManage != null  && cairoBarsGuideManage.GetMax() > maxY)
			maxY = cairoBarsGuideManage.GetMax();

		if (boxplotSession != null)
		{
			if (boxplotSession.MaxAbsolute > maxY)
				maxY = boxplotSession.MaxAbsolute;
			if (boxplotSession.MinAbsolute < minY)
				minY = boxplotSession.MinAbsolute;
		}

		// this is only encoder
		if(maxIntersession >= maxY)
			maxY = maxIntersession;

		if (bestPersonExHistoricalStr != "")
		{
			bottomMargin += bestPersonExHistoricalYpx;
			if (bestPersonExHistoricalD > maxY)
				maxY = bestPersonExHistoricalD;
		}

		//points X start at 1
		minX = 0;
		maxX = barMain_l.Count + 1;

		if (barsOrPoints == BarsOrPoints.BARS)
		{
			maxY += .1*maxY; //to accomodate texts above
			minY = 0;
		} else { // (barsOrPoints == BarsOrPoints.POINTS)
			separateMinYMaxYIfNeeded (ref minY, ref maxY);

			double personIconReal = Math.Abs (calculateRealY (0) - calculateRealY (24));
			maxY += 2 * personIconReal;	// *2 to ensure enough spacing
			minY -= 2 * personIconReal;	// *2 to ensure enough spacing
		}
	}

	protected override void plotBars ()
	{
                //calculate separation between series and bar width
		/*
		   | LM |graphWidthUsable| RM |
		   | LM |  __   __   __  | RM |
		   | LM | |  | |  | |  | | RM |
		   | LM | |  | |  | |  | | RM |
		   | LM |s|  |b|  |b|  |s| RM |

		   LM, RM: Left Margin, Right margin
		   barWidthRatio (here 1)
		   s: sideWidthRatio (here .5)
		   b: spaceBetweenBarsRatio (here .5)
		 */
		double graphWidthUsable = graphWidth -(leftMargin+rightMargin);
		double barWidthRatio = 1; //barWidth will be 1 respect the following two objects:
		double sideWidthRatio = .5; //at left of the bars have the space of .5 barWidth, same for the right
		if(type == Type.ENCODER && barMain_l.Count > 1) //on encoder margins are shown to draw the mice, and just a bit more
			sideWidthRatio = 0.25;

		double spaceBetweenBarsRatio = .7;
		if(! spaceBetweenBars) //on runInterval realtime, as the bars are together continuous on time
			spaceBetweenBarsRatio = 0;

		/*
		   divide graphWidhtUsable by total objects (bars, leftrightspace, spacesbetweenbars)
		   for 3 bars on ratios 1, .5, .5, this will be 5
		   */
		barWidth = UtilAll.DivideSafe(graphWidthUsable,
			barMain_l.Count * barWidthRatio + 2*sideWidthRatio + (barMain_l.Count-1) * spaceBetweenBarsRatio);
		double distanceBetweenCols = barWidth * spaceBetweenBarsRatio;

		resultFontHeight = getBarsResultFontHeight (barWidth*1.20); //*1.2 because there is space at left and right

		double adjustXonPOINTS = UtilAll.DivideSafe (barWidth, 2.0); // as all this class is thought for BARS, when use POINTS, just plot the point half bar to the right to be the same X
		if (barsOrPoints == BarsOrPoints.BARS)
			adjustXonPOINTS = 0;

		//LogB.Information("resultFontHeight: " + resultFontHeight.ToString());

		//debug
		/*
		LogB.Information("inBarNums_l:");
		for(int j=0; j < inBarNums_l.Count; j ++)
			LogB.Information(inBarNums_l[j].ToString());
			*/
		/*
		LogB.Information("edgeBarNums_l:");
		for(int j=0; j < edgeBarNums_l.Count; j ++)
			LogB.Information(edgeBarNums_l[j].ToString());
			*/

		CCGradient ccGradient = null;
		if (color_l.Count > 0)
			ccGradient = new CCGradient (color_l, colorSerieA);

		Pixbuf pixbuf = Chronojump.MyPixbuf.Get (null, Util.GetImagePath(false) + "image_person_outline.png");

		//for video
		double timesSubtestPrevious = 0;
		double timesSubtestThis = 0;

		for(int i = 0; i < barMain_l.Count; i ++)
		{
			PointF p = barMain_l[i];

			double spacesBetweenBars = 0;
			if(i >= 1)
				spacesBetweenBars = i*distanceBetweenCols;

			double x = leftMargin + sideWidthRatio*barWidth + i*barWidth + spacesBetweenBars;
			double y = calculatePaintY(p.Y);

			Cairo.Color barColor = colorSerieA;
			if(colorMain_l != null && colorMain_l.Count == barMain_l.Count)
				barColor = colorMain_l[i];

			// used on encoder POWERGRAVITATORY extraWeight
			if (ccGradient != null && color_l.Count == barMain_l.Count && ccGradient.ValuesAreDifferent ())
				barColor = ccGradient.GetColor (color_l[i]);

			if (barsOrPoints == BarsOrPoints.BARS)
			{
				if (barMainIntervals_l != null && barMainIntervals_l.Count > i && barMainIntervals_l[i].Count > 1)
				{
					// rectangle not rounded (radius = 4) to show correctly the inner color
					drawRoundedRectangle (true, x, y, barWidth, graphHeight -y -bottomMargin, 0, g, barColor,
							UtilList.FoundInListInt (best_l, i),
							UtilList.FoundInListInt (worst_l, i));

					g.SetSourceColor (colorFromRGBA (Config.ColorBackgroundShifted));
					double accu = 0;
					for (int j = 0; j < barMainIntervals_l[i].Count; j ++)
					{
						accu += barMainIntervals_l[i][j];
						if (j >= 1 && ! Util.IsEven (j))
						{
							g.Rectangle (x+2, calculatePaintY (accu) +2, // +1 on Y to allow showing the top of the bar in blue color
								barWidth -4, graphHeight -calculatePaintY (barMainIntervals_l[i][j]) -bottomMargin);
							g.Fill();
						}
					}
				} else
					drawRoundedRectangle (true, x, y, barWidth, graphHeight -y -bottomMargin, 4, g, barColor,
							UtilList.FoundInListInt (best_l, i),
							UtilList.FoundInListInt (worst_l, i));
			} else
				drawCircle (g, x + adjustXonPOINTS, y, POINTS_SIZE, black, barColor);

			bool isSelected = false;
			if (selectedPos_l.Count > 0) // used on encoder reps
				isSelected = UtilList.FoundInListInt (selectedPos_l, i);
			else
				isSelected = i == selectedPos;

			barResult_l.Add (new BarResult (new Point3F(x + barWidth/2, y, p.Y), isSelected, true, black));

			if (barsOrPoints == BarsOrPoints.BARS)
				mouseLimits.AddInPos (i, x, y, x+barWidth, graphHeight -bottomMargin);
			else // (barsOrPoints == BarsOrPoints.POINTS)
				mouseLimits.AddInPos (i, x, y-POINTS_SIZE, x+barWidth, y+POINTS_SIZE);

			//videoPlayTimeInSeconds
			if (videoPlayTimes_l != null && videoPlayTimes_l.Count > i)
			{
				//as bars data is not time for this mode, use supplied videoPlayTimes_l
				timesSubtestThis = videoPlayTimes_l[i];
			}

			string videoPlayingStr = "";
			if (videoPlayTimeInSeconds > 0)
			{
				/*
				LogB.Information ("OOOOOO1");
				LogB.Information ("videoPlayTimeInSeconds", videoPlayTimeInSeconds);
				LogB.Information ("timesSubtestPrevious", timesSubtestPrevious);
				LogB.Information ("timesSubtestThis", timesSubtestThis);
				*/
				if (videoPlayTimeInSeconds >= timesSubtestPrevious &&
						videoPlayTimeInSeconds <= timesSubtestThis)
					videoPlayingStr = " playing";

				timesSubtestPrevious = timesSubtestThis;
			}

			/*
			if (inBarNums_l.Count > 0 && inBarNums_l.Count > i && inBarNums_l[i] >= 0) //not show the non-Wichro -1s
				printTextInBar(x +barWidth/2, graphHeight -bottomMargin -10,
						0, textHeight+2, inBarNums_l[i].ToString(), g, true);
			 */
			//edgeBar is drawn at end to not be overlapped by next bar

			//print the type at bottom
			//printTextMultiline (x + barWidth/2, graphHeight -bottomMargin + fontHeightForBottomNames/2, 0, fontHeightForBottomNames,
			g.SetSourceColor (black);
			int textY = graphHeight - fontHeightForBottomNames * 2/3;
			if (bestPersonExHistoricalStr != "")
				textY -= bestPersonExHistoricalYpx;

			printTextMultiline (x + barWidth/2, textY,
					0, fontHeightForBottomNames,
					names_l[i] + videoPlayingStr, g, alignTypes.CENTER,
					UtilList.FoundInListInt(saved_l, i));
			//LogB.Information("names_l[i]: " + names_l[i]);

			barsXCenter_l.Add(x + barWidth/2);

			//draw personIcon if needed
			if (personIcon_l.Count == barMain_l.Count && personIcon_l[i])
			{
				double personIconY = y -1.5*resultFontHeight -24;	// above text, and 24px is pixbuf height
				if (barsOrPoints == BarsOrPoints.POINTS)
				{
					//personIconY = y +POINTS_SIZE; // above the point
					personIconY = calculatePaintY (minY) -24; // draw at bottom (like on NH)
				}

				Gdk.CairoHelper.SetSourcePixbuf (g, pixbuf,
						x-12 + (barWidth/2.0),  // -12 because pixbuf is 24 px
						personIconY);
				g.Paint();
			}
		}

		if (selectedDoubleDefined)
			selectedForBoxplot_l.Add (calculatePaintY (selectedDouble));
		//encoder
		if (selectedDouble_l.Count > 0)
			foreach (double d in selectedDouble_l)
				selectedForBoxplot_l.Add (calculatePaintY (d));
	}

	//done here and not in the constructor because most of this variables are known after construction
	public override void PassData1Serie (List<PointF> barMain_l,
			List<Cairo.Color> colorMain_l, List<string> names_l,
			List<List<double>> barMainIntervals_l,
			int fontHeightAboveBar, int fontHeightForBottomNames, int marginForBottomNames,
			string titleStr, List<int> best_l, List<int> worst_l,
			BarsOrPoints barsOrPoints)
	{
		this.barMain_l = barMain_l;
		this.colorMain_l = colorMain_l;
		this.names_l = names_l;
		this.barMainIntervals_l = barMainIntervals_l;
		this.fontHeightAboveBar = fontHeightAboveBar;
		this.fontHeightForBottomNames = fontHeightForBottomNames;
		this.marginForBottomNames = marginForBottomNames;

		if(! encoderTitle)
			this.titleStr = titleStr;

		this.best_l = best_l;
		this.worst_l = worst_l;
		this.barsOrPoints = barsOrPoints;
	}

	public override void GraphDo ()
	{
		LogB.Information("at CairoBars1Series.Do");
		//LogB.Information(string.Format("bottomMargin pre: {0}, marginForBottomNames: {1}", bottomMargin, marginForBottomNames));
		bottomMargin += marginForBottomNames;

                findMaximums();

		g.SetFontSize(textHeight);

		if (calculatePaintY (10) > calculatePaintY (0))
		{
			printText (graphWidth/2, graphHeight/2, 0, textHeight,
					Constants.GraphNeedMoreHeight (), g, alignTypes.CENTER);
			endGraphDisposing(g, surface, area.Window);
			return;
		}

		if(paintAxis)
			paintAxisDo (2);

		if(paintGrid)
			paintGridDo (gridTypes.HORIZONTALLINES, true);
		//g.SetFontSize(textHeight);

		/*
		if(cairoBarsGuideManage != null)
			drawGuides(colorSerieA);
			*/

		if (bestPersonExHistoricalStr != "")
			printBestPersonExHistorical ();

		g.SetSourceColor(black);

		selectedForBoxplot_l = new List <double> ();
		plotBars ();

		drawBoxplots (colorSerieA);

		if(cairoBarsArrow != null)
			plotArrow();

		if (cbsld.data_l.Count > 0)
			plotAlternativeLine (cbsld);

		if (edgeBarNums_l.Count > 0)
			plotEdgeBarNums ();

		plotResultsOnBar();

		writeTitleAtTop ();

		if(maxIntersession > 0)
			writePersonsBest (); //encoder !relativeToSet

		addClickableMarkIfNeeded (clickable, g);

		if (screenshotURL != "")
			CairoUtil.GetScreenshotFromDrawingArea (area, g, screenshotURL);

		endGraphDisposing(g, surface, area.Window);
	}
}
