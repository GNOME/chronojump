
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
	public static void Do (List<Point> point_l, double pointsMaxY, DrawingArea area, string title, string date)
	{
		//1 create context
		Cairo.Context g = Gdk.CairoHelper.Create (area.GdkWindow);
		
		//2 clear DrawingArea (white)
		g.SetSourceRGB(1,1,1);
		g.Paint();

		//4 prepare font
		g.SelectFontFace("Helvetica", Cairo.FontSlant.Normal, Cairo.FontWeight.Normal);
		int textHeight = 12;
		g.SetFontSize(textHeight);

		if(pointsMaxY == 0)
		{
			g.SetSourceRGB(0,0,0);
			g.SetFontSize(16);
			printText(100, 100, 24, textHeight, "need points", g, false);
			g.GetTarget().Dispose ();
			g.Dispose ();
			return;
		}

		Cairo.Color color = colorFromRGB(101,86,67);
		int i = 10;
		foreach(Point p in point_l)
		{
			drawRoundedRectangle (0,  i , p.X, 20, 4, g, color);
			drawRoundedRectangle (10,  i+20 , p.Y, 20, 4, g, color);
			i+= 40;
		}

		//dispose
		g.GetTarget().Dispose ();
		g.Dispose ();
	}

	//TODO: inherit this
	private static Cairo.Color colorFromRGB(int red, int green, int blue) {
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
}
