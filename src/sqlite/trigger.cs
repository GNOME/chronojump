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

class SqliteTrigger : Sqlite
{
	private static string table = Constants.TriggerTable;

	public SqliteTrigger() {
	}
	
	~SqliteTrigger() {}
	
	/*
	 * create and initialize tables
	 */
	
	
	protected internal static void createTableTrigger()
	{
		dbcmd.CommandText = 
			"CREATE TABLE " + table + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"mode TEXT, " + 	//encoder; gauge
			"modeID INT, " + 	//on encoder: uniqueID
			"ms INT, " +		//on encoder are milliseconds, on the rest are microseconds!!!
			"inOut INT, " + 	//bool
			"name TEXT, " +
			"color TEXT, " +
			"comments TEXT )";
		dbcmd.ExecuteNonQuery();
	}
	
	public static List<Trigger> Select (bool dbconOpened, Trigger.Modes mode, int modeID)
	{
		openIfNeeded(dbconOpened);
		
		dbcmd.CommandText = "SELECT * FROM " + table +
			" WHERE mode = \"" + mode + "\" AND modeID = " + modeID;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader = dbcmd.ExecuteReader();
	
		List<Trigger> l = new List<Trigger>();
		while(reader.Read()) {
			Trigger trigger = new Trigger(
					Convert.ToInt32(reader[0]), 		//uniqueID
					(Trigger.Modes) Enum.Parse(
						typeof(Trigger.Modes), reader[1].ToString()), //mode
					Convert.ToInt32(reader[2]), 		//modeID
					Convert.ToInt32(reader[3]), 		//milliseconds or microseconds
					Util.IntToBool(Convert.ToInt32(reader[4])), 	//inOut
					reader[5].ToString(), 			//name
					reader[6].ToString(), 			//color
					reader[7].ToString()			//comments
					);
			l.Add(trigger);
		}

		reader.Close();
		closeIfNeeded(dbconOpened);

		return l;
	}

	public static void InsertList(bool dbconOpened, List<Trigger> l)
	{
		openIfNeeded(dbconOpened);
	
		foreach(Trigger trigger in l)
		{
			dbcmd.CommandText = "INSERT INTO " + table + 
				" (uniqueID, mode, modeID, ms, inOut, name, color, comments) VALUES (" +	//microseconds
				trigger.ToSQLInsertString() + ")";
			LogB.SQL(dbcmd.CommandText.ToString());
			dbcmd.ExecuteNonQuery();
		}
		
		closeIfNeeded(dbconOpened);
	}

	//on mode == ENCODER, modeID is encoder.uniqueID
	public static void DeleteByModeID(bool dbconOpened, Trigger.Modes mode, int modeID)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "Delete FROM " + table +
			" WHERE mode = \"" + mode.ToString() +
			"\" AND modeID = " + modeID;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);
	}

	/*
	public static void Delete(bool dbconOpened, Trigger trigger)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "Delete FROM " + table + " WHERE uniqueID = " + trigger.UniqueID;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		closeIfNeeded(dbconOpened);
	}
	*/
}

