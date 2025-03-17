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
	private EncoderConfiguration econf;
	private string eccon;
	private int minHeightMm;

	private string [] repetitionStrArray;

	//constructor
	public EncoderLikeR (EncoderParams encoderParams)
	{
		this.eccon = encoderParams.eccon;
		this.minHeightMm = encoderParams.minHeight * 10;
		this.econf = encoderParams.encoderConfiguration;
	}

	public bool Do (bool capturing,
			List<int> curve_l,
			int startInSet, int curvesAccepted)
	{
		//LogB.Information ("____________ C#: pos_l before reduce __________");
		//LogB.Information (UtilList.ListIntToSQLString (UtilList.Cumsum(curve_l), " "));

		int curveNum = 0;
		repetitionStrArray = new string [] {};

		// 1) get displacement
		EncoderLikeRGetDisplacement elrgd = new EncoderLikeRGetDisplacement ();
		List<double> dis_l = new List<double> ();

		if (econf.has_inertia)
			dis_l = elrgd.GetDisplacementInertial (
					curve_l, econf.name,
					econf.d, econf.D, econf.gearedDown
					);
		else
			dis_l = elrgd.GetDisplacement (
					capturing, econf.name,
					curve_l, econf.d, econf.D, econf.gearedDown
					);

		LogB.Information (string.Format ("encoderLikeR before reduce: dis_l.Count: {0}", dis_l.Count));
		// 2) reduceCurve by speed
		EncoderLikeRReduceCurveBySpeed elrrcs = new EncoderLikeRReduceCurveBySpeed (
				dis_l,
				eccon,
				minHeightMm);
		EncoderLikeRReduceCurveBySpeed.ReducedCurve reducedCurve = elrrcs.ReducedCurveGet;
		//dis_l = reducedCurve.dis_l; //this is not used
		int start = reducedCurve.startPos;
		int end = reducedCurve.endPos;

		//reduceCurveBySpeed reduces the curve. Then startInSet has to change:
		startInSet = startInSet + start;

		LogB.Information (string.Format ("encoderLikeR after reduce: dis_l.Count: {0}, start: {1}, end: {2}",
					dis_l.Count, start, end));

		/*
		 * reduceCurveBySpeed, on inertial doesn't do a good right adjust on changing phase,
		 * it adds a value at right, and this value is a descending value that can produce a high acceleration there
		 * delete that value
		 */
		if (econf.has_inertia)
                        end --;

		dis_l = UtilList.ListGetFromToIncluded (dis_l, start, end);

		//LogB.Information ("____________ C#: pos_l after reduce __________");
		//LogB.Information (UtilList.ListDoubleToString (UtilList.Cumsum(dis_l), 1, " "));

		// 3) check if height is enough
		double sumDis = UtilList.Sum (dis_l);
		LogB.Information (string.Format ("encoderLikeR.Do sumDis: {0}", sumDis));
		if (Math.Abs(sumDis) < minHeightMm)
			return false;

		// 4) calculations
		//TODO: check if this line (277) on capture.R is needed:
		//if(abs(max(position) - min(position)) >= op$MinHeight)

		EncoderLikeRKinematics kinematics = new EncoderLikeRKinematics (dis_l, 15);
		/*
		 * use position?
		 * List<double> pos_l = UtilList.Cumsum (dis_l);
		 * EncoderLikeRKinematics kinematics = new EncoderLikeRKinematics (pos_l, 15); //just trying results seem quite similar
		 */

		kinematics.WriteToFileDebug (string.Format ("encoderDebug_{0}.txt", startInSet));

		// 4) prepare data
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
