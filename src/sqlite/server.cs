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
 * Copyright (C) 2004-2016   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using System.IO;
using System.Collections; //ArrayList
using Mono.Data.Sqlite;

using Mono.Unix; //Catalog

public class SqliteServer : Sqlite
{
	public SqliteServer (SqliteConnection dbcon, SqliteCommand dbcmd)
		:base(dbcon, dbcmd)
	{}
	
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
			"date TEXT ) "; //YYYY-MM-DD since db 0.72
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
			"dateborn TEXT, " + //YYYY-MM-DD since db 0.72
			"countryID INT, " + //foreign key
			"chronometer TEXT, " +
			"device TEXT, " +
			"comments TEXT, " +
			"confiable INT ) "; //bool
		dbcmd.ExecuteNonQuery();
	 }

	//public int InsertPing(ServerPing ping)
	public int InsertPing(bool dbconOpened, int evaluatorID, string cjVersion, string osVersion, string ip, DateTime date)
	{
		if(! dbconOpened)
			SqliteGeneral.Sqlite.Open();

		string uniqueID = "NULL";

		string myString = "INSERT INTO " + Constants.ServerPingTable + 
			" (uniqueID, evaluatorID, cjVersion, osVersion, IP, date) VALUES (" + 
			uniqueID + ", " + evaluatorID + ", \"" + 
			cjVersion + "\", \"" + osVersion + "\", \"" +
			ip + "\", \"" + UtilDate.ToSql(date) + "\")" ;
		
		dbcmd.CommandText = myString;
		
		LogB.SQL(dbcmd.CommandText.ToString());
		
		dbcmd.ExecuteNonQuery();

		//int myLast = dbcon.LastInsertRowId;
		//http://stackoverflow.com/questions/4341178/getting-the-last-insert-id-with-sqlite-net-in-c
		myString = @"select last_insert_rowid()";
		dbcmd.CommandText = myString;
		int myLast = Convert.ToInt32(dbcmd.ExecuteScalar()); // Need to type-cast since `ExecuteScalar` returns an object.

		if(! dbconOpened)
			SqliteGeneral.Sqlite.Close();

		return myLast;
	}

	public int InsertEvaluator(bool dbconOpened, string code, string name, string email, DateTime dateBorn, 
			int countryID, string chronometer, string device, string comments, bool confiable)
	{
		if(! dbconOpened)
			SqliteGeneral.Sqlite.Open();

		string uniqueID = "NULL";

		string myString = "INSERT INTO " + Constants.ServerEvaluatorTable + 
			" (uniqueID, code, name, email, dateBorn, countryID, chronometer, device, comments, confiable) VALUES (" + 
			uniqueID + ", \"" + 
			code + "\", \"" + name + "\", \"" + 
			email + "\", \"" + UtilDate.ToSql(dateBorn) + "\", " +
			countryID + ", \"" + chronometer + "\", \"" + 
			device + "\", \"" + comments + "\", " +
			//Util.BoolToInt(confiable) + 
			Util.BoolToInt(false) + //security: cannot directly insert a confiable person
			")" ;
		
		dbcmd.CommandText = myString;
		
		LogB.SQL(dbcmd.CommandText.ToString());
		
		dbcmd.ExecuteNonQuery();


		//int myLast = dbcon.LastInsertRowId;
		//http://stackoverflow.com/questions/4341178/getting-the-last-insert-id-with-sqlite-net-in-c
		myString = @"select last_insert_rowid()";
		dbcmd.CommandText = myString;
		int myLast = Convert.ToInt32(dbcmd.ExecuteScalar()); // Need to type-cast since `ExecuteScalar` returns an object.

		if(! dbconOpened)
			SqliteGeneral.Sqlite.Close();

		return myLast;
	}
	
	public void UpdateEvaluator(bool dbconOpened, int uniqueID, string code, string name, string email, DateTime dateBorn, 
			int countryID, string chronometer, string device, string comments, bool confiable)
	{
		if(! dbconOpened)
			SqliteGeneral.Sqlite.Open();
		dbcmd.CommandText = "UPDATE " + Constants.ServerEvaluatorTable + " " +
			" SET code = \"" + code +
			"\" , name = \"" + name +
			"\" , email = \"" + email +
			"\" , dateBorn = \"" + UtilDate.ToSql(dateBorn) +
			"\" , countryID = " + countryID +
			", chronometer = \"" + chronometer +
			"\", device = \"" + device +
			"\", comments = \"" + comments +
			//"\", confiable = " + Util.BoolToInt(confiable) + //security: update cannot change confiable
			"\" WHERE uniqueID == " + uniqueID;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		if(! dbconOpened)
			SqliteGeneral.Sqlite.Close();
	}
	
	
	//when client selects in it's DB, there's only a row with uniqueID: 1
	//if confiable is read on client, it will be also checked on server
	public ServerEvaluator SelectEvaluator(int myUniqueID)
	{
		SqliteGeneral.Sqlite.Open();
		dbcmd.CommandText = "SELECT * FROM " + Constants.ServerEvaluatorTable + " WHERE uniqueID == " + myUniqueID ; 
		LogB.SQL(dbcmd.CommandText.ToString());
		
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
			myEval.DateBorn = UtilDate.FromSql(reader[4].ToString());
			myEval.CountryID = Convert.ToInt32(reader[5].ToString());
			myEval.Chronometer = reader[6].ToString();
			myEval.Device = reader[7].ToString();
			myEval.Comments = reader[8].ToString();
			myEval.Confiable = Util.IntToBool(Convert.ToInt32(reader[9].ToString())); 
		}

		reader.Close();
		SqliteGeneral.Sqlite.Close();
		return myEval;
	}
	
	public string [] SelectEvaluators(bool addAnyString)
	{
		SqliteGeneral.Sqlite.Open();
		dbcmd.CommandText = "SELECT " + 
			Constants.ServerEvaluatorTable + ".uniqueID, " + 
			Constants.ServerEvaluatorTable + ".name " +
		       	" FROM " + Constants.ServerEvaluatorTable + ", " + Constants.SessionTable + 
			" WHERE " + Constants.ServerEvaluatorTable + ".uniqueID = " + Constants.SessionTable +".evaluatorID" +
		        " GROUP BY " + Constants.ServerEvaluatorTable + ".uniqueID";
		LogB.SQL(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
	
		ArrayList evals = new ArrayList();
		if(addAnyString)
			evals.Add(Constants.AnyID.ToString() + ":" + Constants.Any);
		while(reader.Read())
			evals.Add(reader[0].ToString() + ":" + reader[1].ToString());

		reader.Close();
		SqliteGeneral.Sqlite.Close();
		return Util.ArrayListToString(evals);
	}
	
	public string Query(string str) {
		SqliteGeneral.Sqlite.Open();

		dbcmd.CommandText = str; 
		LogB.SQL(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
	
		string myReturn = "0:";
		while(reader.Read()) {
			myReturn = reader[0].ToString() + ":" + reader[1].ToString();
		}
		
		reader.Close();
		SqliteGeneral.Sqlite.Close();
		return myReturn;
	}

	public string [] Stats() {
		ArrayList stats = new ArrayList();
			
		SqliteGeneral.Sqlite.Open();

		/*
		 * is good to add the string stuff like "Pings" 
		 * because then client will show this data or not 
		 * depending if it matches what want to show.
		 * Maintain the ':' as separator
		*/
		stats.Add("Pings:" + SqliteGeneral.Sqlite.Count(Constants.ServerPingTable, true).ToString());
		stats.Add("Evaluators:" + SqliteGeneral.Sqlite.Count(Constants.ServerEvaluatorTable, true).ToString());
		stats.Add("Sessions:" + SqliteGeneral.Sqlite.Count(Constants.SessionTable, true).ToString());
		stats.Add("Persons:" + SqliteGeneral.Sqlite.Count(Constants.PersonTable, true).ToString());
		stats.Add("Jumps:" + SqliteGeneral.Sqlite.Count(Constants.JumpTable, true).ToString());
		stats.Add("JumpsRj:" + SqliteGeneral.Sqlite.Count(Constants.JumpRjTable, true).ToString());
		stats.Add("Runs:" + SqliteGeneral.Sqlite.Count(Constants.RunTable, true).ToString());
		stats.Add("RunsInterval:" + SqliteGeneral.Sqlite.Count(Constants.RunIntervalTable, true).ToString());
		stats.Add("ReactionTimes:" + SqliteGeneral.Sqlite.Count(Constants.ReactionTimeTable, true).ToString());
		stats.Add("Pulses:" + SqliteGeneral.Sqlite.Count(Constants.PulseTable, true).ToString());
		stats.Add("MultiChronopic:" + SqliteGeneral.Sqlite.Count(Constants.MultiChronopicTable, true).ToString());
		
		SqliteGeneral.Sqlite.Close();

		string [] statsString = Util.ArrayListToString(stats);
		return statsString;
	}
	
	/*
	 * this is only called on client
	 */
	public string [] StatsMine() {
		ArrayList stats = new ArrayList();
			
		SqliteGeneral.Sqlite.Open();

		/*
		 * is good to add the string stuff like "Pings" 
		 * because then client will show this data or not 
		 * depending if it matches what want to show.
		 * Maintain the ':' as separator
		*/
		stats.Add("Sessions:" + SqliteGeneral.Sqlite.CountCondition(Constants.SessionTable, true, "serverUniqueID", ">", "0").ToString());
		stats.Add("Persons:" + SqliteGeneral.Sqlite.CountCondition(Constants.PersonTable, true, "serverUniqueID", ">", "0").ToString());
		stats.Add("Jumps:" + SqliteGeneral.Sqlite.CountCondition(Constants.JumpTable, true, "simulated", ">", "0").ToString());
		stats.Add("JumpsRj:" + SqliteGeneral.Sqlite.CountCondition(Constants.JumpRjTable, true, "simulated", ">", "0").ToString());
		stats.Add("Runs:" + SqliteGeneral.Sqlite.CountCondition(Constants.RunTable, true, "simulated", ">", "0").ToString());
		stats.Add("RunsInterval:" + SqliteGeneral.Sqlite.CountCondition(Constants.RunIntervalTable, true, "simulated", ">", "0").ToString());
		stats.Add("ReactionTimes:" + SqliteGeneral.Sqlite.CountCondition(Constants.ReactionTimeTable, true, "simulated", ">", "0").ToString());
		stats.Add("Pulses:" + SqliteGeneral.Sqlite.CountCondition(Constants.PulseTable, true, "simulated", ">", "0").ToString());
		stats.Add("MultiChronopic:" + SqliteGeneral.Sqlite.CountCondition(Constants.MultiChronopicTable, true, "simulated", ">", "0").ToString());
		
		SqliteGeneral.Sqlite.Close();

		string [] statsString = Util.ArrayListToString(stats);
		return statsString;
	}
}
