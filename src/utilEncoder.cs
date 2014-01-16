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
 *  Copyright (C) 2004-2012   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
//using System.Data;
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using System.Diagnostics; 	//for detect OS
using System.IO; 		//for detect OS

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
				Log.WriteLine (string.Format("created dir: {0}", d));
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
				Log.WriteLine (string.Format("created dir: {0}", d));
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
	public static string GetEncoderStatusTempFileName() {
		return Path.Combine(Path.GetTempPath(), Constants.EncoderStatusTemp);
	}
	public static string GetEncoderExportTempFileName() {
		return Path.Combine(Path.GetTempPath(), Constants.EncoderExportTemp);
	}
	public static string GetEncoderSpecialDataTempFileName() {
		return Path.Combine(Path.GetTempPath(), Constants.EncoderSpecialDataTemp);
	}


//	public static void MoveTempToEncoderData(int sessionID, int uniqueID) {
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
				
				File.Copy(GetEncoderDataTempFileName(), 
						GetEncoderSessionDataSignalDir(sessionID) + 
						Path.DirectorySeparatorChar + fileName, true);
//			}
		}
		return fileName;
	}
	
	public static bool CopyEncoderDataToTemp(string url, string fileName)
	{
		string origin = url + Path.DirectorySeparatorChar + fileName;
		string dest = GetEncoderDataTempFileName();
		if(File.Exists(origin)) {
			File.Copy(origin, dest, true);
			return true;
		}
		return false;
	}
	
	
	private static string getEncoderScriptCapture() {
		if(UtilAll.IsWindows())
			return System.IO.Path.Combine(Util.GetPrefixDir(), 
				"bin" + Path.DirectorySeparatorChar + "encoder", Constants.EncoderScriptCaptureWindows);
		else
			return System.IO.Path.Combine(
					Util.GetDataDir(), "encoder", Constants.EncoderScriptCaptureLinux);
	}
	
	private static string getEncoderScriptGraph() {
		return System.IO.Path.Combine(
				Util.GetDataDir(), "encoder", Constants.EncoderScriptGraph);
	}
	
	
	/********** end of encoder paths ************/

	private static string changeSpaceToSpaceMark(string myString) 
	{
		StringBuilder myStringBuilder = new StringBuilder(myString);
		myStringBuilder.Replace(" ", "WINDOWSSPACEMARK");
		return myStringBuilder.ToString();
	}

	
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
		

		/*
		on Windows (py2exe) we execute a exe with the py file that contains python
		on linux we execute python and call to the py file
		also on windows we need the full path to find R
		*/
		if (UtilAll.IsWindows()) {
			pBin=getEncoderScriptCapture();
			pinfo.Arguments = title + " " + es.OutputData1 + " " + es.Ep.ToString1() + " " + port 
				+ " " + changeSpaceToSpaceMark(
					System.IO.Path.Combine(Util.GetPrefixDir(), "bin" + Path.DirectorySeparatorChar + "R.exe"));
		}
		else {
			pBin="python";
			pinfo.Arguments = getEncoderScriptCapture() + " " + title + " " + 
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
	
	public static bool RunEncoderGraph(string title, EncoderStruct es) 
	{
		CancelRScript = false;

		ProcessStartInfo pinfo;
	        Process p;
		//If output file is not given, R will try to write in the running folder
		//in which we may haven't got permissions
		
		string pBin="";
		pinfo = new ProcessStartInfo();

		string operatingSystem = "Linux";
			
		pBin="Rscript";
		//pBin="R";
		if (UtilAll.IsWindows()) {
			//on Windows we need the \"str\" to call without problems in path with spaces
			pBin = "\"" + System.IO.Path.Combine(Util.GetPrefixDir(), "bin" + Path.DirectorySeparatorChar + "Rscript.exe") + "\"";
			Log.WriteLine("pBin:" + pBin);

			//On win32 R understands backlash as an escape character and 
			//a file path uses Unix-like path separator '/'		
			es.InputData = es.InputData.Replace("\\","/");
			es.OutputGraph = es.OutputGraph.Replace("\\","/");
			es.OutputData1 = es.OutputData1.Replace("\\","/");
			es.OutputData2 = es.OutputData2.Replace("\\","/");
			es.SpecialData = es.SpecialData.Replace("\\","/");
			operatingSystem = "Windows";
		}
		
		//--- way A. passing options to a file
		string scriptOptions = es.InputData + "\n" + 
		es.OutputGraph + "\n" + es.OutputData1 + "\n" + 
		es.OutputData2 + "\n" + es.SpecialData + "\n" + 
		es.Ep.ToString2("\n") + "\n" + title + "\n" + operatingSystem + "\n";

		string optionsFile = Path.GetTempPath() + "Roptions.txt";
		TextWriter writer = File.CreateText(optionsFile);
		writer.Write(scriptOptions);
		writer.Flush();
		((IDisposable)writer).Dispose();
		
		if (UtilAll.IsWindows()) {
			//On win32 R understands backlash as an escape character and 
			//a file path uses Unix-like path separator '/'		
			optionsFile = optionsFile.Replace("\\","/");
		}
		
		//on Windows we need the \"str\" to call without problems in path with spaces
		pinfo.Arguments = "\"" + getEncoderScriptGraph() + "\" " + optionsFile;
	
		Log.WriteLine("Arguments:" + pinfo.Arguments);
		
		/*
		pinfo.Arguments = "CMD BATCH --no-save '--args optionsFile=\"" + optionsFile + "\"' \"" + 
			getEncoderScriptGraph() + "\" \"" + 
			Path.GetTempPath() + "error.txt\"";
			*/
		
		//--- way B. put options as arguments
		/*
		string argumentOptions = es.InputData + " " + 
			es.OutputGraph + " " + es.OutputData1 + " " + es.OutputData2 + " " + 
			es.Ep.ToString2(" ") + " " + title;
		
		pinfo.Arguments = getEncoderScriptGraph() + " " + argumentOptions;
		*/

		Log.WriteLine("------------- 1 ---");
		Log.WriteLine(optionsFile.ToString());
		Log.WriteLine("------------- 2 ---");
		Log.WriteLine(scriptOptions.ToString());
		Log.WriteLine("------------- 3 ---");
		Log.WriteLine(pinfo.Arguments.ToString());
		Log.WriteLine("------------- 4 ---");
		
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

		//delete output file check(s)
		Console.WriteLine("Deleting... " + outputFileCheck);
		if (File.Exists(outputFileCheck))
			File.Delete(outputFileCheck);

		if(outputFileCheck2 != "") {
			Console.WriteLine("Deleting... " + outputFileCheck2);
			if (File.Exists(outputFileCheck2))
				File.Delete(outputFileCheck2);
		}
			
		//delete 1RM data if exists
		if (File.Exists(es.SpecialData))
			File.Delete(es.SpecialData);

		try {	
			p = new Process();
			p.StartInfo = pinfo;
			p.Start();
			p.WaitForExit();

			if(outputFileCheck2 == "")
				while ( ! ( File.Exists(outputFileCheck) || CancelRScript) );
			else
				while ( ! ( (File.Exists(outputFileCheck) && File.Exists(outputFileCheck2)) || CancelRScript ) );
		} catch {
			return false;
		}

		return true;
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
					if(contents[i-1] == '-')
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
				if(startPos > 0 && digits == start + duration) 
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

	public static void EncoderDeleteCurveFromSignal(string fileName, int start, int duration) {
		string contents = Util.ReadFile(fileName, false);
		string [] startAndDuration = encoderFindPos(contents, start, duration);

		StringBuilder myStringBuilder = new StringBuilder(contents);
		myStringBuilder.Remove(
				Convert.ToInt32(startAndDuration[0]),
				Convert.ToInt32(startAndDuration[1]));
		contents = myStringBuilder.ToString();
		
		TextWriter writer = File.CreateText(fileName);
		writer.Write(contents);
		writer.Flush();
		((IDisposable)writer).Dispose();
	}

	public static string EncoderSaveCurve(string fileNameSignal, int start, int duration, 
			int sessionID, int uniqueID, string personName, string timeStamp, int curveIDMax) 
	{
		string contents = Util.ReadFile(fileNameSignal, false);
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
		
		TextWriter writer = File.CreateText(fileCurveFull);
		writer.Write(contents);
		writer.Flush();
		((IDisposable)writer).Dispose();

		return fileCurve;
	}

	public static int EncoderModeConversions(int byteReaded, string encoderMode, int inertiaMomentum, double diameter) {
		//invert sign if inverted is selected
		if(encoderMode == Constants.EncoderSignalMode.LINEARINVERTED.ToString())
			byteReaded *= -1;

		return byteReaded;
	}	

}
