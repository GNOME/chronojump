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
 * Copyright (C) 2020   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
//using System.Data;
using System.Collections;
using System.Collections.Generic; //List<T>
using Mono.Data.Sqlite;

class SqliteNews : Sqlite
{
	private static string table = Constants.NewsTable;

	public SqliteNews() {
	}
	
	~SqliteNews() {}
	
	/*
	 * create and initialize tables
	 */

	//server table is created like this:
	//create table news (code INT NOT NULL AUTO_INCREMENT PRIMARY KEY, category INT, version INT, versionDateTime TIMESTAMP DEFAULT CURRENT_TIMESTAMP, title char(255), linkWeb char(255), linkImage char(255), description char(255));
	//INSERT INTO news VALUES (NULL, 1, 0, current_timestamp(), "my title", "http://chronojump.org/test", "https://chronojump.org/wp-content/uploads/2019/08/Adaptadors-scaled.jpeg", "This is the description");


	protected internal static void createTable ()
	{
		dbcmd.CommandText = 
			"CREATE TABLE " + table + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"code INT, " +
			"category INT, " +
			"version INT, " +
			"versionDateTime TEXT, " +
			"viewed INT, " +
			"title TEXT, " +
			"link TEXT, " +
			"description TEXT )";
		dbcmd.ExecuteNonQuery();
	}

	public static int Insert (bool dbconOpened, string insertString)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "INSERT INTO " + table +
				" (uniqueID, code, category, version, versionDateTime, viewed, title, link, description)" +
				" VALUES (" + insertString + ")";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		string myString = @"select last_insert_rowid()";
		dbcmd.CommandText = myString;
		int myLast = Convert.ToInt32(dbcmd.ExecuteScalar()); // Need to type-cast since `ExecuteScalar` returns an object.

		closeIfNeeded(dbconOpened);

		return myLast;
	}

}

