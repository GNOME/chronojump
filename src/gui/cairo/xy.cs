
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
 *  Copyright (C) 2004-2021   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List
using Gtk;
using Cairo;
using Mono.Unix;

public abstract class CairoXY : CairoGeneric
{
	//used on construction
	protected List<PointF> point_l;
	protected bool predictedPointDone;

	//regression line straight
	protected double slope;
	protected double intercept;
	protected double f0;
	protected double f0Rel;
	protected double v0;
	//samozino fv
	protected double f0Opt;
	protected double v0Opt;

	//regression line parabole
	protected double[] coefs;
	protected LeastSquaresParabole.ParaboleTypes paraboleType;
	protected double xAtMMaxY;
	protected double pointsMaxValue;

	protected DrawingArea area;
	protected string title;
	protected string jumpType;
	protected string runType;
	protected string date;
	protected Cairo.Color colorBackground;

	protected Cairo.Context g;
	protected int pointsRadius = 6;
	protected string xVariable = "";
	protected string yVariable = "";
	protected string xUnits = "";
	protected string yUnits = "";

	protected double minX = 1000000;
	protected double maxX = 0;
	protected double minY = 1000000;
	protected double maxY = 0;
	double yAtMMaxY;
	protected double absoluteMaxX;
	protected double absoluteMaxY;

	protected Cairo.Color black;
	protected Cairo.Color gray99;
	Cairo.Color white;
	protected Cairo.Color red;
	Cairo.Color blue;
	protected Cairo.Color bluePlots;

	private int crossMargins = 10; //cross slope line with margins will have this length
	int totalMargins;

	//translated strings
	//done to use Catalog just only on gui/cairo/xy.cs
	//but jumpsWeightFVProfile has many messages, so done also there
	protected string needToExecuteJumpsStr = Catalog.GetString("Need to execute jumps:");
	protected string needToExecuteRunsStr = Catalog.GetString("Need to execute races:");
	protected string optimalFallHeightStr = Catalog.GetString("Optimal fall height");
	protected string heightStr = Catalog.GetString("Height");
	protected string extraWeightStr = Catalog.GetString("Extra weight");
	protected string fallStr = Catalog.GetString("Fall");
	protected string speedStr = Catalog.GetString("Speed");
	protected string forceStr = Catalog.GetString("Force");
	protected string dateStr = Catalog.GetString("Date");
	protected string timeStr = Catalog.GetString("Time");
	protected string distanceStr = Catalog.GetString("Distance");
	protected string tfStr = Catalog.GetString("TF");
	protected string tcStr = Catalog.GetString("TC");
	protected string countStr = Catalog.GetString("Num");
	protected string jumpTypeStr = Catalog.GetString("Jump type:");
	protected string runTypeStr = Catalog.GetString("Race type:");
	//protected static int lastPointPainted;

	public virtual bool PassData (List<PointF> point_l)
	{
		return false;
	}

	public virtual void Do(string font)
	{
	}
	public virtual void DoSendingList(string font, List<PointF> points_list, bool forceRedraw, PlotTypes plotType)
	{
	}

	protected void initGraph(string font, double widthPercent1)
	{
		initGraph(font, widthPercent1, true);
	}
	protected void initGraph(string font, double widthPercent1, bool clearDrawingArea)
	{
		this.font = font;
		LogB.Information("Font: " + font);

		//outerMargin = 40; //blank space outside the axis.
		//innerMargin = 30; //space between the axis and the real coordinates.

		totalMargins = outerMargin + innerMargin;

		//1 create context
		g = Gdk.CairoHelper.Create (area.GdkWindow);

		if(clearDrawingArea)
		{
			//2 clear DrawingArea (white)
			g.SetSourceRGB(1,1,1);
			g.Paint();
		}

		graphWidth = Convert.ToInt32(area.Allocation.Width * widthPercent1);
		graphHeight = area.Allocation.Height;

		g.SetSourceRGB(0,0,0);
		g.LineWidth = 2;

		//4 prepare font
		g.SelectFontFace(font, Cairo.FontSlant.Normal, Cairo.FontWeight.Normal);
		g.SetFontSize(textHeight);

		black = colorFromRGB(0,0,0);
		gray99 = colorFromRGB(99,99,99);
		white = colorFromRGB(255,255,255);
		red = colorFromRGB(200,0,0);
		blue = colorFromRGB(178, 223, 238); //lightblue
		bluePlots = colorFromRGB(0, 0, 200);

		predictedPointDone = false;
	}

	//showFullGraph means that has to plot both axis at 0 and maximums have to be f0,v0
	//used by default on jumpsWeightFVProfile
	//called from almost all methods
	//true if changed
	protected bool findPointMaximums(bool showFullGraph)
	{
		return findPointMaximums(showFullGraph, point_l);
	}

	//called from raceAnalyzer (sending it own list of points)
	//true if changed
	protected bool findPointMaximums(bool showFullGraph, List<PointF> points_list)
	{
		//foreach(PointF p in points_list)
		/*
		int start = lastPointPainted;
		if(lastPointPainted < 0)
			start = 0;

		//for(int i = start; i < points_list.Count; i ++)
		*/
		for(int i = 0; i < points_list.Count; i ++)
		{
			PointF p = points_list[i];

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

			//fix axis problems if F0 or V0 are not ok
			if(f0Rel > 0 && v0 > 0)
			{
				maxX = v0;
				if(v0Opt > v0)
					maxX = v0Opt;

				maxY = f0Rel;
				if(f0Opt > f0Rel)
					maxY = f0Opt;
			}

			//have maxX and maxY a 2.5% bigger to have a nicer cut with the predicted line
			maxX += .025 * maxX;
			maxY += .025 * maxY;
		}

		//if there is only one point, or by any reason mins == maxs, have mins and maxs separated
		separateMinXMaxXIfNeeded();
		separateMinYMaxYIfNeeded();

		bool changed = false;
		if(maxX != absoluteMaxX)
		{
			absoluteMaxX = maxX;
			changed = true;
		}
		if(maxY != absoluteMaxY)
		{
			absoluteMaxY = maxY;
			changed = true;
		}

		return changed;
	}

	protected void plotAlternativeLineWithRealPoints (double ax, double ay, double bx, double by, bool showFullGraph)
	{
		LeastSquaresLine lsl = new LeastSquaresLine();
		List<PointF> measures = new List<PointF> {
			new PointF(ax, ay), new PointF(bx, by) };
		lsl.Calculate(measures);

		g.SetSourceRGB(255,0,0);
		if(showFullGraph)
			plotPredictedLine(predictedLineTypes.STRAIGHT, predictedLineCrossMargins.CROSS, lsl.Slope, lsl.Intercept);
		else
			plotPredictedLine(predictedLineTypes.STRAIGHT, predictedLineCrossMargins.DONOTTOUCH, lsl.Slope, lsl.Intercept);
		g.SetSourceRGB(0,0,0);
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

	//reccomended to 1st paint the grid, then the axis
	protected void paintGrid(gridTypes gridType, bool niceAutoValues)
	{
		g.LineWidth = 1; //to allow to be shown the red arrows on jumpsWeightFVProfile

		if(niceAutoValues)
			paintGridNiceAutoValues (g, minX, absoluteMaxX, minY, absoluteMaxY, 5, gridType, textHeight);
		else
			paintGridInt (g, minX, absoluteMaxX, minY, absoluteMaxY, 1, gridType, textHeight);
	}

	protected void paintAxis()
	{
		g.MoveTo(outerMargin, outerMargin);
		g.LineTo(outerMargin, graphHeight - outerMargin);
		g.LineTo(graphWidth - outerMargin, graphHeight - outerMargin);
		g.Stroke ();
		printText(2, Convert.ToInt32(outerMargin/2), 0, textHeight, getYAxisLabel(), g, alignTypes.LEFT);
		printXAxisText();
		g.Stroke ();
		g.LineWidth = 2;
	}

	//this combined with paintVerticalGridLine is different on RaceAnalyzer
	protected virtual void printXAxisText()
	{
		printText(graphWidth - Convert.ToInt32(outerMargin/2), graphHeight - outerMargin, 0, textHeight, getXAxisLabel(), g, alignTypes.LEFT);
	}

	protected string getXAxisLabel()
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
		plotPredictedLine(plt, crossMarginType, slope, intercept);
	}
	protected void plotPredictedLine(predictedLineTypes plt, predictedLineCrossMargins crossMarginType, double slope, double intercept)
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
			int om = outerMargin;

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

	public enum PlotTypes { POINTS, LINES, POINTSLINES }

	//called from almost all methods
	protected void plotRealPoints (PlotTypes plotType)
	{
		plotRealPoints (plotType, point_l, 0);
	}

	//called from raceAnalyzer (sending it own list of points)
	protected void plotRealPoints (PlotTypes plotType, List<PointF> points_list, int startAt)
	{
		if(plotType == PlotTypes.LINES || plotType == PlotTypes.POINTSLINES) //draw line first to not overlap the points
		{
			bool firstDone = false;
			//foreach(PointF p in points_list)
			for(int i = startAt; i < points_list.Count; i ++)
			{
				PointF p = points_list[i];

				double xgraph = calculatePaintX(p.X);
				double ygraph = calculatePaintY(p.Y);

				if(! firstDone)
				{
					g.MoveTo(xgraph, ygraph);
					firstDone = true;
				} else
					g.LineTo(xgraph, ygraph);
			}
			g.Stroke ();
		}

//		lock (point_l) {
//		List<PointF> point_l_copy = point_l>;
//		foreach(PointF p in point_l_copy)
		//foreach(PointF p in points_list)
		/*
		int start = lastPointPainted;
		if(lastPointPainted < 0)
			start = 0;

		//for(int i = start; i < points_list.Count; i ++)
		*/

		if(plotType == PlotTypes.LINES)
			return;

		for(int i = startAt; i < points_list.Count; i ++)
		{
			PointF p = points_list[i];

			//LogB.Information("point: " + p.ToString());
			double xgraph = calculatePaintX(p.X);
			double ygraph = calculatePaintY(p.Y);
			g.Arc(xgraph, ygraph, pointsRadius, 0.0, 2.0 * Math.PI); //full circle
			g.Color = colorBackground;
			g.FillPreserve();
			g.SetSourceRGB(0, 0, 0);
			g.Stroke (); 	//can this be done at the end?

			//lastPointPainted ++;

			/*
			//print X, Y of each point
			printText(xgraph, graphHeight - Convert.ToInt32(bottomMargin/2), 0, textHeight, Util.TrimDecimals(p.X, 2), g, true);
			printText(Convert.ToInt32(leftMargin/2), ygraph, 0, textHeight, Util.TrimDecimals(p.Y, 2), g, true);
			*/
//		}
		}
		//getMinMaxXDrawable(graphWidth, absoluteMaxX, minX, totalMargins, totalMargins);
	}

	protected void plotPredictedMaxPoint()
	{
		double xgraph = calculatePaintX(xAtMMaxY);
		double ygraph = calculatePaintY(yAtMMaxY);

		//print X, Y of maxY
		//at axis
		g.Save();
		g.SetDash(new double[]{14, 6}, 0);
		g.MoveTo(xgraph, graphHeight - outerMargin);
		g.LineTo(xgraph, ygraph);
		g.LineTo(outerMargin, ygraph);
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
		writeTextAtRight(0, fallStr + ": " + Util.TrimDecimals(xAtMMaxY, 2) + " cm", false);
		writeTextAtRight(1, heightStr + ": " + Util.TrimDecimals(yAtMMaxY, 2) + " cm", false);
	}

	protected void writeTextConcaveParabole()
	{
		writeTextAtRight(0, Catalog.GetString("Error") + ":", false);
		writeTextAtRight(1, Catalog.GetString("Parabole is concave"), false);
	}

	protected void writeTextNeed3PointsWithDifferentFall()
	{
		writeTextAtRight(0, Catalog.GetString("Error") + ":", false);
		writeTextAtRight(1, Catalog.GetString("Need at least 3 points"), false);
		writeTextAtRight(2, Catalog.GetString("with different falling heights"), false);
	}

	protected void writeTextAtRight(double line, string text, bool bold)
	{
		if(bold)
			g.SelectFontFace(font, Cairo.FontSlant.Normal, Cairo.FontWeight.Bold);

		printText(graphWidth, Convert.ToInt32(graphHeight/2 + textHeight*2*line), 0, textHeight, text, g, alignTypes.LEFT);

		if(bold)
			g.SelectFontFace(font, Cairo.FontSlant.Normal, Cairo.FontWeight.Normal);
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
				graphX < outerMargin || graphX > graphWidth - outerMargin ||
				graphY < outerMargin || graphY > graphHeight - outerMargin )
			return;

		/* optional show real mouse click
		//write text (of clicked point)
		writeTextAtRight(line, "X: " + Util.TrimDecimals(realX, 2), false);
		writeTextAtRight(line +1, "Y: " + Util.TrimDecimals(realY, 2), false);
		*/

		LogB.Information("calling findClosestGraphPoint ...");
		// 3) find closest point (including predicted point if any)
		PointF pClosest = findClosestGraphPoint(graphX, graphY);

		LogB.Information("writeSelectedValues ...");
		// 4) write text at right
		writeSelectedValues(line, pClosest);

		LogB.Information("painting rectangle ...");
		// 4) write text at right
		// 5) paint rectangle around that point
		g.Color = bluePlots;
		g.Rectangle(calculatePaintX(pClosest.X) - 2*pointsRadius, calculatePaintY(pClosest.Y) -2*pointsRadius,
				4*pointsRadius, 4*pointsRadius);
		g.Stroke();
		g.Color = black;
		LogB.Information("writeCoordinatesOfMouseClick done! disposing");

		endGraphDisposing(g);

		LogB.Information("writeCoordinatesOfMouseClick disposed!");
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

		//also check predicted point if exists
		if(predictedPointDone && Math.Sqrt(Math.Pow(graphX - calculatePaintX(xAtMMaxY), 2) + Math.Pow(graphY - calculatePaintY(yAtMMaxY), 2)) < distMin)
			pClosest = new PointF(xAtMMaxY, yAtMMaxY);

		return pClosest;
	}

	protected virtual void writeSelectedValues(int line, PointF pClosest)
	{
		g.Color = bluePlots;
		writeTextAtRight(line, Catalog.GetString("Selected") + ":", false);
		g.SetSourceRGB(0, 0, 0);

		writeTextAtRight(line +1, string.Format("- {0}: {1} {2}", xVariable, Util.TrimDecimals(pClosest.X, 2), xUnits), false);
		writeTextAtRight(line +2, string.Format("- {0}: {1} {2}", yVariable, Util.TrimDecimals(pClosest.Y, 2), yUnits), false);
	}


	//TODO: check if for one value this is /0
	protected override double calculatePaintX (double realX)
	{
                return totalMargins + (realX - minX) * (graphWidth - totalMargins - totalMargins) / (absoluteMaxX - minX);
        }
	//TODO: check if for one value this is /0
	protected override double calculatePaintY (double realY)
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

	/*
	private void getMinMaxXDrawable(int ancho, double maxValue, double minValue, int rightMargin, int leftMargin)
	{
		LogB.Information(string.Format("Real points fitting on graph margins:  {0} , {1}",
					calculateRealX(totalMargins),
					calculateRealX(graphWidth - totalMargins)
					));
	}
	*/

}
