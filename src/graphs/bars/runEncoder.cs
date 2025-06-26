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


public class CairoPaintBarsPreRunEncoder : CairoPaintBarsPre
{
	public CairoPaintBarsPreRunEncoder (DrawingArea darea, string fontStr, Constants.Modes mode, string personName, string testName, int pDN)
	{
		LogB.Information ("CairoPaintBarsPreRunEncoder constructor");
		initialize (darea, fontStr, mode, personName, testName, pDN);
		this.title = generateTitle();
	}

	public override void StoreEventGraphRunEncoder (PrepareEventGraphRunEncoder eventGraph)
	{
		this.eventGraphRunEncoderStored = eventGraph;
	}

	protected override bool storeCreated ()
	{
		return (eventGraphRunEncoderStored != null);
	}

	protected override bool haveDataToPlot()
	{
		return (eventGraphRunEncoderStored.rowsAtSQL.Count > 0);
	}

	protected override void paintSpecific()
	{
		LogB.Information ("CairoPaintBarsPreRunEncoder paintSpecific");
		/*
		 * check if one bar has to be shown or two
		 * this is important when we are showing multitests
		 */
		cb = new CairoBarsNHSeries (darea, CairoBars.Type.NORMAL, true, true, true, true);

		cb.YVariable = Catalog.GetString("Speed");
		cb.YUnits = "m/s";
		cb.VariableSerieA = Catalog.GetString("Max speed");
		cb.VariableSerieB = Catalog.GetString("Best second");

		//cb.GraphInit(fontStr, ! ShowPersonNames, true); //usePersonGuides, useGroupGuides
		cb.GraphInit(fontStr, true, true); //usePersonGuides, useGroupGuides

		List<Event> events = RunEncoder.RunEncoderListToEventList (eventGraphRunEncoderStored.rowsAtSQL);

		for (int i=0 ; i < eventGraphRunEncoderStored.rowsAtSQL.Count; i++)
			if(! ShowPersonNames)
				eventGraphRunEncoderStored.rowsAtSQL[i].Description = ""; //to avoid showing description

		//findLongestWordCairo uses Type (from Event) but RunEncoder has ExerciseName
		for (int i = 0; i < events.Count; i ++)
			events[i].Type = ((RunEncoder) events[i]).ExerciseName;

		calculateBottomParams (events, eventGraphRunEncoderStored.exerciseAll, "",
				"", false, false);

		List<PointF> pointA_l = new List<PointF>();
		List<PointF> pointB_l = new List<PointF>();
		List<string> names_l = new List<string>();
		List<int> id_l = new List<int>(); //the uniqueIDs for knowing them on bar selection

		int countToDraw = eventGraphRunEncoderStored.rowsAtSQL.Count;
		foreach (RunEncoder re in eventGraphRunEncoderStored.rowsAtSQL)
		{
			//LogB.Information("forceSensor: " + re.ToString());
			// 1) Add data
			pointA_l.Add (new PointF (countToDraw, re.MaxSpeed));
			pointB_l.Add (new PointF (countToDraw, re.MaxAvgSpeed1s));
			countToDraw --;

			// 2) Add bottom names
			string typeRowString = "";
			if (eventGraphRunEncoderStored.exerciseAll) //if "all tests" show type
				typeRowString = re.ExerciseName; //TODO: check this param is filled

			names_l.Add (createTextBelowBar(
						"",
						typeRowString,
						re.Description,
						false, false,
						longestWord.Length, maxRowsForText));

			id_l.Add (re.UniqueID);
			id_l.Add (re.UniqueID);

			if (eventGraphRunEncoderStored.selectedID == re.UniqueID)
				cb.SelectedPos = eventGraphRunEncoderStored.rowsAtSQL.Count -countToDraw -1;
		}

		cb.Id_l = id_l;

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
				new List<int> (), new List<int> ());

		passDataForScreenshotIfNeeded ();

		cb.GraphDo();
	}
}
