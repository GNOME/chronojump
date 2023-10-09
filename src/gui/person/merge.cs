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
 * Copyright (C) 2023   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using Gtk;
using Gdk;
//using Glade;
using Mono.Unix;
using System.Collections; //ArrayList
using System.Collections.Generic; //List

public partial class ChronoJumpWindow
{
	private void on_button_person_merge_clicked (object o, EventArgs args)
	{
		if (currentPerson == null || currentPersonSession == null)
			return;

		person_merge_called_from_person_select_window = false;
		person_merge_do ();
	}

	private void person_merge_do ()
	{
		personMergeWin = PersonMergeWindow.Show (app1,
                                currentSession.UniqueID, currentPerson, preferences.colorBackground);

		personMergeWin.FakeButtonDone.Clicked -= new EventHandler (on_button_person_merge_done);
		personMergeWin.FakeButtonDone.Clicked += new EventHandler (on_button_person_merge_done);

		if (person_merge_called_from_person_select_window)
		{
			personMergeWin.FakeButtonCancel.Clicked -= new EventHandler (on_button_top_person_clicked);
			personMergeWin.FakeButtonCancel.Clicked += new EventHandler (on_button_top_person_clicked);
		}
	}

	private void on_button_person_merge_done (object o, EventArgs args)
	{
		personMergeWin.FakeButtonDone.Clicked -= new EventHandler (on_button_person_merge_done);
		currentPerson = personMergeWin.CurrentPerson;
		currentPersonSession = personMergeWin.CurrentPersonSession;

		label_person_change ();
		personChanged ();
		resetAllTreeViews(true, true, true); //fillTests, resetPersons, fillPersons

		personMergeWin.HideAndNull ();

		if (person_merge_called_from_person_select_window)
		{
			//recreate list
			ArrayList myPersons = SqlitePersonSession.SelectCurrentSessionPersons(
					currentSession.UniqueID,
					false); //means: do not returnPersonAndPSlist
			personSelectWin.Update(myPersons);

			//show personTop window again
			on_button_top_person_clicked (o, args);
		}
	}
}

//merge persons win
//all the find select controls based on PersonShowAllEventsWindow
public class PersonMergeWindow
{
	// at glade ---->
	Gtk.Window person_merge;

	// widgets like in PersonShowAllEventsWindow
	Gtk.HBox hbox_session_radios;
	Gtk.RadioButton radio_session_current;
	Gtk.RadioButton radio_session_all;
	Gtk.HBox hbox_filter;
	Gtk.Entry entry_filter;
	Gtk.Label label_person_name;

	Gtk.Box hbox_combo_persons;

	// widgets specific to this class
	Gtk.Notebook notebook;
	Gtk.Label label_persons_identify;
	Gtk.Label label_persons_tests;
	Gtk.Image image_button_cancel;
	Gtk.Image image_button_back;
	Gtk.Image image_button_accept;
	Gtk.Image image_button_merge;
	Gtk.Grid grid_diffs;
	// <---- at glade

	Gtk.ComboBoxText combo_persons;
	private Gtk.Button fakeButtonDone;
	private Gtk.Button fakeButtonCancel;

	static PersonMergeWindow PersonMergeWindowBox;

	private int sessionID;
	private Person currentPerson;
	private Person personToMerge;

	List<ClassVariance.Struct> pDiff_l;
	List<List<ClassVariance.Struct>> psDiffAllSessions_l;
	List<Session> sessionDiff_l;

	RadioButton pRadioA; //person diffs radiobutton
	//Lists of personSession diffs at each session:
	List<RadioButton> psRadiosA_l;

	PersonMergeWindow (Gtk.Window parent, int sessionID, Person currentPerson)
	{
		/*
		Glade.XML gladeXML;
		gladeXML = Glade.XML.FromAssembly (Util.GetGladePath() + "person_merge.glade", "person_merge", "chronojump");
		gladeXML.Autoconnect(this);
		*/
		Gtk.Builder builder = new Gtk.Builder (null, Util.GetGladePath () + "person_merge.glade", null);
		connectWidgets (builder);
		builder.Autoconnect (this);

		//put an icon to window
		UtilGtk.IconWindow(person_merge);

		//manage window color
		if(! Config.UseSystemColor)
		{
			UtilGtk.WindowColor(person_merge, Config.ColorBackground);

			UtilGtk.WidgetColor (notebook, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsNotebook (Config.ColorBackgroundShiftedIsDark, notebook);
		}

		image_button_cancel.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_cancel.png");
		image_button_back.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "arrow_back.png");
		image_button_accept.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "image_done_blue.png");
		image_button_merge.Pixbuf = new Pixbuf (null, Util.GetImagePath(false) + "merge.png");

		person_merge.Parent = parent;
		this.sessionID = sessionID;
		this.currentPerson = currentPerson;

		fakeButtonDone = new Gtk.Button();
		fakeButtonCancel = new Gtk.Button();

		label_person_name.Text = currentPerson.Name;
		createComboPersons(sessionID, currentPerson.UniqueID.ToString(), currentPerson.Name);
	}

	static public PersonMergeWindow Show (Gtk.Window parent,
			int sessionID, Person currentPerson,
			RGBA colorBackground)
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
		combo_persons = new ComboBoxText ();

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
		label_persons_tests.Text = string.Format (Catalog.GetString ("All tests of person '{0}' will be permanently assigned to person '{1}'."), strFull[1], currentPerson.Name);
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
		List<PersonSession> psCurrentPerson_l = SqlitePersonSession.SelectPersonSessionList (false, currentPerson.UniqueID, -1);
		List<PersonSession> psMergePerson_l = SqlitePersonSession.SelectPersonSessionList (false, personToMergeID, -1);

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
					if (psDiffThisSession_l.Count > 1) //there is at least the difference between uniqueID
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
		if (grid_diffs != null && grid_diffs.Children.Length > 0)
			foreach (Gtk.Widget w in grid_diffs.Children)
				grid_diffs.Remove (w);

		//TODO: care if some of the lists are null

		//person
		int row = 0;
		grid_diffs.ColumnSpacing = 4;
		grid_diffs.RowSpacing = 4;
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

		grid_diffs.ShowAll ();
		//scrolledWin.ShowAll ();
	
		if(! Config.UseSystemColor)
			UtilGtk.ContrastLabelsWidget (Config.ColorBackgroundShiftedIsDark, grid_diffs);
	}

	//each personSession row has first a combined row with session title
	private void createTitleRow (string firstCol, string col2_3, int row)
	{
		Gtk.Label l1 = new Gtk.Label (firstCol);
		grid_diffs.Attach (l1, 0, row, 1, 1);
		l1.Hexpand = true;

		Gtk.Label l2_3 = new Gtk.Label (col2_3);
		l2_3.UseMarkup = true;
		grid_diffs.Attach (l2_3, 1, row, 2, 1);
		l2_3.Hexpand = true;
	}

	private void createPersonRadiosRow (int row)
	{
		Gtk.Label l = new Gtk.Label ("");
		pRadioA = new Gtk.RadioButton (Catalog.GetString ("Use these values"));
		Gtk.RadioButton pRadioB = new Gtk.RadioButton (pRadioA, Catalog.GetString ("Use these values"));

		//to have radios center aligned
		Gtk.HBox hboxA = new Gtk.HBox (false, 1);
		Gtk.HBox hboxB = new Gtk.HBox (false, 1);
		hboxA.PackStart (pRadioA, true, false, 0);
		hboxB.PackStart (pRadioB, true, false, 0);

		grid_diffs.Attach (l, 0, row, 1, 1);
		grid_diffs.Attach (hboxA, 1, row, 1, 1);
		grid_diffs.Attach (hboxB, 2, row, 1, 1);
	}

	private void createPersonSessionRadiosRow (int row)
	{
		Gtk.Label l = new Gtk.Label ("");
		Gtk.RadioButton radioA = new Gtk.RadioButton (Catalog.GetString ("Use these values"));
		Gtk.RadioButton radioB = new Gtk.RadioButton (radioA, Catalog.GetString ("Use these values"));

		//to have radios center aligned
		Gtk.HBox hboxA = new Gtk.HBox (false, 1);
		Gtk.HBox hboxB = new Gtk.HBox (false, 1);
		hboxA.PackStart (radioA, true, false, 0);
		hboxB.PackStart (radioB, true, false, 0);

		grid_diffs.Attach (l, 0, row, 1, 1);
		grid_diffs.Attach (hboxA, 1, row, 1, 1);
		grid_diffs.Attach (hboxB, 2, row, 1, 1);

		psRadiosA_l.Add (radioA);
		//psRadiosB_l.Add (radioB);
	}

	private int createRowsForDiff (List<ClassVariance.Struct> cvs_l, int row)
	{
		foreach (ClassVariance.Struct cvs in cvs_l)
		{
			// do not display personSession.uniqueID
			if (cvs.Prop == "uniqueID")
				continue;

			Gtk.Label lPersonProp = new Gtk.Label (cvs.Prop);
			Gtk.Label lPersonVarA = new Gtk.Label (cvs.valA.ToString ());
			Gtk.Label lPersonVarB = new Gtk.Label (cvs.valB.ToString ());

			if (cvs.Prop == "name")
			{
				lPersonVarA.Text = "'<b>" + lPersonVarA.Text + "</b>'";
				lPersonVarB.Text = "'<b>" + lPersonVarB.Text + "</b>'";
				lPersonVarA.UseMarkup = true;
				lPersonVarB.UseMarkup = true;
			}

			grid_diffs.Attach (lPersonProp, 0, row, 1, 1);
			grid_diffs.Attach (lPersonVarA, 1, row, 1, 1);
			grid_diffs.Attach (lPersonVarB, 2, row, 1, 1);
			row ++;
		}

		return row;
	}

	private void on_button_close_clicked (object o, EventArgs args)
	{
		fakeButtonCancel.Click();

		PersonMergeWindowBox.person_merge.Hide();
		PersonMergeWindowBox = null;
	}

	private void on_button_back_clicked (object o, EventArgs args)
	{
		notebook.CurrentPage = 0;
	}

	/* on an USELESS database, try with:

	   DELETE FROM session; DELETE FROM person77; DELETE FROM personSession77; DELETE FROM jump;

	   INSERT INTO session values (NULL, "1st session", "1st place", "0001-01-01", 1, -1, -1, "", -1);
	   INSERT INTO session values (NULL, "2nd session", "2nd place", "0001-01-01", 1, -1, -1, "", -1);
	   INSERT INTO session values (NULL, "3rd session", "3rd place", "0001-01-01", 1, -1, -1, "", -1);

	   INSERT INTO person77 values (NULL, "p1", "M", "0001-01-01", -1, 1, "", "", "", -1, "");
	   INSERT INTO person77 values (NULL, "p2", "F", "0001-01-01", -1, 1, "", "", "", -1, "");

	   INSERT INTO personSession77 values (NULL, 1, 1, 0, 70, 1, -1, -1, "", 0, 0);
	   INSERT INTO personSession77 values (NULL, 2, 1, 0, 50, 1, -1, -1, "", 0, 0);
	   INSERT INTO personSession77 values (NULL, 1, 2, 0, 72, 1, -1, -1, "", 0, 0);
	   INSERT INTO personSession77 values (NULL, 2, 3, 0, 52, 1, -1, -1, "", 0, 0);

	   INSERT INTO jump VALUES (NULL, 1, 1, "SJ", .7, 0, 0, "", "", 0, 0, "");
	   INSERT INTO jump VALUES (NULL, 2, 1, "CMJ", .5, 0, 0, "", "", 0, 0, "");
	   INSERT INTO jump VALUES (NULL, 1, 2, "SJ", .72, 0, 0, "", "", 0, 0, "");
	   INSERT INTO jump VALUES (NULL, 2, 3, "CMJ", .52, 0, 0, "", "", 0, 0, "");

	   SELECT * FROM session; SELECT * FROM person77; SELECT * FROM personSession77; SELECT * FROM jump;
	 */
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

		Sqlite.Open ();   // --------------------------------->

		// 1) changes in person table
		//Person currentPersonCopy = currentPerson;
		int personIDselected = currentPerson.UniqueID;
		int personIDdiscarded = personToMerge.UniqueID;

		if (pRadioA.Active)
			SqlitePerson.DeletePersonAndImages (true, personToMerge.UniqueID);
		else {
			personIDselected = personToMerge.UniqueID;
			personIDdiscarded = currentPerson.UniqueID;
			SqlitePerson.DeletePersonAndImages (true, currentPerson.UniqueID);
			currentPerson = personToMerge;
		}

		// 2) changes in personSession table for each of the sessions where are diffs between both ps
		count = 0;
		PersonSession ps = new PersonSession ();
		foreach (Gtk.RadioButton psR in psRadiosA_l)
		{
			// get personSession.UniqueID
			int personSessionIDA = -1;
			int personSessionIDB = -1;
			List<ClassVariance.Struct> cvs_l = psDiffAllSessions_l[count];
			foreach (ClassVariance.Struct cvs in cvs_l)
				if (cvs.Prop == "uniqueID")
				{
					personSessionIDA = Convert.ToInt32 (cvs.valA.ToString ());
					personSessionIDB = Convert.ToInt32 (cvs.valB.ToString ());
					break;
				}

			Session s = sessionDiff_l[count];

			// if we selected a personSession different than the person selection, need to put the correct personID on personSession
			if (psR.Active != pRadioA.Active)
			{
				ps = SqlitePersonSession.Select (true, personIDdiscarded, s.UniqueID);
				ps.PersonID = personIDselected; //and update it to use correct personID
				SqlitePersonSession.Update (true, ps);
			}

			// delete the unwanted personSession
			if (psR.Active)
				SqlitePersonSession.DeletePersonSessionOnMerge (true, personSessionIDB);
			else
				SqlitePersonSession.DeletePersonSessionOnMerge (true, personSessionIDA);

			count ++;
		}

		LogB.Information ("personIDselected: " + personIDselected.ToString ());
		LogB.Information ("personIDdiscarded: " + personIDdiscarded.ToString ());

		// 3) Update the personSession tables for sessions that have only the personSession of discarded person
		Sqlite.Update (
                        true, Constants.PersonSessionTable, "personID",
                        personIDdiscarded.ToString (), personIDselected.ToString (),
                        "", "");

		// 3.b) on sessions where there are no differences in session we also need to delete the repeated personSession if any. We need this because 2) only manages sessions where personSession is different
		SqlitePersonSession.DeletePersonSessionsDuplicatedOnMerge (true, personIDselected);

		// 4) If personSession does not exist for current session, create it
		ps = SqlitePersonSession.Select (true, personIDselected, sessionID);
		if (ps.UniqueID < 0)
		{
			ps = SqlitePersonSession.Select (true, personIDselected, -1);

			LogB.Information ("Going to insert personSession: " + ps.ToString ());
			//this inserts in DB
			ps = new PersonSession (
					currentPerson.UniqueID, sessionID,
					ps.Height, ps.Weight,
					ps.SportID, ps.SpeciallityID,
					ps.Practice,
					ps.Comments,
					ps.TrochanterToe,
					ps.TrochanterFloorOnFlexion,
					true); //dbconOpened
		}

		// 5) Change the test tables
		string [] testTables = {
			Constants.JumpTable,
			Constants.JumpRjTable,
			Constants.RunTable,
			Constants.RunIntervalTable,
			Constants.RunEncoderTable,
			Constants.EncoderTable,
			Constants.Encoder1RMTable,
			Constants.ForceSensorTable,
			Constants.PulseTable,
			Constants.ReactionTimeTable,
			Constants.MultiChronopicTable,
		};
		foreach (string table in testTables)
			Sqlite.UpdateTestPersonID (true, table, personIDdiscarded, personIDselected);

		/*
		   - change of person weight should not affect to jump.weight because 100 (%) is 100 (%) for both
		   - raceAnalyzer, encoder, forceSensor files do not change the name as will involve lots of changes on individual sqlite records
		 */

		Sqlite.Close ();   // <--------------------------------

		fakeButtonDone.Click();
	}

	private void on_delete_event (object o, DeleteEventArgs args)
	{
		fakeButtonCancel.Click();

		PersonMergeWindowBox.person_merge.Hide();
		PersonMergeWindowBox = null;
	}

	public void HideAndNull ()
	{
		PersonMergeWindowBox.person_merge.Hide();
		PersonMergeWindowBox = null;
	}

	public Button FakeButtonDone
	{
		get { return fakeButtonDone; }
	}

	public Button FakeButtonCancel
	{
		get { return fakeButtonCancel; }
	}

	public Person CurrentPerson
	{
		get { return currentPerson; }
	}

	public PersonSession CurrentPersonSession
	{
		get { return SqlitePersonSession.Select (false, currentPerson.UniqueID, sessionID); }
	}

	private void connectWidgets (Gtk.Builder builder)
	{
		person_merge = (Gtk.Window) builder.GetObject ("person_merge");

		// widgets like in PersonShowAllEventsWindow
		hbox_session_radios = (Gtk.HBox) builder.GetObject ("hbox_session_radios");
		radio_session_current = (Gtk.RadioButton) builder.GetObject ("radio_session_current");
		radio_session_all = (Gtk.RadioButton) builder.GetObject ("radio_session_all");
		hbox_filter = (Gtk.HBox) builder.GetObject ("hbox_filter");
		entry_filter = (Gtk.Entry) builder.GetObject ("entry_filter");
		label_person_name = (Gtk.Label) builder.GetObject ("label_person_name");

		hbox_combo_persons = (Gtk.Box) builder.GetObject ("hbox_combo_persons");

		// widgets specific to this class
		notebook = (Gtk.Notebook) builder.GetObject ("notebook");
		label_persons_identify = (Gtk.Label) builder.GetObject ("label_persons_identify");
		label_persons_tests = (Gtk.Label) builder.GetObject ("label_persons_tests");
		image_button_cancel = (Gtk.Image) builder.GetObject ("image_button_cancel");
		image_button_back = (Gtk.Image) builder.GetObject ("image_button_back");
		image_button_accept = (Gtk.Image) builder.GetObject ("image_button_accept");
		image_button_merge = (Gtk.Image) builder.GetObject ("image_button_merge");
		grid_diffs = (Gtk.Grid) builder.GetObject ("grid_diffs");
	}
}
