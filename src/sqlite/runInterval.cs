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
 * Copyright (C) 2004-2023   Xavier de Blas <xaviblas@gmail.com>
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
			"datetime TEXT, " +
			"photocellStr TEXT )";
		dbcmd.ExecuteNonQuery();
	}

	public static int Insert(bool dbconOpened, string tableName, string uniqueID, int personID, int sessionID, string type, double distanceTotal, double timeTotal, double distanceInterval, string intervalTimesString, double tracks, string description, string limited, int simulated, bool initialSpeed, string datetime, List<int> photocell_l)
	{
		if(! dbconOpened)
			Sqlite.Open();

		if(uniqueID == "-1")
			uniqueID = "NULL";

		dbcmd.CommandText = "INSERT INTO "+ tableName + 
				" (uniqueID, personID, sessionID, type, distanceTotal, timeTotal, distanceInterval, intervalTimesString, tracks, description, limited, simulated, initialSpeed, datetime, photocellStr)" +
				"VALUES (" + uniqueID + ", " +
				personID + ", " + sessionID + ", \"" + type + "\", " +
				Util.ConvertToPoint(distanceTotal) + ", " + 
				Util.ConvertToPoint(timeTotal) + ", " + 
				Util.ConvertToPoint(distanceInterval) + ", \"" + 
				Util.ConvertToPoint(intervalTimesString) + "\", " +
				Util.ConvertToPoint(tracks) + ", \"" + 
				description + "\", \"" + limited + "\", " + simulated + ", " +
				Util.BoolToInt(initialSpeed) + ", \"" +
				datetime + "\", \"" +
				Util.ListIntToSQLString (photocell_l, ";") + "\")";
				
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
	// limit 0 means no limit (limit negative is the last results) (used on SelectRuns)
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
					reader[14].ToString(), 		//datetime
					Util.SQLStringToListInt(reader[15].ToString(), ";")
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

		//get last values on negative limit
		if (limit < 0 && ri_l.Count + limit >= 0)
			ri_l = ri_l.GetRange (ri_l.Count + limit, -1 * limit);

		return ri_l;
	}

	//method that retruns an string array
	public static string[] SelectRunsSA (bool dbconOpened, int sessionID, int personID, string runType)
	{
		if(!dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = selectCreateSelection (Constants.RunIntervalTable,
				sessionID, personID, runType, Orders_by.DEFAULT, 0, false);
		
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
					reader[14].ToString() + ":" + 		//datetime
					Util.SQLStringToListInt(reader[15].ToString(), ";")
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

	//table can be runInterval or tempRunInterval
	public static RunInterval SelectRunData(string table, int uniqueID, bool personNameInComment, bool dbconOpened)
	{
		if(!dbconOpened)
			Sqlite.Open();

		if(personNameInComment)
		{
			string tp = Constants.PersonTable;
			dbcmd.CommandText = "SELECT " + table + ".*, " + tp + ".Name" +
				" FROM " + table + ", " + tp +
				" WHERE " + table + ".personID = " + tp + ".uniqueID" +
				" AND " + table + ".uniqueID = " + uniqueID;
		} else
		dbcmd.CommandText = "SELECT * FROM " + table + " WHERE uniqueID == " + uniqueID;
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		reader.Read();

		RunInterval myRun = new RunInterval(DataReaderToStringArray(reader, 15));
		if(personNameInComment)
			myRun.Description = reader[15].ToString(); //person.Name

		reader.Close();
		if(!dbconOpened)
			Sqlite.Close();
		return myRun;
	}

	public static void Update (int runID, double distanceInterval, double tracks, string distancesString, int personID, string description)
	{
		Sqlite.Open();
		dbcmd.CommandText = "UPDATE " + Constants.RunIntervalTable +
			" SET personID = " + personID + 
			", distanceInterval = " + Util.ConvertToPoint(distanceInterval) +
			", distanceTotal = " + Util.ConvertToPoint (Util.GetRunITotalDistance (distanceInterval, distancesString, tracks)) +
			", description = \"" + description +
			"\" WHERE uniqueID == " + runID ;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		Sqlite.Close();
	}


}
