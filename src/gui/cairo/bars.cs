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
 *  Copyright (C) 2004-2021   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List
using Gdk;
using Gtk;
using Cairo;

public abstract class CairoBars : CairoGeneric
{
	protected DrawingArea area;
	protected int fontHeightForBottomNames;
	protected int marginForBottomNames;
	protected string title;
	//protected string jumpType;
	//protected string runType;
	protected string date;
	protected Cairo.Color colorSerieA;
	protected CairoBarsGuideManage cairoBarsGuideManage;
	protected bool usePersonGuides;

	protected Cairo.Context g;
	protected int lineWidthDefault = 1; //was 2;
	protected string xVariable = "";
	protected string yVariable = "Height";
	protected string xUnits = "";
	protected string yUnits = "cm";

	protected double minX = 1000000;
	protected double maxX = 0;
	protected double minY = 1000000;
	protected double maxY = 0;

	protected Cairo.Color black;
	protected Cairo.Color gray99;
	Cairo.Color white;
	protected Cairo.Color red;
	protected Cairo.Color blue;
	protected Cairo.Color bluePlots;
	protected Cairo.Color yellow;


	public virtual void GraphInit (string font, bool usePersonGuides) //need to set rightMargin
	{
		this.usePersonGuides = usePersonGuides;

		textHeight = 14;
		initGraph(font, 1); //.8 if writeTextAtRight
	}

	public void PassGuidesData (CairoBarsGuideManage cairoBarsGuideManage)
	{
		this.cairoBarsGuideManage = cairoBarsGuideManage;
	}

	protected void drawGuides ()
	{
		g.Color = black;
		int xStart = 6;

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

		if(usePersonGuides)
		{
			drawGuidesDo (xStart, "image_person_outline.png", ttp,
					personMax, personAvg, personMin,
					personMaxG, personAvgG, personMinG);
			xStart += (24 + 6);
		}

		drawGuidesDo (xStart, "image_group_outline.png", ttp,
				groupMax, groupAvg, groupMin,
				groupMaxG, groupAvgG, groupMinG);

		//write the X text
		if(ttp == textTickPos.ABSOLUTEBOTTOM)
		{
			xStart = 6;
			if(usePersonGuides)
				xStart += (24 + 6)/2;

			printText(graphWidth - rightMargin +xStart +12, graphHeight -bottomMargin -2*textHeight, 0, textHeight -3,
					"X", g, alignTypes.CENTER);

			g.MoveTo(graphWidth - rightMargin +xStart +12 -3, graphHeight -bottomMargin -2*textHeight -5);
			g.LineTo(graphWidth - rightMargin +xStart +12 +3, graphHeight -bottomMargin -2*textHeight -5);
			g.Stroke ();
		}
	}

	protected enum textTickPos { ABOVETICK, BELOWTICK, ABSOLUTEBOTTOM }

	protected textTickPos drawGuidesFindYAvg (
			double personMax, double personAvg, double personMin,
			double personMaxG, double personAvgG, double personMinG,
			double groupMax, double groupAvg, double groupMin,
			double groupMaxG, double groupAvgG, double groupMinG)
	{
		if(usePersonGuides)
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

		if(groupAvgG - groupMaxG > 2*textHeight)
			return textTickPos.ABOVETICK;
		else if(groupMinG - groupAvgG > 2*textHeight)
			return textTickPos.BELOWTICK;
		else
			return textTickPos.ABSOLUTEBOTTOM;
	}

	protected void drawGuidesDo (int xStart, string imageStr, textTickPos ttp,
			double top, double avg, double bottom, double topG, double avgG, double bottomG)
	{
		Pixbuf pixbuf = new Pixbuf (null, Util.GetImagePath(false) + imageStr);
		Gdk.CairoHelper.SetSourcePixbuf (g, pixbuf, graphWidth -rightMargin +xStart, topMargin+2);
		g.Paint();

		if(top != bottom) // if only 1 value (top == bottom), do not draw the arrow
		{
			//draw arrow
			plotArrowPassingGraphPoints (g, black,
					graphWidth -rightMargin +xStart +12,
					bottomG,
					graphWidth -rightMargin +xStart +12,
					topG,
					false, true, 0);

			//print max/min
			printText(graphWidth - rightMargin +xStart +12, topG -textHeight/2, 0, textHeight -3,
					Util.TrimDecimals(top, 1),
					g, alignTypes.CENTER);
			printText(graphWidth - rightMargin +xStart +12, bottomG +textHeight/2, 0, textHeight -3,
					Util.TrimDecimals(bottom, 1),
					g, alignTypes.CENTER);
		}

		//print avg
		g.Color = red;
		Cairo.TextExtents te;
		te = g.TextExtents(Util.TrimDecimals(avg,1));

		if(ttp == textTickPos.ABOVETICK)
		{
			g.Color = white;
			g.Rectangle(graphWidth - rightMargin +xStart +12 -te.Width/2 -1,
					avgG -textHeight -1,
					te.Width +2, te.Height+2);
			g.Fill();

			g.Color = red;
			printText(graphWidth - rightMargin +xStart +12, avgG -textHeight/2, 0, textHeight -3,
					Util.TrimDecimals(avg, 1),
					g, alignTypes.CENTER);
		}
		else if(ttp == textTickPos.BELOWTICK)
		{
			g.Color = white;
			g.Rectangle(graphWidth - rightMargin +xStart +12 -te.Width/2 -1,
					avgG -1,
					te.Width +2, te.Height+2);
			g.Fill();

			g.Color = red;
			printText(graphWidth - rightMargin +xStart +12, avgG +textHeight/2, 0, textHeight -3,
					Util.TrimDecimals(avg, 1),
					g, alignTypes.CENTER);
		}
		else //print avg at bottom
		{
			printText(graphWidth - rightMargin +xStart +12, graphHeight -bottomMargin -textHeight, 0, textHeight -3,
					Util.TrimDecimals(avg, 1),
					g, alignTypes.CENTER);
		}

		//draw the avg red tick
		g.LineWidth = 2;
		g.MoveTo(graphWidth - rightMargin +xStart +6, avgG);
		g.LineTo(graphWidth - rightMargin +xStart +18, avgG);
		g.Stroke ();
		g.LineWidth = 1;
	}

	//TODO: move this to generic (if needed)
	protected void drawHorizontalGuide (CairoBarsGuide cbg)
	{
		/*
		g.LineWidth = cbg.Width;
		g.Color = cbg.Color;

		g.MoveTo(leftMargin, calculatePaintY(cbg.Y));
		g.LineTo(graphWidth -rightMargin +cbg.ExtraRightDist, calculatePaintY(cbg.Y));
		g.Stroke ();

		g.LineWidth = lineWidthDefault;
		g.Color = black;
		*/

		/*
		plotArrowPassingGraphPoints (g, cbg.Color,
				leftMargin, calculatePaintY(cbg.Y),
				graphWidth -rightMargin,// +cbg.ExtraRightDist,
				calculatePaintY(cbg.Y), true, 0);
				*/

		/*
		plotArrowPassingGraphPoints (g, black, //black to not have overlapped lines with different colors
				graphWidth -rightMargin -10,// +cbg.ExtraRightDist,
				calculatePaintY(cbg.Y),
				graphWidth -rightMargin +10,
				calculatePaintY(cbg.Y),
				true, 0);

		g.Color = cbg.Color;
		printText(graphWidth -rightMargin +cbg.ExtraRightDist, calculatePaintY(cbg.Y),
				0, textHeight -2, cbg.C.ToString(), g, alignTypes.LEFT); //TODO: solve align of letter that has to be higher, when it is an icon this will not be a problem, just compare it with the arrow
		g.Color = black;
				*/
		/*
		Cairo.ImageSurface imgSurface;
		if(cbg.Genum == CairoBarsGuide.GuideEnum.SESSION_MAX || cbg.Genum == CairoBarsGuide.GuideEnum.SESSION_AVG)
		{
			imgSurface = new Cairo.ImageSurface(
					"/home/xavier/informatica/progs_meus/chronojump/images/md/image_group_outline.png");
			g.SetSourceSurface (imgSurface, graphWidth -rightMargin +32, 12);//Convert.ToInt32(calculatePaintY(cbg.Y))-12);
		} else {
			imgSurface = new Cairo.ImageSurface(
					"/home/xavier/informatica/progs_meus/chronojump/images/md/ic_person_outline_blue_24dp_1x.png");
			g.SetSourceSurface (imgSurface, graphWidth -rightMargin +4, 12);//Convert.ToInt32(calculatePaintY(cbg.Y))-12);
		}

		g.Paint ();
		*/
	}

	public abstract void GraphDo (List<PointF> pointA_l, List<PointF> pointB_l,
			List<string> names_l, int fontHeightForBottomNames, int marginForBottomNames, string title);

	protected void initGraph(string font, double widthPercent1)
	{
		initGraph(font, widthPercent1, true);
	}
	protected void initGraph(string font, double widthPercent1, bool clearDrawingArea)
	{
		this.font = font;
		//LogB.Information("Font: " + font);

		leftMargin = 26;
		rightMargin = 42; //images are 24 px, separate 6 px from grapharea, and 12 px from absoluteright
		if(usePersonGuides)
			rightMargin = 70;
		topMargin = 12;
		bottomMargin = 9;

		//1 create context
		g = Gdk.CairoHelper.Create (area.GdkWindow);

		if(clearDrawingArea)
		{
			//2 clear DrawingArea (white)
			g.SetSourceRGB(1,1,1);
			g.Paint();
		}

		graphWidth = Convert.ToInt32(area.Allocation.Width * widthPercent1);
		graphHeight = area.Allocation.Height;
		LogB.Information(string.Format("graphWidth: {0}, graphHeight: {1}", graphWidth, graphHeight));

		g.SetSourceRGB(0,0,0);
		g.LineWidth = lineWidthDefault;

		//4 prepare font
		g.SelectFontFace(font, Cairo.FontSlant.Normal, Cairo.FontWeight.Normal);
		g.SetFontSize(textHeight);

		black = colorFromRGB(0,0,0);
		gray99 = colorFromRGB(99,99,99);
		white = colorFromRGB(255,255,255);
		red = colorFromRGB(200,0,0);
		blue = colorFromRGB(178, 223, 238); //lightblue
		bluePlots = colorFromRGB(0, 0, 200);
		yellow = colorFromRGB(255,238,102);
	}

	protected abstract void findMaximums(); //includes point and guides

	protected void paintAxis(int width)
	{
		g.LineWidth = width;
		g.MoveTo(leftMargin, 3*topMargin);
		g.LineTo(leftMargin, graphHeight - bottomMargin);
		g.LineTo(graphWidth - rightMargin, graphHeight - bottomMargin);
		g.Stroke ();

		printText(2, 2*topMargin, 0, textHeight -2, getYAxisLabel(), g, alignTypes.LEFT);
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
	private string getYAxisLabel()
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

	protected override void printText (double x, double y, double heightUnused, int textH,
			string text, Cairo.Context g, alignTypes align)
	{
		g.SetFontSize(textH);

		double moveToLeft = 0;
		if(align == alignTypes.CENTER || align == alignTypes.RIGHT)
		{
			Cairo.TextExtents te;
			te = g.TextExtents(text);
			
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

	//text could have one or more \n
	protected void printTextMultiline (double x, double y, double heightUnused, int textH,
			string text, Cairo.Context g, alignTypes align)
	{
		if(text == "")
			return;

		string [] strFull = text.Split(new char[] {'\n'});

		//reversed to ensure last line is in the bottom
		for (int i = strFull.Length -1; i >= 0; i --)
		{
			printText (x, y, heightUnused, textH, strFull[i], g, align);
			y -= 1.1 * textH;
		}
	}

	protected abstract void plotBars ();

	//adapted from http://www.mono-project.com/docs/tools+libraries/libraries/Mono.Cairo/cookbook/
	//bottomFlat means to have rounded only on top
	protected static void drawRoundedRectangle (bool bottomFlat,
			double x, double y, double width, double height, 
			double radius, Cairo.Context g, Cairo.Color color)
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
		g.Restore ();
		
		g.SetSourceRGB(color.R, color.G, color.B);
		g.FillPreserve ();
		g.SetSourceRGB(0, 0, 0);
		g.LineWidth = 1;
		g.Stroke ();	
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
		LogB.Information(string.Format("GetBottomMarginForText, maxRows: {0}, fontHeight: {1}, result: {2}",
					maxRows, fontHeight, Convert.ToInt32(1.3 * te.Height * maxRows)));

		return Convert.ToInt32(1.3 * te.Height * maxRows);
	}

	//TODO: at the moment we are not lowering decs, make resultsFontHeight and decs global variables
	protected int getBarsResultFontHeight (double maxWidth)
	{
		int decs = 2; //can be 1 if need more space
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
		for(int i = textHeight; te.Width >= maxWidth && i > 0; i --)
		{
			//if(i <= 8)
			//	decs = 1;

			g.SetFontSize(i);
			te = g.TextExtents(Util.TrimDecimals(maxLengthNumber, decs));
			optimalFontHeight = i;
		}

		g.SetFontSize(textHeight); //return font to its default value
		return optimalFontHeight;
	}

	protected double plotResultOnBar(double x, double y, double alto, double result,
			int resultFontHeight, double barWidth, double yStartPointA)
	{
		int decs = 2; //can be 1 if need more space
		g.SetFontSize(resultFontHeight);

		double maxLengthNumber = 9.99;
		if(maxY >= 10)
			maxLengthNumber = 99.99;
		if(maxY >= 100)
			maxLengthNumber = 999.99;
		if(maxY >= 1000)
			maxLengthNumber = 9999.99;

		Cairo.TextExtents te;
		///te = g.TextExtents(Util.TrimDecimals(result,decs));
		te = g.TextExtents(maxLengthNumber.ToString());

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

		//check if there's an overlap with pointA)
		if ( yStartPointA >= 0 && te.Width >= barWidth &&
				( yStart >= yStartPointA && yStart <= yStartPointA + te.Height ||
				  yStart <= yStartPointA && yStart + te.Height >= yStartPointA ) )
			yStart = yStartPointA - 1.1 * te.Height;

		LogB.Information(string.Format("y: {0}, alto: {1}, yStart: {2}", y, alto, yStart));

		if( (yStart + te.Height) > alto )
			yStart = alto - te.Height;

		if(textAboveBar)
			g.Color = white; //to just hide the horizontal grid
		else
			g.Color = yellow; //to have contrast with the bar

		g.Rectangle(x - te.Width/2 -1, yStart-1, te.Width +2, te.Height+2);
		g.Fill();

		//write text
		g.Color = black;
		printText(x, yStart+te.Height/2, 0, Convert.ToInt32(te.Height),
			Util.TrimDecimals(result, decs), g, alignTypes.CENTER);

		//put font size to default value again
		g.SetFontSize(textHeight);

		return yStart;
	}

	protected void writeTitleAtTop()
	{
		printText(graphWidth/2 + leftMargin, textHeight/2, 0, textHeight+2,
				title, g, alignTypes.CENTER);
	}
	/*
	protected void writeTitleAtRight()
	{
		int ypos = -6;

		//writeTextAtRight(ypos++, title, true);
		//writeTextAtRight(ypos++, jumpTypeStr + " " + jumpType, false);
		//writeTextAtRight(ypos++, date, false);
		
		printText(graphWidth, Convert.ToInt32(graphHeight/2 + textHeight*2), 0, textHeight,
				title, g, alignTypes.LEFT);
	}
	*/

	//reccomended to 1st paint the grid, then the axis
	protected void paintGrid(gridTypes gridType, bool niceAutoValues)
	{
		if(minY == maxY)
			return;

		g.LineWidth = 1; //to allow to be shown the red arrows on jumpsWeightFVProfile

		if(niceAutoValues)
			paintGridNiceAutoValues (g, minX, maxX, minY, maxY, 5, gridType, textHeight -2);
		else
			paintGridInt (g, minX, maxX, minY, maxY, 1, gridType, textHeight -2);
	}

	public string YVariable {
		set { yVariable = value; }
	}
	public string YUnits {
		set { yUnits = value; }
	}
}

public class CairoBars1Series : CairoBars
{
	private List<PointF> point_l;
	private List<string> names_l;

	//constructor when there are no points
	public CairoBars1Series (DrawingArea area, string font)
	{
		this.area = area;

		LogB.Information("constructor without points, area is null:" + (area == null).ToString());
		LogB.Information("constructor without points, area.GdkWindow is null:" + (area.GdkWindow == null).ToString());
		initGraph(font, 1); //.8 to have title at right

		endGraphDisposing(g);
	}

	//regular constructor
	public CairoBars1Series (DrawingArea area)
	{
		this.area = area;

		this.colorSerieA = colorFromGdk(Config.ColorBackground); //but note if we are using system colors, this will not match
	}

	protected override void findMaximums()
	{
		foreach(PointF p in point_l)
			if(p.Y > maxY)
				maxY = p.Y;

		if(cairoBarsGuideManage != null  && cairoBarsGuideManage.GetMax() > maxY)
			maxY = cairoBarsGuideManage.GetMax();

		//points X start at 1
		minX = 0;
		maxX = point_l.Count + 1;

		//bars Y have 0 at bottom
		minY = 0;
	}

	protected override void plotBars ()
	{
                //calculate separation between series and bar width
                double distanceBetweenCols = Convert.ToInt32((graphWidth - (leftMargin+rightMargin))*(1+.5)/point_l.Count) -
                        Convert.ToInt32((graphWidth - (leftMargin+rightMargin))*(0+.5)/point_l.Count);

                double barWidth = Convert.ToInt32(.5*distanceBetweenCols);
                double barDesplLeft = Convert.ToInt32(.5*barWidth);
		int resultFontHeight = getBarsResultFontHeight (barWidth*2);
		LogB.Information("resultFontHeight: " + resultFontHeight.ToString());

		for(int i = 0; i < point_l.Count; i ++)
		{
			PointF p = point_l[i];

			double x = (graphWidth - (leftMargin+rightMargin)) * (p.X-.5)/point_l.Count - barDesplLeft + leftMargin;
			double y = calculatePaintY(p.Y);

			drawRoundedRectangle (true, x, y, barWidth, graphHeight -y -bottomMargin, 4, g, colorSerieA);
LogB.Information(string.Format("y: {0}, alto: {1}", y, graphHeight -y - bottomMargin));
			plotResultOnBar(x + barWidth/2, y, graphHeight -bottomMargin, p.Y, resultFontHeight, barWidth, -1);

			//print the type at bottom
			//printTextMultiline (x + barWidth/2, graphHeight -bottomMargin + fontHeightForBottomNames/2, 0, fontHeightForBottomNames,
			printTextMultiline (x + barWidth/2, graphHeight - fontHeightForBottomNames * 2/3, 0, fontHeightForBottomNames,
					names_l[i], g, alignTypes.CENTER);
			LogB.Information("names_l[i]: " + names_l[i]);
		}
	}

	public override void GraphDo (List<PointF> pointA_l, List<PointF> pointB_l,
			List<string> names_l, int fontHeightForBottomNames, int marginForBottomNames, string title)
	{
		LogB.Information("at CairoBars1Series.Do");
		this.point_l = pointA_l;
		//this.pointB_l = pointB_l; unused here
		this.names_l = names_l;
		this.fontHeightForBottomNames = fontHeightForBottomNames;
		this.marginForBottomNames = marginForBottomNames;
		this.title = title;

		LogB.Information(string.Format("bottomMargin pre: {0}, marginForBottomNames: {1}", bottomMargin, marginForBottomNames));
		bottomMargin += marginForBottomNames;

                findMaximums();

		g.SetFontSize(textHeight);
		paintAxis(2);
		paintGrid(gridTypes.HORIZONTALLINES, true);
		//g.SetFontSize(textHeight);

		if(cairoBarsGuideManage != null)
			drawGuides();

		g.Color = black;
		plotBars ();

		writeTitleAtTop ();

		endGraphDisposing(g);
	}
}

//two series in horizontal, like jump Dj tc/tf
public class CairoBars2HSeries : CairoBars
{
	private List<PointF> pointA_l;
	private List<PointF> pointB_l;
	private List<string> names_l;

	private Cairo.Color colorSerieB;

	//constructor when there are no points
	public CairoBars2HSeries (DrawingArea area, string font)
	{
		this.area = area;

		LogB.Information("constructor without points, area is null:" + (area == null).ToString());
		LogB.Information("constructor without points, area.GdkWindow is null:" + (area.GdkWindow == null).ToString());
		initGraph(font, 1); //.8 to have title at right

		endGraphDisposing(g);
	}

	//regular constructor
	public CairoBars2HSeries (DrawingArea area)
	{
		this.area = area;

		colorSerieA = colorFromGdk(UtilGtk.GetColorShifted(Config.ColorBackground,
					! UtilGtk.ColorIsDark(Config.ColorBackground)));
		colorSerieB = colorFromGdk(Config.ColorBackground); //but note if we are using system colors, this will not match
	}

	protected override void findMaximums()
	{
		foreach(PointF p in pointA_l)
			if(p.Y > maxY)
				maxY = p.Y;
		foreach(PointF p in pointB_l)
			if(p.Y > maxY)
				maxY = p.Y;

		//points X start at 1
		minX = 0;
		maxX = pointA_l.Count + .5; //pointA_l and pointB_l have same length

		//bars Y have 0 at bottom
		minY = 0;
	}

	//note pointA_l and pointB_l have same length
	protected override void plotBars ()
	{
                //calculate separation between series and bar width
                double distanceBetweenCols = (graphWidth - (leftMargin+rightMargin))/maxX;

                double barWidth = .4*distanceBetweenCols;
                double barDesplLeft = .5*barWidth;
		//double valueABSep = barWidth / 4.0;
		double valueABSep = 0;
		int resultFontHeight = getBarsResultFontHeight (barWidth*1.5);

		List<Point3F> resultOnBarA_l = new List<Point3F>();
		List<Point3F> resultOnBarB_l = new List<Point3F>();

		for(int i = 0; i < pointA_l.Count; i ++)
		{
			PointF pA = pointA_l[i];
			PointF pB = pointB_l[i];

			if(pA.Y > 0)
			{
				double adjustX = -barDesplLeft;
				if(pB.Y > 0)
					adjustX = -2 * barDesplLeft -.5 * valueABSep;

				double x = (graphWidth - (leftMargin+rightMargin)) * pA.X/maxX - barDesplLeft + adjustX + leftMargin;
				double y = calculatePaintY(pA.Y);

				drawRoundedRectangle (true, x, y, barWidth, graphHeight -y -(topMargin+bottomMargin), 4, g, colorSerieA);
				resultOnBarA_l.Add(new Point3F(x + barWidth/2, y, pA.Y));
			}
			else
				resultOnBarA_l.Add(new Point3F(0, 0, 0));

			if(pB.Y > 0)
			{
				double adjustX = -barDesplLeft;
				if(pA.Y > 0)
					adjustX = .5 * valueABSep;

				double x = (graphWidth - (leftMargin+rightMargin)) * pB.X/maxX - barDesplLeft + adjustX + leftMargin;
				double y = calculatePaintY(pB.Y);

				drawRoundedRectangle (true, x, y, barWidth, graphHeight -y - (topMargin+bottomMargin), 4, g, colorSerieB);
				resultOnBarB_l.Add(new Point3F(x + barWidth/2, y, pB.Y));
			}
			else
				resultOnBarB_l.Add(new Point3F(0, 0, 0));

			//print text at bottom
			printText( (graphWidth - (leftMargin+rightMargin)) * pA.X/maxX + -barDesplLeft + leftMargin,
					graphHeight -(topMargin+bottomMargin) + textHeight/2, 0, textHeight,
					names_l[i], g, alignTypes.CENTER);
		}

		//result on bar painted here (after bars) to not have text overlapped by bars
		double pAyStart;
		for(int i = 0; i < pointA_l.Count; i ++)
		{
			pAyStart = -1;

			if(resultOnBarA_l[i].Y > 0)
				pAyStart = plotResultOnBar(resultOnBarA_l[i].X, resultOnBarA_l[i].Y,
						graphHeight -bottomMargin, resultOnBarA_l[i].Z, resultFontHeight, barWidth, -1);

			if(resultOnBarB_l[i].Y > 0)
				plotResultOnBar(resultOnBarB_l[i].X, resultOnBarB_l[i].Y,
						graphHeight -bottomMargin, resultOnBarB_l[i].Z, resultFontHeight, barWidth, pAyStart);
		}
	}

	public override void GraphDo (List<PointF> pointA_l, List<PointF> pointB_l,
			List<string> names_l, int fontHeightForBottomNames, int marginForBottomNames, string title)
	{
		LogB.Information("at CairoBars2HSeries.GraphDo");
		this.pointA_l = pointA_l;
		this.pointB_l = pointB_l;
		this.names_l = names_l;
		this.fontHeightForBottomNames = fontHeightForBottomNames;
		this.marginForBottomNames = marginForBottomNames;
		this.title = title;

		bottomMargin += marginForBottomNames;

                findMaximums();

		g.SetFontSize(textHeight);
		paintAxis(2);
		//paintAxis(2, textHeight -2);
		paintGrid(gridTypes.HORIZONTALLINES, true);
		//g.SetFontSize(textHeight);

		g.Color = black;
		plotBars();

		writeTitleAtTop ();

		endGraphDisposing(g);
	}
}

// ----------------------------------------------------------------

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

	public CairoBarsGuideManage (bool usePersonGuides, double sessionMAXAtSQL, double sessionAVGAtSQL, double sessionMINAtSQL,
			double personMAXAtSQLAllSessions, double personMAXAtSQL, double personAVGAtSQL, double personMINAtSQL)
	{
		l = new List<CairoBarsGuide> ();
		//int pos = 1;
		//int dist = 8;

		l.Add(new CairoBarsGuide(CairoBarsGuide.GuideEnum.SESSION_MAX, sessionMAXAtSQL,
					2, colorFromRGB(0,0,0), 'G', 12)); //(pos++)*dist));
		l.Add(new CairoBarsGuide(CairoBarsGuide.GuideEnum.SESSION_AVG, sessionAVGAtSQL,
					1, colorFromRGB(0,0,0), 'g', 12)); //(pos++)*dist));
		l.Add(new CairoBarsGuide(CairoBarsGuide.GuideEnum.SESSION_MIN, sessionMINAtSQL,
					1, colorFromRGB(0,0,0), 'g', 12)); //(pos++)*dist));

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
