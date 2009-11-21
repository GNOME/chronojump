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
 * Copyright (C) 2004-2009   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using System.IO;
using System.Collections; //ArrayList
using Mono.Data.Sqlite;


class SqliteRun : Sqlite
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
			"simulated INT )";
		dbcmd.ExecuteNonQuery();
	}
	
	
	/*
	 * Run class methods
	 */
	
	public static int Insert(bool dbconOpened, string tableName, string uniqueID, int personID, int sessionID, string type, double distance, double time, string description, int simulated)
	{
		if(! dbconOpened)
			dbcon.Open();
		
		if(uniqueID == "-1")
			uniqueID = "NULL";

		dbcmd.CommandText = "INSERT INTO " + tableName + 
				" (uniqueID, personID, sessionID, type, distance, time, description, simulated)" +
				" VALUES (" + uniqueID + ", " +
				+ personID + ", " + sessionID + ", '" + type + "', "
				+ Util.ConvertToPoint(distance) + ", " + Util.ConvertToPoint(time) + ", '" + 
				description + "', " + simulated + ")" ;
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		int myLast = dbcon.LastInsertRowId;

		if(! dbconOpened)
			dbcon.Close();

		return myLast;
	}
	
	//if all sessions, put -1 in sessionID
	//if all persons, put -1 in personID
	//if all types, put "" in filterType
	public static string[] SelectRuns(int sessionID, int personID, string filterType) 
	{
		string filterSessionString = "";
		if(sessionID != -1)
			filterSessionString = " AND run.sessionID == " + sessionID;

		string filterPersonString = "";
		if(personID != -1)
			filterPersonString = " AND person.uniqueID == " + personID;

		string filterTypeString = "";
		if(filterType != "")
			filterTypeString = " AND run.type == '" + filterType + "' " ;

		dbcon.Open();
		dbcmd.CommandText = "SELECT person.name, run.* " +
			" FROM person, run " +
			" WHERE person.uniqueID == run.personID" + 
			filterSessionString +
			filterPersonString +
			filterTypeString +
			" ORDER BY upper(person.name), run.uniqueID";
		
		Log.WriteLine(dbcmd.CommandText.ToString());
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
					reader[8].ToString() 		//simulated
					);
			count ++;
		}

		reader.Close();
		dbcon.Close();

		string [] myRuns = new string[count];
		count =0;
		foreach (string line in myArray) {
			myRuns [count++] = line;
		}

		return myRuns;
	}

	public static Run SelectRunData(int uniqueID)
	{
		dbcon.Open();

		dbcmd.CommandText = "SELECT * FROM " + Constants.RunTable + " WHERE uniqueID == " + uniqueID;
		
		Log.WriteLine(dbcmd.CommandText.ToString());

		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		reader.Read();
	
		Run myRun = new Run(DataReaderToStringArray(reader, 8));
	
		dbcon.Close();
		return myRun;
	}
		
	public static void Update(int runID, string type, string distance, string time, int personID, string description)
	{
		dbcon.Open();
		dbcmd.CommandText = "UPDATE " + Constants.RunTable + 
			" SET personID = " + personID + 
			", type = '" + type +
			"', distance = " + Util.ConvertToPoint(Convert.ToDouble(distance)) + 
			", time = " + Util.ConvertToPoint(Convert.ToDouble(time)) + 
			", description = '" + description +
			"' WHERE uniqueID == " + runID ;
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		dbcon.Close();
	}

	public static void Delete(string runTable, string uniqueID)
	{
		dbcon.Open();
		dbcmd.CommandText = "Delete FROM " + runTable + 
			" WHERE uniqueID == " + uniqueID;
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		dbcon.Close();
	}
}

