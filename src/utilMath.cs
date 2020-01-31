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
 *  Copyright (C) 2016-2017, 2019   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List<T>

//note this has doubles. For ints can use Gdk.Point
public class PointF
{
	private double x;
	private double y;

	public PointF(double x, double y)
	{
		this.x = x;
		this.y = y;
	}

	public double X {
		get { return x; }
	}
	
	public double Y {
		get { return y; }
	}

	public override string ToString()
	{
		return string.Format("X:{0}, Y:{1}", x, y);
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
