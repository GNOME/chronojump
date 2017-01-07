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
 * Copyright (C) 2017   Xavier de Blas <xaviblas@gmail.com> 
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
						-1 , encoderGI, true, Constants.DefaultString, new EncoderConfiguration(), "") //LINEAR, not inertial
					);
		else if(encoderGI == Constants.EncoderGI.INERTIAL)
		{
			EncoderConfiguration ec = new EncoderConfiguration(Constants.EncoderConfigurationNames.ROTARYAXISINERTIAL);
			ec.SetInertialDefaultOptions();
			Insert(true,
					new EncoderConfigurationSQLObject(
						-1 , encoderGI, true, Constants.DefaultString, ec, "")
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

	public static string IfNameExistsAddSuffix(string name, string suffix)
	{
		if(Sqlite.Exists(false, Constants.EncoderConfigurationTable, name))
		{
			do {
				name += "_" + suffix;
			} while (Sqlite.Exists(false, Constants.EncoderConfigurationTable, name));
		}
		return name;
	}

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
		}
		reader.Close();
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
		        " AND encoderConfiguration = \"" + econf.ToStringOutput(EncoderConfiguration.Outputs.SQL) + "\"";
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
		Sqlite.Close();

		return econfSO;
	}
}
