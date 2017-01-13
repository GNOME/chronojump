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
 * Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using System.IO;
using System.Collections; //ArrayList
using Mono.Data.Sqlite;


class SqliteReactionTime : Sqlite
{
	public SqliteReactionTime() {
	}
	
	~SqliteReactionTime() {}

	/*
	 * create and initialize tables
	 */
	
	protected override void createTable(string tableName)
	{
		dbcmd.CommandText = 
			"CREATE TABLE " + tableName + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"personID INT, " +
			"sessionID INT, " +
			"type TEXT, " +
			"time FLOAT, " +
			"description TEXT, " +
			"simulated INT )";		
		dbcmd.ExecuteNonQuery();
	}
	
	
	/*
	 * ReactionTime class methods
	 */
	
	public static int Insert(bool dbconOpened, string tableName, string uniqueID, int personID, int sessionID, string type, double time, string description, int simulated)
	{
		if(! dbconOpened)
			Sqlite.Open();

		if(uniqueID == "-1")
			uniqueID = "NULL";

		dbcmd.CommandText = "INSERT INTO " + tableName +  
				" (uniqueID, personID, sessionID, type, time, description, simulated)" +
				" VALUES (" + uniqueID + ", "
				+ personID + ", " + sessionID + ", \"" + type + "\", "
				+ Util.ConvertToPoint(time) + ", \"" + description + "\", " + simulated + ")" ;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		//int myLast = dbcon.LastInsertRowId;
		//http://stackoverflow.com/questions/4341178/getting-the-last-insert-id-with-sqlite-net-in-c
		string myString = @"select last_insert_rowid()";
		dbcmd.CommandText = myString;
		int myLast = Convert.ToInt32(dbcmd.ExecuteScalar()); // Need to type-cast since `ExecuteScalar` returns an object.
		
		if(! dbconOpened)
			Sqlite.Close();

		return myLast;
	}

	//if all persons, put -1 in personID
	//if all types put, "" in filterType
	public static string[] SelectReactionTimes(bool dbconOpened, int sessionID, int personID, string filterType,
			Orders_by order, int limit) 
	{
		if(!dbconOpened)
			Sqlite.Open();

		string tp = Constants.PersonTable;

		string filterPersonString = "";
		if(personID != -1)
			filterPersonString = " AND " + tp + ".uniqueID = " + personID;
		
		string filterTypeString = "";
		if(filterType != "")
			filterTypeString = " AND reactionTime.type == \"" + filterType + "\" ";
		
		string orderByString = " ORDER BY upper(" + tp + ".name), reactionTime.uniqueID";
		if(order == Orders_by.ID_DESC)
			orderByString = " ORDER BY reactionTime.uniqueID DESC ";
		
		string limitString = "";
		if(limit != -1)
			limitString = " LIMIT " + limit;


		dbcmd.CommandText = "SELECT " + tp + ".name, reactionTime.* " +
			" FROM " + tp + ", reactionTime " +
			" WHERE " + tp + ".uniqueID = reactionTime.personID" + 
			" AND reactionTime.sessionID = " + sessionID + 
			filterPersonString +
			filterTypeString +
			orderByString +
			limitString;
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);

		int count = new int();
		count = 0;

		while(reader.Read()) {
			myArray.Add (reader[0].ToString() + ":" +	//person.name
					reader[1].ToString() + ":" +	//uniqueID
					reader[2].ToString() + ":" + 	//personID
					reader[3].ToString() + ":" + 	//sessionID
					reader[4].ToString() + ":" + 	//type
					Util.ChangeDecimalSeparator(reader[5].ToString()) + ":" + 	//time
					reader[6].ToString() + ":" + 	//description
					reader[7].ToString()		//simulated
					);
			count ++;
		}

		reader.Close();
		
		if(!dbconOpened)
			Sqlite.Close();

		string [] myEvents = new string[count];
		count =0;
		foreach (string line in myArray) {
			myEvents [count++] = line;
		}

		return myEvents;
	}

	public static ReactionTime SelectReactionTimeData(int uniqueID, bool dbconOpened)
	{
		if(!dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "SELECT * FROM " + Constants.ReactionTimeTable + " WHERE uniqueID == " + uniqueID;
		
		LogB.SQL(dbcmd.CommandText.ToString());

		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		reader.Read();
		
		ReactionTime myRT = new ReactionTime(DataReaderToStringArray(reader, 7));
	
		reader.Close();
		if(!dbconOpened)
			Sqlite.Close();
		return myRT;
	}
		
	public static void Update(int eventID, string type, string time, int personID, string description)
	{
		Sqlite.Open();
		dbcmd.CommandText = "UPDATE " + Constants.ReactionTimeTable + " SET personID = " + personID + 
			", type = \"" + type +
			"\", time = " + Util.ConvertToPoint(time) +
			", description = \"" + description +
			"\" WHERE uniqueID == " + eventID ;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		Sqlite.Close();
	}

}
