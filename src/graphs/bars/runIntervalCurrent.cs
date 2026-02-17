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


public class CairoPaintBarsPreRunIntervalRealtimeCapture : CairoPaintBarsPre
{
	private bool runTimes;
	private bool metersSecondsPreferred;
	private bool isRelative; //related to names: distance and time
	//private bool ifRSAstartRest; //on RSA if rest starts, this is true and graph do not need to be updated.
					//but if it is last one then should be painted
					//better manage it different

	private List<double> distance_l;
	private List<double> time_l;
	private List<double> speed_l;
	private List<int> photocell_l;
	private List<Cairo.Color> colorMain_l;
	private FeedbackRunsInterval feedbackRunsI;

	// these are lists because on Runs best speed and best time can be sent,
	// and in the future maybe there are other criterias eg. for encoder
	private List<int> best_l;
	private List<int> worst_l;

	//just blank the screen
	public CairoPaintBarsPreRunIntervalRealtimeCapture (DrawingArea darea, string fontStr)
	{
		blankScreen(darea, fontStr);
	}

	public CairoPaintBarsPreRunIntervalRealtimeCapture (DrawingArea darea, string fontStr,
			Constants.Modes mode, string personName, string testName, int pDN,
			bool runTimes, bool metersSecondsPreferred,
			bool isRelative,
			string timesString,
			double distanceInterval, //know each track distance according to this or distancesString
			string distancesString,
			List<int> photocell_l, bool isLastCaptured, FeedbackRunsInterval feedbackRunsI, double videoTime)
	{
		initialize (darea, fontStr, mode, personName, testName, pDN);

		this.runTimes = runTimes;
		this.metersSecondsPreferred = metersSecondsPreferred;
		this.feedbackRunsI = feedbackRunsI;
		this.videoTime = videoTime;

		if(isLastCaptured)
			this.title = Catalog.GetString("Last test:") + " " + generateTitle();
		else
			this.title = generateTitle();

		this.isRelative = isRelative;

		distance_l = new List<double>();
		time_l = new List<double>();
		speed_l = new List<double>();
		this.photocell_l = photocell_l;

		string [] timeFull = timesString.Split(new char[] {'='});
		int count = 0;
		foreach(string t in timeFull)
		{
			if(distancesString != null && distancesString != "") //if distances are variable
			{
				//this will return a 0 on Rest period on RSA
				distanceInterval = Util.GetRunIVariableDistancesStringRow(distancesString, count);
			}

			//ifRSAstartRest = true;
			if(distanceInterval > 0  //is not RSA rest period
					&&
				Util.IsNumber(t, true))
			{
				double tDouble = Convert.ToDouble(t);
				double time = 0;
				if(tDouble < 0)
					time = 0;
				else
					time = tDouble;

				time_l.Add(time);
				distance_l.Add(distanceInterval);
				if (metersSecondsPreferred)
					speed_l.Add(distanceInterval / time);
				else
					speed_l.Add(3.6 * distanceInterval / time);
				//ifRSAstartRest = false;
			}
			count ++;
		}

		/*
		//debug
		LogB.Information("distances:");
		foreach (double distance in distance_l)
			LogB.Information(distance.ToString());
		LogB.Information("times:");
		foreach (double time in time_l)
			LogB.Information(time.ToString());
		LogB.Information("speeds:");
		foreach (double speed in speed_l)
			LogB.Information(speed.ToString());
		*/

		best_l = new List<int> ();
		if (feedbackRunsI.EmphasizeBestSpeed)
			best_l = getBestWorstList (best_l, speed_l, true);
		if (feedbackRunsI.EmphasizeBestTime)
			best_l = getBestWorstList (best_l, time_l, false);

		worst_l = new List<int> ();
		if (feedbackRunsI.EmphasizeWorstSpeed)
			worst_l = getBestWorstList (worst_l, speed_l, false);
		if (feedbackRunsI.EmphasizeWorstTime)
			worst_l = getBestWorstList (worst_l, time_l, true);

		colorMain_l = new List<Cairo.Color>();
	}

	private List<int> getBestWorstList (List<int> return_l, List<double> find_l, bool higher)
	{
		int run = -1;
		double runValue = 0;
		for (int i = 0; i < find_l.Count; i ++)
			if (find_l[i] > 0 &&
					( run == -1 ||
					  (higher && find_l[i] > runValue) ||
					  (! higher && find_l[i] < runValue) ) )
			{
				runValue = find_l[i];
				run = i;
			}

		if (run >= 0)
			return_l.Add (run);

		return return_l;
	}
	/*
	public override void StoreEventGraphJumpReactiveCapture (PrepareEventGraphJumpReactiveRealtimeCapture eventGraph)
	{
		this.eventGraphJumpReactiveCapture = eventGraph;
	}
	*/

	protected override bool storeCreated ()
	{
		return (speed_l.Count == time_l.Count && speed_l.Count > 0);
	}

	protected override bool haveDataToPlot()
	{
		return (speed_l.Count == time_l.Count && speed_l.Count > 0);
	}

	protected override void paintSpecific()
	{
		//extra check
		if(speed_l.Count != time_l.Count)
			return;

		//if(ifRSAstartRest)
		//	return;

		cb = new CairoBars1Series (darea, CairoBars.Type.NORMAL, CairoGeneric.MouseClickable.NO, true, CairoBars.PaintGridEnum.ALL);

		cb.YVariable = Catalog.GetString("Speed");
		if (runTimes)
			cb.YVariable = Catalog.GetString("Time");

		if (runTimes)
			cb.YUnits = "s";
		else {
			if (metersSecondsPreferred)
				cb.YUnits = "m/s";
			else
				cb.YUnits = "Km/h";
		}

		cb.GraphInit(fontStr, true, false); //usePersonGuides, useGroupGuides

		List<PointF> point_l = new List<PointF>();
		List<string> names_l = new List<string>();

		//statistics for speed
		double max = 0;
		double min = 1000;

		//for absolute data. Absolute is from the beginning.
		double distanceTotal = 0;
		double timeTotal = 0;
		for(int i = 0; i < time_l.Count; i ++)
		{
			distanceTotal += distance_l[i];
			timeTotal += time_l[i];
		}
		double distanceAccumulated = 0;
		double timeAccumulated = 0;

		for(int i = 0; i < time_l.Count; i ++)
		{
			double time = Convert.ToDouble(time_l[i]);
			double speed = Convert.ToDouble(speed_l[i]);

			double param = speed;
			if (runTimes)
				param = time;

			point_l.Add(new PointF(i+1, param));

			if(isRelative)
				names_l.Add(string.Format("{0} m\n{1} s",
							Util.TrimDecimals (distance_l[i], 2), Util.TrimDecimals(time,2)));
			else {
				distanceAccumulated += distance_l[i];
				timeAccumulated += time_l[i];
				names_l.Add(string.Format("{0} m\n{1} s",
							Util.TrimDecimals (distanceAccumulated, 2), Util.TrimDecimals(timeAccumulated,2)));
			}

			if (param > max) 	//get max
				max = param;
			if (param < min)	//get min
				min = param;

			colorMain_l.Add (feedbackRunsI.AssignColorMain (speed, time));
		}

		double guidesValue = UtilAll.DivideSafe (distanceTotal, timeTotal);
		if (runTimes)
			guidesValue = UtilAll.DivideSafe (timeTotal, time_l.Count);

		cb.PassGuidesData (new CairoBarsGuideManage(
					true, false, //usePersonGuides, useGroupGuides
					0, 0, 0, 0,
					max,
					guidesValue,
					min));
		/*
		   if(photocell_l.Count > 0)
			cb.InBarNums_l = photocell_l;
		 */
		if(photocell_l.Count > 0)
			cb.EdgeBarNums_l = photocell_l;

		cb.SpaceBetweenBars = false;

		cb.PassData1Serie (point_l,
				colorMain_l, names_l,
				new List<List<double>> (),
				-1, 14, 22, title, //22 because there are two rows
				best_l, worst_l, CairoBars.BarsOrPoints.BARS);

		if (videoTime > 0)
		{
			cb.VideoPlayTimeInSeconds = videoTime;

			//cb.VideoPlayTimes_l = time_l; //VideoPlayTimes is accumulative)
			cb.VideoPlayTimes_l = UtilList.Cumsum (time_l);
		}

		passDataForScreenshotIfNeeded ();

		cb.GraphDo();
	}
}
