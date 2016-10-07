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
	private string session;

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
	public ChronojumpImporter(string sourceFile, string destinationFile, string session)
	{
		this.sourceFile = sourceFile;
		this.destinationFile = destinationFile;
		this.session = session;
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
		parameters.Add (session);

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

	private Result getDatabaseVersionFromFile(string filePath)
	{
		List<string> parameters = new List<string> ();

		parameters.Add ("--source");
		parameters.Add (filePath);
		parameters.Add ("--json_information");

		Result result = executeChronojumpImporter (parameters);

		if (result.success) {
			JsonValue json = "";
			try {
				json = JsonValue.Parse (result.output);
			} catch (Exception e) {
				return new Result(false, "", String.Format(Catalog.GetString("getDatabaseVersionFromFile: invalid JSON content:\n{0}\nException. {1}"), result.output, e.Message));
			}

			string databaseVersion = json ["databaseVersion"];
			
			return new Result (true, databaseVersion);

		} else {
			return new Result(false, "", String.Format(Catalog.GetString("getDatabaseVersionFromFile: no success fetching the database version of:\n{0}\nError: {1}"), filePath, result.error));
		}
	}

	private Result executeChronojumpImporter(List<string> parameters)
	{
		string importer_executable;
		string importer_script_path = "";

		if (UtilAll.IsWindows()) {
			importer_executable = System.IO.Path.Combine (Util.GetPrefixDir (), "bin\\chronojump-importer\\chronojump_importer.exe");
		} else {
			importer_executable = "python";		// chronojump_importer works on Python 2 and Python 3
			importer_script_path = System.IO.Path.Combine (Util.GetPrefixDir (), "bin/chronojump_importer.py");
		}

		parameters.Insert (0, importer_script_path);
		ExecuteProcess.Result execute_result = ExecuteProcess.run (importer_executable, parameters);

		if (execute_result.exitCode != 0) {
			// Python interpretar was executed but the Python file wasn't found or the script failed
			string errorMessage = "";

			if (execute_result.errorMessage == "") {
				// The Python script has been executed and failed (syntax error, crashed).
				// The error message will be in the output:
				errorMessage = execute_result.allOutput;
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
