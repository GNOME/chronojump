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
using Mono.Data.Sqlite;
using Mono.Unix;
using System.Collections.Generic; //List<T>


class SqlitePersonSession : Sqlite
{
	public SqlitePersonSession() {
	}
	
	~SqlitePersonSession() {}

	protected override void createTable(string tableName)
	 {
		dbcmd.CommandText = 
			"CREATE TABLE " + tableName + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"personID INT, " +
			"sessionID INT, " +
			"height FLOAT, " +
			"weight FLOAT, " + 
			"sportID INT, " +
			"speciallityID INT, " +
			"practice INT, " + //also called "level"
			"comments TEXT, " +
			"future1 TEXT, " + 	//since Chronojump 2.0 trochanterToe
			"future2 TEXT)"; 	//since Chronojump 2.0 trochanterFloorOnFlexion
		dbcmd.ExecuteNonQuery();
	 }

	public static int Insert(bool dbconOpened, string uniqueID, int personID, int sessionID, 
			double height, double weight, int sportID, int speciallityID, int practice,
			string comments, double trochanterToe, double trochanterFloorOnFlexion)
	{
		if(!dbconOpened)
			Sqlite.Open();
		
		if(uniqueID == "-1")
			uniqueID = "NULL";

		// -----------------------
		//ATTENTION: if this changes, change the PersonSession.ToSQLInsertString()
		// -----------------------
		dbcmd.CommandText = "INSERT INTO " + Constants.PersonSessionTable + 
			"(uniqueID, personID, sessionID, height, weight, " + 
			"sportID, speciallityID, practice, comments, future1, future2)" + 
		        " VALUES ("
			+ uniqueID + ", " + personID + ", " + sessionID + ", " + 
			Util.ConvertToPoint(height) + ", " + Util.ConvertToPoint(weight) + ", " +
			sportID + ", " + speciallityID + ", " + practice + ", \"" + comments + "\", " +
			Util.ConvertToPoint(trochanterToe) + ", " +
			Util.ConvertToPoint(trochanterFloorOnFlexion) + ")";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		//int myLast = dbcon.LastInsertRowId;
		//http://stackoverflow.com/questions/4341178/getting-the-last-insert-id-with-sqlite-net-in-c
		string myString = @"select last_insert_rowid()";
		dbcmd.CommandText = myString;
		int myLast = Convert.ToInt32(dbcmd.ExecuteScalar()); // Need to type-cast since `ExecuteScalar` returns an object.

		if(!dbconOpened)
			Sqlite.Close();
		return myLast;
	}
	
	//we KNOW session
	//select doubles
	public static double SelectAttribute(bool dbconOpened, int personID, int sessionID, string attribute)
	{
		if( ! dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "SELECT " + attribute + " FROM " + Constants.PersonSessionTable +
			" WHERE personID = " + personID +
			" AND sessionID = " + sessionID;
		
		//LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		
		double myReturn = 0;
		if(reader.Read()) {
			myReturn = Convert.ToDouble(Util.ChangeDecimalSeparator(reader[0].ToString()));
		}
		reader.Close();
		if( ! dbconOpened)
			Sqlite.Close();

		return myReturn;
	}

	//when a session is NOT KNOWN, then select atrribute of last session
	//select doubles
	public static double SelectAttributeOnLastSession (int personID, string attribute)
	{
		Sqlite.Open();

		dbcmd.CommandText = "SELECT " + attribute + ", sessionID FROM " + Constants.PersonSessionTable + 
			" WHERE personID = " + personID +
			" ORDER BY sessionID DESC LIMIT 1";
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		
		double myReturn = 0;
		if(reader.Read()) {
			myReturn = Convert.ToDouble(Util.ChangeDecimalSeparator(reader[0].ToString()));
		}
		reader.Close();
		Sqlite.Close();
		return myReturn;
	}
	
	public static void Update (bool dbconOpened, PersonSession ps)
	{
		openIfNeeded (dbconOpened);

		dbcmd.CommandText = "UPDATE " + Constants.PersonSessionTable + 
			" SET personID = " + ps.PersonID + 
			", sessionID = " + ps.SessionID + 
			", height = " + Util.ConvertToPoint(ps.Height) + 
			", weight = " + Util.ConvertToPoint(ps.Weight) + 
			", sportID = " + ps.SportID + 
			", speciallityID = " + ps.SpeciallityID + 
			", practice = " + ps.Practice + 
			", comments = \"" + ps.Comments + 
			"\", future1 = " + Util.ConvertToPoint(ps.TrochanterToe) +
			", future2 = " + Util.ConvertToPoint(ps.TrochanterFloorOnFlexion) +
			" WHERE uniqueID == " + ps.UniqueID;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		closeIfNeeded (dbconOpened);
	}

	//double
	public static void UpdateAttribute(int personID, int sessionID, string attribute, double attrValue)
	{
		Sqlite.Open();
		dbcmd.CommandText = "UPDATE " + Constants.PersonSessionTable + 
			" SET " + attribute + " = " + Util.ConvertToPoint(attrValue) + 
			" WHERE personID = " + personID +
			" AND sessionID = " + sessionID
			;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		Sqlite.Close();
	}

	public static bool PersonSelectExistsInSession(bool dbconOpened, int myPersonID, int mySessionID)
	{
		if(! dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "SELECT * FROM " + Constants.PersonSessionTable +
			" WHERE personID == " + myPersonID + 
			" AND sessionID == " + mySessionID ; 
		LogB.SQL(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
	
		bool exists = new bool();
		exists = false;
		
		while(reader.Read()) 
			exists = true;

		reader.Close();

		if(! dbconOpened)
			Sqlite.Close();

		return exists;
	}

	//if sessionID == -1
	//then we search data in last sessionID
	//this is used to know personSession attributes
	//in a newly created person	
	//This is like SqlitePerson.Select but this returns a PersonSession

	public static PersonSession Select(int personID, int sessionID)
	{
		return Select(false, personID, sessionID);
	}
	public static PersonSession Select(bool dbconOpened, int personID, int sessionID)
	{
		if( ! dbconOpened)
			Sqlite.Open();

		string tps = Constants.PersonSessionTable;
			
		string sessionIDString = " AND sessionID == " + sessionID;
		if(sessionID == -1)
			sessionIDString = " ORDER BY sessionID DESC limit 1";

		dbcmd.CommandText = "SELECT * FROM " + tps +
			" WHERE personID == " + personID + 
			sessionIDString;
		
		LogB.SQL(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
	
		PersonSession ps = new PersonSession();
		ps.UniqueID = -1;
		while(reader.Read()) {
			ps = new PersonSession(
					Convert.ToInt32(reader[0].ToString()), 	//uniqueID
					personID,				//personID
					sessionID, 				//sessionID
					Convert.ToDouble(Util.ChangeDecimalSeparator(reader[3].ToString())), //height
					Convert.ToDouble(Util.ChangeDecimalSeparator(reader[4].ToString())), //weight
					Convert.ToInt32(reader[5].ToString()), 	//sportID
					Convert.ToInt32(reader[6].ToString()), 	//speciallityID
					Convert.ToInt32(reader[7].ToString()),	//practice
					reader[8].ToString(), 			//comments
					Convert.ToDouble(Util.ChangeDecimalSeparator(reader[9].ToString())), //trochanterToe
					Convert.ToDouble(Util.ChangeDecimalSeparator(reader[10].ToString())) //trochanterFloorOnFlexion
					); 
		}
		
		reader.Close();
		
		if( ! dbconOpened)
			Sqlite.Close();

		return ps;
	}

	//sessionID can be -1
	public static List<Person> SelectCurrentSessionPersonsAsList (bool dbconOpened, int sessionID)
	{
		if(! dbconOpened)
			Sqlite.Open();

		string tp = Constants.PersonTable;
		string tps = Constants.PersonSessionTable;

		string sessionIDString = tps + ".sessionID = " + sessionID + " AND ";
		if(sessionID == -1)
			sessionIDString = "";

		dbcmd.CommandText = "SELECT " + tp + ".*" +
			" FROM " + tp + ", " + tps +
			" WHERE " + sessionIDString +
			tp + ".uniqueID = " + tps + ".personID " +
			" ORDER BY upper(" + tp + ".name)";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		List<Person> person_l = new List<Person>();
		while(reader.Read())
		{
			Person person = new Person(
					Convert.ToInt32(reader[0].ToString()),	//uniqueID
					reader[1].ToString(),			//name
					reader[2].ToString(),			//sex
					UtilDate.FromSql(reader[3].ToString()),	//dateBorn
					Convert.ToInt32(reader[4].ToString()),	//race
					Convert.ToInt32(reader[5].ToString()),	//countryID
					reader[6].ToString(),			//description
					reader[7].ToString(),			//future1: rfid
					reader[8].ToString(),			//future2: clubID
					Convert.ToInt32(reader[9].ToString()),	//serverUniqueID
					reader[10].ToString()			//linkServerImage
					);
			person_l.Add(person);
		}

		reader.Close();

		if(! dbconOpened)
			Sqlite.Close();

		return person_l;
	}
	//the difference between this select and others, is that this returns and ArrayList of Persons
	//this is better than return the strings that can produce bugs in the future
	//sessionID can be -1
	public static ArrayList SelectCurrentSessionPersons(int sessionID, bool returnPersonAndPSlist) 
	{
		string tp = Constants.PersonTable;
		string tps = Constants.PersonSessionTable;
			
		string tpsString = "";
		if(returnPersonAndPSlist)
			tpsString = ", " + tps + ".* ";

		string sessionIDString = tps + ".sessionID = " + sessionID + " AND ";
		if(sessionID == -1)
			sessionIDString = "";
		
		Sqlite.Open();
		dbcmd.CommandText = "SELECT " + tp + ".*" + tpsString +
			" FROM " + tp + ", " + tps + 
			" WHERE " + sessionIDString +
			tp + ".uniqueID = " + tps + ".personID " +
			" ORDER BY upper(" + tp + ".name)";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(1);
		while(reader.Read()) {
			Person person = new Person(
					Convert.ToInt32(reader[0].ToString()),	//uniqueID
					reader[1].ToString(),			//name
					reader[2].ToString(),			//sex
					UtilDate.FromSql(reader[3].ToString()),	//dateBorn
					Convert.ToInt32(reader[4].ToString()),	//race
					Convert.ToInt32(reader[5].ToString()),	//countryID
					reader[6].ToString(),			//description
					reader[7].ToString(),			//future1: rfid
					reader[8].ToString(),			//future2: clubID
					Convert.ToInt32(reader[9].ToString()),	//serverUniqueID
					reader[10].ToString()			//linkServerImage
					);

			if(returnPersonAndPSlist) {
				PersonSession ps = new PersonSession(
						Convert.ToInt32(reader[11].ToString()), 	//uniqueID
						Convert.ToInt32(reader[12].ToString()), 	//personID
						Convert.ToInt32(reader[13].ToString()), 	//sessionID
						Convert.ToDouble(Util.ChangeDecimalSeparator(reader[14].ToString())), //height
						Convert.ToDouble(Util.ChangeDecimalSeparator(reader[15].ToString())), //weight
						Convert.ToInt32(reader[16].ToString()), 	//sportID
						Convert.ToInt32(reader[17].ToString()), 	//speciallityID
						Convert.ToInt32(reader[18].ToString()),	//practice
						reader[19].ToString(), 			//comments
						Convert.ToDouble(Util.ChangeDecimalSeparator(reader[20].ToString())), //trochanterToe
						Convert.ToDouble(Util.ChangeDecimalSeparator(reader[21].ToString())) //trochanterFloorOnFlexion
						);
				myArray.Add(new PersonAndPS(person, ps));
			} else
				myArray.Add (person);
		}
		reader.Close();
		Sqlite.Close();
		return myArray;
	}
	
	//use this in the future:
	public static List<PersonSession> SelectPersonSessionList (bool dbconOpened, int personID, int sessionID)
	{
		string tps = Constants.PersonSessionTable;

		string whereStr = "";
		string andStr = "";

		string personIDStr = "";
		if (personID != -1)
		{
			personIDStr = tps + ".personID = " + personID;
			whereStr = " WHERE ";
		}

		string sessionIDStr = "";
		if (sessionID != -1)
		{
			sessionIDStr = tps + ".sessionID = " + sessionID;
			if (whereStr == "")
				whereStr = " WHERE ";
			else
				andStr = " AND ";
		}

		openIfNeeded (dbconOpened);

		dbcmd.CommandText = "SELECT " + tps + ".*" +
			" FROM " + tps +
			whereStr + personIDStr +
			andStr + sessionIDStr +
			" ORDER BY sessionID"; //used on DeletePersonSessionsDuplicatedOnMerge ()

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		List<PersonSession> list = new List<PersonSession>();
		while(reader.Read())
		{
			PersonSession ps = new PersonSession(
					Convert.ToInt32(reader[0].ToString()), 	//uniqueID
					Convert.ToInt32(reader[1].ToString()), 	//personID
					Convert.ToInt32(reader[2].ToString()), 	//sessionID
					Convert.ToDouble(Util.ChangeDecimalSeparator(reader[3].ToString())), //height
					Convert.ToDouble(Util.ChangeDecimalSeparator(reader[4].ToString())), //weight
					Convert.ToInt32(reader[5].ToString()), 	//sportID
					Convert.ToInt32(reader[6].ToString()), 	//speciallityID
					Convert.ToInt32(reader[7].ToString()),	//practice
					reader[8].ToString(), 			//comments
					Convert.ToDouble(Util.ChangeDecimalSeparator(reader[9].ToString())), //trochanterToe
					Convert.ToDouble(Util.ChangeDecimalSeparator(reader[10].ToString())) //trochanterFloorOnFlexion
					);
			list.Add(ps);
		}
		reader.Close();
		closeIfNeeded (dbconOpened);

		return list;
	}

	public static void DeletePersonFromSessionAndTests(string sessionID, string personID)
	{
		Sqlite.Open();

		//1.- first delete in personSession77 at this session

		//delete relations (existance) within persons and sessions in this session
		dbcmd.CommandText = "Delete FROM " + Constants.PersonSessionTable + 
			" WHERE sessionID == " + sessionID +
			" AND personID == " + personID;
		dbcmd.ExecuteNonQuery();

		//2.- Now, it's not in this personSession77 in other sessions, delete if from DB

		//if person is not in other sessions, delete it from DB
		if (! PersonExistsInAnyPS (true, Convert.ToInt32 (personID)))
			SqlitePerson.DeletePersonAndImages (true, Convert.ToInt32 (personID));

		//3.- Delete tests without files
				
		//delete simple jumps
		dbcmd.CommandText = "Delete FROM jump WHERE sessionID == " + sessionID +
			" AND personID == " + personID;
			
		dbcmd.ExecuteNonQuery();
		
		//delete repetitive jumps
		dbcmd.CommandText = "Delete FROM jumpRj WHERE sessionID == " + sessionID +
			" AND personID == " + personID;
		dbcmd.ExecuteNonQuery();
		
		//delete simple runs
		dbcmd.CommandText = "Delete FROM run WHERE sessionID == " + sessionID +
			" AND personID == " + personID;
		
		dbcmd.ExecuteNonQuery();
		
		//delete intervallic runs
		dbcmd.CommandText = "Delete FROM runInterval WHERE sessionID == " + sessionID +
			" AND personID == " + personID;
			
		dbcmd.ExecuteNonQuery();
		
		//delete reaction times
		dbcmd.CommandText = "Delete FROM reactionTime WHERE sessionID == " + sessionID +
			" AND personID == " + personID;
			
		dbcmd.ExecuteNonQuery();
		
		//delete pulses
		dbcmd.CommandText = "Delete FROM pulse WHERE sessionID == " + sessionID +
			" AND personID == " + personID;
			
		dbcmd.ExecuteNonQuery();
		
		//delete multiChronopic
		dbcmd.CommandText = "Delete FROM multiChronopic WHERE sessionID == " + sessionID +
			" AND personID == " + personID;
			
		dbcmd.ExecuteNonQuery();
	
		// 4) delete from encoder
		//delete encoder signal and curves (and it's videos)
		ArrayList encoderArray = SqliteEncoder.Select(
				true, -1, Convert.ToInt32(personID), Convert.ToInt32(sessionID), Constants.EncoderGI.ALL,
				-1, "signal", EncoderSQL.Eccons.ALL, "",
				false, true, false);

		foreach(EncoderSQL eSQL in encoderArray)
		{
			Util.FileDelete(eSQL.GetFullURL(false));	//signal, don't convertPathToR
			if(eSQL.future2 != "")
				Util.FileDelete(eSQL.future2);		//video
			Sqlite.Delete(true, Constants.EncoderTable, Convert.ToInt32(eSQL.uniqueID));

			//delete related triggers
			SqliteTrigger.DeleteByModeID(true, Trigger.Modes.ENCODER, Convert.ToInt32(eSQL.uniqueID));
		}

		//curves
		encoderArray = SqliteEncoder.Select(
				true, -1, Convert.ToInt32(personID), Convert.ToInt32(sessionID), Constants.EncoderGI.ALL,
				-1, "curve", EncoderSQL.Eccons.ALL, "",
				false, true, true);
		
		foreach(EncoderSQL eSQL in encoderArray) {
			Util.FileDelete(eSQL.GetFullURL(false));	//don't convertPathToR
			/* commented: curve has no video
			if(eSQL.future2 != "")
				Util.FileDelete(eSQL.future2);
			*/
			Sqlite.Delete(true, Constants.EncoderTable, Convert.ToInt32(eSQL.uniqueID));
			SqliteEncoder.DeleteSignalCurveWithCurveID(true, Convert.ToInt32(eSQL.uniqueID));
		}

		// 5) delete forceSensor and related triggers
		List<ForceSensor> fs_l = SqliteForceSensor.Select (true, -1, Convert.ToInt32(personID), Convert.ToInt32(sessionID), -1);
		foreach(ForceSensor fs in fs_l)
		{
			SqliteForceSensor.DeleteSQLAndFiles (true, fs); //deletes also the .csv

			//delete related triggers
			SqliteTrigger.DeleteByModeID(true, Trigger.Modes.FORCESENSOR, fs.UniqueID);
		}

		// 6) delete runEncoder and related triggers
		List<RunEncoder> re_l = SqliteRunEncoder.Select (true, -1, Convert.ToInt32(personID), Convert.ToInt32(sessionID));
		foreach(RunEncoder re in re_l)
		{
			SqliteRunEncoder.DeleteSQLAndFiles (true, re); //deletes also the .csv

			//delete related triggers
			SqliteTrigger.DeleteByModeID(true, Trigger.Modes.RACEANALYZER, re.UniqueID);
		}

		// 7).- TODO: delete videos

		Sqlite.Close();
	}

	/*
	 * this is called from gui/person/merge to delete two different personSessions on same session
	 * and also called from DeletePersonSessionsDuplicatedOnMerge to delete two equal personSessions on same session
	 */
	public static void DeletePersonSessionOnMerge (bool dbconOpened, int uniqueID)
	{
		// 1) delete from DB
		openIfNeeded (dbconOpened);

		dbcmd.CommandText = "DELETE FROM " + Constants.PersonSessionTable + " WHERE uniqueID = " + uniqueID;

		LogB.SQL (dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery ();

		closeIfNeeded (dbconOpened);
	}

	// on sessions where there are no differences in session we also need to delete the repeated personSession if any
	public static void DeletePersonSessionsDuplicatedOnMerge (bool dbconOpened, int personID)
	{
		List<PersonSession> ps_l = SelectPersonSessionList (dbconOpened, personID, -1); //comes sorted by sessionID

		int lastSessionID = -1;
		foreach (PersonSession ps in ps_l)
		{
			if (ps.SessionID == lastSessionID)
				DeletePersonSessionOnMerge (dbconOpened, ps.UniqueID);

			lastSessionID = ps.SessionID;
		}
	}

	public static bool PersonExistsInAnyPS (bool dbconOpened, int personID)
	{
		if( ! dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "SELECT * FROM " + Constants.PersonSessionTable + 
			" WHERE personID == " + personID;
		//LogB.SQL(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
	
		bool exists = new bool();
		exists = false;
		
		if (reader.Read()) {
			exists = true;
		}
		//LogB.SQL(string.Format("personID exists = {0}", exists.ToString()));

		reader.Close();
		
		if( ! dbconOpened)
			Sqlite.Close();

		return exists;
	}

}


//used to insert person and personSession in a single translation when creating multiple persons
//and used to to insert personSession in a single translation when recuperating multiple persons
class SqlitePersonSessionTransaction : Sqlite
{
	List <Person> persons;
	List <PersonSession> personSessions;
	enum Modes { INSERT_PERSONS_MULTIPLE, RECUPERATE_PERSONS_MULTIPLE }
	Modes mode;
	
	public SqlitePersonSessionTransaction(List <PersonSession> personSessions) 
	{
		this.personSessions = personSessions;
		mode = Modes.RECUPERATE_PERSONS_MULTIPLE;
		
		doTransaction();
	}
	public SqlitePersonSessionTransaction(List <Person> persons, List <PersonSession> personSessions) 
	{
		this.persons = persons;
		this.personSessions = personSessions;
		mode = Modes.INSERT_PERSONS_MULTIPLE;
		
		doTransaction();
	}

	public void doTransaction() 
	{
		LogB.SQL("Starting transaction");
		Sqlite.Open();

		using(SqliteTransaction tr = dbcon.BeginTransaction())
		{
			using (SqliteCommand dbcmdTr = dbcon.CreateCommand())
			{
				dbcmdTr.Transaction = tr;
				
				if(mode == Modes.INSERT_PERSONS_MULTIPLE) {
					foreach(Person p in persons) {
						dbcmdTr.CommandText = 
							"INSERT INTO " + Constants.PersonTable +
							" (uniqueID, name, sex, dateBorn, race, countryID, description, future1, future2, serverUniqueID, linkServerImage) " +
							" VALUES (" + p.ToSQLInsertString() + ")";
						LogB.SQL(dbcmdTr.CommandText.ToString());
						dbcmdTr.ExecuteNonQuery();
					}
				}
				foreach(PersonSession ps in personSessions) {
					dbcmdTr.CommandText = 
						"INSERT INTO " + Constants.PersonSessionTable +
						"(uniqueID, personID, sessionID, height, weight, " + 
						"sportID, speciallityID, practice, comments, future1, future2)" + 
						" VALUES (" + ps.ToSQLInsertString() + ")";
					LogB.SQL(dbcmdTr.CommandText.ToString());
					dbcmdTr.ExecuteNonQuery();
				}
			}
			tr.Commit();
		}

		Sqlite.Close();
		LogB.SQL("Ended transaction");
	}
}
