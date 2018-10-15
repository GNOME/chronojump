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
		string platform = os.Platform.ToString();

		//detect a Mac that seems an Unix
		if(platform == "Unix" && GetOSEnum() == OperatingSystems.MACOSX)
			platform = "Unix (MacOSX)";

		string osString =  string.Format("{0}, {1}", platform, os.Version);

		if(! printedOS) {
			Console.WriteLine("GetOS: " + osString);
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
	
	
	public static string GetApplicationDataDir() {
		return Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"Chronojump");
	}
	
	public static string GetECapSimSignalFileName() {
		return Path.Combine(GetApplicationDataDir() +  Path.DirectorySeparatorChar + "eCapSimSignal.txt");
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

	// ----------- logs -----------------------
	

	public static string GetTempDir() {
		string path = Path.GetTempPath();

		path = path.TrimEnd(Path.DirectorySeparatorChar);
		//on Linux folder cannot be opened either with '/' at the end, or without it

		return path;
	}


}
