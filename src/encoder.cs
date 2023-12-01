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
 *  Copyright (C) 2004-2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Data;
using System.Text; //StringBuilder
using System.IO;   //for Path
using System.Collections; //ArrayList
using System.Collections.Generic; //List<T>
using Mono.Unix;

public class EncoderParams
{
	//graph.R need both to know displacedMass depending on encoderConfiguration
	//and plot both as entry data in the table of result data
	private string massBody; //to pass always as "." to R.
	private string massExtra; //to pass always as "." to R
	
	private int minHeight;
	private int exercisePercentBodyWeight; //was private bool isJump; (if it's 0 is like "jump")
	private string eccon;
	private string analysis;
	private string analysisVariables;
	private string analysisOptions;		//p: propulsive
	private bool captureCheckFullyExtended;
	private int captureCheckFullyExtendedValue;
					
	//encoderConfiguration conversions
	//in signals and curves, need to do conversions (invert, inertiaMomentum, diameter)
	private EncoderConfiguration encoderConfiguration;	
	
	private string smoothCon; //to pass always as "." to R
	private int curve;
	private int width;
	private int height;
	private string decimalSeparator;	//used in export data from R to csv
	//private bool inverted; //used only in runEncoderCapturePython. In graph.R will be used encoderConfigurationName

	public EncoderParams()
	{
	}

	
	//to graph.R	
	public EncoderParams(int minHeight, int exercisePercentBodyWeight, string massBody, string massExtra, 
			string eccon, string analysis, string analysisVariables, string analysisOptions, 
			bool captureCheckFullyExtended, int captureCheckFullyExtendedValue,
			EncoderConfiguration encoderConfiguration,
			string smoothCon, int curve, int width, int height, string decimalSeparator)
	{
		this.minHeight = minHeight;
		this.exercisePercentBodyWeight = exercisePercentBodyWeight;
		this.massBody = massBody;
		this.massExtra = massExtra;
		this.eccon = eccon;
		this.analysis = analysis;
		this.analysisVariables = analysisVariables;
		this.analysisOptions = analysisOptions;
		this.captureCheckFullyExtended = captureCheckFullyExtended;
		this.captureCheckFullyExtendedValue = captureCheckFullyExtendedValue;
		this.encoderConfiguration = encoderConfiguration;
		this.smoothCon = smoothCon;
		this.curve = curve;
		this.width = width;
		this.height = height;
		this.decimalSeparator = decimalSeparator;
	}
	
	public string ToStringROptions () 
	{
		string capFullyExtendedStr = "-1";
		if(captureCheckFullyExtended)
			capFullyExtendedStr = captureCheckFullyExtendedValue.ToString(); 
		
		return 
			"#minHeight\n" + 	minHeight + "\n" + 
			"#exercisePercentBodyWeight\n" + exercisePercentBodyWeight + "\n" + 
			"#massBody\n" + 	massBody + "\n" + 
			"#massExtra\n" + 	massExtra + "\n" + 
			"#eccon\n" + 		eccon + "\n" + 
			"#analysis\n" + 	analysis + "\n" + 
			"#analysisVariables\n" + analysisVariables + "\n" + 
			"#analysisOptions\n" + analysisOptions + "\n" + 
			"#captureCheckFullyExtended\n" + capFullyExtendedStr + "\n" + 
			encoderConfiguration.ToStringOutput(EncoderConfiguration.Outputs.ROPTIONS) + "\n" +
			"#smoothCon\n" + 	smoothCon + "\n" + 
			"#curve\n" + 		curve + "\n" + 
			"#width\n" + 		width + "\n" + 
			"#height\n" + 		height + "\n" + 
			"#decimalSeparator\n" + decimalSeparator
			;
	}
	
	public string Analysis {
		get { return analysis; }
	}


	~EncoderParams() {}
}

public class EncoderStruct
{
	public EncoderStruct() {
	}

	public string InputData;
	public string OutputGraph;
	public string OutputData1;
	public string EncoderRPath; //to load other R scripts
	public string EncoderTempPath; //use for Status, Special, GraphParams....
	public EncoderParams Ep;

	//pass this to R
	public EncoderStruct(string InputData, string OutputGraph, 
			string OutputData1, 
			string EncoderRPath, string EncoderTempPath,
			EncoderParams Ep)
	{
		this.InputData = InputData;
		this.OutputGraph = OutputGraph;
		this.OutputData1 = OutputData1;
		this.EncoderRPath = EncoderRPath;
		this.EncoderTempPath = EncoderTempPath;
		this.Ep = Ep;
	}

	~EncoderStruct() {}
}

public class EncoderGraphROptions
{
	public string inputData;
	public string outputGraph;
	public string outputData1;
	public string encoderRPath;
	public string encoderTempPath;
	public EncoderParams ep;
	public string title;
	public string operatingSystem;
	public string englishWords;
	public string translatedWords;
	public bool debug;
	public bool crossValidate;
	private bool cutByTriggers;
	private string triggerList;
	public bool separateSessionInDays;
	public AnalysisModes analysisMode; //the four analysisModes
	Preferences.EncoderInertialGraphsXTypes inertialGraphX;

	public enum AnalysisModes { CAPTURE, INDIVIDUAL_CURRENT_SET, INDIVIDUAL_CURRENT_SESSION, INDIVIDUAL_ALL_SESSIONS, GROUPAL_CURRENT_SESSION }

	public EncoderGraphROptions(
			string inputData, string outputGraph, string outputData1, 
			string encoderRPath, string encoderTempPath,
			EncoderParams ep,
			string title, string operatingSystem,
			string englishWords, string translatedWords,
			bool debug, bool crossValidate, bool cutByTriggers, string triggerList,
			bool separateSessionInDays, AnalysisModes analysisMode, Preferences.EncoderInertialGraphsXTypes inertialGraphX)
	{
		this.inputData = inputData;
		this.outputGraph = outputGraph;
		this.outputData1 = outputData1;
		this.encoderRPath = encoderRPath;
		this.encoderTempPath = encoderTempPath;
		this.ep = ep;
		this.title = title;
		this.operatingSystem = operatingSystem;
		this.englishWords = englishWords;
		this.translatedWords = translatedWords;
		this.debug = debug;
		this.crossValidate = crossValidate;
		this.cutByTriggers = cutByTriggers;
		this.triggerList = triggerList;
		this.separateSessionInDays = separateSessionInDays;
		this.analysisMode = analysisMode;
		this.inertialGraphX = inertialGraphX;

		//ensure triggerList is not null or blank
		if(triggerList == null || triggerList == "")
			triggerList = "-1";
	}

	public override string ToString() {
		return 
			"#inputdata\n" + 	inputData + "\n" + 
			"#outputgraph\n" + 	outputGraph + "\n" + 
			"#outputdata1\n" + 	outputData1 + "\n" + 
			"#encoderRPath\n" + 	encoderRPath + "\n" + 
			"#encoderTempPath\n" + 	encoderTempPath + "\n" + 
			ep.ToStringROptions() + "\n" + 
			"#title\n" + 		title + "\n" + 
			"#operatingsystem\n" + 	operatingSystem + "\n" +
			"#englishWords\n" + 	englishWords + "\n" +
			"#translatedWords\n" + 	translatedWords + "\n" +
			"#debug\n" +		Util.BoolToRBool(debug) + "\n" +
			"#crossValidate\n" +	Util.BoolToRBool(crossValidate) + "\n" +
			"#cutByTriggers\n" +	Util.BoolToRBool(cutByTriggers) + "\n" +
			"#triggerList\n" +	triggerList + "\n" +
			"#separateSessionInDays\n" +	Util.BoolToRBool(separateSessionInDays) + "\n" +
			"#analysisMode\n" + 	analysisMode.ToString() + "\n" +
			"#inertialGraphX\n" + 	inertialGraphX.ToString() + "\n";
	}
	

	~EncoderGraphROptions() {}
}


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

	public double MeanSpeedD { get { return Convert.ToDouble(MeanSpeed); } }
	public double MaxSpeedD  { get { return Convert.ToDouble(MaxSpeed);  } }
	public double MeanPowerD { get { return Convert.ToDouble(MeanPower); } }
	public double PeakPowerD { get { return Convert.ToDouble(PeakPower); } }
	public double MeanForceD { get { return Convert.ToDouble(MeanForce); } }
	public double MaxForceD  { get { return Convert.ToDouble(MaxForce);  } }

	public double WorkJD  { get { return Convert.ToDouble(WorkJ);  } }
	public double WorkKcalD  { get { return Convert.ToDouble(WorkJ) * 0.000239006;  } }
	public double ImpulseD  { get { return Convert.ToDouble(Impulse); } }
	
	~EncoderCurve() {}
}


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
	
	~EncoderSignalCurve() {}
}


//used on TreeView
public class EncoderNeuromuscularData
{
	public string code;
	public string person;
	public string jump_num;  //can be "AVG"
	public double extraWeight;
	public double e1_range; //double on AVG row
	public double e1_t; 	//double on AVG row
	public double e1_fmax;
	public double e1_rfd_avg;
	public double e1_i;

	public double ca_range; //double on AVG row
	public double cl_t;     //double on AVG row
	public double cl_rfd_avg;
	public double cl_i;

	public double cl_f_avg;
	public double cl_vf;
	public double cl_f_max;

	public double cl_s_avg;
	public double cl_s_max;
	public double cl_p_avg;
	public double cl_p_max;

	public EncoderNeuromuscularData () {
	}

	//used on TreeView analyze
	public EncoderNeuromuscularData (
			string code, string person, string jump_num, double extraWeight,
			double e1_range, double e1_t, double e1_fmax, double e1_rfd_avg, double e1_i,
			double ca_range, double cl_t, double cl_rfd_avg, double cl_i,
			double cl_f_avg, double cl_vf, double cl_f_max, 
			double cl_s_avg, double cl_s_max, double cl_p_avg, double cl_p_max
			)
	{
		this.code = code;
		this.person = person;
		this.jump_num = jump_num;
		this.extraWeight = extraWeight;
		this.e1_range = e1_range; 
		this.e1_t = e1_t;
		this.e1_fmax = e1_fmax;
		this.e1_rfd_avg = e1_rfd_avg;
		this.e1_i = e1_i;
		this.ca_range = ca_range;
		this.cl_t = cl_t;
		this.cl_rfd_avg = cl_rfd_avg;
		this.cl_i = cl_i;
		this.cl_f_avg = cl_f_avg;
		this.cl_vf = cl_vf;
		this.cl_f_max = cl_f_max;
		this.cl_s_avg = cl_s_avg;
		this.cl_s_max = cl_s_max;
		this.cl_p_avg = cl_p_avg;
		this.cl_p_max = cl_p_max;
	}

	//reading contents file from graph.R
	public EncoderNeuromuscularData (string [] cells)
	{
		//cells [0-2] are not converted because are strings
		for(int i = 3 ; i < cells.Length ;  i ++)
			cells[i] = Util.TrimDecimals(Convert.ToDouble(Util.ChangeDecimalSeparator(cells[i])),3);
	
		this.code 	= cells[0];
		this.person 	= cells[1];
		this.jump_num 	= cells[2];
		this.extraWeight = Convert.ToDouble(cells[3]);
		this.e1_range 	= Convert.ToDouble(cells[4]);
		this.e1_t 	= Convert.ToDouble(cells[5]);
		this.e1_fmax 	= Convert.ToDouble(cells[6]);
		this.e1_rfd_avg	= Convert.ToDouble(cells[7]);
		this.e1_i	= Convert.ToDouble(cells[8]);
		this.ca_range	= Convert.ToDouble(cells[9]);
		this.cl_t 	= Convert.ToDouble(cells[10]);
		this.cl_rfd_avg = Convert.ToDouble(cells[11]);
		this.cl_i 	= Convert.ToDouble(cells[12]);
		this.cl_f_avg 	= Convert.ToDouble(cells[13]);
		this.cl_vf 	= Convert.ToDouble(cells[14]);
		this.cl_f_max 	= Convert.ToDouble(cells[15]);
		this.cl_s_avg 	= Convert.ToDouble(cells[16]);
		this.cl_s_max 	= Convert.ToDouble(cells[17]);
		this.cl_p_avg 	= Convert.ToDouble(cells[18]);
		this.cl_p_max 	= Convert.ToDouble(cells[19]);
	}

	public string ToCSV (string decimalSeparator) {
		//latin:	2,3 ; 2,5
		//non-latin:	2.3 , 2.5

		string sep = ":::";
		string str = 
			person + sep + jump_num + sep + extraWeight.ToString () + sep +
			e1_range.ToString() + sep +
			e1_t.ToString() + sep + e1_fmax.ToString() + sep + 
			e1_rfd_avg.ToString() + sep + e1_i.ToString() + sep + 
			ca_range.ToString() + sep + cl_t.ToString() + sep + 
			cl_rfd_avg.ToString() + sep + cl_i.ToString() + sep + 
			cl_f_avg.ToString() + sep + cl_vf.ToString() + sep + cl_f_max.ToString() + sep + 
			cl_s_avg.ToString() + sep + cl_s_max.ToString() + sep + 
			cl_p_avg.ToString() + sep + cl_p_max.ToString();

		if(decimalSeparator == "COMMA")
			str = Util.ConvertToComma(str);
		else
			str = Util.ConvertToPoint(str);
			
		if(decimalSeparator == "COMMA")
			return Util.ChangeChars(str, ":::", ";");
		else
			return Util.ChangeChars(str, ":::", ",");
	}
}

public class EncoderSQL
{
	public string uniqueID;
	public int personID;
	public int sessionID;
	public int exerciseID;
	public string eccon;
	public string laterality;
	public string extraWeight;
	public string signalOrCurve;
	public string filename;
	public string url;	//URL of data of signals and curves. Stored in DB as relative. Used in software as absolute. See SqliteEncoder
	public int time;
	public int minHeight;
	public string description;
	public string status;	//active or inactive curves
	public string videoURL;	//URL of video of signals. Stored in DB as relative. Used in software as absolute. See SqliteEncoder
	
	//encoderConfiguration conversions
	//in signals and curves, need to do conversions (invert, inertiaMomentum, diameter)
	public EncoderConfiguration encoderConfiguration;
//	public int inertiaMomentum; //kg*cm^2
//	public double diameter;
	
	public string future1;
	public string future2;
	public string future3;
	public Preferences.EncoderRepetitionCriteria repCriteria;

	public string exerciseName;
	
	public string ecconLong;
	
	public EncoderSQL ()
	{
	}

	public EncoderSQL (string uniqueID, int personID, int sessionID, int exerciseID, 
			string eccon, string laterality, string extraWeight, string signalOrCurve, 
			string filename, string url, int time, int minHeight, 
			string description, string status, string videoURL, 
			EncoderConfiguration encoderConfiguration,
			string future1, string future2, string future3, 
			Preferences.EncoderRepetitionCriteria repCriteria,
			string exerciseName
			)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.sessionID = sessionID;
		this.exerciseID = exerciseID;
		this.eccon = eccon;
		this.laterality = laterality;
		this.extraWeight = extraWeight;
		this.signalOrCurve = signalOrCurve;
		this.filename = filename;
		this.url = url;
		this.time = time;
		this.minHeight = minHeight;
		this.description = description;
		this.status = status;
		this.videoURL = videoURL;
		this.encoderConfiguration = encoderConfiguration;
		this.future1 = future1;	//on curves: meanPower
		this.future2 = future2; //on curves: meanSpeed
		this.future3 = future3; //on curves: meanForce
		this.repCriteria = repCriteria;
		this.exerciseName = exerciseName;

		ecconLong = EcconLong(eccon);
	}

	public static string EcconLong (string ecconChars)
	{
		if(ecconChars == "c")
			return Catalog.GetString("Concentric");
		else if(ecconChars == "ec" || ecconChars == "ecS")
			return Catalog.GetString("Eccentric-concentric");
		else if(ecconChars == "ce" || ecconChars == "ceS")
			return Catalog.GetString("Concentric-eccentric");
		else
			return "";
	}

	//used on encoder table
	public enum Eccons { ALL, ecS, ceS, c } 

	//for new code on other parts, use static method: UtilDate.GetDatetimePrint (DateTime dt)
	public string GetDatetimeStr (bool pretty)
	{
		//LogB.Information ("GetDatetimeStr filename: " + filename);
		int pointPos = filename.LastIndexOf('.');
		int dateLength = 19; //YYYY-MM-DD_hh-mm-ss
		string date = "";

		//if file has been stored incorrectly (without datetime), just avoid crashing here
		try {
			date = filename.Substring(pointPos - dateLength, dateLength);
		} catch {
			date = UtilDate.ToFile (DateTime.Now);
		}

		if(pretty) {
			string [] dateParts = date.Split(new char[] {'_'});
			date = dateParts[0] + " " + dateParts[1].Replace('-',':');
		}
		return date;
	}

	public string GetDateStr ()
	{
		int pointPos = filename.LastIndexOf('.');
		int dateLength = 19; //YYYY-MM-DD_hh-mm-ss
		string date = filename.Substring(pointPos - dateLength, dateLength);
		string [] dateParts = date.Split(new char[] {'_'});
		return dateParts[0];
	}

	public string GetFullURL(bool convertPathToR) {
		string str = url + Path.DirectorySeparatorChar + filename;
		/*	
			in Linux is separated by '/'
			in windows is separated by '\'
			but R needs always '/', then do the conversion
		 */
		if(convertPathToR && UtilAll.IsWindows())
			str = str.Replace("\\","/");

		return str;
	}

	//showMeanPower is used in curves, but not in signal
	public string [] ToStringArray (int count, bool checkboxes, bool video, bool encoderConfigPretty, bool showMeanPSF)
	{
		int all = 9;
		if(checkboxes)
			all ++;
		if(video)
			all++;
		if(showMeanPSF)
			all += 3;


		string [] str = new String [all];
		int i=0;
		str[i++] = uniqueID;
	
		if(checkboxes)
			str[i++] = "";	//checkboxes
	
		str[i++] = count.ToString();
		str[i++] = Catalog.GetString(exerciseName);
		str[i++] = Catalog.GetString(laterality);
		str[i++] = extraWeight;
		
		if(showMeanPSF)
		{
			str[i++] = future1;

			//as recording meanSpeed and meanForce is new on 2.0, show a blank cell instead of a 0
			if(future2 == "0")
				str[i++] = "";
			else
				str[i++] = future2;

			if(future3 == "0")
				str[i++] = "";
			else
				str[i++] = future3;
		}

		if(encoderConfigPretty)
			str[i++] = encoderConfiguration.ToStringPretty();
		else
			str[i++] = encoderConfiguration.code.ToString();
		
		str[i++] = ecconLong;
		str[i++] = GetDatetimeStr (true);
		
		if(video) {
			if(videoURL != "")
				str[i++] = Catalog.GetString("Yes");
			else
				str[i++] = Catalog.GetString("No");
		}

		str[i++] = description;
		return str;
	}

	public override string ToString () 	 //debug
	{
		return string.Format (
				"uniqueID: {0},  personID: {1},  sessionID: {2},  exerciseID: {3},  eccon: {4}, " +
				"laterality: {5},  extraWeight: {6},  signalOrCurve: {7},  filename: {8}, " +
				"url: {9},  time: {10},  minHeight: {11},  description: {12}, " +
				"status: {13},  videoURL: {14},  encoderConfiguration: {15},  future1: {16}, " +
				"future2: {17},  future3: {18},   repCriteria: {19},  exerciseName: {20}",
				uniqueID, personID, sessionID, exerciseID, eccon, laterality, extraWeight, signalOrCurve, filename,
				url, time, minHeight, description, status, videoURL, encoderConfiguration, future1, future2, future3,  repCriteria, exerciseName);
	}

	//uniqueID:name
	public EncoderSQL ChangePerson(string newIDAndName) {
		int newPersonID = Util.FetchID(newIDAndName);
		string newPersonName = Util.FetchName(newIDAndName);
		string newFilename = filename;

		personID = newPersonID;

		/*
		 * this can fail because person name can have an "-"
		string [] filenameParts = filename.Split(new char[] {'-'});
		filenameParts[0] = newPersonID.ToString();
		filenameParts[1] = newPersonName;
		//the rest will be the same: curveID, timestamp, extension 
		filename = Util.StringArrayToString(filenameParts, "-");
		*/


		/*
		 * filename curve has personID-name-uniqueID-fulldate.txt
		 * filename signal as personID-name-fulldate.txt
		 * in both cases name can have '-' (fuck)
		 * eg: filename curve:
		 * 163-personname-840-2013-04-05_14-11-11.txt
		 * filename signal
		 * 163-personname-2013-04-05_14-03-45.txt
		 *
		 * then,
		 * on curve:
		 * last 23 letters are date and ".txt",
		 * write newPersonID-newPersonName-uniqueID-last23letters
		 * 
		 * on signal:
		 * last 23 letters are date and ".txt",
		 * write newPersonID-newPersonName-last23letters
		 */

		if(signalOrCurve == "curve") 
			newFilename = newPersonID + "-" + newPersonName + "-" + uniqueID + "-" + GetDatetimeStr (false) + ".txt";
		else 
			newFilename = newPersonID + "-" + newPersonName + "-" + GetDatetimeStr (false) + ".txt";

		bool success = false;
		success = Util.FileMove(url, filename, newFilename);
		if(success)
			filename = newFilename;

		//will update SqliteEncoder
		return (this);
	}


	/* 
	 * translations stuff
	 * used to store in english and show translated in GUI
	 */
		
	private string [] lateralityOptionsEnglish = { "RL", "R", "L" }; //attention: if this changes, change it also in gui/encoder.cs createEncoderCombos()
	public string LateralityToEnglish() 
	{
		int count = 0;
		foreach(string option in lateralityOptionsEnglish) {
			if(Catalog.GetString(option) == laterality)
				return lateralityOptionsEnglish[count];
			count ++;
		}
		//default return first value
		return lateralityOptionsEnglish[0];
	}

	//used in NUnit
	public string Filename
	{
		set { filename = value; }
	}

}

//related to all reps, not only active
public class EncoderPersonCurvesInDBDeep
{
	public double extraWeight;
	public int count; //this count is all reps (not only active)

	public EncoderPersonCurvesInDBDeep(double w, int c) {
		this.extraWeight = w;
		this.count = c;
	}

	public override string ToString() {
		return count.ToString() + "*" + extraWeight.ToString();// + "Kg";
	}
}
public class EncoderPersonCurvesInDB
{
	public int personID;
	public int sessionID;
	public string sessionName;
	public string sessionDate;
	public int countActive;
	public int countAll;
	public List<EncoderPersonCurvesInDBDeep> lDeep;
	
	public EncoderPersonCurvesInDB() {
	}
	public EncoderPersonCurvesInDB(int personID, int sessionID, string sessionName, string sessionDate) 
	{
		this.personID =		personID;
		this.sessionID = 	sessionID;
		this.sessionName = 	sessionName;
		this.sessionDate = 	sessionDate;
	}

	public string [] ToStringArray(bool deep) {
		string [] s;

		//the "" will be for the checkbox on genericWin
		if(deep) {
			s = new string[]{ sessionID.ToString(), "", sessionName, sessionDate,
				//countActive.ToString(), countAll.ToString()
				countAll.ToString(), DeepPrint()
			};
		} else {
			s = new string[]{ sessionID.ToString(), "", sessionName, sessionDate,
				//countActive.ToString(), countAll.ToString()
				countAll.ToString()
			};
		}

		return s;
	}

	private string DeepPrint() {
		string s = "";
		string sep = "";
		foreach(EncoderPersonCurvesInDBDeep e in lDeep) {
			s += sep + e.ToString();
			sep = " ";
		}
		return s;
	}
}

public class EncoderExercise
{
	public int uniqueID;
	public string name;
	public int percentBodyWeight;
	public string ressistance;
	public string description;
	public double speed1RM;
	private Constants.EncoderGI type;

	public EncoderExercise() {
	}

	public EncoderExercise(string name) {
		this.name = name;
	}

	public EncoderExercise(int uniqueID, string name, int percentBodyWeight, 
			string ressistance, string description, double speed1RM, Constants.EncoderGI type)
	{
		this.uniqueID = uniqueID;
		this.name = name;
		this.percentBodyWeight = percentBodyWeight;
		this.ressistance = ressistance;
		this.description = description;
		this.speed1RM = speed1RM;
		this.type = type;
	}

	/*
	 * unused, on 1.9.1 all encoder exercises can be deleted
	public bool IsPredefined() {
		if(
				name == "Bench press" ||
				name == "Squat" ||
				name == "Free" ||
				name == "Jump" ||
				name == "Inclined plane" ||
				name == "Inclined plane BW" )
			return true;
		else 
			return false;
	}
	*/

	public override string ToString()
	{
		return uniqueID.ToString() + ": " + name + " (" + percentBodyWeight.ToString() + "%) " +
			ressistance + "," + description + "," + speed1RM.ToString() + "," + type.ToString();
	}

	public int UniqueID
	{
		get { return uniqueID; }
	}
	public string Name
	{
		get { return name; }
	}
	public int PercentBodyWeight
	{
		get { return percentBodyWeight; }
	}
	public string Ressistance
	{
		get { return ressistance; }
	}
	public string Description
	{
		get { return description; }
	}
	public double Speed1RM
	{
		get { return speed1RM; }
	}
	public Constants.EncoderGI Type
	{
		get { return type; }
	}

	~EncoderExercise() {}
}

public class Encoder1RM
{
	public int uniqueID;
	public int personID;
	public int sessionID;
	public DateTime date;
	public int exerciseID;
	public double load1RM;
	
	public string personName;
	public string exerciseName;
	
	public Encoder1RM() {
	}

	public Encoder1RM(int uniqueID, int personID, int sessionID, DateTime date, int exerciseID, double load1RM)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.sessionID = sessionID;
		this.date = date;
		this.exerciseID = exerciseID;
		this.load1RM = load1RM;
	}

	public Encoder1RM(int uniqueID, int personID, int sessionID, DateTime date, int exerciseID, double load1RM, string personName, string exerciseName)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.sessionID = sessionID;
		this.date = date;
		this.exerciseID = exerciseID;
		this.load1RM = load1RM;
		this.personName = personName;
		this.exerciseName = exerciseName;
	}

	public string [] ToStringArray() {
		string [] s = { uniqueID.ToString(), load1RM.ToString() };
		return s;
	}
	
	public string [] ToStringArray2() {
		string [] s = { uniqueID.ToString(), personName, exerciseName, load1RM.ToString(), date.ToShortDateString() };
		return s;
	}


	~Encoder1RM() {}
}

public class EncoderCaptureCurve
{
	public bool up;
	public int startFrame;
        public int endFrame;

	public EncoderCaptureCurve(int startFrame, int endFrame)
	{
		this.startFrame = startFrame;
		this.endFrame = endFrame;
	}
	
	public string DirectionAsString() {
		if(up)
			return "UP";
		else
			return "DOWN";
	}

	public override string ToString()
	{
		return "ECC: " + up.ToString() + ";" + startFrame.ToString() + ";" + endFrame.ToString();
	}

	~EncoderCaptureCurve() {}
}

public class EncoderCaptureCurveArray
{
	public ArrayList ecc;	//each of the EncoderCaptureCurve
	public int curvesAccepted; //starts at int 0. How many ecc have been accepted (will be rows in treeview_encoder_capture_curves)
	
	public EncoderCaptureCurveArray() {
		ecc = new ArrayList(0);
		curvesAccepted = 0;
	}

	~EncoderCaptureCurveArray() {}
}

public class EncoderBarsData
{
	public double Start;
	public double Duration;
	public double Range;
	public double MeanSpeed;
	public double MaxSpeed;
	public double MeanForce;
	public double MaxForce;
	public double MeanPower;
	public double PeakPower;
	public double WorkJ;
	public double Impulse;
	
	public EncoderBarsData (double start, double duration, double range,
			double meanSpeed, double maxSpeed, double meanForce, double maxForce,
			double meanPower, double peakPower, double workJ, double impulse)
	{
		this.Start = start;
		this.Duration = duration;
		this.Range = range;
		this.MeanSpeed = meanSpeed;
		this.MaxSpeed  = maxSpeed;
		this.MeanForce = meanForce;
		this.MaxForce  = maxForce;
		this.MeanPower = meanPower;
		this.PeakPower = peakPower;
		this.WorkJ = workJ;
		this.Impulse = impulse;
	}

	public double GetValue (string option)
	{
		if(option == Constants.Start)
			return Start;
		if(option == Constants.Duration)
			return Duration;
		//if(option == Constants.Range)
		//	return Range;
		else if(option == Constants.RangeAbsolute)
			return Math.Abs(Range);
		else if(option == Constants.MeanSpeed)
			return MeanSpeed;
		else if(option == Constants.MaxSpeed)
			return MaxSpeed;
		else if(option == Constants.MeanForce)
			return MeanForce;
		else if(option == Constants.MaxForce)
			return MaxForce;
		else if(option == Constants.MeanPower)
			return MeanPower;
		else if(option == Constants.PeakPower)
			return PeakPower;
		else if(option == Constants.WorkJ)
			return WorkJ;
		else if(option == Constants.Impulse)
			return Impulse;

		return MeanPower;
	}
	
	~EncoderBarsData() {}
}

public class EncoderConfigurationSQLObject
{
	public int uniqueID;
	public Constants.EncoderGI encoderGI;
	public bool active; //true or false. One true for each encoderGI (GRAVITATORY, INERTIAL)
	public string name;
	public EncoderConfiguration encoderConfiguration;
	public string description;

	public EncoderConfigurationSQLObject()
	{
		uniqueID = -1;
	}

	public EncoderConfigurationSQLObject(int uniqueID,
			Constants.EncoderGI encoderGI, bool active, string name,
			EncoderConfiguration encoderConfiguration,
			string description)
	{
		this.uniqueID = uniqueID;
		this.encoderGI = encoderGI;
		this.active = active;
		this.name = name;
		this.encoderConfiguration = encoderConfiguration;
		this.description = description;
	}

	//converts encoderConfiguration string from SQL
	public EncoderConfigurationSQLObject(int uniqueID,
			Constants.EncoderGI encoderGI, bool active, string name,
			string encoderConfigurationString,
			string description)
	{
		string [] strFull = encoderConfigurationString.Split(new char[] {':'});
		EncoderConfiguration econf = new EncoderConfiguration(
				(Constants.EncoderConfigurationNames)
				Enum.Parse(typeof(Constants.EncoderConfigurationNames), strFull[0]) );
		econf.ReadParamsFromSQL(strFull);

		this.uniqueID = uniqueID;
		this.encoderGI = encoderGI;
		this.active = active;
		this.name = name;
		this.encoderConfiguration = econf;
		this.description = description;
	}

	//imports from file
	public EncoderConfigurationSQLObject(string contents)
	{
		string line;
		using (StringReader reader = new StringReader (contents)) {
			do {
				line = reader.ReadLine ();

				if (line == null)
					break;
				if (line == "" || line[0] == '#')
					continue;

				string [] parts = line.Split(new char[] {'='});
				if(parts.Length != 2)
					continue;

				uniqueID = -1;
				if(parts[0] == "encoderGI")
				{
					if(Enum.IsDefined(typeof(Constants.EncoderGI), parts[1]))
						encoderGI = (Constants.EncoderGI) Enum.Parse(typeof(Constants.EncoderGI), parts[1]);
				}

				//active is not needed on import, because on import it's always marked as active
				else if(parts[0] == "active" && parts[1] != "")
					active = (parts[1] == "True");
				else if(parts[0] == "name" && parts[1] != "")
					name = parts[1];
				else if(parts[0] == "EncoderConfiguration")
				{
					string [] ecFull = parts[1].Split(new char[] {':'});
					if(Enum.IsDefined(typeof(Constants.EncoderConfigurationNames), ecFull[0]))
					{
						//create object
						encoderConfiguration = new EncoderConfiguration(
								(Constants.EncoderConfigurationNames)
								Enum.Parse(typeof(Constants.EncoderConfigurationNames), ecFull[0]) );
						//assign the rest of params
						encoderConfiguration.ReadParamsFromSQL(ecFull);
					}
				}
				else if(parts[0] == "description" && parts[1] != "")
					description = parts[1];
			} while(true);
		}
	}

	public string ToSQLInsert()
	{
		 string idStr = uniqueID.ToString();
		 if(idStr == "-1")
			 idStr = "NULL";

		 return idStr +
			 ", \"" + encoderGI.ToString() + "\"" +
			 ", \"" + active.ToString() + "\"" +
			 ", \"" + name + "\"" +
			 ", \"" + encoderConfiguration.ToStringOutput(EncoderConfiguration.Outputs.SQL) + "\"" +
			 ", \"" + description + "\"" +
			 ", \"\", \"\", \"\""; //future1, future2, future3
	}

	public string ToFile()
	{
		return
			"#Case sensitive!\n" +
			"#Comments start with sharp sign\n" +
			"#Options are key/values with an = separating them\n" +
			"#DO NOT write comments in the same line than key/value pairs\n" +
			"#\n" +
			"#DO NOT WRITE SPACES JUST BEFORE OR AFTER THE '=' SIGN\n" +
			"#This work:\n" +
			"#name=My encoder config\n" +
			"#This doesn't work:\n" +
			"#name= My encoder config\n" +
			"#\n" +
			"#Whitelines are allowed\n" +
			"\nname=" + name + "\n" +
			"description=" + description + "\n" +
			"\n#encoderGI must be GRAVITATORY or INERTIAL\n" +
			"encoderGI=" + encoderGI.ToString() + "\n" +
			"\n#EncoderConfiguration if exists, this will be used and cannot be changed\n" +
"#name:d:D:anglePush:angleWeight:inertiaMachine:gearedDown:inertiaTotal:extraWeightN:extraWeightGrams:extraWeightLenght:list_d\n" +
"#list_d is list of anchorages in centimeters. each value separated by '_' . Decimal separator is '.'\n" +
			"EncoderConfiguration=" + encoderConfiguration.ToStringOutput(EncoderConfiguration.Outputs.SQL);
	}
}

public class EncoderConfiguration
{
	public Constants.EncoderConfigurationNames name;
	public Constants.EncoderType type;
	public int position; //used to find values on the EncoderConfigurationList. Numeration changes on every encoder and on not inertial/inertial
	public string image;
	public string code;	//this code will be stored untranslated but will be translated just to be shown
	public string text;
	public bool has_d;	//axis
	public bool has_D;	//external disc or pulley
	public bool has_angle_push;
	public bool has_angle_weight;
	public bool has_inertia;
	public bool has_gearedDown;
	public bool rotaryFrictionOnAxis;
	public double d;	//axis 		//ATTENTION: this inertial param can be changed on main GUI
	public double D;	//external disc or pulley
	public int anglePush;
	public int angleWeight;
	
	public int inertiaMachine; //this is the inertia without the disc
	
	// see methods: GearedUpDisplay() SetGearedDownFromDisplay(string gearedUpStr) 
	public int gearedDown;	//demultiplication
	
	public int inertiaTotal; //this is the inertia used by R
	public int extraWeightN; //how much extra weights (inertia) //ATTENTION: this param can be changed on main GUI
	public int extraWeightGrams; //weight of each extra weight (inertia)
	public double extraWeightLength; //length from center to center (cm) (inertia)
	
	public List_d list_d;	//object managing a list of diameters depending on the anchorage position


	public string textDefault = Catalog.GetString("Linear encoder attached to a barbell.") + "\n" + 
		Catalog.GetString("Also common gym tests like jumps or chin-ups.");

	//this is the default values
	public EncoderConfiguration()
	{
		name = Constants.EncoderConfigurationNames.LINEAR;
		type = Constants.EncoderType.LINEAR;
		position = 0;
		image = Constants.FileNameEncoderLinearFreeWeight;
		code = Constants.DefaultEncoderConfigurationCode;
		text = textDefault;

		setDefaultOptions();
	}

	// note: if this changes, change also in:
	// UtilEncoder.EncoderConfigurationList(enum encoderType)
	
	public EncoderConfiguration(Constants.EncoderConfigurationNames name)
	{
		this.name = name;
		setDefaultOptions();

		// ---- LINEAR ----
		// ---- not inertial
		if(name == Constants.EncoderConfigurationNames.LINEAR) {
			type = Constants.EncoderType.LINEAR;
			position = 0;
			image = Constants.FileNameEncoderLinearFreeWeight;
			code = Constants.DefaultEncoderConfigurationCode;
			text = textDefault;
		}
		else if(name == Constants.EncoderConfigurationNames.LINEARINVERTED) {
			type = Constants.EncoderType.LINEAR;
			position = 1;
			image =Constants.FileNameEncoderLinearFreeWeightInv;
			code = "Linear inv - barbell";
			text = Catalog.GetString("Linear encoder inverted attached to a barbell.");
		}
		else if(name == Constants.EncoderConfigurationNames.WEIGHTEDMOVPULLEYLINEARONPERSON1) {
			type = Constants.EncoderType.LINEAR;
			position = 2;
			image = Constants.FileNameEncoderWeightedMovPulleyOnPerson1;
			code = "Linear - barbell - moving pulley";
			text = Catalog.GetString("Linear encoder attached to a barbell.") + " " + 
				Catalog.GetString("Barbell is connected to a weighted moving pulley.") 
				+ " " + Catalog.GetString("Mass is geared down by 2."); 
		
			gearedDown = 2;
		}
		else if(name == Constants.EncoderConfigurationNames.WEIGHTEDMOVPULLEYLINEARONPERSON1INV) {
			type = Constants.EncoderType.LINEAR;
			position = 3;
			image = Constants.FileNameEncoderWeightedMovPulleyOnPerson1Inv;
			code = "Linear inv - barbell - moving pulley";
			text = Catalog.GetString("Linear encoder inverted attached to a barbell.") + " " + 
				Catalog.GetString("Barbell is connected to a weighted moving pulley.")
				+ " " + Catalog.GetString("Mass is geared down by 2."); 
		
			gearedDown = 2;
		}
		else if(name == Constants.EncoderConfigurationNames.WEIGHTEDMOVPULLEYLINEARONPERSON2) {
			type = Constants.EncoderType.LINEAR;
			position = 4;
			image = Constants.FileNameEncoderWeightedMovPulleyOnPerson2;
			code = "Linear - barbell - pulley - moving pulley";
			text = Catalog.GetString("Linear encoder attached to a barbell.") + " " + 
				Catalog.GetString("Barbell is connected to a fixed pulley that is connected to a weighted moving pulley.")
				+ " " + Catalog.GetString("Mass is geared down by 2."); 
		
			gearedDown = 2;
		}
		else if(name == Constants.EncoderConfigurationNames.WEIGHTEDMOVPULLEYLINEARONPERSON2INV) {
			type = Constants.EncoderType.LINEAR;
			position = 5;
			image = Constants.FileNameEncoderWeightedMovPulleyOnPerson2Inv;
			code = "Linear inv - barbell - pulley - moving pulley";
			text = Catalog.GetString("Linear encoder inverted attached to a barbell.") + " " + 
				Catalog.GetString("Barbell is connected to a fixed pulley that is connected to a weighted moving pulley.")
				+ " " + Catalog.GetString("Mass is geared down by 2."); 
		
			gearedDown = 2;
		}
		else if(name == Constants.EncoderConfigurationNames.WEIGHTEDMOVPULLEYONLINEARENCODER) {
			type = Constants.EncoderType.LINEAR;
			position = 6;
			image = Constants.FileNameEncoderWeightedMovPulleyOnLinearEncoder;
			code = "Linear - moving pulley";
			text = Catalog.GetString("Linear encoder attached to a weighted moving pulley.")
				+ " " + Catalog.GetString("Mass is geared down by 2."); 
		
			gearedDown = 2;
		}
		else if(name == Constants.EncoderConfigurationNames.LINEARONPLANE) {
			type = Constants.EncoderType.LINEAR;
			position = 7;
			image = Constants.FileNameEncoderLinearOnPlane;
			code = "Linear - inclined plane";
			text = Catalog.GetString("Linear encoder on a inclined plane.") + "\n" + 
				Catalog.GetString("Suitable also for horizontal movement. Just set a 0 push angle.");
			
			has_angle_push = true;
			has_angle_weight = false;
		}
		else if(name == Constants.EncoderConfigurationNames.LINEARONPLANEWEIGHTDIFFANGLE) {
			type = Constants.EncoderType.LINEAR;
			position = 8;
			image = Constants.FileNameEncoderLinearOnPlaneWeightDiffAngle;
			code = "Linear - inclined plane different angle";
			text = Catalog.GetString("Linear encoder on a inclined plane moving a weight in different angle.") + "\n" +
				Catalog.GetString("Suitable also for horizontal movement. Just set a 0 push angle.");
			
			has_angle_push = true;
			has_angle_weight = true;
		}
		else if(name == Constants.EncoderConfigurationNames.LINEARONPLANEWEIGHTDIFFANGLEMOVPULLEY) {
			type = Constants.EncoderType.LINEAR;
			position = 9;
			image = Constants.FileNameEncoderLinearOnPlaneWeightDiffAngleMovPulley;
			code = "Linear - inclined plane different angle - moving pulley";
			text = Catalog.GetString("Linear encoder on a inclined plane moving a weight in different angle.") + "\n" +
				Catalog.GetString("Suitable also for horizontal movement. Just set a 0 push angle.") + "\n" +
				Catalog.GetString("Force demultiplier refers to the times the rope comes in and comes out from the moving pulley attached to the extra load.") +
				" " + Catalog.GetString("In the example image demultiplier is 2, hence multiplier is 1/2.");

			has_angle_push = true;
			has_angle_weight = true;
			has_gearedDown = true;
		}
		if(name == Constants.EncoderConfigurationNames.PNEUMATIC) {
			type = Constants.EncoderType.LINEAR;
			position = 10;
			image = Constants.FileNameEncoderLinearPneumatic;
			code = "Linear - Pneumatic machine";
			text = "Linear encoder connected to a pneumatic machine";

			has_angle_push = true;
		}
		// ---- inertial
		else if(name == Constants.EncoderConfigurationNames.LINEARINERTIAL) {
			type = Constants.EncoderType.LINEAR;
			position = 0;
			image = Constants.FileNameEncoderLinearInertial;
			code = "Linear - inertial machine";
			text = Catalog.GetString("Linear encoder on inertia machine.") + "\n" + 
				Catalog.GetString("Configuration NOT Recommended! Please use a rotary encoder.") + "\n" +
				Catalog.GetString("On inertial machines, 'd' means the average diameter where the pull-push string is rolled");
			
			has_d = true;
			has_inertia = true;
		}
		// ---- ROTARY FRICTION ----
		// ---- not inertial
		else if(name == Constants.EncoderConfigurationNames.ROTARYFRICTIONSIDE) {
			type = Constants.EncoderType.ROTARYFRICTION;
			position = 0;
			image = Constants.FileNameEncoderFrictionSide;
			code = "Rotary friction - pulley";
			text = Catalog.GetString("Rotary friction encoder on pulley.");
		}
		else if(name == Constants.EncoderConfigurationNames.ROTARYFRICTIONAXIS) {
			type = Constants.EncoderType.ROTARYFRICTION;
			position = 1;
			image = Constants.FileNameEncoderFrictionAxis;
			code = "Rotary friction - pulley axis";
			text = Catalog.GetString("Rotary friction encoder on pulley axis.");

			has_d = true;
			has_D = true;
		}
		else if(name == Constants.EncoderConfigurationNames.WEIGHTEDMOVPULLEYROTARYFRICTION) {
			type = Constants.EncoderType.ROTARYFRICTION;
			position = 2;
			image = Constants.FileNameEncoderFrictionWithMovPulley;
			code = "Rotary friction - moving pulley";
			text = Catalog.GetString("Rotary friction encoder on weighted moving pulley.");
		}
		// ---- inertial
		// ---- rotary friction not on axis
		else if(name == Constants.EncoderConfigurationNames.ROTARYFRICTIONSIDEINERTIAL) {
			type = Constants.EncoderType.ROTARYFRICTION;
			position = 0;
			image = Constants.FileNameEncoderFrictionSideInertial;
			code = "Rotary friction - inertial machine side";
			text = Catalog.GetString("Rotary friction encoder on inertial machine side.") + "\n" +
				Catalog.GetString("On inertial machines, 'd' means the average diameter where the pull-push string is rolled");

			has_d = true;
			has_D = true;
			has_inertia = true;
		}
		else if(name == Constants.EncoderConfigurationNames.ROTARYFRICTIONSIDEINERTIALLATERAL) {
			type = Constants.EncoderType.ROTARYFRICTION;
			position = 1;
			image = Constants.FileNameEncoderFrictionSideInertialLateral;
			code = "Rotary friction - inertial machine side - horizontal movement";
			text = Catalog.GetString("Rotary friction encoder on inertial machine when person is moving horizontally.") + "\n" +
				Catalog.GetString("On inertial machines, 'd' means the average diameter where the pull-push string is rolled");

			has_d = true;
			has_D = true;
			has_inertia = true;
		}
		else if(name == Constants.EncoderConfigurationNames.ROTARYFRICTIONSIDEINERTIALMOVPULLEY) {
			type = Constants.EncoderType.ROTARYFRICTION;
			position = 2;
			image = Constants.FileNameEncoderFrictionSideInertialMovPulley;
			code = "Rotary friction - inertial machine side geared up";
			text = Catalog.GetString("Rotary friction encoder on inertial machine geared up.") + "\n" +
				Catalog.GetString("On inertial machines, 'd' means the average diameter where the pull-push string is rolled") + "\n" + 
				Catalog.GetString("Inertial machine rolls twice faster than body."); 

			has_d = true;
			has_D = true;
			has_inertia = true;
			has_gearedDown = true;
		}

		// ---- rotary friction on axis
		else if(name == Constants.EncoderConfigurationNames.ROTARYFRICTIONAXISINERTIAL) {
			type = Constants.EncoderType.ROTARYFRICTION;
			position = 0;
			image = Constants.FileNameEncoderFrictionAxisInertial;
			code = "Rotary friction axis - inertial machine axis";
			text = Catalog.GetString("Rotary friction encoder on inertial machine axis.") + "\n" +
				Catalog.GetString("On inertial machines, 'd' means the average diameter where the pull-push string is rolled");

			has_d = true;
			has_inertia = true;
			rotaryFrictionOnAxis = true;
		}
		else if(name == Constants.EncoderConfigurationNames.ROTARYFRICTIONAXISINERTIALLATERAL) {
			type = Constants.EncoderType.ROTARYFRICTION;
			position = 1;
			image = Constants.FileNameEncoderFrictionAxisInertialLateral;
			code = "Rotary friction - inertial machine axis - horizontal movement";
			text = Catalog.GetString("Rotary friction encoder on inertial machine when person is moving horizontally.") + "\n" +
				Catalog.GetString("On inertial machines, 'd' means the average diameter where the pull-push string is rolled");

			has_d = true;
			has_inertia = true;
			rotaryFrictionOnAxis = true;
		}
		else if(name == Constants.EncoderConfigurationNames.ROTARYFRICTIONAXISINERTIALMOVPULLEY) {
			type = Constants.EncoderType.ROTARYFRICTION;
			position = 2;
			image = Constants.FileNameEncoderFrictionAxisInertialMovPulley;
			code = "Rotary friction - inertial machine axis geared up";
			text = Catalog.GetString("Rotary friction encoder on inertial machine geared up.") + "\n" +
				Catalog.GetString("On inertial machines, 'd' means the average diameter where the pull-push string is rolled") + "\n" + 
				Catalog.GetString("Inertial machine rolls twice faster than body."); 

			has_d = true;
			has_inertia = true;
			rotaryFrictionOnAxis = true;
			has_gearedDown = true;
		}

		// ---- ROTARY AXIS ----
		// ---- not inertial
		else if(name == Constants.EncoderConfigurationNames.ROTARYAXIS) {
			type = Constants.EncoderType.ROTARYAXIS;
			position = 0;
			image = Constants.FileNameEncoderRotaryAxisOnAxis;
			code = "Rotary axis - pulley axis";
			text = Catalog.GetString("Rotary axis encoder on pulley axis.");

			has_D = true;
		}
		else if(name == Constants.EncoderConfigurationNames.WEIGHTEDMOVPULLEYROTARYAXIS) {
			type = Constants.EncoderType.ROTARYAXIS;
			position = 1;
			image = Constants.FileNameEncoderAxisWithMovPulley;
			code = "Rotary axis - moving pulley";
			text = Catalog.GetString("Rotary axis encoder on weighted moving pulley.")
				+ " " + Catalog.GetString("Mass is geared down by 2."); 
			
			gearedDown = 2;
		}
		// ---- inertial
		else if(name == Constants.EncoderConfigurationNames.ROTARYAXISINERTIAL) {
			type = Constants.EncoderType.ROTARYAXIS;
			position = 0;
			image = Constants.FileNameEncoderAxisInertial;
			code = "Rotary axis - inertial machine";
			text = Catalog.GetString("Rotary axis encoder on inertial machine.") + "\n" +
				Catalog.GetString("On inertial machines, 'd' means the average diameter where the pull-push string is rolled");

			has_d = true;
			has_inertia = true;
		}
		else if(name == Constants.EncoderConfigurationNames.ROTARYAXISINERTIALLATERAL) {
			type = Constants.EncoderType.ROTARYAXIS;
			position = 1;
			image = Constants.FileNameEncoderAxisInertialLateral;
			code = "Rotary axis - inertial machine - horizontal movement";
			text = Catalog.GetString("Rotary axis encoder on inertial machine when person is moving horizontally.") + "\n" +
				Catalog.GetString("On inertial machines, 'd' means the average diameter where the pull-push string is rolled");

			has_d = true;
			has_inertia = true;
		}
		else if(name == Constants.EncoderConfigurationNames.ROTARYAXISINERTIALMOVPULLEY) {
			type = Constants.EncoderType.ROTARYAXIS;
			position = 2;
			image = Constants.FileNameEncoderAxisInertialMovPulley;
			code = "Rotary axis - inertial machine geared up";
			text = Catalog.GetString("Rotary axis encoder on inertial machine geared up.") + "\n" +
				Catalog.GetString("On inertial machines, 'd' means the average diameter where the pull-push string is rolled") + "\n" + 
				Catalog.GetString("Inertial machine rolls twice faster than body."); 

			has_d = true;
			has_inertia = true;
			has_gearedDown = true;
		}
		else if(name == Constants.EncoderConfigurationNames.ROTARYAXISINERTIALLATERALMOVPULLEY) {
			type = Constants.EncoderType.ROTARYAXIS;
			position = 3;
			image = Constants.FileNameEncoderAxisInertialMovPulleyLateral;
			code = "Rotary axis - inertial machine - horizontal movement - geared up";
			text = Catalog.GetString("Rotary axis encoder on inertial machine geared up when person is moving horizontally.") + "\n" +
				Catalog.GetString("On inertial machines, 'd' means the average diameter where the pull-push string is rolled") + "\n" +
				Catalog.GetString("Inertial machine rolls twice faster than body.");

			has_d = true;
			has_inertia = true;
			has_gearedDown = true;
		}
	}

	private void setDefaultOptions()
	{
		has_d = false;
		has_D = false;
		has_angle_push = false;
		has_angle_weight = false;
		has_inertia = false;
		has_gearedDown = false; //gearedDown can be changed by user
		rotaryFrictionOnAxis = false;
		d = -1;
		D = -1;
		anglePush = -1;
		angleWeight = -1;
		inertiaMachine = -1;
		gearedDown = 1;
		inertiaTotal = -1;
		extraWeightN = 0;
		extraWeightGrams = 0;
		extraWeightLength = 1;
		list_d = new List_d();
	}

	public void SetInertialDefaultOptions()
	{
		//after creating Constants.EncoderConfigurationNames.ROTARYAXISINERTIAL
		inertiaMachine = 900;
		d = 5;
		list_d = new List_d(d);
		inertiaTotal = UtilEncoder.CalculeInertiaTotal(this);
	}

	public bool Equals(EncoderConfiguration other)
	{
		return (this.ToStringOutput(Outputs.SQL) == other.ToStringOutput(Outputs.SQL));
	}

	public void ReadParamsFromSQL (string [] strFull) 
	{
		//adds other params
		this.d = 	   Convert.ToDouble(Util.ChangeDecimalSeparator(strFull[1]));
		this.D = 	   Convert.ToDouble(Util.ChangeDecimalSeparator(strFull[2]));
		this.anglePush =   Convert.ToInt32(strFull[3]);
		this.angleWeight = Convert.ToInt32(strFull[4]);
		this.inertiaMachine = 	Convert.ToInt32(strFull[5]);
		this.gearedDown =  Convert.ToInt32(strFull[6]);
		this.inertiaTotal = 	Convert.ToInt32(strFull[7]);
		this.extraWeightN = 	Convert.ToInt32(strFull[8]);
		this.extraWeightGrams = Convert.ToInt32(strFull[9]);
		this.extraWeightLength = Convert.ToDouble(Util.ChangeDecimalSeparator(strFull[10]));

		//check needed when updating DB to 1.36
		if(strFull.Length == 12)
			this.list_d.ReadFromSQL(strFull[11]);
	}

	public enum Outputs { ROPTIONS, RCSV, SQL, SQLECWINCOMPARE}
	/*
	 * SQLECWINCOMPARE is to know it two encoderConfigurations on encoderConfigurationWindow are the same
	 * because two inertial params change outside that window
	 */
	
	public string ToStringOutput(Outputs o) 
	{
		//for R and SQL		
		string str_d = Util.ConvertToPoint(d);
		string str_D = Util.ConvertToPoint(D);
		
		string sep = "";

		if(o == Outputs.ROPTIONS) {
			sep = "\n";
			return 
				"#name" + sep + 	name + sep + 
				"#str_d" + sep + 	str_d + sep + 
				"#str_D" + sep + 	str_D + sep + 
				"#anglePush" + sep + 	anglePush.ToString() + sep + 
				"#angleWeight" + sep + 	angleWeight.ToString() + sep +
				"#inertiaTotal" + sep + inertiaTotal.ToString() + sep + 
				"#gearedDown" + sep + 	gearedDown.ToString()
				;
		}
		else if (o == Outputs.RCSV) { //not single curve
			sep = ",";
			//do not need inertiaMachine, extraWeightN, extraWeightGrams, extraWeightLength (unneded for the R calculations)
			return 
				name + sep + 
				str_d + sep + 
				str_D + sep + 
				anglePush.ToString() + sep + 
				angleWeight.ToString() + sep +
				inertiaTotal.ToString() + sep + 
				gearedDown.ToString()
				;
		}
		else { //(o == Outputs.SQL || o == OUTPUTS.SQLECWINCOMPARE)
			sep = ":";

			string my_str_d = str_d;
			string my_str_extraWeightN = extraWeightN.ToString();

			if(o == Outputs.SQLECWINCOMPARE)
			{
				//this inertial params can be changed on main GUI
				my_str_d = "%";
				my_str_extraWeightN  = "%";
			}
			return 
				name + sep + 
				my_str_d + sep +
				str_D + sep + 
				anglePush.ToString() + sep + 
				angleWeight.ToString() + sep +
				inertiaMachine.ToString() + sep + 
				gearedDown.ToString() + sep + 
				inertiaTotal.ToString() + sep + 
				my_str_extraWeightN + sep +
				extraWeightGrams.ToString() + sep +
				extraWeightLength.ToString() + sep +
				list_d.ToString();
		}
	}

	//just to show on a treeview	
	public string ToStringPretty() {
		string sep = "; ";

		string str_d = "";
		if(d != -1)
			str_d = sep + "d=" + d.ToString();

		string str_D = "";
		if(D != -1)
			str_D = sep + "D=" + D.ToString();

		string str_anglePush = "";
		if(anglePush != -1)
			str_anglePush = sep + "push angle=" + anglePush.ToString();

		string str_angleWeight = "";
		if(angleWeight != -1)
			str_angleWeight = sep + "weight angle=" + angleWeight.ToString();

		string str_inertia = "";
		if(has_inertia && inertiaTotal != -1)
			str_inertia = sep + "inertia total=" + inertiaTotal.ToString();

		string str_gearedDown = "";
		if(gearedDown != 1)	//1 is the default
			str_gearedDown = sep + "geared down=" + gearedDown.ToString();

		return code + str_d + str_D + str_anglePush + str_angleWeight + str_inertia + str_gearedDown;
	}

	// while capture to show correctly on screen and to send the correct concentric cut to R
	public bool IsInverted ()
	{
		if (name == Constants.EncoderConfigurationNames.LINEARINVERTED ||
				name == Constants.EncoderConfigurationNames.WEIGHTEDMOVPULLEYLINEARONPERSON1INV ||
				name == Constants.EncoderConfigurationNames.WEIGHTEDMOVPULLEYLINEARONPERSON2INV)
			return true;

		return false;
	}

	/*
	 * IMPORTANT: on GUI is gearedDown is shown as UP (for clarity: 4, 3, 2, 1, 1/2, 1/3, 1/4)
	 * on C#, R, SQL we use "gearedDown" for historical reasons. So a conversion is done on displaying data to user
	 * gearedDown is stored as integer on database and is converted to this gearedUp for GUI
	 * R will do another conversion and will use the double
	 *   4   ,    3    ,  2  , 1/2, 1/3, 1/4		#gearedUp string (GUI)
	 *  -4   ,   -3    , -2  ,   2,   3,   4		#gearedDown
	 *   0.25,    0.333,  0.5,   2,   3,   4		#gearedDown on R (see readFromFile.gearedDown() on util.cs)
	 */
	public string GearedUpDisplay() 
	{
		switch(gearedDown) {
			case -4:
				return "4";
			case -3:
				return "3";
			case -2:
				return "2";
			case 2:
				return "1/2";
			case 3:
				return "1/3";
			case 4:
				return "1/4";
			default:
				return "2";
		}
	}
	public void SetGearedDownFromDisplay(string gearedUpStr) 
	{
		switch(gearedUpStr) {
			case "4":
				gearedDown = -4;
				break;
			case "3":
				gearedDown = -3;
				break;
			case "2":
				gearedDown = -2;
				break;
			case "1/2":
				gearedDown = 2;
				break;
			case "1/3":
				gearedDown = 3;
				break;
			case "1/4":
				gearedDown = 4;
				break;
			default:
				gearedDown = -2;
				break;
		}
	}

}
/*
 * class that manages list of diameters on encoderConfiguration
 * read_list_d returns a List<double>. when reading a "" value from SQL is usually converted to 0 here
 * and then a list_d Add will add a new value
 * control list_d values with this class
 * if there are no diameters, list_d has one value: 0
 */
public class List_d
{
	private List<double> l;

	//default constructor
	public List_d()
	{
		l = new List<double>();
		l.Add(0);
	}
	//constructor with a default value
	public List_d(double d)
	{
		l = new List<double>();
		l.Add(d);
	}

	//list_d contains the different diameters (anchorages). They are stored as '_'
	public void ReadFromSQL(string listFromSQL)
	{
		l = new List<double>();
		string [] strFull = listFromSQL.Split(new char[] {'_'});
		foreach (string s in strFull) {
			double d = Convert.ToDouble(Util.ChangeDecimalSeparator(s));
			l.Add(d);
		}

		if(l.Count == 0)
			l.Add(0);
	}

	public void Add(double d)
	{
		if(l.Count == 1 && l[0] == 0)
			l[0] = d;
		else
			l.Add(d);
	}

	public override string ToString()
	{
		string str = "";
		string sep = "";
		foreach(double d in l) {
			str += sep + Util.ConvertToPoint(d);
			sep = "_";
		}

		if(str == "")
			str = "0";

		return str;
	}

	public bool IsEmpty()
	{
		if(l == null || l.Count == 0 || (l.Count == 1 && l[0] == 0) )
			return true;

		return false;
	}

	public List<double> L
	{
		get { return l; }
	}

}

public class EncoderAnalyzeInstant 
{
	public List<double> displ;
	public List<double> speed;
	public List<double> accel;
	public List<double> force;
	public List<double> power;

	public int graphWidth;
	
	private Rx1y2 usr;
	private Rx1y2 plt;
		
	private double pxPlotArea;
	private double msPlotArea;
	
	//last calculated values on last range of msa and msb
	public double displAverageLast;
	public double displMaxLast;
	public double speedAverageLast;
	public double speedMaxLast;
	public double accelAverageLast;
	public double accelMaxLast;
	public double forceAverageLast;
	public double forceMaxLast;
	public double powerAverageLast;
	public double powerMaxLast;

	public EncoderAnalyzeInstant() {
		displ = new List<double>(); 
		speed = new List<double>(); 
		accel = new List<double>(); 
		force = new List<double>(); 
		power = new List<double>();
		
		graphWidth = 0;
		pxPlotArea = 0;
		msPlotArea = 0;
	}

	//file has a first line with headers
	//2nd.... full data
	public void ReadArrayFile(string filename)
	{
		List<string> lines = Util.ReadFileAsStringList(filename);
		if(lines == null)
			return;
		if(lines.Count <= 1) //return if there's only the header
			return;

		bool headerLine = true;
		foreach(string l in lines) {
				if(headerLine) {
					headerLine = false;
					continue;
				}

			string [] lsplit = l.Split(new char[] {','});
			displ.Add(Convert.ToDouble(Util.ChangeDecimalSeparator(lsplit[1])));
			speed.Add(Convert.ToDouble(Util.ChangeDecimalSeparator(lsplit[2])));
			accel.Add(Convert.ToDouble(Util.ChangeDecimalSeparator(lsplit[3])));
			force.Add(Convert.ToDouble(Util.ChangeDecimalSeparator(lsplit[4])));
			power.Add(Convert.ToDouble(Util.ChangeDecimalSeparator(lsplit[5])));
		}
	}
	
	public void ReadGraphParams(string filename)
	{
		List<string> lines = Util.ReadFileAsStringList(filename);
		if(lines == null)
			return;
		if(lines.Count < 3)
			return;

		graphWidth = Convert.ToInt32(lines[0]);
		usr = new Rx1y2(lines[1]);
		plt = new Rx1y2(lines[2]);

		// calculate the pixels in plot area
		pxPlotArea = graphWidth * (plt.x2 - plt.x1);

		//calculate the ms in plot area
		msPlotArea = usr.x2 - usr.x1;
	}
	
	//gets an instant value
	public double GetParam(string param, int ms) 
	{
		ms --; //converts from starting at 1 (graph) to starting at 0 (data)

		if(ms > displ.Count)
			return -1;

		else {
			if(param == "displ")
				return displ[ms];
			else if(param == "speed")
				return speed[ms];
			else if(param == "accel")
				return accel[ms];
			else if(param == "force")
				return force[ms];
			else if(param == "power")
				return power[ms];
			else
				return -2;
		}
	}
	
	//calculates from a range
	public bool CalculateRangeParams(int msa, int msb)
	{
		msa --; //converts from starting at 1 (graph) to starting at 0 (data)
		msb --; //converts from starting at 1 (graph) to starting at 0 (data)
		
		//if msb < msa invert them
		if(msb < msa) {
			int temp = msa;
			msa = msb;
			msb = temp;
		}

		if(msa > displ.Count || msb > displ.Count)
			return false;

		getAverageAndMax(displ, msa, msb, out displAverageLast, out displMaxLast);
		getAverageAndMax(speed, msa, msb, out speedAverageLast, out speedMaxLast);
		getAverageAndMax(accel, msa, msb, out accelAverageLast, out accelMaxLast);
		getAverageAndMax(force, msa, msb, out forceAverageLast, out forceMaxLast);
		getAverageAndMax(power, msa, msb, out powerAverageLast, out powerMaxLast);
		
		return true;
	}
	private void getAverageAndMax(List<double> dlist, int ini, int end, out double listAVG, out double listMAX) {
		if(ini == end) {
			listAVG = dlist[ini];
			listMAX = dlist[ini];
			return;
		}

		double sum = 0;
		double max = - 1000000;
		for(int i = ini; i <= end; i ++) {
			sum += dlist[i];
			if(dlist[i] > max)
				max = dlist[i];
		}

		listAVG = sum / (end - ini + 1); //+1 because count starts at 0
		listMAX = max;
	}


	public int GetVerticalLinePosition(int ms) 
	{
		//this can be called on expose event before calculating needed parameters
		if(graphWidth == 0 || pxPlotArea == 0 || msPlotArea == 0)
			return 0;

		// rule of three
		double px = (ms - usr.x1) * pxPlotArea / msPlotArea;

		// fix margin
		px = px + plt.x1 * graphWidth;

		return Convert.ToInt32(px);
	}
	
	public void ExportToCSV(int msa, int msb, string selectedFileName, string sepString) 
	{
		//if msb < msa invert them
		if(msb < msa) {
			int temp = msa;
			msa = msb;
			msb = temp;
		}

		//this overwrites if needed
		TextWriter writer = File.CreateText(selectedFileName);

		string sep = " ";
		if (sepString == "COMMA")
			sep = ";";
		else
			sep = ",";

		string header = 
			"" + sep +
			Catalog.GetString("Time") + sep + 
			Catalog.GetString("Displacement") + sep +
			Catalog.GetString("Speed") + sep +
			Catalog.GetString("Acceleration") + sep +
			Catalog.GetString("Force") + sep +
			Catalog.GetString("Power");
			
		//write header
		writer.WriteLine(header);

		//write statistics
		writer.WriteLine(
				Catalog.GetString("Difference") + sep +
				(msb-msa).ToString() + sep +
				Util.DoubleToCSV( (GetParam("displ",msb) - GetParam("displ",msa)), sepString ) + sep +
				Util.DoubleToCSV( (GetParam("speed",msb) - GetParam("speed",msa)), sepString ) + sep +
				Util.DoubleToCSV( (GetParam("accel",msb) - GetParam("accel",msa)), sepString ) + sep +
				Util.DoubleToCSV( (GetParam("force",msb) - GetParam("force",msa)), sepString ) + sep +
				Util.DoubleToCSV( (GetParam("power",msb) - GetParam("power",msa)), sepString ) );
		
		//done here because GetParam does the same again, and if we put it in the top of this method, it will be done two times
		msa --; //converts from starting at 1 (graph) to starting at 0 (data)
		msb --; //converts from starting at 1 (graph) to starting at 0 (data)
		
		writer.WriteLine(
				Catalog.GetString("Average") + sep +
				"" + sep +
				Util.DoubleToCSV(displAverageLast, sepString) + sep +
				Util.DoubleToCSV(speedAverageLast, sepString) + sep +
				Util.DoubleToCSV(accelAverageLast, sepString) + sep +
				Util.DoubleToCSV(forceAverageLast, sepString) + sep +
				Util.DoubleToCSV(powerAverageLast, sepString) );
		
		writer.WriteLine(
				Catalog.GetString("Maximum") + sep +
				"" + sep +
				Util.DoubleToCSV(displMaxLast, sepString) + sep +
				Util.DoubleToCSV(speedMaxLast, sepString) + sep +
				Util.DoubleToCSV(accelMaxLast, sepString) + sep +
				Util.DoubleToCSV(forceMaxLast, sepString) + sep +
				Util.DoubleToCSV(powerMaxLast, sepString) );

		//blank line
		writer.WriteLine();

		//write header
		writer.WriteLine(header);

		//write data
		for(int i = msa; i <= msb; i ++)
			writer.WriteLine(
					"" + sep +
					(i+1).ToString() + sep +
					Util.DoubleToCSV(displ[i], sepString) + sep +
					Util.DoubleToCSV(speed[i], sepString) + sep +
					Util.DoubleToCSV(accel[i], sepString) + sep +
					Util.DoubleToCSV(force[i], sepString) + sep +
					Util.DoubleToCSV(power[i], sepString) );

		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();
	}

	public void PrintDebug() {
		LogB.Information("Printing speed");
		foreach(double s in speed)
			LogB.Debug(s.ToString());
	}
}

//for objects coming from R that have "x1 x2 y1 y2" like usr or par
public class Rx1y2 
{
	public double x1;
	public double x2;
	public double y1;
	public double y2;

	public Rx1y2 (string s) {
		string [] sFull = s.Split(new char[] {' '});
		x1 = Convert.ToDouble(Util.ChangeDecimalSeparator(sFull[0]));
		x2 = Convert.ToDouble(Util.ChangeDecimalSeparator(sFull[1]));
		y1 = Convert.ToDouble(Util.ChangeDecimalSeparator(sFull[2]));
		y2 = Convert.ToDouble(Util.ChangeDecimalSeparator(sFull[3]));
	}
}
