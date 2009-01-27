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
	
	protected TreeStore store;
	protected string selected;
	private string selectedWeight;
	[Widget] protected Gtk.TreeView treeview_person_recuperate;
	[Widget] protected Gtk.Button button_recuperate;
	[Widget] protected Gtk.Statusbar statusbar1;
	[Widget] protected Gtk.Entry entry_search_filter;
	
	[Widget] protected Gtk.Box hbox_from_session_hide; //used in person recuperate multiple (hided in current class)
	[Widget] protected Gtk.Box hbox_combo_select_checkboxes_hide; //used in person recuperate multiple (hided in current class)
	[Widget] protected Gtk.Box hbox_search_filter_hide; //used in person recuperateWindow (hided in inherited class)
	
	static PersonRecuperateWindow PersonRecuperateWindowBox;

	protected Gtk.Window parent;
	protected int sessionID;
	
	protected Person currentPerson;

	protected int columnId = 0;
	protected int firstColumn = 0;

	protected PersonRecuperateWindow () {
	}

	PersonRecuperateWindow (Gtk.Window parent, int sessionID) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "person_recuperate", null);
		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(person_recuperate);

		this.sessionID = sessionID;
	
		//no posible to recuperate until one person is selected
		button_recuperate.Sensitive = false;
	
		hbox_from_session_hide.Hide(); //used in person recuperate multiple (hided in current class)
		hbox_combo_select_checkboxes_hide.Hide(); //used in person recuperate multiple (hided in current class)
		
		store = new TreeStore( typeof (string), typeof (string), typeof (string), typeof (string), 
				typeof (string), typeof(string), typeof(string), 
				typeof (string), typeof(string), typeof(string) );
		createTreeView(treeview_person_recuperate, 0);
		treeview_person_recuperate.Model = store;
		fillTreeView(treeview_person_recuperate, store, "");

		treeview_person_recuperate.Model = store;

		treeview_person_recuperate.Selection.Changed += onSelectionEntry;
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
		
		UtilGtk.CreateCols(tv, store, Catalog.GetString("ID"), count++);
		UtilGtk.CreateCols(tv, store, Catalog.GetString("Name"), count++);
		UtilGtk.CreateCols(tv, store, Catalog.GetString("Sex"), count++);
		UtilGtk.CreateCols(tv, store, Catalog.GetString("Height"), count++);
		UtilGtk.CreateCols(tv, store, Catalog.GetString("Weight"), count++);
		UtilGtk.CreateCols(tv, store, Catalog.GetString("Date of Birth"), count++);
		UtilGtk.CreateCols(tv, store, Catalog.GetString("Sport"), count++);
		UtilGtk.CreateCols(tv, store, Catalog.GetString("Speciallity"), count++);
		UtilGtk.CreateCols(tv, store, Catalog.GetString("Level"), count++);
		UtilGtk.CreateCols(tv, store, Catalog.GetString("Description"), count++);

		//sort non textual cols	
		store.SetSortFunc (firstColumn + 0, UtilGtk.IdColumnCompare);
		store.SetSortFunc (firstColumn + 3, heightColumnCompare);
		store.SetSortFunc (firstColumn + 4, weightColumnCompare);
		store.SetSortFunc (firstColumn + 5, birthColumnCompare);
	}
	
	public int heightColumnCompare (TreeModel model, TreeIter iter1, TreeIter iter2)     {
		int val1 = 0;
		int val2 = 0;
		val1 = Convert.ToInt32(model.GetValue(iter1, firstColumn + 3));
		val2 = Convert.ToInt32(model.GetValue(iter2, firstColumn + 3));
		
		return (val1-val2);
	}

	public int weightColumnCompare (TreeModel model, TreeIter iter1, TreeIter iter2)     {
		int val1 = 0;
		int val2 = 0;
		val1 = Convert.ToInt32(model.GetValue(iter1, firstColumn + 4));
		val2 = Convert.ToInt32(model.GetValue(iter2, firstColumn + 4));
		
		return (val1-val2);
	}

	public int birthColumnCompare (TreeModel model, TreeIter iter1, TreeIter iter2)     {
		DateTime val1; 
		DateTime val2; 
		val1 = Util.DateAsDateTime(model.GetValue(iter1, firstColumn + 5).ToString());
		val2 = Util.DateAsDateTime(model.GetValue(iter2, firstColumn + 5).ToString());
		
		return DateTime.Compare(val1, val2);
	}

	protected void fillTreeView (Gtk.TreeView tv, TreeStore store, string searchFilterName) {
		string [] myPersons;
		
		int except = sessionID;
		int inSession = -1;	//search persons for recuperating in all sessions
		myPersons = SqlitePerson.SelectAllPersonsRecuperable("name", except, inSession, searchFilterName); 
		
		
		foreach (string session in myPersons) {
			string [] myStringFull = session.Split(new char[] {':'});

			store.AppendValues (myStringFull[0], myStringFull[1], 
					getCorrectSex(myStringFull[2]), myStringFull[4], myStringFull[5],
					myStringFull[3], 
					myStringFull[6], //sport
					myStringFull[7], //speciallity
					myStringFull[8], //level (practice)
					myStringFull[9] //desc
					);
		}	
		
		//show sorted by column Name	
		store.SetSortColumnId(1, Gtk.SortType.Ascending);
		store.ChangeSortColumn();
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
		store = new TreeStore( typeof (string), typeof (string), typeof (string), typeof (string), 
				typeof (string), typeof(string), typeof(string),
				typeof (string), typeof(string), typeof(string) );
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
			selectedWeight = (string)model.GetValue (iter, 4);
			button_recuperate.Sensitive = true;
		}
		Log.WriteLine (selected + ":" + selectedWeight);
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
			selectedWeight = (string) model.GetValue (iter, 4);
			
			//activate on_button_recuperate_clicked()
			button_recuperate.Activate();
		}
	}
	
	protected virtual void on_button_recuperate_clicked (object o, EventArgs args)
	{
		if(selected != "-1")
		{
			SqlitePersonSession.Insert(Convert.ToInt32(selected), sessionID, Convert.ToInt32(selectedWeight));
			currentPerson = SqlitePersonSession.PersonSelect(Convert.ToInt32(selected), sessionID);

			store = new TreeStore( typeof (string), typeof (string), typeof (string), typeof (string), 
				typeof (string), typeof(string), typeof(string),
					typeof (string), typeof(string), typeof(string) );
			treeview_person_recuperate.Model = store;
		
			fillTreeView(treeview_person_recuperate,store, entry_search_filter.Text.ToString());
				
			statusbar1.Push( 1, Catalog.GetString("Loaded") + " " + currentPerson.Name );
		
			//no posible to recuperate until one person is selected
			button_recuperate.Sensitive = false;
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
	[Widget] Gtk.ComboBox combo_sessions;
	[Widget] Gtk.Box hbox_combo_select_checkboxes;
	[Widget] Gtk.ComboBox combo_select_checkboxes;
	
	
	private static string [] comboCheckboxesOptions = {
		Catalog.GetString("All"),
		Catalog.GetString("None"),
		Catalog.GetString("Selected"),
	};
	
	PersonsRecuperateFromOtherSessionWindow (Gtk.Window parent, int sessionID) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "person_recuperate", null);
		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		//put an icon to window
		UtilGtk.IconWindow(person_recuperate);

	
		//this class doesn't allow to search by name
		hbox_search_filter_hide.Hide();
		
		this.sessionID = sessionID;
	
		firstColumn = 1;
	
		createComboSessions();
		createComboSelectCheckboxes();
		createCheckboxes(treeview_person_recuperate);
		
		store = new TreeStore( typeof (bool), typeof (string), typeof (string), typeof (string), typeof (string), 
				typeof (string), typeof(string), typeof(string),
				typeof (string), typeof(string), typeof(string) );
		createTreeView(treeview_person_recuperate, 1);
		treeview_person_recuperate.Model = store;
		
		string myText = UtilGtk.ComboGetActive(combo_sessions);
		if(myText != "") {
			string [] myStringFull = myText.Split(new char[] {':'});
			fillTreeView( treeview_person_recuperate, store, Convert.ToInt32(myStringFull[0]) );
		}

		//no posible to recuperate until one person is selected
		button_recuperate.Sensitive = false;
	
		treeview_person_recuperate.Selection.Changed += onSelectionEntry;
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
		combo_sessions = ComboBox.NewText();

		bool commentsDisable = true;
		int sessionIdDisable = sessionID; //for not showing current session on the list
		UtilGtk.ComboUpdate(combo_sessions, SqliteSession.SelectAllSessionsSimple(commentsDisable, sessionIdDisable), "");

		combo_sessions.Changed += new EventHandler (on_combo_sessions_changed);

		hbox_combo_sessions.PackStart(combo_sessions, true, true, 0);
		hbox_combo_sessions.ShowAll();
		combo_sessions.Sensitive = true;
	}
	
	private void on_combo_sessions_changed(object o, EventArgs args) {
		string myText = UtilGtk.ComboGetActive(combo_sessions);
		if(myText != "") {
			store = new TreeStore( typeof (bool), typeof (string), typeof (string), typeof (string), typeof (string), 
				typeof (string), typeof(string), typeof(string),
				typeof (string), typeof(string), typeof(string) );
			treeview_person_recuperate.Model = store;
			
			string [] myStringFull = myText.Split(new char[] {':'});

			//fill the treeview passing the uniqueID of selected session as the reference for loading persons
			fillTreeView( treeview_person_recuperate, store, Convert.ToInt32(myStringFull[0]) );
		}
	
		//check if there are rows checked for having sensitive or not in recuperate button
		buttonRecuperateChangeSensitiveness();
	}
	
	private void createComboSelectCheckboxes() {
		combo_select_checkboxes = ComboBox.NewText ();
		UtilGtk.ComboUpdate(combo_select_checkboxes, comboCheckboxesOptions, "");
		
		//combo_select_checkboxes.DisableActivate ();
		combo_select_checkboxes.Changed += new EventHandler (on_combo_select_checkboxes_changed);

		hbox_combo_select_checkboxes.PackStart(combo_select_checkboxes, true, true, 0);
		hbox_combo_select_checkboxes.ShowAll();
		combo_select_checkboxes.Sensitive = true;
	}
	
	private void on_combo_select_checkboxes_changed(object o, EventArgs args) {
		string myText = UtilGtk.ComboGetActive(combo_select_checkboxes);
			
		if (myText != "" & myText != Catalog.GetString("Selected")) {
			try {
				markSelected(myText);
			} catch {
				Log.WriteLine("Do later!!");
			}
		}
	}
	
	public void markSelected(string selected) {
		Gtk.TreeIter iter;
		bool okIter = store.GetIterFirst(out iter);
		if(okIter) {
			if(selected == Catalog.GetString("All")) {
				do {
					//if(isNotAVGOrSD(iter)) {
						store.SetValue (iter, 0, true);
						//addRowToMarkedRows(treeview.Model.GetPath(iter).ToString());
					//}
				} while ( store.IterNext(ref iter) );
			} else if(selected == Catalog.GetString("None")) {
				do {
					store.SetValue (iter, 0, false);
					//deleteRowFromMarkedRows(treeview.Model.GetPath(iter).ToString());
				} while ( store.IterNext(ref iter) );
			}
		}
			
		//check if there are rows checked for having sensitive or not in recuperate button
		buttonRecuperateChangeSensitiveness();
	}
	
	
	void createCheckboxes(TreeView tv) 
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

	void ItemToggled(object o, ToggledArgs args) {
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

	
	protected void fillTreeView (Gtk.TreeView tv, TreeStore store, int inSession) 
	{
		string [] myPersons;
		
		int except = sessionID;
		myPersons = SqlitePerson.SelectAllPersonsRecuperable("name", except, inSession, ""); //"" is searchFilterName (not implemented on recuperate multiple)

		 
		foreach (string session in myPersons) {
			string [] myStringFull = session.Split(new char[] {':'});

			store.AppendValues (true, myStringFull[0], myStringFull[1], 
					getCorrectSex(myStringFull[2]), myStringFull[4], myStringFull[5],
					myStringFull[3], 
					myStringFull[6], //sport
					myStringFull[7], //speciallity
					myStringFull[8], //level (practice)
					myStringFull[9] //desc
					);
		}
		
		//show sorted by column Name	
		store.SetSortColumnId(2, Gtk.SortType.Ascending);
	}

	//protected override void on_treeview_person_recuperate_cursor_changed (object o, EventArgs args)
	protected override void onSelectionEntry (object o, EventArgs args)
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
			//Log.WriteLine("Row {0}, value {1}", count++, val);
			count ++;
			do {
				val = (bool) store.GetValue (iter, 0);

				//if checkbox of person is true
				if(val) {
					//find the uniqueID of selected
					personID = Convert.ToInt32( treeview_person_recuperate.Model.GetValue(iter, 1) );
					//Log.WriteLine("Row {0}, value {1}, personID {2}", count++, val, personID);

					//find weight
					string weightString = (string) store.GetValue (iter, 5);

					//insert in DB
					SqlitePersonSession.Insert(personID, sessionID, Convert.ToInt32(weightString));

					//assign person to currentPerson (last will be really the currentPerson
					currentPerson = SqlitePersonSession.PersonSelect(personID, sessionID);

					inserted ++;
				}
				
			} while ( store.IterNext(ref iter) );

			if(inserted > 0) {
				//update the treeview (only one time)
				string myText = UtilGtk.ComboGetActive(combo_sessions);
				if(myText != "") {
					store = new TreeStore( typeof (bool), typeof (string), typeof (string), typeof (string), typeof (string), 
				typeof (string), typeof(string), typeof(string),
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
		
		//check if there are rows checked for having sensitive or not in recuperate button
		buttonRecuperateChangeSensitiveness();
	}
}

public class PersonAddModifyWindow
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

	[Widget] Gtk.Button button_accept;
	
	//used for connect ok gui/chronojump.cs, this class and gui/convertWeight.cs
	public Gtk.Button fakeButtonAccept;
	
	static ConvertWeightWindow convertWeightWin;
	
	static PersonAddModifyWindow PersonAddModifyWindowBox;
	
	Gtk.Window parent;
	ErrorWindow errorWin;
	
	DialogCalendar myDialogCalendar;
	DateTime dateTime;
	Sport sport;
	string [] sports;
	string [] sportsTranslated;
	int speciallityID;
	string [] speciallities;
	string [] speciallitiesTranslated;
	String level;
	string [] levels;
	string [] continents;
	string [] continentsTranslated;
	string [] countries;
	string [] countriesTranslated;

	GenericWindow genericWin;

	bool adding;

	private Person currentPerson;
	private Session currentSession;
	private int personID;
	private string sex = "M";
	private int weightIni;
	
	private int serverUniqueID;
	
	//
	//if we are adding a person, personID it's -1
	//if we are modifying a person, personID is obviously it's ID
	PersonAddModifyWindow (Gtk.Window parent, Session currentSession, int personID) {
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "chronojump.glade", "person_win", null);
		gladeXML.Autoconnect(this);
		
		//put an icon to window
		UtilGtk.IconWindow(person_win);
	
		this.parent = parent;
		this.currentSession = currentSession;
		this.personID = personID;

		if(personID == -1)
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
			
		fakeButtonAccept = new Gtk.Button();

		if(adding) {
			person_win.Title = Catalog.GetString ("New jumper");
			button_accept.Sensitive = false;
		} else 
			person_win.Title = Catalog.GetString ("Edit jumper");
	}
	
	void on_entries_required_changed (object o, EventArgs args)
	{
		if(entry1.Text.ToString().Length > 0 && (int) spinbutton_weight.Value > 0 &&
				dateTime != DateTime.MinValue &&
				UtilGtk.ComboGetActive(combo_sports) != Catalog.GetString(Constants.SportUndefined) &&
				(! label_speciallity.Visible || UtilGtk.ComboGetActive(combo_speciallities) != Catalog.GetString(Constants.SpeciallityUndefined)) &&
				Util.FetchID(UtilGtk.ComboGetActive(combo_levels)) != Constants.LevelUndefinedID 
				//countries is not required to create a person here, but will be required for server
				//&& 
				//UtilGtk.ComboGetActive(combo_continents) != Catalog.GetString(Constants.ContinentUndefined) &&
				//UtilGtk.ComboGetActive(combo_countries) != Catalog.GetString(Constants.CountryUndefined)
				) 
		{
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
	
	static public PersonAddModifyWindow Show (Gtk.Window parent, Session mySession, int personID)
	{
		if (PersonAddModifyWindowBox == null) {
			PersonAddModifyWindowBox = new PersonAddModifyWindow (parent, mySession, personID);
		}
		PersonAddModifyWindowBox.person_win.Show ();
		
		PersonAddModifyWindowBox.fillDialog ();
		
		return PersonAddModifyWindowBox;
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
			//dateTime = DateTime.Today;
			//now dateTime is undefined until user changes it
			dateTime = DateTime.MinValue;
			label_date.Text = Catalog.GetString("Undefined");

			mySportID = currentSession.PersonsSportID;
			mySpeciallityID = currentSession.PersonsSpeciallityID;
			myLevelID = currentSession.PersonsPractice;
		} else {
			Person myPerson = SqlitePersonSession.PersonSelect(personID, currentSession.UniqueID); 
		
			entry1.Text = myPerson.Name;
			if (myPerson.Sex == "M") {
				radiobutton_man.Active = true;
			} else {
				radiobutton_woman.Active = true;
			}

			dateTime = Util.DateAsDateTime(myPerson.DateBorn);
			if(dateTime == DateTime.MinValue)
				label_date.Text = Catalog.GetString("Undefined");
			else
				label_date.Text = dateTime.ToLongDateString();

			spinbutton_height.Value = myPerson.Height;
			spinbutton_weight.Value = myPerson.Weight;
			weightIni = myPerson.Weight; //store for tracking if changes
		
			mySportID = myPerson.SportID;
			mySpeciallityID = myPerson.SpeciallityID;
			myLevelID = myPerson.Practice;


			TextBuffer tb = new TextBuffer (new TextTagTable());
			tb.Text = myPerson.Description;
			textview2.Buffer = tb;

			//country stuff
			if(myPerson.CountryID != Constants.CountryUndefinedID) {
				string [] countryString = SqliteCountry.Select(myPerson.CountryID);
				combo_continents.Active = UtilGtk.ComboMakeActive(continentsTranslated, 
						Catalog.GetString(countryString[3]));
				combo_countries.Active = UtilGtk.ComboMakeActive(countriesTranslated, 
						Catalog.GetString(countryString[1]));
			}

			serverUniqueID = myPerson.ServerUniqueID;
		}
			
		sport = SqliteSport.Select(mySportID);
		combo_sports.Active = UtilGtk.ComboMakeActive(sportsTranslated, sport.ToString());

		combo_speciallities.Active = UtilGtk.ComboMakeActive(speciallitiesTranslated, SqliteSpeciallity.Select(mySpeciallityID));

		combo_levels.Active = UtilGtk.ComboMakeActive(levels, myLevelID + ":" + Util.FindLevelName(myLevelID));
		
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
	
	
	void on_button_change_date_clicked (object o, EventArgs args)
	{
		myDialogCalendar = new DialogCalendar(Catalog.GetString("Select session date"));
		myDialogCalendar.FakeButtonDateChanged.Clicked += new EventHandler(on_calendar_changed);
	}

	void on_calendar_changed (object obj, EventArgs args)
	{
		dateTime = myDialogCalendar.MyDateTime;
		label_date.Text = dateTime.ToLongDateString();
		on_entries_required_changed(new object(), new EventArgs());
	}

	private void on_combo_sports_changed(object o, EventArgs args) {
		ComboBox combo = o as ComboBox;
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
		genericWin = GenericWindow.Show(Catalog.GetString("Add new sport to database"), true, false);
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
			personExists = SqlitePersonSession.PersonExistsAndItsNotMe (personID, Util.RemoveTilde(entry1.Text));

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
			if(!adding && (int) spinbutton_weight.Value != weightIni) {
				//see if this person has done jumps with weight
				string [] myJumpsNormal = SqliteJump.SelectJumps(currentSession.UniqueID, personID, "withWeight");
				string [] myJumpsReactive = SqliteJumpRj.SelectJumps(currentSession.UniqueID, personID, "withWeight");

				if(myJumpsNormal.Length > 0 || myJumpsReactive.Length > 0) {
					//create the convertWeight Window
					convertWeightWin = ConvertWeightWindow.Show(
							currentSession.UniqueID, personID, 
							weightIni, (int) spinbutton_weight.Value, 
							myJumpsNormal, myJumpsReactive);
					convertWeightWin.Button_accept.Clicked += new EventHandler(on_convertWeightWin_accepted);
					convertWeightWin.Button_cancel.Clicked += new EventHandler(on_convertWeightWin_cancelled);
				} else 
					recordChanges();
				
			} else 
				recordChanges();
			
		}

		if(errorMessage.Length > 0)
			errorWin = ErrorWindow.Show(errorMessage);
	}

	void on_convertWeightWin_accepted (object o, EventArgs args) {
		recordChanges();
	}

	void on_convertWeightWin_cancelled (object o, EventArgs args) {
		//do nothing (wait if user whants to cancel the personModify o change another thing)
	}

	private void recordChanges() {
		//separate by '/' for not confusing with the ':' separation between the other values
		string dateFull = dateTime.Day.ToString() + "/" + dateTime.Month.ToString() + "/" +
			dateTime.Year.ToString();

		if(adding) {
			currentPerson = new Person (entry1.Text, sex, dateFull, 
					(int) spinbutton_height.Value, (int) spinbutton_weight.Value, 
					sport.UniqueID, 
					Convert.ToInt32(Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(combo_speciallities), speciallities)),
					Util.FetchID(UtilGtk.ComboGetActive(combo_levels)),
					textview2.Buffer.Text,
					Constants.RaceUndefinedID,
					Convert.ToInt32(Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(combo_countries), countries)),
					Constants.ServerUndefinedID,
					currentSession.UniqueID);
		} else {
			currentPerson = new Person (personID, entry1.Text, sex, dateFull, 
					(int) spinbutton_height.Value, (int) spinbutton_weight.Value, 
					sport.UniqueID, 
					Convert.ToInt32(Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(combo_speciallities), speciallities)),
					Util.FetchID(UtilGtk.ComboGetActive(combo_levels)),
					textview2.Buffer.Text,
					Constants.RaceUndefinedID,
					Convert.ToInt32(Util.FindOnArray(':', 2, 0, UtilGtk.ComboGetActive(combo_countries), countries)),
					serverUniqueID);
			
			SqlitePerson.Update (currentPerson); 
		
			//change weight if needed
			if((int) spinbutton_weight.Value != weightIni)
				SqlitePersonSession.UpdateWeight (currentPerson.UniqueID, currentSession.UniqueID, (int) spinbutton_weight.Value); 
		}

		fakeButtonAccept.Click();

		PersonAddModifyWindowBox.person_win.Hide();
		PersonAddModifyWindowBox = null;
	}

	public Button FakeButtonAccept 
	{
		set { fakeButtonAccept = value; }
		get { return fakeButtonAccept; }
	}

	public Person CurrentPerson 
	{
		get {
			return currentPerson;
		}
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
	ErrorWindow errorWin;

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

			
			Gtk.RadioButton myRadioM = new Gtk.RadioButton(Catalog.GetString("M"));
			myRadioM.Show();
			radiosM.Add(myRadioM);
			
			Gtk.RadioButton myRadioF = new Gtk.RadioButton(myRadioM, Catalog.GetString("F"));
			myRadioF.Show();
			radiosF.Add(myRadioF);
			
			Gtk.HBox sexBox = new HBox();
			sexBox.PackStart(myRadioM, false, false, 4);
			sexBox.PackStart(myRadioF, false, false, 4);
			sexBox.Show();
			table_main.Attach (sexBox, (uint) 2, (uint) 3, (uint) count, (uint) count +1);


			Gtk.SpinButton mySpin = new Gtk.SpinButton(0, 300, 1);
			table_main.Attach (mySpin, (uint) 3, (uint) 4, (uint) count, (uint) count +1);
			mySpin.Show();
			spins.Add(mySpin);
		}

		string sportStuffString = "";
		if(currentSession.PersonsSportID != Constants.SportUndefinedID)
			sportStuffString += Catalog.GetString("Sport") + ":<i>" + Catalog.GetString(SqliteSport.Select(currentSession.PersonsSportID).Name) + "</i>.";
		if(currentSession.PersonsSpeciallityID != Constants.SpeciallityUndefinedID)
			sportStuffString += " " + Catalog.GetString("Speciallity") + ":<i>" + Util.FetchName(SqliteSpeciallity.Select(currentSession.PersonsSpeciallityID)) + "</i>.";
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
			errorWin = ErrorWindow.Show(combinedErrorString);
		} else {
			prepareAllNonBlankRows();
		
			PersonAddMultipleWindowBox.person_multiple_infinite.Hide();
			PersonAddMultipleWindowBox = null;
		}
	}
		
	void checkEntries(int count, string name, int weight) {
		if(name.Length > 0) {
			bool personExists = Sqlite.Exists (Constants.PersonTable, Util.RemoveTilde(name));
			if(personExists) {
				errorExistsString += "[" + (count+1) + "] " + name + "\n";
			}
			if(weight == 0) {
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
		 				(int) ((Gtk.SpinButton)spins[i]).Value);
	}

	void insertPerson (string name, bool male, int weight) 
	{
		string sex = "F";
		if(male) { sex = "M"; }

		//DateTime dateTime = DateTime.Today;
		//now dateTime is undefined until user changes it
		DateTime dateTime = DateTime.MinValue;
		string dateFull = dateTime.Day.ToString() + "/" + dateTime.Month.ToString() + "/" + dateTime.Year.ToString();

		currentPerson = new Person ( name, sex, dateFull, 
				0, weight, 		//height, weight	
				currentSession.PersonsSportID,
				currentSession.PersonsSpeciallityID,
				currentSession.PersonsPractice,
				"", 			//description
				Constants.RaceUndefinedID,
				Constants.CountryUndefinedID,
				Constants.ServerUndefinedID,
				currentSession.UniqueID		
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
				typeof (string), typeof(string), typeof(string), typeof(string), typeof(string) );
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
		string [] myPersons = SqlitePerson.SelectAllPersonsRecuperable("name", -1, inSession, ""); //"" is searchFilterName (not implemented on PersonShowAllEventsWindow)

		//put only id and name in combo
		string [] myPersons2 = new string[myPersons.Length];
		int count = 0;
		foreach (string person in myPersons) {
			string [] myStr = person.Split(new char[] {':'});
			myPersons2[count++] = myStr[0] + ":" + myStr[1];
		}
		UtilGtk.ComboUpdate(combo_persons, myPersons2, "");
		combo_persons.Active = UtilGtk.ComboMakeActive(myPersons2, personID + ":" + personName);

		combo_persons.Changed += new EventHandler (on_combo_persons_changed);

		hbox_combo_persons.PackStart(combo_persons, true, true, 0);
		hbox_combo_persons.ShowAll();
		combo_persons.Sensitive = true;
	}
	
	private void on_combo_persons_changed(object o, EventArgs args) {
		string myText = UtilGtk.ComboGetActive(combo_persons);
		if(myText != "") {
			store = new TreeStore( typeof (string), typeof (string), typeof (string), typeof (string), 
					typeof (string), typeof(string), typeof(string), typeof(string), typeof(string) );
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
	}
	
	protected void fillTreeView (Gtk.TreeView tv, TreeStore store, int personID) {
		ArrayList myEvents;
		//myEvents = SqlitePerson.SelectAllPersonEvents(currentPerson.UniqueID); 
		myEvents = SqlitePerson.SelectAllPersonEvents(personID); 

		foreach (string myEvent in myEvents) {
			string [] myStr = myEvent.Split(new char[] {':'});

			store.AppendValues (myStr[0], myStr[1], myStr[2], myStr[3], myStr[4], myStr[5], myStr[6], myStr[7], myStr[8]);
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

