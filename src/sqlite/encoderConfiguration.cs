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
 * Copyright (C) 2017-2022   Xavier de Blas <xaviblas@gmail.com>
*/

using System;
using System.Data;
using System.Collections.Generic; //List<T>
using Mono.Data.Sqlite;


class SqliteEncoderConfiguration : Sqlite
{
	/*
	public SqliteEncoderConfiguration() {
	}
	
	~SqliteEncoderConfiguration() {}
	*/

	/*
	 * EncoderConfiguration table
	 */

	protected internal static void createTableEncoderConfiguration()
	{
		//only create it, if not exists (could be a problem updating database, specially from 1.34 - 1.36)
		if(tableExists(true, Constants.EncoderConfigurationTable))
			return;

		dbcmd.CommandText =
			"CREATE TABLE " + Constants.EncoderConfigurationTable + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"encoderGI TEXT, " +
			"active TEXT, " + 		//TRUE or FALSE (one TRUE for GRAVITATORY and one for INERTIAL)
			"name TEXT, " + 
			"encoderConfiguration TEXT, " +	//text separated by ':'
			"description TEXT, " +
			"future1 TEXT, " +
			"future2 TEXT, " +
			"future3 TEXT )";
		dbcmd.ExecuteNonQuery();
	}

	protected internal static void insertDefault(Constants.EncoderGI encoderGI)
	{
		//note DefaultString will not be translated because gettext is changed after this inserts
		if(encoderGI == Constants.EncoderGI.GRAVITATORY)
			Insert(true,
					new EncoderConfigurationSQLObject(
						-1 , encoderGI, true, Constants.DefaultString(), new EncoderConfiguration(), "") //LINEAR, not inertial
					);
		else if(encoderGI == Constants.EncoderGI.INERTIAL)
		{
			EncoderConfiguration ec = new EncoderConfiguration(Constants.EncoderConfigurationNames.ROTARYAXISINERTIAL);
			ec.SetInertialDefaultOptions();
			Insert(true,
					new EncoderConfigurationSQLObject(
						-1 , encoderGI, true, Constants.DefaultString(), ec, "")
					);
		}
		else
			LogB.Error("SqliteEncoderConfiguration.insertDefault with an ALL erroneous value");
	}
	public static void Insert(bool dbconOpened, EncoderConfigurationSQLObject econfSO)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "INSERT INTO " + Constants.EncoderConfigurationTable +
			" (uniqueID, encoderGI, active, name, encoderConfiguration, description, future1, future2, future3)" +
			" VALUES (" + econfSO.ToSQLInsert() + ")";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);
	}

	/*
	 * IfNameExistsAddSuffix starts --------------------->
	 */
	/*
	 * this method check if a name exists.
	 * if exists, add a suffix, like _copy
	 * but if the string already ends with _copy add a number: _copy2
	 * but if the number already exists, like _copy21, convert into _copy22
	 * always check that the new string exists.
	 *
	 * The main reason of this method is not to have a:
	 * unnamed_copy_copy_copy_copy (that's very ugly and breaks the interface), and have instead:
	 * unnamed_copy4
	 */
	public static string IfNameExistsAddSuffix(string name, string suffix)
	{
		Sqlite.Open();
		if(Sqlite.Exists(true, Constants.EncoderConfigurationTable, name))
		{
			do {
				name = ifNameExistsAddSuffixDo(name, suffix);
			} while (Sqlite.Exists(true, Constants.EncoderConfigurationTable, name));
		}
		Sqlite.Close();
		return name;
	}
	private static string ifNameExistsAddSuffixDo(string str, string suffix)
	{
		//suffixStarts will point to the start of suffix (the last suffix if there's > 1)
		int suffixStarts = str.LastIndexOf(suffix);

		// 1) if there's no suffix on str: add it
		if(suffixStarts == -1)
			return str + suffix;

		// 2) check if there's a number at the end of suffix
		int numberShouldStart = suffixStarts + suffix.Length;
		string strBeforeNum = str.Substring(0, numberShouldStart);
		string strNum = str.Substring(numberShouldStart);

		//Console.WriteLine("suffixStarts: " + suffixStarts.ToString() + "; numberShouldStart: " + numberShouldStart + "; strNum: " + strNum);

		// 2.a) there's nothing after the suffix, write a "2"
		if(strNum.Length == 0)
			return str + "2";

		// 2.b) after the last suffix there's something but is not a whole number, add suffix again
		// eg: unnamed_copyk will be unnamed_copyk2
		// but unnamed_copyk2 will be unnamed_copyk22 ...
		if(! Util.IsNumber(strNum, false))
			return str + "2";

		// 2.c) after the suffix, there's a whole number, add +1 to this number
		return strBeforeNum + (Convert.ToInt32(strNum) +1);
	}
	public static void IfNameExistsAddSuffixDoTests()
	{
		string suffix = "_copy";
		string [] tests = {
			"_copy2", "_copy75", "_copy",
			"unnamed_copy2", "unnamed_copy75", "unnamed_copy",
			"lalala_copy", "_copy2_copy2", "hello_good_morning_copy",
			"how are you 21", "_copy2k", "_copy2k2" };

		foreach (string test in tests)
			LogB.Information(test + " -> " + ifNameExistsAddSuffixDo(test, suffix));
	}

	/*
	 * <-------------------------- IfNameExistsAddSuffix ends
	 */


	//called on capture, recalculate, load
	public static void UpdateActive(bool dbconOpened, Constants.EncoderGI encoderGI, EncoderConfiguration econf)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "UPDATE " + Constants.EncoderConfigurationTable +
			" SET encoderConfiguration = \"" + econf.ToStringOutput(EncoderConfiguration.Outputs.SQL) + "\"" +
			" WHERE encoderGI = \"" + encoderGI.ToString() + "\"" +
			" AND active = \"True\"";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);
	}

	//called on gui/encoderConfiguration.cs when click on save
	//also on load set
	public static void Update(bool dbconOpened, Constants.EncoderGI encoderGI, string oldName, EncoderConfigurationSQLObject econfSO)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "UPDATE " + Constants.EncoderConfigurationTable +
			" SET active = \"" + econfSO.active + "\", name = \"" + econfSO.name + "\", " +
			" encoderConfiguration = \"" + econfSO.encoderConfiguration.ToStringOutput(EncoderConfiguration.Outputs.SQL) + "\", " +
			" description = \"" + econfSO.description + "\"" +
			" WHERE name = \"" + oldName + "\" AND encoderGI = \"" + encoderGI.ToString() + "\"";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);
	}

	//note deleting by name should be only on one encoderGI, because same name could have 2 encoderGIs
	public static void Delete (bool dbconOpened, Constants.EncoderGI encoderGI, string name)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "DELETE FROM " + Constants.EncoderConfigurationTable +
			" WHERE encoderGI = \'" + encoderGI.ToString() + "\' " +
			" AND name = \'" + name + "\'";

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);
	}


	public static void MarkAllAsUnactive(bool dbconOpened, Constants.EncoderGI encoderGI)
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "UPDATE " + Constants.EncoderConfigurationTable +
			" SET active = \"False\"" +
			" WHERE encoderGI = \"" + encoderGI.ToString() + "\"";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		closeIfNeeded(dbconOpened);
	}

	//pass customName = "" to select all
	public static List<EncoderConfigurationSQLObject> Select(bool dbconOpened, Constants.EncoderGI encoderGI, string name)
	{
		openIfNeeded(dbconOpened);

		string nameStr = "";
		if(name != "")
			nameStr = " AND name = \"" + name + "\"";

		dbcmd.CommandText = "SELECT * FROM " + Constants.EncoderConfigurationTable +
			" WHERE encoderGI = \"" + encoderGI.ToString() + "\"" + nameStr;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		List<EncoderConfigurationSQLObject> list = new List<EncoderConfigurationSQLObject>();
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		while(reader.Read())
		{
			string [] strFull = reader[4].ToString().Split(new char[] {':'});
			EncoderConfiguration econf = new EncoderConfiguration(
					(Constants.EncoderConfigurationNames)
					Enum.Parse(typeof(Constants.EncoderConfigurationNames), strFull[0]) );
			econf.ReadParamsFromSQL(strFull);
			
			EncoderConfigurationSQLObject econfSO = new EncoderConfigurationSQLObject(
					Convert.ToInt32(reader[0].ToString()),	//uniqueID
					encoderGI,				//encoderGI
					reader[2].ToString() == "True",		//active
					reader[3].ToString(),			//name
					econf,					//encoderConfiguration
					reader[5].ToString()			//description
					);

			list.Add(econfSO);
		}

		reader.Close();
		closeIfNeeded(dbconOpened);

		return list;
	}

	//if by any bug there's no active, then create default
	public static EncoderConfigurationSQLObject SelectActive (Constants.EncoderGI encoderGI) 
	{
		Sqlite.Open();

		dbcmd.CommandText = "SELECT * FROM " + Constants.EncoderConfigurationTable +
			" WHERE encoderGI = \"" + encoderGI.ToString() + "\" AND active = \"True\"";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		EncoderConfigurationSQLObject econfSO = new EncoderConfigurationSQLObject();
	
		bool success = false;
		if(reader.Read())
		{
			string [] strFull = reader[4].ToString().Split(new char[] {':'});
			EncoderConfiguration econf = new EncoderConfiguration(
					(Constants.EncoderConfigurationNames)
					Enum.Parse(typeof(Constants.EncoderConfigurationNames), strFull[0]) );
			econf.ReadParamsFromSQL(strFull);
					
			econfSO = new EncoderConfigurationSQLObject(
					Convert.ToInt32(reader[0].ToString()),	//uniqueID
					encoderGI,				//encoderGI
					true,					//active
					reader[3].ToString(),			//name
					econf,					//encoderConfiguration
					reader[5].ToString()			//description
					);
			success = true;
		}
		reader.Close();

		//if by any bug there's no active, then create default and call himself again to select
		if(! success)
		{
			insertDefault(encoderGI);
			Sqlite.Close();
			return SelectActive (encoderGI);
		}

		Sqlite.Close();

		return econfSO;
	}

	//called on load signal
	//if does not found any encoderConfiguration then return one with -1 as uniqueID
	public static EncoderConfigurationSQLObject SelectByEconf (bool dbconOpened, Constants.EncoderGI encoderGI, EncoderConfiguration econf) 
	{
		openIfNeeded(dbconOpened);

		dbcmd.CommandText = "SELECT * FROM " + Constants.EncoderConfigurationTable + 
			" WHERE encoderGI = \"" + encoderGI.ToString() + "\"" +
		        " AND encoderConfiguration LIKE \"" + econf.ToStringOutput(EncoderConfiguration.Outputs.SQLECWINCOMPARE) + "\"";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		EncoderConfigurationSQLObject econfSO = new EncoderConfigurationSQLObject();

		if(reader.Read())
		{
			econfSO = new EncoderConfigurationSQLObject(
					Convert.ToInt32(reader[0].ToString()),	//uniqueID
					encoderGI,				//encoderGI
					true,					//active
					reader[3].ToString(),			//name
					econf,					//encoderConfiguration
					reader[5].ToString()			//description
					);
		}
		reader.Close();
		closeIfNeeded(dbconOpened);

		return econfSO;
	}

	public static EncoderConfigurationSQLObject SelectByEncoderGIAndName (bool dbconOpened, Constants.EncoderGI encoderGI, string name)
	{
		openIfNeeded(dbconOpened);

		LogB.Information("SelectByEncoderGIAndName");
		dbcmd.CommandText = "SELECT * FROM " + Constants.EncoderConfigurationTable +
			" WHERE encoderGI = \"" + encoderGI.ToString() + "\"" +
		        " AND name = \'" + name + "\'";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		EncoderConfigurationSQLObject econfSO = new EncoderConfigurationSQLObject();

		if(reader.Read())
		{
			string [] strFull = reader[4].ToString().Split(new char[] {':'});
			EncoderConfiguration econf = new EncoderConfiguration(
					(Constants.EncoderConfigurationNames)
					Enum.Parse(typeof(Constants.EncoderConfigurationNames), strFull[0]) );
			econf.ReadParamsFromSQL(strFull);

			econfSO = new EncoderConfigurationSQLObject(
					Convert.ToInt32(reader[0].ToString()),	//uniqueID
					encoderGI,				//encoderGI
					true,					//active
					reader[3].ToString(),			//name
					econf,					//encoderConfiguration
					reader[5].ToString()			//description
					);
			LogB.Information("found: " + econfSO.ToString());
		}
		reader.Close();
		closeIfNeeded(dbconOpened);

		return econfSO;
	}
}
