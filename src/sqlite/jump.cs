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
using Mono.Data.SqliteClient;
using System.Data.SqlClient;


class SqliteJump : Sqlite
{
	/*
	 * create and initialize tables
	 */
	
	protected internal static void createTable()
	{
		dbcmd.CommandText = 
			"CREATE TABLE jump ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"personID INT, " +
			"sessionID INT, " +
			"type TEXT, " +
			"tv FLOAT, " +
			"tc FLOAT, " +
			"fall INT, " +  
			"weight TEXT, " + //string because can contain "33%" or "50Kg"
			"description TEXT )";		
		dbcmd.ExecuteNonQuery();
	}
	
	protected internal static void rjCreateTable(string tableName)
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
			"fall INT, " +  
			"weight TEXT, " + //string because can contain "33%" or "50Kg"
			"description TEXT, " +		//this and the above values are equal than normal jump
			"tvAvg FLOAT, " +		//this and next values are Rj specific
			"tcAvg FLOAT, " +
			"tvString TEXT, " +
			"tcString TEXT, " +
			"jumps INT, " +
			"time FLOAT, " + //if limit it's 'n' jumps, we probably waste 7.371 seconds
			"limited TEXT) "; //for RJ, "11J" or "11S" (11 Jumps, 11 seconds)
		dbcmd.ExecuteNonQuery();
	}

	
	/*
	 * Jump class methods
	 */
	
	public static int Insert(int personID, int sessionID, string type, double tv, double tc, int fall, double weight, string limited, string description)
	{
		dbcon.Open();
		dbcmd.CommandText = "INSERT INTO jump" + 
				"(uniqueID, personID, sessionID, type, tv, tc, fall, weight, description)" +
				" VALUES (NULL, "
				+ personID + ", " + sessionID + ", '" + type + "', "
				+ Util.ConvertToPoint(tv) + ", " + Util.ConvertToPoint(tc) + ", " + fall + ", '" 
				+  Util.ConvertToPoint(weight) + "', '" + description + "')" ;
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		int myLast = dbcon.LastInsertRowId;
		dbcon.Close();

		//Jump myJump = new Jump(myLast, personID, sessionID,
		//		type, tv, tc, fall, weight, description );
		
		return myLast;
	}
	
	//fall has values like "10J" or "10T" (10 jumps, or 10 seconds, respectively)
	public static int InsertRj(string tableName, string uniqueID, int personID, int sessionID, string type, double tvMax, double tcMax, int fall, double weight, string description, double tvAvg, double tcAvg, string tvString, string tcString, int jumps, double time, string limited )
	{
		dbcon.Open();
		dbcmd.CommandText = "INSERT INTO " + tableName + 
				" (uniqueID, personID, sessionID, type, tvMax, tcMax, fall, weight, description, " +
				"tvAvg, tcAvg, tvString, tcString, jumps, time, limited	)" +
				"VALUES (" + uniqueID + ", " +
				personID + ", " + sessionID + ", '" + type + "', " +
				Util.ConvertToPoint(tvMax) + ", " + Util.ConvertToPoint(tcMax) + ", '" + 
				fall + "', '" + Util.ConvertToPoint(weight) + "', '" + description + "', " +
				Util.ConvertToPoint(tvAvg) + ", " + Util.ConvertToPoint(tcAvg) + ", '" + 
				Util.ConvertToPoint(tvString) + "', '" + Util.ConvertToPoint(tcString) + "', " +
				jumps + ", " + Util.ConvertToPoint(time) + ", '" + limited + "')" ;
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		int myLast = dbcon.LastInsertRowId;

		//JumpRj myJump = new JumpRj (myLast, personID, sessionID, type, tvString, tcString,
		//		fall, weight, description, jumps, time, limited );

		dbcon.Close();

		return myLast;
	}

	public static string[] SelectAllNormalJumps(int sessionID) 
	{
		dbcon.Open();
		dbcmd.CommandText = "SELECT person.name, jump.* " +
			" FROM person, jump " +
			" WHERE person.uniqueID == jump.personID" + 
			" AND jump.sessionID == " + sessionID + 
			" ORDER BY person.uniqueID, jump.uniqueID";
		
		Console.WriteLine(dbcmd.CommandText.ToString());
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
					reader[9].ToString() 		//description
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

	public static string[] SelectAllRjJumps(int sessionID) 
	{
		dbcon.Open();
		dbcmd.CommandText = "SELECT person.name, jumpRj.* " +
			" FROM person, jumpRj " +
			" WHERE person.uniqueID == jumpRj.personID" + 
			" AND jumpRj.sessionID == " + sessionID + 
			" ORDER BY person.uniqueID, jumpRj.uniqueID";
		
		Console.WriteLine(dbcmd.CommandText.ToString());
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
					reader[7].ToString() + ":" + 	//fall
					Util.ChangeDecimalSeparator(reader[8].ToString()) + ":" + 	//weight
					reader[9].ToString() + ":" + 	//description
					Util.ChangeDecimalSeparator(reader[10].ToString()) + ":" + 	//tvAvg,
					Util.ChangeDecimalSeparator(reader[11].ToString()) + ":" + 	//tcAvg,
					Util.ChangeDecimalSeparator(reader[12].ToString()) + ":" + 	//tvString,
					Util.ChangeDecimalSeparator(reader[13].ToString()) + ":" + 	//tcString,
					reader[14].ToString() + ":" + 	//jumps,
					reader[15].ToString() + ":" + 	//time,
					reader[16].ToString() 	 	//limited
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

	public static Jump SelectNormalJumpData(int uniqueID)
	{
		dbcon.Open();

		dbcmd.CommandText = "SELECT * FROM jump WHERE uniqueID == " + uniqueID;
		
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		reader.Read();

		Jump myJump = new Jump(
				Convert.ToInt32(reader[0]),
				Convert.ToInt32(reader[1]),
				Convert.ToInt32(reader[2]),
				reader[3].ToString(),
				Convert.ToDouble( Util.ChangeDecimalSeparator(reader[4].ToString()) ),
				Convert.ToDouble( Util.ChangeDecimalSeparator(reader[5].ToString()) ),
				Convert.ToInt32(reader[6]),  //fall
				Convert.ToDouble( Util.ChangeDecimalSeparator(reader[7].ToString()) ), //weight
				reader[8].ToString() //description
				);
	
		return myJump;
	}
		
	public static JumpRj SelectRjJumpData(string tableName, int uniqueID)
	{
		//tableName is jumpRj or tempJumpRj

		dbcon.Open();

		dbcmd.CommandText = "SELECT * FROM " + tableName + " WHERE uniqueID == " + uniqueID;
		
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		reader.Read();

		JumpRj myJump = new JumpRj(
				Convert.ToInt32(reader[0]),	//uniqueID
				Convert.ToInt32(reader[1]),	//personID
				Convert.ToInt32(reader[2]),	//sessionID
				reader[3].ToString(),		//type
				Util.ChangeDecimalSeparator(reader[11].ToString()),		//tvString
				Util.ChangeDecimalSeparator(reader[12].ToString()),		//tcString
				//tvMax and tcMax not needed by the constructor:
				//Convert.ToDouble( reader[4].ToString() ), //tvMax
				//Convert.ToDouble( reader[5].ToString() ), //tcMax
				Convert.ToInt32(reader[6]),  	//fall
				Convert.ToDouble( Util.ChangeDecimalSeparator(reader[7].ToString()) ), 	//weight
				reader[8].ToString(), 		//description
				//tvAvg and tcAvg not needed by the constructor:
				//Convert.ToDouble( reader[9].ToString() ), //tvAvg
				//Convert.ToDouble( reader[10].ToString() ), //tcAvg
				Convert.ToInt32(reader[13]),		//jumps
				Convert.ToDouble(reader[14]),		//time
				reader[15].ToString()		//limited
				);

		return myJump;
	}
	

	//checks if there are Rjs with different number of TCs than TFs
	//then repair database manually, and look if the jump is jumpLimited, and how many jumps there are defined
	public static void FindBadRjs()
	{
		dbcon.Open();

		dbcmd.CommandText = "SELECT uniqueID, tcstring, tvstring, jumps, limited FROM jumpRj";
		
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		reader.Read();

		while(reader.Read()) {
			if(Util.GetNumberOfJumps(reader[1].ToString(), true) != Util.GetNumberOfJumps(reader[2].ToString(), true)) {
				Console.WriteLine("Problem with jumpRj: {0}, tcstring{1}, tvstring{2}, jumps{3}, limited{4}", 
						reader[0].ToString(), 
						Util.GetNumberOfJumps(reader[1].ToString(), true).ToString(), 
						Util.GetNumberOfJumps(reader[2].ToString(), true).ToString(), 
						reader[3].ToString(), reader[4].ToString());
			}
		}
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
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		dbcon.Close();
	}

	public static void UpdateRj(int jumpID, int personID, string fall, double weight, string description)
	{
		dbcon.Open();
		dbcmd.CommandText = "UPDATE jumpRj SET personID = " + personID + 
			", fall = " + Util.ConvertToPoint(Convert.ToDouble(fall)) + 
			", weight = " + Util.ConvertToPoint(weight) + 
			", description = '" + description +
			"' WHERE uniqueID == " + jumpID ;
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		dbcon.Close();
	}

	public static void Delete(string jumpTable, string uniqueID)
	{
		dbcon.Open();
		dbcmd.CommandText = "Delete FROM " + jumpTable +
			" WHERE uniqueID == " + uniqueID;
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		dbcon.Close();
	}

	//onle for change SJ+ CMJ+ and ABK+ to SJl...
	public static void ChangeWeightToL()
	{
		//dbcon.Open();
		
		dbcmd.CommandText = "UPDATE jump SET type = 'SJl' WHERE type == 'SJ+'";
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		dbcmd.CommandText = "UPDATE jump SET type = 'CMJl' WHERE type == 'CMJ+'";
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		dbcmd.CommandText = "UPDATE jump SET type = 'ABKl' WHERE type == 'ABK+'";
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		//dbcon.Close();
	}
}
