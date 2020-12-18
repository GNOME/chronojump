
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
	private int textHeight = 12;
	private int margin = 20;
	private Gtk.DrawingArea area;
	private string font;
	private int graphWidth;
	private int graphHeight;
	private double minSide;
	private Cairo.Color black;
	private Cairo.Color colorArrow;
	private Cairo.Color gray;

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

		if(graphWidth > 1200 || graphHeight > 1000)
			textHeight = 16;

		g.SetSourceRGB(0,0,0);
		g.LineWidth = 1;

		//4 prepare font
		g.SelectFontFace(font, Cairo.FontSlant.Normal, Cairo.FontWeight.Normal);
		g.SetFontSize(textHeight);

		black = colorFromRGB(0,0,0);
		gray = colorFromRGB(99,99,99); //gray99
		colorArrow = colorFromGdk(Config.ColorBackground);

		g.Color = black;
		minSide = graphWidth;
		if(graphHeight < minSide)
			minSide = graphHeight;

		double tickLength = .1;
		for(int i = 0; i <= 20; i ++)
		{
			double iArc = (2*Math.PI / (maxPossibleValue +7)) * (i+17); //+7 for have the maxvalue at bottom right, +17 to have 0 on the bottom left

			//numbers
			printText(Convert.ToInt32(margin + graphWidth/2 + (minSide/2) * 1 * Math.Cos(iArc - Math.PI/2)),
					Convert.ToInt32(margin + graphHeight/2 + (minSide/2) * 1 * Math.Sin(iArc - Math.PI/2)),
					0, textHeight, i.ToString(), g, alignTypes.CENTER);

			//ticks
			g.MoveTo (
					margin + graphWidth/2 + (minSide/2) * .9 * Math.Cos(iArc - Math.PI/2),
					margin + graphHeight/2 + (minSide/2) * .9 * Math.Sin(iArc - Math.PI/2));
			g.LineTo (
					margin + graphWidth/2 + (minSide/2) * (.9-tickLength) * Math.Cos(iArc - Math.PI/2),
					margin + graphHeight/2 + (minSide/2) * (.9-tickLength) * Math.Sin(iArc - Math.PI/2));
			g.Stroke();

			if(tickLength == .1)
				tickLength = .05;
			else
				tickLength = .1;
		}

		/*
		//TEST:
		g.LineWidth = 2;
		graphLineFromCenter(3, colorArrow);
		printText(Convert.ToInt32(margin + graphWidth/2),
				Convert.ToInt32(margin + (.66 * graphHeight)),
				0, textHeight, "Speed: 3 m/s", g, alignTypes.CENTER);
				*/
	}

	//TODO: currently max is 20
	int maxPossibleValue = 20;
	double speedMax = 0;
	public void ResetSpeedMax()
	{
		speedMax = 0;
	}

	public void GraphSpeed(double speed)
	{
		if(speed > speedMax)
			speedMax = speed;

		initGraph();

		//g.SetSourceRGB(0.5, 0.5, 0.5);

		g.LineWidth = 2;
		graphLineFromCenter(speed, colorArrow);
		printText(Convert.ToInt32(margin + graphWidth/2),
				Convert.ToInt32(margin + (.66 * graphHeight)),
				0, textHeight, "Speed: " + Util.TrimDecimals(speed, 1) + " m/s", g, alignTypes.CENTER);

		if(speedMax > speed)
			graphLineFromCenter(speedMax, gray);

		endGraphDisposing();
	}

	private void graphLineFromCenter(double toValue, Cairo.Color color)
	{
		//double arc = (2*Math.PI / maxPossibleValue) * toValue;
		double arc = (2*Math.PI / (maxPossibleValue +7)) * (toValue+17); //+7 for have the maxvalue at bottom right, +17 to have 0 on the bottom left
		g.MoveTo(margin + graphWidth/2, margin + graphHeight/2);

		//thanks to: http://ralph-glass.homepage.t-online.de/clock/readme.html
		g.LineTo(margin + graphWidth/2 + (minSide/2) * .9 * Math.Cos(arc - Math.PI/2),
				margin + graphHeight/2 + (minSide/2) * .9 * Math.Sin(arc - Math.PI/2));

		g.Color = color;
		g.Stroke();
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
	protected Cairo.Color colorFromGdk(Gdk.Color color)
	{
		return new Cairo.Color(color.Red/65536.0, color.Green/65536.0, color.Blue/65536.0);
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

