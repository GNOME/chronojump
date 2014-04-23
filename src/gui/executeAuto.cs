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
 * Copyright (C) 2014   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Gdk;
using Glade;
using System.Collections; //ArrayList
using Mono.Unix;


public class ExecuteAutoWindow
{
	[Widget] Gtk.Window execute_auto;
	[Widget] Gtk.Notebook notebook;
	[Widget] Gtk.Button button_cancel;
	[Widget] Gtk.Button button_next;

	//1st tab
	[Widget] Gtk.RadioButton radio_by_persons;
	[Widget] Gtk.RadioButton radio_by_tests;
	[Widget] Gtk.RadioButton radio_by_series;
	[Widget] Gtk.Image image_auto_by_persons;
	[Widget] Gtk.Image image_auto_by_tests;
	[Widget] Gtk.Image image_auto_by_series;
	[Widget] Gtk.Label label_persons_info;
	[Widget] Gtk.Label label_tests_info;
	[Widget] Gtk.Label label_series_info;
	
	//2nd tab
	[Widget] Gtk.Box hbox_combo_select;
	[Widget] Gtk.ComboBox combo_select;
	[Widget] Gtk.Box hbox_combo_serie1;
	[Widget] Gtk.ComboBox combo_serie1;
	[Widget] Gtk.Box hbox_combo_serie2;
	[Widget] Gtk.ComboBox combo_serie2;
	[Widget] Gtk.Box hbox_combo_serie3;
	[Widget] Gtk.ComboBox combo_serie3;
	[Widget] Gtk.Button button_add1;
	[Widget] Gtk.Button button_add2;
	[Widget] Gtk.Button button_add3;
	[Widget] Gtk.Label label_serie1;
	[Widget] Gtk.Label label_serie2;
	[Widget] Gtk.Label label_serie3;
	
	//3rd tab
	[Widget] Gtk.TreeView treeview;

	
	static ExecuteAutoWindow ExecuteAutoWindowBox;
	Gtk.Window parent;
	int sessionID;

	ArrayList orderedData;
	
	public Gtk.Button FakeButtonAccept; //to return orderedData
	
	public ExecuteAutoWindow (Gtk.Window parent) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "execute_auto", null);
		gladeXML.Autoconnect(this);
		this.parent =  parent;
		
		//put an icon to window
		UtilGtk.IconWindow(execute_auto);
		
		FakeButtonAccept = new Gtk.Button();
	}

	static new public ExecuteAutoWindow Show (Gtk.Window parent, int sessionID)
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
	static new public ExecuteAutoWindow ShowJustOrder (Gtk.Window parent, ArrayList orderedData, int orderedDataPos)
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
		notebook.CurrentPage = 0;
		radio_by_persons.Active = true;

		Pixbuf pixbuf;
		
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "auto-by-persons.png");
		image_auto_by_persons.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "auto-by-tests.png");
		image_auto_by_tests.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "auto-by-series.png");
		image_auto_by_series.Pixbuf = pixbuf;
		
		
		image_auto_by_tests.Sensitive = false;
		image_auto_by_series.Sensitive = false;
		label_persons_info.Visible = true;
		label_tests_info.Visible = false;
		label_series_info.Visible = false;

		createComboSelect();
		createComboSeries();
	}

	private void initializeShowJustOrder(int rowNumber) {

		//know if "serie" has to be plotted or not
		ExecuteAuto eaFirst = (ExecuteAuto) orderedData[0];
		createTreeview(eaFirst.serieID != -1);	//BY_SERIES != -1
		fillTreeview();
	
		//set the selected
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


		button_cancel.Label = Catalog.GetString("Close");
		button_next.Visible = false;

		notebook.CurrentPage = 2;
	}
	
	private void on_radio_mode_toggled(object o, EventArgs args) {
		image_auto_by_persons.Sensitive = radio_by_persons.Active;
		image_auto_by_tests.Sensitive = radio_by_tests.Active;
		image_auto_by_series.Sensitive = radio_by_series.Active;

		label_persons_info.Visible = radio_by_persons.Active;
		label_tests_info.Visible = radio_by_tests.Active;
		label_series_info.Visible = radio_by_series.Active;
	}

	
	ArrayList selectArray;
	
	private void createComboSelect() {
		combo_select = ComboBox.NewText ();

		string [] jumpTypes = SqliteJumpType.SelectJumpTypes("", "", false); //without alljumpsname, without filter, not only name
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

	
	ArrayList comboSerie1Array;
	ArrayList comboSerie2Array;
	ArrayList comboSerie3Array;

	private void createComboSeries() {
		combo_serie1 = ComboBox.NewText ();
		hbox_combo_serie1.PackStart(combo_serie1, true, true, 0);
		hbox_combo_serie1.ShowAll();
		comboSerie1Array = new ArrayList(0);

		combo_serie2 = ComboBox.NewText ();
		hbox_combo_serie2.PackStart(combo_serie2, true, true, 0);
		hbox_combo_serie2.ShowAll();
		comboSerie2Array = new ArrayList(0);

		combo_serie3 = ComboBox.NewText ();
		hbox_combo_serie3.PackStart(combo_serie3, true, true, 0);
		hbox_combo_serie3.ShowAll();
		comboSerie3Array = new ArrayList(0);
	}

	private void on_button_add_exercise_clicked(object o, EventArgs args) 
	{
		int selectedPos = UtilGtk.ComboGetActivePos(combo_select);
		TrCombo tc = (TrCombo) selectArray[selectedPos];
		//Log.WriteLine(tc.ToString());

		if(o == (object) button_add1) 
		{
			comboSerie1Array.Add(tc);
			UtilGtk.ComboAdd(combo_serie1, tc.trName);
			combo_serie1.Active = comboSerie1Array.Count -1;
		} else if(o == (object) button_add2) 
		{
			comboSerie2Array.Add(tc);
			UtilGtk.ComboAdd(combo_serie2, tc.trName);
			combo_serie2.Active = comboSerie2Array.Count -1;
		} else 
		{	//button_add3
			comboSerie3Array.Add(tc);
			UtilGtk.ComboAdd(combo_serie3, tc.trName);
			combo_serie3.Active = comboSerie3Array.Count -1;
		}
		
		//a test is added, sensitivize "next" button
		button_next.Sensitive = true;
	}

	private void showSeriesStuff(bool show) 
	{
		button_add2.Visible = show;
		button_add3.Visible = show;

		hbox_combo_serie2.Visible = show;
		hbox_combo_serie3.Visible = show;

		label_serie1.Visible = show;
		label_serie2.Visible = show;
		label_serie3.Visible = show;
	}
	
	TreeStore store;
	private void createTreeview(bool by_series) {
		if(by_series)
			store = new TreeStore(typeof (string), typeof (string), typeof (string)); //serie, person, test
		else
			store = new TreeStore(typeof (string), typeof (string));		//person, test
	
		treeview.Model = store;
		treeview.HeadersVisible=true;

		int i = 0;
		if(by_series) {
			UtilGtk.CreateCols(treeview, store, Catalog.GetString("Serie"), i++, true);
		}

		UtilGtk.CreateCols(treeview, store, Catalog.GetString("Person"), i++, true);
		UtilGtk.CreateCols(treeview, store, Catalog.GetString("Test"), i++, true);
	}
	
	private void fillTreeview() {
		foreach (ExecuteAuto ea in orderedData)
			store.AppendValues (ea.AsStringArray());
	}
	
	
	private void on_button_next_clicked (object o, EventArgs args)
	{
		if(notebook.CurrentPage == 0) {
			showSeriesStuff(radio_by_series.Active);
			notebook.NextPage();
		
			//next button will be sensitive when first test is added
			button_next.Sensitive = false;
		}
		else if(notebook.CurrentPage == 1) {
			ExecuteAuto.ModeTypes mode = ExecuteAuto.ModeTypes.BY_PERSONS;
			if(radio_by_tests.Active)
				mode = ExecuteAuto.ModeTypes.BY_TESTS;
			else if(radio_by_series.Active)
				mode = ExecuteAuto.ModeTypes.BY_SERIES;

			ArrayList persons = SqlitePersonSession.SelectCurrentSessionPersons(sessionID);
			orderedData = ExecuteAuto.CreateOrder(mode, persons,  
					comboSerie1Array, comboSerie2Array, comboSerie3Array);
			
			createTreeview(radio_by_series.Active);
			fillTreeview();

			button_next.Label = Catalog.GetString("Accept");
			if(orderedData.Count == 0)
				button_next.Sensitive = false;

			notebook.NextPage();
		}
		else {	// notebook.CurrentPage == 2
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
	
}


