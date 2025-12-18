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
 *  Copyright (C) 2004-2024   Xavier de Blas <xaviblas@gmail.com>
 */


using System;
using System.Collections; //ArrayList

//to know which is the best curve in a signal...
public class EncoderSignal
{
	private ArrayList curves;

	// constructor ----

	public EncoderSignal (ArrayList curves) {
		this.curves = curves;
	}


	public int CurvesNum() {
		return curves.Count;
	}

	//this can be an eccentric or concentric curve
	public int FindPosOfBest(int start, string variable)
	{
		double bestValue = 0;
		int bestValuePos = start;
		int i = 0;
		
		foreach(EncoderCurve curve in curves) 
		{
			if(i >= start && curve.GetParameter(variable) > bestValue)
			{
				bestValue = curve.GetParameter(variable);
				bestValuePos = i;
				//LogB.Information(string.Format("bestValue: {0}; bestValuePos: {1}", bestValue, bestValuePos));
			}

			i++;
		}
		return bestValuePos;
	}
	
	//this is an ecc-con curve
	//start is a counter of phases not of repetitions
	public int FindPosOfBestEccCon(int start, string variable, Preferences.EncoderRepetitionCriteria repCriteria)
	{
		double eccValue = 0;
		double conValue = 0;

		double bestValue = 0; //will be ecc-con average, ecc or con depending on repCriteria
		int bestValuePos = start; //will always be the position of the ecc
		int i = 0;
		
		bool ecc = true;
		foreach(EncoderCurve curve in curves) 
		{
			if(repCriteria == Preferences.EncoderRepetitionCriteria.ECC_CON)
			{
				if(ecc) {
					eccValue = curve.GetParameter(variable);
				} else {
					conValue = curve.GetParameter(variable);
					if( i >= start && ( (eccValue + conValue) / 2 ) > bestValue) {
						bestValue = (eccValue + conValue) / 2;
						bestValuePos = i -1; //the ecc
					}
				}
			}
			else if(repCriteria == Preferences.EncoderRepetitionCriteria.ECC)
			{
				if(ecc) {
					eccValue = curve.GetParameter(variable);
					if(i >= start && eccValue > bestValue) {
						bestValue = eccValue;
						bestValuePos = i; //the ecc
					}
				}
			}
			else// if(repCriteria == Preferences.EncoderRepetitionCriteria.CON)
			{
				if(! ecc) {
					conValue = curve.GetParameter(variable);
					if(i >= start && conValue > bestValue) {
						bestValue = conValue;
						bestValuePos = i-1; //the ecc
					}
				}
			}

			ecc = ! ecc;
			i ++;
		}
		return bestValuePos;
	}

	public enum Contraction { EC, C };
	public List<int> FindPosOfBestN(int start, string variable, int n, Contraction eccon, Preferences.EncoderRepetitionCriteria repCriteria)
	{
		//1) find how many values to return
		//size of list will be n or the related curves if it is smaller
		if(curves.Count - start < n)
			n = curves.Count - start;

		if(n <= 0)
			return new List<int>();

		//2) make a copy of curves and have a new EncoderSignal to work with
		ArrayList curvesCopy = new ArrayList();
		foreach(EncoderCurve curve in curves)
			curvesCopy.Add(curve.Copy());
		EncoderSignal es = new EncoderSignal(curvesCopy);

		//3) find the best values and fill listOfPos
		List<int> listOfPos = new List<int>(n);
		int posOfBest = -1;
		int count = 0;

		while(count < n)
		{
			if(posOfBest >= 0)
			{
				//curves.RemoveAt(posOfBest); //do not do it because it is difficult to know pos of next values, just zero that curve
				((EncoderCurve) curvesCopy[posOfBest]).ZeroAll();
				if(eccon == Contraction.EC)
					((EncoderCurve) curvesCopy[posOfBest+1]).ZeroAll();
			}

			if(eccon == Contraction.C)
				posOfBest = es.FindPosOfBest(start, variable);
			else //(eccon == Contraction.EC)
				posOfBest = es.FindPosOfBestEccCon(start, variable, repCriteria);

			listOfPos.Add(posOfBest);
			count ++;
		}
		return listOfPos;
	}

	//TODO: do also for ecc-con
	//returns the pos of the first one of the consecutive rows
	public int FindPosOfBestNConsecutive(int start, string variable, int n)
	{
		//2) find the best values and fill listOfPos
		double bestValue = 0;
		int bestValuePos = -1;
		int count = start;

		while(count <= curves.Count - n)
		{
			double sum = 0;
			for(int i = count; i < count + n; i ++)
			{
				sum += ((EncoderCurve) curves[i]).GetParameter(variable);
			}
			LogB.Information("sum: " + sum.ToString());
			if (sum > bestValue)
			{
				bestValue = sum;
				bestValuePos = count;
				LogB.Information(string.Format("bestValue: {0}, bestValuePos: {1}", bestValue, bestValuePos));
			}

			count ++;
		}
		return bestValuePos;
	}
	public int FindPosOfBestNConsecutiveEccCon(int start, string variable, int n, Preferences.EncoderRepetitionCriteria repCriteria)
	{
		//2) find the best values and fill listOfPos
		double bestValue = 0;
		int bestValuePos = -1;
		int count = start;

		n *= 2;
		while(count <= curves.Count - n)
		{
			double sum = 0;
			double eccSum = 0; //used on EncoderRepetitionCriteria.ECC
			double conSum = 0; //used on EncoderRepetitionCriteria.CON

			for(int i = count; i < count + n; i += 2)
			{
				double eccValue = ((EncoderCurve) curves[i]).GetParameter(variable);
				double conValue = ((EncoderCurve) curves[i+1]).GetParameter(variable);
				sum += (eccValue + conValue) / 2;
				eccSum += eccValue;
				conSum += conValue;
				//LogB.Information(string.Format("eccValue: {0}, conValue: {1}, accumulated sum: {2}", eccValue, conValue, sum));
			}
			//LogB.Information("total sum: " + sum.ToString());
			if(repCriteria == Preferences.EncoderRepetitionCriteria.ECC_CON && sum > bestValue)
			{
				bestValue = sum;
				bestValuePos = count;
				//LogB.Information(string.Format("bestValue: {0}, bestValuePos: {1}", bestValue, bestValuePos));
			}
			else if (repCriteria == Preferences.EncoderRepetitionCriteria.ECC && eccSum > bestValue)
			{
				bestValue = eccSum;
				bestValuePos = count;
			}
			else if (repCriteria == Preferences.EncoderRepetitionCriteria.CON && conSum > bestValue)
			{
				bestValue = conSum;
				bestValuePos = count;
			}

			count += 2;
		}
		return bestValuePos;
	}


	public double GetEccConMean(int eccPos, string variable)
	{
		return(
				(
				((EncoderCurve) curves[eccPos]).GetParameter(variable) +
				((EncoderCurve) curves[eccPos +1]).GetParameter(variable)
				) /2 );
	}

	//use also with range
	public double GetEccConMax(int eccPos, string variable)
	{
		double eccValue = Math.Abs( ((EncoderCurve) curves[eccPos]).GetParameter(variable));
		double conValue = Math.Abs( ((EncoderCurve) curves[eccPos +1]).GetParameter(variable));

		if(eccValue > conValue)
			return eccValue;
		return conValue;
	}

	public int GetEccConLossByOnlyConPhase(string variable)
	{
		double lowest = 100000;
		double highest = 0;
		int highestPos = 0;
		//double eccValue = 0;
		//double conValue = 0;
		bool ecc = true;
		int i = 0;
		foreach (EncoderCurve curve in curves)
		{
			if(ecc)
			{
				ecc = false;
				i++;
				continue;
			}

			double compareTo = curve.MeanSpeedD;
			if(variable == Constants.MeanPower)
				compareTo = curve.MeanPowerD;

			bool needChangeLowest = false;
			//conValue = compareTo;
			if(compareTo > highest)
			{
				highest = compareTo;
				highestPos = i;
				needChangeLowest = true; 	//min rep has to be after max
			} if(needChangeLowest || (compareTo < lowest &&
						((EncoderCurve) curves[i]).GetParameter(Constants.Range) >= .7 * ((EncoderCurve) curves[highestPos]).GetParameter(Constants.Range)
						))
				lowest = compareTo;

			//LogB.Information(string.Format("Loss ecc/con (by con) of {0}; i: {1} is: {2}", variable, i++,
			//			Convert.ToInt32(UtilAll.DivideSafe(100.0 * (highest - lowest), highest))));

			i++;
			ecc = true;
		}
		return Convert.ToInt32(UtilAll.DivideSafe(100.0 * (highest - lowest), highest));

	}
	/*
	 * this method uses ecc and con and calculates the loss by having the average of them for each repetition
	 * better do only using the con phase (see above method)
	 *
	public int GetEccConLoss(string variable)
	{
		double lowest = 100000;
		double highest = 0;
		double eccValue = 0;
		double conValue = 0;
		bool ecc = true;
		//int i = 0;
		foreach (EncoderCurve curve in curves)
		{
			double compareTo = curve.MeanSpeedD;
			if(variable == Constants.MeanPower)
				compareTo = curve.MeanPowerD;

			if(ecc)
				eccValue = compareTo;
			else {
				conValue = compareTo;
				if( ( (eccValue + conValue) / 2 ) > highest)
					highest = (eccValue + conValue) / 2;
				if( ( (eccValue + conValue) / 2 ) < lowest)
					lowest = (eccValue + conValue) / 2;
			}
			//LogB.Information(string.Format("Loss ecc/con (ecc?: {0}) of {1}; i: {2} is: {3}", ecc.ToString(), variable, i++,
			//			Convert.ToInt32(UtilAll.DivideSafe(100.0 * (highest - lowest), highest))));

			ecc = ! ecc;
		}
		return Convert.ToInt32(UtilAll.DivideSafe(100.0 * (highest - lowest), highest));

	}
	*/

	~EncoderSignal() {}
}


//related to encoderSignalCurve table
public class EncoderSignalCurve
{
	public int uniqueID;
	public int signalID;
	public int curveID;
	public int msCentral;
	
	public EncoderSignalCurve(int uniqueID, int signalID, int curveID, int msCentral) {
		this.uniqueID = uniqueID;
		this.signalID = signalID;
		this.curveID = curveID;
		this.msCentral = msCentral;
	}
	
	public override string ToString() {
		return uniqueID.ToString() + ":" + signalID.ToString() + ":" + 
			curveID.ToString() + ":" + msCentral.ToString();
	}

	// for some reason, some EncoderSignalCurve records are repeated on DB
	public override bool Equals (object evalString)
	{
		return this.ToString() == evalString.ToString();
	}
	public override int GetHashCode ()
	{
		return this.ToString().GetHashCode();
	}

	~EncoderSignalCurve() {}
}
