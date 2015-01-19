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
//using System.Data;
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using System.Diagnostics; 	//for detect OS and for Process
using System.IO; 		//for detect OS

using System.Linq;	//RDotNet
using RDotNet;		//RDotNet

//this class tries to be a space for methods that are used in different classes
public class UtilEncoder
{
	public static bool CancelRScript;


	/********** start of encoder paths ************/
	
	/*
	 * encoder data and graphs are organized by sessions
	 * chronojump / encoder / sessionID / data
	 * chronojump / encoder / sessionID / graphs
	 */
		
	public static string GetEncoderDir() {
		return Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"Chronojump" + Path.DirectorySeparatorChar + "encoder");
	}

	//to store encoder data and graphs
	public static void CreateEncoderDirIfNeeded () {
		string [] dirs = { GetEncoderDir() }; 
		foreach (string d in dirs) {
			if( ! Directory.Exists(d)) {
				Directory.CreateDirectory (d);
				LogB.Information ("created dir:", d);
			}
		}
	}

	public static string GetEncoderSessionDir (int sessionID) {
		return GetEncoderDir() + Path.DirectorySeparatorChar + sessionID.ToString();
	}

	public static string GetEncoderSessionDataDir (int sessionID) {
		return GetEncoderSessionDir(sessionID) + Path.DirectorySeparatorChar + "data";
	}

	public static string GetEncoderSessionDataCurveDir (int sessionID) {
		return GetEncoderSessionDataDir(sessionID) + Path.DirectorySeparatorChar + "curve";
	}

	public static string GetEncoderSessionDataSignalDir (int sessionID) {
		return GetEncoderSessionDataDir(sessionID) + Path.DirectorySeparatorChar + "signal";
	}

	public static string GetEncoderSessionGraphsDir (int sessionID) {
		return GetEncoderSessionDir(sessionID) + Path.DirectorySeparatorChar + "graphs";
	}
	
	public static void CreateEncoderSessionDirsIfNeeded (int sessionID) {
		string [] dirs = { 
			GetEncoderSessionDir(sessionID), GetEncoderSessionDataDir(sessionID), 
			GetEncoderSessionDataCurveDir(sessionID), GetEncoderSessionDataSignalDir(sessionID), 
			GetEncoderSessionGraphsDir(sessionID) }; 
		foreach (string d in dirs) {
			if( ! Directory.Exists(d)) {
				Directory.CreateDirectory (d);
				LogB.Information ("created dir:", d);
			}
		}
	}
	
	public static string GetEncoderDataTempFileName() {
		return Path.Combine(Path.GetTempPath(), Constants.EncoderDataTemp);
	}
	public static string GetEncoderCurvesTempFileName() {
		return Path.Combine(Path.GetTempPath(), Constants.EncoderCurvesTemp);
	}
	public static string GetEncoderAnalyzeTableTempFileName() {
		return Path.Combine(Path.GetTempPath(), Constants.EncoderAnalyzeTableTemp);
	}
	public static string GetEncoderGraphTempFileName() {
		return Path.Combine(Path.GetTempPath(), Constants.EncoderGraphTemp);
	}
	public static string GetEncoderGraphInputMulti() {
		return Path.Combine(Path.GetTempPath(), Constants.EncoderGraphInputMulti);
	}
	//this file will have a ...1.txt ...2.txt ... we check now the first part of the file
	public static string GetEncoderStatusTempBaseFileName() {
		return Path.Combine(Path.GetTempPath(), Constants.EncoderStatusTempBase);
	}
	public static string GetEncoderExportTempFileName() {
		return Path.Combine(Path.GetTempPath(), Constants.EncoderExportTemp);
	}
	public static string GetEncoderSpecialDataTempFileName() {
		return Path.Combine(Path.GetTempPath(), Constants.EncoderSpecialDataTemp);
	}


//	public static void MoveTempToEncoderData(int sessionID, int uniqueID) 
	public static string CopyTempToEncoderData(int sessionID, int uniqueID, string personName, string timeStamp) 
	{
		string fileName="";
		if(File.Exists(GetEncoderDataTempFileName())) {
			CreateEncoderSessionDirsIfNeeded(sessionID);
//			try {
//				File.Move(GetEncoderDataTempFileName(), GetEncoderSessionDataDir(sessionID));
//			} catch {
				fileName = uniqueID.ToString() + "-" + personName + "-" +
						timeStamp + ".txt";
				
				try {
					File.Copy(GetEncoderDataTempFileName(), 
							GetEncoderSessionDataSignalDir(sessionID) + 
							Path.DirectorySeparatorChar + fileName, true);
				} catch {
					new DialogMessage(Constants.MessageTypes.WARNING, Constants.FileCopyProblem);
					LogB.Error(Constants.FileCopyProblem);
					return "";
				}
//			}
		}
		return fileName;
	}
	
	public static bool CopyEncoderDataToTemp(string url, string fileName)
	{
		string origin = url + Path.DirectorySeparatorChar + fileName;
		string dest = GetEncoderDataTempFileName();
		if(File.Exists(origin)) {
			try {
				File.Copy(origin, dest, true);
			} catch {
				new DialogMessage(Constants.MessageTypes.WARNING, Constants.FileCopyProblem);
				LogB.Error(Constants.FileCopyProblem);
				return false;
			}
			return true;
		}

		new DialogMessage(Constants.MessageTypes.WARNING, Constants.FileNotFound);
		LogB.Error(Constants.FileNotFound);
		return false;
	}
	

	/*	
	private static string getEncoderScriptCapturePython() {
		if(UtilAll.IsWindows())
			return System.IO.Path.Combine(Util.GetPrefixDir(), 
				"bin" + Path.DirectorySeparatorChar + "encoder", Constants.EncoderScriptCapturePythonWindows);
		else
			return System.IO.Path.Combine(
					Util.GetDataDir(), "encoder", Constants.EncoderScriptCapturePythonLinux);
	}
	*/
	public static string GetEncoderScriptCaptureNoRdotNet() {
		return System.IO.Path.Combine(
				Util.GetDataDir(), "encoder", Constants.EncoderScriptCaptureNoRDotNet);
	}
	

	/*
	 * in RDotNet, graph.R is in memory, and call_graph.R is not called
	 * if RDotNet is not working, then call_graph.R is called and this calls graph.R
	 */

	private static string getEncoderScriptCallGraph() {
		return System.IO.Path.Combine(
				Util.GetDataDir(), "encoder", Constants.EncoderScriptCallGraph);
	}

	public static string GetEncoderScriptGraph() {
		return System.IO.Path.Combine(
				Util.GetDataDir(), "encoder", Constants.EncoderScriptGraph);
	}
	
	private static string getEncoderScriptInertiaMomentum() {
		return System.IO.Path.Combine(
				Util.GetDataDir(), "encoder", Constants.EncoderScriptInertiaMomentum);
	}
	
	public static string GetEncoderScriptNeuromuscularProfile() {
		return System.IO.Path.Combine(
				Util.GetDataDir(), "encoder", Constants.EncoderScriptNeuromuscularProfile);
	}
	
	public static string GetEncoderScriptUtilR() {
		return System.IO.Path.Combine(
				Util.GetDataDir(), "encoder", Constants.EncoderScriptUtilR);
	}
	
	
	/********** end of encoder paths ************/

	private static string changeSpaceToSpaceMark(string myString) 
	{
		StringBuilder myStringBuilder = new StringBuilder(myString);
		myStringBuilder.Replace(" ", "WINDOWSSPACEMARK");
		return myStringBuilder.ToString();
	}

	
	/*
	 * DEPRECATED, now use always RDotNet
	public static void RunEncoderCapturePython(string title, EncoderStruct es, string port) 
	{
		CancelRScript = false;

		ProcessStartInfo pinfo;
	        Process p;
		//Old comment:
		//If output file is not given, R will try to write in the running folder
		//in which we may haven't got permissions
		
		string pBin="";
		pinfo = new ProcessStartInfo();

		string outputFileCheck = "";
		

		//on Windows (py2exe) we execute a exe with the py file that contains python
		//on linux we execute python and call to the py file
		//also on windows we need the full path to find R
		if (UtilAll.IsWindows()) {
			pBin=getEncoderScriptCapturePython();
			pinfo.Arguments = title + " " + es.OutputData1 + " " + es.Ep.ToString1() + " " + port 
				+ " " + changeSpaceToSpaceMark(
					System.IO.Path.Combine(Util.GetPrefixDir(), "bin" + Path.DirectorySeparatorChar + "R.exe"));
		}
		else {
			pBin="python";
			pinfo.Arguments = getEncoderScriptCapturePython() + " " + title + " " + 
				es.OutputData1 + " " + es.Ep.ToString1() + " " + port;
		}

		outputFileCheck = es.OutputData1;

		pinfo.FileName=pBin;

		pinfo.CreateNoWindow = true;
		pinfo.UseShellExecute = false;

		Console.WriteLine(outputFileCheck);
		if (File.Exists(outputFileCheck))
			File.Delete(outputFileCheck);
	
		p = new Process();
		p.StartInfo = pinfo;
		p.Start();
		Log.WriteLine(p.Id.ToString());

		p.WaitForExit();
		while ( ! ( File.Exists(outputFileCheck) || CancelRScript) );
	}
	*/
	
	public static REngine RunEncoderCaptureCsharpInitializeR(REngine rengine, out Constants.Status RInitialized) 
	{
		LogB.Information("initializing rdotnet");
		
		//RDotNet.StartupParameter rsup = new RDotNet.StartupParameter();
		//rsup.Interactive = false;
		//rsup.Quiet = false;

		rengine = REngine.CreateInstance("RDotNet");
		
		// From v1.5, REngine requires explicit initialization.
		// You can set some parameters.

		try {
			//rengine.Initialize(rsup);
			rengine.Initialize();
		} catch {
			RInitialized = Constants.Status.ERROR;
			return rengine;
		}
		//Previous command, unfortunatelly localizes all GUI to english
		//then call Catalog.Init again in order to see new windows localised		
		//Catalog.Init("chronojump",System.IO.Path.Combine(Util.GetPrefixDir(),"share/locale"));

		//load extrema method copied from EMD package
		string utilRPath = GetEncoderScriptUtilR();
		string graphRPath = GetEncoderScriptGraph();
		
		//On win32 R understands backlash as an escape character and 
		//a file path uses Unix-like path separator '/'
		if(UtilAll.IsWindows()) {
			utilRPath = utilRPath.Replace("\\","/");
			graphRPath = graphRPath.Replace("\\","/");
		}
		LogB.Information(utilRPath);
		LogB.Information(graphRPath);
		
		try {
			//load extrema
			rengine.Evaluate("source('" + utilRPath + "')");
			//load more stuff and call later using RDotNet
			rengine.Evaluate("source('" + graphRPath + "')");
		} catch {
			RInitialized = Constants.Status.ERROR;
			return rengine;
		}

		try {
			// .NET Framework array to R vector.
			NumericVector group1 = rengine.CreateNumericVector(new double[] { 30.02, 29.99, 30.11, 29.97, 30.01, 29.99 });
			rengine.SetSymbol("group1", group1);
			// Direct parsing from R script.
			NumericVector group2 = rengine.Evaluate("group2 <- c(29.89, 29.93, 29.72, 29.98, 30.02, 29.98)").AsNumeric();

			// Test difference of mean and get the P-value.
			GenericVector testResult = rengine.Evaluate("t.test(group1, group2)").AsList();
			double p = testResult["p.value"].AsNumeric().First();

			//not using LogB because like with Console the format of numbers is displayed better
			Console.WriteLine("Group1: [{0}]", string.Join(", ", group1));
			Console.WriteLine("Group2: [{0}]", string.Join(", ", group2));
			Console.WriteLine("P-value = {0:0.000}", p);
		} catch {
			RInitialized = Constants.Status.ERROR;
			return rengine;
		}

		LogB.Information("initialized rdotnet");
		
		RInitialized = Constants.Status.OK;

		return rengine;
	}
	
	
	public static EncoderGraphROptions PrepareEncoderGraphOptions(string title, EncoderStruct es, bool neuromuscularProfileDo, bool translate) 
	{
		string scriptUtilR = GetEncoderScriptUtilR();

		string scriptNeuromuscularProfile = "none"; //cannot be blank
		if(neuromuscularProfileDo)
			scriptNeuromuscularProfile = GetEncoderScriptNeuromuscularProfile();

		string scriptGraphR = GetEncoderScriptGraph();
		
		string operatingSystem = "Linux";
		
		if (UtilAll.IsWindows()) {
			//convert accents to Unicode in order to be plotted correctly on R windows
			title = Util.ConvertToUnicode(title);

			//On win32 R understands backlash as an escape character and 
			//a file path uses Unix-like path separator '/'		
			es.InputData = es.InputData.Replace("\\","/");
			es.OutputGraph = es.OutputGraph.Replace("\\","/");
			es.OutputData1 = es.OutputData1.Replace("\\","/");
			es.OutputData2 = es.OutputData2.Replace("\\","/");
			es.SpecialData = es.SpecialData.Replace("\\","/");
			scriptUtilR = scriptUtilR.Replace("\\","/");
			scriptNeuromuscularProfile = scriptNeuromuscularProfile.Replace("\\","/");
			scriptGraphR = scriptGraphR.Replace("\\","/");
			operatingSystem = "Windows";
		}
		
	 	//if translators add ";", it will be converted to ','
	 	//if translators add a "\n", it will be converted to " "
		int count = 0;
		string temp = "";
		string [] encoderTranslatedWordsOK = new String [Constants.EncoderTranslatedWords.Length];

		//if ! translate, then just print the english words
		if(translate) {
			foreach(string etw in Constants.EncoderTranslatedWords) {
				temp = Util.ChangeChars(etw, ";", ",");
				temp = Util.RemoveNewLine(temp, true);
				encoderTranslatedWordsOK[count++] = temp;
			}
		} else
			encoderTranslatedWordsOK = Constants.EncoderEnglishWords;

		return new EncoderGraphROptions( 
				es.InputData, es.OutputGraph, es.OutputData1, 
				es.OutputData2, es.SpecialData, 
				es.Ep,
				title, operatingSystem,
				scriptUtilR, scriptNeuromuscularProfile,
				Util.StringArrayToString(Constants.EncoderEnglishWords,";"),
				Util.StringArrayToString(encoderTranslatedWordsOK,";"), 
				scriptGraphR);
	}

	/*	
	 *	Currently unused because this was called in main thread and when a pulse thread exists
	 *	It should be without threading or in the GTK thread
	 *	Now graph call is so fast with the call_graph.R
	 *	then there's no need to add the complexity of calling RunEncoderGraphRDotNet outside the thread
	 *	because the GTK thread has some interesting things that it does when it ends.
	 *	We have enough speed now and we don't want to add more bugs
	 *
	public static bool RunEncoderGraph(REngine rengine, Constants.Status RInitialized,
			string title, EncoderStruct es, bool neuromuscularProfileDo) 
	{
		if(RInitialized == Constants.Status.UNSTARTED)
			RunEncoderCaptureCsharpInitializeR(rengine, out RInitialized);
		
		bool result = false;
		if(RInitialized == Constants.Status.ERROR)
			result = RunEncoderGraphNoRDotNet(title, es, neuromuscularProfileDo);
		else if(RInitialized == Constants.Status.OK)
			result = RunEncoderGraphRDotNet(rengine, title, es, neuromuscularProfileDo);

		return result;
	}

	//this method uses RDotNet
	public static bool RunEncoderGraphRDotNet(REngine rengine, string title, EncoderStruct es, bool neuromuscularProfileDo) 
	{
		//if RDotNet is ok, graph.R is already loaded
		rengine.Evaluate("meanPower <- mean(c(2,3,4,5,6,7,8))");
		double meanPower = rengine.GetSymbol("meanPower").AsNumeric().First();
		Log.WriteLine(meanPower.ToString());
		rengine.Evaluate("print(findPosInPaf(\"Power\", \"max\"))");

		EncoderGraphROptions roptions = PrepareEncoderGraphOptions(title, es, neuromuscularProfileDo);
		Log.WriteLine(roptions.ToString());	

		//--------------------------------------------
		//		Attention
		//this code should be the same as call_graph.R
		//--------------------------------------------

		//TODO: pass roptions to RDotNet objects and then call graph.R
		CharacterVector charVec;
	
		Log.WriteLine("-1-");	
		string str_string = roptions.outputData2;
		Log.WriteLine(str_string);
		
		charVec = rengine.CreateCharacterVector(new[] { str_string });
		rengine.SetSymbol("OutputData2", charVec);
	
		Log.WriteLine("-2-");	
		Log.WriteLine(roptions.specialData);	
		Log.WriteLine("-3-");	
		CharacterVector charVec2 = rengine.CreateCharacterVector(new[] { roptions.specialData });
		rengine.SetSymbol("SpecialData", charVec2);
		
		Log.WriteLine(roptions.operatingSystem);	
		charVec = rengine.CreateCharacterVector(new[] { roptions.operatingSystem });
		rengine.SetSymbol("OperatingSystem", charVec);

		return true;
	}
	*/



	public static void RunEncoderCaptureNoRDotNetSendCurve(Process p, double [] d)
	{
		LogB.Debug("writing line 1");
		string curveSend = string.Join(" ", Array.ConvertAll(d, x => x.ToString()));
		
		//TODO convert comma to point in this doubles

		LogB.Debug("curveSend",curveSend);
		p.StandardInput.WriteLine(curveSend);
	}
	public static void RunEncoderCaptureNoRDotNetSendEnd(Process p)
	{
		LogB.Debug("sending end line");
		p.StandardInput.WriteLine("Q");
	}

	/*
	 * this method don't use RDotNet, then has to call call_graph.R, who will call graph.R
	 * and has to write a Roptions.txt file
	 */
	public static bool RunEncoderGraphNoRDotNet(string title, EncoderStruct es, bool neuromuscularProfileDo, bool translate) 
	{
		CancelRScript = false;

		ProcessStartInfo pinfo;
	        Process p;
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
		

		string scriptOptions = PrepareEncoderGraphOptions(title, es, neuromuscularProfileDo, translate).ToString();

		string optionsFile = Path.GetTempPath() + "Roptions.txt";
		TextWriter writer = File.CreateText(optionsFile);
		writer.Write(scriptOptions);
		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();
		
		if (UtilAll.IsWindows()) {
			//On win32 R understands backlash as an escape character and 
			//a file path uses Unix-like path separator '/'		
			optionsFile = optionsFile.Replace("\\","/");
		}
		
		//on Windows we need the \"str\" to call without problems in path with spaces
		pinfo.Arguments = "\"" + getEncoderScriptCallGraph() + "\" " + optionsFile;
	
		LogB.Information("Arguments:", pinfo.Arguments);
		LogB.Information("--- 1 --- " + optionsFile.ToString() + " ---");
		LogB.Information("--- 2 --- " + scriptOptions + " ---");
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

		pinfo.FileName=pBin;

		pinfo.CreateNoWindow = true;
		pinfo.UseShellExecute = false;
		pinfo.RedirectStandardError = true;
		pinfo.RedirectStandardOutput = true; 


		//delete output file check(s)
		LogB.Information("Deleting... " + outputFileCheck);
		if (File.Exists(outputFileCheck))
			File.Delete(outputFileCheck);

		if(outputFileCheck2 != "") {
			LogB.Information("Deleting... " + outputFileCheck2);
			if (File.Exists(outputFileCheck2))
				File.Delete(outputFileCheck2);
		}
			
		//delete 1RM data if exists
		if (File.Exists(es.SpecialData))
			File.Delete(es.SpecialData);

		//try catch crash sometimes when used in conjunction with RDotNet
//		try {	
			p = new Process();
			p.StartInfo = pinfo;
			p.Start();

			LogB.Information(p.StandardOutput.ReadToEnd());
			LogB.Warning(p.StandardError.ReadToEnd());

			p.WaitForExit();

			if(outputFileCheck2 == "")
				while ( ! ( File.Exists(outputFileCheck) || CancelRScript) );
			else
				while ( ! ( (File.Exists(outputFileCheck) && File.Exists(outputFileCheck2)) || CancelRScript ) );
		/*
		} catch {
			return false;
		}
		*/

		return true;
	}
	
	//Inertia Momentum
	public static void RunEncoderCalculeIM(double weight, double length) 
	{
		CancelRScript = false;

		ProcessStartInfo pinfo;
	        Process p;
		//If output file is not given, R will try to write in the running folder
		//in which we may haven't got permissions
		
		string pBin="";
		pinfo = new ProcessStartInfo();

		string inputData = GetEncoderDataTempFileName();
		string outputData = GetEncoderSpecialDataTempFileName();
		string operatingSystem = "Linux";
		
		string scriptUtilR = GetEncoderScriptUtilR();

			
		pBin="Rscript";
		if (UtilAll.IsWindows()) {
			//on Windows we need the \"str\" to call without problems in path with spaces
			pBin = "\"" + System.IO.Path.Combine(Util.GetPrefixDir(), "bin" + Path.DirectorySeparatorChar + "Rscript.exe") + "\"";
			LogB.Information("pBin:", pBin);

			//On win32 R understands backlash as an escape character and 
			//a file path uses Unix-like path separator '/'		
			inputData = inputData.Replace("\\","/");
			scriptUtilR = scriptUtilR.Replace("\\","/");
			operatingSystem = "Windows";
		}
		
		//--- way A. passing options to a file
		string scriptOptions = 
			inputData + "\n" + 
			outputData + "\n" + 
			Util.ConvertToPoint(weight) + "\n" + 
			Util.ConvertToPoint(length) + "\n" +
			scriptUtilR + "\n";

		string optionsFile = Path.GetTempPath() + "Roptions.txt";
		TextWriter writer = File.CreateText(optionsFile);
		writer.Write(scriptOptions);
		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();
		
		if (UtilAll.IsWindows()) {
			//On win32 R understands backlash as an escape character and 
			//a file path uses Unix-like path separator '/'		
			optionsFile = optionsFile.Replace("\\","/");
		}
		
		//on Windows we need the \"str\" to call without problems in path with spaces
		pinfo.Arguments = "\"" + getEncoderScriptInertiaMomentum() + "\" " + optionsFile;
		LogB.Information("Arguments:", pinfo.Arguments);
		
		//Wait until this to update encoder gui (if don't wait then treeview will be outdated)
		string outputFileCheck = outputData;

		pinfo.FileName=pBin;

		pinfo.CreateNoWindow = true;
		pinfo.UseShellExecute = false;
		pinfo.RedirectStandardError = true;
		pinfo.RedirectStandardOutput = true; 

		//delete output file check(s)
		LogB.Information("Deleting... ", outputFileCheck);
		if (File.Exists(outputFileCheck))
			File.Delete(outputFileCheck);

		try {	
			p = new Process();
			p.StartInfo = pinfo;
			p.Start();
			
			LogB.Information(p.StandardOutput.ReadToEnd());
			LogB.Warning(p.StandardError.ReadToEnd());

			p.WaitForExit();

			while ( ! ( File.Exists(outputFileCheck) || CancelRScript) );
		} catch {
		}
	}


	private static string [] encoderFindPos(string contents, int start, int duration) {
		int startPos = 0;
		int durationPos = 0;
		int i,digits;
		for(i=0, digits=0; i < contents.Length; i++) {
			if(Char.IsDigit(contents[i])) {
				digits ++;
				if(digits==start) {
					startPos = i;
					//but digit can be negative, check previous char if it was a '-'
					if(i>0 && contents[i-1] == '-')
						startPos = i-1;
					//duration == -1 means: until the end
					if(duration == -1) {
						//when removing from startPos until the end,
						//the ',' before startPos will be in the end of the file
						//and then chronojump will try to read after that comma
						//because it reads in a Split (',')
						//for this reason we need to start removing that comma if exists
						if(contents[startPos-1] == ',')
							startPos --;
						
						durationPos = contents.Length - startPos;
						break;
					}
				}
				if(i>0 && digits == start + duration)
					durationPos = i-startPos;
			}
		}
		//Log.WriteLine("s "+ startPos.ToString());
		//Log.WriteLine("d "+ durationPos.ToString());
		//Log.WriteLine("i " + i.ToString());

		string [] returnStr = new string[2];
		returnStr [0] = startPos.ToString();
		returnStr [1] = durationPos.ToString();
		return returnStr;
	}


	public static string EncoderSaveCurve(string fileNameSignal, 
			int start, int duration, 
			int inertialCheckStart, int inertialCheckDuration, 
			bool inertialCheckPositive, //has to be positive (true) or negative (false)
			int sessionID, int uniqueID, string personName, string timeStamp, int curveIDMax) 
	{
		string contents = Util.ReadFile(fileNameSignal, false);
		
			
		/*
		 * at inertial signals, first curve is eccentric (can be to left or right, maybe positive or negative)
		 * graph.R manages correctly this
		 * But, when saved a curve, eg. concentric this can be positive or negative
		 * (depending on the rotating sign of inertial machine at that curve)
		 * if it's concentric, and it's full of -1,-2,... we have to change sign
		 * if it's eccentric-concentric, and in the eccentric phase is positive, then we should change sign of both phases
		 */
		bool reverseSign = false;
		if(inertialCheckStart != 0 && inertialCheckDuration != 0) {
			string [] startAndDurationCheckInertial = encoderFindPos(contents, inertialCheckStart, inertialCheckDuration);
			string contentsCheckInertial = contents.Substring(
					Convert.ToInt32(startAndDurationCheckInertial[0]), 
					Convert.ToInt32(startAndDurationCheckInertial[1])-1); //-1 is for not ending file with a comma
			
			//see mean of contentsCheckInertial
			int sum = 0;
			int count = 0;
			using (StringReader reader = new StringReader (contentsCheckInertial)) {
				do {
					string line = reader.ReadLine ();
					if (line == null)
						break;

					string [] values = line.Split(new char[] {','});
					foreach(string str in values) {
						if (str == null || str == "" || str == " ")
							break;

						//Log.Write ("(" + str + ":");
						int num = Convert.ToInt32(str);
						//Log.Write (num.ToString() + ")");
						sum += num;
						count ++;
					}
				} while(true);
				
				if(sum == 0 || count == 0) 
					LogB.Warning("inertial check == 0, no data");
				else {
					double average = sum * 1.0 / count * 1.0;
					LogB.Information("inertial check == " + average.ToString());
					if( 
							(average < 0 && inertialCheckPositive) ||
							(average > 0 && ! inertialCheckPositive) ) {
						reverseSign = true;
					}
				}
			}
		}


		string [] startAndDuration = encoderFindPos(contents, start, duration);
		contents = contents.Substring(
				Convert.ToInt32(startAndDuration[0]), 
				Convert.ToInt32(startAndDuration[1])-1); //-1 is for not ending file with a comma
		//don't know why but some curves are stored with a "," as last character
		//this curves also are in the form: "1, 2, 3, 4," instead of "1,2,3,4"
		//this produces an NA in reading of curves on graph.R
		//in the meantime this NA in reading in graph.R has been deleted
		//dataTempFile  = dataTempFile[!is.na(dataTempFile)]
		
		string fileCurve = uniqueID.ToString() + "-" + personName + "-" + 
			(++ curveIDMax).ToString() + "-" + timeStamp + ".txt";
		string fileCurveFull = GetEncoderSessionDataCurveDir(sessionID) + Path.DirectorySeparatorChar + fileCurve;


		if(reverseSign) {
			LogB.Information("reversingSign");
			string contentsReversed = "";
			string sep = "";
			using (StringReader reader = new StringReader (contents)) {
				do {
					string line = reader.ReadLine ();
					if (line == null)
						break;

					string [] values = line.Split(new char[] {','});
					foreach(string str in values) {
						if (str == null || str == "" || str == " ")
							break;

						//Log.Write ("(" + str + ":");
						int num = Convert.ToInt32(str) * -1;
						//Log.Write (num.ToString() + ")");
						contentsReversed += sep + num.ToString();
						sep = ", ";
					}
				} while(true);
			}
			contents = contentsReversed;
		}

		
		TextWriter writer = File.CreateText(fileCurveFull);
		writer.Write(contents);
		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();

		return fileCurve;
	}

	public static ArrayList EncoderConfigurationList(Constants.EncoderType encoderType) {
		ArrayList list = new ArrayList();
		if(encoderType == Constants.EncoderType.LINEAR) {
			list.Add(new EncoderConfiguration(
					Constants.EncoderConfigurationNames.LINEAR));
			list.Add(new EncoderConfiguration(
					Constants.EncoderConfigurationNames.LINEARINVERTED));
			list.Add(new EncoderConfiguration(
					Constants.EncoderConfigurationNames.LINEARINERTIAL));
			list.Add(new EncoderConfiguration(
					Constants.EncoderConfigurationNames.WEIGHTEDMOVPULLEYLINEARONPERSON1));
			list.Add(new EncoderConfiguration(
					Constants.EncoderConfigurationNames.WEIGHTEDMOVPULLEYLINEARONPERSON1INV));
			list.Add(new EncoderConfiguration(
					Constants.EncoderConfigurationNames.WEIGHTEDMOVPULLEYLINEARONPERSON2));
			list.Add(new EncoderConfiguration(
					Constants.EncoderConfigurationNames.WEIGHTEDMOVPULLEYLINEARONPERSON2INV));
			list.Add(new EncoderConfiguration(
					Constants.EncoderConfigurationNames.WEIGHTEDMOVPULLEYONLINEARENCODER));
			list.Add(new EncoderConfiguration(
					Constants.EncoderConfigurationNames.LINEARONPLANE));
			list.Add(new EncoderConfiguration(
					Constants.EncoderConfigurationNames.LINEARONPLANEWEIGHTDIFFANGLE));
		} else if(encoderType == Constants.EncoderType.ROTARYFRICTION) {
			list.Add(new EncoderConfiguration(
					Constants.EncoderConfigurationNames.ROTARYFRICTIONSIDE));
			list.Add(new EncoderConfiguration(
					Constants.EncoderConfigurationNames.ROTARYFRICTIONAXIS));
			list.Add(new EncoderConfiguration(
					Constants.EncoderConfigurationNames.ROTARYFRICTIONSIDEINERTIAL));
			list.Add(new EncoderConfiguration(
					Constants.EncoderConfigurationNames.ROTARYFRICTIONAXISINERTIAL));
			list.Add(new EncoderConfiguration(
					Constants.EncoderConfigurationNames.WEIGHTEDMOVPULLEYROTARYFRICTION));
		} else if(encoderType == Constants.EncoderType.ROTARYAXIS) {
			list.Add(new EncoderConfiguration(
					Constants.EncoderConfigurationNames.ROTARYAXIS));
			list.Add(new EncoderConfiguration(
					Constants.EncoderConfigurationNames.ROTARYAXISINERTIAL));
			list.Add(new EncoderConfiguration(
					Constants.EncoderConfigurationNames.WEIGHTEDMOVPULLEYROTARYAXIS));
		}
		return list;
	}

	/* -------- EncoderConfiguration, kinematics and Dynamics ---- 
	 *
	 *  		this is the same than graph.R
	 * -------------------------------------------------------- */

	/*
	 * in signals and curves, need to do conversions (invert, inertiaMomentum, diameter)
	 * we use 'data' variable because can be position or displacement
	 */

	public static double GetDisplacement(int byteReaded, EncoderConfiguration ec) {
		/* no change:
		 * WEIGHTEDMOVPULLEYLINEARONPERSON1, WEIGHTEDMOVPULLEYLINEARONPERSON1INV,
		 * WEIGHTEDMOVPULLEYLINEARONPERSON2, WEIGHTEDMOVPULLEYLINEARONPERSON2INV,
		 * LINEARONPLANE, LINEARONPLANEWEIGHTDIFFANGLE, ROTARYFRICTIONSIDE, WEIGHTEDMOVPULLEYROTARYFRICTION
		 */

		double data = byteReaded;
		if(
				ec.name == Constants.EncoderConfigurationNames.LINEARINVERTED ||
				ec.name == Constants.EncoderConfigurationNames.WEIGHTEDMOVPULLEYLINEARONPERSON1INV ||
				ec.name == Constants.EncoderConfigurationNames.WEIGHTEDMOVPULLEYLINEARONPERSON2INV ) {
			data *= -1;
		} else if(ec.name == Constants.EncoderConfigurationNames.WEIGHTEDMOVPULLEYONLINEARENCODER) {
			//default is: gearedDown = 2. Future maybe this will be a parameter
			data *= 2;
		} else if(ec.name == Constants.EncoderConfigurationNames.ROTARYFRICTIONAXIS) {
			data = data * ec.D / ec.d;
		} else if(
				ec.name == Constants.EncoderConfigurationNames.ROTARYAXIS || 
				ec.name == Constants.EncoderConfigurationNames.WEIGHTEDMOVPULLEYROTARYAXIS) 
		{
			int ticksRotaryEncoder = 200; //our rotary axis encoder send 200 ticks by turn
			//diameter m -> mm
			data = ( data / ticksRotaryEncoder ) * 2 * Math.PI * ( ec.d * 1000 / 2 );
		}
		return data;
	}

	//gearedDown is positive, normally 2
	private static double getMass(double mass, int gearedDown, int angle) {
		if(mass == 0)
			return 0;

		return ( ( mass / gearedDown ) * Math.Sin( angle * Math.PI / 180 ) );
	}

	private static double getMassBodyByExercise(double massBody, int exercisePercentBodyWeight) {
		if(massBody == 0 || exercisePercentBodyWeight == 0)
			return 0;

		return (massBody * exercisePercentBodyWeight / 100.0);
	}

	public static double GetMassByEncoderConfiguration(
			EncoderConfiguration ec, double massBody, double massExtra, int exercisePercentBodyWeight)
	{
		massBody = getMassBodyByExercise(massBody, exercisePercentBodyWeight);

		if(
			ec.name == Constants.EncoderConfigurationNames.WEIGHTEDMOVPULLEYLINEARONPERSON1 ||
			ec.name == Constants.EncoderConfigurationNames.WEIGHTEDMOVPULLEYLINEARONPERSON1INV ||
			ec.name == Constants.EncoderConfigurationNames.WEIGHTEDMOVPULLEYLINEARONPERSON2 ||
			ec.name == Constants.EncoderConfigurationNames.WEIGHTEDMOVPULLEYLINEARONPERSON2INV ||
			ec.name == Constants.EncoderConfigurationNames.WEIGHTEDMOVPULLEYROTARYFRICTION ||
			ec.name == Constants.EncoderConfigurationNames.WEIGHTEDMOVPULLEYROTARYAXIS 
		  ) {
			massExtra = getMass(massExtra, ec.gearedDown, ec.anglePush);
		} 
		else if(ec.name == Constants.EncoderConfigurationNames.LINEARONPLANE) {
			massBody = getMass(massBody, ec.gearedDown, ec.anglePush);
			massExtra = getMass(massExtra, ec.gearedDown, ec.anglePush);
		}
		else if(ec.name == Constants.EncoderConfigurationNames.LINEARONPLANEWEIGHTDIFFANGLE) {
			massBody = getMass(massBody, ec.gearedDown, ec.anglePush);
			massExtra = getMass(massExtra, ec.gearedDown, ec.angleWeight);
		}

		return (massBody + massExtra);
	}

	/* ----end of EncoderConfiguration, kinematics and Dynamics ---- */ 

}
