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
 *  Copyright (C) 2004-2014   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Diagnostics; 	//for detect OS
using System.IO; 		//for detect OS

//this class tries to be a space for methods that are used in different classes
//in chronojump and chronojump_mini
//we do not use util.cs in mini because it has lot of calls to other files
public class UtilAll
{
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

	public enum OperatingSystems { WINDOWS, LINUX, MACOSX };

	public static OperatingSystems GetOSEnum() 
	{
		string os = GetOS();
		if(os.ToUpper().StartsWith("WIN"))
			return OperatingSystems.WINDOWS;
		else if(os.ToUpper().StartsWith("MAC"))
			return OperatingSystems.MACOSX;
		else
			return OperatingSystems.LINUX;
	}

	
	public static string GetApplicationDataDir() {
		return Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"Chronojump");
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
			detected += Constants.FoundUSBSerialPortsString + " " + usbSerial.Length + "\n" + startStr;
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

	// ----------- logs -----------------------
	
	public static string GetLogsDir() {
		return Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"Chronojump" + Path.DirectorySeparatorChar + "logs");
	}
	public static string GetLogFileCurrent() {
		return Path.Combine(GetLogsDir() +  Path.DirectorySeparatorChar + "log_chronojump.txt");
	}
	public static string GetLogFileOld() {
		return Path.Combine(GetLogsDir() +  Path.DirectorySeparatorChar + "log_chronojump_old.txt");
	}

}
