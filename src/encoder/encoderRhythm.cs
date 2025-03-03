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
 *  Copyright (C) 2018-2022   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Data;
using Mono.Unix;
using System.Diagnostics;  //Stopwatch

public class EncoderRhythm
{
	public bool ActiveRhythm;
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
		ActiveRhythm = false;

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

	public EncoderRhythm(bool activeRhythm, bool repsOrPhases,
			double repSeconds, double eccSeconds, double conSeconds,
			double restRepsSeconds, bool restAfterEcc,
			int repsCluster, double restClustersSeconds)
	{
		ActiveRhythm = activeRhythm;
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
		if(! ActiveRhythm && ! UseClusters())
			return false;

		if(ActiveRhythm && RestRepsSeconds > 0)
			return true;

		if(UseClusters() && RestClustersSeconds > 0)
			return true;

		return false;
	}

	public double RestClustersForEncoderCaptureAutoEnding()
	{
		if(UseClusters() && RestClustersSeconds > 0)
			return RestClustersSeconds;

		return 0;
	}

	public override string ToString()
	{
		return
			"EncoderRhythm:" +
			"\nActiveRhythm: " + ActiveRhythm.ToString() +
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

//not good POO, sorry
public abstract class EncoderRhythmExecute
{
	public bool FirstPhaseDone;

	//REPETITION is doing the repetition (no differentiate between ecc/con)
	//ECC is ECC phase using phases
	//CON is CON phase using phases
	//RESTRESP is rest between repetitions
	//RESTCLUSTER is rest between clusters
	public enum Phases { REPETITION, ECC, CON, RESTREP, RESTCLUSTER }
	public Phases LastPhase;
	protected Phases currentPhase; //TODO: que no sigui públic aquest, que tingui un accessor públic

	protected EncoderRhythm encoderRhythm;

	protected double fraction;
	protected string textRepetition;
	protected string textRest;
	//private double fractionRest;


	public abstract void FirstPhaseDo(bool up);

	// ---- just for EncoderRhythmExecuteHasRhythm --->
	public virtual void CalculateFractionsAndText() {}

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
	public Phases CurrentPhase
	{
		get { return currentPhase; }
	}

	// <---- end of just for EncoderRhythmExecuteHasRhythm ----

	// ---- just for EncoderRhythmExecuteJustClusters ---->

	public virtual bool ClusterRestDoing () { return false; }
	public virtual void ClusterRestStart () {}
	public virtual void ClusterRestStop () {}
	public virtual string ClusterRestSecondsStr () { return ""; }
	// <---- end of just for EncoderRhythmExecuteJustClusters ----
}

/*
 * this is fixed rhythm starting when first repetition ends
 * easy to follow. not adaptative
 */
public class EncoderRhythmExecuteHasRhythm : EncoderRhythmExecute
{
	private DateTime phaseStartDT;
	private int nreps;

	private	bool gravitatory = true;
	/*
	 * on inertial rest is after ecc.
	 * on gravitatory rest can be after ecc or con (see RestAfterEcc)
	 */

	// Constructor ---------------------

	public EncoderRhythmExecuteHasRhythm (EncoderRhythm encoderRhythm, bool gravitatory)
	{
		this.encoderRhythm = encoderRhythm;
		//this.eccon_ec = eccon_ec;
		this.gravitatory = gravitatory;

		FirstPhaseDone = false;
		textRepetition = "";
		textRest = "";

		phaseStartDT = DateTime.MinValue;
		nreps = 0;
	}

	// Public methods ------------------

	public override void FirstPhaseDo(bool up)
	{
		if(FirstPhaseDone)
			return;

		FirstPhaseDone = true;
		phaseStartDT = DateTime.Now;

		if(encoderRhythm.RepsOrPhases)
			currentPhase = getNextPhase(Phases.REPETITION);
		else if(up)
			currentPhase = getNextPhase(Phases.CON);
		else
			currentPhase = getNextPhase(Phases.ECC);
	}


	//useful for fraction of the repetition and the rest time
	public override void CalculateFractionsAndText()
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
		if(currentPhase == Phases.REPETITION && phaseSeconds > encoderRhythm.RepSeconds)
			return true;
		if(currentPhase == Phases.ECC && phaseSeconds > encoderRhythm.EccSeconds)
		       return true;
		if(currentPhase == Phases.CON && phaseSeconds > encoderRhythm.ConSeconds)
		       return true;
		if(currentPhase == Phases.RESTREP && phaseSeconds > encoderRhythm.RestRepsSeconds)
		       return true;
		if(currentPhase == Phases.RESTCLUSTER && phaseSeconds > encoderRhythm.RestClustersSeconds)
		       return true;

		return false;
	}

	private bool endPhaseShouldEndRep()
	{
		if(currentPhase == Phases.REPETITION)
			return true;

		if(gravitatory)
		{
			if(currentPhase == Phases.CON && ! encoderRhythm.RestAfterEcc) //ecc-con
				return true;

			if(currentPhase == Phases.ECC && encoderRhythm.RestAfterEcc) //con-ecc
				return true;
		} else {
			if(currentPhase == Phases.ECC)
				return true;
		}

		return false;
	}

	private Phases getNextPhase(Phases previousPhase)
	{
		//manage what happens after cluster rest or if we should start a cluster rest
		if(encoderRhythm.UseClusters())
		{
			//check what happens after cluster rest
			if(previousPhase == Phases.RESTCLUSTER)
			{
				if(encoderRhythm.RepsOrPhases)
					return Phases.REPETITION;
				else if(gravitatory && ! encoderRhythm.RestAfterEcc) //ecc-con
					return Phases.ECC;
				else // ! gravitatory or gravitatory resting after ecc
					return Phases.CON;
			}

			//check if we are on (REPETITION or ECC or CON) and we should start a cluster rest
			if
				( nreps > 0 && nreps % encoderRhythm.RepsCluster == 0 &&
				  ( previousPhase == Phases.REPETITION ||
				    ( previousPhase == Phases.CON && gravitatory && ! encoderRhythm.RestAfterEcc ) ||
				    ( previousPhase == Phases.ECC && ( ! gravitatory || encoderRhythm.RestAfterEcc ) )
				  )
				)
					return Phases.RESTCLUSTER;
		}

		if(encoderRhythm.RepsOrPhases)
		{
			if(previousPhase == Phases.REPETITION)
			{
				if(encoderRhythm.RestRepsSeconds == 0)
					return Phases.REPETITION;
				else
					return Phases.RESTREP;
			} else //RESTREP
					return Phases.REPETITION;
		}

		if(gravitatory && ! encoderRhythm.RestAfterEcc) //ecc-con
		{
			if(previousPhase == Phases.ECC)
				return Phases.CON;
			else if(previousPhase == Phases.CON)
			{
				if(encoderRhythm.RestRepsSeconds == 0)
					return Phases.ECC;
				else
					return Phases.RESTREP;
			}
			else //(previousPhase == Phases.RESTREP)
				return Phases.ECC;
		}

		else // ! gravitatory or gravitatory resting after ecc
		{
			if(previousPhase == Phases.CON)
				return Phases.ECC;
			else if(previousPhase == Phases.ECC)
			{
				if(encoderRhythm.RestRepsSeconds == 0)
					return Phases.CON;
				else
					return Phases.RESTREP;
			}
			else //(previousPhase == Phases.RESTREP)
				return Phases.CON;
		}
	}

	private void setFractionAndText (double phaseSeconds)
	{
		textRepetition = "";

		if(currentPhase == Phases.REPETITION || currentPhase == Phases.ECC || currentPhase == Phases.CON)
		{
			textRest = "";

			if(currentPhase == Phases.REPETITION)
				fraction = UtilAll.DivideSafeFraction(phaseSeconds, encoderRhythm.RepSeconds);
			if(currentPhase == Phases.ECC)
			{
				fraction =  1 - UtilAll.DivideSafeFraction(phaseSeconds, encoderRhythm.EccSeconds);
				textRepetition = "Excentric";
			}
			if(currentPhase == Phases.CON)
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
			if(currentPhase == Phases.RESTCLUSTER)
				restTime = encoderRhythm.RestClustersSeconds - phaseSeconds;

			textRest = string.Format(Catalog.GetString("Resting {0} s"), Util.TrimDecimals(restTime, 1));
		}
		LogB.Information("currentPhase = " + currentPhase.ToString());
	}
}

//this manages just the rest time between clusters
public class EncoderRhythmExecuteJustClusters : EncoderRhythmExecute
{
	private bool clusterRestSecondsDoing = false;
	private Stopwatch clusterRestSeconds;

	// Constructor ---------------------

	public EncoderRhythmExecuteJustClusters (EncoderRhythm encoderRhythm, bool gravitatory)
	{
		this.encoderRhythm = encoderRhythm;
	}

	// Public methods ------------------

	public override void FirstPhaseDo(bool up)
	{
		if(FirstPhaseDone)
			return;

		FirstPhaseDone = true;
	}

	public override bool ClusterRestDoing ()
	{
		return clusterRestSecondsDoing;
	}

	public override void ClusterRestStart ()
	{
		if(clusterRestSeconds == null)
			clusterRestSeconds = new Stopwatch();
		clusterRestSeconds.Start();

		clusterRestSecondsDoing = true;
	}

	public override void ClusterRestStop ()
	{
		clusterRestSeconds.Reset(); //Stops time interval measurement and resets the elapsed time to zero.
		clusterRestSecondsDoing = false;
	}

	public override string ClusterRestSecondsStr ()
	{
		double restTime = encoderRhythm.RestClustersSeconds - clusterRestSeconds.Elapsed.TotalSeconds;

		//at < 0 do not show any message, user should be doing repetition again
		if(restTime < 0)
			return "";

		return string.Format(Catalog.GetString("Resting {0} s"), Util.TrimDecimals(restTime, 1));
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

	 // if RepsOrPhases == true (by Phases), then ChangePhase will be called when repetition ends
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
