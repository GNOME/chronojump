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

using Mono.Unix;

public class ServerEvaluator
{
	private int uniqueID;
	private string code;
	private string name;
	private string email;
	private DateTime dateBorn;
	private int countryID;
	private string chronometer;
	private string device;
	private string comments;
	private bool confiable;

	//only initializing
	//needed by the webservice
	public ServerEvaluator() {
	}

	public ServerEvaluator(string code, string name, string email, DateTime dateBorn, int countryID, string chronometer, string device, string comments, bool confiable) {
		this.code = code;
		this.name = name;
		this.email = email;
		this.dateBorn = dateBorn;
		this.countryID = countryID;
		this.chronometer = chronometer;
		this.device = device;
		this.comments = comments;
		this.confiable = confiable;
	}

	//allows to do a copy on gui/evaluator.cs	
	public ServerEvaluator(ServerEvaluator oldEval) {
		this.code = oldEval.Code;
		this.name = oldEval.Name;
		this.email = oldEval.Email;
		this.dateBorn = oldEval.DateBorn;
		this.countryID = oldEval.CountryID;
		this.chronometer = oldEval.Chronometer;
		this.device = oldEval.Device;
		this.comments = oldEval.Comments;
		this.confiable = oldEval.Confiable;
	}

	public bool Equals(ServerEvaluator oldEval) {
		if(
				this.code == oldEval.Code &&
				this.name == oldEval.Name &&
				this.email == oldEval.Email &&
				this.dateBorn == oldEval.DateBorn &&
				this.countryID == oldEval.CountryID &&
				this.chronometer == oldEval.Chronometer &&
				this.device == oldEval.Device &&
				this.comments == oldEval.Comments &&
				this.confiable == oldEval.Confiable
		  )
			return true;
		else
			return false;
	}

	public int InsertAtDB(bool dbconOpened){
		int myID = SqliteServer.InsertEvaluator(dbconOpened, code, name, email, dateBorn, countryID, chronometer, device, comments, confiable);
		return myID;
	}	

	public int Update (bool dbconOpened){
		//confiable will not get updated
		SqliteServer.UpdateEvaluator(dbconOpened, uniqueID, code, name, email, dateBorn, countryID, chronometer, device, comments, confiable);
		return uniqueID;
	}	

	public override string ToString() {
		return "ID: " + uniqueID + "; Name: " + name + 
			"; Email: " + email + "; DateBorn: " + dateBorn.ToShortDateString() +
			"; CountryID: " + countryID + "; Confiable: " + confiable;
	}
	
	public int UniqueID {
		get { return uniqueID; }
		set { uniqueID = value; }
	}

	//the set's are for the server
	//"Private, internal, and protected members do not get serialized.  
	//If the accessor is not specific, it is private by default (and will not get serialized)."
	
	public string Code {
		get { return code; }
		set { code = value; }
	}

	public string Name {
		get { return name; }
		set { name = value; }
	}

	public string Email {
		get { return email; }
		set { email = value; }
	}

	public DateTime DateBorn {
		get { return dateBorn; }
		set { dateBorn = value; }
	}

	public int CountryID {
		get { return countryID; }
		set { countryID = value; }
	}
	
	public string Chronometer {
		get { return chronometer; }
		set { chronometer = value; }
	}

	public string Device {
		get { return device; }
		set { device = value; }
	}

	public string Comments {
		get { return comments; }
		set { comments = value; }
	}

	public bool Confiable {
		get { return confiable; }
		set { confiable = value; }
	}

}
