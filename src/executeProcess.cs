using System.Collections.Generic;
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

/* This class executes a process and returns the result. */
class ExecuteProcess
{
	public struct Result
	{
		public const int ERROR_CANT_START = -1;

		// Encapsulates the result of a run: stdout, stderr, exitCode if process couldn't start is ERROR_CANT_START),
		// success (if exitCode != 0) and errorMessage to be shown to the user.
		public string stdout;
		public string stderr;
		public string allOutput;

		public int exitCode;
		public bool success;
		public string errorMessage;

		public Result(string stdout, string stderr, int exitCode, string errorMessage = "")
		{
			this.stdout = stdout;
			this.stderr = stderr;
			this.allOutput = stdout + stderr;
			this.exitCode = exitCode;
			this.success = (exitCode == 0);
			this.errorMessage = errorMessage;
		}
	};

	public static string runShowError(string file_name, List<string> parameters)
	{
		Result result = run(file_name, parameters);

		if (result.exitCode == Result.ERROR_CANT_START) {
			new DialogMessage (Constants.MessageTypes.WARNING, result.errorMessage);
			return "";
		} else {
			return result.allOutput;
		}
	}

	public static Result run(string file_name, List<string> parameters)
	{
		return runDo (file_name, parameters);
	}

	// Executes file_name without creating a Window and without using the shell
	// with the parameters. Waits that it finishes it. Returns the stdout and stderr.
	private static Result runDo(string file_name, List<string> parameters)
	{
		Process process = new Process();
		ProcessStartInfo processStartInfo = new ProcessStartInfo();

		processStartInfo.FileName = file_name;

		string parameters_string = "";
		foreach (string parameter in parameters)
		{
			parameters_string += CommandLineEncoder.EncodeArgText (parameter) + " ";
		}

		processStartInfo.Arguments = parameters_string;

		LogB.Debug ("ExecuteProcess FileName: " + processStartInfo.FileName);
		LogB.Debug ("ExecuteProcess Arguments: " + processStartInfo.Arguments);

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
			                                     processStartInfo.FileName, parameters_string, e.Message);
			LogB.Warning (errorMessage);
			return new Result ("", "", Result.ERROR_CANT_START, errorMessage);
		}

		string stdout = process.StandardOutput.ReadToEnd();
		string stderr = process.StandardError.ReadToEnd();

		process.WaitForExit ();

		int exitCode = process.ExitCode;

		return new Result (stdout, stderr, exitCode);
	}
}
