
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
 *  Copyright (C) 2025   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Collections.Generic; //List
using Gtk;
using Cairo;
using Mono.Unix;

public class CairoGraphWilight : CairoXY
{
	//private int points_l_painted; //never used
	private List<CairoGraphWilightTerminal> wt_l;

	public CairoGraphWilight (Gtk.DrawingArea area, string font)
	{
		initWilight (area, title);
	}

	/*
	public void GraphBlank()
	{
		initGraph();
		endGraphDisposing(g, surface, area.Window);
	}
	*/

	private void initWilight (DrawingArea area, string title)
	{
		this.area = area;
		this.title = title;
		//this.horizontal = horizontal;
		this.colorBackground = colorFromRGBA(Config.ColorBackground); //but note if we are using system colors, this will not match

		//points_l_painted = 0;


		yVariable = "";
		yUnits = "";

		xAtMaxY = 0;
		yAtMaxY = 0;
		xAtMinY = 0;
		yAtMinY = 0;

		gridNiceSeps = 7;
	}

	//separated in two methods to ensure endGraphDisposing on any return of the other method
	public void DoSendingList (string font,
			List<CairoGraphWilightTerminal> wt_l,
			bool forceRedraw)
	{
		if (doSendingList (font,
					wt_l,
						 //capturing,
						 forceRedraw//, plotType))
			))
				endGraphDisposing (g, surface, area.Window);
	}
			
	private bool doSendingList (string font, List<CairoGraphWilightTerminal> wt_l, bool forceRedraw)
	{
		this.wt_l = wt_l;

		bool maxValuesChanged = false;
	
		bool graphInited = false;
		if( maxValuesChanged || forceRedraw ||
				(wt_l != null)// && wt_l.Count != points_l_painted)
				)
		{
			colorCairoBackground = new Cairo.Color (1, 1, 1, 1);

			initGraph (font, 1, (maxValuesChanged || forceRedraw));
			graphInited = true;
			//points_l_painted = 0;
		}

		if( (wt_l == null || wt_l.Count == 0))// && idName_l == null)
		{
			if (! graphInited)
			{
				initGraph (font, 1, true);
				graphInited = true;
			}
			return graphInited;
		}

		//fix an eventual crash on g.LineWidth below
		if(g == null || ! graphInited)
			return false;

		//this try/catch is an extra precaution
		try {
			g.LineWidth = 1;
		} catch {
			LogB.Information("Catched on CairoGraphForceSensorSignal soSendingList() g.LineWidth");
			return graphInited;
		}

		pointsRadius = 8;

		/*
		//not used because need to do a real working conversion between cm and pixels that has same relation for X and for Y
		findPointMaximums (true,
				CairoGraphWilightTerminal.ListToPointF (wt_l),
				false);
		*/
		for (int i = 0; i < wt_l.Count; i ++)
		{
			CairoGraphWilightTerminal wt = wt_l[i];
			if (i == 0)
			{
				minX = wt.x;
				maxX = wt.x;
				minY = wt.y;
				maxY = wt.y;
			} else {
				if (wt.x < minX)
					minX = wt.x;
				if (wt.x > maxX)
					maxX = wt.x;
				if (wt.y < minY)
					minY = wt.y;
				if (wt.y > maxY)
					maxY = wt.x;
			}
		}
		absoluteMaxX = maxX;
		absoluteMaxY = maxY;

		//need to be small because graphHeight could be 100,
		//if margins are big then calculatePaintY could give us reverse results
		leftMargin = 40;
		rightMargin = 40;
		topMargin = 40;
		bottomMargin = 40;
		innerMargin = 0;

		adjustProportionByMargins ();
		doPlot ();

		return true;
	}

	// adjust margins in order to have same relation for X and Y
	private void adjustProportionByMargins ()
	{
		if (minX == maxX || minY == maxY)
			return;

		int graphWidthUseful = graphWidth -rightMargin -leftMargin;
		int graphHeightUseful = graphHeight -topMargin -bottomMargin;
		double realWidth = maxX - minX;
		double realHeight = maxY - minY;

		//TODO: note only centers are considered
		double widthRatio = UtilAll.DivideSafe (graphWidthUseful, realWidth);
		double heightRatio = UtilAll.DivideSafe (graphHeightUseful, realHeight);
		//LogB.Information (string.Format ("Width: graphWidthUseful: {0}, realWidth: {1}, ratio: {2}", graphWidthUseful, realWidth, widthRatio));
		//LogB.Information (string.Format ("Height: graphHeightUseful: {0}, realHeight: {1}, ratio: {2}", graphHeightUseful, realHeight, heightRatio));

		if (heightRatio > widthRatio)
		{
			//heightRatio = UtilAll.DivideSafe (graphHeightUseful, realHeight);
			double graphHeightUsefulFixed = heightRatio * realHeight;
			//assign the width ratio
			graphHeightUsefulFixed = widthRatio * realHeight;
			topMargin += Convert.ToInt32 ((graphHeightUseful - graphHeightUsefulFixed)/2);
			bottomMargin += Convert.ToInt32 ((graphHeightUseful - graphHeightUsefulFixed)/2);
		}
		else if (heightRatio < widthRatio)
		{
			double graphWidthUsefulFixed = widthRatio * realWidth;
			//assign the height ratio
			graphWidthUsefulFixed = heightRatio * realWidth;
			leftMargin += Convert.ToInt32 ((graphWidthUseful - graphWidthUsefulFixed)/2);
			rightMargin += Convert.ToInt32 ((graphWidthUseful - graphWidthUsefulFixed)/2);
		}
	}

	private void doPlot ()
	{
		if (wt_l != null && wt_l.Count > 0)
			foreach (CairoGraphWilightTerminal wt in wt_l)
				doPlotDrawTerminal (wt);
	}

	private void doPlotDrawTerminal (CairoGraphWilightTerminal wt)
	{
		Cairo.Color colorBorder = wt.ToColor;
		if (CairoUtil.ColorIsWhite (wt.ToColor))
			colorBorder = gray;

		int radius = 25;
		g.LineWidth = 1;
		drawCircle (g, calculatePaintX (wt.x),
				calculatePaintY (wt.y),
				radius, colorBorder, wt.ToColor);

		// if blink, then draw the half in black
		if (wt.Blinks)
		{
			g.SetSourceColor (black);
			g.Arc (calculatePaintX (wt.x), calculatePaintY (wt.y), 25, 0.75 * Math.PI, 1.75 * Math.PI);
			g.FillPreserve();
			g.SetSourceColor (black);
			g.Stroke ();
		}
		g.LineWidth = 1;

		if (wt.IsSensitive) //draw a line below the sensitive terminal
		{
			g.SetSourceColor (black);
			drawLine (calculatePaintX (wt.x) -radius, calculatePaintY (wt.y) + 1.25*radius,
				calculatePaintX (wt.x) +radius, calculatePaintY (wt.y) + 1.25*radius);
		}

		g.SetSourceColor (white);
		if (CairoUtil.ColorIsWhite (wt.ToColor)) //note if it blinks has white and black
			g.SetSourceColor (new Cairo.Color (.667, .400, .670)); //#aa6611 https://dev.to/finnhvman/which-colors-look-good-on-black-and-white-2pe6

		printText (calculatePaintX (wt.x), calculatePaintY (wt.y) -textHeight/2 +2, 0, textHeight +4,
				wt.codeNum.ToString (), g, alignTypes.CENTER);
	}

	protected override void writeTitle()
	{
	}

}

//codeNum, codeColor & center of each wilight terminal
public class CairoGraphWilightTerminal
{
	public int codeNum; //currently from 0 to 12
	public int codeColor; //powers of 2 (0 - 512)
	public double x; // (cm)
	public double y; // (cm)

	public CairoGraphWilightTerminal (int codeNum, int codeColor, PointF center)
	{
		this.codeNum = codeNum;
		this.codeColor = codeColor;
		this.x = center.X;
		this.y = center.Y;
	}

	public static List<PointF> ListToPointF (List<CairoGraphWilightTerminal> wt_l)
	{
		List<PointF> p_l = new List<PointF> ();
		foreach (CairoGraphWilightTerminal wt in wt_l)
			p_l.Add (new PointF (wt.x, wt.y));

		return p_l;
	}

	public bool Blinks {
		get {
			return ((codeColor & 32) != 0);
		}
	}

	public bool IsSensitive {
		get {
			return (! Util.IsEven (codeColor));
		}
	}

	public Cairo.Color ToColor {
		get	{
			int redCode = 2;
			int greenCode = 4;
			int blueCode = 8;
			int redBit = 0;
			int greenBit = 0;
			int blueBit = 0;

			for (int i = 0; i < 4; i++)
			{
				int mask = 1 << i;
				if ((codeColor & mask) != 0)
				{
					if (mask == redCode)
						redBit = 1;
					else if (mask == greenCode)
						greenBit = 1;
					else if (mask == blueCode)
						blueBit = 1;
				}
			}

			return (new Cairo.Color (1 * redBit, 1 * greenBit, 1 * blueBit));
		}
	}

}

