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


public class CairoPaintBarsPreForceSensor : CairoPaintBarsPre
{
	public CairoPaintBarsPreForceSensor (DrawingArea darea, string fontStr, Constants.Modes mode,
			string personName, string testName, int pDN,
			int currentPersonID, bool drawBars)
	{
		LogB.Information ("CairoPaintBarsPreForceSensor constructor");
		initialize (darea, fontStr, mode, personName, testName, pDN);
		this.title = generateTitle();
		this.currentPersonID = currentPersonID;
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
		cb = new CairoBarsNHSeries (darea, CairoBars.Type.NORMAL, true, true, true, true);

		cb.YVariable = Catalog.GetString("Force");
		cb.YUnits = "N";
		cb.VariableSerieA = Catalog.GetString("Max force");
		cb.VariableSerieB = Catalog.GetString("Best second");

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

		List<PointF> pointA_l = new List<PointF>();
		List<PointF> pointB_l = new List<PointF>();
		List<string> names_l = new List<string>();
		List<bool> personIcon_l = new List<bool>();
		List<int> id_l = new List<int>(); //the uniqueIDs for knowing them on bar selection

		int countToDraw = eventGraphForceSensorStored.rowsAtSQL.Count;
		foreach (ForceSensor fs in eventGraphForceSensorStored.rowsAtSQL)
		{
			//LogB.Information("forceSensor: " + fs.ToString());
			// 1) Add data
			pointA_l.Add (new PointF (countToDraw, fs.MaxForceRaw));
			pointB_l.Add (new PointF (countToDraw, fs.MaxAvgForce1s));
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
			id_l.Add (fs.UniqueID);

			if (eventGraphForceSensorStored.selectedID == fs.UniqueID)
				cb.SelectedPos = eventGraphForceSensorStored.rowsAtSQL.Count -countToDraw -1;
		}

		cb.Id_l = id_l;
		cb.PersonIcon_l = personIcon_l;

		/*
		cb.PassGuidesData (new CairoBarsGuideManage(
					//! ShowPersonNames, true, //usePersonGuides, useGroupGuides
					true, true, //usePersonGuides, useGroupGuides
					eventGraphJumpsStored.sessionMAXAtSQL,
					eventGraphJumpsStored.sessionAVGAtSQL,
					eventGraphJumpsStored.sessionMINAtSQL,
					eventGraphJumpsStored.personMAXAtSQLAllSessions,
					eventGraphJumpsStored.personMAXAtSQL,
					eventGraphJumpsStored.personAVGAtSQL,
					eventGraphJumpsStored.personMINAtSQL));
		*/

		List<List<PointF>> barsSecondary_ll = new List<List<PointF>>();
		barsSecondary_ll.Add(pointA_l);

		cb.PassData2Series (pointB_l, barsSecondary_ll, false,
				new List<Cairo.Color>(), new List<Cairo.Color>(), names_l,
				"", false,
				-1, fontHeightForBottomNames, bottomMargin, title,
				new List<int> (), new List<int> (), barsOrPoints);

		passDataForScreenshotIfNeeded ();

		cb.GraphDo();
	}
}

