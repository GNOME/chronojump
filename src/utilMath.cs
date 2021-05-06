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
 *  Copyright (C) 2016, 2019   Xavier Padullés <x.padulles@gmail.com>
 *  Copyright (C) 2016-2017, 2019-2020   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.IO; 		//for detect OS //TextWriter
using System.Collections.Generic; //List<T>

//note this has doubles. For ints can use Gdk.Point
public class PointF
{
	private double x;
	private double y;

	//used sometimes to have more info of each point
	public List<KeyDouble> l_keydouble;

	public PointF(double x, double y)
	{
		this.x = x;
		this.y = y;

		l_keydouble = new List<KeyDouble>();
	}

	public double X {
		get { return x; }
	}
	
	public double Y {
		get { return y; }
	}

	public override string ToString()
	{
		//return string.Format("X:{0}, Y:{1}", x, y);
		string str = string.Format("X:{0}, Y:{1}", x, y);
		string sep = "";
		foreach(KeyDouble kd in l_keydouble)
		{
			str += sep + kd.ToString();
			sep = "\t";
		}
		return str;
	}
}

public class KeyDouble
{
	private string key;
	private double d;

	public KeyDouble (string key, double d)
	{
		this.key = key;
		this.d = d;
	}

	public string Key {
		get { return key; }
	}

	public double D {
		get { return d; }
	}

	public override string ToString()
	{
		return string.Format("key:{0}, double:{1}", key, d);
	}
}

//like Point but for having an xStart and xEnd
public class PointStartEnd
{
	private int id;
	private double start;
	private double end;

	public PointStartEnd (int id, double start, double end)
	{
		this.id = id;
		this.start = start;
		this.end = end;
	}

	public int Id {
		get { return id; }
	}
	public double Start {
		get { return start; }
	}
	public double End {
		get { return end; }
	}

	public override string ToString()
	{
		return string.Format("Id:{0}; Start:{1}; End:{2}", id, start, end);
	}

}

public class LeastSquaresLine
{
	//public double [] Coef; 	//indep, x
	public double Slope;
	public double Intercept;

	public LeastSquaresLine() {
		Slope = 0;
		Intercept = 0;
	}

	public void Test()
	{
		List<PointF> measures = new List<PointF> {
			new PointF(1, 10.3214), new PointF(2, 13.3214), new PointF(3, 18.3214) };
		Calculate(measures);
	}

	public void Calculate(List<PointF> measures)
	{
		int n = measures.Count;
		double sumX = 0; //sumatory of the X values
		double sumY = 0; //sumatory of the Y values
		double sumX2 = 0; //sumatory of the squared X values
		double sumXY = 0; //sumatory of the squared X values

		//for(int i = 0; i < numMeasures; i++)
		foreach(PointF p in measures)
		{
			sumX = sumX + p.X;
			sumY = sumY + p.Y;
			sumX2 = sumX2 + p.X * p.X;
			sumXY = sumXY + p.X * p.Y;
		}

		Slope = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
		Intercept = (sumX2 * sumY - sumX * sumXY) / (sumX2 * n - sumX * sumX);

		/*
		double [] yFit = new double[numMeasures];
		for (int i = 0; i < n; i++)
			yFit[i] = slope * x[i] + intercept;
			*/
	}

	public bool SlopeIsNaN()
	{
		return Double.IsNaN(Slope);
	}
}

public class LeastSquaresParabole 
{
	public bool CalculatedCoef;
	public double [] Coef; 	//indep, x, x^2

	/*
	 * R testing
	 * x=seq(from=min(x), to=max(x), length.out=20)
	 * plot(x, 11.51323 + 1.36524*x + -0.01752 * x^2)
	 */

	public bool CalculatedXatMaxY;
	public double XatMaxY;
	public enum ParaboleTypes { NOTCALCULATED, STRAIGHT, CONVEX, CONCAVE } //CONVEX is usually OK

	//constructor
	public LeastSquaresParabole() {
		Coef = null;
	}

	public void Test()
	{
		List<PointF> measures = new List<PointF> {
			new PointF(1, 10.3214), new PointF(2, 13.3214), new PointF(3, 18.3214),
			    new PointF(4, 25.3214), new PointF(5, 34.3214), new PointF(6, 45.3214),
			    new PointF(7, 58.3214), new PointF(8, 73.3214), new PointF(9, 90.3214), new PointF(10, 109.3214) };

		//R testing
		//x=1:10
		//y=c(10.3214,13.3214,18.3214,25.3214,34.3214,45.3214,58.3214,73.3214,90.3214,109.3214)

		Calculate(measures);
	}

	public void Calculate(List<PointF> measures)
	{
		/*
		LogB.Information("printing points at Calculate");
		foreach(PointF p in measures)
			LogB.Information(p.ToString());
			*/

		int numMeasures = measures.Count;
		CalculatedCoef = false;
		CalculatedXatMaxY = false;

		if(numMeasures < 3) {
			LogB.Error(string.Format("LeastSquaresParabole needs at least three values, has: {0}", numMeasures));
			return;
		}

		double [] B = new double[3];
		for(int i = 0; i < numMeasures; i++){
			B[0] = B[0] + measures[i].Y;
			B[1] = B[1] + measures[i].X * measures[i].Y;
			B[2] = B[2] + measures[i].X * measures[i].X * measures[i].Y;
		}

		LogB.Information(string.Format("B = {0} {1} {2}", B[0], B[1], B[2]));

		double sumX = 0; //sumatory of the X values
		double sumX2 = 0; //sumatory of the squared X values
		double sumX3 = 0; //sumatory of the cubic X values
		double sumX4 = 0; //sumatory of the forth power of X values

		for(int i = 0; i < numMeasures; i++){
			sumX  = sumX  + measures[i].X;
			sumX2 = sumX2 + Math.Pow(measures[i].X,2);
			sumX3 = sumX3 + Math.Pow(measures[i].X,3);
			sumX4 = sumX4 + Math.Pow(measures[i].X,4);
		}
		//LogB.Information(string.Format("sumX: {0}; sumX2: {1}; sumX3: {2}; sumX4: {3}", sumX, sumX2, sumX3, sumX4));
		//LogB.Information(string.Format("A: {0}; B: {1}; C: {2}; D: {3}; E: {4}", numMeasures*sumX2*sumX4, 2*sumX*sumX2*sumX3, sumX2*sumX2*sumX2, sumX*sumX*sumX4, numMeasures*sumX3*sumX3));

		double detA = numMeasures*sumX2*sumX4 + 2*sumX*sumX2*sumX3 - sumX2*sumX2*sumX2 - sumX*sumX*sumX4 - numMeasures*sumX3*sumX3;
		if(detA != 0){
			double [,] invA = new double[3,3];

			invA[0,0] = ( sumX2*sumX4 - sumX3*sumX3) / detA;
			invA[0,1] = (-sumX *sumX4 + sumX2*sumX3) / detA;
			invA[0,2] = ( sumX *sumX3 - sumX2*sumX2) / detA;
			invA[1,1] = ( numMeasures*sumX4 - sumX2*sumX2 ) / detA;
			invA[1,2] = (-numMeasures*sumX3 + sumX *sumX2 ) / detA;
			invA[2,2] = ( numMeasures*sumX2 - sumX *sumX  ) / detA;

			//Simetric matrix
			invA[1,0] = invA[0,1];
			invA[2,0] = invA[0,2];
			invA[2,1] = invA[1,2];

			//coef = invA * B
			double [] coef = new double[3];
			coef[0] = invA[0,0]*B[0] + invA[0,1]*B[1] + invA[0,2]*B[2];
			coef[1] = invA[1,0]*B[0] + invA[1,1]*B[1] + invA[1,2]*B[2];
			coef[2] = invA[2,0]*B[0] + invA[2,1]*B[1] + invA[2,2]*B[2];

			Coef = coef;
			CalculatedCoef = true;

			calculateXAtMaxY();
		} else {
			//note determinant == 0 happens on this X data: (2019.11111111111, 2019.78333333333, 2020.025)
			//but subtracting all by the first it works: (0.0000000, 0.6722222, 0.9138889)
			LogB.Error("Determinant of matrix equal to zero");
		}
	}

	//this is the X where maxY is found
	private void calculateXAtMaxY()
	{
		if(Coef[2] == 0)
		{
			LogB.Error("Straight line");
			return;
		}
		else if(Coef[2] > 0)
		{
			LogB.Error("Inverted parabole, also solve division by zero problems");
			return;
		}

		//XatMaxY = -b / 2a
		XatMaxY = - Coef[1] / (2 * Coef[2]);
		CalculatedXatMaxY = true;
	}

	public ParaboleTypes ParaboleType
	{
		get {
			if(! CalculatedCoef)
				return ParaboleTypes.NOTCALCULATED;

			if(Coef[2] == 0)
				return ParaboleTypes.STRAIGHT;

			if(Coef[2] > 0)
				return ParaboleTypes.CONCAVE;

			return ParaboleTypes.CONVEX;
		}
	}
}

public static class MathCJ
{
	public static double ToRadians(double angdeg)
	{
		return angdeg / 180 * Math.PI;
	}
}

public static class MathUtil
{
	/// <summary>
	/// Wrapper for Math.Pow()
	/// Can handle cases like (-8)^(1/3) or  (-1/64)^(1/3)
	/// https://stackoverflow.com/a/14510824/12366369
	/// (used for jumpsWeightFVProfile.cs where need to pow a negative value to a 1/3)
	/// </summary>
	public static double Powfix (double expBase, double power)
	{
		bool sign = (expBase < 0);
		if (sign && HasEvenDenominator(power))
			return double.NaN;  //sqrt(-1) = i
		else
		{
			if (sign && HasOddDenominator(power))
				return -1 * Math.Pow(Math.Abs(expBase), power);
			else
				return Math.Pow(expBase, power);
		}
	}

	private static bool HasEvenDenominator(double input)
	{
		if(input == 0)
			return false;
		else if (input % 1 == 0)
			return false;

		double inverse = 1 / input;
		if (inverse % 2 < double.Epsilon)
			return true;
		else
			return false;
	}

	private static bool HasOddDenominator(double input)
	{
		if (input == 0)
			return false;
		else if (input % 1 == 0)
			return false;

		double inverse = 1 / input;
		if ((inverse + 1) % 2 < double.Epsilon)
			return true;
		else
			return false;
	}

	/*
	public static double ClosestNumber(double num1, double num2, double numToCompare)
	{
		LogB.Information(string.Format("Compare: {0} and {1} with: {2}", num1, num2, numToCompare));
		if( Math.Abs(num1 - numToCompare) <= Math.Abs(num2 - numToCompare) )
			return num1;

		return num2;
	}
	*/
	public static bool PassedSampleIsCloserToCriteria (
			double criteriaPassedValue, double previousToCriteriaValue, double numToCompare)
	{
		LogB.Information(string.Format("Compare: {0} and {1} with: {2}", criteriaPassedValue, previousToCriteriaValue, numToCompare));
		return ( Math.Abs(criteriaPassedValue - numToCompare) <= Math.Abs(previousToCriteriaValue - numToCompare) );
	}

}

public class InterpolateSignal
{
	private List<PointF> point_l;
	private enum types { COSINE, CUBIC };

	public InterpolateSignal (List<PointF> point_l)
	{
		this.point_l = point_l;
	}

	//thanks to: http://paulbourke.net/miscellaneous/interpolation/
	public double CosineInterpolate(double y1, double y2, double mu)
	{
		double mu2;

		mu2 = (1-Math.Cos(mu*Math.PI))/2;

		return(y1*(1-mu2)+y2*mu2);
	}
	//thanks to: http://paulbourke.net/miscellaneous/interpolation/
	public double CubicInterpolate(double y0,double y1, double y2,double y3, double mu)
	{
		double a0, a1, a2, a3, mu2;

		mu2 = mu * mu;
		a0 = y3 - y2 - y0 + y1;
		a1 = y0 - y1 - a0;
		a2 = y2 - y0;
		a3 = y1;

		return(a0*mu*mu2+a1*mu2+a2*mu+a3);
	}

	public static void TestCosineAndCubicInterpolate()
	{
		Random random = new Random();
		List<PointF> l = new List<PointF>();

		for(int i = 0; i < 100; i += 10)
			l.Add(new PointF(i, random.NextDouble() * 10));

		InterpolateSignal fsp = new InterpolateSignal(l);
		fsp.testCosineCubicInterpolateDo(types.COSINE);
		fsp.testCosineCubicInterpolateDo(types.CUBIC);
	}

	private void testCosineCubicInterpolateDo(types type)
	{
		TextWriter writer = File.CreateText(
				Path.Combine(Path.GetTempPath(), string.Format("chronojump_testinterpolate_{0}.csv", type.ToString())));
		writer.WriteLine("X;Y;color;cex");

		for(int i = 0; i < point_l.Count -1; i ++) //each known point
		{
			writer.WriteLine(string.Format("{0};{1};{2};{3}",
						point_l[i].X, point_l[i].Y, "red", 2));
			for(double d = 0; d < 1 ; d += .1) //each interpolated value
			{
				if (type == types.COSINE)
					writer.WriteLine(string.Format("{0};{1};{2};{3}",
								point_l[i].X + d*(point_l[i+1].X - point_l[i].X),
								CosineInterpolate(point_l[i].Y, point_l[i+1].Y, d),
								"black", 1));
				//else if(type == types.CUBIC && i > 0 && i < point_l.Count -2)
				else if(type == types.CUBIC)
				{
					//for cubic we need two extra points
					double pre2Y = 0;
					double post2Y = 0;
					if(i == 0) {
						pre2Y = point_l[point_l.Count -1].Y;
						post2Y = point_l[i+2].Y;
					} else if(i == point_l.Count -2) {
						pre2Y = point_l[i-1].Y;
						post2Y = point_l[0].Y;
					} else {
						pre2Y = point_l[i-1].Y;
						post2Y = point_l[i+2].Y;
					}

					writer.WriteLine(string.Format("{0};{1};{2};{3}",
								point_l[i].X + d*(point_l[i+1].X - point_l[i].X),
								//CubicInterpolate(point_l[i-1].Y, point_l[i].Y, point_l[i+1].Y, point_l[i+2].Y, d),
								CubicInterpolate(pre2Y, point_l[i].Y, point_l[i+1].Y, post2Y, d),
								"green", 1));
				}
			}
		}
		//write the last known point
		writer.WriteLine(string.Format("{0};{1};{2};{3}",
					point_l[point_l.Count -1].X, point_l[point_l.Count -1].Y, "red", 2));

		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();

		/*
		   test it with R:
		   d=read.csv2("/tmp/chronojump_testinterpolate_COSINE.csv")
		   plot(d$X, d$Y, col=d$color, cex=d$cex, type="b", xlim=c(0,90), ylim=c(0,10))
		   par(new=T)
		   d=read.csv2("/tmp/chronojump_testinterpolate_CUBIC.csv")
		   plot(d$X, d$Y, col=d$color, cex=d$cex, type="b", xlim=c(0,90), ylim=c(0,10))
		   */
	}

	/* unused
	//n: number of points between should be at least 1
	public List<PointF> InterpolateBetween (int posFrom, int posTo, int n)
	{
		PointF pointFrom = point_l[posFrom];
		PointF pointTo = point_l[posTo];

		List<PointF> interpolated_l = new List<PointF>();
		for(int i = 1; i < n + 1; i ++)
			interpolated_l.Add(
					new PointF(
						pointFrom.X + UtilAll.DivideSafe((pointTo.X - pointFrom.X) * i, (n + 1)),
						pointFrom.Y + UtilAll.DivideSafe((pointTo.Y - pointFrom.Y) * i, (n + 1))
						));

		return interpolated_l;
	}

	public static void TestInterpolateBetween()
	{
		InterpolateSignal fsp = new InterpolateSignal(new List<PointF>{
				new PointF(5.5, 8),
				new PointF(7.5, 12),
				new PointF(14, 13.2)
				});

		//List<PointF> interpolated_l = fsp.InterpolateBetween(0, 1, 3);
		//List<PointF> interpolated_l = fsp.InterpolateBetween(1, 2, 1);
		List<PointF> interpolated_l = fsp.InterpolateBetween(1, 2, 7);

		//print list
		LogB.Information("Printing interpolates");
		foreach(PointF point in interpolated_l)
			LogB.Information(point.ToString());
	}
	*/

}
