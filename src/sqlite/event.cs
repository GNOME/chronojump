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
//using System.Data;
//using System.IO;
//using System.Collections; //ArrayList
//using Mono.Data.SqliteClient;
//using System.Data.SqlClient;
using Mono.Data.Sqlite;
//using System.Data.SQLite;


/* this class has some initializations used for all events */
 
class SqliteEvent : Sqlite
{
	/*
	 * create and initialize tables
	 */

	/* 
	 * in the future this will not exist, and graphs will be in jumpType, runType, ... tables
	 */

	protected internal static void createGraphLinkTable()
	{
		dbcmd.CommandText = 
			"CREATE TABLE graphLinkTable ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"tableName TEXT, " +
			"eventName TEXT, " +	
			"graphFileName TEXT, " +	//all images arew in the same dir
			"other1 TEXT, " +		//reserved for future
			"other2 TEXT )";		//reserved for future
		dbcmd.ExecuteNonQuery();
	}
	
	public static int GraphLinkInsert(string tableName, string eventName, string graphFileName, bool dbconOpened)
	{
		if(! dbconOpened) {
			dbcon.Open();
		}
		dbcmd.CommandText = "INSERT INTO graphLinkTable" + 
				"(uniqueID, tableName, eventName, graphFileName, other1, other2)" +
				" VALUES (NULL, '" + tableName + "', '" + eventName + "', '" + graphFileName + "', '', '')" ;
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		int myLast = dbcon.LastInsertRowId;
		if(! dbconOpened) {
			dbcon.Close();
		}

		return myLast;
	}
	
	public static string GraphLinkSelectFileName(string tableName, string eventName)
	{
		dbcon.Open();

		dbcmd.CommandText = "SELECT graphFileName FROM graphLinkTable WHERE tableName == '" + tableName + "' AND eventName =='" + eventName + "'";
		
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		string returnString = "";	
		while(reader.Read()) {
			returnString = reader[0].ToString();
		}
	
		dbcon.Close();
		return returnString;
	}
		
	//useful for passing serverUniqueID as simulated int
	//updating local test when it gets uploaded
	public static void UpdateSimulate(bool dbconOpened, string tableName, int uniqueID, int simulated)
	{
		if(!dbconOpened)
			dbcon.Open();

		dbcmd.CommandText = "UPDATE " + tableName + " SET simulated = " + simulated + 
			" WHERE uniqueID == " + uniqueID ;
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		if(!dbconOpened)
			dbcon.Close();
	}

	//convertSimulate and simulateConvertToNegative as a part of db conversion to 0.60
	//0.59 - 0.60 (...) Simulated now are -1, because 0 is real and positive is serverUniqueID
	private static void convertSimulate(string tableName)
	{
		dbcmd.CommandText = "UPDATE " + tableName + " SET simulated = -1" + 
			" WHERE simulated == 1";
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}
	public static void SimulatedConvertToNegative() 
	{
		convertSimulate(Constants.JumpTable);
		convertSimulate(Constants.JumpRjTable);
		convertSimulate(Constants.RunTable);
		convertSimulate(Constants.RunIntervalTable);
		convertSimulate(Constants.PulseTable);
		convertSimulate(Constants.ReactionTimeTable);

		//also as caution:
		convertSimulate(Constants.TempJumpRjTable);
		convertSimulate(Constants.TempRunIntervalTable);
	}
}
