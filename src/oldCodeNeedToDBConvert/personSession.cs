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

public class PersonSessionOld {

	private int uniqueID;
	private int personID;
	private int sessionID;
	private double weight;

	
	public PersonSessionOld() {
	}

	public PersonSessionOld(int uniqueID, int personID, int sessionID, double weight)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.sessionID = sessionID;
		this.weight = weight;
	}
	
	//typical constructor
	public PersonSessionOld(int personID, int sessionID, double weight)
	{
		this.personID = personID;
		this.sessionID = sessionID;
		this.weight = weight;
		

		//insert in the personSession table
		//when insert as personSession we don't know uniqueID
		uniqueID = -1;
		int insertedID = this.InsertAtDB(false, Constants.PersonSessionOldWeightTable);

		//we need uniqueID for personSession
		uniqueID = insertedID;

		LogB.Information(this.ToString());
	}
	
	//used to select a personSession at Sqlite.convertTables
	public PersonSessionOld(string [] myString)
	{
		this.uniqueID = Convert.ToInt32(myString[0]);
		this.personID = Convert.ToInt32(myString[1]);
		this.sessionID = Convert.ToInt32(myString[2]);
		this.weight = Convert.ToDouble(Util.ChangeDecimalSeparator(myString[3]));
	}

	public int InsertAtDB (bool dbconOpened, string tableName) {
		int myID = SqlitePersonSessionOld.Insert(dbconOpened, tableName, 
				uniqueID.ToString(),
				personID, sessionID, weight);
		return myID;
	}
	

	public override string ToString()
	{
		return "";
		//return "[uniqueID: " + uniqueID + "]" + name + ", " + ", " + sex + ", " + dateBorn.ToShortDateString() + ", " + description;
	}
	
	public int UniqueID {
		get { return uniqueID; }
	}

	public int PersonID {
		get { return personID; }
	}

	public int SessionID {
		get { return sessionID; }
	}

	public double Weight {
		get { return weight; }
	}

	~PersonSessionOld() {}
	   
}

