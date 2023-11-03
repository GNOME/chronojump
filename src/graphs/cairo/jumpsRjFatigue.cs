
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

public class JumpsRjFatigueGraph : CairoXY
{
	private List<double> tc_l;
	private List<double> tv_l;
	private int divideIn;

	//constructor when there are no points
	public JumpsRjFatigueGraph (DrawingArea area, string jumpType, string font)//, string title, string jumpType, string date)
	{
		this.area = area;

		initGraph(font, .8);

		g.SetFontSize(16);
		//printText(area.Allocation.Width /2, area.Allocation.Height /2, 24, textHeight,
		//		string.Format("Need to execute jumps: {0}.", jumpType), g, true);

		endGraphDisposing(g, surface, area.Window);
	}

	//regular constructor
	public JumpsRjFatigueGraph (
			List<double> tc_l, List<double> tv_l,
			List<PointF> point_l, double slope, double intercept,
			DrawingArea area, string title, string jumpType, string date,
			JumpsRjFatigue.Statistic statistic, int divideIn,
			double mouseX, double mouseY)
	{
		this.tc_l = tc_l;
		this.tv_l = tv_l;
		this.point_l = point_l;
		this.slope = slope;
		this.intercept = intercept;
		this.area = area;
		this.title = title;
		this.jumpType = jumpType;
		this.date = date;
		this.divideIn = divideIn;
		this.colorBackground = colorFromRGBA(Config.ColorBackground); //but note if we are using system colors, this will not match
		this.mouseX = mouseX;
		this.mouseY = mouseY;

		xVariable = countStr;
		xUnits = "";

		if(statistic == JumpsRjFatigue.Statistic.HEIGHTS)
		{
			yVariable = heightStr;
			yUnits = "cm";
		} else if(statistic == JumpsRjFatigue.Statistic.Q)
		{
			yVariable = tfStr + "/" + tcStr;
			yUnits = "";
		} else //if(statistic == JumpsRjFatigue.Statistic.RSI)
		{
			yVariable = heightStr + "/" + tcStr;
			yUnits = "m/s";
		}

		area.AddEvents((int) Gdk.EventMask.ButtonPressMask); //to have mouse clicks
	}

	public override void Do(string font)
	{
		LogB.Information("at JumpsRjFatigueGraph.Do");
		initGraph(font, .8);

                findPointMaximums(false);
                //findAbsoluteMaximums();
		paintGrid (gridTypes.HORIZONTALLINES, true, 0);
		paintGrid (gridTypes.VERTICALLINES, false, 0);
		paintAxis();

		g.SetSourceColor(red);
		if(point_l.Count > 1)
			plotPredictedLine(predictedLineTypes.STRAIGHT, predictedLineCrossMargins.TOUCH);
		g.SetSourceColor(black);

		plotRealPoints(PlotTypes.POINTSLINES);

		writeTitle();
		g.SetSourceColor(gray99);
		divideAndPlotAverage (divideIn, true);
		g.SetSourceColor(black);

		addClickableMark(g);

		if(mouseX >= 0 && mouseY >= 0)
			calculateAndWriteRealXY ();

		endGraphDisposing(g, surface, area.Window);
	}

	//here X is year, add/subtract third of a year
	protected override void separateMinXMaxXIfNeeded()
	{
		if(minX == maxX || maxX - minX < .1) //<.1 means that maybe we will not see any vertical bar on grid, enlarge it
		{
			minX -= .1;
			maxX += .1;
		}
	}

	int ypos;
	protected override void writeTitle()
	{
		ypos = -6;

		writeTextAtRight(ypos++, title, true);
		writeTextAtRight(ypos++, jumpTypeStr + " " + jumpType, false);
		writeTextAtRight(ypos++, date, false);
	}

	private void divideAndPlotAverage (int parts, bool byTime) //byTime or byJumps
	{
//		if(point_l.Count < parts * 2)
//			return;

		List<List<PointF>> l_l = new List<List<PointF>> ();

		if (byTime)
			l_l = divideAndPlotAverageByTime (parts);
		else
			l_l = divideAndPlotAverageByJumps (parts);

		//return if not succeded on having at least one point on each part
		if (l_l.Count < parts)
			return;

		List<double> partAverage_l = new List<double> ();
		int done = 0;
		ypos ++;
		foreach(List<PointF> l in l_l)
		{
			if(done >= parts)
				break; //to fix i splitList returned more chunks than wanted

			double sum = 0;

			foreach(PointF p in l)
				sum += p.Y;

			partAverage_l.Add (sum / l.Count);

			/*
			 * an user had:
			 * tc_l: 5,51;0,34;0,16;0,21;0,43
			 * tv_l: 0,38;0,44;0,59;0,35;0,37
			 * totalTime: 8,777864, partRange: 4,388932
			 * so first part has no jumps making l.Count -1 fail
			 */
			if (l.Count > 0)
				paintHorizSegment (l[0].X, l[l.Count -1].X, sum / l.Count,
						(done + 1).ToString (), l.Count == 1);

			writeTextAtRight (ypos++,
					string.Format ("Mean {0}: {1}",
						done + 1, Util.TrimDecimals (sum / l.Count, 2) ),
					false);

			done ++;
		}

		if (parts >= 2 && parts == partAverage_l.Count)
		{
			double iniAvg = partAverage_l[0];
			int endPos = parts -1;
			double endAvg = partAverage_l[endPos];

			writeTextAtRight (ypos++,
					string.Format ("IRFR {0}/1: {1}",
						endPos +1,
						Util.TrimDecimals (UtilAll.DivideSafe (endAvg, iniAvg), 2)),
					true);
		}

	}

	private List<List<PointF>> divideAndPlotAverageByTime (int parts)
	{
		return splitListByTime (tc_l, tv_l, point_l, parts);
	}

	private List<List<PointF>> divideAndPlotAverageByJumps (int parts)
	{
		return splitListByJumps (point_l, Convert.ToInt32(Math.Floor(point_l.Count / (1.0 * parts))));
	}

	private void paintHorizSegment (double xa, double xb, double y, string centerText, bool segmentOfJustOnePoint)
	{
		double xap = calculatePaintX(xa);
		double xbp = calculatePaintX(xb);

		//if only one point, show the segment bigger
		//if (segmentOfJustOnePoint)
		//{
		//do it always
			xap -= 10;
			xbp += 10;
		//}

		double yp = calculatePaintY(y);

		g.MoveTo(xap, yp);
		g.LineTo(xbp, yp);

		//paint also small y marks
		g.MoveTo(xap, yp - pointsRadius);
		g.LineTo(xap, yp + pointsRadius);
		g.MoveTo(xbp, yp - pointsRadius);
		g.LineTo(xbp, yp + pointsRadius);

		/*
		printText(
				graphWidth - outerMargin,
				Convert.ToInt32(yp),
				0, textHeight, Util.TrimDecimals(y, 2), g, alignTypes.CENTER);
				*/
		printText ((xap + xbp)/2, Convert.ToInt32(yp) - 1.2*textHeight,
				0, textHeight, centerText, g, alignTypes.CENTER);

		g.Stroke ();
	}

	//this method puts a jump on a part or another depending on if tv start is before or after the parts cut
	private List<List<PointF>> splitListByTime (List<double> tc_l, List<double> tv_l, List<PointF> p_l, int parts)
	{
		//LogB.Information ("splitListByTime start");
		//LogB.Information (string.Format ("tc_l.Count: {0}, tv_l.Count: {1}, p_l.Count: {2}, parts: {3}",
		//			tc_l.Count, tv_l.Count, p_l.Count, parts));

		var l_l = new List<List<PointF>>();

		if (p_l.Count <= 1)
			return l_l;
		//LogB.Information ("splitListByTime start 2");

		//calculate totalTime to know partRange
		double totalTime = 0;
		for (int j = 0; j < tc_l.Count; j ++)
		{
			if (tc_l[j] > 0) //avoid tc == -1 on startIn jumps
				totalTime += tc_l[j];

			totalTime += tv_l[j];
		}

		double partRange = totalTime / (1.0 * parts);
		double partCurrent = 0;
		//double partStart = partCurrent * partRange;
		double partEnd = (partCurrent + 1) * partRange;
		double timeAccu = 0;
		List<PointF> pointsThisPart_l = new List<PointF>();

		//LogB.Information (string.Format ("totalTime: {0}, partRange: {1}", totalTime, partRange));
		for (int j = 0; j < tc_l.Count ; j ++)
		{
			double tcFixed = tc_l[j]; //fix -1 tc on start in jumps
			if (tcFixed < 0)
				tcFixed = 0;

			double tvStart = timeAccu + tcFixed;
			//LogB.Information (string.Format ("tvStart: {0}, partEnd: {1}, tvStart <= partEnd: {2}",
			//			tvStart, partEnd, tvStart <= partEnd));
			if (tvStart <= partEnd)
			{
				pointsThisPart_l.Add (p_l[j]);
				//LogB.Information (string.Format ("Add to part {0} point {1}", partCurrent, p_l[j]));
			}
			else {
				partCurrent ++;
				//partStart = partCurrent * partRange;
				partEnd = (partCurrent + 1) * partRange;
				l_l.Add (pointsThisPart_l);
				//LogB.Information (string.Format ("stored part {0}", partCurrent-1));

				pointsThisPart_l = new List<PointF>();
				pointsThisPart_l.Add (p_l[j]);
				//LogB.Information (string.Format ("Add to part {0} point {1}", partCurrent, p_l[j]));
			}
			timeAccu += tcFixed + tv_l[j];
		}
		l_l.Add (pointsThisPart_l);
		//LogB.Information (string.Format ("stored part {0}", partCurrent));

		return (l_l);
	}

	//https://stackoverflow.com/a/11463800
	private List<List<PointF>> splitListByJumps (List<PointF> p_l, int nSize)
	{
		var l_l = new List<List<PointF>>();

		for (int i = 0; i < p_l.Count; i += nSize)
			l_l.Add (p_l.GetRange (i, Math.Min (nSize, p_l.Count - i)));

		return l_l;
	}

}
