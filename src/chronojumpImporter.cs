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
 * Copyright (C) 2016-2017   Carles Pina i Estany <carles@pina.cat>
 * Copyright (C) 2019-2020   Xavier de Blas <xaviblas@gmail.com>
 */

/*
 * note there is a diagram on diagrams/processes/import
 */

class ChronojumpImporter
{
	public string MessageToPulsebar;

	// Database that it's importing from
	private string sourceFile;

	// Database that it's importing to (usually chronojump.db database)
	private string destinationFile;

	// Session that will import
	private int sourceSession;

	// Session that we will import into. If it's 0 it means into to create
	// a new session, otherwise it will import it into the session indicated by it
	private int destinationSession;

	// to debug to a file if debug mode is started on preferences;
	private bool debugToFile;

	Preferences.pythonVersionEnum pythonVersion;

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

		public void AddErrorComment (string s)
		{
			this.error += "\n\n" + s;
		}
	}

	// ChronojumpImporter class imports a specific session from sourceFile to destinationFile.
	// The main method is "import()" which does all the work.
	public ChronojumpImporter(string sourceFile, string destinationFile,
			int sourceSession, int destinationSession, bool debugToFile, Preferences.pythonVersionEnum pythonVersion)
	{
		this.sourceFile = sourceFile;
		this.destinationFile = destinationFile;
		this.sourceSession = sourceSession;
		this.destinationSession = destinationSession;
		this.debugToFile = debugToFile;

		this.pythonVersion = pythonVersion;

		MessageToPulsebar = "";
	}

	/*
	 * interesting method for showing a dialog on the future
	 * now we do not used because there is a notebook on session_load
	 *
	// Shows a dialogue to the user and lets him cancel the operation. The dialog information depends on
	// this class configuration: depends if the session is going to be inserted in a new session or an
	// existing one.
	// Returns 
	public Gtk.ResponseType showImportConfirmation()
	{
		string message;
		string sessionName = GetSessionName (sourceFile, sourceSession);

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
	*/

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
		//1) create temp dir for forceSensor and runEncoder and copy files there, original files will not be used
		//   no need to be done for encoder files because there we will not to change filename
		MessageToPulsebar = "Copying temporary files";

LogB.Information("import A ");
		string tempImportDir = Util.GetDatabaseTempImportDir();
		if(Directory.Exists(tempImportDir))
		{
			try {
				var dir = new DirectoryInfo(@tempImportDir);
				dir.Delete(true); //recursive delete
			} catch {
				return new Result (false, "", "Could not delete directory: " + tempImportDir);
			}
		}
LogB.Information("import B ");
		string forceSensorName = "forceSensor";
		string raceAnalyzerName = "raceAnalyzer";
LogB.Information("import C ");
		Directory.CreateDirectory(tempImportDir);
LogB.Information("import D ");
		Directory.CreateDirectory(Path.Combine(tempImportDir, forceSensorName, sourceSession.ToString()));
LogB.Information("import E ");
		Directory.CreateDirectory(Path.Combine(tempImportDir, raceAnalyzerName, sourceSession.ToString()));

LogB.Information("import F ");
		string sourceDir = Path.GetDirectoryName(sourceFile);
		try {
			if(Directory.Exists(Path.Combine(sourceDir, "..", forceSensorName, sourceSession.ToString())))
				foreach (FileInfo file in new DirectoryInfo(Path.Combine(sourceDir, "..", forceSensorName, sourceSession.ToString())).GetFiles())
					file.CopyTo(Path.Combine(tempImportDir, forceSensorName, sourceSession.ToString(), file.Name));
		} catch {
			LogB.Information ("Catched on copying files, disk full or files not fully downloaded (One drive or other cloud problems)");
			return new Result (false, "", string.Format(Catalog.GetString ("Cannot copy files from {0} to {1}."),
				Path.Combine(sourceDir, "..", forceSensorName, sourceSession.ToString()),
				Path.Combine(tempImportDir, forceSensorName, sourceSession.ToString()) + "\n" +
				Catalog.GetString ("The disk may be full or the files may be copying from a cloud service but have not been downloaded.")
				));
		}

LogB.Information("import G ");
		try {
			if(Directory.Exists(Path.Combine(sourceDir, "..", raceAnalyzerName, sourceSession.ToString())))
				foreach (FileInfo file in new DirectoryInfo(Path.Combine(sourceDir, "..", raceAnalyzerName, sourceSession.ToString())).GetFiles())
					file.CopyTo(Path.Combine(tempImportDir, raceAnalyzerName, sourceSession.ToString(), file.Name));
		} catch {
			LogB.Information ("Catched on copying files, disk full or files not fully downloaded (One drive or other cloud problems)");
			return new Result (false, "", string.Format(Catalog.GetString ("Cannot copy files from {0} to {1}"),
				Path.Combine(sourceDir, "..", raceAnalyzerName, sourceSession.ToString()),
				Path.Combine(tempImportDir, raceAnalyzerName, sourceSession.ToString()) + "\n" +
				Catalog.GetString ("The disk may be full or the files may be copying from a cloud service but have not been downloaded.")
				));
		}

LogB.Information("import H ");

		//2) prepare SQL files

		MessageToPulsebar = "Preparing database";
		string temporarySourceFile = Path.GetTempFileName ();
		LogB.Information("temporarySourceFile without GetFullPath: " + temporarySourceFile);
		temporarySourceFile = Path.GetFullPath(temporarySourceFile);
		LogB.Information("temporarySourceFile after GetFullPath: " + temporarySourceFile);

		File.Copy (sourceFile, temporarySourceFile, true);

		Result sourceDatabaseVersion = getDatabaseVersionFromFile (temporarySourceFile);
		Result destinationDatabaseVersion = getDatabaseVersionFromFile (destinationFile);

		/* if it fails here and ends on next return
		   probably is because on windows it lacks the msvcr100.dll
		   It is documented on howto_new_version.txt
		   */
LogB.Information("import I ");
		if (! sourceDatabaseVersion.success)
		{
			if (UtilAll.IsWindows())
				sourceDatabaseVersion.AddErrorComment ("This error can be caused by a faulty Windows installation of msvcr100.dll."
						+ "\n" + "Contact Chronojump to solve it."); //TODO: translate this

			return sourceDatabaseVersion;
		}

		if (! destinationDatabaseVersion.success)
		{
			if (UtilAll.IsWindows())
				sourceDatabaseVersion.AddErrorComment ("This error can be caused by a faulty Windows installation of msvcr100.dll."
						+ "\n" + "Contact Chronojump to solve it."); //TODO: translate this

			return destinationDatabaseVersion;
		}

		float destinationDatabaseVersionNum = float.Parse (destinationDatabaseVersion.output);
		float sourceDatabaseVersionNum = float.Parse (sourceDatabaseVersion.output);

		//3 check version of database to be imported

		MessageToPulsebar = "Checking version";
		if (destinationDatabaseVersionNum < sourceDatabaseVersionNum)
		{
			MessageToPulsebar = Catalog.GetString("Please update Chronojump");
			return new Result (false, "", Catalog.GetString ("Trying to import a newer database version than this Chronojump\n" +
				"Please, update the running Chronojump."));
		} else if (destinationDatabaseVersionNum > sourceDatabaseVersionNum)
		{
			LogB.Debug ("chronojump-importer version before update: ", sourceDatabaseVersion.output);
			MessageToPulsebar = "Updating database";
			updateDatabase (temporarySourceFile);
			string versionAfterUpdate = getDatabaseVersionFromFile (temporarySourceFile).output;
			LogB.Debug ("chronojump-importer version after update: ", versionAfterUpdate);
		}

LogB.Information("import J ");
		MessageToPulsebar = "Starting import";
		List<string> parameters = new List<string> ();
		parameters.Add ("--source");
		parameters.Add (temporarySourceFile);
		
		// Parent directory of the original database
		// is where the importer can find the
		// encoder files
		parameters.Add ("--source_base_directory");
		parameters.Add (Path.Combine(Path.GetDirectoryName(sourceFile), "..")); 
		parameters.Add ("--source_temp_directory");
		parameters.Add (Util.GetDatabaseTempImportDir());
		parameters.Add ("--destination");
		parameters.Add (destinationFile);
		parameters.Add ("--source_session");
		parameters.Add (Convert.ToString (sourceSession));

		if (destinationSession != 0) {
			parameters.Add ("--destination_session");
			parameters.Add (Convert.ToString (destinationSession));
		}
		parameters.Add ("--debug_to_file");
		if(debugToFile)
			parameters.Add (Path.Combine(Path.GetTempPath(), "chronojumpImportDebug.txt"));
		else
			parameters.Add ("NONE");

LogB.Information("import K ");
		Result result = executeChronojumpImporter (parameters, pythonVersion);

		MessageToPulsebar = "Done!";
		File.Delete (temporarySourceFile);
LogB.Information("import L ");

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

		Sqlite.UpdatingDBFrom = Sqlite.UpdatingDBFromEnum.IMPORTED_SESSION;

		Sqlite.ConvertToLastChronojumpDBVersion ();

		classOriginalState.writeAttributes (classOriginalState);
		Sqlite.Connect ();
	}

	private static Result getImporterInformation(string filePath, Preferences.pythonVersionEnum pythonVersion)
	{
		// If Result.success == true Result.output contains a valid JSON string.
		// It's a string and not a JsonValue for convenience with other methods (at the moment).
		// If result.success == false then result.error will contain the error that might help
		// the user to fix the problem or Chronojump support/developers to fix the problem.
		List<string> parameters = new List<string> ();

		parameters.Add ("--source");
		parameters.Add (filePath);
		parameters.Add ("--json_information");

		Result result = executeChronojumpImporter (parameters, pythonVersion);

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

	public static string GetSessionName(string filePath, int sessionId, Preferences.pythonVersionEnum pythonVersion)
	{
		Result information = getImporterInformation (filePath, pythonVersion);
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
					return JsonUtils.ValueOrDefault (session, "name", "UNKNOWN");
				}
			}
			LogB.Information ("Trying to import a session that we can't find the name. Output:" + information.output);
			return "UNKNOWN";
		}
	}

	private Result getDatabaseVersionFromFile(string filePath)
	{
		Result information = getImporterInformation (filePath, pythonVersion);

		if (information.success) {
			JsonValue json = JsonValue.Parse (information.output);
			return new Result (true, JsonUtils.ValueOrDefault(json, "databaseVersion", "0"));
		} else {
			return information;
		}
	}

	private static Result executeChronojumpImporter(List<string> parameters, Preferences.pythonVersionEnum pythonVersion)
	{
		string importer_executable;

		if (UtilAll.IsWindows()) {
			// On Windows we execute the .exe file (it's the Python with py2exe)
			// right now default to python2
			if(pythonVersion == Preferences.pythonVersionEnum.Python2)
				importer_executable = System.IO.Path.Combine (Util.GetPrefixDir (), "bin\\chronojump-importer\\chronojump_importer.exe");
			else
				importer_executable = System.IO.Path.Combine (Util.GetPrefixDir (), "bin\\chronojump-importer-python3\\chronojump_importer.exe");
		} else {
			// On Linux and OSX we execute Python and we pass the path to the script as a first argument

			importer_executable = Preferences.GetPythonExecutable(pythonVersion);

			LogB.Information("importer_executable: " + importer_executable);
			string importer_script_path = System.IO.Path.Combine (Util.GetPrefixDir (), "bin/chronojump_importer.py");

			// first argument of the Python: the path to the script
			parameters.Insert (0, importer_script_path);
		}

		ExecuteProcess.Result execute_result = ExecuteProcess.run (importer_executable, parameters, true, true);

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

	public string SourceFile
	{
		get { return sourceFile; }
	}

	public int SourceSession
	{
		get { return sourceSession; }
	}

}
