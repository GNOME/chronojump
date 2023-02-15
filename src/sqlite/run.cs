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
using System.Collections.Generic; //List<T>
using Mono.Data.Sqlite;


class SqliteRun : Sqlite
{
	public SqliteRun() {
	}
	
	~SqliteRun() {}

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
			"type TEXT, " +
			"distance FLOAT, " +
			"time FLOAT, " +
			"description TEXT, " +
			"simulated INT, " +
			"initialSpeed INT, " +
			"datetime TEXT )";
		dbcmd.ExecuteNonQuery();
	}
	
	
	/*
	 * Run class methods
	 */
	
	public static int Insert(bool dbconOpened, string tableName, string uniqueID, int personID, int sessionID, string type, double distance, double time, string description, int simulated, bool initialSpeed, string datetime)
	{
		if(! dbconOpened)
			Sqlite.Open();
		
		if(uniqueID == "-1")
			uniqueID = "NULL";

		dbcmd.CommandText = "INSERT INTO " + tableName + 
				" (uniqueID, personID, sessionID, type, distance, time, description, simulated, initialSpeed, datetime)" +
				" VALUES (" + uniqueID + ", " +
				+ personID + ", " + sessionID + ", \"" + type + "\", "
				+ Util.ConvertToPoint(distance) + ", " + Util.ConvertToPoint(time) + ", \"" + 
				description + "\", " + simulated + ", " + Util.BoolToInt(initialSpeed) + ", \"" + datetime + "\")";
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

	//note this is selecting also the person.name
	//used on run and runI
	// limit 0 means no limit (limit negative is the last results) (used on SelectRuns)
	protected static string selectCreateSelection (string tableName,
			int sessionID, int personID, string filterType,
			Orders_by order, int limit, bool onlyBestInSession)
	{
		string t = tableName;
		string tp = Constants.PersonTable;

		string filterSessionString = "";
		if(sessionID != -1)
			filterSessionString = string.Format(" AND {0}.sessionID = {1}", t, sessionID);

		string filterPersonString = "";
		if(personID != -1)
			filterPersonString = string.Format(" AND {0}.uniqueID = {1}", tp, personID);

		string filterTypeString = "";
		if(filterType != "")
			filterTypeString = " AND " + t + ".type = \"" + filterType + "\" " ;

		string orderByString = string.Format(" ORDER BY upper({0}.name), {1}.uniqueID ", tp, t);
		if(order == Orders_by.ID_ASC)
			orderByString = string.Format(" ORDER BY {0}.uniqueID ", t);
		else if(order == Orders_by.ID_DESC)
			orderByString = string.Format(" ORDER BY {0}.uniqueID DESC ", t);
		if(onlyBestInSession)
			orderByString = string.Format(" ORDER BY {0}.sessionID, {0}.distance/{0}.time DESC ", t);

		string limitString = "";
		if(limit > 0)
			limitString = " LIMIT " + limit;

		return string.Format("SELECT {0}.name, {1}.* ", tp, t) +
			string.Format(" FROM {0}, {1} ", tp, t) +
			string.Format(" WHERE {0}.uniqueID = {1}.personID", tp, t) +
			filterSessionString +
			filterPersonString +
			filterTypeString +
			orderByString +
			limitString;
	}

	//like SelectRuns, but this returns a string[] :( better use below method if possible
	//if all sessions, put -1 in sessionID
	//if all persons, put -1 in personID
	//if all types, put "" in filterType
	// limit 0 means no limit (limit negative is the last results) (used on SelectRuns)
	//SA for String Array
	public static string[] SelectRunsSA (bool dbconOpened, int sessionID, int personID, string filterType,
			Orders_by order, int limit)
	{
		if(!dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = selectCreateSelection (Constants.RunTable,
				sessionID, personID, filterType, order, limit, false);
		
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
					reader[1].ToString() + ":" +	//run.uniqueID
					reader[2].ToString() + ":" + 	//run.personID
					reader[3].ToString() + ":" + 	//run.sessionID
					reader[4].ToString() + ":" + 	//run.type
					Util.ChangeDecimalSeparator(reader[5].ToString()) + ":" + //run.distance
					Util.ChangeDecimalSeparator(reader[6].ToString()) + ":" + //run.time
					reader[7].ToString() + ":" + 	//description
					reader[8].ToString() + ":" +	//simulated
					Util.IntToBool(Convert.ToInt32(reader[9])) + ":" + //initialSpeed
					reader[10].ToString() 		//datetime
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

        /*
         * like SelectRunsSA above method but much better: return list of Run
         * sID -1 means all sessions
         * pID -1 means all persons
         * runType "" means all runs
	 * limit 0 means no limit (limit negative is the last results)
         * personNameInComment is used to be able to display names in graphs
         *   because event.PersonName makes individual SQL SELECTs
         */

	public static List<Run> SelectRuns (bool dbconOpened, int sessionID, int personID, string runType,
			Orders_by order, int limit, bool personNameInComment, bool onlyBestInSession)
	{
		if(! dbconOpened)
			Sqlite.Open();

		//runs previous to DB 2.13 have no datetime on run, runI
		//find session datetime for that runs
		List<Session> session_l = SqliteSession.SelectAll(true, Sqlite.Orders_by.DEFAULT);


		dbcmd.CommandText = selectCreateSelection (Constants.RunTable, sessionID, personID, runType, order, limit, onlyBestInSession);
		LogB.SQL(dbcmd.CommandText.ToString());

		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		List<Run> run_l = new List<Run>();

		while(reader.Read())
		{
			Run run = new Run(
					Convert.ToInt32(reader[1].ToString()),	//run.uniqueID
					Convert.ToInt32(reader[2].ToString()), 	//run.personID
					Convert.ToInt32(reader[3].ToString()), 	//run.sessionID
					reader[4].ToString(), 	//run.type
					Convert.ToDouble(Util.ChangeDecimalSeparator(reader[5].ToString())), //run.distance
					Convert.ToDouble(Util.ChangeDecimalSeparator(reader[6].ToString())), //run.time
					reader[7].ToString(), 	//description
					Convert.ToInt32(reader[8].ToString()),	//simulated
					Util.IntToBool(Convert.ToInt32(reader[9])), //initialSpeed
					reader[10].ToString() 	//datetime
					);

			//runs previous to DB 2.13 have no datetime on run
			//find session datetime for that runs
			if(run.Datetime == "")
			{
				bool found = false;
				foreach(Session session in session_l)
				{
					if(session.UniqueID == run.SessionID)
					{
						run.Datetime = UtilDate.ToFile(session.Date);
						found = true;
						break;
					}

				}
				//on really old versions of Chronojump, deleting a session maybe does not delete the runs
				//so could be able to found a run without a session, so assign here the MinValue possible of DateTime
				if(! found)
					run.Datetime = UtilDate.ToFile(DateTime.MinValue);
			}

			if(personNameInComment)
				run.Description = reader[0].ToString();

			run_l.Add(run);
		}

		reader.Close();

		if(!dbconOpened)
			Sqlite.Close();

		//get last values on negative limit
		if (limit < 0 && run_l.Count + limit >= 0)
			run_l = run_l.GetRange (run_l.Count + limit, -1 * limit);

		return run_l;
	}

	public static Run SelectRunData(int uniqueID, bool dbconOpened)
	{
		if(!dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "SELECT * FROM " + Constants.RunTable + " WHERE uniqueID == " + uniqueID;
		
		LogB.SQL(dbcmd.CommandText.ToString());

		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		reader.Read();
	
		Run myRun = new Run(DataReaderToStringArray(reader, 10));
	
		reader.Close();
		if(!dbconOpened)
			Sqlite.Close();
		return myRun;
	}
		
	public static string [] SelectTestMaxStuff(int personID, RunType runType) 
	{
		Sqlite.Open();
		
		dbcmd.CommandText = "SELECT session.date, session.name, MAX(distance/time), run.simulated " + 
			" FROM run, session WHERE type = \"" + runType.Name + "\" AND personID = " + personID + 
			" AND run.sessionID = session.uniqueID";
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		reader.Read();
		
		string [] str = DataReaderToStringArray(reader, 4);
		
		reader.Close();
		Sqlite.Close();

		return str;
	}
	
	public static void Update(int runID, string type, double distance, string time, int personID, string description)
	{
		Sqlite.Open();
		dbcmd.CommandText = "UPDATE " + Constants.RunTable + 
			" SET personID = " + personID + 
			", type = \"" + type +
			"\", distance = " + Util.ConvertToPoint(distance) +
			", time = " + Util.ConvertToPoint(Convert.ToDouble(time)) + 
			", description = \"" + description +
			"\" WHERE uniqueID == " + runID ;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		Sqlite.Close();
	}

}

