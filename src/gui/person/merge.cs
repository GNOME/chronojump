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
	[Widget] Gtk.Notebook notebook;
	[Widget] Gtk.Label label_persons_identify;
	[Widget] Gtk.Label label_persons_tests;
	[Widget] Gtk.Label label_persons_confirm;
	[Widget] Gtk.Image image_button_cancel;
	[Widget] Gtk.Image image_button_back;
	[Widget] Gtk.Image image_button_accept;
	[Widget] Gtk.Image image_button_merge;
	[Widget] Gtk.Table table_diffs;
	[Widget] Gtk.ScrolledWindow scrolledWin;

	public Gtk.Button fakeButtonDone;

	static PersonMergeWindow PersonMergeWindowBox;

	private int sessionID;
	private Person currentPerson;
	private Person personToMerge;
	private uint padding = 2;

	List<ClassVariance.Struct> pDiff_l;
	List<List<ClassVariance.Struct>> psDiffAllSessions_l;
	List<Session> sessionDiff_l;

	RadioButton pRadioA; //person diffs radiobutton
	//Lists of personSession diffs at each session:
	List<RadioButton> psRadiosA_l;
	//List<RadioButton> psRadiosB_l; //not needed because with psRadiosA_l Active/Not Active is enough

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
		}

		image_button_cancel.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_cancel.png");
		image_button_back.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "arrow_back.png");
		image_button_accept.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_done_blue.png");
		image_button_merge.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "merge.png");

		person_merge.Parent = parent;
		this.sessionID = sessionID;
		this.currentPerson = currentPerson;

		fakeButtonDone = new Gtk.Button();

		label_person_name.Text = currentPerson.Name;
		createComboPersons(sessionID, currentPerson.UniqueID.ToString(), currentPerson.Name);
	}

	static public PersonMergeWindow Show (Gtk.Window parent,
			int sessionID, Person currentPerson,
			Gdk.Color colorBackground)
	{
		if (PersonMergeWindowBox == null) {
			PersonMergeWindowBox = new PersonMergeWindow (parent, sessionID, currentPerson);
		}

		PersonMergeWindowBox.hbox_session_radios.Visible = true;
		PersonMergeWindowBox.hbox_filter.Visible = PersonMergeWindowBox.radio_session_all.Active;
		PersonMergeWindowBox.hbox_combo_persons.Visible = true;
		PersonMergeWindowBox.label_person_name.Visible = false;

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

		ArrayList myPersonsAll = SqlitePerson.SelectAllPersonsRecuperable ("name", -1, inSession, filter);

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
		label_persons_tests.Text = string.Format (Catalog.GetString ("All tests of person '{0}' will be assigned to person '{1}'."), strFull[1], currentPerson.Name);
		//label_persons_confirm.Text = Catalog.GetString ("Are you sure?");
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

	//check the diffs to do the merge
	private void on_button_check_diffs_clicked (object o, EventArgs args)
	{
		// 1) get the person to merge
		string [] strFull = UtilGtk.ComboGetActive(combo_persons).Split (new char[] {':'});
		if (strFull.Length != 2)
			return;

		if (! Util.IsNumber (strFull[0], false))
			return;

		int personToMergeID = Convert.ToInt32(strFull[0]);
		personToMerge = SqlitePerson.Select (false, personToMergeID);

		if (personToMerge == null)
			return;

		// 2) person
		pDiff_l = currentPerson.MergeWithAnotherGetConflicts (personToMerge);

		// 3) personSession
		//select the personSessions where currentPerson and mergePerson are
		List<PersonSession> psCurrentPerson_l = SqlitePersonSession.SelectPersonSessionList (currentPerson.UniqueID, -1);
		List<PersonSession> psMergePerson_l = SqlitePersonSession.SelectPersonSessionList (personToMergeID, -1);

		//Diffs on each session, the top list is each session
		psDiffAllSessions_l = new List<List<ClassVariance.Struct>> ();

		//select all sessions to know session name of each sessionID returned by personSession. This will fill
		List<Session> session_l = SqliteSession.SelectAll (false, Sqlite.Orders_by.ID_DESC);

		//List of sessions where there are differences on personSession, to match name on printed table
		sessionDiff_l = new List<Session> ();

		//List of personSession diffs radios to allow user choose one or the other
		psRadiosA_l = new List<Gtk.RadioButton> ();
		//psRadiosB_l = new List<Gtk.RadioButton> ();

		foreach (PersonSession psCurrentPerson in psCurrentPerson_l)
			foreach (PersonSession psMergePerson in psMergePerson_l)
			{
				if (psCurrentPerson.SessionID == psMergePerson.SessionID)
				{
					List<ClassVariance.Struct> psDiffThisSession_l = psCurrentPerson.MergeWithAnotherGetConflicts (psMergePerson);
					if (psDiffThisSession_l.Count > 0)
					{
						/* debug
						foreach (ClassVariance.Struct cvs in psDiffThisSession_l)
							LogB.Information (cvs.ToString ());
							*/

						//as there are two different lists do this found check to ensure the two lists are same length
						bool found = false;
						foreach (Session session in session_l)
							if (session.UniqueID == psCurrentPerson.SessionID)
							{
								sessionDiff_l.Add (session);
								found = true;
							}

						if (found)
							psDiffAllSessions_l.Add (psDiffThisSession_l);
					}
				}
			}

		notebook.CurrentPage = 1;
		createTable ();
	}

	private void createTable ()
	{
		if (table_diffs != null && table_diffs.Children.Length > 0)
			foreach (Gtk.Widget w in table_diffs.Children)
				table_diffs.Remove (w);

		//TODO: care if some of the lists are null

		//person
		uint row = 0;
		createTitleRow ("\n", "\n " + Catalog.GetString("Differences between persons"), row ++);
		createPersonRadiosRow (row ++);
		row = createRowsForDiff (pDiff_l, row);

		//personSession
		int count = 0;
		foreach (List<ClassVariance.Struct> cvs_l in psDiffAllSessions_l)
		{
			string sessionStr = Catalog.GetString ("Differences in session");
			createTitleRow ("", string.Format ("\n{0} '{1}' ({2})",
						sessionStr, sessionDiff_l[count].Name, sessionDiff_l[count].DateShort), row ++);
			count ++;
			createPersonSessionRadiosRow (row ++);
			row = createRowsForDiff (cvs_l, row);
		}

		table_diffs.ShowAll ();
		//scrolledWin.ShowAll ();
	}

	//each personSession row has first a combined row with session title
	private void createTitleRow (string firstCol, string col2_3, uint row)
	{
		Gtk.Label l1 = new Gtk.Label (firstCol);
		table_diffs.Attach (l1, 0, 1, row, row+1, Gtk.AttachOptions.Expand, Gtk.AttachOptions.Fill, padding, padding);

		Gtk.Label l2_3 = new Gtk.Label (col2_3);
		l2_3.UseMarkup = true;
		table_diffs.Attach (l2_3, 1, 3, row, row+1, Gtk.AttachOptions.Expand, Gtk.AttachOptions.Fill, padding, padding);
	}

	private void createPersonRadiosRow (uint row)
	{
		Gtk.Label l = new Gtk.Label ("");
		pRadioA = new Gtk.RadioButton (Catalog.GetString ("Use these values"));
		Gtk.RadioButton pRadioB = new Gtk.RadioButton (pRadioA, Catalog.GetString ("Use these values"));

		//to have radios center aligned
		Gtk.HBox hboxA = new Gtk.HBox (false, 1);
		Gtk.HBox hboxB = new Gtk.HBox (false, 1);
		hboxA.PackStart (pRadioA, true, false, 0);
		hboxB.PackStart (pRadioB, true, false, 0);

		table_diffs.Attach (l, 0, 1, row, row+1, Gtk.AttachOptions.Fill, Gtk.AttachOptions.Fill, padding, padding);
		table_diffs.Attach (hboxA, 1, 2, row, row+1, Gtk.AttachOptions.Fill, Gtk.AttachOptions.Fill, padding, padding);
		table_diffs.Attach (hboxB, 2, 3, row, row+1, Gtk.AttachOptions.Fill, Gtk.AttachOptions.Fill, padding, padding);
	}

	private void createPersonSessionRadiosRow (uint row)
	{
		Gtk.Label l = new Gtk.Label ("");
		Gtk.RadioButton radioA = new Gtk.RadioButton (Catalog.GetString ("Use these values"));
		Gtk.RadioButton radioB = new Gtk.RadioButton (radioA, Catalog.GetString ("Use these values"));

		//to have radios center aligned
		Gtk.HBox hboxA = new Gtk.HBox (false, 1);
		Gtk.HBox hboxB = new Gtk.HBox (false, 1);
		hboxA.PackStart (radioA, true, false, 0);
		hboxB.PackStart (radioB, true, false, 0);

		table_diffs.Attach (l, 0, 1, row, row+1, Gtk.AttachOptions.Fill, Gtk.AttachOptions.Fill, padding, padding);
		table_diffs.Attach (hboxA, 1, 2, row, row+1, Gtk.AttachOptions.Fill, Gtk.AttachOptions.Fill, padding, padding);
		table_diffs.Attach (hboxB, 2, 3, row, row+1, Gtk.AttachOptions.Fill, Gtk.AttachOptions.Fill, padding, padding);

		psRadiosA_l.Add (radioA);
		//psRadiosB_l.Add (radioB);
	}

	private uint createRowsForDiff (List<ClassVariance.Struct> cvs_l, uint row)
	{
		foreach (ClassVariance.Struct cvs in cvs_l)
		{
			Gtk.Label lPersonProp = new Gtk.Label (cvs.Prop);
			Gtk.Label lPersonVarA = new Gtk.Label (cvs.valA.ToString ());
			Gtk.Label lPersonVarB = new Gtk.Label (cvs.valB.ToString ());

			if (cvs.Prop == "name")
			{
				lPersonVarA.Text = "<b>" + lPersonVarA.Text + "</b>";
				lPersonVarB.Text = "<b>" + lPersonVarB.Text + "</b>";
				lPersonVarA.UseMarkup = true;
				lPersonVarB.UseMarkup = true;
			}

			table_diffs.Attach (lPersonProp, 0, 1, row, row+1, Gtk.AttachOptions.Fill, Gtk.AttachOptions.Fill, padding, padding);
			table_diffs.Attach (lPersonVarA, 1, 2, row, row+1, Gtk.AttachOptions.Fill, Gtk.AttachOptions.Fill, padding, padding);
			table_diffs.Attach (lPersonVarB, 2, 3, row, row+1, Gtk.AttachOptions.Fill, Gtk.AttachOptions.Fill, padding, padding);
			row ++;
		}

		return row;
	}

	private void on_button_close_clicked (object o, EventArgs args)
	{
		fakeButtonDone.Click();
		PersonMergeWindowBox.person_merge.Hide();
		PersonMergeWindowBox = null;
	}

	private void on_button_back_clicked (object o, EventArgs args)
	{
		notebook.CurrentPage = 0;
	}

	private void on_button_merge_clicked (object o, EventArgs args)
	{
		// 0) debug
		LogB.Information ( string.Format ("At person, use default radio? {0}", pRadioA.Active));

		int count = 0;
		foreach (Gtk.RadioButton r in psRadiosA_l)
		{
			Session s = sessionDiff_l[count ++];
			LogB.Information ( string.Format ("At session ({0}) {1} on list, use default radio? {2}", s.UniqueID, s.Name, r.Active));
		}

		// 1) changes in person table (using pDiff_l and pRadioA)
		// 2) changes in personSession table (using psDiffAllSessions_l and psRadiosA_l)
		// 3) changes in all tests tables (using just the personID)
		// 3a) jump
		// 3b) jumpRj
		// 3c) run
		// 3d) runInterval
		// 3e) runEncoder
		// 3f) forceSensor
		// 3g) encoder
		// 3h) encoder1RM
		// unused (not needed) reaction time, pulse, multichronopic
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
