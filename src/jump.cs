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
using Mono.Data.SqliteClient;


public class Jump {

	int personID;
	int sessionID;
	int uniqueID;
	string type;
	double tv;
	double tc;
	int fall;
	string weight;
	string description;
	string tvString;
	string tcString;
	int jumps; //total number of jumps
	double time; //time elapsed
	string limited; //the teorically values, eleven jumps: "11=J" (time recorded in "time"), 10 seconds: "10=T" (jumps recorded in jumps)


	public Jump() {
	}
	
	public Jump(int uniqueID, int personID, int sessionID, string type, double tv, double tc, int fall, string weight, string description)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.sessionID = sessionID;
		this.type = type;
		this.tv = tv;
		this.tc = tc;
		this.fall = fall;
		this.weight = weight;
		this.description = description;
	}

	//RJ
	public Jump(int uniqueID, int personID, int sessionID, string type, string tvString, string tcString, int fall, string weight, string description, int jumps, double time, string limited)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.sessionID = sessionID;
		this.type = type;
		this.tvString = tvString;
		this.tcString = tcString;
		this.fall = fall;
		this.weight = weight;
		this.description = description;
		this.jumps = jumps;
		this.time = time;
		this.limited = limited;
	}

	private double getMax (string values)
	{
		string [] myStringFull = values.Split(new char[] {'='});
		double max = 0;
		foreach (string jump in myStringFull) {
			if ( Convert.ToDouble(jump) > max ) {
				max = Convert.ToDouble(jump);
			}
		}
		return max ; 
	}

	private double getAverage (string jumps)
	{
		string [] myStringFull = jumps.Split(new char[] {'='});
		double myAverage = 0;
		double myCount = 0;
		foreach (string jump in myStringFull) {
			myAverage = myAverage + Convert.ToDouble(jump);
			myCount ++;
		}
		return myAverage / myCount ; 
	}

	
	public string Type
	{
		get
		{
			return type;
		}
		set
		{
			type = value;
		}
	}
	
	public double Tv
	{
		get
		{
			return tv;
		}
		set
		{
			tv = value;
		}
	}
	
	public double Tc
	{
		get
		{
			return tc;
		}
		set
		{
			tc = value;
		}
	}
	
	public int Fall
	{
		get
		{
			return fall;
		}
		set
		{
			fall = value;
		}
	}
	
	public string Weight
	{
		get
		{
			return weight;
		}
		set 
		{
			weight = value;
		}
	}
	
	public string Description
	{
		get
		{
			return description;
		}
		set 
		{
			description = value;
		}
	}
	
	public string Limited
	{
		get {
			return limited;
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

	public int SessionID
	{
		get
		{
			return sessionID;
		}
	}

	public int PersonID
	{
		get
		{
			return personID;
		}
	}
		
	public string JumperName
	{
		get
		{
			return SqlitePerson.SelectJumperName(personID);
		}
	}

	public double TvMax
	{
		get
		{
			return getMax (tvString);
		}
	}
		
	public double TcMax
	{
		get
		{
			return getMax (tcString);
		}
	}
		
	public double TvAvg
	{
		get
		{
			return getAverage (tvString);
		}
	}
		
	public double TcAvg
	{
		get
		{
			return getAverage (tcString);
		}
	}
		
	~Jump() {}
	   
}
