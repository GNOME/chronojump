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
 */

using System;
using System.Data;
using Gtk;
using System.Collections; //ArrayList
using Mono.Unix;


public class TreeViewMultiChronopic : TreeViewEvent
{
	protected string personName = Catalog.GetString("Person");

	public TreeViewMultiChronopic ()
	{
	}
	
	public TreeViewMultiChronopic (Gtk.TreeView treeview, int newPrefsDigitsNumber, ExpandStates expandState)
	{
		this.treeview = treeview;
		pDN = newPrefsDigitsNumber;
		this.expandState = expandState;

		treeviewHasTwoLevels = true;
		dataLineNamePosition = 0; //position of name in the data to be printed


		//These are Not used, TreeViewEvent/Fill will be called always without "filter" here
		dataLineTypePosition = -1; //position of type in the data to be printed
		allEventsName = "";
		

		columnsString = new string[]{personName, 
			Catalog.GetString("Time"), 
			Catalog.GetString("State") + "\nCP1", "\nCP2", "\nCP3", "\nCP4",
			Catalog.GetString("Change") + "\nCP1", "\nCP2", "\nCP3", "\nCP4",
			Catalog.GetString("IN") + "-" + Catalog.GetString("IN") + "\nCP1", "\nCP2", "\nCP3", "\nCP4",
			Catalog.GetString("OUT") + "-" + Catalog.GetString("OUT") + "\nCP1", "\nCP2", "\nCP3", "\nCP4",
			descriptionName};

		eventIDColumn = columnsString.Length ; //column where the uniqueID of event will be (and will be hidded). 
		store = getStore(columnsString.Length +1); //+1 because, eventID is not show in last col
		treeview.Model = store;
		prepareHeaders(columnsString);
	}

	protected override System.Object getObjectFromString(string [] myStringOfData) {
		MultiChronopic mc = new MultiChronopic();
		mc.UniqueID = Convert.ToInt32(myStringOfData[1].ToString()); 
		mc.Type = myStringOfData[4].ToString(); 
		mc.Cp1StartedIn = Convert.ToInt32(myStringOfData[5].ToString()); 
		mc.Cp2StartedIn = Convert.ToInt32(myStringOfData[6].ToString()); 
		mc.Cp3StartedIn = Convert.ToInt32(myStringOfData[7].ToString()); 
		mc.Cp4StartedIn = Convert.ToInt32(myStringOfData[8].ToString()); 
		mc.Cp1InStr = myStringOfData[9].ToString(); 
		mc.Cp1OutStr = myStringOfData[10].ToString(); 
		mc.Cp2InStr = myStringOfData[11].ToString(); 
		mc.Cp2OutStr = myStringOfData[12].ToString(); 
		mc.Cp3InStr = myStringOfData[13].ToString(); 
		mc.Cp3OutStr = myStringOfData[14].ToString(); 
		mc.Cp4InStr = myStringOfData[15].ToString(); 
		mc.Cp4OutStr = myStringOfData[16].ToString(); 
		mc.Description = myStringOfData[17].ToString(); 
		mc.Simulated = Convert.ToInt32(myStringOfData[18].ToString()); 
		
		return mc;
	}
	
	protected override string [] getLineToStore(System.Object myObject)
	{
		MultiChronopic mc = (MultiChronopic)myObject;
		
		string title = mc.Type;
		if(mc.Simulated == Constants.Simulated)
			title += " (s) ";

		//string myTypeComplet = title + "(" + newRunI.DistanceInterval + "x" + Util.GetLimitedRounded(newRunI.Limited, pDN) + ")";
		
		string [] myData = new String [getColsNum()];
		int count = 0;
		//myData[count++] = myTypeComplet;
		myData[count++] = title;
		myData[count++] = ""; //time
		myData[count++] = ""; //state
		myData[count++] = "";
		myData[count++] = "";
		myData[count++] = "";
		myData[count++] = "";//change
		myData[count++] = "";
		myData[count++] = "";
		myData[count++] = "";
		myData[count++] = "";//in-in
		myData[count++] = "";
		myData[count++] = "";
		myData[count++] = "";
		myData[count++] = "";//out-out
		myData[count++] = "";
		myData[count++] = "";
		myData[count++] = "";
		myData[count++] = mc.Description;
		myData[count++] = mc.UniqueID.ToString();
		return myData;
	}

	protected override int getNumOfSubEvents(System.Object myObject)
	{
		MultiChronopic mc = (MultiChronopic)myObject;

		int cp1 = Util.GetNumberOfJumps(mc.Cp1InStr,false) + Util.GetNumberOfJumps(mc.Cp1OutStr,false);
		int cp2 = Util.GetNumberOfJumps(mc.Cp2InStr,false) + Util.GetNumberOfJumps(mc.Cp2OutStr,false);
		int cp3 = Util.GetNumberOfJumps(mc.Cp3InStr,false) + Util.GetNumberOfJumps(mc.Cp3OutStr,false);
		int cp4 = Util.GetNumberOfJumps(mc.Cp4InStr,false) + Util.GetNumberOfJumps(mc.Cp4OutStr,false);

		return 1 + cp1 + cp2 + cp3 +cp4; //first "1+" is for the row with the initial data
	} 
	
	//no statistic here (currently)
	protected override void addStatisticInfo(TreeIter iterDeep, System.Object myObject) {
	}
			
	protected override string [] getSubLineToStore(System.Object myObject, int lineCount)
	{
		MultiChronopic mc = (MultiChronopic)myObject;

		//check the time
//		string [] myStringFull = newRunI.IntervalTimesString.Split(new char[] {'='});
//		string timeInterval = myStringFull[lineCount];
		
		//write line for treeview
		string [] myData = new String [getColsNum()];

		string [] cp1InFull = mc.Cp1InStr.Split(new char[] {'='});
		string [] cp1OutFull = mc.Cp1OutStr.Split(new char[] {'='});
		string [] cp2InFull = mc.Cp2InStr.Split(new char[] {'='});
		string [] cp2OutFull = mc.Cp2OutStr.Split(new char[] {'='});
		string [] cp3InFull = mc.Cp3InStr.Split(new char[] {'='});
		string [] cp3OutFull = mc.Cp3OutStr.Split(new char[] {'='});
		string [] cp4InFull = mc.Cp4InStr.Split(new char[] {'='});
		string [] cp4OutFull = mc.Cp4OutStr.Split(new char[] {'='});


		if(lineCount == 0) {
			int count=0;
			myData[count++] = "0";
			myData[count++] = "0";
			myData[count++] = Util.BoolToInOut(Util.IntToBool(mc.Cp1StartedIn));
			myData[count++] = Util.BoolToInOut(Util.IntToBool(mc.Cp2StartedIn));
			myData[count++] = Util.BoolToInOut(Util.IntToBool(mc.Cp3StartedIn));
			myData[count++] = Util.BoolToInOut(Util.IntToBool(mc.Cp4StartedIn));
			for(int i=0; i<12;i++)
				myData[count++] = "";
				
			myData[count++] = ""; //description column
			myData[count++] = "-1"; //mark to non select here, select first line 
			return myData;
		} else {
			ArrayList array = mc.AsArrayList();
			return array[lineCount-1].ToString().Split(new char[] {':'});
		}

	}
	
}
