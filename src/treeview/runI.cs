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


public class TreeViewRunsInterval : TreeViewRuns
{
	RunType runType;
	private bool barsAreSpeeds;

	public TreeViewRunsInterval (Gtk.TreeView treeview, int newPrefsDigitsNumber, bool metersSecondsPreferred, ExpandStates expandState, bool barsAreSpeeds)
	{
		this.treeview = treeview;
		this.pDN = newPrefsDigitsNumber;
		this.metersSecondsPreferred = metersSecondsPreferred;
		this.expandState = expandState;
		this.barsAreSpeeds = barsAreSpeeds;

		string runnerName = Catalog.GetString("Runner");
		string speedName = Catalog.GetString("Speed");
		if(metersSecondsPreferred)
			speedName += "\n(m/s)";
		else
			speedName += "\n(km/h)";

		string lapTimeName = Catalog.GetString("Lap time") + "\n(s)";
		string splitTimeName = Catalog.GetString("Split time") + "\n(s)";
		
		treeviewHasTwoLevels = true;
		dataLineNamePosition = 0; //position of name in the data to be printed
		dataLineTypePosition = 4; //position of type in the data to be printed
		allEventsName = Constants.AllRunsNameStr();
		boldableColumns_l = new List<int> { 1, 2 }; //speed, laptime
		idColumn = 7; //column where the uniqueID of event will be (and will be hidden)
		
		columnsString = new string[]{runnerName, speedName, lapTimeName, splitTimeName, datetimeName, videoName, descriptionName};
			//,"ID delete" //just for debug
		store = getStore(columnsString.Length +1); //+1 because, eventID is not show in last col
		treeview.Model = store;
		prepareHeaders(columnsString);
	}

	protected override bool shouldRenderBoldable (string columnTitle)
	{
		if (barsAreSpeeds && columnTitle.StartsWith (Catalog.GetString ("Speed")))
			return true;

		if (! barsAreSpeeds && columnTitle.StartsWith (Catalog.GetString ("Lap time")))
			return true;

		return false;
	}

	// result cells than can be in bold to match the results shown on bars
	public override void ResultsInBarsRowChanged ()
	{
		TreeIter iter = new TreeIter();
		if(! treeview.Model.GetIterFirst (out iter))
			return;

		do {
			TreeIter iterDeep = new TreeIter ();
			treeview.Model.IterChildren (out iterDeep, iter);
			do {
				TreeIter iterDeep2 = new TreeIter ();
				treeview.Model.IterChildren (out iterDeep2, iterDeep);
				for (int i = 0; i <= 1; i ++) // related statistic info is in row 0 or 1
				{
					foreach (int j in boldableColumns_l)
						if (treeview.Model.GetValue (iterDeep2, j) != null &&
								((string) treeview.Model.GetValue (iterDeep2, j)).StartsWith (boldMark))
						{
							TreePath path = store.GetPath (iterDeep2);
							//LogB.Information ("EmitRowChanged: " + path.ToString ());
							treeview.Model.EmitRowChanged (path, iterDeep2);
						}
					treeview.Model.IterNext (ref iterDeep2);
				}
			} while (treeview.Model.IterNext (ref iterDeep));
		} while (treeview.Model.IterNext (ref iter));
	}

	protected override System.Object getObjectFromString(string [] myStringOfData) {
		RunInterval myRunI = new RunInterval();
		myRunI.UniqueID = Convert.ToInt32(myStringOfData[1].ToString()); 
		myRunI.Type = myStringOfData[4].ToString();
		myRunI.DistanceTotal = Convert.ToDouble(myStringOfData[5].ToString());
		myRunI.TimeTotal = Convert.ToDouble(myStringOfData[6].ToString());
		myRunI.DistanceInterval = Convert.ToDouble(myStringOfData[7].ToString());
		myRunI.IntervalTimesString = myStringOfData[8].ToString();
		myRunI.Limited = myStringOfData[11].ToString();
		myRunI.Description = myStringOfData[10].ToString();
		myRunI.Simulated = Convert.ToInt32(myStringOfData[12].ToString());
		myRunI.Datetime = myStringOfData[14].ToString();
		//speed is not needed to define
			
		runType = SqliteRunIntervalType.SelectAndReturnRunIntervalType(myRunI.Type, Sqlite.IsOpened);
		
		return myRunI;
	}
	
	protected override string [] getLineToStore(System.Object myObject)
	{
		RunInterval newRunI = (RunInterval)myObject;
		
		string title = Catalog.GetString(newRunI.Type);
		if(newRunI.Simulated == Constants.Simulated)
			title += Constants.SimulatedTreeviewStr() + " ";

		string myTypeComplet = title + "(" + newRunI.DistanceInterval + "x" + Util.GetLimitedRounded(newRunI.Limited, pDN) + ")";
		
		string [] myData = new String [getColsNum()];
		int count = 0;
		myData[count++] = myTypeComplet;
		myData[count++] = "";		//speed
		myData[count++] = "";		//lapTime 
		myData[count++] = "";		//splitTime
		myData[count++] = UtilDate.GetDatetimePrint(UtilDate.FromFile(newRunI.Datetime));

		if (UtilList.StartsWithInListString (videos_l, string.Format ("{0}-{1}", Constants.TestTypes.RUN_I, newRunI.UniqueID)))
			myData[count++] = Catalog.GetString ("Yes");
		else
			myData[count++] = Catalog.GetString ("No");

		myData[count++] = newRunI.Description;
		myData[count++] = newRunI.UniqueID.ToString();
		return myData;
	}
	
	protected override string [] getSubLineToStore(System.Object myObject, int lineCount)
	{
		RunInterval newRunI = (RunInterval)myObject;

		//check the time
		string [] myStringFull = newRunI.IntervalTimesString.Split(new char[] {'='});
		string timeInterval = myStringFull[lineCount];

		
		//write line for treeview
		string [] myData = new String [getColsNum()];
		int count = 0;

		if(newRunI.DistanceInterval == -1) {
			runType = SqliteRunIntervalType.SelectAndReturnRunIntervalType(newRunI.Type, Sqlite.IsOpened);
			myData[count++] = (lineCount + 1).ToString() +  
				" (" + Util.GetRunIVariableDistancesStringRow(runType.DistancesString, lineCount).ToString() + "m)";
			
			myData[count++] =  Util.TrimDecimals( 
					Util.GetSpeed(
						Util.GetRunIVariableDistancesStringRow(runType.DistancesString, lineCount).ToString(), //distancesString (variable)
						timeInterval,
						metersSecondsPreferred )
					, pDN );
		} else {
			myData[count++] = (lineCount +1).ToString();

			myData[count++] =  Util.TrimDecimals( 
					Util.GetSpeed(
						newRunI.DistanceInterval.ToString(), //distanceInterval (same for all subevents)
						timeInterval,
						metersSecondsPreferred )
					, pDN );
		}

		myData[count++] = Util.TrimDecimals( timeInterval, pDN ); //lapTime
		
		myData[count++] = Util.TrimDecimals( getSplitTime(newRunI.IntervalTimesString, lineCount), pDN ); //splitTime
		myData[count++] = ""; 	//datetime
		myData[count++] = "";	//video
		myData[count++] = "";	//description
		
		myData[count++] = MarkNonSelectRowSubEvent.ToString (); //mark to non select here, select first line

		return myData;
	}

	private double getSplitTime(string intervalTimesString, int lineCount) 
	{
		string [] myStringFull = intervalTimesString.Split(new char[] {'='});
		double splitTime = 0;
		for (int i=0; i <= lineCount; i++)
			splitTime += Convert.ToDouble(myStringFull[i]);

		return splitTime;
	}
	
	protected override int getNumOfSubEvents(System.Object myObject)
	{
		RunInterval newRunI = (RunInterval)myObject;

		string [] myStringFull = newRunI.IntervalTimesString.Split(new char[] {'='});

		return myStringFull.Length; 
	} 
			
	protected override string [] printTotal (System.Object myObject)
	{
		RunInterval newRunI = (RunInterval)myObject;

		string [] myData = new String [getColsNum()];
		int count = 0;
		myData[count++] = Catalog.GetString("Total");
		myData[count++] = "";
		myData[count++] = boldMark + Util.TrimDecimals( newRunI.TimeTotal.ToString(), pDN ); //lapTime
		myData[count++] = "";							//splitTime
		myData[count++] = "";							//datetime
		myData[count++] = "";							//video
		myData[count++] = "";							//description
		
		myData[count++] = MarkNonSelectRowSubEvent.ToString (); //mark to non select here, select first line
		
		return myData;
	}
	
	protected override string [] printAVG (System.Object myObject)
	{
		RunInterval newRunI = (RunInterval)myObject;

		string [] myData = new String [getColsNum()];
		int count = 0;
		myData[count++] = Catalog.GetString("AVG");
		myData[count++] = boldMark + Util.TrimDecimals(Util.GetSpeed(
					newRunI.DistanceTotal.ToString(),
					newRunI.TimeTotal.ToString(),
					metersSecondsPreferred), 
				pDN);
		myData[count++] = Util.TrimDecimals( 
				Util.GetAverage(newRunI.IntervalTimesString).ToString()	//AVG of intervalTimesString
							, pDN );
		myData[count++] = "";							//splitTime
		myData[count++] = "";							//datetime
		myData[count++] = "";							//video
		myData[count++] = "";							//description

		myData[count++] = MarkNonSelectRowSubEvent.ToString (); //mark to non select here, select first line
		
		return myData;
	}

	protected override string [] printSD (System.Object myObject)
	{
		RunInterval newRunI = (RunInterval)myObject;

		string [] myData = new String [getColsNum()];
		int count = 0;
		myData[count++] = Catalog.GetString("SD");
		myData[count++] = "";
		myData[count++] = Util.TrimDecimals(Util.CalculateSD(
					Util.ChangeEqualForColon(newRunI.IntervalTimesString),
					Util.GetTotalTime(newRunI.IntervalTimesString),
					Util.GetNumberOfJumps(newRunI.IntervalTimesString, false)).ToString(),
				pDN);
		myData[count++] = "";							//splitTime
		myData[count++] = "";							//datetime
		myData[count++] = "";							//video
		myData[count++] = "";							//description
		
		myData[count++] = MarkNonSelectRowSubEvent.ToString (); //mark to non select here, select first line
		
		return myData;
	}

	public bool BarsAreSpeeds {
		set { barsAreSpeeds = value; }
	}

}
