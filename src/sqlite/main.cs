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
 * Xavier de Blas: 
 * http://www.xdeblas.com, http://www.deporteyciencia.com (parleblas)
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
	//protected static IDbCommand dbcmd;

	//since we use installJammer (chronojump 0.7)	
	//database was on c:\.chronojump\ or in ~/.chronojump
	//now it's on installed dir, eg linux: ~/Chronojump/database
	public static string home = Util.GetDatabaseDir();
	public static string sqlFile = home + Path.DirectorySeparatorChar + "chronojump.db";

	//before installJammer
	public static string homeOld = Util.GetOldDatabaseDir();
	public static string sqlFileOld = homeOld + Path.DirectorySeparatorChar + "chronojump.db";
	
	//http://www.mono-project.com/SQLite

	static string connectionString = "version = 3; Data source = " + sqlFile;

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
	static string lastChronojumpDatabaseVersion = "0.57";

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


	public static void Connect()
	{
		dbcon = new SqliteConnection();
		dbcon.ConnectionString = connectionString;
		dbcmd = dbcon.CreateCommand();
	}

	public static void CreateFile()
	{
		Log.WriteLine("creating file...");
		Log.WriteLine(connectionString);
		
		if(!Directory.Exists(home)) {
			Directory.CreateDirectory (home);
		}
		
		dbcon.Open();
		dbcon.Close();
	}

	public static bool CheckTables()
	{
		return (File.Exists(sqlFile));

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
			return false;
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
		double toReach = Convert.ToDouble(lastChronojumpDatabaseVersion);
		return currentVersion + "/" + toReach.ToString() + " " +
			conversionRate.ToString() + "/" + conversionRateTotal.ToString() + " " +
			conversionSubRate.ToString() + "/" + conversionSubRateTotal.ToString() + " ";
	}

	//for splashWin progressbars
	public static double PrintCreation() {
		return Util.DivideSafe(creationRate, creationTotal);
	}
	public static double PrintConversionVersion() {
		return Util.DivideSafe(Convert.ToDouble(currentVersion), Convert.ToDouble(lastChronojumpDatabaseVersion));
	}
	public static double PrintConversionRate() {
		return Util.DivideSafe(conversionRate, conversionRateTotal);
	}
	public static double PrintConversionSubRate() {
		return Util.DivideSafe(conversionSubRate, conversionSubRateTotal);
	}

	public static bool ConvertToLastChronojumpDBVersion() {

		//if(checkIfIsSqlite2())
		//	convertSqlite2To3();

		addChronopicPortNameIfNotExists();

		//string currentVersion = SqlitePreferences.Select("databaseVersion");
		currentVersion = SqlitePreferences.Select("databaseVersion");

		//Log.WriteLine("lastDB: {0}", Convert.ToDouble(lastChronojumpDatabaseVersion));
		//Log.WriteLine("currentVersion: {0}", Convert.ToDouble(currentVersion));

		bool returnSoftwareIsNew = true; //-1 if there's software is too old for database (moved db to other computer)
		if(Convert.ToDouble(lastChronojumpDatabaseVersion) == Convert.ToDouble(currentVersion))
			Log.WriteLine("Database is already latest version");
		else if(Convert.ToDouble(lastChronojumpDatabaseVersion) < Convert.ToDouble(currentVersion)) {
			Log.WriteLine("User database newer than program, need to update software");
			returnSoftwareIsNew = false;
		} else {
			Log.WriteLine("Old database, need to convert");
			bool needToConvertPersonToSport = false;

			SqliteJumpRj sqliteJumpRjObject = new SqliteJumpRj();
			SqliteRunInterval sqliteRunIntervalObject = new SqliteRunInterval();
			SqliteReactionTime sqliteReactionTimeObject = new SqliteReactionTime();
			SqlitePulse sqlitePulseObject = new SqlitePulse();

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

				SqliteRunType.RunTypeInsert ("Agility-20Yard:18.28:20Yard Agility test", true);
				SqliteRunType.RunTypeInsert ("Agility-505:10:505 Agility test", true);
				SqliteRunType.RunTypeInsert ("Agility-Illinois:60:Illinois Agility test", true);
				SqliteRunType.RunTypeInsert ("Agility-Shuttle-Run:40:Shuttle Run Agility test", true);
				SqliteRunType.RunTypeInsert ("Agility-ZigZag:17.6:ZigZag Agility test", true);

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
				SqliteRunType.AddGraphLinksRunInterval();	
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

				ArrayList arraySimulated = new ArrayList(1);
				arraySimulated.Add("-1");
				
				ArrayList arrayAngleAndSimulated = new ArrayList(1);
				arrayAngleAndSimulated.Add("-1"); //angle
				arrayAngleAndSimulated.Add("-1"); //simulated

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
				
				Log.WriteLine("Added simulated column to each event table on client. Added race, country, serverUniqueID. Convert to sport related done here if needed");
				currentVersion = "0.57";
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
			if(Util.IsWindows()) 
				SqlitePreferences.Insert ("chronopicPort", Constants.ChronopicDefaultPortWindows);
			else
				SqlitePreferences.Insert ("chronopicPort", Constants.ChronopicDefaultPortLinux);
			dbcon.Close();
			
			Log.WriteLine("Added Chronopic port");
		}
	}
	
	public static void CreateTables()
	{
		dbcon.Open();

		creationTotal = 12;
		creationRate = 1;
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
		SqliteRunType.createTableRunType();
		SqliteRunType.createTableRunIntervalType();
		SqliteRunType.initializeTableRunType();
		SqliteRunType.initializeTableRunIntervalType();
		
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
	
		//sports
		creationRate ++;
		SqliteSport.createTable();
		SqliteSport.initialize();
		SqliteSpeciallity.createTable();
		SqliteSpeciallity.initialize();
		SqliteSpeciallity.InsertUndefined(true);

		creationRate ++;
		SqliteSession.createTable(Constants.SessionTable);
		
		creationRate ++;
		SqlitePersonSession.createTable();
		
		creationRate ++;
		SqlitePreferences.createTable();
		SqlitePreferences.initializeTable(lastChronojumpDatabaseVersion);
		
		creationRate ++;
		SqliteCountry.createTable();
		SqliteCountry.initialize();
		
		//changes [from - to - desc]
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
		Log.WriteLine(string.Format("exists = {0}", exists.ToString()));

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
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		Log.WriteLine(dbcmd.CommandText.ToString());

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

		conversionSubRateTotal = myArray.Count * 2;

		if(tableName == Constants.PersonTable) {	
			foreach (Person myPerson in myArray) {
				myPerson.InsertAtDB(true, Constants.ConvertTempTable);
				conversionSubRate ++;
			}
		} else {
			foreach (Event myEvent in myArray) {
				myEvent.InsertAtDB(true, Constants.ConvertTempTable);
				conversionSubRate ++;
			}
		}

		//3rd drop desired table
		Sqlite.dropTable(tableName);

		//4d create desired table (now with new columns)
		sqliteObject.createTable(tableName);

		//5th insert data in desired table
		if(tableName == Constants.PersonTable) {	
			foreach (Person myPerson in myArray) {
				myPerson.InsertAtDB(true, tableName);
				conversionSubRate ++;
			}
		} else {
			foreach (Event myEvent in myArray) {
				myEvent.InsertAtDB(true, tableName);
				conversionSubRate ++;
			}
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
}
