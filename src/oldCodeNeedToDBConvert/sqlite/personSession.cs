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
using Mono.Unix;


class SqlitePersonSessionOld : Sqlite
{
	public SqlitePersonSessionOld() {
	}
	
	~SqlitePersonSessionOld() {}

	protected override void createTable(string tableName)
	 {
		dbcmd.CommandText = 
			"CREATE TABLE " + tableName + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"personID INT, " +
			"sessionID INT, " +
			"weight FLOAT)";		
		dbcmd.ExecuteNonQuery();
	 }

	public static int Insert(bool dbconOpened, string tableName, string uniqueID, int personID, int sessionID, double weight)
	{
		if(!dbconOpened)
			dbcon.Open();
		
		if(uniqueID == "-1")
			uniqueID = "NULL";

		dbcmd.CommandText = "INSERT INTO " + tableName + 
			"(uniqueID, personID, sessionID, weight) VALUES ("
			+ uniqueID + ", " + personID + ", " + sessionID + ", " + Util.ConvertToPoint(weight) + ")" ;
		dbcmd.ExecuteNonQuery();
		int myReturn = -10000; //dbcon.LastInsertRowId;
		if(!dbconOpened)
			dbcon.Close();
		return myReturn;
	}

	//used on Sqlite main convertPersonAndPersonSessionTo77()
	public ArrayList SelectAllPersonSessionsOfAPerson(int personID)
	{
		dbcmd.CommandText = "SELECT * FROM " + Constants.PersonSessionOldWeightTable + " WHERE personID == " + personID + " ORDER BY uniqueID";
		LogB.SQL(dbcmd.CommandText.ToString());
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		
		ArrayList myArray = new ArrayList(1);
		while(reader.Read()) {
			PersonSessionOld ps = new PersonSessionOld(
					Convert.ToInt32(reader[0].ToString()), //uniqueID
					Convert.ToInt32(reader[1].ToString()), //personID
					Convert.ToInt32(reader[2].ToString()), //sessionID
					Convert.ToDouble(Util.ChangeDecimalSeparator(reader[3].ToString())) //weight
					);
			myArray.Add(ps);
		}
		reader.Close();
		return myArray;
	}
		

	/* 
	 * conversion from database 0.52 to 0.53 (add weight into personSession)
	 * now weight of a person can change every session
	*/
	protected internal static void moveOldTableToNewTable() 
	{
		string tp = Constants.PersonOldTable;
		string tps1 = Constants.PersonSessionOldTable;
		string tps2 = Constants.PersonSessionOldWeightTable;
		
		dbcon.Open();
			
		dbcmd.CommandText = "SELECT " + tps1 + ".*, " + tp + ".weight " + 
			" FROM " + tps1 + ", " + tp + 
			" WHERE " + tps1 + ".personID = " + tp + ".UniqueID " + 
			" ORDER BY sessionID, personID";
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		
		ArrayList myArray = new ArrayList(2);
		while(reader.Read()) {
			//reader[0] (uniqueID (of table)) is not used
			myArray.Add (reader[1].ToString() + ":" + reader[2].ToString() + ":" + reader[3].ToString());
		}
		reader.Close();

		dropOldTable(tps1);

		dbcon.Close();
			
		foreach (string line in myArray) {
			string [] stringFull = line.Split(new char[] {':'});
			Insert(
					false,
					tps2,
					"-1",
					Convert.ToInt32(stringFull[0]), //personID
					Convert.ToInt32(stringFull[1]), //sessionID
					Convert.ToInt32(stringFull[2]) //weight
			      );
		}
	}
	
	public static bool PersonExistsInPS(int personID)
	{
		dbcmd.CommandText = "SELECT * FROM " + Constants.PersonSessionOldWeightTable + 
			" WHERE personID == " + personID;
		//LogB.SQL(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
	
		bool exists = new bool();
		exists = false;
		
		if (reader.Read()) {
			exists = true;
		}

		reader.Close();
		return exists;
	}

	
	/* 
	 * conversion from database 0.52 to 0.53 (add weight into personSession)
	 * now weight of a person can change every session
	*/
	private static void dropOldTable(string tableName) {
		dbcmd.CommandText = "DROP TABLE " + tableName;
		dbcmd.ExecuteNonQuery();
	}
	
}

