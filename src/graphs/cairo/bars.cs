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
 *  Copyright (C) 2004-2022   Xavier de Blas <xaviblas@gmail.com>
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

	protected DrawingArea area;
	protected ImageSurface surface;

	protected int fontHeightAboveBar; //will be reduced if does not fit. On encoder is bigger than other places, pass -1 if don't want to define
	protected int fontHeightForBottomNames;
	protected int marginForBottomNames;
	protected bool clickable;
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

	protected Cairo.Color black;
	protected Cairo.Color gray99;
	protected Cairo.Color gray153; //light
	protected Cairo.Color white;
	protected Cairo.Color greenDark;
	protected Cairo.Color blue;
	//protected Cairo.Color blueChronojump;
	//protected Cairo.Color bluePlots;
	//protected Cairo.Color yellow;

	protected RepetitionMouseLimits mouseLimits;
	protected List<int> id_l; //to pass the uniqueID of some test, eg: RunInterval executions and then find it using mouseLimits
	protected int selectedPos;
	protected List<double> lineData_l; //related to secondary variable (by default range)
	protected List<CairoBarsArrow> eccOverload_l;
	protected bool eccOverloadWriteValue;
	protected List<int> saved_l;
	protected double maxIntersession;
	protected Preferences.EncoderRepetitionCriteria maxIntersessionEcconCriteria;
	protected string maxIntersessionValueStr; //with correct decimals and units
	protected string maxIntersessionDate;

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
	}

	public void PassGuidesData (CairoBarsGuideManage cairoBarsGuideManage)
	{
		this.cairoBarsGuideManage = cairoBarsGuideManage;
	}

	protected void drawGuides (Cairo.Color color)
	{
		g.SetSourceColor(color);

		double personMax = cairoBarsGuideManage.GetTipPersonMax();
		double personAvg = cairoBarsGuideManage.GetTipPersonAvg();
		double personMin = cairoBarsGuideManage.GetTipPersonMin();
		double personMaxG = calculatePaintY(personMax);
		double personAvgG = calculatePaintY(personAvg);
		double personMinG = calculatePaintY(personMin);

		double groupMax = cairoBarsGuideManage.GetTipGroupMax();
		double groupAvg = cairoBarsGuideManage.GetTipGroupAvg();
		double groupMin = cairoBarsGuideManage.GetTipGroupMin();
		double groupMaxG = calculatePaintY(groupMax);
		double groupAvgG = calculatePaintY(groupAvg);
		double groupMinG = calculatePaintY(groupMin);

		textTickPos ttp = drawGuidesFindYAvg(
			personMax, personAvg, personMin, personMaxG, personAvgG, personMinG,
			groupMax, groupAvg, groupMin, groupMaxG, groupAvgG, groupMinG);

		int xStart = 6;
		if(usePersonGuides)
			drawGuidesDo (xStart, "image_person_outline.png", ttp, color,
					personMax, personAvg, personMin,
					personMaxG, personAvgG, personMinG);

		if(usePersonGuides && useGroupGuides)
			xStart += (24 + 8);

		if(useGroupGuides)
			drawGuidesDo (xStart, "image_group_outline.png", ttp, color,
					groupMax, groupAvg, groupMin,
					groupMaxG, groupAvgG, groupMinG);

		//write the X text
		if(ttp == textTickPos.ABSOLUTEBOTTOM)
		{
			xStart = 6;
			if(usePersonGuides && useGroupGuides)
				xStart += (24 + 8)/2;

			printText(graphWidth - rightMargin +xStart +12, graphHeight -2*textHeight, 0, textHeight -3,
					"X", g, alignTypes.CENTER);

			g.MoveTo(graphWidth - rightMargin +xStart +12 -3, graphHeight -2*textHeight -5);
			g.LineTo(graphWidth - rightMargin +xStart +12 +3, graphHeight -2*textHeight -5);
			g.Stroke ();
		}
	}

	public void PassArrowData (CairoBarsArrow cairoBarsArrow)
	{
		this.cairoBarsArrow = cairoBarsArrow;
	}

	protected enum textTickPos { ABOVETICK, BELOWTICK, ABSOLUTEBOTTOM }

	protected textTickPos drawGuidesFindYAvg (
			double personMax, double personAvg, double personMin,
			double personMaxG, double personAvgG, double personMinG,
			double groupMax, double groupAvg, double groupMin,
			double groupMaxG, double groupAvgG, double groupMinG)
	{
		if(usePersonGuides && ! useGroupGuides)
		{
			//print avg above avg tick
			if(personAvgG - personMaxG > 2*textHeight)
				return textTickPos.ABOVETICK;
			 //print avg below avg tick
			else if(personMinG - personAvgG > 2*textHeight)
				return textTickPos.BELOWTICK;
			else
				return textTickPos.ABSOLUTEBOTTOM;
		}
		else if(! usePersonGuides && useGroupGuides)
		{
			if(groupAvgG - groupMaxG > 2*textHeight)
				return textTickPos.ABOVETICK;
			else if(groupMinG - groupAvgG > 2*textHeight)
				return textTickPos.BELOWTICK;
			else
				return textTickPos.ABSOLUTEBOTTOM;
		}
		else //if(usePersonGuides && useGroupGuides)
		{
			//print avg above avg tick
			if(groupAvgG - groupMaxG > 2*textHeight && personAvgG - personMaxG > 2*textHeight)
				return textTickPos.ABOVETICK;
			 //print avg below avg tick
			else if(groupMinG - groupAvgG > 2*textHeight && personMinG - personAvgG > 2*textHeight)
				return textTickPos.BELOWTICK;
			else
				return textTickPos.ABSOLUTEBOTTOM;
		}
	}

	protected void drawGuidesDo (int xStart, string imageStr, textTickPos ttp, Cairo.Color color,
			double top, double avg, double bottom, double topG, double avgG, double bottomG)
	{
		Pixbuf pixbuf = new Pixbuf (null, Util.GetImagePath(false) + imageStr);
		Gdk.CairoHelper.SetSourcePixbuf (g, pixbuf, graphWidth -rightMargin +xStart, topMargin -24);
		g.Paint();

		if(top != bottom) // if only 1 value (top == bottom), do not draw the arrow
		{
			//draw arrow
			plotArrowPassingGraphPoints (g, color, true,
					graphWidth -rightMargin +xStart +12,
					bottomG,
					graphWidth -rightMargin +xStart +12,
					topG,
					false, true, 0);

			//print max/min
			printText(graphWidth - rightMargin +xStart +12, topG -textHeight/2, 0, textHeight -3,
					Util.TrimDecimals(top, 2),
					g, alignTypes.CENTER);
			printText(graphWidth - rightMargin +xStart +12, bottomG +textHeight/2, 0, textHeight -3,
					Util.TrimDecimals(bottom, 2),
					g, alignTypes.CENTER);
		}

		//print avg
		g.SetSourceColor(red);
		Cairo.TextExtents te;
		te = g.TextExtents(Util.TrimDecimals(avg,2));

		if(ttp == textTickPos.ABOVETICK)
		{
			g.SetSourceColor(white);
			g.Rectangle(graphWidth - rightMargin +xStart +12 -te.Width/2 -1,
					avgG -textHeight -1,
					te.Width +2, te.Height+2);
			g.Fill();

			g.SetSourceColor(red);
			printText(graphWidth - rightMargin +xStart +12, avgG -textHeight/2, 0, textHeight -3,
					Util.TrimDecimals(avg, 2),
					g, alignTypes.CENTER);
		}
		else if(ttp == textTickPos.BELOWTICK)
		{
			g.SetSourceColor(white);
			g.Rectangle(graphWidth - rightMargin +xStart +12 -te.Width/2 -1,
					avgG -1,
					te.Width +2, te.Height+2);
			g.Fill();

			g.SetSourceColor(red);
			printText(graphWidth - rightMargin +xStart +12, avgG +textHeight/2, 0, textHeight -3,
					Util.TrimDecimals(avg, 2),
					g, alignTypes.CENTER);
		}
		else //(textTickPos.ABSOLUTEBOTTOM)
		{
			printText(graphWidth - rightMargin +xStart +12, graphHeight -textHeight, 0, textHeight -3,
					Util.TrimDecimals(avg, 2),
					g, alignTypes.CENTER);
		}

		//draw the avg red tick
		g.LineWidth = 2;
		g.MoveTo(graphWidth - rightMargin +xStart +6, avgG);
		g.LineTo(graphWidth - rightMargin +xStart +18, avgG);
		g.Stroke ();
		g.LineWidth = 1;
	}

	public virtual void PassData1Serie (List<PointF> barMain_l,
			List<Cairo.Color> colorMain_l, List<string> names_l,
			int fontHeightAboveBar, int fontHeightForBottomNames, int marginForBottomNames,
			string titleStr, List<int> best_l, List<int> worst_l)
	{
		//defined in CairoBars1Series
	}

	public virtual void PassData2Series (List<PointF> barMain_l, List<List<PointF>> barSecondary_ll, bool mainAtLeft,
			List<Cairo.Color> colorMain_l, List<Cairo.Color> colorSecondary_l, List<string> names_l,
			string labelBarMain,// string labelBarSecondary,
			bool labelRotateInFirstBar,
			int fontHeightAboveBar, int fontHeightForBottomNames, int marginForBottomNames,
			string titleStr, List<int> best_l, List<int> worst_l)
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
		white = colorFromRGB(255,255,255);
		greenDark = colorFromRGB(0,140,0);
		blue = colorFromRGB(178, 223, 238); //lightblue
		//blueChronojump = colorFromRGB(14, 30, 70);
		//bluePlots = colorFromRGB(0, 0, 200);
		yellow = colorFromRGB(255,204,1);

		//margins
		leftRightMarginsSet();
		bottomMargin = 9;
		topMarginSet ();

		mouseLimits = new RepetitionMouseLimits();
		id_l = new List<int>();
		lineData_l = new List<double>();
		eccOverload_l = new List<CairoBarsArrow>();
		eccOverloadWriteValue = false;
		saved_l = new List<int>();
		maxIntersession = 0;
		maxIntersessionValueStr = "";
		maxIntersessionDate = "";
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
		printText(graphWidth - Convert.ToInt32(leftMargin/2), graphHeight - 2*bottomMargin, 0, textHeight -2,
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
                return graphHeight - (topMargin + bottomMargin) //graph ata area
			- UtilAll.DivideSafe(
				(realY - minY) * (graphHeight - (topMargin+bottomMargin)),
				//maxY - minY)
				//have 20% extra margin on the top (highest values will be this % far from max of the graph, needed also because text is above)
				1.2*maxY - minY)
			+ topMargin;
        }

	//used for plotAlternative (that uses another series, so pass maxY and minY
	protected double calculatePaintY (double realY, double maxY, double minY)
	{
                return graphHeight - (topMargin + bottomMargin) //graph ata area
			- UtilAll.DivideSafe(
				(realY - minY) * (graphHeight - (topMargin+bottomMargin)),
				//maxY - minY)
				//have 20% extra margin on the top (highest values will be this % far from max of the graph, needed also because text is above)
				1.2*maxY - minY)
			+ topMargin;
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

		g.MoveTo(x - moveToLeft, y + textH/2);
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

		g.SetFontSize(textH+4);
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
			drawRectangleAroundText (x, y, textH, text, g, yellow);
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

	private void drawRectangleAroundText (double x, double y, int textH, string text, Cairo.Context g, Cairo.Color colorRectangle)
	{
		//for inRectangle (now only working on centered text (encoder))
		double rectLeft = 100000;
		double rectRight = 0;
		double rectTop = 100000;
		double rectBottom = 0;

		string [] strFull = text.Split(new char[] {'\n'});

		//reversed to ensure last line is in the bottom
		for (int i = strFull.Length -1; i >= 0; i --)
		{
			g.SetFontSize(textH);
			Cairo.TextExtents te = g.TextExtents(text);
			double left = x -te.Width/2;
			double right = x +te.Width/2;
			double top = y +te.YBearing + textH/2; //+textH/2 because printText will do this move
			double bottom = y +te.YAdvance + textH/2; //+textH/2 (same as above)

			if(left < rectLeft)
				rectLeft = left;
			if(right > rectRight)
				rectRight = right;
			if(top < rectTop)
				rectTop = top;
			if(bottom > rectBottom)
				rectBottom = bottom;
		}

		if (rectLeft < 100000 && rectTop < 100000)
		{
			g.SetSourceColor(colorRectangle);
			g.Rectangle(rectLeft -1, rectTop -1, rectRight-rectLeft +2, rectBottom-rectTop +2);
			g.Fill();
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
		double yValues = UtilAll.DivideSafe(calculatePaintY(maxY), 2);
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
						yValues,
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


	protected void plotAlternativeLine (List<double> lineData_l)
	{
		//be safe
		if(barsXCenter_l.Count != lineData_l.Count)
			return;

		g.SetSourceColor(yellow); //to have contrast with the bar

		// 1) lines
		bool firstDone = false;
		for (int i = 0 ; i < barsXCenter_l.Count; i ++)
		{
			double y = calculatePaintY (lineData_l[i],
					MathUtil.GetMax (lineData_l),
					0);//MathUtil.GetMin (lineData_l));

			if(! firstDone)
			{
				g.MoveTo(barsXCenter_l[i], y);
				firstDone = true;
			} else
				g.LineTo(barsXCenter_l[i], y);
		}
		g.Stroke();

		// 2) points
		int pointsRadius = 4;
		for (int i = 0 ; i < barsXCenter_l.Count; i ++)
		{
			double y = calculatePaintY (lineData_l[i],
					MathUtil.GetMax (lineData_l),
					0);//MathUtil.GetMin (lineData_l));

			g.Arc(barsXCenter_l[i], y, pointsRadius, 0.0, 2.0 * Math.PI); //full circle
			g.FillPreserve();
			g.Stroke();
		}

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
		double marginBetweenTexts = 1.0 * te.Width;

		// 2) find longestWord width
		te = g.TextExtents(longestWord);

		// 3) if longestWord * events.Count does not fit, iterate to find correct font size
		int optimalFontHeight = textHeight;
		for(int i = textHeight; events.Count * (te.Width + marginBetweenTexts) > graphWidth && i > 0; i --)
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
					barResult.p.Z, pAyStart, barResult.selected);
	}

	protected double plotResultOnBarDo (double x, double y, double alto,
			double result, double yStartPointA, bool isSelected)
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

		g.SetSourceColor(black);
		if (isSelected)
		{
			g.Save ();
			g.LineWidth = 2;
			g.SetDash (new double[]{2, 2}, 0);
			g.Rectangle (x - te.Width/2 -2, yStart -2, te.Width +4, te.Height +6); //+6 (from -2 to +4) to accomodate the comma
			g.Stroke ();
			g.Restore ();
		}

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

	//related to secondary variable (by default range)
	public List<double> LineData_l {
		set { lineData_l = value; }
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

	public int Decs {
		set { decs = value; }
	}
}

public class CairoBars1Series : CairoBars
{
	private List<PointF> barMain_l;
	private List<Cairo.Color> colorMain_l;
	private List<string> names_l;

	//constructor when there are no points
	public CairoBars1Series (DrawingArea area, Type type, string font, string message)
	{
		this.area = area;
		this.type = type;

		LogB.Information("constructor without points, area is null:" + (area == null).ToString());
		LogB.Information("constructor without points, area.Window is null:" + (area.Window == null).ToString());
		initGraph(font, 1); //.8 to have title at right

		if(message != "")
			writeMessageAtCenter(message);

		endGraphDisposing(g, surface, area.Window);
	}

	//regular constructor
	public CairoBars1Series (DrawingArea area, Type type, bool clickable, bool paintAxis, bool paintGrid)
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
		foreach(PointF p in barMain_l)
			if(p.Y > maxY)
				maxY = p.Y;

		if(cairoBarsGuideManage != null  && cairoBarsGuideManage.GetMax() > maxY)
			maxY = cairoBarsGuideManage.GetMax();

		if(maxIntersession >= maxY)
			maxY = maxIntersession;

		//points X start at 1
		minX = 0;
		maxX = barMain_l.Count + 1;

		//bars Y have 0 at bottom
		minY = 0;
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

			drawRoundedRectangle (true, x, y, barWidth, graphHeight -y -bottomMargin, 4, g, barColor,
					Util.FoundInListInt (best_l, i),
					Util.FoundInListInt (worst_l, i));
			barResult_l.Add (new BarResult (new Point3F(x + barWidth/2, y, p.Y), i == selectedPos));
			mouseLimits.AddInPos (i, x, y, x+barWidth, graphHeight -bottomMargin);

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
			printTextMultiline (x + barWidth/2,
					graphHeight - fontHeightForBottomNames * 2/3,
					0, fontHeightForBottomNames,
					names_l[i] + videoPlayingStr, g, alignTypes.CENTER,
					Util.FoundInListInt(saved_l, i));
			//LogB.Information("names_l[i]: " + names_l[i]);

			barsXCenter_l.Add(x + barWidth/2);
		}
	}

	//done here and not in the constructor because most of this variables are known after construction
	public override void PassData1Serie (List<PointF> barMain_l,
			List<Cairo.Color> colorMain_l, List<string> names_l,
			int fontHeightAboveBar, int fontHeightForBottomNames, int marginForBottomNames,
			string titleStr, List<int> best_l, List<int> worst_l)
	{
		this.barMain_l = barMain_l;
		this.colorMain_l = colorMain_l;
		this.names_l = names_l;
		this.fontHeightAboveBar = fontHeightAboveBar;
		this.fontHeightForBottomNames = fontHeightForBottomNames;
		this.marginForBottomNames = marginForBottomNames;

		if(! encoderTitle)
			this.titleStr = titleStr;

		this.best_l = best_l;
		this.worst_l = worst_l;
	}

	public override void GraphDo ()
	{
		LogB.Information("at CairoBars1Series.Do");
		//LogB.Information(string.Format("bottomMargin pre: {0}, marginForBottomNames: {1}", bottomMargin, marginForBottomNames));
		bottomMargin += marginForBottomNames;

                findMaximums();

		g.SetFontSize(textHeight);

		if(paintAxis)
			paintAxisDo (2);

		if(paintGrid)
			paintGridDo (gridTypes.HORIZONTALLINES, true);
		//g.SetFontSize(textHeight);

		if(cairoBarsGuideManage != null)
			drawGuides(colorSerieA);

		g.SetSourceColor(black);
		plotBars ();

		if(cairoBarsArrow != null)
			plotArrow();

		if(lineData_l.Count > 0)
			plotAlternativeLine(lineData_l);

		if (edgeBarNums_l.Count > 0)
			plotEdgeBarNums ();

		plotResultsOnBar();

		writeTitleAtTop ();

		if(maxIntersession > 0)
			writePersonsBest (); //encoder !relativeToSet

		if(clickable)
		{
			if(type == Type.ENCODER)
				addClickableMark (g, 0);
			else
				addClickableMark (g, 1); //default
		}

		endGraphDisposing(g, surface, area.Window);
	}
}

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

	//constructor when there are no points
	public CairoBarsNHSeries (DrawingArea area, Type type, string font)
	{
		this.area = area;
		this.type = type;

		LogB.Information("constructor without points, area is null:" + (area == null).ToString());
		LogB.Information("constructor without points, area.Window is null:" + (area.Window == null).ToString());
		initGraph(font, 1); //.8 to have title at right

		endGraphDisposing(g, surface, area.Window);
	}

	//regular constructor
	public CairoBarsNHSeries (DrawingArea area, Type type, bool showLegend, bool clickable, bool paintAxis, bool paintGrid)
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
				if(p.Y > maxY)
					maxY = p.Y;

		foreach(PointF p in barMain_l)
			if(p != null && p.Y > maxY) //on ec at capturing if last is ecc, a con is send as null
				maxY = p.Y;

		if(cairoBarsGuideManage != null  && cairoBarsGuideManage.GetMax() > maxY)
			maxY = cairoBarsGuideManage.GetMax();

		if(maxIntersession >= maxY)
			maxY = maxIntersession;

		//points X start at 1
		minX = 0;
		//maxX = barMain_l.Count + .5; //all barMain_l lists have same length
		maxX = barMain_l.Count + 1;

		//while capturing ecc-con, if last rep don is an ecc, it have to be drawn
		if(barSecondary_ll.Count == 1 && barSecondary_ll[0].Count > barMain_l.Count)
			maxX ++;

		//bars Y have 0 at bottom
		minY = 0;
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
		   divide graphWidhtUsable by total objects (bars, leftrightspace, spacesbetweenbars)
		   for 3 (double) bars on ratios 1, .5, .5, this will be 8
		   */
		int series = 2;
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

			double x = leftMargin + sideWidthRatio*barWidth + i*2*barWidth + spacesBetweenBarGroups;
			double adjustX = 0; //this is used on second bar (at right), can be used on first if mainAtLeft

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

					drawRoundedRectangle (true, x + adjustX, y, barWidth, graphHeight -y -bottomMargin, 4, g, barColor,
							Util.FoundInListInt (best_l, i),
							Util.FoundInListInt (worst_l, i));
					resultOnBarsThisIteration_l.Add(new Point3F(x + adjustX + barWidth/2, y-4, pS.Y));
					//to print line variable if needed
					//barsXCenter_l.Add(x + adjustX + barWidth/2);

					if(labelBarMain != "")
					{
						if(labelRotateInFirstBar)
						{
							if(i == barSecondary_ll[j].Count -1)
							{
								g.SetSourceColor(white);
								int sep = 4;
								printTextRotated (x +adjustX +barWidth -sep, graphHeight -bottomMargin -sep, 0, textHeight, "Ecc", g, true);
								g.SetSourceColor(black);
							}
						}
						else
							printTextInBar(x +adjustX +barWidth/2, graphHeight -bottomMargin -10,
									0, textHeight+2, "e", g, true, false);
					}

					secondaryHasData = true;
					timesSubtestThis += pS.Y;
				}

				//mouse limits stuff
				if(pS.Y > 0)
					mouseLimits.AddInPos (mouseLimitsPos1stBar, x+adjustX, y, x+adjustX+barWidth, graphHeight -bottomMargin);
				else {
					//add it 0 width, to respect order when DJs are mixed with CMJs, but not be able to be selected
					mouseLimits.AddInPos (mouseLimitsPos1stBar, x+adjustX, y, x+adjustX, graphHeight -bottomMargin);
				}
				mouseLimitsPos1stBar += 2;

				adjustX += barWidth;
			}

			//main bar: eg tv on jumps
			if(pB.Y > 0)
			{
				//if there is no data on previous variables, just put pB in the middle
				if(! secondaryHasData)
					adjustX = barWidth/2;

				double y = calculatePaintY(pB.Y);

				Cairo.Color barColor = colorSerieB;
				if(colorMain_l != null && colorMain_l.Count == barMain_l.Count)
					barColor = colorMain_l[i];

				drawRoundedRectangle (true, x+adjustX, y, barWidth, graphHeight -y -bottomMargin, 4, g, barColor,
						Util.FoundInListInt (best_l, i),
						Util.FoundInListInt (worst_l, i));
				resultOnBarsThisIteration_l.Add(new Point3F(x + adjustX + barWidth/2, y, pB.Y));
				//add for the secondary and for the main bar, no problem both will work
				mouseLimits.AddInPos (mouseLimitsPos2ndBar, x+adjustX, y, x+adjustX+barWidth, graphHeight -bottomMargin);
				mouseLimitsPos2ndBar += 2;

				//to print line variable if needed
				//barsXCenter_l.Add(x + adjustX + barWidth/2);

				if(labelBarMain != "")
				{
					if(labelRotateInFirstBar)
					{
						if(i == barMain_l.Count -1)
						{
							g.SetSourceColor(white);
							int sep = 4;
							printTextRotated (x +adjustX +barWidth -sep, graphHeight -bottomMargin -sep, 0, textHeight, "Con", g, true);
							g.SetSourceColor(black);
						}
					}
					else
						printTextInBar(x +adjustX +barWidth/2, graphHeight -bottomMargin -10,
								0, textHeight+2, "c", g, true, false);
				}

				//to show text centered at bottom correctly
				if(! secondaryHasData)
					adjustX = barWidth;

				timesSubtestThis += pB.Y;
			}

			//sort result on bars correctly (this could be useful if mainAtLeft changes)
			for(int j = 0 ; j < resultOnBarsThisIteration_l.Count; j ++)
			{
				barResult_l.Add (new BarResult (resultOnBarsThisIteration_l[j], i == selectedPos));
				barsXCenter_l.Add(resultOnBarsThisIteration_l[j].X);
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
			printTextMultiline(
					x+adjustX,
					graphHeight -fontHeightForBottomNames * 2/3,
					0, fontHeightForBottomNames,
					names_l[i] + videoPlayingStr, g, alignTypes.CENTER,
					Util.FoundInListInt(saved_l, i));
		}


	}

	//done here and not in the constructor because most of this variables are known after construction
	public override void PassData2Series (List<PointF> barMain_l, List<List<PointF>> barSecondary_ll, bool mainAtLeft,
			List<Cairo.Color> colorMain_l, List<Cairo.Color> colorSecondary_l, List<string> names_l,
			string labelBarMain,// string labelBarSecondary,
			bool labelRotateInFirstBar,
			int fontHeightAboveBar, int fontHeightForBottomNames, int marginForBottomNames,
			string titleStr, List<int> best_l, List<int> worst_l)
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
	}

	public override void GraphDo ()
	{
		bottomMargin += marginForBottomNames;

		//LogB.Information(string.Format("NH GraphDo: pointA_l.Count: {0}, pointB_l.Count: {1}", pointA_l.Count, pointB_l.Count));
                findMaximums();

		g.SetFontSize(textHeight);

		if(paintAxis)
			paintAxisDo (2);

		if(paintGrid)
			paintGridDo (gridTypes.HORIZONTALLINES, true);
		//g.SetFontSize(textHeight);

		if(cairoBarsGuideManage != null)
			drawGuides(colorSerieB);

		g.SetSourceColor(black);
		plotBars();

		if(cairoBarsArrow != null)
			plotArrow();

		if(lineData_l.Count > 0)
			plotAlternativeLine(lineData_l);

		if(eccOverload_l.Count > 0)
			plotEccOverload();

		plotResultsOnBar();

		writeTitleAtTop ();

		if(maxIntersession > 0)
			writePersonsBest (); //encoder !relativeToSet

		if(showLegend)
			writeLegend ();

		if(clickable)
		{
			if(type == Type.ENCODER)
				addClickableMark (g, 0);
			else
				addClickableMark (g, 1); //default
		}

		endGraphDisposing(g, surface, area.Window);
	}
}

// ----------------------------------------------------------------

public class BarResult
{
	public Point3F p;
	public bool selected;
	public BarResult (Point3F p, bool selected)
	{
		this.p = p;
		this.selected = selected;
	}
}

public class CairoBarsGuide
{
	public enum GuideEnum { SESSION_MAX, SESSION_AVG, SESSION_MIN, PERSON_MAX_ALL_S, PERSON_MAX_THIS_S, PERSON_AVG_THIS_S, PERSON_MIN_THIS_S }

	private GuideEnum gEnum;
	private double y;
	private int width;
	private Cairo.Color color;
	private char c; //this will be an icon of person or group;
	private double extraRightDist;
	//color, linetype, icon, ...

	public CairoBarsGuide (GuideEnum gEnum, double y, int width, Cairo.Color color, char c, double extraRightDist)
	{
		this.gEnum = gEnum;
		this.y = y;
		this.width = width;
		this.color = color;
		this.c = c;
		this.extraRightDist = extraRightDist;
	}

	public GuideEnum Genum {
		get { return gEnum; }
	}
	public double Y {
		get { return y; }
	}
	public int Width {
		get { return width; }
	}
	public Cairo.Color Color {
		get { return color; }
	}
	public char C {
		get { return c; }
	}
	public double ExtraRightDist {
		get { return extraRightDist; }
	}
}

//manage distances of guides do draw als the person, session indicators
//right now used on jump/run simple
public class CairoBarsGuideManage
{
	private List<CairoBarsGuide> l;

	public CairoBarsGuideManage (bool usePersonGuides, bool useGroupGuides,
			double sessionMAXAtSQL, double sessionAVGAtSQL, double sessionMINAtSQL,
			double personMAXAtSQLAllSessions, double personMAXAtSQL, double personAVGAtSQL, double personMINAtSQL)
	{
		l = new List<CairoBarsGuide> ();
		//int pos = 1;
		//int dist = 8;

		if(useGroupGuides)
		{
			l.Add(new CairoBarsGuide(CairoBarsGuide.GuideEnum.SESSION_MAX, sessionMAXAtSQL,
						2, colorFromRGB(0,0,0), 'G', 12)); //(pos++)*dist));
			l.Add(new CairoBarsGuide(CairoBarsGuide.GuideEnum.SESSION_AVG, sessionAVGAtSQL,
						1, colorFromRGB(0,0,0), 'g', 12)); //(pos++)*dist));
			l.Add(new CairoBarsGuide(CairoBarsGuide.GuideEnum.SESSION_MIN, sessionMINAtSQL,
						1, colorFromRGB(0,0,0), 'g', 12)); //(pos++)*dist));
		}

		if(usePersonGuides)
		{
			//unused
			//l.Add(new CairoBarsGuide(CairoBarsGuide.GuideEnum.PERSON_MAX_ALL_S, personMAXAtSQLAllSessions,
			//			4, colorFromRGB(255,0,255), 'P', 12)); //(pos++)*dist));

			l.Add(new CairoBarsGuide(CairoBarsGuide.GuideEnum.PERSON_MAX_THIS_S, personMAXAtSQL,
						2, colorFromRGB(255,238,102), 'P', 12)); //(pos++)*dist));
			l.Add(new CairoBarsGuide(CairoBarsGuide.GuideEnum.PERSON_AVG_THIS_S, personAVGAtSQL,
						1, colorFromRGB(255,238,102), 'p', 12)); //(pos++)*dist));
			l.Add(new CairoBarsGuide(CairoBarsGuide.GuideEnum.PERSON_MIN_THIS_S, personMINAtSQL,
						2, colorFromRGB(255,238,102), 'P', 12)); //(pos++)*dist));
		}
	}

	protected Cairo.Color colorFromRGB(int red, int green, int blue)
	{
		return new Cairo.Color(red/256.0, green/256.0, blue/256.0);
	}

	public double GetMax ()
	{
		double max = 0;
		foreach(CairoBarsGuide cbg in l)
			if(cbg.Y > max)
				max = cbg.Y;

		return max;
	}

	public double GetTipGroupMax ()
	{
		foreach(CairoBarsGuide cbg in l)
			if(cbg.Genum == CairoBarsGuide.GuideEnum.SESSION_MAX)
				return cbg.Y;

		return 0;
	}
	public double GetTipGroupAvg ()
	{
		foreach(CairoBarsGuide cbg in l)
			if(cbg.Genum == CairoBarsGuide.GuideEnum.SESSION_AVG)
				return cbg.Y;

		return 0;
	}
	public double GetTipGroupMin ()
	{
		foreach(CairoBarsGuide cbg in l)
			if(cbg.Genum == CairoBarsGuide.GuideEnum.SESSION_MIN)
				return cbg.Y;

		return 0;
	}

	public double GetTipPersonMax ()
	{
		foreach(CairoBarsGuide cbg in l)
			if(cbg.Genum == CairoBarsGuide.GuideEnum.PERSON_MAX_THIS_S)
				return cbg.Y;

		return 0;
	}
	public double GetTipPersonAvg ()
	{
		foreach(CairoBarsGuide cbg in l)
			if(cbg.Genum == CairoBarsGuide.GuideEnum.PERSON_AVG_THIS_S)
				return cbg.Y;

		return 0;
	}
	public double GetTipPersonMin ()
	{
		foreach(CairoBarsGuide cbg in l)
			if(cbg.Genum == CairoBarsGuide.GuideEnum.PERSON_MIN_THIS_S)
				return cbg.Y;

		return 0;
	}
	public List<CairoBarsGuide> L {
		get { return l; }
	}
}

// ----
//note x0pos, x1pos are the pos (meaning the bar)
//used also for the eccentric overload
public class CairoBarsArrow
{
	public int x0pos;
	public double y0;
	public int x1pos;
	public double y1;

	public CairoBarsArrow (int x0pos, double y0, int x1pos, double y1)
	{
		this.x0pos = x0pos;
		this.y0 = y0;
		this.x1pos = x1pos;
		this.y1 = y1;
		//LogB.Information(string.Format("x0pos: {0}, x1pos: {1}", x0pos, x1pos));
	}

	public double GetX0Graph (List<double> barsXCenter_l)
	{
		return barsXCenter_l[x0pos];
	}

	public double GetX1Graph (List<double> barsXCenter_l)
	{
		return barsXCenter_l[x1pos];
	}

	public override string ToString()
	{
		return string.Format("x0pos: {0}, y0: {1}, x1pos: {2}, y1: {3}",
				x0pos, y0, x1pos, y1);
	}
}
