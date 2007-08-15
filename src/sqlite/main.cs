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
using System.IO; //"File" things
using System.Collections; //ArrayList
//using System.Data.SqlClient;
//using Mono.Data.SqliteClient;
//using System.Data.SQLite;
using Mono.Data.Sqlite;


class Sqlite
{
	protected static SqliteConnection dbcon;
	protected static SqliteCommand dbcmd;
	//protected static IDbConnection dbcon;
	//protected static IDbCommand dbcmd;
	
	//public static string home = Environment.GetEnvironmentVariable("HOME")+"/.chronojump";
	public static string home = Util.GetHomeDir();
	public static string sqlFile = home + Path.DirectorySeparatorChar + "chronojump.db";
	
	//static string connectionString = "URI=file:" + sqlFile ;
	//static string connectionString = "URI=file:" + sqlFile + "; UseUTF16Encoding=True"; //for windows http://sqlite.phxsoftware.com/forums/thread/117.aspx
	//static string connectionString = "Data source = " + sqlFile + "; UseUTF16Encoding=True"; //for windows http://sqlite.phxsoftware.com/forums/thread/117.aspx
	//static string connectionString = "Data source = " + sqlFile + "; UseUTF8Encoding=True"; //for windows http://sqlite.phxsoftware.com/forums/thread/117.aspx

	//http://www.mono-project.com/SQLite

	static string connectionString = "version = 3; Data source = " + sqlFile;
	//static string connectionString = "URI=file:" + sqlFile ;
	//static string connectionString = "URI=file:" + sqlFile + "; UseUTF16Encoding=True"; //for windows http://sqlite.phxsoftware.com/forums/thread/117.aspx


	/*
	 * Important, change this if there's any update to database
	 */
	static string lastDatabaseVersion = "0.52";


	public static void Connect()
	{
		dbcon = new SqliteConnection();
		dbcon.ConnectionString = connectionString;
		//dbcon = (IDbConnection) new SqliteConnection(connectionString);
		//dbcmd = new SqliteCommand();
		//dbcmd.Connection = dbcon;
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
	
	public static void ConvertToLastVersion() {

		addChronopicPortNameIfNotExists();

		string myVersion = SqlitePreferences.Select("databaseVersion");

		if(lastDatabaseVersion != myVersion) {
			if(myVersion == "0.41") {
				dbcon.Open();

				SqlitePulse.createTable();
				SqlitePulseType.createTablePulseType();
				SqlitePulseType.initializeTablePulseType();

				SqlitePreferences.Update ("databaseVersion", "0.42"); 
				Console.WriteLine("Converted DB to 0.42 (added pulse and pulseType tables)");

				dbcon.Close();
				myVersion = "0.42";
			}

			if(myVersion == "0.42") {
				dbcon.Open();
				SqlitePulseType.Insert ("Free:-1:-1:free PulseStep mode", true); 
				SqlitePreferences.Insert ("language", "es-ES"); 
				SqlitePreferences.Update ("databaseVersion", "0.43"); 
				Console.WriteLine("Converted DB to 0.43 (added 'free' pulseType & language peference)");
				dbcon.Close();
				myVersion = "0.43";
			}

			if(myVersion == "0.43") {
				dbcon.Open();
				SqlitePreferences.Insert ("showQIndex", "False"); 
				SqlitePreferences.Insert ("showDjIndex", "False"); 
				SqlitePreferences.Update ("databaseVersion", "0.44"); 
				Console.WriteLine("Converted DB to 0.44 (added showQIndex, showDjIndex)");
				dbcon.Close();
				myVersion = "0.44";
			}

			if(myVersion == "0.44") {
				dbcon.Open();
				SqlitePreferences.Insert ("allowFinishRjAfterTime", "True"); 
				SqlitePreferences.Update ("databaseVersion", "0.45"); 
				Console.WriteLine("Converted DB to 0.45 (added allowFinishRjAfterTime)");
				dbcon.Close();
				myVersion = "0.45";
			}

			if(myVersion == "0.45") {
				dbcon.Open();
				SqliteJumpType.JumpTypeInsert ("Free:1:0:Free jump", true); 
				SqlitePreferences.Update ("databaseVersion", "0.46"); 
				Console.WriteLine("Added Free jump type");
				dbcon.Close();
				myVersion = "0.46";
			}

			if(myVersion == "0.46") {
				dbcon.Open();

				SqliteReactionTime.createTable();

				SqlitePreferences.Update ("databaseVersion", "0.47"); 
				Console.WriteLine("Added reaction time table");
				dbcon.Close();
				myVersion = "0.47";
			}

			if(myVersion == "0.47") {
				dbcon.Open();

				SqliteJump.rjCreateTable("tempJumpRj");
				SqliteRun.intervalCreateTable("tempRunInterval");

				SqlitePreferences.Update ("databaseVersion", "0.48"); 
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

				SqlitePreferences.Update ("databaseVersion", "0.49"); 
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
				SqlitePreferences.Update ("databaseVersion", "0.50"); 
				Console.WriteLine("changed SJ+ to SJl, same for CMJ+ and ABK+, added jump and jumpRj graph links");
				dbcon.Close();
				myVersion = "0.50";
			}

			if(myVersion == "0.50") {
				dbcon.Open();
				SqliteRunType.AddGraphLinksRunSimple();	
				SqliteRunType.AddGraphLinksRunInterval();	
				SqlitePreferences.Update ("databaseVersion", "0.51"); 
				Console.WriteLine("added graphLinks for run simple and interval");
				dbcon.Close();
				myVersion = "0.51";
			}

			if(myVersion == "0.51") {
				dbcon.Open();
				SqliteJumpType.Update ("CJl", "CMJl"); 
				SqliteEvent.GraphLinkInsert ("jump", "CMJl", "jump_cmj_l.png", true);
				SqliteEvent.GraphLinkInsert ("jump", "ABKl", "jump_abk_l.png", true);
				SqlitePreferences.Update ("databaseVersion", "0.52"); 
				Console.WriteLine("added graphLinks for cmj_l and abk_l, fixed CMJl name");
				dbcon.Close();
				myVersion = "0.52";
			}
		}

		//if changes are made here, remember to change also in CreateTables()
		//remember to change also the databaseVersion below
	}
	
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

		SqlitePerson.createTable();

		//graphLinkTable
		SqliteEvent.createGraphLinkTable();
	
		//jumps
Console.WriteLine("A");
		SqliteJump.createTable();
Console.WriteLine("B");
		SqliteJump.rjCreateTable("jumpRj");
Console.WriteLine("C");
		SqliteJump.rjCreateTable("tempJumpRj");
Console.WriteLine("D");

		//jump Types
		SqliteJumpType.createTableJumpType();
		SqliteJumpType.createTableJumpRjType();
		SqliteJumpType.initializeTableJumpType();
		SqliteJumpType.initializeTableJumpRjType();
		
		//runs
		SqliteRun.createTable();
		SqliteRun.intervalCreateTable("runInterval");
		SqliteRun.intervalCreateTable("tempRunInterval");
		
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
	

		SqliteSession.createTable();
		
		SqlitePersonSession.createTable();
		
		SqlitePreferences.createTable();
		SqlitePreferences.initializeTable(lastDatabaseVersion);
		
		//changes [from - to - desc]
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
