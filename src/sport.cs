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

public class Sport
{
	private int uniqueID;
	private string name;
	private bool userDefined;
	private bool hasSpeciallities = false;
	private string graphLink = "";
	private string userDefinedString = "user";

	//only initializing
	public Sport() {
	}

	//after inserting database (SQL)
	public Sport(int uniqueID, string name, bool userDefined, bool hasSpeciallities, string graphLink) {
		this.uniqueID = uniqueID;
		this.name = name;
		this.userDefined = userDefined;
		this.hasSpeciallities = hasSpeciallities;
		this.graphLink = graphLink;
	}

	//after inserting database (SQL)
	public Sport(int uniqueID, string name, int userDefined, bool hasSpeciallities, string graphLink) {
		this.uniqueID = uniqueID;
		this.name = name;
		this.userDefined = Util.IntToBool(userDefined);
		this.hasSpeciallities = hasSpeciallities;
		this.graphLink = graphLink;
	}

	//used when displayed on combobox, as written below in ToString()
	//Global is... uniqueID:name
	//User is... uniqueID(user):name 
	//(user) is changed Catalog
	public Sport(string myString) {
		string [] sportFull = myString.Split(new char[] {':'});
		if(Util.IsNumber(sportFull[0])) {
			this.uniqueID = Convert.ToInt32(sportFull[0]);
			this.userDefined = false;
		} else {
			string [] myStr2 = sportFull[0].Split(new char[] {'('});
			this.uniqueID = Convert.ToInt32(myStr2[0]);
			this.userDefined = false;
		}
			
		Sport myTempSport = SqliteSport.Select(uniqueID);
		//this.name = sportFull[1];
		this.name = myTempSport.Name; //get sport name in english
		this.hasSpeciallities = myTempSport.HasSpeciallities;
		this.graphLink = myTempSport.GraphLink;
	}

	public override string ToString() {
		string myString = "";
		if(this.userDefined)
			myString = "(" + Catalog.GetString(userDefinedString) + ")";
		return uniqueID + myString + ":" + Catalog.GetString(name);
	}

	public int UniqueID {
		get { return uniqueID; } 
	}
	
	public string Name {
		get { return name; } 
	}

	public bool UserDefined {
		get { return userDefined; } 
	}
	
	public string GraphLink {
		get { return graphLink; } 
	}
	
	public bool HasSpeciallities {
		get { return hasSpeciallities; } 
	}

}
