
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
	protected DrawingArea area;
	protected ImageSurface surface;
	protected Cairo.Context g;
	protected Cairo.Color black;
	protected Cairo.Color colorBackground;

	public CairoRunDoubleContacts () //for inheritance
	{
	}

	public CairoRunDoubleContacts (Gtk.DrawingArea area, string font)
	{
		this.area = area;
		this.font = font;

		rightMargin = 35; //TODO: on windows should be 55
		bottomMargin = 0;

		initGraph();
	}

	//from CairoRadial
	protected void initGraph()
	{
		//1 create context from area->surface (see xy.cs)
                surface = new ImageSurface(Format.RGB24, area.Allocation.Width, area.Allocation.Height);
                g = new Context (surface);
		
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

		colorBackground = colorFromRGBA(Config.ColorBackground);

		black = colorFromRGB(0,0,0);
		//gray = colorFromRGB(99,99,99); //gray99
		g.SetSourceColor(black);
	}

	public void GraphDo (
			List<RunPhaseTimeListObject> runPTLInListForPainting,
			double timeTotal,
			double timeTotalWithExtraPTL, double negativePTLTime)
	{
		LogB.Information("CONTACT CHUNKS at Cairo");
		double lastChunkStart = 0;
		int chunkMargins = 4;

		foreach (RunPhaseTimeListObject inPTL in runPTLInListForPainting)
		{
			double xStart = rightMargin + (graphWidth - 2*rightMargin) *
					(inPTL.tcStart + negativePTLTime) / timeTotalWithExtraPTL;

			double xEnd = rightMargin + (graphWidth - 2*rightMargin) *
					(inPTL.tcEnd + negativePTLTime) / timeTotalWithExtraPTL;

			g.SetSourceColor(colorBackground);
			g.Rectangle(xStart, 22, xEnd-xStart, 4);
			g.Fill();

			if(inPTL.photocellStart >= 0)
				printText(xStart, 30, 0, 10, inPTL.photocellStart.ToString(), g, alignTypes.CENTER);
			if(inPTL.photocellEnd >= 0)
				printText(xEnd, 30, 0, 10, inPTL.photocellEnd.ToString(), g, alignTypes.CENTER);

			g.SetSourceColor(black);
			//manage chunks indications
			if(inPTL.phase == RunPhaseTimeListObject.Phases.START)
			{
				//draw the vertical start line
				g.MoveTo (xStart - chunkMargins, 32);
				g.LineTo (xStart - chunkMargins, 40);
				g.Stroke();
				lastChunkStart = xStart;
			}
			else if(inPTL.phase == RunPhaseTimeListObject.Phases.END)
			{
				//draw the vertical end line
				g.MoveTo(xEnd + chunkMargins, 32);
				g.LineTo(xEnd + chunkMargins, 40);
				g.Stroke();

				//draw the horizontal start-end line
				g.MoveTo(lastChunkStart - chunkMargins, 40);
				g.LineTo(xEnd + chunkMargins, 40);
				g.Stroke();
			}
		}

		drawTracks (timeTotal, timeTotalWithExtraPTL, negativePTLTime);
		drawStartAndEnd (timeTotal, timeTotalWithExtraPTL, negativePTLTime);

		endGraphDisposing(g, surface, area.Window);
	}

	protected virtual void drawTracks (double timeTotal, double timeTotalWithExtraPTL, double negativePTLTime)
	{
		plotArrowPassingGraphPoints (g, colorBackground, true,
				rightMargin + (graphWidth - 2*rightMargin) * (negativePTLTime) / timeTotalWithExtraPTL,
				15,
				rightMargin + (graphWidth - 2*rightMargin) * (timeTotal + negativePTLTime) / timeTotalWithExtraPTL,
				15,
				true, true, 1);

		//draw track num
		double xTrackNum = rightMargin + (graphWidth - 2*rightMargin) *
			(timeTotal/2 + negativePTLTime) / timeTotalWithExtraPTL;
		printText(xTrackNum, 4, 0, 10, "1", g, alignTypes.CENTER);
	}

	protected void drawStartAndEnd (double timeTotal, double timeTotalWithExtraPTL, double negativePTLTime)
	{
		// 1) drawStart
		//paint start vertical line
		double xStart2 = rightMargin + (graphWidth - 2*rightMargin) *
			(negativePTLTime) / timeTotalWithExtraPTL -1;

		//on runInterval, this 3 lines will be done also above
		g.MoveTo (xStart2 +1, 10);
		g.LineTo (xStart2 +1, 25);
		g.Stroke();

		printText(xStart2, 4, 0, 10, "Start", g, alignTypes.RIGHT);

		// 2) drawEnd
		//paint end vertical line
		double xEnd2 = rightMargin + (graphWidth - 2*rightMargin) *
			(timeTotal + negativePTLTime) / timeTotalWithExtraPTL;

		//on runInterval, this 3 lines will be done also above
		g.MoveTo (xEnd2, 10);
		g.LineTo (xEnd2, 25);
		g.Stroke();

		printText(xEnd2, 4, 0, 10, "End", g, alignTypes.LEFT);
	}
}

public class CairoRunIntervalDoubleContacts : CairoRunDoubleContacts
{
	private string intervalTimesString;

	public CairoRunIntervalDoubleContacts (Gtk.DrawingArea area, string font, string intervalTimesString)
	{
		this.area = area;
		this.font = font;
		this.intervalTimesString = intervalTimesString;

		rightMargin = 35; //TODO: on windows should be 55
		bottomMargin = 0;

		initGraph();
	}

	protected override void drawTracks (double timeTotal, double timeTotalWithExtraPTL, double negativePTLTime)
	{
		LogB.Information("intervalTimesString is: " + intervalTimesString);
		string [] times = intervalTimesString.Split(new char[] {'='});
		double accumulated = 0;
		int trackCount = 0;
		foreach(string time in times)
		{
			/*
			//vertical line at end of track

			double xVert = rightMargin + (graphWidth - 2*rightMargin) *
				(Convert.ToDouble(time) + accumulated + negativePTLTime) / timeTotalWithExtraPTL;

			g.MoveTo (xVert, 5);
			g.LineTo (xVert, graphHeight-bottomMargin-4);
			g.Stroke();
			*/

			plotArrowPassingGraphPoints (g, colorBackground, true,
				rightMargin + (graphWidth - 2*rightMargin) * (accumulated + negativePTLTime) / timeTotalWithExtraPTL,
				15,
				rightMargin + (graphWidth - 2*rightMargin) * (Convert.ToDouble(time) + accumulated + negativePTLTime) / timeTotalWithExtraPTL,
				15,
				true, true, 1);

			//draw track num
			double xTrackNum = rightMargin + (graphWidth - 2*rightMargin) *
				(Convert.ToDouble(time)/2 + accumulated + negativePTLTime) / timeTotalWithExtraPTL;
			printText(xTrackNum, 4, 0, 10, (++ trackCount).ToString(), g, alignTypes.CENTER);

			accumulated += Convert.ToDouble(time);
		}
	}
}
