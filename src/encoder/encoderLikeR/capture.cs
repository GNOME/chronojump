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
public class EncoderLikeRCapture
{
	private EncoderConfiguration econf;
	private string eccon;
	private int minHeightMm;
	private double massBody;
	private double massExtra;
	private int anglePush;
	private int angleWeight;
	private int exercisePercentBodyWeight;
	private bool propulsive;


	private string [] repetitionStrArray;

	//constructor
	public EncoderLikeRCapture (EncoderParams encoderParams)
	{
		this.eccon = encoderParams.eccon;
		this.minHeightMm = encoderParams.minHeight * 10;
		this.econf = encoderParams.encoderConfiguration;
		this.massBody = encoderParams.MassBodyD;
		this.massExtra = encoderParams.MassExtraD;
		this.anglePush = encoderParams.encoderConfiguration.anglePush;
		this.angleWeight = encoderParams.encoderConfiguration.angleWeight;
		this.exercisePercentBodyWeight = encoderParams.exercisePercentBodyWeight;
		this.propulsive = encoderParams.Propulsive;
	}

	public bool Do (bool capturing,
			List<int> curve_l,
			int startInSet, int curvesAccepted)
	{
		//LogB.Information ("____________ C#: pos_l before reduce __________");
		//LogB.Information (UtilList.ListIntToSQLString (UtilList.Cumsum(curve_l), " "));

		UtilList.ListIntToFile (curve_l, " ", Path.Combine (Path.GetTempPath (), "chronojump-debug-encoderLikeR_Do0_curve_l.txt")); //es el mateix

		repetitionStrArray = new string [] {};

		// 1) get displacement
		EncoderLikeRGetDisplacement elrgd = new EncoderLikeRGetDisplacement ();
		List<double> dis_l = new List<double> ();

		if (econf.has_inertia)
			dis_l = elrgd.GetDisplacementInertial (
					curve_l, econf.name,
					econf.d, econf.D, econf.gearedDownLikeR
					);
		else
			dis_l = elrgd.GetDisplacement (
					capturing, econf.name,
					curve_l, econf.d, econf.D, econf.gearedDownLikeR
					);

		//LogB.Information (string.Format ("encoderLikeR before reduce: dis_l.Count: {0}", dis_l.Count));
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

		//LogB.Information (string.Format ("encoderLikeR after reduce: dis_l.Count: {0}, start: {1}, end: {2}",
		//			dis_l.Count, start, end));

		/*
		 * reduceCurveBySpeed, on inertial doesn't do a good right adjust on changing phase,
		 * it adds a value at right, and this value is a descending value that can produce a high acceleration there
		 * delete that value
		 */
		if (econf.has_inertia)
                        end --;

		dis_l = UtilList.ListGetFromToIncluded (dis_l, start, end);

		//LogB.Information (string.Format ("encoderLikeR after reduce B: dis_l.Count: {0}",
		//			dis_l.Count));

		//LogB.Information ("____________ C#: pos_l after reduce __________");
		//LogB.Information (UtilList.ListDoubleToString (UtilList.Cumsum(dis_l), 1, " "));

		// 3) check if height is enough
		double sumDis = UtilList.Sum (dis_l);
		LogB.Information (string.Format ("encoderLikeR.Do sumDis: {0}", sumDis));
		if (Math.Abs(sumDis) < minHeightMm)
			return false;


		UtilList.ListDoubleToFile (dis_l, 2, " ", Path.Combine (Path.GetTempPath (), "chronojump-debug-encoderLikeR-after-reduce.txt"));


		// 4) calculations
		//TODO: check if this line (277) on capture.R is needed:
		//if(abs(max(position) - min(position)) >= op$MinHeight)

		string ecconFix = eccon;
		if (eccon == "ecS" || eccon == "ceS")
		{
			if (sumDis >= 0)
				ecconFix = "c";
			else
				ecconFix = "e";
		}

		EncoderLikeRKinematics kinematics;

		if (econf.has_inertia)
			kinematics = new EncoderLikeRKinematicsInertial (econf.d, econf.inertiaTotalLikeR);
		else
			kinematics = new EncoderLikeRKinematicsNotInertial ();

		kinematics.PassParameters (
				dis_l, 15, ecconFix,
				econf.name,
				econf.gearedDownLikeR,
				massBody, massExtra,
				anglePush, angleWeight, exercisePercentBodyWeight,
				propulsive, minHeightMm);

		kinematics.Calculate ();

		kinematics.WriteToFileDebug (string.Format ("encoderDebug_{0}.txt", startInSet));

		List<double> speedAbs_l = UtilList.ListDoubleToAbs (kinematics.Speed_l);
		List<double> forceAbs_l = UtilList.ListDoubleToAbs (kinematics.Force_l);
		List<double> powerAbs_l = UtilList.ListDoubleToAbs (kinematics.Power_l);

		int speedMaxPos = 0;
		double speedMax = UtilList.GetMaxValueAndPos (speedAbs_l, ref speedMaxPos);

		int forceMaxPos = 0;
		double forceMax = UtilList.GetMaxValueAndPos (forceAbs_l, ref forceMaxPos);

		int powerMaxPos = 0;
		double powerMax = UtilList.GetMaxValueAndPos (powerAbs_l, ref powerMaxPos);

		// 4) prepare data
		//TODO: fix this, is the same as capture.R 93-101
		//all decimals . (same as R)
		repetitionStrArray = new string[] {
			(curvesAccepted +1).ToString (),
				Util.CTP (startInSet),  //the difference between this and when graph.R is called is because on saveToFile trimInitialZeros is called, it deletes 0's on signal before 1st rep allowing allowedZeroMSAtStart (1000 zeros)
				Util.CTP (dis_l.Count),
				Util.CTP (sumDis),
				Util.CTP (UtilList.GetAverage (speedAbs_l)),	// 4 speeds: avg, max, maxpos, rvd
				Util.CTP (speedMax),
				speedMaxPos.ToString (),
				Util.CTP (UtilAll.DivideSafe (speedMax, UtilAll.DivideSafe (1.0 * speedMaxPos, 1000))),
				Util.CTP (UtilList.GetAverage (powerAbs_l)), 	// 4 powers: avg, max, maxpos, rpd
				Util.CTP (powerMax),
				powerMaxPos.ToString (),
				Util.CTP (UtilAll.DivideSafe (powerMax, UtilAll.DivideSafe (1.0 * powerMaxPos, 1000))), //ms -> s
				//Util.CTP (UtilList.GetAverage (forceAbs_l)),	// 4 forces: avg, max, maxpos, rpd
				Util.CTP (UtilList.GetAverage (kinematics.Force_l)),	// 4 forces: avg, max, maxpos, rpd //note on util.R pafGenerate the mean force is not done using absolute values
				Util.CTP (forceMax),
				forceMaxPos.ToString (),
				Util.CTP (UtilAll.DivideSafe (forceMax, UtilAll.DivideSafe (1.0 * forceMaxPos, 1000))), //ms -> s
				"0", "0" };	//TODO: work, impulse

		return true;
	}

	public string [] RepetitionStrArray {
		get { return repetitionStrArray; }
	}
}

public class EncoderLikeRRepPhase
{
	public enum EcconEnum { ECC, ISO, CON };
	public EcconEnum Eccon;
	public int Start;
	public int End;

	public EncoderLikeRRepPhase (EcconEnum Eccon, int Start, int End)
	{
		this.Eccon = Eccon;
		this.Start = Start;
		this.End = End;
	}

	// just to debug
	public override string ToString ()
	{
		return string.Format ("EncoderLikeRRepPhase Eccon: {0}, Start: {1}, End: {2}",
				Eccon, Start, End);
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
