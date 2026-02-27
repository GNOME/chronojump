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
 *  Copyright (C) 2004-2026   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using System.Collections.Generic; //List<T>
using System.IO;
using Gtk;
using Gdk;
using System.Text.RegularExpressions; //Regex

//this class tries to be a space for methods that are used in different classes
//only Gtk related methods (not used bu the server) this is the differnece with Util
public class UtilGtk
{
	public static bool CanTouchGTK()
	{
		//Only first thread can touch GTK
		return (System.Threading.Thread.CurrentThread.ManagedThreadId.ToString() == "1");
	}

	// to debug
	// eg call with: UtilGtk. WidgetIsSensitive (alignment_session_persons, "alignment_session_persons", "0");
	public static void WidgetIsSensitive (Gtk.Widget w, string wName, string whereInCode)
	{
		LogB.Information (string.Format ("Widget {0} is sensitive? {1} at {2}", wName, w.Sensitive, whereInCode));
	}

	/*
	 *
	 * COMBO
	 *
	 */

	public static Gtk.ComboBoxText CreateComboBoxText (Gtk.Box box, List<string> values_l, string activeStr)
	{
		Gtk.ComboBoxText combo = new ComboBoxText ();

		ComboUpdate (combo, values_l);
		if (activeStr == "")
			combo.Active = 0;
		else
			combo.Active = ComboMakeActive (combo, activeStr);

		box.PackStart (combo, false, false, 0);
		box.ShowAll ();

		return combo;
	}

	public static Gtk.ComboBoxText ComboSelectPrevious (ComboBoxText myCombo)
	{
		int newPosition = myCombo.Active -1;
		if(newPosition >= 0)
			myCombo.Active = newPosition;

		return myCombo;
	}
	public static Gtk.ComboBoxText ComboSelectPrevious (ComboBoxText myCombo, out bool isFirst)
	{
		int newPosition = myCombo.Active -1;
		if (newPosition >= 0)
			myCombo.Active = newPosition;

		isFirst = (myCombo.Active == 0);
		return myCombo;
	}

	public static Gtk.ComboBoxText ComboSelectNext (ComboBoxText myCombo, out bool isLast)
	{
		int total = ComboCount (myCombo);

		TreeIter iter;
		myCombo.Model.GetIterFirst(out iter);
		int current = myCombo.Active;
		int count = 0;
		do {
			if(count > current)
			{
				myCombo.Active = count;

				isLast = (count == total -1);
				return myCombo;
			}
			count ++;
		} while (myCombo.Model.IterNext (ref iter));

		isLast = true;
		return myCombo;
	}

	public static bool ComboSelectedIsLast (ComboBoxText myCombo)
	{
		TreeIter iter;
		myCombo.Model.GetIterFirst(out iter);
		int current = myCombo.Active;
		int count = 0;
		do {
			if(count > current)
				return false;
			count ++;
		} while (myCombo.Model.IterNext (ref iter));

		return true;
	}


	public static string ComboGetActive (ComboBoxText myCombo) {
		TreeIter iter;
		string myText = "";
		if (myCombo.GetActiveIter (out iter))
			myText = (string) myCombo.Model.GetValue (iter, 0);
		return myText;
	}
	
	//get the position of the active
	public static int ComboGetActivePos (ComboBoxText myCombo) {
		//ComboMakeActive returns the position of a searched value(string)
		//ComboGetActive gets the value of selected value(string)
		return ComboMakeActive(myCombo, ComboGetActive(myCombo));
	}

	public static int ComboCount (ComboBoxText myCombo) {
		int count = 0;
		TreeIter iter;
		myCombo.Model.GetIterFirst(out iter);
		do {
			count ++;
		} while (myCombo.Model.IterNext (ref iter));

		return count;
	}



	//this is better than the below method, because this search in current combo values
	//if we use a predefined set of values (like below method), we can have problems 
	//if some of the values in predefined list have been deleted on combo
	public static int ComboMakeActive (ComboBoxText myCombo, string searched) {
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

	public static void ComboShowAll (ComboBoxText myCombo) {
		TreeIter iter;
		myCombo.Model.GetIterFirst(out iter);
		do {
			LogB.Information((string) myCombo.Model.GetValue (iter, 0));
		} while (myCombo.Model.IterNext (ref iter));
	}

	//used on gui/preferences camera
	public static void ComboDelAll (ComboBoxText myCombo)
	{
		comboDelAll(myCombo);
	}

	private static void comboDelAll (ComboBoxText myCombo) {
		//myCombo = ComboBox.NewText(); don't work
		TreeIter myIter;
		while(myCombo.Model.GetIterFirst(out myIter)) {
			myCombo.Remove (0);
		}
	}

	public static void ComboDelByPosition (ComboBoxText myCombo, int pos)
	{
		myCombo.Remove (pos);
	}

	public static void ComboDelThisValue (ComboBoxText myCombo, string toRemove) {
		int i=0;
		TreeIter iter;
		myCombo.Model.GetIterFirst(out iter);
		do {
			string str = (string) myCombo.Model.GetValue (iter, 0);
			if(str == toRemove) {
				myCombo.Remove (i);
				return;
			}
			i++;
		} while (myCombo.Model.IterNext (ref iter));
	}

	//used on combo_select_contacts_top at least on runEncoder
	public static string [] ComboGetValues (ComboBoxText combo)
	{
		List<string> values_l = new List<string>();

		TreeIter iter;
		combo.Model.GetIterFirst(out iter);
		do {
			values_l.Add((string) combo.Model.GetValue (iter, 0));
		} while (combo.Model.IterNext (ref iter));

		return UtilList.ListStringToStringArray (values_l);
	}


	//for new code, better use the ComboUpdate(ComboBoxText, ArrayList)
	//if there's no default value, simply pass a "" and there will be returned a 0, that's the first value of combo
	public static int ComboUpdate (ComboBoxText myCombo, string [] myData, string strDefault) {
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
	public static void ComboUpdate (ComboBoxText myCombo, string myData) {
		//1stdelete combo values
		comboDelAll(myCombo);

		//2nd put new values
		myCombo.AppendText (myData);
	}

	//more elegant method, with ArrayList	
	public static void ComboUpdate (ComboBoxText myCombo, ArrayList array) {
		//1stdelete combo values
		comboDelAll(myCombo);

		//2nd put new values
		foreach (string str in array)
			myCombo.AppendText (str);
	}
	
	public static void ComboUpdate (ComboBoxText myCombo, List<int> list) {
		//1stdelete combo values
		comboDelAll(myCombo);

		//2nd put new values
		foreach (int l in list)
			myCombo.AppendText (l.ToString());
	}
	
	public static void ComboUpdate (ComboBoxText myCombo, List<double> list) {
		//1stdelete combo values
		comboDelAll(myCombo);

		//2nd put new values
		foreach (double l in list)
			myCombo.AppendText (l.ToString());
	}
	
	public static void ComboUpdate (ComboBoxText myCombo, List<string> list) {
		//1stdelete combo values
		comboDelAll(myCombo);

		//2nd put new values
		foreach (string l in list)
			myCombo.AppendText (l);
	}

	public static void ComboAdd (ComboBoxText myCombo, string str) {
		myCombo.AppendText (str);
	}


	public static void ComboPackShowAndSensitive (Gtk.Box box, Gtk.ComboBoxText combo) {
		box.PackStart(combo, true, true, 0);
		box.ShowAll();
		combo.Sensitive = true;
	}
	

	/*
	 *
	 * TREEVIEW
	 *
	 */

	public static Gtk.TreeStore GetStore (int columns)
	{
		//prepares the TreeStore for required columns
		Type [] types = new Type [columns];
		for (int i=0; i < columns; i++)
			types[i] = typeof (string);

		return new TreeStore(types);
	}

	public static void CreateCols (Gtk.TreeView tv, Gtk.TreeStore store, 
			string name, int verticalPos, bool visible)
	{
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

	//default
	public static int IdColumnCompare (ITreeModel model, TreeIter iter1, TreeIter iter2)
	{
		return idColumnCompareByCol (model, iter1, iter2, 0);
	}
	//Note is better to set & get set_sort_func() see: https://python-gtk-3-tutorial.readthedocs.io/en/latest/treeview.html
	public static int IdColumnCompareCol1 (ITreeModel model, TreeIter iter1, TreeIter iter2)
	{
		return idColumnCompareByCol (model, iter1, iter2, 1);
	}
	public static int IdColumnCompareCol2 (ITreeModel model, TreeIter iter1, TreeIter iter2)
	{
		return idColumnCompareByCol (model, iter1, iter2, 2);
	}
	public static int IdColumnCompareCol3 (ITreeModel model, TreeIter iter1, TreeIter iter2)
	{
		return idColumnCompareByCol (model, iter1, iter2, 3);
	}
	public static int IdColumnCompareCol4 (ITreeModel model, TreeIter iter1, TreeIter iter2)
	{
		return idColumnCompareByCol (model, iter1, iter2, 4);
	}
	//can select the col
	private static int idColumnCompareByCol (ITreeModel model, TreeIter iter1, TreeIter iter2, int col)
	{
		int val1 = 0;
		int val2 = 0;

		if (Util.IsNumber (model.GetValue (iter1, col).ToString (), false))
			val1 = Convert.ToInt32 (model.GetValue (iter1, col));
		if (Util.IsNumber (model.GetValue (iter2, col).ToString (), false))
			val2 = Convert.ToInt32 (model.GetValue (iter2, col));

		return (val1-val2);
	}

	/*
	 * DateColumnCompare depends on the column. Note here we are using column 0
	 * Other parts of the program use another column, search for dateColumnCompare
	 *
	 */
	public static int DateColumnCompare (ITreeModel model, TreeIter iter1, TreeIter iter2)
	{
		var dt1String = (model.GetValue(iter1, 0).ToString());
		var dt2String = (model.GetValue(iter2, 0).ToString());

		DateTime dt1;
		DateTime dt2;

		var converted1 = DateTime.TryParse(dt1String, out dt1);
		var converted2 = DateTime.TryParse(dt2String, out dt2);

		if(converted1 && converted2)
			return DateTime.Compare(dt1, dt2);
		else
			return 0;
	}
	
	public static int GetSelectedRowUniqueID (Gtk.TreeView tv, Gtk.TreeStore store, int uniqueIDcol) {
		ITreeModel model;
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
		ITreeModel model;
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

	//position 0 is start, -1 is end
	public static void TreeviewAddRow(Gtk.TreeView tv, TreeStore store, string [] row, int position) {
		TreeIter iter = new TreeIter();
		store.GetIterFirst(out iter);
		
		if(position == -1)
			iter = store.AppendValues(row);
		else
			iter = store.InsertWithValues(position, row);

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
	 * CONTAINER (also table)
	 *
	 */

	public static void  RemoveChildren (Gtk.Container c)
	{
		foreach (Gtk.Widget w in c.Children)
			c.Remove (w);
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
	public static Gdk.Color GRAY_LIGHT = new Gdk.Color(0x99,0x99,0x99);
	public static Gdk.Color BLUE = new Gdk.Color(0x6c,0x77,0xab);
	public static Gdk.Color BLUE_CLEAR = new Gdk.Color(0xa0,0xa7,0xca);
	public static Gdk.Color BLUE_CLEAR2 = new Gdk.Color(0x02,0x4e,0x69); //used on background of start buttons (modes)
	public static Gdk.Color YELLOW = new Gdk.Color(0xff,0xcc,0x01);
	public static Gdk.Color YELLOW_LIGHT = new Gdk.Color(0xf3,0xde,0x8c);
	public static Gdk.Color YELLOW_MID = new Gdk.Color(0xed,0xce,0x52);
	public static Gdk.Color YELLOW_DARK = new Gdk.Color(0xcd,0xcd,0x00);
	public static Gdk.Color ORANGE_DARK = new Gdk.Color(0xff,0x7f,0x00);
	
	public static Gdk.Color GREEN_PLOTS = new Gdk.Color(0,200,0);
	public static Gdk.Color RED_PLOTS = new Gdk.Color(200,0,0);
	public static Gdk.Color LIGHT_BLUE_PLOTS = new Gdk.Color(178,223,238);
	public static Gdk.Color BLUE_PLOTS = new Gdk.Color(0,0,200);
	public static Gdk.Color BLUE_CHRONOJUMP = new Gdk.Color(14,30,70); //so dark, can be used only for background (is the 0e1e46)
	
	//used on encoder capture
	public static Gdk.Color RED_DARK = new Gdk.Color(140,0,0); //#8c0000
	public static Gdk.Color RED_LIGHT = new Gdk.Color(238,0,0);
	public static Gdk.Color GREEN_DARK = new Gdk.Color(0,140,0);
	public static Gdk.Color GREEN_LIGHT = new Gdk.Color(0,238,0);
	public static Gdk.Color BLUE_DARK = new Gdk.Color(0,0,140);
	public static Gdk.Color BLUE_LIGHT = new Gdk.Color(0,75,238);
	public static Gdk.Color MAGENTA = new Gdk.Color(255,0,255);
	
	public static string ColorGood = "ForestGreen";
	public static string ColorBad = "red";
	public static string ColorNothing = "";
	public static string ColorGray = "gray";
	public static string ColorBlack = "black";

	public enum Colors {
		BLACK, BLUE_CHRONOJUMP, BLUE_LIGHT, BLUE_PLOTS,
		GRAY, GRAY_LIGHT, GRAY_180, GREEN_PLOTS, GREEN_LIGHT, GREEN_DARK,
		RED_PLOTS, RED_DARK, YELLOW, YELLOW_LIGHT, YELLOW_MID, WHITE }

	public static RGBA GetRGBA (Colors colname)
	{
		RGBA color = new RGBA ();

		if (colname == Colors.BLACK)
			color.Parse ("#000000");
		else if (colname == Colors.BLUE_CHRONOJUMP)
			color.Parse ("#0e1e46");
		else if (colname == Colors.BLUE_LIGHT)
			color.Parse ("#004bee");
		else if (colname == Colors.BLUE_PLOTS)
			color.Parse ("#0000c8");
		else if (colname == Colors.GRAY)
			color.Parse ("#666666");
		else if (colname == Colors.GRAY_LIGHT)
			color.Parse ("#999999");
		else if (colname == Colors.GRAY_180)
			color.Parse ("#b4b4b4");
		else if (colname == Colors.GREEN_PLOTS)
			color.Parse ("#00c800");
		else if (colname == Colors.GREEN_LIGHT)
			color.Parse ("#00ee00");
		else if (colname == Colors.GREEN_DARK)
			color.Parse ("#008c00");
		else if (colname == Colors.RED_PLOTS)
			color.Parse ("#c80000");
		else if (colname == Colors.RED_DARK)
			color.Parse ("#8c0000");
		else if (colname == Colors.YELLOW)
			color.Parse ("#ffcc01");
		else if (colname == Colors.YELLOW_LIGHT)
			color.Parse ("#f3de8c");
		else if (colname == Colors.YELLOW_MID)
			color.Parse ("#edce52");
		else if (colname == Colors.WHITE)
			color.Parse ("#ffffff");

		return color;
	}

	public static string GetRGBAs (Colors colname)
	{
		return GetRGBA (colname).ToString ();
	}

	public static bool ColorIsDark (RGBA color)
	{
		//LogB.Information(string.Format("color red: {0}, green: {1}, blue: {2}", color.Red, color.Green, color.Blue));
		//3 components come in ushort (0-65535)
		//return (color.Red + color.Green + color.Blue < 3 * 65535 / 2.0);

		//https://stackoverflow.com/questions/3942878/how-to-decide-font-color-in-white-or-black-depending-on-background-color
		//return (color.Red * 0.299 + color.Green * 0.587 + color.Blue * 0.114 < 186 * 256); //each one is 0,255
		//changed from 186 to 150. Because have eg (rgb 256) 90,205,242 is (rgb 0-1) .35,.8,.95, in the formula= 175, and it's pretty clear
		return (color.Red * 256 * 0.299 + color.Green * 256 * 0.587 + color.Blue * 256 * 0.114 < 150);
	}
	public static bool ColorIsDark (string colorString)
	{
		return ColorIsDark(ColorParse(colorString));
	}


	//darker or clearer
	public static RGBA GetColorShifted (RGBA color, bool darker)
	{
		//LogB.Information(string.Format("colshifted start: red {0}, green {1}, blue {2}",
		//			color.Red, color.Green, color.Blue));
		RGBA colShifted = new RGBA ();
		if(darker)
		{
			colShifted.Red = color.Red -.25;
			colShifted.Green = color.Green -.25;
			colShifted.Blue = color.Blue -.25;
		} else {
			colShifted.Red = color.Red +.25;
			colShifted.Green = color.Green +.25;
			colShifted.Blue = color.Blue +.25;
		}
		//LogB.Information(string.Format("colshifted end: red {0}, green {1}, blue {2}",
		//			colShifted.Red, colShifted.Green, colShifted.Blue));
		colShifted.Alpha = 1; //required

		return colShifted;
	}

	//if color is too white or too yellow, it will not be ok
	public static bool ColorIsOkWithLogoTransparent (RGBA color)
	{
		return ( ! colorIsQuiteClear (color) && ! colorIsYellow (color) );
	}

	//if is very clear it does not show ok the yellow of Boscosystem letters (also if it is clear blue)
	private static bool colorIsQuiteClear (RGBA color)
	{
		return (color.Red * 256 > 150 && color.Green * 256 > 150 && color.Blue * 256 > 150);
	}

	//yellow is red + green
	private static bool colorIsYellow (RGBA color)
	{
		return (color.Red * 256 > 200 && color.Green * 256 > 200 && color.Blue * 256 < 50);
	}

	//method withoud the need of HSV
	//Myindex comment on https://stackoverflow.com/a/9733452
	private static double colorsContrast (RGBA color1, RGBA color2)
	{
		return Math.Abs (colorBrightness (color1) - colorBrightness (color2));
	}
	//method withoud the need of HSV
	//Myindex comment on https://stackoverflow.com/a/9733452
	private static double colorBrightness (RGBA color)
	{
		return .2126*color.Red + .7152*color.Green + .0722*color.Blue;
	}

	public static int GetRadioButtonPosInList (List<Gtk.RadioButton> r_l, Gtk.RadioButton r)
	{
		for (int i = 0; i < r_l.Count; i ++)
			if (r_l[i] == r)
				return i;

		return -1;
	}

	/*
	public static Gdk.Color GetBackgroundColorSelected() {
		Gtk.Style regularLabel = Gtk.Rc.GetStyle (new Gtk.Label());

		return regularLabel.Background (StateType.Selected);
	}
	*/

	//TODO: have all the css names as enums in order to fail in compile if names like darkCss have any typo
	public static void ApplyCSS (int fontSizeAtGui)
	{
		string cssStr = applyCssGetStringAll (fontSizeAtGui);
		cssStr += applyCssGetStringExternalWindow ();

		applyCssDo (cssStr);
	}
	public static void ApplyCSSExternalWindow ()
	{
		string cssStr = applyCssGetStringExternalWindow ();
		applyCssDo (cssStr);
	}

	private static void applyCssDo (string cssStr)
	{
		CssProvider css = new CssProvider ();
		css.LoadFromData (cssStr);
		Gtk.StyleContext.RemoveProviderForScreen (Gdk.Screen.Default, css); //needed
		Gtk.StyleContext.AddProviderForScreen (Gdk.Screen.Default, css, 800); //needed
	}

	private static string applyCssGetStringAll (int fontSizeAtGui)
	{
		string colBgS = Config.ColorBackground.ToString ();
		string colShiftedS = Config.ColorBackgroundShifted.ToString ();

		//NOTEBOOK bgCss header, and any label with big contrast with bg
		string colLabelNotebookBgCss = "#ffffff";
		if (! Config.ColorBackgroundIsDark)
			colLabelNotebookBgCss = "#000000";

		string colLabelContrastBgCss = GetRGBAs (Colors.YELLOW); //also used on hover
		if (colorsContrast (GetRGBA (Colors.YELLOW), Config.ColorBackground) <
				colorsContrast (GetRGBA (Colors.BLUE_CHRONOJUMP), Config.ColorBackground))
			colLabelContrastBgCss = GetRGBAs (Colors.BLUE_CHRONOJUMP);


		//NOTEBOOK shiftedCss header, and any label with big contrast with bg
		string colLabelNotebookShiftedCss = "#ffffff";
		if (! Config.ColorBackgroundShiftedIsDark)
			colLabelNotebookShiftedCss = "#000000";

		string colLabelContrastShiftedCss = GetRGBAs (Colors.YELLOW); //also used on hover
		if (colorsContrast (GetRGBA (Colors.YELLOW), Config.ColorBackgroundShifted) <
				colorsContrast (GetRGBA (Colors.BLUE_CHRONOJUMP), Config.ColorBackgroundShifted))
			colLabelContrastShiftedCss = GetRGBAs (Colors.BLUE_CHRONOJUMP);

		// font size at gui ---->
		//default font size
		Gtk.Viewport vTemp = new Gtk.Viewport();
		Pango.FontDescription fd = vTemp.StyleContext.GetFont (Gtk.StateFlags.Normal); //this is showing warning but it works: 'StyleContext.GetFont(StateFlags)' is obsolete
		//Pango.FontDescription fd = vTemp.Style.FontDescription;	//this could be the solution but fails on compile: error CS1061: 'Style' does not contain a definition for 'FontDescription' and no accessible extension method 'FontDescription' accepting a first argument of type 'Style' could be found (are you missing a using directive or an assembly reference?)

		int defaultFontSize = Convert.ToInt32 (UtilAll.DivideSafe (fd.Size, Pango.Scale.PangoScale));
		LogB.Information ("default font size: " + defaultFontSize.ToString ());

		string labelFontSize = "";
		string labelFontSizeBig = ""; //used on some results like force sensor max on capture tab
		if (fontSizeAtGui >= 0) {
			labelFontSize = string.Format ("font-size: {0}pt;", fontSizeAtGui);
			labelFontSizeBig = string.Format ("font-size: {0}pt;", Convert.ToInt32 (fontSizeAtGui * 1.6));
		}
		else {
			labelFontSize = string.Format ("font-size: {0}pt;", defaultFontSize); //we need to restore it if user selected custom previously
			labelFontSizeBig = string.Format ("font-size: {0}pt;", Convert.ToInt32 (defaultFontSize * 1.6)); //we need to restore it if user selected custom previously
		}

		// <---- font size at gui

		string cssStr =
			//LABELS
			//labels lightCss in light color
			"label#lightCss {" +
				"color: " + GetRGBAs (Colors.WHITE) + ";" +
			"}" +
			//labels darkCss in dark color
			"label#darkCss {" +
				"color: #222222;" +
			"}" +
			//labels big
			"label#big_lightCss {" +
				"color: " + GetRGBAs (Colors.WHITE) + ";" +
				labelFontSizeBig +
			"}" +
			"label#big_darkCss {" +
				"color: #222222;" +
				labelFontSizeBig +
			"}" +
			//labels monospace
			"label#monospace_lightCss {" +
				"color: " + GetRGBAs (Colors.WHITE) + ";" +
				"font-family: monospace;" +
			"}" +
			"label#monospace_darkCss {" +
				"color: #222222;" +
				"font-family: monospace;" +
			"}" +
			//labels big monospace
			"label#big_monospace_lightCss {" +
				"color: " + GetRGBAs (Colors.WHITE) + ";" +
				labelFontSizeBig +
				"font-family: monospace;" +
			"}" +
			"label#big_monospace_darkCss {" +
				"color: #222222;" +
				labelFontSizeBig +
				"font-family: monospace;" +
			"}" +
			"label#labelAlertCss {" +
				//"color: #8c0000;" + //8c0000 is ok for backfound but too dark for label
				"color: #ee0000;" +
			"}" +
			"label#labelAlertBigCss {" +
				//"color: #8c0000;" + //8c0000 is ok for backfound but too dark for label
				"color: #ee0000;" +
				labelFontSizeBig +
			"}" +
			//label used on version hidden
			/*
			"label#blueChronojumpHideCss {" +
				"color: " + GetRGBAs (Colors.BLUE_CHRONOJUMP) + ";" +
				"background-color: " + GetRGBAs (Colors.BLUE_CHRONOJUMP) + ";" +
			"}" +
			*/
			"label#ChronojumpHideCss {" +
				"color: " + Config.ColorBackground.ToString () + ";" +
				"background-color: " + Config.ColorBackground.ToString () + ";" +
			"}" +
			"label#ContrastBgCss {" +
				"color: " + colLabelContrastBgCss + ";" +
			"}" +
			"label#ContrastShiftedCss {" +
				"color: " + colLabelContrastShiftedCss + ";" +
			"}" +
			//rest of labels
			"label {" +
				"color: #000000;" +
				labelFontSize +
			"}" +

			//RADIOS, CHECKBUTTONS & BUTTONS
			//radio normal
			"radio {" +
				"color: #ffffff;" +// background: " + Config.ColorBackgroundShifted.ToString () + ";" + //this makes it emtpy ig bg is shifted
			"}" +
			//radio checked
			"radio:checked {" +
				"color: " + GetRGBAs (Colors.YELLOW_LIGHT) + ";" + // background: " + Config.ColorBackgroundShifted.ToString () + ";" +
			"}" +
			"radio:disabled {" +
			    "background: #999999;" +
			    "background-color: #999999;" +
			"}" +
			"radio:disabled label {" +
			    "color: #666666;" + //this is not working, maybe label is not in radio
			"}" +

			//checkbutton checked
			"checkbutton:checked {" +
				"color: " + GetRGBAs (Colors.YELLOW_LIGHT) + ";" + // background: " + Config.ColorBackgroundShifted.ToString () + ";" +
			"}" +
			"checkbutton:disabled check {" +
			    "background: #cccccc;" +
			"}" +
			//button checked
			"button:checked {" +
				"background: " + GetRGBAs (Colors.YELLOW_LIGHT) + ";" + //TODO: try a YELLOW_MID
			"}" +
			"button:disabled {" +
			    "background: #cccccc;" +
			    "background-color: #cccccc;" +
			"}" +
			"button:disabled label {" +
			    "color: #666666;" +
			"}" +
			// it makes it shorter (in width) maybe implement in the future in buttons like signal ai move
			"button#button_small {" +
			    "padding: 0px;" +
			    "min-height: 0px;" +
			    "min-width: 0px;" +
			"}" +
			"entry:disabled {" +
			    "background: #cccccc;" +
			    "background-color: #cccccc;" +
			"}" +

                       // treeview
                       "treeview.view:selected {" + //the selected line
                               "color: " + GetRGBAs (Colors.BLACK) + ";" +
                               "background: " + GetRGBAs (Colors.YELLOW_LIGHT) + ";" +
			"}" +
			"treeview*:checked {" + // the active checkbutton on treeview_encoder_capture_curves
				"color: " + GetRGBAs (Colors.BLACK) + ";" +
				"background: " + GetRGBAs (Colors.YELLOW_LIGHT) + ";" +
			"}" +

			/*
			"scrollbar {" +
				//"overlay-scrolling: FALSE;" +
				//"overlay-indicator: FALSE;" +
				"overlay.indicator: FALSE;" +
			"}" +
			*/

			//PROGRESSBAR
			"progressbar#lightCss text {" +
				"color: " + GetRGBAs (Colors.WHITE) + ";" +
			"}" +
			"progressbar#darkCss text {" +
				"color: #222222;" +
			"}" +

			//SPINNER
			"spinner#shiftedCss {" +
				"color: " + colLabelNotebookShiftedCss + ";" +
			"}" +

			//ANY WIDGET
			//any widget bgCss
			"*#bgCss {" +
				"background: " + colBgS + ";" +
			"}" +
			//any widget shiftedCss
			"*#shiftedCss {" +
				"background: " + colShiftedS + ";" +
			"}" +
			//any widget whiteBgCss
			"*#whiteBgCss {" +
				"background: #ffffff;" +
			"}" +
			"*#gray180Css {" +
				"background: #b4b4b4;" +
			"}" +
			"*#alertCss {" +
				"background-color: #8c0000;" +
				//"color: #8c0000;" +
			"}" +

			//VIEWPORT
			"viewport#yellowLightCss {" +
				//"background-image: none;" +
				"background: " + GetRGBAs (Colors.YELLOW_LIGHT) + ";" +
			"}" +
			"viewport#greenLightCss {" +
				//"background-image: none;" +
				"background: " + GetRGBAs (Colors.GREEN_LIGHT) + ";" +
			"}" +
			"viewport#whiteCss {" +
				//"background-image: none;" +
				"background: #ffffff;" +
			"}" +
			//using border as app1s_viewport_checkbutton_show_data_jumps ... are very thin
			"viewport#bgCss {" +
				//"background-image: none;" +
				"background: " + Config.ColorBackground.ToString () + ";" +
				"border: " + Config.ColorBackground.ToString () + ";" +
			"}" +

			//SEPARATOR
			"separator#caramelCss {" +
				//"background-image: none;" +
				"background: #cf7d00;" +
				"border-width: 1px;" +
				"border: #ffffff;" +
			"}" +
			"separator#brownCss {" +
				//"background-image: none;" +
				"background: #964b00;" +
				"border-width: 1px;" +
				"border: #ffffff;" +
			"}" +
			"separator#blackCss {" +
				"background: #000000;" +
				"border-width: 1px;" +
				"border: #ffffff;" +
			"}" +

			//NOTEBOOKS bgCss
			//header
			"notebook#bgCss header {" +
				"background-color: " + colBgS + ";" +
			"}" +
			//label of the tab
			"notebook#bgCss tab label {" +
				"color: " + colLabelNotebookBgCss + ";" +
			"}" +
			//label of the tab (checked)
			"notebook#bgCss tab:checked label {" +
				"color: " + colLabelContrastBgCss + ";" + //TODO: try a YELLOW_MID
			"}" +
			"notebook#bgCss tab:checked {" +			// needed for KDE
				"background-color: " + colBgS + ";" +
			"}" +
			//bg of the tab (hover)
			"notebook#bgCss tab:hover {" +
				"background-color: " + colBgS + ";" +
			"}" +
			//color of the label of the tab (hover)
			"notebook#bgCss tab:hover label {" +
				"color: " + colLabelContrastBgCss + ";" + //TODO: try a YELLOW_MID
			"}" +
			//content of the notebook option 1 white
			/*
			"notebook#bgCss stack {" +
				"background-color: #ffffff;" +
			"}" +
			*/
			//content of the notebook option 2 shifted, then need to:
			//UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundShiftedIsDark, notebook)
			"notebook#bgCss stack {" +
				"background-color: " + colShiftedS + ";" +
			"}" +

			//NOTEBOOKS shiftedCss
			//(used in notebooks inside other notebooks, like on preferences)
			//header
			"notebook#shiftedCss header {" +
				"background-color: " + colShiftedS + ";" +
			"}" +
			//label of the tab
			"notebook#shiftedCss tab label {" +
				"color: " + colLabelNotebookShiftedCss + ";" +
			"}" +
			//label of the tab (checked)
			"notebook#shiftedCss tab:checked label {" +
				"color: " + colLabelContrastShiftedCss + ";" + //TODO: try a YELLOW_MID
			"}" +
			"notebook#shiftedCss tab:checked {" + 			// needed for KDE
				"background-color: " + colShiftedS + ";" +
			"}" +
			//bg of the tab (hover)
			"notebook#shiftedCss tab:hover {" +
				"background-color: " + colShiftedS + ";" +
			"}" +
			//color of the label of the tab (hover)
			"notebook#shiftedCss tab:hover label {" +
				"color: " + colLabelContrastShiftedCss + ";" + //TODO: try a YELLOW_MID
			"}" +
			//content of the notebook option 1 white
			/*
			"notebook#shiftedCss stack {" +
				"background-color: #ffffff;" +
			"}" +
			*/
			//content of the notebook option 2 shifted, then need to:
			//UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundShiftedIsDark, notebook)
			"notebook#shiftedCss stack {" +
				"background-color: " + colShiftedS + ";" +
			"}" +

			//TOOLTIPS
			"tooltip {" +
				"background-color: " + GetRGBAs (Colors.BLUE_CHRONOJUMP) + ";" +
				//"border-width: 1px;" +
				"border-style: dotted;" + //or dotted, solid, ... see ttps://docs.gtk.org/gtk3/css-properties.html
				"border-color: #ffffff;" +
			"}" +
			"tooltip label {" +
				"color: #ffffff;" +
			"}" +
			"textview#fontSize9 {" +
			"	font-size: 9pt;" +
			"}";

		return cssStr;
	}

	// separated because it could be called multiple times on spinbutton change
	private static string applyCssGetStringExternalWindow ()
	{
		string labelFontSizeExternalWindow = string.Format ("font-size: {0}pt;", Constants.FontSizeExternalWindow);
		return "label#externalWindow_monospace_lightCss {" +
				"color: " + GetRGBAs (Colors.WHITE) + ";" +
				labelFontSizeExternalWindow +
				"font-family: monospace;" +
			"}" +
			"label#externalWindow_monospace_darkCss {" +
				"color: #222222;" +
				labelFontSizeExternalWindow +
				"font-family: monospace;" +
			"}";
	}

	public static bool LogoBlueOrWhite (RGBA bg)
	{
		return (colorsContrast (bg, GetRGBA (Colors.BLUE_CHRONOJUMP)) >=
				colorsContrast (bg, GetRGBA (Colors.WHITE))
		       );
	}

	//private static RGBA chronopicViewportDefaultBg;
	//private static RGBA chronopicLabelsDefaultFg;

	public static void DeviceColors (Gtk.Viewport v, bool connected)
	{
		/* disabled un gtk3 until find way to do it with css (if needed)

		//if(! (v.StyleContext.GetBackgroundColor (StateFlags.Normal)).Equal(v.StyleContext.GetBackgroundColor (StateFlags.Selected)))
		RGBA a = v.StyleContext.GetBackgroundColor (StateFlags.Normal);
		RGBA b = v.StyleContext.GetBackgroundColor (StateFlags.Selected);
		if(! a.Equals (b))
			chronopicViewportDefaultBg = v.StyleContext.GetBackgroundColor (StateFlags.Normal);

		if(connected) {
			//v.ModifyBg(StateType.Normal, chronopicViewportDefaultBg);
			v.OverrideBackgroundColor (StateFlags.Normal, UtilGtk.GetRGBA (UtilGtk.Colors.WHITE));
		} else {
			//v.ModifyBg(StateType.Normal, BLUE);
			v.OverrideBackgroundColor (StateFlags.Normal, v.StyleContext.GetBackgroundColor (StateFlags.Selected));
		}
		*/
	}

	public static void WindowColor (Gtk.Window w, RGBA color)
	{
		w.Name = "bgCss";
	}

	public static void DialogColor (Gtk.Dialog d, RGBA color)
	{
		d.Name = "bgCss";
	}

	public static void ViewportColor (Gtk.Viewport v, Colors color)
	{
		//v.OverrideBackgroundColor (StateFlags.Normal, GetRGBA (color));
	}
	public static void ViewportColor (Gtk.Viewport v, RGBA color)
	{
		//v.OverrideBackgroundColor (StateFlags.Normal, color);
	}

	public static void ViewportColorBg (Gtk.Viewport v)
	{
		v.Name = "bgCss"; //bg color (has contrast with shifted)
	}
	public static void ViewportColorYellowLight (Gtk.Viewport v)
	{
		v.Name = "yellowLightCss";
	}
	public static void ViewportColorGreenLight (Gtk.Viewport v)
	{
		v.Name = "greenLightCss";
	}
	public static void ViewportColorWhite (Gtk.Viewport v)
	{
		v.Name = "whiteCss";
	}

	public static void WidgetColorGray180 (Gtk.Widget w)
	{
		w.Name = "gray180Css";
	}

	public static void WidgetColor (Gtk.Widget w, RGBA color)
	{
		w.Name = "shiftedCss";
	}

	//does not work in gtk3 //just use ViewportColorWhite
	/*
	public static void ViewportColorDefault (Gtk.Viewport v)
	{
		//v.ModifyBg(StateType.Normal); //resets to the default color

		//create a new viewport and get the color
		Gtk.Viewport vTemp = new Gtk.Viewport();
		*/
		/*
		Gdk.Color colorViewportDefault = vTemp.StyleContext.GetBackgroundColor (StateFlags.Normal);

		//assign the color to our requested viewport
		v.ModifyBg(StateType.Normal, colorViewportDefault); //resets to the default color
		*/
		/*
		v.OverrideBackgroundColor (StateFlags.Normal, vTemp.StyleContext.GetBackgroundColor (StateFlags.Normal));

		RGBA newColor = vTemp.StyleContext.GetBackgroundColor (StateFlags.Normal);
		LogB.Information(string.Format("newColor end: red {0}, green {1}, blue {2}",
					newColor.Red, newColor.Green, newColor.Blue)); //bad: 0,0,0
	}
	*/

	/* disabled on gtk3 until find a way to do it (if needed)
	//changes of colors without widgets that are in a EventBox
	public static void EventBoxColorBackgroundActive (Gtk.EventBox e, Colors colorActive, Colors colorPrelight)
	{
		e.OverrideColor (StateFlags.Active, GetRGBA (colorActive));
		e.OverrideColor (StateFlags.Prelight, GetRGBA (colorPrelight));
	}
	*/

	public static void ContrastLabelsBox (bool bgDark, Gtk.Box box)
	{
		contrastLabelsContainer (bgDark, (Gtk.Container) box);
	}
	public static void ContrastLabelsTable (bool bgDark, Gtk.Table table)
	{
		contrastLabelsContainer (bgDark, (Gtk.Container) table);
	}
	public static void ContrastLabelsGrid (bool bgDark, Gtk.Grid grid)
	{
		contrastLabelsContainer (bgDark, (Gtk.Container) grid);
	}
	public static void ContrastLabelsNotebook (bool bgDark, Gtk.Notebook notebook)
	{
		contrastLabelsContainer (bgDark, (Gtk.Container) notebook);

		//the labels
		for (int i = 0; i < notebook.NPages; i ++)
		{
			Gtk.Widget w = notebook.GetTabLabel (notebook.GetNthPage(i));
			if (w.GetType() == typeof(Gtk.Label))
				UtilGtk.ContrastLabelsLabel (Config.ColorBackgroundShiftedIsDark, (Gtk.Label) w);
			else if (w.GetType() == typeof(Gtk.Box))
				UtilGtk.ContrastLabelsBox (Config.ColorBackgroundShiftedIsDark, (Gtk.Box) w);
		}
	}
	/*
	 * Treeview is not colorized yet as we do not found a way to colorize headers using gtk 3.0
	public static void ContrastLabelsTreeView (bool bgDark, Gtk.TreeView tv)
	{
		//contrastLabelsContainer (bgDark, (Gtk.Container) tv);

		foreach(Gtk.TreeViewColumn c in tv)
		{
			UtilGtk.WidgetColor (c.Widget, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsWidget (Config.ColorBackgroundShiftedIsDark, c.Widget);
			//contrastLabelsContainer (bgDark, (Gtk.Container) c);
		}
	}
	*/

	public static void ContrastLabelsFrame (bool bgDark, Gtk.Frame frame)
	{
		contrastLabelsContainer (bgDark, (Gtk.Container) frame);
	}
	public static void ContrastLabelsWidget (bool bgDark, Gtk.Widget w)
	{
		contrastLabelsContainer (bgDark, (Gtk.Container) w);
	}

	private static void contrastLabelsContainer (bool bgDark, Gtk.Container container)
	{
		foreach(Gtk.Widget w in container.Children)
		{
			if(w.GetType() == typeof(Gtk.Label))
				ContrastLabelsLabel (bgDark, (Gtk.Label) w);

			else if(isContainer(w))
				contrastLabelsContainer (bgDark, (Gtk.Container) w);
		}
	}

	private static bool isContainer(Gtk.Widget w)
	{
		return ( w.GetType() == typeof(Gtk.Box) ||
				w.GetType() == typeof(Gtk.HBox) ||
				w.GetType() == typeof(Gtk.VBox) ||
				w.GetType() == typeof(Gtk.Grid) ||
				w.GetType() == typeof(Gtk.Table) ||
				w.GetType() == typeof(Gtk.Notebook) ||
				w.GetType() == typeof(Gtk.Frame) ||
				w.GetType() == typeof(Gtk.AspectFrame) ||
				(w.GetType() == typeof(Gtk.CheckButton) && ((Gtk.CheckButton) w).DrawIndicator) ||  //check contrast if there is a label inside but not a button with a label
				(w.GetType() == typeof(Gtk.RadioButton) && ((Gtk.RadioButton) w).DrawIndicator) || //same as above
				w.GetType() == typeof(Gtk.ScrolledWindow) ||
				w.GetType() == typeof(Gtk.Viewport) ||
				w.GetType() == typeof(Gtk.ButtonBox) ||
				w.GetType() == typeof(Gtk.VButtonBox) ||
				w.GetType() == typeof(Gtk.HButtonBox) ||
				w.GetType() == typeof(Gtk.Alignment) ||
				w.GetType() == typeof(Gtk.VPaned) ||
				w.GetType() == typeof(Gtk.HPaned) ||
				w.GetType() == typeof(Gtk.Expander)
			);
	}

	public static void ContrastLabelsLabel (bool bgDark, Gtk.Label l)
	{
		string str = "";

		if (l.Name.Contains ("big"))
			str += "big_";
		if (l.Name.Contains ("externalWindow"))
			str += "externalWindow_";
		if (l.Name.Contains ("monospace"))
			str += "monospace_";

		if(bgDark)
			str += "lightCss";
		else
			str += "darkCss";

		l.Name = str;
	}


	/*
	 *
	 * PRETTY THINGS
	 *
	 */

	/*
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
	*/
	
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


	public static string TextViewGetCommentValidSQL (Gtk.TextView tv)
	{
		//string s = Util.MakeValidSQL(textview_force_sensor_capture_comment.Buffer.Text);
		string s = Util.MakeValidSQL(tv.Buffer.Text);
		return s;
	}

	public static void TextViewClear (Gtk.TextView tv)
	{
		tv.Buffer.Text = "";
	}

	public static void TextViewScrollToEnd (Gtk.TextView tv)
	{
		//https://gtk-sharp-list.ximian.narkive.com/0nusInFU/gtk-textview-and-scrolling-text#post5
		TextIter ti = tv.Buffer.GetIterAtLine (tv.Buffer.LineCount-1);
		TextMark tm = tv.Buffer.CreateMark ("eot", ti, false);
		tv.ScrollToMark (tm, 0, false, 0, 0);
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

	public enum ArrowEnum { FORWARD, BACKWARD, FORWARD_EMPHASIS, LEFT, RIGHT }
	public static Button CreateArrowButton (int width, int height, ArrowEnum customArrow)
	{
		Button button = new Button ();

		if(width > 0)
			button.WidthRequest = width;
		if(height > 0)
			button.HeightRequest = height;

		Pixbuf pixbuf;
		if(customArrow == ArrowEnum.FORWARD)
			pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + Constants.FileNameArrowForward);
		else if(customArrow == ArrowEnum.BACKWARD)
			pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + Constants.FileNameArrowBackward);
		else if(customArrow == ArrowEnum.FORWARD_EMPHASIS)
			pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + Constants.FileNameArrowForwardEmphasis);
		else if(customArrow == ArrowEnum.LEFT)
			pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + Constants.FileNameLeft);
		else if(customArrow == ArrowEnum.RIGHT)
			pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + Constants.FileNameRight);
		else
			pixbuf = Chronojump.MyPixbuf.Get(null, Util.GetImagePath(false) + Constants.FileNameArrowForward); //default

		Gtk.Image image = new Gtk.Image();
		image.Pixbuf = pixbuf;
		button.Add(image);

		button.ShowAll();

		return button;
	}


	/*
	 *
	 * DRAWINGAREA
	 *
	 */

	//color string like "#0e1e46" for Chronojump blue
	public static RGBA ColorParse (string colorString)
	{
		RGBA color = new RGBA ();
		color.Parse (colorString);
		return color;
	}
	//reverse of previous method
	public static string ColorToHex (RGBA color)
	{
		var match = Regex.Matches (color.ToString (), @"\d+");
		StringBuilder hexaString = new StringBuilder("#");

		if (match.Count == 0)
			return "#0e1e46"; //default if there are problems

		//this discards the alpha (if exists)
		for(int i = 0; i <= 2; i++)
		{
			int value = Int32.Parse (match[i].Value);
			hexaString.Append (value.ToString ("X2")); //X2 to ensure a FF0000 is not written FF00
		}

		return hexaString.ToString();
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
	private static void imageFileWaitUntilCreated (string filename)
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

	private static Gdk.Pixbuf openPixbufSafe (string filename, Gdk.Pixbuf pixbuf)
	{
		imageFileWaitUntilCreated(filename);
		
		bool readedOk;
		
		/*
		 * An user had a hang with a repeated "File is still not ready. Wait a bit"
		 * for more than one minute. This happened after downloading news. Maybe is any disk buffer problem.
		 */
		int countTimes = 0;

		do {
			readedOk = true;
			try {
				pixbuf = Chronojump.MyPixbuf.Get(filename); //from a file

				//since dotnetgtk3 returns null if not ok
				if (pixbuf == null)
				{
					LogB.Warning("File is still not ready (null). Wait a bit");
					System.Threading.Thread.Sleep(50);
					readedOk = false;
					countTimes ++;
				}
			} catch {
				LogB.Warning("File is still not ready (catched). Wait a bit");
				System.Threading.Thread.Sleep(50);
				readedOk = false;
				countTimes ++;
			}
		} while( ! readedOk && countTimes < 25);

		if (countTimes >= 25)
			return Chronojump.MyPixbuf.Get (null, Util.GetImagePath(false) + "image.png"); //an icon representing an image

		return pixbuf;
	}

	public static Gdk.Pixbuf OpenPixbufSafe (string filename, Gdk.Pixbuf pixbuf)
	{
		return openPixbufSafe (filename, pixbuf);
	}

	public static Gtk.Image OpenImageSafe (string filename, Gtk.Image image)
	{
		Pixbuf pixbuf = Chronojump.MyPixbuf.Get (null, Util.GetImagePath(false) + "image.png");
		pixbuf = openPixbufSafe (filename, pixbuf);

		if (pixbuf != null)
			image.Pixbuf = pixbuf;

		return image;
	}

	/*
	//spacing allows the line to breath
	public static void DrawHorizontalLine(Pixmap pixmap, Gdk.GC pen, int xleft, int xright, int y,
			int spacing, bool arrowleft, bool arrowright, int arrowlength)
	{
		pixmap.DrawLine(pen, xleft + spacing, y, xright - spacing, y);

		//empty arrows
		if(arrowleft)
		{
			pixmap.DrawLine(pen,
					xleft + spacing, y,
					xleft + spacing + arrowlength, y - arrowlength);
			pixmap.DrawLine(pen,
					xleft + spacing, y,
					xleft + spacing + arrowlength, y + arrowlength);
		}
		if(arrowright)
		{
			pixmap.DrawLine(pen,
					xright - spacing, y,
					xright - spacing - arrowlength, y - arrowlength);
			pixmap.DrawLine(pen,
					xright - spacing, y,
					xright - spacing - arrowlength, y + arrowlength);
		}
	}



	// adapted from: https://stackoverflow.com/a/9295210/12366369
	// thanks to renosis and Komplot
	// tip is the point where the arrow will be drawn
	public static void DrawArrow (Pixmap pixmap, Gdk.GC pen, int tipX, int tailX, int tipY, int tailY, int arrowLength)
	{
		int dx = tipX - tailX;
		int dy = tipY - tailY;

		double theta = Math.Atan2(dy, dx);

		double rad = MathCJ.ToRadians(35); //35 angle, can be adjusted
		double x = tipX - arrowLength * Math.Cos(theta + rad);
		double y = tipY - arrowLength * Math.Sin(theta + rad);

		double phi2 = MathCJ.ToRadians(-35);//-35 angle, can be adjusted
		double x2 = tipX - arrowLength * Math.Cos(theta + phi2);
		double y2 = tipY - arrowLength * Math.Sin(theta + phi2);

		Point [] points = new Point[3];
		points[0].X = (int) x;
		points[0].Y = (int) y;
		points[1].X = tipX;
		points[1].Y = tipY;
		points[2].X = (int) x2;
		points[2].Y = (int) y2;

		pixmap.DrawPolygon(pen, true, points);
	}
	*/

	public static void FindPangoFonts(Gtk.DrawingArea drawingarea)
	{
		LogB.Information("Available Pango fonts:");
		foreach(Pango.FontFamily ff in drawingarea.PangoContext.Families)
		{
			LogB.Information(ff.Name);
			foreach(Pango.FontFace faces in ff.Faces)
				LogB.Information("- " + faces.FaceName);
		}
	}

	// avoids problems like converting a 1.8 into 1.8000000000000003 when doing:
	// Convert.ToDouble(spin_force_sensor_analyze_ab_slider_increment.Value)
	public static double GetSpinButtonDouble (Gtk.SpinButton spin, int decimals)
	{
		return Math.Round (Convert.ToDouble (spin.Value), decimals);
	}
}
