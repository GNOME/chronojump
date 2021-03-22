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
 * Copyright (C) 2004-2020   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Glade;
using System.Collections; //ArrayList
using Mono.Unix;


//show all events (jumps and runs) of a person in different sessions
public class PersonShowAllEventsWindow
{
	[Widget] Gtk.Window person_show_all_events;

	[Widget] Gtk.CheckButton checkbutton_only_current_session;
	[Widget] Gtk.Label label_checkbutton_only_current_session;
	[Widget] Gtk.Label label_person;
	[Widget] Gtk.Label label_person_name;

	[Widget] Gtk.TreeView treeview_person_show_all_events;
	[Widget] Gtk.Box hbox_combo_persons;
	[Widget] Gtk.ComboBox combo_persons;

	public Gtk.Button fakeButtonDone;

	TreeStore store;
	static PersonShowAllEventsWindow PersonShowAllEventsWindowBox;

	protected int sessionID;
	
	protected Person currentPerson;
	
	PersonShowAllEventsWindow (Gtk.Window parent, int sessionID, Person currentPerson)
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "person_show_all_events.glade", "person_show_all_events", "chronojump");
		gladeXML.Autoconnect(this);
		
		//put an icon to window
		UtilGtk.IconWindow(person_show_all_events);

		//manage window color
		if(! Config.UseSystemColor)
		{
			UtilGtk.WindowColor(person_show_all_events, Config.ColorBackground);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, label_checkbutton_only_current_session);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, label_person);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, label_person_name);
		}

		person_show_all_events.Parent = parent;
		this.sessionID = sessionID;
		this.currentPerson = currentPerson;

		fakeButtonDone = new Gtk.Button();

		label_person_name.Text = currentPerson.Name;
		createComboPersons(sessionID, currentPerson.UniqueID.ToString(), currentPerson.Name);
		createTreeView(treeview_person_show_all_events);
		store = new TreeStore( typeof (string), typeof (string), typeof (string), typeof (string), 
				typeof (string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string),
				typeof (string), typeof (string), typeof(string), typeof(string));
		treeview_person_show_all_events.Model = store;
		fillTreeView(treeview_person_show_all_events,store, currentPerson.UniqueID);
	}
	
	static public PersonShowAllEventsWindow Show (Gtk.Window parent,
			int sessionID, Person currentPerson, bool allowChangePerson, Gdk.Color colorBackground)
	{
		if (PersonShowAllEventsWindowBox == null) {
			PersonShowAllEventsWindowBox = new PersonShowAllEventsWindow (parent, sessionID, currentPerson);
		}

		if(allowChangePerson)
		{
			PersonShowAllEventsWindowBox.checkbutton_only_current_session.Visible = true;
			PersonShowAllEventsWindowBox.hbox_combo_persons.Visible = true;
			PersonShowAllEventsWindowBox.label_person_name.Visible = false;
		} else {
			PersonShowAllEventsWindowBox.checkbutton_only_current_session.Visible = false;
			PersonShowAllEventsWindowBox.hbox_combo_persons.Visible = false;
			PersonShowAllEventsWindowBox.label_person_name.Visible = true;
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
					typeof (string), typeof (string), typeof(string), typeof(string));
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
		tv.AppendColumn ( Catalog.GetString ("Date"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Jumps\nsimple"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Jumps\nreactive"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Races\nsimple"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Races\ninterval"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Reaction\ntime"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Pulses"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("MultiChronopic"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Encoder sets"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Encoder repetitions"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Force sensor"), new CellRendererText(), "text", count++);
		tv.AppendColumn ( Catalog.GetString ("Race analyzer"), new CellRendererText(), "text", count++);
	}
	
	protected void fillTreeView (Gtk.TreeView tv, TreeStore store, int personID) {
		ArrayList myEvents;
		myEvents = SqlitePerson.SelectAllPersonEvents(personID); 

		foreach (string myEvent in myEvents) {
			string [] myStr = myEvent.Split(new char[] {':'});

			store.AppendValues (myStr[0], myStr[1], myStr[2], myStr[3], myStr[4], myStr[5], 
					myStr[6], myStr[7], myStr[8], myStr[9], myStr[10], myStr[11], myStr[12], myStr[13]);
		}
	}

	protected virtual void on_button_close_clicked (object o, EventArgs args)
	{
		fakeButtonDone.Click();
		PersonShowAllEventsWindowBox.person_show_all_events.Hide();
		PersonShowAllEventsWindowBox = null;
	}
	
	protected virtual void on_delete_event (object o, DeleteEventArgs args)
	{
		fakeButtonDone.Click();
		PersonShowAllEventsWindowBox.person_show_all_events.Hide();
		PersonShowAllEventsWindowBox = null;
	}

	public Button FakeButtonDone
	{
		get { return fakeButtonDone; }
	}
}
