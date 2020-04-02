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
using Gdk; //Pixbuf
using Gtk;
using Glade;
using GLib; //for Value
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using Mono.Unix;

//here using app1s_ , "s" means session
//this file has been moved from his old window to be part of app1 on Chronojump 2.0

public partial class ChronoJumpWindow
{
	private enum app1s_windowType
	{
		LOAD_SESSION,
		IMPORT_SESSION
	};
	
	private TreeStore app1s_store;
	private string app1s_selected;
	private string app1s_import_file_path;
	private int app1s_notebook_sup_entered_from; //to store from which page we entered (to return at it)

	[Widget] Gtk.Notebook app1s_notebook;
	[Widget] Gtk.Label app1s_label_select;

	/*
	 * when fillTreeView() is called, it executes:
	 * SqliteSessionSwitcher sessionSwitcher = new SqliteSessionSwitcher (databaseType, import_file_path);
	 *
	 * then if we finally import the session (on current session or on new session), there's a reloadSession() call
	 * that makes the connection point to client database (and not the database being imported),
	 * but if we cancel after the fillTreeView()
	 * then Chronojump continues on old db until load session is called,
	 * ensure to do a reloadSession() if cancel buttons are clicked or on delete_event
	 *
	 * before 2.0, all load/import gui was on separate window, then this was used:
	 * fakeButton_cancel_maybeDatabaseSwitched.Click();
	 */

	//notebook tab 0
	//notebook tab 1
	[Widget] Gtk.HBox hbox_session_more;
	[Widget] Gtk.VBox vbox_session_overview;
	[Widget] Gtk.RadioButton app1s_radio_import_new_session;
	[Widget] Gtk.RadioButton app1s_radio_import_current_session;
	[Widget] Gtk.Image app1s_image_open_database;
	[Widget] Gtk.Label app1s_label_open_database_file;
	[Widget] Gtk.Button app1s_button_select_file_import_same_database;

	//notebook tab 2
	[Widget] Gtk.TreeView app1s_treeview_session_load;
	[Widget] Gtk.Button app1s_button_accept;
	[Widget] Gtk.Button app1s_button_import;
	[Widget] Gtk.Image app1s_image_import;
	[Widget] Gtk.Entry app1s_entry_search_filter;
	[Widget] Gtk.CheckButton app1s_checkbutton_show_data_jump_run;
	[Widget] Gtk.CheckButton app1s_checkbutton_show_data_other_tests;
	[Widget] Gtk.Label app1s_file_path_import;
	[Widget] Gtk.Notebook app1s_notebook_load_button_animation;
	[Widget] Gtk.HButtonBox app1s_hbuttonbox_page2_import;
	[Widget] Gtk.VBox app1s_vbox_notebook_load;

	//notebook tab 3
	[Widget] Gtk.Label app1s_label_import_session_name;
	[Widget] Gtk.Label app1s_label_import_file;
	[Widget] Gtk.Button app1s_button_import_confirm_accept;

	//notebook tab 4
	[Widget] Gtk.ProgressBar app1s_progressbarImport;
	[Widget] Gtk.Label app1s_label_import_done_at_new_session;
	[Widget] Gtk.Label app1s_label_import_done_at_current_session;
	[Widget] Gtk.ScrolledWindow app1s_scrolledwindow_import_error;
	[Widget] Gtk.TextView app1s_textview_import_error;
	[Widget] Gtk.Image app1s_image_import1;
	[Widget] Gtk.HButtonBox app1s_hbuttonbox_page4;

	private app1s_windowType app1s_type;

	const int app1s_PAGE_MODES = 0;
	const int app1s_PAGE_IMPORT_START = 1;
	const int app1s_PAGE_SELECT_SESSION = 2; //for load session and for import
	public const int app1s_PAGE_IMPORT_CONFIRM = 3;
	public const int app1s_PAGE_IMPORT_RESULT = 4;

	private void app1s_initializeGui()
	{
		if (app1s_type == app1s_windowType.LOAD_SESSION) {
			app1s_file_path_import.Visible = false;
			app1s_notebook_load_button_animation.Visible = true;
			app1s_hbuttonbox_page2_import.Visible = false;
			app1s_label_select.Text = "<b>" + Catalog.GetString ("Load session") + "</b>";
			app1s_notebook.CurrentPage = app1s_PAGE_SELECT_SESSION;
			app1s_notebook_load_button_animation.CurrentPage = 0;
		} else {
			app1s_file_path_import.Visible = true;
			app1s_notebook_load_button_animation.Visible = false;
			app1s_hbuttonbox_page2_import.Visible = true;
			app1s_label_select.Text = "<b>" + Catalog.GetString ("Import session") + "</b>";
			app1s_button_select_file_import_same_database.Visible = false; //is shown when user want to import a second session
			app1s_notebook.CurrentPage = app1s_PAGE_IMPORT_START;
		}
		app1s_label_select.UseMarkup = true;

		app1s_image_open_database.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "folder_open.png");
		app1s_image_import.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameImport);
		app1s_image_import1.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameImport);

		app1s_createTreeView(app1s_treeview_session_load, false, false);
		app1s_store = app1s_getStore(false, false);
		app1s_treeview_session_load.Model = app1s_store;
		app1s_fillTreeView(app1s_treeview_session_load, app1s_store, false, false);

		app1s_store.SetSortColumnId(1, Gtk.SortType.Descending); //date
		app1s_store.ChangeSortColumn();

		app1s_button_accept.Sensitive = false;
		app1s_button_import.Sensitive = false;
		app1s_entry_search_filter.CanFocus = true;
		app1s_entry_search_filter.IsFocus = true;

		app1s_treeview_session_load.Selection.Changed += app1s_onSelectionEntry;

		/**
		* Uncomment if we want the session file chooser to be loaded with the dialog.
		* On Linux at least the placement of the Windows can be strange so at the moment
		* I leave it disabled until we discuss (to enable or to delete it).
		if (type == WindowType.IMPORT_SESSION) {
			chooseDatabaseToImport ();
		}
		*/
	}

	private TreeStore app1s_getStore(bool showContacts, bool showEncoderAndForceSensor) {
		TreeStore s;
		if(showContacts && showEncoderAndForceSensor)
			s = new TreeStore(
				typeof (string), typeof (string), typeof (string), typeof (string),  	//number (hidden), date, name, place
				typeof (string), typeof (string), typeof (string), typeof (string), 	//persons, sport, spllity, level
				typeof (string), typeof (string), typeof (string), typeof(string), 	//jumps s,r, runs s, i,
				typeof (string), typeof (string), typeof (string), 			//rt, pulses, mc
				typeof (string), typeof (string), typeof (string), typeof (string),	//encoder s, c, forceSensor, runEncoder
				typeof (string)								//comments
			       	);
		else if(showContacts && ! showEncoderAndForceSensor)
			s = new TreeStore(
				typeof (string), typeof (string), typeof (string), typeof (string), 	//number (hidden), date, name, place
				typeof (string), typeof (string), typeof (string), typeof (string), 	//persons, sport, spllity, level
				typeof (string), typeof (string), typeof (string), typeof(string), 	//jumps s,r, runs s, i,
				typeof (string), typeof (string), typeof (string), 			//rt, pulses, mc
				typeof (string)								//comments
			       	);
		else if(! showContacts && showEncoderAndForceSensor)
			s = new TreeStore(
				typeof (string), typeof (string), typeof (string), typeof (string), 	//number (hidden), date, name, place
				typeof (string), typeof (string), typeof (string), typeof (string), 	//persons, sport, spllity, level
				typeof (string), typeof (string), typeof (string), typeof (string),	//encoder s, c, forceSensor, runEncoder
				typeof (string)								//comments
			       	);
		else // ! showContacts && ! showEncoderAndForceSensor
			s = new TreeStore(
				typeof (string), typeof (string), typeof (string), typeof (string), 	//number (hidden), date, name, place
				typeof (string), typeof (string), typeof (string), typeof (string), 	//persons, sport, spllity, level
				typeof (string)								//comments
			       	);

		//s.SetSortFunc (0, UtilGtk.IdColumnCompare); //not needed, it's hidden
		s.SetSortFunc (1, app1s_dateColumnCompare);
		s.ChangeSortColumn();

		return s;
	}


	private static int app1s_dateColumnCompare (TreeModel model, TreeIter iter1, TreeIter iter2)
	{
		var dt1String = (model.GetValue(iter1, 1).ToString());
		var dt2String = (model.GetValue(iter2, 1).ToString());

		DateTime dt1;
		DateTime dt2;

		var converted1 = DateTime.TryParse(dt1String, out dt1);
		var converted2 = DateTime.TryParse(dt2String, out dt2);

		if(converted1 && converted2)
			return DateTime.Compare(dt1, dt2);
		else
			return 0;
	}

	private void sessionLoadWindowShow (app1s_windowType type)
	{
		this.app1s_type = type;
		app1s_initializeGui();
		app1s_recreateTreeView("loaded the dialog");
	}
	
	private void app1s_createTreeView (Gtk.TreeView tv, bool showContacts, bool showOtherTests)
	{
		tv.HeadersVisible=true;
		int count = 0;

		Gtk.TreeViewColumn colID = new Gtk.TreeViewColumn(Catalog.GetString ("Number"), new CellRendererText(), "text", count);
		colID.SortColumnId = count ++;
		colID.SortIndicator = true;
		colID.Visible = false; //hidden
		tv.AppendColumn (colID);

		//tv.AppendColumn ( Catalog.GetString ("Date"), new CellRendererText(), "text", count++);
		Gtk.TreeViewColumn colDate = new Gtk.TreeViewColumn(Catalog.GetString ("Date"), new CellRendererText(), "text", count);
		colDate.SortColumnId = count ++;
		colDate.SortIndicator = true;
		tv.AppendColumn (colDate);

		Gtk.TreeViewColumn colName = new Gtk.TreeViewColumn(Catalog.GetString ("Name"), new CellRendererText(), "text", count);
		colName.SortColumnId = count ++;
		colName.SortIndicator = true;
		tv.AppendColumn (colName);
		
		tv.AppendColumn ( Catalog.GetString ("Place"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Persons"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Sport"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Specialty"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Level"), new CellRendererText(), "text", count++);
		if(showContacts) {
			tv.AppendColumn ( Catalog.GetString ("Jumps simple"), new CellRendererText(), "text", count++);
			tv.AppendColumn ( Catalog.GetString ("Jumps reactive"), new CellRendererText(), "text", count++);
			tv.AppendColumn ( Catalog.GetString ("Races simple"), new CellRendererText(), "text", count++);
			tv.AppendColumn ( Catalog.GetString ("Races interval"), new CellRendererText(), "text", count++);
			tv.AppendColumn ( Catalog.GetString ("Reaction time"), new CellRendererText(), "text", count++);
			tv.AppendColumn ( Catalog.GetString ("Pulses"), new CellRendererText(), "text", count++);
			tv.AppendColumn ( Catalog.GetString ("MultiChronopic"), new CellRendererText(), "text", count++);
		}
		if(showOtherTests) {
			tv.AppendColumn ( Catalog.GetString ("Gravitatory encoder") + "\n" +
					Catalog.GetString("Sets") + " ; " + Catalog.GetString("Repetitions"),
					new CellRendererText(), "text", count++);
			tv.AppendColumn ( Catalog.GetString ("Inertial encoder") + "\n" +
					Catalog.GetString("Sets") + " ; " + Catalog.GetString("Repetitions"),
					new CellRendererText(), "text", count++);
			tv.AppendColumn ( Catalog.GetString ("Force sensor"), new CellRendererText(), "text", count++);
			tv.AppendColumn ( Catalog.GetString ("Race analyzer"), new CellRendererText(), "text", count++);
		}
		tv.AppendColumn ( Catalog.GetString ("Comments"), new CellRendererText(), "text", count++);
	}
	
	protected void app1s_on_entry_search_filter_changed (object o, EventArgs args) {
		app1s_recreateTreeView("changed search filter");
	}

	private void app1s_chooseDatabaseToImport()
	{
		//TODO: try it with Gtk.FileChooserWidget, then we do not need to pass app1 (parent)
		Gtk.FileChooserDialog filechooser = new Gtk.FileChooserDialog ("Choose ChronoJump database to import from",
		                                                               app1, FileChooserAction.Open,
		                                                               "Cancel",ResponseType.Cancel,
		                                                               "Open",ResponseType.Accept);

		FileFilter file_filter = new FileFilter();
		file_filter.AddPattern ("*.db");
		file_filter.Name = "ChronoJump database";
		filechooser.AddFilter (file_filter);

		if (filechooser.Run () == (int)ResponseType.Accept) {
			app1s_import_file_path = filechooser.Filename;
			//file_path_import.Text = System.IO.Path.GetFileName (import_file_path);
			app1s_file_path_import.Text = app1s_import_file_path;
			app1s_file_path_import.TooltipText = app1s_import_file_path;
			app1s_notebook.CurrentPage = app1s_PAGE_SELECT_SESSION;
		}
		filechooser.Destroy ();
		app1s_recreateTreeView ("file path changed");
	}
	void app1s_on_checkbutton_show_data_jump_run_toggled (object o, EventArgs args) {
		app1s_recreateTreeView("jump run " + app1s_checkbutton_show_data_jump_run.Active.ToString());
	}
	void app1s_on_checkbutton_show_data_other_tests_toggled (object o, EventArgs args) {
		app1s_recreateTreeView("other tests " + app1s_checkbutton_show_data_other_tests.Active.ToString());
	}
	void app1s_recreateTreeView(string message)
	{
		LogB.Information("Recreate treeview: " + message);

		UtilGtk.RemoveColumns(app1s_treeview_session_load);
		
		app1s_createTreeView(app1s_treeview_session_load,
				app1s_checkbutton_show_data_jump_run.Active, app1s_checkbutton_show_data_other_tests.Active);
		app1s_store = app1s_getStore(
				app1s_checkbutton_show_data_jump_run.Active, app1s_checkbutton_show_data_other_tests.Active);
		app1s_treeview_session_load.Model = app1s_store;
		app1s_fillTreeView(app1s_treeview_session_load, app1s_store,
				app1s_checkbutton_show_data_jump_run.Active, app1s_checkbutton_show_data_other_tests.Active);
		
		app1s_store.SetSortColumnId(1, Gtk.SortType.Descending); //date
		app1s_store.ChangeSortColumn();


		/*
		 * after clicking on checkbuttons, treeview row gets unselected
		 * call onSelectionEntry to see if there's a row selected
		 * and it will sensitive on/off button_accept as needed
		 */
		app1s_onSelectionEntry (app1s_treeview_session_load.Selection, new EventArgs ());
	}

	private void app1s_fillTreeView (Gtk.TreeView tv, TreeStore store, bool showContacts, bool showOtherTests)
	{
		string filterName = "";
		if(app1s_entry_search_filter.Text.ToString().Length > 0)
			filterName = app1s_entry_search_filter.Text.ToString();

		SqliteSessionSwitcher.DatabaseType databaseType;
		if (app1s_type == app1s_windowType.LOAD_SESSION) {
			databaseType = SqliteSessionSwitcher.DatabaseType.DEFAULT;
		} else {
			databaseType = SqliteSessionSwitcher.DatabaseType.SPECIFIC;
		}
		SqliteSessionSwitcher sessionSwitcher = new SqliteSessionSwitcher (databaseType, app1s_import_file_path);
		
		string [] mySessions = sessionSwitcher.SelectAllSessions(filterName); //returns a string of values separated by ':'
		foreach (string session in mySessions) {
			string [] myStringFull = session.Split(new char[] {':'});
		
			//don't show any text at sport, speciallity and level if it's undefined	
			string mySport = "";
			if (myStringFull[4] != Catalog.GetString(Constants.SportUndefined)) 
				mySport = Catalog.GetString(myStringFull[4]);

			string mySpeciallity = ""; //done also because Undefined has "" as name and crashes with gettext
			if (myStringFull[5] != "") 
				mySpeciallity = Catalog.GetString(myStringFull[5]);
			
			string myLevel = "";
			if (myStringFull[6] != Catalog.GetString(Constants.LevelUndefined)) 
				myLevel = Catalog.GetString(myStringFull[6]);

			if(showContacts && showOtherTests)
				app1s_store.AppendValues (
						myStringFull[0], 	//session num
						myStringFull[3],	//session date
						myStringFull[1], 	//session name
						myStringFull[2], 	//session place
						myStringFull[8],	//number of jumpers x session
						mySport,		//personsSport
						mySpeciallity,		//personsSpeciallity
						myLevel,		//personsLevel
						myStringFull[9],	//number of jumps x session
						myStringFull[10],	//number of jumpsRj x session
						myStringFull[11], 	//number of runs x session
						myStringFull[12], 	//number of runsInterval x session
						myStringFull[13], 	//number of reaction times x session
						myStringFull[14], 	//number of pulses x session
						myStringFull[15], 	//number of multiChronopics x session
						myStringFull[16], 	//number of encoder signal x session
						myStringFull[17], 	//number of encoder curve x session
						myStringFull[18], 	//number of forceSensor
						myStringFull[19], 	//number of runEncoder
						myStringFull[7]		//description of session
						);
			else if(showContacts && ! showOtherTests)
				app1s_store.AppendValues (
						myStringFull[0], 	//session num
						myStringFull[3],	//session date
						myStringFull[1], 	//session name
						myStringFull[2], 	//session place
						myStringFull[8],	//number of jumpers x session
						mySport,		//personsSport
						mySpeciallity,		//personsSpeciallity
						myLevel,		//personsLevel
						myStringFull[9],	//number of jumps x session
						myStringFull[10],	//number of jumpsRj x session
						myStringFull[11], 	//number of runs x session
						myStringFull[12], 	//number of runsInterval x session
						myStringFull[13], 	//number of reaction times x session
						myStringFull[14], 	//number of pulses x session
						myStringFull[15], 	//number of multiChronopics x session
						myStringFull[7]		//description of session
						);
			else if(! showContacts && showOtherTests)
				app1s_store.AppendValues (
						myStringFull[0], 	//session num
						myStringFull[3],	//session date
						myStringFull[1], 	//session name
						myStringFull[2], 	//session place
						myStringFull[8],	//number of jumpers x session
						mySport,		//personsSport
						mySpeciallity,		//personsSpeciallity
						myLevel,		//personsLevel
						myStringFull[16], 	//number of encoder signal x session
						myStringFull[17], 	//number of encoder curve x session
						myStringFull[18], 	//number of forceSensor
						myStringFull[19], 	//number of runEncoder
						myStringFull[7]		//description of session
						);
			else // ! showContacts && ! showOtherTests
				app1s_store.AppendValues (
						myStringFull[0], 	//session num
						myStringFull[3],	//session date
						myStringFull[1], 	//session name
						myStringFull[2], 	//session place
						myStringFull[8],	//number of jumpers x session
						mySport,		//personsSport
						mySpeciallity,		//personsSpeciallity
						myLevel,		//personsLevel
						myStringFull[7]		//description of session
						);
		}	

	}
	
	//pass 0 for first row
	public void app1s_SelectRowByPos(int rowNumber)
	{
		TreeIter iter;
		bool iterOk = app1s_store.GetIterFirst(out iter);
		if(iterOk) {
			int count = 0;
			while (count < rowNumber) {
				app1s_store.IterNext(ref iter);
				count ++;
			}
			app1s_treeview_session_load.Selection.SelectIter(iter);
		}
	}

	public bool app1s_SelectRowByID(int searchedID)
	{
		return UtilGtk.TreeviewSelectRowWithID(
				app1s_treeview_session_load, app1s_store, 0, searchedID, false);
	}
	
	private void app1s_onSelectionEntry (object o, EventArgs args)
	{
		TreeModel model;
		TreeIter iter;
		app1s_selected = "-1";

		if (((TreeSelection)o).GetSelected(out model, out iter)) {
			app1s_selected = (string)model.GetValue (iter, 0);
			app1s_button_accept.Sensitive = true;
			app1s_button_import.Sensitive = true;
		} else {
			app1s_button_accept.Sensitive = false;
			app1s_button_import.Sensitive = false;
		}
	}

	//TODO: do not need to be public ? maybe for import
	public int app1s_CurrentSessionId() {
		TreeModel model;
		TreeIter iter;

		if (app1s_treeview_session_load.Selection.GetSelected (out model, out iter)) {
			string selected = (string)model.GetValue (iter, 0);
			return Convert.ToInt32 (selected.Split (':')[0]);
		}
		return -1;
	}

	//TODO: do not need to be public ? maybe for import
	public string app1s_ImportDatabasePath() {
		return app1s_import_file_path;
	}

	//TODO: do not need to be public ? maybe for import
	public bool app1s_ImportToNewSession() {
		return app1s_radio_import_new_session.Active;
	}

	private void app1s_radio_import_new_current_sensitive ()
	{
		if(currentSession == null)
		{
			app1s_radio_import_new_session.Active = true;
			app1s_radio_import_current_session.Sensitive = false;
		} else
			app1s_radio_import_current_session.Sensitive = true;
	}

	//TODO: do not need to be public ? maybe for import
	public void app1s_LabelImportSessionName (string str)
	{
		app1s_label_import_session_name.Text = "<b>" + str + "</b>";
		app1s_label_import_session_name.UseMarkup = true;
	}
	//TODO: do not need to be public ? maybe for import
	public void app1s_LabelImportFile (string str)
	{
		app1s_label_import_file.Text = "<b>" + str + "</b>";
		app1s_label_import_file.UseMarkup = true;
	}

	//TODO: do not need to be public ? maybe for import
	public void app1s_Pulse(string str)
	{
		app1s_progressbarImport.Pulse();
		app1s_progressbarImport.Text = str;
	}
	//TODO: do not need to be public ? maybe for import
	public void app1s_PulseEnd()
	{
		app1s_progressbarImport.Fraction = 1;
		app1s_hbuttonbox_page4.Sensitive = true;
	}

	//TODO: do not need to be public ? maybe for import
	public void app1s_ShowLabelImportedOk()
	{
		if(app1s_radio_import_new_session.Active)
			app1s_label_import_done_at_new_session.Visible = true;
		else
			app1s_label_import_done_at_current_session.Visible = true;
	}

	//TODO: do not need to be public ? maybe for import
	public void app1s_ShowImportError(string str)
	{
		app1s_scrolledwindow_import_error.Visible = true;
		app1s_textview_import_error.Buffer.Text = str;
	}

	//TODO: do not need to be public ? maybe for import
	public void app1s_NotebookPage(int i)
	{
		if(i == app1s_PAGE_IMPORT_RESULT)
			app1s_hbuttonbox_page4.Sensitive = false;

		app1s_notebook.CurrentPage = i;
	}

	private void notebook_supSetOldPage()
	{
		notebook_sup.CurrentPage = app1s_notebook_sup_entered_from;

		//but if it is start page, ensure notebook_start_selector is 0
		if(notebook_sup.CurrentPage == Convert.ToInt32(notebook_sup_pages.START))
			notebook_start_selector.CurrentPage = 0;
	}

	// ---- notebook page 0 buttons ----
	void app1s_on_button_close0_clicked (object o, EventArgs args)
	{
		menus_sensitive_import_not_danger(true);
		notebook_supSetOldPage();
	}

	// ---- notebook page 1 buttons ----
	void app1s_on_button_cancel1_clicked (object o, EventArgs args)
	{
		app1s_notebook.CurrentPage = app1s_PAGE_MODES;
		reloadSession(); //explained at top of the file.
	}
	protected void app1s_on_button_select_file_import_clicked(object o, EventArgs args) {
		app1s_chooseDatabaseToImport ();
	}
	protected void app1s_on_button_select_file_import_same_database_clicked(object o, EventArgs args) {
		app1s_notebook.CurrentPage = app1s_PAGE_SELECT_SESSION;
	}

	// ---- notebook page 2 (load sesion) buttons ----
	void app1s_on_button_cancel2_clicked (object o, EventArgs args)
	{
		notebook_supSetOldPage();
	}

	void app1s_on_row_double_clicked (object o, Gtk.RowActivatedArgs args)
	{
		TreeView tv = (TreeView) o;
		TreeModel model;
		TreeIter iter;

		LogB.Information("double! type: " + app1s_type.ToString());
		if (tv.Selection.GetSelected (out model, out iter)) {
			//put selection in selected
			app1s_selected = (string) model.GetValue (iter, 0);

			if (app1s_type == app1s_windowType.LOAD_SESSION) {
				//activate on_button_accept_clicked()
				app1s_button_accept.Activate();
			} else {
				app1s_button_import.Activate();
			}
		}
	}
	
	private void app1s_on_button_accept_clicked (object o, EventArgs args)
	{
		if(app1s_selected != "-1")
		{
			app1s_notebook_load_button_animation.CurrentPage = 1;
			GLib.Timeout.Add(1000, new GLib.TimeoutHandler(app1s_on_button_accept_clicked_do));
		}
	}
	private bool app1s_on_button_accept_clicked_do ()
	{
		currentSession = SqliteSession.Select (app1s_selected);
		on_load_session_accepted();
		notebook_supSetOldPage();
		app1s_notebook_load_button_animation.CurrentPage = 0;

		return false; //do not call this again
	}

	// ---- notebook page 3 (import) buttons ----
	void app1s_on_button_back_clicked (object o, EventArgs args)
	{
		app1s_notebook.CurrentPage = app1s_PAGE_IMPORT_START;
	}
	void app1s_on_button_import_clicked (object o, EventArgs args)
	{
		if(app1s_selected != "-1") {
			currentSession = SqliteSession.Select (app1s_selected);
			on_load_session_accepted_to_import(o, args);
		}
	}

	// ---- notebook page 4 buttons ----
	private void app1s_on_button_import_confirm_back_clicked(object o, EventArgs args)
	{
		app1s_notebook.CurrentPage = app1s_PAGE_IMPORT_START;
	}
	private void app1s_on_button_import_confirm_accept_clicked(object o, EventArgs args)
	{
		app1s_hbuttonbox_page4.Sensitive = false;
		app1s_notebook.CurrentPage = app1s_PAGE_IMPORT_RESULT;
		importSessionFromDatabasePrepare2 (new object(), new EventArgs());
	}

	//import notebook page 4 buttons
	private void app1s_on_button_import_close_clicked(object o, EventArgs args)
	{
		app1s_notebook.CurrentPage = app1s_PAGE_MODES;
	}
	private void app1s_on_button_import_again_clicked(object o, EventArgs args)
	{
		app1s_label_import_done_at_new_session.Visible = false;
		app1s_label_import_done_at_current_session.Visible = false;
		app1s_scrolledwindow_import_error.Visible = false;

		app1s_radio_import_new_current_sensitive();
		app1s_label_open_database_file.Text = Catalog.GetString("Open another database");
		app1s_button_select_file_import_same_database.Visible = true;

		app1s_notebook.CurrentPage = app1s_PAGE_IMPORT_START;
	}

	public Button app1s_Button_accept
	{
		set { app1s_button_accept = value; }
		get { return app1s_button_accept; }
	}
}
