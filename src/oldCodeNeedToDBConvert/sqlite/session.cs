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
 * Copyright (C) 2004-2015   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using System.IO;
using System.Collections; //ArrayList
using Mono.Data.Sqlite;
using Mono.Unix;


class SqliteSessionOld : Sqlite
{
	public SqliteSessionOld() {
	}
	
	~SqliteSessionOld() {}


	/* OLD STUFF */
	/* 
	 * don't do more like this, use Sqlite.convertTables()
	 */
	//change DB from 0.55 to 0.56
	protected internal static void convertTableAddingSportStuff() 
	{
		ArrayList myArray = new ArrayList(2);

		//1st create a temp table
		//createTable(Constants.ConvertTempTable);
		SqliteSessionOld sqliteSessionObject = new SqliteSessionOld();
		sqliteSessionObject.createTable(Constants.ConvertTempTable);
			
		//2nd copy all data from session table to temp table
		dbcmd.CommandText = "SELECT * " + 
			"FROM " + Constants.SessionTable + " ORDER BY uniqueID"; 
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		while(reader.Read()) {
			Session mySession = new Session(reader[0].ToString(), reader[1].ToString(), 
					reader[2].ToString(), UtilDate.FromSql(reader[3].ToString()), 
					1, //sport undefined
					-1, //speciallity undefined
					-1, //practice level undefined
					reader[4].ToString(), //comments
					Constants.ServerUndefinedID
					); 
			myArray.Add(mySession);
		}
		reader.Close();

		foreach (Session mySession in myArray)
			InsertOld(true, Constants.ConvertTempTable,
				mySession.Name, mySession.Place, UtilDate.ToSql(mySession.Date), 
				mySession.PersonsSportID, mySession.PersonsSpeciallityID, mySession.PersonsPractice, mySession.Comments);

		//3rd drop table sessions
		Sqlite.dropTable(Constants.SessionTable);

		//4d create table persons (now with sport related stuff
		//createTable(Constants.SessionTable);
		sqliteSessionObject.createTable(Constants.SessionTable);

		//5th insert data in sessions (with sport related stuff)
		foreach (Session mySession in myArray) 
			InsertOld(true, Constants.SessionTable,
				mySession.Name, mySession.Place, UtilDate.ToSql(mySession.Date), 
				mySession.PersonsSportID, mySession.PersonsSpeciallityID, mySession.PersonsPractice, mySession.Comments);


		//6th drop temp table
		Sqlite.dropTable(Constants.ConvertTempTable);
	}
	
	/* used only on conversion from 0.55 to 0.56 */
	public static int InsertOld(bool dbconOpened, string tableName, string name, string place, string date, int personsSportID, int personsSpeciallityID, int personsPractice, string comments)
	{
		if(! dbconOpened)
			dbcon.Open();

		dbcmd.CommandText = "INSERT INTO " + tableName + " (uniqueID, name, place, date, personsSportID, personsSpeciallityID, personsPractice, comments)" +
			" VALUES (NULL, '"
			+ name + "', '" + place + "', '" + date + "', " + 
			personsSportID + ", " + personsSpeciallityID + ", " + 
			personsPractice + ", '" + comments + "')" ;
		dbcmd.ExecuteNonQuery();
		int myReturn = -10000; //dbcon.LastInsertRowId;
		
		if(! dbconOpened)
			dbcon.Close();

		return myReturn;
	}
}
