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
 * Copyright (C) 2022-2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
//using System.Data;
using System.Collections;
using System.Collections.Generic; //List<T>
using System.IO; //DirectoryInfo
#if MICROSOFT_DATA_SQLITE
using Microsoft.Data.Sqlite;
using SQLiteTransaction = Microsoft.Data.Sqlite.SqliteTransaction;
using SQLiteCommand = Microsoft.Data.Sqlite.SqliteCommand;
using SQLiteDataReader = Microsoft.Data.Sqlite.SqliteDataReader;
using SQLiteConnection = Microsoft.Data.Sqlite.SqliteConnection;
#else
using System.Data.SQLite;
using SQLiteTransaction = System.Data.SQLite.SQLiteTransaction;
using SQLiteCommand = System.Data.SQLite.SQLiteCommand;
using SQLiteDataReader = System.Data.SQLite.SQLiteDataReader;
using SQLiteConnection = System.Data.SQLite.SQLiteConnection;
#endif
using System.Text.RegularExpressions; //Regex

class SqliteRunEncoderExercise : Sqlite
{
	private static string table = Constants.RunEncoderExerciseTable;

	public SqliteRunEncoderExercise() {
	}

	~SqliteRunEncoderExercise() {}

	/*
	 * create and initialize tables
	 */

	protected internal static new void createTable()
	{
		dbcmd.CommandText =
			"CREATE TABLE " + table + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"name TEXT, " +
			"description TEXT, " +
			"segmentMeters INT, " + 	//changed to cm in DB 2.33
			"segmentVariableCm TEXT, " + 	//separator is ;
			"isSprint INT NOT NULL DEFAULT 1, " + //bool
			"angleDefault INT NOT NULL DEFAULT 0)"; //0 horiz, -90 vert go down, 90 vert go up. Maybe in the future this could be -180 180 to measure force when person is (down the wall)
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}

	//undefined defaultAngle will be 1000
	//note execution can have a different angle than the default angle
	public static int Insert (bool dbconOpened, string insertString)
	{
		if(! dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "INSERT INTO " + table +
				" (uniqueID, name, description, segmentMeters, segmentVariableCm, isSprint, angleDefault)" +
				" VALUES " + insertString;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		string myString = @"select last_insert_rowid()";
		dbcmd.CommandText = myString;
		int myLast = Convert.ToInt32(dbcmd.ExecuteScalar()); // Need to type-cast since `ExecuteScalar` returns an object.

		if(! dbconOpened)
			Sqlite.Close();

		return myLast;
	}

	//Default exercise for users without exercises (empty database creation or never used raceAnalyzer)
	protected internal static void insertDefault ()
	{
		RunEncoderExercise re = new RunEncoderExercise (-1, "Sprint", "", RunEncoderExercise.SegmentCmDefault, new List<int>(), true, 0);
		re.InsertSQL(true);
	}

	public static void Update (bool dbconOpened, RunEncoderExercise ex)
	{
		if(! dbconOpened)
			Sqlite.Open();

		/*
		   string uniqueIDStr = "NULL";
		   if(ex.UniqueID != -1)
			   uniqueIDStr = ex.UniqueID.ToString();
		   */

		//This fixes crash on converting from 2.32 to 2.33
		//because angleDefault is still not set (it comes on 2.38)
		string angleDefaultStr = "";
		if (Sqlite.CurrentVersionAsDouble >= 2.39)
			angleDefaultStr = ", angleDefault = " + ex.AngleDefault;

		dbcmd.CommandText = "UPDATE " + table + " SET " +
			" name = '" + ex.Name +
			"', description = '" + ex.Description +
			"', segmentMeters = " + ex.SegmentCm + 	//cm since DB 2.33
			", segmentVariableCm = '" + ex.SegmentVariableCmToSQL +
			"', isSprint = " + Util.BoolToInt(ex.IsSprint) +
			angleDefaultStr +
			" WHERE uniqueID = " + ex.UniqueID;

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		if(! dbconOpened)
			Sqlite.Close();
	}

	public static void Delete (bool dbconOpened, int uniqueID)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "DELETE FROM " + table + " WHERE uniqueID = " + uniqueID;

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);
	}


	public static List<RunEncoderExercise> Select (bool dbconOpened, int uniqueID)
	{
		if(! dbconOpened)
			Sqlite.Open();

		string uniqueIDStr = "";
		if(uniqueID != -1)
			uniqueIDStr = " WHERE " + table + ".uniqueID = " + uniqueID;

		dbcmd.CommandText = "SELECT * FROM " + table + uniqueIDStr;

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SQLiteDataReader reader;
		reader = dbcmd.ExecuteReader();

		List<RunEncoderExercise> list = new List<RunEncoderExercise>();
		while(reader.Read())
		{
			//This fixes crash on converting from 2.32 to 2.33
			//because angleDefault is still not set (it comes on 2.38)
			int angleDefault = 0;
			if (Sqlite.CurrentVersionAsDouble >= 2.39)
				angleDefault = Convert.ToInt32(reader[6].ToString());

			RunEncoderExercise ex = new RunEncoderExercise (
					Convert.ToInt32(reader[0].ToString()),	//uniqueID
					reader[1].ToString(),			//name
					reader[2].ToString(),			//description
					Convert.ToInt32(reader[3].ToString()),	//segmentCm (cm since DB 2.33)
					UtilList.SQLStringToListInt(reader[4].ToString(), ";"),	//segmentVariableCm
					Util.IntToBool(Convert.ToInt32(reader[5].ToString())),
					angleDefault
					);
			list.Add(ex);
		}

		reader.Close();
		if(! dbconOpened)
			Sqlite.Close();

		return list;
	}

}
