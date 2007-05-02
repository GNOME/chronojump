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
using System.Text; //StringBuilder

using Mono.Unix;

public class Jump : Event 
{
	protected double tv;
	protected double tc;
	protected int fall;
	protected double weight; //always write in % (not kg or %) then sqlite can do avgs

	//for not checking always in database
	protected bool hasFall;

	public Jump() {
	}

	//after inserting database (SQL)
	public Jump(int uniqueID, int personID, int sessionID, string type, double tv, double tc, int fall, double weight, string description)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.personName = SqlitePerson.SelectJumperName(personID);
		this.sessionID = sessionID;
		this.type = type;
		this.tv = tv;
		this.tc = tc;
		this.fall = fall;
		this.weight = weight;
		this.description = description;
	}


	public bool TypeHasWeight
	{
		get { return SqliteJumpType.HasWeight(type); }
	}
	
	public virtual bool TypeHasFall
	{
		get { return SqliteJumpType.HasFall("jumpType", type); } //jumpType is the table name
	}
	
	public double Tv
	{
		get { return tv; }
		set { tv = value; }
	}
	
	public double Tc
	{
		get { return tc; }
		set { tc = value; }
	}
	
	public int Fall
	{
		get { return fall; }
		set { fall = value; }
	}
	
	public double Weight
	{
		get { return weight; }
		set { weight = value; }
	}

	
	~Jump() {}
	   
}

public class JumpRj : Jump
{
	string tvString;
	string tcString;
	int jumps; //total number of jumps
	double time; //time elapsed
	string limited; //the teorically values, eleven jumps: "11=J" (time recorded in "time"), 10 seconds: "10=T" (jumps recorded in jumps)
	double limitAsDouble;	//-1 for non limited (unlimited repetitive jump until "finish" is clicked)
	bool jumpsLimited;
	bool firstRjValue;
	private double tcCount;
	private double tvCount;
	private double lastTc;
	private double lastTv;
	
	public JumpRj() {
	}

	//after inserting database (SQL)
	public JumpRj(int uniqueID, int personID, int sessionID, string type, 
			string tvString, string tcString, int fall, double weight, 
			string description, int jumps, double time, string limited)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.personName = SqlitePerson.SelectJumperName(personID);
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
	
	
	public string Limited
	{
		get { return limited; }
		set { limited = value; }
	}
	
	public override bool TypeHasFall
	{
		get { return SqliteJumpType.HasFall("jumpRjType", type); } //jumpRjType is the table name
	}
	
	public double TvMax
	{
		get { return Util.GetMax (tvString); }
	}
		
	public double TcMax
	{
		get { return Util.GetMax (tcString); }
	}
		
	public double TvAvg
	{
		get { return Util.GetAverage (tvString); }
	}
		
	public double TcAvg
	{
		get { return Util.GetAverage (tcString); }
	}
	
	public string TvString
	{
		get { return tvString; }
		set { tvString = value; }
	}
		
	public string TcString
	{
		get { return tcString; }
		set { tcString = value; }
	}

	public int Jumps
	{
		get { return jumps; }
		set { jumps = value; }
	}
	
	public bool JumpsLimited
	{
		get { return jumpsLimited; }
	}
		
		
	~JumpRj() {}
}
