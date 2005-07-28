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


class SqlitePersonSession : Sqlite
{
	protected static void createTable()
	 {
		dbcmd.CommandText = 
			"CREATE TABLE personSession ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"personID INT, " +
			"sessionID INT)";		
		dbcmd.ExecuteNonQuery();
	 }

	public static int Insert(int personID, int sessionID)
	{
		dbcon.Open();
		dbcmd.CommandText = "INSERT INTO personSession(personID, sessionID) VALUES ("
			+ personID + ", " + sessionID + ")" ;
		dbcmd.ExecuteNonQuery();
		int myReturn = dbcon.LastInsertRowId;
		dbcon.Close();
		return myReturn;
	}

	public static bool SessionExists(string sessionName)
	{
		dbcon.Open();
		dbcmd.CommandText = "SELECT uniqueID FROM session " +
			" WHERE LOWER(session.name) == LOWER('" + sessionName + "')" ;
		Console.WriteLine(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
	
		bool exists = new bool();
		exists = false;
		
		if (reader.Read()) {
			exists = true;
			//Console.WriteLine("valor {0}", reader[0].ToString());
		}
		Console.WriteLine("exists = {0}", exists.ToString());

		return exists;
	}
	
	public static bool PersonExists(string personName)
	{
		dbcon.Open();
		dbcmd.CommandText = "SELECT uniqueID FROM person " +
			" WHERE LOWER(person.name) == LOWER('" + personName + "')" ;
		Console.WriteLine(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
	
		bool exists = new bool();
		exists = false;
		
		if (reader.Read()) {
			exists = true;
			//Console.WriteLine("valor {0}", reader[0].ToString());
		}
		//Console.WriteLine("exists = {0}", exists.ToString());

		return exists;
	}
	
	public static bool PersonExistsAndItsNotMe(int uniqueID, string personName)
	{
		dbcon.Open();
		dbcmd.CommandText = "SELECT uniqueID FROM person " +
			" WHERE LOWER(person.name) == LOWER('" + personName + "')" +
			" AND uniqueID != " + uniqueID ;
		Console.WriteLine(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
	
		bool exists = new bool();
		exists = false;
		
		if (reader.Read()) {
			exists = true;
			//Console.WriteLine("valor {0}", reader[0].ToString());
		}
		//Console.WriteLine("exists = {0}", exists.ToString());

		return exists;
	}
	
	public static bool PersonSelectExistsInSession(string myPersonID, int mySessionID)
	{
		dbcon.Open();
		dbcmd.CommandText = "SELECT * FROM personSession " +
			" WHERE personID == " + myPersonID + 
			" AND sessionID == " + mySessionID ; 
		Console.WriteLine(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
	
		bool exists = new bool();
		exists = false;
		
		while(reader.Read()) {
			exists = true;
			//Console.WriteLine("valor {0}", reader[0].ToString());
		}
		//Console.WriteLine("exists = {0}", exists.ToString());

		return exists;
	}
	
	public static Person PersonSelect(string myUniqueID)
	{
		dbcon.Open();
		dbcmd.CommandText = "SELECT name, sex, dateborn, height, weight, description " +
			"FROM person WHERE uniqueID == " + myUniqueID ; 
		Console.WriteLine(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
	
		string [] values = new string[6];
		
		while(reader.Read()) {
			values[0] = reader[0].ToString(); 
			values[1] = reader[1].ToString(); 
			values[2] = reader[2].ToString();
			values[3] = reader[3].ToString();
			values[4] = reader[4].ToString();
			values[5] = reader[5].ToString();
		}

		Person myPerson = new Person(Convert.ToInt32(myUniqueID), values[0], 
			values[1], values[2], Convert.ToInt32(values[3]), Convert.ToInt32(values[4]), values[5]);
		
		return myPerson;
	}
	
	public static string[] SelectCurrentSession(int sessionID) 
	{
		dbcon.Open();
		dbcmd.CommandText = "SELECT person.* " +
			"FROM person, personSession " +
			" WHERE personSession.sessionID == " + sessionID + 
			" AND person.uniqueID == personSession.personID " + 
			" ORDER BY personSession.uniqueID";
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);

		int count = new int();
		count = 0;

		while(reader.Read()) {
			/*
			Console.WriteLine("[{0}] {1} {2} {3}",
					reader[0].ToString(), reader[1].ToString(), 
					reader[2].ToString(), reader[3].ToString() );
			*/
			myArray.Add (reader[0].ToString() + ": " + reader[1].ToString() );
			count ++;
		}

		reader.Close();
		dbcon.Close();

		string [] myJumpers = new string[count];
		
		//show the results in the combo_sujeto_actual in reversed order, 
		//then when we create a new person, this is the active, and this is shown 
		//correctly in the combo_sujeto_actual
		int count2 = count -1;
		foreach (string line in myArray) {
			myJumpers [count2--] = line;
		}
		return myJumpers;
	}
	
	public static void DeletePersonFromSessionAndJumps(string sessionID, string personID)
	{
		dbcon.Open();

		//delete relations (existance) within persons and sessions in this session
		dbcmd.CommandText = "Delete FROM personSession WHERE sessionID == " + sessionID +
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
		
		//runs PENDING
		
		dbcon.Close();
	}

}

