
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
using Mono.Unix;

public abstract class CairoXY : CairoGeneric
{
	//used on construction
	protected List<PointF> point_l;
	protected List<DateTime> dates_l; //used on button press to know day date instead of date as double
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
	protected ImageSurface surface;
	protected string title;
	protected string jumpType;
	protected string runType;
	protected string date;
	protected Cairo.Color colorBackground; //on Chronojump (not bg of the cairo graph)
	protected Cairo.Color colorCairoBackground = new Cairo.Color (1, 1, 1, 1);

	protected Cairo.Context g;
	protected int pointsRadius = 6;
	protected string xVariable = "";
	protected string yVariable = "";
	protected string xUnits = "";
	protected string yUnits = "";
	protected string yRightVariable = "";
	protected string yRightUnits = "";

	protected double minX = 0;
	protected double maxX = 0;
	protected double minY = 0;
	protected double maxY = 0;
	protected double xAtMaxY = 0; //raw, used on raceAnalyzer & forceSensor
	protected double yAtMaxY = 0; //raw, used on raceAnalyzer & forceSensor (needed because maxY can increase if == to minY, and this yAtMaxY refers to the best point
	protected double xAtMinY = 0; //raw, used on forceSensor
	protected double yAtMinY = 0; //raw, used on forceSensor (needed because maxY can increase if == to minY, and this yAtMaxY refers to the best point
	double yAtMMaxY;
	protected double absoluteMaxX;
	protected double absoluteMaxY;
	protected double mouseX;
	protected double mouseY;

	protected Cairo.Color black;
	protected Cairo.Color brown;
	protected Cairo.Color gray99;
	protected Cairo.Color gray;
	protected Cairo.Color grayClear;
	protected Cairo.Color grayDark;
	protected Cairo.Color white;
	//Cairo.Color blue;
	protected Cairo.Color bluePlots;

	private int crossMargins = 10; //cross slope line with margins will have this length

	//translated strings
	//done to use Catalog just only on gui/cairo/xy.cs
	//but jumpsWeightFVProfile has many messages, so done also there, and also on radial
	protected string needToExecuteJumpsStr = Catalog.GetString("Need to execute jumps:");
	protected string needToExecuteRunsStr = Catalog.GetString("Need to execute races:");
	protected string repeatedJumpsStr = Catalog.GetString("Jumps cannot be the same.");
	protected string optimalFallHeightStr = Catalog.GetString("Optimal fall height");
	protected string heightStr = Catalog.GetString("Height");
	protected string extraWeightStr = Catalog.GetString("Extra weight");
	protected string fallStr = Catalog.GetString("Fall");
	protected string speedStr = Catalog.GetString("Speed");
	protected string accelStr = Catalog.GetString("Acceleration");
	protected string forceStr = Catalog.GetString("Force");
	protected string powerStr = Catalog.GetString("Power");
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

	public void PassMouseXY (double mouseX, double mouseY)
	{
		this.mouseX = mouseX;
		this.mouseY = mouseY;
	}

	public virtual void Do (string font)
	{
	}

	//encoderSignal (with inertial stuff & ! triggers at the moment)
	public virtual void DoSendingList (string font, bool isInertial,
			List<PointF> points_list, List<PointF> points_list_inertial,
			bool forceRedraw, PlotTypes plotType)
	{
	}

	//raceAnalyzer XY graphs (triggers)
//	public virtual void DoSendingList(string font, List<PointF> points_list, TriggerList triggerList, bool forceRedraw, PlotTypes plotType)
//	{
//	}

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

		LogB.Information("initGraph 2");
		// 1 create context
		/* using drawingarea (slow)
		   g = Gdk.CairoHelper.Create (area.Window);

		   //from area->surface (see xy.cs)
		   //draw on surface: create surface, context related to this surface, draw on this contex
		   //copy later to drawingarea before disposing, this is much faster
		 */
		surface = new ImageSurface(Format.RGB24, area.Allocation.Width, area.Allocation.Height);
		g = new Context (surface);

		if(clearDrawingArea)
		{
			//2 clear DrawingArea (context) (paint in white)
			//g.SetSourceRGB(1,1,1);
			g.SetSourceColor (colorCairoBackground);
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
		brown = new Cairo.Color (0.588,0.294,0);
		gray99 = colorFromRGB(99,99,99);
		gray = new Cairo.Color (.5, .5, .5, 1);
		grayClear = new Cairo.Color (.66, .66, .66, 1);
		grayDark = new Cairo.Color (.33, .33, .33, 1);
		white = new Cairo.Color (1, 1, 1, 1);
		//blue = colorFromRGB(178, 223, 238); //lightblue
		bluePlots = colorFromRGB(0, 0, 200);

		predictedPointDone = false;
	}

	//on fullscreen/fullscreen exit
	public void ChangeDrawingArea (DrawingArea area)
	{
		//dispose if we are in the middle of the graph (using another DrawingAreas)
		//cannot happen because we change the drawing area
		//endGraphDisposing(g, surface, area.Window);

		//set the are for the next drawings
		this.area = area;
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
		minX = 0;
		minY = 0;
		maxX = 0;
		maxY = 0;
		for(int i = 0; i < points_list.Count; i ++)
		{
			PointF p = points_list[i];

			if(i == 0 || p.X < minX)
				minX = p.X;
			if(p.X > maxX)
				maxX = p.X;
			if(i == 0 || p.Y < minY)
			{
				minY = p.Y;

				// both used on forceSensor
				xAtMinY = p.X;
				yAtMinY = p.Y; //(needed because maxY can increase if == to minY), and this yAtMinY refers to the best point
			}
			if(i == 0 || p.Y > maxY)
			{
				maxY = p.Y;
				xAtMaxY = p.X; //used on raceAnalyzer
				yAtMaxY = p.Y; //used on raceAnalyzer (needed because maxY can increase if == to minY), and this yAtMaxY refers to the best point
			}
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

	protected int gridNiceSeps = 5;
	//reccomended to 1st paint the grid, then the axis
	//shift right is good when there are several axis at right
	protected void paintGrid (gridTypes gridType, bool niceAutoValues, int shiftRight)
	{
		g.LineWidth = 1; //to allow to be shown the red arrows on jumpsWeightFVProfile

		if(niceAutoValues)
			paintGridNiceAutoValues (g, minX, absoluteMaxX, minY, absoluteMaxY, gridNiceSeps, gridType, shiftRight, textHeight);
		else
			paintGridInt (g, minX, absoluteMaxX, minY, absoluteMaxY, 1, gridType, shiftRight, textHeight);
	}

	protected void paintAxis()
	{
		g.MoveTo (leftMargin, topMargin);
		g.LineTo (leftMargin, graphHeight -bottomMargin);
		if (xVariable != "")
			g.LineTo (graphWidth -3*rightMargin, graphHeight -bottomMargin);
		else
			g.LineTo (graphWidth -rightMargin, graphHeight -bottomMargin);

		g.Stroke ();
		printYAxisText();
		printXAxisText();
		g.Stroke ();
		g.LineWidth = 2;
	}

	protected void paintAxisRight (int shiftToRight, bool axisLabelTop, string variable, string units)
	{
		g.MoveTo (graphWidth -rightMargin + shiftToRight, graphHeight -bottomMargin);
		g.LineTo (graphWidth -rightMargin + shiftToRight, topMargin);
		g.Stroke ();

		printYRightAxisText (shiftToRight, axisLabelTop, getAxisLabel (variable, units));

		g.Stroke ();
		g.LineWidth = 2;
	}

	protected virtual void printYAxisText()
	{
		printText (leftMargin, Convert.ToInt32 (topMargin/2), 0, textHeight, getYAxisLabel(), g, alignTypes.CENTER);
	}
	protected virtual void printYRightAxisText (int shiftToRight, bool axisLabelTop, string variableUnits)
	{
		int y = Convert.ToInt32 (topMargin /2);
		if (! axisLabelTop)
			y = Convert.ToInt32 (graphHeight -bottomMargin /2);

		printText (graphWidth -rightMargin +shiftToRight, y, 0, textHeight, variableUnits, g, alignTypes.CENTER);
	}
	protected virtual void printXAxisText()
	{
		printText (graphWidth - Convert.ToInt32 (rightMargin/2), graphHeight -bottomMargin, 0, textHeight, getXAxisLabel(), g, alignTypes.RIGHT);
	}

	protected string getXAxisLabel()
	{
		return getAxisLabel(xVariable, xUnits);
	}
	protected string getYAxisLabel()
	{
		return getAxisLabel(yVariable, yUnits);
	}
	protected string getAxisLabel(string variable, string units)
	{
		if(units == "")
			return variable;
		return string.Format("{0} ({1})", variable, units);
	}

	protected void preparePredictedLine (List<PointF> p_l)
	{
		LeastSquaresLine ls = new LeastSquaresLine();
		ls.Calculate (p_l);
		if (ls.SlopeIsNaN ())
			return;

		g.Save ();
		g.LineWidth = 1;
		g.SetDash (new double[]{4, 2}, 0);
		plotPredictedLine2023 (ls.Slope, ls.Intercept);
		g.Restore ();
	}

	protected enum predictedLineTypes { STRAIGHT, PARABOLE }
	protected enum predictedLineCrossMargins { TOUCH, CROSS, DONOTTOUCH }
	protected void plotPredictedLine(predictedLineTypes plt, predictedLineCrossMargins crossMarginType)
	{
		plotPredictedLine(plt, crossMarginType, slope, intercept);
	}

	// For new code use the plotPredictedLine2023 (at least when is straight line)
	protected void plotPredictedLine (predictedLineTypes plt, predictedLineCrossMargins crossMarginType, double slope, double intercept)
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
		//done in plotPredictedLine2023

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
			int omX = leftMargin;
			int omY = bottomMargin;

			// have a bit more distance
			if(crossMarginType == predictedLineCrossMargins.CROSS)
			{
				omX -= crossMargins;
				omY -= crossMargins;
			}
			else if(crossMarginType == predictedLineCrossMargins.DONOTTOUCH)
			{
				omX += crossMargins;
				omY += crossMargins;
			}

			if(
					xgraph < omX || xgraph > graphWidth - omX ||
					ygraph < omY || ygraph > graphHeight - omY )
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
	//use this method (at least for straight lines)
	protected void plotPredictedLine2023 (double slope, double intercept)
	{
		// y = slope * x + intercept
		// x = (y - intercept) / slope
		// find when point where line cross vertical margins

		double yTopReal = calculateRealY (topMargin);
		double xTopGraph = calculatePaintX ((yTopReal - intercept) / slope);

		double yBottomReal = calculateRealY (graphHeight - bottomMargin);
		double xBottomGraph = calculatePaintX ((yBottomReal - intercept) / slope);

		g.MoveTo (xTopGraph, topMargin);
		g.LineTo (xBottomGraph, graphHeight - bottomMargin);
		g.Stroke ();
	}


	public enum PlotTypes { POINTS, LINES, POINTSLINES }

	//called from almost all methods
	protected void plotRealPoints (PlotTypes plotType)
	{
		plotRealPoints (plotType, point_l, 0, false);
	}

	//called from raceAnalyzer and encoder (sending it own list of points)
	//fast: calculated first calculatePaintX/Y for most of the formula. foreach value does short operation.
	protected void plotRealPoints (PlotTypes plotType, List<PointF> points_list, int startAt, bool fast)
	{
		if(plotType == PlotTypes.LINES || plotType == PlotTypes.POINTSLINES) //draw line first to not overlap the points
		{
			bool firstDone = false;
			double xgraph;
			double ygraph;
			double paintXfastA = 0;
			double paintXfastB = 0;
			double paintYfastA = 0;
			double paintYfastB = 0;

			if(fast)
			{
				calculatePaintXFastPre (out paintXfastA, out paintXfastB);
				calculatePaintYFastPre (out paintYfastA, out paintYfastB);
			}
			for(int i = startAt; i < points_list.Count; i ++)
			{
				PointF p = points_list[i];

				if(fast)
				{
					xgraph = calculatePaintXFastDo (p.X, paintXfastA, paintXfastB);
					ygraph = calculatePaintYFastDo (p.Y, paintYfastA, paintYfastB);
				} else {
					xgraph = calculatePaintX(p.X);
					ygraph = calculatePaintY(p.Y);
				}

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
			g.SetSourceColor(colorBackground);
			g.FillPreserve();
			g.SetSourceRGB(0, 0, 0);
			g.Stroke (); 	//can this be done at the end?

			//lastPointPainted ++;
		}

		bool debug = false;
		if (debug) 	//print X, Y of each point
		{
			for(int i = startAt; i < points_list.Count; i ++)
			{
				PointF p = points_list[i];
				double xgraph = calculatePaintX (p.X);
				double ygraph = calculatePaintY (p.Y);
				printText (xgraph, ygraph -graphHeight/20, 0, textHeight,
						string.Format ("({0};{1})", Util.TrimDecimals (p.X, 4), Util.TrimDecimals (p.Y, 4)), g, alignTypes.CENTER);
			}
		}

		//getMinMaxXDrawable(graphWidth, absoluteMaxX, minX, totalMargins, totalMargins);
	}

	protected void plotPredictedMaxPoint()
	{
		double xgraph = calculatePaintX(xAtMMaxY);
		double ygraph = calculatePaintY(yAtMMaxY);

		//print X, Y of maxY
		//at axis
		g.Save ();
		g.SetDash (new double[]{14, 6}, 0);
		g.MoveTo (xgraph, graphHeight -bottomMargin);
		g.LineTo (xgraph, ygraph);
		g.LineTo (leftMargin, ygraph);
		g.Stroke ();
		g.Restore ();


		g.MoveTo(xgraph+8, ygraph);
		g.Arc(xgraph, ygraph, 8.0, 0.0, 2.0 * Math.PI); //full circle
		g.SetSourceColor(red);
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

	protected PointF pClosest;
	protected void writeCoordinatesOfMouseClick (double graphX, double graphY, double realX, double realY)
	{
		int line = 4;

		// 1) exit if out of graph area
		LogB.Information(string.Format("graphX: {0}; graphY: {1}", graphX, graphY));
		if(
				graphX < leftMargin || graphX > graphWidth -rightMargin ||
				graphY < topMargin || graphY > graphHeight -bottomMargin )
			return;

		/* optional show real mouse click
		//write text (of clicked point)
		writeTextAtRight(line, "X: " + Util.TrimDecimals(realX, 2), false);
		writeTextAtRight(line +1, "Y: " + Util.TrimDecimals(realY, 2), false);
		*/

		LogB.Information("calling findClosestGraphPoint ...");
		// 2) find closest point (including predicted point if any)
		int closestPos = findClosestGraphPointPos (graphX, graphY);

		LogB.Information("writeSelectedValues ...");
		// 3) write text at right
		writeSelectedValues (line, pClosest, closestPos);

		LogB.Information("painting rectangle ...");
		// 4) paint rectangle around that point
		g.SetSourceColor(bluePlots);
		g.Rectangle(calculatePaintX(pClosest.X) - 2*pointsRadius, calculatePaintY(pClosest.Y) -2*pointsRadius,
				4*pointsRadius, 4*pointsRadius);
		g.Stroke();
		g.SetSourceColor(black);
	}

	/*
	 * using graphPoints and not real points because X and Y scale can be very different
	 * and this would be stranger for user to have a point selected far away to the "graph" closest point
	 */
	//return the position in point_l, -1 if predicted is used
	//the position is useful for jumpsRunsEvolution and asymmetry to know the real date on date_l
	private int findClosestGraphPointPos (double graphX, double graphY)
	{
		double distMin = 10000000;
		pClosest = point_l[0];
		int count = 0;
		int found = 0;
		foreach(PointF p in point_l)
		{
			double dist = CairoUtil.GetDistance2D (graphX, graphY, calculatePaintX (p.X), calculatePaintY (p.Y));
			if(dist < distMin)
			{
				distMin = dist;
				pClosest = p;
				found = count;
			}
			count ++;
		}

		//also check predicted point if exists
		if(predictedPointDone && Math.Sqrt(Math.Pow(graphX - calculatePaintX(xAtMMaxY), 2) + Math.Pow(graphY - calculatePaintY(yAtMMaxY), 2)) < distMin)
		{
			pClosest = new PointF(xAtMMaxY, yAtMMaxY);
			return -1;
		}

		return found;
	}

	protected virtual void writeSelectedValues(int line, PointF pClosest, int closestPos)
	{
		g.SelectFontFace(font, Cairo.FontSlant.Normal, Cairo.FontWeight.Normal);
		g.SetFontSize(textHeight);

		g.SetSourceColor(bluePlots);
		writeTextAtRight(line, Catalog.GetString("Selected") + ":", false);
		g.SetSourceRGB(0, 0, 0);

		if (closestPos >= 0 && dates_l != null && dates_l.Count > 0 && closestPos < dates_l.Count)
			writeTextAtRight(line +1, string.Format("- {0}: {1} {2}", xVariable, printXDateTime (dates_l[closestPos]), xUnits), false);
		else
			writeTextAtRight(line +1, string.Format("- {0}: {1} {2}", xVariable, Util.TrimDecimals(pClosest.X, 2), xUnits), false);

		writeTextAtRight(line +2, string.Format("- {0}: {1} {2}", yVariable, Util.TrimDecimals(pClosest.Y, 2), yUnits), false);
	}

	//on jumpsAsymmetry is overrided and only prints time
	protected virtual string printXDateTime (DateTime dt)
	{
		return (dt.ToString ());
	}

	protected enum Directions { B, L, T, R, LR, BT } //like on R (Bottom, Left, Top, Right) but adding LR and BT
	// gets the margins (outer + inner) of each of the directions
	protected int getMargins (Directions dir)
	{
		if (dir == Directions.B)
			return bottomMargin + innerMargin;
		else if (dir == Directions.L)
			return leftMargin + innerMargin;
		else if (dir == Directions.T)
			return topMargin + innerMargin;
		else if (dir == Directions.R)
			return rightMargin + innerMargin;
		else if (dir == Directions.LR)
			return leftMargin + innerMargin + rightMargin + innerMargin;
		else if (dir == Directions.BT)
			return bottomMargin + innerMargin + topMargin + innerMargin;

		return 0;
	}

	protected override double calculatePaintX (double realX)
	{
                //return totalMargins + (realX - minX) * (graphWidth - totalMargins - totalMargins) / (absoluteMaxX - minX);
                return getMargins (Directions.L) + (realX - minX) * UtilAll.DivideSafe(
				graphWidth - getMargins (Directions.LR),
				absoluteMaxX - minX);
        }
	protected override double calculatePaintY (double realY)
	{
                //return graphHeight - totalMargins - ((realY - minY) * (graphHeight - totalMargins - totalMargins) / (absoluteMaxY - minY));
		//to avoid /0 problems (eg raceAnalyzer change person: absoluteMaxY-minY = 0

		/* debug:
		LogB.Information(string.Format("graphHeight: {0}, totalMargins: {1}, restOfFormula: {2}",
					graphHeight, totalMargins,
					(realY - minY) * UtilAll.DivideSafe (
				graphHeight - totalMargins - totalMargins,
				absoluteMaxY - minY) ));
		*/

                return graphHeight -getMargins (Directions.B) - ((realY - minY) * UtilAll.DivideSafe (
				graphHeight - getMargins (Directions.BT),
				absoluteMaxY - minY));
        }

	//when you want to plot another value, so minY, maxY is different
	protected double calculatePaintYProportion (double YProp) //from 0 to 1
	{
                return graphHeight -getMargins (Directions.B) - (YProp * (graphHeight - getMargins (Directions.BT)));
	}

	// Fast calculatePaintX/Y, sadly for 10000 points the difference between fast and slow is very low

	/*
	   paintX = totalMargins + (realX - minX) * (graphWidth - totalMargins - totalMargins) / (absoluteMaxX - minX);
	   take out realX:
	     paintX = A + realX * B
	     A = totalMargins - minX  *  (graphWidth - totalMargins - totalMargins) / (absoluteMaxX - minX)
	     B = (graphWidth - totalMargins - totalMargins) / (absoluteMaxX - minX)
	   */
	protected void calculatePaintXFastPre (out double A, out double B)
	{
		//A = totalMargins - minX  *  (graphWidth - totalMargins - totalMargins) / (absoluteMaxX - minX);
		A = getMargins (Directions.L) - minX  *  UtilAll.DivideSafe ( //or Directions.L ?
				graphWidth - getMargins (Directions.LR),
				absoluteMaxX - minX);

		//B = (graphWidth - totalMargins - totalMargins) / (absoluteMaxX - minX);
		B = UtilAll.DivideSafe (graphWidth - getMargins (Directions.LR), absoluteMaxX - minX);

		return;
	}
	protected double calculatePaintXFastDo (double realX, double A, double B)
	{
                return A + realX * B;
        }

	/*
	   paintY = graphHeight - totalMargins - ((realY - minY) * (graphHeight - totalMargins - totalMargins) / (absoluteMaxY - minY))
                //x = g - t - ((r - m) * (g - 2*t) / (a - m));

	   take out realY:
	     paintY = A + realY * B
	     By = - (graphHeight - 2*totalMargins) / (absoluteMaxY - minY))
	     Ay = graphHeight - totalMargins - minY * B
	   */
	protected void calculatePaintYFastPre (out double A, out double B)
	{
		B = UtilAll.DivideSafe(
				- (graphHeight - getMargins (Directions.BT)),
				absoluteMaxY - minY);

		A = graphHeight - getMargins (Directions.B) - minY * B; //or Directions.T ?

		return;
	}
	protected double calculatePaintYFastDo (double realY, double A, double B)
	{
                return A + realY * B;
        }


	//calculate real (from graph coordinates)

	private double calculateRealX (double graphX)
	{
                return minX + ( (graphX - getMargins (Directions.L)) * (absoluteMaxX - minX) / (graphWidth - getMargins (Directions.LR)) );
	}
	private double calculateRealY (double graphY)
	{
		return minY - (graphY - graphHeight + getMargins (Directions.T)) * (absoluteMaxY - minY) / (graphHeight - getMargins (Directions.BT));
        }

	//TODO: delete this method (see gui/cairo/jumpsDjOptimalFall.cs), use only the protected below
	public void CalculateAndWriteRealXY (double graphX, double graphY)
	{
		writeCoordinatesOfMouseClick(graphX, graphY, calculateRealX(graphX), calculateRealY(graphY));
	}
	protected void calculateAndWriteRealXY ()
	{
		writeCoordinatesOfMouseClick (mouseX, mouseY,
				calculateRealX(mouseX), calculateRealY(mouseY));
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

	protected void drawCircle (double x, double y, double radio, Cairo.Color color, bool filled)
	{
		g.MoveTo(x +radio, y);
		g.Arc(x, y, radio, 0.0, 2.0 * Math.PI); //full circle
		g.SetSourceColor(color);

		if (filled)
			g.Fill();

		g.Stroke();
	}

	// Thought for signals like forceSensor where points_l.X is time in microseconds and there is not a sample for each second
	// but it is also used on encoder
	//time on X
	protected int configureTimeWindowHorizontal (List<PointF> points_l, int seconds, int rightMarginSeconds, int multiplier)
	{
		double lastTime = points_l[points_l.Count -1].X;

		absoluteMaxX = lastTime + rightMarginSeconds * multiplier;
		if (absoluteMaxX < seconds * multiplier)
			absoluteMaxX = seconds * multiplier;

		int startAt = PointF.FindSampleAtTimeToEnd (points_l, (seconds -rightMarginSeconds) * multiplier);
		minX = points_l[startAt].X;

		return startAt;
	}
	//time on Y
	protected int configureTimeWindowVertical (List<PointF> points_l, int seconds, int rightMarginSeconds, int multiplier)
	{
		double lastTime = points_l[points_l.Count -1].Y;

		absoluteMaxY = lastTime + rightMarginSeconds * multiplier;
		if (absoluteMaxY < seconds * multiplier)
			absoluteMaxY = seconds * multiplier;

		int startAt = PointF.FindSampleAtTimeToEndDateY (points_l, (seconds -rightMarginSeconds) * multiplier);
		minY = points_l[startAt].Y;

		return startAt;
	}

	protected void paintSignalSubtitles (List<string> subtitleWithSetsInfo_l)
	{
		Cairo.TextExtents teAB = g.TextExtents (subtitleWithSetsInfo_l[0]);
		Cairo.TextExtents teCD = g.TextExtents (subtitleWithSetsInfo_l[1]);

		g.SetSourceColor (black);
		printText (graphWidth /2 -(teAB.Width + teCD.Width + 30)/2, graphHeight -bottomMargin -innerMargin/2,
				0, textHeight, subtitleWithSetsInfo_l[0], g, alignTypes.LEFT);

		g.SetSourceColor (grayDark);
		printText (graphWidth /2 +(teAB.Width + teCD.Width + 30)/2, graphHeight -bottomMargin -innerMargin/2,
				0, textHeight, subtitleWithSetsInfo_l[1], g, alignTypes.RIGHT);
	}

	protected Asteroids asteroids;

	//to be able to call from forceSensor and from encoder
	protected void asteroidsPlot (PointF lastPoint, int startAt, int multiplier,
			int marginAfterInSeconds, List<PointF> points_l, bool horizontal,
			ref double lastShot, ref double lastPointUp)
	{
		double lastPointDate = lastPoint.X;
		if (! horizontal)
			lastPointDate = lastPoint.Y;

		// paint asteroids and manage crashes
		List<Asteroid> aPaintable_l = asteroids.GetAllAsteroidsPaintable (lastPointDate, marginAfterInSeconds);

		List<Point3F> aPainted_l = new List <Point3F> ();
		double ax, ay;
		Cairo.Color colorShield = black;
		if (asteroids.Dark)
			colorShield = white;
		foreach (Asteroid a in aPaintable_l)
		{
			if (horizontal)
			{
				ax = graphWidth - (a.GetTimeNowProportion (lastPointDate, marginAfterInSeconds) *
						(graphWidth -getMargins (Directions.LR)) + getMargins (Directions.L));
				ay = calculatePaintY (a.GetYNow (lastPointDate, marginAfterInSeconds));
			} else {
				ax = calculatePaintX (a.GetYNow (lastPointDate, marginAfterInSeconds));
				ay = a.GetTimeNowProportion (lastPointDate, marginAfterInSeconds) *
						(graphHeight -getMargins (Directions.BT)) + getMargins (Directions.T);
			}

			drawCircle (ax, ay, a.Size, a.Color, true);
			if (a.Shield > 0)
			{
				drawCircle (ax, ay, a.Size + 2, colorShield, false);
				if (a.Shield > 1)
				{
					drawCircle (ax, ay, a.Size + 4, colorShield, false);
					if (a.Shield > 2)
						drawCircle (ax, ay, a.Size + 6, colorShield, false);
				}
			}

			if (asteroids.DoesAsteroidCrashedWithPlayer (ax, ay, a.Size,
					calculatePaintX (lastPoint.X), calculatePaintY (lastPoint.Y)))
			{
				asteroids.AsteroidCrashedWithPlayerSetTime (lastPointDate);
				a.Destroy ();
				asteroids.Points -= 20;
			}

			aPainted_l.Add (new Point3F (ax, ay, a.Size));
		}

		double sx, sy;
		//manage shots
		foreach (Shot s in asteroids.GetAllShotsPaintable (lastPointDate))
		{
			if (horizontal)
			{
				sx = calculatePaintX (s.GetXNow (lastPointDate));
				sy = calculatePaintY (s.Ystart);
			} else {
				sx = calculatePaintX (s.Ystart);
				sy = calculatePaintY (s.GetXNow (lastPointDate));
			}
			//LogB.Information (string.Format ("shot: {0}, {1}", sx, sy));

			int i = 0;
			Asteroid asteroid;
			Asteroids.ShotCrashedEnum sce = asteroids.ShotCrashedWithAsteroid (
					sx, sy, s.Size, aPaintable_l, aPainted_l, out i, out asteroid);
			if (sce == Asteroids.ShotCrashedEnum.CRASHEDANDDESTROY)
			{
				asteroids.Points += asteroid.PointsOnDestroy;
				asteroids.AddAsteroidPoint (new AsteroidPoint (
							DateTime.Now, aPainted_l[i].X, aPainted_l[i].Y, asteroid.PointsOnDestroy));
				s.Alive = false;
			} else if (sce == Asteroids.ShotCrashedEnum.CRASHEDNODESTROY)
			{
				s.Alive = false;
			} else
				asteroids.PaintShot (s, sx, sy, lastPoint.X, horizontal, g);
		}

		if (asteroids.Dark)
			g.SetSourceColor (white);
		else
			g.SetSourceColor (black);

		foreach (AsteroidPoint ap in asteroids.GetAllAsteroidPointsPaintable ())
			printText (ap.XGraph, ap.YGraph, 0, textHeight+2,
					string.Format ("+{0}", ap.Points), g, alignTypes.CENTER);

		//add 1 point each s
		if (lastPointDate >= lastPointUp + multiplier)
		{
			asteroids.Points ++;
			lastPointUp = lastPointDate;
		}

		// print points
		g.SetFontSize (textHeight +8);

		printText (graphWidth -rightMargin -innerMargin, .66*topMargin, 0, textHeight +4,
				"Points: " + asteroids.Points.ToString (), g, alignTypes.RIGHT);

		g.SetFontSize (textHeight);

		if (points_l.Count > 3 && lastPointDate >= lastShot + UtilAll.DivideSafe (multiplier, asteroids.ShotsFrequency))
		{
			//create new shot
			if (horizontal)
				asteroids.Shot (lastPoint);
			else
				asteroids.Shot (new PointF (lastPoint.Y, lastPoint.X));

			lastShot = lastPointDate;
		}

		asteroids.PaintShip (calculatePaintX (lastPoint.X), calculatePaintY (lastPoint.Y), lastPointDate, g);
	}
}
