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

//N series in horizontal, like jump Dj tc/tf, jumpRj (maybe with a "number of jumps" column)
public class CairoBarsNHSeries : CairoBars
{
	private List<List<PointF>> barSecondary_ll; //other/s bar/s to display at the side of Main
	private List<PointF> barMain_l;
	private List<Cairo.Color> colorMain_l;
	private List<Cairo.Color> colorSecondary_l;
	private List<string> names_l;
	private bool showLegend;
	private string labelBarMain;
	//private string labelBarSecondary;
	private bool labelRotateInFirstBar;

	private Cairo.Color colorSerieB;
	private double oneRowLegendWidth;
	private bool oneRowLegend;
	private int boxWidth = 10; //px. Same as boxHeight. box - text sep is .5 boxWidth. 1st text - 2nd box sep is 2*boxWidth

	//constructors when there are no points
	public CairoBarsNHSeries (DrawingArea area, Type type, string font)
	{
		new CairoBarsNHSeries (area, type, font, 0, "");
	}
	public CairoBarsNHSeries (DrawingArea area, Type type, string font, double historicalD, string historicalStr)
	{
		this.area = area;
		this.type = type;

		LogB.Information("constructor without points, area is null:" + (area == null).ToString());
		LogB.Information("constructor without points, area.Window is null:" + (area.Window == null).ToString());
		initGraph(font, 1); //.8 to have title at right

		if (historicalStr != "")
			printBestPersonExHistorical (historicalD, historicalStr);

		endGraphDisposing(g, surface, area.Window);
	}

	//regular constructor
	public CairoBarsNHSeries (DrawingArea area, Type type, bool showLegend, MouseClickable clickable, bool paintAxis, bool paintGrid)
	{
		this.area = area;
		this.type = type;
		this.showLegend = showLegend;
		this.clickable = clickable;
		this.paintAxis = paintAxis;
		this.paintGrid = paintGrid;

		colorSerieA = colorFromRGBA (UtilGtk.GetColorShifted(Config.ColorBackground,
					! UtilGtk.ColorIsDark(Config.ColorBackground)));
		colorSerieB = colorFromRGBA (Config.ColorBackground); //but note if we are using system colors, this will not match
	}

	protected override void topMarginSet ()
	{
		if(type == Type.ENCODER)
		{
			topMargin = 20;
			return;
		}

		if(! showLegend)
			return;

		topMargin = 50; //to accomodate legend under title
		oneRowLegend = true;
		calculateOneRowLegendWidth();

		g.SetFontSize(textHeight-2);
		Cairo.TextExtents teYLabel = g.TextExtents(getYAxisLabel());

		//check oneRowLegend does not crash with left axis label or rightMargin (icons)
		if(graphWidth/2 - oneRowLegendWidth /2 -2*boxWidth < teYLabel.Width ||
				graphWidth/2 + oneRowLegendWidth /2 + 2*boxWidth > graphWidth - rightMargin)
		{
			//topMargin really does not change, what is reduced is the space below
			//topMargin += Convert.ToInt32(.5*textHeight); //.5 because font is smaller
			oneRowLegend = false;
		}

		g.SetFontSize(textHeight);
	}
	private void calculateOneRowLegendWidth ()
	{
		g.SetFontSize(textHeight-2);

		Cairo.TextExtents te = g.TextExtents(variableSerieA);
		double serieAWidth = te.Width;

		te = g.TextExtents(variableSerieB);
		double serieBWidth = te.Width;

		oneRowLegendWidth = 1.5*boxWidth + serieAWidth + 2*boxWidth + 1.5*boxWidth + serieBWidth;

		g.SetFontSize(textHeight);
	}

	private void writeLegend ()
	{
		g.SetFontSize(textHeight-2);

		Cairo.TextExtents te = g.TextExtents(variableSerieA);
		double serieAWidth = te.Width;

		te = g.TextExtents(variableSerieB);
		double serieBWidth = te.Width;

		int boxWidth = 10; //px. Same as boxHeight. box - text sep is .5 boxWidth. 1st text - 2nd box sep is 2*boxWidth

		if(oneRowLegend)
		{
			double legendWidth = 1.5*boxWidth + serieAWidth + 2*boxWidth + 1.5*boxWidth + serieBWidth;
			double xStart = .5*graphWidth -.5*legendWidth;

			//paint 1st box
			g.SetSourceColor(colorSerieA);
			g.Rectangle(xStart, topMargin -1.25*textHeight, boxWidth, boxWidth);
			g.FillPreserve();
			g.SetSourceColor(black);
			g.Stroke();

			//write 1st variable
			xStart += 1.5*boxWidth;
			printText(xStart, topMargin -textHeight, 0, textHeight-2, variableSerieA, g, alignTypes.LEFT);

			//paint 2nd box
			xStart += serieAWidth + 2*boxWidth;
			g.SetSourceColor(colorSerieB);
			g.Rectangle(xStart, topMargin -1.25*textHeight, boxWidth, boxWidth);
			g.FillPreserve();
			g.SetSourceColor(black);
			g.Stroke();

			//write 2nd variable
			xStart += 1.5*boxWidth;
			printText(xStart, topMargin -textHeight, 0, textHeight-2, variableSerieB, g, alignTypes.LEFT);
		} else
		{
			//1st row
			double rowWidth = 1.5*boxWidth + serieAWidth;
			double xStart = .5*graphWidth -.5*rowWidth;

			//paint 1st box
			g.SetSourceColor(colorSerieA);
			g.Rectangle(xStart, topMargin -1.25*textHeight, boxWidth, boxWidth);
			g.FillPreserve();
			g.SetSourceColor(black);
			g.Stroke();

			//write 1st variable
			xStart += 1.5*boxWidth;
			printText(xStart, topMargin -textHeight, 0, textHeight-2, variableSerieA, g, alignTypes.LEFT);

			//2nd row
			rowWidth = 1.5*boxWidth + serieBWidth;
			xStart = .5*graphWidth -.5*rowWidth;

			//paint 2nd box (1.25*textHeight below)
			g.SetSourceColor(colorSerieB);
			g.Rectangle(xStart, topMargin -1.25*textHeight +1.25*textHeight, boxWidth, boxWidth);
			g.FillPreserve();
			g.SetSourceColor(black);
			g.Stroke();

			//write 2nd variable
			xStart += 1.5*boxWidth;
			printText(xStart, topMargin -textHeight +1.25*textHeight, 0, textHeight-2, variableSerieB, g, alignTypes.LEFT);
		}

		g.SetFontSize(textHeight);
	}

	protected override void findMaximums()
	{
		foreach(List<PointF> p_l in barSecondary_ll)
			foreach(PointF p in p_l)
			{
				if(p.Y > maxY)
					maxY = p.Y;

				// minY on secondary only if p.Y is > 0 because if this bar does not exists (eg tc on a cmj, then will not be plotted
				if(p.Y > 0 && p.Y < minY)
					minY = p.Y;
			}

		bool first = true;
		foreach(PointF p in barMain_l)
		{
			if (p == null)
				continue; //needed check

			if (p.Y > maxY) //on ec at capturing if last is ecc, a con is send as null
				maxY = p.Y;
			if (p.Y < minY) //on ec at capturing if last is ecc, a con is send as null
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
		//maxX = barMain_l.Count + .5; //all barMain_l lists have same length
		maxX = barMain_l.Count + 1;

		//while capturing ecc-con, if last rep don is an ecc, it have to be drawn
		if(barSecondary_ll.Count == 1 && barSecondary_ll[0].Count > barMain_l.Count)
			maxX ++;

		//bars Y have 0 at bottom
		if (barsOrPoints == BarsOrPoints.BARS)
		{
			maxY += .1*maxY; //to accomodate texts above
			minY = 0;
		} else { // (barsOrPoints == BarsOrPoints.POINTS)
			separateMinYMaxYIfNeeded (ref minY, ref maxY);

			double personIconReal = Math.Abs (calculateRealY (0) - calculateRealY (24));
			maxY += 2 * personIconReal;	// *2 to ensure enough spacing
			minY -= 3.2 * personIconReal;	// *3 to ensure enough spacing & text below point
		}
	}

	//note pointA_l and pointB_l have same length
	protected override void plotBars ()
	{
		/* debug stuff
		LogB.Information("plotBars NH barMain_l.Count: " + barMain_l.Count.ToString());
		LogB.Information("plotBars NH barSecondary_ll.Count: " + barSecondary_ll.Count.ToString());
		LogB.Information("plotBars NH barSecondary_ll[0].Count: " + barSecondary_ll[0].Count.ToString());
		LogB.Information("plotBars NH names_l.Count: " + names_l.Count.ToString());
		*/

		//calculate separation between series and bar width
		/*
		   | LM |     graphWidthUsable    | RM |
		   | LM |     __   __ __   __     | RM |
		   | LM |  __|  | |  |  | |  |    | RM |
		   | LM | |  |  | |  |  | |  |__  | RM |
		   | LM |s|  |  |b|  |  |b|  |  |s| RM |

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
		double spaceBetweenBarsRatio = .5;

		/*
		 * on BARS divide graphWidhtUsable by total objects (bars, leftrightspace, spacesbetweenbars)
		   for 3 (double) bars on ratios 1, .5, .5, this will be 8
		   */
		int series = 2; 	// (barsOrPoints == BarsOrPoints.BARS)
		if (barsOrPoints == BarsOrPoints.POINTS)
			series = 1;

		barWidth = UtilAll.DivideSafe(graphWidthUsable,
			series * barMain_l.Count * barWidthRatio +
			2*sideWidthRatio + (barMain_l.Count-1) * spaceBetweenBarsRatio);
		double distanceBetweenCols = barWidth * spaceBetweenBarsRatio;

		resultFontHeight = getBarsResultFontHeight (barWidth);

		/* mouseLimits
		   if there are 6 bars, 6+6 bars should be 0..11,
		   one bar will go from 0 to 10 and the other from 1 to 11
		   note that this can be reversed according to mainAtLeft.
		   */
		int mouseLimitsPos1stBar = 0;
		int mouseLimitsPos2ndBar = 1;

		/*
		//debug
		LogB.Information("barMain_l:");
		for(int j = 0; j < barMain_l.Count; j ++)
			if(barMain_l[j] != null) //at ec capture, if last is ecc, a con is send as null
				LogB.Information(barMain_l[j].ToString());
		if(barSecondary_ll.Count == 1)
		{
			LogB.Information("barSecondary_ll[0]:");
			for(int j = 0; j < barSecondary_ll[0].Count; j ++)
				LogB.Information(barSecondary_ll[0][j].ToString());
		}
		LogB.Information("saved_l:");
		for(int j=0; j < saved_l.Count; j ++)
			LogB.Information(saved_l[j].ToString());
		*/

		Pixbuf pixbuf = Chronojump.MyPixbuf.Get (null, Util.GetImagePath(false) + "image_person_outline.png");

		//for video
		double timesSubtestPrevious = 0;
		double timesSubtestThis = 0;

		for(int i = 0; i < barMain_l.Count; i ++)
		{
			/*
			   need this to sort correctly, because tests are plotted from last to first (right to left),
			   so pB.Y result should have to be written first
			   */
			List<Point3F> resultOnBarsThisIteration_l = new List<Point3F>();
			List<Cairo.Color> colorOnBarsThisIteration_l = new List<Cairo.Color>();

			bool secondaryHasData = false;

			PointF pB = new PointF(0,0);
			if(barMain_l[i] == null) //on ec if we send a final ecc, con will be null
			{
				if(i < barSecondary_ll[0].Count && barSecondary_ll[0][i] != null)
					pB = new PointF(barSecondary_ll[0][i].X + .5, 0);
			}
			else
				pB = barMain_l[i];

			double spacesBetweenBarGroups = 0;
			if(i >= 1)
				spacesBetweenBarGroups = i*distanceBetweenCols;

			double x = leftMargin + sideWidthRatio*barWidth + i*series*barWidth + spacesBetweenBarGroups;
			double adjustXonBARS = 0; //this is used on second bar (at right), can be used on first if mainAtLeft

			// as all this class is thought for BARS, when use POINTS, just plot the point half bar to the right to be the same X
			double adjustXonPOINTS = UtilAll.DivideSafe (barWidth, 2.0);
			if (barsOrPoints == BarsOrPoints.BARS)
				adjustXonPOINTS = 0;

			//secondary bar: eg tc on jumps
			for(int j = 0; j < barSecondary_ll.Count; j ++)
			{
				PointF pS = barSecondary_ll[j][i];
				double y = 0;
				if(pS.Y > 0)
				{
					y = calculatePaintY(pS.Y);

					Cairo.Color barColor = colorSerieA;

					//only implemented for 1 secondary_l right now
					if(colorSecondary_l != null && colorSecondary_l.Count == barSecondary_ll[j].Count)
						barColor = colorSecondary_l[i];

					if (barsOrPoints == BarsOrPoints.BARS)
						drawRoundedRectangle (true, x + adjustXonBARS, y, barWidth, graphHeight -y -bottomMargin, 4, g, barColor,
								UtilList.FoundInListInt (best_l, i),
								UtilList.FoundInListInt (worst_l, i));
					else
						drawCircle (g, x + adjustXonPOINTS, y, POINTS_SIZE, black, barColor);

					resultOnBarsThisIteration_l.Add(new Point3F(x + adjustXonBARS + barWidth/2, y, pS.Y));
					colorOnBarsThisIteration_l.Add (barColor);
					//to print line variable if needed
					//barsXCenter_l.Add(x + adjustXonBARS + barWidth/2);

					if(labelBarMain != "")
					{
						if(labelRotateInFirstBar)
						{
							if(i == barSecondary_ll[j].Count -1)
							{
								g.SetSourceColor(white);
								int sep = 4;
								printTextRotated (x +adjustXonBARS +barWidth -sep, graphHeight -bottomMargin -sep, 0, textHeight+4, "Ecc", g, true);
								g.SetSourceColor(black);
							}
						}
						else
							printTextInBar(x +adjustXonBARS +barWidth/2, graphHeight -bottomMargin -10,
									0, textHeight+2, "e", g, true, false);
					}

					secondaryHasData = true;
					timesSubtestThis += pS.Y;
				}

				//mouse limits stuff
				if(pS.Y > 0) {
					if (barsOrPoints == BarsOrPoints.BARS)
						mouseLimits.AddInPos (mouseLimitsPos1stBar, x+adjustXonBARS, y, x+adjustXonBARS+barWidth, graphHeight -bottomMargin);
					else // (barsOrPoints == BarsOrPoints.POINTS)
						mouseLimits.AddInPos (mouseLimitsPos1stBar, x+adjustXonBARS, y-POINTS_SIZE, x+adjustXonBARS+barWidth, y+POINTS_SIZE);
				} else {
					//add it 0 width, to respect order when DJs are mixed with CMJs, but not be able to be selected (so same for BARS & POINTS)
					mouseLimits.AddInPos (mouseLimitsPos1stBar, x+adjustXonBARS, y, x+adjustXonBARS, graphHeight -bottomMargin);
				}
				mouseLimitsPos1stBar += 2;

				// on BARS need to manage side space for secondary bars. On POINTS this does not happen.
				if (barsOrPoints == BarsOrPoints.BARS)
					adjustXonBARS += barWidth;
			}

			//main bar: eg tv on jumps
			if(pB.Y > 0)
			{
				//if there is no data on previous variables, just put pB in the middle
				if (barsOrPoints == BarsOrPoints.BARS && ! secondaryHasData)
					adjustXonBARS = barWidth/2;

				double y = calculatePaintY(pB.Y);

				Cairo.Color barColor = colorSerieB;
				if(colorMain_l != null && colorMain_l.Count == barMain_l.Count)
					barColor = colorMain_l[i];

				if (barsOrPoints == BarsOrPoints.BARS)
					drawRoundedRectangle (true, x+adjustXonBARS, y, barWidth, graphHeight -y -bottomMargin, 4, g, barColor,
							UtilList.FoundInListInt (best_l, i),
							UtilList.FoundInListInt (worst_l, i));
				else
					drawCircle (g, x + adjustXonPOINTS, y, POINTS_SIZE, black, barColor);

				resultOnBarsThisIteration_l.Add(new Point3F(x + adjustXonBARS + barWidth/2, y, pB.Y));
				colorOnBarsThisIteration_l.Add (barColor);

				//add for the secondary and for the main bar, no problem both will work
				if (barsOrPoints == BarsOrPoints.BARS)
					mouseLimits.AddInPos (mouseLimitsPos2ndBar, x+adjustXonBARS, y, x+adjustXonBARS+barWidth, graphHeight -bottomMargin);
				else // (barsOrPoints == BarsOrPoints.POINTS)
					mouseLimits.AddInPos (mouseLimitsPos2ndBar, x+adjustXonBARS, y-POINTS_SIZE, x+adjustXonBARS+barWidth, y+POINTS_SIZE);


				//to print line variable if needed
				//barsXCenter_l.Add(x + adjustXonBARS + barWidth/2);

				if(labelBarMain != "")
				{
					if(labelRotateInFirstBar)
					{
						if(i == barMain_l.Count -1)
						{
							g.SetSourceColor(white);
							int sep = 4;
							printTextRotated (x +adjustXonBARS +barWidth -sep, graphHeight -bottomMargin -sep, 4, textHeight, "Con", g, true);
							g.SetSourceColor(black);
						}
					}
					else
						printTextInBar(x +adjustXonBARS +barWidth/2, graphHeight -bottomMargin -10,
								0, textHeight+2, "c", g, true, false);
				}

				//to show text centered at bottom correctly
				if (barsOrPoints == BarsOrPoints.BARS && ! secondaryHasData)
					adjustXonBARS = barWidth;

				timesSubtestThis += pB.Y;
			} else {
				//add the mouseLimits with empty width (so same for BARS & POINTS)
				mouseLimits.AddInPos (mouseLimitsPos2ndBar, x+adjustXonBARS, 0, x+adjustXonBARS, graphHeight -bottomMargin);
			}

			mouseLimitsPos2ndBar += 2;

			// this only works for sets of 2 values, but all the sets on POINTS are 2 values
			bool above = true;
			if (barsOrPoints == BarsOrPoints.POINTS &&
					resultOnBarsThisIteration_l.Count > 1 &&
					resultOnBarsThisIteration_l[0].Y > resultOnBarsThisIteration_l[1].Y)
				above = false;

			//sort result on bars correctly (this could be useful if mainAtLeft changes)
			for(int j = 0 ; j < resultOnBarsThisIteration_l.Count; j ++)
			{
				bool isSelected = false;
				if (selectedPos_l.Count > 0) // used on encoder reps
					isSelected = UtilList.FoundInListInt (selectedPos_l, i);
				else
					isSelected = i == selectedPos;

				barResult_l.Add (new BarResult (resultOnBarsThisIteration_l[j], isSelected, above, colorOnBarsThisIteration_l[j]));

				barsXCenter_l.Add(resultOnBarsThisIteration_l[j].X);

				above = ! above;
			}

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
				LogB.Information ("OOOOOO2");
				LogB.Information ("videoPlayTimeInSeconds", videoPlayTimeInSeconds);
				LogB.Information ("timesSubtestPrevious", timesSubtestPrevious);
				LogB.Information ("timesSubtestThis", timesSubtestThis);
				*/
				if (videoPlayTimeInSeconds >= timesSubtestPrevious &&
						videoPlayTimeInSeconds <= timesSubtestThis)
					videoPlayingStr = " playing";

				timesSubtestPrevious = timesSubtestThis;
			}

			//print text at bottom
			g.SetSourceColor (black);
			int textY = graphHeight - fontHeightForBottomNames * 2/3;
			if (bestPersonExHistoricalStr != "")
				textY -= bestPersonExHistoricalYpx;

			printTextMultiline(
					x +adjustXonBARS +adjustXonPOINTS, textY,
					0, fontHeightForBottomNames,
					names_l[i] + videoPlayingStr, g, alignTypes.CENTER,
					UtilList.FoundInListInt(saved_l, i));

			//draw personIcon if needed
			if (personIcon_l.Count == barMain_l.Count && personIcon_l[i])
			{
				//show on top of the highest bar (main or secondary)
				double ymax = pB.Y;
				for (int j = 0; j < barSecondary_ll.Count; j ++)
				{
					PointF pS = barSecondary_ll[j][i];
					if (pS.Y > ymax)
						ymax = pS.Y;
				}

				double y = calculatePaintY (ymax);
				double personIconY = y -1.5*resultFontHeight -24;	// above text, and 24px is pixbuf height
				if (barsOrPoints == BarsOrPoints.POINTS)
				{
					//personIconY = y +POINTS_SIZE; // above the point
					personIconY = calculatePaintY (minY) -24; // draw at bottom (to not interfere between main & secondary)
				}

				Gdk.CairoHelper.SetSourcePixbuf (g, pixbuf,
						x +adjustXonBARS +adjustXonPOINTS -12,  // -12 because pixbuf is 24 px
						personIconY);
				g.Paint();
			}
		}

		if (selectedDoubleDefined)
			selectedForBoxplot_l.Add (calculatePaintY (selectedDouble));
	}

	//done here and not in the constructor because most of this variables are known after construction
	public override void PassData2Series (List<PointF> barMain_l, List<List<PointF>> barSecondary_ll, bool mainAtLeft,
			List<Cairo.Color> colorMain_l, List<Cairo.Color> colorSecondary_l, List<string> names_l,
			string labelBarMain,// string labelBarSecondary,
			bool labelRotateInFirstBar,
			int fontHeightAboveBar, int fontHeightForBottomNames, int marginForBottomNames,
			string titleStr, List<int> best_l, List<int> worst_l,
			BarsOrPoints barsOrPoints)
	{
		this.barSecondary_ll = barSecondary_ll;
		this.barMain_l = barMain_l;
		this.colorMain_l = colorMain_l;
		this.colorSecondary_l = colorSecondary_l;
		this.names_l = names_l;
		this.labelBarMain = labelBarMain;
		//this.labelBarSecondary = labelBarSecondary;
		this.labelRotateInFirstBar = labelRotateInFirstBar;
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
		bottomMargin += marginForBottomNames;

		//LogB.Information(string.Format("NH GraphDo: pointA_l.Count: {0}, pointB_l.Count: {1}", pointA_l.Count, pointB_l.Count));
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
			drawGuides(colorSerieB);
		*/

		if (bestPersonExHistoricalStr != "")
			printBestPersonExHistorical ();

		g.SetSourceColor(black);

		selectedForBoxplot_l = new List <double> ();
		plotBars();

		drawBoxplots (colorSerieB);

		if(cairoBarsArrow != null)
			plotArrow();

		if (cbsld.data_l.Count > 0)
			plotAlternativeLine (cbsld);

		if(eccOverload_l.Count > 0)
			plotEccOverload();

		plotResultsOnBar();

		writeTitleAtTop ();

		if(maxIntersession > 0)
			writePersonsBest (); //encoder !relativeToSet

		if(showLegend)
			writeLegend ();

		addClickableMarkIfNeeded (clickable, g);

		if (screenshotURL != "")
			CairoUtil.GetScreenshotFromDrawingArea (area, g, screenshotURL);

		endGraphDisposing(g, surface, area.Window);
	}
}
