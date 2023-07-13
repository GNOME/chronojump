
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
 *  Copyright (C) 2004-2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List
using Gtk;
using Cairo;

public abstract class CairoGeneric
{
	protected int graphWidth;
	protected int graphHeight;

	//for all 4 sides
	protected int innerMargin = 30; //space between the axis and the real coordinates.

	protected int leftMargin = 40;
	protected int rightMargin = 40;
	protected int topMargin = 40;
	protected int bottomMargin = 40;

	protected string font;
	protected int textHeight = 12;

	protected Cairo.Color green = colorFromRGB(0, 200, 0);
	protected Cairo.Color red = colorFromRGB(200, 0, 0);
	Cairo.Color black = colorFromRGB(0, 0, 0);
	protected Cairo.Color yellow = new Cairo.Color (0.906, 0.745, 0.098, 1);
	protected Cairo.Color yellowTransp = new Cairo.Color (0.9, 0.9, 0.01, .25);
	protected Cairo.Color greenTransp = new Cairo.Color (0.01, 0.9, 0.01, .25);



	/*
	   need to dispose because Cairo does not clean ok on win and mac:
	   Don’t forget to manually dispose the Context and the target Surface at the end of the expose event. Automatic garbage collecting is not yet 100% working in Cairo.
		https://www.mono-project.com/docs/tools+libraries/libraries/Mono.Cairo/
		eg. on Linux can do the writeCoordinatesOfMouseClick() without disposing, but on win and mac does not work, so dispose always.
	 */
	/*
	   debug cairo disposing problems calling Chronojump like this:
	   MONO_CAIRO_DEBUG_DISPOSE=1 chronojump
	   */
	protected void endGraphDisposing(Cairo.Context g, ImageSurface surface, Gdk.Window window)
	{
		if(surface != null && window != null)
		{
			using (Context gArea = Gdk.CairoHelper.Create (window))
			{
				gArea.SetSource (surface);
				gArea.Paint ();

				gArea.GetTarget().Dispose ();
				gArea.Dispose ();
			}
		}

		g.GetTarget().Dispose ();
		g.Dispose ();

		if(surface != null)
		{
			//surface.Dispose(); //dispose surface here when all the attached stuff has been disposed
			hardDisposeSurface (surface);
		}

		/*
		GC.Collect();
		GC.WaitForPendingFinalizers();
		LogB.Information ("Memory: " + (GC.GetTotalMemory(true) / 1024) + " KB");
		*/
	}
	protected void endGraphDisposing(Cairo.Context g)
	{
		g.GetTarget().Dispose ();
		g.Dispose ();
	}

	/*
	   https://www.debugcn.com/en/article/59607062.html
	   but unused, the problem was finishing the DoSendingList with a return without a previous endGraphDisposing
	   fixed forcing it: see raceAnalyzer and encoder
	   */
	//https://stackoverflow.com/a/38276263
	//needed on GTK3 because surface.Dispose seems do nothing. The memory grows fast.
	private static void hardDisposeSurface (Surface surface)
	{
		var handle = surface.Handle;
		long refCount = surface.ReferenceCount;
		//LogB.Information("hardDisposeSurface refCount pre: " + refCount.ToString());
		surface.Dispose ();
		refCount--;
		if (refCount <= 0)
			return;

		var asm = typeof (Surface).Assembly;
		var nativeMethods = asm.GetType ("Cairo.NativeMethods");
		var surfaceDestroy = nativeMethods.GetMethod ("cairo_surface_destroy",
				System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
		try {
			for (long i = refCount; i > 0; i--)
				surfaceDestroy.Invoke (null, new object [] { handle });
		} catch {
			LogB.Information("catched on hardDisposeSurface");
		}
	}

	//0 - 255
	protected static Cairo.Color colorFromRGB(int red, int green, int blue)
	{
		return new Cairo.Color(red/256.0, green/256.0, blue/256.0);
	}
	//0 - 1
	//if does not work, check CairoUtil.PaintDrawingArea
	public static Cairo.Color colorFromRGBA (Gdk.RGBA color)
	{
		return new Cairo.Color(color.Red, color.Green, color.Blue);
	}

	//not abstract because on radial they are not defined
	protected virtual double calculatePaintX (double realX)
	{
		return 0;
	}
	protected virtual double calculatePaintY (double realY)
	{
		return 0;
	}

	protected enum alignTypes { LEFT, CENTER, RIGHT }
	protected virtual void printText (double x, double y, double height, int textHeight, string text, Cairo.Context g, alignTypes align)
	{
		double moveToLeft = 0;
		if(align == alignTypes.CENTER || align == alignTypes.RIGHT)
		{
			Cairo.TextExtents te;
			te = g.TextExtents(text);

			if(align == alignTypes.CENTER)
				moveToLeft = te.Width/2;
			else
				moveToLeft = te.Width;
		}

		g.MoveTo( x - moveToLeft, ((y+y+height)/2) + textHeight/2 );
		g.ShowText(text);
	}

	// fitting on an horizontal space
	protected bool textFits (string text, Cairo.Context g, int widthToFit)
	{
		Cairo.TextExtents te;
		te = g.TextExtents(text);

		LogB.Information (string.Format ("textFits, te.Width: {0}, widthToFit: {1}",
			te.Width, widthToFit));

		return (te.Width < widthToFit);
	}
	//if <= 0 not found
	protected int findFontThatFits (int textHeight, string text, Cairo.Context g, int widthToFit)
	{
		int textHeightReduced = textHeight;
		do {
			textHeightReduced -= 2;
			if (textHeightReduced <= 0)
				return 0;

			g.SetFontSize (textHeightReduced);
		} while (! textFits (text, g, widthToFit));

		return textHeightReduced;
	}

	// fitting at left (alignTypes.RIGHT)
	//to see if right aligned text crosses leftMargin
	protected bool textRightAlignedFitsOnLeft (double x, string text, Cairo.Context g, int marginX)
	{
		Cairo.TextExtents te;
		te = g.TextExtents(text);

		LogB.Information (string.Format ("textRightAlignedFitsOnLeft, x: {0}, te.Width: {1}, marginX: {2}",
			x, te.Width, marginX));

		return (x - te.Width > marginX);
	}
	//if <= 0 not found
	protected int findFontThatFitsOnLeft (int textHeight, double x, string text, Cairo.Context g, int marginX)
	{
		int textHeightReduced = textHeight;
		do {
			textHeightReduced -= 2;
			if (textHeightReduced <= 0)
				return 0;

			g.SetFontSize (textHeightReduced);
		} while (! textRightAlignedFitsOnLeft (x, text, g, marginX));

		return textHeightReduced;
	}

	//TODO: fix if min == max (crashes)
	protected enum gridTypes { BOTH, HORIZONTALLINES, HORIZONTALLINESATRIGHT, VERTICALLINES }
	protected void paintGridNiceAutoValues (Cairo.Context g, double minX, double maxX, double minY, double maxY, int seps, gridTypes gridType, int shiftRight, int fontH)
	{
		bool errorX;
		bool errorY;

		var gridXTuple = getGridStepAndBoundaries ((decimal) minX, (decimal) maxX, seps, out errorX);
		var gridYTuple = getGridStepAndBoundaries ((decimal) minY, (decimal) maxY, seps, out errorY);

		g.Save();
		g.SetDash(new double[]{1, 2}, 0);

		if (! errorX && (gridType == gridTypes.BOTH || gridType == gridTypes.VERTICALLINES))
			for(double i = gridXTuple.Item1; i <= gridXTuple.Item2 ; i += gridXTuple.Item3)
			{
				int xtemp = Convert.ToInt32(calculatePaintX(i));
				if(xtemp <= leftMargin || xtemp >= graphWidth - rightMargin)
					continue;

				paintVerticalGridLine(g, xtemp, Util.TrimDecimals(i, 2), fontH);
			}

		if (! errorY && (gridType == gridTypes.BOTH || gridType == gridTypes.HORIZONTALLINES || gridType == gridTypes.HORIZONTALLINESATRIGHT))
			for(double i = gridYTuple.Item1; i <= gridYTuple.Item2 ; i += gridYTuple.Item3)
			{
				int ytemp = Convert.ToInt32(calculatePaintY(i));
				if(ytemp <= topMargin || ytemp >= graphHeight -bottomMargin)
					continue;

				paintHorizontalGridLine (g, ytemp, Util.TrimDecimals(i, 2), fontH,
						(gridType == gridTypes.HORIZONTALLINESATRIGHT), shiftRight);
			}

		g.Stroke ();
		g.Restore();
	}

	//for a grid of integers
	protected void paintGridInt (Cairo.Context g, double minX, double maxX, double minY, double maxY, int by, gridTypes gridType, int shiftRight, int fontH)
	{
		g.Save();
		g.SetDash(new double[]{1, 2}, 0);
		if(gridType == gridTypes.BOTH || gridType == gridTypes.VERTICALLINES)
			for(double i = Math.Floor(minX); i <= Math.Ceiling(maxX) ; i += by)
			{
				int xtemp = Convert.ToInt32(calculatePaintX(i));
				if(xtemp <= leftMargin || xtemp >= graphWidth -rightMargin)
					continue;

				paintVerticalGridLine(g, xtemp, Util.TrimDecimals(i, 2), fontH);
			}

		if(gridType == gridTypes.BOTH || gridType == gridTypes.HORIZONTALLINES || gridType == gridTypes.HORIZONTALLINESATRIGHT)
			for(double i = Math.Floor(minY); i <= Math.Ceiling(maxY) ; i += by)
			{
				int ytemp = Convert.ToInt32(calculatePaintY(i));
				if(ytemp <= topMargin || ytemp >= graphHeight -bottomMargin)
					continue;

				paintHorizontalGridLine (g, ytemp, Util.TrimDecimals(i, 2), fontH,
						(gridType == gridTypes.HORIZONTALLINESATRIGHT), shiftRight);
			}
		g.Stroke ();
		g.Restore();
	}

	protected virtual void paintHorizontalGridLine (Cairo.Context g, int ytemp, string text, int fontH, bool atRight, int shiftRight)
	{
		if (atRight) //atRight do not write the line
		{
			//g.MoveTo(leftMargin, ytemp);
			//g.LineTo(graphWidth - rightMargin, ytemp);
			//g.SetDash(new double[] {10,5}, 0);

			printText (graphWidth -rightMargin + shiftRight, ytemp, 0, fontH, text, g, alignTypes.LEFT);

			return;
		}

		g.MoveTo(leftMargin, ytemp);
		g.LineTo(graphWidth - rightMargin, ytemp);

		printText (leftMargin/2, ytemp, 0, fontH, text, g, alignTypes.CENTER);
	}

	//this is different on forceSensor: ms to s (and with 's')
	//this combined with printXAxisText is different on RaceAnalyzer
	protected virtual void paintVerticalGridLine(Cairo.Context g, int xtemp, string text, int fontH)
	{
		if(fontH < 1)
			fontH = 1;

		g.MoveTo(xtemp, topMargin);
		g.LineTo(xtemp, graphHeight - bottomMargin);

		printText(xtemp, graphHeight -bottomMargin/2, 0, fontH, text, g, alignTypes.CENTER);
		//LogB.Information("pvgl fontH: " + fontH.ToString());
	}

	protected enum timeUnits { SECONDS, MICROSECONDS };

	//raceAnalyzer label is "". This method will write the time in seconds
	//forceSensor label is the force in N
	protected void paintVerticalTriggerLine (Cairo.Context g, Trigger trigger, timeUnits tUnits, string label, int fontH)
	{
		if(fontH < 1)
			fontH = 1;

		g.SetSourceColor(green);
		int row = 0;
		if (! trigger.InOut)
		{
			g.SetSourceColor(red);
			row = 1;
		}

		double triggerTimeConverted = trigger.Us;
		if (tUnits == timeUnits.SECONDS)
			triggerTimeConverted = UtilAll.DivideSafe (trigger.Us, 1000000);

		double xtemp = calculatePaintX (triggerTimeConverted);
		//LogB.Information(string.Format("trigger time:{0}, xtemp:{1}", triggerTimeConverted, xtemp));
		g.MoveTo(xtemp, 10 + fontH + row*12);
		g.LineTo(xtemp, graphHeight - bottomMargin);
		g.Stroke ();

		if (label == "")
			label = Util.TrimDecimals (triggerTimeConverted, 1);

		printText (xtemp, 10 + row*12, 0, fontH, label, g, alignTypes.CENTER);

		g.SetSourceColor(black);
	}

	/*
	   not horizontal, not vertical. Adapted from UtilGtk.DrawArrow()
	   tip is the point where the arrow will be drawn
	   */
	protected void plotArrowFree (Cairo.Context g, Cairo.Color color, int lineWidth, int arrowLength, bool fill,
			double tailX, double tailY, double tipX, double tipY)
	{
		g.Save();
		g.LineWidth = lineWidth;
		g.SetSourceColor(color);

		// 1) draw the line
		g.MoveTo (tailX, tailY);
		g.LineTo (tipX, tipY);
		g.Stroke ();

		// 2) draw the tip
		g.LineWidth = 1;
		double dx = tipX - tailX;
		double dy = tipY - tailY;

		double theta = Math.Atan2(dy, dx);

		double rad = MathCJ.ToRadians(35); //35 angle, can be adjusted
		double x = tipX - arrowLength * Math.Cos(theta + rad);
		double y = tipY - arrowLength * Math.Sin(theta + rad);

		double phi2 = MathCJ.ToRadians(-35);//-35 angle, can be adjusted
		double x2 = tipX - arrowLength * Math.Cos(theta + phi2);
		double y2 = tipY - arrowLength * Math.Sin(theta + phi2);

		List<PointF> points_l = new List<PointF>();
		points_l.Add(new PointF(x, y));
		points_l.Add(new PointF(tipX, tipY));
		points_l.Add(new PointF(x2, y2));

		g.MoveTo(points_l[0].X, points_l[0].Y);
		g.LineTo(points_l[1].X, points_l[1].Y);
		g.LineTo(points_l[2].X, points_l[2].Y);

		if (fill)
			g.FillPreserve ();

		g.Stroke();

		g.Restore();
	}

	//horiz or vertical to manage spacement of arrow points and tip draw
	protected void plotArrowPassingRealPoints (Cairo.Context g, Cairo.Color color,
			double ax, double ay, double bx, double by, bool horiz, bool doubleTip, int spacement)
	{
		plotArrowPassingGraphPoints (g, color, true,
				calculatePaintX(ax), calculatePaintY(ay),
				calculatePaintX(bx), calculatePaintY(by),
				horiz, doubleTip, spacement);
	}
	//note this is for left to right arrow and also only one vertical direction arrow.
	//If you want any direction, or just right to left, use plotArrowFree
	protected void plotArrowPassingGraphPoints (Cairo.Context g, Cairo.Color colorArrow, bool blackAfter,
			double ax, double ay, double bx, double by, bool horiz, bool doubleTip, int spacement)
	{
		// 1) have spacements
		if(horiz) {
			ax += spacement;
			bx -= spacement;
		} else {
			ay -= spacement;
			by += spacement;
		}
		g.SetSourceColor (colorArrow);

		// 2) write line (if it fits)
		if(horiz && bx > ax || ! horiz && ay > by)
		{
			g.MoveTo(ax, ay);
			g.LineTo(bx, by);
		} else {
			//if it does not fit, move bx or by to have the arrow at the middle
			if(horiz)
				bx = Convert.ToInt32((ax + bx) / 2);
			else
				by = Convert.ToInt32((ay + by) / 2);
			g.MoveTo(bx, by);
		}

		// 3) write arrow tip(s)
		int tip = 5;
		if(horiz) {
			if(Math.Abs(bx-ax) <= 2*tip)
				tip = 3;

			g.LineTo(bx - tip, by - tip);
			g.MoveTo(bx, by);
			g.LineTo(bx - tip, by + tip);
			if(doubleTip) {
				g.MoveTo(ax, ay);
				g.LineTo(ax + tip, ay - tip);
				g.MoveTo(ax, ay);
				g.LineTo(ax + tip, ay + tip);
			}
		} else {
			if(Math.Abs(by-ay) <= 2*tip)
				tip = 3;

			g.LineTo(bx - tip, by + tip);
			g.MoveTo(bx, by);
			g.LineTo(bx + tip, by + tip);
			if(doubleTip) {
				g.MoveTo(ax, ay);
				g.LineTo(ax - tip, ay - tip);
				g.MoveTo(ax, ay);
				g.LineTo(ax + tip, ay - tip);
			}
		}

		// 4) end
		g.Stroke ();
		if (blackAfter)
			g.SetSourceRGB(0,0,0);
	}

	protected void addClickableMark (Cairo.Context g)
	{
		addClickableMark (g, 1); //default
	}
	//double mousesFromRight is how many mouses width should be at right of the graph
	protected void addClickableMark (Cairo.Context g, double mousesFromRight)
	{
		Gdk.Pixbuf pixbuf = new Gdk.Pixbuf (null, Util.GetImagePath(false) + "mouse.png"); //18px
		Gdk.CairoHelper.SetSourcePixbuf (g, pixbuf,
				graphWidth -rightMargin -mousesFromRight*18,// -4,
				graphHeight - bottomMargin -18 -4);
		g.Paint();
	}

	/*
	 * adapted to not used LinQ from:
	 * https://stackoverflow.com/questions/237220/tickmark-algorithm-for-a-graph-axis
	 *
	 * thanks to: Andrew
	 */
	//private static Tuple<decimal, decimal, decimal> getGridStepAndBoundaries (decimal min, decimal max, int stepCount)
	private static Tuple<double, double, double> getGridStepAndBoundaries (decimal min, decimal max, int stepCount, out bool error)
	{
		// Minimal increment to avoid round extreme values to be on the edge of the chart
		LogB.Information (string.Format ("max: {0}, min: {1}", max, min)); //atencio pq el proper potser peta i caldria posar try/catch i retornar Tupla en blanc si cal
		decimal epsilon = (max - min) / 1e6m;
		max += epsilon;
		min -= epsilon;
		decimal range = max - min;

		// Target number of values to be displayed on the Y axis (it may be less)
		//int stepCount = 10;
		// First approximation
		decimal roughStep = range / (stepCount - 1);

		// Set best step for the range
		decimal[] goodNormalizedSteps = { 1, 1.5m, 2, 2.5m, 5, 7.5m, 10 }; // keep the 10 at the end
		// Or use these if you prefer:  { 1, 2, 5, 10 };

		// Normalize rough step to find the normalized one that fits best
		double stepPowerDouble = (double)Math.Pow(10, -Math.Floor(Math.Log10((double)Math.Abs(roughStep))));
		if (MathUtil.DecimalTooShortOrLarge (stepPowerDouble))
		{
			error = true;
			return new Tuple<double, double, double>(0, 0, 0);
		}

		decimal stepPower = (decimal)Math.Pow(10, -Math.Floor(Math.Log10((double)Math.Abs(roughStep))));
		var normalizedStep = roughStep * stepPower;

		//this uses Linq
		//var goodNormalizedStep = goodNormalizedSteps.First(n => n >= normalizedStep);

		//without Linq
		var goodNormalizedStep = LikeLinQFirst(goodNormalizedSteps, normalizedStep);

		decimal step = goodNormalizedStep / stepPower;

		// Determine the scale limits based on the chosen step.
		decimal scaleMax = Math.Ceiling(max / step) * step;
		decimal scaleMin = Math.Floor(min / step) * step;

		error = false;
		//return new Tuple<decimal, decimal, decimal>(scaleMin, scaleMax, step);
		return new Tuple<double, double, double>(Convert.ToDouble(scaleMin), Convert.ToDouble(scaleMax), Convert.ToDouble(step));
	}
	private static decimal LikeLinQFirst(decimal [] goodNormalizedSteps, decimal normalizedStep)
	{
		//Console.WriteLine(string.Format("normalizedStep: {0}", normalizedStep));
		foreach(var item in goodNormalizedSteps)
		{
			//Console.WriteLine(item);
			if(item >= normalizedStep)
				return item;
		}

		return goodNormalizedSteps[0];
	}

	/*
	 * pathetical old method
	 *
	private double getGridStep(double min, double max, int seps)
	{
		//show 5 steps positive, 5 negative (if possible)
		double temp = UtilAll.DivideSafe(max - min, seps);
		double step = temp;

		//to have values multiples than 10, 100 ...
		if(step == 0) //fix crash when no force
			step = 1;
		else if(step <= 1) //do nothing
			step = .2;
		else if(step <= 3)
			step = 1;
		else if(step <= 10)
			step = 5;
		else if(step <= 100)
			step = temp - (temp % 10);
		else if(step <= 1000)
			step = temp - (temp % 100);
		else if(step <= 10000)
			step = temp - (temp % 1000);
		else //if(step <= 100000)
			step = temp - (temp % 10000);

		return step;
	}
	*/

}
