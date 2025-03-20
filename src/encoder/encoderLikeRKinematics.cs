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

// this class will do same as encoder/util.R kinematicsF

public class EncoderLikeRKinematics
{
	// passed variables
	List<double> dis_l;
	double butterworthFreq;
	string eccon;
	double gearedDown;
	EncoderConfiguration.Names econfName;
	double massBody;
	double massExtra;
	int anglePush;
	int angleWeight;
	int exercisePercentBodyWeight;
	bool propulsive;
	int minHeightMm;

	List<double> disOrig_l; //just to debug
	List<int> time_l;
	List<double> speed_l;
	List<double> accel_l;
	Butterworth bw;

	EncoderLikeRRepPhase eccPhase;
	EncoderLikeRRepPhase isoPhase;
	EncoderLikeRRepPhase conPhase;

	private const double g = 9.81;
	private const double pi = 3.141593;

	public EncoderLikeRKinematics (
			List<double> dis_l, double butterworthFreq, string eccon,
			EncoderConfiguration.Names econfName, double gearedDown,
			double massBody, double massExtra,
			int anglePush, int angleWeight, int exercisePercentBodyWeight,
			bool propulsive, int minHeightMm)
	{
		this.disOrig_l = dis_l;
		this.dis_l = dis_l;
		this.butterworthFreq = butterworthFreq;
		this.eccon = eccon;
		this.econfName = econfName;
		this.gearedDown = gearedDown;
		this.massBody = massBody;
		this.massExtra = massExtra;
		this.anglePush = anglePush;
		this.angleWeight = angleWeight;
		this.exercisePercentBodyWeight = exercisePercentBodyWeight;
		this.propulsive = propulsive;
		this.minHeightMm = minHeightMm;

		calculateSpeed ();
		calculateAccel ();

		int propulsiveEnd; //TODO: if not, select it as the concentric end
		if (propulsive)
			propulsiveEnd = findPropulsiveEndPrepare ();

		getDynamics ();
	}

	private void calculateSpeed ()
	{
		bw = new Butterworth (butterworthFreq);
		bw.AddFromList (dis_l);
		bw.Calculate (Butterworth.TimeEnum.MILIS);

		time_l = bw.Times_l;
		speed_l = bw.Y_l;
	}

	private void calculateAccel ()
	{
		accel_l = new List<double> ();
		int window = 1;
		bw = new Butterworth (butterworthFreq);

		for (int i = 0 ; i < speed_l.Count; i ++)
		{
			int pre = i - window;
			int post = i + window;

			if(pre <= 0)
				pre = 0;
			else if(post >= speed_l.Count -1)
				post = speed_l.Count -1;

			// *1000 to convert to m/s2
			accel_l.Add (1000 * UtilAll.DivideSafe (speed_l[post] - speed_l[pre],
						time_l[post] - time_l[pre]
						));

			bw.AddSample (i, accel_l[i]);
		}

		bw.Calculate (Butterworth.TimeEnum.MILIS);

		//time_l = bw.Times_l;
		accel_l = bw.Y_l;
	}

	private int findPropulsiveEndPrepare ()
	{
		LogB.Information ("findPropulsiveEndPrepare, eccon: " + eccon);
		List<double> disCON_l = new List<double> ();
		int propulsiveEnd = disCON_l.Count -1;

		if (eccon == "c")
		{
			disCON_l = dis_l; //displacement on concentric

			//get the position of max speed in concentric (if there are > 1, get the first)
			List<int> maxSpeedT_l = new List<int> ();
			UtilList.GetMaxValueAndPos (speed_l, ref maxSpeedT_l);
			int maxSpeedTCON = maxSpeedT_l[0];
			LogB.Information ("EncoderLikeRKinematics maxSpeedTCON = " + maxSpeedTCON.ToString ());

			propulsiveEnd = findPropulsiveEndDo (disCON_l, maxSpeedTCON);
		}
		else if (eccon == "ec")
		{
			/*
			 * At the moment not used as we are doing only capture now (and is is never ec)
			 *
			findECPhases ();
			LogB.Information (eccPhase.ToString ());
			LogB.Information (isoPhase.ToString ());
			LogB.Information (conPhase.ToString ());

			//TODO: continue here
			*/
		}
		else if (eccon == "e") {
                        // not propulsive calculations on eccentric
                } else { //ecS
                        // #print("WARNING ECS\n\n\n\n\n")
                }

		if (propulsiveEnd >= accel_l.Count)
			propulsiveEnd = accel_l.Count -1;

		LogB.Information ("EncoderLikeRKinematics propulsiveEnd = " + propulsiveEnd.ToString ());
		return propulsiveEnd;
	}

	// adapted from encoder/util.R
	private int findPropulsiveEndDo (List<double> disCON_l, int maxSpeedTCON)
	{
		double propulsiveEndsAt = -g;
		if (econfName == EncoderConfiguration.Names.LINEARONPLANE)
		{
			// propulsive phase ends at: -g*sin(alfa)
			propulsiveEndsAt = -g * Math.Sin (anglePush * pi / 180);
		}
		else if (econfName == EncoderConfiguration.Names.LINEARONPLANEWEIGHTDIFFANGLE)
		{
			double massBodyUsed = getMassBodyByExercise (massBody, exercisePercentBodyWeight);
			// propulsive phase ends at: g * [massBodyUsed*sin(anglePush) + massExtra*sin(angleWeight)] / (massBodyUsed + massExtra)
			propulsiveEndsAt = -g * (massBodyUsed * Math.Sin (anglePush * pi / 180) +
					massExtra * Math.Sin (angleWeight * pi / 180)) / (massBodyUsed + massExtra);
		}

		//get at which(s) pos the accel is lower han propusliveEndsAt
		List<int> posPropulsiveEnd_l = UtilList.GetPosListBelowAValue (accel_l, propulsiveEndsAt, true);

		//if accel is always greater than g, all is propulsive phase, return the last pos of concentric
		if (posPropulsiveEnd_l.Count == 0)
			return disCON_l.Count -1;
		else {
			/*
			this:
				propulsiveEnd = min(which(accel[concentric] <= -g))
			can be a problem because some people does an strange countermovement at start of concentric movement
			this people moves arms down and legs go up
			at this moment acceleration can be lower than -g
			if this happens, propulsiveEnd will be very early and detected jump will be very high
			is exactly the same problem than findTakeOff, see that method for further help
			another option can be using extrema

			so need to find the accelPropulsiveEnd_l that is closer to maxSpeedTCON
			*/
			int distanceTo = disCON_l.Count -1; // distanceTo: maxSpeedTCON
			int pos = 0;
			foreach (int i in posPropulsiveEnd_l)
				if (Math.Abs (i - maxSpeedTCON) < distanceTo)
				{
					distanceTo = Math.Abs (i - maxSpeedTCON);
					pos = i;
					LogB.Information (string.Format ("EncoderLikeRKinematics distanceTo: {0}, pos: {1}", distanceTo, pos));
				}

			return pos;
		}
	}

	private double getMassBodyByExercise (double massBody, int exercisePercentBodyWeight)
	{
		return (massBody * exercisePercentBodyWeight / 100.0);
	}

	// At the moment not used as we are doing only capture now (and is is never ec)
	private void findECPhases ()
	{
		List<double> pos_l = UtilList.Cumsum (dis_l);

		// 1. get the changeEccCon
		//changeEccCon <- mean (which (position == min (position)))
		List<int> i_l = new List<int> ();
		UtilList.GetMinValueAndPos (pos_l,  ref i_l);
		int changeEccCon = Convert.ToInt32 (UtilList.GetAverage (i_l));

		// 2. ecc using reduceCurveBySpeed
		EncoderLikeRReduceCurveBySpeed rcsEcc = new EncoderLikeRReduceCurveBySpeed (
				UtilList.ListGetFromToIncluded (dis_l, 0, changeEccCon),
				"e", minHeightMm);
		EncoderLikeRReduceCurveBySpeed.ReducedCurve reducedCurveEcc = rcsEcc.ReducedCurveGet;
		eccPhase = new EncoderLikeRRepPhase (
				EncoderLikeRRepPhase.EcconEnum.ECC,
				reducedCurveEcc.startPos,
				reducedCurveEcc.endPos);

		// 3. con using reduceCurveBySpeed
		EncoderLikeRReduceCurveBySpeed rcsCon = new EncoderLikeRReduceCurveBySpeed (
				UtilList.ListGetFromToIncluded (dis_l, changeEccCon, dis_l.Count -1),
				"c", minHeightMm);
		EncoderLikeRReduceCurveBySpeed.ReducedCurve reducedCurveCon = rcsCon.ReducedCurveGet;
		conPhase = new EncoderLikeRRepPhase (
				EncoderLikeRRepPhase.EcconEnum.CON,
				changeEccCon + reducedCurveEcc.startPos,
				changeEccCon + reducedCurveEcc.endPos);

		// 4. iso
		// is this a bug? con_l$startPos will always be > changeEccCon
		// should not be con_l$startPos > 0 ?
		isoPhase = new EncoderLikeRRepPhase (
				EncoderLikeRRepPhase.EcconEnum.ISO,
				-1, -1);
		if (reducedCurveEcc.endPos < changeEccCon || reducedCurveCon.startPos > changeEccCon)
			isoPhase = new EncoderLikeRRepPhase (
					EncoderLikeRRepPhase.EcconEnum.ISO,
					reducedCurveEcc.endPos,
					changeEccCon + reducedCurveCon.endPos -1);
	}

	private void getDynamics ()
	{
		double massBodyUsed = getMassBodyByExercise (massBody, exercisePercentBodyWeight);
		double massExtraUsed = 0; //TODO: think if use massExtra of the class

		if (
				econfName == EncoderConfiguration.Names.WEIGHTEDMOVPULLEYLINEARONPERSON1 ||
				econfName == EncoderConfiguration.Names.WEIGHTEDMOVPULLEYLINEARONPERSON1INV ||
				econfName == EncoderConfiguration.Names.WEIGHTEDMOVPULLEYLINEARONPERSON2 ||
				econfName == EncoderConfiguration.Names.WEIGHTEDMOVPULLEYLINEARONPERSON2INV ||
				econfName == EncoderConfiguration.Names.WEIGHTEDMOVPULLEYROTARYFRICTION ||
				econfName == EncoderConfiguration.Names.WEIGHTEDMOVPULLEYROTARYAXIS)
			massExtraUsed = getMass (massExtraUsed);

		double massTotal = massBodyUsed + massExtraUsed;

		/*
		 * TODO:
		if (econf.has_inertia)
			return (getDynamicsInertial(encoderConfigurationName, displacement, diameter, massTotal, inertiaMomentum, gearedDown, smoothing))
		else
			return (getDynamicsNotInertial (encoderConfigurationName, speed, accel,
						massBody, massExtra, massTotal,
						exercisePercentBodyWeight, gearedDown, anglePush, angleWeight))
						*/
	}

	//gearedDown is positive, normally 2
	//this is not used on inertial machines
	private double getMass (double mass)
	{
		if(mass == 0)
			return 0;

		//default value of angle is 90 degrees. If is not selected, it's -1
		double myAngle = anglePush;
		if (myAngle == -1)
			myAngle = 90;

		return ( (mass / gearedDown) * Math.Sin (myAngle * pi / 180) );
	}


	public void WriteToFileDebug (string filename)
	{
                TextWriter writer = File.CreateText (Path.Combine (Path.GetTempPath (), filename));
	        writer.WriteLine (string.Format ("x;yUnfiltered;speed;accel"));
		for (int i = 0; i < speed_l.Count; i ++)
	              writer.WriteLine (string.Format ("{0};{1};{2};{3}",
					      time_l[i], disOrig_l[i], speed_l[i], accel_l[i]));

                writer.Flush();
                writer.Close();
                ((IDisposable)writer).Dispose();

		// check in R like with:
		// Rscript encoderLikeRKinematics.R filename
	}

	public List<double> Speed_l {
		get { return speed_l; }
	}
	public double SpeedAVG {
		get { return UtilList.GetAverage (speed_l); }
	}
}

