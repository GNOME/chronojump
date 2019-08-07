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


//no sj+
public class StatPotency : Stat
{
	//protected bool percent = false; //show weight in simplesession as a percent of body weight or kg
	
	//if this is not present i have problems like (No overload for method `xxx' takes `0' arguments) with some inherited classes
	
	protected string indexType;

	public StatPotency () 
	{
		this.showSex = false;
		this.statsJumpsType = 0;
		this.limit = 0;
	}

	 public StatPotency (StatTypeStruct myStatTypeStruct, Gtk.TreeView treeview, string indexType) 
	{
		this.indexType = indexType;

		completeConstruction (myStatTypeStruct, treeview);
		
		this.dataColumns = 4;	//for simplesession (potency, personweightinkg, extraweightink, height)
		
		if(sessions.Count > 1) {
			store = getStore(sessions.Count +3); //+3 (for jumper, the AVG horizontal and SD horizontal)
		} else {
			store = getStore(dataColumns +1); //jumper + dataColumns (weight cols have characters '%' and 'Kg') solved in sqlite
		}
		

		string [] columnsString = { Catalog.GetString("Jumper"), 
			Catalog.GetString("Peak Power"), 
			Catalog.GetString("Person's Weight"), 
			Catalog.GetString("Extra Weight") + " (Kg)", 
			Catalog.GetString("Height") };
		/*
		if(! percent) {
			columnsString[3] = Catalog.GetString("Weight Kg");
		}
		*/
		
		if(toReport) {
			reportString = prepareHeadersReport(columnsString);
		} else {
			treeview.Model = store;
			prepareHeaders(columnsString);
		}
	}

	
	public override void PrepareData() 
	{
		string sessionString = obtainSessionSqlString(sessions, "jump");
		bool multisession = false;
		if(sessions.Count > 1) {
			multisession = true;
		}

		if(statsJumpsType == 3) { //avg of each jumper
			/* not allowed in this jumpType
			if(multisession) {
				string operation = "AVG";
				processDataMultiSession ( 
						SqliteStat.CmjPlusPotency(sessionString, multisession, 
							operation, jumpType, showSex, heightPreferred), 
						true, sessions.Count);
			} else {
				string operation = "AVG";
				processDataSimpleSession ( cleanDontWanted (
							SqliteStat.CmjPlusPotency(sessionString, multisession, 
								operation, jumpType, showSex, heightPreferred), 
							statsJumpsType, limit),
						true, dataColumns);
			}
			*/
		} else {
			//if more than on session, show only the avg or max of each jump/jumper
			if(multisession) {
				string operation = "MAX";
				processDataMultiSession ( SqliteStat.Potency(indexType, sessionString, multisession, 
							operation, jumpType, showSex, heightPreferred),  
						true, sessions.Count);
			} else {
				string operation = ""; //no need of "MAX", there's an order by jump.tv desc
							//and cleanDontWanted will do his work
				processDataSimpleSession ( cleanDontWanted (
							SqliteStat.Potency(indexType, sessionString, multisession, 
								operation, jumpType, showSex, heightPreferred), 
							statsJumpsType, limit),
						true, dataColumns);
			}
		}
	}
		
	public override string ToString () 
	{
		string selectedValuesString = "";
		if(statsJumpsType == 0) { //all jumps
			selectedValuesString = allValuesString; 
		} 

		string mySessionString = "";
		if(sessions.Count > 1) {
			mySessionString =  Catalog.GetString (" various sessions "); 
		} else {
			string [] strFull = sessions[0].ToString().Split(new char[] {':'});
			mySessionString =  Catalog.GetString (" session ") + 
				strFull[0] + "(" + strFull[2] + ")";
		}

		string indexTypePrint = indexType;
		if(indexType == Constants.PotencyLewisFormulaShortStr())
			indexTypePrint = Constants.PotencyLewisFormulaStr();
		else if(indexType == Constants.PotencyHarmanFormulaShortStr())
			indexTypePrint = Constants.PotencyHarmanFormulaStr();
		else if(indexType == Constants.PotencySayersSJFormulaShortStr())
			indexTypePrint = Constants.PotencySayersSJFormulaStr();
		else if(indexType == Constants.PotencySayersCMJFormulaShortStr())
			indexTypePrint = Constants.PotencySayersCMJFormulaStr();
		else if(indexType == Constants.PotencyShettyFormulaShortStr())
			indexTypePrint = Constants.PotencyShettyFormulaStr();
		else if(indexType == Constants.PotencyCanavanFormulaShortStr())
			indexTypePrint = Constants.PotencyCanavanFormulaStr();
		else if(indexType == Constants.PotencyLaraMaleApplicantsSCFormulaShortStr())
			indexTypePrint = Constants.PotencyLaraMaleApplicantsSCFormulaStr();
		else if(indexType == Constants.PotencyLaraFemaleEliteVoleiFormulaShortStr())
			indexTypePrint = Constants.PotencyLaraFemaleEliteVoleiFormulaStr();
		else if(indexType == Constants.PotencyLaraFemaleMediumVoleiFormulaShortStr())
			indexTypePrint = Constants.PotencyLaraFemaleMediumVoleiFormulaStr();
		else if(indexType == Constants.PotencyLaraFemaleSCStudentsFormulaShortStr())
			indexTypePrint = Constants.PotencyLaraFemaleSCStudentsFormulaStr();
		else if(indexType == Constants.PotencyLaraFemaleSedentaryFormulaShortStr())
			indexTypePrint = Constants.PotencyLaraFemaleSedentaryFormulaStr();

		return string.Format(Catalog.GetString("{0} in {1} applied to {2} on {3}"), selectedValuesString, indexTypePrint, jumpType, mySessionString);
	}

}

