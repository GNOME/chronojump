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
	protected static string allRunsName = "All runs";
	
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

		string [] runTypes = SqliteRunType.SelectRunTypes("", false);
		
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
		string [] runTypes = SqliteRunType.SelectRunTypes("", false);
	
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
		string iterRunnerString;
		int runIDColumn = 4;
		
		do {
			iterRunnerString = ( treeview.Model.GetValue (iter, 0) ).ToString();
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
				return Convert.ToInt32 ( treeview.Model.GetValue(iter, 4) );
			} else {
				return 0;
			}
		}
	}
	
}
