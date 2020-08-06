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
 *  Copyright (C) 2020   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List

//TODO: very similar to JumpsDjOptimalFall, jumpsEvolution, refactorize if needed

public class JumpsRjFatigue
{
	private List<PointF> point_l;
	LeastSquaresLine ls;

	//constructor
	public JumpsRjFatigue()
	{
	}
	
	public void Calculate (int sessionID, int personID, string jumpType, bool useHeights)
	{
		//1 get data
                List<JumpRj> jrj_l = SqliteJumpRj.SelectJumps (false, sessionID, personID, jumpType);

		//2 convert to list of PointF
		point_l = new List<PointF>();
		int currentSession = -1;
                foreach(JumpRj j in jrj_l)
		{
			List<double> y_l;
			if(useHeights)
				y_l = j.HeightList;
			else
				y_l = j.TvTcList;

                	for(int i = 0; i < y_l.Count ; i ++)
				point_l.Add(new PointF(
							i+1,
							y_l[i]
						      ));
			break; //at the moment only do it for one jump
		}

		//3 get LeastSquaresLine (straight line)
		ls = new LeastSquaresLine();
		ls.Calculate(point_l);

		//4 print data
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
