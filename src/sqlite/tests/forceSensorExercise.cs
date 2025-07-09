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

class SqliteForceSensorExercise : Sqlite
{
    protected static string table = Constants.ForceSensorExerciseTable;

    public SqliteForceSensorExercise()
    {
    }

    ~SqliteForceSensorExercise() { }

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
            "resistance TEXT, " +               //unused
            "angleDefault INT, " +
            "description TEXT, " +
            "tareBeforeCapture INT, " +
            "forceResultant INT NOT NULL, " +
            "elastic INT NOT NULL, " +  //since 2.2.2 on edit can be also -1 (meaning both, used when force is divided into isometric/elastic)
            "eccReps INT DEFAULT 0, " +     //since ~2.2.2 (not really a change on DB) is repetitionsShow
            "eccMin FLOAT DEFAULT -1, " +   //can be displacement or N
            "conMin FLOAT DEFAULT -1)";     //can be displacement or N
        LogB.SQL(dbcmd.CommandText.ToString());
        dbcmd.ExecuteNonQuery();
    }

    //undefined defaultAngle will be 1000
    //note execution can have a different angle than the default angle
    public static int Insert(bool dbconOpened, ForceSensorExercise ex)
    {
        if (!dbconOpened)
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

        if (!dbconOpened)
            Sqlite.Close();

        return myLast;
    }

    //Default exercise for users without exercises (empty database creation or never used forceSensor)
    protected internal static void insertDefault()
    {
        Insert(true, new ForceSensorExercise(-1, "Leg extension", 0, "", 0,
                    "", false, false, ForceSensorExercise.Types.ISOMETRIC,
                    ForceSensorExercise.RepetitionsShowTypes.CONCENTRIC,
                    100, 100));
        Insert(true, new ForceSensorExercise(-1, "ABD/ADD", 0, "", 0,
                    "Abduction/Adduction", false, false, ForceSensorExercise.Types.ISOMETRIC,
                    ForceSensorExercise.RepetitionsShowTypes.BOTHSEPARATED,
                    100, 100));
        Insert(true, new ForceSensorExercise(-1, "Mid thigh pull", 100, "", 90,
                    "", false, true, ForceSensorExercise.Types.ISOMETRIC,
                    ForceSensorExercise.RepetitionsShowTypes.CONCENTRIC,
                    200, 200));
        Insert(true, new ForceSensorExercise(-1, "Hamstring", 0, "", 0,
                    "", true, true, ForceSensorExercise.Types.ISOMETRIC,
                    ForceSensorExercise.RepetitionsShowTypes.CONCENTRIC,
                    50, 50));
        Insert(true, new ForceSensorExercise(-1, "Pull rubber band", 0, "", 0,
                    "", false, true, ForceSensorExercise.Types.ELASTIC,
                    ForceSensorExercise.RepetitionsShowTypes.CONCENTRIC,
                    -1, -1));
    }

    public static void Update(bool dbconOpened, ForceSensorExercise ex)
    {
        if (!dbconOpened)
            Sqlite.Open();

        dbcmd.CommandText = "UPDATE " + table + " SET " +
            " name = '" + ex.Name +
            "', percentBodyWeight = " + ex.PercentBodyWeight +
            ", resistance = '" + ex.Resistance +                   //unused
            "', angleDefault = " + ex.AngleDefault +
            ", description = '" + ex.Description +
            "', tareBeforeCapture = " + Util.BoolToInt(ex.TareBeforeCaptureOnExerciseEdit).ToString() +
            ", forceResultant = " + Util.BoolToInt(ex.ForceResultant).ToString() +
            ", elastic = " + ex.TypeToInt().ToString() +
            ", eccReps = " + ex.RepetitionsShowToCode().ToString() +
            ", eccMin = " + Util.ConvertToPoint(ex.EccMin) +
            ", conMin = " + Util.ConvertToPoint(ex.ConMin) +
            " WHERE uniqueID = " + ex.UniqueID;

        LogB.SQL(dbcmd.CommandText.ToString());
        dbcmd.ExecuteNonQuery();

        if (!dbconOpened)
            Sqlite.Close();
    }

    public static void Delete(bool dbconOpened, int uniqueID)
    {
        openIfNeeded(dbconOpened);

        dbcmd.CommandText = "DELETE FROM " + table + " WHERE uniqueID = " + uniqueID;

        LogB.SQL(dbcmd.CommandText.ToString());
        dbcmd.ExecuteNonQuery();

        closeIfNeeded(dbconOpened);
    }


    //elastic (-1: both; 0: not elastic; 1: elastic)
    //nameLike apply a LIKE %name%
    public static List<ForceSensorExercise> Select (bool dbconOpened, int uniqueID, int elastic, bool onlyNames, string nameLike)
    {
        if (!dbconOpened)
            Sqlite.Open();

        string whereOrAndStr = " WHERE ";

        string uniqueIDStr = "";
        if (uniqueID != -1)
        {
            uniqueIDStr = whereOrAndStr + table + ".uniqueID = " + uniqueID;
            whereOrAndStr = " AND ";
        }

        string elasticStr = "";
        if (elastic != -1)
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

        if (onlyNames)
            dbcmd.CommandText = "SELECT name FROM " + table + uniqueIDStr + elasticStr + filterNameStr;
        else
            dbcmd.CommandText = "SELECT * FROM " + table + uniqueIDStr + elasticStr + filterNameStr;

        LogB.SQL(dbcmd.CommandText.ToString());
        dbcmd.ExecuteNonQuery();

        SQLiteDataReader reader;
        reader = dbcmd.ExecuteReader();

        List<ForceSensorExercise> fsex_l = new List<ForceSensorExercise> ();
        ForceSensorExercise fsex = new ForceSensorExercise();

        if (onlyNames)
        {
            while (reader.Read())
            {
                fsex = new ForceSensorExercise(reader[0].ToString());
                fsex_l.Add (fsex); //note this add a ForceSensorExercise with all fields, not just the name
            }
        }
        else
        {
            while (reader.Read())
            {
                if (reader.FieldCount == 9) //DB 1.73
                    fsex = new ForceSensorExercise(
                            Convert.ToInt32(reader[0].ToString()),  //uniqueID
                            reader[1].ToString(),           //name
                            Convert.ToInt32(reader[2].ToString()),  //percentBodyWeight
                            reader[3].ToString(),           //resistance (unused)
                            Convert.ToInt32(reader[4].ToString()),  //angleDefault
                            reader[5].ToString(),           //description
                            Util.IntToBool(Convert.ToInt32(reader[6].ToString())),  //tareBeforeCapture
                            Util.IntToBool(Convert.ToInt32(reader[7].ToString())),  //forceResultant
                            ForceSensorExercise.IntToType(Convert.ToInt32(reader[8].ToString()))    //elastic (on this DB conversation cannot be both: "-1")
                            );
                else //if(reader.FieldCount == 12) DB: 1.87
                    fsex = new ForceSensorExercise(
                            Convert.ToInt32(reader[0].ToString()),  //uniqueID
                            reader[1].ToString(),           //name
                            Convert.ToInt32(reader[2].ToString()),  //percentBodyWeight
                            reader[3].ToString(),           //resistance (unused)
                            Convert.ToInt32(reader[4].ToString()),  //angleDefault
                            reader[5].ToString(),           //description
                            Util.IntToBool(Convert.ToInt32(reader[6].ToString())),  //tareBeforeCapture
                            Util.IntToBool(Convert.ToInt32(reader[7].ToString())),  //forceResultant
                            ForceSensorExercise.IntToType(Convert.ToInt32(reader[8].ToString())),   //elastic (on this DB conversation cannot be both: "-1")
                            ForceSensorExercise.RepetitionsShowFromCode(Convert.ToInt32(reader[9].ToString())), //eccReps
                            Convert.ToDouble(Util.ChangeDecimalSeparator(reader[10].ToString())),   //eccMin
                            Convert.ToDouble(Util.ChangeDecimalSeparator(reader[11].ToString()))    //conMin
                            );
                fsex_l.Add (fsex);
            }
        }

        reader.Close();
        if (!dbconOpened)
            Sqlite.Close();

        return fsex_l;
    }

    /*
	   ForceSensor exercises raw are now both (isometric & elastic) because there was a bug creating raw exercises
	   (elastic was not asked and was assigned true) and we don't know where to put them
	   */
    protected internal static void UpdateTo2_40()
    {
        dbcmd.CommandText = "UPDATE " + table + " SET elastic = -1 WHERE forceResultant = 0";
        //-1 as is the same than a select inespecific

        LogB.SQL(dbcmd.CommandText.ToString());
        dbcmd.ExecuteNonQuery();
    }
}

class SqliteForceSensorExerciseImport : SqliteForceSensorExercise
{
    public SqliteForceSensorExerciseImport()
    {
    }

    ~SqliteForceSensorExerciseImport() { }

    protected internal static void createTable_v_1_58()
    {
        dbcmd.CommandText =
            "CREATE TABLE " + table + " ( " +
            "uniqueID INTEGER PRIMARY KEY, " +
            "name TEXT, " +
            "percentBodyWeight INT NOT NULL, " +
            "resistance TEXT, " +               //unused
            "angleDefault INT, " +
            "description TEXT, " +
            "tareBeforeCapture INT)";
        LogB.SQL(dbcmd.CommandText.ToString());
        dbcmd.ExecuteNonQuery();
    }

    public static int InsertAtDB_1_68(bool dbconOpened, ForceSensorExercise ex)
    {
        if (!dbconOpened)
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

        if (!dbconOpened)
            Sqlite.Close();

        return myLast;
    }

    //database is opened
    protected internal static void import_partially_from_1_73_to_1_74_unify_resistance_and_description()
    {
        List<ForceSensorExercise> fsex_l = Select(true, -1, -1, false, "");
        foreach (ForceSensorExercise ex in fsex_l)
        {
            LogB.Information(ex.ToString());
            if (ex.Resistance == "")
                continue;

            if (ex.Description == "")
                ex.Description = ex.Resistance;
            else
                ex.Description = ex.Resistance + " - " + ex.Description;

            ex.Resistance = "";

            Update_1_73_to_1_74(true, ex);
        }
    }

    public static void Update_1_73_to_1_74(bool dbconOpened, ForceSensorExercise ex)
    {
        if (!dbconOpened)
            Sqlite.Open();

        dbcmd.CommandText = "UPDATE " + table + " SET " +
            " name = '" + ex.Name +
            "', percentBodyWeight = " + ex.PercentBodyWeight +
            ", resistance = '" + ex.Resistance +                   //unused
            "', angleDefault = " + ex.AngleDefault +
            ", description = '" + ex.Description +
            "', tareBeforeCapture = " + Util.BoolToInt(ex.TareBeforeCaptureOnExerciseEdit).ToString() +
            ", forceResultant = " + Util.BoolToInt(ex.ForceResultant).ToString() +
            ", elastic = " + ex.TypeToInt().ToString() + //on this DB conversation cannot be both "-1"
            " WHERE uniqueID = " + ex.UniqueID;

        LogB.SQL(dbcmd.CommandText.ToString());
        dbcmd.ExecuteNonQuery();

        if (!dbconOpened)
            Sqlite.Close();
    }

}

