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


class SqliteRunInterval : SqliteRun
{
	public SqliteRunInterval() {
	}
	
	~SqliteRunInterval() {}

	protected override void createTable(string tableName)
	{
		//values: 'runInterval' and 'tempRunInterval'

		dbcmd.CommandText = 
			"CREATE TABLE " + tableName  +
			" (uniqueID INTEGER PRIMARY KEY, " +
			"personID INT, " +
			"sessionID INT, " +
			"type TEXT, " +
			"distanceTotal FLOAT, " +
			"timeTotal FLOAT, " +
			"distanceInterval FLOAT, " +
			"intervalTimesString TEXT, " +
			"tracks FLOAT, " +	//float because if we limit by time (runType tracksLimited false), we do n.nn tracks
			"description TEXT, " +
			"limited TEXT, " +
			"simulated INT )";
		dbcmd.ExecuteNonQuery();
	}

	public static int Insert(bool dbconOpened, string tableName, string uniqueID, int personID, int sessionID, string type, double distanceTotal, double timeTotal, double distanceInterval, string intervalTimesString, double tracks, string description, string limited, int simulated )
	{
		if(! dbconOpened)
			dbcon.Open();

		dbcmd.CommandText = "INSERT INTO "+ tableName + 
				" (uniqueID, personID, sessionID, type, distanceTotal, timeTotal, distanceInterval, intervalTimesString, tracks, description, limited, simulated )" +
				"VALUES (" + uniqueID + ", " +
				personID + ", " + sessionID + ", '" + type + "', " +
				Util.ConvertToPoint(distanceTotal) + ", " + 
				Util.ConvertToPoint(timeTotal) + ", " + 
				Util.ConvertToPoint(distanceInterval) + ", '" + 
				Util.ConvertToPoint(intervalTimesString) + "', " +
				Util.ConvertToPoint(tracks) + ", '" + 
				description + "', '" + limited + "', " + simulated + ")" ;
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		int myLast = dbcon.LastInsertRowId;

		if(! dbconOpened)
			dbcon.Close();

		return myLast;
	}

	public new static string[] SelectAllRuns(int sessionID) 
	{
		dbcon.Open();
		dbcmd.CommandText = "SELECT person.name, runInterval.* " +
			" FROM person, runInterval " +
			" WHERE person.uniqueID == runInterval.personID" + 
			" AND runInterval.sessionID == " + sessionID + 
			" ORDER BY upper(person.name), runInterval.uniqueID";
		
		Log.WriteLine(dbcmd.CommandText.ToString());
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
					Util.ChangeDecimalSeparator(reader[5].ToString()) + ":" + //distanceTotal
					Util.ChangeDecimalSeparator(reader[6].ToString()) + ":" + //timeTotal
					Util.ChangeDecimalSeparator(reader[7].ToString()) + ":" + //distanceInterval
					Util.ChangeDecimalSeparator(reader[8].ToString()) + ":" + //intervalTimesString
					Util.ChangeDecimalSeparator(reader[9].ToString()) + ":" + //tracks
					reader[10].ToString() + ":" + 	//description
					reader[11].ToString() + ":" +  	//limited
					reader[12].ToString() 	 	//simulated
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

	public static RunInterval SelectRunData(string tableName, int uniqueID)
	{
		//tableName can be runInterval or tempRunInterval

		dbcon.Open();

		dbcmd.CommandText = "SELECT * FROM " + tableName + " WHERE uniqueID == " + uniqueID;
		
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		reader.Read();

		RunInterval myRun = new RunInterval(DataReaderToStringArray(reader, 12));

		dbcon.Close();
		return myRun;
	}

	public static void Update(int runID, int personID, string description)
	{
		dbcon.Open();
		dbcmd.CommandText = "UPDATE " + Constants.RunIntervalTable +
			" SET personID = " + personID + 
			", description = '" + description +
			"' WHERE uniqueID == " + runID ;
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		dbcon.Close();
	}


}
