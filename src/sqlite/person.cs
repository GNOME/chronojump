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


class SqlitePerson : Sqlite
{
	protected static void createTable()
	 {
		dbcmd.CommandText = 
			"CREATE TABLE person ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"name TEXT, " +
			"sex TEXT, " +
			"dateborn TEXT, " +
			"height TEXT, " +
			"weight TEXT, " +
			"description TEXT )";		
		dbcmd.ExecuteNonQuery();
	 }

	public static int Insert(string name, string sex, string dateBorn, int height, int weight, string description)
	{
		dbcon.Open();

		string myString = "INSERT INTO person (uniqueID, name, sex, dateBorn, height, weight, description) VALUES (NULL, '" +
			name + "', '" + sex + "', '" + dateBorn + "', " + 
			height + ", " + weight + ", '" + description + "')" ;
		
		dbcmd.CommandText = myString;
		dbcmd.ExecuteNonQuery();
		int myReturn = dbcon.LastInsertRowId;
		dbcon.Close();
		return myReturn;
	}

	public static string SelectJumperName(int uniqueID)
	{
		dbcon.Open();

		dbcmd.CommandText = "SELECT name FROM person WHERE uniqueID == " + uniqueID;
		
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		
		string myReturn = "";
		if(reader.Read()) {
			myReturn = reader[0].ToString();
		}
		return myReturn;
	}
		

/*
	public static string[] SelectAllPersons(string orderedBy) 
	{
		//orderedBy = name or uniqueID (= creation date)
		
		dbcon.Open();

		dbcmd.CommandText = "SELECT * FROM " + personTableName + " ORDER BY " + orderedBy;
		
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);

		int count = new int();
		count = 0;

		//we don't include height and weight, now
		while(reader.Read()) {
			myArray.Add (reader[0].ToString() + ":" + reader[1].ToString() + ":" +
					reader[2].ToString() + ":" + reader[3].ToString() + ":" +
					reader[6].ToString()
					);
			count ++;
		}

		reader.Close();
		dbcon.Close();

		string [] myPersons = new string[count];
		count =0;
		foreach (string line in myArray) {
			myPersons [count++] = line;
		}

		return myPersons;
	}
*/
	
	public static string[] SelectAllPersonsRecuperable(string sortedBy, int except, int inSession) 
	{
		//sortedBy = name or uniqueID (= creation date)
	

		//1st select all the person.uniqueID of people who are in CurrentSession
		//2n select all names in database (or in one session if inSession != -1)
		//3d filter all names (save all found in 2 that is not in 1)
		//
		//probably this can be made in only one time... future
		//
		//1
		
		dbcon.Open();
		dbcmd.CommandText = "SELECT person.uniqueID " +
			" FROM person, personSession " +
			" WHERE personSession.sessionID == " + except + 
			" AND person.uniqueID == personSession.personID "; 
		
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
			dbcmd.CommandText = "SELECT * FROM person ORDER BY " + sortedBy;
		} else {
			dbcmd.CommandText = "SELECT person.* FROM person, personSession " +
				" WHERE personSession.sessionID == " + inSession + 
				" AND person.uniqueID == personSession.personID " + 
				"ORDER BY " + sortedBy;
		}
		
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
			
			if (found) {
				//Console.WriteLine("FOUND: {0}", reader2[0].ToString());
			} else  {
				//Console.WriteLine("{0} {1} {2} {3}", reader2[0].ToString(), reader2[1].ToString(), 
				//		reader2[2].ToString(), reader2[3].ToString()
				//		);
				myArray2.Add (reader2[0].ToString() + ":" + reader2[1].ToString() + ":" +
						reader2[2].ToString() + ":" + reader2[3].ToString() + ":" +
						reader2[4].ToString() + ":" + reader2[5].ToString() + ":" +
						reader2[6].ToString()
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

	public static void Update(Person myPerson)
	{
		dbcon.Open();
		dbcmd.CommandText = "UPDATE person " + 
			" SET name = '" + myPerson.Name + 
			"', sex = '" + myPerson.Sex +
			"', dateborn = '" + myPerson.DateBorn +
			"', height = " + myPerson.Height +
			", weight = " + myPerson.Weight +
			", description = '" + myPerson.Description +
			"' WHERE uniqueID == '" + myPerson.UniqueID + "'" ;
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		dbcon.Close();
	}

	
	public static void Delete()
	{
	}
}
