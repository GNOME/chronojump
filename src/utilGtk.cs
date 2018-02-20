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
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using System.Collections.Generic; //List<T>
using System.IO;
using Gtk;
using Gdk;

//this class tries to be a space for methods that are used in different classes
//only Gtk related methods (not used bu the server) this is the differnece with Util
public class UtilGtk
{
	public static bool CanTouchGTK()
	{
		//Only first thread can touch GTK
		return (System.Threading.Thread.CurrentThread.ManagedThreadId.ToString() == "1");
	}

	/*
	 *
	 * COMBO
	 *
	 */

	public static Gtk.ComboBox ComboSelectPrevious(ComboBox myCombo)
	{
		int newPosition = myCombo.Active -1;
		if(newPosition >= 0)
			myCombo.Active = newPosition;

		return myCombo;
	}
	public static Gtk.ComboBox ComboSelectNext(ComboBox myCombo, out bool isLast)
	{
		TreeIter iter;
		myCombo.Model.GetIterFirst(out iter);
		int current = myCombo.Active;
		int count = 0;
		isLast = false;
		do {
			if(count > current)
			{
				myCombo.Active = count;
				isLast = false;

				return myCombo;
			}
			count ++;
		} while (myCombo.Model.IterNext (ref iter));

		isLast = true;
		return myCombo;
	}


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
			LogB.Information((string) myCombo.Model.GetValue (iter, 0));
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

	//for new code, better use the ComboUpdate(ComboBox, ArrayList)
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

	//more elegant method, with ArrayList	
	public static void ComboUpdate(ComboBox myCombo, ArrayList array) {
		//1stdelete combo values
		comboDelAll(myCombo);

		//2nd put new values
		foreach (string str in array)
			myCombo.AppendText (str);
	}
	
	public static void ComboUpdate(ComboBox myCombo, List<int> list) {
		//1stdelete combo values
		comboDelAll(myCombo);

		//2nd put new values
		foreach (int l in list)
			myCombo.AppendText (l.ToString());
	}
	
	public static void ComboUpdate(ComboBox myCombo, List<double> list) {
		//1stdelete combo values
		comboDelAll(myCombo);

		//2nd put new values
		foreach (double l in list)
			myCombo.AppendText (l.ToString());
	}
	
	public static void ComboAdd(ComboBox myCombo, string str) {
		myCombo.AppendText (str);
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

	/*
	 * DateColumnCompare depends on the column. Better define this method where is needed.
	 * Search for dateColumnCompare
	 *
	public static int DateColumnCompare (TreeModel model, TreeIter iter1, TreeIter iter2)     {
	}
	*/

	
	public static int GetSelectedRowUniqueID (Gtk.TreeView tv, Gtk.TreeStore store, int uniqueIDcol) {
		TreeModel model;
		TreeIter iter1;

		if (tv.Selection.GetSelected (out model, out iter1)) {
			 return Convert.ToInt32(model.GetValue(iter1, uniqueIDcol));
		}
		return -1;
	}

	//finds the row number (starting at 0) of a cell (usually an uniqueID in col 0)
	private static int getRowNumOfThisID(Gtk.TreeStore store, int colNum, int searchedID)
	{
		TreeIter iter;
		int count = 0;
		bool iterOk = store.GetIterFirst(out iter);
		while(iterOk) {
			int thisID = Convert.ToInt32((string) store.GetValue (iter, colNum));
			if(thisID == searchedID)
				return count;
			
			count ++;
			iterOk = store.IterNext(ref iter);
		}
		return -1;
	}

	//finds the row number (starting at 0) of a cell
	private static int getRowNumOfThisName(Gtk.TreeStore store, int colNum, string searchedName)
	{
		TreeIter iter;
		int count = 0;
		bool iterOk = store.GetIterFirst(out iter);
		while(iterOk) {
			string thisName = (string) store.GetValue (iter, colNum);
			if(thisName == searchedName)
				return count;

			count ++;
			iterOk = store.IterNext(ref iter);
		}
		return -1;
	}

	public static bool TreeviewSelectFirstRow(Gtk.TreeView tv, Gtk.TreeStore store, bool scrollToRow)
	{
		return treeviewSelectRow(tv, store, 0, scrollToRow);
	}
	//selects a row that has an uniqueID (usually at col 0)
	public static bool TreeviewSelectRowWithID(Gtk.TreeView tv, Gtk.TreeStore store, int colNum, int id, bool scrollToRow)
	{
		if(id <= 0)
			return false;

		int rowNum = getRowNumOfThisID(store, colNum, id);
		return treeviewSelectRow(tv, store, rowNum, scrollToRow);
	}
	public static bool TreeviewSelectRowWithName(Gtk.TreeView tv, Gtk.TreeStore store, int colNum, string name, bool scrollToRow)
	{
		if(name == null || name == "")
			return false;

		int rowNum = getRowNumOfThisName(store, colNum, name);
		return treeviewSelectRow(tv, store, rowNum, scrollToRow);
	}
	private static bool treeviewSelectRow(Gtk.TreeView tv, Gtk.TreeStore store, int rowNum, bool scrollToRow)
	{
		if(rowNum == -1)
			return false;

		//set the selected
		int count = 0;
		TreeIter iter;
		bool iterOk = store.GetIterFirst(out iter);
		while(iterOk) {
			if(count == rowNum) {
				//1 select row
				tv.Selection.SelectIter(iter);
		
				//2 scroll to that row
				if(scrollToRow) {
					TreePath path = store.GetPath (iter);
					LogB.Debug(path.ToString());
					tv.ScrollToCell (path, tv.Columns[0], true, 0, 0);
				}

				return true;
			}
			
			count ++;
			store.IterNext(ref iter);
		}
		return false;
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

	//startOrEnd is true at start, false at end
	public static void TreeviewAddRow(Gtk.TreeView tv, TreeStore store, string [] row, bool startOrEnd) {
		TreeIter iter = new TreeIter();
		store.GetIterFirst(out iter);
		
		if(startOrEnd)
			iter = store.InsertWithValues(0,row);
		else
			iter = store.AppendValues(row);

		//scroll treeview if needed
		TreePath path = store.GetPath (iter);
		tv.ScrollToCell (path, null, true, 0, 0);
	}

	public static void TreeviewScrollToLastRow(Gtk.TreeView tv, Gtk.ListStore store, int nrows) {
		TreeIter iter = new TreeIter();
		bool iterOk = store.GetIterFirst(out iter);
		if(! iterOk)
			return;

		for(int i=0; i < (nrows -1); i++)
			iterOk = tv.Model.IterNext (ref iter);
		
		if(! iterOk)
			return;
		
		TreePath path = store.GetPath (iter);
		LogB.Debug(path.ToString());
		tv.ScrollToCell (path, tv.Columns[0], true, 0, 0);
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
	public static Gdk.Color GRAY = new Gdk.Color(0x66,0x66,0x66);
	public static Gdk.Color BLUE = new Gdk.Color(0x6c,0x77,0xab);
	public static Gdk.Color BLUE_CLEAR = new Gdk.Color(0xa0,0xa7,0xca);
	public static Gdk.Color YELLOW = new Gdk.Color(0xff,0xcc,0x01);
	
	public static Gdk.Color GREEN_PLOTS = new Gdk.Color(0,200,0);
	public static Gdk.Color RED_PLOTS = new Gdk.Color(200,0,0);
	public static Gdk.Color LIGHT_BLUE_PLOTS = new Gdk.Color(178,223,238);
	public static Gdk.Color BLUE_PLOTS = new Gdk.Color(0,0,200);
	public static Gdk.Color BLUE_CHRONOJUMP = new Gdk.Color(14,30,70); //so dark, can be used only for background
	
	//used on encoder capture
	public static Gdk.Color RED_DARK = new Gdk.Color(140,0,0);
	public static Gdk.Color RED_LIGHT = new Gdk.Color(238,0,0);
	public static Gdk.Color GREEN_DARK = new Gdk.Color(0,140,0);
	public static Gdk.Color GREEN_LIGHT = new Gdk.Color(0,238,0);
	public static Gdk.Color BLUE_DARK = new Gdk.Color(0,0,140);
	public static Gdk.Color BLUE_LIGHT = new Gdk.Color(0,75,238);
	
	public static string ColorGood = "ForestGreen";
	public static string ColorBad = "red";
	public static string ColorNothing = "";

	
	public static Gdk.Color SELECTED = GetBackgroundColorSelected();


	public static Gdk.Color GetBackgroundColorSelected() {
		Gtk.Style regularLabel = Gtk.Rc.GetStyle (new Gtk.Label());

		return regularLabel.Background (StateType.Selected);
	}

	public static void ColorsMenuLabel(Gtk.Viewport v, Gtk.Label l) {
		l.ModifyFg(StateType.Active, v.Style.Foreground(StateType.Selected));
		l.ModifyFg(StateType.Prelight, v.Style.Foreground(StateType.Selected));
	}
	
	public static void ColorsTestLabel(Gtk.Viewport v, Gtk.Label l) {
		l.ModifyFg(StateType.Active, v.Style.Foreground(StateType.Selected));
		l.ModifyFg(StateType.Prelight, v.Style.Foreground(StateType.Selected));
	}
	
	public static void ColorsRadio(Gtk.Viewport v, Gtk.RadioButton r) {
		r.ModifyBg(StateType.Active, v.Style.Background(StateType.Selected));
		r.ModifyBg(StateType.Prelight, v.Style.Background(StateType.Selected));
	}

	public static void ColorsCheckbox(Gtk.Viewport v, Gtk.CheckButton c) {
		c.ModifyBg(StateType.Active, v.Style.Background(StateType.Selected));
		c.ModifyBg(StateType.Prelight, v.Style.Background(StateType.Selected));
	}
	
	
	public static void ColorsCheckOnlyPrelight(Gtk.CheckButton c) {
		//c.ModifyBg(StateType.Normal, WHITE);
		//c.ModifyBg(StateType.Active, WHITE);
		//c.ModifyBg(StateType.Prelight, BLUE_CLEAR);
		
		//c.ModifyBg(StateType.Active, c.Style.Background(StateType.Selected));
		//c.ModifyBg(StateType.Prelight, c.Style.Background(StateType.Selected));
	}

	public static void ColorsTreeView(Gtk.Viewport v, Gtk.TreeView tv) {
		tv.ModifyBg(StateType.Active, v.Style.Background(StateType.Selected));
		tv.ModifyBg(StateType.Prelight, v.Style.Background(StateType.Selected));
	}
	

	private static Gdk.Color chronopicViewportDefaultBg;
	private static Gdk.Color chronopicLabelsDefaultFg;

	public static void DeviceColors(Gtk.Viewport v, bool connected)
	{
		if(! v.Style.Background(StateType.Normal).Equal(v.Style.Background(StateType.Selected)))
			chronopicViewportDefaultBg = v.Style.Background(StateType.Normal);

		if(connected) {
			//v.ModifyBg(StateType.Normal, chronopicViewportDefaultBg);
			v.ModifyBg(StateType.Normal, WHITE);
		} else {
			//v.ModifyBg(StateType.Normal, BLUE);
			v.ModifyBg(StateType.Normal, v.Style.Background(StateType.Selected));
		}
	}

	public static void ViewportColor(Gtk.Viewport v, Gdk.Color color)
	{
		v.ModifyBg(StateType.Normal, color);
	}

	public static void ChronopicColors(Gtk.Viewport v, Gtk.Label l1, Gtk.Label l2, bool connected) {
		//if(! v.Style.Background(StateType.Normal).Equal(BLUE))
		if(! v.Style.Background(StateType.Normal).Equal(v.Style.Background(StateType.Selected)))
			chronopicViewportDefaultBg = v.Style.Background(StateType.Normal);
		if(! l1.Style.Foreground(StateType.Normal).Equal(WHITE))
			chronopicLabelsDefaultFg = l1.Style.Foreground(StateType.Normal);

		if(connected) {
			v.ModifyBg(StateType.Normal, chronopicViewportDefaultBg);
			l1.ModifyFg(StateType.Normal, chronopicLabelsDefaultFg);
			l2.ModifyFg(StateType.Normal, chronopicLabelsDefaultFg);
		} else {
			//v.ModifyBg(StateType.Normal, BLUE);
			v.ModifyBg(StateType.Normal, v.Style.Background(StateType.Selected));
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

	public static Button CreateArrowButton(ArrowType arrow_type, ShadowType shadow_type)
	{
		return CreateArrowButton(arrow_type, shadow_type, -1, -1);
	}
	public static Button CreateArrowButton(ArrowType arrow_type, ShadowType shadow_type, int width, int height)
	{
		Button button = new Button ();

		if(width > 0)
			button.WidthRequest = width;
		if(height > 0)
			button.HeightRequest = height;

		Arrow  arrow = new Arrow (arrow_type, shadow_type);

		button.Add(arrow);

		button.Show();
		arrow.Show();

		return button;
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
	
	/*
	 *
	 * IMAGE
	 *
	 */

	/*
	 * OpenImageSafe checks if it's created and if it has size and can be opened
	 * this is used when one process takes an image from another process and maybe is not finished
	 * Now we use imageFileWaitUntilCreated()
	 */
	private static void imageFileWaitUntilCreated(string filename) 
	{
		while( ! File.Exists(filename) );

		bool hasSize = false;
		do {
			FileInfo fi = new FileInfo(filename);
			if(fi.Length > 0)
				hasSize = true;
			else
				System.Threading.Thread.Sleep(50);
		} while( ! hasSize );
	}

	public static Gtk.Image OpenImageSafe(string filename, Gtk.Image image) 
	{
		imageFileWaitUntilCreated(filename);
		
		bool readedOk;
		do {
			readedOk = true;
			try {
				Pixbuf pixbuf = new Pixbuf (filename); //from a file
				image.Pixbuf = pixbuf;
			} catch {
				LogB.Warning("File is still not ready. Wait a bit");
				System.Threading.Thread.Sleep(50);
				readedOk = false;
			}
		} while( ! readedOk );

		return image;
	}

	public static Gdk.Pixbuf OpenPixbufSafe(string filename, Gdk.Pixbuf pixbuf) 
	{
		imageFileWaitUntilCreated(filename);
		
		bool readedOk;
		do {
			readedOk = true;
			try {
				pixbuf = new Pixbuf (filename); //from a file
			} catch {
				LogB.Warning("File is still not ready. Wait a bit");
				System.Threading.Thread.Sleep(50);
				readedOk = false;
			}
		} while( ! readedOk );

		return pixbuf;
	}



}
