
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
 *  Copyright (C) 2021   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using System.Collections.Generic; //List
using Cairo;

public class CairoRunDoubleContacts : CairoGeneric
{
	private DrawingArea area;
	private Cairo.Context g;
	private Cairo.Color black;
	private int rightMargin = 35; //TODO: on windows should be 55
	private int bottomMargin = 0;

	public CairoRunDoubleContacts (Gtk.DrawingArea area, string font)
	{
		this.area = area;
		this.font = font;

		initGraph();
	}

	//from CairoRadial
	private void initGraph()
	{
		//1 create context
		g = Gdk.CairoHelper.Create (area.GdkWindow);
		
		//2 clear DrawingArea (white)
		g.SetSourceRGB(1,1,1);
		g.Paint();

		graphWidth = area.Allocation.Width;// -margin;
		graphHeight = area.Allocation.Height;// -margin;
		LogB.Information(string.Format("graphWidth: {0}" , graphWidth));

		textHeight = 12;
		/*
		if(graphWidth < 300 || graphHeight < 300)
			textHeight = 10;
		else if(graphWidth > 1200 || graphHeight > 1000)
			textHeight = 16;
		*/

		g.SetSourceRGB(0,0,0);
		g.LineWidth = 1;

		//4 prepare font
		g.SelectFontFace(font, Cairo.FontSlant.Normal, Cairo.FontWeight.Normal);
		g.SetFontSize(textHeight);

		black = colorFromRGB(0,0,0);
		//gray = colorFromRGB(99,99,99); //gray99
		g.Color = black;
	}

	public void GraphDo (List<RunPhaseTimeListObject> runPTLInListForPainting,
			double timeTotal, double timeTotalWithExtraPTL, double negativePTLTime,
			bool drawStart, bool drawEnd)
	{
		LogB.Information("CONTACT CHUNKS at Cairo");
		g.Color = black;
		double lastChunkStart = 0;
		int chunkMargins = 4;

		foreach (RunPhaseTimeListObject inPTL in runPTLInListForPainting)
		{
			LogB.Information("inPTL: " + inPTL.ToString());
			double xStart = rightMargin + (graphWidth - 2*rightMargin) *
					(inPTL.tcStart + negativePTLTime) / timeTotalWithExtraPTL;
			LogB.Information(string.Format("xStart parts: {0}, {1}, {2}",
					inPTL.tcStart, negativePTLTime, timeTotalWithExtraPTL));

			double xEnd = rightMargin + (graphWidth - 2*rightMargin) *
					(inPTL.tcEnd + negativePTLTime) / timeTotalWithExtraPTL;
			LogB.Information(string.Format("xEnd parts: {0}, {1}, {2}",
					inPTL.tcEnd, negativePTLTime, timeTotalWithExtraPTL));

			g.Rectangle(xStart, graphHeight-bottomMargin-4, xEnd-xStart, 4);
			g.Fill();

			LogB.Information(string.Format("xStart: {0}, yTop: {1}, width: {2}, height: {3}",
				xStart, graphHeight-bottomMargin-4, xEnd-xStart, 4));

			//manage chunks indications
			if(inPTL.phase == RunPhaseTimeListObject.Phases.START)
			{
				//draw the vertical start line
				g.MoveTo (xStart - chunkMargins, graphHeight-bottomMargin -4);
				g.LineTo (xStart - chunkMargins, graphHeight-bottomMargin -(4 + chunkMargins));
				g.Stroke();
				lastChunkStart = xStart;
				LogB.Information("runPTL draw start");
			}
			else if(inPTL.phase == RunPhaseTimeListObject.Phases.END)
			{
				//draw the vertical end line
				g.MoveTo(xEnd + chunkMargins, graphHeight-bottomMargin -4);
				g.LineTo(xEnd + chunkMargins, graphHeight-bottomMargin -(4 + chunkMargins));
				g.Stroke();

				//draw the horizontal start-end line
				g.MoveTo(lastChunkStart - chunkMargins, graphHeight-bottomMargin -(4 + chunkMargins));
				g.LineTo(xEnd + chunkMargins, graphHeight-bottomMargin -(4 + chunkMargins));
				g.Stroke();
				LogB.Information("runPTL draw end");
			}
		}

		if(drawStart)
		{
			//paint start vertical line
			double xStart2 = rightMargin + (graphWidth - 2*rightMargin) *
					(negativePTLTime) / timeTotalWithExtraPTL -1;
			g.MoveTo (xStart2 +1, 10);
			g.LineTo (xStart2 +1, graphHeight-bottomMargin-4);
			g.Stroke();

			printText(xStart2, 4, 0, 10, "Start", g, alignTypes.CENTER);
		}

		if(drawEnd)
		{
			//paint end vertical line
			double xEnd2 = rightMargin + (graphWidth - 2*rightMargin) *
					(timeTotal + negativePTLTime) / timeTotalWithExtraPTL;
			g.MoveTo (xEnd2, 10);
			g.LineTo (xEnd2, graphHeight-bottomMargin-4);
			g.Stroke();

			printText(xEnd2, 4, 0, 10, "End", g, alignTypes.CENTER);
		}
		//printText(graphWidth/2, 0, 0, 10, "testing top", g, alignTypes.CENTER);
		//printText(graphWidth/2, graphHeight-10, 0, 10, "testing bottom", g, alignTypes.CENTER);

		endGraphDisposing(g);
	}

}

