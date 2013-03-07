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
 * Copyright (C) 2004-2009   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Gdk;
using Glade;
using GLib; //for Value
using System.Text; //StringBuilder
using System.Collections; //ArrayList
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
	protected PersonAddModifyWindow personAddModifyWin; 

	protected Gtk.Window parent;
	
	protected Person currentPerson;
	protected Session currentSession;

	protected int columnId = 0;
	protected int firstColumn = 0;
	protected int pDN;
	
	public Gtk.Button fakeButtonDone;

	protected PersonRecuperateWindow () {
	}

	PersonRecuperateWindow (Gtk.Window parent, Session currentSession) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "person_recuperate", null);
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
			Person person = SqlitePerson.Select(Convert.ToInt32(selected));

			personAddModifyWin = PersonAddModifyWindow.Show(
					parent, currentSession, person, pDN, true); //comes from recuperate window
			personAddModifyWin.FakeButtonAccept.Clicked += new EventHandler(on_edit_current_person_accepted);
		}
	}
	
	private void on_edit_current_person_cancelled (object o, EventArgs args) {
		personAddModifyWin.FakeButtonCancel.Clicked -= new EventHandler(on_edit_current_person_cancelled);
		fakeButtonDone.Click();
	}
	
	protected virtual void on_edit_current_person_accepted (object o, EventArgs args) {
		personAddModifyWin.FakeButtonAccept.Clicked -= new EventHandler(on_edit_current_person_accepted);
		if (personAddModifyWin.CurrentPerson != null)
		{
			currentPerson = personAddModifyWin.CurrentPerson;
			
			store = new TreeStore( typeof (string), typeof (string), typeof (string), typeof (string), typeof (string) );
			treeview_person_recuperate.Model = store;
		
			fillTreeView(treeview_person_recuperate,store, entry_search_filter.Text.ToString());
				
			statusbar1.Push( 1, Catalog.GetString("Loaded") + " " + currentPerson.Name );
		
			//no posible to recuperate until one person is selected
			button_recuperate.Sensitive = false;

//			personAddModifyWin.Destroy();
		
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

}


//load person (jumper)
public class PersonsRecuperateFromOtherSessionWindow : PersonRecuperateWindow 
{
	static PersonsRecuperateFromOtherSessionWindow PersonsRecuperateFromOtherSessionWindowBox;
	
	[Widget] Gtk.Box hbox_combo_sessions;
	[Widget] Gtk.ComboBox combo_sessions;
	[Widget] protected Gtk.Box hbox_combo_select_checkboxes;
	[Widget] protected Gtk.ComboBox combo_select_checkboxes;
	
	private Gtk.Button fakeButtonPreDone;
	
	
	protected static string [] comboCheckboxesOptions = {
		Catalog.GetString("All"),
		Catalog.GetString("None"),
		Catalog.GetString("Selected"),
	};
	
	protected PersonsRecuperateFromOtherSessionWindow () {
	}

	PersonsRecuperateFromOtherSessionWindow (Gtk.Window parent, Session currentSession) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "person_recuperate", null);
		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(person_recuperate);
		person_recuperate.Title = Catalog.GetString("Load persons from other session");

	
		//this class doesn't allow to search by name
		hbox_search_filter_hide.Hide();
		
		this.currentSession = currentSession;
		
		fakeButtonDone = new Gtk.Button();
		fakeButtonPreDone = new Gtk.Button();
	
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

	static public new PersonsRecuperateFromOtherSessionWindow Show (Gtk.Window parent, Session currentSession)
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
				Log.WriteLine("Do later!!");
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
		Log.WriteLine("Toggled");

		int column = 0;
		TreeIter iter;
		if (store.GetIter (out iter, new TreePath(args.Path))) 
		{
			bool val = (bool) store.GetValue (iter, column);
			Log.WriteLine (string.Format("toggled {0} with value {1}", args.Path, !val));

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

	int currentRow;
	int inserted;
	protected override void on_button_recuperate_clicked (object o, EventArgs args)
	{
		inserted = 0;
		currentRow = 0;

		fakeButtonPreDone.Clicked += new EventHandler(updateStoreAndEnd);
		processRow();
	}

	//takes a row every time
	//if it founds data to sent to AddModifyWin and will be called again
	//else don't will be called again, for this reason calls: fakeButtonPreDone
	private void processRow()
	{
		Gtk.TreeIter iter;
		bool val;
		int count = 0;
		bool found = false;
		if (store.GetIterFirst(out iter)) {
			do {
				val = (bool) store.GetValue (iter, 0);
				//if checkbox of person is true and is the row that we are processing
				if(val && count++ == currentRow) {
					Person person = SqlitePerson.Select(
							Convert.ToInt32(treeview_person_recuperate.Model.GetValue(iter, 1)) );
					personAddModifyWin = PersonAddModifyWindow.Show(
							parent, currentSession, person, pDN, true); //comes from recuperate window
					PersonAddModifyWindow.MakeVisible();
					personAddModifyWin.FakeButtonAccept.Clicked += new EventHandler(on_edit_current_person_accepted);
					personAddModifyWin.FakeButtonCancel.Clicked += new EventHandler(on_edit_current_person_cancelled);
					inserted ++;
					found = true;
				}
			} while ( store.IterNext(ref iter) );
		}
		if(!found)
			fakeButtonPreDone.Click();
	}

	private void updateStoreAndEnd(object o, EventArgs args)
	{
		fakeButtonPreDone.Clicked -= new EventHandler(updateStoreAndEnd);
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
	
	protected override void on_edit_current_person_accepted (object o, EventArgs args) {
		personAddModifyWin.FakeButtonAccept.Clicked -= new EventHandler(on_edit_current_person_accepted);
		if (personAddModifyWin.CurrentPerson != null)
		{
			currentPerson = personAddModifyWin.CurrentPerson;
			currentRow ++;
				
			Log.WriteLine("To sleep in order AddMoidfyWin gets closed, in order to open again");
			System.Threading.Thread.Sleep (100);
			Log.WriteLine("done");
			processRow();
		}
	}
	
	private void on_edit_current_person_cancelled (object o, EventArgs args) {
		personAddModifyWin.FakeButtonCancel.Clicked -= new EventHandler(on_edit_current_person_cancelled);
		if (personAddModifyWin.CurrentPerson != null)
		{
			currentRow ++;
			
			Log.WriteLine("To sleep in order AddModifyWin gets closed, in order to open again");
			System.Threading.Thread.Sleep (100);
			Log.WriteLine("done");
			inserted --;
			processRow();
		}
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
	public Gtk.Button fakeButtonDone;
	
	PersonNotUploadWindow (Gtk.Window parent, int sessionID) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "person_recuperate", null);
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

	static public new PersonNotUploadWindow Show (Gtk.Window parent, int sessionID)
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
	
	public Button FakeButtonDone 
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
	
	[Widget] Gtk.Label label_date;
	//[Widget] Gtk.Button button_change_date;
	[Widget] Gtk.Button button_calendar;
	[Widget] Gtk.Image image_calendar;

	[Widget] Gtk.SpinButton spinbutton_height;
	[Widget] Gtk.SpinButton spinbutton_weight;
	[Widget] Gtk.Button button_height_metric;
	[Widget] Gtk.Button button_weight_metric;
	
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
	[Widget] Gtk.Image image_date;
	[Widget] Gtk.Image image_weight;
	[Widget] Gtk.Image image_sport;
	[Widget] Gtk.Image image_speciallity;
	[Widget] Gtk.Image image_level;
	
	[Widget] Gtk.Button button_zoom;
	[Widget] Gtk.Image image_photo_mini;
	[Widget] Gtk.Image image_zoom;

	[Widget] Gtk.Button button_accept;
	[Widget] Gtk.Button button_cancel;
	
	//used for connect ok gui/chronojump.cs, PersonRecuperate, PersonRecuperateFromOtherSession,this class, gui/convertWeight.cs
	public Gtk.Button fakeButtonAccept;
	//used for connect PersonRecuperateFromOtherSession
	public Gtk.Button fakeButtonCancel;
	
	static ConvertWeightWindow convertWeightWin;
	
	static PersonAddModifyWindow PersonAddModifyWindowBox;
	
	Gtk.Window parent;
	
	DialogCalendar myDialogCalendar;
	DateTime dateTime;
	Sport sport;
	string [] sports;
	string [] sportsTranslated;
	int speciallityID;
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
	
	private int serverUniqueID;

	private bool comesFromRecuperateWin;
	
	//
	//if we are adding a person, currentPerson.UniqueID it's -1
	//if we are modifying a person, currentPerson.UniqueID is obviously it's ID
	PersonAddModifyWindow (Gtk.Window parent, Session currentSession, Person currentPerson) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "person_win", null);
		gladeXML.Autoconnect(this);
		
		//put an icon to window
		UtilGtk.IconWindow(person_win);
	
		this.parent = parent;
		this.currentSession = currentSession;
		this.currentPerson = currentPerson;

		//when comesFromRecuperateWin is true, is considered editing because uniqueID is known
		if(currentPerson.UniqueID == -1)
			adding = true;
		else
			adding = false;
		
		createComboSports();
		createComboSpeciallities(-1);
		image_speciallity.Hide();
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
			
		fakeButtonAccept = new Gtk.Button();
		fakeButtonCancel = new Gtk.Button();

		if(adding) {
			person_win.Title = Catalog.GetString ("New jumper");
			button_accept.Sensitive = false;
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

		new DialogImageTest(currentPerson.Name, tempFileName);
	}

	Gtk.Window capturerWindow;
	CapturerBin capturer;
	void on_button_take_photo_clicked (object o, EventArgs args) 
	{

		new DialogMessage(Constants.MessageTypes.INFO, 
				Catalog.GetString("Sorry, photos are disabled on this version."));
		/*
		capturer = new CapturerBin();
		CapturePropertiesStruct s = new CapturePropertiesStruct();

		s.CaptureSourceType = CaptureSourceType.Raw;

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
		*/
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
		bool allOk = true;
		
		if(entry1.Text.ToString().Length > 0)
			image_name.Hide();
		else {
			image_name.Show();
			allOk = false;
		}

		if((double) spinbutton_weight.Value > 0)
			image_weight.Hide();
		else {
			image_weight.Show();
			allOk = false;
		}
				
		if(dateTime != DateTime.MinValue)
			image_date.Hide();
		else {
			image_date.Show();
			allOk = false;
		}

		if(UtilGtk.ComboGetActive(combo_sports) != Catalog.GetString(Constants.SportUndefined))
			image_sport.Hide();
		else {
			image_sport.Show();
			allOk = false;
		}

		if (! label_speciallity.Visible || 
				UtilGtk.ComboGetActive(combo_speciallities) != Catalog.GetString(Constants.SpeciallityUndefined))
			image_speciallity.Hide();
		else {
			image_speciallity.Show();
			allOk = false;
		}
				
		if(Util.FetchID(UtilGtk.ComboGetActive(combo_levels)) != Constants.LevelUndefinedID)
			image_level.Hide();
		else {
			image_level.Show();
			allOk = false;
		}
				
		//countries is not required to create a person here, but will be required for server
		//&& 
		//UtilGtk.ComboGetActive(combo_continents) != Catalog.GetString(Constants.ContinentUndefined) &&
		//UtilGtk.ComboGetActive(combo_countries) != Catalog.GetString(Constants.CountryUndefined)
			
		if(allOk)
			button_accept.Sensitive = true;
		else
			button_accept.Sensitive = false;
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
			Session mySession, Person currentPerson, int pDN, bool comesFromRecuperateWin)
	{
		if (comesFromRecuperateWin) 
			PersonAddModifyWindowBox = null;

		if (PersonAddModifyWindowBox == null) {
			PersonAddModifyWindowBox = new PersonAddModifyWindow (parent, mySession, currentPerson);
		}

		PersonAddModifyWindowBox.pDN = pDN;
		PersonAddModifyWindowBox.comesFromRecuperateWin = comesFromRecuperateWin;

		//No more hide cancel button.
		//Better to show it and allow to not recuperate if user changes his mind
		//if(comesFromRecuperateWin)
			//PersonAddModifyWindowBox.button_cancel.Hide();
		
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
			PersonSession myPS = new PersonSession();
			if(comesFromRecuperateWin)
				//select a personSession of last session to obtain it's attributes
				myPS = SqlitePersonSession.Select(currentPerson.UniqueID, -1);
			else
				//we edit a person that is already on this session, then take personSession data from this session
				myPS = SqlitePersonSession.Select(currentPerson.UniqueID, currentSession.UniqueID);

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
			
		sport = SqliteSport.Select(mySportID);
		combo_sports.Active = UtilGtk.ComboMakeActive(sportsTranslated, sport.ToString());

		combo_speciallities.Active = UtilGtk.ComboMakeActive(speciallitiesTranslated, SqliteSpeciallity.Select(mySpeciallityID));

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
		spinbutton_weight.Value = Util.ConvertPoundsToKg(genericWin.SpinDoubleSelected);
	}


	private void on_combo_sports_changed(object o, EventArgs args) {
		if (o == null)
			return;

		//Log.WriteLine("changed");
		try {
			int sportID = Convert.ToInt32(Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(combo_sports), sports));
			sport = SqliteSport.Select(sportID);

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
				catch { Log.WriteLine("do later"); }
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
				catch { Log.WriteLine("do later"); }
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
					Log.Write("hide");
					combo_speciallities.Active = UtilGtk.ComboMakeActive(speciallitiesTranslated,
						       	Catalog.GetString(Constants.SpeciallityUndefined));
					label_speciallity.Hide();
					combo_speciallities.Hide();
				}
			}
		} catch { 
			//Log.WriteLine("do later");
		}

		on_entries_required_changed(new object(), new EventArgs());
		Log.WriteLine(sport.ToString());
	}
	
	private void on_combo_speciallities_changed(object o, EventArgs args) {
		Log.WriteLine("changed speciallities");
		on_entries_required_changed(new object(), new EventArgs());
	}

	private void on_combo_levels_changed(object o, EventArgs args) {
		//string myText = UtilGtk.ComboGetActive(combo_sports);
		Log.WriteLine("changed levels");
		on_entries_required_changed(new object(), new EventArgs());
		//level = UtilGtk.ComboGetActive(combo_levels);
				
		//if it's sedentary, put sport to none
		/*
		 * Now undone because sedentary has renamed to "sedentary/Ocasional practice"
		if(UtilGtk.ComboGetActive(combo_levels) == "0:" + Catalog.GetString(Constants.LevelSedentary))
			combo_sports.Active = UtilGtk.ComboMakeActive(sports, "2:" + Catalog.GetString(Constants.SportNone));
		*/
	}
	
	private void on_combo_continents_changed(object o, EventArgs args) {
		//Console.WriteLine("Changed");
		
		if(UtilGtk.ComboGetActive(combo_continents) == Catalog.GetString(Constants.ContinentUndefined)) {
			countries [0] = Constants.CountryUndefinedID + ":" + Constants.CountryUndefined + ":" + Catalog.GetString(Constants.CountryUndefined);
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
		Log.WriteLine("sport add clicked");
		genericWin = GenericWindow.Show(Catalog.GetString("Add new sport to database"), Constants.GenericWindowShow.ENTRY);
		genericWin.Button_accept.Clicked += new EventHandler(on_sport_add_accepted);
	}

	private void on_sport_add_accepted (object o, EventArgs args) {
		genericWin.Button_accept.Clicked -= new EventHandler(on_sport_add_accepted);
		string newSportName = genericWin.EntrySelected;
		if(Sqlite.Exists(Constants.SportTable, newSportName) ||
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
		bool personExists;
		if(adding)
			personExists = Sqlite.Exists (Constants.PersonTable, Util.RemoveTilde(entry1.Text));
		else
			personExists = SqlitePerson.ExistsAndItsNotMe (currentPerson.UniqueID, Util.RemoveTilde(entry1.Text));

		string errorMessage = "";

		if(personExists) 
			errorMessage += string.Format(Catalog.GetString("Person: '{0}' exists. Please, use another name"), Util.RemoveTildeAndColonAndDot(entry1.Text) );
		else if (sport.Name == Catalog.GetString(Constants.SportUndefined)) 
			errorMessage += Catalog.GetString("Please select an sport");

		//here sport shouldn't be undefined, then check 
		//if it has speciallities and if they are selected
		else if (sport.HasSpeciallities && 
				UtilGtk.ComboGetActive(combo_speciallities) == Catalog.GetString(Constants.SpeciallityUndefined))
			errorMessage += Catalog.GetString("Please select an speciallity");
		else if (UtilGtk.ComboGetActive(combo_levels) ==
				Constants.LevelUndefinedID.ToString() + ":" + 
			       	Catalog.GetString(Constants.LevelUndefined))
			errorMessage += Catalog.GetString("Please select a level");
		else {
			//if weight has changed
			if(!adding && (double) spinbutton_weight.Value != weightIni) {
				//see if this person has done jumps with weight
				string [] myJumpsNormal = SqliteJump.SelectJumps(currentSession.UniqueID, currentPerson.UniqueID, "withWeight", "");
				string [] myJumpsReactive = SqliteJumpRj.SelectJumps(currentSession.UniqueID, currentPerson.UniqueID, "withWeight", "");

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
		string [] myMargarias = SqliteRun.SelectRuns(currentSession.UniqueID, currentPerson.UniqueID, "Margaria");
		foreach(string myStr in myMargarias) {
			string [] margaria = myStr.Split(new char[] {':'});
			Run mRun = SqliteRun.SelectRunData(Convert.ToInt32(margaria[1]), false);
			double distanceMeters = mRun.Distance / 1000;
			mRun.Description = "P = " + Util.TrimDecimals ( (weight * 9.8 * distanceMeters / mRun.Time).ToString(), pDN) + " (Watts)";
			SqliteRun.Update(mRun.UniqueID, mRun.Type, mRun.Distance.ToString(), mRun.Time.ToString(), mRun.PersonID, mRun.Description);
		}


		if(adding) {
			//here we add rows in the database
			currentPerson = new Person (entry1.Text, sex, dateTime, 
					Constants.RaceUndefinedID,
					Convert.ToInt32(Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(combo_countries), countries)),
					textview_description.Buffer.Text,
					Constants.ServerUndefinedID);
					
			currentPersonSession = new PersonSession (
					currentPerson.UniqueID, currentSession.UniqueID, 
					(double) spinbutton_height.Value, (double) weight, 
					sport.UniqueID, 
					Convert.ToInt32(Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(combo_speciallities), speciallities)),
					Util.FetchID(UtilGtk.ComboGetActive(combo_levels)),
					textview_ps_comments.Buffer.Text);
		} else {
			//here we update rows in the database
			currentPerson = new Person (currentPerson.UniqueID, entry1.Text, sex, dateTime, 
					Constants.RaceUndefinedID,
					Convert.ToInt32(Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(combo_countries), countries)),
					textview_description.Buffer.Text,
					serverUniqueID);
			SqlitePerson.Update (currentPerson); 
			
			//person session stuff
			//if comesFromRecuperate means that we are recuperating (loading a person)
			//the recuperate person gui calls this gui to know if anything changed
			//then we are editing person (it exists before), but has no personSession record related to this session
			//then we insert:
			if(comesFromRecuperateWin)
				currentPersonSession = new PersonSession (
						currentPerson.UniqueID, currentSession.UniqueID, 
						(double) spinbutton_height.Value, (double) weight, 
						sport.UniqueID, 
						Convert.ToInt32(Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(combo_speciallities), speciallities)),
						Util.FetchID(UtilGtk.ComboGetActive(combo_levels)),
						textview_ps_comments.Buffer.Text);
			else {
				//don't come from recuperate
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
		}


		PersonAddModifyWindowBox.person_win.Hide();
		PersonAddModifyWindowBox = null;
		
		fakeButtonAccept.Click();
	}
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		PersonAddModifyWindowBox.person_win.Hide();
		PersonAddModifyWindowBox = null;
		
		fakeButtonCancel.Click();
	}
	
	//void on_person_modify_delete_event (object o, EventArgs args)
	void on_person_win_delete_event (object o, DeleteEventArgs args)
	{
		//nice: this makes windows no destroyed, now nothing happens
		if(comesFromRecuperateWin)
			args.RetVal = true;
		else {
			PersonAddModifyWindowBox.person_win.Hide();
			PersonAddModifyWindowBox = null;
		
			fakeButtonCancel.Click();
		}
	}
	
	

	public void Destroy() {
		//PersonAddModifyWindowBox.person_win.Destroy();
	}

	public Button FakeButtonAccept 
	{
		set { fakeButtonAccept = value; }
		get { return fakeButtonAccept; }
	}
	
	public Button FakeButtonCancel 
	{
		set { fakeButtonCancel = value; }
		get { return fakeButtonCancel; }
	}


	public Person CurrentPerson {
		get { return currentPerson; }
	}
	
	public PersonSession CurrentPersonSession {
		get { return currentPersonSession; }
	}
	
}

//new persons multiple (infinite)
public class PersonAddMultipleWindow {
	
	[Widget] Gtk.Window person_multiple_infinite;

	ArrayList entries;
	ArrayList radiosM;
	ArrayList radiosF;
	ArrayList spins;
	int rows;
	
	[Widget] Gtk.Table table_main;
	[Widget] Gtk.Label label_sport_stuff;
	
	[Widget] Gtk.Button button_accept;
	
	static PersonAddMultipleWindow PersonAddMultipleWindowBox;
	Gtk.Window parent;

	private Person currentPerson;
	Session currentSession;
	int personsCreatedCount;
	string errorExistsString;
	string errorWeightString;
	string errorRepeatedEntryString;
	
	PersonAddMultipleWindow (Gtk.Window parent, Session currentSession) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "person_multiple_infinite", null);
		gladeXML.Autoconnect(this);
		
		//put an icon to window
		UtilGtk.IconWindow(person_multiple_infinite);
	
		this.parent = parent;
		this.currentSession = currentSession;
	}
	
	static public PersonAddMultipleWindow Show (Gtk.Window parent, Session currentSession, int rows)
	{
		if (PersonAddMultipleWindowBox == null) {
			PersonAddMultipleWindowBox = new PersonAddMultipleWindow (parent, currentSession);
		}
		PersonAddMultipleWindowBox.rows = rows;
		PersonAddMultipleWindowBox.create ();

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
	
	void create() {
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
		

		table_main.Attach (nameLabel, (uint) 1, (uint) 2, 0, 1);
		table_main.Attach (sexLabel, (uint) 2, (uint) 3, 0, 1);
		table_main.Attach (weightLabel, (uint) 3, (uint) 4, 0, 1);

		for (int count=1; count <= rows; count ++) {
			Gtk.Label myLabel = new Gtk.Label((count).ToString());
			table_main.Attach (myLabel, (uint) 0, (uint) 1, (uint) count, (uint) count +1);
			myLabel.Show();
			//labels.Add(myLabel);

			Gtk.Entry myEntry = new Gtk.Entry();
			table_main.Attach (myEntry, (uint) 1, (uint) 2, (uint) count, (uint) count +1);
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
			table_main.Attach (sexBox, (uint) 2, (uint) 3, (uint) count, (uint) count +1);


			Gtk.SpinButton mySpin = new Gtk.SpinButton(0, 300, .1);
			table_main.Attach (mySpin, (uint) 3, (uint) 4, (uint) count, (uint) count +1);
			mySpin.Show();
			spins.Add(mySpin);
		}

		string sportStuffString = "";
		if(currentSession.PersonsSportID != Constants.SportUndefinedID)
			sportStuffString += Catalog.GetString("Sport") + ":<i>" + Catalog.GetString(SqliteSport.Select(currentSession.PersonsSportID).Name) + "</i>.";
		if(currentSession.PersonsSpeciallityID != Constants.SpeciallityUndefinedID)
			sportStuffString += " " + Catalog.GetString("Speciallity") + ":<i>" + SqliteSpeciallity.Select(currentSession.PersonsSpeciallityID) + "</i>.";
		if(currentSession.PersonsPractice != Constants.LevelUndefinedID)
			sportStuffString += " " + Catalog.GetString("Level") + ":<i>" + Util.FindLevelName(currentSession.PersonsPractice) + "</i>.";

		if(sportStuffString.Length > 0)
			sportStuffString = Catalog.GetString("Persons will be created with default session values") + 
				":\n" + sportStuffString;
		label_sport_stuff.Text = sportStuffString;
		label_sport_stuff.UseMarkup = true;

		table_main.Show();
	}

	void on_button_accept_clicked (object o, EventArgs args)
	{
		errorExistsString = "";
		errorWeightString = "";
		errorRepeatedEntryString = "";
		personsCreatedCount = 0;

		for (int i = 0; i < rows; i ++) 
			checkEntries(i, ((Gtk.Entry)entries[i]).Text.ToString(), (int) ((Gtk.SpinButton)spins[i]).Value);
	
		checkAllEntriesAreDifferent();

		string combinedErrorString = "";
		combinedErrorString = readErrorStrings();
		
		if (combinedErrorString.Length > 0) {
			ErrorWindow.Show(combinedErrorString);
		} else {
			prepareAllNonBlankRows();
		
			PersonAddMultipleWindowBox.person_multiple_infinite.Hide();
			PersonAddMultipleWindowBox = null;
		}
	}
		
	void checkEntries(int count, string name, double weight) {
		if(name.Length > 0) {
			bool personExists = Sqlite.Exists (Constants.PersonTable, Util.RemoveTilde(name));
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
	void prepareAllNonBlankRows() 
	{
		//the last is the first for having the first value inserted as currentPerson
		for (int i = rows -1; i >= 0; i --) 
			if(((Gtk.Entry)entries[i]).Text.ToString().Length > 0)
				insertPerson (
						((Gtk.Entry)entries[i]).Text.ToString(), 
						((Gtk.RadioButton)radiosM[i]).Active, 
		 				(double) ((Gtk.SpinButton)spins[i]).Value);
	}

	void insertPerson (string name, bool male, double weight) 
	{
		string sex = Constants.F;
		if(male) { sex = Constants.M; }

		//now dateTime is undefined until user changes it
		DateTime dateTime = DateTime.MinValue;

		currentPerson = new Person ( name, sex, dateTime, 
				Constants.RaceUndefinedID,
				Constants.CountryUndefinedID,
				"", 			//description
				Constants.ServerUndefinedID);
				

		new PersonSession (
				currentPerson.UniqueID, currentSession.UniqueID, 
				0, weight, 		//height, weight	
				currentSession.PersonsSportID,
				currentSession.PersonsSpeciallityID,
				currentSession.PersonsPractice,
				""); 			//comments

		personsCreatedCount ++;
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

	Gtk.Window parent;	
	protected int sessionID;
	
	protected Person currentPerson;
	
	PersonShowAllEventsWindow (Gtk.Window parent, int sessionID, Person currentPerson) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "person_show_all_events", null);
		gladeXML.Autoconnect(this);
		
		//put an icon to window
		UtilGtk.IconWindow(person_show_all_events);

		this.parent = parent;
		this.sessionID = sessionID;
		this.currentPerson = currentPerson;
	
		createComboPersons(sessionID, currentPerson.UniqueID.ToString(), currentPerson.Name);
		createTreeView(treeview_person_show_all_events);
		store = new TreeStore( typeof (string), typeof (string), typeof (string), typeof (string), 
				typeof (string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string) );
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
					typeof (string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string) );
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
		tv.AppendColumn ( Catalog.GetString ("Runs\nsimple"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Runs\ninterval"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Reaction\ntime"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Pulses"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("MultiChronopic"), new CellRendererText(), "text", count++);
	}
	
	protected void fillTreeView (Gtk.TreeView tv, TreeStore store, int personID) {
		ArrayList myEvents;
		myEvents = SqlitePerson.SelectAllPersonEvents(personID); 

		foreach (string myEvent in myEvents) {
			string [] myStr = myEvent.Split(new char[] {':'});

			store.AppendValues (myStr[0], myStr[1], myStr[2], myStr[3], myStr[4], myStr[5], 
					myStr[6], myStr[7], myStr[8], myStr[9]);
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


