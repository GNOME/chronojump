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
 *  Copyright (C) 2018-2020   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using Mono.Unix;

public class EncoderRhythm
{
	public bool Active;
	public bool RepsOrPhases; //true is by repetition, using RepSeconds. False is by phases, using EccSeconds, ConSeconds
	public double RepSeconds;
	public double EccSeconds;
	public double ConSeconds;
	public double RestRepsSeconds; //rest between repetitions
	public bool RestAfterEcc; //rest after eccentric or concentric. Only applies to gravitatory

	//cluster stuff
	public int RepsCluster;
	public double RestClustersSeconds; //rest between clusters

	// Constructors ---------------------

	public EncoderRhythm()
	{
		Active = false;

		//default values
		RepsOrPhases = false; //it's always Phases (reps was not easy to follow the bar while doing ecc/con)
		RepSeconds = 2;
		EccSeconds = 1;
		ConSeconds = 1;

		RestRepsSeconds = 0;
		RestAfterEcc = true;

		RepsCluster = 1; //1 is default, minimum value and means "no use clusters"
		RestClustersSeconds = 6;
	}

	public EncoderRhythm(bool active, bool repsOrPhases,
			double repSeconds, double eccSeconds, double conSeconds,
			double restRepsSeconds, bool restAfterEcc,
			int repsCluster, double restClustersSeconds)
	{
		Active = active;
		RepsOrPhases = repsOrPhases;
		RepSeconds = repSeconds;
		EccSeconds = eccSeconds;
		ConSeconds = conSeconds;

		RestRepsSeconds = restRepsSeconds;
		RestAfterEcc = restAfterEcc;

		RepsCluster = repsCluster;
		RestClustersSeconds = restClustersSeconds;
	}

	// Public methods ------------------

	//this also affects to encoder automatic end time. Is deactivated if UseClusters()
	public bool UseClusters()
	{
		return (RepsCluster > 1);
	}

	//to show or not image_encoder_rhythm_rest
	public bool UseRest()
	{
		if(! Active)
			return false;

		if(RestRepsSeconds > 0)
			return true;

		if(UseClusters() && RestClustersSeconds > 0)
			return true;

		return false;
	}

	public double RestClustersForEncoderCaptureAutoEnding()
	{
		if(Active && UseClusters() && RestClustersSeconds > 0)
			return RestClustersSeconds;

		return 0;
	}

	public override string ToString()
	{
		return
			"EncoderRhythm:" +
			"\nActive: " + Active.ToString() +
			"\nRepsOrPhases: " + RepsOrPhases.ToString() +
			"\nRepSeconds: " + RepSeconds.ToString() +
			"\nEccSeconds: " + EccSeconds.ToString() +
			"\nConSeconds: " + ConSeconds.ToString() +
			"\nRestRepsSeconds: " + RestRepsSeconds.ToString() +
			"\nRestAfterEcc: " + RestAfterEcc.ToString() +
			"\nRepsCluster: " + RepsCluster.ToString() +
			"\nRestClustersSeconds: " + RestClustersSeconds.ToString() +
			"\n";
	}
}

/*
 * this is fixed rhythm starting when first repetition ends
 * easy to follow. not adaptative
 */
public class EncoderRhythmExecute
{
	public bool FirstPhaseDone;
	private DateTime phaseStartDT;
	private EncoderRhythm encoderRhythm;
	private int nreps;

	private double fraction;
	private string textRepetition;
	private string textRest;

	//private double fractionRest;

	//REPETITION is doing the repetition (no differentiate between ecc/con)
	//ECC is ECC phase using phases
	//CON is CON phase using phases
	//RESTRESP is rest between repetitions
	//RESTCLUSTER is rest between clusters
	enum phases { REPETITION, ECC, CON, RESTREP, RESTCLUSTER }
	phases currentPhase;

	private	bool gravitatory = true;
	/*
	 * on inertial rest is after ecc.
	 * on gravitatory rest can be after ecc or con (see RestAfterEcc)
	 */


	// Constructor ---------------------

	public EncoderRhythmExecute(EncoderRhythm encoderRhythm, bool gravitatory)
	{
		this.encoderRhythm = encoderRhythm;
		//this.eccon_ec = eccon_ec;
		this.gravitatory = gravitatory;

		FirstPhaseDone = false;
		textRepetition = "";
		textRest = "";

		phaseStartDT = DateTime.MinValue;
		nreps = 1; //start with 1 because it's the phase user will do before calling here
	}

	// Public methods ------------------

	public void FirstPhaseDo(bool up)
	{
		if(FirstPhaseDone)
			return;

		FirstPhaseDone = true;
		phaseStartDT = DateTime.Now;

		if(encoderRhythm.RepsOrPhases)
			currentPhase = getNextPhase(phases.REPETITION);
		else if(up)
			currentPhase = getNextPhase(phases.CON);
		else
			currentPhase = getNextPhase(phases.ECC);
	}

	//useful for fraction of the repetition and the rest time
	public void CalculateFractionsAndText()
	{
		//double fraction = 0;
		TimeSpan span = DateTime.Now - phaseStartDT;
		double phaseSeconds = span.TotalSeconds;

		//check if should end phase
		if(shouldEndPhase(phaseSeconds))
		{
			phaseStartDT = DateTime.Now;
			phaseSeconds = 0;

			//check if should end rep
			if(endPhaseShouldEndRep())
				nreps ++;

			//change to next phase
			currentPhase = getNextPhase(currentPhase);
		}

		setFractionAndText(phaseSeconds);
	}

	// Private methods ------------------

	private bool shouldEndPhase(double phaseSeconds)
	{
		if(currentPhase == phases.REPETITION && phaseSeconds > encoderRhythm.RepSeconds)
			return true;
		if(currentPhase == phases.ECC && phaseSeconds > encoderRhythm.EccSeconds)
		       return true;
		if(currentPhase == phases.CON && phaseSeconds > encoderRhythm.ConSeconds)
		       return true;
		if(currentPhase == phases.RESTREP && phaseSeconds > encoderRhythm.RestRepsSeconds)
		       return true;
		if(currentPhase == phases.RESTCLUSTER && phaseSeconds > encoderRhythm.RestClustersSeconds)
		       return true;

		return false;
	}

	private bool endPhaseShouldEndRep()
	{
		if(currentPhase == phases.REPETITION)
			return true;

		if(gravitatory)
		{
			if(currentPhase == phases.CON && ! encoderRhythm.RestAfterEcc) //ecc-con
				return true;

			if(currentPhase == phases.ECC && encoderRhythm.RestAfterEcc) //con-ecc
				return true;
		} else {
			if(currentPhase == phases.ECC)
				return true;
		}

		return false;
	}

	private phases getNextPhase(phases previousPhase)
	{
		//manage what happens after cluster rest or if we should start a cluster rest
		if(encoderRhythm.UseClusters())
		{
			//check what happens after cluster rest
			if(previousPhase == phases.RESTCLUSTER)
			{
				if(encoderRhythm.RepsOrPhases)
					return phases.REPETITION;
				else if(gravitatory && ! encoderRhythm.RestAfterEcc) //ecc-con
					return phases.ECC;
				else // ! gravitatory or gravitatory resting after ecc
					return phases.CON;
			}

			//check if we are on (REPETITION or ECC or CON) and we should start a cluster rest
			if
				( nreps > 0 && nreps % encoderRhythm.RepsCluster == 0 &&
				  ( previousPhase == phases.REPETITION ||
				    ( previousPhase == phases.CON && gravitatory && ! encoderRhythm.RestAfterEcc ) ||
				    ( previousPhase == phases.ECC && ( ! gravitatory || encoderRhythm.RestAfterEcc ) )
				  )
				)
					return phases.RESTCLUSTER;
		}

		if(encoderRhythm.RepsOrPhases)
		{
			if(previousPhase == phases.REPETITION)
			{
				if(encoderRhythm.RestRepsSeconds == 0)
					return phases.REPETITION;
				else
					return phases.RESTREP;
			} else //RESTREP
					return phases.REPETITION;
		}

		if(gravitatory && ! encoderRhythm.RestAfterEcc) //ecc-con
		{
			if(previousPhase == phases.ECC)
				return phases.CON;
			else if(previousPhase == phases.CON)
			{
				if(encoderRhythm.RestRepsSeconds == 0)
					return phases.ECC;
				else
					return phases.RESTREP;
			}
			else //(previousPhase == phases.RESTREP)
				return phases.ECC;
		}

		else // ! gravitatory or gravitatory resting after ecc
		{
			if(previousPhase == phases.CON)
				return phases.ECC;
			else if(previousPhase == phases.ECC)
			{
				if(encoderRhythm.RestRepsSeconds == 0)
					return phases.CON;
				else
					return phases.RESTREP;
			}
			else //(previousPhase == phases.RESTREP)
				return phases.CON;
		}
	}

	private void setFractionAndText (double phaseSeconds)
	{
		textRepetition = "";

		if(currentPhase == phases.REPETITION || currentPhase == phases.ECC || currentPhase == phases.CON)
		{
			textRest = "";

			if(currentPhase == phases.REPETITION)
				fraction = UtilAll.DivideSafeFraction(phaseSeconds, encoderRhythm.RepSeconds);
			if(currentPhase == phases.ECC)
			{
				fraction =  1 - UtilAll.DivideSafeFraction(phaseSeconds, encoderRhythm.EccSeconds);
				textRepetition = "Excentric";
			}
			if(currentPhase == phases.CON)
			{
				fraction = UtilAll.DivideSafeFraction(phaseSeconds, encoderRhythm.ConSeconds);
				textRepetition = "Concentric";
			}
			if(encoderRhythm.UseClusters())
				textRepetition += " (" +
					( (nreps % encoderRhythm.RepsCluster) +1 ).ToString() + "/" +
					encoderRhythm.RepsCluster.ToString() + ")";
		} else {
			//no change fraction
			double restTime = encoderRhythm.RestRepsSeconds - phaseSeconds;
			if(currentPhase == phases.RESTCLUSTER)
				restTime = encoderRhythm.RestClustersSeconds - phaseSeconds;

			textRest = string.Format(Catalog.GetString("Resting {0} s"), Util.TrimDecimals(restTime, 1));
		}
		LogB.Information("currentPhase = " + currentPhase.ToString());
	}

	// Accessors ------------------

	public double Fraction
	{
		get {
			if(fraction < 0)
				return 0;
			else if(fraction > 1)
				return 1;
			return fraction;
		}
	}

	public string TextRepetition
	{
		get { return textRepetition; }
	}

	public string TextRest
	{
		get { return textRest; }
	}
}

/*
 * this code is adaptative and is quite difficult to follow, specially what's related to rest times between repetitions
 *
public class EncoderRhythmExecute
{
	public string TextRepetition;
	public string TextRest;

	private DateTime lastRepetitionDT;
	private EncoderRhythm encoderRhythm;
	private int nreps;
	private bool lastIsUp;
	private bool restClusterTimeEndedFlag;

	private double fractionRepetition;
	private double fractionRest;

	//true is for con or ecc-con (gravitatory, always end on "con"), false is for con-ecc (inertial)
	//private	bool eccon_ec = true;
	private	bool gravitatory = true;

	//on inertial rest is after ecc.
	//on gravitatory rest can be after ecc or con (see RestAfterEcc)


	//constructor
	public EncoderRhythmExecute(EncoderRhythm encoderRhythm, bool gravitatory)
	{
		this.encoderRhythm = encoderRhythm;
		//this.eccon_ec = eccon_ec;
		this.gravitatory = gravitatory;

		initialize();
	}

	private void initialize()
	{
		TextRepetition = "";
		TextRest = "";

		lastRepetitionDT = DateTime.MinValue;
		nreps = 0;
		restClusterTimeEndedFlag = false;
	}

	public bool FirstRepetitionDone()
	{
		return (lastRepetitionDT > DateTime.MinValue);
	}

	private bool firstInCluster()
	{
		return (nreps % encoderRhythm.RepsCluster == 0);
	}

	 // if RepsOrPhases == true (by phases), then ChangePhase will be called when repetition ends
	 // else will be called when ecc or con ends
	public void ChangePhase(int nrep, bool up)
	{
		lastRepetitionDT = DateTime.Now;
		restClusterTimeEndedFlag = false;

		nreps = nrep;
		lastIsUp = up;
	}

	private bool restBetweenRepetitions()
	{
		return
			( gravitatory && lastIsUp != encoderRhythm.RestAfterEcc ) ||
			( ! gravitatory && ! lastIsUp );
	}

	private bool checkIfRestingBetweenClusters(double totalSeconds)
	{
		if(restClusterTimeEndedFlag)
			return false;

		 // We are resting when we done more than 0 repetitions, AND
		 // mod of repetitions by RepsCluster == 0 AND
		 // if repetition ends on c, whe have done c (or if it ends on e, we have done e)

		 //if(nreps > 0 && nreps % encoderRhythm.RepsCluster == 0 && lastIsUp == eccon_ec)
		if(nreps > 0 && nreps % encoderRhythm.RepsCluster == 0 && restBetweenRepetitions())
		{
			if(totalSeconds < encoderRhythm.RestClustersSeconds)
				return true;
			else {
				//resting time passed, force finish rest,
				//mark change of lastRepetitionDT to calculate fraction correctly below
				lastRepetitionDT = DateTime.Now;
				restClusterTimeEndedFlag = true;
				return false;
			}
		}

		return false;
	}

	//useful for fraction of the repetition and the rest time
	public void CalculateFractionsAndText()
	{
		//double fraction = 0;
		TimeSpan span = DateTime.Now - lastRepetitionDT;
		double totalSeconds = span.TotalSeconds;

		if(encoderRhythm.UseClusters() && checkIfRestingBetweenClusters(totalSeconds))
			calculateClusterRestingFraction(totalSeconds);
		else
			calculateRepetitionFraction(totalSeconds);
	}

	private void calculateRepetitionFraction(double totalSeconds)
	{
		//first repetition in cluster will not have rest
		double restRepsSeconds = encoderRhythm.RestRepsSeconds;
		if(encoderRhythm.UseClusters() && firstInCluster())
			restRepsSeconds = 0;

		 // if ( (we ended concentric and it's ecc-con ||
		 //   we ended excentric and it's con-ecc) && totalSeconds < restRepsSeconds)
		 //    then there's a rest between repetitions

		//if(lastIsUp == eccon_ec && totalSeconds < restRepsSeconds)
		if (totalSeconds < restRepsSeconds && restBetweenRepetitions())
		{
			TextRepetition = "";
			TextRest = "Resting " +
				Util.TrimDecimals((restRepsSeconds - totalSeconds),1) +
				" s";
			fractionRepetition = 0;
			fractionRest = totalSeconds / restRepsSeconds;
			return;
		}

		TextRest = "";
			fractionRest = 0;

		// if we ended con and repetition ends at con, then substract restRepsSeconds to totalSeconds to calculate fraction
		// also when we done ecc and repetition ends at ecc

		//if(restRepsSeconds > 0 && lastIsUp == eccon_ec)
		if( restRepsSeconds > 0 && restBetweenRepetitions())
			totalSeconds -= restRepsSeconds;

		if(encoderRhythm.RepsOrPhases)
		{
			TextRepetition = "";
			fractionRepetition = (totalSeconds) / encoderRhythm.RepSeconds;
		}
		else if(lastIsUp)
		{
			TextRepetition = "Excentric";
			fractionRepetition = 1 - ((totalSeconds) / encoderRhythm.EccSeconds);
		} else {
			TextRepetition = "Concentric";
			fractionRepetition = (totalSeconds) / encoderRhythm.ConSeconds;
		}
	}

	private void calculateClusterRestingFraction(double totalSeconds)
	{
		TextRepetition = "";
		TextRest = "Resting " + Convert.ToInt32((encoderRhythm.RestClustersSeconds - totalSeconds)).ToString() + " s";
		fractionRepetition = 0;
		fractionRest = totalSeconds / encoderRhythm.RestClustersSeconds;
		return;
	}

	public double FractionRepetition
	{
		get {
			if(fractionRepetition < 0)
				return 0;
			else if(fractionRepetition > 1)
				return 1;
			return fractionRepetition;
		}
	}

	public double FractionRest
	{
		get {
			if(fractionRest < 0)
				return 0;
			else if(fractionRest > 1)
				return 1;
			return fractionRest;
		}
	}
}
*/
