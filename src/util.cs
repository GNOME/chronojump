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
 *  Copyright (C) 2004-2009   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
//using System.Data;
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using System.Diagnostics; 	//for detect OS
using System.IO; 		//for detect OS

//this class tries to be a space for methods that are used in different classes
public class Util
{
	//all numbers are saved in database with '.' as decimal separator (method for numbers)
	public static string ConvertToPoint (double myDouble)
	{
		StringBuilder myStringBuilder = new StringBuilder(myDouble.ToString());
		myStringBuilder.Replace(",", ".");
		return myStringBuilder.ToString();
	}
	
	//all numbers are saved in database with '.' as decimal separator
	//method for the tvString, tcString, and runIntervalTimesString
	public static string ConvertToPoint (string myString)
	{
		StringBuilder myStringBuilder = new StringBuilder(myString);
		myStringBuilder.Replace(",", ".");
		return myStringBuilder.ToString();
	}
	
	
	//used for load from the database all numbers with correct decimal separator (locale defined)
	//used also for the tvString, tcString, and runIntervalTimesString
	public static string ChangeDecimalSeparator(string myString) {
		if(myString == "") {
			return "0";
		}
		System.Globalization.NumberFormatInfo localeInfo = new System.Globalization.NumberFormatInfo();
		localeInfo = System.Globalization.NumberFormatInfo.CurrentInfo;
		
		StringBuilder myStringBuilder = new StringBuilder(myString);
		if(localeInfo.NumberDecimalSeparator != ".") {
			myStringBuilder.Replace(".", localeInfo.NumberDecimalSeparator);
		}
		return myStringBuilder.ToString();
	}

	public static string TrimDecimals (double time, int prefsDigitsNumber) {
		return Math.Round(time, prefsDigitsNumber).ToString();
	}

	public static string TrimDecimals (string time, int prefsDigitsNumber) {
		if(time == "-1") 
			return "-1";
		else if(time == "-") 
			return "-";
		else 
			return Math.Round(Convert.ToDouble(time), prefsDigitsNumber).ToString();
	}

	//if passed (number=1, digits=4)
	//takes 1 and returns "0001" 
	public static string DigitsCreate (int number, int digits)
	{
		string str = number.ToString();
		while(str.Length < digits)
			str = "0" + str;
		return str;
	}
	
	public static double GetMax (string values)
	{
		string [] myStringFull = values.Split(new char[] {'='});
		double max = 0;
		foreach (string jump in myStringFull) {
			if ( Convert.ToDouble(jump) > max ) {
				max = Convert.ToDouble(jump);
			}
		}
		return max ; 
	}
	
	//don't use if there are no jumps, then the big value 999999999 could return
	public static double GetMin (string values)
	{
		string [] myStringFull = values.Split(new char[] {'='});
		double min = 999999999999;
		foreach (string jump in myStringFull) {
			if ( Convert.ToDouble(jump) < min ) {
				min = Convert.ToDouble(jump);
			}
		}
		return min ; 
	}
	
	public static double GetAverage (string values)
	{
		if(values.Length == 0)
			return 0;
		string [] myStringFull = values.Split(new char[] {'='});
		double myAverage = 0;
		double myCount = 0;

		//if(myStringFull[0] == "-1") {
		//	return 0;
		//}
		foreach (string jump in myStringFull) {
			//if there's a -1 value, should not be counted in the averages
			if(Convert.ToDouble(jump) != -1) {
				myAverage = myAverage + Convert.ToDouble(jump);
				myCount ++;
			}
		}
		if (myAverage == 0 || myCount == 0) { return 0; } //fixes problems when processing only a -1
		else { return myAverage / myCount ; }
	}

	public static double GetLast (string values)
	{
		string [] myStringFull = values.Split(new char[] {'='});
		double lastSubEvent = 0;
		foreach (string myString in myStringFull) 
			lastSubEvent = Convert.ToDouble(myString);
			
		return lastSubEvent; 
	}
	
	public static int GetPosMax (string values)
	{
		string [] myStringFull = values.Split(new char[] {'='});
		double max = 0;
		int count = 0;
		int countMax = 0;
		foreach (string myEvent in myStringFull) {
			if ( Convert.ToDouble(myEvent) > max ) {
				max = Convert.ToDouble(myEvent);
				countMax = count;
			}
			count ++;
		}
		return countMax ; 
	}
	
	//don't use if there are no jumps, then the big value 999999999 could return
	public static int GetPosMin (string values)
	{
		string [] myStringFull = values.Split(new char[] {'='});
		double min = 999999999999;
		int count = 0;
		int countMin = 0;
		foreach (string myEvent in myStringFull) {
			if ( Convert.ToDouble(myEvent) < min ) {
				min = Convert.ToDouble(myEvent);
				countMin = count;
			}
			count ++;
		}
		return countMin ; 
	}

	
	public static double CalculateSD(string valuesList, double sumValues, int count) {
		if(count >1) {
			/*	  
			 * std = SQRT( Σ(Xi-Xavg)² /n )
			 * stdSample = SQRT(n / n-1) * std
			 */

			double avg = sumValues / count;
			double summatory = 0;
			string [] valuesListFull = valuesList.Split(new char[] {':'});
			
			for(int i=0; i<count; i++) {
				summatory += System.Math.Pow ( (Convert.ToDouble(valuesListFull[i]) - avg), 2);
			}

			/*
			 * things inside the sqrt have an "(Double)" for not being returned a truncated number (without comma). 
			 * Eg: 
			 * System.Math.Sqrt(10/9) = 1 
			 * System.Math.Sqrt(10/(Double)9) = 1,05409255338946
			 */
			
			double std = System.Math.Sqrt(summatory / (Double)count);
			double stdSample = System.Math.Sqrt( count/(Double)(count-1) ) * std;

			return stdSample;
		} else {
			return -1;
		}
	}
	
	//useful for jumpType and jumpRjType, because the third value is the same
	public static bool HasWeight(string [] jumpTypes, string myType) {
		foreach (string myString in jumpTypes) {
			string [] myStringFull = myString.Split(new char[] {':'});
			if(myStringFull[1] == myType) {
				if(myStringFull[3] == "1") { return true;
				} else { return false;
				}
			}
		}
		Log.WriteLine(string.Format("Error, myType: {0} not found", myType));
		return false;
	}
	
	//useful for jumpType and jumpRjType, because the second value is the same
	public static bool HasFall(string [] jumpTypes, string myType) {
		foreach (string myString in jumpTypes) {
			string [] myStringFull = myString.Split(new char[] {':'});
			if(myStringFull[1] == myType) {
				if(myStringFull[2] == "0") { return true;
				} else { return false;
				}
			}
		}
		Log.WriteLine(string.Format("Error, myType: {0} not found", myType));
		return false;
	}

	public static string RemoveTilde(string myString) 
	{
		StringBuilder myStringBuilder = new StringBuilder(myString);
		myStringBuilder.Replace("'", " ");
		return myStringBuilder.ToString();
	}
	
	public static string RemoveTildeAndColon(string myString) 
	{
		StringBuilder myStringBuilder = new StringBuilder(myString);
		myStringBuilder.Replace("'", " ");
		myStringBuilder.Replace(":", " ");
		return myStringBuilder.ToString();
	}
	
	//dot is used for separating sex in stats names (cannot be used for a new jumpType)
	public static string RemoveTildeAndColonAndDot(string myString) 
	{
		StringBuilder myStringBuilder = new StringBuilder(myString);
		myStringBuilder.Replace("'", " ");
		myStringBuilder.Replace(":", " ");
		myStringBuilder.Replace(".", " ");
		return myStringBuilder.ToString();
	}
	
	public static string RemoveTab(string myString) 
	{
		StringBuilder myStringBuilder = new StringBuilder(myString);
		myStringBuilder.Replace("\t", " ");
		return myStringBuilder.ToString();
	}

	public static string RemoveNewLine(string myString) 
	{
		StringBuilder myStringBuilder = new StringBuilder(myString);
		myStringBuilder.Replace("\n", " ");
		return myStringBuilder.ToString();
	}

	public static string RemoveZeroOrMinus(string myString) 
	{
		if(myString == "0" || myString == "-")
			return "";
		else
			return myString;
	}

	public static string ChangeEqualForColon(string myString) 
	{
		StringBuilder myStringBuilder = new StringBuilder(myString);
		myStringBuilder.Replace("=", ":");
		return myStringBuilder.ToString();
	}

	public static string GetHeightInCentimeters (string time) {
		// s = 4.9 * (tv/2)^2
		double timeAsDouble = Convert.ToDouble(time);
		double height = 100 * 4.9 * ( timeAsDouble / 2.0 ) * ( timeAsDouble / 2.0 ) ;

		return height.ToString();
	}
	
	public static double WeightFromKgToPercent (double jumpKg, double personKg) {
		return (double) jumpKg *100 / (double) personKg;
	}

	public static double WeightFromPercentToKg (double jumpPercent, double personKg) {
		return (double) jumpPercent * personKg / (double) 100;
	}

	public static int GetNumberOfJumps(string myString, bool countMinus)
	{
		if(myString.Length > 0) {
			string [] jumpsSeparated = myString.Split(new char[] {'='});
			int count = 0;
			foreach (string temp in jumpsSeparated) {
				if(countMinus || temp != "-1")
					count++;
			}
			if(count == 0) { count =1; }
			
			return count;
		} else { 
			return 0;
		}
	}
	
	public static double GetTotalTime (string stringTC, string stringTF)
	{
		if(stringTC.Length > 0 && stringTF.Length > 0) {
			string [] tc = stringTC.Split(new char[] {'='});
			string [] tv = stringTF.Split(new char[] {'='});

			double totalTime = 0;

			foreach (string jump in tc) {
				if(jump != "-1") 
					totalTime = totalTime + Convert.ToDouble(jump);
			}
			foreach (string jump in tv) {
				if(jump != "-1") 
					totalTime = totalTime + Convert.ToDouble(jump);
			}

			return totalTime ;
		} else {
			return 0;
		}
	}
	
	public static double GetTotalTime (string timeString)
	{
		try{
			if(timeString.Length > 0) {
				string [] time= timeString.Split(new char[] {'='});

				double totalTime = 0;

				foreach (string temp in time) 
					if(temp != "-1") 
						totalTime = totalTime + Convert.ToDouble(temp);

				return totalTime ;
			} else {
				return 0;
			}
		}
		//it seems in runInterval, sometimes intervalTimesString is not defined. Check this, now just return a 0, like if it's idefined but is 0-length
		catch {
			return 0;
		}
	}

	//we cannot count with GetNumberOfJumps because that method doesn't count the -1
	//here we want to know if there's more tc data than tv and remove this tc not needed
	//there's no need to record a las tc (currently)
	public static string DeleteLastTcIfNeeded (string tcString, string tvString)
	{
		string [] tcFull = tcString.Split(new char[] {'='});
		string [] tvFull = tvString.Split(new char[] {'='});

		if(tcFull.Length > tvFull.Length) {
			int lastEqualPos = tcString.LastIndexOf('=');
			return tcString.Substring(0, lastEqualPos -1);
		} else {
			return tcString;
		}
	}
				
	
	public static string [] DeleteFirstStrings(string [] str, int maxStrings) {
		string [] str2 = new String [maxStrings];
		for(int i=str.Length - maxStrings, j=0; i < str.Length; i++, j++) 
			str2[j] = str [i];
		return str2;
	}
	
	public static string [] DeleteString(string [] initial, string delete) {
		string [] final = new String [initial.Length -1];
		int count = 0;
		foreach(string str in initial)
			if(str != delete)
				final[count++] = str;
		return final;
	}
	
	
	//called from jumpRj.Write() and from interval
	//when we mark that jump should finish by time, chronopic thread is probably capturing data
	//check if it captured more than date limit, and if it has done, delete last(s) jump(s)
	//also have in mind that allowFinishAfterTime exist
	public static bool EventPassedFromMaxTime(
			string tcString, string tvString, double timeLimit, bool allowFinishAfterTime) 
	{
		if(Util.GetTotalTime(tcString, tvString) > timeLimit) {
			if(allowFinishAfterTime) {
				//if allowFinishAfterTime, know if there's one event with part of the tv after time (ok) or more (bad)
				if(Util.GetTotalTime(tcString, tvString) - Util.GetLast(tvString) > timeLimit)
					return true;	//eventsTime are higher than timeLimit and allowFinish... 
							//and without the lastTv it exceeds, then one ore more exceeds 
				else 
					return false;	//eventsTime are higher than timeLimit and allowFinish... 
							//but without the lastTv no exceeds, then no problem
			} 
			else
				return true;		//eventsTime are higher than timeLimit and !allowFinish... one ore more exceeds 
		}
		else
			return false;			//eventsTime are lower than timeLimit: no problem
	}

	//also for runInterval (simple and without allowFinish...
	public static bool EventPassedFromMaxTime(
			string timesString, double timeLimit) 
	{
		if(Util.GetTotalTime(timesString) > timeLimit) 
			return true;	//eventsTime are higher than timeLimit: one ore more exceeds 
		else
			return false;	//eventsTime are lower than timeLimit: no problem
	}
	
	public static string DeleteFirstSubEvent (string myString)
	{
		int firstEqualPos = myString.IndexOf('=');
		if(firstEqualPos > 0) {
			return myString.Substring(firstEqualPos +1);
		} else
			return myString;
	}

	
	public static string DeleteLastSubEvent (string myString)
	{
		int lastEqualPos = myString.LastIndexOf('=');
		if(lastEqualPos > 0) {
			return myString.Substring(0, lastEqualPos);
		}
		else
			//probably string has only one subEvent, then we cannot delete last
			//imagine a runInterval where we only have 10 seconds for go, return, go... n times. And imagine, going is like 20 seconds, then 
			//runInterval write will try to delete last subEvent, but this is the only one
			//then return the time in negative (-) as a mark, and caller will show the time late in a popup win
			return "-" + myString;
	}


	//public static string GetSpeed (string distance, string time) {
	public static string GetSpeed (string distance, string time, bool metersSecondsPreferred) {
		double distanceAsDouble = Convert.ToDouble(distance);
		double timeAsDouble = Convert.ToDouble(time);

		if(metersSecondsPreferred)
			return (distanceAsDouble / timeAsDouble).ToString();
		else
			return (3.6 * distanceAsDouble / timeAsDouble).ToString();
	}
					
	
	public static int FetchID (string text)
	{
		if (text.Length == 0) {
			return -1;
		}
		string [] myStringFull = text.Split(new char[] {':'});

		for (int i=0; i < myStringFull[0].Length; i++)
			    {
				    if( ! Char.IsNumber(myStringFull[0], i)) {
					    return -1;
				    }
			    }
		return Convert.ToInt32(myStringFull[0]);
	}
	
	public static string FetchName (string text)
	{
		//"id: name" (return only name)
		bool found = false;
		int i;
		for (i=0; ! found ; i++) {
			if(text[i] == ':') {
				found = true;
			}
		}
		return text.Substring(i);
	}

	public static string GetInitialSpeed (string time, bool metersSecondsPreferred) 
	{
		double height = Convert.ToDouble( GetHeightInCentimeters(time) );
		height = height / 100; //in meters
		
		// Vo = sqrt(2gh)
		double initialSpeed = System.Math.Sqrt ( 2 * 9.81 * height ); 

		if(! metersSecondsPreferred)
			initialSpeed *= 3.6;

		return initialSpeed.ToString();
	}
	
	public static double GetDjPower (double tc, double tf) 
	{
		//relative potency in Watts/Kg
		//Bosco. Pendent to find if published

		//P = 24.6 * (TotalTime + FlightTime) / ContactTime

		double tt = tc + tf; //totalTime

		return 24.06 * ( tt * tf ) / (Double)tc;
	}


	public static double GetQIndex (double tv, double tc) 
	{
		if(tv == 0 || tc == 0)
			return 0;
		
		if(tv == -1 || tc == -1)
			return 0;
		
		return tv/tc;
	}

	public static double GetDjIndex (double tv, double tc) 
	{
		if(tv == 0 || tc == 0)
			return 0;
		
		if(tv == -1 || tc == -1)
			return 0;
		
		return 100 * (tv-tc)/tc;
	}

	public static string GetReportDirectoryName (string fileName) {
		//gets exportfile.html or exportfile.htm and returns exportfile_files
		int posOfDot = fileName.LastIndexOf('.');
		string directoryName = fileName.Substring(0,posOfDot);
		directoryName += "_files";
		return directoryName;
	}

	public static string GetLastPartOfPath (string fileName) {
		//gets a complete url with report directory path and return only last part of path
		//useful for linking images as relative and not absolute in export to HTML
		//works on win and linux
		int temp1 = fileName.LastIndexOf('\\');
		int temp2 = fileName.LastIndexOf('/');
		int posOfBar = 0;
		if(temp1>temp2)
			posOfBar = temp1;
		else
			posOfBar = temp2;

		string lastPartOfPath = fileName.Substring(posOfBar+1, fileName.Length - posOfBar -1);
		return lastPartOfPath;
	}

	//gets a string and returns if all the chars are numbers or the decimal point in current localization
	//there should be also only one decimal point
	//method made because i didn't find it in mono
	//ATTENTTION ONLY WORKS FOR POSITIVES
	//before changing this method, better create another method for all numbers, 
	//and call that method on possible negative numbers
	public static bool IsNumber(string myString, bool canBeDecimal) {
		System.Globalization.NumberFormatInfo localeInfo = new System.Globalization.NumberFormatInfo();
		localeInfo = System.Globalization.NumberFormatInfo.CurrentInfo;
		
		int countDecimals = 0;
		foreach(char myChar in myString) {
			if( ! System.Char.IsNumber(myChar) && myChar.ToString() != localeInfo.NumberDecimalSeparator ) {
				return false;
			}
			if( myChar.ToString() == localeInfo.NumberDecimalSeparator ) {
				countDecimals ++;
			}
		}
		if(countDecimals > 0 && !canBeDecimal) { return false; }
		if(countDecimals > 1) { return false; }

		//false if it's blank, or it's only a decimal "."
		if(myString.Length == 0 || (myString.Length == 1 && countDecimals == 1)) { 
			return false; }
				
		return true;
	}

	//Adapted from Mono. A developer's notebook. p 244
	
	//this is used in chronojump for working with the ports,
	//in chronojump we compile now for Linux with Mono and for Windows with .NET
	//it should be something like IsDotNet()
	public static bool IsWindows() {
		string os = GetOS();
		if(os.ToUpper().StartsWith("WIN"))
			return true;
		else 
			return false;
	}
	
	public static string GetOS() {
		OperatingSystem os = Environment.OSVersion;
		string osString =  string.Format("{0}, {1}", os.Platform, os.Version);
		return osString;
	}
	
	public static string GetReallyOldDatabaseDir() {
		return Environment.GetEnvironmentVariable("HOME")+ Path.DirectorySeparatorChar + ".chronojump";
	}
	
	public static string GetOldDatabaseDir() {
		//we are on:
		//Chronojump/chronojump-x.y/data/
		//we have to go to
		//Chronojump/database/
		
		return ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "database";
	}
	
	public static string GetApplicationDataDir() {
		return Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"Chronojump");
	}
	
	public static string GetDatabaseDir() {
		//fixing:
		//http://lists.ximian.com/pipermail/mono-list/2008-November/040480.html
		return Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"Chronojump" + Path.DirectorySeparatorChar + "database");
	}
	
	//if database dir has illegal characters, use this temp dir and remember to copy db at end, or to restore if chrashed
	public static string GetDatabaseTempDir() {
		return Path.Combine(Path.GetTempPath(), "Chronojump");
	}

	public static string GetManualDir() {
		//we are on:
		//lib/chronojump/ (Unix) or bin/ (win32)
		//we have to go to
		//share/doc/chronojump
		return System.IO.Path.Combine(Util.GetPrefixDir(),"share/doc/chronojump");
	}

	public static string GetPrefixDir(){
		string runningFolder = System.AppDomain.CurrentDomain.BaseDirectory;
		if (Environment.OSVersion.Platform == PlatformID.Win32NT)	
			return System.IO.Path.Combine(runningFolder,"../");
		else 
			return System.IO.Path.Combine(runningFolder,"../../");
	}

	private static string getDataDir(){
		return System.IO.Path.Combine(GetPrefixDir(),"share/chronojump");
	}

	public static string GetImagesDir(){
		return System.IO.Path.Combine(getDataDir(),"images");
	}

	public static string GetCssDir(){
		return getDataDir();
	}
	
	public static void BackupDirCreateIfNeeded () {
		string backupDir = GetDatabaseDir() + Path.DirectorySeparatorChar + "backup";
		if( ! Directory.Exists(backupDir)) {
			Directory.CreateDirectory (backupDir);
			Log.WriteLine ("created backup dir");
		}
	}

	public static void BackupDatabase () {
		string homeDir = GetDatabaseDir();
		string backupDir = homeDir + "/backup";
		
		string dateParsed = UtilDate.ToFile(DateTime.Now);

		if(File.Exists(System.IO.Path.Combine(homeDir, "chronojump.db")))
			File.Copy(System.IO.Path.Combine(homeDir, "chronojump.db"), 
				System.IO.Path.Combine(backupDir, "chronojump_" + dateParsed + ".db"));
		else {
			Log.WriteLine("Error, chronojump.db file doesn't exist!");
		}
	}

	public static void RunRScript(string rScript){
		ProcessStartInfo pinfo;
        Process r;
		string rBin="R";
		//If output file is not given, R will try to write in the running folder
		//in which we may haven't got permissions
		string outputFile = rScript+".Rout";
		
		if (File.Exists(outputFile))
			File.Delete(outputFile);
 
		if (IsWindows())
			rBin=System.IO.Path.Combine(GetPrefixDir(), "bin/R.exe");

		pinfo = new ProcessStartInfo();
		pinfo.FileName=rBin;
		pinfo.Arguments ="CMD BATCH --no-save " + rScript +" " + outputFile;
		pinfo.CreateNoWindow = true;
		pinfo.UseShellExecute = false;
		
		r = new Process();
		r.StartInfo = pinfo;
		r.Start();
		r.WaitForExit();
		while (!File.Exists(outputFile));				
	}
/*
 * currently not used, we copy the assemblies now
 *
	public static void CopyArchivesOninstallation(string fileName) {
		string homeDir = GetDatabaseDir();
		//copy files, and continue if already exists or if origin file doesn't exist
		try {
			File.Copy(fileName , homeDir + "/" + fileName );
		} catch {}
	}
*/
	
	//public static void PlaySound (System.Media.SystemSounds mySound, bool volumeOn) {
	public static void PlaySound (Constants.SoundTypes mySound, bool volumeOn) {
		if(volumeOn) {
			//TODO: this try/catch still doesn't work in my laptop with sound problems
			try {
				switch(mySound) {
					case Constants.SoundTypes.CAN_START:
						System.Media.SystemSounds.Question.Play();
						break;
					case Constants.SoundTypes.GOOD:
						System.Media.SystemSounds.Asterisk.Play();
						break;
					case Constants.SoundTypes.BAD:
						System.Media.SystemSounds.Beep.Play();
						break;
				}
			} catch {}
		}
	}

	/* LANGUAGES STUFF */
	public static string GetLanguageCode(string languageString) {
		string [] myStringFull = languageString.Split(new char[] {':'});
		return myStringFull[0];
	}

	public static string GetLanguageName(string languageString) {
		string [] myStringFull = languageString.Split(new char[] {':'});
		return myStringFull[1];
	}
	
	public static string GetLanguageNameFromCode(string languageCode) {
		foreach (string lang in Constants.Languages) {
			if (languageCode == GetLanguageCode(lang)) {
				return GetLanguageName(lang);
			}
		}
		//if there's an error:
		return GetLanguageName(Constants.LanguageDefault);
	}
		
	public static string GetLanguageCodeFromName(string languageName) {
		foreach (string lang in Constants.Languages) {
			if (languageName == GetLanguageName(lang)) {
				return GetLanguageCode(lang);
			}
		}
		//if there's an error:
		return GetLanguageCode(Constants.LanguageDefault);
	}
		
	public static string [] GetLanguagesCodes() {
		string [] codes = new string[Constants.Languages.Length];
		int count = 0;
		foreach (string lang in Constants.Languages) 
			codes[count++] = GetLanguageCode(lang);

		return codes;
	}
		
	public static string [] GetLanguagesNames() {
		string [] names = new string[Constants.Languages.Length];
		int count = 0;
		foreach (string lang in Constants.Languages) 
			names[count++] = GetLanguageName(lang);
		
		return names;
	}

	public static string GetImagePath(bool mini) {
		string returnString = "";
		if (Util.IsWindows()) {
			if (mini) {
				returnString = Constants.ImagesMiniWindows;
			} else {
				returnString = Constants.ImagesWindows;
			}
		} else {
			if (mini) {
				returnString = Constants.ImagesMiniLinux;
			} else {
				returnString = Constants.ImagesLinux;
			}
		}
		return returnString;
	}
		
	public static string GetGladePath() {
		if (Util.IsWindows())
			return Constants.GladeWindows;
		else
			return Constants.GladeLinux;
	}

		
	//do this for showing the Limited with selected decimals and without loosing the end letter: 'J' or 'T'
	//called by treeview_jump, treeview_run and gui/jump_edit and gui/run_edit?
	public static string GetLimitedRounded(string limitedString, int pDN) {
		string myLimitedWithoutLetter = limitedString.Substring(0, limitedString.Length -1);
		string myLimitedLetter = limitedString.Substring(limitedString.Length -1, 1);

		return TrimDecimals(myLimitedWithoutLetter, pDN) + myLimitedLetter;
	}

	public static string [] AddArrayString(string [] initialString, string [] addString) {
		string [] returnString = new string[initialString.Length + addString.Length];
		int i;
		int j;
		for (i=0 ; i < initialString.Length; i ++)
			returnString[i] = initialString[i];
		for (j=0 ; j < addString.Length; j ++)
			returnString[i+j] = addString[j];

		return returnString;
	}

	public static string [] ArrayListToString (ArrayList myArrayList) {
		//if myArrayList is not defined, return with an empty string
		try { 
			string [] myString = new String[myArrayList.Count];
			int i=0;
			foreach (string str in myArrayList) 
				myString[i++] = str;
		
			return myString;
		}
		catch {
			string [] myString = new String[0];
			return myString;
		}
	}
			
	public static string ArrayListToSingleString (ArrayList myArrayList) {
		string myString = "";
		foreach (string str in myArrayList) 
			myString += str + "\n";

		return myString;
	}
			
	public static ArrayList AddToArrayListIfNotExist(ArrayList myArrayList, string str) {
	 	bool found = FoundInArrayList(myArrayList, str);
		if(!found)
			myArrayList.Add(str);

		return myArrayList;
	}

	public static bool FoundInArrayList(ArrayList myArrayList, string str) {
	 	bool found = false;
		foreach (string str2 in myArrayList)
			if(str2 == str)
				found = true;

		return found;
	}



	/*
	//delete a row of and arraylist of string[] if the string[0] is the value coming from startsWith
	public static ArrayList DeleteFromArrayList(ArrayList firstArrayList, string startsWith, char delimited) {
		ArrayList secondArrayList = new ArrayList();
		foreach (string str2 in firstArrayList) {
			string [] strFull = str2.Split(new char[] {delimited});
			if(strFull[0] != startsWith)
				secondArrayList.Add(str2);
		}

		return secondArrayList;
	}
*/
	public static string StringArrayToString (string [] myFullString, string separator) {
		string uniqueString = "";
		bool firstValue = true;
		foreach (string myStr in myFullString) {
			if(firstValue)
				uniqueString += myStr;
			else
				uniqueString += separator + myStr;
			firstValue = false;
		}
		return uniqueString;
	}
	
	//to create an string [] of one member
	public static string [] StringToStringArray (string str) {
		string [] ret = new string[1];
		ret[0] = str;
		return ret;
	}
	
	public static ArrayList StringToArrayList (string str, char sep) {
		ArrayList array = new ArrayList(1);
		string [] strFull = str.Split(new char[] {sep});
		for (int i=0; i < strFull.Length ; i++) {
			array.Add(strFull[i]);
		}
		return array;
	}



	public static bool IntToBool (int myInt) {
		if(myInt == 1)
			return true;
		else
			return false;
	}

	public static bool StringToBool (string myString) {
		if(myString.ToUpper() == "TRUE")
			return true;
		else
			return false;
	}

	public static string BoolToInOut (bool areWeIn) {
		if(areWeIn)
			return Constants.In;
		else
			return Constants.Out;
	}

	public static int BoolToInt (bool myBool) {
		if(myBool)
			return 1;
		else
			return 0;
	}

	//used by simulated, since cj 0.8.1.2, db: 0.60
	//-1 simulated test
	//0 real test not uploaded
	//>0 serverUniqueID of uploaded test
	public static int BoolToNegativeInt (bool myBool) {
		if(myBool)
			return -1;
		else
			return 0;
	}

	public static string FindLevelName(int levelInt) {
		string foundLevelName = "";
		foreach(string level in Constants.Levels)
			if(FetchID(level) == levelInt)
				foundLevelName = FetchName(level);

		return foundLevelName;
	}

	/* eg we have an stringArray containing in a row "Letonia, Republica de" and we want to find ID
	 * 2:Latvia, Republic of:Letonia, Republica de
	 * we do string myString = Util.FindOnArray(':', 2, 0, "Letonoa, Republica de", stringArray);
	 */
	public static string FindOnArray(char separator, int partPassed, int partToReturn, string stringPassed, string [] stringArray) 
	{
		string foundString = "";
		foreach(string myString in stringArray) {
			string [] myStrFull = myString.Split(new char[] {separator});
			if(myStrFull[partPassed] == stringPassed)
				foundString = myStrFull[partToReturn];

		}
		return foundString;
	}


	//avoids divide by zero
	//thought for being between 0, 1
	//ideal for progressBars
	public static double DivideSafeFraction (double val1, double val2) {
		double result = val1 / val2;
		if(result > 1)
			result = 1;
		else if(result < 0)
			result = 0;
		return result;
	}

	/*
	//converts all values to positive
	public static string StringValuesAbsolute (string myString) {
		return myString.Trim('-');
	}
*/
	
	public static string DetectPortsLinux(bool formatting) {
		string startStr = "";
		string midStr = "\n";
		string endStr = "";
		if(formatting) {
			startStr = "<i>";
			midStr = "\t";
			endStr = "</i>";
		}
		string detected = "";
		string [] usbSerial = Directory.GetFiles("/dev/", "ttyUSB*");
		if(usbSerial.Length > 0) {
			detected += "\n" + Constants.FoundUSBSerialPortsString + " " + usbSerial.Length + "\n" + startStr;
			foreach(string myPort in usbSerial)
				detected += midStr + myPort;
			detected += endStr;
		} else {
			detected += Constants.NotFoundUSBSerialPortsString + "\n";
			string [] serial = Directory.GetFiles("/dev/", "ttyS*");
			detected += Constants.FoundSerialPortsString + " " + serial.Length + "\n" + startStr;
			foreach(string myPort in serial)
				detected += midStr + myPort;
			detected += endStr;
		}
		return detected;
	}

	/*
	  tests:
	  -1 simulated
	  0 real not uplaoded
	  >0 uploaded, num is the uniqueID on server
	 */

	public static string SimulatedTestNoYes(int num) {
		if(num < 0)
			return Constants.Yes;
		else
			return Constants.No;
	}


	/* 
	 * when distances are variable on run interval 
	 */
	//thought for values starting by 0
	public static double GetRunIVariableDistancesStringRow(string distancesString, int row) {
		string [] str = distancesString.Split(new char[] {'-'});
		row = row % str.Length;
		return Convert.ToDouble(str[row]);
	}
	
	public static double GetRunIVariableDistancesDistanceDone(string distancesString, int tracks) {
		double distanceTotal = 0;
		for(int i=0; i < tracks; i++) 
			distanceTotal += GetRunIVariableDistancesStringRow(distancesString, i);
		return distanceTotal;
	}

	//decides if it's variable or not
	public static double GetRunITotalDistance(double distanceInterval, string distancesString, double tracks) {
		if(distanceInterval == -1) 
			return GetRunIVariableDistancesDistanceDone(distancesString, (int) tracks);
		else
			return tracks * distanceInterval;
	}

	public static double GetRunIVariableDistancesSpeeds(string distancesString, string timesString, bool max) {
		double  searchedValue = -1; //to find max values (higher than this)
		if(! max)
			searchedValue = 1000; //to find min values (lower than this)

		string [] times = timesString.Split(new char[] {'='});
		string [] distances = distancesString.Split(new char[] {'-'});
		for(int i=0; i < times.Length; i++) {
			double time = Convert.ToDouble(times[i]);
		
			int distPos = i % times.Length;
			double distance = Convert.ToDouble(distances[distPos]);

			double speed = distance / time;
			if(max) {
				if(speed > searchedValue) 
					searchedValue = speed;
			} else {
				if(speed < searchedValue)
					searchedValue = speed;
			}
		}
		return searchedValue;
	}

	public static string SQLBuildString (string tableName, string test, string variable,
			int sex, string ageInterval,
			int countryID, int sportID, int speciallityID, int levelID)
	{
		string strSelect = "SELECT COUNT(" + variable + "), AVG(" + variable + ")";
		string strFrom   = " FROM " + tableName;
		string strWhere  = " WHERE " + tableName + ".type = '" + test + "'";

		string strSex = "";
		if(sex == Constants.MaleID) 
			strSex = " AND person.sex == '" + Constants.M + "'";
		else if (sex == Constants.FemaleID) 
			strSex = " AND person.sex == '" + Constants.F + "'";

		string strAge = "";
		if(ageInterval != "") {
			strFrom += ", session";
			string [] strFull = ageInterval.Split(new char[] {':'});
			strAge = " AND (julianday(session.date) - julianday(person.dateBorn))/365.25 " + 
				strFull[0] + " " + strFull[1];
			if(strFull.Length == 4)
				strAge += " AND (julianday(session.date) - julianday(person.dateBorn))/365.25 " + 
					strFull[2] + " " + strFull[3];
			strAge += " AND " + tableName + ".sessionID = session.uniqueID";
		}

		string strCountry = "";
		if(countryID != Constants.CountryUndefinedID) 
			strCountry = " AND person.countryID == " + countryID;

		string strSport = "";
		if(sportID != Constants.SportUndefinedID) 
			strSport = " AND person.sportID == " + sportID;

		string strSpeciallity = "";
		if(speciallityID != Constants.SpeciallityUndefinedID) 
			strSpeciallity = " AND person.speciallityID == " + speciallityID;
		
		string strLevel = "";
		if(levelID != Constants.LevelUndefinedID) 
			strLevel = " AND person.practice == " + levelID;

		string strLast = "";
		if(strSex.Length > 0 || strAge.Length > 0 || 
				strCountry.Length > 0 || strSport.Length > 0 || strSpeciallity.Length > 0 || strLevel.Length > 0) {
			strFrom += ", person";
			strLast = " AND " + tableName + ".personID == person.UniqueID";
		}	
		return strSelect + strFrom + strWhere + strSex + strAge
			+ strCountry + strSport + strSpeciallity + strLevel + strLast;

	}

}
