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
 *  Copyright (C) 2004-2016   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Collections.Generic; //List
using Gtk;
using Cairo;

public static class JumpsProfileGraph
{
	public static void Do (List<JumpsProfileIndex> l_jpi, DrawingArea area) 
	{
		//1 create context
		Cairo.Context g = Gdk.CairoHelper.Create (area.GdkWindow);
		
		//2 clear DrawingArea (white)
		g.SetSourceRGB(1,1,1);
		g.Paint();

		//3 calculate sum
		double sum = 0;
		foreach(JumpsProfileIndex jpi in l_jpi)
			sum += jpi.Result;
		
		//4 prepare font
		g.SelectFontFace("Helvetica", Cairo.FontSlant.Normal, Cairo.FontWeight.Normal);
		int textHeight = 12;
		g.SetFontSize(textHeight);

		//5 plot arcs
		if(sum > 0 ) {
			double acc = 0; //accumulated
			foreach(JumpsProfileIndex jpi in l_jpi) {
				double percent = 2 * jpi.Result / sum; //*2 to be in range 0*pi - 2*pi
				plotArc(200, 200, 150, acc, acc + percent, g, jpi.Color);
				acc += percent;
			}
		}

		//6 draw legend at right
		int y = 50;
		//R seq(from=50,to=(350-24),length.out=5)
		//[1] 50 119 188 257 326 #difference is 69
		foreach(JumpsProfileIndex jpi in l_jpi) {
			drawRoundedRectangle (400,  y, 40, 24, 6, g, jpi.Color);
			
			double percent = 0;
			if(sum > 0)
				percent = 100 * jpi.Result / sum;

			printText(460,  y, 24, textHeight, Util.TrimDecimals(percent, 1) + jpi.Text, g);
			y += 69;
		}
	
		//7 print errors (if any)
		g.SetSourceRGB(0.5, 0, 0);
		y = 70;
		foreach(JumpsProfileIndex jpi in l_jpi) {
			printText(460,  y, 24, textHeight, jpi.ErrorMessage, g);
			y += 69;
		}
		
		//8 dispose
		g.GetTarget().Dispose ();
		g.Dispose ();
	}
	
	private static void plotArc (int centerx, int centery, int radius, double start, double end, 
			Cairo.Context g, Cairo.Color color) 
	{
		//pie chart
		g.MoveTo (centerx,  centery);
		g.Arc(centerx, centery, radius, start * Math.PI, end * Math.PI);
		g.ClosePath();
		g.SetSourceRGB(color.R, color.G, color.B);
		g.FillPreserve ();

		g.SetSourceRGB(0,0,0);
		g.LineWidth = 2;
		g.Stroke ();
	}
	
	private static void printText (int x, int y, int height, int textHeight, string text, Cairo.Context g) 
	{
		g.MoveTo(x, ((y+y+height)/2) + textHeight/2.0);
		g.ShowText(text);
	}

	//http://www.mono-project.com/docs/tools+libraries/libraries/Mono.Cairo/cookbook/	
	private static void drawRoundedRectangle (double x, double y, double width, double height, 
			double radius, Cairo.Context g, Cairo.Color color)
	{
		g.Save ();

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

	private static double min (params double[] arr)
	{
		int minp = 0;
		for (int i = 1; i < arr.Length; i++)
			if (arr[i] < arr[minp])
				minp = i;

		return arr[minp];
	}

	//save to png with http://www.mono-project.com/docs/tools+libraries/libraries/Mono.Cairo/tutorial/ stroke
	/*
	ImageSurface surface = new ImageSurface(Format.ARGB32, 120, 120);
	Context g = new Context(surface);
	// Examples are in 1.0 x 1.0 coordinate space
	g.Scale(120, 120);

	// Drawing code goes here
	g.LineWidth = 0.1;

	//g.Color = new Color(0, 0, 0); #deprecated, use this:
	g.SetSourceRGBA(0, 0, 0, 1);

	g.Rectangle(0.25, 0.25, 0.5, 0.5);
	g.Stroke();

	surface.WriteToPng("stroke.png");
	*/

}
