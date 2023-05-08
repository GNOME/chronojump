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
 *  Copyright (C) 2004-2022   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Data;
using System.Text; //StringBuilder
using System.Collections; //ArrayList
using System.Collections.Generic; //List
using Mono.Unix;


public class Person
{
	private int uniqueID;
	private string name;
	private string sex; // "-" (Unspecified), "M" (male) , "F" (female) (Constants.SexU, Constants.SexM, Constants.SexF)
	private DateTime dateBorn;
	private int race;
	private int countryID;
	private string description;
	private string future1; 	//rfid
	private string future2; 	//club ID, is an integer
	private int serverUniqueID; //not on server
	private string linkServerImage;

	public Person() {
	}
	
	//used when we create a new person, then uniqueID is -1
	public Person(int uniqueID) {
		this.uniqueID = uniqueID;
	}

	//coming from compujump server
	public Person(bool insertPerson, int uniqueID, string name, string rfid, string image) //TODO:, string clubID)
	{
		this.uniqueID = uniqueID;
		this.name = name;
		this.sex = Constants.SexU;
		this.dateBorn = DateTime.Now;
		this.race = Constants.RaceUndefinedID;
		this.countryID = Constants.CountryUndefinedID; //1
		this.description = "";
		this.future1 = rfid;
		//TODO: this.future2 = clubID;
		this.serverUniqueID = Constants.ServerUndefinedID;
		this.linkServerImage = image;

		/*
		 * Before insertion check that uniqueID exists locally
		 * can happen when there are rfid changes on server
		 */
		if(insertPerson)
			SqlitePerson.Insert(false,
					uniqueID.ToString(), name, sex, dateBorn, race, countryID,
					description, future1, future2, serverUniqueID, linkServerImage);
	}

	//suitable when we load a person from the database for being the current Person
	//we know uniqueID
	//used also in class PersonSessionTransaction where we define the uniqueID 
	public Person(int uniqueID, string name, string sex, DateTime dateBorn, 
			int race, int countryID, string description,
			string future1, string future2, int serverUniqueID, string linkServerImage)
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
		this.future1 = future1;
		this.future2 = future2;
		this.serverUniqueID = serverUniqueID; //remember don't do this on server
		this.linkServerImage = linkServerImage;
	}

	//typical constructor
	//used when we create new person 
	//we don't know uniqueID
	public Person(string name, string sex, DateTime dateBorn, int race, int countryID, string description,
			string future1, string future2, int serverUniqueID, string linkServerImage, bool dbconOpened)
	{
		name = Util.RemoveTildeAndColon(name);
		description = Util.RemoveTildeAndColon(description);
		
		this.name = name;
		this.sex = sex;
		this.dateBorn = dateBorn;
		this.race = race;
		this.countryID = countryID;
		this.description = description;
		this.future1 = future1;
		this.future2 = future2;
		this.serverUniqueID = serverUniqueID; //remember don't do this on server
		this.linkServerImage = linkServerImage;

		//insert in the person table
		//when insert as person we don't know uniqueID
		uniqueID = -1;
		int insertedID = this.InsertAtDB(dbconOpened, Constants.PersonTable);

		//we need uniqueID for personSession
		uniqueID = insertedID;

		LogB.Information(this.ToString());
	}
	
	public int InsertAtDB (bool dbconOpened, string tableName) {
		int myID = SqlitePerson.Insert(dbconOpened,  
				uniqueID.ToString(), name, sex, dateBorn, race, countryID,
				description, future1, future2, serverUniqueID, linkServerImage);
		return myID;
	}
	
	public string IDAndName (string sep) {
		return uniqueID.ToString() + sep + name;
	}
	
	public string [] IDAndName () {
		string [] str = new String [2];
		str[0] = uniqueID.ToString();
		str[1] = name;
		return str;
	}

	public override string ToString()
	{
		return "[uniqueID: " + uniqueID + "]" + name + ", " + ", " + sex + ", " + dateBorn.ToShortDateString() + ", " + description;
	}
	
	public string ToSQLInsertString()
	{
		return uniqueID.ToString() + ", '"  + name + "', '" + sex + "', '" + 
			UtilDate.ToSql(dateBorn) + "', " + race + ", " + countryID + ", '" +
			description + "', '" + future1 + "', '" + future2 + "', " +
			serverUniqueID + ", '" + linkServerImage + "'";
	}
	
	
	public override bool Equals(object evalString)
	{
		return this.ToString() == evalString.ToString();
	}
	
	public override int GetHashCode()
	{
		return this.ToString().GetHashCode();
	}

	//personToMerge will be merged with currentPerson
	public List<ClassVariance.Struct> MergeWithAnotherGetConflicts (Person personToMerge)
	{
		List<ClassVariance> v_l = this.DetailedCompare (
				personToMerge, ClassCompare.Visibility.PUBLICANDPRIVATE);

		List<ClassVariance.Struct> propDiff_l = new List<ClassVariance.Struct> ();
		if (v_l.Count > 0)
		{
			LogB.Information ("Differences found between persons:");
			foreach (ClassVariance v in v_l)
			{
				//LogB.Information (v.ToString()); //debug
				//don't add the uniqueID, Obviously it is different
				if (v.Prop != "uniqueID")
					propDiff_l.Add (v.GetStruct ());
			}
		}

		return propDiff_l;
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

	public string LinkServerImage {
		get { return linkServerImage; }
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

//useful when you just want to know all of the data of a person in this session
public class PersonAndPS {
	public Person p;
	public PersonSession ps;
	
	//default constructor
	public PersonAndPS(Person p, PersonSession ps) {
		this.p = p;
		this.ps = ps;
	}
	
	~PersonAndPS() {}
}
public static class PersonAndPSUtil {
	public static int Find(ArrayList papsArray, int personID) 
	{
		int count = 0;
		foreach(PersonAndPS paps in papsArray) {
			if(paps.p.UniqueID == personID)
				return count;
			count ++;
		}

		return -1;
	}
}
