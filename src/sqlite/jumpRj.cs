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


class SqliteJumpRj : SqliteJump
{
	public SqliteJumpRj() {
	}
	~SqliteJumpRj() {}
	
	protected override void createTable(string tableName)
	{
		//values: 'jumpRj' and 'tempJumpRj'

		dbcmd.CommandText = 
			"CREATE TABLE " + tableName + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"personID INT, " +
			"sessionID INT, " +
			"type TEXT, " + 
			"tvMax FLOAT, " +
			"tcMax FLOAT, " +
			"fall FLOAT, " +  
			"weight TEXT, " + //string because can contain "33%" or "50Kg"
			"description TEXT, " +		//this and the above values are equal than normal jump
			"tvAvg FLOAT, " +		//this and next values are Rj specific
			"tcAvg FLOAT, " +
			"tvString TEXT, " +
			"tcString TEXT, " +
			"jumps INT, " +
			"time FLOAT, " + //if limit it's 'n' jumps, we probably waste 7.371 seconds
			"limited TEXT, " + //for RJ, "11J" or "11S" (11 Jumps, 11 seconds)
			"angleString TEXT, " + //"-1" if undef
			"simulated INT )";
		dbcmd.ExecuteNonQuery();
	}

	public static int Insert (bool dbconOpened, string tableName, string uniqueID, int personID, int sessionID, string type, double tvMax, double tcMax, double fall, double weight, string description, double tvAvg, double tcAvg, string tvString, string tcString, int jumps, double time, string limited, string angleString, int simulated )
	{
		if(! dbconOpened)
			Sqlite.Open();
		
		if(uniqueID == "-1")
			uniqueID = "NULL";

		dbcmd.CommandText = "INSERT INTO " + tableName + 
				" (uniqueID, personID, sessionID, type, tvMax, tcMax, fall, weight, description, " +
				"tvAvg, tcAvg, tvString, tcString, jumps, time, limited, angleString, simulated )" +
				"VALUES (" + uniqueID + ", " +
				personID + ", " + sessionID + ", \"" + type + "\", " +
				Util.ConvertToPoint(tvMax) + ", " + Util.ConvertToPoint(tcMax) + ", \"" + 
				Util.ConvertToPoint(fall) + "\", \"" + Util.ConvertToPoint(weight) + "\", \"" + description + "\", " +
				Util.ConvertToPoint(tvAvg) + ", " + Util.ConvertToPoint(tcAvg) + ", \"" + 
				Util.ConvertToPoint(tvString) + "\", \"" + Util.ConvertToPoint(tcString) + "\", " +
				jumps + ", " + Util.ConvertToPoint(time) + ", \"" + limited + "\", \"" + angleString + "\", " + simulated +")" ;
		LogB.SQL(dbcmd.CommandText.ToString());

		LogB.Information("SQL going to insert RJ, status: " + dbcon.State.ToString());
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

	public static string[] SelectJumps(bool dbconOpened, int sessionID, int personID, string filterWeight, string filterType) 
	{
		if(!dbconOpened)
			Sqlite.Open();

		string tp = Constants.PersonTable;
		string tps = Constants.PersonSessionTable;

		string filterSessionString = "";
		if(sessionID != -1)
			filterSessionString = " AND jumpRj.sessionID == " + sessionID;

		string filterPersonString = "";
		if(personID != -1)
			filterPersonString = " AND " + tp + ".uniqueID == " + personID;

		string filterWeightString = "";
		if(filterWeight == "withWeight")
			filterWeightString = " AND jumpRj.weight != 0 ";

		string filterTypeString = "";
		if(filterType != "")
			filterTypeString = " AND jumpRj.type == \"" + filterType + "\" ";

		dbcmd.CommandText = "SELECT " + tp + ".name, jumpRj.*, " + tps + ".weight " +
			" FROM " + tp + ", jumpRj, " + tps + " " +
			" WHERE " + tp + ".uniqueID == jumpRj.personID" + 
			filterSessionString +
			filterPersonString +
			filterWeightString +
			filterTypeString +
			" AND " + tps + ".personID == " + tp + ".uniqueID " +
			" AND " + tps + ".sessionID == jumpRj.sessionID " +
			" ORDER BY upper(" + tp + ".name), jumpRj.uniqueID";
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);

		int count = new int();
		count = 0;

		while(reader.Read()) {
			myArray.Add (reader[0].ToString() + ":" +	//person.name
					reader[1].ToString() + ":" +	//jumpRj.uniqueID
					reader[2].ToString() + ":" + 	//jumpRj.personID
					reader[3].ToString() + ":" + 	//jumpRj.sessionID
					reader[4].ToString() + ":" + 	//jumpRj.type
					Util.ChangeDecimalSeparator(reader[5].ToString()) + ":" + 	//tvMax
					Util.ChangeDecimalSeparator(reader[6].ToString()) + ":" + 	//tcMax
					Util.ChangeDecimalSeparator(reader[7].ToString()) + ":" + 	//fall
					Util.ChangeDecimalSeparator(reader[8].ToString()) + ":" + 	//weight
					reader[9].ToString() + ":" + 	//description
					Util.ChangeDecimalSeparator(reader[10].ToString()) + ":" + 	//tvAvg,
					Util.ChangeDecimalSeparator(reader[11].ToString()) + ":" + 	//tcAvg,
					Util.ChangeDecimalSeparator(reader[12].ToString()) + ":" + 	//tvString,
					Util.ChangeDecimalSeparator(reader[13].ToString()) + ":" + 	//tcString,
					reader[14].ToString() + ":" + 	//jumps,
					reader[15].ToString() + ":" + 	//time,
					reader[16].ToString() + ":" + 	//limited
					reader[17].ToString() + ":" +	//angleString
					reader[18].ToString() + ":" +	//simulated
					reader[19].ToString() 	 	//person.weight
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

	public static JumpRj SelectJumpData(string tableName, int uniqueID, bool dbconOpened)
	{
		//tableName is jumpRj or tempJumpRj

		if(!dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "SELECT * FROM " + tableName + " WHERE uniqueID == " + uniqueID;
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		reader.Read();

		JumpRj myJump = new JumpRj(DataReaderToStringArray(reader, 18));

		reader.Close();
		if(!dbconOpened)
			Sqlite.Close();
		return myJump;
	}
	
	public static void Update(int jumpID, int personID, string fall, double weight, string description)
	{
		Sqlite.Open();
		dbcmd.CommandText = "UPDATE jumpRj SET personID = " + personID + 
			", fall = " + Util.ConvertToPoint(Convert.ToDouble(fall)) + 
			", weight = " + Util.ConvertToPoint(weight) + 
			", description = \"" + description +
			"\" WHERE uniqueID == " + jumpID ;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		Sqlite.Close();
	}

	//checks if there are Rjs with different number of TCs than TFs
	//then repair database manually, and look if the jump is jumpLimited, and how many jumps there are defined
	public static void FindBadRjs()
	{
		Sqlite.Open();

		dbcmd.CommandText = "SELECT uniqueID, tcstring, tvstring, jumps, limited FROM jumpRj";
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		reader.Read();

		while(reader.Read()) {
			if(Util.GetNumberOfJumps(reader[1].ToString(), true) != Util.GetNumberOfJumps(reader[2].ToString(), true)) {
				LogB.Error(string.Format("Problem with jumpRj: {0}, tcstring{1}, tvstring{2}, jumps{3}, limited{4}", 
						reader[0].ToString(), 
						Util.GetNumberOfJumps(reader[1].ToString(), true).ToString(), 
						Util.GetNumberOfJumps(reader[2].ToString(), true).ToString(), 
						reader[3].ToString(), reader[4].ToString()));
			}
		}
		reader.Close();
		Sqlite.Close();
	}

}

