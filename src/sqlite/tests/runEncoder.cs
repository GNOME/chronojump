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
 * Copyright (C) 2022-2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
//using System.Data;
using System.Collections;
using System.Collections.Generic; //List<T>
using System.IO; //DirectoryInfo
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
using System.Text.RegularExpressions; //Regex

class SqliteRunEncoder : SqliteTests
{
	private static string tableStatic = Constants.RunEncoderTable;

	public SqliteRunEncoder()
	{
		tableName = Constants.RunEncoderTable;
		columnsStr = " (uniqueID, personID, sessionID, exerciseID, device, distance, temperature, filename, url, dateTime, comments, videoURL, angle, totalTime, maxSpeed, maxAvgSpeed1s)";
	}

	~SqliteRunEncoder() {}

	/*
	 * create and initialize tables
	 */

	protected override void createTable()
	{
		dbcmd.CommandText =
			"CREATE TABLE " + tableName + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"personID INT, " +
			"sessionID INT, " +
			"exerciseID INT, " + //right now all will be exercise 0, until we have a clear idea of what exercises could be done and how can affect measurements
			"device TEXT, " +
			"distance INT, " +
			"temperature INT, " +
			"filename TEXT, " +
			"url TEXT, " +		//URL of data files. stored as relative
			"datetime TEXT, " + 	//2019-07-11_15-01-44
			"comments TEXT, " +
			"videoURL TEXT, " +	//URL of video of signals. stored as relative
			"angle INT, " +		//capture can be at angleDefault (or not), nice if you have a run inclinated exercise and you want to change the angle depending on the place you perform
			"totalTime INT, " +	//needed to sync with video. If we press finish when there are no pulses we cannot sync. If we use totalTime we can sync.
			"maxSpeed FLOAT, " +
			"maxAvgSpeed1s FLOAT)";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}

	public static void Update (bool dbconOpened, string updateString)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "UPDATE " + tableStatic + " SET " + updateString;

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);
	}

	public static void UpdateComments (bool dbconOpened, int uniqueID, string comments)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "UPDATE " + tableStatic + " SET comments = '" + comments + "'" +
			" WHERE uniqueID = " + uniqueID;

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);
	}

	/* right now unused
	public static void DeleteSQLAndFile (bool dbconOpened, int uniqueID)
	{
		RunEncoder fs = (RunEncoder) Select (dbconOpened, uniqueID, -1, -1)[0];
		DeleteSQLAndFile (dbconOpened, fs);
	}
	*/
	public static void DeleteSQLAndFiles (bool dbconOpened, RunEncoder re)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "DELETE FROM " + tableStatic + " WHERE uniqueID = " + re.UniqueID;

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);

		//delete the files
		Util.FileDelete(re.FullURL);

		if(re.FullVideoURL != "")
			Util.FileDelete(re.FullVideoURL);
	}

    	//call used in most part of the program
	public static List<RunEncoder> Select (bool dbconOpened, int uniqueID, int personID, int sessionID)
	{
		return Select (dbconOpened, uniqueID, personID, sessionID,
				-1, Orders_by.ID_ASC, 0, false);
	}

	//call used on PrepareEventGraphRunEncoder
	// limit 0 means no limit (limit negative is the last results)
	public static List<RunEncoder> Select (bool dbconOpened,
			int uniqueID, int personID, int sessionID,
			int exerciseID, Orders_by order, int limit, bool personNameInComment//, bool onlyBestInSession
			)
	{
		openIfNeeded(dbconOpened);

		//for personNameInComment
		List<Person> person_l =
			SqlitePersonSession.SelectCurrentSessionPersonsAsList (true, sessionID);

		dbcmd.CommandText =
			"SELECT " + tableStatic + ".*, " + Constants.RunEncoderExerciseTable + ".Name FROM " + tableStatic + ", " + Constants.RunEncoderExerciseTable +
			selectDo (uniqueID, sessionID, personID, exerciseID, order);

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SQLiteDataReader reader;
		reader = dbcmd.ExecuteReader();

		List<RunEncoder> list = new List<RunEncoder>();
		RunEncoder re;

		while(reader.Read())
		{
			// 1. get person name in description if personNameInComment
			int personIDThisRecord = Convert.ToInt32 (reader[1].ToString());
			string description = reader[10].ToString();

			if (personNameInComment)
				foreach (Person person in person_l)
					if (person.UniqueID == personIDThisRecord)
						description = person.Name;

			re = new RunEncoder (
					Convert.ToInt32(reader[0].ToString()),	//uniqueID
					Convert.ToInt32(reader[1].ToString()),	//personID
					Convert.ToInt32(reader[2].ToString()),	//sessionID
					Convert.ToInt32(reader[3].ToString()),	//exerciseID
					(RunEncoder.Devices) Enum.Parse(
						typeof(RunEncoder.Devices), reader[4].ToString()), 	//device
					Convert.ToInt32(reader[5].ToString()),	//distance
					Convert.ToInt32(reader[6].ToString()),	//temperature
					reader[7].ToString(),			//filename
					Util.MakeURLabsolute(FixOSpath(reader[8].ToString())),	//url
					reader[9].ToString(),			//datetime
					description,
					reader[11].ToString(),			//videoURL
					Convert.ToInt32(reader[12].ToString()),	//angle
					Convert.ToInt32(reader[13].ToString()),	//totalTime
					Convert.ToDouble (Util.CDS (reader[14].ToString())), //maxSpeed
					Convert.ToDouble (Util.CDS (reader[15].ToString())), //maxAvgSpeed1s
					reader[16].ToString()			//exerciseName
					);
			list.Add(re);
		}

		reader.Close();
		closeIfNeeded(dbconOpened);

		//get last values on negative limit
		if (limit < 0 && list.Count + limit >= 0)
			list = list.GetRange (list.Count + limit, -1 * limit);

		return list;
	}

	public static List<double> Select (bool dbconOpened, string selectParam,
			int uniqueID, int personID, int sessionID,
			int exerciseID, Orders_by order, int limit)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText =
			"SELECT " + selectParam + " FROM " + tableStatic + ", " + Constants.RunEncoderExerciseTable +
			selectDo (uniqueID, sessionID, personID, exerciseID, order);

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SQLiteDataReader reader;
		reader = dbcmd.ExecuteReader();

		List<double> d_l = new List<double> ();
		while (reader.Read())
			d_l.Add (Convert.ToDouble (Util.ChangeDecimalSeparator (reader [0].ToString ())));

		reader.Close();
		closeIfNeeded(dbconOpened);

		//get last values on negative limit
		if (limit < 0 && d_l.Count + limit >= 0)
			d_l = d_l.GetRange (d_l.Count + limit, -1 * limit);

		return d_l;
	}

	private static string selectDo (int uniqueID, int sessionID, int personID, int exerciseID, Orders_by order)
	{
		string whereStr = " WHERE " + tableStatic + ".exerciseID = " + Constants.RunEncoderExerciseTable + ".UniqueID ";

		string uniqueIDStr = "";
		if(uniqueID != -1)
			uniqueIDStr = " AND " + tableStatic + ".uniqueID = " + uniqueID;

		string personIDStr = "";
		if(personID != -1)
			personIDStr = " AND " + tableStatic + ".personID = " + personID;

		string sessionIDStr = "";
		if(sessionID != -1)
			sessionIDStr = " AND " + tableStatic + ".sessionID = " + sessionID;

		string andExerciseStr = "";
		if (exerciseID != -1)
			andExerciseStr = string.Format (" AND {0}.exerciseID = {1} ", tableStatic, exerciseID);

		string orderByString = string.Format (" ORDER BY {0}.uniqueID ", tableStatic);
		if (order == Orders_by.ID_DESC)
			orderByString = string.Format(" ORDER BY {0}.uniqueID DESC ", tableStatic);
		else if (order == Orders_by.BEST)
			orderByString = string.Format ( " ORDER BY {0}.maxSpeed ", tableStatic);
		else if (order == Orders_by.BEST2)
			orderByString = string.Format ( " ORDER BY {0}.maxAvgSpeed1s ", tableStatic);

		return whereStr + uniqueIDStr + personIDStr + sessionIDStr +
			andExerciseStr + orderByString;// + limitString
	}

	protected override string selectSAArray (SQLiteDataReader reader)
	{
		return
			reader[0].ToString() + ":" +	//person.name
			reader[1].ToString() + ":" +	//uniqueID
			reader[2].ToString() + ":" +	//personID
			reader[3].ToString() + ":" +	//sessionID
			reader[4].ToString() + ":" +	//exerciseID
			reader[5].ToString() + ":" +	//device
			reader[6].ToString() + ":" +	//distance
			reader[7].ToString() + ":" +	//temperature
			reader[8].ToString() + ":" +	//filename
			reader[9].ToString() + ":" +	//url
			reader[10].ToString() + ":" +	//datetime
			reader[11].ToString() + ":" +	//comments
			reader[12].ToString() + ":" +	//videoURL
			reader[13].ToString() + ":" +	//angle
			reader[14].ToString() + ":" +	//totalTime
			Util.CDS (reader[15].ToString()) + ":" + //maxSpeed
			Util.CDS (reader[16].ToString()) + ":" + //maxAvgSpeed1s
			reader[17].ToString()		//exerciseName
			;
	}

	public static RunEncoder SelectData (int uniqueID, bool getExerciseName, bool dbconOpened)
	{
		//to manage problems at deleting and updating treeview/bars
		string [] testData = selectTestData (uniqueID, dbconOpened, tableStatic, 16);
		if (testData == null)
			return new RunEncoder ();

		RunEncoder re = new RunEncoder (testData);

		if (getExerciseName)
		{
			List<RunEncoderExercise> reex_l = SqliteRunEncoderExercise.Select (dbconOpened, re.ExerciseID);
			if (reex_l.Count > 0)
				re.ExerciseName = reex_l[0].Name;
		}

		return re;
	}

	public static ArrayList SelectRowsOfAnExercise(bool dbconOpened, int exerciseID)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "select count(*), " +
			Constants.PersonTable + ".name, " +
			Constants.SessionTable + ".name, " +
			Constants.SessionTable + ".date " +
			" FROM " + tableStatic + ", " + Constants.PersonTable + ", " + Constants.SessionTable +
			" WHERE exerciseID = " + exerciseID +
			" AND " + Constants.PersonTable + ".uniqueID = " + tableStatic + ".personID " +
		        " AND " + Constants.SessionTable + ".uniqueID = " + tableStatic + ".sessionID " +
			" GROUP BY sessionID, personID";

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		SQLiteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList array = new ArrayList();
		int count = 0;
		while(reader.Read()) {
			array.Add(new string [] {
					count.ToString(),
					reader[0].ToString(), //count
					reader[1].ToString(), //person name
					reader[2].ToString(), //session name
					reader[3].ToString()  //session date
			});
			count ++;
		}

		reader.Close();
		closeIfNeeded(dbconOpened);

		return array;
	}

	public static ArrayList SelectSessionOverviewSets (bool dbconOpened, int sessionID, bool byExercises)
	{
		if(! dbconOpened)
			Sqlite.Open();

		string byExercisesStr = "";
		if (byExercises)
			byExercisesStr = ", exerciseID";

		dbcmd.CommandText =
			"SELECT person77.uniqueID, person77.name, person77.sex, runEncoderExercise.name, COUNT(*)" +
			" FROM person77, personSession77, runEncoderExercise, runEncoder" +
			" WHERE person77.uniqueID = runEncoder.personID AND personSession77.personID = runEncoder.personID AND personSession77.sessionID = runEncoder.sessionID AND runEncoderExercise.uniqueID=runEncoder.exerciseID AND runEncoder.sessionID = " + sessionID +
			" GROUP BY runEncoder.personID" + byExercisesStr +
			" ORDER BY person77.name";

		LogB.SQL(dbcmd.CommandText.ToString());

		SQLiteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList array = new ArrayList();
		while(reader.Read())
		{
			string [] s = {
				reader[0].ToString(), 	//personID
				reader[1].ToString(), 	//person name
				reader[2].ToString(), 	//person sex
				reader[3].ToString(), 	//exercise name
				reader[4].ToString()	//sets count
			}; //note this is used on gui/genericWindow
			array.Add (s);
		}

		reader.Close();
		if(! dbconOpened)
			Sqlite.Close();

		return array;
	}

	protected internal static void import_from_1_70_to_1_71() //database is opened
	{
		//LogB.PrintAllThreads = true; //TODO: remove this
		LogB.Information("at import_from_1_70_to_1_71()");

		string raceAnalyzerDir = Util.GetRunEncoderDir();
		if(Sqlite.UpdatingDBFrom == Sqlite.UpdatingDBFromEnum.IMPORTED_SESSION)
			raceAnalyzerDir = Path.Combine(Util.GetDatabaseTempImportDir(), "raceAnalyzer");

		if(! Directory.Exists(raceAnalyzerDir))
		{
			LogB.Information("nothing to import");
			//LogB.PrintAllThreads = false; //TODO: remove this
			return;
		}

		bool importedSomething = false;
		DirectoryInfo [] sessions = new DirectoryInfo(raceAnalyzerDir).GetDirectories();
		foreach (DirectoryInfo session in sessions) //session.Name will be the UniqueID
		{
			FileInfo[] files = session.GetFiles();
			foreach (FileInfo file in files)
			{
				//in dir there are .csv and .png, take only the .csv
				if(Util.GetExtension(file.Name) != ".csv")
					continue;

				string fileWithoutExtension = Util.RemoveExtension(Util.GetLastPartOfPath(file.Name));
				RunEncoderLoadTryToAssignPersonAndComment relt =
					new RunEncoderLoadTryToAssignPersonAndComment(true, fileWithoutExtension, Convert.ToInt32(session.Name));

				Person p = relt.GetPerson();
				if(p.UniqueID == -1)
					continue;

				if(! Util.IsNumber(session.Name, false))
					continue;

				string parsedDate = UtilDate.ToFile(DateTime.MinValue);
				Match match = Regex.Match(file.Name, @"(\d+-\d+-\d+_\d+-\d+-\d+)");
				if(match.Groups.Count == 2)
					parsedDate = match.Value;

				//filename will be this
				string myFilename = p.UniqueID + "_" + p.Name + "_" + parsedDate + ".csv";
				//try to move the file
				try{
					File.Move(file.FullName, Util.GetRunEncoderSessionDir(Convert.ToInt32(session.Name)) + Path.DirectorySeparatorChar + myFilename);
				} catch {
					//if cannot, then use old filename
					myFilename = file.FullName;
				}

				int exerciseID = 0; //initial import with all exercises as 0 (because exercises are not yet defined)
				int distance = 99; //mark to know at import that this have to be changed
				int temperature = 25;
				RunEncoder runEncoder = new RunEncoder(-1, p.UniqueID, Convert.ToInt32(session.Name), exerciseID,
						RunEncoder.Devices.MANUAL, distance, temperature,
						myFilename,
						Util.MakeURLrelative(Util.GetRunEncoderSessionDir(Convert.ToInt32(session.Name))),
						parsedDate, relt.Comment,
						"", 0, 0, ""); //import without video and without name on comment

				runEncoder.InsertSQL(true);
				importedSomething = true;
			}
		}

		//need to create an exercise to assign to the imported files
		if(importedSomething)
		{
			RunEncoderExercise ex = new RunEncoderExercise(0, "Sprint", "", RunEncoderExercise.SegmentCmDefault, new List<int>(), true, 0);
			ex.InsertSQL(true);
		}

		LogB.Information("end of import_from_1_70_to_1_71()");
		//LogB.PrintAllThreads = false; //TODO: remove this
	}

}
