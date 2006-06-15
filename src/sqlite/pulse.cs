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


class SqlitePulse : Sqlite
{
	/*
	 * create and initialize tables
	 */
	
	protected internal static void createTable()
	{
		dbcmd.CommandText = 
			"CREATE TABLE pulse ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"personID INT, " +
			"sessionID INT, " +
			"type TEXT, " +
			"fixedPulse FLOAT, " +
			"totalPulsesNum INT, " +
			"timeString TEXT, " +
			"description TEXT )";		
		dbcmd.ExecuteNonQuery();
	}
	
	
	/*
	 * Pulse class methods
	 */
	
	public static int Insert(int personID, int sessionID, string type, double fixedPulse, int totalPulsesNum, string timeString, string description)
	{
		dbcon.Open();
		dbcmd.CommandText = "INSERT INTO pulse" + 
				"(uniqueID, personID, sessionID, type, fixedPulse, totalPulsesNum, timeString, description)" +
				" VALUES (NULL, "
				+ personID + ", " + sessionID + ", '" + type + "', "
				+ Util.ConvertToPoint(fixedPulse) + ", " + totalPulsesNum + ", '"
				+ timeString + "', '" + description + "')" ;
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		int myLast = dbcon.LastInsertRowId;
		dbcon.Close();

		return myLast;
	}
	


	public static string[] SelectAllPulses(int sessionID) 
	{
		dbcon.Open();
		dbcmd.CommandText = "SELECT person.name, pulse.* " +
			" FROM person, pulse " +
			" WHERE person.uniqueID == pulse.personID" + 
			" AND pulse.sessionID == " + sessionID + 
			" ORDER BY person.uniqueID, pulse.uniqueID";
		
		Console.WriteLine(dbcmd.CommandText.ToString());
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
					reader[8].ToString() + ":"  	//description
					);
			count ++;
		}

		reader.Close();
		dbcon.Close();

		string [] myPulses = new string[count];
		count =0;
		foreach (string line in myArray) {
			myPulses [count++] = line;
		}

		return myPulses;
	}

	/*
	public static Run SelectNormalRunData(int uniqueID)
	{
		dbcon.Open();

		dbcmd.CommandText = "SELECT * FROM run WHERE uniqueID == " + uniqueID;
		
		Console.WriteLine(dbcmd.CommandText.ToString());

		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		reader.Read();
		
		Run myRun = new Run(
				Convert.ToInt32(reader[0]),	//uniqueID
				Convert.ToInt32(reader[1]),	//personID
				Convert.ToInt32(reader[2]),	//sessionID
				reader[3].ToString(),		//type
				Convert.ToDouble( Util.ChangeDecimalSeparator(reader[4].ToString()) ),
				Convert.ToDouble( Util.ChangeDecimalSeparator(reader[5].ToString()) ),
				reader[6].ToString() //description
				);
	
		return myRun;
	}
		
	public static RunInterval SelectIntervalRunData(int uniqueID)
	{
		dbcon.Open();

		dbcmd.CommandText = "SELECT * FROM runInterval WHERE uniqueID == " + uniqueID;
		
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		reader.Read();

		RunInterval myRun = new RunInterval(
				Convert.ToInt32(reader[0]),	//uniqueID
				Convert.ToInt32(reader[1]),	//personID
				Convert.ToInt32(reader[2]),	//sessionID
				reader[3].ToString(),		//type
				Convert.ToDouble(Util.ChangeDecimalSeparator(reader[4].ToString())), //distanceTotal
				Convert.ToDouble(Util.ChangeDecimalSeparator(reader[5].ToString())), //timeTotal
				Convert.ToDouble(Util.ChangeDecimalSeparator(reader[6].ToString())), //distanceInterval
				Util.ChangeDecimalSeparator(reader[7].ToString()),	//intervalTimesString
				Convert.ToDouble(Util.ChangeDecimalSeparator(reader[8].ToString())), //tracks
				reader[9].ToString(), 		//description
				reader[10].ToString() 		//limited
				);

		return myRun;
	}
	*/

	/*
	public static void Update(int pulseID, string type, string distance, string time, int personID, string description)
	{
		dbcon.Open();
		dbcmd.CommandText = "UPDATE pulse " + 
			" SET personID = " + personID + 
			", type = '" + type +
			"', distance = " + Util.ConvertToPoint(Convert.ToDouble(distance)) + 
			", time = " + Util.ConvertToPoint(Convert.ToDouble(time)) + 
			", description = '" + description +
			"' WHERE uniqueID == " + runID ;
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		dbcon.Close();
	}
	*/

	public static void Delete(string uniqueID)
	{
		dbcon.Open();
		dbcmd.CommandText = "Delete FROM pulse WHERE uniqueID == " + uniqueID;
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		dbcon.Close();
	}
}
