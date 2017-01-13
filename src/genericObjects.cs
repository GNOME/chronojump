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
 *  Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */


using System;
using System.Collections; //ArrayList
using System.Collections.Generic; //List<T>

public class IDName
{
	private int uniqueID;
	private string name;

	public IDName(int uniqueID, string name) {
		this.uniqueID = uniqueID;
		this.name = name;
	}

	public override string ToString() {
		return uniqueID.ToString() + ":" + name.ToString();
	}

	public int UniqueID {
	       get { return uniqueID; }
	}	       
	
	public string Name {
	       get { return name; }
	}
}
public class IDNameList
{
	public List<IDName> l; //public in order to do foreach from other methods

	public IDNameList() {
		l = new List<IDName>();
	}
	
	//some string [], divided by ":" have first and second strings: uniqueID and name
	public IDNameList(string [] strArray, char sep) 
	{
		l = new List<IDName>();
		foreach(string strJump in strArray) {
			string [] strFull = strJump.Split(new char[] {sep});
			l.Add(new IDName(Convert.ToInt32(strFull[0]), strFull[1]));
		}
	}

	public void Add(IDName idName) {
		l.Add(idName);
	}
	
	public int Count() {
		return l.Count;
	}
	
	public int FindID(string name) {
		foreach(IDName i in l)
			if(i.Name == name)
				return i.UniqueID;
		return -1;
	}

	public string FindName(int uniqueID) {
		foreach(IDName i in l)
			if(i.UniqueID == uniqueID)
				return i.Name;
		return "";
	}
}


public class IDDouble
{
	private int uniqueID;
	private double num;

	public IDDouble() {
	}

	public IDDouble(int uniqueID, double num) {
		this.uniqueID = uniqueID;
		this.num = num;
	}
	
	public override string ToString() {
		return uniqueID.ToString() + ":" + num.ToString();
	}


	public int UniqueID {
	       get { return uniqueID; }
	}	       
	
	public double Num {
	       get { return num; }
	}
}
//an SQL select can return a list of pairs id and double
public class IDDoubleList
{
	public List<IDDouble> l; //public in order to do foreach from other methods

	public IDDoubleList() {
		l = new List<IDDouble>();
	}
	
	public void Add(IDDouble idDouble) {
		l.Add(idDouble);
	}

	public override string ToString() {
		string str = "";
		foreach(IDDouble i in l)
			str += "\n" + i.ToString();
		
		return str;
	}
	
	//is useful also as an Exists method
	public double FindDouble(int uniqueID) {
		foreach(IDDouble i in l)
			if(i.UniqueID == uniqueID)
				return i.Num;
		return -1;
	}
}

//this is a list of the lists
//it's used when some gets some lists
//and want to match them together, writing '-' when lacks a value
public class IDNameIDDoubleListOfLists
{
	private IDNameList lname;
	private ArrayList ldoublelistoflists;

	public IDNameIDDoubleListOfLists(IDNameList lname, ArrayList ldoublelistoflists) {
		this.lname = lname;
		this.ldoublelistoflists = ldoublelistoflists;
	}

	public string [] GetStringArray() {
		string [] str = new string [lname.Count()];
		int i = 0;
		foreach(IDName iname in lname.l)
			str[i++] = iname.ToString();

		//read every list
		foreach(IDDoubleList ldoublelist in ldoublelistoflists) {
			LogB.Information(ldoublelist.ToString());

			//find if exists a record on this list for the uniqueID on lname
			i = 0;
			foreach(IDName iname in lname.l) {
				double d = ldoublelist.FindDouble(iname.UniqueID);
				if(d == -1)
					str[i++] += ":" + "-";
				else
					str[i++] += ":" + d.ToString();
			}
		}
		return str;
	}
	
	public ArrayList GetArray() {
		ArrayList array = new ArrayList();
		int i = 0;
		foreach(IDName iname in lname.l)
			//array.Add(iname.ToString());
			array.Add(iname.Name);

		//read every list
		foreach(IDDoubleList ldoublelist in ldoublelistoflists) {
			LogB.Information(ldoublelist.ToString());

			//find if exists a record on this list for the uniqueID on lname
			i = 0;
			foreach(IDName iname in lname.l) {
				double d = ldoublelist.FindDouble(iname.UniqueID);
				if(d == -1)
					array[i++] += ":" + "-";
				else
					array[i++] += ":" + d.ToString();
			}
		}
		LogB.Information("printing at GetArray()");
		foreach(string str in array)
			LogB.Information(str);

		return array;
	}
}

