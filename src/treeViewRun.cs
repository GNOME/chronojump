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
 * Xavier de Blas: 
 * http://www.xdeblas.com, http://www.deporteyciencia.com (parleblas)
 */

using System;
using System.Data;
using Gtk;
using System.Collections; //ArrayList
using Mono.Unix;


public class TreeViewRuns : TreeViewEvent
{
	protected bool metersSecondsPreferred;
		
	public TreeViewRuns ()
	{
	}
	
	public TreeViewRuns (Gtk.TreeView treeview, int newPrefsDigitsNumber, bool metersSecondsPreferred)
	{
		this.treeview = treeview;
		this.pDN = newPrefsDigitsNumber;
		this.metersSecondsPreferred = metersSecondsPreferred;

		treeviewHasTwoLevels = false;
		dataLineNamePosition = 0; //position of name in the data to be printed
		dataLineTypePosition = 4; //position of type in the data to be printed
		allEventsName = Constants.AllRunsName;
		eventIDColumn = 4; //column where the uniqueID of event will be (and will be hidded)
	
		string runnerName = Catalog.GetString("Runner");
		string speedName = Catalog.GetString("Speed");
		string distanceName = Catalog.GetString("Distance");
		string timeName = Catalog.GetString("Time");

		string [] columnsString = { runnerName, speedName, distanceName, timeName };
		store = getStore(columnsString.Length +1); //+1 because, eventID is not show in last col
		treeview.Model = store;
		prepareHeaders(columnsString);
	}

	
	protected override System.Object getObjectFromString(string [] myStringOfData) {
		Run myRun = new Run();
		myRun.UniqueID = Convert.ToInt32(myStringOfData[1].ToString()); 
		myRun.Type = myStringOfData[4].ToString();
		myRun.Distance = Convert.ToDouble(myStringOfData[5].ToString());
		myRun.Time = Convert.ToDouble(myStringOfData[6].ToString());
		//speed is not needed to define

		return myRun;
	}

	protected override string [] getLineToStore(System.Object myObject)
	{
		Run newRun = (Run)myObject;

		string [] myData = new String [5]; //columnsString +1
		int count = 0;
		myData[count++] = newRun.Type;
		myData[count++] = Util.TrimDecimals(newRun.Speed.ToString(), pDN);
		myData[count++]	= Util.TrimDecimals(newRun.Distance.ToString(), pDN);
		myData[count++] = Util.TrimDecimals(newRun.Time.ToString(), pDN);
		myData[count++] = newRun.UniqueID.ToString();
		return myData;
	}
	
}


public class TreeViewRunsInterval : TreeViewRuns
{
	public TreeViewRunsInterval (Gtk.TreeView treeview, int newPrefsDigitsNumber, bool metersSecondsPreferred)
	{
		this.treeview = treeview;
		this.pDN = newPrefsDigitsNumber;
		this.metersSecondsPreferred = metersSecondsPreferred;

		string runnerName = Catalog.GetString("Runner");
		string speedName = Catalog.GetString("Speed");
		string timeName = Catalog.GetString("Time");
		
		treeviewHasTwoLevels = true;
		dataLineNamePosition = 0; //position of name in the data to be printed
		dataLineTypePosition = 4; //position of type in the data to be printed
		allEventsName = Constants.AllRunsName;
		eventIDColumn = 3;
		
		string [] columnsString = { runnerName, speedName, timeName };
		store = getStore(columnsString.Length +1); //+1 because, eventID is not show in last col
		treeview.Model = store;
		prepareHeaders(columnsString);
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
		//speed is not needed to define
		
		return myRunI;
	}
	
	protected override string [] getLineToStore(System.Object myObject)
	{
		RunInterval newRunI = (RunInterval)myObject;

		//typeComplet
		string myTypeComplet = newRunI.Type + "(" + newRunI.DistanceInterval + "x" + newRunI.Limited + ") AVG: ";
		
		string [] myData = new String [4]; //columnsString +1
		int count = 0;
		myData[count++] = myTypeComplet;
		myData[count++] = Util.TrimDecimals(newRunI.Speed.ToString(), pDN);
		myData[count++] = Util.TrimDecimals( 
				Util.GetAverage(newRunI.IntervalTimesString).ToString()	//AVG of intervalTimesString
							, pDN );
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
		string [] myData = new String [4]; //columnsString +1
		int count = 0;
		myData[count++] = (lineCount +1).ToString();
		myData[count++] =  Util.TrimDecimals( 
				Util.GetSpeed(
					newRunI.DistanceInterval.ToString(), //distanceInterval (same for all subevents)
					timeInterval )
				, pDN );

		myData[count++] = Util.TrimDecimals( timeInterval, pDN );
		myData[count++] = newRunI.UniqueID.ToString(); 

		return myData;
	}
	
	protected override int getNumOfSubEvents(System.Object myObject)
	{
		RunInterval newRunI = (RunInterval)myObject;

		string [] myStringFull = newRunI.IntervalTimesString.Split(new char[] {'='});

		return myStringFull.Length; 
	} 
			
}
