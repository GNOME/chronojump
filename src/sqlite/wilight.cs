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

class SqliteWilight : SqliteTests
{
	private static string table = Constants.WilightTable;

	public SqliteWilight() {
	}

	~SqliteWilight() {}

	/*
	 * create and initialize tables
	 */

	protected internal static new void createTable()
	{
		dbcmd.CommandText =
			"CREATE TABLE " + table + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"personID INT, " +
			"sessionID INT, " +
			"exerciseID INT, " + //right now all will be exercise 0, until we have a clear idea of what exercises could be done
			"datetime TEXT, " + 	//2019-07-11_15-01-44
			"videoURL TEXT, " +	//URL of video of signals. stored as relative
			"totalMs INT, " +
			"onString TEXT)";	////8;2035;ON=6;2120;ON=...

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}

	public static int Insert (bool dbconOpened, string insertString)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "INSERT INTO " + table +
				" (uniqueID, personID, sessionID, exerciseID, dateTime, videoURL, totalMs, onString)" +
				" VALUES " + insertString;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		string myString = @"select last_insert_rowid()";
		dbcmd.CommandText = myString;
		int myLast = Convert.ToInt32(dbcmd.ExecuteScalar()); // Need to type-cast since `ExecuteScalar` returns an object.

		closeIfNeeded(dbconOpened);

		return myLast;
	}

        /*
         * sID -1 means all sessions
         * pID -1 means all persons
         * type "" means all types
	 * limit 0 means no limit (limit negative is the last results)
         * personNameInComment is used to be able to display names in graphs
         *   because event.PersonName makes individual SQL SELECTs
         */

	public static List<Wilight> Select (bool dbconOpened, int sessionID, int personID,
			//string type,
			Orders_by order, int limit, bool personNameInComment//, bool onlyBestInSession
			)
	{
		openIfNeeded(dbconOpened);

		//for personNameInComment
		List<Person> person_l =
			SqlitePersonSession.SelectCurrentSessionPersonsAsList (true, sessionID);

		dbcmd.CommandText = selectResultsCreateSelection (
				table,
				sessionID, personID, "", //type,
				order, limit, false //onlyBestInSession
				);
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SQLiteDataReader reader;
		reader = dbcmd.ExecuteReader();
		List<Wilight> wilight_l = new List<Wilight>();

		while(reader.Read())
		{
			// 1. get person name in description if personNameInComment
			int personIDThisRecord = Convert.ToInt32(reader[2].ToString());
			string description = "";

			if (personNameInComment)
				foreach (Person person in person_l)
					if (person.UniqueID == personIDThisRecord)
						description = person.Name;

			// 2. create object
			Wilight wilight = new Wilight(
					Convert.ToInt32(reader[1].ToString()),	//uniqueID
					personIDThisRecord,		 	//personID
					Convert.ToInt32(reader[3].ToString()), 	//sessionID
					Convert.ToInt32(reader[4].ToString()), 	//type
					reader[5].ToString(), 	//datetime
					reader[6].ToString(),	//videoURL
					Convert.ToInt32(reader[7].ToString()),	//totalMs
					reader[8].ToString(),	//onString
					description
					);

			wilight_l.Add (wilight);
		}

		reader.Close();
		closeIfNeeded(dbconOpened);

		//get last values on negative limit
		if (limit < 0 && wilight_l.Count + limit >= 0)
			wilight_l = wilight_l.GetRange (wilight_l.Count + limit, -1 * limit);

		return wilight_l;
	}

	//SA for String Array, used on treeview
	public static string [] SelectSA (bool dbconOpened, int sessionID, int personID,
			//string type,
			Orders_by order, int limit
			//, bool personNameInComment, bool onlyBestInSession
			)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = selectResultsCreateSelection (
				table,
				sessionID, personID, "", //type,
				order, limit, false //onlyBestInSession
				);
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SQLiteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);

		int count = new int();
		count = 0;

		while(reader.Read())
		{
			myArray.Add (
					reader[0].ToString() + ":" + 	//person.name
					reader[1].ToString() + ":" +	//wilight.uniqueID
					reader[2].ToString() + ":" + 	//wilight.personID
					reader[3].ToString() + ":" + 	//wilight.sessionID
					reader[4].ToString() + ":" + 	//wilight.type
					reader[5].ToString() + ":" + 	//datetime
					reader[6].ToString() + ":" +	//videoURL
					reader[7].ToString() + ":" + 	//totalMs
					reader[8].ToString()		//onString
					);

			count ++;
		}

		reader.Close();
		closeIfNeeded(dbconOpened);

		string [] rows = new string[count];
		count =0;
		foreach (string line in myArray) {
			rows [count++] = line;
		}

		return rows;
	}

	public static Wilight SelectData (int uniqueID, bool dbconOpened)
	{
		return new Wilight (selectTestData (uniqueID, dbconOpened, table, 8));
	}

	public static void Update (int uniqueID,
			//string type,	//TODO
			int personID)
	{
		Sqlite.Open();
		dbcmd.CommandText = "UPDATE " + table +
			" SET personID = " + personID +
			//", type = '" + type + //remember to close '
			" WHERE uniqueID = " + uniqueID ;

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		Sqlite.Close();
	}

}

