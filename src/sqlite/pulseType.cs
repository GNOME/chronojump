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


class SqlitePulseType : Sqlite
{
	/*
	 * create and initialize tables
	 */
	
	//creates table containing the types of simple Pulses
	//following INT values are booleans
	protected internal static void createTablePulseType()
	{
		dbcmd.CommandText = 
			"CREATE TABLE " + Constants.PulseTypeTable + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"name TEXT, " +
			"fixedPulse FLOAT, " + //-1: not fixed, 0,344: 0,344 seconds between pulses
			"totalPulsesNum FLOAT, " +  //-1: not fixed (unlimited), 5: 5 times
			"description TEXT )";		
		dbcmd.ExecuteNonQuery();
	}
	
	//if this changes, pulseType.cs constructor should change 
	protected internal static void initializeTablePulseType()
	{
		string [] iniPulseTypes = {
			//name:fixedPulse:totalPulsesNum:description
			"Free:-1:-1:free pulseStep mode",	
			"Custom:-1:-1:select pulseStep" //the difference between free and custom is: on custom user is asked for the pulseType, on free not
		};
		foreach(string myPulseType in iniPulseTypes) {
			Insert(myPulseType, true);
		}
	}
	
	/*
	 * PulseType class methods
	 */

	public static void Insert(string myPulse, bool dbconOpened)
	{
		string [] myStr = myPulse.Split(new char[] {':'});
		if(! dbconOpened) {
			Sqlite.Open();
		}
		dbcmd.CommandText = "INSERT INTO " + Constants.PulseTypeTable +  
				" (uniqueID, name, fixedPulse, totalPulsesNum, description)" +
				" VALUES (NULL, \""
				+ myStr[0] + "\", " + myStr[1] + ", " +	//name, fixedPulse
				myStr[2] + ", \"" + myStr[3] + "\")" ;	//totalPulsesNum, description
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		if(! dbconOpened) {
			Sqlite.Close();
		}
	}
	
	public static string[] SelectPulseTypes(string allPulsesName, bool onlyName) 
	{
		//allPulsesName: add and "allPulsesName" value
		//onlyName: return only type name
	
		Sqlite.Open();
		dbcmd.CommandText = "SELECT * " +
			" FROM " + Constants.PulseTypeTable + 
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
						reader[2].ToString() + ":" + 	//fixedPulse
						reader[3].ToString() + ":" + 	//totalPulsesNum
						reader[4].ToString() 		//description
					    );
			}
			count ++;
		}

		reader.Close();
		Sqlite.Close();

		int numRows;
		if(allPulsesName != "") {
			numRows = count +1;
		} else {
			numRows = count;
		}
		string [] myTypes = new string[numRows];
		count =0;
		if(allPulsesName != "") {
			myTypes [count++] = allPulsesName;
			//LogB.SQL("{0} - {1}", myTypes[count-1], count-1);
		}
		foreach (string line in myArray) {
			myTypes [count++] = line;
			//LogB.SQL("{0} - {1}", myTypes[count-1], count-1);
		}

		return myTypes;
	}

	public static PulseType SelectAndReturnPulseType(string typeName) 
	{
		Sqlite.Open();
		dbcmd.CommandText = "SELECT * " +
			" FROM " + Constants.PulseTypeTable +
			" WHERE name  = \"" + typeName +
			"\" ORDER BY uniqueID";
		
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		PulseType myPulseType = new PulseType();
		
		while(reader.Read()) {
			myPulseType.Name = reader[1].ToString();
			myPulseType.FixedPulse = Convert.ToDouble(reader[2].ToString());
			myPulseType.TotalPulsesNum = Convert.ToInt32(reader[3]);
		}

		reader.Close();
		Sqlite.Close();

		return myPulseType;
	}
}	
