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

//TODO: very similar to JumpsDjOptimalFall, jumpsEvolution, refactorize if needed

public class JumpsRjFatigue : Graphs
{
	private List<double> tc_l;
	private List<double> tv_l;
	LeastSquaresLine ls;
	public enum Statistic { HEIGHTS, Q, RSI } //RSI is jump height (m)/ contact time (s)

	//constructor
	public JumpsRjFatigue()
	{
		MouseReset ();
	}

	public void Calculate (int uniqueID, Statistic statistic)
	{
		//1 get data
		JumpRj jumpRj = SqliteJumpRj.SelectJumpData (Constants.JumpRjTable, uniqueID, false, false);

		//2 convert to list of PointF
		List<double> y_l = jumpRj.HeightList; //(statistic == Statistic.HEIGHTS)
		if(statistic == Statistic.Q)
			y_l = jumpRj.TvTcList;
		else if(statistic == Statistic.RSI)
			y_l = jumpRj.RSIList;

		tc_l = jumpRj.TcList;
		tv_l = jumpRj.TvList;

		point_l = new List<PointF>();

		for(int i = 0; i < y_l.Count ; i ++)
			point_l.Add (new PointF (i+1, y_l[i]));

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

	public List<double> Tc_l
	{
		get { return tc_l; }
	}
	public List<double> Tv_l
	{
		get { return tv_l; }
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
