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
			"type TEXT, " + //now all as "default", but in the future...
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
			dbcon.Open();

		dbcmd.CommandText = "INSERT INTO " + tableName +  
				" (uniqueID, personID, sessionID, type, time, description, simulated)" +
				" VALUES (" + uniqueID + ", "
				+ personID + ", " + sessionID + ", '" + type + "', "
				+ Util.ConvertToPoint(time) + ", '" + description + "', " + simulated + ")" ;
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		int myLast = dbcon.LastInsertRowId;
		
		if(! dbconOpened)
			dbcon.Close();

		return myLast;
	}

	public static string[] SelectAllReactionTimes(int sessionID) 
	{
		dbcon.Open();
		dbcmd.CommandText = "SELECT person.name, reactionTime.* " +
			" FROM person, reactionTime " +
			" WHERE person.uniqueID == reactionTime.personID" + 
			" AND reactionTime.sessionID == " + sessionID + 
			" ORDER BY person.uniqueID, reactionTime.uniqueID";
		
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
					Util.ChangeDecimalSeparator(reader[5].ToString()) + ":" + 	//jump.time
					reader[6].ToString() + ":" + 	//description
					reader[7].ToString()		//simulated
					);
			count ++;
		}

		reader.Close();
		dbcon.Close();

		string [] myEvents = new string[count];
		count =0;
		foreach (string line in myArray) {
			myEvents [count++] = line;
		}

		return myEvents;
	}

	public static ReactionTime SelectReactionTimeData(int uniqueID)
	{
		dbcon.Open();

		dbcmd.CommandText = "SELECT * FROM " + Constants.ReactionTimeTable + " WHERE uniqueID == " + uniqueID;
		
		Log.WriteLine(dbcmd.CommandText.ToString());

		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		reader.Read();
		
		ReactionTime myRT = new ReactionTime(DataReaderToStringArray(reader, 7));
		/*
		ReactionTime myRT = new ReactionTime(
				Convert.ToInt32(reader[0]),	//uniqueID
				Convert.ToInt32(reader[1]),	//personID
				Convert.ToInt32(reader[2]),	//sessionID
				//reader[3].ToString(),		//type
				Convert.ToDouble( Util.ChangeDecimalSeparator(reader[4].ToString()) ),
				reader[5].ToString(),  //description
				Convert.ToInt32(reader[6]) //simulated
				);
				*/
	
		dbcon.Close();
		return myRT;
	}
		
	public static void Update(int eventID, string type, string time, int personID, string description)
	{
		dbcon.Open();
		dbcmd.CommandText = "UPDATE " + Constants.ReactionTimeTable + " SET personID = " + personID + 
			", type = '" + type +
			"', time = " + Util.ConvertToPoint(time) +
			", description = '" + description +
			"' WHERE uniqueID == " + eventID ;
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		dbcon.Close();
	}

	public static void Delete(string uniqueID)
	{
		dbcon.Open();
		dbcmd.CommandText = "Delete FROM " + Constants.ReactionTimeTable + " WHERE uniqueID == " + uniqueID;
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		dbcon.Close();
	}
}
