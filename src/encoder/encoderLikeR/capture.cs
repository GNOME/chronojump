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
	// passed variables assigned with this
	// ... on constructor
	private EncoderConfiguration econf;
	private string eccon;
	private int minHeightMm;
	private double massBody;
	private double massExtra;
	private int anglePush;
	private int angleWeight;
	private int exercisePercentBodyWeight;
	private bool propulsive;

	// passed variables assigned with this
	// ... on Do ()
	private bool capturing;
	private int startInSet;
	private int smoothSamplesLeft;
	//private int smoothSamplesRight; //unused var, we find the right have left + count

	private int start;
	private string [] repetitionStrArray;
	private EncoderLikeRKinematics kinematics;

	private List<List<double>> smoothTestSpeed_ll;
	private List<List<double>> smoothTestAccel_ll;
	private List<List<double>> smoothTestPower_ll;

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

		smoothTestSpeed_ll = new List<List<double>> ();
		smoothTestAccel_ll = new List<List<double>> ();
		smoothTestPower_ll = new List<List<double>> ();
	}

	public bool Do (bool justDebug, //to make calculations but do not send repetitionStrArray
			bool capturing,
			List<int> curve_l,
			List<int> curveForSmooth_l, //has directionChangePeriod: +200 samples (or more at each side to do the smoothing)
			int smoothSamplesLeft, //int smoothSamplesRight, //should be the same but maybe we cannot guarantee that (end of the capture)
			bool inertialDiscAbove0BodyBelow0,
			int startInSet, int curvesAccepted
			)
	{
		this.capturing = capturing;
		this.startInSet = startInSet;
		this.smoothSamplesLeft = smoothSamplesLeft;
		//this.smoothSamplesRight = smoothSamplesRight;

		// 1) get displacement
		List<double> dis_l = getDisplacement (curve_l);
		List<double> disSmooth_l = getDisplacement (curveForSmooth_l);

		// 2) cut displacement on dis_l
		dis_l = cutDisplacement (dis_l);

		// 3) check if height is enough
		double sumDis = UtilList.Sum (dis_l);
		LogB.Information (string.Format ("encoderLikeR.Do sumDis: {0}", sumDis));
		if (Math.Abs(sumDis) < minHeightMm)
			return false;

		// 4) fix eccon
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

		// 5) calculate kinematics (includes dynamics)

		// Last one will be the used on repetitionStrArray. If do not want test all, just create the list with one element like:
		//List<double> smoothBwFreqTesting_l = new List<double> { 5 };
		List<double> smoothBwFreqTesting_l = new List<double> { 3, 4, 6, 7, 8, 9, -1 , 5};

		bool arrayCreated = false;
		for (int i = 0; i < smoothBwFreqTesting_l.Count; i ++)
		{
			// disSmooth_l is bigger, calculate on it the speed & accel smoothed
			EncoderLikeRKinematics kinematicsSmooth = getKinematicsSmooth (
					smoothBwFreqTesting_l[i], disSmooth_l, ecconFix);

			// using kinematicsSmooth speed, accel cutted to dis_l size, calculate rest of variables
			EncoderLikeRKinematics kinematics = getKinematics (
					smoothBwFreqTesting_l[i], dis_l, ecconFix,
					getTimeSmoothedCutted (kinematicsSmooth.Time_l, dis_l.Count),
					getVarSmoothedCutted (kinematicsSmooth.Speed_l, dis_l.Count),
					getVarSmoothedCutted (kinematicsSmooth.Accel_l, dis_l.Count),
					inertialDiscAbove0BodyBelow0);

			if (i == smoothBwFreqTesting_l.Count -1)
			{
				//create repetitionStrArray
				repetitionStrArray = repetitionStrArrayCreate (
						dis_l, sumDis, kinematics, curvesAccepted);
				arrayCreated = true;
			}
		}

		// 6) write debug csv bysmooths
		writeSpeedAccelBySmoothToFileDebug (string.Format ("encoderCSharpDebug_{0}_bysmooths.csv",
					startInSet));

		// 7) return true if repetitionStrArray is created
		if (justDebug || ! arrayCreated)
			return false;

		return true;
	}

	private List<double> getDisplacement (List<int> disInts_l)
	{
		// 1) get displacement
		EncoderLikeRGetDisplacement elrgd = new EncoderLikeRGetDisplacement ();

		if (econf.has_inertia)
			return elrgd.GetDisplacementInertial (disInts_l, econf.name, econf.d, econf.D, econf.gearedDownLikeR);
		else
			return elrgd.GetDisplacement (capturing, econf.name, disInts_l, econf.d, econf.D, econf.gearedDownLikeR);
	}

	private List<double> cutDisplacement (List<double> disDoubles_l)
	{
		// 2) reduceCurve by speed
		EncoderLikeRReduceCurveBySpeed elrrcs = new EncoderLikeRReduceCurveBySpeed (
				disDoubles_l,
				eccon,
				minHeightMm);

		EncoderLikeRReduceCurveBySpeed.ReducedCurve reducedCurve = elrrcs.ReducedCurveGet;
		//disDoubles_l = reducedCurve.dis_l; //the reconstructed with zeros is not used
		start = reducedCurve.startPos;
		int end = reducedCurve.endPos;

		//reduceCurveBySpeed reduces the curve. Then startInSet has to change:
		startInSet = startInSet + start;

		/*
		 * reduceCurveBySpeed, on inertial doesn't do a good right adjust on changing phase,
		 * it adds a value at right, and this value is a descending value that can produce a high acceleration there
		 * delete that value
		 */
		if (econf.has_inertia)
                        end --;

		return UtilList.ListGetFromToIncluded (disDoubles_l, start, end);
	}

	private EncoderLikeRKinematics getKinematicsSmooth (
			double smoothBwFreq, List<double> disSmooth_l, string ecconFix)
	{
		EncoderLikeRKinematics kinematicsSmooth;
		if (econf.has_inertia)
			kinematicsSmooth = new EncoderLikeRKinematicsInertial (econf.d, econf.inertiaTotalLikeR);
		else
			kinematicsSmooth = new EncoderLikeRKinematicsNotInertial ();

		kinematicsSmooth.PassParameters (
				disSmooth_l,
				smoothBwFreq, ecconFix, econf.name, econf.gearedDownLikeR,
				massBody, massExtra, anglePush, angleWeight, exercisePercentBodyWeight,
				propulsive, minHeightMm);

		kinematicsSmooth.CalculateSpeedAccelForSmooth ();

		return kinematicsSmooth;
	}

	// get the smoothed variables but cut them to be appropriate for the desired interval
	private List<int> getTimeSmoothedCutted (List<int> timeSmoothed_l, int dis_lCount)
	{
		return UtilList.ListGetFromToIncluded (timeSmoothed_l,
				smoothSamplesLeft + start,
				smoothSamplesLeft + start + dis_lCount -1);
	}
	private List<double> getVarSmoothedCutted (List<double> varSmoothed_l, int dis_lCount)
	{
		return UtilList.ListGetFromToIncluded (varSmoothed_l,
				smoothSamplesLeft + start,
				smoothSamplesLeft + start + dis_lCount -1);
	}

	private EncoderLikeRKinematics getKinematics (
			double smoothBwFreq, List<double> dis_l, string ecconFix,
			List<int> timeSmoothed_l, List<double> speedSmoothed_l, List<double> accelSmoothed_l,
			bool inertialDiscAbove0BodyBelow0)
	{
		// 4.c) on inertia disSmooth_l is data of the disc in order to smooth speed and accel using disc movement.
		// Now that is smoothed, just convert it to body movement

		if (econf.has_inertia && inertialDiscAbove0BodyBelow0)
			for (int i = 0; i < speedSmoothed_l.Count; i ++)
			{
				speedSmoothed_l[i] *= -1;
				if (i < accelSmoothed_l.Count)
					accelSmoothed_l[i] *= -1;
			}

		// 4.c) calculate kinematics on desired interval but using (passing to it) the smoothed variables

		if (econf.has_inertia)
			kinematics = new EncoderLikeRKinematicsInertial (econf.d, econf.inertiaTotalLikeR);
		else
			kinematics = new EncoderLikeRKinematicsNotInertial ();

		kinematics.PassParameters (
				dis_l,
				smoothBwFreq, ecconFix, econf.name, econf.gearedDownLikeR,
				massBody, massExtra, anglePush, angleWeight, exercisePercentBodyWeight,
				propulsive, minHeightMm);

		kinematics.PassTimeSpeedAccelSmoothed (timeSmoothed_l, speedSmoothed_l, accelSmoothed_l);
		kinematics.CalculatePropulsiveAndDynamics ();

		//kinematics.WriteToFileDebug (string.Format ("encoderCSharpDebug_{0}_smooth_{1}.csv",
		//			startInSet, smoothBwFreq));

		smoothTestSpeed_ll.Add (speedSmoothed_l);
		smoothTestAccel_ll.Add (accelSmoothed_l);
		smoothTestPower_ll.Add (kinematics.Power_l);

		return kinematics;
	}

	private string [] repetitionStrArrayCreate (
			List<double> dis_l, double sumDis, EncoderLikeRKinematics kinematics, int curvesAccepted)
	{
		List<double> speedAbs_l = UtilList.ListDoubleToAbs (kinematics.Speed_l);
		List<double> forceAbs_l = UtilList.ListDoubleToAbs (kinematics.Force_l);
		List<double> powerAbs_l = UtilList.ListDoubleToAbs (kinematics.Power_l);

		int speedMaxPos = 0;
		double speedMax = UtilList.GetMaxValueAndPos (speedAbs_l, ref speedMaxPos);

		int forceMaxPos = 0;
		double forceMax = UtilList.GetMaxValueAndPos (forceAbs_l, ref forceMaxPos);

		int powerMaxPos = 0;
		double powerMax = UtilList.GetMaxValueAndPos (powerAbs_l, ref powerMaxPos);
		// 4) prepare data to be shown to user

		//TODO: fix this, is the same as capture.R 93-101
		//all decimals . (same as R)
		return new string[] {
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
	}

	public void writeSpeedAccelBySmoothToFileDebug (string filename)
	{
		LogB.Information ("At writeSpeedAccelBySmoothToFileDebug");
		TextWriter writer = File.CreateText (Path.Combine (Path.GetTempPath (), filename));

		//TODO: generate this according to smoothBwFreqTesting_l
		//writer.WriteLine ("speed 3;speed 4;speed 5;speed 6;speed 7;speed 8;speed 9;speed 10;" +
		//		"accel 3;accel 4;accel 5;accel 6;accel 7;accel 8;accel 9;accel 10;" +
		//		"power 3;power 4;power 5;power 6;power 7;power 8;power 9;power 10");
		writer.WriteLine ("speed 5;accel 5;power 5");

		int sample = 0;
		bool listsEnded = false;
		List<string> rowStr_l = new List<string> ();
		while (! listsEnded)
		{
			rowStr_l = new List<string> ();

			for (int i = 0; i < smoothTestSpeed_ll.Count; i ++)
			{
				if (sample < smoothTestSpeed_ll[i].Count)
					rowStr_l.Add (Util.TrimDecimals (smoothTestSpeed_ll[i][sample], 4));
				else {
					rowStr_l.Add ("");
					listsEnded = true;
				}
			}
			for (int i = 0; i < smoothTestAccel_ll.Count; i ++)
			{
				if (sample < smoothTestAccel_ll[i].Count)
					rowStr_l.Add (Util.TrimDecimals (smoothTestAccel_ll[i][sample], 4));
				else {
					rowStr_l.Add ("");
					listsEnded = true;
				}
			}
			for (int i = 0; i < smoothTestPower_ll.Count; i ++)
			{
				if (sample < smoothTestPower_ll[i].Count)
					rowStr_l.Add (Util.TrimDecimals (smoothTestPower_ll[i][sample], 4));
				else {
					rowStr_l.Add ("");
					listsEnded = true;
				}
			}
			writer.WriteLine (UtilList.ListStringToString (rowStr_l, ";"));
			sample ++;
		}

                writer.Flush();
                writer.Close();
                ((IDisposable)writer).Dispose();

		/*
		with R check this like this:

		repStart <- 1368
		d <- read.csv2 (paste ("encoderCSharpDebug_", repStart, "_bysmooths.csv", sep=""))
		r <- read.csv2("encoderR_0.csv")

#		xlim <- c(0, 600)
		ylim <- c(0, max(r$speed))
		#ylim <- c(0.6,.8)
#plot (r$speed, col="green", type="l", lwd=3, xlim=xlim, ylim=ylim)
		plot (r$speed[1:200], col="green", type="l", lwd=3)
		abline (h=0, lty=2, col="gray")
		lines (d$speed.3, col="red")
		lines (d$speed.4, col="red", lty=2)
		lines (d$speed.5, col="green")
		lines (d$speed.6, col="green", lty=2)
		lines (d$speed.7, col="blue")
		lines (d$speed.8, col="blue", lty=2)
		lines (d$speed.9, col="brown")
		lines (d$speed.10, col="brown", lty=2) #or automatic

		par (new=T)
		xlim <- c(0, 600)
		ylim <- c(min(r$accel), max(r$accel))
		#ylim <- c(-7.5, 3.5)
		plot (r$accel, col="red", type="l", lwd=3, xlim=xlim, ylim=ylim, axes=F)
		abline (h=0, lty=2, col="gray")
		axis (4, col="red")
		lines (d$accel.3, col="red")
		lines (d$accel.4, col="red", lty=2)
		lines (d$accel.5, col="green")
		lines (d$accel.6, col="green", lty=2)
		lines (d$accel.7, col="blue")
		lines (d$accel.8, col="blue", lty=2)
		lines (d$accel.9, col="brown")
		lines (d$accel.10, col="brown", lty=2) #or automatic
		*/
	}

	public string [] RepetitionStrArray {
		get { return repetitionStrArray; }
	}

//	public EncoderLikeRKinematics Kinematics {
//		get { return kinematics; }
//	}
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

// TODO: in the future use something like this instead of the string [] repetitionStrArray
public class EncoderLikeRRepetition
{
	/*
	public double start;
	public double duration;
	public double range;
	*/
	public int start;
	public int end;

	public EncoderLikeRRepetition (int start, int end)
	{
		this.start = start;
		this.end = end;
	}
}
