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
 * Copyright (C) 2004-2009   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using System.IO; //"File" things. TextWriter
using System.Collections; //ArrayList
using Mono.Data.Sqlite;
using System.Diagnostics; 	//for launching other process


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
	 * Important2: if database version get numbers higher than 1, check if the comparisons with currentVersion works ok
	 */
	static string lastChronojumpDatabaseVersion = "0.73";

	public Sqlite() {
	}

	/*
	public void CreateTable(string tableName) {
		createTable(tableName);
	}
	*/
	protected virtual void createTable(string tableName) {
	}
	
	~Sqlite() {}


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

//			dbcon = new SqliteConnection();
			*/
		/*
			dbcon.ConnectionString = connectionString;
			//dbcon.ConnectionString = connectionStringTemp;
			dbcmd = dbcon.CreateCommand();
		} catch {
			try {
				Log.WriteLine(string.Format("Trying database in ... " + connectionStringTemp));

//				dbcon = new SqliteConnection();
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

		string applicationDataDir = Util.GetApplicationDataDir();

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

//		try {	
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
			dbcon.Close();
			return true;
		}
		else if(sqlite2SelectWorks()) {
			Log.WriteLine("SQLITE2");
			dbcon.Close();
			//write sqlFile path on data/databasePath.txt
			//TODO
			//

			return false;
		}
		else {
			Log.WriteLine("ERROR in sqlite detection");
			dbcon.Close();
			return false;
		}
	}
	private static bool sqlite3SelectWorks() {
		try {
			SqlitePreferences.Select("chronopicPort");
		} catch {
			/*
			try {
				dbcon.Close();
				if(File.Exists(Util.GetDatabaseDir() + Path.DirectorySeparatorChar + "chronojump.db"))
					File.Move(Util.GetDatabaseDir() + Path.DirectorySeparatorChar + "chronojump.db",
							Util.GetDatabaseTempDir() + Path.DirectorySeparatorChar + "chronojump.db");

				dbcon.ConnectionString = connectionStringTemp;
				dbcmd = dbcon.CreateCommand();
				dbcon.Open();
				SqlitePreferences.Select("chronopicPort");
			} catch {
				dbcon.Close();
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
		dbcon.Close();
		connectionString = "version=2; URI=file:" + sqlFile;
		dbcon.ConnectionString = connectionString;
		dbcon.Open();
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
			if(Util.IsWindows()) {
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

			SqliteJumpRj sqliteJumpRjObject = new SqliteJumpRj();
			SqliteRunInterval sqliteRunIntervalObject = new SqliteRunInterval();
			SqliteReactionTime sqliteReactionTimeObject = new SqliteReactionTime();
			SqlitePulse sqlitePulseObject = new SqlitePulse();
			SqliteMultiChronopic sqliteMultiChronopicObject = new SqliteMultiChronopic();

			if(currentVersion == "0.41") {
				dbcon.Open();

				//SqlitePulse.createTable(Constants.PulseTable);
				sqlitePulseObject.createTable(Constants.PulseTable);
				SqlitePulseType.createTablePulseType();
				SqlitePulseType.initializeTablePulseType();

				SqlitePreferences.Update ("databaseVersion", "0.42", true); 
				Log.WriteLine("Converted DB to 0.42 (added pulse and pulseType tables)");

				dbcon.Close();
				currentVersion = "0.42";
			}

			if(currentVersion == "0.42") {
				dbcon.Open();
				SqlitePulseType.Insert ("Free:-1:-1:free PulseStep mode", true); 
				SqlitePreferences.Insert ("language", "es-ES"); 
				SqlitePreferences.Update ("databaseVersion", "0.43", true); 
				Log.WriteLine("Converted DB to 0.43 (added 'free' pulseType & language peference)");
				dbcon.Close();
				currentVersion = "0.43";
			}

			if(currentVersion == "0.43") {
				dbcon.Open();
				SqlitePreferences.Insert ("showQIndex", "False"); 
				SqlitePreferences.Insert ("showDjIndex", "False"); 
				SqlitePreferences.Update ("databaseVersion", "0.44", true); 
				Log.WriteLine("Converted DB to 0.44 (added showQIndex, showDjIndex)");
				dbcon.Close();
				currentVersion = "0.44";
			}

			if(currentVersion == "0.44") {
				dbcon.Open();
				SqlitePreferences.Insert ("allowFinishRjAfterTime", "True"); 
				SqlitePreferences.Update ("databaseVersion", "0.45", true); 
				Log.WriteLine("Converted DB to 0.45 (added allowFinishRjAfterTime)");
				dbcon.Close();
				currentVersion = "0.45";
			}

			if(currentVersion == "0.45") {
				dbcon.Open();
				SqliteJumpType.JumpTypeInsert ("Free:1:0:Free jump", true); 
				SqlitePreferences.Update ("databaseVersion", "0.46", true); 
				Log.WriteLine("Added Free jump type");
				dbcon.Close();
				currentVersion = "0.46";
			}

			if(currentVersion == "0.46") {
				dbcon.Open();

				//SqliteReactionTime.createTable(Constants.ReactionTimeTable);
				sqliteReactionTimeObject.createTable(Constants.ReactionTimeTable);

				SqlitePreferences.Update ("databaseVersion", "0.47", true); 
				Log.WriteLine("Added reaction time table");
				dbcon.Close();
				currentVersion = "0.47";
			}

			if(currentVersion == "0.47") {
				dbcon.Open();

				//SqliteJumpRj.createTable(Constants.TempJumpRjTable);
				sqliteJumpRjObject.createTable(Constants.TempJumpRjTable);
				//SqliteRun.intervalCreateTable(Constants.TempRunIntervalTable);
				sqliteRunIntervalObject.createTable(Constants.TempRunIntervalTable);

				SqlitePreferences.Update ("databaseVersion", "0.48", true); 
				Log.WriteLine("created tempJumpReactive and tempRunInterval tables");
				dbcon.Close();
				currentVersion = "0.48";
			}

			if(currentVersion == "0.48") {
				dbcon.Open();

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

				dbcon.Close();
				currentVersion = "0.49";
			}

			if(currentVersion == "0.49") {
				dbcon.Open();
				SqliteJumpType.Update ("SJ+", "SJl"); 
				SqliteJumpType.Update ("CMJ+", "CJl"); 
				SqliteJumpType.Update ("ABK+", "ABKl"); 
				SqliteJump.ChangeWeightToL();
				SqliteJumpType.AddGraphLinks();	
				SqliteJumpType.AddGraphLinksRj();	
				SqlitePreferences.Update ("databaseVersion", "0.50", true); 
				Log.WriteLine("changed SJ+ to SJl, same for CMJ+ and ABK+, added jump and jumpRj graph links");
				dbcon.Close();
				currentVersion = "0.50";
			}

			if(currentVersion == "0.50") {
				dbcon.Open();
				SqliteRunType.AddGraphLinksRunSimple();	
				SqliteRunIntervalType.AddGraphLinksRunInterval();	
				SqlitePreferences.Update ("databaseVersion", "0.51", true); 
				Log.WriteLine("added graphLinks for run simple and interval");
				dbcon.Close();
				currentVersion = "0.51";
			}

			if(currentVersion == "0.51") {
				dbcon.Open();
				SqliteJumpType.Update ("CJl", "CMJl"); 
				SqliteEvent.GraphLinkInsert (Constants.JumpTable, "CMJl", "jump_cmj_l.png", true);
				SqliteEvent.GraphLinkInsert (Constants.JumpTable, "ABKl", "jump_abk_l.png", true);
				SqlitePreferences.Update ("databaseVersion", "0.52", true); 
				Log.WriteLine("added graphLinks for cmj_l and abk_l, fixed CMJl name");
				dbcon.Close();
				currentVersion = "0.52";
			}
			
			if(currentVersion == "0.52") {
				dbcon.Open();
				SqlitePersonSession.createTable (); 
				dbcon.Close();
				
				//this needs the dbCon closed
				SqlitePersonSession.moveOldTableToNewTable (); 
				
				dbcon.Open();
				SqlitePreferences.Update ("databaseVersion", "0.53", true); 
				dbcon.Close();
				
				Log.WriteLine("created weightSession table. Moved person weight data to weightSession table for each session that has performed");
				currentVersion = "0.53";
			}
			
			if(currentVersion == "0.53") {
				dbcon.Open();

				SqliteSport.createTable();
				SqliteSport.initialize();
				SqliteSpeciallity.createTable();
				SqliteSpeciallity.initialize();

				//SqlitePerson.convertTableToSportRelated (); 
				needToConvertPersonToSport = true;
				
				SqlitePreferences.Update ("databaseVersion", "0.54", true); 
				dbcon.Close();
				
				Log.WriteLine("Created sport tables. Added sport data, speciallity and level of practice to person table");
				currentVersion = "0.54";
			}
			if(currentVersion == "0.54") {
				dbcon.Open();

				SqliteSpeciallity.InsertUndefined(true);

				SqlitePreferences.Update ("databaseVersion", "0.55", true); 
				dbcon.Close();
				
				Log.WriteLine("Added undefined to speciallity table");
				currentVersion = "0.55";
			}
			if(currentVersion == "0.55") {
				dbcon.Open();

				SqliteSession.convertTableAddingSportStuff();

				SqlitePreferences.Update ("databaseVersion", "0.56", true); 
				dbcon.Close();
				
				Log.WriteLine("Added session default sport stuff into session table");
				currentVersion = "0.56";
			}
			if(currentVersion == "0.56") {
				dbcon.Open();

				//jump and jumpRj
				ArrayList arrayAngleAndSimulated = new ArrayList(1);
				arrayAngleAndSimulated.Add("-1"); //angle
				arrayAngleAndSimulated.Add("-1"); //simulated
				
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
				convertTables(new SqlitePerson(), Constants.PersonTable, columnsBefore, arrayPersonRaceCountryServerID, putDescriptionInMiddle);

				SqlitePreferences.Update ("databaseVersion", "0.57", true); 
				dbcon.Close();
				
				Log.WriteLine("Added simulated column to each event table on client. Added to person: race, country, serverUniqueID. Convert to sport related done here if needed");
				currentVersion = "0.57";
			}
			if(currentVersion == "0.57") {
				dbcon.Open();
		
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
				dbcon.Close();
				
				currentVersion = "0.58";
			}

			if(currentVersion == "0.58") {
				dbcon.Open();
				conversionRateTotal = 2;
				conversionRate = 1;
				SqlitePreferences.Insert ("showAngle", "False"); 
				alterTableColumn(new SqliteJump(), Constants.JumpTable, 11);

				SqlitePreferences.Update ("databaseVersion", "0.59", true); 
				Log.WriteLine("Converted DB to 0.59 (added 'showAngle' to preferences, changed angle on jump to double)"); 
				conversionRate = 2;
				dbcon.Close();
				currentVersion = "0.59";
			}

			if(currentVersion == "0.59") {
				dbcon.Open();
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
				dbcon.Close();
				currentVersion = "0.60";
			}

			if(currentVersion == "0.60") {
				dbcon.Open();
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
				dbcon.Close();
				currentVersion = "0.61";
			}
			if(currentVersion == "0.61") {
				dbcon.Open();
				SqliteJumpType.JumpRjTypeInsert ("RJ(hexagon):1:0:1:18:Reactive Jump on a hexagon until three full revolutions are done", true);
				SqlitePreferences.Update ("databaseVersion", "0.62", true); 
				Log.WriteLine("Converted DB to 0.62 added hexagon");
				dbcon.Close();
				currentVersion = "0.62";
			}
			if(currentVersion == "0.62") {
				dbcon.Open();
				SqlitePreferences.Insert ("versionAvailable", "");
				SqlitePreferences.Update ("databaseVersion", "0.63", true); 
				Log.WriteLine("Converted DB to 0.63 (added 'versionAvailable' to preferences)"); 
				dbcon.Close();
				currentVersion = "0.63";
			}
			if(currentVersion == "0.63") {
				dbcon.Open();
				
				RunType type = new RunType();
				type.Name = "Margaria";
				type.Distance = 0;
				type.Description = "Margaria-Kalamen test";
				SqliteRunType.Insert(type, Constants.RunTypeTable, true);

				SqliteEvent.GraphLinkInsert (Constants.RunTable, "Margaria", "margaria.png", true);
				SqlitePreferences.Update ("databaseVersion", "0.64", true); 
				
				Log.WriteLine("Converted DB to 0.64 (added margaria test)"); 
				dbcon.Close();
				currentVersion = "0.64";
			}
			if(currentVersion == "0.64") {
				dbcon.Open();
				
				SqliteServer sqliteServerObject = new SqliteServer();
				//user has also an evaluator table with a row (it's row)	
				sqliteServerObject.CreateEvaluatorTable();

				SqlitePreferences.Update ("databaseVersion", "0.65", true); 
				
				Log.WriteLine("Converted DB to 0.65 (added Sevaluator on client)"); 
				dbcon.Close();
				currentVersion = "0.65";
			}
			if(currentVersion == "0.65") {
				dbcon.Open();
				//now runAnalysis is a multiChronopic event
				//SqliteJumpType.JumpRjTypeInsert ("RunAnalysis:0:0:1:-1:Run between two photocells recording contact and flight times in contact platform/s. Until finish button is clicked.", true);

				SqlitePreferences.Update ("databaseVersion", "0.66", true); 
				
				//Log.WriteLine("Converted DB to 0.66 (added RunAnalysis Reactive jump)"); 
				Log.WriteLine("Converted DB to 0.66 (done nothing)"); 
				dbcon.Close();
				currentVersion = "0.66";
			}
			if(currentVersion == "0.66") {
				dbcon.Open();
				SqliteJumpType.JumpTypeInsert ("TakeOff:0:0:Take off", true);
				SqliteJumpType.JumpTypeInsert ("TakeOffWeight:0:0:Take off with weight", true);

				SqlitePreferences.Update ("databaseVersion", "0.67", true); 
				
				Log.WriteLine("Converted DB to 0.67 (added TakeOff jumps)"); 
				dbcon.Close();
				currentVersion = "0.67";
			}
			if(currentVersion == "0.67") {
				dbcon.Open();
				sqliteMultiChronopicObject.createTable(Constants.MultiChronopicTable);

				SqlitePreferences.Update ("databaseVersion", "0.68", true); 
				
				Log.WriteLine("Converted DB to 0.68 (added multiChronopic tests table)"); 
				dbcon.Close();
				currentVersion = "0.68";
			}
			if(currentVersion == "0.68") {
				dbcon.Open();
				
				RunType type = new RunType();
				type.Name = "Gesell-DBT";
				type.Distance = 2.5;
				type.Description = "Gesell Dynamic Balance Test";
				SqliteRunType.Insert(type, Constants.RunTypeTable, true);

				SqliteEvent.GraphLinkInsert (Constants.RunTable, "Gesell-DBT", "gesell_dbt.png", true);
				SqlitePreferences.Update ("databaseVersion", "0.69", true); 
				
				Log.WriteLine("Converted DB to 0.69 (added Gesell-DBT test)"); 
				dbcon.Close();
				currentVersion = "0.69";
			}
			if(currentVersion == "0.69") {
				dbcon.Open();
				SqlitePreferences.Insert ("showPower", "True"); 
				SqlitePreferences.Update ("databaseVersion", "0.70", true); 
				Log.WriteLine("Converted DB to 0.70 (added showPower)");
				dbcon.Close();
				currentVersion = "0.70";
			}
			if(currentVersion == "0.70") {
				dbcon.Open();
				
				SqlitePersonSessionNotUpload.CreateTable();

				SqlitePreferences.Update ("databaseVersion", "0.71", true); 
				
				Log.WriteLine("Converted DB to 0.71 (created personNotUploadTable on client)"); 
				dbcon.Close();
				currentVersion = "0.71";
			}
			if(currentVersion == "0.71") {
				dbcon.Open();
				
				datesToYYYYMMDD();

				SqlitePreferences.Update ("databaseVersion", "0.72", true); 
				
				Log.WriteLine("Converted DB to 0.72 (dates to YYYY-MM-DD)"); 
				dbcon.Close();
				currentVersion = "0.72";
			}
			if(currentVersion == "0.72") {
				dbcon.Open();
				
				deleteOrphanedPersons();

				SqlitePreferences.Update ("databaseVersion", "0.73", true); 
				
				Log.WriteLine("Converted DB to 0.73 (deleted orphaned persons (in person table but not in personSessionWeight table)"); 
				dbcon.Close();
				currentVersion = "0.73";
			}


		}

		//if changes are made here, remember to change also in CreateTables()
		//remember to change also the databaseVersion below
		
		return returnSoftwareIsNew;
	}

	private static void addChronopicPortNameIfNotExists() {
		string myPort = SqlitePreferences.Select("chronopicPort");
		if(myPort == "0") {
			//if doesn't exist (for any reason, like old database)
			dbcon.Open();
			if(Util.IsWindows() || creatingBlankDatabase)
				SqlitePreferences.Insert ("chronopicPort", Constants.ChronopicDefaultPortWindows);
			else
				SqlitePreferences.Insert ("chronopicPort", Constants.ChronopicDefaultPortLinux);
			dbcon.Close();
			
			Log.WriteLine("Added Chronopic port");
		}
	}
	
	public static void CreateTables(bool server)
	{
		dbcon.Open();

		creationTotal = 13;
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
	
		//sports
		creationRate ++;
		SqliteSport.createTable();
		SqliteSport.initialize();
		SqliteSpeciallity.createTable();
		SqliteSpeciallity.initialize();
		SqliteSpeciallity.InsertUndefined(true);

		creationRate ++;
		SqlitePersonSession.createTable();
		
		creationRate ++;
		SqlitePreferences.createTable();
		SqlitePreferences.initializeTable(lastChronojumpDatabaseVersion, creatingBlankDatabase);
		
		creationRate ++;
		SqliteCountry.createTable();
		SqliteCountry.initialize();
		
		//changes [from - to - desc]
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
		//0.56 - 0.57 Added simulated column to each event table on client. person: race, country, serverID. Convert to sport related done here if needed");
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
		

		dbcon.Close();
		creationRate ++;
	}

	public static bool Exists(string tableName, string findName)
	{
		dbcon.Open();
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

		dbcon.Close();
		return exists;
	}

	/* 
	 * temp data stuff
	 */
	public static int TempDataExists(string tableName)
	{
		//tableName can be tempJumpRj or tempRunInterval
		
		dbcon.Open();
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
		dbcon.Close();

		return exists;
	}

	public static void DeleteTempEvents(string tableName)
	{
		//tableName can be tempJumpRj or tempRunInterval

		dbcon.Open();
		//dbcmd.CommandText = "Delete FROM tempJumpRj";
		dbcmd.CommandText = "Delete FROM " + tableName;
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		dbcon.Close();
	}

	protected static void dropTable(string tableName) {
		dbcmd.CommandText = "DROP TABLE " + tableName;
		dbcmd.ExecuteNonQuery();
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

	//to convert to sqlite 0.73
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
			if(! SqlitePersonSession.PersonExistsInPSW(personID))
				SqlitePerson.Delete(personID);
		}
	}

	protected internal static void convertTables(Sqlite sqliteObject, string tableName, int columnsBefore, ArrayList columnsToAdd, bool putDescriptionInMiddle) 
	{
		conversionSubRate = 1;
		conversionSubRateTotal = -1; //unknown yet


		//2st create convert temp table
		sqliteObject.createTable(Constants.ConvertTempTable);

		//2nd copy all data from desired table to temp table adding the simulated column
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

			if(tableName == Constants.PersonTable) {	
				Person myPerson =  new Person(myReaderStr);
				myArray.Add(myPerson);
			} else if(tableName == Constants.SessionTable) {	
				Session mySession = new Session(myReaderStr);
				myArray.Add(mySession);
			} else if(tableName == Constants.RunIntervalTypeTable) {	
				RunType myType = new RunType(myReaderStr, true); //interval
				myArray.Add(myType);
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

		if(tableName == Constants.PersonTable) {	
			foreach (Person myPerson in myArray) {
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
		if(tableName == Constants.PersonTable) {	
			foreach (Person myPerson in myArray) {
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

	public static int Count (string tableName, bool dbconOpened)
	{
		if(!dbconOpened)
			dbcon.Open();

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
			dbcon.Close();
		return myReturn;
	}

	public static int CountCondition (string tableName, bool dbconOpened, string condition, string operand, string myValue) {
		if(!dbconOpened)
			dbcon.Open();

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
			dbcon.Close();
		return myReturn;
	}

	
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
			dbcon.Close();
		} catch {
			return false;
		}
		return true;
	}


}
