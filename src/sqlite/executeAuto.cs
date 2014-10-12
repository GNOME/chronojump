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
 * Copyright (C) 2004-2014   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using System.IO;
using System.Collections; //ArrayList
using System.Collections.Generic; //List<T>
using Mono.Data.Sqlite;

class SqliteExecuteAuto : Sqlite
{
	public SqliteExecuteAuto() {
	}
	
	~SqliteExecuteAuto() {}

	/*
	 * create and initialize tables
	 */
	
	protected internal static void createTableExecuteAuto()
	{
		dbcmd.CommandText = 
			"CREATE TABLE " + Constants.ExecuteAutoTable + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"name TEXT, " +	
			"mode TEXT, " +	
			"description TEXT, " +	
			"serie1IDs TEXT, " +	
			"serie2IDs TEXT, " +	
			"serie3IDs TEXT, " +	
		       	"future1 TEXT, " +
		       	"future2 TEXT, " + 
		       	"future3 TEXT )";
		dbcmd.ExecuteNonQuery();
	}
	
	/*
	 * class methods
	 */
	
	public static void Insert(bool dbconOpened, string name, string mode, string description, string serie1IDs, string serie2IDs, string serie3IDs)
	{
		if(! dbconOpened)
			dbcon.Open();

		dbcmd.CommandText = "INSERT INTO " + Constants.ExecuteAutoTable +  
			" (uniqueID, name, mode, description, " +
			" serie1IDs, serie2IDs, serie3IDs, " + 
			" future1, future2, future3)" +
			" VALUES ( NULL, '" +
			name + "', '" + mode + "', '" + description + "', '" +
			serie1IDs + "', '" + serie2IDs + "', '" + serie3IDs + "', " + 
			"'', '', '')"; //future1, future2, future3
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		if(! dbconOpened)
			dbcon.Close();
	}

	public static List<ExecuteAutoSQL> SelectAll(bool dbconOpened) 
	//public static ArrayList SelectAll(bool dbconOpened) 
	{
		if(! dbconOpened)
			dbcon.Open();

		dbcmd.CommandText = "SELECT * from " + Constants.ExecuteAutoTable; 
		Log.WriteLine(dbcmd.CommandText.ToString());

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		
		List<ExecuteAutoSQL> sequences = new List<ExecuteAutoSQL>();
		//ArrayList sequences = new ArrayList();
		int i;
		while(reader.Read()) {
			i=1; //start at name
			ExecuteAutoSQL eaSQL = new ExecuteAutoSQL(
					reader[i++].ToString(), //name
					(ExecuteAuto.ModeTypes) Enum.Parse(typeof(ExecuteAuto.ModeTypes), reader[i++].ToString()), //mode
					reader[i++].ToString(), //description
					ExecuteAutoSQL.SerieIDsFromStr(reader[i++].ToString()), //serie1IDs
					ExecuteAutoSQL.SerieIDsFromStr(reader[i++].ToString()), //serie2IDs
					ExecuteAutoSQL.SerieIDsFromStr(reader[i++].ToString())  //serie3IDs
					);
			sequences.Add(eaSQL);
		}
		reader.Close();
		if(! dbconOpened)
			dbcon.Close();

		return sequences;
	}
}	
