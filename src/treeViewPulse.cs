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


public class TreeViewPulses
{
	protected TreeStore store;
	protected Gtk.TreeView treeview;
	protected static int pDN; //prefsDigitsNumber;
	
	public TreeViewPulses ()
	{
	}
	
	public TreeViewPulses (Gtk.TreeView treeview, int newPrefsDigitsNumber)
	{
		this.treeview = treeview;
		pDN = newPrefsDigitsNumber;

		string jumperName = Catalog.GetString("Person");
		string timeName = Catalog.GetString("Time");
		string diffName = Catalog.GetString("Difference");
		string diffPercentName = Catalog.GetString("Difference %");

		int colsNum = obtainPulseIDColumn() +1;
		store = getStore(colsNum); //because, jumpID is not show in last col
		string [] columnsString = { jumperName, timeName, diffName, diffPercentName };
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

	public virtual void Fill(string [] myPulses, string filter)
	{
		TreeIter iter = new TreeIter();
		TreeIter iterDeep = new TreeIter();

		string tempPerson = ":"; //one value that's not possible

		string myType ;
		string myTypeComplet ;
			
		foreach (string pulse in myPulses) {
			string [] myStringFull = pulse.Split(new char[] {':'});

			//show always the names of persons ...
			if(tempPerson != myStringFull[0])
			{
				iter = store.AppendValues (myStringFull[0]);
				tempPerson = myStringFull[0];
			}

			//... but if we selected one type of pulse and this it's not the type, don't show
			if(filter == Constants.AllPulsesName || filter == myStringFull[4]) {
				myType = myStringFull[4];
				string fixedPulse = myStringFull[5];
				myTypeComplet = myType + "(" + fixedPulse + ")";

				
				int colsNum = obtainPulseIDColumn() +1;
				string [] myData = new String [colsNum];
				int count = 0;
				myData[count++] = myTypeComplet;
				myData[count++] = "";	//time column
				myData[count++] = "";	//diff column
				myData[count++] = "";	//diff percent column
				myData[count++] = myStringFull[1]; //jumpUniqueID (not shown) 

				iterDeep = store.AppendValues (iter, myData);

				//if it's an RJ, we should make a deeper tree with all the jumps
				//the info above it's average

				string [] timesString = myStringFull[7].Split(new char[] {'='});
				int countRows = 0;
				foreach (string myTime in timesString) 
				{
					string [] myData2 = new String [colsNum];
					count = 0;
					myData2[count++] = (countRows+1).ToString();
					myData2[count++] = Util.TrimDecimals( myTime, pDN );
					myData2[count++] = Util.TrimDecimals( ( Convert.ToDouble(myTime) - 
							Convert.ToDouble(fixedPulse) ).ToString(), pDN );
					myData2[count++] = Util.TrimDecimals( ( Convert.ToDouble(myTime) * 100 / 
							Convert.ToDouble(fixedPulse) ).ToString(), pDN );
					myData2[count++] = myStringFull[1]; //jumpUniqueID (not shown) 

					store.AppendValues (iterDeep, myData2);

					countRows ++;
				}
			}
		}
	}
	
	public virtual void Add (string personName, Pulse newPulse)
	{
		TreeIter iter = new TreeIter();
		TreeIter iterDeep = new TreeIter();
		bool modelNotEmpty = treeview.Model.GetIterFirst ( out iter ) ;
		string iterPersonString;
		bool found = false;
		
		int colsNum = obtainPulseIDColumn() +1;
		if(modelNotEmpty) {
			do {
				iterPersonString = ( treeview.Model.GetValue (iter, 0) ).ToString();
				if(iterPersonString == personName) {
					found = true;
				
					//expand the pulse
					treeview.ExpandToPath( treeview.Model.GetPath(iter) );

					/*
					//if limited  value is "-1" comes from a "unlimited" intervalic run, 
					//put runs total time as limitation value
					if(newRun.Limited == "-1T") { 
						newRun.Limited = newRun.TimeTotal.ToString() + "T";
					}
					*/
					
					string myTypeComplet = newPulse.Type + "(" + newPulse.FixedPulse + ")";
					
					string [] myData = new String [colsNum];
					int count = 0;
					myData[count++] = myTypeComplet;
					myData[count++] = "";	//time column
					myData[count++] = "";	//diff column
					myData[count++] = "";	//diff percent column
					myData[count++] = newPulse.UniqueID.ToString(); //pulseUniqueID (not shown) 

					iterDeep = store.AppendValues (iter, myData);
					//scroll treeview if needed
					TreePath path = store.GetPath (iterDeep);
					treeview.ScrollToCell (path, null, true, 0, 0);

					//fill the subdata
					string [] allTimes = newPulse.TimesString.Split(new char[] {'='});
					int countRows = 0;
					foreach (string myTime in allTimes) {
						string [] myData2 = new String [colsNum];
						count = 0;
						myData2[count++] = (countRows+1).ToString();
						myData2[count++] = Util.TrimDecimals(myTime, pDN );
						myData2[count++] = Util.TrimDecimals( (Convert.ToDouble(myTime) - Convert.ToDouble(newPulse.FixedPulse) ).ToString(), pDN );
						myData2[count++] = Util.TrimDecimals( (Convert.ToDouble(myTime) * 100 / Convert.ToDouble(newPulse.FixedPulse) ).ToString() , pDN );
						myData2[count++] = newPulse.UniqueID.ToString(); //pulseUniqueID (not shown) 

						store.AppendValues (iterDeep, myData2);
							
						countRows ++;
					}
				}
			} while (treeview.Model.IterNext (ref iter));
		}

		//if the person has not done pulses in this session, it's name doesn't appear in the treeview
		//create the name, and write the pulse
		if(! found) {
			iter = store.AppendValues (personName);
			
			string myTypeComplet = newPulse.Type + "(" + newPulse.FixedPulse + ")";
						
			string [] myData2 = new String [colsNum];
			int count = 0;
			myData2[count++] = myTypeComplet;
			myData2[count++] = "";	//time column
			myData2[count++] = "";	//diff column
			myData2[count++] = "";	//diff percent column
			myData2[count++] = newPulse.UniqueID.ToString(); //runUniqueID (not shown) 

			iterDeep = store.AppendValues (iter, myData2);
					
			//scroll treeview if needed
			TreePath path = store.GetPath (iterDeep);
			treeview.ScrollToCell (path, null, true, 0, 0);
							
			//fill the pulses
			string [] allTimes = newPulse.TimesString.Split(new char[] {'='});
			int countRows = 0;
			foreach (string myTime in allTimes) {
				myData2 = new String [colsNum];
				count = 0;
				myData2[count++] = (countRows+1).ToString();
				myData2[count++] = Util.TrimDecimals(myTime, pDN );
				myData2[count++] = Util.TrimDecimals( ( Convert.ToDouble(myTime) - 
						Convert.ToDouble(newPulse.FixedPulse) ).ToString(), pDN );
				myData2[count++] = Util.TrimDecimals( ( Convert.ToDouble(myTime) * 100 / 
							Convert.ToDouble(newPulse.FixedPulse) ).ToString(), pDN );
				myData2[count++] = newPulse.UniqueID.ToString(); //pulseUniqueID (not shown) 

				store.AppendValues (iterDeep, myData2);

				countRows ++;
			}
		//expand the runner
		treeview.ExpandToPath( treeview.Model.GetPath(iter) );
		}
	}

	protected int obtainPulseIDColumn () {
		int pulseIDColumn = 4;
		return pulseIDColumn;
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
	
	public virtual void DelPulse (int pulseID)
	{
		TreeIter iter = new TreeIter();
		treeview.Model.GetIterFirst ( out iter ) ;
		int pulseIDColumn = obtainPulseIDColumn();
	
		do {
			if( treeview.Model.IterHasChild(iter) ) {
				treeview.Model.IterChildren (out iter, iter);
				do {
					int iterPulseID =  Convert.ToInt32 ( treeview.Model.GetValue (iter, pulseIDColumn) );
					//Console.WriteLine("iterPulseID {0}, pulseID{1}", iterPulseID, pulseID);
					if(iterPulseID == pulseID) {
						store.Remove(ref iter);
						return;
					}
				} while (treeview.Model.IterNext (ref iter));
				treeview.Model.IterParent (out iter, iter);
			}
		} while (treeview.Model.IterNext (ref iter));
	}

	public void Unselect () {
		treeview.Selection.UnselectAll();
	}

	public int PulseSelectedID {
		get {
			TreeIter iter = new TreeIter();
			TreeModel myModel = treeview.Model;
			if (treeview.Selection.GetSelected (out myModel, out iter)) {
				return Convert.ToInt32 ( treeview.Model.GetValue(iter, obtainPulseIDColumn()) );
			} else {
				return 0;
			}
		}
	}
	
}
