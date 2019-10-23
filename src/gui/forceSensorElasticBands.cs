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
 * Copyright (C) 2004-2019   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gdk;
using Gtk;
using Glade;
using GLib; //for Value
using System.Collections.Generic; //List<T>
using Mono.Unix;

public class ForceSensorElasticBandsWindow
{
	[Widget] Gtk.Window force_sensor_elastic_bands;
	[Widget] Gtk.Label label_header;
	//[Widget] Gtk.VBox vbox_bands;
	//[Widget] Gtk.ScrolledWindow scrolled_window_treeview;
	[Widget] Gtk.TreeView treeview;
	[Widget] Gtk.Button button_save;
	[Widget] Gtk.Button button_delete;
	[Widget] Gtk.Button button_close;
	[Widget] Gtk.Label label_total_stiffness_value;
	[Widget] Gtk.Frame frame_add_edit;
	[Widget] Gtk.Label label_edit_or_add;
	[Widget] Gtk.Entry entry_brand;
	[Widget] Gtk.Entry entry_color;
	[Widget] Gtk.Entry entry_comments;
	[Widget] Gtk.SpinButton spin_stiffness;

	[Widget] Gtk.Button fakeButton_stiffness_changed;

	static ForceSensorElasticBandsWindow ForceSensorElasticBandsWindowBox;
	
	private TreeStore store;

	//used to read data, see if it's ok, and print an error message.
	//if all is ok, destroy it with HideAndNull()
	//public bool HideOnAccept;
	
	//used when we don't need to read data, 
	//and we want to ensure next window will be created at needed size
	//public bool DestroyOnAccept;
	public int TreeviewSelectedUniqueID;

	public int uniqueID; 			//used on encoder & forceSensor edit exercise
	private int stiffnessColumn;

	private enum modes { EDITING, ADDING } 
	private modes currentMode;

	public ForceSensorElasticBandsWindow ()
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "force_sensor_elastic_bands.glade", "force_sensor_elastic_bands", "chronojump");
		gladeXML.Autoconnect(this);
	
		//HideOnAccept = true;
		//DestroyOnAccept = false;
		fakeButton_stiffness_changed = new Gtk.Button();
	}

	//for an array of widgets
	static public ForceSensorElasticBandsWindow Show (string title, string textHeader)
	{
		if (ForceSensorElasticBandsWindowBox == null) {
			ForceSensorElasticBandsWindowBox = new ForceSensorElasticBandsWindow();
		} else {
			ForceSensorElasticBandsWindowBox.setTitle(title);
			ForceSensorElasticBandsWindowBox.label_header.Text = textHeader;
		}

		ForceSensorElasticBandsWindowBox.initializeGui(title, textHeader);
		ForceSensorElasticBandsWindowBox.frame_add_edit.Sensitive = false;
		ForceSensorElasticBandsWindowBox.force_sensor_elastic_bands.Show ();
		
		return ForceSensorElasticBandsWindowBox;
	}

	private void setTitle(string title)
	{
		if(title != "")
			force_sensor_elastic_bands.Title = "Chronojump - " + title;
	}

	private void initializeGui(string title, string textHeader)
	{
		setTitle(title);
		label_header.Text = textHeader;

		//put an icon to window
		UtilGtk.IconWindow(force_sensor_elastic_bands);
	
		setTreeview();
	}

	private ForceSensorElasticBand getSelectedForceSensorElasticBand()
	{
		ForceSensorElasticBand fseb = new ForceSensorElasticBand();

		TreeIter iter = new TreeIter();
		TreeModel myModel = treeview.Model;
		if (treeview.Selection.GetSelected (out myModel, out iter))
		{
			fseb = getForceSensorElasticBand(iter);
		}

		return fseb;
	}
			
	private ForceSensorElasticBand getForceSensorElasticBand(TreeIter iter)
	{
		return new ForceSensorElasticBand(
				Convert.ToInt32(store.GetValue (iter, 0)),
				(bool) store.GetValue (iter, 1),
				store.GetValue (iter, 2).ToString(), //brand
				store.GetValue (iter, 3).ToString(), //color
				Convert.ToDouble(store.GetValue (iter, stiffnessColumn)),
				store.GetValue (iter, 5).ToString() //comments
				);
	}

	private void empty_frame()
	{
		entry_brand.Text = "";
		entry_color.Text = "";
		entry_comments.Text = "";
		spin_stiffness.Value = 0;
	}
	private void fill_frame(ForceSensorElasticBand fseb)
	{
		entry_brand.Text = fseb.Brand;
		entry_color.Text = fseb.Color;
		entry_comments.Text = fseb.Comments;
		spin_stiffness.Value = fseb.Stiffness;
	}

	//data is an ArrayList of strings[], each string [] is a row, each of its strings is a column
	private void setTreeview() 
	{
		string [] columnsString = new string [] {
			//Catalog.GetString("ID"),
			"ID",
			Catalog.GetString("Selected"),	//checkboxes
			Catalog.GetString("Brand"), Catalog.GetString("Color"),
				Catalog.GetString("Stiffness"), Catalog.GetString("Comments") 
		};
		stiffnessColumn = 4;

		store = getStore(columnsString.Length); 
		treeview.Model = store;
		prepareHeaders(columnsString);
		treeview.HeadersClickable = false;
		fillTreeview();

		/*
		LogB.Debug("aaaaaaaaaaaaaaaa1");	
		foreach (string [] line in data) {
			store.AppendValues (line);
			//Log.WriteLine(Util.StringArrayToString(line,"\n"));
		}
		LogB.Debug("aaaaaaaaaaaaaaaa2");	
		*/
		treeview.CursorChanged += on_treeview_cursor_changed;
	}

	private void fillTreeview()
	{
		List<ForceSensorElasticBand> list_fseb = SqliteForceSensorElasticBand.SelectAll(false, false);
		foreach (ForceSensorElasticBand fseb in list_fseb) {
			store.AppendValues (fseb.ToStringArray());
		}

		markActiveRows(list_fseb);
		stiffnessTotalUpdate();
	}

	private void markActiveRows(List<ForceSensorElasticBand> list_fseb) 
	{
		int count = 0;
		Gtk.TreeIter iter;
		bool okIter = store.GetIterFirst(out iter);
		if(okIter) {
			do {
				foreach(ForceSensorElasticBand fseb in list_fseb)
					if(fseb.Active && fseb.UniqueID == Convert.ToInt32(store.GetValue (iter, 0)))
						store.SetValue (iter, 1, true);
			} while ( store.IterNext(ref iter) );
		}
	}
	
	private TreeStore getStore (int columns)
	{
		//prepares the TreeStore for required columns
		Type [] types = new Type [columns];

		for (int i=0; i < columns; i++) {
			if(i == 1)
				types[1] = typeof (bool);
			else
				types[i] = typeof (string);
		}
		TreeStore myStore = new TreeStore(types);
		return myStore;
	}
	
	private void prepareHeaders(string [] columnsString)
	{
		treeview = UtilGtk.RemoveColumns(treeview);
		treeview.HeadersVisible=true;
		int i=0;
		bool visible = false;
		foreach(string myCol in columnsString) {
			if(i == 1)
				createCheckboxes(treeview, columnsString[1]);
			else
				UtilGtk.CreateCols(treeview, store, myCol, i, visible);
//			if(i == 1)	//first columns: ID, is hidden
//				store.SetSortFunc (0, UtilGtk.IdColumnCompare);
			visible = true;
			i++;
		}
	}
	
	private void on_treeview_cursor_changed (object o, EventArgs args) 
	{
		TreeIter iter = new TreeIter();
		TreeModel myModel = treeview.Model;
		if (treeview.Selection.GetSelected (out myModel, out iter))
		{
			currentMode = modes.EDITING;
			button_delete.Sensitive = true;
			label_edit_or_add.Text = Catalog.GetString("Edit selected fixation");
			ForceSensorElasticBand fseb = getSelectedForceSensorElasticBand();
			fill_frame(fseb);
			frame_add_edit.Sensitive = true;
		}
	}

	private void on_button_add_show_clicked (object o, EventArgs args)
	{
		currentMode = modes.ADDING;
		empty_frame(); //empty all
		label_edit_or_add.Text = Catalog.GetString("Add fixation");
		frame_add_edit.Sensitive = true;
	}
	private void on_button_save_clicked (object o, EventArgs args)
	{
		//1) insert on SQL
		if(currentMode == modes.ADDING)
		{
			//create fseb from frame_add_edit
			ForceSensorElasticBand fseb = new ForceSensorElasticBand(-1, false, entry_brand.Text, entry_color.Text, spin_stiffness.Value, entry_comments.Text);

			//insert on SQL
			SqliteForceSensorElasticBand.Insert(false, fseb);

			//unsensitivize frame_add_edit
			frame_add_edit.Sensitive = false;
		}
		else //(currentMode == modes.EDITING)
		{
			//get selected just to know uniqueID and if it is active
			ForceSensorElasticBand fseb = getSelectedForceSensorElasticBand();

			//change the params on frame_add_edit
			fseb.Update(entry_brand.Text, entry_color.Text, spin_stiffness.Value, entry_comments.Text);

			//update SQL
			SqliteForceSensorElasticBand.Update(false, fseb);
		}
		
		//vbox_bands.Sensitive = true;

		//2) regenerate treeview
		UtilGtk.RemoveColumns(treeview);
		setTreeview();
	}

	private void on_button_delete_clicked (object o, EventArgs args)
	{
		//1) get selected just to know uniqueID and if it is active
		ForceSensorElasticBand fseb = getSelectedForceSensorElasticBand();

		//2) delete on SQL
		SqliteForceSensorElasticBand.Delete(false, fseb.UniqueID);

		//3) regenerate treeview
		UtilGtk.RemoveColumns(treeview);
		setTreeview();

		//4) fix the rest of the gui
		empty_frame(); //empty all
		frame_add_edit.Sensitive = false;
		button_delete.Sensitive = false;
	}

	private void createCheckboxes(TreeView tv, string headerName) 
	{
		CellRendererToggle crt = new CellRendererToggle();
		crt.Visible = true;
		crt.Activatable = true;
		crt.Active = true;
		crt.Toggled += ItemToggled;

		TreeViewColumn column = new TreeViewColumn (headerName, crt, "active", 1);
		column.Clickable = true;
		tv.AppendColumn (column);
		
	}

	private void ItemToggled(object o, ToggledArgs args)
	{
		int column = 1;
		TreeIter iter;
		if (store.GetIter (out iter, new TreePath(args.Path))) 
		{
			//Log.WriteLine(args.Path);
			bool val = (bool) store.GetValue (iter, column);
			//Log.WriteLine (string.Format("toggled {0} with value {1}", args.Path, !val));

			store.SetValue (iter, column, !val);

			ForceSensorElasticBand fseb = getForceSensorElasticBand(iter);
			SqliteForceSensorElasticBand.Update(false, fseb);
		}

		stiffnessTotalUpdate();
	}

	private void stiffnessTotalUpdate()
	{
		double sum = 0;
		Gtk.TreeIter iter;
		bool okIter = store.GetIterFirst(out iter);
		if(okIter) {
			do {
				if((bool) store.GetValue (iter, 1))
					sum += Convert.ToDouble(store.GetValue (iter, stiffnessColumn));
			} while ( store.IterNext(ref iter) );
		}
		label_total_stiffness_value.Text = sum.ToString();
		fakeButton_stiffness_changed.Click();
	}

	private void on_entries_changed (object o, EventArgs args)
	{
		Gtk.Entry entry = o as Gtk.Entry;
		if (o == null)
			return;

		entry.Text = Util.MakeValidSQL(entry.Text);

		button_save.Sensitive = ( entry_brand.Text != "" || entry_color.Text != "" || entry_comments.Text != "" );
	}

	private void on_button_close_clicked (object o, EventArgs args)
	{
		ForceSensorElasticBandsWindowBox.force_sensor_elastic_bands.Hide();
		ForceSensorElasticBandsWindowBox = null;
	}

	private void on_delete_event (object o, DeleteEventArgs args)
	{
		LogB.Information("calling on_delete_event");

		//args.RetVal = true;
			
		ForceSensorElasticBandsWindowBox.force_sensor_elastic_bands.Hide();
		ForceSensorElasticBandsWindowBox = null;
	}


	/*	
	//when ! HideOnAccept, use this to close window
	//also is better to call it always tat is closed clicking on accept (after data has been readed)
	public void HideAndNull()
	{
		//this check is extra safety if there are extra EventHandlers opened with +=
		if(ForceSensorElasticBandsWindowBox.force_sensor_elastic_bands != null)
			ForceSensorElasticBandsWindowBox.force_sensor_elastic_bands.Hide();

		ForceSensorElasticBandsWindowBox = null;
	}
	*/

	/*	
	public Button Button_accept {
		set { button_accept = value; }
		get { return button_accept; }
	}
	*/

	public string TotalStiffness
	{
		get { return label_total_stiffness_value.Text; }
	}

	public Button FakeButton_stiffness_changed
	{
		get { return fakeButton_stiffness_changed; }
	}

	~ForceSensorElasticBandsWindow() {}
	
}

