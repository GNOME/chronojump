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
 * Xavier de Blas: 
 * http://www.xdeblas.com, http://www.deporteyciencia.com (parleblas)
 */

using System;
using System.Data;
using System.IO;
using System.Collections; //ArrayList
//using Mono.Data.SqliteClient;
//using System.Data.SqlClient;
using Mono.Data.Sqlite;
//using System.Data.SQLite;


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
			"fall INT, " +  
			"weight TEXT, " + //string because can contain "33%" or "50Kg"
			"description TEXT, " +
			"angle INT, " + //-1 if undef
			"simulated INT )";
		dbcmd.ExecuteNonQuery();
	}
	
	
	/*
	 * Jump class methods
	 */
	
	//public static int Insert(int personID, int sessionID, string type, double tv, double tc, int fall, double weight, string limited, string description, int simulated)
	public static int Insert(bool dbconOpened, string tableName, string uniqueID, int personID, int sessionID, string type, double tv, double tc, int fall, double weight, string description, int angle, int simulated)
	{
		if(! dbconOpened)
			dbcon.Open();

		dbcmd.CommandText = "INSERT INTO " + tableName +  
				" (uniqueID, personID, sessionID, type, tv, tc, fall, weight, description, angle, simulated)" +
				" VALUES (" + uniqueID + ", "
				+ personID + ", " + sessionID + ", '" + type + "', "
				+ Util.ConvertToPoint(tv) + ", " + Util.ConvertToPoint(tc) + ", " + fall + ", '" 
				+  Util.ConvertToPoint(weight) + "', '" + description + "', " + angle + ", " + simulated +")" ;
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		int myLast = dbcon.LastInsertRowId;
		if(! dbconOpened)
			dbcon.Close();

		return myLast;
	}
	
	//if all persons, put -1 in personID
	public static string[] SelectJumps(int sessionID, int personID, string filterWeight) 
	{
		string filterPersonString = "";
		if(personID != -1)
			filterPersonString = " AND person.uniqueID == " + personID;

		string filterWeightString = "";
		if(filterWeight == "withWeight")
			filterWeightString = " AND jump.weight != 0 ";

		dbcon.Open();
		dbcmd.CommandText = "SELECT person.name, jump.*, personSessionWeight.weight " +
			" FROM person, jump, personSessionWeight " +
			" WHERE person.uniqueID == jump.personID " + 
			" AND jump.sessionID == " + sessionID + 
			filterPersonString +
			filterWeightString +
			" AND personSessionWeight.personID == person.uniqueID " +
			" AND personSessionWeight.sessionID == jump.sessionID " +
			" ORDER BY person.uniqueID, jump.uniqueID";
		
		Log.WriteLine(dbcmd.CommandText.ToString());
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
					reader[7].ToString() + ":" + 	//fall
					Util.ChangeDecimalSeparator(reader[8].ToString()) + ":" + 	//weight
					reader[9].ToString() + ":" +	//description
					reader[10].ToString() + ":" +	//angle
					reader[11].ToString() + ":" +	//simulated
					reader[12].ToString() 		//person.weight
					);
			count ++;
		}

		reader.Close();
		dbcon.Close();

		string [] myJumps = new string[count];
		count =0;
		foreach (string line in myArray) {
			myJumps [count++] = line;
		}

		return myJumps;
	}

	public static Jump SelectJumpData(int uniqueID)
	{
		dbcon.Open();

		dbcmd.CommandText = "SELECT * FROM jump WHERE uniqueID == " + uniqueID;
		
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		reader.Read();

		Jump myJump = new Jump(DataReaderToStringArray(reader, 11));
	
		dbcon.Close();
		return myJump;
	}
		

	public static void Update(int jumpID, string type, string tv, string tc, string fall, int personID, double weight, string description)
	{
		dbcon.Open();
		dbcmd.CommandText = "UPDATE jump SET personID = " + personID + 
			", type = '" + type +
			"', tv = " + Util.ConvertToPoint(tv) +
			", tc = " + Util.ConvertToPoint(tc) +
			", fall = " + Util.ConvertToPoint(fall) +
			", weight = " + Util.ConvertToPoint(weight) + 
			", description = '" + description +
			"' WHERE uniqueID == " + jumpID ;
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		dbcon.Close();
	}

	public static void UpdateWeight(string tableName, int uniqueID, int weight)
	{
		dbcon.Open();
		dbcmd.CommandText = "UPDATE " + tableName + " SET weight = " + weight + 
			" WHERE uniqueID == " + uniqueID ;
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		dbcon.Close();
	}

	public static void Delete(string jumpTable, string uniqueID)
	{
		dbcon.Open();
		dbcmd.CommandText = "Delete FROM " + jumpTable +
			" WHERE uniqueID == " + uniqueID;
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		dbcon.Close();
	}

	//onle for change SJ+ CMJ+ and ABK+ to SJl...
	public static void ChangeWeightToL()
	{
		dbcmd.CommandText = "UPDATE jump SET type = 'SJl' WHERE type == 'SJ+'";
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		dbcmd.CommandText = "UPDATE jump SET type = 'CMJl' WHERE type == 'CMJ+'";
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		dbcmd.CommandText = "UPDATE jump SET type = 'ABKl' WHERE type == 'ABK+'";
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}
}
