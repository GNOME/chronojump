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
 * Copyright (C) 2014-2023   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Gdk;
//using Glade;
using System.Collections; //ArrayList
using System.Collections.Generic; //List<T>
using Mono.Unix;

public class ExecuteAutoWindow
{
	// at glade ---->
	Gtk.Window execute_auto;
	Gtk.Notebook notebook_main;
	Gtk.Button button_cancel;
	Gtk.Button button_next;

	//1st tab
	Gtk.RadioButton radio_load;
	Gtk.RadioButton radio_new;
	Gtk.Notebook notebook_load_or_new;
	Gtk.TreeView treeview_load;
	Gtk.RadioButton radio_by_persons;
	Gtk.RadioButton radio_by_tests;
	Gtk.RadioButton radio_by_sets;
	Gtk.Image image_auto_by_persons;
	Gtk.Image image_auto_by_tests;
	Gtk.Image image_auto_by_sets;
	Gtk.Label label_persons_info;
	Gtk.Label label_tests_info;
	Gtk.Label label_series_info;
	
	//2nd tab
	Gtk.Box hbox_combo_select;
	Gtk.Button button_add1;
	Gtk.Button button_add2;
	Gtk.Button button_add3;
	Gtk.Label label_serie1;
	Gtk.Label label_serie2;
	Gtk.Label label_serie3;
	Gtk.ScrolledWindow scrolled_win_serie2;
	Gtk.ScrolledWindow scrolled_win_serie3;
	Gtk.TreeView treeview_serie1;
	Gtk.TreeView treeview_serie2;
	Gtk.TreeView treeview_serie3;
	
	Gtk.Box vbox_save;
	Gtk.Entry entry_save_name;
	Gtk.Entry entry_save_description;
	Gtk.Button button_save;
	
	//3rd tab
	Gtk.TreeView treeview_result;

	TreeStore store_load;
	TreeStore store_serie1;
	TreeStore store_serie2;
	TreeStore store_serie3;
	TreeStore store_result;
	// <---- at glade
	
	Gtk.ComboBoxText combo_select;

	static ExecuteAutoWindow ExecuteAutoWindowBox;
	int sessionID;

	ExecuteAuto.ModeTypes mode;
	ArrayList orderedData;
	string [] jumpTypes;

	public Gtk.Button FakeButtonAccept; //to return orderedData
	
	public ExecuteAutoWindow (Gtk.Window parent)
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "execute_auto.glade", "execute_auto", null);
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "execute_auto.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);

		//manage window color
		if(! Config.UseSystemColor)
		{
			UtilGtk.WindowColor (execute_auto, Config.ColorBackground);
			UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundIsDark, notebook_main);
		}

		execute_auto.Parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(execute_auto);
		
		FakeButtonAccept = new Gtk.Button();
	}

	static public ExecuteAutoWindow Show (Gtk.Window parent, int sessionID)
	{
		if (ExecuteAutoWindowBox == null) {
			ExecuteAutoWindowBox = new ExecuteAutoWindow (parent);
		}	

		ExecuteAutoWindowBox.initialize();
		ExecuteAutoWindowBox.sessionID = sessionID;

		ExecuteAutoWindowBox.execute_auto.Show ();

		return ExecuteAutoWindowBox;
	}
	
	//creates and shows the third tab
	static public ExecuteAutoWindow ShowJustOrder (Gtk.Window parent, ArrayList orderedData, int orderedDataPos)
	{
		if (ExecuteAutoWindowBox == null) {
			ExecuteAutoWindowBox = new ExecuteAutoWindow (parent);
		}	

		ExecuteAutoWindowBox.initialize();
		
		ExecuteAutoWindowBox.orderedData = orderedData;
		
		ExecuteAutoWindowBox.initializeShowJustOrder(orderedDataPos);

		ExecuteAutoWindowBox.execute_auto.Show ();

		return ExecuteAutoWindowBox;
	}
	
	private void initialize() {
		notebook_main.CurrentPage = 0;
		radio_by_persons.Active = true;

		Pixbuf pixbuf;
		
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "auto-by-persons.png");
		image_auto_by_persons.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "auto-by-tests.png");
		image_auto_by_tests.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "auto-by-series.png");
		image_auto_by_sets.Pixbuf = pixbuf;
		
		
		image_auto_by_tests.Sensitive = false;
		image_auto_by_sets.Sensitive = false;
		label_persons_info.Visible = true;
		label_tests_info.Visible = false;
		label_series_info.Visible = false;

		jumpTypes = SqliteJumpType.SelectJumpTypes(false, "", "", false); //without alljumpsname, without filter, not only name

		createTreeviewLoad();
		fillTreeviewLoad();

		createComboSelect();
		createTreeviewSeries();
	}
	
	void on_radio_load_new_toggled (object obj, EventArgs args) {
		if(radio_load.Active) {
			notebook_load_or_new.CurrentPage = 0;
			vbox_save.Visible = false;
		
			button_next.Sensitive = 
				(UtilGtk.GetSelectedRowUniqueID(
					treeview_load, store_load, store_load_uniqueID_col) > 0);
		} else {
			notebook_load_or_new.CurrentPage = 1;
			vbox_save.Visible = true;
			button_next.Sensitive = true;
		}
	}

	//----- treeeview_load (start)
	
	int store_load_uniqueID_col = 6;
	private void createTreeviewLoad() {
		store_load = new TreeStore(
				typeof (string), typeof (string), typeof(string),  //name, mode, desc
				typeof (string), typeof (string), typeof (string), //serie1 jumps, serie2 jumps, serie3jumps
				typeof (string)); 	//uniqueID (hidden)
	
		treeview_load.Model = store_load;
		treeview_load.HeadersVisible=true;

		int i = 0;
		UtilGtk.CreateCols(treeview_load, store_load, Catalog.GetString("Name"), i++, true);
		UtilGtk.CreateCols(treeview_load, store_load, Catalog.GetString("Mode"), i++, true);
		UtilGtk.CreateCols(treeview_load, store_load, Catalog.GetString("Description"), i++, true);
		UtilGtk.CreateCols(treeview_load, store_load, "Tests (1)", i++, true);
		UtilGtk.CreateCols(treeview_load, store_load, "Tests (2)", i++, true);
		UtilGtk.CreateCols(treeview_load, store_load, "Tests (3)", i++, true);
		UtilGtk.CreateCols(treeview_load, store_load, "uniqueID", store_load_uniqueID_col, false);
		
		treeview_load.Selection.Changed += onLoadSelectionEntry;
	}
	
	private void fillTreeviewLoad() {
		List<ExecuteAutoSQL> sequences = SqliteExecuteAuto.Select(false, -1);

		foreach (ExecuteAutoSQL eaSQL in sequences)
			store_load.AppendValues (eaSQL.ToLoadTreeview(jumpTypes));
	}
	
	private void onLoadSelectionEntry (object o, EventArgs args)
	{
		ITreeModel model;
		TreeIter iter;

		if (((TreeSelection)o).GetSelected(out model, out iter)) {
			button_next.Sensitive = true;
		}
	}

	void on_load_row_double_clicked (object o, Gtk.RowActivatedArgs args)
	{
		ITreeModel model;
		TreeIter iter;

		if (treeview_load.Selection.GetSelected (out model, out iter)) {
			//put selection in selected
			//selected = (string) model.GetValue (iter, 0);

			//activate on_button_accept_clicked()
			button_next.Activate();
		}
	}

	private void on_treeview_load_button_release_event (object o, ButtonReleaseEventArgs args) {
		Gdk.EventButton e = args.Event;
		if (e.Button == 3) {
			Menu myMenu = new Menu ();
			Gtk.MenuItem myItem;

			myItem = new MenuItem (Catalog.GetString("Delete selected")); 
			myItem.Activated += on_delete_selected_row_clicked;
			myMenu.Attach( myItem, 0, 1, 0, 1 );

			myMenu.Popup();
			myMenu.ShowAll();
		}
	}
	
	private void on_delete_selected_row_clicked (object o, EventArgs args) {
		int uniqueID = UtilGtk.GetSelectedRowUniqueID(
					treeview_load, store_load, store_load_uniqueID_col);
		
		if(uniqueID > 0) {
			Sqlite.Delete(false, Constants.ExecuteAutoTable, uniqueID);
			store_load = UtilGtk.RemoveRow(treeview_load, store_load);
			button_next.Sensitive = false; 
		}
	}

	private void loadDo () {
		ITreeModel model;
		TreeIter iter;
		
		if (treeview_load.Selection.GetSelected (out model, out iter)) {
			int uniqueID = UtilGtk.GetSelectedRowUniqueID(
					treeview_load, store_load, store_load_uniqueID_col);
			
			if(uniqueID > 0) {
				ExecuteAutoSQL eaSQL = SqliteExecuteAuto.Select(false, uniqueID)[0];
				
				foreach(int i in eaSQL.Serie1IDs)
					button_simulate_exercise_clicked(i, 1); //first treeview
				
				mode = eaSQL.Mode;
				if(mode == ExecuteAuto.ModeTypes.BY_SETS) {
					foreach(int i in eaSQL.Serie2IDs)
						button_simulate_exercise_clicked(i, 2);
					foreach(int i in eaSQL.Serie3IDs)
						button_simulate_exercise_clicked(i, 3);
				}
			}
		}
	}

	//----- treeeview_load (end)
	

	private void initializeShowJustOrder(int rowNumber) {

		//know if "serie" has to be plotted or not
		ExecuteAuto eaFirst = (ExecuteAuto) orderedData[0];
		createTreeviewResult(eaFirst.serieID != -1);	//BY_SETS != -1
		fillTreeviewResult();
	
		//set the selected
		TreeIter iter;
		bool iterOk = store_result.GetIterFirst(out iter);
		if(iterOk) {
			int count = 0;
			while (count < rowNumber) {
				store_result.IterNext(ref iter);
				count ++;
			}
			treeview_result.Selection.SelectIter(iter);
		}


		button_cancel.Label = Catalog.GetString("Close");
		button_next.Visible = false;

		notebook_main.CurrentPage = 2;
	}
	
	private void on_radio_mode_toggled(object o, EventArgs args) {
		image_auto_by_persons.Sensitive = radio_by_persons.Active;
		image_auto_by_tests.Sensitive = radio_by_tests.Active;
		image_auto_by_sets.Sensitive = radio_by_sets.Active;

		label_persons_info.Visible = radio_by_persons.Active;
		label_tests_info.Visible = radio_by_tests.Active;
		label_series_info.Visible = radio_by_sets.Active;
	}

	
	ArrayList selectArray;
	
	private void createComboSelect() {
		combo_select = new ComboBoxText ();

		selectArray = new ArrayList(jumpTypes.Length);
		string [] jumpNamesToCombo = new String [jumpTypes.Length];
		int i =0;
		foreach(string jumpType in jumpTypes) {
			string [] j = jumpType.Split(new char[] {':'});
			string nameTranslated = Catalog.GetString(j[1]);
			
			jumpNamesToCombo[i] = nameTranslated;
			i++;
			
			ArrayList options = new ArrayList(3);
			options.Add("startIn:" + j[2]); 	//startIn
			options.Add("weight:" + j[3]);		//weight
			options.Add("description:" + j[4]);	//description
			TrCombo tc = new TrCombo(
					Convert.ToInt32(j[0]), j[1], 		//uniqueID, name
					nameTranslated, options);
			selectArray.Add(tc);
		}

		UtilGtk.ComboUpdate(combo_select, jumpNamesToCombo, "");
		combo_select.Active = 0;

		hbox_combo_select.PackStart(combo_select, true, true, 0);
		hbox_combo_select.ShowAll();
	}

	
	ArrayList treeviewSerie1Array;
	ArrayList treeviewSerie2Array;
	ArrayList treeviewSerie3Array;

	private void createTreeviewSeries() 
	{
		treeviewSerie1Array = new ArrayList(0);
		store_serie1 = new TreeStore(typeof (string), typeof (string));
		treeview_serie1.Model = store_serie1;
		treeview_serie1.HeadersVisible=false;
		UtilGtk.CreateCols(treeview_serie1, store_serie1, "", 0, true);
		UtilGtk.CreateCols(treeview_serie1, store_serie1, "", 1, true);
		treeview_serie1.Selection.Mode = SelectionMode.None;
		
		treeviewSerie2Array = new ArrayList(0);
		store_serie2 = new TreeStore(typeof (string), typeof (string));
		treeview_serie2.Model = store_serie2;
		treeview_serie2.HeadersVisible=false;
		UtilGtk.CreateCols(treeview_serie2, store_serie2, "", 0, true);
		UtilGtk.CreateCols(treeview_serie2, store_serie2, "", 1, true);
		treeview_serie2.Selection.Mode = SelectionMode.None;
		
		treeviewSerie3Array = new ArrayList(0);
		store_serie3 = new TreeStore(typeof (string), typeof (string));
		treeview_serie3.Model = store_serie3;
		treeview_serie3.HeadersVisible=false;
		UtilGtk.CreateCols(treeview_serie3, store_serie3, "", 0, true);
		UtilGtk.CreateCols(treeview_serie3, store_serie3, "", 1, true);
		treeview_serie3.Selection.Mode = SelectionMode.None;
	}


	private void button_simulate_exercise_clicked(int uniqueID, int treeviewNum) {
		int count = 0;
		foreach(TrCombo tc in selectArray) {
			if(tc.id == uniqueID)
				on_button_add_exercise_do(count, treeviewNum);
			count ++;
		}
	}

	private void on_button_add_exercise_clicked(object o, EventArgs args) 
	{
		int treeviewNum;
		if(o == (object) button_add1) 
			treeviewNum = 1;
		else if(o == (object) button_add2) 
			treeviewNum = 2;
		else
			treeviewNum = 3;

		int selectedPos = UtilGtk.ComboGetActivePos(combo_select);
		on_button_add_exercise_do(selectedPos, treeviewNum);
	}
	//can be done manually by clicking on add
	//or automatically when loading sequence
	private void on_button_add_exercise_do(int selectedPos, int treeviewNum)
	{	
		TrCombo tc = (TrCombo) selectArray[selectedPos];
		//Log.WriteLine(tc.ToString());

		if(treeviewNum == 1) 
		{
			treeviewSerie1Array.Add(tc);
			UtilGtk.TreeviewAddRow(treeview_serie1, store_serie1, 
					new String [] { treeviewSerie1Array.Count.ToString(), tc.trName }, -1 ); //at end
		} else if(treeviewNum == 2) 
		{
			treeviewSerie2Array.Add(tc);
			UtilGtk.TreeviewAddRow(treeview_serie2, store_serie2, 
					new String [] { treeviewSerie2Array.Count.ToString(), tc.trName }, -1 ); //at end
		} else 
		{	//treeviewNum == 3
			treeviewSerie3Array.Add(tc);
			UtilGtk.TreeviewAddRow(treeview_serie3, store_serie3, 
					new String [] { treeviewSerie3Array.Count.ToString(), tc.trName }, -1 ); //at end
		}
		
		button_save.Sensitive = (treeviewSerie1Array.Count > 0 && entry_save_name.Text.ToString().Length > 0);
		
		//a test is added, sensitivize "next" button
		button_next.Sensitive = true;
	}


	private void on_entry_save_name_changed(object o, EventArgs args) 
	{
		button_save.Sensitive = (treeviewSerie1Array.Count > 0 && entry_save_name.Text.ToString().Length > 0);
	}

	private void on_button_save_clicked(object o, EventArgs args) 
	{
		ExecuteAutoSQL eaSQL = new ExecuteAutoSQL(-1, entry_save_name.Text.ToString(), mode, entry_save_description.Text.ToString(), 
				getTrComboInts(treeviewSerie1Array), getTrComboInts(treeviewSerie2Array), getTrComboInts(treeviewSerie3Array));
		bool saved = eaSQL.SaveToSQL();
		
		if(saved)
			new DialogMessage(Constants.MessageTypes.INFO, Catalog.GetString("Saved"));
		else
			new DialogMessage(Constants.MessageTypes.WARNING, 
					String.Format(Catalog.GetString("Sorry, this sequence '{0}' already exists in database"), eaSQL.name));
	}

	private List<int> getTrComboInts(ArrayList arrayTrCombo) {
		List<int> IDs = new List<int>();
	        foreach(TrCombo tr in arrayTrCombo)
			IDs.Add(tr.id);
		return IDs;
	}

	private void showSeriesStuff(bool newSequence, bool bySets)
	{
		//on load do not allow to edit
		combo_select.Visible = newSequence;
		button_add1.Visible = newSequence;

		button_add2.Visible = newSequence && bySets;
		button_add3.Visible = newSequence && bySets;

		label_serie1.Visible = newSequence && bySets;
		label_serie2.Visible = newSequence && bySets;
		label_serie3.Visible = newSequence && bySets;

		if(! bySets)
			treeview_serie1.SetSizeRequest(150,120);
		else {
			treeview_serie1.SetSizeRequest(150,80);
			treeview_serie2.SetSizeRequest(150,80);
			treeview_serie3.SetSizeRequest(150,80);
		}
		
		scrolled_win_serie2.Visible = bySets;
		scrolled_win_serie3.Visible = bySets;
	}
	
	private void createTreeviewResult(bool by_sets) {
		if(by_sets)
			store_result = new TreeStore(typeof (string), typeof (string), typeof (string)); //serie, person, test
		else
			store_result = new TreeStore(typeof (string), typeof (string));		//person, test
	
		treeview_result.Model = store_result;
		treeview_result.HeadersVisible=true;

		int i = 0;
		if(by_sets) {
			UtilGtk.CreateCols(treeview_result, store_result, Catalog.GetString("Serie"), i++, true);
		}

		UtilGtk.CreateCols(treeview_result, store_result, Catalog.GetString("Person"), i++, true);
		UtilGtk.CreateCols(treeview_result, store_result, Catalog.GetString("Test"), i++, true);
	}
	
	private void fillTreeviewResult() {
		foreach (ExecuteAuto ea in orderedData)
			store_result.AppendValues (ea.AsStringArray());
	}
	
	
	private void on_button_next_clicked (object o, EventArgs args)
	{
		if(notebook_main.CurrentPage == 0) {
			if(radio_load.Active)
				loadDo(); //this also defines the 'mode' variable
			else {
				mode = ExecuteAuto.ModeTypes.BY_PERSONS;
				if(radio_by_tests.Active)
					mode = ExecuteAuto.ModeTypes.BY_TESTS;
				else if(radio_by_sets.Active)
					mode = ExecuteAuto.ModeTypes.BY_SETS;

				//next button will be sensitive when first test is added
				button_next.Sensitive = false;
			}

			showSeriesStuff(radio_new.Active, mode == ExecuteAuto.ModeTypes.BY_SETS);

			notebook_main.NextPage();
		}
		else if(notebook_main.CurrentPage == 1) {
			ArrayList persons = SqlitePersonSession.SelectCurrentSessionPersons(
					sessionID,
					false); //means: do not returnPersonAndPSlist
			orderedData = ExecuteAuto.CreateOrder(mode, persons,  
					treeviewSerie1Array, treeviewSerie2Array, treeviewSerie3Array);
			
			createTreeviewResult(mode == ExecuteAuto.ModeTypes.BY_SETS);
			fillTreeviewResult();

			button_next.Label = Catalog.GetString("Accept");
			if(orderedData.Count == 0)
				button_next.Sensitive = false;

			notebook_main.NextPage();
		}
		else {	// notebook_main.CurrentPage == 2
			FakeButtonAccept.Click(); //signal to read orderedData
		}
	}
	
	public ArrayList GetOrderedData() {
		return orderedData;
	}

	public void Close() {
		on_button_cancel_clicked (new object (), new EventArgs ());
	}

	private void on_button_cancel_clicked (object o, EventArgs args)
	{
		ExecuteAutoWindowBox.execute_auto.Hide();
		ExecuteAutoWindowBox = null;
	}
	
	private void on_delete_event (object o, DeleteEventArgs args)
	{
		ExecuteAutoWindowBox.execute_auto.Hide();
		ExecuteAutoWindowBox = null;
	}
	
	private void connectWidgets (Gtk.Builder builder)
	{
		execute_auto = (Gtk.Window) builder.GetObject ("execute_auto");
		notebook_main = (Gtk.Notebook) builder.GetObject ("notebook_main");
		button_cancel = (Gtk.Button) builder.GetObject ("button_cancel");
		button_next = (Gtk.Button) builder.GetObject ("button_next");

		//1st tab
		radio_load = (Gtk.RadioButton) builder.GetObject ("radio_load");
		radio_new = (Gtk.RadioButton) builder.GetObject ("radio_new");
		notebook_load_or_new = (Gtk.Notebook) builder.GetObject ("notebook_load_or_new");
		treeview_load = (Gtk.TreeView) builder.GetObject ("treeview_load");
		radio_by_persons = (Gtk.RadioButton) builder.GetObject ("radio_by_persons");
		radio_by_tests = (Gtk.RadioButton) builder.GetObject ("radio_by_tests");
		radio_by_sets = (Gtk.RadioButton) builder.GetObject ("radio_by_sets");
		image_auto_by_persons = (Gtk.Image) builder.GetObject ("image_auto_by_persons");
		image_auto_by_tests = (Gtk.Image) builder.GetObject ("image_auto_by_tests");
		image_auto_by_sets = (Gtk.Image) builder.GetObject ("image_auto_by_sets");
		label_persons_info = (Gtk.Label) builder.GetObject ("label_persons_info");
		label_tests_info = (Gtk.Label) builder.GetObject ("label_tests_info");
		label_series_info = (Gtk.Label) builder.GetObject ("label_series_info");

		//2nd tab
		hbox_combo_select = (Gtk.Box) builder.GetObject ("hbox_combo_select");
		button_add1 = (Gtk.Button) builder.GetObject ("button_add1");
		button_add2 = (Gtk.Button) builder.GetObject ("button_add2");
		button_add3 = (Gtk.Button) builder.GetObject ("button_add3");
		label_serie1 = (Gtk.Label) builder.GetObject ("label_serie1");
		label_serie2 = (Gtk.Label) builder.GetObject ("label_serie2");
		label_serie3 = (Gtk.Label) builder.GetObject ("label_serie3");
		scrolled_win_serie2 = (Gtk.ScrolledWindow) builder.GetObject ("scrolled_win_serie2");
		scrolled_win_serie3 = (Gtk.ScrolledWindow) builder.GetObject ("scrolled_win_serie3");
		treeview_serie1 = (Gtk.TreeView) builder.GetObject ("treeview_serie1");
		treeview_serie2 = (Gtk.TreeView) builder.GetObject ("treeview_serie2");
		treeview_serie3 = (Gtk.TreeView) builder.GetObject ("treeview_serie3");

		vbox_save = (Gtk.Box) builder.GetObject ("vbox_save");
		entry_save_name = (Gtk.Entry) builder.GetObject ("entry_save_name");
		entry_save_description = (Gtk.Entry) builder.GetObject ("entry_save_description");
		button_save = (Gtk.Button) builder.GetObject ("button_save");

		//3rd tab
		treeview_result = (Gtk.TreeView) builder.GetObject ("treeview_result");
	}
}


