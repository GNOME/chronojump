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


class SqlitePulse : Sqlite
{
	public SqlitePulse() {
	}
	
	~SqlitePulse() {}

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
			"fixedPulse FLOAT, " +
			"totalPulsesNum INT, " +
			"timeString TEXT, " +
			"description TEXT, " +
			"simulated INT )";		
		dbcmd.ExecuteNonQuery();
	}
	
	
	/*
	 * Pulse class methods
	 */
	
	public static int Insert(bool dbconOpened, string tableName, string uniqueID, int personID, int sessionID, string type, double fixedPulse, int totalPulsesNum, string timeString, string description, int simulated)
	{
		if(! dbconOpened)
			Sqlite.Open();
	
		if(uniqueID == "-1")
			uniqueID = "NULL";

		dbcmd.CommandText = "INSERT INTO " + tableName + 
				" (uniqueID, personID, sessionID, type, fixedPulse, totalPulsesNum, timeString, description, simulated)" +
				" VALUES (" + uniqueID + ", " + personID + ", " + sessionID + ", \"" + type + "\", "
				+ Util.ConvertToPoint(fixedPulse) + ", " + totalPulsesNum + ", \""
				+ timeString + "\", \"" + description + "\", " + simulated + ")" ;
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
	public static string[] SelectPulses(bool dbconOpened, int sessionID, int personID) 
	{
		if(!dbconOpened)
			Sqlite.Open();

		string tp = Constants.PersonTable;

		string filterPersonString = "";
		if(personID != -1)
			filterPersonString = " AND " + tp + ".uniqueID == " + personID;

		dbcmd.CommandText = "SELECT " + tp + ".name, pulse.* " +
			" FROM " + tp + ", pulse " +
			" WHERE " + tp + ".uniqueID == pulse.personID" + 
			" AND pulse.sessionID == " + sessionID + 
			filterPersonString +
			" ORDER BY upper(" + tp + ".name), pulse.uniqueID";
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);

		int count = new int();
		count = 0;

		while(reader.Read()) {
			myArray.Add (reader[0].ToString() + ":" +	//person.name
					reader[1].ToString() + ":" +	//pulse.uniqueID
					reader[2].ToString() + ":" + 	//pulse.personID
					reader[3].ToString() + ":" + 	//pulse.sessionID
					reader[4].ToString() + ":" + 	//pulse.type
					Util.ChangeDecimalSeparator(reader[5].ToString()) + ":" + //fixedPulse
					reader[6].ToString() + ":" + //totalPulsesNum
					Util.ChangeDecimalSeparator(reader[7].ToString()) + ":" + //timesString
					reader[8].ToString() + ":" +  	//description
					reader[9].ToString()	  	//simulated
					);
			count ++;
		}

		reader.Close();
		
		if(!dbconOpened)
			Sqlite.Close();

		string [] myPulses = new string[count];
		count =0;
		foreach (string line in myArray) {
			myPulses [count++] = line;
		}

		return myPulses;
	}

	public static Pulse SelectPulseData(int uniqueID, bool dbconOpened)
	{
		if(!dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "SELECT * FROM " + Constants.PulseTable + " WHERE uniqueID == " + uniqueID;
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		reader.Read();

		Pulse myPulse = new Pulse(DataReaderToStringArray(reader, 9));

		reader.Close();
		if(!dbconOpened)
			Sqlite.Close();
		return myPulse;
	}

	public static void Update(int pulseID, int personID, string description)
	{
		Sqlite.Open();
		dbcmd.CommandText = "UPDATE " + Constants.PulseTable + 
			" SET personID = " + personID + 
			", description = \"" + description +
			"\" WHERE uniqueID == " + pulseID ;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		Sqlite.Close();
	}

}
