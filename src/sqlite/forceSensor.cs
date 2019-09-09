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
 * Copyright (C) 2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
//using System.Data;
using System.Collections;
using System.Collections.Generic; //List<T>
using System.IO; //DirectoryInfo
using Mono.Data.Sqlite;
using System.Text.RegularExpressions; //Regex
using Mono.Unix;

class SqliteForceSensor : Sqlite
{
	private static string table = Constants.ForceSensorTable;

	public SqliteForceSensor() {
	}

	~SqliteForceSensor() {}

	/*
	 * create and initialize tables
	 */

	protected internal static void createTable()
	{
		dbcmd.CommandText =
			"CREATE TABLE " + table + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"personID INT, " +
			"sessionID INT, " +
			"exerciseID INT, " +
			"angle INT, " + 	//angle can be different than the defaultAngle on exercise
			"laterality TEXT, " +	//"Both" "Right" "Left". stored in english
			"filename TEXT, " +
			"url TEXT, " +		//URL of data files. stored as relative
			"datetime TEXT, " + 	//2019-07-11_15-01-44
			"comments TEXT, " +
			"videoURL TEXT)";	//URL of video of signals. stored as relative
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}

	public static void Insert (bool dbconOpened, string insertString)
	{
		if(! dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "INSERT INTO " + table +
				" (uniqueID, personID, sessionID, exerciseID, angle, laterality, filename, url, dateTime, comments, videoURL)" +
				" VALUES " + insertString;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery(); //TODO uncomment this again

		if(! dbconOpened)
			Sqlite.Close();
	}

	//SELECT forceSensor.*, forceSensorExercise.Name FROM forceSensor, forceSensorExercise WHERE forceSensor.exerciseID = forceSensorExercise.UniqueID ORDER BY forceSensor.uniqueID;
	public static ArrayList Select (bool dbconOpened, int uniqueID, int personID, int sessionID)
	{
		if(! dbconOpened)
			Sqlite.Open();

		string selectStr = "SELECT " + table + ".*, " + Constants.ForceSensorExerciseTable + ".Name FROM " + table + ", " + Constants.ForceSensorExerciseTable;
		string whereStr = " WHERE " + table + ".exerciseID = " + Constants.ForceSensorExerciseTable + ".UniqueID ";

		string uniqueIDStr = "";
		if(uniqueID != -1)
			uniqueIDStr = " AND " + table + ".uniqueID = " + uniqueID;

		string personIDStr = "";
		if(personID != -1)
			personIDStr = " AND " + table + ".personID = " + personID;

		string sessionIDStr = "";
		if(sessionID != -1)
			sessionIDStr = " AND " + table + ".sessionID = " + sessionID;

		dbcmd.CommandText = selectStr + whereStr + uniqueIDStr + personIDStr + sessionIDStr + " Order BY " + table + ".uniqueID";

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList array = new ArrayList(1);
		ForceSensor fs;

		while(reader.Read()) {
			fs = new ForceSensor (
					Convert.ToInt32(reader[0].ToString()),	//uniqueID
					Convert.ToInt32(reader[1].ToString()),	//personID
					Convert.ToInt32(reader[2].ToString()),	//sessionID
					Convert.ToInt32(reader[3].ToString()),	//exerciseID
					Convert.ToInt32(reader[4].ToString()),	//angle
					reader[5].ToString(),			//laterality
					reader[6].ToString(),			//filename
					reader[7].ToString(),			//url
					reader[8].ToString(),			//datetime
					reader[9].ToString(),			//comments
					reader[10].ToString(),			//videoURL
					reader[11].ToString()			//exerciseName
					);
			array.Add(fs);
		}

		reader.Close();
		if(! dbconOpened)
			Sqlite.Close();

		return array;
	}

	protected internal static void import_from_1_68_to_1_69() //database is opened
	{
		LogB.PrintAllThreads = true; //TODO: remove this
		LogB.Information("at import_from_1_68_to_1_69()");
		//LogB.Information("Sqlite isOpened: " + Sqlite.IsOpened.ToString());

		string forceSensorDir = Util.GetForceSensorDir();

		int unknownExerciseID = Sqlite.ExistsAndGetUniqueID(true, Constants.ForceSensorExerciseTable, Catalog.GetString("Unknown"));

		DirectoryInfo [] sessions = new DirectoryInfo(forceSensorDir).GetDirectories();
		foreach (DirectoryInfo session in sessions) //session.Name will be the UniqueID
		{
			FileInfo[] files = session.GetFiles();
			foreach (FileInfo file in files)
			{
				string fileWithoutExtension = Util.RemoveExtension(Util.GetLastPartOfPath(file.Name));
				ForceSensorLoadTryToAssignPersonAndMore fslt =
					new ForceSensorLoadTryToAssignPersonAndMore(true, fileWithoutExtension, Convert.ToInt32(session.Name));
				//TODO: no se si session.ToString() és la manera de saber el nom del DirectoryInfo

				Person p = fslt.GetPerson();
				if(p.UniqueID == -1)
					continue;

				if(! Util.IsNumber(session.Name, false))
					continue;

				//at the beginning exercise was not written on the filename, because force sensor started without exercises on sql
				//"person name_2017-11-11_19-35-55.csv"
				//if cannot found exercise, assign to Unknown
				int exerciseID = -1;
				string exerciseName = fslt.Exercise;
				if(fslt.Exercise != "")
					exerciseID = ExistsAndGetUniqueID(true, Constants.ForceSensorExerciseTable, fslt.Exercise);

				if(fslt.Exercise == "" || exerciseID == -1)
				{
					if(unknownExerciseID == -1)
						unknownExerciseID = SqliteForceSensorExercise.Insert (true, -1, Catalog.GetString("Unknown"), 0, "", 0, "", false);

					exerciseID = unknownExerciseID;
					exerciseName = Catalog.GetString("Unknown");
				}

				//laterality (in English)
				string lat = fslt.Laterality;
				if(lat == Catalog.GetString(Constants.ForceSensorLateralityRight))
					lat = Constants.ForceSensorLateralityRight;
				else if(lat == Catalog.GetString(Constants.ForceSensorLateralityLeft))
					lat = Constants.ForceSensorLateralityLeft;
				else
					lat = Constants.ForceSensorLateralityBoth;

				string parsedDate = UtilDate.ToFile(DateTime.MinValue);
				Match match = Regex.Match(file.Name, @"(\d+-\d+-\d+_\d+-\d+-\d+)");
				if(match.Groups.Count == 2)
					parsedDate = match.Value;

				//filename will be this
				string myFilename = p.UniqueID + "_" + p.Name + "_" + parsedDate;
				//try to move the file
				try{
					File.Move(file.FullName, Util.GetForceSensorSessionDir(Convert.ToInt32(session.Name)) + Path.DirectorySeparatorChar + myFilename);
				} catch {
					//if cannot, then use old filename
					myFilename = file.FullName;
				}

				ForceSensor forceSensor = new ForceSensor(-1, p.UniqueID, Convert.ToInt32(session.Name), exerciseID,
						ForceSensor.AngleUndefined, lat,
						myFilename,
						Util.MakeURLrelative(Util.GetForceSensorSessionDir(Convert.ToInt32(session.Name))),
						parsedDate, fslt.Comment, "", exerciseName);
				forceSensor.InsertSQL(true);
			}
		}

		LogB.PrintAllThreads = false; //TODO: remove this
	}
}

class SqliteForceSensorExercise : Sqlite
{
	private static string table = Constants.ForceSensorExerciseTable;

	public SqliteForceSensorExercise() {
	}

	~SqliteForceSensorExercise() {}

	/*
	 * create and initialize tables
	 */

	protected internal static void createTable()
	{
		dbcmd.CommandText =
			"CREATE TABLE " + table + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"name TEXT, " +
			"percentBodyWeight INT, " +
			"resistance TEXT, " +
			"angleDefault INT, " +
			"description TEXT, " +
			"tareBeforeCapture INT)";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}

	//undefined defaultAngle will be 1000
	//note execution can have a different angle than the default angle
	public static int Insert (bool dbconOpened, int uniqueID, string name, int percentBodyWeight,
			string resistance, int angleDefault, string description, bool tareBeforeCapture)
	{
		if(! dbconOpened)
			Sqlite.Open();

		string uniqueIDStr = "NULL";
		if(uniqueID != -1)
			uniqueIDStr = uniqueID.ToString();

		dbcmd.CommandText = "INSERT INTO " + table +
				" (uniqueID, name, percentBodyWeight, resistance, angleDefault, description, tareBeforeCapture)" +
				" VALUES (" + uniqueIDStr + ", \"" + name + "\", " + percentBodyWeight + ", \"" +
				resistance + "\", " + angleDefault + ", \"" + description + "\", " +
				Util.BoolToInt(tareBeforeCapture).ToString() + ")";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		string myString = @"select last_insert_rowid()";
		dbcmd.CommandText = myString;
		int myLast = Convert.ToInt32(dbcmd.ExecuteScalar()); // Need to type-cast since `ExecuteScalar` returns an object.

		if(! dbconOpened)
			Sqlite.Close();

		return myLast;
	}

	public static void Update (bool dbconOpened, ForceSensorExercise ex)
	{
		if(! dbconOpened)
			Sqlite.Open();

		/*
		   string uniqueIDStr = "NULL";
		   if(ex.UniqueID != -1)
			   uniqueIDStr = ex.UniqueID.ToString();
		   */

		dbcmd.CommandText = "UPDATE " + table + " SET " +
			" name = \"" + ex.Name +
			"\", percentBodyWeight = " + ex.PercentBodyWeight +
			", resistance = \"" + ex.Resistance +
			"\", angleDefault = " + ex.AngleDefault +
			", description = \"" + ex.Description +
			"\", tareBeforeCapture = " + Util.BoolToInt(ex.TareBeforeCapture).ToString() +
			" WHERE uniqueID = " + ex.UniqueID;

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		if(! dbconOpened)
			Sqlite.Close();
	}

	public static ArrayList Select (bool dbconOpened, int uniqueID, bool onlyNames)
	{
		if(! dbconOpened)
			Sqlite.Open();

		string uniqueIDStr = "";
		if(uniqueID != -1)
			uniqueIDStr = " WHERE " + table + ".uniqueID = " + uniqueID;

		if(onlyNames)
			dbcmd.CommandText = "SELECT name FROM " + table + uniqueIDStr;
		else
			dbcmd.CommandText = "SELECT * FROM " + table + uniqueIDStr;

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList array = new ArrayList(1);
		ForceSensorExercise ex = new ForceSensorExercise();

		if(onlyNames) {
			while(reader.Read()) {
				ex = new ForceSensorExercise (reader[0].ToString());
				array.Add(ex);
			}
		} else {
			while(reader.Read()) {
				int angleDefault = 0;

				ex = new ForceSensorExercise (
						Convert.ToInt32(reader[0].ToString()),	//uniqueID
						reader[1].ToString(),			//name
						Convert.ToInt32(reader[2].ToString()),	//percentBodyWeight
						reader[3].ToString(),			//resistance
						angleDefault,
						reader[5].ToString(),			//description
						Util.IntToBool(Convert.ToInt32(reader[6].ToString()))	//tareBeforeCapture
						);
				array.Add(ex);
			}
		}

		reader.Close();
		if(! dbconOpened)
			Sqlite.Close();

		return array;
	}

}


class SqliteForceSensorRFD : Sqlite
{
	private static string table = Constants.ForceRFDTable;

	public SqliteForceSensorRFD() {
	}

	~SqliteForceSensorRFD() {}

	/*
	 * create and initialize tables
	 */

	protected internal static void createTable()
	{
		dbcmd.CommandText = 
			"CREATE TABLE " + table + " ( " +
			"code TEXT, " + 	//RFD1...4, I (Impulse)
			"active INT, " + 	//bool
			"function TEXT, " +
			"type TEXT, " +
			"num1 INT, " +
			"num2 INT )";
		dbcmd.ExecuteNonQuery();
	}

	public static void InsertDefaultValues(bool dbconOpened)
	{
		openIfNeeded(dbconOpened);

		Insert(true, new ForceSensorRFD("RFD1", true,
					ForceSensorRFD.Functions.FITTED, ForceSensorRFD.Types.INSTANTANEOUS, 0, -1));
		Insert(true, new ForceSensorRFD("RFD2", true,
					ForceSensorRFD.Functions.RAW, ForceSensorRFD.Types.AVERAGE, 0, 100));
		Insert(true, new ForceSensorRFD("RFD3", false,
					ForceSensorRFD.Functions.FITTED, ForceSensorRFD.Types.PERCENT_F_MAX, 50, -1));
		Insert(true, new ForceSensorRFD("RFD4", false,
					ForceSensorRFD.Functions.RAW, ForceSensorRFD.Types.RFD_MAX, -1, -1));

		InsertDefaultValueImpulse(true);

		closeIfNeeded(dbconOpened);
	}

	public static void InsertDefaultValueImpulse(bool dbconOpened)
	{
		openIfNeeded(dbconOpened);

		Insert(true, new ForceSensorImpulse(true,
					ForceSensorImpulse.Functions.RAW, ForceSensorImpulse.Types.IMP_RANGE, 0, 500));

		closeIfNeeded(dbconOpened);
	}

	public static void Insert(bool dbconOpened, ForceSensorRFD rfd)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "INSERT INTO " + table +
			" (code, active, function, type, num1, num2) VALUES (" + rfd.ToSQLInsertString() + ")";

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);
	}
	public static void InsertImpulse(bool dbconOpened, ForceSensorImpulse impulse)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "INSERT INTO " + table +
			" (code, active, function, type, num1, num2) VALUES (" + impulse.ToSQLInsertString() + ")";

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);
	}


	public static void Update(bool dbconOpened, ForceSensorRFD rfd)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "UPDATE " + table + " SET " +
			" active = " + Util.BoolToInt(rfd.active).ToString() + "," +
			" function = \"" + rfd.function.ToString() + "\"" + "," +
			" type = \"" + rfd.type.ToString() + "\"" + "," +
			" num1 = " + rfd.num1.ToString() + "," +
			" num2 = " + rfd.num2.ToString() +
			" WHERE code = \"" + rfd.code + "\"";

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);
	}
	public static void UpdateImpulse(bool dbconOpened, ForceSensorImpulse impulse)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "UPDATE " + table + " SET " +
			" active = " + Util.BoolToInt(impulse.active).ToString() + "," +
			" function = \"" + impulse.function.ToString() + "\"" + "," +
			" type = \"" + impulse.type.ToString() + "\"" + "," +
			" num1 = " + impulse.num1.ToString() + "," +
			" num2 = " + impulse.num2.ToString() +
			" WHERE code = \"" + impulse.code + "\"";

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);
	}

	//used when button_force_rfd_default is clicked
	public static void DeleteAll(bool dbconOpened)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "DELETE FROM " + table;

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);
	}

	public static List<ForceSensorRFD> SelectAll (bool dbconOpened)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "SELECT * FROM " + table + " WHERE code != \"I\"";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader = dbcmd.ExecuteReader();

		List<ForceSensorRFD> l = new List<ForceSensorRFD>();
		while(reader.Read()) {
			ForceSensorRFD rfd = new ForceSensorRFD(
					reader[0].ToString(), 				//code
					Util.IntToBool(Convert.ToInt32(reader[1])), 	//active
					(ForceSensorRFD.Functions) Enum.Parse(
						typeof(ForceSensorRFD.Functions), reader[2].ToString()), 	//function
					(ForceSensorRFD.Types) Enum.Parse(
						typeof(ForceSensorRFD.Types), reader[3].ToString()), 	//type
					Convert.ToInt32(reader[4]), 			//num1
					Convert.ToInt32(reader[5]) 			//num2
					);
			l.Add(rfd);
		}

		reader.Close();
		closeIfNeeded(dbconOpened);

		return l;
	}

	public static ForceSensorImpulse SelectImpulse (bool dbconOpened)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "SELECT * FROM " + table + " WHERE code == \"I\"";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader = dbcmd.ExecuteReader();

		ForceSensorImpulse impulse = null;
		while(reader.Read()) {
			impulse = new ForceSensorImpulse(
					Util.IntToBool(Convert.ToInt32(reader[1])), 	//active
					(ForceSensorImpulse.Functions) Enum.Parse(
						typeof(ForceSensorImpulse.Functions), reader[2].ToString()), 	//function
					(ForceSensorImpulse.Types) Enum.Parse(
						typeof(ForceSensorImpulse.Types), reader[3].ToString()), //type
					Convert.ToInt32(reader[4]), 			//num1
					Convert.ToInt32(reader[5]) 			//num2
					);
		}

		reader.Close();
		closeIfNeeded(dbconOpened);

		return impulse;
	}
}
