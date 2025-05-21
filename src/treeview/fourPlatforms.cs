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


public class TreeViewFourPlatforms : TreeViewEvent
{
	//RunType runType;

	public TreeViewFourPlatforms (Gtk.TreeView treeview, int newPrefsDigitsNumber, ExpandStates expandState)
	{
		this.treeview = treeview;
		this.pDN = newPrefsDigitsNumber;
		this.expandState = expandState;

		string time0ONName = Catalog.GetString ("Time") + " 1 ON" + "\n(s)";
		string time0OFFName = Catalog.GetString ("Time") + " 1 OFF" + "\n(s)";
		string time1ONName = Catalog.GetString ("Time") + " 2 ON" + "\n(s)";
		string time1OFFName = Catalog.GetString ("Time") + " 2 OFF" + "\n(s)";
		string time2ONName = Catalog.GetString ("Time") + " 3 ON" + "\n(s)";
		string time2OFFName = Catalog.GetString ("Time") + " 3 OFF" + "\n(s)";
		string time3ONName = Catalog.GetString ("Time") + " 4 ON" + "\n(s)";
		string time3OFFName = Catalog.GetString ("Time") + " 4 OFF" + "\n(s)";
		
		treeviewHasTwoLevels = true;
		dataLineNamePosition = 0; //position of name in the data to be printed
		//dataLineTypePosition = 4; //position of type in the data to be printed
		allEventsName = "";
		idColumn = 12; //column where the uniqueID of event will be (and will be hidden)
		
		columnsString = new string[] {
			personName,
				time0ONName, time0OFFName,
				time1ONName, time1OFFName,
				time2ONName, time2OFFName,
				time3ONName, time3OFFName,
				datetimeName, videoName, descriptionName};
			//,"ID delete"	}; // just for debug
		store = getStore(columnsString.Length +1); //+1 because, eventID is not show in last col
		treeview.Model = store;
		prepareHeaders(columnsString);
	}

	protected override System.Object getObjectFromString (string [] myStringOfData)
	{
		LogB.Information ("myStringOfData:");
		LogB.Information (Util.StringArrayToString (myStringOfData, "____"));

		FourPlatforms fp = new FourPlatforms (
				Convert.ToInt32 (myStringOfData[1].ToString()),  //uniqueID
				Convert.ToInt32 (myStringOfData[2].ToString()),  //personID
				Convert.ToInt32 (myStringOfData[3].ToString()),  //sessionID
				Convert.ToInt32 (myStringOfData[4].ToString()),  //exerciseID
				UtilList.SQLStringToListDouble (myStringOfData[5].ToString (), "="),
				UtilList.SQLStringToListDouble (myStringOfData[6].ToString (), "="),
				UtilList.SQLStringToListDouble (myStringOfData[7].ToString (), "="),
				UtilList.SQLStringToListDouble (myStringOfData[8].ToString (), "="),
				UtilList.SQLStringToListDouble (myStringOfData[9].ToString (), "="),
				UtilList.SQLStringToListDouble (myStringOfData[10].ToString (), "="),
				UtilList.SQLStringToListDouble (myStringOfData[11].ToString (), "="),
				UtilList.SQLStringToListDouble (myStringOfData[12].ToString (), "="),
				myStringOfData[13].ToString (),			//datetime
				myStringOfData[14].ToString (),			//description
				myStringOfData[15].ToString (),			//videoURL
				Convert.ToDouble (myStringOfData[16].ToString())   //totalTime
				);

		return fp;
	}

	protected override string [] getLineToStore (System.Object myObject)
	{
		FourPlatforms fp = (FourPlatforms) myObject;
		
		//string title = Catalog.GetString(newRunI.Type);
		//string myTypeComplet = title + "(" + newRunI.DistanceInterval + "x" + Util.GetLimitedRounded(newRunI.Limited, pDN) + ")";
		
		string [] myData = new String [getColsNum()];
		int count = 0;
		myData[count++] = string.Format ("{0} ({1} s)", fp.GetCaptureEnumStr (), Util.TrimDecimals (fp.TotalTime, 3));
		myData[count++] = ""; //0on
		myData[count++] = ""; //0off
		myData[count++] = ""; //1on
		myData[count++] = ""; //1off
		myData[count++] = ""; //2on
		myData[count++] = ""; //2off
		myData[count++] = ""; //3on
		myData[count++] = ""; //3off
		myData[count++] = UtilDate.GetDatetimePrint (UtilDate.FromFile (fp.DateTime));

		if (UtilList.StartsWithInListString (videos_l, string.Format ("{0}-{1}", Constants.TestTypes.FOURPLATFORMS, fp.UniqueID)))
			myData[count++] = Catalog.GetString ("Yes");
		else
			myData[count++] = Catalog.GetString ("No");
		myData[count++] = fp.Description;

		myData[count++] = fp.UniqueID.ToString();
		return myData;
	}

	protected override string [] getSubLineToStore (System.Object myObject, int lineCount)
	{
		FourPlatforms fp = (FourPlatforms) myObject;

		//write line for treeview
		string [] myData = new String [getColsNum()];
		int count = 0;

		myData[count++] = ""; //person
		myData[count++] = fp.GetTimeAtChannelAsStr (0, true, lineCount);
		myData[count++] = fp.GetTimeAtChannelAsStr (0, false, lineCount);
		myData[count++] = fp.GetTimeAtChannelAsStr (1, true, lineCount);
		myData[count++] = fp.GetTimeAtChannelAsStr (1, false, lineCount);
		myData[count++] = fp.GetTimeAtChannelAsStr (2, true, lineCount);
		myData[count++] = fp.GetTimeAtChannelAsStr (2, false, lineCount);
		myData[count++] = fp.GetTimeAtChannelAsStr (3, true, lineCount);
		myData[count++] = fp.GetTimeAtChannelAsStr (3, false, lineCount);
		myData[count++] = ""; 	//datetime
		myData[count++] = "";	//video
		myData[count++] = "";	//description
		
		myData[count++] = MarkNonSelectRowSubEvent.ToString (); //mark to non select here, select first line

		return myData;
	}

	protected override int getNumOfSubEvents (System.Object myObject)
	{
		FourPlatforms fp = (FourPlatforms) myObject;
		return fp.GetMaxEventsOnAnyChannel;
	} 

	protected override void addStatisticInfo (TreeIter iterDeep, System.Object myObject)
	{
		/*
		 * max, avg, sd are not very important here.
		 * Depending on test maybe the relevant is the time from one column to another.
		 * Like in 1->2, 1->3, 1->4
		 */
	}

}
