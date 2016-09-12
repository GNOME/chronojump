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
 * Copyright (C) 2004-2016   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using System.IO;
using System.Collections; //ArrayList
using Mono.Data.Sqlite;


public class SqliteRun : Sqlite
{
	public SqliteRun() {
	}
	
	~SqliteRun() {}

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
			"distance FLOAT, " +
			"time FLOAT, " +
			"description TEXT, " +
			"simulated INT, " +
			"initialSpeed INT )";
		dbcmd.ExecuteNonQuery();
	}
	
	
	/*
	 * Run class methods
	 */
	
	public int Insert(bool dbconOpened, string tableName, string uniqueID, int personID, int sessionID, string type, double distance, double time, string description, int simulated, bool initialSpeed)
	{
		if(! dbconOpened)
			SqliteGeneral.Sqlite.Open();
		
		if(uniqueID == "-1")
			uniqueID = "NULL";

		dbcmd.CommandText = "INSERT INTO " + tableName + 
				" (uniqueID, personID, sessionID, type, distance, time, description, simulated, initialSpeed)" +
				" VALUES (" + uniqueID + ", " +
				+ personID + ", " + sessionID + ", \"" + type + "\", "
				+ Util.ConvertToPoint(distance) + ", " + Util.ConvertToPoint(time) + ", \"" + 
				description + "\", " + simulated + ", " + Util.BoolToInt(initialSpeed) + ")" ;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		//int myLast = dbcon.LastInsertRowId;
		//http://stackoverflow.com/questions/4341178/getting-the-last-insert-id-with-sqlite-net-in-c
		string myString = @"select last_insert_rowid()";
		dbcmd.CommandText = myString;
		int myLast = Convert.ToInt32(dbcmd.ExecuteScalar()); // Need to type-cast since `ExecuteScalar` returns an object.

		if(! dbconOpened)
			SqliteGeneral.Sqlite.Close();

		return myLast;
	}
	
	//if all sessions, put -1 in sessionID
	//if all persons, put -1 in personID
	//if all types, put "" in filterType
	//unlimited put -1 in limit
	public string[] SelectRuns(bool dbconOpened, int sessionID, int personID, string filterType,
			Orders_by order, int limit) 
	{
		if(!dbconOpened)
			SqliteGeneral.Sqlite.Open();

		string tp = Constants.PersonTable;

		string filterSessionString = "";
		if(sessionID != -1)
			filterSessionString = " AND run.sessionID == " + sessionID;

		string filterPersonString = "";
		if(personID != -1)
			filterPersonString = " AND " + tp + ".uniqueID == " + personID;

		string filterTypeString = "";
		if(filterType != "")
			filterTypeString = " AND run.type == \"" + filterType + "\" " ;

		string orderByString = " ORDER BY upper(" + tp + ".name), run.uniqueID ";
		if(order == Orders_by.ID_DESC)
			orderByString = " ORDER BY run.uniqueID DESC ";
		
		string limitString = "";
		if(limit != -1)
			limitString = " LIMIT " + limit;


		dbcmd.CommandText = "SELECT " + tp + ".name, run.* " +
			" FROM " + tp + ", run " +
			" WHERE " + tp + ".uniqueID == run.personID" + 
			filterSessionString +
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
					reader[1].ToString() + ":" +	//run.uniqueID
					reader[2].ToString() + ":" + 	//run.personID
					reader[3].ToString() + ":" + 	//run.sessionID
					reader[4].ToString() + ":" + 	//run.type
					Util.ChangeDecimalSeparator(reader[5].ToString()) + ":" + //run.distance
					Util.ChangeDecimalSeparator(reader[6].ToString()) + ":" + //run.time
					reader[7].ToString() + ":" + 	//description
					reader[8].ToString() + ":" +	//simulated
					Util.IntToBool(Convert.ToInt32(reader[9])) //initialSpeed
					);
			count ++;
		}

		reader.Close();
		
		if(!dbconOpened)
			SqliteGeneral.Sqlite.Close();

		string [] myRuns = new string[count];
		count =0;
		foreach (string line in myArray) {
			myRuns [count++] = line;
		}

		return myRuns;
	}

	public Run SelectRunData(int uniqueID, bool dbconOpened)
	{
		if(!dbconOpened)
			SqliteGeneral.Sqlite.Open();

		dbcmd.CommandText = "SELECT * FROM " + Constants.RunTable + " WHERE uniqueID == " + uniqueID;
		
		LogB.SQL(dbcmd.CommandText.ToString());

		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		reader.Read();
	
		Run myRun = new Run(DataReaderToStringArray(reader, 9));
	
		reader.Close();
		if(!dbconOpened)
			SqliteGeneral.Sqlite.Close();
		return myRun;
	}
		
	public string [] SelectTestMaxStuff(int personID, RunType runType) 
	{
		SqliteGeneral.Sqlite.Open();
		
		dbcmd.CommandText = "SELECT session.date, session.name, MAX(distance/time), run.simulated " + 
			" FROM run, session WHERE type = \"" + runType.Name + "\" AND personID = " + personID + 
			" AND run.sessionID = session.uniqueID";
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		reader.Read();
		
		string [] str = DataReaderToStringArray(reader, 4);
		
		reader.Close();
		SqliteGeneral.Sqlite.Close();

		return str;
	}
	
	public void Update(int runID, string type, string distance, string time, int personID, string description)
	{
		SqliteGeneral.Sqlite.Open();
		dbcmd.CommandText = "UPDATE " + Constants.RunTable + 
			" SET personID = " + personID + 
			", type = \"" + type +
			"\", distance = " + Util.ConvertToPoint(Convert.ToDouble(distance)) + 
			", time = " + Util.ConvertToPoint(Convert.ToDouble(time)) + 
			", description = \"" + description +
			"\" WHERE uniqueID == " + runID ;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		SqliteGeneral.Sqlite.Close();
	}

}

