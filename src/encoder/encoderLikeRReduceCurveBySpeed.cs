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

public class EncoderCaptureLikeRReduceCurveBySpeed
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

	// constructor
	public EncoderCaptureLikeRReduceCurveBySpeed ()
	{
	}

	// This should work for all eccons, check tests/fixEccConCutOnNotSingleFile/getCurveStartEnd.R
	// and the example on getStableConcentricStart
	private ReducedCurve reduceCurveByPredictStartEnd (List<double> dis_l, string eccon, int minHeight)
	{
		LogB.Information ("reduceCurveByPredictStartEnd start");
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

		dis_l = UtilList.ListGetFromToIncluded (dis_l, startByStability, endByStability);

		// 2) delete initial/final zeros
		int firstInitialNonZero = getFirstNonZero (dis_l);
		int lastFinalNonZero = getLastNonZero (dis_l);

		if (firstInitialNonZero >= lastFinalNonZero)
			return new ReducedCurve (dis_l,
					1,
					dis_l.Count); // TODO: take care if this has to be dis_l.Count -1

		dis_l = UtilList.ListGetFromToIncluded (dis_l, firstInitialNonZero, lastFinalNonZero);

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

		LogB.Information (string.Format ("zerosAtLeft: {0}, zerosAtRight: {1}", zerosAtLeft, zerosAtRight));

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

	private int getStableConcentricStart (List<double> dis_l, int minHeight)
	{
		return 0; //TODO
	}
	private int getStableEccentricStart (List<double> dis_l, int minHeight)
	{
		return 0; //TODO
	}
	private int getStableConcentricEnd (List<double> dis_l, int minHeight)
	{
		return 0; //TODO
	}
	private int getStableEccentricEnd (List<double> dis_l, int minHeight)
	{
		return 0; //TODO
	}

	private int predictNeededZerosAtLeft (List<double> dis_l)
	{
		return 0; //TODO:
		/*
		// 1 find the first 3 values
		firstThreeNonZeroPos <- head (which (displacement [2:length(displacement)] != 0), n = 3);

		// if there are less than 3 values, just return the number of initial zeros (the same will be used)
		if (length (firstThreeNonZeroPos) < 3)
			return (firstThreeNonZeroPos[1]);

		position <- cumsum (displacement);

		// 2 try to find the x at min (position) -1
		xAtDesiredY <- getXatY (firstThreeNonZeroPos, cumsum(position)[firstThreeNonZeroPos], min(position) -1);

		// if is.nan, the parabole does not pass by the point, we can increase the number of values or just return the num of initial zeros
		if (is.nan (xAtDesiredY) || xAtDesiredY < 0)
			return (firstThreeNonZeroPos[1]);

		// 3 detected num of initial zeros
		return (round (xAtDesiredY, 0));
		*/
	}
}
