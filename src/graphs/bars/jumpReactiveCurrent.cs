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
 * Copyright (C) 2004-2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Gtk;
using System.Collections.Generic; //List
using Mono.Unix;


//realtime jump reactive capture
public class CairoPaintBarsPreJumpReactiveRealtimeCapture : CairoPaintBarsPre
{
	//private double lastTv;
	//private double lastTc;
	private List<double> secondary_l; //bar of the left
	private List<double> main_l; //bar of the right
	private List<Cairo.Color> colorMain_l;
	private List<Cairo.Color> colorSecondary_l;
	private FeedbackJumpsRj feedbackJumpsRj;

	// these are lists because on Runs best speed and best time can be sent,
	// and in the future maybe there are other criterias eg. for encoder
	private List<int> best_l;
	private List<int> worst_l;

	//just blank the screen
	public CairoPaintBarsPreJumpReactiveRealtimeCapture (DrawingArea darea, string fontStr)
	{
		blankScreen(darea, fontStr);
	}

	//isLastCaptured: if what we are showing is currentJumpRj then true, if is a selection from treeview and id != currentJumpRj then is false (meaning selected)

	public CairoPaintBarsPreJumpReactiveRealtimeCapture (DrawingArea darea, string fontStr,
			Constants.Modes mode, string personName, string testName, int pDN,// bool heightPreferred,
			//double lastTv, double lastTc,
			string tvString, string tcString, bool isLastCaptured,
			FeedbackJumpsRj feedbackJumpsRj, double videoTime)
	{
		initialize (darea, fontStr, mode, personName, testName, pDN);
		this.feedbackJumpsRj = feedbackJumpsRj;
		this.videoTime = videoTime;

		if(isLastCaptured)
			this.title = Catalog.GetString("Last test:") + " " + generateTitle();
		else
			this.title = Catalog.GetString("Viewing:") + " " + generateTitle();

		//this.lastTv = lastTv;
		//this.lastTc = lastTc;

		secondary_l = new List<double>();
		main_l = new List<double>();

		if (UseHeights)
		{
			List<double> tv_l = JumpRj.HeightListFromTvString (tvString);
			foreach(double d in tv_l)
				main_l.Add (d);
		} else {
			string [] tvFull = tvString.Split(new char[] {'='});
			string [] tcFull = tcString.Split(new char[] {'='});
			if(tvFull.Length != tcFull.Length)
				return;

			foreach(string tv in tvFull)
				if(Util.IsNumber(tv, true))
					main_l.Add(Convert.ToDouble(tv));
			foreach(string tc in tcFull)
				if(Util.IsNumber(tc, true))
					secondary_l.Add(Convert.ToDouble(tc));
		}

		if (! UseHeights)
		{
			if (feedbackJumpsRj.EmphasizeBestTvTc)
				best_l = getBestWorstTimesList (true);
			else
				best_l = new List<int> ();

			if (feedbackJumpsRj.EmphasizeWorstTvTc)
				worst_l = getBestWorstTimesList (false);
			else
				worst_l = new List<int> ();
		}

		colorMain_l = new List<Cairo.Color>();
		colorSecondary_l = new List<Cairo.Color>();
	}

	private List<int> getBestWorstTimesList (bool best)
	{
		int jump = -1;
		double jumpValue = 0;
		for (int i = 0; i < secondary_l.Count; i ++)
			if (secondary_l[i] > 0 &&
					( jump == -1 ||
					  (best && main_l[i] / secondary_l[i] > jumpValue) ||
					  (! best && main_l[i] / secondary_l[i] < jumpValue) ) )
			{
				jumpValue = main_l[i] / secondary_l[i];
				jump = i;
			}

		List<int> l = new List<int> ();
		if (jump >= 0)
			l.Add (jump);

		return l;
	}

	/*
	public override void StoreEventGraphJumpReactiveCapture (PrepareEventGraphJumpReactiveRealtimeCapture eventGraph)
	{
		this.eventGraphJumpReactiveCapture = eventGraph;
	}
	*/

	protected override bool storeCreated ()
	{
		return (main_l.Count == secondary_l.Count && main_l.Count > 0);
	}

	protected override bool haveDataToPlot()
	{
		return (main_l.Count == secondary_l.Count && main_l.Count > 0);
	}

	protected override void paintSpecific()
	{
		//extra check
		if(main_l.Count != secondary_l.Count)
			return;

		if (UseHeights)
			cb = new CairoBars1Series (darea, CairoBars.Type.NORMAL, CairoGeneric.MouseClickable.NO, true, CairoBars.PaintGridEnum.ALL);
		else
			cb = new CairoBarsNHSeries (darea, CairoBars.Type.NORMAL, true, CairoGeneric.MouseClickable.NO, true, CairoBars.PaintGridEnum.ALL);

		if(UseHeights) {
			cb.YVariable = Catalog.GetString("Height");
			cb.YUnits = "cm";
			cb.VariableSerieB = Catalog.GetString("Jump height");
		} else {
			cb.YVariable = Catalog.GetString("Time");
			cb.YUnits = "s";

			cb.VariableSerieA = Catalog.GetString("Contact time");
			cb.VariableSerieB = Catalog.GetString("Flight time");
		}

		cb.GraphInit(fontStr, true, false); //usePersonGuides, useGroupGuides

		List<PointF> pointA_l = new List<PointF>();
		List<PointF> pointB_l = new List<PointF>();
		List<string> names_l = new List<string>();

		//statistics for tv
		double max = 0;
		double sum = 0; //for main_l avg
		double min = 1000;

		for(int i = 0; i < main_l.Count; i ++)
		{
			double a = 0;
			double b = 0;

			if (UseHeights)
				b = Util.GetHeightInCm (Convert.ToDouble(main_l[i]));
			else {
				a = Convert.ToDouble(secondary_l[i]);
				b = Convert.ToDouble(main_l[i]);
			}

			pointA_l.Add(new PointF(i+1, a));
			pointB_l.Add(new PointF(i+1, b));
			names_l.Add((i+1).ToString());

			//get max (only of tv)
			if(b > max)
				max = b;

			//get avg (only of tv)
			sum += Convert.ToDouble(b);

			//get min (only of tv)
			if(b < min)
				min = b;
		}

		feedbackJumpsRj.ResetBestSetValue ();
		feedbackJumpsRj.UpdateBestSetValue (max);

		for(int i = 0; i < main_l.Count; i ++)
		{
			double a = 0;
			double b = 0;

			if (UseHeights)
				b = Util.GetHeightInCm (Convert.ToDouble(main_l[i]));
			else {
				a = Convert.ToDouble(secondary_l[i]);
				b = Convert.ToDouble(main_l[i]);
			}

			if (UseHeights)
			{
				colorMain_l.Add (feedbackJumpsRj.AssignColorMainByHeight (b));
			} else {
				colorMain_l.Add (feedbackJumpsRj.AssignColorMain (b));
				colorSecondary_l.Add (feedbackJumpsRj.AssignColorSecondary (a));
			}
		}

		cb.PassGuidesData (new CairoBarsGuideManage(
					true, false, //usePersonGuides, useGroupGuides
					0,
					0,
					0,
					0,
					max,
					sum / main_l.Count,
					min));

		if (UseHeights)
			cb.PassData1Serie (pointB_l,
					colorMain_l, names_l,
					new List<List<double>> (),
					-1, 14, 8,
					title, best_l, worst_l, CairoBars.BarsOrPoints.BARS);
		else {
			List<List<PointF>> barsSecondary_ll = new List<List<PointF>>();
			barsSecondary_ll.Add(pointA_l);
			cb.PassData2Series (pointB_l, barsSecondary_ll, false,
					colorMain_l, colorSecondary_l, names_l,
					"", false,
					-1, 14, 8, title, best_l, worst_l, CairoBars.BarsOrPoints.BARS);
		}

		if (videoTime > 0)
			cb.VideoPlayTimeInSeconds = videoTime;

		passDataForScreenshotIfNeeded ();

		cb.GraphDo();
	}
}
