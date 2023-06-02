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
using Gtk;
//using Glade;
using System.Collections; //ArrayList
using System.Collections.Generic; //List
using Mono.Unix;


//load person
public class PersonRecuperateWindow
{
	protected Gtk.Window person_recuperate;

	protected Gtk.Label label_top;
	protected Gtk.Label label_from_session;
	protected Gtk.Label label_check;
	protected Gtk.Label label_filter;
	protected Gtk.Label label_feedback;
	
	protected Gtk.CheckButton checkbutton_sorted_by_creation_date;
	
	protected Gtk.TreeView treeview_person_recuperate;
	protected Gtk.Button button_recuperate;
	protected Gtk.Entry entry_search_filter;
	
	protected Gtk.Box hbox_from_session_hide; //used in person recuperate multiple (hided in current class)
	protected Gtk.Box hbox_combo_select_checkboxes_hide; //used in person recuperate multiple (hided in current class)
	protected Gtk.Box hbox_search_filter_hide; //used in person recuperateWindow (hided in inherited class)
	
	protected TreeStore store;
	protected string selected;

	static PersonRecuperateWindow PersonRecuperateWindowBox;

	protected Gtk.Window parent;
	
	protected Person currentPerson;
	protected Session currentSession;
	protected PersonSession currentPersonSession;

	protected int columnId = 0;
	protected int firstColumn = 0;
	protected int pDN;
	
	protected Gtk.Button fakeButtonDone;
	protected Gtk.Button fakeButtonCancel;

	protected PersonRecuperateWindow () {
	}

	PersonRecuperateWindow (Gtk.Window parent, Session currentSession)
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "person_recuperate.glade", "person_recuperate", null);
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "person_recuperate.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);

		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(person_recuperate);

		//manage window color
		if(! Config.UseSystemColor)
		{
			UtilGtk.WindowColor(person_recuperate, Config.ColorBackground);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, label_top);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, label_from_session);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, label_check);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, label_filter);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, label_feedback);
		}

		this.currentSession = currentSession;
		
		fakeButtonDone = new Gtk.Button();
		fakeButtonCancel = new Gtk.Button();
	
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
	public int heightColumnCompare (ITreeModel model, TreeIter iter1, TreeIter iter2)     {
		double val1 = 0;
		double val2 = 0;
		val1 = Convert.ToDouble(model.GetValue(iter1, firstColumn + 3));
		val2 = Convert.ToDouble(model.GetValue(iter2, firstColumn + 3));
		
		return (int) (10*val1-10*val2);
	}

	//cannot be double
	public int weightColumnCompare (ITreeModel model, TreeIter iter1, TreeIter iter2)     {
		double val1 = 0;
		double val2 = 0;
		val1 = Convert.ToDouble(model.GetValue(iter1, firstColumn + 4));
		val2 = Convert.ToDouble(model.GetValue(iter2, firstColumn + 4));
		
		return (int) (10*val1-10*val2);
	}
*/

	private void fillTreeView (Gtk.TreeView tv, TreeStore store, string searchFilterName) 
	{
		int exceptSession = currentSession.UniqueID;
		int inSession = -1;	//search persons for recuperating in all sessions
		ArrayList myPersons = SqlitePerson.SelectAllPersonsRecuperable("name", exceptSession, inSession, searchFilterName);
		
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
		if (sex == Constants.SexU)
			return Catalog.GetString ("Unspecified");
		else if (sex == Constants.SexM)
			return Catalog.GetString ("Man");
		else if (sex == Constants.SexF)
			return Catalog.GetString ("Woman");
		else
			return ""; //PersonsRecuperateFromOtherSessionWindow should pass a "" for ALL PERSONS
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
		ITreeModel model;
		TreeIter iter;
		selected = "-1";

		if (((TreeSelection)o).GetSelected(out model, out iter))
		{
			selected = (string)model.GetValue (iter, 0);
			button_recuperate.Sensitive = true;
		}
	}

	//if called from personRecuperateWindow
	public virtual void HideAndNull()
	{
		if(PersonRecuperateWindowBox != null)
			PersonRecuperateWindowBox.person_recuperate.Hide();

		if(PersonRecuperateWindowBox != null)
			PersonRecuperateWindowBox = null;
	}
	
	protected virtual void on_button_close_clicked (object o, EventArgs args)
	{
		fakeButtonCancel.Click (); //managed if persons in top win

		PersonRecuperateWindowBox.person_recuperate.Hide();
		PersonRecuperateWindowBox = null;
	}
	
	protected virtual void on_person_recuperate_delete_event (object o, DeleteEventArgs args)
	{
		fakeButtonCancel.Click (); //managed if persons in top win

		PersonRecuperateWindowBox.person_recuperate.Hide();
		PersonRecuperateWindowBox = null;
	}
	
	protected virtual void on_row_double_clicked (object o, Gtk.RowActivatedArgs args)
	{
		TreeView tv = (TreeView) o;
		ITreeModel model;
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
					myPS.TrochanterToe,
					myPS.TrochanterFloorOnFlexion,
					false); //dbconOpened
						
			store = new TreeStore( typeof (string), typeof (string), typeof (string), typeof (string), typeof (string) );
			treeview_person_recuperate.Model = store;

			fillTreeView(treeview_person_recuperate,store, entry_search_filter.Text.ToString());

			label_feedback.Text = Catalog.GetString("Loaded") + " " + currentPerson.Name;

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

	public Button FakeButtonCancel
	{
		get { return fakeButtonCancel; }
	}

	
	public Person CurrentPerson {
		get { return currentPerson; }
	}
	public PersonSession CurrentPersonSession {
		get { return currentPersonSession; }
	}


	protected void connectWidgets (Gtk.Builder builder)
	{
		person_recuperate = (Gtk.Window) builder.GetObject ("person_recuperate");
		label_top = (Gtk.Label) builder.GetObject ("label_top");
		label_from_session = (Gtk.Label) builder.GetObject ("label_from_session");
		label_check = (Gtk.Label) builder.GetObject ("label_check");
		label_filter = (Gtk.Label) builder.GetObject ("label_filter");
		label_feedback = (Gtk.Label) builder.GetObject ("label_feedback");
		checkbutton_sorted_by_creation_date = (Gtk.CheckButton) builder.GetObject ("checkbutton_sorted_by_creation_date");
		treeview_person_recuperate = (Gtk.TreeView) builder.GetObject ("treeview_person_recuperate");
		button_recuperate = (Gtk.Button) builder.GetObject ("button_recuperate");
		entry_search_filter = (Gtk.Entry) builder.GetObject ("entry_search_filter");
		hbox_from_session_hide = (Gtk.Box) builder.GetObject ("hbox_from_session_hide"); //used in person recuperate multiple (hided in current class)
		hbox_combo_select_checkboxes_hide = (Gtk.Box) builder.GetObject ("hbox_combo_select_checkboxes_hide"); //used in person recuperate multiple (hided in current class)
		hbox_search_filter_hide = (Gtk.Box) builder.GetObject ("hbox_search_filter_hide"); //used in person recuperateWindow (hided in inherited class)
	}

}


//load person
public class PersonsRecuperateFromOtherSessionWindow : PersonRecuperateWindow 
{
	Gtk.Box hbox_combo_sessions;
	Gtk.ComboBoxText combo_sessions;
	protected Gtk.Box hbox_combo_select_checkboxes;
	protected Gtk.ComboBoxText combo_select_checkboxes;
	
	static PersonsRecuperateFromOtherSessionWindow PersonsRecuperateFromOtherSessionWindowBox;
	
	
	protected static string [] comboCheckboxesOptions = {
		Catalog.GetString("All"),
		Catalog.GetString("None"),
		Catalog.GetString("Selected"),
	};
	
	protected PersonsRecuperateFromOtherSessionWindow () {
	}

	PersonsRecuperateFromOtherSessionWindow (Gtk.Window parent, Session currentSession) {
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "person_recuperate.glade", "person_recuperate", null);
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "person_recuperate.glade", null);
		connectWidgets (builder);
		connectWidgetsFromOtherSession (builder);
		builder.Autoconnect (this);

		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(person_recuperate);

		//manage window color
		if(! Config.UseSystemColor)
		{
			UtilGtk.WindowColor(person_recuperate, Config.ColorBackground);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, label_top);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, label_from_session);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, label_check);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, label_filter);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, label_feedback);
		}

		person_recuperate.Title = Catalog.GetString("Load persons from other session");

	
		//this class doesn't allow to search by name
		hbox_search_filter_hide.Hide();
		
		this.currentSession = currentSession;
		
		fakeButtonDone = new Gtk.Button();
		fakeButtonCancel = new Gtk.Button();

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
		combo_sessions = new ComboBoxText();

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
		combo_select_checkboxes = new ComboBoxText ();
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

	
	private void fillTreeView (Gtk.TreeView tv, TreeStore store, int exceptSession, int inSession)
	{
		ArrayList myPersons = SqlitePerson.SelectAllPersonsRecuperable("name", exceptSession, inSession, ""); //"" is searchFilterName (not implemented on recuperate multiple)

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
		fakeButtonCancel.Click (); //managed if persons in top win

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
								currentPersonSession.SpeciallityID, currentPersonSession.Practice, currentPersonSession.Comments,
								currentPersonSession.TrochanterToe, currentPersonSession.TrochanterFloorOnFlexion)
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
					label_feedback.Text = Catalog.GetString("Loaded") + " " + currentPerson.Name;
				else 
					label_feedback.Text = string.Format(Catalog.GetPluralString(
									"Successfully added one person.",
									"Successfully added {0} persons.",
									inserted),
								inserted);
				fakeButtonDone.Click();
			}
		}
	
		//check if there are rows checked for having sensitive or not in recuperate button
		buttonRecuperateChangeSensitiveness();
	}

	public override void HideAndNull()
	{
		if(PersonsRecuperateFromOtherSessionWindowBox != null)
			PersonsRecuperateFromOtherSessionWindowBox.person_recuperate.Hide();

		if(PersonsRecuperateFromOtherSessionWindowBox != null)
			PersonsRecuperateFromOtherSessionWindowBox = null;
	}

	protected void connectWidgetsFromOtherSession (Gtk.Builder builder)
	{
		hbox_combo_sessions = (Gtk.Box) builder.GetObject ("hbox_combo_sessions");
		combo_sessions = (Gtk.ComboBoxText) builder.GetObject ("combo_sessions");
		hbox_combo_select_checkboxes = (Gtk.Box) builder.GetObject ("hbox_combo_select_checkboxes");
		combo_select_checkboxes = (Gtk.ComboBoxText) builder.GetObject ("combo_select_checkboxes");
	}

}


//discard people to being uploadd to server
//inherits from PersonRecuperateFromOtherSession because uses same window
public class PersonNotUploadWindow : PersonsRecuperateFromOtherSessionWindow 
{
	Gtk.Button button_go_forward;
	Gtk.Button button_close;

	static PersonNotUploadWindow PersonNotUploadWindowBox;
	ArrayList initiallyUnchecked;
	
	private int sessionID;
	public new Gtk.Button fakeButtonDone;
	
	PersonNotUploadWindow (Gtk.Window parent, int sessionID)
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "person_recuperate.glade", "person_recuperate", null);
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "person_recuperate.glade", null);
		connectWidgets (builder);
		connectWidgetsFromOtherSession (builder);
		connectWidgetsNotUpload (builder);
		builder.Autoconnect (this);

		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(person_recuperate);

		//this class doesn't allow to search by name
		hbox_search_filter_hide.Hide();
		//this class doesn't use button recuperate
		button_recuperate.Hide();
		//this class doesn't use feedback on bottom
		label_feedback.Hide();
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

	private void connectWidgetsNotUpload (Gtk.Builder builder)
	{
		button_go_forward = (Gtk.Button) builder.GetObject ("button_go_forward");
		button_close = (Gtk.Button) builder.GetObject ("button_close");
	}
}

