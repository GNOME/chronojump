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
using System.Collections.Generic; //List<T>
using Mono.Data.Sqlite;

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
			"ressistance TEXT, " +
			"angle INT, " +
			"description TEXT )";
		dbcmd.ExecuteNonQuery();
	}

	//undefined defaultAngle will be 1000
	//note execution can have a different angle than the default angle
	public static void Insert (bool dbconOpened, int uniqueID, string name, int percentBodyWeight,
			string ressistance, int angleDefault, string description)
	{
		if(! dbconOpened)
			Sqlite.Open();

		string uniqueIDStr = "NULL";
		if(uniqueID != -1)
			uniqueIDStr = uniqueID.ToString();

		dbcmd.CommandText = "INSERT INTO " + table +
				" (uniqueID, name, percentBodyWeight, ressistance, angleDefault, description)" +
				" VALUES (" + uniqueIDStr + ", \"" + name + "\", " + percentBodyWeight + ", \"" +
				ressistance + "\", " + angleDefault + ", \"" + description + "\")";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		if(! dbconOpened)
			Sqlite.Close();
	}

}

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
