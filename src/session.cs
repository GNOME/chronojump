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
 * Xavier de Blas: 
 * http://www.xdeblas.com, http://www.deporteyciencia.com (parleblas)
 */

using System;
using System.Data;
using System.Text; //StringBuilder
using Mono.Unix;


public class Session {

	int uniqueID;
	string name;
	string place;
	string date;
	string comments;
	
	private int personsSportID;	//1 undefined, 2 none, 3...n other sports (check table sportType). On session, undefined means that there's no default sport because persons have different sports
	private int personsSpeciallityID;
	private int personsPractice;	//-1 undefined, sedentary, 1 regular practice, 2 competition, 3 (alto rendimiento)
	
	//on gui SessionAddEditWindow, when we add a session, we call that class from gui/chronojump.cs with a session with -1 as uniqueID
	public Session() {
		uniqueID = -1;
		name = "";
	}

	//suitable when we load a session from the database for being the current session. 
	//With person sport stuff
	public Session(string newUniqueID, string newName, string newPlace, string newDate, 
			int personsSportID, int personsSpeciallityID, int personsPractice,
			string newComments) 
	{
		uniqueID = Convert.ToInt32(newUniqueID);
		name = newName;
		place = newPlace;
		date = newDate;
		this.personsSportID = personsSportID;
		this.personsSpeciallityID = personsSpeciallityID;
		this.personsPractice = personsPractice;
		comments = newComments;
	}

	//typical constructor with personsSport stuff
	public Session(string newName, string newPlace, string newDate, 
			int personsSportID, int personsSpeciallityID, int personsPractice,
			string newComments) 
	{
		name = newName;
		place = newPlace;
		date = newDate;
		this.personsSportID = personsSportID;
		this.personsSpeciallityID = personsSpeciallityID;
		this.personsPractice = personsPractice;
		comments = newComments;

		name = Util.RemoveTildeAndColon(name);
		place = Util.RemoveTildeAndColon(place);
		comments = Util.RemoveTildeAndColon(comments);

		
		uniqueID = SqliteSession.Insert (false, //dbconOpened,
				Constants.SessionTable, name, place, date, personsSportID, personsSpeciallityID, personsPractice, comments);

		Log.WriteLine(this.ToString());
	}

	
	public override string ToString()
	{
		return "[uniqueID: " + uniqueID + "]" + name + ", " + place + ", " + date + ", " + comments;
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

	public string Date {
		get { return date; }
		set { date = value; }
	}

	public string DateLong {
		get { return Util.DateAsDateTime(date).ToLongDateString(); }
	}
	
	public string DateShort {
		get { return Util.DateAsDateTime(date).ToShortDateString(); }
	}
	
	public string Comments { 
		get { return comments; } 
		set { comments = value; }
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
	
	~Session() {}
	   
}

