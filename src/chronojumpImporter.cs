using System.Collections; //ArrayList
using System.Collections.Generic; //List
using System.Json;
using System.Diagnostics;
using System;
using Mono.Unix;

class ChronojumpImporter
{
	private string sourceFile;
	private string destinationFile;
	private string session;

	public ChronojumpImporter(string sourceFile, string destinationFile, string session)
	{
		this.sourceFile = sourceFile;
		this.destinationFile = destinationFile;
		this.session = session;
	}

	public bool import()
	{
		string sourceDatabaseVersion = getSourceDatabaseVersion ();
		string destinationDatabaseVersion = getDestinationDatabaseVersion ();

		if (float.Parse(destinationDatabaseVersion) < float.Parse(sourceDatabaseVersion)) {
			new DialogMessage (Constants.MessageTypes.WARNING, Catalog.GetString ("Trying to import a newer database version than this Chronojump\nPlease, update the running Chronojump."));
			return false;
		}

		List<string> parameters = new List<string> ();
		parameters.Add ("--source");
		parameters.Add (CommandLineEncoder.EncodeArgText (sourceFile));
		parameters.Add ("--destination");
		parameters.Add (CommandLineEncoder.EncodeArgText (destinationFile));
		parameters.Add ("--source_session");
		parameters.Add (CommandLineEncoder.EncodeArgText (session));

		executeChronojumpImporter (parameters);

		return true;
	}

	public string getSourceDatabaseVersion()
	{
		return getDatabaseVersionFromFile (sourceFile);
	}

	public string getDestinationDatabaseVersion()
	{
		return getDatabaseVersionFromFile (destinationFile);
	}

	private string getDatabaseVersionFromFile(string filePath)
	{
		List<string> parameters = new List<string> ();

		parameters.Add ("--source");
		parameters.Add (filePath);
		parameters.Add ("--json_information");

		string jsonText = executeChronojumpImporter (parameters);

		JsonValue result = JsonValue.Parse(jsonText);

		string databaseVersion = result ["databaseVersion"];

		return databaseVersion;
	}

	private string executeChronojumpImporter(List<string> parameters)
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

		// processStartInfo.Arguments = importer_script_path + " --source \"" + CommandLineEncoder.EncodeArgText (sourceFile) + "\" --destination \"" + CommandLineEncoder.EncodeArgText (destinationFile) + "\" --source_session \"" + CommandLineEncoder.EncodeArgText (session) + "\"";
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

		bool started = false;
		try {
			process.Start();
			started = true;
		}
		catch(Exception e) {
			string errorMessage;
			errorMessage = String.Format ("Cannot execute:\n   {0}\nwith the parameters:\n   {1}\n\nThe exception is: {2}", processStartInfo.FileName, processStartInfo.Arguments, e.Message);
			ErrorWindow.Show (errorMessage);
			return "";
		}

/*		if (started) {
			process.BeginOutputReadLine ();
			process.BeginErrorReadLine ();
		}
*/
		string allOutput = "";
		allOutput += process.StandardOutput.ReadToEnd();
		allOutput += process.StandardError.ReadToEnd();
		Console.WriteLine(allOutput);

		process.WaitForExit ();

		return allOutput;
	}
}