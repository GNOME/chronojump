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
 * Copyright (C) 2004-2025   Xavier de Blas <xaviblas@gmail.com>
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

class SqliteEncoderExercise : Sqlite
{
    //ressistance (weight bar, machine, goma, none, inertial, ...)
    protected internal static void createTableEncoderExercise()
    {
        dbcmd.CommandText =
            "CREATE TABLE " + Constants.EncoderExerciseTable + " ( " +
            "uniqueID INTEGER PRIMARY KEY, " +
            "name TEXT, " +
            "percentBodyWeight INT, " +
            "ressistance TEXT, " +
            "description TEXT, " +
            "future1 TEXT, " +  //speed1RM: speed in m/s at 1RM with decimal point separator '.' ; 0 means undefined
            "future2 TEXT, " +  //bodyAngle (unused)
            "future3 TEXT, " +  //weightAngle (unused)
            "type TEXT DEFAULT 'ALL')";   //ALL, GRAVITATORY, INERTIAL (enum constants.EncoderGI)
        dbcmd.ExecuteNonQuery();
    }

    //if uniqueID == -1, NULL will be used (correlative uniqueID)
    //uniqueID != -1 when an exercise is downloaded from server on compujump and need to have the same uniqueID as server
    public static void InsertExercise(bool dbconOpened, int uniqueID, string name, int percentBodyWeight,
            string ressistance, string description, string speed1RM,    //speed1RM decimal point = '.'
            Constants.EncoderGI encoderGI)                  //type
    {
        if (!dbconOpened)
            Sqlite.Open();

        string uniqueIDStr = "NULL";
        if (uniqueID != -1)
            uniqueIDStr = uniqueID.ToString();

        dbcmd.CommandText = "INSERT INTO " + Constants.EncoderExerciseTable +
                " (uniqueID, name, percentBodyWeight, ressistance, description, future1, future2, future3, type)" +
                " VALUES (" + uniqueIDStr + ", '" + name + "', " + percentBodyWeight + ", '" +
                ressistance + "', '" + description + "', '" + speed1RM + "', '', '', '" + encoderGI.ToString() + "')";
        LogB.SQL(dbcmd.CommandText.ToString());
        dbcmd.ExecuteNonQuery();

        if (!dbconOpened)
            Sqlite.Close();
    }

    //Note: if this names change, or there are new, change them on both:
    //gui/encoder createEncoderCombos();	
    //gui/encoder on_button_encoder_exercise_add_accepted (object o, EventArgs args) 
    protected internal static void initializeTableEncoderExercise()
    {
        string[] iniEncoderExercises = {
			//name:percentBodyWeight:ressistance:description:speed1RM:pullAngle:weightAngle
			"Bench press:0:weight bar::0.185:::GRAVITATORY", //González-Badillo, J. 2010. Movement velocity as a measure of loading intensity in resistance training
			"Squat:100:weight bar::0.31:::GRAVITATORY" //González-Badillo, JJ.2000b http://foro.chronojump.org/showthread.php?tid=1288&page=3
		};

        foreach (string line in iniEncoderExercises)
        {
            string[] parts = line.Split(new char[] { ':' });
            InsertExercise(true, -1, parts[0], Convert.ToInt32(parts[1]), parts[2], parts[3], parts[4],
                    (Constants.EncoderGI)Enum.Parse(typeof(Constants.EncoderGI), parts[7]));
        }

        addEncoderFreeExercise();
        addEncoderJumpExercise();
        addEncoderInclinedExercises();
    }

    protected internal static void addEncoderFreeExercise()
    {
        bool exists = Sqlite.Exists(true, Constants.EncoderExerciseTable, "Free");
        if (!exists)
            InsertExercise(true, -1, "Free", 0, "", "", "", Constants.EncoderGI.ALL);
    }
    protected internal static void addEncoderJumpExercise()
    {
        bool exists = Sqlite.Exists(true, Constants.EncoderExerciseTable, "Jump");
        if (!exists)
            InsertExercise(true, -1, "Jump", 100, "", "", "", Constants.EncoderGI.GRAVITATORY);
    }
    protected internal static void addEncoderInclinedExercises()
    {
        string[] iniEncoderExercises = {
			//name:percentBodyWeight:ressistance:description:speed1RM:bodyAngle:weightAngle:type
			"Inclined plane:0:machine:::::GRAVITATORY",
            "Inclined plane BW:100:machine:::::GRAVITATORY",
        };

        foreach (string line in iniEncoderExercises)
        {
            string[] parts = line.Split(new char[] { ':' });
            InsertExercise(true, -1, parts[0], Convert.ToInt32(parts[1]), parts[2], parts[3], parts[4],
                    (Constants.EncoderGI)Enum.Parse(typeof(Constants.EncoderGI), parts[7]));
        }
    }

    public static void UpdateExercise(bool dbconOpened, EncoderExercise ex)
    {
        if (!dbconOpened)
            Sqlite.Open();

        dbcmd.CommandText = "UPDATE " + Constants.EncoderExerciseTable + " SET" +
                " name = '" + ex.Name +
                "', percentBodyWeight = " + ex.PercentBodyWeight +
                ", ressistance = '" + ex.Ressistance +
                "', description = '" + ex.Description +
                "', future1 = '" + Util.ConvertToPoint(ex.Speed1RM) +
                "', type = '" + ex.Type +
                "' WHERE uniqueID = " + ex.UniqueID;

        LogB.SQL(dbcmd.CommandText.ToString());
        dbcmd.ExecuteNonQuery();

        if (!dbconOpened)
            Sqlite.Close();
    }
    public static void UpdateExerciseByName_old_do_not_use(bool dbconOpened, string nameOld, string name, int percentBodyWeight,
            string ressistance, string description, string speed1RM, Constants.EncoderGI type)
    {
        if (!dbconOpened)
            Sqlite.Open();

        dbcmd.CommandText = "UPDATE " + Constants.EncoderExerciseTable + " SET " +
                " name = '" + name +
                "', percentBodyWeight = " + percentBodyWeight +
                ", ressistance = '" + ressistance +
                "', description = '" + description +
                "', future1 = '" + speed1RM +
                "', type = '" + type.ToString() +
                "' WHERE name = '" + nameOld + "'";

        LogB.SQL(dbcmd.CommandText.ToString());
        dbcmd.ExecuteNonQuery();

        if (!dbconOpened)
            Sqlite.Close();
    }

    //if uniqueID != -1, returns an especific EncoderExercise that can be read like this	
    //EncoderExercise ex = (EncoderExercise) SqliteEncoder.SelectEncoderExercises(eSQL.exerciseID)[0];
    //if encoderGI == GRAVITATORY, return GRAVITATORY and ALL
    //if encoderGI == INERTIAL, return INERTIAL and ALL
    //if encoderGI == ALL, return everything

    //this is the regular call
    public static List<EncoderExercise> SelectEncoderExercises (bool dbconOpened, int uniqueID, bool onlyNames, Constants.EncoderGI encoderGI)
    {
	    return selectEncoderExercises (dbconOpened, uniqueID, onlyNames, encoderGI, dbcmd);
    }

    //called from SqlitePreferences.initializeTable passing the SQLiteCommand of a transaction
    public static List<EncoderExercise> SelectEncoderExercises (bool dbconOpened, int uniqueID, bool onlyNames, Constants.EncoderGI encoderGI, SQLiteCommand mycmd)
    {
	    return selectEncoderExercises (dbconOpened, uniqueID, onlyNames, encoderGI, mycmd);
    }

    private static List<EncoderExercise> selectEncoderExercises (bool dbconOpened, int uniqueID, bool onlyNames, Constants.EncoderGI encoderGI, SQLiteCommand mycmd)
    {
        if (!dbconOpened)
            Sqlite.Open();

        string encoderGIconnector = " WHERE ";

        string uniqueIDStr = "";
        if (uniqueID != -1)
        {
            uniqueIDStr = " WHERE " + Constants.EncoderExerciseTable + ".uniqueID = " + uniqueID;
            encoderGIconnector = " AND ";
        }

        string encoderGIstr = "";
        if (encoderGI == Constants.EncoderGI.GRAVITATORY)
            encoderGIstr = encoderGIconnector + " type != 'INERTIAL'";
        else if (encoderGI == Constants.EncoderGI.INERTIAL)
            encoderGIstr = encoderGIconnector + " type != 'GRAVITATORY'";

        if (onlyNames)
            mycmd.CommandText = "SELECT name FROM " + Constants.EncoderExerciseTable + uniqueIDStr + encoderGIstr;
        else
            mycmd.CommandText = "SELECT * FROM " + Constants.EncoderExerciseTable + uniqueIDStr + encoderGIstr;

        LogB.SQL(mycmd.CommandText.ToString());
        mycmd.ExecuteNonQuery();

        SQLiteDataReader reader;
        reader = mycmd.ExecuteReader();

	List<EncoderExercise> ex_l = new List<EncoderExercise> ();
        EncoderExercise ex = new EncoderExercise();

        if (onlyNames)
        {
            while (reader.Read())
                ex_l.Add (new EncoderExercise (reader[0].ToString()));
        }
        else
        {
            while (reader.Read())
            {
                double speed1RM = 0;
                if (reader[5].ToString() != "")
                    speed1RM = Convert.ToDouble(Util.ChangeDecimalSeparator(reader[5].ToString()));

                ex = new EncoderExercise(
                        Convert.ToInt32(reader[0].ToString()),  //uniqueID
                        reader[1].ToString(),           //name
                        Convert.ToInt32(reader[2].ToString()),  //percentBodyWeight
                        reader[3].ToString(),           //resistance
                        reader[4].ToString(),           //description
                        speed1RM,
                        (Constants.EncoderGI)Enum.Parse(typeof(Constants.EncoderGI), reader[8].ToString())
                        );
                ex_l.Add (ex);
            }
        }

        reader.Close();
        if (!dbconOpened)
            Sqlite.Close();

        return ex_l;
    }

    //gets a list of the exercises in curves to show them on encoder analyze tab
    //-1 if all sessions or all persons
    public static List<int> SelectAnalyzeExercisesInCurves(bool dbconOpened, int personID, int sessionID, Constants.EncoderGI encoderGI)
    {
        if (!dbconOpened)
            Sqlite.Open();

        string whereStr = " WHERE signalOrCurve = 'curve' ";

        if (personID != -1)
            whereStr += " AND " + Constants.EncoderTable + ".personID = " + personID;

        if (sessionID != -1)
            whereStr += " AND " + Constants.EncoderTable + ".sessionID = " + sessionID;

        dbcmd.CommandText = "SELECT exerciseID, encoderConfiguration FROM " + Constants.EncoderTable + whereStr +
            " ORDER BY exerciseID";

        LogB.SQL(dbcmd.CommandText.ToString());
        dbcmd.ExecuteNonQuery();

        SQLiteDataReader reader;
        reader = dbcmd.ExecuteReader();
        List<int> l = new List<int>();

        while (reader.Read())
        {
            //discard if != encoderGI
            string[] strFull = reader[1].ToString().Split(new char[] { ':' });
            EncoderConfiguration econf = new EncoderConfiguration(
                (EncoderConfiguration.Names)
                Enum.Parse(typeof(EncoderConfiguration.Names), strFull[0]));

            //if encoderGI != ALL discard non wanted repetitions
            if (encoderGI == Constants.EncoderGI.GRAVITATORY && econf.has_inertia)
                continue;
            else if (encoderGI == Constants.EncoderGI.INERTIAL && !econf.has_inertia)
                continue;

            int exID = Convert.ToInt32(reader[0].ToString());
            //Add to list l if not exists
            if (l.IndexOf(exID) == -1)
                l.Add(exID);
        }

        reader.Close();
        if (!dbconOpened)
            Sqlite.Close();

        return l;
    }

    public static ArrayList SelectEncoderSetsOfAnExercise(bool dbconOpened, int exerciseID)
    {
        if (!dbconOpened)
            Sqlite.Open();

        dbcmd.CommandText = "select count(*), " +
            Constants.PersonTable + ".name, " +
            Constants.SessionTable + ".name, " +
            Constants.SessionTable + ".date " +
            " FROM " + Constants.EncoderTable + ", " + Constants.PersonTable + ", " + Constants.SessionTable +
            " WHERE exerciseID = " + exerciseID +
                " AND signalOrCurve = 'signal' " +
            " AND " + Constants.PersonTable + ".uniqueID = " + Constants.EncoderTable + ".personID " +
                " AND " + Constants.SessionTable + ".uniqueID = " + Constants.EncoderTable + ".sessionID " +
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
        if (!dbconOpened)
            Sqlite.Close();

        return array;
    }

    //conversion from DB 0.99 to 1.00
    protected internal static void putEncoderExerciseAnglesAt90()
    {
        dbcmd.CommandText = "UPDATE " + Constants.EncoderExerciseTable +
            " SET future2 = 90, future3 = 90";

        LogB.SQL(dbcmd.CommandText.ToString());
        dbcmd.ExecuteNonQuery();
    }

    //conversion from DB 1.02 to 1.03
    protected internal static void removeEncoderExerciseAngles()
    {
        dbcmd.CommandText = "UPDATE " + Constants.EncoderExerciseTable +
            " SET future2 = '', future3 = ''";

        LogB.SQL(dbcmd.CommandText.ToString());
        dbcmd.ExecuteNonQuery();
    }
}
