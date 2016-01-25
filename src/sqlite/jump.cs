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
 * Copyright (C) 2004-2015   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using System.IO;
using System.Collections; //ArrayList
using Mono.Data.Sqlite;


class SqliteJump : Sqlite
{
	public SqliteJump() {
	}
	
	~SqliteJump() {}

	/*
	 * create and initialize tables
	 */
	
	protected override void createTable(string tableName)
	{
		//values: Constants.JumpTable and Constants.TempEventTable'
		dbcmd.CommandText = 
			"CREATE TABLE " + tableName + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"personID INT, " +
			"sessionID INT, " +
			"type TEXT, " +
			"tv FLOAT, " +
			"tc FLOAT, " +
			"fall FLOAT, " +  
			"weight TEXT, " + //string because can contain "33%" or "50Kg"
			"description TEXT, " +
			"angle FLOAT, " + //-1.0 if undef
			"simulated INT )"; 	//since db: 0.60 (cj 0.8.1.2) simulated = -1, real test (not uploaded to server) = 0, 
						//positive numbers represent the serverUniqueID
						//the simulated has two purposes, but it's logical because 
						//only real tests can be uploaded
		dbcmd.ExecuteNonQuery();
	}
	
	
	/*
	 * Jump class methods
	 */
	
	//public static int Insert(int personID, int sessionID, string type, double tv, double tc, int fall, double weight, string limited, string description, int simulated)
	public static int Insert(bool dbconOpened, string tableName, string uniqueID, int personID, int sessionID, string type, double tv, double tc, double fall, double weight, string description, double angle, int simulated)
	{
		if(! dbconOpened)
			Sqlite.Open();

		if(uniqueID == "-1")
			uniqueID = "NULL";

		dbcmd.CommandText = "INSERT INTO " + tableName +  
				" (uniqueID, personID, sessionID, type, tv, tc, fall, weight, description, angle, simulated)" +
				" VALUES (" + uniqueID + ", "
				+ personID + ", " + sessionID + ", \"" + type + "\", "
				+ Util.ConvertToPoint(tv) + ", " + Util.ConvertToPoint(tc) + ", " + Util.ConvertToPoint(fall) + ", \"" 
				+ Util.ConvertToPoint(weight) + "\", \"" + description + "\", "
				+ Util.ConvertToPoint(angle) + ", " + simulated +")" ;
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
	
	//if all sessions, put -1 in sessionID
	//if all persons, put -1 in personID
	//if all types put, "" in filterType
	public static string[] SelectJumps(bool dbconOpened, int sessionID, int personID, string filterWeight, string filterType) 
	{
		if(!dbconOpened)
			Sqlite.Open();

		string tp = Constants.PersonTable;
		string tps = Constants.PersonSessionTable;

		string filterSessionString = "";
		if(sessionID != -1)
			filterSessionString = " AND jump.sessionID == " + sessionID;

		string filterPersonString = "";
		if(personID != -1)
			filterPersonString = " AND " + tp + ".uniqueID == " + personID;

		string filterWeightString = "";
		if(filterWeight == "withWeight")
			filterWeightString = " AND jump.weight != 0 ";

		string filterTypeString = "";
		if(filterType != "")
			filterTypeString = " AND jump.type == \"" + filterType + "\" ";

		dbcmd.CommandText = "SELECT " + tp + ".name, jump.*, " + tps + ".weight " +
			" FROM " + tp + ", jump, " + tps + 
			" WHERE " + tp + ".uniqueID == jump.personID " + 
			filterSessionString +
			filterPersonString +
			filterWeightString +
			filterTypeString +
			" AND " + tps + ".personID == " + tp + ".uniqueID " +
			" AND " + tps + ".sessionID == jump.sessionID " +
			" ORDER BY upper(" + tp + ".name), jump.uniqueID";
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);

		int count = new int();
		count = 0;
		
		while(reader.Read()) {

			myArray.Add (reader[0].ToString() + ":" +	//person.name
					reader[1].ToString() + ":" +	//jump.uniqueID
					reader[2].ToString() + ":" + 	//jump.personID
					reader[3].ToString() + ":" + 	//jump.sessionID
					reader[4].ToString() + ":" + 	//jump.type
					Util.ChangeDecimalSeparator(reader[5].ToString()) + ":" + 	//jump.tv
					Util.ChangeDecimalSeparator(reader[6].ToString()) + ":" + 	//jump.tc
					Util.ChangeDecimalSeparator(reader[7].ToString()) + ":" + 	//fall
					Util.ChangeDecimalSeparator(reader[8].ToString()) + ":" + 	//weight
					reader[9].ToString() + ":" +	//description
					Util.ChangeDecimalSeparator(reader[10].ToString()) + ":" +	//angle
					reader[11].ToString() + ":" +	//simulated
					reader[12].ToString() 		//person.weight
					);
			count ++;
		}

		reader.Close();
		
		if(!dbconOpened)
			Sqlite.Close();


		string [] myJumps = new string[count];
		count =0;
		foreach (string line in myArray) {
			myJumps [count++] = line;
		}

		return myJumps;
	}

	public static Jump SelectJumpData(int uniqueID, bool dbconOpened)
	{
		if(!dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "SELECT * FROM jump WHERE uniqueID == " + uniqueID;
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		reader.Read();

		Jump myJump = new Jump(DataReaderToStringArray(reader, 11));
	
		reader.Close();
		
		if(!dbconOpened)
			Sqlite.Close();

		return myJump;
	}
		

	public static void Update(int jumpID, string type, string tv, string tc, string fall, int personID, double weight, string description, double angle)
	{
		Sqlite.Open();
		dbcmd.CommandText = "UPDATE jump SET personID = " + personID + 
			", type = \"" + type +
			"\", tv = " + Util.ConvertToPoint(tv) +
			", tc = " + Util.ConvertToPoint(tc) +
			", fall = " + Util.ConvertToPoint(fall) +
			", weight = " + Util.ConvertToPoint(weight) + 
			", description = \"" + description +
			"\", angle = " + Util.ConvertToPoint(angle) +
			" WHERE uniqueID == " + jumpID ;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		Sqlite.Close();
	}

	public static void UpdateWeight(string tableName, int uniqueID, double weight)
	{
		Sqlite.Open();
		dbcmd.CommandText = "UPDATE " + tableName + " SET weight = " + Util.ConvertToPoint(weight) + 
			" WHERE uniqueID == " + uniqueID ;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		Sqlite.Close();
	}

	public static void UpdateDescription(string tableName, int uniqueID, string description)
	{
		Sqlite.Open();
		dbcmd.CommandText = "UPDATE " + tableName + " SET description = \"" + description + 
			"\" WHERE uniqueID == " + uniqueID ;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		Sqlite.Close();
	}

	//onle for change SJ+ CMJ+ and ABK+ to SJl...
	public static void ChangeWeightToL()
	{
		dbcmd.CommandText = "UPDATE jump SET type = \"SJl\" WHERE type == \"SJ+\"";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		dbcmd.CommandText = "UPDATE jump SET type = \"CMJl\" WHERE type == \"CMJ+\"";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		dbcmd.CommandText = "UPDATE jump SET type = \"ABKl\" WHERE type == \"ABK+\"";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}
}
