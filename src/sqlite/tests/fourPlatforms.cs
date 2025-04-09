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
 * Copyright (C) 2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
//using System.Data;
using System.Collections;
using System.Collections.Generic; //List<T>
//using System.IO; //DirectoryInfo
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
//using System.Text.RegularExpressions; //Regex

class SqliteFourPlatforms : SqliteTests
{
	private static string tableStatic = Constants.FourPlatformsTable;

	public SqliteFourPlatforms()
	{
		tableName = Constants.FourPlatformsTable;
		columnsStr = " (uniqueID, personID, sessionID, exerciseID, dateTime, " +
				" b0_1, b0_0, b1_1, b1_0, b2_1, b2_0, b3_1, b3_0, " +
				" comments, videoURL, totalTime)";
	}

	~SqliteFourPlatforms() {}

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
			"b0_1 TEXT, " +  //on
			"b0_0 TEXT, " +  //off
			"b1_1 TEXT, " +
			"b1_0 TEXT, " +
			"b2_1 TEXT, " +
			"b2_0 TEXT, " +
			"b3_1 TEXT, " +
			"b3_0 TEXT, " +
			"datetime TEXT, " + 	//2019-07-11_15-01-44
			"comments TEXT, " +
			"videoURL TEXT, " +	//URL of video of signals. stored as relative
			"totalTime FLOAT)"; 	//note on 1->2 1->3 1->4 is the time from 1st 1 off that goes to a 2,3 or 4. to the last arrival to 2,3,4. There could be more events before and after
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}


        /*
         * sID -1 means all sessions
         * pID -1 means all persons
         * type "" means all types
	 * limit 0 means no limit (limit negative is the last results)
         * personNameInComment is used to be able to display names in graphs
         * because event.PersonName makes individual SQL SELECTs
         */

	public static List<FourPlatforms> Select (bool dbconOpened, int sessionID, int personID,
			//string type,
			Orders_by order, int limit, bool personNameInComment//, bool onlyBestInSession
			)
	{
		openIfNeeded(dbconOpened);

		//for personNameInComment
		List<Person> person_l =
			SqlitePersonSession.SelectCurrentSessionPersonsAsList (true, sessionID);

		dbcmd.CommandText = selectResultsCreateSelection (
				tableStatic,
				sessionID, personID, "", //type,
				order, limit, false //onlyBestInSession
				);
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SQLiteDataReader reader;
		reader = dbcmd.ExecuteReader();
		List<FourPlatforms> fp_l = new List<FourPlatforms>();

		while(reader.Read())
		{
			// 1. get person name in description if personNameInComment
			int personIDThisRecord = Convert.ToInt32(reader[2].ToString());
			string description = reader[14].ToString();

			if (personNameInComment)
				foreach (Person person in person_l)
					if (person.UniqueID == personIDThisRecord)
						description = person.Name;

			// 2. create object
			FourPlatforms fp = new FourPlatforms(
					Convert.ToInt32(reader[1].ToString()),	//uniqueID
					personIDThisRecord,		 	//personID
					Convert.ToInt32(reader[3].ToString()), 	//sessionID
					Convert.ToInt32(reader[4].ToString()), 	//type
					UtilList.SQLStringToListDouble (Util.CDSNoZero (reader[5].ToString()), "="), //b0_1
					UtilList.SQLStringToListDouble (Util.CDSNoZero (reader[6].ToString()), "="), //b0_0
					UtilList.SQLStringToListDouble (Util.CDSNoZero (reader[7].ToString()), "="), //b1_1
					UtilList.SQLStringToListDouble (Util.CDSNoZero (reader[8].ToString()), "="), //b1_0
					UtilList.SQLStringToListDouble (Util.CDSNoZero (reader[9].ToString()), "="), //b2_1
					UtilList.SQLStringToListDouble (Util.CDSNoZero (reader[10].ToString()), "="), //b2_0
					UtilList.SQLStringToListDouble (Util.CDSNoZero (reader[11].ToString()), "="), //b3_1
					UtilList.SQLStringToListDouble (Util.CDSNoZero (reader[12].ToString()), "="), //b3_0
					reader[13].ToString(), 	//datetime
					description,
					reader[15].ToString(),	//videoURL
					Convert.ToDouble (Util.CDS (reader[16].ToString()))	//totalTime
					);

			fp_l.Add (fp);
		}

		reader.Close();
		closeIfNeeded(dbconOpened);

		//get last values on negative limit
		if (limit < 0 && fp_l.Count + limit >= 0)
			fp_l = fp_l.GetRange (fp_l.Count + limit, -1 * limit);

		return fp_l;
	}

	protected override string selectSAArray (SQLiteDataReader reader)
	{
		return
			reader[0].ToString() + ":" + 	//person.name
			reader[1].ToString() + ":" +	//fourPlatforms.uniqueID
			reader[2].ToString() + ":" + 	//fourPlatforms.personID
			reader[3].ToString() + ":" + 	//fourPlatforms.sessionID
			reader[4].ToString() + ":" + 	//fourPlatforms.type
			Util.CDSNoZero (reader[5].ToString()) + ":" + 	//fourPlatforms.b0_1
			Util.CDSNoZero (reader[6].ToString()) + ":" + 	//fourPlatforms.b0_0
			Util.CDSNoZero (reader[7].ToString()) + ":" + 	//fourPlatforms.b1_1
			Util.CDSNoZero (reader[8].ToString()) + ":" + 	//fourPlatforms.b1_0
			Util.CDSNoZero (reader[9].ToString()) + ":" + 	//fourPlatforms.b2_1
			Util.CDSNoZero (reader[10].ToString()) + ":" + 	//fourPlatforms.b2_0
			Util.CDSNoZero (reader[11].ToString()) + ":" + 	//fourPlatforms.b3_1
			Util.CDSNoZero (reader[12].ToString()) + ":" + 	//fourPlatforms.b3_0
			reader[13].ToString() + ":" + 	//datetime
			reader[14].ToString() + ":" + 	//comments
			reader[15].ToString() + ":" +	//videoURL
			Util.CDS (reader[16].ToString());	 	//totalTime
	}

	public static FourPlatforms SelectData (int uniqueID, bool dbconOpened)
	{
		return new FourPlatforms (selectTestData (uniqueID, dbconOpened, tableStatic, 16));
	}

	//this method is here to have a createTable that does not change in future versions
	protected internal static void createTable_fourPlatforms_db_2_58_migration
		(SQLiteCommand mycmd, string migrateToTable) //needed for migration from 2_57 to 2_58
		{
			mycmd.CommandText =
				"DROP TABLE IF EXISTS '" + migrateToTable +
				"'; CREATE TABLE '" + migrateToTable + "' ( " +
				"uniqueID INTEGER PRIMARY KEY, " +
				"personID INT, " +
				"sessionID INT, " +
				"exerciseID INT, " + //right now all will be exercise 0, until we have a clear idea of what exercises could be done and how can affect measurements
				"b0_1 TEXT, " +  //on
				"b0_0 TEXT, " +  //off
				"b1_1 TEXT, " +
				"b1_0 TEXT, " +
				"b2_1 TEXT, " +
				"b2_0 TEXT, " +
				"b3_1 TEXT, " +
				"b3_0 TEXT, " +
				"datetime TEXT, " + 	//2019-07-11_15-01-44
				"comments TEXT, " +
				"videoURL TEXT, " +	//URL of video of signals. stored as relative
				"totalTime FLOAT)";	//needed to sync with video. If we press finish when there are no pulsees we cannot sync. If we use totalTime we can sync.
		LogB.SQL(mycmd.CommandText.ToString());
		mycmd.ExecuteNonQuery();
		}
}

//using fourPlatforms to store simple jumps
class SqliteFourPlatformsJumpsSimple : Sqlite
{
	private static string table = Constants.JumpTable;
	private bool hasFall;

	public SqliteFourPlatformsJumpsSimple (bool hasFall)
	{
		this.hasFall = hasFall;
	}

	//public int Insert (int personID, int sessionID,
	public List<Jump> Insert (List<IDName> person_l, int sessionID,
			string jumpType, List<List<double>> off_ll, List<List<double>> on_ll, double firstFall,
			double weight, string description, int angle, bool simulated,
			string datetimeStr)
	{
		Sqlite.Open();
		List<Jump> jump_l = new List<Jump> ();

		using(SQLiteTransaction tr = dbcon.BeginTransaction())
		{
			using (SQLiteCommand dbcmdTr = dbcon.CreateCommand())
			{
				dbcmdTr.Transaction = tr;
				for (int p = 0; p < 4; p ++)
				{
					if (person_l[p].UniqueID < 0)
						continue;

					double tf = 0;
					double tc = 0;
					double fall = 0;

					bool firstJump = true; //to use firstFall
					double thisOnTime = 0;
					double lastOnTime = 0;
					double tfLast = 0; //used to calculate fall

					for (int j = 0; j < on_ll[p].Count; j ++)
					{
						//LogB.Information (string.Format ("on {0}, onTime {1} ", j, on_ll[p][j]));
						bool found = false;
						int k = 0;
						for (k = off_ll[p].Count -1; k >= 0; k --)
						{
							//LogB.Information (string.Format ("off {0}, offTime {1} ", k, off_ll[p][k]));
							if (off_ll[p][k] < on_ll[p][j])
							{
								//LogB.Information ("found!");
								tf = on_ll[p][j] - off_ll[p][k];
								thisOnTime = on_ll[p][j];

								found = true;
								break;
							}
						}

						if (found && hasFall)
						{
							if (firstJump)
							{
								firstJump = false;

								//do not accept a jump with tc when started in
								if (off_ll[p][0] < on_ll[p][0])
								{
									lastOnTime = thisOnTime;
									tfLast = tf;
									continue;
								}

								fall = firstFall;
							} else
								fall = Util.GetHeightInCentimeters (tfLast);

							tc = off_ll[p][k] - lastOnTime;
						}

						if (found)
						{
							Jump jump = new Jump (
									-1, person_l[p].UniqueID, sessionID, jumpType,
									tf, tc, fall, weight, "", -1, 0, datetimeStr);
							jump.UniqueID = jump.InsertAtDB (true, table, dbcmdTr);
							jump_l.Add (jump);

							lastOnTime = thisOnTime;
							tfLast = tf;
						}
					}
				}
			}
			tr.Commit();
		}
		Sqlite.Close();
		return jump_l;
	}

	~SqliteFourPlatformsJumpsSimple() {}
}
