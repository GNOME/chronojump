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
 * Copyright (C) 2004-2020   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using Gtk;
using System.Collections; //ArrayList

//using NPlot.Gtk;
//using NPlot;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO; 	//TextWriter
using System.Diagnostics; 	//Processing
using Mono.Unix;

/* ------------ CLASS HERENCY MAP ---------
 *
 * Stat
 * 	StatSjCmjAbk	
 *	StatDjTV	
 *	StatDjIndex	
 *		StatRjIndex
 *	StatPotencyBosco
 *	StatIE
 *		StatIUB //Arms Use Index
 * 	StatGlobal	//suitable for global and for a unique jumper
 *
 * ---------------------------------------
 */


public class Stat
{
	protected int sessionUniqueID;

	protected ArrayList sessions;
	protected int dataColumns; //for SimpleSession
	
	protected string jumpType; //mean also runType
	protected bool showSex;
	protected int statsJumpsType;
	protected int limit;
	protected bool heightPreferred;

	protected TreeStore store;
	protected TreeIter iter;
	protected Gtk.TreeView treeview;

	protected bool weightStatsPercent;

	protected static int pDN; //prefsDigitsNumber;
	//protected static string manName = "M";
	//protected static string womanName = "F";

	protected ArrayList markedRows;
	private ArrayList personsWithData; //useful for selecting only people with data on comboCheckboxes

	protected GraphROptions gRO;

	protected bool toReport = false;
	protected string reportString;
	
	protected bool graphTranslate = true;
	protected bool useHeightsOnJumpIndexes;


	//for toString() in every stat
	protected string allValuesString = Catalog.GetString("All values");
	protected string avgValuesString = Catalog.GetString("Avg values of each person");
	
	protected int numContinuous; //for stats rj evolution and runIntervallic
	
	//for raise a signal and manage it on src/gui/stats.cs
	//signal will be catched first in src/statType.cs and there a equal signal will be raised
	public Gtk.Button fakeButtonRowCheckedUnchecked;
	public Gtk.Button fakeButtonRowsSelected;
	public Gtk.Button fakeButtonNoRowsSelected;
	
	//private bool selectedMakeAVGSD;

	bool makeAVGSD;

	public Stat () 
	{
		fakeButtonRowCheckedUnchecked = new Gtk.Button();
		fakeButtonRowsSelected = new Gtk.Button();
		fakeButtonNoRowsSelected = new Gtk.Button();
	}

	protected void completeConstruction (StatTypeStruct myStatTypeStruct, Gtk.TreeView treeview)
	{
		//TODO: check, this is weird...
		this.sessions = myStatTypeStruct.SendSelectedSessions;
		pDN = myStatTypeStruct.preferences.digitsNumber;
		this.showSex = myStatTypeStruct.Sex_active;
		this.statsJumpsType = myStatTypeStruct.StatsJumpsType;
		this.heightPreferred = myStatTypeStruct.preferences.heightPreferred;
		this.weightStatsPercent = myStatTypeStruct.preferences.weightStatsPercent;
		this.statsJumpsType = myStatTypeStruct.StatsJumpsType;
		this.limit = myStatTypeStruct.Limit;
		this.jumpType = myStatTypeStruct.StatisticApplyTo;
		
		this.markedRows = myStatTypeStruct.MarkedRows;
		
		this.gRO = myStatTypeStruct.GRO;
		this.toReport = myStatTypeStruct.ToReport;
		this.graphTranslate = myStatTypeStruct.preferences.RGraphsTranslate;
		this.useHeightsOnJumpIndexes = myStatTypeStruct.preferences.useHeightsOnJumpIndexes;
		
		this.treeview = treeview;
		
		//initialize reportString
		reportString = "";

		iter = new TreeIter();

		personsWithData = new ArrayList();
	}

	protected string translateYesNo(string str) {
		if(graphTranslate)
			return Catalog.GetString(str);
		else
			return str;
	}
	
	void createCheckboxes(TreeView tv) 
	{
		CellRendererToggle crt = new CellRendererToggle();
		crt.Visible = true;
		crt.Activatable = true;
		crt.Active = true;
		crt.Toggled += ItemToggled;

		TreeViewColumn column = new TreeViewColumn ("", crt, "active", 0);
		column.Clickable = true;
		tv.InsertColumn (column, 0);
	}

	
	private void addRowToMarkedRows(string rowToAdd) {
		//add a row only if doesn't exist in markedRows previously
		bool found = false;
		foreach(string myRow in markedRows) {
			if(myRow == rowToAdd) {
				found = true;
				break;
			}
		}
		if(!found) {
			markedRows.Add(rowToAdd);
		}
	}
	
	private void deleteRowFromMarkedRows(string rowToDelete)
	{
		int i = 0;
		foreach(string myRow in markedRows) {
			if(myRow == rowToDelete) {
				markedRows.RemoveAt(i);
				break;
			}
			i++;
		}
	}
	
	
	void ItemToggled(object o, ToggledArgs args) {
		LogB.Information("Fake button will be pressed");
		fakeButtonRowCheckedUnchecked.Click();
		
		int column = 0;

		TreeIter iter;
		if (store.GetIter (out iter, new TreePath(args.Path)))
		{
			bool val = (bool) store.GetValue (iter, column);
			//LogB.Information ("toggled {0} with value {1}", args.Path, !val);

			//if this row is not AVG or SD
			string avgOrSD = (string) store.GetValue (iter, 1);
			if(avgOrSD != Catalog.GetString("AVG") && avgOrSD != Catalog.GetString("SD")) 
			{
				//change the checkbox value
				store.SetValue (iter, column, !val);
				//add or delete from ArrayList markedRows
				//if (val) means was true, and now has changed to false. Has been deactivated
				if(val) {
					deleteRowFromMarkedRows(args.Path);
				} else {
					addRowToMarkedRows(args.Path);
				}
				CreateOrUpdateAVGAndSD();
			}
			
			if (isThereAnyRowSelected(store)) {
				fakeButtonRowsSelected.Click();
			} else {
				fakeButtonNoRowsSelected.Click();
			}
		} else {
			//if we cannot access the treeview, also don't allow to graph or report
			fakeButtonNoRowsSelected.Click();
		}
	}
			
	private bool isNotAVGOrSD (Gtk.TreeIter iter) {
		//except AVG and SD
		string avgOrSD = (string) store.GetValue (iter, 1);
		if(avgOrSD != Catalog.GetString("AVG") && avgOrSD != Catalog.GetString("SD")) {
			return true;
		} else {
			return false;
		}
	}
	
	public void MarkSelected(string selected) {
		Gtk.TreeIter iter;
		bool okIter = store.GetIterFirst(out iter);
		if(okIter) {
			if(selected == Catalog.GetString("All")) {
				do {
					if(isNotAVGOrSD(iter)) {
						store.SetValue (iter, 0, true);
						addRowToMarkedRows(treeview.Model.GetPath(iter).ToString());
					}
				} while ( store.IterNext(ref iter) );
			} else if(selected == Catalog.GetString("None")) {
				do {
					store.SetValue (iter, 0, false);
					deleteRowFromMarkedRows(treeview.Model.GetPath(iter).ToString());
				} while ( store.IterNext(ref iter) );
			} else if(selected == Catalog.GetString("Invert")) {
				do {
					if(isNotAVGOrSD(iter)) {
						bool val = (bool) store.GetValue (iter, 0);
						store.SetValue (iter, 0, !val);
						if(val)
							deleteRowFromMarkedRows(treeview.Model.GetPath(iter).ToString());
						else
							addRowToMarkedRows(treeview.Model.GetPath(iter).ToString());
					}
				} while ( store.IterNext(ref iter) );
			} else if(selected == Catalog.GetString("Male")) {
				do {
					if(isNotAVGOrSD(iter)) {
						string nameAndSex = (string) store.GetValue (iter, 1);
						string [] stringFull = nameAndSex.Split(new char[] {'.'});
						if(stringFull.Length > 1 && stringFull[1].StartsWith (Constants.SexM)) {
							store.SetValue (iter, 0, true);
							addRowToMarkedRows(treeview.Model.GetPath(iter).ToString());
						} else {
							store.SetValue (iter, 0, false);
							deleteRowFromMarkedRows(treeview.Model.GetPath(iter).ToString());
						}
					}
				} while ( store.IterNext(ref iter) );
			} else if(selected == Catalog.GetString("Female")) {
				do {
					if(isNotAVGOrSD(iter)) {
						string nameAndSex = (string) store.GetValue (iter, 1);
						string [] stringFull = nameAndSex.Split(new char[] {'.'});
						if(stringFull.Length > 1 && stringFull[1].StartsWith (Constants.SexF)) {
							store.SetValue (iter, 0, true);
							addRowToMarkedRows(treeview.Model.GetPath(iter).ToString());
						} else {
							store.SetValue (iter, 0, false);
							deleteRowFromMarkedRows(treeview.Model.GetPath(iter).ToString());
						}
					}
				} while ( store.IterNext(ref iter) );
			} else {
				//a person is selected
				do {
					if(isNotAVGOrSD(iter)) {
						string nameWithMoreData = (string) store.GetValue (iter, 1);
						string onlyName = fetchNameOnStatsData(nameWithMoreData);
						/*
						//probably name has a jumpType like:
						//myName (CMJ), or myName.F (CMJ)
						//int parenthesesPos = nameWithMoreData.LastIndexOf('(');
						//it can have two parentheses, like:
						//myName (Rj(j))
						int parenthesesPos = nameWithMoreData.IndexOf('(');
						string nameWithoutJumpType;
						if(parenthesesPos == -1)
							nameWithoutJumpType = nameWithMoreData;
						else
							nameWithoutJumpType = nameWithMoreData.Substring(0, parenthesesPos-1);
						//probably name has sex like:
						//myName.F, or myName.F (CMJ)
						string [] onlyName = nameWithoutJumpType.Split(new char[] {'.'});
						*/
						//if(onlyName[0] == selected) {
						if(onlyName == selected) {
							store.SetValue (iter, 0, true);
							addRowToMarkedRows(treeview.Model.GetPath(iter).ToString());
						} else {
							store.SetValue (iter, 0, false);
							deleteRowFromMarkedRows(treeview.Model.GetPath(iter).ToString());
						}
					}
				} while ( store.IterNext(ref iter) );
			}

			//check rows selected and raise a signal if noone is selected
			if (isThereAnyRowSelected(store)) {
				fakeButtonRowsSelected.Click();
			} else {
				fakeButtonNoRowsSelected.Click();
			}
		} else {
			//if we cannot access the treeview, also don't allow to graph or report
			fakeButtonNoRowsSelected.Click();
		}
	}
	
	private bool isThereAnyRowSelected(TreeStore myStore) {
		Gtk.TreeIter iter;
		bool okIter = store.GetIterFirst(out iter);
		if(okIter) {
			do {
				bool val = (bool) store.GetValue (iter, 0);
				if(val) {
					return true;
				}
			} while ( store.IterNext(ref iter) );
		}
		return false;
	}

	protected void prepareHeaders(string [] columnsString) 
	{
		createCheckboxes(treeview);
		
		treeview.HeadersVisible=true;
		treeview.AppendColumn (Catalog.GetString(columnsString[0]), new CellRendererText(), "text", 1);

		int i;
		if(sessions.Count > 1) {
			string myHeaderString = "";
			string [] stringFullResults;
			for (i=0; i < sessions.Count ; i++) {
				//we need to know the name of the column: session
				stringFullResults = sessions[i].ToString().Split(new char[] {':'});
				myHeaderString = stringFullResults[1] + "\n" + 
					stringFullResults[2] + "\n" + 
					Catalog.GetString(columnsString[1]); //name, date, col name
				treeview.AppendColumn (myHeaderString, new CellRendererText(), "text", i+2); 
			}
			//if multisession, add AVG and SD cols
			treeview.AppendColumn (Catalog.GetString("AVG"), new CellRendererText(), "text", i+2); 
			treeview.AppendColumn (Catalog.GetString("SD"), new CellRendererText(), "text", i+3);
		} else {
			treeview.AppendColumn (Catalog.GetString(columnsString[1]), new CellRendererText(), "text", 2); 
			//if there's only one session, add extra data columns if needed
			for(i=2 ; i <= dataColumns ; i++) {
				treeview.AppendColumn (columnsString[i], new CellRendererText(), "text", i+1);
			}
		}
	}
	
	protected string prepareHeadersReport(string [] columnsString) 
	{
		string myHeaderString = "";
		myHeaderString += "<TABLE cellspacing=2 cellpadding=2>\n";
		myHeaderString += "<TR><TH>" + Catalog.GetString(columnsString[0]) + "</TH>";
		
		if(sessions.Count > 1) {
			string [] stringFullResults;
			for (int i=0; i < sessions.Count ; i++) {
				stringFullResults = sessions[i].ToString().Split(new char[] {':'});
				
				myHeaderString += "<TH>" + stringFullResults[1] + "\n" + 
					stringFullResults[2] + "\n" + 
					Catalog.GetString(columnsString[1]) + "</TH>"; //name, date, col name
			}
			//if multisession, add AVG and SD cols
			myHeaderString += "<TH>" + Catalog.GetString("AVG") + "</TH>";
			myHeaderString += "<TH>" + Catalog.GetString("SD") + "</TH>";
		} else {
			//for(int i=1 ; i <= dataColumns ; i++) {
			for(int i=1 ; i < columnsString.Length ; i++) {
				myHeaderString += "<TH>" + columnsString[i] + "</TH>";
			}
		}
		myHeaderString += "</TR>\n";

		return myHeaderString;
	}
	
	protected TreeStore getStore (int columns)
	{
		//prepares the TreeStore for required columns
		//columns +1 for the checkbox col (not counted in columns)
		Type [] types = new Type [columns+1];
		
		//adding the checkbox col (not counted in columns)
		types[0] = typeof (bool);
		for (int i=1; i <= columns; i++) {
			types[i] = typeof (string);
		}
		TreeStore myStore = new TreeStore(types);
		return myStore;
	}
	
	protected string obtainSessionSqlString(ArrayList sessions, string tableName)
	{
		string newStr = "WHERE (";
		for (int i=0; i < sessions.Count; i++) {
			string [] stringFullResults = sessions[i].ToString().Split(new char[] {':'});
			newStr = newStr + " " + tableName + ".sessionID == " + stringFullResults[0];
			if (i+1 < sessions.Count) {
				newStr = newStr + " OR ";
			}
		}
		newStr = newStr + ") ";
		return newStr;		
	}
	
	protected string obtainSessionSqlStringTwoTests(ArrayList sessions)
	{
		string newStr = "WHERE (";
		for (int i=0; i < sessions.Count; i++) {
			string [] stringFullResults = sessions[i].ToString().Split(new char[] {':'});
			newStr = newStr + " (j1.sessionID == " + stringFullResults[0] +
				" AND j2.sessionID == " + stringFullResults[0] + ")";
			if (i+1 < sessions.Count) {
				newStr = newStr + " OR ";
			}
		}
		newStr = newStr + ") ";
		return newStr;
	}

	
	public virtual void PrepareData () {
	}


	private bool isThisRowMarked(int rowNum) {
		for(int k=0; k < markedRows.Count; k++) {
			if(Convert.ToInt32(markedRows[k]) == rowNum) {
				return true;
			}
		}
		return false;
	}
	
	//called before processDataSimpleSession, 
	//used for deleting rows dont wanted by the statsJumpsType 0 and 1 values
	protected ArrayList cleanDontWanted (ArrayList startJumps, int statsJumpsType, int limit)
	{
		int i;
		ArrayList endJumps = new ArrayList(2);
		string [] stringFullResults;
		ArrayList arrayJumpers = new ArrayList(2);
		
		for (i=0 ; i < startJumps.Count ; i ++) 
		{
			stringFullResults = startJumps[i].ToString().Split(new char[] {':'});

			//if limited number of total jumps and we reached:
			if (statsJumpsType == 1 && i >= limit) {
				break;
			}
			//if only 'n' jumps by person and we reached:
			else if (statsJumpsType == 2) {
				if (nFoundInArray (stringFullResults[0], arrayJumpers, limit)) {
					continue;
				} else {
					arrayJumpers.Add(stringFullResults[0]);
				}
			}
			//accept this row
			endJumps.Add(startJumps[i]);
		}
		return endJumps;
	}

	//one column by each dataColumn returned by SQL
	protected void processDataSimpleSession (ArrayList arrayFromSql, bool makeAVGSD, int dataColumns) 
	{
		this.makeAVGSD = makeAVGSD;

		statPrintedData = new ArrayList();

		string [] rowFromSql = new string [dataColumns +1];
		int i;

		//removes persons in personsWithData
		personsWithData = new ArrayList();
	
		//process all SQL results line x line
		for (i=0; i < arrayFromSql.Count ; i ++) {
			rowFromSql = arrayFromSql[i].ToString().Split(new char[] {':'});
		
			for (int j=1; j <= dataColumns ; j++) {
				rowFromSql[j] = Util.TrimDecimals(rowFromSql[j], pDN);
			}
			printData( rowFromSql );

			//add people who are in this stat into personsWithData
			//for being selected on comboCheckboxes
			personsWithData = Util.AddToArrayListIfNotExist(
					personsWithData, fetchNameOnStatsData(rowFromSql[0]));
		}
		//only show the row if sqlite returned values
		if(i >0)
		{
			if(makeAVGSD) 
		//	don't do it here because we can come from graph and then there's no treeview
				CreateOrUpdateAVGAndSD();
		} else {
			//if we cannot access the treeview, also don't allow to graph or report
			LogB.Information("no rows Clicking in stats/main.cs simple session");
			fakeButtonNoRowsSelected.Click();
		}
	}

	//one column by each session returned by SQL
	protected void processDataMultiSession (ArrayList arrayFromSql, bool makeAVGSD, int sessionsNum) 
	{
		this.makeAVGSD = makeAVGSD;

		statPrintedData = new ArrayList();

		string [] rowFromSql = new string [sessionsNum +1];
		string [] sendRow = new string [sessionsNum +1];
		int i;

		//initialize values
		for(int j=1; j< sessionsNum+1 ; j++) {
			sendRow[j] = "-";
		}

		//removes persons in personsWithData
		personsWithData = new ArrayList();

		string oldStat = "-1";
	
		//process all SQL results line x line
		for (i=0 ; i < arrayFromSql.Count ; i ++) {
			rowFromSql = arrayFromSql[i].ToString().Split(new char[] {':'});

			if (rowFromSql[0] != oldStat) {
				//print the values, except on the first iteration
				if (i>0) {
					printData( calculateRowAVGSD(sendRow) );
				}
				
				//add people who are in this stat into personsWithData
				//for being selected on comboCheckboxes
				personsWithData = Util.AddToArrayListIfNotExist(
						personsWithData, fetchNameOnStatsData(rowFromSql[0]));


				//process another stat
				sendRow[0] = rowFromSql[0]; //first value to send (the name of stat)
				for(int j=1; j< sessionsNum+1 ; j++) {
					sendRow[j] = "-";
				}
			}

			for (int j=0; j < sessions.Count ; j++) {
				string [] str = sessions[j].ToString().Split(new char[] {':'});
				if(rowFromSql[1] == str[0]) { //if matches the session num
					sendRow[j+1] = Util.TrimDecimals(rowFromSql[2], pDN); //put value from sql in the desired pos of sendRow
				}
			}
			oldStat = sendRow[0];
		}
		
		//only show the row if sqlite returned values
		if(i > 0)
		{
			printData( calculateRowAVGSD(sendRow) );

			if(makeAVGSD) {
				CreateOrUpdateAVGAndSD();
			}
		} else {
			//if we cannot access the treeview, also don't allow to graph or report
			LogB.Information("no rows Clicking in stats/main.cs multi session");
			fakeButtonNoRowsSelected.Click();
		}
	}

	//now using statPrintedData arraylist
	//works for treeview, report, and graph (in report or not)
	public void CreateOrUpdateAVGAndSD() {
		if( ! makeAVGSD) 
			return;

		//delete the AVG, SD rows of store if we are not in report
		try {
			Gtk.TreeIter iter;
			bool okIter = store.GetIterFirst(out iter);
			if(okIter) {
				do {
					if(! isNotAVGOrSD(iter)) {
						//delete AVG and SD rows
						okIter = store.Remove(ref iter);
						okIter = store.Remove(ref iter);
						//okIter is because iter is invalidated when deleted last row
					}
				} while (okIter && store.IterNext(ref iter));
			}
		} catch {
			LogB.Error("On graph or report (or graph, report)");
		}


		//if multisession number of dataCols will be sessions
		//else it will be dataColumns
		int myDataColumns = 0;
		if(sessions.Count > 1)
			myDataColumns = sessions.Count;
		else
			myDataColumns = dataColumns;


		//if called from a graph will not work because 
		//nothing is in store
		try {
			if(statPrintedData.Count > 0) {
				double [] sumValue = new double [myDataColumns];
				string [] valuesList = new string [myDataColumns];
				int [] valuesOk = new int [myDataColumns]; //values in a checked row and that contain data (not "-")
				//initialize values
				for(int j=1; j< myDataColumns ; j++) {
					sumValue[j] = 0;
					valuesList[j] = "";
					valuesOk[j] = 0;
				}
				int rowsFound = 0;
				int rowsProcessed = 0;
				foreach(string myStatData in statPrintedData) {
					string [] myStrFull = myStatData.Split(new char[] {':'});

						if(isThisRowMarked(rowsFound)) {
							for(int column = 0; column < myDataColumns; column ++) {
								//LogB.Information("value: {0}", store.GetValue(iter, column+2));
								//string myValue = store.GetValue(iter, column+2).ToString();
								string myValue = myStrFull[column+1];
								if(myValue != "-") {
									if(valuesOk[column] == 0)
										valuesList[column] = myValue;
									else
										valuesList[column] += ":" + myValue;
									sumValue[column] += Convert.ToDouble(myValue);
									valuesOk[column] ++;
								}
							}
							rowsProcessed ++;
						}
					rowsFound ++;
				}

				if(rowsProcessed > 0) {			
					string [] sendAVG = new string [myDataColumns +1];
					string [] sendSD = new string [myDataColumns +1];

					sendAVG[0] = Catalog.GetString("AVG");
					sendSD[0] =  Catalog.GetString("SD");

					for (int j=0; j < myDataColumns; j++) {
						if(valuesOk[j] > 0)
							sendAVG[j+1] = Util.TrimDecimals( (sumValue[j] / valuesOk[j]).ToString(), pDN );
						else
							sendAVG[j+1] = "-";
						//LogB.Information("j({0}), SendAVG[j]({1}), valuesList[j]({2})", j, sendAVG[j+1], valuesList[j]);
						sendSD[j+1] = Util.TrimDecimals( Util.CalculateSD(valuesList[j], sumValue[j], valuesOk[j]).ToString(), pDN );
						//LogB.Information("j({0}), SendSD[j]({1})", j, sendSD[j+1]);
					}
					printData( sendAVG );
					printData( sendSD );
				}
			}
		} catch {
			/* check this if it's needed now*/
			//write a row of AVG because graphs of stats with AVG and SD
			//are waiting the AVG row for ending and painting graph
			LogB.Error("catched!");
			string [] sendAVG = new string [myDataColumns +1];
			sendAVG[0] = Catalog.GetString("AVG");
			printData(sendAVG);
			LogB.Information("Graph should work!");
		}
	}

	//returns a row with it's AVG and SD in last two columns
	protected string [] calculateRowAVGSD(string [] rowData) 
	{
		string [] rowReturn = new String[sessions.Count +3];
		int count =0;
		double sumValue = 0;
		string valuesList ="";
		string separator ="";
	
		if(sessions.Count > 1) {
			int i=0;
			for (i=0; i < sessions.Count + 1; i++) {
				rowReturn[i] = rowData[i];
				if(i>0 && rowReturn[i] != "-") { //first column is text
					count++;
					sumValue += Convert.ToDouble(rowReturn[i]); 
					valuesList += separator + rowReturn[i];
					separator = ":";
				}
			}
			if(count > 0) {
				rowReturn[i] = Util.TrimDecimals( (sumValue /count).ToString(), pDN );
				if(count > 1) {
					rowReturn[i+1] = Util.TrimDecimals( Util.CalculateSD(valuesList, sumValue, count).ToString(), pDN );
				} else {
					rowReturn[i+1] = "-";
				}
			} else {
				rowReturn[i] = "-";
				rowReturn[i+1] = "-";
			}
					
			return rowReturn;
		} else {
			return rowData;
		}
	}
	
	//for stripping off unchecked rows in report
	private int rowsPassedToReport = 0;
	private ArrayList statPrintedData;
	
	protected void recordStatValues (string [] statValues) {
		if(statValues[0] != Catalog.GetString("AVG") &&
				statValues[0] != Catalog.GetString("SD")) {
			string completeRowAsOneString = Util.StringArrayToString(statValues, ":");
			statPrintedData.Add(completeRowAsOneString);
		}
	}

	protected virtual void printData (string [] statValues) 
	{
		//record all data in an ArrayList except AVG and stat
		//then gui stat and report can read this ArrayList
		//(before CreateOrUpdateAVGAndSD only read in treeview, and this doesn't work for report
		recordStatValues(statValues);

		if(toReport) {
			//LogB.Information("REPORT: {0}", statValues[0]);
			//print marked rows and AVG, SD rows
			bool allowedRow = isThisRowMarked(rowsPassedToReport);

			bool isAVGOrSD;
			string boldIni = "";
			string boldEnd = "";

			if(statValues[0] == Catalog.GetString("AVG") || statValues[0] == Catalog.GetString("SD")) {
				isAVGOrSD = true;
				boldIni = "<b>";
				boldEnd = "</b";
			}
			else 
				isAVGOrSD = false;

			if(allowedRow || isAVGOrSD) {
				reportString += "<TR>";
				for (int i=0; i < statValues.Length ; i++) {
					reportString += "<TD>" + boldIni + statValues[i] + boldEnd + "</TD>";
				}
				reportString += "</TR>\n";
			}
			rowsPassedToReport ++;
		} else {
			iter = new TreeIter();
			
			//iter = store.Append (iter);	//doesn't work
			//store.Append (out iter);	//add new row and make iter point to it
			iter = store.AppendNode ();
		
			//addAllNoneIfNeeded(statValues.Length);
			
			TreePath myPath = store.GetPath(iter); 
			
			if(statValues[0] != Catalog.GetString("AVG") && statValues[0] != Catalog.GetString("SD")) {
				store.SetValue(iter, 0, true);	//first col is true if it's not AVG or SD
				markedRows.Add(myPath.ToString());
				//LogB.Information("FROM PRINTDATA Added to markedRows row:{0}", myPath.ToString());
			}
			
			for(int i=0; i < statValues.Length; i++) {
				store.SetValue(iter, i+1, statValues[i]);
			}
		}
	}

	//public virtual string ObtainEnunciate () {
	//}

	protected static double makeSquare (string myValueStr) {
		double myDouble;
		myDouble = Convert.ToDouble(myValueStr);
		return myDouble * myDouble;
	}
	
	//true if found equal or more than 'limit' occurrences of 'searching' in array
	protected static bool nFoundInArray (string searching, ArrayList myArray, int limit) 
	{
		int count = 0;
		for (int i=0; i< myArray.Count && count <= limit ; i ++) {
			//LogB.Information("searching {0}, myArray[i] {1}, limit {2}", searching, myArray[i], limit);
			if (searching == myArray[i].ToString()) {
				count ++;
			}
		}
		if(count >= limit) {
			return true;
		}
		return false;
	}
	
	private string fetchNameOnStatsData (string nameWithMoreData)
	{
		//probably name has a jumpType like:
		//myName (CMJ), or myName.F (CMJ)
		//int parenthesesPos = nameWithMoreData.LastIndexOf('(');
		//it can have two parentheses, like:
		//myName (Rj(j))
		int parenthesesPos = nameWithMoreData.IndexOf('(');
		string nameWithoutJumpType;
		if(parenthesesPos == -1)
			nameWithoutJumpType = nameWithMoreData;
		else
			nameWithoutJumpType = nameWithMoreData.Substring(0, parenthesesPos-1);
		//probably name has sex like:
		//myName.F, or myName.F (CMJ)
		string [] onlyName = nameWithoutJumpType.Split(new char[] {'.'});
		return onlyName[0];
	}

	public virtual void RemoveColumns() {
		Gtk.TreeViewColumn [] myColumns = treeview.Columns;
		foreach (Gtk.TreeViewColumn column in myColumns) {
			treeview.RemoveColumn (column);
		}
	}
	
	public string ReportString () {
		return reportString + "</TABLE></p>\n";
	}

	/*
	 * ---------------------------------
	 * graph things to inherit
	 * ---------------------------------
	 */

	//new classes graphSerie, graphData
	protected GraphData CurrentGraphData = new GraphData();
	protected ArrayList GraphSeries = new ArrayList();

	protected bool isRjEvolution = false; //needed because in RjEvolution graph, series are treaten in a different way
	protected bool isRunIntervalEvolution = false; //small differences with isRjEvolution
	int rjEvolutionMaxJumps; //we should care of maxjumps of the checked rjEvolution rows

	
	//temporary hack for a gtk# garbage collecting error
	//protected ArrayList onlyUsefulForNotBeingGarbageCollected = new ArrayList(); 
	//has to be static
	protected static ArrayList onlyUsefulForNotBeingGarbageCollected = new ArrayList(); 


/* R TESTS */

		/* 
		interessant:
		plot(table(rpois(100,5)), type = "h", col = "red", lwd=10, main="rpois(100,lambda=5)")
		*/

	private bool hasTwoAxis()
	{
		bool left = false;
		bool right = false;
		foreach(GraphSerie serie in GraphSeries) {
			if(serie.IsLeftAxis)
				left=true;
			else
				right=true;
		}
		return (left && right);
	}


	private string convertDataToR(GraphROptions gro, Sides side) 
	{
		string rD = ""; //rDataString
		string bD = "data <- cbind("; //bindDataString
		string colNamesD = "colnames(data) <- c("; //colNamesDataString
		string sepSerie = "";
		string xyFirstFound = "";
		int count = 0; //this counts accepted series
		int countCols=0;
		int countRows=0; //on multisession, names of persons come in rows. Use this to discard some rows if unselected on treeview (! markedRows)
		foreach(GraphSerie serie in GraphSeries) {
			LogB.Information("serie:" + serie.Title);
			if(
					side == Sides.LEFT && ! serie.IsLeftAxis ||
					side == Sides.RIGHT && serie.IsLeftAxis) {
				continue;
			}

			//don't plot AVG row on multisession
			if(sessions.Count > 1 && serie.Title == Catalog.GetString("AVG"))
				continue;

			//on multisession, names of persons come in rows. Use this to discard some rows if unselected on treeview (! markedRows)
			if(sessions.Count > 1 && ! acceptCheckedData(countRows)) {
				countRows ++;
				continue;
			}

			//on XY only take two vars
			if(gro.Type == Constants.GraphTypeXY) {
				LogB.Information("groVarX: " + gro.VarX + " groVarY: " + gro.VarY + " tit: " + serie.Title);
				if(gro.VarX != serie.Title && gro.VarY != serie.Title)
					continue;
				else if (xyFirstFound == "") {
					if(gro.VarX == serie.Title)
						xyFirstFound = "x";
					else
						xyFirstFound = "y";
				}
			}
			
			//Histogram and Dotchart plot col 1
			if( (gro.Type == Constants.GraphTypeHistogram || gro.Type == Constants.GraphTypeDotchart)
					&& gro.VarX != serie.Title)
				continue;


			rD += "serie" + count.ToString() + " <- c(";
			string sep = "";
			countCols=0;
			foreach(string val in serie.SerieData) {
				LogB.Information(" val:" + val);
				bool use = true;

				//on simplesession, cols are persons. See if they are discarded on markedRows
				if(sessions.Count == 1 && ! acceptCheckedData(countCols))
					use = false;

				//don't plot AVG col on multisession
				if(sessions.Count > 1 && countCols == serie.SerieData.Count)
					use = false;
				
				//don't plot SD col on multisession
				if(sessions.Count > 1 && countCols +1 == serie.SerieData.Count)
					use = false;

				countCols++;
				if(! use)
					continue;
				
				if(val == "-") 
					rD += sep + "NA";
				else
					rD += sep + Util.ConvertToPoint(val);
				sep = ", ";
			}
			rD += ")\n";
			bD += sepSerie + "serie" + count.ToString();
		
			colNamesD += sepSerie + "'" + Util.RemoveTilde(serie.Title)  + "'";
			sepSerie = ", ";
			count ++;
			countRows ++;
		}

		string rowNamesD = "rownames(data) <- c("; //rowNamesDataString
		//create rows
		string sep2 = "";
		for(int i=0; i < CurrentGraphData.XAxisNames.Count; i++) {
			//on simplesession, cols are persons. See if they are discarded on markedRows
			if(sessions.Count == 1 && ! acceptCheckedData(i))
				continue;
			
			string name = Util.RemoveTilde(CurrentGraphData.XAxisNames[i].ToString());	
			
			//convert accents to Unicode in order to be plotted correctly on R windows
			if(UtilAll.IsWindows()) 
				name = Util.ConvertToUnicode(name);

			rowNamesD += sep2 + "'" + name  + "'";

			//each four values add a \n to not have a "long line" problem in sending data to R
			if((i+1) % 4 == 0)
				rowNamesD += "\n";
			
			sep2 = ", ";
		}
		
		bD += ")\n";
		colNamesD += ")\n";
		rowNamesD += ")\n";

		if(gro.Type == Constants.GraphTypeXY) {
			if(gro.VarX == gro.VarY) {
				//if it's an XY with only one serie, (both selected vars are the same
				rD += rD.Replace("serie0", "serie1"); //duplicate rD changing serie name
				bD = "data <- cbind(serie0, serie1)\n";
			
				//have two colNamesD equal to first
				string [] cn = colNamesD.Split(new char[] {'\''});
				colNamesD = "colnames(data) <- c('" + cn[1] + "', '" + cn[1] + "')\n";
			} else if (xyFirstFound == "y") {
				//if we first found the y value change serie0 to serie1
				rD = rD.Replace("serie1", "serie2"); 
				rD = rD.Replace("serie0", "serie1");
				rD = rD.Replace("serie2", "serie0");
			}
		}

		string allData = rD + bD + colNamesD + rowNamesD + "data\n";

		if(gro.Transposed && 
				gro.Type != Constants.GraphTypeXY && 
				gro.Type != Constants.GraphTypeDotchart &&
				gro.Type != Constants.GraphTypeBoxplot &&
				gro.Type != Constants.GraphTypeHistogram &&
				gro.Type != Constants.GraphTypeStripchart
				)
			allData += "data <- t(data)\n";
	
		return allData;	
	}

	private string convertDataToROnRjEvolution(GraphROptions gro, Sides side) 
	{
		string rD = ""; //rDataString
		string bD = "data <- cbind("; //bindDataString
		string colNamesD = "colnames(data) <- c("; //colNamesDataString
		string sepSerie = "";
		int count = 0; //this counts accepted series
		int countSeries = 0; //for RJ
		int countAcceptedCols=0;
		rjEvolutionMaxJumps = -1;
		foreach(GraphSerie serie in GraphSeries) {
			if(
					side == Sides.LEFT && ! serie.IsLeftAxis ||
					side == Sides.RIGHT && serie.IsLeftAxis) {
				continue;
			}

			//in isRjEvolution then check if this serie will be shown (each jumper has a TC and a TF serie)
			if(isRjEvolution) {
				if( ! acceptCheckedData( divideAndRoundDown(countSeries)) ) {
					countSeries ++;
					continue;
				}
			} else {
				if(! acceptCheckedData(countSeries)) {
					countSeries ++;
					continue;
				}
			}

			rD += "serie" + count.ToString() + " <- c(";
			string sep = "";
			countAcceptedCols=0;
			foreach(string val in serie.SerieData) {
				countAcceptedCols++;
				if(val == "-1")
					rD += sep + "NA"; //don't plot starting -1 on evolutions
				else if(val == "-") 
					rD += sep + "NA"; //don't plot 0's on ended evolutions
				else
					rD += sep + Util.ConvertToPoint(val);
				sep = ", ";
				
				if(val != "-" && countAcceptedCols > rjEvolutionMaxJumps)
					rjEvolutionMaxJumps = countAcceptedCols;
			}
			rD += ")\n";
			bD += sepSerie + "serie" + count.ToString();
		
			sepSerie = ", ";
			count ++;
			countSeries ++;
		}

		string rowNamesD = "rownames(data) <- c("; //rowNamesDataString

		//create cols
		int i=0;
		string sep2 = "";
		foreach(GraphSerie serie in GraphSeries) {
			if(isRjEvolution) {
				if(acceptCheckedData(divideAndRoundDown(i++))) {
					colNamesD += sep2 + "'" + serie.Title + "'";
					sep2 = ", ";
				}
			} else {
				if(acceptCheckedData(i++)) {
					colNamesD += sep2 + "'" + serie.Title + "'";
					sep2 = ", ";
				}
			}
		}
		//create rows
		sep2 = "";
		for(int j=1; j < countAcceptedCols+1; j++) {
			rowNamesD += sep2 + j;
			sep2 = ", ";
		}
		
		bD += ")\n";
		colNamesD += ")\n";
		rowNamesD += ")\n";

			
		string allData = rD + bD + colNamesD + rowNamesD + "data\n";

		if(gro.Transposed)
			allData += "data <- t(data)\n";
	
		return allData;	
	}

	string getTitle(string graphType, string subTitle) {
		//subtitle can be XY correlation or can come from graphdata (see rjPotencyBosco)
		string sub = subTitle;
		if(sub == "")
			sub="sub='" + CurrentGraphData.GraphSubTitle + "'";

		return "title(main='" + CurrentGraphData.GraphTitle + " (" + graphType +")', " + 
			sub + ", cex.sub=0.75, font.sub=3, col.sub='grey30')\n";
	}
	
	private string getRBoxplotString(GraphROptions gro, string fileName, Sides side) {
		string allData = convertDataToR(gro, side);
		
		string ylabStr = "";
		if(side == Sides.RIGHT) {
			if(CurrentGraphData.LabelRight != "")
				ylabStr = ", ylab='" + Util.RemoveTilde(CurrentGraphData.LabelRight) + "'";
		}
		else { //ALL or LEFT
			if(CurrentGraphData.LabelLeft != "")
				ylabStr = ", ylab='" + Util.RemoveTilde(CurrentGraphData.LabelLeft) + "'";
		}

		string rG = //rGraphString
			"boxplot(as.data.frame(data), lwd="+ gro.LineWidth +", las=2, xlab=''" + ylabStr + ")\n" +
			"axis(1, 1:length(colnames(data)), colnames(data), las=2)\n"; //axis separated from boxplot because if data hsa one col, names are not displayed
		
		//have an unique title for both graphs
		string titStr = getTitle("Boxplot","");
		if(hasTwoAxis()) {
		       if(side==Sides.RIGHT)
				rG += "par(mfrow=c(1,1), new=TRUE)\n" +
					"plot(-1, axes=FALSE, type='n', xlab='', ylab='')\n" +
					titStr + 
					"par(mfrow=c(1,1), new=FALSE)\n";
		} else
			rG += titStr;

		return allData + rG;
	}


	private string getRBarplotString(GraphROptions gro, string fileName, Sides side) {
		string allData = convertDataToR(gro, side);
		
		string ylabStr = "";
		if(side == Sides.RIGHT) {
			if(CurrentGraphData.LabelRight != "")
				ylabStr = ", ylab='" + Util.RemoveTilde(CurrentGraphData.LabelRight) + "'";
		}
		else { //ALL or LEFT
			if(CurrentGraphData.LabelLeft != "")
				ylabStr = ", ylab='" + Util.RemoveTilde(CurrentGraphData.LabelLeft) + "'";
		}
	
		//black only has no sense on barplot	
		if(gro.Palette == Constants.GraphPaletteBlackStr())
			gro.Palette="gray.colors";

		string rG = //rGraphString
		   	" colors=" + gro.Palette +"(length(rownames(data)))\n" +
			"barplot(data, beside=T, col=colors, lwd="+ gro.LineWidth +", las=2, xlab=''" + ylabStr + ")\n" +
			" legend('" + gro.Legend +"', legend=rownames(data), cex=.7, col=colors, pch=15)\n";
		
		//have an unique title for both graphs
		string titStr = getTitle("Barplot","");
		if(hasTwoAxis()) {
		       if(side==Sides.RIGHT)
				rG += "par(mfrow=c(1,1), new=TRUE)\n" +
					"plot(-1, axes=FALSE, type='n', xlab='', ylab='')\n" +
					titStr + 
					"par(mfrow=c(1,1), new=FALSE)\n";
		} else
			rG += titStr;

		return allData + rG;
	}


	private string getRLinesString(GraphROptions gro, string fileName, Sides side) {
		string allData = "";
		if(isRjEvolution || isRunIntervalEvolution)
			allData = convertDataToROnRjEvolution(gro, side);
		else
			allData = convertDataToR(gro, side);
		
		string axesStr = "";
		string ylabStr = "";
		if(side == Sides.RIGHT) {
			axesStr = " axis(4)\n";
			if(CurrentGraphData.LabelRight != "")
				ylabStr = ", ylab='" + Util.RemoveTilde(CurrentGraphData.LabelRight) + "'";
		}
		else { //ALL or LEFT
			axesStr = " axis(2)\n";
			if(CurrentGraphData.LabelLeft != "")
				ylabStr = ", ylab='" + Util.RemoveTilde(CurrentGraphData.LabelLeft) + "'";
		}

		string naString = ", na.rm = TRUE";

		string xlimString = "c(0,length(colnames(data))+1)";
		if(isRjEvolution || isRunIntervalEvolution)
			xlimString = "c(1," + rjEvolutionMaxJumps + ")";
	
		//TC and TF same color on rjEvo	
		string colorsConversionString = "";

		string colors1="colors[1]";
		string colorsi="colors[i]";
		string colors="colors";
		bool changedPalette = false;
		if(gro.Palette == Constants.GraphPaletteBlackStr()) {
			colors1="'black'";
			colorsi="'black'";
			colors="'black'";
			gro.Palette="gray.colors";
			changedPalette = true;
		} else if(isRjEvolution) //runIntervalEvolution doesn't have this because only has one serie
			colorsConversionString = "for(i in 2:length(colors)) if(i%%2 == 0) colors[i]=colors[i-1]\n";

		string rG = //rGraphString
		   	" colors=" + gro.Palette +"(length(rownames(data)))\n" +
			colorsConversionString +
		   	" plot(data[1,1:length(colnames(data))], type='b', lwd="+ gro.LineWidth +", xlim=" + xlimString + "," +
			" ylim=c(min(data" + naString +"),max(data" + naString + ")), pch=1, axes=FALSE, col="+ colors1 +", xlab=''" + ylabStr + ")\n" +
			" if(length(rownames(data))>=2) {\n" +
			" 	for(i in 2:length(rownames(data)))\n" +
		   	" 		points(data[i,1:length(colnames(data))], type='b', lwd="+ gro.LineWidth +", pch=i, col="+ colorsi +")\n" +
			" }\n" +
			" axis(1, 1:length(colnames(data)), colnames(data), las=2)\n" +
			axesStr + 
			" legend('" + gro.Legend +"', legend=rownames(data), pch=c(1:length(rownames(data))), cex=.7, col="+ colors +")\n";
			
		if(changedPalette)
			gro.Palette=Constants.GraphPaletteBlackStr();
		
		//have an unique title for both graphs
		string titStr = getTitle("Lines", "");
		if(hasTwoAxis()) {
		       if(side==Sides.RIGHT)
				rG += "par(mfrow=c(1,1), new=TRUE)\n" +
					"plot(-1, axes=FALSE, type='n', xlab='', ylab='')\n" +
					titStr + 
					"par(mfrow=c(1,1), new=FALSE)\n";
		} else
			rG += titStr;
		
		return allData + rG;
	}
	
	private string getRStripchartString(GraphROptions gro, string fileName, Sides side) {
		string allData = convertDataToR(gro, side);
		
		string xlabStr = "";
		if(side == Sides.RIGHT) {
			if(CurrentGraphData.LabelRight != "")
				xlabStr = ", xlab='" + Util.RemoveTilde(CurrentGraphData.LabelRight) + "'";
		}
		else { //ALL or LEFT
			if(CurrentGraphData.LabelLeft != "")
				xlabStr = ", xlab='" + Util.RemoveTilde(CurrentGraphData.LabelLeft) + "'";
		}

		string rG = //rGraphString
			"stripchart(as.data.frame(data), lwd="+ gro.LineWidth +", las=2" + xlabStr + ", ylab='', method='jitter', pch=3, jitter=.2)\n" +
			"axis(2, 1:length(colnames(data)), colnames(data), las=2)\n"; //axis separated from boxplot because if data hsa one col, names are not displayed
		
		//have an unique title for both graphs
		string titStr = getTitle("Stripchart","");
		if(hasTwoAxis()) {
		       if(side==Sides.RIGHT)
				rG += "par(mfrow=c(1,1), new=TRUE)\n" +
					"plot(-1, axes=FALSE, type='n', xlab='', ylab='')\n" +
					titStr + 
					"par(mfrow=c(1,1), new=FALSE)\n";
		} else
			rG += titStr;

		return allData + rG;
	}

	private string getRXYString(GraphROptions gro, string fileName) {
		string allData = convertDataToR(gro, Sides.ALL);
		string titStr = getTitle(Catalog.GetString("Dispersion"), "sub=paste('correlation:',cor(serie0,serie1),'   R^2:',cor(serie0,serie1)^2)");
		
		string colors="colors";
		bool changedPalette = false;
		if(gro.Palette == Constants.GraphPaletteBlackStr()) {
			colors="'black'";
			gro.Palette="gray.colors";
			changedPalette = true;
		}

		//prediction is discussed here:
		//https://stat.ethz.ch/pipermail/r-help-es/2009-December/000539.html

		string rG = //rGraphString
		   	"colors=" + gro.Palette +"(length(rownames(data)))\n" +
			"rang <- c(1:length(rownames(data)))\n" +
			//"plot(serie0, serie1, pch=rang, col="+ colors +", xlab='" + Util.RemoveTilde(gro.VarX) + "', ylab='" + Util.RemoveTilde(gro.VarY) + "')\n" +
			"plot(serie0,serie1,xlim=c(min(serie0),max(serie0)),ylim=c(min(serie1),max(serie1)), " + 
			"pch=rang, lwd="+ gro.LineWidth +", col="+ colors +", xlab='" + Util.RemoveTilde(gro.VarX) + 
			"', ylab='" + Util.RemoveTilde(gro.VarY) + "')\n" +
			"legend('" + gro.Legend +"' ,legend=rownames(data), pch=rang, col="+ colors +", cex=.7)\n" +
			titStr + "\n" +
	
			"mylm<-lm(serie1~serie0)\n" +
			"abline(mylm,col='red', lwd="+ gro.LineWidth +")\n" +
			"newx<-seq(min(serie0),max(serie0),length.out=length(serie0))\n" +
			"prd<-predict(mylm,newdata=data.frame(serie0=newx),interval = c('confidence'), level = 0.90,type='response')\n" +
			"lines(newx,prd[,3],col='red', lwd="+ gro.LineWidth +",lty=2)\n" +
			"lines(newx,prd[,2],col='red', lwd="+ gro.LineWidth +",lty=2)\n" +
			"text(newx[1],prd[1,3],'90%', cex=0.6)\n" +
			"text(newx[1],prd[1,2],'90%', cex=0.6)\n" +
			"text(newx[length(newx)],prd[length(newx),3],'90%', cex=0.6)\n" +
			"text(newx[length(newx)],prd[length(newx),2],'90%', cex=0.6)\n";

		if(changedPalette)
			gro.Palette=Constants.GraphPaletteBlackStr();

		return allData + rG;
	}

	private string getRHistogramString(GraphROptions gro, string fileName) {
		string allData = convertDataToR(gro, Sides.ALL);
		string titStr = getTitle("Histogram","");
		string rG = //rGraphString
			
			"hist(serie0, main='', lwd="+ gro.LineWidth +", xlab=colnames(data)[1], cex=1)\n" +
			"abline(v=mean(serie0, na.rm=T), lwd="+ gro.LineWidth +", lty=1, col='grey20')\n" +
			"abline(v=median(serie0, na.rm=T), lwd="+ gro.LineWidth +", lty=2, col='grey40')\n" +
			"mtext('avg', at=mean(serie0, na.rm=T), side=3, cex=.7, col='grey20')\n" +
			"mtext('median', at=median(serie0, na.rm=T), side=1, cex=.7, col='grey40')\n" +
			titStr;

		return allData + rG;
	}

	private string getRDotchartString(GraphROptions gro, string fileName) {
		string allData = convertDataToR(gro, Sides.ALL);
		string titStr = getTitle("Dotchart","");
		string rG = //rGraphString
			"dotchart(serie0, labels=rownames(data), xlab=colnames(data)[1], lwd="+ gro.LineWidth +", cex=1)\n" +
			"abline(v=mean(serie0, na.rm=T), lwd="+ gro.LineWidth +", lty=1, col='grey20')\n" +
			"abline(v=median(serie0, na.rm=T), lwd="+ gro.LineWidth +", lty=2, col='grey40')\n" +
			"mtext('avg', at=mean(serie0, na.rm=T), side=3, cex=.7, col='grey20')\n" +
			"mtext('median', at=median(serie0, na.rm=T), side=1, cex=.7, col='grey40')\n" +
			titStr;

		return allData + rG;
	}

	public enum Sides { ALL, LEFT, RIGHT };

	//currently only creates one customized graph	
	public bool CreateGraphR (string fileName, bool show, int graphNum) 
	{
		//only graph if there's data
		//TODO: check also later if none row is selected
		if(CurrentGraphData.XAxisNames.Count == 0) {
			return false;
		}
		if (!show) { //report
			string directoryName = Util.GetReportDirectoryName(fileName);

			/*
			string [] pngs = Directory.GetFiles(directoryName, "*.png");

			//if found 3 images, sure will be 1.png, 2.png and 3.png, next will be 4.png
			//there will be always a png with chronojump_logo
			fileName = directoryName+"/"+pngs.Length.ToString() + ".png";
			*/
			fileName = directoryName+"/"+(graphNum+1).ToString() + ".png";
		} else
			fileName = Path.GetTempPath()+"/"+fileName;
		//On win32 R understands backlash as an escape character and 
		//a file path uses Unix-like path separator '/'		
		fileName = fileName.Replace("\\","/");
		
		string cexAxisString = "";
		if(gRO.Type == Constants.GraphTypeBarplot || gRO.Type == Constants.GraphTypeLines) 
			cexAxisString = ", cex.axis=" + Util.ConvertToPoint(gRO.XAxisFontSize);
		
		string rString = "";
		if(UtilAll.IsWindows()) 
			rString = "library(\"Cairo\")\n" + 
				"Cairo(" + gRO.Width + ", " + gRO.Height + 
				", file = '" + fileName + "', type=\"png\", bg=\"white\")\n";
		else
			rString = "png(filename = '" + fileName + "'\n" + 
				" , width = " + gRO.Width + ", height = " + gRO.Height + ", units = 'px'\n" +
				" , pointsize = 12, bg = 'white', res = NA)\n";

		rString += "par(mar=c(" + gRO.MarginBottom + "," + gRO.MarginLeft + "," + 
			gRO.MarginTop + "," + gRO.MarginRight + ")" + cexAxisString + ")\n";

		if(gRO.Type == Constants.GraphTypeBoxplot) {
			if(hasTwoAxis()) {
				rString += "par(mfrow=c(1,2))\n";
				rString += getRBoxplotString(
						gRO, fileName, Sides.LEFT);
				rString += getRBoxplotString(
						gRO, fileName, Sides.RIGHT);
			}
			else
				rString += getRBoxplotString(
						gRO, fileName, Sides.ALL);
		}
		else if(gRO.Type == Constants.GraphTypeBarplot) {
			if(hasTwoAxis()) {
				rString += "par(mfrow=c(1,2))\n";
				rString += getRBarplotString(
						gRO, fileName, Sides.LEFT);
				rString += getRBarplotString(
						gRO, fileName, Sides.RIGHT);
			}
			else
				rString += getRBarplotString(
						gRO, fileName, Sides.ALL);
		}
		else if(gRO.Type == Constants.GraphTypeLines) {
			if(hasTwoAxis()) {
				rString += "par(mfrow=c(1,2))\n";
				rString += getRLinesString(
						gRO, fileName, Sides.LEFT);
				rString += getRLinesString(
						gRO, fileName, Sides.RIGHT);
			}
			else
				rString += getRLinesString(
						gRO, fileName, Sides.ALL);
		}
		else if(gRO.Type == Constants.GraphTypeStripchart) {
			if(hasTwoAxis()) {
				rString += "par(mfrow=c(2,1))\n";
				rString += getRStripchartString(
						gRO, fileName, Sides.LEFT);
				rString += getRStripchartString(
						gRO, fileName, Sides.RIGHT);
			}
			else
				rString += getRStripchartString(
						gRO, fileName, Sides.ALL);
		}
		else if(gRO.Type == Constants.GraphTypeXY)
			rString += getRXYString(gRO, fileName);
		else if(gRO.Type == Constants.GraphTypeHistogram)
			rString += getRHistogramString(gRO, fileName);
		else //if(CurrentGraphData.GraphType == Constants.GraphTypeDotchart))
			rString += getRDotchartString(gRO, fileName);
		
		rString += " dev.off()\n";

		//fileName = System.IO.Path.Combine(Path.GetTempPath(), fileName);

		string rScript = System.IO.Path.Combine(Path.GetTempPath(), graphNum+Constants.FileNameRScript);
		TextWriter writer = File.CreateText(rScript);
		writer.Write(rString);
		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();
		
		Util.RunRScript(rScript);

		if(show) {
			if(! File.Exists(fileName) || File.GetLastWriteTime(fileName) < File.GetLastWriteTime(rScript))
				new DialogMessage(Constants.MessageTypes.WARNING, 
						Catalog.GetString("Sorry. Error doing graph.") + 
						"\n" + Catalog.GetString("Maybe R is not installed.") + 
						"\n" + Catalog.GetString("Please, install it from here:") +
						"\n\n" + Constants.RmacDownload);
			else
				new DialogImageTest(Catalog.GetString("Chronojump Graph"), fileName, DialogImageTest.ArchiveType.FILE);
		}

		return true;
	}
			
		

	protected bool acceptCheckedData(int myData) {
		foreach(string marked in markedRows) {
			if(Convert.ToInt32(marked) == myData) {
				return true;
			}
		}
		return false;
	}

	//used only by RjEvolution in plotGraphGraphSeries, 
	//because rjevolution has a serie for TC and a serie for TF for each jumper
	int divideAndRoundDown (int myData) {
		if(myData == 0) { return 0;}
		
		if( Math.IEEERemainder( myData, 2) == 0.5) {
			//if the remainding of division between myData and 2 is .5, decrease .5
			return Convert.ToInt32(myData/2 -.5); 
		} else {
			return Convert.ToInt32(myData/2);
		}
	}


	public ArrayList Sessions {
		get { return sessions; }
	}

	public ArrayList MarkedRows {
		get { return markedRows; }
		//assigned for operating when a graph is the last stat made
		set { markedRows = value; } 
	}

	public ArrayList PersonsWithData {
		get { return personsWithData; }
	}

	public Gtk.Button FakeButtonRowCheckedUnchecked {
		get { return  fakeButtonRowCheckedUnchecked; }
	}

	public Gtk.Button FakeButtonRowsSelected {
		get { return  fakeButtonRowsSelected; }
	}
	
	public Gtk.Button FakeButtonNoRowsSelected {
		get { return  fakeButtonNoRowsSelected; }
	}

	//if we just made a graph, store is not made, 
	//and we cannot change the Male/female visualizations in the combo
	//with this we can assign a store to the graph (we assign the store of the last stat (not graph)
	public TreeStore Store {
		get { return store; }
		set { 
			store = value; 
			treeview = new TreeView();
			treeview.Model = store;
		}
	}

	~Stat() {}
}
