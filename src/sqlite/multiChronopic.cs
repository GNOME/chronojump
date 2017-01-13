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


class SqliteMultiChronopic : Sqlite
{
	public SqliteMultiChronopic() {
	}
	
	~SqliteMultiChronopic() {}

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
			"type TEXT, " + //now all as "default", but in the future...
			"cp1StartedIn INT, " +
			"cp2StartedIn INT, " +
			"cp3StartedIn INT, " +
			"cp4StartedIn INT, " +
			"cp1InStr TEXT, " +
			"cp1OutStr TEXT, " +
			"cp2InStr TEXT, " +
			"cp2OutStr TEXT, " +
			"cp3InStr TEXT, " +
			"cp3OutStr TEXT, " +
			"cp4InStr TEXT, " +
			"cp4OutStr TEXT, " +
			"vars TEXT, " + //some vars separated by "=" used by different test types
			"description TEXT, " +
			"simulated INT )";		
		dbcmd.ExecuteNonQuery();
	}
	
	
	/*
	 * multiChronopic class methods
	 */

	public static int Insert(bool dbconOpened, string tableName, string uniqueID, int personID, int sessionID, string type, 
			int cp1StartedIn, int cp2StartedIn, int cp3StartedIn, int cp4StartedIn,
			string cp1InStr, string cp1OutStr,
			string cp2InStr, string cp2OutStr,
			string cp3InStr, string cp3OutStr,
			string cp4InStr, string cp4OutStr,
			string vars, 
			string description, int simulated)
	{
		if(! dbconOpened)
			Sqlite.Open();

		if(uniqueID == "-1")
			uniqueID = "NULL";

		dbcmd.CommandText = "INSERT INTO " + tableName +  
			" (uniqueID, personID, sessionID, type, " +
		       	" cp1StartedIn, cp2StartedIn, cp3StartedIn, cp4StartedIn, " +	
			" cp1InStr, cp1OutStr, cp2InStr, cp2OutStr, cp3InStr, cp3OutStr, cp4InStr, cp4OutStr, " +
			" vars, description, simulated)" +
			" VALUES (" + uniqueID + ", " +
			personID + ", " + sessionID + ", \"" + type + "\", " +
			cp1StartedIn + ", " + cp2StartedIn + ", " +
			cp3StartedIn + ", " + cp4StartedIn + ", \"" +
			cp1InStr + "\", \"" + cp1OutStr + "\", \"" +
			cp2InStr + "\", \"" + cp2OutStr + "\", \"" +
			cp3InStr + "\", \"" + cp3OutStr + "\", \"" +
			cp4InStr + "\", \"" + cp4OutStr + "\", \"" +
			vars + "\", \"" +
			description + "\", " + simulated + ")" ;
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
	public static string[] SelectTests(bool dbconOpened, int sessionID, int personID) 
	{
		if(!dbconOpened)
			Sqlite.Open();

		string tp = Constants.PersonTable;

		string filterPersonString = "";
		if(personID != -1)
			filterPersonString = " AND " + tp + ".uniqueID == " + personID;

		dbcmd.CommandText = "SELECT " + tp + ".name, multiChronopic.* " +
			" FROM " + tp + ", multiChronopic " +
			" WHERE " + tp + ".uniqueID == multiChronopic.personID" + 
			" AND multiChronopic.sessionID == " + sessionID + 
			filterPersonString +
			" ORDER BY upper(" + tp + ".name), multiChronopic.uniqueID";
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);

		int count = new int();
		count = 0;

		
		while(reader.Read()) {
			//Util.ConvertToPointIfNeeded is used because multichronopic data is recorded by mistake as ',' instead of '.' on database
			myArray.Add (reader[0].ToString() + ":" +	//person.name
					reader[1].ToString() + ":" +	//mc.uniqueID
					reader[2].ToString() + ":" + 	//mc.personID
					reader[3].ToString() + ":" + 	//mc.sessionID
					reader[4].ToString() + ":" + 	//mc.type
					reader[5].ToString() + ":" + 	//mc.cp1StartedIn
					reader[6].ToString() + ":" + 	//mc.cp2StartedIn
					reader[7].ToString() + ":" + 	//mc.cp3StartedIn
					reader[8].ToString() + ":" + 	//mc.cp4StartedIn
					Util.ConvertToPointIfNeeded(reader[9].ToString()) + ":" + 	//mc.cp1InStr
					Util.ConvertToPointIfNeeded(reader[10].ToString()) + ":" + 	//mc.cp1OutStr
					Util.ConvertToPointIfNeeded(reader[11].ToString()) + ":" + 	//mc.cp2InStr
					Util.ConvertToPointIfNeeded(reader[12].ToString()) + ":" + 	//mc.cp2OutStr
					Util.ConvertToPointIfNeeded(reader[13].ToString()) + ":" + 	//mc.cp3InStr
					Util.ConvertToPointIfNeeded(reader[14].ToString()) + ":" + 	//mc.cp3OutStr
					Util.ConvertToPointIfNeeded(reader[15].ToString()) + ":" + 	//mc.cp4InStr
					Util.ConvertToPointIfNeeded(reader[16].ToString()) + ":" + 	//mc.cp4OutStr
					reader[17].ToString() + ":" + 	//vars
					reader[18].ToString() + ":" + 	//description
					reader[19].ToString()		//simulated
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

	public static MultiChronopic SelectMultiChronopicData(int uniqueID, bool dbconOpened)
	{
		if(!dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "SELECT * FROM " + Constants.MultiChronopicTable + " WHERE uniqueID == " + uniqueID;
		
		LogB.SQL(dbcmd.CommandText.ToString());

		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		reader.Read();
		
		MultiChronopic mc = new MultiChronopic(DataReaderToStringArray(reader, 19));
	
		reader.Close();
		if(!dbconOpened)
			Sqlite.Close();
		return mc;
	}

	public static int MaxCPs(bool dbconOpened, int sessionID)
	{
		if(!dbconOpened)
			Sqlite.Open();

		int maxCPs = 2;

		dbcmd.CommandText = "SELECT uniqueID FROM " + Constants.MultiChronopicTable + 
			" WHERE (cp3InStr != \"\" OR cp3OutStr != \"\") AND sessionID == " + sessionID;
		LogB.SQL(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		if (reader.Read()) {
			maxCPs = 3;
		}
		reader.Close();

		dbcmd.CommandText = "SELECT uniqueID FROM " + Constants.MultiChronopicTable + 
			" WHERE (cp4InStr != \"\" OR cp4OutStr != \"\") AND sessionID == " + sessionID;
		LogB.SQL(dbcmd.CommandText.ToString());
		
		reader = dbcmd.ExecuteReader();
		if (reader.Read()) {
			maxCPs = 4;
		}
		
		reader.Close();
		
		if(!dbconOpened)
			Sqlite.Close();

		return maxCPs;
	}

	public static void Update(int eventID, int personID, string vars, string description)
	{
		Sqlite.Open();
		dbcmd.CommandText = "UPDATE " + Constants.MultiChronopicTable + " SET personID = " + personID + 
			", vars = \"" + vars + 		//vars is distance on runAnalysis
			"\", description = \"" + description +
			"\" WHERE uniqueID == " + eventID ;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		Sqlite.Close();
	}

}
