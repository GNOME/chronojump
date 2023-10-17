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
 *  Copyright (C) 2016-2017, 2019-2023   Xavier de Blas <xaviblas@gmail.com>
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
		set { x = value; }
	}
	
	public double Y {
		get { return y; }
	}

	public PointF Transpose ()
	{
		return new PointF (y, x);
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

	public static bool ListsTimeOverlap (List<PointF> p1_l, List<PointF> p2_l, int p2Start, int p2End)
	{
		if (p1_l[0].X > p2_l[p2End].X)
			return false;
		else if (Last (p1_l).X < p2_l[p2Start].X)
			return false;

		return true;
	}

	public static int FindSampleCloseToTime (List<PointF> p_l, double searchTime)
	{
		int sample = 0;
		for (int i = 0; i < p_l.Count && p_l[i].X <= searchTime; i ++)
			sample = i;

		if (sample +1 >= p_l.Count)
			return sample;

		if (searchTime - p_l[sample].X < p_l[sample +1].X - searchTime)
			return sample;
		else
			return sample + 1;
	}

	//for finding wich sample is at selected time from the end
	public static int FindSampleAtTimeToEnd (List<PointF> p_l, double timeToEnd)
	{
		if (p_l.Count < 2)
			return 0;

		double searchTime = p_l[p_l.Count -1].X - timeToEnd;
		if (searchTime <= 0)
			return 0;

		int sample = 0;
		for (int i = p_l.Count -1; i >= 0; i --)
		{
			if (p_l[i].X <= searchTime)
			{
				sample = i;
				break;
			}
		}

		if (sample +1 >= p_l.Count)
			return sample;

		if (searchTime - p_l[sample].X < p_l[sample +1].X - searchTime)
			return sample;
		else
			return sample + 1;
	}

	//same as above but with date on Y
	public static int FindSampleAtTimeToEndDateY (List<PointF> p_l, double timeToEnd)
	{
		if (p_l.Count < 2)
			return 0;

		double searchTime = p_l[p_l.Count -1].Y - timeToEnd;
		if (searchTime <= 0)
			return 0;

		int sample = 0;
		for (int i = p_l.Count -1; i >= 0; i --)
		{
			if (p_l[i].Y <= searchTime)
			{
				sample = i;
				break;
			}
		}

		if (sample +1 >= p_l.Count)
			return sample;

		if (searchTime - p_l[sample].Y < p_l[sample +1].Y - searchTime)
			return sample;
		else
			return sample + 1;
	}

	//if want to use sublist just call also below method GetSubList ()
	public static double GetMaxY (List<PointF> p_l)
	{
		double maxY = 0;
		if (p_l == null || p_l.Count == 0)
			return maxY;

		for (int i = 0; i < p_l.Count ; i ++)
			if (i == 0 || p_l[i].Y > maxY)
				maxY = p_l[i].Y;

		return maxY;
	}

	public static double GetMinY (List<PointF> p_l)
	{
		double minY = 0;
		if (p_l == null || p_l.Count == 0)
			return minY;

		for (int i = 0; i < p_l.Count ; i ++)
			if (i == 0 || p_l[i].Y < minY)
				minY = p_l[i].Y;

		return minY;
	}

	//if want to use sublist just call also below method GetSubList ()
	public static double GetAvgY (List<PointF> p_l)
	{
		if (p_l == null || p_l.Count == 0)
			return 0;

		double sum = 0;
		for (int i = 0; i < p_l.Count ; i ++)
			sum += p_l[i].Y;

		return UtilAll.DivideSafe (sum, p_l.Count);
	}

	//just to debug
	public static string PrintList (string title, List<PointF> p_l, string sep)
	{
		string str = "";
		if (title != "")
			str += title + "\n";

		string sepDo = "";
		foreach (PointF p in p_l)
		{
			str += sepDo + p.ToString ();
			sepDo = sep;
		}

		return str;
	}

	public static List<PointF> ShiftX (List<PointF> p_l, double xShift)
	{
		for (int i = 0; i < p_l.Count; i ++)
			p_l[i].X += xShift;

		return p_l;
	}

	public static PointF Last (List<PointF> p_l)
	{
		if (p_l.Count == 0)
			return null;

		return p_l[p_l.Count -1];
	}

	public static List<PointF> GetSubList (List<PointF> orig_l, int start, int end)
	{
		// if passed -1, return orig_l
		if (start < 0 && end < 0)
			return orig_l;

		// if end is before start then change them
		if (end < start)
		{
			int temp = end;
			end = start;
			start = temp;
		}

		// if start || end are beyond list, return orig_l
		if (start >= orig_l.Count || end >= orig_l.Count)
			return orig_l;

		List<PointF> p_l = new List <PointF> ();
		for (int i = start; i < end; i ++)
			p_l.Add (orig_l[i]);

		return p_l;
	}
}

//nice to have an X, Y and a value (Z)
public class Point3F
{
	private double x;
	private double y;
	private double z;

	public Point3F (double x, double y, double z)
	{
		this.x = x;
		this.y = y;
		this.z = z;
	}

	public double X {
		get { return x; }
	}
	public double Y {
		get { return y; }
	}
	public double Z {
		get { return z; }
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

public class TwoListsOfInts
{
	private string firstIntName; //just to be able to debug
	private string secondIntName; //same

	private List<int> first_l;
	private List<int> second_l;

	public TwoListsOfInts (string firstIntName, string secondIntName)
	{
		this.firstIntName = firstIntName;
		this.secondIntName = secondIntName;

		Reset();
	}

	public void Reset()
	{
		first_l = new List<int>();
		second_l = new List<int>();
	}

	public bool HasData()
	{
		return (first_l != null && first_l.Count > 0);
	}

	public int Count()
	{
		return first_l.Count;
	}

	public void Add (int addToFirst, int addToSecond)
	{
		first_l.Add(addToFirst);
		second_l.Add(addToSecond);
	}

	public int GetFromFirst(int pos)
	{
		return first_l[pos];
	}
	public int GetFromSecond(int pos)
	{
		return second_l[pos];
	}

	public override string ToString()
	{
		return (string.Format("firstIntName: {0}, secondIntName: {1}",
					firstIntName, secondIntName)); // just to debug, add the values
	}

}

public class TwoListsOfDoubles
{
	private List<double> first_l;
	private List<double> second_l;

	public TwoListsOfDoubles ()
	{
		Reset();
	}

	public void Reset()
	{
		first_l = new List<double>();
		second_l = new List<double>();
	}

	public bool HasData()
	{
		return (first_l != null && first_l.Count > 0);
	}

	public int Count()
	{
		return first_l.Count;
	}

	public void Add (double addToFirst, double addToSecond)
	{
		first_l.Add(addToFirst);
		second_l.Add(addToSecond);
	}

	public double GetFromFirst(int pos)
	{
		return first_l[pos];
	}
	public double GetFromSecond(int pos)
	{
		return second_l[pos];
	}
}

//like Point but for having an xStart and xEnd
public class PointInRectangle
{
	private int id;
	private double startX;
	private double startY;
	private double endX;
	private double endY;

	public PointInRectangle (int id, double startX, double startY, double endX, double endY)
	{
		this.id = id;
		this.startX = startX;
		this.startY = startY;
		this.endX = endX;
		this.endY = endY;
	}

	public int Id {
		get { return id; }
	}
	public double StartX {
		get { return startX; }
	}
	public double StartY {
		get { return startY; }
	}
	public double EndX {
		get { return endX; }
	}
	public double EndY {
		get { return endY; }
	}

	public override string ToString()
	{
		return string.Format("Id:{0}; StartX:{1}; StartY:{2}, EndX:{3}, EndY:{4}", id, startX, startY, endX, endY);
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

	public double CalculateYAtSomeX (double x)
	{
		//check first CalculatedCoef and Parabole type if needed
		return Coef[2] * Math.Pow (x, 2) + Coef[1] * x + Coef[0];
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

public class MovingAverage
{
	//passed params
	private List<PointF> points_l;
	private int oddSampleWindow; //if 5, two values pre, 1 current, two post

	private int samplesAtEachSide;
	private List<PointF> movingAverage_l; //calculated list

	public MovingAverage (List<PointF> points_l, int oddSampleWindow)
	{
		this.points_l = points_l;
		this.oddSampleWindow = oddSampleWindow;

		samplesAtEachSide = (int) Math.Floor(Convert.ToDouble(oddSampleWindow / 2));
		movingAverage_l = new List<PointF>();
	}

	//calculate movingAverage_l
	public bool Calculate ()
	{
		if(oddSampleWindow > points_l.Count)
			return false;

		double yCount = 0;
		bool firstTime = true;
		for(int i = samplesAtEachSide; i < points_l.Count - samplesAtEachSide; i ++)
		{
			if(firstTime)
			{
				for(int j = i - samplesAtEachSide; j <= i + samplesAtEachSide; j ++)
					yCount += points_l[j].Y;
				firstTime = false;
			} else {
				yCount -= points_l[i-samplesAtEachSide-1].Y;
				yCount += points_l[i+samplesAtEachSide].Y;
			}

			PointF movingA = new PointF(points_l[i].X, 1.0 * yCount / oddSampleWindow);
			movingAverage_l.Add(movingA);
		}
		return true;
	}

	//returns the PointF (so the X and Y of max(y))
	//if there are two with the same Y, gets the first because of:  if(p.Y > pmax.Y)
	public PointF GetMaxY ()
	{
		if(movingAverage_l == null || movingAverage_l.Count == 0)
			return new PointF(0,0);

		PointF pmax = new PointF(0,0);
		foreach(PointF p in movingAverage_l)
			if(p.Y > pmax.Y)
				pmax = p;

		return pmax;
	}

	//to debug
	public string GetMovingAverageList()
	{
		string str = "MovingAverage calculated list:\n";
		foreach(PointF p in movingAverage_l)
			str += p.ToString() + "\n";

		return str;
	}

	public static void TestCalculate()
	{
		List<PointF> test_l = new List<PointF>{
				new PointF(5.5, 8),
				new PointF(7.5, 12),
				new PointF(14, 13.2),
				new PointF(17, 13.8),
				new PointF(19, 14.15),
				new PointF(21.1, 13.9),
				new PointF(22.4, 11.5),
				new PointF(27.5, 8.3)
				};

		MovingAverage ma = new MovingAverage(test_l, 5);
		ma.Calculate();
		LogB.Information(ma.GetMovingAverageList());

		//tested with LibreOffice Calc, works as expected
	}

	//the smoothed list
	public List<PointF> MovingAverage_l {
		get { return movingAverage_l; }
	}
	public List<double> MovingAverage_l_Y {
		get {
			List<double> d_l = new List<double> ();
			foreach (PointF p in movingAverage_l)
				d_l.Add (p.Y);

			return d_l;
		}
	}
}

public class VariabilityAndAccuracy
{
	private	List<PointF> p_l;

	private double variability;
	private double feedbackDiff;

	public VariabilityAndAccuracy ()
	{
	}

	public void Calculate (List<PointF> p_l, int countA, int countB,
			int feedbackF, Preferences.VariabilityMethodEnum variabilityMethod, int lag)
	{
		if(countA == countB)
		{
			variability = 0;
			feedbackDiff = 0;
			return;
		}

		this.p_l = p_l;

		// 1) calculate numSamples. Note countA and countB are included, so
		//countA = 2; countB = 4; samples are: 2,3,4; 3 samples
		int numSamples = (countB - countA) + 1;

		// 2) get variability
		if(variabilityMethod == Preferences.VariabilityMethodEnum.CHRONOJUMP_OLD)
			variability = getVariabilityOldMethod (countA, countB, numSamples);
		else if(variabilityMethod == Preferences.VariabilityMethodEnum.CV)
			variability = getVariabilityCV (countA, countB, numSamples);
		else
			variability = getVariabilityRMSSDCVRMSSD (variabilityMethod, lag, countA, countB, numSamples);

		// 3) Calculate difference.
		// Average of the differences between force and average
		//feedbackDiff = Math.Abs(feedbackF - avg);
		double sum = 0;
		for(int i = countA; i <= countB; i ++)
			sum += Math.Abs (p_l[i].Y - feedbackF);

		feedbackDiff = UtilAll.DivideSafe (sum, numSamples);
	}

	private double getVariabilityRMSSDCVRMSSD (
			Preferences.VariabilityMethodEnum method, int lag,
			int countA, int countB, int numSamples)
	{
		//see a test of this method below:
		//public static void TestVariabilityCVRMSSD()

		//sqrt(Σ( x_i - x_{i+1})^2 /(n-1)) )   //note pow should be inside the summation
		double sum = 0;
		double sumForMean = 0;
		for(int i = countA; i+lag <= countB; i ++)
		{
			sum += Math.Pow(p_l[i].Y - p_l[i+lag].Y, 2);
			sumForMean += p_l[i].Y;
		}

		double rmssd = Math.Sqrt (UtilAll.DivideSafe (sum, numSamples -lag));
		LogB.Information("RMSSD: " + rmssd.ToString());

		if(method == Preferences.VariabilityMethodEnum.RMSSD)
			return rmssd;

		//sumForMean += forces[countB]; //need this?
		double mean = sumForMean / numSamples;

		return 100 * UtilAll.DivideSafe (rmssd, mean);
	}

	private double getVariabilityCV (int countA, int countB, int numSamples)
	{
		// 1) get average
		double sum = 0;
		for(int i = countA; i <= countB; i ++)
			sum += p_l[i].Y;

		double avg = sum / numSamples;

		// 2) calculate SD
		sum = 0;
		for (int i = countA; i <= countB; i ++)
			sum += Math.Abs (Math.Pow (p_l[i].Y - avg, 2));
		double sd = Math.Sqrt (UtilAll.DivideSafe (sum, numSamples -1)); //-1 because is sample and not population sd

		return 100 * UtilAll.DivideSafe (sd, avg);
	}

	private double getVariabilityOldMethod (int countA, int countB, int numSamples)
	{
		// 1) get average
		double sum = 0;
		for(int i = countA; i <= countB; i ++)
			sum += p_l[i].Y;

		double avg = sum / numSamples;

		// 2) Average of the differences between force and average
		sum = 0;
		for (int i = countA; i <= countB; i ++)
			sum += Math.Abs (p_l[i].Y-avg);

		return UtilAll.DivideSafe (sum, numSamples);
	}

	private List<PointF> createTestData ()
	{
		List<PointF> pTest_l = new List<PointF> ();
		List <double> nums = new List<double> {21, 45, 75, 54, 5.5, 545.5, 44, 17, 8, -15, -12.8, -15.9, -11.5, -2, 5.3};
		for (int i = 0; i < nums.Count; i ++)
			pTest_l.Add (new PointF (i+1, nums[i]));

		return pTest_l;
	}

	public void TestVariabilityCVRMSSD (int lag)
	{
		/*
		   R psych test:
		   library("psych")
		   > x=c(21, 45, 75, 54, 5.5, 545.5, 44, 17, 8, -15, -12.8, -15.9, -11.5, -2, 5.3)
		   > rmssd(x,group=NULL, lag=1, na.rm=TRUE)
		   [1] 234.2467
		*/

		List<PointF> pTest_l = createTestData ();
		Calculate (pTest_l, 0, pTest_l.Count -1, 20, Preferences.VariabilityMethodEnum.CVRMSSD, lag);
		LogB.Information("cvRMSSD: " + variability);
	}

	public void TestVariabilityCV ()
	{
		/*
		   R test:
		   library("REAT")
		   > x <- c(21, 45, 75, 54, 5.5, 545.5, 44, 17, 8, -15, -12.8, -15.9, -11.5, -2, 5.3)
		   > 100 * cv (x)
		   [1] 274.4306
		*/

		List<PointF> pTest_l = createTestData ();
		Calculate (pTest_l, 0, pTest_l.Count -1, 0, Preferences.VariabilityMethodEnum.CV, 0);
		LogB.Information("cv: " + variability);
	}

	public void TestVariabilityRMSSDAndCVRMSSD ()
	{
		/* R code:

		library("psych")
		d=read.csv2("rmssd2.csv")
		png(width=1920, height=1080, filename="rmssd.png")
		y=d$Force..N.
		maxyStored = max(y)
		plot(y, ylim=c(0,maxyStored))
		#abline(v=seq(from=1, to=length(y), by=100))
		maxVariab = 0
		for(n in seq(from=1, to=(length(y) -100), by=100)) {
			variab = rmssd(y[n:(n+100)], group=NULL, lag=1, na.rm=TRUE)
			mtext(side=1, at=n+50, round(variab,3), line=0)
			if(variab > maxVariab)
				maxVariab = variab
		}
		for(n in seq(from=1, to=(length(y) -100), by=100)) {
			variab = rmssd(y[n:(n+100)], group=NULL, lag=1, na.rm=TRUE)
			variabScaled = max(y) * variab / maxVariab
			#rect(n, 0, n+100, variabScaled)
		}

		#try to lower the values
		y=y-100
		lines(y, col="blue")
		for(n in seq(from=1, to=(length(y) -100), by=100)) {
			variab = rmssd(y[n:(n+100)], group=NULL, lag=1, na.rm=TRUE)
			mtext(side=1, at=n+50, round(variab,3), col="blue", line=1)
		}
		for(n in seq(from=1, to=(length(y) -100), by=100)) {
			variab = rmssd(y[n:(n+100)], group=NULL, lag=1, na.rm=TRUE)
			variabScaled = maxyStored * variab / maxVariab
			#rect(n, 0, n+100, variabScaled, col="blue")
		}

		#cv rmssd
		y=d$Force..N.
		lines(y, col="green4")
		for(n in seq(from=1, to=(length(y) -100), by=100)) {
			variab = 100*rmssd(y[n:(n+100)], group=NULL, lag=1, na.rm=TRUE)/mean(y[n:(n+100)])
			mtext(side=1, at=n+50, round(variab,3), col="green4", line=2)
		}
		for(n in seq(from=1, to=(length(y) -100), by=100)) {
			variab = 100*rmssd(y[n:(n+100)], group=NULL, lag=1, na.rm=TRUE)/mean(y[n:(n+100)])
			variabScaled = maxyStored * variab / maxVariab
			#rect(n, 0, n+100, variabScaled, col="green4")
		}

		# y/2
		y=d$Force..N.
		y=y/2
		lines(y, col="red")
		for(n in seq(from=1, to=(length(y) -100), by=100)) {
			variab = rmssd(y[n:(n+100)], group=NULL, lag=1, na.rm=TRUE)
			mtext(side=1, at=n+50, round(variab,3), col="red", line=3)
		}
		for(n in seq(from=1, to=(length(y) -100), by=100)) {
			variab = rmssd(y[n:(n+100)], group=NULL, lag=1, na.rm=TRUE)
			variabScaled = maxyStored * variab / maxVariab
			#rect(n, 0, n+100, variabScaled, col="red")
		}
		dev.off()
		 */
	}

	public double Variability {
		get { return variability; }
	}

	public double FeedbackDiff {
		get { return feedbackDiff; }
	}
}

public abstract class GetMaxValueInWindow
{
	protected double max;
	protected int maxSampleStart;
	protected int maxSampleEnd;
	protected string error;

	protected List<PointF> p_l;
	protected int countA;
	protected int countB;
	protected double windowSeconds;

	protected bool parametersBad ()
	{
		return (p_l == null || countA < 0 || countB < 0 || countA >= p_l.Count || countB >= p_l.Count);
	}

	protected bool dataTooShort ()
	{
		return (p_l[countB].X - p_l[countA].X <= 1000000 * windowSeconds);
	}

	protected abstract void calculate ();

	//just to debug
	public override string ToString ()
	{
		return string.Format ("start: {0}, end: {1}, max: {2}, error: {3}",
				maxSampleStart, maxSampleEnd, max, error);
	}

	public double Max
	{
		get { return max; }
		set { max = value; }
	}

	public int MaxSampleStart
	{
		get { return maxSampleStart; }
		set { maxSampleStart = value; }
	}

	public int MaxSampleEnd
	{
		get { return maxSampleEnd; }
		set { maxSampleEnd = value; }
	}
	public string Error
	{
		get { return error; }
	}
}

// TODO: manage if X is micros or millis
public class GetMaxAvgInWindow : GetMaxValueInWindow
{
	//when there is no fsAI || gmaiw.Error (used on updateForceSensorAICairo)
	public GetMaxAvgInWindow ()
	{
		maxSampleStart = -1;
		maxSampleEnd = -1;
		max = -1;
		error = "null";
	}

	public GetMaxAvgInWindow (List<PointF> p_l, int countA, int countB, double windowSeconds)
	{
		this.p_l = p_l;
		this.countA = countA;
		this.countB = countB;
		this.windowSeconds = windowSeconds;

		LogB.Information ("GetMaxAvgInWindow start");

		/* TODO: manage this
		// 1) check if ws calculated before
		if(calculatedForceMaxAvgInWindow != null &&
		calculatedForceMaxAvgInWindow.InputsToString() ==
		new CalculatedForceMaxAvgInWindow(countA, countB, windowSeconds).InputsToString())
		{
		LogB.Information("Was calculated before");
		avgMax = calculatedForceMaxAvgInWindow.Result;
		avgMaxSampleStart = calculatedForceMaxAvgInWindow.ResultSampleStart;
		avgMaxSampleEnd = calculatedForceMaxAvgInWindow.ResultSampleEnd;
		error = ""; //there will be no error, because when is stored is without error
		return;
		}
		*/

		LogB.Information (string.Format ("p_l.Count: {0}, countA: {1}, countB: {2}, windowSeconds: {3}",
					p_l.Count, countA, countB, windowSeconds));

		if (parametersBad ())
		{
			error = string.Format ("p_l.Count: {0}, countA: {1}, countB: {2}, windowSeconds: {3}",
					p_l.Count, countA, countB, windowSeconds);
			return;
		}

		// 2) check if countB - countA fits in window time
		if (dataTooShort ())
		{
			max = 0;
			maxSampleStart = countA; //there is an error, this will not be used
			maxSampleEnd = countA; //there is an error, this will not be used
			error = "Need more time";
			return;
		}

		calculate ();

		/*
		// 5) store data to not calculate it again if data is the same
		calculatedForceMaxAvgInWindow = new CalculatedForceMaxAvgInWindow (
				countA, countB, windowSeconds, max, maxSampleStart, maxSampleEnd);
				*/
		LogB.Information ("GetMaxAvgInWindow done!");
	}

	protected override void calculate ()
	{
		max = 0;
		maxSampleStart = countA; 	//sample where avgMax starts (to draw a line)
		maxSampleEnd = countA; 	 	//sample where avgMax ends (to draw a line)
		error = "";

		double timeA = p_l[countA].X;
		double sum = 0;
		int count = 0;

		//note if countB - countA < 1s then can have higher values than all the set
		//TODO: can be more accurate with public static bool PassedSampleIsCloserToCriteria (check GetBestRFDInWindow)
		// 3) get the first second (or whatever in windowSeconds)
		int i;
		for(i = countA; i <= countB && p_l[i].X - timeA <= 1000000 * windowSeconds; i ++)
		{
			sum += p_l[i].Y;
			count ++;
		}
		max = sum / count;
		maxSampleEnd = countA + count;

		LogB.Information(string.Format("avgMax 1st for: {0}", max));
		//note "count" has the window size in samples

		// 4) continue until the end (countB)
		for(int j = i; j < countB; j ++)
		{
			sum -= p_l[j - count].Y;
			sum += p_l[j].Y;

			double avg = sum / count;
			if(avg > max)
			{
				max = avg;
				maxSampleStart = j - count;
				maxSampleEnd = j;
			}
		}

		LogB.Information(string.Format("Average max force in {0} seconds: {1}, started at sample range: {2}:{3}",
					windowSeconds, max, maxSampleStart, maxSampleEnd));
	}
}

public class GetBestRFDInWindow : GetMaxValueInWindow
{
	public GetBestRFDInWindow (List<PointF> p_l, int countA, int countB, double windowSeconds)
	{
		this.p_l = p_l;
		this.countA = countA;
		this.countB = countB;
		this.windowSeconds = windowSeconds;

		if (parametersBad ())
		{
			error = string.Format ("p_l.Count: {0}, countA: {1}, countB: {2}, windowSeconds: {3}",
					p_l.Count, countA, countB, windowSeconds);
			return;
		}

		// 2) check if countB - countA fits in window time
		if (dataTooShort ())
		{
			error = "Need more time";
			return;
		}

		calculate ();
	}

	protected override void calculate ()
	{
		max = 0;
		maxSampleStart = countA; 	//sample where best RFD starts (to draw a line)
		maxSampleEnd = countA; 		//sample where best RFD ends (to draw a line)
		error = "too short for calcule";

		for (int i = countA; i < countB; i ++)
		{
			int j = findSampleAtWindowSeconds (i);
			if (j < 0)
				return;

			error = ""; //not show the error mark
			double temp = ForceCalcs.GetRFD (p_l, j, i);
			if (temp > max)
			{
				max = temp;
				maxSampleStart = i;
				maxSampleEnd = j;
			}
		}
	}

	// return -1 if sampleEnd is out of array
	private int findSampleAtWindowSeconds (int sampleStart)
	{
		double timeStart = p_l[sampleStart].X;
		for (int i = sampleStart; i < p_l.Count; i ++)
			if (p_l[i].X - timeStart >= 1000000 * windowSeconds)
			{
				if (MathUtil.PassedSampleIsCloserToCriteria (
							p_l[i].X - timeStart,
							p_l[i-1].X - timeStart,
							1000000 * windowSeconds))
					return i-1;
				else
					return i;
			}

		return -1;
	}
}

/*
public class ForceCalculateRange
{
	private List<PointF> p_l;
	private int countA;
	private int countB;
	private double maxAVGInWindowSeconds;

	private double forceAVG;
	private double forceMAX;
	private GetMaxAvgInWindow gmaiw;

	public double SpeedAVG;
	public double SpeedMAX;
	public double AccelAVG;
	public double AccelMAX;
	public double PowerAVG;
	public double PowerMAX;

	public ForceCalculateRange (
			List<PointF> p_l,
			int countA, int countB, double maxAVGInWindowSeconds)
	{
		this.p_l = p_l;
		this.countA = countA;
		this.countB = countB;
		this.maxAVGInWindowSeconds = maxAVGInWindowSeconds;

		calcule ();
	}

	public ForceCalculateRange (
			List<PointF> p_l,
			List<PointF> speed_l, List<PointF> accel_l, List<PointF> power_l,
			int countA, int countB, double maxAVGInWindowSeconds)
	{
		this.p_l = p_l;
		this.countA = countA;
		this.countB = countB;
		this.maxAVGInWindowSeconds = maxAVGInWindowSeconds;

		calcule ();

		calculeElasticAvgAndMax (speed_l, countA, countB, out SpeedAVG, out SpeedMAX);
		calculeElasticAvgAndMax (accel_l, countA, countB, out AccelAVG, out AccelMAX);
		calculeElasticAvgAndMax (power_l, countA, countB, out PowerAVG, out PowerMAX);
	}

	private void calcule ()
	{
	*/
		/*
		 * countA will be the lowest and countB the highest
		 * to calcule Avg and max correctly no matter if B is before A
		 */
	/*
		if(countA > countB) {
			int temp = countA;
			countA = countB;
			countB = temp;
		}

		getAverageAndMaxForce ();
		gmaiw = new GetMaxAvgInWindow (p_l, countA, countB, maxAVGInWindowSeconds);
	}

	private void getAverageAndMaxForce ()
	{
		if(countA == countB) {
			forceAVG = p_l[countA].Y;
			forceMAX = p_l[countA].Y;
			return;
		}

		double sum = 0;
		forceMAX = -100000;
		for (int i = countA; i <= countB; i ++) {
			sum += p_l[i].Y;
			if (p_l[i].Y > forceMAX)
				forceMAX = p_l[i].Y;
		}

		forceAVG = sum / ((countB - countA) +1);
	}

	private void calculeElasticAvgAndMax (List<PointF> list, int countA, int countB,
			out double avg, out double max)
	{
		if(countA == countB) {
			avg = list[countA].Y;
			max = list[countA].Y;
			return;
		}

		double sum = 0;
		max = 0;
		for(int i = countA; i <= countB; i ++) {
			sum += list[i].Y;
			if(list[i].Y > max)
				max = list[i].Y;
		}

		avg = sum / ((countB - countA) +1);
	}

	public double ForceAVG {
		get { return forceAVG; }
	}
	public double ForceMAX {
		get { return forceMAX; }
	}

	public GetMaxAvgInWindow Gmaiw {
		get { return gmaiw; }
	}
}
*/

public static class ForceCalcs
{
	public static void GetAverageAndMaxForce (List<double> p_l, int countA, int countB, out double avg, out double max)
	{
		if(countA == countB) {
			avg = p_l[countA];
			max = p_l[countA];
			return;
		}

		double sum = 0;
		max = 0;
		for (int i = countA; i <= countB; i ++) {
			sum += p_l[i];
			if (i == countA || p_l[i] > max)
				max = p_l[i];
		}

		avg = sum / ((countB - countA) +1);
	}
	public static void GetAverageAndMaxForce (List<PointF> p_l, int countA, int countB, out double avg, out double max)
	{
		if(countA == countB) {
			avg = p_l[countA].Y;
			max = p_l[countA].Y;
			return;
		}

		double sum = 0;
		max = 0;
		for (int i = countA; i <= countB; i ++) {
			sum += p_l[i].Y;
			if (i == countA || p_l[i].Y > max)
				max = p_l[i].Y;
		}

		avg = sum / ((countB - countA) +1);
	}

	public static double GetRFD (List<PointF> p_l, int countA, int countB)
	{
		//LogB.Information(string.Format("GetRFD count A: {0}, count B: {1}, forces.Count: {2}, time.Count: {3}",
		//			countA, countB, forces.Count, times.Count));

		double calc = (p_l[countB].Y - p_l[countA].Y) / (p_l[countB].X/1000000.0 - p_l[countA].X/1000000.0); //microsec to sec
		//LogB.Information(string.Format("GetRFD {0}, {1}, {2}, {3}, {4}, {5}, RFD: {6}",
		//			countA, countB, forces[countA], forces[countB], times[countA], times[countB], calc));
		return calc;
	}

	public static double GetImpulse (List<PointF> p_l, int countA, int countB)
	{
		double sum = 0;
		int samples = 0;
		for(int i = countA; i <= countB; i ++)
		{
			sum += p_l[i].Y;
			samples ++;
		}

		double elapsedSeconds = p_l[countB].X/1000000.0 - p_l[countA].X/1000000.0;
		return sum * UtilAll.DivideSafe (elapsedSeconds, samples);
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
		//LogB.Information(string.Format("Compare: {0} and {1} with: {2}", criteriaPassedValue, previousToCriteriaValue, numToCompare));
		return ( Math.Abs(criteriaPassedValue - numToCompare) <= Math.Abs(previousToCriteriaValue - numToCompare) );
	}

	public static double GetMax (List<double> d_l)
	{
		double max = 0;
		foreach (double d in d_l)
			if ( d > max )
				max = d;

		return max;
	}
	public static double GetMin (List<double> d_l)
	{
		double min = 1000000;
		foreach (double d in d_l)
			if ( d < min )
				min = d;

		return min;
	}

	public static double GetProportion (double d, double min, double max)
	{
		d -= min;
		max -= min;
		return UtilAll.DivideSafe(d, max);
	}

	/*
	   on raceAnalyzer we want a proportion of a value between a list of values, and we want 0 is included.
	   Maybe in that list the min is above 0, so shouldInclude0 fixes it
	   */
	public static double GetProportion (double d, List<double> d_l, bool shouldInclude0)
	{
		double max = GetMax (d_l);
		double min = GetMin (d_l);

		if (shouldInclude0 && min > 0)
			min = 0;

		// move d and max to min
		d -= min;
		max -= min;
		return UtilAll.DivideSafe(d, max);
	}

	public static bool DecimalTooShortOrLarge (double d)
	{
		return (d < (double)decimal.MinValue || d > (double)decimal.MaxValue);
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
	public InterpolateSignal (int minY, int maxY, int maxX, int stepX)
	{
		Random random = new Random();
		this.point_l = new List<PointF>();
		int range = maxY - minY;

		//LogB.Information(string.Format("InterpolateSignal maxX: {0}, stepX: {1}", maxX, stepX));

		for(int i = 0; i < maxX; i += stepX)
		{
			point_l.Add(new PointF(i, minY + (random.NextDouble() * range)));

			/*
			PointF p = new PointF(i, minY + (random.NextDouble() * range));
			point_l.Add(p);
			LogB.Information(string.Format("point: {0}", p));
			*/
		}
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

	public static void TestCosineAndCubicInterpolate(bool onlyPositives)
	{
		Random random = new Random();
		List<PointF> l = new List<PointF>();

		for(int i = 0; i < 100; i += 10)
		{
			if(onlyPositives) {
				l.Add(new PointF(i, random.NextDouble() * 10)); // 0-10
			} else {
				//negatives & positives (0-10)
				//https://stackoverflow.com/a/1064907
				l.Add(new PointF(i, random.NextDouble() * (10 - -10) + -10));
			}
		}

		InterpolateSignal fsp = new InterpolateSignal(l);

		//cosine
		fsp.testCosineCubicInterpolateDo(types.COSINE);

		//cubic
		fsp.testCosineCubicInterpolateDo(types.CUBIC);
		//fsp.toFile(interpolated_l, types.CUBIC);
	}

	public List<PointF> GetCubicInterpolated()
	{
		return testCosineCubicInterpolateDo(types.CUBIC);
	}

	private List<PointF> testCosineCubicInterpolateDo(types type)
	{
		List<PointF> interpolated_l = new List<PointF>();

		//point_l.Count has to be >= 3
		for(int i = 0; i < point_l.Count; i ++) //each known point
		{
			//for(double j = 0.05; j < 1 ; j += .1) //10 interpolated value for each master (see timeStep on gui/app1/forceSensor.cs)
			for(double j = 0.005; j < 1 ; j += .01) //100 interpolated value for each master (see timeStep & lastTime += 10000 on gui/app1/forceSensor.cs)
			{
				if (type == types.COSINE)
				{
					int second = i+1; //the second point
					if(i == point_l.Count -1)
						second = 0;

					interpolated_l.Add(new PointF(
							point_l[i].X + j*(point_l[second].X - point_l[i].X),
							CosineInterpolate(point_l[i].Y, point_l[second].Y, j)));
				}
				else if(type == types.CUBIC)
				{
					//for cubic we need two extra points
					int a = i-1;
					int b = i;
					int c = i+1;
					int d = i+2;
					if(i == 0)
						a = point_l.Count -1;
					else if(i == point_l.Count -2)
						d = 0;
					else if(i == point_l.Count -1) {
						c = 0;
						d = 1;
					}

					interpolated_l.Add(new PointF(
								point_l[b].X + j*(point_l[c].X - point_l[b].X),
								CubicInterpolate(
									point_l[a].Y,
									point_l[b].Y,
									point_l[c].Y,
									point_l[d].Y,
									j)));
				}
			}
		}

		return interpolated_l;
	}

	//just to debug, unused right now
	private void toFile(List<PointF> interpolated_l, types type)
	{
		TextWriter writer = File.CreateText(
				Path.Combine(Path.GetTempPath(), string.Format("chronojump_testinterpolate_{0}.csv", type.ToString())));

		writer.WriteLine("X;Y;color;cex");

		for(int i = 0; i < point_l.Count; i ++)
			writer.WriteLine(string.Format("{0};{1};{2};{3}",
						point_l[i].X, point_l[i].Y, "red", 2));

		for(int i = 0; i < interpolated_l.Count; i ++)
			writer.WriteLine(string.Format("{0};{1};{2};{3}",
						interpolated_l[i].X, interpolated_l[i].Y, "black", 1));

		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();

		/*
		   test it with R:
		   d=read.csv2("/tmp/chronojump_testinterpolate_COSINE.csv")
		   plot(d$X, d$Y, col=d$color, cex=d$cex, type="b")
		   par(new=T)
		   d=read.csv2("/tmp/chronojump_testinterpolate_CUBIC.csv")
		   plot(d$X, d$Y, col=d$color, cex=d$cex, type="b")
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
