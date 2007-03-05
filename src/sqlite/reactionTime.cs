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


class SqliteReactionTime : Sqlite
{
	/*
	 * create and initialize tables
	 */
	
	protected internal static void createTable()
	{
		dbcmd.CommandText = 
			"CREATE TABLE reactionTime ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"personID INT, " +
			"sessionID INT, " +
			"type TEXT, " + //now all as "default", but in the future...
			"time FLOAT, " +
			"description TEXT )";		
		dbcmd.ExecuteNonQuery();
	}
	
	
	/*
	 * ReactionTime class methods
	 */
	
	public static int Insert(int personID, int sessionID, string type, double time, string description)
	{
		dbcon.Open();
		dbcmd.CommandText = "INSERT INTO reactionTime" + 
				"(uniqueID, personID, sessionID, type, time, description)" +
				" VALUES (NULL, "
				+ personID + ", " + sessionID + ", '" + type + "', "
				+ Util.ConvertToPoint(time) + ", '" + description + "')" ;
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		int myLast = dbcon.LastInsertRowId;
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
					Util.ChangeDecimalSeparator(reader[5].ToString()) + ":" + 	//jump.time
					reader[6].ToString() 		//description
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

/*
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
*/

	public static void Update(int eventID, string type, string time, int personID, string description)
	{
		dbcon.Open();
		dbcmd.CommandText = "UPDATE reactionTime SET personID = " + personID + 
			", type = '" + type +
			"', time = " + Util.ConvertToPoint(time) +
			", description = '" + description +
			"' WHERE uniqueID == " + eventID ;
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		dbcon.Close();
	}

	public static void Delete(string uniqueID)
	{
		dbcon.Open();
		dbcmd.CommandText = "Delete FROM reactionTime WHERE uniqueID == " + uniqueID;
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		dbcon.Close();
	}
}
