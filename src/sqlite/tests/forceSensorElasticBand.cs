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

class SqliteForceSensorElasticBand : Sqlite
{
    private static string table = Constants.ForceSensorElasticBandTable;

    public SqliteForceSensorElasticBand()
    {
    }

    ~SqliteForceSensorElasticBand() { }

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
            "active INT, " +    //0 inactive, 3 using 3 like this now
            "brand TEXT, " +
            "color TEXT, " +
            "stiffness FLOAT, " +
            "comments TEXT)";
        LogB.SQL(dbcmd.CommandText.ToString());
        dbcmd.ExecuteNonQuery();
    }

    public static int Insert(bool dbconOpened, ForceSensorElasticBand eb)
    {
        if (!dbconOpened)
            Sqlite.Open();

        dbcmd.CommandText = "INSERT INTO " + table +
                " (uniqueID, active, brand, color, stiffness, comments)" +
                " VALUES (" + eb.ToSQLInsertString() + ")";
        LogB.SQL(dbcmd.CommandText.ToString());
        dbcmd.ExecuteNonQuery();

        string myString = @"select last_insert_rowid()";
        dbcmd.CommandText = myString;
        int myLast = Convert.ToInt32(dbcmd.ExecuteScalar()); // Need to type-cast since `ExecuteScalar` returns an object.

        if (!dbconOpened)
            Sqlite.Close();

        return myLast;
    }

    public static void Update(bool dbconOpened, ForceSensorElasticBand eb)
    {
        openIfNeeded(dbconOpened);

        dbcmd.CommandText = "UPDATE " + table + " SET " +
            " active = " + eb.Active.ToString() +
            ", brand = '" + eb.Brand +
            "', color = '" + eb.Color +
            "', stiffness = " + Util.ConvertToPoint(eb.Stiffness) +
            ", comments = '" + eb.Comments +
            "' WHERE uniqueID = " + eb.UniqueID;

        LogB.SQL(dbcmd.CommandText.ToString());
        dbcmd.ExecuteNonQuery();

        closeIfNeeded(dbconOpened);
    }
    public static void UpdateList(bool dbconOpened, List<ForceSensorElasticBand> list_fseb)
    {
        openIfNeeded(dbconOpened);

        foreach (ForceSensorElasticBand fseb in list_fseb)
            Update(true, fseb);

        closeIfNeeded(dbconOpened);
    }

    public static void Delete(bool dbconOpened, int uniqueID)
    {
        openIfNeeded(dbconOpened);

        dbcmd.CommandText = "DELETE FROM " + table + " WHERE uniqueID = " + uniqueID;

        LogB.SQL(dbcmd.CommandText.ToString());
        dbcmd.ExecuteNonQuery();

        closeIfNeeded(dbconOpened);
    }

    public static List<ForceSensorElasticBand> SelectAll(bool dbconOpened, bool onlyActive)
    {
        openIfNeeded(dbconOpened);

        dbcmd.CommandText = "SELECT * FROM " + table;
        if (onlyActive)
            dbcmd.CommandText += " WHERE active > 0";

        LogB.SQL(dbcmd.CommandText.ToString());

        dbcmd.ExecuteNonQuery();

        SQLiteDataReader reader = dbcmd.ExecuteReader();

        List<ForceSensorElasticBand> list_fseb = new List<ForceSensorElasticBand>();

        while (reader.Read())
        {
            ForceSensorElasticBand fseb = new ForceSensorElasticBand(
                    Convert.ToInt32(reader[0].ToString()),  //uniqueID
                    Convert.ToInt32(reader[1].ToString()),  //active
                    reader[2].ToString(),           //brand
                    reader[3].ToString(),           //color
                    Convert.ToDouble(Util.ChangeDecimalSeparator(reader[4].ToString())),
                    reader[5].ToString()            //comments
                    );
            list_fseb.Add(fseb);
        }

        reader.Close();
        closeIfNeeded(dbconOpened);

        return list_fseb;
    }

    public static List<string> SelectSessionNamesWithCapturesWithElasticBand(int elasticBandID)
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

        SQLiteDataReader reader;
        reader = dbcmd.ExecuteReader();

        List<string> sessionsWithThisEB = new List<string>();

        while (reader.Read())
        {
            //if we already have this session on list, continue
            string sessionName = reader[0].ToString();
            foreach (string s in sessionsWithThisEB)
                if (s == sessionName)
                    continue;

            string stiffnessString = reader[1].ToString();
            string[] stiffPairs = stiffnessString.Split(new char[] { ';' });
            foreach (string str in stiffPairs)
            {
                string[] strPair = str.Split(new char[] { '*' });
                if (Util.IsNumber(strPair[0], false) && Convert.ToInt32(strPair[0]) == elasticBandID)
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
    public static double GetStiffnessOfACapture(bool dbconOpened, string stiffnessString)
    {
        //return 0 if empty
        if (stiffnessString == "")
            return 0;

        string[] strFull = stiffnessString.Split(new char[] { ';' });
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

        return getStiffnessOfACaptureDo(dbconOpened, strFull);
    }
    private static double getStiffnessOfACaptureDo(bool dbconOpened, string[] stiffnessStrArray)
    {
        openIfNeeded(dbconOpened);

        /*
		 * instead of doing a select for each of the members of stiffnessArray (slow),
		 * do a select of all and the filter what is not on the array
		 */

        dbcmd.CommandText = "SELECT uniqueID, stiffness FROM " + table;
        LogB.SQL(dbcmd.CommandText.ToString());
        dbcmd.ExecuteNonQuery();

        SQLiteDataReader reader = dbcmd.ExecuteReader();

        double sum = 0;
        while (reader.Read())
        {
            string id = reader[0].ToString();
            foreach (string str in stiffnessStrArray)
            {
                string[] strFull = str.Split(new char[] { '*' });
                if (strFull[0] == id)
                    sum += Convert.ToDouble(Util.ChangeDecimalSeparator(reader[1].ToString())) * Convert.ToInt32(strFull[1]);
            }
        }

        reader.Close();
        closeIfNeeded(dbconOpened);

        return sum;
    }
}


