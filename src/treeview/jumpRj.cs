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

public class TreeViewJumpsRj : TreeViewJumps
{
	public TreeViewJumpsRj (Gtk.TreeView treeview, Preferences preferences, ExpandStates expandState, bool barsAreDistance)
	{
		this.treeview = treeview;
		this.preferences = preferences;
		this.expandState = expandState;
		this.barsAreDistance = barsAreDistance;
		
		this.pDN = preferences.digitsNumber; //pDN short and very used name
		
		treeviewHasTwoLevels = true;
		dataLineNamePosition = 0; //position of name in the data to be printed
		dataLineTypePosition = 4; //position of type in the data to be printed
		allEventsName = Constants.AllJumpsNameStr();
		showPowerFormula = false; //it seems it is always used Chronojump formula (TC + TF)
			
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

		boldableColumns_l = new List<int> { 1, 2, 5 }; //tc, tf, height
		idColumn = columnsString.Length ; //column where the uniqueID of event will be (and will be hidden). 
		//idColumn = columnsString.Length -1; //column where the uniqueID of event will be (and will be hidden).  (with the ID)

		store = getStore(columnsString.Length +1); //+1 because, eventID is not show in last col
//store = getStore(columnsString.Length); //+1 because, eventID is not show in last col
		treeview.Model = store;
		prepareHeaders(columnsString);
	}

	protected override bool shouldRenderBoldable (string columnTitle)
	{
		if (barsAreDistance && columnTitle.StartsWith (Catalog.GetString ("Height")))
			return true;

		if (! barsAreDistance && (
					columnTitle.StartsWith (Catalog.GetString ("TC")) ||
					columnTitle.StartsWith (Catalog.GetString ("TF"))
					))
			return true;

		return false;
	}

	protected override System.Object getObjectFromString(string [] myStringOfData)
	{
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

		personWeight = Convert.ToDouble(myStringOfData[21]);
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

		if (UtilList.StartsWithInListString (videos_l, string.Format ("{0}-{1}", Constants.TestTypes.JUMP_RJ, newJumpRj.UniqueID)))
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

			// TODO: check if this is needed, as always it will use GetDjPower
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
	
		myData[count++] = MarkNonSelectRowSubEvent.ToString (); //mark to non select here, select first line

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

		myData[count++] = MarkNonSelectRowSubEvent.ToString (); //mark to non select here, select first line
		
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

		myData[count++] = boldMark + Util.TrimDecimals(tcAVGDouble.ToString(), pDN);
		myData[count++] = boldMark + Util.TrimDecimals(tvAVGDouble.ToString(), pDN);
		myData[count++] = ""; //weight
		myData[count++] = ""; //fall

		//this values are calculated using the AVG of the tcs or tvs, not as an avg of individual values

		myData[count++] = boldMark + Util.TrimDecimals (
				UtilList.GetAverage (newJumpRj.HeightList)
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

				// TODO: check if this is needed, as always it will use GetDjPower
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
			myData[count++] = Util.TrimDecimals(UtilList.GetAverage(newJumpRj.RSIList), pDN);
		if (preferences.showQIndex) 
			myData[count++] = Util.TrimDecimals(
					Util.GetQIndex(tvAVGDouble,tcAVGDouble).ToString(), pDN);
		else if (preferences.showDjIndex) 
			myData[count++] = Util.TrimDecimals(
					Util.GetDjIndex(tvAVGDouble,tcAVGDouble).ToString(), pDN);

		myData[count++] = ""; 	//datetime
		myData[count++] = "";	//video
		myData[count++] = ""; 	//description
		
		myData[count++] = MarkNonSelectRowSubEvent.ToString (); //mark to non select here, select first line
		
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
		
		myData[count++] = MarkNonSelectRowSubEvent.ToString (); //mark to non select here, select first line
		
		return myData;
	}
	
	
	protected override int getNumOfSubEvents(System.Object myObject)
	{
		JumpRj newJumpRj = (JumpRj)myObject;

		string [] myStringFull = newJumpRj.TvString.Split(new char[] {'='});

		return myStringFull.Length; 
	} 
}
