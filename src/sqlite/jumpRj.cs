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
using System.Collections.Generic; //List<T>
using Mono.Data.Sqlite;


class SqliteJumpRj : SqliteJump
{
	public SqliteJumpRj() {
	}
	~SqliteJumpRj() {}
	
	protected override void createTable(string tableName)
	{
		//values: 'jumpRj' and 'tempJumpRj'

		dbcmd.CommandText = 
			"CREATE TABLE " + tableName + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"personID INT, " +
			"sessionID INT, " +
			"type TEXT, " + 
			"tvMax FLOAT, " +
			"tcMax FLOAT, " +
			"fall FLOAT, " +  
			"weight TEXT, " + //string because can contain "33%" or "50Kg"
			"description TEXT, " +		//this and the above values are equal than simple jump
			"tvAvg FLOAT, " +		//this and next values are Rj specific
			"tcAvg FLOAT, " +
			"tvString TEXT, " +
			"tcString TEXT, " +
			"jumps INT, " +
			"time FLOAT, " + //if limit it's 'n' jumps, we probably waste 7.371 seconds
			"limited TEXT, " + //for RJ, "11J" or "11S" (11 Jumps, 11 seconds)
			"angleString TEXT, " + //"-1" if undef
			"simulated INT, " +
			"datetime TEXT )";
		dbcmd.ExecuteNonQuery();
	}

	public static int Insert (bool dbconOpened, string tableName, string uniqueID, int personID, int sessionID, string type, double tvMax, double tcMax, double fall, double weight, string description, double tvAvg, double tcAvg, string tvString, string tcString, int jumps, double time, string limited, string angleString, int simulated, string datetime )
	{
		Console.WriteLine("At SQL insert RJ");

		if(! dbconOpened)
			Sqlite.Open();

		if(uniqueID == "-1")
			uniqueID = "NULL";

		dbcmd.CommandText = "INSERT INTO " + tableName + 
				" (uniqueID, personID, sessionID, type, tvMax, tcMax, fall, weight, description, " +
				"tvAvg, tcAvg, tvString, tcString, jumps, time, limited, angleString, simulated, datetime )" +
				"VALUES (" + uniqueID + ", " +
				personID + ", " + sessionID + ", \"" + type + "\", " +
				Util.ConvertToPoint(tvMax) + ", " + Util.ConvertToPoint(tcMax) + ", \"" + 
				Util.ConvertToPoint(fall) + "\", \"" + Util.ConvertToPoint(weight) + "\", \"" + description + "\", " +
				Util.ConvertToPoint(tvAvg) + ", " + Util.ConvertToPoint(tcAvg) + ", \"" + 
				Util.ConvertToPoint(tvString) + "\", \"" + Util.ConvertToPoint(tcString) + "\", " +
				jumps + ", " + Util.ConvertToPoint(time) + ", \"" + limited + "\", \"" + angleString + "\", " + simulated + ", \"" + datetime + "\")" ;
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

	// limit 0 means no limit (limit negative is the last results)
	public static List<JumpRj> SelectJumps (bool dbconOpened, int sessionID, int personID, string filterType, Orders_by order, int limit, bool personNameInComment)
	{
		if(! dbconOpened)
			Sqlite.Open();

		//jumps previous to DB 1.82 have no datetime on jump
		//find session datetime for that jumps
		List<Session> session_l = SqliteSession.SelectAll(true, Sqlite.Orders_by.DEFAULT);

		//for personNameInComment
		List<Person> person_l =
			SqlitePersonSession.SelectCurrentSessionPersonsAsList(true, sessionID);

		string sep = " WHERE ";

		string filterSessionString = "";
		if(sessionID != -1)
		{
			filterSessionString = sep + "jumpRj.sessionID = " + sessionID;
			if(sep == " WHERE ")
				sep = " AND ";
		}

		string filterPersonString = "";
		if(personID != -1)
		{
			filterPersonString = sep + "jumpRj.personID = " + personID;
			if(sep == " WHERE ")
				sep = " AND ";
		}

		string filterTypeString = "";
		if(filterType != "")
		{
			filterTypeString = sep + "jumpRj.type = \"" + filterType + "\"";
			if(sep == " WHERE ")
				sep = " AND ";
		}

		string orderByString = " ORDER BY jumpRj.uniqueID "; //ID_ASC
		if(order == Orders_by.ID_DESC)
			orderByString = " ORDER BY jumpRj.uniqueID DESC ";

		string limitString = "";
		if(limit > 0)
			limitString = " LIMIT " + limit;

		dbcmd.CommandText = "SELECT * FROM jumpRj" +
			filterSessionString +
			filterPersonString +
			filterTypeString +
			orderByString +
			limitString;

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		List<JumpRj> jmpRj_l = DataReaderToJumpRj(reader, session_l, person_l, personNameInComment);

		reader.Close();

		if(!dbconOpened)
			Sqlite.Close();

		//get last values on negative limit
		if (limit < 0 && jmpRj_l.Count + limit >= 0)
			jmpRj_l = jmpRj_l.GetRange (jmpRj_l.Count + limit, -1 * limit);

		return jmpRj_l;
	}

	public static string[] SelectJumpsSA(bool dbconOpened, int sessionID, int personID, string filterWeight, string filterType) 
	{
		if(!dbconOpened)
			Sqlite.Open();

		string tp = Constants.PersonTable;
		string tps = Constants.PersonSessionTable;

		string filterSessionString = "";
		if(sessionID != -1)
			filterSessionString = " AND jumpRj.sessionID == " + sessionID;

		string filterPersonString = "";
		if(personID != -1)
			filterPersonString = " AND " + tp + ".uniqueID == " + personID;

		string filterWeightString = "";
		if(filterWeight == "withWeight")
			filterWeightString = " AND jumpRj.weight != 0 ";

		string filterTypeString = "";
		if(filterType != "")
			filterTypeString = " AND jumpRj.type == \"" + filterType + "\" ";

		dbcmd.CommandText = "SELECT " + tp + ".name, jumpRj.*, " + tps + ".weight " +
			" FROM " + tp + ", jumpRj, " + tps + " " +
			" WHERE " + tp + ".uniqueID == jumpRj.personID" + 
			filterSessionString +
			filterPersonString +
			filterWeightString +
			filterTypeString +
			" AND " + tps + ".personID == " + tp + ".uniqueID " +
			" AND " + tps + ".sessionID == jumpRj.sessionID " +
			" ORDER BY upper(" + tp + ".name), jumpRj.uniqueID";
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);

		int count = new int();
		count = 0;

		while(reader.Read()) {
			myArray.Add (reader[0].ToString() + ":" +	//person.name
					reader[1].ToString() + ":" +	//jumpRj.uniqueID
					reader[2].ToString() + ":" + 	//jumpRj.personID
					reader[3].ToString() + ":" + 	//jumpRj.sessionID
					reader[4].ToString() + ":" + 	//jumpRj.type
					Util.ChangeDecimalSeparator(reader[5].ToString()) + ":" + 	//tvMax
					Util.ChangeDecimalSeparator(reader[6].ToString()) + ":" + 	//tcMax
					Util.ChangeDecimalSeparator(reader[7].ToString()) + ":" + 	//fall
					Util.ChangeDecimalSeparator(reader[8].ToString()) + ":" + 	//weight
					reader[9].ToString() + ":" + 	//description
					Util.ChangeDecimalSeparator(reader[10].ToString()) + ":" + 	//tvAvg,
					Util.ChangeDecimalSeparator(reader[11].ToString()) + ":" + 	//tcAvg,
					Util.ChangeDecimalSeparator(reader[12].ToString()) + ":" + 	//tvString,
					Util.ChangeDecimalSeparator(reader[13].ToString()) + ":" + 	//tcString,
					reader[14].ToString() + ":" + 	//jumps,
					reader[15].ToString() + ":" + 	//time,
					reader[16].ToString() + ":" + 	//limited
					reader[17].ToString() + ":" +	//angleString
					reader[18].ToString() + ":" +	//simulated
					reader[19].ToString() + ":" +	//datetime
					reader[20].ToString() 	 	//person.weight
					);
			count ++;
		}

		reader.Close();
		
		if(!dbconOpened)
			Sqlite.Close();

		string [] myJumps = new string[count];
		count =0;
		foreach (string line in myArray) {
			myJumps [count++] = line;
		}

		return myJumps;
	}

	//table is jumpRj or tempJumpRj
	public static JumpRj SelectJumpData(string table, int uniqueID, bool personNameInComment, bool dbconOpened)
	{
		if(!dbconOpened)
			Sqlite.Open();

		if(personNameInComment)
		{
			string tp = Constants.PersonTable;
			dbcmd.CommandText = "SELECT " + table + ".*, " + tp + ".Name" +
				" FROM " + table + ", " + tp +
				" WHERE " + table + ".personID = " + tp + ".uniqueID" +
				" AND " + table + ".uniqueID = " + uniqueID;
		} else
			dbcmd.CommandText = "SELECT * FROM " + table + " WHERE uniqueID = " + uniqueID;
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		reader.Read();

		JumpRj myJump = new JumpRj(DataReaderToStringArray(reader, 19));
		if(personNameInComment)
			myJump.Description = reader[19].ToString(); //person.Name

		reader.Close();
		if(!dbconOpened)
			Sqlite.Close();

		return myJump;
	}

	private static List<JumpRj> DataReaderToJumpRj (SqliteDataReader reader, List<Session> session_l,
			List<Person> person_l, bool personNameInComment)
	{
	  List<JumpRj> jmp_l = new List<JumpRj>();
	  JumpRj jmp;

	  //LogB.Information("Imprimire JumpRjs:");
	  while(reader.Read()) {
		  jmp = new JumpRj (
				  Convert.ToInt32(reader[0].ToString()),	//jumpRj.uniqueID
				  Convert.ToInt32(reader[1].ToString()), 	//jumpRj.personID
				  Convert.ToInt32(reader[2].ToString()), 	//jumpRj.sessionID
				  reader[3].ToString(), 	//jumpRj.type
				  Convert.ToDouble(Util.ChangeDecimalSeparator(reader[4].ToString())), 	//tvMax
				  Convert.ToDouble(Util.ChangeDecimalSeparator(reader[5].ToString())), 	//tcMax
				  Convert.ToDouble(Util.ChangeDecimalSeparator(reader[6].ToString())), 	//fall
				  Convert.ToDouble(Util.ChangeDecimalSeparator(reader[7].ToString())), 	//weight
				  reader[8].ToString(), 	//description
				  Convert.ToDouble(Util.ChangeDecimalSeparator(reader[9].ToString())), 	//tvAvg
				  Convert.ToDouble(Util.ChangeDecimalSeparator(reader[10].ToString())), 	//tcAvg
				  Util.ChangeDecimalSeparator(reader[11].ToString()), 	//tvString
				  Util.ChangeDecimalSeparator(reader[12].ToString()), 	//tcString
				  Convert.ToInt32(reader[13].ToString()), 	//jumps
				  Convert.ToDouble(Util.ChangeDecimalSeparator(reader[14].ToString())), //time
				  reader[15].ToString(), 	//limited
				  reader[16].ToString(),	//angleString
				  Convert.ToInt32(reader[17].ToString()),	//simulated
				  reader[18].ToString()	//datetime
				  );

		  //jumps previous to DB 1.82 have no datetime on jump
		  //find session datetime for that jumps
		  if(jmp.Datetime == "")
		  {
			  bool found = false;
			  foreach(Session session in session_l)
			  {
				  if(session.UniqueID == jmp.SessionID)
				  {
					  jmp.Datetime = UtilDate.ToFile(session.Date);
					  found = true;
					  break;
				  }

			  }
			  //on really old versions of Chronojump, deleting a session maybe does not delete the jumps
			  //so could be to found a jump without a session, so assign here the MinValue possible of DateTime
			  if(! found)
				  jmp.Datetime = UtilDate.ToFile(DateTime.MinValue);
		  }

		  if(personNameInComment)
			  foreach(Person person in person_l)
				  if(person.UniqueID == jmp.PersonID)
					  jmp.Description = person.Name;


		  jmp_l.Add(jmp);
		  //LogB.Information(jmp.ToString());
	  }
	  return jmp_l;
	}

	public static void Update(int jumpID, int personID, string fall, double weight, string description)
	{
		Sqlite.Open();
		dbcmd.CommandText = "UPDATE jumpRj SET personID = " + personID + 
			", fall = " + Util.ConvertToPoint(Convert.ToDouble(fall)) + 
			", weight = " + Util.ConvertToPoint(weight) + 
			", description = \"" + description +
			"\" WHERE uniqueID == " + jumpID ;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		Sqlite.Close();
	}

	//checks if there are Rjs with different number of TCs than TFs
	//then repair database manually, and look if the jump is jumpLimited, and how many jumps there are defined
	public static void FindBadRjs()
	{
		Sqlite.Open();

		dbcmd.CommandText = "SELECT uniqueID, tcstring, tvstring, jumps, limited FROM jumpRj";
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		reader.Read();

		while(reader.Read()) {
			if(Util.GetNumberOfJumps(reader[1].ToString(), true) != Util.GetNumberOfJumps(reader[2].ToString(), true)) {
				LogB.Error(string.Format("Problem with jumpRj: {0}, tcstring{1}, tvstring{2}, jumps{3}, limited{4}", 
						reader[0].ToString(), 
						Util.GetNumberOfJumps(reader[1].ToString(), true).ToString(), 
						Util.GetNumberOfJumps(reader[2].ToString(), true).ToString(), 
						reader[3].ToString(), reader[4].ToString()));
			}
		}
		reader.Close();
		Sqlite.Close();
	}

}

