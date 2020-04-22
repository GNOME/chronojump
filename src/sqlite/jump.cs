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
 * Copyright (C) 2004-2020   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using System.IO;
using System.Collections; //ArrayList
using System.Collections.Generic; //List<T>
using Mono.Data.Sqlite;


class SqliteJump : Sqlite
{
	public SqliteJump() {
	}
	
	~SqliteJump() {}

	/*
	 * create and initialize tables
	 */
	
	protected override void createTable(string tableName)
	{
		//values: Constants.JumpTable and Constants.TempEventTable'
		dbcmd.CommandText = 
			"CREATE TABLE " + tableName + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"personID INT, " +
			"sessionID INT, " +
			"type TEXT, " +
			"tv FLOAT, " +
			"tc FLOAT, " +
			"fall FLOAT, " +  
			"weight TEXT, " + //string because can contain "33%" or "50Kg"
			"description TEXT, " +
			"angle FLOAT, " + //-1.0 if undef
			"simulated INT, " + 	//since db: 0.60 (cj 0.8.1.2) simulated = -1, real test (not uploaded to server) = 0,
						//positive numbers represent the serverUniqueID
						//the simulated has two purposes, but it's logical because 
						//only real tests can be uploaded
			"datetime TEXT )";
		dbcmd.ExecuteNonQuery();
	}
	
	
	/*
	 * Jump class methods
	 */
	
	//public static int Insert(int personID, int sessionID, string type, double tv, double tc, int fall, double weight, string limited, string description, int simulated)
	public static int Insert(bool dbconOpened, string tableName, string uniqueID, int personID, int sessionID, string type, double tv, double tc, double fall, double weight, string description, double angle, int simulated, string datetime)
	{
		if(! dbconOpened)
			Sqlite.Open();

		if(uniqueID == "-1")
			uniqueID = "NULL";

		dbcmd.CommandText = "INSERT INTO " + tableName +  
				" (uniqueID, personID, sessionID, type, tv, tc, fall, weight, description, angle, simulated, datetime)" +
				" VALUES (" + uniqueID + ", "
				+ personID + ", " + sessionID + ", \"" + type + "\", "
				+ Util.ConvertToPoint(tv) + ", " + Util.ConvertToPoint(tc) + ", " + Util.ConvertToPoint(fall) + ", \"" 
				+ Util.ConvertToPoint(weight) + "\", \"" + description + "\", "
				+ Util.ConvertToPoint(angle) + ", " + simulated + ", \"" + datetime + "\")" ;
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

	//like SelectJumps, but this returns a string[] :( better use above method if possible
	//if all sessions, put -1 in sessionID
	//if all persons, put -1 in personID
	//if all types put, "" in filterType
	//unlimited put -1 in limit
	public static string[] SelectJumps(bool dbconOpened, int sessionID, int personID, string filterWeight, string filterType, 
			Orders_by order, int limit) 
	{
		if(!dbconOpened)
			Sqlite.Open();

		string tp = Constants.PersonTable;
		string tps = Constants.PersonSessionTable;

		string filterSessionString = "";
		if(sessionID != -1)
			filterSessionString = " AND jump.sessionID == " + sessionID;

		string filterPersonString = "";
		if(personID != -1)
			filterPersonString = " AND " + tp + ".uniqueID == " + personID;

		string filterWeightString = "";
		if(filterWeight == "withWeight")
			filterWeightString = " AND jump.weight != 0 ";

		string filterTypeString = "";
		if(filterType != "")
			filterTypeString = " AND jump.type == \"" + filterType + "\" ";

		string orderByString = " ORDER BY upper(" + tp + ".name), jump.uniqueID ";
		if(order == Orders_by.ID_DESC)
			orderByString = " ORDER BY jump.uniqueID DESC ";
		
		string limitString = "";
		if(limit != -1)
			limitString = " LIMIT " + limit;

		dbcmd.CommandText = "SELECT " + tp + ".name, jump.*, " + tps + ".weight " +
			" FROM " + tp + ", jump, " + tps + 
			" WHERE " + tp + ".uniqueID == jump.personID " + 
			filterSessionString +
			filterPersonString +
			filterWeightString +
			filterTypeString +
			" AND " + tps + ".personID == " + tp + ".uniqueID " +
			" AND " + tps + ".sessionID == jump.sessionID " +
			orderByString +
			limitString;
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);

		int count = new int();
		count = 0;
		
		while(reader.Read())
		{
			myArray.Add (reader[0].ToString() + ":" +	//person.name
					reader[1].ToString() + ":" +	//jump.uniqueID
					reader[2].ToString() + ":" + 	//jump.personID
					reader[3].ToString() + ":" + 	//jump.sessionID
					reader[4].ToString() + ":" + 	//jump.type
					Util.ChangeDecimalSeparator(reader[5].ToString()) + ":" + 	//jump.tv
					Util.ChangeDecimalSeparator(reader[6].ToString()) + ":" + 	//jump.tc
					Util.ChangeDecimalSeparator(reader[7].ToString()) + ":" + 	//fall
					Util.ChangeDecimalSeparator(reader[8].ToString()) + ":" + 	//weight
					reader[9].ToString() + ":" +	//description
					Util.ChangeDecimalSeparator(reader[10].ToString()) + ":" +	//angle
					reader[11].ToString() + ":" +	//simulated
					reader[12].ToString() + ":" + 	//datetime
					reader[13].ToString() 		//person.weight
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

	//like SelectJumps above method but much better: return list of jumps
	//sID -1 means all sessions
	public static List<Jump> SelectJumps (int pID, int sID, string jumpType)
	{
	  //jumps previous to DB 1.82 have no datetime on jump
	  //find session datetime for that jumps
	  List<Session> session_l = SqliteSession.SelectAll();

	  string personID = pID.ToString();
	  string filterSessionString = "";
	  if(sID != -1)
		  filterSessionString = " AND sessionID == " + sID.ToString();

	  Sqlite.Open();

	  // Selecciona les dades de tots els salts
	  dbcmd.CommandText = "SELECT * FROM jump WHERE personID = " + personID +
		  filterSessionString +  " AND jump.type = \"" + jumpType + "\"";

	  LogB.SQL(dbcmd.CommandText.ToString());
	  dbcmd.ExecuteNonQuery();

	  SqliteDataReader reader;
	  reader = dbcmd.ExecuteReader();

	  List<Jump> jmp_l = DataReaderToJump (reader, session_l);

	  reader.Close();
	  Sqlite.Close();

	  return jmp_l;
	}

	public static Jump SelectJumpData(int uniqueID, bool dbconOpened)
	{
		if(!dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "SELECT * FROM jump WHERE uniqueID == " + uniqueID;
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		reader.Read();

		Jump myJump = new Jump(DataReaderToStringArray(reader, 12));
	
		reader.Close();
		
		if(!dbconOpened)
			Sqlite.Close();

		return myJump;
	}
	
	public static string [] SelectTestMaxStuff(int personID, JumpType jumpType) 
	{
		double tc = 0.0;
		if(! jumpType.StartIn)
			tc = 1; //just a mark meaning that tc has to be shown

		double tv = 1;
		//special cases where there's no tv
		if(jumpType.Name == Constants.TakeOffName || jumpType.Name == Constants.TakeOffWeightName)
			tv = 0.0;
	

		string sqlSelect = "";
		if(tv > 0) {
			if(tc <= 0)
				sqlSelect = "100*4.9*(jump.TV/2)*(jump.TV/2)";
			else
				sqlSelect = "jump.TV"; //if tc is higher than tv it will be fixed on PrepareJumpSimpleGraph
		} else
			sqlSelect = "jump.TC";
		
		Sqlite.Open();
		dbcmd.CommandText = "SELECT session.date, session.name, MAX(" + sqlSelect + "), jump.simulated " + 
			" FROM jump, session WHERE type = \"" + jumpType.Name + "\" AND personID = " + personID + 
			" AND jump.sessionID = session.uniqueID";
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		reader.Read();
		
		string [] str = DataReaderToStringArray(reader, 4);
		
		reader.Close();
		Sqlite.Close();

		return str;
	}
	
	public static List<Double> SelectChronojumpProfile (int pID, int sID)
	{
		string personID = pID.ToString();
		string sessionID = sID.ToString();

		Sqlite.Open();
		
		double sj = selectDouble( 
				"SELECT MAX(tv * tv * 1.226) " +
				" FROM jump " +
				" WHERE type = \"SJ\" " +
				" AND personID = " + personID + " AND sessionID = " + sessionID);
		
		double sjl = selectDouble( 
				"SELECT MAX(tv * tv * 1.226) " +
				" FROM jump " +
				" WHERE type = \"SJl\" AND jump.weight = 100 " +
				" AND personID = " + personID + " AND sessionID = " + sessionID);
		
		double cmj = selectDouble( 
				"SELECT MAX(tv * tv * 1.226) " +
				" FROM jump " +
				" WHERE type = \"CMJ\" " +
				" AND personID = " + personID + " AND sessionID = " + sessionID);
		
		double abk = selectDouble( 
				"SELECT MAX(tv * tv * 1.226) " +
				" FROM jump " +
				" WHERE type = \"ABK\" " +
				" AND personID = " + personID + " AND sessionID = " + sessionID);
		
		double dja = selectDouble( 
				"SELECT MAX(tv * tv * 1.226) " +
				" FROM jump " +
				" WHERE type = \"DJa\" " +
				" AND personID = " + personID + " AND sessionID = " + sessionID);

		Sqlite.Close();

		List<Double> l = new List<Double>();
		l.Add(sj);
	        l.Add(sjl);
	        l.Add(cmj);
		l.Add(abk);
		l.Add(dja);
		return l;
	}

	private static List<Jump> DataReaderToJump (SqliteDataReader reader, List<Session> session_l)
	{
	  List<Jump> jmp_l = new List<Jump>();
	  Jump jmp;

	  LogB.Information("Imprimire Jumps:");
	  while(reader.Read()) {
		  jmp = new Jump (
				  Convert.ToInt32(reader[0].ToString()),              //uniqueID
				  Convert.ToInt32(reader[1].ToString()),	            //personID
				  Convert.ToInt32(reader[2].ToString()),	            //sessionID
				  reader[3].ToString(),                               //type
				  Convert.ToDouble(Util.ChangeDecimalSeparator(
						  reader[4].ToString())),                         //tv
				  Convert.ToDouble(Util.ChangeDecimalSeparator(
						  reader[5].ToString())),                          //tc
				  Convert.ToDouble(Util.ChangeDecimalSeparator(
						  reader[6].ToString())),                          //fall
				  Convert.ToDouble(Util.ChangeDecimalSeparator(
						  reader[7].ToString())),                        //weight
				  reader[8].ToString(),                                // description
				  Convert.ToDouble(Util.ChangeDecimalSeparator(
						  reader[9].ToString())),                          //angle
				  Convert.ToInt32(reader[10].ToString()),              //simulated
				  reader[11].ToString()                               //datetime
				 );

		  //jumps previous to DB 1.82 have no datetime on jump
		  //find session datetime for that jumps
		  if(jmp.Datetime == "")
			  foreach(Session session in session_l)
				  if(session.UniqueID == jmp.SessionID)
					  jmp.Datetime = UtilDate.ToFile(session.Date);

		  jmp_l.Add(jmp);
		  LogB.Information(jmp.ToString());
	  }
	  return jmp_l;
	}

	//last boolean: on JumpsDj analyze graph, only show the higher of values of the same fall
	public static List<Jump> SelectDJ (int pID, int sID, string jumpType, bool onlyHigherOfSameFall)
	{
	  //jumps previous to DB 1.82 have no datetime on jump
	  //find session datetime for that jumps
	  List<Session> session_l = SqliteSession.SelectAll();

	  string personID = pID.ToString();
	  string sessionID = sID.ToString();

	  Sqlite.Open();

	  // Selecciona les dades de tots els salts
	  dbcmd.CommandText = "SELECT * FROM jump WHERE personID = " + personID +
	  " AND sessionID = " + sessionID  +  " AND jump.type = \"" + jumpType + "\"";

	  if(onlyHigherOfSameFall)
		  dbcmd.CommandText += " ORDER BY fall DESC, tv DESC";

	  LogB.SQL(dbcmd.CommandText.ToString());
	  dbcmd.ExecuteNonQuery();

	  SqliteDataReader reader;
	  reader = dbcmd.ExecuteReader();

	  List<Jump> jmp_l = DataReaderToJump (reader, session_l);

	  reader.Close();
	  Sqlite.Close();

	  if(onlyHigherOfSameFall)
	  {
		  List<Jump> jmp_l_purged = new List<Jump>();
		  double lastFall = 0;
		  foreach(Jump j in jmp_l)
		  {
			  if(j.Fall != lastFall)
				  jmp_l_purged.Add(j);

			  lastFall = j.Fall;
		  }
		  return jmp_l_purged;
	  }

	  return jmp_l;
	}

	//TODO: too similar to above method, unify them
	//TODO: note we do not want % weight, we want absolute weight so we need to select on personSession77 table
	public static List<Jump> SelectJumpsWeightFVProfile (int pID, int sID, bool onlyHigherOfSameWeight)
	{
	  //jumps previous to DB 1.82 have no datetime on jump
	  //find session datetime for that jumps
	  List<Session> session_l = SqliteSession.SelectAll();

	  string personID = pID.ToString();
	  string sessionID = sID.ToString();

	  Sqlite.Open();

	  // Selecciona les dades de tots els salts
	  dbcmd.CommandText = "SELECT * FROM jump WHERE personID = " + personID +
	  " AND sessionID = " + sessionID  +  " AND (jump.type = \"SJ\" OR jump.type = \"SJl\")";

	  if(onlyHigherOfSameWeight)
		  dbcmd.CommandText += " ORDER BY weight DESC, tv DESC";

	  LogB.SQL(dbcmd.CommandText.ToString());
	  dbcmd.ExecuteNonQuery();

	  SqliteDataReader reader;
	  reader = dbcmd.ExecuteReader();

	  List<Jump> jmp_l = DataReaderToJump (reader, session_l);

	  reader.Close();
	  Sqlite.Close();

	  if(onlyHigherOfSameWeight)
	  {
		  LogB.Information("PPPP");
		  List<Jump> jmp_l_purged = new List<Jump>();
		  double lastWeight = 0;
		  foreach(Jump j in jmp_l)
		  {
			  if(j.Weight != lastWeight)
				  jmp_l_purged.Add(j);

			  lastWeight = j.Weight;
		  }
		  return jmp_l_purged;
	  }

	  return jmp_l;
	}

	public static void Update(int jumpID, string type, string tv, string tc, string fall, int personID, double weight, string description, double angle)
	{
		Sqlite.Open();
		dbcmd.CommandText = "UPDATE jump SET personID = " + personID + 
			", type = \"" + type +
			"\", tv = " + Util.ConvertToPoint(tv) +
			", tc = " + Util.ConvertToPoint(tc) +
			", fall = " + Util.ConvertToPoint(fall) +
			", weight = " + Util.ConvertToPoint(weight) + 
			", description = \"" + description +
			"\", angle = " + Util.ConvertToPoint(angle) +
			" WHERE uniqueID == " + jumpID ;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		Sqlite.Close();
	}

	public static void UpdateWeight(string tableName, int uniqueID, double weight)
	{
		Sqlite.Open();
		dbcmd.CommandText = "UPDATE " + tableName + " SET weight = " + Util.ConvertToPoint(weight) + 
			" WHERE uniqueID == " + uniqueID ;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		Sqlite.Close();
	}

	public static void UpdateDescription(string tableName, int uniqueID, string description)
	{
		Sqlite.Open();
		dbcmd.CommandText = "UPDATE " + tableName + " SET description = \"" + description + 
			"\" WHERE uniqueID == " + uniqueID ;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		Sqlite.Close();
	}

	//onle for change SJ+ CMJ+ and ABK+ to SJl...
	public static void ChangeWeightToL()
	{
		dbcmd.CommandText = "UPDATE jump SET type = \"SJl\" WHERE type == \"SJ+\"";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		dbcmd.CommandText = "UPDATE jump SET type = \"CMJl\" WHERE type == \"CMJ+\"";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		dbcmd.CommandText = "UPDATE jump SET type = \"ABKl\" WHERE type == \"ABK+\"";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}
}
