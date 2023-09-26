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
 *  Copyright (C) 2004-2023   Xavier de Blas <xaviblas@gmail.com>
 */


using System;
using Gtk;
//using Glade;
using Mono.Unix;
using System.IO; //"File" things
using System.Threading;
using System.Diagnostics; //Process

using System.Collections; //ArrayList

using System.Runtime.InteropServices;


public class ChronoJump 
{
	Gtk.Button fakeSplashButton; //raised when splash win ended
	SplashWindow splashWin;
	
	private static string progVersion = ""; //now in "version" file
	private static string progName = "Chronojump";
	private static UtilAll.OperatingSystems operatingSystem;
	
	private string runningFileName; //useful for knowing if there are two chronojump instances
	private string messageToShowOnBoot = "";
	private string messageCrashedBefore = "";
	private bool chronojumpHasToExit = false;
	private Config configChronojump;

	Thread thread;
	bool needEndSplashWin = false;
	bool needUpdateSplashMessage = false;
	string splashMessage = "";
	bool creatingDB = false; //on creation and on update always refresh labels
	bool updatingDB = false;
	private static string baseDirectory;
	private static bool debugModeAtStart;


#if OSTYPE_WINDOWS
	[DllImport("libglib-2.0-0.dll") /* willfully unmapped */ ]
	static extern bool g_setenv (String env, String val, bool overwrite);
#endif

	public static void Main(string [] args)
	{
		//record GetOsEnum on variables to not call it all the time
		operatingSystem = UtilAll.GetOSEnum();
		Util.operatingSystem = operatingSystem;

		//show version on console and exit before the starting logs
		//note version, version2 args are available since: 2.2.0-112-ga4eaadcbc
		if(args.Length > 0 && args[0] != "simulatedCapture" && args[0] != "printAll" && args[0] != "debug")
		{
			string helpMessage = "Execute 'chronojump' or 'chronojump option'" +
				"\nOptions:" +
				"\n- version: Version of the software and the DB" +
				"\n- version2: Version of the software" +
				"\n- configAll: All possible options on chronojump_config.txt" +
				"\n- configDefined: Correctly defined options on chronojump_config.txt" +
				"\n- simulatedCapture (**): Capture is simulated. Working on encoder, forceSensor, and Race Analyzer (in Race Analyzer needs Arduino)" +
				"\n- printAll: (*)" +
				"\n- debug: (*)" +
				"\n- help: this help" +
				"\n\n(*) printAll and debug are only for debug purposes." +
				"\nprintAll prints all threads at the same time (and is normal that it shows an error on Chronojump exit)" +
				"\ndebug ensures logs are printed while capturing on encoder, forceSensor, raceAnalyzer and while using the python importer." +
				"\ndebug will call also printAll, but if you want to know errors on Chronojump start is better to use printAll because it will act from the beginning." +
				"\n(**) simulatedCapture can be combined with printAll or debug, passing simulatedCapture as 1st parameter and the other as 2nd";

			if(args[0] == "version")
			{
				Console.WriteLine("Chronojump version: " + BuildInfo.chronojumpVersion);
				if(operatingSystem == UtilAll.OperatingSystems.LINUX)
					Console.WriteLine("Note in Linux is not reliable if ./autogen.sh was not un before make");

				if(File.Exists(System.IO.Path.Combine(Util.GetDatabaseDir(), "chronojump.db"))) {
					try {
						Sqlite.Connect();
						Console.WriteLine("Chronojump DB version: " +
								SqlitePreferences.Select("databaseVersion", false));
						Sqlite.DisConnect();
					}
					catch {
						Console.WriteLine("Cannot check DB version, failed checking");
						Sqlite.DisConnect();
					}
				} else
					Console.WriteLine("Cannot check DB version, chronojump.db not found on: " + System.IO.Path.Combine(Util.GetDatabaseDir()));
			}
			else if (args[0] == "version2")
			{
				Console.WriteLine(BuildInfo.chronojumpVersion.ToString());
			}
			else if (args[0] == "configAll" || args[0] == "configDefined")
			{
				Config cc = new Config();
				cc.Read ();

				if (args[0] == "configAll")
					Console.WriteLine (cc.PrintAll ());
				else if (args[0] == "configDefined")
					Console.WriteLine (cc.PrintDefined ());
			}
			else if (args[0] == "help")
			{
				Console.WriteLine (helpMessage);
			} else {
				Console.WriteLine (string.Format ("Option incorrect: {0}", args[0]));
				Console.WriteLine (helpMessage);
			}

			return;
		}

		/*
		bool timeLogPassedOk = Log.Start(args);
		Log.WriteLine(string.Format("Time log passed: {0}", timeLogPassedOk.ToString()));
		Log.WriteLine(string.Format("Client database option 1 in … " + Util.GetDatabaseDir()));
		Log.WriteLine(string.Format("Client database option 2 in … " + Util.GetDatabaseTempDir()));
		*/
		
		LogSync.Initialize();
		//1.4.10
		Log.Start();
		LogB.Debugging = true; //now LogB.Debug will be shown. Also there will be thread info on Warning, Error, Information
		if(args.Length > 0)
		{
			//note this allows to call: 'chronojump simulatedCapture debug'
			if (args[0] == "simulatedCapture")
				Config.SimulatedCapture = true;
			if (args[0] == "printAll" || (args.Length > 1 && args[1] == "printAll"))
				LogB.PrintAllThreads = true;
			else if (args[0] == "debug" || (args.Length > 1 && args[1] == "debug"))
				debugModeAtStart = true;
		}

		var envPath = Environment.GetEnvironmentVariable ("PATH");
		var rBinPath = "";

		//we need to set Util.operatingSytem before GetPrefixDir()
		baseDirectory = Util.GetPrefixDir();

		/*
		 * location of gtkrc file
		 * DISABLED tight now because on windows there are inestabilities on jumps results
		 * and on Mode menu
		 */
		//Rc.AddDefaultFile (Util.GetThemeFile());
		//LogB.Information("gtk theme:" + Util.GetThemeFile());

		if(UtilAll.IsWindows()) {
			//Environment.SetEnvironmentVariable ("R_HOME", RelativeToPrefix ("library"));
			//rBinPath = RelativeToPrefix ("lib");
			//rBinPath = RelativeToPrefix ("library");
			//var rPath = System.Environment.Is64BitProcess ? @"C:\Program Files\R\R-3.0.2\bin\x64" : @"C:\Program Files\R\R-3.0.2\bin\i386";
			string x64 = "bin" + System.IO.Path.DirectorySeparatorChar + "x64";
			string i386 = "bin" + System.IO.Path.DirectorySeparatorChar + "i386";
			var rPath = System.Environment.Is64BitProcess ? 
				System.IO.Path.Combine(baseDirectory, x64) : System.IO.Path.Combine(baseDirectory, i386);

			if (Directory.Exists(rPath) == false) {
				LogB.Error("Could not found the specified path to the directory containing R.dll: ", rPath);
				throw new DirectoryNotFoundException(string.Format("Could not found the specified path to the directory containing R.dll: {0}", rPath));
			}

			var newPath = string.Format("{0}{1}{2}", rPath, System.IO.Path.PathSeparator, envPath);
			LogB.Information("newPath:", newPath);
		
			System.Environment.SetEnvironmentVariable("PATH", newPath);
			LogB.Information("path:", System.Environment.GetEnvironmentVariable("PATH"));
			
			//use this because we don't want to look at the registry
			//we don't want to force user to install R
			Environment.SetEnvironmentVariable ("R_HOME", baseDirectory);
			LogB.Information("R_HOME:", baseDirectory);
		} else {
			switch (operatingSystem) {
				case UtilAll.OperatingSystems.MACOSX:
					LogB.Information(Environment.GetEnvironmentVariable("R_HOME"));
					rBinPath = "/Library/Frameworks/R.Framework/Libraries";
						Environment.SetEnvironmentVariable ("R_HOME", "/Library/Frameworks/R.Framework/Resources");
						Environment.SetEnvironmentVariable("PATH", rBinPath + Path.PathSeparator + envPath);
					LogB.Information("environments");
					LogB.Information(Environment.GetEnvironmentVariable("R_HOME"));
					LogB.Information(Environment.GetEnvironmentVariable("PATH"));

					/*
					//Gstreamer stuff (right now not used, we used ffplay)
					string prefix="/Applications/Chronojump.app/Contents/Home/";
					Environment.SetEnvironmentVariable ("GST_PLUGIN_PATH", prefix + "lib/gstreamer-0.10");
					Environment.SetEnvironmentVariable ("GST_PLUGIN_SYSTEM_PATH", prefix + "lib/gstreamer-0.10");
					Environment.SetEnvironmentVariable ("GST_PLUGIN_SCANNER_PATH", prefix + "lib/gstreamer-0.10/gst-plugin-scanner");
					*/
					break;
				case UtilAll.OperatingSystems.LINUX:
					rBinPath = @"/usr/lib/R/lib";
					Environment.SetEnvironmentVariable ("R_HOME", @"/usr/lib/R");
					Environment.SetEnvironmentVariable("PATH", envPath + Path.PathSeparator + rBinPath);
					break;
			}
		}
		
		LogB.Information("Platform:" + Environment.OSVersion.Platform);
		
		LogB.Information("baseDir0:", System.AppDomain.CurrentDomain.BaseDirectory);
		LogB.Information("baseDir1:", baseDirectory);
		LogB.Information("envPath+rBinPath:", envPath + Path.PathSeparator + rBinPath);


		//UtilCSV.ReadValues("/tmp/chronojump-encoder-graph-input-multi.csv");	
		
		if(UtilAll.IsWindows())
			Environment.SetEnvironmentVariable("GST_PLUGIN_PATH",RelativeToPrefix("lib\\gstreamer-0.10"));

		//this call has to be done to chronojump.prg
		//chronojump.prg createBlankDB
		//this creates a blank database and exists.
		//Then new user will have an updated database without the need of creating in
		if(args.Length > 0 && args[0] == "createBlankDB") {
			createBlankDB();
			Environment.Exit(1);
		}

		if(args.Length > 0 && args[0] == "createBlankDBServer") {
			createBlankDBServer();
			Environment.Exit(1);
		}


	string language = "";
	if(File.Exists(System.IO.Path.Combine(Util.GetDatabaseDir(), "chronojump.db"))) {
		try {
			Sqlite.Connect();

			/*
			 * chronojump 1.5.2 converts DB 1.24 to 1.25 changing language to ""
			 * but this operation is done later (on sqliteThings)
			 * We need here! to define the language from the beginning
			 * so we use language = "" if version is prior to 1.25
			 */
			string currentDBVersion = SqlitePreferences.Select("databaseVersion", false);
			double currentDBVersionDouble = Convert.ToDouble(Util.ChangeDecimalSeparator(currentDBVersion));
			if(currentDBVersionDouble < Convert.ToDouble(Util.ChangeDecimalSeparator("1.25")))
				language = "";
			else
				language = SqlitePreferences.Select("language", false);

			Sqlite.DisConnect();
	
			if(language != "") {
				//convert pt-BR to pt_BR
				if(language.Contains("-"))
					language = language.Replace("-", "_");

				Environment.SetEnvironmentVariable ("LANGUAGE", language); //works
#if OSTYPE_WINDOWS
				g_setenv ("LANGUAGE", language, true);
#endif
			}
		}
		catch {
			LogB.Warning("Problem reading language on start");
		}
	}

	Catalog.Init("chronojump",System.IO.Path.Combine(Util.GetPrefixDir(),"share/locale"));

	new ChronoJump(args);
	}

	public static string RelativeToPrefix(string relativePath) {
		return System.IO.Path.Combine(baseDirectory, relativePath);
	}

	public ChronoJump (string [] args) 
	{
		Application.Init();

		//start threading to show splash window
		SplashWindow.Show();

		fakeSplashButton = new Gtk.Button();
		fakeSplashButton.Clicked += new EventHandler(on_splash_ended);

		//LongoMatch.Video.Capturer.GstCameraCapturer.InitBackend("");

		thread = new Thread(new ThreadStart(sqliteThings));
		GLib.Idle.Add (new GLib.IdleHandler (PulseGTK));
		LogB.ThreadStart();
		thread.Start(); 

		Application.Run();
	}

	//variables to manage the ping thread
	string versionAvailable;
	//bool pingStart;
	//bool pingEnd;
	bool allSQLCallsDoneOnSqliteThingsThread;

	//used when Chronojump is being running two or more times (quadriple-click on start)
	bool quitNowCjTwoTimes = false;

	protected void sqliteThings ()
	{
		configChronojump = new Config();
		configChronojump.Read ();
		configChronojump.PrintDefined ();

		bool badExit = checkIfChronojumpExitAbnormally();
		if(badExit) {
			if(chronojumpIsExecutingNTimes())
			{
				messageToShowOnBoot += Catalog.GetString("Chronojump is already running") + "\n\n" +
					Catalog.GetString("Chronojump will exit now.");

				chronojumpHasToExit = true;
				quitNowCjTwoTimes = true;
				LogB.Error("Chronojump is already running.");

				return;
			}
			else
				chronojumpCrashedBefore();
		}

		//print version of chronojump
		progVersion = BuildInfo.chronojumpVersion;

		LogB.Information("Chronojump version: ", progVersion);

		//to store user videos and photos
		Util.CreateMultimediaDirsIfNeeded();

		//to store (encoder, force sensor, run encoder) data and graphs
		UtilEncoder.CreateEncoderDirIfNeeded();
		Util.CreateForceSensorDirIfNeeded();
		Util.CreateRunEncoderDirIfNeeded();
		News.CreateNewsDirIfNeeded();
		ExerciseImage.CreateDirsIfNeeded ();

//TODO: when a session is deleted, encoder data has to be deleted, also multimedia videos, I suppose. Show message to user warning about it
//TODO: encoder weight auto written depending on person loaded, and changes if it changes person or weight

		
		//move database to new location if chronojump version is before 0.7
		moveDatabaseToNewLocationIfNeeded();

		LogB.Information("move? ended");

		splashMessageChange(1);  //checking database

		/*
		splashMessage = "pre-connect";
		needUpdateSplashMessage = true;
		Console.ReadLine();		
		*/
		
		Sqlite.CreateDir();
		bool defaultDBLocation = Sqlite.Connect();

		LogB.SQL("sqlite connected");

		/*	
		splashMessage = "post-connect" + defaultDBLocation.ToString();
		needUpdateSplashMessage = true;
		Console.ReadLine();		
		*/
		
		//Chech if the DB file exists
		if (!Sqlite.CheckTables(defaultDBLocation)) {
			LogB.SQL ( Catalog.GetString ("no tables, creating …") );

			creatingDB = true;
			splashMessageChange(2);  //creating database



			/*
			splashMessage = "pre-create";
			needUpdateSplashMessage = true;
			Console.ReadLine();		
			*/



			Sqlite.CreateDir();
			Sqlite.CreateFile();
			//Sqlite.CreateFile(defaultDBLocation);



			/*
			splashMessage = "post-create";
			needUpdateSplashMessage = true;
			Console.ReadLine();		
			*/



			createRunningFileName(runningFileName);
			Sqlite.CreateTables(false); //not server
			creatingDB = false;
		} else {
			if(! Sqlite.IsSqlite3()) {
				bool ok = Sqlite.ConvertFromSqlite2To3();
				if (!ok) {
					LogB.Error("problem with sqlite");
					//check (spanish)
					//http://mail.gnome.org/archives/chronojump-devel-list/2008-March/msg00011.html
					string errorMessage = Catalog.GetString("Failed database conversion, ensure you have libsqlite3-0 installed.");
					errorMessage += "\n\n" + string.Format(Catalog.GetString("If you have no data on your database (you just installed Chronojump), you can fix this problem deleting this file: {0}"), 
							Util.GetDatabaseDir() + Path.DirectorySeparatorChar + "chronojump.db") + 
						"\n" + Catalog.GetString("And starting Chronojump again.");
					LogB.Error(errorMessage);
					messageToShowOnBoot += errorMessage;
					chronojumpHasToExit = true;
					return;
				}
				Sqlite.Connect();
			}

			splashMessageChange(4);  //updating DB
			updatingDB = true;

			if(Sqlite.ChangeDjToDJna())
				messageToShowOnBoot += Catalog.GetString("All DJ jumps have been renamed as 'DJna' (Drop Jumps with No Arms).") + "\n\n"+ 
					Catalog.GetString("If your Drop Jumps were executed using the arms, please rename them manually as 'DJa'.") + "\n";

			Sqlite.UpdatingDBFrom = Sqlite.UpdatingDBFromEnum.LOCAL;

			bool softwareIsNew = Sqlite.ConvertToLastChronojumpDBVersion();
			updatingDB = false;
			
				
			if(! softwareIsNew) {
				//Console.Clear();
				string errorMessage = string.Format(Catalog.GetString ("Sorry, this Chronojump version ({0}) is too old for your database."), progVersion) + "\n" +  
						Catalog.GetString("Please update Chronojump") + ":\n"; 
				errorMessage += "http://chronojump.org"; 
				//errorMessage += "\n\n" + Catalog.GetString("Press any key");
				LogB.Error(errorMessage);
				messageToShowOnBoot += errorMessage;
				chronojumpHasToExit = true;
			}

			LogB.Information ( Catalog.GetString ("tables already created") ); 
		

			//check for bad Rjs (activate if program crashes and you use it in the same db before v.0.41)
			//SqliteJump.FindBadRjs();
		
			createRunningFileName(runningFileName);
		}
		

		//splashMessageChange(5);  //check for new version
		splashMessageChange(5);  //connecting to server

		messageToShowOnBoot += recuperateBrokenEvents();

		//connect to server to Ping
		versionAvailable = "";
		versionAvailable = Constants.ServerOfflineStr();
	

		//doing ping using json methods
		/*
		 * temporarily disabled on start
		string machineID = SqlitePreferences.Select("machineID", false);
		Json js = new Json();
		bool success = js.Ping(UtilAll.GetOS(), progVersion, machineID);
		if(success)
			LogB.Information(js.ResultMessage);
		else
			LogB.Error(js.ResultMessage);
			*/

		allSQLCallsDoneOnSqliteThingsThread = false;
			
		//wait until pinging ends (or it's cancelled)
		//while(! pingEnd) {
		//}

		Sqlite.Open();

//TODO: fix this to the new code

		string versionAvailableKnown = SqlitePreferences.Select("versionAvailable", true);
		if( versionAvailable != Constants.ServerOfflineStr() && new Version(versionAvailable) > new Version(progVersion) ) {
			//check if available version is higher than known available version
			Version versionAvailableAsV = new Version(versionAvailable);

			Version versionAvailableKnownAsV;
			bool updateKnownVersion = false;
			if(versionAvailableKnown == "")
				updateKnownVersion = true;
			else {
				versionAvailableKnownAsV = new Version(versionAvailableKnown);
				if(versionAvailableAsV > versionAvailableKnownAsV)
					updateKnownVersion = true;
			}

			if(updateKnownVersion) {
				//is the first time we know about this new version
				//just write on db and show message to user
				SqlitePreferences.Update(Constants.PrefVersionAvailable, versionAvailable, true);
				versionAvailableKnown = versionAvailable;
				messageToShowOnBoot += string.Format(Catalog.GetString(
							"\nNew Chronojump version available on website.\nYour Chronojump version is: {1}"), 
						versionAvailable, progVersion) + "\n\n" + 
					Catalog.GetString("Please, update to new version.") + "\n";
			}
		}


		//if chronojump chrashed before
		if(badExit) {
			if( versionAvailableKnown.Length > 0 && new Version(versionAvailableKnown) > new Version(progVersion) ) 
				messageToShowOnBoot += "\n" + Catalog.GetString("Chronojump crashed before.") + "\n" +
				       Catalog.GetString("Please, update to new version: ") + versionAvailableKnown + "\n";
			else {
				messageToShowOnBoot += messageCrashedBefore;
				//SqlitePreferences.Update("videoOn", "False", true);
			}
		}
		
		
		splashMessageChange(6);  //preparing main window
		

		//start as "simulated"
		SqlitePreferences.Update("simulated", "True", true); //dbcon opened

		Sqlite.Close();

		// Chronojump sqlite is in an initialized state, let's keep the Sqlite state here
		// to be re-used
		Sqlite.saveClassState ();

		allSQLCallsDoneOnSqliteThingsThread = true;
		LogB.SQL("all SQL calls done on sqliteThings thread");
		
		UtilAll.IsWindows();	//only as additional info here
		
		//Application.Init();
		
		needEndSplashWin = true;

	}

	private void on_find_version_cancelled(object o, EventArgs args) {
		versionAvailable = Constants.ServerOfflineStr();
		//pingEnd = true;
	}

	protected void readMessageToStart()
	{
		if(messageToShowOnBoot.Length > 0)
		{
			if(chronojumpHasToExit)
			{
				if(quitNowCjTwoTimes) {
					splashWin.UpdateLabel(messageToShowOnBoot);
					splashWin.FakeButtonClose.Clicked += new EventHandler(on_message_boot_accepted_quit_not_deleting_runningfilename);
				} else {
					messageToShowOnBoot += "\n<b>" + string.Format(Catalog.GetString("Chronojump will exit now.")) + "</b>\n";
					splashWin.UpdateLabel(messageToShowOnBoot);
					splashWin.Show_button_open_database_folder();
					splashWin.Button_close.Clicked += new EventHandler(on_message_boot_accepted_quit_nice);
				}
				splashWin.ShowButtonClose();
			} else {
				if(configChronojump.Compujump || configChronojump.Raspberry)
					startChronojump(false); //don't sendLog
				else
					startChronojump(true); //sendLog
			}
		} else {
			startChronojump(false);
		}
	}

	private void on_message_boot_accepted_continue (object o, EventArgs args)
	{
		startChronojump(false);
	}

	private void on_message_boot_accepted_quit_nice (object o, EventArgs args)
	{
		quitChronojump(true);
	}
	private void on_message_boot_accepted_quit_not_deleting_runningfilename (object o, EventArgs args)
	{
		quitChronojump(false);
	}
	private void quitChronojump(bool deleteRunningFileName)
	{
		if(deleteRunningFileName)
		{
			try {
				File.Delete(runningFileName);
			} catch {
				//done because if database dir is moved in a chronojump conversion
				//(eg from before installer to installjammer) maybe it will not find this runningFileName
			}
		}

		if(splashWin != null)
			splashWin.Destroy();
		else
			SplashWindow.Hide();

		if(! quitNowCjTwoTimes)
			Log.End();

		Application.Quit();
	}


	private void startChronojump(bool sendLog) {

		//wait until all sql calls are done in other thread
		//then there will be no more a try to open an already opened dbcon
		LogB.SQL("Checking if all SQL calls done on sqliteThings thread");
		while(! allSQLCallsDoneOnSqliteThingsThread) {
		}
		LogB.SQL("all SQL done! starting Chronojump");

		string topMessage = "";
		if(operatingSystem == UtilAll.OperatingSystems.LINUX && ! linuxUserHasPermissions())
			topMessage = Catalog.GetString("Need permissions to read from device.") + "\n" +
				Catalog.GetString("Check software page on Chronojump website");

		bool showCameraStop = (ExecuteProcess.IsRunning3 (-1, WebcamFfmpeg.GetExecutableCapture(operatingSystem)));

		new ChronoJumpWindow(progVersion, progName, runningFileName, splashWin,
				sendLog, messageToShowOnBoot, topMessage, showCameraStop, debugModeAtStart);
	}

	private bool linuxUserHasPermissions ()
	{
		LogB.Information("Finding dialout or uucp:");
		string executable = "groups";
		ExecuteProcess.Result execute_result = ExecuteProcess.run (executable, true, true);
		if(execute_result.success)
		{
			LogB.Information(execute_result.stdout);
			return (execute_result.stdout.Contains("dialout") || //debian based
					execute_result.stdout.Contains("uucp")); //arch/manjaro
		}

		//if script has failed, don't bother the user
		return true;
	}


	private static void createBlankDB() {
		LogB.SQL("Creating blank database");
		Sqlite.ConnectBlank();
		Sqlite.CreateFile();
		Sqlite.CreateTables(false); //not server
		LogB.SQL("Created blank database! Exiting");
	}
	
	private static void createBlankDBServer() {
		LogB.SQL("Creating blank database for server");
		if(Sqlite.CheckFileServer())
			LogB.Error("File already exists. Cannot create.");
		else {
			Sqlite.ConnectServer();
			Sqlite.CreateFile();
			Sqlite.CreateTables(true); //server
			LogB.SQL("Created blank database! Exiting");
			string myVersion = UtilAll.ReadVersion();
			LogB.Warning("CAUTION: client info about versionAvailable (on server): ", myVersion);
			SqlitePreferences.Update ("availableVersion", myVersion, false); 
			LogB.Information("Maybe you don't want to show this version on pings, change it to last stable published version");
		}
	}


	/* --------------------
	/* splash window things 
	 * --------------------*/

	private void splashMessageChange(int messageInt)
	{
	       splashMessage = Catalog.GetString(Constants.SplashMessages[messageInt]);
	       needUpdateSplashMessage = true;
	       System.Threading.Thread.Sleep(50);
	}
	
	protected bool PulseGTK ()
	{
		if( quitNowCjTwoTimes || needEndSplashWin || ! thread.IsAlive )
		{
			LogB.Information(string.Format("pulseGTK ending conditions: {0}, {1}, {2}",
						quitNowCjTwoTimes, needEndSplashWin, ! thread.IsAlive));
			LogB.ThreadEnding();
			fakeSplashButton.Click();

			LogB.Information("splash window ending here");
			LogB.ThreadEnded();
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
		//Log.Write(" (PulseGTK:" + thread.ThreadState.ToString() + ") ");
		return true;
	}
	
	private void on_splash_ended(object o, EventArgs args)
	{
		LogB.Information("splash screen going to END");
		fakeSplashButton.Clicked -= new EventHandler(on_splash_ended);

		LogB.Information("splash screen ENDED!");

		readMessageToStart();
	}

	
	/* ---------------------
	 * other support methods 
	 * ---------------------*/
	
	private void chronojumpCrashedBefore() 
	{
		Log.CopyOldToCrashed();

		/*
		  string windowsTextLog = "";
		  
		  string crashLogFile = Log.GetLast().Replace(".txt", "-crash.txt");
		//on vista there's no crash file because redirection is forbidden
		if(UtilAll.IsWindows() && File.Exists(crashLogFile)) 
		windowsTextLog = "\n" + crashLogFile;
		*/
			
		//if there's a copy on temp …
		if(File.Exists(Util.GetDatabaseTempDir() + Path.DirectorySeparatorChar + "chronojump.db")) 
		{
			// if exist also a file in default db location (improbable but done for solve eventual problems
			if(File.Exists(Util.GetDatabaseDir() + Path.DirectorySeparatorChar + "chronojump.db")) {
				File.Delete(Util.GetDatabaseDir() + Path.DirectorySeparatorChar + "chronojump.db"); //delete it
			}

			//move temp dir to default db location dir
			File.Move(Util.GetDatabaseTempDir() + Path.DirectorySeparatorChar + "chronojump.db",
					Util.GetDatabaseDir() + Path.DirectorySeparatorChar + "chronojump.db");
		}

		string sendErrorLogStr = Catalog.GetString("Send error log");
		messageCrashedBefore =
			string.Format(Catalog.GetString("Chronojump {0} crashed before."), BuildInfo.chronojumpVersion) + "\n" +
			string.Format(Catalog.GetString("Please, fill your email and click on '{0}' in order to fix this fast and contact you if appropriate."), sendErrorLogStr) + " " +
			Catalog.GetString("Your help is needed.");


		//messageCrashedBefore += "\n" + Catalog.GetString("Experimental webcam record has been disabled.") + "\n";

		/*
		 * This are the only outputs to Console. Other's use Log that prints to console and to log file
		 * this doesn't go to log because it talks about log
		 */
		LogB.Warning(messageCrashedBefore);
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
			JumpRj myJumpRj = SqliteJumpRj.SelectJumpData("tempJumpRj", existsTempData, false, false);
			try {
				myJumpRj.InsertAtDB (true, Constants.JumpRjTable);
			} catch {} //pitty, cannot insert

			Sqlite.DeleteTempEvents(tableName);
			returnString = "Recuperated last Reactive Jump";
		}

		tableName = "tempRunInterval";
		existsTempData = Sqlite.TempDataExists(tableName);
		if(existsTempData > 0)
		{
			RunInterval myRun = SqliteRunInterval.SelectRunData("tempRunInterval", existsTempData, false, false);
			try {
				myRun.InsertAtDB (true, Constants.RunIntervalTable);
			} catch {} //pitty, cannot insert

			Sqlite.DeleteTempEvents(tableName);
			returnString = "Recuperated last Intervallic Run";
		}
		
		return returnString;
	}

	/*
	 * note this is created relatively late
	 * so it can happen that two Chronojump instances are run almost at the same time
	 * 2nd instance will check chronojumpIsExecutingNTimes only if checkIfChronojumpExitAbnormally is true
	 * checkIfChronojumpExitAbnormally is true eg when chronojump_running exists
	 * so maybe chronojump_running still does not exists because createRunningFileName is called a bit late.
	 * So if there is a service to autostart chronojump (eg. in networks), do not make that service start very fast
	 */
	private void createRunningFileName(string runningFileName) {
		TextWriter writer = File.CreateText(runningFileName);
		writer.WriteLine(Process.GetCurrentProcess().Id);
		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();
	}

	private bool checkIfChronojumpExitAbnormally() {
		runningFileName = Util.GetDatabaseDir() + Path.DirectorySeparatorChar + "chronojump_running";
		if(File.Exists(runningFileName))
			return true;
		else
			return false;
	}

	private bool chronojumpIsExecutingNTimesComparePids(string pidName, string pid) {
		Process [] pids = Process.GetProcessesByName(pidName);

		foreach (Process myPid in pids)
			if (myPid.Id == Convert.ToInt32(pid))
				return true;
		return false;
	}

	private bool chronojumpIsExecutingNTimes() {
		try {
			StreamReader reader = File.OpenText(runningFileName);
			string pid = reader.ReadToEnd();
			reader.Close();

			//delete the '\n' that ReaderToEnd() has put
			pid = pid.TrimEnd(new char[1] {'\n'});
			
			if(UtilAll.IsWindows())
				return chronojumpIsExecutingNTimesComparePids("Chronojump", pid);
			else {
				//in linux process was names mono, but now is named mono-sgen
				bool found = chronojumpIsExecutingNTimesComparePids("mono", pid);
				if(found)
					return true;
				else
					return chronojumpIsExecutingNTimesComparePids("mono-sgen", pid);
			}
						
		} catch {
			/*
			   if we a chronojump older that 0.8.9.8 has crashed, and now we install 0.8.9.8
			   it will try to read the pid
			   but there will be no pid because old chronojumps have nothing written on that file
			   A -not perfect- solution is if there are problems here, return false: (is not executing n times)
			   */

			return false;
		}
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
			} else if(Directory.Exists(reallyOldDB)){
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

					string [] filesToMove = Directory.GetFiles(previous);
					foreach (string oldFile in filesToMove) {
						string newFile = newDB + Path.DirectorySeparatorChar + oldFile.Substring(oldDB.Length);
						try {
							File.Move(oldFile, newFile);
						}
						catch {
							fileMoveProblems ++;
							try {
								LogB.Warning(string.Format("{0}-{1}", oldFile, newFile));
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
					LogB.Error(feedback);
					chronojumpHasToExit = true;
				}
				if(fileCopyProblems > 0) {
					feedback += string.Format(Catalog.GetString("Cannot copy {0} files from {1} to {2}"), fileCopyProblems, previous, Path.GetFullPath(newDB)) + "\n";
					feedback += string.Format(Catalog.GetString("Please, do it manually.")) + "\n"; 
					feedback += string.Format(Catalog.GetString("Chronojump will exit now.")) + "\n";
					messageToShowOnBoot += feedback;	
					LogB.Error(feedback);
					chronojumpHasToExit = true;
				}
				if(fileMoveProblems > 0) {
					feedback += string.Format(Catalog.GetString("Cannot move {0} files from {1} to {2}"), fileMoveProblems, previous, Path.GetFullPath(newDB)) + "\n";
					feedback += string.Format(Catalog.GetString("Please, do it manually")) + "\n";
					messageToShowOnBoot += feedback;	
					LogB.Error(feedback);
				}
			}
					
			string dbMove = string.Format(Catalog.GetString("Database is now here: {0}"), Path.GetFullPath(newDB));
			messageToShowOnBoot += dbMove;	
			LogB.Warning(dbMove);
		}
	}

}
