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
 *  Copyright (C) 2004-2009   Xavier de Blas <xaviblas@gmail.com> 
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
	
	public TreeViewPersons ()
	{
	}
	
	public TreeViewPersons (Gtk.TreeView treeview)
	{
		this.treeview = treeview;

		store = getStore(2); 
		string [] columnsString = { "ID", Catalog.GetString("person")};
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
		foreach(string myCol in columnsString) {
			//treeview.AppendColumn (Catalog.GetString(myCol), new CellRendererText(), "text", i++);
			UtilGtk.CreateCols(treeview, store, Catalog.GetString(myCol), i++);
			if(i == 1)
				store.SetSortFunc (0, UtilGtk.IdColumnCompare);
		}
	}
	
	public int idColumnCompare (TreeModel model, TreeIter iter1, TreeIter iter2)     {
		int val1 = 0;
		int val2 = 0;
		val1 = Convert.ToInt32(model.GetValue(iter1, 0));
		val2 = Convert.ToInt32(model.GetValue(iter2, 0));
		
		return (val1-val2);
	}

	public void RemoveColumns() {
		Gtk.TreeViewColumn [] myColumns = treeview.Columns;
		foreach (Gtk.TreeViewColumn column in myColumns) {
			treeview.RemoveColumn (column);
		}
	}

	public void Fill(ArrayList myPersons)
	{
		foreach (Person person in myPersons) {
			store.AppendValues (person.IDAndName());
		}
		//show sorted by column name	
		store.SetSortColumnId(1, Gtk.SortType.Ascending);
		store.ChangeSortColumn();
			
	}
	
	//pass 0 for first row
	public void SelectRow(int rowNumber)
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
		}
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

		
	public void SelectNextRow(int personID)
	{
		SelectRow(FindRow(personID) +1);
	}
	
	public void SelectPreviousRow(int personID)
	{
		SelectRow(FindRow(personID) -1);
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
		} else {
			//first ID, then Name
			iter2 = store.AppendValues (jumperID, jumperName);
		}
			
		//scroll treeview if needed
		TreePath path = store.GetPath (iter2);
		treeview.ScrollToCell (path, null, true, 0, 0);
	}
}

