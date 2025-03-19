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

	List<double> disOrig_l; //just to debug
	List<int> time_l;
	List<double> speed_l;
	List<double> accel_l;
	Butterworth bw;

	private const double g = 9.81;
	private const double pi = 3.141593;

	public EncoderLikeRKinematics (
			List<double> dis_l, double butterworthFreq, string eccon, EncoderConfiguration.Names econfName,
			double massBody, double massExtra,
			int anglePush, int angleWeight, int exercisePercentBodyWeight)
	{
		this.dis_l = dis_l;
		this.butterworthFreq = butterworthFreq;
		this.eccon = eccon;
		this.disOrig_l = dis_l;

		calculateSpeed ();
		calculateAccel ();

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

			propulsiveEnd = findPropulsiveEnd (accel_l, disCON_l, maxSpeedTCON,
					econfName, anglePush, angleWeight,
					massBody, massExtra, exercisePercentBodyWeight
					);
		} //TODO: continue

		LogB.Information ("EncoderLikeRKinematics propulsiveEnd = " + propulsiveEnd.ToString ());
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

	// adapted from encoder/util.R
	private int findPropulsiveEnd (
			List<double> accel_l, List<double> disCON_l, int maxSpeedTCON,
			EncoderConfiguration.Names econfName, int anglePush, int angleWeight,
			double massBody, double massExtra, int exercisePercentBodyWeight)
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

