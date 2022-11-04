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
 * Copyright (C) 2022   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Gdk;
using Glade;
using Mono.Unix;
using System.Collections; //ArrayList
using System.Collections.Generic; //List

public partial class ChronoJumpWindow
{
	private void on_button_person_merge_clicked (object o, EventArgs args)
	{
		if (currentPerson == null || currentPersonSession == null)
			return;

		Person personToMerge = SqlitePerson.Select (false, 752);
		
		if (personToMerge == null)
			return;

		//person
		List<ClassVariance.Struct> pPropDiff_l = currentPerson.MergeWithAnotherGetConflicts (personToMerge);
		if (pPropDiff_l.Count > 0)
		{
			int row = 0;
			foreach (ClassVariance.Struct cvs in pPropDiff_l)
			{
				//LogB.Information (cvs.ToString ());
				//like this the widgets will be managed in each row:
				LogB.Information (string.Format ("Person diff row {0}", row));
				LogB.Information (cvs.Prop.ToString ());
				LogB.Information (cvs.valA.ToString ());
				LogB.Information (cvs.valB.ToString ());
				row ++;
			}
		}

		//personSession
		List<PersonSession> psCurrentPerson_l = SqlitePersonSession.SelectPersonSessionList (currentPerson.UniqueID, -1);
		List<PersonSession> psMergePerson_l = SqlitePersonSession.SelectPersonSessionList (752, -1);

		foreach (PersonSession psCurrentPerson in psCurrentPerson_l)
			foreach (PersonSession psMergePerson in psMergePerson_l)
			{
				if (psCurrentPerson.SessionID == psMergePerson.SessionID)
				{
					List<ClassVariance.Struct> psPropDiff_l = psCurrentPerson.MergeWithAnotherGetConflicts (psMergePerson);
					if (psPropDiff_l.Count > 0)
						foreach (ClassVariance.Struct cvs in psPropDiff_l)
							LogB.Information (cvs.ToString ());
				}
			}

		personMergeWin = PersonMergeWindow.Show (app1,
                                currentSession.UniqueID, currentPerson, preferences.colorBackground);
	}
}

//merge persons win
//all the find select controls based on PersonShowAllEventsWindow
public class PersonMergeWindow
{
	[Widget] Gtk.Window person_merge;

	// widgets like in PersonShowAllEventsWindow
	[Widget] Gtk.HBox hbox_session_radios;
	[Widget] Gtk.RadioButton radio_session_current;
	[Widget] Gtk.RadioButton radio_session_all;
	[Widget] Gtk.Label label_radio_session_current;
	[Widget] Gtk.Label label_radio_session_all;
	[Widget] Gtk.HBox hbox_filter;
	[Widget] Gtk.Label label_filter;
	[Widget] Gtk.Entry entry_filter;
	[Widget] Gtk.Label label_person;
	[Widget] Gtk.Label label_person_name;

	[Widget] Gtk.Box hbox_combo_persons;
	[Widget] Gtk.ComboBox combo_persons;

	// widgets specific to this class
	[Widget] Gtk.Label label_persons_identify;
	[Widget] Gtk.Label label_persons_tests;
	[Widget] Gtk.Label label_persons_confirm;
	[Widget] Gtk.Image image_button_cancel;
	[Widget] Gtk.Image image_button_accept;

	public Gtk.Button fakeButtonDone;

	static PersonMergeWindow PersonMergeWindowBox;

	private int sessionID;
	private Person currentPerson;

	PersonMergeWindow (Gtk.Window parent, int sessionID, Person currentPerson)
	{
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "person_merge.glade", "person_merge", "chronojump");
		gladeXML.Autoconnect(this);

		//put an icon to window
		UtilGtk.IconWindow(person_merge);

		//manage window color
		if(! Config.UseSystemColor)
		{
			UtilGtk.WindowColor(person_merge, Config.ColorBackground);
			/*
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, label_radio_session_current);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, label_radio_session_all);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, label_filter);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, label_person);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, label_person_name);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, label_persons_identify);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, label_persons_tests);
			UtilGtk.ContrastLabelsLabel(Config.ColorBackgroundIsDark, label_persons_confirm);
			*/
		}

		image_button_cancel.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_cancel.png");
		image_button_accept.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_done_blue.png");

		person_merge.Parent = parent;
		this.sessionID = sessionID;
		this.currentPerson = currentPerson;

		fakeButtonDone = new Gtk.Button();

		label_person_name.Text = currentPerson.Name;
		createComboPersons(sessionID, currentPerson.UniqueID.ToString(), currentPerson.Name);
	}

	static public PersonMergeWindow Show (Gtk.Window parent,
			int sessionID, Person currentPerson,// bool allowChangePerson,
			Gdk.Color colorBackground)
	{
		if (PersonMergeWindowBox == null) {
			PersonMergeWindowBox = new PersonMergeWindow (parent, sessionID, currentPerson);
		}

		//if(allowChangePerson)
		//{
			PersonMergeWindowBox.hbox_session_radios.Visible = true;
			PersonMergeWindowBox.hbox_filter.Visible =
				PersonMergeWindowBox.radio_session_all.Active;
			PersonMergeWindowBox.hbox_combo_persons.Visible = true;
			PersonMergeWindowBox.label_person_name.Visible = false;
		/*} else {
			PersonMergeWindowBox.hbox_session_radios.Visible = false;
			PersonMergeWindowBox.hbox_filter.Visible = false;
			PersonMergeWindowBox.entry_filter.Text = "";
			PersonMergeWindowBox.hbox_combo_persons.Visible = false;
			PersonMergeWindowBox.label_person_name.Visible = true;
		}*/

		PersonMergeWindowBox.person_merge.Show ();

		return PersonMergeWindowBox;
	}

	private void on_entry_filter_changed (object o, EventArgs args)
	{
		radio_session_toggled_do (); 	//this will update combo and treeview
	}

	private void createComboPersons (int sessionID, string personID, string personName)
	{
		combo_persons = ComboBox.NewText ();

		int inSession = -1;		//select persons from all sessions
		if (radio_session_current.Active)
			inSession = sessionID;	//select only persons who are on currentSession

		string filter = "";
		if (entry_filter != null && entry_filter.Text != "")
			filter = entry_filter.Text;

		ArrayList myPersonsAll = SqlitePerson.SelectAllPersonsRecuperable("name", -1, inSession, filter);

		//discard current person
		ArrayList myPersons = new ArrayList ();
		foreach (Person p in myPersonsAll)
			if (p.UniqueID != currentPerson.UniqueID)
				myPersons.Add (p);

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

		on_combo_persons_changed (0, new EventArgs ());	//called for updating the widgets ifcombo_persons.Entry changed, and at start.
	}

	private void on_combo_persons_changed (object o, EventArgs args)
	{
		string [] strFull = UtilGtk.ComboGetActive(combo_persons).Split (new char[] {':'});
		if (strFull.Length != 2)
			return;

		label_persons_identify.Text = string.Format (Catalog.GetString ("Merge persons '{0}' with '{1}' in all sessions."), currentPerson.Name, strFull[1]);
		label_persons_tests.Text = string.Format (Catalog.GetString ("All tests of person '{0}' will be assigned to person '{1}'."), currentPerson.Name, strFull[1]);
		label_persons_confirm.Text = Catalog.GetString ("Are you sure?");
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
			createComboPersons(sessionID, myStringFull[0], myStringFull[1] );
		} else
			createComboPersons(sessionID, "-1", "" );

		on_combo_persons_changed (0, new EventArgs ());	//called for updating the widgets ifcombo_persons.Entry changed
	}

	private void on_button_close_clicked (object o, EventArgs args)
	{
		fakeButtonDone.Click();
		PersonMergeWindowBox.person_merge.Hide();
		PersonMergeWindowBox = null;
	}

	private void on_delete_event (object o, DeleteEventArgs args)
	{
		fakeButtonDone.Click();
		PersonMergeWindowBox.person_merge.Hide();
		PersonMergeWindowBox = null;
	}

	public Button FakeButtonDone
	{
		get { return fakeButtonDone; }
	}
}