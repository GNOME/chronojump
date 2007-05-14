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
		string [] columnsString = { Catalog.GetString("person"), "ID" };
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
			treeview.AppendColumn (Catalog.GetString(myCol), new CellRendererText(), "text", i++);
		}
	}
	
	public void RemoveColumns() {
		Gtk.TreeViewColumn [] myColumns = treeview.Columns;
		foreach (Gtk.TreeViewColumn column in myColumns) {
			treeview.RemoveColumn (column);
		}
	}

	public void Fill(string [] myPersons)
	{
		foreach (string person in myPersons) {
			string [] myStringFull = person.Split(new char[] {':'});
			string [] myData = new String [2];
			//first name, then ID
			myData[1] = myStringFull[0].ToString();
			myData[0] = myStringFull[1].ToString();
			store.AppendValues (myData);
		}	
			
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
				if(Convert.ToInt32 ((string) treeview.Model.GetValue (iter, 1)) == uniqueID) {
					found = count;
				}
				count ++;
			} while (store.IterNext (ref iter) && found == -1);
		}
		return found;
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
							((string) treeview.Model.GetValue (iter, 0)).ToUpper()) < 0 ) {
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
			//first name, then ID
			store.SetValue (iter2, 0, jumperName);
			store.SetValue (iter2, 1, jumperID);
		} else {
			//first name, then ID
			iter2 = store.AppendValues (jumperName, jumperID);
		}
			
		//scroll treeview if needed
		TreePath path = store.GetPath (iter2);
		treeview.ScrollToCell (path, null, true, 0, 0);
	}
}

