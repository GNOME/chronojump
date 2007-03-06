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
	protected bool showQIndex;
	protected bool showDjIndex;
	
	protected string jumperName = Catalog.GetString("Jumper");
	protected string weightName = Catalog.GetString("Weight") + "\n(%)";
	protected string fallName = Catalog.GetString("Fall") + "\n(cm)";
	protected string heightName = Catalog.GetString("Height") + "\n(cm)";
	protected string initialSpeedName = Catalog.GetString("Initial Speed");

	//one of both indexes can be shown if selected on preferences
	protected string qIndexName = "Q Index" + "\n(%)";
	protected string djIndexName = "Dj Index" + "\n(%)";
	
	protected bool metersSecondsPreferred;
		
	public TreeViewJumps ()
	{
	}
	
	public TreeViewJumps (Gtk.TreeView treeview, bool showHeight, bool showInitialSpeed, 
			bool showQIndex, bool showDjIndex, int newPrefsDigitsNumber, bool metersSecondsPreferred)
	{
		this.treeview = treeview;
		this.showHeight = showHeight;
		this.showInitialSpeed = showInitialSpeed;
		this.showQIndex = showQIndex;
		this.showDjIndex = showDjIndex;
		pDN = newPrefsDigitsNumber;
		this.metersSecondsPreferred = metersSecondsPreferred;

		treeviewHasTwoLevels = false;
		dataLineNamePosition = 0; //position of name in the data to be printed
		dataLineTypePosition = 4; //position of type in the data to be printed
		allEventsName = Constants.AllJumpsName;
		
		string [] columnsStringPre = { jumperName, "TC\n(s)", "TF\n(s)", weightName, fallName };
		string [] columnsString = obtainColumnsString(columnsStringPre);
	

		eventIDColumn = columnsString.Length ; //column where the uniqueID of event will be (and will be hidded). 
		store = getStore(columnsString.Length +1); //+1 because, eventID is not show in last col
		treeview.Model = store;
		prepareHeaders(columnsString);
	}

	protected string [] obtainColumnsString(string [] columnsStringPre) {
		//for sure there should be a better way than this
 
		//check long of new array
		int i = 5;
		if (showHeight)  
			i ++;
		if (showInitialSpeed) 
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
		if (showQIndex) 
			columnsString[i++] = qIndexName;
		if (showDjIndex) 
			columnsString[i++] = djIndexName;

		return columnsString;
	}
	
	protected override System.Object getObjectFromString(string [] myStringOfData) {
		Jump myJump = new Jump();
		myJump.UniqueID = Convert.ToInt32(myStringOfData[1].ToString()); 
		myJump.Type = myStringOfData[4].ToString();
		myJump.Tv = Convert.ToDouble(myStringOfData[5].ToString());
		myJump.Tc = Convert.ToDouble(myStringOfData[6].ToString());
		myJump.Fall = Convert.ToInt32(myStringOfData[7].ToString());
		myJump.Weight = Convert.ToDouble(myStringOfData[8].ToString());

		return myJump;
	}

	protected override string [] getLineToStore(System.Object myObject)
	{
		Jump newJump = (Jump)myObject;

		int i = 5;
		if (showHeight)  
			i ++;
		if (showInitialSpeed) 
			i ++;
		if (showQIndex || showDjIndex) 
			i ++;

		string [] myData = new String [i+1]; //columnsString +1
		int count = 0;
		myData[count++] = newJump.Type;
		myData[count++] = Util.TrimDecimals(newJump.Tc.ToString(), pDN);
		myData[count++] = Util.TrimDecimals(newJump.Tv.ToString(), pDN);
		myData[count++] = Util.TrimDecimals(newJump.Weight.ToString(), pDN);
		myData[count++] = newJump.Fall.ToString();
		if (showHeight)  
			myData[count++] = Util.TrimDecimals(Util.GetHeightInCentimeters(newJump.Tv.ToString()), pDN);
		if (showInitialSpeed) 
			myData[count++] = Util.TrimDecimals(Util.GetInitialSpeed(newJump.Tv.ToString(), metersSecondsPreferred), pDN);
		if(showQIndex)
			myData[count++] = Util.TrimDecimals(Util.GetQIndex(newJump.Tv, newJump.Tc).ToString(), pDN);
		if(showDjIndex)
			myData[count++] = Util.TrimDecimals(Util.GetDjIndex(newJump.Tv, newJump.Tc).ToString(), pDN);
		
		myData[count++] = newJump.UniqueID.ToString();
		return myData;
	}
	
}

public class TreeViewJumpsRj : TreeViewJumps
{
	public TreeViewJumpsRj (Gtk.TreeView treeview, bool showHeight, bool showInitialSpeed, bool showQIndex, bool showDjIndex, int newPrefsDigitsNumber, bool metersSecondsPreferred)
	{
		this.treeview = treeview;
		this.showHeight = showHeight;
		this.showInitialSpeed = showInitialSpeed;
		this.showQIndex = showQIndex;
		this.showDjIndex = showDjIndex;
		pDN = newPrefsDigitsNumber;
		this.metersSecondsPreferred = metersSecondsPreferred;

		treeviewHasTwoLevels = true;
		dataLineNamePosition = 0; //position of name in the data to be printed
		dataLineTypePosition = 4; //position of type in the data to be printed
		allEventsName = Constants.AllJumpsName;
			
		string [] columnsStringPre = { jumperName, "TC\n(s)", "TF\n(s)", weightName, fallName };
		string [] columnsString = obtainColumnsString(columnsStringPre);

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
		myJumpRj.Weight = Convert.ToDouble(myStringOfData[8].ToString());
		myJumpRj.TvString = myStringOfData[12].ToString();
		myJumpRj.TcString = myStringOfData[13].ToString();
		myJumpRj.Limited = myStringOfData[16].ToString();

		return myJumpRj;
	}

	protected override string [] getLineToStore(System.Object myObject)
	{
		JumpRj newJumpRj = (JumpRj)myObject;

		//typeComplet
		//do this for showing the Limited with selected decimals and without loosing the end letter: 'J' or 'T'
		string myLimitedWithoutLetter = newJumpRj.Limited.Substring(0, newJumpRj.Limited.Length -1);
		string myLimitedLetter = newJumpRj.Limited.Substring(newJumpRj.Limited.Length -1, 1);
		string myTypeComplet = newJumpRj.Type + "(" + Util.TrimDecimals(myLimitedWithoutLetter, pDN) + myLimitedLetter + ") AVG: ";
		
		int i = 5;
		if (showHeight)  
			i ++;
		if (showInitialSpeed) 
			i ++;
		if (showQIndex || showDjIndex) 
			i ++;

		//little optimization
		double avgTc = Util.GetAverage(newJumpRj.TcString);
		double avgTv = Util.GetAverage(newJumpRj.TvString);
		
		string [] myData = new String [i+1]; //columnsString +1
		int count = 0;
		myData[count++] = myTypeComplet;
		myData[count++] = Util.TrimDecimals(avgTc.ToString(), pDN);
		myData[count++] = Util.TrimDecimals(avgTv.ToString(), pDN);
		myData[count++] = Util.TrimDecimals(newJumpRj.Weight.ToString(), pDN);
		myData[count++] = newJumpRj.Fall.ToString();
		if (showHeight)  
			myData[count++] = Util.TrimDecimals(Util.GetHeightInCentimeters(avgTv.ToString()), pDN);
		if (showInitialSpeed) 
			myData[count++] = Util.TrimDecimals(Util.GetInitialSpeed(avgTv.ToString(), metersSecondsPreferred), pDN);
		if(showQIndex)
			myData[count++] = Util.TrimDecimals(Util.GetQIndex(avgTv, avgTc).ToString(), pDN);
		if(showDjIndex)
			myData[count++] = Util.TrimDecimals(Util.GetDjIndex(avgTv, avgTc).ToString(), pDN);
		
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

		
		//write line for treeview
		int i = 5;
		if (showHeight)  
			i ++;
		if (showInitialSpeed) 
			i ++;
		if (showQIndex || showDjIndex) 
			i ++;
		
		string [] myData = new String [i+1]; //columnsString +1
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
		
		
		myData[count++] = newJumprRj.UniqueID.ToString(); 

		return myData;
	}
	
	protected override int getNumOfSubEvents(System.Object myObject)
	{
		JumpRj newJumpRj = (JumpRj)myObject;

		string [] myStringFull = newJumpRj.TvString.Split(new char[] {'='});

		return myStringFull.Length; 
	} 
			
}
