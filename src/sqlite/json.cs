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
 * Copyright (C) 2018   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Collections.Generic; //List<T>
using Mono.Data.Sqlite;

class SqliteJson : Sqlite
{
	private static string tableEncoder = Constants.UploadEncoderDataTempTable;
	private static string tableSprint = Constants.UploadSprintDataTempTable;
	private static string tableExhibitionTest = Constants.UploadExhibitionTestTempTable;

	public SqliteJson() {
	}

	~SqliteJson() {}

	/*
	 * create and initialize tables
	 */

	/*
	 * ENCODER
	 */	

	protected internal static void createTableUploadEncoderDataTemp()
	{
		dbcmd.CommandText = 
			"CREATE TABLE " + tableEncoder + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"personId INT, " +
			"stationId INT, " +
			"exerciseId INT, " +
			"laterality TEXT, " +
			"resistance TEXT, " +
			"repetitions INT, " +
			//BySpeed stuff
			"numBySpeed INT, " +
			"lossBySpeed INT, " +
			"rangeBySpeed TEXT, " +
			"vmeanBySpeed TEXT, " +
			"vmaxBySpeed TEXT, " +
			"pmeanBySpeed TEXT, " +
			"pmaxBySpeed TEXT, " +
			//ByPower stuff
			"numByPower INT, " +
			"lossByPower INT, " +
			"rangeByPower TEXT, " +
			"vmeanByPower TEXT, " +
			"vmaxByPower TEXT, " +
			"pmeanByPower TEXT, " +
			"pmaxByPower TEXT )";
		dbcmd.ExecuteNonQuery();
	}

	public static void InsertTempEncoder (bool dbconOpened, UploadEncoderDataFullObject o)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "INSERT INTO " + tableEncoder + 
			" (uniqueID, personID, stationID, exerciseID, laterality, resistance, " +
			"repetitions, " +
			"numBySpeed, lossBySpeed, rangeBySpeed, vmeanBySpeed, vmaxBySpeed, pmeanBySpeed, pmaxBySpeed, " +
			"numByPower, lossByPower, rangeByPower, vmeanByPower, vmaxByPower, pmeanByPower, pmaxByPower) " +
			" VALUES (" +
			o.ToSQLInsertString();
		LogB.SQL(dbcmd.CommandText.ToString());

		dbcmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);
	}

	public static List<UploadEncoderDataFullObject> SelectTempEncoder(bool dbconOpened)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "SELECT * FROM " + tableEncoder + " ORDER BY uniqueID";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader = dbcmd.ExecuteReader();

		List<UploadEncoderDataFullObject> l = new List<UploadEncoderDataFullObject>();
		while(reader.Read())
		{
			UploadEncoderDataObject uo = new UploadEncoderDataObject(
					Convert.ToInt32(reader[6]), 		//repetitions
					Convert.ToInt32(reader[7]), 		//numBySpeed
					Convert.ToInt32(reader[8]), 		//lossBySpeed
					reader[9].ToString(), 			//rangeBySpeed
					reader[10].ToString(), 			//vmeanBySpeed
					reader[11].ToString(), 			//vmaxBySpeed
					reader[12].ToString(), 			//pmeanBySpeed
					reader[13].ToString(), 			//pmaxBySpeed
					Convert.ToInt32(reader[14]), 		//numByPower
					Convert.ToInt32(reader[15]), 		//lossByPower
					reader[16].ToString(), 			//rangeByPower
					reader[17].ToString(), 			//vmeanByPower
					reader[18].ToString(), 			//vmaxByPower
					reader[19].ToString(), 			//pmeanByPower
					reader[20].ToString() 			//pmaxByPower
						);

			UploadEncoderDataFullObject o = new UploadEncoderDataFullObject(
					Convert.ToInt32(reader[0]), 		//uniqueID
					Convert.ToInt32(reader[1]), 		//personID
					Convert.ToInt32(reader[2]), 		//stationID
					Convert.ToInt32(reader[3]), 		//exerciseID
					reader[4].ToString(), 			//laterality
					reader[5].ToString(), 			//resistance
					uo);

			l.Add(o);
		}

		reader.Close();
		closeIfNeeded(dbconOpened);

		return l;
	}

	public static void DeleteTempEncoder(bool dbconOpened, int uniqueID)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "Delete FROM " + tableEncoder + " WHERE uniqueID = " + uniqueID;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);
	}


	/*
	 * SPRINT
	 */	

	protected internal static void createTableUploadSprintDataTemp()
	{
		dbcmd.CommandText = 
			"CREATE TABLE " + tableSprint + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"personId INT, " +
			"sprintPositions TEXT, " +
			"splitTimes TEXT, " +
			"k FLOAT, " +
			"vmax FLOAT, " +
			"amax FLOAT, " +
			"fmax FLOAT, " +
			"pmax FLOAT )";
		dbcmd.ExecuteNonQuery();
	}

	public static void InsertTempSprint (bool dbconOpened, UploadSprintDataObject o)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "INSERT INTO " + tableSprint + 
			" (uniqueID, personID, sprintPositions, splitTimes, " +
			" k, vmax, amax, fmax, pmax) VALUES (" +
			o.ToSQLInsertString();
		LogB.SQL(dbcmd.CommandText.ToString());

		dbcmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);
	}

	public static List<UploadSprintDataObject> SelectTempSprint (bool dbconOpened)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "SELECT * FROM " + tableSprint + " ORDER BY uniqueID";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader = dbcmd.ExecuteReader();

		List<UploadSprintDataObject> l = new List<UploadSprintDataObject>();
		while(reader.Read())
		{
			UploadSprintDataObject o = new UploadSprintDataObject(
					Convert.ToInt32(reader[0]), 		//uniqueID
					Convert.ToInt32(reader[1]), 		//personID
					reader[2].ToString(), 				//sprintPositions
					UploadSprintDataObject.SplitTimesStringToList(reader[3].ToString()), //splitTimes
					Convert.ToDouble(Util.ChangeDecimalSeparator(reader[4].ToString())), //k
					Convert.ToDouble(Util.ChangeDecimalSeparator(reader[5].ToString())), //vmax
					Convert.ToDouble(Util.ChangeDecimalSeparator(reader[6].ToString())), //amax
					Convert.ToDouble(Util.ChangeDecimalSeparator(reader[7].ToString())), //fmax
					Convert.ToDouble(Util.ChangeDecimalSeparator(reader[8].ToString())) //pmax
					);

			l.Add(o);
		}

		reader.Close();
		closeIfNeeded(dbconOpened);

		return l;
	}

	public static void DeleteTempSprint(bool dbconOpened, int uniqueID)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "Delete FROM " + tableSprint + " WHERE uniqueID = " + uniqueID;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);
	}

	/*
	 * EXHIBITION //right now does not upload to server when connection returns
	 */

	public static void UploadExhibitionTest(ExhibitionTest et)
	{
		JsonExhibitions js = new JsonExhibitions();
		if( ! js.UploadExhibitionTest (et))
		{
			LogB.Error(js.ResultMessage);
			SqliteJson.InsertTempExhibitionTest(false, et); //insert only if could'nt be uploaded
		}
	}

	//At YOMO free network without transaction each record needs 0.9 seconds
	//At YOMO free network with transaction each record needs 0.3 seconds
	public static void UploadExhibitionTestsPending()
	{
		JsonExhibitions json = new JsonExhibitions();
		Sqlite.Open(); // ---------------->

		int countSucceded = 0;
		List<ExhibitionTest> listEtTemp = SqliteJson.SelectTempExhibitionTest(true);
		if(listEtTemp.Count > 0)
		{
			LogB.Information("Starting to upload {0} exhibitionTests...");
			using(SqliteTransaction tr = dbcon.BeginTransaction())
			{
				using (SqliteCommand dbcmdTr = dbcon.CreateCommand())
				{
					dbcmdTr.Transaction = tr;

					foreach(ExhibitionTest et in listEtTemp)
					{
						bool success = json.UploadExhibitionTest(et);
						//LogB.Information(json.ResultMessage);
						if(success) {
							countSucceded ++;
							SqliteJson.DeleteTempExhibitionTest(true, et, dbcmdTr); //delete the record
						}
					}
				}
				tr.Commit();
			}
			LogB.Information(string.Format("Done! succeded {0}/{1}", countSucceded, listEtTemp.Count));
		}
		Sqlite.Close(); // <----------------
	}

	protected internal static void createTableUploadExhibitionTestTemp()
	{
		dbcmd.CommandText =
			"CREATE TABLE " + tableExhibitionTest + " (" +
			"schoolID INT, " +
			"groupID INT, " +
			"personID INT, " +
			"testType TEXT, " +
			"result DOUBLE)";
		dbcmd.ExecuteNonQuery();
	}

	public static void InsertTempExhibitionTest (bool dbconOpened, ExhibitionTest et)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "INSERT INTO " + tableExhibitionTest +
			" (schoolID, groupID, personID, testType, result) VALUES (" +
			et.ToSQLTempInsertString() + ")";
		LogB.SQL(dbcmd.CommandText.ToString());

		dbcmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);
	}

	public static List<ExhibitionTest> SelectTempExhibitionTest (bool dbconOpened)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "SELECT * FROM " + tableExhibitionTest + " ORDER BY personID";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader = dbcmd.ExecuteReader();

		List<ExhibitionTest> l = new List<ExhibitionTest>();
		while(reader.Read())
		{
			ExhibitionTest et = new ExhibitionTest(
					Convert.ToInt32(reader[0]), 		//schoolID
					Convert.ToInt32(reader[1]), 		//groupID
					Convert.ToInt32(reader[2]), 		//personID
					(ExhibitionTest.testTypes) Enum.Parse(
						typeof(ExhibitionTest.testTypes), reader[3].ToString()),    //testType
					Convert.ToDouble(Util.ChangeDecimalSeparator(reader[4].ToString())) //result
					);

			l.Add(et);
		}

		reader.Close();
		closeIfNeeded(dbconOpened);

		return l;
	}

	public static void DeleteTempExhibitionTest(bool dbconOpened, ExhibitionTest et)
	{
		DeleteTempExhibitionTest(dbconOpened, et, dbcmd);
	}
	public static void DeleteTempExhibitionTest(bool dbconOpened, ExhibitionTest et, SqliteCommand mycmd)
	{
		openIfNeeded(dbconOpened);

		mycmd.CommandText = "Delete FROM " + tableExhibitionTest + " WHERE " +
			"schoolID = " + et.schoolID + " AND " +
			"groupID = " + et.groupID + " AND " +
			"personID = " + et.personID + " AND " +
			"testType = \"" + et.testType.ToString() + "\" AND " +
			"result = " + et.resultToJson;
		//LogB.SQL(mycmd.CommandText.ToString());
		mycmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);
	}

}

