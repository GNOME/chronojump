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
 *  Copyright (C) 2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Data;
using Gtk;
using System.Collections; //ArrayList
using Mono.Unix;


public class TreeViewRunEncoder : TreeViewEvent
{
	private bool barsAreSpeeds;

	public TreeViewRunEncoder ()
	{
	}
	
	public TreeViewRunEncoder (Gtk.TreeView treeview, int pDN, ExpandStates expandState, bool barsAreSpeeds)
	{
		LogB.Information ("At TreeViewRunEncoder constructor");
		this.treeview = treeview;
		this.pDN = pDN;
		this.expandState = expandState;
		this.barsAreSpeeds = barsAreSpeeds;
		
		treeviewHasTwoLevels = false;
		dataLineNamePosition = 0; //position of name in the data to be printed
		dataLineTypePosition = 17; //position of type in the data to be printed.
		allEventsName = Constants.AllTestsNameStr();
		boldableColumns_l = new List<int> { 1, 2 }; //maxspeed, bestSecond
		idColumn = 6; //column where the uniqueID of event will be (and will be hidden).
		columnsString = new string[] { 
			personName,
			Catalog.GetString ("Max speed"),
			Catalog.GetString ("Best second"),
			datetimeName,
			videoName,
			descriptionName};
			//,"ID delete"	}; //just for debug

		LogB.Information ("At TreeViewRunEncoder:  dataLineTypePosition = " + dataLineTypePosition.ToString  ());
		store = getStore(columnsString.Length +1); //+1 because, eventID is not show in last col
		treeview.Model = store;
		prepareHeaders(columnsString);
		
		//on creation, treeview is minimized
		expandState = ExpandStates.MINIMIZED;
	}

	protected override bool shouldRenderBoldable (string columnTitle)
	{
		if (barsAreSpeeds && columnTitle.StartsWith (Catalog.GetString ("Max speed")))
			return true;

		if (! barsAreSpeeds && columnTitle.StartsWith (Catalog.GetString ("Best second")))
			return true;

		return false;
	}

	protected override System.Object getObjectFromString (string [] str)
	{
		LogB.Information ("getObjectFromString str:");
		LogB.Information (Util.StringArrayToString (str, "____"));

		return new RunEncoder (
				Convert.ToInt32 (str[1]), 	//uniqueID
				Convert.ToDouble (str[15]), 	//maxSpeed
			 	Convert.ToDouble (str[16]),	//maxAvgSpeed1s (Best second)
				str[10], //dateTime
				str[12], //videoURL
				str[11], //description
				str[17] //exerciseName
				);
	}
	
	protected override string [] getLineToStore (System.Object myObject)
	{
		LogB.Information ("at getLineToStore");
		RunEncoder re = (RunEncoder) myObject;
		LogB.Information ("re uniqueID: " + re.UniqueID.ToString  ());

		string [] myData = new String [getColsNum()];
		int count = 0;

		myData[count++] = re.ExerciseName;
		myData[count++] = boldMark + Util.TrimDecimals (re.MaxSpeed, 3);
		myData[count++] = boldMark + Util.TrimDecimals (re.MaxAvgSpeed1s, 3);
		myData[count++] = re.DateTimePublic;

		if (UtilList.StartsWithInListString (videos_l, string.Format ("{0}-{1}", Constants.TestTypes.RACEANALYZER, re.UniqueID)))
			myData[count++] = Catalog.GetString ("Yes");
		else
			myData[count++] = Catalog.GetString ("No");

		myData[count++] = re.Description;
		myData[count++] = re.UniqueID.ToString ();

		return myData;
	}

	public bool BarsAreSpeeds {
		set { barsAreSpeeds = value; }
	}

}
