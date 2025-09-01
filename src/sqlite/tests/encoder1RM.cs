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

class SqliteEncoder1RM : Sqlite
{
    protected internal static void createTable1RM()
    {
        dbcmd.CommandText =
            "CREATE TABLE " + Constants.Encoder1RMTable + " ( " +
            "uniqueID INTEGER PRIMARY KEY, " +
            "personID INT, " +
            "sessionID INT, " +
            "exerciseID INT, " +
            "load1RM FLOAT, " +
            "future1 TEXT, " +
            "future2 TEXT, " +
            "future3 TEXT )";
        dbcmd.ExecuteNonQuery();
    }

    public static int Insert1RM(bool dbconOpened, int personID, int sessionID, int exerciseID, double load1RM)
    {
        if (!dbconOpened)
            Sqlite.Open();

        dbcmd.CommandText = "INSERT INTO " + Constants.Encoder1RMTable +
                " (uniqueID, personID, sessionID, exerciseID, load1RM, future1, future2, future3)" +
                " VALUES (NULL, " + personID + ", " + sessionID + ", " +
                exerciseID + ", " + Util.ConvertToPoint(load1RM) + ", '','','')";
        LogB.SQL(dbcmd.CommandText.ToString());
        dbcmd.ExecuteNonQuery();

        //int myLast = dbcon.LastInsertRowId;
        //http://stackoverflow.com/questions/4341178/getting-the-last-insert-id-with-sqlite-net-in-c
        string myString = @"select last_insert_rowid()";
        dbcmd.CommandText = myString;
        int myLast = Convert.ToInt32(dbcmd.ExecuteScalar()); // Need to type-cast since `ExecuteScalar` returns an object.

        if (!dbconOpened)
            Sqlite.Close();

        return myLast;
    }

    public static ArrayList Select1RM(bool dbconOpened, int personID, int sessionID, int exerciseID, bool returnPersonNameAndExerciseName)
    {
        if (!dbconOpened)
            Sqlite.Open();

        string whereStr = "";
        if (personID != -1 || sessionID != -1 || exerciseID != -1)
        {
            whereStr = " WHERE ";
            string andStr = "";

            if (personID != -1)
            {
                whereStr += " " + Constants.Encoder1RMTable + ".personID = " + personID;
                andStr = " AND ";
            }

            if (sessionID != -1)
            {
                whereStr += andStr + " " + Constants.Encoder1RMTable + ".sessionID = " + sessionID;
                andStr = " AND ";
            }

            if (exerciseID != -1)
                whereStr += andStr + " " + Constants.Encoder1RMTable + ".exerciseID = " + exerciseID;
        }

        if (returnPersonNameAndExerciseName)
        {
            if (whereStr == "")
                whereStr = " WHERE ";
            else
                whereStr += " AND ";
            whereStr += Constants.Encoder1RMTable + ".personID = person77.uniqueID AND " +
                Constants.Encoder1RMTable + ".exerciseID = encoderExercise.uniqueID";
        }

        if (returnPersonNameAndExerciseName)
            dbcmd.CommandText = "SELECT " + Constants.Encoder1RMTable + ".*, person77.name, encoderExercise.name, session.date" +
                " FROM " + Constants.Encoder1RMTable + ", person77, encoderExercise, session " +
                whereStr + " AND " + Constants.Encoder1RMTable + ".sessionID = session.uniqueID " +
                " ORDER BY uniqueID DESC"; //this allows to select the last uniqueID because will be the first in the returned array 
        else
            dbcmd.CommandText = "SELECT " + Constants.Encoder1RMTable + ".*, session.date FROM " +
                Constants.Encoder1RMTable + ", session" + whereStr +
                " ORDER BY uniqueID DESC"; //this allows to select the last uniqueID because will be the first in the returned array 

        LogB.SQL(dbcmd.CommandText.ToString());

        SQLiteDataReader reader;
        reader = dbcmd.ExecuteReader();

        ArrayList array = new ArrayList(1);

        Encoder1RM e1RM = new Encoder1RM();
        while (reader.Read())
        {
            if (returnPersonNameAndExerciseName)
                e1RM = new Encoder1RM(
                        Convert.ToInt32(reader[0].ToString()),  //uniqueID
                        Convert.ToInt32(reader[1].ToString()),  //personID	
                        Convert.ToInt32(reader[2].ToString()),  //sessionID
                        UtilDate.FromSQL(reader[10].ToString()),//date
                        Convert.ToInt32(reader[3].ToString()),  //exerciseID
                        Convert.ToDouble(Util.ChangeDecimalSeparator(reader[4].ToString())),  //load1RM
                        reader[8].ToString(),   //personName
                        reader[9].ToString()    //exerciseName
                        );
            else
                e1RM = new Encoder1RM(
                        Convert.ToInt32(reader[0].ToString()),  //uniqueID
                        Convert.ToInt32(reader[1].ToString()),  //personID	
                        Convert.ToInt32(reader[2].ToString()),  //sessionID
                        UtilDate.FromSQL(reader[5].ToString()),     //date
                        Convert.ToInt32(reader[3].ToString()),  //exerciseID
                        Convert.ToDouble(Util.ChangeDecimalSeparator(reader[4].ToString()))  //load1RM
                        );
            array.Add(e1RM);
        }
        reader.Close();
        if (!dbconOpened)
            Sqlite.Close();

        return array;
    }
}
