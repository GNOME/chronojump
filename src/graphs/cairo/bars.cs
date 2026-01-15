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

public abstract class CairoBars : CairoGeneric
{
	public enum Type { NORMAL, ENCODER };
	protected Type type;
	public enum BarsOrPoints { BARS, POINTS };
	protected const int POINTS_SIZE = 10;
	public BarsOrPoints barsOrPoints;

	protected DrawingArea area;
	protected ImageSurface surface;

	protected int fontHeightAboveBar; //will be reduced if does not fit. On encoder is bigger than other places, pass -1 if don't want to define
	protected int fontHeightForBottomNames;
	protected int marginForBottomNames;
	protected MouseClickable clickable;
	protected bool paintAxis;
	protected bool paintGrid; //if paint grid, then paint a rectangle below barResult (on encoder: false)

	protected string titleStr;
	protected List<int> best_l;
	protected List<int> worst_l;
	//3 encoder title variales
	protected string lossStr; //loss in grey
	protected string workStr;
	protected string impulseStr;
	protected bool encoderTitle; //boolean meaning previous variables are used, SetEncoderTitle() has been called

	//protected string jumpType;
	//protected string runType;
	protected string date;
	protected Cairo.Color colorSerieA;
	protected CairoBarsGuideManage cairoBarsGuideManage;
	protected bool usePersonGuides;
	protected bool useGroupGuides;
	protected CairoBarsArrow cairoBarsArrow;
	protected Boxplot boxplotPerson;
	protected Boxplot boxplotSession;
	protected List<double> selectedForBoxplot_l; // on graph units

	protected Cairo.Context g;
	protected int lineWidthDefault = 1; //was 2;
	protected List<double> barsXCenter_l; //store center of the bars to draw range pointline and lossArrow on encoder
	protected List<BarResult> barResult_l;
	protected int resultFontHeight;
	protected double barWidth;

	protected double minX = 1000000;
	protected double maxX = 0;
	protected double minY = 1000000;
	protected double maxY = 0;

	protected double maxYForBoxplotShadow;
	protected double minYForBoxplotShadow;

	protected Cairo.Color black;
	protected Cairo.Color gray99;
	protected Cairo.Color gray153; //light
	protected Cairo.Color gray180; //lighter
	protected Cairo.Color white;
	protected Cairo.Color greenDark;
	protected Cairo.Color blue;
	//protected Cairo.Color blueChronojump;
	//protected Cairo.Color bluePlots;
	//protected Cairo.Color yellow;

	protected RepetitionMouseLimits mouseLimits;
	protected List<int> id_l; //to pass the uniqueID of some test, eg: RunInterval executions and then find it using mouseLimits
	protected int selectedPos;
	protected double selectedDouble;
	protected bool selectedDoubleDefined;
	protected List<int> selectedPos_l; // encoder reps
	protected List<double> selectedDouble_l; // encoder reps
	protected List<double> color_l;
	protected List<bool> personIcon_l;

	protected CairoBarsSecondaryLineData cbsld; //related to secondary variable (by default range)

	protected List<CairoBarsArrow> eccOverload_l;
	protected bool eccOverloadWriteValue;
	protected List<int> saved_l;
	protected double maxIntersession;
	protected Preferences.EncoderRepetitionCriteria maxIntersessionEcconCriteria;
	protected string maxIntersessionValueStr; //with correct decimals and units
	protected string maxIntersessionDate;

	protected int bestPersonExHistoricalYpx = 30; // px reserved at bottom if bestPersonExHistoricalStr != ""
	protected double bestPersonExHistoricalD;
	protected string bestPersonExHistoricalStr;

	// ---- values can be passed from outside via accessors ---->
	protected string xVariable = "";
	protected string yVariable = "Height";
	protected string xUnits = "";
	protected string yUnits = "cm";
	//protected List<int> inBarNums_l; //currently unused
	protected List<int> edgeBarNums_l; //used on Wichro to identify photocells
	protected bool spaceBetweenBars;
	protected double videoPlayTimeInSeconds;
	protected List<double> videoPlayTimes_l; //for runInterval (because passed speeds and need times for video)
	protected string screenshotURL;

	//used when there are two series (for legend)
	protected string variableSerieA = "";
	protected string variableSerieB = "";
	protected int decs;
	// <---- end of passed variables

	public virtual void GraphInit (string font, bool usePersonGuides, bool useGroupGuides) //needed to set rightMargin
	{
		this.usePersonGuides = usePersonGuides;
		this.useGroupGuides = useGroupGuides;

		textHeight = 14;
		decs = 2;
		initGraph(font, 1); //.8 if writeTextAtRight
		barsXCenter_l = new List<double>();
		barResult_l = new List<BarResult> ();
		//inBarNums_l = new List<int>();
		edgeBarNums_l = new List<int>();
		encoderTitle = false;
		selectedPos = -1;
		selectedDouble = -1;
		selectedDoubleDefined = false;
		selectedPos_l = new List<int> ();
		selectedDouble_l = new List<double> ();
		screenshotURL = "";
	}

	public void PassGuidesData (CairoBarsGuideManage cairoBarsGuideManage)
	{
		this.cairoBarsGuideManage = cairoBarsGuideManage;
	}

	public void PassArrowData (CairoBarsArrow cairoBarsArrow)
	{
		this.cairoBarsArrow = cairoBarsArrow;
	}

	protected void drawPersonIcon (int xStart)
	{
		Gdk.Pixbuf pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "image_person_outline.png");
		Gdk.CairoHelper.SetSourcePixbuf (g, pixbuf, graphWidth -rightMargin +xStart, topMargin -24);
		g.Paint();
	}
	protected void drawGroupIcon (int xStart)
	{
		Gdk.Pixbuf pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + "image_group_outline.png");
		Gdk.CairoHelper.SetSourcePixbuf (g, pixbuf, graphWidth -rightMargin +xStart, topMargin -24);
		g.Paint();
	}

	public void PassBoxplots (Boxplot boxplotPerson, Boxplot boxplotSession)
	{
		this.boxplotPerson = boxplotPerson;
		this.boxplotSession = boxplotSession;
	}

	protected void drawBoxplots (Cairo.Color color)
	{
		//g.SetSourceColor (color);

		int xStart = 6;

		if(usePersonGuides)
			drawBoxplot (boxplotPerson, xStart+6, color);

		if(usePersonGuides && useGroupGuides)
			xStart += (24 + 8);

		if(useGroupGuides)
			drawBoxplot (boxplotSession, xStart+6, color);

		// drawn here because if done above then boxplots are not drawn
		if (boxplotPerson != null && boxplotPerson.Calculated)
			drawPersonIcon (6);

		if (boxplotSession != null && boxplotSession.Calculated)
			drawGroupIcon (xStart);
	}

	protected void drawBoxplot (Boxplot bp, int xStart, Cairo.Color color)
	{
		if (bp == null || ! bp.Calculated)
			return;

		int width = 8;
		//LogB.Information ("bp: " + bp.ToString ());

		g.LineWidth = 1;
		int x = graphWidth - rightMargin +xStart +6;

		// shadowed rectangle showing area of coverage of the boxplot
		g.SetSourceColor (gray180); //to have contrast with the bar
		g.Rectangle (x-width, calculatePaintY (maxYForBoxplotShadow),
				2*width, calculatePaintY (minYForBoxplotShadow) - calculatePaintY (maxYForBoxplotShadow));
		g.Fill ();

		g.SetSourceColor (color);
		// iqr rectangle
		g.Rectangle (x-width, calculatePaintY (bp.Quartiles.Item3),
				2*width, calculatePaintY (bp.Quartiles.Item1) -calculatePaintY (bp.Quartiles.Item3));
		// median
		g.MoveTo (x-width, calculatePaintY (bp.Quartiles.Item2));
		g.LineTo (x+width, calculatePaintY (bp.Quartiles.Item2));
		g.Stroke ();

		// top quartile
		g.MoveTo (x-width, calculatePaintY (bp.MaxAccepted));
		g.LineTo (x+width, calculatePaintY (bp.MaxAccepted));
		g.MoveTo (x, calculatePaintY (bp.MaxAccepted));
		g.LineTo (x, calculatePaintY (bp.Quartiles.Item3));
		g.Stroke ();

		// bottom quartile
		g.MoveTo (x-width, calculatePaintY (bp.MinAccepted));
		g.LineTo (x+width, calculatePaintY (bp.MinAccepted));
		g.MoveTo (x, calculatePaintY (bp.Quartiles.Item1));
		g.LineTo (x, calculatePaintY (bp.MinAccepted));
		g.Stroke ();

		// mean
		g.Rectangle (x-3, calculatePaintY (bp.Average) -3, 6, 6);
		g.Fill ();

		// outliers
		foreach (double d in bp.Outlier_l)
		{
			g.MoveTo (x +4, calculatePaintY (d));
			g.Arc(x, calculatePaintY (d), 4, 0.0, 2.0 * Math.PI); //full circle
			g.Stroke();
		}

		foreach (double d in selectedForBoxplot_l)
		{
			g.SetSourceColor(yellow); //to have contrast with the bar
			g.MoveTo (x, d-4);
			g.RelLineTo (4, 4);
			g.RelLineTo (-4, 4);
			g.RelLineTo (-4, -4);
			g.ClosePath ();
			g.FillPreserve ();
			g.SetSourceColor(black);
			g.Stroke ();
		}
	}


	public virtual void PassData1Serie (List<PointF> barMain_l,
			List<Cairo.Color> colorMain_l, List<string> names_l,
			List<List<double>> interval_l,
			int fontHeightAboveBar, int fontHeightForBottomNames, int marginForBottomNames,
			string titleStr, List<int> best_l, List<int> worst_l,
			BarsOrPoints barsOrPoints)
	{
		//defined in CairoBars1Series
	}

	public virtual void PassData2Series (List<PointF> barMain_l, List<List<PointF>> barSecondary_ll, bool mainAtLeft,
			List<Cairo.Color> colorMain_l, List<Cairo.Color> colorSecondary_l, List<string> names_l,
			string labelBarMain,// string labelBarSecondary,
			bool labelRotateInFirstBar,
			int fontHeightAboveBar, int fontHeightForBottomNames, int marginForBottomNames,
			string titleStr, List<int> best_l, List<int> worst_l,
			BarsOrPoints barsOrPoints)
	{
		//defined in CairoBarsNHSeries
	}

	public abstract void GraphDo();

	protected void initGraph(string font, double widthPercent1)
	{
		initGraph(font, widthPercent1, true);
	}
	protected void initGraph(string font, double widthPercent1, bool clearDrawingArea)
	{
		this.font = font;

		//LogB.Information("Font: " + font);

		//1 create context from area->surface (see xy.cs)
                surface = new ImageSurface(Format.RGB24, area.Allocation.Width, area.Allocation.Height);
                g = new Context (surface);

		if(clearDrawingArea)
		{
			//2 clear DrawingArea (white)
			g.SetSourceRGB(1,1,1);
			g.Paint();
		}

		graphWidth = Convert.ToInt32(area.Allocation.Width * widthPercent1);
		graphHeight = area.Allocation.Height;
		//LogB.Information(string.Format("graphWidth: {0}, graphHeight: {1}", graphWidth, graphHeight));

		g.SetSourceRGB(0,0,0);
		g.LineWidth = lineWidthDefault;

		//4 prepare font
		g.SelectFontFace(font, Cairo.FontSlant.Normal, Cairo.FontWeight.Normal);
		g.SetFontSize(textHeight);

		black = colorFromRGB(0,0,0);
		gray99 = colorFromRGB(99,99,99);
		gray153 = colorFromRGB(153,153,153);
		gray180 = colorFromRGB(180,180,180);
		white = colorFromRGB(255,255,255);
		greenDark = colorFromRGB(0,140,0);
		blue = colorFromRGB(178, 223, 238); //lightblue
		//blueChronojump = colorFromRGB(14, 30, 70);
		//bluePlots = colorFromRGB(0, 0, 200);
		yellow = colorFromRGB(255,204,1);
		//yellowLight = colorFromRGB(243,222,140); //f3de8c
		yellowDark = colorFromRGB(205,205,0);

		//margins
		leftRightMarginsSet();
		bottomMargin = 9;
		topMarginSet ();

		mouseLimits = new RepetitionMouseLimits();
		id_l = new List<int>();
		color_l = new List<double>();
		personIcon_l = new List<bool>();

		cbsld = new CairoBarsSecondaryLineData ();

		eccOverload_l = new List<CairoBarsArrow>();
		eccOverloadWriteValue = false;
		saved_l = new List<int>();
		maxIntersession = 0;
		maxIntersessionValueStr = "";
		maxIntersessionDate = "";
		bestPersonExHistoricalD = 0;
		bestPersonExHistoricalStr = "";
	}

	private void leftRightMarginsSet ()
	{
		if(type == Type.ENCODER)
		{
			//to just show the mice icon
			leftMargin = 18;
			rightMargin = 18;
		}
		else {
			leftMargin = 26;
			rightMargin = 42; //images are 24 px, separate 6 px from grapharea, and 12 px from absoluteright
		}

		if(usePersonGuides && useGroupGuides)
			rightMargin = 70;
	}

	protected abstract void topMarginSet ();
	protected abstract void findMaximums(); //includes point and guides

	protected void paintAxisDo (int width)
	{
		g.LineWidth = width;
		g.MoveTo(leftMargin, topMargin);
		g.LineTo(leftMargin, graphHeight - bottomMargin);
		g.LineTo(graphWidth - rightMargin, graphHeight - bottomMargin);
		g.Stroke ();

		printText(2, topMargin -textHeight, 0, textHeight -2, getYAxisLabel(), g, alignTypes.LEFT);
		printXAxisText();
		g.Stroke ();

		g.LineWidth = lineWidthDefault;
	}

	//this combined with paintVerticalGridLine is different on RaceAnalyzer
	protected virtual void printXAxisText()
	{
		printText(graphWidth -rightMargin +6, graphHeight -bottomMargin -2, 0, textHeight,
				getXAxisLabel(), g, alignTypes.LEFT);
	}

	protected string getXAxisLabel()
	{
		return getAxisLabel(xVariable, xUnits);
	}
	protected string getYAxisLabel()
	{
		return getAxisLabel(yVariable, yUnits);
	}
	private string getAxisLabel(string variable, string units)
	{
		if(units == "")
			return variable;
		return string.Format("{0} ({1})", variable, units);
	}

	//TODO: check if for one value this is /0
	protected override double calculatePaintX (double realX)
	{
		return leftMargin + (realX - minX) * UtilAll.DivideSafe(
				graphWidth - (leftMargin + rightMargin),
				maxX - minX);
        }
	protected override double calculatePaintY (double realY)
	{
                return graphHeight - (topMargin + bottomMargin) //graph data area
			- UtilAll.DivideSafe(
				(realY - minY) * (graphHeight - (topMargin+bottomMargin)),
				//maxY - minY)
				//have 20% extra margin on the top (highest values will be this % far from max of the graph, needed also because text is above)
				//1.2*maxY - minY)
				//1.1*maxY - minY)
				maxY - minY)
			+ topMargin;
        }

	//used for plotAlternative (that uses another series, so pass maxY and minY
	//percentOnTop recommended is 1.2 to have 20% extra margin on the top (highest values will be this % far from max of the graph, needed also because text is above)
	//if do not want percentOnTop, pass 1
	protected double calculatePaintY (double realY, double maxY, double minY, double percentOnTop)
	{
                return graphHeight - (topMargin + bottomMargin) //graph data area
			- UtilAll.DivideSafe(
				(realY - minY) * (graphHeight - (topMargin+bottomMargin)),
				//maxY - minY)
				//have 20% extra margin on the top (highest values will be this % far from max of the graph, needed also because text is above)
				percentOnTop*maxY - minY)
			+ topMargin;
	}

	protected double calculateRealY (double graphY)
	{
		return minY - UtilAll.DivideSafe (
				(graphY - graphHeight + topMargin) * (maxY - minY),
				graphHeight - (bottomMargin + topMargin)
				);
	}

	protected override void printText (double x, double y, double heightUnused, int textH,
			string text, Cairo.Context g, alignTypes align)
	{
		g.SetFontSize(textH);

		double moveToLeft = 0;
		if(align == alignTypes.CENTER || align == alignTypes.RIGHT)
		{
			Cairo.TextExtents te = g.TextExtents(text);
			
			if(align == alignTypes.CENTER)
				moveToLeft = te.Width/2;
			else
				moveToLeft = te.Width;
		}

		g.MoveTo(x - moveToLeft, Convert.ToInt32 (y + textH/2)); //y as int on dotnetgtk3 because on windows top row of text is sometimes not shown if double
		g.ShowText(text);

		//restore text size
		g.SetFontSize(textHeight);
	}

	protected void printTextInBar (double x, double y, double heightUnused, int textH,
			string text, Cairo.Context g, bool bold, bool inRectangle)
	{
		g.Save();
		g.SetFontSize(textHeight+4);
		if(bold)
			g.SelectFontFace(font, Cairo.FontSlant.Normal, Cairo.FontWeight.Bold);

		if(inRectangle)
		{
			g.SetSourceColor(black);
			drawRectangleAroundText (x, y, textH, text, g, colorSerieA);
		}

		g.SetSourceColor(white);
		printText (x, y, heightUnused, textH, text, g, alignTypes.CENTER);
		g.Restore();
	}

	protected void printTextRotated (double x, double y, double heightUnused, int textH,
			string text, Cairo.Context g, bool bold)
	{
		g.Save();

		g.SetFontSize (textH);
		if(bold)
			g.SelectFontFace(font, Cairo.FontSlant.Normal, Cairo.FontWeight.Bold);

		g.MoveTo(x, y);
		g.Rotate(MathCJ.ToRadians(-90));

		g.ShowText(text);

		g.Restore();

		//restore text size
		//g.SetFontSize(textHeight);
	}

	//text could have one or more \n
	protected void printTextMultiline (double x, double y, double heightUnused, int textH,
			string text, Cairo.Context g, alignTypes align, bool inRectangle) //inRectangle is used on encoder to indicate it is a saved repetition
	{
		if(text == "")
			return;

		//draw rectangle first as it will be in the back
		if(inRectangle)
		{
			drawRectangleAroundText (x, y, textH, text, g, yellowMid);
			g.SetSourceColor (black);
		}

		string [] strFull = text.Split(new char[] {'\n'});

		//reversed to ensure last line is in the bottom
		for (int i = strFull.Length -1; i >= 0; i --)
		{
			printText (x, y, heightUnused, textH, strFull[i], g, align);
			y -= 1.1 * textH;
		}
	}

	protected abstract void plotBars ();

	protected void plotArrow ()
	{
		//caution
		if(cairoBarsArrow == null || barsXCenter_l == null ||
				cairoBarsArrow.x0pos >= barsXCenter_l.Count ||
				cairoBarsArrow.x1pos >= barsXCenter_l.Count)
			return;

		plotArrowFree (g, gray153, 5, 20, true,
				cairoBarsArrow.GetX0Graph (barsXCenter_l),
				calculatePaintY(cairoBarsArrow.y0),
				cairoBarsArrow.GetX1Graph (barsXCenter_l),
				calculatePaintY(cairoBarsArrow.y1));
	}

	//same as above but as a list
	protected virtual void plotEccOverload ()
	{
		//caution
		if(eccOverload_l == null || barsXCenter_l == null)
			return;

		g.SetSourceColor (greenDark);
		foreach(CairoBarsArrow cba in eccOverload_l)
		{
			LogB.Information("eccOverload: " + cba.ToString());

			if(cba.x0pos >= barsXCenter_l.Count ||
					cba.x1pos >= barsXCenter_l.Count)
				continue;

			plotArrowFree (g, greenDark, 3, 14, true,
					cba.GetX0Graph (barsXCenter_l),
					calculatePaintY(cba.y0),
					cba.GetX1Graph (barsXCenter_l),
					calculatePaintY(cba.y1));

			if(eccOverloadWriteValue)
				printText((cba.GetX0Graph (barsXCenter_l) + cba.GetX1Graph(barsXCenter_l))/2,
						//same height aprox than values (non clear if overload has 3 digits)
						//calculatePaintY(cba.y1) -1.5*resultFontHeight + resultFontHeight/2,
						//up the bar values, ok, but maybe better all on same Y
						//calculatePaintY(cba.y1) -2*resultFontHeight,
						2*textHeight,
						0, resultFontHeight,
						Util.TrimDecimals(100.0 * UtilAll.DivideSafe(cba.y1 - cba.y0, cba.y0), 0) + "%",
						g, alignTypes.CENTER);
		}
		g.SetSourceColor (black);
	}

	protected void plotEdgeBarNums ()
	{
		if (edgeBarNums_l.Count == 0)
			return;

		int eCount = 0; //edgeBarNums_l

		//1st edgeBarNums is the beginning (at left of first photocell)
		if(edgeBarNums_l.Count == barsXCenter_l.Count +1)
		{
			if (edgeBarNums_l[0] >= 0) //not show the non-Wichro -1s
				printTextInBar(barsXCenter_l[0] -barWidth/2, graphHeight -bottomMargin -10,
						0, textHeight+2, edgeBarNums_l[0].ToString(), g, true, true);
			eCount = 1;
		}

		for(int bCount = 0; bCount < barsXCenter_l.Count && eCount < edgeBarNums_l.Count; bCount ++, eCount ++)
			if (edgeBarNums_l[eCount] >= 0) //not show the non-Wichro -1s
				printTextInBar(barsXCenter_l[bCount] +barWidth/2, graphHeight -bottomMargin -10,
						0, textHeight+2, edgeBarNums_l[eCount].ToString(), g, true, true);
	}


	protected void plotAlternativeLine (CairoBarsSecondaryLineData sld)
	{
		//be safe
		if(barsXCenter_l.Count != sld.data_l.Count)
			return;

		//g.SetSourceColor (yellow); //to have contrast with the bar
		g.SetSourceColor (caramel);

		if (sld.yMin < 0 && sld.yMax < 0) //means detect y range automatically
		{
			sld.yMax = MathUtil.GetMax (sld.data_l);
			sld.yMin = 0; //or MathUtil.GetMin (sld.data_l)
		}

		// 1) lines
		bool firstDone = false;
		for (int i = 0 ; i < barsXCenter_l.Count; i ++)
		{
			double y = calculatePaintY (sld.data_l[i], sld.yMax, sld.yMin, 1.1);

			if(! firstDone)
			{
				g.MoveTo(barsXCenter_l[i], y);
				firstDone = true;
			} else
				g.LineTo(barsXCenter_l[i], y);
		}
		g.Stroke();

		// 2) points
		int pointsRadius = 5;
		for (int i = 0 ; i < barsXCenter_l.Count; i ++)
		{
			double y = calculatePaintY (sld.data_l[i], sld.yMax, sld.yMin, 1.1);

			g.SetSourceColor (brown);
			g.Arc(barsXCenter_l[i], y, pointsRadius, 0.0, 2.0 * Math.PI); //full circle
			g.FillPreserve();
			g.SetSourceColor (white);
			g.Stroke();
		}
		g.SetSourceColor (caramel);

		// 3) axis
		double yMaxPaint = calculatePaintY (sld.yMax, sld.yMax, sld.yMin, 1.1);
		double yMinPaint = calculatePaintY (sld.yMin, sld.yMax, sld.yMin, 1.1);

		g.MoveTo (leftMargin +4, yMinPaint);
		g.LineTo (leftMargin, yMinPaint);
		g.LineTo (leftMargin, yMaxPaint);
		g.LineTo (leftMargin +4, yMaxPaint);
		g.Stroke ();

		string str = sld.magnitude + " (" + sld.units + ")";
		Cairo.TextExtents te = g.TextExtents (str);
		printTextRotated (leftMargin -4, (yMaxPaint + yMinPaint)/2 +te.Width/2, 0, textHeight -2,
				str, g, false);

		str = Util.TrimDecimals (sld.yMin, 2);
		te = g.TextExtents (str);
		printTextRotated (leftMargin -4, yMinPaint +te.Width/2, 0, textHeight -2,
				str, g, false);

		str = Util.TrimDecimals (sld.yMax, 2);
		te = g.TextExtents (str);
		printTextRotated (leftMargin -4, yMaxPaint +te.Width/2, 0, textHeight -2,
				str, g, false);

		g.Stroke ();

		// 4) default g values
		g.SetSourceColor(black);
	}

	//adapted from http://www.mono-project.com/docs/tools+libraries/libraries/Mono.Cairo/cookbook/
	//bottomFlat means to have rounded only on top
	protected static void drawRoundedRectangle (bool bottomFlat,
			double x, double y, double width, double height, 
			double radius, Cairo.Context g, Cairo.Color color,
			bool bestValue, bool worstValue)
	{
		g.Save ();

		//manage negative widths
		if(width < 0)
		{
			x += width; //it will shift to the left (width is negative)
			width *= -1;
		}

		if ((radius > height / 2) || (radius > width / 2))
			radius = min (height / 2, width / 2);

		g.MoveTo (x, y + radius);
		g.Arc (x + radius, y + radius, radius, Math.PI, -Math.PI / 2);
		g.LineTo (x + width - radius, y);
		g.Arc (x + width - radius, y + radius, radius, -Math.PI / 2, 0);

		if(bottomFlat)
		{
			g.LineTo (x + width, y + height);
			g.LineTo (x, y + height);
		} else {
			g.LineTo (x + width, y + height - radius);
			g.Arc (x + width - radius, y + height - radius, radius, 0, Math.PI / 2);
			g.LineTo (x + radius, y + height);
			g.Arc (x + radius, y + height - radius, radius, Math.PI / 2, Math.PI);
		}

		g.ClosePath ();

		g.SetSourceRGB(color.R, color.G, color.B);
		g.FillPreserve ();

		if (bestValue)
		{
			g.SetSourceRGB (1,.8,0); //yellow
			g.LineWidth = 4;
		}
		else if (worstValue)
		{
			g.SetSourceRGB (.28,.14,.06); //brownish
			g.LineWidth = 4;
		}
		else
		{
			g.SetSourceRGB(0, 0, 0);
			g.LineWidth = 1;
		}

		g.Stroke ();
		g.Restore ();
	}
	private static double min (params double[] arr)
	{
		int minp = 0;
		for (int i = 1; i < arr.Length; i++)
			if (arr[i] < arr[minp])
				minp = i;

		return arr[minp];
	}

	public int GetFontForBottomNames (List<Event> events, string longestWord)
	{
		// 1) set marginBetweenTexts to 1.0 character
		Cairo.TextExtents te = g.TextExtents("A");
		double marginBetweenTexts = 2.0 * te.Width; //1.0 creates overlaps. This fixes #1107

		// 2) find longestWord width
		te = g.TextExtents(longestWord);
		//LogB.Information (string.Format ("longestWord: {0} te.Width: {1}", longestWord, te.Width));

		// 3) if longestWord * events.Count does not fit, iterate to find correct font size
		int optimalFontHeight = textHeight;
		for (int i = textHeight; events.Count * (te.Width + marginBetweenTexts) > (graphWidth -leftMargin -rightMargin) && i > 0; i --)
		{
			g.SetFontSize(i);
			te = g.TextExtents(longestWord);
			optimalFontHeight = i;
		}

		g.SetFontSize(textHeight); //return font to its default value
		return optimalFontHeight;
	}

	public int GetBottomMarginForText (int maxRows, int fontHeight)
	{
		g.SetFontSize(fontHeight);
		Cairo.TextExtents te = g.TextExtents("A");
		/*
		LogB.Information(string.Format("GetBottomMarginForText, maxRows: {0}, fontHeight: {1}, result: {2}",
					maxRows, fontHeight, Convert.ToInt32(1.3 * te.Height * maxRows)));
					*/

		return Convert.ToInt32(1.3 * te.Height * maxRows);
	}

	protected int getBarsResultFontHeight (double maxWidth)
	{
		double maxLengthNumber = 9.99;
		if(maxY >= 10)
			maxLengthNumber = 99.99;
		if(maxY >= 100)
			maxLengthNumber = 999.99;
		if(maxY >= 1000)
			maxLengthNumber = 9999.99;

		Cairo.TextExtents te;
		te = g.TextExtents(Util.TrimDecimals(maxLengthNumber, decs));

		//fix if label is wider than bar
		int optimalFontHeight = textHeight;
		if(fontHeightAboveBar >= 0)
			optimalFontHeight = fontHeightAboveBar;

		int i = optimalFontHeight;
		for(i = optimalFontHeight; te.Width >= maxWidth && i > 0; i --)
		{
			g.SetFontSize(i);
			te = g.TextExtents(Util.TrimDecimals(maxLengthNumber, decs));
		}

		g.SetFontSize(textHeight); //return font to its default value
		return i;
	}

	protected void plotResultsOnBar ()
	{
		//result on bar painted here (after bars) to not have text overlapped by bars
		double pAyStart = -1;
		foreach(BarResult barResult in barResult_l)
			pAyStart = plotResultOnBarDo (barResult.p.X, barResult.p.Y, graphHeight -bottomMargin,
					barResult.p.Z, pAyStart, barResult.above, barResult.color, barResult.selected);
	}

	protected double plotResultOnBarDo (double x, double y, double alto,
			double result, double yStartPointA, bool above, Cairo.Color color, bool isSelected)
	{
		g.SetFontSize(resultFontHeight);

		/*
		double maxLengthNumber = 9.99;
		if(maxY >= 10)
			maxLengthNumber = 99.99;
		if(maxY >= 100)
			maxLengthNumber = 999.99;
		if(maxY >= 1000)
			maxLengthNumber = 9999.99;
		*/

		Cairo.TextExtents te;
		te = g.TextExtents(Util.TrimDecimals(result,decs));
		//te = g.TextExtents(maxLengthNumber.ToString());

		bool textAboveBar = true;
		/*
		 * text and surrounding rect are in the middle of bar
		 * if bar is so small, then text and rect will not be fully shown
		 * for this reason, show rect and bar in a higher position
		 * use 2*lHeight in order to accomodate "Simulated" message below
		 */
		double yStart = (y+alto)/2 - te.Height/2;
		if(textAboveBar)
		{
			//print the result at top of the bar (better because there is the X grid and in the middle of the bar is confusing)
			yStart = y - 1.5*te.Height;
			if (barsOrPoints == BarsOrPoints.POINTS)
			{
				if (above)
					yStart -= 8; //move up to not be on point
				else
					yStart += 3 * POINTS_SIZE +2; //move up to not be on point
			}
		}

		/*
		   Do not move the bar above to fix overlappings as it's very ugly
		//check if there's an overlap with pointA)
		if ( yStartPointA >= 0 && te.Width >= barWidth &&
				( yStart >= yStartPointA && yStart <= yStartPointA + te.Height ||
				  yStart <= yStartPointA && yStart + te.Height >= yStartPointA ) )
			yStart = yStartPointA - 1.1 * te.Height;

		LogB.Information(string.Format("y: {0}, alto: {1}, yStart: {2}", y, alto, yStart));

		if( (yStart + te.Height) > alto )
			yStart = alto - te.Height;
		*/

		if(paintGrid)
		{
			if(textAboveBar)
				g.SetSourceColor(white); //to just hide the horizontal grid
			else
				g.SetSourceColor(yellow); //to have contrast with the bar

			//g.Rectangle(x - te.Width/2 -1, yStart-1, te.Width +2, te.Height+2);
			g.Rectangle(x - te.Width/2, yStart, te.Width, te.Height);
			g.Fill();
		}

		if (isSelected)
		{
			// 2.5.2 yellow rectangle (like on encoder set barplot and signal graph)
			g.SetSourceColor (yellowMid);
			g.Rectangle (x - te.Width/2 -1, yStart -1, te.Width +2, te.Height +4); //+4 (from -1 to +2) to accomodate the comma
			g.Fill();
		}

		g.SetSourceColor (color);

		//write text
		printText(x, yStart+te.Height/2, 0, resultFontHeight,
			Util.TrimDecimals(result, decs), g, alignTypes.CENTER);

		//put font size to default value again
		g.SetFontSize(textHeight);

		return yStart;
	}

	protected void writeTitleAtTop()
	{
		if(encoderTitle)
			writeTitleAtTopEncoder ();
		else
			printText(graphWidth/2 + leftMargin, textHeight/2, 0, textHeight+2,
					titleStr, g, alignTypes.CENTER);
	}

	int titleTextHeight;
	protected void writeTitleAtTopEncoder()
	{
		g.Save();

		//have title and titleFull to be able to position all perfectly but having two pens (colors)
		string titleFull = titleStr + lossStr + workStr + impulseStr;

		// 1) get the titleTextHeight for titleFull
		titleTextHeight = textHeight +2;
		g.SetFontSize(titleTextHeight);
		g.SelectFontFace(font, Cairo.FontSlant.Normal, Cairo.FontWeight.Bold);
		Cairo.TextExtents te = g.TextExtents(titleFull);

		if (te.Width > graphWidth) //margins?
		{
			do {
				titleTextHeight --;
				if(titleTextHeight <= 1)
				{
					titleTextHeight = 1;
					g.SetFontSize(titleTextHeight);
					te = g.TextExtents(titleFull);
					break;
				}
				g.SetFontSize(titleTextHeight);
				te = g.TextExtents(titleFull);
			} while (te.Width > graphWidth); //margins?
		}
		double titleFullWidth = te.Width;
		//g.SetFontSize(titleTextHeight);

		// 2) get the width to paint each string at its position
		//double titleWidth = (g.TextExtents(titleStr)).Width;
		double titleWidth = (g.TextExtents(titleStr)).XAdvance; //used this becuase the ending whitespace is not used on Width calculation
		double lossWidth = (g.TextExtents(lossStr)).Width;
		double workWidth = (g.TextExtents(workStr)).Width;
		//double impulseWidth = (g.TextExtents(impulseStr)).Width;

		// 3) paint title, loss, work, impulse
		g.SetSourceColor(black);
		printText(graphWidth/2 -titleFullWidth/2, textHeight/3, 0, titleTextHeight,
				titleStr, g, alignTypes.LEFT);

		if(lossStr != "")
		{
			g.SetSourceColor(gray99); //darker than the arrow line
			printText(graphWidth/2 -titleFullWidth/2 + titleWidth, textHeight/3, 0, titleTextHeight,
					lossStr, g, alignTypes.LEFT);
			g.SetSourceColor(black);
		}

		printText(graphWidth/2 -titleFullWidth/2 + titleWidth +lossWidth, textHeight/3, 0, titleTextHeight,
				workStr, g, alignTypes.LEFT);

		printText(graphWidth/2 -titleFullWidth/2 + titleWidth +lossWidth +workWidth, textHeight/3, 0, titleTextHeight,
				impulseStr, g, alignTypes.LEFT);

		g.Restore();
	}

	// this is the call when there is data
	protected void printBestPersonExHistorical ()
	{
		printBestPersonExHistorical (bestPersonExHistoricalD, bestPersonExHistoricalStr, textHeight -2, true);
	}
	// this is the call when there is no data
	protected void printBestPersonExHistorical (double d, string str)
	{
		printBestPersonExHistorical (d, str, textHeight, false);
	}
	// this is regular call with data
	protected void printBestPersonExHistorical (double d, string str, int textH, bool graph)
	{
		g.SetSourceColor (greenDark);
		/*
		LogB.Information ("is null d?");
		LogB.Information ((d == null).ToString ());
		LogB.Information ("is null str?");
		LogB.Information ((str == null).ToString ());
		*/
		printText (leftMargin, graphHeight -textHeight, 0, textH, str, g, alignTypes.LEFT);

		if (graph)
		{
			g.MoveTo(leftMargin, calculatePaintY (d));
			g.LineTo(graphWidth-rightMargin, calculatePaintY (d));
			g.Stroke ();
		}
	}

	//encoder !relativeToSet
	protected void writePersonsBest ()
	{
		double y = calculatePaintY(maxIntersession);

		// 1) line
		g.Save();
		g.LineWidth = 2;
		g.SetDash(new double[]{2, 2}, 0);

		g.MoveTo(0, y);
		g.LineTo(graphWidth, y);
		g.Stroke ();

		g.Restore();

		// 2) texts
		printText(0, y -titleTextHeight, 0, titleTextHeight,
				string.Format ("Person's historical best {0} saved repetition", maxIntersessionEcconCriteria),
				g, alignTypes.LEFT);

		if(maxIntersessionValueStr != "")
			printText(graphWidth, y -titleTextHeight, 0, titleTextHeight,
					maxIntersessionValueStr + " (" + maxIntersessionDate + ")",
					g, alignTypes.RIGHT);
	}

	protected void writeMessageAtCenter(string message)
	{
		if (message.Contains ('\n'))
		{
			printTextMultiline (graphWidth/2, graphHeight/2, 0, textHeight + 2,
					message, g, alignTypes.CENTER, false); //do not show with yellow rectangle ass its difficoult to align
			return;
		}

		Cairo.TextExtents te;
		int messageTextHeight = textHeight +2;

		do {
			g.SetFontSize(messageTextHeight);
			te = g.TextExtents(message);
			if(te.Width >= .9 * graphWidth)
				messageTextHeight --;
		} while (te.Width >= .9 * graphWidth && messageTextHeight >= 1);

		g.SetSourceColor(yellow); //to have contrast with the bar

		g.Rectangle(graphWidth/2 -te.Width/2 -1, graphHeight/2 -messageTextHeight -1,
				te.Width +2, te.Height+4);

		g.Fill();

		g.SetSourceColor(black);

		printText (graphWidth/2, graphHeight/2 -messageTextHeight/2,
				0, messageTextHeight,
				message, g, alignTypes.CENTER);

		g.SetFontSize(textHeight -2);
	}

	/*
	protected void writeTitleAtRight()
	{
		int ypos = -6;

		//writeTextAtRight(ypos++, titleStr, true);
		//writeTextAtRight(ypos++, jumpTypeStr + " " + jumpType, false);
		//writeTextAtRight(ypos++, date, false);
		
		printText(graphWidth, Convert.ToInt32(graphHeight/2 + textHeight*2), 0, textHeight,
				titleStr, g, alignTypes.LEFT);
	}
	*/

	//reccomended to 1st paint the grid, then the axis
	protected void paintGridDo (gridTypes gridType, bool niceAutoValues)
	{
		if(minY == maxY)
			return;

		g.LineWidth = 1; //to allow to be shown the red arrows on jumpsWeightFVProfile

		if(niceAutoValues)
			paintGridNiceAutoValues (g, minX, maxX, minY, maxY, 5, gridType, 0, textHeight -2);
		else
			paintGridInt (g, minX, maxX, minY, maxY, 1, gridType, 0, textHeight -2);
	}

	//return the bar num from 0 (left bar) to the last bar
	public int FindBarInPixel (double px, double py)
	{
		LogB.Information("cairo bars FindBarInPixel 0");
		if(mouseLimits == null)
			return -1;

		LogB.Information("cairo bars FindBarInPixel 1");
		return mouseLimits.FindBarInPixel (px, py);
	}

	//return the id (uniqueID)
	public int FindBarIdInPixel (double px, double py)
	{
		LogB.Information("cairo bars FindBarIdInPixel 0");

		int bar = FindBarInPixel (px, py);
		if(bar == -1)
			return -1;

		//LogB.Information(string.Format("mouseLimits.Count: {0}, id_l.Count: {1}, bar: {2}",
		//			mouseLimits.Count(), id_l.Count, bar));

		if(id_l == null || bar >= id_l.Count)
			return -1;

		LogB.Information("cairo bars FindBarIdInPixel 1");
		return id_l[bar];
	}

	/*
	   encoder title has different strings, one of them in grey, more or less on the center
	   we need to pass the strings here to create the title
	   */
	public void SetEncoderTitle (string titleStr, string lossStr, string workStr, string impulseStr)
	{
		this.titleStr = titleStr;
		this.lossStr = lossStr;
		this.workStr = workStr;
		this.impulseStr = impulseStr;

		encoderTitle = true;
	}

	public string XVariable {
		set { xVariable = value; }
	}
	public string YVariable {
		set { yVariable = value; }
	}
	public string YUnits {
		set { yUnits = value; }
	}

	/*
	public List<int> InBarNums_l {
		set { inBarNums_l = value; }
	}
	*/
	public List<int> EdgeBarNums_l {
		set { edgeBarNums_l = value; }
	}

	public bool SpaceBetweenBars {
		set { spaceBetweenBars = value; }
	}

	public double VideoPlayTimeInSeconds {
		set { videoPlayTimeInSeconds = value; }
	}

	public List<double> VideoPlayTimes_l {
		set { videoPlayTimes_l = value; }
	}

	//for CairoBarsNHSeries (legend)
	public string VariableSerieA {
		set { variableSerieA = value; }
	}
	public string VariableSerieB {
		set { variableSerieB = value; }
	}

	public List<int> Id_l {
		set { id_l = value; }
	}

	public int SelectedPos {
		set { selectedPos = value; }
	}

	public List<int> SelectedPos_l {
		get { return selectedPos_l; }
		set { selectedPos_l = value; }
	}

	// used to draw on boxplot values if they are not part of the shown bars
	// on encoder will be a list
	public double SelectedDouble {
		set {
			selectedDouble = value;
			selectedDoubleDefined = true;
		}
	}
	public List<double> SelectedDouble_l
	{
		get { return selectedDouble_l; }
		set { selectedDouble_l = value; }
	}

	public List<double> Color_l {
		set { color_l = value; }
	}

	public List<bool> PersonIcon_l {
		set { personIcon_l = value; }
	}

	public CairoBarsSecondaryLineData Cbsld {
		set { cbsld = value; }
	}

	public List<CairoBarsArrow> EccOverload_l {
		set { eccOverload_l = value; }
	}
	public bool EccOverloadWriteValue {
		set { eccOverloadWriteValue = value; }
	}

	public List<int> Saved_l {
		set { saved_l = value; }
	}

	public double MaxIntersession {
		set { maxIntersession = value; }
	}
	public Preferences.EncoderRepetitionCriteria MaxIntersessionEcconCriteria {
		set { maxIntersessionEcconCriteria = value; }
	}
	public string MaxIntersessionValueStr {
		set { maxIntersessionValueStr = value; }
	}
	public string MaxIntersessionDate {
		set { maxIntersessionDate = value; }
	}

	public double BestPersonExHistoricalD {
		set { bestPersonExHistoricalD = value; }
	}
	public string BestPersonExHistoricalStr {
		set { bestPersonExHistoricalStr = value; }
	}

	public int Decs {
		set { decs = value; }
	}

	public string ScreenshotURL
	{
		set { screenshotURL = value; }
	}
}
