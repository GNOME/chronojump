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
using Mono.Unix;


class SqlitePersonSession : Sqlite
{
	protected internal static void createTable()
	 {
		dbcmd.CommandText = 
			"CREATE TABLE " + Constants.PersonSessionWeightTable + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"personID INT, " +
			"sessionID INT, " +
			"weight INT)";		
		dbcmd.ExecuteNonQuery();
	 }

	public static int Insert(int personID, int sessionID, int weight)
	{
		dbcon.Open();
		dbcmd.CommandText = "INSERT INTO " + Constants.PersonSessionWeightTable + 
			"(personID, sessionID, weight) VALUES ("
			+ personID + ", " + sessionID + ", " + weight + ")" ;
		dbcmd.ExecuteNonQuery();
		int myReturn = dbcon.LastInsertRowId;
		dbcon.Close();
		return myReturn;
	}
	
	/* new in database 0.53 (weight can change in different sessions) */
	public static double SelectPersonWeight(int personID, int sessionID)
	{
		dbcon.Open();

		dbcmd.CommandText = "SELECT weight FROM " + Constants.PersonSessionWeightTable +
		       	" WHERE personID == " + personID + 
			" AND sessionID == " + sessionID;
		
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		
		double myReturn = 0;
		if(reader.Read()) {
			myReturn = Convert.ToDouble(Util.ChangeDecimalSeparator(reader[0].ToString()));
		}
		dbcon.Close();
		return myReturn;
	}

	/* when a session is not know, then select last weight */	
	/* new in database 0.53 (weight can change in different sessions) */
	public static double SelectPersonWeight(int personID)
	{
		dbcon.Open();

		dbcmd.CommandText = "SELECT weight, sessionID FROM " + Constants.PersonSessionWeightTable + 
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
		dbcon.Close();
		return myReturn;
	}
	
	public static void UpdateWeight(int personID, int sessionID, int weight)
	{
		dbcon.Open();
		dbcmd.CommandText = "UPDATE " + Constants.PersonSessionWeightTable + 
			" SET weight = " + weight + 
			" WHERE personID = " + personID +
			" AND sessionID = " + sessionID
			;
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		dbcon.Close();
	}

	public static bool PersonExistsAndItsNotMe(int uniqueID, string personName)
	{
		dbcon.Open();
		dbcmd.CommandText = "SELECT uniqueID FROM " + Constants.PersonTable +
			" WHERE LOWER(person.name) == LOWER('" + personName + "')" +
			" AND uniqueID != " + uniqueID ;
		Log.WriteLine(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
	
		bool exists = new bool();
		exists = false;
		
		if (reader.Read()) {
			exists = true;
			//Log.WriteLine("valor {0}", reader[0].ToString());
		}
		//Log.WriteLine("exists = {0}", exists.ToString());

		dbcon.Close();
		return exists;
	}
	
	public static bool PersonSelectExistsInSession(string myPersonID, int mySessionID)
	{
		dbcon.Open();
		dbcmd.CommandText = "SELECT * FROM " + Constants.PersonSessionWeightTable +
			" WHERE personID == " + myPersonID + 
			" AND sessionID == " + mySessionID ; 
		Log.WriteLine(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
	
		bool exists = new bool();
		exists = false;
		
		while(reader.Read()) {
			exists = true;
			//Log.WriteLine("valor {0}", reader[0].ToString());
		}
		//Log.WriteLine("exists = {0}", exists.ToString());

		dbcon.Close();
		return exists;
	}
	
	public static Person PersonSelect(int uniqueID, int sessionID)
	{
		dbcon.Open();
		//dbcmd.CommandText = "SELECT name, sex, dateborn, height, weight, description " +
		dbcmd.CommandText = "SELECT person.name, person.sex, person.dateborn, person.height, " +
			"personSessionWeight.weight, person.sportID, person.speciallityID, person.practice, person.description, " +
			"person.race, person.countryID, person.serverUniqueID " +
			" FROM person, personSessionWeight WHERE person.uniqueID == " + uniqueID + 
			" AND personSessionWeight.sessionID == " + sessionID +
			" AND person.uniqueID == personSessionWeight.personID";
		Log.WriteLine(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
	
		string [] values = new string[12];

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
			values[9] = reader[9].ToString();
			values[10] = reader[10].ToString();
			values[11] = reader[11].ToString();
		}

		Person myPerson = new Person(uniqueID, values[0], 
			values[1], values[2], Convert.ToInt32(values[3]), Convert.ToInt32(values[4]), 
			Convert.ToInt32(values[5]), Convert.ToInt32(values[6]), Convert.ToInt32(values[7]),
			values[8], //desc
			Convert.ToInt32(values[9]), Convert.ToInt32(values[10]), Convert.ToInt32(values[11])
			); 
		
		dbcon.Close();
		return myPerson;
	}
	
	public static string[] SelectCurrentSession(int sessionID, bool onlyIDAndName, bool reverse) 
	{
		dbcon.Open();
		dbcmd.CommandText = "SELECT person.*, personSessionWeight.weight, sport.name, speciallity.name " +
			"FROM person, personSessionWeight, sport, speciallity " +
			" WHERE personSessionWeight.sessionID == " + sessionID + 
			" AND person.uniqueID == personSessionWeight.personID " + 
			" AND person.sportID == sport.uniqueID " + 
			" AND person.speciallityID == speciallity.uniqueID " + 
			" ORDER BY upper(person.name)";
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
				string sportName = Catalog.GetString(reader[11].ToString());

				string speciallityName = ""; //to solve a gettext bug (probably because speciallity undefined name is "")
				if(reader[12].ToString() != "")
					speciallityName = Catalog.GetString(reader[12].ToString());
				string levelName = Catalog.GetString(Util.FindLevelName(Convert.ToInt32(reader[8])));

				myArray.Add (
						reader[0].ToString() + ":" + reader[1].ToString() + ":" + //id, name
						reader[2].ToString() + ":" + reader[3].ToString() + ":" + //sex, dateborn
						reader[4].ToString() + ":" + reader[10].ToString() + ":" + //height, weight (from personSessionWeight)
						sportName + ":" + speciallityName + ":" + levelName + ":" +
						reader[9].ToString()  //desc
					    );
			}
			count ++;
		}

		reader.Close();
		dbcon.Close();

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
	
	public static void DeletePersonFromSessionAndTests(string sessionID, string personID)
	{
		dbcon.Open();

		//delete relations (existance) within persons and sessions in this session
		dbcmd.CommandText = "Delete FROM personSessionWeight WHERE sessionID == " + sessionID +
			" AND personID == " + personID;
		dbcmd.ExecuteNonQuery();
		
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
		
		
		dbcon.Close();
	}

	/* 
	 * conversion from database 0.52 to 0.53 (add weight into personSession)
	 * now weight of a person can change every session
	*/
	protected internal static void moveOldTableToNewTable() 
	{
		dbcon.Open();
			
		dbcmd.CommandText = "SELECT personSession.*, person.weight " + 
			"FROM personSession, person " + 
			"WHERE personSession.personID = person.UniqueID " + 
			"ORDER BY sessionID, personID";
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		
		ArrayList myArray = new ArrayList(2);
		while(reader.Read()) {
			//reader[0] (uniqueID (of table)) is not used
			myArray.Add (reader[1].ToString() + ":" + reader[2].ToString() + ":" + reader[3].ToString());
		}
		reader.Close();

		dropOldTable();

		dbcon.Close();
			
		foreach (string line in myArray) {
			string [] stringFull = line.Split(new char[] {':'});
			Insert(
					Convert.ToInt32(stringFull[0]), //personID
					Convert.ToInt32(stringFull[1]), //sessionID
					Convert.ToInt32(stringFull[2]) //weight
			      );
		}
	}
	
	/* 
	 * conversion from database 0.52 to 0.53 (add weight into personSession)
	 * now weight of a person can change every session
	*/
	private static void dropOldTable() {
		dbcmd.CommandText = "DROP TABLE " + Constants.PersonSessionTable;
		dbcmd.ExecuteNonQuery();
	}

}

