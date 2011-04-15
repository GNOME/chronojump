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
using System.IO;
using Mono.Data.Sqlite;


class SqlitePreferences : Sqlite
{
	protected internal static void createTable()
	{
		dbcmd.CommandText = 
			"CREATE TABLE " + Constants.PreferencesTable + " ( " +
			"name TEXT, " +
			"value TEXT) ";
		dbcmd.ExecuteNonQuery();
	}
	
	protected internal static void initializeTable(string databaseVersion, bool creatingBlankDatabase)
	{
		Insert ("databaseVersion", databaseVersion); 
			
		if(Util.IsWindows() || creatingBlankDatabase)
			Insert ("chronopicPort", Constants.ChronopicDefaultPortWindows);
		else
			Insert ("chronopicPort", Constants.ChronopicDefaultPortLinux);
		
		Insert ("digitsNumber", "3");
		Insert ("showHeight", "True");
		Insert ("showPower", "True");
		Insert ("showInitialSpeed", "True");
		Insert ("showAngle", "False"); //for treeviewjumps
		Insert ("showQIndex", "False"); //for treeviewJumps
		Insert ("showDjIndex", "False"); //for treeviewJumps
		Insert ("simulated", "True");
		Insert ("weightStatsPercent", "False");
		Insert ("askDeletion", "True");
		Insert ("heightPreferred", "False");
		Insert ("metersSecondsPreferred", "True");
		Insert ("language", "es-ES"); 
		Insert ("allowFinishRjAfterTime", "True"); 
		Insert ("volumeOn", "True"); 
		Insert ("videoOn", "True"); 
		Insert ("evaluatorServerID", "-1");
		Insert ("versionAvailable", "");
		
		Random rnd = new Random();
		string machineID = rnd.Next().ToString();
		Insert ("machineID", machineID);
		
		Insert ("multimediaStorage", Constants.MultimediaStorage.BYSESSION.ToString());
	}

	public static void Insert(string myName, string myValue)
	{
		//dbcon.Open();
		dbcmd.CommandText = "INSERT INTO " + Constants.PreferencesTable + 
			" (name, value) VALUES ('" + 
			myName + "', '" + myValue + "')" ;
		dbcmd.ExecuteNonQuery();
		//dbcon.Close();
	}

	public static void Update(string myName, string myValue, bool dbconOpened)
	{
		if(! dbconOpened)
			dbcon.Open();

		dbcmd.CommandText = "UPDATE " + Constants.PreferencesTable +
			" SET value = '" + myValue + 
			"' WHERE name == '" + myName + "'" ;
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		if(! dbconOpened)
			dbcon.Close();
	}

	public static string Select (string myName) 
	{
		dbcon.Open();
		dbcmd.CommandText = "SELECT value FROM " + Constants.PreferencesTable + 
			" WHERE name == '" + myName + "'" ;
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		//SqliteDataReader reader;
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		string myReturn = "0";
	
		if(reader.Read()) {
			myReturn = reader[0].ToString();
		}
		reader.Close();
		dbcon.Close();

		return myReturn;
	}
}

