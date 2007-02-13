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
 * Xavier de Blas: 
 * http://www.xdeblas.com, http://www.deporteyciencia.com (parleblas)
 */

using System;
using Gtk;
using Glade;
using GLib; //for Value
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using Mono.Unix;


//load person (jumper)
public class PersonRecuperateWindow {
	
	[Widget] protected Gtk.Window person_recuperate;
	
	[Widget] protected Gtk.CheckButton checkbutton_sorted_by_creation_date;
	protected bool sortByCreationDate = false;
	
	protected TreeStore store;
	protected string selected;
	[Widget] protected Gtk.TreeView treeview_person_recuperate;
	[Widget] protected Gtk.Button button_recuperate;
	[Widget] protected Gtk.Statusbar statusbar1;
	[Widget] protected Gtk.Entry entry_search_filter;
	
	[Widget] protected Gtk.Box hbox_from_session_hide; //used in person recuperate multiple (hided in current class)
	[Widget] protected Gtk.Box hbox_search_filter_hide; //used in person recuperateWindow (hided in inherited class)
	
	static PersonRecuperateWindow PersonRecuperateWindowBox;
	
	protected Gtk.Window parent;

	protected int sessionID;
	
	protected Person currentPerson;
	
	protected PersonRecuperateWindow () {
	}

	PersonRecuperateWindow (Gtk.Window parent, int sessionID) {
		Glade.XML gladeXML;
		try {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "person_recuperate", null);
		} catch {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade.chronojump.glade", "person_recuperate", null);
		}

		gladeXML.Autoconnect(this);
		this.parent = parent;

		this.sessionID = sessionID;
	
		//no posible to recuperate until one person is selected
		button_recuperate.Sensitive = false;
	
		hbox_from_session_hide.Hide(); //used in person recuperate multiple (hided in current class)
		
		createTreeView(treeview_person_recuperate, 0);
		store = new TreeStore( typeof (string), typeof (string), typeof (string), typeof (string), 
				typeof (string), typeof(string), typeof(string) );
		treeview_person_recuperate.Model = store;
		fillTreeView(treeview_person_recuperate, store, "");
	}
	
	static public PersonRecuperateWindow Show (Gtk.Window parent, int sessionID)
	{
		if (PersonRecuperateWindowBox == null) {
			PersonRecuperateWindowBox = new PersonRecuperateWindow (parent, sessionID);
		}
		PersonRecuperateWindowBox.person_recuperate.Show ();
		
		return PersonRecuperateWindowBox;
	}
	
	protected void createTreeView (Gtk.TreeView tv, int count) {
		tv.HeadersVisible=true;
		tv.AppendColumn ( Catalog.GetString("ID"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString("Name"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString("Sex"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString("Height"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString("Weight"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString("Date of Birth"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString("Description"), new CellRendererText(), "text", count++);
	}
	
	protected void fillTreeView (Gtk.TreeView tv, TreeStore store, string searchFilterName) {
		string [] myPersons;
		
		int except = sessionID;
		int inSession = -1;	//search persons for recuperating in all sessions
		string mySort = "name";
		if(sortByCreationDate) {
			mySort = "uniqueID";
		}
		myPersons = SqlitePerson.SelectAllPersonsRecuperable(mySort, except, inSession, searchFilterName); 
		
		
		foreach (string session in myPersons) {
			string [] myStringFull = session.Split(new char[] {':'});

			store.AppendValues (myStringFull[0], myStringFull[1], 
					getCorrectSex(myStringFull[2]), myStringFull[4], myStringFull[5],
					myStringFull[3], myStringFull[6]
					);
		}	

	}

	protected string getCorrectSex (string sex) 
	{
		if (sex == "M") return  Catalog.GetString("Man");
		//this "F" is in spanish, change in the future to "W"
		else if (sex == "F") return  Catalog.GetString ("Woman");
		else { 
			return ""; //PersonsRecuperateFromOtherSessionWindow should pass a "" for ALL PERSONS
		}
	}
	
	protected virtual void on_entry_search_filter_changed (object o, EventArgs args) {
		if(entry_search_filter.Text.ToString().Length > 0) {
			store = new TreeStore( typeof (string), typeof (string), typeof (string), typeof (string), 
					typeof (string), typeof(string), typeof(string) );
			treeview_person_recuperate.Model = store;

			fillTreeView(treeview_person_recuperate,store, entry_search_filter.Text.ToString());
			
			//unselect all and make button_recuperate unsensitive
			treeview_person_recuperate.Selection.UnselectAll();
			button_recuperate.Sensitive = false;
		}
	}
	
	protected virtual void on_checkbutton_sort_by_creation_date_clicked(object o, EventArgs args) {
		if (sortByCreationDate) { sortByCreationDate = false; }
		else { sortByCreationDate = true; }
		
		store = new TreeStore( typeof (string), typeof (string), typeof (string), typeof (string), 
				typeof (string), typeof(string), typeof(string) );
		treeview_person_recuperate.Model = store;
		
		fillTreeView(treeview_person_recuperate,store, entry_search_filter.Text.ToString());
			
		button_recuperate.Sensitive = false;
	}
	
	//puts a value in private member selected
	protected virtual void on_treeview_person_recuperate_cursor_changed (object o, EventArgs args)
	{
		TreeView tv = (TreeView) o;
		TreeModel model;
		TreeIter iter;
		selected = "-1";

		// you get the iter and the model if something is selected
		if (tv.Selection.GetSelected (out model, out iter)) {
			selected = (string) model.GetValue (iter, 0);

			//allow clicking button_recuperate
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
			SqlitePersonSession.Insert(Convert.ToInt32(selected), sessionID);
			currentPerson = SqlitePersonSession.PersonSelect(selected);

			store = new TreeStore( typeof (string), typeof (string), typeof (string), typeof (string), 
					typeof (string), typeof(string), typeof(string) );
			treeview_person_recuperate.Model = store;
		
			fillTreeView(treeview_person_recuperate,store, entry_search_filter.Text.ToString());
				
			statusbar1.Push( 1, Catalog.GetString("Loaded") + " " + currentPerson.Name );
		}
	}
	
	public Button Button_recuperate 
	{
		set {
			button_recuperate = value;	
		}
		get {
			return button_recuperate;
		}
	}
	
	public Person CurrentPerson 
	{
		get {
			return currentPerson;
		}
	}

}


//load person (jumper)
public class PersonsRecuperateFromOtherSessionWindow : PersonRecuperateWindow 
{
	static PersonsRecuperateFromOtherSessionWindow PersonsRecuperateFromOtherSessionWindowBox;
	
	[Widget] Gtk.Box hbox_combo_sessions;
	[Widget] Gtk.Combo combo_sessions;
	
	PersonsRecuperateFromOtherSessionWindow (Gtk.Window parent, int sessionID) {
		Glade.XML gladeXML;
		try {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "person_recuperate", null);
		} catch {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade.chronojump.glade", "person_recuperate", null);
		}

		gladeXML.Autoconnect(this);
		this.parent = parent;

	
		//this class doesn't allow to search by name
		hbox_search_filter_hide.Hide();
		
		this.sessionID = sessionID;
	
		createComboSessions();
		
		createCheckboxes(treeview_person_recuperate);
		createTreeView(treeview_person_recuperate, 1);
		
		store = new TreeStore( typeof (bool), typeof (string), typeof (string), typeof (string), typeof (string), 
				typeof (string), typeof(string), typeof(string) );
		treeview_person_recuperate.Model = store;
		
		string myText = combo_sessions.Entry.Text;
		if(myText != "") {
			string [] myStringFull = myText.Split(new char[] {':'});
			fillTreeView( treeview_person_recuperate, store, Convert.ToInt32(myStringFull[0]) );
		}

		//check if there are rows checked for having sensitive or not in recuperate button
		buttonRecuperateChangeSensitiveness();
		
	}

	static public new PersonsRecuperateFromOtherSessionWindow Show (Gtk.Window parent, int sessionID)
	{
		if (PersonsRecuperateFromOtherSessionWindowBox == null) {
			PersonsRecuperateFromOtherSessionWindowBox = 
				new PersonsRecuperateFromOtherSessionWindow (parent, sessionID);
		}
		PersonsRecuperateFromOtherSessionWindowBox.person_recuperate.Show ();
		
		return PersonsRecuperateFromOtherSessionWindowBox;
	}
	
	private void createComboSessions() {
		combo_sessions = new Combo ();

		bool commentsDisable = true;
		int sessionIdDisable = sessionID; //for not showing current session on the list
		combo_sessions.PopdownStrings = 
			SqliteSession.SelectAllSessionsSimple(commentsDisable, sessionIdDisable);

		combo_sessions.DisableActivate ();
		combo_sessions.Entry.Changed += new EventHandler (on_combo_sessions_changed);

		hbox_combo_sessions.PackStart(combo_sessions, true, true, 0);
		hbox_combo_sessions.ShowAll();
		
		combo_sessions.Sensitive = true;
	}
	
	private void on_combo_sessions_changed(object o, EventArgs args) {
		string myText = combo_sessions.Entry.Text;
		if(myText != "") {
			store = new TreeStore( typeof (bool), typeof (string), typeof (string), typeof (string), typeof (string), 
				typeof (string), typeof(string), typeof(string) );
			treeview_person_recuperate.Model = store;
			
			string [] myStringFull = myText.Split(new char[] {':'});

			//fill the treeview passing the uniqueID of selected session as the reference for loading persons
			fillTreeView( treeview_person_recuperate, store, Convert.ToInt32(myStringFull[0]) );
		}
	}
	
	void createCheckboxes(TreeView tv) 
	{
		CellRendererToggle rendererToggle = new CellRendererToggle ();
		rendererToggle.Xalign = 0.0f;
		GLib.Object ugly = (GLib.Object) rendererToggle;
		ugly.Data ["column"] = 0;
		rendererToggle.Toggled += new ToggledHandler (ItemToggled);
		rendererToggle.Activatable = true;
		rendererToggle.Active = true;

		TreeViewColumn column = new TreeViewColumn ("", rendererToggle, "active", 0);
		column.Sizing = TreeViewColumnSizing.Fixed;
		column.FixedWidth = 50;
		column.Clickable = true;
		tv.InsertColumn (column, 0);
	}

	void ItemToggled(object o, ToggledArgs args) {
		Console.WriteLine("Toggled");

		GLib.Object cellRendererToggle = (GLib.Object) o;
		int column = (int) cellRendererToggle.Data["column"];

		Gtk.TreeIter iter;
		if (store.GetIterFromString (out iter, args.Path))
		{
			bool val = (bool) store.GetValue (iter, column);
			Console.WriteLine ("toggled {0} with value {1}", args.Path, !val);

			if(args.Path == "0") {
				if (store.GetIterFirst(out iter)) {
					val = (bool) store.GetValue (iter, column);
					store.SetValue (iter, column, !val);
					while ( store.IterNext(ref iter) ){
						store.SetValue (iter, column, !val);
					}
				}
			} else {
				store.SetValue (iter, column, !val);
			}

			//check if there are rows checked for having sensitive or not in recuperate button
			buttonRecuperateChangeSensitiveness();
		}
	}

	
	protected void fillTreeView (Gtk.TreeView tv, TreeStore store, int inSession) 
	{
		string [] myPersons;
		
		int except = sessionID;
		string mySort = "name";
		if(sortByCreationDate) {
			mySort = "uniqueID";
		}
		myPersons = SqlitePerson.SelectAllPersonsRecuperable(mySort, except, inSession, ""); //"" is searchFilterName (not implemented on recuperate multiple)

		//add a string for first row (for checking or unchecking all)
		myPersons = addAllPersonsCheckboxName(myPersons);	
		 
		foreach (string session in myPersons) {
			string [] myStringFull = session.Split(new char[] {':'});

			store.AppendValues (true, myStringFull[0], myStringFull[1], 
					getCorrectSex(myStringFull[2]), myStringFull[4], myStringFull[5],
					myStringFull[3], myStringFull[6]
					);
		}
	}

	protected override void on_checkbutton_sort_by_creation_date_clicked(object o, EventArgs args) {
		if (sortByCreationDate) { sortByCreationDate = false; }
		else { sortByCreationDate = true; }
		
		string myText = combo_sessions.Entry.Text;
		if(myText != "") {
			store = new TreeStore( typeof (bool), typeof (string), typeof (string), typeof (string), typeof (string), 
				typeof (string), typeof(string), typeof(string) );
			treeview_person_recuperate.Model = store;
		
			string [] myStringFull = myText.Split(new char[] {':'});

			//fill the treeview passing the uniqueID of selected session as the reference for loading persons
			fillTreeView( treeview_person_recuperate, store, Convert.ToInt32(myStringFull[0]) );
		}
	}
	
	protected string [] addAllPersonsCheckboxName(string [] myPersons) {
		string [] myPersonsReturn = new string[myPersons.Length +1];
		int count = 0;
		myPersonsReturn [count ++] = ":" + Catalog.GetString("MARK ALL/NONE") + ": : : : : ";
		
		foreach (string session in myPersons) {
			myPersonsReturn [count ++] = session;
		}
		return myPersonsReturn;
	}
	
	protected override void on_treeview_person_recuperate_cursor_changed (object o, EventArgs args)
	{
		//unselect, because in this treeview the important it's what is checked on first row, and not the selected row
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
			rowChecked = (bool) store.GetValue (iter, 0);
			if(!rowChecked)
				while ( store.IterNext(ref iter) && !rowChecked) {
					rowChecked = (bool) store.GetValue (iter, 0);
				}
		if(rowChecked)
			button_recuperate.Sensitive = true;
		else
			button_recuperate.Sensitive = false;
		}
	}

	
	protected override void on_row_double_clicked (object o, Gtk.RowActivatedArgs args) {
		//don't do nothing
	}
	
	protected override void on_button_recuperate_clicked (object o, EventArgs args)
	{
		Gtk.TreeIter iter;
		
		int inserted = 0;
		bool val;
		int count = 0;
		int personID;
		if (store.GetIterFirst(out iter)) {
			//don't catch 0 value
			//val = (bool) store.GetValue (iter, 0);
			//Console.WriteLine("Row {0}, value {1}", count++, val);
			count ++;
			while ( store.IterNext(ref iter) ){
				val = (bool) store.GetValue (iter, 0);

				//if checkbox of person is true
				if(val) {
					//find the uniqueID of selected
					personID = Convert.ToInt32( treeview_person_recuperate.Model.GetValue(iter, 1) );
					//Console.WriteLine("Row {0}, value {1}, personID {2}", count++, val, personID);

					//insert in DB
					SqlitePersonSession.Insert(personID, sessionID);

					//assign person to currentPerson (last will be really the currentPerson
					currentPerson = SqlitePersonSession.PersonSelect(personID.ToString());

					inserted ++;
				}
				
			}
	
			if(inserted > 0) {
				//update the treeview (only one time)
				string myText = combo_sessions.Entry.Text;
				if(myText != "") {
					store = new TreeStore( typeof (bool), typeof (string), typeof (string), typeof (string), typeof (string), 
							typeof (string), typeof(string), typeof(string) );
					treeview_person_recuperate.Model = store;

					string [] myStringFull = myText.Split(new char[] {':'});

					//fill the treeview passing the uniqueID of selected session as the reference for loading persons
					fillTreeView( treeview_person_recuperate, store, Convert.ToInt32(myStringFull[0]) );
					
					if(inserted == 1)
						statusbar1.Push( 1, Catalog.GetString("Loaded") + " " + currentPerson.Name );
					else //more inserted
						statusbar1.Push( 1, string.Format(Catalog.GetString("Successfully added {0} persons"), inserted));
						
				}
			}
		}
	}
}

//new person (jumper)
public class PersonAddWindow {
	
	[Widget] Gtk.Window person_win;
	[Widget] Gtk.Entry entry1;
	[Widget] Gtk.RadioButton radiobutton_man;
	[Widget] Gtk.RadioButton radiobutton_woman;

	[Widget] Gtk.Label label_date;
	[Widget] Gtk.Button button_change_date;

	[Widget] Gtk.TextView textview2;
	[Widget] Gtk.SpinButton spinbutton_height;
	[Widget] Gtk.SpinButton spinbutton_weight;
	
	[Widget] Gtk.Button button_accept;
	
	static PersonAddWindow PersonAddWindowBox;
	Gtk.Window parent;
	ErrorWindow errorWin;

	DialogCalendar myDialogCalendar;
	DateTime dateTime;

	private Person currentPerson;
	private int sessionID;
	private string sex = "M";
	
	PersonAddWindow (Gtk.Window parent, int sessionID) {
		Glade.XML gladeXML;
		try {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "person_win", null);
		} catch {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade.chronojump.glade", "person_win", null);
		}

		gladeXML.Autoconnect(this);
		this.parent = parent;
		this.sessionID = sessionID;
		button_accept.Sensitive = false; //only make sensitive when required values are inserted

		dateTime = DateTime.Today;
		label_date.Text = dateTime.ToLongDateString();
		
		person_win.Title =  Catalog.GetString ("New jumper");
	}
	
	void on_entries_required_changed (object o, EventArgs args)
	{
		if(entry1.Text.ToString().Length > 0 && (int) spinbutton_weight.Value > 0) {
			button_accept.Sensitive = true;
		}
		else {
			button_accept.Sensitive = false;
		}
	}
		
	void on_radiobutton_man_toggled (object o, EventArgs args)
	{
		sex = "M";
	}
	
	void on_radiobutton_woman_toggled (object o, EventArgs args)
	{
		sex = "F";
	}
	
	static public PersonAddWindow Show (Gtk.Window parent, int sessionID)
	{
		if (PersonAddWindowBox == null) {
			PersonAddWindowBox = new PersonAddWindow (parent, sessionID);
		}
		PersonAddWindowBox.person_win.Show ();
		
		return PersonAddWindowBox;
	}
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		PersonAddWindowBox.person_win.Hide();
		PersonAddWindowBox = null;
	}
	
	void on_person_win_delete_event (object o, DeleteEventArgs args)
	{
		PersonAddWindowBox.person_win.Hide();
		PersonAddWindowBox = null;
	}
	
	
	void on_button_change_date_clicked (object o, EventArgs args)
	{
		myDialogCalendar = new DialogCalendar(Catalog.GetString("Select Date of Birth"));
		myDialogCalendar.FakeButtonDateChanged.Clicked += new EventHandler(on_calendar_changed);
	}

	void on_calendar_changed (object obj, EventArgs args)
	{
		dateTime = myDialogCalendar.MyDateTime;
		label_date.Text = dateTime.ToLongDateString();
	}


	void on_button_accept_clicked (object o, EventArgs args)
	{
		//separate by '/' for not confusing with the ':' separation between the other values
		string dateFull = dateTime.Day.ToString() + "/" + dateTime.Month.ToString() + "/" +
			dateTime.Year.ToString();
		
		bool personExists = SqlitePersonSession.PersonExists (Util.RemoveTilde(entry1.Text));
		if(personExists) {
			//string myString =  Catalog.GetString ("Jumper: '") + Util.RemoveTilde(entry1.Text) +  Catalog.GetString ("' exists. Please, use another name");
			string myString = string.Format(Catalog.GetString("Person: '{0}' exists. Please, use another name"), Util.RemoveTildeAndColonAndDot(entry1.Text) );
			errorWin = ErrorWindow.Show(person_win, myString);
		} else {
			currentPerson = new Person (entry1.Text, sex, dateFull, (int) spinbutton_height.Value,
						(int) spinbutton_weight.Value, textview2.Buffer.Text, sessionID);
		
			PersonAddWindowBox.person_win.Hide();
			PersonAddWindowBox = null;
		}
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
	
	public Person CurrentPerson 
	{
		get {
			return currentPerson;
		}
	}

}

public class PersonModifyWindow
{
	
	[Widget] Gtk.Window person_win;
	[Widget] Gtk.Entry entry1;
	[Widget] Gtk.RadioButton radiobutton_man;
	[Widget] Gtk.RadioButton radiobutton_woman;
	[Widget] Gtk.TextView textview2;
	
	[Widget] Gtk.Label label_date;
	[Widget] Gtk.Button button_change_date;

	[Widget] Gtk.SpinButton spinbutton_height;
	[Widget] Gtk.SpinButton spinbutton_weight;
	
	[Widget] Gtk.Button button_accept;
	
	static PersonModifyWindow PersonModifyWindowBox;
	Gtk.Window parent;
	ErrorWindow errorWin;
	
	DialogCalendar myDialogCalendar;
	DateTime dateTime;

	private Person currentPerson;
	private int sessionID;
	private int uniqueID;
	private string sex = "M";
	
	
	PersonModifyWindow (Gtk.Window parent, int sessionID) {
		Glade.XML gladeXML;
		try {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "person_win", null);
		} catch {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade.chronojump.glade", "person_win", null);
		}
		
		gladeXML.Autoconnect(this);
		this.parent = parent;
		this.sessionID = sessionID;

		person_win.Title =  Catalog.GetString ("Edit jumper");
	}
	
	void on_entries_required_changed (object o, EventArgs args)
	{
		if(entry1.Text.ToString().Length > 0 && (int) spinbutton_weight.Value > 0) {
			button_accept.Sensitive = true;
		}
		else {
			button_accept.Sensitive = false;
		}
	}
		
	void on_radiobutton_man_toggled (object o, EventArgs args)
	{
		sex = "M";
	}
	
	void on_radiobutton_woman_toggled (object o, EventArgs args)
	{
		sex = "F";
	}
	
	//static public PersonModifyWindow Show (Gtk.Window parent, int sessionID)
	static public PersonModifyWindow Show (Gtk.Window parent, int sessionID, int personID)
	{
		if (PersonModifyWindowBox == null) {
			PersonModifyWindowBox = new PersonModifyWindow (parent, sessionID);
		}
		PersonModifyWindowBox.person_win.Show ();
		
		PersonModifyWindowBox.fillDialog (personID);
		
		return PersonModifyWindowBox;
	}

	private void fillDialog (int personID)
	{
		Person myPerson = SqlitePersonSession.PersonSelect(personID.ToString()); 
		
		entry1.Text = myPerson.Name;
		if (myPerson.Sex == "M") {
			radiobutton_man.Active = true;
		} else {
			radiobutton_woman.Active = true;
		}

		label_date.Text = myPerson.DateLong;
		
		spinbutton_height.Value = myPerson.Height;
		spinbutton_weight.Value = myPerson.Weight;

		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.SetText(myPerson.Description);
		textview2.Buffer = tb;
			
		uniqueID = personID;
	}
		
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		PersonModifyWindowBox.person_win.Hide();
		PersonModifyWindowBox = null;
	}
	
	//void on_person_modify_delete_event (object o, EventArgs args)
	void on_person_win_delete_event (object o, DeleteEventArgs args)
	{
		PersonModifyWindowBox.person_win.Hide();
		PersonModifyWindowBox = null;
	}
	
	
	void on_button_change_date_clicked (object o, EventArgs args)
	{
		myDialogCalendar = new DialogCalendar(Catalog.GetString("Select session date"));
		myDialogCalendar.FakeButtonDateChanged.Clicked += new EventHandler(on_calendar_changed);
	}

	void on_calendar_changed (object obj, EventArgs args)
	{
		dateTime = myDialogCalendar.MyDateTime;
		label_date.Text = dateTime.ToLongDateString();
	}

	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		bool personExists = SqlitePersonSession.PersonExistsAndItsNotMe (uniqueID, Util.RemoveTilde(entry1.Text));
		if(personExists) {
			//string myString =  Catalog.GetString ("Jumper: '") + Util.RemoveTilde(entry1.Text) +  Catalog.GetString ("' exists. Please, use another name");
			string myString = string.Format(Catalog.GetString("Person: '{0}' exists. Please, use another name"), Util.RemoveTildeAndColonAndDot(entry1.Text) );
			errorWin = ErrorWindow.Show(person_win, myString);
		} else {
			//separate by '/' for not confusing with the ':' separation between the other values
			string dateFull = dateTime.Day.ToString() + "/" + dateTime.Month.ToString() + "/" +
				dateTime.Year.ToString();
			
			currentPerson = new Person (uniqueID, entry1.Text, sex, dateFull, (int) spinbutton_height.Value,
						(int) spinbutton_weight.Value, textview2.Buffer.Text);

			SqlitePerson.Update (currentPerson); 
		
			PersonModifyWindowBox.person_win.Hide();
			PersonModifyWindowBox = null;
		}
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
	
	public Person CurrentPerson 
	{
		get {
			return currentPerson;
		}
	}
	
}

//new persons multiple (10)
public class PersonAddMultipleWindow {
	
	[Widget] Gtk.Window person_add_multiple;
	
	[Widget] Gtk.Entry entry1;
	[Widget] Gtk.Entry entry2;
	[Widget] Gtk.Entry entry3;
	[Widget] Gtk.Entry entry4;
	[Widget] Gtk.Entry entry5;
	[Widget] Gtk.Entry entry6;
	[Widget] Gtk.Entry entry7;
	[Widget] Gtk.Entry entry8;
	[Widget] Gtk.Entry entry9;
	[Widget] Gtk.Entry entry10;
	
	[Widget] Gtk.RadioButton r_1_m;
	[Widget] Gtk.RadioButton r_1_f;
	[Widget] Gtk.RadioButton r_2_m;
	[Widget] Gtk.RadioButton r_2_f;
	[Widget] Gtk.RadioButton r_3_m;
	[Widget] Gtk.RadioButton r_3_f;
	[Widget] Gtk.RadioButton r_4_m;
	[Widget] Gtk.RadioButton r_4_f;
	[Widget] Gtk.RadioButton r_5_m;
	[Widget] Gtk.RadioButton r_5_f;
	[Widget] Gtk.RadioButton r_6_m;
	[Widget] Gtk.RadioButton r_6_f;
	[Widget] Gtk.RadioButton r_7_m;
	[Widget] Gtk.RadioButton r_7_f;
	[Widget] Gtk.RadioButton r_8_m;
	[Widget] Gtk.RadioButton r_8_f;
	[Widget] Gtk.RadioButton r_9_m;
	[Widget] Gtk.RadioButton r_9_f;
	[Widget] Gtk.RadioButton r_10_m;
	[Widget] Gtk.RadioButton r_10_f;

	[Widget] Gtk.SpinButton spinbutton1;
	[Widget] Gtk.SpinButton spinbutton2;
	[Widget] Gtk.SpinButton spinbutton3;
	[Widget] Gtk.SpinButton spinbutton4;
	[Widget] Gtk.SpinButton spinbutton5;
	[Widget] Gtk.SpinButton spinbutton6;
	[Widget] Gtk.SpinButton spinbutton7;
	[Widget] Gtk.SpinButton spinbutton8;
	[Widget] Gtk.SpinButton spinbutton9;
	[Widget] Gtk.SpinButton spinbutton10;
	
	[Widget] Gtk.Button button_accept;
	
	static PersonAddMultipleWindow PersonAddMultipleWindowBox;
	Gtk.Window parent;
	ErrorWindow errorWin;

	private Person currentPerson;
	int sessionID;
	int personsCreatedCount;
	string errorExistsString;
	string errorWeightString;
	string errorRepeatedEntryString;
	
	PersonAddMultipleWindow (Gtk.Window parent, int sessionID) {
		Glade.XML gladeXML;
		try {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "person_add_multiple", null);
		} catch {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade.chronojump.glade", "person_add_multiple", null);
		}

		gladeXML.Autoconnect(this);
		this.parent = parent;
		this.sessionID = sessionID;
	}
	
	static public PersonAddMultipleWindow Show (Gtk.Window parent, int sessionID)
	{
		if (PersonAddMultipleWindowBox == null) {
			PersonAddMultipleWindowBox = new PersonAddMultipleWindow (parent, sessionID);
		}
		PersonAddMultipleWindowBox.person_add_multiple.Show ();
		
		return PersonAddMultipleWindowBox;
	}
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		PersonAddMultipleWindowBox.person_add_multiple.Hide();
		PersonAddMultipleWindowBox = null;
	}
	
	void on_delete_event (object o, DeleteEventArgs args)
	{
		PersonAddMultipleWindowBox.person_add_multiple.Hide();
		PersonAddMultipleWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		errorExistsString = "";
		errorWeightString = "";
		errorRepeatedEntryString = "";
		personsCreatedCount = 0;

		int count = 1;
		checkEntries(count++, entry1.Text.ToString(), (int) spinbutton1.Value);
		checkEntries(count++, entry2.Text.ToString(), (int) spinbutton2.Value);
		checkEntries(count++, entry3.Text.ToString(), (int) spinbutton3.Value);
		checkEntries(count++, entry4.Text.ToString(), (int) spinbutton4.Value);
		checkEntries(count++, entry5.Text.ToString(), (int) spinbutton5.Value);
		checkEntries(count++, entry6.Text.ToString(), (int) spinbutton6.Value);
		checkEntries(count++, entry7.Text.ToString(), (int) spinbutton7.Value);
		checkEntries(count++, entry8.Text.ToString(), (int) spinbutton8.Value);
		checkEntries(count++, entry9.Text.ToString(), (int) spinbutton9.Value);
		checkEntries(count++, entry10.Text.ToString(), (int) spinbutton10.Value);
	
		checkAllEntriesAreDifferent();

		string combinedErrorString = "";
		combinedErrorString = readErrorStrings();
		
		if (combinedErrorString.Length > 0) {
			errorWin = ErrorWindow.Show(person_add_multiple, combinedErrorString);
		} else {
			prepareAllNonBlankRows();
		
			PersonAddMultipleWindowBox.person_add_multiple.Hide();
			PersonAddMultipleWindowBox = null;
		}
	}
		
	void checkEntries(int count, string name, int weight) {
		if(name.Length > 0) {
			bool personExists = SqlitePersonSession.PersonExists (Util.RemoveTilde(name));
			if(personExists) {
				errorExistsString += "[" + count + "] " + name + "\n";
			}
			if(weight == 0) {
				errorWeightString += "[" + count + "] " + name + "\n";
			}
		}
	}
		
	void checkAllEntriesAreDifferent() {
		ArrayList newNames= new ArrayList();
		newNames.Add(entry1.Text.ToString());
		newNames.Add(entry2.Text.ToString());
		newNames.Add(entry3.Text.ToString());
		newNames.Add(entry4.Text.ToString());
		newNames.Add(entry5.Text.ToString());
		newNames.Add(entry6.Text.ToString());
		newNames.Add(entry7.Text.ToString());
		newNames.Add(entry8.Text.ToString());
		newNames.Add(entry9.Text.ToString());
		newNames.Add(entry10.Text.ToString());

		for(int i=0; i<10; i++) {
			bool repeated = false;
			if(Util.RemoveTilde(newNames[i].ToString()).Length > 0) {
				int j;
				for(j=i+1; j<10 && !repeated; j++) {
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
		
		if( entry10.Text.ToString().Length > 0 ) { 
			insertPerson (entry10.Text.ToString(), r_10_m.Active, (int) spinbutton10.Value);
		}
		if( entry9.Text.ToString().Length > 0 ) { 
			insertPerson (entry9.Text.ToString(), r_9_m.Active, (int) spinbutton9.Value);
		}
		if( entry8.Text.ToString().Length > 0 ) { 
			insertPerson (entry8.Text.ToString(), r_8_m.Active, (int) spinbutton8.Value);
		}
		if( entry7.Text.ToString().Length > 0 ) { 
			insertPerson (entry7.Text.ToString(), r_7_m.Active, (int) spinbutton7.Value);
		}
		if( entry6.Text.ToString().Length > 0 ) { 
			insertPerson (entry6.Text.ToString(), r_6_m.Active, (int) spinbutton6.Value);
		}
		if( entry5.Text.ToString().Length > 0 ) { 
			insertPerson (entry5.Text.ToString(), r_5_m.Active, (int) spinbutton5.Value);
		}
		if( entry4.Text.ToString().Length > 0 ) { 
			insertPerson (entry4.Text.ToString(), r_4_m.Active, (int) spinbutton4.Value);
		}
		if( entry3.Text.ToString().Length > 0 ) { 
			insertPerson (entry3.Text.ToString(), r_3_m.Active, (int) spinbutton3.Value);
		}
		if( entry2.Text.ToString().Length > 0 ) { 
			insertPerson (entry2.Text.ToString(), r_2_m.Active, (int) spinbutton2.Value);
		}
		if( entry1.Text.ToString().Length > 0 ) { 
			insertPerson (entry1.Text.ToString(), r_1_m.Active, (int) spinbutton1.Value);
		}
	}

	void insertPerson (string name, bool male, int weight) 
	{
		string sex = "F";
		if(male) { sex = "M"; }
		
		currentPerson = new Person ( name, sex, "0/0/1900", 
				0, weight, 		//height, weight	
				"", sessionID		//description, sessionID
				);

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
	[Widget] Gtk.Combo combo_persons;
	
	static PersonShowAllEventsWindow PersonShowAllEventsWindowBox;
	
	protected Gtk.Window parent;

	protected int sessionID;
	
	protected Person currentPerson;
	
	PersonShowAllEventsWindow (Gtk.Window parent, int sessionID, Person currentPerson) {
		Glade.XML gladeXML;
		try {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "person_show_all_events", null);
		} catch {
			gladeXML = Glade.XML.FromAssembly ("chronojump.glade.chronojump.glade", "person_show_all_events", null);
		}

		gladeXML.Autoconnect(this);
		this.parent = parent;

		this.sessionID = sessionID;
		this.currentPerson = currentPerson;
	
		createComboPersons(sessionID, currentPerson.UniqueID.ToString(), currentPerson.Name);
		createTreeView(treeview_person_show_all_events);
		store = new TreeStore( typeof (string), typeof (string), typeof (string), typeof (string), 
				typeof (string), typeof(string), typeof(string), typeof(string) );
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
		combo_persons = new Combo ();

		int inSession = -1;		//select persons from all sessions
		if(checkbutton_only_current_session.Active) {
			inSession = sessionID;	//select only persons who are on currentSession
		}
		string [] myPersons = SqlitePerson.SelectAllPersonsRecuperable("name", -1, inSession, ""); //"" is searchFilterName (not implemented on PersonShowAllEventsWindow)

		//put only id and name in combo
		string [] myPersons2 = new string[myPersons.Length];
		int count = 0;
		foreach (string person in myPersons) {
			string [] myStr = person.Split(new char[] {':'});
			myPersons2[count++] = myStr[0] + ":" + myStr[1];
		}
		combo_persons.PopdownStrings = myPersons2; 

		//selected is current person
		foreach (string person in myPersons2) {
			if (person == personID + ":" + personName) {
				combo_persons.Entry.Text = person;
			}
		}

		combo_persons.DisableActivate ();
		combo_persons.Entry.Changed += new EventHandler (on_combo_persons_changed);

		hbox_combo_persons.PackStart(combo_persons, true, true, 0);
		hbox_combo_persons.ShowAll();
		
		combo_persons.Sensitive = true;
	}
	
	private void on_combo_persons_changed(object o, EventArgs args) {
		string myText = combo_persons.Entry.Text;
		if(myText != "") {
			store = new TreeStore( typeof (string), typeof (string), typeof (string), typeof (string), 
					typeof (string), typeof(string), typeof(string), typeof(string) );
			treeview_person_show_all_events.Model = store;
			
			string [] myStringFull = myText.Split(new char[] {':'});

			fillTreeView( treeview_person_show_all_events, store, Convert.ToInt32(myStringFull[0]) );
		}
	}
	
	protected void on_checkbutton_only_current_session_clicked(object o, EventArgs args) {
		string myText = combo_persons.Entry.Text;
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
		tv.AppendColumn ( Catalog.GetString ("Pulses"), new CellRendererText(), "text", count++);
	}
	
	protected void fillTreeView (Gtk.TreeView tv, TreeStore store, int personID) {
		ArrayList myEvents;
		//myEvents = SqlitePerson.SelectAllPersonEvents(currentPerson.UniqueID); 
		myEvents = SqlitePerson.SelectAllPersonEvents(personID); 

		foreach (string myEvent in myEvents) {
			string [] myStr = myEvent.Split(new char[] {':'});

			store.AppendValues (myStr[0], myStr[1], myStr[2], myStr[3], myStr[4], myStr[5], myStr[6], myStr[7]);
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

