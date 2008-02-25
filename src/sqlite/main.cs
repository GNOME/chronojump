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
	
	public static string home = Util.GetHomeDir();
	public static string sqlFile = home + Path.DirectorySeparatorChar + "chronojump.db";
	//public static string sqlFile = home + Path.DirectorySeparatorChar + "chronojump_altre_copia.db";
	
	//http://www.mono-project.com/SQLite

	static string connectionString = "version = 3; Data source = " + sqlFile;


	/*
	 * Important, change this if there's any update to database
	 * Important2: if database version get numbers higher than 1, check if the comparisons with myVersion works ok
	 */
	static string lastChronojumpDatabaseVersion = "0.54";


	public static void Connect()
	{
		dbcon = new SqliteConnection();
		dbcon.ConnectionString = connectionString;
		dbcmd = dbcon.CreateCommand();
	}

	public static void CreateFile()
	{
		Console.WriteLine("creating file...");
		Console.WriteLine(connectionString);
		
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
			Console.WriteLine("SQLITE3");
			dbcon.Close();
			return true;
		}
		else if(sqlite2SelectWorks()) {
			Console.WriteLine("SQLITE2");
			dbcon.Close();
			//write sqlFile path on data/databasePath.txt
			//TODO
			//

			return false;
		}
		else {
			Console.WriteLine("ERROR in sqlite detection");
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

		string sqlite2File = Util.GetHomeDir() + Path.DirectorySeparatorChar + "chronojump-sqlite2.81.db";
		string sqliteDB = Util.GetHomeDir() + Path.DirectorySeparatorChar + "chronojump.db";

		File.Copy(sqliteDB, sqlite2File, true);

		string myPath = "";
		string sqliteStr = "";
		string sqlite3Str = "";
		string extension = "";
		try {
			if(Util.IsWindows()) {
				myPath = Constants.UtilProgramsWindows;
				extension = Constants.ExtensionProgramsWindows;
				sqliteStr = "sqlite.exe";
				sqlite3Str = "sqlite3.exe";
			}
			else {
				myPath = Constants.UtilProgramsLinux;
				extension = Constants.ExtensionProgramsLinux;
				sqliteStr = "sqlite-2.8.17.bin";
				sqlite3Str = "sqlite3-3.5.0.bin";
			}

			if(File.Exists(myPath + Path.DirectorySeparatorChar + sqliteStr)) 
				Console.WriteLine("exists1");
			if(File.Exists(sqlite2File)) 
				Console.WriteLine("exists2");

			/*
			Console.WriteLine("{0}-{1}", myPath + Path.DirectorySeparatorChar + sqliteStr , sqlite2File + " .dump");
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
			writer.WriteLine(Util.GetHomeDir());
			((IDisposable)writer).Dispose();
			
			Console.WriteLine("Path written");

			Process p2 = Process.Start(myPath + Path.DirectorySeparatorChar + "convert_database." + extension);
			p2.WaitForExit();

			Console.WriteLine("sqlite3 db created");
				
			File.Copy(myPath + Path.DirectorySeparatorChar + "tmp.db", sqliteDB, true ); //overwrite
		} catch {
			Console.WriteLine("PROBLEMS");
			return false;
		}

		Console.WriteLine("done");
		return true;

	}

	public static bool ConvertToLastChronojumpDBVersion() {

		//if(checkIfIsSqlite2())
		//	convertSqlite2To3();

		addChronopicPortNameIfNotExists();

		string myVersion = SqlitePreferences.Select("databaseVersion");

		//Console.WriteLine("lastDB: {0}", Convert.ToDouble(lastChronojumpDatabaseVersion));
		//Console.WriteLine("myVersion: {0}", Convert.ToDouble(myVersion));

		bool returnSoftwareIsNew = true; //-1 if there's software is too old for database (moved db to other computer)
		if(Convert.ToDouble(lastChronojumpDatabaseVersion) == Convert.ToDouble(myVersion))
			Console.WriteLine("Database is already latest version");
		else if(Convert.ToDouble(lastChronojumpDatabaseVersion) < Convert.ToDouble(myVersion)) {
			Console.WriteLine("User database newer than program, need to update software");
			returnSoftwareIsNew = false;
		} else {
			Console.WriteLine("Old database, need to convert");
			if(myVersion == "0.41") {
				dbcon.Open();

				SqlitePulse.createTable();
				SqlitePulseType.createTablePulseType();
				SqlitePulseType.initializeTablePulseType();

				SqlitePreferences.Update ("databaseVersion", "0.42", true); 
				Console.WriteLine("Converted DB to 0.42 (added pulse and pulseType tables)");

				dbcon.Close();
				myVersion = "0.42";
			}

			if(myVersion == "0.42") {
				dbcon.Open();
				SqlitePulseType.Insert ("Free:-1:-1:free PulseStep mode", true); 
				SqlitePreferences.Insert ("language", "es-ES"); 
				SqlitePreferences.Update ("databaseVersion", "0.43", true); 
				Console.WriteLine("Converted DB to 0.43 (added 'free' pulseType & language peference)");
				dbcon.Close();
				myVersion = "0.43";
			}

			if(myVersion == "0.43") {
				dbcon.Open();
				SqlitePreferences.Insert ("showQIndex", "False"); 
				SqlitePreferences.Insert ("showDjIndex", "False"); 
				SqlitePreferences.Update ("databaseVersion", "0.44", true); 
				Console.WriteLine("Converted DB to 0.44 (added showQIndex, showDjIndex)");
				dbcon.Close();
				myVersion = "0.44";
			}

			if(myVersion == "0.44") {
				dbcon.Open();
				SqlitePreferences.Insert ("allowFinishRjAfterTime", "True"); 
				SqlitePreferences.Update ("databaseVersion", "0.45", true); 
				Console.WriteLine("Converted DB to 0.45 (added allowFinishRjAfterTime)");
				dbcon.Close();
				myVersion = "0.45";
			}

			if(myVersion == "0.45") {
				dbcon.Open();
				SqliteJumpType.JumpTypeInsert ("Free:1:0:Free jump", true); 
				SqlitePreferences.Update ("databaseVersion", "0.46", true); 
				Console.WriteLine("Added Free jump type");
				dbcon.Close();
				myVersion = "0.46";
			}

			if(myVersion == "0.46") {
				dbcon.Open();

				SqliteReactionTime.createTable();

				SqlitePreferences.Update ("databaseVersion", "0.47", true); 
				Console.WriteLine("Added reaction time table");
				dbcon.Close();
				myVersion = "0.47";
			}

			if(myVersion == "0.47") {
				dbcon.Open();

				SqliteJump.rjCreateTable(Constants.TempJumpRjTable);
				SqliteRun.intervalCreateTable(Constants.TempRunIntervalTable);

				SqlitePreferences.Update ("databaseVersion", "0.48", true); 
				Console.WriteLine("created tempJumpReactive and tempRunInterval tables");
				dbcon.Close();
				myVersion = "0.48";
			}

			if(myVersion == "0.48") {
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
				Console.WriteLine("Added graphLinkTable, added Rocket jump and 5 agility tests: (20Yard, 505, Illinois, Shuttle-Run & ZigZag. Added graphs pof the 5 agility tests)");

				dbcon.Close();
				myVersion = "0.49";
			}

			if(myVersion == "0.49") {
				dbcon.Open();
				SqliteJumpType.Update ("SJ+", "SJl"); 
				SqliteJumpType.Update ("CMJ+", "CJl"); 
				SqliteJumpType.Update ("ABK+", "ABKl"); 
				SqliteJump.ChangeWeightToL();
				SqliteJumpType.AddGraphLinks();	
				SqliteJumpType.AddGraphLinksRj();	
				SqlitePreferences.Update ("databaseVersion", "0.50", true); 
				Console.WriteLine("changed SJ+ to SJl, same for CMJ+ and ABK+, added jump and jumpRj graph links");
				dbcon.Close();
				myVersion = "0.50";
			}

			if(myVersion == "0.50") {
				dbcon.Open();
				SqliteRunType.AddGraphLinksRunSimple();	
				SqliteRunType.AddGraphLinksRunInterval();	
				SqlitePreferences.Update ("databaseVersion", "0.51", true); 
				Console.WriteLine("added graphLinks for run simple and interval");
				dbcon.Close();
				myVersion = "0.51";
			}

			if(myVersion == "0.51") {
				dbcon.Open();
				SqliteJumpType.Update ("CJl", "CMJl"); 
				SqliteEvent.GraphLinkInsert (Constants.JumpTable, "CMJl", "jump_cmj_l.png", true);
				SqliteEvent.GraphLinkInsert (Constants.JumpTable, "ABKl", "jump_abk_l.png", true);
				SqlitePreferences.Update ("databaseVersion", "0.52", true); 
				Console.WriteLine("added graphLinks for cmj_l and abk_l, fixed CMJl name");
				dbcon.Close();
				myVersion = "0.52";
			}
			
			if(myVersion == "0.52") {
				dbcon.Open();
				SqlitePersonSession.createTable (); 
				dbcon.Close();
				
				//this needs the dbCon closed
				SqlitePersonSession.moveOldTableToNewTable (); 
				
				dbcon.Open();
				SqlitePreferences.Update ("databaseVersion", "0.53", true); 
				dbcon.Close();
				
				Console.WriteLine("created weightSession table. Moved person weight data to weightSession table for each session that has performed");
				myVersion = "0.53";
			}
			
			if(myVersion == "0.53") {
				dbcon.Open();

				SqliteSport.createTable();
				SqliteSport.initialize();
				SqliteSpeciallity.createTable();
				SqliteSpeciallity.initialize();

				SqlitePerson.convertTableToSportRelated (); 
				
				SqlitePreferences.Update ("databaseVersion", "0.54", true); 
				dbcon.Close();
				
				Console.WriteLine("Created sport tables. Added sport data, speciallity and level of practice to person table");
				myVersion = "0.54";
			}
		}

		//if changes are made here, remember to change also in CreateTables()
		//remember to change also the databaseVersion below
		
		return returnSoftwareIsNew;
	}

	/*
	private static bool checkIfIsSqlite2() {
		//fileExists, but is sqlite 3 or 2
		try {
			//sample select
			string myPort = SqlitePreferences.Select("chronopicPort");
			Console.WriteLine("---------");
		} catch {
			Console.WriteLine("+++++++++");
			return true ;
		}

			Console.WriteLine("999999999");
		dbcon.Open();
		dbcmd.CommandText = "dump;";
		dbcmd.ExecuteNonQuery();
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		if(reader.Read()) 
			Console.WriteLine(reader[0].ToString());
		
		reader.Close();
		dbcon.Close();

			Console.WriteLine("3333333333");
		return false;
	}
	
	private static void convertSqlite2To3() {
	*/
		/*
		SqliteConnection dbcon2 = new SqliteConnection();
		dbcon2.ConnectionString = "version = 2; Data source = " + sqlFile;
		dbcon2.Open();

		if we don't know how to dump
		don't do it here!
		show user a window
		that executes a batch file called
		convert_database.bat
		(this .bat will be created also by the install_bundle, and will use sqlite.exe and sqlite3.exe from the sqlite dir)
		*/
/*
	}
	*/

	private static void addChronopicPortNameIfNotExists() {
		string myPort = SqlitePreferences.Select("chronopicPort");
		if(myPort == "0") {
			//if doesn't exist (for any reason, like old database)
			dbcon.Open();
			if(Util.IsWindows()) 
				SqlitePreferences.Insert ("chronopicPort", "COM1");
			else
				SqlitePreferences.Insert ("chronopicPort", "/dev/ttyS0");
			dbcon.Close();
			
			Console.WriteLine("Added Chronopic port");
		}
	}
	
	public static void CreateTables()
	{
		dbcon.Open();

		SqlitePerson.createTable(Constants.PersonTable);

		//graphLinkTable
		SqliteEvent.createGraphLinkTable();
	
		//jumps
		SqliteJump.createTable();
		SqliteJump.rjCreateTable(Constants.JumpRjTable);
		SqliteJump.rjCreateTable(Constants.TempJumpRjTable);

		//jump Types
		SqliteJumpType.createTableJumpType();
		SqliteJumpType.createTableJumpRjType();
		SqliteJumpType.initializeTableJumpType();
		SqliteJumpType.initializeTableJumpRjType();
		
		//runs
		SqliteRun.createTable();
		SqliteRun.intervalCreateTable(Constants.RunIntervalTable);
		SqliteRun.intervalCreateTable(Constants.TempRunIntervalTable);
		
		//run Types
		SqliteRunType.createTableRunType();
		SqliteRunType.createTableRunIntervalType();
		SqliteRunType.initializeTableRunType();
		SqliteRunType.initializeTableRunIntervalType();
		
		//reactionTimes
		SqliteReactionTime.createTable();
		
		//pulses and pulseTypes
		SqlitePulse.createTable();
		SqlitePulseType.createTablePulseType();
		SqlitePulseType.initializeTablePulseType();
	
		//sports
		SqliteSport.createTable();
		SqliteSport.initialize();
		SqliteSpeciallity.createTable();
		SqliteSpeciallity.initialize();

		SqliteSession.createTable();
		
		SqlitePersonSession.createTable();
		
		SqlitePreferences.createTable();
		SqlitePreferences.initializeTable(lastChronojumpDatabaseVersion);
		
		//changes [from - to - desc]
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
	}

	public static bool Exists(string tableName, string findName)
	{
		dbcon.Open();
		dbcmd.CommandText = "SELECT uniqueID FROM " + tableName + 
			" WHERE LOWER(name) == LOWER('" + findName + "')" ;
		Console.WriteLine(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
	
		bool exists = new bool();
		exists = false;
		
		if (reader.Read()) {
			exists = true;
		}
		Console.WriteLine("exists = {0}", exists.ToString());

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
		Console.WriteLine(dbcmd.CommandText.ToString());
		
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
		Console.WriteLine("exists = {0}", exists.ToString());
		dbcon.Close();

		return exists;
	}

	public static void DeleteTempEvents(string tableName)
	{
		//tableName can be tempJumpRj or tempRunInterval

		dbcon.Open();
		//dbcmd.CommandText = "Delete FROM tempJumpRj";
		dbcmd.CommandText = "Delete FROM " + tableName;
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		dbcon.Close();
	}
}
