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
 * Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using Gtk;
using System.Collections; //ArrayList
using Mono.Unix;


public class StatRjAVGSD : Stat
{
	protected string indexType;

	protected string [] columnsString = { 
		Catalog.GetString("Jumper"), 
		Catalog.GetString("AVG"), 
		Catalog.GetString("SD"),
		Catalog.GetString("Jumps") };
	
	//if this is not present i have problems like (No overload for method `xxx' takes `0' arguments) with some inherited classes
	public StatRjAVGSD () 
	{
		this.showSex = false;
		this.statsJumpsType = 0;
		this.limit = 0;
	}

	public StatRjAVGSD (StatTypeStruct myStatTypeStruct, Gtk.TreeView treeview, string indexType) 
	{
		completeConstruction (myStatTypeStruct, treeview);

		this.indexType = indexType;
		
		this.dataColumns = 3;
	
		/*
		 * only simplesession, because it has to plot two values: AVG, and SD	
		if(sessions.Count > 1) {
			store = getStore(sessions.Count +3); //+3 (for jumper, the AVG horizontal and SD horizontal)
		} else {
		*/
			store = getStore(dataColumns +1); //jumper, AVG, SD, jumps
		//}
		
		if(toReport) {
			reportString = prepareHeadersReport(columnsString);
		} else {
			treeview.Model = store;
			prepareHeaders(columnsString);
		}
	}
	
	public override void PrepareData() 
	{
		string sessionString = obtainSessionSqlString(sessions, "jumpRj");
		bool multisession = false;
		/*
		if(sessions.Count > 1) {
			multisession = true;
		}
		*/

		string operation = ""; // cannot be avg in this statistic
		int maxJumps = SqliteStat.ObtainMaxNumberOfJumps(sessionString);
		
		processDataSimpleSession ( 
				cleanDontWanted (
					convertTCsTFsCombinedToIndexAVGSD (
						SqliteStat.RjEvolution(sessionString, multisession, 
							operation, jumpType, showSex, maxJumps)), 
						statsJumpsType, limit),
				false, dataColumns);
	}
		
	/* 
	 * data come from RjEvolution like this:
			myArray.Add (reader[0].ToString() + showSexString + showJumpTypeString +
					returnSessionString + ":" + 		//session
					Util.ChangeDecimalSeparator(reader[3].ToString()) +			//index
					returnFallString + 			//fall
					allTCsTFsCombined			//tc:tv:tc:tv...
				    );
	 * let's chage the allTCsTFsCombined to:
	 * AVG(index), SD (index)
	 * being index djIndex or Qindex
	 */
	private ArrayList convertTCsTFsCombinedToIndexAVGSD (ArrayList myDataCombined) {
		int firstTCPos = 3;

		ArrayList arrayConverted = new ArrayList();

		string valuesListForAVG; //separated by '='
		string valuesListForSD; //separated by ':'
		double sumValues;
		int count;

		foreach (string row in myDataCombined) {
			string [] strFull = row.Split(new char[] {':'});

			valuesListForAVG = "";
			valuesListForSD = "";
			sumValues = 0;
			count = 0;
			
			string sepAVG = "";
			string sepSD = "";

			/*
			 * from the first TC in this serie TC:TF:TC:TF:...
			 * to the end of string
			 * calculate the index of every tc-tf pair
			 * if both values are different than "-"
			 * and prepare data for AVG and SD of index
			 */
			int numberOfJumps = 0;
			for (int i=firstTCPos; i+1 < strFull.Length; i+=2) {
				if(strFull[i] != "-" && strFull[i+1] != "-") {
					double myIndex = getIndex(
							Convert.ToDouble(strFull[i+1]), 
							Convert.ToDouble(strFull[i]));

					valuesListForAVG += sepAVG + myIndex.ToString();
					valuesListForSD += sepSD + myIndex.ToString();
					sumValues += myIndex;
					count ++;

					sepAVG = "=";
					sepSD = ":";

					numberOfJumps ++;
				}
			}

			double avg = Util.GetAverage(valuesListForAVG);
			double sd = Util.CalculateSD(valuesListForSD, sumValues, count);

			string rowConverted = 
				strFull[0] + ":" + 
				avg.ToString() + ":" + sd.ToString() + ":" + 
				numberOfJumps.ToString(); 

			arrayConverted.Add(rowConverted); 
		}

		return arrayConverted;
	}

	private double getIndex(double tf, double tc) {
		if(indexType == Constants.RjIndexName)
			return Util.GetDjIndex(tf, tc);
		else
			return Util.GetQIndex(tf, tc);
	}

	public override string ToString () 
	{
		string selectedValuesString = "";
		if(statsJumpsType == 0) { //all jumps
			selectedValuesString = allValuesString; 
		} else if(statsJumpsType == 1) { //limit
			selectedValuesString = string.Format(Catalog.GetPluralString(
						"First value", "First {0} values", limit), limit);
		} else if(statsJumpsType == 2) { //best of each jumper
			selectedValuesString = string.Format(Catalog.GetPluralString(
						"Max value of each person", "Max {0} values of each person", limit), limit);
		}
		/* this option is not possible in this statistic
		 * 
		} else if(statsJumpsType == 3) { //avg of each jumper
			selectedValuesString = avgValuesString; 
		}  
		*/

		string [] strFull = sessions[0].ToString().Split(new char[] {':'});
		string mySessionString =  Catalog.GetString (" session ") + 
			strFull[0] + "(" + strFull[2] + ")";

		string myFormula = "";
		if(indexType == Constants.RjIndexName)
			myFormula = Constants.RJAVGSDRjIndexName + " " + Constants.RjIndexOnlyFormula;
		else
			myFormula = Constants.RJAVGSDQIndexName + " " + Constants.QIndexOnlyFormula;

		return string.Format(Catalog.GetString("{0} at average of jumps using {1} applied to {2} on {3}"), 
				selectedValuesString, myFormula, jumpType, mySessionString);
	}
}


