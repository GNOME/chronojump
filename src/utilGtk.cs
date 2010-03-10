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
using System.Text; //StringBuilder
using Gtk;
using Gdk;

//this class tries to be a space for methods that are used in different classes
//only Gtk related methods (not used bu the server) this is the differnece with Util
public class UtilGtk
{
	public static void ResizeIfNeeded(Gtk.Window win) {
		int winX, winY;
		win.GetSize(out winX, out winY);
		int maxY = ScreenHeightFitted(true);
		if(winY > maxY)
			win.Resize(winX, maxY);
	}

	//(takes care)? of menu bar
	public static int ScreenHeightFitted(bool fit) {
		if(fit)
			return ScreenHeight() -25;
		else
			return ScreenHeight();
	}
	
	private static int ScreenHeight() {
		//libmono-cairo2.0-cil
		return Gdk.Display.Default.GetScreen(0).Height;
	}

	public static void IconWindow(Gtk.Window myWindow) {
		Gdk.Pixbuf chronojumpIcon = new Gdk.Pixbuf (null, Constants.FileNameIcon);
            	myWindow.Icon = chronojumpIcon;
	}

	public static void IconWindowGraph(Gtk.Window myWindow) {
		Gdk.Pixbuf chronojumpIcon = new Gdk.Pixbuf (null, Constants.FileNameIconGraph);
            	myWindow.Icon = chronojumpIcon;
	}

	public static string ComboGetActive(ComboBox myCombo) {
		TreeIter iter;
		string myText = "";
		if (myCombo.GetActiveIter (out iter))
			myText = (string) myCombo.Model.GetValue (iter, 0);
		return myText;
	}

	//this is better than the below method, because this search in current combo values
	//if we use a predefined set of values (like below method), we can have problems 
	//if some of the values in predefined list have been deleted on combo
	public static int ComboMakeActive(ComboBox myCombo, string searched) {
		int returnValue = 0;
		int count = 0;
		TreeIter iter;
		myCombo.Model.GetIterFirst(out iter);
		do {
			if( ((string) myCombo.Model.GetValue (iter, 0)) == searched)
				returnValue = count;
			count ++;
		} while (myCombo.Model.IterNext (ref iter));

		return returnValue;
	}

	//this is not truly gtk related		
	//better use above method
	public static int ComboMakeActive(string [] allValues, string searched) {
		int returnValue = 0;
		int count = 0;
		foreach (string str in allValues) {
			if (str == searched) {
				returnValue = count;
			}
			count ++;
		}
		return returnValue;
	}

	public static void ComboShowAll(ComboBox myCombo) {
		TreeIter iter;
		myCombo.Model.GetIterFirst(out iter);
		do {
			Log.WriteLine((string) myCombo.Model.GetValue (iter, 0));
		} while (myCombo.Model.IterNext (ref iter));
	}

	private static void comboDelAll(ComboBox myCombo) {
		//myCombo = ComboBox.NewText(); don't work
		TreeIter myIter;
		while(myCombo.Model.GetIterFirst(out myIter)) {
			myCombo.RemoveText(0);
		}
	}

	public static void ComboDelThisValue(ComboBox myCombo, string toRemove) {
		int i=0;
		TreeIter iter;
		myCombo.Model.GetIterFirst(out iter);
		do {
			string str = (string) myCombo.Model.GetValue (iter, 0);
			if(str == toRemove) {
				myCombo.RemoveText(i);
				return;
			}
			i++;
		} while (myCombo.Model.IterNext (ref iter));
	}

	//if there's no default value, simply pass a "" and there will be returned a 0, that's the first value of combo
	public static int ComboUpdate(ComboBox myCombo, string [] myData, string strDefault) {
		//1stdelete combo values
		comboDelAll(myCombo);

		//2nd put new values
		int i=0;
		int foundValue=0;
		foreach (string str in myData) {
			myCombo.AppendText (str);
			if(str == strDefault)
				foundValue = i;
			i++;
		}
		return foundValue;
	}

	//when only one value has to be displayed
	public static void ComboUpdate(ComboBox myCombo, string myData) {
		//1stdelete combo values
		comboDelAll(myCombo);

		//2nd put new values
		myCombo.AppendText (myData);
	}

	public static void CreateCols (Gtk.TreeView tv, Gtk.TreeStore store, string name, int verticalPos) {
		Gtk.TreeViewColumn myCol = new Gtk.TreeViewColumn (name, new CellRendererText(), "text", verticalPos);
		myCol.SortColumnId = verticalPos;
		myCol.SortIndicator = true;
		tv.AppendColumn ( myCol );
	}
	
	public static string [] GetCols(Gtk.TreeView tv, int first) {
		Gtk.TreeViewColumn [] cols = tv.Columns;
		string [] colNames = new String [cols.Length];
		int i=0;
		foreach (Gtk.TreeViewColumn col in cols) {
			if(i >= first)
				colNames[i] = col.Title;
			i++;
		}
		return colNames;
	}
	
	public static string GetCol(Gtk.TreeView tv, int colWanted) {
		Gtk.TreeViewColumn [] cols = tv.Columns;
		int i=0;
		foreach (Gtk.TreeViewColumn col in cols) 
			if(i++ == colWanted)
				return col.Title;
		
		return "";
	}

	public static int IdColumnCompare (TreeModel model, TreeIter iter1, TreeIter iter2)     {
		int val1 = 0;
		int val2 = 0;
		val1 = Convert.ToInt32(model.GetValue(iter1, 0));
		val2 = Convert.ToInt32(model.GetValue(iter2, 0));
		
		return (val1-val2);
	}

	public static void ComboPackShowAndSensitive (Gtk.Box box, Gtk.ComboBox combo) {
		box.PackStart(combo, true, true, 0);
		box.ShowAll();
		combo.Sensitive = true;
	}

}
