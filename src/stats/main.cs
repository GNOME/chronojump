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

using NPlot.Gtk;
using NPlot;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO; 	//TextWriter

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

	protected static int pDN; //prefsDigitsNumber;
	//protected static string manName = "M";
	//protected static string womanName = "F";

	protected ArrayList markedRows;

	protected bool toReport = false;
	protected string reportString;


	//for toString() in every stat
	protected string allValuesString = "All values";
	protected string avgValuesString = "Avg values of each jumper";
	
	protected int numContinuous; //for stats rj evolution
	
	//for raise a signal and manage it on src/gui/stats.cs
	//signal will be catched first in src/statType.cs and there a equal signal will be raised
	public Gtk.Button fakeButtonRowCheckedUnchecked;
	public Gtk.Button fakeButtonRowsSelected;
	public Gtk.Button fakeButtonNoRowsSelected;
	
	//private bool selectedMakeAVGSD;


	public Stat () 
	{
		fakeButtonRowCheckedUnchecked = new Gtk.Button();
		fakeButtonRowsSelected = new Gtk.Button();
		fakeButtonNoRowsSelected = new Gtk.Button();
	}

	protected void completeConstruction (StatTypeStruct myStatTypeStruct, Gtk.TreeView treeview)
	{
		this.sessions = myStatTypeStruct.SendSelectedSessions;
		pDN = myStatTypeStruct.PrefsDigitsNumber;
		this.showSex = myStatTypeStruct.Sex_active;
		this.statsJumpsType = myStatTypeStruct.StatsJumpsType;
		this.heightPreferred = myStatTypeStruct.HeightPreferred;
		this.statsJumpsType = myStatTypeStruct.StatsJumpsType;
		this.limit = myStatTypeStruct.Limit;
		this.jumpType = myStatTypeStruct.StatisticApplyTo;
		
		this.markedRows = myStatTypeStruct.MarkedRows;
		
		this.toReport = myStatTypeStruct.ToReport;
		
		this.treeview = treeview;
		
		//initialize reportString
		reportString = "";

		iter = new TreeIter();
	}
	
	void createCheckboxes(TreeView tv) 
	{
		CellRendererToggle rendererToggle = new CellRendererToggle ();
		rendererToggle.Xalign = 0.0f;
		GLib.Object ugly = (GLib.Object) rendererToggle;
		ugly.Data ["column"] = 0;
		rendererToggle.Toggled += new ToggledHandler (ItemToggled);
		rendererToggle.Activatable = true;
		rendererToggle.Active = true;

		TreeViewColumn column = new TreeViewColumn ("", rendererToggle, "active", 0);
		column.Sizing = TreeViewColumnSizing.Fixed;
		column.FixedWidth = 50;
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
			//Console.WriteLine("Added to markedRows row:{0}", rowToAdd);
		}
	}
	
	private void deleteRowFromMarkedRows(string rowToDelete)
	{
		int i = 0;
		foreach(string myRow in markedRows) {
			if(myRow == rowToDelete) {
				markedRows.RemoveAt(i);
				//Console..WriteLine("deleted from markedRows row:{0}", rowToDelete);
				break;
			}
			i++;
		}
	}
	
	
	void ItemToggled(object o, ToggledArgs args) {
		Console.WriteLine("Fake button will be pressed");
		fakeButtonRowCheckedUnchecked.Click();
		
		Console.WriteLine("Toggled");

		GLib.Object cellRendererToggle = (GLib.Object) o;
		int column = (int) cellRendererToggle.Data["column"];

		Gtk.TreeIter iter;
		if (store.GetIterFromString (out iter, args.Path))
		{
			bool val = (bool) store.GetValue (iter, column);
			Console.WriteLine ("toggled {0} with value {1}", args.Path, !val);

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
		
		foreach(string myString in markedRows) {
			Console.Write(":" + myString);
		}
		Console.WriteLine();
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
					stringFullResults[2] + "\n" + Catalog.GetString(columnsString[1]); //name, date, col name
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
					stringFullResults[2] + "\n" + Catalog.GetString(columnsString[1]) + "</TH>"; //name, date, col name
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
	
	protected string obtainSessionSqlString(ArrayList sessions)
	{
		string newStr = "WHERE (";
		for (int i=0; i < sessions.Count; i++) {
			string [] stringFullResults = sessions[i].ToString().Split(new char[] {':'});
			newStr = newStr + " sessionID == " + stringFullResults[0];
			if (i+1 < sessions.Count) {
				newStr = newStr + " OR ";
			}
		}
		newStr = newStr + ") ";
		return newStr;		
	}
	
	public virtual void PrepareData () {
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
		//record makeavgsd for using in checkboxes for not being selected
		//selectedMakeAVGSD = makeAVGSD;
	
		string [] rowFromSql = new string [dataColumns +1];
		double [] sumValue = new double [dataColumns +1];
		double [] sumSquaredValue = new double [dataColumns +1];
		int i;

		//process all SQL results line x line
		for (i=0 ; i < arrayFromSql.Count ; i ++) {
			rowFromSql = arrayFromSql[i].ToString().Split(new char[] {':'});
			
			for (int j=1; j <= dataColumns ; j++) {
				if(makeAVGSD) {
					sumValue[j] += Convert.ToDouble(rowFromSql[j]);
					sumSquaredValue[j] += makeSquare(rowFromSql[j]);
				}
				rowFromSql[j] = Util.TrimDecimals(rowFromSql[j], pDN);
			}
			printData( rowFromSql );
		}
		//only show the row if sqlite returned values
		if(i > 0)
		{
			if(makeAVGSD) {
				//printData accepts two cols: name, values (values separated again by ':')
				string [] sendAVG = new string [dataColumns +1];
				string [] sendSD = new string [dataColumns +1];

				sendAVG[0] = Catalog.GetString("AVG");
				sendSD[0] =  Catalog.GetString("SD");

				for (int j=1; j <= dataColumns; j++) {
					sendAVG[j] = Util.TrimDecimals( (sumValue[j] /i).ToString(), pDN );
					sendSD[j] = Util.TrimDecimals( calculateSD(sumValue[j], sumSquaredValue[j], i), pDN );
				}
				printData( sendAVG );
				printData( sendSD );
			}
		} else {
			//if we cannot access the treeview, also don't allow to graph or report
			Console.WriteLine("no rows Clicking in stats/main.cs simple session");
			fakeButtonNoRowsSelected.Click();
		}
	}

	//one column by each session returned by SQL
	protected void processDataMultiSession (ArrayList arrayFromSql, bool makeAVGSD, int sessionsNum) 
	{
		//record makeavgsd for using in checkboxes for not being selected
		//selectedMakeAVGSD = makeAVGSD;
		
		string [] rowFromSql = new string [sessionsNum +1];
		double [] sumValue = new double [sessionsNum +1];
		double [] sumSquaredValue = new double [sessionsNum +1];
		string [] sendRow = new string [sessionsNum +1];
		//int [] countRows = new int [sessions.Count +1]; //count the number of valid cells (rows) for make the AVG
		int [] countRows = new int [sessionsNum +1]; //count the number of valid cells (rows) for make the AVG
		int i;
		
		//initialize values
		for(int j=1; j< sessionsNum+1 ; j++) {
			sendRow[j] = "-";
			sumValue[j] = 0;
			sumSquaredValue[j] = 0;
			countRows[j] = 0;
		}
		string oldStat = "-1";
	
		//process all SQL results line x line
		for (i=0 ; i < arrayFromSql.Count ; i ++) {
			rowFromSql = arrayFromSql[i].ToString().Split(new char[] {':'});

			//if (sessionsNum == 1 || rowFromSql[0] != oldStat) {
			if (rowFromSql[0] != oldStat) {
				//print the values, except on the first iteration
				if (i>0) {
					printData( calculateRowAVGSD(sendRow) );
				}
				
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

					if(makeAVGSD) {
						sumValue[j+1] += Convert.ToDouble(rowFromSql[2]);
						sumSquaredValue[j+1] += makeSquare(rowFromSql[2]);
						countRows[j+1] ++;
					}
				}
			}
			oldStat = sendRow[0];
		}
		
		//only show the row if sqlite returned values
		if(i > 0)
		{
			printData( calculateRowAVGSD(sendRow) );

			if(makeAVGSD) {
				//printData accepts two cols: name, values (values separated again by ':')
				string [] sendAVG = new string [sessions.Count +1];
				string [] sendSD = new string [sessions.Count +1];

				sendAVG[0] = Catalog.GetString("AVG");
				sendSD[0] =  Catalog.GetString("SD");

				for (int j=1; j <= sessions.Count; j++) {
					if(countRows[j] > 0) {
						sendAVG[j] = Util.TrimDecimals( (sumValue[j] /countRows[j]).ToString(), pDN );
						if(countRows[j] > 1) {
							sendSD[j] = Util.TrimDecimals( calculateSD(sumValue[j], sumSquaredValue[j], countRows[j]), pDN );
						} else {
							sendSD[j] = "-";
						}
						//Console.WriteLine("sumValue: {0}, sumSquaredValue: {1}, countRows[j]: {2}, j: {3}", 
						//		sumValue[j], sumSquaredValue[j], countRows[j], j);
					} else {
						sendAVG[j] = "-";
						sendSD[j] = "-";
					}
				}
				printData( calculateRowAVGSD(sendAVG) );
				printData( calculateRowAVGSD(sendSD) );
			}
		} else {
			//if we cannot access the treeview, also don't allow to graph or report
			Console.WriteLine("no rows Clicking in stats/main.cs multi session");
			fakeButtonNoRowsSelected.Click();
		}
			
	}

	//returns a row with it's AVG and SD in last two columns
	protected string [] calculateRowAVGSD(string [] rowData) 
	{
		string [] rowReturn = new String[sessions.Count +3];
		int count =0;
		double sumValue = 0;
		double sumSquaredValue = 0;
	
		if(sessions.Count > 1) {
			int i=0;
			for (i=0; i < sessions.Count + 1; i++) {
				rowReturn[i] = rowData[i];
				if(i>0 && rowReturn[i] != "-") { //first column is text
					count++;
					sumValue += Convert.ToDouble(rowReturn[i]); 
					sumSquaredValue += makeSquare(rowReturn[i]); 
				}
			}
			if(count > 0) {
				rowReturn[i] = Util.TrimDecimals( (sumValue /count).ToString(), pDN );
				if(count > 1) {
					rowReturn[i+1] = Util.TrimDecimals( calculateSD(sumValue, sumSquaredValue, count), pDN );
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
	//private int rowsPassedToReport = 1;
	private int rowsPassedToReport = 0;
	
	protected virtual void printData (string [] statValues) 
	{
		if(toReport) {
			bool allowedRow = false;
			for(int i=0; i < markedRows.Count; i++) {
				if(Convert.ToInt32(markedRows[i]) == rowsPassedToReport) {
					allowedRow = true;
					break;
				}
			}
			if(allowedRow) {
				reportString += "<TR>";
				for (int i=0; i < statValues.Length ; i++) {
					reportString += "<TD>" + statValues[i] + "</TD>";
				}
				reportString += "</TR>\n";
			}
			rowsPassedToReport ++;
		} else {
			iter = new TreeIter();
			
			//iter = store.Append (iter);	//doesn't work
			store.Append (out iter);	//add new row and make iter point to it
		
			//addAllNoneIfNeeded(statValues.Length);
			
			TreePath myPath = store.GetPath(iter); 
			
			if(statValues[0] != Catalog.GetString("AVG") && statValues[0] != Catalog.GetString("SD")) {
				store.SetValue(iter, 0, true);	//first col is true if it's not AVG or SD
				markedRows.Add(myPath.ToString());
				//Console.WriteLine("FROM PRINTDATA Added to markedRows row:{0}", myPath.ToString());
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
	
	protected static string calculateSD(double sumValues, double sumSquaredValues, int count) {
		if(count >1) {
			//return (System.Math.Sqrt(
			//		sumSquaredValues -(sumValues*sumValues/count) / (count -1) )).ToString();
			return "-";
		} else {
			return "-";
		}
	}
	
	//true if found equal or more than 'limit' occurrences of 'searching' in array
	protected static bool nFoundInArray (string searching, ArrayList myArray, int limit) 
	{
		int count = 0;
		for (int i=0; i< myArray.Count && count <= limit ; i ++) {
			//Console.WriteLine("searching {0}, myArray[i] {1}, limit {2}", searching, myArray[i], limit);
			if (searching == myArray[i].ToString()) {
				count ++;
			}
		}
		if(count >= limit) {
			return true;
		}
		return false;
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
	

	public void CreateGraph () 
	{
		//only graph if there's data
		//TODO: check also later if none row is selected
		if(CurrentGraphData.XAxisNames.Count == 0) {
			return;
		}
		
		int x = 500;
		int y= 400;

		Gtk.Window w = new Window (CurrentGraphData.WindowTitle);

		w.SetDefaultSize (x, y);
		NPlot.Gtk.PlotSurface2D plot = new NPlot.Gtk.PlotSurface2D ();

		//create plot (same as below)
		plot.Clear();
		plot.Title = CurrentGraphData.GraphTitle;
		int acceptedSeries = plotGraphGraphSeries (plot, 
				CurrentGraphData.XAxisNames.Count + 2, //xtics (+2 for left, right space)
				GraphSeries);
		if(acceptedSeries == 0) { return; }
		
		createAxisGraphSeries (plot, CurrentGraphData);

		writeLegend(plot);
		plot.Add( new Grid() );

		//put in window
		//fixes a gtk# garbage collecting bug
		onlyUsefulForNotBeingGarbageCollected.Add(plot);

		plot.Show ();
		w.Add (plot);
		w.ShowAll ();
	}

	public bool CreateGraph (string fileName) 
	{
		//only graph if there's data
		//TODO: check also later if none row is selected
		if(CurrentGraphData.XAxisNames.Count == 0) {
			return false;
		}
		
		int x = 500;
		int y= 400;

		NPlot.PlotSurface2D plot = new NPlot.PlotSurface2D ();
		Bitmap b = new Bitmap (x, y);
		Graphics g = Graphics.FromImage (b);
		g.FillRectangle  (Brushes.White, 0, 0, x, y);
		Rectangle bounds = new Rectangle (0, 0, x, y);

		//create plot (same as above)
		plot.Clear();
		plot.Title = CurrentGraphData.GraphTitle;
		int acceptedSeries = plotGraphGraphSeries (plot, 
				CurrentGraphData.XAxisNames.Count + 2, //xtics (+2 for left, right space)
				GraphSeries);
		if(acceptedSeries == 0) { return false; }
		
		createAxisGraphSeries (plot, CurrentGraphData);

		writeLegend(plot);
		plot.Add( new Grid() );

		//save to file
		plot.Draw (g, bounds);
		string directoryName = Util.GetReportDirectoryName(fileName);
		string [] pngs = Directory.GetFiles(directoryName, "*.png");

		//if found 3 images, sure will be 1.png, 2.png and 3.png, next will be 4.png
		//there will be always a png with chronojump_logo
		b.Save (directoryName + "/" + pngs.Length.ToString() + ".png", ImageFormat.Png);

		return true;
	}
			

	/*
	 * SAVED COMMENTED FOR HAVING A SAMPLE OF HISTOGRAMS
	 *
	protected int plotGraphSimplesessionJumps(IPlotSurface2D plot, ArrayList myValues)
	{
		HistogramPlot hp = new HistogramPlot();
		hp.BaseWidth = 0.4f;

		int xtics = myValues.Count;
		double[] myData = new double[xtics];

		hp.Label = "TV (seconds)";

		int count=0;
		for(int i=0; i < myValues.Count ; i++) {
			string [] jump = myValues[i].ToString().Split(new char[] {':'});
			
			int j=0;
			foreach (string myValue in jump) 
			{
				if(j>0) {
					myData[count] = Convert.ToDouble(myValue);
					//Console.WriteLine("count {0}, myData {1}", count, myData[count]);
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

	bool acceptCheckedData(int myData) {
		foreach(string marked in markedRows) {
			if(Convert.ToInt32(marked) == myData) {
				return true;
			}
		}
		return false;
	}
	
	//used only by RjEvolution in plotGraphGraphSeries, 
	//because rjevolution has a serie for TC and a serie for TV for each jumper
	int divideAndRoundDown (int myData) {
		if(myData == 0) { return 0;}
		
		if( Math.IEEERemainder( myData, 2) == 0.5) {
			//if the remainding of division between myData and 2 is .5, decrease .5
			return Convert.ToInt32(myData/2 -.5); 
		} else {
			return Convert.ToInt32(myData/2);
		}
	}
	
	protected int plotGraphGraphSeries (IPlotSurface2D plot, int xtics, ArrayList allSeries)
	{
		rjEvolutionMaxJumps = -1;
		
		int acceptedSerie = 0;
		int countSerie = 0;
		foreach(GraphSerie mySerie in allSeries) 
		{
			
			//in isRjEvolution then check it this serie will be shown (each jumper has a TC and a TV serie)
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
			Console.WriteLine("{0}:{1}:{2}", xtics, markedRows.Count, xtics-( (xtics-2)-(markedRows.Count) ) );
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

				//Console.WriteLine("linedata :" + mySerie +":" + myValue);

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
				
			acceptedSerie ++;
			countSerie ++;
		}
		return acceptedSerie; //for knowing if a serie was accepted, and then createAxisGraphSeries
	}
		
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
	
		if(graphData.LabelLeft != "") {
			LinearAxis ly1 = (LinearAxis)plot.YAxis1;
			ly1.Label = graphData.LabelLeft;
		}
		
		if(graphData.LabelRight != "") {
			LinearAxis ly2 = (LinearAxis)plot.YAxis2;
			ly2.Label = graphData.LabelRight;
		}
	}
	
	protected void writeLegend(IPlotSurface2D plot)
	{	
		plot.Legend = new Legend();
		plot.Legend.XOffset = +30;
	}
	

	public ArrayList Sessions {
		get { return sessions; }
	}

	public ArrayList MarkedRows {
		get { return markedRows; }
		//assigned for operating when a graph is the last stat made
		set { markedRows = value; } 
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
