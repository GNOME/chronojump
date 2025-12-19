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
 *  Copyright (C) 2004-2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Data;
using System.Text; //StringBuilder
using System.Collections.Generic; //List
using Mono.Unix;

#if MICROSOFT_DATA_SQLITE
using Microsoft.Data.Sqlite;
using SQLiteTransaction = Microsoft.Data.Sqlite.SqliteTransaction;
using SQLiteCommand = Microsoft.Data.Sqlite.SqliteCommand;
using SQLiteDataReader = Microsoft.Data.Sqlite.SqliteDataReader;
using SQLiteConnection = Microsoft.Data.Sqlite.SqliteConnection;
#else
using System.Data.SQLite;
using SQLiteTransaction = System.Data.SQLite.SQLiteTransaction;
using SQLiteCommand = System.Data.SQLite.SQLiteCommand;
using SQLiteDataReader = System.Data.SQLite.SQLiteDataReader;
using SQLiteConnection = System.Data.SQLite.SQLiteConnection;
#endif

public class Jump : Event 
{
	protected double tv;
	protected double tc;
	protected double fall;	//-1 if start inside to detect the fall. This is a special case where there are two flight times, but 1st is only used to detect fall-
				//when jump finishes, fall is calculated and 2nd flight time is stored. It becomes a jump with one TC and one TF
	protected double weightPercent; //always write in % (not kg or %) then sqlite can do avgs

	//for not checking always in database
	protected bool hasFall;
	private double angle;
	protected string datetime;

	public Jump() {
	}
	
	//after inserting database (SQL)
	public Jump(int uniqueID, int personID, int sessionID, string type, double tv, double tc, double fall, double weightPercent, string description, double angle, int simulated, string datetime)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.sessionID = sessionID;
		this.type = type;
		this.tv = tv;
		this.tc = tc;
		this.fall = fall;
		this.weightPercent = weightPercent;
		this.description = description;
		this.angle = angle;
		this.simulated = simulated;
		this.datetime = datetime;
	}

	/*
	 * used to select a jump at old Sqlite.converTables
	 * converTables was last used on DB .79 (2010)
	 * it is not being updated to use readerOrdinal to fix mac arm problems
	 */
	public Jump(string [] eventString)
	{
		this.uniqueID = Convert.ToInt32(eventString[0]);
		this.personID = Convert.ToInt32(eventString[1]);
		this.sessionID = Convert.ToInt32(eventString[2]);
		this.type = eventString[3].ToString();
		this.tv = Convert.ToDouble(Util.ChangeDecimalSeparator(eventString[4]));
		this.tc = Convert.ToDouble(Util.ChangeDecimalSeparator(eventString[5]));
		this.fall = Convert.ToDouble(Util.ChangeDecimalSeparator(eventString[6]));
		this.weightPercent = Convert.ToDouble(Util.ChangeDecimalSeparator(eventString[7]));
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

	public override int InsertAtDB (bool dbconOpened, string tableName)
	{
		return SqliteJump.Insert (dbconOpened, tableName,
				uniqueID.ToString(), 
				personID, sessionID, 
				type, tv, tc, fall, 
				weightPercent, description, 
				angle, simulated, datetime);
	}
	//used on transaction at SqliteFourPlatformsJumpsSimple
	public int InsertAtDB (bool dbconOpened, string tableName, SQLiteCommand myCmd)
	{
		return SqliteJump.Insert (dbconOpened, tableName,
				uniqueID.ToString(),
				personID, sessionID,
				type, tv, tc, fall,
				weightPercent, description,
				angle, simulated, datetime, myCmd);
	}

	public static double GetDjPower (double tc, double tf, double mass, double fallHeight)
	{
		/*
		 * old method
		//relative potency in Watts/kg
		//Bosco. Pendent to find if published

		//P = 24.6 * (TotalTime + FlightTime) / ContactTime

		double tt = tc + tf; //totalTime

		return 24.6 * ( tt + tf ) / (Double)tc;
		*/

		//new method (proposal by Xavier Padullés)
		//Calcule the potential energies before (mass * g * fallHeight) and after the jump (mass * g * tv^2 * 1.22625)
		//and divide by the time during force is applied
		double g = 9.81;
		fallHeight = fallHeight / 100.0; //cm -> m

		return mass * g * ( fallHeight + 1.22625 * Math.Pow(tf,2) ) / (Double)tc;
	}

	//only Lewis now
	public static double GetPower (double tf, double bodyWeight, double extraWeightKg)
	{
		//LogB.Information ("tf: " + tf + ", bodyWeight: " + bodyWeight + ", extra: " + extraWeightKg);
		double pw = System.Math.Sqrt ( 4.9 ) * 9.8 * (bodyWeight + extraWeightKg) *
			System.Math.Sqrt(
				       Convert.ToDouble ( Util.GetHeightInCentimeters(tf.ToString()))/100);
		//LogB.Information ("pw: " + pw);
		return pw;

	}

	public virtual double Stiffness(double personMassInKg, double extraMass) 
	{
		return Util.GetStiffness(personMassInKg, extraMass, tv, tc);
	}	

	//in JumpRj do not use this because we don't know on which subjumps use it
	public double GetInitialSpeedJumpSimple (bool metersSecondsPreferred)
	{
		return (GetInitialSpeed (tv, metersSecondsPreferred));
	}

	//old code sends and returns strings
	public static string GetInitialSpeed (string time, bool metersSecondsPreferred)
	{
		return GetInitialSpeed (Convert.ToDouble (time), metersSecondsPreferred).ToString();
	}
	//new code (2019 ...) sends and returns doubles
	public static double GetInitialSpeed (double time, bool metersSecondsPreferred)
	{
		double height = Util.GetHeightInCentimeters (time);
		height = height / 100; //in meters

		// Vo = sqrt(2gh)
		double initialSpeed = System.Math.Sqrt ( 2 * 9.81 * height );

		if(! metersSecondsPreferred)
			initialSpeed *= 3.6;

		return initialSpeed;
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

	public double Height {
		// get { return (100 * 4.905 * Math.Pow (UtilAll.DivideSafe (tv, 2.0), 2)); }
		get { return 100.0 * 1.22625 * Math.Pow (tv,2); }
	}

	public double Tc {
		get { return tc; }
		set { tc = value; }
	}
	
	public double Fall {
		get { return fall; }
		set { fall = value; }
	}
	
	public double WeightPercent {
		get { return weightPercent; }
		set { weightPercent = value; }
	}
	public double WeightInKg (double personMassInKg)
	{
		return Util.WeightFromPercentToKg (weightPercent, personMassInKg);
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
