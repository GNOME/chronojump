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


public class TreeViewJumps : TreeViewEvent
{
	protected bool showHeight;
	protected bool showInitialSpeed;
	protected bool showAngle;
	protected bool showQIndex;
	protected bool showDjIndex;
	
	protected string jumperName = Catalog.GetString("Jumper");
	protected string weightName = Catalog.GetString("Weight");
	protected string fallName = Catalog.GetString("Fall") + "\n(cm)";
	protected string heightName = Catalog.GetString("Height") + "\n(cm)";
	protected string initialSpeedName = Catalog.GetString("Initial Speed");
	protected string angleName = Catalog.GetString("Angle");

	//one of both indexes can be shown if selected on preferences
	protected string qIndexName = "Q Index" + "\n(%)";
	protected string djIndexName = "Dj Index" + "\n(%)";
	
	protected double personWeight;
	protected bool metersSecondsPreferred;
		
	public TreeViewJumps ()
	{
	}
	
	public TreeViewJumps (Gtk.TreeView treeview, bool showHeight, bool showInitialSpeed, bool showAngle, 
			bool showQIndex, bool showDjIndex, int newPrefsDigitsNumber, 
			bool weightPercentPreferred, bool metersSecondsPreferred, 
			ExpandStates expandState)
	{
		this.treeview = treeview;
		this.showHeight = showHeight;
		this.showInitialSpeed = showInitialSpeed;
		this.showAngle = showAngle;
		this.showQIndex = showQIndex;
		this.showDjIndex = showDjIndex;
		pDN = newPrefsDigitsNumber;
		this.weightPercentPreferred = weightPercentPreferred;
		this.metersSecondsPreferred = metersSecondsPreferred;
		this.expandState = expandState;


		treeviewHasTwoLevels = false;
		dataLineNamePosition = 0; //position of name in the data to be printed
		dataLineTypePosition = 4; //position of type in the data to be printed
		allEventsName = Constants.AllJumpsName;
		
		if(weightPercentPreferred)
			weightName += "\n(%)";
		else
			weightName += "\n(Kg)";

		string [] columnsStringPre = { jumperName, "TC\n(s)", "TF\n(s)", weightName, fallName }; //Note: if this changes, check the '5's in obtainColumnsString

		columnsString = obtainColumnsString(columnsStringPre);
	

		eventIDColumn = columnsString.Length ; //column where the uniqueID of event will be (and will be hidded). 
		store = getStore(columnsString.Length +1); //+1 because, eventID is not show in last col
		treeview.Model = store;
		prepareHeaders(columnsString);
		
		//on creation, treeview is minimized
		expandState = ExpandStates.MINIMIZED;
	}

	protected override int getColsNum() {
		int i = columnsString.Length;
		if (showHeight)  
			i ++;
		if (showInitialSpeed) 
			i ++;
		if (showAngle) 
			i ++;
		if (showQIndex || showDjIndex) 
			i ++;
		return i +1; //+1 is for the uniqueID hidden col (last)
	}
	
	protected string [] obtainColumnsString(string [] columnsStringPre) 
	{
		//check long of new array
		int i = 6;
		if (showHeight)  
			i ++;
		if (showInitialSpeed) 
			i ++;
		if (showAngle) 
			i ++;
		if (showQIndex || showDjIndex) 
			i ++;

		//create new array
		string [] columnsString = new String[i];
		Array.Copy(columnsStringPre, columnsString, 5);

	
		if(metersSecondsPreferred)
			initialSpeedName += "\n(m/s)";
		else
			initialSpeedName += "\n(Km/h)";


		//fill names
		i = 5;
		if (showHeight)  
			columnsString[i++] = heightName;
		if (showInitialSpeed) 
			columnsString[i++] = initialSpeedName;
		if (showAngle) 
			columnsString[i++] = angleName;
		if (showQIndex) 
			columnsString[i++] = qIndexName;
		if (showDjIndex) 
			columnsString[i++] = djIndexName;
			
		columnsString[i++] = descriptionName;

		return columnsString;
	}
	
	protected override System.Object getObjectFromString(string [] myStringOfData) {
		Jump myJump = new Jump();
		myJump.UniqueID = Convert.ToInt32(myStringOfData[1].ToString()); 
		myJump.Type = myStringOfData[4].ToString();
		myJump.Tv = Convert.ToDouble(myStringOfData[5].ToString());
		myJump.Tc = Convert.ToDouble(myStringOfData[6].ToString());
		myJump.Fall = Convert.ToInt32(myStringOfData[7].ToString());
		myJump.Angle = Convert.ToDouble(myStringOfData[10].ToString());
		myJump.Description = myStringOfData[9].ToString();
		myJump.Simulated = Convert.ToInt32(myStringOfData[11].ToString());

		//we create the jump with a weight of percent or kk
		if(weightPercentPreferred)
			myJump.Weight = Convert.ToDouble(myStringOfData[8].ToString());
		else
			myJump.Weight = Util.WeightFromPercentToKg(
					Convert.ToDouble(myStringOfData[8]), 
					Convert.ToDouble(myStringOfData[12]));


		return myJump;
	}

	protected override string [] getLineToStore(System.Object myObject)
	{
		Jump newJump = (Jump)myObject;

		string title = newJump.Type;
		if(newJump.Simulated == 1)
			title += " (s)";

		string [] myData = new String [getColsNum()];
		int count = 0;
		//myData[count++] = newJump.Type;
		myData[count++] = title;
		myData[count++] = Util.TrimDecimals(newJump.Tc.ToString(), pDN);
		myData[count++] = Util.TrimDecimals(newJump.Tv.ToString(), pDN);
		
		myData[count++] = Util.TrimDecimals(newJump.Weight.ToString(), pDN);

		myData[count++] = newJump.Fall.ToString();
		if (showHeight)  
			myData[count++] = Util.TrimDecimals(Util.GetHeightInCentimeters(newJump.Tv.ToString()), pDN);
		if (showInitialSpeed) 
			myData[count++] = Util.TrimDecimals(Util.GetInitialSpeed(newJump.Tv.ToString(), metersSecondsPreferred), pDN);
		if (showAngle) 
			myData[count++] = Util.TrimDecimals(newJump.Angle.ToString(), pDN);
		if(showQIndex)
			myData[count++] = Util.TrimDecimals(Util.GetQIndex(newJump.Tv, newJump.Tc).ToString(), pDN);
		if(showDjIndex)
			myData[count++] = Util.TrimDecimals(Util.GetDjIndex(newJump.Tv, newJump.Tc).ToString(), pDN);
		
		myData[count++] = newJump.Description;

		myData[count++] = newJump.UniqueID.ToString();
		return myData;
	}

}

public class TreeViewJumpsRj : TreeViewJumps
{
	public TreeViewJumpsRj (Gtk.TreeView treeview, bool showHeight, bool showInitialSpeed, bool showQIndex, bool showDjIndex, int newPrefsDigitsNumber, bool weightPercentPreferred, bool metersSecondsPreferred, ExpandStates expandState)
	{
		this.treeview = treeview;
		this.showHeight = showHeight;
		this.showInitialSpeed = showInitialSpeed;
		this.showQIndex = showQIndex;
		this.showDjIndex = showDjIndex;
		pDN = newPrefsDigitsNumber;
		this.weightPercentPreferred = weightPercentPreferred;
		this.metersSecondsPreferred = metersSecondsPreferred;
		this.expandState = expandState;

		treeviewHasTwoLevels = true;
		dataLineNamePosition = 0; //position of name in the data to be printed
		dataLineTypePosition = 4; //position of type in the data to be printed
		allEventsName = Constants.AllJumpsName;
			
		if(weightPercentPreferred)
			weightName += "\n(%)";
		else
			weightName += "\n(Kg)";

		string [] columnsStringPre = { jumperName, "TC\n(s)", "TF\n(s)", weightName, fallName };
		columnsString = obtainColumnsString(columnsStringPre);

		eventIDColumn = columnsString.Length ; //column where the uniqueID of event will be (and will be hidded). 
		store = getStore(columnsString.Length +1); //+1 because, eventID is not show in last col
		treeview.Model = store;
		prepareHeaders(columnsString);
	}
	
	protected override System.Object getObjectFromString(string [] myStringOfData) {
		JumpRj myJumpRj = new JumpRj();
		myJumpRj.UniqueID = Convert.ToInt32(myStringOfData[1].ToString()); 
		myJumpRj.Type = myStringOfData[4].ToString();
		myJumpRj.Fall = Convert.ToInt32(myStringOfData[7].ToString());
		myJumpRj.TvString = myStringOfData[12].ToString();
		myJumpRj.TcString = myStringOfData[13].ToString();
		myJumpRj.Limited = myStringOfData[16].ToString();
		myJumpRj.Description = myStringOfData[9].ToString();
		myJumpRj.Simulated = Convert.ToInt32(myStringOfData[18].ToString());
		
		//we create the jump with a weight of percent or kk
		if(weightPercentPreferred)
			myJumpRj.Weight = Convert.ToDouble(myStringOfData[8].ToString());
		else
			myJumpRj.Weight = Util.WeightFromPercentToKg(Convert.ToDouble(myStringOfData[8]), Convert.ToDouble(myStringOfData[19]));

		return myJumpRj;
	}

	protected override string [] getLineToStore(System.Object myObject)
	{
		JumpRj newJumpRj = (JumpRj)myObject;

		string title = newJumpRj.Type;
		if(newJumpRj.Simulated == 1)
			title += " (s) ";

		string myTypeComplet = title + "(" + Util.GetLimitedRounded(newJumpRj.Limited, pDN) + ")";
		
		string [] myData = new String [getColsNum()];
		int count = 0;
		myData[count++] = myTypeComplet;
		myData[count++] = "";
		myData[count++] = "";
		
		myData[count++] = Util.TrimDecimals(newJumpRj.Weight.ToString(), pDN);

		myData[count++] = newJumpRj.Fall.ToString();
		if (showHeight)  
			myData[count++] = "";
		if (showInitialSpeed) 
			myData[count++] = "";
		if(showQIndex)
			myData[count++] = "";
		if(showDjIndex)
			myData[count++] = "";
		
		myData[count++] = newJumpRj.Description;
		myData[count++] = newJumpRj.UniqueID.ToString();
		return myData;
	}
	
	protected override string [] getSubLineToStore(System.Object myObject, int lineCount)
	{
		JumpRj newJumprRj = (JumpRj)myObject;

		//find tv and tc of this lineCount
		string [] myStringTv = newJumprRj.TvString.Split(new char[] {'='});
		string thisTv = myStringTv[lineCount];
		string [] myStringTc = newJumprRj.TcString.Split(new char[] {'='});
		string thisTc = myStringTc[lineCount];

		string [] myData = new String [getColsNum()];
		int count = 0;
		myData[count++] = (lineCount +1).ToString();
		myData[count++] = Util.TrimDecimals( thisTc, pDN );
		myData[count++] = Util.TrimDecimals( thisTv, pDN );
		myData[count++] = ""; 
		myData[count++] = ""; 
		if (showHeight)  
			myData[count++] = Util.TrimDecimals(Util.GetHeightInCentimeters(thisTv), pDN);
		if (showInitialSpeed) 
			myData[count++] = Util.TrimDecimals(Util.GetInitialSpeed(thisTv, metersSecondsPreferred), pDN);
		if(showQIndex)
			myData[count++] = Util.TrimDecimals(
					Util.GetQIndex(Convert.ToDouble(thisTv), Convert.ToDouble(thisTc)).ToString(), 
					pDN);
		if(showDjIndex)
			myData[count++] = Util.TrimDecimals(
					Util.GetDjIndex(Convert.ToDouble(thisTv), Convert.ToDouble(thisTc)).ToString(), 
					pDN);
		
		
		myData[count++] = ""; 
	
		myData[count++] = "-1"; //mark to non select here, select first line 

		return myData;
	}

	protected override string [] printTotal(System.Object myObject, int cols) {
		JumpRj newJumpRj = (JumpRj)myObject;

		string [] myData = new String [getColsNum()];
		int count = 0;
		myData[count++] = Catalog.GetString("Total");
		myData[count++] = Util.TrimDecimals(Util.GetTotalTime(newJumpRj.TcString).ToString(), pDN);
		myData[count++] = Util.TrimDecimals(Util.GetTotalTime(newJumpRj.TvString).ToString(), pDN);
		myData[count++] = ""; //weight
		myData[count++] = ""; //fall
		if (showHeight)  
			myData[count++] = ""; 
		if (showInitialSpeed) 
			myData[count++] = ""; 
		if (showQIndex || showDjIndex) 
			myData[count++] = ""; 

		myData[count++] = ""; 

		myData[count++] = "-1"; //mark to non select here, select first line 
		
		return myData;
	}
	
	protected override string [] printAVG(System.Object myObject, int cols) {
		JumpRj newJumpRj = (JumpRj)myObject;

		//littleOptimization
		double tcAVGDouble = Util.GetAverage(newJumpRj.TcString);
		double tvAVGDouble = Util.GetAverage(newJumpRj.TvString);

		string [] myData = new String [getColsNum()];
		int count = 0;
		myData[count++] = Catalog.GetString("AVG");
		myData[count++] = Util.TrimDecimals(tcAVGDouble.ToString(), pDN);
		myData[count++] = Util.TrimDecimals(tvAVGDouble.ToString(), pDN);
		myData[count++] = ""; //weight
		myData[count++] = ""; //fall

		//this values are calculated using the AVG of the tcs or tvs, not as an avg of individual values

		if (showHeight)  
			myData[count++] = Util.TrimDecimals(
					Util.GetHeightInCentimeters(
						tvAVGDouble.ToString())
					, pDN);
		if (showInitialSpeed) 
			myData[count++] = Util.TrimDecimals(
					Util.GetInitialSpeed(
						tvAVGDouble.ToString(), metersSecondsPreferred)
					, pDN);
		if (showQIndex) 
			myData[count++] = Util.TrimDecimals(
					Util.GetQIndex(tvAVGDouble,tcAVGDouble).ToString(), pDN);
		else if (showDjIndex) 
			myData[count++] = Util.TrimDecimals(
					Util.GetDjIndex(tvAVGDouble,tcAVGDouble).ToString(), pDN);

		myData[count++] = ""; 
		
		myData[count++] = "-1"; //mark to non select here, select first line 
		
		return myData;
	}
	
	protected override string [] printSD(System.Object myObject, int cols) {
		JumpRj newJumpRj = (JumpRj)myObject;

		string [] myData = new String [getColsNum()];
		int count = 0;
		myData[count++] = Catalog.GetString("SD");
		myData[count++] = Util.TrimDecimals(Util.CalculateSD(
			Util.ChangeEqualForColon(newJumpRj.TcString),
			Util.GetTotalTime(newJumpRj.TcString),
			Util.GetNumberOfJumps(newJumpRj.TcString, false)).ToString(),
				pDN);
		myData[count++] = Util.TrimDecimals(Util.CalculateSD(
			Util.ChangeEqualForColon(newJumpRj.TvString),
			Util.GetTotalTime(newJumpRj.TvString),
			Util.GetNumberOfJumps(newJumpRj.TvString, false)).ToString(),
				pDN);
		
		
		myData[count++] = ""; //weight
		myData[count++] = ""; //fall

		if (showHeight)  
			myData[count++] = "";
		if (showInitialSpeed) 
			myData[count++] = "";
		if (showQIndex || showDjIndex) 
			myData[count++] = "";

		myData[count++] = ""; 
		
		myData[count++] = "-1"; //mark to non select here, select first line 
		
		return myData;
	}
	
	
	protected override int getNumOfSubEvents(System.Object myObject)
	{
		JumpRj newJumpRj = (JumpRj)myObject;

		string [] myStringFull = newJumpRj.TvString.Split(new char[] {'='});

		return myStringFull.Length; 
	} 
			
	public override ExpandStates ZoomChange(ExpandStates myExpand) {
		if(myExpand == ExpandStates.MINIMIZED)
			return ExpandStates.OPTIMAL;
		else if(myExpand == ExpandStates.OPTIMAL)
			return ExpandStates.MAXIMIZED;
		else
			return ExpandStates.MINIMIZED;
	}

}
