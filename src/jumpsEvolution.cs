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
 *  Copyright (C) 2019   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List

//TODO: very similar to JumpsDjOptimalFall, refactorize if needed

public class JumpsEvolution
{
	private List<Point> point_l;
	LeastSquaresLine ls;

	//constructor
	public JumpsEvolution()
	{
	}
	
	public void Calculate (int personID, string jumpType)
	{
		//1 get data
                List<Jump> jump_l = SqliteJump.SelectJumps (personID, -1, jumpType);

		//2 convert to list of Point
		point_l = new List<Point>();
                foreach(Jump j in jump_l)
		{
			DateTime dt = UtilDate.FromFile(j.Datetime);
			double dtDouble = UtilDate.DateTimeYearDayAsDouble(dt);

			point_l.Add(new Point(
						dtDouble,
						Util.GetHeightInCentimeters(j.Tv)
						));
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
                foreach(Point p in point_l)
		{
			if(p.X > maxValue)
				maxValue = p.X;
			if(p.Y > maxValue)
				maxValue = p.Y;
		}

		return maxValue;
	}

	public List<Point> Point_l
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
