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


class SqliteSport : Sqlite
{
	protected internal static new void createTable()
	 {
		dbcmd.CommandText = 
			"CREATE TABLE " + Constants.SportTable + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"name TEXT, " +
			"userDefined INT, " +
			"hasSpeciallities INT, " +
			"graphLink TEXT )";
		dbcmd.ExecuteNonQuery();
	 }

	// intialize sport table
	protected internal static void initialize()
	{
		conversionSubRateTotal = sportsChronojump.Length;
		conversionSubRate = 0;

		using(SqliteTransaction tr = dbcon.BeginTransaction())
		{
			using (SqliteCommand dbcmdTr = dbcon.CreateCommand())
			{
				dbcmdTr.Transaction = tr;
	
				foreach(string sportString in sportsChronojump) {
					//put in db only english name
					string [] sportFull = sportString.Split(new char[] {':'});
					//Sport sport = new Sport(sportFull[0]);
					Insert(true, dbcmdTr, "-1", sportFull[0], false,  		//dbconOpened, not user defined 
							Util.StringToBool(sportFull[2]), sportFull[3]);	//hasSpeciallities, graphLink
					conversionSubRate ++;
				}
			}
			tr.Commit();
		}
	}

	//called from some Chronojump methods
	//adds dbcmd to be used on next Insert method
	public static int Insert(bool dbconOpened, string uniqueID, string name, bool userDefined, bool hasSpeciallities, string graphLink)
	{
		return Insert(dbconOpened, dbcmd, uniqueID, name, userDefined, hasSpeciallities, graphLink);

	}
	//Called from initialize
	public static int Insert(bool dbconOpened, SqliteCommand mycmd, string uniqueID, string name, bool userDefined, bool hasSpeciallities, string graphLink)
	{
		if(! dbconOpened)
			Sqlite.Open();

		if(uniqueID == "-1")
			uniqueID = "NULL";

		string myString = "INSERT INTO " + Constants.SportTable + 
			" (uniqueID, name, userDefined, hasSpeciallities, graphLink) VALUES (" + uniqueID + ", \"" + name + "\", " + 
			Util.BoolToInt(userDefined) + ", " + Util.BoolToInt(hasSpeciallities) + ", \"" + graphLink + "\")";
		
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

	public static Sport Select(bool dbconOpened, int uniqueID)
	{
		if(! dbconOpened)
			Sqlite.Open();
		
		dbcmd.CommandText = "SELECT * FROM " + Constants.SportTable + " WHERE uniqueID == " + uniqueID;
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		Sport mySport = null;
		while(reader.Read())
			mySport = new Sport(
					uniqueID,
					reader[1].ToString(), //name
					Util.IntToBool(Convert.ToInt32(reader[2])), //userDefined
					Util.IntToBool(Convert.ToInt32(reader[3])), //hasSpeciallities
					reader[4].ToString() //graphLink
					);

		reader.Close();

		//manage problem if sport was deleted, return 1st sport: "undefined"
		if(mySport == null)
		{
			//sportsChronojump[0] is "undefined"
			string [] sportsFull = sportsChronojump[0].Split(new char[] {':'});
			mySport = new Sport(
					1, 					//uniqueID
					sportsFull[0], 				//name
					false, 					//userDefined
					Util.StringToBool(sportsFull[2]), 	//hasSpeciallities
					sportsFull[3] 				//graphLink
					);
		}
		
		if(! dbconOpened)
			Sqlite.Close();

		return mySport;
	}
		
	public static int SelectID(string name)
	{
		//Sqlite.Open();
		
		dbcmd.CommandText = "SELECT uniqueID FROM " + Constants.SportTable + " WHERE name == \"" + name + "\"";
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		reader.Read();
		int myID = Convert.ToInt32(reader[0]);
		reader.Close();
		
		//Sqlite.Close();
		
		return myID;
	}

	public static string [] SelectAll() 
	{
		Sqlite.Open();
		SqliteDataReader reader;
		ArrayList myArray = new ArrayList(2);
		int count = 0;

		dbcmd.CommandText = "SELECT uniqueID, name, userDefined " +
			" FROM " + Constants.SportTable + " " +
			" ORDER BY name";
		
		dbcmd.ExecuteNonQuery();
		reader = dbcmd.ExecuteReader();
		string userDefinedString;
		while(reader.Read()) {
			userDefinedString = "";
			if(reader[2].ToString() == "1")
				userDefinedString = "(" + Catalog.GetString("user") + ")";

			myArray.Add(reader[0].ToString() + ":" + 
					reader[1].ToString() + ":" + 
					userDefinedString + Catalog.GetString(reader[1].ToString())
					);
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

	/*
	 * the Catalog.GetString is only for having a translation that will be used on display sport name if available
	 * don't sportuserchecks here, doit in the database because sports will grow
	 * all this are obviously NOT user defined
	 * last string is for graphLink
	 */
	private static string [] sportsChronojump = {
		//true or false means if it has speciallities
		Constants.SportUndefined + ":" + Catalog.GetString(Constants.SportUndefined) + ":" + false + ":" + "", //will be 1 (it's also written in Constants.SportUndefinedID
		Constants.SportNone + ":" + Catalog.GetString(Constants.SportNone) + ":" + false + ":" + "", 	 //will be 2 (it's also written in Constants.SportNoneID
		"Aquatics:" + Catalog.GetString("Aquatics") + ":" + true + ":" + "",
		"Archery:" + Catalog.GetString("Archery") + ":" + false + ":" + "", 
		"Athletics:" + Catalog.GetString("Athletics") + ":" + true + ":" + "",
		"Badminton:" + Catalog.GetString("Badminton") + ":" + false + ":" + "", 
		"Baseball:" + Catalog.GetString("Baseball") + ":" + false + ":" + "", 
		"Basketball:" + Catalog.GetString("Basketball") + ":" + false + ":" + "", 
		"Biathlon:" + Catalog.GetString("Biathlon") + ":" + false + ":" + "", 
		"Bobsleigh:" + Catalog.GetString("Bobsleigh") + ":" + true + ":" + "", 
		"Boxing:" + Catalog.GetString("Boxing") + ":" + false + ":" + "", 
		"Canoe-Cayak:" + Catalog.GetString("Canoe-Cayak") + ":" + true + ":" + "", 
		"Curling:" + Catalog.GetString("Curling") + ":" + false + ":" + "", 
		"Cycling:" + Catalog.GetString("Cycling") + ":" + true + ":" + "", 
		"Equestrian:" + Catalog.GetString("Equestrian") + ":" + true + ":" + "", 
		"Fencing:" + Catalog.GetString("Fencing") + ":" + false + ":" + "", 
		"Football:" + Catalog.GetString("Football") + ":" + false + ":" + "", 
		"Gymnastics:" + Catalog.GetString("Gymnastics") + ":" + true + ":" + "", 
		"Handball:" + Catalog.GetString("Handball") + ":" + false + ":" + "", 
		"Hockey:" + Catalog.GetString("Hockey") + ":" + false + ":" + "", 
		"Ice Hockey:" + Catalog.GetString("Ice Hockey") + ":" + false + ":" + "", 
		"Judo:" + Catalog.GetString("Judo") + ":" + false + ":" + "", 
		"Luge:" + Catalog.GetString("Luge") + ":" + false + ":" + "", 
		"Modern Pentathlon:" + Catalog.GetString("Modern Pentathlon") + ":" + false + ":" + "", 
		"Rowing:" + Catalog.GetString("Rowing") + ":" + false + ":" + "", 
		"Sailing:" + Catalog.GetString("Sailing") + ":" + false + ":" + "", 
		"Shooting:" + Catalog.GetString("Shooting") + ":" + false + ":" + "", 
		"Skating:" + Catalog.GetString("Skating") + ":" + true + ":" + "", 
		"Skiing:" + Catalog.GetString("Skiing") + ":" + true + ":" + "", 
		"Softball:" + Catalog.GetString("Softball") + ":" + false + ":" + "", 
		"Table Tennis:" + Catalog.GetString("Table Tennis") + ":" + false + ":" + "", 
		"Taekwondo:" + Catalog.GetString("Taekwondo") + ":" + false + ":" + "", 
		"Tennis:" + Catalog.GetString("Tennis") + ":" + false + ":" + "", 
		"Triathlon:" + Catalog.GetString("Triathlon") + ":" + false + ":" + "", 
		"Volleyball:" + Catalog.GetString("Volleyball") + ":" + true + ":" + "", 
		"Weightlifting:" + Catalog.GetString("Weightlifting") + ":" + false + ":" + "", 
		"Wrestling:" + Catalog.GetString("Wrestling") + ":" + true + ":" + "", 
		//add ALWAYS below
	};


}
