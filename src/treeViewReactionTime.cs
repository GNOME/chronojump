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
 *  Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using Gtk;
using System.Collections; //ArrayList
using Mono.Unix;


public class TreeViewReactionTimes : TreeViewEvent
{
	protected string personName = Catalog.GetString("Person");

	public TreeViewReactionTimes ()
	{
	}
	
	public TreeViewReactionTimes (Gtk.TreeView treeview, int newPrefsDigitsNumber, ExpandStates expandState)
	{
		this.treeview = treeview;
		pDN = newPrefsDigitsNumber;
		this.expandState = expandState;

		treeviewHasTwoLevels = false;
		dataLineNamePosition = 0; //position of name in the data to be printed


		dataLineTypePosition = 4; //position of type in the data to be printed
		allEventsName = "";
		

		columnsString = new string[]{personName, Catalog.GetString("Time") + " (s)", descriptionName};

		eventIDColumn = columnsString.Length ; //column where the uniqueID of event will be (and will be hidden). 
		store = getStore(columnsString.Length +1); //+1 because, eventID is not show in last col
		treeview.Model = store;
		prepareHeaders(columnsString);
	}

	
	protected override System.Object getObjectFromString(string [] myStringOfData) {
		ReactionTime myReactionTime = new ReactionTime();
		myReactionTime.UniqueID = Convert.ToInt32(myStringOfData[1].ToString()); 
		myReactionTime.Type = myStringOfData[4].ToString();
		myReactionTime.Time = Convert.ToDouble(myStringOfData[5].ToString());
		myReactionTime.Description = myStringOfData[6].ToString();
		myReactionTime.Simulated = Convert.ToInt32(myStringOfData[7].ToString());

		return myReactionTime;
	}

	protected override string [] getLineToStore(System.Object myObject)
	{
		ReactionTime newReactionTime = (ReactionTime)myObject;

		string title = "";
		if(newReactionTime.Simulated == Constants.Simulated)
			title += Constants.SimulatedTreeviewStr() + " ";

		string [] myData = new String [getColsNum()];
		int count = 0;
		//myData[count++] = newReactionTime.Type;
		myData[count++] = title;
		myData[count++] = Util.TrimDecimals(newReactionTime.Time.ToString(), pDN);
		
		myData[count++] = newReactionTime.Description;
		myData[count++] = newReactionTime.UniqueID.ToString();
		return myData;
	}
	
}
