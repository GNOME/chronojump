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
 * Copyright (C) 2020   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
//using System.Data;
using System.Collections;
using System.Collections.Generic; //List<T>
using Mono.Data.Sqlite;

class SqliteTagSession : Sqlite
{
	private static string table = Constants.TagSessionTable;

	public SqliteTagSession() {
	}
	
	~SqliteTagSession() {}
	
	/*
	 * create and initialize tables
	 */
	
	
	protected internal static void createTable ()
	{
		dbcmd.CommandText = 
			"CREATE TABLE " + table + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"name TEXT, " +
			"color TEXT, " +
			"comments TEXT )";
		dbcmd.ExecuteNonQuery();
	}

	public static int Insert (bool dbconOpened, string insertString)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "INSERT INTO " + table +
				" (uniqueID, name, color, comments)" +
				" VALUES " + insertString;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		string myString = @"select last_insert_rowid()";
		dbcmd.CommandText = myString;
		int myLast = Convert.ToInt32(dbcmd.ExecuteScalar()); // Need to type-cast since `ExecuteScalar` returns an object.

		closeIfNeeded(dbconOpened);

		return myLast;
	}

	public static ArrayList Select (bool dbconOpened, int uniqueID)
	{
		openIfNeeded(dbconOpened);

		string selectStr = "SELECT * FROM " + table;

		string uniqueIDStr = "";
		if(uniqueID != -1)
			uniqueIDStr = " WHERE " + table + ".uniqueID = " + uniqueID;

		dbcmd.CommandText = selectStr + uniqueIDStr + " Order BY " + table + ".uniqueID";

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList array = new ArrayList(1);
		TagSession tagS;

		while(reader.Read()) {
			tagS = new TagSession (
					Convert.ToInt32(reader[0].ToString()),	//uniqueID
					reader[1].ToString(),			//name
					reader[2].ToString(),			//color
					reader[3].ToString()			//comments
					);
			array.Add(tagS);
		}

		reader.Close();
		closeIfNeeded(dbconOpened);

		return array;
	}

}

class SqliteSessionTagSession : Sqlite
{
	private static string table = Constants.SessionTagSessionTable;

	public SqliteSessionTagSession() {
	}
	
	~SqliteSessionTagSession() {}
	
	/*
	 * create and initialize tables
	 */
	
	
	protected internal static void createTable ()
	{
		dbcmd.CommandText = 
			"CREATE TABLE " + table + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"sessionID INT, " +
			"tagSessionID INT )";
		dbcmd.ExecuteNonQuery();
	}
}
