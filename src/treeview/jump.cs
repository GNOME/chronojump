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
 *  Copyright (C) 2004-2022   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Data;
using Gtk;
using System.Collections; //ArrayList
using Mono.Unix;


public class TreeViewJumps : TreeViewEvent
{
	protected string jumperName = Catalog.GetString("Jumper");
	protected string weightName = Catalog.GetString("Extra weight");
	protected string fallName = Catalog.GetString("Fall") + "\n(cm)";
	protected string heightName = Catalog.GetString("Height") + "\n(cm)";
	protected string powerName = Catalog.GetString("Power") + "\n(W)";
	protected string stiffnessName = Catalog.GetString("Stiffness") + "\n(N/m)";
	protected string initialSpeedName = Catalog.GetString("Initial Speed");
	protected string rsiName = "RSI" + "\n(m/s)";
	protected string angleName = Catalog.GetString("Angle");
	protected string datetimeName = Catalog.GetString("Date");

	//one of both indexes can be shown if selected on preferences
	protected string qIndexName = "Q Index" + "\n(%)";
	protected string djIndexName = "Dj Index" + "\n(%)";
	
	//to calculate potency
	protected double personWeight;
	protected double weightInKg;

	public TreeViewJumps ()
	{
	}
	
	public TreeViewJumps (Gtk.TreeView treeview, Preferences preferences, ExpandStates expandState)
	{
		this.treeview = treeview;
		this.preferences = preferences;
		this.expandState = expandState;
		
		this.pDN = preferences.digitsNumber; //pDN short and very used name
		
		treeviewHasTwoLevels = false;
		dataLineNamePosition = 0; //position of name in the data to be printed
		dataLineTypePosition = 4; //position of type in the data to be printed
		allEventsName = Constants.AllJumpsNameStr();
		
		if(preferences.weightStatsPercent)
			weightName += "\n(%)";
		else
			weightName += "\n(Kg)";

		string [] columnsStringPre = { jumperName, 
			Catalog.GetString("TC") + "\n(s)", 
			Catalog.GetString("TF") + "\n(s)", 
			weightName, fallName,
			heightName
	       	};

		columnsString = obtainColumnsString(columnsStringPre);
	

		eventIDColumn = columnsString.Length ; //column where the uniqueID of event will be (and will be hidden). 
		store = getStore(columnsString.Length +1); //+1 because, eventID is not show in last col
		treeview.Model = store;
		prepareHeaders(columnsString);
		
		//on creation, treeview is minimized
		expandState = ExpandStates.MINIMIZED;
	}

	protected override int getColsNum() {
		int i = columnsString.Length;
		
		if (preferences.showPower)  
			i ++;
		if (preferences.showStiffness)  
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
	}
	
	protected string [] obtainColumnsString(string [] columnsStringPre) 
	{
		//check long of new array
		int i = columnsStringPre.Length + 3; //columnsStringPre + dateTime + video + description
		
		if (preferences.showPower)  
			i ++;
		if (preferences.showStiffness)  
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
			columnsString[i++] = powerName;
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
		myData[count++] = Util.TrimDecimals(newJump.Tv.ToString(), pDN);
		
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
		myData[count++] = Util.TrimDecimals(Util.GetHeightInCentimeters(newJump.Tv.ToString()), pDN);

		

		if (preferences.showPower)  {
			//takeoff has no tv. power should not be calculated
			//calculate jumps with tf
			if(newJump.Tv > 0) {	
				if(newJump.Tc > 0) 	//if it's Dj (has tf, and tc)
					myData[count++] = Util.TrimDecimals(
							Jump.GetDjPower (newJump.Tc, newJump.Tv, (personWeight + weightInKg), newJump.Fall).ToString(), 1);
				else {			//it's a simple jump without tc
					myData[count++] = Util.TrimDecimals(
							Jump.GetPower (newJump.Tv, personWeight, weightInKg).ToString(), 1);
				}
			} else
				myData[count++] = "0";
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

		if (Util.StartsWithInListString (videos_l, string.Format ("{0}-{1}", Constants.TestTypes.JUMP, newJump.UniqueID)))
			myData[count++] = Catalog.GetString ("Yes");
		else
			myData[count++] = Catalog.GetString ("No");
		LogB.Information ("find video: " + string.Format ("{0}-{1}", Constants.TestTypes.JUMP, newJump.UniqueID));
		LogB.Information ("on list:");
		LogB.Information (Util.ListStringToString (videos_l));

		myData[count++] = newJump.Description;
		myData[count++] = newJump.UniqueID.ToString();
		return myData;
	}
	
	//used on Add
	public double PersonWeight {
		set {
			personWeight = value;
		}
	}

}

public class TreeViewJumpsRj : TreeViewJumps
{
	public TreeViewJumpsRj (Gtk.TreeView treeview, Preferences preferences, ExpandStates expandState)
	{
		this.treeview = treeview;
		this.preferences = preferences;
		this.expandState = expandState;
		
		this.pDN = preferences.digitsNumber; //pDN short and very used name
		
		treeviewHasTwoLevels = true;
		dataLineNamePosition = 0; //position of name in the data to be printed
		dataLineTypePosition = 4; //position of type in the data to be printed
		allEventsName = Constants.AllJumpsNameStr();
			
		if(preferences.weightStatsPercent)
			weightName += "\n(%)";
		else
			weightName += "\n(Kg)";

		string [] columnsStringPre = { jumperName, 
			Catalog.GetString("TC") + "\n(s)", 
			Catalog.GetString("TF") + "\n(s)", 
			weightName, fallName,
			heightName
	       	};
		columnsString = obtainColumnsString(columnsStringPre);

		eventIDColumn = columnsString.Length ; //column where the uniqueID of event will be (and will be hidden). 
		store = getStore(columnsString.Length +1); //+1 because, eventID is not show in last col
		treeview.Model = store;
		prepareHeaders(columnsString);
	}
	
	protected override System.Object getObjectFromString(string [] myStringOfData) {
		JumpRj myJumpRj = new JumpRj();
		myJumpRj.UniqueID = Convert.ToInt32(myStringOfData[1].ToString()); 
		myJumpRj.Type = myStringOfData[4].ToString();
		myJumpRj.Fall = Convert.ToDouble(myStringOfData[7].ToString());
		myJumpRj.TvString = myStringOfData[12].ToString();
		myJumpRj.TcString = myStringOfData[13].ToString();
		myJumpRj.Limited = myStringOfData[16].ToString();
		myJumpRj.Description = myStringOfData[9].ToString();
		myJumpRj.Simulated = Convert.ToInt32(myStringOfData[18].ToString());
		myJumpRj.Datetime = myStringOfData[19].ToString();
		
		myJumpRj.WeightPercent = Convert.ToDouble (myStringOfData[8].ToString());

		personWeight = Convert.ToDouble(myStringOfData[20]);
		weightInKg = Util.WeightFromPercentToKg (myJumpRj.WeightPercent, personWeight);

		return myJumpRj;
	}

	protected override string [] getLineToStore(System.Object myObject)
	{
		JumpRj newJumpRj = (JumpRj)myObject;

		string title = Catalog.GetString(newJumpRj.Type);
		if(newJumpRj.Simulated == Constants.Simulated)
			title += Constants.SimulatedTreeviewStr() + " ";

		string myTypeComplet = "";
		if(newJumpRj.Type == Constants.RunAnalysisName) 
			myTypeComplet = title + "(" + newJumpRj.Fall + " cm)"; //distance is recorded as fall in RunAnalysis
		else
			myTypeComplet = title + "(" + Util.GetLimitedRounded(newJumpRj.Limited, pDN) + ")";
		
		string [] myData = new String [getColsNum()];
		int count = 0;
		myData[count++] = myTypeComplet;
		myData[count++] = "";
		myData[count++] = "";
		
		weightInKg = Util.WeightFromPercentToKg(
				Convert.ToDouble (newJumpRj.WeightPercent.ToString()),
				personWeight);
		myData[count++] = Util.TrimDecimals(weightInKg.ToString(), pDN);

		myData[count++] = Util.TrimDecimals(newJumpRj.Fall.ToString(), pDN);
		myData[count++] = ""; //height
		if (preferences.showPower)
			myData[count++] = "";
		if (preferences.showStiffness)
			myData[count++] = "";
		if (preferences.showInitialSpeed) 
			myData[count++] = "";
		if (preferences.showJumpRSI)
			myData[count++] = "";
		if (preferences.showQIndex)
			myData[count++] = "";
		if (preferences.showDjIndex)
			myData[count++] = "";
		
		myData[count++] = UtilDate.GetDatetimePrint(UtilDate.FromFile(newJumpRj.Datetime));

		if (Util.StartsWithInListString (videos_l, string.Format ("{0}-{1}", Constants.TestTypes.JUMP_RJ, newJumpRj.UniqueID)))
			myData[count++] = Catalog.GetString ("Yes");
		else
			myData[count++] = Catalog.GetString ("No");

		myData[count++] = newJumpRj.Description;
		myData[count++] = newJumpRj.UniqueID.ToString();
		return myData;
	}
	
	protected override string [] getSubLineToStore(System.Object myObject, int lineCount)
	{
		JumpRj newJumpRj = (JumpRj)myObject;

		//find tv and tc of this lineCount
		string [] myStringTv = newJumpRj.TvString.Split(new char[] {'='});
		string thisTv = myStringTv[lineCount];
		double thisTvD = Convert.ToDouble(thisTv);

		string [] myStringTc = newJumpRj.TcString.Split(new char[] {'='});
		string thisTc = myStringTc[lineCount];
		double thisTcD = Convert.ToDouble(thisTc);

		string [] myData = new String [getColsNum()];
		int count = 0;

		if(newJumpRj.Type == Constants.RunAnalysisName) {
			if(lineCount == 0)
				myData[count++] = Catalog.GetString("First photocell");
			else
				myData[count++] = (lineCount).ToString();
		}
		else
			myData[count++] = (lineCount +1).ToString();

		myData[count++] = Util.TrimDecimals( thisTc, pDN );
		myData[count++] = Util.TrimDecimals( thisTv, pDN );
		myData[count++] = ""; 
		myData[count++] = ""; 
		myData[count++] = Util.TrimDecimals(Util.GetHeightInCentimeters(thisTv), pDN);
		
		//This is needed on Add (where personWeight is passed using PersonWeight, but not weightInKg)
		weightInKg = Util.WeightFromPercentToKg(
				Convert.ToDouble (newJumpRj.WeightPercent.ToString()),
				personWeight);
		
		if (preferences.showPower) {
			double myFall;
			if(lineCount == 0)
				myFall = newJumpRj.Fall;
			else
				myFall = Convert.ToDouble(Util.GetHeightInCentimeters(myStringTv[lineCount -1]));
			
			if(Convert.ToDouble(thisTc) > 0)
				myData[count++] = Util.TrimDecimals(
						Jump.GetDjPower (thisTcD, thisTvD,
							(personWeight + weightInKg), myFall).ToString(), 1);
			else
				myData[count++] = Util.TrimDecimals(
						Jump.GetPower (thisTvD, personWeight, weightInKg).ToString(), 1);
		}
		if (preferences.showStiffness) {
			//use directly Util.GetStiffness because we want to get from this specific subjump, not all the reactive jump.
			if(thisTcD > 0) {
				myData[count++] = Util.TrimDecimals(
						Util.GetStiffness(personWeight, weightInKg, thisTvD, thisTcD)
						.ToString(), 1);
			}
			else
				myData[count++] = ""; 
		}
		if (preferences.showInitialSpeed) 
			myData[count++] = Util.TrimDecimals(Jump.GetInitialSpeed(
						thisTv, preferences.metersSecondsPreferred), pDN);
		if (preferences.showJumpRSI)
			myData[count++] = Util.TrimDecimals(
					UtilAll.DivideSafe(Util.GetHeightInMeters(thisTvD), thisTcD),
					pDN);
		if (preferences.showQIndex)
			myData[count++] = Util.TrimDecimals(
					Util.GetQIndex(thisTvD, thisTcD).ToString(),
					pDN);
		if (preferences.showDjIndex)
			myData[count++] = Util.TrimDecimals(
					Util.GetDjIndex(thisTvD, thisTcD).ToString(),
					pDN);
		
		
		myData[count++] = ""; 	//datetime
		myData[count++] = "";	//video
		myData[count++] = ""; 	//description
	
		myData[count++] = "-1"; //mark to non select here, select first line 

		return myData;
	}

	protected override string [] printTotal (System.Object myObject)
	{
		JumpRj newJumpRj = (JumpRj)myObject;

		string [] myData = new String [getColsNum()];
		int count = 0;
		myData[count++] = Catalog.GetString("Total");
		myData[count++] = Util.TrimDecimals(Util.GetTotalTime(newJumpRj.TcString).ToString(), pDN);
		myData[count++] = Util.TrimDecimals(Util.GetTotalTime(newJumpRj.TvString).ToString(), pDN);
		myData[count++] = ""; //weight
		myData[count++] = ""; //fall
		myData[count++] = ""; //height
		if (preferences.showPower)
			myData[count++] = "";
		if (preferences.showStiffness)
			myData[count++] = "";
		if (preferences.showInitialSpeed) 
			myData[count++] = ""; 
		if (preferences.showJumpRSI)
			myData[count++] = "";
		if (preferences.showQIndex || preferences.showDjIndex) 
			myData[count++] = ""; 

		myData[count++] = ""; 	//datetime
		myData[count++] = "";	//video
		myData[count++] = ""; 	//description

		myData[count++] = "-1"; //mark to non select here, select first line 
		
		return myData;
	}
	
	protected override string [] printAVG (System.Object myObject)
	{
		JumpRj newJumpRj = (JumpRj)myObject;

		string tcString = newJumpRj.TcString;
		string tvString = newJumpRj.TvString;

		if(newJumpRj.Type == Constants.RunAnalysisName) {
			tcString = Util.DeleteFirstSubEvent(tcString);
			tvString = Util.DeleteFirstSubEvent(tvString);
		}

		double tcAVGDouble = Util.GetAverage(tcString);
		double tvAVGDouble = Util.GetAverage(tvString);

		string [] myData = new String [getColsNum()];
		int count = 0;
		if(newJumpRj.Type == Constants.RunAnalysisName) 
			myData[count++] = Catalog.GetString("AVG") + " (" + Catalog.GetString("photocells not included") + ")";
		else
			myData[count++] = Catalog.GetString("AVG");

		myData[count++] = Util.TrimDecimals(tcAVGDouble.ToString(), pDN);
		myData[count++] = Util.TrimDecimals(tvAVGDouble.ToString(), pDN);
		myData[count++] = ""; //weight
		myData[count++] = ""; //fall

		//this values are calculated using the AVG of the tcs or tvs, not as an avg of individual values

		myData[count++] = Util.TrimDecimals(
				Util.GetHeightInCentimeters(
					tvAVGDouble.ToString())
				, pDN);

		if (preferences.showPower || preferences.showStiffness)
		{
			/*
			 * it can be done using AVG values like the other AVG statistics,
			 * but result will not be the same than making the avg of the different power values for each row
			 * for this reason is best to first calculate the different values of each column and store separately
			 * in order to calculate the total, AVG, SD using that data
			 */

			weightInKg = Util.WeightFromPercentToKg (
					Convert.ToDouble (newJumpRj.WeightPercent.ToString ()),
					personWeight);

			string [] tc_array = newJumpRj.TcString.Split(new char[] {'='});
			string [] tv_array = newJumpRj.TvString.Split(new char[] {'='});
			//TODO: store this list outside because this method to be used on MAX, AVG, SD
			double powerSum = 0;
			double stiffnessSum = 0;
			int powerCount = 0;
			int stiffnessCount = 0;
			for(int i = 0; i < tc_array.Length; i ++)
			{
				double tc = Convert.ToDouble(tc_array[i]);
				double tv = Convert.ToDouble(tv_array[i]);
				double fall = 0;
				if(tc_array[i] == "-1") //startIn at first jump tc is 0, better check like this (string)
					powerSum += Jump.GetPower (tv, personWeight, weightInKg);
				else {
					if(i == 0)
						fall = newJumpRj.Fall;
					else
						fall = Util.GetHeightInCentimeters(Convert.ToDouble(tv_array[i-1]));

					powerSum += Jump.GetDjPower (tc, tv,
							(personWeight + weightInKg), fall);

					/* debug
					LogB.Information (string.Format (
								"at treeviewJump, tc: {0}, tv: {1}, (personWeight + weightInKg): {2}, fall: {3}, powerSum: {4}",
							tc, tv, (personWeight + weightInKg), fall, powerSum));
							*/

					stiffnessSum += Util.GetStiffness(personWeight, weightInKg, tv, tc);
					stiffnessCount ++;
				}
				//LogB.Information ("at treeviewJump, powerSum = ", powerSum.ToString());
				powerCount ++;
			}
			if (preferences.showPower)
				myData[count++] = Util.TrimDecimals(UtilAll.DivideSafe(powerSum, powerCount), 1);
			if (preferences.showStiffness)
				myData[count++] = Util.TrimDecimals(UtilAll.DivideSafe(stiffnessSum, stiffnessCount), 1);
		}
		if (preferences.showInitialSpeed) 
			myData[count++] = Util.TrimDecimals(
					Jump.GetInitialSpeed(
						tvAVGDouble.ToString(), preferences.metersSecondsPreferred)
					, pDN);
		if (preferences.showJumpRSI)
			myData[count++] = Util.TrimDecimals(Util.GetAverage(newJumpRj.RSIList), pDN);
		if (preferences.showQIndex) 
			myData[count++] = Util.TrimDecimals(
					Util.GetQIndex(tvAVGDouble,tcAVGDouble).ToString(), pDN);
		else if (preferences.showDjIndex) 
			myData[count++] = Util.TrimDecimals(
					Util.GetDjIndex(tvAVGDouble,tcAVGDouble).ToString(), pDN);

		myData[count++] = ""; 	//datetime
		myData[count++] = "";	//video
		myData[count++] = ""; 	//description
		
		myData[count++] = "-1"; //mark to non select here, select first line 
		
		return myData;
	}
	
	protected override string [] printSD (System.Object myObject)
	{
		JumpRj newJumpRj = (JumpRj)myObject;
		
		string tcString = newJumpRj.TcString;
		string tvString = newJumpRj.TvString;

		if(newJumpRj.Type == Constants.RunAnalysisName) {
			tcString = Util.DeleteFirstSubEvent(tcString);
			tvString = Util.DeleteFirstSubEvent(tvString);
		}

		string [] myData = new String [getColsNum()];
		int count = 0;
		if(newJumpRj.Type == Constants.RunAnalysisName) 
			myData[count++] = Catalog.GetString("SD") + " (" + Catalog.GetString("photocells not included") + ")";
		else
			myData[count++] = Catalog.GetString("SD");

		myData[count++] = Util.TrimDecimals(Util.CalculateSD(
			Util.ChangeEqualForColon(tcString),
			Util.GetTotalTime(tcString),
			Util.GetNumberOfJumps(tcString, false)).ToString(),
				pDN);
		myData[count++] = Util.TrimDecimals(Util.CalculateSD(
			Util.ChangeEqualForColon(tvString),
			Util.GetTotalTime(tvString),
			Util.GetNumberOfJumps(tvString, false)).ToString(),
				pDN);
		
		
		myData[count++] = ""; //weight
		myData[count++] = ""; //fall
		myData[count++] = ""; //height
		if (preferences.showPower)
			myData[count++] = "";
		if (preferences.showStiffness)
			myData[count++] = "";
		if (preferences.showJumpRSI)
			myData[count++] = "";
		if (preferences.showInitialSpeed) 
			myData[count++] = "";
		if (preferences.showQIndex || preferences.showDjIndex) 
			myData[count++] = "";

		myData[count++] = ""; 	//datetime
		myData[count++] = "";	//video
		myData[count++] = ""; 	//description
		
		myData[count++] = "-1"; //mark to non select here, select first line 
		
		return myData;
	}
	
	
	protected override int getNumOfSubEvents(System.Object myObject)
	{
		JumpRj newJumpRj = (JumpRj)myObject;

		string [] myStringFull = newJumpRj.TvString.Split(new char[] {'='});

		return myStringFull.Length; 
	} 
			
}
