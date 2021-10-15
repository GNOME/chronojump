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
 *  Copyright (C) 2004-2020   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using System.Text; //StringBuilder
using System.Collections.Generic; //List
using Mono.Unix;

public class Jump : Event 
{
	protected double tv;
	protected double tc;
	protected double fall;	//-1 if start inside to detect the fall. This is a special case where there are two flight times, but 1st is only used to detect fall-
				//when jump finishes, fall is calculated and 2nd flight time is stored. It becomes a jump with one TC and one TF
	protected double weight; //always write in % (not kg or %) then sqlite can do avgs

	//for not checking always in database
	protected bool hasFall;
	private double angle;
	protected string datetime;

	public Jump() {
	}
	
	//after inserting database (SQL)
	public Jump(int uniqueID, int personID, int sessionID, string type, double tv, double tc, double fall, double weight, string description, double angle, int simulated, string datetime)
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
		this.datetime = datetime;
	}

	//used to select a jump at SqliteJump.SelectJumpData and at Sqlite.converTables
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
		this.datetime = eventString[11];
	}

	public static List<Event> JumpListToEventList(List<Jump> jumps)
	{
		List<Event> events = new List<Event>();
		foreach(Jump jump in jumps)
			events.Add((Event) jump);

		return events;
	}

	public override int InsertAtDB (bool dbconOpened, string tableName) {
		return SqliteJump.Insert(dbconOpened, tableName, 
				uniqueID.ToString(), 
				personID, sessionID, 
				type, tv, tc, fall, 
				weight, description, 
				angle, simulated, datetime);
	}
	
	public virtual double Stiffness(double personMassInKg, double extraMass) 
	{
		return Util.GetStiffness(personMassInKg, extraMass, tv, tc);
	}	
	

	public override string ToString() {
		return uniqueID + ":" + personID + ":" + sessionID + ":" + type + ":" + tv + ":" + tc + ":" + datetime + ":" + description;
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

	public double RSI {
		get { return UtilAll.DivideSafe(Util.GetHeightInMeters(tv), tc); }
	}
	public static double CalculateRSI (double mytv, double mytc)
	{
		return UtilAll.DivideSafe(Util.GetHeightInMeters(mytv), mytc);
	}

	public double Angle {
		get { return angle; }
		set { angle = value; }
	}

	public string Datetime {
		get { return datetime; }
		set { datetime = value; }
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
	bool jumpsLimited;
	private string angleString;

	bool calculatedStats;
	private double tvMax;
	private double tcMax;
	private double tvAvg;
	private double tcAvg;


	public JumpRj() {
		calculatedStats = false;
	}

	public JumpRj(int uniqueID, int personID, int sessionID, string type,
			double tvMax, double tcMax,
			double fall, double weight, string description,
			double tvAvg, double tcAvg,
			string tvString, string tcString,
			int jumps, double time, string limited,
			string angleString, int simulated, string datetime)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.sessionID = sessionID;
		this.type = type;
		this.tvMax = tvMax;
		this.tcMax = tcMax;
		this.fall = fall;
		this.weight = weight;
		this.description = description;
		this.tvAvg = tvAvg;
		this.tcAvg = tcAvg;
		this.tvString = tvString;
		this.tcString = tcString;
		this.jumps = jumps;
		this.time = time;
		this.limited = limited;
		this.angleString = angleString;
		this.simulated = simulated;
		this.datetime = datetime;

		calculatedStats = true;
	}
	
	//after inserting database (SQL)
	public JumpRj(int uniqueID, int personID, int sessionID, string type, 
			string tvString, string tcString, double fall, double weight, 
			string description, int jumps, double time, string limited, string angleString, int simulated, string datetime)
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
		this.datetime = datetime;

		calculatedStats = false;
	}
	
	//used to select a jump at SqliteJumpRj.SelectJumpData and at Sqlite.convertTables
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
		this.datetime = eventString[18];

		calculatedStats = false;
	}

	public static List<Event> JumpListToEventList(List<JumpRj> jumps)
	{
		List<Event> events = new List<Event>();
		foreach(JumpRj jump in jumps)
			events.Add((Event) jump);

		return events;
	}

	public override int InsertAtDB (bool dbconOpened, string tableName) {
		return SqliteJumpRj.Insert(dbconOpened, tableName, 
				uniqueID.ToString(),
				personID, sessionID, 
				type, TvMax, TcMax, fall, weight,
				description, TvAvg, TcAvg, tvString, tcString,
				jumps, time, limited, 
				angleString, simulated, datetime);
	}


	public override double Stiffness(double personMassInKg, double extraMass) 
	{
		return Util.GetStiffness(personMassInKg, extraMass, TvAvg, TcAvg);
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
		get {
			if(! calculatedStats)
				tvMax = Util.GetMax (tvString);

			return tvMax;
		}
	}

	public double TcMax
	{
		get {
			if(! calculatedStats)
				tcMax = Util.GetMax (tcString);

			return tcMax;
		}
	}

	public double TvAvg
	{
		get {
			if(! calculatedStats)
				tvAvg = Util.GetAverage (tvString);

			return tvAvg;
		}
	}

	public double TcAvg
	{
		get {
			if(! calculatedStats)
				tcAvg = Util.GetAverage (tcString);

			return tcAvg;
		}
	}
	
		
	public string TvString
	{
		get { return tvString; }
		set { tvString = value; }
	}

	public List<double> TvTcList
	{
		get {
			List<double> l = new List<double>();
			string [] tvFull = TvString.Split(new char[] {'='});
			string [] tcFull = TcString.Split(new char[] {'='});
			if(tvFull.Length != tcFull.Length)
				return l;

			for(int i = 0; i < tvFull.Length ; i ++)
			{
				if( Util.IsNumber(Util.ChangeDecimalSeparator(tvFull[i]), true) &&
					Util.IsNumber(Util.ChangeDecimalSeparator(tcFull[i]), true) )
					l.Add(
							Convert.ToDouble(Util.ChangeDecimalSeparator(tvFull[i])) /
							Convert.ToDouble(Util.ChangeDecimalSeparator(tcFull[i])) );
			}
			return l;
		}
	}

	private List<double> tvList
	{
		get {
			List<double> l = new List<double>();
			string [] strFull = TvString.Split(new char[] {'='});
			foreach(string str in strFull)
			{
				if(Util.IsNumber(Util.ChangeDecimalSeparator(str), true))
					l.Add(Convert.ToDouble(Util.ChangeDecimalSeparator(str)));
			}
			return l;
		}
	}
	private List<double> tcList
	{
		get {
			List<double> l = new List<double>();
			string [] strFull = TcString.Split(new char[] {'='});
			foreach(string str in strFull)
			{
				if(Util.IsNumber(Util.ChangeDecimalSeparator(str), true))
					l.Add(Convert.ToDouble(Util.ChangeDecimalSeparator(str)));
			}
			return l;
		}
	}

	public List<double> HeightList
	{
		get {
			List<double> l = new List<double>();
			string [] strFull = TvString.Split(new char[] {'='});
			foreach(string str in strFull)
			{
				if(Util.IsNumber(Util.ChangeDecimalSeparator(str), true))
					l.Add(Util.GetHeightInCentimeters(Convert.ToDouble(Util.ChangeDecimalSeparator(str))));
			}
			return l;
		}
	}

	public double HeightTotal
	{
		get {
			double total = 0;
			foreach(double h in HeightList)
				total += h;

			return total;
		}
	}

	public double TvSum
	{
		get {
			double total = 0;
			foreach(double d in tvList)
				total += d;

			return total;
		}
	}
	public double TcSumCaringForStartIn //does not add to the sum the -1 on startIn
	{
		get {
			double total = 0;
			foreach(double d in tcList)
			{
				if(total == 0 && d < 0)
					continue;

				total += d;
			}

			return total;
		}
	}


	public List<double> RSIList
	{
		get {
			List<double> l = new List<double>();
			List<double> heightFull = HeightList;
			string [] tcFull = TcString.Split(new char[] {'='});
			if(heightFull.Count != tcFull.Length)
				return l;

			for(int i = 0; i < heightFull.Count ; i ++)
			{
				if(Util.IsNumber(Util.ChangeDecimalSeparator(tcFull[i]), true))
					l.Add(
							(heightFull[i] / 100.0) //cm to m
							/
							Convert.ToDouble(Util.ChangeDecimalSeparator(tcFull[i])) );
			}
			return l;
		}
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
		get { return time; }
		set { time = value; }
	}
	
	public bool JumpsLimited
	{
		get { return jumpsLimited; }
	}
		
		
	~JumpRj() {}
}
