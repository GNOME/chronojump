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


public class CairoPaintBarsPreForceSensor : CairoPaintBarsPre
{
	private bool bestSecond;

	public CairoPaintBarsPreForceSensor (DrawingArea darea, string fontStr, Constants.Modes mode,
			string personName, string testName, int pDN,
			int currentPersonID, bool bestSecond, bool drawBars)
	{
		LogB.Information ("CairoPaintBarsPreForceSensor constructor");
		initialize (darea, fontStr, mode, personName, testName, pDN);
		this.title = generateTitle();
		this.currentPersonID = currentPersonID;
		this.bestSecond = bestSecond;
		this.drawBars = drawBars;
	}

	public override void StoreEventGraphForceSensor (PrepareEventGraphForceSensor eventGraph)
	{
		this.eventGraphForceSensorStored = eventGraph;
	}

	protected override bool storeCreated ()
	{
		return (eventGraphForceSensorStored != null);
	}

	protected override bool haveDataToPlot()
	{
		return (eventGraphForceSensorStored.rowsAtSQL.Count > 0);
	}

	protected override void paintSpecific()
	{
		LogB.Information ("CairoPaintBarsPreForceSensor paintSpecific");
		/*
		 * check if one bar has to be shown or two
		 * this is important when we are showing multitests
		 */
		cb = new CairoBars1Series (darea, CairoBars.Type.NORMAL, CairoGeneric.MouseClickable.CLICKLR, true, true);

		cb.YVariable = Catalog.GetString("Max force");
		if (bestSecond)
			cb.YVariable = Catalog.GetString("Best second");

		cb.YUnits = "N";
		cb.XVariable = Catalog.GetString (eventGraphForceSensorStored.OrderX.ToString ());

		//cb.GraphInit(fontStr, ! ShowPersonNames, true); //usePersonGuides, useGroupGuides
		cb.GraphInit(fontStr, true, true); //usePersonGuides, useGroupGuides

		List<Event> events = ForceSensor.ForceSensorListToEventList (eventGraphForceSensorStored.rowsAtSQL);

		for (int i=0 ; i < eventGraphForceSensorStored.rowsAtSQL.Count; i++)
			if(! ShowPersonNames)
				eventGraphForceSensorStored.rowsAtSQL[i].Description = ""; //to avoid showing description

		//findLongestWordCairo uses Type (from Event) but ForceSensor has ExerciseName
		for (int i = 0; i < events.Count; i ++)
			events[i].Type = ((ForceSensor) events[i]).ExerciseName;

		calculateBottomParams (events, eventGraphForceSensorStored.exerciseAll, "",
				"", false, false);

		List<PointF> point_l = new List<PointF>();
		List<string> names_l = new List<string>();
		List<bool> personIcon_l = new List<bool>();
		List<int> id_l = new List<int>(); //the uniqueIDs for knowing them on bar selection

		int countToDraw = eventGraphForceSensorStored.rowsAtSQL.Count;
		foreach (ForceSensor fs in eventGraphForceSensorStored.rowsAtSQL)
		{
			//LogB.Information("forceSensor: " + fs.ToString());
			// 1) Add data
			if (bestSecond)
				point_l.Add (new PointF (countToDraw, fs.MaxAvgForce1s));
			else
				point_l.Add (new PointF (countToDraw, fs.MaxForceRaw));
			countToDraw --;

			// 2) Add bottom names
			string typeRowString = "";
			if (eventGraphForceSensorStored.exerciseAll) //if "all tests" show type
				typeRowString = fs.ExerciseName; //TODO: check this param is filled

			names_l.Add (createTextBelowBar(
						"",
						typeRowString,
						fs.Description,
						false, false,
						longestWord.Length, maxRowsForText));

			personIcon_l.Add (personName == "" && currentPersonID >= 0 && fs.PersonID == currentPersonID);

			id_l.Add (fs.UniqueID);

			if (eventGraphForceSensorStored.selectedID == fs.UniqueID)
				cb.SelectedPos = eventGraphForceSensorStored.rowsAtSQL.Count -countToDraw -1;
		}

		cb.Id_l = id_l;
		cb.PersonIcon_l = personIcon_l;

		if (eventGraphForceSensorStored.HistoricalExStr != "")
		{
			cb.BestPersonExHistoricalD = eventGraphForceSensorStored.HistoricalExD;
			cb.BestPersonExHistoricalStr = eventGraphForceSensorStored.HistoricalExStr;
		}

		cb.PassBoxplots (eventGraphForceSensorStored.BoxplotPerson, eventGraphForceSensorStored.BoxplotSession);
		// pass selectedEvent to plot if it's not part of the shown events
		if (eventGraphForceSensorStored.selectedEvent != null)
		{
			if (bestSecond)
				cb.SelectedDouble = ((ForceSensor) eventGraphForceSensorStored.selectedEvent).MaxAvgForce1s;
			else {
				cb.SelectedDouble = ((ForceSensor) eventGraphForceSensorStored.selectedEvent).MaxForceRaw;
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
		return eventGraphForceSensorStored.HistoricalExD;
	}
	protected override string getHistoricStr ()
	{
		if (eventGraphForceSensorStored.HistoricalExStr == "")
			return "";
		else
			return eventGraphForceSensorStored.HistoricalExStr;
	}
}

