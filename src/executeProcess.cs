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
 * Copyright (C) 2017   Xavier de Blas <xaviblas@gmail.com>
 */

using System.Collections.Generic;
using System.Diagnostics;
using System;
using Mono.Unix;
using System.IO;                //FILE


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

	public static Result runShowErrorIfNotStarted(string file_name, List<string> parameters)
	{
		Result result = run(file_name, parameters);

		if (result.exitCode == Result.ERROR_CANT_START) {
			new DialogMessage (Constants.MessageTypes.WARNING, result.errorMessage);
		}

		return result;
	}

	public static Result run(string file_name)
	{
		return runDo (file_name, new List<string>());
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

		string stdout = process.StandardOutput.ReadToEnd().TrimEnd ('\n');
		string stderr = process.StandardError.ReadToEnd ().TrimEnd ('\n');

		process.WaitForExit ();

		if (stderr != "") {
			LogB.Warning(String.Format("Executed: {0} Parameters: {1} Stdout: {2} Stderr: {3}", processStartInfo.FileName, parameters_string, stdout, stderr));
		}

		int exitCode = process.ExitCode;

		return new Result (stdout, stderr, exitCode);
	}

	/*
	 * run a process on the background, eg: read rfid.
	 * don't call WaitForExit(), kill it on Chronojump exit
	 * returns false if there are problems calling it
	 */
	public static bool RunAtBackground(ref Process process, string file_name, List<string> parameters, bool redirectInput)
	{
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
		processStartInfo.RedirectStandardInput = redirectInput; //note UseShellExecute has to be false to be able to redirect
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
			return false;
		}

		return true;
	}

	/*
	 * xungo pq sembla que crea un nou process i l'associa amb el que estem avaluant
	 *
	public static bool IsRunning4 (int processID)
	{
		try {
			Process p = Process.GetProcessById(processID);
			if(p == null)
			{
				LogB.Information("process does not exist (null)");
				return false;
			}
		} catch {
			LogB.Information("process does not exist (catch)");
			return false;
		}

		LogB.Information("process is running!");
		return true;
	}
	*/
	/*
	 * This is the best method because it does not need the process,
	 * note that process maybe is null
	 * and some code in IsRunning2old will fail
	 */
	public static bool IsRunning3 (int processID, string executable)
	{
		//Debug
		Process[] allThisMachine = Process.GetProcesses();
		LogB.Information("All processes in this machine:");
		foreach(Process p in allThisMachine)
			LogB.Information(p.ToString());

		//Debug
		LogB.Information("LastPartOfPath of executable: " + Util.GetLastPartOfPath(executable));

		Process[] pNameArray = Process.GetProcessesByName(Util.GetLastPartOfPath(executable));
		if (pNameArray.Length == 0)
			return false;

		foreach(Process p in pNameArray)
			if(p.Id == processID)
				return true;

		return false;
	}
	/*
	//better method than below
	//https://stackoverflow.com/a/262291
	public static bool IsRunning2old (Process process, string executable)
	{
		Process[] pNameArray = Process.GetProcessesByName(Util.GetLastPartOfPath(process.ProcessName));
		if (pNameArray.Length == 0)
			return false;

		foreach(Process p in pNameArray)
			if(p.Id == process.Id)
				return true;

		return false;
	}
	*/

	//This seems is not working with ffmpeg capture, try method obove
	public static bool IsRunning(Process process)
	{
		LogB.Debug("calling Process.IsRunning()");
		if(process == null) {
			LogB.Debug("process == null");
			return false;
		} else {
			if(isRunningThisProcess(process))
				return true;
		}

		return false;
	}

	private static bool isRunningThisProcess(Process p)
	{
		/*
		 * Process Id is not valid if the associated process is not running.
		 * Need to ensure that the process is running before attempting to retrieve the Id property.
		 */

		try {
			LogB.Debug(string.Format("last pid id {0}", p.Id));
			Process pid = Process.GetProcessById(p.Id);
			if(pid == null)
				return false;
		} catch {
			return false;
		}

		return true;
	}

	/*
	 * don't use this because in linux R script can be called by:
	 * "/usr/lib/R/bin/exec/R"
	 * and it will not be found passing "R" or "*R"
	private bool isRunningThisProcess(string name)
	{
		Process [] pids = Process.GetProcessesByName(name);
		foreach (Process myPid in pids) {
			LogB.Debug(string.Format("pids id: {0}", myPid.Id));
			if (myPid.Id == Convert.ToInt32(p.Id))
				return true;
		}

		return false;
	}
	*/

	/*
	 * The process.Responding only works on GUI processes
	 * So, here we send a "ping" expecting to see the result in short time
	 *
	 * TODO: maybe is good to kill the unresponsive processes
	 */
	public static bool IsResponsive(Process p)
	{
		Random rnd = new Random();
		int randomInt = rnd.Next(); //eg. 1234

		string randomPingStr = "PING" + Path.Combine(Path.GetTempPath(), "chronojump" + randomInt.ToString() + ".txt"); 
		//eg Linux: 'PING/tmp/chronojump1234.txt'
		//eg Windows: 'PINGC:\Temp...\chronojump1234.txt'

		if (UtilAll.IsWindows()) {
			//On win32 R understands backlash as an escape character and
			//a file path uses Unix-like path separator '/'
			randomPingStr = randomPingStr.Replace("\\","/");
		}
		//eg Windows: 'PINGC:/Temp.../chronojump1234.txt'

		LogB.Information("Sending ping: " + randomPingStr);
		try {
			p.StandardInput.WriteLine(randomPingStr);
		} catch {
			LogB.Warning("Catched waiting response");
			return false;
		}

		//wait 250ms the response
		System.Threading.Thread.Sleep(250);

		//On Linux will be '/' on Windows '\'
		if(File.Exists(Path.Combine(Path.GetTempPath(), "chronojump" + randomInt.ToString() + ".txt"))) {
			LogB.Information("Process is responding");
			return true;
		}

		LogB.Warning("Process is NOT responding");
		return false;
	}

	public static bool CallR(string script)
	{
		string executable = UtilEncoder.RProcessBinURL();
		List<string> parameters = new List<string>();

		//A) fix script name
		if(UtilAll.IsWindows())
			script = script.Replace("\\","/");

		parameters.Insert(0, "\"" + script + "\"");

		//B) tempPath
		string tempPath = Path.GetTempPath();
		if(UtilAll.IsWindows())
			tempPath = tempPath.Replace("\\","/");

		parameters.Insert(1, "\"" + tempPath + "\"");

		LogB.Information("\nCalling R file ----->");

		//C) call process
		//ExecuteProcess.run (executable, parameters);
		Result execute_result = run (executable, parameters);
		//LogB.Information("Result = " + execute_result.stdout);

		LogB.Information("\n<------ Done calling R file.");
		return execute_result.success;
	}

}
