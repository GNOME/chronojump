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
using Gnome;
using GLib; //for Value
using System.Text; //StringBuilder
using System.Collections; //ArrayList


//load person (jumper)
public class PersonRecuperateWindow {
	
	[Widget] Gtk.Window person_recuperate;
	
	[Widget] Gtk.CheckButton checkbutton_sorted_by_creation_date;
	bool sortByCreationDate = false;
	
	private TreeStore store;
	private string selected;
	[Widget] Gtk.TreeView treeview_person_recuperate;
	[Widget] Gtk.Button button_recuperate;
	
	static PersonRecuperateWindow PersonRecuperateWindowBox;
	Gtk.Window parent;

	private int sessionID;
	
	private Person currentPerson;
	
	PersonRecuperateWindow (Gtk.Window parent, int sessionID) {
		Glade.XML gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "person_recuperate", null);

		gladeXML.Autoconnect(this);
		this.parent = parent;

		this.sessionID = sessionID;
	
		//no posible to recuperate until one person is selected
		button_recuperate.Sensitive = false;
		
		createTreeView(treeview_person_recuperate);
		store = new TreeStore( typeof (string), typeof (string), typeof (string), typeof (string), 
				typeof (string), typeof(string), typeof(string) );
		treeview_person_recuperate.Model = store;
		fillTreeView(treeview_person_recuperate,store);
	}
	
	static public PersonRecuperateWindow Show (Gtk.Window parent, int sessionID)
	{
		if (PersonRecuperateWindowBox == null) {
			PersonRecuperateWindowBox = new PersonRecuperateWindow (parent, sessionID);
		}
		PersonRecuperateWindowBox.person_recuperate.Show ();
		
		return PersonRecuperateWindowBox;
	}
	
	private void createTreeView (Gtk.TreeView tv) {
		tv.HeadersVisible=true;
		int count = 0;
		tv.AppendColumn ( Catalog.GetString("Number"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString("Name"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString("Sex"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString("Height"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString("Weight"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString("Date born"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString("Description"), new CellRendererText(), "text", count++);
	}
	
	private void fillTreeView (Gtk.TreeView tv, TreeStore store) {
		TreeIter iter = new TreeIter();

		string [] mySessions;
		
		if(sortByCreationDate) {
			mySessions = SqlitePerson.SelectAllPersonsRecuperable("uniqueID", sessionID); //returns a string of values separated by ':'
		} else {
			mySessions = SqlitePerson.SelectAllPersonsRecuperable("name", sessionID); //returns a string of values separated by ':'
		}

		
		
		foreach (string session in mySessions) {
			string [] myStringFull = session.Split(new char[] {':'});

			iter = store.AppendValues (myStringFull[0], myStringFull[1], 
					getCorrectSex(myStringFull[2]), myStringFull[4], myStringFull[5],
					myStringFull[3], myStringFull[6]
					);
		}	

	}

	private string getCorrectSex (string sex) 
	{
		if (sex == "M") return  Catalog.GetString("Man");
		else return  Catalog.GetString ("Woman");
	}
	
	private void on_checkbutton_sort_by_creation_date_clicked(object o, EventArgs args) {
		if (sortByCreationDate) { sortByCreationDate = false; }
		else { sortByCreationDate = true; }
		
		store = new TreeStore( typeof (string), typeof (string), typeof (string), typeof (string), 
				typeof (string), typeof(string), typeof(string) );
		treeview_person_recuperate.Model = store;
		
		fillTreeView(treeview_person_recuperate,store);
	}
	
	//puts a value in private member selected
	private void on_treeview_person_recuperate_cursor_changed (object o, EventArgs args)
	{
		Console.WriteLine("cursor_changed");
		TreeView tv = (TreeView) o;
		TreeModel model;
		TreeIter iter;
		selected = "-1";

		// you get the iter and the model if something is selected
		if (tv.Selection.GetSelected (out model, out iter)) {
			selected = (string) model.GetValue (iter, 0);

			Person currentPerson = SqlitePersonSession.PersonSelect(selected);
	
			//allow clicking button_recuperate
			button_recuperate.Sensitive = true;
		}

		Console.WriteLine (selected);
	}
	
	void on_button_close_clicked (object o, EventArgs args)
	{
		PersonRecuperateWindowBox.person_recuperate.Hide();
		PersonRecuperateWindowBox = null;
	}
	
	void on_person_recuperate_delete_event (object o, EventArgs args)
	{
		PersonRecuperateWindowBox.person_recuperate.Hide();
		PersonRecuperateWindowBox = null;
	}
	
	void on_button_recuperate_clicked (object o, EventArgs args)
	{
		if(selected != "-1")
		{
			int myInt = SqlitePersonSession.Insert(Convert.ToInt32(selected), sessionID);

			store = new TreeStore( typeof (string), typeof (string), typeof (string), typeof (string), 
					typeof (string), typeof(string), typeof(string) );
			treeview_person_recuperate.Model = store;
		
			fillTreeView(treeview_person_recuperate,store);
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

//new person (jumper)
public class PersonAddWindow {
	
	[Widget] Gtk.Window person_win;
	[Widget] Gtk.Entry entry1;
	[Widget] Gtk.RadioButton radiobutton_man;
	[Widget] Gtk.RadioButton radiobutton_woman;
	[Widget] Gtk.SpinButton spinbutton_day;
	[Widget] Gtk.SpinButton spinbutton_month;
	[Widget] Gtk.SpinButton spinbutton_year;
	[Widget] Gtk.TextView textview2;
	[Widget] Gtk.SpinButton spinbutton_height;
	[Widget] Gtk.SpinButton spinbutton_weight;
	
	[Widget] Gtk.Button button_accept;
	
	static PersonAddWindow PersonAddWindowBox;
	Gtk.Window parent;
	ErrorWindow errorWin;

	private Person currentPerson;
	private int sessionID;
	private string sex = "M";
	
	PersonAddWindow (Gtk.Window parent, int sessionID) {
		Glade.XML gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "person_win", null);

		gladeXML.Autoconnect(this);
		this.parent = parent;
		this.sessionID = sessionID;
		button_accept.Sensitive = false; //only make sensitive when required values are inserted

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
		Console.WriteLine("sex: {0}", sex);
	}
	
	void on_radiobutton_woman_toggled (object o, EventArgs args)
	{
		sex = "F";
		Console.WriteLine("sex: {0}", sex);
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
	
	void on_person_win_delete_event (object o, EventArgs args)
	{
		PersonAddWindowBox.person_win.Hide();
		PersonAddWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		//separate by '/' for not confusing with the ':' separation between the other values
		string dateFull = spinbutton_day.Value.ToString() + "/" + 
			spinbutton_month.Value.ToString() + "/" + spinbutton_year.Value.ToString(); 
		
		bool personExists = SqlitePersonSession.PersonExists (removeTilde(entry1.Text));
		if(personExists) {
			string myString =  Catalog.GetString ("Jumper: '") + removeTilde(entry1.Text) +  Catalog.GetString ("' exists. Please, use another name");
			Console.WriteLine (myString);
			errorWin = ErrorWindow.Show(person_win, myString);
		} else {
			currentPerson = new Person (entry1.Text, sex, dateFull, (int) spinbutton_height.Value,
						(int) spinbutton_weight.Value, textview2.Buffer.Text, sessionID);
		
			PersonAddWindowBox.person_win.Hide();
			PersonAddWindowBox = null;
		}
	}
	
	private string removeTilde(string myString) 
	{
		StringBuilder myStringBuilder = new StringBuilder(myString);
		myStringBuilder.Replace("'", "");
		return myStringBuilder.ToString();
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
	[Widget] Gtk.SpinButton spinbutton_day;
	[Widget] Gtk.SpinButton spinbutton_month;
	[Widget] Gtk.SpinButton spinbutton_year;
	[Widget] Gtk.SpinButton spinbutton_height;
	[Widget] Gtk.SpinButton spinbutton_weight;
	
	[Widget] Gtk.Button button_accept;
	
	static PersonModifyWindow PersonModifyWindowBox;
	Gtk.Window parent;
	ErrorWindow errorWin;

	private Person currentPerson;
	private int sessionID;
	private int uniqueID;
	private string sex = "M";
	
	
	PersonModifyWindow (Gtk.Window parent, int sessionID) {
		Glade.XML gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "person_win", null);
		
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
		Console.WriteLine("sex: {0}", sex);
	}
	
	void on_radiobutton_woman_toggled (object o, EventArgs args)
	{
		sex = "F";
		Console.WriteLine("sex: {0}", sex);
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

		string [] dateFull = myPerson.DateBorn.Split(new char[] {'/'});
		spinbutton_day.Value = Convert.ToDouble ( dateFull[0] );	
		spinbutton_month.Value = Convert.ToDouble ( dateFull[1] );	
		spinbutton_year.Value = Convert.ToDouble ( dateFull[2] );	
		
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
	void on_person_win_delete_event (object o, EventArgs args)
	{
		PersonModifyWindowBox.person_win.Hide();
		PersonModifyWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		bool personExists = SqlitePersonSession.PersonExistsAndItsNotMe (uniqueID, removeTilde(entry1.Text));
		if(personExists) {
			string myString =  Catalog.GetString ("Jumper: '") + removeTilde(entry1.Text) +  Catalog.GetString ("' exists. Please, use another name");
			Console.WriteLine (myString);
			errorWin = ErrorWindow.Show(person_win, myString);
		} else {
			//separate by '/' for not confusing with the ':' separation between the other values
			string dateFull = spinbutton_day.Value.ToString() + "/" + 
				spinbutton_month.Value.ToString() + "/" + spinbutton_year.Value.ToString(); 
			
			currentPerson = new Person (uniqueID, entry1.Text, sex, dateFull, (int) spinbutton_height.Value,
						(int) spinbutton_weight.Value, textview2.Buffer.Text);

			SqlitePerson.Update (currentPerson); 
		
			PersonModifyWindowBox.person_win.Hide();
			PersonModifyWindowBox = null;
		}
	}
	
	private string removeTilde(string myString) 
	{
		StringBuilder myStringBuilder = new StringBuilder(myString);
		myStringBuilder.Replace("'", "''");
		return myStringBuilder.ToString();
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
