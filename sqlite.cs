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


class Sqlite
{
	static SqliteConnection dbcon;
	static SqliteCommand dbcmd;
	static string sqlFile = "chronojump.db";
	static string connectionString = "URI=file:" + sqlFile ;

	public static void Connect()
	{
		dbcon = new SqliteConnection();
		dbcon.ConnectionString = connectionString;
		dbcmd = new SqliteCommand();
		dbcmd.Connection = dbcon;
	}

	public static void CreateFile()
	{
		Console.WriteLine("creating file...");
		dbcon.Open();
		dbcon.Close();
	}

	public static bool CheckTables()
	{
		return (File.Exists(sqlFile));
	}
	
	public static void CreateTables()
	{
		dbcon.Open();

		personCreateTable();
		jumpCreateTable();
		jumpRjCreateTable();
		sessionCreateTable();
		personSessionCreateTable();
		preferencesCreateTable();

		preferencesInsert ("digitsNumber", "7");
		preferencesInsert ("showHeight", "True");
		preferencesInsert ("simulated", "True");

		dbcon.Close();
	}

	
// ------------------------------------------------------
// ------------------------------ PERSON ----------------
// ------------------------------------------------------

	
	public static void personCreateTable()
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

	public static int PersonInsert(string name, string sex, string dateBorn, int height, int weight, string description)
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
	
	public static string[] SelectAllPersonsRecuperable(string sortedBy, int except) 
	{
		//sortedBy = name or uniqueID (= creation date)
	

		//1st select all the person.uniqueID of peopleo who are in CurrentSession
		//2n select all names
		//3d filter all names (stripping off the first selected)
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
			Console.WriteLine("{0}", reader[0].ToString());
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
		dbcmd.CommandText = "SELECT * FROM person ORDER BY " + sortedBy;
		
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
				Console.WriteLine("FOUND: {0}", reader2[0].ToString());
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

	public static void PersonUpdate(Person myPerson)
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

	
	public static void PersonDelete()
	{
	}

// ------------------------------------------------------
// ------------------------------ JUMP ------------------
// ------------------------------------------------------
	
	public static void jumpCreateTable()
	{
		dbcmd.CommandText = 
			"CREATE TABLE jump ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"personID INT, " +
			"sessionID INT, " +
			"type TEXT, " +
			"tv FLOAT, " +
			"tc FLOAT, " +
			"fall INT, " +  
			"weight TEXT, " + //string because can contain "33%" or "50Kg"
			"description TEXT )";		
		dbcmd.ExecuteNonQuery();
	}
	
	public static void jumpRjCreateTable()
	{
		dbcmd.CommandText = 
			"CREATE TABLE jumpRj ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"personID INT, " +
			"sessionID INT, " +
			"type TEXT, " + //in a future probably there are some types of rj
			"tvMax FLOAT, " +
			"tcMax FLOAT, " +
			"fall INT, " +  
			"weight TEXT, " + //string because can contain "33%" or "50Kg"
			"description TEXT, " +		//this and the above values are equal than normal jump
			"tvAvg FLOAT, " +		//this and next values are Rj specific
			"tcAvg FLOAT, " +
			"tvString TEXT, " +
			"tcString TEXT, " +
			"jumps INT, " +
			"time FLOAT, " + //if limit it's 'n' jumps, we probably waste 7.371 seconds
			"limited TEXT) "; //for RJ, "11J" or "11S" (11 Jumps, 11 seconds)
		dbcmd.ExecuteNonQuery();
	}
	
	
	public static Jump JumpInsert(int personID, int sessionID, string type, double tv, double tc, int fall, string weight, string limited, string description)
	{
		dbcon.Open();
		dbcmd.CommandText = "INSERT INTO jump" + 
				"(uniqueID, personID, sessionID, type, tv, tc, fall, weight, description)" +
				" VALUES (NULL, "
				+ personID + ", " + sessionID + ", '" + type + "', "
				+ tv + ", " + tc + ", " + fall + ", '" 
				+ weight + "', '" + description + "')" ;
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		int myLast = dbcon.LastInsertRowId;
		dbcon.Close();

		Jump myJump = new Jump(myLast, personID, sessionID,
				type, tv, tc, fall, weight, description );
		
		return myJump;
	}
	
	//fall has values like "10J" or "10T" (10 jumps, or 10 seconds, respectively)
	public static Jump JumpInsertRj(int personID, int sessionID, string type, double tvMax, double tcMax, int fall, string weight, string description, double tvAvg, double tcAvg, string tvString, string tcString, int jumps, double time, string limited )
	{
		dbcon.Open();
		dbcmd.CommandText = "INSERT INTO jumpRj " + 
				"(uniqueID, personID, sessionID, type, tvMax, tcMax, fall, weight, description, " +
				"tvAvg, tcAvg, tvString, tcString, jumps, time, limited	)" +
				"VALUES (NULL, " +
				personID + ", " + sessionID + ", '" + type + "', " +
				tvMax + ", " + tcMax + ", '" + fall + "', '" + weight + "', '" + description + "', " +
				tvAvg + ", " + tcAvg + ", '" + tvString + "', '" + tcString + "', " +
				jumps + ", " + time + ", '" + limited + "')" ;
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		int myLast = dbcon.LastInsertRowId;

		//Jump myJump = new JumpRj (myLast, personID, sessionID, type, tvMax, tcMax,
		//		fall, weight, description, tvAvg, tcAvg, tvString, tcString, 
		//		jumps, time, limited );
		Jump myJump = new Jump (myLast, personID, sessionID, type, tvString, tcString,
				fall, weight, description, jumps, time, limited );

		dbcon.Close();

		return myJump;
	}

	/*
	private static double getAverage (string values)
	{
		string [] myStringFull = values.Split(new char[] {'='});
		double myAverage = 0;
		double myCount = 0;
		foreach (string jump in myStringFull) {
			myAverage = myAverage + Convert.ToDouble(jump);
			myCount ++;
		}
		return myAverage / myCount ; 
	}
	*/

	
	public static string[] SelectAllNormalJumps(int sessionID, string ordered_by) 
	{
		string secondOrder;
		if(ordered_by == "ordered_by_time") {
			secondOrder = "jump.uniqueID";
		}
		else { //by type
			secondOrder = "jump.type, " + "jump.uniqueID";
		}
		
		dbcon.Open();
		dbcmd.CommandText = "SELECT person.name, jump.* " +
			" FROM person, jump " +
			" WHERE person.uniqueID == jump.personID" + 
			" AND jump.sessionID == " + sessionID + 
			" ORDER BY person.uniqueID, " +
			secondOrder;
		
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);

		int count = new int();
		count = 0;

		
		while(reader.Read()) {

			myArray.Add (reader[0].ToString() + ":" +	//person.name
					reader[1].ToString() + ":" +	//jump.uniqueID
					reader[2].ToString() + ":" + 	//jump.personID
					reader[3].ToString() + ":" + 	//jump.sessionID
					reader[4].ToString() + ":" + 	//jump.type
					reader[5].ToString() + ":" + 	//jump.tv
					reader[6].ToString() + ":" + 	//jump.tc
					reader[7].ToString() + ":" + 	//fall
					reader[8].ToString() + ":" + 	//weight
					reader[9].ToString() 		//description
					);
			count ++;
		}

		reader.Close();
		dbcon.Close();

		string [] myJumps = new string[count];
		count =0;
		foreach (string line in myArray) {
			myJumps [count++] = line;
		}

		return myJumps;
	}

	public static string[] SelectAllRjJumps(int sessionID) 
	{
		
		dbcon.Open();
		dbcmd.CommandText = "SELECT person.name, jumpRj.* " +
			" FROM person, jumpRj " +
			" WHERE person.uniqueID == jumpRj.personID" + 
			" AND jumpRj.sessionID == " + sessionID + 
			" ORDER BY person.uniqueID, jumpRj.uniqueID";
		
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);

		int count = new int();
		count = 0;

		while(reader.Read()) {
			myArray.Add (reader[0].ToString() + ":" +	//person.name
					reader[1].ToString() + ":" +	//jump.uniqueID
					reader[2].ToString() + ":" + 	//jump.personID
					reader[3].ToString() + ":" + 	//jump.sessionID
					reader[4].ToString() + ":" + 	//jump.type
					reader[5].ToString() + ":" + 	//tvMax
					reader[6].ToString() + ":" + 	//tcMax
					reader[7].ToString() + ":" + 	//fall
					reader[8].ToString() + ":" + 	//weight
					reader[9].ToString() + ":" + 	//description
					reader[10].ToString() + ":" + 	//tvAvg,
					reader[11].ToString() + ":" + 	//tcAvg,
					reader[12].ToString() + ":" + 	//tvString,
					reader[13].ToString() + ":" + 	//tcString,
					reader[14].ToString() + ":" + 	//jumps,
					reader[15].ToString() + ":" + 	//time,
					reader[16].ToString() 	 	//limited
					);
			count ++;
		}

		reader.Close();
		dbcon.Close();

		string [] myJumps = new string[count];
		count =0;
		foreach (string line in myArray) {
			myJumps [count++] = line;
		}

		return myJumps;
	}

	public static Jump SelectNormalJumpData(int uniqueID)
	{
		dbcon.Open();

		dbcmd.CommandText = "SELECT * FROM jump WHERE uniqueID == " + uniqueID;
		
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		reader.Read();

		Jump myJump = new Jump(
				Convert.ToInt32(reader[0]),
				Convert.ToInt32(reader[1]),
				Convert.ToInt32(reader[2]),
				reader[3].ToString(),
				Convert.ToDouble( reader[4].ToString() ),
				Convert.ToDouble( reader[5].ToString() ),
				Convert.ToInt32(reader[6]),  //fall
				reader[7].ToString(), //weight
				reader[8].ToString() //description
				);
	
		return myJump;
	}
		
	public static Jump SelectRjJumpData(int uniqueID)
	{
		dbcon.Open();

		dbcmd.CommandText = "SELECT * FROM jumpRj WHERE uniqueID == " + uniqueID;
		
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		reader.Read();

		Jump myJump = new Jump(
				Convert.ToInt32(reader[0]),	//uniqueID
				Convert.ToInt32(reader[1]),	//personID
				Convert.ToInt32(reader[2]),	//sessionID
				reader[3].ToString(),		//type
				reader[11].ToString(),		//tvString
				reader[12].ToString(),		//tcString
				//tvMax and tcMax not needed by the constructor:
				//Convert.ToDouble( reader[4].ToString() ), //tvMax
				//Convert.ToDouble( reader[5].ToString() ), //tcMax
				Convert.ToInt32(reader[6]),  	//fall
				reader[7].ToString(), 		//weight
				reader[8].ToString(), 		//description
				//tvAvg and tcAvg not needed by the constructor:
				//Convert.ToDouble( reader[9].ToString() ), //tvAvg
				//Convert.ToDouble( reader[10].ToString() ), //tcAvg
				Convert.ToInt32(reader[13]),		//jumps
				Convert.ToDouble(reader[14]),		//time
				reader[15].ToString()		//limited
				);

		return myJump;
	}
		
	public static void JumpUpdate(int jumpID, int personID, string description)
	{
		dbcon.Open();
		dbcmd.CommandText = "UPDATE jump " + 
			" SET personID = " + personID + 
			", description = '" + description +
			"' WHERE uniqueID == " + jumpID ;
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		dbcon.Close();
	}

	public static void JumpRjUpdate(int jumpID, int personID, string description)
	{
		dbcon.Open();
		dbcmd.CommandText = "UPDATE jumpRj " + 
			" SET personID = " + personID + 
			", description = '" + description +
			"' WHERE uniqueID == " + jumpID ;
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		dbcon.Close();
	}

	public static void JumpDelete(string uniqueID)
	{
		dbcon.Open();
		dbcmd.CommandText = "Delete FROM jump WHERE uniqueID == " + uniqueID;
		dbcmd.ExecuteNonQuery();
		dbcon.Close();
	}

	public static void JumpRjDelete(string uniqueID)
	{
		dbcon.Open();
		dbcmd.CommandText = "Delete FROM jumpRj WHERE uniqueID == " + uniqueID;
		dbcmd.ExecuteNonQuery();
		dbcon.Close();
	}

// ------------------------------------------------------
// ------------------------------ SESSION ---------------
// ------------------------------------------------------
		
	public static void sessionCreateTable()
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
	
	public static int SessionInsert(string name, string place, string date, string comments)
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
	
	public static Session SessionSelect(string myUniqueID)
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
	public static string[] SelectAllSessionsSimple() 
	{
		dbcon.Open();
		dbcmd.CommandText = "SELECT * FROM session ORDER BY uniqueID";
		
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
		reader_jumps.Close();
		
	
		//close database connection
		dbcon.Close();

		//mix four arrayLists
		string [] mySessions = new string[count];
		count =0;
		bool found;
		foreach (string line in myArray) {
			string lineNotReadOnly = line;
			
			//add persons for each session	
			found = false;
			foreach (string line_persons in myArray_persons) {
				string [] myStringFull = line_persons.Split(new char[] {':'});
				if(myStringFull[0] == (count+1).ToString()) {
					lineNotReadOnly  = lineNotReadOnly + ":" + myStringFull[1];
					found = true;
				}
			}
			if (!found) { lineNotReadOnly  = lineNotReadOnly + ":0"; }
		
			//add jumps for each session
			found = false;
			foreach (string line_jumps in myArray_jumps) {
				string [] myStringFull = line_jumps.Split(new char[] {':'});
				if(myStringFull[0] == (count+1).ToString()) {
					lineNotReadOnly  = lineNotReadOnly + ":" + myStringFull[1];
					found = true;
				}
			}
			if (!found) { lineNotReadOnly  = lineNotReadOnly + ":0"; }
			
			//add jumpsRj for each session
			found = false;
			foreach (string line_jumpsRj in myArray_jumpsRj) {
				string [] myStringFull = line_jumpsRj.Split(new char[] {':'});
				if(myStringFull[0] == (count+1).ToString()) {
					lineNotReadOnly  = lineNotReadOnly + ":" + myStringFull[1];
					found = true;
				}
			}
			if (!found) { lineNotReadOnly  = lineNotReadOnly + ":0"; }
			
			Console.WriteLine("LineNotReadOnly {0}: {1}", count, lineNotReadOnly);
			mySessions [count++] = lineNotReadOnly;
		}

		return mySessions;
	}

	public static void SessionDelete()
	{
	}

// ------------------------------------------------------
// -------------------------PERSON SESSION -------------
// ------------------------------------------------------
		
	
	public static void personSessionCreateTable()
	 {
		dbcmd.CommandText = 
			"CREATE TABLE personSession ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"personID INT, " +
			"sessionID INT)";		
		dbcmd.ExecuteNonQuery();
	 }

	public static int PersonSessionInsert(int personID, int sessionID)
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
	
	public static string[] PersonSessionSelectCurrentSession(int sessionID) 
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





		
// ------------------------------------------------------------
// ------------------------------ STATS -----------------------
// ------------------------------------------------------------

	
	private static string getSexSqlString (string sex)
	{
		string sexSqlString = "";
		if (sex == "M") {
			sexSqlString = " AND person.sex == 'M' " ;
		} else if (sex == "F") {
			sexSqlString = " AND person.sex == 'F' " ;
		}

		return sexSqlString;
	}
	
	//for SJ, SJ+, CMJ, ABK
	//not for DJ, there's an specific method below: StatOneJumpJumpersDj 
	//not for RJ, there's an specific method below: StatOneJumpJumpersRj 
	public static ArrayList StatOneJumpJumpers (int sessionID, string jumpType, bool sexSeparated, string operation)
	{
		string orderByString = "";
		if (sexSeparated) {
			orderByString = "person.sex DESC, ";
		}
		
		string moreSelect = "";
		if (jumpType == "SJ+") {
			moreSelect = ", jump.weight " ;
		}
		
		dbcon.Open();
		//0 always tv
		//1 always person
		//2 always sex
		dbcmd.CommandText = "SELECT " + operation + "(jump.tv) AS jump_tv, person.name, person.sex " + moreSelect +
			" FROM jump, person " +
			" WHERE jump.type == '" + jumpType + "' " +
			" AND jump.sessionID == " + sessionID + 
			" AND jump.personID == person.uniqueID " +
			" GROUP BY person.uniqueID " +
			" ORDER BY " + orderByString + " jump.tv DESC " ; 

		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);
		while(reader.Read()) {
			if (jumpType == "SJ+") {
				myArray.Add (reader[0].ToString() + ":" + reader[1].ToString() + ":" +
						reader[2].ToString() + ":" + reader[3].ToString() );
			} else {
				myArray.Add (reader[0].ToString() + ":" + reader[1].ToString() + ":" +
						reader[2].ToString() );
			}
		}
		
		reader.Close();
		dbcon.Close();
		
		return myArray;
	}

	//Only for DJ, for solving a problem that tv and tc doesn't match in some options
	public static ArrayList StatOneJumpJumpersDj (int sessionID, bool index, bool sexSeparated, string operation)
	{
		string orderByString = "";
		if (sexSeparated) {
			orderByString = "person.sex DESC, ";
		}
		
		string selectString = "";
		
		dbcon.Open();
		//0 always tv
		//1 always person
		//2 always sex
		if (operation == "AVG") {
			if (index) {
				selectString = " AVG(jump.tv) AS jump_tv, person.name, person.sex, AVG(jump.tc) AS jump_tc, AVG(jump.fall), AVG(100*((jump.tv-jump.tc)/jump.tc)) AS jump_index ";
				orderByString = orderByString + "jump_index DESC ";
			} else {
				selectString = " AVG(jump.tv) AS jump_tv, person.name, person.sex, AVG(jump.tc) AS jump_tc, AVG(jump.fall) ";
				orderByString = orderByString + "jump_tv DESC ";
			}
			dbcmd.CommandText = "SELECT " + selectString + 
				" FROM jump, person " +
				" WHERE jump.type == 'DJ' " +
				" AND jump.sessionID == " + sessionID + 
				" AND jump.personID == person.uniqueID " +
				" GROUP BY person.uniqueID " +
				" ORDER BY " + orderByString ; 
		} else {
			//operation == MAX 
			//not "GROUP BY" because it doens't match the tv with the tc
			if (index) {
				selectString = " jump.tv, person.name, person.sex, jump.tc, jump.fall, 100*(jump.tv-jump.tc)/jump.tc AS jump_index ";
				orderByString = orderByString + "jump_index DESC ";
			} else {
				selectString = " jump.tv, person.name, person.sex, jump.tc, jump.fall  ";
				orderByString = orderByString + "jump.tv DESC ";
			}
			dbcmd.CommandText = "SELECT " + selectString +
				" FROM jump, person " +
				" WHERE jump.type == 'DJ' " +
				" AND jump.sessionID == " + sessionID + 
				" AND jump.personID == person.uniqueID " +
				" ORDER BY " + orderByString ; 
		}

		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);
		ArrayList arrayJumpers = new ArrayList(2);
		
		while(reader.Read()) { 	
			//filter all except the first value of each person
			if ( ! (operation == "MAX" && foundInArray(reader[1].ToString(), arrayJumpers) ) ) 
			{
				if(index) {
					myArray.Add (reader[0].ToString() + ":" + reader[1].ToString() + ":" +
							reader[2].ToString() + ":" + reader[3].ToString() + ":" +
							reader[4].ToString() + ":" + reader[5].ToString() );
				} else {
					myArray.Add (reader[0].ToString() + ":" + reader[1].ToString() + ":" +
							reader[2].ToString() + ":" + reader[3].ToString() + ":" +
							reader[4].ToString() );
				}
			arrayJumpers.Add(reader[1].ToString());
			}
		}
		
		reader.Close();
		dbcon.Close();
		
		return myArray;
	}

	//Only for RJ, for solving a problem that tv and tc doesn't match in some options
	public static ArrayList StatOneJumpJumpersRj (int sessionID, bool sexSeparated, string operation)
	{
		string orderByString = "";
		if (sexSeparated) {
			orderByString = "person.sex DESC, ";
		}
		
		string selectString = "";
		
		dbcon.Open();
		//0 always tv
		//1 always person
		//2 always sex
		if (operation == "AVG") {
			selectString = " AVG(tvavg) AS jump_tv, person.name, person.sex, AVG(tcavg) AS jump_tc, AVG(fall), AVG(100*((tvavg-tcavg)/tcavg)) AS jump_index ";
			orderByString = orderByString + "jump_index DESC ";
			dbcmd.CommandText = "SELECT " + selectString + 
				" FROM jumpRj, person " +
				" WHERE type == 'RJ' " +
				" AND jumpRj.sessionID == " + sessionID + 
				" AND jumpRj.personID == person.uniqueID " +
				" GROUP BY person.uniqueID " +
				" ORDER BY " + orderByString ; 
		} else {
			selectString = " tvavg, person.name, person.sex, tcavg, fall, 100*(tvavg-tcavg)/tcavg AS jump_index ";
			orderByString = orderByString + "jump_index DESC ";
			dbcmd.CommandText = "SELECT " + selectString +
				" FROM jumpRj, person " +
				" WHERE type == 'RJ' " +
				" AND jumpRj.sessionID == " + sessionID + 
				" AND jumpRj.personID == person.uniqueID " +
				" ORDER BY " + orderByString ; 
		}

		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);
		ArrayList arrayJumpers = new ArrayList(2);
		
		while(reader.Read()) { 	
			//filter all except the first value of each person
			if ( ! (operation == "MAX" && foundInArray(reader[1].ToString(), arrayJumpers) ) ) 
			{
				myArray.Add (reader[0].ToString() + ":" + reader[1].ToString() + ":" +
						reader[2].ToString() + ":" + reader[3].ToString() + ":" +
						reader[4].ToString() + ":" + reader[5].ToString() );
				arrayJumpers.Add(reader[1].ToString());
			}
		}
		
		reader.Close();
		dbcon.Close();
		
		return myArray;
	}

	public static ArrayList StatOneJumpJumpersRjPotencyAguado (int sessionID, bool sexSeparated, string operation)
	{
		string orderByString = "";
		if (sexSeparated) {
			orderByString = "person.sex DESC, ";
		}
		
		string selectString = "";
		
		dbcon.Open();
		
		if (operation == "AVG") {
			selectString = " person.name, person.sex, AVG(jumpRj.jumps), AVG(jumpRj.time), AVG(jumpRj.tvAvg), " +
				"AVG(9.81*9.81 * tvavg*jumps * time / ( 4 * jumps * (time - tvavg*jumps) ) ) AS potency ";
			orderByString = orderByString + "potency DESC ";
			dbcmd.CommandText = "SELECT " + selectString + 
				" FROM jumpRj, person " +
				" WHERE type == 'RJ' " +
				" AND jumpRj.sessionID == " + sessionID + 
				" AND jumpRj.personID == person.uniqueID " +
				" GROUP BY person.uniqueID " +
				" ORDER BY " + orderByString ; 
		} else {
			selectString = " person.name, person.sex, jumpRj.jumps, jumpRj.time, jumpRj.tvAvg, " +
				"(9.81*9.81 * tvavg*jumps * time / ( 4 * jumps * (time - tvavg*jumps) ) ) AS potency ";
			orderByString = orderByString + "potency DESC ";
			dbcmd.CommandText = "SELECT " + selectString +
				" FROM jumpRj, person " +
				" WHERE type == 'RJ' " +
				" AND jumpRj.sessionID == " + sessionID + 
				" AND jumpRj.personID == person.uniqueID " +
				" ORDER BY " + orderByString ; 
		}

		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);
		ArrayList arrayJumpers = new ArrayList(2);
		
		while(reader.Read()) { 	
			//filter all except the first value of each person
			if ( ! (operation == "MAX" && foundInArray(reader[0].ToString(), arrayJumpers) ) ) 
			{
				myArray.Add (reader[0].ToString() + ":" + reader[1].ToString() + ":" +
						reader[2].ToString() + ":" + reader[3].ToString() + ":" +
						reader[4].ToString() + ":" + reader[5].ToString() );
				arrayJumpers.Add(reader[0].ToString());
			}
		}
		
		reader.Close();
		dbcon.Close();
		
		return myArray;
	}

	static bool foundInArray (string searching, ArrayList myArray) 
	{
		for (int i=0; i< myArray.Count ; i ++) {
			if (searching == myArray[i].ToString()) {
				return true;
			}
		}
		return false;
	}
	
	//for SJ, SJ+, CMJ, ABK, DJ
	//RJ ?? how to read the max from the SQL?
	//probably the solution is use a different method for RJ
	public static ArrayList StatOneJumpJumps (int sessionID, string jumpType, bool index, bool sexSeparated, int limit)
	{
		string orderByString = "";
		if (sexSeparated) {
			orderByString = "person.sex DESC, ";
		}
		
		if (limit > 0) {
			//if we use jumps and limit, don't separate by sex, because it's difficult to show 
			//the same proportion of man and woman without doing all this two times
			orderByString = "";
		}
			
		string moreSelect = "";
		if (jumpType == "DJ") {
			if (index) {
				moreSelect = ", jump.tc, jump.fall, (100*(jump.tv-jump.tc)/jump.tc) AS jump_index " ;
				orderByString = orderByString + "jump_index DESC, ";
			} else {
				moreSelect = ", jump.tc , jump.fall " ;
			}
		}
		else if (jumpType == "SJ+") {
			moreSelect = ", jump.weight " ;
		}

		
		dbcon.Open();
		//0 always tv
		//1 always person
		//2 always sex
		dbcmd.CommandText = "SELECT jump.tv, person.name, person.sex " + moreSelect +
			" FROM jump, person " +
			" WHERE jump.type == '" + jumpType + "' " +
			" AND jump.sessionID == " + sessionID + 
			" AND jump.personID == person.uniqueID " +
			" ORDER BY " + orderByString + " jump.tv DESC " +
			" LIMIT " + limit ; 

		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);
		while(reader.Read()) {
			if (jumpType == "DJ") {
				if(index) {
					myArray.Add (reader[0].ToString() + ":" + reader[1].ToString() + ":" +
							reader[2].ToString() + ":" + reader[3].ToString() + ":" +
							reader[4].ToString() + ":" + reader[5].ToString() );
				} else {
					myArray.Add (reader[0].ToString() + ":" + reader[1].ToString() + ":" +
						reader[2].ToString() + ":" + reader[3].ToString() + ":" +
						reader[4].ToString() );
				}
			}
			else if (jumpType == "SJ+") {
				myArray.Add (reader[0].ToString() + ":" + reader[1].ToString() + ":" +
						reader[2].ToString() + ":" + reader[3].ToString() );
			} else {
				myArray.Add (reader[0].ToString() + ":" + reader[1].ToString() + ":" +
						reader[2].ToString() );
			}
		}
		
		reader.Close();
		dbcon.Close();
		
		return myArray;
	}

	public static ArrayList StatRjJumps (int sessionID, bool sexSeparated, int limit)
	{
		string orderByString = "";
		if (sexSeparated) {
			orderByString = "person.sex DESC, ";
		}
		
		if (limit > 0) {
			//if we use jumps and limit, don't separate by sex, because it's difficult to show 
			//the same proportion of man and woman without doing all this two times
			orderByString = "";
		}
			
		string moreSelect = "";
		moreSelect = ", tcavg, fall, (100*(tvavg-tcavg)/tcavg) AS jumpRj_index " ;
		orderByString = orderByString + "jumpRj_index DESC ";

		
		dbcon.Open();
		//0 always tv
		//1 always person
		//2 always sex
		dbcmd.CommandText = "SELECT tvavg, person.name, person.sex " + moreSelect +
			" FROM jumpRj, person " +
			" WHERE type == 'RJ' " +
			" AND jumpRj.sessionID == " + sessionID + 
			" AND jumpRj.personID == person.uniqueID " +
			" ORDER BY " + orderByString +
			" LIMIT " + limit ; 

		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);
		while(reader.Read()) {
			myArray.Add (reader[0].ToString() + ":" + reader[1].ToString() + ":" +
					reader[2].ToString() + ":" + reader[3].ToString() + ":" +
					reader[4].ToString() + ":" + reader[5].ToString() );
		}
		
		reader.Close();
		dbcon.Close();
		
		return myArray;
	}

	public static ArrayList StatRjPotencyAguadoJumps (int sessionID, bool sexSeparated, int limit)
	{
		string orderByString = "";
		if (sexSeparated) {
			orderByString = "person.sex DESC, ";
		}
		
		if (limit > 0) {
			//if we use jumps and limit, don't separate by sex, because it's difficult to show 
			//the same proportion of man and woman without doing all this two times
			orderByString = "";
		}
			
		orderByString = orderByString + "potency_not_exact DESC ";

		
		dbcon.Open();
		//0 always tv
		//1 always person
		//2 always sex
		dbcmd.CommandText = "SELECT person.name, person.sex, jumpRj.jumps, jumpRj.time, jumpRj.tvAvg" +
			", (9.81*9.81 * tvavg*jumps * time / ( 4 * jumps * (time - tvavg*jumps) ) ) AS potency_not_exact " +
			" FROM jumpRj, person " +
			" WHERE type == 'RJ' " +
			" AND jumpRj.sessionID == " + sessionID + 
			" AND jumpRj.personID == person.uniqueID " +
			" ORDER BY " + orderByString +
			" LIMIT " + limit ; 

		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);
		while(reader.Read()) {
			myArray.Add (reader[0].ToString() + ":" + reader[1].ToString() + ":" +
					reader[2].ToString() + ":" + reader[3].ToString() + ":" +
					reader[4].ToString() + ":" + reader[5].ToString() );
		}
		
		reader.Close();
		dbcon.Close();
		
		return myArray;
	}

	
	//IE (Elasticity index) relationship between SJ and CMJ
	//IUB (Using of arms index) relationship between CMJ and ABK
	//this method gets all SJ or CMJ or ABK of all persons in a session
	public static ArrayList StatClassificationIeIub (int sessionID, string indexType, string operation, bool sexSeparated)
	{
		//string indexTypeString = "";
		string jump1Name = "";
		string jump2Name = "";
		if (indexType == "IE") {
			//indexTypeString = " AND ( jump.type == 'SJ' OR jump.type == 'CMJ' ) " ;
			jump1Name = "SJ";
			jump2Name = "CMJ";
		} else { //IUB
			//indexTypeString = " AND ( jump.type == 'CMJ' OR jump.type == 'ABK' ) " ;
			jump1Name = "CMJ";
			jump2Name = "ABK";
		}

		string sexSeparatedString = "";
		if (sexSeparated) {
			sexSeparatedString = "person.sex DESC, ";
		}
		
		dbcon.Open();
		
		dbcmd.CommandText = "SELECT person.name, " +
			"((" + operation + "(j2.tv) - " + operation + "(j1.tv))*100/" + operation + "(j1.tv)) AS myindex, " +
			operation + "(j1.tv), " + operation + "(j2.tv), person.sex " +
			" FROM person, jump AS j1, jump AS j2 " +
			" WHERE j1.type == '" + jump1Name + "' " +
			" AND j2.type == '" + jump2Name + "' " +
			" AND person.uniqueID == j1.personID " +
			" AND j1.personID == j2.personID " +
			" AND j1.sessionID == " + sessionID +
			" AND j2.sessionID == " + sessionID +
			" GROUP BY person.uniqueID " +
			" ORDER BY " + sexSeparatedString + " myIndex DESC";
			
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);
		while(reader.Read()) {
			if(reader[2].ToString() == "0") {
				myArray.Add (reader[0].ToString() + ":" + "DIV" + ":" +
						reader[2].ToString() + ":" + reader[3].ToString() + ":" +
						reader[4].ToString());
			} else {
				myArray.Add (reader[0].ToString() + ":" + reader[1].ToString() + ":" +
						reader[2].ToString() + ":" + reader[3].ToString() + ":" +
						reader[4].ToString());
			}
		}
		
		reader.Close();
		dbcon.Close();
		
		return myArray;
	}

	
	public static ArrayList StatGlobalNormal (string sessionString, string operation, bool sexSeparated, int personID)
	{
		dbcon.Open();
		
		string personString = "";
		if(personID != -1) { 
			personString = " AND jump.personID == " + personID;
		}
		
		if (sexSeparated) {
			dbcmd.CommandText = "SELECT type, sessionID, " + operation + "(tv), sex" + 
				" FROM jump, person " +
				sessionString + 
				" AND jump.personID == person.uniqueID" + personString +
				" GROUP BY jump.type, sessionID, person.sex " +
				" ORDER BY jump.type, person.sex DESC, sessionID" ; 
		} else {
		
			dbcmd.CommandText = "SELECT type, sessionID, " + operation + "(tv) " +
				" FROM jump " +
				sessionString + personString +
				" GROUP BY type, sessionID " +
				" ORDER BY type, sessionID " ; 
		}

		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		//returns always two columns
		ArrayList myArray = new ArrayList(2);
		while(reader.Read()) {
			if (reader[0].ToString() != "DJ") {
				if (sexSeparated) {
					myArray.Add (reader[0].ToString() + " (" + reader[3].ToString() + "):" +
							reader[1].ToString() + ":" + reader[2].ToString()
							);
				} else {
					myArray.Add (reader[0].ToString() + ":" + reader[1].ToString() 
							+ ":" + reader[2].ToString() 
							);
				}
			}
		}
		
		reader.Close();
		dbcon.Close();
		
		return myArray;
	}

	public static ArrayList StatGlobalOthers (string statName, string statFormulae, string jumpTable, string jumpType, string sessionString, string operation, bool sexSeparated, int personID)
	{
		dbcon.Open();
		
		string personString = "";
		if(personID != -1) { 
			personString = " AND personID == " + personID;
		}
		
		if (sexSeparated) {
			//select the MAX or AVG index grouped by sex
			//returns 0-2 rows
			dbcmd.CommandText = "SELECT sessionID, " + operation + statFormulae + ", sex " + 
				" FROM " + jumpTable + ", person " +
				sessionString +	
				" AND personID == person.uniqueID" +
				" AND type == '" + jumpType + "'" + personString +
				" GROUP BY sessionID, person.sex " +
				" ORDER BY person.sex DESC, sessionID" ; 
		} else {
			//select the MAX or AVG index. 
			//returns 0-1 rows
			dbcmd.CommandText = "SELECT sessionID, " + operation + statFormulae +
				" FROM " + jumpTable + " " +
				sessionString +	
				" AND type == '" + jumpType + "'" + personString +
				//the following solves a problem
				//of sqlite, that returns an 
				//"empty line" when there are no
				//values to return in a index
				//calculation.
				//With the group by, 
				//if there are no values, 
				//it does not return any line
				" GROUP by sessionID, " +
				" ORDER by sessionID";
		}

		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);
		
		//returns always two columns
		while(reader.Read()) {
			if (sexSeparated) {
				myArray.Add (statName +" (" + reader[2].ToString() + "):" + reader[0].ToString() 
							+ ":" + reader[1].ToString() );
			} else {
				myArray.Add (statName + ":" + reader[0].ToString() 
							+ ":" + reader[1].ToString()	);
			}
		}
		
		reader.Close();
		dbcon.Close();
		
		return myArray;
	}

//--------------------------------------------------------
//---------------- PREFERENCES ---------------------------
//--------------------------------------------------------
	
	private static void preferencesCreateTable()
	{
		dbcmd.CommandText = 
			"CREATE TABLE preferences ( " +
			"name TEXT, " +
			"value TEXT) ";
		dbcmd.ExecuteNonQuery();
	}
	
	private static void preferencesInsert(string myName, string myValue)
	{
		dbcon.Open();
		dbcmd.CommandText = "INSERT INTO preferences (name, value) VALUES ('" + 
			+ myName + "', '" + myValue + "')" ;
		dbcmd.ExecuteNonQuery();
		dbcon.Close();
	}

	public static void PreferencesUpdate(string myName, string myValue)
	{
		dbcon.Open();
		dbcmd.CommandText = "UPDATE preferences " + 
			" SET value = '" + myValue + 
			"' WHERE name == '" + myName + "'" ;
		dbcmd.ExecuteNonQuery();
		dbcon.Close();
	}

	public static string PreferencesSelect (string myName) 
	{
		dbcon.Open();
		dbcmd.CommandText = "SELECT value FROM preferences " + 
			" WHERE name == '" + myName + "'" ;
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		string myReturn = "0";
	
		if(reader.Read()) {
			myReturn = reader[0].ToString();
		}
		reader.Close();
		dbcon.Close();

		return myReturn;
	}

}
