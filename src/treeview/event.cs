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
 *  Copyright (C) 2004-2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Data;
using Gtk;
using System.Collections; //ArrayList
using System.Collections.Generic; //List
using Mono.Unix;


public class TreeViewEvent
{
	protected TreeStore store;
	protected Gtk.TreeView treeview;

	protected Preferences preferences;
	protected int pDN; //prefsDigitsNumber;
	
	protected bool treeviewHasTwoLevels;
	protected int dataLineNamePosition; //position of name in the data to be printed
	protected int dataLineTypePosition; //position of type in the data to be printed
	protected string allEventsName; //Constants.AllJumpsName or Constants.AllRunsName orConstants.AllPulsesName
	protected int eventIDColumn; //column where the uniqueID of event will be (and will be hidden)
	protected string videoName = Catalog.GetString("Video");
	protected string descriptionName = Catalog.GetString("Description");
	
	protected bool weightPercentPreferred;
	protected List<string> videos_l;

	protected string [] columnsString;

	public enum ExpandStates {
		MINIMIZED, OPTIMAL, MAXIMIZED
	}
	
	public ExpandStates expandState;

	public TreeViewEvent ()
	{
	}
	
	public TreeViewEvent (Gtk.TreeView treeview, int newPrefsDigitsNumber, ExpandStates expandState)
	{
		this.treeview = treeview;
		this.pDN = newPrefsDigitsNumber;
		this.expandState = expandState;

		//orientative values, used for Run class
		treeviewHasTwoLevels = false;
		dataLineNamePosition = 0;
		dataLineTypePosition = 4;
		allEventsName = "";
		eventIDColumn = 4;

		columnsString = new string[0];
	
		store = getStore(columnsString.Length +1); //+1 because, eventID is not show in last col
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
			treeview.AppendColumn (myCol, new CellRendererText(), "text", i++);
		}
	}
	
	public virtual void RemoveColumns() {
		Gtk.TreeViewColumn [] myColumns = treeview.Columns;
		foreach (Gtk.TreeViewColumn column in myColumns) {
			treeview.RemoveColumn (column);
		}
	}

	protected virtual System.Object getObjectFromString(string [] myStringOfData)
	{
		System.Object myObject = new System.Object();
		return myObject;
	}
	
	protected virtual int getNumOfSubEvents(System.Object myObject)
	{
		return 0; //not used in treeViewEventClass
	} 
			

	//1st level
	protected virtual string [] getLineToStore(System.Object myObject)
	{
		string [] myData = new String [1]; //columnsString + 1
		//int count = 0;
		//myData[count++] = myObject.Name + ...;
		//...

		return myData;
	}
	
	//for 2nd level
	protected virtual string [] getSubLineToStore(System.Object myObject, int lineCount)
	{
		string [] myData = new String [1]; //columnsString + 1
		//int count = 0;
		//myData[count++] = lineCount.ToString() ...;
		//...

		return myData;
	}

	protected virtual int getColsNum() {
		return columnsString.Length +1;
	}
	
	protected virtual void addStatisticInfo (TreeIter iterDeep, System.Object myObject)
	{
		store.AppendValues (iterDeep, printTotal (myObject));
		store.AppendValues (iterDeep, printAVG (myObject));
		store.AppendValues (iterDeep, printSD (myObject));
	}
	
	protected virtual string [] printTotal (System.Object myObject)
	{
		string [] nothing = new string[0];
		return nothing;
	}
	
	protected virtual string [] printAVG (System.Object myObject)
	{
		string [] nothing = new string[0];
		return nothing;
	}

	protected virtual string [] printSD (System.Object myObject)
	{
		string [] nothing = new string[0];
		return nothing;
	}

	public virtual void Fill(string [] myEvents, string filter, List<string> videos_l)
	{
		this.videos_l = videos_l;

		TreeIter iter = new TreeIter();
		TreeIter iterDeep = new TreeIter(); //only used by two levels treeviews
		string tempPerson = ":"; //one value that's not possible

		foreach (string singleEvent in myEvents) {
			string [] myStringFull = singleEvent.Split(new char[] {':'});

			//show always the names of persons ...
			if(tempPerson != myStringFull[dataLineNamePosition])
			{
				iter = store.AppendValues (myStringFull[dataLineNamePosition]);
				tempPerson = myStringFull[dataLineNamePosition];
			}

			//... but if we selected one type of run and this it's not the type, don't show
			if(filter == allEventsName || filter == Catalog.GetString(myStringFull[dataLineTypePosition]))
			{
				//get the object from the string
				System.Object myEvent = getObjectFromString(myStringFull);
				
				//getLineToStoreFromString is overriden in two level treeviews
				iterDeep = store.AppendValues (iter, getLineToStore(myEvent));
				if(treeviewHasTwoLevels) {
					addStatisticInfo(iterDeep, myEvent);
					for(int i = 0; i < getNumOfSubEvents(myEvent); i ++) {
						store.AppendValues(iterDeep, getSubLineToStore(myEvent, i));
					}
				}
			}
		}
	}

	public void SelectHeaderLine() {
		TreeIter iter = new TreeIter();
		ITreeModel myModel = treeview.Model;
		if (treeview.Selection.GetSelected (out myModel, out iter)) {
			string pathString = store.GetPath(iter).ToString();
			string [] myStrFull = pathString.Split(new char[] {':'});
			string pathStringZero = myStrFull[0] + ":" + myStrFull[1];
			TreeIter iter2;
			store.GetIterFromString(out iter2, pathStringZero);
			treeview.Selection.SelectIter(iter2);
		}
	}

	public void Update (Event myEvent)
	{
		LogB.Information ("Called TreeViewEvent.Update ()");
		TreeIter iter = new TreeIter();
		ITreeModel myModel = treeview.Model;
		if (treeview.Selection.GetSelected (out myModel, out iter))
		{
			//this doesn't work on windows gtk-sharp 2.10 (works on 2.12)
			//store.SetValues (iter, getLineToStore(myEvent));
			string [] myRow = getLineToStore(myEvent);
			for (int i = 0; i < myRow.Length; i++)
				store.SetValue (iter, i, myRow[i]);

			if (treeviewHasTwoLevels)
			{
				TreeIter iterDeep = new TreeIter ();
				treeview.Model.IterChildren (out iterDeep, iter);

				string firstCol = treeview.Model.GetValue (iterDeep, 0).ToString ();
				if (firstCol.StartsWith (Catalog.GetString ("Total")))
				{
					//do nothing as update right now only updates distance that makes change speed AVG
					treeview.Model.IterNext (ref iterDeep);
					firstCol = treeview.Model.GetValue (iterDeep, 0).ToString ();
				}

				if (firstCol.StartsWith (Catalog.GetString ("AVG")))
				{
					myRow = printAVG (myEvent);
					for (int i = 0; i < myRow.Length; i++)
						store.SetValue (iterDeep, i, myRow[i]);

					treeview.Model.IterNext (ref iterDeep);
					firstCol = treeview.Model.GetValue (iterDeep, 0).ToString ();
				}

				if (firstCol.StartsWith (Catalog.GetString ("SD")))
				{
					//do nothing as update right now only updates distance that makes change speed AVG
					treeview.Model.IterNext (ref iterDeep);
					firstCol = treeview.Model.GetValue (iterDeep, 0).ToString ();
				}

				for (int j = 0; j < getNumOfSubEvents (myEvent); j++)
				{
					myRow = getSubLineToStore (myEvent, j);
					for (int i = 0; i < myRow.Length; i++)
						store.SetValue (iterDeep, i, myRow[i]);

					treeview.Model.IterNext (ref iterDeep);
				}
			}
		}
	}

	public ExpandStates ZoomChange(ExpandStates myExpand) {
		if(treeviewHasTwoLevels) {
			if(myExpand == ExpandStates.MINIMIZED)
				return ExpandStates.OPTIMAL;
			else if(myExpand == ExpandStates.OPTIMAL)
				return ExpandStates.MAXIMIZED;
			else
				return ExpandStates.MINIMIZED;
		} else {
			if(myExpand == ExpandStates.MINIMIZED)
				return ExpandStates.MAXIMIZED;
			else
				return ExpandStates.MINIMIZED;
		}
	}

	//if only shown persons, zoom to tests
	public void ZoomToTestsIfNeeded ()
	{
		if(expandState == ExpandStates.MINIMIZED)
		{
			if(treeviewHasTwoLevels)
			{
				expandState = ExpandStates.OPTIMAL;
				ExpandOptimal();
			} else
			{
				expandState = ExpandStates.MAXIMIZED;
				treeview.ExpandAll();
			}
		}
	}

	//TODO: with video here
	public void Add (string personName, System.Object newEvent, string videoStr)
	{
		TreeIter iter = new TreeIter();
		TreeIter iterDeep = new TreeIter(); //only used by two levels treeviews
		bool modelNotEmpty = treeview.Model.GetIterFirst ( out iter ) ;
		string iterPersonString;
		bool found = false;

		//on Add blank videos_l and if video the just add this one
		videos_l = new List<string> ();
		if (videoStr != "")
			videos_l.Add (videoStr);
	
		if(modelNotEmpty) {
			do {
				iterPersonString = ( treeview.Model.GetValue (iter, 0) ).ToString();
				if(iterPersonString == personName) {
					found = true;

					//expand the person
					treeview.ExpandToPath( treeview.Model.GetPath(iter) );

					//getLineToStore is overriden in two level treeviews
					iterDeep = store.AppendValues (iter, getLineToStore(newEvent));

					//select the test			
					treeview.Selection.SelectIter(iterDeep);
					
					TreePath path = store.GetPath (iterDeep);
					treeview.ScrollToCell (path, null, true, 0, 0);
				
					if(treeviewHasTwoLevels) {
						addStatisticInfo(iterDeep, newEvent);
						for(int i=0; i < getNumOfSubEvents(newEvent); i++) {
							store.AppendValues(iterDeep, getSubLineToStore(newEvent, i));
						}
					}
				}
			} while (treeview.Model.IterNext (ref iter));
		}

		//if the person has not done this kind of event in this session, it's name doesn't appear in the treeview
		//create the name, and write the event
		if(! found) {
			iter = store.AppendValues (personName);
			
			iterDeep = store.AppendValues (iter, getLineToStore(newEvent));
			
			//scroll treeview if needed
			TreePath path = store.GetPath (iterDeep);
			treeview.ScrollToCell (path, null, true, 0, 0);
		
			if(treeviewHasTwoLevels) {
				addStatisticInfo(iterDeep, newEvent);
				for(int i=0; i < getNumOfSubEvents(newEvent); i++) {
					store.AppendValues(iterDeep, getSubLineToStore(newEvent, i));
				}
			}
			
			//expand the person
			treeview.ExpandToPath( treeview.Model.GetPath(iter) );
			
			//select the test			
			treeview.Selection.SelectIter(iterDeep);
		}
	}
		
	private void deleteParentIfEmpty(TreeIter iter) {
		if( ! treeview.Model.IterHasChild(iter) ) 
			store.Remove(ref iter);
	}

	public void DelEvent (int eventID)
	{
		TreeIter iter = new TreeIter();
		treeview.Model.GetIterFirst ( out iter ) ;

		/*
		  new GTK# makes IterNext point to an invalid iter if there's no next
		  then we cannot find parent of iter
		  with the iterValid, we have the last valid children iter
		  and we use it to find parent
		  */
		TreeIter iterValid = new TreeIter();
		
		do {
			if( treeview.Model.IterHasChild(iter) ) {
				treeview.Model.IterChildren (out iter, iter);
				do {
					int iterEventID =  Convert.ToInt32 ( treeview.Model.GetValue (iter, eventIDColumn) );
					if(iterEventID == eventID) {
						//get parent (to delete if empty)
						TreeIter iterParent;
					       	bool parentOk = treeview.Model.IterParent(out iterParent, iter);

						//delete iter (test)
						store.Remove(ref iter);

						//delete parent (person on eventTreeview) if has no more child
						if(parentOk)
							deleteParentIfEmpty(iterParent);

						return;
					}
					iterValid = iter;
				} while (treeview.Model.IterNext (ref iter));
					
				iter= iterValid;
				treeview.Model.IterParent (out iter, iter);
			}
		} while (treeview.Model.IterNext (ref iter));
	}

	//used to select person on results treeviews when personChanged
	public void SelectPerson(string name)
	{
		TreeIter iter = new TreeIter();
		if(! treeview.Model.GetIterFirst (out iter))
			return;

		do {
			if(treeview.Model.GetValue (iter, 0).ToString() == name)
			{
				treeview.Selection.SelectIter(iter);

				//scroll treeview if needed
				TreePath path = store.GetPath (iter);
				treeview.ScrollToCell (path, null, true, 0, 0);

				return;
			}
		} while (treeview.Model.IterNext (ref iter));

		Unselect(); //if not found: unselect all
	}

	public void SelectEvent (int uniqueID, bool scrollToEvent)
	{
		TreeIter iter = new TreeIter();
		treeview.Model.GetIterFirst ( out iter ) ;
		
		/*
		  new GTK# makes IterNext point to an invalid iter if there's no next
		  then we cannot find parent of iter
		  with the iterValid, we have the last valid children iter
		  and we use it to find parent
		  */
		TreeIter iterValid = new TreeIter();

		bool found = false;
		do {
			if( treeview.Model.IterHasChild(iter) ) {
				treeview.Model.IterChildren (out iter, iter);
				do {
					int iterEventID =  Convert.ToInt32 ( treeview.Model.GetValue (iter, eventIDColumn) );
					if(iterEventID == uniqueID) {
						LogB.Information("We select:" + iterEventID);
						treeview.Selection.SelectIter (iter);

						if(scrollToEvent) {
							TreePath path = store.GetPath (iter);
							LogB.Debug(path.ToString());
							treeview.ScrollToCell (path, null, true, 0, 0);
						}

						found = true;
					}
					iterValid = iter;
				} while (treeview.Model.IterNext (ref iter) && ! found);

				iter= iterValid;
				treeview.Model.IterParent (out iter, iter);
			}
		} while (treeview.Model.IterNext (ref iter) && ! found);
	}	
	
	public void Unselect () {
		treeview.Selection.UnselectAll();
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

	public int EventSelectedID {
		get {
			TreeIter iter = new TreeIter();
			ITreeModel myModel = treeview.Model;
			if (treeview.Selection.GetSelected (out myModel, out iter)) {
				return Convert.ToInt32 ( treeview.Model.GetValue(iter, eventIDColumn) );
			} else {
				return 0;
			}
		}
	}
	
	public ExpandStates ExpandState {
		get { return expandState; }
		set { expandState = value; }
	}
}
