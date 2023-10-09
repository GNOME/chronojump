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
 *  Copyright (C) 2004-2023   Xavier de Blas <xaviblas@gmail.com>
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
	public bool Debug = false;
	public bool CrossValidate;
	public bool SeparateSessionInDays;
	public int CurvesReaded;

	protected string optionsFile;	
	protected EncoderStruct es;


	public bool StartOrContinue(EncoderStruct es)
	{
		status = Status.RUNNING;

		this.es = es;

		//options change at every capture. So do at continueProcess and startProcess
		writeOptionsFile();

		bool ok = true;
			
		if(ExecuteProcess.IsRunning(p) && ExecuteProcess.IsResponsive(p)) {
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


	protected virtual void writeOptionsFile()
	{
	}

	protected virtual string printTriggers(TriggerList.Type3 type3)
	{
		return TriggerList.TriggersNotFoundString;
	}

	protected virtual bool startProcess() {
		return true;
	}

	protected virtual bool continueProcess() 
	{
		/*
		LogB.Debug("sending continue process");
		//try/catch because sometimes the stdin write gots broken
		try {
			p.StandardInput.WriteLine("C");
		} catch {
			LogB.Debug("calling start because continue process was problematic");
			return startProcess();
		}
		*/
		
		//1.5.3 has the isResponding call.
		//in capture, if answers it starts automatically. Don't send a "C"
		//in graph is different because we need to prepare the outputFileCheck files. So there "C" is needed
		LogB.Debug("continuing process");

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
		if (String.IsNullOrEmpty(errorFromR.Data))
			return;

		string str = errorFromR.Data;
		if(str.Length > 6 && str.StartsWith("***") && str.EndsWith("***")) {
			/*
			 * 0123456
			 * ***1***
			 * str.Substring(3,1) 1 is the length
			 */
			str = str.Substring(3, str.Length -6); 
			if(Util.IsNumber(str,false))
				CurvesReaded = Convert.ToInt32(str);

			return;
		}
		
		LogB.Warning(str);
	}

	public void SendEndProcess() 
	{
		if(ExecuteProcess.IsRunning(p)) {
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
	public Preferences.TriggerTypes CutByTriggers;

	public EncoderRProcCapture()
	{
	}
	
	protected override bool startProcess() 
	{
		//If output file is not given, R will try to write in the running folder
		//in which we may haven't got permissions

		string pBin = UtilEncoder.RProcessBinURL();

		pinfo = new ProcessStartInfo();

		//on Windows we need the \"str\" to call without problems in path with spaces
		//pinfo.Arguments = "\"" + "passToR.R" + "\" " + optionsFile;
		//on Windows also if user folder is: C:\Users\name surname, we need the quotes because R will try to open C:\Users\name
		pinfo.Arguments = "\"" + UtilEncoder.GetEncoderScriptCallCaptureNoRdotNet() + "\" " +
			"\"" + optionsFile + "\"";

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
	public bool SendCurve(int startFrame, string curveCompressed)
	{
		/*
		 * curveCompressed print has made crash Chronojump once.
		 * Seems to be a problem with multithreading and Console.SetOut, see logB Commit (added a try/catch there)
		 * on 2016 August 5 (1.6.2) should be fixed with LogSync class, but for now better leave this commented until more tests are done
		 */
		//LogB.Information("curveSend [displacement array]",curveCompressed);


		//since 1.7.1 it's needed to send the startFrame in order to know the startFrame of the accepted repetitions (on R)
		//then this data will be used to save the "Best?" repetitions on C# without calling curves on cont mode

		//TODO: a try/catch coluld be good here to solve this errors:
		//System.IO.IOException: Write fault on path /home/(user)/informatica/progs_meus/chronojump/chronojump/[Unknown]
		//  at System.IO.FileStream.WriteInternal (System.Byte[] src, Int32 offset, Int32 count) [0x00000] in <filename unknown>:0
		//  at System.IO.FileStream.Write (System.Byte[] array, Int32 offset, Int32 count) [0x00000] in <filename unknown>:0
		//  at System.IO.StreamWriter.FlushBytes () [0x00000] in <filename unknown>:0
		//  at System.IO.StreamWriter.FlushCore () [0x00000] in <filename unknown>:0
		//  at System.IO.StreamWriter.Write (System.String value) [0x00000] in <filename unknown>:0
		//  at System.IO.TextWriter.WriteLine (System.String value) [0x00000] in <filename unknown>:0
		//  at EncoderRProcCapture.SendCurve (Int32 startFrame, System.String curveCompressed) [0x00000] in <filename unknown>:0
		//  at EncoderCapture.Capture (System.String outputData1, EncoderRProcCapture encoderRProcCapture, Boolean compujump) [0x00000] in <filename unknown>:0
		//  at ChronoJumpWindow.encoderDoCaptureCsharp () [0x00000] in <filename unknown>:0
		//
		// maybe R capture process is dead
		// this happened capturing after a 20 minutes delay

		/*
		 * "Write fault on path" error fixed on Mono 4.4:
		 * http://www.mono-project.com/docs/about-mono/releases/4.4.0/
		 * https://bugzilla.xamarin.com/show_bug.cgi?id=32905
		 */
		try {
			p.StandardInput.WriteLine(startFrame.ToString());
			p.StandardInput.WriteLine(curveCompressed); 	//this will send some lines because compressed data comes with '\n's
			p.StandardInput.WriteLine("E");		//this will mean the 'E'nd of the curve. Then data can be uncompressed on R
		} catch {
			return false;
		}

		return true;
	}

	protected override void writeOptionsFile()
	{
		optionsFile = Path.GetTempPath() + "Roptions.txt";

		string scriptOptions = UtilEncoder.PrepareEncoderGraphOptions(
				"none", 	//title
				es, 
				false,	//neuromuscularProfile
				false,	//translate (graphs)
				Debug,
				false,	//crossValidate (unactive on capture at the moment)
				(CutByTriggers != Preferences.TriggerTypes.NO_TRIGGERS),
				printTriggers(TriggerList.Type3.ON),
				false, 	//separateSessionInDays (false at capture)
				EncoderGraphROptions.AnalysisModes.CAPTURE,
				Preferences.EncoderInertialGraphsXTypes.EQUIVALENT_MASS //unused on capture
				).ToString();

		TextWriter writer = File.CreateText(optionsFile);
		writer.Write(scriptOptions);
		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();
	}

	protected override string printTriggers(TriggerList.Type3 type3)
	{
		//just use triggeres tp cut sets into repetitions, or not.
		//Cut is done in C# but this will change minHeight behaviour and reduceCurveBySpeed
		if(CutByTriggers != Preferences.TriggerTypes.NO_TRIGGERS)
			return "1";
		else
			return TriggerList.TriggersNotFoundString;
	}

}

public class EncoderRProcAnalyze : EncoderRProc 
{
	private string title;
	private bool neuromuscularProfileDo;
	private bool translate;
	private bool cutByTriggers;
	private TriggerList triggerList;
	private EncoderGraphROptions.AnalysisModes analysisMode;
	private Preferences.EncoderInertialGraphsXTypes inertialGraphX;

	/*
	 * to avoid problems on some windows. R exports csv to Util.GetEncoderExportTempFileName()
	 * then C# copies it to exportFileName
	 */
	public string ExportFileName;

	public bool CancelRScript;

	public EncoderRProcAnalyze() {
	}

	public void SendData(string title, bool neuromuscularProfileDo, bool translate,
			bool cutByTriggers, TriggerList triggerList,
			EncoderGraphROptions.AnalysisModes analysisMode, Preferences.EncoderInertialGraphsXTypes inertialGraphX)
	{
		this.title = title;
		this.neuromuscularProfileDo = neuromuscularProfileDo;
		this.translate = translate;
		this.cutByTriggers = cutByTriggers;
		this.triggerList = triggerList;
		this.analysisMode = analysisMode;
		this.inertialGraphX = inertialGraphX;
		
		CancelRScript = false;
	}

	protected override bool startProcess() 
	{
		CurvesReaded = 0;
		
		//If output file is not given, R will try to write in the running folder
		//in which we may haven't got permissions
	
		pinfo = new ProcessStartInfo();

		string pBin = UtilEncoder.RProcessBinURL();
		
		if (UtilAll.IsWindows()) {
			//On win32 R understands backlash as an escape character and 
			//a file path uses Unix-like path separator '/'		
			optionsFile = optionsFile.Replace("\\","/");
		}
		
		//on Windows we need the \"str\" to call without problems in path with spaces
		//pinfo.Arguments = "\"" + getEncoderScriptCallGraph() + "\" " + optionsFile;
		//on Windows also if user folder is: C:\Users\name surname, we need the quotes because R will try to open C:\Users\name
		pinfo.Arguments = "\"" + getEncoderScriptCallGraph() + "\" " +
			"\"" + optionsFile + "\"";
	
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
		
		//delete status-6 mark used on export csv
		if(es.Ep.Analysis == "exportCSV")
			Util.FileDelete(UtilEncoder.GetEncoderStatusTempBaseFileName() + "6.txt");

		//delete SpecialData if exists
		string specialData = UtilEncoder.GetEncoderSpecialDataTempFileName();
		if (File.Exists(specialData))
			File.Delete(specialData);


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
				while ( ! ( Util.FileReadable(outputFileCheck) || CancelRScript) );
			else
				while ( ! ( (Util.FileReadable(outputFileCheck) && Util.FileReadable(outputFileCheck2)) || CancelRScript ) );

			//copy export from temp file to the file that user has selected
			if(es.Ep.Analysis == "exportCSV" && ! CancelRScript)
				copyExportedFile();
	
		} catch {
			LogB.Warning("catched at startProcess");
			return false;
		}

		return true;

	}
	
	protected override bool continueProcess() 
	{
		CurvesReaded = 0;
		
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
		
		//delete status-6 mark used on export csv
		if(es.Ep.Analysis == "exportCSV")
			Util.FileDelete(UtilEncoder.GetEncoderStatusTempBaseFileName() + "6.txt");
		
		//delete SpecialData if exists
		string specialData = UtilEncoder.GetEncoderSpecialDataTempFileName();
		if (File.Exists(specialData))
			File.Delete(specialData);



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
			while ( ! ( Util.FileReadable(outputFileCheck) || CancelRScript) );
		else
			while ( ! ( (Util.FileReadable(outputFileCheck) && Util.FileReadable(outputFileCheck2)) || CancelRScript ) );
			
		//copy export from temp file to the file that user has selected
		if(es.Ep.Analysis == "exportCSV" && ! CancelRScript)
			copyExportedFile();
		
		LogB.Debug("files written");
		
		return true;
	}

	//copy export from temp file to the file that user has selected
	private void copyExportedFile() {
		//wait first this status mark that is created when file is fully exported
		while ( ! Util.FileExists(UtilEncoder.GetEncoderStatusTempBaseFileName() + "6.txt") ) 
			;

		if (! Util.FileCopySafe (es.OutputData1, ExportFileName, true))
			Config.ErrorInExport = true;
	}
	
	
	private string getEncoderScriptCallGraph()
	{
		return System.IO.Path.Combine(
				Util.GetChronojumpDir (), "encoder", Constants.EncoderScriptCallGraph);
	}
	
	protected override void writeOptionsFile()
	{
		optionsFile = Path.GetTempPath() + "Roptions.txt";
	
		string scriptOptions = UtilEncoder.PrepareEncoderGraphOptions(
				title,
				es, 
				neuromuscularProfileDo,
				translate,
				Debug,
				CrossValidate,
				cutByTriggers,
				printTriggers(TriggerList.Type3.ON),
				SeparateSessionInDays,
				analysisMode,
				inertialGraphX
				).ToString();

		TextWriter writer = File.CreateText(optionsFile);
		writer.Write(scriptOptions);
		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();
	}

	protected override string printTriggers(TriggerList.Type3 type3)
	{
		return triggerList.ToRCurvesString(type3, ';');
	}
		
	private void deleteFile(string filename)
	{
		LogB.Information("Deleting... " + filename);
		if (File.Exists(filename))
			File.Delete(filename);
		LogB.Information("Deleted " + filename);
	}

}
