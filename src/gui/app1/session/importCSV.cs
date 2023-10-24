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

//here using app1sae_ , "sae" means session add edit
//this file has been moved from his old window to be part of app1 on Chronojump 2.0

public partial class ChronoJumpWindow
{
	public void on_button_import_from_csv (object o, EventArgs args)
	{
		app1s_notebook.CurrentPage = app1s_PAGE_IMPORT_FROM_CSV;

		app1s_label_import_csv_result.Text = "";
		app1s_button_import_from_csv_view_errors.Visible = false;
		notebook_session_import_from_csv.Page = 0;

		on_app1s_import_data_type_toggled (o, args); //to fill format treeview
	}

	private void on_app1s_import_data_type_toggled (object o, EventArgs args)
	{
		TextBuffer tb1 = new TextBuffer (new TextTagTable());

		string str = string.Format ("Column separator should be '{0}'", preferences.CSVColumnDelimiter);
		str += string.Format ("\tDecimal character should be '{0}'", preferences.CSVExportDecimalSeparatorChar);
		str += "\nYou can change both in preferences/language.";

		str += "\n\n Data should be:";
		str += "\n- 1st ROW: headers (will be discarded).";
		str += "\n- 1st COLUMN: person names like in Chronojump. All should exist in session.";

		if (app1s_import_jumps_simple.Active)
		{
			str += "\n- 2nd COLUMN: jump simple type in English (should exist in Chronojump).";
			str += "\n- 3rd COLUMN: jump flight time in seconds.";
			str += "\n- 4th COLUMN (optional): jump contact time in seconds.";
			str += "\n- 5th COLUMN (optional): jump falling height in cm.";
			str += "\n- 6th COLUMN (optional): jump weight in percentage (without the '%' sign).";
		} else if (app1s_import_jumps_multiple.Active) {
			str = "\nSorry, Not available yet!";
		} else if (app1s_import_runs_simple.Active) {
			str = "\nSorry, Not available yet!";
		} else if (app1s_import_runs_intervallic.Active) {
			str += "\n- 2nd COLUMN: run interval type in English (should exist in Chronojump).";
			str += "\n- 3rd COLUMN: starts in contact with the photocell? T/F or t/f.";
			str += "\n- 4th COLUMN: total distance in meters.";
			str += "\n- 5th COLUMN: number of tracks (all the tracks need to have same distance).";
			str += "\n- 6th, 7th, ... COLUMNS: time in seconds of each of the tracks.";
		}

                tb1.Text = str;
		app1s_textview_import_from_csv_format.Buffer = tb1;

		app1s_button_import_csv_select_and_import.Sensitive = (app1s_import_jumps_simple.Active || app1s_import_runs_intervallic.Active);
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
		else if (app1s_import_runs_intervallic.Active)
			testType_l = SqliteRunIntervalType.SelectRunIntervalTypesNew ("", true) ;
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
		List<Jump> jumpSimpleToImport_l = new List<Jump> ();
		List<RunInterval> runIToImport_l = new List<RunInterval> ();

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

			if (app1s_import_jumps_simple.Active)
				jumpSimpleToImport_l = importCSVReadFileJumpSimple (fc.Filename, person_l, testType_l, ref error_l);
			else if (app1s_import_runs_intervallic.Active)
				runIToImport_l = importCSVReadFileRunInterval (fc.Filename, person_l, testType_l, ref error_l);

			file.Close();

			if (error_l.Count > 0)
			{
				app1s_button_import_from_csv_view_errors.Visible = true;
				app1s_label_import_csv_result.Text = string.Format ("{0} errors found.", error_l.Count);

				TextBuffer tb2 = new TextBuffer (new TextTagTable());
		                tb2.Text = Util.ListStringToString (error_l);
				app1s_textview_import_from_csv_errors.Buffer = tb2;
			} else {
				app1s_button_import_from_csv_view_errors.Visible = false;
				int importedCount = 0;

				Sqlite.Open (); // ---->

				if (app1s_import_jumps_simple.Active && jumpSimpleToImport_l.Count > 0)
				{
					foreach (Jump j in jumpSimpleToImport_l)
					{
						j.InsertAtDB (true, Constants.JumpTable);
						importedCount ++;
					}
					pre_fillTreeView_jumps (true);
				}
				else if (app1s_import_runs_intervallic.Active && runIToImport_l.Count > 0)
				{
					foreach (RunInterval ri in runIToImport_l)
					{
						ri.InsertAtDB (true, Constants.RunIntervalTable);
						importedCount ++;
					}
					pre_fillTreeView_runs_interval (true);
				}

				Sqlite.Close (); // <----

				app1s_label_import_csv_result.Text = string.Format ("Imported {0} records", importedCount);
			}
		}
		fc.Destroy ();
	}

	private List<Jump> importCSVReadFileJumpSimple (string filename, List<Person> person_l, List<object> testType_l, ref List<string> error_l)
	{
		List<Jump> jumpSimpleToImport_l = new List<Jump> ();

		List<string> columns = new List<string>();
		using (var reader = new CsvFileReader (filename))
		{
			reader.ChangeDelimiter (preferences.CSVColumnDelimiter);
			int row = 0;
			while (reader.ReadRow (columns))
			{
				int col = 0;
				bool rowErrors = false;
				string personName = "";
				string jType = "";
				double jTv = 0;
				double jTc = 0;
				double jFall = 0;
				double jWeightPercent = 0;

				foreach (string str in columns)
				{
					if (row == 0) //discard first row
						continue;

					//LogB.Information (string.Format ("row: {0}, col: {1}, content: {2}", row, col, str));

					if (col == 0)
					{
						if (! importCSVPersonExistsInSession (person_l, str)) {
							error_l.Add (string.Format ("Row {0}: person '{1}' does not exists in session.", row, str));
							rowErrors = true;
						} else
							personName = str;
					}
					else if (col == 1)
					{
						if (! importCSVTestExists (testType_l, str)) {
							error_l.Add (string.Format ("Row {0}: jump simple '{1}' does not exists.", row, str));
							rowErrors = true;
						} else
							jType = str;
					}
					else if (col == 2)
					{
						if (str == "") {
							error_l.Add (string.Format ("Row {0}: there is no flight time.", row));
							rowErrors = true;
						} else if (! Util.IsNumber (str, true)) {
							error_l.Add (string.Format ("Row {0}: flight time '{1}' is not a number or decimal character is not correct.", row, str));
							rowErrors = true;
						} else
							jTv = Convert.ToDouble (str);
					}
					else if (col == 3 && str != "")
					{
						if (! Util.IsNumber (str, true)) {
							error_l.Add (string.Format ("Row {0}: contact time '{1}' is not a number or decimal character is not correct.", row, str));
							rowErrors = true;
						} else
							jTc = Convert.ToDouble (str);
					}
					else if (col == 4 && str != "")
					{
						if (! Util.IsNumber (str, true)) {
							error_l.Add (string.Format ("Row {0}: fall '{1}' is not a number or decimal character is not correct.", row, str));
							rowErrors = true;
						} else
							jFall = Convert.ToDouble (str);
					}
					else if (col == 5 && str != "")
					{
						if (! Util.IsNumber (str, true)) {
							error_l.Add (string.Format ("Row {0}: weight '{1}' is not a number or decimal character is not correct.", row, str));
							rowErrors = true;
						} else
							jWeightPercent = Convert.ToDouble (str);
					}

					col ++;
				}

				if (! rowErrors)
				{
					int personID = importCSVPersonFindID (person_l, personName);
					if (personID >= 0)
						jumpSimpleToImport_l.Add (new Jump (-1, personID, currentSession.UniqueID, jType,
									jTv, jTc, jFall, jWeightPercent, "", -1,
									Util.BoolToNegativeInt (false), UtilDate.ToFile (System.DateTime.Now)));
				}

				row ++;
			}
		}

		return jumpSimpleToImport_l;
	}

	private List<RunInterval> importCSVReadFileRunInterval (string filename, List<Person> person_l, List<object> testType_l, ref List<string> error_l)
	{
		List<RunInterval> runIToImport_l = new List<RunInterval> ();

		List<string> columns = new List<string>();
		using (var reader = new CsvFileReader (filename))
		{
			reader.ChangeDelimiter (preferences.CSVColumnDelimiter);
			int row = 0;
			while (reader.ReadRow (columns))
			{
				int col = 0;
				bool rowErrors = false;
				string personName = "";
				string riType = "";
				bool riStartIn = false;
				double riDistanceTotal = 0;
				int riTracks = 0;
				List<string> times_l = new List<string> ();
				double timeTotal = 0;

				foreach (string str in columns)
				{
					if (row == 0) //discard first row
						continue;

					//LogB.Information (string.Format ("row: {0}, col: {1}, content: {2}", row, col, str));

					if (col == 0)
					{
						if (! importCSVPersonExistsInSession (person_l, str)) {
							error_l.Add (string.Format ("Row {0}: person '{1}' does not exists in session.", row, str));
							rowErrors = true;
						} else
							personName = str;
					}
					else if (col == 1)
					{
						if (! importCSVTestExists (testType_l, str)) {
							error_l.Add (string.Format ("Row {0}: run intervallic '{1}' does not exists.", row, str));
							rowErrors = true;
						} else
							riType = str;
					}
					else if (col == 2)
					{
						if (str.ToUpper () != "T" && str.ToUpper () != "F") {
							error_l.Add (string.Format ("Row {0}: startIn found is '{1}' must be 'T', 't', 'F' or 'f'.", row, str));
							rowErrors = true;
						} else
							riStartIn = (str.ToUpper () == "T");
					}
					else if (col == 3)
					{
						if (str == "") {
							error_l.Add (string.Format ("Row {0}: there is no total distance.", row));
							rowErrors = true;
						} else if (! Util.IsNumber (str, true)) {
							error_l.Add (string.Format ("Row {0}: total distance '{1}' is not a number or decimal character is not correct.", row, str));
							rowErrors = true;
						} else
							riDistanceTotal = Convert.ToDouble (str);
					}
					else if (col == 4)
					{
						if (str == "" || ! Util.IsNumber (str, false)) {
							error_l.Add (string.Format ("Row {0}: Tracks '{1}' is not a number an integer number.", row, str));
							rowErrors = true;
						} else
							riTracks = Convert.ToInt32 (str);
					}
					else if (col > 4 && str != "")
					{
						if (! Util.IsNumber (str, true)) {
							error_l.Add (string.Format ("Row {0} col {1}: Time '{2}' is not a number or decimal character is not correct.", row, col, str));
							rowErrors = true;
						} else {
							times_l.Add (str);
							timeTotal += Convert.ToDouble (str);
						}
					}

					col ++;
				}

				if (! rowErrors)
				{
					int personID = importCSVPersonFindID (person_l, personName);
					if (personID >= 0)
						runIToImport_l.Add (new RunInterval (-1, personID, currentSession.UniqueID, riType,
									riDistanceTotal, timeTotal,
									UtilAll.DivideSafe (riDistanceTotal, riTracks), //distanceInterval
									Util.ListStringToString (times_l, "="), 	//intervalTimesString
									riTracks, "",  					//tracks, description
									riTracks + "R",					//limited
									Util.BoolToNegativeInt (false), ! riStartIn, 	//simulated, initialSpeed
									UtilDate.ToFile (System.DateTime.Now),		//datetime
									new List<int> ()				//photocell_l
									));
				}

				row ++;
			}
		}

		return runIToImport_l;
	}

	private bool importCSVPersonExistsInSession (List<Person> person_l, string personName)
	{
		foreach (Person p in person_l)
			if (p.Name == personName)
				return true;

		return false;
	}

	private int importCSVPersonFindID (List<Person> person_l, string personName)
	{
		foreach (Person p in person_l)
			if (p.Name == personName)
				return p.UniqueID;

		return -1;
	}

	private bool importCSVTestExists (List<object> testType_l, string typeName)
	{
		if (app1s_import_jumps_simple.Active)
		{
			foreach (SelectJumpTypes s in testType_l)
				if (s.NameEnglish == typeName)
					return true;
		}
		else if (app1s_import_runs_intervallic.Active)
		{
			foreach (SelectRunITypes s in testType_l)
				if (s.NameEnglish == typeName)
					return true;
		}
		// else TODO:

		return false;
	}

	private void on_app1s_button_import_from_csv_view_errors_clicked (object o, EventArgs args)
	{
		notebook_session_import_from_csv.Page = 1;
		app1s_button_import_from_csv_close.Sensitive = false;
	}
	private void on_app1s_button_import_from_csv_errors_back_clicked (object o, EventArgs args)
	{
		notebook_session_import_from_csv.Page = 0;
		app1s_button_import_from_csv_close.Sensitive = true;
	}

	private void on_app1s_button_import_from_csv_close_clicked (object o, EventArgs args)
	{
		app1s_notebook.CurrentPage = app1s_PAGE_MODES;
	}
}


