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
 * Copyright (C) 2004-2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Data;
using System.IO; //"File" things. TextWriter
using System.Collections; //ArrayList
using System.Collections.Generic; //List
using Mono.Data.Sqlite;
using System.Diagnostics; 	//for launching other process
using System.Text.RegularExpressions; //Match

using Mono.Unix;

class SqliteGeneral
{
	private SqliteConnection dbcon;
	private bool isOpened;
	public SqliteGeneral(string databasePath)
	{
		isOpened = false;

		if (!File.Exists (databasePath)) {
			return;
		}
		dbcon = new SqliteConnection ();
		string connectionString = "version = 3; Data source = " + databasePath;
		dbcon.ConnectionString = connectionString;
		try {
			dbcon.Open();
			isOpened = true;
		}
		catch {

		}
	}

	//if this does not work, the just export to another folder and then copy where desired and the compress
	//needed to compress db on export
	/* unfortunately this does not help to be able to export/compress the used chronojump.db
	   //TODO: maybe try also to close the command
	public void CloseConnection ()
	{
		dbcon.Close ();
		dbcon.Dispose ();
		GC.Collect ();
	}
	*/

	public bool IsOpened
	{
		get
		{
			return isOpened;
		}
	}

	public SqliteConnection connection
	{
		get
		{
			return dbcon;
		}
	}

	public SqliteCommand command()
	{
		SqliteCommand dbcmd = dbcon.CreateCommand();
		return dbcmd;
	}
}

class Sqlite
{
	protected static SqliteConnection dbcon;
	protected static SqliteCommand dbcmd;

	//since we use installJammer (chronojump 0.7)	
	//database was on c:\.chronojump\ or in ~/.chronojump
	//now it's on installed dir, eg linux: ~/Chronojump/database
	private static string home = Util.GetDatabaseDir();
	private static string sqlFile = home + Path.DirectorySeparatorChar + "chronojump.db";
	
	private static string temp = Util.GetDatabaseTempDir();
	private static string sqlFileTemp = temp + Path.DirectorySeparatorChar + "chronojump.db";

	//http://www.mono-project.com/SQLite

	static string connectionString = "version = 3; Data source = " + sqlFile;
	static string connectionStringTemp = "version = 3; Data source = " + sqlFileTemp;

	//test to try to open db in a dir with accents (latin)
	//static string connectionString = "globalization requestEncoding=\"iso-8859-1\"; responseEncoding=\"iso-8859-1\"; fileEncoding=\"iso-8859-1\"; culture=\"es-ES\";version = 3; Data source = " + sqlFile;
	
	//create blank database
	static bool creatingBlankDatabase = false;

	//use LOCAL on chronojump start if db changed
	//use IMPORTED_SESSION when importing a session
	public enum UpdatingDBFromEnum { LOCAL, IMPORTED_SESSION }
	public static UpdatingDBFromEnum UpdatingDBFrom;
	
	public enum StatType { MAX, AVG }
	public enum Orders_by { DEFAULT, ID_ASC, ID_DESC }

	//for db creation
	static int creationRate;
	static int creationTotal;

	//for db conversion
	static string currentVersion = "0";
	
	protected static int conversionRate;
	protected static int conversionRateTotal;
	protected static int conversionSubRate;
	protected static int conversionSubRateTotal;

	public static bool IsOpened = false;
	public static bool SafeClose = true;
	public static bool GCCollect = false; //Experimental
	public static bool NeverCloseDB = false; //Experimental

	// Here it saves the initial class state before it's used. So we can restore it any time
	// if needed
	private static StaticClassState initialState;

	/*
	 * Important, change this if there's any update to database
	 */
	static string lastChronojumpDatabaseVersion = "2.47";

	public Sqlite()
	{
	}

	protected virtual void createTable(string tableName) {
	}
	
	//used by personSessionWeight
	protected virtual void createTable() {
	}
	
	~Sqlite() {}

	//these two methods are used by methods who want to leave the connection opened
	//because lots of similar transactions have to be done
	public static void Open()
	{
		LogB.Information("SQL going to open, status: " + dbcon.State.ToString());
		if(dbcon.State == System.Data.ConnectionState.Closed)
		{
			LogB.SQLon();
			try {
				dbcon.Open();
			} catch {
				LogB.SQL("-- catched --");

				Close();

				LogB.Warning(" going to open again ");
				LogB.SQLon();
				dbcon.Open();

				Console.WriteLine("-- end of catched --");
			}
		} else {
			LogB.SQLonAlready();
		}

		IsOpened = true;
	}
	public static void Close()
	{
		LogB.Information("SQL going to close, status: " + dbcon.State.ToString());
		if(NeverCloseDB)
			return;

		LogB.SQLoff();
			
		if(SafeClose) {
			dbcmd.Dispose(); //this seems critical in multiple open/close SQL
		}

		dbcon.Close();
		
		if(SafeClose) {
			if(GCCollect)
				GC.Collect(); //don't need and very slow

			dbcmd = dbcon.CreateCommand();
		}
		
		IsOpened = false;
	}

	protected static void openIfNeeded(bool dbconOpened)
	{
		if(! dbconOpened)
			Open();
	}
	protected static void closeIfNeeded(bool dbconOpened)
	{
		if(! dbconOpened)
			Close();
	}

	public static bool Connect()
	{
		/*
	       splashMessage = "pre";
		needUpdateSplashMessage = true;
		Console.ReadLine();		
		*/

		LogB.SQL("home is: " + home);

		bool defaultDBLocation = true;

		dbcon = new SqliteConnection();

		/*
		 * the Open() helps to know it threre are problems with path and sqlite
		 * passing utf-8 or looking for invalid chars is not enough
		 * but, as Open creates a file (if it doesn't exist)
		 * we prefer to create a test file (test.db) instead of chronojump.db
		 */
		string sqlFileTest = home + Path.DirectorySeparatorChar + "test.db";
		string sqlFileTestTemp = temp + Path.DirectorySeparatorChar + "test.db";
		string connectionStringTest = "version = 3; Data source = " + sqlFileTest;
		string connectionStringTestTemp = "version = 3; Data source = " + sqlFileTestTemp;


		dbcon.ConnectionString = connectionStringTest;
		dbcmd = dbcon.CreateCommand();

		try {
			dbcon.Open();
		} catch {
			dbcon.Close();
			dbcon.ConnectionString = connectionStringTestTemp;
			dbcmd = dbcon.CreateCommand();
			dbcon.Open();
			defaultDBLocation = false;
		}
		dbcon.Close();
		
		
		if(defaultDBLocation) {
			dbcon.ConnectionString = connectionString;
			if (File.Exists(sqlFileTest)){
				File.Delete(sqlFileTest);
			}
		} else {
			dbcon.ConnectionString = connectionStringTemp;
			if (File.Exists(sqlFileTestTemp)){
				File.Delete(sqlFileTestTemp);
			}
		}
		dbcmd = dbcon.CreateCommand();

		/*
		LogB.SQL(string.Format("press3"));
	       	splashMessage = "post1";
		needUpdateSplashMessage = true;
		Console.ReadLine();		
		*/

		/*
		try{
			LogB.SQL(string.Format("Trying database in ... " + connectionString));

		//dbcon = new SqliteConnection();
			*/
		/*
			dbcon.ConnectionString = connectionString;
			//dbcon.ConnectionString = connectionStringTemp;
			dbcmd = dbcon.CreateCommand();
		} catch {
			try {
				LogB.SQL(string.Format("Trying database in ... " + connectionStringTemp));

		//dbcon = new SqliteConnection();
				dbcon.ConnectionString = connectionStringTemp;
				dbcmd = dbcon.CreateCommand();
			} catch { 
				LogB.SQL("Problems, exiting...\n");
				System.Console.Out.Close();
				Log.End();
				Log.Delete();
				Environment.Exit(1);
			}

		}

		*/
			
		return defaultDBLocation;
		
	}

	public static string DatabaseFilePath
	{
		get {
			return sqlFile;
		}
	}

	public static string CurrentVersion
	{
		set { currentVersion = value; }
	}

	public static double CurrentVersionAsDouble
	{
		get { return Convert.ToDouble (Util.ChangeDecimalSeparator (currentVersion)); }
	}


	//used on Chronojump-Networks admin (if Config.LastDBFullPath is != "")
	//Config.LastDBFullPathStatic has been changed before
	public static void SetHome ()
	{
		home = Util.GetDatabaseDir ();

		//DB will be updated in gui/networks
		sqlFile = home + Path.DirectorySeparatorChar + "chronojump.db";

		connectionString = "version = 3; Data source = " + sqlFile;
	}

	//used on import
	public static void setSqlFilePath(string filePath)
	{
		sqlFile = filePath;
		connectionString = "version = 3; Data source = " + sqlFile;
	}

	public static void saveClassState()
	{
		initialState = new StaticClassState (typeof (Sqlite));
		initialState.readAttributes ();
	}

	public static StaticClassState InitialState
	{
		get
		{
			return initialState;
		}
	}

	//only create blank DB
	public static void ConnectBlank()
	{
		string sqlFileBlank = "chronojump_blank.db"; //copied on /chronojump-x.y/data installjammer will copy it to database
		string connectionStringBlank = "version = 3; Data source = " + sqlFileBlank;

		//delete blank file if exists
		if (File.Exists(sqlFileBlank)) {
			LogB.SQL("File blank exists, deleting...");
			File.Delete(sqlFileBlank);
		}

		dbcon = new SqliteConnection();
		dbcon.ConnectionString = connectionStringBlank;
		dbcmd = dbcon.CreateCommand();

		/*
		 * blankDB is created on Linux (create_release.sh) but
		 * it will work on windows.
		 * this bool allows to put COM? as default port
		 */

		creatingBlankDatabase = true; 
	}
	
	public static void CreateDir()
	{
		LogB.SQL(connectionString);

		string applicationDataDir = Util.GetLocalDataDir (false);

		if(!Directory.Exists(applicationDataDir)) {
			LogB.SQL("creating dir 1...");
			Directory.CreateDirectory (applicationDataDir);
		}
		
		if(!Directory.Exists(home)) {
			LogB.SQL("creating dir 2...");
			Directory.CreateDirectory (home);
		}
		LogB.SQL("Dirs created.");
	}

	public static void CreateFile()
	{
		LogB.SQL("creating file...");
		LogB.SQL(connectionString);
		
		//	if(!Directory.Exists(home)) {
		//		Directory.CreateDirectory (home);
		//	}

		//try {	
		dbcon.Open();
		/*
		   } catch {
		   dbcon.Close();
		   dbcon.ConnectionString = connectionStringTemp;
		   dbcmd = dbcon.CreateCommand();
		   dbcon.Open();
		   }
		   */
		dbcon.Close();
	}
	

	public static bool CheckTables(bool defaultDBLocation)
	{
		if(defaultDBLocation) {
			if (File.Exists(sqlFile)){
				return true;
			}
		} else {
			if (File.Exists(sqlFile)) {
				File.Move(Util.GetDatabaseDir() + Path.DirectorySeparatorChar + "chronojump.db",
					Util.GetDatabaseTempDir() + Path.DirectorySeparatorChar + "chronojump.db");
				return true;
			}
		}
		return false;
	}


	public static bool IsSqlite3() {
		if(sqlite3SelectWorks()){
			LogB.SQL("SQLITE3");
			Sqlite.Close();
			return true;
		}
		else if(sqlite2SelectWorks()) {
			LogB.SQL("SQLITE2");
			Sqlite.Close();
			//write sqlFile path on data/databasePath.txt
			//TODO
			//

			return false;
		}
		else {
			LogB.SQL("ERROR in sqlite detection");
			Sqlite.Close();
			return false;
		}
	}
	private static bool sqlite3SelectWorks() {
		try {
			SqlitePreferences.Select("chronopicPort");
		} catch {
			/*
			try {
				Sqlite.Close();
				if(File.Exists(Util.GetDatabaseDir() + Path.DirectorySeparatorChar + "chronojump.db"))
					File.Move(Util.GetDatabaseDir() + Path.DirectorySeparatorChar + "chronojump.db",
							Util.GetDatabaseTempDir() + Path.DirectorySeparatorChar + "chronojump.db");

				dbcon.ConnectionString = connectionStringTemp;
				dbcmd = dbcon.CreateCommand();
				Sqlite.Open();
				SqlitePreferences.Select("chronopicPort");
			} catch {
				Sqlite.Close();
				if(File.Exists(Util.GetDatabaseTempDir() + Path.DirectorySeparatorChar + "chronojump.db"))
					File.Move(Util.GetDatabaseTempDir() + Path.DirectorySeparatorChar + "chronojump.db",
							Util.GetDatabaseDir() + Path.DirectorySeparatorChar + "chronojump.db");

			*/
				return false;
			//}
		}
		return true;
	}
	private static bool sqlite2SelectWorks() {
		/*
		 *it says:
		 Unhandled Exception: System.NotSupportedException: Only Sqlite Version 3 is supported at this time
		   at Mono.Data.Sqlite.SqliteConnection.Open () [0x00000]
		 *
		Sqlite.Close();
		connectionString = "version=2; URI=file:" + sqlFile;
		dbcon.ConnectionString = connectionString;
		Sqlite.Open();
		try {
			SqlitePreferences.Select("chronopicPort");
		} catch {
			return false;
		}
		*/
		return true;
	}


	public static bool ConvertFromSqlite2To3() {
		/*
		 * 1 write the sqlite2 dumped data to an archive
		 * 2 copy db
		 * 3 create sqlite3 file from archive
		 */

		string sqlite2File = Util.GetDatabaseDir() + Path.DirectorySeparatorChar + "chronojump-sqlite2.81.db";
		string sqliteDB = Util.GetDatabaseDir() + Path.DirectorySeparatorChar + "chronojump.db";

		File.Copy(sqliteDB, sqlite2File, true);

		string myPath = "";
		string sqliteStr = "";
		//string sqlite3Str = "";
		string extension = "";
		try {
			if(UtilAll.IsWindows()) {
				myPath = Constants.UtilProgramsWindows;
				extension = Constants.ExtensionProgramsWindows;
				sqliteStr = "sqlite.exe";
				//sqlite3Str = "sqlite3.exe";
			}
			else {
				myPath = Constants.UtilProgramsLinux;
				extension = Constants.ExtensionProgramsLinux;
				sqliteStr = "sqlite-2.8.17.bin";
				//sqlite3Str = "sqlite3-3.5.0.bin";
			}

			if(File.Exists(myPath + Path.DirectorySeparatorChar + sqliteStr)) 
				LogB.SQL("exists1");
			if(File.Exists(sqlite2File)) 
				LogB.SQL("exists2");

			/*
			LogB.SQL("{0}-{1}", myPath + Path.DirectorySeparatorChar + sqliteStr , sqlite2File + " .dump");
			ProcessStartInfo ps = new ProcessStartInfo(myPath + Path.DirectorySeparatorChar + sqliteStr , sqlite2File + " .dump");

			ps.UseShellExecute = false;
			//ps.UseShellExecute = true;
			ps.RedirectStandardOutput = true;
			string output = "";
			using(Process p = Process.Start(ps)) {
				//TODO: this doesn't work on windows (it gets hanged)
				p.WaitForExit();
				output = p.StandardOutput.ReadToEnd();
			}
*/
			
			//write the path to chronojumpdb in a txt file (for convert_database.bat and .sh)
			TextWriter writer = File.CreateText(myPath + Path.DirectorySeparatorChar + "db_path.txt");
			string scriptsAreTwoDirsAhead = ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar;
			writer.WriteLine(scriptsAreTwoDirsAhead + Util.GetDatabaseDir());
			((IDisposable)writer).Dispose();
			
			LogB.SQL("Path written");

			Process p2 = Process.Start(myPath + Path.DirectorySeparatorChar + "convert_database." + extension);
			p2.WaitForExit();

			LogB.SQL("sqlite3 db created");
				
			File.Copy(myPath + Path.DirectorySeparatorChar + "tmp.db", sqliteDB, true ); //overwrite
		} catch {
			LogB.Error("PROBLEMS");
			return false;
		}

		LogB.SQL("done");
		return true;

	}

	//for splashWin text
	public static string PrintConversionText() {
		double toReach = Convert.ToDouble(Util.ChangeDecimalSeparator(lastChronojumpDatabaseVersion));
		return currentVersion + "/" + toReach.ToString() + " " +
			conversionRate.ToString() + "/" + conversionRateTotal.ToString() + " " +
			conversionSubRate.ToString() + "/" + conversionSubRateTotal.ToString() + " ";
	}

	//for splashWin progressbars
	public static double PrintCreation() {
		return UtilAll.DivideSafeFraction(creationRate, creationTotal);
	}
	public static double PrintConversionVersion() {
		return UtilAll.DivideSafeFraction(
				Convert.ToDouble(Util.ChangeDecimalSeparator(currentVersion)), 
				Convert.ToDouble(Util.ChangeDecimalSeparator(lastChronojumpDatabaseVersion))
				);
	}
	public static double PrintConversionRate() {
		return UtilAll.DivideSafeFraction(conversionRate, conversionRateTotal);
	}
	public static double PrintConversionSubRate() {
		return UtilAll.DivideSafeFraction(conversionSubRate, conversionSubRateTotal);
	}

	public static bool ConvertToLastChronojumpDBVersion() {
		LogB.SQL("SelectChronojumpProfile ()");

		//if(checkIfIsSqlite2())
		//	convertSqlite2To3();

		addChronopicPortNameIfNotExists();

		currentVersion = SqlitePreferences.Select("databaseVersion");

		//LogB.SQL("lastDB: {0}", Convert.ToDouble(lastChronojumpDatabaseVersion));
		//LogB.SQL("currentVersion: {0}", Convert.ToDouble(currentVersion));

		bool returnSoftwareIsNew = true; //-1 if software is too old for database (moved db to other computer)
		if(
				Convert.ToDouble(Util.ChangeDecimalSeparator(lastChronojumpDatabaseVersion)) == 
				Convert.ToDouble(Util.ChangeDecimalSeparator(currentVersion)))
			LogB.SQL("Database is already latest version");
		else if(
				Convert.ToDouble(Util.ChangeDecimalSeparator(lastChronojumpDatabaseVersion)) < 
				Convert.ToDouble(Util.ChangeDecimalSeparator(currentVersion))) {
			LogB.SQL("User database newer than program, need to update software");
			returnSoftwareIsNew = false;
		} else {
			//LogB.PrintAllThreads = true; //comment this
			LogB.Warning("Old database, need to convert");
			LogB.Warning("db version: " + currentVersion);

			bool needToConvertPersonToSport = false;
			bool jumpFallAsDouble = false;
	 		bool runAndRunIntervalInitialSpeedAdded = false;
			bool addedRSA = false;

			SqliteJumpRj sqliteJumpRjObject = new SqliteJumpRj();
			SqliteRunInterval sqliteRunIntervalObject = new SqliteRunInterval();
			SqliteReactionTime sqliteReactionTimeObject = new SqliteReactionTime();
			SqlitePulse sqlitePulseObject = new SqlitePulse();
			SqliteMultiChronopic sqliteMultiChronopicObject = new SqliteMultiChronopic();
			SqlitePersonSessionOld sqlitePersonSessionOldObject = new SqlitePersonSessionOld();

			UtilAll.OperatingSystems os = UtilAll.GetOSEnum();

			if(currentVersion == "0.41") {
				Sqlite.Open();

				//SqlitePulse.createTable(Constants.PulseTable);
				sqlitePulseObject.createTable(Constants.PulseTable);
				SqlitePulseType.createTablePulseType();
				SqlitePulseType.initializeTablePulseType();

				SqlitePreferences.Update ("databaseVersion", "0.42", true); 
				LogB.SQL("Converted DB to 0.42 (added pulse and pulseType tables)");

				Sqlite.Close();
				currentVersion = "0.42";
			}

			if(currentVersion == "0.42") {
				Sqlite.Open();
				SqlitePulseType.Insert ("Free:-1:-1:free PulseStep mode", true); 
				SqlitePreferences.Insert ("language", "es-ES"); 
				SqlitePreferences.Update ("databaseVersion", "0.43", true); 
				LogB.SQL("Converted DB to 0.43 (added 'free' pulseType & language peference)");
				Sqlite.Close();
				currentVersion = "0.43";
			}

			if(currentVersion == "0.43") {
				Sqlite.Open();
				SqlitePreferences.Insert ("showQIndex", "False"); 
				SqlitePreferences.Insert ("showDjIndex", "False"); 
				SqlitePreferences.Update ("databaseVersion", "0.44", true); 
				LogB.SQL("Converted DB to 0.44 (added showQIndex, showDjIndex)");
				Sqlite.Close();
				currentVersion = "0.44";
			}

			if(currentVersion == "0.44") {
				Sqlite.Open();
				SqlitePreferences.Insert ("allowFinishRjAfterTime", "True"); 
				SqlitePreferences.Update ("databaseVersion", "0.45", true); 
				LogB.SQL("Converted DB to 0.45 (added allowFinishRjAfterTime)");
				Sqlite.Close();
				currentVersion = "0.45";
			}

			if(currentVersion == "0.45") {
				Sqlite.Open();
				SqliteJumpType.JumpTypeInsert ("Free:1:0:Free jump", true); 
				SqlitePreferences.Update ("databaseVersion", "0.46", true); 
				LogB.SQL("Added Free jump type");
				Sqlite.Close();
				currentVersion = "0.46";
			}

			if(currentVersion == "0.46") {
				Sqlite.Open();

				//SqliteReactionTime.createTable(Constants.ReactionTimeTable);
				sqliteReactionTimeObject.createTable(Constants.ReactionTimeTable);

				SqlitePreferences.Update ("databaseVersion", "0.47", true); 
				LogB.SQL("Added reaction time table");
				Sqlite.Close();
				currentVersion = "0.47";
			}

			if(currentVersion == "0.47") {
				Sqlite.Open();

				//SqliteJumpRj.createTable(Constants.TempJumpRjTable);
				sqliteJumpRjObject.createTable(Constants.TempJumpRjTable);
				//SqliteRun.intervalCreateTable(Constants.TempRunIntervalTable);
				sqliteRunIntervalObject.createTable(Constants.TempRunIntervalTable);

				SqlitePreferences.Update ("databaseVersion", "0.48", true); 
				LogB.SQL("created tempJumpReactive and tempRunInterval tables");
				Sqlite.Close();
				currentVersion = "0.48";
			}

			if(currentVersion == "0.48") {
				Sqlite.Open();

				SqliteJumpType.JumpTypeInsert ("Rocket:1:0:Rocket jump", true); 

				string [] iniRunTypes = {
					"Agility-20Yard:18.28:20Yard Agility test",
					"Agility-505:10:505 Agility test",
					"Agility-Illinois:60:Illinois Agility test",
					"Agility-Shuttle-Run:40:Shuttle Run Agility test",
					"Agility-ZigZag:17.6:ZigZag Agility test"
				};
				foreach(string myString in iniRunTypes) {
					string [] s = myString.Split(new char[] {':'});
					RunType type = new RunType();
					type.Name = s[0];
					type.Distance = Convert.ToDouble(s[1]);
					type.Description = s[2];
					SqliteRunType.Insert(type, Constants.RunTypeTable, true);
				}
	

				SqliteEvent.createGraphLinkTable();
				SqliteRunType.AddGraphLinksRunSimpleAgility();	

				SqlitePreferences.Update ("databaseVersion", "0.49", true); 
				LogB.SQL("Added graphLinkTable, added Rocket jump and 5 agility tests: (20Yard, 505, Illinois, Shuttle-Run & ZigZag. Added graphs pof the 5 agility tests)");

				Sqlite.Close();
				currentVersion = "0.49";
			}

			if(currentVersion == "0.49") {
				Sqlite.Open();
				SqliteJumpType.Update ("SJ+", "SJl"); 
				SqliteJumpType.Update ("CMJ+", "CJl"); 
				SqliteJumpType.Update ("ABK+", "ABKl"); 
				SqliteJump.ChangeWeightToL();
				SqliteJumpType.AddGraphLinks();	
				SqliteJumpType.AddGraphLinksRj();	
				SqlitePreferences.Update ("databaseVersion", "0.50", true); 
				LogB.SQL("changed SJ+ to SJl, same for CMJ+ and ABK+, added jump and jumpRj graph links");
				Sqlite.Close();
				currentVersion = "0.50";
			}

			if(currentVersion == "0.50") {
				Sqlite.Open();
				SqliteRunType.AddGraphLinksRunSimple();	
				SqliteRunIntervalType.AddGraphLinksRunInterval();	
				SqlitePreferences.Update ("databaseVersion", "0.51", true); 
				LogB.SQL("added graphLinks for run simple and interval");
				Sqlite.Close();
				currentVersion = "0.51";
			}

			if(currentVersion == "0.51") {
				Sqlite.Open();
				SqliteJumpType.Update ("CJl", "CMJl"); 
				SqliteEvent.GraphLinkInsert (Constants.JumpTable, "CMJl", "jump_cmj_l.png", true);
				SqliteEvent.GraphLinkInsert (Constants.JumpTable, "ABKl", "jump_abk_l.png", true);
				SqlitePreferences.Update ("databaseVersion", "0.52", true); 
				LogB.SQL("added graphLinks for cmj_l and abk_l, fixed CMJl name");
				Sqlite.Close();
				currentVersion = "0.52";
			}
			
			if(currentVersion == "0.52") {
				Sqlite.Open();
				sqlitePersonSessionOldObject.createTable (); 
				Sqlite.Close();
				
				//this needs the dbCon closed
				SqlitePersonSessionOld.moveOldTableToNewTable (); 
				
				Sqlite.Open();
				SqlitePreferences.Update ("databaseVersion", "0.53", true); 
				Sqlite.Close();
				
				LogB.SQL("created weightSession table. Moved person weight data to weightSession table for each session that has performed");
				currentVersion = "0.53";
			}
			
			if(currentVersion == "0.53") {
				Sqlite.Open();

				SqliteSport.createTable();
				SqliteSport.initialize();
				SqliteSpeciallity.createTable();
				SqliteSpeciallity.initialize();

				//SqlitePersonOld.convertTableToSportRelated (); 
				needToConvertPersonToSport = true;
				
				SqlitePreferences.Update ("databaseVersion", "0.54", true); 
				Sqlite.Close();
				
				LogB.SQL("Created sport tables. Added sport data, speciallity and level of practice to person table");
				currentVersion = "0.54";
			}
			if(currentVersion == "0.54") {
				Sqlite.Open();

				SqliteSpeciallity.InsertUndefined(true);

				SqlitePreferences.Update ("databaseVersion", "0.55", true); 
				Sqlite.Close();
				
				LogB.SQL("Added undefined to speciallity table");
				currentVersion = "0.55";
			}
			if(currentVersion == "0.55") {
				Sqlite.Open();

				SqliteSessionOld.convertTableAddingSportStuff();

				SqlitePreferences.Update ("databaseVersion", "0.56", true); 
				Sqlite.Close();
				
				LogB.SQL("Added session default sport stuff into session table");
				currentVersion = "0.56";
			}
			if(currentVersion == "0.56") {
				Sqlite.Open();

				//jump and jumpRj
				ArrayList arrayAngleAndSimulated = new ArrayList(1);
				arrayAngleAndSimulated.Add("-1"); //angle
				arrayAngleAndSimulated.Add("-1"); //simulated
				
				//run and runInterval
				ArrayList arraySimulatedAndInitialSpeed = new ArrayList(1);
				arraySimulatedAndInitialSpeed.Add("-1"); //simulated
				arraySimulatedAndInitialSpeed.Add("0"); //initial speed
				
				//others
				ArrayList arraySimulated = new ArrayList(1);
				arraySimulated.Add("-1"); //simulated


				conversionRateTotal = 9;
				conversionRate = 1;
				convertTables(new SqliteJump(), Constants.JumpTable, 9, arrayAngleAndSimulated, false);
				conversionRate ++;
				convertTables(new SqliteJumpRj(), Constants.JumpRjTable, 16, arrayAngleAndSimulated, false);
				conversionRate ++;
				convertTables(new SqliteRun(), Constants.RunTable, 7, arraySimulated, false);
				conversionRate ++;
				convertTables(new SqliteRunInterval(), Constants.RunIntervalTable, 11, arraySimulated, false);
				runAndRunIntervalInitialSpeedAdded = true;
				
				conversionRate ++;
				convertTables(new SqliteReactionTime(), Constants.ReactionTimeTable, 6, arraySimulated, false);
				conversionRate ++;
				convertTables(new SqlitePulse(), Constants.PulseTable, 8, arraySimulated, false);


				//reacreate temp tables for have also the simulated column
				conversionRate ++;
				Sqlite.dropTable(Constants.TempJumpRjTable);
				sqliteJumpRjObject.createTable(Constants.TempJumpRjTable);
				Sqlite.dropTable(Constants.TempRunIntervalTable);
				sqliteRunIntervalObject.createTable(Constants.TempRunIntervalTable);

				conversionRate ++;
				SqliteCountry.createTable();
				SqliteCountry.initialize();
				
				conversionRate ++;
				int columnsBefore = 10;
				bool putDescriptionInMiddle = false;
				ArrayList arrayPersonRaceCountryServerID = new ArrayList(1);
				if(needToConvertPersonToSport) {
					columnsBefore = 7;
					arrayPersonRaceCountryServerID.Add(Constants.SportUndefinedID.ToString());
					arrayPersonRaceCountryServerID.Add(Constants.SpeciallityUndefinedID.ToString());
					arrayPersonRaceCountryServerID.Add(Constants.LevelUndefinedID.ToString());
					putDescriptionInMiddle = true;
				}
				arrayPersonRaceCountryServerID.Add(Constants.RaceUndefinedID.ToString());
				arrayPersonRaceCountryServerID.Add(Constants.CountryUndefinedID.ToString());
				arrayPersonRaceCountryServerID.Add(Constants.ServerUndefinedID.ToString());
				convertTables(new SqlitePersonOld(), Constants.PersonOldTable, columnsBefore, arrayPersonRaceCountryServerID, putDescriptionInMiddle);

				SqlitePreferences.Update ("databaseVersion", "0.57", true); 
				Sqlite.Close();
				
				LogB.SQL("Added simulated column to each event table on client. Added to person: race, country, serverUniqueID. Convert to sport related done here if needed. Added also run and runInterval initial speed");
				currentVersion = "0.57";
			}
			if(currentVersion == "0.57") {
				Sqlite.Open();
		
				//check if "republic" is in country table
				if(SqliteCountry.TableHasOldRepublicStuff()){
					conversionRateTotal = 4;
					conversionRate = 1;
					Sqlite.dropTable(Constants.CountryTable);
					conversionRate ++;
					SqliteCountry.createTable();
					conversionRate ++;
					SqliteCountry.initialize();
					conversionRate ++;
					LogB.SQL("Countries without kingdom or republic (except when needed)");
				}
				
				SqlitePreferences.Update ("databaseVersion", "0.58", true); 
				Sqlite.Close();
				
				currentVersion = "0.58";
			}

			if(currentVersion == "0.58") {
				Sqlite.Open();
				conversionRateTotal = 2;
				conversionRate = 1;
				SqlitePreferences.Insert ("showAngle", "False"); 
				alterTableColumn(new SqliteJump(), Constants.JumpTable, 11);

				//jump fall is also converted to double (don't need to do at conversion to 0.76)
				jumpFallAsDouble = true;

				SqlitePreferences.Update ("databaseVersion", "0.59", true); 
				LogB.SQL("Converted DB to 0.59 (added 'showAngle' to preferences, changed angle on jump to double)"); 
				conversionRate = 2;
				Sqlite.Close();
				currentVersion = "0.59";
			}

			if(currentVersion == "0.59") {
				Sqlite.Open();
				conversionRateTotal = 4;

				conversionRate = 1;
				SqlitePreferences.Insert ("volumeOn", "True"); 
				SqlitePreferences.Insert ("evaluatorServerID", "-1");

				conversionRate = 2;
			
				int columnsBefore = 8;
				ArrayList arrayServerID = new ArrayList(1);
				arrayServerID.Add(Constants.ServerUndefinedID.ToString());
				convertTables(new SqliteSession(), Constants.SessionTable, columnsBefore, arrayServerID, false);
				
				conversionRate = 3;
				SqliteEvent.SimulatedConvertToNegative();

				SqlitePreferences.Update ("databaseVersion", "0.60", true); 
				LogB.SQL("Converted DB to 0.60 (added volumeOn and evaluatorServerID to preferences. session has now serverUniqueID. Simulated now are -1, because 0 is real and positive is serverUniqueID)"); 
				
				conversionRate = 4;
				Sqlite.Close();
				currentVersion = "0.60";
			}

			if(currentVersion == "0.60") {
				Sqlite.Open();
				conversionRateTotal = 3;
				conversionRate = 1;

				ArrayList arrayDS = new ArrayList(1);
				arrayDS.Add("-1"); //distancesString
				convertTables(new SqliteRunIntervalType(), Constants.RunIntervalTypeTable, 7, arrayDS, false);
				
				conversionRate = 2;

				//SqliteRunType.RunIntervalTypeInsert ("MTGUG:-1:true:3:false:Modified time Getup and Go test:1-7-19", true);
				RunType type = new RunType();
				type.Name = "MTGUG";
				type.Distance = -1;
				type.TracksLimited = true;
				type.FixedValue = 3;
				type.Unlimited = false;
				type.Description = "Modified time Getup and Go test";
				type.DistancesString = "1-7-19";
				SqliteRunIntervalType.Insert(type, Constants.RunIntervalTypeTable, true);
				
				SqlitePreferences.Update ("databaseVersion", "0.61", true); 
				LogB.SQL("Converted DB to 0.61 added RunIntervalType distancesString (now we van have interval tests with different distances of tracks). Added MTGUG");
				
				conversionRate = 3;
				Sqlite.Close();
				currentVersion = "0.61";
			}
			if(currentVersion == "0.61") {
				Sqlite.Open();
				SqliteJumpType.JumpRjTypeInsert ("RJ(hexagon):1:0:1:18:Reactive Jump on a hexagon until three full revolutions are done", true);
				SqlitePreferences.Update ("databaseVersion", "0.62", true); 
				LogB.SQL("Converted DB to 0.62 added hexagon");
				Sqlite.Close();
				currentVersion = "0.62";
			}
			if(currentVersion == "0.62") {
				Sqlite.Open();
				SqlitePreferences.Insert ("versionAvailable", "");
				SqlitePreferences.Update ("databaseVersion", "0.63", true); 
				LogB.SQL("Converted DB to 0.63 (added 'versionAvailable' to preferences)"); 
				Sqlite.Close();
				currentVersion = "0.63";
			}
			if(currentVersion == "0.63") {
				Sqlite.Open();
				
				RunType type = new RunType();
				type.Name = "Margaria";
				type.Distance = 0;
				type.Description = "Margaria-Kalamen test";
				SqliteRunType.Insert(type, Constants.RunTypeTable, true);

				SqliteEvent.GraphLinkInsert (Constants.RunTable, "Margaria", "margaria.png", true);
				SqlitePreferences.Update ("databaseVersion", "0.64", true); 
				
				LogB.SQL("Converted DB to 0.64 (added margaria test)"); 
				Sqlite.Close();
				currentVersion = "0.64";
			}
			if(currentVersion == "0.64") {
				Sqlite.Open();
				
				SqliteServer sqliteServerObject = new SqliteServer();
				//user has also an evaluator table with a row (it's row)	
				sqliteServerObject.CreateEvaluatorTable();

				SqlitePreferences.Update ("databaseVersion", "0.65", true); 
				
				LogB.SQL("Converted DB to 0.65 (added Sevaluator on client)"); 
				Sqlite.Close();
				currentVersion = "0.65";
			}
			if(currentVersion == "0.65") {
				Sqlite.Open();
				//now runAnalysis is a multiChronopic event
				//SqliteJumpType.JumpRjTypeInsert ("RunAnalysis:0:0:1:-1:Run between two photocells recording contact and flight times in contact platform/s. Until finish button is clicked.", true);

				SqlitePreferences.Update ("databaseVersion", "0.66", true); 
				
				//LogB.SQL("Converted DB to 0.66 (added RunAnalysis Reactive jump)"); 
				LogB.SQL("Converted DB to 0.66 (done nothing)"); 
				Sqlite.Close();
				currentVersion = "0.66";
			}
			if(currentVersion == "0.66") {
				Sqlite.Open();
				SqliteJumpType.JumpTypeInsert ("TakeOff:0:0:Take off", true);
				SqliteJumpType.JumpTypeInsert ("TakeOffWeight:0:0:Take off with weight", true);

				SqlitePreferences.Update ("databaseVersion", "0.67", true); 
				
				LogB.SQL("Converted DB to 0.67 (added TakeOff jumps)"); 
				Sqlite.Close();
				currentVersion = "0.67";
			}
			if(currentVersion == "0.67") {
				Sqlite.Open();
				sqliteMultiChronopicObject.createTable(Constants.MultiChronopicTable);

				SqlitePreferences.Update ("databaseVersion", "0.68", true); 
				
				LogB.SQL("Converted DB to 0.68 (added multiChronopic tests table)"); 
				Sqlite.Close();
				currentVersion = "0.68";
			}
			if(currentVersion == "0.68") {
				Sqlite.Open();
				
				RunType type = new RunType();
				type.Name = "Gesell-DBT";
				type.Distance = 2.5;
				type.Description = "Gesell Dynamic Balance Test";
				SqliteRunType.Insert(type, Constants.RunTypeTable, true);

				SqliteEvent.GraphLinkInsert (Constants.RunTable, "Gesell-DBT", "gesell_dbt.png", true);
				SqlitePreferences.Update ("databaseVersion", "0.69", true); 
				
				LogB.SQL("Converted DB to 0.69 (added Gesell-DBT test)"); 
				Sqlite.Close();
				currentVersion = "0.69";
			}
			if(currentVersion == "0.69") {
				Sqlite.Open();
				SqlitePreferences.Insert ("showPower", "True"); 
				SqlitePreferences.Update ("databaseVersion", "0.70", true); 
				LogB.SQL("Converted DB to 0.70 (added showPower)");
				Sqlite.Close();
				currentVersion = "0.70";
			}
			if(currentVersion == "0.70") {
				Sqlite.Open();
				
				SqlitePersonSessionNotUpload.CreateTable();

				SqlitePreferences.Update ("databaseVersion", "0.71", true); 
				
				LogB.SQL("Converted DB to 0.71 (created personNotUploadTable on client)"); 
				Sqlite.Close();
				currentVersion = "0.71";
			}
			if(currentVersion == "0.71") {
				Sqlite.Open();
				
				datesToYYYYMMDD();

				SqlitePreferences.Update ("databaseVersion", "0.72", true); 
				
				LogB.SQL("Converted DB to 0.72 (dates to YYYY-MM-DD)"); 
				Sqlite.Close();
				currentVersion = "0.72";
			}
			if(currentVersion == "0.72") {
				Sqlite.Open();
				
				deleteOrphanedPersonsOld();

				SqlitePreferences.Update ("databaseVersion", "0.73", true); 
				
				LogB.SQL("Converted DB to 0.73 (deleted orphaned persons (in person table but not in personSessionWeight table)"); 
				Sqlite.Close();
				currentVersion = "0.73";
			}
			if(currentVersion == "0.73") {
				//dbcon open laters on mid convertDJinDJna()
				
				convertDJInDJna();

				SqlitePreferences.Update ("databaseVersion", "0.74", true); 
				
				LogB.SQL("Converted DB to 0.74 (All DJ converted to DJna)"); 
				Sqlite.Close();
				currentVersion = "0.74";
			}
			if(currentVersion == "0.74") {
				conversionRateTotal = 3;
				conversionRate = 1;
				
				Sqlite.Open();

				convertTables(new SqlitePersonOld(), Constants.PersonOldTable, 13, new ArrayList(), false);
				conversionRate++;
				
				convertTables(new SqlitePersonSessionOld(), Constants.PersonSessionOldWeightTable, 4, new ArrayList(), false);

				SqlitePreferences.Update ("databaseVersion", "0.75", true); 
				conversionRate++;
				
				LogB.SQL("Converted DB to 0.75 (person, and personSessionWeight have height and weight as double)"); 
				Sqlite.Close();
				currentVersion = "0.75";
			}
			if(currentVersion == "0.75") {
				conversionRateTotal = 3;
				conversionRate = 1;
				Sqlite.Open();

				if(!jumpFallAsDouble)
					alterTableColumn(new SqliteJump(), Constants.JumpTable, 11);
				
				conversionRate++;
				
				alterTableColumn(new SqliteJumpRj(), Constants.JumpRjTable, 18);
				
				SqlitePreferences.Update ("databaseVersion", "0.76", true); 
				conversionRate++;
				
				LogB.SQL("Converted DB to 0.76 (jump & jumpRj falls as double)"); 
				Sqlite.Close();
				currentVersion = "0.76";
			}

			bool person77AddedLinkServerImage = false;

			if(currentVersion == "0.76") {
				Sqlite.Open();

				//but first add migration from 2.17 to 2.18
				LogB.SQL("Person77 adding field: linkServerImage (for networks)");
				try {
					executeSQL("ALTER TABLE " + Constants.PersonTable + " ADD COLUMN linkServerImage TEXT;");
				} catch {
					LogB.SQL("Catched. maybe person77.linkServerImage already exists.");
				}
				person77AddedLinkServerImage = true;
				
				convertPersonAndPersonSessionTo77();
				SqlitePreferences.Update ("databaseVersion", "0.77", true); 
				LogB.SQL("Converted DB to 0.77 (person77, personSession77)"); 

				Sqlite.Close();
				currentVersion = "0.77";
			}
			if(currentVersion == "0.77") {
				Sqlite.Open();

				SqliteJumpType.UpdateOther ("weight", Constants.TakeOffWeightName, "1"); 

				Random rnd = new Random();
				string machineID = rnd.Next().ToString();
				SqlitePreferences.Insert ("machineID", machineID); 

				SqlitePreferences.Update ("databaseVersion", "0.78", true); 
				LogB.SQL("Converted DB to 0.78 (Added machineID to preferences, takeOffWeight has no weight in db conversions since 0.66)"); 

				Sqlite.Close();
				currentVersion = "0.78";
			}
			if(currentVersion == "0.78") {
				Sqlite.Open();

				SqlitePreferences.Insert ("multimediaStorage", Constants.MultimediaStorage.BYSESSION.ToString());

				SqlitePreferences.Update ("databaseVersion", "0.79", true); 
				LogB.SQL("Converted DB to 0.79 (Added multimediaStorage structure id)"); 

				Sqlite.Close();
				currentVersion = "0.79";
			}
			if(currentVersion == "0.79") {
				Sqlite.Open();

	 			if(! runAndRunIntervalInitialSpeedAdded) {
					ArrayList myArray = new ArrayList(1);
					myArray.Add("0"); //initial speed
				
					conversionRateTotal = 3;
					conversionRate = 1;
					convertTables(new SqliteRun(), Constants.RunTable, 8, myArray, false);
					conversionRate ++;
					convertTables(new SqliteRunInterval(), Constants.RunIntervalTable, 12, myArray, false);
					conversionRate ++;
					LogB.SQL("Converted DB to 0.80 Added run and runInterval initial speed (if not done in 0.56 conversion)"); 
				}

				SqlitePreferences.Update ("databaseVersion", "0.80", true); 
				
				Sqlite.Close();
				currentVersion = "0.80";
			}
			if(currentVersion == "0.80") {
				Sqlite.Open();

				ArrayList myArray = new ArrayList(1);
				myArray.Add("0"); //initial speed
				
				conversionRateTotal = 2;
				conversionRate = 1;
				Sqlite.dropTable(Constants.TempRunIntervalTable);
				sqliteRunIntervalObject.createTable(Constants.TempRunIntervalTable);
				conversionRate ++;
				LogB.SQL("Converted DB to 0.81 Added tempRunInterval initial speed"); 

				SqlitePreferences.Update ("databaseVersion", "0.81", true); 
				
				Sqlite.Close();
				currentVersion = "0.81";
			}
			if(currentVersion == "0.81") {
				Sqlite.Open();
				conversionRateTotal = 2;

				conversionRate = 1;
				SqlitePreferences.Insert ("videoOn", "False"); 
				conversionRate = 2;
				LogB.SQL("Converted DB to 0.82 Added videoOn"); 

				SqlitePreferences.Update ("databaseVersion", "0.82", true); 
				
				Sqlite.Close();
				currentVersion = "0.82";
			}
			if(currentVersion == "0.82") {
				Sqlite.Open();
				conversionRateTotal = 2;
				
				conversionRate = 1;
				SqliteEncoder.createTableEncoder();
				SqliteEncoder.createTableEncoderExercise();
				SqliteEncoder.initializeTableEncoderExercise();
				conversionRate = 2;
				LogB.SQL("Created encoder tables.");

				SqlitePreferences.Update ("databaseVersion", "0.83", true); 
				Sqlite.Close();
				currentVersion = "0.83";
			}
			if(currentVersion == "0.83") {
				Sqlite.Open();
				
				RunType type = new RunType();
				type.Name = "RSA 8-4-R3-5";
				type.Distance = -1;
				type.TracksLimited = true;
				type.FixedValue = 4;
				type.Unlimited = false;
				type.Description = "RSA testing";
				type.DistancesString = "8-4-R3-5";
				SqliteRunIntervalType.Insert(type, Constants.RunIntervalTypeTable, true);

				LogB.SQL("Added 1st RSA test.");

				SqlitePreferences.Update ("databaseVersion", "0.84", true); 
				Sqlite.Close();
				currentVersion = "0.84";
			}
			if(currentVersion == "0.84") {
				Sqlite.Open();
				SqliteJumpType.JumpTypeInsert ("slCMJ:1:0:Single-leg CMJ jump", true);

				SqlitePreferences.Update ("databaseVersion", "0.85", true); 
				
				LogB.SQL("Converted DB to 0.85 (added slCMJ jump)"); 
				Sqlite.Close();
				currentVersion = "0.85";
			}
			if(currentVersion == "0.85") {
				Sqlite.Open();
				LogB.SQL("Converted DB to 0.86 videoOn: TRUE"); 

				SqlitePreferences.Update("videoOn", "True", true);
				SqlitePreferences.Update ("databaseVersion", "0.86", true); 
				
				Sqlite.Close();
				currentVersion = "0.86";
			}
			if(currentVersion == "0.86") {
				Sqlite.Open();
				LogB.SQL("Added run speed start preferences on sqlite"); 

				SqlitePreferences.Insert ("runSpeedStartArrival", "True");
				SqlitePreferences.Insert ("runISpeedStartArrival", "True");
				SqlitePreferences.Update ("databaseVersion", "0.87", true); 
				
				Sqlite.Close();
				currentVersion = "0.87";
			}
			if(currentVersion == "0.87") {
				//delete runInterval type
				SqliteRunIntervalType.Delete("RSA 8-4-R3-5");

				//delete all it's runs
				Sqlite.Open();
				dbcmd.CommandText = "DELETE FROM " + Constants.RunIntervalTable +
					" WHERE type == \"RSA 8-4-R3-5\"";
				LogB.SQL(dbcmd.CommandText.ToString());
				dbcmd.ExecuteNonQuery();
				
				//add know RSAs
				SqliteRunIntervalType.addRSA();
				addedRSA = true;

				LogB.SQL("Deleted fake RSA test and added known RSA tests.");
				
				SqlitePreferences.Update ("databaseVersion", "0.88", true); 
				Sqlite.Close();

				currentVersion = "0.88";
			}
			if(currentVersion == "0.88") {
				Sqlite.Open();
	
				SqliteEncoder.addEncoderFreeExercise();
				
				LogB.SQL("Added encoder exercise: Free");
				
				SqlitePreferences.Update ("databaseVersion", "0.89", true); 
				Sqlite.Close();

				currentVersion = "0.89";
			}
			if(currentVersion == "0.89") {
				Sqlite.Open();
	
				SqlitePreferences.Insert("encoderPropulsive", "True");
				SqlitePreferences.Insert("encoderSmoothEccCon", "0.6");
				SqlitePreferences.Insert("encoderSmoothCon", "0.7");
				LogB.SQL("Preferences added propulsive and encoder smooth");
				
				SqlitePreferences.Update ("databaseVersion", "0.90", true); 
				Sqlite.Close();

				currentVersion = "0.90";
			}
			if(currentVersion == "0.90") {
				Sqlite.Open();
				
				SqliteEncoder.UpdateExerciseByName_old_do_not_use(true, "Squat", "Squat", 100, "weight bar", "", "",
						Constants.EncoderGI.ALL);
				LogB.SQL("Encoder Squat 75% -> 100%");
				
				SqlitePreferences.Update ("databaseVersion", "0.91", true); 
				Sqlite.Close();

				currentVersion = "0.91";
			}
			if(currentVersion == "0.91") {
				Sqlite.Open();
				
				SqlitePreferences.Insert("videoDevice", "");
				LogB.SQL("Added videoDevice to preferences");
				
				SqlitePreferences.Update ("databaseVersion", "0.92", true); 
				Sqlite.Close();

				currentVersion = "0.92";
			}
			if(currentVersion == "0.92") {
				Sqlite.Open();
				
				SqliteEncoder.UpdateExerciseByName_old_do_not_use(true, "Bench press", "Bench press", 0, "weight bar", "","0.185",
						Constants.EncoderGI.ALL);
				SqliteEncoder.UpdateExerciseByName_old_do_not_use(true, "Squat", "Squat", 100, "weight bar", "","0.31",
						Constants.EncoderGI.ALL);
				LogB.SQL("Added speed1RM on encoder exercise");
				
				SqlitePreferences.Update ("databaseVersion", "0.93", true); 
				Sqlite.Close();

				currentVersion = "0.93";
			}
			if(currentVersion == "0.93") {
				Sqlite.Open();
				
				SqliteEncoder.createTable1RM();
				LogB.SQL("Added encoder1RM table");
				
				SqlitePreferences.Update ("databaseVersion", "0.94", true); 
				Sqlite.Close();

				currentVersion = "0.94";
			}
			if(currentVersion == "0.94") {
				Sqlite.Open();
				
				SqlitePreferences.Insert ("encoder1RMMethod", 
						Constants.Encoder1RMMethod.WEIGHTED2.ToString());
				LogB.SQL("Added encoder1RMMethod");
				
				SqlitePreferences.Update ("databaseVersion", "0.95", true); 
				Sqlite.Close();

				currentVersion = "0.95";
			}
			if(currentVersion == "0.95") {
				Sqlite.Open();
				
				Update(true, Constants.EncoderTable, "future3", "", Constants.EncoderConfigurationNames.LINEAR.ToString(), 
						"signalOrCurve", "signal");
				Update(true, Constants.EncoderTable, "future3", "0", Constants.EncoderConfigurationNames.LINEAR.ToString(), 
						"signalOrCurve", "signal");
				Update(true, Constants.EncoderTable, "future3", "1", Constants.EncoderConfigurationNames.LINEARINVERTED.ToString(),
						"signalOrCurve", "signal");

				LogB.SQL("Encoder signal future3 three modes");
				
				SqlitePreferences.Update ("databaseVersion", "0.96", true); 
				Sqlite.Close();

				currentVersion = "0.96";
			}
			if(currentVersion == "0.96") {
				Sqlite.Open();
				
				SqlitePreferences.Insert ("inertialmomentum", "0.01");
				LogB.SQL("Added inertialmomentum in preferences");
				
				SqlitePreferences.Update ("databaseVersion", "0.97", true); 
				Sqlite.Close();

				currentVersion = "0.97";
			}
			if(currentVersion == "0.97") {
				Sqlite.Open();
				
				Update(true, Constants.EncoderTable, "laterality", "both", "RL", "", "");
				Update(true, Constants.EncoderTable, "laterality", "Both", "RL", "", "");
				Update(true, Constants.EncoderTable, "laterality", Catalog.GetString("Both"), "RL", "", "");
				Update(true, Constants.EncoderTable, "laterality", "right", "R", "", "");
				Update(true, Constants.EncoderTable, "laterality", "Right", "R", "", "");
				Update(true, Constants.EncoderTable, "laterality", Catalog.GetString("Right"), "R", "", "");
				Update(true, Constants.EncoderTable, "laterality", "left", "L", "", "");
				Update(true, Constants.EncoderTable, "laterality", "Left", "L", "", "");
				Update(true, Constants.EncoderTable, "laterality", Catalog.GetString("Left"), "L", "", "");
				LogB.SQL("Fixed encoder laterality");
				
				SqlitePreferences.Update ("databaseVersion", "0.98", true); 
				Sqlite.Close();

				currentVersion = "0.98";
			}
			if(currentVersion == "0.98") {
				Sqlite.Open();
		
				ArrayList array = SqliteOldConvert.EncoderSelect098(true,-1,-1,-1,"all",false);
				
				conversionRateTotal = array.Count;
				
				dropTable(Constants.EncoderTable);
				
				//CAUTION: do like this and never do createTableEncoder,
				//because method will change in the future and will break updates
				SqliteOldConvert.createTableEncoder99(); 
			
				int count = 1;	
				foreach( EncoderSQL098 es in array) {
					conversionRate = count;
				
					//do not use SqliteEncoder.Insert because that method maybe changes in the future,
					//and here we need to do a conversion that works from 0.98 to 0.99
					dbcmd.CommandText = "INSERT INTO " + Constants.EncoderTable +  
						" (uniqueID, personID, sessionID, exerciseID, eccon, laterality, extraWeight, " + 
						"signalOrCurve, filename, url, time, minHeight, smooth, description, status, " +
						"videoURL, mode, inertiaMomentum, diameter, future1, future2, future3)" +
						" VALUES (" + es.uniqueID + ", " +
						es.personID + ", " + es.sessionID + ", " +
						es.exerciseID + ", \"" + es.eccon + "\", \"" +
						es.laterality + "\", \"" + es.extraWeight + "\", \"" +
						es.signalOrCurve + "\", \"" + es.filename + "\", \"" +
						es.url + "\", " + es.time + ", " + es.minHeight + ", " +
						Util.ConvertToPoint(es.smooth) + ", \"" + es.description + "\", \"" +
						es.future1 + "\", \"" + es.future2 + "\", \"LINEAR\", " + //status, videoURL, mode
						"0, 0, \"\", \"\", \"\")"; //inertiaMomentum, diameter, future1, 2, 3
					LogB.SQL(dbcmd.CommandText.ToString());
					dbcmd.ExecuteNonQuery();
					count ++;
				}	

				conversionRate = count;
				LogB.SQL("Encoder table improved");
				SqlitePreferences.Update ("databaseVersion", "0.99", true); 
				Sqlite.Close();

				currentVersion = "0.99";
			}
			if(currentVersion == "0.99") {
				Sqlite.Open();

				SqliteEncoder.putEncoderExerciseAnglesAt90();
				SqliteEncoder.addEncoderJumpExercise();
				SqliteEncoder.addEncoderInclinedExercises();

				LogB.SQL("Added Free and inclinedExercises");
				SqlitePreferences.Update ("databaseVersion", "1.00", true); 
				Sqlite.Close();

				currentVersion = "1.00";
			}
			if(currentVersion == "1.00") {
				Sqlite.Open();
			
				SqlitePreferences.Insert ("CSVExportDecimalSeparator", Util.GetDecimalSeparatorFromLocale());

				LogB.SQL("Added export to CSV configuration on preferences");
				SqlitePreferences.Update ("databaseVersion", "1.01", true); 
				Sqlite.Close();

				currentVersion = "1.01";
			}
			if(currentVersion == "1.01") {
				Sqlite.Open();
			
				RunType type = new RunType("Agility-T-Test");
				SqliteRunType.Insert(type, Constants.RunTypeTable, true);
				type = new RunType("Agility-3L3R");
				SqliteRunIntervalType.Insert(type, Constants.RunIntervalTypeTable, true);

				LogB.SQL("Added Agility Tests: Agility-T-Test, Agility-3l3R");
				SqlitePreferences.Update ("databaseVersion", "1.02", true); 
				Sqlite.Close();

				currentVersion = "1.02";
			}
			if(currentVersion == "1.02") {
				Sqlite.Open();
		
				DeleteFromName(true, Constants.EncoderExerciseTable, "Inclinated plane Custom");
				SqliteEncoder.removeEncoderExerciseAngles();

				LogB.SQL("Updated encoder exercise, angle is now on encoder configuration");
				SqlitePreferences.Update ("databaseVersion", "1.03", true); 
				Sqlite.Close();

				currentVersion = "1.03";
			}
			if(currentVersion == "1.03") {
				Sqlite.Open();
		
				ArrayList array = SqliteOldConvert.EncoderSelect103(true,-1,-1,-1,"all",false);
				
				conversionRateTotal = array.Count;
				
				dropTable(Constants.EncoderTable);
				
				//CAUTION: do like this and never do createTableEncoder,
				//because method will change in the future and will break updates
				SqliteOldConvert.createTableEncoder104(); 
				
				//in this conversion put this as default for all SQL rows
				EncoderConfiguration econf = new EncoderConfiguration();
			
				int count = 1;	
				foreach(EncoderSQL103 es in array) {
					conversionRate = count;
				
					//do not use SqliteEncoder.Insert because that method maybe changes in the future,
					//and here we need to do a conversion that works from 1.03 to 1.04
					dbcmd.CommandText = "INSERT INTO " + Constants.EncoderTable +  
						" (uniqueID, personID, sessionID, exerciseID, eccon, laterality, extraWeight, " + 
						"signalOrCurve, filename, url, time, minHeight, description, status, " +
						"videoURL, encoderConfiguration, future1, future2, future3)" +
						" VALUES (" + es.uniqueID + ", " +
						es.personID + ", " + es.sessionID + ", " +
						es.exerciseID + ", \"" + es.eccon + "\", \"" +
						es.laterality + "\", \"" + es.extraWeight + "\", \"" +
						es.signalOrCurve + "\", \"" + es.filename + "\", \"" +
						es.url + "\", " + es.time + ", " + es.minHeight + ", \"" + es.description + "\", \"" + 
						es.status + "\", \"" + es.videoURL + "\", \"" + 
						econf.ToStringOutput(EncoderConfiguration.Outputs.SQL) + "\", \"" + //in this conversion put this as default for all SQL rows.
						es.future1 + "\", \"" + es.future2 + "\", \"" + es.future3 + "\")";
					LogB.SQL(dbcmd.CommandText.ToString());
					dbcmd.ExecuteNonQuery();
					count ++;
				}	

				conversionRate = count;
				LogB.SQL("Encoder table improved");
				SqlitePreferences.Update ("databaseVersion", "1.04", true); 
				Sqlite.Close();

				currentVersion = "1.04";
			}
			if(currentVersion == "1.04") {
				Sqlite.Open();
				
				dbcmd.CommandText = "DELETE FROM " + Constants.EncoderTable + 
					" WHERE encoderConfiguration LIKE \"%INERTIAL%\" AND " +
					" signalOrCurve == \"curve\"";
				LogB.SQL(dbcmd.CommandText.ToString());
				dbcmd.ExecuteNonQuery();

				LogB.SQL("Removed inertial curves, because sign was not checked on 1.04 when saving curves");
				SqlitePreferences.Update ("databaseVersion", "1.05", true); 
				Sqlite.Close();

				currentVersion = "1.05";
			}
			if(currentVersion == "1.05") {
				Sqlite.Open();
		
				SqliteEncoder.createTableEncoderSignalCurve();

				ArrayList signals = SqliteEncoder.Select(true, -1, -1, -1, Constants.EncoderGI.ALL,
						-1, "signal", EncoderSQL.Eccons.ALL, "", false, false, false); //last false is because EncoderSignalCurve is already not created
				ArrayList curves = SqliteEncoder.Select(true, -1, -1, -1, Constants.EncoderGI.ALL,
						-1, "curve", EncoderSQL.Eccons.ALL, "", false, false, false); //last false is because EncoderSignalCurve is already not created
				int signalID;
				conversionRateTotal = signals.Count;
				conversionRate = 1;
				//in 1.05 curves can be related to signals only by date
				foreach(EncoderSQL s in signals) {
					conversionRate ++;
					conversionSubRateTotal = curves.Count;
					conversionSubRate = 1;
							
					//needed to know if there are duplicates
					ArrayList curvesStored = new ArrayList();

					foreach(EncoderSQL c in curves) {
						conversionSubRate ++;
						if(s.GetDatetimeStr (false) == c.GetDatetimeStr (false) && s.eccon == c.eccon) {
							int msCentral = SqliteEncoder.FindCurveInSignal(
									s.GetFullURL(false), c.GetFullURL(false));

							signalID = Convert.ToInt32(s.uniqueID);
							if(msCentral == -1)
								signalID = -1; //mark as an orphaned curve (without signal)

							/*
							 * about duplicated curves from 1.05 and before:
							 * There are two kind of duplicates, in both, eccon is the same, and they overlap.
							 *
							 * Overlapings situations:
							 * - they are saved two times (same msCentral), or
							 * - they are saved two times with different smoothing (different msCentral)
							 *
							 * from now on (1.06) there will be no more duplicates
							 * about the old duplicated curves, is the user problem,
							 * except the curves of the first kind, that we know exactly that they are duplicated
							 */
							
							//curves come sorted by UniqueID DESC (selected with orderIDascendent = false)
							//if does not exist: insert in encoderSignalCurve
							bool exists = false;
							foreach(int ms in curvesStored)
								if(ms == msCentral)
									exists = true;
							if(exists) {
								//delete this (newer will not be deleted)
								Sqlite.Delete(true, 
										Constants.EncoderTable, Convert.ToInt32(c.uniqueID));
							} else {
								curvesStored.Add(msCentral);
								SqliteEncoder.SignalCurveInsert(true, 
										signalID, Convert.ToInt32(c.uniqueID), msCentral);
							}
						}
					}
				}

				conversionSubRate ++;
				conversionRate ++;
				LogB.SQL("Curves are now linked to signals");
				SqlitePreferences.Update ("databaseVersion", "1.06", true); 
				
				Sqlite.Close();
				currentVersion = "1.06";
			}
			if(currentVersion == "1.06") {
				Sqlite.Open();
			
				Update(true, Constants.GraphLinkTable, "graphFileName", "jump_dj.png", "jump_dj_a.png",
						"eventName", "DJa");
				
				LogB.SQL("Added jump_dj_a.png");
				SqlitePreferences.Update ("databaseVersion", "1.07", true); 
				Sqlite.Close();

				currentVersion = "1.07";
			}
			if(currentVersion == "1.07") {
				Sqlite.Open();
			
				LogB.SQL("Added translate statistics graph option to preferences");
				
				SqlitePreferences.Insert ("RGraphsTranslate", "True"); 
				SqlitePreferences.Update ("databaseVersion", "1.08", true); 
				Sqlite.Close();

				currentVersion = "1.08";
			}
			if(currentVersion == "1.08") {
				Sqlite.Open();
			
				LogB.SQL("Added option on preferences to useHeightsOnJumpIndexes (default) or not");
				
				SqlitePreferences.Insert ("useHeightsOnJumpIndexes", "True"); 
				SqlitePreferences.Update ("databaseVersion", "1.09", true); 
				Sqlite.Close();

				currentVersion = "1.09";
			}
			if(currentVersion == "1.09") {
				Sqlite.Open();
			
				LogB.SQL("Added RSA RAST on runType");

				/*
				 * addRSA() contains RAST since 1.10
				 * database started at 1.10 or more contains RAST
				 * database started before 0.87 adds RAST on the addRSA() method
				 * satabase started after 0.87 adds RAST now
				 */
				if(! addedRSA) {
					RunType type = new RunType();
					type.Name = "RSA RAST 35, R10 x 6";
					type.Distance = -1;
					type.TracksLimited = true;
					type.FixedValue = 12;
					type.Unlimited = false;
					type.Description = "RSA RAST Test";
					type.DistancesString = "35-R10";
					
					SqliteRunIntervalType.Insert(type, Constants.RunIntervalTypeTable, true);
					addedRSA = true;
				}
				
				SqlitePreferences.Update ("databaseVersion", "1.10", true); 
				Sqlite.Close();

				currentVersion = "1.10";
			}
			if(currentVersion == "1.10") {
				Sqlite.Open();
			
				LogB.SQL("Added option on autosave curves on capture (all/bestmeanpower/none)");
				
				SqlitePreferences.Insert ("encoderAutoSaveCurve", Constants.EncoderAutoSaveCurve.BEST.ToString()); 
				SqlitePreferences.Update ("databaseVersion", "1.11", true); 
				Sqlite.Close();

				currentVersion = "1.11";
			}
			if(currentVersion == "1.11") {
				Sqlite.Open();
			
				LogB.SQL("URLs from absolute to relative)");
				
				SqliteOldConvert.ConvertAbsolutePathsToRelative(); 
				SqlitePreferences.Update ("databaseVersion", "1.12", true); 
				Sqlite.Close();

				currentVersion = "1.12";
			}
			if(currentVersion == "1.12") {
				Sqlite.Open();
			
				LogB.SQL("Added ExecuteAuto table");
				
				SqliteExecuteAuto.createTableExecuteAuto();
				SqlitePreferences.Update ("databaseVersion", "1.13", true); 
				Sqlite.Close();

				currentVersion = "1.13";
			}
			if(currentVersion == "1.13") {
				Sqlite.Open();
			
				LogB.SQL("slCMJ -> slCMJleft, slCMJright");

				SqliteOldConvert.slCMJDivide();
				SqlitePreferences.Update ("databaseVersion", "1.14", true); 
				Sqlite.Close();

				currentVersion = "1.14";
			}
			if(currentVersion == "1.14") {
				Sqlite.Open();
			
				LogB.SQL("added Chronojump profile and bilateral profile");

				SqliteExecuteAuto.addChronojumpProfileAndBilateral();
				SqlitePreferences.Update ("databaseVersion", "1.15", true); 
				Sqlite.Close();

				currentVersion = "1.15";
			}
			if(currentVersion == "1.15") {
				Sqlite.Open();
			
				LogB.SQL("Cyprus moved to Europe");

				Update(true, Constants.CountryTable, "continent", "Asia", "Europe", "code", "CYP"); 
				SqlitePreferences.Update ("databaseVersion", "1.16", true); 
				Sqlite.Close();

				currentVersion = "1.16";
			}
			if(currentVersion == "1.16") {
				Sqlite.Open();
			
				LogB.SQL("Deleting Max jump");

				Update(true, Constants.JumpTable, "type", "Max", "Free", "", ""); 
				DeleteFromName(true, Constants.JumpTypeTable, "Max");
				SqlitePreferences.Update ("databaseVersion", "1.17", true); 
				Sqlite.Close();

				currentVersion = "1.17";
			}
			if(currentVersion == "1.17") {
				Sqlite.Open();
			
				LogB.SQL("Deleted Negative runInterval runs (bug from last version)");

				SqliteOldConvert.deleteNegativeRuns();
				SqlitePreferences.Update ("databaseVersion", "1.18", true); 
				Sqlite.Close();

				currentVersion = "1.18";
			}
			if(currentVersion == "1.18") {
				LogB.SQL("Preferences deleted showHeight, added showStiffness");
				
				Sqlite.Open();
				DeleteFromName(true, Constants.PreferencesTable, "showHeight");
				SqlitePreferences.Insert ("showStiffness", "True"); 
				SqlitePreferences.Update ("databaseVersion", "1.19", true); 
				Sqlite.Close();

				currentVersion = "1.19";
			}
			if(currentVersion == "1.19") {
				LogB.SQL("Preferences: added user email");
				
				Sqlite.Open();
				SqlitePreferences.Insert ("email", ""); 
				SqlitePreferences.Update ("databaseVersion", "1.20", true); 
				Sqlite.Close();

				currentVersion = "1.20";
			}
			if(currentVersion == "1.20") {
				LogB.SQL("Fixing loosing of encoder videoURL after recalculate");
				
				Sqlite.Open();
				
				SqliteOldConvert.ConvertAbsolutePathsToRelative(); //videoURLs got absolute again
				SqliteOldConvert.FixLostVideoURLAfterEncoderRecalculate();

				SqlitePreferences.Update ("databaseVersion", "1.21", true); 
				Sqlite.Close();

				currentVersion = "1.21";
			}
			if(currentVersion == "1.21") {
				LogB.SQL("Encoder laterality in english again");
				
				Sqlite.Open();
		
				if(Catalog.GetString("RL") != "RL")	
					Update(true, Constants.EncoderTable, "laterality", Catalog.GetString("RL"), "RL", "", "");
				
				if(Catalog.GetString("R") != "R")	
					Update(true, Constants.EncoderTable, "laterality", Catalog.GetString("R"), "R", "", "");
				
				if(Catalog.GetString("L") != "L")	
					Update(true, Constants.EncoderTable, "laterality", Catalog.GetString("L"), "L", "", "");

				SqlitePreferences.Update ("databaseVersion", "1.22", true); 
				Sqlite.Close();

				currentVersion = "1.22";
			}
			if(currentVersion == "1.22") {
				LogB.SQL("Added encoder configuration");
				
				Sqlite.Open();
				SqlitePreferences.Insert ("encoderConfiguration", new EncoderConfiguration().ToStringOutput(EncoderConfiguration.Outputs.SQL)); 
				SqlitePreferences.Update ("databaseVersion", "1.23", true); 
				Sqlite.Close();

				currentVersion = "1.23";
			}

			// ----------------------------------------------
			// IMPORTANT HERE IS DEFINED sqliteOpened == true
			// this is useful to not do more than 50 SQL open close
			// that crashes mac (Linux 100)
			// ----------------------------------------------
			LogB.SQL("Leaving Sqlite opened before DB updates");
	
			Sqlite.Open(); //------------------------------------------------


			if(currentVersion == "1.23") {
				LogB.SQL("Delete runISpeedStartArrival and add 4 double contacts configs");

				DeleteFromName(true, Constants.PreferencesTable, "runISpeedStartArrival");
				SqlitePreferences.Insert ("runDoubleContactsMode", Constants.DoubleContact.LAST.ToString()); 
				SqlitePreferences.Insert ("runDoubleContactsMS", "1000");
				SqlitePreferences.Insert ("runIDoubleContactsMode", Constants.DoubleContact.AVERAGE.ToString()); 
				SqlitePreferences.Insert ("runIDoubleContactsMS", "1000");

				currentVersion = updateVersion("1.24");
			}
			if(currentVersion == "1.24") {
				LogB.SQL("Language defaults to (empty string), means detected");
				SqlitePreferences.Update("language", "", true);
				
				currentVersion = updateVersion("1.25");
			}
			if(currentVersion == "1.25") {
				LogB.SQL("Changed Inclinated to Inclined");
				Update(true, Constants.EncoderExerciseTable, "name", "Inclinated plane", "Inclined plane", "", "");
				Update(true, Constants.EncoderExerciseTable, "name", "Inclinated plane BW", "Inclined plane BW", "", "");
				
				currentVersion = updateVersion("1.26");
			}
			if(currentVersion == "1.26") {
				LogB.SQL("Changing runDoubleContactsMS and runIDoubleContactsMS from 1000ms to 300ms");
				SqlitePreferences.Update("runDoubleContactsMS", "300", true);
				SqlitePreferences.Update("runIDoubleContactsMS", "300", true);
				
				currentVersion = updateVersion("1.27");
			}
			if(currentVersion == "1.27") {
				LogB.SQL("Changed encoderAutoSaveCurve BESTMEANPOWER to BEST");
				Update(true, Constants.PreferencesTable, "value", "BESTMEANPOWER", "BEST", "name", "encoderAutoSaveCurve");
				
				currentVersion = updateVersion("1.28");
			}
			if(currentVersion == "1.28") {
				LogB.SQL("Changed reaction time rows have reactionTime as default value");
				Update(true, Constants.ReactionTimeTable, "type", "", "reactionTime", "", "");
				
				currentVersion = updateVersion("1.29");
			}
			if(currentVersion == "1.29") {
				LogB.SQL("Added SIMULATED session");
	
				//add SIMULATED session if doesn't exists. Unique session where tests can be simulated.
				SqliteSession.insertSimulatedSession();

				currentVersion = updateVersion("1.30");
			}
			if(currentVersion == "1.30") {
				LogB.SQL("Insert encoderCaptureCheckFullyExtended and ...Value at preferences");

				SqlitePreferences.Insert ("encoderCaptureCheckFullyExtended", "True");
				SqlitePreferences.Insert ("encoderCaptureCheckFullyExtendedValue", "4");

				currentVersion = updateVersion("1.31");
			}
			if(currentVersion == "1.31") {
				LogB.SQL("encoderCaptureOptionsWin -> preferences");

				SqlitePreferences.Insert ("encoderCaptureTime", "60");
				SqlitePreferences.Insert ("encoderCaptureInactivityEndTime", "3");
				SqlitePreferences.Insert ("encoderCaptureMainVariable", Constants.EncoderVariablesCapture.MeanPower.ToString());
				SqlitePreferences.Insert ("encoderCaptureMinHeightGravitatory", "20");
				SqlitePreferences.Insert ("encoderCaptureMinHeightInertial", "5");
				SqlitePreferences.Insert ("encoderShowStartAndDuration", "False");

				currentVersion = updateVersion("1.32");
			}
			if(currentVersion == "1.32") {
				LogB.SQL("Added chronopicRegister table");

				SqliteChronopicRegister.createTableChronopicRegister();

				currentVersion = updateVersion("1.33");
			}
			if(currentVersion == "1.33") {
				LogB.SQL("Added thresholdJumps, thresholdRuns, thresholdOther to preferences");

				SqlitePreferences.Insert ("thresholdJumps", "50");
				SqlitePreferences.Insert ("thresholdRuns", "10");
				SqlitePreferences.Insert ("thresholdOther", "50");

				//jump directly to 1.36 because 1.34 has a first implementation of encoderConfiguration (not released)
				//1.35 deletes it
				//1.36 creates new encoderConfiguration ------------------------>
				currentVersion = updateVersion("1.36");
			}
			if(currentVersion == "1.34") {
				//1.36 creates new encoderConfiguration ------------------------>
				currentVersion = updateVersion("1.36");

				/*
				LogB.SQL("Added encoderConfiguration table");

				SqliteEncoderConfiguration.createTableEncoderConfiguration();

				currentVersion = updateVersion("1.35");
				*/
			}
			if(currentVersion == "1.35") {
				LogB.SQL("Deleted encoderConfiguration table");

				dropTable(Constants.EncoderConfigurationTable);

				currentVersion = updateVersion("1.36");
			}
			if(currentVersion == "1.36")
			{
				LogB.SQL("Deleted encoderConfiguration variable. Added encoderConfiguration table (1.36)");

				//1 create table
				SqliteEncoderConfiguration.createTableEncoderConfiguration();

				//2 load encoderConfiguration from SQL
				string ecStr = SqlitePreferences.Select("encoderConfiguration", true);
				string [] ecStrFull = ecStr.Split(new char[] {':'});

				//2.a create object
				EncoderConfiguration econfOnPreferences = new EncoderConfiguration(
						(Constants.EncoderConfigurationNames)
						Enum.Parse(typeof(Constants.EncoderConfigurationNames), ecStrFull[0]) );

				//2b assign the rest of params
				econfOnPreferences.ReadParamsFromSQL(ecStrFull);

				//3 insert default configurations
				if(econfOnPreferences.has_inertia)
				{
					SqliteEncoderConfiguration.Insert(true,
							new EncoderConfigurationSQLObject(
								-1, Constants.EncoderGI.INERTIAL, true, Constants.DefaultString(), econfOnPreferences, "")
							);
					SqliteEncoderConfiguration.insertDefault(Constants.EncoderGI.GRAVITATORY);
				}
				else
				{
					SqliteEncoderConfiguration.Insert(true,
							new EncoderConfigurationSQLObject(
								-1, Constants.EncoderGI.GRAVITATORY, true, Constants.DefaultString(), econfOnPreferences, "")
							);
					SqliteEncoderConfiguration.insertDefault(Constants.EncoderGI.INERTIAL);
				}

				//4 delete "encoderConfiguration" variable from SQL
				DeleteFromName(true, Constants.PreferencesTable, "encoderConfiguration");

				currentVersion = updateVersion("1.37");
			}
			if(currentVersion == "1.37")
			{
				/*
				 * encoderConfiguration has 7 values, 11 at 1.5.1 and 12 from 1.5.3.
				 * I't safe to convert everything to 12
				 * Example of checking the number of values using the separator (colon) character
				 * SELECT LENGTH(encoderConfiguration) - LENGTH(REPLACE(encoderConfiguration, ":", "")) AS colons, count(*) FROM encoder GROUP BY colons;
				 * 6|2149
				 * 10|74
				 * 11|1505
				 * */

				//encoderConfiguration table. Update fields with 6 ':'
				executeSQL("UPDATE encoderConfiguration " +
						"SET encoderConfiguration = encoderConfiguration || \":-1:0:0:1:0\" " +
						"WHERE LENGTH(encoderConfiguration) - LENGTH(REPLACE(encoderConfiguration, \":\", \"\")) = 6");

				//encoderConfiguration table. Update fields with 10 ':'
				executeSQL("UPDATE encoderConfiguration " +
						"SET encoderConfiguration = encoderConfiguration || \":0\" " +
						"WHERE LENGTH(encoderConfiguration) - LENGTH(REPLACE(encoderConfiguration, \":\", \"\")) = 10");

				//encoder table. Update fields with 6 ':'
				executeSQL("UPDATE encoder " +
						"SET encoderConfiguration = encoderConfiguration || \":-1:0:0:1:0\" " +
						"WHERE LENGTH(encoderConfiguration) - LENGTH(REPLACE(encoderConfiguration, \":\", \"\")) = 6");

				//encoder table. Update fields with 10 ':'
				executeSQL("UPDATE encoder " +
						"SET encoderConfiguration = encoderConfiguration || \":0\" " +
						"WHERE LENGTH(encoderConfiguration) - LENGTH(REPLACE(encoderConfiguration, \":\", \"\")) = 10");


				/*
				 * encoderConfiguration last parameter: list_d when is not used, sometimes is ":" or ":0" or ":-1"
				 * Convert all to ":0" that's how is going to be always when there are empty vales, from now on
				 *
				 * Don't use REPLACE because it will change all the -1 and not just the last one
				 */

				// A) encoderConfiguration table
				// If ends with ":" convert to ":0"
				executeSQL("UPDATE encoderConfiguration " +
						"SET encoderConfiguration = encoderConfiguration || \"0\" " +
						"WHERE SUBSTR(encoderConfiguration, -1, 1) = \":\"");

				// If ends with ":-1" convert to ":0"
				executeSQL("UPDATE encoderConfiguration " +
						"SET encoderConfiguration = SUBSTR(encoderConfiguration, 0, LENGTH(encoderConfiguration) +1 -2) || \"0\" " +
						"WHERE SUBSTR(encoderConfiguration, -3, 3) = \":-1\"");

				// B) encoder table
				// If ends with ":" convert to ":0"
				executeSQL("UPDATE encoder " +
						"SET encoderConfiguration = encoderConfiguration || \"0\" " +
						"WHERE SUBSTR(encoderConfiguration, -1, 1) = \":\"");

				// If ends with ":-1" convert to ":0"
				executeSQL("UPDATE encoder " +
						"SET encoderConfiguration = SUBSTR(encoderConfiguration, 0, LENGTH(encoderConfiguration) +1 -2) || \"0\" " +
						"WHERE SUBSTR(encoderConfiguration, -3, 3) = \":-1\"");

				currentVersion = updateVersion("1.38");
			}
			if(currentVersion == "1.38")
			{
				LogB.SQL("Created trigger table");
				SqliteTrigger.createTableTrigger();
				currentVersion = updateVersion("1.39");
			}
			if(currentVersion == "1.39")
			{
				LogB.SQL("Added to preferences: maximized, personWinHide, encoderCaptureShowOnlyBars");

				SqlitePreferences.Insert ("maximized", "0");
				SqlitePreferences.Insert ("personWinHide", "False");
				SqlitePreferences.Insert ("encoderCaptureShowOnlyBars", "True");

				currentVersion = updateVersion("1.40");
			}
			if(currentVersion == "1.40")
			{
				LogB.SQL("Updated preferences maximized: from true/false to no/yes/undecorated");

				SqlitePreferences.Update ("maximized", Preferences.MaximizedTypes.NO.ToString(), true);

				currentVersion = updateVersion("1.41");
			}

			if(currentVersion == "1.41")
			{
				LogB.SQL("Created and default values for ForceSensorRFD");

				SqliteForceSensorRFD.createTable();
				SqliteForceSensorRFD.InsertDefaultValues(true);

				currentVersion = updateVersion("1.42");
			}

			if(currentVersion == "1.42")
			{
				LogB.SQL("Added exercise params of last capture for next Chronojump start");

				//1 exercise
				ArrayList encoderExercises =
					SqliteEncoder.SelectEncoderExercises(true, -1, true, Constants.EncoderGI.ALL);

				if(encoderExercises.Count > 0) {
					EncoderExercise ex = (EncoderExercise) encoderExercises[0];
					SqlitePreferences.Insert(SqlitePreferences.EncoderExerciseIDGravitatory, ex.uniqueID.ToString());
					SqlitePreferences.Insert(SqlitePreferences.EncoderExerciseIDInertial, ex.uniqueID.ToString());
				}
				else {
					SqlitePreferences.Insert (SqlitePreferences.EncoderExerciseIDGravitatory, "1");
					SqlitePreferences.Insert (SqlitePreferences.EncoderExerciseIDInertial, "1");
				}

				//2 contraction
				SqlitePreferences.Insert (SqlitePreferences.EncoderContractionGravitatory, Constants.Concentric);
				SqlitePreferences.Insert (SqlitePreferences.EncoderContractionInertial, Constants.EccentricConcentric);

				//3 laterality
				SqlitePreferences.Insert (SqlitePreferences.EncoderLateralityGravitatory, "RL");
				SqlitePreferences.Insert (SqlitePreferences.EncoderLateralityInertial, "RL");

				//4 mass/weights
				SqlitePreferences.Insert (SqlitePreferences.EncoderMassGravitatory, "10");
				SqlitePreferences.Insert (SqlitePreferences.EncoderWeightsInertial, "0");

				currentVersion = updateVersion("1.43");
			}
			if(currentVersion == "1.43")
			{
				LogB.SQL("Added encoderCaptureCutByTriggers to preferences");

				SqlitePreferences.Insert ("encoderCaptureCutByTriggers", "False");

				currentVersion = updateVersion("1.44");
			}
			if(currentVersion == "1.44")
			{
				LogB.SQL("Added ForceSensorImpulse value");

				SqliteForceSensorRFD.InsertDefaultValueImpulse(true);

				currentVersion = updateVersion("1.45");
			}
			if(currentVersion == "1.45")
			{
				LogB.SQL("Added muteLogs at preferences");

				SqlitePreferences.Insert ("muteLogs", "False");

				currentVersion = updateVersion("1.46");
			}
			if(currentVersion == "1.46")
			{
				LogB.SQL("Added encoderCaptureBarplotFontSize at preferences");

				SqlitePreferences.Insert ("encoderCaptureBarplotFontSize", "14");

				currentVersion = updateVersion("1.47");
			}
			if(currentVersion == "1.47")
			{
				LogB.SQL("Updated preferences: added gstreamer");

				if(os == UtilAll.OperatingSystems.WINDOWS)
					SqlitePreferences.Insert (Preferences.GstreamerStr, Preferences.GstreamerTypes.SYSTEMSOUNDS.ToString());
				else if(os == UtilAll.OperatingSystems.MACOSX)
					SqlitePreferences.Insert (Preferences.GstreamerStr, Preferences.GstreamerTypes.FFPLAY.ToString());
				else
					SqlitePreferences.Insert (Preferences.GstreamerStr, Preferences.GstreamerTypes.GST_1_0.ToString());

				currentVersion = updateVersion("1.48");
			}
			if(currentVersion == "1.48")
			{
				LogB.SQL("Updated preferences: added force sensor tare/calibration stuff");

				SqlitePreferences.Insert (SqlitePreferences.ForceSensorTareDateTimeStr, "");
				SqlitePreferences.Insert (SqlitePreferences.ForceSensorTareStr, "-1"); //result value from sensor
				SqlitePreferences.Insert (SqlitePreferences.ForceSensorCalibrationDateTimeStr, "");
				SqlitePreferences.Insert (SqlitePreferences.ForceSensorCalibrationWeightStr, "-1");
				SqlitePreferences.Insert (SqlitePreferences.ForceSensorCalibrationFactorStr, "-1"); //result value from sensor

				currentVersion = updateVersion("1.49");
			}
			if(currentVersion == "1.49")
			{
				LogB.SQL("Updated preferences: added crashLogLanguage");

				SqlitePreferences.Insert ("crashLogLanguage", "English");

				currentVersion = updateVersion("1.50");
			}
			if(currentVersion == "1.50")
			{
				LogB.SQL("Updated encoderCaptureCutByTriggers variable");

				string cutStr = SqlitePreferences.Select("encoderCaptureCutByTriggers", true);
				if(cutStr == "True")
					SqlitePreferences.Update ("encoderCaptureCutByTriggers",
							Preferences.TriggerTypes.START_AT_CAPTURE.ToString(), true);
				else
					SqlitePreferences.Update ("encoderCaptureCutByTriggers",
							Preferences.TriggerTypes.NO_TRIGGERS.ToString(), true);

				currentVersion = updateVersion("1.51");
			}
			if(currentVersion == "1.51")
			{
				LogB.SQL("Added encoderRhtyhm stuff");

				EncoderRhythm er = new EncoderRhythm();
				SqlitePreferences.Insert (SqlitePreferences.EncoderRhythmEccSecondsStr,
						Util.ConvertToPoint(er.EccSeconds));
				SqlitePreferences.Insert (SqlitePreferences.EncoderRhythmConSecondsStr,
						Util.ConvertToPoint(er.ConSeconds));
				SqlitePreferences.Insert (SqlitePreferences.EncoderRhythmRestRepsSecondsStr,
						Util.ConvertToPoint(er.RestRepsSeconds));
				SqlitePreferences.Insert (SqlitePreferences.EncoderRhythmRepsClusterStr,
						er.RepsCluster.ToString()); //int
				SqlitePreferences.Insert (SqlitePreferences.EncoderRhythmRestClustersSecondsStr,
						Util.ConvertToPoint(er.RestClustersSeconds));

				currentVersion = updateVersion("1.52");
			}
			if(currentVersion == "1.52")
			{
				LogB.SQL("Added encoderRhtyhm active variable");

				SqlitePreferences.Insert (SqlitePreferences.EncoderRhythmActiveStr, "False");

				currentVersion = updateVersion("1.53");
			}
			if(currentVersion == "1.53")
			{
				LogB.SQL("Added encoderRhythm variables: repsOrPhases, repSeconds");

				EncoderRhythm er = new EncoderRhythm();
				SqlitePreferences.Insert (SqlitePreferences.EncoderRhythmRepsOrPhasesStr, er.RepsOrPhases.ToString());
				SqlitePreferences.Insert (SqlitePreferences.EncoderRhythmRepSecondsStr,
						Util.ConvertToPoint(er.RepSeconds));

				currentVersion = updateVersion("1.54");
			}
			if(currentVersion == "1.54")
			{
				LogB.SQL("Added preferences: personPhoto");

				SqlitePreferences.Insert ("personPhoto", "False");

				currentVersion = updateVersion("1.55");
			}
			if(currentVersion == "1.55")
			{
				LogB.SQL("Added encoder rhythm restAfterEcc");

				SqlitePreferences.Insert (SqlitePreferences.EncoderRhythmRestAfterEccStr, "True");

				currentVersion = updateVersion("1.56");
			}
			if(currentVersion == "1.56")
			{
				LogB.SQL("Created table UploadEncoderDataTemp, UploadSprintDataTemp");

				SqliteJson.createTableUploadEncoderDataTemp ();
				SqliteJson.createTableUploadSprintDataTemp ();

				currentVersion = updateVersion("1.57");
			}
			if(currentVersion == "1.57")
			{
				LogB.SQL("Added to preferences: encoderCaptureShowNRepetitions");

				SqlitePreferences.Insert ("encoderCaptureShowNRepetitions", "-1");

				currentVersion = updateVersion("1.58");
			}

			//bool createdForceSensorExerciseWith_tareBeforeCapture = false;
			if(currentVersion == "1.58")
			{
				LogB.SQL("Created ForceSensorExercise");

				SqliteForceSensorExerciseImport.createTable_v_1_58(); //now this createTable has the tareBeforeCapture column
				//do not use this because if update fails and is done on different Chronojump executions,
				//we will arrive to 1.66 having  this as false:
				//createdForceSensorExerciseWith_tareBeforeCapture
				//so better use a try/catch on ALTER TABLE

				currentVersion = updateVersion("1.59");
			}
			if(currentVersion == "1.59")
			{
				LogB.SQL("Created table UploadExhibitionTestTemp");

				SqliteJson.createTableUploadExhibitionTestTemp ();

				currentVersion = updateVersion("1.60");
			}
			if(currentVersion == "1.60")
			{
				LogB.SQL("Added to preferences: videoDeviceResolution, videoDeviceFramerate");

				SqlitePreferences.Insert ("videoDeviceResolution", "");
				SqlitePreferences.Insert ("videoDeviceFramerate", "");

				currentVersion = updateVersion("1.61");
			}
			if(currentVersion == "1.61")
			{
				LogB.SQL("Added to preferences: videoStopAfter");

				SqlitePreferences.Insert ("videoStopAfter", "2");

				currentVersion = updateVersion("1.62");
			}
			if(currentVersion == "1.62")
			{
				LogB.SQL("Added to preferences: encoderCaptureInertialDiscardFirstN");

				SqlitePreferences.Insert ("encoderCaptureInertialDiscardFirstN", "3");

				currentVersion = updateVersion("1.63");
			}
			if(currentVersion == "1.63")
			{
				LogB.SQL("Added to preferences: videoDevicePixelFormat");

				SqlitePreferences.Insert ("videoDevicePixelFormat", "");

				currentVersion = updateVersion("1.64");
			}
			if(currentVersion == "1.64")
			{
				LogB.SQL("Added to preferences: encoderCaptureSecondaryVariable");

				SqlitePreferences.Insert ("encoderCaptureSecondaryVariable", Constants.EncoderVariablesCapture.RangeAbsolute.ToString());

				currentVersion = updateVersion("1.65");
			}
			if(currentVersion == "1.65")
			{
				LogB.SQL("Added to preferences: encoderCaptureSecondaryVariableShow");

				SqlitePreferences.Insert ("encoderCaptureSecondaryVariableShow", "True");

				currentVersion = updateVersion("1.66");
			}
			if(currentVersion == "1.66")
			{
				LogB.SQL("Doing alter table forceSensorExercise adding tarebeforeCapture ...");
				try {
					executeSQL("ALTER TABLE " + Constants.ForceSensorExerciseTable + " ADD COLUMN tareBeforeCapture INT NOT NULL DEFAULT 0;");
				} catch {
					LogB.SQL("Catched. tareBeforeCapture already exists.");

				}
				LogB.SQL("Done!");

				currentVersion = updateVersion("1.67");
			}
			if(currentVersion == "1.67")
			{
				LogB.SQL("Created table: ForceSensor");

				SqliteForceSensor.createTable();

				currentVersion = updateVersion("1.68");
			}
			if(currentVersion == "1.68")
			{
				if(! person77AddedLinkServerImage)
				{
					//but first add migration from 2.17 to 2.18
					LogB.SQL("Person77 adding field: linkServerImage (for networks)");
					try {
						executeSQL("ALTER TABLE " + Constants.PersonTable + " ADD COLUMN linkServerImage TEXT;");
					} catch {
						LogB.SQL("Catched. maybe person77.linkServerImage already exists.");
					}
					person77AddedLinkServerImage = true;
				}

				LogB.SQL("Imported force sensor text files into SQL");

				SqliteForceSensor.import_from_1_68_to_1_69();

				currentVersion = updateVersion("1.69");
			}
			if(currentVersion == "1.69")
			{
				LogB.SQL("Created tables: RunEncoder, RunEncoderExercise");

				SqliteRunEncoder.createTable();
				SqliteRunEncoderExercise.createTable();

				currentVersion = updateVersion("1.70");
			}
			if(currentVersion == "1.70")
			{
				LogB.SQL("Imported run encoder text files into SQL");

				SqliteRunEncoder.import_from_1_70_to_1_71();

				currentVersion = updateVersion("1.71");
			}
			if(currentVersion == "1.71")
			{
				LogB.SQL("Inserted into preferences: forceSensorCaptureWidthSeconds, forceSensorCaptureScroll");

				SqlitePreferences.Insert (SqlitePreferences.ForceSensorCaptureWidthSeconds, "10");
				SqlitePreferences.Insert (SqlitePreferences.ForceSensorCaptureScroll, "True");

				currentVersion = updateVersion("1.72");
			}
			if(currentVersion == "1.72")
			{
				//LogB.SQL("Inserted into preferences: jumpsDjGraphHeights");

				//SqlitePreferences.Insert (SqlitePreferences.JumpsDjGraphHeights, "True");
				//unused, now using heightPreferred

				currentVersion = updateVersion("1.73");
			}
			if(currentVersion == "1.73")
			{
				LogB.SQL("Doing alter table forceSensorExercise adding forceResultant, elastic ...");
				try {
					//sqlite does not have drop column
					executeSQL("ALTER TABLE " + Constants.ForceSensorExerciseTable + " ADD COLUMN forceResultant INT NOT NULL DEFAULT 0;");
					executeSQL("ALTER TABLE " + Constants.ForceSensorExerciseTable + " ADD COLUMN elastic INT NOT NULL DEFAULT 0;");

					SqliteForceSensorExerciseImport.import_partially_from_1_73_to_1_74_unify_resistance_and_description();
				} catch {
					LogB.SQL("Catched. forceResultant or elastic already exists, or at unify resitance and desc.");

				}
				LogB.SQL("Done!");

				currentVersion = updateVersion("1.74");
			}
			if(currentVersion == "1.74")
			{
				LogB.SQL("Created table ForceSensorElasticBand");

				SqliteForceSensorElasticBand.createTable();

				currentVersion = updateVersion("1.75");
			}
			if(currentVersion == "1.75")
			{
				LogB.SQL("Doing alter table forceSensor adding stiffness/stiffnessString");
				try {
					//sqlite does not have drop column
					executeSQL("ALTER TABLE " + Constants.ForceSensorTable + " ADD COLUMN stiffness FLOAT DEFAULT -1;");
					executeSQL("ALTER TABLE " + Constants.ForceSensorTable + " ADD COLUMN stiffnessString TEXT;");

				} catch {
					LogB.SQL("Catched. forceSensor stiffness/stiffnessString already exists.");

				}
				LogB.SQL("Done!");

				currentVersion = updateVersion("1.76");
			}
			if(currentVersion == "1.76")
			{
				LogB.SQL("Inserted into preferences: forceSensorGraphsLineWidth");

				SqlitePreferences.Insert (SqlitePreferences.ForceSensorGraphsLineWidth, "2");

				currentVersion = updateVersion("1.77");
			}
			if(currentVersion == "1.77")
			{
				LogB.SQL("Inserted into preferences: encoderAutoSaveCurveBestNValue");

				SqlitePreferences.Insert (SqlitePreferences.EncoderAutoSaveCurveBestNValue, "3");

				currentVersion = updateVersion("1.78");
			}
			if(currentVersion == "1.78")
			{
				LogB.SQL("Inserted into preferences: encoderWorkKcal");

				SqlitePreferences.Insert (SqlitePreferences.EncoderWorkKcal, "True");

				currentVersion = updateVersion("1.79");
			}
			if(currentVersion == "1.79")
			{
				LogB.SQL("Inserted forceSensorElasticEccMinDispl, ...");

				SqlitePreferences.Insert (SqlitePreferences.ForceSensorElasticEccMinDispl, ".1");
				SqlitePreferences.Insert (SqlitePreferences.ForceSensorElasticConMinDispl, ".1");
				SqlitePreferences.Insert (SqlitePreferences.ForceSensorNotElasticEccMinForce, "10");
				SqlitePreferences.Insert (SqlitePreferences.ForceSensorNotElasticConMinForce, "10");

				currentVersion = updateVersion("1.80");
			}
			if(currentVersion == "1.80")
			{
				LogB.SQL("Inserted forceSensorCaptureFeedbackActive /At /Range");

				SqlitePreferences.Insert (SqlitePreferences.ForceSensorCaptureFeedbackActive, Preferences.ForceSensorCaptureFeedbackActiveEnum.NO.ToString());
				SqlitePreferences.Insert (SqlitePreferences.ForceSensorCaptureFeedbackAt, "100");
				SqlitePreferences.Insert (SqlitePreferences.ForceSensorCaptureFeedbackRange, "40");

				currentVersion = updateVersion("1.81");
			}
			if(currentVersion == "1.81")
			{
				LogB.SQL("Doing alter table jump, jumpRj, tempJumpRj add datetime");
				try {
					//sqlite does not have drop column
					executeSQL("ALTER TABLE " + Constants.JumpTable + " ADD COLUMN datetime TEXT;");
					executeSQL("ALTER TABLE " + Constants.JumpRjTable + " ADD COLUMN datetime TEXT;");
					executeSQL("ALTER TABLE " + Constants.TempJumpRjTable + " ADD COLUMN datetime TEXT;");
				} catch {
					LogB.SQL("Catched. forceSensor stiffness/stiffnessString already exists.");

				}
				LogB.SQL("Done!");

				currentVersion = updateVersion("1.82");
			}
			if(currentVersion == "1.82")
			{
				LogB.SQL("Added missing agility_t_test image");
				SqliteEvent.GraphLinkInsert (Constants.RunTable, "Agility-T-Test", "agility_t_test.png", true);
				currentVersion = updateVersion("1.83");
			}
			if(currentVersion == "1.83")
			{
				LogB.SQL("Inserted into preferences: forceSensorMIFDuration Mode/Seconds/Percent");

				SqlitePreferences.Insert (SqlitePreferences.ForceSensorMIFDurationMode, Preferences.ForceSensorMIFDurationModes.SECONDS.ToString());
				SqlitePreferences.Insert (SqlitePreferences.ForceSensorMIFDurationSeconds, "2");
				SqlitePreferences.Insert (SqlitePreferences.ForceSensorMIFDurationPercent, "5");

				currentVersion = updateVersion("1.84");
			}
			if(currentVersion == "1.84")
			{
				LogB.SQL("Inserted 5 vars into preferences: EncoderCaptureMainVariable...");

				SqlitePreferences.Insert (SqlitePreferences.EncoderCaptureMainVariableThisSetOrHistorical, "True");
				SqlitePreferences.Insert (SqlitePreferences.EncoderCaptureMainVariableGreaterActive, "False");
				SqlitePreferences.Insert (SqlitePreferences.EncoderCaptureMainVariableGreaterValue, "90");
				SqlitePreferences.Insert (SqlitePreferences.EncoderCaptureMainVariableLowerActive, "False");
				SqlitePreferences.Insert (SqlitePreferences.EncoderCaptureMainVariableLowerValue, "70");

				currentVersion = updateVersion("1.85");
			}
			if(currentVersion == "1.85")
			{
				LogB.SQL("Inserted into preferences: RunEncoderMinAccel");

				SqlitePreferences.Insert (SqlitePreferences.RunEncoderMinAccel, "10.0");

				currentVersion = updateVersion("1.86");
			}
			if(currentVersion == "1.86")
			{
				LogB.SQL("Doing alter table forceSensorExercise adding eccReps, eccMin, conMin");
				try {
					//sqlite does not have drop column
					executeSQL("ALTER TABLE " + Constants.ForceSensorExerciseTable + " ADD COLUMN eccReps INT DEFAULT 0;");
					executeSQL("ALTER TABLE " + Constants.ForceSensorExerciseTable + " ADD COLUMN eccMin FLOAT DEFAULT -1;");
					executeSQL("ALTER TABLE " + Constants.ForceSensorExerciseTable + " ADD COLUMN conMin FLOAT DEFAULT -1;");
				} catch {
					LogB.SQL("Catched. ");

				}
				LogB.SQL("Done!");

				currentVersion = updateVersion("1.87");
			}
			if(currentVersion == "1.87")
			{
				LogB.SQL("Inserted into preferences: encoderCaptureInertialEccOverloadMode");

				SqlitePreferences.Insert (SqlitePreferences.EncoderCaptureInertialEccOverloadMode,
						Preferences.encoderCaptureEccOverloadModes.SHOW_LINE.ToString());

				currentVersion = updateVersion("1.88");
			}
			if(currentVersion == "1.88")
			{
				LogB.SQL("Inserted into preferences: encoderCaptureFeedbackEccon");

				SqlitePreferences.Insert (SqlitePreferences.EncoderCaptureFeedbackEccon,
						Preferences.EncoderPhasesEnum.BOTH.ToString());

				currentVersion = updateVersion("1.89");
			}
			if(currentVersion == "1.89")
			{
				LogB.SQL("Inserted into preferences: units");

				SqlitePreferences.Insert (SqlitePreferences.UnitsStr, Preferences.UnitsEnum.METRIC.ToString());

				currentVersion = updateVersion("1.90");
			}
			if(currentVersion == "1.90")
			{
				LogB.SQL("Inserted into preferences: colorBackground");
				SqlitePreferences.Insert (SqlitePreferences.ColorBackground, "#0e1e46");
				currentVersion = updateVersion("1.91");
			}
			if(currentVersion == "1.91")
			{
				LogB.SQL("Inserted into preferences: menuType");

				Preferences.MenuTypes menuType = Preferences.MenuTypes.ALL;
				if(SqlitePreferences.Select("personWinHide", true) == "True")
					menuType = Preferences.MenuTypes.ICONS;

				SqlitePreferences.Insert (SqlitePreferences.MenuType, menuType.ToString());

				currentVersion = updateVersion("1.92");
			}
			if(currentVersion == "1.92")
			{
				LogB.SQL("Inserted into preferences: EncoderCaptureInfinite, LogoAnimatedShow");

				SqlitePreferences.Insert (SqlitePreferences.EncoderCaptureInfinite, "False");
				SqlitePreferences.Insert (SqlitePreferences.LogoAnimatedShow, "True");

				currentVersion = updateVersion("1.93");
			}
			if(currentVersion == "1.93")
			{
				LogB.SQL("Inserted into preferences: RunEncoderAnalyzeAccel/Force/Power");

				SqlitePreferences.Insert (Preferences.runEncoderAnalyzeAccel.Name,
					Preferences.runEncoderAnalyzeAccel.SqlDefaultName);
				SqlitePreferences.Insert (Preferences.runEncoderAnalyzeForce.Name,
					Preferences.runEncoderAnalyzeForce.SqlDefaultName);
				SqlitePreferences.Insert (Preferences.runEncoderAnalyzePower.Name,
					Preferences.runEncoderAnalyzePower.SqlDefaultName);

				currentVersion = updateVersion("1.94");
			}
			if(currentVersion == "1.94")
			{
				LogB.SQL("Inserted into preferences: importerPythonVersion");

				SqlitePreferences.Insert (SqlitePreferences.ImporterPythonVersion,
					Preferences.pythonVersionEnum.Python3.ToString());

				currentVersion = updateVersion("1.95");
			}
			if(currentVersion == "1.95")
			{
				LogB.SQL("Inserted into preferences: jumpsFVProfileOnlyBestInWeight, jumpsFVProfileShowFullGraph, jumpsEvolutionOnlyBestInSession");

				SqlitePreferences.Insert (SqlitePreferences.JumpsFVProfileOnlyBestInWeight, "True");
				SqlitePreferences.Insert (SqlitePreferences.JumpsFVProfileShowFullGraph, "True");
				SqlitePreferences.Insert (SqlitePreferences.JumpsEvolutionOnlyBestInSession, "False");

				currentVersion = updateVersion("1.96");
			}
			if(currentVersion == "1.96")
			{
				LogB.SQL("Inserted into preferences: loadLastSessionAtStart, lastSessionID, loadLastModeAtStart, lastMode");

				SqlitePreferences.Insert (SqlitePreferences.LoadLastSessionAtStart, "True");
				SqlitePreferences.Insert (SqlitePreferences.LastSessionID, "-1");
				SqlitePreferences.Insert (SqlitePreferences.LoadLastModeAtStart, "True");
				SqlitePreferences.Insert (SqlitePreferences.LastMode, Constants.Modes.UNDEFINED.ToString());

				currentVersion = updateVersion("1.97");
			}
			if(currentVersion == "1.97")
			{
				LogB.SQL("Inserted into preferences: colorBackgroundOsColor");
				SqlitePreferences.Insert (SqlitePreferences.ColorBackgroundOsColor, "False");
				currentVersion = updateVersion("1.98");
			}
			if(currentVersion == "1.98")
			{
				LogB.SQL("Updated 3L3R tracks fixedValue (just affected description)");
				Update(true, Constants.RunIntervalTypeTable, "fixedValue",
						"", "2",
						"name", "Agility-3L3R");
				currentVersion = updateVersion("1.99");
			}
			if(currentVersion == "1.99")
			{
				LogB.SQL("Inserted into preferences: fontsOnGraphs");

				SqlitePreferences.Insert (SqlitePreferences.FontsOnGraphs, Preferences.FontTypes.Helvetica.ToString());

				currentVersion = updateVersion("2.00");
			}
			if(currentVersion == "2.00")
			{
				LogB.SQL("RunEncoderExercise ALTER TABLE: added column segmentMeters");
				try {
					//sqlite does not have drop column
					executeSQL("ALTER TABLE " + Constants.RunEncoderExerciseTable + " ADD COLUMN segmentMeters INT DEFAULT 5;");
				} catch {
					LogB.SQL("Catched. ");

				}
				LogB.SQL("Done!");

				currentVersion = updateVersion("2.01");
			}
			if(currentVersion == "2.01")
			{
				LogB.SQL("Inserted into preferences: restTimeMinutes, restTimeSeconds");
				SqlitePreferences.Insert (SqlitePreferences.RestTimeMinutes, "2");
				SqlitePreferences.Insert (SqlitePreferences.RestTimeSeconds, "0");

				currentVersion = updateVersion("2.02");
			}
			if(currentVersion == "2.02")
			{
				LogB.SQL("Inserted into preferences: encoderInertialGraphsX");
				SqlitePreferences.Insert (SqlitePreferences.EncoderInertialGraphsX,
						Preferences.EncoderInertialGraphsXTypes.EQUIVALENT_MASS.ToString());

				currentVersion = updateVersion("2.03");
			}
			if(currentVersion == "2.03")
			{
				LogB.SQL("Created tables: tagSession, sessionTagSession");

				SqliteTagSession.createTable();
				SqliteSessionTagSession.createTable();

				currentVersion = updateVersion("2.04");
			}
			if(currentVersion == "2.04")
			{
				LogB.SQL("Inserted into preferences: forceSensorStartEndOptimized");
				SqlitePreferences.Insert (SqlitePreferences.ForceSensorStartEndOptimized,
						"True");

				currentVersion = updateVersion("2.05");
			}
			if(currentVersion == "2.05")
			{
				LogB.SQL("Default python version for all users: Python3");
				SqlitePreferences.Update (SqlitePreferences.ImporterPythonVersion, Preferences.pythonVersionEnum.Python3.ToString(), true); 

				currentVersion = updateVersion("2.06");
			}
			if(currentVersion == "2.06")
			{
				LogB.SQL("Inserted into preferences: forceSensorVariabilityMethod");

				SqlitePreferences.Insert (SqlitePreferences.ForceSensorVariabilityMethod,
						Preferences.VariabilityMethodEnum.CVRMSSD.ToString());

				currentVersion = updateVersion("2.07");
			}
			if(currentVersion == "2.07")
			{
				LogB.SQL("Create table news and insert newsLanguageEs on preferences");

				SqliteNews.createTable();
				SqlitePreferences.Insert (SqlitePreferences.NewsLanguageEs, "False");

				currentVersion = updateVersion("2.08");
			}
			if(currentVersion == "2.08")
			{
				LogB.SQL("Inserted into preferences: forceSensorAnalyzeABSliderIncrement");

				SqlitePreferences.Insert (SqlitePreferences.ForceSensorAnalyzeABSliderIncrement,
						"1");

				currentVersion = updateVersion("2.09");
			}
			if(currentVersion == "2.09")
			{
				LogB.SQL("Inserted prefs: encoderCaptureShowLoss, runEncoderPPS");

				SqlitePreferences.Insert (SqlitePreferences.EncoderCaptureShowLoss, "True");
				SqlitePreferences.Insert (SqlitePreferences.RunEncoderPPS, "10");

				currentVersion = updateVersion("2.10");
			}
			if(currentVersion == "2.10")
			{
				LogB.SQL("Inserted prefs: clientNewsDatetime");

				SqlitePreferences.Insert (SqlitePreferences.ClientNewsDatetime, "");

				currentVersion = updateVersion("2.11");
			}
			if(currentVersion == "2.11")
			{
				LogB.SQL("Inserted into preferences: forceSensorAnalyzeMaxAVGInWindow");

				SqlitePreferences.Insert (SqlitePreferences.ForceSensorAnalyzeMaxAVGInWindow, "1");

				currentVersion = updateVersion("2.12");
			}
			if(currentVersion == "2.12")
			{
				LogB.SQL("Inserted into preferences: PersonSelectWinImages, ExportGraphWidth, ExportGraphHeight");

				SqlitePreferences.Insert (SqlitePreferences.PersonSelectWinImages, "True");
				SqlitePreferences.Insert (SqlitePreferences.ExportGraphWidth, "900");
				SqlitePreferences.Insert (SqlitePreferences.ExportGraphHeight, "600");

				currentVersion = updateVersion("2.13");
			}
			if(currentVersion == "2.13")
			{
				LogB.SQL("Doing alter table run, runInterval, tempRunInterval add datetime");
				try {
					//sqlite does not have drop column
					executeSQL("ALTER TABLE " + Constants.RunTable + " ADD COLUMN datetime TEXT;");
					executeSQL("ALTER TABLE " + Constants.RunIntervalTable + " ADD COLUMN datetime TEXT;");
					executeSQL("ALTER TABLE " + Constants.TempRunIntervalTable + " ADD COLUMN datetime TEXT;");
				} catch {
					LogB.SQL("Catched at Doing alter table run, runInterval, tempRunInterval add datetime.");

				}
				LogB.SQL("Done!");

				currentVersion = updateVersion("2.14");
			}
			if(currentVersion == "2.14")
			{
				LogB.SQL("Inserted into preferences: SessionLoadDisplay");

				SqlitePreferences.Insert (SqlitePreferences. SessionLoadDisplay, "0");

				currentVersion = updateVersion("2.15");
			}
			if(currentVersion == "2.15")
			{
				LogB.SQL("Created table lastJumpSimpleTypeParams");

				SqliteJumpType.createTableLastJumpSimpleTypeParams();

				currentVersion = updateVersion("2.16");
			}
			if(currentVersion == "2.16")
			{
				LogB.SQL("Created table lastJumpRjTypeParams");

				SqliteJumpType.createTableLastJumpRjTypeParams();

				currentVersion = updateVersion("2.17");
			}
			if(currentVersion == "2.17")
			{
				if(! person77AddedLinkServerImage)
				{
					LogB.SQL("Person77 adding field: linkServerImage (for networks)");
					try {
						executeSQL("ALTER TABLE " + Constants.PersonTable + " ADD COLUMN linkServerImage TEXT;");
					} catch {
						LogB.SQL("Catched. maybe person77.linkServerImage already exists.");

					}
				}

				currentVersion = updateVersion("2.18");
			}
			if(currentVersion == "2.18")
			{
				LogB.SQL("Doing alter table encoderExercise ADD COLUMN type TEXT ...");
				try {
					executeSQL("ALTER TABLE " + Constants.EncoderExerciseTable + " ADD COLUMN type TEXT DEFAULT \"ALL\";");
				} catch {
					LogB.SQL("Catched at Doing alter table encoderExercise ADD COLUMN type TEXT ...");
				}
				LogB.SQL("Done!");

				currentVersion = updateVersion("2.19");
			}
			if(currentVersion == "2.19")
			{
				LogB.SQL("Inserted into preferences: runsEvolutionOnlyBestInSession");

				SqlitePreferences.Insert (SqlitePreferences.RunsEvolutionOnlyBestInSession, "False");

				currentVersion = updateVersion("2.20");
			}
			if(currentVersion == "2.20")
			{
				LogB.SQL("Inserted into preferences: runsEvolutionShowTime");

				SqlitePreferences.Insert (SqlitePreferences.RunsEvolutionShowTime, "False");

				currentVersion = updateVersion("2.21");
			}
			if(currentVersion == "2.21")
			{
				LogB.SQL("Inserted forceSensorFeedbackPath params");

				SqlitePreferences.Insert (SqlitePreferences.ForceSensorFeedbackPathMax, "100");
				SqlitePreferences.Insert (SqlitePreferences.ForceSensorFeedbackPathMin, "0");
				SqlitePreferences.Insert (SqlitePreferences.ForceSensorFeedbackPathMasters, "8");
				SqlitePreferences.Insert (SqlitePreferences.ForceSensorFeedbackPathMasterSeconds, "2");
				SqlitePreferences.Insert (SqlitePreferences.ForceSensorFeedbackPathLineWidth, "33");

				currentVersion = updateVersion("2.22");
			}
			if(currentVersion == "2.22")
			{
				LogB.SQL("Inserted socialNetwork variables at preferences");

				SqlitePreferences.Insert (SqlitePreferences.SocialNetwork, "");
				SqlitePreferences.Insert (SqlitePreferences.SocialNetworkDatetime, "");

				currentVersion = updateVersion("2.23");
			}
			if(currentVersion == "2.23")
			{
				LogB.SQL("LogoAnimatedShow made False by default on Mac (thinking on Big Sur problems)");

				if(os == UtilAll.OperatingSystems.MACOSX)
					SqlitePreferences.Update (SqlitePreferences.LogoAnimatedShow, "False", true);

				currentVersion = updateVersion("2.24");
			}
			if(currentVersion == "2.24")
			{
				LogB.SQL("encoderCaptureShowOnlyBars now uses BooleansInt");

				if(SqlitePreferences.Select("encoderCaptureShowOnlyBars", true) == "True")
				{
					EncoderCaptureDisplay ecd = new EncoderCaptureDisplay(false, false, true);
					SqlitePreferences.Update ("encoderCaptureShowOnlyBars", ecd.GetInt.ToString(), true);
				} else {
					EncoderCaptureDisplay ecd = new EncoderCaptureDisplay(true, true, true);
					SqlitePreferences.Update ("encoderCaptureShowOnlyBars", ecd.GetInt.ToString(), true);
				}

				currentVersion = updateVersion("2.25");
			}
			if(currentVersion == "2.25")
			{
				LogB.SQL("contactsCaptureDisplay with BooleansInt, and bool runEncoderCaptureDisplaySimple");

				SqlitePreferences.Insert(SqlitePreferences.ContactsCaptureDisplayStr,
						new ContactsCaptureDisplay(false, true).GetInt.ToString());
				SqlitePreferences.Insert(SqlitePreferences.RunEncoderCaptureDisplaySimple, "True");

				currentVersion = updateVersion("2.26");
			}
			if(currentVersion == "2.26")
			{
				LogB.SQL("Inserted lastBackupDir, lastBackupDatetime, backupScheduledCreatedDate, backupScheduledNextDays");

				SqlitePreferences.Insert (SqlitePreferences.LastBackupDirStr, "");
				SqlitePreferences.Insert (SqlitePreferences.LastBackupDatetimeStr, UtilDate.ToSql(DateTime.MinValue));
				SqlitePreferences.Insert (SqlitePreferences.BackupScheduledCreatedDateStr, UtilDate.ToSql(DateTime.MinValue));
				SqlitePreferences.Insert (SqlitePreferences.BackupScheduledNextDaysStr, "30");

				currentVersion = updateVersion("2.27");
			}
			if(currentVersion == "2.27")
			{
				LogB.SQL("Inserted at preferences showJumpRSI");
				SqlitePreferences.Insert (SqlitePreferences.ShowJumpRSI, "True");
				currentVersion = updateVersion("2.28");
			}
			if(currentVersion == "2.28")
			{
				LogB.SQL("ForceSensor ALTER TABLE added maxForceRaw, maxAvgForce1s");
				/*
				   we need this try/catch and in the rest of the ALTER TABLEs
				   because eg. in migration from 1.68 forceSensor table is created with all columns (including these two)
				   lack of this try/catch was the problem of Chronojump 2.2.0 in migration from 1.9.0 or older
				 */
				try {
					executeSQL("ALTER TABLE " + Constants.ForceSensorTable + " ADD COLUMN maxForceRaw FLOAT DEFAULT -1;");
					executeSQL("ALTER TABLE " + Constants.ForceSensorTable + " ADD COLUMN maxAvgForce1s FLOAT DEFAULT -1;");
				} catch {
					LogB.SQL("Catched at Doing ALTER TABLE added maxForceRaw, maxAvgForce1s. Probably forceSensorTable has been created with this columns already added.");
				}
				currentVersion = updateVersion("2.29");
			}
			if(currentVersion == "2.29")
			{
				LogB.SQL("Inserted default exercises of forceSensor and raceAnalyzer if empty");

				ArrayList exercises = SqliteForceSensorExercise.Select(true, -1, -1, true, "");
				if(exercises == null || exercises.Count == 0)
					SqliteForceSensorExercise.insertDefault();

				/* moved to 2.31 to do it after RunEncoderExercise added segmentVariableCm
				exercises = SqliteRunEncoderExercise.Select(true, -1, true);
				if(exercises == null || exercises.Count == 0)
					SqliteRunEncoderExercise.insertDefault();
					*/

				currentVersion = updateVersion("2.30");
			}
			if(currentVersion == "2.30")
			{
				LogB.SQL("RunEncoderExercise ALTER TABLE added segmentVariableCm");
				try {
					executeSQL("ALTER TABLE " + Constants.RunEncoderExerciseTable + " ADD COLUMN segmentVariableCm TEXT;");
				} catch {
					LogB.SQL("Catched at Doing ALTER TABLE added segmentVariableCm.");
				}

				currentVersion = updateVersion("2.31");
			}
			if(currentVersion == "2.31")
			{
				LogB.SQL("RunEncoderExercise ALTER TABLE added isSprint");
				try {
					executeSQL("ALTER TABLE " + Constants.RunEncoderExerciseTable + " ADD COLUMN isSprint INT NOT NULL DEFAULT 1;");
				} catch {
					LogB.SQL("Catched at Doing ALTER TABLE added isSprint.");
				}

				currentVersion = updateVersion("2.32");
			}
			if(currentVersion == "2.32")
			{
				LogB.SQL("RunEncoderExercise segmentMeters but now is in cm");

				List<RunEncoderExercise> ex_l = SqliteRunEncoderExercise.Select (true, -1);
				foreach(RunEncoderExercise ex in ex_l)
				{
					if(ex.SegmentCm > 0) //do not update them if is -1 (unused)
					{
						RunEncoderExercise exChanged = ex;
						exChanged.SegmentCm *= 100;
						SqliteRunEncoderExercise.Update(true, exChanged);
					}
				}

				currentVersion = updateVersion("2.33");
			}
			if(currentVersion == "2.33")
			{
				LogB.SQL("Fixed duplicated names of exercises on modes encoder, forceSensor, raceAnalyzer caused by import bug");

				fixDuplicatedExercises(Constants.EncoderExerciseTable);
				fixDuplicatedExercises(Constants.ForceSensorExerciseTable);
				fixDuplicatedExercises(Constants.RunEncoderExerciseTable);

				currentVersion = updateVersion("2.34");
			}
			if(currentVersion == "2.34")
			{
				LogB.SQL("Ensure maxForceRAW is converted to maxForceRaw");
				/*
				   Chronojump 2.2.0 (published on 10 jun 2022) has maxForceRAW on ALTER TABLE (on git since 2 jun 2022)
				   Chronojump 2.2.0-12 (18 jun) Changed to maxForceRaw, on C# is the same but on Python3 (importer) could lead to problems
				   Sqlite3 RENAME COLUMN is on Sqlite since 3.25.0 15-9-2018, so no problem on using on Linux and hopefully on Mac
				   On windows not sure if it will be available on sqlite that is installed with Chronojump
				   if this is not solved, Chronojump importer will import all except the forceSensor data,
				   solution will be to install on client machine sqlite3 and run there the command
				   "ALTER TABLE forceSensor RENAME COLUMN maxForceRAW TO maxForceRaw";
				 */

				try {
					if(columnExists(true, Constants.ForceSensorTable, "maxForceRAW", true))
					{
						LogB.SQL("renaming column maxForceRAW...");

						//on Windows with cerbero compilation, sqlite implementation is very old (previous to 3.25.0)
						//so there is no RENAME COLUMN and we need to do it in old way
						if(! UtilAll.IsWindows())
							renameColumnLinuxOrMac (Constants.ForceSensorTable, "maxForceRAW", "maxForceRaw");
						else
						{
							using(SqliteTransaction tr = dbcon.BeginTransaction())
							{
								using (SqliteCommand dbcmdTr = dbcon.CreateCommand())
								{
									dbcmdTr.Transaction = tr;

									//1. create table temp (dropping it first if exists)
									SqliteForceSensor.createTable_windows_forceSensor_db_2_34_migration (dbcmdTr, "forceSensorTemp");

									//2. copy data
									dbcmdTr.CommandText = "INSERT INTO forceSensorTemp SELECT * from forceSensor";
									LogB.SQL(dbcmdTr.CommandText.ToString());
									dbcmdTr.ExecuteNonQuery();

									//3. drop initial table
									dbcmdTr.CommandText = "DROP TABLE forceSensor";
									LogB.SQL(dbcmdTr.CommandText.ToString());
									dbcmdTr.ExecuteNonQuery();

									//4. rename table (this works on old sqlite implementations, tested on our cerbero)
									dbcmdTr.CommandText = "ALTER TABLE forceSensorTemp RENAME TO forceSensor";
									LogB.SQL(dbcmdTr.CommandText.ToString());
									dbcmdTr.ExecuteNonQuery();
								}
								tr.Commit();
							}
						}
						LogB.SQL("renamed.");
					}
				} catch {
					LogB.SQL("Catched checking if maxForceRAW exists of renaming to maxForceRaw");
				}

				currentVersion = updateVersion("2.35");
			}
			if(currentVersion == "2.35")
			{
				LogB.SQL("Inserted into preferences: encoderRepetitionCriteriaGravitatory, encoderRepetitionCriteriaInertial");

				SqlitePreferences.Insert (
						SqlitePreferences.EncoderRepetitionCriteriaGravitatoryStr,
						Preferences.EncoderRepetitionCriteria.CON.ToString());
				SqlitePreferences.Insert (
						SqlitePreferences.EncoderRepetitionCriteriaInertialStr,
						Preferences.EncoderRepetitionCriteria.CON.ToString());

				currentVersion = updateVersion("2.36");
			}
			if(currentVersion == "2.36")
			{
				LogB.SQL("Doing ALTER TABLE encoder add repCriteria.");

				try {
					executeSQL("ALTER TABLE " + Constants.EncoderTable +
							" ADD COLUMN repCriteria TEXT NOT NULL DEFAULT " + //TODO: try to do the migration and see if overview works ok
							Preferences.EncoderRepetitionCriteria.CON.ToString() + ";");
							//note this will make the encoder sets (signals) have CON on this columns, but this will not be used on signals, only on curves
				} catch {
					LogB.SQL("Catched at Doing ALTER TABLE encoder add repCriteria.");
				}

				currentVersion = updateVersion("2.37");
			}
			if(currentVersion == "2.37")
			{
				LogB.SQL("Doing alter table runInterval, tempRunInterval add photocellStr");
				try {
					//sqlite does not have drop column
					executeSQL("ALTER TABLE " + Constants.RunIntervalTable + " ADD COLUMN photocellStr TEXT;");
					executeSQL("ALTER TABLE " + Constants.TempRunIntervalTable + " ADD COLUMN photocellStr TEXT;");
				} catch {
					LogB.SQL("Catched at Doing alter table runInterval, tempRunInterval add photocellStr.");

				}
				LogB.SQL("Done!");

				currentVersion = updateVersion("2.38");
			}
			if(currentVersion == "2.38")
			{
				LogB.SQL("RunEncoderExercise ALTER TABLE added angleDefault. RunEncoder ALTER TABLE added angle.");
				try {
					executeSQL("ALTER TABLE " + Constants.RunEncoderExerciseTable + " ADD COLUMN angleDefault INT NOT NULL DEFAULT 0;");
				} catch {
					LogB.SQL("Catched at Doing ALTER TABLE RunEncoderExercise added angleDefault.");
				}

				try {
					executeSQL("ALTER TABLE " + Constants.RunEncoderTable + " ADD COLUMN angle INT NOT NULL DEFAULT 0;");
				} catch {
					LogB.SQL("Catched at Doing ALTER TABLE RunEncoder added angle.");
				}

				//adding the insertDefault here because now it will be in cm
				List<RunEncoderExercise> ex_l = SqliteRunEncoderExercise.Select (true, -1);
				if(ex_l == null || ex_l.Count == 0)
					SqliteRunEncoderExercise.insertDefault();

				currentVersion = updateVersion("2.39");
			}
			if(currentVersion == "2.39")
			{
				LogB.SQL("ForceSensor exercises raw are now both (isometric & elastic) because there was a bug creating raw exercises (elastic was not asked and was assigned true) and we don't know where to put them.");

				SqliteForceSensorExercise.UpdateTo2_40 ();

				currentVersion = updateVersion("2.40");
			}
			if(currentVersion == "2.40")
			{
				//to have less problems, eg on SqliteJump.SelectJumpsStatsByDay do not assign date to a grouped jumps from different sessions
				LogB.SQL("Tests without datetime: jump (db 1.81), jumpRj (db 1.81), run (db 2.13), runI (2.13) now have session date (and 00-00-01 time)");

				updateTo2_41 ();

				currentVersion = updateVersion("2.41");
			}
			if(currentVersion == "2.41")
			{
				LogB.SQL("Added ForceSensorVariabilityLag");
				SqlitePreferences.Insert (SqlitePreferences.ForceSensorVariabilityLag, "1");
				currentVersion = updateVersion("2.42");
			}
			if(currentVersion == "2.42")
			{
				LogB.SQL("Inserted into preferences: lastPersonID");
				SqlitePreferences.Insert (SqlitePreferences.LastPersonID, "-1");
				currentVersion = updateVersion("2.43");
			}
			if(currentVersion == "2.43")
			{
				LogB.SQL("Converted all encoder.future(1|2|3) from , to .");
				executeSQL("UPDATE " + Constants.EncoderTable + " SET future1 = REPLACE (future1, ',', '.')");
				executeSQL("UPDATE " + Constants.EncoderTable + " SET future2 = REPLACE (future2, ',', '.')");
				executeSQL("UPDATE " + Constants.EncoderTable + " SET future3 = REPLACE (future3, ',', '.')");
				currentVersion = updateVersion("2.44");
			}
			if(currentVersion == "2.44")
			{
				LogB.SQL("Added JumpsRj, RunsI feedback variables");

				SqlitePreferences.insertJumpsRjRunsIFeedback2_45 (dbcmd);

				currentVersion = updateVersion("2.45");
			}
			if(currentVersion == "2.45")
			{
				LogB.SQL("Added two missing RunsI feedback variables: RunsIFeedbackShowBestSpeed, RunsIFeedbackShowWorstSpeed");

				SqlitePreferences.insertJumpsRjRunsIFeedback2_46 (dbcmd);

				currentVersion = updateVersion("2.46");
			}
			if(currentVersion == "2.46")
			{
				LogB.SQL("Added RFDs 5-10");
				SqliteForceSensorRFD.UpdateTo2_47 ();
				currentVersion = updateVersion("2.47");
			}

			/*
			if(currentVersion == "1.79")
			{
				LogB.SQL("Created table ForceSensorElasticBandGlue and moved stiffnessString records there");

				SqliteForceSensorElasticBandGlue.createTable();

				//TODO: move everything here
				SqliteForceSensor.import_from_1_79_to_1_80();

				currentVersion = updateVersion("1.80");
			}
			*/



			// --- add more updates here
		

			
			// --- end of update, close DB

			LogB.SQL("Closing Sqlite after DB updates");

			Sqlite.Close(); //------------------------------------------------

			//LogB.PrintAllThreads = false; //comment this
		}

		//if changes are made here, remember to change also in CreateTables()
		//remember to change also the databaseVersion below
		
		return returnSoftwareIsNew;
	}
	
	private static void executeSQL(string command)
	{
		dbcmd.CommandText = command;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}

	private static string updateVersion(string newVersion) {
		SqlitePreferences.Update ("databaseVersion", newVersion, true); 
		return newVersion;
	}

	public static bool ChangeDjToDJna() {
		string v = SqlitePreferences.Select("databaseVersion");
		LogB.SQL(Convert.ToDouble(Util.ChangeDecimalSeparator(v)).ToString());
		if(Convert.ToDouble(Util.ChangeDecimalSeparator(v)) < Convert.ToDouble(Util.ChangeDecimalSeparator("0.74")))
			return true;
		return false;
	}

	private static void addChronopicPortNameIfNotExists() {
		string myPort = SqlitePreferences.Select("chronopicPort");
		if(myPort == "0") {
			//if doesn't exist (for any reason, like old database)
			Sqlite.Open();
			if(UtilAll.IsWindows() || creatingBlankDatabase)
				SqlitePreferences.Insert ("chronopicPort", Constants.ChronopicDefaultPortWindows);
			else
				SqlitePreferences.Insert ("chronopicPort", Constants.ChronopicDefaultPortLinux);
			Sqlite.Close();
			
			LogB.SQL("Added Chronopic port");
		}
	}
	
	public static void CreateTables(bool server)
	{
		Sqlite.Open();

		creationTotal = 14;
		creationRate = 1;

		SqliteServer sqliteServerObject = new SqliteServer();
		//user has also an evaluator table with a row (it's row)	
		sqliteServerObject.CreateEvaluatorTable();
		
		if(server) {
			sqliteServerObject.CreatePingTable();
			
			SqliteServerSession sqliteSessionObject = new SqliteServerSession();
			sqliteSessionObject.createTable(Constants.SessionTable);
		} else {
			SqliteSession sqliteSessionObject = new SqliteSession();
			sqliteSessionObject.createTable(Constants.SessionTable);
			
			//add SIMULATED session if doesn't exists. Unique session where tests can be simulated.
			SqliteSession.insertSimulatedSession();
			
			SqlitePersonSessionNotUpload.CreateTable();
			creationRate ++;
		}

		SqliteTagSession.createTable();
		SqliteSessionTagSession.createTable();
		SqliteNews.createTable();

		SqlitePerson sqlitePersonObject = new SqlitePerson();
		sqlitePersonObject.createTable(Constants.PersonTable);

		//graphLinkTable
		SqliteEvent.createGraphLinkTable();
		creationRate ++;
		
		//jumps
		SqliteJump sqliteJumpObject = new SqliteJump();
		SqliteJumpRj sqliteJumpRjObject = new SqliteJumpRj();
		sqliteJumpObject.createTable(Constants.JumpTable);
		sqliteJumpRjObject.createTable(Constants.JumpRjTable);
		sqliteJumpRjObject.createTable(Constants.TempJumpRjTable);

		//jump Types
		creationRate ++;
		SqliteJumpType.createTableJumpType();
		SqliteJumpType.createTableJumpRjType();
		SqliteJumpType.initializeTableJumpType();
		SqliteJumpType.initializeTableJumpRjType();
		SqliteJumpType.createTableLastJumpSimpleTypeParams();
		SqliteJumpType.createTableLastJumpRjTypeParams();
		
		//runs
		creationRate ++;
		SqliteRun sqliteRunObject = new SqliteRun();
		SqliteRunInterval sqliteRunIntervalObject = new SqliteRunInterval();
		sqliteRunObject.createTable(Constants.RunTable);
		sqliteRunIntervalObject.createTable(Constants.RunIntervalTable);
		sqliteRunIntervalObject.createTable(Constants.TempRunIntervalTable);
		
		//run Types
		creationRate ++;
		SqliteRunType sqliteRunTypeObject = new SqliteRunType();
		sqliteRunTypeObject.createTable(Constants.RunTypeTable);
		SqliteRunType.initializeTable();

		SqliteRunIntervalType sqliteRunIntervalTypeObject = new SqliteRunIntervalType();
		sqliteRunIntervalTypeObject.createTable(Constants.RunIntervalTypeTable);
		SqliteRunIntervalType.initializeTable();
		
		//reactionTimes
		creationRate ++;
		SqliteReactionTime sqliteReactionTimeObject = new SqliteReactionTime();
		sqliteReactionTimeObject.createTable(Constants.ReactionTimeTable);
		
		//pulses and pulseTypes
		creationRate ++;
		SqlitePulse sqlitePulseObject = new SqlitePulse();
		sqlitePulseObject.createTable(Constants.PulseTable);
		SqlitePulseType.createTablePulseType();
		SqlitePulseType.initializeTablePulseType();
		
		//multiChronopic tests		
		creationRate ++;
		SqliteMultiChronopic sqliteMultiChronopicObject = new SqliteMultiChronopic();
		sqliteMultiChronopicObject.createTable(Constants.MultiChronopicTable);
	
		//encoder	
		creationRate ++;
		SqliteEncoder.createTableEncoder();
		SqliteEncoder.createTableEncoderSignalCurve();
		SqliteEncoder.createTableEncoderExercise();
		SqliteEncoder.initializeTableEncoderExercise();
		SqliteEncoder.createTable1RM();

		//encoderConfiguration
		SqliteEncoderConfiguration.createTableEncoderConfiguration();
		SqliteEncoderConfiguration.insertDefault(Constants.EncoderGI.GRAVITATORY);
		SqliteEncoderConfiguration.insertDefault(Constants.EncoderGI.INERTIAL);

		//sports
		creationRate ++;
		SqliteSport.createTable();
		SqliteSport.initialize();
		SqliteSpeciallity.createTable();
		SqliteSpeciallity.initialize();
		SqliteSpeciallity.InsertUndefined(true);
				
		creationRate ++;
		SqlitePersonSession sqlitePersonSessionObject = new SqlitePersonSession();
		sqlitePersonSessionObject.createTable(Constants.PersonSessionTable);
		
		creationRate ++;
		SqliteCountry.createTable();
		SqliteCountry.initialize();
				
		SqliteExecuteAuto.createTableExecuteAuto();
		SqliteExecuteAuto.addChronojumpProfileAndBilateral();

		SqliteChronopicRegister.createTableChronopicRegister();
		SqliteTrigger.createTableTrigger();

		//forceSensor
		SqliteForceSensor.createTable();
		SqliteForceSensorExercise.createTable();
		SqliteForceSensorExercise.insertDefault();
		SqliteForceSensorRFD.createTable();
		SqliteForceSensorRFD.InsertDefaultValues(true);
		SqliteForceSensorElasticBand.createTable();
		//SqliteForceSensorElasticBandGlue.createTable();

		//runEncoder
		SqliteRunEncoder.createTable();
		SqliteRunEncoderExercise.createTable();
		SqliteRunEncoderExercise.insertDefault();

		creationRate ++;
		SqlitePreferences.createTable();
		SqlitePreferences.initializeTable(lastChronojumpDatabaseVersion, creatingBlankDatabase);

		//compujump json temp tables
		SqliteJson.createTableUploadEncoderDataTemp ();
		SqliteJson.createTableUploadSprintDataTemp ();
		SqliteJson.createTableUploadExhibitionTestTemp ();

		//changes [from - to - desc]
//just testing: 1.79 - 1.80 Converted DB to 1.80 Created table ForceSensorElasticBandGlue and moved stiffnessString records there

		//2.46 - 2.47 Converted DB to 2.47 Added RFDs 5-10
		//2.45 - 2.46 Converted DB to 2.46 Added two missing RunsI feedback variables: RunsIFeedbackShowBestSpeed, RunsIFeedbackShowWorstSpeed
		//2.44 - 2.45 Converted DB to 2.45 Added JumpsRj, RunsI feedback variables
		//2.43 - 2.44 Converted DB to 2.44 Converted all encoder.future(1|2|3) from , to .
		//2.42 - 2.43 Converted DB to 2.43 Inserted into preferences: lastPersonID
		//2.41 - 2.42 Converted DB to 2.42 Added ForceSensorVariabilityLag
		//2.40 - 2.41 Converted DB to 2.41 Tests without datetime: jump (db 1.81), jumpRj (db 1.81), run (db 2.13), runI (2.13) now have session date (and 00-00-01 time)
		//2.39 - 2.40 Converted DB to 2.40 ForceSensor exercises raw are now both (isometric & elastic) because there was a bug creating raw exercises (elastic was not asked and was assigned true) and we don't know where to put them.
		//2.38 - 2.39 Converted DB to 2.39 RunEncoderExercise ALTER TABLE added angleDefault. RunEncoder ALTER TABLE added angle
		//2.37 - 2.38 Converted DB to 2.38 Doing alter table runInterval, tempRunInterval add photocellStr
		//2.36 - 2.37 Converted DB to 2.37 Doing ALTER TABLE encoder add repCriteria.
		//2.35 - 2.36 Converted DB to 2.36 Inserted into preferences: encoderRepetitionCriteriaGravitatory, encoderRepetitionCriteriaInertial
		//2.34 - 2.35 Converted DB to 2.35 Ensure maxForceRAW is converted to maxForceRaw
		//2.33 - 2.34 Converted DB to 2.34 Fixed duplicated names of exercises on encoder, forceSensor, raceAnalyzer caused by import bug
		//2.32 - 2.33 Converted DB to 2.33 RunEncoderExercise segmentMeters but now is in cm
		//2.31 - 2.32 Converted DB to 2.32 RunEncoderExercise ALTER TABLE added isSprint
		//2.30 - 2.31 Converted DB to 2.31 RunEncoderExercise ALTER TABLE added segmentVariableCm
		//2.29 - 2.30 Converted DB to 2.30 Inserted default exercises of forceSensor and raceAnalyzer if empty
		//2.28 - 2.29 Converted DB to 2.29 ForceSensor ALTER TABLE added maxForceRaw, maxAvgForce1s
		//2.27 - 2.28 Converted DB to 2.28 Inserted at preferences showJumpRSI
		//2.26 - 2.27 Converted DB to 2.27 Inserted lastBackupDir, lastBackupDatetime, backupScheduledCreatedDate, backupScheduledNextDays
		//2.25 - 2.26 Converted DB to 2.26 contactsCaptureDisplay with BooleansInt, and bool runEncoderCaptureDisplaySimple
		//2.24 - 2.25 Converted DB to 2.25 encoderCaptureShowOnlyBars now uses BooleansInt
		//2.23 - 2.24 Converted DB to 2.24 LogoAnimatedShow made False by default on Mac
		//2.22 - 2.23 Converted DB to 2.23 Inserted socialNetwork variables at preferences
		//2.21 - 2.22 Converted DB to 2.22 Inserted forceSensorFeedbackPath params
		//2.20 - 2.21 Converted DB to 2.21 Inserted into preferences: runsEvolutionShowTime
		//2.19 - 2.20 Converted DB to 2.20 Inserted into preferences: runsEvolutionOnlyBestInSession
		//2.18 - 2.19 Converted DB to 2.19 Doing alter table encoderExercise ADD COLUMN type TEXT ...
		//2.17 - 2.18 Converted DB to 2.18 Person77 ALTER TABLE added field: linkServerImage (for networks)
		//2.16 - 2.17 Converted DB to 2.17 Created table lastJumpRjTypeParams
		//2.15 - 2.16 Converted DB to 2.16 Created table lastJumpSimpleTypeParams
		//2.14 - 2.15 Converted DB to 2.15 Inserted into preferences: SessionLoadDisplay
		//2.13 - 2.14 Converted DB to 2.14 Doing alter table run, runInterval, tempRunInterval add datetime
		//2.12 - 2.13 Converted DB to 2.13 Inserted prefs: PersonSelectWinImages, ExportGraphWidth, ExportGraphHeight
		//2.11 - 2.12 Converted DB to 2.12 Inserted prefs: forceSensorAnalyzeMaxAVGInWindow
		//2.10 - 2.11 Converted DB to 2.11 Inserted prefs: clientNewsDatetime
		//2.09 - 2.10 Converted DB to 2.10 Inserted prefs: encoderCaptureShowLoss, runEncoderPPS
		//2.08 - 2.09 Converted DB to 2.09 Inserted into preferences: forceSensorAnalyzeABSliderIncrement
		//2.07 - 2.08 Converted DB to 2.08 Create table news and insert newsLanguageEs on preferences
		//2.06 - 2.07 Converted DB to 2.07 Inserted into preferences: forceSensorVariabilityMethod
		//2.05 - 2.06 Converted DB to 2.06 Default python version for all users: Python3
		//2.04 - 2.05 Converted DB to 2.05 Inserted into preferences: forceSensorStartEndOptimized
		//2.03 - 2.04 Converted DB to 2.04 Created tables: tagSession, sessionTagSession
		//2.02 - 2.03 Converted DB to 2.03 Inserted into preferences: encoderInertialGraphsX
		//2.01 - 2.02 Converted DB to 2.02 Inserted into preferences: restTimeMinutes, restTimeSeconds")
		//2.00 - 2.01 Converted DB to 2.01 RunEncoderExercise ALTER TABLE: added column segmentMeters
		//1.99 - 2.00 Converted DB to 2.00 Inserted into preferences: fontsOnGraphs
		//1.98 - 1.99 Converted DB to 1.99 Updated 3L3R tracks fixedValue (just affected description)
		//1.97 - 1.98 Converted DB to 1.98 Inserted into preferences: colorBackgroundOsColor
		//1.96 - 1.97 Converted DB to 1.97 Inserted into preferences: loadLastSessionAtStart, lastSessionID, loadLastModAtStart, lastMode
		//1.95 - 1.96 Converted DB to 1.96 Inserted into preferences: jumpsFVProfileOnlyBestInWeight, jumpsFVProfileShowFullGraph, jumpsEvolutionOnlyBestInSession
		//1.94 - 1.95 Converted DB to 1.95 Inserted into preferences: importerPythonVersion
		//1.93 - 1.94 Converted DB to 1.94 Inserted into preferences: RunEncoderAnalyzeAccel/Force/Power
		//1.92 - 1.93 Converted DB to 1.93 Inserted into preferences: EncoderCaptureInfinite, LogoAnimatedShow
		//1.91 - 1.92 Converted DB to 1.92 Inserted into preferences: menuType
		//1.90 - 1.91 Converted DB to 1.91 Inserted into preferences: ColorBackground
		//1.89 - 1.90 Converted DB to 1.90 Inserted into preferences: units
		//1.88 - 1.89 Converted DB to 1.89 Inserted into preferences: encoderCaptureFeedbackEccon
		//1.87 - 1.88 Converted DB to 1.88 Inserted into preferences: encoderCaptureInertialEccOverloadMode
		//1.86 - 1.87 Converted DB to 1.87 Doing alter table forceSensorExercise adding eccReps, eccMin, conMin.
		//1.85 - 1.86 Converted DB to 1.86 Inserted into preferences: RunEncoderMinAccel
		//1.84 - 1.85 Converted DB to 1.85 Inserted 5 vars into preferences: EncoderCaptureMainVariable...
		//1.83 - 1.84 Converted DB to 1.84 Inserted into preferences: forceSensorMIFDuration Mode/Seconds/Percent
		//1.82 - 1.83 Converted DB to 1.83 Added missing agility_t_test image
		//1.81 - 1.82 Converted DB to 1.82 Doing alter table jump, jumpRj, tempJumpRj add datetime
		//1.80 - 1.81 Converted DB to 1.81 Inserted forceSensorCaptureFeedbackActive /At /Range
		//1.79 - 1.80 Converted DB to 1.80 Inserted forceSensorElasticEccMinDispl, ...
		//1.78 - 1.79 Converted DB to 1.79 Inserted into preferences: encoderWorkKcal
		//1.77 - 1.78 Converted DB to 1.78 Inserted into preferences: encoderAutoSaveCurveBestNValue
		//1.76 - 1.77 Converted DB to 1.77 Inserted into preferences: forceSensorGraphsLineWidth
		//1.75 - 1.76 Converted DB to 1.76 ALTER TABLE " + Constants.ForceSensorTable + " ADD COLUMN (stiffness float, stiffnessString string)
		//1.74 - 1.75 Converted DB to 1.75 Created table ForceSensorElasticBand
		//1.73 - 1.74 Converted DB to 1.74 ALTER TABLE Constants.ForceSensorExerciseTable ADD COLUMN forceResultant, elastic
		//1.72 - 1.73 Converted DB to 1.73 Inserted into preferences: jumpsDjGraphHeights
		//1.71 - 1.72 Converted DB to 1.72 Inserted into preferences: forceSensorCaptureWidthSeconds, forceSensorCaptureScroll
		//1.70 - 1.71 Converted DB to 1.71 Imported run encoder text files into SQL
		//1.69 - 1.70 Converted DB to 1.70 Created tables: RunEncoder, RunEncoderExercise
		//1.68 - 1.69 Converted DB to 1.69 Imported force sensor text files into SQL
		//1.67 - 1.68 Converted DB to 1.68 Created table: ForceSensor
		//1.66 - 1.67 Converted DB to 1.67 ALTER TABLE Constants.ForceSensorExerciseTable ADD COLUMN tareBeforeCapture INT
		//1.65 - 1.66 Converted DB to 1.66 Added to preferences: encoderCaptureSecondaryVariableShow
		//1.64 - 1.65 Converted DB to 1.65 Added to preferences: encoderCaptureSecondaryVariable
		//1.63 - 1.64 Converted DB to 1.64 Added to preferences: videoDevicePixelFormat
		//1.62 - 1.63 Converted DB to 1.63 Added to preferences: encoderCaptureInertialDiscardFirstN
		//1.61 - 1.62 Converted DB to 1.62 Added to preferences: videoStopAfter
		//1.60 - 1.61 Converted DB to 1.61 Added to preferences: videoDeviceResolution, videoDeviceFramerate
		//1.59 - 1.60 Converted DB to 1.60 Created table UploadExhibitionTestTemp
		//1.58 - 1.59 Converted DB to 1.59 Created ForceSensorExercise
		//1.57 - 1.58 Converted DB to 1.58 Added to preferences: encoderCaptureShowNRepetitions
		//1.56 - 1.57 Converted DB to 1.57 Created table UploadEncoderDataTemp, UploadSprintDateTemp
		//1.55 - 1.56 Converted DB to 1.56 Added encoder rhythm restAfterEcc
		//1.54 - 1.55 Converted DB to 1.55 Added preferences: personPhoto
		//1.53 - 1.54 Converted DB to 1.54 Added encoderRhythm variables: repOrPhases, repSeconds
		//1.52 - 1.53 Converted DB to 1.53 Added encoderRhtyhm active variable
		//1.51 - 1.52 Converted DB to 1.52 Added encoderRhtyhm stuff
		//1.50 - 1.51 Converted DB to 1.51 Updated encoderCaptureCutByTriggers variable
		//1.49 - 1.50 Converted DB to 1.50 Updated preferences: added crashLogLanguage
		//1.48 - 1.49 Converted DB to 1.49 Updated preferences: added force sensor tare/calibration stuff
		//1.47 - 1.48 Converted DB to 1.48 Updated preferences: added gstreamer
		//1.46 - 1.47 Converted DB to 1.47 Added encoderCaptureBarplotFontSize at preferences
		//1.45 - 1.46 Converted DB to 1.46 Added muteLogs at preferences
		//1.44 - 1.45 Converted DB to 1.45 Added ForceSensorImpulse value
		//1.43 - 1.44 Converted DB to 1.44 Added encoderCaptureCutByTriggers to preferences
		//1.42 - 1.43 Converted DB to 1.43 Added exercise params of last capture for next Chronojump start
		//1.41 - 1.42 Converted DB to 1.42 Created and default values for ForceSensorRFD
		//1.40 - 1.41 Converted DB to 1.41 Updated preferences maximized: from true/false to no/yes/undecorated
		//1.39 - 1.40 Converted DB to 1.40 Added to preferences: maximized, personWinHide, encoderCaptureShowOnlyBars
		//1.38 - 1.39 Converted DB to 1.39 Created trigger table 
		//1.37 - 1.38 Converted DB to 1.38 encoderConfiguration always with 12 values. Empty encoderConfiguration list_d as '' instead of '-1' or '0'
		//1.36 - 1.37 Converted DB to 1.37 Deleted encoderConfiguration variable. Added encoderConfiguration table (1.36)
		//1.35 - 1.36 Converted DB to 1.36 Deleted encoderConfiguration table
		//1.34 - 1.35 Converted DB to 1.35 Added encoderConfiguration table
		//1.33 - 1.34 Converted DB to 1.34 Added thresholdJumps, thresholdRuns, thresholdOther to preferences
		//1.32 - 1.33 Converted DB to 1.33 Added chronopicRegister table
		//1.31 - 1.32 Converted DB to 1.32 encoderCaptureOptionsWin -> preferences
		//1.30 - 1.31 Converted DB to 1.31 Insert encoderCaptureCheckFullyExtended and ...Value at preferences
		//1.29 - 1.30 Converted DB to 1.30 Added SIMULATED session
		//1.28 - 1.29 Converted DB to 1.29 Changed reaction time rows have reactionTime as default value
		//1.27 - 1.28 Converted DB to 1.28 Changed encoderAutoSaveCurve BESTMEANPOWER to BEST
		//1.26 - 1.27 Converted DB to 1.27 Changing runDoubleContactsMS and runIDoubleContactsMS from 1000ms to 300ms
		//1.25 - 1.26 Converted DB to 1.26 Changed Inclinated to Inclined
		//1.24 - 1.25 Converted DB to 1.25 Language defaults to (empty string), means detected
		//1.23 - 1.24 Converted DB to 1.24 Delete runISpeedStartArrival and add 4 double contacts configs
		//1.22 - 1.23 Converted DB to 1.23 Added encoder configuration
		//1.21 - 1.22 Converted DB to 1.22 Encoder laterality in english again
		//1.20 - 1.21 Converted DB to 1.21 Fixing loosing of encoder videoURL after recalculate
		//1.19 - 1.20 Converted DB to 1.20 Preferences: added user email
		//1.18 - 1.19 Converted DB to 1.19 Preferences deleted showHeight, added showStiffness
		//1.17 - 1.18 Converted DB to 1.18 deleted Negative runInterval runs (bug from last version)
		//1.16 - 1.17 Converted DB to 1.17 Deleted Max jump (we already have "Free")
		//1.15 - 1.16 Converted DB to 1.16 Cyprus moved to Europe
		//1.14 - 1.15 Converted DB to 1.15 added Chronojump profile and bilateral profile
		//1.13 - 1.14 Converted DB to 1.14 slCMJ -> slCMJleft, slCMJright
		//1.12 - 1.13 Converted DB to 1.13 Added ExecuteAuto table
		//1.11 - 1.12 Converted DB to 1.12 URLs from absolute to relative
		//1.10 - 1.11 Converted DB to 1.11 Added option on autosave curves on capture (all/bestmeanpower/none)
		//1.09 - 1.10 Converted DB to 1.10 Added RSA RAST on runType
		//1.08 - 1.09 Converted DB to 1.09 Added option on preferences to useHeightsOnJumpIndexes (default) or not
		//1.07 - 1.08 Converted DB to 1.08 Added translate statistics graph option to preferences
		//1.06 - 1.07 Converted DB to 1.07 Added jump_dj_a.png
		//1.05 - 1.06 Converted DB to 1.06 Curves are now linked to signals
		//1.04 - 1.05 Converted DB to 1.05 Removed inertial curves, because sign was not checked on 1.04 when saving curves
		//1.03 - 1.04 Converted DB to 1.04 Encoder table improved
		//1.02 - 1.03 Converted DB to 1.03 Updated encoder exercise, angle is now on encoder configuration
		//1.01 - 1.02 Converted DB to 1.02 Added Agility Tests: Agility-T-Test, Agility-3L3R
		//1.00 - 1.01 Converted DB to 1.01 Added export to CSV configuration on preferences
		//0.99 - 1.00 Converted DB to 1.00 Encoder added Free and Inclined Exercises
		//0.98 - 0.99 Converted DB to 0.99 Encoder table improved 
		//0.97 - 0.98 Converted DB to 0.98 Fixed encoder laterality
		//0.96 - 0.97 Converted DB to 0.97 Added inertialmomentum in preferences
		//0.95 - 0.96 Converted DB to 0.96 Encoder signal future3 three modes
		//0.94 - 0.95 Converted DB to 0.95 Added encoder1RMMethod
		//0.93 - 0.94 Converted DB to 0.94 Added encoder1RM table
		//0.92 - 0.93 Converted DB to 0.93 Added speed1RM on encoder exercise
		//0.91 - 0.92 Converted DB to 0.92 Added videoDevice to preferences
		//0.90 - 0.91 Converted DB to 0.91 Encoder Squat 75% -> 100%
		//0.89 - 0.90 Converted DB to 0.90 Preferences added propulsive and encoder smooth
		//0.88 - 0.89 Converted DB to 0.89 Added encoder exercise: Free
		//0.87 - 0.88 Converted DB to 0.88 Deleted fake RSA test and added known RSA tests
		//0.86 - 0.87 Converted DB to 0.87 Added run speed start preferences on sqlite
		//0.85 - 0.86 Converted DB to 0.86 videoOn: TRUE
		//0.84 - 0.85 Converted DB to 0.85 Added slCMJ jump 
		//0.83 - 0.84 Converted DB to 0.84 Added first RSA test 
		//0.82 - 0.83 Converted DB to 0.83 Created encoder table
		//0.81 - 0.82 Converted DB to 0.82 Added videoOn 
		//0.80 - 0.81 Converted DB to 0.81 Added tempRunInterval initial speed
		//0.79 - 0.80 Converted DB to 0.80 Added run and runInterval initial speed (if not done in 0.56 conversion)
		//0.78 - 0.79 Converted DB to 0.79 (Added multimediaStorage structure id)
		//0.77 - 0.78 Converted DB to 0.78 (Added machineID to preferences, takeOffWeight has no weight in db conversions since 0.66)
		//0.76 - 0.77 Converted DB to 0.77 (person77, personSession77)
		//0.75 - 0.76 Converted DB to 0.76 (jump & jumpRj falls as double)
		//0.74 - 0.75 Converted DB to 0.75 (person, and personSessionWeight have height and weight as double)
		//0.73 - 0.74 Converted DB to 0.74 (All DJ converted to DJna)
		//0.72 - 0.73 Converted DB to 0.73 (deleted orphaned persons (in person table but not in personSessionWeight table))
		//0.71 - 0.72 dates to YYYY-MM-DD
		//0.70 - 0.71 created personNotUploadTable on client
		//0.69 - 0.70 added showPower to preferences
		//0.68 - 0.69 added Gesell-DBT test
		//0.67 - 0.68 added multiChronopic tests table
		//0.66 - 0.67 added TakeOff jumps 
		//0.65 - 0.66 added done nothing 
		//0.64 - 0.65 added Sevaluator on client
		//0.63 - 0.64 added margaria test
		//0.62 - 0.63 added 'versionAvailable' to preferences
		//0.61 - 0.62 added hexagon (jumpRj test)
		//0.60 - 0.61 added RunIntervalType distancesString (now we van have interval tests with different distances of tracks). Added MTGUG
		//0.59 - 0.60 added volumeOn and evaluatorServerID to preferences. Session has now serverUniqueID. Simulated now are -1, because 0 is real and positive is serverUniqueID
		//0.58 - 0.59 Added 'showAngle' to preferences, changed angle on jump to double
		//0.57 - 0.58 Countries without kingdom or republic (except when needed)
		//0.56 - 0.57 Added simulated column to each event table on client. person: race, country, serverID. Convert to sport related done here if needed. Added also run and runInterval initial speed);
		//0.55 - 0.56 Added session default sport stuff into session table
		//0.54 - 0.55 Added undefined to speciallity table
		//0.53 - 0.54 created sport tables. Added sport data, speciallity and level of practice to person table
		//0.52 - 0.53 added table weightSession, moved person weight data to weightSession table for each session that has performed
		//0.51 - 0.52 added graphLinks for cmj_l and abk_l. Fixed CMJ_l name
		//0.50 - 0.51 added graphLinks for run simple and interval
		//0.49 - 0.50: changed SJ+ to SJl, same for CMJ+ and ABK+, added jump and jumpRj graph links
		//0.48 - 0.49: added graphLinkTable, added rocket jump and 5 agility tests: (20Yard, 505, Illinois, Shuttle-Run & ZigZag). Added graphs pof the 5 agility tests
		//0.47 - 0.48: added tempJumpReactive and tempRunInterval tables
		//0.46 - 0.47: added reactionTime table
		//0.45 - 0.46: added "Free" jump type
		//0.44 - 0.45: added allowFinishRjAfterTime
		//0.43 - 0.44: added showQIndex and showDjIndex
		//0.42 - 0.43: added 'free' pulseType & language preference
		//0.41 - 0.42: added pulse and pulseType tables
		//0.4 - 0.41: jump, jumpRj weight is double (always a percent)
		

		Sqlite.Close();
		creationRate ++;
	}

	protected static bool tableExists (bool dbconOpened, string tableName)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "SELECT name FROM sqlite_master WHERE type=\"table\" AND name=\"" + tableName + "\"";
		LogB.SQL(dbcmd.CommandText.ToString());

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		bool exists = false;
		if (reader.Read())
			exists = true;
		//LogB.SQL(string.Format("name exists = {0}", exists.ToString()));

		reader.Close();

		closeIfNeeded(dbconOpened);

		return exists;
	}

	protected static bool columnExists (bool dbconOpened, string tableName, string columnName, bool caseSensitive)
	{
		openIfNeeded(dbconOpened);

		if(caseSensitive)
			executeSQL("PRAGMA case_sensitive_like=ON;");

		dbcmd.CommandText = "SELECT * FROM sqlite_master WHERE type = \"table\" AND name = \"" +
			tableName + "\" AND sql LIKE \"%" + columnName + "%\"";

		LogB.SQL(dbcmd.CommandText.ToString());

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		bool exists = false;
		if (reader.Read())
			exists = true;
		//LogB.SQL(string.Format("name exists = {0}", exists.ToString()));

		reader.Close();

		if(caseSensitive)
			executeSQL("PRAGMA case_sensitive_like=OFF;");

		closeIfNeeded(dbconOpened);

		return exists;
	}

	//on Windows with cerbero compilation, sqlite implementation is very old (previous to 3.25.0)
	//so there is no RENAME COLUMN and we need to do it in old way
	private static void renameColumnLinuxOrMac (string table, string cOld, string cNew)
	{
		executeSQL("ALTER TABLE " + table + " RENAME COLUMN \"" + cOld + "\" TO \"" + cNew + "\";");
	}

	public static bool Exists(bool dbconOpened, string tableName, string findName)
	{
		return (ExistsDo(dbconOpened, tableName, findName) != -1);
	}
	public static int ExistsAndGetUniqueID(bool dbconOpened, string tableName, string findName)
	{
		return ExistsDo(dbconOpened, tableName, findName);
	}
	public static int ExistsDo(bool dbconOpened, string tableName, string findName)
	{
		if(!dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "SELECT uniqueID FROM " + tableName + 
			" WHERE LOWER(name) == LOWER(\"" + findName + "\")" ;
		LogB.SQL(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
	
		int id = -1;
		
		if (reader.Read()) {
			string s = reader[0].ToString();
			if(Util.IsNumber(s, false))
				id = Convert.ToInt32(s);
		}
		LogB.SQL(string.Format("SQL.ExistsDo... table: {0}; name: {1}; id: {2}.", tableName, findName, id.ToString()));

		reader.Close();
		if(!dbconOpened)
			Sqlite.Close();

		return id;
	}

	private static void fixDuplicatedExercises (string table)
	{
		// 1) find duplicates
		dbcmd.CommandText = "SELECT name, COUNT(*) AS count FROM " + table +
				" GROUP BY name HAVING count > 1";
		LogB.SQL(dbcmd.CommandText.ToString());

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		List<string> namesConflicting_l = new List<string>();
		while(reader.Read())
			namesConflicting_l.Add(reader[0].ToString());

		reader.Close();
		if(namesConflicting_l.Count == 0)
			return;

		List<string> namesWithSimilarStart_l = new List<string>();
		foreach(string nameConflict in namesConflicting_l)
		{
			// LogB.Information(string.Format("\n\nnameConflict: |{0}|\n", nameConflict));
			// 2.a) for each duplicate, find distinct names that start similar to it
			//      to not use them on rename namesConflicting as "name (1)","name (2)" ...
			dbcmd.CommandText = "SELECT DISTINCT name FROM " + table +
			" WHERE name LIKE \"" + nameConflict + "%\"";
			LogB.SQL(dbcmd.CommandText.ToString());

			reader = dbcmd.ExecuteReader();
			while(reader.Read())
				namesWithSimilarStart_l.Add(reader[0].ToString());

			reader.Close();

			// 2.b) select the ids of each of the nameConflict
			dbcmd.CommandText = "SELECT uniqueID FROM " + table +
				" WHERE name = \"" + nameConflict + "\"";
			LogB.SQL(dbcmd.CommandText.ToString());

			reader = dbcmd.ExecuteReader();
			List<int> idsOfThisName_l = new List<int>();
			while(reader.Read())
				idsOfThisName_l.Add(Convert.ToInt32(reader[0].ToString()));

			reader.Close();

			// 2.c) find if there is this name with " (number)". If exists, get the number
			//      adapted from https://stackoverflow.com/a/22373595
			string nameWithoutNumber = nameConflict;
			int number = 1;
			Match regex = Regex.Match(nameWithoutNumber, @"^(.+) \((\d+)\)$");
			if (regex.Success)
			{
				nameWithoutNumber = regex.Groups[1].Value;
				number = int.Parse(regex.Groups[2].Value);
			}

			// 2.d) foreach of the ids of that nameConflict, find a new name that is available.
			//      start in the second value (will b the first duplicated)
			for(int i = 1; i < idsOfThisName_l.Count; i ++)
			{
				// 2.d.1) find a number that it does not exists on similar names
				bool foundSimilar = false;
				string nameFixed = nameWithoutNumber;
				do {
					nameFixed = string.Format("{0} ({1})", nameWithoutNumber, number);
					foundSimilar = false;
					foreach(string nameWithSimilarStart in namesWithSimilarStart_l)
						if(nameFixed.ToLower() == nameWithSimilarStart.ToLower())
							foundSimilar = true;

					number ++;
				} while (foundSimilar);

				// 2.d.2) add new name to list
				namesWithSimilarStart_l.Add(nameFixed);

				LogB.Information(string.Format("nameConflict: |{0}| will change to: |{1}|\n\n",
							nameConflict, nameFixed));

				executeSQL("UPDATE " + table + " SET name = \"" + nameFixed +
						"\" WHERE uniqueID = " + idsOfThisName_l[i]);
			}
		}
	}

	//OLD: used on queries to old server
	public static string SQLBuildQueryString (string tableName, string test, string variable,
			int sex, string ageInterval,
			int countryID, int sportID, int speciallityID, int levelID, int evaluatorID)
	{
		string tp = Constants.PersonTable;
		string tps = Constants.PersonSessionTable;

		string strSelect = "SELECT COUNT(" + variable + "), AVG(" + variable + ")";
		string strFrom   = " FROM " + tableName;
		string strWhere  = " WHERE " + tableName + ".type = \"" + test + "\"";

		string strSex = "";
		if(sex == Constants.SexUID)
			strSex = " AND " + tp + ".sex == \"" + Constants.SexM + "\"";
		else if(sex == Constants.SexMID)
			strSex = " AND " + tp + ".sex == \"" + Constants.SexM + "\"";
		else if (sex == Constants.SexFID)
			strSex = " AND " + tp + ".sex == \"" + Constants.SexF + "\"";

		string strAge = "";
		string strEval = "";
		string strSession = "";
		if(ageInterval != "" || evaluatorID != Constants.AnyID) {
			strFrom += ", session";
			if(ageInterval != "") {
				string [] strFull = ageInterval.Split(new char[] {':'});
				strAge = " AND (julianday(session.date) - julianday(" + tp + ".dateBorn))/365.25 " + 
					strFull[0] + " " + strFull[1];
				if(strFull.Length == 4)
					strAge += " AND (julianday(session.date) - julianday(" + tp + ".dateBorn))/365.25 " + 
						strFull[2] + " " + strFull[3];
			}
			if(evaluatorID != Constants.AnyID) 
				strEval = " AND session.evaluatorID == " + evaluatorID;
			
			strSession = " AND " + tableName + ".sessionID = session.uniqueID";
		}

		string strCountry = "";
		if(countryID != Constants.CountryUndefinedID) 
			strCountry = " AND " + tp + ".countryID == " + countryID;

		string strSport = "";
		if(sportID != Constants.SportUndefinedID) 
			strSport = " AND " + tps + ".sportID == " + sportID;

		string strSpeciallity = "";
		if(speciallityID != Constants.SpeciallityUndefinedID) 
			strSpeciallity = " AND " + tps + ".speciallityID == " + speciallityID;
		
		string strLevel = "";
		if(levelID != Constants.LevelUndefinedID) 
			strLevel = " AND " + tps + ".practice == " + levelID;

		string strLast = "";
		if(strSex.Length > 0 || strAge.Length > 0 || 
				strCountry.Length > 0 || strSport.Length > 0 || strSpeciallity.Length > 0 || strLevel.Length > 0) {
			strFrom += ", " + tp + ", " + tps;
			strLast = " AND " + tableName + ".personID == " + tp + ".uniqueID" +
				" AND " + tp + ".uniqueID == " + tps + ".personID";
		}	
		return strSelect + strFrom + strWhere + strSex + strAge
			+ strCountry + strSport + strSpeciallity + strLevel + strEval + strSession + strLast;
	}


	/* 
	 * temp data stuff
	 */
	public static int TempDataExists(string tableName)
	{
		//tableName can be tempJumpRj or tempRunInterval
		
		Sqlite.Open();
		dbcmd.CommandText = "SELECT MAX(uniqueID) FROM " + tableName;
		LogB.SQL(dbcmd.CommandText.ToString());
		
		//SqliteDataReader reader;
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
	
		int exists = 0;
		
		if (reader.Read()) {
			//sqlite3 returns a line (without data) if there's no data. Converting to int the line makes chronojump crash
			try {
				exists = Convert.ToInt32(reader[0]);
			} catch { exists = 0; }
		}
		LogB.SQL(string.Format("exists = {0}", exists.ToString()));
		reader.Close();
		Sqlite.Close();

		return exists;
	}

	public static void DeleteTempEvents(string tableName)
	{
		//tableName can be tempJumpRj or tempRunInterval

		Sqlite.Open();
		//dbcmd.CommandText = "Delete FROM tempJumpRj";
		dbcmd.CommandText = "DELETE FROM " + tableName;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		Sqlite.Close();
	}

	protected static void dropTable(string tableName) {
		dbcmd.CommandText = "DROP TABLE " + tableName;
		dbcmd.ExecuteNonQuery();
	}
				
	protected static void convertPersonAndPersonSessionTo77() {
		//create person77
		SqlitePerson sqlitePersonObject = new SqlitePerson();
		sqlitePersonObject.createTable(Constants.PersonTable);
		
		//create personSession77
		SqlitePersonSession sqlitePersonSessionObject = new SqlitePersonSession();
		sqlitePersonSessionObject.createTable(Constants.PersonSessionTable);

		//select all personOld data
		SqlitePersonOld sqlitePersonOldObject = new SqlitePersonOld();
		ArrayList personsOld = sqlitePersonOldObject.SelectAllPersons();

		conversionRateTotal = personsOld.Count;
		conversionRate = 1;
		foreach (PersonOld pOld in personsOld) {
			Person p = new Person(
				       pOld.UniqueID,
				       pOld.Name,
				       pOld.Sex,
				       pOld.DateBorn,
				       pOld.Race,
				       pOld.CountryID,
				       pOld.Description,
				       "", "", 	//future1: rfid; future2: clubID
				       pOld.ServerUniqueID,
				       "" //linkServerImage
				       );
			p.InsertAtDB(true, Constants.PersonTable);
		
			//select all personSessionOld data of this person
			SqlitePersonSessionOld sqlitePersonSessionOldObject = new SqlitePersonSessionOld();
			ArrayList personSessionsOld = sqlitePersonSessionOldObject.SelectAllPersonSessionsOfAPerson(p.UniqueID);
			conversionSubRateTotal = personSessionsOld.Count;
			conversionSubRate = 1;
			foreach (PersonSessionOld psOld in personSessionsOld) {
				PersonSession ps = new PersonSession(
						psOld.UniqueID,
						psOld.PersonID,
						psOld.SessionID,
						pOld.Height,
						psOld.Weight,
						pOld.SportID,
						pOld.SpeciallityID,
						pOld.Practice,
						"", 		//comments
						Constants.TrochanterToeUndefinedID,
						Constants.TrochanterFloorOnFlexionUndefinedID
						);
				ps.InsertAtDB(true, Constants.PersonSessionTable);
				conversionSubRate ++;
			}
			conversionRate ++;
		}
		
		//drop old tables
		Sqlite.dropTable(Constants.PersonOldTable);
		Sqlite.dropTable(Constants.PersonSessionOldWeightTable);

	}



	//to convert to sqlite 0.72
	protected internal static void datesToYYYYMMDD()
	{
		conversionRateTotal = 4;
		conversionRate = 1;
		conversionSubRateTotal = -1;

		/* person table */

		dbcmd.CommandText = "SELECT uniqueID, dateBorn FROM person";
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		ArrayList myArray = new ArrayList(1);
		while(reader.Read()) 
			myArray.Add (reader[0].ToString() + ":" + reader[1].ToString());

		reader.Close();

		/*
		   convert dates from D/M/Y
		   to YYYY-MM-DD
		   */
		conversionSubRateTotal = myArray.Count;
		conversionSubRate = 1;
		foreach(string str in myArray) {
			string [] id_date = str.Split(new char[] {':'});
			DateTime dt = UtilDate.FromSql(id_date[1]);
			dbcmd.CommandText = "UPDATE person set dateBorn = \"" + UtilDate.ToSql(dt) +
				"\" WHERE uniqueID = " + id_date[0];
			LogB.SQL(dbcmd.CommandText.ToString());
			dbcmd.ExecuteNonQuery();
			conversionSubRate ++;
		}

		conversionRate ++;

		/* session table */

		dbcmd.CommandText = "SELECT uniqueID, date FROM session";
		reader = dbcmd.ExecuteReader();
		myArray = new ArrayList(1);
		while(reader.Read()) 
			myArray.Add (reader[0].ToString() + ":" + reader[1].ToString());

		reader.Close();

		/*
		   convert dates from D/M/Y
		   to YYYY-MM-DD
		   */
		conversionSubRateTotal = myArray.Count;
		conversionSubRate = 1;
		foreach(string str in myArray) {
			string [] id_date = str.Split(new char[] {':'});
			DateTime dt = UtilDate.FromSql(id_date[1]);
			dbcmd.CommandText = "UPDATE session set date = \"" + UtilDate.ToSql(dt) +
				"\" WHERE uniqueID = " + id_date[0];
			LogB.SQL(dbcmd.CommandText.ToString());
			dbcmd.ExecuteNonQuery();
			conversionSubRate ++;
		}

		conversionRate ++;
		
		/* SEvaluator table */

		dbcmd.CommandText = "SELECT uniqueID, dateBorn FROM SEvaluator";
		reader = dbcmd.ExecuteReader();
		myArray = new ArrayList(1);
		while(reader.Read()) 
			myArray.Add (reader[0].ToString() + ":" + reader[1].ToString());

		reader.Close();

		/*
		   convert dates from D/M/Y
		   to YYYY-MM-DD
		   */
		conversionSubRateTotal = myArray.Count;
		conversionSubRate = 1;
		foreach(string str in myArray) {
			string [] id_date = str.Split(new char[] {':'});
			DateTime dt = UtilDate.FromSql(id_date[1]);
			dbcmd.CommandText = "UPDATE SEvaluator set dateBorn = \"" + UtilDate.ToSql(dt) +
				"\" WHERE uniqueID = " + id_date[0];
			LogB.SQL(dbcmd.CommandText.ToString());
			dbcmd.ExecuteNonQuery();
			conversionSubRate ++;
		}
		conversionRate ++;
	}

	//used to delete persons (if needed) when a session is deleted. See SqliteSession.DeleteAllStuff
	protected internal static void deleteOrphanedPersons()
	{
		dbcmd.CommandText = "SELECT uniqueID FROM " + Constants.PersonTable;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		ArrayList myArray = new ArrayList(1);

		while(reader.Read())
			myArray.Add (Convert.ToInt32(reader[0]));
		reader.Close();

		foreach (int personID in myArray) {
			//if person is not in other sessions, delete it from DB
			if (! SqlitePersonSession.PersonExistsInAnyPS (true, personID))
				SqlitePerson.DeletePersonAndImages (true, personID);
		}
	}
				
	//used to delete persons (if needed) when a session is deleted. See SqliteSession.DeleteAllStuff
	//also used to convert to sqlite 0.73
	//this is old method (before .77), now use above method
	protected internal static void deleteOrphanedPersonsOld()
	{
		dbcmd.CommandText = "SELECT uniqueID FROM " + Constants.PersonOldTable;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		ArrayList myArray = new ArrayList(1);

		while(reader.Read())
			myArray.Add (Convert.ToInt32(reader[0]));
		reader.Close();

		foreach(int personID in myArray) {
			//if person is not in other sessions, delete it from DB
			if(! SqlitePersonSessionOld.PersonExistsInPS(personID))
				SqlitePersonOld.Delete(personID);
		}
	}
				
	//used to convert to sqlite 0.74
	protected internal static void convertDJInDJna()
	{
		//Dja exists in DB? (user defined)
		if(Exists(false, Constants.JumpTypeTable, "DJa")) {
			string [] names = { "DJa-user", "DJa-user2", "DJa-user3", "DJa-user4" }; //sorry, we cannot check all the names in the world, ok, yes, i know, we can, but it's ok like this
			bool success = false;
			foreach(string name in names) {
				if(!Exists(false, Constants.JumpTypeTable, name)) {
					success = true;
					dbcmd.CommandText = "UPDATE jump SET type = \"" + name + "\" WHERE type == \"DJa\"";
					LogB.SQL(dbcmd.CommandText.ToString());
					dbcmd.ExecuteNonQuery();
				}
				if(success) 
					break;
			}
		}
		
		//Djna exists in DB? (user defined)
		if(Exists(false, Constants.JumpTypeTable, "DJna")) {
			string [] names = { "DJna-user", "DJna-user2", "DJna-user3", "DJna-user4" }; //sorry, we cannot check all the names in the world, ok, yes, i know, we can, but it's ok like this
			bool success = false;
			foreach(string name in names) {
				if(!Exists(false, Constants.JumpTypeTable, name)) {
					success = true;
					dbcmd.CommandText = "UPDATE jump SET type = \"" + name + "\" WHERE type == \"DJna\"";
					LogB.SQL(dbcmd.CommandText.ToString());
					dbcmd.ExecuteNonQuery();
				}
				if(success) 
					break;
			}
		}

		//no opened before because Exists is for closed dbcon
		Sqlite.Open();

		//create new jump types
		SqliteJumpType.JumpTypeInsert ("DJa:0:0:DJ jump using arms", true); 
		SqliteJumpType.JumpTypeInsert ("DJna:0:0:DJ jump without using arms", true); 
		
		//add auto-converted on description
		dbcmd.CommandText = "UPDATE jump SET description = description || \" Auto-converted from DJ\" WHERE type == \"DJ\"";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		//conversion
		dbcmd.CommandText = "UPDATE jump SET type = \"DJna\" WHERE type == \"DJ\"";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		//delete DJ
		SqliteJumpType.Delete(Constants.JumpTypeTable, "DJ", true);
	}

	private static void updateTo2_41 ()
	{
		updateTo2_41_do (Constants.JumpTable);
		updateTo2_41_do (Constants.JumpRjTable);
		updateTo2_41_do (Constants.TempJumpRjTable);
		updateTo2_41_do (Constants.RunTable);
		updateTo2_41_do (Constants.RunIntervalTable);
		updateTo2_41_do (Constants.TempRunIntervalTable);
	}
	private static void updateTo2_41_do (string table)
	{
		dbcmd.CommandText = string.Format ("SELECT {0}.uniqueID, session.date FROM {0}, session WHERE (datetime IS NULL OR datetime = '') AND {0}.sessionID = session.uniqueID", table);
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		List<SqliteStruct.IntegerText> list = new List<SqliteStruct.IntegerText> ();
		while (reader.Read ())
			list.Add (new SqliteStruct.IntegerText (
						Convert.ToInt32 (reader[0].ToString ()),
						reader[1].ToString () ));

		reader.Close ();

		//update as transaction
		using (SqliteTransaction tr = dbcon.BeginTransaction())
		{
			using (SqliteCommand dbcmdTr = dbcon.CreateCommand())
			{
				dbcmdTr.Transaction = tr;

				foreach (SqliteStruct.IntegerText it in list)
				{
					dbcmdTr.CommandText = string.Format ("UPDATE {0} SET datetime = '{1}' WHERE uniqueID = {2}", table, it.text + "_00-00-01", it.integer);
					LogB.SQL (dbcmdTr.CommandText.ToString());
					dbcmdTr.ExecuteNonQuery();
				}
			}
			tr.Commit();
		}
	}


	/*
	 * The problem of this method is that uses class constructors: person, jump, ...
	 * and if the sqlite version is updated from a really old version
	 * maybe the object has to be converted from really older class to old, and then to new class (two conversions)
	 * and this can have problems in the class construction
	 * The best seem to have a boolean that indicates if certain conversion has done before
	 * (see bool runAndRunIntervalInitialSpeedAdded)
	 */
	protected internal static void convertTables(Sqlite sqliteObject, string tableName, int columnsBefore, ArrayList columnsToAdd, bool putDescriptionInMiddle) 
	{
		conversionSubRate = 1;
		conversionSubRateTotal = -1; //unknown yet


		//2st create convert temp table
		sqliteObject.createTable(Constants.ConvertTempTable);

		//2nd copy all data from desired table to temp table (in event tables, adding the simulated column)
		ArrayList myArray = new ArrayList(2);
		dbcmd.CommandText = "SELECT * " + 
			"FROM " + tableName + " ORDER BY uniqueID"; 
		LogB.SQL(dbcmd.CommandText.ToString());
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		while(reader.Read()) {
			string [] myReaderStr = new String[columnsBefore + columnsToAdd.Count];
			int i;
			for (i=0; i < columnsBefore; i ++) 
				myReaderStr[i] = reader[i].ToString();
		
			foreach (string myStr in columnsToAdd) 
				myReaderStr[i++] = myStr;

			if (putDescriptionInMiddle) {
				//string [] strFull = changePos.Split(new char[] {':'});
				//int row1 = Convert.ToInt32(strFull[0]);
				//int row2 = Convert.ToInt32(strFull[1]);
				string desc = myReaderStr[6];
				myReaderStr[6] = myReaderStr[7];
				myReaderStr[7] = myReaderStr[8];
				myReaderStr[8] = myReaderStr[9];
				myReaderStr[9] = desc;
			}

			if(tableName == Constants.PersonOldTable) {	
				PersonOld myPerson =  new PersonOld(myReaderStr);
				myArray.Add(myPerson);
			} else if(tableName == Constants.SessionTable) {	
				Session mySession = new Session(myReaderStr);
				myArray.Add(mySession);
			} else if(tableName == Constants.RunIntervalTypeTable) {	
				RunType myType = new RunType(myReaderStr, true); //interval
				myArray.Add(myType);
			} else if(tableName == Constants.PersonSessionOldWeightTable) {	
				PersonSessionOld myPS = new PersonSessionOld(myReaderStr);
				myArray.Add(myPS);
			} else {
				Event myEvent =  new Event();	
				switch (tableName) {
					case Constants.JumpTable:
						myEvent = new Jump(myReaderStr);
						break;
					case Constants.JumpRjTable:
						myEvent = new JumpRj(myReaderStr);
						break;
					case Constants.RunTable:
						myEvent = new Run(myReaderStr);
						break;
					case Constants.RunIntervalTable:
						myEvent = new RunInterval(myReaderStr);
						break;
					case Constants.ReactionTimeTable:
						myEvent = new ReactionTime(myReaderStr);
						break;
					case Constants.PulseTable:
						myEvent = new Pulse(myReaderStr);
						break;
				}
				myArray.Add(myEvent);
			}
		}
		reader.Close();

LogB.SQL("1" + tableName);

		conversionSubRateTotal = myArray.Count * 2;

		if(tableName == Constants.PersonOldTable) {	
			foreach (PersonOld myPerson in myArray) {
				myPerson.InsertAtDB(true, Constants.ConvertTempTable);
				conversionSubRate ++;
			}
		} else if(tableName == Constants.SessionTable) {	
			foreach (Session mySession in myArray) {
				mySession.InsertAtDB(true, Constants.ConvertTempTable);
				conversionSubRate ++;
			}
		} else if(tableName == Constants.RunIntervalTypeTable) {	
			foreach (RunType type in myArray) {
				type.InsertAtDB(true, Constants.ConvertTempTable, true); //last true is for interval
				conversionSubRate ++;
			}
		} else if(tableName == Constants.PersonSessionOldWeightTable) {	
			foreach (PersonSessionOld ps in myArray) {
				ps.InsertAtDB(true, Constants.ConvertTempTable);
				conversionSubRate ++;
			}
		} else {
			foreach (Event myEvent in myArray) {
				myEvent.InsertAtDB(true, Constants.ConvertTempTable);
				conversionSubRate ++;
			}
		}
		
LogB.SQL("2" + tableName);
		//3rd drop desired table
		Sqlite.dropTable(tableName);

LogB.SQL("3" + tableName);
		//4d create desired table (now with new columns)
		sqliteObject.createTable(tableName);


LogB.SQL("4" + tableName);

		//5th insert data in desired table
		if(tableName == Constants.PersonOldTable) {	
			foreach (PersonOld myPerson in myArray) {
				myPerson.InsertAtDB(true, tableName);
				conversionSubRate ++;
			}
		} else if(tableName == Constants.SessionTable) {	
			foreach (Session mySession in myArray) {
				mySession.InsertAtDB(true, tableName);
				conversionSubRate ++;
			}
		} else if(tableName == Constants.RunIntervalTypeTable) {	
			foreach (RunType type in myArray) {
				type.InsertAtDB(true, tableName, true); //last true is for interval
				conversionSubRate ++;
			}
		} else if(tableName == Constants.PersonSessionOldWeightTable) {	
			foreach (PersonSessionOld ps in myArray) {
				ps.InsertAtDB(true, tableName);
				conversionSubRate ++;
			}
		} else {
			foreach (Event myEvent in myArray) {
				myEvent.InsertAtDB(true, tableName);
				conversionSubRate ++;
			}
		}

LogB.SQL("5" + tableName);
		//6th drop temp table
		Sqlite.dropTable(Constants.ConvertTempTable);
	}
	
	/*
	 * useful to do a conversion from an int to a double
	 * used on jump.angle
	 * we done on sqlite/jump.cs:
	 * on createTable change "angle INT" to "angle FLOAT"
	 * then call this alterTableColumn
	 *
	 * but CAUTION: doing this, all the float data is converted to .0
	 * eg: 27.35 will be 27.0
	 *     -1 will be -1.0
	 *
	 * if we don't use this, and we have created a column as int, and introduce floats or doubles, 
	 * we can insert ok the float or doubles, but on select we will have ints
	 */
	protected internal static void alterTableColumn(Sqlite sqliteObject, string tableName, int columns) 
	{
		conversionSubRate = 1;
		conversionSubRateTotal = -1; //unknown yet

		//2st create convert temp table
		sqliteObject.createTable(Constants.ConvertTempTable);

		//2nd copy all data from desired table to temp table adding the simulated column
		ArrayList myArray = new ArrayList(2);
		dbcmd.CommandText = "SELECT * " + 
			"FROM " + tableName + " ORDER BY uniqueID"; 
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		LogB.SQL(dbcmd.CommandText.ToString());

		while(reader.Read()) {
			string [] myReaderStr = new String[columns];
			for (int i=0; i < columns; i ++) 
				myReaderStr[i] = reader[i].ToString();
		
			Event myEvent =  new Event();	
			switch (tableName) {
				case Constants.JumpTable:
					myEvent = new Jump(myReaderStr);
					break;
				case Constants.JumpRjTable:
					myEvent = new JumpRj(myReaderStr);
					break;
				case Constants.RunTable:
					myEvent = new Run(myReaderStr);
					break;
				case Constants.RunIntervalTable:
					myEvent = new RunInterval(myReaderStr);
					break;
				case Constants.ReactionTimeTable:
					myEvent = new ReactionTime(myReaderStr);
					break;
				case Constants.PulseTable:
					myEvent = new Pulse(myReaderStr);
					break;
			}
			myArray.Add(myEvent);
		}
		reader.Close();

		conversionSubRateTotal = myArray.Count * 2;

		foreach (Event myEvent in myArray) {
			myEvent.InsertAtDB(true, Constants.ConvertTempTable);
			conversionSubRate ++;
		}

		//3rd drop desired table
		Sqlite.dropTable(tableName);

		//4d create desired table (now with new columns)
		sqliteObject.createTable(tableName);

		//5th insert data in desired table
		foreach (Event myEvent in myArray) {
			myEvent.InsertAtDB(true, tableName);
			conversionSubRate ++;
		}

		//6th drop temp table
		Sqlite.dropTable(Constants.ConvertTempTable);
	}

	protected static string [] DataReaderToStringArray (SqliteDataReader reader, int columns) {
		string [] myReaderStr = new String[columns];
		for (int i=0; i < columns; i ++)
			myReaderStr[i] = reader[i].ToString();
		return myReaderStr;
	}

	protected static IDNameList fillIDNameList(string selectStr) 
	{
		//select personID and jump type 'SJ' mean
		dbcmd.CommandText = selectStr;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader = dbcmd.ExecuteReader();

		IDNameList list = new IDNameList();
		while(reader.Read()) {
			IDName idname = new IDName( Convert.ToInt32(reader[0].ToString()),
						reader[1].ToString());
			LogB.SQL(idname.ToString());
			
			list.Add(new IDName( Convert.ToInt32(reader[0].ToString()),
						reader[1].ToString() ));
		}
		reader.Close();
		return list;
	}

	protected static IDDoubleList fillIDDoubleList(string selectStr) 
	{
		//select personID and jump type 'SJ' mean
		dbcmd.CommandText = selectStr;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader = dbcmd.ExecuteReader();

		IDDoubleList list = new IDDoubleList();
		while(reader.Read()) {
			list.Add(new IDDouble( Convert.ToInt32(reader[0].ToString()),
						Convert.ToDouble(Util.ChangeDecimalSeparator(reader[1].ToString())) ));
		}
		reader.Close();
		return list;
	}


	/* methods for different classes */
	
	//when select from database, ensure path separators are ok for this platform
	//useful if person moved database between diff OS
	protected static string fixOSpath(string url) {
		if(UtilAll.IsWindows())
			return url.Replace("/","\\");
		else
			return url.Replace("\\","/");
	}


	protected static double selectDouble (string sqlSelect) 
	{
		dbcmd.CommandText = sqlSelect;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		
		double result = 0;
		if(reader.Read())
			result = Convert.ToDouble(Util.ChangeDecimalSeparator(reader[0].ToString()));

		reader.Close();
		
		return result;
	}


	public static int Max (string tableName, string column, bool dbconOpened)
	{
		if(!dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "SELECT MAX(" + column + ") FROM " + tableName ;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		
		int myReturn = 0;
		//caution because it there are no values, a whiteline or NULL can be returned and can fail in this Convert.ToInt32
		if(reader.Read()) 
			myReturn = Convert.ToInt32(reader[0].ToString());
		reader.Close();

		if(!dbconOpened)
			Sqlite.Close();
		return myReturn;
	}

	public static int Count (string tableName, bool dbconOpened)
	{
		if(!dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "SELECT COUNT(*) FROM " + tableName ;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		
		int myReturn = 0;
		if(reader.Read()) 
			myReturn = Convert.ToInt32(reader[0].ToString());
		reader.Close();

		if(!dbconOpened)
			Sqlite.Close();
		return myReturn;
	}

	public static int CountCondition (string tableName, bool dbconOpened, string condition, string operand, string myValue) {
		if(!dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "SELECT COUNT(*) FROM " + tableName +
			" WHERE " + condition + " " + operand + " " + myValue;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		
		int myReturn = 0;
		if(reader.Read()) 
			myReturn = Convert.ToInt32(reader[0].ToString());
		reader.Close();

		if(!dbconOpened)
			Sqlite.Close();
		return myReturn;
	}

	//if we want to use the condition2 but not the searchValue, leave this as ""
	//note that eg. newValue could be an int or others
	//not sure if this works for non-literals
	public static void Update(
			bool dbconOpened, string tableName, string columnName, 
			string searchValue, string newValue, 
			string columnNameCondition2, string searchValueCondition2)
	{
		if( ! dbconOpened)
			Sqlite.Open();
		
		bool whereDone = false;
		string cond1 = "";
		if(searchValue != "") {
			cond1 = " WHERE " + columnName + " == \"" + searchValue + "\"";
			whereDone = true;
		}

		string cond2 = "";
		if(columnNameCondition2 != "" && searchValueCondition2 != "") 
		{
			string cond2Pre = "";
			if(whereDone)
				cond2Pre = " AND ";
			else
				cond2Pre = " WHERE ";

			cond2 = cond2Pre + columnNameCondition2 + " == \"" + searchValueCondition2 + "\""; 
		}

		dbcmd.CommandText = "UPDATE " + tableName +
			" SET " + columnName + " = \"" + newValue + "\"" +  
			cond1 +
			cond2
			;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		if( ! dbconOpened)
			Sqlite.Close();
	}

	public static void UpdateTestPersonID (bool dbconOpened, string tableName, int personIDold, int personIDnew)
	{
		openIfNeeded (dbconOpened);

		dbcmd.CommandText = "UPDATE " + tableName +
			" SET personID = " + personIDnew +
			" WHERE personID = " + personIDold;
		LogB.SQL (dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery ();

		closeIfNeeded (dbconOpened);
	}

	public static void Delete(bool dbconOpened, string tableName, int uniqueID)
	{
		if( ! dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "DELETE FROM " + tableName +
			" WHERE uniqueID == " + uniqueID.ToString();
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		if( ! dbconOpened)
			Sqlite.Close();
	}
	
	public static void DeleteSelectingField(bool dbconOpened, string tableName, string fieldName, string id)
	{
		if( ! dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "DELETE FROM " + tableName +
			" WHERE " + fieldName + " == " + id;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		if( ! dbconOpened)
			Sqlite.Close();
	}


	public static void DeleteFromName(bool dbconOpened, string tableName, string searchName)
	{
		DeleteFromName(dbconOpened, tableName, "name", searchName);
	}
	public static void DeleteFromName(bool dbconOpened, string tableName, string colName, string searchName)
	{
		if( ! dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "DELETE FROM " + tableName +
			" WHERE " + colName + " = \"" + searchName + "\"";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		if( ! dbconOpened)
			Sqlite.Close();
	}

	public static void DeleteFromAnInt(bool dbconOpened, string tableName, string colName, int id)
	{
		if( ! dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "DELETE FROM " + tableName +
			" WHERE " + colName + " == " + id;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		if( ! dbconOpened)
			Sqlite.Close();
	}


	/* end of methods for different classes */
	
	/* 
	 * SERVER STUFF
	 */
	
	public static string sqlFileServer = home + Path.DirectorySeparatorChar + "chronojump_server.db";
	static string connectionStringServer = "version = 3; Data source = " + sqlFileServer;
	
	public static bool CheckFileServer(){
		if (File.Exists(sqlFileServer))
			return true;
		else
			return false;
	}
	
	public static void ConnectServer()
	{
		dbcon = new SqliteConnection();
		dbcon.ConnectionString = connectionStringServer;
		dbcmd = dbcon.CreateCommand();
	}
	
	public static bool DisConnect() {
		try {
			Sqlite.Close();
		} catch {
			return false;
		}
		return true;
	}


}

public class SqliteStruct
{
	public struct DateTypeResult
	{
		public string date;
		public string type;
		public double result;

		public DateTypeResult (string date, string type, double result)
		{
			this.date = date;
			this.type = type;
			this.result = result;
		}
	};

	public struct IntTypeDoubleDouble
	{
		public int personID;
		public string type;
		public double avg;
		public double max;

		public IntTypeDoubleDouble (int personID, string type, double avg, double max)
		{
			this.personID = personID;
			this.type = type;
			this.avg = avg;
			this.max = max;
		}

		public static IntTypeDoubleDouble FindRowFromPersonID (List<IntTypeDoubleDouble> l, int personID)
		{
			foreach (IntTypeDoubleDouble idd in l)
				if (idd.personID == personID)
					return idd;

			return new IntTypeDoubleDouble (-1, "", 0, 0);
		}

		//debug
		public override string ToString ()
		{
			return string.Format ("{0}, {1}: {2}, {3}", personID, type, avg, max);
		}
	}

	public struct IntegerText
	{
		public int integer;
		public string text;

		public IntegerText (int integer, string text)
		{
			this.integer = integer;
			this.text = text;
		}
	}
}

