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
 *  Copyright (C) 2004-2015   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Diagnostics; 	//for detect OS and for Process
using System.IO; 		//for detect OS


public abstract class EncoderRProc 
{
	protected Process p;
	protected ProcessStartInfo pinfo;

	public enum Status { WAITING, RUNNING, DONE } 
	public Status status;

	protected string optionsFile;	
	protected EncoderStruct es;

	public bool StartOrContinue(EncoderStruct es)
	{
		status = Status.RUNNING;

		this.es = es;

		//options change at every capture. So do at continueProcess and startProcess
		writeOptionsFile();

		bool ok = true;
			
		if(isRunning()) {
			LogB.Debug("calling continue");
			ok = continueProcess();
		} else {
			LogB.Debug("calling start");
			ok = startProcess();
			LogB.Debug("StartedOk: " + ok.ToString());
		}
	
		status = Status.DONE;

		return ok;
	}

	protected bool isRunning() 
	{
		LogB.Debug("calling isRunning()");
		if(p == null) {
			LogB.Debug("p == null");
			return false;
		}

		/*	
		LogB.Debug("print processes");	
		Process [] pids = Process.GetProcesses();
		foreach (Process myPid in pids)
			LogB.Information(myPid.ToString());
		*/
		
		LogB.Debug(string.Format("last pid id {0}", p.Id));


		//if(isRunningThisProcess("Rscript") || isRunningThisProcess("*R*"))
		if(isRunningThisProcess(p.Id))
			return true;
	
		return false;
	}

	private bool isRunningThisProcess(int id)
	{
		try {
			Process pid = Process.GetProcessById(id);
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
		
	protected virtual void writeOptionsFile()
	{
	}

	protected virtual bool startProcess() {
		return true;
	}
	
	protected virtual bool continueProcess() 
	{
		LogB.Debug("sending continue process");
		//try/catch because sometimes the stdin write gots broken
		try {
			p.StandardInput.WriteLine("C");
		} catch {
			LogB.Debug("calling start because continue process was problematic");
			return startProcess();
		}

		return true;
	}
	
	/*
	protected void readingOutput (object sendingProcess, DataReceivedEventArgs outputFromR)
	{
		if (! String.IsNullOrEmpty(outputFromR.Data))
			LogB.Information(outputFromR.Data);
	}
	*/
	protected void readingError (object sendingProcess, DataReceivedEventArgs errorFromR)
	{
		if (! String.IsNullOrEmpty(errorFromR.Data))
			LogB.Warning(errorFromR.Data);
	}

	public void SendEndProcess() 
	{
		if(isRunning()) {
			LogB.Debug("Closing R script");
			try {
				p.StandardInput.WriteLine("Q");
			} catch {
				LogB.Warning("Seems stdin write gots broken");
			}
		} else
			LogB.Debug("R script is not working. Don't need to close.");
	}
}

public class EncoderRProcCapture : EncoderRProc 
{
	public EncoderRProcCapture() {
	}
	
	protected override bool startProcess() 
	{
		//If output file is not given, R will try to write in the running folder
		//in which we may haven't got permissions

		string pBin="";
		pinfo = new ProcessStartInfo();

		pBin="Rscript";
		if (UtilAll.IsWindows()) {
			//on Windows we need the \"str\" to call without problems in path with spaces
			pBin = "\"" + System.IO.Path.Combine(Util.GetPrefixDir(), "bin" + Path.DirectorySeparatorChar + "Rscript.exe") + "\"";
			LogB.Information("pBin:", pBin);
		}


		//on Windows we need the \"str\" to call without problems in path with spaces
		//pinfo.Arguments = "\"" + "passToR.R" + "\" " + optionsFile;
		pinfo.Arguments = "\"" + UtilEncoder.GetEncoderScriptCallCaptureNoRdotNet() + "\" " + optionsFile;

		LogB.Information("Arguments:", pinfo.Arguments);
		LogB.Information("--- 1 --- " + optionsFile.ToString() + " ---");
		LogB.Information("--- 2 --- " + pinfo.Arguments.ToString() + " ---");

		pinfo.FileName=pBin;

		pinfo.CreateNoWindow = true;
		pinfo.UseShellExecute = false;
		pinfo.RedirectStandardInput = true;
		pinfo.RedirectStandardError = true;
		//pinfo.RedirectStandardOutput = true; 


		try {
			p = new Process();
			p.StartInfo = pinfo;

			p.ErrorDataReceived += new DataReceivedEventHandler(readingError);

			p.Start();

			// Start asynchronous read of the output.
			// Caution: This has to be called after Start
			//p.BeginOutputReadLine();
			p.BeginErrorReadLine();

		
			LogB.Debug("D");
			
			LogB.Debug(string.Format("this pid id : {0}", p.Id));
		} catch {
			Console.WriteLine("catched at runEncoderCaptureNoRDotNetStart");
			return false;
		}
			
		return true;
	}
	
	//here curve is sent compressed (string. eg: "0*5 1 0 -1*3 2")
	public void SendCurve(double heightAtStart, string curveCompressed)
	{
		LogB.Debug("writing line 1 -->");
		
		string curveSend = "ps " + Util.ConvertToPoint(heightAtStart);
		LogB.Debug("curveSend [heightAtStart]",curveSend);
		p.StandardInput.WriteLine(curveSend);
								
		curveSend = curveCompressed;
		
		//TODO convert comma to point in this doubles

		LogB.Debug("curveSend [displacement array]",curveSend);
		p.StandardInput.WriteLine(curveSend); 	//this will send some lines because compressed data comes with '\n's
		p.StandardInput.WriteLine("E");		//this will mean the 'E'nd of the curve. Then data can be uncompressed on R
		
		LogB.Debug("<-- writen line 1");
	}	
	
	protected override void writeOptionsFile()
	{
		optionsFile = Path.GetTempPath() + "Roptions.txt";
	
		string scriptOptions = UtilEncoder.PrepareEncoderGraphOptions(
				"none", 	//title
				es, 
				false,	//neuromuscularProfile
				false	//translate (graphs)
				).ToString();

		TextWriter writer = File.CreateText(optionsFile);
		writer.Write(scriptOptions);
		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();
	}


}

public class EncoderRProcAnalyze : EncoderRProc 
{
	private string title;
	private bool neuromuscularProfileDo;
	private bool translate;

	public bool CancelRScript;

	public EncoderRProcAnalyze() {
	}

	public void SendData(string title, bool neuromuscularProfileDo, bool translate) {
		this.title = title;
		this.neuromuscularProfileDo = neuromuscularProfileDo;
		this.translate = translate;
		
		CancelRScript = false;
	}

	protected override bool startProcess() 
	{
		//If output file is not given, R will try to write in the running folder
		//in which we may haven't got permissions
	
		string pBin="";
		pinfo = new ProcessStartInfo();

		pBin="Rscript";
		if (UtilAll.IsWindows()) {
			//on Windows we need the \"str\" to call without problems in path with spaces
			pBin = "\"" + System.IO.Path.Combine(Util.GetPrefixDir(), "bin" + Path.DirectorySeparatorChar + "Rscript.exe") + "\"";
			LogB.Information("pBin:", pBin);
		}
		
		if (UtilAll.IsWindows()) {
			//On win32 R understands backlash as an escape character and 
			//a file path uses Unix-like path separator '/'		
			optionsFile = optionsFile.Replace("\\","/");
		}
		
		//on Windows we need the \"str\" to call without problems in path with spaces
		pinfo.Arguments = "\"" + getEncoderScriptCallGraph() + "\" " + optionsFile;
	
		LogB.Information("Arguments:", pinfo.Arguments);
		LogB.Information("--- 1 --- " + optionsFile.ToString() + " ---");
		//LogB.Information("--- 2 --- " + scriptOptions + " ---");
		LogB.Information("--- 3 --- " + pinfo.Arguments.ToString() + " ---");
		
		string outputFileCheck = "";
		string outputFileCheck2 = "";
		
		//Wait until this to update encoder gui (if don't wait then treeview will be outdated)
		//exportCSV is the only one that doesn't have graph. all the rest Analysis have graph and data
		if(es.Ep.Analysis == "exportCSV")
			outputFileCheck = es.OutputData1; 
		else {
			//outputFileCheck = es.OutputGraph;
			//
			//OutputData1 because since Chronojump 1.3.6, 
			//encoder analyze has a treeview that can show the curves
			//when a graph analysis is done, curves file has to be written
			outputFileCheck = es.OutputData1;
		        //check also the otuput graph
			outputFileCheck2 = es.OutputGraph; 
		}
			
		LogB.Information("outputFileChecks");
		LogB.Information(outputFileCheck);
		LogB.Information(outputFileCheck2);

		pinfo.FileName=pBin;

		pinfo.CreateNoWindow = true;
		pinfo.UseShellExecute = false;
		pinfo.RedirectStandardInput = true;
		pinfo.RedirectStandardError = true;
		
		/*
		 * if redirect this there are problems because the buffers get saturated
		 * pinfo.RedirectStandardOutput = true; 
		 * if is not redirected, then prints are shown by console (but not in logB
		 * best solution is make the prints as write("message", stderr())
		 * and then will be shown in logB by readError
		 */


		//delete output file check(s)
		deleteFile(outputFileCheck);
		if(outputFileCheck2 != "")
			deleteFile(outputFileCheck2);

			
		//delete 1RM data if exists
		if (File.Exists(es.SpecialData))
			File.Delete(es.SpecialData);

		try {	
			p = new Process();
			p.StartInfo = pinfo;
			
			//do not redirect ouptut. Read above
			//p.OutputDataReceived += new DataReceivedEventHandler(readingOutput);
			p.ErrorDataReceived += new DataReceivedEventHandler(readingError);
			
			p.Start();

			//don't do this ReadToEnd because then this method never ends
			//LogB.Information(p.StandardOutput.ReadToEnd()); 
			//LogB.Warning(p.StandardError.ReadToEnd());
			
			// Start asynchronous read of the output.
			// Caution: This has to be called after Start
			//p.BeginOutputReadLine();
			p.BeginErrorReadLine();


			if(outputFileCheck2 == "")
				while ( ! ( fileWritten(outputFileCheck) || CancelRScript) );
			else
				while ( ! ( (fileWritten(outputFileCheck) && fileWritten(outputFileCheck2)) || CancelRScript ) );
		} catch {
			LogB.Warning("catched at startProcess");
			return false;
		}

		return true;

	}
	
	protected override bool continueProcess() 
	{
		//TODO: outputFileCheck creation/deletion here and at startProcess, should be unique
		string outputFileCheck = "";
		string outputFileCheck2 = "";
		
		if(es.Ep.Analysis == "exportCSV")
			outputFileCheck = es.OutputData1; 
		else {
			//outputFileCheck = es.OutputGraph;
			//
			//OutputData1 because since Chronojump 1.3.6, 
			//encoder analyze has a treeview that can show the curves
			//when a graph analysis is done, curves file has to be written
			outputFileCheck = es.OutputData1;
		        //check also the otuput graph
			outputFileCheck2 = es.OutputGraph; 
		}

		//delete output file check(s)
		deleteFile(outputFileCheck);
		if(outputFileCheck2 != "")
			deleteFile(outputFileCheck2);

		LogB.Debug("sending continue process");
		//try/catch because sometimes the stdin write gots broken
		try {
			p.StandardInput.WriteLine("C");
		} catch {
			LogB.Debug("calling start because continue process was problematic");
			return startProcess();
		}

		LogB.Debug("waiting files");
		if(outputFileCheck2 == "")
			while ( ! ( fileWritten(outputFileCheck) || CancelRScript) );
		else
			while ( ! ( (fileWritten(outputFileCheck) && fileWritten(outputFileCheck2)) || CancelRScript ) );
		LogB.Debug("files written");
		
		return true;
	}
	
	
	private string getEncoderScriptCallGraph() {
		return System.IO.Path.Combine(
				Util.GetDataDir(), "encoder", Constants.EncoderScriptCallGraph);
	}
	
	protected override void writeOptionsFile()
	{
		optionsFile = Path.GetTempPath() + "Roptions.txt";
	
		string scriptOptions = UtilEncoder.PrepareEncoderGraphOptions(
				title,
				es, 
				neuromuscularProfileDo,
				translate
				).ToString();

		TextWriter writer = File.CreateText(optionsFile);
		writer.Write(scriptOptions);
		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();
	}
		
	private void deleteFile(string filename)
	{
		LogB.Information("Deleting... " + filename);
		if (File.Exists(filename))
			File.Delete(filename);
	}

	private bool fileWritten(string filename)
	{
		if(File.Exists(filename)) {
			FileInfo fi = new FileInfo(filename);
			if(fi.Length > 0)
				return true;
		}

		return false;
	}

}
