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

class SqliteEncoderSignalCurve : Sqlite
{

    protected internal static void createTableEncoderSignalCurve()
    {
        dbcmd.CommandText =
            "CREATE TABLE " + Constants.EncoderSignalCurveTable + " ( " +
            "uniqueID INTEGER PRIMARY KEY, " +
            "signalID INT, " +
            "curveID INT, " +
            "msCentral INT, " +
            "future1 TEXT )"; //right now unused. need future2, future3. Better to use alter table here and on encoder table
        dbcmd.ExecuteNonQuery();
    }

    public static void SignalCurveInsert(bool dbconOpened, int signalID, int curveID, int msCentral)
    {
        if (!dbconOpened)
            Sqlite.Open();

        dbcmd.CommandText = "INSERT INTO " + Constants.EncoderSignalCurveTable +
            " (uniqueID, signalID, curveID, msCentral, future1) " +
            "VALUES (NULL, " + signalID + ", " + curveID + ", " + msCentral + ", '')";
        LogB.SQL(dbcmd.CommandText.ToString());
        dbcmd.ExecuteNonQuery();

        if (!dbconOpened)
            Sqlite.Close();
    }


    //signalID == -1 (any signal)
    //curveID == -1 (any curve)
    //if msStart and msEnd != -1 (means find a curve with msCentral contained between both values)
    public static ArrayList SelectSignalCurve(bool dbconOpened, int signalID, int curveID, double msStart, double msEnd)
    {
        if (!dbconOpened)
            Sqlite.Open();

        string whereStr = "";
        if (signalID != -1 || curveID != -1 || msStart != -1)
            whereStr = " WHERE ";

        string signalIDstr = "";
        if (signalID != -1)
            signalIDstr = " signalID = " + signalID;

        string curveIDstr = "";
        if (curveID != -1)
        {
            curveIDstr = " curveID = " + curveID;
            if (signalID != -1)
                curveIDstr = " AND" + curveIDstr;
        }

        string msCentralstr = "";
        if (msStart != -1)
        {
            msCentralstr = " msCentral >= " + Util.ConvertToPoint(msStart) + " AND msCentral <= " + Util.ConvertToPoint(msEnd);
            if (signalID != -1 || curveID != -1)
                msCentralstr = " AND" + msCentralstr;
        }

        dbcmd.CommandText =
            "SELECT uniqueID, signalID, curveID, msCentral " +
            " FROM " + Constants.EncoderSignalCurveTable +
            whereStr + signalIDstr + curveIDstr + msCentralstr;

        LogB.SQL(dbcmd.CommandText.ToString());

        SQLiteDataReader reader;
        reader = dbcmd.ExecuteReader();

        ArrayList array = new ArrayList();
        while (reader.Read())
        {
            EncoderSignalCurve esc = new EncoderSignalCurve(
                    Convert.ToInt32(reader[0].ToString()),
                    Convert.ToInt32(reader[1].ToString()),
                    Convert.ToInt32(reader[2].ToString()),
                    Convert.ToInt32(reader[3].ToString()));

            array.Add(esc);
        }
        reader.Close();
        if (!dbconOpened)
            Sqlite.Close();

        return array;
    }

    public static void DeleteSignalCurveWithCurveID(bool dbconOpened, int curveID)
    {
        if (!dbconOpened)
            Sqlite.Open();

        dbcmd.CommandText = "Delete FROM " + Constants.EncoderSignalCurveTable +
            " WHERE curveID = " + curveID.ToString();
        LogB.SQL(dbcmd.CommandText.ToString());
        dbcmd.ExecuteNonQuery();

        if (!dbconOpened)
            Sqlite.Close();
    }
}
