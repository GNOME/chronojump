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
	private List<PersonImportConflict> importSessionCheckConflicts (string databasePath, int sourceSession)
	{
		//select all persons on current database
		ArrayList arrayDB = SqlitePersonSession.SelectCurrentSessionPersons (-1, true);

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
			Gtk.Box bSamePerson = new Gtk.Box (Gtk.Orientation.Horizontal, 10);
			Gtk.RadioButton rSamePersonYes = new Gtk.RadioButton ("Yes");
			Gtk.RadioButton rSamePersonNo = new Gtk.RadioButton (rSamePersonYes, "No");
			bSamePerson.PackStart (rSamePersonYes, false, false, 0);
			bSamePerson.PackStart (rSamePersonNo, false, false, 0);

			//col4
			Gtk.Box bNameAuto = new Gtk.Box (Gtk.Orientation.Vertical, 6);
			//row1
			Gtk.Box bNameAutoYesNo = new Gtk.Box (Gtk.Orientation.Horizontal, 10);
			Gtk.RadioButton rNameAutoYes = new Gtk.RadioButton ("Yes");
			Gtk.RadioButton rNameAutoNo = new Gtk.RadioButton (rNameAutoYes, "No");
			bNameAutoYesNo.PackStart (rNameAutoYes, false, false, 0);
			bNameAutoYesNo.PackStart (rNameAutoNo, false, false, 0);
			//row2
			Gtk.Label lNameCustom = new Gtk.Label ("Write new name");
			//row3
			Gtk.Entry eNameCustom = new Gtk.Entry ();
			//row4
			Gtk.Button bNameCustomCheck = new Gtk.Button ("Check availability");
			//row5
			Gtk.Label lNameCustomCheckSuccess = new Gtk.Label ("----");
			//pack all in vertical box
			bNameAuto.PackStart (bNameAutoYesNo, false, false, 0);
			bNameAuto.PackStart (lNameCustom, false, false, 0);
			bNameAuto.PackStart (eNameCustom, false, false, 0);
			bNameAuto.PackStart (bNameCustomCheck, false, false, 0);
			bNameAuto.PackStart (lNameCustomCheckSuccess, false, false, 0);

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
		if(! Config.UseSystemColor)
			UtilGtk.ContrastLabelsGrid (Config.ColorBackgroundShiftedIsDark, grid_import_session_issues_persons);
	}
}
