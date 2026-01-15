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
	private bool bestSecond;

	public CairoPaintBarsPreRunEncoder (DrawingArea darea, string fontStr,
			Constants.Modes mode, string personName, string testName, int pDN,
			int currentPersonID, bool bestSecond, bool drawBars)
	{
		LogB.Information ("CairoPaintBarsPreRunEncoder constructor");
		initialize (darea, fontStr, mode, personName, testName, pDN);
		this.title = generateTitle();
		this.currentPersonID = currentPersonID;
		this.bestSecond = bestSecond;
		this.drawBars = drawBars;
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
		cb = new CairoBars1Series (darea, CairoBars.Type.NORMAL, CairoGeneric.MouseClickable.CLICKLR, true, true);

		cb.YVariable = Catalog.GetString("Max speed");
		if (bestSecond)
			cb.YVariable = Catalog.GetString("Best second");

		cb.YUnits = "m/s";
		cb.XVariable = Catalog.GetString (eventGraphRunEncoderStored.OrderX.ToString ());

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

		List<PointF> point_l = new List<PointF>();
		List<string> names_l = new List<string>();
		List<bool> personIcon_l = new List<bool>();
		List<int> id_l = new List<int>(); //the uniqueIDs for knowing them on bar selection

		int countToDraw = eventGraphRunEncoderStored.rowsAtSQL.Count;
		foreach (RunEncoder re in eventGraphRunEncoderStored.rowsAtSQL)
		{
			//LogB.Information("forceSensor: " + re.ToString());
			// 1) Add data
			if (bestSecond)
				point_l.Add (new PointF (countToDraw, re.MaxAvgSpeed1s));
			else
				point_l.Add (new PointF (countToDraw, re.MaxSpeed));
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

			personIcon_l.Add (personName == "" && currentPersonID >= 0 && re.PersonID == currentPersonID);

			id_l.Add (re.UniqueID);

			if (eventGraphRunEncoderStored.selectedID == re.UniqueID)
				cb.SelectedPos = eventGraphRunEncoderStored.rowsAtSQL.Count -countToDraw -1;
		}

		cb.Id_l = id_l;
		cb.PersonIcon_l = personIcon_l;

		if (eventGraphRunEncoderStored.HistoricalExStr != "")
		{
			cb.BestPersonExHistoricalD = eventGraphRunEncoderStored.HistoricalExD;
			cb.BestPersonExHistoricalStr = eventGraphRunEncoderStored.HistoricalExStr;
		}

		cb.PassBoxplots (eventGraphRunEncoderStored.BoxplotPerson, eventGraphRunEncoderStored.BoxplotSession);
		// pass selectedEvent to plot if it's not part of the shown events
		if (eventGraphRunEncoderStored.selectedEvent != null)
		{
			if (bestSecond)
				cb.SelectedDouble = ((RunEncoder) eventGraphRunEncoderStored.selectedEvent).MaxAvgSpeed1s;
			else
				cb.SelectedDouble = ((RunEncoder) eventGraphRunEncoderStored.selectedEvent).MaxSpeed;
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
		return eventGraphRunEncoderStored.HistoricalExD;
	}
	protected override string getHistoricStr ()
	{
		if (eventGraphRunEncoderStored.HistoricalExStr == "")
			return "";
		else
			return eventGraphRunEncoderStored.HistoricalExStr;
	}
}
