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


public class TreeViewEncoder : TreeViewEvent
{
	public TreeViewEncoder (Gtk.TreeView treeview, int newPrefsDigitsNumber, ExpandStates expandState)
	{
		this.treeview = treeview;
		this.pDN = newPrefsDigitsNumber;
		this.expandState = expandState;

		treeviewHasTwoLevels = true;
		dataLineNamePosition = 0; //position of name in the data to be printed
		dataLineTypePosition = 4; //position of type in the data to be printed
		allEventsName = Constants.AllRunsNameStr();
		idColumn = 8; //column where the uniqueID of event will be (and will be hidden)
	
		columnsString = new string[] { 
			personName,
			lateralityName,
			weightExtraName,
			Catalog.GetString ("Encoder configuration"),
			Catalog.GetString ("Contraction"),
			datetimeName,
			videoName,
			descriptionName
			//	, "UNIQUEID" //just for debug
		};

		store = getStore(columnsString.Length +1); //+1 because, eventID is not show in last col
		treeview.Model = store;
		prepareHeaders(columnsString);
	}

	/*
	protected override System.Object getObjectFromString (string [] myStringOfData)
	{
	}

	protected override string [] getLineToStore(System.Object myObject)
	{
	}
	
	protected override string [] getSubLineToStore(System.Object myObject, int lineCount)
	{
	}

	protected override string [] printTotal (System.Object myObject)
	{
	}
	
	protected override string [] printAVG (System.Object myObject)
	{
	}

	protected override string [] printSD (System.Object myObject)
	{
	}
	 */
}
