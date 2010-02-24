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
 * Copyright (C) 2004-2009   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using System.IO;
using System.Collections; //ArrayList
using Mono.Data.Sqlite;


class SqlitePersonOld : Sqlite
{
	public SqlitePersonOld() {
	}
	
	~SqlitePersonOld() {}

	//can be "Constants.PersonOldTable" or "Constants.ConvertTempTable"
	//temp is used to modify table between different database versions if needed
	//protected new internal static void createTable(string tableName)
	protected override void createTable(string tableName)
	 {
		dbcmd.CommandText = 
			"CREATE TABLE " + tableName + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"name TEXT, " +
			"sex TEXT, " +
			"dateborn TEXT, " + //YYYY-MM-DD since db 0.72
			"height FLOAT, " +
			"weight FLOAT, " + //now used personSession and person can change weight in every session. person.weight is not used
			"sportID INT, " + 
			"speciallityID INT, " + 
			"practice INT, " + //also called "level"
			"description TEXT, " +	
			"race INT, " + 
			"countryID INT, " + 
			"serverUniqueID INT ) ";
		dbcmd.ExecuteNonQuery();
	 }

	//can be "Constants.PersonOldTable" or "Constants.ConvertTempTable"
	//temp is used to modify table between different database versions if needed
	public static int Insert(bool dbconOpened, string tableName, string uniqueID, string name, string sex, DateTime dateBorn, double height, double weight, int sportID, int speciallityID, int practice, string description, int race, int countryID, int serverUniqueID)
	{
		if(! dbconOpened)
			dbcon.Open();

		if(uniqueID == "-1")
			uniqueID = "NULL";

		string myString = "INSERT INTO " + tableName + 
			" (uniqueID, name, sex, dateBorn, height, weight,  sportID, speciallityID, practice, description, race, countryID, serverUniqueID) VALUES (" + uniqueID + ", '" +
			name + "', '" + sex + "', '" + UtilDate.ToSql(dateBorn) + "', " + 
			Util.ConvertToPoint(height) + ", " + "-1" + ", " + //"-1" is weight because it's defined in personSesionWeight for allow change between sessions
			sportID + ", " + speciallityID + ", " + practice + ", '" + description + "', " + 
			race + ", " + countryID + ", " + serverUniqueID + ")" ;
		
		dbcmd.CommandText = myString;
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		int myReturn = dbcon.LastInsertRowId;

		if(! dbconOpened)
			dbcon.Close();

		return myReturn;
	}
		
	/* 
	   from SqlitePersonSessionWeight.DeletePersonFromSessionAndTests()
	   if person is not in other sessions, delete it from DB
	 */
	public static void Delete(int uniqueID)
	{
		dbcmd.CommandText = "Delete FROM " + Constants.PersonOldTable +
			" WHERE uniqueID == " + uniqueID.ToString();
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}

}
