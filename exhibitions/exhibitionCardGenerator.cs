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
using System.Collections.Generic; //Dictionary, List<T>
using Mono.Data.Sqlite;

public static class Options
{
	public static int MaleStartID = 100;
	public static string DbPath = ".";
	public static string Database = "exhibitionCardGenerator.db";
	//public static string Database = "prova.db";
	public static ConsoleColor ColorDefault = ConsoleColor.Blue;
	public static ConsoleColor ColorHigh = ConsoleColor.White;
	public static bool Debug = false;
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
		if(! School.TableExists(dbcmd))
			createTablesPrepare();

		menu();
	}

	private void menu()
	{
		List<School> l_school = new List<School>();
		string option;
		do {
			Console.Clear();
			l_school = schoolList(); //generate school list
			foreach (School school in l_school)
				school.PrettyPrint();
			Console.WriteLine();

			if(l_school.Count > 0)
				printOption("", "codi", " (per seleccionar escola); ");

			printOption("", "a", "fegir escola; ");
			printOption("", "b", "usca escola; ");
			printOption("", "e", "stadístiques; ");
			printOption("esborrar ", "t", "aules (ho esborra tot); ");
			printOption("", "q", "uit; ? ");

			option = Console.ReadLine();
			if(l_school.Count > 0 && isNumber(option))
			{
				School s = getSchoolFromID(l_school, Convert.ToInt32(option));
				if(s.ID != -1)
					submenu(s, false);
			}
			else if(option == "a")
			{
				School s = schoolAdd();
				submenu(s, true);
			}
			/*
			else if(option.StartsWith("b:"))
			{
				string [] strFull = option.Split(new char[] {':'});
				//Console.WriteLine("has escrit: [" + option + "]");
				if(strFull.Length == 2)
				{
					schoolFind(strFull[1]);
					printOption("", "(enter)", " ?");
					option = Console.ReadLine();
				}
			}
			*/
			else if(option == "b")
			{
				Console.Write(" text buscat: ");
				List <School> l = schoolFind(Console.ReadLine());
				if(l.Count > 0)
					printOption("\n", "codi", " seleccionar aquesta escola; ");

				printOption("", "q", "uit al Menu d'escoles; ? ");
				option = Console.ReadLine();

				if(l.Count > 0 && isNumber(option))
				{
					School s = new School(-1, "");
					foreach(School sTemp in l)
						if(sTemp.ID == Convert.ToInt32(option))
							s = sTemp;

					if(s.ID != -1)
						submenu(s, false);
				}

				//need to do this in order to not exit the program
				if(option == "q")
					option = "nothing";
			}
			else if(option == "e")
			{
				new Statistics(dbcmd);
				printOption("", "(enter)", " ?");
				option = Console.ReadLine();
			}
			else if(option == "t")
				createTablesPrepare();
		} while (option != "q");
	}

	private void submenu(School s, bool addingGroup)
	{
		List<Group> l_group = new List<Group>();
		string option;
		do {
			Console.Clear();
			l_group = groupList(s); //show groups
			foreach (Group group in l_group)
				group.PrettyPrint();
			Console.WriteLine();

			if(addingGroup) {
				Console.Write("Afegint grup a la nova escola... ");
				groupAdd(s.ID);
				addingGroup = false;
				option = "g";
			} else {
				if(l_group.Count > 0)
					printOption("", "codi", " (afegir persona en aquest grup); ");

				printOption("afegir ", "g", "rup; ");
				printOption("", "q", "uit al Menu d'escoles; ? ");

				option = Console.ReadLine();
				if(l_group.Count > 0 && isNumber(option) && groupExists(l_group, Convert.ToInt32(option)))
					personAdd(s.ID, Convert.ToInt32(option));
				else if(option == "g")
					groupAdd(s.ID);
			}
		} while (option != "q");
	}

	private School getSchoolFromID (List <School> l_school, int id)
	{
		foreach (School s in l_school)
			if(s.ID == id)
				return s;

		return new School(-1, "");
	}

	private bool isNumber(string str)
	{
		//false if it's blank
		if(str.Length == 0)
			return false;

		int numI;
		if (int.TryParse(str, out numI))
			return true;

		return false;
	}

	private bool groupExists(List <Group> l, int id)
	{
		foreach (Group g in l)
			if(g.ID == id)
				return true;

		return false;
	}

	private void printOption(string textPre, string op, string textPost)
	{
		Console.ForegroundColor = Options.ColorDefault;
		Console.Write(textPre);

		Console.ForegroundColor = Options.ColorHigh;
		Console.Write(op);

		Console.ForegroundColor = Options.ColorDefault;
		Console.Write(textPost);
	}

	private School schoolAdd()
	{
		Console.Write("Escriu el nom de l'escola: ");
		string name = Console.ReadLine();
		School s = new School(-1, name);
		s.Insert(dbcmd);
		return s;
	}
	private List<School> schoolFind(string str)
	{
		return School.Find(dbcmd, str);
	}
	private List<School> schoolList()
	{
		Console.WriteLine("--- Chronojump exhibitions - Menu d'Escoles ---\n"); //fer algo de borrar pantalla, mirar lo de wheelchair
		return School.List(dbcmd);
	}
	
	private void groupAdd(int schoolID)
	{
		Console.Write("Escriu el nom del grup: ");
		string groupName = Console.ReadLine();

		Group g = new Group(-1, schoolID, groupName);
		g.Insert(dbcmd);
	}
	private List<Group> groupList(School s)
	{
		Console.WriteLine(string.Format("--- Grups de l'escola: {0} ---\n", s.Name));
		return Group.List(dbcmd, s.ID);
	}

	private void personAdd(int schoolID, int groupID)
	{
		Person.SexTypes st;
		int numPersons = 0;
		bool allOk;
		do {
			st = Person.SexTypes.UNKNOWN;
			allOk = false;

			printOption("\n", "F", "emale; ");
			printOption("", "M", "ale; ");
			printOption("or eg. ", "5M", " (to insert 5 males) ? ");
			string option = Console.ReadLine();

			if(option.EndsWith("M") || option.EndsWith("m"))
				st = Person.SexTypes.M;
			else if(option.EndsWith("F") || option.EndsWith("f"))
				st = Person.SexTypes.F;

			if(st != Person.SexTypes.UNKNOWN)
			{
				if(option.Length == 1) {
					numPersons = 1;
					allOk = true;
				} else {
					numPersons = parseNumPersonsAndSex(option);
					allOk = (numPersons > 0);
				}
			}
		} while(! allOk);

		if(numPersons == 1)
		{
			Person p = new Person(-1, schoolID, groupID, st);
			p.Insert(dbcmd);
			p.PrintCard();
			Console.WriteLine();
		}
		else if(numPersons > 1)
		{
			int min = 0;
			int max = 0;
			for(int i=0; i < numPersons; i++)
			{
				Person p = new Person(-1, schoolID, groupID, st);
 				p.Insert(dbcmd);

				if(i==0)
					min = p.ID;
				max = p.ID;
			}
			Person.PrintCardMultiple(min, max, groupID, schoolID);
		}

		printOption("", "(enter)", " ?");
		Console.ReadLine();
	}

	private bool isMale(string option)
	{
		return (option == "M" || option == "m");
	}
	private bool isFemale(string option)
	{
		return (option == "F" || option == "f");
	}
	private int parseNumPersonsAndSex(string option)
	{
		string optionWithoutSexLetter = option.Substring(0, option.Length -1);
		Console.WriteLine("parsing [" + optionWithoutSexLetter + "]");
		if(isNumber(optionWithoutSexLetter))
			return Convert.ToInt32(optionWithoutSexLetter);

		return 0; //return this if there are errors
	}

	private void createTablesPrepare()
	{
		Console.WriteLine("\nEstàs segur de que vols esborrar tot i crear les taules des de zero?");
		Console.WriteLine("Per esborrar tot escriu 'Y' i pulsa enter. Qualsevol altra cosa per cancel.lar");
		string option = Console.ReadLine();
		if (option != "Y")
			return;

		createTablesDo();
	}

	private void createTablesDo()
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
	private int id;
	private string name;
	static string table = "school";

	public School(int id, string name)
	{
		this.id = id;
		this.name = name;
	}

	public override string ToString()
	{
		return string.Format("{0}:{1}", id.ToString(), name);
	}
	public void PrettyPrint()
	{
		Console.ForegroundColor = Options.ColorHigh;
		Console.Write(id.ToString());

		Console.ForegroundColor = Options.ColorDefault;
		Console.WriteLine(": " + name);
	}

	public static bool TableExists(SqliteCommand dbcmd)
	{
		dbcmd.CommandText = "SELECT count(*) FROM sqlite_master WHERE type='table' AND name='school'";
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		int count = 0;
		while(reader.Read()) {
			if(reader[0].ToString() != "")
				count = Convert.ToInt32(reader[0].ToString());
		}
		reader.Close();
		return (count == 1);
	}

	public static List<School> Find(SqliteCommand dbcmd, string str)
	{
		dbcmd.CommandText = "SELECT * FROM " + table + " WHERE name LIKE LOWER (\"%" + str + "%\")";
		if(Options.Debug)
			Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		List<School> l_school = new List<School>();
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		while(reader.Read()) {
			School s = new School(
					Convert.ToInt32(reader[0].ToString()),
					reader[1].ToString());
			s.PrettyPrint();
			l_school.Add(s);
		}
		reader.Close();
		return l_school;
	}

	//this helps to start autoincrement at 0 instead of 1
	private int getNextIDLikeThis(SqliteCommand dbcmd)
	{
		dbcmd.CommandText = "SELECT MAX(id) FROM " + table;
		if(Options.Debug)
			Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		int lastID = -1;
		while(reader.Read()) {
			if(reader[0].ToString() != "")
				lastID = Convert.ToInt32(reader[0].ToString());
		}
		reader.Close();
		return lastID + 1;
	}

	public void Insert(SqliteCommand dbcmd)
	{
		int nextID = getNextIDLikeThis(dbcmd);

		dbcmd.CommandText = "INSERT INTO " + table + " (id, name) VALUES (" + nextID + ", \"" +
			name + "\")";
		if(Options.Debug)
			Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		this.id=nextID;
	}

	public static List<School> List(SqliteCommand dbcmd)
	{
		dbcmd.CommandText = "SELECT * FROM " + table;
		dbcmd.ExecuteNonQuery();
		
		List<School> l_school = new List<School>();
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		while(reader.Read()) {
			School s = new School(
					Convert.ToInt32(reader[0].ToString()),
					reader[1].ToString());
			if(Options.Debug)
				Console.WriteLine(s.ToString());
			l_school.Add(s);
		}
		reader.Close();
		return l_school;
	}

	public static void CreateTable(SqliteCommand dbcmd)
	{
		dbcmd.CommandText = "DROP TABLE IF EXISTS " + table;
		dbcmd.ExecuteNonQuery();

		dbcmd.CommandText =
			"CREATE TABLE " + table + " (" +
			"id INTEGER PRIMARY KEY, " +
			"name TEXT)";
		if(Options.Debug)
			Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}

	public int ID {
		get { return id; }
	}
	public string Name {
		get { return name; }
	}
}

public class Group
{
	private int id;
	private int schoolID;
	private string name;
	static string table = "groupClass";

	public Group(int id, int schoolID, string name)
	{
		this.id = id;
		this.schoolID = schoolID;
		this.name = name;
	}

	public override string ToString()
	{
		//return string.Format("{0}:{1}:{2}", id.ToString(), schoolID.ToString(), name);
		return string.Format("{0}:{1}", id.ToString(), name); //do not show schoolID to not confuse the user
	}
	public void PrettyPrint()
	{
		Console.ForegroundColor = Options.ColorHigh;
		Console.Write(id.ToString());

		Console.ForegroundColor = Options.ColorDefault;
		Console.WriteLine(": " + name);
	}

	//this helps to start at 0 instead of 1
	private int getNextIDLikeThis(SqliteCommand dbcmd)
	{
		dbcmd.CommandText = "SELECT MAX(id) FROM " + table + " WHERE schoolID = " + schoolID;
		if(Options.Debug)
			Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		int lastID = -1;
		while(reader.Read()) {
			if(reader[0].ToString() != "")
				lastID = Convert.ToInt32(reader[0].ToString());
		}
		reader.Close();
		return lastID + 1;
	}

	public void Insert(SqliteCommand dbcmd)
	{
		int nextID = getNextIDLikeThis(dbcmd);

		dbcmd.CommandText = "INSERT INTO " + table + " (id, schoolID, name) VALUES (" + nextID + ", " +
			schoolID + ", \"" + name + "\")";
		if(Options.Debug)
			Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}

	public static List<Group> List(SqliteCommand dbcmd, int schoolID)
	{
		dbcmd.CommandText = "SELECT * FROM " + table + " WHERE schoolID = " + schoolID;
		dbcmd.ExecuteNonQuery();
		
		List<Group> l_group = new List<Group>();
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		while(reader.Read()) {
			Group g = new Group(
					Convert.ToInt32(reader[0].ToString()),
					Convert.ToInt32(reader[1].ToString()),
					reader[2].ToString());
			if(Options.Debug)
				Console.WriteLine(g.ToString());
			l_group.Add(g);
		}
		reader.Close();
		return l_group;
	}

	public static void CreateTable(SqliteCommand dbcmd)
	{
		dbcmd.CommandText = "DROP TABLE IF EXISTS " + table;
		dbcmd.ExecuteNonQuery();

		dbcmd.CommandText =
			"CREATE TABLE " + table + " (" +
			"id INT NOT NULL, " +
			"schoolID INT, " +
			"name TEXT)";
		if(Options.Debug)
			Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}

	public int ID {
		get { return id; }
	}
}

public class Person
{
	private int id;
	private int schoolID;
	private int groupID;
	private SexTypes sex;
	//string name;
	static string table = "person";

	public enum SexTypes { F, M, UNKNOWN };

	public Person(int id, int schoolID, int groupID, SexTypes sex)
	{
		this.id = id;
		this.schoolID = schoolID;
		this.groupID = groupID;
		this.sex = sex;
	}

	private int getNextIDLikeThis(SqliteCommand dbcmd)
	{
		dbcmd.CommandText = "SELECT MAX(id) FROM " + table + " WHERE schoolID = " + schoolID +
			" AND groupID = " + groupID + " AND sex = \"" + sex.ToString() + "\"";
		if(Options.Debug)
			Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		int lastID = -1;
		while(reader.Read()) {
			if(reader[0].ToString() != "")
				lastID = Convert.ToInt32(reader[0].ToString());
		}
		reader.Close();

		//females start at 0, males start at Options.MaleStartID
		if(lastID == -1 && sex == SexTypes.M)
			return Options.MaleStartID;
		else
			return lastID + 1;
	}

	public void Insert(SqliteCommand dbcmd)
	{
		int nextID = getNextIDLikeThis(dbcmd);

		dbcmd.CommandText = "INSERT INTO " + table + " (id, schoolID, groupID, sex) VALUES (" + nextID + ", " +
			schoolID + ", " + groupID + ", \"" + sex.ToString() + "\")";
		if(Options.Debug)
			Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		this.id = nextID;
	}

	public static void CreateTable(SqliteCommand dbcmd)
	{
		dbcmd.CommandText = "DROP TABLE IF EXISTS " + table;
		dbcmd.ExecuteNonQuery();

		dbcmd.CommandText =
			"CREATE TABLE " + table + " (" +
			"id INT NOT NULL, " +
			"schoolID INT, " +
			"groupID INT, " +
			"sex TEXT)";
		if(Options.Debug)
			Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}

	public void PrintCard()
	{
		Console.WriteLine("\n-- Targeta --");
		Console.ForegroundColor = Options.ColorDefault;
		Console.Write("Codi: ");
		Console.ForegroundColor = Options.ColorHigh;
		Console.WriteLine(id);

		Console.ForegroundColor = Options.ColorDefault;
		Console.Write("Grup: ");
		Console.ForegroundColor = Options.ColorHigh;
		Console.WriteLine(groupID);

		Console.ForegroundColor = Options.ColorDefault;
		Console.Write("Escola: ");
		Console.ForegroundColor = Options.ColorHigh;
		Console.WriteLine(schoolID);
		Console.ForegroundColor = Options.ColorDefault;
	}

	public static void PrintCardMultiple(int idMin, int idMax, int groupID, int schoolID)
	{
		Console.WriteLine("\n-- Targetes --");
		Console.ForegroundColor = Options.ColorDefault;
		Console.Write("Codi: ");
		Console.ForegroundColor = Options.ColorHigh;
		Console.WriteLine(string.Format("{0}-{1}", idMin, idMax));

		Console.ForegroundColor = Options.ColorDefault;
		Console.Write("Grup: ");
		Console.ForegroundColor = Options.ColorHigh;
		Console.WriteLine(groupID);

		Console.ForegroundColor = Options.ColorDefault;
		Console.Write("Escola: ");
		Console.ForegroundColor = Options.ColorHigh;
		Console.WriteLine(schoolID);
		Console.ForegroundColor = Options.ColorDefault;
	}

	public int ID {
		get { return id; }
	}
}

public class Statistics
{
	private SqliteCommand dbcmd;

	public Statistics(SqliteCommand dbcmd)
	{
		this.dbcmd = dbcmd;

		Console.WriteLine("\n--- Estadístiques ---");
		Console.WriteLine("\nTotal de alumnes: " + totalPersons().ToString());
		Console.WriteLine("\nTotal de centre: " + totalSchools().ToString());
		Console.WriteLine("\nRanking de centres (per nombre de inscrits): ");
		schoolsRanking();
	}

	private int totalPersons()
	{
		dbcmd.CommandText = "SELECT count(*) FROM person";
		if(Options.Debug)
			Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		int count = 0;
		while(reader.Read()) {
			if(reader[0].ToString() != "")
				count = Convert.ToInt32(reader[0].ToString());
		}
		reader.Close();
		return count;
	}

	private int totalSchools()
	{
		dbcmd.CommandText = "SELECT count(*) FROM school";
		if(Options.Debug)
			Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		int count = 0;
		while(reader.Read()) {
			if(reader[0].ToString() != "")
				count = Convert.ToInt32(reader[0].ToString());
		}
		reader.Close();
		return count;
	}

	private void schoolsRanking()
	{
		dbcmd.CommandText = "SELECT school.name, count(*) AS conta FROM person, school WHERE person.schoolID = school.id GROUP BY person.schoolID ORDER BY conta DESC LIMIT 10";
		if(Options.Debug)
			Console.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		//Console.WriteLine("Escola: Participants");
		while(reader.Read()) {
			if(reader[0].ToString() != "")
				Console.WriteLine(string.Format("- {0}: {1}", reader[0], reader[1]));
		}
		reader.Close();
	}
}

