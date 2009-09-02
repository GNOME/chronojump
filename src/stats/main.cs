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
 *		StatIUB
 * 	StatGlobal	//suitable for global and for a unique jumper
 *
 * ---------------------------------------
 */


public class Stat
{
	protected int sessionUniqueID;

	protected ArrayList sessions;
	protected int dataColumns; //for SimpleSession
	
	protected string jumpType;
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


	//for toString() in every stat
	protected string allValuesString = Catalog.GetString("All values");
	protected string avgValuesString = Catalog.GetString("Avg values of each jumper");
	
	protected int numContinuous; //for stats rj evolution
	
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
		pDN = myStatTypeStruct.PrefsDigitsNumber;
		this.showSex = myStatTypeStruct.Sex_active;
		this.statsJumpsType = myStatTypeStruct.StatsJumpsType;
		this.heightPreferred = myStatTypeStruct.HeightPreferred;
		this.weightStatsPercent = myStatTypeStruct.WeightStatsPercent;
		this.statsJumpsType = myStatTypeStruct.StatsJumpsType;
		this.limit = myStatTypeStruct.Limit;
		this.jumpType = myStatTypeStruct.StatisticApplyTo;
		
		this.markedRows = myStatTypeStruct.MarkedRows;
		
		this.gRO = myStatTypeStruct.GRO;
		this.toReport = myStatTypeStruct.ToReport;
		
		this.treeview = treeview;
		
		//initialize reportString
		reportString = "";

		iter = new TreeIter();

		personsWithData = new ArrayList();
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
			//Log.WriteLine("Added to markedRows row:{0}", rowToAdd);
		}
	}
	
	private void deleteRowFromMarkedRows(string rowToDelete)
	{
		int i = 0;
		foreach(string myRow in markedRows) {
			if(myRow == rowToDelete) {
				markedRows.RemoveAt(i);
				//Log..WriteLine("deleted from markedRows row:{0}", rowToDelete);
				break;
			}
			i++;
		}
	}
	
	
	void ItemToggled(object o, ToggledArgs args) {
		Log.WriteLine("Fake button will be pressed");
		fakeButtonRowCheckedUnchecked.Click();
		
		Log.WriteLine("Toggled");

		int column = 0;

		TreeIter iter;
		if (store.GetIter (out iter, new TreePath(args.Path)))
		{
			bool val = (bool) store.GetValue (iter, column);
			//Log.WriteLine ("toggled {0} with value {1}", args.Path, !val);

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
	
		/*	
		foreach(string myString in markedRows) {
			Log.Write(":" + myString);
		}
		Log.WriteLine();
		*/
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
						if(stringFull.Length > 1 && stringFull[1].StartsWith("M")) {
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
						if(stringFull.Length > 1 && stringFull[1].StartsWith("F")) {
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
	
	public virtual void PrepareData () {
	}


	private bool isThisRowMarked(int rowNum) {
		for(int k=0; k < markedRows.Count; k++) {
			//Log.WriteLine("{0}-{1}", Convert.ToInt32(markedRows[k]), rowNum);
			if(Convert.ToInt32(markedRows[k]) == rowNum) {
			//	Log.WriteLine("YES");
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
			Log.WriteLine("no rows Clicking in stats/main.cs simple session");
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
			Log.WriteLine("no rows Clicking in stats/main.cs multi session");
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
			Log.WriteLine("On graph or report (or graph, report)");
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
								//Log.WriteLine("value: {0}", store.GetValue(iter, column+2));
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
						//Log.WriteLine("j({0}), SendAVG[j]({1}), valuesList[j]({2})", j, sendAVG[j+1], valuesList[j]);
						sendSD[j+1] = Util.TrimDecimals( Util.CalculateSD(valuesList[j], sumValue[j], valuesOk[j]).ToString(), pDN );
						//Log.WriteLine("j({0}), SendSD[j]({1})", j, sendSD[j+1]);
					}
					printData( sendAVG );
					printData( sendSD );
				}
			}
		} catch {
			/* check this if it's needed now*/
			//write a row of AVG because graphs of stats with AVG and SD
			//are waiting the AVG row for ending and painting graph
			Log.WriteLine("catched!");
			string [] sendAVG = new string [myDataColumns +1];
			sendAVG[0] = Catalog.GetString("AVG");
			printData(sendAVG);
			Log.WriteLine("Graph should work!");
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
			//Log.WriteLine("REPORT: {0}", statValues[0]);
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
				//Log.WriteLine("FROM PRINTDATA Added to markedRows row:{0}", myPath.ToString());
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
			//Log.WriteLine("searching {0}, myArray[i] {1}, limit {2}", searching, myArray[i], limit);
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
	int rjEvolutionMaxJumps; //we should care of majjumps of the checked rjEvolution rows

	
	//temporary hack for a gtk# garbage collecting error
	//protected ArrayList onlyUsefulForNotBeingGarbageCollected = new ArrayList(); 
	//has to be static
	protected static ArrayList onlyUsefulForNotBeingGarbageCollected = new ArrayList(); 

	/*	
	private Grid addGrid() 
	{
		Grid myGrid = new Grid();
		myGrid.VerticalGridType = Grid.GridType.Coarse;
		Pen majorGridPen = new Pen( Color.LightGray );
		float[] pattern = {1.0f,2.0f};
		majorGridPen.DashPattern = pattern;
		myGrid.MajorGridPen = majorGridPen;
		return myGrid;
	}
	*/

	/*
	   //nplot will be converted to R
	public void CreateGraph () 
	{
		//only graph if there's data
		//TODO: check also later if none row is selected
		if(CurrentGraphData.XAxisNames.Count == 0) {
			return;
		}
		
		NPlot.Gtk.PlotSurface2D plot = new NPlot.Gtk.PlotSurface2D ();

		//create plot (same as below)
		plot.Clear();
		
		plot.Add(addGrid());

		plot.Title = CurrentGraphData.GraphTitle;
		int acceptedSeries = plotGraphGraphSeries (plot, 
				CurrentGraphData.XAxisNames.Count + 2, //xtics (+2 for left, right space)
				GraphSeries);
		if(acceptedSeries == 0) { return; }
		
		createAxisGraphSeries (plot, CurrentGraphData);

		writeLegend(plot);

		//put in window
		//fixes a gtk# garbage collecting bug
		onlyUsefulForNotBeingGarbageCollected.Add(plot);

		plot.Show ();
		
		Gtk.Window w = new Window (CurrentGraphData.WindowTitle);
		//put an icon to window
		UtilGtk.IconWindowGraph(w);
	
		int x = getSizeX();
		int y = getSizeY(x, acceptedSeries);
		w.SetDefaultSize (x,y);

		w.Add (plot);
		w.ShowAll ();
	}

	   //nplot will be converted to R
	public bool CreateGraph (string fileName) 
	{
		//only graph if there's data
		//TODO: check also later if none row is selected
		if(CurrentGraphData.XAxisNames.Count == 0) {
			return false;
		}
		
		NPlot.PlotSurface2D plot = new NPlot.PlotSurface2D ();

		//create plot (same as above)
		plot.Clear();
		
		plot.Add(addGrid());

		plot.Title = CurrentGraphData.GraphTitle;

		int acceptedSeries = plotGraphGraphSeries (plot, 
				CurrentGraphData.XAxisNames.Count + 2, //xtics (+2 for left, right space)
				GraphSeries);
		if(acceptedSeries == 0) { return false; }
		
		createAxisGraphSeries (plot, CurrentGraphData);

		writeLegend(plot);

		int x = getSizeX();
		int y = getSizeY(x, acceptedSeries);
		Bitmap b = new Bitmap (x, y);
		Graphics g = Graphics.FromImage (b);
		g.FillRectangle  (Brushes.White, 0, 0, x, y);
		Rectangle bounds = new Rectangle (0, 0, x, y);

		//save to file
		plot.Draw (g, bounds);
		string directoryName = Util.GetReportDirectoryName(fileName);
		string [] pngs = Directory.GetFiles(directoryName, "*.png");

		//if found 3 images, sure will be 1.png, 2.png and 3.png, next will be 4.png
		//there will be always a png with chronojump_logo
		b.Save (directoryName + "/" + pngs.Length.ToString() + ".png", ImageFormat.Png);

		return true;
	}
	*/

/* R TESTS */
		/*
		   if there's lot of data of only one variable:
		   dotchart(data, cex=.5)
		   */

		/*
		   xy graphs like this:
		   rang <- c(1:length(rownames(data)))
		   plot(dataDF$TC, dataDF$TV, pch=rang, col=rang)
		   legend(75,150 ,legend=rownames(data), pch=rang, col=rang, cex=.5)
		   */

		/*
		   two series of person X 4 jumpTypes

		   NEED TO ADJUST INITAL-END X on xaxis

		   serie0=c(.3, .5, .55, .6)
		   serie1=c(.32, .52, .55, .62)
		   maxVal=max(serie0,serie1)
		   people=c("joan", "pep")
		   jumpTypes=c("sj","Rocket","ABK","Free")

		   xNames = jumpTypes
		   legendNames = people

		   colors=rainbow(length(legendNames))
		   plot(serie0,xlim=c(0,length(xNames)+1), ylim=c(0,maxVal), pch=1, axes=FALSE, col=colors[1])
		   points(serie1, pch=2, col=colors[2])
		   axis(1, 1:4, xNames)
		   axis(2)
		   legend("topright", legend=legendNames, pch=c(1,2), cex=.5, col=colors)
		   
		   ---------------------
		   (transposed) two series of jumpTypes X 2 persons
		   best see how to transpose automatically

		   serie0=c(.3, .32)
		   serie1=c(.5, .52)
		   serie2=c(.55, .55)
		   serie3=c(.6, .62)
		   maxVal=max(serie0,serie1,serie2,serie3)
		   people=c("joan", "pep")
		   jumpTypes=c("sj","Rocket","ABK","Free")

		   xNames = people
		   legendNames = jumpTypes

		   colors=topo.colors(length(legendNames))
		   plot(serie0,xlim=c(0,length(xNames)+1), ylim=c(0,maxVal), pch=1, axes=FALSE, col=colors[1])
		   points(serie1, pch=2, col=colors[2])
		   points(serie2, pch=3, col=colors[3])
		   points(serie3, pch=4, col=colors[4])
		   axis(1, 1:2, xNames)
		   axis(2)
		   legend("topright", legend=legendNames, pch=c(1:4), cex=.5, col=colors)

		   ----------------------------------
		   nicer, with table

		   serie0=c(.3, .5, .55, .6)
		   serie1=c(.32, .52, .55, .62)
		   table <- rbind(serie0, serie1)
		   maxVal=max(table)
		   rownames(table)=c("joan", "pep")
		   colnames(table)=c("sj","Rocket","ABK","Free")

		#if transpose uncomment this:
		#table <-t(table)

		   colors=topo.colors(length(rownames(table)))
		   plot(table[1,1:length(colnames(table))],xlim=c(0,length(colnames(table))+1), ylim=c(0,maxVal), pch=1, axes=FALSE, col=colors[1])
		   for(i in 2:length(rownames(table))) 
		   	points(table[i,1:length(colnames(table))], pch=i, col=colors[i])
		  
		   axis(1, 1:length(colnames(table)), colnames(table))
		   axis(2)
		   legend("bottomright", legend=rownames(table), pch=c(1:length(rownames(table))), cex=.5, col=colors)

		   */




		/* 
		interessant:
		plot(table(rpois(100,5)), type = "h", col = "red", lwd=10, main="rpois(100,lambda=5)")
		*/




		/*
	> heights
	[1] 0.30 0.50 0.55 0.60
	> jumpTypes
	[1] "sj"     "Rocket" "ABK"    "Free"  
	> plot(heights, axes = FALSE)
	> axis(1, 1:4, jumpTypes)
	> axis(2)
	> box()
	> title(main='my tit', sub='subtit', cex.sub=0.75, font.sub=3, col.sub='red')
	*/

		/*
		   joan <- c(.2, .5, .6)
		   dayron <- c(.25, .65, .65)
		   pepa <- c(.1, .2, .21)
		   jumps <- cbind(joan, dayron, pepa)
		   rownames(jumps) <- c("SJ", "CMJ", "ABK")
		   barplot(jumps, beside=T, legend=rownames(jumps))
		   barplot(jumps, beside=T, legend=rownames(jumps), col=heat.colors(3))
		   barplot(jumps, beside=T, legend=rownames(jumps), col=gray.colors(3))
  		   barplot(jumps, beside=T, legend=rownames(jumps), col=rainbow(3))
		   barplot(jumps, beside=T, legend=rownames(jumps), col=topo.colors(3))
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
		foreach(GraphSerie serie in GraphSeries) {
			if(
					side == Sides.LEFT && ! serie.IsLeftAxis ||
					side == Sides.RIGHT && serie.IsLeftAxis) {
				continue;
			}
			//on XY only take two vars
			if(gro.Type == Constants.GraphTypeXY) {
				Log.WriteLine("groVarX: " + gro.VarX + " groVarY: " + gro.VarY + " tit: " + serie.Title);
				if(gro.VarX != serie.Title && gro.VarY != serie.Title)
					continue;
				else if (xyFirstFound == "") {
					if(gro.VarX == serie.Title)
						xyFirstFound = "x";
					else
						xyFirstFound = "y";
				}
			}


			rD += "serie" + count.ToString() + " <- c(";
			string sep = "";
			int count2=0;
			foreach(string val in serie.SerieData) {
				if(! acceptCheckedData(count2++))
					continue;
				if(val == "-")
					rD += sep + "0";
				else
					rD += sep + Util.ConvertToPoint(val);
				sep = ", ";
			}
			rD += ")\n";
			bD += sepSerie + "serie" + count.ToString();
			colNamesD += sepSerie + "'" + Util.RemoveTilde(serie.Title)  + "'";
			sepSerie = ", ";
			count ++;
		}

		bD += ")\n";
		colNamesD += ")\n";

		string rowNamesD = "rownames(data) <- c("; //rowNamesDataString
		string sep2 = "";
		for(int i=0; i < CurrentGraphData.XAxisNames.Count; i++) {
			if(! acceptCheckedData(i))
				continue;
			rowNamesD += sep2 + "'" + Util.RemoveTilde(CurrentGraphData.XAxisNames[i].ToString()) + "'";
			sep2 = ", ";
		}
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

		if(gro.Transposed && gro.Type != Constants.GraphTypeXY)
			allData += "data <- t(data)\n";
	
		return allData;	
	}

	string getTitle(string graphType) {
		return "title(main='" + CurrentGraphData.GraphTitle + " (" + graphType +")', sub='" + 
			CurrentGraphData.GraphSubTitle + "', cex.sub=0.75, font.sub=3, col.sub='grey30')\n";
	}

	private string getRBarplotString(GraphROptions gro, string fileName, Sides side) {
		string allData = convertDataToR(gro, side);
		
		string ylabStr = "";
		if(side == Sides.RIGHT) {
			if(CurrentGraphData.LabelRight != "")
				ylabStr = ", ylab='" + CurrentGraphData.LabelRight + "'";
		}
		else { //ALL or LEFT
			if(CurrentGraphData.LabelLeft != "")
				ylabStr = ", ylab='" + CurrentGraphData.LabelLeft + "'";
		}

		string rG = //rGraphString
		   	" colors=" + gro.Palette +"(length(rownames(data)))\n" +
			"barplot(data, beside=T, col=colors, las=2, xlab=''" + ylabStr + ")\n" +
			" legend('" + gro.Legend +"', legend=rownames(data), cex=.7, col=colors, pch=3)\n";
		
		//have an unique title for both graphs
		string titStr = getTitle("Barplot");
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
		string allData = convertDataToR(gro, side);
		
		string axesStr = "";
		string ylabStr = "";
		if(side == Sides.RIGHT) {
			axesStr = " axis(4)\n";
			if(CurrentGraphData.LabelRight != "")
				ylabStr = ", ylab='" + CurrentGraphData.LabelRight + "'";
		}
		else { //ALL or LEFT
			axesStr = " axis(2)\n";
			if(CurrentGraphData.LabelLeft != "")
				ylabStr = ", ylab='" + CurrentGraphData.LabelLeft + "'";
		}

		string rG = //rGraphString
		   	" colors=" + gro.Palette +"(length(rownames(data)))\n" +
		   	" plot(data[1,1:length(colnames(data))], type='b', xlim=c(0,length(colnames(data))+1), ylim=c(min(data),max(data)), pch=1, axes=FALSE, col=colors[1], xlab=''" + ylabStr + ")\n" +
			" if(length(rownames(data))>=2) {\n" +
			" 	for(i in 2:length(rownames(data)))\n" +
		   	" 		points(data[i,1:length(colnames(data))], type='b', pch=i, col=colors[i])\n" +
			" }\n" +
			" axis(1, 1:length(colnames(data)), colnames(data), las=2)\n" +
			axesStr + 
			" legend('" + gro.Legend +"', legend=rownames(data), pch=c(1:length(rownames(data))), cex=.7, col=colors)\n";
		
		//have an unique title for both graphs
		string titStr = getTitle("Lines");
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
		string titStr = getTitle("XY");
		string rG = //rGraphString
			"rang <- c(1:length(rownames(data)))\n" +
			"plot(serie0, serie1, pch=rang, col=rang, xlab='" + gro.VarX + "', ylab='" + gro.VarY + "')\n" +
			"legend('" + gro.Legend +"' ,legend=rownames(data), pch=rang, col=rang, cex=.7)\n" +
			titStr;

		return allData + rG;
	}

	private string getRDotchartString(GraphROptions gro, string fileName) {
		return "";
	}

	public enum Sides { ALL, LEFT, RIGHT };

	//currently only creates one customized graph	
	public bool CreateGraphR (string fileName, bool show) 
	{
		//only graph if there's data
		//TODO: check also later if none row is selected
		if(CurrentGraphData.XAxisNames.Count == 0) {
			return false;
		}
		if (!show) { //report
			string directoryName = Util.GetReportDirectoryName(fileName);
			string [] pngs = Directory.GetFiles(directoryName, "*.png");

			//if found 3 images, sure will be 1.png, 2.png and 3.png, next will be 4.png
			//there will be always a png with chronojump_logo
			fileName = directoryName + "/" + pngs.Length.ToString() + ".png";
		} else
			fileName = System.IO.Path.Combine(Path.GetTempPath(), fileName); 
	
		string rString = "png(filename = '" + fileName + "'\n" + 
			" , width = " + gRO.Width + ", height = " + gRO.Height + ", units = 'px'\n" +
			" , pointsize = 12, bg = 'white', res = NA)\n";

		if(gRO.Type == Constants.GraphTypeBarplot) {
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
		else if(gRO.Type == Constants.GraphTypeXY)
			rString += getRXYString(gRO, fileName);
		else //if(CurrentGraphData.GraphType == Constants.GraphTypeDotchart))
			rString += getRDotchartString(gRO, fileName);
		
		rString += " dev.off()\n";

		fileName = System.IO.Path.Combine(Path.GetTempPath(), fileName);

		string rScript = System.IO.Path.Combine(Path.GetTempPath(), Constants.FileNameRScript);
		TextWriter writer = File.CreateText(rScript);
		writer.Write(rString);
		writer.Flush();
		((IDisposable)writer).Dispose();
		
		Process r = Process.Start("R CMD BATCH " + rScript);
		r.WaitForExit();

		if(show) {
			if(! File.Exists(fileName) || File.GetLastWriteTime(fileName) < File.GetLastWriteTime(rScript))
				new DialogMessage(Constants.MessageTypes.WARNING, "Sorry. Error doing graph");
			else
				new DialogImageTest(Catalog.GetString("Chronojump Graph"), fileName);
		}

		return true;
	}
			
		
	/*	
	private int getSizeX() {
		int x;
		int xMultiplier = 50;
		int minimum = 300;
		int maximum = 800;
		
		if(isRjEvolution)
			x = rjEvolutionMaxJumps * xMultiplier;
		else
			x = markedRows.Count * xMultiplier;

		if(x < minimum)
			x = minimum;
		else if(x > maximum)
			x = maximum;
		
		return x;
	}

	//calculated using series number.
	//Also if x is big, then lots of data has to be plotted
	//is better to have a taller graph, for this reason
	//x value is used
	private int getSizeY(int x, int series) {
		//return (int) x * 3/4;
		int y;
		int yMultiplier = 150;
		int minimum = 300;
		int maximum = 600;
		y = series * yMultiplier;
		if(y < minimum)
			y = minimum;
		if(y < x*3/4)
			y = x*3/4;
		if(y > maximum)
			y = maximum;
		
		return y;
	}
	*/


	/*
	 * SAVED COMMENTED FOR HAVING A SAMPLE OF HISTOGRAMS
	 *
	protected int plotGraphSimplesessionJumps(IPlotSurface2D plot, ArrayList myValues)
	{
		HistogramPlot hp = new HistogramPlot();
		hp.BaseWidth = 0.4f;

		int xtics = myValues.Count;
		double[] myData = new double[xtics];

		hp.Label = "TF (seconds)";

		int count=0;
		for(int i=0; i < myValues.Count ; i++) {
			string [] jump = myValues[i].ToString().Split(new char[] {':'});
			
			int j=0;
			foreach (string myValue in jump) 
			{
				if(j>0) {
					myData[count] = Convert.ToDouble(myValue);
					//Log.WriteLine("count {0}, myData {1}", count, myData[count]);
					count ++;
				}
				j++;
			}
		}
		hp.DataSource = myData;
		hp.BrushOrientation = HistogramPlot.BrushOrientations.HorizontalFadeIn;
		hp.Filled = true;
			
		hp.Pen = Pens.Red;
		plot.Add(hp);
		
		//return the number of plotted bars 
		return count;
	}
*/

	protected bool acceptCheckedData(int myData) {
		foreach(string marked in markedRows) {
			if(Convert.ToInt32(marked) == myData) {
				return true;
			}
		}
		return false;
	}

	/*	
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
	*/

	/*	
	protected virtual int plotGraphGraphSeries (IPlotSurface2D plot, int xtics, ArrayList allSeries)
	{
		rjEvolutionMaxJumps = -1;
		
		int acceptedSerie = 0;
		int countSerie = 0;
		
		foreach(GraphSerie mySerie in allSeries) 
		{
			//in isRjEvolution then check it this serie will be shown (each jumper has a TC and a TF serie)
			if( isRjEvolution && ! acceptCheckedData( divideAndRoundDown(countSerie)) ) {
				countSerie ++;
				continue;
			
			}
			//in multisession if a stats row is unchecked, jump to next iteration
			else if( sessions.Count > 1 && ! acceptCheckedData(countSerie) ) {
				countSerie ++;
				continue;
			}
			
			
			//xtics value is all rows +2 (left & right space)
			//lineData should contain xtics but without the rows thar are not in markedRows
			//Log.WriteLine("{0}:{1}:{2}", xtics, markedRows.Count, xtics-( (xtics-2)-(markedRows.Count) ) );
			double[] lineData;
			if(sessions.Count == 1 && !isRjEvolution) {
				//in single session lineData should contain all rows from stats except unchecked
				lineData = new double[ xtics-( (xtics-2)-(markedRows.Count) ) ];
			} else {
				//in multisession lineData does not contain rows from stats, it contains sessions name
				lineData = new double[ xtics ];
			}
			
			Marker m = mySerie.SerieMarker;
		
			PointPlot pp;
			LinePlot lp;

			pp = new PointPlot( m );
			pp.Label = mySerie.Title; 
			lp = new LinePlot();
			lp.Label = mySerie.Title; 
			lp.Color = mySerie.SerieColor;

			//left margin
			lineData[0] = double.NaN;
	
			int added=1;
			int counter=0;
			foreach (string myValue in mySerie.SerieData) 
			{
				//TODO: check this:
				//don't graph AVG and SD right cols in multisession
				if ( counter >= xtics -2 ) {
					break;
				}	
				
				//in single session lineData should contain all rows from stats except unchecked
				if(sessions.Count == 1 && !isRjEvolution && ! acceptCheckedData(counter) ) {
					counter ++;
					continue;
				}
				
				if(myValue == "-") {
					lineData[added++] = double.NaN;
				} else {
					lineData[added++] = Convert.ToDouble(myValue);
				}
				counter++;

				//Log.WriteLine("linedata :" + mySerie +":" + myValue);

				if(isRjEvolution && myValue != "-" && added -1 > rjEvolutionMaxJumps) {
					rjEvolutionMaxJumps = added -1;
				}
			}
			
			//right margin
			lineData[added] = double.NaN;
			
			lp.DataSource = lineData;
			pp.OrdinateData = lineData;
			pp.AbscissaData = new StartStep( 0, 1 ); //ini 0, step 1 (ini 0 because in lineData we start with blank value)
			lp.ShowInLegend = false;
				
			if(mySerie.IsLeftAxis) {
				plot.Add( lp );
				plot.Add( pp );
			} else {
				plot.Add( lp, NPlot.PlotSurface2D.XAxisPosition.Bottom, NPlot.PlotSurface2D.YAxisPosition.Right );
				plot.Add( pp, NPlot.PlotSurface2D.XAxisPosition.Bottom, NPlot.PlotSurface2D.YAxisPosition.Right );
			}
				
			// plot AVG 
			if(mySerie.Avg != 0) {
				HorizontalLine hl1 = new HorizontalLine(mySerie.Avg, mySerie.SerieColor);
				//Log.WriteLine("serie.AVG: {0}", mySerie.Avg);
				hl1.ShowInLegend = false;
				hl1.Pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
				//hl1.Pen.Width = 2F;
				if(mySerie.IsLeftAxis) {
					plot.Add( hl1 );
				} else {
					plot.Add( hl1, NPlot.PlotSurface2D.XAxisPosition.Bottom, NPlot.PlotSurface2D.YAxisPosition.Right );
				}

			}

			acceptedSerie ++;
			countSerie ++;
		}
		return acceptedSerie; //for knowing if a serie was accepted, and then createAxisGraphSeries
	}
	*/

	/*
	protected void createAxisGraphSeries (IPlotSurface2D plot, GraphData graphData)
	{
		LabelAxis la = new LabelAxis( plot.XAxis1 );
		int added=1;
		int counter=0;
		foreach (string name in graphData.XAxisNames) {
			if(sessions.Count == 1 && !isRjEvolution && !acceptCheckedData(counter)) {
				//in single session lineData should contain all rows from stats except unchecked
				counter ++;
				continue;
			}
			la.AddLabel( name, added++ );
			counter ++;
		}
		la.WorldMin = 0.7f;
		
		if(isRjEvolution) {
			la.WorldMax = rjEvolutionMaxJumps + .3f;
		} else {
			if(sessions.Count == 1) {
				//in single session lineData should contain all rows from stats except unchecked
				la.WorldMax = graphData.XAxisNames.Count-(graphData.XAxisNames.Count-(markedRows.Count)) + .3f;
			} else {
				la.WorldMax = graphData.XAxisNames.Count + .3f;
			}
		}
		plot.XAxis1 = la;
		//plot.XAxis1.LargeTickSize = 0.0f;
		plot.XAxis1.TicksLabelAngle = 35.0f;
		if(graphData.LabelBottom != "")
			la.Label = graphData.LabelBottom;
	
		LinearAxis ly1 = new LinearAxis();
		if(graphData.LabelLeft != "") {
			try {
				ly1 = createLinearAxis(graphData.LabelLeft, plot.YAxis1, graphData.IsLeftAxisInteger);
			} catch {
				//on stats global inter-session maybe cannot create axis because maybe there's no data in seconds like sj, cmj, abk
			}
		}
		
		LinearAxis ly2 = new LinearAxis();
		if(graphData.LabelRight != "") {
			try {
				ly2 = createLinearAxis(graphData.LabelRight, plot.YAxis2, graphData.IsRightAxisInteger);
			} catch {
				//on stats global inter-session maybe cannot create axis because maybe there's no data in % like dj indexes, ...
			}
		}
	}

	private LinearAxis createLinearAxis(string label, Axis myAxis, bool isInt) {
		LinearAxis ly = (LinearAxis) myAxis;
		ly.Label = label;
		if(isInt) {
			ly.LargeTickStep = 1;
			ly.NumberOfSmallTicks = 0;
		}
		return ly;
	}
	
	protected void writeLegend(IPlotSurface2D plot)
	{	
		plot.Legend = new Legend();
		plot.Legend.XOffset = +30;
	}
	*/	

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
