
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

public class CairoRadial : CairoGeneric
{
	private Cairo.Context g;
	private int textHeight = 12;
	private int margin = 1;
	private int offsetV = 6; //to move the graph vertically
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
	}

	public void GraphBlank()
	{
		initGraph();
		endGraphDisposing(g);
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
			printText(
					Convert.ToInt32(margin + graphWidth/2 + (minSide/2) * 1 * Math.Cos(iArc - Math.PI/2)),
					Convert.ToInt32(offsetV + margin + graphHeight/2 + (minSide/2) * 1 * Math.Sin(iArc - Math.PI/2)),
					0, textHeight, i.ToString(), g, alignTypes.CENTER);

			//ticks
			g.MoveTo (
					margin + graphWidth/2 + (minSide/2) * .9 * Math.Cos(iArc - Math.PI/2),
					offsetV + margin + graphHeight/2 + (minSide/2) * .9 * Math.Sin(iArc - Math.PI/2));
			g.LineTo (
					margin + graphWidth/2 + (minSide/2) * (.9-tickLength) * Math.Cos(iArc - Math.PI/2),
					offsetV + margin + graphHeight/2 + (minSide/2) * (.9-tickLength) * Math.Sin(iArc - Math.PI/2));
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

	//used while capturing
	public void GraphSpeedAndDistance(double speed, double distance)
	{
		if(speed > speedMax)
			speedMax = speed;

		initGraph();

		//g.SetSourceRGB(0.5, 0.5, 0.5);

		g.LineWidth = 2;
		graphLineFromCenter(speed, colorArrow);
		printText(
				Convert.ToInt32(margin + graphWidth/2),
				Convert.ToInt32(offsetV + margin + (.66 * graphHeight)),
				0, textHeight,
				"Speed: " + Util.TrimDecimals(speed, 1) + " m/s",
				g, alignTypes.CENTER);
		printText(
				Convert.ToInt32(margin + graphWidth/2),
				Convert.ToInt32(offsetV + margin + (.66 * graphHeight)) + 20,
				0, textHeight,
				"Distance: " + Util.TrimDecimals(distance, 3) + " m",
				g, alignTypes.CENTER);

		if(speedMax > speed)
			graphLineFromCenter(speedMax, gray);

		endGraphDisposing(g);
	}

	//used at end or capture or at load
	public void GraphSpeedMaxAndDistance(double speedMax, double distance)
	{
		initGraph();

		//g.SetSourceRGB(0.5, 0.5, 0.5);

		g.LineWidth = 2;
		printText(
				Convert.ToInt32(margin + graphWidth/2),
				Convert.ToInt32(offsetV + margin + (.66 * graphHeight)),
				0, textHeight,
				"Max Speed: " + Util.TrimDecimals(speedMax, 1) + " m/s",
				g, alignTypes.CENTER);
		printText(
				Convert.ToInt32(margin + graphWidth/2),
				Convert.ToInt32(offsetV + margin + (.66 * graphHeight)) + 20,
				0, textHeight,
				"Distance: " + Util.TrimDecimals(distance, 3) + " m",
				g, alignTypes.CENTER);

		graphLineFromCenter(speedMax, gray);

		endGraphDisposing(g);
	}

	private void graphLineFromCenter(double toValue, Cairo.Color color)
	{
		//double arc = (2*Math.PI / maxPossibleValue) * toValue;
		double arc = (2*Math.PI / (maxPossibleValue +7)) * (toValue+17); //+7 for have the maxvalue at bottom right, +17 to have 0 on the bottom left
		g.MoveTo(
				margin + graphWidth/2,
				offsetV + margin + graphHeight/2);

		//thanks to: http://ralph-glass.homepage.t-online.de/clock/readme.html
		g.LineTo(
				margin + graphWidth/2 + (minSide/2) * .9 * Math.Cos(arc - Math.PI/2),
				offsetV + margin + graphHeight/2 + (minSide/2) * .9 * Math.Sin(arc - Math.PI/2));

		g.Color = color;
		g.Stroke();
	}

}

