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
 *  Copyright (C) 2004-2010   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using System.Text; //StringBuilder
using Mono.Unix;

public partial class Person {

	private int uniqueID;
	private string name;
	private string sex; // "M" (male) , "F" (female) (Constants.M, Constants.F)
	private DateTime dateBorn;
	private int race;
	private int countryID;
	private string description;
	private int serverUniqueID; //not on server

	private int sessionID;
	
	public Person() {
	}
	
	//used when we create a new person, then uniqueID is -1
	public Person(int uniqueID) {
		this.uniqueID = uniqueID;
	}


	//suitable when we load a person from the database for being the current Person
	//we know uniqueID
	public Person(int uniqueID, string name, string sex, DateTime dateBorn, 
		       int race, int countryID, string description, int serverUniqueID) 
	{
		//needed by the return of gui/personAddModifyWindow
		name = Util.RemoveTildeAndColon(name);
		description = Util.RemoveTildeAndColon(description);

		this.uniqueID = uniqueID;
		this.sex = sex;
		this.name = name;
		this.dateBorn = dateBorn;
		this.race = race;
		this.countryID = countryID;
		this.description = description;
		this.serverUniqueID = serverUniqueID; //remember don't do this on server
	}
	
	//typical constructor
	//used when we create new person 
	//we don't know uniqueID
	public Person(string name, string sex, DateTime dateBorn, int race, int countryID, string description,
			int serverUniqueID, int sessionID) 
	{
		name = Util.RemoveTildeAndColon(name);
		description = Util.RemoveTildeAndColon(description);
		
		this.name = name;
		this.sex = sex;
		this.dateBorn = dateBorn;
		this.race = race;
		this.countryID = countryID;
		this.description = description;
		this.serverUniqueID = serverUniqueID; //remember don't do this on server
		this.sessionID = sessionID;

		//insert in the person table
		//when insert as person we don't know uniqueID
		uniqueID = -1;
		int insertedID = this.InsertAtDB(false, Constants.PersonTable);

		//we need uniqueID for personSession
		uniqueID = insertedID;

		Log.WriteLine(this.ToString());

		//insert in the personSession table (fast way of knowing who was in each session)
		//SqlitePersonSession.Insert (false, Constants.PersonSessionWeightTable, "-1", uniqueID, sessionID, weight);
	}
	
	public int InsertAtDB (bool dbconOpened, string tableName) {
		int myID = SqlitePerson.Insert(dbconOpened, tableName, 
				uniqueID.ToString(), name, sex, dateBorn, race, countryID,
				description, serverUniqueID);
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
	
	
	~Person() {}
	   
}

