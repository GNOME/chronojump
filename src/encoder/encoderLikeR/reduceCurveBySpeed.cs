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
 *  Copyright (C) 2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List<T>

/*
 * 2023 getStableConcentricStart && reduceCurveByPredictStartEnd
 *
 * The initial / final zeros affect a lot to the power. reduceCurveBySpeed depends on this initial zeros, and some parts of the program like analysis set and analysis session sometimes have different number of zeros affecting the result.
 * This functions deletes initial zeros (as we do not know if movement started)
 * And calculates where the initial zero should be from a regression from right to left
 * And reconstruct the displacement
 * Check tests/fixEccConCutOnNotSingleFile/getCurveStartEnd.R
 * Check tests/fixEccConCutOnNotSingleFile/fixEccConCutOnNotSingleFile.R
 */

public class EncoderLikeRReduceCurveBySpeed
{
	public struct ReducedCurve
	{
		public List<double> dis_l;
		public int startPos;
		public int endPos;

		public ReducedCurve (List<double> dis_l, int startPos, int endPos)
		{
			this.dis_l = dis_l;
			this.startPos = startPos;
			this.endPos = endPos;
		}
	}

	public ReducedCurve reducedCurve;

	// constructor
	public EncoderLikeRReduceCurveBySpeed (List<double> dis_l, string eccon, int minHeightMm)
	{
		reducedCurve = reduceCurveByPredictStartEnd (dis_l, eccon, minHeightMm);
	}

	// This should work for all eccons, check tests/fixEccConCutOnNotSingleFile/getCurveStartEnd.R
	// and the example on getStableConcentricStart
	private ReducedCurve reduceCurveByPredictStartEnd (List<double> dis_l, string eccon, int minHeight)
	{
		LogB.Information ("reduceCurveByPredictStartEnd start");
		//LogB.Information ("reduceCurveByPredictStartEnd A cumsum(dis_l): " + UtilList.ListDoubleToString (UtilList.Cumsum(dis_l), 1, " "));
	        int displacementLengthStored = dis_l.Count;

	        // 1) cut by getStableConcentricStart, getStableEccentricStart
		int startByStability = 1;
	        int endByStability = dis_l.Count;

		if (eccon == "c")
		{
			startByStability = getStableConcentricStart (dis_l, minHeight);
			endByStability = getStableConcentricEnd (dis_l, minHeight);
		}
		else if (eccon == "e")
		{
			startByStability = getStableEccentricStart (dis_l, minHeight);
			endByStability = getStableEccentricEnd (dis_l, minHeight);
		}
		else if (eccon == "ec")
		{
			startByStability = getStableEccentricStart (dis_l, minHeight);
			endByStability = getStableConcentricEnd (dis_l, minHeight);
		}

		// trying only to do the stability thing as the zeros checks are not needed because we are smoothing more a bigger interval
		return new ReducedCurve (dis_l,
				startByStability,
				endByStability);


		//TODO: decide if rest of the function is used or not.



		dis_l = UtilList.ListGetFromToIncluded (dis_l, startByStability, endByStability);
		//LogB.Information ("reduceCurveByPredictStartEnd B cumsum(dis_l): " + UtilList.ListDoubleToString (UtilList.Cumsum(dis_l), 1, " "));

		// 2) delete initial/final zeros
		int firstInitialNonZero = getFirstNonZero (dis_l);
		int lastFinalNonZero = getLastNonZero (dis_l);

		if (firstInitialNonZero >= lastFinalNonZero)
			return new ReducedCurve (dis_l,
					1,
					dis_l.Count); // TODO: take care if this has to be dis_l.Count -1

		dis_l = UtilList.ListGetFromToIncluded (dis_l, firstInitialNonZero, lastFinalNonZero);
		//LogB.Information ("reduceCurveByPredictStartEnd C cumsum(dis_l): " + UtilList.ListDoubleToString (UtilList.Cumsum(dis_l), 1, " "));

		int zerosAtLeft = 0;
		int zerosAtRight = 0;

		if (eccon == "c")
		{
			zerosAtLeft = predictNeededZerosAtLeft (dis_l);
			zerosAtRight = predictNeededZerosAtLeft (UtilList.ListReverse (dis_l));
		}
		else if (eccon == "e")
		{
			zerosAtLeft = predictNeededZerosAtLeft (UtilList.ListReverseSign (dis_l));
			zerosAtRight = predictNeededZerosAtLeft (UtilList.ListReverseSign (UtilList.ListReverse (dis_l)));
		}
		else if (eccon == "ec")
		{
			zerosAtLeft = predictNeededZerosAtLeft (UtilList.ListReverseSign (dis_l));
			zerosAtRight = predictNeededZerosAtLeft (UtilList.ListReverse (dis_l));
		}

		LogB.Information (string.Format ("C# zerosAtLeft: {0}, zerosAtRight: {1}", zerosAtLeft, zerosAtRight));

		int startPos = startByStability + firstInitialNonZero-1 - zerosAtLeft;
		int endPos = startByStability + firstInitialNonZero-1 + lastFinalNonZero + zerosAtRight;

		/*
		// if the displacement is all 0s then startPos is na. For this reason there are is.na checks (on R, on C# it will be -1)
		if (startPos < 0 || endPos < 0)
			return new ReducedCurve (dis_l,
					1,
					dis_l.Count); // TODO: take care if this has to be dis_l.Count -1
		*/

		if (startPos < 0)
			startPos = 0;

		if (endPos < 0)
			endPos =0;
		if (endPos > displacementLengthStored)
			endPos = displacementLengthStored;

		LogB.Information ("reduceCurveByPredictStartEnd end");

		// 4) return the reconstructed curve
        	// print (paste ("start moved to: ", startByStability + (firstInitialNonZero -1) - zerosAtLeft))
		List<double> disReconstructed_l = new List<double> ();
		LogB.Information ("reduceCurveByPredictStartEnd D cumsum(disReconstructed_l): " + UtilList.ListDoubleToString (UtilList.Cumsum(disReconstructed_l), 1, " "));
		for (int i = 0; i < zerosAtLeft; i ++)
			disReconstructed_l.Add (0);
		foreach (double dis in dis_l)
			disReconstructed_l.Add (dis);
		for (int i = 0; i < zerosAtRight; i ++)
			disReconstructed_l.Add (0);

		return new ReducedCurve (
				disReconstructed_l,
				startPos, // TODO: take care if this has to be -1
				endPos); // TODO: take care if this has to be -1
	}

	private int getFirstNonZero (List<double> dis_l)
	{
		for (int i = 0; i < dis_l.Count; i ++)
			if (! Util.SimilarDouble (dis_l[i], 0))
				return i;

		return 0;
	}
	private int getLastNonZero (List<double> dis_l)
	{
		for (int i = (dis_l.Count -1); i >= 0; i --)
			if (! Util.SimilarDouble (dis_l[i], 0))
				return i;

		return dis_l.Count -1;
	}

	// i is the position we are searching if has n zeros at left
	private bool hasNZerosAtLeft (List<double> dis_l, int i, int n)
	{
		if (i <= n)
			return false;

		for (int j = (i-1); j >= (i-n) ; j --)
			if (! Util.SimilarDouble (dis_l[j], 0))
				return false;

		return true;
	}

	private int zerosAtLeft (List<double> dis_l, int i)
	{
		int zeros = 0;
		if (i == 0)
			return 0;

		for (int j = (i-1); j >= 0 ; j --)
		{
			if (Util.SimilarDouble (dis_l[j], 0))
				zeros ++;
			else
				return (zeros);
		}

		return zeros;
	}

	/*
					con
					/
				      t
				     /
				 ---s
			   -----s
	       -----------S
	 -----s

		this function finds s that has at least 30 ms of stability at left
		t is the minHeight needed for being a repetition
		Considers also that from s,S to top has to be >= minHeight
		in the graph s,S have 30 zeros or more at left
		S is the point below t that has more zeros at left
	*/
	private int getStableConcentricStart (List<double> dis_l, int minHeight)
	{
		List<double> pos_l = UtilList.Cumsum (dis_l);

		if (UtilList.GetMax (pos_l) < minHeight)
			return 0;

		int t = 0;
		for (int i = 0; i < pos_l.Count ; i ++)
			if (pos_l[i] >= minHeight)
			{
				t = i;
				break;
			}

		int nZerosAtLeft = 30;
		if (t - nZerosAtLeft <= 0)
			return 0;

		int storedNZerosAtLeft = 0;
		int storedSample = -1;
		for (int j = t; j >= nZerosAtLeft; j --)
			if (pos_l[j] < pos_l[t] &&
					hasNZerosAtLeft (dis_l, j, nZerosAtLeft) &&
					UtilList.GetMax (pos_l) - pos_l[j] >= minHeight)
			{
				if (storedSample < 0)
				{
					storedSample = j;
					//storedHeight = pos_l[j];
					storedNZerosAtLeft = zerosAtLeft (dis_l, j);
				} else
				{
					int zerosAtLeftHere = zerosAtLeft (dis_l, j);
					if (zerosAtLeftHere > storedNZerosAtLeft)
					{
						storedSample = j;
						//storedHeight = pos_l[j];
						storedNZerosAtLeft = zerosAtLeftHere;
					}
				}
			}

		if (storedSample > 0)
			return storedSample;

		return 0;
	}

	/* reverse vertically and getStableConcentricStart
	  to find A, convert, find B, A == B
	  FROM 0,0,0,-1,-1,-2, ...  TO: 0,0,0,1,1,2, ...
	  FROM                TO
	                        ----
	                       /
	                      /
	0 -A-\            -B-/
	      \
	       \
	        ----
	*/
	private int getStableEccentricStart (List<double> dis_l, int minHeight)
	{
		return getStableConcentricStart (UtilList.ListReverseSign (dis_l), minHeight);
	}

	/* reverse horizontally, getStableConcentricStart, and then horizontally reverse the value again
	  and vertically to have the - as +
	  to find A, convert, find B, A = length - B
	  FROM 0,0,0,-1,-1,-2, ...  TO: 0,0,0,1,1,2, ...
	  FROM                TO
	                         --
	                        /
	                       /
	 0 --\            -B--/
	      \
	       \
	        -A--
	*/
	private int getStableEccentricEnd (List<double> dis_l, int minHeight)
	{
		return dis_l.Count -getStableConcentricStart (
					UtilList.ListReverseSign (UtilList.ListReverse (dis_l)),
					minHeight);
	}

	/* reverse horizontally, getStableConcentricStart
	   to find A, convert (simply reverse horizontally) , find B, A = length - B
		   FROM                 TO
		   -A-             ----
	          /               /
		 /               /
	  0 ----/            -B-/
	*/
	private int getStableConcentricEnd (List<double> dis_l, int minHeight)
	{
		return dis_l.Count -getStableConcentricStart (
				UtilList.ListReverse (dis_l),
				minHeight);
	}

	private int predictNeededZerosAtLeft (List<double> dis_l)
	{
		/*
		LogB.Information ("____________ C#: predictNeededZerosAtLeft __________");
		LogB.Information (dis_l.Count.ToString ());
		if (dis_l.Count >= 30)
		{
			LogB.Information ("____________ C#: like head(displacement, 30) __________");
			List<double> temp_l = UtilList.ListGetFromToIncluded (dis_l, 0, 29);
			LogB.Information (UtilList.ListDoubleToString (temp_l, 2, " "));
		}
		*/

		// 1) find the first 3 values (that are non zero)
		//LogB.Information ("C# threeNonZeros at positions:");
		List<int> x_l = new List<int> ();
		// note 1st value in dis_l is going to be != than 0 but is not counted (the same as in R method)
		for (int i = 1; i < dis_l.Count; i ++) 		//starting at 2nd value
			if (! Util.SimilarDouble (dis_l[i], 0))
			{
				x_l.Add (i);
				//LogB.Information (i.ToString ());
				if (x_l.Count >= 3)
					break;
			}

		// if there are less than 3 values, just return the number of initial zeros (the same will be used)
		if (x_l.Count < 3)
		{
			if (x_l.Count >= 1)
				return x_l[0];
			else
				return 0;
		}

		List<double> pos_l = UtilList.Cumsum (dis_l);

		// 2) try to find the x at min (position) -1
		List<PointF> threePoints_l = new List<PointF> ();
		for (int i = 0; i < 3; i ++)
			threePoints_l.Add (new PointF (x_l[i], pos_l[x_l[i]] -1)); // -1 to have data like R

		LeastSquaresParabole lsp = new LeastSquaresParabole ();
		lsp.Calculate (threePoints_l);
		// LogB.Information (string.Format ("C#: yDesired: {0}", UtilList.GetMin (pos_l) -1));

		double xAtDesiredY = 0;
		if (lsp.ParaboleType == LeastSquaresParabole.ParaboleTypes.STRAIGHT || lsp.ParaboleTypeAlmostStraight)
			xAtDesiredY = lsp.CalculateXAtSomeYAsStraightLine (threePoints_l[0], threePoints_l[1], UtilList.GetMin (pos_l) -1);
		else
			xAtDesiredY = lsp.CalculateXAtSomeY (UtilList.GetMin (pos_l) -1);

		// if null, the parabole does not pass by the point, we can increase the number of values or just return the num of initial zeros
		if (double.IsNaN (xAtDesiredY) || xAtDesiredY < 0 ||
				! MathUtil.DoubleCanBeInt32Safe (xAtDesiredY)
				)
		{
			if (x_l.Count >= 1)
				return x_l[0];
			else
				return 0;
		}

		//LogB.Information ("xAtDesiredY = ");
		//LogB.Information (xAtDesiredY.ToString ());

		// 3 detected num of initial zeros
		return Convert.ToInt32 (xAtDesiredY);
	}

	public ReducedCurve ReducedCurveGet {
		get { return reducedCurve; }
	}
}
