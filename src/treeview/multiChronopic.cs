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


public class TreeViewMultiChronopic : TreeViewEvent
{
	protected string personName = Catalog.GetString("Person");
	private int maxCPs;

	public TreeViewMultiChronopic ()
	{
	}

	//session is created or loaded, we know maxCPs will be written
	public TreeViewMultiChronopic (Gtk.TreeView treeview, int newPrefsDigitsNumber, ExpandStates expandState, int maxCPs)
	{
		this.treeview = treeview;
		pDN = newPrefsDigitsNumber;
		this.expandState = expandState;
		this.maxCPs = maxCPs;

		treeviewHasTwoLevels = true;
		dataLineNamePosition = 0; //position of name in the data to be printed


		//These are Not used, TreeViewEvent/Fill will be called always without "filter" here
		dataLineTypePosition = -1; //position of type in the data to be printed
		allEventsName = "";
		
		if(maxCPs == 2) 
			columnsString = new string[]{ personName, 
				Catalog.GetString("Time"), 
				Catalog.GetString("State") + "\nCP1", "\nCP2",
				Catalog.GetString("Change") + "\nCP1", "\nCP2",
				Catalog.GetString("IN") + "-" + Catalog.GetString("IN") + "\nCP1", "\nCP2",
				Catalog.GetString("OUT") + "-" + Catalog.GetString("OUT") + "\nCP1", "\nCP2",
				descriptionName};
		else if (maxCPs == 3) 
			columnsString = new string[]{ personName, 
				Catalog.GetString("Time"), 
				Catalog.GetString("State") + "\nCP1", "\nCP2", "\nCP3",
				Catalog.GetString("Change") + "\nCP1", "\nCP2", "\nCP3",
				Catalog.GetString("IN") + "-" + Catalog.GetString("IN") + "\nCP1", "\nCP2", "\nCP3",
				Catalog.GetString("OUT") + "-" + Catalog.GetString("OUT") + "\nCP1", "\nCP2", "\nCP3",
				descriptionName};
		else  // ==4
			columnsString = new string[]{ personName, 
				Catalog.GetString("Time"), 
				Catalog.GetString("State") + "\nCP1", "\nCP2", "\nCP3", "\nCP4",
				Catalog.GetString("Change") + "\nCP1", "\nCP2", "\nCP3", "\nCP4",
				Catalog.GetString("IN") + "-" + Catalog.GetString("IN") + "\nCP1", "\nCP2", "\nCP3", "\nCP4",
				Catalog.GetString("OUT") + "-" + Catalog.GetString("OUT") + "\nCP1", "\nCP2", "\nCP3", "\nCP4",
				descriptionName};


		eventIDColumn = columnsString.Length ; //column where the uniqueID of event will be (and will be hidden). 
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
		mc.Vars = myStringOfData[17].ToString(); 
		mc.Description = myStringOfData[18].ToString(); 
		mc.Simulated = Convert.ToInt32(myStringOfData[19].ToString()); 
		
		return mc;
	}
	
	protected override string [] getLineToStore(System.Object myObject)
	{
		MultiChronopic mc = (MultiChronopic)myObject;
		ArrayList array = mc.AsArrayList(pDN);
		
		string title;

		string typeExtra = mc.GetCPsString();
		if(mc.Type == Constants.RunAnalysisName) 
			typeExtra = mc.Vars + "cm " + Util.TrimDecimals(mc.GetTimeRunA(), pDN) + "s " + Util.TrimDecimals(mc.GetAVGSpeedRunA(), pDN) + "m/s";
		title = mc.Type + " " + typeExtra;
		title += " " + array.Count.ToString() +"n";
		if(mc.Simulated == Constants.Simulated)
			title += Constants.SimulatedTreeviewStr() + " ";

		string [] myData = new String [19+1];
		int count = 0;
		myData[count++] = title;
		
		for(int i=0; i<17;i++) 
			myData[count++] = "";
		
		myData[count++] = mc.Description;
		myData[count++] = mc.UniqueID.ToString();
		return mc.DeleteCols(myData, maxCPs, false);
	}
	
	protected override int getNumOfSubEvents(System.Object myObject)
	{
		MultiChronopic mc = (MultiChronopic)myObject;
		if(mc.Type == Constants.RunAnalysisName) {
			int cp2In = Util.GetNumberOfJumps(mc.Cp2InStr,false);
			int cp2Out = Util.GetNumberOfJumps(mc.Cp2OutStr,false);
			if(cp2In < cp2Out)
				return 1 + cp2In;	//first "1+" is for the row with column names
			else
				return 1 + cp2Out;	//first "1+" is for the row with column names
		}

		int cp1 = Util.GetNumberOfJumps(mc.Cp1InStr,false) + Util.GetNumberOfJumps(mc.Cp1OutStr,false);
		int cp2 = Util.GetNumberOfJumps(mc.Cp2InStr,false) + Util.GetNumberOfJumps(mc.Cp2OutStr,false);

		int cp3 = 0;
		if(maxCPs >= 3)
			cp3 = Util.GetNumberOfJumps(mc.Cp3InStr,false) + Util.GetNumberOfJumps(mc.Cp3OutStr,false);
		int cp4 = 0;
		if(maxCPs == 4)
			cp4 = Util.GetNumberOfJumps(mc.Cp4InStr,false) + Util.GetNumberOfJumps(mc.Cp4OutStr,false);

		return 1 + cp1 + cp2 + cp3 +cp4; //first "1+" is for the row with the initial data
	} 
	
	//no total here
	protected override void addStatisticInfo(TreeIter iterDeep, System.Object myObject) {
		//store.AppendValues(iterDeep, printTotal(myObject, 19+1));
		
		MultiChronopic mc = (MultiChronopic)myObject;
		if(mc.Type == Constants.RunAnalysisName) {
		} else {
			store.AppendValues (iterDeep, printAVG (myObject));
			store.AppendValues (iterDeep, printSD (myObject));
		}

	}
	
	protected override string [] printAVG (System.Object myObject)
	{
		MultiChronopic mc = (MultiChronopic)myObject;
		string [] averages = mc.Statistics(true, pDN); //first boolean is averageOrSD
		
		string [] myData = new String [19+1];
		int count = 0;
		myData[count++] = Catalog.GetString("AVG");
		
		for(int i=0; i<9;i++) 
			myData[count++] = "";

		for(int i=0; i<8;i++) 
			myData[count++] = Util.RemoveZeroOrMinus(Util.TrimDecimals( averages[i], pDN ));

		myData[count++] = ""; //desc
		myData[count++] = "-1"; //mark to non select here, select first line 
		
		return mc.DeleteCols(myData, maxCPs, false);
	}

	protected override string [] printSD (System.Object myObject)
	{
		MultiChronopic mc = (MultiChronopic)myObject;
		string [] sds = mc.Statistics(false, pDN); //first boolean is averageOrSD
		
		string [] myData = new String [19+1];
		int count = 0;
		myData[count++] = Catalog.GetString("SD");
		
		for(int i=0; i<9;i++) 
			myData[count++] = "";
		
		for(int i=0; i<8;i++) 
			myData[count++] = Util.RemoveZeroOrMinus(Util.TrimDecimals( sds[i], pDN ));

		myData[count++] = ""; //desc
		myData[count++] = "-1"; //mark to non select here, select first line 
		
		return mc.DeleteCols(myData, maxCPs, false);
	}
			
	protected override string [] getSubLineToStore(System.Object myObject, int lineCount)
	{
		MultiChronopic mc = (MultiChronopic)myObject;
		//write line for treeview
		string [] myData = new String [19+1];

		if(mc.Type == Constants.RunAnalysisName) {
			if(lineCount == 0) {
				int count = 0;
				myData[count++] = Catalog.GetString("Stride");
				myData[count++] = "TC";
				myData[count++] = "TF"; 
				myData[count++] = "TT"; 
				myData[count++] = Catalog.GetString("Freq."); 
				myData[count++] = Catalog.GetString("Width"); 
				myData[count++] = Catalog.GetString("Height"); 
				myData[count++] = Catalog.GetString("Angle"); 
				for(int i=0; i<10;i++) 
					myData[count++] = "";

				myData[count++] = ""; //description column
				myData[count++] = "-1"; //mark to non select here, select first line 
				return mc.DeleteCols(myData, maxCPs, false);
			} else {
				ArrayList array = mc.AsArrayList(pDN);
				return mc.DeleteCols( array[lineCount-1].ToString().Split(new char[] {':'} ), maxCPs, false );
			}
		}

		//not runAnalsysis
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
			return mc.DeleteCols(myData, maxCPs, false);
		} else {
			ArrayList array = mc.AsArrayList(pDN);
			return mc.DeleteCols( array[lineCount-1].ToString().Split(new char[] {':'} ), maxCPs, false );
		}

	}
	
}
