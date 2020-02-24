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
 *  Copyright (C) 2019   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Collections.Generic; //List<T>

public abstract class ForceSensorDynamics
{
	//Position Speed Accel Power, boolena to know if we have to gather these lists
	public bool CalculedElasticPSAP;

	/*
	 * Used for elastic. Minimum has to be 1
	 * values to be removed at the beginning, and also at the end:
	 * 10 will be 10 at start, 10 at end.
	 */
	public int RemoveNValues = 0;

	protected bool isElastic;
	protected List<int> time_micros_l;
	protected List<double> force_l;
	protected List<ForceSensorRepetition> forceSensorRepetition_l;
	protected ForceSensor.CaptureOptions fsco;
	protected ForceSensorExercise fse;
	protected double stiffness;
	protected double totalMass;

	protected double eccMinDisplacement;
	protected double conMinDisplacement;

	protected List<double> time_l;

	protected void initialize(bool isElastic, List<double> force_l,
			ForceSensor.CaptureOptions fsco, ForceSensorExercise fse,
			double personMass, double stiffness,
			double eccMinDisplacement, double conMinDisplacement)
	{
		this.isElastic = isElastic;
		this.force_l = force_l;
		this.fsco = fsco;
		this.fse = fse;
		this.stiffness = stiffness;
		this.eccMinDisplacement = eccMinDisplacement;
		this.conMinDisplacement = conMinDisplacement;

		totalMass = 0;
		if(fse.PercentBodyWeight > 0 && personMass > 0)
			totalMass = fse.PercentBodyWeight * personMass / 100.0;

		CalculedElasticPSAP = false;
		forceSensorRepetition_l = new List<ForceSensorRepetition>();
	}

	//first value has to be removed also on not elastic because time between first and second is so short
	//so is better to remove it
	protected virtual void removeFirstValue()
	{
		force_l.RemoveAt(0);
	}

	protected double calculeForceWithCaptureOptions(double force)
	{
		if(fsco == ForceSensor.CaptureOptions.ABS)
			return Math.Abs(force);
		if(fsco == ForceSensor.CaptureOptions.INVERTED)
			return -1 * force;

		return force;
	}
	protected void calculeForceWithCaptureOptionsFullSet()
	{
		for(int i = 0 ; i < force_l.Count; i ++)
			force_l[i] = calculeForceWithCaptureOptions(force_l[i]);
	}

	//adapted from r-scripts/forcePosition.R
	//yList is position_not_smoothed_l on elastic
	//yList is force_l on not elastic
	protected void calculeRepetitions(List<double> yList)
	{
		double minDisplacement;

		//The comments supposes that the current phase is concentric. In the case that the phase is eccentric
		//the signal is inverted by multiplying it by -1.

		//for each phase, stores the sample number of the biggest current sample.
		int possibleExtremeSample = 0;

		//Stores the sample of the last actual maximum of the phase
		int lastExtremeSample = 0;

		int currentSample = 1;

		//The firstPhase is treated different
		bool firstPhase = true;

		// to find if there is a previous extreme than first one with minDisplacement
		bool searchingFirstExtreme = true;
		int minimumPosBeforeFirstExtreme = 1;
		int maximumPosBeforeFirstExtreme = 1;
		double minimumValueBeforeFirstExtreme = yList[minimumPosBeforeFirstExtreme];
		double maximumValueBeforeFirstExtreme = yList[maximumPosBeforeFirstExtreme];

		int concentricFlag; //1: concentric; -1: excentric

		//Detecting the first phase type
		if(yList[currentSample] > yList[possibleExtremeSample])
		{
			concentricFlag = 1;
			minDisplacement = eccMinDisplacement;
		} else {
			concentricFlag = -1;
			minDisplacement = conMinDisplacement;
		}

		while(currentSample < yList.Count)
		{
			if(searchingFirstExtreme)
			{
				if(yList[currentSample] > maximumValueBeforeFirstExtreme)
				{
					maximumValueBeforeFirstExtreme = yList[currentSample];
					maximumPosBeforeFirstExtreme = currentSample;
				}
				if(yList[currentSample] < minimumValueBeforeFirstExtreme)
				{
					minimumValueBeforeFirstExtreme = yList[currentSample];
					minimumPosBeforeFirstExtreme = currentSample;
				}
			}

			//Checking if the current position is greater than the previous possible maximum
			if(concentricFlag * yList[currentSample] > concentricFlag * yList[possibleExtremeSample])
			{
				//The current sample is the new candidate to be a maximum
				//LogB.Information(string.Format("updated possibleExtremeSample to: {0}; position: {1}", currentSample, yList[currentSample]));
				possibleExtremeSample = currentSample;
			}

			//Checking if the current position is at minDisplacement below the last possible extreme
			if( concentricFlag * yList[currentSample] - concentricFlag * yList[possibleExtremeSample] < - minDisplacement)
			{
				//possibleExtremeSample is now the new extreme

				//firstExtreme will find if there is a previous extreme with minDisplacement
				if(searchingFirstExtreme)
				{
					int samplePreFirst = minimumPosBeforeFirstExtreme;
					if(concentricFlag == -1)
						samplePreFirst = maximumPosBeforeFirstExtreme;

					if(samplePreFirst < possibleExtremeSample && displacementOk(concentricFlag, yList[possibleExtremeSample], yList[samplePreFirst]))
					{
						//lastExtremeSample = possibleExtremeSample;
						lastExtremeSample = samplePreFirst;//TODO: try it
					}
					searchingFirstExtreme = false;
				}

				LogB.Information("care to fix problem of sending both extremes at pos 0 on the first rep (caused by samplePreFirst code)");
				if(possibleExtremeSample > lastExtremeSample)
					prepareCheckAndSendRepetition(concentricFlag, yList, lastExtremeSample, possibleExtremeSample);

				//Save the sample of the last extreme in order to compare new samples with it
				lastExtremeSample = possibleExtremeSample;

				//Changing the phase from concentric to eccentric or viceversa
				concentricFlag *= -1;
				if (concentricFlag > 0)
					minDisplacement = eccMinDisplacement;
				else
					minDisplacement = conMinDisplacement;
			}

			currentSample += 1;
		}

		if(possibleExtremeSample > lastExtremeSample)
			prepareCheckAndSendRepetition(concentricFlag, yList, lastExtremeSample, possibleExtremeSample);
	}

	private void prepareCheckAndSendRepetition(int concentricFlag, List<double> yList, int sampleStart, int sampleEnd)
	{
		LogB.Information(string.Format("prepareCheckAndSendRepetition params: concentricFlag: {0}, yList.Count: {1}, sampleStart: {2}, sampleEnd: {3}",
				concentricFlag, yList.Count, sampleStart, sampleEnd));

		//should accept eccentric reps?
		if(! fse.EccReps && concentricFlag == -1)
			return;

		// 1) remove low force at beginning ot end of the repetition
		double maxAbs = 0;

		//List with absolute values
		List<double> yListAbs = new List<double>();

		//List with absolute values reversed to find end of rep
		List<double> yListAbsRev = new List<double>();

		for(int i = 0; i < yList.Count ; i ++)
		{
			double abs = Math.Abs(yList[i]);
			yListAbs.Add(abs);
			if (abs > maxAbs)
				maxAbs = abs;
		}

		sampleStart = findLocalExtreme(yListAbs, sampleStart, sampleEnd, maxAbs);

		for(int i = yListAbs.Count -1; i >= 0 ; i --)
			yListAbsRev.Add(yListAbs[i]);

		sampleEnd = yList.Count - findLocalExtreme(yListAbsRev,
				yList.Count - sampleEnd, yList.Count - sampleStart, maxAbs);

		// 2) remove values to avoid smoothing problems:
		sampleStart = sampleStart -RemoveNValues -1;
		if(sampleStart < 0)
			sampleStart = 0;

		sampleEnd = sampleEnd -RemoveNValues -1;
		if(sampleEnd < 0)
			return;

		// 3) check that end does not outside the after forceSensor.cs cut:
		//    times = times.GetRange(forceSensorDynamics.RemoveNValues -1, times.Count -2*forceSensorDynamics.RemoveNValues);

		if(sampleEnd >= yList.Count - 2*RemoveNValues)
			sampleEnd = yList.Count - 2*RemoveNValues -1;

		if(sampleEnd < 0 || sampleStart >= sampleEnd)
			return;

		// 4) check if displacement is ok, and add the repetition
		if(displacementOk(concentricFlag, yList[sampleStart], yList[sampleEnd]))
		{
			LogB.Information(" Adding repetition ");
			addRepetition(yList, sampleStart, sampleEnd);
		}
	}

	/*
	 * this finds local extreme on concentric at the beginning of the "real" phase
	 * data comes in Abs
	 * if eccentric: yList comes reversed (left - right)
	 */
	private int findLocalExtreme(List<double> yList, int sampleStart, int sampleEnd, double maxAbs)
	{
		LogB.Information(string.Format("findLocalExtreme params: yList.Count: {0}, sampleStart: {1}, sampleEnd: {2}, maxAbs: {3}",
				yList.Count, sampleStart, sampleEnd, maxAbs));

		int i = sampleStart;
		//threshold for "non-force" segment at 2.5% of max
		while(Math.Abs(yList[i]) < .025 * maxAbs && i < sampleEnd)
			i ++;

		//find the lowest value at 70% end of the "non-force" segment
		int startAt = Convert.ToInt32(sampleStart + (.7 * (i - sampleStart)));
		int minLocalPos = startAt;
		double minLocal = yList[startAt];

		for(int j = startAt; j <= i; j ++)
		{
			if(yList[j] < minLocal) {
				minLocal = yList[j];
				minLocalPos = j;
			}
		}
		return minLocalPos;
	}

	private bool displacementOk (int concentricFlag, double sampleStart, double sampleEnd)
	{
		if ( ( concentricFlag == 1 && Math.Abs(sampleStart - sampleEnd) >= conMinDisplacement) ||
			(concentricFlag == -1 && Math.Abs(sampleStart - sampleEnd) >= eccMinDisplacement) )
			return true;

		return false;
	}

	protected abstract void addRepetition(List <double> yList, int sampleStart, int sampleEnd);

	public virtual List<double> GetForces()
	{
		return force_l;
	}

	public List<ForceSensorRepetition> GetRepetitions()
	{
		return forceSensorRepetition_l;
	}

	//----- start of: only implemented on elastic ----->

	public virtual List<double> GetPositions()
	{
		return new List<double>();
	}

	public virtual List<double> GetSpeeds()
	{
		return new List<double>();
	}

	public virtual List<double> GetAccels()
	{
		return new List<double>();
	}

	public virtual List<double> GetPowers()
	{
		return new List<double>();
	}

	//<----- end of: only implemented on elastic -----

}
	
public class ForceSensorDynamicsNotElastic : ForceSensorDynamics
{
	public ForceSensorDynamicsNotElastic (List<int> time_micros_l, List<double> force_l, 
			ForceSensor.CaptureOptions fsco, ForceSensorExercise fse,
			double personMass, double stiffness,
			double eccMinDisplacement, double conMinDisplacement)
	{
		initialize(false, force_l, fsco, fse, personMass, stiffness, eccMinDisplacement, conMinDisplacement);
		removeFirstValue();

		if(! fse.ForceResultant)
		{
			calculeForceWithCaptureOptionsFullSet();
			calculeRepetitions(force_l);

			return;
		}

		calcule();
	}

	//forces are updated, so do not Add to the list
	private void calcule()
	{
		double accel = 0;

		for (int i = 0 ; i < force_l.Count; i ++)
		{
			double force = Math.Sqrt(
					Math.Pow(Math.Cos(fse.AngleDefault * Math.PI / 180.0) * (force_l[i] + totalMass * accel), 2) +                  //Horizontal
					Math.Pow(Math.Sin(fse.AngleDefault * Math.PI / 180.0) * (force_l[i] + totalMass * accel) + totalMass * 9.81, 2) //Vertical
					);
			force_l[i] = calculeForceWithCaptureOptions(force);
		}

		calculeRepetitions(force_l);
	}

	protected override void addRepetition(List <double> yList, int sampleStart, int sampleEnd)
	{
		//TODO: need to cut reps with low force prolonged at start or end

		forceSensorRepetition_l.Add(new ForceSensorRepetition(sampleStart, sampleEnd));
	}
}

public class ForceSensorDynamicsElastic : ForceSensorDynamics
{
	//this used to calcule the repetitions. All the values on this class are smoothed except this.
	List<double> position_not_smoothed_l;
	List<double> position_l;

	List<double> speed_l;
	List<double> accel_l;
	List<double> power_l;

	public ForceSensorDynamicsElastic (List<int> time_micros_l, List<double> force_l, 
			ForceSensor.CaptureOptions fsco, ForceSensorExercise fse,
			double personMass, double stiffness,
			double eccMinDisplacement, double conMinDisplacement)
	{
		RemoveNValues = 10;
		initialize(true, force_l, fsco, fse, personMass, stiffness, eccMinDisplacement, conMinDisplacement);
		convertTimeToSeconds(time_micros_l);
		removeFirstValue();

		if(! fse.ForceResultant)
		{
			calculeForceWithCaptureOptionsFullSet();
			calculeRepetitions(force_l);

			return;
		}

		calcule();
		CalculedElasticPSAP = true;
	}

	//time comes in microseconds
	private void convertTimeToSeconds(List<int> time_micros_l)
	{
		time_l = new List<double>();
		for (int i = 0 ; i < time_micros_l.Count; i ++)
		{
			time_l.Add(time_micros_l[i] / 1000000.0);
			//LogB.Information(string.Format("i: {0}, time_micros_l[i]: {1}, time_l: {2}", i, time_micros_l[i], time_l[i]));
		}
	}

	protected override void removeFirstValue()
	{
		force_l.RemoveAt(0);
		time_l.RemoveAt(0);
	}

	private void calcule()
	{
		//TODO: check minimum length of forces

		position_not_smoothed_l = new List<double>();
		position_l = new List<double>();
		speed_l = new List<double>();
		accel_l = new List<double>();
		power_l = new List<double>();

		calculePositions();
		calculeSpeeds();
		calculeAccels();
		calculeForces();
		calculePowers();
		calculeRepetitions(position_not_smoothed_l);
	}

	private int smoothFactor = 5; //use odd (impar) values like 5, 7, 9
	/*
	 * A smothFactor == 5, this will use 5 values: 2 previous, current value, 2 post.
	 * the calculated average is assigned to the current
	 */
	private List<double> smoothVariable(List<double> original_l)
	{
		List<double> smoothed_l = new List<double>();

		//a smoothFactor == 5 will iterate two times here:
		for(int i = 0; i < Math.Floor(smoothFactor /2.0); i ++)
			smoothed_l.Add(0);

		for(int i = 2; i < original_l.Count -2; i ++)
			smoothed_l.Add( (original_l[i-2] + original_l[i-1] + original_l[i] + original_l[i+1] + original_l[i+2]) / 5.0 );

		for(int i = 0; i < Math.Floor(smoothFactor /2.0); i ++)
			smoothed_l.Add(0);

		return smoothed_l;
	}

		
	private void calculePositions()
	{
		for (int i = 0 ; i < force_l.Count; i ++)
			position_not_smoothed_l.Add(force_l[i] / stiffness);

		position_l = smoothVariable(position_not_smoothed_l);
	}

	private void calculeSpeeds()
	{
		for (int i = 0 ; i < time_l.Count; i ++)
		{
			int pre = i - 1;
			int post = i + 1;

			if(i == 0)
				pre = 0;
			else if(i == time_l.Count -1)
				post = i;

			speed_l.Add( (position_l[post] - position_l[pre]) / (time_l[post] - time_l[pre]) );
		}

		speed_l = smoothVariable(speed_l);
	}

	private void calculeAccels()
	{
		for (int i = 0 ; i < speed_l.Count; i ++)
		{
			int pre = i - 1;
			int post = i + 1;

			if(i == 0)
				pre = 0;
			else if(i == speed_l.Count -1)
				post = i;

			accel_l.Add( (speed_l[post] - speed_l[pre]) / (time_l[post] - time_l[pre]) );
			//LogB.Information(string.Format("i: {0}, accel_l[i]: {1}", i, accel_l[i]));
		}

		accel_l = smoothVariable(accel_l);
	}

	//forces are updated, so do not Add to the list
	private void calculeForces()
	{
		LogB.Information("elastic calculeForces: " + fsco.ToString());
		for (int i = 0 ; i < force_l.Count; i ++)
		{
			LogB.Information(string.Format("i pre: {0}, force_l[i]: {1}", i, force_l[i]));
			//LogB.Information(string.Format("i: {0}, force_l[i]: {1}, force_l.Count: {2}", i, force_l[i], force_l.Count));
			double force = Math.Sqrt(
					Math.Pow(Math.Cos(fse.AngleDefault * Math.PI / 180.0) * (force_l[i] + totalMass * accel_l[i]), 2) +                  //Horizontal
					Math.Pow(Math.Sin(fse.AngleDefault * Math.PI / 180.0) * (force_l[i] + totalMass * accel_l[i]) + totalMass * 9.81, 2) //Vertical
					);
			LogB.Information(string.Format("i post: {0}, force: {1}", i, force));
			force_l[i] = calculeForceWithCaptureOptions(force);
		}
	}
	
	private void calculePowers()
	{	
		for (int i = 0 ; i < force_l.Count; i ++)
		{
			power_l.Add(
					speed_l[i] * (force_l[i] + totalMass * accel_l[i]) + //Power associated to the acceleration of the mass
					speed_l[i] * (Math.Sin(fse.AngleDefault * Math.PI / 180.0) * totalMass * 9.81) //Power associated to the gravitatory field
				   );
		}
	}

	protected override void addRepetition(List <double> yList, int sampleStart, int sampleEnd)
	{
		//TODO: need to cut reps with low force prolonged at start or end

		//Calculate mean RFD and mean speed of the phase
		double lastRFD = (force_l[sampleEnd] - force_l[sampleStart]) / (time_l[sampleEnd] - time_l[sampleStart]);
		double lastMeanSpeed = (yList[sampleEnd] - yList[sampleStart]) / (time_l[sampleEnd] - time_l[sampleStart]);

		forceSensorRepetition_l.Add(new ForceSensorRepetition(sampleStart, sampleEnd, lastMeanSpeed, lastRFD));
	}

	private List<double> stripStartEnd(List<double> l)
	{
		LogB.Information(string.Format("removeN: {0}, l.Count: {1}", RemoveNValues, l.Count));
		return l.GetRange(RemoveNValues -1, l.Count - 2*RemoveNValues);
	}

	public override List<double> GetForces()
	{
		return stripStartEnd(force_l);
	}

	public override List<double> GetPositions()
	{
		return stripStartEnd(position_l);
	}

	public override List<double> GetSpeeds()
	{
		return stripStartEnd(speed_l);
	}

	public override List<double> GetAccels()
	{
		return stripStartEnd(accel_l);
	}

	public override List<double> GetPowers()
	{
		return stripStartEnd(power_l);
	}
}

public class ForceSensorRepetition
{
	public int sampleStart; // this is sample, not graph in pixels.
	public int sampleEnd; // this is sample, not graph in pixels.
	public double meanSpeed;
	public double RFD;

	//not elastic
	public ForceSensorRepetition(int sampleStart, int sampleEnd)
	{
		this.sampleStart = sampleStart;
		this.sampleEnd = sampleEnd;
		this.meanSpeed = 0;
		this.RFD = 0;
	}
	//elastic
	public ForceSensorRepetition(int sampleStart, int sampleEnd, double meanSpeed, double RFD)
	{
		this.sampleStart = sampleStart;
		this.sampleEnd = sampleEnd;
		this.meanSpeed = meanSpeed;
		this.RFD = RFD;
	}

	public override string ToString()
	{
		return string.Format("sampleStart:{0}; sampleEnd:{1}; meanSpeed:{2}; RFD:{3}", sampleStart, sampleEnd, meanSpeed, RFD);
	}

	//gets repetition num form a list
	public static int GetRepetitionNumFromList(List<ForceSensorRepetition> l, int sampleEnd)
	{
		int rep = 0;
		foreach(ForceSensorRepetition fsr in l)
		{
			if(sampleEnd <= fsr.sampleEnd)
			{
				LogB.Information(string.Format("fsr.sampleStart: {0}; fsr.sampleEnd: {1}; sampleEnd: {2}; rep: {3}", fsr.sampleStart, fsr.sampleEnd, sampleEnd, rep));
				return rep;
			}

			rep ++;
		}
		return rep;
	}
}
