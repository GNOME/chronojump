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
	
	public static string ConvertToComma (string myString)
	{
		StringBuilder myStringBuilder = new StringBuilder(myString);
		myStringBuilder.Replace(".", ",");
		return myStringBuilder.ToString();
	}
	
	public static string DoubleToCSV(double d, string CSVsepString) {
		string s = d.ToString();
		if(CSVsepString == "COMMA")
			return ConvertToComma(s);
		else
			return ConvertToPoint(s);
	}
		


	//when we do a query to the server, it returns avg as "0,54" because it's latin localized
	//if client is on english machine, need to convert this to "0.54"
	public static string ConvertToPointIfNeeded (string myString)
	{
		System.Globalization.NumberFormatInfo localeInfo = new System.Globalization.NumberFormatInfo();
		localeInfo = System.Globalization.NumberFormatInfo.CurrentInfo;
		
		StringBuilder myStringBuilder = new StringBuilder(myString);
		if(localeInfo.NumberDecimalSeparator == ".") {
			myStringBuilder.Replace(",", localeInfo.NumberDecimalSeparator);
		}
		return myStringBuilder.ToString();
	}
	
	//this is only used to define a preferences configuration
	//to send to R and export csv,  gui/encoder.cs uses CSVExportDecimalSeparator variable that is read from preferences SQL
	public static string GetDecimalSeparatorFromLocale() {
		System.Globalization.NumberFormatInfo localeInfo = new System.Globalization.NumberFormatInfo();
		localeInfo = System.Globalization.NumberFormatInfo.CurrentInfo;

		if(localeInfo.NumberDecimalSeparator == ".")
			return("POINT");
		else if(localeInfo.NumberDecimalSeparator == ",")
			return("COMMA");
		else 
			return("OTHER");
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
				summatory += Math.Pow ( (Convert.ToDouble(valuesListFull[i]) - avg), 2);
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

	public static int CalculateJumpAngle(double height, int distance) {
		if(distance == 0)
			return 90;
		else
			return Convert.ToInt32(System.Math.Atan(height / (distance * 1.0)) 
				* 180 / System.Math.PI);
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
		LogB.Error(string.Format("Error, myType: {0} not found", myType));
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
		LogB.Error(string.Format("Error, myType: {0} not found", myType));
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
	//also recomended:
	//name = Util.RemoveChar(name, '"');
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

	public static string RemoveNewLine(string myString, bool changeBySpace) 
	{
		StringBuilder myStringBuilder = new StringBuilder(myString);
		if(changeBySpace)
			myStringBuilder.Replace("\n", " ");
		else
			myStringBuilder.Replace("\n", "");
		return myStringBuilder.ToString();
	}

	//needed for encoder R files
	public static string RemoveBackSlash(string myString)
	{
		StringBuilder myStringBuilder = new StringBuilder(myString);
		myStringBuilder.Replace("\\", " ");
		return myStringBuilder.ToString();
	}

	//use this!!!	
	public static string RemoveChar(string s, char c) 
	{
		StringBuilder myStringBuilder = new StringBuilder(s);
		myStringBuilder.Replace(c,' ');
		return myStringBuilder.ToString();
	}

	public static string RemoveZeroOrMinus(string myString) 
	{
		if(myString == "0" || myString == "-")
			return "";
		else
			return myString;
	}
	
	public static string RemoveMarkup(string s) 
	{
		bool done = false;
		while(! done) {
			int tagStart = s.IndexOf('<');
			int tagEnd = s.IndexOf('>');
			if(tagStart != -1 && tagEnd != -1 && tagEnd > tagStart) 
				s = s.Remove(tagStart, tagEnd-tagStart+1);
			else
				done = true;
		}
		return s;
	}

	public static string ChangeEqualForColon(string myString) 
	{
		StringBuilder myStringBuilder = new StringBuilder(myString);
		myStringBuilder.Replace("=", ":");
		return myStringBuilder.ToString();
	}

	public static string ChangeSpaceAndMinusForUnderscore(string myString) 
	{
		StringBuilder myStringBuilder = new StringBuilder(myString);
		myStringBuilder.Replace(" ", "_");
		myStringBuilder.Replace("-", "_");
		return myStringBuilder.ToString();
	}
	
	public static string ChangeChars(string str, string charIni, string charEnd) 
	{
		StringBuilder myStringBuilder = new StringBuilder(str);
		myStringBuilder.Replace(charIni, charEnd);
		return myStringBuilder.ToString();
	}

	
	public static string GetHeightInCentimeters (string time) {
		// s = 4.9 * (tv/2)^2
		double timeAsDouble = Convert.ToDouble(time);
		double height = 100 * 4.9 * Math.Pow( timeAsDouble / 2.0 , 2 );

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

	public static string [] DeleteStringAtPosition(string [] initial, int pos) {
		string [] final = new String [initial.Length -1];
		int count = 0;
		foreach(string str in initial) {
			if(count != pos)
				final[count] = str;
			count ++;
		}

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
		//Absolute value because timesString can start with a '-' if run limited by time and
		//first subRun has arrived later than maximum for the whole run
		if(Math.Abs(Util.GetTotalTime(timesString)) > timeLimit) 
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
		
			//but maybe the mark has added first because this was called on tempTable iteration and later on final SQL write iteration
			if( ! myString.StartsWith("-") )
				myString = "-" + myString;
				
			return myString;
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

	public static double GetStiffness(double personMassInKg, double extraMass, double tv, double tc) 
	{
		//LogB.Warning("AT GetStiffness");
		//LogB.Warning("1: " + personMassInKg.ToString() +  " 2: " + extraMass.ToString() + " 3: " + tv.ToString() + " 4: " + tc.ToString());
		double totalMass = personMassInKg + extraMass;
		
		//return if mass is zero or there's no contact time
		if( totalMass == 0 || tv <= 0 || tc <= 0)
			return 0;

		double stiffness;
		try {
			stiffness = totalMass * Math.PI * ( tv + tc ) / ( Math.Pow(tc,2) * ( (tv+tc)/Math.PI - tc/4 ) );
		} catch { 
			return 0;
		}

		return stiffness;
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
	
	public static double GetDjPower (double tc, double tf, double mass, double fallHeight) 
	{
		/*
		 * old method
		//relative potency in Watts/Kg
		//Bosco. Pendent to find if published

		//P = 24.6 * (TotalTime + FlightTime) / ContactTime

		double tt = tc + tf; //totalTime

		return 24.6 * ( tt + tf ) / (Double)tc;
		*/

		//new method (proposal by Xavier Padullés)
		//Calcule the potential energies before (mass * g * fallHeight) and after the jump (mass * g * tv^2 * 1.226)
		//and divide by the time during force is applied
		double g = 9.81;
		fallHeight = fallHeight / 100.0; //cm -> m
		
		return mass * g * ( fallHeight + 1.226 * Math.Pow(tf,2) ) / (Double)tc;
	}
				
	//only Lewis now
	public static double GetPower (double tf, double bodyWeight, double extraWeightKg) 
	{
		//Log.WriteLine("tf: " + tf + ", bodyWeight: " + bodyWeight + ", extra: " + extraWeightKg);
		double pw = System.Math.Sqrt ( 4.9 ) * 9.8 * (bodyWeight + extraWeightKg) *
			System.Math.Sqrt(
				       Convert.ToDouble(GetHeightInCentimeters(tf.ToString()))/100);
		//Log.WriteLine("pw: " + pw);
		return pw;

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
	
	public static void IsNumberTests() {
		LogB.Information("tryParse -23 as int: " + Util.IsNumber("-23",false).ToString()); 	//catalan (True)
		LogB.Information("tryParse -23.75 as int: " + Util.IsNumber("-23.75",false).ToString()); 	//catalan (False)
		LogB.Information("tryParse -23,75 as int: " + Util.IsNumber("-23,75",false).ToString()); 	//catalan (False)
		
		LogB.Information("tryParse -23.75 as double: " + Util.IsNumber("-23.75",true).ToString()); //catalan (False)
		LogB.Information("tryParse -23,75 as double: " + Util.IsNumber("-23,75",true).ToString()); //catalan (True)
		
		LogB.Information("tryParse '' as int: " + Util.IsNumber("",false).ToString()); 		//catalan (True)
		LogB.Information("tryParse '' as double: " + Util.IsNumber("",true).ToString()); 	//catalan (True)
		
		LogB.Information("tryParse 'joan' as int: " + Util.IsNumber("joan",false).ToString()); 	//catalan (True)
		LogB.Information("tryParse 'joan' as double: " + Util.IsNumber("joan",true).ToString()); //catalan (True)
	}

	//gets a string and returns if all the chars are numbers or the decimal point in current localization
	public static bool IsNumber(string str, bool canBeDecimal) 
	{
		//false if it's blank
		if(str.Length == 0)
			return false;
		
		if(canBeDecimal) {
			double numD;
			//param 2 && 3 are needed on latin languages to achieve a negative result on "23.75"
			//without those params, "23.75" and "23,75" will be true on latin. Undesired
			if (double.TryParse(
						str,
						System.Globalization.NumberStyles.Float,	
						System.Globalization.NumberFormatInfo.CurrentInfo,	
						out numD))
				return true;
		}

		int numI;	
		if (int.TryParse(str, out numI))
			return true;

		return false;
	}

	public static bool IsEven(string myString) {
		return IsEven(Convert.ToInt32(myString));
	}
	public static bool IsEven(int myInt) {
		return (myInt % 2 == 0); //check if it's even (in spanish "par")
	}
	
	/********** start of LocalApplicationData path ************/
	
	//parent of database, multimedia and encoder
	//if withFinalSeparator, then return a '\' or '/' at the end
	public static string GetParentDir(bool withFinalSeparator) {
		string path = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"Chronojump");

		if(withFinalSeparator)
			path += Path.DirectorySeparatorChar;

		return path;
	}
	
	/********** end of LocalApplicationData path ************/
	
	/********** start of database paths ************/

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
	
	/********** end of database paths ************/

	/*	
	public static string GetChronojumpNetworksFile() {
		return Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"Chronojump" + Path.DirectorySeparatorChar + "chronojump_networks.txt");
	}
	*/
	

	/********** start of multimedia paths ************/

	public static string GetMultimediaDir() {
		return Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"Chronojump" + Path.DirectorySeparatorChar + "multimedia");
	}
	
	public static string GetPhotosDir(bool small) {
		string smallDir = "";
		if(small)
			smallDir = Path.DirectorySeparatorChar + Constants.SmallPhotoDir; 

		return Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"Chronojump" + Path.DirectorySeparatorChar + "multimedia" +
				Path.DirectorySeparatorChar + "photos") + smallDir;
	}
	
	public static string GetVideosDir() {
		return Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"Chronojump" + Path.DirectorySeparatorChar + "multimedia" +
				Path.DirectorySeparatorChar + "videos");
	}
	

	//to store user videos and photos
	public static void CreateMultimediaDirsIfNeeded () {
		string [] dirs = { GetMultimediaDir(), GetPhotosDir(false), GetPhotosDir(true), GetVideosDir() }; 
		foreach (string d in dirs) {
			if( ! Directory.Exists(d)) {
				Directory.CreateDirectory (d);
				LogB.Information ("created dir:", d);
			}
		}
	}
	
	//videos ar organized by sessions. Photos no.	
	public static string GetVideoSessionDir (int sessionID) {
		return GetVideosDir() + Path.DirectorySeparatorChar + sessionID.ToString();
	}
	
	public static void CreateVideoSessionDirIfNeeded (int sessionID) {
		string sessionDir = GetVideoSessionDir(sessionID);
		if( ! Directory.Exists(sessionDir)) {
			Directory.CreateDirectory (sessionDir);
			LogB.Information ("created dir:", sessionDir);
		}
	}

	//returns absolute path, but in encoder this URL is stored in database as relative to be able to move data between computers
	//see SqliteEncoder.removeURLpath
	public static string GetVideoFileName (int sessionID, Constants.TestTypes testType, int uniqueID) {
		return GetVideoSessionDir(sessionID) + Path.DirectorySeparatorChar + 
			testType.ToString() + "-" + uniqueID.ToString() +
			GetMultimediaExtension(Constants.MultimediaItems.VIDEO);
	}
	public static string GetVideoFileNameOnlyName (Constants.TestTypes testType, int uniqueID) {
		return testType.ToString() + "-" + uniqueID.ToString() +
			GetMultimediaExtension(Constants.MultimediaItems.VIDEO);
	}
	public static string GetVideoFileNameOnlyFolder (int sessionID) {
		return GetVideoSessionDir(sessionID);
	}
	
	public static string GetPhotoFileName (bool small, int uniqueID) {
		return GetPhotosDir(small) + Path.DirectorySeparatorChar + uniqueID.ToString() +
			GetMultimediaExtension(Constants.MultimediaItems.PHOTO);
	}
	
	public static string GetVideoTempFileName() {
		return Path.Combine(
				Path.GetTempPath(), Constants.VideoTemp + 
				GetMultimediaExtension(Constants.MultimediaItems.VIDEO));
	}
	
	public static string GetPhotoTempFileName(bool small) {
		string fileName = Constants.PhotoTemp;
		if(small)
			fileName = Constants.PhotoSmallTemp;

		return Path.Combine(
				Path.GetTempPath(), fileName + GetMultimediaExtension(Constants.MultimediaItems.PHOTO));
	}
	
	public static string GetMultimediaExtension (Constants.MultimediaItems multimediaItem) {
		if(multimediaItem == Constants.MultimediaItems.VIDEO)
			return Constants.ExtensionVideo;
		else //multimediaItem = Constants.MultimediaItems.PHOTO
			return Constants.ExtensionPhoto;
	}
			
	public static bool CopyTempVideo(int sessionID, Constants.TestTypes type, int uniqueID) {
		string origin = GetVideoTempFileName();
		string destination = GetVideoFileName(sessionID, type, uniqueID);
		if(File.Exists(origin)) {
			CreateVideoSessionDirIfNeeded(sessionID);
			/*
			 * no more move it, just copy it, because maybe is still being recorded
			try {
				File.Move(origin, destination);
			} catch {
			*/
				File.Copy(origin, destination, true); //can be overwritten
			//}
			return true;
		} else
			return false;
	}
	
	public static void DeleteVideo(int sessionID, Constants.TestTypes type, int uniqueID) {
		string fileName = GetVideoFileName(sessionID, type, uniqueID);
		if(File.Exists(fileName)) 
			File.Delete(fileName);
	}

	/********** end of multimedia paths ************/
	

	/********** start of encoder paths ************/

	public static string GetEncoderExportTempFileName() {
		return Path.Combine(Path.GetTempPath(), "encoder_set_export.csv");
	}

	public static string GetEncoderTriggerFileName() {
		return Path.Combine(Path.GetTempPath(), "encoder_trigger.txt");
	}
	public static string GetEncoderTriggerDateTimeFileName() {
		return Path.Combine(Path.GetTempPath(), "encoder_trigger_" + DateTime.Now.ToString() + ".txt");
	}

	/********** end of encoder paths ************/


	/********** start of rfid paths ************/

	public static string GetRFIDCaptureScript() {
		return Path.Combine(GetPrefixDir(), "bin/chronojump_rfid_capture.py");
	}
	public static string GetRFIDCapturedFile() {
		return Path.Combine(Path.GetTempPath(), "chronojump_rfid.txt");
	}


	/********** end of rfid paths ************/


	public static string GetManualDir() {
		//we are on:
		//lib/chronojump/ (Unix) or bin/ (win32)
		//we have to go to
		//share/doc/chronojump
		return System.IO.Path.Combine(Util.GetPrefixDir(),
			"share" + Path.DirectorySeparatorChar + "doc" + Path.DirectorySeparatorChar + "chronojump");
	}

	public static string GetPrefixDir(){
		string baseDirectory = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "..");
		if (! Directory.Exists(Path.Combine(baseDirectory, "lib" + Path.DirectorySeparatorChar + "chronojump"))) {
			baseDirectory = System.IO.Path.Combine(baseDirectory, "..");
		}
		return baseDirectory;
	}

	public static string GetDataDir(){
		return System.IO.Path.Combine(GetPrefixDir(),
			"share" + Path.DirectorySeparatorChar + "chronojump");
	}

	public static string GetImagesDir(){
		return System.IO.Path.Combine(GetDataDir(),"images");
	}

	public static string GetCssDir(){
		return GetDataDir();
	}

	//currently sounds are inside images folder	
	public static string GetSoundsDir(){
		return GetImagesDir();
	}
	
	
	public static void BackupDirCreateIfNeeded () {
		string backupDir = GetDatabaseDir() + Path.DirectorySeparatorChar + "backup";
		if( ! Directory.Exists(backupDir)) {
			Directory.CreateDirectory (backupDir);
			LogB.Information ("created backup dir");
		}
	}

	public static void BackupDatabase () {
		string homeDir = GetDatabaseDir();
		string backupDir = homeDir + Path.DirectorySeparatorChar + "backup";
		
		string dateParsed = UtilDate.ToFile(DateTime.Now);

		if(File.Exists(System.IO.Path.Combine(homeDir, "chronojump.db")))
			File.Copy(System.IO.Path.Combine(homeDir, "chronojump.db"), 
				System.IO.Path.Combine(backupDir, "chronojump_" + dateParsed + ".db"));
		else {
			LogB.Error("Error, chronojump.db file doesn't exist!");
		}
	}

	//http://stackoverflow.com/a/58779	
	public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target) {
		foreach (DirectoryInfo dir in source.GetDirectories())
			CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
		foreach (FileInfo file in source.GetFiles())
			file.CopyTo(Path.Combine(target.FullName, file.Name));
	}

	public static bool FileDelete(string fileName) 
	{
		LogB.Information("Checking if this filename exists: " + fileName);
		try {
			if(File.Exists(fileName)) {
				LogB.Information("Deleting " + fileName + " ...");
				File.Delete(fileName);
				LogB.Information("Deleted");
				return true;
			}
		} catch {
			LogB.Error("Problem deleting");
		}
		return false;
	}
	
	public static bool FileMove(string path, string filenameOrigin, string filenameDestination) {
		try {
			File.Move(
					path + Path.DirectorySeparatorChar + filenameOrigin, 
					path + Path.DirectorySeparatorChar + filenameDestination
					);
			return true;
		} catch {}
		return false;
	}
	
	public static bool FileExists(string fileName){
		return File.Exists(fileName);
	}
	
	public static bool FileReadable(string filename)
	{
		//http://stackoverflow.com/a/17318735
		try {
			File.Open(filename, FileMode.Open, FileAccess.Read).Dispose();
			//LogB.Information("success at Util.FileReadable: " + filename);
			return true;
		}
		catch (IOException) {
			System.Threading.Thread.Sleep(10);
			return false;
		}
	}

	//not recommended, better use below method. Better for bigger files
	public static string ReadFile(string fileName, bool removeEOL)
	{
		try {
			StreamReader reader = File.OpenText(fileName);
			string contents = reader.ReadToEnd ();
			reader.Close();
			
			//delete the '\n' that ReaderToEnd() has put
			if(removeEOL)
				contents = contents.TrimEnd(new char[1] {'\n'});
			
			return contents;
		} catch {
			return null;
		}
	}
	//recommended method
	public static List<string> ReadFileAsStringList(string fileName)
	{
		try {
			List<string> lines = new List<string>();
			using (var sr = new StreamReader(fileName))
			{
				while (sr.Peek() >= 0)
				{
					lines.Add(sr.ReadLine());
				}
			}
			return(lines);
		} catch {
			return null;
		}
	}


	//returns int [] of encoder signal or curve
	//currently used on db conversion 1.05 -> 1.06
	public static int [] ReadFileAsInts(string fileName)
	{
		string contents;
		try {
			StreamReader reader = File.OpenText(fileName);
			contents = reader.ReadToEnd ();
			reader.Close();
		} catch {
			return new int [] {};
		}
	
		//create ints file bigger enought to hold all possible values 	
		int [] ints = new int [contents.Length];
		int count = 0;
		for(int i=0; i < contents.Length; i++) {
			if(Char.IsDigit(contents[i])) {
				ints[count] = Convert.ToInt32(contents[i]);
				if(i > 0 && contents[i-1] == '-')
					ints[count] *= -1;
				
				count ++;
			}
		}
		
		//create int [] with the needed sized
		int [] intsCut = new int [count];
		for(int i=0; i < count; i ++)
			intsCut[i] = ints[i];

		return intsCut;
	}


	public static void RunRScript(string rScript){
		//CancelRScript = false;

		ProcessStartInfo pinfo;
	        Process r;
		string rBin="R";
		//If output file is not given, R will try to write in the running folder
		//in which we may haven't got permissions
		string outputFile = rScript+".Rout";
		
		if (File.Exists(outputFile))
			File.Delete(outputFile);
 
		if (UtilAll.IsWindows())
			rBin=System.IO.Path.Combine(GetPrefixDir(), "bin/R.exe");
		else if(UtilAll.GetOSEnum() == UtilAll.OperatingSystems.MACOSX)
			rBin = Constants.ROSX;

		pinfo = new ProcessStartInfo();
		pinfo.FileName=rBin;
		pinfo.Arguments ="CMD BATCH --no-save " + rScript +" " + outputFile;
		pinfo.CreateNoWindow = true;
		pinfo.UseShellExecute = false;
	
		try {	
			r = new Process();
				r.StartInfo = pinfo;
				r.Start();
			r.WaitForExit();
			//while ( ! ( File.Exists(outputFile) || CancelRScript) );
			while ( ! ( File.Exists(outputFile) ) );
		} catch {
			//maybe R is not installed
		}
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


	/*
	 * ------------- sound stuff -----------
	 */

	public static void PlaySound (Constants.SoundTypes mySound, bool volumeOn) {
		if ( ! volumeOn )
			return;
		
		/*
		 * Using GstreamerMethod because .Net method makes crash some Linux ALSA,
		 * and some MacOSX users have 300% CPU
		 */

		if( UtilAll.GetOSEnum() == UtilAll.OperatingSystems.LINUX ||
				UtilAll.GetOSEnum() == UtilAll.OperatingSystems.MACOSX )
			playSoundGstreamer(mySound);
		else //Windows
			playSoundWindows(mySound);
	}
	
	private static void playSoundGstreamer (Constants.SoundTypes mySound) 
	{
		string fileName = "";
		switch(mySound) {
			case Constants.SoundTypes.CAN_START:
				fileName = "123804__kendallbear__kendallklap1.wav";
				//author: kendallbear
				//https://www.freesound.org/people/kendallbear/sounds/123804/
				break;
			case Constants.SoundTypes.GOOD:
				fileName = "135936__bradwesson__collectcoin.wav";
				//author: bradwesson
				//https://www.freesound.org/people/bradwesson/sounds/135936/
				break;
			case Constants.SoundTypes.BAD:
				fileName = "142608__autistic-lucario__error.wav";
				//author: Autistic Lucario
				//https://www.freesound.org/people/Autistic%20Lucario/sounds/142608/
				break;
		}

		fileName = Util.GetSoundsDir() + Path.DirectorySeparatorChar + fileName;

		if(! File.Exists(fileName)) {
			LogB.Warning("Cannot found this sound file: " + fileName);
			return;
		}

		try {
			ProcessStartInfo pinfo = new ProcessStartInfo();
			string pBin="gst-launch-0.10";

			pinfo.FileName=pBin;
			pinfo.Arguments = "playbin2 " + @"uri=file://" + fileName;
			LogB.Information("Arguments:", pinfo.Arguments);
			pinfo.CreateNoWindow = true;
			pinfo.UseShellExecute = false;

			Process p = new Process();
			p.StartInfo = pinfo;
			p.Start();
		} catch {
			LogB.Error("Cannot playSoundGstreamer");
		}
	}

	//maybe in the future this method will be deprecated and it only will be used the Gstreamer method
	private static void playSoundWindows (Constants.SoundTypes mySound) 
	{
		try {
			switch(mySound) {
				case Constants.SoundTypes.CAN_START:
					System.Media.SystemSounds.Question.Play();
					break;
				case Constants.SoundTypes.GOOD:
					System.Media.SystemSounds.Asterisk.Play();
					break;
				case Constants.SoundTypes.BAD:
					//System.Media.SystemSounds.Beep.Play();
					System.Media.SystemSounds.Hand.Play();
					break;
			}
		} catch {
			LogB.Error("Cannot playSoundWindows");
		}
	}
	
	/*
	 * ------------- end of sound stuff -----------
	 */



	public static string GetImagePath(bool mini) {
		string returnString = "";
		if (UtilAll.IsWindows()) {
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
		if (UtilAll.IsWindows())
			return Constants.GladeWindows;
		else
			return Constants.GladeLinux;
	}

		
	//do this for showing the Limited with selected decimals and without loosing the end letter: 'J' or 'T'
	//called by treeview_jump, treeview_run and gui/jump_edit and gui/run_edit?
	public static string GetLimitedRounded(string limitedString, int pDN)
	{
		LogB.Information("GetLimitedRounded limitedString pdN");
		LogB.Information(limitedString);
		LogB.Information(pDN.ToString());

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

	//bool firstOrLast: true means first
	public static string [] AddArrayString(string [] initialString, string addString, bool firstOrLast) {
		string [] returnString = new string[initialString.Length + 1];

		int i;
		if(firstOrLast) {
			returnString[0] = addString;
			
			for (i=0 ; i < initialString.Length; i ++)
				returnString[i+1] = initialString[i];
		} else {
			for (i=0 ; i < initialString.Length; i ++)
				returnString[i] = initialString[i];

			returnString[i] = addString;
		}

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
			
	public static string ArrayListToSingleString (ArrayList myArrayList, string sep) {
		string myString = "";
		string sepUsed = "";
		foreach (string str in myArrayList) { 
			myString += sepUsed + str;
			sepUsed =  sep;
		}

		return myString;
	}
			
	public static ArrayList AddToArrayListIfNotExist(ArrayList myArrayList, string str) {
	 	bool found = FoundInArrayList(myArrayList, str);
		if(!found)
			myArrayList.Add(str);

		return myArrayList;
	}
	
	public static ArrayList AddToArrayListIfNotExist(ArrayList myArrayList, double d) {
	 	bool found = FoundInArrayList(myArrayList, d);
		if(!found)
			myArrayList.Add(d);

		return myArrayList;
	}
	
	public static List<double> AddToListDoubleIfNotExist(List<double> l, double d) {
	 	bool found = FoundInListDouble(l, d);
		if(!found)
			l.Add(d);
	
		return l;
	}

	public static bool FoundInArrayList(ArrayList a, string str) {
		foreach (string str2 in a)
			if(str2 == str)
				return true;

		return false;
	}

	public static bool FoundInArrayList(ArrayList a, int i) {
		foreach (int j in a)
			if(j == i)
				return true;

		return false;
	}

	public static bool FoundInArrayList(ArrayList a, double d) {
		foreach (double d2 in a)
			if(d2 == d)
				return true;

		return false;
	}
	
	public static bool FoundInStringArray(string [] a, string str) {
		foreach (string str2 in a)
			if(str2 == str)
				return true;

		return false;
	}

	public static bool FoundInListDouble(List<double>l, double d) {
		foreach (double d2 in l)
			if(d2 == d)
				return true;

		return false;
	}

	public static ArrayList RemoveLastArrayElement(ArrayList a) {
		if(a.Count > 0)
			a.RemoveAt(a.Count - 1);

		return a;
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
	
	public static string StringArrayToStringWithQuotes (string [] myFullString, string separator) {
		string uniqueString = "";
		string sep = "";
		string quote = "\"";
		foreach (string myStr in myFullString) {
			uniqueString += sep + quote + myStr + quote;
			sep = separator;
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

	public static string AddCsvIfNeeded(string myFile) {
		return addExtensionIfNeeded(myFile, ".csv");
	}
	public static string AddPngIfNeeded(string myFile) {
		return addExtensionIfNeeded(myFile, ".png");
	}
	public static string AddTxtIfNeeded(string myFile) {
		return addExtensionIfNeeded(myFile, ".txt");
	}
	private static string addExtensionIfNeeded(string myFile, string extension)
	{
		int posOfDot = myFile.LastIndexOf('.');
		if (posOfDot == -1) 
			myFile += extension;
		
		return myFile;
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
	
	public static string BoolToRBool (bool myBool) {
		if(myBool)
			return "TRUE";
		else
			return "FALSE";
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
	 * if partToReturn == -1, then return all the string (all the row) 
	 * if partToReturn == -2, then return the row number as string (starting by "0") 
	 */
	public static string FindOnArray(char separator, int partPassed, int partToReturn, string stringPassed, string [] stringArray) 
	{
		string foundString = "";
		int i = 0;
		foreach(string myString in stringArray) {
			string [] myStrFull = myString.Split(new char[] {separator});
			if(myStrFull[partPassed] == stringPassed) {
				if(partToReturn == -1)
					foundString = myString;
				else if(partToReturn == -2)
					foundString = i.ToString();
				else
					foundString = myStrFull[partToReturn];
			}
			i ++;

		}
		return foundString;
	}


	//avoids divide by zero
	//thought for being between 0, 1
	//ideal for progressBars
	public static double DivideSafeFraction (double val1, double val2) {
		if(val1 == 0 || val2 == 0)
			return 0;

		double result = val1 / val2;
		
		if(result > 1)
			result = 1;
		else if(result < 0)
			result = 0;

		return result;
	}
	
	public static double DivideSafeFraction (int val1, int val2) {
		return DivideSafeFraction(Convert.ToDouble(val1), Convert.ToDouble(val2));
	}

	/*
	//converts all values to positive
	public static string StringValuesAbsolute (string myString) {
		return myString.Trim('-');
	}
*/


	public static string GetDefaultPort() {
		if(UtilAll.IsWindows())
			return Constants.ChronopicDefaultPortWindows;
		else
			return Constants.ChronopicDefaultPortLinux;
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

	public static string NoYes(bool b) {
		if(! b)
			return Constants.No;
		else
			return Constants.Yes;
	}


	/* 
	 * when distances are variable on run interval 
	 */ 
	/*
	 * RSA has this code:
	 * runIntervalType distancesString 8-5-R6-9   means: 8m, 5m, rest 6 seconds, 9m
	 */

	
	//returns 0 if not RSA, if RSA, returns seconds
	public static double GetRunIVariableDistancesThisRowIsRSA(string distancesString, int row) {
		string [] str = distancesString.Split(new char[] {'-'});
		row = row % str.Length;
		if(str[row].StartsWith("R"))
			return Convert.ToDouble(str[row].Substring(1));
		else
			return 0;
	}

	//thought for values starting by 0
	public static double GetRunIVariableDistancesStringRow(string distancesString, int row) {
		string [] str = distancesString.Split(new char[] {'-'});
		row = row % str.Length;
		if(str[row].StartsWith("R"))
			return 0;
		else	
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

	public static double GetRunIVariableDistancesSpeeds(string distancesString, string timesString, bool max) 
	{
		double  searchedValue = -1; //to find max values (higher than this)
		if(! max)
			searchedValue = 1000; //to find min values (lower than this)

		string [] times = timesString.Split(new char[] {'='});
		string [] distances = distancesString.Split(new char[] {'-'});
		for(int i=0; i < times.Length; i++) {
			double time = Convert.ToDouble(times[i]);
		
			int distPos = i % times.Length;

			/*
			 * A test limited by time or undefined can have 
			 * timesString: 3,595518=6,971076=7,427422=8,109596 (4 values) 
			 * and distancesString 12-3-5 (3 values)
			 */
			if(distPos >= distances.Length)
				continue;

			//RSA is not counted as speed
			if(distances[distPos].StartsWith("R"))
				continue;

			double distance = Convert.ToDouble(distances[distPos]);

			double speed;
			if(distance == 0 || time == 0)
				speed = 0;
			else
				speed = distance / time;

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
	
	public static string GetRunISpeedsString(double distanceInterval, string timesString, 
			string distancesString, string separator, int maxRuns) 
	{
		string [] times = timesString.Split(new char[] {'='});
		string [] distances = distancesString.Split(new char[] {'-'});
		string speeds = "";
		string sep = "";
		double distance;
		int i;
		for(i=0; i < times.Length; i++) {
			double time = Convert.ToDouble(times[i]);

			//if has variable distance each track
			if(distanceInterval == -1.0) {
				int distPos = i % distances.Length;
			
				//RSA is not counted as speed
				if(distances[distPos].StartsWith("R")) {
					//if don't want to show the speed as 0, then delete next two lines
					speeds += sep + "0"; 
					sep = separator;
					continue;
				}

				distance = Convert.ToDouble(distances[distPos]);
			} else 
				distance = distanceInterval;

			double speedNow;
			if(distance == 0 || time == 0)
				speedNow = 0;
			else
				speedNow = distance / time;
			
			speeds += sep + (speedNow * 1.0).ToString();
			sep = separator;
		}
		//fill the row with 0's equalling largest row
		for(int j=i; j < maxRuns; j++) {
			speeds = speeds + ":-";
		}
		return speeds;
	}

	public static double ConvertFeetInchesToCm(int feet, double inches) {
		return feet * 30.48 + inches * 2.54;
	}
	
	public static double ConvertPoundsToKg(double pounds) {
		return pounds * 0.45359237;
	}


	/* 
	 * used to display correcly strings on R Windows, because it doesn't support accents in plots,
	 * but it plots correctly the unicode
	 */
	public static string ConvertToUnicode(string str) {
		//http://stackoverflow.com/questions/6348022/escaping-an-unicode-string-using-backslash-notation-in-c-sharp
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		foreach (char c in str)
		{
			if (' ' <= c && c <= '~')
				sb.Append(c);
			else
				sb.AppendFormat("\\U{0:X4}", (int)c);
		}
		return sb.ToString();
	}

	//test if an email is valid
	//http://stackoverflow.com/questions/1365407/c-sharp-code-to-validate-email-address
	public static bool IsValidEmail(string email)
	{
		try {
			var addr = new System.Net.Mail.MailAddress(email);
			return addr.Address == email;
		}
		catch {
			return false;
		}
	}

	//http://stackoverflow.com/a/2401873
	public static void InsertTextBeginningOfFile(string textToInsert, string filename)
	{
		try {
			string tempfile = Path.GetTempFileName();
			using (var writer = new StreamWriter(tempfile))
				using (var reader = new StreamReader(filename))
				{
					writer.WriteLine(textToInsert);
					while (! reader.EndOfStream)
						writer.WriteLine(reader.ReadLine());
				}
			File.Copy(tempfile, filename, true);
			LogB.Information("Insert text at the beginning of File successful");
		} catch {
			LogB.Error("Couldn't Insert text at the beginning of File");
		}
	}
}
