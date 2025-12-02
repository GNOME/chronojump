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
 *  Copyright (C) 2004-2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Data;
using Gtk;
using System.Collections; //ArrayList
using Mono.Unix;


public class TreeViewRuns : TreeViewEvent
{
	protected bool metersSecondsPreferred;
	protected bool barsAreSpeeds;

	public TreeViewRuns ()
	{
	}
	
	public TreeViewRuns (Gtk.TreeView treeview, int newPrefsDigitsNumber,
			bool metersSecondsPreferred, ExpandStates expandState, bool barsAreSpeeds)
	{
		this.treeview = treeview;
		this.pDN = newPrefsDigitsNumber;
		this.metersSecondsPreferred = metersSecondsPreferred;
		this.expandState = expandState;
		this.barsAreSpeeds = barsAreSpeeds;

		treeviewHasTwoLevels = false;
		dataLineNamePosition = 0; //position of name in the data to be printed
		dataLineTypePosition = 4; //position of type in the data to be printed
		allEventsName = Constants.AllRunsNameStr();
		boldableColumns_l = new List<int> { 1, 3 }; //speed, time
		idColumn = 7; //column where the uniqueID of event will be (and will be hidden)
	
		string runnerName = Catalog.GetString("Runner");
		string speedName = Catalog.GetString("Speed");
		if(metersSecondsPreferred)
			speedName += "\n(m/s)";
		else
			speedName += "\n(km/h)";

		string distanceName = Catalog.GetString("Distance") + "\n(m)";
		string timeName = Catalog.GetString("Time") + "\n(s)";

		columnsString = new string[]{ runnerName, speedName, distanceName, timeName, datetimeName, videoName, descriptionName};
			//,"ID delete"	; // just for debug

		store = getStore(columnsString.Length +1); //+1 because, eventID is not show in last col
		treeview.Model = store;
		prepareHeaders(columnsString);
	}

	protected override bool shouldRenderBoldable (string columnTitle)
	{
		if (barsAreSpeeds && columnTitle.StartsWith (Catalog.GetString ("Speed")))
			return true;

		if (! barsAreSpeeds && columnTitle.StartsWith (Catalog.GetString ("Time")))
			return true;

		return false;
	}

	protected override System.Object getObjectFromString(string [] myStringOfData)
	{
		Run myRun = new Run();
		myRun.UniqueID = Convert.ToInt32(myStringOfData[1].ToString()); 
		myRun.Type = myStringOfData[4].ToString();
		myRun.Distance = Convert.ToDouble(myStringOfData[5].ToString());
		myRun.Time = Convert.ToDouble(myStringOfData[6].ToString());
		myRun.Description = myStringOfData[7].ToString();
		myRun.Simulated = Convert.ToInt32(myStringOfData[8].ToString());
		myRun.Datetime = myStringOfData[10].ToString();
		//speed is not needed to define

		return myRun;
	}

	protected override string [] getLineToStore(System.Object myObject)
	{
		Run newRun = (Run)myObject;
		//LogB.Information("getLineToStore, object: " + newRun.ToString());

		string title = Catalog.GetString(newRun.Type);
		if(newRun.Simulated == Constants.Simulated)
			title += Constants.SimulatedTreeviewStr();

		string [] myData = new String [getColsNum()];
		int count = 0;
		myData[count++] = title;
		//myData[count++] = Util.TrimDecimals(newRun.Speed.ToString(), pDN); this doesn't know the metersSecondsPreferred
		if(newRun.Type == "Margaria") 
			myData[count++] = ""; //don't show speed, because has no sense on Margaria
		else {
			myData[count++] = boldMark + Util.TrimDecimals(Util.GetSpeed(
						newRun.Distance.ToString(),
						newRun.Time.ToString(),
						metersSecondsPreferred ), pDN);
		}
		
		string distanceUnits = "";
		if(newRun.Type == "Margaria") 
			distanceUnits = " mm";
		myData[count++]	= Util.TrimDecimals(newRun.Distance.ToString(), pDN) + distanceUnits;

		myData[count++] = boldMark + Util.TrimDecimals(newRun.Time.ToString(), pDN);
		myData[count++] = UtilDate.GetDatetimePrint(UtilDate.FromFile(newRun.Datetime));

		if (UtilList.StartsWithInListString (videos_l, string.Format ("{0}-{1}", Constants.TestTypes.RUN, newRun.UniqueID)))
			myData[count++] = Catalog.GetString ("Yes");
		else
			myData[count++] = Catalog.GetString ("No");

		myData[count++] = newRun.Description;
		myData[count++] = newRun.UniqueID.ToString();
		return myData;
	}

	public bool BarsAreSpeeds {
		set { barsAreSpeeds = value; }
	}

}
