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
 *  Copyright (C) 2019-2020   Xavier de Blas <xaviblas@gmail.com>
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

	protected void initialize(bool isElastic,
			List<int> time_micros_l, List<double> force_l,
			ForceSensor.CaptureOptions fsco, ForceSensorExercise fse,
			double personMass, double stiffness,
			double eccMinDisplacement, double conMinDisplacement)
	{
		this.isElastic = isElastic;
		this.time_micros_l = time_micros_l;
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
		LogB.Information(string.Format("size of force_l: {0}", force_l.Count));
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

	protected abstract void calculeResultant ();

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
		//bool firstPhase = true;

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
		if(fse.RepetitionsShow == ForceSensorExercise.RepetitionsShowTypes.CONCENTRIC && concentricFlag == -1)
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
		sampleStart = sampleStart -RemoveNValues ;//-1;  this -1 makes the rep have start erroneously 1 sample before
		if(sampleStart < 0)
			sampleStart = 0;

		sampleEnd = sampleEnd -RemoveNValues ;//-1;  this -1 makes the rep have end erroneously 1 sample before
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

	protected ForceSensorRepetition.Types getForceSensorRepetitionType (double ySampleStart, double ySampleEnd)
	{
		ForceSensorRepetition.Types fsrType = ForceSensorRepetition.Types.DONOTSHOW;
		if(fse.RepetitionsShow != ForceSensorExercise.RepetitionsShowTypes.CONCENTRIC)
		{
			if(ySampleEnd < ySampleStart)
				fsrType = ForceSensorRepetition.Types.ECC;
			else
				fsrType = ForceSensorRepetition.Types.CON;
		}

		return fsrType;
	}

	protected abstract void addRepetition(List <double> yList, int sampleStart, int sampleEnd);

	public void CutSamplesForZoom(int startAtSample, int endAtSample)
	{
		//1 cut force_l on not elastic and all the lists on elastic
		cutSamplesForZoomDo(startAtSample, endAtSample);
		LogB.Information(" csfz 2b ");

		//2 cut repetitions
		List<ForceSensorRepetition> zoomed_forceSensorRepetition_l = new List <ForceSensorRepetition>();
		foreach(ForceSensorRepetition fsr in forceSensorRepetition_l)
			if(fsr.sampleStart >= startAtSample && fsr.sampleEnd <= endAtSample)
				zoomed_forceSensorRepetition_l.Add(fsr);

		forceSensorRepetition_l = zoomed_forceSensorRepetition_l;
	}
	protected abstract void cutSamplesForZoomDo(int startAtSample, int endAtSample);

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
		initialize(false, time_micros_l, force_l, fsco, fse, personMass, stiffness, eccMinDisplacement, conMinDisplacement);
		removeFirstValue();

		if(! fse.ForceResultant)
		{
			calculeForceWithCaptureOptionsFullSet();
			calculeRepetitions(force_l);

			return;
		}

		calculeResultant ();
	}

	//forces are updated, so do not Add to the list
	protected override void calculeResultant ()
	{
		for (int i = 0 ; i < force_l.Count; i ++)
		{
			/*
			double force = Math.Sqrt(
					Math.Pow(Math.Cos(fse.AngleDefault * Math.PI / 180.0) * (Math.Abs(force_l[i])), 2) +                  //Horizontal
					Math.Pow(Math.Sin(fse.AngleDefault * Math.PI / 180.0) * (Math.Abs(force_l[i])) + totalMass * 9.81, 2) //Vertical
					);
			*/

			//on 2.2.1 ABS or inverted is not done on forceResultant,
			//is done on force coming from the sensor
			force_l[i] = calculeForceWithCaptureOptions(force_l[i]);
			double force = force_l[i]  +  totalMass * 9.81 * Math.Sin(fse.AngleDefault * Math.PI / 180.0);
			//force_l[i] = calculeForceWithCaptureOptions(force);
			force_l[i] = force;
		}

		calculeRepetitions(force_l);
	}

	protected override void addRepetition(List <double> yList, int sampleStart, int sampleEnd)
	{
		//TODO: need to cut reps with low force prolonged at start or end

		ForceSensorRepetition.Types fsrType = getForceSensorRepetitionType (yList[sampleStart], yList[sampleEnd]);

		//  modify previous repetition if this repetition is ECC and fse.RepetitionsShow == ForceSensorExercise.RepetitionsShowTypes.BOTHTOGETHER
		if(fsrType == ForceSensorRepetition.Types.ECC &&
				fse.RepetitionsShow == ForceSensorExercise.RepetitionsShowTypes.BOTHTOGETHER &&
				forceSensorRepetition_l.Count > 0)
		{
			forceSensorRepetition_l[forceSensorRepetition_l.Count -1].sampleEnd = sampleEnd;
			forceSensorRepetition_l[forceSensorRepetition_l.Count -1].type = ForceSensorRepetition.Types.DONOTSHOW;
		} else
			forceSensorRepetition_l.Add(new ForceSensorRepetition(sampleStart, sampleEnd, fsrType));
	}

	protected override void cutSamplesForZoomDo(int startAtSample, int endAtSample)
	{
		LogB.Information(string.Format("cutSamplesForZoomDo, force_l.Count: {0}, startAtSample: {1}, endAtSample: {2}, endAtSample - startAtSample: {3}",
			force_l.Count, startAtSample, endAtSample, endAtSample - startAtSample));

		//+1 because we want both,the start and the end
		force_l = force_l.GetRange(startAtSample, endAtSample - startAtSample + 1);
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
	private bool zoomed;

	public ForceSensorDynamicsElastic (List<int> time_micros_l, List<double> force_l, 
			ForceSensor.CaptureOptions fsco, ForceSensorExercise fse,
			double personMass, double stiffness,
			double eccMinDisplacement, double conMinDisplacement,
			bool zoomed)
	{
		RemoveNValues = 10;
		initialize(true, time_micros_l, force_l, fsco, fse, personMass, stiffness, eccMinDisplacement, conMinDisplacement);
		convertTimeToSeconds(time_micros_l);
		removeFirstValue();
		this.zoomed = zoomed;

		if(! fse.ForceResultant)
		{
			calculeForceWithCaptureOptionsFullSet();
			calculeRepetitions(force_l);

			return;
		}

		//2.2.1 on resultant, ABS or INVERTED is done by data coming from the sensor
		for (int i = 0 ; i < force_l.Count; i ++)
			force_l[i] = calculeForceWithCaptureOptions(force_l[i]);

		calculeResultant ();
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

	protected override void calculeResultant ()
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

	//TODO: now not need to change because it works, but for future code use: UtileMath.MovingAverage
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
		int window = 10;
		for (int i = 0 ; i < speed_l.Count; i ++)
		{
			int pre = i - window;
			int post = i + window;

			if(pre <= 0)
				pre = 0;
			else if(post >= speed_l.Count -1)
				post = speed_l.Count -1;

			accel_l.Add( UtilAll.DivideSafe(speed_l[post] - speed_l[pre], time_l[post] - time_l[pre]) );
                }
		accel_l = smoothVariable(accel_l);
	}

	//forces are updated, so do not Add to the list
	private void calculeForces()
	{
		LogB.Information("elastic calculeForces: " + fsco.ToString());
		for (int i = 0 ; i < force_l.Count; i ++)
		{
			LogB.Information(string.Format("i pre: {0}, force_l[i]: {1}, accel_l[i]: {2}", i, force_l[i], accel_l[i]));
			//LogB.Information(string.Format("i: {0}, force_l[i]: {1}, force_l.Count: {2}", i, force_l[i], force_l.Count));
			/*
			double force = Math.Sqrt(
					Math.Pow(Math.Cos(fse.AngleDefault * Math.PI / 180.0) * (Math.Abs(force_l[i]) + totalMass * accel_l[i]), 2) +                  //Horizontal
					Math.Pow(Math.Sin(fse.AngleDefault * Math.PI / 180.0) * (Math.Abs(force_l[i]) + totalMass * accel_l[i]) + totalMass * 9.81, 2) //Vertical
					);
			 */

			double force = force_l[i]  +  totalMass*(accel_l[i] + 9.81 * Math.Sin(fse.AngleDefault * Math.PI / 180.0));

			LogB.Information(string.Format("post force (but before applying captureoptions): {0}", force));

			//force_l[i] = calculeForceWithCaptureOptions(force); //
			//2.2.1 this is applied now at constructor
			force_l[i] = force;

			LogB.Information(string.Format("post force (after applying captureoptions): {0}", force_l[i]));
		}
	}
	
	private void calculePowers()
	{	
		for (int i = 0 ; i < force_l.Count; i ++)
		{
			power_l.Add(
					//speed_l[i] * (force_l[i] + totalMass * accel_l[i]) + //Power associated to the acceleration of the mass
					//speed_l[i] * (Math.Sin(fse.AngleDefault * Math.PI / 180.0) * totalMass * 9.81) //Power associated to the gravitatory field
                    speed_l[i]*force_l[i]
				   );
		}
	}

	protected override void addRepetition(List <double> yList, int sampleStart, int sampleEnd)
	{
		//TODO: need to cut reps with low force prolonged at start or end

		ForceSensorRepetition.Types fsrType = getForceSensorRepetitionType (yList[sampleStart], yList[sampleEnd]);

		//  modify previous repetition if this repetition is ECC and fse.RepetitionsShow == ForceSensorExercise.RepetitionsShowTypes.BOTHTOGETHER
		if(fsrType == ForceSensorRepetition.Types.ECC &&
				fse.RepetitionsShow == ForceSensorExercise.RepetitionsShowTypes.BOTHTOGETHER &&
				forceSensorRepetition_l.Count > 0)
		{
			forceSensorRepetition_l[forceSensorRepetition_l.Count -1].sampleEnd = sampleEnd;
			forceSensorRepetition_l[forceSensorRepetition_l.Count -1].type = ForceSensorRepetition.Types.DONOTSHOW;

			int sampleStartPreviousRep = forceSensorRepetition_l[forceSensorRepetition_l.Count -1].sampleStart;

			forceSensorRepetition_l[forceSensorRepetition_l.Count -1].RFD =
				(force_l[sampleEnd] - force_l[sampleStartPreviousRep]) / (time_l[sampleEnd] - time_l[sampleStartPreviousRep]);

			forceSensorRepetition_l[forceSensorRepetition_l.Count -1].meanSpeed =
				(yList[sampleEnd] - yList[sampleStartPreviousRep]) /  (time_l[sampleEnd] - time_l[sampleStartPreviousRep]);
		} else
		{
			//Calculate mean RFD and mean speed of the phase
			double lastRFD = (force_l[sampleEnd] - force_l[sampleStart]) / (time_l[sampleEnd] - time_l[sampleStart]);
			double lastMeanSpeed = (yList[sampleEnd] - yList[sampleStart]) / (time_l[sampleEnd] - time_l[sampleStart]);

			forceSensorRepetition_l.Add(new ForceSensorRepetition(sampleStart, sampleEnd, fsrType, lastMeanSpeed, lastRFD));
		}
	}

	protected override void cutSamplesForZoomDo(int startAtSample, int endAtSample)
	{
		//to cut, shift both values at right in order to be the same sample in/out zoom
		startAtSample += RemoveNValues +1;
		endAtSample += RemoveNValues +1;

		//+1 because we want both,the start and the end
		force_l = force_l.GetRange(startAtSample, endAtSample - startAtSample + 1);
		position_l = position_l.GetRange(startAtSample, endAtSample - startAtSample + 1);
		speed_l = speed_l.GetRange(startAtSample, endAtSample - startAtSample + 1);
		accel_l = accel_l.GetRange(startAtSample, endAtSample - startAtSample + 1);
		power_l = power_l.GetRange(startAtSample, endAtSample - startAtSample + 1);
	}

	private List<double> stripStartEnd(List<double> l)
	{
		if(zoomed) {
			//values have been shifted at cutSamplesForZoomDo
			return l;
		} else {
			LogB.Information(string.Format("removeN: {0}, l.Count: {1}", RemoveNValues, l.Count));
			return l.GetRange(RemoveNValues +1, l.Count - 2*RemoveNValues);
		}
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

	//if(!fse.EccReps), then DONOTSHOW
	//if(fse.RepetitionsShow == ForceSensorExercise.RepetitionsShowTypes.CONCENTRIC), then DONOTSHOW
	public enum Types { DONOTSHOW, CON, ECC }
	public Types type;

	public bool con; // false is ecc
	public double meanSpeed;
	public double RFD;

	//not elastic
	public ForceSensorRepetition(int sampleStart, int sampleEnd, Types type)
	{
		this.sampleStart = sampleStart;
		this.sampleEnd = sampleEnd;
		this.type = type;
		this.meanSpeed = 0;
		this.RFD = 0;
	}
	//elastic
	public ForceSensorRepetition(int sampleStart, int sampleEnd, Types type, double meanSpeed, double RFD)
	{
		this.sampleStart = sampleStart;
		this.sampleEnd = sampleEnd;
		this.type = type;
		this.meanSpeed = meanSpeed;
		this.RFD = RFD;
	}

	public override string ToString()
	{
		return string.Format("sampleStart:{0}; sampleEnd:{1}; meanSpeed:{2}; RFD:{3}", sampleStart, sampleEnd, meanSpeed, RFD);
	}

	public ForceSensorRepetition Clone ()
	{
		ForceSensorRepetition fsr = new ForceSensorRepetition (sampleStart, sampleEnd, type, meanSpeed, RFD);
		fsr.con = con;
		return fsr;
	}

	/*
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
	*/

	public static string GetRepetitionCodeFromList(List<ForceSensorRepetition> l, int sampleEnd, ForceSensorExercise.RepetitionsShowTypes repetitionsShow)
	{
		int rep = 1;
		foreach(ForceSensorRepetition fsr in l)
		{
			if(sampleEnd >= fsr.sampleStart && sampleEnd <= fsr.sampleEnd)
			{
				LogB.Information(string.Format("fsr.sampleStart: {0}; fsr.sampleEnd: {1}; sampleEnd: {2}; rep: {3}", fsr.sampleStart, fsr.sampleEnd, sampleEnd, rep));
				if(repetitionsShow == ForceSensorExercise.RepetitionsShowTypes.CONCENTRIC ||
						repetitionsShow == ForceSensorExercise.RepetitionsShowTypes.BOTHTOGETHER ||
						rep == 0)
					return rep.ToString();
				else
					return string.Format("{0}{1}", Math.Ceiling(rep/2.0), typeShort(fsr.type));
			}
			rep ++;
		}

		/*
		if(! eccReps || rep == 0)
			return rep.ToString();
		else
			return string.Format("{0}{1}", Math.Ceiling(rep/2.0), typeShort(((ForceSensorRepetition) l[l.Count - 1]).type));
		*/
		return "0";
	}

	private static string typeShort(Types type)
	{
		if(type == Types.CON)
			return "c";
		else if (type == Types.ECC)
			return "e";
		else
			return "";
	}

	public string TypeShort()
	{
		if(type == Types.CON)
			return "c";
		else if (type == Types.ECC)
			return "e";
		else
			return "";
	}
}
