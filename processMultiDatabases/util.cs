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
using System.Collections.Generic; //List<T>
using System.IO; //"File" things. TextWriter. Path
using System.Diagnostics; 	//for detect OS and for Process
using System.Text; //StringBuilder

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

	public static string RemoveTildeAndColon(string myString)
	{
		StringBuilder myStringBuilder = new StringBuilder(myString);
		myStringBuilder.Replace("'", " ");
		myStringBuilder.Replace(":", " ");
		return myStringBuilder.ToString();
	}


	/*
	public static string MakeURLabsolute(string url) {
		string parentDir = Util.GetParentDir(true); //add final '/' or '\'
		if( ! url.StartsWith(parentDir) )
			url = parentDir + url;

		return url;
	}
	*/

	public static void DeleteFile(string filename)
	{
		Console.WriteLine("Deleting... " + filename);
		if (File.Exists(filename))
			File.Delete(filename);
		Console.WriteLine("Deleted " + filename);
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

	public static string BoolToRBool (bool myBool) {
		if(myBool)
			return "TRUE";
		else
			return "FALSE";
	}

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
}

