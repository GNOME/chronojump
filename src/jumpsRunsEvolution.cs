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
 *  Copyright (C) 2019-2021   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List

//TODO: very similar to JumpsDjOptimalFall, refactorize if needed

public abstract class JumpsRunsEvolution
{
	protected List<PointF> point_l;
	protected LeastSquaresLine ls;

	public abstract void Calculate(int personID, string type, bool onlyBestInSession);

	protected void getLeastSquaresLine () //straight line
	{
		ls = new LeastSquaresLine();
		ls.Calculate(point_l);

		LogB.Information(string.Format("slope = {0}; intercept = {1}", ls.Slope, ls.Intercept));
	}

	public double GetMaxValue()
	{
		double maxValue = 0;
                foreach(PointF p in point_l)
		{
			if(p.X > maxValue)
				maxValue = p.X;
			if(p.Y > maxValue)
				maxValue = p.Y;
		}

		return maxValue;
	}

	public List<PointF> Point_l
	{
		get { return point_l; }
	}

	public double Slope
	{
		get { return ls.Slope; }
	}

	public double Intercept
	{
		get { return ls.Intercept; }
	}
}

public class JumpsEvolution : JumpsRunsEvolution
{
	//constructor
	public JumpsEvolution()
	{
	}

	public override void Calculate (int personID, string jumpType, bool onlyBestInSession)
	{
		//1 get data
                List<Jump> jump_l = SqliteJump.SelectJumps (-1, personID, jumpType, Sqlite.Orders_by.DEFAULT, -1, false, onlyBestInSession);

		//2 convert to list of PointF
		point_l = new List<PointF>();
		int currentSession = -1;
                foreach(Jump j in jump_l)
		{
			if(onlyBestInSession)
			{
				//at onlyBestInSession they return ordered by sessionID, jump.Tv DESC
				if(j.SessionID == currentSession)
					continue;
				else
					currentSession = j.SessionID;
			}

			DateTime dt = UtilDate.FromFile(j.Datetime);
			double dtDouble = UtilDate.DateTimeYearDayAsDouble(dt);

			point_l.Add(new PointF(dtDouble, Util.GetHeightInCentimeters(j.Tv)));
		}

		getLeastSquaresLine ();
	}
}

public class RunsEvolution : JumpsRunsEvolution
{
	private bool showTime;
	private bool metersSecondsPreferred;

	//constructor
	public RunsEvolution()
	{
	}

	public void PassParameters(bool showTime, bool metersSecondsPreferred)
	{
		this.showTime = showTime;
		this.metersSecondsPreferred = metersSecondsPreferred;
	}

	public override void Calculate (int personID, string runType, bool onlyBestInSession)
	{
		//1 get data
                List<Run> run_l = SqliteRun.SelectRuns (false, -1, personID, runType,
				Sqlite.Orders_by.DEFAULT, -1, false, onlyBestInSession);

		//2 convert to list of PointF
		point_l = new List<PointF>();
		int currentSession = -1;
                foreach(Run r in run_l)
		{

			if(onlyBestInSession)
			{
				//at onlyBestInSession they return ordered by sessionID, run.distance/run.time DESC
				if(r.SessionID == currentSession)
					continue;
				else
					currentSession = r.SessionID;
			}

			DateTime dt = UtilDate.FromFile(r.Datetime);
			double dtDouble = UtilDate.DateTimeYearDayAsDouble(dt);

			if(showTime)
				point_l.Add(new PointF(dtDouble, r.Time));
			else {
				r.MetersSecondsPreferred = metersSecondsPreferred;
				point_l.Add(new PointF(dtDouble, r.Speed));
			}
		}

		getLeastSquaresLine ();
	}
}
