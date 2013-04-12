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
 *  Copyright (C) 2004-2012   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Text; //StringBuilder
using Gtk;
using Gdk;

//this class tries to be a space for methods that are used in different classes
//only Gtk related methods (not used bu the server) this is the differnece with Util
public class UtilGtk
{

	/*
	 *
	 * COMBO
	 *
	 */


	public static string ComboGetActive(ComboBox myCombo) {
		TreeIter iter;
		string myText = "";
		if (myCombo.GetActiveIter (out iter))
			myText = (string) myCombo.Model.GetValue (iter, 0);
		return myText;
	}
	
	//get the position of the active
	public static int ComboGetActivePos(ComboBox myCombo) {
		//ComboMakeActive returns the position of a searched value(string)
		//ComboGetActive gets the value of selected value(string)
		return ComboMakeActive(myCombo, ComboGetActive(myCombo));
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

	public static void ComboPackShowAndSensitive (Gtk.Box box, Gtk.ComboBox combo) {
		box.PackStart(combo, true, true, 0);
		box.ShowAll();
		combo.Sensitive = true;
	}
	

	/*
	 *
	 * TREEVIEW
	 *
	 */


	public static void CreateCols (Gtk.TreeView tv, Gtk.TreeStore store, 
			string name, int verticalPos, bool visible) {
		Gtk.TreeViewColumn myCol = new Gtk.TreeViewColumn (name, new CellRendererText(), "text", verticalPos);
		myCol.SortColumnId = verticalPos;
		myCol.SortIndicator = true;
		myCol.Visible = visible;
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

	public static int CountRows(Gtk.ListStore ls) {
		return(ls.IterNChildren());
	}
	public static int CountRows(Gtk.TreeStore ts) {
		return(ts.IterNChildren());
	}

	public static int IdColumnCompare (TreeModel model, TreeIter iter1, TreeIter iter2)     {
		int val1 = 0;
		int val2 = 0;
		val1 = Convert.ToInt32(model.GetValue(iter1, 0));
		val2 = Convert.ToInt32(model.GetValue(iter2, 0));
		
		return (val1-val2);
	}
	
	public static Gtk.TreeStore RemoveRow (Gtk.TreeView tv, Gtk.TreeStore store) {
		TreeModel model;
		TreeIter iter1;

		if (tv.Selection.GetSelected (out model, out iter1)) {
			store.Remove(ref iter1);
		}
		return store;
	}

	public static Gtk.TreeView RemoveColumns(Gtk.TreeView tv) {
		Gtk.TreeViewColumn [] myColumns = tv.Columns;
		foreach (Gtk.TreeViewColumn column in myColumns) {
			tv.RemoveColumn (column);
		}
		return tv;
	}


	/*
	 *
	 * COLORS
	 *
	 */

	/* howto find nice colors
	 * open R
	 * > colors()
	 * if you like lightblue2 and red2 then:
	 * > col2rgb(colors()[401])
	 * > col2rgb(colors()[554])
	 */
	public static Gdk.Color WHITE = new Gdk.Color(0xff,0xff,0xff);
	public static Gdk.Color BLACK = new Gdk.Color(0x00,0x00,0x00);
	public static Gdk.Color BLUE = new Gdk.Color(0x6c,0x77,0xab);
	public static Gdk.Color BLUE_CLEAR = new Gdk.Color(0xa0,0xa7,0xca);
	
	public static Gdk.Color RED_PLOTS = new Gdk.Color(238,0,0);
	public static Gdk.Color LIGHT_BLUE_PLOTS = new Gdk.Color(178,223,238);
	public static Gdk.Color BLUE_PLOTS = new Gdk.Color(0,0,238);
	

	public static void ColorsMenuLabel(Gtk.Label l) {
		l.ModifyFg(StateType.Active, WHITE);
		l.ModifyFg(StateType.Normal, BLACK);
	}
	
	public static void ColorsTestLabel(Gtk.Label l) {
		l.ModifyFg(StateType.Active, WHITE);
	}
	
	public static void ColorsRadio(Gtk.RadioButton r) {
		r.ModifyBg(StateType.Normal, WHITE);
		r.ModifyBg(StateType.Active, BLUE);
		r.ModifyBg(StateType.Prelight, BLUE_CLEAR);
	}
	
	public static void ColorsCheck(Gtk.CheckButton c) {
		c.ModifyBg(StateType.Normal, WHITE);
		c.ModifyBg(StateType.Active, BLUE);
		c.ModifyBg(StateType.Prelight, BLUE_CLEAR);
	}

	public static void ColorsCheckOnlyPrelight(Gtk.CheckButton c) {
		c.ModifyBg(StateType.Normal, WHITE);
		c.ModifyBg(StateType.Active, WHITE);
		c.ModifyBg(StateType.Prelight, BLUE_CLEAR);
	}


	private static Gdk.Color chronopicViewportDefaultBg;
	private static Gdk.Color chronopicLabelsDefaultFg;

	public static void ChronopicColors(Gtk.Viewport v, Gtk.Label l1, Gtk.Label l2, bool connected) {
		if(! v.Style.Background(StateType.Normal).Equal(BLUE))
			chronopicViewportDefaultBg = v.Style.Background(StateType.Normal);
		if(! l1.Style.Foreground(StateType.Normal).Equal(WHITE))
			chronopicLabelsDefaultFg = l1.Style.Foreground(StateType.Normal);

		if(connected) {
			v.ModifyBg(StateType.Normal, chronopicViewportDefaultBg);
			l1.ModifyFg(StateType.Normal, chronopicLabelsDefaultFg);
			l2.ModifyFg(StateType.Normal, chronopicLabelsDefaultFg);
		} else {
			v.ModifyBg(StateType.Normal, BLUE);
			l1.ModifyFg(StateType.Normal, WHITE);
			l2.ModifyFg(StateType.Normal, WHITE);
		}
	}

	/*
	 *
	 * PRETTY THINGS
	 *
	 */

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
	
	public static int WidgetWidth(Gtk.Widget w) {
		return w.Allocation.Width;
	}
	public static int WidgetHeight(Gtk.Widget w) {
		return w.Allocation.Height;
	}

	public static void IconWindow(Gtk.Window myWindow) {
		Gdk.Pixbuf chronojumpIcon = new Gdk.Pixbuf (null, Constants.FileNameIcon);
            	myWindow.Icon = chronojumpIcon;
	}

	public static void IconWindowGraph(Gtk.Window myWindow) {
		Gdk.Pixbuf chronojumpIcon = new Gdk.Pixbuf (null, Constants.FileNameIconGraph);
            	myWindow.Icon = chronojumpIcon;
	}


	
	public static TextBuffer TextViewPrint(string message) {
		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.Text = message;
		return tb;
	}

	//Done because align to the right on TreeView is not working
	public static string TVNumPrint (string num, int start, int dec) {
		string decS="}";
		if(dec==1)
			decS = ".0}";
		else if(dec==2)
			decS = ".00}";
		else if(dec==3)
			decS = ".000}";

		int inc;
		if(Convert.ToDouble(num) >= 10000)
			inc = 0;
		else if(Convert.ToDouble(num) >= 1000)
			inc = 1;
		else if(Convert.ToDouble(num) >= 100)
			inc = 2;
		else if(Convert.ToDouble(num) >= 10)
			inc = 3;
		else
			inc = 4;
		
		return "{0," + (start + inc).ToString() + ":0" + decS;
	}

	public static void PrintLabelWithTooltip (Gtk.Label l, string s) {
		l.Text = s;
		l.UseMarkup = true; 
		l.TooltipText = Util.RemoveMarkup(s);
	}


	/*
	 *
	 * DRAWINGAREA
	 *
	 */

	
	public static void ErasePaint(Gtk.DrawingArea da, Gdk.Pixmap px) {
		px.DrawRectangle (da.Style.WhiteGC, true, 0, 0, da.Allocation.Width, da.Allocation.Height);
		da.QueueDraw(); // -- refresh
	}
	
	//called for cleaning the graph of a event done before than the current
	public static void ClearDrawingArea(Gtk.DrawingArea da, Gdk.Pixmap px) 
	{
		if(px == null) 
			px = new Gdk.Pixmap (da.GdkWindow, da.Allocation.Width, da.Allocation.Height, -1);
		
		UtilGtk.ErasePaint(da, px);
	}
	


}
