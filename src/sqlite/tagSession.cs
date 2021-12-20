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
	
	
	protected internal static new void createTable ()
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
				" VALUES (" + insertString + ")";
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

		dbcmd.CommandText = selectStr + uniqueIDStr + " Order BY LOWER(" + table + ".name)";

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

	//deletes the tag and also all related rows on sessionTagSession
	public static void Delete (bool dbconOpened, int uniqueID)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "DELETE FROM " + table +
				" WHERE uniqueID = " + uniqueID.ToString();
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		dbcmd.CommandText = "DELETE FROM " + Constants.SessionTagSessionTable +
				" WHERE tagSessionID = " + uniqueID.ToString();
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);
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
	
	
	protected internal static new void createTable ()
	{
		dbcmd.CommandText = 
			"CREATE TABLE " + table + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"sessionID INT, " +
			"tagSessionID INT )";
		dbcmd.ExecuteNonQuery();
	}

	//default insertion
	public static void Insert (bool dbconOpened, int sessionID, int tagSessionID)
	{
		Insert (dbconOpened, sessionID, tagSessionID, dbcmd);
	}
	//the method, can be called passing an special SqliteCommand to perform transactions
	public static void Insert (bool dbconOpened, int sessionID, int tagSessionID, SqliteCommand mycmd)
	{
		openIfNeeded(dbconOpened);

		mycmd.CommandText = "INSERT INTO " + table +
				" (uniqueID, sessionID, tagSessionID)" +
				" VALUES (NULL, " +
				sessionID.ToString() + ", " +
				tagSessionID.ToString() + ")";

		LogB.SQL(mycmd.CommandText.ToString());
		mycmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);
	}

	public static void Delete (bool dbconOpened, int sessionID, int tagSessionID)
	{
		Delete (dbconOpened, sessionID, tagSessionID, dbcmd);
	}
	//the method, can be called passing an special SqliteCommand to perform transactions
	public static void Delete (bool dbconOpened, int sessionID, int tagSessionID, SqliteCommand mycmd)
	{
		openIfNeeded(dbconOpened);

		mycmd.CommandText = "DELETE FROM " + table +
				" WHERE sessionID = " + sessionID +
				" AND tagSessionID = " + tagSessionID;

		LogB.SQL(mycmd.CommandText.ToString());
		mycmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);
	}

	//gets the active tagSessions in session
	public static List<TagSession> SelectTagsOfASession (bool dbconOpened, int sessionID)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "SELECT tagSession.* FROM tagSession, sessionTagSession " +
			"WHERE tagSession.uniqueID = sessionTagSession.tagSessionID AND " +
			"sessionTagSession.sessionID = " + sessionID + " ORDER BY LOWER(NAME)";

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		List<TagSession> list = new List<TagSession>();

		while(reader.Read()) {
			TagSession tagS = new TagSession (
					Convert.ToInt32(reader[0].ToString()),	//uniqueID
					reader[1].ToString(),			//name
					reader[2].ToString(),			//color
					reader[3].ToString()			//comments
					);
			list.Add(tagS);
		}

		reader.Close();
		closeIfNeeded(dbconOpened);

		return list;
	}

	public static List<SessionTagSession> SelectTagsOfAllSessions (bool dbconOpened)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "SELECT sessionTagSession.sessionID, tagSession.* FROM tagSession, sessionTagSession " +
			"WHERE tagSession.uniqueID = sessionTagSession.tagSessionID ORDER BY NAME";

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		List<SessionTagSession> list = new List<SessionTagSession>();

		while(reader.Read())
		{
			SessionTagSession sts = new SessionTagSession
				(
					Convert.ToInt32(reader[0].ToString()),	//sessionID
					new TagSession (
						Convert.ToInt32(reader[1].ToString()),	//uniqueID
						reader[2].ToString(),			//name
						reader[3].ToString(),			//color
						reader[4].ToString()			//comments
						)
					);
			list.Add(sts);
		}

		reader.Close();
		closeIfNeeded(dbconOpened);

		return list;
	}

	public static void UpdateTransaction(int sessionID, ArrayList allTags_list,
			List<TagSession> tagsActiveThisSession_list, string [] checkboxes)
	{
		LogB.SQL("Starting sessionTagSession transaction");
		Sqlite.Open();

		int count = 0;
		using(SqliteTransaction tr = dbcon.BeginTransaction())
		{
			using (SqliteCommand dbcmdTr = dbcon.CreateCommand())
			{
				dbcmdTr.Transaction = tr;

				foreach(TagSession tagSession in allTags_list)
				{
					string statusOld = "inactive";
					foreach(TagSession tagSearchIfActive in tagsActiveThisSession_list)
						if(tagSearchIfActive.UniqueID == tagSession.UniqueID)
						{
							statusOld = "active";
							break;
						}

					if(statusOld != checkboxes[count]) {
						if(checkboxes[count] == "active")
							Insert(true, sessionID, tagSession.UniqueID, dbcmdTr);
						else
							Delete(true, sessionID, tagSession.UniqueID, dbcmdTr);
					}

					count ++;
				}
			}
			tr.Commit();
		}

		Sqlite.Close();
		LogB.SQL("Ended transaction");
	}

}
