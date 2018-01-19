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

public class EncoderRhythmObject
{
	public double EccSeconds;
	public double ConSeconds;
	public double RestRepsSeconds; //rest between repetitions

	//cluster stuff
	public double RepsCluster;
	public double RestClustersSeconds; //rest between clusters

	public EncoderRhythmObject()
	{
		//default values
		EccSeconds = .5;
		ConSeconds = 0.5;
		RestRepsSeconds = 1;

		RepsCluster = 5; //1 is minimum value and means "no use clusters"
		RestClustersSeconds = 6;
	}

	public bool UseClusters()
	{
		return (RepsCluster > 1);
	}
}

public class EncoderRhythm
{
	public string TextRepetition;
	public string TextRest;

	private DateTime lastRepetitionDT;
	private EncoderRhythmObject ero;
	private int nreps;
	private bool restClusterTimeEndedFlag;

	private double fractionRepetition;
	private double fractionRest;


	//constructor
	public EncoderRhythm()
	{
		TextRepetition = "";
		TextRest = "";

		lastRepetitionDT = DateTime.MinValue;
		ero = new EncoderRhythmObject();
		nreps = 0;
		restClusterTimeEndedFlag = false;
	}

	public bool FirstRepetitionDone()
	{
		return (lastRepetitionDT > DateTime.MinValue);
	}

	private bool firstInCluster()
	{
		return (nreps % ero.RepsCluster == 0);
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

		if(nreps > 0 && nreps % ero.RepsCluster == 0)
		{
			if(totalSeconds < ero.RestClustersSeconds)
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

		if(ero.UseClusters() && checkIfRestingBetweenClusters(totalSeconds))
			calculateClusterRestingFraction(totalSeconds);
		else
			calculateRepetitionFraction(totalSeconds);
	}

	//reptition has an initial rest phase
	private void calculateRepetitionFraction(double totalSeconds)
	{
		//first repetition in cluster will not have rest
		double restRepsSeconds = ero.RestRepsSeconds;
		if(ero.UseClusters() && firstInCluster())
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
		else if((totalSeconds - restRepsSeconds) < ero.EccSeconds)
		{
			TextRepetition = "Excentric";
			TextRest = "";
			fractionRepetition = 1 - ((totalSeconds - restRepsSeconds) / ero.EccSeconds);
			fractionRest = 0;
			return;
		}
		else {
			TextRepetition = "Concentric";
			TextRest = "";
			fractionRepetition = (totalSeconds - (restRepsSeconds + ero.EccSeconds)) / ero.ConSeconds;
			fractionRest = 0;
			return;
		}
	}

	private void calculateClusterRestingFraction(double totalSeconds)
	{
		TextRepetition = "";
		TextRest = "Resting " + Convert.ToInt32((ero.RestClustersSeconds - totalSeconds)).ToString() + " s";
		fractionRepetition = 0;
		fractionRest = totalSeconds / ero.RestClustersSeconds;
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
