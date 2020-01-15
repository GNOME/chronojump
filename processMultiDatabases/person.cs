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
 *  Copyright (C) 2019   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using System.Text.RegularExpressions; //Regex

public class Person {

	private int uniqueID;
	private string name;
	private string sex; // "M" (male) , "F" (female) (Constants.M, Constants.F)
	private DateTime dateBorn;
	//private int race;
	//private int countryID;
	private string description;
	//private string future1; 	//rfid
	//private string future2; 	//club ID, is an integer
	//private int serverUniqueID; //not on server

	/*
	public Person() {
	}
	*/
	
	//used when we create a new person, then uniqueID is -1
	public Person(int uniqueID) {
		this.uniqueID = uniqueID;
	}

	//suitable when we load a person from the database for being the current Person
	//we know uniqueID
	//used also in class PersonSessionTransaction where we define the uniqueID 
	public Person(int uniqueID, string name, string sex, DateTime dateBorn, 
			int race, int countryID, string description, string future1, string future2, int serverUniqueID)
	{
		//needed by the return of gui/personAddModifyWindow
		name = Util.RemoveTildeAndColon(name);
		description = Util.RemoveTildeAndColon(description);

		this.uniqueID = uniqueID;
		this.sex = sex;
		this.name = name;
		this.dateBorn = dateBorn;
		//this.race = race;
		//this.countryID = countryID;
		this.description = description;
		//this.future1 = future1;
		//this.future2 = future2;
		//this.serverUniqueID = serverUniqueID; //remember don't do this on server
	}

	public override string ToString()
	{
		return "[uniqueID: " + uniqueID + "]" + name + ", " + ", " + sex + ", " + dateBorn.ToShortDateString() + ", " + description;
	}
	
	public string FindPersonCode (string city)
	{
		if(city == "denmark")
		{
			Match match = Regex.Match(name, @"^(\d+)");
			if(match.Groups.Count == 2)
				return match.Value;
		}

		return "";
	}
	
	//some "set"s are needed. If not data of personSession does not arrive to the server
	
	public string Name {
		get { return name; }
		set { name = value; }
	}
	
	public string Sex {
		get { return sex; } 
		set { sex = value; }
	}

	/*	
	public DateTime DateBorn {
		get { return dateBorn; }
		set { dateBorn = value; }
	}
	
	public int Race {
		get { return race; }
		set { race = value; }
	}

	public int CountryID {
		get { return countryID; }
		set { countryID = value; }
	}

	public string Description {
		get { return description; }
		set { description = value; }
	}

	//rfid
	public string Future1 {
		get { return future1; }
		set { future1 = value; }
	}

	//clubID
	public string Future2 {
		get { return future2; }
		set { future2 = value; }
	}

	public int ServerUniqueID {
		get { return serverUniqueID; }
		set { serverUniqueID = value; }
	}

	public int UniqueID {
		get { return uniqueID; }
		set { uniqueID = value; }
	}

	public string DateLong {
		get { return dateBorn.ToLongDateString(); }
	}
	
	public string DateShort {
		get { return dateBorn.ToShortDateString(); }
	}
	*/
	
	
	~Person() {}
	   
}
