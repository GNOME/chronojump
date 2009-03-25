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
			"cp1InStr TEXT, " +
			"cp1OutStr TEXT, " +
			"cp2InStr TEXT, " +
			"cp2OutStr TEXT, " +
			"cp3InStr TEXT, " +
			"cp3OutStr TEXT, " +
			"cp4InStr TEXT, " +
			"cp4OutStr TEXT, " +
			"description TEXT, " +
			"simulated INT )";		
		dbcmd.ExecuteNonQuery();
	}
	
	
	/*
	 * multiChronopic class methods
	 */

	public static int Insert(bool dbconOpened, string tableName, string uniqueID, int personID, int sessionID, string type, 
			string cp1InStr, string cp1OutStr,
			string cp2InStr, string cp2OutStr,
			string cp3InStr, string cp3OutStr,
			string cp4InStr, string cp4OutStr,
			string description, int simulated)
	{
		if(! dbconOpened)
			dbcon.Open();

		if(uniqueID == "-1")
			uniqueID = "NULL";

		dbcmd.CommandText = "INSERT INTO " + tableName +  
			" (uniqueID, personID, sessionID, type, " + 
			" cp1InStr, cp1OutStr, cp2InStr, cp2OutStr, cp3InStr, cp3OutStr, cp4InStr, cp4OutStr, " +
			" description, simulated)" +
			" VALUES (" + uniqueID + ", " +
			personID + ", " + sessionID + ", '" + type + "', '" +
			cp1InStr + "', '" + cp1OutStr + "', '" +
			cp2InStr + "', '" + cp2OutStr + "', '" +
			cp3InStr + "', '" + cp3OutStr + "', '" +
			cp4InStr + "', '" + cp4OutStr + "', '" +
			description + "', " + simulated + ")" ;
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		int myLast = dbcon.LastInsertRowId;
		
		if(! dbconOpened)
			dbcon.Close();

		return myLast;
	}

	/*
	//if all persons, put -1 in personID
	public static string[] SelectAllReactionTimes(int sessionID, int personID) 
	{
		string filterPersonString = "";
		if(personID != -1)
			filterPersonString = " AND person.uniqueID == " + personID;

		dbcon.Open();
		dbcmd.CommandText = "SELECT person.name, reactionTime.* " +
			" FROM person, reactionTime " +
			" WHERE person.uniqueID == reactionTime.personID" + 
			" AND reactionTime.sessionID == " + sessionID + 
			filterPersonString +
			" ORDER BY upper(person.name), reactionTime.uniqueID";
		
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
	*/
}
