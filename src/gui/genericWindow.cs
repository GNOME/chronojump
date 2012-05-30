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
 * Copyright (C) 2004-2012   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Glade;
//using Gnome;
using GLib; //for Value
//using System.Text; //StringBuilder
using System.Collections; //ArrayList


public class GenericWindow
{
	[Widget] Gtk.Window generic_window;
	[Widget] Gtk.Label label_header;
	[Widget] Gtk.Label label_generic_name;
	[Widget] Gtk.Entry entry;
	[Widget] Gtk.SpinButton spin_int;
	[Widget] Gtk.SpinButton spin_double;
	[Widget] Gtk.Box hbox_height_metric;
	[Widget] Gtk.SpinButton spin_feet;
	[Widget] Gtk.SpinButton spin_inches;
	[Widget] Gtk.ScrolledWindow scrolled_window_textview;
	[Widget] Gtk.TextView textview;
	[Widget] Gtk.ScrolledWindow scrolled_window_treeview;
	[Widget] Gtk.TreeView treeview;
	[Widget] Gtk.Button button_accept;

	static GenericWindow GenericWindowBox;
	
	private TreeStore store;
	
	public GenericWindow ()
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "generic_window", null);
		gladeXML.Autoconnect(this);
		
		//put an icon to window
		UtilGtk.IconWindow(generic_window);
	}

	static public GenericWindow Show (string textHeader, Constants.GenericWindowShow stuff)
	{
		if (GenericWindowBox == null) {
			GenericWindowBox = new GenericWindow();
		}
		GenericWindowBox.showHideWidgets(stuff);
		GenericWindowBox.label_header.Text = textHeader;

		GenericWindowBox.generic_window.Show ();
		
		return GenericWindowBox;
	}
	
	void showHideWidgets(Constants.GenericWindowShow stuff) {
		entry.Hide();
		spin_int.Hide();
		spin_double.Hide();
		hbox_height_metric.Hide();
		scrolled_window_textview.Hide();
		scrolled_window_treeview.Hide();

		if(stuff == Constants.GenericWindowShow.ENTRY)
			entry.Show();
		else if(stuff == Constants.GenericWindowShow.SPININT)
			spin_int.Show();
		else if(stuff == Constants.GenericWindowShow.SPINDOUBLE)
			spin_double.Show();
		else if(stuff == Constants.GenericWindowShow.HEIGHTMETRIC)
			hbox_height_metric.Show();
		else if(stuff == Constants.GenericWindowShow.TEXTVIEW)
			scrolled_window_textview.Show();
		else //if(stuff == Constants.GenericWindowShow.TREEVIEW)
			scrolled_window_treeview.Show();
	}
	
	public void SetSpinRange(double min, double max) {
		spin_int.SetRange(min, max);
	}
	
	public void SetTextview(string str) {
		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.Text = str;
		textview.Buffer = tb;
	}
	
	//data is an ArrayList of strings[], each string [] is a row, each of its strings is a column
	public void SetTreeview(string [] columnsString, ArrayList data) 
	{
		//adjust window to be bigger
		generic_window.Resizable = true;
		scrolled_window_treeview.WidthRequest = 500;
		scrolled_window_treeview.HeightRequest = 250;

		store = getStore(columnsString.Length); 
		treeview.Model = store;
		prepareHeaders(columnsString);
		
		foreach (string [] line in data) 
			store.AppendValues (line);
		
		setButtonAcceptSensitive(false);
		treeview.CursorChanged += on_treeview_cursor_changed; 
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
	
	private void prepareHeaders(string [] columnsString) 
	{
		treeviewRemoveColumns();
		treeview.HeadersVisible=true;
		int i=0;
		bool visible = false;
		foreach(string myCol in columnsString) {
			UtilGtk.CreateCols(treeview, store, myCol, i++, visible);
			if(i == 1)	//first columns: ID, is hidden
				store.SetSortFunc (0, UtilGtk.IdColumnCompare);
			visible = true;
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
			setButtonAcceptSensitive(true);
		else
			setButtonAcceptSensitive(false);
	}
	
	public int TreeviewSelectedRowID() {
		TreeIter iter = new TreeIter();
		TreeModel myModel = treeview.Model;
		if (treeview.Selection.GetSelected (out myModel, out iter)) 
			return Convert.ToInt32(treeview.Model.GetValue (iter, 0));
		else
			return 0;
	}
	
	public void SetButtonAcceptLabel(string str) {
		button_accept.Label=str;
	}
	
	private void setButtonAcceptSensitive(bool show) {
		button_accept.Sensitive=show;
	}


	protected void on_button_cancel_clicked (object o, EventArgs args)
	{
		GenericWindowBox.generic_window.Hide();
		GenericWindowBox = null;
	}
	
	protected void on_delete_event (object o, DeleteEventArgs args)
	{
		GenericWindowBox.generic_window.Hide();
		GenericWindowBox = null;
	}

	protected void on_button_accept_clicked (object o, EventArgs args)
	{
		GenericWindowBox.generic_window.Hide();
		//GenericWindowBox = null;
	}
	
	public Button Button_accept {
		set { button_accept = value; }
		get { return button_accept; }
	}

	public string EntrySelected {
		get { return entry.Text.ToString(); }
	}

	public int SpinIntSelected {
		get { return (int) spin_int.Value; }
	}
	
	public double SpinDoubleSelected {
		get { return (double) spin_double.Value; }
	}
	
	public string TwoSpinSelected {
		get { return ((int) spin_feet.Value).ToString() + ":" + ((int) spin_inches.Value).ToString(); }
	}
	
	public string TextviewSelected {
		get { return Util.RemoveTab(textview.Buffer.Text); }
	}
	

	~GenericWindow() {}
	
}

