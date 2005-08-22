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


public class TreeViewRuns
{
	protected bool sortByType;
	protected TreeStore store;
	protected Gtk.TreeView treeview;
	//protected static int pDN; //prefsDigitsNumber;
	protected int pDN; //prefsDigitsNumber;
	//protected static bool metersSecondsPreferred;
	protected bool metersSecondsPreferred;
	protected static string allRunsName = Catalog.GetString("All runs");
	protected int runIDColumn;
	
	public TreeViewRuns ()
	{
	}
	
	public TreeViewRuns (Gtk.TreeView treeview, bool sortByType, int newPrefsDigitsNumber, bool metersSecondsPreferred)
	{
		this.treeview = treeview;
		this.sortByType = sortByType;
		//pDN = newPrefsDigitsNumber;
		this.pDN = newPrefsDigitsNumber;
		this.metersSecondsPreferred = metersSecondsPreferred;

		string runnerName = Catalog.GetString("Runner");
		string speedName = Catalog.GetString("Speed");
		string distanceName = Catalog.GetString("Distance");
		string timeName = Catalog.GetString("Time");

		store = getStore(5); //because, runID is not show in last col
		string [] columnsString = { runnerName, speedName, distanceName, timeName };
		treeview.Model = store;
		prepareHeaders(columnsString);
	
		runIDColumn = 4;
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
	
	protected virtual void prepareHeaders(string [] columnsString) 
	{
		treeview.HeadersVisible=true;
		int i=0;
		foreach(string myCol in columnsString) {
			treeview.AppendColumn (Catalog.GetString(myCol), new CellRendererText(), "text", i++);
		}
	}
	
	public virtual void RemoveColumns() {
		Gtk.TreeViewColumn [] myColumns = treeview.Columns;
		foreach (Gtk.TreeViewColumn column in myColumns) {
			treeview.RemoveColumn (column);
		}
	}

	public virtual void Fill(string [] myRuns, string filter)
	{
		TreeIter iter = new TreeIter();

		string tempRunner = ":"; //one value that's not possible
	
		string myType;
		double speed;

		foreach (string run in myRuns) {
			string [] myStringFull = run.Split(new char[] {':'});

			//show always the names of runners ...
			if(tempRunner != myStringFull[0])
			{
				iter = store.AppendValues (myStringFull[0]);
				tempRunner = myStringFull[0];
			}
			
			if(metersSecondsPreferred) {
				speed = Convert.ToDouble( myStringFull[5].ToString() ) /
					Convert.ToDouble( myStringFull[6].ToString() );
			} else {
				speed = ( Convert.ToDouble( myStringFull[5].ToString() ) /
					Convert.ToDouble( myStringFull[6].ToString() ) ) * 3.6;
			}

			//... but if we selected one type of run and this it's not the type, don't show
			if(filter == allRunsName || filter == myStringFull[4]) {

				myType = myStringFull[4];
				
				store.AppendValues (iter, 
						myType, 
						Util.TrimDecimals( speed.ToString(), pDN ),
						Util.TrimDecimals( myStringFull[5].ToString(), pDN ), //distance
						Util.TrimDecimals( myStringFull[6].ToString(), pDN ), //time
						myStringFull[1] //runUniqueID (not shown) 
						);
			}
		}	
	}
	
	public virtual void Add (string runnerName, Run newRun)
	{
		TreeIter iter = new TreeIter();
		bool modelNotEmpty = treeview.Model.GetIterFirst ( out iter ) ;
		string iterRunnerString;
		bool found = false;
	
		if(modelNotEmpty) {
			do {
				iterRunnerString = ( treeview.Model.GetValue (iter, 0) ).ToString();
				if(iterRunnerString == runnerName) {
					found = true;

					//expand the runner
					treeview.ExpandToPath( treeview.Model.GetPath(iter) );

					store.AppendValues ( iter, newRun.Type, 
							Util.TrimDecimals(newRun.Speed.ToString(), pDN), 
							Util.TrimDecimals(newRun.Distance.ToString(), pDN), 
							Util.TrimDecimals(newRun.Time.ToString(), pDN), 
							newRun.UniqueID.ToString() );
				}
			} while (treeview.Model.IterNext (ref iter));
		}

		//if the runner has not run in this session, it's name doesn't appear in the treeview
		//create the name, and write the run
		if(! found) {
			iter = store.AppendValues (runnerName);
	
			store.AppendValues ( iter, newRun.Type, 
					Util.TrimDecimals(newRun.Speed.ToString(), pDN), 
					Util.TrimDecimals(newRun.Distance.ToString(), pDN), 
					Util.TrimDecimals(newRun.Time.ToString(), pDN), 
					newRun.UniqueID.ToString() );
			
			//expand the runner
			treeview.ExpandToPath( treeview.Model.GetPath(iter) );
		}
	}
	
	public virtual void DelRun (int runID)
	{
		TreeIter iter = new TreeIter();
		treeview.Model.GetIterFirst ( out iter ) ;
		
		do {
			if( treeview.Model.IterHasChild(iter) ) {
				treeview.Model.IterChildren (out iter, iter);
				do {
					int iterRunID =  Convert.ToInt32 ( treeview.Model.GetValue (iter, runIDColumn) );
					if(iterRunID == runID) {
						store.Remove(ref iter);
						return;
					}
				} while (treeview.Model.IterNext (ref iter));
				treeview.Model.IterParent (out iter, iter);
			}
		} while (treeview.Model.IterNext (ref iter));
	}

	public int RunSelectedID {
		get {
			TreeIter iter = new TreeIter();
			TreeModel myModel = treeview.Model;
			if (treeview.Selection.GetSelected (out myModel, out iter)) {
				return Convert.ToInt32 ( treeview.Model.GetValue(iter, runIDColumn) );
			} else {
				return 0;
			}
		}
	}
	
}

public class TreeViewRunsInterval : TreeViewRuns
{
	int colsNum;
	
	public TreeViewRunsInterval (Gtk.TreeView treeview, bool sortByType, int newPrefsDigitsNumber, bool metersSecondsPreferred)
	{
		this.treeview = treeview;
		this.sortByType = sortByType;
		//pDN = newPrefsDigitsNumber;
		this.pDN = newPrefsDigitsNumber;
		this.metersSecondsPreferred = metersSecondsPreferred;

		string runnerName = Catalog.GetString("Runner");
		string speedName = Catalog.GetString("Speed");
		string timeName = Catalog.GetString("Time");
		
		colsNum = 4;
		store = getStore(colsNum); //because, runID is not show in last col
		string [] columnsString = { runnerName, speedName, timeName };
		treeview.Model = store;
		prepareHeaders(columnsString);
	
		runIDColumn = 3;
	}
	
	public override void Fill(string [] myRuns, string filter)
	{
		TreeIter iter = new TreeIter();
		TreeIter iterDeep = new TreeIter(); //only for Interval

		string tempRunner = ":"; //one value that's not possible

		string myType ;
		string myTypeComplet ;
			
		foreach (string run in myRuns) {
			string [] myStringFull = run.Split(new char[] {':'});

			//show always the names of runners ...
			if(tempRunner != myStringFull[0])
			{
				iter = store.AppendValues (myStringFull[0]);
				tempRunner = myStringFull[0];
			}

			//... but if we selected one type of jump and this it's not the type, don't show
			if(filter == allRunsName || filter == myStringFull[4]) {
				myType = myStringFull[4];
				myTypeComplet = myType + "(" + myStringFull[7] + "x" + myStringFull[11] + ") AVG: "; //limited

				string [] myData = new String [colsNum];
				int count = 0;
				myData[count++] = myTypeComplet;
				myData[count++] = Util.TrimDecimals( 
						Util.GetSpeed(myStringFull[5].ToString(), 	//distanceTotal
							myStringFull[6].ToString() )		//timeTotal
							, pDN );
				myData[count++] = Util.TrimDecimals( 
						Util.GetAverage(myStringFull[8].ToString()).ToString()	//AVG of intervalTimesString
							, pDN );
				myData[count++] = myStringFull[1]; //runUniqueID (not shown) 

				iterDeep = store.AppendValues (iter, myData);

				//if it's an Interval run, we should make a deeper tree with all the runs
				//the info above it's average

				string [] intervalTimes = myStringFull[8].Split(new char[] {'='});
				int countRows = 0;
				foreach (string myInterval in intervalTimes) 
				{
					string [] myData2 = new String [colsNum];
					count = 0;
					myData2[count++] = (countRows+1).ToString();
					myData2[count++] = Util.TrimDecimals( 
						Util.GetSpeed(
							myStringFull[7].ToString(), //distanceInterval (all the intervals must have same distance)
							myInterval )			//time this interval
							, pDN );
					myData2[count++] = Util.TrimDecimals( myInterval, pDN );
					myData2[count++] = myStringFull[1]; //jumpUniqueID (not shown) 

					store.AppendValues (iterDeep, myData2);

					
					countRows ++;
				}
			}
		}
	}
	
	public virtual void Add (string runnerName, RunInterval newRun)
	{
		TreeIter iter = new TreeIter();
		TreeIter iterDeep = new TreeIter();
		bool modelNotEmpty = treeview.Model.GetIterFirst ( out iter ) ;
		string iterRunnerString;
		bool found = false;
		
		if(modelNotEmpty) {
			do {
				iterRunnerString = ( treeview.Model.GetValue (iter, 0) ).ToString();
				if(iterRunnerString == runnerName) {
					found = true;
				
					//expand the runner
					treeview.ExpandToPath( treeview.Model.GetPath(iter) );

					//if limited  value is "-1" comes from a "unlimited" intervalic run, 
					//put runs total time as limitation value
					if(newRun.Limited == "-1T") { 
						newRun.Limited = newRun.TimeTotal.ToString() + "T";
					}
					
					string myTypeComplet = newRun.Type + "(" + 
						newRun.DistanceInterval + "x" + newRun.Limited + ") AVG: "; //limited
					
					string [] myData = new String [colsNum];
					int count = 0;
					myData[count++] = myTypeComplet;
					myData[count++] = Util.TrimDecimals( 
							Util.GetSpeed(newRun.DistanceTotal.ToString(), 	//distanceTotal
								newRun.TimeTotal.ToString() )			//timeTotal
							, pDN );
					myData[count++] = Util.TrimDecimals( 
							Util.GetAverage(newRun.IntervalTimesString.ToString()).ToString()
							, pDN );
					myData[count++] = newRun.UniqueID.ToString(); //runUniqueID (not shown) 

					iterDeep = store.AppendValues (iter, myData);

					//fill the intervals
					string [] allIntervalTimes = newRun.IntervalTimesString.Split(new char[] {'='});
					int countRows = 0;
					foreach (string myInterval in allIntervalTimes) {
						string [] myData2 = new String [colsNum];
						count = 0;
						myData2[count++] = (countRows+1).ToString();
						myData2[count++] = Util.TrimDecimals( 
								Util.GetSpeed(newRun.DistanceInterval.ToString(), 	//distance Interval
									myInterval )			//time Interval
								, pDN );
						myData2[count++] = Util.TrimDecimals( myInterval, pDN );
						myData2[count++] = newRun.UniqueID.ToString(); //runUniqueID (not shown) 

						store.AppendValues (iterDeep, myData2);
							
						countRows ++;
					}
				}
			} while (treeview.Model.IterNext (ref iter));
		}

		//if the runner has not run in this session, it's name doesn't appear in the treeview
		//create the name, and write the run
		if(! found) {
			iter = store.AppendValues (runnerName);
			
			string myTypeComplet = newRun.Type + "(" + 
				newRun.DistanceInterval + "x" + newRun.Limited + ") AVG: "; //limited
						
			string [] myData2 = new String [colsNum];
			int count = 0;
			myData2[count++] = myTypeComplet;
			myData2[count++] = Util.TrimDecimals( 
					Util.GetSpeed(newRun.DistanceTotal.ToString(), 	//distanceTotal
						newRun.TimeTotal.ToString() )			//timeTotal
					, pDN );
			myData2[count++] = Util.TrimDecimals( 
					Util.GetAverage(newRun.IntervalTimesString.ToString()).ToString()
					, pDN );
			myData2[count++] = newRun.UniqueID.ToString(); //runUniqueID (not shown) 

			iterDeep = store.AppendValues (iter, myData2);
							
			//fill the intervals
			string [] allIntervalTimes = newRun.IntervalTimesString.Split(new char[] {'='});
			int countRows = 0;
			foreach (string myInterval in allIntervalTimes) {
				myData2 = new String [colsNum];
				count = 0;
				myData2[count++] = (countRows+1).ToString();
				myData2[count++] = Util.TrimDecimals( 
						Util.GetSpeed(newRun.DistanceInterval.ToString(), 	//distance Interval
							myInterval )			//time Interval
						, pDN );
				myData2[count++] = Util.TrimDecimals( myInterval, pDN );
				myData2[count++] = newRun.UniqueID.ToString(); //runUniqueID (not shown) 

				store.AppendValues (iterDeep, myData2);

				countRows ++;
			}
		//expand the runner
		treeview.ExpandToPath( treeview.Model.GetPath(iter) );
		}
	}

	public virtual void ExpandOptimal()
	{
		TreeIter iter = new TreeIter();
		bool tvExists = treeview.Model.GetIterFirst ( out iter ) ; //returns false if empty
	
		if (tvExists) {
			do {
				treeview.ExpandToPath( treeview.Model.GetPath(iter) );
			} while (treeview.Model.IterNext (ref iter));
		}
	}
	
}
