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

// this class does the same as encoder/util.R kinematicsF
public abstract class EncoderLikeRKinematics
{
	// passed variables
	protected List<double> dis_l;
	double butterworthFreq;
	string eccon;
	protected double gearedDown;
	protected EncoderConfiguration.Names econfName;
	protected double massBody;
	protected double massExtra;
	protected double massTotal;
	protected int anglePush;
	protected int angleWeight;
	int exercisePercentBodyWeight;
	bool propulsive;
	int minHeightMm;

	List<double> disOrig_l; //just to debug
	List<int> time_l;
	protected List<double> speed_l;
	protected List<double> accel_l;
	protected List<double> force_l;
	protected List<double> power_l;
	Butterworth bw;

	EncoderLikeRRepPhase eccPhase;
	EncoderLikeRRepPhase isoPhase;
	EncoderLikeRRepPhase conPhase;

	protected const double g = 9.81;
	protected const double pi = 3.141593;

	public void PassParameters (
			List<double> dis_l, double butterworthFreq, string eccon,
			EncoderConfiguration.Names econfName,
			double gearedDown, double massBody, double massExtra,
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
	}

	public void Calculate ()
	{
		calculateSpeed ();
		calculateAccel ();

		int propulsiveEnd = 0;
		if (propulsive)
			propulsiveEnd = findPropulsiveEndPrepare ();

		getDynamics ();

		/*
		 * TODO
		// on "e", "ec", start on ground
		if(eccModesStartOnGround && ! isInertial(repOp$econfName) &&
				(repOp$eccon == "e" || repOp$eccon == "ec")) {
			//if eccentric is undefined, find it
			if(length(eccentric) == 1 && eccentric == 0) {
				if(repOp$eccon == "e")
					eccentric = 1:length(displacement)
				else {  #(repOp$eccon=="ec")
					phases_l <- findECPhases (displacement, minHeight)
						eccentric <- phases_l$eccentric
				}
			}

			weight = mass * g
				if(length(which(force[eccentric] <= 0)) > 0) {
					landing = max(which(force[eccentric]<=0))
						start = landing
				}
		}
		*/

		int end = speed_l.Count -1;
		if (propulsive && (eccon == "c" || eccon == "ec")) 
	                end = propulsiveEnd;

		/*
		 * TODO
        	//as acceleration can oscillate, start at the eccentric part where there are not negative values
	        if(repOp$inertiaM > 0 && (repOp$eccon == "e" || repOp$eccon == "ec"))
        	{
                	if(repOp$eccon=="e") {
                        	eccentric=1:length(displacement)
	                }

        	        //if there is eccentric data and there are negative values
                	if(length(eccentric) > 0 && min(accel$y[eccentric]) < 0)
	                {
        	                #deactivated:
                	        #start = max(which(accel$y[eccentric] < 0)) +1
                        	#print("------------ start -----------")
	                        #print(start)
        	        }
	        }

		speedyangular = 0
	        if(isInertial(repOp$econfName))
                	speedyangular = dynamics$angleSpeed[start:end]
		*/

		dis_l = UtilList.ListGetFromToIncluded (dis_l, 0, end);
		speed_l = UtilList.ListGetFromToIncluded (speed_l, 0, end);
		accel_l = UtilList.ListGetFromToIncluded (accel_l, 0, end);
		force_l = UtilList.ListGetFromToIncluded (force_l, 0, end);
		power_l = UtilList.ListGetFromToIncluded (power_l, 0, end);
                //also returned massTotal & speedyangular
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
		massBody = getMassBodyByExercise (massBody, exercisePercentBodyWeight);

		if (
				econfName == EncoderConfiguration.Names.WEIGHTEDMOVPULLEYLINEARONPERSON1 ||
				econfName == EncoderConfiguration.Names.WEIGHTEDMOVPULLEYLINEARONPERSON1INV ||
				econfName == EncoderConfiguration.Names.WEIGHTEDMOVPULLEYLINEARONPERSON2 ||
				econfName == EncoderConfiguration.Names.WEIGHTEDMOVPULLEYLINEARONPERSON2INV ||
				econfName == EncoderConfiguration.Names.WEIGHTEDMOVPULLEYROTARYFRICTION ||
				econfName == EncoderConfiguration.Names.WEIGHTEDMOVPULLEYROTARYAXIS)
		{
			massExtra = getMass (massExtra);
		}

		massTotal = massBody + massExtra;
		getDynamicsSpecific ();
	}

	protected abstract void getDynamicsSpecific ();

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

	public List<double> Dis_l {
		get { return dis_l; }
	}
	public List<double> Speed_l {
		get { return speed_l; }
	}
	public List<double> Force_l {
		get { return force_l; }
	}
	public List<double> Power_l {
		get { return power_l; }
	}
	public double MassTotal {
		get { return massTotal; }
	}
	/*
	   public double Speedyangular {
	   get { return speedyangular = 0; }
	   }
	   */
}

public class EncoderLikeRKinematicsNotInertial : EncoderLikeRKinematics
{
	//constructor
	public EncoderLikeRKinematicsNotInertial ()
	{
	}

	protected override void getDynamicsSpecific ()
	{
		force_l = new List<double> ();
		power_l = new List<double> ();

		for (int i = 0; i < accel_l.Count ; i ++)
		{
			double a = accel_l[i];
			double f = 0;

			if (econfName == EncoderConfiguration.Names.LINEARONPLANEWEIGHTDIFFANGLE)
			{
				f = massBody * (a + g * Math.Sin (anglePush * pi / 180)) + massExtra * (a + g * Math.Sin (angleWeight * pi / 180));
			}
			else if (econfName == EncoderConfiguration.Names.LINEARONPLANEWEIGHTDIFFANGLEMOVPULLEY)
			{
				f = massBody * (a + g * Math.Sin (anglePush * pi / 180)) + massExtra * (g * Math.Sin (angleWeight * pi / 180) + a) / gearedDown;
			}
			else if (econfName == EncoderConfiguration.Names.LINEARONPLANE)
			{
				f = (massBody + massExtra) *  (a + g * Math.Sin (anglePush * pi / 180));
			}
			else if (econfName == EncoderConfiguration.Names.PNEUMATIC)
			{
				//The force generated by the machine is supposed to be constant
				f = massExtra * g + massBody * (a + g * Math.Sin (anglePush * pi / 180));
			} else {
				//g:9.81 (used when movement is against gravity)
				f = massTotal * (a + g);
			}
			force_l.Add (f);
			power_l.Add (f * speed_l[i]);
		}
	}
}

public class EncoderLikeRKinematicsInertial : EncoderLikeRKinematics
{
	/*
	diameter: diameter of the axis (where string is wrapped)
	angle: angle (rotation of disc) in radians
	angleSpeed: speed of angle
	angleAccel: acceleration of angle
	encoderConfiguration:
		LINEARINERTIAL Linear encoder on inertial machine (rolled on axis)
		ROTARYFRICTIONSIDEINERTIAL Rotary friction encoder connected to inertial machine on the side of the disc
		ROTARYFRICTIONAXISINERTIAL Rotary friction encoder connected to inertial machine on the axis
		ROTARYAXISINERTIAL Rotary axis encoder  connected to inertial machine on the axis
		ROTARYAXISINERTIALMOVPULLEY Rotari axis encoder connected to inertial machine on the axis and the subject pulling from a moving pulley
	ROTARYAXISINERTIALLATERALMOVPULLEY Rotari axis encoder connected to inertial machine on the axis and the subject pulling from a moving pulley and performing and horizontal movement
	*/

	//passed variables
	private double diameter;
	private double inertiaMomentum;

	private List<double> posM_l;
	private List<double> angle_l;
	private List<double> angleSpeed_l;
	private List<double> angleAccel_l;
	private List<double> forceDisc_l;
	private List<double> forceBody_l;
	private List<double> powerDisc_l;
	private List<double> powerBody_l;
	private double diameterM;
	private double anglePush;

	//constructor
	public EncoderLikeRKinematicsInertial (double diameter,  double inertiaMomentum)
	{
		this.diameter = diameter;
		this.inertiaMomentum = inertiaMomentum;
	}

	protected override void getDynamicsSpecific ()
	{
		force_l = new List<double> ();
		power_l = new List<double> ();

		//avoid division by zero problems
		if (diameter == 0)
			return;

		initVariables ();
		calculateDynamics ();
	}

	private void initVariables ()
	{
		// let's work with SI now

		List<double> pos_l = UtilList.Cumsum (dis_l);

		posM_l = new List<double> ();
		foreach (double d in pos_l)
			posM_l.Add (Math.Abs (d / 1000)); // meters

		diameterM = diameter / 100;	// cm -> m

		angle_l = new List<double>();
		angleSpeed_l = new List<double>();
		angleAccel_l = new List<double>();
		forceDisc_l = new List<double>();
		forceBody_l = new List<double>();
		powerDisc_l = new List<double>();
		powerBody_l = new List<double>();
	}

	private void calculateDynamics ()
	{
		if (
				econfName == EncoderConfiguration.Names.ROTARYAXISINERTIAL ||
				econfName == EncoderConfiguration.Names.ROTARYFRICTIONSIDEINERTIAL ||
				econfName == EncoderConfiguration.Names.ROTARYFRICTIONAXISINERTIAL ||
				econfName == EncoderConfiguration.Names.LINEARINERTIAL)
		{
			calculateDynamicsDefault ();
		}
		else if (
				econfName == EncoderConfiguration.Names.ROTARYAXISINERTIALMOVPULLEY ||
				econfName == EncoderConfiguration.Names.ROTARYFRICTIONAXISINERTIALMOVPULLEY ||
				econfName == EncoderConfiguration.Names.ROTARYFRICTIONSIDEINERTIALMOVPULLEY)
		{
			calculateDynamicsMovPulley ();
		}
		else if (
				econfName == EncoderConfiguration.Names.ROTARYAXISINERTIALLATERAL ||
				econfName == EncoderConfiguration.Names.ROTARYFRICTIONAXISINERTIALLATERAL ||
				econfName == EncoderConfiguration.Names.ROTARYFRICTIONSIDEINERTIALLATERAL ||
				econfName == EncoderConfiguration.Names.ROTARYAXISINERTIALLATERALMOVPULLEY)
		{
			calculateDynamicsLateral ();
		}

		for (int i = 0; i < force_l.Count; i ++)
		{
			force_l.Add ((forceDisc_l[i] / gearedDown) * forceBody_l[i]);
			power_l.Add (powerDisc_l[i] + powerBody_l[i]);
		}
		/*
		//TODO:
		loopsMax = diameter.m * max(angle)
		loopsAblines = seq(from=0, to=loopsMax, by=diameter.m*pi)
		*/
	}

	private void calculateDynamicsDefault ()
	{
		anglePush = 90; // TODO: send from C#

		for (int i = 0; i < posM_l.Count; i ++)
		{
			angle_l.Add (posM_l[i] * 2 / diameterM);
			angleSpeed_l.Add (speed_l[i] * 2 / diameterM);
			angleAccel_l.Add (accel_l[i] * 2 / diameterM);

			forceDisc_l.Add (Math.Abs (inertiaMomentum * angleAccel_l[i]) * (2 / diameterM));
			forceBody_l.Add (massTotal * (Math.Abs (accel_l[i]) + g * Math.Sin (anglePush * pi / 180)));
			powerDisc_l.Add (Math.Abs ((inertiaMomentum * angleAccel_l[i]) * angleSpeed_l[i]));
			powerBody_l.Add (Math.Abs (massTotal * (accel_l[i] + g * Math.Sin (anglePush * pi / 180)) * speed_l[i]));
		}
	}

	private void calculateDynamicsMovPulley ()
	{
		anglePush = 90; // TODO: send from C#

		// With a moving pulley, the displacement of the body is half of the displacement of the rope in the inertial machine side
		// So, the multiplier is 4 instead of 2 to get the rotational kinematics.

		for (int i = 0; i < posM_l.Count; i ++)
		{
			angle_l.Add (posM_l[i] * 2 / gearedDown / diameterM);
			angleSpeed_l.Add (speed_l[i] * 2 / gearedDown / diameterM);
			angleAccel_l.Add (accel_l[i] * 2 / gearedDown / diameterM);
			// The configuration covers horizontal, vertical and inclined movements
			// If the movement is vertical g*sin(alpha) = g
			// If the movement is horizontal g*sin(alpha) = 0

			forceDisc_l.Add (Math.Abs (inertiaMomentum * angleAccel_l[i]) * (2 / diameterM));
			forceBody_l.Add (massTotal * (Math.Abs (accel_l[i]) + g * Math.Sin (anglePush * pi / 180)));
			powerDisc_l.Add (Math.Abs ((inertiaMomentum * angleAccel_l[i]) * angleSpeed_l[i]));
			powerBody_l.Add (Math.Abs (massTotal * (accel_l[i] + g * Math.Sin (anglePush * pi / 180)) * speed_l[i]));
		}
	}

	private void calculateDynamicsLateral ()
	{
		anglePush = 0; // TODO: send from C#

		for (int i = 0; i < posM_l.Count; i ++)
		{
			angle_l.Add (posM_l[i] * 2 / gearedDown / diameterM);
			angleSpeed_l.Add (speed_l[i] * 2 / gearedDown / diameterM);
			angleAccel_l.Add (accel_l[i] * 2 / gearedDown / diameterM);

			forceDisc_l.Add (Math.Abs (inertiaMomentum * angleAccel_l[i]) * (2 / diameterM));
			forceBody_l.Add (massTotal * Math.Abs (accel_l[i]));
			powerDisc_l.Add (Math.Abs ((inertiaMomentum * angleAccel_l[i]) * angleSpeed_l[i]));
			powerBody_l.Add (Math.Abs (massTotal * accel_l[i] * speed_l[i]));
		}
	}
}
