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
 * Copyright (C) 2004-2014   Xavier de Blas <xaviblas@gmail.com> 
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
			"future1 TEXT, " +
			"future2 TEXT)";
		dbcmd.ExecuteNonQuery();
	 }

	public static int Insert(bool dbconOpened, string uniqueID, int personID, int sessionID, 
			double height, double weight, int sportID, int speciallityID, int practice,
			string comments) 
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
			sportID + ", " + speciallityID + ", " + practice + ", '" + 
			comments + "', '', '')"; 
		Log.WriteLine(dbcmd.CommandText.ToString());
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
		       	" WHERE personID == " + personID + 
			" AND sessionID == " + sessionID;
		
		//Log.WriteLine(dbcmd.CommandText.ToString());
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
	public static double SelectAttribute(int personID, string attribute)
	{
		Sqlite.Open();

		dbcmd.CommandText = "SELECT " + attribute + ", sessionID FROM " + Constants.PersonSessionTable + 
			" WHERE personID == " + personID + 
			"ORDER BY sessionID DESC LIMIT 1";
		
		Log.WriteLine(dbcmd.CommandText.ToString());
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
	
	public static void Update(PersonSession ps)
	{
		Sqlite.Open();
		dbcmd.CommandText = "UPDATE " + Constants.PersonSessionTable + 
			" SET personID = " + ps.PersonID + 
			", sessionID = " + ps.SessionID + 
			", height = " + Util.ConvertToPoint(ps.Height) + 
			", weight = " + Util.ConvertToPoint(ps.Weight) + 
			", sportID = " + ps.SportID + 
			", speciallityID = " + ps.SpeciallityID + 
			", practice = " + ps.Practice + 
			", comments = '" + ps.Comments + 
			"' WHERE uniqueID == " + ps.UniqueID;
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		Sqlite.Close();
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
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		Sqlite.Close();
	}

	public static bool PersonSelectExistsInSession(int myPersonID, int mySessionID)
	{
		Sqlite.Open();
		dbcmd.CommandText = "SELECT * FROM " + Constants.PersonSessionTable +
			" WHERE personID == " + myPersonID + 
			" AND sessionID == " + mySessionID ; 
		Log.WriteLine(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
	
		bool exists = new bool();
		exists = false;
		
		while(reader.Read()) 
			exists = true;

		reader.Close();
		Sqlite.Close();
		return exists;
	}

	//if sessionID == -1
	//then we search data in last sessionID
	//this is used to know personSession attributes
	//in a newly created person	

	//This is like SqlitePerson.Selectbut this returns a PersonSession
	public static PersonSession Select(int personID, int sessionID)
	{
		string tps = Constants.PersonSessionTable;
			
		string sessionIDString = " AND sessionID == " + sessionID;
		if(sessionID == -1)
			sessionIDString = " ORDER BY sessionID DESC limit 1";

		Sqlite.Open();
		dbcmd.CommandText = "SELECT * FROM " + tps +
			" WHERE personID == " + personID + 
			sessionIDString;
		
		Log.WriteLine(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
	
		PersonSession ps = new PersonSession();
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
					reader[8].ToString() 			//comments
					); 
		}
		
		reader.Close();
		Sqlite.Close();
		return ps;
	}
	
	//the difference between this select and others, is that this returns and ArrayList of Persons
	//this is better than return the strings that can produce bugs in the future
	//use this in the future:
	public static ArrayList SelectCurrentSessionPersons(int sessionID) 
	{
		string tp = Constants.PersonTable;
		string tps = Constants.PersonSessionTable;
		
		Sqlite.Open();
		dbcmd.CommandText = "SELECT " + tp + ".*" +
			" FROM " + tp + ", " + tps + 
			" WHERE " + tps + ".sessionID == " + sessionID + 
			" AND " + tp + ".uniqueID == " + tps + ".personID " + 
			" ORDER BY upper(" + tp + ".name)";
		Log.WriteLine(dbcmd.CommandText.ToString());
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
					Convert.ToInt32(reader[9].ToString())	//serverUniqueID
					);
			myArray.Add (person);
		}
		reader.Close();
		Sqlite.Close();
		return myArray;
	}
	
	/*
	   try to use upper method:
		public static ArrayList SelectCurrentSessionPersons(int sessionID) 
		
	public static string[] SelectCurrentSession(int sessionID, bool onlyIDAndName, bool reverse) 
	{
		string tp = Constants.PersonTable;
		string tps = Constants.PersonSessionTable;
		
		Sqlite.Open();
		dbcmd.CommandText = "SELECT " + tp + ".*, " + tps + ".weight, sport.name, speciallity.name " +
			"FROM " + tp + ", " + tps + ", sport, speciallity " +
			" WHERE " + tps + ".sessionID == " + sessionID + 
			" AND " + tp + ".uniqueID == " + tps + ".personID " + 
			" AND " + tp + ".sportID == sport.uniqueID " + 
			" AND " + tp + ".speciallityID == speciallity.uniqueID " + 
			" ORDER BY upper(" + tp + ".name)";
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);

		int count = new int();
		count = 0;

		while(reader.Read()) {
			if(onlyIDAndName)
				myArray.Add (reader[0].ToString() + ":" + reader[1].ToString() );
			else {
				string sportName = Catalog.GetString(reader[14].ToString());

				string speciallityName = ""; //to solve a gettext bug (probably because speciallity undefined name is "")
				if(reader[15].ToString() != "")
					speciallityName = Catalog.GetString(reader[15].ToString());
				string levelName = Catalog.GetString(Util.FindLevelName(Convert.ToInt32(reader[8])));

				myArray.Add (
						reader[0].ToString() + ":" + reader[1].ToString() + ":" + 	//id, name
						reader[2].ToString() + ":" + 					//sex
						UtilDate.FromSql(reader[3].ToString()).ToShortDateString() + ":" +	//dateborn
						reader[4].ToString() + ":" + reader[13].ToString() + ":" + //height, weight (from personSessionWeight)
						sportName + ":" + speciallityName + ":" + levelName + ":" +
						reader[9].ToString()  //desc
					    );
			}
			count ++;
		}

		reader.Close();
		Sqlite.Close();

		string [] myJumpers = new string[count];
		
		if(reverse) {
			//show the results in the combo_sujeto_actual in reversed order, 
			//then when we create a new person, this is the active, and this is shown 
			//correctly in the combo_sujeto_actual
			int count2 = count -1;
			foreach (string line in myArray) {
				myJumpers [count2--] = line;
			}
		} else {
			int count2 = 0;
			foreach (string line in myArray) {
				myJumpers [count2++] = line;
			}
		}
		return myJumpers;
	}
	*/
	
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
		if(! PersonExistsInPS(true, Convert.ToInt32(personID))) {
			//this will open and close DB connection
			Delete(true, Constants.PersonTable, Convert.ToInt32(personID));

			//delete photos if any
			if(File.Exists(Util.GetPhotoFileName(false, Convert.ToInt32(personID))))
				File.Delete(Util.GetPhotoFileName(false, Convert.ToInt32(personID)));
			if(File.Exists(Util.GetPhotoFileName(true, Convert.ToInt32(personID))))
				File.Delete(Util.GetPhotoFileName(true, Convert.ToInt32(personID)));
		}

		//3.- Delete tests
				
		//delete normal jumps
		dbcmd.CommandText = "Delete FROM jump WHERE sessionID == " + sessionID +
			" AND personID == " + personID;
			
		dbcmd.ExecuteNonQuery();
		
		//delete repetitive jumps
		dbcmd.CommandText = "Delete FROM jumpRj WHERE sessionID == " + sessionID +
			" AND personID == " + personID;
		dbcmd.ExecuteNonQuery();
		
		//delete normal runs
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
	
		//delete from encoder
		dbcmd.CommandText = "Delete FROM " + Constants.EncoderTable + " WHERE sessionID == " + sessionID +
			" AND personID == " + personID;
			
		dbcmd.ExecuteNonQuery();
		

		//delete encoder signal and curves (and it's videos)
		ArrayList encoderArray = SqliteEncoder.Select(
				true, -1, Convert.ToInt32(personID), Convert.ToInt32(sessionID), -1,
				"signal", EncoderSQL.Eccons.ALL,
				false, true);

		foreach(EncoderSQL eSQL in encoderArray) {
			Util.FileDelete(eSQL.GetFullURL(false));	//signal, don't convertPathToR
			if(eSQL.future2 != "")
				Util.FileDelete(eSQL.future2);		//video
			Sqlite.Delete(true, Constants.EncoderTable, Convert.ToInt32(eSQL.uniqueID));
		}

		encoderArray = SqliteEncoder.Select(
				true, -1, Convert.ToInt32(personID), Convert.ToInt32(sessionID), -1, 
				"curve", EncoderSQL.Eccons.ALL, 
				false, true);
		
		foreach(EncoderSQL eSQL in encoderArray) {
			Util.FileDelete(eSQL.GetFullURL(false));	//don't convertPathToR
			/* commented: curve has no video
			if(eSQL.future2 != "")
				Util.FileDelete(eSQL.future2);
			*/
			Sqlite.Delete(true, Constants.EncoderTable, Convert.ToInt32(eSQL.uniqueID));
			SqliteEncoder.DeleteSignalCurveWithCurveID(true, Convert.ToInt32(eSQL.uniqueID));
		}
				
		
		//4.- TODO: delete videos


		Sqlite.Close();
	}

	public static bool PersonExistsInPS(bool dbconOpened, int personID)
	{
		if( ! dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "SELECT * FROM " + Constants.PersonSessionTable + 
			" WHERE personID == " + personID;
		//Log.WriteLine(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
	
		bool exists = new bool();
		exists = false;
		
		if (reader.Read()) {
			exists = true;
		}
		//Log.WriteLine(string.Format("personID exists = {0}", exists.ToString()));

		reader.Close();
		
		if( ! dbconOpened)
			Sqlite.Close();

		return exists;
	}

}


//used to insert person and personSession in a single translation
class SqlitePersonSessionTransaction : Sqlite
{
	public SqlitePersonSessionTransaction(List <Person> persons, List <PersonSession> personSessions) 
	{
		Log.WriteLine("Starting transaction");
		Sqlite.Open();

		using(SqliteTransaction tr = dbcon.BeginTransaction())
		{
			using (SqliteCommand dbcmdTr = dbcon.CreateCommand())
			{
				dbcmdTr.Transaction = tr;
				
				foreach(Person p in persons) {
					dbcmdTr.CommandText = 
						"INSERT INTO " + Constants.PersonTable +
						" (uniqueID, name, sex, dateBorn, race, countryID, description, future1, future2, serverUniqueID) " + 
						" VALUES (" + p.ToSQLInsertString() + ")";
					Log.WriteLine(dbcmdTr.CommandText.ToString());
					dbcmdTr.ExecuteNonQuery();
				}
				foreach(PersonSession ps in personSessions) {
					dbcmdTr.CommandText = 
						"INSERT INTO " + Constants.PersonSessionTable +
						"(uniqueID, personID, sessionID, height, weight, " + 
						"sportID, speciallityID, practice, comments, future1, future2)" + 
						" VALUES (" + ps.ToSQLInsertString() + ")";
					Log.WriteLine(dbcmdTr.CommandText.ToString());
					dbcmdTr.ExecuteNonQuery();
				}
			}
			tr.Commit();
		}

		Sqlite.Close();
		Log.WriteLine("Ended transaction");
	}
}
