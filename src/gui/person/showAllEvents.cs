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
using System.Collections.Generic; //List<T>
using Mono.Unix;


//show all events (jumps and runs) of a person in different sessions
public class PersonShowAllEventsWindow
{
	// at glade ---->
	Gtk.Window person_show_all_events;

	Gtk.HBox hbox_session_radios;
	Gtk.RadioButton radio_session_current;
	Gtk.RadioButton radio_session_all;
	Gtk.Label label_radio_session_current;
	Gtk.Label label_radio_session_all;
	Gtk.HBox hbox_filter;
	Gtk.Label label_filter;
	Gtk.Entry entry_filter;
	Gtk.Label label_person;
	Gtk.Label label_person_name;

	Gtk.TreeView treeview_person_show_all_events;
	Gtk.Box hbox_combo_persons;
	Gtk.Button button_load_session;
	// <---- at glade

	Gtk.ComboBoxText combo_persons;
	public Gtk.Button fakeButtonLoadSession;

	//when using persons at top, at close this window need to show again the persons top window
	public Gtk.Button fakeButtonDoneCalledFromTop;

	TreeStore store;
	static PersonShowAllEventsWindow PersonShowAllEventsWindowBox;

	private int selectedPersonID;
	private int currentSessionID;
	private int selectedSessionID;


	PersonShowAllEventsWindow (Gtk.Window parent, int currentSessionID, Person currentPerson)
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "person_show_all_events.glade", "person_show_all_events", "chronojump");
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "person_show_all_events.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);
		
		//put an icon to window
		UtilGtk.IconWindow(person_show_all_events);

		//manage window color
		if(! Config.UseSystemColor)
		{
			UtilGtk.WindowColor(person_show_all_events, Config.ColorBackground);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, label_radio_session_current);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, label_radio_session_all);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, label_filter);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, label_person);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, label_person_name);
		}

		person_show_all_events.Parent = parent;
		this.currentSessionID = currentSessionID;

		fakeButtonLoadSession = new Gtk.Button();
		fakeButtonDoneCalledFromTop = new Gtk.Button();

		label_person_name.Text = currentPerson.Name;
		createComboPersons(currentSessionID, currentPerson.UniqueID.ToString(), currentPerson.Name);
		createTreeView(treeview_person_show_all_events);
		store = new TreeStore(
				typeof (string), //sessionID (hidden)
				typeof (string), typeof (string), typeof (string), //session
				typeof (string), typeof (string), //jumps
				typeof (string), typeof (string), typeof (string), //races
				typeof (string), typeof (string), //isometric, elastic
				typeof (string), typeof (string) ); //weights, inertial
		treeview_person_show_all_events.Model = store;
		fillTreeView(treeview_person_show_all_events,store, currentPerson.UniqueID);

		treeview_person_show_all_events.CursorChanged += on_treeview_cursor_changed;
		//treeview_person_show_all_events.RowActivated += on_row_double_clicked;
	}
	
	static public PersonShowAllEventsWindow Show (Gtk.Window parent,
			int currentSessionID, Person currentPerson, bool allowChangePerson, Gdk.RGBA colorBackground)
	{
		if (PersonShowAllEventsWindowBox == null) {
			PersonShowAllEventsWindowBox = new PersonShowAllEventsWindow (parent, currentSessionID, currentPerson);
		}

		if(allowChangePerson)
		{
			PersonShowAllEventsWindowBox.hbox_session_radios.Visible = true;
			PersonShowAllEventsWindowBox.hbox_filter.Visible =
				PersonShowAllEventsWindowBox.radio_session_all.Active;
			PersonShowAllEventsWindowBox.hbox_combo_persons.Visible = true;
			PersonShowAllEventsWindowBox.label_person_name.Visible = false;
		} else {
			PersonShowAllEventsWindowBox.hbox_session_radios.Visible = false;
			PersonShowAllEventsWindowBox.hbox_filter.Visible = false;
			PersonShowAllEventsWindowBox.entry_filter.Text = "";
			PersonShowAllEventsWindowBox.hbox_combo_persons.Visible = false;
			PersonShowAllEventsWindowBox.label_person_name.Visible = true;
		}

		PersonShowAllEventsWindowBox.person_show_all_events.Show ();
		
		return PersonShowAllEventsWindowBox;
	}

	private void on_entry_filter_changed (object o, EventArgs args)
	{
		radio_session_toggled_do (); 	//this will update combo and treeview
	}

	private void createComboPersons (int currentSessionID, string personID, string personName)
	{
		combo_persons = new ComboBoxText ();

		int inSession = -1;		//select persons from all sessions
		if (radio_session_current.Active)
			inSession = currentSessionID;	//select only persons who are on currentSession

		string filter = "";
		if (entry_filter != null && entry_filter.Text != "")
			filter = entry_filter.Text;

		ArrayList myPersons = SqlitePerson.SelectAllPersonsRecuperable("name", -1, inSession, filter);

		//put only id and name in combo
		string [] myPersonsIDName = new string[myPersons.Count];
		int count = 0;
		foreach (Person person in myPersons) 
			myPersonsIDName[count++] = person.IDAndName(":");
		
		UtilGtk.ComboUpdate(combo_persons, myPersonsIDName, "");
		if (personID == "-1" || personName == "")
			combo_persons.Active = 0;
		else
			combo_persons.Active = UtilGtk.ComboMakeActive(myPersonsIDName, personID + ":" + personName);

		combo_persons.Changed += new EventHandler (on_combo_persons_changed);

		hbox_combo_persons.PackStart(combo_persons, true, true, 0);
		hbox_combo_persons.ShowAll();
		combo_persons.Sensitive = true;
	}
	
	private void on_combo_persons_changed (object o, EventArgs args)
	{
		string myText = UtilGtk.ComboGetActive(combo_persons);

		store = new TreeStore(
				typeof (string), //sessionID (hidden)
				typeof (string), typeof (string), typeof (string), //session
				typeof (string), typeof (string), //jumps
				typeof (string), typeof (string), typeof (string), //races
				typeof (string), typeof (string), //isometric, elastic
				typeof (string), typeof (string) ); //weights, inertial
		treeview_person_show_all_events.Model = store;

		if(myText != "") {
			string [] myStringFull = myText.Split(new char[] {':'});
			selectedPersonID = Convert.ToInt32 (myStringFull[0]);
			fillTreeView (treeview_person_show_all_events, store, selectedPersonID);
		} else
			fillTreeView (treeview_person_show_all_events, store, -1);
	}
	
	private void on_radio_session_toggled (object o, EventArgs args)
	{
		//only manage active
		if (o == (object) radio_session_current && radio_session_current.Active)
			radio_session_toggled_do ();
		else if (o == (object) radio_session_all && radio_session_all.Active)
			radio_session_toggled_do ();
	}

	private void radio_session_toggled_do ()
	{
		hbox_filter.Visible = radio_session_all.Active;

		string myText = UtilGtk.ComboGetActive (combo_persons);
		combo_persons.Destroy();

		if(myText != "") {
			string [] myStringFull = myText.Split(new char[] {':'});
			createComboPersons(currentSessionID, myStringFull[0], myStringFull[1] );
		} else
			createComboPersons(currentSessionID, "-1", "" );

		on_combo_persons_changed (0, new EventArgs ());	//called for updating the treeview ifcombo_persons.Entry changed

		button_load_session.Visible = radio_session_all.Active;
	}
	
	private void createTreeView (Gtk.TreeView tv)
	{
		tv.HeadersVisible=true;
		int count = 0;

		//invisible sessionID column
		Gtk.TreeViewColumn sessionIDCol = new Gtk.TreeViewColumn ("sessionID", new CellRendererText(), "text", count ++);
		sessionIDCol.Visible = false;
		tv.AppendColumn (sessionIDCol);

		tv.AppendColumn ( Catalog.GetString ("Date"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Session name"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Place"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Jumps\nsimple"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Jumps\nreactive"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Races\nsimple"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Races\ninterval"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Race analyzer"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Isometric"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Elastic"), new CellRendererText(), "text", count++);
		tv.AppendColumn (Catalog.GetString ("Weights") + "\n" +
				Catalog.GetString("Sets") + " ; " + Catalog.GetString("Repetitions"),
				new CellRendererText(), "text", count++);
		tv.AppendColumn (Catalog.GetString ("Inertial") + "\n" +
				Catalog.GetString("Sets") + " ; " + Catalog.GetString("Repetitions"),
				new CellRendererText(), "text", count++);
	}
	
	private void fillTreeView (Gtk.TreeView tv, TreeStore store, int personID)
	{
		selectedSessionID = -1;

		if (personID < 0)
			return;

		List<SessionTestsCount> stc_l = SqliteSession.SelectAllSessionsTestsCount (personID); //returns a string of values separated by ':'
		foreach (SessionTestsCount stc in stc_l)
		{
			string [] strings = new string [13];
			int i = 0;
			strings[i ++] = stc.sessionParams.ID.ToString ();
			strings[i ++] = stc.sessionParams.Date;
			strings[i ++] = stc.sessionParams.Name;
			//no tags
			strings[i ++] = stc.sessionParams.Place;
			strings[i ++] = stc.JumpsSimple.ToString ();
			strings[i ++] = stc.JumpsReactive.ToString ();
			strings[i ++] = stc.RunsSimple.ToString ();
			strings[i ++] = stc.RunsInterval.ToString ();
			strings[i ++] = stc.RunsEncoder.ToString ();
			strings[i ++] = stc.Isometric.ToString ();
			strings[i ++] = stc.Elastic.ToString ();
			strings[i ++] = string.Format ("{0} ; {1}",
					stc.WeightsSets, stc.WeightsReps); //number of encoder grav signal,reps x session
			strings[i ++] = string.Format ("{0} ; {1}",
					stc.InertialSets, stc.InertialReps); //number of encoder inertial signal,reps x session
			store.AppendValues (strings);
		}

		store.SetSortFunc (1, UtilGtk.DateColumnCompare);
		store.SetSortColumnId (1, Gtk.SortType.Descending); //date
		store.ChangeSortColumn();

		on_treeview_cursor_changed (treeview_person_show_all_events, new EventArgs ());
	}

	private void on_treeview_cursor_changed (object o, EventArgs args)
	{
		LogB.Information("on_treeview_cursor_changed");

		TreeIter iter = new TreeIter();

		ITreeModel myModel = treeview_person_show_all_events.Model;
		if (treeview_person_show_all_events.Selection.GetSelected (out myModel, out iter))
		{
			string selected = (treeview_person_show_all_events.Model.GetValue (iter, 0) ).ToString();
			if (Util.IsNumber (selected, false) && Convert.ToInt32 (selected) != currentSessionID)
			{
				selectedSessionID = Convert.ToInt32 (selected);
				button_load_session.Sensitive = radio_session_all.Active;
				return;
			}
		}

		button_load_session.Sensitive = false;
	}

	private void on_button_load_session_clicked (object o, EventArgs args)
	{
		if (selectedSessionID >= 0)
			fakeButtonLoadSession.Click ();
	}

	private void on_button_close_clicked (object o, EventArgs args)
	{
		fakeButtonDoneCalledFromTop.Click();
		PersonShowAllEventsWindowBox.person_show_all_events.Hide();
		PersonShowAllEventsWindowBox = null;
	}

	//this is never called when persons is on top
	public void CloseWindowAfterLoadSession ()
	{
		PersonShowAllEventsWindowBox.person_show_all_events.Hide();
		PersonShowAllEventsWindowBox = null;
	}

	private void on_delete_event (object o, DeleteEventArgs args)
	{
		fakeButtonDoneCalledFromTop.Click();
		PersonShowAllEventsWindowBox.person_show_all_events.Hide();
		PersonShowAllEventsWindowBox = null;
	}

	public int SelectedPersonID
	{
		get { return selectedPersonID; }
	}

	public int SelectedSessionID
	{
		get { return selectedSessionID; }
	}

	public Button FakeButtonLoadSession
	{
		get { return fakeButtonLoadSession; }
	}

	public Button FakeButtonDoneCalledFromTop
	{
		get { return fakeButtonDoneCalledFromTop; }
	}

	private void connectWidgets (Gtk.Builder builder)
	{
		person_show_all_events = (Gtk.Window) builder.GetObject ("person_show_all_events");

		hbox_session_radios = (Gtk.HBox) builder.GetObject ("hbox_session_radios");
		radio_session_current = (Gtk.RadioButton) builder.GetObject ("radio_session_current");
		radio_session_all = (Gtk.RadioButton) builder.GetObject ("radio_session_all");
		label_radio_session_current = (Gtk.Label) builder.GetObject ("label_radio_session_current");
		label_radio_session_all = (Gtk.Label) builder.GetObject ("label_radio_session_all");
		hbox_filter = (Gtk.HBox) builder.GetObject ("hbox_filter");
		label_filter = (Gtk.Label) builder.GetObject ("label_filter");
		entry_filter = (Gtk.Entry) builder.GetObject ("entry_filter");
		label_person = (Gtk.Label) builder.GetObject ("label_person");
		label_person_name = (Gtk.Label) builder.GetObject ("label_person_name");

		treeview_person_show_all_events = (Gtk.TreeView) builder.GetObject ("treeview_person_show_all_events");
		hbox_combo_persons = (Gtk.Box) builder.GetObject ("hbox_combo_persons");
		button_load_session = (Gtk.Button) builder.GetObject ("button_load_session");
	}
}
