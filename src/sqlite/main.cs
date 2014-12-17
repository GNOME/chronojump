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
 * Copyright (C) 2004-2014   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using System.IO; //"File" things. TextWriter
using System.Collections; //ArrayList
using Mono.Data.Sqlite;
using System.Diagnostics; 	//for launching other process

using Mono.Unix;


class Sqlite
{
	protected static SqliteConnection dbcon;
	protected static SqliteCommand dbcmd;

	//since we use installJammer (chronojump 0.7)	
	//database was on c:\.chronojump\ or in ~/.chronojump
	//now it's on installed dir, eg linux: ~/Chronojump/database
	public static string home = Util.GetDatabaseDir();
	public static string sqlFile = home + Path.DirectorySeparatorChar + "chronojump.db";
	
	public static string temp = Util.GetDatabaseTempDir();
	public static string sqlFileTemp = temp + Path.DirectorySeparatorChar + "chronojump.db";

	//before installJammer
	public static string homeOld = Util.GetOldDatabaseDir();
	public static string sqlFileOld = homeOld + Path.DirectorySeparatorChar + "chronojump.db";
	
	//http://www.mono-project.com/SQLite

	static string connectionString = "version = 3; Data source = " + sqlFile;
	static string connectionStringTemp = "version = 3; Data source = " + sqlFileTemp;

	//test to try to open db in a dir with accents (latin)
	//static string connectionString = "globalization requestEncoding=\"iso-8859-1\"; responseEncoding=\"iso-8859-1\"; fileEncoding=\"iso-8859-1\"; culture=\"es-ES\";version = 3; Data source = " + sqlFile;
	
	//create blank database
	static bool creatingBlankDatabase = false;

	

	//for db creation
	static int creationRate;
	static int creationTotal;

	//for db conversion
	static string currentVersion = "0";
	static int conversionRate;
	static int conversionRateTotal;
	protected static int conversionSubRate;
	protected static int conversionSubRateTotal;

	/*
	 * Important, change this if there's any update to database
	 */
	static string lastChronojumpDatabaseVersion = "1.18";

	public Sqlite() {
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
		try {
			Log.WriteLine("SQL ON");
			dbcon.Open();
		} catch {
			Log.WriteLine("-- catched --");

			Log.WriteLine("SQL OFF");
			dbcon.Close();
			
			Log.WriteLine("SQL ON");
			dbcon.Open();
			
			Log.WriteLine("-- end of catched --");
		}
	}
	public static void Close()
	{
		Log.WriteLine("SQL OFF");
		dbcon.Close();
	}

	public static bool Connect()
	{
		/*
	       splashMessage = "pre";
		needUpdateSplashMessage = true;
		Console.ReadLine();		
		*/

		Log.WriteLine("home is: " + home);

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
		Log.WriteLine(string.Format("press3"));
	       	splashMessage = "post1";
		needUpdateSplashMessage = true;
		Console.ReadLine();		
		*/

		/*
		try{
			Log.WriteLine(string.Format("Trying database in ... " + connectionString));

		//dbcon = new SqliteConnection();
			*/
		/*
			dbcon.ConnectionString = connectionString;
			//dbcon.ConnectionString = connectionStringTemp;
			dbcmd = dbcon.CreateCommand();
		} catch {
			try {
				Log.WriteLine(string.Format("Trying database in ... " + connectionStringTemp));

		//dbcon = new SqliteConnection();
				dbcon.ConnectionString = connectionStringTemp;
				dbcmd = dbcon.CreateCommand();
			} catch { 
				Console.WriteLine("Problems, exiting...\n");
				System.Console.Out.Close();
				Log.End();
				Log.Delete();
				Environment.Exit(1);
			}

		}

		*/
			
		return defaultDBLocation;
		
	}

	//only create blank DB
	public static void ConnectBlank()
	{
		string sqlFileBlank = "chronojump_blank.db"; //copied on /chronojump-x.y/data installjammer will copy it to database
		string connectionStringBlank = "version = 3; Data source = " + sqlFileBlank;

		//delete blank file if exists
		if (File.Exists(sqlFileBlank)) {
			Console.WriteLine("File blank exists, deleting...");
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
		Log.WriteLine(connectionString);

		string applicationDataDir = UtilAll.GetApplicationDataDir();

		if(!Directory.Exists(applicationDataDir)) {
			Log.WriteLine("creating dir 1...");
			Directory.CreateDirectory (applicationDataDir);
		}
		
		if(!Directory.Exists(home)) {
			Log.WriteLine("creating dir 2...");
			Directory.CreateDirectory (home);
		}
		Log.WriteLine("Dirs created.");
	}

	public static void CreateFile()
	{
		Log.WriteLine("creating file...");
		Log.WriteLine(connectionString);
		
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
			if (File.Exists(sqlFile)){
				//backup the database
				Util.BackupDirCreateIfNeeded();
				Util.BackupDatabase();
				Log.WriteLine ("made a database backup"); //not compressed yet, it seems System.IO.Compression.DeflateStream and
				//System.IO.Compression.GZipStream are not in mono

				File.Move(Util.GetDatabaseDir() + Path.DirectorySeparatorChar + "chronojump.db",
					Util.GetDatabaseTempDir() + Path.DirectorySeparatorChar + "chronojump.db");
				return true;
			}
		}
		return false;
	}


	public static bool IsSqlite3() {
		if(sqlite3SelectWorks()){
			Log.WriteLine("SQLITE3");
			Sqlite.Close();
			return true;
		}
		else if(sqlite2SelectWorks()) {
			Log.WriteLine("SQLITE2");
			Sqlite.Close();
			//write sqlFile path on data/databasePath.txt
			//TODO
			//

			return false;
		}
		else {
			Log.WriteLine("ERROR in sqlite detection");
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
				Log.WriteLine("exists1");
			if(File.Exists(sqlite2File)) 
				Log.WriteLine("exists2");

			/*
			Log.WriteLine("{0}-{1}", myPath + Path.DirectorySeparatorChar + sqliteStr , sqlite2File + " .dump");
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
			
			Log.WriteLine("Path written");

			Process p2 = Process.Start(myPath + Path.DirectorySeparatorChar + "convert_database." + extension);
			p2.WaitForExit();

			Log.WriteLine("sqlite3 db created");
				
			File.Copy(myPath + Path.DirectorySeparatorChar + "tmp.db", sqliteDB, true ); //overwrite
		} catch {
			Log.WriteLine("PROBLEMS");
			return false;
		}

		Log.WriteLine("done");
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
		return Util.DivideSafeFraction(creationRate, creationTotal);
	}
	public static double PrintConversionVersion() {
		return Util.DivideSafeFraction(
				Convert.ToDouble(Util.ChangeDecimalSeparator(currentVersion)), 
				Convert.ToDouble(Util.ChangeDecimalSeparator(lastChronojumpDatabaseVersion))
				);
	}
	public static double PrintConversionRate() {
		return Util.DivideSafeFraction(conversionRate, conversionRateTotal);
	}
	public static double PrintConversionSubRate() {
		return Util.DivideSafeFraction(conversionSubRate, conversionSubRateTotal);
	}

	public static bool ConvertToLastChronojumpDBVersion() {
		Log.WriteLine("SelectChronojumpProfile ()");

		//if(checkIfIsSqlite2())
		//	convertSqlite2To3();

		addChronopicPortNameIfNotExists();

		currentVersion = SqlitePreferences.Select("databaseVersion");

		//Log.WriteLine("lastDB: {0}", Convert.ToDouble(lastChronojumpDatabaseVersion));
		//Log.WriteLine("currentVersion: {0}", Convert.ToDouble(currentVersion));

		bool returnSoftwareIsNew = true; //-1 if software is too old for database (moved db to other computer)
		if(
				Convert.ToDouble(Util.ChangeDecimalSeparator(lastChronojumpDatabaseVersion)) == 
				Convert.ToDouble(Util.ChangeDecimalSeparator(currentVersion)))
			Log.WriteLine("Database is already latest version");
		else if(
				Convert.ToDouble(Util.ChangeDecimalSeparator(lastChronojumpDatabaseVersion)) < 
				Convert.ToDouble(Util.ChangeDecimalSeparator(currentVersion))) {
			Log.WriteLine("User database newer than program, need to update software");
			returnSoftwareIsNew = false;
		} else {
			Log.WriteLine("Old database, need to convert");
			Log.WriteLine("db version: " + currentVersion);

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

			if(currentVersion == "0.41") {
				Sqlite.Open();

				//SqlitePulse.createTable(Constants.PulseTable);
				sqlitePulseObject.createTable(Constants.PulseTable);
				SqlitePulseType.createTablePulseType();
				SqlitePulseType.initializeTablePulseType();

				SqlitePreferences.Update ("databaseVersion", "0.42", true); 
				Log.WriteLine("Converted DB to 0.42 (added pulse and pulseType tables)");

				Sqlite.Close();
				currentVersion = "0.42";
			}

			if(currentVersion == "0.42") {
				Sqlite.Open();
				SqlitePulseType.Insert ("Free:-1:-1:free PulseStep mode", true); 
				SqlitePreferences.Insert ("language", "es-ES"); 
				SqlitePreferences.Update ("databaseVersion", "0.43", true); 
				Log.WriteLine("Converted DB to 0.43 (added 'free' pulseType & language peference)");
				Sqlite.Close();
				currentVersion = "0.43";
			}

			if(currentVersion == "0.43") {
				Sqlite.Open();
				SqlitePreferences.Insert ("showQIndex", "False"); 
				SqlitePreferences.Insert ("showDjIndex", "False"); 
				SqlitePreferences.Update ("databaseVersion", "0.44", true); 
				Log.WriteLine("Converted DB to 0.44 (added showQIndex, showDjIndex)");
				Sqlite.Close();
				currentVersion = "0.44";
			}

			if(currentVersion == "0.44") {
				Sqlite.Open();
				SqlitePreferences.Insert ("allowFinishRjAfterTime", "True"); 
				SqlitePreferences.Update ("databaseVersion", "0.45", true); 
				Log.WriteLine("Converted DB to 0.45 (added allowFinishRjAfterTime)");
				Sqlite.Close();
				currentVersion = "0.45";
			}

			if(currentVersion == "0.45") {
				Sqlite.Open();
				SqliteJumpType.JumpTypeInsert ("Free:1:0:Free jump", true); 
				SqlitePreferences.Update ("databaseVersion", "0.46", true); 
				Log.WriteLine("Added Free jump type");
				Sqlite.Close();
				currentVersion = "0.46";
			}

			if(currentVersion == "0.46") {
				Sqlite.Open();

				//SqliteReactionTime.createTable(Constants.ReactionTimeTable);
				sqliteReactionTimeObject.createTable(Constants.ReactionTimeTable);

				SqlitePreferences.Update ("databaseVersion", "0.47", true); 
				Log.WriteLine("Added reaction time table");
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
				Log.WriteLine("created tempJumpReactive and tempRunInterval tables");
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
				Log.WriteLine("Added graphLinkTable, added Rocket jump and 5 agility tests: (20Yard, 505, Illinois, Shuttle-Run & ZigZag. Added graphs pof the 5 agility tests)");

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
				Log.WriteLine("changed SJ+ to SJl, same for CMJ+ and ABK+, added jump and jumpRj graph links");
				Sqlite.Close();
				currentVersion = "0.50";
			}

			if(currentVersion == "0.50") {
				Sqlite.Open();
				SqliteRunType.AddGraphLinksRunSimple();	
				SqliteRunIntervalType.AddGraphLinksRunInterval();	
				SqlitePreferences.Update ("databaseVersion", "0.51", true); 
				Log.WriteLine("added graphLinks for run simple and interval");
				Sqlite.Close();
				currentVersion = "0.51";
			}

			if(currentVersion == "0.51") {
				Sqlite.Open();
				SqliteJumpType.Update ("CJl", "CMJl"); 
				SqliteEvent.GraphLinkInsert (Constants.JumpTable, "CMJl", "jump_cmj_l.png", true);
				SqliteEvent.GraphLinkInsert (Constants.JumpTable, "ABKl", "jump_abk_l.png", true);
				SqlitePreferences.Update ("databaseVersion", "0.52", true); 
				Log.WriteLine("added graphLinks for cmj_l and abk_l, fixed CMJl name");
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
				
				Log.WriteLine("created weightSession table. Moved person weight data to weightSession table for each session that has performed");
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
				
				Log.WriteLine("Created sport tables. Added sport data, speciallity and level of practice to person table");
				currentVersion = "0.54";
			}
			if(currentVersion == "0.54") {
				Sqlite.Open();

				SqliteSpeciallity.InsertUndefined(true);

				SqlitePreferences.Update ("databaseVersion", "0.55", true); 
				Sqlite.Close();
				
				Log.WriteLine("Added undefined to speciallity table");
				currentVersion = "0.55";
			}
			if(currentVersion == "0.55") {
				Sqlite.Open();

				SqliteSessionOld.convertTableAddingSportStuff();

				SqlitePreferences.Update ("databaseVersion", "0.56", true); 
				Sqlite.Close();
				
				Log.WriteLine("Added session default sport stuff into session table");
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
				
				Log.WriteLine("Added simulated column to each event table on client. Added to person: race, country, serverUniqueID. Convert to sport related done here if needed. Added also run and runInterval initial speed");
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
					Log.WriteLine("Countries without kingdom or republic (except when needed)");
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
				Log.WriteLine("Converted DB to 0.59 (added 'showAngle' to preferences, changed angle on jump to double)"); 
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
				Log.WriteLine("Converted DB to 0.60 (added volumeOn and evaluatorServerID to preferences. session has now serverUniqueID. Simulated now are -1, because 0 is real and positive is serverUniqueID)"); 
				
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
				Log.WriteLine("Converted DB to 0.61 added RunIntervalType distancesString (now we van have interval tests with different distances of tracks). Added MTGUG");
				
				conversionRate = 3;
				Sqlite.Close();
				currentVersion = "0.61";
			}
			if(currentVersion == "0.61") {
				Sqlite.Open();
				SqliteJumpType.JumpRjTypeInsert ("RJ(hexagon):1:0:1:18:Reactive Jump on a hexagon until three full revolutions are done", true);
				SqlitePreferences.Update ("databaseVersion", "0.62", true); 
				Log.WriteLine("Converted DB to 0.62 added hexagon");
				Sqlite.Close();
				currentVersion = "0.62";
			}
			if(currentVersion == "0.62") {
				Sqlite.Open();
				SqlitePreferences.Insert ("versionAvailable", "");
				SqlitePreferences.Update ("databaseVersion", "0.63", true); 
				Log.WriteLine("Converted DB to 0.63 (added 'versionAvailable' to preferences)"); 
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
				
				Log.WriteLine("Converted DB to 0.64 (added margaria test)"); 
				Sqlite.Close();
				currentVersion = "0.64";
			}
			if(currentVersion == "0.64") {
				Sqlite.Open();
				
				SqliteServer sqliteServerObject = new SqliteServer();
				//user has also an evaluator table with a row (it's row)	
				sqliteServerObject.CreateEvaluatorTable();

				SqlitePreferences.Update ("databaseVersion", "0.65", true); 
				
				Log.WriteLine("Converted DB to 0.65 (added Sevaluator on client)"); 
				Sqlite.Close();
				currentVersion = "0.65";
			}
			if(currentVersion == "0.65") {
				Sqlite.Open();
				//now runAnalysis is a multiChronopic event
				//SqliteJumpType.JumpRjTypeInsert ("RunAnalysis:0:0:1:-1:Run between two photocells recording contact and flight times in contact platform/s. Until finish button is clicked.", true);

				SqlitePreferences.Update ("databaseVersion", "0.66", true); 
				
				//Log.WriteLine("Converted DB to 0.66 (added RunAnalysis Reactive jump)"); 
				Log.WriteLine("Converted DB to 0.66 (done nothing)"); 
				Sqlite.Close();
				currentVersion = "0.66";
			}
			if(currentVersion == "0.66") {
				Sqlite.Open();
				SqliteJumpType.JumpTypeInsert ("TakeOff:0:0:Take off", true);
				SqliteJumpType.JumpTypeInsert ("TakeOffWeight:0:0:Take off with weight", true);

				SqlitePreferences.Update ("databaseVersion", "0.67", true); 
				
				Log.WriteLine("Converted DB to 0.67 (added TakeOff jumps)"); 
				Sqlite.Close();
				currentVersion = "0.67";
			}
			if(currentVersion == "0.67") {
				Sqlite.Open();
				sqliteMultiChronopicObject.createTable(Constants.MultiChronopicTable);

				SqlitePreferences.Update ("databaseVersion", "0.68", true); 
				
				Log.WriteLine("Converted DB to 0.68 (added multiChronopic tests table)"); 
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
				
				Log.WriteLine("Converted DB to 0.69 (added Gesell-DBT test)"); 
				Sqlite.Close();
				currentVersion = "0.69";
			}
			if(currentVersion == "0.69") {
				Sqlite.Open();
				SqlitePreferences.Insert ("showPower", "True"); 
				SqlitePreferences.Update ("databaseVersion", "0.70", true); 
				Log.WriteLine("Converted DB to 0.70 (added showPower)");
				Sqlite.Close();
				currentVersion = "0.70";
			}
			if(currentVersion == "0.70") {
				Sqlite.Open();
				
				SqlitePersonSessionNotUpload.CreateTable();

				SqlitePreferences.Update ("databaseVersion", "0.71", true); 
				
				Log.WriteLine("Converted DB to 0.71 (created personNotUploadTable on client)"); 
				Sqlite.Close();
				currentVersion = "0.71";
			}
			if(currentVersion == "0.71") {
				Sqlite.Open();
				
				datesToYYYYMMDD();

				SqlitePreferences.Update ("databaseVersion", "0.72", true); 
				
				Log.WriteLine("Converted DB to 0.72 (dates to YYYY-MM-DD)"); 
				Sqlite.Close();
				currentVersion = "0.72";
			}
			if(currentVersion == "0.72") {
				Sqlite.Open();
				
				deleteOrphanedPersonsOld();

				SqlitePreferences.Update ("databaseVersion", "0.73", true); 
				
				Log.WriteLine("Converted DB to 0.73 (deleted orphaned persons (in person table but not in personSessionWeight table)"); 
				Sqlite.Close();
				currentVersion = "0.73";
			}
			if(currentVersion == "0.73") {
				//dbcon open laters on mid convertDJinDJna()
				
				convertDJInDJna();

				SqlitePreferences.Update ("databaseVersion", "0.74", true); 
				
				Log.WriteLine("Converted DB to 0.74 (All DJ converted to DJna)"); 
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
				
				Log.WriteLine("Converted DB to 0.75 (person, and personSessionWeight have height and weight as double)"); 
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
				
				Log.WriteLine("Converted DB to 0.76 (jump & jumpRj falls as double)"); 
				Sqlite.Close();
				currentVersion = "0.76";
			}
			if(currentVersion == "0.76") {
				Sqlite.Open();
				
				convertPersonAndPersonSessionTo77();
				SqlitePreferences.Update ("databaseVersion", "0.77", true); 
				Log.WriteLine("Converted DB to 0.77 (person77, personSession77)"); 
				
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
				Log.WriteLine("Converted DB to 0.78 (Added machineID to preferences, takeOffWeight has no weight in db conversions since 0.66)"); 

				Sqlite.Close();
				currentVersion = "0.78";
			}
			if(currentVersion == "0.78") {
				Sqlite.Open();

				SqlitePreferences.Insert ("multimediaStorage", Constants.MultimediaStorage.BYSESSION.ToString());

				SqlitePreferences.Update ("databaseVersion", "0.79", true); 
				Log.WriteLine("Converted DB to 0.79 (Added multimediaStorage structure id)"); 

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
					Log.WriteLine("Converted DB to 0.80 Added run and runInterval initial speed (if not done in 0.56 conversion)"); 
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
				Log.WriteLine("Converted DB to 0.81 Added tempRunInterval initial speed"); 

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
				Log.WriteLine("Converted DB to 0.82 Added videoOn"); 

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
				Log.WriteLine("Created encoder tables.");

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

				Log.WriteLine("Added 1st RSA test.");

				SqlitePreferences.Update ("databaseVersion", "0.84", true); 
				Sqlite.Close();
				currentVersion = "0.84";
			}
			if(currentVersion == "0.84") {
				Sqlite.Open();
				SqliteJumpType.JumpTypeInsert ("slCMJ:1:0:Single-leg CMJ jump", true);

				SqlitePreferences.Update ("databaseVersion", "0.85", true); 
				
				Log.WriteLine("Converted DB to 0.85 (added slCMJ jump)"); 
				Sqlite.Close();
				currentVersion = "0.85";
			}
			if(currentVersion == "0.85") {
				Sqlite.Open();
				Log.WriteLine("Converted DB to 0.86 videoOn: TRUE"); 

				SqlitePreferences.Update("videoOn", "True", true);
				SqlitePreferences.Update ("databaseVersion", "0.86", true); 
				
				Sqlite.Close();
				currentVersion = "0.86";
			}
			if(currentVersion == "0.86") {
				Sqlite.Open();
				Log.WriteLine("Added run speed start preferences on sqlite"); 

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
				dbcmd.CommandText = "Delete FROM " + Constants.RunIntervalTable +
					" WHERE type == 'RSA 8-4-R3-5'";
				Log.WriteLine(dbcmd.CommandText.ToString());
				dbcmd.ExecuteNonQuery();
				
				//add know RSAs
				SqliteRunIntervalType.addRSA();
				addedRSA = true;

				Log.WriteLine("Deleted fake RSA test and added known RSA tests.");
				
				SqlitePreferences.Update ("databaseVersion", "0.88", true); 
				Sqlite.Close();

				currentVersion = "0.88";
			}
			if(currentVersion == "0.88") {
				Sqlite.Open();
	
				SqliteEncoder.addEncoderFreeExercise();
				
				Log.WriteLine("Added encoder exercise: Free");
				
				SqlitePreferences.Update ("databaseVersion", "0.89", true); 
				Sqlite.Close();

				currentVersion = "0.89";
			}
			if(currentVersion == "0.89") {
				Sqlite.Open();
	
				SqlitePreferences.Insert("encoderPropulsive", "True");
				SqlitePreferences.Insert("encoderSmoothEccCon", "0.6");
				SqlitePreferences.Insert("encoderSmoothCon", "0.7");
				Log.WriteLine("Preferences added propulsive and encoder smooth");
				
				SqlitePreferences.Update ("databaseVersion", "0.90", true); 
				Sqlite.Close();

				currentVersion = "0.90";
			}
			if(currentVersion == "0.90") {
				Sqlite.Open();
				
				SqliteEncoder.UpdateExercise(true, "Squat", "Squat", 100, "weight bar", "", "");	
				Log.WriteLine("Encoder Squat 75% -> 100%");
				
				SqlitePreferences.Update ("databaseVersion", "0.91", true); 
				Sqlite.Close();

				currentVersion = "0.91";
			}
			if(currentVersion == "0.91") {
				Sqlite.Open();
				
				SqlitePreferences.Insert("videoDevice", "0");
				Log.WriteLine("Added videoDevice to preferences");
				
				SqlitePreferences.Update ("databaseVersion", "0.92", true); 
				Sqlite.Close();

				currentVersion = "0.92";
			}
			if(currentVersion == "0.92") {
				Sqlite.Open();
				
				SqliteEncoder.UpdateExercise(true, "Bench press", "Bench press", 0, "weight bar", "","0.185");
				SqliteEncoder.UpdateExercise(true, "Squat", "Squat", 100, "weight bar", "","0.31");
				Log.WriteLine("Added speed1RM on encoder exercise");
				
				SqlitePreferences.Update ("databaseVersion", "0.93", true); 
				Sqlite.Close();

				currentVersion = "0.93";
			}
			if(currentVersion == "0.93") {
				Sqlite.Open();
				
				SqliteEncoder.createTable1RM();
				Log.WriteLine("Added encoder1RM table");
				
				SqlitePreferences.Update ("databaseVersion", "0.94", true); 
				Sqlite.Close();

				currentVersion = "0.94";
			}
			if(currentVersion == "0.94") {
				Sqlite.Open();
				
				SqlitePreferences.Insert ("encoder1RMMethod", 
						Constants.Encoder1RMMethod.WEIGHTED2.ToString());
				Log.WriteLine("Added encoder1RMMethod");
				
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

				Log.WriteLine("Encoder signal future3 three modes");
				
				SqlitePreferences.Update ("databaseVersion", "0.96", true); 
				Sqlite.Close();

				currentVersion = "0.96";
			}
			if(currentVersion == "0.96") {
				Sqlite.Open();
				
				SqlitePreferences.Insert ("inertialmomentum", "0.01");
				Log.WriteLine("Added inertialmomentum in preferences");
				
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
				Log.WriteLine("Fixed encoder laterality");
				
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
						es.exerciseID + ", '" + es.eccon + "', '" +
						es.laterality + "', '" + es.extraWeight + "', '" +
						es.signalOrCurve + "', '" + es.filename + "', '" +
						es.url + "', " + es.time + ", " + es.minHeight + ", " +
						Util.ConvertToPoint(es.smooth) + ", '" + es.description + "', '" +
						es.future1 + "', '" + es.future2 + "', 'LINEAR', " + //status, videoURL, mode
						"0, 0, '', '', '')"; //inertiaMomentum, diameter, future1, 2, 3
					Log.WriteLine(dbcmd.CommandText.ToString());
					dbcmd.ExecuteNonQuery();
					count ++;
				}	

				conversionRate = count;
				Log.WriteLine("Encoder table improved");
				SqlitePreferences.Update ("databaseVersion", "0.99", true); 
				Sqlite.Close();

				currentVersion = "0.99";
			}
			if(currentVersion == "0.99") {
				Sqlite.Open();

				SqliteEncoder.putEncoderExerciseAnglesAt90();
				SqliteEncoder.addEncoderJumpExercise();
				SqliteEncoder.addEncoderInclinatedExercises();

				Log.WriteLine("Added Free and inclinatedExercises");
				SqlitePreferences.Update ("databaseVersion", "1.00", true); 
				Sqlite.Close();

				currentVersion = "1.00";
			}
			if(currentVersion == "1.00") {
				Sqlite.Open();
			
				SqlitePreferences.Insert ("CSVExportDecimalSeparator", Util.GetDecimalSeparatorFromLocale());

				Log.WriteLine("Added export to CSV configuration on preferences");
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

				Log.WriteLine("Added Agility Tests: Agility-T-Test, Agility-3l3R");
				SqlitePreferences.Update ("databaseVersion", "1.02", true); 
				Sqlite.Close();

				currentVersion = "1.02";
			}
			if(currentVersion == "1.02") {
				Sqlite.Open();
		
				DeleteFromName(true, Constants.EncoderExerciseTable, "Inclinated plane Custom");
				SqliteEncoder.removeEncoderExerciseAngles();

				Log.WriteLine("Updated encoder exercise, angle is now on encoder configuration");
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
						es.exerciseID + ", '" + es.eccon + "', '" +
						es.laterality + "', '" + es.extraWeight + "', '" +
						es.signalOrCurve + "', '" + es.filename + "', '" +
						es.url + "', " + es.time + ", " + es.minHeight + ", '" + es.description + "', '" + 
						es.status + "', '" + es.videoURL + "', '" + 
						econf.ToString(":", true) + "', '" + //in this conversion put this as default for all SQL rows
						es.future1 + "', '" + es.future2 + "', '" + es.future3 + "')";
					Log.WriteLine(dbcmd.CommandText.ToString());
					dbcmd.ExecuteNonQuery();
					count ++;
				}	

				conversionRate = count;
				Log.WriteLine("Encoder table improved");
				SqlitePreferences.Update ("databaseVersion", "1.04", true); 
				Sqlite.Close();

				currentVersion = "1.04";
			}
			if(currentVersion == "1.04") {
				Sqlite.Open();
				
				dbcmd.CommandText = "DELETE FROM " + Constants.EncoderTable + 
					" WHERE encoderConfiguration LIKE \"%INERTIAL%\" AND " +
					" signalOrCurve == \"curve\"";
				Log.WriteLine(dbcmd.CommandText.ToString());
				dbcmd.ExecuteNonQuery();

				Log.WriteLine("Removed inertial curves, because sign was not checked on 1.04 when saving curves");
				SqlitePreferences.Update ("databaseVersion", "1.05", true); 
				Sqlite.Close();

				currentVersion = "1.05";
			}
			if(currentVersion == "1.05") {
				Sqlite.Open();
		
				SqliteEncoder.createTableEncoderSignalCurve();

				ArrayList signals = SqliteEncoder.Select(true, -1, -1, -1, -1, "signal", EncoderSQL.Eccons.ALL, false, false);
				ArrayList curves = SqliteEncoder.Select(true, -1, -1, -1, -1, "curve", EncoderSQL.Eccons.ALL, false, false);
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
						if(s.GetDate(false) == c.GetDate(false) && s.eccon == c.eccon) {
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
				Log.WriteLine("Curves are now linked to signals");
				SqlitePreferences.Update ("databaseVersion", "1.06", true); 
				
				Sqlite.Close();
				currentVersion = "1.06";
			}
			if(currentVersion == "1.06") {
				Sqlite.Open();
			
				Update(true, Constants.GraphLinkTable, "graphFileName", "jump_dj.png", "jump_dj_a.png",
						"eventName", "DJa");
				
				Log.WriteLine("Added jump_dj_a.png");
				SqlitePreferences.Update ("databaseVersion", "1.07", true); 
				Sqlite.Close();

				currentVersion = "1.07";
			}
			if(currentVersion == "1.07") {
				Sqlite.Open();
			
				Log.WriteLine("Added translate statistics graph option to preferences");
				
				SqlitePreferences.Insert ("RGraphsTranslate", "True"); 
				SqlitePreferences.Update ("databaseVersion", "1.08", true); 
				Sqlite.Close();

				currentVersion = "1.08";
			}
			if(currentVersion == "1.08") {
				Sqlite.Open();
			
				Log.WriteLine("Added option on preferences to useHeightsOnJumpIndexes (default) or not");
				
				SqlitePreferences.Insert ("useHeightsOnJumpIndexes", "True"); 
				SqlitePreferences.Update ("databaseVersion", "1.09", true); 
				Sqlite.Close();

				currentVersion = "1.09";
			}
			if(currentVersion == "1.09") {
				Sqlite.Open();
			
				Log.WriteLine("Added RSA RAST on runType");

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
			
				Log.WriteLine("Added option on autosave curves on capture (all/bestmeanpower/none)");
				
				SqlitePreferences.Insert ("encoderAutoSaveCurve", Constants.EncoderAutoSaveCurve.BESTMEANPOWER.ToString()); 
				SqlitePreferences.Update ("databaseVersion", "1.11", true); 
				Sqlite.Close();

				currentVersion = "1.11";
			}
			if(currentVersion == "1.11") {
				Sqlite.Open();
			
				Log.WriteLine("URLs from absolute to relative)");
				
				SqliteOldConvert.ConvertAbsolutePathsToRelative(); 
				SqlitePreferences.Update ("databaseVersion", "1.12", true); 
				Sqlite.Close();

				currentVersion = "1.12";
			}
			if(currentVersion == "1.12") {
				Sqlite.Open();
			
				Log.WriteLine("Added ExecuteAuto table");
				
				SqliteExecuteAuto.createTableExecuteAuto();
				SqlitePreferences.Update ("databaseVersion", "1.13", true); 
				Sqlite.Close();

				currentVersion = "1.13";
			}
			if(currentVersion == "1.13") {
				Sqlite.Open();
			
				Log.WriteLine("slCMJ -> slCMJleft, slCMJright");

				SqliteOldConvert.slCMJDivide();
				SqlitePreferences.Update ("databaseVersion", "1.14", true); 
				Sqlite.Close();

				currentVersion = "1.14";
			}
			if(currentVersion == "1.14") {
				Sqlite.Open();
			
				Log.WriteLine("added Chronojump profile and bilateral profile");

				SqliteExecuteAuto.addChronojumpProfileAndBilateral();
				SqlitePreferences.Update ("databaseVersion", "1.15", true); 
				Sqlite.Close();

				currentVersion = "1.15";
			}
			if(currentVersion == "1.15") {
				Sqlite.Open();
			
				Log.WriteLine("Cyprus moved to Europe");

				Update(true, Constants.CountryTable, "continent", "Asia", "Europe", "code", "CYP"); 
				SqlitePreferences.Update ("databaseVersion", "1.16", true); 
				Sqlite.Close();

				currentVersion = "1.16";
			}
			if(currentVersion == "1.16") {
				Sqlite.Open();
			
				Log.WriteLine("Deleting Max jump");

				Update(true, Constants.JumpTable, "type", "Max", "Free", "", ""); 
				DeleteFromName(true, Constants.JumpTypeTable, "Max");
				SqlitePreferences.Update ("databaseVersion", "1.17", true); 
				Sqlite.Close();

				currentVersion = "1.17";
			}
			if(currentVersion == "1.17") {
				Sqlite.Open();
			
				Log.WriteLine("Deleted Negative runInterval runs (bug from last version)");

				SqliteOldConvert.deleteNegativeRuns();
				SqlitePreferences.Update ("databaseVersion", "1.18", true); 
				Sqlite.Close();

				currentVersion = "1.18";
			}
	
		}

		//if changes are made here, remember to change also in CreateTables()
		//remember to change also the databaseVersion below
		
		return returnSoftwareIsNew;
	}

	public static bool ChangeDjToDJna() {
		string v = SqlitePreferences.Select("databaseVersion");
		Log.WriteLine(Convert.ToDouble(Util.ChangeDecimalSeparator(v)).ToString());
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
			
			Log.WriteLine("Added Chronopic port");
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
			SqlitePersonSessionNotUpload.CreateTable();
			creationRate ++;
		}
		

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
		SqlitePreferences.createTable();
		SqlitePreferences.initializeTable(lastChronojumpDatabaseVersion, creatingBlankDatabase);
		
		creationRate ++;
		SqliteCountry.createTable();
		SqliteCountry.initialize();
				
		SqliteExecuteAuto.createTableExecuteAuto();
		SqliteExecuteAuto.addChronojumpProfileAndBilateral();
		
		//changes [from - to - desc]
		//1.18 - 1.18 Converted DB to 1.18 deleted Negative runInterval runs (bug from last version)
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
		//0.99 - 1.00 Converted DB to 1.00 Encoder added Free and Inclinated Exercises
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

	public static bool Exists(bool dbconOpened, string tableName, string findName)
	{
		if(!dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "SELECT uniqueID FROM " + tableName + 
			" WHERE LOWER(name) == LOWER('" + findName + "')" ;
		Log.WriteLine(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
	
		bool exists = new bool();
		exists = false;
		
		if (reader.Read()) {
			exists = true;
		}
		Log.WriteLine(string.Format("name exists = {0}", exists.ToString()));

		reader.Close();
		if(!dbconOpened)
			Sqlite.Close();

		return exists;
	}

	public static string SQLBuildQueryString (string tableName, string test, string variable,
			int sex, string ageInterval,
			int countryID, int sportID, int speciallityID, int levelID, int evaluatorID)
	{
		string tp = Constants.PersonTable;
		string tps = Constants.PersonSessionTable;

		string strSelect = "SELECT COUNT(" + variable + "), AVG(" + variable + ")";
		string strFrom   = " FROM " + tableName;
		string strWhere  = " WHERE " + tableName + ".type = '" + test + "'";

		string strSex = "";
		if(sex == Constants.MaleID) 
			strSex = " AND " + tp + ".sex == '" + Constants.M + "'";
		else if (sex == Constants.FemaleID) 
			strSex = " AND " + tp + ".sex == '" + Constants.F + "'";

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
		Log.WriteLine(dbcmd.CommandText.ToString());
		
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
		Log.WriteLine(string.Format("exists = {0}", exists.ToString()));
		reader.Close();
		Sqlite.Close();

		return exists;
	}

	public static void DeleteTempEvents(string tableName)
	{
		//tableName can be tempJumpRj or tempRunInterval

		Sqlite.Open();
		//dbcmd.CommandText = "Delete FROM tempJumpRj";
		dbcmd.CommandText = "Delete FROM " + tableName;
		Log.WriteLine(dbcmd.CommandText.ToString());
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
				       pOld.ServerUniqueID
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
						"" 		//comments
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
			dbcmd.CommandText = "UPDATE person set dateBorn = '" + UtilDate.ToSql(dt) +
				"' WHERE uniqueID = " + id_date[0];
			Log.WriteLine(dbcmd.CommandText.ToString());
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
			dbcmd.CommandText = "UPDATE session set date = '" + UtilDate.ToSql(dt) +
				"' WHERE uniqueID = " + id_date[0];
			Log.WriteLine(dbcmd.CommandText.ToString());
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
			dbcmd.CommandText = "UPDATE SEvaluator set dateBorn = '" + UtilDate.ToSql(dt) +
				"' WHERE uniqueID = " + id_date[0];
			Log.WriteLine(dbcmd.CommandText.ToString());
			dbcmd.ExecuteNonQuery();
			conversionSubRate ++;
		}
		conversionRate ++;
	}

	//used to delete persons (if needed) when a session is deleted. See SqliteSession.DeleteAllStuff
	protected internal static void deleteOrphanedPersons()
	{
		dbcmd.CommandText = "SELECT uniqueID FROM " + Constants.PersonTable;
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		ArrayList myArray = new ArrayList(1);

		while(reader.Read())
			myArray.Add (Convert.ToInt32(reader[0]));
		reader.Close();

		foreach(int personID in myArray) {
			//if person is not in other sessions, delete it from DB
			if(! SqlitePersonSession.PersonExistsInPS(true, personID))
				Delete(true, Constants.PersonTable, personID);
		}
	}
				
	//used to delete persons (if needed) when a session is deleted. See SqliteSession.DeleteAllStuff
	//also used to convert to sqlite 0.73
	//this is old method (before .77), now use above method
	protected internal static void deleteOrphanedPersonsOld()
	{
		dbcmd.CommandText = "SELECT uniqueID FROM " + Constants.PersonOldTable;
		Log.WriteLine(dbcmd.CommandText.ToString());
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
					dbcmd.CommandText = "UPDATE jump SET type = '" + name + "' WHERE type == 'DJa'";
					Log.WriteLine(dbcmd.CommandText.ToString());
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
					dbcmd.CommandText = "UPDATE jump SET type = '" + name + "' WHERE type == 'DJna'";
					Log.WriteLine(dbcmd.CommandText.ToString());
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
		dbcmd.CommandText = "UPDATE jump SET description = description || ' Auto-converted from DJ' WHERE type == 'DJ'";
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		//conversion
		dbcmd.CommandText = "UPDATE jump SET type = 'DJna' WHERE type == 'DJ'";
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		//delete DJ
		SqliteJumpType.Delete(Constants.JumpTypeTable, "DJ", true);
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
		Log.WriteLine(dbcmd.CommandText.ToString());
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

Console.WriteLine("1" + tableName);

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
		
Console.WriteLine("2" + tableName);
		//3rd drop desired table
		Sqlite.dropTable(tableName);

Console.WriteLine("3" + tableName);
		//4d create desired table (now with new columns)
		sqliteObject.createTable(tableName);


Console.WriteLine("4" + tableName);

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

Console.WriteLine("5" + tableName);
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
		Log.WriteLine(dbcmd.CommandText.ToString());

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
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader = dbcmd.ExecuteReader();

		IDNameList list = new IDNameList();
		while(reader.Read()) {
			IDName idname = new IDName( Convert.ToInt32(reader[0].ToString()),
						reader[1].ToString());
			Log.WriteLine(idname.ToString());
			
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
		Log.WriteLine(dbcmd.CommandText.ToString());
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

	public static int Max (string tableName, string column, bool dbconOpened)
	{
		if(!dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "SELECT MAX(" + column + ") FROM " + tableName ;
		Log.WriteLine(dbcmd.CommandText.ToString());
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
		Log.WriteLine(dbcmd.CommandText.ToString());
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
		Log.WriteLine(dbcmd.CommandText.ToString());
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

	public static void Update(bool dbconOpened, string tableName, string columnName, string searchValue, string newValue, 
			string columnNameCondition2, string searchValueCondition2)
	{
		if( ! dbconOpened)
			Sqlite.Open();
		
		string andStr = "";
		if(columnNameCondition2 != "" && searchValueCondition2 != "")
			andStr = " AND " + columnNameCondition2 + " == '" + searchValueCondition2 + "'"; 

		dbcmd.CommandText = "Update " + tableName +
			" SET " + columnName + " = '" + newValue + "'" +  
			" WHERE " + columnName + " == '" + searchValue + "'" + 
			andStr
			;
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		if( ! dbconOpened)
			Sqlite.Close();
	}

	public static void Delete(bool dbconOpened, string tableName, int uniqueID)
	{
		if( ! dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "Delete FROM " + tableName +
			" WHERE uniqueID == " + uniqueID.ToString();
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		if( ! dbconOpened)
			Sqlite.Close();
	}
	
	public static void DeleteSelectingField(bool dbconOpened, string tableName, string fieldName, string id)
	{
		if( ! dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "Delete FROM " + tableName +
			" WHERE " + fieldName + " == " + id;
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		if( ! dbconOpened)
			Sqlite.Close();
	}


	public static void DeleteFromName(bool dbconOpened, string tableName, string name)
	{
		if( ! dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "Delete FROM " + tableName +
			" WHERE name == '" + name + "'";
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		if( ! dbconOpened)
			Sqlite.Close();
	}

	public static void DeleteFromAnInt(bool dbconOpened, string tableName, string colName, int id)
	{
		if( ! dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "Delete FROM " + tableName +
			" WHERE " + colName + " == " + id;
		Log.WriteLine(dbcmd.CommandText.ToString());
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
