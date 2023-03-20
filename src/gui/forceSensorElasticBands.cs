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
 * Copyright (C) 2004-2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using Gdk;
using Gtk;
//using Glade;
using GLib; //for Value
using System.Collections.Generic; //List<T>
using Mono.Unix;

public class ForceSensorElasticBandsWindow
{
	Gtk.Window force_sensor_elastic_bands;
	Gtk.Frame frame_main;
	Gtk.Label label_header;
	//Gtk.ScrolledWindow scrolled_window_treeview;
	Gtk.TreeView treeview;
	Gtk.Notebook notebook;

	//fist tab "add/edit"
	Gtk.Image image_add;
	Gtk.Image image_edit;
	Gtk.Image image_delete;
	Gtk.Image image_save;
	Gtk.Image image_cancel;
	Gtk.Button button_edit_save;
	Gtk.CheckButton check_active_view;
	Gtk.CheckButton check_active_edit;
	Gtk.HBox hbox_active_view;
	Gtk.HBox hbox_active_edit;
	Gtk.SpinButton spin_active_units_view;
	Gtk.SpinButton spin_active_units_edit;
	Gtk.SpinButton spin_stiffness_view;
	Gtk.SpinButton spin_stiffness_edit;
	Gtk.Label label_stiffness_of_each_fixture_view;
	Gtk.Label label_stiffness_of_each_fixture_edit;
	Gtk.Label label_total_stiffness_value;
	Gtk.Frame frame_in_use;
	Gtk.Label label_edit_or_add;
	Gtk.Entry entry_brand;
	Gtk.Entry entry_color;
	Gtk.Entry entry_comments;

	//second tab "delete confirm"
	Gtk.Image image_delete_confirm;
	Gtk.Image image_cancel_delete;
	//Gtk.TreeView treeview_delete;
	Gtk.TextView textview_delete;

	Gtk.Image image_close;
	//Gtk.Button button_close;

	Gtk.Button fakeButton_stiffness_changed;

	private TreeStore store;

	public int TreeviewSelectedUniqueID;

	public int uniqueID; 			//used on encoder & forceSensor edit exercise
	private int stiffnessColumn;
	private bool followSignals;

	private enum modes { EDITING, ADDING } 
	private modes currentMode;

	public ForceSensorElasticBandsWindow ()
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "force_sensor_elastic_bands.glade", "force_sensor_elastic_bands", "chronojump");
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "force_sensor_elastic_bands.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);

		Pixbuf pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_add.png");
		image_add.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_edit.png");
		image_edit.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "stock_delete.png");
		image_delete.Pixbuf = pixbuf;
		image_delete_confirm.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "save.png");
		image_save.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_cancel.png");
		image_cancel.Pixbuf = pixbuf;
		image_cancel_delete.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_close_blue.png");
		image_close.Pixbuf = pixbuf;

		//HideOnAccept = true;
		//DestroyOnAccept = false;
		fakeButton_stiffness_changed = new Gtk.Button();
	}

	public void Show (string title, string textHeader)
	{
		setTitle(title);
		label_header.Text = textHeader;

		initializeGui(title, textHeader);
		frame_in_use.Sensitive = false;
		followSignals = true;
		force_sensor_elastic_bands.Show ();
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

		//manage window color
		if(! Config.UseSystemColor)
		{
			UtilGtk.WindowColor(force_sensor_elastic_bands, Config.ColorBackground);
			UtilGtk.WidgetColor (frame_main, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsFrame (Config.ColorBackgroundShiftedIsDark, frame_main);
		}
	
		setTreeview();
	}

	private ForceSensorElasticBand getSelectedForceSensorElasticBand()
	{
		ForceSensorElasticBand fseb = new ForceSensorElasticBand();

		TreeIter iter = new TreeIter();
		ITreeModel myModel = treeview.Model;
		if (treeview.Selection.GetSelected (out myModel, out iter))
		{
			fseb = getForceSensorElasticBand(iter);
		}

		return fseb;
	}
			
	private ForceSensorElasticBand getForceSensorElasticBand(TreeIter iter)
	{
		LogB.Information("getForceSensorElasticBand uniqueID: " + Convert.ToInt32(store.GetValue (iter, 0)).ToString());
		return new ForceSensorElasticBand(
				Convert.ToInt32(store.GetValue (iter, 0)),
				//(bool) store.GetValue (iter, 1),
				Convert.ToInt32(store.GetValue (iter, 1)), //active
				store.GetValue (iter, 3).ToString(), //brand
				store.GetValue (iter, 4).ToString(), //color
				Convert.ToDouble(store.GetValue (iter, stiffnessColumn)),
				store.GetValue (iter, 5).ToString() //comments
				);
	}

	private void empty_frame()
	{
		check_active_view.Active = false;
		check_active_edit.Active = false;
		spin_active_units_view.Value = 1;
		spin_active_units_edit.Value = 1;
		entry_brand.Text = "";
		entry_color.Text = "";
		entry_comments.Text = "";
		spin_stiffness_view.Value = 0;
		spin_stiffness_edit.Value = 0;
	}
	private void fill_frame(ForceSensorElasticBand fseb)
	{
		if(fseb.Active == 0) {
			check_active_view.Active = false;
			check_active_edit.Active = false;
			spin_active_units_view.Value = 1;
			spin_active_units_edit.Value = 1;
		}
		else {
			check_active_view.Active = true;
			check_active_edit.Active = true;
			spin_active_units_view.Value = fseb.Active;
			spin_active_units_edit.Value = fseb.Active;
		}

		entry_brand.Text = fseb.Brand;
		entry_color.Text = fseb.Color;
		entry_comments.Text = fseb.Comments;
		spin_stiffness_view.Value = fseb.Stiffness;
		spin_stiffness_edit.Value = fseb.Stiffness;
	}

	//data is an ArrayList of strings[], each string [] is a row, each of its strings is a column
	private void setTreeview() 
	{
		string [] columnsString = new string [] {
			"ID",
			Catalog.GetString("Attached units"),
			Catalog.GetString("Stiffness"),
			Catalog.GetString("Brand"),
			Catalog.GetString("Color"),
			Catalog.GetString("Comments")
		};
		stiffnessColumn = 2;

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

		stiffnessTotalUpdate();
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
		treeview = UtilGtk.RemoveColumns(treeview);
		treeview.HeadersVisible=true;
		int i=0;
		bool visible = false;
		foreach(string myCol in columnsString) {
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
		ITreeModel myModel = treeview.Model;
		if (treeview.Selection.GetSelected (out myModel, out iter))
		{
			currentMode = modes.EDITING;
			label_edit_or_add.Text = Catalog.GetString("Edit selected");
			ForceSensorElasticBand fseb = getSelectedForceSensorElasticBand();
			fill_frame(fseb);
			frame_in_use.Sensitive = true;
		}
	}

	private int getSelectedID()
	{
		TreeIter iter = new TreeIter();
		ITreeModel model = treeview.Model;
		if (treeview.Selection.GetSelected (out model, out iter))
			return Convert.ToInt32(model.GetValue(iter, 0).ToString());

		return -1;
	}
	//pass 0 for first row
	private void selectRow(int rowNumber)
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

	private void on_button_add_clicked (object o, EventArgs args)
	{
		currentMode = modes.ADDING;
		empty_frame(); //empty all
		label_edit_or_add.Text = Catalog.GetString("Add new elastic band/tube");
		treeview.Selection.UnselectAll();

		notebook.CurrentPage = 1;
	}

	private void on_button_edit_clicked (object o, EventArgs args)
	{
		notebook.CurrentPage = 1;
	}

	private void on_button_edit_save_clicked (object o, EventArgs args)
	{
		int active = 0;
		if(check_active_edit.Active)
			active = Convert.ToInt32(spin_active_units_edit.Value);

		//1) insert on SQL
		if(currentMode == modes.ADDING)
		{
			//create fseb from frame_in_use
			ForceSensorElasticBand fseb = new ForceSensorElasticBand(-1, active, entry_brand.Text, entry_color.Text, spin_stiffness_edit.Value, entry_comments.Text);

			//insert on SQL
			SqliteForceSensorElasticBand.Insert(false, fseb);

			//unsensitivize frame_in_use
			frame_in_use.Sensitive = false;
		}
		else //(currentMode == modes.EDITING)
		{
			//get selected just to know uniqueID and if it is active
			ForceSensorElasticBand fseb = getSelectedForceSensorElasticBand();

			//change the params on frame_in_use
			fseb.Update(active, entry_brand.Text, entry_color.Text, spin_stiffness_edit.Value, entry_comments.Text);

			//update SQL
			SqliteForceSensorElasticBand.Update(false, fseb);

			//unsensitivize frame_in_use
			frame_in_use.Sensitive = false;
		}

		//udpate the main (view) tab
		spin_stiffness_view.Value = spin_stiffness_edit.Value;

		followSignals = false;
		check_active_view.Active = check_active_edit.Active;
		spin_active_units_view.Value = spin_active_units_edit.Value;
		followSignals = true;

		//vbox_bands.Sensitive = true;

		//2) regenerate treeview
		UtilGtk.RemoveColumns(treeview);
		setTreeview();
		notebook.CurrentPage = 0;
	}

	private void on_button_edit_cancel_clicked (object o, EventArgs args)
	{
		if(currentMode == modes.ADDING)
			frame_in_use.Sensitive = false;

		notebook.CurrentPage = 0;
	}

	private void on_button_delete_clicked (object o, EventArgs args)
	{
		ForceSensorElasticBand fseb = getSelectedForceSensorElasticBand();
		List<string> sessionsWithThisEB = SqliteForceSensorElasticBand.SelectSessionNamesWithCapturesWithElasticBand (fseb.UniqueID);

		if(sessionsWithThisEB.Count == 0)
			on_button_delete_confirm_clicked (o, args);
		else {
			textview_delete.Buffer.Text = Util.ListStringToString(sessionsWithThisEB);
			notebook.CurrentPage = 2;
		}
	}
	private void on_button_cancel_delete_clicked (object o, EventArgs args)
	{
		notebook.CurrentPage = 0;
	}
	private void on_button_delete_confirm_clicked (object o, EventArgs args)
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
		frame_in_use.Sensitive = false;

		notebook.CurrentPage = 0;
	}

	private void stiffnessTotalUpdate()
	{
		double sum = 0;
		Gtk.TreeIter iter;
		bool okIter = store.GetIterFirst(out iter);
		if(okIter) {
			do {
				int mult = Convert.ToInt32(store.GetValue (iter, 1));
				sum += mult * Convert.ToDouble(store.GetValue (iter, stiffnessColumn));
			} while ( store.IterNext(ref iter) );
		}
		label_total_stiffness_value.Text = sum.ToString();
		fakeButton_stiffness_changed.Click();
	}

	private void on_check_active_view_toggled (object o, EventArgs args)
	{
		hbox_active_view.Visible = check_active_view.Active;
		on_spin_active_units_view_value_changed (new object (), new EventArgs ());

		check_active_edit.Active = check_active_view.Active;
	}
	private void on_check_active_edit_toggled (object o, EventArgs args)
	{
		hbox_active_edit.Visible = check_active_edit.Active;
	}


	private void on_spin_active_units_view_value_changed (object o, EventArgs args)
	{
		label_stiffness_of_each_fixture_view.Visible = (Convert.ToInt32(spin_active_units_view.Value) > 1);
		label_stiffness_of_each_fixture_edit.Visible = (Convert.ToInt32(spin_active_units_edit.Value) > 1);

		spin_active_units_edit.Value = spin_active_units_view.Value;

		/*
		//save and update treeview
		currentMode = modes.EDITING;
		on_button_edit_save_clicked (new object (), new EventArgs ());
		*/

		if(followSignals)
			updateFixtures();
	}

	private void updateFixtures()
	{
		TreeIter iter = new TreeIter();
		ITreeModel model = treeview.Model;
		int active = 0;
		if (treeview.Selection.GetSelected (out model, out iter))
			if(check_active_view.Active)
				active = Convert.ToInt32(spin_active_units_view.Value);

		model.SetValue(iter, 1, active.ToString());

		ForceSensorElasticBand fseb = getSelectedForceSensorElasticBand();

		//change the params on frame_in_use
		fseb.Active = active;

		//update SQL
		SqliteForceSensorElasticBand.Update(false, fseb);

		stiffnessTotalUpdate();
	}

	private void on_entries_changed (object o, EventArgs args)
	{
		Gtk.Entry entry = o as Gtk.Entry;
		if (o == null)
			return;

		entry.Text = Util.MakeValidSQL(entry.Text);

		button_edit_save.Sensitive = ( entry_brand.Text != "" || entry_color.Text != "" || entry_comments.Text != "" );
	}

	private void on_button_close_clicked (object o, EventArgs args)
	{
		force_sensor_elastic_bands.Hide();
	}

	private void on_delete_event (object o, DeleteEventArgs args)
	{
		LogB.Information("calling on_delete_event on ForceSensorElasticBandsWindow");

		//args.RetVal = true;
			
		force_sensor_elastic_bands.Hide();
	}

	public double TotalStiffness
	{
		get {
			if(label_total_stiffness_value.Text == "")
				return 0;
			return Convert.ToDouble(label_total_stiffness_value.Text);
		}
	}

	public Button FakeButton_stiffness_changed
	{
		get { return fakeButton_stiffness_changed; }
	}

	private void connectWidgets (Gtk.Builder builder)
	{
		force_sensor_elastic_bands = (Gtk.Window) builder.GetObject ("force_sensor_elastic_bands");
		frame_main = (Gtk.Frame) builder.GetObject ("frame_main");
		label_header = (Gtk.Label) builder.GetObject ("label_header");
		//scrolled_window_treeview = (Gtk.ScrolledWindow) builder.GetObject ("scrolled_window_treeview");
		treeview = (Gtk.TreeView) builder.GetObject ("treeview");
		notebook = (Gtk.Notebook) builder.GetObject ("notebook");

		//fist tab "add/edit"
		image_add = (Gtk.Image) builder.GetObject ("image_add");
		image_edit = (Gtk.Image) builder.GetObject ("image_edit");
		image_delete = (Gtk.Image) builder.GetObject ("image_delete");
		image_save = (Gtk.Image) builder.GetObject ("image_save");
		image_cancel = (Gtk.Image) builder.GetObject ("image_cancel");
		button_edit_save = (Gtk.Button) builder.GetObject ("button_edit_save");
		check_active_view = (Gtk.CheckButton) builder.GetObject ("check_active_view");
		check_active_edit = (Gtk.CheckButton) builder.GetObject ("check_active_edit");
		hbox_active_view = (Gtk.HBox) builder.GetObject ("hbox_active_view");
		hbox_active_edit = (Gtk.HBox) builder.GetObject ("hbox_active_edit");
		spin_active_units_view = (Gtk.SpinButton) builder.GetObject ("spin_active_units_view");
		spin_active_units_edit = (Gtk.SpinButton) builder.GetObject ("spin_active_units_edit");
		spin_stiffness_view = (Gtk.SpinButton) builder.GetObject ("spin_stiffness_view");
		spin_stiffness_edit = (Gtk.SpinButton) builder.GetObject ("spin_stiffness_edit");
		label_stiffness_of_each_fixture_view = (Gtk.Label) builder.GetObject ("label_stiffness_of_each_fixture_view");
		label_stiffness_of_each_fixture_edit = (Gtk.Label) builder.GetObject ("label_stiffness_of_each_fixture_edit");
		label_total_stiffness_value = (Gtk.Label) builder.GetObject ("label_total_stiffness_value");
		frame_in_use = (Gtk.Frame) builder.GetObject ("frame_in_use");
		label_edit_or_add = (Gtk.Label) builder.GetObject ("label_edit_or_add");
		entry_brand = (Gtk.Entry) builder.GetObject ("entry_brand");
		entry_color = (Gtk.Entry) builder.GetObject ("entry_color");
		entry_comments = (Gtk.Entry) builder.GetObject ("entry_comments");

		//second tab "delete confirm"
		image_delete_confirm = (Gtk.Image) builder.GetObject ("image_delete_confirm");
		image_cancel_delete = (Gtk.Image) builder.GetObject ("image_cancel_delete");
		//treeview_delete = (Gtk.TreeView) builder.GetObject ("treeview_delete");
		textview_delete = (Gtk.TextView) builder.GetObject ("textview_delete");

		image_close = (Gtk.Image) builder.GetObject ("image_close");
		//button_close = (Gtk.Button) builder.GetObject ("button_close");

		fakeButton_stiffness_changed = (Gtk.Button) builder.GetObject ("fakeButton_stiffness_changed");
	}


	~ForceSensorElasticBandsWindow() {}
	
}

