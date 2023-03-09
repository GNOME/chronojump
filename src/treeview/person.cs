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
 *  Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using Gtk;
using System.Collections; //ArrayList
using Mono.Unix;


public class TreeViewPersons
{
	protected TreeStore store;
	protected Gtk.TreeView treeview;

	//if 0 don't use it
	//if > 0 then show in red when >= to this value
	public int RestSecondsMark;
	
	public TreeViewPersons ()
	{
	}
	
	public TreeViewPersons (Gtk.TreeView treeview, int restSeconds)
	{
		this.treeview = treeview;

		RestSecondsMark = restSeconds;

		store = getStore(3);
		string [] columnsString = { "ID", Catalog.GetString("person"), Catalog.GetString("Rest")};
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
	
	protected void prepareHeaders(string [] columnsString) 
	{
		treeview.HeadersVisible=true;
		int i=0;
		bool visible = false;
		foreach(string myCol in columnsString) {
			if(i < 2)
				UtilGtk.CreateCols(treeview, store, Catalog.GetString(myCol), i++, visible);
			else {
				//do it here to use a custom colored Renderer
				Gtk.TreeViewColumn aColumn = new Gtk.TreeViewColumn ();
				CellRendererText aCell = new CellRendererText();
				aColumn.Title = Catalog.GetString(myCol);
				aColumn.PackStart (aCell, true);
				aColumn.SetCellDataFunc (aCell, new Gtk.TreeCellDataFunc (RenderRestTime));

				aColumn.SortColumnId = i;
				aColumn.SortIndicator = true;
				aColumn.Visible = visible;
				treeview.AppendColumn ( aColumn );
			}

			if(i == 1)
				store.SetSortFunc (0, UtilGtk.IdColumnCompare);

			visible = true;
		}
	}

	private void RenderRestTime (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		string restTime = (string) model.GetValue(iter, 2);

		if(RestSecondsMark > 0 && LastTestTime.GetSeconds(restTime) >= RestSecondsMark)
		{
			Gtk.ITreeModel model2;
			Gtk.TreeIter iter2;
			bool selected = false;
			if (treeview.Selection.GetSelected (out model2, out iter2))
				if(model.GetValue(iter, 0).ToString() == model2.GetValue(iter2, 0).ToString())
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

	/*
	 * this method works fine but does not show foreground in color when cell is selected
	 * above method solves this
	private void RenderRestTime (Gtk.TreeViewColumn column, Gtk.CellRenderer cell, Gtk.ITreeModel model, Gtk.TreeIter iter)
	{
		string restTime = (string) model.GetValue(iter, 2);
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

	public void Fill(ArrayList myPersons, RestTime rt)
	{
		foreach (Person person in myPersons)
		{
			//take care on null at restTime. This happens eg on start of session where SessionMode == UNIQUE
			string restedTime = "";
			if(rt != null && rt.RestedTime(person.UniqueID) != null)
				restedTime = rt.RestedTime(person.UniqueID);

			store.AppendValues ( new String [] {
					person.UniqueID.ToString(),
					person.Name.ToString(),
					restedTime }
					);
		}

		//show sorted by column name	
		store.SetSortColumnId(1, Gtk.SortType.Ascending);

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
				if(Convert.ToInt32 ((string) treeview.Model.GetValue (iter, 0)) == uniqueID) {
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
	public void Add (string jumperID, string jumperName)
	{
		TreeIter iter = new TreeIter();
		bool iterOk = store.GetIterFirst(out iter);
		int found = -1;

		int count = 0;
		if(iterOk) {
			do {
				//search until find when jumperName is lexicographically > than current row
				if(String.Compare(jumperName.ToUpper(), 
							((string) treeview.Model.GetValue (iter, 1)).ToUpper()) < 0 ) {
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
			store.SetValue (iter2, 0, jumperID);
			store.SetValue (iter2, 1, jumperName);
			store.SetValue (iter2, 2, ""); //restTime
		} else {
			//first ID, then Name
			iter2 = store.AppendValues (jumperID, jumperName, "");
		}
			
		//scroll treeview if needed
		TreePath path = store.GetPath (iter2);
		treeview.ScrollToCell (path, null, true, 0, 0);
	}

	public void UpdateRestTimes(RestTime restTime)
	{
		TreeIter iter;
		bool iterOk = store.GetIterFirst(out iter);
		if(iterOk) {
			do {
				string rested = restTime.RestedTime(
						Convert.ToInt32(store.GetValue(iter, 0)));
				if(rested != "")
					store.SetValue(iter, 2, rested);

			} while (store.IterNext (ref iter));
		}
	}
}

