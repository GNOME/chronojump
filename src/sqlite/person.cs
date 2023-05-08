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
 * Copyright (C) 2004-2022   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Data;
using System.IO;
using System.Collections; //ArrayList
using Mono.Data.Sqlite;


class SqlitePerson : Sqlite
{
	public SqlitePerson() {
	}
	
	~SqlitePerson() {}

	//can be "Constants.PersonTable" or "Constants.ConvertTempTable"
	//temp is used to modify table between different database versions if needed
	//protected new internal static void createTable(string tableName)
	protected override void createTable(string tableName)
	 {
		dbcmd.CommandText = 
			"CREATE TABLE " + tableName + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"name TEXT, " +
			"sex TEXT, " + //since May 2023 can be -,F,M (first one is Unspecified)
			"dateborn TEXT, " + //YYYY-MM-DD since db 0.72
			"race INT, " + 
			"countryID INT, " + 
			"description TEXT, " +	
			"future1 TEXT, " + //rfid
			"future2 TEXT, " + //clubID
			"serverUniqueID INT, " +
			"linkServerImage TEXT ) "; //for networks
		dbcmd.ExecuteNonQuery();
	 }

	public static int Insert(bool dbconOpened, string uniqueID, string name, string sex, DateTime dateBorn, 
			int race, int countryID, string description, string future1, string future2, int serverUniqueID, string linkServerImage)
	{
		LogB.SQL("going to insert");
		if(! dbconOpened)
			Sqlite.Open();

		if(uniqueID == "-1")
			uniqueID = "NULL";

		// -----------------------
		//ATTENTION: if this changes, change the Person.ToSQLInsertString()
		// -----------------------
		string myString = "INSERT INTO " + Constants.PersonTable + 
			" (uniqueID, name, sex, dateBorn, race, countryID, description, future1, future2, serverUniqueID, linkServerImage) VALUES (" + uniqueID + ", \"" +
			name + "\", \"" + sex + "\", \"" + UtilDate.ToSql(dateBorn) + "\", " + 
			race + ", " + countryID + ", \"" + description + "\", \"" +
			future1 + "\", \"" + future2 + "\", " + serverUniqueID + ", \"" + linkServerImage + "\")";
		
		dbcmd.CommandText = myString;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		//int myLast = -10000; //dbcon.LastInsertRowId;
		//http://stackoverflow.com/questions/4341178/getting-the-last-insert-id-with-sqlite-net-in-c
		myString = @"select last_insert_rowid()";
		dbcmd.CommandText = myString;
		int myLast = Convert.ToInt32(dbcmd.ExecuteScalar()); // Need to type-cast since `ExecuteScalar` returns an object.

		if(! dbconOpened)
			Sqlite.Close();

		return myLast;
	}

	//This is like SqlitePersonSession.Selectbut this returns a Person
	
	public static Person Select(bool dbconOpened, int uniqueID) {
		return Select(dbconOpened, " WHERE uniqueID = " + uniqueID);
	}
	public static Person Select(int uniqueID) {
		return Select(false, " WHERE uniqueID = " + uniqueID);
	}
	public static Person SelectByName(bool dbconOpened, string name) {
		return Select(dbconOpened, " WHERE name = \"" + name + "\"");
	}
	public static Person SelectByRFID(string rfid) {
		return Select(false, " WHERE future1 = \"" + rfid + "\"");
	}
	public static Person Select(bool dbconOpened, string whereStr)
	{
		if(! dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "SELECT * FROM " + Constants.PersonTable + " " + whereStr;
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		
		Person p = new Person(-1);
		if(reader.Read()) {
			p = new Person(
					Convert.ToInt32(reader[0].ToString()), //uniqueID
					reader[1].ToString(), 			//name
					reader[2].ToString(), 			//sex
					UtilDate.FromSql(reader[3].ToString()),//dateBorn
					Convert.ToInt32(reader[4].ToString()), //race
					Convert.ToInt32(reader[5].ToString()), //countryID
					reader[6].ToString(), 			//description
					reader[7].ToString(), 			//future1: rfid
					reader[8].ToString(), 			//future2: clubID
					Convert.ToInt32(reader[9].ToString()), //serverUniqueID
					reader[10].ToString() 			//linkServerImage
					);
		}
		reader.Close();
		if(! dbconOpened)
			Sqlite.Close();

		return p;
	}
		
	//public static string SelectJumperName(int uniqueID)
	//select strings
	public static string SelectAttribute(int uniqueID, string attribute)
	{
		Sqlite.Open();

		dbcmd.CommandText = "SELECT " + attribute + " FROM " + Constants.PersonTable + " WHERE uniqueID == " + uniqueID;
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		
		string myReturn = "";
		if(reader.Read()) {
			myReturn = reader[0].ToString();
		}
		reader.Close();
		Sqlite.Close();
		return myReturn;
	}
		
	//currently only used on server
	public static ArrayList SelectAllPersons() 
	{
		Sqlite.Open();
		dbcmd.CommandText = "SELECT uniqueID, name FROM " + Constants.PersonTable; 
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(1);

		while(reader.Read()) 
			myArray.Add ("(" + reader[0].ToString() + ") " + reader[1].ToString());

		reader.Close();
		Sqlite.Close();

		return myArray;
	}
		
	public static ArrayList SelectAllPersonsRecuperable(string sortedBy, int exceptSession, int inSession, string searchFilterName)
	{
		//sortedBy = name or uniqueID (= creation date)
	

		//1st select all the person.uniqueID of people who are in CurrentSession (or none if exceptSession == -1)
		//2n select all names in database (or in one session if inSession != -1)
		//3d filter all names (save all found in 2 that is not in 1)
		//
		//probably this can be made in only one time... future
		//
		//1
		
		string tp = Constants.PersonTable;
		string tps = Constants.PersonSessionTable;

		Sqlite.Open();
		dbcmd.CommandText = "SELECT " + tp + ".uniqueID " +
			" FROM " + tp + "," + tps +
			" WHERE " + tps + ".sessionID == " + exceptSession +
			" AND " + tp + ".uniqueID == " + tps + ".personID "; 
		LogB.SQL(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList arrayExcept = new ArrayList(2);

		while(reader.Read()) 
			arrayExcept.Add (reader[0].ToString());

		reader.Close();
		Sqlite.Close();
		
		//2
		//sort no case sensitive when we sort by name
		if(sortedBy == "name") { 
			sortedBy = "lower(" + tp + ".name)" ; 
		} else { 
			sortedBy = tp + ".uniqueID" ; 
		}
		
		Sqlite.Open();
		if(inSession == -1) {
			string nameLike = "";
			if(searchFilterName != "")
				nameLike = " WHERE LOWER(" + tp + ".name) LIKE LOWER (\"%" + searchFilterName + "%\") ";

			dbcmd.CommandText = 
				"SELECT * FROM " + tp + 
				nameLike + 
				" ORDER BY " + sortedBy;

		} else {
			dbcmd.CommandText = 
				"SELECT " + tp + ".* FROM " + tp + ", " + tps + 
				" WHERE " + tps + ".sessionID == " + inSession + 
				" AND " + tp + ".uniqueID == " + tps + ".personID " + 
				" ORDER BY " + sortedBy;
		}
		LogB.SQL(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader2;
		reader2 = dbcmd.ExecuteReader();

		ArrayList arrayReturn = new ArrayList(2);

		bool found;

		//3
		while(reader2.Read()) {
			found = false;
			foreach (string line in arrayExcept) {
				if(line == reader2[0].ToString()) {
					found = true;
					goto finishForeach;
				}
			}
			
finishForeach:
			
			if (!found) {
				Person p = new Person(
						Convert.ToInt32(reader2[0].ToString()), //uniqueID
						reader2[1].ToString(), 			//name
						reader2[2].ToString(), 			//sex
						UtilDate.FromSql(reader2[3].ToString()),//dateBorn
						Convert.ToInt32(reader2[4].ToString()), //race
						Convert.ToInt32(reader2[5].ToString()), //countryID
						reader2[6].ToString(), 			//description
						reader2[7].ToString(), 			//future1: rfid
						reader2[8].ToString(), 			//future2: clubID
						Convert.ToInt32(reader2[9].ToString()), //serverUniqueID
						reader2[10].ToString() 			//linkServerImage
						);
				arrayReturn.Add(p);
			}
		}

		reader2.Close();
		Sqlite.Close();

		return arrayReturn;
	}

	/*
	   unused, now used SqliteSession.SelectAllSessionsTestsCount

	public static ArrayList SelectAllPersonEvents (int personID)
	{
		SqliteDataReader reader;
		ArrayList arraySessions = new ArrayList(2);
		ArrayList arrayJumps = new ArrayList(2);
		ArrayList arrayJumpsRj = new ArrayList(2);
		ArrayList arrayRuns = new ArrayList(2);
		ArrayList arrayRunsInterval = new ArrayList(2);
		ArrayList arrayRTs = new ArrayList(2);
		ArrayList arrayPulses = new ArrayList(2);
		ArrayList arrayMCs = new ArrayList(2);
		ArrayList arrayEncS = new ArrayList(2);
		ArrayList arrayEncC = new ArrayList(2);
		ArrayList arrayForceIsometric = new ArrayList(2);
		ArrayList arrayForceElastic = new ArrayList(2);
		ArrayList arrayRunEncoder = new ArrayList(2);
		
		string tps = Constants.PersonSessionTable;
	
		Sqlite.Open();
		
		//session where this person is loaded
		dbcmd.CommandText = "SELECT sessionID, session.Name, session.Place, session.Date " + 
			" FROM " + tps + ", session " + 
			" WHERE personID = " + personID + " AND session.uniqueID == " + tps + ".sessionID " +
			" ORDER BY sessionID";
		LogB.SQL(dbcmd.CommandText.ToString());
		
		reader = dbcmd.ExecuteReader();
		while(reader.Read()) {
			arraySessions.Add ( reader[0].ToString() + ":" + reader[1].ToString() + ":" +
					reader[2].ToString() + ":" + 
					UtilDate.FromSql(reader[3].ToString()).ToShortDateString()
					);
		}
		reader.Close();

		
		//jumps
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM jump WHERE personID = " + personID +
			" GROUP BY sessionID ORDER BY sessionID";
		LogB.SQL(dbcmd.CommandText.ToString());
		
		reader = dbcmd.ExecuteReader();
		while(reader.Read()) {
			arrayJumps.Add ( reader[0].ToString() + ":" + reader[1].ToString() );
		}
		reader.Close();
		
		//jumpsRj
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM jumpRj WHERE personID = " + personID +
			" GROUP BY sessionID ORDER BY sessionID";
		LogB.SQL(dbcmd.CommandText.ToString());
		
		reader = dbcmd.ExecuteReader();
		while(reader.Read()) {
			arrayJumpsRj.Add ( reader[0].ToString() + ":" + reader[1].ToString() );
		}
		reader.Close();
		
		//runs
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM run WHERE personID = " + personID +
			" GROUP BY sessionID ORDER BY sessionID";
		LogB.SQL(dbcmd.CommandText.ToString());
		
		reader = dbcmd.ExecuteReader();
		while(reader.Read()) {
			arrayRuns.Add ( reader[0].ToString() + ":" + reader[1].ToString() );
		}
		reader.Close();
		
		//runsInterval
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM runInterval WHERE personID = " + personID +
			" GROUP BY sessionID ORDER BY sessionID";
		LogB.SQL(dbcmd.CommandText.ToString());
		
		reader = dbcmd.ExecuteReader();
		while(reader.Read()) {
			arrayRunsInterval.Add ( reader[0].ToString() + ":" + reader[1].ToString() );
		}
		reader.Close();
		
		//reaction time
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM reactiontime WHERE personID = " + personID +
			" GROUP BY sessionID ORDER BY sessionID";
		LogB.SQL(dbcmd.CommandText.ToString());
		
		reader = dbcmd.ExecuteReader();
		while(reader.Read()) {
			arrayRTs.Add ( reader[0].ToString() + ":" + reader[1].ToString() );
		}
		reader.Close();
	
		//pulses
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM pulse WHERE personID = " + personID +
			" GROUP BY sessionID ORDER BY sessionID";
		LogB.SQL(dbcmd.CommandText.ToString());
		
		reader = dbcmd.ExecuteReader();
		while(reader.Read()) {
			arrayPulses.Add ( reader[0].ToString() + ":" + reader[1].ToString() );
		}
		reader.Close();
	
		//MC
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM multiChronopic WHERE personID = " + personID +
			" GROUP BY sessionID ORDER BY sessionID";
		LogB.SQL(dbcmd.CommandText.ToString());
		
		reader = dbcmd.ExecuteReader();
		while(reader.Read()) {
			arrayMCs.Add ( reader[0].ToString() + ":" + reader[1].ToString() );
		}
		reader.Close();
	
		//EncS (encoder signal)
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM " + Constants.EncoderTable + 
		       " WHERE personID == " + personID +
		       " AND signalOrCurve == \"signal\" " +
			" GROUP BY sessionID ORDER BY sessionID";
		LogB.SQL(dbcmd.CommandText.ToString());
		
		reader = dbcmd.ExecuteReader();
		while(reader.Read()) {
			arrayEncS.Add ( reader[0].ToString() + ":" + reader[1].ToString() );
		}
		reader.Close();
	
		//EncC (encoder curve)
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM " + Constants.EncoderTable + 
		       " WHERE personID == " + personID +
		       " AND signalOrCurve == \"curve\" " +
			" GROUP BY sessionID ORDER BY sessionID";
		LogB.SQL(dbcmd.CommandText.ToString());
		
		reader = dbcmd.ExecuteReader();
		while(reader.Read()) {
			arrayEncC.Add ( reader[0].ToString() + ":" + reader[1].ToString() );
		}
		reader.Close();

		//forceSensor (isometric)
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM " + Constants.ForceSensorTable +
			" WHERE personID = " + personID +
			" AND " + Constants.ForceSensorTable + ".stiffness < 0" + //isometric has stiffness -1.0
			" GROUP BY sessionID ORDER BY sessionID";
		LogB.SQL(dbcmd.CommandText.ToString());

		reader = dbcmd.ExecuteReader();
		while(reader.Read()) {
			arrayForceIsometric.Add ( reader[0].ToString() + ":" + reader[1].ToString() );
		}
		reader.Close();

		//forceSensor (elastic)
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM " + Constants.ForceSensorTable +
			" WHERE personID = " + personID +
			" AND " + Constants.ForceSensorTable + ".stiffness > 0" + //elastic has stiffness > 0
			" GROUP BY sessionID ORDER BY sessionID";
		LogB.SQL(dbcmd.CommandText.ToString());

		reader = dbcmd.ExecuteReader();
		while(reader.Read()) {
			arrayForceElastic.Add ( reader[0].ToString() + ":" + reader[1].ToString() );
		}
		reader.Close();

		//runEncoder
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM " + Constants.RunEncoderTable + " WHERE personID = " + personID +
			" GROUP BY sessionID ORDER BY sessionID";
		LogB.SQL(dbcmd.CommandText.ToString());

		reader = dbcmd.ExecuteReader();
		while(reader.Read()) {
			arrayRunEncoder.Add ( reader[0].ToString() + ":" + reader[1].ToString() );
		}
		reader.Close();



		Sqlite.Close();
		
	
		ArrayList arrayAll = new ArrayList(2);
		string tempJumps;
		string tempJumpsRj;
		string tempRuns;
		string tempRunsInterval;
		string tempRTs;
		string tempPulses;
		string tempMCs;
		string tempEncS;
		string tempEncC;
		string tempForceIsometric;
		string tempForceElastic;
		string tempRunEncoder;
		bool found; 	//using found because a person can be loaded in a session 
				//but whithout having done any event yet

		//foreach session where this jumper it's loaded, check which events has
		foreach (string mySession in arraySessions) {
			string [] myStrSession = mySession.Split(new char[] {':'});
			tempJumps = "";
			tempJumpsRj = "";
			tempRuns = "";
			tempRunsInterval = "";
			tempRTs = "";
			tempPulses = "";
			tempMCs = "";
			tempEncS = "";
			tempEncC = "";
			tempForceIsometric = "";
			tempForceElastic = "";
			tempRunEncoder = "";
			found = false;
			
			foreach (string myJumps in arrayJumps) {
				string [] myStr = myJumps.Split(new char[] {':'});
				if(myStrSession[0] == myStr[0]) {
					tempJumps = myStr[1];
					found = true;
					break;
				}
			}
		
			foreach (string myJumpsRj in arrayJumpsRj) {
				string [] myStr = myJumpsRj.Split(new char[] {':'});
				if(myStrSession[0] == myStr[0]) {
					tempJumpsRj = myStr[1];
					found = true;
					break;
				}
			}
			
			foreach (string myRuns in arrayRuns) {
				string [] myStr = myRuns.Split(new char[] {':'});
				if(myStrSession[0] == myStr[0]) {
					tempRuns = myStr[1];
					found = true;
					break;
				}
			}
			
			foreach (string myRunsInterval in arrayRunsInterval) {
				string [] myStr = myRunsInterval.Split(new char[] {':'});
				if(myStrSession[0] == myStr[0]) {
					tempRunsInterval = myStr[1];
					found = true;
					break;
				}
			}
			
			foreach (string myRTs in arrayRTs) {
				string [] myStr = myRTs.Split(new char[] {':'});
				if(myStrSession[0] == myStr[0]) {
					tempRTs = myStr[1];
					found = true;
					break;
				}
			}
			
			foreach (string myPulses in arrayPulses) {
				string [] myStr = myPulses.Split(new char[] {':'});
				if(myStrSession[0] == myStr[0]) {
					tempPulses = myStr[1];
					found = true;
					break;
				}
			}
			
			foreach (string myMCs in arrayMCs) {
				string [] myStr = myMCs.Split(new char[] {':'});
				if(myStrSession[0] == myStr[0]) {
					tempMCs = myStr[1];
					found = true;
					break;
				}
			}

			foreach (string myEncS in arrayEncS) {
				string [] myStr = myEncS.Split(new char[] {':'});
				if(myStrSession[0] == myStr[0]) {
					tempEncS = myStr[1];
					found = true;
					break;
				}
			}

			foreach (string myEncC in arrayEncC) {
				string [] myStr = myEncC.Split(new char[] {':'});
				if(myStrSession[0] == myStr[0]) {
					tempEncC = myStr[1];
					found = true;
					break;
				}
			}

			foreach (string f in arrayForceIsometric) {
				string [] myStr = f.Split(new char[] {':'});
				if(myStrSession[0] == myStr[0]) {
					tempForceIsometric = myStr[1];
					found = true;
					break;
				}
			}

			foreach (string f in arrayForceElastic) {
				string [] myStr = f.Split(new char[] {':'});
				if(myStrSession[0] == myStr[0]) {
					tempForceElastic = myStr[1];
					found = true;
					break;
				}
			}

			foreach (string re in arrayRunEncoder) {
				string [] myStr = re.Split(new char[] {':'});
				if(myStrSession[0] == myStr[0]) {
					tempRunEncoder = myStr[1];
					found = true;
					break;
				}
			}


			//if has events, write it's data
			if (found) {
				arrayAll.Add (myStrSession[1] + ":" + myStrSession[2] + ":" + 	//session name, place
						myStrSession[3] + ":" + tempJumps + ":" + 	//sessionDate, jumps
						tempJumpsRj + ":" + tempRuns + ":" + 		//jumpsRj, Runs
						tempRunsInterval + ":" + tempRTs + ":" + 	//runsInterval, Reaction times
						tempPulses + ":" + tempMCs + ":" +		//pulses, MultiChronopic
						tempEncS + ":" + tempEncC + ":" + 		//encoder signal, encoder curve
						tempForceIsometric + ":" + tempForceElastic + ":" + //forceSensor
						tempRunEncoder 		//runEncoder
						);
			}
		}

		return arrayAll;
	}
*/
	
	public static bool ExistsAndItsNotMe(int uniqueID, string personName)
	{
		Sqlite.Open();
		dbcmd.CommandText = "SELECT uniqueID FROM " + Constants.PersonTable +
			" WHERE LOWER(" + Constants.PersonTable + ".name) == LOWER(\"" + personName + "\")" +
			" AND uniqueID != " + uniqueID ;
		LogB.SQL(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
	
		bool exists = new bool();
		exists = false;
		
		if (reader.Read()) {
			exists = true;
			//LogB.SQL("valor {0}", reader[0].ToString());
		}
		//LogB.SQL("exists = {0}", exists.ToString());

		reader.Close();
		Sqlite.Close();
		return exists;
	}
	
	
	public static void Update(Person myPerson)
	{
		Sqlite.Open();
		dbcmd.CommandText = "UPDATE " + Constants.PersonTable + 
			" SET name = \"" + myPerson.Name + 
			"\", sex = \"" + myPerson.Sex +
			"\", dateborn = \"" + UtilDate.ToSql(myPerson.DateBorn) +
			"\", race = " + myPerson.Race +
			", countryID = " + myPerson.CountryID +
			", description = \"" + myPerson.Description +
			"\", future1 = \"" + myPerson.Future1 + 		//rfid
			"\", future2 = \"" + myPerson.Future2 + 		//clubID
			"\", serverUniqueID = " + myPerson.ServerUniqueID +
			", linkServerImage = \"" + myPerson.LinkServerImage + //linkServerImage
			"\" WHERE uniqueID == " + myPerson.UniqueID;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		Sqlite.Close();
	}

	//used on compujump
	public static void UpdateName (int uniqueID, string name)
	{
		Sqlite.Open();
		dbcmd.CommandText = "UPDATE " + Constants.PersonTable +
			" SET name = \"" + name +
			"\" WHERE uniqueID = " + uniqueID;

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		Sqlite.Close();
	}

	//used on compujump
	public static void UpdateRFID (int uniqueID, string rfid)
	{
		Sqlite.Open();
		dbcmd.CommandText = "UPDATE " + Constants.PersonTable +
			" SET future1 = \"" + rfid +
			"\" WHERE uniqueID = " + uniqueID;

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		Sqlite.Close();
	}

	public static void DeletePersonAndImages (bool dbconOpened, int uniqueID)
	{
		// 1) delete from DB
		openIfNeeded (dbconOpened);

		dbcmd.CommandText = "DELETE FROM " + Constants.PersonTable + " WHERE uniqueID = " + uniqueID;

		LogB.SQL (dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery ();

		closeIfNeeded (dbconOpened);

		// 2) delete photos if any
		if (File.Exists (Util.UserPhotoURL (false, uniqueID)))
			File.Delete (Util.UserPhotoURL (false, uniqueID));
		if (File.Exists (Util.UserPhotoURL (true, uniqueID)))
			File.Delete (Util.UserPhotoURL (true, uniqueID));
	}
}
