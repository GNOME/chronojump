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
using Mono.Data.SqliteClient;
using Gtk;
using Glade;
using Gnome;


public class Session {

	int uniqueID;
	string name;
	string place;
	string date;
	string comments;
	

	//suitable when we load a session from the database for being the current session
	public Session(string newUniqueID, string newName, string newPlace, string newDate, string newComments) 
	{
		uniqueID = Convert.ToInt32(newUniqueID);
		name = newName;
		place = newPlace;
		date = newDate;
		comments = newComments;
	}

	//typical constructor
	public Session(string newName, string newPlace, string newDate, string newComments) 
	{
		name = newName;
		place = newPlace;
		date = newDate;
		comments = newComments;

		name = removeTildeAndColon(name);
		place = removeTildeAndColon(place);
		comments = removeTildeAndColon(comments);

		
		uniqueID = Sqlite.SessionInsert (name, place, date, comments);

		Console.WriteLine(this.ToString());
	}
	
	private string removeTildeAndColon(string myString) 
	{
		StringBuilder myStringBuilder = new StringBuilder(myString);
		myStringBuilder.Replace("'", "");
		myStringBuilder.Replace(":", "");
		return myStringBuilder.ToString();
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
	
	public string Name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}
	
	public string Place
	{
		get
		{
			return place;
		}
		set
		{
			place = value;
		}
	}
	
	public string Date
	{
		get
		{
			return date;
		}
		set
		{
			date = value;
		}
	}
	
	public string Comments
	{
		get
		{
			return comments;
		}
		set 
		{
			comments = value;
		}
	}
	
	public int UniqueID
	{
		get
		{
			return uniqueID;
		}
		set 
		{
			uniqueID = value;
		}
	}
	
	~Session() {}
	   
}

