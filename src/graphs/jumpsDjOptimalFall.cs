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
 *  Copyright (C) 2019-2023   Xavier de Blas <xaviblas@gmail.com>
 *  Copyright (C) 2020 Jordi Rodeiro <jordirodeiro@gmail.com>
 */

using System;
using System.Collections.Generic; //List

public class JumpsDjOptimalFall : Graphs
{
	LeastSquaresParabole ls;

	//constructor
	public JumpsDjOptimalFall()
	{
	}
	
	public void Calculate (int personID, int sessionID, string jumpType)
	{
		//1 get data
                List<Jump> jump_l = SqliteJump.SelectDJ (personID, sessionID, jumpType, true);

		//2 convert to list of Point
		//List<Point> point_l = new List<Point>();
		point_l = new List<PointF>();
                foreach(Jump j in jump_l)
			point_l.Add(new PointF(
						j.Fall,
						Util.GetHeightInCentimeters(j.Tv)
						));

		//3 get LeastSquaresParabole
		ls = new LeastSquaresParabole();
		ls.Calculate(point_l);

		//4 print data
		if(ls.CalculatedCoef)
			LogB.Information(string.Format("coef = {0} {1} {2}",
						ls.Coef[0], ls.Coef[1], ls.Coef[2]));

		if(ls.CalculatedXatMaxY)
			LogB.Information(string.Format("XatMaxY = {0}", ls.XatMaxY));
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

	public double[] Coefs
	{
		get {
			if(! ls.CalculatedCoef)
				return new double[0];

			LogB.Information(string.Format("coef0:{0}", ls.Coef[0]));
			return ls.Coef;
		}
	}

	public LeastSquaresParabole.ParaboleTypes ParaboleType
	{
		get { return ls.ParaboleType; }
	}

	public double XatMaxY //model
	{
		get {
			if(! ls.CalculatedXatMaxY)
				return -1;

			return ls.XatMaxY;
		}
	}
}
