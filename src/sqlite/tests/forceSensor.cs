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
 * Copyright (C) 2017-2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
//using System.Data;
using System.Collections;
using System.Collections.Generic; //List<T>
using System.IO; //DirectoryInfo
using System.Text.RegularExpressions; //Regex
using Mono.Unix;

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

class SqliteForceSensor : SqliteTests
{
    private static string tableStatic = Constants.ForceSensorTable;

    public SqliteForceSensor()
    {
	    tableName = Constants.ForceSensorTable;
	    columnsStr = " (uniqueID, personID, sessionID, exerciseID, captureOption, angle, laterality," +
		    " filename, url, dateTime, comments, videoURL, stiffness, stiffnessString," +
		    " maxForceRaw, maxAvgForce1s)";
    }

    ~SqliteForceSensor() { }

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
            "exerciseID INT, " +
            "captureOption TEXT, " + //ForceSensor.CaptureOptions {NORMAL, ABS, INVERTED}
            "angle INT, " +     //angle can be different than the defaultAngle on exercise
            "laterality TEXT, " +   //"Both" "Right" "Left". stored in english
            "filename TEXT, " +
            "url TEXT, " +      //URL of data files. stored as relative
            "datetime TEXT, " +     //2019-07-11_15-01-44
            "comments TEXT, " +
            "videoURL TEXT, " + //URL of video of signals. stored as relative
            "stiffness FLOAT DEFAULT -1, " +    //this is the important, next one is needed for recalculate, but note that some bands can have changed or being deleted
            "stiffnessString TEXT, " + //uniqueID*active of ElasticBand separated by ';' or empty if exerciseID ! elastic
            "maxForceRaw FLOAT, " +
            "maxAvgForce1s FLOAT)";
        LogB.SQL(dbcmd.CommandText.ToString());
        dbcmd.ExecuteNonQuery();
    }

    public static void Update(bool dbconOpened, string updateString)
    {
        openIfNeeded(dbconOpened);

        dbcmd.CommandText = "UPDATE " + tableStatic + " SET " + updateString;

        LogB.SQL(dbcmd.CommandText.ToString());
        dbcmd.ExecuteNonQuery();

        closeIfNeeded(dbconOpened);
    }

    public static void UpdateComments(bool dbconOpened, int uniqueID, string comments)
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
		ForceSensor fs = (ForceSensor) Select (dbconOpened, uniqueID, -1, -1)[0];
		DeleteSQLAndFile (dbconOpened, fs);
	}
	*/
    public static void DeleteSQLAndFiles(bool dbconOpened, ForceSensor fs)
    {
        openIfNeeded(dbconOpened);

        dbcmd.CommandText = "DELETE FROM " + tableStatic + " WHERE uniqueID = " + fs.UniqueID;

        LogB.SQL(dbcmd.CommandText.ToString());
        dbcmd.ExecuteNonQuery();

        closeIfNeeded(dbconOpened);

        //delete the files
        Util.FileDelete(fs.FullURL);

        if (fs.FullVideoURL != "")
            Util.FileDelete(fs.FullVideoURL);
    }

    //elastic (-1: both; 0: not elastic; 1: elastic)
    public static List<ForceSensor> Select(bool dbconOpened, int uniqueID, int personID, int sessionID, int elastic)
    {
        openIfNeeded(dbconOpened);

        string selectStr = "SELECT " + tableStatic + ".*, " + Constants.ForceSensorExerciseTable + ".Name FROM " + tableStatic + ", " + Constants.ForceSensorExerciseTable;
        string whereStr = " WHERE " + tableStatic + ".exerciseID = " + Constants.ForceSensorExerciseTable + ".UniqueID ";

        string uniqueIDStr = "";
        if (uniqueID != -1)
            uniqueIDStr = " AND " + tableStatic + ".uniqueID = " + uniqueID;

        string personIDStr = "";
        if (personID != -1)
            personIDStr = " AND " + tableStatic + ".personID = " + personID;

        string sessionIDStr = "";
        if (sessionID != -1)
            sessionIDStr = " AND " + tableStatic + ".sessionID = " + sessionID;

        string elasticStr = "";
        if (elastic == 0)
            elasticStr = " AND " + Constants.ForceSensorExerciseTable + ".elastic != 1"; //0 or -1 (both)
        else if (elastic == 1)
            elasticStr = " AND " + Constants.ForceSensorExerciseTable + ".elastic != 0"; //1 or -1 (both)

        dbcmd.CommandText = selectStr + whereStr + uniqueIDStr + personIDStr + sessionIDStr + elasticStr +
            " Order BY " + tableStatic + ".uniqueID";

        LogB.SQL(dbcmd.CommandText.ToString());
        dbcmd.ExecuteNonQuery();

        SQLiteDataReader reader;
        reader = dbcmd.ExecuteReader();

        List<ForceSensor> list = new List<ForceSensor>();
        ForceSensor fs;

        while (reader.Read())
        {
            fs = new ForceSensor(
                    Convert.ToInt32(reader[0].ToString()),  //uniqueID
                    Convert.ToInt32(reader[1].ToString()),  //personID
                    Convert.ToInt32(reader[2].ToString()),  //sessionID
                    Convert.ToInt32(reader[3].ToString()),  //exerciseID
                    (ForceSensor.CaptureOptions)Enum.Parse(
                        typeof(ForceSensor.CaptureOptions), reader[4].ToString()),  //captureOption
                    Convert.ToInt32(reader[5].ToString()),  //angle
                    reader[6].ToString(),           //laterality
                    reader[7].ToString(),           //filename
                    Util.MakeURLabsolute(fixOSpath(reader[8].ToString())),  //url
                    reader[9].ToString(),           //datetime
                    reader[10].ToString(),          //comments
                    reader[11].ToString(),          //videoURL
                    Convert.ToDouble(Util.ChangeDecimalSeparator(
                            reader[12].ToString())), //stiffness
                    reader[13].ToString(),          //stiffnessString
                    Convert.ToDouble(Util.ChangeDecimalSeparator(
                            reader[14].ToString())), //maxForceRaw
                    Convert.ToDouble(Util.ChangeDecimalSeparator(
                            reader[15].ToString())), //maxAVgForce1s
                    reader[16].ToString()           //exerciseName
                    );
            list.Add(fs);
        }

        reader.Close();
        closeIfNeeded(dbconOpened);

        return list;
    }

    protected override string selectSAArray (SQLiteDataReader reader)
    {
	    return
		    reader[0].ToString() + ":" + 	//person.name
		    reader[1].ToString() + ":" +	//fs.uniqueID
		    reader[2].ToString() + ":" + 	//fs.personID
		    reader[3].ToString() + ":" + 	//fs.sessionID
		    reader[4].ToString() + ":" + 	//fs.exerciseID
		    reader[5].ToString() + ":" + 	//fs.captureOption
		    reader[6].ToString() + ":" +  	//fs.angle
		    reader[7].ToString() + ":" + 	//fs.laterality
		    reader[8].ToString() + ":" + 	//fs.filename
		    reader[9].ToString() + ":" + 	//fs.url
		    reader[10].ToString() + ":" + 	//fs.datetime
		    Util.CDSNoZero (reader[11].ToString()) + ":" + 	//fs.comments
		    reader[12].ToString() + ":" + 	//fs.videoURL
		    reader[13].ToString() + ":" + 	//fs.stiffness
		    reader[14].ToString() + ":" + 	//fs.stiffnessString
                    Util.CDSNoZero (reader[15].ToString()) + ":" + //maxForceRaw
                    Util.CDSNoZero (reader[16].ToString()) + ":" + //maxAVgForce1s
                    reader[17].ToString()           //exerciseName
		    ;
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
        while (reader.Read())
        {
            array.Add(new string[] {
                    count.ToString(),
                    reader[0].ToString(), //count
					reader[1].ToString(), //person name
					reader[2].ToString(), //session name
					reader[3].ToString()  //session date
			});
            count++;
        }

        reader.Close();
        closeIfNeeded(dbconOpened);

        return array;
    }

    public static ArrayList SelectSessionOverviewSets (bool dbconOpened, int sessionID, bool byExercises, Constants.Modes chronojumpMode)
    {
        if (!dbconOpened)
            Sqlite.Open();

        string elasticStr = "";
        if (chronojumpMode == Constants.Modes.FORCESENSORISOMETRIC)
            elasticStr = " AND " + Constants.ForceSensorExerciseTable + ".elastic != 1"; //0 or -1 (both)
        else if (chronojumpMode == Constants.Modes.FORCESENSORELASTIC)
            elasticStr = " AND " + Constants.ForceSensorExerciseTable + ".elastic != 0"; //1 or -1 (both)

	string byExercisesStr = "";
	if (byExercises)
		byExercisesStr = ", exerciseID";

        dbcmd.CommandText =
            "SELECT person77.uniqueID, person77.name, person77.sex, forceSensorExercise.name, COUNT(*)" +
            " FROM person77, personSession77, forceSensorExercise, forceSensor" +
            " WHERE person77.uniqueID = forceSensor.personID AND personSession77.personID = forceSensor.personID AND personSession77.sessionID = forceSensor.sessionID AND forceSensorExercise.uniqueID=forceSensor.exerciseID AND forceSensor.sessionID = " + sessionID + elasticStr +
            " GROUP BY forceSensor.personID" + byExercisesStr +
            " ORDER BY person77.name";

        LogB.SQL(dbcmd.CommandText.ToString());

        SQLiteDataReader reader;
        reader = dbcmd.ExecuteReader();

        ArrayList array = new ArrayList();
        while (reader.Read())
        {
            string[] s = {
                reader[0].ToString(), 	//personID
				reader[1].ToString(), 	//person name
				reader[2].ToString(), 	//person sex
				reader[3].ToString(), 	//exercise name
				reader[4].ToString()	//sets count
			}; //note this is used on gui/genericWindow
            array.Add(s);
        }

        reader.Close();
        if (!dbconOpened)
            Sqlite.Close();

        return array;
    }

    //this method is here to have a createTable that does not change in future versions
    protected internal static void createTable_windows_forceSensor_db_2_34_migration
        (SQLiteCommand mycmd, string migrateToTable) //needed for migration from 2_34 to 2.35 on windows
    {
        mycmd.CommandText =
            "DROP TABLE IF EXISTS '" + migrateToTable +
            "'; CREATE TABLE '" + migrateToTable + "' ( " +
            "uniqueID INTEGER PRIMARY KEY, " +
            "personID INT, " +
            "sessionID INT, " +
            "exerciseID INT, " +
            "captureOption TEXT, " + //ForceSensor.CaptureOptions {NORMAL, ABS, INVERTED}
            "angle INT, " +     //angle can be different than the defaultAngle on exercise
            "laterality TEXT, " +   //"Both" "Right" "Left". stored in english
            "filename TEXT, " +
            "url TEXT, " +      //URL of data files. stored as relative
            "datetime TEXT, " +     //2019-07-11_15-01-44
            "comments TEXT, " +
            "videoURL TEXT, " + //URL of video of signals. stored as relative
            "stiffness FLOAT DEFAULT -1, " +    //this is the important, next one is needed for recalculate, but note that some bands can have changed or being deleted
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
        if (Sqlite.UpdatingDBFrom == Sqlite.UpdatingDBFromEnum.IMPORTED_SESSION)
            forceSensorDir = Path.Combine(Util.GetDatabaseTempImportDir(), "forceSensor");

        int unknownPersonID = Sqlite.ExistsAndGetUniqueID(true, Constants.PersonTable, Catalog.GetString("Unknown"));
        bool personSessionExistsInSession;
        int unknownExerciseID = Sqlite.ExistsAndGetUniqueID(true, Constants.ForceSensorExerciseTable, Catalog.GetString("Unknown"));

        DirectoryInfo[] sessions = new DirectoryInfo(forceSensorDir).GetDirectories();
        conversionRateTotal = sessions.Length;
        conversionRate = 1;
        foreach (DirectoryInfo session in sessions) //session.Name will be the UniqueID
        {
            //if there is a session where the user manually changed the folder name (has to be a sessionID)
            //to any other thing, then do not import this session
            if (!Util.IsNumber(session.Name, false))
                continue;

            if (unknownPersonID == -1)
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
                if (p.UniqueID == -1)
                {
                    if (unknownPersonID == -1)
                    {
                        LogB.Information("going to insert person Unknown");
                        Person pUnknown = new Person(Catalog.GetString("Unknown"), Constants.SexU, DateTime.Now,
                                Constants.RaceUndefinedID,
                                Constants.CountryUndefinedID,
                                "", "", "", //description; future1: rfid; future2: clubID
                                Constants.ServerUndefinedID, "", //linkServerImage
                                true); //dbconOpened
                        unknownPersonID = pUnknown.UniqueID;
                    }
                    p.UniqueID = unknownPersonID;
                    p.Name = Catalog.GetString("Unknown");

                    if (!personSessionExistsInSession)
                    {
                        LogB.Information("going to insert personSession");
                        new PersonSession(unknownPersonID, Convert.ToInt32(session.Name), 0, 75,
                                Constants.SportUndefinedID, Constants.SpeciallityUndefinedID, Constants.LevelUndefinedID,
                                "",         //comments
                                Constants.TrochanterToeUndefinedID,
                                Constants.TrochanterFloorOnFlexionUndefinedID,
                                true);      //dbconOpened

                        personSessionExistsInSession = true;
                    }
                }

                if (!Util.IsNumber(session.Name, false))
                    continue;

                //at the beginning exercise was not written on the filename, because force sensor started without exercises on sql
                //"person name_2017-11-11_19-35-55.csv"
                //if cannot found exercise, assign to Unknown
                int exerciseID = -1;
                string exerciseName = fslt.Exercise;
                if (fslt.Exercise != "")
                    exerciseID = ExistsAndGetUniqueID(true, Constants.ForceSensorExerciseTable, fslt.Exercise);

                if (fslt.Exercise == "" || exerciseID == -1)
                {
                    if (unknownExerciseID == -1)
                    {
                        ForceSensorExercise fse = new ForceSensorExercise(-1, Catalog.GetString("Unknown"), 0, "", 0, "", false, false, ForceSensorExercise.Types.ISOMETRIC);
                        //note we are on 1_68 so we need this import method
                        unknownExerciseID = SqliteForceSensorExerciseImport.InsertAtDB_1_68(true, fse);
                    }

                    exerciseID = unknownExerciseID;
                    exerciseName = Catalog.GetString("Unknown");

                    //put the old path on comment
                    fslt.Comment = file.Name;
                }

                if (fslt.Exercise != "" && exerciseID == -1)
                {
                    ForceSensorExercise fse = new ForceSensorExercise(-1, fslt.Exercise, 0, "", 0, "", false, false, ForceSensorExercise.Types.ISOMETRIC);
                    //note we are on 1_68 so we need this import method
                    unknownExerciseID = SqliteForceSensorExerciseImport.InsertAtDB_1_68(true, fse);
                }

                //laterality (in English)
                string lat = fslt.Laterality;
                if (lat == Catalog.GetString(Constants.ForceSensorLateralityRight))
                    lat = Constants.ForceSensorLateralityRight;
                else if (lat == Catalog.GetString(Constants.ForceSensorLateralityLeft))
                    lat = Constants.ForceSensorLateralityLeft;
                else
                    lat = Constants.ForceSensorLateralityBoth;

                string parsedDate = UtilDate.ToFile(DateTime.MinValue);
                LogB.Information("KKKKKK " + file.Name);
                Match match = Regex.Match(file.Name, @"(\d+-\d+-\d+_\d+-\d+-\d+)");
                if (match.Groups.Count == 2)
                    parsedDate = match.Value;

                //filename will be this
                string myFilename = p.UniqueID + "_" + p.Name + "_" + parsedDate + ".csv";
                //try to rename the file
                try
                {
                    //File.Move(file.FullName, Util.GetForceSensorSessionDir(Convert.ToInt32(session.Name)) + Path.DirectorySeparatorChar + myFilename);
                    //file.MoveTo(myFilename);
                    LogB.Information("copy from file.FullName: " + file.FullName);
                    LogB.Information("copy to: " + file.FullName.Replace(file.Name, myFilename));
                    File.Move(file.FullName, file.FullName.Replace(file.Name, myFilename));
                }
                catch
                {
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
                conversionSubRate++;
            }
            conversionRate++;
        }

        //LogB.PrintAllThreads = false; //TODO: remove this
    }
}
