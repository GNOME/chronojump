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
 * Copyright (C) 2004-2020   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using System.IO;
using System.Collections; //ArrayList
using System.Collections.Generic; //List<T>
using Mono.Data.Sqlite;


class SqliteRunType : Sqlite
{
	public SqliteRunType() {
	}
	
	~SqliteRunType() {}

	/*
	 * create and initialize tables
	 */
	
	//creates table containing the types of simple Runs
	//following INT values are booleans
	//protected internal static void createTableRunType()
	//protected internal static void createTable(string tableName)
	protected override void createTable(string tableName)
	{
		dbcmd.CommandText = 
			//"CREATE TABLE " + Constants.RunTypeTable + " ( " +
			"CREATE TABLE " + tableName + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"name TEXT, " +
			"distance FLOAT, " + //>0 variable distance, ==0 fixed distance
			"description TEXT )";		
		dbcmd.ExecuteNonQuery();
	}
	
	//if this changes, runType.cs constructor should change 
	//protected internal static void initializeTableRunType()
	protected internal static void initializeTable()
	{
		string [] iniRunTypes = {
			//name:distance:description
			"Custom:0:variable distance running", 
			"20m:20:run 20 meters",
			"100m:100:run 100 meters",
			"200m:200:run 200 meters",
			"400m:400:run 400 meters",
			"1000m:1000:run 1000 meters",
			"2000m:2000:run 2000 meters",
			"Margaria:0:Margaria-Kalamen test",
			"Gesell-DBT:2.5:Gesell Dynamic Balance Test",

			//also simple agility tests
			"Agility-20Yard:18.28:20Yard Agility test",
			"Agility-505:10:505 Agility test",
			"Agility-Illinois:60:Illinois Agility test",
			"Agility-Shuttle-Run:40:Shuttle Run Agility test",
			"Agility-ZigZag:17.6:ZigZag Agility test",
			"Agility-T-Test:36:T Test"
		};
		conversionSubRateTotal = iniRunTypes.Length;
		conversionSubRate = 0;

		using(SqliteTransaction tr = dbcon.BeginTransaction())
		{
			using (SqliteCommand dbcmdTr = dbcon.CreateCommand())
			{
				dbcmdTr.Transaction = tr;

				foreach(string myString in iniRunTypes) {
					//RunTypeInsert(myString, true);
					conversionSubRate ++;
					string [] s = myString.Split(new char[] {':'});
					RunType type = new RunType();
					type.Name = s[0];
					type.Distance = Convert.ToDouble(Util.ChangeDecimalSeparator(s[1]));
					type.Description = s[2];
					Insert(type, Constants.RunTypeTable, true, dbcmdTr);
				}
			}
			tr.Commit();
		}
	
		AddGraphLinksRunSimple();	
		AddGraphLinksRunSimpleAgility();	
	}
	
	/*
	 * RunType class methods
	 */

	//called from some Chronojump methods
	//adds dbcmd to be used on next Insert method
	public static int Insert(RunType t, string tableName, bool dbconOpened)
	{
		return Insert(t, tableName, dbconOpened, dbcmd);
	}
	//Called from initialize
	public static int Insert(RunType t, string tableName, bool dbconOpened, SqliteCommand mycmd)
	{
		//string [] myStr = myRun.Split(new char[] {':'});
		if(! dbconOpened) {
			Sqlite.Open();
		}
		mycmd.CommandText = "INSERT INTO " + tableName + 
				" (uniqueID, name, distance, description)" +
				" VALUES (NULL, \"" +
				/*
				myStr[0] + "\", " + myStr[1] + ", \"" +	//name, distance
				myStr[2] + "\")" ;	//description
				*/
				t.Name + "\", " + Util.ConvertToPoint(t.Distance) + ", \"" + t.Description +	"\")" ;	
		LogB.SQL(mycmd.CommandText.ToString());
		mycmd.ExecuteNonQuery();

		//int myLast = dbcon.LastInsertRowId;
		//http://stackoverflow.com/questions/4341178/getting-the-last-insert-id-with-sqlite-net-in-c
		string myString = @"select last_insert_rowid()";
		mycmd.CommandText = myString;
		int myLast = Convert.ToInt32(mycmd.ExecuteScalar()); // Need to type-cast since `ExecuteScalar` returns an object.

		if(! dbconOpened) {
			Sqlite.Close();
		}
		return myLast;
	}
	
	public static RunType SelectAndReturnRunType(string typeName, bool dbconOpened) 
	{
		if(!dbconOpened)
			Sqlite.Open();
		dbcmd.CommandText = "SELECT * " +
			" FROM " + Constants.RunTypeTable +
			" WHERE name  = \"" + typeName +
			"\" ORDER BY uniqueID";
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		RunType myRunType = new RunType();
		
		while(reader.Read()) {
			myRunType.Name = reader[1].ToString();
			myRunType.Distance = Convert.ToDouble( reader[2].ToString() );
			myRunType.Description = reader[3].ToString();
		}
		
		myRunType.IsPredefined = myRunType.FindIfIsPredefined();

		reader.Close();
		if(!dbconOpened)
			Sqlite.Close();

		return myRunType;
	}

	//use SelectRunTypes object. Since 1.6.3
	public static List<object> SelectRunTypesNew(string allRunsName, bool onlyName) 
	{
		//allRunsName: add and "allRunsName" value
		//onlyName: return only type name

		string whereString = "";

		Sqlite.Open();
		dbcmd.CommandText = "SELECT * " +
			" FROM " + Constants.RunTypeTable +
			whereString +
			" ORDER BY uniqueID";

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		List<object> types = new List<object>();

		SelectRunTypes type;
		if(allRunsName != "") {
			type = new SelectRunTypes(allRunsName);
			types.Add(type);
		}

		while(reader.Read()) {
			if(onlyName) {
				type = new SelectRunTypes(reader[1].ToString());
			} else {
				type = new SelectRunTypes(
						Convert.ToInt32(reader[0]), 	//uniqueID
						reader[1].ToString(),		//nameEnglish
						Convert.ToDouble(Util.ChangeDecimalSeparator(reader[2].ToString())), 	//distance
						reader[3].ToString() 		//description
						);
			}
			types.Add(type);
		}

		reader.Close();
		Sqlite.Close();

		return types;
	}
	//on newly cereated code use above method
	public static string[] SelectRunTypes(string allRunsName, bool onlyName) 
	{
		//allRunsName: add and "allRunsName" value
		//onlyName: return only type name
	
		string whereString = "";
		
		Sqlite.Open();
		dbcmd.CommandText = "SELECT * " +
			" FROM " + Constants.RunTypeTable +
			whereString +
			" ORDER BY uniqueID";
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);

		int count = new int();
		count = 0;
		while(reader.Read()) {
			if(onlyName) {
				myArray.Add (reader[1].ToString());
			} else {
				myArray.Add (reader[0].ToString() + ":" +	//uniqueID
						reader[1].ToString() + ":" +	//name
						reader[2].ToString() + ":" + 	//distance
						reader[3].ToString() 		//description
					    );
			}
			count ++;
		}

		reader.Close();
		Sqlite.Close();

		int numRows;
		if(allRunsName != "") {
			numRows = count +1;
		} else {
			numRows = count;
		}
		string [] myTypes = new string[numRows];
		count =0;
		if(allRunsName != "") {
			myTypes [count++] = allRunsName;
			//LogB.SQL("{0} - {1}", myTypes[count-1], count-1);
		}
		foreach (string line in myArray) {
			myTypes [count++] = line;
			//LogB.SQL("{0} - {1}", myTypes[count-1], count-1);
		}

		return myTypes;
	}

	public static double Distance (string typeName) 
	{
		Sqlite.Open();
		dbcmd.CommandText = "SELECT distance " +
			" FROM " + Constants.RunTypeTable +
			" WHERE name == \"" + typeName + "\"";
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		double distance = 0;
		while(reader.Read()) {
			distance = Convert.ToDouble(reader[0].ToString());
		}
		reader.Close();
		Sqlite.Close();
		return distance;
	}
	
	public static void AddGraphLinksRunSimple() {
		using(SqliteTransaction tr = dbcon.BeginTransaction())
		{
			using (SqliteCommand dbcmdTr = dbcon.CreateCommand())
			{
				dbcmdTr.Transaction = tr;

				SqliteEvent.GraphLinkInsert (Constants.RunTable, "20m", "run_simple.png", true, dbcmdTr);
				SqliteEvent.GraphLinkInsert (Constants.RunTable, "100m", "run_simple.png", true, dbcmdTr);
				SqliteEvent.GraphLinkInsert (Constants.RunTable, "200m", "run_simple.png", true, dbcmdTr);
				SqliteEvent.GraphLinkInsert (Constants.RunTable, "400m", "run_simple.png", true, dbcmdTr);
				SqliteEvent.GraphLinkInsert (Constants.RunTable, "1000m", "run_simple.png", true, dbcmdTr);
				SqliteEvent.GraphLinkInsert (Constants.RunTable, "2000m", "run_simple.png", true, dbcmdTr);
			}
			tr.Commit();
		}
	}

	public static void AddGraphLinksRunSimpleAgility() {
		using(SqliteTransaction tr = dbcon.BeginTransaction())
		{
			using (SqliteCommand dbcmdTr = dbcon.CreateCommand())
			{
				dbcmdTr.Transaction = tr;

				SqliteEvent.GraphLinkInsert (Constants.RunTable, "Agility-20Yard", "agility_20yard.png", true, dbcmdTr);
				SqliteEvent.GraphLinkInsert (Constants.RunTable, "Agility-505", "agility_505.png", true, dbcmdTr);
				SqliteEvent.GraphLinkInsert (Constants.RunTable, "Agility-Illinois", "agility_illinois.png", true, dbcmdTr);
				SqliteEvent.GraphLinkInsert (Constants.RunTable, "Agility-Shuttle-Run", "agility_shuttle.png", true, dbcmdTr);
				SqliteEvent.GraphLinkInsert (Constants.RunTable, "Agility-ZigZag", "agility_zigzag.png", true, dbcmdTr);
				SqliteEvent.GraphLinkInsert (Constants.RunTable, "Margaria", "margaria.png", true, dbcmdTr);
				SqliteEvent.GraphLinkInsert (Constants.RunTable, "Gesell-DBT", "gesell_dbt.png", true, dbcmdTr);
				SqliteEvent.GraphLinkInsert (Constants.RunTable, "Agility-T-Test", "agility_t_test.png", true, dbcmdTr);
			}
			tr.Commit();
		}
	}


	public static void Delete(string name)
	{
		Sqlite.Open();
		dbcmd.CommandText = "Delete FROM " + Constants.RunTypeTable +
			" WHERE name == \"" + name + "\"";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		Sqlite.Close();
	}


}	

class SqliteRunIntervalType : SqliteRunType
{
	public SqliteRunIntervalType() {
	}
	
	~SqliteRunIntervalType() {}
	
	//creates table containing the types of Interval Runs 
	//following INT values are booleans
	//protected internal static void createTableRunIntervalType()
	//protected internal static void createTable(string tableName)
	protected override void createTable(string tableName)
	{
		dbcmd.CommandText = 
			//"CREATE TABLE " + Constants.RunIntervalTypeTable + " ( " +
			"CREATE TABLE " + tableName + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"name TEXT, " +
			"distance FLOAT, " + //>0 variable distance, ==0 fixed distance
					//this distance will be the same in all tracks.
					//-1 each track can have a different distance (started at 0.8.1.5, see distancesString)
			"tracksLimited INT, " +  //1 limited by tracks (intervals); 0 limited by time
			"fixedValue INT, " +   //0: no fixed value; 3: 3 tracks or seconds 
			"unlimited INT, " +		
			"description TEXT, " +	
			"distancesString TEXT )"; 	//new at 0.8.1.5:
		       					//when distance is 0 or >0, distancesString it's ""
							//when distance is -1, distancesString is distance of each track, 
							//	eg: "7-5-9" for a runInterval with three tracks of 7, 5 and 9 meters each
							//	this is nice for agility tests
		dbcmd.ExecuteNonQuery();
	}
	
	//if this changes, runType.cs constructor should change 
	//protected internal static void initializeTableRunIntervalType()
	protected internal static new void initializeTable()
	{
		string [] iniRunTypes = {
			//name:distance:tracksLimited:fixedValue:unlimited:description:distancesString
			"byLaps:0:1:0:0:Run n laps x distance:",
			"byTime:0:0:0:0:Make max laps in n seconds:",
			"unlimited:0:0:0:1:Continue running in n distance:",	//suppose limited by time
			"20m10times:20:1:10:0:Run 10 times a 20m distance:",	//only in more runs
			"7m30seconds:7:0:30:0:Make max laps in 30 seconds:",	//only in more runs
			"20m endurance:20:0:0:1:Continue running in 20m distance:",	//only in more runs
			"MTGUG:-1:1:3:0:Modified time Getup and Go test:1-7-19",
			"Agility-3L3R:-1:1:2:0:Turn left three times and turn right three times:24.14-24.14"
		};
		
		using(SqliteTransaction tr = dbcon.BeginTransaction())
		{
			using (SqliteCommand dbcmdTr = dbcon.CreateCommand())
			{
				dbcmdTr.Transaction = tr;

				foreach(string myString in iniRunTypes) {
					//RunIntervalTypeInsert(myString, true);
					string [] s = myString.Split(new char[] {':'});
					RunType type = new RunType();
					type.Name = s[0];
					type.Distance = Convert.ToDouble(Util.ChangeDecimalSeparator(s[1]));
					type.TracksLimited = Util.IntToBool(Convert.ToInt32(s[2]));
					type.FixedValue = Convert.ToInt32(s[3]);
					type.Unlimited = Util.IntToBool(Convert.ToInt32(s[4]));
					type.Description = s[5];
					type.DistancesString = s[6];
					Insert(type, Constants.RunIntervalTypeTable, true, dbcmdTr);
				}
			}
			tr.Commit();
		}
		
		AddGraphLinksRunInterval();

		addRSA();
	}

	protected internal static void addRSA()
	{
		string [] iniRunTypes = {
			//name:distance:tracksLimited:fixedValue:unlimited:description:distancesString
			"RSA Aziz 2000 40, R30 x 8:-1:1:16:0:RSA Aziz et al. 2000:40-R30",
			"RSA Balsom 15, R30 x 40:-1:1:80:0:RSA Balsom et al. 1992:15-R30",
			"RSA Balsom 30, R30 x 20:-1:1:40:0:RSA Balsom et al. 1992:30-R30",
			"RSA Balsom 40, R30 x 15:-1:1:30:0:RSA Balsom et al. 1992:40-R30",
			"RSA Dawson 40, R24 x 6:-1:1:12:0:RSA Dawson et al. 1998:40-R24",
			"RSA Fitzsimons 40, R24 x 6:-1:1:12:0:RSA Fitzsimons et al. 1993:40-R24",
			"RSA Gaitanos 6, R30 x 10:-1:1:20:0:RSA Gaitanos et al. 1991:6-R30",
			"RSA Hamilton 6, R30 x 10:-1:1:20:0:RSA Hamilton et al. 1991:6-R30",
			"RSA RAST 35, R10 x 6:-1:1:12:0:RSA RAST Test:35-R10",			//this is added on DB 1.10
			"RSA Mujica 15, R24 x 6:-1:1:12:0:RSA Mujica et al. 2000:15-R24",
			"RSA Wadley 20, R17 x 12:-1:1:24:0:RSA Wadley and Le Rossignol 1998:20-R17",
			"RSA Wragg 34.2, R25 x 7:-1:1:14:0:RSA Wragg et al. 2000:34.2-R25"
		};
		using(SqliteTransaction tr = dbcon.BeginTransaction())
		{
			using (SqliteCommand dbcmdTr = dbcon.CreateCommand())
			{
				dbcmdTr.Transaction = tr;

				foreach(string myString in iniRunTypes) {
					//RunIntervalTypeInsert(myString, true);
					string [] s = myString.Split(new char[] {':'});
					RunType type = new RunType();
					type.Name = s[0];
					type.Distance = Convert.ToDouble(Util.ChangeDecimalSeparator(s[1]));
					type.TracksLimited = Util.IntToBool(Convert.ToInt32(s[2]));
					type.FixedValue = Convert.ToInt32(s[3]);
					type.Unlimited = Util.IntToBool(Convert.ToInt32(s[4]));
					type.Description = s[5];
					type.DistancesString = s[6];
					Insert(type, Constants.RunIntervalTypeTable, true, dbcmdTr);
				}
			}
			tr.Commit();
		}
	}


	//called from some Chronojump methods
	//adds dbcmd to be used on next Insert method
	public static new int Insert(RunType t, string tableName, bool dbconOpened)
	{
		return Insert(t, tableName, dbconOpened, dbcmd);
	}
	//Called from initialize
	public static new int Insert(RunType t, string tableName, bool dbconOpened, SqliteCommand mycmd)
	{
		//done here for not having twho Sqlite.Opened
		//double distance = t.Distance;

		if(! dbconOpened) {
			Sqlite.Open();
		}
		mycmd.CommandText = "INSERT INTO " + tableName + 
				" (uniqueID, name, distance, tracksLimited, fixedValue, unlimited, description, distancesString)" +
				" VALUES (NULL, \"" +
				t.Name + 	"\", " + Util.ConvertToPoint(t.Distance) + ", " + Util.BoolToInt(t.TracksLimited) + 	", " + t.FixedValue + ", " +
				Util.BoolToInt(t.Unlimited) + 	", \"" + t.Description +	"\", \"" + t.DistancesString + 	"\")" ;	
		LogB.SQL(mycmd.CommandText.ToString());
		mycmd.ExecuteNonQuery();
		
		//int myLast = dbcon.LastInsertRowId;
		//http://stackoverflow.com/questions/4341178/getting-the-last-insert-id-with-sqlite-net-in-c
		string myString = @"select last_insert_rowid()";
		mycmd.CommandText = myString;
		int myLast = Convert.ToInt32(mycmd.ExecuteScalar()); // Need to type-cast since `ExecuteScalar` returns an object.

		if(! dbconOpened) {
			Sqlite.Close();
		}
		return myLast;
	}

	//use SelectRunITypes object. Since 1.6.3
	public static List<object> SelectRunIntervalTypesNew(string allRunsName, bool onlyName)
	{
		Sqlite.Open();
		dbcmd.CommandText = "SELECT * " +
			" FROM " + Constants.RunIntervalTypeTable +
			" ORDER BY uniqueID";

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		List<object> types = new List<object>();

		SelectRunITypes type;
		if(allRunsName != "") {
			type = new SelectRunITypes(allRunsName);
			types.Add(type);
		}

		while(reader.Read()) {
			if(onlyName) {
				type = new SelectRunITypes(reader[1].ToString());
			} else {
				type = new SelectRunITypes(
						Convert.ToInt32(reader[0]), 	//uniqueID
						reader[1].ToString(),		//nameEnglish
						Convert.ToDouble(Util.ChangeDecimalSeparator(reader[2].ToString())), 	//distance
						Util.IntToBool(Convert.ToInt32(reader[3].ToString())), 	//tracksLimited
						Convert.ToInt32(reader[4].ToString()), 			//fixedValue
						Util.IntToBool(Convert.ToInt32(reader[5].ToString())), 	//unlimited
						reader[6].ToString(),					//description
						Util.ChangeDecimalSeparator(reader[7].ToString())	//distancesString
					    );
			}
			types.Add(type);
		}

		reader.Close();
		Sqlite.Close();

		return types;
	}
	//on newly created code use above method
	public static string[] SelectRunIntervalTypes(string allRunsName, bool onlyName) 
	{
		Sqlite.Open();
		dbcmd.CommandText = "SELECT * " +
			" FROM " + Constants.RunIntervalTypeTable +
			" ORDER BY uniqueID";
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList myArray = new ArrayList(2);

		int count = new int();
		count = 0;
		while(reader.Read()) {
			if(onlyName) {
				myArray.Add (reader[1].ToString());
			} else {
				myArray.Add (reader[0].ToString() + ":" +	//uniqueID
						reader[1].ToString() + ":" +	//name
						reader[2].ToString() + ":" + 	//distance
						reader[3].ToString() + ":" + 	//tracksLimited
						reader[4].ToString() + ":" + 	//fixedValue
						reader[5].ToString() + ":" + 	//unlimited
						reader[6].ToString() + ":" +	//description
						Util.ChangeDecimalSeparator(reader[7].ToString())	//distancesString
					    );
			}
			count ++;
		}

		reader.Close();
		Sqlite.Close();

		int numRows;
		if(allRunsName != "") {
			numRows = count +1;
		} else {
			numRows = count;
		}
		string [] myTypes = new string[numRows];
		count =0;
		if(allRunsName != "") {
			myTypes [count++] = allRunsName;
		}
		foreach (string line in myArray) {
			myTypes [count++] = line;
		}

		return myTypes;
	}

	public static RunType SelectAndReturnRunIntervalType(string typeName, bool dbconOpened) 
	{
		if(!dbconOpened)
			Sqlite.Open();
		dbcmd.CommandText = "SELECT * " +
			" FROM " + Constants.RunIntervalTypeTable +
			" WHERE name  = \'" + typeName +
			"\' ORDER BY uniqueID";
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		RunType myRunType = new RunType();
		
		while(reader.Read()) {
			myRunType.Name = reader[1].ToString();
			myRunType.Distance = Convert.ToDouble( reader[2].ToString() );
			myRunType.HasIntervals = true;
			myRunType.TracksLimited = Util.IntToBool(Convert.ToInt32(reader[3].ToString()));
			myRunType.FixedValue = Convert.ToInt32( reader[4].ToString() );
			myRunType.Unlimited = Util.IntToBool(Convert.ToInt32(reader[5].ToString()));
			myRunType.Description = reader[6].ToString();
			
			myRunType.DistancesString = Util.ChangeDecimalSeparator(reader[7].ToString());
			//if it has no value sqlite reads it as 0, but should be ""
			if(myRunType.DistancesString == "0")
				myRunType.DistancesString = "";
		}

		myRunType.IsPredefined = myRunType.FindIfIsPredefined();

		reader.Close();
		if(!dbconOpened)
			Sqlite.Close();

		return myRunType;
	}

	public static void AddGraphLinksRunInterval() {
		using(SqliteTransaction tr = dbcon.BeginTransaction())
		{
			using (SqliteCommand dbcmdTr = dbcon.CreateCommand())
			{
				dbcmdTr.Transaction = tr;

				SqliteEvent.GraphLinkInsert (Constants.RunIntervalTable, "byLaps", "run_interval.png", true, dbcmdTr);
				SqliteEvent.GraphLinkInsert (Constants.RunIntervalTable, "byTime", "run_interval.png", true, dbcmdTr);
				SqliteEvent.GraphLinkInsert (Constants.RunIntervalTable, "unlimited", "run_interval.png", true, dbcmdTr);
				SqliteEvent.GraphLinkInsert (Constants.RunIntervalTable, "20m10times", "run_interval.png", true, dbcmdTr);
				SqliteEvent.GraphLinkInsert (Constants.RunIntervalTable, "7m30seconds", "run_interval.png", true, dbcmdTr);
				SqliteEvent.GraphLinkInsert (Constants.RunIntervalTable, "20m endurance", "run_interval.png", true, dbcmdTr);
				SqliteEvent.GraphLinkInsert (Constants.RunIntervalTable, "MTGUG", "mtgug.png", true, dbcmdTr);
				SqliteEvent.GraphLinkInsert (Constants.RunIntervalTable, "Agility-3L3R", "agility_3l3r.png", true, dbcmdTr);
			}
			tr.Commit();
		}
	}
	
	public static new void Delete(string name)
	{
		Sqlite.Open();
		dbcmd.CommandText = "Delete FROM " + Constants.RunIntervalTypeTable +
			" WHERE name == \"" + name + "\"";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		Sqlite.Close();
	}


}
