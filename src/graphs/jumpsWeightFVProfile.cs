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
	public bool NeedMoreXData;

	private List<PointF> point_l;
	private List<PointF> point_l_relative;
	LeastSquaresLine ls;
	private double personWeight;
	private double hp0; //meters
	private double g = 9.81;
	private double z;
	private double sfvOpt;

	//constructor
	public JumpsWeightFVProfile()
	{
		point_l = new List<PointF>();
	}
	
	public void Calculate (int personID, int sessionID, double personWeight, double trochanterToe, double trochanterFloorOnFlexion, bool onlyBestInWeight)
	{
		this.personWeight = personWeight;
		hp0 = (trochanterToe - trochanterFloorOnFlexion) / 100.0;

		//1 get data
                List<Jump> jump_l = SqliteJump.SelectJumpsWeightFVProfile (personID, sessionID, onlyBestInWeight);

		//2 convert to list of PointF
		point_l = new List<PointF>();
		point_l_relative = new List<PointF>();
                foreach(Jump j in jump_l)
		{
			double jumpHeightM = Util.GetHeightInMeters(j.Tv);

			// 1 calcule force
			//Samozino formula is F = m*g*( (h/hp0) +1)
			//h is jump's height
			//hp0 = trochanterToe - trochanterFloorOnFlexion
			double force = (personWeight + (personWeight * j.WeightPercent / 100.0)) * 9.81 * ( ( jumpHeightM / hp0 ) + 1 );
			//use force relative
			//force /= personWeight;
			//not because affects z and other calculations, do it later, just on the cairo graph

			// 2 create point
			PointF p = new PointF(Util.GetAverageImpulsionSpeed (jumpHeightM), force);

			// 3 add informational height and extra weight
			List<KeyDouble> lkd = new List<KeyDouble>();
			lkd.Add(new KeyDouble("Height (cm)", jumpHeightM * 100.0));
			lkd.Add(new KeyDouble("Extra weight (Kg)", personWeight * j.WeightPercent / 100.0));
			p.l_keydouble = lkd;

			//4 add to point_l
			point_l.Add(p);

			//5 add to point_l_relative
			p = new PointF(Util.GetAverageImpulsionSpeed (jumpHeightM), force / personWeight);
			p.l_keydouble = lkd;
			point_l_relative.Add(p);
		}

		//3 get LeastSquaresLine (straight line)
		ls = new LeastSquaresLine();
		ls.Calculate(point_l);

		if(ls.SlopeIsNaN())
		{
			LogB.Information("Slope is NaN");
			NeedMoreXData = true;
			return;
		}

		NeedMoreXData = false;

		LogB.Information(string.Format("Slope (sfv): {0}", Math.Round(Slope,2)));
		//LogB.Information(string.Format("Z: {0}", Math.Round(z(),2)));

		z = calculateZ();
		sfvOpt = calculateSfvOpt();
		LogB.Information(string.Format("sfvOpt: {0}", Math.Round(sfvOpt, 2)));

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

	//just to be shorter on below formulas
	private double pow(double x, double y)
	{
		return Math.Pow(x, y);
	}

	//TODO: to be calculated on constructor as private methods, and then accessed out if needed with public accessors

	//TODO: check values that need to be > 0...
	//Optimal Force–Velocity Profile in Ballistic Movements—Altius: Citius or Fortius? (Appendix)
	private double calculateZ()
	{
		/*
		LogB.Information(string.Format("z row 1: {0}", - (pow(g,6) * pow(hp0,6)) ));
		LogB.Information(string.Format("z row 2: {0}", - (18 * pow(g,3) * pow(hp0,5) * pow(PmaxRel,2)) ));
		LogB.Information(string.Format("z row 3: {0}", - (54 * pow(hp0,4) * pow(PmaxRel,4)) ));
		LogB.Information(string.Format("z row 4: {0}", 6 * Math.Sqrt(3) * Math.Sqrt( (2 * pow(g,3) * pow(hp0,9) * pow(PmaxRel,6)) + (27 * pow(hp0,8) * pow(PmaxRel,8)) ) ));
		LogB.Information(string.Format("z operation: {0}",
				- (pow(g,6) * pow(hp0,6))
				- (18 * pow(g,3) * pow(hp0,5) * pow(PmaxRel,2))
				- (54 * pow(hp0,4) * pow(PmaxRel,4))
				+ ( 6 * Math.Sqrt(3) * Math.Sqrt( (2 * pow(g,3) * pow(hp0,9) * pow(PmaxRel,6)) + (27 * pow(hp0,8) * pow(PmaxRel,8)) ) ) ));

		LogB.Information(string.Format("prova: {0}", MathUtil.Powfix(-1547882.26541462, 1/3.0)));
		*/

		/*
		 * Z(Pmax, hp0) = (
		 * 	- (g^6) * hp0^6
		 * 	- 18 * g^3 * hp0^5 * PmaxRel^2
		 * 	- 54 * hp0^4 * PmaxRel^4
		 * 	+ 6 * sqrt(3) * sqrt( (2 * g^3 * hp0^9 * pmax^6) + (27 * hp0^8 * PmaxRel^8) )
		 * 	) / 3
		 */
		return ( MathUtil.Powfix (
					- (pow(g,6) * pow(hp0,6))
					- (18 * pow(g,3) * pow(hp0,5) * pow(PmaxRel,2))
					- (54 * pow(hp0,4) * pow(PmaxRel,4))
					+ ( 6 * Math.Sqrt(3) * Math.Sqrt( (2 * pow(g,3) * pow(hp0,9) * pow(PmaxRel,6)) + (27 * pow(hp0,8) * pow(PmaxRel,8)) ) )
					, 1/3.0 ) );
	}

	//TODO: check values that need to be > 0...
	//Optimal Force–Velocity Profile in Ballistic Movements—Altius: Citius or Fortius? (Appendix)
	//private double sfvOpt()

	/*
	 * note the formula was not ok on the original paper but it was ok on the spreadsheet. It has been commented to authors and corrected on the original paper. See:
	 * https://www.researchgate.net/publication/320146284_JUMP_FVP_profile_spreadsheet/comments
	 */
	private double calculateSfvOpt()
	{
		/*
		LogB.Information(string.Format("sfvOpt row 1: {0}", - (pow(g,2) / (3.0 * PmaxRel)) ));
		LogB.Information(string.Format("sfvOpt row 2: {0}", - ( ( ((pow(g,4)) * pow(hp0,4)) - (12 * g * pow(hp0,3) * pow(PmaxRel,2)) ) / ( 3.0 * pow(hp0,2) * PmaxRel * z() ) ) ));
		LogB.Information(string.Format("sfvOpt row 2 num: {0}", - ( (-pow(g,4) * pow(hp0,4)) - (12 * g * pow(hp0,3) * pow(PmaxRel,2)) ) ));
		LogB.Information(string.Format("sfvOpt row 2 den: {0}", 3.0 * pow(hp0,2) * PmaxRel * z ));
		LogB.Information(string.Format("sfvOpt row 3: {0}", + (z / (3.0 * pow(hp0,2) * PmaxRel)) ));
		*/
		/*
		 * SFVopt =
		 * 	- g^2 / (3 * PmaxRel)
		 * 	- ( ( -(g^4) * hp0^4 - (12 * g * hp0^3 * PmaxRel^2) ) / ( 3 * hp0^2 * PmaxRel * Z(PmaxRel,hp0) ) )
		 * 	+ Z(Pmax,hp0) / (3 * hp0^2 * PmaxRel)
		 */
		return
			- (pow(g,2) / (3.0 * PmaxRel))
			- ( ( (- pow(g,4) * pow(hp0,4)) - (12 * g * pow(hp0,3) * pow(PmaxRel,2)) ) / ( 3.0 * pow(hp0,2) * PmaxRel * z ) )
			+ (z / (3.0 * pow(hp0,2) * PmaxRel));
	}


	//similar to imbalance, see spreadsheet at:
	//https://www.researchgate.net/publication/320146284_JUMP_FVP_profile_spreadsheet
	public double FvProfileFor90()
	{
		return 100 * SfvRel / sfvOpt;
	}

	public bool NeedDevelopForce()
	{
		return (FvProfileFor90() < 100);
	}

	//TODO: check values that need to be > 0...
	//Optimal Force–Velocity Profile in Ballistic Movements—Altius: Citius or Fortius? (Appendix)
	public int Imbalance()
	{
		// Fvimb = 100 * | 1- (SfvRel / Sfvopt) |
		return Convert.ToInt32(100 * Math.Abs( 1 - (SfvRel / sfvOpt) ));
	}

	public List<PointF> Point_l
	{
		get { return point_l; }
	}
	public List<PointF> Point_l_relative
	{
		get { return point_l_relative; }
	}

	//Slope is Sfv
	public double Slope
	{
		get { return ls.Slope; }
	}
	//Slope is Sfv
	public double Sfv
	{
		get { return ls.Slope; }
	}
	//Slope is Sfv
	public double SfvRel
	{
		get { return ls.Slope / personWeight; }
	}

	public double Intercept
	{
		get { return ls.Intercept; }
	}
	//f0 = intercept
	public double F0
	{
		get { return ls.Intercept; }
	}
	public double F0Rel
	{
		get { return F0 / personWeight; }
	}

	public double V0
	{
		get { return - F0 / Slope; }
	}

	public double Pmax
	{
		get { return (F0 * V0) / 4.0; }
	}

	//relative to weight
	public double PmaxRel
	{
		get { return (F0 * V0) / 4.0 / personWeight; }
	}

	public double Hp0
	{
		get { return hp0; }
	}

	//to debug
	public double Z
	{
		get { return z; }
	}
	//to debug
	public double SfvOpt
	{
		get { return sfvOpt; }
	}

	//to draw the optimum line
	public double F0Opt
	{
		get { return 2 * Math.Sqrt(- PmaxRel * sfvOpt); }
	}
	public double V0Opt
	{
		get { return 4 * PmaxRel / F0Opt; }
	}

	public double PersonWeight
	{
		get { return personWeight; }
	}
}
