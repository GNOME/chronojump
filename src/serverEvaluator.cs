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
 */

using System;
using System.Data;
using System.Text; //StringBuilder

using Mono.Unix;

public partial class ServerEvaluator
{
	private int uniqueID;
	private string name;
	private string email;
	private string dateBorn;
	private int countryID;
	private bool confiable;

	//only initializing
	//needed by the webservice
	public ServerEvaluator() {
	}

	public ServerEvaluator(string name, string email, string dateBorn, int countryID, bool confiable) {
		this.name = name;
		this.email = email;
		this.dateBorn = dateBorn;
		this.countryID = countryID;
		this.confiable = confiable;
	}

	public int InsertAtDB(bool dbconOpened){
		Console.WriteLine("here" + this.ToString());
		int myID = SqliteServer.InsertEvaluator(dbconOpened, name, email, dateBorn, countryID, confiable);
		return myID;
	}	

	public override string ToString() {
		return "ID: " + uniqueID + "; Name: " + name + 
			"; Email: " + email + "; DateBorn: " + dateBorn +
			"; CountryID: " + countryID + "; Confiable: " + confiable;
	}
	
	public int UniqueID {
		get { return uniqueID; }
		set { uniqueID = value; }
	}

	//the set's are for the server
	//"Private, internal, and protected members do not get serialized.  
	//If the accessor is not specific, it is private by default (and will not get serialized)."
	
	public string Name {
		get { return name; }
		set { name = value; }
	}

	public string Email {
		get { return email; }
		set { email = value; }
	}

	public string DateBorn {
		get { return dateBorn; }
		set { dateBorn = value; }
	}

	public int CountryID {
		get { return countryID; }
		set { countryID = value; }
	}

	public bool Confiable {
		get { return confiable; }
		set { confiable = value; }
	}

}
