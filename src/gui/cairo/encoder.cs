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
 *  Copyright (C) 2022   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List
using Gtk;
using Cairo;


public class CairoGraphEncoderSignal : CairoXY
{
	//TODO: check if this two are doing anything
	private int points_list_painted;
	private int points_list_inertial_painted;
	//private bool doing;

	//regular constructor
	public CairoGraphEncoderSignal (DrawingArea area, string title)
	{
		this.area = area;
		this.title = title;
		this.colorBackground = colorFromGdk(Config.ColorBackground); //but note if we are using system colors, this will not match
		
		//doing = false;
		points_list_painted = 0;
		points_list_inertial_painted = 0;

		//need to be small because graphHeight could be 100,
		//if margins are big then calculatePaintY could give us reverse results
		outerMargin = 10;
		innerMargin = 0;
	}

	//separated in two methods to ensure endGraphDisposing on any return of the other method
	public override void DoSendingList (string font, bool isInertial,
			List<PointF> points_list, List<PointF> points_list_inertial,
			bool forceRedraw, PlotTypes plotType)
	{
		if(doSendingList (font, isInertial, points_list, points_list_inertial, forceRedraw, plotType))
			endGraphDisposing(g, surface, area.GdkWindow);
	}

	//return true if graph is inited (to dispose it)
	private bool doSendingList (string font, bool isInertial,
			List<PointF> points_list, List<PointF> points_list_inertial,
			bool forceRedraw, PlotTypes plotType)
	{
//		if(doing)
//			return false;

		//doing = true;
		bool maxValuesChanged = false;

		if(points_list != null)
		{
			maxValuesChanged = findPointMaximums(false, points_list);
			if(isInertial && points_list_inertial != null)
			{
				bool maxValuesChangedInertial = findPointMaximums(false, points_list_inertial);
				if(! maxValuesChanged && maxValuesChangedInertial)
					maxValuesChanged = true;
			}

			//show a vertical window of 100 mm (on inertial -+100 mm)
			//LogB.Information(string.Format("minY: {0}, maxY: {1}", minY, maxY));
			if(maxY < 100)
				maxY = 100; //to be able to graph at start when all the points are 0
			if(isInertial && minY > -100)
				minY = -100;
		}

		bool graphInited = false;
		if( maxValuesChanged || forceRedraw ||
				(points_list != null && points_list.Count != points_list_painted) ||
				(points_list_inertial != null && points_list_inertial.Count != points_list_inertial_painted)
				)
		{
			initGraph( font, 1, (maxValuesChanged || forceRedraw) );
			graphInited = true;
			points_list_painted = 0;
			points_list_inertial_painted = 0;
		}

		//do not draw axis at the moment (and it is not in 0Y right now)
		//if(maxValuesChanged || forceRedraw)
		//	paintAxis();

		if( points_list == null || points_list.Count == 0 ||
				(isInertial && (points_list_inertial == null || points_list_inertial.Count == 0)) )
			return graphInited;

		//fix an eventual crash on g.LineWidth below
		if(g == null || ! graphInited)
			return false;

		//this try/catch is an extra precaution
		try {
			g.LineWidth = 1;
		} catch {
			LogB.Information("Catched on CairoGraphEncoderSignal soSendingList() g.LineWidth");
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
			//on inertial draw person on 3 px, disk on 1
			if(isInertial)
				g.LineWidth = 2;

			/*
			ChronoDebug cDebug = new ChronoDebug("ChronoDebug plotRealPoints for n points: " + (points_list.Count - startAt).ToString());
			cDebug.Start();
			cDebug.Add("calling fast op");
			*/

			plotRealPoints(plotType, points_list, startAt, true); //fast (but the difference is very low)

			/*
			cDebug.Add("calling slow op");
			plotRealPoints(plotType, points_list, startAt, false); //slow?
			cDebug.StopAndPrint();
			*/

			points_list_painted = points_list.Count;
		}

		if( isInertial &&
				(maxValuesChanged || forceRedraw || points_list_inertial.Count != points_list_inertial_painted) )
		{
			g.LineWidth = 1;
			plotRealPoints(plotType, points_list_inertial, startAt, true); //fast
			points_list_inertial_painted = points_list_inertial.Count;
		}

		//doing = false;
		return true;
	}

	protected override void writeTitle()
	{
	}
}
