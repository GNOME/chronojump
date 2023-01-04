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
 *  Copyright (C) 2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List
using Gtk;
using Cairo;


public class CairoGraphForceSensorSignal : CairoXY
{
	private int points_list_painted;

	//regular constructor
	public CairoGraphForceSensorSignal (DrawingArea area, string title)
	{
		this.area = area;
		this.title = title;
		this.colorBackground = colorFromGdk(Config.ColorBackground); //but note if we are using system colors, this will not match
		
		//doing = false;
		points_list_painted = 0;

		//need to be small because graphHeight could be 100,
		//if margins are big then calculatePaintY could give us reverse results
		outerMargin = 10;
		innerMargin = 0;

		xAtMaxY = 0;
		yAtMaxY = 0;
		xAtMinY = 0;
		yAtMinY = 0;
	}

	//separated in two methods to ensure endGraphDisposing on any return of the other method
	public void DoSendingList (string font,
			List<PointF> points_list,
			bool forceRedraw, PlotTypes plotType)
	{
		if(doSendingList (font, points_list, forceRedraw, plotType))
			endGraphDisposing(g, surface, area.GdkWindow);
	}

	//similar to encoder method
	//return true if graph is inited (to dispose it)
	private bool doSendingList (string font,
			List<PointF> points_list,
			bool forceRedraw, PlotTypes plotType)
	{
		bool maxValuesChanged = false;

		if(points_list != null)
		{
			maxValuesChanged = findPointMaximums(false, points_list);

			//LogB.Information(string.Format("minY: {0}, maxY: {1}", minY, maxY));
			if(maxY < 100)
				maxY = 100; //to be able to graph at start when all the points are 0
		}

		bool graphInited = false;
		if( maxValuesChanged || forceRedraw ||
				(points_list != null && points_list.Count != points_list_painted)
				)
		{
			initGraph( font, 1, (maxValuesChanged || forceRedraw) );
			graphInited = true;
			points_list_painted = 0;
		}

		//do not draw axis at the moment (and it is not in 0Y right now)
		//if(maxValuesChanged || forceRedraw)
		//	paintAxis();

		if( points_list == null || points_list.Count == 0)
			return graphInited;

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
		pointsRadius = 1;

		//display this milliseconds on screen, when is higher, scroll
		int msWidth = 10000;
		if(absoluteMaxX < msWidth)
			absoluteMaxX = msWidth;

		int startAt = 0;
		if(points_list.Count - msWidth > 0)
		{
			startAt = points_list.Count - msWidth;
			minX = points_list[startAt].X;
		}

		if(maxValuesChanged || forceRedraw || points_list.Count != points_list_painted)
		{
			plotRealPoints(plotType, points_list, startAt, true); //fast (but the difference is very low)
			drawCircle (calculatePaintX (xAtMaxY), calculatePaintY (yAtMaxY), 8, red);
			drawCircle (calculatePaintX (xAtMinY), calculatePaintY (yAtMinY), 8, red);

			points_list_painted = points_list.Count;
		}

		return true;
	}

	protected override void writeTitle()
	{
	}
}
