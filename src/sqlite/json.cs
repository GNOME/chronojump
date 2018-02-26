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
 * Copyright (C) 2018   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
//using System.Collections.Generic; //List<T>
using Mono.Data.Sqlite;

class SqliteJson : Sqlite
{
	private static string tableEncoder = Constants.UploadEncoderDataTempTable;
	private static string tableSprint = Constants.UploadSprintDataTempTable;

	public SqliteJson() {
	}

	~SqliteJson() {}

	/*
	 * create and initialize tables
	 */

	/*
	 * ENCODER
	 */	

	protected internal static void createTableUploadEncoderDataTemp()
	{
		dbcmd.CommandText = 
			"CREATE TABLE " + tableEncoder + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"personId INT, " +
			"stationId INT, " +
			"exerciseId INT, " +
			"laterality TEXT, " +
			"resistance TEXT, " +
			"repetitions INT, " +
			//BySpeed stuff
			"numBySpeed INT, " +
			"lossBySpeed INT, " +
			"rangeBySpeed TEXT, " +
			"vmeanBySpeed TEXT, " +
			"vmaxBySpeed TEXT, " +
			"pmeanBySpeed TEXT, " +
			"pmaxBySpeed TEXT, " +
			//ByPower stuff
			"numByPower INT, " +
			"lossByPower INT, " +
			"rangeByPower TEXT, " +
			"vmeanByPower TEXT, " +
			"vmaxByPower TEXT, " +
			"pmeanByPower TEXT, " +
			"pmaxByPower TEXT )";
		dbcmd.ExecuteNonQuery();
	}

	public static void InsertTempEncoder (bool dbconOpened, UploadEncoderDataFullObject o)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "INSERT INTO " + tableEncoder + 
			" (uniqueID, personID, stationID, exerciseID, laterality, resistance, " +
			"repetitions, " +
			"numBySpeed, lossBySpeed, rangeBySpeed, vmeanBySpeed, vmaxBySpeed, pmeanBySpeed, pmaxBySpeed, " +
			"numByPower, lossByPower, rangeByPower, vmeanByPower, vmaxByPower, pmeanByPower, pmaxByPower) " +
			" VALUES (" +
			o.ToSQLInsertString();
		LogB.SQL(dbcmd.CommandText.ToString());

		dbcmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);
	}


	/*
	 * SPRINT
	 */	

	protected internal static void createTableUploadSprintDataTemp()
	{
		dbcmd.CommandText = 
			"CREATE TABLE " + tableSprint + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"personId INT, " +
			"sprintPositions TEXT, " +
			"splitTimes TEXT, " +
			"k FLOAT, " +
			"vmax FLOAT, " +
			"amax FLOAT, " +
			"fmax FLOAT, " +
			"pmax FLOAT )";
		dbcmd.ExecuteNonQuery();
	}

	public static void InsertTempSprint (bool dbconOpened, UploadSprintDataObject o)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "INSERT INTO " + tableSprint + 
			" (uniqueID, personID, sprintPositions, splitTimes, " +
			" k, vmax, amax, fmax, pmax) VALUES (" +
			o.ToSQLInsertString();
		LogB.SQL(dbcmd.CommandText.ToString());

		dbcmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);
	}

}

