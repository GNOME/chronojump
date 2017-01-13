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
 * Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */


using System;
using System.Data;
using System.IO;
using System.Collections; //ArrayList
using Mono.Data.Sqlite;

using Mono.Unix;


class SqliteSpeciallity : Sqlite
{
	protected internal static new void createTable()
	 {
		dbcmd.CommandText = 
			"CREATE TABLE " + Constants.SpeciallityTable + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"sportID INT, " +
			"name TEXT )";
		dbcmd.ExecuteNonQuery();
	 }

	// intialize sport table
	protected internal static void initialize()
	{
		conversionSubRateTotal = Speciallities.Length;
		conversionSubRate = 0;

		using(SqliteTransaction tr = dbcon.BeginTransaction())
		{
			using (SqliteCommand dbcmdTr = dbcon.CreateCommand())
			{
				dbcmdTr.Transaction = tr;
	
				foreach(string myString in Speciallities) {
					string [] strFull = myString.Split(new char[] {':'});
					string sportName = strFull[0];
					string speciallityEnglishName = strFull[1];
					int sportID = SqliteSport.SelectID(sportName);
					Insert(true, dbcmdTr, sportID, speciallityEnglishName);
					conversionSubRate ++;
				}
			}
			tr.Commit();
		}
	}

	public static int Insert(bool dbconOpened, SqliteCommand mycmd, int sportID, string speciallityName)
	{
		if(! dbconOpened)
			Sqlite.Open();

		string myString = "INSERT INTO " + Constants.SpeciallityTable + 
			" (uniqueID, sportID, name) VALUES (NULL, " + sportID + ", \"" + speciallityName + "\")"; 
		
		mycmd.CommandText = myString;
		LogB.SQL(mycmd.CommandText.ToString());
		mycmd.ExecuteNonQuery();

		//int myLast = dbcon.LastInsertRowId;
		//http://stackoverflow.com/questions/4341178/getting-the-last-insert-id-with-sqlite-net-in-c
		myString = @"select last_insert_rowid()";
		mycmd.CommandText = myString;
		int myLast = Convert.ToInt32(mycmd.ExecuteScalar()); // Need to type-cast since `ExecuteScalar` returns an object.
		
		if(! dbconOpened)
			Sqlite.Close();

		return myLast;
	}

	public static string Select(bool dbconOpened, int uniqueID)
	{
		if(uniqueID == -1)
			return "";

		if(! dbconOpened)
			Sqlite.Open();
		
		dbcmd.CommandText = "SELECT name FROM " + Constants.SpeciallityTable + " WHERE uniqueID == " + uniqueID;
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		reader.Read();

		string speciallityName = reader[0].ToString(); //name
	
		reader.Close();
		
		if(! dbconOpened)
			Sqlite.Close();
		
		return Catalog.GetString(speciallityName);
	}
	
	public static string [] SelectAll(bool showUndefined, int sportFilter) 
	{
		string whereString = "";
		if(sportFilter != -1)
			whereString = " WHERE sportID == " + sportFilter;

		Sqlite.Open();
		SqliteDataReader reader;
		ArrayList myArray = new ArrayList(2);
		int count = 0;

		dbcmd.CommandText = "SELECT uniqueID, name " +
			" FROM " + Constants.SpeciallityTable + " " +
			whereString +
			" ORDER BY uniqueID";
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		reader = dbcmd.ExecuteReader();

		if(showUndefined) {
			myArray.Add("-1:" + Constants.SpeciallityUndefined + ":" + Catalog.GetString(Constants.SpeciallityUndefined)); 
			count ++;
		}
		while(reader.Read()) {
			myArray.Add(reader[0].ToString() + ":" + reader[1].ToString() + ":" + Catalog.GetString(reader[1].ToString()));
			count ++;
		}
		reader.Close();
		Sqlite.Close();

		string [] myReturn = new string[count];
		count = 0;
		foreach (string line in myArray) {
			myReturn [count++] = line;
		}

		return myReturn;
	}
	
	//speciallities of some sports
	//string will be shown in user language
	//speciallities use sport names and not ids, because in the future if sports grow it can be messed with user sports
	//when it's not defined it will be -1
	private static string [] Speciallities = {
		//"-1:" + Constants.SpeciallityUndefined + ":" + Catalog.GetString(Constants.SpeciallityUndefined), 
		
		"Aquatics:" + "Diving" + ":" + Catalog.GetString("Diving"), 
		"Aquatics:" + "Swimming" + ":" + Catalog.GetString("Swimming"), 
		"Aquatics:" + "Synchronized Swimming" + ":" + Catalog.GetString("Synchronized Swimming"), 
		"Aquatics:" + "Waterpolo" + ":" + Catalog.GetString("Waterpolo"), 
		
		"Athletics:" + "Runs, sprints" + ":" + Catalog.GetString("Runs, Sprints"), 
		"Athletics:" + "Runs, middle-distance" + ":" + Catalog.GetString("Runs, Middle-distance"), 
		"Athletics:" + "Runs, long-distance" + ":" + Catalog.GetString("Runs, Long-distance"), 
		"Athletics:" + "Jumps" + ":" + Catalog.GetString("Jumps"), 
		"Athletics:" + "Throws" + ":" + Catalog.GetString("Throws"), 
		"Athletics:" + "Combined" + ":" + Catalog.GetString("Combined"), 
		
		"Bobsleigh:" + "Bobsleigh" + ":" + Catalog.GetString("Bobsleigh"), 
		"Bobsleigh:" + "Skeleton" + ":" + Catalog.GetString("Skeleton"), 

		"Canoe-Cayak:" + "Flatwater" + ":" + Catalog.GetString("Flatwater"), 
		"Canoe-Cayak:" + "Slalom" + ":" + Catalog.GetString("Slalom"), 
		
		"Cycling:" + "Cycling BMX" + ":" + Catalog.GetString("Cycling BMX"), 
		"Cycling:" + "Cycling Road" + ":" + Catalog.GetString("Cycling Road"), 
		"Cycling:" + "Cycling Track" + ":" + Catalog.GetString("Cycling Track"), 
		"Cycling:" + "Mountain Bike" + ":" + Catalog.GetString("Mountain Bike"), 
		
		"Equestrian:" + "Dressage" + ":" + Catalog.GetString("Dressage"), 
		"Equestrian:" + "Eventing" + ":" + Catalog.GetString("Eventing"), 
		"Equestrian:" + "jumping" + ":" + Catalog.GetString("jumping"), 

		"Gymnastics:" + "Artistic" + ":" + Catalog.GetString("Artistic"), 
		"Gymnastics:" + "Rhythmic" + ":" + Catalog.GetString("Rhythmic"), 
		"Gymnastics:" + "Trampoline" + ":" + Catalog.GetString("Trampoline"), 

		"Skating:" + "Figure skating" + ":" + Catalog.GetString("Figure skating"), 
		"Skating:" + "Short Track Speed Skating" + ":" + Catalog.GetString("Short Track Speed Skating"), 
		"Skating:" + "Speed skating" + ":" + Catalog.GetString("Speed skating"), 

		"Skiing:" + "Alpine Skiing" + ":" + Catalog.GetString("Alpine Skiing"), 
		"Skiing:" + "Cross Country Skiing" + ":" + Catalog.GetString("Cross Country Skiing"), 
		"Skiing:" + "Freestyle Skiing" + ":" + Catalog.GetString("Freestyle Skiing"), 
		"Skiing:" + "Nordic Combined" + ":" + Catalog.GetString("Nordic Combined"), 
		"Skiing:" + "Ski Jumping" + ":" + Catalog.GetString("Ski Jumping"), 
		"Skiing:" + "Snowboard" + ":" + Catalog.GetString("Snowboard"), 

		"Volleyball:" + "Beach volleyball" + ":" + Catalog.GetString("Beach volleyball"), 
		"Volleyball:" + "Volleyball" + ":" + Catalog.GetString("Volleyball"), 

		"Wrestling:" + "Freestyle" + ":" + Catalog.GetString("Freestyle"), 
		"Wrestling:" + "Greco-Roman" + ":" + Catalog.GetString("Greco-Roman"), 
	};

	//convert from DB 0.54 to 0.55
	public static void InsertUndefined(bool dbconOpened)
	{
		if(! dbconOpened)
			Sqlite.Open();

		string myString = "INSERT INTO " + Constants.SpeciallityTable + 
			" (uniqueID, sportID, name) VALUES (-1, -1, \"\")"; 
		
		dbcmd.CommandText = myString;
		dbcmd.ExecuteNonQuery();
		
		if(! dbconOpened)
			Sqlite.Close();
	}
}
