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

// this class will do same as encoder/capture.R
public class EncoderLikeR
{
	private EncoderConfiguration.Names econfName;
	private int minHeightMm;

	private string [] repetitionStrArray;

	//constructor
	public EncoderLikeR (EncoderConfiguration.Names econfName, int minHeightCm)
	{
		this.econfName = econfName;
		this.minHeightMm = minHeightCm * 10;
	}

	public bool Do (bool capturing,
			List<int> curve_l,
			double diameter, double diameterExt, int gearedDown,
			int startInSet, int curvesAccepted)
	{
		//LogB.Information ("____________ C#: pos_l before reduce __________");
		//LogB.Information (UtilList.ListIntToSQLString (UtilList.Cumsum(curve_l), " "));

		int curveNum = 0;
		repetitionStrArray = new string [] {};

		// 1) get displacement
		EncoderLikeRGetDisplacement elrgd = new EncoderLikeRGetDisplacement ();

		List<double> dis_l = elrgd.GetDisplacement (
				capturing,
				econfName,
				curve_l,
				diameter, diameterExt, gearedDown
				);

		LogB.Information (string.Format ("encoderLikeR before reduce: dis_l.Count: {0}", dis_l.Count));
		// 2) reduceCurve by speed
		EncoderLikeRReduceCurveBySpeed elrrcs = new EncoderLikeRReduceCurveBySpeed (
				dis_l,
				"c", 	//TODO: rest of eccons
				minHeightMm);
		EncoderLikeRReduceCurveBySpeed.ReducedCurve reducedCurve = elrrcs.ReducedCurveGet;
		//dis_l = reducedCurve.dis_l; //this is not used
		int start = reducedCurve.startPos;
		int end = reducedCurve.endPos;

		//reduceCurveBySpeed reduces the curve. Then startInSet has to change:
		startInSet = startInSet + start;

		LogB.Information (string.Format ("encoderLikeR after reduce: dis_l.Count: {0}, start: {1}, end: {2}",
					dis_l.Count, start, end));

		dis_l = UtilList.ListGetFromToIncluded (dis_l, start, end);

		//LogB.Information ("____________ C#: pos_l after reduce __________");
		//LogB.Information (UtilList.ListDoubleToString (UtilList.Cumsum(dis_l), 1, " "));

		//TODO: 3) calculations

		// 4) prepare data
		double sumDis = UtilList.Sum (dis_l);
		LogB.Information (string.Format ("encoderLikeR.Do sumDis: {0}", sumDis));
		if (sumDis > minHeightMm)
		{
			//TODO: fix this, is the same as capture.R 93-101
			//all decimals . (same as R)
			repetitionStrArray = new string[] {
				(curvesAccepted +1).ToString (),
				Util.ConvertToPoint (startInSet),
				Util.ConvertToPoint (dis_l.Count),
				Util.ConvertToPoint (sumDis),
				"0", "0", "0", "0",
				"0", "0", "0", "0",
				"0", "0", "0", "0",
				"0", "0" };

			return true;
		}
		return false;
	}

	public string [] RepetitionStrArray {
		get { return repetitionStrArray; }
	}
}

/*
 * TODO: in the future use something like this instead of the string [] repetitionStrArray
public class EncoderLikeRRepetition
{
	public double start;
	public double duration;
	public double range;
	public EncoderLikeRRepetition ()
	{
	}
}
*/
