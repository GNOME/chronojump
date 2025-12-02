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

public class TreeViewJumps : TreeViewEvent
{
	protected string jumperName = Catalog.GetString("Jumper");
	protected string fallName = Catalog.GetString("Fall") + "\n(cm)";
	protected string heightName = Catalog.GetString("Height") + "\n(cm)";
	protected string powerName = Catalog.GetString("Power") + "\n(W)";
	protected string powerFormulaName = Catalog.GetString("Power formula");
	protected string stiffnessName = Catalog.GetString("Stiffness") + "\n(N/m)";
	protected string initialSpeedName = Catalog.GetString("Initial Speed");
	protected string rsiName = "RSI" + "\n(m/s)";
	protected string angleName = Catalog.GetString("Angle");
	protected bool showPowerFormula;

	//one of both indexes can be shown if selected on preferences
	protected string qIndexName = "Q Index" + "\n(%)";
	protected string djIndexName = "Dj Index" + "\n(%)";
	protected bool barsAreDistance;

	public TreeViewJumps ()
	{
	}
	
	public TreeViewJumps (Gtk.TreeView treeview, Preferences preferences, ExpandStates expandState, bool barsAreDistance)
	{
		this.treeview = treeview;
		this.preferences = preferences;
		this.expandState = expandState;
		this.barsAreDistance = barsAreDistance;

		this.pDN = preferences.digitsNumber; //pDN short and very used name
		
		treeviewHasTwoLevels = false;
		dataLineNamePosition = 0; //position of name in the data to be printed
		dataLineTypePosition = 4; //position of type in the data to be printed
		allEventsName = Constants.AllJumpsNameStr();
		showPowerFormula = true;
		
		if(preferences.weightStatsPercent)
			weightExtraName += "\n(%)";
		else
			weightExtraName += "\n(kg)";

		string [] columnsStringPre = { jumperName, 
			Catalog.GetString("TC") + "\n(s)", 
			Catalog.GetString("TF") + "\n(s)", 
			weightExtraName, fallName,
			heightName
	       	};

		columnsString = obtainColumnsString(columnsStringPre);
	
		//columnsString = Util.AddToArrayString (columnsString, new List<string> () {"ID remove this"}); //just for debug

		boldableColumns_l = new List<int> { 2, 5 }; //tf, height
		idColumn = columnsString.Length ; //column where the uniqueID of event will be (and will be hidden). 
		//idColumn = columnsString.Length -1; //column where the uniqueID of event will be (and will be hidden).  (with the ID)

		store = getStore(columnsString.Length +1); //+1 because, eventID is not show in last col
//store = getStore(columnsString.Length); //+1 because, eventID is not show in last col
		treeview.Model = store;
		prepareHeaders(columnsString);
		
		//on creation, treeview is minimized
		expandState = ExpandStates.MINIMIZED;
	}

	//used on jumps, jumpsRj
	protected override int getColsNum() {
		int i = columnsString.Length;
		
		if (preferences.showPower)
		{
			i ++;
			if (showPowerFormula)
				i ++;
		} if (preferences.showStiffness)
			i ++;
		if (preferences.showInitialSpeed) 
			i ++;
		if (preferences.showJumpRSI)
			i ++;
		if (preferences.showAngle) 
			i ++;
		if (preferences.showQIndex || preferences.showDjIndex) 
			i ++;
		return i +1; //+1 is for the uniqueID hidden col (last)
//return i; //+1 is for the uniqueID hidden col (last)
	}
	
	//used on jumps, jumpsRj
	protected string [] obtainColumnsString(string [] columnsStringPre) 
	{
		//check long of new array
		int i = columnsStringPre.Length + 3; //columnsStringPre + dateTime + video + description
		
		if (preferences.showPower)
		{
			i ++;
			if (showPowerFormula)
				i ++;
		} if (preferences.showStiffness)
			i ++;
		if (preferences.showInitialSpeed) 
			i ++;
		if (preferences.showJumpRSI)
			i ++;
		if (preferences.showAngle) 
			i ++;
		if (preferences.showQIndex || preferences.showDjIndex) 
			i ++;

		//create new array
		string [] columnsString = new String[i];
		Array.Copy(columnsStringPre, columnsString, columnsStringPre.Length); //copy columnsStringPre

	
		if(preferences.metersSecondsPreferred)
			initialSpeedName += "\n(m/s)";
		else
			initialSpeedName += "\n(km/h)";


		//fill names
		i = columnsStringPre.Length; //start at end of columnsStringPre
		
		if (preferences.showPower)
		{
			columnsString[i++] = powerName;
			if (showPowerFormula)
				columnsString[i++] = powerFormulaName;
		}
		if (preferences.showStiffness)
			columnsString[i++] = stiffnessName;
		if (preferences.showInitialSpeed) 
			columnsString[i++] = initialSpeedName;
		if (preferences.showJumpRSI)
			columnsString[i++] = rsiName;
		if (preferences.showAngle) 
			columnsString[i++] = angleName;
		if (preferences.showQIndex) 
			columnsString[i++] = qIndexName;
		if (preferences.showDjIndex) 
			columnsString[i++] = djIndexName;
			
		columnsString[i++] = datetimeName;
		columnsString[i++] = videoName;
		columnsString[i++] = descriptionName;

		return columnsString;
	}

	protected override bool shouldRenderBoldable (string columnTitle)
	{
		if (barsAreDistance && columnTitle.StartsWith (Catalog.GetString ("Height")))
			return true;

		if (! barsAreDistance && columnTitle.StartsWith (Catalog.GetString ("TF")))
			return true;

		return false;
	}

	protected override System.Object getObjectFromString(string [] myStringOfData) {
		Jump myJump = new Jump();
		myJump.UniqueID = Convert.ToInt32(myStringOfData[1].ToString()); 
		myJump.Type = myStringOfData[4].ToString();
		myJump.Tv = Convert.ToDouble(myStringOfData[5].ToString());
		myJump.Tc = Convert.ToDouble(myStringOfData[6].ToString());
		myJump.Fall = Convert.ToDouble(myStringOfData[7].ToString());
		myJump.Angle = Convert.ToDouble(myStringOfData[10].ToString());
		myJump.Description = myStringOfData[9].ToString();
		myJump.Simulated = Convert.ToInt32(myStringOfData[11].ToString());
		myJump.Datetime = myStringOfData[12].ToString();

		myJump.WeightPercent = Convert.ToDouble(myStringOfData[8].ToString());

		//to calculate potency
		personWeight = Convert.ToDouble(myStringOfData[13]);
		weightInKg = Util.WeightFromPercentToKg (myJump.WeightPercent, personWeight);

		return myJump;
	}

	protected override string [] getLineToStore(System.Object myObject)
	{
		Jump newJump = (Jump)myObject;

		string title = Catalog.GetString(newJump.Type);
		if(newJump.Simulated == Constants.Simulated)
			title += Constants.SimulatedTreeviewStr();

		string [] myData = new String [getColsNum()];
		int count = 0;
		//myData[count++] = newJump.Type;
		myData[count++] = title;
		myData[count++] = Util.TrimDecimals(newJump.Tc.ToString(), pDN);
		myData[count++] = boldMark + Util.TrimDecimals(newJump.Tv.ToString(), pDN);
		
		//we calculate weightInKg again because can be changed in edit jump, and then treeview is no re-done
		//but we do not calculate again person weight, because if it changes treeview is created again
		//
		//Also this is needed on Add (where personWeight is passed using PersonWeight, but not weightInKg)
		//LogB.Information("getLineToStore personWeight: " + personWeight.ToString());
		weightInKg = Util.WeightFromPercentToKg(
				Convert.ToDouble (newJump.WeightPercent.ToString ()),
				personWeight);
		
		if(preferences.weightStatsPercent)
			myData[count++] = Util.TrimDecimals (newJump.WeightPercent.ToString(), pDN);
		else
			myData[count++] = Util.TrimDecimals (weightInKg.ToString (), pDN);

		myData[count++] = Util.TrimDecimals(newJump.Fall.ToString(), pDN);
		myData[count++] = boldMark + Util.TrimDecimals(Util.GetHeightInCentimeters(newJump.Tv.ToString()), pDN);

		

		if (preferences.showPower)  {
			//takeoff has no tv. power should not be calculated
			//calculate jumps with tf
			if(newJump.Tv > 0) {	
				if(newJump.Tc > 0) {	//if it's Dj (has tf, and tc)
					myData[count++] = Util.TrimDecimals(
							Jump.GetDjPower (newJump.Tc, newJump.Tv, (personWeight + weightInKg), newJump.Fall).ToString(), 1);
					myData[count++] = "Chronojump";
				} else {			//it's a simple jump without tc
					myData[count++] = Util.TrimDecimals(
							Jump.GetPower (newJump.Tv, personWeight, weightInKg).ToString(), 1);
					myData[count++] = "Lewis 1974";
				}
			} else {
				myData[count++] = "0";
				myData[count++] = "";
			}
		}

		if (preferences.showStiffness)
			myData[count++] = Util.TrimDecimals (newJump.Stiffness(personWeight, weightInKg), pDN);
		if (preferences.showInitialSpeed) 
			myData[count++] = Util.TrimDecimals(newJump.GetInitialSpeedJumpSimple (preferences.metersSecondsPreferred), pDN);
		if (preferences.showJumpRSI)
			myData[count++] = Util.TrimDecimals(newJump.RSI, pDN);
		if (preferences.showAngle) 
			myData[count++] = Util.TrimDecimals(newJump.Angle.ToString(), pDN);
		if (preferences.showQIndex)
			myData[count++] = Util.TrimDecimals(Util.GetQIndex(newJump.Tv, newJump.Tc).ToString(), pDN);
		if (preferences.showDjIndex)
			myData[count++] = Util.TrimDecimals(Util.GetDjIndex(newJump.Tv, newJump.Tc).ToString(), pDN);
		
		myData[count++] = UtilDate.GetDatetimePrint(UtilDate.FromFile(newJump.Datetime));

		if (UtilList.StartsWithInListString (videos_l, string.Format ("{0}-{1}", Constants.TestTypes.JUMP, newJump.UniqueID)))
			myData[count++] = Catalog.GetString ("Yes");
		else
			myData[count++] = Catalog.GetString ("No");
		LogB.Information ("find video: " + string.Format ("{0}-{1}", Constants.TestTypes.JUMP, newJump.UniqueID));
		LogB.Information ("on list:");
		LogB.Information (UtilList.ListStringToString (videos_l));

		myData[count++] = newJump.Description;
		myData[count++] = newJump.UniqueID.ToString();
		return myData;
	}

	public bool BarsAreDistance {
		set { barsAreDistance = value; }
	}

}
