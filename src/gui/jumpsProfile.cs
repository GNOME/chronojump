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
	}

	public void Graph (DrawingArea area) 
	{
		Cairo.Context g = Gdk.CairoHelper.Create (area.GdkWindow);

		//TODO: delete this		
		plotArc(100, 100, 50, 0, 0.25, g, new Cairo.Color(1,0,0));
		plotArc(100, 100, 50, .25, .75, g, new Cairo.Color(0,1,0));
		plotArc(100, 100, 50, .75, 2, g, new Cairo.Color(0,0,1));
	
		//palette: http://www.colourlovers.com/palette/7991/%28not_so%29_still_life	
		Cairo.Color color1 = colorFromRGB(90,68,102);
		Cairo.Color color2 = colorFromRGB(240,57,43);
		Cairo.Color color3 = colorFromRGB(254,176,20);
		Cairo.Color color4 = colorFromRGB(250,209,7);
		Cairo.Color color5 = colorFromRGB(235,235,207);
		
		//TODO: use indexes
		plotArc(300, 200, 150, 0, 0.25, g, color1);
		plotArc(300, 200, 150, .25, .75, g, color2);
		plotArc(300, 200, 150, .75, 1.25, g, color3);
		plotArc(300, 200, 150, 1.25, 1.5, g, color4);
		plotArc(300, 200, 150, 1.5, 2, g, color5);

		int width = 40;
		int height = 24;
		//R seq(from=50,to=(350-24),length.out=5)
		//[1] 50 119 188 257 326 
		drawRoundedRectangle (500,  50, width, height, 3, g, color1);
		drawRoundedRectangle (500, 119, width, height, 4, g, color2);
		drawRoundedRectangle (500, 188, width, height, 5, g, color3);
		drawRoundedRectangle (500, 257, width, height, 6, g, color4);
		drawRoundedRectangle (500, 326, width, height, 7, g, color5);
	
		g.SelectFontFace("Helvetica", Cairo.FontSlant.Normal, Cairo.FontWeight.Normal);
		int textHeight = 12;
		g.SetFontSize(textHeight);
		
		printText(560,  50, height, textHeight, "Index 1", g);
		printText(560, 119, height, textHeight, "Index 2", g);
		printText(560, 188, height, textHeight, "Index 3", g);
		printText(560, 257, height, textHeight, "Index 4", g);
		printText(560, 326, height, textHeight, "Index 5", g);

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
		return new Cairo.Color(red/255.0, green/255.0, blue/255.0);
	}
}
