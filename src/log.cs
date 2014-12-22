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
using System.IO; 		//for detect OS

public class Log
{
	/*
	 * writes to screen and to log
	 * timeLog ensures a different log for every chronojump launch
	 * log is deleted if all ends ok.
	 */
	
	//private static TextWriter writer; //writer is not used now, all is thone in the Main (on chronojump.cs).we only need to print to console now (0.7.5)
	//private static string timeLog = "";
	//private static bool useConsole = true; //for the new method on chronojump.cs for redirecting output and error to same file also on windows (0.7.5)
	
	//1.4.10 have log again by default to all windows users
	//only two logs: current execution log and previous execution log
	private static TextWriter writer;
	private static bool useConsole;
				
	/*
	private static bool initializeTime(string [] args) {
		if(! Directory.Exists(GetDir())) {
			try { 
				Directory.CreateDirectory(GetDir());
			} catch {}
		}

		bool timeLogPassedOk = true;
		if(args.Length == 1) 
			timeLog = args[0];
		else {
			timeLog = DateTime.Now.ToString();
			timeLogPassedOk = false;
		}

		return timeLogPassedOk;
	}
	*/

	/*
	public static string GetDir() {
		//we are on:
		//Chronojump/chronojump-x.y/data/
		//we have to go to
		//Chronojump/logs
		//return ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "logs";
		
		//fixing:
		//http://lists.ximian.com/pipermail/mono-list/2008-November/040480.html
		return Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"Chronojump/logs");
	}
	
	public static string GetFile() {
		return Path.GetFullPath(GetDir() + Path.DirectorySeparatorChar + "" + timeLog + ".txt");
	}
	*/
	
//	public static bool Start(string [] args) {
		/* this is ok for define a time that will be the name of the log file
		 * if program ends ok, it will delete that file. 
		 * The problem is that i don't know how to redirect a crash there.
		 */

	       /* a solution is to put on chronojump.sh
		* 
		* LOG_DATE=`date +%d-%m-%Y_%H-%M-%S`
		* LOG_FILE="../../logs/$LOG_DATE.txt"
		* mono chronojump.prg $LOG_FILE 2>>$LOG_FILE
		*
		* if chronojump it's called from .sh (that's the normal thing), then all will work:
		* log of the app and adding the crash log at end
		*
		* if chronojump it's called directly like mono chronojump.prg, 
		* then log filename will be created here, and there will be no redirection on crash 
		*
		* study how to do it in the bat file for windows
		*/

	/*	
		bool timeLogPassedOk = initializeTime(args);
		
		if(useConsole)
			Console.WriteLine(GetFile());
//		try {
//			writer = File.CreateText(GetFile());
//		} catch {}
		
		return timeLogPassedOk;
	}
	*/


	//on Windows since 1.4.10
	public static void Start() 
	{
		//first define console will be used.
		//if writer is created ok, then console will NOT be used
		useConsole = true;

		//create dir if not exists
		string dir = UtilAll.GetLogsDir();
		if( ! Directory.Exists(dir)) {
			try {
				Directory.CreateDirectory (dir);
			} catch {
				return;
			}
		}
		
		string filename = UtilAll.GetLogFileCurrent();
		string filenameOld = UtilAll.GetLogFileOld();

		//if exists, copy to old
		if(File.Exists(filename)) {
			try {
				File.Copy(filename, filenameOld, true); //can be overwritten
			} catch {}
		}

		/*
		try {
			writer = File.CreateText(filename);
			useConsole = false;
		} catch {}
		*/
	

		//this does not write until exit
		//StreamWriter sw = new StreamWriter(new BufferedStream(new FileStream(UtilAll.GetLogFileCurrent(), FileMode.Create)));

		//this writes all the time
		StreamWriter sw = new StreamWriter(new FileStream(UtilAll.GetLogFileCurrent(), FileMode.Create));

		System.Console.SetOut(sw);
		System.Console.SetError(sw);
		sw.AutoFlush = true;
	}

	public static void Write(string text) 
	{
		if(useConsole)
			Console.Write(text);
		else {
			try {
				writer.Write(text);
				writer.Flush();
			} catch {}
		}
	}
	
	public static void WriteLine(string text) 
	{
		if(useConsole) {
			//Console.WriteLine(text);
			LogB.Information(text);
		}
		else {
			try {
			writer.WriteLine(text);
			writer.Flush();
			} catch {}
		}
	}
	
	public static void End() 
	{
		if(useConsole)
			System.Console.Out.Close();
		else {
			try {
				((IDisposable)writer).Dispose();
			} catch {}
		}
	}
	
	//if exit normally, then delete file
	/*
	public static void Delete() {
		try {
			File.Delete(GetFile());
		} catch {}
	}
	*/

	/*
	//GetLast should NOT return the newer log: the just created and empty file, 
	//it should return the just before the last, that's the log of when chronojump crashed last time
	public static string GetLast() {
		string [] files = Directory.GetFiles(GetDir());
		DateTime myTime;

		//last file created (empty log of new chronojump execution)
		DateTime newestTime = new DateTime(1900,1,1); 
		string newEmptyFile = ""; 
		
	       	//file created just before above (in previous chronojump execution)
		DateTime secondNewestTime = new DateTime(1900,1,1);
		string lastLogFile = ""; 

		foreach (string file in files) {
			//check only the files that doesn't end with a "-crash".
			//This comes from windows were we need to separate both logs

			//some windows doesn't allow to .bat to create the crash file as a redirection
			//it seems is because is in another folder
			//do it in same folder (data)
			//then crash file has not to be searched here

//			if(!file.EndsWith("crash.txt")) {
				myTime = File.GetCreationTime(file);

				//if time it's newer
				if(DateTime.Compare(myTime, secondNewestTime) > 0 && DateTime.Compare(myTime, newestTime) > 0) {
					secondNewestTime = newestTime;
					newestTime = myTime;

					lastLogFile = newEmptyFile;
					newEmptyFile = file;
				} else if(DateTime.Compare(myTime, secondNewestTime) > 0) {
					secondNewestTime = myTime;
					lastLogFile = file;
				}
//			}
		}
		//Console.WriteLine("new empty: {0}\n, the log: {1}", newEmptyFile, lastLogFile);

		if(lastLogFile == "")
			return "";
		else
			return Path.GetFullPath(lastLogFile);
	}
	*/
}
