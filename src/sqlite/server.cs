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
 * Xavier de Blas: 
 * http://www.xdeblas.com, http://www.deporteyciencia.com (parleblas)
 */

using System;
using System.Data;
using System.IO;
using System.Collections; //ArrayList
//using Mono.Data.SqliteClient;
//using System.Data.SqlClient;
using Mono.Data.Sqlite;
//using System.Data.SQLite;

using Mono.Unix; //Catalog

class SqliteServer : Sqlite
{
	public SqliteServer() {
	}
	
	~SqliteServer() {}

	public void CreatePingTable()
	 {
		dbcmd.CommandText = 
			"CREATE TABLE " + Constants.ServerPingTable + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"evaluatorID INT, " + //foreign key
			"cjVersion TEXT, " +
			"osVersion TEXT, " +
			"IP TEXT, " +
			"date TEXT ) ";
		dbcmd.ExecuteNonQuery();
	 }

	public void CreateEvaluatorTable()
	 {
		dbcmd.CommandText = 
			"CREATE TABLE " + Constants.ServerEvaluatorTable + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"name TEXT, " +
			"email TEXT, " +
			"dateborn TEXT, " +
			"countryID INT, " + //foreign key
			"confiable INT ) "; //bool
		dbcmd.ExecuteNonQuery();
	 }

	//public static int InsertPing(ServerPing ping)
	public static int InsertPing(bool dbconOpened, int evaluatorID, string cjVersion, string osVersion, string ip, string date)
	{
		if(! dbconOpened)
			dbcon.Open();

		string uniqueID = "NULL";

		string myString = "INSERT INTO " + Constants.ServerPingTable + 
			" (uniqueID, evaluatorID, cjVersion, osVersion, IP, date) VALUES (" + 
			uniqueID + ", " + evaluatorID + ", '" + 
			cjVersion + "', '" + osVersion + "', '" +
			ip + "', '" + date + "')" ;
		
		dbcmd.CommandText = myString;
		
		Log.WriteLine(dbcmd.CommandText.ToString());
		
		dbcmd.ExecuteNonQuery();
		int myReturn = dbcon.LastInsertRowId;

		if(! dbconOpened)
			dbcon.Close();

		return myReturn;
	}

	public static int InsertEvaluator(bool dbconOpened, string name, string email, string dateBorn, int countryID, bool confiable)
	{
		if(! dbconOpened)
			dbcon.Open();

		string uniqueID = "NULL";

		string myString = "INSERT INTO " + Constants.ServerEvaluatorTable + 
			" (uniqueID, name, email, dateBorn, countryID, confiable) VALUES (" + 
			uniqueID + ", '" + name + "', '" + 
			email + "', '" + dateBorn + "', " +
			countryID + ", " + Util.BoolToInt(confiable) + ")" ;
		
		dbcmd.CommandText = myString;
		
		Log.WriteLine(dbcmd.CommandText.ToString());
		
		dbcmd.ExecuteNonQuery();
		int myReturn = dbcon.LastInsertRowId;

		if(! dbconOpened)
			dbcon.Close();

		return myReturn;
	}

	public static ArrayList Stats() 
	{
		dbcon.Open();
		dbcmd.CommandText = "SELECT " +
			" MAX(SPing.uniqueID), MAX(SEvaluator.uniqueID), MAX(session.uniqueID), MAX(person.uniqueID) " +
			//", " + 
			//" MAX(jump.uniqueID), MAX(jumpRj.uniqueID), MAX(run.uniqueID), MAX(runInterval.uniqueID), "+
			//" MAX(reactionTime.uniqueID), MAX(pulse.uniqueID)" +
			" FROM SPing, SEvaluator, session, person";
			//, jump, jumpRj, run, runInterval, reactionTime, pulse";
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		//TODO: problema quan no hi ha registres d'alguna tabla, com per exemple: reactionTime, llavors dona sempre: |||||||||
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		ArrayList myArray = new ArrayList(1);
		
		while(reader.Read()) {
			myArray.Add(Catalog.GetString("Pings")  	+ ": " + reader[0].ToString()); //ping
			myArray.Add(Catalog.GetString("Evaluators") 	+ ": " + reader[1].ToString()); //eval
			myArray.Add(Catalog.GetString("Sessions")   	+ ": " + reader[2].ToString()); //sess
			myArray.Add(Catalog.GetString("Persons")  	+ ": " + reader[3].ToString()); //pers
			/*
			myArray.Add(reader[4].ToString()); //jump
			myArray.Add(reader[5].ToString()); //jumpRj
			myArray.Add(reader[6].ToString()); //run
			myArray.Add(reader[7].ToString()); //runI
			myArray.Add(reader[8].ToString()); //rt
			myArray.Add(reader[9].ToString()); //pulse
			*/
		}
		
		dbcon.Close();
		return myArray;
	}

}
