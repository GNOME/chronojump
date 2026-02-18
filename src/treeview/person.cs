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
using Mono.Unix;


public class TreeViewPersons
{
	private TreeStore store;
	private Gtk.TreeView treeview;
	private bool showRestOrStatus;

	private const int colID = 0;
	private const int colClubID = 1;
	private const int colNameFirst = 2;
	private const int colNameLast = 3;
	private const int colN = 4;
	private const int colRestOrStatus = 5; //status is used on beepTest

	//if 0 don't use it
	//if > 0 then show in red when >= to this value
	public int RestSecondsMark;
	
	public TreeViewPersons ()
	{
	}
	
	public TreeViewPersons (Gtk.TreeView treeview, bool showClubID, Constants.Modes current_mode, int restSeconds)
	{
		this.treeview = treeview;
		this.showRestOrStatus = current_mode != Constants.Modes.BEEPTEST;

		RestSecondsMark = restSeconds;

		//LogB.Information ("TreeViewPersons current_mode: " + current_mode.ToString ());
		string nColumn = "n";
		if (Constants.ModeIsENCODER (current_mode))
			nColumn = Catalog.GetString ("Sets");

		string [] columnsString = { "ID", Catalog.GetString ("Club ID"), Catalog.GetString("First name"), Catalog.GetString ("Last name"), nColumn, Catalog.GetString("Rest")};
		if (! showRestOrStatus)
			columnsString = new string [] { "ID", Catalog.GetString ("Club ID"), Catalog.GetString("First name"), Catalog.GetString ("Last name"), nColumn, Catalog.GetString("Status")};

		store = getStore (columnsString.Length);

		treeview.Model = store;
		prepareHeaders (columnsString, showClubID, current_mode != Constants.Modes.UNDEFINED);
	}
	
	private TreeStore getStore (int columns)
	{
		//prepares the TreeStore for required columns
		Type [] types = new Type [columns];
		for (int i=0; i < columns; i++) {
			types[i] = typeof (string);
		}
		TreeStore myStore = new TreeStore(types);
		return myStore;
	}
	
	private void prepareHeaders (string [] columnsString, bool showClubID, bool showN)
	{
		treeview.HeadersVisible = true;
		int i=0;
		bool visible = false;
		foreach (string myCol in columnsString) {
			if (i < colRestOrStatus)
			{
				if (i == colClubID)
					UtilGtk.CreateCols(treeview, store, Catalog.GetString(myCol), i++, showClubID);
				else if (i == colN)
					UtilGtk.CreateCols(treeview, store, Catalog.GetString(myCol), i++, showN);
				else
					UtilGtk.CreateCols(treeview, store, Catalog.GetString(myCol), i++, visible);
			}
			else {
				//do it here to use a custom colored Renderer
				Gtk.TreeViewColumn aColumn = new Gtk.TreeViewColumn ();
				CellRendererText aCell = new CellRendererText();
				aColumn.Title = Catalog.GetString(myCol);
				aColumn.PackStart (aCell, true);

				if (showRestOrStatus)
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderRestTime));
				else
					aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderStatus));

				aColumn.SortColumnId = i;
				aColumn.SortIndicator = true;
				aColumn.Visible = visible;
				treeview.AppendColumn ( aColumn );
			}

			if (i == colClubID)
				store.SetSortFunc (i, UtilGtk.IdColumnCompareCol1);
			else if (i == colN)
				store.SetSortFunc (i, UtilGtk.IdColumnCompareCol4);

			visible = true;
		}
	}

	private void RenderRestTime (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		string restTime = (string) model.GetValue(iter, colRestOrStatus);

		if(RestSecondsMark > 0 && LastTestTime.GetSeconds(restTime) >= RestSecondsMark)
		{
			Gtk.ITreeModel model2;
			Gtk.TreeIter iter2;
			bool selected = false;
			if (treeview.Selection.GetSelected (out model2, out iter2))
				if(model.GetValue(iter, colID).ToString() == model2.GetValue(iter2, colID).ToString())
					selected = true;

			if(selected) {
				//based on http://stackoverflow.com/a/9548415
				(cell as Gtk.CellRendererText).Markup = "<span foreground=\"red\" background=\"white\">"+restTime+"</span>";
			}
			else {
				(cell as Gtk.CellRendererText).Foreground = UtilGtk.ColorBad;
				(cell as Gtk.CellRendererText).Text = restTime;
			}
		} else {
			(cell as Gtk.CellRendererText).Foreground = null;	//will show default color
			(cell as Gtk.CellRendererText).Text = restTime;
		}
	}

	private void RenderStatus (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		// 1. decide word to show and fg color
		// the cell has the translated value
		string status = (string) model.GetValue(iter, colRestOrStatus);
		string statusString = status;

		if (statusString == RunnerStatus.GetStatusEnumTr (RunnerStatus.StatusEnum.Nothing))
			statusString = "";
		string colorFg = UtilGtk.ColorBlack;

		if (status == RunnerStatus.GetStatusEnumTr (RunnerStatus.StatusEnum.Running))
			colorFg = UtilGtk.ColorGood;

		// 2. check if is selected. To show bg white color
		Gtk.ITreeModel model2;
		Gtk.TreeIter iter2;
		bool selected = false;
		if (treeview.Selection.GetSelected (out model2, out iter2))
			if(model.GetValue(iter, colID).ToString() == model2.GetValue(iter2, colID).ToString())
				selected = true;

		if(selected) {
			//based on http://stackoverflow.com/a/9548415
			(cell as Gtk.CellRendererText).Markup = "<span foreground=\"" + colorFg + "\" background=\"white\">" +statusString + "</span>";
		}
		else {
			(cell as Gtk.CellRendererText).Foreground = colorFg;
			(cell as Gtk.CellRendererText).Text = statusString;
		}
	}

	/*
	 * this method works fine but does not show foreground in color when cell is selected
	 * above method solves this
	private void RenderRestTime (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		string restTime = (string) model.GetValue(iter, colRest);
		(cell as Gtk.CellRendererText).Text = restTime;

		if(RestMinutesMark > 0 && LastTestTime.GetMinutes(restTime) >= RestMinutesMark)
			(cell as Gtk.CellRendererText).Foreground = UtilGtk.ColorBad;
		else
			(cell as Gtk.CellRendererText).Foreground = null; 	//will show default color
	}
	*/

	public void RemoveColumns() {
		Gtk.TreeViewColumn [] myColumns = treeview.Columns;
		foreach (Gtk.TreeViewColumn column in myColumns) {
			treeview.RemoveColumn (column);
		}
	}

	public void Fill (ArrayList myPersons, RestTime rt)
	{
		foreach (Person person in myPersons)
		{
			//take care on null at restTime. This happens eg on start of session where SessionMode == UNIQUE
			string restedTime = "";
			if(rt != null && rt.RestedTime(person.UniqueID) != null)
				restedTime = rt.RestedTime(person.UniqueID);

			store.AppendValues ( new String [] {
					person.UniqueID.ToString(),
					person.ClubID,			//future2: ClubID
					person.NameFirst,
					person.NameLast,
					"0",
					restedTime }
					);
		}

		//show sorted by column nameFirst
		store.SetSortColumnId (colNameFirst, Gtk.SortType.Ascending);
		store.SetSortColumnId (colNameLast, Gtk.SortType.Ascending);

		store.ChangeSortColumn();
	}
	
	//pass 0 for first row
	public bool SelectRow(int rowNumber)
	{
		TreeIter iter;
		bool iterOk = store.GetIterFirst(out iter);
		if(iterOk) {
			int count = 0;
			while (count < rowNumber) {
				store.IterNext(ref iter);
				count ++;
			}
			treeview.Selection.SelectIter(iter);
			TreePath path = store.GetPath (iter);
			treeview.ScrollToCell (path, null, true, 0, 0);
			return true;
		}
		return false;
	}
	
	public bool IsThereAnyRecord() {
		TreeIter iter;
		return store.GetIterFirst(out iter);
	}
	
	public int FindRow(int uniqueID)
	{
		TreeIter iter;
		int found = -1;
		bool iterOk = store.GetIterFirst(out iter);
		if(iterOk) {
			int count = 0;
			do {
				if(Convert.ToInt32 ((string) treeview.Model.GetValue (iter, colID)) == uniqueID) {
					found = count;
				}
				count ++;
			} while (store.IterNext (ref iter) && found == -1);
		}
		return found;
	}

	//to scroll when elements of gui are resized changed, like the sidebar session/persons shrink
	public void ScrollToSelectedRow ()
	{
		TreeIter iter;
		Gtk.ITreeModel model = treeview.Model;

		if (! treeview.Selection.GetSelected (out model, out iter))
			return;

		TreePath path = store.GetPath (iter);
		treeview.ScrollToCell (path, null, true, 0, 0);
	}

	public void SelectRowByUniqueID(int personID)
	{
		SelectRow(FindRow(personID));
	}

	public void SelectNextRow(int personID)
	{
		SelectRow(FindRow(personID) +1);
	}
	
	public void SelectPreviousRow(int personID)
	{
		SelectRow(FindRow(personID) -1);
	}

	public IDName GetPersonByRow (int rowNumber)
	{
		TreeIter iter;
		bool iterOk = store.GetIterFirst(out iter);
		if(iterOk) {
			int count = 0;
			do {
				if (rowNumber == count ++)
					return (new IDName (
								Convert.ToInt32 ((string) treeview.Model.GetValue (iter, colID)),
								Person.GetNameFromFirstAndLast (
									(string) treeview.Model.GetValue (iter, colNameFirst),
									(string) treeview.Model.GetValue (iter, colNameLast)
									)
							   ));
			} while (store.IterNext (ref iter));
		}

		return (new IDName (-1, ""));
	}

	public int CountRows() {
		return(store.IterNChildren());
	}

	public bool IsFirst(int personID)
	{
		return (FindRow(personID) <= 0);
	}

	public bool IsLast(int personID)
	{
		return (FindRow(personID) == CountRows() -1);
	}

	
	//add in the row position by alfabetical order
	public void Add (Person p)
	{
		TreeIter iter = new TreeIter();
		bool iterOk = store.GetIterFirst(out iter);
		int found = -1;

		int count = 0;
		if(iterOk) {
			do {
				//search until find when jumperName is lexicographically > than current row
				if(String.Compare(p.Name.ToUpper(),
							(
							 Person.GetNameFromFirstAndLast (
								 (string) treeview.Model.GetValue (iter, colNameFirst),
								 (string) treeview.Model.GetValue (iter, colNameLast)
								 )
							).ToUpper()
						 ) < 0 )
				{
					found = count;
					break;
				}
				count ++;
			} while (store.IterNext (ref iter));
		}
		
		TreeIter iter2 = new TreeIter();
		
		if(found != -1) {
			//store.Insert (out iter2, found);
			iter2 = store.InsertNode (found);
			//first ID, then Name
			store.SetValue (iter2, colID, p.UniqueID);
			store.SetValue (iter2, colClubID, p.ClubID);
			store.SetValue (iter2, colNameFirst, p.NameFirst);
			store.SetValue (iter2, colNameLast, p.NameLast);
			store.SetValue (iter2, colN, "0");
			store.SetValue (iter2, colRestOrStatus, "");
		} else {
			//first ID, then Name
			iter2 = store.AppendValues (p.UniqueID, p.ClubID, p.NameFirst, p.NameLast, "0", "");
		}
			
		//scroll treeview if needed
		TreePath path = store.GetPath (iter2);
		treeview.ScrollToCell (path, null, true, 0, 0);
	}

	/*
	 * unused now, just do not shown the column
	public void UpdateTestsNBlank () //used when mode is UNDEFINED (user at select modes screen
	{
		TreeIter iter;
		bool iterOk = store.GetIterFirst(out iter);
		if(! iterOk)
			return;

		do {
			store.SetValue (iter, colN, "");
		} while (store.IterNext (ref iter));
	}
	*/

	public void UpdateTestsN (List<IntInt> ii_l)
	{
		TreeIter iter;
		bool iterOk = store.GetIterFirst(out iter);
		if(! iterOk)
			return;

		do {
			// get personID of each row on treeview persons
			string pIDStr = (string) store.GetValue (iter, colID);
			if (! Util.IsNumber (pIDStr, false))
				continue;
			int pID = Convert.ToInt32 (pIDStr);

			// get n of each row on treeview persons
			string nStr = (string) store.GetValue(iter, colN);
			if (! Util.IsNumber (nStr, false))
				continue;
			int n = Convert.ToInt32 (nStr);

			// assign n to the person
			bool found = false;
			foreach (IntInt ii in ii_l)
				if (ii.a == pID)
				{
					store.SetValue (iter, colN, ii.b.ToString ());
					found = true;
					break;
				}
			if (! found)
				store.SetValue (iter, colN, "0");
		} while (store.IterNext (ref iter));
	}

	public void UpdateRestTimes(RestTime restTime)
	{
		TreeIter iter;
		bool iterOk = store.GetIterFirst(out iter);
		if(iterOk) {
			do {
				string rested = restTime.RestedTime(
						Convert.ToInt32(store.GetValue(iter, colID)));
				if(rested != "")
					store.SetValue(iter, colRestOrStatus, rested);
				//else
				//	store.SetValue(iter, colRest, "");
				//	above is useful for beepTest putting all to 0 at start, but better have a col with status

			} while (store.IterNext (ref iter));
		}
	}

	//personID == -1 means all
	public void UpdateStatus (int personID, RunnerStatus.StatusEnum statusEnum)
	{
		TreeIter iter;
		bool iterOk = store.GetIterFirst(out iter);
		if(iterOk) {
			do {
				if (personID < 0 || Convert.ToInt32(store.GetValue(iter, colID)) == personID)
					store.SetValue(iter, colRestOrStatus,
							Catalog.GetString (statusEnum.ToString ()));
			} while (store.IterNext (ref iter));
		}
	}
}

