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
 *  Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
//using System.Data;
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using System.Collections.Generic; //List<T>
using System.Diagnostics; 	//for detect OS and for Process
using System.IO; 		//for detect OS
using Mono.Unix;


//this class tries to be a space for methods that are used in different classes
public class UtilEncoder
{

	/********** start of encoder paths ************/
	
	/*
	 * encoder data and graphs are organized by sessions
	 * chronojump / encoder / sessionID / data
	 * chronojump / encoder / sessionID / graphs
	 */
		
	public static string GetEncoderDir() {
		return Path.Combine(
				Util.GetLocalDataDir (false) + Path.DirectorySeparatorChar + "encoder");
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
	
	public static string GetEncoderCaptureTempFileName() {
		return Path.Combine(Path.GetTempPath(), Constants.EncoderCaptureTemp);
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
	public static string GetEncoderInstantDataTempFileName() {
		return Path.Combine(Path.GetTempPath(), Constants.EncoderInstantDataTemp);
	}
	public static string GetEncoderTempPathWithoutLastSep() {
		string s = Path.GetTempPath(); //is just temp path

		//but send it without the final '\' or '/' (if found)
		if(s.EndsWith("/") || s.EndsWith("\\"))
			s = s.Substring(0, s.Length -1);

		return s;
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
					new DialogMessage(Constants.MessageTypes.WARNING, Constants.FileCopyProblemStr());
					LogB.Error(Constants.FileCopyProblemStr());
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
				new DialogMessage(Constants.MessageTypes.WARNING, Constants.FileCopyProblemStr());
				LogB.Error(Constants.FileCopyProblemStr());
				return false;
			}
			return true;
		}

		new DialogMessage(Constants.MessageTypes.WARNING, Constants.FileNotFoundStr());
		LogB.Error(Constants.FileNotFoundStr());
		return false;
	}

	/*	
	private static string getEncoderScriptCapturePython() {
		if(UtilAll.IsWindows())
			return System.IO.Path.Combine(Util.GetPrefixDir(), 
				"bin" + Path.DirectorySeparatorChar + "encoder", Constants.EncoderScriptCapturePythonWindows);
		else
			return System.IO.Path.Combine(
					Util.GetChronojumpDir(), "encoder", Constants.EncoderScriptCapturePythonLinux);
	}
	*/
	public static string GetEncoderScriptCallCaptureNoRdotNet() {
		return System.IO.Path.Combine(
				Util.GetChronojumpDir(), "encoder", Constants.EncoderScriptCallCaptureNoRDotNet);
	}
	
	public static string GetEncoderScriptCaptureNoRdotNet() {
		return System.IO.Path.Combine(
				Util.GetChronojumpDir(), "encoder", Constants.EncoderScriptCaptureNoRDotNet);
	}
	

	public static string GetEncoderScriptGraph() {
		return System.IO.Path.Combine(
				Util.GetChronojumpDir(), "encoder", Constants.EncoderScriptGraph);
	}
	
	private static string getEncoderScriptInertiaMomentum() {
		return System.IO.Path.Combine(
				Util.GetChronojumpDir(), "encoder", Constants.EncoderScriptInertiaMomentum);
	}
	
	public static string GetEncoderScriptNeuromuscularProfile() {
		return System.IO.Path.Combine(
				Util.GetChronojumpDir(), "encoder", Constants.EncoderScriptNeuromuscularProfile);
	}
	
	public static string GetEncoderScriptUtilR() {
		return System.IO.Path.Combine(
				Util.GetChronojumpDir(), "encoder", Constants.EncoderScriptUtilR);
	}
	
	public static string GetEncoderScriptsPathWithoutLastSep() {
		string s = System.IO.Path.Combine(Util.GetChronojumpDir(), "encoder");

		//but send it without the final '\' or '/' (if found)
		if(s.EndsWith("/") || s.EndsWith("\\"))
			s = s.Substring(0, s.Length -1);

		return s;
	}

	
	
	/********** end of encoder paths ************/


	/********** start of r-scripts paths ************/

	public static string GetSprintPath() {
		return System.IO.Path.Combine(Util.GetChronojumpDir(), "r-scripts");
	}
	public static string GetSprintPhotocellsScript() {
		return System.IO.Path.Combine(GetSprintPath(), "sprintPhotocells.R");
	}
	public static string GetSprintCSVInputMulti() {
		return Path.Combine(Path.GetTempPath(), "sprintInputMulti.csv");
	}

	public static string GetSprintImage() {
		return System.IO.Path.Combine(Path.GetTempPath(), "sprintGraph.png");
	}
	public static string GetSprintEncoderImage() {
		return System.IO.Path.Combine(Path.GetTempPath(), "sprintEncoderGraph.png");
	}

	public static string GetScriptsPath() {
		return System.IO.Path.Combine(Util.GetChronojumpDir(), "r-scripts");
	}

	public static string GetmifScript() {
		return System.IO.Path.Combine(GetSprintPath(), "maximumIsometricForce.R");
	}
	public static string GetmifCSVInputMulti() {
		return Path.Combine(Path.GetTempPath(), "maximumIsometricForceInputMulti.csv");
	}
	public static string GetmifCSVFileName() {
		return Path.Combine(Path.GetTempPath(), "cj_mif_Data.csv");
	}
	public static string GetmifCSVFileName_CD () {
		return Path.Combine(Path.GetTempPath(), "cj_mif_Data_CD.csv");
	}
	public static string GetmifTempFileName() {
		return Path.Combine(Path.GetTempPath(), "cj_mif_Graph.png");
	}

	/********** end of r-scripts paths ************/

	private static string changeSpaceToSpaceMark(string myString) 
	{
		StringBuilder myStringBuilder = new StringBuilder(myString);
		myStringBuilder.Replace(" ", "WINDOWSSPACEMARK");
		return myStringBuilder.ToString();
	}

	
	/*
	 * DEPRECATED, now use always RDotNet. Until 1.5.0 where RDotNet is not used anymore. Neither this Pyton method.
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

	//R plots on Windows using Cairo, need to know if it's windows or not
	public static string OperatingSystemForRGraphs()
	{
		string operatingSystem = "Linux";
		if (UtilAll.IsWindows())
			operatingSystem = "Windows";

		return operatingSystem;
	}

	public static EncoderGraphROptions PrepareEncoderGraphOptions(
			string title, EncoderStruct es, bool neuromuscularProfileDo, bool translate, bool debug, bool crossValidate,
			bool cutByTriggers, string triggerStr, bool separateSessionInDays,
			EncoderGraphROptions.AnalysisModes analysisMode, Preferences.EncoderInertialGraphsXTypes inertialGraphX)
	{
		string operatingSystem = OperatingSystemForRGraphs();
			
		title = Util.RemoveBackSlash(title);
		title = Util.RemoveChar(title, '\''); 
		
		if (UtilAll.IsWindows()) {
			//convert accents to Unicode in order to be plotted correctly on R windows
			title = Util.ConvertToUnicode(title);

			//On win32 R understands backlash as an escape character and 
			//a file path uses Unix-like path separator '/'		
			es.InputData = es.InputData.Replace("\\","/");
			es.OutputGraph = es.OutputGraph.Replace("\\","/");
			es.OutputData1 = es.OutputData1.Replace("\\","/");
			//es.OutputData2 = es.OutputData2.Replace("\\","/");
			//es.SpecialData = es.SpecialData.Replace("\\","/");
			es.EncoderTempPath = es.EncoderTempPath.Replace("\\","/");
		}
		
	 	//if translators add ";", it will be converted to ','
	 	//if translators add a "\n", it will be converted to " "
		int count = 0;
		string temp = "";
		string [] encoderTranslatedWordsOK = new String [Constants.EncoderTranslatedWords.Length];

		//if ! translate, then just print the english words
		if(translate) {
			foreach(string etw in Constants.EncoderTranslatedWords) {
				temp = Util.ChangeChars(Catalog.GetString(etw), ";", ",");
				temp = Util.RemoveChar(temp, '\'');
				temp = Util.RemoveNewLine(temp, true);
				temp = Util.RemoveChar(temp, '#'); //needed to distinguish comments '#' than normal lines like the EncoderTranslatedWords
		
				if (UtilAll.IsWindows()) {
					LogB.Debug(" (1) Unicoding:", temp);
					temp = Util.ConvertToUnicode(temp);
					LogB.Debug(" (2) Unicoded:", temp);
				}

				encoderTranslatedWordsOK[count++] = temp;

			}
		} else
			encoderTranslatedWordsOK = Constants.EncoderEnglishWords;

		return new EncoderGraphROptions( 
				es.InputData, es.OutputGraph, es.OutputData1, 
				//es.OutputData2, es.SpecialData, 
				es.EncoderRPath, es.EncoderTempPath,
				es.Ep,
				title, operatingSystem,
				Util.StringArrayToString(Constants.EncoderEnglishWords,";"),
				Util.StringArrayToString(encoderTranslatedWordsOK,";"),
				debug, crossValidate, cutByTriggers, triggerStr, separateSessionInDays,
				analysisMode, inertialGraphX
				);
	}

	public static string RProcessBinURL()
	{
		string pBin="Rscript";
		if (UtilAll.IsWindows())
		{
			//on Windows we need the \"str\" to call without problems in path with spaces
			pBin = "\"" + System.IO.Path.Combine(Util.GetPrefixDir(), "bin" + Path.DirectorySeparatorChar + "Rscript.exe") + "\"";
		}
		else if(UtilAll.GetOSEnum() == UtilAll.OperatingSystems.MACOSX)
		{
			pBin = Constants.RScriptOSX;
		}

		LogB.Information("pBin:", pBin);
		return pBin;
	}


	//Inertia Momentum
	//TODO: make this work with encoderRProc
	public static void RunEncoderCalculeIM(double weight, double distance, EncoderRProcAnalyze encoderRProcAnalyze) 
	{
		encoderRProcAnalyze.CancelRScript = false;

		ProcessStartInfo pinfo;
	        Process p;
		//If output file is not given, R will try to write in the running folder
		//in which we may haven't got permissions
		
		string pBin="";
		pinfo = new ProcessStartInfo();

		string inputData = GetEncoderDataTempFileName();
		string outputData = GetEncoderSpecialDataTempFileName();
		
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
		}
		
		//--- way A. passing options to a file
		string scriptOptions = 
			inputData + "\n" + 
			outputData + "\n" + 
			Util.ConvertToPoint(weight) + "\n" + 
			Util.ConvertToPoint(distance) + "\n" +
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

			while ( ! ( File.Exists(outputFileCheck) || encoderRProcAnalyze.CancelRScript) );
		} catch {
		}
	}


	/* convert this
	 * 0, 0, 0, 0, 0, 0, 1, 0, 0, 1, 1, 2, 1
	 * to
	 * 0*6 1 0*2 1*2 2 1
	 * in order to be shorter if has to be sended by network
	 * this compression reduces size six times aproximately
	 */
	
	/*
	 * newlines help to send data to R (encoder) and read from there more safely
	 * valuesForNewLine is 25 means every 25 values there will be a newLine. 0 will mean no newlines
	 */
	
	/*
	 * valuesForNewLine means how many values want in a line. If it's >= []curve, then there will be only one line
	 * there's a test of this function at testing-stuff/compressCurve.cs
	 */
	public static string CompressData(double [] curve, int valuesForNewLine)
	{
		string compressed = "";
		
		bool start = true;
		int digit = -10000;
		int digitPre = -10000; //just an impossible mark
		int rep = 0;
		int countNewLine = 0;
		
		/*
		LogB.Information("Compressing curve");
		string debugStr = "";
		for(int i=0; i < curve.Length; i++) {
			debugStr += curve[i].ToString();
		}
		LogB.Debug(debugStr);
		*/

		for(int i=0; i < curve.Length; i++) 
		{
			digit = Convert.ToInt32(curve[i]);
			if(start) {
				rep ++;
				start = false;
				countNewLine ++;
			} else if(digit == digitPre)
				rep ++;
			else {
				if(rep == 1)
					compressed += digitPre.ToString() + " ";
				else {
					compressed += digitPre.ToString() + "*" + rep.ToString() + " ";
					rep = 1;
				}
				countNewLine ++;
			}

			if(valuesForNewLine > 0 && countNewLine >= valuesForNewLine) {
				compressed += "\n";
				countNewLine = 0;
			}

			digitPre = digit;
		}

		if(rep == 0)
			compressed += "";
		else if(rep == 1)
			compressed += digit.ToString();
		else
			compressed += digit.ToString() + "*" + rep.ToString();

		return compressed;
	}


	/* unused
	public static string CompressSignal(string fileNameSignal)
	{
		return CompressData(Util.ReadFile(fileNameSignal, false));
	}
	
	public static string CompressData(string contents)
	{
		string compressed = "";
		
		bool start = true;
		int digit = -10000;
		int digitPre = -10000; //just an impossible mark
		int rep = 0;
		for(int i=0; i < contents.Length; i++) 
		{
			if(! Char.IsDigit(contents[i]))
				continue;

			digit = contents[i] -'0'; 
			if(start) {
				rep ++;
				start = false;
			} else if(digit == digitPre)
				rep ++;
			else {
				if(rep == 1)
					compressed += digitPre.ToString() + " ";
				else {
					compressed += digitPre.ToString() + "*" + rep.ToString() + " ";
					rep = 1;
				}
			}

			digitPre = digit;
		}

		if(rep == 0)
			compressed += "";
		else if(rep == 1)
			compressed += digit.ToString();
		else
			compressed += digit.ToString() + "*" + rep.ToString();

		return compressed;
	}
	*/


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

		LogB.Information("EncoderSaveCurve start:" + start.ToString() + "; duration:" + duration.ToString());
		
			
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

		try {
			TextWriter writer = File.CreateText(fileCurveFull);
			writer.Write(contents);
			writer.Flush();
			writer.Close();
			((IDisposable)writer).Dispose();
		}
		catch {
			LogB.Error("Problems on EncoderSaveCurve with file: " + fileCurveFull);
			return "";
		}

		return fileCurve;
	}

	public static ArrayList EncoderConfigurationList(Constants.EncoderType encoderType, bool inertial, bool rotaryFrictionOnAxis) {
		ArrayList list = new ArrayList();
		if(encoderType == Constants.EncoderType.LINEAR) {
			if(! inertial) {
				list.Add(new EncoderConfiguration(
							Constants.EncoderConfigurationNames.LINEAR));
				list.Add(new EncoderConfiguration(
							Constants.EncoderConfigurationNames.LINEARINVERTED));
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
				list.Add(new EncoderConfiguration(
							Constants.EncoderConfigurationNames.LINEARONPLANEWEIGHTDIFFANGLEMOVPULLEY));
				list.Add(new EncoderConfiguration(
							Constants.EncoderConfigurationNames.PNEUMATIC));
			} else {
				list.Add(new EncoderConfiguration(
							Constants.EncoderConfigurationNames.LINEARINERTIAL));
			}
		} else if(encoderType == Constants.EncoderType.ROTARYFRICTION) {
			if(! inertial) {
				list.Add(new EncoderConfiguration(
							Constants.EncoderConfigurationNames.ROTARYFRICTIONSIDE));
				list.Add(new EncoderConfiguration(
							Constants.EncoderConfigurationNames.ROTARYFRICTIONAXIS));
				list.Add(new EncoderConfiguration(
							Constants.EncoderConfigurationNames.WEIGHTEDMOVPULLEYROTARYFRICTION));
			}
			else {
				if(! rotaryFrictionOnAxis) {
					list.Add(new EncoderConfiguration(
								Constants.EncoderConfigurationNames.ROTARYFRICTIONSIDEINERTIAL));
					list.Add(new EncoderConfiguration(
								Constants.EncoderConfigurationNames.ROTARYFRICTIONSIDEINERTIALLATERAL));
					list.Add(new EncoderConfiguration(
								Constants.EncoderConfigurationNames.ROTARYFRICTIONSIDEINERTIALMOVPULLEY));
				} else {
					list.Add(new EncoderConfiguration(
								Constants.EncoderConfigurationNames.ROTARYFRICTIONAXISINERTIAL));
					list.Add(new EncoderConfiguration(
								Constants.EncoderConfigurationNames.ROTARYFRICTIONAXISINERTIALLATERAL));
					list.Add(new EncoderConfiguration(
								Constants.EncoderConfigurationNames.ROTARYFRICTIONAXISINERTIALMOVPULLEY));
				}
			}
		} else if(encoderType == Constants.EncoderType.ROTARYAXIS) {
			if(! inertial) {
				list.Add(new EncoderConfiguration(
							Constants.EncoderConfigurationNames.ROTARYAXIS));
				list.Add(new EncoderConfiguration(
							Constants.EncoderConfigurationNames.WEIGHTEDMOVPULLEYROTARYAXIS));
			} else {
				list.Add(new EncoderConfiguration(
							Constants.EncoderConfigurationNames.ROTARYAXISINERTIAL));
				list.Add(new EncoderConfiguration(
							Constants.EncoderConfigurationNames.ROTARYAXISINERTIALLATERAL));
				list.Add(new EncoderConfiguration(
							Constants.EncoderConfigurationNames.ROTARYAXISINERTIALMOVPULLEY));
				list.Add(new EncoderConfiguration(
							Constants.EncoderConfigurationNames.ROTARYAXISINERTIALLATERALMOVPULLEY));
			}
		}
		return list;
	}

	public static int CalculeInertiaTotal (EncoderConfiguration econf) {
		//LogB.Debug(econf.ToString(":",false,false));
		int n = econf.extraWeightN;
		double weightInKg = econf.extraWeightGrams / 1000.0;
		double length = econf.extraWeightLength;

		//IM of all the weights = n * (weight * length^2) Kg*cm^2
		int im_weights = Convert.ToInt32(n * ( weightInKg * Math.Pow(length,2) ));

		return econf.inertiaMachine + im_weights;
	}

	public static int GetActiveCurvesNum(ArrayList curvesArray) {
		int countActiveCurves = 0;
		foreach(EncoderSQL es in curvesArray) 
			if(es.status == "active")
				countActiveCurves ++;
		
		return countActiveCurves;
	}
	
	public static List<int> GetActiveCheckboxesList(string [] checkboxes) {
		List<int> activeCurvesList = new List<int>();
		int i=0;
		foreach(string cb in checkboxes) {
			if(cb == "active")
				activeCurvesList.Add(i+1);
			i++;
		}
		return activeCurvesList;
	}
	


		

}
