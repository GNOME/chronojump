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


public class CairoPaintBarsPreJumpSimple : CairoPaintBarsPre
{
	public CairoPaintBarsPreJumpSimple (DrawingArea darea, string fontStr,
			Constants.Modes mode, string personName, string testName, int pDN,
			int currentPersonID, bool drawBars)
	{
		initialize (darea, fontStr, mode, personName, testName, pDN);
		this.title = generateTitle();
		this.currentPersonID = currentPersonID;
		this.drawBars = drawBars;
	}

	public override void StoreEventGraphJumps (PrepareEventGraphJumpSimple eventGraph)
	{
		this.eventGraphJumpsStored = eventGraph;
	}

	protected override bool storeCreated ()
	{
		return (eventGraphJumpsStored != null);
	}

	protected override bool haveDataToPlot()
	{
		return (eventGraphJumpsStored.rowsAtSQL.Count > 0);
	}

	protected override void paintSpecific()
	{
		/*
		 * check if one bar has to be shown or two
		 * this is important when we are showing multitests
		 */
		bool showBarA = false; //tc or fall
		bool showBarB = false; //tv or height
		foreach(Jump jump in eventGraphJumpsStored.rowsAtSQL)
		{
			if(jump.Fall > 0 || jump.Tc > 0) //jump.Tc to include takeOff, takeOffWeiht
				showBarA = true;
			if(jump.Tv > 0)
				showBarB = true;

			//if both found do not need to search more
			if(showBarA && showBarB)
				break;
		}
		//takeOff, takeOff weights show times (Tc)
		if(showBarA && ! showBarB)
			UseHeights = false;

		if(showBarA && showBarB) //Dja, Djna
			cb = new CairoBarsNHSeries (darea, CairoBars.Type.NORMAL, true, CairoGeneric.MouseClickable.CLICKLR, true, true);
		else if (showBarA) //takeOff, takeOffWeight
			cb = new CairoBars1Series (darea, CairoBars.Type.NORMAL, CairoGeneric.MouseClickable.CLICKLR, true, true);
		else //rest of the jumps: sj, cmj, ..
			cb = new CairoBars1Series (darea, CairoBars.Type.NORMAL, CairoGeneric.MouseClickable.CLICKLR, true, true);

		if(UseHeights) {
			cb.YVariable = Catalog.GetString("Jump height");
			cb.YUnits = "cm";
			if(showBarA && showBarB) //Dja, Djna
			{
				cb.VariableSerieA = Catalog.GetString("Falling height");
				cb.VariableSerieB = Catalog.GetString("Jump height");
			}
		} else {
			cb.YVariable = Catalog.GetString("Time");
			cb.YUnits = "s";
			if(showBarA && showBarB) //Dja, Djna
			{
				cb.VariableSerieA = Catalog.GetString("Contact time");
				cb.VariableSerieB = Catalog.GetString("Flight time");
			}
		}
		cb.XVariable = Catalog.GetString (eventGraphJumpsStored.OrderX.ToString ());

		//cb.GraphInit(fontStr, ! ShowPersonNames, true); //usePersonGuides, useGroupGuides
		cb.GraphInit(fontStr, true, true); //usePersonGuides, useGroupGuides

		List<Event> events = Jump.JumpListToEventList(eventGraphJumpsStored.rowsAtSQL);

		//find if there is a simulated
		bool thereIsASimulated = false;
		for(int i=0 ; i < eventGraphJumpsStored.rowsAtSQL.Count; i++)
		{
			if(eventGraphJumpsStored.rowsAtSQL[i].Simulated == -1)
				thereIsASimulated = true;

			if(! ShowPersonNames)
				eventGraphJumpsStored.rowsAtSQL[i].Description = ""; //to avoid showing description
		}

		calculateBottomParams (events, eventGraphJumpsStored.Type == "", "",
				"(" + Catalog.GetString("Simulated") + ")", thereIsASimulated, false);

		List<PointF> pointA_l = new List<PointF>();
		List<PointF> pointB_l = new List<PointF>();
		List<string> names_l = new List<string>();
		List<bool> personIcon_l = new List<bool>();
		List<int> id_l = new List<int>(); //the uniqueIDs for knowing them on bar selection

		int countToDraw = eventGraphJumpsStored.rowsAtSQL.Count;
		foreach(Jump jump in eventGraphJumpsStored.rowsAtSQL)
		{
			//LogB.Information("jump: " + jump.ToString());
			// 1) Add data
			double valueA = jump.Fall;
			double valueB = Util.GetHeightInCentimeters(jump.Tv); //jump height
			if(! UseHeights) {
				valueA = jump.Tc;
				valueB = jump.Tv;
			}

			pointA_l.Add(new PointF(countToDraw, valueA));
			pointB_l.Add(new PointF(countToDraw, valueB));
			countToDraw --;

			// 2) Add bottom names
			//names_l.Add(Catalog.GetString(jump.Type));
			string typeRowString = "";
			if (eventGraphJumpsStored.Type == "") //if "all runs" show run.Type
				typeRowString = jump.Type;

			names_l.Add(createTextBelowBar(
						"",
						typeRowString,
						jump.Description,
						thereIsASimulated, (jump.Simulated == -1),
						longestWord.Length, maxRowsForText));

			personIcon_l.Add (personName == "" && currentPersonID >= 0 && jump.PersonID == currentPersonID);

			id_l.Add(jump.UniqueID);
			if(showBarA && showBarB) //there are jumps like Dja, Djna
				id_l.Add(jump.UniqueID);

			if (eventGraphJumpsStored.selectedID == jump.UniqueID)
				cb.SelectedPos = eventGraphJumpsStored.rowsAtSQL.Count -countToDraw -1;
		}

		cb.Id_l = id_l;
		cb.PersonIcon_l = personIcon_l;

		cb.PassBoxplots (eventGraphJumpsStored.BoxplotPerson, eventGraphJumpsStored.BoxplotSession);
		// pass selectedEvent to plot if it's not part of the shown events
		if (eventGraphJumpsStored.selectedEvent != null)
		{
			if (UseHeights)
				cb.SelectedDouble = Util.GetHeightInCentimeters (((Jump) eventGraphJumpsStored.selectedEvent).Tv); //jump height
			else
				cb.SelectedDouble = ((Jump) eventGraphJumpsStored.selectedEvent).Tv;
		}

		if(showBarA && showBarB) //Dja, Djna
		{
			List<List<PointF>> barsSecondary_ll = new List<List<PointF>>();
			barsSecondary_ll.Add(pointA_l);

			cb.PassData2Series (pointB_l, barsSecondary_ll, false,
					new List<Cairo.Color>(), new List<Cairo.Color>(), names_l,
					"", false,
					-1, fontHeightForBottomNames, bottomMargin, title,
					 new List<int> (), new List<int> (), barsOrPoints);
		} else if (showBarA) //takeOff, takeOffWeight
			cb.PassData1Serie (pointA_l,
					new List<Cairo.Color>(), names_l,
					new List<List<double>> (),
					-1, fontHeightForBottomNames, bottomMargin, title,
					new List<int> (), new List<int> (), barsOrPoints);
		else //rest of the jumps: sj, cmj, ..
			cb.PassData1Serie (pointB_l,
					new List<Cairo.Color>(), names_l,
					new List<List<double>> (),
					-1, fontHeightForBottomNames, bottomMargin, title,
					new List<int> (), new List<int> (), barsOrPoints);

		passDataForScreenshotIfNeeded ();

		cb.GraphDo();
	}
}

