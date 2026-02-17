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


public class CairoGraphEncoderSignal : CairoXY
{
	protected List<PointF> points_l;
	protected List<PointF> points_l_inertial;
	protected int startAt;
	protected int marginAfterInSeconds;
	protected bool horizontal;

	//TODO: check if this two are doing anything
	private bool capturing;
	private int points_l_painted;
	private int points_l_inertial_painted;
	private Gtk.ListStore encoderCaptureListStore;
	private string eccon;
	private int discardNReps;
	//private bool doing;
	private bool customAxisDispl;
	private int customAxisDisplMax;
	private int customAxisDisplMin;
	private List<EncoderBarsData> captureCurvesBarsData_l;

	// to inherit
	public CairoGraphEncoderSignal ()
	{
	}

	// regular constructor
	public CairoGraphEncoderSignal (DrawingArea area, string title,
		bool customAxisDispl, int customAxisDisplMax, int customAxisDisplMin, bool horizontal)
	{
		this.customAxisDispl = customAxisDispl;
		this.customAxisDisplMax = customAxisDisplMax;
		this.customAxisDisplMin = customAxisDisplMin;
		captureCurvesBarsData_l = new List<EncoderBarsData> ();

		initEncoder (area, title, horizontal);
	}

	// separated in two methods to ensure endGraphDisposing on any return of the other method
	public void DoSendingList (string font, bool capturing, bool isInertial,
			List<PointF> points_l, List<PointF> points_l_inertial,
			Gtk.ListStore encoderCaptureListStore, // to know saved (Record) repetitions
			string eccon,
			int discardNReps, // on inertial discard n reps. On gravitatory will be 0
			double videoPlayTimeInSeconds,
			bool forceRedraw, PlotTypes plotType)
	{
		LogB.Information ("CairoGraphEncoderSignal DoSendingList");
		this.capturing = capturing;
		this.points_l = points_l;
		this.points_l_inertial = points_l_inertial;
		this.encoderCaptureListStore = encoderCaptureListStore;
		this.eccon = eccon;
		this.discardNReps = discardNReps;

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
		leftMargin = 40;
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

			if (asteroids == null && customAxisDispl)
			{
				if (horizontal)
				{
					minY = customAxisDisplMin;
					absoluteMaxY = customAxisDisplMax;
				} else {
					minX = customAxisDisplMin;
					absoluteMaxX = customAxisDisplMax;
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
		if (! capturing && points_l != null && points_l.Count > 0)
			sWidth = Convert.ToInt32 (Math.Ceiling (UtilAll.DivideSafe (points_l.Count, 1000)));

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
			else //if (! customAxisDispl)
				startAt = configureTimeWindowVertical (points_l, sWidth, marginAfterInSeconds, 1000);
		}

		if(maxValuesChanged || forceRedraw || points_l.Count != points_l_painted)
		{
			int divBy = 5;
			if (horizontal && absoluteMaxY - minY > divBy)
				paintGridInt (g, minX, absoluteMaxX, minY, absoluteMaxY, Convert.ToInt32 ((absoluteMaxY - minY)/divBy), gridTypes.HORIZONTALLINES, 0, textHeight, true);
			if (! horizontal && absoluteMaxX - minX > divBy)
				paintGridInt (g, minX, absoluteMaxX, minY, absoluteMaxY, Convert.ToInt32 ((absoluteMaxX - minX)/divBy), gridTypes.VERTICALLINES, 0, textHeight, true);

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
			//LogB.Information ("last points_l.X", PointF.Last (points_l).X);
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

		drawRepetitionsInfoIfNeeded ();

		//doing = false;
		return true;
	}

	// on asteroids this will not be done
	protected virtual void drawRepetitionsInfoIfNeeded ()
	{
		if (captureCurvesBarsData_l.Count == 0 || encoderCaptureListStore == null ||
				UtilGtk.CountRows (encoderCaptureListStore) == 0)
			return;

		// 1 vertical dotted lines
		g.Save ();
		g.SetDash (new double[]{4, 2}, 0);
		g.SetSourceColor (grayDark);
		foreach (EncoderBarsData ebd in captureCurvesBarsData_l)
		{
			g.MoveTo (calculatePaintX (ebd.Start), topMargin);
			g.LineTo (calculatePaintX (ebd.Start), graphHeight - bottomMargin);
			g.Stroke ();

			g.MoveTo (calculatePaintX (ebd.End), topMargin);
			g.LineTo (calculatePaintX (ebd.End), graphHeight - bottomMargin);
			g.Stroke ();
		}
		g.Restore (); //to have solid lines

		List<string> repStr_l = UtilEncoder.GetEcconListString (eccon, UtilGtk.CountRows (encoderCaptureListStore));

		// 2 yellow rectangle on saved repetitions
		int i = 0;
		if (encoderCaptureListStore != null && UtilGtk.CountRows (encoderCaptureListStore) == captureCurvesBarsData_l.Count)
		{
			g.SetSourceColor (yellowMid);
			TreeIter iter;
			bool iterOk = encoderCaptureListStore.GetIterFirst (out iter);
			if (iterOk)
			{
				do {
					EncoderCurve curve = (EncoderCurve) encoderCaptureListStore.GetValue (iter, 0);
					if (curve.Record)
					{
						double y = minY;
						if (eccon != "c")
							y = points_l[PointF.FindSampleCloseToTime (points_l, curve.StartD)].Y;
						drawRectangleAroundText (calculatePaintX (curve.CenterD), calculatePaintY (y)-10, textHeight, repStr_l[i], g, yellowMid);
					}
					i ++;
				} while (encoderCaptureListStore.IterNext (ref iter));
			}
		}

		// 3 num of each repetition (saved or not)
		i = 0;
		foreach (EncoderBarsData ebd in captureCurvesBarsData_l)
		{
			if ( (eccon == "c" && i < discardNReps) || (eccon != "c" && i < 2*discardNReps) )
				g.SetSourceColor (grayDark);
			else
				g.SetSourceColor (bluePlots);

			double y = minY;
			if (eccon != "c")
				y = points_l[PointF.FindSampleCloseToTime (points_l, ebd.Start)].Y;

			g.MoveTo (calculatePaintX (ebd.Start), calculatePaintY (y));
			g.LineTo (calculatePaintX (ebd.End), calculatePaintY (y));
			g.Stroke ();

			if (i < repStr_l.Count) //needed check when eccon != c
				printText (calculatePaintX (ebd.Center), calculatePaintY (y)-10,
						0, textHeight, repStr_l[i], g, alignTypes.CENTER);
			i ++;
		}
		g.SetSourceColor (black);
	}

	protected virtual void plotSpecific ()
	{
		//do nothing
	}

	protected override void writeTitle()
	{
	}

	public void PassRepetitions (List<EncoderBarsData> captureCurvesBarsData_l)
	{
		this.captureCurvesBarsData_l = captureCurvesBarsData_l;
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

	protected override void drawRepetitionsInfoIfNeeded ()
	{
	}
}
