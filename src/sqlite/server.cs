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
 */

using System;
using System.Data;
using System.IO;
using System.Collections; //ArrayList
using Mono.Data.Sqlite;

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

	public static string Stats() {
		ArrayList stats = new ArrayList();
			
		dbcon.Open();

		stats.Add("Pings\t" + Sqlite.Count(Constants.ServerPingTable, true).ToString());
		stats.Add("Evaluators\t" + Sqlite.Count(Constants.ServerEvaluatorTable, true).ToString());
		stats.Add("Persons\t" + Sqlite.Count(Constants.PersonTable, true).ToString());
		stats.Add("Sessions\t" + Sqlite.Count(Constants.SessionTable, true).ToString());
		stats.Add("Jumps\t" + Sqlite.Count(Constants.JumpTable, true).ToString());
		stats.Add("JumpsRj\t" + Sqlite.Count(Constants.JumpRjTable, true).ToString());
		stats.Add("Runs\t" + Sqlite.Count(Constants.RunTable, true).ToString());
		stats.Add("RunsInterval\t" + Sqlite.Count(Constants.RunIntervalTable, true).ToString());
		stats.Add("Reaction times\t" + Sqlite.Count(Constants.ReactionTimeTable, true).ToString());
		stats.Add("Pulses\t" + Sqlite.Count(Constants.PulseTable, true).ToString());
		
		dbcon.Close();

		string statsString = Util.ArrayListToSingleString(stats);

		return statsString;
	}
	

}
