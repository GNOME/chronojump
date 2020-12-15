
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
 *  Copyright (C) 2020   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Collections.Generic; //List
using Gtk;
using Cairo;
using Mono.Unix;

public class CairoRadial
{
	private Cairo.Context g;
	private const int textHeight = 12;
	private int margin = 20;
	private Gtk.DrawingArea area;
	private string font;
	private int graphWidth;
	private int graphHeight;
	private Cairo.Color black;
	private Cairo.Color red;

	public CairoRadial (Gtk.DrawingArea area, string font)
	{
		this.area = area;
		this.font = font;

		initGraph();
		endGraphDisposing();
	}

	private void initGraph()
	{
		//1 create context
		g = Gdk.CairoHelper.Create (area.GdkWindow);
		
		//2 clear DrawingArea (white)
		g.SetSourceRGB(1,1,1);
		g.Paint();

		graphWidth = area.Allocation.Width - 2*margin;
		graphHeight = area.Allocation.Height - 2*margin;

		g.SetSourceRGB(0,0,0);
		g.LineWidth = 2;

		//4 prepare font
		g.SelectFontFace(font, Cairo.FontSlant.Normal, Cairo.FontWeight.Normal);
		g.SetFontSize(textHeight);

		black = colorFromRGB(0,0,0);
		//gray99 = colorFromRGB(99,99,99);
		//white = colorFromRGB(255,255,255);
		red = colorFromRGB(200,0,0);
		//blue = colorFromRGB(178, 223, 238); //lightblue
		//bluePlots = colorFromRGB(0, 0, 200);

		g.Color = black;
		double minSide = graphWidth;
		if(graphHeight < minSide)
			minSide = graphHeight;
		for(int i = 0; i < 20; i ++)
		{
			double iArc = (2*Math.PI / maxValue) * i;
			printText(Convert.ToInt32(margin + graphWidth/2 + (minSide/2) * 1 * Math.Cos(iArc - Math.PI/2)),
					Convert.ToInt32(margin + graphHeight/2 + (minSide/2) * 1 * Math.Sin(iArc - Math.PI/2)),
					0, textHeight, i.ToString(), g, alignTypes.CENTER);
		}
	}

	//TODO: currently max is 20
	//TODO: make this go from bottom left to bottom right like a car
	int maxValue = 20;
	public void GraphSpeed(double speed)
	{
		initGraph();

		double speedArc = (2*Math.PI / maxValue) * speed;

		//g.SetSourceRGB(0.5, 0.5, 0.5);
		g.MoveTo(margin + graphWidth/2, margin + graphHeight/2);

		double minSide = graphWidth;
		if(graphHeight < minSide)
			minSide = graphHeight;

		//thanks to: http://ralph-glass.homepage.t-online.de/clock/readme.html
		g.LineTo(margin + graphWidth/2 + (minSide/2) * .9 * Math.Cos(speedArc - Math.PI/2),
				margin + graphHeight/2 + (minSide/2) * .9 * Math.Sin(speedArc - Math.PI/2));
		g.Color = red;
		g.Stroke();

		endGraphDisposing();
	}

	//TODO: all this methods have to be shared with xy.cs

	protected void endGraphDisposing()
	{
		g.GetTarget().Dispose ();
		g.Dispose ();
	}

	//0 - 255
	private Cairo.Color colorFromRGB(int red, int green, int blue)
	{
		return new Cairo.Color(red/256.0, green/256.0, blue/256.0);
	}
	
	private enum alignTypes { LEFT, CENTER, RIGHT }
	private void printText (int x, int y, int height, int textHeight, string text, Cairo.Context g, alignTypes align)
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
}

