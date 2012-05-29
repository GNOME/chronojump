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
 * Copyright (C) 2004-2012   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using System.IO;
using System.Collections; //ArrayList
using Mono.Data.Sqlite;


class SqliteEncoder : Sqlite
{
	public SqliteEncoder() {
	}
	
	~SqliteEncoder() {}

	/*
	 * create and initialize tables
	 */
	
	protected internal static void createTable()
	{
		dbcmd.CommandText = 
			"CREATE TABLE " + Constants.EncoderTable + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"personID INT, " +
			"sessionID INT, " +
			"name TEXT, " +
			"url TEXT, " +
			"type TEXT, " +		//"streamBAR", "streamJUMP", "curveBAR", "curveJUMP"
			"extraWeight TEXT, " +	//string because can contain "33%" or "50Kg"
			"eccon TEXT, " +	//"c" or "ec"
			"time INT, " +
			"minHeight INT, " +
			"smooth FLOAT, " +  
			"description TEXT )";
		dbcmd.ExecuteNonQuery();
	}
	
	
	/*
	 * Encoder class methods
	 */
	
	public static int Insert(bool dbconOpened, string uniqueID, int personID, int sessionID, string name, string url, string type, string extraWeight, string eccon, int time, int minHeight, double smooth, string description)
	{
		if(! dbconOpened)
			dbcon.Open();

		if(uniqueID == "-1")
			uniqueID = "NULL";

		dbcmd.CommandText = "INSERT INTO " + Constants.EncoderTable +  
				" (uniqueID, personID, sessionID, name, url, type, extraWeight, eccon, time, minHeight, smooth, description)" +
				" VALUES (" + uniqueID + ", "
				+ personID + ", " + sessionID + ", '" + name + "', '" + url + "', '" + type + "', '" 
				+ extraWeight + "', '" + eccon + "', " + time + ", " + minHeight + ", " 
				+ Util.ConvertToPoint(smooth) + ", '" + description + "')" ;
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		//int myLast = dbcon.LastInsertRowId;
		//http://stackoverflow.com/questions/4341178/getting-the-last-insert-id-with-sqlite-net-in-c
		string myString = @"select last_insert_rowid()";
		dbcmd.CommandText = myString;
		int myLast = Convert.ToInt32(dbcmd.ExecuteScalar()); // Need to type-cast since `ExecuteScalar` returns an object.

		if(! dbconOpened)
			dbcon.Close();

		return myLast;
	}
	

}
