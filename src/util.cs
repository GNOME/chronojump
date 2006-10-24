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
 * Xavier de Blas: 
 * http://www.xdeblas.com, http://www.deporteyciencia.com (parleblas)
 */

using System;
//using System.Data;
using System.Text; //StringBuilder

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


	public static string TrimDecimals (string time, int prefsDigitsNumber) {
		//the +2 is a workarround for not counting the two first characters: "0."
		//this will not work with the fall
		if(time == "-1") {
			return "-";
		} else {
			/*
			 * this doesn't work fine for values like 1234,4 because it counts the 1234
			 * this is ONLY suitable for values starting with "0."
			return time.Length > prefsDigitsNumber + 2 ? 
				time.Substring( 0, prefsDigitsNumber + 2 ) : 
					time;
					*/
			System.Globalization.NumberFormatInfo localeInfo = new System.Globalization.NumberFormatInfo();
			localeInfo = System.Globalization.NumberFormatInfo.CurrentInfo;
		
			int countIntegers = 0;
			bool decimalPointFound = false;
			foreach(char myChar in time) {
				if( myChar.ToString() == localeInfo.NumberDecimalSeparator ) {
					decimalPointFound = true;
					break;
				} else {
					countIntegers ++;
				}
			}
			
			if(decimalPointFound) {
				return time.Length > countIntegers + 1 + prefsDigitsNumber ? 
					time.Substring( 0, countIntegers + 1 + prefsDigitsNumber ) : 
						time;
			} else {
				return time;
			}
			
		}
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
		Console.WriteLine("Error, myType: {0} not found", myType);
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
		Console.WriteLine("Error, myType: {0} not found", myType);
		return false;
	}

	public static string RemoveTilde(string myString) 
	{
		StringBuilder myStringBuilder = new StringBuilder(myString);
		myStringBuilder.Replace("'", "");
		return myStringBuilder.ToString();
	}
	
	public static string RemoveTildeAndColon(string myString) 
	{
		StringBuilder myStringBuilder = new StringBuilder(myString);
		myStringBuilder.Replace("'", "");
		myStringBuilder.Replace(":", "");
		return myStringBuilder.ToString();
	}
	
	//dot is used for separating sex in stats names (cannot be used for a new jumpType)
	public static string RemoveTildeAndColonAndDot(string myString) 
	{
		StringBuilder myStringBuilder = new StringBuilder(myString);
		myStringBuilder.Replace("'", "");
		myStringBuilder.Replace(":", "");
		myStringBuilder.Replace(".", "");
		return myStringBuilder.ToString();
	}

	public static string GetHeightInCentimeters (string time) {
		// s = 4.9 * (tv/2)^2
		double timeAsDouble = Convert.ToDouble(time);
		double height = 100 * 4.9 * ( timeAsDouble / 2 ) * ( timeAsDouble / 2 ) ;

		return height.ToString();
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

				foreach (string temp in time) {
					totalTime = totalTime + Convert.ToDouble(temp);
				}

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
	
	public static string DeleteLastSubEvent (string myString)
	{
		int lastEqualPos = myString.LastIndexOf('=');
		return myString.Substring(0, lastEqualPos -1);
	}


	public static string GetSpeed (string distance, string time) {
		double distanceAsDouble = Convert.ToDouble(distance);
		double timeAsDouble = Convert.ToDouble(time);

		return (distanceAsDouble / timeAsDouble).ToString();
	}
					
	
	public static string FetchID (string text)
	{
		if (text.Length == 0) {
			return "-1";
		}
		string [] myStringFull = text.Split(new char[] {':'});

		for (int i=0; i < myStringFull[0].Length; i++)
			    {
				    if( ! Char.IsNumber(myStringFull[0], i)) {
					    return "-1";
				    }
			    }
		return myStringFull[0];
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

	public static string GetInitialSpeed (string time) 
	{
		double height = Convert.ToDouble( GetHeightInCentimeters(time) );
		height = height / 100; //in meters
		
		// Vo = sqrt(2gh)
		double initialSpeed = System.Math.Sqrt ( 2 * 9.81 * height ); 

		return initialSpeed.ToString();
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

	//gets a string and returns if all the chars are numbers or the decimal point in current localization
	//there should be also only one decimal point
	//method made because i didn't find it in mono
	public static bool IsNumber(string myString) {
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
		OperatingSystem os = Environment.OSVersion;

		Console.WriteLine("platform: {0}, version: {1}", os.Platform, os.Version);

		if(os.Platform.ToString().ToUpper().StartsWith("WIN"))
			return true;
		else 
			return false;
	}
	
	public static DateTime DateAsDateTime (string dateString) {
		string [] dateFull = dateString.Split(new char[] {'/'});
		DateTime dateTime;

		try {
			//Datetime (year, month, day) constructor
			dateTime = new DateTime(
					Convert.ToInt32(dateFull[2]), 
					Convert.ToInt32(dateFull[1]), 
					Convert.ToInt32(dateFull[0]));
		}
		catch {
			//Datetime (year, month, day) constructor
			//in old chronojump versions, date was recorded in a different way, solve like this
			dateTime = new DateTime(
					Convert.ToInt32(dateFull[2]), 
					Convert.ToInt32(dateFull[0]), 
					Convert.ToInt32(dateFull[1]));
		}
		return dateTime;
	}

	public static string GetHomeDir() {
		return Environment.GetEnvironmentVariable("HOME")+"/.chronojump";
	}
	
	public static void BackupDirCreateIfNeeded () {
		string backupDir = GetHomeDir() + "/backup";
		if( ! Directory.Exists(backupDir)) {
			Directory.CreateDirectory (backupDir);
			Console.WriteLine ("created backup dir");
		}
	}
	
	public static void BackupDatabase () {
		string homeDir = GetHomeDir();
		string backupDir = homeDir + "/backup";
		
		StringBuilder myStringBuilder = new StringBuilder(DateTime.Now.ToString());
		myStringBuilder.Replace("/", "-"); //replace the '/' for '-' for not having problems with the directories
		
		if(File.Exists(homeDir + "/chronojump.db"))
			File.Copy(homeDir + "/chronojump.db", backupDir + "/chronojump_" + myStringBuilder + ".db");
		else {
			Console.WriteLine("Error, chronojump.db file doesn't exist!");
		}
	}
	
}
