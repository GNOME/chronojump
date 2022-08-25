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
 * Copyright (C) 2004-2022   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Data;
using System.IO;
using System.Collections; //ArrayList
using System.Collections.Generic; //List<T>
using Mono.Data.Sqlite;


class SqliteJumpType : Sqlite
{
	/*
	 * create and initialize tables
	 */
	
	//creates table containing the types of simple Jumps
	//following INT values are booleans
	protected internal static void createTableJumpType()
	{
		dbcmd.CommandText = 
			"CREATE TABLE " + Constants.JumpTypeTable + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"name TEXT, " + //if name it's Constants.TakeOffName or Constants.TakeOffWeightName it's an exception and will record only one tc
			"startIn INT, " + //if it starts inside or outside the platform
			"weight INT, " + 
			"description TEXT )";		
		dbcmd.ExecuteNonQuery();
	}
	
	//if this changes, jumpType.cs constructor should change 
	protected internal static void initializeTableJumpType()
	{
		string [] iniJumpTypes = {
			//name:startIn:weight:description
			"Free:1:0:Free jump", 
			"SJ:1:0:SJ jump", 
			"SJl:1:1:SJ jump with weight", 
			"CMJ:1:0:CMJ jump", 
			"CMJl:1:1:CMJ jump with weight", 
			"slCMJleft:1:0:Single-leg CMJ jump",
			"slCMJright:1:0:Single-leg CMJ jump",
			"ABK:1:0:ABK jump", 
			"ABKl:1:1:ABK jump with weight", 
			//"Max:1:0:;Maximum jump", 
			//"DJ:0:0:DJ jump",
			"DJa:0:0:DJ jump using arms",
			"DJna:0:0:DJ jump without using arms",
			"Rocket:1:0:Rocket jump",
			"TakeOff:0:0:Take off",
			"TakeOffWeight:0:1:Take off with weight"
		};
		conversionSubRateTotal = iniJumpTypes.Length;
		conversionSubRate = 0;

		using(SqliteTransaction tr = dbcon.BeginTransaction())
		{
			using (SqliteCommand dbcmdTr = dbcon.CreateCommand())
			{
				dbcmdTr.Transaction = tr;
	
				foreach(string myJumpType in iniJumpTypes) {
					JumpTypeInsert(-1, myJumpType, true, dbcmdTr);
					conversionSubRate ++;
				}
			}
			tr.Commit();
		}

		AddGraphLinks();	
	}

	//put the graph links on the db
	//don't put the full description because if the user changes language, description will be in old lang
	//description will be on src/jumpType
	public static void AddGraphLinks() {
		using(SqliteTransaction tr = dbcon.BeginTransaction())
		{
			using (SqliteCommand dbcmdTr = dbcon.CreateCommand())
			{
				dbcmdTr.Transaction = tr;

				SqliteEvent.GraphLinkInsert (Constants.JumpTable, "Free", "jump_free.png", true, dbcmdTr);
				SqliteEvent.GraphLinkInsert (Constants.JumpTable, "SJ", "jump_sj.png", true, dbcmdTr);
				SqliteEvent.GraphLinkInsert (Constants.JumpTable, "SJl", "jump_sj_l.png", true, dbcmdTr);
				SqliteEvent.GraphLinkInsert (Constants.JumpTable, "CMJ", "jump_cmj.png", true, dbcmdTr);
				SqliteEvent.GraphLinkInsert (Constants.JumpTable, "CMJl", "jump_cmj_l.png", true, dbcmdTr);
				SqliteEvent.GraphLinkInsert (Constants.JumpTable, "ABK", "jump_abk.png", true, dbcmdTr);
				SqliteEvent.GraphLinkInsert (Constants.JumpTable, "ABKl", "jump_abk_l.png", true, dbcmdTr);
				//SqliteEvent.GraphLinkInsert (Constants.JumpTable, "Max", "jump_max.png", true, dbcmdTr); //we already have "Free"
				SqliteEvent.GraphLinkInsert (Constants.JumpTable, "Rocket", "jump_rocket.png", true, dbcmdTr);
				SqliteEvent.GraphLinkInsert (Constants.JumpTable, "DJa", "jump_dj_a.png", true, dbcmdTr);
				SqliteEvent.GraphLinkInsert (Constants.JumpTable, "DJna", "jump_dj.png", true, dbcmdTr);
			}
			tr.Commit();
		}
	}

	//creates table containing the types of repetitive Jumps
	//following INT values are booleans
	protected internal static void createTableJumpRjType()
	{
		dbcmd.CommandText = 
			"CREATE TABLE " + Constants.JumpRjTypeTable + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"name TEXT, " +
			"startIn INT, " + //if it starts inside or outside the platform
			"weight INT, " + 
			"jumpsLimited INT, " +  //1 limited by jumps; 0 limited by time
			"fixedValue FLOAT, " +  //0: no fixed value (ask in jump_extra widget), 
						//3.5: 3.5 jumps or seconds (don't ask in jump_extra)
						//-1: unlimited: jump until "finish" button is clicked 
						//	don't ask in jump_extra
						//	always comes with jumpsLimited value)
						//	in runs, unlimited goes with seconds, this is because
						//	unlimited jumps can finish when a certain number is reached,
						//	and it's easy to stop just when on jump is finished.
						//	In runs, by the other way, sometimes is not possible to arrive
						//	to the end of the track, and limitation value is seconds
			"description TEXT )";		
		dbcmd.ExecuteNonQuery();
	}
	
	//if this changes, jumpType.cs constructor should change 
	protected internal static void initializeTableJumpRjType()
	{
		string [] iniJumpTypes = {
			//name:startIn:weight:jumpsLimited:limitValue:description
			"RJ(j):0:0:1:0:RJ limited by jumps",
			"RJ(t):0:0:0:0:RJ limited by time",
			"RJ(unlimited):1:0:1:-1:Jump unlimited until finish is clicked",
			"RJ(hexagon):1:0:1:18:Reactive Jump on a hexagon until three full revolutions are done",
			"triple jump:0:0:1:3:Triple jump",
			//"RunAnalysis:0:0:1:-1:Run between two photocells recording contact and flight times in contact platform/s. Until finish button is clicked."
		};

		using(SqliteTransaction tr = dbcon.BeginTransaction())
		{
			using (SqliteCommand dbcmdTr = dbcon.CreateCommand())
			{
				dbcmdTr.Transaction = tr;

				foreach(string myJumpType in iniJumpTypes) {
					JumpRjTypeInsert(-1, myJumpType, true, dbcmdTr);
				}
			}
			tr.Commit();
		}
		
		AddGraphLinksRj();	
	}

	public static void AddGraphLinksRj() {
		using(SqliteTransaction tr = dbcon.BeginTransaction())
		{
			using (SqliteCommand dbcmdTr = dbcon.CreateCommand())
			{
				dbcmdTr.Transaction = tr;
				
				SqliteEvent.GraphLinkInsert (Constants.JumpRjTable, "RJ(j)", "jump_rj.png", true, dbcmdTr);
				SqliteEvent.GraphLinkInsert (Constants.JumpRjTable, "RJ(t)", "jump_rj.png", true, dbcmdTr);
				SqliteEvent.GraphLinkInsert (Constants.JumpRjTable, "RJ(unlimited)", "jump_rj_in.png", true, dbcmdTr);
				SqliteEvent.GraphLinkInsert (Constants.JumpRjTable, "triple jump", "jump_rj.png", true, dbcmdTr);
			}
			tr.Commit();
		}
	}

	/*
	 * JumpType class methods
	 */

	//called from some Chronojump methods
	//adds dbcmd to be used on next Insert method
	public static void JumpTypeInsert (string myJump, bool dbconOpened)
	{
		JumpTypeInsert (-1, myJump, dbconOpened, dbcmd);
	}
	//used on networks when will force the server id of that exercise
	public static void JumpTypeInsert (int uniqueID, string myJump, bool dbconOpened)
	{
		JumpTypeInsert (uniqueID, myJump, dbconOpened, dbcmd);
	}
	//Called from initialize
	public static void JumpTypeInsert (int uniqueID, string myJump, bool dbconOpened, SqliteCommand mycmd)
	{
		string [] myStr = myJump.Split(new char[] {':'});
		if(! dbconOpened) {
			Sqlite.Open();
		}
		string uniqueIDstr = "NULL";
		if (uniqueID >= 0)
			uniqueIDstr = uniqueID.ToString ();

		mycmd.CommandText = "INSERT INTO " + Constants.JumpTypeTable +  
				" (uniqueID, name, startIn, weight, description)" +
				//" VALUES (NULL, \""
				" VALUES (" + uniqueIDstr + ", \""
				+ myStr[0] + "\", " + myStr[1] + ", " +	//name, startIn
				myStr[2] + ", \"" + myStr[3] + "\")" ;	//weight, description
		LogB.SQL(mycmd.CommandText.ToString());
		mycmd.ExecuteNonQuery();
		if(! dbconOpened) {
			Sqlite.Close();
		}
	}

	//called from some Chronojump methods
	//adds dbcmd to be used on next Insert method
	public static void JumpRjTypeInsert (string myJump, bool dbconOpened)
	{
		JumpRjTypeInsert (-1, myJump, dbconOpened, dbcmd);
	}
	//used on networks when will force the server id of that exercise
	public static void JumpRjTypeInsert (int uniqueID, string myJump, bool dbconOpened)
	{
		JumpRjTypeInsert (uniqueID, myJump, dbconOpened, dbcmd);
	}
	//Called from initialize
	public static void JumpRjTypeInsert (int uniqueID, string myJump, bool dbconOpened, SqliteCommand mycmd)
	{
		string [] myStr = myJump.Split(new char[] {':'});
		if(! dbconOpened) {
			Sqlite.Open();
		}
		string uniqueIDstr = "NULL";
		if (uniqueID >= 0)
			uniqueIDstr = uniqueID.ToString ();

		mycmd.CommandText = "INSERT INTO " + Constants.JumpRjTypeTable + 
				" (uniqueID, name, startIn, weight, jumpsLimited, fixedValue, description)" +
				//" VALUES (NULL, \""
				" VALUES (" + uniqueIDstr + ", \""
				+ myStr[0] + "\", " + myStr[1] + ", " +	//name, startIn
				myStr[2] + ", " + myStr[3] + ", " +	//weight, jumpsLimited
				myStr[4] + ", \"" + myStr[5] + "\")" ;	//fixedValue, description
		LogB.SQL(mycmd.CommandText.ToString());
		mycmd.ExecuteNonQuery();
		if(! dbconOpened) {
			Sqlite.Close();
		}
	}
	
	//use SelectJumpTypes object. Since 1.6.3
	public static List<object> SelectJumpTypesNew(bool dbconOpened, string allJumpsName, string filter, bool onlyName) 
	{
		//allJumpsName: add and "allJumpsName" value
		//filter:
		//	"" all jumps,
		//	"TC" only with previous fall,
		//	"nonTC" only not with previous fall
		//	used in gui/stats.cs
		//onlyName: return only type name

		string whereString = "";
		if(filter == "TC") { whereString = " WHERE startIn == 0 "; }
		else if(filter == "nonTC") { whereString = " WHERE startIn == 1 "; }

		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "SELECT * " +
			" FROM " + Constants.JumpTypeTable + " " +
			whereString +
			" ORDER BY uniqueID";

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		List<object> types = new List<object>();

		SelectJumpTypes type;
		if(allJumpsName != "") {
			type = new SelectJumpTypes(allJumpsName);
			types.Add(type);
		}

		while(reader.Read()) {
			if(onlyName) {
				type = new SelectJumpTypes(reader[1].ToString());
			} else {
				type = new SelectJumpTypes(
						Convert.ToInt32(reader[0]), 	//uniqueID
						reader[1].ToString(),		//nameEnglish
						Util.IntToBool(Convert.ToInt32(reader[2].ToString())), 	//startIn
						Util.IntToBool(Convert.ToInt32(reader[3].ToString())), 	//hasWeight
						reader[4].ToString()); 		//description
			}
			types.Add(type);
		}

		reader.Close();
		closeIfNeeded(dbconOpened);

		return types;
	}
	//on newly cereated code use above method
	public static string[] SelectJumpTypes(bool dbconOpened, string allJumpsName, string filter, bool onlyName) 
	{
		//allJumpsName: add and "allJumpsName" value
		//filter: 
		//	"" all jumps, 
		//	"TC" only with previous fall, 
		//	"nonTC" only not with previous fall
		//	used in gui/stats.cs
		//onlyName: return only type name

		string whereString = "";
		if(filter == "TC") { whereString = " WHERE startIn == 0 "; }
		else if(filter == "nonTC") { whereString = " WHERE startIn == 1 "; }

		if(! dbconOpened)	
			Sqlite.Open();

		dbcmd.CommandText = "SELECT * " +
			" FROM " + Constants.JumpTypeTable + " " +
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
						reader[2].ToString() + ":" + 	//startIn
						reader[3].ToString() + ":" + 	//weight
						reader[4].ToString() 		//description
					    );
			}
			count ++;
		}

		reader.Close();

		if(! dbconOpened)	
			Sqlite.Close();

		int numRows;
		if(allJumpsName != "") {
			numRows = count +1;
		} else {
			numRows = count;
		}
		string [] myTypes = new string[numRows];
		count =0;
		if(allJumpsName != "") {
			myTypes [count++] = allJumpsName;
			//LogB.SQL("{0} - {1}", myTypes[count-1], count-1);
		}
		foreach (string line in myArray) {
			myTypes [count++] = line;
			//LogB.SQL("{0} - {1}", myTypes[count-1], count-1);
		}

		return myTypes;
	}

	//use SelectJumpTypes object. Since 1.6.3
	public static List<object> SelectJumpRjTypesNew(string allJumpsName, bool onlyName) 
	{
		Sqlite.Open();
		dbcmd.CommandText = "SELECT * " +
			" FROM " + Constants.JumpRjTypeTable + " " +
			" ORDER BY uniqueID";

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		List<object> types = new List<object>();

		SelectJumpRjTypes type;
		if(allJumpsName != "") {
			type = new SelectJumpRjTypes(allJumpsName);
			types.Add(type);
		}

		while(reader.Read()) {
			if(onlyName) {
				type = new SelectJumpRjTypes(reader[1].ToString());
			} else {
				type = new SelectJumpRjTypes(
						Convert.ToInt32(reader[0]), 	//uniqueID
						reader[1].ToString(),		//nameEnglish
						Util.IntToBool(Convert.ToInt32(reader[2].ToString())), 	//startIn
						Util.IntToBool(Convert.ToInt32(reader[3].ToString())), 	//hasWeight
						Util.IntToBool(Convert.ToInt32(reader[4].ToString())), 	//jumpsLimited
						Convert.ToDouble(Util.ChangeDecimalSeparator(reader[5].ToString())), 	//fixedValue
						reader[6].ToString() 		//description
					    );
			}
			types.Add(type);
		}

		reader.Close();
		Sqlite.Close();

		return types;
	}
	//on newly cereated code use above method
	public static string[] SelectJumpRjTypes(string allJumpsName, bool onlyName) 
	{
		Sqlite.Open();
		dbcmd.CommandText = "SELECT * " +
			" FROM " + Constants.JumpRjTypeTable + " " +
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
						reader[2].ToString() + ":" + 	//startIn
						reader[3].ToString() + ":" + 	//weight
						reader[4].ToString() + ":" + 	//jumpsLimited
						reader[5].ToString() + ":" + 	//fixedValue
						reader[6].ToString() 		//description
					    );
			}
			count ++;
		}

		reader.Close();
		Sqlite.Close();

		int numRows;
		if(allJumpsName != "") {
			numRows = count +1;
		} else {
			numRows = count;
		}
		string [] myTypes = new string[numRows];
		count =0;
		if(allJumpsName != "") {
			myTypes [count++] = allJumpsName;
		}
		foreach (string line in myArray) {
			myTypes [count++] = line;
		}

		return myTypes;
	}
	
	public static JumpType SelectAndReturnJumpType(string typeName, bool dbconOpened) 
	{
		if(!dbconOpened)
			Sqlite.Open();
		dbcmd.CommandText = "SELECT * " +
			" FROM " + Constants.JumpTypeTable + " " +
			" WHERE name  = \"" + typeName +
			"\" ORDER BY uniqueID";
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		JumpType myJumpType = new JumpType();
		
		while(reader.Read()) {
			myJumpType.UniqueID = Convert.ToInt32(reader[0].ToString());
			myJumpType.Name = reader[1].ToString();
			myJumpType.StartIn = Util.IntToBool(Convert.ToInt32(reader[2].ToString()));
			myJumpType.HasWeight = Util.IntToBool(Convert.ToInt32(reader[3].ToString()));
			myJumpType.Description = reader[4].ToString();
		}

		myJumpType.IsPredefined = myJumpType.FindIfIsPredefined();

		reader.Close();
		if(!dbconOpened)
			Sqlite.Close();

		return myJumpType;
	}

	public static JumpType SelectAndReturnJumpRjType(string typeName, bool dbconOpened) 
	{
		if(!dbconOpened)
			Sqlite.Open();
		dbcmd.CommandText = "SELECT * " +
			" FROM " + Constants.JumpRjTypeTable + " " +
			" WHERE name  = \"" + typeName +
			"\" ORDER BY uniqueID";
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		JumpType myJumpType = new JumpType();
		
		while(reader.Read()) {
			myJumpType.UniqueID = Convert.ToInt32(reader[0].ToString());
			myJumpType.Name = reader[1].ToString();
			
			if(reader[2].ToString() == "1") { myJumpType.StartIn = true; }
			else { myJumpType.StartIn = false; }
			
			if(reader[3].ToString() == "1") { myJumpType.HasWeight = true; }
			else { myJumpType.HasWeight = false; }
			
			myJumpType.IsRepetitive = true;
			
			if(reader[4].ToString() == "1") { myJumpType.JumpsLimited = true; }
			else { myJumpType.JumpsLimited = false; }
			
			myJumpType.FixedValue = Convert.ToInt32( reader[5].ToString() );
		}
		
		myJumpType.IsPredefined = myJumpType.FindIfIsPredefined();

		reader.Close();
		if(!dbconOpened)
			Sqlite.Close();

		return myJumpType;
	}

	//tableName is jumpType or jumpRjType
	public static bool HasWeight(string tableName, string typeName) 
	{
		Sqlite.Open();
		dbcmd.CommandText = "SELECT weight " +
			" FROM " + tableName +
			" WHERE name == \"" + typeName + "\"";
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		bool hasWeight = false;
		while(reader.Read()) {
			if(reader[0].ToString() == "1") {
				hasWeight = true;
				LogB.SQL("found type: hasWeight");
			} else {
				LogB.SQL("found type: NO hasWeight");
			}
		}
		reader.Close();
		Sqlite.Close();
		return hasWeight;
	}

	//we know if it has fall if it starts in 
	public static bool HasFall(string tableName, string typeName) 
	{
		Sqlite.Open();
		dbcmd.CommandText = "SELECT startIn " +
			" FROM " + tableName +
			" WHERE name == \"" + typeName + "\"";
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		bool hasFall = true;
		while(reader.Read()) {
			if(reader[0].ToString() == "1") {
				hasFall = false;
			}
		}
		reader.Close();
		Sqlite.Close();
		return hasFall;
	}

	public static bool IsUnlimited(string typeName)
	{
		Sqlite.Open();
		dbcmd.CommandText = "SELECT fixedValue " +
			" FROM jumpRjType" +
			" WHERE name == \"" + typeName + "\"";

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		bool unlimited = false;
		while(reader.Read()) {
			if(reader[0].ToString() == "-1.0" || reader[0].ToString() == "-1") {
				unlimited = true;
			}
		}
		reader.Close();
		Sqlite.Close();
		return unlimited;
	}

	//updates name	
	public static void Update(string nameOld, string nameNew)
	{
		//Sqlite.Open();
		dbcmd.CommandText = "UPDATE jumpType SET name = \"" + nameNew + 
			"\" WHERE name == \"" + nameOld + "\"";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		//Sqlite.Close();
	}

	public static void UpdateOther(string column, string typeName, string newValue)
	{
		//Sqlite.Open();
		dbcmd.CommandText = "UPDATE jumpType SET " + column + " = \"" + newValue + 
			"\" WHERE name == \"" + typeName + "\"";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		//Sqlite.Close();
	}
	
	public static void Delete(string tableName, string name, bool dbconOpened)
	{
		if(!dbconOpened)
			Sqlite.Open();
		dbcmd.CommandText = "Delete FROM " + tableName + 
			" WHERE name == \"" + name + "\"";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		if(!dbconOpened)
			Sqlite.Close();
	}

	// **************************
	// **** lastJumpTypeParams **
	// **************************
	//for store default params of each jump (simple)
	//note name is on jumpType table but rest of params are related to jump table
	protected internal static void createTableLastJumpSimpleTypeParams()
	{
		dbcmd.CommandText =
			"CREATE TABLE " + Constants.LastJumpSimpleTypeParamsTable + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " + //to order if there are more than one by import problems
			"name TEXT, " +
			"weightIsPercent INT, " + //it is a boolean
			"weightValue FLOAT, " + //decimal is point
			"fallmm INT)"; //-1: start in; >=0: falling height in mm (to be int, can be 0 like int he gui can be 0)
		dbcmd.ExecuteNonQuery();
	}

	public static LastJumpSimpleTypeParams LastJumpSimpleTypeParamsSelect (string name)
	{
		Sqlite.Open();
		dbcmd.CommandText = "SELECT * " +
			" FROM " + Constants.LastJumpSimpleTypeParamsTable + " " +
			" WHERE name  = \"" + name +
			"\" ORDER BY uniqueID DESC"; //to shown last if there are more than one by import problems

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		LastJumpSimpleTypeParams ljstp = new LastJumpSimpleTypeParams(name);

		if (reader.Read())
			ljstp = new LastJumpSimpleTypeParams(
					Convert.ToInt32(reader[0].ToString()), 	 //uniqueID
					reader[1].ToString(), 			//name
					Util.IntToBool(Convert.ToInt32(reader[2].ToString())), //weightIsPercent
					Convert.ToDouble(Util.ChangeDecimalSeparator(reader[3].ToString())), //weightValue
					Convert.ToInt32(reader[4].ToString())); //fallmm

		reader.Close();
		Sqlite.Close();

		return ljstp;
	}

	public static void LastJumpSimpleTypeParamsInsertOrUpdate (LastJumpSimpleTypeParams ljstp)
	{
		LastJumpSimpleTypeParams ljstpFound = LastJumpSimpleTypeParamsSelect(ljstp.name);
//		LogB.Information("LastJumpSimpleTypeParamsInsertOrUpdate search: " + ljstp.ToSqlString());
		if(ljstpFound.uniqueID == -1)
			lastJumpSimpleTypeParamsInsert (ljstp.ToSqlString());
		else
			lastJumpSimpleTypeParamsUpdate (ljstp, ljstpFound.uniqueID); //ljstp comes from the gui choices, but we need the uniqueID
	}

	private static void lastJumpSimpleTypeParamsInsert (string ljstp_string)
	{
		Sqlite.Open();
		dbcmd.CommandText = "INSERT INTO " + Constants.LastJumpSimpleTypeParamsTable +
			" (uniqueID, name, weightIsPercent, weightValue, fallmm) VALUES (" +
			ljstp_string + ")";

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		Sqlite.Close();
	}

	private static void lastJumpSimpleTypeParamsUpdate (LastJumpSimpleTypeParams ljstp, int uniqueID)
	{
		Sqlite.Open();
		LogB.Information("LastJumpSimpleTypeParamsUpdate ljstp: " + ljstp.ToSqlString());
		dbcmd.CommandText = "UPDATE " + Constants.LastJumpSimpleTypeParamsTable +
			" SET weightIsPercent = " + Util.BoolToInt(ljstp.weightIsPercent) +
			", weightValue = " + Util.ConvertToPoint(ljstp.weightValue) +
			", fallmm = " + ljstp.fallmm +
			" WHERE uniqueID = " + uniqueID;

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		Sqlite.Close();
	}

	// ****************************
	// **** lastJumpRjTypeParams **
	// ****************************
	//for store default params of each jump (multiple)
	protected internal static void createTableLastJumpRjTypeParams()
	{
		dbcmd.CommandText =
			"CREATE TABLE " + Constants.LastJumpRjTypeParamsTable + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " + //to order if there are more than one by import problems
			"name TEXT, " +
			"limitedValue INT, " +
			"weightIsPercent INT, " + 	 //boolean
			"weightValue FLOAT, " + 	//decimal is point
			"fallmm INT)"; //-1: start in; >=0: falling height in mm (to be int, can be 0 like int he gui can be 0)
		dbcmd.ExecuteNonQuery();
	}

	public static LastJumpRjTypeParams LastJumpRjTypeParamsSelect (string name)
	{
		Sqlite.Open();
		dbcmd.CommandText = "SELECT * " +
			" FROM " + Constants.LastJumpRjTypeParamsTable + " " +
			" WHERE name  = \"" + name +
			"\" ORDER BY uniqueID DESC"; //to shown last if there are more than one by import problems

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		LastJumpRjTypeParams ljrtp = new LastJumpRjTypeParams(name);

		if (reader.Read())
			ljrtp = new LastJumpRjTypeParams(
					Convert.ToInt32(reader[0].ToString()), 	 //uniqueID
					reader[1].ToString(), 			//name
					Convert.ToInt32(reader[2].ToString()),  //limitedValue
					Util.IntToBool(Convert.ToInt32(reader[3].ToString())), //weightIsPercent
					Convert.ToDouble(Util.ChangeDecimalSeparator(reader[4].ToString())), //weightValue
					Convert.ToInt32(reader[5].ToString())); //fallmm

		reader.Close();
		Sqlite.Close();

		return ljrtp;
	}

	public static void LastJumpRjTypeParamsInsertOrUpdate (LastJumpRjTypeParams ljrtp)
	{
		LastJumpRjTypeParams ljrtpFound = LastJumpRjTypeParamsSelect(ljrtp.name);
//		LogB.Information("LastJumpRjTypeParamsInsertOrUpdate search: " + ljrtp.ToSqlString());
		if(ljrtpFound.uniqueID == -1)
			lastJumpRjTypeParamsInsert (ljrtp.ToSqlString());
		else
			lastJumpRjTypeParamsUpdate (ljrtp, ljrtpFound.uniqueID); //ljrtp comes from the gui choices, but we need the uniqueID
	}

	private static void lastJumpRjTypeParamsInsert (string ljrtp_string)
	{
		Sqlite.Open();
		dbcmd.CommandText = "INSERT INTO " + Constants.LastJumpRjTypeParamsTable +
			" (uniqueID, name, limitedValue, weightIsPercent, weightValue, fallmm) VALUES (" +
			ljrtp_string + ")";

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		Sqlite.Close();
	}

	private static void lastJumpRjTypeParamsUpdate (LastJumpRjTypeParams ljrtp, int uniqueID)
	{
		Sqlite.Open();
		LogB.Information("LastJumpRjTypeParamsUpdate ljrtp: " + ljrtp.ToSqlString());
		dbcmd.CommandText = "UPDATE " + Constants.LastJumpRjTypeParamsTable +
			" SET limitedValue = " + ljrtp.limitedValue +
			", weightIsPercent = " + Util.BoolToInt(ljrtp.weightIsPercent) +
			", weightValue = " + Util.ConvertToPoint(ljrtp.weightValue) +
			", fallmm = " + ljrtp.fallmm +
			" WHERE uniqueID = " + uniqueID;

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		Sqlite.Close();
	}
}	
