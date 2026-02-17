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


//copied from CairoPaintBarsWilight. TODO: unify them
public class CairoPaintBarsFourPlatforms : CairoPaintBarsPre
{
	public CairoPaintBarsFourPlatforms (DrawingArea darea, string fontStr,
			Constants.Modes mode, string personName, string testName, int pDN,
			int currentPersonID, bool drawBars)
	{
		initialize (darea, fontStr, mode, personName, testName, pDN);
		this.title = generateTitle();
		this.currentPersonID = currentPersonID;
		this.drawBars = drawBars;
	}

	public override void StoreEventGraphFourPlatforms (PrepareEventGraphFourPlatforms eventGraph)
	{
		this.eventGraphFourPlatformsStored = eventGraph;
	}

	protected override bool storeCreated ()
	{
		return (eventGraphFourPlatformsStored != null);
	}

	protected override bool haveDataToPlot()
	{
		return (eventGraphFourPlatformsStored.rowsAtSQL.Count > 0);
	}

	protected override void paintSpecific()
	{
		cb = new CairoBars1Series (darea, CairoBars.Type.NORMAL, CairoGeneric.MouseClickable.CLICKLR, true, CairoBars.PaintGridEnum.ALL);

		cb.YVariable = Catalog.GetString("Time");
		cb.YUnits = "s";

		//cb.GraphInit(fontStr, ! ShowPersonNames, true); //usePersonGuides, useGroupGuides
		cb.GraphInit(fontStr, true, true); //usePersonGuides, useGroupGuides

		List<Event> events = FourPlatforms.FourPlatformsListToEventList (eventGraphFourPlatformsStored.rowsAtSQL);

		List<PointF> point_l = new List<PointF>();
		List<string> names_l = new List<string>();
		List<bool> personIcon_l = new List<bool>();
		List<int> id_l = new List<int>(); //the uniqueIDs for knowing them on bar selection

		calculateBottomParams (events, true, "", "", false, false);

		int countToDraw = eventGraphFourPlatformsStored.rowsAtSQL.Count;
		foreach (FourPlatforms fp in eventGraphFourPlatformsStored.rowsAtSQL)
		{
			// 1) Add data
			//point_l.Add(new PointF(countToDraw --, UtilAll.DivideSafe (fp.TotalMs, 1000)));
			point_l.Add (new PointF(countToDraw --, fp.TotalTime));

			// 2) Add bottom names
			string typeRowString = "";
			//if (eventGraphFourPlatformsStored.type == "")
			//	typeRowString = jump.Type;

			names_l.Add (createTextBelowBar(
						"",
						typeRowString,
						fp.Description, //person name
						false, false,
						longestWord.Length, maxRowsForText));

			personIcon_l.Add (personName == "" && currentPersonID >= 0 && fp.PersonID == currentPersonID);

			id_l.Add (fp.UniqueID);

			if (eventGraphFourPlatformsStored.selectedID == fp.UniqueID)
				cb.SelectedPos = eventGraphFourPlatformsStored.rowsAtSQL.Count -countToDraw -1;
		}

		cb.Id_l = id_l;
		cb.PersonIcon_l = personIcon_l;

		cb.PassData1Serie (point_l,
				new List<Cairo.Color>(), names_l,
				new List<List<double>> (),
				-1, fontHeightForBottomNames, bottomMargin, title,
				new List<int> (), new List<int> (), barsOrPoints);

		passDataForScreenshotIfNeeded ();

		cb.GraphDo();
	}
}
