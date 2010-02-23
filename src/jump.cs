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
 *  Copyright (C) 2004-2009   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using System.Text; //StringBuilder

using Mono.Unix;

public class Jump : Event 
{
	protected double tv;
	protected double tc;
	protected double fall;
	protected double weight; //always write in % (not kg or %) then sqlite can do avgs

	//for not checking always in database
	protected bool hasFall;
	private double angle;

	public Jump() {
	}
	
	//after inserting database (SQL)
	public Jump(int uniqueID, int personID, int sessionID, string type, double tv, double tc, double fall, double weight, string description, double angle, int simulated)
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
		this.angle = angle;
		this.simulated = simulated;
	}

	//used to select a jump at SqliteJump.SelectNormalJumpData and at Sqlite.converTables
	public Jump(string [] eventString)
	{
		this.uniqueID = Convert.ToInt32(eventString[0]);
		this.personID = Convert.ToInt32(eventString[1]);
		this.sessionID = Convert.ToInt32(eventString[2]);
		this.type = eventString[3].ToString();
		this.tv = Convert.ToDouble(Util.ChangeDecimalSeparator(eventString[4]));
		this.tc = Convert.ToDouble(Util.ChangeDecimalSeparator(eventString[5]));
		this.fall = Convert.ToDouble(Util.ChangeDecimalSeparator(eventString[6]));
		this.weight = Convert.ToDouble(Util.ChangeDecimalSeparator(eventString[7]));
		this.description = eventString[8].ToString();
		this.angle = Convert.ToDouble(Util.ChangeDecimalSeparator(eventString[9]));
		this.simulated = Convert.ToInt32(eventString[10]);
	}


	public override int InsertAtDB (bool dbconOpened, string tableName) {
		return SqliteJump.Insert(dbconOpened, tableName, 
				uniqueID.ToString(), 
				personID, sessionID, 
				type, tv, tc, fall, 
				weight, description, 
				angle, simulated);
	}

	public override string ToString() {
		return uniqueID + ":" + personID + ":" + sessionID + ":" + type + ":" + tv + ":" + tc; //...
	}

	public virtual bool TypeHasWeight {
		get { return SqliteJumpType.HasWeight("jumpType", type); }
	}
	
	public virtual bool TypeHasFall {
		get { return SqliteJumpType.HasFall("jumpType", type); } //jumpType is the table name
	}
	
	public double Tv {
		get { return tv; }
		set { tv = value; }
	}
	
	public double Tc {
		get { return tc; }
		set { tc = value; }
	}
	
	public double Fall {
		get { return fall; }
		set { fall = value; }
	}
	
	public double Weight {
		get { return weight; }
		set { weight = value; }
	}

	public double Angle {
		get { return angle; }
		set { angle = value; }
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
	private string angleString;
	
	public JumpRj() {
	}
	
	//after inserting database (SQL)
	public JumpRj(int uniqueID, int personID, int sessionID, string type, 
			string tvString, string tcString, double fall, double weight, 
			string description, int jumps, double time, string limited, string angleString, int simulated)
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
		this.angleString = angleString;
		this.simulated = simulated;
	}
	
	//used to select a jump at SqliteJump.SelectRjJumpData and at Sqlite.convertTables
	public JumpRj(string [] eventString)
	{
		//foreach(string myStr in eventString)
		//	Log.WriteLine(myStr);

		this.uniqueID = Convert.ToInt32(eventString[0]);
		this.personID = Convert.ToInt32(eventString[1]);
		this.sessionID = Convert.ToInt32(eventString[2]);
		this.type = eventString[3].ToString();
		this.tvString = Util.ChangeDecimalSeparator(eventString[11].ToString());
		this.tcString = Util.ChangeDecimalSeparator(eventString[12].ToString());
		this.fall = Convert.ToDouble(Util.ChangeDecimalSeparator(eventString[6]));
		this.weight = Convert.ToDouble(Util.ChangeDecimalSeparator(eventString[7]));
		this.description = eventString[8].ToString();
		this.jumps = Convert.ToInt32(eventString[13]);
		this.time = Convert.ToDouble(Util.ChangeDecimalSeparator(eventString[14]));
		this.limited = eventString[15];
		this.angleString = eventString[16];
		this.simulated = Convert.ToInt32(eventString[17]);
	}
	


	public override int InsertAtDB (bool dbconOpened, string tableName) {
		return SqliteJumpRj.Insert(dbconOpened, tableName, 
				uniqueID.ToString(),
				personID, sessionID, 
				type, TvMax, TcMax, fall, weight,
				description, TvAvg, TcAvg, tvString, tcString,
				jumps, time, limited, 
				angleString, simulated);
	}

	public string Limited
	{
		get { return limited; }
		set { limited = value; }
	}
	
	public override bool TypeHasWeight
	{
		get { return SqliteJumpType.HasWeight("jumpRjType", type); }
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
	
	public double Time
	{
		set { time = value; }
	}
	
	public bool JumpsLimited
	{
		get { return jumpsLimited; }
	}
		
		
	~JumpRj() {}
}
