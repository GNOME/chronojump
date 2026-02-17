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
 * Copyright (C) 2004-2026   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Gtk;
using System.Collections.Generic; //List
using Mono.Unix;


public class CairoPaintBarsPreRunInterval : CairoPaintBarsPre
{
	private bool runTimes;
	private bool metersSecondsPreferred;

	public CairoPaintBarsPreRunInterval (DrawingArea darea, string fontStr,
			Constants.Modes mode, string personName, string testName, int pDN,
			bool runTimes, bool metersSecondsPreferred,
			int currentPersonID, bool drawBars)
	{
		initialize (darea, fontStr, mode, personName, testName, pDN);

		this.title = generateTitle();
		this.runTimes = runTimes;
		this.metersSecondsPreferred = metersSecondsPreferred;
		this.currentPersonID = currentPersonID;
		this.drawBars = drawBars;
	}

	public override void StoreEventGraphRunsInterval (PrepareEventGraphRunInterval eventGraph)
	{
		this.eventGraphRunsIntervalStored = eventGraph;
	}

	protected override bool storeCreated ()
	{
		return (eventGraphRunsIntervalStored != null);
	}

	protected override bool haveDataToPlot()
	{
		return (eventGraphRunsIntervalStored.rowsAtSQL.Count > 0);
	}

	protected override void paintSpecific()
	{
		cb = new CairoBars1Series (darea, CairoBars.Type.NORMAL, CairoGeneric.MouseClickable.CLICKLR, true, CairoBars.PaintGridEnum.ALL);

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
		cb.XVariable = Catalog.GetString (eventGraphRunsIntervalStored.OrderX.ToString ());

		//cb.GraphInit(fontStr, ! ShowPersonNames, true); //usePersonGuides, useGroupGuides
		cb.GraphInit(fontStr, true, true); //usePersonGuides, useGroupGuides

		List<Event> events = RunInterval.RunIntervalListToEventList (eventGraphRunsIntervalStored.rowsAtSQL);

		//find if there is a simulated
		bool thereIsASimulated = false;
		for(int i=0 ; i < eventGraphRunsIntervalStored.rowsAtSQL.Count; i++)
		{
			if(eventGraphRunsIntervalStored.rowsAtSQL[i].Simulated == -1)
				thereIsASimulated = true;

			if(! ShowPersonNames)
				eventGraphRunsIntervalStored.rowsAtSQL[i].Description = ""; //to avoid showing description
		}

		calculateBottomParams (events, true, " - 99", //thinking on 99 tracks
				"(" + Catalog.GetString("Simulated") + ")", thereIsASimulated, false);

		List<PointF> point_l = new List<PointF>();
		List<string> names_l = new List<string>();
		List<List<double>> intervals_l = new List<List<double>>();
		List<bool> personIcon_l = new List<bool>();
		List<int> id_l = new List<int>(); //the uniqueIDs for knowing them on bar selection

		int countToDraw = eventGraphRunsIntervalStored.rowsAtSQL.Count;
		foreach(RunInterval runI in eventGraphRunsIntervalStored.rowsAtSQL)
		{
			// 1) Add data
			runI.MetersSecondsPreferred = metersSecondsPreferred;
			if (runTimes)
			{
				point_l.Add(new PointF(countToDraw --, runI.TimeTotal));
				intervals_l.Add (runI.TimeList);
			} else {
				point_l.Add(new PointF(countToDraw --, runI.Speed));
				// TODO: intervals_l
			}

			// 2) Add bottom names
			/*
			string typeRowString = "";
			if (eventGraphRunsIntervalStored.type == "") //if "all runs" show run.Type
				typeRowString = runI.Type;
				*/
			//TYPE B: on runI show always run type to show at the side the number of tracks. If change here, change it above (TYPEA)
			string typeRowString = string.Format("{0} - {1}", runI.Type, runI.Tracks);

			names_l.Add(createTextBelowBar(
						"",
						typeRowString,
						runI.Description,
						thereIsASimulated, (runI.Simulated == -1),
						longestWord.Length, maxRowsForText));

			personIcon_l.Add (personName == "" && currentPersonID >= 0 && runI.PersonID == currentPersonID);

			id_l.Add(runI.UniqueID);

			if (eventGraphRunsIntervalStored.selectedID == runI.UniqueID)
				cb.SelectedPos = eventGraphRunsIntervalStored.rowsAtSQL.Count -countToDraw -1;
		}

		cb.Id_l = id_l;
		cb.PersonIcon_l = personIcon_l;

		if (eventGraphRunsIntervalStored.HistoricalExStr != "")
		{
			cb.BestPersonExHistoricalD = eventGraphRunsIntervalStored.HistoricalExD;
			cb.BestPersonExHistoricalStr = eventGraphRunsIntervalStored.HistoricalExStr;
		}

		cb.PassBoxplots (eventGraphRunsIntervalStored.BoxplotPerson, eventGraphRunsIntervalStored.BoxplotSession);
		// pass selectedEvent to plot if it's not part of the shown events
		if (eventGraphRunsIntervalStored.selectedEvent != null)
		{
			if (runTimes)
				cb.SelectedDouble = ((RunInterval) eventGraphRunsIntervalStored.selectedEvent).TimeTotal;
			else {
				//cb.SelectedDouble = ((RunInterval) eventGraphRunsIntervalStored.selectedEvent).Speed; //note this will be always Km/h
				RunInterval r = (RunInterval) eventGraphRunsIntervalStored.selectedEvent;
				r.MetersSecondsPreferred = metersSecondsPreferred;
				cb.SelectedDouble = r.Speed;
			}
		}

		cb.PassData1Serie (point_l,
				new List<Cairo.Color>(), names_l,
				intervals_l,
				-1, fontHeightForBottomNames, bottomMargin, title,
				new List<int> (), new List<int> (), barsOrPoints);

		passDataForScreenshotIfNeeded ();

		cb.GraphDo();
	}

	// to show historic data even if in this session user has not data on that ex.
	protected override double getHistoricD ()
	{
		return eventGraphRunsIntervalStored.HistoricalExD;
	}
	protected override string getHistoricStr ()
	{
		if (eventGraphRunsIntervalStored.HistoricalExStr == "")
			return "";
		else
			return eventGraphRunsIntervalStored.HistoricalExStr;
	}
}
