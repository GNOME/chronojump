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


class SqliteSession : Sqlite
{
	protected internal static void createTable()
	{
		dbcmd.CommandText = 
			"CREATE TABLE session ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"name TEXT, " +
			"place TEXT, " +
			"date TEXT, " +		
			"comments TEXT )";		
		dbcmd.ExecuteNonQuery();
	}
	
	public static int Insert(string name, string place, string date, string comments)
	{
		dbcon.Open();
		dbcmd.CommandText = "INSERT INTO session (uniqueID, name, place, date, comments)" +
			" VALUES (NULL, '"
			+ name + "', '" + place + "', '" + date + "', '" + comments + "')" ;
		dbcmd.ExecuteNonQuery();
		int myReturn = dbcon.LastInsertRowId;
		dbcon.Close();
		return myReturn;
	}
	
	public static void Edit(int uniqueID, string name, string place, string date, string comments)
	{
		dbcon.Open();
		dbcmd.CommandText = "UPDATE session " +
			" SET name = '" + name +
			"' , date = '" + date +
			"' , place = '" + place +
			"' , comments = '" + comments +
			"' WHERE uniqueID == " + uniqueID;
		dbcmd.ExecuteNonQuery();
		dbcon.Close();
	}
	
	public static Session Select(string myUniqueID)
	{
		dbcon.Open();
		dbcmd.CommandText = "SELECT * FROM session WHERE uniqueID == " + myUniqueID ; 
		Console.WriteLine(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
	
		string [] values = new string[5];
		
		while(reader.Read()) {
			values[0] = reader[0].ToString(); 
			values[1] = reader[1].ToString(); 
			values[2] = reader[2].ToString();
			values[3] = reader[3].ToString();
			values[4] = reader[4].ToString();
		}

		Session mySession = new Session(values[0], 
			values[1], values[2], values[3], values[4]);
		
		return mySession;
	}
	
	//used by the stats selector of sessions
	//also by PersonsRecuperateFromOtherSessionWindowBox (src/gui/person.cs)
	public static string[] SelectAllSessionsSimple(bool commentsDisable, int sessionIdDisable) 
	{
		string selectString = "*";
		if(commentsDisable) {
			selectString = " uniqueID, name, place, date ";
		}
		
		dbcon.Open();
		//dbcmd.CommandText = "SELECT * FROM session ORDER BY uniqueID";
		dbcmd.CommandText = "SELECT " + selectString + " FROM session " + 
			" WHERE uniqueID != " + sessionIdDisable + " ORDER BY uniqueID";
		
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		ArrayList myArray = new ArrayList(2);

		int count = new int();
		count = 0;

		while(reader.Read()) {
			if(commentsDisable) {
				myArray.Add (reader[0].ToString() + ":" + reader[1].ToString() + ":" +
						reader[2].ToString() + ":" + reader[3].ToString() );
			} else {
				myArray.Add (reader[0].ToString() + ":" + reader[1].ToString() + ":" +
						reader[2].ToString() + ":" + reader[3].ToString() + ":" +
						reader[4].ToString() );
			}
			count ++;
		}

		reader.Close();
	
		//close database connection
		dbcon.Close();

		string [] mySessions = new string[count];
		count =0;
		foreach (string line in myArray) {
			mySessions [count++] = line;
		}

		return mySessions;
	}


	public static string[] SelectAllSessions() 
	{
		dbcon.Open();
		dbcmd.CommandText = "SELECT * FROM session ORDER BY uniqueID";
		/*dbcmd.CommandText = "SELECT session.*, count(*) " +
			"FROM session, jump " +
			" WHERE session.uniqueID == jump.sessionID " +
			" GROUP BY sessionID" + 
			" ORDER BY session.uniqueID";
			*/
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		ArrayList myArray = new ArrayList(2);

		int count = new int();
		count = 0;

		while(reader.Read()) {
			myArray.Add (reader[0].ToString() + ":" + reader[1].ToString() + ":" +
					reader[2].ToString() + ":" + reader[3].ToString() + ":" +
					reader[4].ToString() );
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
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM PERSONSESSION GROUP BY sessionID ORDER BY sessionID";
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader_persons;
		reader_persons = dbcmd.ExecuteReader();
		ArrayList myArray_persons = new ArrayList(2);
		
		while(reader_persons.Read()) {
			myArray_persons.Add (reader_persons[0].ToString() + ":" + reader_persons[1].ToString() + ":" );
		}
		reader_persons.Close();
		
		//select jumps of each session
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM JUMP GROUP BY sessionID ORDER BY sessionID";
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader_jumps;
		reader_jumps = dbcmd.ExecuteReader();
		ArrayList myArray_jumps = new ArrayList(2);
		
		while(reader_jumps.Read()) {
			myArray_jumps.Add (reader_jumps[0].ToString() + ":" + reader_jumps[1].ToString() + ":" );
		}
		reader_jumps.Close();
		
		//select jumpsRj of each session
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM JUMPRJ GROUP BY sessionID ORDER BY sessionID";
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader_jumpsRj;
		reader_jumpsRj = dbcmd.ExecuteReader();
		ArrayList myArray_jumpsRj = new ArrayList(2);
		
		while(reader_jumpsRj.Read()) {
			myArray_jumpsRj.Add (reader_jumpsRj[0].ToString() + ":" + reader_jumpsRj[1].ToString() + ":" );
		}
		reader_jumpsRj.Close();
		
		//select runs of each session
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM RUN GROUP BY sessionID ORDER BY sessionID";
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader_runs;
		reader_runs = dbcmd.ExecuteReader();
		ArrayList myArray_runs = new ArrayList(2);
		
		while(reader_runs.Read()) {
			myArray_runs.Add (reader_runs[0].ToString() + ":" + reader_runs[1].ToString() + ":" );
		}
		reader_runs.Close();
		
		//select runsInterval of each session
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM RUNINTERVAL GROUP BY sessionID ORDER BY sessionID";
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader_runs_interval;
		reader_runs_interval = dbcmd.ExecuteReader();
		ArrayList myArray_runs_interval = new ArrayList(2);
		
		while(reader_runs_interval.Read()) {
			myArray_runs_interval.Add (reader_runs_interval[0].ToString() + ":" + reader_runs_interval[1].ToString() + ":" );
		}
		reader_runs_interval.Close();
	
		//select reaction time of each session
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM REACTIONTIME GROUP BY sessionID ORDER BY sessionID";
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader_rt;
		reader_rt = dbcmd.ExecuteReader();
		ArrayList myArray_rt = new ArrayList(2);
		
		while(reader_rt.Read()) {
			myArray_rt.Add (reader_rt[0].ToString() + ":" + reader_rt[1].ToString() + ":" );
		}
		reader_rt.Close();
	
		//select pulses of each session
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM PULSE GROUP BY sessionID ORDER BY sessionID";
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader_pulses;
		reader_pulses = dbcmd.ExecuteReader();
		ArrayList myArray_pulses = new ArrayList(2);
		
		while(reader_pulses.Read()) {
			myArray_pulses.Add (reader_pulses[0].ToString() + ":" + reader_pulses[1].ToString() + ":" );
		}
		reader_pulses.Close();
	
		
		//close database connection
		dbcon.Close();

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

			
			mySessions [count++] = lineNotReadOnly;
		}

		return mySessions;
	}


	//called from gui/event.cs for doing the graph
	//we need to know the avg of events of a type (SJ, CMJ, free (pulse).. of a person, or of all persons on the session
	public static double SelectAllEventsOfAType(int sessionID, int personID, string table, string type, string valueToSelect) 
	{
		//if personIDString == -1, the applies for all persons
		
		string personIDString = "";
		if(personID != -1)
			personIDString = " AND personID == " + personID; 

		
		dbcon.Open();
		dbcmd.CommandText = "SELECT AVG(" + valueToSelect + ")" +
			" FROM " + table +				
			" WHERE sessionID == " + sessionID + 
			" AND type == '" + type + "' " +
			personIDString; 
		
		Console.WriteLine(dbcmd.CommandText.ToString());
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
		dbcon.Close();

		if (found) {
			return myReturn;
		} else {
			return 0;
		}
	}


	
	public static void DeleteWithJumps(string uniqueID)
	{
		dbcon.Open();

		//delete the session
		dbcmd.CommandText = "Delete FROM session WHERE uniqueID == " + uniqueID;
		dbcmd.ExecuteNonQuery();
		
		//delete relations (existance) within persons and sessions in this session
		dbcmd.CommandText = "Delete FROM personSession WHERE sessionID == " + uniqueID;
		dbcmd.ExecuteNonQuery();
		
		//delete normal jumps
		dbcmd.CommandText = "Delete FROM jump WHERE sessionID == " + uniqueID;
		dbcmd.ExecuteNonQuery();
		
		//delete repetitive jumps
		dbcmd.CommandText = "Delete FROM jumpRj WHERE sessionID == " + uniqueID;
		dbcmd.ExecuteNonQuery();
		
		//runs PENDING
		
		dbcon.Close();
	}

}
