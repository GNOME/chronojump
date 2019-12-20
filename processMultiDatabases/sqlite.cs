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
 * Copyright (C) 2019   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.IO; //"File" things. TextWriter. Path
using System.Collections.Generic; //List<T>
using Mono.Data.Sqlite;

public class Sqlite
{
	private SqliteConnection dbcon;
	private SqliteCommand dbcmd;

	public Sqlite()
	{
	}

	public void CreateConnection(string currentDBPath)
	{
		dbcon = new SqliteConnection ();
	        string sqlFile = currentDBPath + Path.DirectorySeparatorChar + "chronojump.db";
		Console.WriteLine(sqlFile);
		dbcon.ConnectionString = "version = 3; Data source = " + sqlFile;
		dbcmd = dbcon.CreateCommand();
	}

	public void Open()
	{
		dbcon.Open();
	}

	public void Close()
	{
		dbcon.Close();
	}

	public Person SelectPerson (int uniqueID)
	{
		dbcmd.CommandText = "SELECT * FROM person77 WHERE uniqueID = " + uniqueID;
		Console.WriteLine(dbcmd.CommandText.ToString());

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		Person p = new Person(-1);
		if(reader.Read()) {
			p = new Person(
					Convert.ToInt32(reader[0].ToString()), //uniqueID
					reader[1].ToString(),                   //name
					reader[2].ToString(),                   //sex
					UtilDate.FromSql(reader[3].ToString()),//dateBorn
					Convert.ToInt32(reader[4].ToString()), //race
					Convert.ToInt32(reader[5].ToString()), //countryID
					reader[6].ToString(),                   //description
					reader[7].ToString(),                   //future1: rfid
					reader[8].ToString(),                   //future2: clubID
					Convert.ToInt32(reader[9].ToString()) //serverUniqueID
				      );
		}
		reader.Close();

		return p;
	}

	//is not in above method because it checks personSession77
	public double SelectPersonWeight (int personID)
	{
		dbcmd.CommandText = "SELECT weight FROM personSession77 WHERE personID = " + personID;
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		double myReturn = 0;
                if(reader.Read()) {
                        myReturn = Convert.ToDouble(Util.ChangeDecimalSeparator(reader[0].ToString()));
                }
                reader.Close();

                return myReturn;
	}

	//TODO: will need session to process this by sessions or compare with filenames
	public List<EncoderSQL> SelectEncoder (int exerciseID)
        {
		dbcmd.CommandText = "SELECT * FROM encoder WHERE signalOrCurve = 'signal' AND exerciseID = " + exerciseID;
		Console.WriteLine(dbcmd.CommandText.ToString());
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		List<EncoderSQL> list = new List<EncoderSQL>();
		while(reader.Read())
		{
			EncoderSQL eSQL = new EncoderSQL (
					reader[0].ToString(),                   //uniqueID
					Convert.ToInt32(reader[1].ToString()),  //personID      
					Convert.ToInt32(reader[2].ToString()),  //sessionID
					Convert.ToInt32(reader[3].ToString()),  //exerciseID
					reader[4].ToString(),                   //eccon
					reader[5].ToString(),//laterality
					Util.ChangeDecimalSeparator(reader[6].ToString()),      //extraWeight
					reader[7].ToString(),                   //signalOrCurve
					reader[8].ToString(),                   //filename
					fixOSpath(reader[9].ToString()), //Util.MakeURLabsolute(fixOSpath(reader[9].ToString())),  //url
					Convert.ToInt32(reader[10].ToString()), //time
					Convert.ToInt32(reader[11].ToString()), //minHeight
					reader[12].ToString(),                  //description
					reader[13].ToString(),                  //status
					reader[14].ToString(), //videoURL,                               //videoURL
					reader[15].ToString(), //econf,                                  //encoderConfiguration
					Util.ChangeDecimalSeparator(reader[16].ToString()),     //future1 (meanPower on curves)
					reader[17].ToString(),                  //future2
					reader[18].ToString()//,                  //future3
					//reader[19].ToString()                   //EncoderExercise.name
						);
			list.Add (eSQL);
		}
		reader.Close();
		return list;
	}

	private static string fixOSpath(string url) {
		//if(UtilAll.IsWindows())
		//	return url.Replace("/","\\");
		//else
			return url.Replace("\\","/");
	}
}
