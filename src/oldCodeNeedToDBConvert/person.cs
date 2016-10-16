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
 *  Copyright (C) 2004-2015   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using System.Text; //StringBuilder
using Mono.Unix;

public class PersonOld {

	private int uniqueID;
	private string name;
	private DateTime dateBorn;
	private double height;
	private double weight;
	private int sportID;	//1 undefined, 2 none, 3...n other sports (check table sportType)
	private int speciallityID;
	private int practice;	//-1 undefined, sedentary, 1 regular practice, 2 competition, 3 (alto rendimiento)
	private string sex; // "M" (male) , "F" (female) (Constants.M, Constants.F)
	private string description;
	private int race;
	private int countryID;
	private int serverUniqueID; //not on server

	public PersonOld() {
	}

	//suitable when we load a person from the database for being the current Person
	public PersonOld(int uniqueID, string name, string sex, DateTime dateBorn, 
			double height, double weight, int sportID, int speciallityID, int practice, string description,
		       int race, int countryID, int serverUniqueID	
			) 
	{
		//needed by the return of gui/personAddModifyWindow
		name = Util.RemoveTildeAndColon(name);
		description = Util.RemoveTildeAndColon(description);

		this.uniqueID = uniqueID;
		this.sex = sex;
		this.name = name;
		this.dateBorn = dateBorn;
		this.height = height;
		this.weight = weight;
		this.sportID = sportID;
		this.speciallityID = speciallityID;
		this.practice = practice;
		this.description = description;
		this.race = race;
		this.countryID = countryID;
		this.serverUniqueID = serverUniqueID; //remember don't do this on server
	}
	
	//typical constructor
	public PersonOld(string name, string sex, DateTime dateBorn, 
			double height, double weight, int sportID, int speciallityID, int practice, string description,
		       int race, int countryID, int serverUniqueID,	
			int sessionID) 
	{
		name = Util.RemoveTildeAndColon(name);
		description = Util.RemoveTildeAndColon(description);
		
		this.name = name;
		this.sex = sex;
		this.dateBorn = dateBorn;
		this.height = height;
		this.weight = weight;
		this.sportID = sportID;
		this.speciallityID = speciallityID;
		this.practice = practice;
		this.description = description;
		this.race = race;
		this.countryID = countryID;
		this.serverUniqueID = serverUniqueID; //remember don't do this on server

		//insert in the person table
		//when insert as person we don't know uniqueID
		uniqueID = -1;
		int insertedID = this.InsertAtDB(false, Constants.PersonOldTable);

		//we need uniqueID for personSession
		uniqueID = insertedID;

		LogB.Information(this.ToString());

		//insert in the personSession table (fast way of knowing who was in each session)
		SqlitePersonSessionOld.Insert (false, Constants.PersonSessionOldWeightTable, "-1", uniqueID, sessionID, weight);
	}
	
	//used to select a person at Sqlite.convertTables
	public PersonOld(string [] myString)
	{
		this.uniqueID = Convert.ToInt32(myString[0]);
		this.name = myString[1];
		this.sex = myString[2];
		this.dateBorn = UtilDate.FromSql(myString[3]);
		this.height = Convert.ToDouble(Util.ChangeDecimalSeparator(myString[4]));
		this.weight = Convert.ToDouble(Util.ChangeDecimalSeparator(myString[5]));
		this.sportID = Convert.ToInt32(myString[6]);
		this.speciallityID = Convert.ToInt32(myString[7]);
		this.practice = Convert.ToInt32(myString[8]);
		this.description = myString[9];
		this.race = Convert.ToInt32(myString[10]);
		this.countryID = Convert.ToInt32(myString[11]);
		this.serverUniqueID = Convert.ToInt32(myString[12]); //remember don't do this on server
	}

	public int InsertAtDB (bool dbconOpened, string tableName) {
		int myID = SqlitePersonOld.Insert(dbconOpened, tableName, 
				uniqueID.ToString(), name,
				sex, dateBorn, height, -1, //person weight is '-1', weight is in personSessionWeight table
				sportID, speciallityID, practice,
				description, race, countryID,
				serverUniqueID);
		return myID;
	}
	

	public override string ToString()
	{
		return "[uniqueID: " + uniqueID + "]" + name + ", " + ", " + sex + ", " + dateBorn.ToShortDateString() + ", " + description;
	}
	
	public override bool Equals(object evalString)
	{
		return this.ToString() == evalString.ToString();
	}
	
	public override int GetHashCode()
	{
		return this.ToString().GetHashCode();
	}
	
	
	public string Name {
		get { return name; }
		set { name = value; }
	}
	
	public string Sex {
		get { return sex; } 
		set { sex = value; }
	}
	
	public DateTime DateBorn {
		get { return dateBorn; }
		set { dateBorn = value; }
	}
	
	
	public double Height {
		get { return height; }
		set { height = value; }
	}
	
	public double Weight {
		get { return weight; }
		set { weight = value; }
	}
	
	public int SportID {
		get { return sportID; }
		set { sportID = value; }
	}

	public int SpeciallityID {
		get { return speciallityID; }
		set { speciallityID = value; }
	}

	public int Practice {
		get { return practice; }
		set { practice = value; }
	}
	
	public string Description {
		get { return description; }
		set { description = value; }
	}
	
	public int Race {
		get { return race; }
		set { race = value; }
	}

	public int CountryID {
		get { return countryID; }
		set { countryID = value; }
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
	
	
	~PersonOld() {}
	   
}

