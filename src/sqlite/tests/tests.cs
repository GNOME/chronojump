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
 * Copyright (C) 2004-2026   Xavier de Blas <xaviblas@gmail.com>
 */

using System.Collections; //ArrayList
using System.Collections.Generic; //List<T>
using System.Diagnostics; 	//for launching other process
using System.Text.RegularExpressions; //Match

#if MICROSOFT_DATA_SQLITE
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

class SqliteTests : Sqlite
{
	protected string tableName;
	protected string columnsStr;
	protected static string filterOtherString = "";

	public SqliteTests ()
	{
		tableName = "";
		columnsStr = "";
	}

	public string FilterOtherString {
		set { filterOtherString = value; }
	}

	/*
	 * done in parent
	protected virtual void createTable()
	{
		LogB.Information ("SqliteTests.createTable() nothing done");
	}
	*/

	public int Insert (bool dbconOpened, string insertString)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "INSERT INTO " + tableName + columnsStr + " VALUES " + insertString;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		string myString = @"select last_insert_rowid()";
		dbcmd.CommandText = myString;
		int myLast = Convert.ToInt32(dbcmd.ExecuteScalar()); // Need to type-cast since `ExecuteScalar` returns an object.

		closeIfNeeded(dbconOpened);

		return myLast;
	}

	//SA for String Array, used on treeview
	public string [] SelectSA (bool dbconOpened, int sessionID, int personID,
			//string type,
			bool addExerciseNameInOtherTable, string exerciseTable,
			Orders_by order, int limit
			//, bool personNameInComment, bool onlyBestInSession
			)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = selectResultsCreateSelection (
				"", //selectRow1 default
				tableName,
				sessionID, personID, "", //type,
				addExerciseNameInOtherTable, exerciseTable,
				order, "", limit, false //onlyBestInSession
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
			myArray.Add (selectSAArray (reader));
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

	protected virtual string selectSAArray (SQLiteDataReader reader)
	{
		return "";
	}

	//note this is selecting also the person.name
	//used on run, runI, wilight
	// limit 0 means no limit (limit negative is the last results) (used on SelectRuns)
	protected static string selectResultsCreateSelection (
			string selectVar, // if selectVar it just selects this variable. If not, it will select tp.name, t.*
			string t,
			int sessionID, int personID, string filterType,
			bool addExerciseNameInOtherTable, string exerciseTable,
			Orders_by order, string orderByBestStr, int limit, bool onlyBestInSession)
	{
		string tp = Constants.PersonTable;

		string selectRow1 = string.Format ("SELECT {0}.name, {1}.* ", tp, t);
		if (selectVar != "")
			selectRow1 = string.Format ("SELECT {0}.{1} ", t, selectVar);

		string filterSessionString = "";
		if(sessionID != -1)
			filterSessionString = string.Format(" AND {0}.sessionID = {1}", t, sessionID);

		string filterPersonString = "";
		if(personID != -1)
			filterPersonString = string.Format(" AND {0}.uniqueID = {1}", tp, personID);

		string filterTypeString = "";
		if(filterType != "")
			filterTypeString = " AND " + t + ".type = '" + filterType + "' " ;

		string selectExerciseNameStr = "";
		string fromExerciseStr = "";
		string andExerciseStr = "";
		if (addExerciseNameInOtherTable && exerciseTable != "")
		{
			selectExerciseNameStr = string.Format (", {0}.name ", exerciseTable);
			fromExerciseStr = string.Format (", {0} ", exerciseTable);
			andExerciseStr = string.Format (" AND {0}.exerciseID = {1}.uniqueID ", t, exerciseTable);
		}

		//LogB.Information ("At selectRunsCreateSelection order: " + order.ToString ());
		string orderByString = string.Format(" ORDER BY upper({0}.name), {1}.uniqueID ", tp, t);
		if(order == Orders_by.ID_ASC)
			orderByString = string.Format(" ORDER BY {0}.uniqueID ", t);
		else if(order == Orders_by.ID_DESC)
			orderByString = string.Format(" ORDER BY {0}.uniqueID DESC ", t);
		if(onlyBestInSession)
			orderByString = string.Format(" ORDER BY {0}.sessionID, {0}.distance/{0}.time DESC ", t);
		if(order == Orders_by.BEST)
			orderByString = orderByBestStr;
		//LogB.Information ("At selectRunsCreateSelection orderByString: " + orderByString);

		string limitString = "";
		if(limit > 0)
			limitString = " LIMIT " + limit;

		return selectRow1 +
			selectExerciseNameStr +
			string.Format(" FROM {0}, {1} ", tp, t) +
			fromExerciseStr +
			string.Format(" WHERE {0}.uniqueID = {1}.personID", tp, t) +
			andExerciseStr +
			filterSessionString +
			filterPersonString +
			filterTypeString +
			filterOtherString +
			orderByString +
			limitString;
	}

	protected static string [] selectTestData (int uniqueID, bool dbconOpened, string tablename, int columns)
	{
		if(!dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "SELECT * FROM " + tablename + " WHERE uniqueID = " + uniqueID;

		LogB.SQL(dbcmd.CommandText.ToString());

		dbcmd.ExecuteNonQuery();

		SQLiteDataReader reader;
		reader = dbcmd.ExecuteReader();

		string [] testData = null;
		if (reader.Read())
		{
			try {
				testData = DataReaderToStringArray (reader, columns);
			} catch {
				LogB.Information ("catched at selectTestData ()");
			}
		}

		reader.Close();
		if(!dbconOpened)
			Sqlite.Close();

		return testData;
	}

	// used on treeview person (n), shows tests of each person
	public static List<IntInt> SessionTestsByPerson (bool dbconOpened, int sessionID, Constants.Modes mode)
	{
		List<IntInt> ii_l = new List<IntInt> ();
		openIfNeeded (dbconOpened); // ---->

		// mode specific ->
		string modeSpecificStr = "";
		if (mode == Constants.Modes.POWERGRAVITATORY)
			modeSpecificStr = " AND signalOrCurve = 'signal' AND hasInertia = 0 "; // hasInertia field since DB 2.63
		else if (mode == Constants.Modes.POWERINERTIAL)
			modeSpecificStr = " AND signalOrCurve = 'signal' AND hasInertia = 1 ";
		else if (mode == Constants.Modes.FORCESENSORISOMETRIC)
			modeSpecificStr = " AND stiffness < 0 "; // isometric has stiffness < 0
		else if (mode == Constants.Modes.FORCESENSORELASTIC)
			modeSpecificStr = " AND stiffness > 0 "; // elastic has stiffness > 0
		// <- mode specific

		dbcmd.CommandText =
			"SELECT personID, COUNT(*) FROM " + Constants.ModeTable (mode) +
			" WHERE sessionID = " + sessionID +
			modeSpecificStr +
			" GROUP BY personID";

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SQLiteDataReader reader; // -->
		reader = dbcmd.ExecuteReader();

		while (reader.Read())
			ii_l.Add (new IntInt (
						Convert.ToInt32 (reader[0].ToString()),
						Convert.ToInt32 (reader[1].ToString ())
					     ));

		reader.Close(); // <--

		closeIfNeeded (dbconOpened); // <----
		return ii_l;
	}

	public static string SelectExerciseNameInOtherTable (bool dbconOpened, int exerciseID, string exerciseTable)
	{
		openIfNeeded (dbconOpened);

		dbcmd.CommandText = "SELECT name FROM " + exerciseTable + " WHERE uniqueID = " + exerciseID;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SQLiteDataReader reader;
		reader = dbcmd.ExecuteReader();
		reader.Read();
                string name = reader[0].ToString();
		reader.Close();

		closeIfNeeded (dbconOpened);

		return name;
	}

	protected static string [] DataReaderToStringArray (SQLiteDataReader reader, int columns)
	{
		string [] myReaderStr = new String[columns];
		for (int i=0; i < columns; i ++)
			myReaderStr[i] = reader[i].ToString();
		return myReaderStr;
	}

	// tests with exerciseID as an int
	// if do not update exerciseID (depending on mode) pass -1
	// comments are always updated even if "" because user can supress them from edit
	public void UpdateFromEdit (int uniqueID, int personID, int exerciseID, string comments)
	{
		string exerciseStr = "";
		if (exerciseID >= 0)
			exerciseStr = ", exerciseID = " + exerciseID + "";

		// use field "comments" or "description"
		// description is used on encoder, jump, jumpRj, run, runInterval
		// but only encoder uses this method. In the future have a commentsFieldName that can be comments or description and use that
		string commentsStr = " , comments = '" + comments + "'";
		if (tableName == Constants.EncoderTable)
			commentsStr = " , description = '" + comments + "'";
		if (tableName == Constants.WilightTable) // wilight has no comments
			commentsStr = "";

		Sqlite.Open();
		dbcmd.CommandText = "UPDATE " + tableName +
			" SET personID = " + personID +
			exerciseStr +
			commentsStr +
			" WHERE uniqueID = " + uniqueID ;

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		// Thought for being used on encoder to update related curves. But is not used because encoder is very different and is not using UpdateFromEdit. It is using UpdateFromEditEncoder that just changes personID
		// updateSpecific (uniqueID, personID);

		Sqlite.Close();
	}
	/*
	 * implement this if needed for modes that do not use exerciseID and use exerciseName (or type), take care with translations
	public void UpdateFromEdit (int uniqueID, int personID, string exerciseName)
	{
	}
	*/

	// just to change the person on encoder, because the other params are very different
	public void UpdateFromEditEncoder (int uniqueID, int personID)
	{
		updateSpecific (uniqueID, personID);
	}

	protected virtual void updateSpecific (int uniqueID, int personID)
	{
	}

	public static void UpdateTestPersonID (bool dbconOpened, string tName, int personIDold, int personIDnew)
	{
		openIfNeeded (dbconOpened);

		dbcmd.CommandText = "UPDATE " + tName +
			" SET personID = " + personIDnew +
			" WHERE personID = " + personIDold;
		LogB.SQL (dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery ();

		closeIfNeeded (dbconOpened);
	}

	/* 
	 * temp data stuff
	 */
	public static int TempDataExists (string tName)
	{
		//tName can be Constants.TempJumpRjTable or Constants.TempJumpRunIntervalTable
		
		Sqlite.Open();
		dbcmd.CommandText = "SELECT MAX(uniqueID) FROM " + tName;
		LogB.SQL(dbcmd.CommandText.ToString());
		
		//SQLiteDataReader reader;
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
		Sqlite.Close();

		return exists;
	}

	public static void DeleteTempEvents (string tName)
	{
		//tName can be Constants.TempJumpRjTable or Constants.TempJumpRunIntervalTable

		Sqlite.Open();
		dbcmd.CommandText = "DELETE FROM " + tName;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		Sqlite.Close();
	}

}

