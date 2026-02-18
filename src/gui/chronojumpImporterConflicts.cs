/*
 * This file is part of ChronoJump
 *
 * ChronoJump is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or   
 * (at your option) any later version.
 *    
 * ChronoJump is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 * Copyright (C) 2024   Xavier de Blas <xaviblas@gmail.com>
 */

using Gtk;
using System;
using System.Collections; //ArrayList

public partial class ChronoJumpWindow
{
	List<Gtk.RadioButton> importerConflictsSamePersonYes_l = new List <Gtk.RadioButton> ();
	List<Gtk.RadioButton> importerConflictsSamePersonNo_l = new List <Gtk.RadioButton> ();
	List<Gtk.Box> importerConflictsBoxNameAuto_l = new List <Gtk.Box> ();
	List<Gtk.Box> importerConflictsBoxNameAuto2_l = new List <Gtk.Box> ();
	List<Gtk.RadioButton> importerConflictsNameAutoYes_l = new List <Gtk.RadioButton> ();
	List<Gtk.RadioButton> importerConflictsNameAutoNo_l = new List <Gtk.RadioButton> ();

	private List<PersonImportConflict> importSessionCheckConflicts (string databasePath, int sourceSession)
	{
		//select all persons on current database
		ArrayList arrayDB = SqlitePersonSession.SelectCurrentSessionPersons (
				-1, 	// all sessions
				false, 	// false ifAllSessionsGetLastOfEachPerson: means get all rows of all sessions of each person
				true); 	// returnPersonAndPSlist

		//select all sessions on current database
		List<Session> sessionsDB_l = SqliteSession.SelectAll (false, Sqlite.Orders_by.ID_ASC);

		SqliteSessionSwitcher sessionSwitcher = new SqliteSessionSwitcher (
				SqliteSessionSwitcher.DatabaseType.IMPORT, databasePath);

		//select all persons on Importing Session (IS)
		ArrayList arrayIS = sessionSwitcher.SelectPersonsInSession (sourceSession);
		sessionSwitcher = null;

		List<PersonImportConflict> pic_l = new List<PersonImportConflict> ();
		//bool conflicts = false;
		if (arrayIS.Count > 0)
		{
			//PersonAndPSUtil.CompareAtImportPrintStr (arrayDB, arrayIS);

			pic_l = PersonAndPSUtil.CompareAtImport (arrayDB, arrayIS, sessionsDB_l);

			LogB.Information ("printing PersonImportConflicts");
			foreach (PersonImportConflict pic in pic_l)
			{
				//conflicts = true;
				LogB.Information (pic.ToString ());
			}
		}

		return pic_l;
	}

	private void importSessionConflictsFillGrid (List<PersonImportConflict> pic_l)
	{
		importerConflictsSamePersonYes_l = new List <Gtk.RadioButton> ();
		importerConflictsSamePersonNo_l = new List <Gtk.RadioButton> ();
		importerConflictsNameAutoYes_l = new List <Gtk.RadioButton> ();
		importerConflictsNameAutoNo_l = new List <Gtk.RadioButton> ();
		importerConflictsBoxNameAuto_l = new List <Gtk.Box> ();
		importerConflictsBoxNameAuto2_l = new List <Gtk.Box> ();

		UtilGtk.RemoveChildren (grid_import_session_issues_persons);
		grid_import_session_issues_persons.ColumnSpacing = 20;
		grid_import_session_issues_persons.RowSpacing = 14;

		Gtk.Label lCol1Header = new Gtk.Label ("<b>" + "Person name" + "</b>");
		Gtk.Label lCol2Header = new Gtk.Label ("<b>" + "Already present in session/s" + "</b>");
		Gtk.Label lCol3Header = new Gtk.Label ("<b>" + "Is the same person?" + "</b>");
		Gtk.Label lCol4Header = new Gtk.Label ("<b>" + "Assign automatic name" + "</b>");
		lCol1Header.UseMarkup = true;
		lCol2Header.UseMarkup = true;
		lCol3Header.UseMarkup = true;
		lCol4Header.UseMarkup = true;
		grid_import_session_issues_persons.Attach (lCol1Header, 0, 0, 1, 1);
		grid_import_session_issues_persons.Attach (lCol2Header, 1, 0, 1, 1);
		grid_import_session_issues_persons.Attach (lCol3Header, 2, 0, 1, 1);
		grid_import_session_issues_persons.Attach (lCol4Header, 3, 0, 1, 1);

		int row = 1;
		foreach (PersonImportConflict pic in pic_l)
		{
			//col1
			Gtk.Label lName = new Gtk.Label (pic.NameImporting);

			//col2
			Gtk.Label lSessions = new Gtk.Label (pic.SessionsAtLocalDB);

			//col3
			//create and attach widgets
			Gtk.Box bSamePerson = new Gtk.Box (Gtk.Orientation.Horizontal, 10);
			Gtk.RadioButton rSamePersonYes = new Gtk.RadioButton ("Yes");
			Gtk.RadioButton rSamePersonNo = new Gtk.RadioButton (rSamePersonYes, "No");
			bSamePerson.PackStart (rSamePersonYes, false, false, 0);
			bSamePerson.PackStart (rSamePersonNo, false, false, 0);
			//connect signals
			rSamePersonYes.Clicked -= new EventHandler (on_importerConflictsSamePersonYes_radio_clicked);
			rSamePersonYes.Clicked += new EventHandler (on_importerConflictsSamePersonYes_radio_clicked);
			rSamePersonNo.Clicked -= new EventHandler (on_importerConflictsSamePersonNo_radio_clicked);
			rSamePersonNo.Clicked += new EventHandler (on_importerConflictsSamePersonNo_radio_clicked);
			//add to lists
			importerConflictsSamePersonYes_l.Add (rSamePersonYes);
			importerConflictsSamePersonNo_l.Add (rSamePersonNo);

			//col4
			Gtk.Box bNameAuto = new Gtk.Box (Gtk.Orientation.Vertical, 6);
			//row1
			Gtk.Box bNameAutoYesNo = new Gtk.Box (Gtk.Orientation.Horizontal, 10);
			Gtk.RadioButton rNameAutoYes = new Gtk.RadioButton ("Yes");
			Gtk.RadioButton rNameAutoNo = new Gtk.RadioButton (rNameAutoYes, "No");
			bNameAutoYesNo.PackStart (rNameAutoYes, false, false, 0);
			bNameAutoYesNo.PackStart (rNameAutoNo, false, false, 0);
			//row2
			Gtk.Box bNameAuto2 = new Gtk.Box (Gtk.Orientation.Vertical, 6);
			//row2.1
			Gtk.Label lNameCustom = new Gtk.Label ("Write new name");
			//row2.2
			Gtk.Entry eNameCustom = new Gtk.Entry ();
			//row2.3
			Gtk.Button bNameCustomCheck = new Gtk.Button ("Check availability");
			//row2.4
			Gtk.Label lNameCustomCheckSuccess = new Gtk.Label ("----");
			//pack all
			bNameAuto.PackStart (bNameAutoYesNo, false, false, 0);
			bNameAuto2.PackStart (lNameCustom, false, false, 0);
			bNameAuto2.PackStart (eNameCustom, false, false, 0);
			bNameAuto2.PackStart (bNameCustomCheck, false, false, 0);
			bNameAuto2.PackStart (lNameCustomCheckSuccess, false, false, 0);
			bNameAuto.PackStart (bNameAuto2, false, false, 0);

			//connect signals
			rNameAutoYes.Clicked -= new EventHandler (on_importerConflictsNameAutoYes_radio_clicked);
			rNameAutoYes.Clicked += new EventHandler (on_importerConflictsNameAutoYes_radio_clicked);
			rNameAutoNo.Clicked -= new EventHandler (on_importerConflictsNameAutoNo_radio_clicked);
			rNameAutoNo.Clicked += new EventHandler (on_importerConflictsNameAutoNo_radio_clicked);
			//add to lists
			importerConflictsNameAutoYes_l.Add (rNameAutoYes);
			importerConflictsNameAutoNo_l.Add (rNameAutoNo);
			importerConflictsBoxNameAuto_l.Add (bNameAuto);
			importerConflictsBoxNameAuto2_l.Add (bNameAuto2);
			//hide this cell
			//bNameAuto.Visible = false; //it is hidden after the ShowAll

			//grid vertical aligns
			lName.Valign = Gtk.Align.Start;
			lSessions.Valign = Gtk.Align.Start;
			bSamePerson.Valign = Gtk.Align.Start;
			bNameAuto.Valign = Gtk.Align.Start;

			//grid attach
			grid_import_session_issues_persons.Attach (lName, 0, row, 1, 1);
			grid_import_session_issues_persons.Attach (lSessions, 1, row, 1, 1);
			grid_import_session_issues_persons.Attach (bSamePerson, 2, row, 1, 1);
			grid_import_session_issues_persons.Attach (bNameAuto, 3, row, 1, 1);

			row ++;
		}

		grid_import_session_issues_persons.ShowAll ();
		//but hide the column4 widgets
		foreach (Gtk.Box b in importerConflictsBoxNameAuto_l)
			b.Visible = false;

		if(! Config.UseSystemColor)
		{
			UtilGtk.WidgetColor (box_session_import_conflicts, Config.ColorBackgroundShifted);
			UtilGtk.ContrastLabelsBox (Config.ColorBackgroundShiftedIsDark, box_session_import_conflicts);
		}
	}

	private void on_importerConflictsSamePersonYes_radio_clicked (object o, EventArgs args)
	{
		Gtk.RadioButton r = (Gtk.RadioButton) o;
		if (r == null || ! r.Active)
			return;

		importerConflictsManageVisibilities (UtilGtk.GetRadioButtonPosInList (
					importerConflictsSamePersonYes_l, r));
	}
	private void on_importerConflictsSamePersonNo_radio_clicked (object o, EventArgs args)
	{
		Gtk.RadioButton r = (Gtk.RadioButton) o;
		if (r == null || ! r.Active)
			return;

		importerConflictsManageVisibilities (UtilGtk.GetRadioButtonPosInList (
					importerConflictsSamePersonNo_l, r));
	}

	private void on_importerConflictsNameAutoYes_radio_clicked (object o, EventArgs args)
	{
		Gtk.RadioButton r = (Gtk.RadioButton) o;
		if (r == null || ! r.Active)
			return;

		importerConflictsManageVisibilities (UtilGtk.GetRadioButtonPosInList (
					importerConflictsNameAutoYes_l, r));
	}
	private void on_importerConflictsNameAutoNo_radio_clicked (object o, EventArgs args)
	{
		Gtk.RadioButton r = (Gtk.RadioButton) o;
		if (r == null || ! r.Active)
			return;

		importerConflictsManageVisibilities (UtilGtk.GetRadioButtonPosInList (
					importerConflictsNameAutoNo_l, r));
	}

	private void importerConflictsManageVisibilities (int row)
	{
		if (row < 0)
			return;

		importerConflictsBoxNameAuto_l[row].Visible = ! importerConflictsSamePersonYes_l[row].Active;
		importerConflictsBoxNameAuto2_l[row].Visible =
			! importerConflictsSamePersonYes_l[row].Active &&
			! importerConflictsNameAutoYes_l[row].Active;
	}
}
