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


public class TreeViewBeepTest : TreeViewEvent
{
	protected string personName = Catalog.GetString("Person");

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
		dataLineTypePosition = 4; //position of type in the data to be printed
		allEventsName = ""; //Constants.AllJumpsNameStr();
		eventIDColumn = 4; //column where the uniqueID of event will be (and will be hidden). //unused now
		columnsString = new string[] { personName, "Stages", "Laps", datetimeName };

		store = getStore(columnsString.Length +1); //+1 because, eventID is not show in last col
		treeview.Model = store;
		prepareHeaders(columnsString);
		
		//on creation, treeview is minimized
		expandState = ExpandStates.MINIMIZED;
	}

	protected override System.Object getObjectFromString(string [] myStringOfData)
	{
		LogB.Information ("myStringOfData:");
		LogB.Information (Util.StringArrayToString (myStringOfData, "____"));

		BeepTest beepTest = new BeepTest (
				Convert.ToInt32(myStringOfData[1].ToString()),  //uniqueID
				Convert.ToInt32(myStringOfData[2].ToString()),  //personID
				Convert.ToInt32(myStringOfData[3].ToString()),  //sessionID
				Convert.ToInt32(myStringOfData[4].ToString()),  //exerciseID
				myStringOfData[5].ToString(),			//options
				Convert.ToInt32(myStringOfData[6].ToString()),  //stages
				Convert.ToInt32(myStringOfData[7].ToString()),  //laps
				Convert.ToInt32(myStringOfData[8].ToString()),  //totalMeters
				Convert.ToDouble (Util.CDS (myStringOfData[9].ToString())),  //maxSpeed
				myStringOfData[10].ToString(),			//datetime
				myStringOfData[11].ToString(),			//videoURL
				myStringOfData[12].ToString()			//description
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
		
		myData[count++] = "Default";
		myData[count++] = beepTest.Stages.ToString ();
		myData[count++] = beepTest.Laps.ToString ();
		myData[count++] = beepTest.UniqueID.ToString ();

		return myData;
	}
}
