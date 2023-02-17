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
using System.IO; 		//for detect OS

public class Log
{
	//1.4.10 have log again by default to all windows users
	//only two logs: current execution log and previous execution log
	//private static TextWriter writer;
	private static bool useConsole;
				

	//on Windows since 1.4.10
	public static void Start() 
	{
		//first define console will be used.
		//if writer is created ok, then console will NOT be used
		useConsole = true;

		//create dir if not exists
		string dir = UtilAll.GetLogsDir ("");
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

	public static void End() 
	{
		if(useConsole)
			System.Console.Out.Close();
		/*
		else {
			try {
				((IDisposable)writer).Dispose();
			} catch {}
		}
		*/
	}

	/*
	 * On Chronojump starts, Log.Start() is called, this copies chronojump_log.txt to chronojump_log_old.txt
	 * a bit later sqliteThings calls: checkIfChronojumpExitAbnormally() and here detects if chrased before
	 * if crashed then the log will be: chronojump_log_old.txt
	 * move this log to crashed logs folder
	 */
	public static void CopyOldToCrashed()
	{
		//create dir if not exists
		string dir = UtilAll.GetLogsCrashedDir();
		if( ! Directory.Exists(dir)) {
			try {
				Directory.CreateDirectory (dir);
			} catch {
				return;
			}
		}

		string filenameOld = UtilAll.GetLogFileOld();

		//if exists, copy to old
		if(File.Exists(filenameOld)) {
			try {
				File.Copy(filenameOld, UtilAll.GetLogCrashedFileTimeStamp(), true); //can be overwritten
			} catch {}
		}
	}


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
