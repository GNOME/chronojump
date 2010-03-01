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
 *  Copyright (C) 2004-2009   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using System.Text; //StringBuilder
using Mono.Unix;

public partial class PersonSession {

	private int uniqueID;
	private int personID;
	private int sessionID;
	private double height;
	private double weight;
	private int sportID;	//1 undefined, 2 none, 3...n other sports (check table sportType)
	private int speciallityID;
	private int practice;	//-1 undefined, sedentary, 1 regular practice, 2 competition, 3 (alto rendimiento)
	private string comments;

	
	public PersonSession() {
	}

	//loading
	//we know uniqueID
	public PersonSession(int uniqueID, int personID, int sessionID,
			double height, double weight, int sportID, 
			int speciallityID, int practice, string comments)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.sessionID = sessionID;
		this.height = height;
		this.weight = weight;
		this.sportID = sportID;
		this.speciallityID = speciallityID;
		this.practice = practice;
		this.comments = comments;
	}

	//creation
	//we know personID but not personSession.UniqueID
	//this adds to database
	public PersonSession(int personID, int sessionID,
			double height, double weight, int sportID, 
			int speciallityID, int practice, string comments)
	{
		this.personID = personID;
		this.sessionID = sessionID;
		this.height = height;
		this.weight = weight;
		this.sportID = sportID;
		this.speciallityID = speciallityID;
		this.practice = practice;
		this.comments = comments;
		

		//insert in the personSession table
		//when insert as personSession we don't know uniqueID
		uniqueID = -1;
		int insertedID = this.InsertAtDB(false, Constants.PersonSessionTable);
		uniqueID = insertedID;

		Log.WriteLine(this.ToString());
	}
	
	public int InsertAtDB (bool dbconOpened, string tableName) {
		int myID = SqlitePersonSession.Insert(dbconOpened, tableName, 
				uniqueID.ToString(),
				personID, sessionID, height, weight
				sportID, speciallityID,
				practice, comments);
		return myID;
	}
	

	public override string ToString()
	{
		return "";
		//return "[uniqueID: " + uniqueID + "]" + name + ", " + ", " + sex + ", " + dateBorn.ToShortDateString() + ", " + description;
	}
	
	~PersonSession() {}
	   
}

