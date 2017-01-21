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
 * Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Gdk;
using Glade;
using GLib; //for Value
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using System.Collections.Generic; //List
using Mono.Unix;
using System.Threading;
using System.IO; 
using LongoMatch.Gui;
using LongoMatch.Video.Capturer;
using LongoMatch.Video.Common;

//load person (jumper)
public class PersonRecuperateWindow {
	
	[Widget] protected Gtk.Window person_recuperate;
	
	[Widget] protected Gtk.CheckButton checkbutton_sorted_by_creation_date;
	
	protected TreeStore store;
	protected string selected;
	[Widget] protected Gtk.TreeView treeview_person_recuperate;
	[Widget] protected Gtk.Button button_recuperate;
	[Widget] protected Gtk.Statusbar statusbar1;
	[Widget] protected Gtk.Entry entry_search_filter;
	
	[Widget] protected Gtk.Box hbox_from_session_hide; //used in person recuperate multiple (hided in current class)
	[Widget] protected Gtk.Box hbox_combo_select_checkboxes_hide; //used in person recuperate multiple (hided in current class)
	[Widget] protected Gtk.Box hbox_search_filter_hide; //used in person recuperateWindow (hided in inherited class)
	
	static PersonRecuperateWindow PersonRecuperateWindowBox;

	protected Gtk.Window parent;
	
	protected Person currentPerson;
	protected Session currentSession;
	protected PersonSession currentPersonSession;

	protected int columnId = 0;
	protected int firstColumn = 0;
	protected int pDN;
	
	public Gtk.Button fakeButtonDone;

	protected PersonRecuperateWindow () {
	}

	PersonRecuperateWindow (Gtk.Window parent, Session currentSession) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "person_recuperate.glade", "person_recuperate", null);
		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(person_recuperate);

		this.currentSession = currentSession;
		
		fakeButtonDone = new Gtk.Button();
	
		//no posible to recuperate until one person is selected
		button_recuperate.Sensitive = false;
	
		hbox_from_session_hide.Hide(); //used in person recuperate multiple (hided in current class)
		hbox_combo_select_checkboxes_hide.Hide(); //used in person recuperate multiple (hided in current class)
		
		store = new TreeStore( 
				typeof (string), typeof (string), typeof (string), typeof (string), typeof (string) );
		createTreeView(treeview_person_recuperate, 0);
		treeview_person_recuperate.Model = store;
		fillTreeView(treeview_person_recuperate, store, "");

		treeview_person_recuperate.Model = store;

		treeview_person_recuperate.Selection.Changed += onSelectionEntry;
	}
	
	static public PersonRecuperateWindow Show (Gtk.Window parent, Session currentSession, int pDN)
	{
		if (PersonRecuperateWindowBox == null) {
			PersonRecuperateWindowBox = new PersonRecuperateWindow (parent, currentSession);
		}
		PersonRecuperateWindowBox.pDN = pDN;

		PersonRecuperateWindowBox.person_recuperate.Show ();
		
		return PersonRecuperateWindowBox;
	}
	
	protected void createTreeView (Gtk.TreeView tv, int count) {
		tv.HeadersVisible=true;
		
		UtilGtk.CreateCols(tv, store, Catalog.GetString("ID"), count++, true);
		UtilGtk.CreateCols(tv, store, Catalog.GetString("Name"), count++, true);
		UtilGtk.CreateCols(tv, store, Catalog.GetString("Sex"), count++, true);
		UtilGtk.CreateCols(tv, store, Catalog.GetString("Date of Birth"), count++, true);
		UtilGtk.CreateCols(tv, store, Catalog.GetString("Description"), count++, true);

		//sort non textual cols	
		store.SetSortFunc (firstColumn + 0, UtilGtk.IdColumnCompare);
		//store.SetSortFunc (firstColumn + 3, heightColumnCompare);
		//store.SetSortFunc (firstColumn + 4, weightColumnCompare);
		//store.SetSortFunc (firstColumn + 5, birthColumnCompare);
	}

/*	
	//cannot be double
	public int heightColumnCompare (TreeModel model, TreeIter iter1, TreeIter iter2)     {
		double val1 = 0;
		double val2 = 0;
		val1 = Convert.ToDouble(model.GetValue(iter1, firstColumn + 3));
		val2 = Convert.ToDouble(model.GetValue(iter2, firstColumn + 3));
		
		return (int) (10*val1-10*val2);
	}

	//cannot be double
	public int weightColumnCompare (TreeModel model, TreeIter iter1, TreeIter iter2)     {
		double val1 = 0;
		double val2 = 0;
		val1 = Convert.ToDouble(model.GetValue(iter1, firstColumn + 4));
		val2 = Convert.ToDouble(model.GetValue(iter2, firstColumn + 4));
		
		return (int) (10*val1-10*val2);
	}
*/

	/*
	public int birthColumnCompare (TreeModel model, TreeIter iter1, TreeIter iter2)     {
		DateTime val1; 
		DateTime val2; 
		val1 = UtilDate.DateAsDateTime(model.GetValue(iter1, firstColumn + 5).ToString());
		val2 = UtilDate.DateAsDateTime(model.GetValue(iter2, firstColumn + 5).ToString());
		
		return DateTime.Compare(val1, val2);
	}
	*/

	private void fillTreeView (Gtk.TreeView tv, TreeStore store, string searchFilterName) 
	{
		int except = currentSession.UniqueID;
		int inSession = -1;	//search persons for recuperating in all sessions
		ArrayList myPersons = SqlitePerson.SelectAllPersonsRecuperable("name", except, inSession, searchFilterName); 
		
		foreach (Person person in myPersons) {
			store.AppendValues (
					person.UniqueID.ToString(), 
					person.Name, 
					getCorrectSex(person.Sex), 
					person.DateBorn.ToShortDateString(), 
					person.Description);
		}	
		
		//show sorted by column Name	
		store.SetSortColumnId(1, Gtk.SortType.Ascending);
		store.ChangeSortColumn();
	}

	protected string getCorrectSex (string sex) 
	{
		if (sex == Constants.M) return  Catalog.GetString("Man");
		//this "F" is in spanish, change in the future to "W"
		else if (sex == Constants.F) return  Catalog.GetString ("Woman");
		else { 
			return ""; //PersonsRecuperateFromOtherSessionWindow should pass a "" for ALL PERSONS
		}
	}
	
	protected void on_entry_search_filter_changed (object o, EventArgs args) {
		store = new TreeStore( typeof (string), typeof (string), typeof (string), typeof (string), typeof (string) );
		treeview_person_recuperate.Model = store;

		string myFilter = "";
		if(entry_search_filter.Text.ToString().Length > 0) 
			myFilter = entry_search_filter.Text.ToString();

		fillTreeView(treeview_person_recuperate,store, myFilter);

		//unselect all and make button_recuperate unsensitive
		treeview_person_recuperate.Selection.UnselectAll();
		button_recuperate.Sensitive = false;
	}

	protected virtual void onSelectionEntry (object o, EventArgs args)
	{
		TreeModel model;
		TreeIter iter;
		selected = "-1";

		if (((TreeSelection)o).GetSelected(out model, out iter))
		{
			selected = (string)model.GetValue (iter, 0);
			button_recuperate.Sensitive = true;
		}
	}

	
	protected virtual void on_button_close_clicked (object o, EventArgs args)
	{
		PersonRecuperateWindowBox.person_recuperate.Hide();
		PersonRecuperateWindowBox = null;
	}
	
	protected virtual void on_person_recuperate_delete_event (object o, DeleteEventArgs args)
	{
		PersonRecuperateWindowBox.person_recuperate.Hide();
		PersonRecuperateWindowBox = null;
	}
	
	protected virtual void on_row_double_clicked (object o, Gtk.RowActivatedArgs args)
	{
		TreeView tv = (TreeView) o;
		TreeModel model;
		TreeIter iter;

		if (tv.Selection.GetSelected (out model, out iter)) {
			selected = (string) model.GetValue (iter, 0);
			
			//activate on_button_recuperate_clicked()
			button_recuperate.Activate();
		}
	}
	
	protected virtual void on_button_recuperate_clicked (object o, EventArgs args)
	{
		if(selected != "-1")
		{
			currentPerson = SqlitePerson.Select(Convert.ToInt32(selected));
				
			PersonSession myPS = SqlitePersonSession.Select(currentPerson.UniqueID, -1); //if sessionID == -1 we search data in last sessionID
			//this inserts in DB
			currentPersonSession = new PersonSession (
					currentPerson.UniqueID, currentSession.UniqueID, 
					myPS.Height, myPS.Weight, 
					myPS.SportID, myPS.SpeciallityID,
					myPS.Practice,
					myPS.Comments, 
					false); //dbconOpened
						
			store = new TreeStore( typeof (string), typeof (string), typeof (string), typeof (string), typeof (string) );
			treeview_person_recuperate.Model = store;

			fillTreeView(treeview_person_recuperate,store, entry_search_filter.Text.ToString());

			statusbar1.Push( 1, Catalog.GetString("Loaded") + " " + currentPerson.Name );

			//no posible to recuperate until one person is selected
			button_recuperate.Sensitive = false;
			
			fakeButtonDone.Click();
		}
	}

	public Button FakeButtonDone 
	{
		set { fakeButtonDone = value; }
		get { return fakeButtonDone; }
	}

	
	public Person CurrentPerson {
		get { return currentPerson; }
	}
	public PersonSession CurrentPersonSession {
		get { return currentPersonSession; }
	}

}


//load person (jumper)
public class PersonsRecuperateFromOtherSessionWindow : PersonRecuperateWindow 
{
	static PersonsRecuperateFromOtherSessionWindow PersonsRecuperateFromOtherSessionWindowBox;
	
	[Widget] Gtk.Box hbox_combo_sessions;
	[Widget] Gtk.ComboBox combo_sessions;
	[Widget] protected Gtk.Box hbox_combo_select_checkboxes;
	[Widget] protected Gtk.ComboBox combo_select_checkboxes;
	
	
	protected static string [] comboCheckboxesOptions = {
		Catalog.GetString("All"),
		Catalog.GetString("None"),
		Catalog.GetString("Selected"),
	};
	
	protected PersonsRecuperateFromOtherSessionWindow () {
	}

	PersonsRecuperateFromOtherSessionWindow (Gtk.Window parent, Session currentSession) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "person_recuperate.glade", "person_recuperate", null);
		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(person_recuperate);
		person_recuperate.Title = Catalog.GetString("Load persons from other session");

	
		//this class doesn't allow to search by name
		hbox_search_filter_hide.Hide();
		
		this.currentSession = currentSession;
		
		fakeButtonDone = new Gtk.Button();
	
		firstColumn = 1;
	
		createComboSessions();
		createComboSelectCheckboxes();
		combo_select_checkboxes.Active = 0; //ALL
		createCheckboxes(treeview_person_recuperate);
		
		store = new TreeStore( typeof (bool), 
				typeof (string), typeof (string), typeof (string), typeof (string), typeof (string) );
		createTreeView(treeview_person_recuperate, 1);
		treeview_person_recuperate.Model = store;
		
		string myText = UtilGtk.ComboGetActive(combo_sessions);
		if(myText != "") {
			string [] myStringFull = myText.Split(new char[] {':'});
			fillTreeView( treeview_person_recuperate, store, 
					currentSession.UniqueID, //except current session
					Convert.ToInt32(myStringFull[0]) //select from this session (on combo_sessions)
					);
		}

		//no posible to recuperate until one person is selected
		button_recuperate.Sensitive = false;
	
		treeview_person_recuperate.Selection.Changed += onSelectionEntry;
	}

	static public PersonsRecuperateFromOtherSessionWindow Show (
			Gtk.Window parent, Session currentSession)
	{
		if (PersonsRecuperateFromOtherSessionWindowBox == null) {
			PersonsRecuperateFromOtherSessionWindowBox = 
				new PersonsRecuperateFromOtherSessionWindow (parent, currentSession);
		}
		PersonsRecuperateFromOtherSessionWindowBox.person_recuperate.Show ();
		
		return PersonsRecuperateFromOtherSessionWindowBox;
	}
	
	private void createComboSessions() {
		combo_sessions = ComboBox.NewText();

		bool commentsDisable = true;
		int sessionIdDisable = currentSession.UniqueID; //for not showing current session on the list
		UtilGtk.ComboUpdate(combo_sessions, SqliteSession.SelectAllSessionsSimple(commentsDisable, sessionIdDisable), "");

		combo_sessions.Changed += new EventHandler (on_combo_sessions_changed);

		hbox_combo_sessions.PackStart(combo_sessions, true, true, 0);
		hbox_combo_sessions.ShowAll();
		combo_sessions.Sensitive = true;
	}
	
	private void on_combo_sessions_changed(object o, EventArgs args) {
		string myText = UtilGtk.ComboGetActive(combo_sessions);
		if(myText != "") {
			store = new TreeStore( typeof (bool), 
					typeof (string), typeof (string), typeof (string), typeof (string), typeof (string) );
			treeview_person_recuperate.Model = store;
			
			string [] myStringFull = myText.Split(new char[] {':'});

			//fill the treeview passing the uniqueID of selected session as the reference for loading persons
			fillTreeView( treeview_person_recuperate, store, 
					currentSession.UniqueID, //except current session
					Convert.ToInt32(myStringFull[0]) //select from this session (on combo_sessions)
					);
		}
	
		//check if there are rows checked for having sensitive or not in recuperate button
		buttonRecuperateChangeSensitiveness();
	}
	
	protected void createComboSelectCheckboxes() {
		combo_select_checkboxes = ComboBox.NewText ();
		UtilGtk.ComboUpdate(combo_select_checkboxes, comboCheckboxesOptions, "");
		
		//combo_select_checkboxes.DisableActivate ();
		combo_select_checkboxes.Changed += new EventHandler (on_combo_select_checkboxes_changed);

		hbox_combo_select_checkboxes.PackStart(combo_select_checkboxes, true, true, 0);
		hbox_combo_select_checkboxes.ShowAll();
		combo_select_checkboxes.Sensitive = true;
	}
	
	protected void on_combo_select_checkboxes_changed(object o, EventArgs args) {
		string myText = UtilGtk.ComboGetActive(combo_select_checkboxes);
			
		if (myText != "" & myText != Catalog.GetString("Selected")) {
			try {
				markSelected(myText);
			} catch {
				LogB.Warning("Do later!!");
			}
		}
	}
	
	protected void markSelected(string selected) {
		Gtk.TreeIter iter;
		bool okIter = store.GetIterFirst(out iter);
		if(okIter) {
			if(selected == Catalog.GetString("All")) {
				do {
					store.SetValue (iter, 0, true);
				} while ( store.IterNext(ref iter) );
			} else if(selected == Catalog.GetString("None")) {
				do {
					store.SetValue (iter, 0, false);
				} while ( store.IterNext(ref iter) );
			}
		}
			
		//check if there are rows checked for having sensitive or not in recuperate button
		buttonRecuperateChangeSensitiveness();
	}
	

	protected void createCheckboxes(TreeView tv) 
	{
		CellRendererToggle crt = new CellRendererToggle();
		crt.Visible = true;
		crt.Activatable = true;
		crt.Active = true;
		crt.Toggled += ItemToggled;

		TreeViewColumn column = new TreeViewColumn ("", crt, "active", 0);
		column.Clickable = true;
		tv.InsertColumn (column, 0);
	}

	protected void ItemToggled(object o, ToggledArgs args) {
		LogB.Information("Toggled");

		int column = 0;
		TreeIter iter;
		if (store.GetIter (out iter, new TreePath(args.Path))) 
		{
			bool val = (bool) store.GetValue (iter, column);
			LogB.Information (string.Format("toggled {0} with value {1}", args.Path, !val));

			store.SetValue (iter, column, !val);
		
			combo_select_checkboxes.Active = UtilGtk.ComboMakeActive(comboCheckboxesOptions, Catalog.GetString("Selected"));

			//check if there are rows checked for having sensitive or not in recuperate button
			buttonRecuperateChangeSensitiveness();
		}
	}

	
	private void fillTreeView (Gtk.TreeView tv, TreeStore store, int except, int inSession) 
	{
		ArrayList myPersons = SqlitePerson.SelectAllPersonsRecuperable("name", except, inSession, ""); //"" is searchFilterName (not implemented on recuperate multiple)

		foreach (Person person in myPersons) {
			store.AppendValues (
					true,
					person.UniqueID.ToString(), 
					person.Name, 
					getCorrectSex(person.Sex), 
					person.DateBorn.ToShortDateString(), 
					person.Description);
		}	
		
		//show sorted by column Name	
		store.SetSortColumnId(2, Gtk.SortType.Ascending);
	}

	//protected override void on_treeview_person_recuperate_cursor_changed (object o, EventArgs args)
	protected override void onSelectionEntry (object o, EventArgs args)
	{
		//unselect, because in this treeview the important it's what is checked on first cloumn, and not the selected row
		treeview_person_recuperate.Selection.UnselectAll();
	}
	
	protected override void on_button_close_clicked (object o, EventArgs args)
	{
		PersonsRecuperateFromOtherSessionWindowBox.person_recuperate.Hide();
		PersonsRecuperateFromOtherSessionWindowBox = null;
	}
	
	protected override void on_person_recuperate_delete_event (object o, DeleteEventArgs args)
	{
		PersonsRecuperateFromOtherSessionWindowBox.person_recuperate.Hide();
		PersonsRecuperateFromOtherSessionWindowBox = null;
	}
	
	private void buttonRecuperateChangeSensitiveness() 
	{
		bool rowChecked = false;
		Gtk.TreeIter iter;
		if (store.GetIterFirst(out iter)) {
			do 
				rowChecked = (bool) store.GetValue (iter, 0);
			while ( !rowChecked && store.IterNext(ref iter));
		}
		if(rowChecked)
			button_recuperate.Sensitive = true;
		else
			button_recuperate.Sensitive = false;
	}

	
	protected override void on_row_double_clicked (object o, Gtk.RowActivatedArgs args) {
		//don't do nothing
	}

	int inserted; //just to say how much rows are inserted

	protected override void on_button_recuperate_clicked (object o, EventArgs args)
	{
		inserted = 0;
		processRows();
	}

	private void processRows()
	{
		Gtk.TreeIter iter;
		bool val;
		
		List <PersonSession> personSessions = new List<PersonSession>();
		int psID;
		int countPersonSessions = Sqlite.Count(Constants.PersonSessionTable, false);
		if(countPersonSessions == 0)
			psID = 1;
		else {
			//Sqlite.Max will return NULL if there are no values, for this reason we use the Sqlite.Count before
			int maxPSUniqueID = Sqlite.Max(Constants.PersonSessionTable, "uniqueID", false);
			psID = maxPSUniqueID + 1;
		}

		if (store.GetIterFirst(out iter)) 
		{
			Sqlite.Open();
			do {
				val = (bool) store.GetValue (iter, 0);
				//if checkbox of person is true
				if(val) {
					currentPerson = SqlitePerson.Select(true, Convert.ToInt32(treeview_person_recuperate.Model.GetValue(iter, 1)) );
					PersonSession currentPersonSession = SqlitePersonSession.Select(
							true, currentPerson.UniqueID, -1); //if sessionID == -1 search data in last sessionID
					personSessions.Add(new PersonSession(
								psID ++, currentPerson.UniqueID, currentSession.UniqueID, 
								currentPersonSession.Height, currentPersonSession.Weight, currentPersonSession.SportID, 
								currentPersonSession.SpeciallityID, currentPersonSession.Practice, currentPersonSession.Comments)
							);

					inserted ++;

				}
			} while ( store.IterNext(ref iter) );
		
			Sqlite.Close();

			//do the transaction	
			new SqlitePersonSessionTransaction(personSessions);
		}
			
		updateStoreAndEnd();
	}

	private void updateStoreAndEnd()
	{
		//update the treeview (only one time)
		string myText = UtilGtk.ComboGetActive(combo_sessions);
		if(myText != "") {
			store = new TreeStore( typeof (bool), 
					typeof (string), typeof (string), typeof (string), typeof (string), typeof (string) );
			treeview_person_recuperate.Model = store;

			string [] myStringFull = myText.Split(new char[] {':'});

			//fill the treeview passing the uniqueID of selected session as the reference for loading persons
			fillTreeView( treeview_person_recuperate, store, 
					currentSession.UniqueID, //except current session
					Convert.ToInt32(myStringFull[0]) //select from this session (on combo_sessions)
				    );

			if(inserted >= 1) {
				if(inserted == 1)
					statusbar1.Push( 1, Catalog.GetString("Loaded") + " " + currentPerson.Name );
				else 
					statusbar1.Push( 1, string.Format(Catalog.GetPluralString(
									"Successfully added one person.",
									"Successfully added {0} persons.",
									inserted),
								inserted));
				fakeButtonDone.Click();
			}
		}
	
		//check if there are rows checked for having sensitive or not in recuperate button
		buttonRecuperateChangeSensitiveness();
	}

}


//discard people to being uploadd to server
//inherits from PersonRecuperateFromOtherSession because uses same window
public class PersonNotUploadWindow : PersonsRecuperateFromOtherSessionWindow 
{
	static PersonNotUploadWindow PersonNotUploadWindowBox;
	ArrayList initiallyUnchecked;
	
	[Widget] Gtk.Label label_top;
	[Widget] Gtk.Button button_go_forward;
	[Widget] Gtk.Button button_close;

	private int sessionID;
	public new Gtk.Button fakeButtonDone;
	
	PersonNotUploadWindow (Gtk.Window parent, int sessionID) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "person_recuperate.glade", "person_recuperate", null);
		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(person_recuperate);

		//this class doesn't allow to search by name
		hbox_search_filter_hide.Hide();
		//this class doesn't use button recuperate
		button_recuperate.Hide();
		//this class doesn't use status bar
		statusbar1.Hide();
		//it's clearer to use go_forward instead of close
		button_go_forward.Show();
		button_close.Hide();

		person_recuperate.Title = Catalog.GetString("Include / Discard athletes");
		//person_recuperate.Title = Catalog.GetString("Incluir / Descartar atletas");
		
		fakeButtonDone = new Gtk.Button();
		
		this.sessionID = sessionID;
	
		firstColumn = 1;
			
		label_top.Text = Catalog.GetString("All persons checked at first column will be uploaded into database with his/her tests on this session.\nIf you want that a person is not uploaded, just uncheck it.");
		//label_top.Text = Catalog.GetString("Las personas marcadas en la primera columna serán subidas al servidor junto con sus tests en esta sesión.\nSi no desea subir los datos de una persona, desmarque su casilla en la primera columna.");
	
		hbox_from_session_hide.Hide(); //used in person recuperate multiple (hided in current class)
		createComboSelectCheckboxes();
		createCheckboxes(treeview_person_recuperate);
		
		store = new TreeStore( typeof (bool), 
				typeof (string), typeof (string), typeof (string), typeof (string), typeof (string) );
		createTreeView(treeview_person_recuperate, 1);
		treeview_person_recuperate.Model = store;
		
		initiallyUnchecked = SqlitePersonSessionNotUpload.SelectAll(sessionID);
		if(initiallyUnchecked.Count > 0)
			combo_select_checkboxes.Active = 2; //SELECTED
		else
			combo_select_checkboxes.Active = 0; //ALL

		fillTreeView( treeview_person_recuperate, store, 
				sessionID, //select from this session
				initiallyUnchecked
			    );

		treeview_person_recuperate.Selection.Changed += onSelectionEntry;
	}

	static public PersonNotUploadWindow Show (Gtk.Window parent, int sessionID)
	{
		if (PersonNotUploadWindowBox == null) {
			PersonNotUploadWindowBox = 
				new PersonNotUploadWindow (parent, sessionID);
		}
		PersonNotUploadWindowBox.person_recuperate.Show ();
		
		return PersonNotUploadWindowBox;
	}
	
	private void fillTreeView (Gtk.TreeView tv, TreeStore store, int sessionID, ArrayList initiallyUnchecked ) 
	{
		/*
		   this is a bit weird because we use Sqlite.SelectAllPersonsRecuperable as inherithed methods 
		   that slq method needs a session where we want to search and a session not to search (current session)
		   now here is different, we want to select persons from this session.
		   we continue using method SelectAllPersonsRecuperable because we want same output columns
		   */

		ArrayList myPersons = SqlitePerson.SelectAllPersonsRecuperable("name", -1, sessionID, ""); //"" is searchFilterName (not implemented on recuperate multiple)

		foreach (Person person in myPersons) {
			store.AppendValues (
					! Util.FoundInArrayList(initiallyUnchecked, person.UniqueID.ToString()), 
					person.UniqueID.ToString(), 
					person.Name, 
					getCorrectSex(person.Sex), 
					person.DateBorn.ToShortDateString(), 
					person.Description);
		}	
		
		//show sorted by column Name	
		store.SetSortColumnId(2, Gtk.SortType.Ascending);
	}

	
	protected override void on_button_close_clicked (object o, EventArgs args)
	{
		int personID;
		
		bool bannedToUploadBefore; 
		/*
		   bannedUploadBefore doesn't means that this person has not been upload before,
		   it means that a person has been added to personNotUpload table in the possible previous upload of that session
		   (remember a session can be uploaded more than one time)
		   */

		bool uploadNow;
		
		Gtk.TreeIter iter;
		bool okIter = store.GetIterFirst(out iter);
		if(okIter) {
			do {
				personID = Convert.ToInt32(store.GetValue (iter, 1));
				bannedToUploadBefore = Util.FoundInArrayList(initiallyUnchecked, personID.ToString());
				uploadNow = (bool) store.GetValue (iter, 0);

				/*
				   if a person is bannedToUploadBefore, means that 
				   in previous upload of the same session, this person has been added to personNotUpload table
				   then: 
				   - if bannedToUploadBefore and have to uploadNow, delete row on personNotUpload
				   - if bannedToUploadBefore and NOT have to uploadNow, nothing to be done
				   - if NOT bannedToUploadBefore and have to uploadNow, nothing to be done
				   - if NOT bannedToUploadBefore and NOT have to uploadNow, a row on personNotUpload should be added
				 */

				if(bannedToUploadBefore && uploadNow) 
					SqlitePersonSessionNotUpload.Delete(personID, sessionID);
				else if (! bannedToUploadBefore && ! uploadNow) 
					SqlitePersonSessionNotUpload.Add(personID, sessionID);
			} while ( store.IterNext(ref iter) );
		}

		fakeButtonDone.Click();

		PersonNotUploadWindowBox.person_recuperate.Hide();
		PersonNotUploadWindowBox = null;
	}
	
	protected override void on_person_recuperate_delete_event (object o, DeleteEventArgs args)
	{
		on_button_close_clicked (o, new EventArgs());
	}
	
	public new Button FakeButtonDone 
	{
		set { fakeButtonDone = value; }
		get { return fakeButtonDone; }
	}

}

public class PersonAddModifyWindow
{
	
	[Widget] Gtk.Window person_win;
	[Widget] Gtk.Entry entry1;
	[Widget] Gtk.RadioButton radiobutton_man;
	[Widget] Gtk.RadioButton radiobutton_woman;
	[Widget] Gtk.TextView textview_description;
	[Widget] Gtk.TextView textview_ps_comments;
	
	[Widget] Gtk.Box vbox_photo;
	
	[Widget] Gtk.Label label_date;
	//[Widget] Gtk.Button button_change_date;
	[Widget] Gtk.Image image_calendar;

	[Widget] Gtk.SpinButton spinbutton_height;
	[Widget] Gtk.SpinButton spinbutton_weight;
	
	[Widget] Gtk.Box hbox_combo_sports;
	[Widget] Gtk.ComboBox combo_sports;
	[Widget] Gtk.Label label_speciallity;
	[Widget] Gtk.Box hbox_combo_speciallities;
	[Widget] Gtk.ComboBox combo_speciallities;
	[Widget] Gtk.Box hbox_combo_levels;
	[Widget] Gtk.ComboBox combo_levels;
	
	[Widget] Gtk.Box hbox_combo_continents;
	[Widget] Gtk.Box hbox_combo_countries;
	[Widget] Gtk.ComboBox combo_continents;
	[Widget] Gtk.ComboBox combo_countries;
	
	[Widget] Gtk.Image image_name;
	[Widget] Gtk.Image image_weight;
	
	[Widget] Gtk.Button button_zoom;
	[Widget] Gtk.Image image_photo_mini;
	[Widget] Gtk.Image image_zoom;

	[Widget] Gtk.Button button_accept;

	//used for connect ok gui/chronojump.cs, PersonRecuperate, PersonRecuperateFromOtherSession,this class, gui/convertWeight.cs
	public Gtk.Button fakeButtonAccept;
	
	static ConvertWeightWindow convertWeightWin;
	
	static PersonAddModifyWindow PersonAddModifyWindowBox;
	
	DialogCalendar myDialogCalendar;
	DateTime dateTime;
	Sport sport;
	string [] sports;
	string [] sportsTranslated;
	string [] speciallities;
	string [] speciallitiesTranslated;
	//String level;
	string [] levels;
	string [] continents;
	string [] continentsTranslated;
	string [] countries;
	string [] countriesTranslated;

	GenericWindow genericWin;

	bool adding;

	private Person currentPerson;
	private Session currentSession;
	private PersonSession currentPersonSession;
	private string sex = Constants.M;
	private double weightIni;
	int pDN;
	Gtk.CheckButton app1_checkbutton_video;
	
	private int serverUniqueID;

	//
	//if we are adding a person, currentPerson.UniqueID it's -1
	//if we are modifying a person, currentPerson.UniqueID is obviously it's ID
	//showPhotoStuff is false on raspberry to not use camera
	PersonAddModifyWindow (Gtk.Window parent, Session currentSession, Person currentPerson, bool showPhotoStuff) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "person_win.glade", "person_win", null);
		gladeXML.Autoconnect(this);
		
		//put an icon to window
		UtilGtk.IconWindow(person_win);
	
		person_win.Parent = parent;
		this.currentSession = currentSession;
		this.currentPerson = currentPerson;

		if(currentPerson.UniqueID == -1)
			adding = true;
		else
			adding = false;
		
		createComboSports();
		createComboSpeciallities(-1);
		label_speciallity.Hide();
		combo_speciallities.Hide();
		createComboLevels();
		createComboContinents();
		createComboCountries();
		
		Pixbuf pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "calendar.png"); //from asssembly
		image_calendar.Pixbuf = pixbuf;

		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameZoomInIcon);
		image_zoom.Pixbuf = pixbuf;

		if(showPhotoStuff) {
			string photoFile = Util.GetPhotoFileName(true, currentPerson.UniqueID);
			if(File.Exists(photoFile)) {
				try {
					pixbuf = new Pixbuf (photoFile); //from a file
					image_photo_mini.Pixbuf = pixbuf;
				} catch {
					//on windows there are problem using the fileNames that are not on temp
					string tempFileName = Path.Combine(Path.GetTempPath(), Constants.PhotoSmallTemp +
							Util.GetMultimediaExtension(Constants.MultimediaItems.PHOTO));
					File.Copy(photoFile, tempFileName, true);
					pixbuf = new Pixbuf (tempFileName);
					image_photo_mini.Pixbuf = pixbuf;
				}
			}
			//show zoom button only if big image exists
			if(File.Exists(Util.GetPhotoFileName(false, currentPerson.UniqueID)))
				button_zoom.Sensitive = true;
			else
				button_zoom.Sensitive = false;
		}
		else
			vbox_photo.Visible = false;
			
		fakeButtonAccept = new Gtk.Button();

		entry1.CanFocus = true;
		entry1.IsFocus = true;

		if(adding) {
			person_win.Title = Catalog.GetString ("New jumper");
			//button_accept.Sensitive = false;
		} else 
			person_win.Title = Catalog.GetString ("Edit jumper");
	}
	
	void on_button_zoom_clicked (object o, EventArgs args) {
		string tempFileName = Path.Combine(Path.GetTempPath(), Constants.PhotoTemp +
				Util.GetMultimediaExtension(Constants.MultimediaItems.PHOTO));
		if(! adding) {
			//on windows there are problem using the fileNames that are not on temp
			string fileName = Util.GetPhotoFileName(false, currentPerson.UniqueID);
			File.Copy(fileName, tempFileName, true);
		}

		new DialogImageTest(currentPerson.Name, tempFileName, DialogImageTest.ArchiveType.FILE);
	}

	Gtk.Window capturerWindow;
	CapturerBin capturer;
	void on_button_take_photo_clicked (object o, EventArgs args) 
	{
		List<LongoMatch.Video.Utils.Device> devices = LongoMatch.Video.Utils.Device.ListVideoDevices();
		if(devices.Count == 0) {
			new DialogMessage(Constants.MessageTypes.WARNING, Constants.CameraNotFound);
			return;
		}

		//deactivate camera to allow camera on edit person. videoOn will have same value to light checkbutton again later
		app1_checkbutton_video.Active = false;

		capturer = new CapturerBin();
		CapturePropertiesStruct s = new CapturePropertiesStruct();

		s.DeviceID = devices[0].ID;

		s.CaptureSourceType = CaptureSourceType.System;

		capturer.CaptureProperties = s;
		capturer.Type = CapturerType.Snapshot;
		capturer.Visible=true;
		capturer.NewSnapshot += on_snapshot_done;
		capturer.NewSnapshotMini += on_snapshot_mini_done;

		capturerWindow = new Gtk.Window("Capturer");
		capturerWindow.Add(capturer);
		capturerWindow.Modal=true;
		capturerWindow.SetDefaultSize(400,400);

		person_win.Hide();

		capturerWindow.ShowAll();
		capturerWindow.Present();
		capturerWindow.DeleteEvent += delegate(object sender, DeleteEventArgs e) {capturer.Close(); capturer.Dispose(); person_win.Show(); };
		
		capturer.Run();
	}

	private void on_snapshot_done(Pixbuf pixbuf) {
		string fileName = Path.Combine(Path.GetTempPath(), Constants.PhotoTemp +
				Util.GetMultimediaExtension(Constants.MultimediaItems.PHOTO));
		
		pixbuf.Save(fileName,"jpeg");
		
		//on windows there are problem using the fileNames that are not on temp
		if(!adding)
			File.Copy(fileName, Util.GetPhotoFileName(false, currentPerson.UniqueID), true); //overwrite

		button_zoom.Sensitive = true;
	}

	private void on_snapshot_mini_done(Pixbuf pixbuf) {
		string tempSmallFileName = Path.Combine(Path.GetTempPath(), Constants.PhotoSmallTemp +
				Util.GetMultimediaExtension(Constants.MultimediaItems.PHOTO));
		
		pixbuf.Save(tempSmallFileName,"jpeg");
		
		//on windows there are problem using the fileNames that are not on temp
		if(!adding)
			File.Copy(tempSmallFileName, Util.GetPhotoFileName(true, currentPerson.UniqueID), true); //overwrite
		
		capturer.Close();
		capturer.Dispose();
		capturerWindow.Hide();

		person_win.Show();


		string tempFileName = Path.Combine(Path.GetTempPath(), Constants.PhotoSmallTemp +
			Util.GetMultimediaExtension(Constants.MultimediaItems.PHOTO));
		if(!adding) {
			//on windows there are problem using the fileNames that are not on temp
			string fileName = Util.GetPhotoFileName(true, currentPerson.UniqueID);
			File.Copy(fileName, tempFileName, true);
		}
		
		if(File.Exists(tempFileName)) {
			Pixbuf pixbuf2 = new Pixbuf (tempFileName); //from a file
			image_photo_mini.Pixbuf = pixbuf2;
		}
	}

	void on_entries_required_changed (object o, EventArgs args)
	{
		if(entry1.Text.ToString().Length > 0)
			image_name.Hide();
		else {
			image_name.Show();
		}

		if((double) spinbutton_weight.Value > 0)
			image_weight.Hide();
		else {
			image_weight.Show();
		}
	
		/*		
		if(dateTime != DateTime.MinValue)
			image_date.Hide();
		else {
			image_date.Show();
			allOk = false;
		}
		*/

		//countries is not required to create a person here, but will be required for server
		//&& 
		//UtilGtk.ComboGetActive(combo_continents) != Catalog.GetString(Constants.ContinentUndefined) &&
		//UtilGtk.ComboGetActive(combo_countries) != Catalog.GetString(Constants.CountryUndefined)
			
		/*
		if(allOk)
			button_accept.Sensitive = true;
		else
			button_accept.Sensitive = false;
		*/
		/*
		Always true because there's problems detecting the spinbutton change (when inserting data directly on entry)
		and there's an error message after if there's missing data	
		*/
		button_accept.Sensitive = true;
	}
		
	void on_radiobutton_man_toggled (object o, EventArgs args)
	{
		sex = Constants.M;
	}
	
	void on_radiobutton_woman_toggled (object o, EventArgs args)
	{
		sex = Constants.F;
	}
	
	static public PersonAddModifyWindow Show (Gtk.Window parent, 
			Session mySession, Person currentPerson, int pDN, 
			Gtk.CheckButton app1_checkbutton_video, bool showPhotoStuff)
	{
		if (PersonAddModifyWindowBox == null) {
			PersonAddModifyWindowBox = new PersonAddModifyWindow (parent, mySession, currentPerson, showPhotoStuff);
		}

		PersonAddModifyWindowBox.pDN = pDN;
		PersonAddModifyWindowBox.app1_checkbutton_video = app1_checkbutton_video;

		PersonAddModifyWindowBox.person_win.Show ();

		PersonAddModifyWindowBox.fillDialog ();
		
		return PersonAddModifyWindowBox;
	}
	
	static public void MakeVisible () {
		PersonAddModifyWindowBox.person_win.Show();
	}


	private void createComboSports() {
		combo_sports = ComboBox.NewText ();
		sports = SqliteSport.SelectAll();
			
		//create sports translated, only with translated stuff
		sportsTranslated = new String[sports.Length];
		int i = 0;
		foreach(string row in sports) {
			string [] myStrFull = row.Split(new char[] {':'});
			sportsTranslated[i++] = myStrFull[2];
			}
		
		//sort array (except second row)
		System.Array.Sort(sportsTranslated, 2, sportsTranslated.Length-2);
		
		UtilGtk.ComboUpdate(combo_sports, sportsTranslated, "");
		combo_sports.Active = UtilGtk.ComboMakeActive(sportsTranslated, 
				Catalog.GetString(Constants.SportUndefined));
	
		combo_sports.Changed += new EventHandler (on_combo_sports_changed);

		hbox_combo_sports.PackStart(combo_sports, true, true, 0);
		hbox_combo_sports.ShowAll();
		combo_sports.Sensitive = true;
	}
	
	private void createComboSpeciallities(int sportID) {
		combo_speciallities = ComboBox.NewText ();
		speciallities = SqliteSpeciallity.SelectAll(true, sportID); //show undefined, filter by sport
		
		//create speciallities translated, only with translated stuff
		speciallitiesTranslated = new String[speciallities.Length];
		int i = 0;
		foreach(string row in speciallities) {
			string [] myStrFull = row.Split(new char[] {':'});
			speciallitiesTranslated[i++] = myStrFull[2];
			}
		
		//sort array (except first row)
		System.Array.Sort(speciallities, 1, speciallities.Length-1);

		UtilGtk.ComboUpdate(combo_speciallities, speciallitiesTranslated, "");
		combo_speciallities.Active = UtilGtk.ComboMakeActive(speciallitiesTranslated, 
				Catalog.GetString(Constants.SpeciallityUndefined));

		combo_speciallities.Changed += new EventHandler (on_combo_speciallities_changed);

		hbox_combo_speciallities.PackStart(combo_speciallities, true, true, 0);
		hbox_combo_speciallities.ShowAll();
		combo_speciallities.Sensitive = true;
	}
	
	private void createComboLevels() {
		combo_levels = ComboBox.NewText ();
		levels = Constants.Levels;
		
		UtilGtk.ComboUpdate(combo_levels, levels, "");
		combo_levels.Active = UtilGtk.ComboMakeActive(levels, 
				Constants.LevelUndefinedID.ToString() + ":" + 
				Catalog.GetString(Constants.LevelUndefined));

		combo_levels.Changed += new EventHandler (on_combo_levels_changed);

		hbox_combo_levels.PackStart(combo_levels, true, true, 0);
		hbox_combo_levels.ShowAll();
		combo_levels.Sensitive = false; //level is shown when sport is not "undefined" and not "none"
	}
		
	private void createComboContinents() {
		combo_continents = ComboBox.NewText ();
		continents = Constants.Continents;

		//create continentsTranslated, only with translated stuff
		continentsTranslated = new String[Constants.Continents.Length];
		int i = 0;
		foreach(string continent in continents) 
			continentsTranslated[i++] = Util.FetchName(continent);

		UtilGtk.ComboUpdate(combo_continents, continentsTranslated, "");
		combo_continents.Active = UtilGtk.ComboMakeActive(continentsTranslated, 
				Catalog.GetString(Constants.ContinentUndefined));

		combo_continents.Changed += new EventHandler (on_combo_continents_changed);

		hbox_combo_continents.PackStart(combo_continents, true, true, 0);
		hbox_combo_continents.ShowAll();
		combo_continents.Sensitive = true;
	}

	private void createComboCountries() {
		combo_countries = ComboBox.NewText ();

		countries = new String[1];
		//record countries with id:english name:translatedName
		countries [0] = Constants.CountryUndefinedID + ":" + Constants.CountryUndefined + ":" + Catalog.GetString(Constants.CountryUndefined);

		string [] myCountries = new String[1];
		myCountries [0] = Catalog.GetString(Constants.CountryUndefined);
		UtilGtk.ComboUpdate(combo_countries, myCountries, "");
		combo_countries.Active = UtilGtk.ComboMakeActive(myCountries, 
				Catalog.GetString(Constants.CountryUndefined));
		
		//create countriesTranslated, only with translated stuff
		countriesTranslated = new String[1];

		
		combo_countries.Changed += new EventHandler (on_combo_countries_changed);

		hbox_combo_countries.PackStart(combo_countries, true, true, 0);
		hbox_combo_countries.ShowAll();
		combo_countries.Sensitive = false;
	}


	private void fillDialog ()
	{
		int mySportID;
		int mySpeciallityID;
		int myLevelID;
		if(adding) {
			//now dateTime is undefined until user changes it
			dateTime = DateTime.MinValue;
			label_date.Text = Catalog.GetString("Undefined");

			mySportID = currentSession.PersonsSportID;
			mySpeciallityID = currentSession.PersonsSpeciallityID;
			myLevelID = currentSession.PersonsPractice;
		} else {
			//PERSON STUFF
			entry1.Text = currentPerson.Name;
			if (currentPerson.Sex == Constants.M) {
				radiobutton_man.Active = true;
			} else {
				radiobutton_woman.Active = true;
			}

			dateTime = currentPerson.DateBorn;
			if(dateTime == DateTime.MinValue)
				label_date.Text = Catalog.GetString("Undefined");
			else
				label_date.Text = dateTime.ToLongDateString();

			//country stuff
			if(currentPerson.CountryID != Constants.CountryUndefinedID) {
				string [] countryString = SqliteCountry.Select(currentPerson.CountryID);
			
				combo_continents.Active = UtilGtk.ComboMakeActive(continentsTranslated, 
						Catalog.GetString(countryString[3]));
				
				combo_countries.Active = UtilGtk.ComboMakeActive(countriesTranslated, 
						Catalog.GetString(countryString[1]));
			}

			TextBuffer tb1 = new TextBuffer (new TextTagTable());
			tb1.Text = currentPerson.Description;
			textview_description.Buffer = tb1;
			
			serverUniqueID = currentPerson.ServerUniqueID;
			

			//PERSONSESSION STUFF
			PersonSession myPS = SqlitePersonSession.Select(currentPerson.UniqueID, currentSession.UniqueID);

			spinbutton_height.Value = myPS.Height;
			spinbutton_weight.Value = myPS.Weight;

			weightIni = myPS.Weight; //store for tracking if changes
		
			mySportID = myPS.SportID;
			mySpeciallityID = myPS.SpeciallityID;
			myLevelID = myPS.Practice;

			TextBuffer tb2 = new TextBuffer (new TextTagTable());
			tb2.Text = myPS.Comments;
			textview_ps_comments.Buffer = tb2;
		}
			
		sport = SqliteSport.Select(false, mySportID);
		combo_sports.Active = UtilGtk.ComboMakeActive(sportsTranslated, sport.ToString());

		combo_speciallities.Active = UtilGtk.ComboMakeActive(speciallitiesTranslated, SqliteSpeciallity.Select(false, mySpeciallityID));

		combo_levels.Active = UtilGtk.ComboMakeActive(levels, myLevelID + ":" + Util.FindLevelName(myLevelID));
		
	}
		
	
	void on_button_calendar_clicked (object o, EventArgs args)
	{
		DateTime dt = dateTime;
		if(dt == DateTime.MinValue)
			dt = DateTime.Now;
		myDialogCalendar = new DialogCalendar(Catalog.GetString("Select session date"), dt);
		myDialogCalendar.FakeButtonDateChanged.Clicked += new EventHandler(on_calendar_changed);
	}

	void on_calendar_changed (object obj, EventArgs args)
	{
		dateTime = myDialogCalendar.MyDateTime;
		label_date.Text = dateTime.ToLongDateString();
		on_entries_required_changed(new object(), new EventArgs());
	}
	
	void on_button_height_metric_clicked(object obj, EventArgs args) 
	{
		genericWin = GenericWindow.Show(Catalog.GetString("Select your height"), Constants.GenericWindowShow.HEIGHTMETRIC);
		genericWin.Button_accept.Clicked += new EventHandler(on_button_height_metric_accepted);
	}
	void on_button_height_metric_accepted (object obj, EventArgs args)
	{
		genericWin.Button_accept.Clicked -= new EventHandler(on_button_height_metric_accepted);

		string [] myStr = genericWin.TwoSpinSelected.Split(new char[] {':'});
		spinbutton_height.Value = Util.ConvertFeetInchesToCm(
			Convert.ToInt32(myStr[0]), 
			Convert.ToDouble(myStr[1])
		);
	}
	
	void on_button_weight_metric_clicked(object obj, EventArgs args) 
	{
		genericWin = GenericWindow.Show(Catalog.GetString("Select your weight in pounds"), Constants.GenericWindowShow.SPINDOUBLE);
		genericWin.Button_accept.Clicked += new EventHandler(on_button_weight_metric_accepted);
	}
	void on_button_weight_metric_accepted (object obj, EventArgs args)
	{
		genericWin.Button_accept.Clicked -= new EventHandler(on_button_weight_metric_accepted);

		spinbutton_weight.Value = Util.ConvertPoundsToKg(genericWin.SpinDoubleSelected);
	}


	private void on_combo_sports_changed(object o, EventArgs args) {
		if (o == null)
			return;

		//LogB.Information("changed");
		try {
			int sportID = Convert.ToInt32(Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(combo_sports), sports));
			sport = SqliteSport.Select(false, sportID);

			if(Catalog.GetString(sport.Name) == Catalog.GetString(Constants.SportUndefined)) {
				//if sport is undefined, level should be undefined, and unsensitive
				try { 
					combo_levels.Active = UtilGtk.ComboMakeActive(levels, 
							Constants.LevelUndefinedID.ToString() + ":" + 
							Catalog.GetString(Constants.LevelUndefined));
					combo_levels.Sensitive = false;
					combo_speciallities.Active = UtilGtk.ComboMakeActive(speciallitiesTranslated, 
							Catalog.GetString(Constants.SpeciallityUndefined));
					label_speciallity.Hide();
					combo_speciallities.Hide();
				}
				catch { LogB.Warning("do later"); }
			} else if(Catalog.GetString(sport.Name) == Catalog.GetString(Constants.SportNone)) {
				//if sport is none, level should be sedentary and unsensitive
				try { 
					combo_levels.Active = UtilGtk.ComboMakeActive(levels, 
							Constants.LevelSedentaryID.ToString() + ":" + 
							Catalog.GetString(Constants.LevelSedentary));
					combo_levels.Sensitive = false;

					combo_speciallities.Active = UtilGtk.ComboMakeActive(speciallitiesTranslated, 
							Catalog.GetString(Constants.SpeciallityUndefined));

					label_speciallity.Hide();
					combo_speciallities.Hide();
				}
				catch { LogB.Warning("do later"); }
			} else {
				//sport is not undefined and not none

				//if level is "sedentary", then change level to "undefined"
				if(UtilGtk.ComboGetActive(combo_levels) ==
						Constants.LevelSedentaryID.ToString() + ":" + 
					       	Catalog.GetString(Constants.LevelSedentary)) {
					combo_levels.Active = UtilGtk.ComboMakeActive(levels,
							Constants.LevelUndefinedID.ToString() + ":" + 
						       	Catalog.GetString(Constants.LevelUndefined));
				}

				//show level
				combo_levels.Sensitive = true;
		
				if(sport.HasSpeciallities) {
					combo_speciallities.Destroy();
					createComboSpeciallities(sport.UniqueID);
					label_speciallity.Show();
					combo_speciallities.Show();
				} else {
					LogB.Information("hide");
					combo_speciallities.Active = UtilGtk.ComboMakeActive(speciallitiesTranslated,
						       	Catalog.GetString(Constants.SpeciallityUndefined));
					label_speciallity.Hide();
					combo_speciallities.Hide();
				}
			}
		} catch { 
			//LogB.Warning("do later");
		}

		on_entries_required_changed(new object(), new EventArgs());
		LogB.Information(sport.ToString());
	}
	
	private void on_combo_speciallities_changed(object o, EventArgs args) {
		LogB.Information("changed speciallities");
		on_entries_required_changed(new object(), new EventArgs());
	}

	private void on_combo_levels_changed(object o, EventArgs args) {
		//string myText = UtilGtk.ComboGetActive(combo_sports);
		LogB.Information("changed levels");
		on_entries_required_changed(new object(), new EventArgs());
		//level = UtilGtk.ComboGetActive(combo_levels);
				
		//if it's sedentary, put sport to none
		/*
		 * Now undone because sedentary has renamed to "sedentary/Occasional practice"
		if(UtilGtk.ComboGetActive(combo_levels) == "0:" + Catalog.GetString(Constants.LevelSedentary))
			combo_sports.Active = UtilGtk.ComboMakeActive(sports, "2:" + Catalog.GetString(Constants.SportNone));
		*/
	}
	
	private void on_combo_continents_changed(object o, EventArgs args) {
		//LogB.Information("Changed");

		if(UtilGtk.ComboGetActive(combo_continents) == Catalog.GetString(Constants.ContinentUndefined)) {
			countries [0] = Constants.CountryUndefinedID + ":" + 
				Constants.CountryUndefined + ":" + Catalog.GetString(Constants.CountryUndefined);
			countriesTranslated = new String[1];
			countriesTranslated [0] = Catalog.GetString(Constants.CountryUndefined);
			combo_countries.Sensitive = false;
		}
		else {
			//get the active continent
			string continentEnglish = Util.FindOnArray(':', 1, 0, UtilGtk.ComboGetActive(combo_continents), continents); 
			countries = SqliteCountry.SelectCountriesOfAContinent(continentEnglish, true); //put undefined first

			//create countries translated, only with translated stuff
			countriesTranslated = new String[countries.Length];
			int i = 0;
			foreach(string row in countries) {
				string [] myStrFull = row.Split(new char[] {':'});
				countriesTranslated[i++] = myStrFull[2];
			}
		}
		//sort array (except first row)
		System.Array.Sort(countriesTranslated, 1, countriesTranslated.Length-1);

		UtilGtk.ComboUpdate(combo_countries, countriesTranslated, "");
		combo_countries.Active = UtilGtk.ComboMakeActive(countriesTranslated, 
				Catalog.GetString(Constants.CountryUndefined));

		combo_countries.Sensitive = true;

		on_entries_required_changed(new object(), new EventArgs());
	}
	
	private void on_combo_countries_changed(object o, EventArgs args) {
		//define country is not needed to accept person
		//on_entries_required_changed(new object(), new EventArgs());
	}
	
	void on_button_sport_add_clicked (object o, EventArgs args)
	{
		LogB.Information("sport add clicked");
		genericWin = GenericWindow.Show(Catalog.GetString("Add new sport to database"), Constants.GenericWindowShow.ENTRY);
		genericWin.Button_accept.Clicked += new EventHandler(on_sport_add_accepted);
	}

	private void on_sport_add_accepted (object o, EventArgs args)
	{
		genericWin.Button_accept.Clicked -= new EventHandler(on_sport_add_accepted);

		string newSportName = genericWin.EntrySelected;
		if(Sqlite.Exists(false, Constants.SportTable, newSportName) ||
				newSportName == Catalog.GetString(Constants.SportUndefined) || //let's save problems
				newSportName == Catalog.GetString(Constants.SportNone)		//let's save problems
				)
				new DialogMessage(Constants.MessageTypes.WARNING, string.Format(
							Catalog.GetString("Sorry, this sport '{0}' already exists in database"), 
							newSportName));
		else {
			int myID = SqliteSport.Insert(false, "-1", newSportName, true, //dbconOpened, , userDefined
					false, "");	//hasSpeciallities, graphLink 

			Sport mySport = new Sport(myID, newSportName, true, 
					false, "");	//hasSpeciallities, graphLink 
			sports = SqliteSport.SelectAll();
			//create sports translated, only with translated stuff
			sportsTranslated = new String[sports.Length];
			int i = 0;
			foreach(string row in sports) {
				string [] myStrFull = row.Split(new char[] {':'});
				sportsTranslated[i++] = myStrFull[2];
				}
		
			//sort array (except second row)
			System.Array.Sort(sportsTranslated, 2, sportsTranslated.Length-2);

			UtilGtk.ComboUpdate(combo_sports, sportsTranslated, mySport.ToString());
			combo_sports.Active = UtilGtk.ComboMakeActive(sportsTranslated, mySport.ToString());
			//on_combo_sports_changed(combo_sports, new EventArgs());
		}
	}
		
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		string errorMessage = "";

		//Check if person name exists and weight is > 0
		string personName = entry1.Text;
		if(personName == "")
			errorMessage += "\n" + Catalog.GetString("Please, write the name of the person.");
		if((double) spinbutton_weight.Value == 0)
			errorMessage += "\n" + Catalog.GetString("Please, complete the weight of the person.");
		if(errorMessage.Length > 0) {
			ErrorWindow.Show(errorMessage);
			return;
		}


		bool personExists;
		if(adding)
			personExists = Sqlite.Exists (false, Constants.PersonTable, Util.RemoveTilde(personName));
		else
			personExists = SqlitePerson.ExistsAndItsNotMe (currentPerson.UniqueID, Util.RemoveTilde(personName));

		if(personExists) 
			errorMessage += string.Format(Catalog.GetString("Person: '{0}' exists. Please, use another name"), 
					Util.RemoveTildeAndColonAndDot(personName) );
		else {
			//if weight has changed
			if(!adding && (double) spinbutton_weight.Value != weightIni) {
				//see if this person has done jumps with weight
				string [] myJumpsNormal = SqliteJump.SelectJumps(false, currentSession.UniqueID, currentPerson.UniqueID, "withWeight", "",
						Sqlite.Orders_by.DEFAULT, -1);
				string [] myJumpsReactive = SqliteJumpRj.SelectJumps(false, currentSession.UniqueID, currentPerson.UniqueID, "withWeight", "");

				if(myJumpsNormal.Length > 0 || myJumpsReactive.Length > 0) {
					//create the convertWeight Window
					convertWeightWin = ConvertWeightWindow.Show(
							weightIni, (double) spinbutton_weight.Value, 
							myJumpsNormal, myJumpsReactive);
					convertWeightWin.Button_accept.Clicked += new EventHandler(on_convertWeightWin_accepted);
					convertWeightWin.Button_cancel.Clicked += new EventHandler(on_convertWeightWin_cancelled);
				} else 
					recordChanges();
				
			} else 
				recordChanges();
			
		}

		if(errorMessage.Length > 0)
			ErrorWindow.Show(errorMessage);
	}

	void on_convertWeightWin_accepted (object o, EventArgs args) {
		recordChanges();
	}

	void on_convertWeightWin_cancelled (object o, EventArgs args) {
		//do nothing (wait if user whants to cancel the personModify o change another thing)
	}

	private void recordChanges() {
		//separate by '/' for not confusing with the ':' separation between the other values
		//string dateFull = dateTime.Day.ToString() + "/" + dateTime.Month.ToString() + "/" +
		//	dateTime.Year.ToString();
		
		double weight = (double) spinbutton_weight.Value;

		//convert margarias (it's power is calculated using weight and it's written on description)
		string [] myMargarias = SqliteRun.SelectRuns(false, currentSession.UniqueID, currentPerson.UniqueID, "Margaria",
				Sqlite.Orders_by.DEFAULT, -1);

		foreach(string myStr in myMargarias) {
			string [] margaria = myStr.Split(new char[] {':'});
			Run mRun = SqliteRun.SelectRunData(Convert.ToInt32(margaria[1]), false);
			double distanceMeters = mRun.Distance / 1000;
			mRun.Description = "P = " + Util.TrimDecimals ( (weight * 9.8 * distanceMeters / mRun.Time).ToString(), pDN) + " (Watts)";
			SqliteRun.Update(mRun.UniqueID, mRun.Type, mRun.Distance.ToString(), mRun.Time.ToString(), mRun.PersonID, mRun.Description);
		}


		if(adding) {
			//here we add rows in the database
			LogB.Information("Going to insert person");
			currentPerson = new Person (entry1.Text, sex, dateTime, 
					Constants.RaceUndefinedID,
					Convert.ToInt32(Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(combo_countries), countries)),
					textview_description.Buffer.Text,
					Constants.ServerUndefinedID, false); //dbconOpened
					
			LogB.Information("Going to insert personSession");
			currentPersonSession = new PersonSession (
					currentPerson.UniqueID, currentSession.UniqueID, 
					(double) spinbutton_height.Value, (double) weight, 
					sport.UniqueID, 
					Convert.ToInt32(Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(combo_speciallities), speciallities)),
					Util.FetchID(UtilGtk.ComboGetActive(combo_levels)),
					textview_ps_comments.Buffer.Text, false); //dbconOpened
			LogB.Information("inserted both");
		} else {
			//here we update rows in the database
			currentPerson = new Person (currentPerson.UniqueID, entry1.Text, sex, dateTime, 
					Constants.RaceUndefinedID,
					Convert.ToInt32(Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(combo_countries), countries)),
					textview_description.Buffer.Text,
					serverUniqueID);
			SqlitePerson.Update (currentPerson); 
		
			//we only need to update personSession
			//1.- search uniqueID
			PersonSession ps = SqlitePersonSession.Select(currentPerson.UniqueID, currentSession.UniqueID);

			//2.- create new instance
			currentPersonSession = new PersonSession (
					ps.UniqueID,
					currentPerson.UniqueID, currentSession.UniqueID, 
					(double) spinbutton_height.Value, (double) weight, 
					sport.UniqueID, 
					Convert.ToInt32(Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(combo_speciallities), speciallities)),
					Util.FetchID(UtilGtk.ComboGetActive(combo_levels)),
					textview_ps_comments.Buffer.Text);

			//3.- update in database
			SqlitePersonSession.Update (currentPersonSession); 
		}


		PersonAddModifyWindowBox.person_win.Hide();
		PersonAddModifyWindowBox = null;
		
		fakeButtonAccept.Click();
	}
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		PersonAddModifyWindowBox.person_win.Hide();
		PersonAddModifyWindowBox = null;
	}
	
	//void on_person_modify_delete_event (object o, EventArgs args)
	void on_person_win_delete_event (object o, DeleteEventArgs args)
	{
			PersonAddModifyWindowBox.person_win.Hide();
			PersonAddModifyWindowBox = null;
	}
	
	

	public void Destroy() {
		//PersonAddModifyWindowBox.person_win.Destroy();
	}

	public Button FakeButtonAccept 
	{
		set { fakeButtonAccept = value; }
		get { return fakeButtonAccept; }
	}
	

	public Person CurrentPerson {
		get { return currentPerson; }
	}
	
	public PersonSession CurrentPersonSession {
		get { return currentPersonSession; }
	}
	
}

public class PersonAddMultipleTable {
	public string name;
	public bool maleOrFemale;
	public double weight;

	public PersonAddMultipleTable(string name, bool maleOrFemale, double weight) {
		this.name = name;
		this.maleOrFemale = maleOrFemale;
		this.weight = weight;
	}

	~PersonAddMultipleTable() {}
}

//new persons multiple (infinite)
public class PersonAddMultipleWindow {
	
	[Widget] Gtk.Window person_multiple_infinite;
		
	[Widget] Gtk.Notebook notebook;
	
	[Widget] Gtk.RadioButton radio_csv;
	[Widget] Gtk.RadioButton radio_manually;
	[Widget] Gtk.Box hbox_csv;
	[Widget] Gtk.Box hbox_manually;
	[Widget] Gtk.SpinButton spin_manually;
	
	[Widget] Gtk.Image image_csv_headers;
	[Widget] Gtk.Image image_csv_noheaders;
	[Widget] Gtk.Image image_csv_headers_help;
	[Widget] Gtk.Image image_csv_noheaders_help;
	
	[Widget] Gtk.Table table_headers_1_column;
	[Widget] Gtk.Table table_no_headers_1_column;
	[Widget] Gtk.Table table_headers_2_columns;
	[Widget] Gtk.Table table_no_headers_2_columns;
	
	[Widget] Gtk.Image image_name1;
	[Widget] Gtk.Image image_name2;
	[Widget] Gtk.Image image_name1_help;
	[Widget] Gtk.Image image_name2_help;
	
	[Widget] Gtk.CheckButton check_headers;
	[Widget] Gtk.CheckButton check_name_1_column;
	
	[Widget] Gtk.Label label_csv_help;

	//use this to read/write table
	ArrayList entries;
	ArrayList radiosM;
	ArrayList radiosF;
	ArrayList spins;
	
	int rows;
	bool created_table;
	
	[Widget] Gtk.ScrolledWindow scrolledwindow;
	[Widget] Gtk.Table table_main;
	[Widget] Gtk.Label label_message;
	
	[Widget] Gtk.Button button_accept;
	
	static PersonAddMultipleWindow PersonAddMultipleWindowBox;

	private Person currentPerson;
	Session currentSession;
	int personsCreatedCount;
	string errorExistsString;
	string errorWeightString;
	string errorRepeatedEntryString;
	string tableAlreadyCreatedString = Catalog.GetString("Table has already been created.");

	
	PersonAddMultipleWindow (Gtk.Window parent, Session currentSession) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "person_multiple_infinite.glade", "person_multiple_infinite", null);
		gladeXML.Autoconnect(this);
		
		//put an icon to window
		UtilGtk.IconWindow(person_multiple_infinite);
	
		person_multiple_infinite.Parent = parent;
		this.currentSession = currentSession;
	}
	
	static public PersonAddMultipleWindow Show (Gtk.Window parent, Session currentSession)
	{
		if (PersonAddMultipleWindowBox == null) {
			PersonAddMultipleWindowBox = new PersonAddMultipleWindow (parent, currentSession);
		}
		
		PersonAddMultipleWindowBox.putNonStandardIcons ();
		PersonAddMultipleWindowBox.tablesVisibility ();

		PersonAddMultipleWindowBox.created_table = false;

		PersonAddMultipleWindowBox.person_multiple_infinite.Show ();
		
		return PersonAddMultipleWindowBox;
	}
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		PersonAddMultipleWindowBox.person_multiple_infinite.Hide();
		PersonAddMultipleWindowBox = null;
	}
	
	void on_delete_event (object o, DeleteEventArgs args)
	{
		PersonAddMultipleWindowBox.person_multiple_infinite.Hide();
		PersonAddMultipleWindowBox = null;
	}
		
	void putNonStandardIcons() {
		Pixbuf pixbuf;
		
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameCSVHeadersIcon);
		image_csv_headers.Pixbuf = pixbuf;
		image_csv_headers_help.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameCSVNoHeadersIcon);
		image_csv_noheaders.Pixbuf = pixbuf;
		image_csv_noheaders_help.Pixbuf = pixbuf;
		
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameCSVName1Icon);
		image_name1.Pixbuf = pixbuf;
		image_name1_help.Pixbuf = pixbuf;
		pixbuf = new Pixbuf (null, Util.GetImagePath(false) + Constants.FileNameCSVName2Icon);
		image_name2.Pixbuf = pixbuf;
		image_name2_help.Pixbuf = pixbuf;
	}
	
	void tablesVisibility() {
		table_headers_1_column.Visible = false;
		table_no_headers_1_column.Visible = false;
		table_headers_2_columns.Visible = false;
		table_no_headers_2_columns.Visible = false;
		
		if(check_headers.Active) {
			if(check_name_1_column.Active)
				table_headers_1_column.Visible = true;
			else
				table_headers_2_columns.Visible = true;
		} else {
			if(check_name_1_column.Active)
				table_no_headers_1_column.Visible = true;
			else
				table_no_headers_2_columns.Visible = true;
		}
		
		button_accept.Sensitive = false;
	}

	void on_check_headers_toggled (object obj, EventArgs args) {
		image_csv_headers.Visible = (check_headers.Active == true);
		image_csv_noheaders.Visible = (check_headers.Active == false);

		tablesVisibility();
	}
	
	void on_check_name_1_column_toggled (object obj, EventArgs args) {
		image_name1.Visible = (check_name_1_column.Active == true);
		image_name2.Visible = (check_name_1_column.Active == false);
		
		tablesVisibility();
	}
	
	void on_radio_csv_toggled (object obj, EventArgs args) {
		if(radio_csv.Active) {
			hbox_csv.Sensitive = true;
			hbox_manually.Sensitive = false;
		}
	}
	void on_radio_manually_toggled (object obj, EventArgs args) {
		if(radio_manually.Active) {
			hbox_csv.Sensitive = false;
			hbox_manually.Sensitive = true;
		}
	}
		
	void on_button_csv_load_clicked (object obj, EventArgs args) 
	{
		if(created_table) {
			label_message.Text = tableAlreadyCreatedString;
			label_message.Visible = true;
			return;
		}

		Gtk.FileChooserDialog fc=
			new Gtk.FileChooserDialog(Catalog.GetString("Select CSV file"),
					null,
					FileChooserAction.Open,
					Catalog.GetString("Cancel"),ResponseType.Cancel,
					Catalog.GetString("Load"),ResponseType.Accept
					);

		fc.Filter = new FileFilter();
		fc.Filter.AddPattern("*.csv");
		fc.Filter.AddPattern("*.CSV");

		ArrayList array = new ArrayList();
		if (fc.Run() == (int)ResponseType.Accept) 
		{
			LogB.Warning("Opening CSV...");
			System.IO.FileStream file;
			try {
				file = System.IO.File.OpenRead(fc.Filename); 
			} catch {
				LogB.Warning("Catched, maybe is used by another program");
				new DialogMessage(Constants.MessageTypes.WARNING, 
						Catalog.GetString("Sorry, video cannot be stored.") + "\n\n" +
						Catalog.GetString("Maybe this file is opened by an SpreadSheet software like Excel. Please, close that program.")
						);
				fc.Destroy();
				return;
			}

			List<string> columns = new List<string>();
			using (var reader = new CsvFileReader(fc.Filename))
			{
				bool headersActive = check_headers.Active;
				bool name1Column = check_name_1_column.Active;
				int row = 0;
				while (reader.ReadRow(columns))
				{
					string fullname = "";
					string onlyname = "";
					bool maleOrFemale = true;
					double weight = 0;
					int col = 0;
					foreach(string str in columns) {
						//if headers are active do not process first row
						//do not process this first row because weight can be a string
						if(headersActive && row == 0)
							continue;
						
						LogB.Debug(":" + str);

						if(col == 0) {
							if(name1Column)
								fullname = str;
							else
								onlyname = str;
						}
						else if(col == 1 && ! name1Column)
							fullname = onlyname + " " + str;
						else if( (col == 1 && name1Column) || (col == 2 && ! name1Column) ) {
							//female symbols
							if(str == "0" || str == "f" || str == "F")
								maleOrFemale = false;
						}
						else if( (col == 2 && name1Column) || (col == 3 && ! name1Column) ) {
							try {
								weight = Convert.ToDouble(Util.ChangeDecimalSeparator(str));
							} catch {
								string message = Catalog.GetString("Error importing data.");
								if( ! check_headers.Active && row == 0)
									message += "\n" + Catalog.GetString("Seems there's a header row and you have not marked it.");

								new DialogMessage(Constants.MessageTypes.WARNING, message);

								file.Close(); 
								//Don't forget to call Destroy() or the FileChooserDialog window won't get closed.
								fc.Destroy();

								return;
							}
						}
						col ++;
					}
					//if headers are active do not add first row
					if( ! (headersActive && row == 0) ) {
						PersonAddMultipleTable pamt = new PersonAddMultipleTable(fullname, maleOrFemale, weight);
						array.Add(pamt);
					}
					
					row ++;
					LogB.Debug("\n");
				}
			}

			file.Close(); 

			rows = array.Count;
			createEmptyTable();
			fillTableFromCSV(array);
		} 

		//Don't forget to call Destroy() or the FileChooserDialog window won't get closed.
		fc.Destroy();
	}
	
	
	void on_button_csv_help_clicked (object obj, EventArgs args) 
	{
		label_csv_help.Text =
			"<b>" + Catalog.GetString("Import persons from an spreadsheet. Eg. Excel, LibreOffice, Google Drive.") + "</b>\n\n" +
			Catalog.GetString("Open the spreadsheet with the persons data to be added.") + "\n" +
			Catalog.GetString("Spreadsheet structure need to have this structure:");
		label_csv_help.UseMarkup = true;

		notebook.CurrentPage = 1;
	}
	
	void on_button_csv_help_close_clicked (object obj, EventArgs args) {
		notebook.CurrentPage = 0;
	}
	
	void on_button_manually_create_clicked (object obj, EventArgs args) 
	{
		if(created_table) {
			label_message.Text = tableAlreadyCreatedString;
			label_message.Visible = true;
			return;
		}

		rows = Convert.ToInt32(spin_manually.Value);

		createEmptyTable();
	}

	void createEmptyTable() {
		entries = new ArrayList();
		radiosM = new ArrayList();
		radiosF = new ArrayList();
		spins = new ArrayList();

		Gtk.Label nameLabel = new Gtk.Label("<b>" + Catalog.GetString("Full name") + "</b>");
		Gtk.Label sexLabel = new Gtk.Label("<b>" + Catalog.GetString("Sex") + "</b>");
		Gtk.Label weightLabel = new Gtk.Label("<b>" + Catalog.GetString("Weight") +
			"</b>(" + Catalog.GetString("Kg") + ")" );
		
		nameLabel.UseMarkup = true;
		sexLabel.UseMarkup = true;
		weightLabel.UseMarkup = true;

		nameLabel.Xalign = 0;
		sexLabel.Xalign = 0;
		weightLabel.Xalign = 0;
		
		weightLabel.Show();
		nameLabel.Show();
		sexLabel.Show();
	
		uint padding = 4;	

		table_main.Attach (nameLabel, (uint) 1, (uint) 2, 0, 1, 
				Gtk.AttachOptions.Fill | Gtk.AttachOptions.Expand , Gtk.AttachOptions.Shrink, padding, padding);
		table_main.Attach (sexLabel, (uint) 2, (uint) 3, 0, 1, 
				Gtk.AttachOptions.Shrink, Gtk.AttachOptions.Shrink, padding, padding);
		table_main.Attach (weightLabel, (uint) 3, (uint) 4, 0, 1, 
				Gtk.AttachOptions.Shrink, Gtk.AttachOptions.Shrink, padding, padding);

		for (int count=1; count <= rows; count ++) {
			Gtk.Label myLabel = new Gtk.Label((count).ToString());
			table_main.Attach (myLabel, (uint) 0, (uint) 1, (uint) count, (uint) count +1, 
					Gtk.AttachOptions.Shrink, Gtk.AttachOptions.Shrink, padding, padding);
			myLabel.Show();
			//labels.Add(myLabel);

			Gtk.Entry myEntry = new Gtk.Entry();
			table_main.Attach (myEntry, (uint) 1, (uint) 2, (uint) count, (uint) count +1, 
					Gtk.AttachOptions.Fill | Gtk.AttachOptions.Expand , Gtk.AttachOptions.Shrink, padding, padding);
			myEntry.Show();
			entries.Add(myEntry);

			
			Gtk.RadioButton myRadioM = new Gtk.RadioButton(Catalog.GetString(Constants.M));
			myRadioM.Show();
			radiosM.Add(myRadioM);
			
			Gtk.RadioButton myRadioF = new Gtk.RadioButton(myRadioM, Catalog.GetString(Constants.F));
			myRadioF.Show();
			radiosF.Add(myRadioF);
			
			Gtk.HBox sexBox = new HBox();
			sexBox.PackStart(myRadioM, false, false, 4);
			sexBox.PackStart(myRadioF, false, false, 4);
			sexBox.Show();
			table_main.Attach (sexBox, (uint) 2, (uint) 3, (uint) count, (uint) count +1, 
					Gtk.AttachOptions.Shrink, Gtk.AttachOptions.Shrink, padding, padding);


			Gtk.SpinButton mySpin = new Gtk.SpinButton(0, 300, .1);
			table_main.Attach (mySpin, (uint) 3, (uint) 4, (uint) count, (uint) count +1, 
					Gtk.AttachOptions.Shrink, Gtk.AttachOptions.Shrink, padding, padding);
			mySpin.Show();
			spins.Add(mySpin);
		}

		string sportStuffString = "";
		if(currentSession.PersonsSportID != Constants.SportUndefinedID)
			sportStuffString += Catalog.GetString("Sport") + ":<i>" + Catalog.GetString(SqliteSport.Select(false, currentSession.PersonsSportID).Name) + "</i>.";
		if(currentSession.PersonsSpeciallityID != Constants.SpeciallityUndefinedID)
			sportStuffString += " " + Catalog.GetString("Specialty") + ":<i>" + SqliteSpeciallity.Select(false, currentSession.PersonsSpeciallityID) + "</i>.";
		if(currentSession.PersonsPractice != Constants.LevelUndefinedID)
			sportStuffString += " " + Catalog.GetString("Level") + ":<i>" + Util.FindLevelName(currentSession.PersonsPractice) + "</i>.";

		if(sportStuffString.Length > 0)
			sportStuffString = Catalog.GetString("Persons will be created with default session values") + 
				":\n" + sportStuffString;
		label_message.Text = sportStuffString;
		label_message.UseMarkup = true;
		label_message.Visible = true;

		table_main.Show();
		scrolledwindow.Visible = true;
		notebook.CurrentPage = 0;
			
		//once loaded table cannot be created again
		//don't do this: it crashes
		//button_manually_created.Sensitive = false;
		//do this:
		created_table = true;
	
		button_accept.Sensitive = true;
	}
		
	void fillTableFromCSV(ArrayList array) {
		int i = 0;
		foreach(PersonAddMultipleTable pamt in array) {
			((Gtk.Entry)entries[i]).Text = pamt.name;
			((Gtk.RadioButton)radiosM[i]).Active = pamt.maleOrFemale;
			((Gtk.RadioButton)radiosF[i]).Active = ! pamt.maleOrFemale;
			((Gtk.SpinButton)spins[i]).Value = pamt.weight;
			i++;
		}
	}

	void on_button_accept_clicked (object o, EventArgs args)
	{
		errorExistsString = "";
		errorWeightString = "";
		errorRepeatedEntryString = "";
		personsCreatedCount = 0;

		Sqlite.Open();
		for (int i = 0; i < rows; i ++) 
			checkEntries(i, ((Gtk.Entry)entries[i]).Text.ToString(), (int) ((Gtk.SpinButton)spins[i]).Value);
		Sqlite.Close();
	
		checkAllEntriesAreDifferent();

		string combinedErrorString = "";
		combinedErrorString = readErrorStrings();
		
		if (combinedErrorString.Length > 0) {
			ErrorWindow.Show(combinedErrorString);
		} else {
			processAllNonBlankRows();
		
			PersonAddMultipleWindowBox.person_multiple_infinite.Hide();
			PersonAddMultipleWindowBox = null;
		}
	}
		
	private void checkEntries(int count, string name, double weight) {
		if(name.Length > 0) {
			bool personExists = Sqlite.Exists (true, Constants.PersonTable, Util.RemoveTilde(name));
			if(personExists) {
				errorExistsString += "[" + (count+1) + "] " + name + "\n";
			}
			if(Convert.ToInt32(weight) == 0) {
				errorWeightString += "[" + (count+1) + "] " + name + "\n";
			}
		}
	}
		
	void checkAllEntriesAreDifferent() {
		ArrayList newNames= new ArrayList();
		for (int i = 0; i < rows; i ++) 
			newNames.Add(((Gtk.Entry)entries[i]).Text.ToString());

		for(int i=0; i < rows; i++) {
			bool repeated = false;
			if(Util.RemoveTilde(newNames[i].ToString()).Length > 0) {
				int j;
				for(j=i+1; j < newNames.Count && !repeated; j++) {
					if( Util.RemoveTilde(newNames[i].ToString()) == Util.RemoveTilde(newNames[j].ToString()) ) {
						repeated = true;
					}
				}
				if(repeated) {
					errorRepeatedEntryString += string.Format("[{0}] {1} - [{2}] {3}\n",
							i+1, newNames[i].ToString(), j, newNames[j-1].ToString());
				}
			}
		}
	}
	
	string readErrorStrings() {
		if (errorExistsString.Length > 0) {
			errorExistsString = "ERROR This person(s) exists in the database:\n" + errorExistsString;
		}
		if (errorWeightString.Length > 0) {
			errorWeightString = "\nERROR weight of this person(s) cannot be 0:\n" + errorWeightString;
		}
		if (errorRepeatedEntryString.Length > 0) {
			errorRepeatedEntryString = "\nERROR this names are repeated:\n" + errorRepeatedEntryString;
		}
		
		return errorExistsString + errorWeightString + errorRepeatedEntryString;
	}

	//inserts all the rows where name is not blank
	//all this names doesn't match with other in the database, and the weights are > 0 ( checked in checkEntries() )
	void processAllNonBlankRows() 
	{
		int pID;
		int countPersons = Sqlite.Count(Constants.PersonTable, false);
		if(countPersons == 0)
			pID = 1;
		else {
			//Sqlite.Max will return NULL if there are no values, for this reason we use the Sqlite.Count before
			int maxPUniqueID = Sqlite.Max(Constants.PersonTable, "uniqueID", false);
			pID = maxPUniqueID + 1;
		}

		int psID;
		int countPersonSessions = Sqlite.Count(Constants.PersonSessionTable, false);
		if(countPersonSessions == 0)
			psID = 1;
		else {
			//Sqlite.Max will return NULL if there are no values, for this reason we use the Sqlite.Count before
			int maxPSUniqueID = Sqlite.Max(Constants.PersonSessionTable, "uniqueID", false);
			psID = maxPSUniqueID + 1;
		}
		
		string sex = "";
		double weight = 0;
				
		List <Person> persons = new List<Person>();
		List <PersonSession> personSessions = new List<PersonSession>();

		DateTime dateTime = DateTime.MinValue;

		//the last is the first for having the first value inserted as currentPerson
		for (int i = rows -1; i >= 0; i --) 
			if(((Gtk.Entry)entries[i]).Text.ToString().Length > 0) 
			{
				sex = Constants.F;
				if(((Gtk.RadioButton)radiosM[i]).Active) { sex = Constants.M; }

				currentPerson = new Person(
							pID ++,
							((Gtk.Entry)entries[i]).Text.ToString(), //name
							sex,
							dateTime,
							Constants.RaceUndefinedID,
							Constants.CountryUndefinedID,
							"", 					//description
							Constants.ServerUndefinedID
							);
				
				persons.Add(currentPerson);
						
				weight = (double) ((Gtk.SpinButton)spins[i]).Value;
				personSessions.Add(new PersonSession(
							psID ++,
							currentPerson.UniqueID, currentSession.UniqueID, 
							0, weight, 		//height, weight	
							currentSession.PersonsSportID,
							currentSession.PersonsSpeciallityID,
							currentSession.PersonsPractice,
							"") 			//comments
						);

				personsCreatedCount ++;
			}
	
		//do the transaction	
		new SqlitePersonSessionTransaction(persons, personSessions);
	}

	public Button Button_accept 
	{
		set {
			button_accept = value;	
		}
		get {
			return button_accept;
		}
	}

	public int PersonsCreatedCount 
	{
		get { return personsCreatedCount; }
	}
	
	public Person CurrentPerson 
	{
		get {
			return currentPerson;
		}
	}

}

//show all events (jumps and runs) of a person in different sessions
public class PersonShowAllEventsWindow {
	
	[Widget] Gtk.Window person_show_all_events;
	
	[Widget] Gtk.CheckButton checkbutton_only_current_session;
	
	TreeStore store;
	[Widget] Gtk.TreeView treeview_person_show_all_events;
	[Widget] Gtk.Box hbox_combo_persons;
	[Widget] Gtk.ComboBox combo_persons;
	
	static PersonShowAllEventsWindow PersonShowAllEventsWindowBox;

	protected int sessionID;
	
	protected Person currentPerson;
	
	PersonShowAllEventsWindow (Gtk.Window parent, int sessionID, Person currentPerson) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "person_show_all_events.glade", "person_show_all_events", "chronojump");
		gladeXML.Autoconnect(this);
		
		//put an icon to window
		UtilGtk.IconWindow(person_show_all_events);

		person_show_all_events.Parent = parent;
		this.sessionID = sessionID;
		this.currentPerson = currentPerson;
	
		createComboPersons(sessionID, currentPerson.UniqueID.ToString(), currentPerson.Name);
		createTreeView(treeview_person_show_all_events);
		store = new TreeStore( typeof (string), typeof (string), typeof (string), typeof (string), 
				typeof (string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string),
			       	typeof (string), typeof(string));
		treeview_person_show_all_events.Model = store;
		fillTreeView(treeview_person_show_all_events,store, currentPerson.UniqueID);
	}
	
	static public PersonShowAllEventsWindow Show (Gtk.Window parent, int sessionID, Person currentPerson)
	{
		if (PersonShowAllEventsWindowBox == null) {
			PersonShowAllEventsWindowBox = new PersonShowAllEventsWindow (parent, sessionID, currentPerson);
		}
		PersonShowAllEventsWindowBox.person_show_all_events.Show ();
		
		return PersonShowAllEventsWindowBox;
	}
	
	private void createComboPersons(int sessionID, string personID, string personName) {
		combo_persons = ComboBox.NewText ();

		int inSession = -1;		//select persons from all sessions
		if(checkbutton_only_current_session.Active) {
			inSession = sessionID;	//select only persons who are on currentSession
		}
		
		ArrayList myPersons = SqlitePerson.SelectAllPersonsRecuperable("name", -1, inSession, ""); //"" is searchFilterName (not implemented on PersonShowAllEventsWindow)

		//put only id and name in combo
		string [] myPersonsIDName = new string[myPersons.Count];
		int count = 0;
		foreach (Person person in myPersons) 
			myPersonsIDName[count++] = person.IDAndName(":");
		
		UtilGtk.ComboUpdate(combo_persons, myPersonsIDName, "");
		combo_persons.Active = UtilGtk.ComboMakeActive(myPersonsIDName, personID + ":" + personName);

		combo_persons.Changed += new EventHandler (on_combo_persons_changed);

		hbox_combo_persons.PackStart(combo_persons, true, true, 0);
		hbox_combo_persons.ShowAll();
		combo_persons.Sensitive = true;
	}
	
	private void on_combo_persons_changed(object o, EventArgs args) {
		string myText = UtilGtk.ComboGetActive(combo_persons);
		if(myText != "") {
			store = new TreeStore( typeof (string), typeof (string), typeof (string), typeof (string), 
					typeof (string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string),
				       	typeof (string), typeof(string));
			treeview_person_show_all_events.Model = store;
			
			string [] myStringFull = myText.Split(new char[] {':'});

			fillTreeView( treeview_person_show_all_events, store, Convert.ToInt32(myStringFull[0]) );
		}
	}
	
	protected void on_checkbutton_only_current_session_clicked(object o, EventArgs args) {
		string myText = UtilGtk.ComboGetActive(combo_persons);
		if(myText != "") {
			string [] myStringFull = myText.Split(new char[] {':'});
			combo_persons.Destroy();
			createComboPersons(sessionID, myStringFull[0], myStringFull[1] );
			on_combo_persons_changed(0, args);	//called for updating the treeview ifcombo_persons.Entry changed
		}
	}
	
	protected void createTreeView (Gtk.TreeView tv) {
		tv.HeadersVisible=true;
		int count = 0;
		tv.AppendColumn ( Catalog.GetString ("Session name"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Place"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Date\n"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Jumps\nsimple"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Jumps\nreactive"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Races\nsimple"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Races\ninterval"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Reaction\ntime"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Pulses"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("MultiChronopic"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Encoder sets"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Encoder repetitions"), new CellRendererText(), "text", count++);
	}
	
	protected void fillTreeView (Gtk.TreeView tv, TreeStore store, int personID) {
		ArrayList myEvents;
		myEvents = SqlitePerson.SelectAllPersonEvents(personID); 

		foreach (string myEvent in myEvents) {
			string [] myStr = myEvent.Split(new char[] {':'});

			store.AppendValues (myStr[0], myStr[1], myStr[2], myStr[3], myStr[4], myStr[5], 
					myStr[6], myStr[7], myStr[8], myStr[9], myStr[10], myStr[11]);
		}
	}
	

	protected virtual void on_button_close_clicked (object o, EventArgs args)
	{
		PersonShowAllEventsWindowBox.person_show_all_events.Hide();
		PersonShowAllEventsWindowBox = null;
	}
	
	protected virtual void on_delete_event (object o, DeleteEventArgs args)
	{
		PersonShowAllEventsWindowBox.person_show_all_events.Hide();
		PersonShowAllEventsWindowBox = null;
	}
}
