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
			"code TEXT, " +
			"name TEXT, " +
			"email TEXT, " +
			"dateborn TEXT, " +
			"countryID INT, " + //foreign key
			"chronometer TEXT, " +
			"device TEXT, " +
			"comments TEXT, " +
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

	public static int InsertEvaluator(bool dbconOpened, string code, string name, string email, string dateBorn, 
			int countryID, string chronometer, string device, string comments, bool confiable)
	{
		if(! dbconOpened)
			dbcon.Open();

		string uniqueID = "NULL";

		string myString = "INSERT INTO " + Constants.ServerEvaluatorTable + 
			" (uniqueID, code, name, email, dateBorn, countryID, chronometer, device, comments, confiable) VALUES (" + 
			uniqueID + ", '" + 
			code + "', '" + name + "', '" + 
			email + "', '" + dateBorn + "', " +
			countryID + ", '" + chronometer + "', '" + 
			device + "', '" + comments + "', " +
			//Util.BoolToInt(confiable) + 
			Util.BoolToInt(false) + //security: cannot directly insert a confiable person
			")" ;
		
		dbcmd.CommandText = myString;
		
		Log.WriteLine(dbcmd.CommandText.ToString());
		
		dbcmd.ExecuteNonQuery();
		int myReturn = dbcon.LastInsertRowId;

		if(! dbconOpened)
			dbcon.Close();

		return myReturn;
	}
	
	public static void UpdateEvaluator(bool dbconOpened, int uniqueID, string code, string name, string email, string dateBorn, 
			int countryID, string chronometer, string device, string comments, bool confiable)
	{
		if(! dbconOpened)
			dbcon.Open();
		dbcmd.CommandText = "UPDATE " + Constants.ServerEvaluatorTable + " " +
			" SET code = '" + code +
			"' , name = '" + name +
			"' , email = '" + email +
			"' , dateBorn = '" + dateBorn +
			"' , countryID = " + countryID +
			", chronometer = '" + chronometer +
			"', device = '" + device +
			"', comments = '" + comments +
			//"', confiable = " + Util.BoolToInt(confiable) + //security: update cannot change confiable
			"' WHERE uniqueID == " + uniqueID;
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		if(! dbconOpened)
			dbcon.Close();
	}
	
	
	//when client selects in it's DB, there's only a row with uniqueID: 1
	//if confiable is read on client, it will be also checked on server
	public static ServerEvaluator SelectEvaluator(int myUniqueID)
	{
		dbcon.Open();
		dbcmd.CommandText = "SELECT * FROM " + Constants.ServerEvaluatorTable + " WHERE uniqueID == " + myUniqueID ; 
		Log.WriteLine(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
	
		ServerEvaluator myEval = new ServerEvaluator();

		//will return a -1 on uniqueID to know that evaluator data is not in the database		
		myEval.UniqueID = -1; 

		while(reader.Read()) {
			myEval.UniqueID = Convert.ToInt32(reader[0].ToString()); 
			myEval.Code = reader[1].ToString(); 
			myEval.Name = reader[2].ToString(); 
			myEval.Email = reader[3].ToString(); 
			myEval.DateBorn = reader[4].ToString();
			myEval.CountryID = Convert.ToInt32(reader[5].ToString());
			myEval.Chronometer = reader[6].ToString();
			myEval.Device = reader[7].ToString();
			myEval.Comments = reader[8].ToString();
			myEval.Confiable = Util.IntToBool(Convert.ToInt32(reader[9].ToString())); 
		}

		dbcon.Close();
		return myEval;
	}
	

	public static string [] Stats() {
		ArrayList stats = new ArrayList();
			
		dbcon.Open();

		/*
		 * is good to add the string stuff like "Pings" 
		 * because then client will show this data or not 
		 * depending if it matches what want to show.
		 * Maintain the ':' as separator
		*/
		stats.Add("Pings:" + Sqlite.Count(Constants.ServerPingTable, true).ToString());
		stats.Add("Evaluators:" + Sqlite.Count(Constants.ServerEvaluatorTable, true).ToString());
		stats.Add("Sessions:" + Sqlite.Count(Constants.SessionTable, true).ToString());
		stats.Add("Persons:" + Sqlite.Count(Constants.PersonTable, true).ToString());
		stats.Add("Jumps:" + Sqlite.Count(Constants.JumpTable, true).ToString());
		stats.Add("JumpsRj:" + Sqlite.Count(Constants.JumpRjTable, true).ToString());
		stats.Add("Runs:" + Sqlite.Count(Constants.RunTable, true).ToString());
		stats.Add("RunsInterval:" + Sqlite.Count(Constants.RunIntervalTable, true).ToString());
		stats.Add("ReactionTimes:" + Sqlite.Count(Constants.ReactionTimeTable, true).ToString());
		stats.Add("Pulses:" + Sqlite.Count(Constants.PulseTable, true).ToString());
		
		dbcon.Close();

		string [] statsString = Util.ArrayListToString(stats);
		return statsString;
	}
	
	/*
	 * this is only called on client
	 */
	public static string [] StatsMine() {
		ArrayList stats = new ArrayList();
			
		dbcon.Open();

		/*
		 * is good to add the string stuff like "Pings" 
		 * because then client will show this data or not 
		 * depending if it matches what want to show.
		 * Maintain the ':' as separator
		*/
		stats.Add("Sessions:" + Sqlite.CountCondition(Constants.SessionTable, true, "serverUniqueID", ">", "0").ToString());
		stats.Add("Persons:" + Sqlite.CountCondition(Constants.PersonTable, true, "serverUniqueID", ">", "0").ToString());
		stats.Add("Jumps:" + Sqlite.CountCondition(Constants.JumpTable, true, "simulated", ">", "0").ToString());
		stats.Add("JumpsRj:" + Sqlite.CountCondition(Constants.JumpRjTable, true, "simulated", ">", "0").ToString());
		stats.Add("Runs:" + Sqlite.CountCondition(Constants.RunTable, true, "simulated", ">", "0").ToString());
		stats.Add("RunsInterval:" + Sqlite.CountCondition(Constants.RunIntervalTable, true, "simulated", ">", "0").ToString());
		stats.Add("ReactionTimes:" + Sqlite.CountCondition(Constants.ReactionTimeTable, true, "simulated", ">", "0").ToString());
		stats.Add("Pulses:" + Sqlite.CountCondition(Constants.PulseTable, true, "simulated", ">", "0").ToString());
		
		dbcon.Close();

		string [] statsString = Util.ArrayListToString(stats);
		return statsString;
	}
	

}
