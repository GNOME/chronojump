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

//compile:
//mcs yomoClientSQLGenerator.cs -r:Mono.Data.Sqlite -r:System.Data

using System;
using System.IO; //"File" things. TextWriter. Path
using Mono.Data.Sqlite;

class YomoClientGenerator
{

	/*
	 * atencio pq la sessio 0 tindria que ser la SIMULATED
	 * o una altra opcio es que es borressin les sessions: a part de tota la resta
	 */

	// start of configuration variables ---->

	//private static string dbPath = "~/.local/share/Chronojump/database"; //aixi no va
	private static string username = "cj";
	private static string dbPath = "/home/" + username + "/.local/share/Chronojump/database";
	private static string database = "chronojump.db";
	private static bool debug = false;
	private static bool createTables = false;
	private static bool shouldDestroyOldData = true;
	
	int schools = 300;
	int groupsBySchool = 50;
	int femaleByGroup = 100;
	int maleByGroup = 100;

	// <---- end of configuration variables

        private static SqliteConnection dbcon;
	protected static SqliteCommand dbcmd;


	public static void Main(string[] args)
	{
		sqliteCreateConnection();
		sqliteOpen();

		new YomoClientGenerator();

		sqliteClose();
	}

	// ---- sqlite main methods ----

	private static void sqliteCreateConnection()
	{
		dbcon = new SqliteConnection ();
	        string sqlFile = dbPath + Path.DirectorySeparatorChar + database;
		Console.WriteLine(sqlFile);
		dbcon.ConnectionString = "version = 3; Data source = " + sqlFile;
		dbcmd = dbcon.CreateCommand();
	}
	private static void sqliteOpen()
	{
		dbcon.Open();
	}
	private static void sqliteClose()
	{
		dbcon.Close();
	}
	
	// ---- end of sqlite main methods ----

	// ---- generator ----
	public YomoClientGenerator()
	{
		if(createTables)
			createDatabaseTablesForDebug (); //aixo no caldrà
		else if(shouldDestroyOldData)
			destroyOldData();

		generate();
	}

	// ---- generator helpful methods----

	private void destroyOldData()
	{
		string str = "DELETE FROM jump";
		executeQuery(dbcmd, str);

		str = "DELETE FROM run";
		executeQuery(dbcmd, str);

		str = "DELETE FROM encoder";
		executeQuery(dbcmd, str);

		str = "DELETE FROM session";
		executeQuery(dbcmd, str);

		str = "DELETE FROM personSession77";
		executeQuery(dbcmd, str);

		str = "DELETE FROM person77";
		executeQuery(dbcmd, str);
	}

	//aixo no caldra
	private void createDatabaseTablesForDebug ()
	{
		createSessionTable();
		createPersonTable();
		createPersonSessionTable();
	}

	private void createSessionTable()
	{
		dbcmd.CommandText =
			"CREATE TABLE session ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"name TEXT, " +
			"place TEXT, " +
			"date TEXT, " +  //YYYY-MM-DD since db 0.72
			"personsSportID INT, " +
			"personsSpeciallityID INT, " +
			"personsPractice INT, " + //also called "level"
			"comments TEXT, " +
			"serverUniqueID INT " +
			" ) ";
		dbcmd.ExecuteNonQuery();
	}

 	private void createPersonTable()
	{
		dbcmd.CommandText =
			"CREATE TABLE person77 ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"name TEXT, " +
			"sex TEXT, " +
			"dateborn TEXT, " + //YYYY-MM-DD since db 0.72
			"race INT, " +
			"countryID INT, " +
			"description TEXT, " +
			"future1 TEXT, " + //rfid
			"future2 TEXT, " +
			"serverUniqueID INT ) ";
		dbcmd.ExecuteNonQuery();
	}

	private void createPersonSessionTable()
	{
		dbcmd.CommandText =
			"CREATE TABLE personSession77 ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"personID INT, " +
			"sessionID INT, " +
			"height FLOAT, " +
			"weight FLOAT, " +
			"sportID INT, " +
			"speciallityID INT, " +
			"practice INT, " + //also called "level"
			"comments TEXT, " +
			"future1 TEXT, " +
			"future2 TEXT)";
		dbcmd.ExecuteNonQuery();
	}

	private void generate ()
	{
		int sessionID = 0;
		bool needToCreatePersons = true;

		using(SqliteTransaction tr = dbcon.BeginTransaction())
		{
			using (SqliteCommand dbcmdTr = dbcon.CreateCommand())
			{
				dbcmdTr.Transaction = tr;

				Console.WriteLine("Generating schools ({0}), groups and persons.", schools);
				for(int s = 0; s < schools ; s ++)
				{
					Console.Write("{0} ", s);
					for(int g = 0; g < groupsBySchool ; g ++, sessionID ++)
					{
						insertSession(dbcmdTr, sessionID, string.Format("{0}-{1}", s, g), "", "2019-02-25");
						string sex = "F";
						for(int p = 0; p < femaleByGroup + maleByGroup; p ++)
						{
							if(needToCreatePersons)
							{
								insertPerson(dbcmdTr, p, string.Format("{0:000}", p), sex);

								if(p >= femaleByGroup -1)
									sex = "M";
							}
							insertPersonSession(dbcmdTr, p, sessionID);
						}
						needToCreatePersons = false;
					}
				}
			}
			Console.WriteLine("\nInserting all to SQL. Please wait!");
			tr.Commit();
		}
	}

	private void insertSession (SqliteCommand mycmd, int uniqueID, string name, string place, string date)
	{
		string str = "INSERT INTO session (uniqueID, name, place, date, personsSportID, personsSpeciallityID, personsPractice, comments, serverUniqueID)" +
			" VALUES (" + uniqueID + ", \""	+ name + "\", \"" + place + "\", \"" + date + "\", 1, -1, -1, \"\", -1)";

		executeQuery(mycmd, str);
	}
	private void insertPerson(SqliteCommand mycmd, int uniqueID, string name, string sex)
	{
		string str = "INSERT INTO person77 (uniqueID, name, sex, dateBorn, race, countryID, description, future1, future2, serverUniqueID) VALUES (" +
			uniqueID + ", \"" + name + "\", \"" + sex + "\", \"0001-01-01\", -1, 1, \"\", \"\", \"\", -1)";

		executeQuery(mycmd, str);
	}	
	private void insertPersonSession(SqliteCommand mycmd, int personID, int sessionID)
	{
		string str = "INSERT INTO personSession77 (uniqueID, personID, sessionID, height, weight, " +
			"sportID, speciallityID, practice, comments, future1, future2)" +
			" VALUES (NULL, " + personID + ", " + sessionID + ",0.0,50.0,1,-1,-1,'','','')";

		executeQuery(mycmd, str);
	}

	private void executeQuery(SqliteCommand mycmd, string str)
	{
		mycmd.CommandText = str;
		if (debug)
			Console.WriteLine(mycmd.CommandText.ToString());
		mycmd.ExecuteNonQuery();
	}
}
