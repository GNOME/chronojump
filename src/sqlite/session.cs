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
using Mono.Unix;
using System.Collections.Generic;


public class SqliteSessionSwitcher
{
	/** SqliteSessionSwitcher implements two methods that SqliteSesion class had (SelectAllSessionsTesstCount and Select).
 	* These methods are used by SessionLoadWindow and depending on the parameters passed to the
	* SqliteSessionSwitcher constructor:
	* -it might use the static methods of SqliteSession (so it will access the main chronojump.db file)
	* -it might use a specific database (depends on databasePath)
	* 
	* Doing this SessionLoadWindow can display sessions from other databases when SessionLoadWindow
	* is used to import sessions from another Chronojump database instead of displaying sessions of the current
	* database.
	*
	* on import we want to hide simulated, because we do not want to see that session
	* on export we want to show it to delete it and all its tests
	*/
	private string databasePath;
	private DatabaseType type;
	public enum DatabaseType
	{
		DEFAULT,
		IMPORT,
		EXPORT
	};
	
	public SqliteSessionSwitcher(DatabaseType type, string databasePath)
	{
		this.type = type;
		this.databasePath = databasePath;
	}

	public List<SessionTestsCount> SelectAllSessionsTestsCount (string filterName)
	{
		if (type == DatabaseType.DEFAULT)
		{
			return SqliteSession.SelectAllSessionsTestsCount (filterName);
		}
		else
		{
			SqliteGeneral sqliteGeneral = new SqliteGeneral(databasePath);
			if (! sqliteGeneral.IsOpened)
				return new List<SessionTestsCount> ();

			SqliteConnection dbcon = sqliteGeneral.connection;

			List<SessionTestsCount> allSessions_l = SqliteSession.SelectAllSessionsTestsCount (filterName, dbcon);
			/*
			   does not help to be able to export/compress the used chronojump.db
			sqliteGeneral.CloseConnection ();
			sqliteGeneral = null;
			*/

			// on IMPORT, filtered sessions will contain all sessions but not the "SIMULATED"
			List<SessionTestsCount> stc_l = new List<SessionTestsCount> ();
			foreach(SessionTestsCount stc in allSessions_l)
			{
				if (type == DatabaseType.IMPORT && stc.sessionParams.Name == "SIMULATED")
					continue;

				stc_l.Add (stc);
			}

			return stc_l;
		}
	}

	public Session Select(string myUniqueID)
	{
		if (type == DatabaseType.DEFAULT)
		{
			return SqliteSession.Select (myUniqueID);
		}
		else
		{
			// This code could be refactored from existing code in SqliteSession::Select()

			SqliteGeneral sqliteGeneral = new SqliteGeneral(databasePath);
			SqliteCommand dbcommand = sqliteGeneral.command();

			dbcommand.CommandText = "SELECT * FROM Session WHERE uniqueID == @myUniqueID";
			dbcommand.Parameters.Add (new SqliteParameter ("@myUniqueID", myUniqueID));

			SqliteDataReader reader = dbcommand.ExecuteReader ();

			// Copied from a non-callable (yet) static method: SqliteSession::Select()
			string [] values = new string[9];

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
			}
			reader.Close();

			Session mySession = new Session(values[0], 
			                                values[1], values[2], UtilDate.FromSql(values[3]), 
			                                Convert.ToInt32(values[4]), Convert.ToInt32(values[5]), Convert.ToInt32(values[6]), 
			                                values[7], Convert.ToInt32(values[8]) );

			return mySession;
		}
	}

	//for export session
	//TODO; at the moment use the above string[] SelectAllSessionsTestsCount (string filterName)
	/*
	public List<Session> SelectAll()
	{
	}
	*/

	public void DeleteAllStuff(string sessionID)
	{
		SqliteGeneral sqliteGeneral = new SqliteGeneral(databasePath);
		SqliteConnection dbcon = sqliteGeneral.connection;

		SqliteSession.DeleteAllStuff (sessionID, dbcon);
	}
}

class SqliteSession : Sqlite
{
	public SqliteSession() {
	}
	
	~SqliteSession() {}

	//can be "Constants.SessionTable" or "Constants.ConvertTempTable"
	//temp is used to modify table between different database versions if needed
	//protected new internal static void createTable(string tableName)
	protected override void createTable(string tableName)
	{
		dbcmd.CommandText = 
			"CREATE TABLE " + tableName + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"name TEXT, " +
			"place TEXT, " +
			"date TEXT, " +	 //YYYY-MM-DD since db 0.72	
			"personsSportID INT, " + 
			"personsSpeciallityID INT, " + 
			"personsPractice INT, " + //also called "level"
			"comments TEXT, " +
			"serverUniqueID INT " +
			" ) ";
		dbcmd.ExecuteNonQuery();
	}
	
	public static int Insert(bool dbconOpened, string tableName, string uniqueID, string name, string place, DateTime date, int personsSportID, int personsSpeciallityID, int personsPractice, string comments, int serverUniqueID)
	{
		if(! dbconOpened)
			Sqlite.Open();

		if(uniqueID == "-1")
			uniqueID = "NULL";

		dbcmd.CommandText = "INSERT INTO " + tableName + " (uniqueID, name, place, date, personsSportID, personsSpeciallityID, personsPractice, comments, serverUniqueID)" +
			" VALUES (" + uniqueID + ", \""
			+ name + "\", \"" + place + "\", \"" + UtilDate.ToSql(date) + "\", " + 
			personsSportID + ", " + personsSpeciallityID + ", " + 
			personsPractice + ", \"" + comments + "\", " +
			serverUniqueID + ")" ;
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

	protected internal static void insertSimulatedSession()
	{
		if(! Sqlite.Exists (true, Constants.SessionTable, Constants.SessionSimulatedName))
			Insert(true, Constants.SessionTable, "-1", Constants.SessionSimulatedName, "", DateTime.Today, 
					Constants.SportUndefinedID, Constants.SpeciallityUndefinedID, Constants.LevelUndefinedID,
					Catalog.GetString("Use this session to simulate tests."), Constants.ServerUndefinedID);
	}

	public static void Update(int uniqueID, string name, string place, DateTime date, int personsSportID, int personsSpeciallityID, int personsPractice, string comments) 
	{
		//TODO: serverUniqueID (but cannot be changed in gui/edit, then not need now)
		Sqlite.Open();
		dbcmd.CommandText = "UPDATE " + Constants.SessionTable + " " +
			" SET name = \"" + name +
			"\" , date = \"" + UtilDate.ToSql(date) +
			"\" , place = \"" + place +
			"\" , personsSportID = " + personsSportID +
			", personsSpeciallityID = " + personsSpeciallityID +
			", personsPractice = " + personsPractice +
			", comments = \"" + comments +
			"\" WHERE uniqueID == " + uniqueID;
		dbcmd.ExecuteNonQuery();
		Sqlite.Close();
	}
	
	//updating local session when it gets uploaded
	public static void UpdateServerUniqueID(int uniqueID, int serverID)
	{
		//if(!dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "UPDATE " +Constants.SessionTable + " SET serverUniqueID = " + serverID + 
			" WHERE uniqueID == " + uniqueID ;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		//if(!dbconOpened)
			Sqlite.Close();
	}

	// ---- use this methods ----

	//by name (only in gui/networks.cs configInit
	//be careful because name is not unique
	public static Session SelectByName(string name)
	{
		dbcmd.CommandText = "SELECT * FROM " + Constants.SessionTable + " WHERE LOWER(name) = LOWER(\"" + name + "\")";

		List<Session> session_l = selectDo(false, dbcmd);
		if(session_l.Count == 0)
			return new Session();

		//return (Session) selectDo(dbcmd)[0];
		return session_l[0];
	}
	//by ID (default
	public static Session Select(string myUniqueID)
	{
		dbcmd.CommandText = "SELECT * FROM " + Constants.SessionTable + " WHERE uniqueID == " + myUniqueID ; 

		List<Session> session_l = selectDo(false, dbcmd);
		if(session_l.Count == 0)
			return new Session();

		//return (Session) selectDo(dbcmd)[0];
		return session_l[0];
	}
	public static List<Session> SelectAll(bool dbconOpened, Orders_by orderBy)
	{
		string orderByStr = " ORDER BY uniqueID";
		if(orderBy == Orders_by.ID_DESC)
			orderByStr += " DESC";

		dbcmd.CommandText = "SELECT * FROM " + Constants.SessionTable + orderByStr;
		return selectDo(dbconOpened, dbcmd);
	}
	private static List<Session> selectDo(bool dbconOpened, SqliteCommand mydbcmd)
	{
		if( ! dbconOpened)
		{
			try {
				Sqlite.Open();
			} catch {
				//done because there's an eventual problem maybe thread related on very few starts of chronojump
				LogB.SQL("Catched dbcon problem at Session.Select");
				Sqlite.Close();
				Sqlite.Open();
				LogB.SQL("reopened again");
			}
		}
		LogB.SQL(mydbcmd.CommandText.ToString());

		
		SqliteDataReader reader;
		reader = mydbcmd.ExecuteReader();
		List<Session> session_l = new List<Session>();
	
		while(reader.Read())
		{
			Session session = new Session(
					reader[0].ToString(),
					reader[1].ToString(),
					reader[2].ToString(),
					UtilDate.FromSql(reader[3].ToString()),
					Convert.ToInt32(reader[4].ToString()),
					Convert.ToInt32(reader[5].ToString()),
					Convert.ToInt32(reader[6].ToString()),
					reader[7].ToString(),
					Convert.ToInt32(reader[8].ToString())
					);

			session_l.Add(session);
		}

		reader.Close();

		if( ! dbconOpened)
			Sqlite.Close();

		return session_l;
	}
	
	//used by the stats selector of sessions
	//also by PersonsRecuperateFromOtherSessionWindowBox (src/gui/person.cs)
	//sessionIdDisable allows to not return current session on selection, for returning all just put -1
	public static string[] SelectAllSessionsSimple(bool commentsDisable, int sessionIdDisable) 
	{
		string selectString = " uniqueID, name, place, date, comments ";
		if(commentsDisable) {
			selectString = " uniqueID, name, place, date ";
		}
		
		Sqlite.Open();
		//dbcmd.CommandText = "SELECT * FROM session ORDER BY uniqueID";
		dbcmd.CommandText = "SELECT " + selectString + " FROM " + Constants.SessionTable + " " + 
			" WHERE uniqueID != " + sessionIdDisable + " ORDER BY uniqueID";
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		ArrayList myArray = new ArrayList(2);

		int count = new int();
		count = 0;

		while(reader.Read()) {
			if(commentsDisable) {
				myArray.Add (reader[0].ToString() + ":" + reader[1].ToString() + ":" +
						reader[2].ToString() + ":" + UtilDate.FromSql(reader[3].ToString()).ToShortDateString() );
			} else {
				myArray.Add (reader[0].ToString() + ":" + reader[1].ToString() + ":" +
						reader[2].ToString() + ":" + UtilDate.FromSql(reader[3].ToString()).ToShortDateString() + ":" +
						reader[4].ToString() );
			}
			count ++;
		}

		reader.Close();
	
		//close database connection
		Sqlite.Close();

		string [] mySessions = new string[count];
		count =0;
		foreach (string line in myArray) {
			mySessions [count++] = line;
		}

		return mySessions;
	}

	// It's used by chronojump-importer and receives a specific database
	public static List<SessionTestsCount> SelectAllSessionsTestsCount (string filterName, SqliteConnection dbcon)
	{
		return selectAllSessionsTestsCountDo (filterName, -1, dbcon); //-1 for allTests, contrary to person show all events use
	}

	private static double testsProgress;
	private static int testsAll = 15;

	public static void TestsProgressReset ()
	{
		testsProgress = 0;
	}
	public static double TestsProgressGet ()
	{
		return UtilAll.DivideSafeFraction (testsProgress, testsAll);
	}


	// This is the usual chronojump's call (default database)
	public static List<SessionTestsCount> SelectAllSessionsTestsCount (string filterName)
	{
		Sqlite.Open();

		// SelectAllSessionsTestCount is used here and by the Chronojump importer to allow to pass an arbitrary dbcon.
		List<SessionTestsCount> stc_l = selectAllSessionsTestsCountDo (filterName, -1, dbcon);

		//close database connection
		Sqlite.Close();

		return stc_l;
	}

	//called from person show all events
	public static List<SessionTestsCount> SelectAllSessionsTestsCount (int personID)
	{
		Sqlite.Open();

		// SelectAllSessionsTestCount is used here and by the Chronojump importer to allow to pass an arbitrary dbcon.
		List<SessionTestsCount> stc_l = selectAllSessionsTestsCountDo ("", personID, dbcon);

		//close database connection
		Sqlite.Close();

		return stc_l;
	}

	private static List<SessionTestsCount> selectAllSessionsTestsCountDo (string filterName, int personID, SqliteConnection dbcon)
	{
		// This method should NOT use Sqlite.open() / Sqlite.close(): it should only use dbcon
		// to connect to the database. This method is used by the importer after opening an arbitrary
		// ChronoJump sqlite database. It needs to be refactored to the new database system.

		testsProgress = 0;
		dbcmd = dbcon.CreateCommand();

		string filterNameString = "";
		if(filterName != "")
			filterNameString = " AND LOWER(session.name) LIKE LOWER (\"%" + filterName  + "%\") ";

		if (personID < 0)
			dbcmd.CommandText =
				"SELECT session.*, sport.name, speciallity.name" +
				" FROM session, sport, speciallity " +
				" WHERE session.personsSportID == sport.uniqueID " + 
				" AND session.personsSpeciallityID == speciallity.UniqueID " +
				filterNameString + 
				" ORDER BY session.uniqueID";
		else {
			string tps = Constants.PersonSessionTable;
			dbcmd.CommandText =
				"SELECT session.*, sport.name, speciallity.name" +
				" FROM session, sport, speciallity, " + tps +
				" WHERE session.personsSportID == sport.uniqueID " +
				" AND session.personsSpeciallityID == speciallity.UniqueID " +
				" AND " + tps + ".personID = " + personID + " AND " + tps + ".sessionID = session.UniqueID" +
				filterNameString +
				" ORDER BY session.uniqueID";
		}

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		testsProgress = 1;

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		List<SessionParams> sessionParams_l = new List<SessionParams> ();

		while(reader.Read()) {
			string sportName = Catalog.GetString(reader[9].ToString());
			string speciallityName = ""; //to solve a gettext bug (probably because speciallity undefined name is "")			
			if(reader[10].ToString() != "")
				speciallityName = Catalog.GetString(reader[10].ToString());
			string levelName = Catalog.GetString(Util.FindLevelName(Convert.ToInt32(reader[6])));

			sessionParams_l.Add (new SessionParams (
						Convert.ToInt32 (reader[0].ToString()),
						reader[1].ToString(),
						reader[2].ToString(),
						UtilDate.FromSql(reader[3].ToString()).ToShortDateString(),
						sportName,
						speciallityName,
						levelName,
						reader[7].ToString() //desc
						));
		}

		reader.Close();
		testsProgress = 2;

		/* FIXME:
		 * all this thing it's because if someone has createds sessions without jumps or jumpers,
		 * and we make a GROUP BY selection, this sessions doesn't appear as results
		 * in the near future, learn better sqlite for solving this in a nicer way
		 * */
		/* another solution is not show nothing about jumpers and jumps, but show a button of "details"
		 * this will open a new window showing this values.
		 * this solution it's more "lighter" for people who have  abig DB
		 * */

		string wherePersonStr = "";
		string andPersonStr = "";
		if (personID >= 0)
		{
			wherePersonStr = string.Format (" WHERE personID = {0} ", personID);
			andPersonStr = string.Format (" AND personID = {0} ", personID);
		}

		//select persons of each session
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM " + Constants.PersonSessionTable + 
			wherePersonStr +
			" GROUP BY sessionID ORDER BY sessionID";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader_persons;
		reader_persons = dbcmd.ExecuteReader();
		ArrayList myArray_persons = new ArrayList(2);

		while(reader_persons.Read()) {
			myArray_persons.Add (reader_persons[0].ToString() + ":" + reader_persons[1].ToString() + ":" );
		}
		reader_persons.Close();
		testsProgress = 3;

		//select jumps of each session
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM " + Constants.JumpTable + 
			wherePersonStr +
			" GROUP BY sessionID ORDER BY sessionID";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader_jumps;
		reader_jumps = dbcmd.ExecuteReader();
		ArrayList myArray_jumps = new ArrayList(2);

		while(reader_jumps.Read()) {
			myArray_jumps.Add (reader_jumps[0].ToString() + ":" + reader_jumps[1].ToString() + ":" );
		}
		reader_jumps.Close();
		testsProgress = 4;

		//select jumpsRj of each session
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM " + Constants.JumpRjTable + 
			wherePersonStr +
			" GROUP BY sessionID ORDER BY sessionID";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader_jumpsRj;
		reader_jumpsRj = dbcmd.ExecuteReader();
		ArrayList myArray_jumpsRj = new ArrayList(2);

		while(reader_jumpsRj.Read()) {
			myArray_jumpsRj.Add (reader_jumpsRj[0].ToString() + ":" + reader_jumpsRj[1].ToString() + ":" );
		}
		reader_jumpsRj.Close();
		testsProgress = 5;

		//select runs of each session
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM " + Constants.RunTable + 
			wherePersonStr +
			" GROUP BY sessionID ORDER BY sessionID";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader_runs;
		reader_runs = dbcmd.ExecuteReader();
		ArrayList myArray_runs = new ArrayList(2);

		while(reader_runs.Read()) {
			myArray_runs.Add (reader_runs[0].ToString() + ":" + reader_runs[1].ToString() + ":" );
		}
		reader_runs.Close();
		testsProgress = 6;

		//select runsInterval of each session
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM " + Constants.RunIntervalTable + 
			wherePersonStr +
			" GROUP BY sessionID ORDER BY sessionID";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader_runs_interval;
		reader_runs_interval = dbcmd.ExecuteReader();
		ArrayList myArray_runs_interval = new ArrayList(2);

		while(reader_runs_interval.Read()) {
			myArray_runs_interval.Add (reader_runs_interval[0].ToString() + ":" + reader_runs_interval[1].ToString() + ":" );
		}
		reader_runs_interval.Close();
		testsProgress = 7;

		//select reaction time of each session
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM " + Constants.ReactionTimeTable + 
			wherePersonStr +
			" GROUP BY sessionID ORDER BY sessionID";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader_rt;
		reader_rt = dbcmd.ExecuteReader();
		ArrayList myArray_rt = new ArrayList(2);

		while(reader_rt.Read()) {
			myArray_rt.Add (reader_rt[0].ToString() + ":" + reader_rt[1].ToString() + ":" );
		}
		reader_rt.Close();
		testsProgress = 8;

		//select pulses of each session
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM " + Constants.PulseTable + 
			wherePersonStr +
			" GROUP BY sessionID ORDER BY sessionID";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader_pulses;
		reader_pulses = dbcmd.ExecuteReader();
		ArrayList myArray_pulses = new ArrayList(2);

		while(reader_pulses.Read()) {
			myArray_pulses.Add (reader_pulses[0].ToString() + ":" + reader_pulses[1].ToString() + ":" );
		}
		reader_pulses.Close();
		testsProgress = 9;

		//select multichronopic of each session
		dbcmd.CommandText = "SELECT sessionID, count(*) FROM " + Constants.MultiChronopicTable + 
			wherePersonStr +
			" GROUP BY sessionID ORDER BY sessionID";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader_mcs;
		reader_mcs = dbcmd.ExecuteReader();
		ArrayList myArray_mcs = new ArrayList(2);

		while(reader_mcs.Read()) {
			myArray_mcs.Add (reader_mcs[0].ToString() + ":" + reader_mcs[1].ToString() + ":" );
		}
		reader_mcs.Close();
		testsProgress = 10;

		//select encoder stuff of each session
		// 1st need to know the count to update the progressbar
		dbcmd.CommandText = "SELECT COUNT (*) FROM " + Constants.EncoderTable +
			wherePersonStr + " ORDER BY sessionID";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		SqliteDataReader reader_enc = dbcmd.ExecuteReader();
		int testsProgressSubCount = 0;
		if (reader_enc.Read())
			testsProgressSubCount = Convert.ToInt32 (reader_enc[0].ToString());
		reader_enc.Close();

		// now the actual select
		dbcmd.CommandText = "SELECT sessionID, encoderConfiguration, signalOrCurve FROM " + Constants.EncoderTable +
			wherePersonStr + " ORDER BY sessionID";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		reader_enc = dbcmd.ExecuteReader();
		ArrayList myArray_enc_g_s = new ArrayList(2); //gravitatory sets
		ArrayList myArray_enc_g_r = new ArrayList(2); //gravitatory repetitions
		ArrayList myArray_enc_i_s = new ArrayList(2); //inertial sets
		ArrayList myArray_enc_i_r = new ArrayList(2); //inertial repetitions

		int count_g_s = 0;	
		int count_g_r = 0;	
		int count_i_s = 0;
		int count_i_r = 0;
		int sessionBefore = -1;
		int sessionNow = -1;

		while(reader_enc.Read()) 
		{
			//get econf to separate gravitatory and inertial
			string [] strFull = reader_enc[1].ToString().Split(new char[] {':'});
			EncoderConfiguration econf = new EncoderConfiguration(
				(Constants.EncoderConfigurationNames) 
				Enum.Parse(typeof(Constants.EncoderConfigurationNames), strFull[0]) );

			sessionNow = Convert.ToInt32(reader_enc[0].ToString());
			if(sessionNow != sessionBefore && sessionBefore != -1) {
				myArray_enc_g_s.Add (sessionBefore.ToString() + ":" + count_g_s.ToString() + ":" );
				myArray_enc_g_r.Add (sessionBefore.ToString() + ":" + count_g_r.ToString() + ":" );
				myArray_enc_i_s.Add (sessionBefore.ToString() + ":" + count_i_s.ToString() + ":" );
				myArray_enc_i_r.Add (sessionBefore.ToString() + ":" + count_i_r.ToString() + ":" );
				count_g_s = 0;
				count_g_r = 0;
				count_i_s = 0;
				count_i_r = 0;
			}
			sessionBefore = sessionNow;

			if(! econf.has_inertia) {
				if(reader_enc[2].ToString() == "signal")
					count_g_s ++;
				else
					count_g_r ++;
			} else {
				if(reader_enc[2].ToString() == "signal")
					count_i_s ++;
				else
					count_i_r ++;
			}

			if (testsProgressSubCount > 0)
				testsProgress += (1.0 / testsProgressSubCount);
		}
		myArray_enc_g_s.Add (sessionBefore.ToString() + ":" + count_g_s.ToString() + ":" );
		myArray_enc_g_r.Add (sessionBefore.ToString() + ":" + count_g_r.ToString() + ":" );
		myArray_enc_i_s.Add (sessionBefore.ToString() + ":" + count_i_s.ToString() + ":" );
		myArray_enc_i_r.Add (sessionBefore.ToString() + ":" + count_i_r.ToString() + ":" );

		reader_enc.Close();
		testsProgress = 11;

		//select force sensor isometric of each session
		ArrayList myArray_fs_isometric = new ArrayList(2);

		//if we are importing from a session who was not the forceSensor table (db version < 1.68)
		if(tableExists(true, Constants.ForceSensorTable))
		{
			dbcmd.CommandText = "SELECT sessionID, count(*) FROM " + Constants.ForceSensorTable +
				" WHERE " + Constants.ForceSensorTable + ".stiffness < 0" + //isometric has stiffness -1.0
				andPersonStr +
				" GROUP BY sessionID ORDER BY sessionID";
			LogB.SQL(dbcmd.CommandText.ToString());
			dbcmd.ExecuteNonQuery();

			SqliteDataReader reader_fs_isometric;
			reader_fs_isometric = dbcmd.ExecuteReader();

			while(reader_fs_isometric.Read()) {
				myArray_fs_isometric.Add (reader_fs_isometric[0].ToString() + ":" + reader_fs_isometric[1].ToString() + ":" );
			}
			reader_fs_isometric.Close();
		}
		testsProgress = 12;

		//select force sensor elastic of each session
		ArrayList myArray_fs_elastic = new ArrayList(2);

		//if we are importing from a session who was not the forceSensor table (db version < 1.68)
		if(tableExists(true, Constants.ForceSensorTable))
		{
			dbcmd.CommandText = "SELECT sessionID, count(*) FROM " + Constants.ForceSensorTable +
				" WHERE " + Constants.ForceSensorTable + ".stiffness > 0" + //elastic has stiffness > 0
				andPersonStr +
				" GROUP BY sessionID ORDER BY sessionID";
			LogB.SQL(dbcmd.CommandText.ToString());
			dbcmd.ExecuteNonQuery();

			SqliteDataReader reader_fs_elastic;
			reader_fs_elastic = dbcmd.ExecuteReader();

			while(reader_fs_elastic.Read()) {
				myArray_fs_elastic.Add (reader_fs_elastic[0].ToString() + ":" + reader_fs_elastic[1].ToString() + ":" );
			}
			reader_fs_elastic.Close();
		}
		testsProgress = 13;

		//select run encoder of each session
		ArrayList myArray_re = new ArrayList(2);

		//if we are importing from a session who was not the forceSensor table (db version < 1.70)
		if(tableExists(true, Constants.RunEncoderTable))
		{
			dbcmd.CommandText = "SELECT sessionID, count(*) FROM " + Constants.RunEncoderTable +
				wherePersonStr +
				" GROUP BY sessionID ORDER BY sessionID";
			LogB.SQL(dbcmd.CommandText.ToString());
			dbcmd.ExecuteNonQuery();

			SqliteDataReader reader_re;
			reader_re = dbcmd.ExecuteReader();

			while(reader_re.Read()) {
				myArray_re.Add (reader_re[0].ToString() + ":" + reader_re[1].ToString() + ":" );
			}
			reader_re.Close();
		}
		testsProgress = 14;

		//mix all arrayLists
		List<SessionTestsCount> stc_l = new List<SessionTestsCount> ();
		foreach (SessionParams sessionParam in sessionParams_l)
		{
			SessionTestsCount stc = new SessionTestsCount ();

			//if some sessions are deleted, do not use count=0 to mix arrays, use sessionID of line
			//string [] mixingSessionFull = line.Split(new char[] {':'});
			//string mixingSessionID = mixingSessionFull[0];

			stc.sessionParams = sessionParam;
			int sID = stc.sessionParams.ID;

			stc.Persons = getTestsInTable (myArray_persons, sID);
			stc.JumpsSimple = getTestsInTable (myArray_jumps, sID);
			stc.JumpsReactive = getTestsInTable (myArray_jumpsRj, sID);
			stc.RunsSimple = getTestsInTable (myArray_runs, sID);
			stc.RunsInterval = getTestsInTable (myArray_runs_interval, sID);
			stc.RunsEncoder = getTestsInTable (myArray_re, sID);
			stc.Isometric = getTestsInTable (myArray_fs_isometric, sID);
			stc.Elastic = getTestsInTable (myArray_fs_elastic, sID);
			stc.WeightsSets = getTestsInTable (myArray_enc_g_s, sID);
			stc.WeightsReps = getTestsInTable (myArray_enc_g_r, sID);
			stc.InertialSets = getTestsInTable (myArray_enc_i_s, sID);
			stc.InertialReps = getTestsInTable (myArray_enc_i_r, sID);
			stc.ReactionTimeOld = getTestsInTable (myArray_rt, sID);
			stc.Pulses = getTestsInTable (myArray_pulses, sID);
			stc.MultiChronopic = getTestsInTable (myArray_mcs, sID);

			//mySessions [count++] = lineNotReadOnly;
			stc_l.Add (stc);
		}
		testsProgress = 15;

		return stc_l;
	}
	private static int getTestsInTable (ArrayList array, int sessionID)
	{
		foreach (string str in array)
		{
			string [] stringFull = str.Split (new char[] {':'});
			if (Convert.ToInt32 (stringFull[0]) == sessionID)
				return Convert.ToInt32 (stringFull [1]);
		}
		return 0;
	}


	//called from gui/event.cs for doing the graph
	//we need to know the avg of events of a type (SJ, CMJ, free (pulse).. of a person, or of all persons on the session
	//from 2.0 type can be "" so all types
	public static double SelectMAXEventsOfAType(bool dbconOpened, int sessionID, int personID,
			string table, string type, string valueToSelect)
	{
		return selectEventsOfAType(dbconOpened, sessionID, personID,
				table, type, valueToSelect, "MAX_AVG_MIN")[0];
	}
	public static double SelectAVGEventsOfAType(bool dbconOpened, int sessionID, int personID,
			string table, string type, string valueToSelect)
	{
		return selectEventsOfAType(dbconOpened, sessionID, personID,
				table, type, valueToSelect, "MAX_AVG_MIN")[1];
	}
	public static double SelectMINEventsOfAType(bool dbconOpened, int sessionID, int personID,
			string table, string type, string valueToSelect)
	{
		return selectEventsOfAType(dbconOpened, sessionID, personID,
				table, type, valueToSelect, "MAX_AVG_MIN")[2];
	}

	//to have the three in one call, much better, use this in new code
	public static List<double> Select_MAX_AVG_MIN_EventsOfAType(bool dbconOpened, int sessionID, int personID,
			string table, string type, string valueToSelect)
	{
		return selectEventsOfAType(dbconOpened, sessionID, personID,
				table, type, valueToSelect, "MAX_AVG_MIN");
	}

	private static List<double> selectEventsOfAType(bool dbconOpened, int sessionID, int personID,
			string table, string type, string valueToSelect, string statistic) 
	{
		if(!dbconOpened)
			Sqlite.Open();

		string connector = " WHERE "; //WHERE or AND
		
		string sessionIDString = "";
		if(sessionID != -1)
		{
			sessionIDString = connector + "sessionID = " + sessionID;
			connector = " AND ";
		}

		string personIDString = "";
		if(personID != -1)
		{
			personIDString = connector + "personID = " + personID;
			connector = " AND ";
		}

		string typeString = "";
		if(type != "")
		{
			typeString = connector + "type = \"" + type + "\"";
			connector = " AND ";
		}

		string selectString = statistic + "(" + valueToSelect + ")";
		if(statistic == "MAX_AVG_MIN")
			selectString = "MAX (" + valueToSelect + "), " +
				"AVG (" + valueToSelect + "), " +
				"MIN (" + valueToSelect + ")";
		
		dbcmd.CommandText = "SELECT " + selectString +
			" FROM " + table +				
			sessionIDString +
			personIDString + 
			typeString;
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		List<double> return_l = new List<double>();
		while(reader.Read()) {
			return_l.Add(Convert.ToDouble(Util.ChangeDecimalSeparator(reader[0].ToString())));
			return_l.Add(Convert.ToDouble(Util.ChangeDecimalSeparator(reader[1].ToString())));
			return_l.Add(Convert.ToDouble(Util.ChangeDecimalSeparator(reader[2].ToString())));
		}
		reader.Close();
		
		if(!dbconOpened)
			Sqlite.Close();

		if(return_l.Count == 0)
		{
			return_l.Add(0);
			return_l.Add(0);
			return_l.Add(0);
		}
		return return_l;
	}

	// It's used by export and receives a specific database
	// we want to delete all stuff of unwanted sessions
	public static void DeleteAllStuff(string sessionID, SqliteConnection dbcon)
	{
		Sqlite.Open();

		deleteAllStuffDo (sessionID, dbcon, true);

		Sqlite.Close();
	}
	
	// This is the usual chronojump's call (default database)
	public static void DeleteAllStuff(string sessionID)
	{
		Sqlite.Open();

		deleteAllStuffDo (sessionID, dbcon, false);

		Sqlite.Close();
	}

	private static void deleteAllStuffDo (string sessionID, SqliteConnection dbcon, bool export)
	{
		LogB.Information("DeleteAllStuffDo 0");
		dbcmd = dbcon.CreateCommand();

		// 1) delete the session
		dbcmd.CommandText = "Delete FROM " + Constants.SessionTable + " WHERE uniqueID == " + sessionID;
		dbcmd.ExecuteNonQuery();
		
		// 2) delete relations (existance) within persons and sessions in this session
		dbcmd.CommandText = "Delete FROM " + Constants.PersonSessionTable + " WHERE sessionID == " + sessionID;
		dbcmd.ExecuteNonQuery();

		LogB.Information("DeleteAllStuffDo 1");
		//TODO: take care on export to do that but passing dbcon
		if(! export)
			Sqlite.deleteOrphanedPersons();
		LogB.Information("DeleteAllStuffDo 2");
		
		// 3) delete tests without files

		//delete simple jumps
		dbcmd.CommandText = "Delete FROM " + Constants.JumpTable + " WHERE sessionID == " + sessionID;
		dbcmd.ExecuteNonQuery();
		
		//delete repetitive jumps
		dbcmd.CommandText = "Delete FROM " + Constants.JumpRjTable + " WHERE sessionID == " + sessionID;
		dbcmd.ExecuteNonQuery();
		
		//delete simple runs
		dbcmd.CommandText = "Delete FROM " + Constants.RunTable + " WHERE sessionID == " + sessionID;
		dbcmd.ExecuteNonQuery();
		
		//delete intervallic runs
		dbcmd.CommandText = "Delete FROM " + Constants.RunIntervalTable + " WHERE sessionID == " + sessionID;
		dbcmd.ExecuteNonQuery();
		
		//delete reaction times
		dbcmd.CommandText = "Delete FROM " + Constants.ReactionTimeTable + " WHERE sessionID == " + sessionID;
		dbcmd.ExecuteNonQuery();
		
		//delete pulses
		dbcmd.CommandText = "Delete FROM " + Constants.PulseTable + " WHERE sessionID == " + sessionID;
		dbcmd.ExecuteNonQuery();
		
		//delete multiChronopic
		dbcmd.CommandText = "Delete FROM " + Constants.MultiChronopicTable + " WHERE sessionID == " + sessionID;
		dbcmd.ExecuteNonQuery();
		
		// 4) delete from encoder start ------>

		/*
		 * on export we only want to delete SQL stuff, because files of other sessions will not be copied
		 * but note we use dbcmd, if we want to call some other SQL method, we need to take care to pass dbcon or dbcmd
		 */

		SqliteDataReader reader;
		if(export)
		{
			// 1 get all the encoder signals of that session
			dbcmd.CommandText = "SELECT uniqueID FROM " + Constants.EncoderTable +
				" WHERE signalOrCurve = \"signal\"" +
				" AND sessionID = " + sessionID;

			reader = dbcmd.ExecuteReader();
			List<string> signal_l = new List<string>();

			// 2 delete all the EncoderSignalCurves (relation with signal and curves) of that signals, and also triggers
			while(reader.Read())
				signal_l.Add(reader[0].ToString());

			reader.Close();

			foreach(string signal in signal_l)
			{
				dbcmd.CommandText = "Delete FROM " + Constants.EncoderSignalCurveTable +
					" WHERE signalID = " + signal;
				dbcmd.ExecuteNonQuery();

				// delete related triggers
				//SqliteTrigger.DeleteByModeID(true, Convert.ToInt32(signal));
				//to export we have to do it with the dbcmd:
				dbcmd.CommandText = "Delete FROM " + Constants.TriggerTable +
					" WHERE mode = \"" + Trigger.Modes.ENCODER.ToString() +
					"\" AND modeID = " + Convert.ToInt32(signal);
				LogB.SQL(dbcmd.CommandText.ToString());
				dbcmd.ExecuteNonQuery();
			}

			// 3 delete all encoder table stuff (signals and curves)
			dbcmd.CommandText = "Delete FROM " + Constants.EncoderTable + " WHERE sessionID = " + sessionID;
			dbcmd.ExecuteNonQuery();
		} else
		{
			//signals
			ArrayList encoderArray = SqliteEncoder.Select(
					true, -1, -1, Convert.ToInt32(sessionID), Constants.EncoderGI.ALL,
					-1, "signal", EncoderSQL.Eccons.ALL, "",
					false, true, false);

			foreach(EncoderSQL eSQL in encoderArray) {
				Util.FileDelete(eSQL.GetFullURL(false));	//signal, don't convertPathToR
				if(eSQL.videoURL != "")
					Util.FileDelete(eSQL.videoURL);		//video
				Sqlite.Delete(true, Constants.EncoderTable, Convert.ToInt32(eSQL.uniqueID));
			}

			//curves
			encoderArray = SqliteEncoder.Select(
					true, -1, -1, Convert.ToInt32(sessionID), Constants.EncoderGI.ALL,
					-1, "curve",  EncoderSQL.Eccons.ALL, "",
					false, true, true);

			foreach(EncoderSQL eSQL in encoderArray) {
				Util.FileDelete(eSQL.GetFullURL(false));	//don't convertPathToR
				/* commented: curve has no video
				   if(eSQL.videoURL != "")
				   Util.FileDelete(eSQL.videoURL);
				   */
				Sqlite.Delete(true, Constants.EncoderTable, Convert.ToInt32(eSQL.uniqueID));
				SqliteEncoder.DeleteSignalCurveWithCurveID(true, Convert.ToInt32(eSQL.uniqueID));

				//delete related triggers
				SqliteTrigger.DeleteByModeID(true, Trigger.Modes.ENCODER, Convert.ToInt32(eSQL.uniqueID));
			}
		}
		
		//<------- delete from encoder end

		// 5) delete forceSensor start ----->

		// delete triggers
		//List<ForceSensor> fs_l = SqliteForceSensor.Select (true, -1, -1, Convert.ToInt32(sessionID)); //this will not work on export because we cannot use this dbcmd
		dbcmd.CommandText = "SELECT uniqueID FROM " + Constants.ForceSensorTable +
				" WHERE sessionID = " + sessionID;

		reader = dbcmd.ExecuteReader();
		List<int> uniqueID_l = new List<int>();
		while(reader.Read())
			uniqueID_l.Add(Convert.ToInt32(reader[0].ToString()));

		reader.Close();

		foreach(int id in uniqueID_l)
		{
			//delete related triggers
			//SqliteTrigger.DeleteByModeID(true, Convert.ToInt32(fs.UniqueID));
			//to export we have to do it with the dbcmd:
			dbcmd.CommandText = "Delete FROM " + Constants.TriggerTable +
				" WHERE mode = \"" + Trigger.Modes.FORCESENSOR.ToString() +
				"\" AND modeID = " + id;
			LogB.SQL(dbcmd.CommandText.ToString());
			dbcmd.ExecuteNonQuery();
		}

		// delete forceSensor sets
		dbcmd.CommandText = "Delete FROM " + Constants.ForceSensorTable + " WHERE sessionID = " + sessionID;
		dbcmd.ExecuteNonQuery();

		System.IO.DirectoryInfo folderSession;
		//on export we only want to delete SQL stuff, because files of other sessions will not be copied
		if(! export)
		{
			folderSession = new System.IO.DirectoryInfo(
					Util.GetForceSensorSessionDir(Convert.ToInt32(sessionID)));

			if(folderSession.Exists)
				foreach (FileInfo file in folderSession.GetFiles())
					Util.FileDelete(file.Name);
		}

		// <----- delete forceSensor end

		// 6) delete runEncoder start ----->

		// delete triggers
		//ArrayList re_array = SqliteRunEncoder.Select (true, -1, -1, Convert.ToInt32(sessionID));  //this will not work on export because we cannot use this dbcmd
		dbcmd.CommandText = "SELECT uniqueID FROM " + Constants.RunEncoderTable +
				" WHERE sessionID = " + sessionID;

		reader = dbcmd.ExecuteReader();
		uniqueID_l = new List<int>();
		while(reader.Read())
			uniqueID_l.Add(Convert.ToInt32(reader[0].ToString()));

		reader.Close();

		foreach(int id in uniqueID_l)
		{
			//delete related triggers
			//SqliteTrigger.DeleteByModeID(true, Convert.ToInt32(re.UniqueID));
			//to export we have to do it with the dbcmd:
			dbcmd.CommandText = "Delete FROM " + Constants.TriggerTable +
				" WHERE mode = \"" + Trigger.Modes.RACEANALYZER.ToString() +
				"\" AND modeID = " + id;
			LogB.SQL(dbcmd.CommandText.ToString());
			dbcmd.ExecuteNonQuery();
		}

		// delete runEncoder sets
		dbcmd.CommandText = "Delete FROM " + Constants.RunEncoderTable + " WHERE sessionID = " + sessionID;
		dbcmd.ExecuteNonQuery();

		//on export we only want to delete SQL stuff, because files of other sessions will not be copied
		if(! export)
		{
			folderSession = new System.IO.DirectoryInfo(
					Util.GetRunEncoderSessionDir(Convert.ToInt32(sessionID)));

			if(folderSession.Exists)
				foreach (FileInfo file in folderSession.GetFiles())
					Util.FileDelete(file.Name);
		}

		// <----- delete runEncoder end

		// 7) delete videos
		//on export we only want to delete SQL stuff, because files of other sessions will not be copied
		if(! export)
		{
			folderSession = new System.IO.DirectoryInfo(
					Util.GetVideoSessionDir (Convert.ToInt32(sessionID)));

			if(folderSession.Exists)
				foreach (FileInfo file in folderSession.GetFiles())
					Util.FileDelete(file.Name);
		}
	}

}

class SqliteServerSession : SqliteSession
{
	public SqliteServerSession() {
	}
	
	protected override void createTable(string tableName)
	{
		string serverSpecificString = 
			", evaluatorID INT " +
			", evaluatorCJVersion TEXT " + 
			", evaluatorOS TEXT " +
			", uploadedDate TEXT " + //YYYY-MM-DD since db 0.72	
			", uploadingState INT ";

		dbcmd.CommandText = 
			"CREATE TABLE " + tableName + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"name TEXT, " +
			"place TEXT, " +
			"date TEXT, " +	//YYYY-MM-DD since db 0.72		
			"personsSportID INT, " + 
			"personsSpeciallityID INT, " + 
			"personsPractice INT, " + //also called "level"
			"comments TEXT, " +
			"serverUniqueID INT " +
			serverSpecificString + 
			" ) ";
		dbcmd.ExecuteNonQuery();
	}
	
	public static int Insert(bool dbconOpened, string tableName, string name, string place, DateTime date, int personsSportID, int personsSpeciallityID, int personsPractice, string comments, int serverUniqueID, int evaluatorID, string evaluatorCJVersion, string evaluatorOS, DateTime uploadedDate, int uploadingState)
	{
		if(! dbconOpened)
			Sqlite.Open();

		//(uniqueID == "-1")
		//	uniqueID = "NULL";
		string uniqueID = "NULL";

		dbcmd.CommandText = "INSERT INTO " + tableName + " (uniqueID, name, place, date, personsSportID, personsSpeciallityID, personsPractice, comments, serverUniqueID, evaluatorID, evaluatorCJVersion, evaluatorOS, uploadedDate, uploadingState)" +
			" VALUES (" + uniqueID + ", \""
			+ name + "\", \"" + place + "\", \"" + UtilDate.ToSql(date) + "\", " + 
			personsSportID + ", " + personsSpeciallityID + ", " + 
			personsPractice + ", \"" + comments + "\", " +
			serverUniqueID + ", " + evaluatorID + ", \"" +
			evaluatorCJVersion + "\", \"" + evaluatorOS + "\", \"" +
			UtilDate.ToSql(uploadedDate) + "\", " + uploadingState +
			")" ;
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
	
	//updating local session when it gets uploaded
	public static void UpdateUploadingState(int uniqueID, int state)
	{
		//if(!dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "UPDATE " + Constants.SessionTable + " SET uploadingState = " + state + 
			" WHERE uniqueID == " + uniqueID ;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		//if(!dbconOpened)
			Sqlite.Close();
	}

	
	~SqliteServerSession() {}
}
