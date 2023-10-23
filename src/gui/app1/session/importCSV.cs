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
using GLib; //for Value
using System.Collections.Generic; //List<T>
using Mono.Unix;

//here using app1sae_ , "sae" means session add edit
//this file has been moved from his old window to be part of app1 on Chronojump 2.0

public partial class ChronoJumpWindow
{
	public void on_button_import_from_csv (object o, EventArgs args)
	{
		app1s_notebook.CurrentPage = app1s_PAGE_IMPORT_FROM_CSV;

		app1s_label_import_csv_result.Text = "";
		on_app1s_import_data_type_toggled (o, args);
	}

	private void on_app1s_import_data_type_toggled (object o, EventArgs args)
	{
		TextBuffer tb1 = new TextBuffer (new TextTagTable());

		string str = string.Format ("Column separator has to be '{0}'", preferences.CSVColumnDelimiter);
		str += string.Format ("\tDecimal character has to be '{0}'", preferences.CSVExportDecimalSeparatorChar);
		str += "\nYou can change both in preferences/language.";

		str += "\n\n Data should be:";
		str += "\n- 1st ROW: headers (will be discarded).";
		str += "\n- 1st COLUMN: person names like in Chronojump. All should exist in session.";

		if (app1s_import_jumps_simple.Active)
		{
			str += "\n- 2nd COLUMN: jump simple type in English (should exist in Chronojump).";
			str += "\n- 3rd COLUMN: jump height in seconds.";
		} else if (app1s_import_jumps_multiple.Active) {
			str += "\n\nSorry, Not available yet!";
		} else if (app1s_import_runs_simple.Active) {
			str += "\n\nSorry, Not available yet!";
		} else if (app1s_import_runs_intervallic.Active) {
			str += "\n\nSorry, Not available yet!";
		}

                tb1.Text = str;
		app1s_textview_import_from_csv_format.Buffer = tb1;

		app1s_button_import_csv_select_and_import.Sensitive = app1s_import_jumps_simple.Active;
	}

	private void on_app1s_button_import_csv_select_and_import_clicked (object o, EventArgs args)
	{
		if (currentSession == null || currentSession.UniqueID == 0)
			return;

		// 1) get persons on this session
		List<Person> person_l = SqlitePersonSession.SelectCurrentSessionPersonsAsList (false, currentSession.UniqueID);

		List<object> testType_l = new List<object> ();
		// 2) get tests of this type on DB
		if (app1s_import_jumps_simple.Active)
			testType_l = SqliteJumpType.SelectJumpTypesNew (false, "", "", true) ;
		//else TODO

		// 3) do the import
		//TODO: try it with Gtk.FileChooserWidget, then we do not need to pass app1 (parent)
		Gtk.FileChooserDialog fc = new Gtk.FileChooserDialog ("Select a CSV file",
				app1, FileChooserAction.Open,
				"Cancel",ResponseType.Cancel,
				"Open",ResponseType.Accept);

		fc.Filter = new FileFilter();
		fc.Filter.AddPattern ("*.csv");
		fc.Filter.AddPattern ("*.CSV");

		List<string> error_l = new List<string> ();
		char columnDelimiter = preferences.CSVColumnDelimiter;
		if (fc.Run () == (int)ResponseType.Accept)
		{
			System.IO.FileStream file;
			try {
				file = System.IO.File.OpenRead (fc.Filename);
			} catch {
				LogB.Warning("Catched, maybe is used by another program");
				new DialogMessage (Constants.MessageTypes.WARNING,
						Constants.FileCannotOpenedMaybeSpreadsheetOpened ()
						);
				fc.Destroy();
				return;
			}

			List<string> columns = new List<string>();
			using (var reader = new CsvFileReader(fc.Filename))
			{
				reader.ChangeDelimiter(columnDelimiter);
				int row = 0;
				while (reader.ReadRow (columns))
				{
					int col = 0;
					foreach (string str in columns)
					{
						if (row == 0) //discard first row
							continue;

						LogB.Information (string.Format (
									"row: {0}, col: {1}, content: {2}",
									row, col, str));

						if (col == 0 && ! importCSVPersonExistsInSession (person_l, str))
						{
							error_l.Add (string.Format ("Error: at row {0}: person {1} does not exists in session", row, str));
						}
						else if (col == 1 && ! importCSVTestExists (testType_l, str))
						{
							error_l.Add (string.Format ("Error at row {0}: jump simple {1} does not exists", row, str));
						}
						else if (col == 2 && str == "")
						{
							error_l.Add (string.Format ("Error at row {0}: there is no data", row));
						}

						col ++;
					}

					row ++;
				}
			}
			file.Close();
		}
		fc.Destroy ();

		if (error_l.Count > 0)
		{
			LogB.Information ("errors found:");
			LogB.Information (Util.ListStringToString (error_l));
			app1s_label_import_csv_result.Text = "Errors found, check log.";
		} else
			app1s_label_import_csv_result.Text = "No errors found";
	}

	private bool importCSVPersonExistsInSession (List<Person> person_l, string personName)
	{
		foreach (Person p in person_l)
			if (p.Name == personName)
				return true;

		return false;
	}
	private bool importCSVTestExists (List<object> testType_l, string jumpSimpleTypeName)
	{
		if (app1s_import_jumps_simple.Active)
		{
			foreach (SelectJumpTypes jt in testType_l)
				if (jt.NameEnglish == jumpSimpleTypeName)
					return true;
		}
		// else TODO:

		return false;
	}

	private void on_app1s_button_import_from_csv_close_clicked (object o,EventArgs args)
	{
		app1s_notebook.CurrentPage = app1s_PAGE_MODES;
	}
}


