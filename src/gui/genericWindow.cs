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
 * Copyright (C) 2004-2016   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gdk;
using Gtk;
using Glade;
//using Gnome;
using GLib; //for Value
//using System.Text; //StringBuilder
using System.Collections; //ArrayList
using Mono.Unix;


public class GenericWindow
{
	[Widget] Gtk.Window generic_window;
	[Widget] Gtk.Label label_header;
	[Widget] Gtk.Box hbox_error;
	[Widget] Gtk.Label label_error;
	[Widget] Gtk.Entry entry;
	
	[Widget] Gtk.Box hbox_spin_int;
	[Widget] Gtk.Label label_spin_int;
	[Widget] Gtk.SpinButton spin_int;
	[Widget] Gtk.Box hbox_spin_int2;
	[Widget] Gtk.Label label_spin_int2;
	[Widget] Gtk.SpinButton spin_int2;
	[Widget] Gtk.Box hbox_spin_int3;
	[Widget] Gtk.Label label_spin_int3;
	[Widget] Gtk.SpinButton spin_int3;

	[Widget] Gtk.SpinButton spin_double;
	[Widget] Gtk.Box hbox_height_metric;
	
	[Widget] Gtk.HButtonBox hbuttonbox_middle;
	[Widget] Gtk.Button button_middle;

	//Edit row
	[Widget] Gtk.Box hbox_edit_row;
	[Widget] Gtk.Label hbox_combo_label;
	[Widget] Gtk.Box hbox_combo;
	[Widget] Gtk.ComboBox combo;
	[Widget] Gtk.Button hbox_combo_button;
	[Widget] Gtk.Entry entry_edit_row;
	
	[Widget] Gtk.Box hbox_all_none_selected;
	[Widget] Gtk.Box hbox_combo_all_none_selected;
	[Widget] Gtk.ComboBox combo_all_none_selected;
	[Widget] Gtk.SpinButton spin_feet;
	[Widget] Gtk.SpinButton spin_inches;
	[Widget] Gtk.ScrolledWindow scrolled_window_textview;
	[Widget] Gtk.TextView textview;
	[Widget] Gtk.ScrolledWindow scrolled_window_treeview;
	[Widget] Gtk.TreeView treeview;
	[Widget] Gtk.Button button_accept;
	[Widget] Gtk.Button button_cancel;
	[Widget] Gtk.Button button_delete; //currently only on encoder exercise edit
	
	//treeview fake buttons
	[Widget] Gtk.Button button_row_edit;
	[Widget] Gtk.Button button_row_delete;
	
	[Widget] Gtk.Box hbox_entry2;
	[Widget] Gtk.Label label_entry2;
	[Widget] Gtk.Entry entry2;
	[Widget] Gtk.Box hbox_entry3;
	[Widget] Gtk.Label label_entry3;
	[Widget] Gtk.Entry entry3;

	[Widget] Gtk.Box hbox_spin_double2;
	[Widget] Gtk.Label label_spin_double2;
	[Widget] Gtk.SpinButton spin_double2;
	
	[Widget] Gtk.Image image_delete;
	
	private ArrayList nonSensitiveRows;

	static GenericWindow GenericWindowBox;
	
	private TreeStore store;
	private Constants.ContextMenu genericWinContextMenu;
	
	//used to read data, see if it's ok, and print an error message.
	//if all is ok, destroy it with HideAndNull()
	public bool HideOnAccept;
	
	//used when we don't need to read data, 
	//and we want to ensure next window will be created at needed size
	public bool DestroyOnAccept;
	public int TreeviewSelectedUniqueID;
	private int commentColumn;

	//used on encoder edit exercise
	public int uniqueID;
	public string nameUntranslated;

	public enum Types { UNDEFINED, ENCODER_SESSION_LOAD, 
		ENCODER_SEL_REPS_IND_CURRENT_SESS, ENCODER_SEL_REPS_IND_ALL_SESS, ENCODER_SEL_REPS_GROUP_CURRENT_SESS };
	//used to decide if a genericWin has to be recreated
	public Types Type;


	public GenericWindow (string textHeader)
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "generic_window.glade", "generic_window", "chronojump");
		gladeXML.Autoconnect(this);
	
		//put an icon to window
		UtilGtk.IconWindow(generic_window);
		
		hideWidgets();
		generic_window.Resizable = false;
		label_header.Text = textHeader;
		
		HideOnAccept = true;
		DestroyOnAccept = false;
	}

	//for some widgets
	static public GenericWindow Show (bool showNow, string textHeader, ArrayList array)
	{
		if (GenericWindowBox == null) {
			GenericWindowBox = new GenericWindow(textHeader);
		}

		foreach(ArrayList widgetArray in array)
			GenericWindowBox.showWidgetsPowerful(widgetArray);

		Pixbuf pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_delete.png");
		GenericWindowBox.image_delete.Pixbuf = pixbuf;

		GenericWindowBox.Type = Types.UNDEFINED;
		
		GenericWindowBox.label_header.Text = textHeader;
		
		if(showNow)
			GenericWindowBox.generic_window.Show ();
		
		return GenericWindowBox;
	}
			
	//for only one widget
	static public GenericWindow Show (string textHeader, Constants.GenericWindowShow stuff)
	{
		if (GenericWindowBox == null) {
			GenericWindowBox = new GenericWindow(textHeader);
		}
		
		GenericWindowBox.Type = Types.UNDEFINED;

		GenericWindowBox.showWidget(stuff);
		GenericWindowBox.generic_window.Show ();
		
		return GenericWindowBox;
	}
	
	public void ShowNow() {
		if(GenericWindowBox == null) {
			LogB.Error("at showNow GenericWindowBox is null!!");
		//	GenericWindowBox = new GenericWindow("");
		}

		GenericWindowBox.generic_window.Show ();
	}
	
	
	void hideWidgets() {
		hbox_error.Hide();
		entry.Hide();
		hbox_entry2.Hide();
		hbox_entry3.Hide();
		hbox_spin_int.Hide();
		hbox_spin_int2.Hide();
		hbox_spin_int3.Hide();
		spin_double.Hide();
		hbox_spin_double2.Hide();
		hbox_height_metric.Hide();
		hbox_edit_row.Hide();
		hbox_all_none_selected.Hide();
		hbox_combo_all_none_selected.Hide();
		hbuttonbox_middle.Hide();
		scrolled_window_textview.Hide();
		scrolled_window_treeview.Hide();
	}
	
	void showWidgetsPowerful (ArrayList widgetArray) {
		Constants.GenericWindowShow stuff = (Constants.GenericWindowShow) widgetArray[0];
		bool editable = (bool) widgetArray[1];
		string text = (string) widgetArray[2];
		
		if(stuff == Constants.GenericWindowShow.ENTRY) {
			entry.Show();
			entry.IsEditable = editable;
			entry.Sensitive = editable;
			entry.Text = text;
		}
		else if(stuff == Constants.GenericWindowShow.ENTRY2) {
			hbox_entry2.Show();
			entry2.IsEditable = editable;
			entry2.Sensitive = editable;
			entry2.Text = text;
		}
		else if(stuff == Constants.GenericWindowShow.ENTRY3) {
			hbox_entry3.Show();
			entry3.IsEditable = editable;
			entry3.Sensitive = editable;
			entry3.Text = text;
		}
		else if(stuff == Constants.GenericWindowShow.SPININT) {
			hbox_spin_int.Show();
			spin_int.IsEditable = editable;
			spin_int.Sensitive = editable;
		}
		else if(stuff == Constants.GenericWindowShow.SPINDOUBLE) {
			spin_double.Show();
			spin_double.IsEditable = editable;
		}
		else if(stuff == Constants.GenericWindowShow.HBOXSPINDOUBLE2) {
			hbox_spin_double2.Show();
			spin_double2.IsEditable = editable;
			spin_double2.Sensitive = editable;
		}
		else if(stuff == Constants.GenericWindowShow.SPININT2) {
			hbox_spin_int2.Show();
			spin_int2.IsEditable = editable;
		}
		else if(stuff == Constants.GenericWindowShow.SPININT3) {
			hbox_spin_int3.Show();
			spin_int3.IsEditable = editable;
		}
		else if(stuff == Constants.GenericWindowShow.HEIGHTMETRIC) {
			hbox_height_metric.Show();
		}
		else if(stuff == Constants.GenericWindowShow.COMBO) {
			/*
			hbox_combo.Show();
			combo.Show();
			*/
		}
		else if(stuff == Constants.GenericWindowShow.COMBOALLNONESELECTED) {
			//createComboCheckBoxes();
			//combo_all_none_selected.Active = 
			//	UtilGtk.ComboMakeActive(comboCheckBoxesOptions, Catalog.GetString("Selected"));
			hbox_combo_all_none_selected.Show();
			hbox_all_none_selected.Show();
		}
		else if(stuff == Constants.GenericWindowShow.BUTTONMIDDLE) {
			hbuttonbox_middle.Show();
		}
		else if(stuff == Constants.GenericWindowShow.TEXTVIEW) {
			scrolled_window_textview.Show();
		}
		else { //if(stuff == Constants.GenericWindowShow.TREEVIEW)
			scrolled_window_treeview.Show();
		}
		
	}

	void showWidget(Constants.GenericWindowShow stuff) {
		if(stuff == Constants.GenericWindowShow.ENTRY)
			entry.Show();
		else if(stuff == Constants.GenericWindowShow.ENTRY2)
			hbox_entry2.Show();
		else if(stuff == Constants.GenericWindowShow.ENTRY3)
			hbox_entry3.Show();
		else if(stuff == Constants.GenericWindowShow.SPININT)
			hbox_spin_int.Show();
		else if(stuff == Constants.GenericWindowShow.SPINDOUBLE)
			spin_double.Show();
		else if(stuff == Constants.GenericWindowShow.HBOXSPINDOUBLE2)
			hbox_spin_double2.Show();
		else if(stuff == Constants.GenericWindowShow.HEIGHTMETRIC)
			hbox_height_metric.Show();
		else if(stuff == Constants.GenericWindowShow.SPININT2)
			hbox_spin_int2.Show();
		else if(stuff == Constants.GenericWindowShow.SPININT3)
			hbox_spin_int3.Show();
		else if(stuff == Constants.GenericWindowShow.COMBO) {
			//do later, we need to create them first
			/*
			hbox_combo.Show();
			combo.Show();
			*/
		}
		else if(stuff == Constants.GenericWindowShow.BUTTONMIDDLE)
			hbuttonbox_middle.Show();
		else if(stuff == Constants.GenericWindowShow.TEXTVIEW)
			scrolled_window_textview.Show();
		else //if(stuff == Constants.GenericWindowShow.TREEVIEW)
			scrolled_window_treeview.Show();
	}

	public void SetSize(int width, int height) {
		generic_window.SetDefaultSize(width, height);
	}
	
	public void SetTreeviewSize(int width, int height) {
		treeview.SetSizeRequest(width, height);
	}


	public void SetLabelError(string text) {
		label_error.Text = text;
		hbox_error.Show();
	}
	
	public void SetSpinValue(int num) {
		spin_int.Value = num;
	}
	public void SetSpinRange(int min, int max) {
		spin_int.SetRange(min, max);
	}
	public void SetSpinRange(double min, double max) {
		spin_int.SetRange(min, max);
	}
	
	public void SetSpin2Value(int num) {
		spin_int2.Value = num;
	}
	public void SetSpin2Range(int min, int max) {
		spin_int2.SetRange(min, max);
	}
	
	public void SetSpin3Value(int num) {
		spin_int3.Value = num;
	}
	public void SetSpin3Range(int min, int max) {
		spin_int3.SetRange(min, max);
	}
	
	public void SetSpinDouble2Value(double num) {
		spin_double2.Value = num;
	}
	public void SetSpinDouble2Increments(double min, double max) {
		spin_double2.SetIncrements(min, max);
	}
	public void SetSpinDouble2Range(double min, double max) {
		spin_double2.SetRange(min, max);
	}
	
	public void SetComboValues(string [] values, string current) {
		combo = ComboBox.NewText ();
		UtilGtk.ComboUpdate(combo, values, "");
		
		hbox_combo.PackStart(combo, true, true, 0);
		hbox_combo.ShowAll();
		combo.Sensitive = true;
			
		combo.Active = UtilGtk.ComboMakeActive(values, current);
	}
	public void SetComboLabel(string l) {
		hbox_combo_label.Text = l;
	}
	public void ShowEditRow(bool show) {
		hbox_edit_row.Visible = show;
	}

	
	private static string [] comboCheckBoxesOptionsDefault = {
		Catalog.GetString("All"),
		Catalog.GetString("None"),
		Catalog.GetString("Invert"),
		Catalog.GetString("Selected"),
	};
	private string [] comboCheckBoxesOptions = comboCheckBoxesOptionsDefault;

	public void ResetComboCheckBoxesOptions() {
		comboCheckBoxesOptions = comboCheckBoxesOptionsDefault;
	}
	
	//this search in first column
	public void AddOptionsToComboCheckBoxesOptions(ArrayList newOptions) {
		comboCheckBoxesOptions = Util.AddArrayString( comboCheckBoxesOptions, 
				Util.ArrayListToString(newOptions) );
	}

	public void CreateComboCheckBoxes() 
	{
		if(hbox_combo_all_none_selected.Children.Length > 0)
			hbox_combo_all_none_selected.Remove(combo_all_none_selected);

		combo_all_none_selected = ComboBox.NewText ();
		UtilGtk.ComboUpdate(combo_all_none_selected, comboCheckBoxesOptions, "");
		
		//combo_all_none_selected.DisableActivate ();
		combo_all_none_selected.Changed += new EventHandler (on_combo_all_none_selected_changed);

		hbox_combo_all_none_selected.PackStart(combo_all_none_selected, true, true, 0);
		hbox_combo_all_none_selected.ShowAll();
		combo_all_none_selected.Sensitive = true;
			
		combo_all_none_selected.Active = 
			UtilGtk.ComboMakeActive(comboCheckBoxesOptions, Catalog.GetString("Selected"));
	}
	
	private void on_combo_all_none_selected_changed(object o, EventArgs args) {
		string myText = UtilGtk.ComboGetActive(combo_all_none_selected);
			
		if (myText != "" & myText != Catalog.GetString("Selected")) {
			try {
				markSelected(myText);
			} catch {
				LogB.Warning("Do later!!");
			}
		}
	}
	
	private void markSelected(string selected) {
		Gtk.TreeIter iter;
		bool okIter = store.GetIterFirst(out iter);
		if(okIter) {
			int i=0;
			if(selected == Catalog.GetString("All")) {
				do {
					if(! Util.FoundInArrayList(nonSensitiveRows, i))
						store.SetValue (iter, 1, true);
					i++;
				} while ( store.IterNext(ref iter) );
			} else if(selected == Catalog.GetString("Invert")) {
				bool val;
				do {
					if(! Util.FoundInArrayList(nonSensitiveRows, i)) {
						val = (bool) store.GetValue (iter, 1);
						store.SetValue (iter, 1, !val);
					}
					i++;
				} while ( store.IterNext(ref iter) );
			} else if(selected == Catalog.GetString("None")) {
				do {
					store.SetValue (iter, 1, false);
				} while ( store.IterNext(ref iter) );
			} else {	//encoderExercises
				do {
					if(selected == (string) store.GetValue (iter, 3) &&
							! Util.FoundInArrayList(nonSensitiveRows, i))
						store.SetValue (iter, 1, true);
					else
						store.SetValue (iter, 1, false);
					i++;
				} while ( store.IterNext(ref iter) );
			}
		}
			
		//check if there are rows checked for having sensitive or not in recuperate button
		//buttonRecuperateChangeSensitiveness();
	}
	
	public void SetButtonMiddleLabel(string str) {
		button_middle.Label=str;
	}
	
	public void SetTextview(string str) {
		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.Text = str;
		textview.Buffer = tb;
	}
	
	bool activateRowAcceptsWindow;
	//data is an ArrayList of strings[], each string [] is a row, each of its strings is a column
	public void SetTreeview(string [] columnsString, bool addCheckbox, 
			ArrayList data, ArrayList myNonSensitiveRows, Constants.ContextMenu contextMenu,
			bool activateRowAcceptsWindow	//this param makes button_accept the window if 'enter' on a row or double click
			) 
	{
		//adjust window to be bigger
		generic_window.Resizable = true;
		scrolled_window_treeview.WidthRequest = 550;
		scrolled_window_treeview.HeightRequest = 250;

		store = getStore(columnsString.Length, addCheckbox); 
		treeview.Model = store;
		prepareHeaders(columnsString, addCheckbox);
		treeview.HeadersClickable = false;

		nonSensitiveRows = myNonSensitiveRows;
	
		LogB.Debug("aaaaaaaaaaaaaaaa1");	
		foreach (string [] line in data) {
			store.AppendValues (line);
			//Log.WriteLine(Util.StringArrayToString(line,"\n"));
		}
		LogB.Debug("aaaaaaaaaaaaaaaa2");	

		genericWinContextMenu = contextMenu;
		this.activateRowAcceptsWindow = activateRowAcceptsWindow;

		treeview.CursorChanged += on_treeview_cursor_changed; 
		if(contextMenu == Constants.ContextMenu.EDITDELETE) {
			button_row_edit = new Gtk.Button();
			button_row_delete = new Gtk.Button();
			treeview.ButtonReleaseEvent -= on_treeview_button_release_event;
			treeview.ButtonReleaseEvent += on_treeview_button_release_event;
		} else if(contextMenu == Constants.ContextMenu.DELETE) {
			button_row_delete = new Gtk.Button();
			treeview.ButtonReleaseEvent -= on_treeview_button_release_event;
			treeview.ButtonReleaseEvent += on_treeview_button_release_event;
		}
	}

	public void SelectRowWithID(int colNum, int id) 
	{
		if(id <= 0)
			return;

		UtilGtk.TreeviewSelectRowWithID(treeview, store, colNum, id, true); //last boolean is 'scroll to row'
	}
	
	public void MarkActiveCurves(string [] checkboxes) 
	{
		int count = 0;
		Gtk.TreeIter iter;
		bool okIter = store.GetIterFirst(out iter);
		if(okIter) {
			do {
				if(checkboxes[count++] == "active")
					store.SetValue (iter, 1, true);
			} while ( store.IterNext(ref iter) );
		}
	}
	
	private TreeStore getStore (int columns, bool addCheckbox)
	{
		//prepares the TreeStore for required columns
		Type [] types = new Type [columns];

		for (int i=0; i < columns; i++) {
			if(addCheckbox && i == 1)
				types[1] = typeof (bool);
			else
				types[i] = typeof (string);
		}
		TreeStore myStore = new TreeStore(types);
		return myStore;
	}
	
	private void prepareHeaders(string [] columnsString, bool addCheckbox) 
	{
		treeviewRemoveColumns();
		treeview.HeadersVisible=true;
		int i=0;
		bool visible = false;
		foreach(string myCol in columnsString) {
			if(addCheckbox && i == 1)
				createCheckboxes(treeview);
			else
				UtilGtk.CreateCols(treeview, store, myCol, i, visible);
//			if(i == 1)	//first columns: ID, is hidden
//				store.SetSortFunc (0, UtilGtk.IdColumnCompare);
			visible = true;
			i++;
		}
	}
	
	private void treeviewRemoveColumns() {
		Gtk.TreeViewColumn [] myColumns = treeview.Columns;
		foreach (Gtk.TreeViewColumn column in myColumns) {
			treeview.RemoveColumn (column);
		}
	}

	private void on_treeview_cursor_changed (object o, EventArgs args) 
	{
		TreeIter iter = new TreeIter();
		TreeModel myModel = treeview.Model;
		if (treeview.Selection.GetSelected (out myModel, out iter)) 
			SetButtonAcceptSensitive(true);
		else
			SetButtonAcceptSensitive(false);

		ShowEditRow(false);
	}

	//get the selected	
	public int TreeviewSelectedRowID() {
		TreeIter iter = new TreeIter();
		TreeModel myModel = treeview.Model;
		if (treeview.Selection.GetSelected (out myModel, out iter)) 
			return Convert.ToInt32(treeview.Model.GetValue (iter, 0));
		else
			return 0;
	}

	private void createCheckboxes(TreeView tv) 
	{
		CellRendererToggle crt = new CellRendererToggle();
		crt.Visible = true;
		crt.Activatable = true;
		crt.Active = true;
		crt.Toggled += ItemToggled;

		TreeViewColumn column = new TreeViewColumn ("", crt, "active", 1);
		column.Clickable = true;
		tv.AppendColumn (column);
		
	}
	
	public int GetCell(int uniqueID, int column) 
	{
		//LogB.Information(" GetCell " + uniqueID.ToString() + " " + column.ToString());
		Gtk.TreeIter iter;
		bool okIter = store.GetIterFirst(out iter);
		if(okIter) {
			do {
				//LogB.Information("_0_ " + (string) store.GetValue (iter, 0));
				//LogB.Information("_column_ " + column.ToString() + " " + (string) store.GetValue (iter, column));
				if( ((string) store.GetValue (iter, 0)) == uniqueID.ToString()) {
					return Convert.ToInt32( (string) store.GetValue (iter, column) );
				}
			} while ( store.IterNext(ref iter) );
		}

		return 0;
	}
	public bool GetCheckboxStatus(int uniqueID) {
		Gtk.TreeIter iter;
		bool okIter = store.GetIterFirst(out iter);
		if(okIter) {
			do {
				if( ((string) store.GetValue (iter, 0)) == uniqueID.ToString())
					return (bool) store.GetValue (iter, 1);
			} while ( store.IterNext(ref iter) );
		}
		//if error, return false
		return false;
	}
	//if column == 1 returns checkboxes column. If is 2 returns column 2...
	//Attention: Used on checkboxes treeviews
	public string [] GetColumn(int column, bool onlyActive) 
	{
		//to store active or inactive status of curves
		string [] checkboxes = new string[UtilGtk.CountRows(store)];
		
		int count = 0;
		Gtk.TreeIter iter;
		bool okIter = store.GetIterFirst(out iter);
		if(okIter) {
			do {
				if(column == 1) {
					if((bool) store.GetValue (iter, 1))
						checkboxes[count++] = "active";
					else
						checkboxes[count++] = "inactive";
				}
				else
					if((bool) store.GetValue (iter, 1) || ! onlyActive)
						checkboxes[count++] = ((string) store.GetValue (iter, column));
				
			} while ( store.IterNext(ref iter) );
		}
		if(column == 1)
			return checkboxes;
		else {
			string [] checkboxesWithoutGaps = new string[count];
			for(int i=0; i < count; i ++)
				checkboxesWithoutGaps[i] = checkboxes[i];
			return checkboxesWithoutGaps;
		}
	}

	private void ItemToggled(object o, ToggledArgs args) {
		int column = 1;
		TreeIter iter;
		if (store.GetIter (out iter, new TreePath(args.Path))) 
		{
			//Log.WriteLine(args.Path);
			if(! Util.FoundInArrayList(nonSensitiveRows, 
						Convert.ToInt32(args.Path))) {
				bool val = (bool) store.GetValue (iter, column);
				//Log.WriteLine (string.Format("toggled {0} with value {1}", args.Path, !val));

				store.SetValue (iter, column, !val);

				combo_all_none_selected.Active =
					UtilGtk.ComboMakeActive(
							comboCheckBoxesOptions, Catalog.GetString("Selected"));

				//check if there are rows checked for having sensitive or not
				//buttonRecuperateChangeSensitiveness();
				
				hbox_error.Hide();
			} else {
				label_error.Text = "Cannot select rows without data";
				hbox_error.Show();
			}
		}
	}
	
	void on_row_double_clicked (object o, Gtk.RowActivatedArgs args)
	{
		if(activateRowAcceptsWindow) {
			TreeView tv = (TreeView) o;
			TreeModel model;
			TreeIter iter;

			if (tv.Selection.GetSelected (out model, out iter)) {
				//activate on_button_accept_clicked()
				button_accept.Activate();
			}
		}
	}
	

	private void on_treeview_button_release_event (object o, ButtonReleaseEventArgs args) {
		//TreeviewSelectedUniqueID = -1;

                Gdk.EventButton e = args.Event;
                Gtk.TreeView tv = (Gtk.TreeView) o;
		TreeModel model = treeview.Model;
		if (e.Button == 3) {
			TreeIter iter = new TreeIter();
			if (tv.Selection.GetSelected (out model, out iter)) {
				TreeviewSelectedUniqueID = Convert.ToInt32((string) store.GetValue (iter, 0));
				treeviewContextMenu();
			}
		}
		ShowEditRow(false);
	}

	Menu menuCtx;
	private void treeviewContextMenu() {
		menuCtx = new Menu ();
		Gtk.MenuItem myItem;

		if(genericWinContextMenu == Constants.ContextMenu.EDITDELETE) {
			myItem = new MenuItem ( Catalog.GetString("Edit selected") );
			myItem.Activated += on_edit_selected_clicked;
			menuCtx.Attach( myItem, 0, 1, 0, 1 );

			myItem = new MenuItem ( Catalog.GetString("Delete selected") );
			myItem.Activated += on_delete_selected_clicked;
			menuCtx.Attach( myItem, 0, 1, 1, 2 );
		}
		else if(genericWinContextMenu == Constants.ContextMenu.DELETE) {
			myItem = new MenuItem ( Catalog.GetString("Delete selected") );
			myItem.Activated += on_delete_selected_clicked;
			menuCtx.Attach( myItem, 0, 1, 0, 1 );
		} else {
			//don't show nothing if there are no options
			menuCtx.Popdown();
			return; 
		}

		menuCtx.Popup();
		menuCtx.ShowAll();
	}

	private void on_edit_selected_clicked (object o, EventArgs args) 
	{
		TreeModel model;
		TreeIter iter = new TreeIter();
		treeview.Selection.GetSelected (out model, out iter);
		entry_edit_row.Text = (string) model.GetValue (iter, commentColumn);

		button_row_edit.Click();
	}
	
	public void on_edit_selected_done_update_treeview() 
	{
		//remove conflictive characters
		entry_edit_row.Text = Util.RemoveTildeAndColonAndDot(entry_edit_row.Text);

		TreeModel model;
		TreeIter iter = new TreeIter();
		treeview.Selection.GetSelected (out model, out iter);
		store.SetValue (iter, commentColumn, entry_edit_row.Text);
	}

	//this method is only used when try to delete an encoder exercise,
	//and cannot because there are encoder rows done with this exercise.
	//Just unsensitive some stuff now in order to not be able to change them
	public void DeletingExerciseHideSomeWidgets() {
		hbox_spin_int.Hide();
		hbox_entry2.Hide();
		hbox_entry3.Hide();
		hbox_spin_double2.Hide();

		SetButtonAcceptLabel(Catalog.GetString("Close"));
	}	

	public void RemoveSelectedRow () {
		store = UtilGtk.RemoveRow(treeview, store);
	}

	private void on_delete_selected_clicked (object o, EventArgs args) {
		//activate button to manage on gui/encoder.cs in order to delete from SQL
		button_row_delete.Click();
	}

	//if confirmed deletion, this will be called
	public void Delete_row_accepted() {
		//remove selected row from treeview
		store = UtilGtk.RemoveRow(treeview, store);
		menuCtx.Popdown();
	}
	
	public void Row_add(string [] row) {
		//add row to treeview
		UtilGtk.TreeviewAddRow(treeview, store, row, true); //insert at beginning
	}
	
	
	public void ShowTextview() {
		scrolled_window_textview.Show();
	}

	public void ShowTreeview() {
		scrolled_window_treeview.Show();
	}

	public void ShowButtonDelete(bool show) {
		button_delete.Visible = show;
	}

	public void SetButtonAcceptLabel(string str) {
		button_accept.Label=str;
	}
	
	public void SetButtonAcceptSensitive(bool show) {
		button_accept.Sensitive=show;
	}
	
	public void SetButtonCancelLabel(string str) {
		button_cancel.Label=str;
	}
	
	public void ShowButtonCancel(bool show) {
		button_cancel.Visible = show;
	}

	private void on_button_cancel_clicked (object o, EventArgs args)
	{
		GenericWindowBox.generic_window.Hide();
		GenericWindowBox = null;
	}

	private void on_delete_event (object o, DeleteEventArgs args)
	{
		LogB.Information("calling on_delete_event");

		args.RetVal = true;
			
		GenericWindowBox.generic_window.Hide();
		GenericWindowBox = null;
	}

	private void on_button_accept_clicked (object o, EventArgs args)
	{
		LogB.Information("called on_button_accept_clicked");

		if(HideOnAccept)
			GenericWindowBox.generic_window.Hide();
		if(DestroyOnAccept)
			GenericWindowBox = null;
	}
	
	//when ! HideOnAccept, use this to close window
	//also is better to call it always tat is closed clicking on accept (after data has been readed)
	public void HideAndNull() {
		GenericWindowBox.generic_window.Hide();
		GenericWindowBox = null;
	}
	
	public bool GenericWindowBoxIsNull() {
		if(GenericWindowBox == null)
			return true;
		return false;
	}
	
	public Button Button_middle {
		get { return button_middle; }
	}
	
	public Button Button_delete {
		set { button_delete = value; }
		get { return button_delete; }
	}
		

	public Button Button_accept {
		set { button_accept = value; }
		get { return button_accept; }
	}
		
	public Button Button_row_edit {
		set { button_row_edit = value; }
		get { return button_row_edit; }
	}
	
	public Button Button_row_edit_apply {
		set { hbox_combo_button = value; }
		get { return hbox_combo_button; }
	}
	
	public int CommentColumn {
		set { commentColumn = value; }
	}
		
	public Button Button_row_delete {
		set { button_row_delete = value; }
		get { return button_row_delete; }
	}
	
	public string EntrySelected {
		set { entry.Text = value; }
		get { return entry.Text.ToString(); }
	}
	
	public string LabelEntry2 {
		set { label_entry2.Text = value; }
	}
	public string Entry2Selected {
		set { entry2.Text = value; }
		get { return entry2.Text.ToString(); }
	}
	
	public string LabelEntry3 {
		set { label_entry3.Text = value; }
	}
	public string Entry3Selected {
		set { entry3.Text = value; }
		get { return entry3.Text.ToString(); }
	}

	public string EntryEditRow {
		get { return entry_edit_row.Text.ToString(); }
	}

	public string LabelSpinInt {
		set { label_spin_int.Text = value; }
	}
	public int SpinIntSelected {
		get { return (int) spin_int.Value; }
	}
	
	public string LabelSpinInt2 {
		set { label_spin_int2.Text = value; }
	}
	public int SpinInt2Selected {
		get { return (int) spin_int2.Value; }
	}
	
	public string LabelSpinInt3 {
		set { label_spin_int3.Text = value; }
	}
	public int SpinInt3Selected {
		get { return (int) spin_int3.Value; }
	}
	
	public double SpinDoubleSelected {
		get { return (double) spin_double.Value; }
	}

	public string LabelSpinDouble2 {
		set { label_spin_double2.Text = value; }
	}
	public double SpinDouble2Selected {
		get { return (double) spin_double2.Value; }
	}
	
	public string TwoSpinSelected {
		get { return ((int) spin_feet.Value).ToString() + ":" + ((int) spin_inches.Value).ToString(); }
	}
	
	public string TextviewSelected {
		get { return Util.RemoveTab(textview.Buffer.Text); }
	}

	public string GetComboSelected {
		get { return UtilGtk.ComboGetActive(combo); }
	}


	~GenericWindow() {}
	
}

