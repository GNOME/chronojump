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
 *  Copyright (C) 2026   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Data;
using Gtk;
using System.Collections; //ArrayList
using Mono.Unix;


public class TreeViewBeepTest : TreeViewEvent
{
	public TreeViewBeepTest ()
	{
	}
	
	public TreeViewBeepTest (Gtk.TreeView treeview, int pDN, ExpandStates expandState)
	{
		this.treeview = treeview;
		this.pDN = pDN;
		this.expandState = expandState;
		
		treeviewHasTwoLevels = false;
		dataLineNamePosition = 0; //position of name in the data to be printed
		dataLineTypePosition = 5; //position of type in the data to be printed. Here is used as str
		allEventsName = Constants.AllTestsNameStr();
		idColumn = 6; //column where the uniqueID of event will be (and will be hidden).
		columnsString = new string[] { personName, "Final stage", "Final lap", "Speed max", "Vo2 max", datetimeName};
			//,"ID delete"	};

		store = getStore(columnsString.Length +1); //+1 because, eventID is not show in last col
		treeview.Model = store;
		prepareHeaders(columnsString);
		
		//on creation, treeview is minimized
		expandState = ExpandStates.MINIMIZED;
	}

	protected override System.Object getObjectFromString (string [] str)
	{
		LogB.Information ("getObjectFromString str:");
		LogB.Information (Util.StringArrayToString (str, "____"));

		BeepTest beepTest = new BeepTest (
				Convert.ToInt32 (str[1].ToString ()),  //uniqueID
				Convert.ToInt32 (str[2].ToString ()),  //personID
				Convert.ToInt32 (str[3].ToString ()),  //sessionID
				Convert.ToInt32 (str[4].ToString ()),  //exerciseID
				//str[5] exerciseStr is not used here
				str[6].ToString (),			//options
				Convert.ToInt32 (str[7].ToString ()),  //stages
				Convert.ToInt32 (str[8].ToString ()),  //laps
				Convert.ToInt32 (str[9].ToString ()),  //totalMeters
				Convert.ToDouble (Util.CDS (str[10].ToString ())),  //maxSpeed
				str[11].ToString (),			//datetime
				str[12].ToString (),			//videoURL
				str[13].ToString ()			//description
				);

		return beepTest;
	}

	protected override string [] getLineToStore (System.Object myObject)
	{
		BeepTest beepTest = (BeepTest) myObject;

		/*
		string title = Catalog.GetString(newJump.Type);
		if(newJump.Simulated == Constants.Simulated)
			title += Constants.SimulatedTreeviewStr();
			*/

		string [] myData = new String [getColsNum()];
		int count = 0;
		//myData[count++] = newJump.Type;
		//myData[count++] = title;

		myData[count++] = beepTest.ExerciseName;
		myData[count++] = beepTest.GetAchievedStageName;
		myData[count++] = (beepTest.Laps +1).ToString ();
		myData[count++] = Util.TrimDecimals (beepTest.MaxSpeed, 3);

		string vo2MaxStr = "";
		if (beepTest.GetVo2Max > 0)
			vo2MaxStr = Util.TrimDecimals (beepTest.GetVo2Max, 4);
		myData[count++] = vo2MaxStr;

		myData[count++] = UtilDate.GetDatetimePrint (UtilDate.FromFile (beepTest.DateTimePublic));
		myData[count++] = beepTest.UniqueID.ToString ();

		return myData;
	}
}
