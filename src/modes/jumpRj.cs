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


public class JumpRj : Jump
{
	string tvString;
	string tcString;
	int jumps; //total number of jumps
	double time; //time elapsed
	string limited; //the teorically values, eleven jumps: "11J" (time recorded in "time"), 10 seconds: "10T" (jumps recorded in jumps)
	private string angleString;

	bool calculatedStats;
	private double tvMax;
	private double tcMax;
	private double tvAvg;
	private double tcAvg;
	private double heightAvg;


	public JumpRj() {
		calculatedStats = false;
	}

	public JumpRj(int uniqueID, int personID, int sessionID, string type,
			double tvMax, double tcMax,
			double fall, double weightPercent, string description,
			double tvAvg, double tcAvg,
			string tvString, string tcString,
			int jumps, double time, string limited,
			string angleString, int simulated, string datetime,
			double heightAvg)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.sessionID = sessionID;
		this.type = type;
		this.tvMax = tvMax;
		this.tcMax = tcMax;
		this.fall = fall;
		this.weightPercent = weightPercent;
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
		this.heightAvg = heightAvg;

		calculatedStats = true;
	}
	
	//after inserting database (SQL)
	public JumpRj(int uniqueID, int personID, int sessionID, string type, 
			string tvString, string tcString, double fall, double weightPercent, 
			string description, int jumps, double time, string limited, string angleString, int simulated, string datetime)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.sessionID = sessionID;
		this.type = type;
		this.tvString = tvString;
		this.tcString = tcString;
		this.fall = fall;
		this.weightPercent = weightPercent;
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
		this.weightPercent = Convert.ToDouble(Util.ChangeDecimalSeparator(eventString[7]));
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
				type, TvMax, TcMax, fall, weightPercent,
				description, TvAvg, TcAvg, tvString, tcString,
				jumps, time, limited, 
				angleString, simulated, datetime, heightAvg);
	}

	// based on treeviewJump printAVG
	public double PowerAverage (double personMassInKg)
	{
		double powerSum = 0;
		for (int i = 0; i < tcList.Count; i ++)
		{
			double tc = tcList[i];
			double tv = tvList[i];
			double myfall = 0;
			double weightInKg = WeightInKg (personMassInKg);
			if (tcList[i] == -1) //startIn at first jump tc is 0, better check like this (string)
				powerSum += GetPower (tv, personMassInKg, weightInKg);
			else {
				if (i == 0)
					myfall = fall;
				else
					myfall = Util.GetHeightInCentimeters (tvList[i-1]);

				powerSum += Jump.GetDjPower (tc, tv,
						(personMassInKg + weightInKg), myfall);

				/* debug
				LogB.Information (string.Format (
							"at jumpRj.PowerAverage, tc: {0}, tv: {1}, (personMassInKg + weightInKg): {2}, myfall: {3}, powerSum: {4}",
							tc, tv, (personMassInKg + weightInKg), myfall, powerSum));

				LogB.Information ("at jumpRj.PowerAverage, powerSum = ", powerSum.ToString());
				*/
			}
		}
		return UtilAll.DivideSafe (powerSum, tcList.Count);
	}

	// based on treeviewJump printAVG
	public double StiffnessAverage (double personMassInKg, double weightInKg)
	{
		double stiffnessSum = 0;
		int stiffnessCount = 0;

		for (int i = 0; i < tcList.Count; i ++)
		{
			if(tcList[i] == -1) //startIn at first jump tc is 0, better check like this (string)
			{
				//do nothing
			} else
			{
				stiffnessSum += Util.GetStiffness
					(personMassInKg, weightInKg, tvList[i], tcList[i]);
				stiffnessCount ++;
			}
		}

		return UtilAll.DivideSafe (stiffnessSum, stiffnessCount);
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

	//both used on jumpsRjFatigue
	public List<double> TvList
	{
		get { return tvList; }
	}
	public List<double> TcList
	{
		get { return tcList; }
	}


	/*
	public List<double> TcPlusTvList
	{
		get {
			List<double> l = new List<double>();
			for (int i = 0; i < tcList.Count; i ++)
			{
				if (tcList[i] <= 0)
					l.Add (tvList[i]); //discard -1 tc on first jump starting in
				else
					l.Add (tcList[i] + tvList[i]);
			}
			return l;
		}
	}
	*/
	//tc + tv, used on jumpsRjFatigue
	// this method has an static version on Util.GetTotalTime ()
	public List<double> TcPlusTvAccumulatedList
	{
		get {
			List<double> l = new List<double>();
			double accumulatedSum = 0;
			for (int i = 0; i < tcList.Count; i ++)
			{
				if (tcList[i] <= 0)
					accumulatedSum += tvList[i]; //discard -1 tc on first jump starting in
				else
					accumulatedSum += tcList[i] + tvList[i];

				l.Add (accumulatedSum);
			}
			return l;
		}
	}

	public double tvLast
	{
		get {
			if(tvList == null || tvList.Count == 0)
				return 0;
			else
				return tvList[tvList.Count -1];
		}
	}
	public double tcLast
	{
		get {
			if(tcList == null || tcList.Count == 0)
				return 0;
			else
				return tcList[tcList.Count -1];
		}
	}

	private double tcFirst
	{
		get {
			if(tcList == null || tcList.Count == 0)
				return 0;
			else
				return tcList[0];
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
	public static List<double> HeightListFromTvString (string tvString)
	{
		List<double> l = new List<double>();
		string [] strFull = tvString.Split(new char[] {'='});
		foreach(string str in strFull)
			if(Util.IsNumber(Util.ChangeDecimalSeparator(str), true))
				l.Add(Util.GetHeightInCentimeters(Convert.ToDouble(Util.ChangeDecimalSeparator(str))));

		return l;
	}

	public List<double> FallList
	{
		get {
			//get the heights
			List<double> l = HeightList;

			//insert a fall at position 0 (if started outside)
			if (tcFirst > 0)
				l.Insert (0, fall);

			//delete the last flight time
			l.RemoveAt (l.Count -1);

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

	~JumpRj() {}
}
