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
 *  Copyright (C) 2018   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;

public class EncoderRhythm
{
	public double EccSeconds;
	public double ConSeconds;
	public double RestRepsSeconds; //rest between repetitions

	//cluster stuff
	public int RepsCluster;
	public double RestClustersSeconds; //rest between clusters

	public EncoderRhythm()
	{
		//default values
		EccSeconds = .5;
		ConSeconds = 0.5;
		RestRepsSeconds = 1;

		RepsCluster = 5; //1 is minimum value and means "no use clusters"
		RestClustersSeconds = 6;
	}

	public EncoderRhythm(double eccSeconds, double conSeconds, double restRepsSeconds,
			int repsCluster, double restClustersSeconds)
	{
		EccSeconds = eccSeconds;
		ConSeconds = conSeconds;
		RestRepsSeconds = restRepsSeconds;

		RepsCluster = repsCluster;
		RestClustersSeconds = restClustersSeconds;
	}

	public bool UseClusters()
	{
		return (RepsCluster > 1);
	}
}

public class EncoderRhythmExecute
{
	public string TextRepetition;
	public string TextRest;

	private DateTime lastRepetitionDT;
	private EncoderRhythm encoderRhythm;
	private int nreps;
	private bool restClusterTimeEndedFlag;

	private double fractionRepetition;
	private double fractionRest;


	//constructor
	public EncoderRhythmExecute(EncoderRhythm encoderRhythm)
	{
		this.encoderRhythm = encoderRhythm;
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

	public void SetLastRepetitionDT()
	{
		lastRepetitionDT = DateTime.Now;
		nreps ++;
		restClusterTimeEndedFlag = false;
	}

	private bool checkIfRestingBetweenClusters(double totalSeconds)
	{
		if(restClusterTimeEndedFlag)
			return false;

		if(nreps > 0 && nreps % encoderRhythm.RepsCluster == 0)
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

	//reptition has an initial rest phase
	private void calculateRepetitionFraction(double totalSeconds)
	{
		//first repetition in cluster will not have rest
		double restRepsSeconds = encoderRhythm.RestRepsSeconds;
		if(encoderRhythm.UseClusters() && firstInCluster())
			restRepsSeconds = 0;

		if(totalSeconds < restRepsSeconds)
		{
			TextRepetition = "";
			TextRest = "Resting " +
				Util.TrimDecimals((restRepsSeconds - totalSeconds),1) +
				" s";
			fractionRepetition = 0;
			fractionRest = totalSeconds / restRepsSeconds;
			return;
		}
		else if((totalSeconds - restRepsSeconds) < encoderRhythm.EccSeconds)
		{
			TextRepetition = "Excentric";
			TextRest = "";
			fractionRepetition = 1 - ((totalSeconds - restRepsSeconds) / encoderRhythm.EccSeconds);
			fractionRest = 0;
			return;
		}
		else {
			TextRepetition = "Concentric";
			TextRest = "";
			fractionRepetition = (totalSeconds - (restRepsSeconds + encoderRhythm.EccSeconds)) / encoderRhythm.ConSeconds;
			fractionRest = 0;
			return;
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
