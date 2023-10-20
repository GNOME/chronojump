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
//using System.Data;
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using System.Collections.Generic; //List<T>
using System.Diagnostics; 	//for detect OS
using System.IO; 		//for detect OS
using System.Globalization; 	//Unicode

//this class tries to be a space for methods that are used in different classes
public class Util
{
	public static UtilAll.OperatingSystems operatingSystem;

	/*
	 * sometimes two doubles are similar "human eye" but different when they are compared with equal
	 * just return true if the difference between them is lower than 0.001
	 */
	public static bool SimilarDouble (double a, double b)
	{
		if(Math.Abs(a - b) < 0.001)
			return true;

		return false;
	}

	/*
	 * before 2.0.3 decimal point of forceSensor forces was culture specific. From 2.0.3 is .
	 * this method helps to see how is stored to be opened in R
	 */
	public static bool CSVDecimalColumnIsPoint(string filename, int column) //column starts at 0
	{
		List<string> contents = Util.ReadFileAsStringList(filename);
		bool headersRow = true;

		foreach(string str in contents)
		{
			//avoid header row (if any)
			if(headersRow)
				headersRow = false;
			else {
				string [] strFull = str.Split(new char[] {';'});
				if(strFull.Length < column)
					continue;

				//check that is a number when converted to current locale
				if(! Util.IsNumber(Util.ChangeDecimalSeparator(strFull[column]), true))
					continue;

				//it could be a int, or a double, if is a double check the decimal sep
				if(strFull[column].Contains("."))
					return true;
				else if(strFull[column].Contains(","))
					return false;
				//if it is an int, check on next number
			}
		}
		//if nothing is found, just say is point '.' Is not relevant
		return(true);
	}

	//all numbers are saved in database with '.' as decimal separator (method for numbers)
	public static string ConvertToPoint (double myDouble)
	{
		return ConvertToPoint (myDouble.ToString ());
	}
	//all numbers are saved in database with '.' as decimal separator
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
	//this method performs a Math.Round
	public static string DoubleToCSV(double d, int roundDecimals, string CSVsepString) {
		string s = Math.Round(d, roundDecimals).ToString();
		if(CSVsepString == "COMMA")
			return ConvertToComma(s);
		else
			return ConvertToPoint(s);
	}

	public static string ListIntToSQLString (List<int> ints, string sep)
	{
		string str = "";
		string sepStr = "";
		foreach(int i in ints)
		{
			str += sepStr + i.ToString();
			sepStr = sep;
		}
		return str;
	}
	public static List<int> SQLStringToListInt (string sqlString, string sep)
	{
		List<int> l = new List<int>();
		string [] strFull = sqlString.Split(sep.ToCharArray());
		foreach(string str in strFull)
			if(IsNumber(str, false))
				l.Add(Convert.ToInt32(str));

		return l;
	}

	public static string ListDoubleToString (List<double> d_l, int decs, string sep)
	{
		string str = "";
		string sepStr = "";
		foreach (double d in d_l)
		{
			str += sepStr + TrimDecimals (d, decs);
			sepStr = sep;
		}
		return str;
	}

	//to pass a list like: (.5, .7, .2) to: (.5, 1.2, 1.4)
	public static List<double> ListDoubleToAccumulative (List<double> original_l)
	{
		List<double> accu_l = new List<double> ();
		double previous = 0;
		foreach (double d in original_l)
		{
			accu_l.Add (d + previous);
			previous = d + previous;
		}

		return accu_l;
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
	//also used for reading . data coming from force sensor
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

	//for List of doubles better use MathUtil class (utilMath.cs)
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
	
	public static double GetAverage (List<double> values)
	{
		double sum = 0;
		foreach(double d in values)
			sum += d;

		return UtilAll.DivideSafe(sum, values.Count);
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

	public static string MakeValidSQLAndFileName(string name)
	{
		name = MakeValidFileName(MakeValidSQL(name));
		name = name.Replace(@"\","");

		return name;
	}
	//http://stackoverflow.com/a/847251
	public static string MakeValidFileName(string name)
	{
		string invalidChars = System.Text.RegularExpressions.Regex.Escape( new string( System.IO.Path.GetInvalidFileNameChars() ) );
		string invalidRegStr = string.Format( @"([{0}]*\.+$)|([{0}]+)", invalidChars );

		return System.Text.RegularExpressions.Regex.Replace( name, invalidRegStr, "_" );
	}
	public static string MakeValidSQL(string str)
	{
		/*
		 * This code sometimes does not work with '
		 * below code is better
		char [] trimChars = {'"','\''};
		return str.Trim(trimChars);
		*/

		StringBuilder myStringBuilder = new StringBuilder(str);
		myStringBuilder.Replace("'", "");
		myStringBuilder.Replace("\"", "");
		return myStringBuilder.ToString();
	}

	//to pass latin chars to JSON
	//http://stackoverflow.com/a/249126
	public static string RemoveAccents(string text)
	{
		var normalizedString = text.Normalize(NormalizationForm.FormD);
		var stringBuilder = new StringBuilder();

		foreach (var c in normalizedString)
		{
			var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
			if (unicodeCategory != UnicodeCategory.NonSpacingMark)
			{
				stringBuilder.Append(c);
			}
		}

		return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
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

	public static string RemoveComma (string myString) //to not make fail encoder exercise on tables sent to R
	{
		return myString.Replace(",", "");
	}
	public static string RemoveSemicolon (string myString) //to not make fail encoder exercise on tables sent to R
	{
		return myString.Replace(";", "");
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

	//problems with paths
	public static string RemoveSlash(string myString)
	{
		StringBuilder myStringBuilder = new StringBuilder(myString);
		myStringBuilder.Replace("/", " ");
		return myStringBuilder.ToString();
	}

	public static string AddBoldMarks(string s)
	{
		return "<b>" + s + "</b>";
	}
	public static string RemoveBoldMarks(string s)
	{
		s = s.Replace("<b>", "");
	        s = s.Replace("</b>", "");
		return s;
	}

	//use on of this two methods
	//this is the default, used by almost all methods: change to an space
	public static string RemoveChar(string s, char c)
	{
		return RemoveChar(s, c, true);
	}
	public static string RemoveChar(string s, char c, bool changeBySpace)
	{
		StringBuilder myStringBuilder = new StringBuilder(s);
		if(changeBySpace)
			myStringBuilder.Replace(c,' ');
		else
			myStringBuilder.Replace(c.ToString(), "");

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

	public static string RemoveFromChar (string str, char c)
	{
		int pos = str.IndexOf(c);
		if (pos > 0)
			return str.Substring(0, pos);

		return str;
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
	
	public static string ChangeChars(string str, string charBefore, string charAfter)
	{
		StringBuilder myStringBuilder = new StringBuilder(str);
		myStringBuilder.Replace(charBefore, charAfter);
		return myStringBuilder.ToString();
	}

	public static string RemoveCenterCharsOnLongString (string str, int maxLength)
	{
		if (str.Length > maxLength)
		{
			int charsAtSide = Convert.ToInt32 (Math.Floor (maxLength / 2.0));
			return (
					str.Substring (0, charsAtSide) + "…" +
					str.Substring (str.Length -charsAtSide, charsAtSide) );
		}
		return str;
	}

	//old code sends and returns strings
	public static string GetHeightInCentimeters (string time) {
		return GetHeightInCentimeters (Convert.ToDouble(time)).ToString();
	}
	//new code (2019 ...) sends and returns doubles
	public static double GetHeightInCentimeters (double tv) {
		// s = 100 * 4.905 * (tv/2)^2
		return 100 * 4.905 * Math.Pow( tv / 2.0 , 2 );
	}
	//shorter
	public static double GetHeightInCm (double tv) {
		return GetHeightInCentimeters (tv);
	}

	public static double GetHeightInMeters (double tv) {
		// s = 4.905 * (tv/2)^2
		return 4.905 * Math.Pow( tv / 2.0 , 2 );
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
			return tcString.Substring(0, lastEqualPos);
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


	public static string GetSpeed (string distance, string time, bool metersSecondsPreferred)
	{
		return GetSpeed (Convert.ToDouble (distance), Convert.ToDouble (time), metersSecondsPreferred).ToString ();
	}
	public static double GetSpeed (double distance, double time, bool metersSecondsPreferred)
	{
		if(metersSecondsPreferred)
			return UtilAll.DivideSafe (distance, time);
		else
			return UtilAll.DivideSafe (3.6 * distance, time);
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

	public static double GetAverageImpulsionSpeed(double jumpHeightM)
	{
		return Math.Sqrt( (9.81 * jumpHeightM) / 2);
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

	/*
	 * unused
	public static bool IsNumber(char c)
	{
		return IsNumber(c.ToString(), false);
	}
	*/

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
	//Since 2.2.2 on Compujump admin LocaApplicationData can change
	//this will check if any config path is present
	public static string GetLocalDataDir (bool withFinalSeparator)
	{
		if (Config.LastDBFullPathStatic == "")
			return UtilAll.GetDefaultLocalDataDir (withFinalSeparator); //this can be checked by Mini
		else {
			if (withFinalSeparator)
				 return Config.LastDBFullPathStatic + Path.DirectorySeparatorChar;
			else
				 return Config.LastDBFullPathStatic;
		}
	}

	public static string GetConfigFileName() {
		return Path.Combine (GetLocalDataDir (false) +  Path.DirectorySeparatorChar + Constants.FileNameConfig);
	}
	public static string GetECapSimSignalFileName() {
		return Path.Combine (GetLocalDataDir (false) +  Path.DirectorySeparatorChar + "eCapSimSignal.txt");
	}

	public static string GetPresentationFileName() {
		return Path.Combine (GetLocalDataDir (false) +  Path.DirectorySeparatorChar + Constants.FileNamePresentation);
	}

	//used when presentation contains a filename and need to get full url
	public static string GetPresentationFileName (string filename) {
		return Path.Combine (GetLocalDataDir (false) +  Path.DirectorySeparatorChar + filename);
	}

	//url and videoURL stored path is relative to be able to move data between computers
	//then SELECT: makes it abolute (addURLpath)
	//INSERT and UPDATE: makes it relative (removeURLpath)
	public static string MakeURLabsolute(string url) {
		string parentDir = Util.GetLocalDataDir(true); //add final '/' or '\'
		if( ! url.StartsWith(parentDir) )
			url = parentDir + url;

		return url;
	}
	public static string MakeURLrelative(string url) {
		string parentDir = Util.GetLocalDataDir(true); //add final '/' or '\'
		if( url.StartsWith(parentDir) )
			url = url.Replace(parentDir, "");

		return url;
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
		return Path.Combine (
				GetLocalDataDir (false) + Path.DirectorySeparatorChar + "database");
	}
	
	//if database dir has illegal characters, use this temp dir and remember to copy db at end, or to restore if chrashed
	public static string GetDatabaseTempDir() {
		return Path.Combine(Path.GetTempPath(), "Chronojump");
	}

	public static string GetDatabaseTempImportDir() {
		return Path.Combine(Path.GetTempPath(), "ChronojumpImportDir");
	}

	public static string GetCloudReadTempDir ()
	{
		return Path.Combine(Path.GetTempPath(), "ChronojumpCloudRead");
	}
	/*
	   when exporting a session the 7z filename will be the same than the folder inside
	   and this is imported correctly on 2.3.0,

	   BUT if on passing the file to another person the 7z exists,
	   then will be renamed by the OS something like _copy.7z
	   and 2.3.0 will not be able to find the dir inside because it is named different than the 7z

	   So we use this TempImportExtractDir to put the extracted content on that folder and be able to find it on import

	   We do this process because listing the .db with 7zr gives too much info
	   */
	/*
	   Unfortunately on Windows we cannot delete the dir after a call, because the sessionSwitcher has this dir opened for some Sqlite problem, we have not succeded on closing the dbcon or whatever, so instead of having a dir, have subdirs from 1 to ... that will be deleted on computer reboot

	public static string GetDatabaseTempImportExtractDir() {
		return Path.Combine(Path.GetTempPath(), "ChronojumpImportExtractDir");
	}
	*/
	public static string CreateAndGetDatabaseTempImportExtractDirNext ()
	{
		string parentDir = Path.Combine(Path.GetTempPath(), "ChronojumpImportExtractDir");
		int biggest = -1;

		if( ! Directory.Exists (parentDir))
			Directory.CreateDirectory (parentDir);
		else {
			foreach (string dir in Directory.GetDirectories (parentDir))
			{
				string dirName = GetLastPartOfPath (dir);
				if (IsNumber (dirName, false) && Convert.ToInt32 (dirName) > biggest)
					biggest = Convert.ToInt32 (dirName);
			}
		}

		string pathToReturn = Path.Combine (parentDir, (biggest + 1).ToString ());
		Directory.CreateDirectory (pathToReturn);
		return pathToReturn;
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
				GetLocalDataDir (false) + Path.DirectorySeparatorChar + "multimedia");
	}
	
	public static string GetPhotosDir(bool small) {
		string smallDir = "";
		if(small)
			smallDir = Path.DirectorySeparatorChar + Constants.SmallPhotoDir; 

		return Path.Combine(
				GetLocalDataDir (false) +
				Path.DirectorySeparatorChar + "multimedia" +
				Path.DirectorySeparatorChar + "photos") + smallDir;
	}
	
	public static string GetVideosDir() {
		return Path.Combine(
				GetLocalDataDir (false) +
				Path.DirectorySeparatorChar + "multimedia" +
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
	
	/*
	 * force sensor suff ------------------>
	 */

	//to store force sensor data and graphs
	public static string GetForceSensorDir()
	{
		return Path.Combine(
				GetLocalDataDir (false) +
				Path.DirectorySeparatorChar + "forceSensor");
	}
	public static void CreateForceSensorDirIfNeeded ()
	{
		string dir = GetForceSensorDir();
		if( ! Directory.Exists(dir)) {
			Directory.CreateDirectory (dir);
			LogB.Information ("created dir:", dir);
		}
	}

	//forceSensor organized by sessions.
	public static string GetForceSensorSessionDir (int sessionID)
	{
		return GetForceSensorDir() + Path.DirectorySeparatorChar + sessionID.ToString();
	}
	public static void CreateForceSensorSessionDirIfNeeded (int sessionID)
	{
		string dir = GetForceSensorSessionDir(sessionID);
		if( ! Directory.Exists(dir)) {
			Directory.CreateDirectory (dir);
			LogB.Information ("created dir:", dir);
		}
	}

	/*
	 * <--------------- end of force sensor suff
	 */

	/*
	 * run encoder suff ------------------>
	 */

	//to store run encoder data and graphs
	public static string GetRunEncoderDir()
	{
		return Path.Combine(
				GetLocalDataDir (false) +
				Path.DirectorySeparatorChar + "raceAnalyzer");
	}
	public static void CreateRunEncoderDirIfNeeded ()
	{
		string dir = GetRunEncoderDir();
		if( ! Directory.Exists(dir)) {
			Directory.CreateDirectory (dir);
			LogB.Information ("created dir:", dir);
		}
	}

	//runEncoder organized by sessions.
	public static string GetRunEncoderSessionDir (int sessionID)
	{
		return GetRunEncoderDir() + Path.DirectorySeparatorChar + sessionID.ToString();
	}
	public static void CreateRunEncoderSessionDirIfNeeded (int sessionID)
	{
		string dir = GetRunEncoderSessionDir(sessionID);
		if( ! Directory.Exists(dir)) {
			Directory.CreateDirectory (dir);
			LogB.Information ("created dir:", dir);
		}
	}

	public static string GetRunEncoderTempProgressDir() {
		return Path.Combine(Path.GetTempPath(), "chronojump_race_analyzer_progress");
	}
	public static string GetRunEncoderTempGraphsDir() {
		return Path.Combine(Path.GetTempPath(), "chronojump_race_analyzer_graphs");
	}

	/*
	 * <--------------- end of force sensor suff
	 */


	//videos are organized by sessions. Photos no.
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

	public static List<string> GetVideosOfSessionAndMode (int sessionID, Constants.TestTypes testType)
	{
		List<string> l = new List<string> ();

		string sessionDir = GetVideoSessionDir (sessionID);
		if (! Directory.Exists (sessionDir))
			return l;

		foreach (string s in Directory.GetFiles (sessionDir))
		{
			string s2 = GetLastPartOfPath (s);
			if (s2.StartsWith (testType.ToString() + "-"))
				l.Add (s2);
		}

		return l;
	}


//TODO: now using mp4, ensure old avi can be also retrieved

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

	/*
	 * --------------------- GetPhoto stuff
	 */
	//returns the jpg, png or ""
	public static string UserPhotoURL (bool small, int uniqueID)
	{
		string jpeg = GetPhotoFileName (small, uniqueID);
		if(File.Exists(jpeg))
			return jpeg;

		string png = GetPhotoPngFileName (small, uniqueID);
		if(File.Exists(png))
			return png;

		return "";
	}
	
	//jpg
	public static string GetPhotoFileName (bool small, int uniqueID) {
		return GetPhotosDir(small) + Path.DirectorySeparatorChar + uniqueID.ToString() +
			GetMultimediaExtension(Constants.MultimediaItems.PHOTO);
	}
	//png
	public static string GetPhotoPngFileName (bool small, int uniqueID) {
		return GetPhotosDir(small) + Path.DirectorySeparatorChar + uniqueID.ToString() +
			GetMultimediaExtension(Constants.MultimediaItems.PHOTOPNG);
	}

	/*
	 * --------------------- End of GetPhoto stuff
	 */

	public static bool IsJpeg(string filename)
	{
		return (filename.ToLower().EndsWith("jpeg") || filename.ToLower().EndsWith("jpg"));
	}

	public static bool IsPng(string filename)
	{
		return (filename.ToLower().EndsWith("png"));
	}

	/*
	 * --------------------- Start of video stuff
	 */

	//Pre: filename without 0001.png
	public static string GetWebcamPhotoTempFileNamePre()
	{
		return Path.Combine(Path.GetTempPath(), Constants.PhotoTemp + "-");
	}

	//Pre: filename with 0001.png
	public static string GetWebcamPhotoTempFileNamePost()
	{
		return GetWebcamPhotoTempFileNamePre() + "0001.png";
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
	

	public static string GetMultimediaExtension (string filename)
	{
		if(UtilMultimedia.GetImageType(filename) == UtilMultimedia.ImageTypes.JPEG)
			return Constants.ExtensionPhoto;
		if(UtilMultimedia.GetImageType(filename) == UtilMultimedia.ImageTypes.PNG)
			return Constants.ExtensionPhotoPng;

		return "";
	}

	public static string GetMultimediaExtension (Constants.MultimediaItems multimediaItem) {
		if(multimediaItem == Constants.MultimediaItems.VIDEO)
			return Constants.ExtensionVideo;
		else if(multimediaItem == Constants.MultimediaItems.PHOTO)
			return Constants.ExtensionPhoto;
		else //multimediaItem == Constants.MultimediaItems.PHOTOPNG
			return Constants.ExtensionPhotoPng;
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
				//check size before copyiing. To know if file has been copied while is not fully written
				LogB.Information("sizeOrigin: " + (new System.IO.FileInfo(origin).Length).ToString());

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
	
	public static string GetThemeDir() {
		return Path.Combine(GetChronojumpDir (), "theme");
	}
	public static string GetThemeFile() {
		return Path.Combine(GetThemeDir(), "gtk-2.0" + Path.DirectorySeparatorChar + "gtkrc");
	}

	/********** start of encoder paths ************/

	public static string GetEncoderExportTempFileName() {
		return Path.Combine(Path.GetTempPath(), "encoder_set_export.csv");
	}

	public static string GetEncoderTriggerFileName() {
		return Path.Combine(Path.GetTempPath(), "encoder_trigger.txt");
	}
	public static string GetEncoderTriggerDateTimeFileName() {
		return Path.Combine(Path.GetTempPath(), "encoder_trigger_" + UtilDate.ToFile(DateTime.Now) + ".txt");
	}

	/********** end of encoder paths ************/


	/********** start of rfid paths ************/

	/*
	   Not used anymore, read: src/gui/networks.cs

	public static string GetRFIDCaptureScript() {
		return Path.Combine(GetPrefixDir(), "bin/chronojump_rfid_capture.py");
	}
	public static string GetRFIDCapturedFile() {
		return Path.Combine(Path.GetTempPath(), "chronojump_rfid.txt");
	}
	*/


	/********** end of rfid paths ************/


	public static string GetManualDir() {
		//we are on:
		//lib/chronojump/ (Unix) or bin/ (win32)
		//we have to go to
		//share/doc/chronojump
		return System.IO.Path.Combine(Util.GetPrefixDir(),
			"share" + Path.DirectorySeparatorChar + "doc" + Path.DirectorySeparatorChar + "chronojump");
	}

	public static string GetPrefixDir()
	{
		//on mac with the bundle we use this method to return PrefixDir, as BaseDirectory and GetCurrentDirectory() do not work
		if(operatingSystem == UtilAll.OperatingSystems.MACOSX)
			return (System.IO.Path.Combine(
						Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName),
						".."));

		string baseDirectory = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "..");
		if (! Directory.Exists(Path.Combine(baseDirectory, "lib" + Path.DirectorySeparatorChar + "chronojump"))) {
			baseDirectory = System.IO.Path.Combine(baseDirectory, "..");
		}
		return baseDirectory;
	}

	public static string GetChronojumpDir ()
	{
		return System.IO.Path.Combine(GetPrefixDir(),
			"share" + Path.DirectorySeparatorChar + "chronojump");
	}

	public static string GetImagesDir ()
	{
		return System.IO.Path.Combine (GetChronojumpDir (),"images");
	}

	public static string GetCssDir(){
		return GetChronojumpDir( );
	}

	//currently sounds are inside images folder	
	public static string GetSoundsDir(){
		return GetImagesDir();
	}

	//previous to 2.1.3
	static string backupDirOld = GetDatabaseDir() + Path.DirectorySeparatorChar + "backup";
	public static string GetBackupDirOld () {
		return backupDirOld;
	}

	/*
	   Unused since 2.1.3

	public static void BackupDirOldCreateIfNeeded () {
		if( ! Directory.Exists(backupDirOld)) {
			Directory.CreateDirectory (backupDirOld);
			LogB.Information ("created backup dir");
		}
	}

	public static void BackupDatabase () {
		string homeDir = GetDatabaseDir();
		string dateParsed = UtilDate.ToFile(DateTime.Now);

		if(File.Exists(System.IO.Path.Combine(homeDir, "chronojump.db")))
			File.Copy(System.IO.Path.Combine(homeDir, "chronojump.db"), 
				System.IO.Path.Combine(backupDirOld, "chronojump_" + dateParsed + ".db"));
		else {
			LogB.Error("Error, chronojump.db file doesn't exist!");
		}
	}
	*/

	//size of the "backups" dir (used for automatic backups on start < 2.1.3)
	public static void GetBackupsSize (out int files, out int sizeInKB)
	{
		if(! Directory.Exists(backupDirOld))
		{
			files = 0;
			sizeInKB = 0;
			return;
		}

		DirectoryInfo info = new DirectoryInfo(backupDirOld);
		//long totalSize = info.EnumerateFiles().Sum(file => file.Length); 	//LinQ 4.0
		//long totalSize = info.GetFiles().Sum(file => file.Length); 		//LinQ 3.5

		files = 0;
		long totalSize = 0;
		foreach(var file in info.EnumerateFiles())
		{
			totalSize += file.Length;
			files ++;
		}
		sizeInKB = (int) UtilAll.DivideSafe(totalSize, 1024);
	}

	//TODO: maybe this will need a thread
	public static int GetFullDataSize (bool includingLogs, bool includingOldBackupsDir)
	{
		int sizeInKB = 0;

		long fullDataSize = DirSizeWithSubdirs (new DirectoryInfo (GetLocalDataDir(false)));
		sizeInKB = (int) UtilAll.DivideSafe (fullDataSize, 1024);

		if (! includingLogs)
		{
			long logsSize = DirSizeWithSubdirs (new DirectoryInfo (UtilAll.GetLogsDir (Config.LastDBFullPathStatic)));
			sizeInKB -= (int) UtilAll.DivideSafe (logsSize, 1024);
		}

		if(! includingOldBackupsDir)
		{
			int filesBackup, sizeInKBBackup;
			Util.GetBackupsSize (out filesBackup, out sizeInKBBackup);
			sizeInKB -= sizeInKBBackup;
		}

		return sizeInKB;
	}

	//https://stackoverflow.com/a/468131
	public static long DirSizeWithSubdirs (DirectoryInfo d)
	{
		long size = 0;
		// Add file sizes.
		FileInfo[] fis = d.GetFiles();
		foreach (FileInfo fi in fis)
		{
			size += fi.Length;
		}
		// Add subdirectory sizes.
		DirectoryInfo[] dis = d.GetDirectories();
		foreach (DirectoryInfo di in dis)
		{
			size += DirSizeWithSubdirs (di);
		}
		return size;
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
	
	public static bool DirectoryDelete (string fileName)
	{
		LogB.Information("Checking if this directory exists: " + fileName);
		try {
			if(Directory.Exists (fileName)) {
				LogB.Information ("Deleting " + fileName + " ...");
				Directory.Delete (fileName, true); //recursive
				LogB.Information ("Deleted");
				return true;
			}
		} catch {
			LogB.Error ("Problem deleting");
		}
		return false;
	}

	/*
	 * To avoid crash on file copy on windows when the file is already opened by another application
	 * eg. export a csv, excel opens it, export again and crash to Chronojump
	 * for overwiting a file "owned" by spreadsheet software on a defective operating system
	 * note the "above" catch on gui/app1/encoder checkFile does not catch this, maybe because thread are involved, so return false here and then Config.ErrorInExport will be true;
	 */
	public static bool FileCopySafe (string origin, string destination, bool overwrite)
	{
		try {
			File.Copy (origin, destination, overwrite);
		} catch {
			LogB.Information (string.Format ("Catched on File Copy from: {0}, to: {1}, overwrite: {2}",
						origin, destination, overwrite));
			return false;
		}

		return true;
	}

	public static bool FileMove(string path, string filenameOrigin, string filenameDestination)
	{
		LogB.Information(string.Format("Going to move: {0} to {1}",
					path + Path.DirectorySeparatorChar + filenameOrigin,
					path + Path.DirectorySeparatorChar + filenameDestination
					));
		try {
			File.Move(
					path + Path.DirectorySeparatorChar + filenameOrigin, 
					path + Path.DirectorySeparatorChar + filenameDestination
					);
			LogB.Information("Moved ok");
			return true;
		} catch {
			LogB.Information("Catched! cannot move");
		}

		return false;
	}
	
	public static bool DirectoryExists(string dirName){
		return Directory.Exists(dirName);
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
		else if(operatingSystem == UtilAll.OperatingSystems.MACOSX)
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

	//file or folder
	public static bool OpenURL (string url)
	{
		LogB.Information("OpenFolder without quotes: " + url);

		//add quotes to solve spacing problems
		url = "\"" + url + "\"";
		LogB.Information("OpenFolder with quotes: " + url);

		try {
			//more system specific methods on: https://stackoverflow.com/a/49664847/12366369
			//also if do not work, check about relative paths here: https://stackoverflow.com/questions/52599105/c-sharp-under-linux-process-start-exception-of-no-such-file-or-directory
			if(operatingSystem == UtilAll.OperatingSystems.LINUX)
				System.Diagnostics.Process.Start("xdg-open", url);
			else
				System.Diagnostics.Process.Start(url);

			return true;
		} catch {
			return false;
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

	public enum SoundCodes { VOLUME_OFF, OK, PROBLEM_NO_FILE, PROBLEM_OTHER };
	public static bool TestSound;

	public static SoundCodes PlaySound (Constants.SoundTypes mySound,
			bool volumeOn, Preferences.GstreamerTypes gstreamer)
	{
		if ( ! volumeOn )
			return SoundCodes.VOLUME_OFF;
		
		/*
		 * Using GstreamerMethod because .Net method makes crash some Linux ALSA,
		 * and some MacOSX users have 300% CPU
		 */

		if(operatingSystem == UtilAll.OperatingSystems.WINDOWS ||
				gstreamer == Preferences.GstreamerTypes.SYSTEMSOUNDS)
			return playSoundWindows(mySound);
		else
			return playSoundGstreamer(mySound, gstreamer);
	}
	
	private static SoundCodes playSoundGstreamer (Constants.SoundTypes mySound, Preferences.GstreamerTypes gstreamer)
	{
		string fileName = "";
		if(! UseSoundList)
			fileName = getSound(mySound); //default chronojump
		else
			fileName = getSoundFromSoundList(); //espectacle

		if(! File.Exists(fileName)) {
			LogB.Warning("Cannot found this sound file: " + fileName);
			return SoundCodes.PROBLEM_NO_FILE;
		}

		Process p;
		try {
			ProcessStartInfo pinfo = new ProcessStartInfo();

			string pBin= "";
			if(gstreamer == Preferences.GstreamerTypes.GST_0_1) {
				pBin="gst-launch-0.10";
				pinfo.Arguments = "playbin2 " + @"uri=file://" + fileName;
			}
			else if (gstreamer == Preferences.GstreamerTypes.GST_1_0) {
				pBin="gst-launch-1.0";
				pinfo.Arguments = "playbin " + @"uri=file://" + fileName;
			}
			else if (gstreamer == Preferences.GstreamerTypes.FFPLAY)
			{
				pBin="ffplay";
				if(operatingSystem == UtilAll.OperatingSystems.WINDOWS)
				{
					//if(System.Environment.Is64BitProcess)
						pBin = System.IO.Path.Combine(Util.GetPrefixDir(), "bin/ffplay.exe");
					//else
					//	pBin = System.IO.Path.Combine(Util.GetPrefixDir(), "bin/i386/ffplay.exe"); i386 is no longer updated/included
				}
				else if(operatingSystem == UtilAll.OperatingSystems.MACOSX)
					pBin = System.IO.Path.Combine(Util.GetPrefixDir(), "bin/ffplay");

				pinfo.Arguments = fileName + " -nodisp -nostats -hide_banner -autoexit";
			}

			pinfo.FileName=pBin;

			LogB.Information("Arguments:", pinfo.Arguments);
			pinfo.CreateNoWindow = true;
			pinfo.UseShellExecute = false;

			if(TestSound)
				pinfo.RedirectStandardError = true;

			p = new Process();
			p.StartInfo = pinfo;
			p.Start();
		} catch {
			LogB.Error("Cannot playSoundGstreamer");
			return SoundCodes.PROBLEM_OTHER;
		}

		if(TestSound)
		{
			string stderr = p.StandardError.ReadToEnd ().TrimEnd ('\n');
			if(stderr != "")
				return SoundCodes.PROBLEM_OTHER;
		}

		return SoundCodes.OK;
	}

	//maybe in the future this method will be deprecated and it only will be used the Gstreamer method
	private static SoundCodes playSoundWindows (Constants.SoundTypes mySound)
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
			return SoundCodes.PROBLEM_OTHER;
		}

		return SoundCodes.OK;
	}
	
	private static string getSound (Constants.SoundTypes mySound)
	{
		string fileName = "";
		switch(mySound) {
			case Constants.SoundTypes.CAN_START:
				fileName = "start.wav";
				//author: kendallbear
				//https://www.freesound.org/people/kendallbear/sounds/123804/
				break;
			case Constants.SoundTypes.GOOD:
				fileName = "ok.wav";
				//author: bradwesson
				//https://www.freesound.org/people/bradwesson/sounds/135936/
				break;
			case Constants.SoundTypes.BAD:
				fileName = "bad.wav";
				//author: Autistic Lucario
				//https://www.freesound.org/people/Autistic%20Lucario/sounds/142608/
				break;
		}

		fileName = Util.GetSoundsDir() + Path.DirectorySeparatorChar + fileName;
		return fileName;
	}

	public static bool UseSoundList;
	static List<string> soundList;
	static int soundListPos;

	public static string getSoundsFileName() {
		return Path.Combine(Util.GetLocalDataDir (false) +  Path.DirectorySeparatorChar + "sounds.txt");
	}

	public static void CreateSoundList()
	{
		soundList = new List<string>();
		soundListPos = 0;
		string contents = Util.ReadFile(getSoundsFileName(), false);
		if (contents != null && contents != "")
		{
			string line;
			using (StringReader reader = new StringReader (contents)) {
				do {
					line = reader.ReadLine ();

					if (line == null)
						break;
					if (line == "" || line[0] == '#')
						continue;

					soundList.Add(line);
				} while(true);
			}
		}
	}

	//called on trigger use
	public static void NextSongInList()
	{
		if(soundList == null)
			return;

		if(soundListPos +1 >= soundList.Count)
			soundListPos = 0;
		else
			soundListPos ++;

		/*
		 * done like above instead of below code,
		 * to ensure soundListPos will not be outside the array when the other thread reads
		 *
		 * soundListPos ++;
		 * if(soundListPos >= soundList.Count)
		 * 	soundListPos = 0;
		 */
	}

	private static string getSoundFromSoundList()
	{
		return soundList[soundListPos];
	}

	public static bool SoundIsPum()
	{
		return (soundList[soundListPos].EndsWith("we_will_rock_you_pum.wav"));
	}
	public static bool SoundIsPam()
	{
		return (soundList[soundListPos].EndsWith("we_will_rock_you_pam.wav"));
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
	//called by treeview_jump, treeview_run and gui/jump_edit and gui/run_edit
	public static string GetLimitedRounded (string limitedString, int pDN)
	{
		LogB.Information("GetLimitedRounded limitedString pdN");
		LogB.Information(limitedString);
		LogB.Information(pDN.ToString());

		string myLimitedWithoutLetter = limitedString.Substring(0, limitedString.Length -1);

		string myLimitedLetter = limitedString.Substring(limitedString.Length -1, 1);

		//without the letter, use jumps, tracks or seconds
		string myLimiter = " " + Constants.jumpsTranslatedStr (); //("J")
		if (myLimitedLetter == "R")
			myLimiter = " " + Constants.tracksTranslatedStr ();
		if (myLimitedLetter == "T")
			myLimiter = " s";

		return TrimDecimals(myLimitedWithoutLetter, pDN) + myLimiter;
	}

	public static string [] AddToArrayString (string [] initialString, List<string> add_l)
	{
		string [] returnString = new string[initialString.Length + add_l.Count];
		int i, j;
		for (i=0 ; i < initialString.Length; i ++)
			returnString[i] = initialString[i];
		for (j=0 ; j < add_l.Count; j ++)
			returnString[i+j] = add_l[j];

		return returnString;
	}

	public static string [] AddArrayString (string [] initialString, string [] addString)
	{
		return UtilAll.AddArrayString (initialString, addString);
	}

	//bool firstOrLast: true means first
	public static string [] AddArrayString (string [] initialString, string addString, bool firstOrLast)
	{
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

	public static string ListStringToString (List<string> l)
	{
		string str = "";
		string sep = "";
		if(l == null)
			return str;

		foreach (string s in l)
		{
			str += sep + s;
			sep = "\n";
		}

		return str;
	}
	public static string ListStringToString (List<string> l, string separator)
	{
		if (l == null || l.Count == 0)
			return "";

		string str = "";
		string sepDo = "";

		foreach (string s in l)
		{
			str += sepDo + s;
			sepDo = separator;
		}

		return str;
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

	public static List<int> AddToListIntIfNotExist (List<int> l, int i) {
		bool found = FoundInListInt(l, i);
		if(! found)
			l.Add(i);

		return l;
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

	public static bool FoundInListInt(List<int> l, int i) {
		foreach (int i2 in l)
			if(i2 == i)
				return true;

		return false;
	}
	public static bool FoundInListDouble(List<double> l, double d) {
		foreach (double d2 in l)
			if(d2 == d)
				return true;

		return false;
	}
	public static bool FoundInListString (List<string> l, string s)
	{
		foreach (string l2 in l)
			if(l2 == s)
				return true;

		return false;
	}
	public static bool StartsWithInListString (List<string> l, string s)
	{
		foreach (string l2 in l)
			if(l2.StartsWith (s))
				return true;

		return false;
	}

	public static ArrayList RemoveLastArrayElement(ArrayList a) {
		if(a.Count > 0)
			a.RemoveAt(a.Count - 1);

		return a;
	}

	// https://stackoverflow.com/a/273666
	public static List<T> ListRandomize<T>(List<T> list)
	{
		List<T> randomizedList = new List<T>();
		Random rnd = new Random();
		while (list.Count > 0)
		{
			int index = rnd.Next(0, list.Count); //pick a random item from the master list
			randomizedList.Add(list[index]); //place it at the end of the randomized list
			list.RemoveAt(index);
		}
		return randomizedList;
	}

	public static List<T> ListGetFirstN<T> (List<T> original_l, int firstN)
	{
		List<T> cutted_l = new List<T>();
		for (int i = 0; i < firstN; i ++)
			cutted_l.Add (original_l[i]);

		return cutted_l;
	}

	public static void TestSortDoublesListstring()
	{
		List<string> numbers_l = new List<string>() { "3", "99", "135", "45", "75", "17", "88", "5" }; //ints
		//List<string> numbers_l = new List<string>() { "3,5", "99,54", "135,1", "45,5", "75,5", "45,9", "88", "45,3" }; //latin

		List<string> numbersSorted_l = SortDoublesListString(numbers_l);

		LogB.Information("Sorted: ");
		foreach(string s in numbersSorted_l)
			LogB.Information(s);
	}

	//sort a list of doubles descending until 0
	public static List<string> SortDoublesListString (List<string> unsorted_l)
	{
		if(unsorted_l.Count < 2)
			return unsorted_l;

		List<string> sorted_l = new List<string>();
		for(int i = 0; i < unsorted_l.Count; i ++)
		{
			double highest = 0;
			int highestPos = 0;
			for(int j = 0; j < unsorted_l.Count; j ++)
			{
				double comparingToNum = Convert.ToDouble(unsorted_l[j]);
				if(comparingToNum > highest)
				{
					highest = comparingToNum;
					highestPos = j;
				}
			}
			sorted_l.Add(unsorted_l[highestPos]); //store highest value on sorted_l
			unsorted_l[highestPos] = "0"; //mark as 0 on unsortd_l to not choose it again
		}
		return sorted_l;
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
	public static string DoubleArrayToString (double [] doublesArray, int decs, string separator)
	{
		string str = "";
		string sep = "";
		foreach (double d in doublesArray)
		{
			str += sep + TrimDecimals (d, decs);
			sep = separator;
		}
		return str;
	}

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

	public static List<string> StringArrayToListString (string [] arrayString)
	{
		List<string> str_l = new List<string> ();
		foreach (string myStr in arrayString)
			str_l.Add (myStr);
	
		return str_l;
	}
	
	//to create an string [] of one member
	public static string [] StringToStringArray (string str) {
		string [] ret = new string[1];
		ret[0] = str;
		return ret;
	}

	public static string [] ListStringToStringArray (List<string> string_l)
	{
		string [] stringArray = new string[string_l.Count];
		for(int i = 0; i < string_l.Count; i ++)
			stringArray[i] = string_l[i];

		return stringArray;
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
		if(myFile.ToLower().EndsWith(extension))
			return myFile;

		return myFile + extension;
	}

	//this includes the '.', eg: returns ".csv"
	public static string GetExtension(string myFile)
	{
		int posOfDot = myFile.LastIndexOf('.');
		if (posOfDot > 0)
			return myFile.Substring(posOfDot);

		return "";
	}

	public static string RemoveExtension(string myFile)
	{
		int posOfDot = myFile.LastIndexOf('.');
		if (posOfDot > 0)
			myFile = myFile.Substring(0, posOfDot);

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
			return Constants.InStr();
		else
			return Constants.OutStr();
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
		foreach(string level in Constants.LevelsStr())
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

	public static int FindOnListString (List<string> str_l, string searched)
	{
		for (int i = 0; i < str_l.Count; i ++)
			if(str_l[i] == searched)
				return i;

		return 0;
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
			return Constants.YesStr();
		else
			return Constants.NoStr();
	}

	public static string NoYes(bool b) {
		if(! b)
			return Constants.NoStr();
		else
			return Constants.YesStr();
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

	public static void ConvertCmToFeetInches(double cm, out int feet, out double inches)
	{
		double numInches = UtilAll.DivideSafe(cm, 2.54);
		feet = Convert.ToInt32(Math.Floor(UtilAll.DivideSafe(numInches, 12)));
		inches = numInches % 12;
	}

	public static double ConvertPoundsToKg(double pounds) {
		return pounds * 0.45359237;
	}
	public static double ConvertKgToPounds(double kg) {
		return kg / 0.45359237;
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
			{
				//old: C#
				//sb.AppendFormat("\\U{0:X4}", (int)c);
				//On R, note:
				//\Unnnn 1-8 hex digits (so this can have problems when a unicode char is at the side of another char
				// 	\U00E1 2 : works
				// 	\U00E12 : does not work, tries to find a unicode char different than the U00E1
				//\unnnn 1-4 hex digits
				// 	\u00E1 2 : works
				// 	\u00E12 : works
				//https://stat.ethz.ch/R-manual/R-devel/library/base/html/Quotes.html
				sb.AppendFormat("\\u{0:X4}", (int)c);
			}
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

	//used on export where we just want to copy the dirs without updating gui objects
	//https://stackoverflow.com/a/3822913
	public static void CopyFilesRecursively(string sourcePath, string targetPath)
	{
		//Now Create all of the directories
		foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
		{
			Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
		}

		//Copy all the files & Replaces any files with the same name
		foreach (string newPath in Directory.GetFiles(sourcePath, "*.*",SearchOption.AllDirectories))
		{
			File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
		}
	}

}

//used on backup
public class UtilCopy
{
	public int BackupMainDirsCount;
	public int BackupSecondDirsCount;
	public int BackupSecondDirsLength;
	public string LastMainDir;
	public string LastSecondDir;
	private int sessionID;
	private bool copyLogs;
	private bool copyConfig;
	private bool copyOtherExistingDBs;

	//to go faster on CopyFilesRecursively
	static string backupDirOld = Util.GetBackupDirOld();

	//-1 is the default on a backup, means all sessions (regular backup)
	//4 will only copy files related to session 4 (for export session)
	//on export do not copy logs, on backup user can select
	//copyOtherExistingDBs is true in backup but false on export
	public UtilCopy(int sessionID, bool copyLogs, bool copyConfig, bool copyOtherExistingDBs)
	{
		BackupMainDirsCount = 0;
		BackupSecondDirsCount = 0;
		BackupSecondDirsLength = 0;
		LastMainDir = "";
		LastSecondDir = "";

		this.sessionID = sessionID;
		this.copyLogs = copyLogs;
		this.copyConfig = copyConfig;
		this.copyOtherExistingDBs = copyOtherExistingDBs;
	}

	//http://stackoverflow.com/a/58779
	public bool CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target, uint level)
	{
		DirectoryInfo [] diArray = source.GetDirectories();
		foreach (DirectoryInfo dir in diArray)
			if(dir.ToString() != backupDirOld) //do not copy backup files
			{
				if(level == 0)
				{
					if (! copyLogs && Util.GetLastPartOfPath(dir.ToString()) == "logs")
						continue;

					BackupMainDirsCount ++;
					LastMainDir = Util.GetLastPartOfPath (dir.ToString());
					BackupSecondDirsCount = 0;
					//LogB.Information("at level 0: " + dir);
				} else if(level == 1)
				{
					//on export, discard the unwanted sessions
					if(sessionID > 0 && Util.IsNumber(Util.GetLastPartOfPath(dir.ToString()), false) &&
							Convert.ToInt32(Util.GetLastPartOfPath(dir.ToString())) != sessionID)
					{
						//LogB.Information("Discarded: " + dir.ToString());
						continue;
					}

					BackupSecondDirsLength = diArray.Length;
					BackupSecondDirsCount ++;
					LastSecondDir = Util.GetLastPartOfPath (dir.ToString());
					//LogB.Information("at level 1: " + dir);
				} else if(level == 2)
				{
					//on export, discard the unwanted sessions of multimedia videos
					if(sessionID > 0 && dir.ToString().Contains("multimedia") && dir.ToString().Contains("videos") &&
							Util.IsNumber(Util.GetLastPartOfPath(dir.ToString()), false) &&
							Convert.ToInt32(Util.GetLastPartOfPath(dir.ToString())) != sessionID)
					{
						LogB.Information("Discarded video dir: " + dir.ToString());
						continue;
					}
				}

				//create new dir with try/catch to avoid disk problems (eg. disk full)
				DirectoryInfo newTarget;
				try {
					newTarget = target.CreateSubdirectory(dir.Name);
				} catch {
					return false;
				}

				if(! CopyFilesRecursively(dir, newTarget, level +1))
					return false; //exit if disk problem found in that call
			}

		//copy files with try/catch to avoid disk problems (eg. disk full)
		try {
			foreach (FileInfo file in source.GetFiles())
			{
				if(file.Name == "chronojump_running") 	//do not copy chronojump_running file
					continue;
	
				if (! copyConfig && file.Name.StartsWith ("chronojump_config"))
					continue;

				if (! copyOtherExistingDBs &&
						file.Name.StartsWith ("chronojump") && file.Name.EndsWith (".db") &&
						file.Name != "chronojump.db")
					continue;

				file.CopyTo(Path.Combine(target.FullName, file.Name));
			}
		} catch {
			LogB.Warning("CopyFilesRecursively catched, maybe disk full");
			return false;
		}

		return true;
	}
}

/*
   Manage conversions between 3 booleans (or maybe more) and one int.
   Booleans have weights 1, 2, 4
   eg 6 will be false, false, true
   tested with:
   for(int i = 0; i <= 7; i++)
     LogB.Information(new SessionLoadDisplay(i).ToString());
*/
//see SessionLoadDisplay (right now limited to 3 bits)
public class BooleansInt
{
	protected int i;

	//to inherit
	public BooleansInt()
	{
	}

	public BooleansInt(int i)
	{
		this.i = i;
	}

	public BooleansInt(bool b1, bool b2, bool b3)
	{
		this.i = 0;
		if(b1)
			i ++;
		if(b2)
			i += 2;
		if(b3)
			i += 4;
	}

	public bool Bit3
	{
		get { return (i >= 4); }
	}

	public bool Bit2
	{
		get {
			int temp = i;
			if(temp >= 4)
				temp -= 4;

			return (temp >= 2);
		}
	}

	public bool Bit1
	{
		get {
			int temp = i;
			if(temp >= 4)
				temp -= 4;
			if(temp >= 2)
				temp -= 2;

			return (temp == 1);
		}
	}

	public int GetInt
	{
		get { return i; }
	}

	//just to debug
	public override string ToString()
	{
		return string.Format("i: {0} (bit1: {1}, bit2: {2}, bit3: {3})",
				i, Bit1, Bit2, Bit3);
	}
}

/*
   Used for SQL store (non limited to 3 bits)
   works two sided
   See Test below to know its usage
   */
public class ConvertBooleansInt
{
	public ConvertBooleansInt ()
	{
	}

	public int GetInt (List<bool> bool_l)
	{
		int intValue = 0;

		for (int i = 0 ; i < bool_l.Count; i ++)
			if(bool_l[i])
				intValue += Convert.ToInt32(Math.Pow(2, bool_l.Count -1 - i));

		return intValue;
	}

	//adapted from https://stackoverflow.com/a/49418086
	public List<bool> GetBooleans (int intValue, int sizeBool_l)
	{
		List<bool> bool_l = new List<bool>();
		int pow = 2 * sizeBool_l;

		for (var i = 0; i < sizeBool_l; ++i)
		{
			bool_l.Add(intValue > 0 ? (intValue & pow) != 0 : (intValue & pow) == 0);
			pow /= 2;
		}

		return bool_l;
	}

	public string PrintBooleans(List<bool> bool_l)
	{
		string s = "";
		string sep = "";

		foreach(bool b in bool_l)
		{
			s += sep + b.ToString();
			sep = ", ";
		}

		return s;
	}

	public static void Test()
	{
		ConvertBooleansInt cbi = new ConvertBooleansInt();
		List<bool> bool_l = new List<bool> { true, true, false, true}; //13

		LogB.Information(string.Format("ConvertBooleansInt for values: {0} is: {1}",
					cbi.PrintBooleans(bool_l), cbi.GetInt(bool_l) ));

		int i = 13;
		int iSize = 4; //4 booleans
		LogB.Information(string.Format("ConvertBooleansInt for int: {0} is: {1}",
					i, cbi.PrintBooleans(cbi.GetBooleans(i, iSize)) ));
	}
}
