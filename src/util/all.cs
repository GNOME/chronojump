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
using System.Diagnostics; 	//for detect OS
using System.IO; 		//for detect OS
using System.Reflection; // Read Version

//this class tries to be a space for methods that are used in different classes
//in chronojump and chronojump_mini
//we do not use util.cs in mini because it has lot of calls to other files
public class UtilAll
{
	static bool printedOS = false;

	//Eg. 1.6.2.0
	public static string ReadVersion()
	{
		Version version = Assembly.GetExecutingAssembly().GetName().Version;
		return version.ToString();
	}	
	//Eg. 1.6.2-398-gfc1bb50
	public static string ReadVersionFromBuildInfo()
	{
		return BuildInfo.chronojumpVersion;
	}
	

	//Adapted from Mono. A developer's notebook. p 244
	
	//this is used in chronojump for working with the ports,
	//in chronojump we compile now for Linux with Mono and for Windows with .NET
	//it should be something like IsDotNet()
	public static bool IsWindows() {
		string os = GetOS();

		/*
		 * on Turkisk upper i is not always I
		 * "i".uppercased(with: Locale(identifier: "tr_TR")) // returns "İ"
		 * "i".uppercased(with: Locale(identifier: "en_US")) // returns "I"
		 * "ı".uppercased(with: Locale(identifier: "tr_TR")) // returns "I"
		 * so not only check "WIN" Upper, also "win" Lower
		 */
		if(os.ToUpper().StartsWith("WIN") || os.ToLower().StartsWith("win"))
			return true;
		else 
			return false;
	}
	
	public static string GetOS()
	{
		OperatingSystem os = Environment.OSVersion;
		string platform = os.Platform.ToString();

		//detect a Mac that seems an Unix
		if(platform == "Unix" && GetOSEnum() == OperatingSystems.MACOSX)
			platform = "Unix (MacOSX)";
		if (platform == "Unix" && IsChromeOS ())
			platform = "Unix ChromeOS";

		string osString =  string.Format("{0}, {1}", platform, os.Version);

		if(! printedOS) {
			LogB.Information("GetOS: " + osString);
			printedOS = true;
		}
		
		return osString;
	}

	public enum OperatingSystems { WINDOWS, LINUX, MACOSX };

	public static OperatingSystems GetOSEnum() 
	{
		//http://stackoverflow.com/questions/10138040/how-to-detect-properly-windows-linux-mac-operating-systems
		switch (Environment.OSVersion.Platform)
		{
			case PlatformID.Unix:
				// Well, there are chances MacOSX is reported as Unix instead of MacOSX.
				// Instead of platform check, we'll do a feature checks (Mac specific root folders)
				if (Directory.Exists("/Applications")
						& Directory.Exists("/System")
						& Directory.Exists("/Users")
						& Directory.Exists("/Volumes"))
					return OperatingSystems.MACOSX;
				else
					return OperatingSystems.LINUX;

			case PlatformID.MacOSX:
				return OperatingSystems.MACOSX;

			default:
				return OperatingSystems.WINDOWS;
		}
	}

	//check if a Linux is a ChromeOS
	public static bool IsChromeOS ()
	{
		return (File.Exists ("/dev/.cros_milestone"));

	}

	/*
	public static string GetApplicationDataDir() {
		return Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"Chronojump");
	}
	*/
	//if withFinalSeparator, then return a '\' or '/' at the end
	public static string GetDefaultLocalDataDir (bool withFinalSeparator)
	{
		string path = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"Chronojump");

		if(withFinalSeparator)
			path += Path.DirectorySeparatorChar;

		return path;
	}

	// ----------- logs -----------------------
	
	//Since 2.2.2 on Compujump admin LocaApplicationData can change
	//this will check if any config path is present
	public static string GetLogsDir (string pathChangedInConfig)
	{
		if (pathChangedInConfig == "")
			return Path.Combine(
					GetDefaultLocalDataDir (false) + Path.DirectorySeparatorChar + "logs");
		else
			return pathChangedInConfig + Path.DirectorySeparatorChar + "logs";
	}

	public static string GetLogFileCurrent() {
		return Path.Combine(GetLogsDir("") +  Path.DirectorySeparatorChar + Constants.FileNameLog);
	}
	public static string GetLogFileOld() {
		return Path.Combine(GetLogsDir("") +  Path.DirectorySeparatorChar + Constants.FileNameLogOld);
	}

	public static string GetLogsCrashedDir() {
		return Path.Combine(
				GetDefaultLocalDataDir (false) + Path.DirectorySeparatorChar + "logs" +
				Path.DirectorySeparatorChar + "crashed");
	}
	public static string GetLogCrashedFileTimeStamp() {
		return Path.Combine(GetLogsCrashedDir() +  Path.DirectorySeparatorChar +
				"crashed_log_" + UtilDate.ToFile(DateTime.Now) + ".txt");
	}
	
	
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
			detected += Constants.FoundUSBSerialPortsString() + " " + usbSerial.Length + "\n" + startStr;
			foreach(string myPort in usbSerial)
				detected += midStr + myPort;
			detected += endStr;
		} 
		/*
		   else {
			detected += Constants.NotFoundUSBSerialPortsString + "\n";
			string [] serial = Directory.GetFiles("/dev/", "ttyS*");
			detected += Constants.FoundSerialPortsString + " " + serial.Length + "\n" + startStr;
			foreach(string myPort in serial)
				detected += midStr + myPort;
			detected += endStr;
		}
		*/
		return detected;
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


	public static string GetTempDir() {
		string path = Path.GetTempPath();

		path = path.TrimEnd(Path.DirectorySeparatorChar);
		//on Linux folder cannot be opened either with '/' at the end, or without it

		return path;
	}

	//avoids divide by zero
	//thought for being between 0, 1
	//ideal for progressBars
	public static double DivideSafeFraction (double num, double denom)
	{
		if(num == 0 || denom == 0)
			return 0;

		double result = num / denom;

		if(result > 1)
			result = 1;
		else if(result < 0)
			result = 0;

		return result;
	}

	public static double DivideSafeFraction (int val1, int val2) {
		return DivideSafeFraction(Convert.ToDouble(val1), Convert.ToDouble(val2));
	}

	//Not restricted to values 0-1
	public static double DivideSafe (double val1, double val2)
	{
		if(val1 == 0 || val2 == 0)
			return 0;

		return val1 / val2;
	}

	public static int DivideSafeAndGetInt (double val1, double val2)
	{
		return Convert.ToInt32(DivideSafe(val1, val2));
	}

	//here to be able to be called from chronojumpMini
	public static string [] AddArrayString(string [] initialString, string [] addString)
	{
		string [] returnString = new string[initialString.Length + addString.Length];
		int i;
		int j;
		for (i=0 ; i < initialString.Length; i ++)
			returnString[i] = initialString[i];
		for (j=0 ; j < addString.Length; j ++)
			returnString[i+j] = addString[j];

		return returnString;
	}


}
