using System.Collections; //ArrayList
using System.Collections.Generic; //List
using System.Json;
using System.Diagnostics;
using System;
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
	private string sourceFile;
	private string destinationFile;
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
		Result sourceDatabaseVersion = getSourceDatabaseVersion ();
		Result destinationDatabaseVersion = getDestinationDatabaseVersion ();

		if (! sourceDatabaseVersion.success)
			return sourceDatabaseVersion;

		if (! destinationDatabaseVersion.success)
			return destinationDatabaseVersion;

		if (float.Parse(destinationDatabaseVersion.output) < float.Parse(sourceDatabaseVersion.output)) {
			return new Result (false, Catalog.GetString ("Trying to import a newer database version than this Chronojump\n" +
				"Please, update the running Chronojump."));
		}

		List<string> parameters = new List<string> ();
		parameters.Add ("--source");
		parameters.Add (CommandLineEncoder.EncodeArgText (sourceFile));
		parameters.Add ("--destination");
		parameters.Add (CommandLineEncoder.EncodeArgText (destinationFile));
		parameters.Add ("--source_session");
		parameters.Add (CommandLineEncoder.EncodeArgText (session));

		Result result = executeChronojumpImporter (parameters);

		return result;
	}

	private Result getSourceDatabaseVersion()
	{
		return getDatabaseVersionFromFile (sourceFile);
	}

	private Result getDestinationDatabaseVersion()
	{
		return getDatabaseVersionFromFile (destinationFile);
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
			importer_script_path = CommandLineEncoder.EncodeArgText (System.IO.Path.Combine (Util.GetPrefixDir (), "bin/chronojump_importer.py"));
		}

		Process process = new Process();
		ProcessStartInfo processStartInfo;

		processStartInfo = new ProcessStartInfo();

		processStartInfo.Arguments = importer_script_path + " " + string.Join (" ", parameters);
		processStartInfo.FileName = importer_executable;

		LogB.Debug ("chronojump-importer fileName: " + processStartInfo.FileName);
		LogB.Debug ("chronojump-importer Arguments: " + processStartInfo.Arguments);

		processStartInfo.CreateNoWindow = true;
		processStartInfo.UseShellExecute = false;
		processStartInfo.RedirectStandardInput = false;
		processStartInfo.RedirectStandardError = true;
		processStartInfo.RedirectStandardOutput = true;

		process.StartInfo = processStartInfo;

		try {
			process.Start();
		}
		catch(Exception e) {
			string errorMessage = String.Format (Catalog.GetString("Cannot start:\n" +
			                                                       "{0}\n" +
			                                                       "with the parameters:" +
			                                                       "{1}\n" +
			                                                       "Exception:\n" +
			                                                       "{2}"),
			                                     processStartInfo.FileName, processStartInfo.Arguments, e.Message);
			return new Result (false, "", errorMessage);
		}

		string allOutput = "";
		allOutput += process.StandardOutput.ReadToEnd();
		allOutput += process.StandardError.ReadToEnd();

		process.WaitForExit ();

		if (process.ExitCode != 0) {
			string errorMessage = String.Format (Catalog.GetString("Error executing: {0}\n" +
			                                                       "with the parameters: {1}\n" +
			                                                       "output: {2}"),
			                                     processStartInfo.FileName, processStartInfo.Arguments, allOutput);

			// Python interpretar was executed but the Python file wasn't found or the script failed
			return new Result (false, allOutput, errorMessage);
		}

		return new Result (true, allOutput);
	}
}