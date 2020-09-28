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
 * Copyright (C) 2019-2020   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.IO; //"File" things. TextWriter. Path
using System.Diagnostics; 	//for detect OS and for Process

public class CallR
{
	private string optionsFile;
	private EncoderStruct es;

	public EncoderGraphROptions.AnalysisModes analysisMode;

	public CallR(EncoderStruct es)
	{
		this.es = es;
		this.analysisMode = EncoderGraphROptions.AnalysisModes.INDIVIDUAL_CURRENT_SET;

		writeOptionsFile();

		callRStart();
	}

	private bool callRStart()
	{
		ProcessStartInfo pinfo = new ProcessStartInfo();
		string pBin="Rscript";
	
		/*	
		if (UtilAll.IsWindows()) {
			//On win32 R understands backlash as an escape character and 
			//a file path uses Unix-like path separator '/'		
			optionsFile = optionsFile.Replace("\\","/");
		}
		*/
		
		//on Windows we need the \"str\" to call without problems in path with spaces
		pinfo.Arguments = "\"" + getEncoderScriptCallGraph() + "\" " + optionsFile;

		/*	
		Console.WriteLine("Arguments:", pinfo.Arguments);
		Console.WriteLine("--- 1 --- " + optionsFile.ToString() + " ---");
		//Console.WriteLine("--- 2 --- " + scriptOptions + " ---");
		Console.WriteLine("--- 3 --- " + pinfo.Arguments.ToString() + " ---");
		*/

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
		        //check also the output graph
			//do not do it hat processMultiDatabases because we do not need it and we want to go fast
			//outputFileCheck2 = es.OutputGraph;
		}
			
		Console.WriteLine("outputFileChecks");
		Console.WriteLine(outputFileCheck);
		Console.WriteLine(outputFileCheck2);

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
		Util.DeleteFile(outputFileCheck);
		if(outputFileCheck2 != "")
			Util.DeleteFile(outputFileCheck2);
		
		//delete status-6 mark used on export csv
		if(es.Ep.Analysis == "exportCSV")
			Util.DeleteFile(getEncoderStatusTempBaseFileName() + "6.txt");

		//delete SpecialData if exists
		string specialData = getEncoderSpecialDataTempFileName();
		if (File.Exists(specialData))
			Util.DeleteFile(specialData);


//		try {	
			Process p = new Process();
			p.StartInfo = pinfo;
			
			//do not redirect ouptut. Read above
			//p.OutputDataReceived += new DataReceivedEventHandler(readingOutput);
			p.ErrorDataReceived += new DataReceivedEventHandler(readingError);
			
			p.Start();

			//don't do this ReadToEnd because then this method never ends
			//Console.WriteLine(p.StandardOutput.ReadToEnd()); 
			//LogB.Warning(p.StandardError.ReadToEnd());
			
			// Start asynchronous read of the output.
			// Caution: This has to be called after Start
			//p.BeginOutputReadLine();
			p.BeginErrorReadLine();

			if(outputFileCheck2 == "")
				while ( ! ( Util.FileReadable(outputFileCheck) )); //|| CancelRScript) );
			else
				while ( ! ( (Util.FileReadable(outputFileCheck) && Util.FileReadable(outputFileCheck2)) ));//|| CancelRScript ) );

			/*
			//copy export from temp file to the file that user has selected
			if(es.Ep.Analysis == "exportCSV")// && ! CancelRScript)
				copyExportedFile();
				*/

			try {
				p.StandardInput.WriteLine("Q");
			} catch {
				Console.WriteLine("catched at CallR at sending the Q, maybe process has exited.");
				return false;
			}

//		} catch {
//			Console.WriteLine("catched at startProcess");
//			return false;
//		}

		return true;
	}

        private void readingError (object sendingProcess, DataReceivedEventArgs errorFromR)
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
//                        if(Util.IsNumber(str,false))
//                              CurvesReaded = Convert.ToInt32(str);
 
                        return;
                }
        
                Console.WriteLine(str);
        }

	private string getEncoderScriptCallGraph()
	{
		/*
		return System.IO.Path.Combine(
				Util.GetDataDir(), "encoder", Constants.EncoderScriptCallGraph);
				*/

		//but note if graph.R changes, we need to do make install there to be able to use new graph.R on processMultiDatabases
		//return "/home/xavier/informatica/progs_meus/chronojump/chronojump/encoder/call_graph.R"; //hardcoded (computer)
		return "/home/xavier/informatica/progs_meus/chronojump/encoder/call_graph.R"; //hardcoded (laptop)
	}
	
	private string getEncoderStatusTempBaseFileName() {
		return Path.Combine(Path.GetTempPath(), Constants.EncoderStatusTempBase);
	}

	private string getEncoderSpecialDataTempFileName() {
		return Path.Combine(Path.GetTempPath(), Constants.EncoderSpecialDataTemp);
	}

	private void writeOptionsFile()
	{
		optionsFile = Path.GetTempPath() + "Roptions.txt";
	
		string scriptOptions = UtilEncoder.PrepareEncoderGraphOptions(
				"hola", //title,
				es, 
				false, //neuromuscularProfileDo,
				false, //translate,
				false, //Debug,
				false, //CrossValidate,
				false, //cutByTriggers,
				"-1", //printTriggers(),
				false, //SeparateSessionInDays,
				analysisMode
				).ToString();

		TextWriter writer = File.CreateText(optionsFile);
		writer.Write(scriptOptions);
		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();
	}
}

