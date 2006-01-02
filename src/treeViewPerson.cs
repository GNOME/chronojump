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
	
	public void Add (string jumperID, string jumperName)
	{
		//first name, then ID
		store.AppendValues (jumperName, jumperID);
	}
}

