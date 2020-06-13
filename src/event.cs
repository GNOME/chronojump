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
 *  Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using System.Text; //StringBuilder

public class Event 
{
	protected int personID;
	protected string personName;
	protected int sessionID;
	protected int uniqueID;
	protected string type;
	protected string description;
	protected int simulated;

	public Event() {
	}
	
	//used to select an event at Sqlite.addSimulatedInEventTables
	public Event(string [] eventString) {
	}

	/*	
	public virtual void HolaServer (ChronojumpServer myServer) {
	}
	*/

	
	public virtual int InsertAtDB (bool dbconOpened, string tableName) {
		Console.WriteLine("++++++++");
		return -1;
	}	
	
	
	public string Type
	{
		get { return type; }
		set { type = value; }
	}
	
	public string Description
	{
		get { return description; }
		set { description = value; }
	}
	
	public int UniqueID
	{
		get { return uniqueID; }
		set { uniqueID = value; }
	}

	public int SessionID
	{
		get { return sessionID; }
		set { sessionID = value; }
	}

	public int PersonID
	{
		get { return personID; }
		set { personID = value; }
	}
	
	public int Simulated {
		get { return simulated; }
		set { simulated = value; }
	}

	public string PersonName
	{
		//get { return personName; }

		//this is very inneficient if we are processing a list of events, eg. jumps
		get { return SqlitePerson.SelectAttribute(personID, Constants.Name); }
	}
	
	
	~Event() {}
	   
}

