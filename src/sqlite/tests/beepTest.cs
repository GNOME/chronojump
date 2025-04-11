//TODO: static sutff like SelectSA could be returned as a List<System.Object> or List<Event>

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
 * Copyright (C) 2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
//using System.Data;
using System.Collections;
using System.Collections.Generic; //List<T>
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

class SqliteBeepTest : SqliteTests
{
	private static string tableStatic = Constants.BeepTestTable;

	public SqliteBeepTest ()
	{
		tableName = Constants.BeepTestTable;
		columnsStr = " (uniqueID, personID, sessionID, exerciseID," +
                                " options, stages, laps, totalMeters, maxSpeed," +
                                " dateTime, comments, videoURL)";
	}

	~SqliteBeepTest () {}

	/*
	 * create and initialize tables
	 */

	protected override void createTable()
	{
		dbcmd.CommandText =
			"CREATE TABLE " + tableName + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"personID INT, " +
			"sessionID INT, " +
			"exerciseID INT, " +
			"options TEXT, " +	//additional options of the selected exercise
			"stages INT, " + 	//stored stage and not stageName, it will be converted to show, export, ...
			"laps INT, " + 		//start by 0, when show/export add 1
			"totalMeters INT, " +
			"maxSpeed FLOAT, " +
                        "datetime TEXT, " +     //2019-07-11_15-01-44
                        "comments TEXT, " +     //can include the warnings
			"videoURL TEXT)";       //URL of video of signals. stored as relative

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}

	protected override string selectSAArray (SQLiteDataReader reader)
	{
		string exerciseStr = "";
		if (Util.IsNumber (reader[4].ToString (), false))
		{
			int exerciseID = Convert.ToInt32 (reader[4].ToString ());
			if (exerciseID < (BeepTestCM.TypesArray ()).Length)
				exerciseStr = (BeepTestCM.TypesArray ())[exerciseID];
		}

		// TODO: need to convert stages to stageName and add +1 to laps
		return
			reader[0].ToString() + ":" + 	//person.name
			reader[1].ToString() + ":" +	//beepTest.uniqueID
			reader[2].ToString() + ":" + 	//beepTest.personID
			reader[3].ToString() + ":" + 	//beepTest.sessionID
			reader[4].ToString() + ":" + 	//beepTest.exerciseID
			exerciseStr 	+ ":" + 	//beepTest.exerciseID -> as str for treeviewResults
			reader[5].ToString() + ":" + 	//beepTest.options
			reader[6].ToString() + ":" + 	//beepTest.stages
			reader[7].ToString() + ":" + 	//beepTest.laps
			reader[8].ToString() + ":" + 	//beepTest.totalMeters
			Util.CDSNoZero (reader[9].ToString()) + ":" + 	//beepTest.maxSpeed
			reader[10].ToString() + ":" + 	//beepTest.dateTime
			reader[11].ToString() + ":" + 	//beepTest.description
			reader[12].ToString();	 	//beepTest.videoURL
	}

	public static BeepTest SelectData (int uniqueID, bool dbconOpened)
	{
		return new BeepTest (selectTestData (uniqueID, dbconOpened, tableStatic, 12));
	}
}
