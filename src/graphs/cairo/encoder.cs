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


public class CairoGraphEncoderSignal : CairoXY
{
	protected List<PointF> points_l;
	protected List<PointF> points_l_inertial;
	protected int startAt;
	protected int marginAfterInSeconds;
	protected bool horizontal;

	//TODO: check if this two are doing anything
	private int points_l_painted;
	private int points_l_inertial_painted;
	//private bool doing;

	// to inherit
	public CairoGraphEncoderSignal ()
	{
	}

	// regular constructor
	public CairoGraphEncoderSignal (DrawingArea area, string title, bool horizontal)
	{
		initEncoder (area, title, horizontal);
	}

	// separated in two methods to ensure endGraphDisposing on any return of the other method
	public void DoSendingList (string font, bool isInertial,
			List<PointF> points_l, List<PointF> points_l_inertial,
			double videoPlayTimeInSeconds,
			bool forceRedraw, PlotTypes plotType)
	{
		this.points_l = points_l;
		this.points_l_inertial = points_l_inertial;

		if(doSendingList (font, isInertial, videoPlayTimeInSeconds, forceRedraw, plotType))
			endGraphDisposing(g, surface, area.Window);
	}

	protected void initEncoder (DrawingArea area, string title, bool horizontal)
	{
		this.area = area;
		this.title = title;
		this.horizontal = horizontal;
		this.colorBackground = colorFromRGBA (Config.ColorBackground); //but note if we are using system colors, this will not match
		
		//doing = false;
		points_l_painted = 0;
		points_l_inertial_painted = 0;

		//need to be small because graphHeight could be 100,
		//if margins are big then calculatePaintY could give us reverse results
		bottomMargin = 10;
		leftMargin = 10;
		topMargin = 10;
		rightMargin = 10;
		innerMargin = 0;
	}

	//return true if graph is inited (to dispose it)
	private bool doSendingList (string font, bool isInertial, double videoPlayTimeInSeconds, bool forceRedraw, PlotTypes plotType)
	{
//		if(doing)
//			return false;

		//doing = true;
		bool maxValuesChanged = false;

		if(points_l != null)
		{
			maxValuesChanged = findPointMaximums(false, points_l);
			if(isInertial && points_l_inertial != null)
			{
				double minYperson = minY;
				bool maxValuesChangedInertial = findPointMaximums(false, points_l_inertial);
				if (minYperson < minY)
					minY = minYperson;

				if(! maxValuesChanged && maxValuesChangedInertial)
					maxValuesChanged = true;
			}

			//show a vertical window of 100 mm (on inertial -+100 mm)
			//LogB.Information(string.Format("minY: {0}, maxY: {1}", minY, maxY));
			if(maxY < 100)
				maxY = 100; //to be able to graph at start when all the points are 0
			if(isInertial && minY > -100)
				minY = -100;

			// if vertical do have X in the center (at least at start)
			if (! horizontal)
			{
				if (minX > -50)
					minX = -50;
				if (maxX < 50)
					maxX = 50;
				if (absoluteMaxX < 50)
					absoluteMaxX = 50;
			}

			if (asteroids != null)
			{
				if (horizontal)
				{
					if (asteroids.MinY < minY)
						minY = asteroids.MinY;
					if (asteroids.MaxY > absoluteMaxY)
						absoluteMaxY = asteroids.MaxY;
				} else {
					if (asteroids.MinY < minX)
						minX = asteroids.MinY;
					if (asteroids.MaxY > absoluteMaxX)
						absoluteMaxX = asteroids.MaxY;
				}
			}
		}

		bool graphInited = false;
		if( maxValuesChanged || forceRedraw ||
				(points_l != null && points_l.Count != points_l_painted) ||
				(points_l_inertial != null && points_l_inertial.Count != points_l_inertial_painted)
				)
		{
			if (asteroids != null && asteroids.Dark)
				colorCairoBackground = new Cairo.Color (.005, .005, .05, 1);
			else
				colorCairoBackground = new Cairo.Color (1, 1, 1, 1);

			initGraph( font, 1, (maxValuesChanged || forceRedraw) );
			graphInited = true;
			points_l_painted = 0;
			points_l_inertial_painted = 0;
		}

		//do not draw axis at the moment (and it is not in 0Y right now)
		//if(maxValuesChanged || forceRedraw)
		//	paintAxis();

		if( points_l == null || points_l.Count == 0 ||
				(isInertial && (points_l_inertial == null || points_l_inertial.Count == 0)) )
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
		int sWidth = 10;
		int msWidth = sWidth * 1000;
		if (horizontal && absoluteMaxX < msWidth)
			absoluteMaxX = msWidth;
		else if (! horizontal && absoluteMaxY < msWidth)
			absoluteMaxY = msWidth;

		startAt = 0;
		if(points_l.Count - msWidth > 0)
		{
			startAt = points_l.Count - msWidth;
			if (horizontal)
				minX = points_l[startAt].X;
			else
				minY = points_l[startAt].Y;
		}
		if (asteroids != null)
		{
			marginAfterInSeconds = Convert.ToInt32 (.66 * sWidth);
			if (horizontal)
				startAt = configureTimeWindowHorizontal (points_l, sWidth, marginAfterInSeconds, 1000);
			else
				startAt = configureTimeWindowVertical (points_l, sWidth, marginAfterInSeconds, 1000);
		}

		if(maxValuesChanged || forceRedraw || points_l.Count != points_l_painted)
		{
			plotSpecific ();

			//on inertial draw person on 3 px, disk on 1
			if(isInertial)
				g.LineWidth = 2;

			/*
			ChronoDebug cDebug = new ChronoDebug("ChronoDebug plotRealPoints for n points: " + (points_l.Count - startAt).ToString());
			cDebug.Start();
			cDebug.Add("calling fast op");
			*/

			plotRealPoints(plotType, points_l, startAt, true); //fast (but the difference is very low)

			/*
			cDebug.Add("calling slow op");
			plotRealPoints(plotType, points_l, startAt, false); //slow?
			cDebug.StopAndPrint();
			*/

			points_l_painted = points_l.Count;
		}

		if( isInertial &&
				(maxValuesChanged || forceRedraw || points_l_inertial.Count != points_l_inertial_painted) )
		{
			g.LineWidth = 1;
			plotRealPoints(plotType, points_l_inertial, startAt, true); //fast
			points_l_inertial_painted = points_l_inertial.Count;
		}

		if (videoPlayTimeInSeconds > 0)
		{
			//LogB.Information ("signal videoPlayTimeInSeconds", videoPlayTimeInSeconds);
			LogB.Information ("last points_l.X", PointF.Last (points_l).X);
			if (horizontal)
			{
				g.MoveTo (calculatePaintX (videoPlayTimeInSeconds * 1000), topMargin);
				g.LineTo (calculatePaintX (videoPlayTimeInSeconds * 1000), graphHeight - bottomMargin);
			} else {
				g.MoveTo (leftMargin, calculatePaintY (videoPlayTimeInSeconds * 1000));
				g.LineTo (graphWidth - rightMargin, calculatePaintY (videoPlayTimeInSeconds * 1000));
			}
			g.Stroke ();
		}

		//doing = false;
		return true;
	}

	protected virtual void plotSpecific ()
	{
		//do nothing
	}

	protected override void writeTitle()
	{
	}

	public Asteroids PassAsteroids {
		set { asteroids = value; }
	}
}

// almost the same than: CairoGraphForceSensorSignalAsteroids
public class CairoGraphEncoderSignalAsteroids : CairoGraphEncoderSignal
{
	private double lastShot;
	private double lastPointUp;
	private int multiplier;

	public CairoGraphEncoderSignalAsteroids (DrawingArea area, string title, bool horizontal)
	{
		initEncoder (area, title, horizontal);
		multiplier = 1000; //encoder

		lastShot = 0;
		lastPointUp = 0; //each s 1 point up
	}

	protected override void plotSpecific ()
	{
		/* do not need as at end of capture and load, an R image is loaded
		if (! capturing)
			return;
			*/

		asteroidsPlot (points_l[points_l.Count -1], startAt, multiplier,
				marginAfterInSeconds, points_l, horizontal,
				ref lastShot, ref lastPointUp);
	}
}
