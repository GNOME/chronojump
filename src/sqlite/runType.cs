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
using Mono.Data.SqliteClient;
using System.Data.SqlClient;


class SqliteRunType : Sqlite
{
	/*
	 * create and initialize tables
	 */
	
	//creates table containing the types of simple Runs
	//following INT values are booleans
	protected internal static void createTableRunType()
	{
		dbcmd.CommandText = 
			"CREATE TABLE runType ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"name TEXT, " +
			"distance FLOAT, " + //>0 variable distance, ==0 fixed distance
			"description TEXT )";		
		dbcmd.ExecuteNonQuery();
	}
	
	//if this changes, runType.cs constructor should change 
	protected internal static void initializeTableRunType()
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

			//also simple agility tests
			"Agility-20Yard:18.28:20Yard Agility test",
			"Agility-505:10:505 Agility test",
			"Agility-Illinois:60:Illinois Agility test",
			"Agility-Shuttle-Run:40:Shuttle Run Agility test",
			"Agility-ZigZag:17.6:ZigZag Agility test"
		};
		foreach(string myRunType in iniRunTypes) {
			RunTypeInsert(myRunType, true);
		}
	
		AddGraphLinksRunSimple();	
		AddGraphLinksRunSimpleAgility();	
		AddGraphLinksRunInterval();	
	}
	
	public static void AddGraphLinksRunSimple() {
		SqliteEvent.Insert ("run", "20m", "run_simple.png");
		SqliteEvent.Insert ("run", "100m", "run_simple.png");
		SqliteEvent.Insert ("run", "200m", "run_simple.png");
		SqliteEvent.Insert ("run", "400m", "run_simple.png");
		SqliteEvent.Insert ("run", "1000m", "run_simple.png");
		SqliteEvent.Insert ("run", "2000m", "run_simple.png");
	}

	public static void AddGraphLinksRunSimpleAgility() {
		SqliteEvent.Insert ("run", "Agility-20Yard", "agility_20yard.png");
		SqliteEvent.Insert ("run", "Agility-505", "agility_505.png");
		SqliteEvent.Insert ("run", "Agility-Illinois", "agility_illinois.png");
		SqliteEvent.Insert ("run", "Agility-Shuttle-Run", "agility_shuttle.png");
		SqliteEvent.Insert ("run", "Agility-ZigZag", "agility_zigzag.png");
	}

	public static void AddGraphLinksRunInterval() {
		SqliteEvent.Insert ("runInterval", "byLaps", "run_interval.png");
		SqliteEvent.Insert ("runInterval", "byTime", "run_interval.png");
		SqliteEvent.Insert ("runInterval", "unlimited", "run_interval.png");
		SqliteEvent.Insert ("runInterval", "20m10times", "run_interval.png");
		SqliteEvent.Insert ("runInterval", "7m30seconds", "run_interval.png");
		SqliteEvent.Insert ("runInterval", "20m endurance", "run_interval.png");
	}


	//creates table containing the types of Interval Runs 
	//following INT values are booleans
	protected internal static void createTableRunIntervalType()
	{
		dbcmd.CommandText = 
			"CREATE TABLE runIntervalType ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"name TEXT, " +
			"distance FLOAT, " + //>0 variable distance, ==0 fixed distance
					//this distance will be the same in all tracks
			"tracksLimited INT, " +  //1 limited by tracks (intervals); 0 limited by time
			"fixedValue INT, " +   //0: no fixed value; 3: 3 intervals or seconds 
			"unlimited INT, " +		
			"description TEXT )";		
		dbcmd.ExecuteNonQuery();
	}
	
	//if this changes, runType.cs constructor should change 
	protected internal static void initializeTableRunIntervalType()
	{
		string [] iniRunTypes = {
			//name:distance:tracksLimited:fixedValue:unlimited:description
			"byLaps:0:1:0:0:Run n laps x distance",
			"byTime:0:0:0:0:Make max laps in n seconds",
			"unlimited:0:0:0:1:Continue running in n distance",	//suppose limited by time
			"20m10times:20:1:10:0:Run 10 times a 20m distance",	//only in more runs
			"7m30seconds:7:0:30:0:Make max laps in 30 seconds",	//only in more runs
			"20m endurance:20:0:0:1:Continue running in 20m distance"	//only in more runs
		};
		foreach(string myRunType in iniRunTypes) {
			RunIntervalTypeInsert(myRunType, true);
		}
	}

	/*
	 * RunType class methods
	 */

	public static void RunTypeInsert(string myRun, bool dbconOpened)
	{
		string [] myStr = myRun.Split(new char[] {':'});
		if(! dbconOpened) {
			dbcon.Open();
		}
		dbcmd.CommandText = "INSERT INTO runType" + 
				"(uniqueID, name, distance, description)" +
				" VALUES (NULL, '"
				+ myStr[0] + "', " + myStr[1] + ", '" +	//name, distance
				myStr[2] + "')" ;	//description
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		if(! dbconOpened) {
			dbcon.Close();
		}
	}
	
	public static void RunIntervalTypeInsert(string myRun, bool dbconOpened)
	{
		string [] myStr = myRun.Split(new char[] {':'});
		if(! dbconOpened) {
			dbcon.Open();
		}
		dbcmd.CommandText = "INSERT INTO runIntervalType" + 
				"(uniqueID, name, distance, tracksLimited, fixedValue, unlimited, description)" +
				" VALUES (NULL, '"
				+ myStr[0] + "', " + myStr[1] + ", " +	//name, distance
				myStr[2] + ", " + myStr[3] + ", " +	//tracksLimited, fixedValue
				myStr[4] + ", '" + myStr[5] +"')" ;	//unlimited, description
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		if(! dbconOpened) {
			dbcon.Close();
		}
	}

	public static string[] SelectRunTypes(string allRunsName, bool onlyName) 
	{
		//allRunsName: add and "allRunsName" value
		//onlyName: return only type name
	
		string whereString = "";
		
		dbcon.Open();
		dbcmd.CommandText = "SELECT * " +
			" FROM runType " +
			whereString +
			" ORDER BY uniqueID";
		
		Console.WriteLine(dbcmd.CommandText.ToString());
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
		dbcon.Close();

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
			//Console.WriteLine("{0} - {1}", myTypes[count-1], count-1);
		}
		foreach (string line in myArray) {
			myTypes [count++] = line;
			//Console.WriteLine("{0} - {1}", myTypes[count-1], count-1);
		}

		return myTypes;
	}

	public static string[] SelectRunIntervalTypes(string allRunsName, bool onlyName) 
	{
		dbcon.Open();
		dbcmd.CommandText = "SELECT * " +
			" FROM runIntervalType " +
			" ORDER BY uniqueID";
		
		Console.WriteLine(dbcmd.CommandText.ToString());
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
						reader[6].ToString() 		//description
					    );
			}
			count ++;
		}

		reader.Close();
		dbcon.Close();

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

	public static RunType SelectAndReturnRunIntervalType(string typeName) 
	{
		dbcon.Open();
		dbcmd.CommandText = "SELECT * " +
			" FROM runIntervalType " +
			" WHERE name  = '" + typeName +
			"' ORDER BY uniqueID";
		
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		RunType myRunType = new RunType();
		
		while(reader.Read()) {
			myRunType.Name = reader[1].ToString();
			
			myRunType.Distance = Convert.ToInt32( reader[2].ToString() );
			
			myRunType.HasIntervals = true;
			
			if(reader[3].ToString() == "1") { myRunType.TracksLimited = true; }
			else { myRunType.TracksLimited = false; }
			
			myRunType.FixedValue = Convert.ToInt32( reader[4].ToString() );
			if(reader[5].ToString() == "1") { myRunType.Unlimited = true; }
			else { myRunType.Unlimited = false; }
		}

		reader.Close();
		dbcon.Close();

		return myRunType;
	}
	
	public static bool Exists(string typeName)
	{
		dbcon.Open();
		dbcmd.CommandText = "SELECT uniqueID FROM runType " +
			" WHERE LOWER(name) == LOWER('" + typeName + "')" ;
		Console.WriteLine(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
	
		bool exists = new bool();
		exists = false;
		
		if (reader.Read()) {
			exists = true;
		}
		Console.WriteLine("exists = {0}", exists.ToString());

		return exists;
	}
	
	public static double Distance (string typeName) 
	{
		dbcon.Open();
		dbcmd.CommandText = "SELECT distance " +
			" FROM runType " +
			" WHERE name == '" + typeName + "'";
		
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		double distance = 0;
		while(reader.Read()) {
			distance = Convert.ToDouble(reader[0].ToString());
		}
		return distance;
	}

}	
