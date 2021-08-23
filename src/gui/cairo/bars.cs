
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
using Gtk;
using Cairo;

public abstract class CairoBars : CairoGeneric
{
	//used on construction
	protected List<PointF> point_l;
	protected List<string> names_l;

	protected DrawingArea area;
	protected string title;
	//protected string jumpType;
	//protected string runType;
	protected string date;
	protected Cairo.Color colorBackground;

	protected Cairo.Context g;
	protected int lineWidthDefault = 2;
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
	Cairo.Color blue;
	protected Cairo.Color bluePlots;
	protected Cairo.Color yellow;


	public virtual void Do(string font)
	{
	}

	protected void initGraph(string font, double widthPercent1)
	{
		initGraph(font, widthPercent1, true);
	}
	protected void initGraph(string font, double widthPercent1, bool clearDrawingArea)
	{
		this.font = font;
		LogB.Information("Font: " + font);

		outerMargins = 18; //blank space outside the axis.

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

	protected void findPointMaximums()
	{
		foreach(PointF p in point_l)
			if(p.Y > maxY)
				maxY = p.Y;

		//points X start at 1
		minX = 0;
		maxX = point_l.Count + 1;

		//bars Y have 0 at bottom
		minY = 0;
	}

	protected void paintAxis(int width)
	{
		g.LineWidth = width;
		g.MoveTo(outerMargins, outerMargins);
		g.LineTo(outerMargins, graphHeight - outerMargins);
		g.LineTo(graphWidth - outerMargins, graphHeight - outerMargins);
		g.Stroke ();

		printText(2, Convert.ToInt32(outerMargins/2), 0, textHeight, getYAxisLabel(), g, alignTypes.LEFT);
		printXAxisText();
		g.Stroke ();

		g.LineWidth = lineWidthDefault;
	}

	//this combined with paintVerticalGridLine is different on RaceAnalyzer
	protected virtual void printXAxisText()
	{
		printText(graphWidth - Convert.ToInt32(outerMargins/2), graphHeight - outerMargins, 0, textHeight,
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
		return outerMargins + (realX - minX) * UtilAll.DivideSafe(
				graphWidth - 2*outerMargins,
				maxX - minX);
        }
	protected override double calculatePaintY (double realY)
	{
                return graphHeight - outerMargins - UtilAll.DivideSafe(
				(realY - minY) * (graphHeight - 2*outerMargins),
				//maxY - minY);
				//have 10% extra margin on the top (highest values will be 10% far from max of the graph)
				1.1*maxY - minY);
        }

	protected void printText (double x, double y, double height, int textHeight,
			string text, Cairo.Context g, alignTypes align)
	{
		int moveToLeft = 0;
		if(align == alignTypes.CENTER || align == alignTypes.RIGHT)
		{
			Cairo.TextExtents te;
			te = g.TextExtents(text);
			
			if(align == alignTypes.CENTER)
				moveToLeft = Convert.ToInt32(te.Width/2);
			else
				moveToLeft = Convert.ToInt32(te.Width);
		}

		g.MoveTo( x - moveToLeft, ((y+y+height)/2) + textHeight/2 );
		g.ShowText(text);
	}

	protected void plotBars()
	{
                //calculate separation between series and bar width
                int distanceBetweenCols = Convert.ToInt32((graphWidth - 2*outerMargins)*(1+.5)/point_l.Count) -
                        Convert.ToInt32((graphWidth - 2*outerMargins)*(0+.5)/point_l.Count);

		//int tctfSep = Convert.ToInt32(.3*distanceBetweenCols);
                int barWidth = Convert.ToInt32(.5*distanceBetweenCols);
                int barDesplLeft = Convert.ToInt32(.5*barWidth);

		for(int i = 0; i < point_l.Count; i ++)
		{
			PointF p = point_l[i];

			double x = (graphWidth - 2*outerMargins) * (p.X-.5)/point_l.Count - barDesplLeft + outerMargins;
			double y = calculatePaintY(p.Y);

			drawRoundedRectangle (true, x, y, barWidth, graphHeight -y -outerMargins, 4, g, colorBackground);

			plotResultOnBar(x + barWidth/2, y, graphHeight -outerMargins, p.Y);

			//print the type at bottom
			printText(x + barWidth/2, graphHeight -outerMargins + textHeight/2, 0, textHeight,
					names_l[i], g, alignTypes.CENTER);

			/*
			x = Convert.ToInt32((graphWidth - outerMargins)*(count+.5)/point_l.Count)-barDesplLeft+tctfSep;
			y = calculatePaintY(p.Y);

			LogB.Information(string.Format("blue: {0}, {1}, {2}, {3}", Convert.ToDouble(p.Y), graphHeight, maxY, y));
			drawRoundedRectangle (x, y, barWidth, graphHeight -y - outerMargins, 4, g, blue);
			*/
		}
	}

	//adapted from http://www.mono-project.com/docs/tools+libraries/libraries/Mono.Cairo/cookbook/
	//bottomFlat means to have rounded only on top
	private static void drawRoundedRectangle (bool bottomFlat,
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

	private void plotResultOnBar(double x, double y, double alto, double result)
	{
		Cairo.TextExtents te;
		te = g.TextExtents(Util.TrimDecimals(result,2));

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

		LogB.Information(string.Format("y: {0}, alto: {1}, yStart: {2}", y, alto, yStart));

		if( (yStart + te.Height) > alto )
			yStart = alto - te.Height;

		if(textAboveBar)
			g.Color = white; //to just hide the horizontal grid
		else
			g.Color = yellow; //to have contrast with the bar

		g.Rectangle(x - te.Width/2, yStart-1, te.Width, te.Height+2);
		g.Fill();

		//write text
		g.Color = black;
		printText(x, yStart+te.Height/2, 0, Convert.ToInt32(te.Height),
			Util.TrimDecimals(result, 2), g, alignTypes.CENTER);
	}

	protected void writeTitleAtTop()
	{
		printText(graphWidth/2 + outerMargins, textHeight/2, 0, textHeight,
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
		g.LineWidth = 1; //to allow to be shown the red arrows on jumpsWeightFVProfile

		if(niceAutoValues)
			paintGridNiceAutoValues (g, minX, maxX, minY, maxY, 5, gridType);
		else
			paintGridInt (g, minX, maxX, minY, maxY, 1, gridType);
	}

}

public class CairoBarsJustTesting : CairoBars
{
	//constructor when there are no points
	public CairoBarsJustTesting (DrawingArea area, string font)
	{
		this.area = area;

		LogB.Information("constructor without points, area is null:" + (area == null).ToString());
		initGraph(font, 1); //.8 to have title at right

		endGraphDisposing(g);
	}

	//regular constructor
	public CairoBarsJustTesting (List<PointF> point_l, List<string> names_l, DrawingArea area, string title)
	{
		this.point_l = point_l;
		this.names_l = names_l;
		this.area = area;
		this.title = title;

		this.colorBackground = colorFromGdk(Config.ColorBackground); //but note if we are using system colors, this will not match
	}

	public override void Do(string font)
	{
		LogB.Information("at CairoBarsJustTesting.Do");

		textHeight = 14;

		initGraph(font, 1); //.8 if writeTextAtRight

                findPointMaximums();

		g.SetFontSize(textHeight -2);
		paintAxis(2);
		paintGrid(gridTypes.HORIZONTALLINES, true);
		g.SetFontSize(textHeight);

		g.Color = black;
		plotBars();

		writeTitleAtTop ();

		endGraphDisposing(g);
	}
}
