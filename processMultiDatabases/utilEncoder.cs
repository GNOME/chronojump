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
 * Copyright (C) 2019   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.IO; 		//for detect OS

public class UtilEncoder
{
	public static EncoderGraphROptions PrepareEncoderGraphOptions(
			string title, EncoderStruct es, bool neuromuscularProfileDo, bool translate, bool debug, bool crossValidate,
			bool cutByTriggers, string triggerStr, bool separateSessionInDays, EncoderGraphROptions.AnalysisModes analysisMode)
	{
		string operatingSystem = OperatingSystemForRGraphs();
		
		/*	
		title = Util.RemoveBackSlash(title);
		title = Util.RemoveChar(title, '\''); 
		*/
		title = "hola";
	
		/*	
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
		*/
		
	 	//if translators add ";", it will be converted to ','
	 	//if translators add a "\n", it will be converted to " "
		/*
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
			*/

		return new EncoderGraphROptions( 
				es.InputData, es.OutputGraph, es.OutputData1, 
				//es.OutputData2, es.SpecialData, 
				es.EncoderRPath, es.EncoderTempPath,
				es.Ep,
				title, operatingSystem,
				"hola2", // Util.StringArrayToString(Constants.EncoderEnglishWords,";"),
				"hola3", // Util.StringArrayToString(encoderTranslatedWordsOK,";"),
				debug, crossValidate, cutByTriggers, triggerStr, separateSessionInDays, analysisMode
				);
	}

	public static string OperatingSystemForRGraphs()
	{
		string operatingSystem = "Linux";
		//if (UtilAll.IsWindows())
		//	operatingSystem = "Windows";

		return operatingSystem;
	}
	
	public static string GetEncoderDataTempFileName() {
		return Path.Combine(Path.GetTempPath(), Constants.EncoderDataTemp);
	}
	public static string GetEncoderCurvesTempFileName() {
		return Path.Combine(Path.GetTempPath(), Constants.EncoderCurvesTemp);
	}
	public static string GetEncoderGraphTempFileName() {
		return Path.Combine(Path.GetTempPath(), Constants.EncoderGraphTemp);
	}
	public static string GetEncoderScriptsPathWithoutLastSep()
	{
		/*
		string s = System.IO.Path.Combine(Util.GetDataDir(), "encoder");

		//but send it without the final '\' or '/' (if found)
		if(s.EndsWith("/") || s.EndsWith("\\"))
			s = s.Substring(0, s.Length -1);

		return s;
		*/
		//harcoded:
		return "/usr/local/lib/chronojump/../../share/chronojump/encoder";
	}
	public static string GetEncoderTempPathWithoutLastSep() {
		string s = Path.GetTempPath(); //is just temp path

		//but send it without the final '\' or '/' (if found)
		if(s.EndsWith("/") || s.EndsWith("\\"))
			s = s.Substring(0, s.Length -1);

		return s;
	}

	public static bool CopyEncoderDataToTemp(string url, string fileName)
	{
		string origin = url + Path.DirectorySeparatorChar + fileName;
		string dest = GetEncoderDataTempFileName();
		if(File.Exists(origin)) {
			try {
				File.Copy(origin, dest, true);
			} catch {
				Console.WriteLine(Constants.FileCopyProblemStr());
				return false;
			}
			return true;
		}

		Console.WriteLine(Constants.FileNotFoundStr());
		return false;
	}

}
