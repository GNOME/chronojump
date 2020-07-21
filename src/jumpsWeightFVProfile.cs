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
 *  Copyright (C) 2019-2020   Xavier de Blas <xaviblas@gmail.com>, Jordi Rodeiro <jordirodeiro@gmail.com>
 */

using System;
using System.Collections.Generic; //List

public class JumpsWeightFVProfile
{
	private List<PointF> point_l;
	LeastSquaresLine ls;
	private double hp0; //meters
	private double g = 9.81;

	//constructor
	public JumpsWeightFVProfile()
	{
	}
	
	public void Calculate (int personID, int sessionID, double personWeight, double trochanterToe, double trochanterFloorOnFlexion)
	{
		hp0 = (trochanterToe - trochanterFloorOnFlexion) / 100.0;

		//1 get data
                List<Jump> jump_l = SqliteJump.SelectJumpsWeightFVProfile (personID, sessionID, false); //TODO:true);

		//2 convert to list of PointF
		point_l = new List<PointF>();
                foreach(Jump j in jump_l)
		{
			/*
			point_l.Add(new PointF(
						j.Weight,
						Util.GetHeightInCentimeters(j.Tv)
						));
			LogB.Information("Added point: {0}", j.ToString());
			LogB.Information("with weight: {0}", j.Weight.ToString());
			*/
			//Samozino formula is F = m*g*( (h/hp0) +1)
			//h is jump's height
			//hp0 = trochanterToe - trochanterFloorOnFlexion
			double force = (personWeight + (personWeight * j.Weight / 100.0)) * 9.81 * ( ( Util.GetHeightInMeters(j.Tv) / hp0 ) + 1 );

			point_l.Add(new PointF(
						Util.GetInitialSpeed(j.Tv, true), //TODO: pass preferences.metersSecondsPreferred and show it on graph label
						force
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
