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


public class CairoPaintBarsPreJumpReactive : CairoPaintBarsPre
{
	public CairoPaintBarsPreJumpReactive (DrawingArea darea, string fontStr,
			Constants.Modes mode, string personName, string testName, int pDN,
			int currentPersonID, bool drawBars)
	{
		initialize (darea, fontStr, mode, personName, testName, pDN);
		this.title = generateTitle();
		this.currentPersonID = currentPersonID;
		this.drawBars = drawBars;
	}

	public override void StoreEventGraphJumpsRj (PrepareEventGraphJumpReactive eventGraph)
	{
		this.eventGraphJumpsRjStored = eventGraph;
	}

	protected override bool storeCreated ()
	{
		return (eventGraphJumpsRjStored != null);
	}

	protected override bool haveDataToPlot()
	{
		return (eventGraphJumpsRjStored.rowsAtSQL.Count > 0);
	}

	protected override void paintSpecific()
	{
		// on rj use heights we only show jump height (and not fall data) because both values are shown (usually at same place)
		if (UseHeights)
			cb = new CairoBars1Series (darea, CairoBars.Type.NORMAL, CairoGeneric.MouseClickable.CLICKLR, true, true);
		else
			cb = new CairoBarsNHSeries (darea, CairoBars.Type.NORMAL, true, CairoGeneric.MouseClickable.CLICKLR, true, true);

		if(UseHeights) {
			cb.YVariable = Catalog.GetString("Jump height");
			cb.YUnits = "cm";
		} else {
			cb.YVariable = Catalog.GetString("Time");
			cb.YUnits = "s";
			cb.VariableSerieA = Catalog.GetString("Contact time") + " (" + Catalog.GetString("AVG") + ") ";
			cb.VariableSerieB = Catalog.GetString("Flight time") + " (" + Catalog.GetString("AVG") + ") ";
		}
		cb.XVariable = Catalog.GetString (eventGraphJumpsRjStored.OrderX.ToString ());

		//cb.GraphInit(fontStr, ! ShowPersonNames, true); //usePersonGuides, useGroupGuides
		cb.GraphInit(fontStr, true, true); //usePersonGuides, useGroupGuides

		List<Event> events = JumpRj.JumpListToEventList(eventGraphJumpsRjStored.rowsAtSQL);

		//find if there is a simulated
		bool thereIsASimulated = false;
		for(int i=0 ; i < eventGraphJumpsRjStored.rowsAtSQL.Count; i++)
		{
			if(eventGraphJumpsRjStored.rowsAtSQL[i].Simulated == -1)
				thereIsASimulated = true;

			if(! ShowPersonNames)
				eventGraphJumpsRjStored.rowsAtSQL[i].Description = ""; //to avoid showing description
		}

		calculateBottomParams (events, true, " - 99", //thinking on 99 jumps
				"(" + Catalog.GetString("Simulated") + ")", thereIsASimulated, false);


		//List<PointF> pointA0_l = new List<PointF>();
		List<PointF> pointA1_l = new List<PointF>();

		List<PointF> pointB_l = new List<PointF>();
		List<string> names_l = new List<string>();
		List<bool> personIcon_l = new List<bool>();
		List<int> id_l = new List<int>(); //the uniqueIDs for knowing them on bar selection

		int countToDraw = eventGraphJumpsRjStored.rowsAtSQL.Count;
		foreach(JumpRj jump in eventGraphJumpsRjStored.rowsAtSQL)
		{
			//LogB.Information("jump: " + jump.ToString());
			// 1) Add data
			//sum of the subjumps
			//double valueA = jump.TcSumCaringForStartIn;
			//double valueB = jump.TvSum;

			//avg of the subjumps
			double valueA = jump.TcAvg; //this cares for the -1 on start in. Does not count it.
			double valueB = jump.TvAvg;
			if(UseHeights) {
				valueA = UtilList.GetAverage (jump.FallList);
				valueB = UtilList.GetAverage (jump.HeightList);
			}

			//pointA0_l.Add(new PointF(countToDraw, jump.Jumps));
			pointA1_l.Add(new PointF(countToDraw, valueA));

			pointB_l.Add(new PointF(countToDraw, valueB));
			countToDraw --;

			// 2) Add bottom names
			/*
			string typeRowString = "";
			if (eventGraphJumpsRjStored.type == "") //if "all runs" show run.Type
				typeRowString = jump.Type;
				*/
			//TYPE B: on jumpRj show always jump type to show at the side the number of jumps. If change here, change it above (TYPEA)
			string typeRowString = string.Format("{0} - {1}", jump.Type, jump.Jumps);

			names_l.Add(createTextBelowBar(
						"",
						typeRowString,
						jump.Description,
						thereIsASimulated, (jump.Simulated == -1),
						longestWord.Length, maxRowsForText));

			personIcon_l.Add (personName == "" && currentPersonID >= 0 && jump.PersonID == currentPersonID);

			//add uniqueID two times, one for the each serie
			id_l.Add(jump.UniqueID); //UseHeights only shows height
			if (! UseHeights)
				id_l.Add(jump.UniqueID); //times show tc i tv

			if (eventGraphJumpsRjStored.selectedID == jump.UniqueID)
				cb.SelectedPos = eventGraphJumpsRjStored.rowsAtSQL.Count -countToDraw -1;
		}

		cb.Id_l = id_l;
		cb.PersonIcon_l = personIcon_l;

		if (eventGraphJumpsRjStored.HistoricalExStr != "")
		{
			cb.BestPersonExHistoricalD = eventGraphJumpsRjStored.HistoricalExD;
			cb.BestPersonExHistoricalStr = eventGraphJumpsRjStored.HistoricalExStr;
		}

		cb.PassBoxplots (eventGraphJumpsRjStored.BoxplotPerson, eventGraphJumpsRjStored.BoxplotSession);
		// pass selectedEvent to plot if it's not part of the shown events
		if (eventGraphJumpsRjStored.selectedEvent != null)
		{
			if (UseHeights)
				cb.SelectedDouble = UtilList.GetAverage (((JumpRj) eventGraphJumpsRjStored.selectedEvent).HeightList);
			else
				cb.SelectedDouble = ((JumpRj) eventGraphJumpsRjStored.selectedEvent).TvAvg;
		}

		if (UseHeights)
		{
			cb.PassData1Serie (pointB_l,
					new List<Cairo.Color>(), names_l,
					new List<List<double>> (),
					-1, fontHeightForBottomNames, bottomMargin, title,
					new List<int> (), new List<int> (), barsOrPoints);
		} else {
			List<List<PointF>> barsSecondary_ll = new List<List<PointF>>();
			barsSecondary_ll.Add(pointA1_l);

			cb.PassData2Series (pointB_l, barsSecondary_ll, false,
					new List<Cairo.Color>(), new List<Cairo.Color>(), names_l,
					"", false,
					-1, fontHeightForBottomNames, bottomMargin, title,
					new List<int> (), new List<int> (), barsOrPoints);
		}

		passDataForScreenshotIfNeeded ();
		cb.GraphDo();
	}

	// to show historic data even if in this session user has not data on that ex.
	protected override double getHistoricD ()
	{
		return eventGraphJumpsRjStored.HistoricalExD;
	}
	protected override string getHistoricStr ()
	{
		if (eventGraphJumpsRjStored.HistoricalExStr == "")
			return "";
		else
			return eventGraphJumpsRjStored.HistoricalExStr;
	}
}
