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
//mcs exhibitionCardGenerator.cs -r:Mono.Data.Sqlite -r:System.Data

using System;
using System.IO; //"File" things. TextWriter. Path
using Mono.Data.Sqlite;

public class ExhibitionCardGenerator
{
	private static string dbPath = ".";
	private static string database = "exhibitionCardGenerator.db";
        
	private static SqliteConnection dbcon;
	protected static SqliteCommand dbcmd;
	
	public static void Main(string[] args)
	{
		sqliteCreateConnection();
		sqliteOpen();

		new ExhibitionCardGenerator();

		sqliteClose();
	}
		
	public ExhibitionCardGenerator()
	{
		int option;
		do {
			Console.WriteLine("1 add school; 2 list schools; 3 add group to school; 4 list groups; 9 create tables; 0 exit");
			option = Int32.Parse(Console.ReadLine());
			Console.WriteLine("selected: " + option);
			if(option == 1)
				schoolAdd();
			if(option == 2)
				schoolList();
			if(option == 3)
				groupAdd();
			if(option == 4)
				groupList();
			if(option == 9)
				createTables();
		} while (option != 0);

	}
				
	private void schoolAdd()
	{
		Console.Write("Write school name: ");
		string name = Console.ReadLine();
		School s = new School(-1, name);
		s.Insert(dbcmd);
	}
	private void schoolList()
	{
		School.List(dbcmd);
	}
	
	private void groupAdd()
	{
		Console.Write("Write school id: ");
		int schoolID = Int32.Parse(Console.ReadLine());

		Console.Write("Write new group name: ");
		string groupName = Console.ReadLine();

		Group g = new Group(-1, schoolID, groupName);
		g.Insert(dbcmd);
	}
	private void groupList()
	{
		Console.Write("Write school id: ");
		int schoolID = Int32.Parse(Console.ReadLine());

		Group.List(dbcmd, schoolID);
	}
	private void createTables()
	{
		School.CreateTable(dbcmd);
		Group.CreateTable(dbcmd);
		Person.CreateTable(dbcmd);
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
}

public class School
{
	int id;
	string fullname;
	static string table = "school";

	public School(int id, string fullname)
	{
		this.id = id;
		this.fullname = fullname;
	}

	public override string ToString()
	{
		return string.Format("{0}:{1}", id.ToString(), fullname);
	}

	public void Insert(SqliteCommand dbcmd)
	{
		dbcmd.CommandText = "INSERT INTO " + table + " (id, fullname) VALUES (NULL, \"" +
			fullname + "\")";
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}

	public static void List(SqliteCommand dbcmd)
	{
		dbcmd.CommandText = "SELECT * FROM " + table;
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		while(reader.Read()) {
			School s = new School(
					Convert.ToInt32(reader[0].ToString()),
					reader[1].ToString());
			Console.WriteLine(s.ToString());
		}
		reader.Close();
	}

	public static void CreateTable(SqliteCommand dbcmd)
	{
		dbcmd.CommandText =
			"CREATE TABLE " + table + " (" +
			"id INTEGER PRIMARY KEY, " +
			"fullname TEXT)";
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}
}

public class Group
{
	int id;
	int schoolID;
	string name;
	static string table = "groupClass";

	public Group(int id, int schoolID, string name)
	{
		this.id = id;
		this.schoolID = schoolID;
		this.name = name;
	}

	public override string ToString()
	{
		return string.Format("{0}:{1}:{2}", id.ToString(), schoolID.ToString(), name);
	}

	public void Insert(SqliteCommand dbcmd)
	{
		dbcmd.CommandText = "INSERT INTO " + table + " (id, schoolID, name) VALUES (NULL," +
			schoolID + ", \"" + name + "\")";
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}

	public static void List(SqliteCommand dbcmd, int schoolID)
	{
		dbcmd.CommandText = "SELECT * FROM " + table + " WHERE schoolID = " + schoolID;
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		while(reader.Read()) {
			Group g = new Group(
					Convert.ToInt32(reader[0].ToString()),
					Convert.ToInt32(reader[1].ToString()),
					reader[2].ToString());
			Console.WriteLine(g.ToString());
		}
		reader.Close();
	}

	public static void CreateTable(SqliteCommand dbcmd)
	{
		dbcmd.CommandText =
			"CREATE TABLE " + table + " (" +
			"id INTEGER PRIMARY KEY, " +
			"schoolID INT, " +
			"name TEXT)";
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}
}

public class Person
{
	int id;
	SexTypes sex;
	//string name;
	static string table = "person";

	public enum SexTypes { F, M };

	public Person(int id, SexTypes sex)
	{
		this.id = id;
		this.sex = sex;
	}

	public void Insert(SqliteCommand dbcmd)
	{
		dbcmd.CommandText = "INSERT INTO " + table + " (id, sex) VALUES (NULL, \"" +
			sex.ToString() + "\")";
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}

	public static void CreateTable(SqliteCommand dbcmd)
	{
		dbcmd.CommandText =
			"CREATE TABLE " + table + " (" +
			"id INTEGER PRIMARY KEY, " +
			"sex TEXT)";
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}
}
