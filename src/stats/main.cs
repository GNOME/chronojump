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

/* ------------ CLASS HERENCY MAP ---------
 *
 * Stat
 * 	StatSjCmjAbk	
 *	StatDjTV	
 *	StatDjIndex	
 *		StatRjIndex
 *	StatPotencyAguado
 *	StatIE
 *		StatIUB
 * 	StatGlobal	//suitable for global and for a unique jumper
 *
 * ---------------------------------------
 */


public class Stat
{
	protected int sessionUniqueID;

	protected string sessionName;
	protected ArrayList sessions;
	protected int dataColumns; //for SimpleSession
	
	protected string jumpType;
	protected bool showSex;
	protected int statsJumpsType;
	protected int limit;

	protected TreeStore store;
	protected TreeIter iter;
	protected Gtk.TreeView treeview;

	protected static int prefsDigitsNumber;
	//protected static string manName = "M";
	//protected static string womanName = "F";

	//if this is not present i have problems like (No overload for method `xxx' takes `0' arguments) with some inherited classes
	public Stat () 
	{
		this.sessionName = "";
		this.showSex = false;
		this.statsJumpsType = 0;
		this.limit = 0;
	}

	public Stat (Gtk.TreeView treeview, ArrayList sessions, int newPrefsDigitsNumber, bool showSex, int statsJumpsType) 
	{
		sessionName = "nothing";
		if(sessions.Count > 1) {
			store = getStore(sessions.Count +3); //+3 (for the statName, the AVG horizontal and SD horizontal
		} else {
			store = getStore(sessions.Count +1);
		}
		treeview.Model = store;

		completeConstruction (treeview, sessions, newPrefsDigitsNumber, showSex, statsJumpsType);
		string [] columnsString = { "Jumper", "TV" };
		prepareHeaders(columnsString);
	}

	protected virtual void completeConstruction (Gtk.TreeView treeview, ArrayList sessions, int newPrefsDigitsNumber, bool showSex, int statsJumpsType)
	{
		this.sessions = sessions;
		this.treeview = treeview;
		prefsDigitsNumber = newPrefsDigitsNumber;
		this.showSex = showSex;
		this.statsJumpsType = statsJumpsType;

		iter = new TreeIter();
	}

	protected virtual void prepareHeaders(string [] columnsString) 
	{
		treeview.HeadersVisible=true;
		treeview.AppendColumn (Catalog.GetString(columnsString[0]), new CellRendererText(), "text", 0);

		int i;
		if(sessions.Count > 1) {
			string myHeaderString = "";
			string [] stringFullResults;
			for (i=0; i < sessions.Count ; i++) {
				//we need to know the name of the column: session
				stringFullResults = sessions[i].ToString().Split(new char[] {':'});
				myHeaderString = stringFullResults[1] + "\n" + 
					stringFullResults[2] + "\n" + Catalog.GetString(columnsString[1]); //name, date, col name
				treeview.AppendColumn (myHeaderString, new CellRendererText(), "text", i+1); 
			}
			//if multisession, add AVG and SD cols
			treeview.AppendColumn (Catalog.GetString("AVG"), new CellRendererText(), "text", i+1); 
			treeview.AppendColumn (Catalog.GetString("SD"), new CellRendererText(), "text", i+2);
		} else {
			treeview.AppendColumn (Catalog.GetString(columnsString[1]), new CellRendererText(), "text", 1); 
			//if there's only one session, add extra data columns if needed
			for(i=2 ; i <= dataColumns ; i++) {
				treeview.AppendColumn (columnsString[i], new CellRendererText(), "text", i);
			}
		}
	}
	
	protected TreeStore getStore (int columns)
	{
		//prepares the TreeStore for required columns
		Type [] types = new Type [columns];
		for (int i=0; i < columns; i++) {
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
	
	public virtual void prepareData () {
	}

	//called before ProcessDataSimpleSession, 
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
				rowFromSql[j] = trimDecimals(rowFromSql[j]);
			}
			//Console.WriteLine("r0 {0} r1 {1}", rowFromSql[0], rowFromSql[1]);
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
					sendAVG[j] = trimDecimals( (sumValue[j] /i).ToString() );
					sendSD[j] = trimDecimals( calculateSD(sumValue[j], sumSquaredValue[j], i) );
				}
				printData( sendAVG );
				printData( sendSD );
			}
		}
	}

	//one column by each session returned by SQL
	protected void processDataMultiSession (ArrayList arrayFromSql, bool makeAVGSD, int sessionsNum) 
	{
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
					sendRow[j+1] = trimDecimals(rowFromSql[2]); //put value from sql in the desired pos of sendRow

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
						sendAVG[j] = trimDecimals( (sumValue[j] /countRows[j]).ToString() );
						if(countRows[j] > 1) {
							sendSD[j] = trimDecimals( calculateSD(sumValue[j], sumSquaredValue[j], countRows[j]) );
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
				rowReturn[i] = trimDecimals( (sumValue /count).ToString() );
				if(count > 1) {
					rowReturn[i+1] = trimDecimals( calculateSD(sumValue, sumSquaredValue, count) );
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
		
	protected virtual void printData (string [] statValues) 
	{
			iter = store.AppendValues (statValues); 
	}


	//public virtual string ObtainEnunciate () {
	//}
		
	protected static string trimDecimals (string time) {
		//the +2 is a workarround for not counting the two first characters: "0."
		//this will not work with the fall
		
		return time.Length > prefsDigitsNumber + 2 ? 
			time.Substring( 0, prefsDigitsNumber + 2 ) : 
				time;
	}

	protected static double makeSquare (string myValueStr) {
		double myDouble;
		myDouble = Convert.ToDouble(myValueStr);
		return myDouble * myDouble;
	}
	
	protected static string calculateSD(double sumValues, double sumSquaredValues, int count) {
		if(count >1) {
			return (System.Math.Sqrt(
					sumSquaredValues -(sumValues*sumValues/count) / (count -1) )).ToString();
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


	public string SessionName
	{
		get { 
			return sessionName;
		}
	}
	
	public ArrayList Sessions
	{
		get { 
			return sessions;
		}
	}

	~Stat() {}
}
