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
	protected static Process p;
	protected static ProcessStartInfo pinfo;
	protected static bool running;

	protected string optionsFile;	
	protected EncoderStruct es;

	public void StartOrContinue(EncoderStruct es)
	{
		this.es = es;

		//options change at every capture. So do at continueProcess and startProcess
		writeOptionsFile();
			
		if(isRunning()) {
			LogB.Debug("calling continue");
			continueProcess();
		} else {
			LogB.Debug("calling start");
			bool startedOk = startProcess();
			LogB.Debug("StartedOk: " + startedOk.ToString());
		}
	}

	private bool isRunning() 
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
	protected virtual void continueProcess() {
	}
}

public class EncoderRProcCapture : EncoderRProc 
{
	public EncoderRProcCapture() {
	}
	
	protected override bool startProcess() 
	{
		LogB.Debug("A");
		//ProcessStartInfo pinfo;
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
		LogB.Debug("B");


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



		LogB.Debug("C");
		try {
			p= new Process();
			p.StartInfo = pinfo;

			p.ErrorDataReceived += new DataReceivedEventHandler(readingCurveFromRerror);

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
	private void readingCurveFromRerror (object sendingProcess, DataReceivedEventArgs curveFromR)
	{
		if (! String.IsNullOrEmpty(curveFromR.Data))
		{
			//use Warning because it's used also to print flow messages
			LogB.Warning(curveFromR.Data);
		}
	}
	
	protected override void continueProcess() 
	{
		LogB.Debug("sending continue process");
		p.StandardInput.WriteLine("C");
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
	
	public void SendCaptureEnd() 
	{
		/*
		 * don't send end line
		 * process should remain opened
		 *
		LogB.Debug("sending end line");
		p.StandardInput.WriteLine("Q");
		*/
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
	public EncoderRProcAnalyze() {
	}

	protected override bool startProcess() 
	{
		/*
		 * WIP
		 *
		try {   
			p = new Process();
			p.StartInfo = pinfo;
			p.Start();

			LogB.Information(p.StandardOutput.ReadToEnd());
			LogB.Warning(p.StandardError.ReadToEnd());

			//p.WaitForExit(); //do NOT wait for exit
		} catch {
			return false;
		}
			
		running = true;
		*/
		return true;
	}
	
	protected override void continueProcess() 
	{
		/*
		 * create a file to be file to be readed by the Rscript
		 * or send there STDIN, like:
		 * p.StandardInput.WriteLine();
		 */
	}

}
