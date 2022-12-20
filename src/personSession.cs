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
using System.Collections.Generic; //List
using Mono.Unix;

public class PersonSession
{
	private int uniqueID;
	private int personID;
	private int sessionID;
	private double height;
	private double weight;
	private int sportID;	//1 undefined, 2 none, 3...n other sports (check table sportType)
	private int speciallityID;
	private int practice;	//-1 undefined, sedentary, 1 regular practice, 2 competition, 3 (alto rendimiento)
	private string comments;
	private double trochanterToe;
	private double trochanterFloorOnFlexion;

	
	public PersonSession()
	{
	}

	//loading
	//we know uniqueID
	public PersonSession(int uniqueID,
			int personID, int sessionID,
			double height, double weight, int sportID, 
			int speciallityID, int practice, string comments,
			double trochanterToe, double trochanterFloorOnFlexion)
	{
		comments = Util.RemoveTildeAndColon(comments);

		this.uniqueID = uniqueID;
		this.personID = personID;
		this.sessionID = sessionID;
		this.height = height;
		this.weight = weight;
		this.sportID = sportID;
		this.speciallityID = speciallityID;
		this.practice = practice;
		this.comments = comments;
		this.trochanterToe = trochanterToe;
		this.trochanterFloorOnFlexion = trochanterFloorOnFlexion;
	}

	//creation
	//we know personID but not personSession.UniqueID
	//this adds to personSession77 table in database
	public PersonSession(int personID, int sessionID,
			double height, double weight, int sportID, 
			int speciallityID, int practice, string comments,
			double trochanterToe, double trochanterFloorOnFlexion,
			bool dbconOpened)
	{
		comments = Util.RemoveTildeAndColon(comments);

		this.personID = personID;
		this.sessionID = sessionID;
		this.height = height;
		this.weight = weight;
		this.sportID = sportID;
		this.speciallityID = speciallityID;
		this.practice = practice;
		this.comments = comments;
		this.trochanterToe = trochanterToe;
		this.trochanterFloorOnFlexion = trochanterFloorOnFlexion;
		
		//insert in the personSession table
		//when insert as personSession we don't know uniqueID
		uniqueID = -1;
		int insertedID = this.InsertAtDB(dbconOpened, Constants.PersonSessionTable);
		uniqueID = insertedID;

		LogB.Information(this.ToString());
	}
	
	public int InsertAtDB (bool dbconOpened, string tableName) {
		int myID = SqlitePersonSession.Insert(dbconOpened,  
				uniqueID.ToString(),
				personID, sessionID, height, weight,
				sportID, speciallityID,
				practice, comments,
				trochanterToe, trochanterFloorOnFlexion);
		return myID;
	}
	

	public override string ToString()
	{
		return "[uniqueID: " + uniqueID + "]," + personID + ", " + ", " + sessionID + ", " + height + ", " + weight + ", " + sportID + ", " + speciallityID + ", " + practice + ", " + comments;
	}
	
	public string ToSQLInsertString()
	{
		string uniqueIDStr;
		if(uniqueID == -1)
			uniqueIDStr = "null";
		else
			uniqueIDStr = uniqueID.ToString();

		return uniqueIDStr + ", " + personID + ", " + sessionID + ", " +
			Util.ConvertToPoint(height) + ", " + Util.ConvertToPoint(weight) + ", " +
			sportID + ", " + speciallityID + ", " + practice + ", '" + 
			comments + "', " +
			Util.ConvertToPoint(trochanterToe) + ", " +
			Util.ConvertToPoint(trochanterFloorOnFlexion);
	}

	//personToMerge will be merged with currentPerson
	public List<ClassVariance.Struct> MergeWithAnotherGetConflicts (PersonSession personSessionToMerge)
	{
		List<ClassVariance> v_l = this.DetailedCompare (
				personSessionToMerge, ClassCompare.Visibility.PUBLICANDPRIVATE);

		List<ClassVariance.Struct> propDiff_l = new List<ClassVariance.Struct> ();
		if (v_l.Count > 0)
		{
			LogB.Information ("Differences found between persons sessions:");
			foreach (ClassVariance v in v_l)
			{
				//LogB.Information (v.ToString()); //debug
				//don't personID. Obviously is different
				//but need uniqueID to do the merge
				if (v.Prop != "personID")
					propDiff_l.Add (v.GetStruct ());
			}
		}

		return propDiff_l;
	}


	//some "set"s are needed. If not data of personSession does not arrive to the server

	public int UniqueID {
		get { return uniqueID; }
		set { uniqueID = value; }
	}
	
	public int PersonID {
		get { return personID; }
		set { personID = value; }
	}

	public int SessionID {
		get { return sessionID; }
		set { sessionID = value; }
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

	public string Comments {
		get { return comments; }
		set { comments = value; }
	}

	public double TrochanterToe {
		get { return trochanterToe; }
		set { trochanterToe = value; }
	}

	public double TrochanterFloorOnFlexion {
		get { return trochanterFloorOnFlexion; }
		set { trochanterFloorOnFlexion = value; }
	}


	~PersonSession() {}
	   
}
