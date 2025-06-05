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

//used on TreeViews capture and analyze
//in ec and ecS there are two separated curves, unfortunately, here is not known if it's ecc or con
public class EncoderCurve
{
	public bool Record;	//only on capture
	public string N;
	public string Series;
	public string Exercise;
	public string Laterality;	//only on analyze
	public double ExtraWeight;
	public double DisplacedWeight;
	public int Inertia; 		//analyze inertial
	public double Diameter;		//analyze inertial
	public double EquivalentMass;	//analyze inertial
	public string Start;
	public string Duration;
	public string Height;
	public string MeanSpeed;
	public string MaxSpeed;
	public string MaxSpeedT;
	public string RVD;
	public string MeanPower;
	public string PeakPower;
	public string PeakPowerT;
	public string PP_PPT;
	public string MeanForce;
	public string MaxForce;
	public string MaxForceT;
	public string MaxForce_MaxForceT;
	public string WorkJ;
	public string Impulse;

	public EncoderCurve () {
	}

	//used on TreeView capture
	public EncoderCurve (bool record, string n, 
			string start, string duration, string height, 
			string meanSpeed, string maxSpeed, string maxSpeedT,
			string RVD,
			string meanPower, string peakPower, string peakPowerT, 
			string PP_PPT,
			string meanForce, string maxForce, string maxForceT,
			string maxForce_maxForceT,
			string workJ, string impulse
			)
	{
		this.Record = record;
		this.N = n;
		this.Start = start;
		this.Duration = duration;
		this.Height = height;
		this.MeanSpeed = meanSpeed;
		this.MaxSpeed = maxSpeed;
		this.MaxSpeedT = maxSpeedT;
		this.RVD = RVD;
		this.MeanPower = meanPower;
		this.PeakPower = peakPower;
		this.PeakPowerT = peakPowerT;
		this.PP_PPT = PP_PPT;	//PeakPower / PeakPowerTime
		this.MeanForce = meanForce;
		this.MaxForce = maxForce;
		this.MaxForceT = maxForceT;
		this.MaxForce_MaxForceT = maxForce_maxForceT;
		this.WorkJ = workJ;
		this.Impulse = impulse;
	}

	//used on TreeView analyze
	public EncoderCurve (string n, string series, string exercise, 
			string laterality,
			double extraWeight, double displacedWeight,
			int inertia, double diameter, double EquivalentMass, 	//3 inertial params
			string start, string duration, string height,
			string meanSpeed, string maxSpeed, string maxSpeedT,
			string RVD,
			string meanPower, string peakPower, string peakPowerT, 
			string PP_PPT,
			string meanForce, string maxForce, string maxForceT,
			string maxForce_maxForceT,
			string workJ, string impulse)
	{
		this.N = n;
		this.Series = series;
		this.Exercise = exercise;
		this.Laterality = laterality;
		this.ExtraWeight = extraWeight;
		this.DisplacedWeight = displacedWeight;
		this.Inertia = inertia;		//inertial
		this.Diameter = diameter;	//inertial
		this.EquivalentMass = EquivalentMass;	//inertial
		this.Start = start;
		this.Duration = duration;
		this.Height = height;
		this.MeanSpeed = meanSpeed;
		this.MaxSpeed = maxSpeed;
		this.MaxSpeedT = maxSpeedT;
		this.RVD = RVD;
		this.MeanPower = meanPower;
		this.PeakPower = peakPower;
		this.PeakPowerT = peakPowerT;
		this.PP_PPT = PP_PPT;	//PeakPower / PeakPowerTime
		this.MeanForce = meanForce;
		this.MaxForce = maxForce;
		this.MaxForceT = maxForceT;
		this.MaxForce_MaxForceT = maxForce_maxForceT;
		this.WorkJ = workJ;
		this.Impulse = impulse;
	}

	public EncoderCurve Copy()
	{
		EncoderCurve curveCopy = new EncoderCurve(Record, N,
				Start, Duration, Height,
				MeanSpeed, MaxSpeed, MaxSpeedT,
				RVD,
				MeanPower, PeakPower, PeakPowerT,
				PP_PPT,
				MeanForce, MaxForce, MaxForceT,
				MaxForce_MaxForceT,
				WorkJ, Impulse);
		return curveCopy;
	}

	//used on FindPosOfBestN
	public void ZeroAll ()
	{
		Height = "0";
		MeanSpeed = "0";
		MaxSpeed = "0";
		MeanPower = "0";
		PeakPower = "0";
		MeanForce = "0";
		MaxForce = "0";
	}

	//http://stackoverflow.com/questions/894263/how-to-identify-if-a-string-is-a-number
	//this does not check if decimal point is a different character (eg '.' or ',')
	//note new method IsNumber on util.cs is better than this
	public bool IsNumberN() {
		int num;
		return int.TryParse(N, out num);
	}

	//check if last char is 'e' or 'c'
	private bool isValidLastCharN() {
		if(N.Length <= 1)
			return false;
		
		char lastChar = N[N.Length-1];
		if(lastChar == 'e' || lastChar == 'c')
			return true;
		
		return false;
	}
	//check if it's "21c" or "15e"
	public bool IsNumberNandEorC() {
		if(N.Length <= 1)
			return false;

		int num;
		if(int.TryParse(N.Substring(0, N.Length-1), out num) && isValidLastCharN())
			return true;

		return false;
	}

	//note this only works if IsNumberNandEorC()
	public Preferences.EncoderPhasesEnum GetPhaseEnum()
	{
		char lastChar = N[N.Length-1];
		if(lastChar == 'e')
			return Preferences.EncoderPhasesEnum.ECC;
		else
			return Preferences.EncoderPhasesEnum.CON;
	}


	//at least for RenderNAnalyze
	public bool IsValidN() {
		if (N == "MAX" || N == "AVG" || N == "SD" || IsNumberN() || IsNumberNandEorC())
			return true;
		return false;
	}

	public double GetParameter(string parameter) {
		switch(parameter) {
			case Constants.Range:
				return Math.Abs(Convert.ToDouble(Height));
			case Constants.MeanSpeed:
				return Convert.ToDouble(MeanSpeed);
			case Constants.MaxSpeed:
				return Convert.ToDouble(MaxSpeed);
			case Constants.MeanForce:
				return Convert.ToDouble(MeanForce);
			case Constants.MaxForce:
				return Convert.ToDouble(MaxForce);
			case Constants.MeanPower:
				return Convert.ToDouble(MeanPower);
			case Constants.PeakPower:
				return Convert.ToDouble(PeakPower);
			default:
				return Convert.ToDouble(MeanPower);
		}
	}

	//ecChar adds an 'e', 'c' or nothing to N
	//if e or c then N will be /2
	public string ToCSV (bool captureOrAnalyze, Constants.Modes currentMode,
			string decimalSeparator, bool useWorkKcal, string ecChar)
	{
		//latin:	2,3 ; 2,5
		//non-latin:	2.3 , 2.5

		string sep = ":::";

		string workString = Util.TrimDecimals(WorkJD, 1);
		if(useWorkKcal)
			workString = Util.TrimDecimals(WorkKcalD, 3);
		
		string str = "";
		//TODO: if capture not shown because some variables like Inertia are not defined
		if(! captureOrAnalyze)
		{
			string nprint = N;
			if(ecChar == "e" || ecChar == "c")
				nprint = decimal.Truncate((Convert.ToInt32(nprint) +1) /2).ToString() + ecChar;

			str = 
				nprint + sep +
				Series + sep + Exercise + sep + Laterality + sep +
				ExtraWeight + sep + DisplacedWeight + sep;

			if(currentMode == Constants.Modes.POWERINERTIAL)
				str += Inertia + sep + Util.TrimDecimals(Diameter,1) + sep + Util.TrimDecimals(EquivalentMass,3) + sep;

			str +=
				Start + sep + Duration + sep + Height + sep + 
				MeanSpeed + sep + MaxSpeed + sep + MaxSpeedT + sep +
				RVD + sep +
				MeanPower + sep + PeakPower + sep + PeakPowerT + sep + 
				PP_PPT + sep +
				MeanForce + sep + MaxForce + sep + MaxForceT + sep +
				MaxForce_MaxForceT + sep +
				workString + sep + Impulse;
		}
		
		if(decimalSeparator == "COMMA")
			str = Util.ConvertToComma(str);
		else
			str = Util.ConvertToPoint(str);
			
		if(decimalSeparator == "COMMA")
			return Util.ChangeChars(str, ":::", ";");
		else
			return Util.ChangeChars(str, ":::", ",");
	}

	public double StartD {
		get { return Convert.ToDouble (Start); }
	}
	public double EndD {
		get { return StartD + Convert.ToDouble (Duration); }
	}
	public double CenterD {
		get { return UtilAll.DivideSafe (StartD + EndD, 2); }
	}

	public double MeanSpeedD { get { return Convert.ToDouble(MeanSpeed); } }
	public double MaxSpeedD  { get { return Convert.ToDouble(MaxSpeed);  } }
	/*
	public double RVD {
		get {
			return UtilAll.DivideSafe (MaxSpeedD, Convert.ToDouble (MaxSpeedT) /1000);
		}
	}
	*/
	public double MeanPowerD { get { return Convert.ToDouble(MeanPower); } }
	public double PeakPowerD { get { return Convert.ToDouble(PeakPower); } }
	public double MeanForceD { get { return Convert.ToDouble(MeanForce); } }
	public double MaxForceD  { get { return Convert.ToDouble(MaxForce);  } }

	public double WorkJD  { get { return Convert.ToDouble(WorkJ);  } }
	public double WorkKcalD  { get { return Convert.ToDouble(WorkJ) * 0.000239006;  } }
	public double ImpulseD  { get { return Convert.ToDouble(Impulse); } }
	
	~EncoderCurve() {}
}
