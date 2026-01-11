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
 * Copyright (C) 2004-2026   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Data;
using System.IO;
using System.Collections; //ArrayList
using System.Collections.Generic; //List<T>
#if MICROSOFT_DATA_SQLITE
using Microsoft.Data.Sqlite;
using SQLiteTransaction = Microsoft.Data.Sqlite.SqliteTransaction;
using SQLiteCommand = Microsoft.Data.Sqlite.SqliteCommand;
using SQLiteDataReader = Microsoft.Data.Sqlite.SqliteDataReader;
using SQLiteConnection = Microsoft.Data.Sqlite.SqliteConnection;
#else
using System.Data.SQLite;
using SQLiteTransaction = System.Data.SQLite.SQLiteTransaction;
using SQLiteCommand = System.Data.SQLite.SQLiteCommand;
using SQLiteDataReader = System.Data.SQLite.SQLiteDataReader;
using SQLiteConnection = System.Data.SQLite.SQLiteConnection;
#endif

class SqliteJump : SqliteTests
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
			"weight TEXT, " +
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

	//normal Chronojump call (will pass dbcmd to the insert.
	//on SqliteFourPlatformsJumpsSimple it sends its SQLiteCommand to perform a transaction
	public static int Insert (bool dbconOpened, string tableName, string uniqueID, int personID, int sessionID, string type, double tv, double tc, double fall, double weight, string description, double angle, int simulated, string datetime)
	{
		return InsertDo (dbconOpened, tableName, uniqueID, personID, sessionID, type, tv, tc, fall, weight, description, angle, simulated, datetime, dbcmd);
	}

	public static int Insert (bool dbconOpened, string tableName, string uniqueID, int personID, int sessionID, string type, double tv, double tc, double fall, double weight, string description, double angle, int simulated, string datetime, SQLiteCommand mycmd)
	{
		return InsertDo (dbconOpened, tableName, uniqueID, personID, sessionID, type, tv, tc, fall, weight, description, angle, simulated, datetime, mycmd);
	}

	public static int InsertDo (bool dbconOpened, string tableName, string uniqueID, int personID, int sessionID, string type, double tv, double tc, double fall, double weight, string description, double angle, int simulated, string datetime, SQLiteCommand mycmd)
	{
		if(! dbconOpened)
			Sqlite.Open();

		if(uniqueID == "-1")
			uniqueID = "NULL";

		mycmd.CommandText = "INSERT INTO " + tableName +
				" (uniqueID, personID, sessionID, type, tv, tc, fall, weight, description, angle, simulated, datetime)" +
				" VALUES (" + uniqueID + ", "
				+ personID + ", " + sessionID + ", '" + type + "', "
				+ Util.ConvertToPoint(tv) + ", " + Util.ConvertToPoint(tc) + ", " + Util.ConvertToPoint(fall) + ", '" 
				+ Util.ConvertToPoint(weight) + "', '" + description + "', "
				+ Util.ConvertToPoint(angle) + ", " + simulated + ", '" + datetime + "')" ;
		LogB.SQL(mycmd.CommandText.ToString());
		mycmd.ExecuteNonQuery();

		//int myLast = dbcon.LastInsertRowId;
		//http://stackoverflow.com/questions/4341178/getting-the-last-insert-id-with-sqlite-net-in-c
		string myString = @"select last_insert_rowid()";
		mycmd.CommandText = myString;
		int myLast = Convert.ToInt32(mycmd.ExecuteScalar()); // Need to type-cast since `ExecuteScalar` returns an object.

		if(! dbconOpened)
			Sqlite.Close();

		return myLast;
	}

	//like SelectJumps, but this returns a string[] :( better use below method if possible
	//if all sessions, put -1 in sessionID
	//if all persons, put -1 in personID
	//if all types put, "" in filterType
	// limit 0 means no limit (limit negative is the last results)
	//SA for String Array
	public static string[] SelectJumpsSA (bool dbconOpened, int sessionID, int personID, string filterWeight, string filterType,
			Orders_by order, int limit) 
	{
		if(!dbconOpened)
			Sqlite.Open();

		string tp = Constants.PersonTable;
		string tps = Constants.PersonSessionTable;

		string filterSessionString = "";
		if(sessionID != -1)
			filterSessionString = " AND jump.sessionID = " + sessionID;

		string filterPersonString = "";
		if(personID != -1)
			filterPersonString = " AND " + tp + ".uniqueID = " + personID;

		string filterWeightString = "";
		if(filterWeight == "withWeight")
			filterWeightString = " AND jump.weight != 0 ";

		string filterTypeString = "";
		if(filterType != "")
			filterTypeString = " AND jump.type = '" + filterType + "' ";

		string orderByString = " ORDER BY upper(" + tp + ".name), jump.uniqueID ";
		if(order == Orders_by.ID_ASC)
			orderByString = " ORDER BY jump.uniqueID ";
		else if(order == Orders_by.ID_DESC)
			orderByString = " ORDER BY jump.uniqueID DESC ";
		
		string limitString = "";
		if(limit > 0)
			limitString = " LIMIT " + limit;

		dbcmd.CommandText = "SELECT " + tp + ".name AS person_name, jump.*, " + tps + ".weight AS personSession_weight" +
			" FROM " + tp + ", jump, " + tps + 
			" WHERE " + tp + ".uniqueID = jump.personID " + 
			filterSessionString +
			filterPersonString +
			filterWeightString +
			filterTypeString +
			" AND " + tps + ".personID = " + tp + ".uniqueID " +
			" AND " + tps + ".sessionID = jump.sessionID " +
			orderByString +
			limitString;
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SQLiteDataReader reader;
		reader = dbcmd.ExecuteReader();

		Dictionary<string, int> colOrder_d = readerOrdinals (
				reader,
				new List<string> {
					"person_name",
					"uniqueID", "personID", "sessionID",
					"type", "tv", "tc", "fall",
					"weight", "description", "angle",
					"simulated", "datetime",
					"personSession_weight"
				});

		ArrayList myArray = new ArrayList(2);

		int count = new int();
		count = 0;

		while(reader.Read())
		{
			myArray.Add (
					reader [colOrder_d["person_name"]].ToString () + ":" +
					reader [colOrder_d["uniqueID"]].ToString () + ":" +
					reader [colOrder_d["personID"]].ToString () + ":" +
					reader [colOrder_d["sessionID"]].ToString () + ":" +
					reader [colOrder_d["type"]].ToString () + ":" +
					Util.ChangeDecimalSeparator (reader [colOrder_d["tv"]].ToString ()) + ":" +
					Util.ChangeDecimalSeparator (reader [colOrder_d["tc"]].ToString ()) + ":" +
					Util.ChangeDecimalSeparator (reader [colOrder_d["fall"]].ToString ()) + ":" +
					Util.ChangeDecimalSeparator (reader [colOrder_d["weight"]].ToString ()) + ":" +
					reader [colOrder_d["description"]].ToString () + ":" +
					Util.ChangeDecimalSeparator (reader [colOrder_d["angle"]].ToString ()) + ":" +
					reader [colOrder_d["simulated"]].ToString () + ":" +
					reader [colOrder_d["datetime"]].ToString () + ":" +
					reader [colOrder_d["personSession_weight"]].ToString ()
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

	/*
	 * like SelectJumpsSA above method but much better: return list of jumps
	 * sID -1 means all sessions
	 * pID -1 means all persons
	 * jumpType "" means all jumps
	 * limit 0 means no limit (limit negative is the last results)
	 * personNameInComment is used to be able to display names in graphs
	 *   because event.PersonName makes individual SQL SELECTs
	 * this returns a List<Jump>
	 */
	public static List<Jump> SelectJumps (bool dbconOpened, int sID, int pID, string jumpType, Orders_by order, int limit, bool personNameInComment, bool onlyBestInSession)
	{
		openIfNeeded (dbconOpened); //  -------------------->

		//jumps previous to DB 1.82 have no datetime on jump
		//find session datetime for that jumps
		List<Session> session_l = SqliteSession.SelectAll(true, Sqlite.Orders_by.DEFAULT);

		//for personNameInComment
		List<Person> person_l =
			SqlitePersonSession.SelectCurrentSessionPersonsAsList(true, sID);

		dbcmd.CommandText = "SELECT * FROM jump " +
			selectDo (sID, pID, jumpType, order, limit, onlyBestInSession);

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SQLiteDataReader reader;
		reader = dbcmd.ExecuteReader();

		List<Jump> jmp_l = DataReaderToJump (reader, session_l, person_l, personNameInComment);

		reader.Close();
		closeIfNeeded (dbconOpened); // <--------------------

		//get last values on negative limit
		if (limit < 0 && jmp_l.Count + limit >= 0)
			jmp_l = jmp_l.GetRange (jmp_l.Count + limit, -1 * limit);

		return jmp_l;
	}

	// same as above but this returns a List<double>, and without the personNameInComment
	public static List<double> SelectJumps (bool dbconOpened, string selectParam, int sID, int pID, string jumpType, Orders_by order, int limit, bool onlyBestInSession)
	{
		openIfNeeded (dbconOpened); //  -------------------->

		dbcmd.CommandText = "SELECT " + selectParam + " FROM jump " +
			selectDo (sID, pID, jumpType, order, limit, onlyBestInSession);

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SQLiteDataReader reader;
		reader = dbcmd.ExecuteReader();

		//List<Jump> jmp_l = DataReaderToJump (reader, session_l);
		List<double> d_l = new List<double> ();
		while (reader.Read())
			d_l.Add (Convert.ToDouble (Util.ChangeDecimalSeparator (reader [0].ToString ())));

		reader.Close();
		closeIfNeeded (dbconOpened); // <--------------------

		//get last values on negative limit
		if (limit < 0 && d_l.Count + limit >= 0)
			d_l = d_l.GetRange (d_l.Count + limit, -1 * limit);

		return d_l;
	}

	private static string selectDo (int sID, int pID, string jumpType, Orders_by order, int limit, bool onlyBestInSession)
	{
		string andString = "";
		string sessionString = "";
		if(sID != -1)
		{
			sessionString = " sessionID = " + sID.ToString();
			andString = " AND ";
		}

		string personString = "";
		if(pID != -1)
		{
			personString = andString + " personID = " + pID.ToString();
			andString = " AND ";
		}

		string jumpTypeString = "";
		if(jumpType != "")
		{
			jumpTypeString = andString + " jump.type = '" + jumpType + "' ";
			andString = " AND ";
		}

		string whereString = "";
		if(sessionString != "" || personString != "" || jumpTypeString != "")
			whereString = " WHERE ";

		string orderByString = " ORDER BY jump.uniqueID "; //ID_ASC
		if(order == Orders_by.ID_DESC)
			orderByString = " ORDER BY jump.uniqueID DESC ";
		if(onlyBestInSession)
			orderByString = " ORDER BY jump.sessionID, jump.Tv DESC ";
		if(order == Orders_by.BEST)
			orderByString = " ORDER BY jump.Tv ";

		string limitString = "";
		if(limit > 0)
			limitString = " LIMIT " + limit;

		return whereString + sessionString + personString + jumpTypeString +
			orderByString + limitString;
	}

	//pID can be -1 for all
	public static List<string> SelectJumpsTypeInSession (bool dbconOpened, int sID, int pID)
	{
		if(!dbconOpened)
			Sqlite.Open(); //  -------------------->

		List<string> list = new List<string> ();

		string personIDStr = "";
		if (pID >= 0)
			personIDStr = " AND personID = " + pID;

		dbcmd.CommandText = "SELECT DISTINCT (type) FROM jump WHERE sessionID = " + sID +
			personIDStr + " ORDER BY type";
		LogB.SQL(dbcmd.CommandText.ToString());

		SQLiteDataReader reader;
		reader = dbcmd.ExecuteReader();

		while (reader.Read())
			list.Add (reader[0].ToString ());

		reader.Close ();
		if(!dbconOpened)
			Sqlite.Close(); // <--------------------

		return list;
	}

	//called once for each jumpType
	//pID can be -1 for all
	public static List<SqliteStruct.IntTypeDoubleDouble> SelectJumpsToCSVExport (bool dbconOpened, int sID, int pID, string jumpType)
	{
		if(!dbconOpened)
			Sqlite.Open(); //  -------------------->

		List<SqliteStruct.IntTypeDoubleDouble> list = new List<SqliteStruct.IntTypeDoubleDouble> ();

		string personIDStr = "";
		if (pID >= 0)
			personIDStr = " AND personID = " + pID;

		dbcmd.CommandText = "SELECT personID, type, " +
			"AVG (tv * tv * 1.22625), " +
			"MAX (tv * tv * 1.22625) " +
			" FROM jump WHERE sessionID = " + sID + personIDStr +
			" AND type = '" + jumpType + "'" +
			" GROUP BY personID ORDER BY personID";
		LogB.SQL(dbcmd.CommandText.ToString());

		SQLiteDataReader reader;
		reader = dbcmd.ExecuteReader();

		//read personname and 2 cols for each test
		while (reader.Read())
			list.Add (new SqliteStruct.IntTypeDoubleDouble (
						Convert.ToInt32 (reader[0].ToString ()),
						reader[1].ToString (),
						Convert.ToDouble (Util.ChangeDecimalSeparator (reader[2].ToString ())),
						Convert.ToDouble (Util.ChangeDecimalSeparator (reader[3].ToString ()))
						));

		reader.Close ();
		if(!dbconOpened)
			Sqlite.Close(); // <--------------------

		return list;
	}

	/* returns:
	   2022-09-20|CMJ|0.427596
	   2022-09-20|SJ|0.456648
	   2022-09-19|CMJ|0.733992
	   */
	//TODO: if no date, select the session date
	public static List<SqliteStruct.DateTypeResult> SelectJumpsStatsByDay (int pID, List<string> jumps_l, StatType statType)
	{
		List<SqliteStruct.DateTypeResult> list = new List<SqliteStruct.DateTypeResult> ();
		if (jumps_l.Count == 0)
			return list;

		Sqlite.Open();

		//jumps previous to DB 1.82 have no datetime on jump
		//find session datetime for that jumps
		//List<Session> session_l = SqliteSession.SelectAll(true, Sqlite.Orders_by.DEFAULT);

		string jumpTypes = "";
		string orStr = "";
		foreach (string str in jumps_l)
		{
			jumpTypes += orStr + string.Format ("type='{0}'", str);
			orStr = " OR ";
		}

		dbcmd.CommandText = string.Format ("SELECT SUBSTR(datetime, 1, 10) AS day, type, {0}(tv) FROM jump WHERE ({1}) AND personID={2} GROUP BY day, type ORDER BY day desc, type",
				statType, jumpTypes, pID);

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SQLiteDataReader reader;
		reader = dbcmd.ExecuteReader();

		//note DB 2.41 forces all old jump, jumpRj, run, runI to have datetime
		while (reader.Read())
			list.Add (new SqliteStruct.DateTypeResult (
						reader[0].ToString (),
						reader[1].ToString (),
						Convert.ToDouble(Util.ChangeDecimalSeparator(reader[2].ToString ()))
						));

		reader.Close ();
		Sqlite.Close ();

		return list;
	}

	public static Jump SelectJumpData(int uniqueID, bool dbconOpened)
	{
		if(!dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "SELECT * FROM jump WHERE uniqueID = " + uniqueID;
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SQLiteDataReader reader;
		reader = dbcmd.ExecuteReader();

		Jump myJump = DataReaderToJump (reader)[0];

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
			" FROM jump, session WHERE type = '" + jumpType.Name + "' AND personID = " + personID + 
			" AND jump.sessionID = session.uniqueID";
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SQLiteDataReader reader;
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
				"SELECT MAX(tv * tv * 1.22625) " +
				" FROM jump " +
				" WHERE type = 'SJ' " +
				" AND personID = " + personID + " AND sessionID = " + sessionID);
		
		double sjl = selectDouble( 
				"SELECT MAX(tv * tv * 1.22625) " +
				" FROM jump " +
				" WHERE type = 'SJl' AND jump.weight = 100 " +
				" AND personID = " + personID + " AND sessionID = " + sessionID);
		
		double cmj = selectDouble( 
				"SELECT MAX(tv * tv * 1.22625) " +
				" FROM jump " +
				" WHERE type = 'CMJ' " +
				" AND personID = " + personID + " AND sessionID = " + sessionID);
		
		double abk = selectDouble( 
				"SELECT MAX(tv * tv * 1.22625) " +
				" FROM jump " +
				" WHERE type = 'ABK' " +
				" AND personID = " + personID + " AND sessionID = " + sessionID);
		
		double dja = selectDouble( 
				"SELECT MAX(tv * tv * 1.22625) " +
				" FROM jump " +
				" WHERE type = 'DJa' " +
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

	private static List<Jump> DataReaderToJump (SQLiteDataReader reader)
	{
		return DataReaderToJump (reader,
				null,	//we do not care here by the session_l (datetime)
				new List<Person> (), false);
	}
	private static List<Jump> DataReaderToJump (SQLiteDataReader reader, List<Session> session_l)
	{
		return DataReaderToJump (reader, session_l, new List<Person> (), false);
	}
	private static List<Jump> DataReaderToJump (SQLiteDataReader reader, List<Session> session_l,
			List<Person> person_l, bool personNameInComment)
	{
	  List<Jump> jmp_l = new List<Jump>();
	  Jump jmp;

	  Dictionary<string, int> colOrder_d = readerOrdinals (
			  reader,
			  new List<string> {
			  "uniqueID", "personID", "sessionID",
			  "type", "tv", "tc", "fall",
			  "weight", "description", "angle",
			  "simulated", "datetime",
			  });

	  //LogB.Information("Imprimire Jumps:");
	  while(reader.Read()) {
		  jmp = new Jump (
				  Convert.ToInt32 (reader [colOrder_d["uniqueID"]].ToString ()),
				  Convert.ToInt32 (reader [colOrder_d["personID"]].ToString ()),
				  Convert.ToInt32 (reader [colOrder_d["sessionID"]].ToString ()),
				  reader [colOrder_d["type"]].ToString (),
				  Convert.ToDouble (Util.ChangeDecimalSeparator (reader [colOrder_d["tv"]].ToString ())),
				  Convert.ToDouble (Util.ChangeDecimalSeparator (reader [colOrder_d["tc"]].ToString ())),
				  Convert.ToDouble (Util.ChangeDecimalSeparator (reader [colOrder_d["fall"]].ToString ())),
				  Convert.ToDouble (Util.ChangeDecimalSeparator (reader [colOrder_d["weight"]].ToString ())),
				  reader [colOrder_d["description"]].ToString (),
				  Convert.ToDouble (Util.ChangeDecimalSeparator (reader [colOrder_d["angle"]].ToString ())),
				  Convert.ToInt32 (reader [colOrder_d["simulated"]].ToString ()),
				  reader [colOrder_d["datetime"]].ToString ()
				 );

		  //jumps previous to DB 1.82 have no datetime on jump
		  //find session datetime for that jumps
		  if(session_l != null && jmp.Datetime == "")
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

	//last boolean: on JumpsDj analyze graph, only show the higher of values of the same fall
	public static List<Jump> SelectDJ (int pID, int sID, string jumpType, bool onlyHigherOfSameFall)
	{
	  //jumps previous to DB 1.82 have no datetime on jump
	  //find session datetime for that jumps
	  List<Session> session_l = SqliteSession.SelectAll(false, Sqlite.Orders_by.DEFAULT);

	  string personID = pID.ToString();
	  string sessionID = sID.ToString();

	  Sqlite.Open();

	  // Selecciona les dades de tots els salts
	  dbcmd.CommandText = "SELECT * FROM jump WHERE personID = " + personID +
	  " AND sessionID = " + sessionID  +  " AND jump.type = '" + jumpType + "'";

	  if(onlyHigherOfSameFall)
		  dbcmd.CommandText += " ORDER BY fall DESC, tv DESC";

	  LogB.SQL(dbcmd.CommandText.ToString());
	  dbcmd.ExecuteNonQuery();

	  SQLiteDataReader reader;
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
	  List<Session> session_l = SqliteSession.SelectAll(false, Sqlite.Orders_by.DEFAULT);

	  string personID = pID.ToString();
	  string sessionID = sID.ToString();

	  Sqlite.Open();

	  // Selecciona les dades de tots els salts
	  dbcmd.CommandText = "SELECT * FROM jump WHERE personID = " + personID +
	  " AND sessionID = " + sessionID  +  " AND (jump.type = 'SJ' OR jump.type = 'SJl')";

	  if(onlyHigherOfSameWeight)
		  dbcmd.CommandText += " ORDER BY weight DESC, tv DESC";

	  LogB.SQL(dbcmd.CommandText.ToString());
	  dbcmd.ExecuteNonQuery();

	  SQLiteDataReader reader;
	  reader = dbcmd.ExecuteReader();

	  List<Jump> jmp_l = DataReaderToJump (reader, session_l);

	  reader.Close();
	  Sqlite.Close();

	  if(onlyHigherOfSameWeight)
	  {
		  List<Jump> jmp_l_purged = new List<Jump>();
		  double lastWeight = 0;
		  foreach(Jump j in jmp_l)
		  {
			  if(j.WeightPercent != lastWeight)
				  jmp_l_purged.Add(j);

			  lastWeight = j.WeightPercent;
		  }
		  return jmp_l_purged;
	  }

	  return jmp_l;
	}

	public static void Update(int jumpID, string type, string tv, string tc, string fall, int personID, double weight, string description, double angle)
	{
		Sqlite.Open();
		dbcmd.CommandText = "UPDATE jump SET personID = " + personID + 
			", type = '" + type +
			"', tv = " + Util.ConvertToPoint(tv) +
			", tc = " + Util.ConvertToPoint(tc) +
			", fall = " + Util.ConvertToPoint(fall) +
			", weight = " + Util.ConvertToPoint(weight) + 
			", description = '" + description +
			"', angle = " + Util.ConvertToPoint(angle) +
			" WHERE uniqueID = " + jumpID ;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		Sqlite.Close();
	}

	public static void UpdateWeight(string tableName, int uniqueID, double weight)
	{
		Sqlite.Open();
		dbcmd.CommandText = "UPDATE " + tableName + " SET weight = " + Util.ConvertToPoint(weight) + 
			" WHERE uniqueID = " + uniqueID ;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		Sqlite.Close();
	}

	public static void UpdateDescription(string tableName, int uniqueID, string description)
	{
		Sqlite.Open();
		dbcmd.CommandText = "UPDATE " + tableName + " SET description = '" + description + 
			"' WHERE uniqueID = " + uniqueID ;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		Sqlite.Close();
	}

	//onle for change SJ+ CMJ+ and ABK+ to SJl...
	public static void ChangeWeightToL()
	{
		dbcmd.CommandText = "UPDATE jump SET type = 'SJl' WHERE type = 'SJ+'";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		dbcmd.CommandText = "UPDATE jump SET type = 'CMJl' WHERE type = 'CMJ+'";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		dbcmd.CommandText = "UPDATE jump SET type = 'ABKl' WHERE type = 'ABK+'";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}
}
