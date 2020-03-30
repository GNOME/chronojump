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
 * Copyright (C) 2019   Xavier de Blas <xaviblas@gmail.com>
 */

using Gtk;
using System;
using System.Threading;

public partial class ChronoJumpWindow
{
	static ChronojumpImporter.Result importerResult;
	static Thread threadImport;
	static ChronojumpImporter chronojumpImporter;

	private void on_button_menu_session_advanced_clicked (object o, EventArgs args)
	{
		on_button_import_chronojump_session (o, args);
	}
	private void on_button_import_chronojump_session(object o, EventArgs args)
	{
		sessionLoadWin = SessionLoadWindow.Show (app1, SessionLoadWindow.WindowType.IMPORT_SESSION);

		if (currentSession == null) {
			sessionLoadWin.DisableImportToCurrentSession ();
		}

		sessionLoadWin.Button_import.Clicked -= new EventHandler(on_load_session_accepted_to_import);
		sessionLoadWin.Button_import.Clicked += new EventHandler(on_load_session_accepted_to_import);
		sessionLoadWin.Button_import_confirm_accept.Clicked -= new EventHandler(importSessionFromDatabasePrepare2);
		sessionLoadWin.Button_import_confirm_accept.Clicked += new EventHandler(importSessionFromDatabasePrepare2);
		sessionLoadWin.FakeButton_cancel_maybeDatabaseSwitched.Clicked -= new EventHandler(on_import_cancelled_maybe_database_switched);
		sessionLoadWin.FakeButton_cancel_maybeDatabaseSwitched.Clicked += new EventHandler(on_import_cancelled_maybe_database_switched);
	}

	private void on_import_cancelled_maybe_database_switched (object o, EventArgs args)
	{
		sessionLoadWin.HideAndNull();
		reloadSession();
	}

	//from import session
	private void on_load_session_accepted_to_import(object o, EventArgs args)
	{
		int sourceSession = sessionLoadWin.CurrentSessionId();
		string databasePath = sessionLoadWin.ImportDatabasePath();
		LogB.Information (databasePath);

		Session destinationSession = currentSession;

		if (sessionLoadWin.ImportToNewSession ()) {
			destinationSession = null;
		}

		importSessionFromDatabasePrepare (databasePath, sourceSession, destinationSession);
	}

	private void importSessionFromDatabasePrepare (string databasePath, int sourceSession, Session destinationSession)
	{
		string source_filename = databasePath;
		string destination_filename = Sqlite.DatabaseFilePath;

		int destinationSessionId;
		if (destinationSession == null)
			destinationSessionId = 0;
		else
			destinationSessionId = destinationSession.UniqueID;

		chronojumpImporter = new ChronojumpImporter (app1, source_filename, destination_filename, sourceSession, destinationSessionId, preferences.debugMode);

		if(destinationSessionId == 0)
		{
			sessionLoadWin.NotebookPage(SessionLoadWindow.PAGE_IMPORT_RESULT); //import do and end page
			importSessionFromDatabasePrepare2 (new object(), new EventArgs());
		} else
		{
			string sessionName = ChronojumpImporter.GetSessionName (chronojumpImporter.SourceFile, chronojumpImporter.SourceSession);
			sessionLoadWin.LabelImportSessionName(sessionName);
			sessionLoadWin.LabelImportFile(chronojumpImporter.SourceFile);

			sessionLoadWin.NotebookPage(SessionLoadWindow.PAGE_IMPORT_CONFIRM); //import confirm page
		}
	}

	private void importSessionFromDatabasePrepare2 (object o, EventArgs args)
	{
		LogB.Information("import before thread");	
		LogB.PrintAllThreads = true; //TODO: remove this

		threadImport = new Thread(new ThreadStart(importSessionFromDatabaseDo));
		GLib.Idle.Add (new GLib.IdleHandler (PulseGTKImport));

		LogB.ThreadStart(); 
		threadImport.Start(); 
	}

	//no GTK here!
	private void importSessionFromDatabaseDo()
	{
		importerResult = chronojumpImporter.import ();
	}

	private bool PulseGTKImport ()
	{
		if ( ! threadImport.IsAlive ) {
			LogB.ThreadEnding();
			importSessionFromDatabaseEnd();

			sessionLoadWin.Pulse(chronojumpImporter.MessageToPulsebar);
			sessionLoadWin.PulseEnd();

			LogB.ThreadEnded();
			return false;
		}

		sessionLoadWin.Pulse(chronojumpImporter.MessageToPulsebar);

		Thread.Sleep (100);
		//LogB.Debug(threadImport.ThreadState.ToString());
		return true;
	}

	private void importSessionFromDatabaseEnd()
	{
		if (importerResult.success)
		{
			//update GUI if events have been added
			//1) simple jump
			createComboSelectJumps(false);
			UtilGtk.ComboUpdate(combo_result_jumps,
					SqliteJumpType.SelectJumpTypes(false, Constants.AllJumpsNameStr(), "", true), ""); //without filter, only select name
			combo_select_jumps.Active = 0;
			combo_result_jumps.Active = 0;

			createComboSelectJumpsDjOptimalFall(false);
			createComboSelectJumpsWeightFVProfile(false);
			createComboSelectJumpsEvolution(false);

			//2) reactive jump
			createComboSelectJumpsRj(false);
			UtilGtk.ComboUpdate(combo_result_jumps_rj,
					SqliteJumpType.SelectJumpRjTypes(Constants.AllJumpsNameStr(), true), ""); //without filter, only select name
			combo_select_jumps_rj.Active = 0;
			combo_result_jumps_rj.Active = 0;

			//3) simple run
			createComboSelectRuns(false);
			UtilGtk.ComboUpdate(combo_result_runs,
					SqliteRunType.SelectRunTypes(Constants.AllRunsNameStr(), true), ""); //without filter, only select name
			combo_select_runs.Active = 0;
			combo_result_runs.Active = 0;

			//4) intervallic run
			createComboSelectRunsInterval(false);
			UtilGtk.ComboUpdate(combo_result_runs_interval,
					SqliteRunIntervalType.SelectRunIntervalTypes(Constants.AllRunsNameStr(), true), ""); //without filter, only select name
			combo_select_runs_interval.Active = 0;
			combo_result_runs_interval.Active = 0;

			// TODO: we need this on encoder or is already done at reloadSession???
			//createEncoderCombos();

			// forceSensor
			fillForceSensorExerciseCombo("");

			// runEncoder
			fillRunEncoderExerciseCombo("");

			//update stats combos
			updateComboStats ();

			reloadSession ();

			//chronojumpImporter.showImportCorrectlyFinished ();
			sessionLoadWin.ShowLabelImportedOk();
		} else {
			LogB.Debug ("Chronojump Importer error: ", importerResult.error);
			sessionLoadWin.ShowImportError(importerResult.error);
		}
	}
}
