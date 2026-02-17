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


public class CairoPaintBarsPreRunSimple : CairoPaintBarsPre
{
	private bool runTimes;
	private bool metersSecondsPreferred;

	public CairoPaintBarsPreRunSimple (DrawingArea darea, string fontStr,
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

	public override void StoreEventGraphRuns (PrepareEventGraphRunSimple eventGraph)
	{
		this.eventGraphRunsStored = eventGraph;
	}

	protected override bool storeCreated ()
	{
		return (eventGraphRunsStored != null);
	}

	protected override bool haveDataToPlot()
	{
		return (eventGraphRunsStored.rowsAtSQL.Count > 0);
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
		cb.XVariable = Catalog.GetString (eventGraphRunsStored.OrderX.ToString ());

		//cb.GraphInit(fontStr, ! ShowPersonNames, true); //usePersonGuides, useGroupGuides
		cb.GraphInit(fontStr, true, true); //usePersonGuides, useGroupGuides

		List<Event> events = Run.RunListToEventList(eventGraphRunsStored.rowsAtSQL);

		//find if there is a simulated
		bool thereIsASimulated = false;
		for (int i=0 ; i < eventGraphRunsStored.rowsAtSQL.Count; i++)
		{
			if(eventGraphRunsStored.rowsAtSQL[i].Simulated == -1)
				thereIsASimulated = true;

			if(! ShowPersonNames)
				eventGraphRunsStored.rowsAtSQL[i].Description = ""; //to avoid showing description
		}

		calculateBottomParams (events, eventGraphRunsStored.TypeForGraph == "", "",
				"(" + Catalog.GetString("Simulated") + ")", thereIsASimulated, false);

		List<PointF> point_l = new List<PointF>();
		List<string> names_l = new List<string>();
		List<bool> personIcon_l = new List<bool>();
		List<int> id_l = new List<int>(); //the uniqueIDs for knowing them on bar selection

		int countToDraw = eventGraphRunsStored.rowsAtSQL.Count;
		foreach (Run run in eventGraphRunsStored.rowsAtSQL)
		{
			// 1) Add data
			run.MetersSecondsPreferred = metersSecondsPreferred;
			if (runTimes)
				point_l.Add(new PointF(countToDraw --, run.Time));
			else
				point_l.Add(new PointF(countToDraw --, run.Speed));

			// 2) Add bottom names
			string typeRowString = "";
			if (eventGraphRunsStored.TypeForGraph == "") //if "all runs" show run.Type
				typeRowString = run.Type;

			names_l.Add(createTextBelowBar(
						"",
						typeRowString,
						run.Description,
						thereIsASimulated, (run.Simulated == -1),
						longestWord.Length, maxRowsForText));

			personIcon_l.Add (personName == "" && currentPersonID >= 0 && run.PersonID == currentPersonID);

			id_l.Add(run.UniqueID);

			if (eventGraphRunsStored.selectedID == run.UniqueID)
				cb.SelectedPos = eventGraphRunsStored.rowsAtSQL.Count -countToDraw -1;
		}

		cb.Id_l = id_l;
		cb.PersonIcon_l = personIcon_l;

		if (eventGraphRunsStored.HistoricalExStr != "")
		{
			cb.BestPersonExHistoricalD = eventGraphRunsStored.HistoricalExD;
			cb.BestPersonExHistoricalStr = eventGraphRunsStored.HistoricalExStr;
		}

		cb.PassBoxplots (eventGraphRunsStored.BoxplotPerson, eventGraphRunsStored.BoxplotSession);
		// pass selectedEvent to plot if it's not part of the shown events
		if (eventGraphRunsStored.selectedEvent != null)
		{
			if (runTimes)
				cb.SelectedDouble = ((Run) eventGraphRunsStored.selectedEvent).Time;
			else {
				//cb.SelectedDouble = ((Run) eventGraphRunsStored.selectedEvent).Speed; //note this will be always Km/h
				Run r = (Run) eventGraphRunsStored.selectedEvent;
				r.MetersSecondsPreferred = metersSecondsPreferred;
				cb.SelectedDouble = r.Speed;
			}
		}

		cb.PassData1Serie (point_l,
				new List<Cairo.Color>(), names_l,
				new List<List<double>> (),
				-1, fontHeightForBottomNames, bottomMargin, title,
				new List<int> (), new List<int> (), barsOrPoints);

		passDataForScreenshotIfNeeded ();

		cb.GraphDo();
	}

	// to show historic data even if in this session user has not data on that ex.
	protected override double getHistoricD ()
	{
		return eventGraphRunsStored.HistoricalExD;
	}
	protected override string getHistoricStr ()
	{
		if (eventGraphRunsStored.HistoricalExStr == "")
			return "";
		else
			return eventGraphRunsStored.HistoricalExStr;
	}
}
