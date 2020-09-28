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
 *  Copyright (C) 2019-2020   Xavier de Blas <xaviblas@gmail.com> 
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
		} else if (city == "belfast")
		{
			//firs num is the centre, second is the personCode
			//03-222-12M
			//03-176 PI
			//03-329
			Match match = Regex.Match(name, @"^\d+-(\d+)");
			if(match.Groups.Count == 2)
				return match.Groups[1].Value;
		}
		else if(city == "ulm")
		{
			//note that just after the A2 can be the code, eg. A404559, so remove the A2 or a3 or A4 if exists
			string nameClean = name;
			nameClean = nameClean.Replace("a2", "");
			nameClean = nameClean.Replace("A2", "");
			nameClean = nameClean.Replace("a3", "");
			nameClean = nameClean.Replace("A3", "");
			nameClean = nameClean.Replace("a4", "");
			//nameClean = nameClean.Replace("A5", "");
			nameClean = nameClean.Replace("A4", "");

			//then return the number
			Match match = Regex.Match(nameClean, @"(\d+)");
			if(match.Groups.Count == 2)
				return match.Value;
		} else if (city == "barcelona")
		{
			//first num is the centre, second is the personCode, third is the moment
			//but need to return both first numbers without the _
			//02_092_4 should be 2092
			//10_317_3 should be 10317
			Match match = Regex.Match(name, @"^(\d+)_(\d+)_\d+");
			if(match.Groups.Count == 3)
			{
				string firstChars = match.Groups[1].Value;
				if(firstChars[0] == 0)
					firstChars = firstChars.Substring(1,1);

				return firstChars + match.Groups[2].Value;
			}

			//043_2 should be 1043
			match = Regex.Match(name, @"^(\d+)_\d+");
			if(match.Groups.Count == 2)
			{
				return "1" + match.Groups[1].Value;
			}

			//062 should be: 1062
			//123 should be: 1123
			if(Util.IsNumber(name, false))
			{
				int num = Convert.ToInt32(name);
				if(num < 1000)
					num += 1000;

				return num.ToString();
			}

			//sometimes the code is: 01035 and should be: 1035
			if(name.Length > 0 && name[0] == '0')
			{
				return name.Substring(1, name.Length -1);
			}

			//many of them come with the personCode correct, like 1025
			return name;
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
