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


class SqliteRun : Sqlite
{
	/*
	 * create and initialize tables
	 */
	
	protected static void createTable()
	{
		dbcmd.CommandText = 
			"CREATE TABLE run ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"personID INT, " +
			"sessionID INT, " +
			"type TEXT, " +
			"distance FLOAT, " +
			"time FLOAT, " +
			"description TEXT )";		
		dbcmd.ExecuteNonQuery();
	}
	
	protected static void intervalCreateTable()
	{
		dbcmd.CommandText = 
			"CREATE TABLE runInterval ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"personID INT, " +
			"sessionID INT, " +
			"type TEXT, " +
			"distanceTotal FLOAT, " +
			"timeTotal FLOAT, " +
			"distanceInterval FLOAT, " +
			"intervalTimesString TEXT, " +
			"tracks FLOAT, " +	//float because if we limit by time (runType tracksLimited false), we do n.nn tracks
			"description TEXT, " +
			"limited TEXT) ";
		dbcmd.ExecuteNonQuery();
	}

	
	/*
	 * Run class methods
	 */
	
	public static int Insert(int personID, int sessionID, string type, double distance, double time, string description)
	{
		dbcon.Open();
		dbcmd.CommandText = "INSERT INTO run" + 
				"(uniqueID, personID, sessionID, type, distance, time, description)" +
				" VALUES (NULL, "
				+ personID + ", " + sessionID + ", '" + type + "', "
				+ distance + ", " + time + ", '" + description + "')" ;
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		int myLast = dbcon.LastInsertRowId;
		dbcon.Close();

		return myLast;
	}
	
	public static int InsertInterval(int personID, int sessionID, string type, double distanceTotal, double timeTotal, double distanceInterval, string intervalTimesString, double tracks, string description, string limited )
	{
		dbcon.Open();
		dbcmd.CommandText = "INSERT INTO runInterval " + 
				"(uniqueID, personID, sessionID, type, distanceTotal, timeTotal, distanceInterval, intervalTimesString, tracks, description, limited )" +
				"VALUES (NULL, " +
				personID + ", " + sessionID + ", '" + type + "', " +
				distanceTotal + ", " + timeTotal + ", " + distanceInterval + ", '" + 
				intervalTimesString + "', " +
				tracks + ", '" + description + "', '" + limited + "')" ;
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		int myLast = dbcon.LastInsertRowId;

		dbcon.Close();

		return myLast;
	}

	public static string[] SelectAllNormalRuns(int sessionID, string ordered_by) 
	{
		string secondOrder;
		if(ordered_by == "ordered_by_time") {
			secondOrder = "run.uniqueID";
		}
		else { //by type
			secondOrder = "run.type, " + "run.uniqueID";
		}
		
		dbcon.Open();
		dbcmd.CommandText = "SELECT person.name, run.* " +
			" FROM person, run " +
			" WHERE person.uniqueID == run.personID" + 
			" AND run.sessionID == " + sessionID + 
			" ORDER BY person.uniqueID, " +
			secondOrder;
		
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);

		int count = new int();
		count = 0;

		
		while(reader.Read()) {

			myArray.Add (reader[0].ToString() + ":" +	//person.name
					reader[1].ToString() + ":" +	//run.uniqueID
					reader[2].ToString() + ":" + 	//run.personID
					reader[3].ToString() + ":" + 	//run.sessionID
					reader[4].ToString() + ":" + 	//run.type
					reader[5].ToString() + ":" + 	//run.distance
					reader[6].ToString() + ":" + 	//run.time
					reader[7].ToString() 		//description
					);
			count ++;
		}

		reader.Close();
		dbcon.Close();

		string [] myRuns = new string[count];
		count =0;
		foreach (string line in myArray) {
			myRuns [count++] = line;
		}

		return myRuns;
	}

	public static string[] SelectAllIntervalRuns(int sessionID, string ordered_by) 
	{
		string secondOrder;
		if(ordered_by == "ordered_by_time") {
			secondOrder = "runInterval.uniqueID";
		}
		else { //by type
			secondOrder = "runInterval.type, " + "runInterval.uniqueID";
		}
		
		
		dbcon.Open();
		dbcmd.CommandText = "SELECT person.name, runInterval.* " +
			" FROM person, runInterval " +
			" WHERE person.uniqueID == runInterval.personID" + 
			" AND runInterval.sessionID == " + sessionID + 
			" ORDER BY person.uniqueID, " + secondOrder;
		
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);

		int count = new int();
		count = 0;

		while(reader.Read()) {
			myArray.Add (reader[0].ToString() + ":" +	//person.name
					reader[1].ToString() + ":" +	//runInterval.uniqueID
					reader[2].ToString() + ":" + 	//runInterval.personID
					reader[3].ToString() + ":" + 	//runInterval.sessionID
					reader[4].ToString() + ":" + 	//runInterval.type
					reader[5].ToString() + ":" + 	//distanceTotal
					reader[6].ToString() + ":" + 	//timeTotal
					reader[7].ToString() + ":" + 	//distanceInterval
					reader[8].ToString() + ":" + 	//intervalTimesString
					reader[9].ToString() + ":" + 	//tracks
					reader[10].ToString() + ":" + 	//description
					reader[11].ToString() 	 	//limited
					);
			count ++;
		}

		reader.Close();
		dbcon.Close();

		string [] myRuns = new string[count];
		count =0;
		foreach (string line in myArray) {
			myRuns [count++] = line;
		}

		return myRuns;
	}

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
				Convert.ToDouble( reader[4].ToString() ),
				Convert.ToDouble( reader[5].ToString() ),
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
				Convert.ToDouble(reader[4]),	//distanceTotal
				Convert.ToDouble(reader[5]),	//timeTotal
				Convert.ToDouble(reader[6]),	//distanceInterval
				reader[7].ToString(),		//intervalTimesString
				Convert.ToDouble(reader[8]),		//tracks
				reader[9].ToString(), 		//description
				reader[10].ToString() 		//limited
				);

		return myRun;
	}

	public static void Update(int runID, int personID, string description)
	{
		dbcon.Open();
		dbcmd.CommandText = "UPDATE run " + 
			" SET personID = " + personID + 
			", description = '" + description +
			"' WHERE uniqueID == " + runID ;
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		dbcon.Close();
	}

	public static void IntervalUpdate(int runID, int personID, string description)
	{
		dbcon.Open();
		dbcmd.CommandText = "UPDATE runInterval " + 
			" SET personID = " + personID + 
			", description = '" + description +
			"' WHERE uniqueID == " + runID ;
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		dbcon.Close();
	}

	public static void Delete(string uniqueID)
	{
		dbcon.Open();
		dbcmd.CommandText = "Delete FROM run WHERE uniqueID == " + uniqueID;
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		dbcon.Close();
	}

	public static void IntervalDelete(string uniqueID)
	{
		dbcon.Open();
		dbcmd.CommandText = "Delete FROM runInterval WHERE uniqueID == " + uniqueID;
		dbcmd.ExecuteNonQuery();
		dbcon.Close();
	}
}
