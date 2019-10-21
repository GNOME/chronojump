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


public class SessionLoadWindow
{
	public enum WindowType
	{
		LOAD_SESSION,
		IMPORT_SESSION
	};
	[Widget] Gtk.Window session_load;
	
	private TreeStore store;
	private string selected;
	private string import_file_path;

	[Widget] Gtk.Notebook notebook_import;

	/*
	 * when fillTreeView() is called, it executes:
	 * SqliteSessionSwitcher sessionSwitcher = new SqliteSessionSwitcher (databaseType, import_file_path);
	 *
	 * then if we finally import the session (on current session or on new session), there's a reloadSession() call
	 * that makes the connection point to client database (and not the database being imported),
	 * but if we cancel after the fillTreeView()
	 * then Chronojump continues on old db until load session is called,
	 * so this fakeButton_cancel_maybeDatabaseSwitched
	 * ensure to do a reloadSession() if cancel buttons are clicked or on delete_event
	 */
	[Widget] Gtk.Button fakeButton_cancel_maybeDatabaseSwitched;

	//notebook import tab 0
	[Widget] Gtk.RadioButton radio_import_new_session;
	[Widget] Gtk.RadioButton radio_import_current_session;
	[Widget] Gtk.Image image_open_database;

	//notebook import tab 1
	[Widget] Gtk.TreeView treeview_session_load;
	[Widget] Gtk.Button button_accept;
	[Widget] Gtk.Button button_import;
	[Widget] Gtk.Image image_import;
	[Widget] Gtk.Entry entry_search_filter;
	[Widget] Gtk.CheckButton checkbutton_show_data_jump_run;
	[Widget] Gtk.CheckButton checkbutton_show_data_other_tests;
	[Widget] Gtk.Label file_path_import;
	[Widget] Gtk.HButtonBox hbuttonbox_page1_load;
	[Widget] Gtk.HButtonBox hbuttonbox_page1_import;

	//notebook import tab 2
	[Widget] Gtk.Label label_import_session_name;
	[Widget] Gtk.Label label_import_file;
	[Widget] Gtk.Button button_import_confirm_accept;

	//notebook import tab 3
	[Widget] Gtk.ProgressBar progressbarImport;
	[Widget] Gtk.Label label_import_done_at_new_session;
	[Widget] Gtk.Label label_import_done_at_current_session;
	[Widget] Gtk.ScrolledWindow scrolledwindow_import_error;
	[Widget] Gtk.TextView textview_import_error;
	[Widget] Gtk.Image image_import1;
	[Widget] Gtk.HButtonBox hbuttonbox_page3;


	static SessionLoadWindow SessionLoadWindowBox;
	
	private Session currentSession;
	private WindowType type;

	const int PAGE_IMPORT_START = 0;
	const int PAGE_SELECT_SESSION = 1; //for load session and for import
	public const int PAGE_IMPORT_CONFIRM = 2;
	public const int PAGE_IMPORT_RESULT = 3;

	SessionLoadWindow (Gtk.Window parent)
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "session_load.glade", "session_load", null);
		gladeXML.Autoconnect(this);
		session_load.Parent = parent;
	}

	private void initializeGui()
	{
		if (type == WindowType.LOAD_SESSION) {
			file_path_import.Visible = false;
			hbuttonbox_page1_load.Visible = true;
			hbuttonbox_page1_import.Visible = false;
			session_load.Title = Catalog.GetString ("Load session");
			notebook_import.CurrentPage = PAGE_SELECT_SESSION;
		} else {
			file_path_import.Visible = true;
			hbuttonbox_page1_load.Visible = false;
			hbuttonbox_page1_import.Visible = true;
			session_load.Title = Catalog.GetString ("Import session");
			notebook_import.CurrentPage = PAGE_IMPORT_START;
		}

		//put an icon to window
		UtilGtk.IconWindow(session_load);

		image_open_database.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "folder_open.png");
		image_import.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameImport);
		image_import1.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameImport);

		createTreeView(treeview_session_load, false, false);
		store = getStore(false, false);
		treeview_session_load.Model = store;
		fillTreeView(treeview_session_load, store, false, false);

		store.SetSortColumnId(1, Gtk.SortType.Descending); //date
		store.ChangeSortColumn();

		button_accept.Sensitive = false;
		button_import.Sensitive = false;
		entry_search_filter.CanFocus = true;
		entry_search_filter.IsFocus = true;

		// Leave the state of the Importing Comboboxes as they are by default
		/*radio_import_new_session.Active = true;
		radio_import_current_session.Sensitive = false;
		*/

		treeview_session_load.Selection.Changed += onSelectionEntry;

		/**
		* Uncomment if we want the session file chooser to be loaded with the dialog.
		* On Linux at least the placement of the Windows can be strange so at the moment
		* I leave it disabled until we discuss (to enable or to delete it).
		if (type == WindowType.IMPORT_SESSION) {
			chooseDatabaseToImport ();
		}
		*/

		fakeButton_cancel_maybeDatabaseSwitched = new Gtk.Button();
	}

	private TreeStore getStore(bool showContacts, bool showEncoderAndForceSensor) {
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
		s.SetSortFunc (1, dateColumnCompare);
		s.ChangeSortColumn();

		return s;
	}


	private static int dateColumnCompare (TreeModel model, TreeIter iter1, TreeIter iter2)
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
	
	static public SessionLoadWindow Show (Gtk.Window parent, WindowType type)
	{
		if (SessionLoadWindowBox == null) {
			SessionLoadWindowBox = new SessionLoadWindow (parent);
		}

		SessionLoadWindowBox.type = type;
		SessionLoadWindowBox.initializeGui();
		SessionLoadWindowBox.recreateTreeView("loaded the dialog");

		SessionLoadWindowBox.session_load.Show ();
		
		return SessionLoadWindowBox;
	}
	
	private void createTreeView (Gtk.TreeView tv, bool showContacts, bool showOtherTests)
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
	
	protected void on_entry_search_filter_changed (object o, EventArgs args) {
		recreateTreeView("changed search filter");
	}

	private void chooseDatabaseToImport()
	{
		Gtk.FileChooserDialog filechooser = new Gtk.FileChooserDialog ("Choose ChronoJump database to import from",
		                                                               session_load, FileChooserAction.Open,
		                                                               "Cancel",ResponseType.Cancel,
		                                                               "Open",ResponseType.Accept);

		FileFilter file_filter = new FileFilter();
		file_filter.AddPattern ("*.db");
		file_filter.Name = "ChronoJump database";
		filechooser.AddFilter (file_filter);

		if (filechooser.Run () == (int)ResponseType.Accept) {
			import_file_path = filechooser.Filename;
			//file_path_import.Text = System.IO.Path.GetFileName (import_file_path);
			file_path_import.Text = import_file_path;
			file_path_import.TooltipText = import_file_path;
			notebook_import.CurrentPage = PAGE_SELECT_SESSION;
		}
		filechooser.Destroy ();
		recreateTreeView ("file path changed");
	}
	void on_checkbutton_show_data_jump_run_toggled (object o, EventArgs args) {
		recreateTreeView("jump run " + checkbutton_show_data_jump_run.Active.ToString());
	}
	void on_checkbutton_show_data_other_tests_toggled (object o, EventArgs args) {
		recreateTreeView("other tests " + checkbutton_show_data_other_tests.Active.ToString());
	}
	void recreateTreeView(string message)
	{
		LogB.Information("Recreate treeview: " + message);

		UtilGtk.RemoveColumns(treeview_session_load);
		
		createTreeView(treeview_session_load, 
				checkbutton_show_data_jump_run.Active, checkbutton_show_data_other_tests.Active);
		store = getStore(
				checkbutton_show_data_jump_run.Active, checkbutton_show_data_other_tests.Active);
		treeview_session_load.Model = store;
		fillTreeView(treeview_session_load, store,
				checkbutton_show_data_jump_run.Active, checkbutton_show_data_other_tests.Active);
		
		store.SetSortColumnId(1, Gtk.SortType.Descending); //date
		store.ChangeSortColumn();


		/*
		 * after clicking on checkbuttons, treeview row gets unselected
		 * call onSelectionEntry to see if there's a row selected
		 * and it will sensitive on/off button_accept as needed
		 */
		onSelectionEntry (treeview_session_load.Selection, new EventArgs ());
	}

	private void fillTreeView (Gtk.TreeView tv, TreeStore store, bool showContacts, bool showOtherTests)
	{
		string filterName = "";
		if(entry_search_filter.Text.ToString().Length > 0) 
			filterName = entry_search_filter.Text.ToString();

		SqliteSessionSwitcher.DatabaseType databaseType;
		if (type == WindowType.LOAD_SESSION) {
			databaseType = SqliteSessionSwitcher.DatabaseType.DEFAULT;
		} else {
			databaseType = SqliteSessionSwitcher.DatabaseType.SPECIFIC;
		}
		SqliteSessionSwitcher sessionSwitcher = new SqliteSessionSwitcher (databaseType, import_file_path);
		
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
				store.AppendValues (
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
				store.AppendValues (
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
				store.AppendValues (
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
				store.AppendValues (
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
	public void SelectRowByPos(int rowNumber)
	{
		TreeIter iter;
		bool iterOk = store.GetIterFirst(out iter);
		if(iterOk) {
			int count = 0;
			while (count < rowNumber) {
				store.IterNext(ref iter);
				count ++;
			}
			treeview_session_load.Selection.SelectIter(iter);
		}
	}

	public bool SelectRowByID(int searchedID)
	{
		return UtilGtk.TreeviewSelectRowWithID(
				treeview_session_load, store, 0, searchedID, false);
	}
	
	private void onSelectionEntry (object o, EventArgs args)
	{
		TreeModel model;
		TreeIter iter;
		selected = "-1";

		if (((TreeSelection)o).GetSelected(out model, out iter)) {
			selected = (string)model.GetValue (iter, 0);
			button_accept.Sensitive = true;
			button_import.Sensitive = true;
		} else {
			button_accept.Sensitive = false;
			button_import.Sensitive = false;
		}
	}

	public int CurrentSessionId() {
		TreeModel model;
		TreeIter iter;

		if (treeview_session_load.Selection.GetSelected (out model, out iter)) {
			string selected = (string)model.GetValue (iter, 0);
			return Convert.ToInt32 (selected.Split (':')[0]);
		}
		return -1;
	}

	public string ImportDatabasePath() {
		return import_file_path;
	}

	public bool ImportToNewSession() {
		return radio_import_new_session.Active;
	}

	public void DisableImportToCurrentSession() {
		radio_import_new_session.Active = true;
		radio_import_current_session.Sensitive = false;
	}

	public void LabelImportSessionName (string str)
	{
		label_import_session_name.Text = str;
	}
	public void LabelImportFile (string str)
	{
		label_import_file.Text = str;
	}

	public void Pulse(string str)
	{
		progressbarImport.Pulse();
		progressbarImport.Text = str;
	}
	public void PulseEnd()
	{
		progressbarImport.Fraction = 1;
		hbuttonbox_page3.Sensitive = true;
	}

	public void ShowLabelImportedOk()
	{
		if(radio_import_new_session.Active)
			label_import_done_at_new_session.Visible = true;
		else
			label_import_done_at_current_session.Visible = true;
	}

	public void ShowImportError(string str)
	{
		scrolledwindow_import_error.Visible = true;
		textview_import_error.Buffer.Text = str;
	}

	public void NotebookPage(int i)
	{
		if(i == PAGE_IMPORT_RESULT)
			hbuttonbox_page3.Sensitive = false;

		notebook_import.CurrentPage = i;
	}

	//import notebook page 0 buttons
	void on_button_cancel0_clicked (object o, EventArgs args)
	{
		/*
		SessionLoadWindowBox.session_load.Hide();
		SessionLoadWindowBox = null;
		*/
		fakeButton_cancel_maybeDatabaseSwitched.Click();
	}
	protected void on_select_file_import_clicked(object o, EventArgs args) {
		chooseDatabaseToImport ();
	}

	//import notebook page 1 (load sesion) buttons
	void on_button_cancel1_clicked (object o, EventArgs args)
	{
		SessionLoadWindowBox.session_load.Hide();
		SessionLoadWindowBox = null;
	}

	void on_row_double_clicked (object o, Gtk.RowActivatedArgs args)
	{
		TreeView tv = (TreeView) o;
		TreeModel model;
		TreeIter iter;

		LogB.Information("double! type: " + type.ToString());
		if (tv.Selection.GetSelected (out model, out iter)) {
			//put selection in selected
			selected = (string) model.GetValue (iter, 0);

			if (type == WindowType.LOAD_SESSION) {
				//activate on_button_accept_clicked()
				button_accept.Activate();
			} else {
				button_import.Activate();
			}
		}
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		if(selected != "-1") {
			currentSession = SqliteSession.Select (selected);
			SessionLoadWindowBox.session_load.Hide();
			SessionLoadWindowBox = null;
		}
	}

	//import notebook page 1 (import) buttons
	void on_button_back_clicked (object o, EventArgs args)
	{
		notebook_import.CurrentPage = PAGE_IMPORT_START;
	}
	void on_button_import_clicked (object o, EventArgs args)
	{
		if(selected != "-1") {
			currentSession = SqliteSession.Select (selected);
		}
	}

	//import notebook page 2 buttons
	private void on_button_import_confirm_back_clicked(object o, EventArgs args)
	{
		notebook_import.CurrentPage = PAGE_IMPORT_START;
	}
	private void on_button_import_confirm_accept_clicked(object o, EventArgs args)
	{
		hbuttonbox_page3.Sensitive = false;
		notebook_import.CurrentPage = PAGE_IMPORT_RESULT;
	}

	//import notebook page 3 buttons
	private void on_button_import_close_clicked(object o, EventArgs args)
	{
		SessionLoadWindowBox.session_load.Hide();
		SessionLoadWindowBox = null;
	}
	private void on_button_import_again_clicked(object o, EventArgs args)
	{
		label_import_done_at_new_session.Visible = false;
		label_import_done_at_current_session.Visible = false;
		scrolledwindow_import_error.Visible = false;

		notebook_import.CurrentPage = PAGE_IMPORT_START;
	}

	
	void on_session_load_delete_event (object o, DeleteEventArgs args)
	{
		/*
		SessionLoadWindowBox.session_load.Hide();
		SessionLoadWindowBox = null;
		*/
		//read fakeButton_cancel_maybeDatabaseSwitched comment on the top of this file

		args.RetVal = true;
		fakeButton_cancel_maybeDatabaseSwitched.Click();
	}

	public void HideAndNull()
	{
		if(SessionLoadWindowBox.session_load != null)
			SessionLoadWindowBox.session_load.Hide();

		SessionLoadWindowBox = null;
	}

	public Button FakeButton_cancel_maybeDatabaseSwitched
	{
		get { return fakeButton_cancel_maybeDatabaseSwitched; }
	}

	public Button Button_accept 
	{
		set { button_accept = value; }
		get { return button_accept; }
	}
	public Button Button_import
	{
		get { return button_import; }
	}
	public Button Button_import_confirm_accept
	{
		get { return button_import_confirm_accept; }
	}
	
	public Session CurrentSession 
	{
		get { return currentSession; }
	}

}
