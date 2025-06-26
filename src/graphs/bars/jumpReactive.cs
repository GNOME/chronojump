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


public class CairoPaintBarsPreJumpReactive : CairoPaintBarsPre
{
	public CairoPaintBarsPreJumpReactive (DrawingArea darea, string fontStr, Constants.Modes mode, string personName, string testName, int pDN)
	{
		initialize (darea, fontStr, mode, personName, testName, pDN);
		this.title = generateTitle();
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
		return (eventGraphJumpsRjStored.jumpsAtSQL.Count > 0);
	}

	protected override void paintSpecific()
	{
		cb = new CairoBarsNHSeries (darea, CairoBars.Type.NORMAL, true, true, true, true);

		if(UseHeights) {
			cb.YVariable = Catalog.GetString("Height");
			cb.YUnits = "cm";
			cb.VariableSerieA = Catalog.GetString("Falling height") + " (" + Catalog.GetString("AVG") + ") ";
			cb.VariableSerieB = Catalog.GetString("Jump height") + " (" + Catalog.GetString("AVG") + ") ";
		} else {
			cb.YVariable = Catalog.GetString("Time");
			cb.YUnits = "s";
			cb.VariableSerieA = Catalog.GetString("Contact time") + " (" + Catalog.GetString("AVG") + ") ";
			cb.VariableSerieB = Catalog.GetString("Flight time") + " (" + Catalog.GetString("AVG") + ") ";
		}

		//cb.GraphInit(fontStr, ! ShowPersonNames, true); //usePersonGuides, useGroupGuides
		cb.GraphInit(fontStr, true, true); //usePersonGuides, useGroupGuides

		List<Event> events = JumpRj.JumpListToEventList(eventGraphJumpsRjStored.jumpsAtSQL);

		//find if there is a simulated
		bool thereIsASimulated = false;
		for(int i=0 ; i < eventGraphJumpsRjStored.jumpsAtSQL.Count; i++)
		{
			if(eventGraphJumpsRjStored.jumpsAtSQL[i].Simulated == -1)
				thereIsASimulated = true;

			if(! ShowPersonNames)
				eventGraphJumpsRjStored.jumpsAtSQL[i].Description = ""; //to avoid showing description
		}

		calculateBottomParams (events, true, " - 99", //thinking on 99 jumps
				"(" + Catalog.GetString("Simulated") + ")", thereIsASimulated, false);


		//List<PointF> pointA0_l = new List<PointF>();
		List<PointF> pointA1_l = new List<PointF>();

		List<PointF> pointB_l = new List<PointF>();
		List<string> names_l = new List<string>();
		List<int> id_l = new List<int>(); //the uniqueIDs for knowing them on bar selection

		int countToDraw = eventGraphJumpsRjStored.jumpsAtSQL.Count;
		foreach(JumpRj jump in eventGraphJumpsRjStored.jumpsAtSQL)
		{
			LogB.Information("jump: " + jump.ToString());
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

			//add uniqueID two times, one for the each serie
			id_l.Add(jump.UniqueID);
			id_l.Add(jump.UniqueID);

			if (eventGraphJumpsRjStored.selectedID == jump.UniqueID)
				cb.SelectedPos = eventGraphJumpsRjStored.jumpsAtSQL.Count -countToDraw -1;
		}

		cb.Id_l = id_l;

		cb.PassGuidesData (new CairoBarsGuideManage(
					//! ShowPersonNames, true, //usePersonGuides, useGroupGuides
					true, true, //usePersonGuides, useGroupGuides
					eventGraphJumpsRjStored.sessionMAXAtSQL,
					eventGraphJumpsRjStored.sessionAVGAtSQL,
					eventGraphJumpsRjStored.sessionMINAtSQL,
					0,
					eventGraphJumpsRjStored.personMAXAtSQL,
					eventGraphJumpsRjStored.personAVGAtSQL,
					eventGraphJumpsRjStored.personMINAtSQL
					));

		List<List<PointF>> barsSecondary_ll = new List<List<PointF>>();
		barsSecondary_ll.Add(pointA1_l);

		cb.PassData2Series (pointB_l, barsSecondary_ll, false,
				new List<Cairo.Color>(), new List<Cairo.Color>(), names_l,
				"", false,
				-1, fontHeightForBottomNames, bottomMargin, title,
				new List<int> (), new List<int> ());

		passDataForScreenshotIfNeeded ();

		cb.GraphDo();
	}
}
