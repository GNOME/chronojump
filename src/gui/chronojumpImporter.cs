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
 * Copyright (C) 2019-2023   Xavier de Blas <xaviblas@gmail.com>
 */

using Gtk;
using System;
using System.IO;
using System.Threading;

public partial class ChronoJumpWindow
{
	static ChronojumpImporter.Result importerResult;
	static Thread threadImport;
	static ChronojumpImporter chronojumpImporter;

	private void on_button_import_chronojump_session(object o, EventArgs args)
	{
		if (operatingSystem == UtilAll.OperatingSystems.LINUX && ! ExecuteProcess.InstalledOnLinux ("7z"))
		{
			showLinux7zInstallMessage ();
			return;
		}

		sessionLoadWindowShow (app1s_windowType.IMPORT_SESSION);
		app1s_radio_import_new_current_sensitive();
	}

	//from import session
	private void on_load_session_accepted_to_import(object o, EventArgs args)
	{
		int sourceSession = app1s_CurrentSessionId();
		string databasePath = app1s_ImportDatabasePath();
		LogB.Information (databasePath);

		Session destinationSession = currentSession;

		if (app1s_ImportToNewSession ()) {
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

		chronojumpImporter = new ChronojumpImporter (source_filename, destination_filename, sourceSession, destinationSessionId,
				preferences.debugMode, preferences.importerPythonVersion);

		if(destinationSessionId == 0)
		{
			app1s_NotebookPage(app1s_PAGE_IMPORT_RESULT); //import do and end page
			importSessionFromDatabasePrepare2 (new object(), new EventArgs());
		} else
		{
			string sessionName = ChronojumpImporter.GetSessionName (chronojumpImporter.SourceFile, chronojumpImporter.SourceSession, preferences.importerPythonVersion);
			app1s_LabelImportSessionName(sessionName);
			app1s_LabelImportFile(chronojumpImporter.SourceFile);

			app1s_NotebookPage(app1s_PAGE_IMPORT_CONFIRM); //import confirm page
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

			app1s_ImportPulse(chronojumpImporter.MessageToPulsebar);
			app1s_ImportPulseEnd();

			//to not allow to load a session or create a new session until close session/more
			menus_sensitive_import_not_danger(false);

			//change here SQL because radios changed before done on importing session
			sqlChangeSessionLoadDisplay();

			LogB.ThreadEnded();
			return false;
		}

		string message = chronojumpImporter.MessageToPulsebar;
		string statusDir = Util.GetDatabaseTempImportDir() + Path.DirectorySeparatorChar +
			"status" + Path.DirectorySeparatorChar;

		message = getRealtimeMessage(statusDir, message);

		app1s_ImportPulse(message);

		Thread.Sleep (100);
		//LogB.Debug(threadImport.ThreadState.ToString());
		return true;
	}

	private string getRealtimeMessage(string statusDir, string message)
	{
		// files are created in opposite order as shown here

		if(File.Exists(statusDir + "allData.txt"))
			return "All data imported, finishing";

		if(File.Exists(statusDir + "runEncoder.txt"))
			return "Importing race analyzer files";

		if(File.Exists(statusDir + "forceSensor.txt"))
			return "Importing forceSensor files";

		if(File.Exists(statusDir + "encoder.txt"))
			return "Importing encoder files";

		if(File.Exists(statusDir + "runs.txt"))
			return "Importing races";

		if(File.Exists(statusDir + "jumps.txt"))
			return "Importing jumps";

		if(File.Exists(statusDir + "persons.txt"))
			return "Importing persons";

		return message;
	}

	private void importSessionFromDatabaseEnd()
	{
		if (importerResult.success)
		{
			//update GUI if events have been added
			fillAllCombos ();

			reloadSession ();

			app1s_scrolledwindow_import_error.Visible = false;

			//chronojumpImporter.showImportCorrectlyFinished ();
			app1s_ShowLabelImportedOk();
		} else {
			LogB.Debug ("Chronojump Importer error: ", importerResult.error);
			app1s_ShowImportError(importerResult.error);
		}
	}
}
