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
 * Copyright (C) 2026   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Data;
using System.IO;
using System.Collections.Generic;

#if MICROSOFT_DATA_SQLITE
using Microsoft.Data.Sqlite;
using SQLiteTransaction = Microsoft.Data.Sqlite.SqliteTransaction;
using SQLiteCommand = Microsoft.Data.Sqlite.SqliteCommand;
using SQLiteDataReader = Microsoft.Data.Sqlite.SqliteDataReader;
using SQLiteConnection = Microsoft.Data.Sqlite.SqliteConnection;
using SQLiteParameter = Microsoft.Data.Sqlite.SqliteParameter;
#else
using System.Data.SQLite;
using SQLiteTransaction = System.Data.SQLite.SQLiteTransaction;
using SQLiteCommand = System.Data.SQLite.SQLiteCommand;
using SQLiteDataReader = System.Data.SQLite.SQLiteDataReader;
using SQLiteConnection = System.Data.SQLite.SQLiteConnection;
using SQLiteParameter = System.Data.SQLite.SQLiteParameter;
#endif


class SqliteBest : Sqlite
{
	// constructor
	public SqliteBest ()
	{
	}

	// in order to use or not historical values, first check if there are tests of this person, exercise on other sessions than sessionID
	public bool HaveEventsInOtherSessions (bool dbconOpened, int sessionID, int personID,
			string table, string type, int exerciseID, string exerciseTable)
	{
		openIfNeeded (dbconOpened); // ----->

		string connector = " WHERE "; //WHERE or AND

		string sessionIDString = "";
		if (sessionID != -1)
		{
			sessionIDString = connector + "sessionID != " + sessionID; // note we search for values NOT in sessionID
			connector = " AND ";
		}

		string personIDString = "";
		if (personID != -1)
		{
			personIDString = connector + "personID = " + personID;
			connector = " AND ";
		}

		string typeString = "";
		string tableExercise = "";
		if (type != "")
		{
			typeString = connector + "type = '" + type + "'";
			connector = " AND ";
		} else if (exerciseID >= 0 && exerciseTable != "")
		{
			typeString = connector + "exerciseID = " + exerciseID +
				" AND " + table + ".exerciseID = " + exerciseTable + ".uniqueID";
			tableExercise = ", " + exerciseTable;
			connector = " AND ";
		}

		dbcmd.CommandText = "SELECT COUNT (*) FROM " + table + tableExercise +
			sessionIDString + personIDString + typeString;

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SQLiteDataReader reader;
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

		closeIfNeeded (dbconOpened); // <-----

		return (exists > 0);
	}

	public SqliteStruct.DateTypeResult Select_MAX_EventsOfAType (bool dbconOpened, int sessionID, int personID,
			string table, string type, int exerciseID, string exerciseTable,
			string valueToSelect)
	{
		return selectEventsOfAType (dbconOpened, sessionID, personID,
				table, type, exerciseID, exerciseTable,
				"MAX", valueToSelect);
	}

	private SqliteStruct.DateTypeResult selectEventsOfAType (bool dbconOpened, int sessionID, int personID,
			string table, string type, int exerciseID, string exerciseTable,
			string stat, string valueToSelect)
	{
		if (! dbconOpened)
			Sqlite.Open();

		string connector = " WHERE "; //WHERE or AND

		string sessionIDString = "";
		if (sessionID != -1)
		{
			sessionIDString = connector + "sessionID = " + sessionID;
			connector = " AND ";
		}

		string personIDString = "";
		if (personID != -1)
		{
			personIDString = connector + "personID = " + personID;
			connector = " AND ";
		}

		string selectString = "";
		string typeString = "";
		string tableExercise = "";
		if (type != "")
		{
			selectString = string.Format ("{0}({1}), datetime, type", stat, valueToSelect);
			typeString = connector + "type = '" + type + "'";
			connector = " AND ";
		}
		else if (exerciseID >= 0 && exerciseTable != "")
		{
			selectString = string.Format ("{0}({1}), datetime, {2}.name", stat, valueToSelect, exerciseTable);
			typeString = connector + "exerciseID = " + exerciseID +
				" AND " + table + ".exerciseID = " + exerciseTable + ".uniqueID";
			tableExercise = ", " + exerciseTable;
			connector = " AND ";
		}

		dbcmd.CommandText = "SELECT " + selectString + " FROM " + table + tableExercise +
			sessionIDString + personIDString + typeString;

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SQLiteDataReader reader;
		reader = dbcmd.ExecuteReader();

		SqliteStruct.DateTypeResult dtr = SqliteStruct.DateTypeResult.Init ();
		if (reader.Read() && Util.IsNumber (Util.CDSNoZero (reader[0].ToString ()), true))
			dtr = new SqliteStruct.DateTypeResult (
					reader[1].ToString (),
					reader[2].ToString (),
					Convert.ToDouble (Util.CDSNoZero (reader[0].ToString ())));

		reader.Close();

		if (! dbconOpened)
			Sqlite.Close();

		return dtr;
	}
}
