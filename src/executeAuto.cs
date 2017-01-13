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
 * Copyright (C) 2014-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Collections; //ArrayList
using System.Collections.Generic; //List<T>
using Mono.Unix;

public class ExecuteAuto {
	public enum ModeTypes { BY_PERSONS, BY_TESTS, BY_SETS }
	
	public int serieID;	//only in BY_SETS (in BY_PERSONS or BY_TESTS, value is -1)
	public int personUniqueID;
	public string personName;
	public int testUniqueID;
	public string testEName;	//english name
	public string testTrName;	//translated name
	
	public ExecuteAuto(int serieID, int personUniqueID, string personName, 
			int testUniqueID, string testEName, string testTrName) {
		this.serieID = serieID;
		this.personUniqueID = personUniqueID;
		this.personName = personName;
		this.testUniqueID = testUniqueID;
		this.testEName = testEName;
		this.testTrName = testTrName;
	}

	public string [] AsStringArray() {
		if(serieID == -1)
			return new string [] { personName, testTrName };
		else
			return new string [] { serieID.ToString(), personName, testTrName };
	}
	
	public static ArrayList CreateOrder(ModeTypes mode, ArrayList persons, 
			ArrayList comboSerie1Array, ArrayList comboSerie2Array, ArrayList comboSerie3Array) 
	{
	        ArrayList orderArray = new ArrayList(persons.Count);

		if(mode == ModeTypes.BY_PERSONS)
			foreach(Person p in persons)
				foreach(TrCombo tc in comboSerie1Array)
					orderArray.Add(new ExecuteAuto(-1, p.UniqueID, p.Name, tc.id, tc.eName, tc.trName));
		else if(mode == ModeTypes.BY_TESTS)
			foreach(TrCombo tc in comboSerie1Array)
				foreach(Person p in persons)
					orderArray.Add(new ExecuteAuto(-1, p.UniqueID, p.Name, tc.id, tc.eName, tc.trName));
		else {
			//by series
			foreach(Person p in persons)
				foreach(TrCombo tc in comboSerie1Array)
					orderArray.Add(new ExecuteAuto(1, p.UniqueID, p.Name, tc.id, tc.eName, tc.trName));

			foreach(Person p in persons)
				foreach(TrCombo tc in comboSerie2Array)
					orderArray.Add(new ExecuteAuto(2, p.UniqueID, p.Name, tc.id, tc.eName, tc.trName));

			foreach(Person p in persons)
				foreach(TrCombo tc in comboSerie3Array)
					orderArray.Add(new ExecuteAuto(3, p.UniqueID, p.Name, tc.id, tc.eName, tc.trName));
		}

		return orderArray;
	}
	
	//position is needed because person will be moved to the end (but only from current pos to the end, not previous)
	public static ArrayList SkipPerson(ArrayList orderArray, int position, Person p)
	{
		ArrayList returnArray = new ArrayList();
		ArrayList skippedArray = new ArrayList();
		int count = 0;
		foreach (ExecuteAuto ea in orderArray) {
			if(count < position || ea.personUniqueID != p.UniqueID)
				returnArray.Add(ea);
			else
				skippedArray.Add(ea);
			count ++;
		}
		
		foreach (ExecuteAuto ea in skippedArray) {
			returnArray.Add(ea);
		}

		return returnArray;
	}

	//position is needed because person will be removed from current pos to the end (not previous)
	public static ArrayList RemovePerson(ArrayList orderArray, int position, Person p)
	{
		ArrayList returnArray = new ArrayList();
		int count = 0;
		foreach (ExecuteAuto ea in orderArray) {
			if(count < position || ea.personUniqueID != p.UniqueID)
				returnArray.Add(ea);
			count ++;
		}
		return returnArray;
	}

	~ExecuteAuto() {}	
}

//sequence saved or loaded on SQL
public class ExecuteAutoSQL
{
	public int uniqueID;
	public string name;
	private ExecuteAuto.ModeTypes mode;
	private string description;
	private List<int> serie1IDs;
	private List<int> serie2IDs;
	private List<int> serie3IDs;
	
	public ExecuteAutoSQL(int uniqueID, string name, ExecuteAuto.ModeTypes mode, string description, 
			List<int> serie1IDs, List<int> serie2IDs, List<int> serie3IDs) 
	{
		this.uniqueID = uniqueID;
		this.name = name;
		this.mode = mode;
		this.description = description;
		this.serie1IDs = serie1IDs;
		this.serie2IDs = serie2IDs;
		this.serie3IDs = serie3IDs;
	}

	public string SerieIDsToStr(List<int> serieIDs) 
	{
		string str = "";
		string sep = "";
		foreach(int i in serieIDs) {
			str += sep + i.ToString();
			sep = ":";
		}
		return str;
	}

	//just used to display name of jumpTypes on executeAuto load treeview	
	private string serieIDsToStrName(List<int> serieIDs, string [] jumpTypes) 
	{
		string str = "";
		string sep = "";
		foreach(int i in serieIDs) {
			foreach(string jumpType in jumpTypes) {
				string [] j = jumpType.Split(new char[] {':'});
				if(i == Convert.ToInt32(j[0]))
					str += sep + Catalog.GetString(j[1]);
			}
			sep = ", ";
		}
		return str;
	}


	public bool SaveToSQL() 
	{
		if(Sqlite.Exists(false, Constants.ExecuteAutoTable, name))
			return false; //not saved because name exists

		SqliteExecuteAuto.Insert(false, this);
			
		return true; //saved
	}
	
	public static List<int> SerieIDsFromStr(string str) 
	{
		if(str == null || str == "")
			return new List<int>();

		LogB.Information("SerieIDsFromStr", str);
		string [] strFull = str.Split(new char[] {':'});
		
		List <int>l = new List <int>();
		foreach(string s in strFull) {
			l.Add(Convert.ToInt32(s));
		}
		return l;
	}

	public string[] ToLoadTreeview(string [] jumpTypes) 
	{
		return new string [] { name, mode.ToString(), description,
				serieIDsToStrName(serie1IDs, jumpTypes), 
				serieIDsToStrName(serie2IDs, jumpTypes), 
				serieIDsToStrName(serie3IDs, jumpTypes),
				uniqueID.ToString()
		};
	}
	
	public ExecuteAuto.ModeTypes Mode {
		get { return mode; }
	}
	
	public string Description {
		get { return description; }
	}

	public List<int> Serie1IDs {
		get { return serie1IDs; }
	}
	public List<int> Serie2IDs {
		get { return serie2IDs; }
	}
	public List<int> Serie3IDs {
		get { return serie3IDs; }
	}

	~ExecuteAutoSQL() {}	
}


/*
 * TrCombo (Translatable Combo)
 * use this with an arraylist instead of strings [], and the Util.FindOnArray
 * see implementation on ExecuteAutoWindow
 */

public class TrCombo {
	public int id; 		//uniqueID
	public string eName;	//englishName
	public string trName;	//translatedName
	public ArrayList options;

	/*
	public TrCombo() {
	}
	*/

	public TrCombo(int id, string eName, string trName, ArrayList options) {
		this.id = id;
		this.eName = eName;
		this.trName = trName;
		this.options = options;
	}

	public override string ToString() {
		return id + ":" + eName + ":" + trName + ":" + Util.ArrayListToSingleString(options, ":"); 
	}

	~TrCombo() {}
}
