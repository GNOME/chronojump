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
 *  Copyright (C) 2004-2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Data;
using Gtk;
using System.Collections; //ArrayList
using System.Collections.Generic; //List
using Mono.Unix;


public abstract class TreeViewEvent
{
	protected TreeStore store;
	protected Gtk.TreeView treeview;

	protected Preferences preferences;
	protected int pDN; //prefsDigitsNumber;
	
	protected bool treeviewHasTwoLevels;
	protected int dataLineNamePosition; //position of name in the data to be printed
	protected int dataLineTypePosition; //position of type in the data to be printed
	protected string allEventsName; //Constants.AllJumpsName or Constants.AllRunsName orConstants.AllPulsesName
	protected int idColumn; //column where the uniqueID of event will be (and will be hidden). Note sice 17 apr 2025 it also contains the personID on its row
	protected int personIdColumn = 2;
	protected int descriptionColumn = -1; //used only on encoder

	//EventSelectedID >= 0 a test; -1 a person: -2 a subtest (do not select)
	public const int MarkRowIsPerson = -1;
	public const int MarkNonSelectRowSubEvent = -2;
	protected const string boldMark = "_BOLD_";
	protected List<int> boldableColumns_l = new List<int> ();

	protected string personName = Catalog.GetString ("Person");
	protected string lateralityName = Catalog.GetString ("Laterality");
	protected string weightExtraName = Catalog.GetString("Extra weight");
	protected string videoName = Catalog.GetString("Video");
	protected string datetimeName = Catalog.GetString("Date");
	protected string descriptionName = Catalog.GetString("Description");

	private int currentPersonID; //used to show it on bold

	//to calculate potency (on jumps)
	protected double personWeight; 
	protected double weightInKg;

	protected bool weightPercentPreferred;
	protected List<string> videos_l;

	protected string [] columnsString;

	public enum ExpandStates {
		MINIMIZED, OPTIMAL, MAXIMIZED
	}
	
	public ExpandStates expandState;

	private static int lastPersonID;

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

	protected virtual void prepareHeaders (string [] columnsString)
	{
		treeview.HeadersVisible=true;
		int i=0;
		foreach (string colStr in columnsString)
		{
			if (i == 0 || UtilList.FoundInListInt (boldableColumns_l, i))
			{
				Gtk.TreeViewColumn col = new Gtk.TreeViewColumn ();
				CellRendererText cell = new CellRendererText();
				col.Title = colStr;
				col.PackStart (cell, true);

				if (i == 0)	// to show person name in bold if is currentPerson
					col.SetCellDataFunc (cell, new Gtk.TreeCellDataFunc (RenderPersonName));
				else
					col.SetCellDataFunc (cell, new Gtk.TreeCellDataFunc (RenderBoldableCols));

				treeview.AppendColumn (col);
				i ++;
			} else
				treeview.AppendColumn (colStr, new CellRendererText(), "text", i++);
		}
	}

	protected void RenderPersonName (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		if(! (cell is CellRendererText))
			return;

		string text = (string) model.GetValue (iter, 0);
		int personID = -1;
		if (model.GetValue (iter, idColumn) != null)
			if (Util.IsNumber ((string) (model.GetValue (iter, idColumn)), false))
				personID = Convert.ToInt32 ( (string) model.GetValue (iter, idColumn));

		if (idIsPerson (iter) && personID >= 0 && personID == currentPersonID)
			(cell as Gtk.CellRendererText).Markup = "<span weight=\"bold\">" + text + "</span>";
		else
			(cell as Gtk.CellRendererText).Text = text;
	}

	protected void RenderBoldableCols (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		if(! (cell is CellRendererText))
			return;

		// get the colID to use just this for all cols
		int colNum = 0;
		for (int i = 0; i < columnsString.Length; i ++)
			if (column.Title == columnsString[i])
				colNum = i;

		string text = (string) model.GetValue (iter, colNum);

		if (text.StartsWith (boldMark))
		{
			text = Util.RemoveSubstring (text, boldMark);
			if (shouldRenderBoldable (column.Title))
			{
				// (cell as Gtk.CellRendererText).Markup = "<span weight=\"bold\">" + text + "</span>"; //bold
				(cell as Gtk.CellRendererText).Markup = "<span weight=\"600\">" + text + "</span>"; //demibold
			} else
				(cell as Gtk.CellRendererText).Text = text;
		} else
			(cell as Gtk.CellRendererText).Text = text;
	}

	protected virtual bool shouldRenderBoldable (string columnTitle)
	{
		return false;
	}

	public void PersonEmitRowChanged (int personID)
	{
		TreeIter iter = new TreeIter();
		if(! treeview.Model.GetIterFirst (out iter))
			return;

		do {
			if (treeview.Model.GetValue (iter, idColumn) != null && idIsPerson (iter))
				if (Util.IsNumber ((string) treeview.Model.GetValue (iter, idColumn), false) &&
						personID == Convert.ToInt32 ((string) treeview.Model.GetValue (iter, idColumn)))
				{
					//LogB.Information ("PersonEmitRowChanged: " + personID.ToString ());
					TreePath path = store.GetPath (iter);
					treeview.Model.EmitRowChanged (path, iter);
					return;
				}
		} while (treeview.Model.IterNext (ref iter));
	}

	// result cells than can be in bold to match the results shown on bars
	public void ResultsInBarsRowChanged ()
	{
		if (treeviewHasTwoLevels)
			resultsInBarsRowChangedTwoLevels ();
		else
			resultsInBarsRowChangedOneLevel ();
	}

	private void resultsInBarsRowChangedOneLevel ()
	{
		TreeIter iter = new TreeIter();
		if(! treeview.Model.GetIterFirst (out iter))
			return;

		do {
			TreeIter iterDeep = new TreeIter ();
			treeview.Model.IterChildren (out iterDeep, iter);
			do {
				foreach (int j in boldableColumns_l)
					if (treeview.Model.GetValue (iterDeep, j) != null &&
							((string) treeview.Model.GetValue (iterDeep, j)).StartsWith (boldMark))
					{
						TreePath path = store.GetPath (iterDeep);
						LogB.Information ("EmitRowChanged: " + path.ToString ());
						treeview.Model.EmitRowChanged (path, iterDeep);
					}
			} while (treeview.Model.IterNext (ref iterDeep));
		} while (treeview.Model.IterNext (ref iter));
	}

	private void resultsInBarsRowChangedTwoLevels ()
	{
		TreeIter iter = new TreeIter();
		if(! treeview.Model.GetIterFirst (out iter))
			return;

		do {
			TreeIter iterDeep = new TreeIter ();
			treeview.Model.IterChildren (out iterDeep, iter);
			do {
				TreeIter iterDeep2 = new TreeIter ();
				treeview.Model.IterChildren (out iterDeep2, iterDeep);
				for (int i = 0; i <= 1; i ++) // related statistic info is in row 0 or 1
				{
					foreach (int j in boldableColumns_l)
						if (treeview.Model.GetValue (iterDeep2, j) != null &&
								((string) treeview.Model.GetValue (iterDeep2, j)).StartsWith (boldMark))
						{
							TreePath path = store.GetPath (iterDeep2);
							//LogB.Information ("EmitRowChanged: " + path.ToString ());
							treeview.Model.EmitRowChanged (path, iterDeep2);
						}
					treeview.Model.IterNext (ref iterDeep2);
				}
			} while (treeview.Model.IterNext (ref iterDeep));
		} while (treeview.Model.IterNext (ref iter));
	}

	//to know if is person. If has no parents it is top level
	private bool idIsPerson (TreeIter iter)
	{
		return isTopLevel (iter);
	}
	private bool isTopLevel (TreeIter iter)
	{
		TreeIter iterParent;
		return (! treeview.Model.IterParent (out iterParent, iter));
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

	public void Fill (string [] myEvents, string filterExercise, List<string> videos_l)
	{
		LogB.Information ("called Fill");
		this.videos_l = videos_l;

		TreeIter iter = new TreeIter();
		TreeIter iterDeep = new TreeIter(); //only used by two levels treeviews
		string tempPerson = ":"; //one value that's not possible

		LogB.Information ("dataLineTypePostion = " + dataLineTypePosition.ToString  ());
		foreach (string singleEvent in myEvents)
		{
			string [] myStringFull = singleEvent.Split(new char[] {':'});

			//show always the names of persons ...
			if (tempPerson != myStringFull[dataLineNamePosition])
			{
				iter = store.AppendValues (createPersonRow (myStringFull));
				tempPerson = myStringFull[dataLineNamePosition];
			}

			LogB.Information (string.Format (
				"At Fill, filterExercise: {0}, allEventsName: {1}, Catalog.GetString(myStringFull[dataLineTypePosition]): {2}",
				filterExercise, allEventsName, Catalog.GetString(myStringFull[dataLineTypePosition]) ));

			//... but if we selected one type of test of this mode and this it's not the type, don't show
			if (filterExercise == allEventsName ||
					filterExercise == Catalog.GetString (myStringFull[dataLineTypePosition]))
			{
				//get the object from the string
				System.Object myEvent = getObjectFromString (myStringFull);
				
				//getLineToStoreFromString is overriden in two level treeviews
				iterDeep = store.AppendValues (iter, getLineToStore (myEvent));
				if (treeviewHasTwoLevels)
				{
					addStatisticInfo (iterDeep, myEvent);
					int nSubEvents = getNumOfSubEvents (myEvent);
					for(int i = 0; i < nSubEvents; i ++)
						store.AppendValues (iterDeep, getSubLineToStore (myEvent, i));
				}
			}
		}
	}

	//TODO: if more tests send their objects, send List<List<Event>> or List<Event>
	public virtual void FillEncoder (List<List<EncoderSQL>> eSQL_ll, string filterExercise, List<string> videos_l)
	{
	}

	//used on Fill
	private string [] createPersonRow (string [] strFull)
	{
		string [] row = new String [idColumn +1];
		row[0] = strFull[dataLineNamePosition];
		int i;
		for (i = 1; i < idColumn; i ++)
			row[i] = "";

		row[i] = (Convert.ToInt32 (strFull[personIdColumn])).ToString ();
		return row;
	}
	//used on Add, and on FillEncoder
	protected string [] createPersonRow (int id, string name)
	{
		string [] row = new String [idColumn +1];
		row[0] = name;
		int i;
		for (i = 1; i < idColumn; i ++)
			row[i] = "";

		row[i] = id.ToString ();
		return row;
	}

	// ---- on two level treeviews ---->
	public void SelectEventHeaderLine()
	{
		TreeIter iter2 = new TreeIter ();
		if (getIterParentOfSelectedSubEvent (ref iter2))
			treeview.Selection.SelectIter(iter2);
	}
	public int GetIDOfSelectedSubEvent ()
	{
		TreeIter iter2 = new TreeIter ();
		if (getIterParentOfSelectedSubEvent (ref iter2))
			return Convert.ToInt32 (treeview.Model.GetValue (iter2, idColumn));
		else
			return MarkNonSelectRowSubEvent;
	}
	private bool getIterParentOfSelectedSubEvent (ref TreeIter iter2)
	{
		TreeIter iter = new TreeIter();
		ITreeModel myModel = treeview.Model;
		if (! treeview.Selection.GetSelected (out myModel, out iter))
			return false;

		string pathString = store.GetPath(iter).ToString();
		string [] myStrFull = pathString.Split(new char[] {':'});
		if (myStrFull.Length < 2)
			return false;

		string pathStringZero = myStrFull[0] + ":" + myStrFull[1]; //this will be the person name and the header line of the test

		store.GetIterFromString (out iter2, pathStringZero);
		return true;
	}

	// <---- on two level treeviews ----

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

	public virtual void UpdateReps (List<List<EncoderSQL>> eSQL_ll)
	{
	}

	// right now only on encoder
	public void UpdateDescription (int setID, string desc)
	{
		if (descriptionColumn < 0)
			return;

		TreeIter iter = new TreeIter ();
		if (! getEvent (setID, out iter))
			return;

		store.SetValue (iter, descriptionColumn, desc);
	}

	public void ZoomChange (Gtk.Image icon_zoom)
	{
		expandState = zoomChangeDo (expandState);
		if (expandState == TreeViewEvent.ExpandStates.MINIMIZED)
		{
			treeview.CollapseAll();
			icon_zoom.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) +
					Constants.FileNameZoomInIcon);
		} else if (treeviewHasTwoLevels && expandState == ExpandStates.OPTIMAL)
		{
			treeview.CollapseAll();
			ExpandOptimal();
			icon_zoom.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) +
					Constants.FileNameZoomInIcon);
		} else
		{ //expandState == TreeViewEvent.ExpandStates.MAXIMIZED
			treeview.ExpandAll();
			icon_zoom.Pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) +
					Constants.FileNameZoomOutIcon);
		}
	}

	private ExpandStates zoomChangeDo (ExpandStates myExpand)
	{
		if (treeviewHasTwoLevels) {
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
	public void Add (int personID, string pName, System.Object newEvent, string videoStr)
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
				if(iterPersonString == pName) {
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
		if(! found)
		{
			iter = store.AppendValues (createPersonRow (personID, pName));
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

	//TODO: if more tests send their objects, send List<List<Event>> or List<Event>
	public virtual void AddEncoder (int personID, string pName, List<List<EncoderSQL>> eSQL_ll, string videoStr)
	{
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
					int iterEventID =  Convert.ToInt32 ( treeview.Model.GetValue (iter, idColumn) );
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

	// to select person on results treeviews when personChanged
	public void SelectPerson (string name)
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

	//this selects a test (not a person) selection comes from clicking a bar cairoPaintBarsPre.FindBarIdInPixel
	//so need to guarantee that the found id is not a person
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
			if( treeview.Model.IterHasChild(iter) )
			{
				treeview.Model.IterChildren (out iter, iter);
				do {
					int iterEventID =  Convert.ToInt32 ( treeview.Model.GetValue (iter, idColumn) );
					if(iterEventID == uniqueID && ! idIsPerson (iter)) {
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

	// 1st level (maybe use this into SelectEvent)
	protected bool getEvent (int uniqueID, out TreeIter iter)
	{
		treeview.Model.GetIterFirst (out iter) ;

		/*
		  new GTK# makes IterNext point to an invalid iter if there's no next
		  then we cannot find parent of iter
		  with the iterValid, we have the last valid children iter
		  and we use it to find parent
		  */
		TreeIter iterValid = new TreeIter();

		do {
			if( treeview.Model.IterHasChild(iter) )
			{
				treeview.Model.IterChildren (out iter, iter);
				do {
					int iterEventID = Convert.ToInt32 (treeview.Model.GetValue (iter, idColumn));
					if(iterEventID == uniqueID && ! idIsPerson (iter))
						return true; //no se si aquí cal el iterValid

					iterValid = iter;
				} while (treeview.Model.IterNext (ref iter));

				iter = iterValid;
				treeview.Model.IterParent (out iter, iter);
			}
		} while (treeview.Model.IterNext (ref iter));

		return false;
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

	public int EventSelectedID
	{
		get {
			TreeIter iter = new TreeIter();
			ITreeModel myModel = treeview.Model;
			if (treeview.Selection.GetSelected (out myModel, out iter))
			{
				if (idIsPerson (iter))
					return MarkRowIsPerson; // it is a -1, because 0 can be an event
				else
					return Convert.ToInt32 ( treeview.Model.GetValue(iter, idColumn) );
			} else {
				return -1;
			}
		}
	}
	
	//any treeview (1 level or 2 levels) get the id of the person (selecting at his name or any of the levels)
	public int GetPersonIDOfSelectedRow
	{
		get {
			TreeIter iter = new TreeIter();
			ITreeModel myModel = treeview.Model;
			if (treeview.Selection.GetSelected (out myModel, out iter))
			{
				string pathString = store.GetPath(iter).ToString();
				//LogB.Information ("At GetPersonIDOfSelectedRow, pathString: " + pathString);
				string [] myStrFull = pathString.Split(new char[] {':'});
				string pathStringZero = myStrFull[0]; //this will be the iter to the person row
				TreeIter iter2;
				store.GetIterFromString(out iter2, pathStringZero);

				if (Util.IsNumber (treeview.Model.GetValue (iter2, idColumn).ToString (), false))
					return Convert.ToInt32 (treeview.Model.GetValue (iter2, idColumn).ToString ());
			}

			return -1;
		}
	}

	public ExpandStates ExpandState {
		get { return expandState; }
		set { expandState = value; }
	}

	//used on jumps: Add
	public double PersonWeight {
		set { personWeight = value; }
	}

	public int CurrentPersonID {
		get { return currentPersonID; }
		set { currentPersonID = value; }
	}

	public static int LastPersonID {
		set { lastPersonID = value; }
		get { return lastPersonID; }
	}

}
