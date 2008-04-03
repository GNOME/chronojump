/*
 * This file is part of ChronoJump
 *
 * Chronojump is free software; you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *   the Free Software Foundation; either version 2 of the License, or   
 *    (at your option) any later version.
 *    
 * Chronojump is distributed in the hope that it will be useful,
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
using Gtk;
using Mono.Unix;
using System.IO; //"File" things


public class ChronoJump 
{
	ChronoJumpWindow chronoJumpWin;
	
	private static string [] authors = {"Xavier de Blas", "Juan Gonzalez", "Juan Fernando Pardo"};
	private static string progversion = ""; //now in "version" file
	private static string progname = "Chronojump";
	
	private string runningFileName; //useful for knowing if there are two chronojump instances


	public static void Main(string [] args) 
	{
		Catalog.Init ("chronojump", "./locale");
		new ChronoJump(args);
	}

	public ChronoJump (string [] args) 
	{
		//works on Linux
		//Console.WriteLine("lang: {0}", System.Environment.GetEnvironmentVariable("LANG"));
		//Console.WriteLine("language: {0}", System.Environment.GetEnvironmentVariable("LANGUAGE"));
	

		/* SERVER COMMUNICATION TESTS */
		/*
		try {
			ChronojumpServer myServer = new ChronojumpServer();

			//example of list a dir in server
			string [] myListDir = myServer.ListDirectory("/home");
			foreach (string myResult in myListDir) 
				Console.WriteLine(myResult);

			Console.WriteLine(myServer.ConnectDatabase());
			//select name of person with uniqueid 1
			Console.WriteLine(myServer.SelectPersonName(1));
		}
		catch {
			Console.WriteLine("Unable to call server");
		}
		*/
		/* END OF SERVER COMMUNICATION TESTS */

		//move database to new location if chronojump version is before 0.7
		moveDatabaseToInstallJammerLocationIfNeeded();

		checkIfChronojumpExitAbnormally();
		
		Sqlite.Connect();

		//isFirstTime we run chronojump in this machine? 
		//(or is there a DB file?)
		bool isFirstTime = false;

		//Chech if the DB file exists
		if (!Sqlite.CheckTables()) {
			Console.WriteLine ( Catalog.GetString ("no tables, creating ...") );
			Sqlite.CreateFile();
			File.Create(runningFileName);
			Sqlite.CreateTables();

			isFirstTime = true;
		} else {
			//backup the database
			Util.BackupDirCreateIfNeeded();

			Util.BackupDatabase();
			Console.WriteLine ("made a database backup"); //not compressed yet, it seems System.IO.Compression.DeflateStream and
			//System.IO.Compression.GZipStream are not in mono

			if(! Sqlite.IsSqlite3()) {
				bool ok = Sqlite.ConvertFromSqlite2To3();
				if (!ok) {
					Console.WriteLine("******\n problem with sqlite \n******");
					//check (spanish)
					//http://mail.gnome.org/archives/chronojump-devel-list/2008-March/msg00011.html
					Console.WriteLine(Catalog.GetString("Failed database conversion, ensure you have libsqlite3-0 installed. \nIf problems persist ask in chronojump-list"));
					Console.ReadLine();
					quitFromConsole();
				}
				Sqlite.Connect();
			}

			bool softwareIsNew = Sqlite.ConvertToLastChronojumpDBVersion();
			if(! softwareIsNew) {
				Console.Clear();
				Console.WriteLine ( 
						string.Format(Catalog.GetString ("Sorry, this Chronojump version ({0}) is too old for your database."), readVersion()) + "\n" +  
						Catalog.GetString("Please update Chronojump") + ":\n" ); 
				Console.WriteLine ( "http://www.gnome.org/projects/chronojump/installation"); 
				Console.WriteLine( "\n\n" + Catalog.GetString("Press any key"));
				Console.ReadKey(true);
				quitFromConsole();
			}

			Console.WriteLine ( Catalog.GetString ("tables already created") ); 

			//check for bad Rjs (activate if program crashes and you use it in the same db before v.0.41)
			//SqliteJump.FindBadRjs();
		}


		string recuperatedString = recuperateBrokenEvents();

		//start as "simulated"
		SqlitePreferences.Update("simulated", "True", false); //false (dbcon not opened)

			
		//we need to connect sqlite to do the languageChange
		//change language works on windows. On Linux let's change the locale
		//if(Util.IsWindows()) 
		//	languageChange();

		
		Application.Init();

		Util.IsWindows();	//only as additional info here


		/*
		//if firstTime on windows, then ask for the language
		if(Util.IsWindows() && isFirstTime) {
			//show language dialog (only first time)
			LanguageWindow languageWin = LanguageWindow.Show();

			languageWin.ButtonAccept.Clicked += new EventHandler(on_language_clicked);
			Application.Run();
		} else {
		*/
			chronoJumpWin = new ChronoJumpWindow(recuperatedString, isFirstTime, 
					authors, readVersion(), progname, runningFileName);
			Application.Run();
			/*
		}
		*/
	}

	private bool askContinueChronojump() {
		Console.Clear();
		Console.WriteLine(Catalog.GetString("Chronojump is already running (program opened two times) or it crashed before"));
		Console.WriteLine("\n" +
				string.Format(Catalog.GetString("Please, if crashed, write an email to {0} including what you done when Chronojump crashed."), "xaviblas@gmail.com") + "\n" +
				Catalog.GetString("Subject should be something like \"bug in Chronojump\". Your help is needed.")
				);

		bool success = false;
		bool launchChronojump = true;
		ConsoleKeyInfo myKey;
		do {
			Console.WriteLine("\n" + Catalog.GetString("Please press key") + ":");
			Console.WriteLine("[ Q " + Catalog.GetString("or") + " q ] " + 
					Catalog.GetString("to exit program if it's already opened"));
			Console.WriteLine("[ Y " + Catalog.GetString("or") + " y ] " + 
					Catalog.GetString("to launch Chronojump"));

			myKey = Console.ReadKey(true);

			if(myKey.KeyChar == 'Q' || myKey.KeyChar == 'q') {
				Console.WriteLine("Quit");
				launchChronojump = false;
				success = true;
			} else if(myKey.KeyChar == 'Y' || myKey.KeyChar == 'y') {
				Console.WriteLine("Launch Chronojump");
				launchChronojump = true;
				success = true;
			}
		} while (!success);

		return launchChronojump;
	}

	//recuperate temp jumpRj or RunI if chronojump hangs
	private string recuperateBrokenEvents() 
	{
		string returnString = "";
		
		string tableName = "tempJumpRj";
		int existsTempData = Sqlite.TempDataExists(tableName);
		if(existsTempData > 0)
		{
			JumpRj myJump = SqliteJump.SelectRjJumpData("tempJumpRj", existsTempData);
			SqliteJump.InsertRj("jumpRj", "NULL", myJump.PersonID, myJump.SessionID,
					myJump.Type, myJump.TvMax, myJump.TcMax, 
					myJump.Fall, myJump.Weight, "", //fall, weight, description
					myJump.TvAvg, myJump.TcAvg,
					myJump.TvString, myJump.TcString,
					myJump.Jumps, Util.GetTotalTime(myJump.TcString, myJump.TvString), myJump.Limited
					);

			Sqlite.DeleteTempEvents(tableName);
			returnString = "Recuperated last Reactive Jump";
		}

		tableName = "tempRunInterval";
		existsTempData = Sqlite.TempDataExists(tableName);
		if(existsTempData > 0)
		{
			RunInterval myRun = SqliteRun.SelectIntervalRunData("tempRunInterval", existsTempData);
			SqliteRun.InsertInterval("runInterval", "NULL", myRun.PersonID, myRun.SessionID,
					myRun.Type, myRun.DistanceTotal, myRun.TimeTotal, 
					myRun.DistanceInterval, myRun.IntervalTimesString,  
					myRun.Tracks, "", //description
					myRun.Limited
					);

			Sqlite.DeleteTempEvents(tableName);
			returnString = "Recuperated last Intervallic Run";
		}
		
		return returnString;
	}

	private void quitFromConsole() {
		try {
			File.Delete(runningFileName);
		} catch {
			//done because if database dir is moved in a chronojump conversion (eg from before installer to installjammer) maybe it will not find this runningFileName
		}
		Environment.Exit(1);
	}

	private string readVersion() {
		string version = "";
		try  {
			StreamReader reader = File.OpenText(Constants.FileNameVersion);
			version = reader.ReadToEnd();
			reader.Close();

			//delete the '\n' that ReaderToEnd() has put
			version = version.TrimEnd(new char[1] {'\n'});
		} catch {
			version = "not available";
		}
		return version;
	}	
		
	private void checkIfChronojumpExitAbnormally() {
		runningFileName = Util.GetDatabaseDir() + Path.DirectorySeparatorChar + "chronojump_running";
		if(File.Exists(runningFileName)) {
			bool continueChronojump = askContinueChronojump();
			if(!continueChronojump)
				quitFromConsole();
		} else {
			if (Sqlite.CheckTables()) {
				File.Create(runningFileName);
			}
		}
	}

	//move database to new location if chronojump version is before 0.7
	private void moveDatabaseToInstallJammerLocationIfNeeded() {
		string oldDB = Util.GetOldDatabaseDir();
		string newDB = Util.GetDatabaseDir();
		string problemsMoving = string.Format(Catalog.GetString("Problems moving database from {0} to {1}\n Please, do it manually and the execute Chronojump.\n\n Chronojump will exit."), oldDB, newDB);
	
		if(Directory.Exists(oldDB)) {
			//check if there's anything on newDB, and if there's anything move it to a newly created "old" dir
			//this doesn't work because there's no Directory.Copy. maybe it's done by File.Copy, ?
			//with a move has no sense because it's the same problem (newDB is already created)
			/*
			if(Directory.Exists(newDB)) {
				try {
					Directory.Copy(newDB, newDB + "-old-" + DateTime.Now.ToString()); 
				}
				catch {
					Console.WriteLine(problemsMoving);
					quitFromConsole();
				}
			}
			*/
			
			//move db from oldDB to newDB
			try {
				Directory.Move(oldDB, newDB); 
			}
			catch {
				Console.WriteLine(problemsMoving);
				quitFromConsole();
			}
			Console.WriteLine(string.Format(Catalog.GetString("Database moved from {0} to {1}"), oldDB, newDB));
		}
	}

}
