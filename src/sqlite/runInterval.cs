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
 * Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using System.IO;
using System.Collections; //ArrayList
using System.Collections.Generic; //List
using Mono.Data.Sqlite;


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
			"simulated INT, " +
			"initialSpeed INT, " +
			"datetime TEXT )";
		dbcmd.ExecuteNonQuery();
	}

	public static int Insert(bool dbconOpened, string tableName, string uniqueID, int personID, int sessionID, string type, double distanceTotal, double timeTotal, double distanceInterval, string intervalTimesString, double tracks, string description, string limited, int simulated, bool initialSpeed, string datetime)
	{
		if(! dbconOpened)
			Sqlite.Open();

		if(uniqueID == "-1")
			uniqueID = "NULL";

		dbcmd.CommandText = "INSERT INTO "+ tableName + 
				" (uniqueID, personID, sessionID, type, distanceTotal, timeTotal, distanceInterval, intervalTimesString, tracks, description, limited, simulated, initialSpeed, datetime)" +
				"VALUES (" + uniqueID + ", " +
				personID + ", " + sessionID + ", \"" + type + "\", " +
				Util.ConvertToPoint(distanceTotal) + ", " + 
				Util.ConvertToPoint(timeTotal) + ", " + 
				Util.ConvertToPoint(distanceInterval) + ", \"" + 
				Util.ConvertToPoint(intervalTimesString) + "\", " +
				Util.ConvertToPoint(tracks) + ", \"" + 
				description + "\", \"" + limited + "\", " + simulated + ", " +
				Util.BoolToInt(initialSpeed) + ", \"" +
				datetime + "\")";
				
		LogB.SQL(dbcmd.CommandText.ToString());
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

        //like SelectRunsSA below method but much better: return list of RunInterval
	public static List<RunInterval> SelectRuns (bool dbconOpened, int sessionID, int personID, string runType,
			Orders_by order, int limit, bool personNameInComment)
	{
		if(!dbconOpened)
			Sqlite.Open();

		//runs previous to DB 2.13 have no datetime on run, runI
		//find session datetime for that runs
		List<Session> session_l = SqliteSession.SelectAll(true, Sqlite.Orders_by.DEFAULT);

		dbcmd.CommandText = selectCreateSelection (Constants.RunIntervalTable,
				sessionID, personID, runType, order, limit, false);

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		List<RunInterval> ri_l = new List<RunInterval>();

		while(reader.Read())
		{
			RunInterval runI = new RunInterval(
					Convert.ToInt32(reader[1].ToString()),	//runInterval.uniqueID
					Convert.ToInt32(reader[2].ToString()), 	//runInterval.personID
					Convert.ToInt32(reader[3].ToString()), 	//runInterval.sessionID
					reader[4].ToString(), 	//runInterval.type
					Convert.ToDouble(Util.ChangeDecimalSeparator(reader[5].ToString())), //distanceTotal
					Convert.ToDouble(Util.ChangeDecimalSeparator(reader[6].ToString())), //timeTotal
					Convert.ToDouble(Util.ChangeDecimalSeparator(reader[7].ToString())), //distanceInterval
					Util.ChangeDecimalSeparator(reader[8].ToString()), //intervalTimesString
					Convert.ToDouble(Util.ChangeDecimalSeparator(reader[9].ToString())), //tracks
					reader[10].ToString(), 	//description
					reader[11].ToString(), 	//limited
					Convert.ToInt32(reader[12].ToString()),	//simulated
					Util.IntToBool(Convert.ToInt32(reader[13])), //initialSpeed
					reader[14].ToString() 		//datetime
					);

			//runs previous to DB 2.13 have no datetime on run
			//find session datetime for that runs
			if(runI.Datetime == "")
			{
				bool found = false;
				foreach(Session session in session_l)
				{
					if(session.UniqueID == runI.SessionID)
					{
						runI.Datetime = UtilDate.ToFile(session.Date);
						found = true;
						break;
					}

				}
				//on really old versions of Chronojump, deleting a session maybe does not delete the runs
				//so could be able to found a run without a session, so assign here the MinValue possible of DateTime
				if(! found)
					runI.Datetime = UtilDate.ToFile(DateTime.MinValue);
			}

			if(personNameInComment)
				runI.Description = reader[0].ToString();

			ri_l.Add(runI);
		}

		reader.Close();

		if(!dbconOpened)
			Sqlite.Close();

		return ri_l;
	}

	//method that retruns an string array
	public static string[] SelectRunsSA (bool dbconOpened, int sessionID, int personID, string runType)
	{
		if(!dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = selectCreateSelection (Constants.RunIntervalTable,
				sessionID, personID, runType, Orders_by.DEFAULT, -1, false);
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);

		int count = new int();
		count = 0;

		while(reader.Read())
		{
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
					reader[12].ToString() + ":" +	//simulated
					Util.IntToBool(Convert.ToInt32(reader[13])) + ":" + //initialSpeed
					reader[14].ToString() 		//datetime
					);
			count ++;
		}

		reader.Close();
		
		if(!dbconOpened)
			Sqlite.Close();

		string [] myRuns = new string[count];
		count =0;
		foreach (string line in myArray) {
			myRuns [count++] = line;
		}

		return myRuns;
	}

	public static RunInterval SelectRunData(string tableName, int uniqueID, bool dbconOpened)
	{
		//tableName can be runInterval or tempRunInterval

		if(!dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "SELECT * FROM " + tableName + " WHERE uniqueID == " + uniqueID;
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		reader.Read();

		RunInterval myRun = new RunInterval(DataReaderToStringArray(reader, 14));

		reader.Close();
		if(!dbconOpened)
			Sqlite.Close();
		return myRun;
	}

	public static void Update(int runID, int personID, string description)
	{
		Sqlite.Open();
		dbcmd.CommandText = "UPDATE " + Constants.RunIntervalTable +
			" SET personID = " + personID + 
			", description = \"" + description +
			"\" WHERE uniqueID == " + runID ;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		Sqlite.Close();
	}


}
