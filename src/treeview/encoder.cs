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
		allEventsName = Constants.AllTestsNameStr();
		idColumn = 8; //column where the uniqueID of event will be (and will be hidden)
	
		columnsString = new string[] { 
			personName,
			lateralityName,
			weightExtraName,
			//Catalog.GetString ("Encoder configuration"),
			Catalog.GetString ("Contraction"),
			Catalog.GetString ("Mean power"),
			Catalog.GetString ("Mean speed"),
			Catalog.GetString ("Mean force"),
//			datetimeName,
//			videoName,
			descriptionName
			//	, "UNIQUEID" //just for debug
		};

		store = getStore(columnsString.Length +1); //+1 because, eventID is not show in last col
		treeview.Model = store;
		prepareHeaders(columnsString);
	}

	public override void FillEncoder (List<List<EncoderSQL>> eSQL_ll, string filterExercise, List<string> videos_l)
	{
		LogB.Information ("called Fill Encoder");
		this.videos_l = videos_l;

		TreeIter iter = new TreeIter();
		TreeIter iterDeep = new TreeIter(); //only used by two levels treeviews
		int tempPersonID = -1; //one value that's not possible

		foreach (List<EncoderSQL> eSQL_l in eSQL_ll)
		{
			EncoderSQL eSQL0 = (EncoderSQL) eSQL_l[0]; //to have code a bit clearer

			//show always the names of persons ...
			if (tempPersonID != eSQL0.PersonID)
			{
				iter = store.AppendValues (createPersonRow (eSQL0.PersonID, eSQL0.PersonName));
				tempPersonID = eSQL0.PersonID;
			}

			//... but if we selected one type of test of this mode and this it's not the type, don't show
			if (filterExercise == allEventsName || filterExercise == Catalog.GetString (eSQL0.exerciseName))
			{
				//getLineToStoreFromString is overriden in two level treeviews
				iterDeep = store.AppendValues (iter, getLineToStore (eSQL0));
				if (treeviewHasTwoLevels)
				{
					//addStatisticInfo (iterDeep, myEvent);
					for (int i = 1; i < eSQL_l.Count; i ++)
						store.AppendValues (iterDeep, getSubLineToStore (eSQL_l[i], i));
				}
			}
		}
	}


	/*
	 * unused as we used FillEncoder instead of Event.Fill
	protected override System.Object getObjectFromString (string [] strA)
	{
		return new EncoderSQL (
				Convert.ToInt32 (strA[1]), 	//uniqueID
				strA[6],	//laterality
				strA[7],	//extraWeight
				strA[5], 	//eccon
				strA[13], 	//description
				strA[20]  	//exerciseName
				);
	}
	*/

	protected override string [] getLineToStore (System.Object myObject)
	{
		EncoderSQL eSQL = (EncoderSQL) myObject;

		string [] myData = new String [getColsNum()];
		int count = 0;

		myData[count++] = eSQL.exerciseName;
		myData[count++] = Catalog.GetString (eSQL.laterality);
		myData[count++] = eSQL.extraWeight;
		myData[count++] = eSQL.ecconLong;
		myData[count++] = ""; //meanPower
		myData[count++] = ""; //meanSpeed
		myData[count++] = ""; //meanForce
		myData[count++] = eSQL.Description;

		return myData;
	}

	// TODO: add meanPower, meanSpeed, meanForce
	protected override string [] getSubLineToStore (System.Object myObject, int i)
	{
		EncoderSQL eSQL = (EncoderSQL) myObject;

		string [] myData = new String [getColsNum()];
		int count = 0;

		myData[count++] = i.ToString ();
		myData[count++] = ""; //Catalog.GetString (eSQL.laterality);
		myData[count++] = ""; //eSQL.extraWeight;
		myData[count++] = ""; //eSQL.ecconLong;
		myData[count++] = eSQL.meanPower;
		myData[count++] = eSQL.meanSpeed;
		myData[count++] = eSQL.meanForce;
		myData[count++] = ""; //eSQL.Description;

		return myData;
	}

	/*
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
