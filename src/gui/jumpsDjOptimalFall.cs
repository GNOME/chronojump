
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
 *  Copyright (C) 2004-2019   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Collections.Generic; //List
using Gtk;
using Cairo;

public static class JumpsDjOptimalFallGraph
{
	public static void Do (List<Point> point_l, double[] coefs, double xAtMMaxY, //x at Model MaxY
			double pointsMaxValue, DrawingArea area, string title, string date)
	{
		LogB.Information("at JumpsDjOptimalFallGraph.Do");
		//1 create context
		Cairo.Context g = Gdk.CairoHelper.Create (area.GdkWindow);
		
		//2 clear DrawingArea (white)
		g.SetSourceRGB(1,1,1);
		g.Paint();

		int graphWidth = Convert.ToInt32(area.Allocation.Width *.8); //in the future check this is not bigger than area widt

		//for all sides
		int outerMargins = 30; //blank space outside the axis
		int innerMargins = 30; //space between the axis and the real coordinates
		int totalMargins = outerMargins + innerMargins;

		/*
                //calculate separation between series and bar width
                int distanceBetweenCols = Convert.ToInt32((graphWidth - rightMargin)*(1+.5)/point_l.Count) -
                        Convert.ToInt32((graphWidth - rightMargin)*(0+.5)/point_l.Count);

                int tctfSep = Convert.ToInt32(.3*distanceBetweenCols);
                int barWidth = Convert.ToInt32(.3*distanceBetweenCols);
                int barDesplLeft = Convert.ToInt32(.5*barWidth);
		*/

		g.SetSourceRGB(0,0,0);
		g.LineWidth = 2;

		//4 prepare font
		g.SelectFontFace("Helvetica", Cairo.FontSlant.Normal, Cairo.FontWeight.Normal);
		int textHeight = 12;
		g.SetFontSize(textHeight);

		if(pointsMaxValue == 0)
		{
			g.SetSourceRGB(0,0,0);
			g.SetFontSize(16);
			printText(100, 100, 24, textHeight, "need points", g, false);
			g.GetTarget().Dispose ();
			g.Dispose ();
			return;
		}

		Cairo.Color red = colorFromRGB(200,0,0);
		Cairo.Color blue = colorFromRGB(178, 223, 238); //lightblue

		/*
		int i = 10;
		int count = 0;
		//note p.X is jump fall and p.Y jump height
		//TODO: maybe this will be for a legend, because the graph wants X,Y points
		foreach(Point p in point_l)
		{
			int x = Convert.ToInt32((graphWidth - rightMargin)*(count+.5)/point_l.Count)-barDesplLeft;
			int y = calculatePaintY(Convert.ToDouble(p.X), area.Allocation.Height, pointsMaxValue, 0, topMargin, bottomMargin + bottomAxis);

			LogB.Information(string.Format("red: {0}, {1}, {2}, {3}", Convert.ToDouble(p.X), area.Allocation.Height, pointsMaxValue, y));
			drawRoundedRectangle (x, y, barWidth, area.Allocation.Height - y, 4, g, red);

			x = Convert.ToInt32((graphWidth - rightMargin)*(count+.5)/point_l.Count)-barDesplLeft+tctfSep;
			y = calculatePaintY(Convert.ToDouble(p.Y), area.Allocation.Height, pointsMaxValue, 0, topMargin, bottomMargin + bottomAxis);

			LogB.Information(string.Format("blue: {0}, {1}, {2}, {3}", Convert.ToDouble(p.Y), area.Allocation.Height, pointsMaxValue, y));
			drawRoundedRectangle (x, y, barWidth, area.Allocation.Height -y, 4, g, blue);

			count ++;
		}
		*/

		LogB.Information(string.Format("coef length:{0}", coefs.Length));
		if(coefs.Length == 3)
		{
			double minX = 1000000;
			double maxX = 0;
			double minY = 1000000;
			double maxY = 0;
			double xgraph = 0;
			double ygraph = 0;

			foreach(Point p in point_l)
			{
				if(p.X < minX)
					minX = p.X;
				if(p.X > maxX)
					maxX = p.X;
				if(p.Y < minY)
					minY = p.Y;
				if(p.Y > maxY)
					maxY = p.Y;
			}

			double yAtMMaxY = coefs[0] + coefs[1]*xAtMMaxY + coefs[2]*Math.Pow(xAtMMaxY,2);
			double absoluteMaxY = yAtMMaxY;
			if(maxY > absoluteMaxY)
				absoluteMaxY = maxY;

			//paint axis
			g.MoveTo(outerMargins, outerMargins);
			g.LineTo(outerMargins, area.Allocation.Height - outerMargins);
			g.LineTo(graphWidth - outerMargins, area.Allocation.Height - outerMargins);
			g.Stroke ();
			printText(2, Convert.ToInt32(outerMargins/2), 0, textHeight, "Height (cm)", g, false);
			printText(graphWidth - Convert.ToInt32(outerMargins/2), area.Allocation.Height - outerMargins, 0, textHeight, "Fall (cm)", g, false);

			//paint grid: horizontal, vertical
			paintCairoGrid (minY, absoluteMaxY, 5, graphWidth, area.Allocation.Height, true,
					outerMargins, innerMargins, g, textHeight);
			paintCairoGrid (minX, maxX, 5, graphWidth, area.Allocation.Height, false,
					outerMargins, innerMargins, g, textHeight);

			//plot predicted line (model)
			bool firstValue = false;
			double minMax50Percent = (minX + maxX)/2;
			double xgraphOld = 0;
			for(double x = minX - minMax50Percent; x < maxX + minMax50Percent; x += (maxX-minX)/200)
			{
				xgraph = calculatePaintX(
						( x ),
						graphWidth, maxX, minX, totalMargins, totalMargins);

				//do not plot two times the same x point
				if(xgraph == xgraphOld)
					continue;
				xgraphOld = xgraph;

				ygraph = calculatePaintY(
						( coefs[0] + coefs[1]*x + coefs[2]*Math.Pow(x,2) ),
						area.Allocation.Height, absoluteMaxY, minY, totalMargins, totalMargins);

				//do not plot line outer the axis
				if(
						xgraph < outerMargins || xgraph > graphWidth - outerMargins ||
						ygraph < outerMargins || ygraph > area.Allocation.Height - outerMargins )
					continue;

				if(! firstValue)	
					g.LineTo(xgraph, ygraph);

				g.MoveTo(xgraph, ygraph);
				firstValue = false;
			}
			g.Stroke ();

			//plot real points
			foreach(Point p in point_l)
			{
				xgraph = calculatePaintX(
						( p.X ),
						graphWidth, maxX, minX, totalMargins, totalMargins);
				ygraph = calculatePaintY(
						( p.Y ),
						area.Allocation.Height, absoluteMaxY, minY, totalMargins, totalMargins);
				g.MoveTo(xgraph+6, ygraph);
				g.Arc(xgraph, ygraph, 6.0, 0.0, 2.0 * Math.PI); //full circle
				g.Color = blue;
				g.FillPreserve();
				g.SetSourceRGB(0, 0, 0);
				g.Stroke ();

				/*
				//print X, Y of each point
				printText(xgraph, area.Allocation.Height - Convert.ToInt32(bottomMargin/2), 0, textHeight, Util.TrimDecimals(p.X, 2), g, true);
				printText(Convert.ToInt32(leftMargin/2), ygraph, 0, textHeight, Util.TrimDecimals(p.Y, 2), g, true);
				*/
			}

			//plot predicted max point
			xgraph = calculatePaintX(xAtMMaxY, graphWidth, maxX, minX, totalMargins, totalMargins);
			ygraph = calculatePaintY(yAtMMaxY, area.Allocation.Height, absoluteMaxY, minY, totalMargins, totalMargins);

			//print X, Y of maxY
			//at axis
			g.Save();
			g.SetDash(new double[]{14, 6}, 0);
			g.MoveTo(xgraph, area.Allocation.Height - outerMargins);
			g.LineTo(xgraph, ygraph);
			g.LineTo(outerMargins, ygraph);
			g.Stroke ();
			g.Restore();

			/*
			printText(xgraph, area.Allocation.Height - Convert.ToInt32(bottomMargin/2), 0, textHeight, Util.TrimDecimals(xAtMMaxY, 2), g, true);
			printText(Convert.ToInt32(leftMargin/2), ygraph, 0, textHeight, Util.TrimDecimals(
					absoluteMaxY, 2), g, true);
			*/

			//at right
			printText(graphWidth + outerMargins, Convert.ToInt32(area.Allocation.Height/2) - textHeight*2, 0, textHeight, "Optimal values:", g, false);
			printText(graphWidth + outerMargins, Convert.ToInt32(area.Allocation.Height/2),      0, textHeight, "Fall: " + Util.TrimDecimals(xAtMMaxY, 2) + " cm", g, false);
			printText(graphWidth + outerMargins, Convert.ToInt32(area.Allocation.Height/2) + textHeight*2, 0, textHeight, "Jump height: " + Util.TrimDecimals(yAtMMaxY, 2) + " cm", g, false);

			g.MoveTo(xgraph+8, ygraph);
			g.Arc(xgraph, ygraph, 8.0, 0.0, 2.0 * Math.PI); //full circle
			g.Color = red;
			g.FillPreserve();
			g.SetSourceRGB(0, 0, 0);
			g.Stroke ();
		}


		//dispose
		g.GetTarget().Dispose ();
		g.Dispose ();
	}

	private static void paintCairoGrid (double min, double max, int seps, int horizontalSize, int verticalSize, bool horiz,
			int outerMargins, int innerMargins, Cairo.Context g, int textHeight)
	{
		//show 5 steps positive, 5 negative (if possible)
		int temp = Convert.ToInt32(Util.DivideSafe(max - min, seps));
		int step = temp;

		//to have values multiples than 10, 100 ...
		if(step <= 10)
			step = temp;
		else if(step <= 100)
			step = temp - (temp % 10);
		else if(step <= 1000)
			step = temp - (temp % 100);
		else if(step <= 10000)
			step = temp - (temp % 1000);
		else //if(step <= 100000)
			step = temp - (temp % 10000);

		//fix crash when no force
		if(step == 0)
			step = 1;

		g.Save();
		g.SetDash(new double[]{1, 2}, 0);
		// i <= max*1.5 to allow to have grid just above the maxpoint if it's below innermargins
		// see: if(ytemp < outerMargins) continue;
		for(int i = Convert.ToInt32(min); i <= max *1.5 ; i += step)
		{
			//LogB.Information("i: " + i.ToString());
			if(horiz)
			{
				int ytemp = Convert.ToInt32(calculatePaintY(i, verticalSize, max, min, outerMargins + innerMargins, outerMargins + innerMargins));
				if(ytemp < outerMargins)
					continue;
				g.MoveTo(outerMargins, ytemp);
				g.LineTo(horizontalSize - outerMargins, ytemp);
				printText(Convert.ToInt32(outerMargins/2), ytemp, 0, textHeight, i.ToString(), g, true);
			} else {
				int xtemp = Convert.ToInt32(calculatePaintX(i, horizontalSize, max, min, outerMargins + innerMargins, outerMargins + innerMargins));
				if(xtemp > horizontalSize)
					continue;
				g.MoveTo(xtemp, verticalSize - outerMargins);
				g.LineTo(xtemp, outerMargins);
				printText(xtemp, verticalSize - Convert.ToInt32(outerMargins/2), 0, textHeight, i.ToString(), g, true);
			}
		}
		g.Stroke ();
		g.Restore();
	}

	private static double calculatePaintX(double currentValue, int ancho, double maxValue, double minValue, int rightMargin, int leftMargin)
	{
                return leftMargin + (currentValue - minValue) * (ancho - rightMargin - leftMargin) / (maxValue - minValue);
        }

	private static double calculatePaintY(double currentValue, int alto, double maxValue, double minValue, int topMargin, int bottomMargin)
	{
                return alto - bottomMargin - ((currentValue - minValue) * (alto - topMargin - bottomMargin) / (maxValue - minValue));
        }

	//TODO: inherit this
	private static Cairo.Color colorFromRGB(int red, int green, int blue)
	{
		return new Cairo.Color(red/256.0, green/256.0, blue/256.0);
	}

	//TODO: inherit this
	private static void printText (int x, int y, int height, int textHeight, string text, Cairo.Context g, bool centered)
	{
		int moveToLeft = 0;
		if(centered)
		{
			Cairo.TextExtents te;
			te = g.TextExtents(text);
			moveToLeft = Convert.ToInt32(te.Width/2);
		}
		g.MoveTo( x - moveToLeft, ((y+y+height)/2) + textHeight/2 );
		g.ShowText(text);
	}

	/*
	//TODO: do not use this
	private static void drawRoundedRectangle (double x, double y, double width, double height, 
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
		g.LineTo (x + width, y + height - radius);
		g.Arc (x + width - radius, y + height - radius, radius, 0, Math.PI / 2);
		g.LineTo (x + radius, y + height);
		g.Arc (x + radius, y + height - radius, radius, Math.PI / 2, Math.PI);
		g.ClosePath ();
		g.Restore ();
		
		g.SetSourceRGB(color.R, color.G, color.B);
		g.FillPreserve ();
		g.SetSourceRGB(0, 0, 0);
		g.LineWidth = 2;
		g.Stroke ();	
	}

	//TODO: inherit this
	private static double min (params double[] arr)
	{
		int minp = 0;
		for (int i = 1; i < arr.Length; i++)
			if (arr[i] < arr[minp])
				minp = i;

		return arr[minp];
	}
	*/
}
