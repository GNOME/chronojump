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
using Glade;
using Mono.Unix;
using System.IO; //"File" things
using System.Threading;
using System.Diagnostics; //Process


public class ChronoJump 
{
	ChronoJumpWindow chronoJumpWin;
	SplashWindow splashWin;
	
	private static string progversion = ""; //now in "version" file
	private static string progname = "Chronojump";
	
	private string runningFileName; //useful for knowing if there are two chronojump instances
	private string messageToShowOnBoot = "";
	private bool chronojumpHasToExit = false;
		
	[Widget] Gtk.Button fakeSplashButton; //raised when splash win ended
	Thread thread;
	bool needEndSplashWin = false;
	bool needUpdateSplashMessage = false;
	string splashMessage = "";
	bool creatingDB = false; //on creation and on update always refresh labels
	bool updatingDB = false;


	public static void Main(string [] args) 
	{
		bool timeLogPassedOk = Log.Start(args);
		Log.WriteLine(string.Format("Time log passed: {0}", timeLogPassedOk.ToString()));
		Log.WriteLine(string.Format("Trying database in ... " + Util.GetDatabaseDir()));
		Log.WriteLine(string.Format("Trying database in ... " + Util.GetDatabaseTempDir()));
		string errorFile = Log.GetFile();

		StreamWriter sw = new StreamWriter(new BufferedStream(new FileStream(errorFile, FileMode.Create)));
		System.Console.SetOut(sw);
		System.Console.SetError(sw);
		sw.AutoFlush = true;

		//this call has to be done to chronojump.prg
		//chronojump.prg createBlankDB
		//this creates a blank database and exists.
		//Then new user will have an updated database without the need of creating in
		if(args.Length > 0 && args[0] == "createBlankDB") {
			createBlankDB();
			Environment.Exit(1);
		}

		Catalog.Init ("chronojump", "./locale");
			
		new ChronoJump(args);
	}

	public ChronoJump (string [] args) 
	{
		
		Application.Init();

		//start threading to show splash window
		SplashWindow splashWin = SplashWindow.Show();
		
		fakeSplashButton = new Gtk.Button();
		fakeSplashButton.Clicked += new EventHandler(on_splash_ended);

		thread = new Thread(new ThreadStart(sqliteThings));
		GLib.Idle.Add (new GLib.IdleHandler (PulseGTK));
		thread.Start(); 
		
		Application.Run();
	}

	protected void sqliteThings () {
		checkIfChronojumpExitAbnormally();
		

		/* SERVER COMMUNICATION TESTS */
		/*
		try {
			ChronojumpServer myServer = new ChronojumpServer();

			//example of list a dir in server
			string [] myListDir = myServer.ListDirectory("/home");
			foreach (string myResult in myListDir) 
				Log.WriteLine(myResult);

			Log.WriteLine(myServer.ConnectDatabase());
			//select name of person with uniqueid 1
			Log.WriteLine(myServer.SelectPersonName(1));
		}
		catch {
			Log.WriteLine("Unable to call server");
		}
		*/
		/* END OF SERVER COMMUNICATION TESTS */

		//print version of chronojump
		Log.WriteLine(string.Format("Chronojump version: {0}", readVersion()));

		//move database to new location if chronojump version is before 0.7
		moveDatabaseToNewLocationIfNeeded();

		splashMessageChange(1);  //checking database

		/*
		splashMessage = "pre-connect";
		needUpdateSplashMessage = true;
		Console.ReadLine();		
		*/
		
		bool defaultDBLocation = Sqlite.Connect();

		/*	
		splashMessage = "post-connect" + defaultDBLocation.ToString();
		needUpdateSplashMessage = true;
		Console.ReadLine();		
		*/
		
		//Chech if the DB file exists
		if (!Sqlite.CheckTables(defaultDBLocation)) {
			Log.WriteLine ( Catalog.GetString ("no tables, creating ...") );

			creatingDB = true;
			splashMessageChange(2);  //creating database



			/*
			splashMessage = "pre-create";
			needUpdateSplashMessage = true;
			Console.ReadLine();		
			*/



			Sqlite.CreateFile();
			//Sqlite.CreateFile(defaultDBLocation);



			/*
			splashMessage = "post-create";
			needUpdateSplashMessage = true;
			Console.ReadLine();		
			*/



			File.Create(runningFileName);
			Sqlite.CreateTables();
			creatingDB = false;
		} else {/*
			//backup the database
			Util.BackupDirCreateIfNeeded();

			splashMessageChange(3);  //making db backup

			Util.BackupDatabase();
			Log.WriteLine ("made a database backup"); //not compressed yet, it seems System.IO.Compression.DeflateStream and
			//System.IO.Compression.GZipStream are not in mono
			*/


			if(! Sqlite.IsSqlite3()) {
				bool ok = Sqlite.ConvertFromSqlite2To3();
				if (!ok) {
					Log.WriteLine("******\n problem with sqlite \n******");
					//check (spanish)
					//http://mail.gnome.org/archives/chronojump-devel-list/2008-March/msg00011.html
					string errorMessage = Catalog.GetString("Failed database conversion, ensure you have libsqlite3-0 installed. \nIf problems persist ask in chronojump-list");
					errorMessage += "\n\n" + string.Format(Catalog.GetString("If you have no data on your database (you just installed Chronojump), you can fix this problem deleting this file: {0}"), 
							Util.GetDatabaseDir() + Path.DirectorySeparatorChar + "chronojump.db") + 
						"\n" + Catalog.GetString("And starting Chronojump again.");
					Log.WriteLine(errorMessage);
					messageToShowOnBoot += errorMessage;
					chronojumpHasToExit = true;
				}
				Sqlite.Connect();
			}

			splashMessageChange(4);  //updating DB
			updatingDB = true;

			bool softwareIsNew = Sqlite.ConvertToLastChronojumpDBVersion();
			updatingDB = false;

			if(! softwareIsNew) {
				//Console.Clear();
				string errorMessage = string.Format(Catalog.GetString ("Sorry, this Chronojump version ({0}) is too old for your database."), readVersion()) + "\n" +  
						Catalog.GetString("Please update Chronojump") + ":\n"; 
				errorMessage += "http://www.gnome.org/projects/chronojump/installation"; 
				//errorMessage += "\n\n" + Catalog.GetString("Press any key");
				Log.WriteLine(errorMessage);
				messageToShowOnBoot += errorMessage;
				chronojumpHasToExit = true;
			}

			Log.WriteLine ( Catalog.GetString ("tables already created") ); 

			//check for bad Rjs (activate if program crashes and you use it in the same db before v.0.41)
			//SqliteJump.FindBadRjs();
		}
		
		
		File.Create(runningFileName);


		splashMessageChange(5);  //preparing main window

		messageToShowOnBoot += recuperateBrokenEvents();

		//start as "simulated"
		SqlitePreferences.Update("simulated", "True", false); //false (dbcon not opened)

		Util.IsWindows();	//only as additional info here
		
		//Application.Init();
		
		needEndSplashWin = true;

	}

	protected void readMessageToStart() {
		if(messageToShowOnBoot.Length > 0) {
			ErrorWindow errorWin;
			if(chronojumpHasToExit) {
				messageToShowOnBoot += "\n<b>" + string.Format(Catalog.GetString("Chronojump will exit now.")) + "</b>\n";
				errorWin = ErrorWindow.Show(messageToShowOnBoot);
				errorWin.Button_accept.Clicked += new EventHandler(on_message_boot_accepted_quit);
//				Application.Run();
			} else { 
				errorWin = ErrorWindow.Show(messageToShowOnBoot);
				errorWin.Button_accept.Clicked += new EventHandler(on_message_boot_accepted_continue);
//				Application.Run();
			}
		} else {
			startChronojump();
//			Application.Run();
		}
	}

	private void on_message_boot_accepted_continue (object o, EventArgs args) {
		startChronojump();
	}

	private void on_message_boot_accepted_quit (object o, EventArgs args) {
		try {
			File.Delete(runningFileName);
		} catch {
			//done because if database dir is moved in a chronojump conversion (eg from before installer to installjammer) maybe it will not find this runningFileName
		}
		System.Console.Out.Close();
		Log.End();
		Log.Delete();
		Application.Quit();
	}

	private void startChronojump() {	
		chronoJumpWin = new ChronoJumpWindow(readVersion(), progname, runningFileName);
	}

	private static void createBlankDB() {
		Console.WriteLine("Creating blank database");
		Sqlite.ConnectBlank();
		Sqlite.CreateFile();
		Sqlite.CreateTables();
		Console.WriteLine("Done! Exiting");
	}

	/* --------------------
	/* splash window things 
	 * --------------------*/

	private void splashMessageChange(int messageInt) {
	       splashMessage = Catalog.GetString(Constants.SplashMessages[messageInt]);
		needUpdateSplashMessage = true;
	}
	
	protected bool PulseGTK ()
	{
		if(needEndSplashWin || ! thread.IsAlive) {
			fakeSplashButton.Click();
			Log.Write("splash window dying here");
			return false;
		}
		//need to do this, if not it crashes because chronopicWin gets died by thread ending
		splashWin = SplashWindow.Show();
		//Log.WriteLine("splash");

		if(updatingDB) {
			splashWin.ShowProgressbar("updating");
			splashWin.UpdateLabel(splashMessage + " " + Sqlite.PrintConversionText());
		
			splashWin.UpdateProgressbar("version", Sqlite.PrintConversionVersion());
			splashWin.UpdateProgressbar("rate", Sqlite.PrintConversionRate());
			splashWin.UpdateProgressbar("subrate", Sqlite.PrintConversionSubRate());

		} else if(creatingDB) {
			splashWin.ShowProgressbar("creating");
			splashWin.UpdateProgressbar("version", Sqlite.PrintCreation());
			
			//splashWin.UpdateProgressbar("rate", Sqlite.PrintConversionRate());
			splashWin.UpdateProgressbar("subrate", Sqlite.PrintConversionSubRate());
		}

		if(needUpdateSplashMessage) {
			splashWin.UpdateLabel(splashMessage);
			needUpdateSplashMessage = false;
		}
			

		Thread.Sleep (50);
		Log.Write(thread.ThreadState.ToString());
		return true;
	}
	
	private void on_splash_ended(object o, EventArgs args) {
		splashWin.Destroy();
		Log.WriteLine("splash screen ENDED!");
		readMessageToStart();
	}

		
	/* ---------------------
	 * other support methods 
	 * ---------------------*/
		
	private void chronojumpCrashedBefore() {

		/*
		string windowsTextLog = "";
			
		string crashLogFile = Log.GetLast().Replace(".txt", "-crash.txt");
		//on vista there's no crash file because redirection is forbidden
		if(Util.IsWindows() && File.Exists(crashLogFile)) 
			windowsTextLog = "\n" + crashLogFile;
			*/

		//if there's a copy on temp...
		if(File.Exists(Util.GetDatabaseTempDir() + Path.DirectorySeparatorChar + "chronojump.db")) {

			// if exist also a file in default db location (improbable but done for solve eventual problems
			if(File.Exists(Util.GetDatabaseDir() + Path.DirectorySeparatorChar + "chronojump.db")) {
				Util.BackupDatabase();//copy it to backup
				File.Delete(Util.GetDatabaseDir() + Path.DirectorySeparatorChar + "chronojump.db"); //delete it
			}

			//move temp dir to default db location dir
			File.Move(Util.GetDatabaseTempDir() + Path.DirectorySeparatorChar + "chronojump.db",
				Util.GetDatabaseDir() + Path.DirectorySeparatorChar + "chronojump.db");
		}


		string errorMessage = "\n" +
				string.Format(Catalog.GetString("Chronojump crashed before. Please, <b>write an email</b> to {0} including this file:"), "xaviblas@gmail.com") + "\n\n" +
					Log.GetLast() +
		//			windowsTextLog +
				       "\n\n" +	
				Catalog.GetString("Subject should be something like \"bug in Chronojump\". Your help is needed.") + "\n";

		/*
		 * This are the only outputs to Console. Other's use Log that prints to console and to log file
		 * this doesn't go to log because it talks about log
		 */
		Log.WriteLine(errorMessage);
		
		messageToShowOnBoot += errorMessage;	
		return;
	}

	//recuperate temp jumpRj or RunI if chronojump hangs
	private string recuperateBrokenEvents() 
	{
		string returnString = "";
		
		string tableName = "tempJumpRj";
		int existsTempData = Sqlite.TempDataExists(tableName);
		if(existsTempData > 0)
		{
			JumpRj myJumpRj = SqliteJumpRj.SelectJumpData("tempJumpRj", existsTempData);
			myJumpRj.InsertAtDB (false, Constants.JumpRjTable);

			Sqlite.DeleteTempEvents(tableName);
			returnString = "Recuperated last Reactive Jump";
		}

		tableName = "tempRunInterval";
		existsTempData = Sqlite.TempDataExists(tableName);
		if(existsTempData > 0)
		{
			RunInterval myRun = SqliteRunInterval.SelectRunData("tempRunInterval", existsTempData);
			myRun.InsertAtDB (false, Constants.RunIntervalTable);

			Sqlite.DeleteTempEvents(tableName);
			returnString = "Recuperated last Intervallic Run";
		}
		
		return returnString;
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
		if(File.Exists(runningFileName)) 
			chronojumpCrashedBefore();
		/*
		else {
			if (Sqlite.CheckTables()) 
				File.Create(runningFileName);
		}
		*/
	}

	//move database to new location if chronojump version is before 0.7
	private void moveDatabaseToNewLocationIfNeeded() 
	{
		string reallyOldDB = Util.GetReallyOldDatabaseDir();
		string oldDB = Util.GetOldDatabaseDir();
		string newDB = Util.GetDatabaseDir();
		string previous = "";
		bool moveNeeded = false;

		if(! Directory.Exists(newDB)) {
			if(Directory.Exists(oldDB)) {
				previous = oldDB;
				moveNeeded = true;
			} else {
				previous = reallyOldDB;
				moveNeeded = true;
			}
		}

		if(moveNeeded) {
			try {
				Directory.Move(previous, newDB);
			}
			catch {
				string feedback = "";
				feedback += string.Format(Catalog.GetString("Cannot move database directory from {0} to {1}"), 
						previous, Path.GetFullPath(newDB)) + "\n";
				feedback += string.Format(Catalog.GetString("Trying to move/copy each file now")) + "\n";

				int fileMoveProblems = 0;
				int fileCopyProblems = 0;

				try {
					Directory.CreateDirectory(newDB);
					Directory.CreateDirectory(newDB + Path.DirectorySeparatorChar + "backup");

					string [] filesToMove = Directory.GetFiles(previous);
					foreach (string oldFile in filesToMove) {
						string newFile = newDB + Path.DirectorySeparatorChar + oldFile.Substring(oldDB.Length);
						try {
							File.Move(oldFile, newFile);
						}
						catch {
							fileMoveProblems ++;
							try {
								Log.WriteLine(string.Format("{0}-{1}", oldFile, newFile));
								File.Copy(oldFile, newFile);
							}
							catch {
								fileCopyProblems ++;
							}
						}
					}

				}
				catch {
					feedback += string.Format(Catalog.GetString("Cannot create directory {0}"), Path.GetFullPath(newDB)) + "\n";
					feedback += string.Format(Catalog.GetString("Please, do it manually.")) + "\n"; 
					feedback += string.Format(Catalog.GetString("Chronojump will exit now.")) + "\n";
					messageToShowOnBoot += feedback;	
					Log.WriteLine(feedback);
					chronojumpHasToExit = true;
				}
				if(fileCopyProblems > 0) {
					feedback += string.Format(Catalog.GetString("Cannot copy {0} files from {1} to {2}"), fileCopyProblems, previous, Path.GetFullPath(newDB)) + "\n";
					feedback += string.Format(Catalog.GetString("Please, do it manually.")) + "\n"; 
					feedback += string.Format(Catalog.GetString("Chronojump will exit now.")) + "\n";
					messageToShowOnBoot += feedback;	
					Log.WriteLine(feedback);
					chronojumpHasToExit = true;
				}
				if(fileMoveProblems > 0) {
					feedback += string.Format(Catalog.GetString("Cannot move {0} files from {1} to {2}"), fileMoveProblems, previous, Path.GetFullPath(newDB)) + "\n";
					feedback += string.Format(Catalog.GetString("Please, do it manually")) + "\n";
					messageToShowOnBoot += feedback;	
					Log.WriteLine(feedback);
				}
			}
					
			string dbMove = string.Format(Catalog.GetString("Database is now here: {0}"), Path.GetFullPath(newDB));
			messageToShowOnBoot += dbMove;	
			Log.WriteLine(dbMove);
		}
	}

}
