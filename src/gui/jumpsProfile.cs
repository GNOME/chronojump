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
using Gtk;
using Cairo;
using System.Collections.Generic; //List

public class JumpsProfileGraph
{
	private double index1;
	private double index2;
	private double index3;
	private double index4;
	private double index5;

	public JumpsProfileGraph() {
	}

	public void Calculate (int personID, int sessionID)
	{
		List<Double> l = SqliteStat.SelectChronojumpProfile(personID, sessionID);

		index1 = l[0];
		index2 = l[1];
		index3 = l[2];
		index4 = l[3];
		index5 = l[4];

		//indexes cannot be below 0. They ruin the graph
		//eg: SJ higher than CMJ
		if(index1 < 0)
			index1 = 0;
		if(index2 < 0)
			index2 = 0;
		if(index3 < 0)
			index3 = 0;
		if(index4 < 0)
			index4 = 0;
		if(index5 < 0)
			index5 = 0;
	}

	public void Graph (DrawingArea area) 
	{
		Cairo.Context g = Gdk.CairoHelper.Create (area.GdkWindow);
		
		//clear (white)
		g.SetSourceRGB(1,1,1);
		g.Paint();

		//palette: http://www.colourlovers.com/palette/7991/%28not_so%29_still_life	
		Cairo.Color color1 = colorFromRGB(90,68,102);
		Cairo.Color color2 = colorFromRGB(240,57,43);
		Cairo.Color color3 = colorFromRGB(254,176,20);
		Cairo.Color color4 = colorFromRGB(250,209,7);
		Cairo.Color color5 = colorFromRGB(235,235,207);

		double sum = index1 + index2 + index3 + index4 + index5;
		if(sum == 0)
			return;

		double acc = 0; //accumulated
		
		double percent = 2 * index1 / sum; //*2 to be in range 0*pi - 2*pi
		plotArc(200, 200, 150, acc, acc + percent, g, color1);

		acc += percent;
		percent = 2 * index2 / sum; //*2 to be in range 0*pi - 2*pi
		plotArc(200, 200, 150, acc, acc + percent, g, color2);

		acc += percent;
		percent = 2 * index3 / sum; //*2 to be in range 0*pi - 2*pi
		plotArc(200, 200, 150, acc, acc + percent, g, color3);

		acc += percent;
		percent = 2 * index4 / sum; //*2 to be in range 0*pi - 2*pi
		plotArc(200, 200, 150, acc, acc + percent, g, color4);

		acc += percent;
		percent = 2 * index5 / sum; //*2 to be in range 0*pi - 2*pi
		plotArc(200, 200, 150, acc, acc + percent, g, color5);

		int width = 40;
		int height = 24;
		//R seq(from=50,to=(350-24),length.out=5)
		//[1] 50 119 188 257 326 
		drawRoundedRectangle (400,  50, width, height, 3, g, color1);
		drawRoundedRectangle (400, 119, width, height, 4, g, color2);
		drawRoundedRectangle (400, 188, width, height, 5, g, color3);
		drawRoundedRectangle (400, 257, width, height, 6, g, color4);
		drawRoundedRectangle (400, 326, width, height, 7, g, color5);
	
		g.SelectFontFace("Helvetica", Cairo.FontSlant.Normal, Cairo.FontWeight.Normal);
		int textHeight = 12;
		g.SetFontSize(textHeight);
		
		printText(460,  50, height, textHeight, Util.TrimDecimals((100 * index1 / sum),1) + "% F. Maximum  SJ100% / DJa", g);
		printText(460, 119, height, textHeight, Util.TrimDecimals((100 * index2 / sum),1) + "% F. Explosive  (SJ - SJ100%) / Dja", g);
		printText(460, 188, height, textHeight, Util.TrimDecimals((100 * index3 / sum),1) + "% Hab. Elastic  (CMJ - SJ) / Dja", g);
		printText(460, 257, height, textHeight, Util.TrimDecimals((100 * index4 / sum),1) + "% Hab. Arms  (ABK - CMJ) / Dja", g);
		printText(460, 326, height, textHeight, Util.TrimDecimals((100 * index5 / sum),1) + "% F. Reactive-reflex  (DJa - ABK) / Dja", g);

		g.GetTarget().Dispose ();
		g.Dispose ();
	}
	
	private void plotArc(int centerx, int centery, int radius, double start, double end, 
			Cairo.Context g, Cairo.Color color) 
	{
		//pie chart
		g.MoveTo (centerx,  centery);
		g.Arc(centerx, centery, radius, start * Math.PI, end * Math.PI);
		g.ClosePath();
		g.Color = color;
		g.FillPreserve ();

		g.Color = new Cairo.Color (0,0,0);
		g.LineWidth = 2;
		g.Stroke ();
	}
	
	private void printText(int x, int y, int height, int textHeight, string text, Cairo.Context g) {
		g.MoveTo(x, ((y+y+height)/2) + textHeight/2.0);
		g.ShowText(text);
	}

	//http://www.mono-project.com/docs/tools+libraries/libraries/Mono.Cairo/cookbook/	
	private void drawRoundedRectangle (double x, double y, double width, double height, 
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
		
		g.Color = color;
		g.FillPreserve ();
		g.Color = new Cairo.Color (0, 0, 0);
		g.LineWidth = 2;
		g.Stroke ();	
	}

	private double min (params double[] arr)
	{
		int minp = 0;
		for (int i = 1; i < arr.Length; i++)
			if (arr[i] < arr[minp])
				minp = i;

		return arr[minp];
	}

	private Cairo.Color colorFromRGB(int red, int green, int blue) {
		return new Cairo.Color(red/256.0, green/256.0, blue/256.0);
	}

	//save to png with http://www.mono-project.com/docs/tools+libraries/libraries/Mono.Cairo/tutorial/ stroke
	/*
	ImageSurface surface = new ImageSurface(Format.ARGB32, 120, 120);
	Context cr = new Context(surface);
	// Examples are in 1.0 x 1.0 coordinate space
	cr.Scale(120, 120);

	// Drawing code goes here
	cr.LineWidth = 0.1;

	//cr.Color = new Color(0, 0, 0); #deprecated, use this:
	cr.SetSourceRGBA(0, 0, 0, 1);

	cr.Rectangle(0.25, 0.25, 0.5, 0.5);
	cr.Stroke();

	surface.WriteToPng("stroke.png");
	*/

}
