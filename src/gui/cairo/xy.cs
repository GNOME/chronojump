
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
using System.Collections.Generic; //List
using Gtk;
using Cairo;

public abstract class CairoXY
{
	//used on construction
	protected List<PointF> point_l;
	protected bool predictedPointDone;

	//regression line straight
	protected double slope;
	protected double intercept;
	protected double f0;
	protected double v0;

	//regression line parabole
	protected double[] coefs;
	protected LeastSquaresParabole.ParaboleTypes paraboleType;
	protected double xAtMMaxY;
	protected double pointsMaxValue;

	protected DrawingArea area;
	protected string title;
	protected string jumpType;
	protected string date;

	protected Cairo.Context g;
	protected const int textHeight = 12;
	protected string xVariable = "";
	protected string yVariable = "";
	protected string xUnits = "";
	protected string yUnits = "";

	protected double minX = 1000000;
	protected double maxX = 0;
	protected double minY = 1000000;
	protected double maxY = 0;
	double yAtMMaxY;
	double absoluteMaxX;
	double absoluteMaxY;
	protected int graphWidth;
	protected int graphHeight;

	Cairo.Color black;
	Cairo.Color white;
	Cairo.Color red;
	Cairo.Color blue;

	//for all 4 sides
	protected int outerMargins = 40; //blank space outside the axis.
	protected int innerMargins = 30; //space between the axis and the real coordinates.
	private int crossMargins = 10; //cross slope line with margins will have this length
	int totalMargins;

	public abstract void Do();

	protected void initGraph()
	{
		totalMargins = outerMargins + innerMargins;

		//1 create context
		g = Gdk.CairoHelper.Create (area.GdkWindow);
		
		//2 clear DrawingArea (white)
		g.SetSourceRGB(1,1,1);
		g.Paint();

		graphWidth = Convert.ToInt32(area.Allocation.Width *.8);
		graphHeight = area.Allocation.Height;

		g.SetSourceRGB(0,0,0);
		g.LineWidth = 2;

		//4 prepare font
		g.SelectFontFace("Helvetica", Cairo.FontSlant.Normal, Cairo.FontWeight.Normal);
		g.SetFontSize(textHeight);

		black = colorFromRGB(0,0,0);
		white = colorFromRGB(255,255,255);
		red = colorFromRGB(200,0,0);
		blue = colorFromRGB(178, 223, 238); //lightblue

		predictedPointDone = false;
	}

	//showFullGraph means that has to plot both axis at 0 and maximums have to be f0,v0
	//used by default on jumpsWeightFVProfile
	protected void findPointMaximums(bool showFullGraph)
	{
		foreach(PointF p in point_l)
		{
			if(p.X < minX)
				minX = p.X;
			if(p.X > maxX)
				maxX = p.X;
			if(p.Y < minY)
				minY = p.Y;
			if(p.Y > maxY)
				maxY = p.Y;
		}

		if (showFullGraph)
		{
			minX = 0;
			minY = 0;
			maxX = v0;
			maxY = f0;

			//have maxX and maxY a 2.5% bigger to have a nicer cut with the predicted line
			maxX += .025 * maxX;
			maxY += .025 * maxY;
		}

		//if there is only one point, or by any reason mins == maxs, have mins and maxs separated
		separateMinXMaxXIfNeeded();
		separateMinYMaxYIfNeeded();

		absoluteMaxX = maxX;
		absoluteMaxY = maxY;
	}

	protected virtual void separateMinXMaxXIfNeeded()
	{
		if(minX == maxX)
		{
			minX -= .5 * minX;
			maxX += .5 * maxX;
		}
	}
	protected virtual void separateMinYMaxYIfNeeded()
	{
		if(minY == maxY)
		{
			minY -= .5 * minY;
			maxY += .5 * maxY;
		}
	}

	//includes point  and model
	protected void findAbsoluteMaximums()
	{
		if(coefs.Length == 3 && paraboleType == LeastSquaresParabole.ParaboleTypes.CONVEX)
		{
			//x
			absoluteMaxX = xAtMMaxY;
			if(maxX > absoluteMaxX)
				absoluteMaxX = maxX;

			//y
			yAtMMaxY = coefs[0] + coefs[1]*xAtMMaxY + coefs[2]*Math.Pow(xAtMMaxY,2);
			absoluteMaxY = yAtMMaxY;
			if(maxY > absoluteMaxY)
				absoluteMaxY = maxY;
		}
	}

	protected void paintAxisAndGrid(gridTypes gridType)
	{
		//1 paint axis
		g.MoveTo(outerMargins, outerMargins);
		g.LineTo(outerMargins, graphHeight - outerMargins);
		g.LineTo(graphWidth - outerMargins, graphHeight - outerMargins);
		g.Stroke ();
		printText(2, Convert.ToInt32(outerMargins/2), 0, textHeight, getYAxisLabel(), g, false);
		printText(graphWidth - Convert.ToInt32(outerMargins/2), graphHeight - outerMargins, 0, textHeight, getXAxisLabel(), g, false);

		paintGrid (minX, absoluteMaxX, minY, absoluteMaxY, 5, gridType);
	}

	private string getXAxisLabel()
	{
		return getAxisLabel(xVariable, xUnits);
	}
	private string getYAxisLabel()
	{
		return getAxisLabel(yVariable, yUnits);
	}
	private string getAxisLabel(string variable, string units)
	{
		if(units == "")
			return variable;
		return string.Format("{0} ({1})", variable, units);
	}

	protected enum predictedLineTypes { STRAIGHT, PARABOLE }
	protected enum predictedLineCrossMargins { TOUCH, CROSS, DONOTTOUCH }
	protected void plotPredictedLine(predictedLineTypes plt, predictedLineCrossMargins crossMarginType)
	{
		bool firstValue = false;
		double range = absoluteMaxX - minX;
		double xgraphOld = 0;
		bool wasOutOfMargins = false; //avoids to not draw a line between the end point of a line on a margin and the start point again of that line

		double xStart = minX - range/2;
		double xEnd = absoluteMaxX + range/2;
		LogB.Information(string.Format("minX: {0}, absoluteMaxX: {1}, range: {2}, xStart: {3}; xEnd: {4}", minX, absoluteMaxX, range, xStart, xEnd));
		//TODO: instead of doing this procedure for a straight line,
		//just find the two points where the line gets out of the graph and draw a line between them

		for(double x = xStart; x < xEnd; x += (xEnd - xStart)/1000)
		{
			double xgraph = calculatePaintX(x);

			//do not plot two times the same x point
			if(xgraph == xgraphOld)
				continue;
			xgraphOld = xgraph;

			double ygraph = 0;

			if(plt == predictedLineTypes.STRAIGHT)
				ygraph = calculatePaintY(slope * x + intercept);
			else //(plt == predictedLineTypes.PARABOLE)
				ygraph = calculatePaintY(coefs[0] + coefs[1]*x + coefs[2]*Math.Pow(x,2));

			// ---- do not plot line outer the axis ---->
			int om = outerMargins;

			// have a bit more distance
			if(crossMarginType == predictedLineCrossMargins.CROSS)
				om -= crossMargins;
			else if(crossMarginType == predictedLineCrossMargins.DONOTTOUCH)
				om += crossMargins;

			if(
					xgraph < om || xgraph > graphWidth - om ||
					ygraph < om || ygraph > graphHeight - om )
			{
				wasOutOfMargins = true;
				continue;
			} else {
				if(wasOutOfMargins)
					g.MoveTo(xgraph, ygraph);

				wasOutOfMargins = false;
			}
			// <---- end of do not plot line outer the axis ----

			if(! firstValue)
				g.LineTo(xgraph, ygraph);

			g.MoveTo(xgraph, ygraph);
			firstValue = false;
		}
		g.Stroke ();
	}

	protected void plotRealPoints()
	{
		foreach(PointF p in point_l)
		{
			LogB.Information(string.Format("point: {0}", p));
			double xgraph = calculatePaintX(p.X);
			double ygraph = calculatePaintY(p.Y);
			LogB.Information(string.Format("{0}, {1}", xgraph, ygraph));
			g.MoveTo(xgraph+6, ygraph);
			g.Arc(xgraph, ygraph, 6.0, 0.0, 2.0 * Math.PI); //full circle
			g.Color = blue;
			g.FillPreserve();
			g.SetSourceRGB(0, 0, 0);
			g.Stroke ();

			/*
			//print X, Y of each point
			printText(xgraph, graphHeight - Convert.ToInt32(bottomMargin/2), 0, textHeight, Util.TrimDecimals(p.X, 2), g, true);
			printText(Convert.ToInt32(leftMargin/2), ygraph, 0, textHeight, Util.TrimDecimals(p.Y, 2), g, true);
			*/

			LogB.Information(string.Format("xgraph: {0} corresponds to x real point: {1}", xgraph,
						calculateRealX(xgraph)));
			LogB.Information(string.Format("ygraph: {0} corresponds to y real point: {1}", ygraph,
						calculateRealY(ygraph)));
		}
		getMinMaxXDrawable(graphWidth, absoluteMaxX, minX, totalMargins, totalMargins);
	}

	protected void plotPredictedMaxPoint()
	{
		double xgraph = calculatePaintX(xAtMMaxY);
		double ygraph = calculatePaintY(yAtMMaxY);

		//print X, Y of maxY
		//at axis
		g.Save();
		g.SetDash(new double[]{14, 6}, 0);
		g.MoveTo(xgraph, graphHeight - outerMargins);
		g.LineTo(xgraph, ygraph);
		g.LineTo(outerMargins, ygraph);
		g.Stroke ();
		g.Restore();


		g.MoveTo(xgraph+8, ygraph);
		g.Arc(xgraph, ygraph, 8.0, 0.0, 2.0 * Math.PI); //full circle
		g.Color = red;
		g.FillPreserve();
		g.SetSourceRGB(0, 0, 0);
		g.Stroke ();
	}

	protected abstract void writeTitle();

	protected void writeTextPredictedPoint()
	{
		writeTextAtRight(0, "Fall: " + Util.TrimDecimals(xAtMMaxY, 2) + " cm", false);
		writeTextAtRight(1, "Jump height: " + Util.TrimDecimals(yAtMMaxY, 2) + " cm", false);
	}

	protected void writeTextConcaveParabole()
	{
		writeTextAtRight(0, "Error:", false);
		writeTextAtRight(1, "Parabole is concave", false);
	}

	protected void writeTextNeed3PointsWithDifferentFall()
	{
		writeTextAtRight(0, "Error:", false);
		writeTextAtRight(1, "Need at least 3 points", false);
		writeTextAtRight(2, "with different falling heights", false);
	}

	protected void writeTextAtRight(double line, string text, bool bold)
	{
		if(bold)
			g.SelectFontFace("Helvetica", Cairo.FontSlant.Normal, Cairo.FontWeight.Bold);

		printText(graphWidth, Convert.ToInt32(graphHeight/2 + textHeight*2*line), 0, textHeight, text, g, false);

		if(bold)
			g.SelectFontFace("Helvetica", Cairo.FontSlant.Normal, Cairo.FontWeight.Normal);
	}

	protected void writeCoordinatesOfMouseClick(double graphX, double graphY, double realX, double realY)
	{
		// 1) need to do this because context has been disposed
		LogB.Information(string.Format("g == null: {0}", (g = null)));
		if(g == null)
			g = Gdk.CairoHelper.Create (area.GdkWindow);


		int line = 4;
		/*
		 * This is not needed because graph is re-done at each mouse click
		 *
		//rectangle to erase previous values
		g.Color = white;
		g.Rectangle(graphWidth + 1, Convert.ToInt32(graphHeight/2) + textHeight*2*line - textHeight,
				area.Allocation.Width -1, textHeight*8);
		g.Fill();
		g.Color = black;
		*/

		// 2) exit if out of graph area
		LogB.Information(string.Format("graphX: {0}; graphY: {1}", graphX, graphY));
		if(
				graphX < outerMargins || graphX > graphWidth - outerMargins ||
				graphY < outerMargins || graphY > graphHeight - outerMargins )
			return;

		/* optional show real mouse click
		//write text (of clicked point)
		writeTextAtRight(line, "X: " + Util.TrimDecimals(realX, 2), false);
		writeTextAtRight(line +1, "Y: " + Util.TrimDecimals(realY, 2), false);
		*/

		// 3) find closest point (including predicted point if any)
		PointF pClosest = findClosestGraphPoint(graphX, graphY);

		// 4) write text at right
		writeSelectedValues(line, pClosest);

		// 5) paint rectangle around that point
		g.Color = red;
		g.Rectangle(calculatePaintX(pClosest.X) -12, calculatePaintY(pClosest.Y) -12, 24, 24);
		g.Stroke();
		g.Color = black;
	}

	/*
	 * using graphPoints and not real points because X and Y scale can be very different
	 * and this would be stranger for user to have a point selected far away to the "graph" closest point
	 */
	private PointF findClosestGraphPoint(double graphX, double graphY)
	{
		double distMin = 10000000;
		PointF pClosest = point_l[0];
		foreach(PointF p in point_l)
		{
			double dist = Math.Sqrt(Math.Pow(graphX - calculatePaintX(p.X), 2) + Math.Pow(graphY - calculatePaintY(p.Y), 2));
			if(dist < distMin)
			{
				distMin = dist;
				pClosest = p;
			}
		}

		//also check predicted point if exits
		if(predictedPointDone && Math.Sqrt(Math.Pow(graphX - calculatePaintX(xAtMMaxY), 2) + Math.Pow(graphY - calculatePaintY(yAtMMaxY), 2)) < distMin)
			pClosest = new PointF(xAtMMaxY, yAtMMaxY);

		return pClosest;
	}

	protected virtual void writeSelectedValues(int line, PointF pClosest)
	{
		writeTextAtRight(line, "Selected:", false);
		writeTextAtRight(line +1, string.Format("- {0}: {1} {2}", xVariable, Util.TrimDecimals(pClosest.X, 2), xUnits), false);
		writeTextAtRight(line +2, string.Format("- {0}: {1} {2}", yVariable, Util.TrimDecimals(pClosest.Y, 2), yUnits), false);
	}

	protected void endGraph()
	{
		g.GetTarget().Dispose ();
		g.Dispose ();
	}

	//TODO: fix if min == max (crashes)
	protected enum gridTypes { BOTH, HORIZONTALLINES, VERTICALLINES }
	protected void paintGrid (double minX, double maxX, double minY, double maxY, int seps, gridTypes gridType)
	{
		var gridXTuple = getGridStepAndBoundaries ((decimal) minX, (decimal) maxX, seps);
		var gridYTuple = getGridStepAndBoundaries ((decimal) minY, (decimal) maxY, seps);

		g.Save();
		g.SetDash(new double[]{1, 2}, 0);
		if(gridType != gridTypes.HORIZONTALLINES)
			for(double i = gridXTuple.Item1; i <= gridXTuple.Item2 ; i += gridXTuple.Item3)
			{
				int xtemp = Convert.ToInt32(calculatePaintX(i));
				if(xtemp < outerMargins || xtemp > graphWidth - outerMargins)
					continue;

				paintVerticalGridLine(xtemp, Util.TrimDecimals(i, 2));
			}

		if(gridType != gridTypes.VERTICALLINES)
			for(double i = gridYTuple.Item1; i <= gridYTuple.Item2 ; i += gridYTuple.Item3)
			{
				int ytemp = Convert.ToInt32(calculatePaintY(i));
				if(ytemp < outerMargins || ytemp > graphHeight - outerMargins)
					continue;

				paintHorizontalGridLine(ytemp, Util.TrimDecimals(i, 2));
			}
		g.Stroke ();
		g.Restore();
	}

	protected void paintHorizontalGridLine(int ytemp, string text)
	{
		g.MoveTo(outerMargins, ytemp);
		g.LineTo(graphWidth - outerMargins, ytemp);
		printText(Convert.ToInt32(outerMargins/2), ytemp, 0, textHeight, text, g, true);
	}
	protected void paintVerticalGridLine(int xtemp, string text)
	{
		g.MoveTo(xtemp, graphHeight - outerMargins);
		g.LineTo(xtemp, outerMargins);
		printText(xtemp, graphHeight - Convert.ToInt32(outerMargins/2), 0, textHeight, text, g, true);
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

	protected double calculatePaintX (double realX)
	{
                return totalMargins + (realX - minX) * (graphWidth - totalMargins - totalMargins) / (absoluteMaxX - minX);
        }
	protected double calculatePaintY (double realY)
	{
                return graphHeight - totalMargins - ((realY - minY) * (graphHeight - totalMargins - totalMargins) / (absoluteMaxY - minY));
        }

	private double calculateRealX (double graphX)
	{
                return minX + ( (graphX - totalMargins) * (absoluteMaxX - minX) / (graphWidth - totalMargins - totalMargins) );
	}
	private double calculateRealY (double graphY)
	{
		return minY - (graphY - graphHeight + totalMargins) * (absoluteMaxY - minY) / (graphHeight - totalMargins - totalMargins);
        }
	public void CalculateAndWriteRealXY (double graphX, double graphY)
	{
		writeCoordinatesOfMouseClick(graphX, graphY, calculateRealX(graphX), calculateRealY(graphY));
	}

	private void getMinMaxXDrawable(int ancho, double maxValue, double minValue, int rightMargin, int leftMargin)
	{
		LogB.Information(string.Format("Real points fitting on graph margins:  {0} , {1}",
					calculateRealX(totalMargins),
					calculateRealX(graphWidth - totalMargins)
					));
	}

	protected Cairo.Color colorFromRGB(int red, int green, int blue)
	{
		return new Cairo.Color(red/256.0, green/256.0, blue/256.0);
	}

	protected void printText (int x, int y, int height, int textHeight, string text, Cairo.Context g, bool centered)
	{
		int moveToLeft = 0;
		if(centered)
		{
			Cairo.TextExtents te;
			te = g.TextExtents(text);
			moveToLeft = Convert.ToInt32(te.Width/2);
		}
		g.MoveTo( x - moveToLeft, ((y+y+height)/2) + textHeight/2 );
		g.ShowText(text);
	}

	/*
	//unused code
	private void plotBars()
	{
                //calculate separation between series and bar width
                int distanceBetweenCols = Convert.ToInt32((graphWidth - rightMargin)*(1+.5)/point_l.Count) -
                        Convert.ToInt32((graphWidth - rightMargin)*(0+.5)/point_l.Count);

                int tctfSep = Convert.ToInt32(.3*distanceBetweenCols);
                int barWidth = Convert.ToInt32(.3*distanceBetweenCols);
                int barDesplLeft = Convert.ToInt32(.5*barWidth);

		int i = 10;
		int count = 0;
		//note p.X is jump fall and p.Y jump height
		//TODO: maybe this will be for a legend, because the graph wants X,Y points
		foreach(PointF p in point_l)
		{
			int x = Convert.ToInt32((graphWidth - rightMargin)*(count+.5)/point_l.Count)-barDesplLeft;
			int y = calculatePaintY(Convert.ToDouble(p.X), graphHeight, pointsMaxValue, 0, topMargin, bottomMargin + bottomAxis);

			LogB.Information(string.Format("red: {0}, {1}, {2}, {3}", Convert.ToDouble(p.X), graphHeight, pointsMaxValue, y));
			drawRoundedRectangle (x, y, barWidth, graphHeight - y, 4, g, red);

			x = Convert.ToInt32((graphWidth - rightMargin)*(count+.5)/point_l.Count)-barDesplLeft+tctfSep;
			y = calculatePaintY(Convert.ToDouble(p.Y), graphHeight, pointsMaxValue, 0, topMargin, bottomMargin + bottomAxis);

			LogB.Information(string.Format("blue: {0}, {1}, {2}, {3}", Convert.ToDouble(p.Y), graphHeight, pointsMaxValue, y));
			drawRoundedRectangle (x, y, barWidth, graphHeight -y, 4, g, blue);

			count ++;
		}
	}
	*/

}
