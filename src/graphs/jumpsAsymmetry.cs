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
 *  Copyright (C) 2022-2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List

public class JumpsAsymmetry : Graphs
{
	private List<JumpsAsymmetryDay> jad_l;
	protected List<DateTime> dates_l; //used on button press to know day date instead of date as double

	//constructor
	public JumpsAsymmetry ()
	{
		MouseReset ();
	}

	public void Calculate (int personID, int sessionID, bool bilateral, bool means,
			string jumpBilateralStr, string j1Str, string j2Str)
	{
		List<string> jumpTypes_l = new List<string> ();
		if (bilateral)
			jumpTypes_l.Add (jumpBilateralStr);
		jumpTypes_l.Add (j1Str);
		jumpTypes_l.Add (j2Str);

		Sqlite.StatType stat = Sqlite.StatType.AVG;
		if (! means)
			stat = Sqlite.StatType.MAX;

		//1 get data
                List<SqliteStruct.DateTypeResult> sdtr_l = SqliteJump.SelectJumpsStatsByDay (personID, jumpTypes_l, stat);

		point_l = new List<PointF> ();
		dates_l = new List<DateTime> ();
		jad_l = new List<JumpsAsymmetryDay> ();
		JumpsAsymmetryDay jad = new JumpsAsymmetryDay (bilateral);
		string currentDate = "";

		foreach (SqliteStruct.DateTypeResult sdtr in sdtr_l)
		{
			LogB.Information (string.Format ("-MMMM {0} {1} {2}", sdtr.date, sdtr.type, Util.GetHeightInCm (sdtr.result)));

			if (sdtr.date != currentDate)
			{
				if (jad.Complete ())
				{
					jad.AddDate (currentDate);
					jad_l.Add (jad);
					//LogB.Information ("currentDate: " + currentDate);

					//add time to be able to graph
					DateTime dt = UtilDate.FromFile (currentDate + "_00-00-01");
					double dtDouble = UtilDate.DateTimeYearDayAsDouble (dt);
					point_l.Add (new PointF(dtDouble, jad.GetIndex ()));
					dates_l.Add (dt);
				}

				//If new new date, create a new jad, even if previous was complete or not
				jad = new JumpsAsymmetryDay (bilateral);
			}

			if (bilateral && sdtr.type == jumpBilateralStr)
				jad.AddBilateral (sdtr.type, Util.GetHeightInCm (sdtr.result));
			else if (sdtr.type == j1Str)
				jad.AddAsymmetry1 (sdtr.type, Util.GetHeightInCm (sdtr.result));
			else if (sdtr.type == j2Str)
				jad.AddAsymmetry2 (sdtr.type, Util.GetHeightInCm (sdtr.result));

			currentDate = sdtr.date;
		}
		
		if (jad.Complete ())
		{
			jad.AddDate (currentDate);
			jad_l.Add (jad);

			//add time to be able to graph
			DateTime dt = UtilDate.FromFile (currentDate + "_00-00-01");
			double dtDouble = UtilDate.DateTimeYearDayAsDouble (dt);
			point_l.Add (new PointF(dtDouble, jad.GetIndex ()));
			dates_l.Add (dt);
		}

		/* debug
		LogB.Information ("jads: " + jad_l.Count.ToString ());
		foreach (JumpsAsymmetryDay jadDebug in jad_l)
			LogB.Information (jadDebug.ToString ());
			*/
	}

	public List<JumpsAsymmetryDay> Jad_l
	{
		get { return jad_l; }
	}
	public List<DateTime> Dates_l
	{
		get { return dates_l; }
	}

}
	
public class JumpsAsymmetryDay
{
	private bool bilateral;

	public string date;
	public string bilateralType;
	public double bilateralResult;
	public string asymmetry1Type;
	public double asymmetry1Result;
	public string asymmetry2Type;
	public double asymmetry2Result;

	public JumpsAsymmetryDay (bool bilateral)
	{
		this.bilateral = bilateral;

		date = "";
		bilateralType = "";
		asymmetry1Type = "";
		asymmetry2Type = "";
	}

	public void AddBilateral (string type, double result)
	{
		bilateralType = type;
		bilateralResult = result;
	}

	public void AddAsymmetry1 (string type, double result)
	{
		asymmetry1Type = type;
		asymmetry1Result = result;
	}

	public void AddAsymmetry2 (string type, double result)
	{
		asymmetry2Type = type;
		asymmetry2Result = result;
	}

	public void AddDate (string date)
	{
		this.date = date;
	}

	public bool Complete ()
	{
		//LogB.Information (string.Format ("complete? bilateral: {0}, bilateral ({1}): {2}, a1: ({3}): {4}, a2: ({5}): {6}", 
		//		bilateral, bilateralType, bilateralResult, asymmetry1Type, asymmetry1Result, asymmetry2Type, asymmetry2Result));

		if (bilateral && bilateralType != "" && asymmetry1Type != "" && asymmetry2Type != "")
			return true;
		else if (! bilateral && asymmetry1Type != "" && asymmetry2Type != "")
			return true;

		return false;
	}

	public double GetIndex ()
	{
		if (bilateral) {
			//CMJ-(slcmjR+slcmjL)
			return bilateralResult - (asymmetry1Result + asymmetry2Result);
		} else {
			//100*(tfG-tfB)/tfG (being tfG the flight time of the "good" leg, and tfB of the "bad" leg). Good and bad will be related to the results of each day.
			double good = asymmetry1Result;
			double bad = asymmetry2Result;
			if (bad > good)
			{
				good = asymmetry2Result;
				bad = asymmetry1Result;
			}
			return 100.0 * UtilAll.DivideSafe (good-bad, good);
		}
	}

	//debug
	public override string ToString ()
	{
		return string.Format ("bilateral ({0}): {1}, a1: ({2}): {3}, a2: ({4}): {5}", 
				bilateralType, bilateralResult, asymmetry1Type, asymmetry1Result, asymmetry2Type, asymmetry2Result);
	}
}
