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
 * Copyright (C) 2017-2022   Xavier de Blas <xaviblas@gmail.com>
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

	protected internal static new void createTable()
	{
		dbcmd.CommandText =
			"CREATE TABLE " + table + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"personID INT, " +
			"sessionID INT, " +
			"exerciseID INT, " +
			"captureOption TEXT, " + //ForceSensor.CaptureOptions {NORMAL, ABS, INVERTED}
			"angle INT, " + 	//angle can be different than the defaultAngle on exercise
			"laterality TEXT, " +	//"Both" "Right" "Left". stored in english
			"filename TEXT, " +
			"url TEXT, " +		//URL of data files. stored as relative
			"datetime TEXT, " + 	//2019-07-11_15-01-44
			"comments TEXT, " +
			"videoURL TEXT, " +	//URL of video of signals. stored as relative
			"stiffness FLOAT DEFAULT -1, " +	//this is the important, next one is needed for recalculate, but note that some bands can have changed or being deleted
			"stiffnessString TEXT, " + //uniqueID*active of ElasticBand separated by ';' or empty if exerciseID ! elastic
			"maxForceRaw FLOAT, " +
			"maxAvgForce1s FLOAT)";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}

	public static int Insert (bool dbconOpened, string insertString)
	{
		openIfNeeded(dbconOpened);

		LogB.Information("goint to insert: " + insertString);
		dbcmd.CommandText = "INSERT INTO " + table +
				" (uniqueID, personID, sessionID, exerciseID, captureOption, angle, laterality," +
				" filename, url, dateTime, comments, videoURL, stiffness, stiffnessString," +
				" maxForceRaw, maxAvgForce1s)" +
				" VALUES " + insertString;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		string myString = @"select last_insert_rowid()";
		dbcmd.CommandText = myString;
		int myLast = Convert.ToInt32(dbcmd.ExecuteScalar()); // Need to type-cast since `ExecuteScalar` returns an object.

		closeIfNeeded(dbconOpened);

		return myLast;
	}

	public static void Update (bool dbconOpened, string updateString)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "UPDATE " + table + " SET " + updateString;

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);
	}

	public static void UpdateComments (bool dbconOpened, int uniqueID, string comments)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "UPDATE " + table + " SET comments = \"" + comments + "\"" +
			" WHERE uniqueID = " + uniqueID;

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);
	}

	/* right now unused
	public static void DeleteSQLAndFile (bool dbconOpened, int uniqueID)
	{
		ForceSensor fs = (ForceSensor) Select (dbconOpened, uniqueID, -1, -1)[0];
		DeleteSQLAndFile (dbconOpened, fs);
	}
	*/
	public static void DeleteSQLAndFiles (bool dbconOpened, ForceSensor fs)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "DELETE FROM " + table + " WHERE uniqueID = " + fs.UniqueID;

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);

		//delete the files
		Util.FileDelete(fs.FullURL);

		if(fs.FullVideoURL != "")
			Util.FileDelete(fs.FullVideoURL);
	}

	//elastic (-1: both; 0: not elastic; 1: elastic)
	public static List<ForceSensor> Select (bool dbconOpened, int uniqueID, int personID, int sessionID, int elastic)
	{
		openIfNeeded(dbconOpened);

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

		string elasticStr = "";
		if (elastic == 0)
			elasticStr = " AND " + Constants.ForceSensorExerciseTable + ".elastic != 1"; //0 or -1 (both)
		else if (elastic == 1)
			elasticStr = " AND " + Constants.ForceSensorExerciseTable + ".elastic != 0"; //1 or -1 (both)

		dbcmd.CommandText = selectStr + whereStr + uniqueIDStr + personIDStr + sessionIDStr + elasticStr +
			" Order BY " + table + ".uniqueID";

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		List<ForceSensor> list = new List<ForceSensor>();
		ForceSensor fs;

		while(reader.Read()) {
			fs = new ForceSensor (
					Convert.ToInt32(reader[0].ToString()),	//uniqueID
					Convert.ToInt32(reader[1].ToString()),	//personID
					Convert.ToInt32(reader[2].ToString()),	//sessionID
					Convert.ToInt32(reader[3].ToString()),	//exerciseID
					(ForceSensor.CaptureOptions) Enum.Parse(
						typeof(ForceSensor.CaptureOptions), reader[4].ToString()), 	//captureOption
					Convert.ToInt32(reader[5].ToString()),	//angle
					reader[6].ToString(),			//laterality
					reader[7].ToString(),			//filename
					Util.MakeURLabsolute(fixOSpath(reader[8].ToString())),	//url
					reader[9].ToString(),			//datetime
					reader[10].ToString(),			//comments
					reader[11].ToString(),			//videoURL
					Convert.ToDouble(Util.ChangeDecimalSeparator(
							reader[12].ToString())), //stiffness
					reader[13].ToString(),			//stiffnessString
					Convert.ToDouble(Util.ChangeDecimalSeparator(
							reader[14].ToString())), //maxForceRaw
					Convert.ToDouble(Util.ChangeDecimalSeparator(
							reader[15].ToString())), //maxAVgForce1s
					reader[16].ToString()			//exerciseName
					);
			list.Add(fs);
		}

		reader.Close();
		closeIfNeeded(dbconOpened);

		return list;
	}

	public static ArrayList SelectRowsOfAnExercise(bool dbconOpened, int exerciseID)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "select count(*), " +
			Constants.PersonTable + ".name, " +
			Constants.SessionTable + ".name, " +
			Constants.SessionTable + ".date " +
			" FROM " + table + ", " + Constants.PersonTable + ", " + Constants.SessionTable +
			" WHERE exerciseID == " + exerciseID +
			" AND " + Constants.PersonTable + ".uniqueID == " + table + ".personID " +
		        " AND " + Constants.SessionTable + ".uniqueID == " + table + ".sessionID " +
			" GROUP BY sessionID, personID";

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		SqliteDataReader reader;
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

	public static ArrayList SelectSessionOverviewSets (bool dbconOpened, int sessionID, Constants.Modes chronojumpMode)
	{
		if(! dbconOpened)
			Sqlite.Open();

		string elasticStr = "";
		if (chronojumpMode == Constants.Modes.FORCESENSORISOMETRIC)
			elasticStr = " AND " + Constants.ForceSensorExerciseTable + ".elastic != 1"; //0 or -1 (both)
		else if (chronojumpMode == Constants.Modes.FORCESENSORELASTIC)
			elasticStr = " AND " + Constants.ForceSensorExerciseTable + ".elastic != 0"; //1 or -1 (both)

		dbcmd.CommandText =
			"SELECT person77.uniqueID, person77.name, person77.sex, forceSensorExercise.name, COUNT(*)" +
			" FROM person77, personSession77, forceSensorExercise, forceSensor" +
			" WHERE person77.uniqueID == forceSensor.personID AND personSession77.personID == forceSensor.personID AND personSession77.sessionID == forceSensor.sessionID AND forceSensorExercise.uniqueID==forceSensor.exerciseID AND forceSensor.sessionID == " + sessionID + elasticStr +
			" GROUP BY forceSensor.personID, exerciseID" +
			" ORDER BY person77.name";

		LogB.SQL(dbcmd.CommandText.ToString());

		SqliteDataReader reader;
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
			};
			array.Add (s);
		}

		reader.Close();
		if(! dbconOpened)
			Sqlite.Close();

		return array;
	}

	//this method is here to have a createTable that does not change in future versions
	protected internal static void createTable_windows_forceSensor_db_2_34_migration
		(SqliteCommand mycmd, string migrateToTable) //needed for migration from 2_34 to 2.35 on windows
	{
		mycmd.CommandText =
			"DROP TABLE IF EXISTS \"" + migrateToTable +
			"\"; CREATE TABLE \"" + migrateToTable + "\" ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"personID INT, " +
			"sessionID INT, " +
			"exerciseID INT, " +
			"captureOption TEXT, " + //ForceSensor.CaptureOptions {NORMAL, ABS, INVERTED}
			"angle INT, " + 	//angle can be different than the defaultAngle on exercise
			"laterality TEXT, " +	//"Both" "Right" "Left". stored in english
			"filename TEXT, " +
			"url TEXT, " +		//URL of data files. stored as relative
			"datetime TEXT, " + 	//2019-07-11_15-01-44
			"comments TEXT, " +
			"videoURL TEXT, " +	//URL of video of signals. stored as relative
			"stiffness FLOAT DEFAULT -1, " +	//this is the important, next one is needed for recalculate, but note that some bands can have changed or being deleted
			"stiffnessString TEXT, " + //uniqueID*active of ElasticBand separated by ';' or empty if exerciseID ! elastic
			"maxForceRaw FLOAT, " +
			"maxAvgForce1s FLOAT)";
		LogB.SQL(mycmd.CommandText.ToString());
		mycmd.ExecuteNonQuery();
	}

	/*
	 * this import converts all the forceSensor files into SQL rows with a column pointing the file
	 * persons have to be recognized/created (if is not possible to get the person then an Unknown person is created)
	 * forceSensorExercises created (if is not possible to assign the exercise, or there are no exercises, a Unknown exercise is created
	 */
	protected internal static void import_from_1_68_to_1_69() //database is opened
	{
		//LogB.PrintAllThreads = true; //comment this
		LogB.Information("at import_from_1_68_to_1_69()");

		string forceSensorDir = Util.GetForceSensorDir();
		if(Sqlite.UpdatingDBFrom == Sqlite.UpdatingDBFromEnum.IMPORTED_SESSION)
			forceSensorDir = Path.Combine(Util.GetDatabaseTempImportDir(), "forceSensor");

		int unknownPersonID = Sqlite.ExistsAndGetUniqueID(true, Constants.PersonTable, Catalog.GetString("Unknown"));
		bool personSessionExistsInSession;
		int unknownExerciseID = Sqlite.ExistsAndGetUniqueID(true, Constants.ForceSensorExerciseTable, Catalog.GetString("Unknown"));

		DirectoryInfo [] sessions = new DirectoryInfo(forceSensorDir).GetDirectories();
		conversionRateTotal = sessions.Length;
		conversionRate = 1;
		foreach (DirectoryInfo session in sessions) //session.Name will be the UniqueID
		{
			//if there is a session where the user manually changed the folder name (has to be a sessionID)
			//to any other thing, then do not import this session
			if(! Util.IsNumber(session.Name, false))
				continue;

			if(unknownPersonID == -1)
				personSessionExistsInSession = false;
			else
				personSessionExistsInSession = SqlitePersonSession.PersonSelectExistsInSession(true, unknownPersonID, Convert.ToInt32(session.Name));

			FileInfo[] files = session.GetFiles();
			conversionSubRateTotal = files.Length;
			conversionSubRate = 1;
			foreach (FileInfo file in files)
			{
				string fileWithoutExtension = Util.RemoveExtension(Util.GetLastPartOfPath(file.Name));
				ForceSensorLoadTryToAssignPersonAndMore fslt =
					new ForceSensorLoadTryToAssignPersonAndMore(true, fileWithoutExtension, Convert.ToInt32(session.Name));

				Person p = fslt.GetPerson();
				//if person is not found
				if(p.UniqueID == -1)
				{
					if(unknownPersonID == -1)
					{
						LogB.Information("going to insert person Unknown");
						Person pUnknown = new Person (Catalog.GetString("Unknown"), Constants.SexU, DateTime.Now,
								Constants.RaceUndefinedID,
								Constants.CountryUndefinedID,
								"", "", "", //description; future1: rfid; future2: clubID
								Constants.ServerUndefinedID, "", //linkServerImage
								true); //dbconOpened
						unknownPersonID = pUnknown.UniqueID;
					}
					p.UniqueID = unknownPersonID;
					p.Name = Catalog.GetString("Unknown");

					if(! personSessionExistsInSession)
					{
						LogB.Information("going to insert personSession");
						new PersonSession(unknownPersonID, Convert.ToInt32(session.Name), 0, 75,
								Constants.SportUndefinedID, Constants.SpeciallityUndefinedID, Constants.LevelUndefinedID,
								"", 		//comments
								Constants.TrochanterToeUndefinedID,
								Constants.TrochanterFloorOnFlexionUndefinedID,
								true); 		//dbconOpened

						personSessionExistsInSession = true;
					}
				}

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
					{
						ForceSensorExercise fse = new ForceSensorExercise (-1, Catalog.GetString("Unknown"), 0, "", 0, "", false, false, ForceSensorExercise.Types.ISOMETRIC);
						//note we are on 1_68 so we need this import method
						unknownExerciseID = SqliteForceSensorExerciseImport.InsertAtDB_1_68(true, fse);
					}

					exerciseID = unknownExerciseID;
					exerciseName = Catalog.GetString("Unknown");

					//put the old path on comment
					fslt.Comment = file.Name;
				}

				if(fslt.Exercise != "" && exerciseID == -1)
				{
					ForceSensorExercise fse = new ForceSensorExercise (-1, fslt.Exercise, 0, "", 0, "", false, false, ForceSensorExercise.Types.ISOMETRIC);
					//note we are on 1_68 so we need this import method
					unknownExerciseID = SqliteForceSensorExerciseImport.InsertAtDB_1_68(true, fse);
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
				LogB.Information("KKKKKK " + file.Name);
				Match match = Regex.Match(file.Name, @"(\d+-\d+-\d+_\d+-\d+-\d+)");
				if(match.Groups.Count == 2)
					parsedDate = match.Value;

				//filename will be this
				string myFilename = p.UniqueID + "_" + p.Name + "_" + parsedDate + ".csv";
				//try to rename the file
				try{
					//File.Move(file.FullName, Util.GetForceSensorSessionDir(Convert.ToInt32(session.Name)) + Path.DirectorySeparatorChar + myFilename);
					//file.MoveTo(myFilename);
					LogB.Information("copy from file.FullName: " + file.FullName);
				        LogB.Information("copy to: " + file.FullName.Replace(file.Name, myFilename));
					File.Move(file.FullName, file.FullName.Replace(file.Name, myFilename));
				} catch {
					//if cannot, then use old filename
					//myFilename = file.FullName;
					LogB.Information("catched at move, using the old filename: " + file.Name);
					myFilename = file.Name;
				}

				LogB.Information("going to insert forceSensor");
				ForceSensor forceSensor = new ForceSensor(-1, p.UniqueID, Convert.ToInt32(session.Name), exerciseID,
						ForceSensor.CaptureOptions.NORMAL,
						ForceSensor.AngleUndefined, lat,
						myFilename,
						Util.MakeURLrelative(Util.GetForceSensorSessionDir(Convert.ToInt32(session.Name))),
						parsedDate, fslt.Comment,
						"", -1, "", //videoURL, stiffness, stiffnessString
						-1, -1, //maxForceRaw, maxAvgForce1s
						exerciseName);
				forceSensor.InsertSQL(true);
				conversionSubRate ++;
			}
			conversionRate ++;
		}

		//LogB.PrintAllThreads = false; //TODO: remove this
	}
}

class SqliteForceSensorExercise : Sqlite
{
	protected static string table = Constants.ForceSensorExerciseTable;

	public SqliteForceSensorExercise() {
	}

	~SqliteForceSensorExercise() {}

	/*
	 * create and initialize tables
	 */

	protected internal static new void createTable()
	{
		dbcmd.CommandText =
			"CREATE TABLE " + table + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"name TEXT, " +
			"percentBodyWeight INT NOT NULL, " +
			"resistance TEXT, " + 				//unused
			"angleDefault INT, " +
			"description TEXT, " +
			"tareBeforeCapture INT, " +
			"forceResultant INT NOT NULL, " +
			"elastic INT NOT NULL, " + 	//since 2.2.2 on edit can be also -1 (meaning both, used when force is divided into isometric/elastic)
			"eccReps INT DEFAULT 0, " + 	//since ~2.2.2 (not really a change on DB) is repetitionsShow
			"eccMin FLOAT DEFAULT -1, " + 	//can be displacement or N
			"conMin FLOAT DEFAULT -1)"; 	//can be displacement or N
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}

	//undefined defaultAngle will be 1000
	//note execution can have a different angle than the default angle
	public static int Insert (bool dbconOpened, ForceSensorExercise ex)
	{
		if(! dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "INSERT INTO " + table +
				" (uniqueID, name, percentBodyWeight, resistance, angleDefault, " +
				" description, tareBeforeCapture, forceResultant, elastic, " +
				" eccReps, eccMin, conMin)" +
				" VALUES (" + ex.ToSQLInsertString() + ")";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		string myString = @"select last_insert_rowid()";
		dbcmd.CommandText = myString;
		int myLast = Convert.ToInt32(dbcmd.ExecuteScalar()); // Need to type-cast since `ExecuteScalar` returns an object.

		if(! dbconOpened)
			Sqlite.Close();

		return myLast;
	}

	//Default exercise for users without exercises (empty database creation or never used forceSensor)
	protected internal static void insertDefault ()
	{
		Insert (true, new ForceSensorExercise(-1, "Leg extension", 0, "", 0,
					"", false, false, ForceSensorExercise.Types.ISOMETRIC,
					ForceSensorExercise.RepetitionsShowTypes.CONCENTRIC,
					100, 100));
		Insert (true, new ForceSensorExercise(-1, "ABD/ADD", 0, "", 0,
					"Abduction/Adduction", false, false, ForceSensorExercise.Types.ISOMETRIC,
					ForceSensorExercise.RepetitionsShowTypes.BOTHSEPARATED,
					100, 100));
		Insert (true, new ForceSensorExercise(-1, "Mid thigh pull", 100, "", 90,
					"", false, true, ForceSensorExercise.Types.ISOMETRIC,
					ForceSensorExercise.RepetitionsShowTypes.CONCENTRIC,
					200, 200));
		Insert (true, new ForceSensorExercise(-1, "Hamstring", 0, "", 0,
					"", true, true, ForceSensorExercise.Types.ISOMETRIC,
					ForceSensorExercise.RepetitionsShowTypes.CONCENTRIC,
					50, 50));
		Insert (true, new ForceSensorExercise(-1, "Pull rubber band", 0, "", 0,
					"", false, true, ForceSensorExercise.Types.ELASTIC,
					ForceSensorExercise.RepetitionsShowTypes.CONCENTRIC,
					-1, -1));
	}

	public static void Update (bool dbconOpened, ForceSensorExercise ex)
	{
		if(! dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "UPDATE " + table + " SET " +
			" name = \"" + ex.Name +
			"\", percentBodyWeight = " + ex.PercentBodyWeight +
			", resistance = \"" + ex.Resistance + 					//unused
			"\", angleDefault = " + ex.AngleDefault +
			", description = \"" + ex.Description +
			"\", tareBeforeCapture = " + Util.BoolToInt(ex.TareBeforeCaptureOnExerciseEdit).ToString() +
			", forceResultant = " + Util.BoolToInt(ex.ForceResultant).ToString() +
			", elastic = " + ex.TypeToInt ().ToString() +
			", eccReps = " + ex.RepetitionsShowToCode().ToString() +
			", eccMin = " + Util.ConvertToPoint(ex.EccMin) +
			", conMin = " + Util.ConvertToPoint(ex.ConMin) +
			" WHERE uniqueID = " + ex.UniqueID;

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		if(! dbconOpened)
			Sqlite.Close();
	}

	public static void Delete (bool dbconOpened, int uniqueID)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "DELETE FROM " + table + " WHERE uniqueID = " + uniqueID;

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);
	}


	//elastic (-1: both; 0: not elastic; 1: elastic)
	//nameLike apply a LIKE %name%
	public static ArrayList Select (bool dbconOpened, int uniqueID, int elastic, bool onlyNames, string nameLike)
	{
		if(! dbconOpened)
			Sqlite.Open();

		string whereOrAndStr = " WHERE ";

		string uniqueIDStr = "";
		if(uniqueID != -1)
		{
			uniqueIDStr = whereOrAndStr + table + ".uniqueID = " + uniqueID;
			whereOrAndStr = " AND ";
		}

		string elasticStr = "";
		if(elastic != -1)
		{
			/*
			//note for elastic need: elastic = 1 && forceResultant = 1 (like ForceSensor.ComputeAsElastic does)
			if (elastic == 1)
				elasticStr = whereOrAndStr + table + ".elastic = 1 AND " + table + ".forceResultant = 1";
			else //elastic == 0
				elasticStr = whereOrAndStr + " (" + table + ".elastic = 0 OR " + table + ".forceResultant = 0)";
				*/
			//since the separation between isometric and elastic, show on elastic all the elastic exercises (not only the resultant = 1)
			if (elastic == 1)
				elasticStr = whereOrAndStr + table + ".elastic != 0"; //elastic && both (-1)
			else //elastic == 0
				elasticStr = whereOrAndStr + table + ".elastic != 1"; //isometric && both (-1)

			whereOrAndStr = " AND ";
		}

		string filterNameStr = "";
		if (nameLike != "")
			filterNameStr = whereOrAndStr + table + ".name LIKE '%" + nameLike + "%'";

		if(onlyNames)
			dbcmd.CommandText = "SELECT name FROM " + table + uniqueIDStr + elasticStr + filterNameStr;
		else
			dbcmd.CommandText = "SELECT * FROM " + table + uniqueIDStr + elasticStr + filterNameStr;

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
			while(reader.Read())
			{
				if(reader.FieldCount == 9) //DB 1.73
					ex = new ForceSensorExercise (
							Convert.ToInt32(reader[0].ToString()),	//uniqueID
							reader[1].ToString(),			//name
							Convert.ToInt32(reader[2].ToString()),	//percentBodyWeight
							reader[3].ToString(),			//resistance (unused)
							Convert.ToInt32(reader[4].ToString()), 	//angleDefault
							reader[5].ToString(),			//description
							Util.IntToBool(Convert.ToInt32(reader[6].ToString())),	//tareBeforeCapture
							Util.IntToBool(Convert.ToInt32(reader[7].ToString())),	//forceResultant
							ForceSensorExercise.IntToType (Convert.ToInt32(reader[8].ToString()))	//elastic (on this DB conversation cannot be both: "-1")
							);
				else //if(reader.FieldCount == 12) DB: 1.87
					ex = new ForceSensorExercise (
							Convert.ToInt32(reader[0].ToString()),	//uniqueID
							reader[1].ToString(),			//name
							Convert.ToInt32(reader[2].ToString()),	//percentBodyWeight
							reader[3].ToString(),			//resistance (unused)
							Convert.ToInt32(reader[4].ToString()), 	//angleDefault
							reader[5].ToString(),			//description
							Util.IntToBool(Convert.ToInt32(reader[6].ToString())),	//tareBeforeCapture
							Util.IntToBool(Convert.ToInt32(reader[7].ToString())),	//forceResultant
							ForceSensorExercise.IntToType (Convert.ToInt32(reader[8].ToString())),	//elastic (on this DB conversation cannot be both: "-1")
							ForceSensorExercise.RepetitionsShowFromCode(Convert.ToInt32(reader[9].ToString())),	//eccReps
							Convert.ToDouble(Util.ChangeDecimalSeparator(reader[10].ToString())), 	//eccMin
							Convert.ToDouble(Util.ChangeDecimalSeparator(reader[11].ToString())) 	//conMin
							);
				array.Add(ex);
			}
		}

		reader.Close();
		if(! dbconOpened)
			Sqlite.Close();

		return array;
	}

	/*
	   ForceSensor exercises raw are now both (isometric & elastic) because there was a bug creating raw exercises
	   (elastic was not asked and was assigned true) and we don't know where to put them
	   */
	protected internal static void UpdateTo2_40 ()
	{
		dbcmd.CommandText = "UPDATE " + table + " SET elastic = -1 WHERE forceResultant = 0";
		//-1 as is the same than a select inespecific

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}
}

class SqliteForceSensorExerciseImport : SqliteForceSensorExercise
{
	public SqliteForceSensorExerciseImport() {
	}

	~SqliteForceSensorExerciseImport() {}

	protected internal static void createTable_v_1_58()
	{
		dbcmd.CommandText =
			"CREATE TABLE " + table + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"name TEXT, " +
			"percentBodyWeight INT NOT NULL, " +
			"resistance TEXT, " + 				//unused
			"angleDefault INT, " +
			"description TEXT, " +
			"tareBeforeCapture INT)";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}

	public static int InsertAtDB_1_68 (bool dbconOpened, ForceSensorExercise ex)
	{
		if(! dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "INSERT INTO " + table +
				" (uniqueID, name, percentBodyWeight, resistance, angleDefault, " +
				" description, tareBeforeCapture)" +
				" VALUES (" + ex.ToSQLInsertString_DB_1_68() + ")";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		string myString = @"select last_insert_rowid()";
		dbcmd.CommandText = myString;
		int myLast = Convert.ToInt32(dbcmd.ExecuteScalar()); // Need to type-cast since `ExecuteScalar` returns an object.

		if(! dbconOpened)
			Sqlite.Close();

		return myLast;
	}

	//database is opened
	protected internal static void import_partially_from_1_73_to_1_74_unify_resistance_and_description()
	{
		ArrayList exercises = Select(true, -1, -1, false, "");
		foreach (ForceSensorExercise ex in exercises)
		{
			LogB.Information(ex.ToString());
			if(ex.Resistance == "")
				continue;

			if(ex.Description == "")
				ex.Description = ex.Resistance;
			else
				ex.Description = ex.Resistance + " - " + ex.Description;

			ex.Resistance = "";

			Update_1_73_to_1_74(true, ex);
		}
	}

	public static void Update_1_73_to_1_74 (bool dbconOpened, ForceSensorExercise ex)
	{
		if(! dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "UPDATE " + table + " SET " +
			" name = \"" + ex.Name +
			"\", percentBodyWeight = " + ex.PercentBodyWeight +
			", resistance = \"" + ex.Resistance + 					//unused
			"\", angleDefault = " + ex.AngleDefault +
			", description = \"" + ex.Description +
			"\", tareBeforeCapture = " + Util.BoolToInt(ex.TareBeforeCaptureOnExerciseEdit).ToString() +
			", forceResultant = " + Util.BoolToInt(ex.ForceResultant).ToString() +
			", elastic = " + ex.TypeToInt ().ToString() + //on this DB conversation cannot be both "-1"
			" WHERE uniqueID = " + ex.UniqueID;

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		if(! dbconOpened)
			Sqlite.Close();
	}

}

class SqliteForceSensorElasticBand : Sqlite
{
	private static string table = Constants.ForceSensorElasticBandTable;

	public SqliteForceSensorElasticBand() {
	}

	~SqliteForceSensorElasticBand() {}

	/*
	 * create and initialize tables
	 */

	/*
	 * note we use AUTOINCREMENT here
	 * because rubber bands can be deleted
	 * and deleting them will not delete the forceSensor table rows
	 * but if we add a new rubber band, we want that it has a different ID than previously deleted.
	 * This is different from the rest of the sofware because:
	 * on the rest of the software, we care to delete the rows on related tables
	 *
	 * Note AUTOINCREMENT should only be used on special situations:
	 * https://www.sqlitetutorial.net/sqlite-autoincrement/
	 */
	protected internal static new void createTable()
	{
		dbcmd.CommandText =
			"CREATE TABLE " + table + " ( " +
			"uniqueID INTEGER PRIMARY KEY AUTOINCREMENT, " +
			"active INT, " + 	//0 inactive, 3 using 3 like this now
			"brand TEXT, " +
			"color TEXT, " +
			"stiffness FLOAT, " +
			"comments TEXT)";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}

	public static int Insert (bool dbconOpened, ForceSensorElasticBand eb)
	{
		if(! dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "INSERT INTO " + table +
				" (uniqueID, active, brand, color, stiffness, comments)" +
				" VALUES (" + eb.ToSQLInsertString() + ")";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		string myString = @"select last_insert_rowid()";
		dbcmd.CommandText = myString;
		int myLast = Convert.ToInt32(dbcmd.ExecuteScalar()); // Need to type-cast since `ExecuteScalar` returns an object.

		if(! dbconOpened)
			Sqlite.Close();

		return myLast;
	}

	public static void Update (bool dbconOpened, ForceSensorElasticBand eb)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "UPDATE " + table + " SET " +
			" active = " + eb.Active.ToString() +
			", brand = \"" + eb.Brand +
			"\", color = \"" + eb.Color +
			"\", stiffness = " + Util.ConvertToPoint(eb.Stiffness) +
			", comments = \"" + eb.Comments +
			"\" WHERE uniqueID = " + eb.UniqueID;

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);
	}
	public static void UpdateList (bool dbconOpened, List<ForceSensorElasticBand> list_fseb)
	{
		openIfNeeded(dbconOpened);

		foreach(ForceSensorElasticBand fseb in list_fseb)
			Update (true, fseb);

		closeIfNeeded(dbconOpened);
	}

	public static void Delete (bool dbconOpened, int uniqueID)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "DELETE FROM " + table + " WHERE uniqueID = " + uniqueID;

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);
	}

	public static List<ForceSensorElasticBand> SelectAll (bool dbconOpened, bool onlyActive)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "SELECT * FROM " + table;
		if(onlyActive)
			dbcmd.CommandText += " WHERE active > 0";

		LogB.SQL(dbcmd.CommandText.ToString());

		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader = dbcmd.ExecuteReader();

		List<ForceSensorElasticBand> list_fseb = new List<ForceSensorElasticBand>();

		while(reader.Read()) {
			ForceSensorElasticBand fseb = new ForceSensorElasticBand (
					Convert.ToInt32(reader[0].ToString()),	//uniqueID
					Convert.ToInt32(reader[1].ToString()), 	//active
					reader[2].ToString(),			//brand
					reader[3].ToString(),			//color
					Convert.ToDouble(Util.ChangeDecimalSeparator(reader[4].ToString())),
					reader[5].ToString()			//comments
					);
			list_fseb.Add(fseb);
		}

		reader.Close();
		closeIfNeeded(dbconOpened);

		return list_fseb;
	}

	public static List<string> SelectSessionNamesWithCapturesWithElasticBand (int elasticBandID)
	{
		Sqlite.Open();
		dbcmd.CommandText =
			"SELECT session.name, forceSensor.stiffnessString " +
			"FROM session, forceSensor, forceSensorExercise " +
			"WHERE forceSensor.sessionID = session.uniqueID " +
			"AND forceSensor.exerciseID = forceSensorExercise.uniqueID " +
			"AND forceSensorExercise.elastic != 0 " + //elastic && both
			"AND forceSensorExercise.forceResultant = 1 " +
			"ORDER BY session.name";

		LogB.SQL(dbcmd.CommandText.ToString());

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		List<string> sessionsWithThisEB = new List<string>();

		while(reader.Read())
		{
			//if we already have this session on list, continue
			string sessionName = reader[0].ToString();
			foreach(string s in sessionsWithThisEB)
				if(s == sessionName)
					continue;

			string stiffnessString = reader[1].ToString();
			string [] stiffPairs = stiffnessString.Split(new char[] {';'});
			foreach(string str in stiffPairs)
			{
				string [] strPair = str.Split(new char[] {'*'});
				if(Util.IsNumber(strPair[0], false) && Convert.ToInt32(strPair[0]) == elasticBandID)
				{
					sessionsWithThisEB.Add(sessionName);
					continue;
				}
			}
		}

		reader.Close();
		Sqlite.Close();

		return sessionsWithThisEB;
	}

	//stiffnessString is a parameter of forceSensor table
	public static double GetStiffnessOfACapture (bool dbconOpened, string stiffnessString)
	{
		//return 0 if empty
		if(stiffnessString == "")
			return 0;

		string [] strFull = stiffnessString.Split(new char[] {';'});
		/*
		 * TODO: fix this comprovations knowing that values come as "id*active;..."
		 *
		//return 0 if there is only one value and is not a integer
		if(strFull.Length == 1) //there is just one value (there are no ';')
			if(! Util.IsNumber(strFull[0], false))
				return 0;

		//return 0 if there is any of the values is not an integer
		foreach(string s in strFull)
			if(! Util.IsNumber(s, false))
				return 0;
				*/

		return getStiffnessOfACaptureDo (dbconOpened, strFull);
	}
	private static double getStiffnessOfACaptureDo (bool dbconOpened, string [] stiffnessStrArray)
	{
		openIfNeeded(dbconOpened);

		/*
		 * instead of doing a select for each of the members of stiffnessArray (slow),
		 * do a select of all and the filter what is not on the array
		 */

		dbcmd.CommandText = "SELECT uniqueID, stiffness FROM " + table;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader = dbcmd.ExecuteReader();

		double sum = 0;
		while(reader.Read())
		{
			string id = reader[0].ToString();
			foreach(string str in stiffnessStrArray)
			{
				string [] strFull = str.Split(new char[] {'*'});
				if(strFull[0] == id)
					sum += Convert.ToDouble(Util.ChangeDecimalSeparator(reader[1].ToString())) * Convert.ToInt32(strFull[1]);
			}
		}

		reader.Close();
		closeIfNeeded(dbconOpened);

		return sum;
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

	protected internal static new void createTable()
	{
		dbcmd.CommandText = 
			"CREATE TABLE " + table + " ( " +
			"code TEXT, " + 	//RFD1...10, I (Impulse)
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
		UpdateTo2_47 ();

		InsertDefaultValueImpulse(true);

		closeIfNeeded(dbconOpened);
	}

	// adds RFDs 5-10
	protected internal static void UpdateTo2_47 ()
	{
		Insert(true, new ForceSensorRFD("RFD5", false,
					ForceSensorRFD.Functions.RAW, ForceSensorRFD.Types.BEST_AVG_RFD_IN_X_MS, 50, -1));
		Insert(true, new ForceSensorRFD("RFD6", false,
					ForceSensorRFD.Functions.RAW, ForceSensorRFD.Types.RFD_MAX, -1, -1));
		Insert(true, new ForceSensorRFD("RFD7", false,
					ForceSensorRFD.Functions.RAW, ForceSensorRFD.Types.RFD_MAX, -1, -1));
		Insert(true, new ForceSensorRFD("RFD8", false,
					ForceSensorRFD.Functions.RAW, ForceSensorRFD.Types.RFD_MAX, -1, -1));
		Insert(true, new ForceSensorRFD("RFD9", false,
					ForceSensorRFD.Functions.RAW, ForceSensorRFD.Types.RFD_MAX, -1, -1));
		Insert(true, new ForceSensorRFD("RFD10", false,
					ForceSensorRFD.Functions.RAW, ForceSensorRFD.Types.RFD_MAX, -1, -1));
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
			//try/catch here because BEST_AVG_RFD_IN_X_MS is new and a user can have a db of another more updated
			ForceSensorRFD rfd;
			try {
				rfd = new ForceSensorRFD(
						reader[0].ToString(), 				//code
						Util.IntToBool(Convert.ToInt32(reader[1])), 	//active
						(ForceSensorRFD.Functions) Enum.Parse(
							typeof(ForceSensorRFD.Functions), reader[2].ToString()), 	//function
						(ForceSensorRFD.Types) Enum.Parse(
							typeof(ForceSensorRFD.Types), reader[3].ToString()), 	//type
						Convert.ToInt32(reader[4]), 			//num1
						Convert.ToInt32(reader[5]) 			//num2
						);
			} catch  {
				LogB.Information (string.Format (
							"Catched on SqliteForceSensorRFD.SelectAll creating a ForceSensorRFD of function: {0} and type: {1}",
							reader[2].ToString(), reader[3].ToString() ));

				//create a default rfd for that code and make it inactive
				rfd = new ForceSensorRFD (reader[0].ToString(), false,
					ForceSensorRFD.Functions.FITTED, ForceSensorRFD.Types.INSTANTANEOUS, 0, -1);
			}

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
