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
using System.IO;
using System.Collections; //ArrayList
using Mono.Data.SqliteClient;
using System.Data.SqlClient;


class Sqlite
{
	protected static SqliteConnection dbcon;
	protected static SqliteCommand dbcmd;
	
	public static string home = Environment.GetEnvironmentVariable("HOME")+"/.chronojump";
	public static string sqlFile = home + "/chronojump.db";
	
	static string connectionString = "URI=file:" + sqlFile ;

	public static void Connect()
	{
		dbcon = new SqliteConnection();
		dbcon.ConnectionString = connectionString;
		dbcmd = new SqliteCommand();
		dbcmd.Connection = dbcon;
	}

	public static void CreateFile()
	{
		Console.WriteLine("creating file...");
		
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
	
	public static void AddChronopicPortNameIfNotExists() {
		string myPort = SqlitePreferences.Select("chronopicPort");
		if(myPort == "0") {
			//if doesn't exist (for any reason, like old database)
			SqlitePreferences.insert ("chronopicPort", "ttyS0");
			Console.WriteLine("Added Chronopic port");
		}
	}
	
	public static void CreateTables()
	{
		dbcon.Open();

		SqlitePerson.createTable();
	
		//jumps
		SqliteJump.createTable();
		SqliteJump.rjCreateTable();

		//jump Types
		SqliteJumpType.createTableJumpType();
		SqliteJumpType.createTableJumpRjType();
		SqliteJumpType.initializeTableJumpType();
		SqliteJumpType.initializeTableJumpRjType();
		
		//runs
		SqliteRun.createTable();
		SqliteRun.intervalCreateTable();
		
		//run Types
		SqliteRunType.createTableRunType();
		SqliteRunType.createTableRunIntervalType();
		SqliteRunType.initializeTableRunType();
		SqliteRunType.initializeTableRunIntervalType();
		
		SqliteSession.createTable();
		
		SqlitePersonSession.createTable();
		
		SqlitePreferences.createTable();
		
		SqlitePreferences.insert ("databaseVersion", "0.41"); 
		//changes from 0.4 to 0.41: jump, jumpRj weight is double (always a percent)
		
		SqlitePreferences.insert ("chronopicPort", "ttyS0");
		SqlitePreferences.insert ("digitsNumber", "3");
		SqlitePreferences.insert ("showHeight", "True");
		SqlitePreferences.insert ("showInitialSpeed", "True");
		SqlitePreferences.insert ("simulated", "True");
		SqlitePreferences.insert ("weightStatsPercent", "True"); //currently not used
		SqlitePreferences.insert ("askDeletion", "True");
		SqlitePreferences.insert ("heightPreferred", "False");
		SqlitePreferences.insert ("metersSecondsPreferred", "True");

		dbcon.Close();
	}
}
