using System.Collections; //ArrayList
using System.Collections.Generic; //List
using System.Json;
using System.Diagnostics;
using System;
using System.IO;
using Mono.Unix;

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
 * Copyright (C) 2016   Carles Pina i Estany <carles@pina.cat>
 */


class ChronojumpImporter
{
	// Database that it's importing from
	private string sourceFile;

	// Database that it's importing to (usually chronojump.db database)
	private string destinationFile;

	// Session that will import
	private int sourceSession;

	// Session that we will import into. If it's 0 it means into to create
	// a new session, otherwise it will import it into the session indicated by it
	private int destinationSession;

	Gtk.Window parentWindow;

	// Result struct holds the output, error and success operations. It's used to pass
	// errors from different layers (e.g. executing Python scripts) to the UI layer
	public struct Result
	{
		public bool success;
		public string output;
		public string error;

		public Result(bool success, string output, string error = "")
		{
			this.success = success;
			this.output = output;
			this.error = error;
		}
	}

	// ChronojumpImporter class imports a specific session from sourceFile to destinationFile.
	// The main method is "import()" which does all the work.
	public ChronojumpImporter(Gtk.Window parentWindow, string sourceFile, string destinationFile, int sourceSession, int destinationSession)
	{
		this.parentWindow = parentWindow;
		this.sourceFile = sourceFile;
		this.destinationFile = destinationFile;
		this.sourceSession = sourceSession;
		this.destinationSession = destinationSession;
	}

	// Shows a dialogue to the user and lets him cancel the operation. The dialog information depends on
	// this class configuration: depends if the session is going to be inserted in a new session or an
	// existing one.
	// Returns 
	public Gtk.ResponseType showImportConfirmation()
	{
		string message;
		string sessionName = getSessionName (sourceFile, sourceSession);

		if (importsToNew ()) {
			// We don't need any confirmation to import into a new session (the user could delete it easily if it was a mistake)
			return Gtk.ResponseType.Ok;
		} else {
			// If the user is importing it into an existing session we require a confirmation.
			// This is very hard to Undo.
			string sessionInformation = String.Format (Catalog.GetString ("Session name: {0}\n" +
			                                                              "from file: {1}"), sessionName, sourceFile);
			message = String.Format (Catalog.GetString ("The current session will be modified. The data from:") + "\n\n" +
				sessionInformation + "\n\n" +
				Catalog.GetString ("Will be imported into the current session."));

			Gtk.MessageDialog confirmationDialog = new Gtk.MessageDialog (parentWindow, Gtk.DialogFlags.Modal, Gtk.MessageType.Question, Gtk.ButtonsType.OkCancel, message);
			confirmationDialog.Title = Catalog.GetString ("Import session?");
			Gtk.ResponseType response = (Gtk.ResponseType)confirmationDialog.Run ();

			confirmationDialog.Destroy ();

			return response;
		}
	}

	public void showImportCorrectlyFinished()
	{
		string message;

		if (importsToNew()) {
			message = Catalog.GetString ("Imported to a new session. You can load it now in Session - Load.");
		} else {
			message = Catalog.GetString ("Data merged into the open session.");
		}
		new DialogMessage (Catalog.GetString("Chronojump importer"), Constants.MessageTypes.INFO, message);
	}

	private bool importsToNew()
	{
		return destinationSession == 0;
	}

	// Tries to import the session and files defined in the constructor and returns Result. See
	// Result struct information to have a better idea.
	//
	// It checks the database versions and aborts (see Result information) if this Chronojump
	// tries to import a newer chronojump version.
	public Result import()
	{
		string temporarySourceFile = Path.GetTempFileName ();
		File.Copy (sourceFile, temporarySourceFile, true);

		Result sourceDatabaseVersion = getDatabaseVersionFromFile (temporarySourceFile);
		Result destinationDatabaseVersion = getDatabaseVersionFromFile (destinationFile);

		if (! sourceDatabaseVersion.success)
			return sourceDatabaseVersion;

		if (! destinationDatabaseVersion.success)
			return destinationDatabaseVersion;

		float destinationDatabaseVersionNum = float.Parse (destinationDatabaseVersion.output);
		float sourceDatabaseVersionNum = float.Parse (sourceDatabaseVersion.output);

		if (destinationDatabaseVersionNum < sourceDatabaseVersionNum) {
			return new Result (false, Catalog.GetString ("Trying to import a newer database version than this Chronojump\n" +
				"Please, update the running Chronojump."));
		} else if (destinationDatabaseVersionNum > sourceDatabaseVersionNum) {
			LogB.Debug ("chronojump-importer version before update: ", sourceDatabaseVersion.output);
			updateDatabase (temporarySourceFile);
			string versionAfterUpdate = getDatabaseVersionFromFile (temporarySourceFile).output;
			LogB.Debug ("chronojump-importer version after update: ", versionAfterUpdate);
		}

		List<string> parameters = new List<string> ();
		parameters.Add ("--source");
		parameters.Add (temporarySourceFile);
		
		// Parent directory of the original database
		// is where the importer can find the
		// encoder files
		parameters.Add ("--source_base_directory");
		parameters.Add (Path.Combine(Path.GetDirectoryName(sourceFile), "..")); 
		parameters.Add ("--destination");
		parameters.Add (destinationFile);
		parameters.Add ("--source_session");
		parameters.Add (Convert.ToString (sourceSession));

		if (destinationSession != 0) {
			parameters.Add ("--destination_session");
			parameters.Add (Convert.ToString (destinationSession));
		}

		Result result = executeChronojumpImporter (parameters);

		File.Delete (temporarySourceFile);

		return result;
	}

	private static void updateDatabase(string databaseFile)
	{
		StaticClassState classOriginalState = new StaticClassState (typeof (Sqlite));

		classOriginalState.readAttributes ();

		classOriginalState.writeAttributes (Sqlite.InitialState);

		Sqlite.CurrentVersion = "0";
		Sqlite.setSqlFilePath (databaseFile);
		Sqlite.Connect ();

		Sqlite.ConvertToLastChronojumpDBVersion ();

		classOriginalState.writeAttributes (classOriginalState);
		Sqlite.Connect ();
	}

	private static Result getImporterInformation(string filePath)
	{
		// If Result.success == true Result.output contains a valid JSON string.
		// It's a string and not a JsonValue for convenience with other methods (at the moment).
		// If result.success == false then result.error will contain the error that might help
		// the user to fix the problem or Chronojump support/developers to fix the problem.
		List<string> parameters = new List<string> ();

		parameters.Add ("--source");
		parameters.Add (filePath);
		parameters.Add ("--json_information");

		Result result = executeChronojumpImporter (parameters);

		if (result.success) {
			try {
				JsonValue.Parse (result.output);
			} catch (Exception e) {
				return new Result(false, "", String.Format("getDatabaseVersionFromFile: invalid JSON content:\n{0}\nException. {1}", result.output, e.Message));
			}

			return new Result (true, result.output);

		} else {
			return new Result(false, "", String.Format("getDatabaseVersionFromFile: no success fetching the database version of:\n{0}\nError: {1}", filePath, result.error));
		}
	}

	private static string getSessionName(string filePath, int sessionId)
	{
		Result information = getImporterInformation (filePath);
		if (information.success == false) {
			// This shouldn't happen, other getImporterInformation is used in different ways.
			LogB.Information ("chronojumpImporter::getSessionName failed. Output:" + information.output + "Error:" + information.error);
			return "UNKNOWN";
		} else {
			JsonValue json = JsonValue.Parse (information.output);

			if (!json.ContainsKey ("sessions")) {
				LogB.Information ("Trying to import a session but sessions doesn't exist. Output:" + information.output);
				return "UNKNOWN";
			}

			foreach(JsonValue session in json["sessions"])
			{
				if (session.ContainsKey ("uniqueID") && session ["uniqueID"] == sessionId) {
					return JsonUtils.valueOrDefault (session, "name", "UNKNOWN");
				}
			}
			LogB.Information ("Trying to import a session that we can't find the name. Output:" + information.output);
			return "UNKNOWN";
		}
	}

	private Result getDatabaseVersionFromFile(string filePath)
	{
		Result information = getImporterInformation (filePath);

		if (information.success) {
			JsonValue json = JsonValue.Parse (information.output);
			return new Result (true, JsonUtils.valueOrDefault(json, "databaseVersion", "0"));
		} else {
			return information;
		}
	}

	private static Result executeChronojumpImporter(List<string> parameters)
	{
		string importer_executable;

		if (UtilAll.IsWindows()) {
			// On Windows we execute the .exe file (it's the Python with py2exe)
			importer_executable = System.IO.Path.Combine (Util.GetPrefixDir (), "bin\\chronojump-importer\\chronojump_importer.exe");
		} else {
			// On Linux and OSX we execute Python and we pass the path to the script as a first argument

			importer_executable = "python";		// chronojump_importer.py works on Python 2 and Python 3

			string importer_script_path = System.IO.Path.Combine (Util.GetPrefixDir (), "bin/chronojump_importer.py");

			// first argument of the Python: the path to the script
			parameters.Insert (0, importer_script_path);
		}

		ExecuteProcess.Result execute_result = ExecuteProcess.run (importer_executable, parameters);

		if (execute_result.exitCode != 0) {
			// Python interpretar was executed but the Python file wasn't found or the script failed
			string errorMessage = "";

			if (execute_result.errorMessage == "") {
				// The Python script has been executed and failed (syntax error, crashed).
				// The error message will be in the output:
				errorMessage = execute_result.allOutput;
				errorMessage += "\nArguments: " + String.Join (" ", parameters);
			} else {
				// The Python script has not been executed, return the error message from ExecuteProcess
				errorMessage = execute_result.errorMessage;
			}
			return new Result (false, execute_result.allOutput, errorMessage);
		}

		// All good, returns the output
		return new Result (true, execute_result.allOutput);
	}
}
