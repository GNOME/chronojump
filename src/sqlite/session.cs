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
 * Copyright (C) 2004-2015   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using System.IO;
using System.Collections; //ArrayList
using Mono.Data.Sqlite;
using Mono.Unix;


class SqliteSession : Sqlite
{
	public SqliteSession() {
	}
	
	~SqliteSession() {}

	//can be "Constants.SessionTable" or "Constants.ConvertTempTable"
	//temp is used to modify table between different database versions if needed
	//protected new internal static void createTable(string tableName)
	protected override void createTable(string tableName)
	{
		dbcmd.CommandText = 
			"CREATE TABLE " + tableName + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"name TEXT, " +
			"place TEXT, " +
			"date TEXT, " +	 //YYYY-MM-DD since db 0.72	
			"personsSportID INT, " + 
			"personsSpeciallityID INT, " + 
			"personsPractice INT, " + //also called "level"
			"comments TEXT, " +
			"serverUniqueID INT " +
			" ) ";
		dbcmd.ExecuteNonQuery();
	}
	
	public static int Insert(bool dbconOpened, string tableName, string uniqueID, string name, string place, DateTime date, int personsSportID, int personsSpeciallityID, int personsPractice, string comments, int serverUniqueID)
	{
		if(! dbconOpened)
			Sqlite.Open();

		if(uniqueID == "-1")
			uniqueID = "NULL";

		dbcmd.CommandText = "INSERT INTO " + tableName + " (uniqueID, name, place, date, personsSportID, personsSpeciallityID, personsPractice, comments, serverUniqueID)" +
			" VALUES (" + uniqueID + ", '"
			+ name + "', '" + place + "', '" + UtilDate.ToSql(date) + "', " + 
			personsSportID + ", " + personsSpeciallityID + ", " + 
			personsPractice + ", '" + comments + "', " +
			serverUniqueID + ")" ;
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

	public static void Update(int uniqueID, string name, string place, DateTime date, int personsSportID, int personsSpeciallityID, int personsPractice, string comments) 
	{
		//TODO: serverUniqueID (but cannot be changed in gui/edit, then not need now)
		Sqlite.Open();
		dbcmd.CommandText = "UPDATE " + Constants.SessionTable + " " +
			" SET name = '" + name +
			"' , date = '" + UtilDate.ToSql(date) +
			"' , place = '" + place +
			"' , personsSportID = " + personsSportID +
			", personsSpeciallityID = " + personsSpeciallityID +
			", personsPractice = " + personsPractice +
			", comments = '" + comments +
			"' WHERE uniqueID == " + uniqueID;
		dbcmd.ExecuteNonQuery();
		Sqlite.Close();
	}
	
	//updating local session when it gets uploaded
	public static void UpdateServerUniqueID(int uniqueID, int serverID)
	{
		//if(!dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "UPDATE " +Constants.SessionTable + " SET serverUniqueID = " + serverID + 
			" WHERE uniqueID == " + uniqueID ;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		//if(!dbconOpened)
			Sqlite.Close();
	}


	//by name (only in gui/networks.cs configInit
	//be careful because name is not unique
	public static Session SelectByName(string name)
	{
		dbcmd.CommandText = "SELECT * FROM " + Constants.SessionTable + " WHERE name == '" + name + "'"; 
		return SelectDo(dbcmd);
	}
	//by ID (default
	public static Session Select(string myUniqueID)
	{
		dbcmd.CommandText = "SELECT * FROM " + Constants.SessionTable + " WHERE uniqueID == " + myUniqueID ; 
		return SelectDo(dbcmd);
	}
	public static Session SelectDo(SqliteCommand mydbcmd)
	{
		try {
			Sqlite.Open();
		} catch {
			//done because there's an eventual problem maybe thread related on very few starts of chronojump
			LogB.SQL("Catched dbcon problem at Session.Select");
			Sqlite.Close();
			Sqlite.Open();
			LogB.SQL("reopened again");
		}
		LogB.SQL(mydbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = mydbcmd.ExecuteReader();
	
		string [] values = new string[9];
		
		while(reader.Read()) {
			values[0] = reader[0].ToString(); 
			values[1] = reader[1].ToString(); 
			values[2] = reader[2].ToString();
			values[3] = reader[3].ToString();
			values[4] = reader[4].ToString();
			values[5] = reader[5].ToString();
			values[6] = reader[6].ToString();
			values[7] = reader[7].ToString();
			values[8] = reader[8].ToString();
		}

		Session mySession = new Session(values[0], 
			values[1], values[2], UtilDate.FromSql(values[3]), 
			Convert.ToInt32(values[4]), Convert.ToInt32(values[5]), Convert.ToInt32(values[6]), 
			values[7], Convert.ToInt32(values[8]) );
		
		reader.Close();
		Sqlite.Close();
		return mySession;
	}
	
	//used by the stats selector of sessions
	//also by PersonsRecuperateFromOtherSessionWindowBox (src/gui/person.cs)
	public static string[] SelectAllSessionsSimple(bool commentsDisable, int sessionIdDisable) 
	{
		string selectString = " uniqueID, name, place, date, comments ";
		if(commentsDisable) {
			selectString = " uniqueID, name, place, date ";
		}
		
		Sqlite.Open();
		//dbcmd.CommandText = "SELECT * FROM session ORDER BY uniqueID";
		dbcmd.CommandText = "SELECT " + selectString + " FROM " + Constants.SessionTable + " " + 
			" WHERE uniqueID != " + sessionIdDisable + " ORDER BY uniqueID";
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		ArrayList myArray = new ArrayList(2);

		int count = new int();
		count = 0;

		while(reader.Read()) {
			if(commentsDisable) {
				myArray.Add (reader[0].ToString() + ":" + reader[1].ToString() + ":" +
						reader[2].ToString() + ":" + UtilDate.FromSql(reader[3].ToString()).ToShortDateString() );
			} else {
				myArray.Add (reader[0].ToString() + ":" + reader[1].ToString() + ":" +
						reader[2].ToString() + ":" + UtilDate.FromSql(reader[3].ToString()).ToShortDateString() + ":" +
						reader[4].ToString() );
			}
			count ++;
		}

		reader.Close();
	
		//close database connection
		Sqlite.Close();

		string [] mySessions = new string[count];
		count =0;
		foreach (string line in myArray) {
			mySessions [count++] = line;
		}

		return mySessions;
	}


	public static string[] SelectAllSessions(string filterName) 
	{
		Sqlite.Open();

		string filterNameString = "";
		if(filterName != "")
			filterNameString = " AND LOWER(session.name) LIKE LOWER ('%" + filterName  + "%') ";

		dbcmd.CommandText = 
			"SELECT session.*, sport.name, speciallity.name" +
			" FROM session, sport, speciallity " +
			" WHERE session.personsSportID == sport.uniqueID " + 
			" AND session.personsSpeciallityID == speciallity.UniqueID " +
			filterNameString + 
			" ORDER BY session.uniqueID";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		ArrayList myArray = new ArrayList(2);

		int count = new int();
		count = 0;

		while(reader.Read()) {
			string sportName = Catalog.GetString(reader[9].ToString());
			string speciallityName = ""; //to solve a gettext bug (probably because speciallity undefined name is "")			
			if(reader[10].ToString() != "")
				speciallityName = Catalog.GetString(reader[10].ToString());
			string levelName = Catalog.GetString(Util.FindLevelName(Convert.ToInt32(reader[6])));

			myArray.Add (reader[0].ToString() + ":" + reader[1].ToString() + ":" +
					reader[2].ToString() + ":" + UtilDate.FromSql(reader[3].ToString()).ToShortDateString() + ":" +
					sportName + ":" + speciallityName + ":" +
					levelName + ":" + reader[7].ToString() ); //desc
			count ++;
		}

		reader.Close();

		/* FIXME:
		 * all this thing it's because if someone has createds sessions without jumps or jumpers,
		 * and we make a GROUP BY selection, this sessions doesn't appear as results
		 * in the near future, learn better sqlite for solving this in a nicer way
		 * */
		/* another solution is not show nothing about jumpers and jumps, but show a button of "details"
		 * this will open a new window showing this values.
		 * this solution it's more "lighter" for people who have  abig DB
		 * */
		
		//select persons of each session
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM " + Constants.PersonSessionTable + 
			" GROUP BY sessionID ORDER BY sessionID";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader_persons;
		reader_persons = dbcmd.ExecuteReader();
		ArrayList myArray_persons = new ArrayList(2);
		
		while(reader_persons.Read()) {
			myArray_persons.Add (reader_persons[0].ToString() + ":" + reader_persons[1].ToString() + ":" );
		}
		reader_persons.Close();
		
		//select jumps of each session
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM " + Constants.JumpTable + 
			" GROUP BY sessionID ORDER BY sessionID";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader_jumps;
		reader_jumps = dbcmd.ExecuteReader();
		ArrayList myArray_jumps = new ArrayList(2);
		
		while(reader_jumps.Read()) {
			myArray_jumps.Add (reader_jumps[0].ToString() + ":" + reader_jumps[1].ToString() + ":" );
		}
		reader_jumps.Close();
		
		//select jumpsRj of each session
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM " + Constants.JumpRjTable + 
			" GROUP BY sessionID ORDER BY sessionID";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader_jumpsRj;
		reader_jumpsRj = dbcmd.ExecuteReader();
		ArrayList myArray_jumpsRj = new ArrayList(2);
		
		while(reader_jumpsRj.Read()) {
			myArray_jumpsRj.Add (reader_jumpsRj[0].ToString() + ":" + reader_jumpsRj[1].ToString() + ":" );
		}
		reader_jumpsRj.Close();
		
		//select runs of each session
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM " + Constants.RunTable + 
			" GROUP BY sessionID ORDER BY sessionID";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader_runs;
		reader_runs = dbcmd.ExecuteReader();
		ArrayList myArray_runs = new ArrayList(2);
		
		while(reader_runs.Read()) {
			myArray_runs.Add (reader_runs[0].ToString() + ":" + reader_runs[1].ToString() + ":" );
		}
		reader_runs.Close();
		
		//select runsInterval of each session
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM " + Constants.RunIntervalTable + 
			" GROUP BY sessionID ORDER BY sessionID";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader_runs_interval;
		reader_runs_interval = dbcmd.ExecuteReader();
		ArrayList myArray_runs_interval = new ArrayList(2);
		
		while(reader_runs_interval.Read()) {
			myArray_runs_interval.Add (reader_runs_interval[0].ToString() + ":" + reader_runs_interval[1].ToString() + ":" );
		}
		reader_runs_interval.Close();
	
		//select reaction time of each session
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM " + Constants.ReactionTimeTable + 
			" GROUP BY sessionID ORDER BY sessionID";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader_rt;
		reader_rt = dbcmd.ExecuteReader();
		ArrayList myArray_rt = new ArrayList(2);
		
		while(reader_rt.Read()) {
			myArray_rt.Add (reader_rt[0].ToString() + ":" + reader_rt[1].ToString() + ":" );
		}
		reader_rt.Close();
	
		//select pulses of each session
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM " + Constants.PulseTable + 
			" GROUP BY sessionID ORDER BY sessionID";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader_pulses;
		reader_pulses = dbcmd.ExecuteReader();
		ArrayList myArray_pulses = new ArrayList(2);
		
		while(reader_pulses.Read()) {
			myArray_pulses.Add (reader_pulses[0].ToString() + ":" + reader_pulses[1].ToString() + ":" );
		}
		reader_pulses.Close();
	
		//select multichronopic of each session
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM " + Constants.MultiChronopicTable + 
			" GROUP BY sessionID ORDER BY sessionID";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader_mcs;
		reader_mcs = dbcmd.ExecuteReader();
		ArrayList myArray_mcs = new ArrayList(2);
		
		while(reader_mcs.Read()) {
			myArray_mcs.Add (reader_mcs[0].ToString() + ":" + reader_mcs[1].ToString() + ":" );
		}
		reader_mcs.Close();
	
		//select encoder signal of each session
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM " + Constants.EncoderTable + 
			" WHERE signalOrCurve == 'signal' GROUP BY sessionID ORDER BY sessionID";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader_enc_s;
		reader_enc_s = dbcmd.ExecuteReader();
		ArrayList myArray_enc_s = new ArrayList(2);
		
		while(reader_enc_s.Read()) {
			myArray_enc_s.Add (reader_enc_s[0].ToString() + ":" + reader_enc_s[1].ToString() + ":" );
		}
		reader_enc_s.Close();
	
		//select encoder curve of each session
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM " + Constants.EncoderTable + 
			" WHERE signalOrCurve == 'curve' GROUP BY sessionID ORDER BY sessionID";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader_enc_c;
		reader_enc_c = dbcmd.ExecuteReader();
		ArrayList myArray_enc_c = new ArrayList(2);
		
		while(reader_enc_c.Read()) {
			myArray_enc_c.Add (reader_enc_c[0].ToString() + ":" + reader_enc_c[1].ToString() + ":" );
		}
		reader_enc_c.Close();
	
		
		//close database connection
		Sqlite.Close();

		//mix seven arrayLists
		string [] mySessions = new string[count];
		count =0;
		bool found;
		foreach (string line in myArray) {
			string lineNotReadOnly = line;

			//if some sessions are deleted, do not use count=0 to mix arrays, use sessionID of line
			string [] mixingSessionFull = line.Split(new char[] {':'});
			string mixingSessionID = mixingSessionFull[0];
			
			//add persons for each session	
			found = false;
			foreach (string line_persons in myArray_persons) {
				string [] myStringFull = line_persons.Split(new char[] {':'});
				if(myStringFull[0] == mixingSessionID) {
					lineNotReadOnly  = lineNotReadOnly + ":" + myStringFull[1];
					found = true;
				}
			}
			if (!found) { lineNotReadOnly  = lineNotReadOnly + ":0"; }
		
			//add jumps for each session
			found = false;
			foreach (string line_jumps in myArray_jumps) {
				string [] myStringFull = line_jumps.Split(new char[] {':'});
				if(myStringFull[0] == mixingSessionID) {
					lineNotReadOnly  = lineNotReadOnly + ":" + myStringFull[1];
					found = true;
				}
			}
			if (!found) { lineNotReadOnly  = lineNotReadOnly + ":0"; }
			
			//add jumpsRj for each session
			found = false;
			foreach (string line_jumpsRj in myArray_jumpsRj) {
				string [] myStringFull = line_jumpsRj.Split(new char[] {':'});
				if(myStringFull[0] == mixingSessionID) {
					lineNotReadOnly  = lineNotReadOnly + ":" + myStringFull[1];
					found = true;
				}
			}
			if (!found) { lineNotReadOnly  = lineNotReadOnly + ":0"; }
			
			//add runs for each session
			found = false;
			foreach (string line_runs in myArray_runs) {
				string [] myStringFull = line_runs.Split(new char[] {':'});
				if(myStringFull[0] == mixingSessionID) {
					lineNotReadOnly  = lineNotReadOnly + ":" + myStringFull[1];
					found = true;
				}
			}
			if (!found) { lineNotReadOnly  = lineNotReadOnly + ":0"; }
			
			//add runsInterval for each session
			found = false;
			foreach (string line_runs_interval in myArray_runs_interval) {
				string [] myStringFull = line_runs_interval.Split(new char[] {':'});
				if(myStringFull[0] == mixingSessionID) {
					lineNotReadOnly  = lineNotReadOnly + ":" + myStringFull[1];
					found = true;
				}
			}
			if (!found) { lineNotReadOnly  = lineNotReadOnly + ":0"; }
			
			//add reaction time for each session
			found = false;
			foreach (string line_rt in myArray_rt) {
				string [] myStringFull = line_rt.Split(new char[] {':'});
				if(myStringFull[0] == mixingSessionID) {
					lineNotReadOnly  = lineNotReadOnly + ":" + myStringFull[1];
					found = true;
				}
			}
			if (!found) { lineNotReadOnly  = lineNotReadOnly + ":0"; }

			//add pulses for each session
			found = false;
			foreach (string line_pulses in myArray_pulses) {
				string [] myStringFull = line_pulses.Split(new char[] {':'});
				if(myStringFull[0] == mixingSessionID) {
					lineNotReadOnly  = lineNotReadOnly + ":" + myStringFull[1];
					found = true;
				}
			}
			if (!found) { lineNotReadOnly  = lineNotReadOnly + ":0"; }

			//add multiChronopic for each session
			found = false;
			foreach (string line_mcs in myArray_mcs) {
				string [] myStringFull = line_mcs.Split(new char[] {':'});
				if(myStringFull[0] == mixingSessionID) {
					lineNotReadOnly  = lineNotReadOnly + ":" + myStringFull[1];
					found = true;
				}
			}
			if (!found) { lineNotReadOnly  = lineNotReadOnly + ":0"; }

			//add encoder signal for each session
			found = false;
			foreach (string line_enc_s in myArray_enc_s) {
				string [] myStringFull = line_enc_s.Split(new char[] {':'});
				if(myStringFull[0] == mixingSessionID) {
					lineNotReadOnly  = lineNotReadOnly + ":" + myStringFull[1];
					found = true;
				}
			}
			if (!found) { lineNotReadOnly  = lineNotReadOnly + ":0"; }

			//add encoder curve for each session
			found = false;
			foreach (string line_enc_c in myArray_enc_c) {
				string [] myStringFull = line_enc_c.Split(new char[] {':'});
				if(myStringFull[0] == mixingSessionID) {
					lineNotReadOnly  = lineNotReadOnly + ":" + myStringFull[1];
					found = true;
				}
			}
			if (!found) { lineNotReadOnly  = lineNotReadOnly + ":0"; }

			
			mySessions [count++] = lineNotReadOnly;
		}

		return mySessions;
	}


	//called from gui/event.cs for doing the graph
	//we need to know the avg of events of a type (SJ, CMJ, free (pulse).. of a person, or of all persons on the session
	public static double SelectAVGEventsOfAType(bool dbconOpened, int sessionID, int personID, string table, string type, string valueToSelect) 
	{
		if(!dbconOpened)
			Sqlite.Open();

		//if personIDString == -1, the applies for all persons
		
		string personIDString = "";
		if(personID != -1)
			personIDString = " AND personID == " + personID; 

		
		dbcmd.CommandText = "SELECT AVG(" + valueToSelect + ")" +
			" FROM " + table +				
			" WHERE sessionID == " + sessionID + 
			" AND type == '" + type + "' " +
			personIDString; 
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		double myReturn = 0;
		bool found = false;
		if(reader.Read()) {
			found = true;
			myReturn = Convert.ToDouble(Util.ChangeDecimalSeparator(reader[0].ToString()));
		}
		reader.Close();
		
		if(!dbconOpened)
			Sqlite.Close();

		if (found) {
			return myReturn;
		} else {
			return 0;
		}
	}

	
	public static void DeleteAllStuff(string uniqueID)
	{
		Sqlite.Open();

		//delete the session
		dbcmd.CommandText = "Delete FROM " + Constants.SessionTable + " WHERE uniqueID == " + uniqueID;
		dbcmd.ExecuteNonQuery();
		
		//delete relations (existance) within persons and sessions in this session
		dbcmd.CommandText = "Delete FROM " + Constants.PersonSessionTable + " WHERE sessionID == " + uniqueID;
		dbcmd.ExecuteNonQuery();

		Sqlite.deleteOrphanedPersons();
		
		//delete normal jumps
		dbcmd.CommandText = "Delete FROM " + Constants.JumpTable + " WHERE sessionID == " + uniqueID;
		dbcmd.ExecuteNonQuery();
		
		//delete repetitive jumps
		dbcmd.CommandText = "Delete FROM " + Constants.JumpRjTable + " WHERE sessionID == " + uniqueID;
		dbcmd.ExecuteNonQuery();
		
		//delete normal runs
		dbcmd.CommandText = "Delete FROM " + Constants.RunTable + " WHERE sessionID == " + uniqueID;
		dbcmd.ExecuteNonQuery();
		
		//delete intervallic runs
		dbcmd.CommandText = "Delete FROM " + Constants.RunIntervalTable + " WHERE sessionID == " + uniqueID;
		dbcmd.ExecuteNonQuery();
		
		//delete reaction times
		dbcmd.CommandText = "Delete FROM " + Constants.ReactionTimeTable + " WHERE sessionID == " + uniqueID;
		dbcmd.ExecuteNonQuery();
		
		//delete pulses
		dbcmd.CommandText = "Delete FROM " + Constants.PulseTable + " WHERE sessionID == " + uniqueID;
		dbcmd.ExecuteNonQuery();
		
		//delete multiChronopic
		dbcmd.CommandText = "Delete FROM " + Constants.MultiChronopicTable + " WHERE sessionID == " + uniqueID;
		dbcmd.ExecuteNonQuery();
		
		//delete from encoder start ------>

		//signals
		ArrayList encoderArray = SqliteEncoder.Select(
				true, -1, -1, Convert.ToInt32(uniqueID), -1,
				"signal", EncoderSQL.Eccons.ALL,
				false, true);
		
		foreach(EncoderSQL eSQL in encoderArray) {
			Util.FileDelete(eSQL.GetFullURL(false));	//signal, don't convertPathToR
			if(eSQL.videoURL != "")
				Util.FileDelete(eSQL.videoURL);		//video
			Sqlite.Delete(true, Constants.EncoderTable, Convert.ToInt32(eSQL.uniqueID));
		}
		
		//curves
		encoderArray = SqliteEncoder.Select(
				true, -1, -1, Convert.ToInt32(uniqueID), -1,
				"curve",  EncoderSQL.Eccons.ALL,
				false, true);
		
		foreach(EncoderSQL eSQL in encoderArray) {
			Util.FileDelete(eSQL.GetFullURL(false));	//don't convertPathToR
			/* commented: curve has no video
			if(eSQL.videoURL != "")
				Util.FileDelete(eSQL.videoURL);
			*/
			Sqlite.Delete(true, Constants.EncoderTable, Convert.ToInt32(eSQL.uniqueID));
			SqliteEncoder.DeleteSignalCurveWithCurveID(true, Convert.ToInt32(eSQL.uniqueID));
		}
		
		//<------- delete from encoder end
				
		
		Sqlite.Close();
	}

}

class SqliteServerSession : SqliteSession
{
	public SqliteServerSession() {
	}
	
	protected override void createTable(string tableName)
	{
		string serverSpecificString = 
			", evaluatorID INT " +
			", evaluatorCJVersion TEXT " + 
			", evaluatorOS TEXT " +
			", uploadedDate TEXT " + //YYYY-MM-DD since db 0.72	
			", uploadingState INT ";

		dbcmd.CommandText = 
			"CREATE TABLE " + tableName + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"name TEXT, " +
			"place TEXT, " +
			"date TEXT, " +	//YYYY-MM-DD since db 0.72		
			"personsSportID INT, " + 
			"personsSpeciallityID INT, " + 
			"personsPractice INT, " + //also called "level"
			"comments TEXT, " +
			"serverUniqueID INT " +
			serverSpecificString + 
			" ) ";
		dbcmd.ExecuteNonQuery();
	}
	
	public static int Insert(bool dbconOpened, string tableName, string name, string place, DateTime date, int personsSportID, int personsSpeciallityID, int personsPractice, string comments, int serverUniqueID, int evaluatorID, string evaluatorCJVersion, string evaluatorOS, DateTime uploadedDate, int uploadingState)
	{
		if(! dbconOpened)
			Sqlite.Open();

		//(uniqueID == "-1")
		//	uniqueID = "NULL";
		string uniqueID = "NULL";

		dbcmd.CommandText = "INSERT INTO " + tableName + " (uniqueID, name, place, date, personsSportID, personsSpeciallityID, personsPractice, comments, serverUniqueID, evaluatorID, evaluatorCJVersion, evaluatorOS, uploadedDate, uploadingState)" +
			" VALUES (" + uniqueID + ", '"
			+ name + "', '" + place + "', '" + UtilDate.ToSql(date) + "', " + 
			personsSportID + ", " + personsSpeciallityID + ", " + 
			personsPractice + ", '" + comments + "', " +
			serverUniqueID + ", " + evaluatorID + ", '" +
			evaluatorCJVersion + "', '" + evaluatorOS + "', '" +
			UtilDate.ToSql(uploadedDate) + "', " + uploadingState +
			")" ;
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
	
	//updating local session when it gets uploaded
	public static void UpdateUploadingState(int uniqueID, int state)
	{
		//if(!dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "UPDATE " + Constants.SessionTable + " SET uploadingState = " + state + 
			" WHERE uniqueID == " + uniqueID ;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		//if(!dbconOpened)
			Sqlite.Close();
	}

	
	~SqliteServerSession() {}
}
