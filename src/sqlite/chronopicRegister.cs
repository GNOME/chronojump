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
 * Copyright (C) 2004-2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
//using System.Data;
using System.Collections.Generic; //List<T>
using Mono.Data.Sqlite;

class SqliteChronopicRegister : Sqlite
{
	private static string table = Constants.ChronopicRegisterTable;

	public SqliteChronopicRegister() {
	}
	
	~SqliteChronopicRegister() {}
	
	/*
	 * create and initialize tables
	 */
	
	
	protected internal static void createTableChronopicRegister()
	{
		dbcmd.CommandText = 
			"CREATE TABLE " + table + " ( " +
			"serialNumber TEXT, " +
			"type TEXT )";
		dbcmd.ExecuteNonQuery();
	}
	
	public static List<ChronopicRegisterPort> SelectAll (bool dbconOpened, bool showArduinoRFID, bool showRunWireless)
	{
		openIfNeeded(dbconOpened);
		
		dbcmd.CommandText = "SELECT * FROM " + table;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader = dbcmd.ExecuteReader();
	
		List<ChronopicRegisterPort> l = new List<ChronopicRegisterPort>();
		while(reader.Read()) {
			ChronopicRegisterPort crp = new ChronopicRegisterPort(
					reader[0].ToString(), //serialNumber
					(ChronopicRegisterPort.Types) Enum.Parse(
						typeof(ChronopicRegisterPort.Types), reader[1].ToString()) //type
					);

			if(! showArduinoRFID && crp.Type == ChronopicRegisterPort.Types.ARDUINO_RFID)
				crp.Type = ChronopicRegisterPort.Types.UNKNOWN;

			if(! showRunWireless && crp.Type == ChronopicRegisterPort.Types.RUN_WIRELESS)
				crp.Type = ChronopicRegisterPort.Types.UNKNOWN;

			l.Add(crp);
		}

		reader.Close();
		closeIfNeeded(dbconOpened);

		return l;
	}

	public static bool Exists (bool dbconOpened, string serialNumber)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "SELECT * FROM " + table + " WHERE serialNumber = \"" + serialNumber + "\"";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader = dbcmd.ExecuteReader();

		bool found = false;
		if (reader.Read ())
			found = true;

		reader.Close();
		closeIfNeeded(dbconOpened);

		return found;
	}

	public static void Insert(bool dbconOpened, ChronopicRegisterPort crp)
	{
		openIfNeeded(dbconOpened);
		
		dbcmd.CommandText = "INSERT INTO " + table + 
			" (serialNumber, type) VALUES (\"" + 
			crp.SerialNumber + "\", \"" + crp.Type.ToString() + "\")" ;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		closeIfNeeded(dbconOpened);
	}
	
	public static void Update(bool dbconOpened, ChronopicRegisterPort crp, ChronopicRegisterPort.Types newType)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "UPDATE " + table +
			" SET type = \"" + newType.ToString() + 
			"\" WHERE serialNumber = \"" + crp.SerialNumber + "\"" ;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		closeIfNeeded(dbconOpened);
	}
	
	public static void Delete(bool dbconOpened, ChronopicRegisterPort crp)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "Delete FROM " + table +
			" WHERE serialNumber = " + crp.SerialNumber;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		closeIfNeeded(dbconOpened);
	}

}

