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


class SqlitePerson : Sqlite
{
	//can be "Constants.PersonTable" or "Constants.TempPersonTable"
	//temp is used to modify table between different database versions if needed
	protected internal static void createTable(string tableName)
	 {
		dbcmd.CommandText = 
			"CREATE TABLE " + tableName + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"name TEXT, " +
			"sex TEXT, " +
			"dateborn TEXT, " +
			"height TEXT, " +
			"weight TEXT, " + //now used personSession and person can change weight in every session. person.weight is not used
			"sportID INT, " + 
			"speciallityID INT, " + 
			"practice INT, " + //also called "level"
			"description TEXT )";		
		dbcmd.ExecuteNonQuery();
	 }

	//can be "Constants.PersonTable" or "Constants.TempPersonTable"
	//temp is used to modify table between different database versions if needed
	//public static int Insert(bool dbconOpened, string tableName, string name, string sex, string dateBorn, int height, int weight, int sportID, bool sportUserDefined, int practice, string description)
	public static int Insert(bool dbconOpened, string tableName, string name, string sex, string dateBorn, int height, int weight, int sportID, int speciallityID, int practice, string description)
	{
		if(! dbconOpened)
			dbcon.Open();

		string myString = "INSERT INTO " + tableName + 
			//" (uniqueID, name, sex, dateBorn, height, weight,  sportID, sportUserDefined, practice, description) VALUES (NULL, '" +
			" (uniqueID, name, sex, dateBorn, height, weight,  sportID, speciallityID, practice, description) VALUES (NULL, '" +
			name + "', '" + sex + "', '" + dateBorn + "', " + 
			height + ", " + "-1" + ", " + //"-1" is weight because it's defined in personSesionWeight for allow change between sessions
			sportID + ", " + speciallityID + ", " + practice + ", '" + description + "')" ;
		
		dbcmd.CommandText = myString;
		dbcmd.ExecuteNonQuery();
		int myReturn = dbcon.LastInsertRowId;

		if(! dbconOpened)
			dbcon.Close();

		return myReturn;
	}

	public static string SelectJumperName(int uniqueID)
	{
		dbcon.Open();

		dbcmd.CommandText = "SELECT name FROM " + Constants.PersonTable + " WHERE uniqueID == " + uniqueID;
		
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		
		string myReturn = "";
		if(reader.Read()) {
			myReturn = reader[0].ToString();
		}
		dbcon.Close();
		return myReturn;
	}
		
	public static string[] SelectAllPersonsRecuperable(string sortedBy, int except, int inSession, string searchFilterName) 
	{
		//sortedBy = name or uniqueID (= creation date)
	

		//1st select all the person.uniqueID of people who are in CurrentSession (or none if except == -1)
		//2n select all names in database (or in one session if inSession != -1)
		//3d filter all names (save all found in 2 that is not in 1)
		//
		//probably this can be made in only one time... future
		//
		//1
		
		dbcon.Open();
		dbcmd.CommandText = "SELECT person.uniqueID " +
			" FROM person, personSessionWeight " +
			" WHERE personSessionWeight.sessionID == " + except + 
			" AND person.uniqueID == personSessionWeight.personID "; 
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);

		int count = new int();
		count = 0;

		while(reader.Read()) {
			myArray.Add (reader[0].ToString());
			count ++;
		}

		reader.Close();
		dbcon.Close();
		
		//2
		//sort no case sensitive when we sort by name
		if(sortedBy == "name") { 
			sortedBy = "lower(person.name)" ; 
		} else { 
			sortedBy = "person.uniqueID" ; 
		}
		
		dbcon.Open();
		if(inSession == -1) {
			string nameLike = "";
			if(searchFilterName != "")
				nameLike = "LOWER(person.name) LIKE LOWER ('%" + searchFilterName + "%') AND ";

			dbcmd.CommandText = 
				"SELECT person.*, personSessionWeight.weight, sport.Name, speciallity.Name  " +
				" FROM person, personSessionWeight, sport, speciallity " + 
				" WHERE " + nameLike + " person.UniqueID == personSessionWeight.personID " +
				" AND person.sportID == sport.UniqueID AND person.speciallityID == speciallity.UniqueID " +
				" GROUP BY person.uniqueID" +
				" ORDER BY " + sortedBy;
		} else {
			dbcmd.CommandText = 
				"SELECT person.*, personSessionWeight.weight, sport.Name, speciallity.Name " +
				" FROM person, personSessionWeight, sport, speciallity " + 
				" WHERE personSessionWeight.sessionID == " + inSession + 
				" AND person.uniqueID == personSessionWeight.personID " + 
				" AND person.sportID == sport.UniqueID AND person.speciallityID == speciallity.UniqueID " +
				" ORDER BY " + sortedBy;
		}
		Console.WriteLine(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader2;
		reader2 = dbcmd.ExecuteReader();

		ArrayList myArray2 = new ArrayList(2);

		int count2 = new int();
		count2 = 0;
		bool found;

		//3
		while(reader2.Read()) {
			found = false;
			foreach (string line in myArray) {
				if(line == reader2[0].ToString()) {
					found = true;
					goto finishForeach;
				}
			}
			
finishForeach:
			
			if (!found) {
				myArray2.Add (reader2[0].ToString() + ":" + reader2[1].ToString() + ":" +
						reader2[2].ToString() + ":" + reader2[3].ToString() + ":" +
						reader2[4].ToString() + ":" + 
						reader2[10].ToString() + ":" + //weight (from personSessionWeight)
						reader2[11].ToString() + ":" + //sportName
						reader2[12].ToString() + ":" + //speciallityName
						Util.FindLevelName(Convert.ToInt32(reader2[8])) + ":" + //levelName
						reader2[9].ToString() //description
						);
				count2 ++;
			}
		}

		reader2.Close();
		dbcon.Close();

		string [] myPersons = new string[count2];
		count2 = 0;
		foreach (string line in myArray2) {
			myPersons [count2++] = line;
		}

		return myPersons;
	}

	public static ArrayList SelectAllPersonEvents(int personID) 
	{
		SqliteDataReader reader;
		ArrayList arraySessions = new ArrayList(2);
		ArrayList arrayJumps = new ArrayList(2);
		ArrayList arrayJumpsRj = new ArrayList(2);
		ArrayList arrayRuns = new ArrayList(2);
		ArrayList arrayRunsInterval = new ArrayList(2);
		ArrayList arrayRTs = new ArrayList(2);
		ArrayList arrayPulses = new ArrayList(2);
	
		dbcon.Open();
		
		//session where this person is loaded
		dbcmd.CommandText = "SELECT sessionID, session.Name, session.Place, session.Date " + 
			" FROM personSessionWeight, session " + 
			" WHERE personID = " + personID + " AND session.uniqueID == personSessionWeight.sessionID " +
			" ORDER BY sessionID";
		Console.WriteLine(dbcmd.CommandText.ToString());
		
		reader = dbcmd.ExecuteReader();
		while(reader.Read()) {
			arraySessions.Add ( reader[0].ToString() + ":" + reader[1].ToString() + ":" +
					reader[2].ToString() + ":" + reader[3].ToString() );
		}
		reader.Close();

		
		//jumps
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM jump WHERE personID = " + personID +
			" GROUP BY sessionID ORDER BY sessionID";
		Console.WriteLine(dbcmd.CommandText.ToString());
		
		reader = dbcmd.ExecuteReader();
		while(reader.Read()) {
			arrayJumps.Add ( reader[0].ToString() + ":" + reader[1].ToString() );
		}
		reader.Close();
		
		//jumpsRj
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM jumpRj WHERE personID = " + personID +
			" GROUP BY sessionID ORDER BY sessionID";
		Console.WriteLine(dbcmd.CommandText.ToString());
		
		reader = dbcmd.ExecuteReader();
		while(reader.Read()) {
			arrayJumpsRj.Add ( reader[0].ToString() + ":" + reader[1].ToString() );
		}
		reader.Close();
		
		//runs
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM run WHERE personID = " + personID +
			" GROUP BY sessionID ORDER BY sessionID";
		Console.WriteLine(dbcmd.CommandText.ToString());
		
		reader = dbcmd.ExecuteReader();
		while(reader.Read()) {
			arrayRuns.Add ( reader[0].ToString() + ":" + reader[1].ToString() );
		}
		reader.Close();
		
		//runsInterval
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM runInterval WHERE personID = " + personID +
			" GROUP BY sessionID ORDER BY sessionID";
		Console.WriteLine(dbcmd.CommandText.ToString());
		
		reader = dbcmd.ExecuteReader();
		while(reader.Read()) {
			arrayRunsInterval.Add ( reader[0].ToString() + ":" + reader[1].ToString() );
		}
		reader.Close();
		
		//reaction time
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM reactiontime WHERE personID = " + personID +
			" GROUP BY sessionID ORDER BY sessionID";
		Console.WriteLine(dbcmd.CommandText.ToString());
		
		reader = dbcmd.ExecuteReader();
		while(reader.Read()) {
			arrayRTs.Add ( reader[0].ToString() + ":" + reader[1].ToString() );
		}
		reader.Close();
	
		//pulses
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM pulse WHERE personID = " + personID +
			" GROUP BY sessionID ORDER BY sessionID";
		Console.WriteLine(dbcmd.CommandText.ToString());
		
		reader = dbcmd.ExecuteReader();
		while(reader.Read()) {
			arrayPulses.Add ( reader[0].ToString() + ":" + reader[1].ToString() );
		}
		reader.Close();
	
	
		dbcon.Close();
		
	
		ArrayList arrayAll = new ArrayList(2);
		string tempJumps;
		string tempJumpsRj;
		string tempRuns;
		string tempRunsInterval;
		string tempRTs;
		string tempPulses;
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
			


			//if has events, write it's data
			if (found) {
				arrayAll.Add (myStrSession[1] + ":" + myStrSession[2] + ":" + 	//session name, place
						myStrSession[3] + ":" + tempJumps + ":" + 	//sessionDate, jumps
						tempJumpsRj + ":" + tempRuns + ":" + 		//jumpsRj, Runs
						tempRunsInterval + ":" + tempRTs + ":" + 	//runsInterval, Reaction times
						tempPulses);					//pulses
			}
		}

		return arrayAll;
	}
	
	public static void Update(Person myPerson)
	{
		dbcon.Open();
		dbcmd.CommandText = "UPDATE " + Constants.PersonTable + 
			" SET name = '" + myPerson.Name + 
			"', sex = '" + myPerson.Sex +
			"', dateborn = '" + myPerson.DateBorn +
			"', height = " + myPerson.Height +
			", weight = " + myPerson.Weight +
			", sportID = " + myPerson.SportID +
			", speciallityID = " + myPerson.SpeciallityID +
			", practice = " + myPerson.Practice +
			", description = '" + myPerson.Description +
			"' WHERE uniqueID == '" + myPerson.UniqueID + "'" ;
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		dbcon.Close();
	}

	
	public static void Delete()
	{
	}

	//change DB from 0.53 to 0.54	
	protected internal static void convertTableToSportRelated() 
	{
		ArrayList myArray = new ArrayList(2);

		//1st create a temp table
		createTable(Constants.TempPersonTable);
			
		//2nd copy all data from person table to temp table
		dbcmd.CommandText = "SELECT * " + 
			"FROM " + Constants.PersonTable + " ORDER BY uniqueID"; 
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		while(reader.Read()) {
			Person myPerson = new Person(Convert.ToInt32(reader[0]), reader[1].ToString(), reader[2].ToString(), reader[3].ToString(), Convert.ToInt32(reader[4]), Convert.ToInt32(reader[5]),
					1, //sport undefined
					-1, //speciallity undefined
					-1, //practice level undefined
					reader[6].ToString()); //desc
			myArray.Add(myPerson);

		}
		reader.Close();

		foreach (Person myPerson in myArray)
			Insert(true, Constants.TempPersonTable,
				myPerson.Name, myPerson.Sex, myPerson.DateBorn, 
				myPerson.Height, myPerson.Weight, myPerson.SportID, myPerson.SpeciallityID, myPerson.Practice, myPerson.Description);

		//3rd drop table persons
		dropTable(Constants.PersonTable);

		//4d create table persons (now with sport related stuff
		createTable(Constants.PersonTable);

		//5th insert data in persons (with sport related stuff)
		foreach (Person myPerson in myArray) 
			Insert(true, Constants.PersonTable,
				myPerson.Name, myPerson.Sex, myPerson.DateBorn, 
				//myPerson.Height, myPerson.Weight, myPerson.SportID, myPerson.SportUserDefined, myPerson.Practice, myPerson.Description);
				myPerson.Height, myPerson.Weight, myPerson.SportID, myPerson.SpeciallityID, myPerson.Practice, myPerson.Description);


		//6th drop temp table
		dropTable(Constants.TempPersonTable);
			
	}
	
	private static void dropTable(string tableName) {
		dbcmd.CommandText = "DROP TABLE " + tableName;
		dbcmd.ExecuteNonQuery();
	}

}
