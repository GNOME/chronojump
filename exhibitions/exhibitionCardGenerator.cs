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

public static class Options
{
	public static int MaleStartID = 100;
	public static string DbPath = ".";
	public static string Database = "exhibitionCardGenerator.db";
}

public class ExhibitionCardGenerator
{
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
			Console.WriteLine("\n1 add school; 2 list schools; 3 add group to school; 4 list groups; 5 add person; 9 create tables; 0 exit");
			option = Int32.Parse(Console.ReadLine());
			Console.WriteLine("selected: " + option);
			if(option == 1)
				schoolAdd();
			else if(option == 2)
				schoolList();
			else if(option == 3)
				groupAdd();
			else if(option == 4)
				groupList();
			else if(option == 5)
				personAdd();
			else if(option == 9)
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

	private void personAdd()
	{
		Console.Write("Write school id: ");
		int schoolID = Int32.Parse(Console.ReadLine());

		Console.Write("Write group id: ");
		int groupID = Int32.Parse(Console.ReadLine());

		Console.Write("'F' female or 'M' male? ");
		string sex = Console.ReadLine();
		Person.SexTypes st = Person.SexParse(sex);

		Person p = new Person(schoolID, groupID, st);
		p.Insert(dbcmd);
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
	        string sqlFile = Options.DbPath + Path.DirectorySeparatorChar + Options.Database;
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
		dbcmd.CommandText = "DROP TABLE IF EXISTS " + table;
		dbcmd.ExecuteNonQuery();

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
		dbcmd.CommandText = "DROP TABLE IF EXISTS " + table;
		dbcmd.ExecuteNonQuery();

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
	//int id;
	int schoolID;
	int groupID;
	SexTypes sex;
	//string name;
	static string table = "person";

	public enum SexTypes { F, M };

	//public Person(int id, int schoolID, int groupID, SexTypes sex)
	public Person(int schoolID, int groupID, SexTypes sex)
	{
		//this.id = id;
		this.schoolID = schoolID;
		this.groupID = groupID;
		this.sex = sex;
	}

	private int getNextIDLikeThis(SqliteCommand dbcmd)
	{
		dbcmd.CommandText = "SELECT MAX(id) FROM " + table + " WHERE schoolID = " + schoolID +
			" AND groupID = " + groupID + " AND sex = \"" + sex.ToString() + "\"";
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		int lastID = 0;
		while(reader.Read()) {
			if(reader[0].ToString() != "")
				lastID = Convert.ToInt32(reader[0].ToString());
		}
		reader.Close();

		//females start at 0, males start at Options.MaleStartID
		if(lastID == 0)
		{
			if(sex == SexTypes.F)
				return 0;
			else
				return Options.MaleStartID;
		} else
			return lastID + 1;
	}

	public void Insert(SqliteCommand dbcmd)
	{
		int nextID = getNextIDLikeThis(dbcmd);

		dbcmd.CommandText = "INSERT INTO " + table + " (id, schoolID, groupID, sex) VALUES (" + nextID + ", " +
			schoolID + ", " + groupID + ", \"" + sex.ToString() + "\")";
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}

	public static void CreateTable(SqliteCommand dbcmd)
	{
		dbcmd.CommandText = "DROP TABLE IF EXISTS " + table;
		dbcmd.ExecuteNonQuery();

		dbcmd.CommandText =
			"CREATE TABLE " + table + " (" +
			"id INT, " +
			"schoolID INT, " +
			"groupID INT, " +
			"sex TEXT, " +
			"personID INT)";
		Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}

	public static SexTypes SexParse(string sex)
	{
		if(sex == "M" || sex == "m")
			return SexTypes.M;
		else
			return SexTypes.F;
	}
}
