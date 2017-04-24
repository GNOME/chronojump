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

class SqliteForceSensor : Sqlite
{
	private static string table = Constants.ForceRFDTable;

	public SqliteForceSensor() {
	}

	~SqliteForceSensor() {}

	/*
	 * create and initialize tables
	 */

	protected internal static void createTableForceRFD()
	{
		dbcmd.CommandText = 
			"CREATE TABLE " + table + " ( " +
			"code TEXT, " + 	//RFD1...4
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

		dbcmd.CommandText = "SELECT * FROM " + table;
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

}
