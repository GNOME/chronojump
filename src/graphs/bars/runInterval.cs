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


public class CairoPaintBarsPreRunInterval : CairoPaintBarsPre
{
	private bool metersSecondsPreferred;

	public CairoPaintBarsPreRunInterval (DrawingArea darea, string fontStr, Constants.Modes mode, string personName, string testName, int pDN, bool metersSecondsPreferred, int currentPersonID)
	{
		initialize (darea, fontStr, mode, personName, testName, pDN);
		this.title = generateTitle();
		this.metersSecondsPreferred = metersSecondsPreferred;
		this.currentPersonID = currentPersonID;
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
		return (eventGraphRunsIntervalStored.runsAtSQL.Count > 0);
	}

	protected override void paintSpecific()
	{
		cb = new CairoBars1Series (darea, CairoBars.Type.NORMAL, true, true, true);

		cb.YVariable = Catalog.GetString("Speed");
		if (metersSecondsPreferred)
			cb.YUnits = "m/s";
		else
			cb.YUnits = "Km/h";

		//cb.GraphInit(fontStr, ! ShowPersonNames, true); //usePersonGuides, useGroupGuides
		cb.GraphInit(fontStr, true, true); //usePersonGuides, useGroupGuides

		List<Event> events = RunInterval.RunIntervalListToEventList (eventGraphRunsIntervalStored.runsAtSQL);

		//find if there is a simulated
		bool thereIsASimulated = false;
		for(int i=0 ; i < eventGraphRunsIntervalStored.runsAtSQL.Count; i++)
		{
			if(eventGraphRunsIntervalStored.runsAtSQL[i].Simulated == -1)
				thereIsASimulated = true;

			if(! ShowPersonNames)
				eventGraphRunsIntervalStored.runsAtSQL[i].Description = ""; //to avoid showing description
		}

		calculateBottomParams (events, true, " - 99", //thinking on 99 tracks
				"(" + Catalog.GetString("Simulated") + ")", thereIsASimulated, RunsShowTime);

		List<PointF> point_l = new List<PointF>();
		List<string> names_l = new List<string>();
		List<bool> personIcon_l = new List<bool>();
		List<int> id_l = new List<int>(); //the uniqueIDs for knowing them on bar selection

		int countToDraw = eventGraphRunsIntervalStored.runsAtSQL.Count;
		foreach(RunInterval runI in eventGraphRunsIntervalStored.runsAtSQL)
		{
			// 1) Add data
			runI.MetersSecondsPreferred = metersSecondsPreferred;
			point_l.Add(new PointF(countToDraw --, runI.Speed));

			// 2) Add bottom names
			/*
			string typeRowString = "";
			if (eventGraphRunsIntervalStored.type == "") //if "all runs" show run.Type
				typeRowString = runI.Type;
				*/
			//TYPE B: on runI show always run type to show at the side the number of tracks. If change here, change it above (TYPEA)
			string typeRowString = string.Format("{0} - {1}", runI.Type, runI.Tracks);

			string timeString = "";
			if(RunsShowTime)
				timeString = string.Format("{0} s", Util.TrimDecimals(runI.TimeTotal, pDN));

			names_l.Add(createTextBelowBar(
						timeString,
						typeRowString,
						runI.Description,
						thereIsASimulated, (runI.Simulated == -1),
						longestWord.Length, maxRowsForText));

			personIcon_l.Add (personName == "" && currentPersonID >= 0 && runI.PersonID == currentPersonID);

			id_l.Add(runI.UniqueID);

			if (eventGraphRunsIntervalStored.selectedID == runI.UniqueID)
				cb.SelectedPos = eventGraphRunsIntervalStored.runsAtSQL.Count -countToDraw -1;
		}

		cb.Id_l = id_l;
		cb.PersonIcon_l = personIcon_l;

		cb.PassGuidesData (new CairoBarsGuideManage(
					//! ShowPersonNames, true, //usePersonGuides, useGroupGuides
					true, true, //usePersonGuides, useGroupGuides
					eventGraphRunsIntervalStored.sessionMAXAtSQL,
					eventGraphRunsIntervalStored.sessionAVGAtSQL,
					eventGraphRunsIntervalStored.sessionMINAtSQL,
					0,
					eventGraphRunsIntervalStored.personMAXAtSQL,
					eventGraphRunsIntervalStored.personAVGAtSQL,
					eventGraphRunsIntervalStored.personMINAtSQL));

		cb.PassData1Serie (point_l,
				new List<Cairo.Color>(), names_l,
				-1, fontHeightForBottomNames, bottomMargin, title,
				new List<int> (), new List<int> ());

		passDataForScreenshotIfNeeded ();

		cb.GraphDo();
	}
}
