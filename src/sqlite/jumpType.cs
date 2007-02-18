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
			"CREATE TABLE jumpType ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"name TEXT, " +
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
			"SJ+:1:1:SJ jump with weight", 
			"CMJ:1:0:CMJ jump", 
			"CMJ+:1:1:CMJ jump with weight", 
			"ABK:1:0:ABK jump", 
			"ABK+:1:1:ABK jump with weight", 
			"DJ:0:0:DJ jump"
		};
		foreach(string myJumpType in iniJumpTypes) {
			JumpTypeInsert(myJumpType, true);
		}
	}
	
	//creates table containing the types of repetitive Jumps
	//following INT values are booleans
	protected internal static void createTableJumpRjType()
	{
		dbcmd.CommandText = 
			"CREATE TABLE jumpRjType ( " +
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
			//name:startIn:weight:jumpsLimited:limitValue:unlimited:description
			"RJ(j):0:0:1:0:RJ limited by jumps",
			"RJ(t):0:0:0:0:RJ limited by time",
			"RJ(unlimited):1:0:1:-1:Jump unlimited until finish is clicked",
			"triple jump:0:0:1:3:Triple jump"
		};
		foreach(string myJumpType in iniJumpTypes) {
			JumpRjTypeInsert(myJumpType, true);
		}
	}

	/*
	 * JumpType class methods
	 */

	public static void JumpTypeInsert(string myJump, bool dbconOpened)
	{
		string [] myStr = myJump.Split(new char[] {':'});
		if(! dbconOpened) {
			dbcon.Open();
		}
		dbcmd.CommandText = "INSERT INTO jumpType" + 
				"(uniqueID, name, startIn, weight, description)" +
				" VALUES (NULL, '"
				+ myStr[0] + "', " + myStr[1] + ", " +	//name, startIn
				myStr[2] + ", '" + myStr[3] + "')" ;	//weight, description
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		if(! dbconOpened) {
			dbcon.Close();
		}
	}
	
	public static void JumpRjTypeInsert(string myJump, bool dbconOpened)
	{
		string [] myStr = myJump.Split(new char[] {':'});
		if(! dbconOpened) {
			dbcon.Open();
		}
		dbcmd.CommandText = "INSERT INTO jumpRjType" + 
				"(uniqueID, name, startIn, weight, jumpsLimited, fixedValue, description)" +
				" VALUES (NULL, '"
				+ myStr[0] + "', " + myStr[1] + ", " +	//name, startIn
				myStr[2] + ", " + myStr[3] + ", " +	//weight, jumpsLimited
				myStr[4] + ", '" + myStr[5] + "')" ;	//fixedValue, description
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		if(! dbconOpened) {
			dbcon.Close();
		}
	}

	public static string[] SelectJumpTypes(string allJumpsName, string filter, bool onlyName) 
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
		
		dbcon.Open();
		dbcmd.CommandText = "SELECT * " +
			" FROM jumpType " +
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
						reader[2].ToString() + ":" + 	//startIn
						reader[3].ToString() + ":" + 	//weight
						reader[4].ToString() 		//description
					    );
			}
			count ++;
		}

		reader.Close();
		dbcon.Close();

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
			//Console.WriteLine("{0} - {1}", myTypes[count-1], count-1);
		}
		foreach (string line in myArray) {
			myTypes [count++] = line;
			//Console.WriteLine("{0} - {1}", myTypes[count-1], count-1);
		}

		return myTypes;
	}

	public static string[] SelectJumpRjTypes(string allJumpsName, bool onlyName) 
	{
		dbcon.Open();
		dbcmd.CommandText = "SELECT * " +
			" FROM jumpRjType " +
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
		dbcon.Close();

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
	
	public static JumpType SelectAndReturnJumpRjType(string typeName) 
	{
		dbcon.Open();
		dbcmd.CommandText = "SELECT * " +
			" FROM jumpRjType " +
			" WHERE name  = '" + typeName +
			"' ORDER BY uniqueID";
		
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		JumpType myJumpType = new JumpType();
		
		while(reader.Read()) {
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

		reader.Close();
		dbcon.Close();

		return myJumpType;
	}
	
	public static bool Exists(string typeName)
	{
		dbcon.Open();
		dbcmd.CommandText = "SELECT uniqueID FROM jumpType " +
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

	public static bool HasWeight(string typeName) 
	{
		dbcon.Open();
		dbcmd.CommandText = "SELECT weight " +
			" FROM jumpType " +
			" WHERE name == '" + typeName + "'";
		
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		bool hasWeight = false;
		while(reader.Read()) {
			if(reader[0].ToString() == "1") {
				hasWeight = true;
				Console.WriteLine("found type: hasWeight");
			} else {
				Console.WriteLine("found type: NO hasWeight");
			}
		}
		return hasWeight;
	}

	//we know if it has fall if it starts in 
	public static bool HasFall(string tableName, string typeName) 
	{
		dbcon.Open();
		dbcmd.CommandText = "SELECT startIn " +
			" FROM " + tableName +
			" WHERE name == '" + typeName + "'";
		
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		bool hasFall = true;
		while(reader.Read()) {
			if(reader[0].ToString() == "1") {
				hasFall = false;
			}
		}
		return hasFall;
	}
}	
