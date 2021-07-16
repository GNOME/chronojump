
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
 *  Copyright (C) 2004-2020   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Cairo;

public abstract class CairoGeneric
{
	protected int graphWidth;
	protected int graphHeight;

	//for all 4 sides
	protected int outerMargins = 40; //blank space outside the axis.
	protected int innerMargins = 30; //space between the axis and the real coordinates.

	protected string font;
	protected int textHeight = 12;

	/*
	   need to dispose because Cairo does not clean ok on win and mac:
	   Don’t forget to manually dispose the Context and the target Surface at the end of the expose event. Automatic garbage collecting is not yet 100% working in Cairo.
		https://www.mono-project.com/docs/tools+libraries/libraries/Mono.Cairo/
		eg. on Linux can do the writeCoordinatesOfMouseClick() without disposing, but on win and mac does not work, so dispose always.
	 */

	protected void endGraphDisposing(Cairo.Context g)
	{
		g.GetTarget().Dispose ();
		g.Dispose ();
	}

	//0 - 255
	protected Cairo.Color colorFromRGB(int red, int green, int blue)
	{
		return new Cairo.Color(red/256.0, green/256.0, blue/256.0);
	}
	protected Cairo.Color colorFromGdk(Gdk.Color color)
	{
		return new Cairo.Color(color.Red/65536.0, color.Green/65536.0, color.Blue/65536.0);
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
	protected void printText (int x, int y, int height, int textHeight, string text, Cairo.Context g, alignTypes align)
	{
		int moveToLeft = 0;
		if(align == alignTypes.CENTER || align == alignTypes.RIGHT)
		{
			Cairo.TextExtents te;
			te = g.TextExtents(text);

			if(align == alignTypes.CENTER)
				moveToLeft = Convert.ToInt32(te.Width/2);
			else
				moveToLeft = Convert.ToInt32(te.Width);
		}

		g.MoveTo( x - moveToLeft, ((y+y+height)/2) + textHeight/2 );
		g.ShowText(text);
	}

	//TODO: fix if min == max (crashes)
	protected enum gridTypes { BOTH, HORIZONTALLINES, VERTICALLINES }
	protected void paintGridNiceAutoValues (Cairo.Context g, double minX, double maxX, double minY, double maxY, int seps, gridTypes gridType)
	{
		var gridXTuple = getGridStepAndBoundaries ((decimal) minX, (decimal) maxX, seps);
		var gridYTuple = getGridStepAndBoundaries ((decimal) minY, (decimal) maxY, seps);

		g.Save();
		g.SetDash(new double[]{1, 2}, 0);
		if(gridType != gridTypes.HORIZONTALLINES)
			for(double i = gridXTuple.Item1; i <= gridXTuple.Item2 ; i += gridXTuple.Item3)
			{
				int xtemp = Convert.ToInt32(calculatePaintX(i));
				if(xtemp <= outerMargins || xtemp >= graphWidth - outerMargins)
					continue;

				paintVerticalGridLine(g, xtemp, Util.TrimDecimals(i, 2));
			}

		if(gridType != gridTypes.VERTICALLINES)
			for(double i = gridYTuple.Item1; i <= gridYTuple.Item2 ; i += gridYTuple.Item3)
			{
				int ytemp = Convert.ToInt32(calculatePaintY(i));
				if(ytemp <= outerMargins || ytemp >= graphHeight - outerMargins)
					continue;

				paintHorizontalGridLine(g, ytemp, Util.TrimDecimals(i, 2));
			}
		g.Stroke ();
		g.Restore();
	}

	//for a grid of integers
	protected void paintGridInt (Cairo.Context g, double minX, double maxX, double minY, double maxY, int by, gridTypes gridType)
	{
		g.Save();
		g.SetDash(new double[]{1, 2}, 0);
		if(gridType != gridTypes.HORIZONTALLINES)
			for(double i = Math.Floor(minX); i <= Math.Ceiling(maxX) ; i += by)
			{
				int xtemp = Convert.ToInt32(calculatePaintX(i));
				if(xtemp <= outerMargins || xtemp >= graphWidth - outerMargins)
					continue;

				paintVerticalGridLine(g, xtemp, Util.TrimDecimals(i, 2));
			}

		if(gridType != gridTypes.VERTICALLINES)
			for(double i = Math.Floor(minX); i <= Math.Ceiling(maxY) ; i += by)
			{
				int ytemp = Convert.ToInt32(calculatePaintY(i));
				if(ytemp <= outerMargins || ytemp >= graphHeight - outerMargins)
					continue;

				paintHorizontalGridLine(g, ytemp, Util.TrimDecimals(i, 2));
			}
		g.Stroke ();
		g.Restore();
	}

	protected void paintHorizontalGridLine(Cairo.Context g, int ytemp, string text)
	{
		g.MoveTo(outerMargins, ytemp);
		g.LineTo(graphWidth - outerMargins, ytemp);
		printText(Convert.ToInt32(outerMargins/2), ytemp, 0, textHeight, text, g, alignTypes.CENTER);
	}
	//this combined with printXAxisText is different on RaceAnalyzer
	protected virtual void paintVerticalGridLine(Cairo.Context g, int xtemp, string text)
	{
		g.MoveTo(xtemp, graphHeight - outerMargins);
		g.LineTo(xtemp, outerMargins);
		printText(xtemp, graphHeight - Convert.ToInt32(outerMargins/2), 0, textHeight, text, g, alignTypes.CENTER);
	}

	/*
	 * adapted to not used LinQ from:
	 * https://stackoverflow.com/questions/237220/tickmark-algorithm-for-a-graph-axis
	 *
	 * thanks to: Andrew
	 */
	//private static Tuple<decimal, decimal, decimal> getGridStepAndBoundaries (decimal min, decimal max, int stepCount)
	private static Tuple<double, double, double> getGridStepAndBoundaries (decimal min, decimal max, int stepCount)
	{
		// Minimal increment to avoid round extreme values to be on the edge of the chart
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

		//return new Tuple<decimal, decimal, decimal>(scaleMin, scaleMax, step);
		return new Tuple<double, double, double>(Convert.ToDouble(scaleMin), Convert.ToDouble(scaleMax), Convert.ToDouble(step));
	}
	private static decimal LikeLinQFirst(decimal [] goodNormalizedSteps, decimal normalizedStep)
	{
		//Console.WriteLine(string.Format("normalizedStep: {0}", normalizedStep));
		foreach(var item in goodNormalizedSteps)
		{
			Console.WriteLine(item);
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
