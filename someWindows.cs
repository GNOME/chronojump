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

//--------------------------------------------------------
//---------------- SESSION WIDGETS -----------------------
//--------------------------------------------------------

public class SessionAddWindow {
	
	[Widget] Gtk.Window session_add;
	[Widget] Gtk.Entry entry_name;
	[Widget] Gtk.Entry entry_place;
	[Widget] Gnome.DateEdit dateedit;
	[Widget] Gtk.TextView textview;
	[Widget] Gtk.Button button_accept;
	
	ErrorWindow errorWin;

	private Session currentSession;
	
	static SessionAddWindow SessionAddWindowBox;
	Gtk.Window parent;
	
	
	SessionAddWindow (Gtk.Window parent) {
		Glade.XML gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "session_add", null);

		gladeXML.Autoconnect(this);
		this.parent = parent;
		
	}
	
	static public SessionAddWindow Show (Gtk.Window parent)
	{
		if (SessionAddWindowBox == null) {
			SessionAddWindowBox = new SessionAddWindow (parent);
		}
		SessionAddWindowBox.session_add.Show ();

		return SessionAddWindowBox;
	}
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		SessionAddWindowBox.session_add.Hide();
		SessionAddWindowBox = null;
	}
	
	void on_session_add_delete_event (object o, EventArgs args)
	{
		SessionAddWindowBox.session_add.Hide();
		SessionAddWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		string [] dateTimeFull = dateedit.Time.ToString().Split(new char[] {' '});
	
		bool sessionExists = Sqlite.SessionExists (removeTilde(entry_name.Text));
		if(sessionExists) {
			string myString = "Session: '" + removeTilde(entry_name.Text) + "' exists. Please, use another name";
			Console.WriteLine (myString);
			errorWin = ErrorWindow.Show(session_add, myString);

		} else {
			currentSession = new Session (entry_name.Text, entry_place.Text, dateTimeFull[0], textview.Buffer.Text);
			SessionAddWindowBox.session_add.Hide();
			SessionAddWindowBox = null;
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

	public Session CurrentSession 
	{
		get {
			return currentSession;
		}
	}

}


public class SessionLoadWindow {
	
	[Widget] Gtk.Window session_load;
	
	private TreeStore store;
	private string selected;
	[Widget] Gtk.TreeView treeview_session_load;
	[Widget] Gtk.Button button_accept;

	static SessionLoadWindow SessionLoadWindowBox;
	Gtk.Window parent;
	
	private Session currentSession;
	
	SessionLoadWindow (Gtk.Window parent) {
		Glade.XML gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "session_load", null);

		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		createTreeView(treeview_session_load);
		store = new TreeStore(typeof (string), typeof (string), typeof (string), typeof (string), 
				typeof (string), typeof (string), typeof (string), typeof (string) );
		treeview_session_load.Model = store;
		fillTreeView(treeview_session_load,store);

		button_accept.Sensitive = false;
	}
	
	static public SessionLoadWindow Show (Gtk.Window parent)
	{
		if (SessionLoadWindowBox == null) {
			SessionLoadWindowBox = new SessionLoadWindow (parent);
		}
		SessionLoadWindowBox.session_load.Show ();
		
		return SessionLoadWindowBox;
	}
	
	private void createTreeView (Gtk.TreeView tv) {
		tv.HeadersVisible=true;
		int count = 0;
		
		tv.AppendColumn ("number", new CellRendererText(), "text", count++);
		tv.AppendColumn ("Name", new CellRendererText(), "text", count++);
		tv.AppendColumn ("Place", new CellRendererText(), "text", count++);
		tv.AppendColumn ("Date", new CellRendererText(), "text", count++);
		tv.AppendColumn ("Jumpers", new CellRendererText(), "text", count++);
		tv.AppendColumn ("Jumps", new CellRendererText(), "text", count++);
		tv.AppendColumn ("RJ Jumps", new CellRendererText(), "text", count++);
		tv.AppendColumn ("Comments", new CellRendererText(), "text", count++);
	}
	
	private void fillTreeView (Gtk.TreeView tv, TreeStore store) {
		TreeIter iter = new TreeIter();

		string [] mySessions = Sqlite.SelectAllSessions(); //returns a string of values separated by ':'
		foreach (string session in mySessions) {
			string [] myStringFull = session.Split(new char[] {':'});

			iter = store.AppendValues (myStringFull[0], myStringFull[1], 
					myStringFull[2], myStringFull[3], 
					myStringFull[5],	//number of jumpers x session
					myStringFull[6],	//number of jumps x session
					myStringFull[7],	//number of jumpsRj x session
					myStringFull[4]		//description of session
					);
		}	

	}
	
	//puts a value in private member selected
	private void on_treeview_session_load_cursor_changed (object o, EventArgs args)
	{
		Console.WriteLine("cursor_changed");
		TreeView tv = (TreeView) o;
		TreeModel model;
		TreeIter iter;
		selected = "-1";

		// you get the iter and the model if something is selected
		if (tv.Selection.GetSelected (out model, out iter)) {
			selected = (string) model.GetValue (iter, 0);
			button_accept.Sensitive = true;
		}

		Console.WriteLine (selected);
	}
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		SessionLoadWindowBox.session_load.Hide();
		SessionLoadWindowBox = null;
	}
	
	void on_session_load_delete_event (object o, EventArgs args)
	{
		SessionLoadWindowBox.session_load.Hide();
		SessionLoadWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		if(selected != "-1")
		{
			currentSession = Sqlite.SessionSelect (selected);
			SessionLoadWindowBox.session_load.Hide();
			SessionLoadWindowBox = null;
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
	
	public Session CurrentSession 
	{
		get {
			return currentSession;
		}
	}

}

//--------------------------------------------------------
//---------------- PERSON WIDGETS ------------------------
//--------------------------------------------------------

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
		tv.AppendColumn ("Number", new CellRendererText(), "text", count++);
		tv.AppendColumn ("Name", new CellRendererText(), "text", count++);
		tv.AppendColumn ("Sex", new CellRendererText(), "text", count++);
		tv.AppendColumn ("Date born", new CellRendererText(), "text", count++);
		tv.AppendColumn ("Height", new CellRendererText(), "text", count++);
		tv.AppendColumn ("Weight", new CellRendererText(), "text", count++);
		tv.AppendColumn ("Description", new CellRendererText(), "text", count++);
	}
	
	private void fillTreeView (Gtk.TreeView tv, TreeStore store) {
		TreeIter iter = new TreeIter();

		string [] mySessions;
		
		if(sortByCreationDate) {
			mySessions = Sqlite.SelectAllPersonsRecuperable("uniqueID", sessionID); //returns a string of values separated by ':'
		} else {
			mySessions = Sqlite.SelectAllPersonsRecuperable("name", sessionID); //returns a string of values separated by ':'
		}

		foreach (string session in mySessions) {
			string [] myStringFull = session.Split(new char[] {':'});

			iter = store.AppendValues (myStringFull[0], myStringFull[1], 
					getCorrectSex(myStringFull[2]), myStringFull[3], myStringFull[4],
					myStringFull[5], myStringFull[6]
					);
		}	

	}

	private string getCorrectSex (string sex) 
	{
		if (sex == "M") return "Man";
		else return "Woman";
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

			Person currentPerson = Sqlite.PersonSelect(selected);
	
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
			int myInt = Sqlite.PersonSessionInsert(Convert.ToInt32(selected), sessionID);

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

		person_win.Title = "Load jumper";
	}
	
	void on_radiobutton_man_toggled (object o, EventArgs args)
	{
		Console.WriteLine("radiobutton man toggled");
		sex = "M";
		Console.WriteLine("sex: {0}", sex);
	}
	
	void on_radiobutton_woman_toggled (object o, EventArgs args)
	{
		Console.WriteLine("radiobutton woman toggled");
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
		string dateFull = spinbutton_day.Value.ToString() + ":" + 
			spinbutton_month.Value.ToString() + ":" + spinbutton_year.Value.ToString(); 
		
		bool personExists = Sqlite.PersonExists (removeTilde(entry1.Text));
		if(personExists) {
			string myString = "Jumper: '" + removeTilde(entry1.Text) + "' exists. Please, use another name";
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

		person_win.Title = "Edit jumper";
	}
	
	void on_radiobutton_man_toggled (object o, EventArgs args)
	{
		Console.WriteLine("radiobutton man toggled");
		sex = "M";
		Console.WriteLine("sex: {0}", sex);
	}
	
	void on_radiobutton_woman_toggled (object o, EventArgs args)
	{
		Console.WriteLine("radiobutton woman toggled");
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
		Person myPerson = Sqlite.PersonSelect(personID.ToString()); 
		
		entry1.Text = myPerson.Name;
		//Console.WriteLine("myPerson.Sex: {0}", myPerson.Sex);
		if (myPerson.Sex == "M") {
			//Console.WriteLine("SEX Male");
			radiobutton_man.Active = true;
		} else {
			//Console.WriteLine("SEX Female");
			radiobutton_woman.Active = true;
		}

		Console.WriteLine(myPerson.DateBorn);
		string [] dateFull = myPerson.DateBorn.Split(new char[] {':'});
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
		bool personExists = Sqlite.PersonExistsAndItsNotMe (uniqueID, removeTilde(entry1.Text));
		if(personExists) {
			string myString = "Jumper: '" + removeTilde(entry1.Text) + "' exists. Please, use another name";
			Console.WriteLine (myString);
			errorWin = ErrorWindow.Show(person_win, myString);
		} else {
			string dateFull = spinbutton_day.Value.ToString() + ":" + 
				spinbutton_month.Value.ToString() + ":" + spinbutton_year.Value.ToString(); 
			
			currentPerson = new Person (uniqueID, entry1.Text, sex, dateFull, (int) spinbutton_height.Value,
						(int) spinbutton_weight.Value, textview2.Buffer.Text);

			Sqlite.PersonUpdate (currentPerson); 
		
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

//--------------------------------------------------------
//---------------- EDIT JUMP WIDGET ----------------------
//--------------------------------------------------------

public class EditJumpWindow {
	[Widget] Gtk.Window edit_jump;
	[Widget] Gtk.Button button_accept;
	[Widget] Gtk.Label label_jump_id_value;
	[Widget] Gtk.Label label_type_value;
	[Widget] Gtk.Label label_tv_value;
	[Widget] Gtk.Label label_tc_value;
	[Widget] Gtk.Label label_fall_value;
	[Widget] Gtk.Label label_weight_value;
	[Widget] Gtk.Label label_limited_value;
	[Widget] Gtk.Box hbox_combo;
	[Widget] Gtk.Combo combo_jumpers;
	[Widget] Gtk.TextView textview_description;

	static EditJumpWindow EditJumpWindowBox;
	Gtk.Window parent;
	string type;

	EditJumpWindow (Gtk.Window parent) {
		Glade.XML gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "edit_jump", null);

		gladeXML.Autoconnect(this);
		this.parent = parent;
	}
	
	static public EditJumpWindow Show (Gtk.Window parent, Jump myJump)
	{
		Console.WriteLine(myJump);
		if (EditJumpWindowBox == null) {
			EditJumpWindowBox = new EditJumpWindow (parent);
		}
		
		EditJumpWindowBox.edit_jump.Show ();

		EditJumpWindowBox.fillDialog (myJump);


		return EditJumpWindowBox;
	}
	
	private void fillDialog (Jump myJump)
	{
		label_jump_id_value.Text = myJump.UniqueID.ToString();
		label_type_value.Text = myJump.Type;
		label_tv_value.Text = myJump.Tv.ToString();
		label_tc_value.Text = myJump.Tc.ToString();
	
		label_fall_value.Text = "0";
		label_weight_value.Text = "0";
		label_limited_value.Text = "0";
		
		if(myJump.Type == "SJ+") {
			label_weight_value.Text = myJump.Weight.ToString();
		} else if (myJump.Type == "DJ") {
			label_fall_value.Text = myJump.Fall.ToString();
		} else if (myJump.Type == "RJ") {
			label_limited_value.Text = myJump.Limited.ToString();
			//future: RJ with weight and/or fall
			//label_fall_value.Text = myJump.Fall.ToString();
			//label_weight_value.Text = myJump.Weight.ToString();
		}

		this.type = myJump.Type;

		TextBuffer tb = new TextBuffer (new TextTagTable());
		tb.SetText(myJump.Description);
		textview_description.Buffer = tb;

		string [] jumpers = Sqlite.PersonSessionSelectCurrentSession(myJump.SessionID);
		combo_jumpers = new Combo();
		combo_jumpers.PopdownStrings = jumpers;
		foreach (string jumper in jumpers) {
			Console.WriteLine("jumper: {0}, name: {1}", jumper, myJump.PersonID + ": " + myJump.JumperName);
			if (jumper == myJump.PersonID + ": " + myJump.JumperName) {
				combo_jumpers.Entry.Text = jumper;
			}
		}
		
		hbox_combo.PackStart(combo_jumpers, true, true, 0);
		hbox_combo.ShowAll();
	
	}
		
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		EditJumpWindowBox.edit_jump.Hide();
		EditJumpWindowBox = null;
	}
	
	void on_edit_jump_delete_event (object o, EventArgs args)
	{
		EditJumpWindowBox.edit_jump.Hide();
		EditJumpWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		int jumpID = Convert.ToInt32 ( label_jump_id_value.Text );
		string myJumper = combo_jumpers.Entry.Text;
		string [] myJumperFull = myJumper.Split(new char[] {':'});
		
		string myDesc = textview_description.Buffer.Text;
	
		if (type == "RJ") {
			Sqlite.JumpRjUpdate(jumpID, Convert.ToInt32 (myJumperFull[0]), myDesc);
		} else {
			Sqlite.JumpUpdate(jumpID, Convert.ToInt32 (myJumperFull[0]), myDesc);
		}

		EditJumpWindowBox.edit_jump.Hide();
		EditJumpWindowBox = null;
	}

	public Button Button_accept 
	{
		set { button_accept = value;	}
		get { return button_accept;	}
	}

}

//--------------------------------------------------------
//---------------- SJ+ WIDGET ----------------------------
//--------------------------------------------------------

public class SjPlusWindow {
	[Widget] Gtk.Window sj_plus;
	[Widget] Gtk.SpinButton spinbutton1;
	[Widget] Gtk.Button button_accept;
	[Widget] Gtk.RadioButton radiobutton_kg;
	[Widget] Gtk.RadioButton radiobutton_weight;

	static string option = "Kg";
	static int weight = 10;
	
	static SjPlusWindow SjPlusWindowBox;
	Gtk.Window parent;

	SjPlusWindow (Gtk.Window parent) {
		Glade.XML gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "sj_plus", null);

		gladeXML.Autoconnect(this);
		this.parent = parent;
		
	}
	
	static public SjPlusWindow Show (Gtk.Window parent)
	{
		if (SjPlusWindowBox == null) {
			SjPlusWindowBox = new SjPlusWindow (parent);
		}
		SjPlusWindowBox.spinbutton1.Value = weight;
		if (option == "Kg") {
			SjPlusWindowBox.radiobutton_kg.Active = true;
		} else {
			SjPlusWindowBox.radiobutton_weight.Active = true;
		}
		
		SjPlusWindowBox.sj_plus.Show ();

		return SjPlusWindowBox;
	}
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		SjPlusWindowBox.sj_plus.Hide();
		SjPlusWindowBox = null;
	}
	
	void on_sj_plus_delete_event (object o, EventArgs args)
	{
		SjPlusWindowBox.sj_plus.Hide();
		SjPlusWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		weight = (int) spinbutton1.Value;
		
		Console.WriteLine("button_accept_clicked. Value: {0}{1}", spinbutton1.Value.ToString(), option);

		SjPlusWindowBox.sj_plus.Hide();
		SjPlusWindowBox = null;
	}

	void on_radiobutton_kg_toggled (object o, EventArgs args)
	{
		Console.WriteLine("radiobutton Kg toggled");
		option = "Kg";
		Console.WriteLine("option: {0}", option);
	}
	
	void on_radiobutton_weight_toggled (object o, EventArgs args)
	{
		Console.WriteLine("radiobutton %weight toggled");
		option = "%";
		Console.WriteLine("option: {0}", option);
	}

	public Button Button_accept 
	{
		set { button_accept = value;	}
		get { return button_accept;	}
	}

	public string Option 
	{
		get { return option;	}
	}

	public int Weight 
	{
		get { return weight;	}
	}
}

//--------------------------------------------------------
//---------------- DJ FALL WIDGET ------------------------
//--------------------------------------------------------

public class DjFallWindow {
	[Widget] Gtk.Window dj_fall;
	[Widget] Gtk.SpinButton spinbutton_fall;
	[Widget] Gtk.Button button_accept;

	static int fall = 20;
	
	static DjFallWindow DjFallWindowBox;
	Gtk.Window parent;

	DjFallWindow (Gtk.Window parent) {
		Glade.XML gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "dj_fall", null);

		gladeXML.Autoconnect(this);
		this.parent = parent;
		
	}
	
	static public DjFallWindow Show (Gtk.Window parent)
	{
		if (DjFallWindowBox == null) {
			DjFallWindowBox = new DjFallWindow (parent);
		}
		DjFallWindowBox.spinbutton_fall.Value = fall;
		
		DjFallWindowBox.dj_fall.Show ();

		return DjFallWindowBox;
	}
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		DjFallWindowBox.dj_fall.Hide();
		DjFallWindowBox = null;
	}
	
	void on_dj_fall_delete_event (object o, EventArgs args)
	{
		DjFallWindowBox.dj_fall.Hide();
		DjFallWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		fall = (int) spinbutton_fall.Value;
		
		Console.WriteLine("button_accept_clicked. Value: {0}", spinbutton_fall.Value.ToString());

		DjFallWindowBox.dj_fall.Hide();
		DjFallWindowBox = null;
	}

	public Button Button_accept 
	{
		set { button_accept = value;	}
		get { return button_accept;	}
	}

	public int Fall 
	{
		get { return fall;	}
	}
}

//--------------------------------------------------------
//---------------- RJ WIDGET (similar to sj+ ) -----------
//--------------------------------------------------------


public class RjWindow {
	
	[Widget] Gtk.Window rj;
	[Widget] Gtk.SpinButton spinbutton_limit;

	[Widget] Gtk.Button button_accept;
	[Widget] Gtk.RadioButton radiobutton_jump;
	[Widget] Gtk.RadioButton radiobutton_time;
	
	static RjWindow RjWindowBox;
	Gtk.Window parent;

	static string option = "J";
	static int limited = 20;
	
	RjWindow (Gtk.Window parent) {
		Glade.XML gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "rj", null);

		gladeXML.Autoconnect(this);
		this.parent = parent;
		
	}
	
	static public RjWindow Show (Gtk.Window parent)
	{
		if (RjWindowBox == null) {
			RjWindowBox = new RjWindow (parent);
		}
		RjWindowBox.spinbutton_limit.Value = limited;
		if (option == "J") {
			RjWindowBox.radiobutton_jump.Active = true;
		} else {
			RjWindowBox.radiobutton_time.Active = true;
		}
		
		RjWindowBox.rj.Show ();

		return RjWindowBox;
	}
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		RjWindowBox.rj.Hide();
		RjWindowBox = null;
	}
	
	void on_rj_delete_event (object o, EventArgs args)
	{
		RjWindowBox.rj.Hide();
		RjWindowBox = null;
	}
	
	void on_radiobutton_jump_toggled (object o, EventArgs args)
	{
		Console.WriteLine("radiobutton jump toggled");
		option = "J";
		Console.WriteLine("option: {0}", option);
	}
	
	void on_radiobutton_time_toggled (object o, EventArgs args)
	{
		Console.WriteLine("radiobutton time toggled");
		option = "T";
		Console.WriteLine("option: {0}", option);
	}

	void on_button_accept_clicked (object o, EventArgs args)
	{
		limited = (int) spinbutton_limit.Value;
		
		RjWindowBox.rj.Hide();
		RjWindowBox = null;
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

	public string Option 
	{
		get { return option;	}
	}
	
	public int Limited 
	{
		get { return limited; }
	}

}

//--------------------------------------------------------
//---------------- PREFERENCES WIDGET --------------------
//--------------------------------------------------------


public class PreferencesWindow {
	
	[Widget] Gtk.Window preferences;
	[Widget] Gtk.SpinButton spinbutton_decimals;
	[Widget] Gtk.CheckButton checkbutton_height;
	[Widget] Gtk.CheckButton checkbutton_ask_deletion;

	[Widget] Gtk.Button button_accept;
	
	static PreferencesWindow PreferencesWindowBox;
	Gtk.Window parent;

	
	PreferencesWindow (Gtk.Window parent) {
		Glade.XML gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "preferences", null);

		gladeXML.Autoconnect(this);
		this.parent = parent;
		
	}
	
	static public PreferencesWindow Show (Gtk.Window parent, int digitsNumber, bool showHeight, bool simulated, bool askDeletion)
	{
		if (PreferencesWindowBox == null) {
			PreferencesWindowBox = new PreferencesWindow (parent);
		}
		PreferencesWindowBox.spinbutton_decimals.Value = digitsNumber;
	
		if(showHeight) { 
			PreferencesWindowBox.checkbutton_height.Active = true; 
		}
		else {
			PreferencesWindowBox.checkbutton_height.Active = false; 
		}

		if(askDeletion) { 
			PreferencesWindowBox.checkbutton_ask_deletion.Active = true; 
		}
		else {
			PreferencesWindowBox.checkbutton_ask_deletion.Active = false; 
		}

		PreferencesWindowBox.preferences.Show ();

		return PreferencesWindowBox;
	}
	
	void on_button_cancel_clicked (object o, EventArgs args)
	{
		PreferencesWindowBox.preferences.Hide();
		PreferencesWindowBox = null;
	}
	
	void on_preferences_delete_event (object o, EventArgs args)
	{
		PreferencesWindowBox.preferences.Hide();
		PreferencesWindowBox = null;
	}
	
	void on_button_accept_clicked (object o, EventArgs args)
	{
		Sqlite.PreferencesUpdate("digitsNumber", spinbutton_decimals.Value.ToString());
		Sqlite.PreferencesUpdate("showHeight", PreferencesWindowBox.checkbutton_height.Active.ToString());
		Sqlite.PreferencesUpdate("askDeletion", PreferencesWindowBox.checkbutton_ask_deletion.Active.ToString());
		
		PreferencesWindowBox.preferences.Hide();
		PreferencesWindowBox = null;
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

}

//--------------------------------------------------------
//---------------- CONFIRM  WIDGETS ----------------------
//--------------------------------------------------------


public class ConfirmWindow
{
	[Widget] Gtk.Window confirm_window;
	[Widget] Gtk.Label label1;
	[Widget] Gtk.Label label2;
	[Widget] Gtk.Button button_accept;

	Gtk.Window parent;
	
	string table;
	int uniqueID;
	bool isRj;
	static ConfirmWindow ConfirmWindowBox;
	
	public ConfirmWindow (Gtk.Window parent, string text1, string text2, string table, int uniqueID, bool isRj)
	{
		//Setup (text, table, uniqueID);
		Glade.XML gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "confirm_window", null);

		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		label1.Text = text1;
		label2.Text = text2;
		this.table = table;
		this.uniqueID = uniqueID;
		this.isRj = isRj;
	}

	static public ConfirmWindow Show (Gtk.Window parent, string text1, string text2, string table, int uniqueID, bool isRj)
	{
		if (ConfirmWindowBox == null) {
			ConfirmWindowBox = new ConfirmWindow(parent, text1, text2, table, uniqueID, isRj);
		}
		ConfirmWindowBox.confirm_window.Show ();
		
		return ConfirmWindowBox;
	}
	
	protected void on_button_cancel_clicked (object o, EventArgs args)
	{
		ConfirmWindowBox.confirm_window.Hide();
		ConfirmWindowBox = null;
	}
	
	protected void on_delete_selected_jump_delete_event (object o, EventArgs args)
	{
		ConfirmWindowBox.confirm_window.Hide();
		ConfirmWindowBox = null;
	}
	

	protected void on_button_accept_clicked (object o, EventArgs args)
	{
		if (table == "jump") {
			if (isRj) {
				Sqlite.JumpRjDelete(uniqueID.ToString());
			} else {
				Sqlite.JumpDelete(uniqueID.ToString());
			}
		} else if (table == "person") {
		} else if (table == "session") {
		} else {
			Console.WriteLine ("Error, table: {0}", table);
		}
		
		ConfirmWindowBox.confirm_window.Hide();
		ConfirmWindowBox = null;
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

	~ConfirmWindow() {}
	
}


public class ConfirmWindowPlatform
{
	[Widget] Gtk.Window confirm_window;
	[Widget] Gtk.Label label1;
	[Widget] Gtk.Label label2;
	[Widget] Gtk.Button button_accept;

	Gtk.Window parent;
	
	static ConfirmWindowPlatform ConfirmWindowBox;
	
	public ConfirmWindowPlatform (Gtk.Window parent, string text1, string text2)
	{
		Glade.XML gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "confirm_window", null);

		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		label1.Text = text1;
		label2.Text = text2;
	}

	static public ConfirmWindowPlatform Show (Gtk.Window parent, string text1, string text2)
	{
		if (ConfirmWindowBox == null) {
			ConfirmWindowBox = new ConfirmWindowPlatform(parent, text1, text2);
		}
		ConfirmWindowBox.confirm_window.Show ();
		
		return ConfirmWindowBox;
	}
	
	protected void on_button_cancel_clicked (object o, EventArgs args)
	{
		ConfirmWindowBox.confirm_window.Hide();
		ConfirmWindowBox = null;
	}
	
	protected void on_delete_selected_jump_delete_event (object o, EventArgs args)
	{
		ConfirmWindowBox.confirm_window.Hide();
		ConfirmWindowBox = null;
	}
	

	protected void on_button_accept_clicked (object o, EventArgs args)
	{
		ConfirmWindowBox.confirm_window.Hide();
		ConfirmWindowBox = null;
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

	~ConfirmWindowPlatform() {}
	
}

//--------------------------------------------------------
//---------------- ERROR  WINDOW -------------------------
//--------------------------------------------------------


public class ErrorWindow
{
	[Widget] Gtk.Window error_window;
	[Widget] Gtk.Label label1;
	[Widget] Gtk.Button button_accept;

	Gtk.Window parent;
	
	string table;
	static ErrorWindow ErrorWindowBox;
	
	public ErrorWindow (Gtk.Window parent, string text1)
	{
		Glade.XML gladeXML = Glade.XML.FromAssembly ("chronojump.glade", "error_window", null);

		gladeXML.Autoconnect(this);
		this.parent = parent;
		
		label1.Text = text1;
	}

	static public ErrorWindow Show (Gtk.Window parent, string text1)
	{
		if (ErrorWindowBox == null) {
			ErrorWindowBox = new ErrorWindow(parent, text1);
		}
		ErrorWindowBox.error_window.Show ();
		
		return ErrorWindowBox;
	}
	
	protected void on_delete_window_event (object o, EventArgs args)
	{
		ErrorWindowBox.error_window.Hide();
		ErrorWindowBox = null;
	}
	

	protected void on_button_accept_clicked (object o, EventArgs args)
	{
		ErrorWindowBox.error_window.Hide();
		ErrorWindowBox = null;
	}
	
	~ErrorWindow() {}
	
}

