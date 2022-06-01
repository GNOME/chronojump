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
using Mono.Unix;

public class Session {

	protected int uniqueID;
	
	protected string name;
	protected string place;
	protected DateTime date;
	protected string comments;
	protected int serverUniqueID; //not on server
	
	protected int personsSportID;	//1 undefined, 2 none, 3...n other sports (check table sportType). On session, undefined means that there's no default sport because persons have different sports
	protected int personsSpeciallityID;
	protected int personsPractice;	//-1 undefined, sedentary, 1 regular practice, 2 competition, 3 (alto rendimiento)


	//on gui SessionAddEditWindow, when we add a session, we call that class from gui/chronojump.cs with a session with -1 as uniqueID
	public Session() {
		uniqueID = -1;
		name = "";
	}

	//suitable when we load a session from the database for being the current session. 
	//With person sport stuff
	public Session(string newUniqueID, string newName, string newPlace, DateTime newDate, 
			int personsSportID, int personsSpeciallityID, int personsPractice,
			string comments, int serverUniqueID) 
	{
		uniqueID = Convert.ToInt32(newUniqueID);
		name = newName;
		place = newPlace;
		date = newDate;
		this.personsSportID = personsSportID;
		this.personsSpeciallityID = personsSpeciallityID;
		this.personsPractice = personsPractice;
		this.comments = comments;
		this.serverUniqueID = serverUniqueID; //remember don't do this on server
	}

	//typical constructor with personsSport stuff
	//this inserts the session in SQL
	public Session(string newName, string newPlace, DateTime newDate, 
			int personsSportID, int personsSpeciallityID, int personsPractice,
			string comments, int serverUniqueID) 
	{
		name = newName;
		place = newPlace;
		date = newDate;
		this.personsSportID = personsSportID;
		this.personsSpeciallityID = personsSpeciallityID;
		this.personsPractice = personsPractice;

		name = Util.RemoveTildeAndColon(name);
		place = Util.RemoveTildeAndColon(place);
		this.comments = Util.RemoveTildeAndColon(comments);
		this.serverUniqueID = serverUniqueID; //remember don't do this on server

		/*
		uniqueID = SqliteSession.Insert (false, //dbconOpened,
				Constants.SessionTable, name, place, date, personsSportID, personsSpeciallityID, personsPractice, comments, serverUniqueID);
		*/
		uniqueID = -1;
		int insertedID = this.InsertAtDB(false, Constants.SessionTable);

		//we need uniqueID for personSession
		uniqueID = insertedID;


		LogB.Information(this.ToString());
	}

	//used to select a session at Sqlite.convertTables
	public Session(string [] myString)
	{
		this.uniqueID = Convert.ToInt32(myString[0]);
		this.name = myString[1];
		this.place = myString[2];
		this.date = UtilDate.FromSql(myString[3]);
		this.personsSportID = Convert.ToInt32(myString[4]);
		this.personsSpeciallityID = Convert.ToInt32(myString[5]);
		this.personsPractice = Convert.ToInt32(myString[6]);
		this.comments = myString[7];
		this.serverUniqueID = Convert.ToInt32(myString[8]);
	}

	public virtual int InsertAtDB (bool dbconOpened, string tableName) {
		int myID = SqliteSession.Insert(dbconOpened, tableName, 
				uniqueID.ToString(), name,
				place, date, 
				personsSportID, 
				personsSpeciallityID, 
				personsPractice, 
				comments,
				serverUniqueID);
		return myID;
	}
	
	public override string ToString()
	{
		return "[uniqueID: " + uniqueID + "]" + name + ", " + place + ", " + date.ToShortDateString() + ", " + comments;
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
	
	public string Place { 
		get { return place; } 
		set { place = value; } 
	}

	public DateTime Date {
		get { return date; }
		set { date = value; }
	}

	public string Comments { 
		get { return comments; } 
		set { comments = value; }
	}
	
	public int ServerUniqueID {
		get { return serverUniqueID; }
		set { serverUniqueID = value; }
	}

	public int UniqueID {
		get { return uniqueID; } 
		set { uniqueID = value; }
	}
	
	public int PersonsSportID
	{
		get { return personsSportID; }
		set { personsSportID = value; }
	}

	public int PersonsSpeciallityID
	{
		get { return personsSpeciallityID; }
		set { personsSpeciallityID = value; }
	}

	public int PersonsPractice
	{
		get { return personsPractice; }
		set { personsPractice = value; }
	}
	
	public string DateLong {
		get { return date.ToLongDateString(); }
	}
	
	//latin: 20/11/2016
	public string DateShort {
		get { return date.ToShortDateString(); }
	}

	//latin: 2016-11-20
	public string DateShortAsSQL {
		get { return date.Year.ToString() + "-" + 
				date.Month.ToString() + "-" + 
				date.Day.ToString(); }
	}
	
	
	~Session() {}
	   
}

public class ServerSession : Session
{
	//server stuff
	int evaluatorID;
	string evaluatorCJVersion;
	string evaluatorOS;
	DateTime uploadedDate;
	int uploadingState;

	public ServerSession() {
	}
	
	public ServerSession(Session mySession, int evaluatorID, string evaluatorCJVersion, 
			string evaluatorOS, DateTime uploadedDate, int uploadingState)
	{
		uniqueID = mySession.UniqueID;
		name = mySession.Name;
		place = mySession.Place;
		date = mySession.Date;
		personsSportID = mySession.PersonsSportID;
		personsSpeciallityID = mySession.PersonsSpeciallityID;
		personsPractice = mySession.PersonsPractice;
		comments = mySession.Comments;
		this.evaluatorID = evaluatorID;
		this.evaluatorCJVersion = evaluatorCJVersion;
		this.evaluatorOS = evaluatorOS;
		this.uploadedDate = uploadedDate;
		this.uploadingState = uploadingState;
	}

	public override int InsertAtDB (bool dbconOpened, string tableName) {
		int myID = SqliteServerSession.Insert(dbconOpened, tableName, 
				//uniqueID.ToString(),
				name,
				place, date, 
				personsSportID, 
				personsSpeciallityID, 
				personsPractice, 
				comments,
				serverUniqueID,
				evaluatorID,
				evaluatorCJVersion,
				evaluatorOS,
				uploadedDate,
				uploadingState
				);
		return myID;
	}
	
	public override string ToString()
	{
		return "[" + uniqueID + "]" + name + ", " + place + ", " + date.ToShortDateString() + ", " + 
			comments + ",(" + serverUniqueID + "), /" + evaluatorID + "/, " + 
			evaluatorCJVersion + ", " + evaluatorOS + ", " + uploadedDate.ToString() + ", " + uploadingState;
	}
	
	public int EvaluatorID {
		get { return evaluatorID; }
		set { evaluatorID = value; }
	}

	public string EvaluatorCJVersion {
		get { return evaluatorCJVersion; }
		set { evaluatorCJVersion = value; }
	}

	public string EvaluatorOS {
		get { return evaluatorOS; }
		set { evaluatorOS = value; }
	}

	public DateTime UploadedDate {
		get { return uploadedDate; }
		set { uploadedDate = value; }
	}

	//public Constants.ServerSessionStates UploadingState {
	public int UploadingState {
		get { return uploadingState; }
		set { uploadingState = value; }
	}


}

/*
   SqliteSessionSwitcher and SqliteSession SelectAllSessions, returns a list of this
   instead of old string []
   */
public class SessionTestsCount
{
	public SessionParams sessionParams;

	public int Persons;
	public int JumpsSimple;
	public int JumpsReactive;
	public int RunsSimple;
	public int RunsInterval;
	public int RunsEncoder;
	public int Isometric;
	public int Elastic;
	public int WeightsSets;
	public int WeightsReps;
	public int InertialSets;
	public int InertialReps;

	public int ReactionTimeOld;
	public int Pulses;
	public int MultiChronopic;

	public SessionTestsCount ()
	{
	}
}
public class SessionParams
{
	public int ID;
	public string Name;
	public string Place;
	public string Date;
	public string SportName;
	public string SpeciallityName;
	public string LevelName;
	public string Description;

	public SessionParams (int ID, string Name, string Place, string Date,
			string SportName, string SpeciallityName,
			string LevelName, string Description)
	{
		this.ID = ID;
		this.Name = Name;
		this.Place = Place;
		this.Date = Date;
		this.SportName = SportName;
		this.SpeciallityName = SpeciallityName;
		this.LevelName = LevelName;
		this.Description = Description;
	}
}
