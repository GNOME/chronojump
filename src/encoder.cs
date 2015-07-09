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
 *  Copyright (C) 2004-2015   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using System.Text; //StringBuilder
using System.IO;   //for Path
using System.Collections; //ArrayList
using Mono.Unix;

public class EncoderParams
{
	private int time;
	private string mass; //to pass always as "." to R
	
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
					
	//encoderConfiguration conversions
	//in signals and curves, need to do conversions (invert, inertiaMomentum, diameter)
	private EncoderConfiguration encoderConfiguration;	
	
	private string smoothCon; //to pass always as "." to R
	private int curve;
	private int width;
	private int height;
	private double heightHigherCondition;
	private double heightLowerCondition;
	private double meanSpeedHigherCondition;
	private double meanSpeedLowerCondition;
	private double maxSpeedHigherCondition;
	private double maxSpeedLowerCondition;
	private int powerHigherCondition;
	private int powerLowerCondition;
	private int peakPowerHigherCondition;
	private int peakPowerLowerCondition;
	private string mainVariable;
	private string decimalSeparator;	//used in export data from R to csv
	//private bool inverted; //used only in runEncoderCapturePython. In graph.R will be used encoderConfigurationName

	public EncoderParams()
	{
	}

	
	//to graph.R	
	public EncoderParams(int minHeight, int exercisePercentBodyWeight, string massBody, string massExtra, 
			string eccon, string analysis, string analysisVariables, string analysisOptions, 
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
		this.encoderConfiguration = encoderConfiguration;
		this.smoothCon = smoothCon;
		this.curve = curve;
		this.width = width;
		this.height = height;
		this.decimalSeparator = decimalSeparator;
	}
	
	public string ToStringROptions () 
	{
		return 
			"#minHeight\n" + 	minHeight + "\n" + 
			"#exercisePercentBodyWeight\n" + exercisePercentBodyWeight + "\n" + 
			"#massBody\n" + 	massBody + "\n" + 
			"#massExtra\n" + 	massExtra + "\n" + 
			"#eccon\n" + 		eccon + "\n" + 
			"#analysis\n" + 	analysis + "\n" + 
			"#analysisVariables\n" + analysisVariables + "\n" + 
			"#analysisOptions\n" + analysisOptions + "\n" + 
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
	
	public int Time {
		get { return time; }
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
	public string OutputData2;
	public string SpecialData;
	public EncoderParams Ep;

	public EncoderStruct(string InputData, string OutputGraph, string OutputData1, string OutputData2,
			string SpecialData, EncoderParams Ep)
	{
		this.InputData = InputData;
		this.OutputGraph = OutputGraph;
		this.OutputData1 = OutputData1;
		this.OutputData2 = OutputData2;
		this.SpecialData = SpecialData;
		this.Ep = Ep;
	}

	~EncoderStruct() {}
}

public class EncoderGraphROptions
{
	public string inputData;
	public string outputGraph;
	public string outputData1;
	public string outputData2;
	public string specialData;
	public EncoderParams ep;
	public string title;
	public string operatingSystem;
	public string scriptUtilR;
	public string scriptNeuromuscularProfile;
	public string englishWords;
	public string translatedWords;
	public string scriptGraphR;
	
	public EncoderGraphROptions(
			string inputData, string outputGraph, string outputData1, 
			string outputData2, string specialData, 
			EncoderParams ep,
			string title, string operatingSystem,
			string scriptUtilR, string scriptNeuromuscularProfile,
			string englishWords, string translatedWords,
			string scriptGraphR) 
	{
		this.inputData = inputData;
		this.outputGraph = outputGraph;
		this.outputData1 = outputData1;
		this.outputData2 = outputData2;
		this.specialData = specialData;
		this.ep = ep;
		this.title = title;
		this.operatingSystem = operatingSystem;
		this.scriptUtilR = scriptUtilR;
		this.scriptNeuromuscularProfile = scriptNeuromuscularProfile;
		this.englishWords = englishWords;
		this.translatedWords = translatedWords;
		this.scriptGraphR = scriptGraphR;
	}

	public override string ToString() {
		return 
			"#inputdata\n" + 	inputData + "\n" + 
			"#outputgraph\n" + 	outputGraph + "\n" + 
			"#outputdata1\n" + 	outputData1 + "\n" + 
			"#outputdata2\n" + 	outputData2 + "\n" + 
			"#specialdata\n" + 	specialData + "\n" + 
			ep.ToStringROptions() + "\n" + 
			"#title\n" + 		title + "\n" + 
			"#operatingsystem\n" + 	operatingSystem + "\n" +
			"#scriptUtilR\n" + 	scriptUtilR + "\n" + 
			"#scriptNeuromuscularProfile\n" + scriptNeuromuscularProfile + "\n" +
			"#englishWords\n" + 	englishWords + "\n" +
			"#translatedWords\n" + 	translatedWords + "\n" + 
			"#scriptGraphR\n" + 	scriptGraphR + "\n";
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
	
	public EncoderCurve () {
	}

	//used on TreeView capture
	public EncoderCurve (bool record, string n, 
			string start, string duration, string height, 
			string meanSpeed, string maxSpeed, string maxSpeedT,
			string meanPower, string peakPower, string peakPowerT, 
			string PP_PPT,
			string meanForce, string maxForce, string maxForceT
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
	}

	//used on TreeView analyze
	public EncoderCurve (string n, string series, string exercise, 
			string laterality,
			double extraWeight, double displacedWeight,
			string start, string duration, string height,
			string meanSpeed, string maxSpeed, string maxSpeedT,
			string meanPower, string peakPower, string peakPowerT, 
			string PP_PPT,
			string meanForce, string maxForce, string maxForceT)
	{
		this.N = n;
		this.Series = series;
		this.Exercise = exercise;
		this.Laterality = laterality;
		this.ExtraWeight = extraWeight;
		this.DisplacedWeight = displacedWeight;
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
	}

	public string ToCSV(string decimalSeparator) {
		//latin:	2,3 ; 2,5
		//non-latin:	2.3 , 2.5

		string sep = ":::";
		string str = 
			N + sep + Series + sep + Exercise + sep + Laterality + sep +
			ExtraWeight + sep + DisplacedWeight + sep + 
			Start + sep + Duration + sep + Height + sep + 
			MeanSpeed + sep + MaxSpeed + sep + MaxSpeedT + sep + 
			MeanPower + sep + PeakPower + sep + PeakPowerT + sep + 
			PP_PPT + sep +
			MeanForce + sep + MaxForce + sep + MaxForceT;
		
		if(decimalSeparator == "COMMA")
			str = Util.ConvertToComma(str);
		else
			str = Util.ConvertToPoint(str);
			
		if(decimalSeparator == "COMMA")
			return Util.ChangeChars(str, ":::", ";");
		else
			return Util.ChangeChars(str, ":::", ",");
	}
	
	~EncoderCurve() {}
}


//to know which is the best curve in a signal...
public class EncoderSignal
{
	private ArrayList curves;

	public EncoderSignal (ArrayList curves) {
		this.curves = curves;
	}

	//this can be an eccentric or concentric curve
	public int FindPosOfBestMeanPower() {
		double bestMeanPower = 0;
		int bestMeanPowerPos = 0;
		int i = 0;
		foreach(EncoderCurve curve in curves) {
			if(Convert.ToDouble(curve.MeanPower) > bestMeanPower) {
				bestMeanPower = Convert.ToDouble(curve.MeanPower);
				bestMeanPowerPos = i;
			}
			i++;
		}
		return bestMeanPowerPos;
	}
	
	~EncoderSignal() {}
}


//related to encoderSignalCurve table
public class EncoderSignalCurve {
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
	
	public string ToString() {
		return uniqueID.ToString() + ":" + signalID.ToString() + ":" + 
			curveID.ToString() + ":" + msCentral.ToString();
	}
	
	~EncoderSignalCurve() {}
}


//used on TreeView
public class EncoderNeuromuscularData
{
	public string n; 
	public int e1_range;
	public int e1_t;
	public double e1_fmax;
	public double e1_rfd_avg;
	public double e1_i;

	public int ca_range;
	public int cl_t;
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
			string n, 
			int e1_range, int e1_t, double e1_fmax, double e1_rfd_avg, double e1_i,
			int ca_range, int cl_t, double cl_rfd_avg, double cl_i, 
			double cl_f_avg, double cl_vf, double cl_f_max, 
			double cl_s_avg, double cl_s_max, double cl_p_avg, double cl_p_max
			)
	{
		this.n = n;
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
		//cell[0] is not converted because is string
		for(int i = 1 ; i < cells.Length ;  i ++)
			cells[i] = Util.TrimDecimals(Convert.ToDouble(Util.ChangeDecimalSeparator(cells[i])),3);
	
		this.n 		= cells[0];
		this.e1_range 	= Convert.ToInt32(cells[1]); 
		this.e1_t 	= Convert.ToInt32(cells[2]);
		this.e1_fmax 	= Convert.ToDouble(cells[3]);
		this.e1_rfd_avg	= Convert.ToDouble(cells[4]);
		this.e1_i	= Convert.ToDouble(cells[5]);
		this.ca_range	= Convert.ToInt32(cells[6]);
		this.cl_t 	= Convert.ToInt32(cells[7]);
		this.cl_rfd_avg = Convert.ToDouble(cells[8]);
		this.cl_i 	= Convert.ToDouble(cells[9]);
		this.cl_f_avg 	= Convert.ToDouble(cells[10]);
		this.cl_vf 	= Convert.ToDouble(cells[11]);
		this.cl_f_max 	= Convert.ToDouble(cells[12]);
		this.cl_s_avg 	= Convert.ToDouble(cells[13]);
		this.cl_s_max 	= Convert.ToDouble(cells[14]);
		this.cl_p_avg 	= Convert.ToDouble(cells[15]);
		this.cl_p_max 	= Convert.ToDouble(cells[16]);
	}

	public string ToCSV(string decimalSeparator) {
		//latin:	2,3 ; 2,5
		//non-latin:	2.3 , 2.5

		string sep = ":::";
		string str = 
			n + sep + e1_range.ToString() + sep + 
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
		this.future2 = future2;
		this.future3 = future3;
		this.exerciseName = exerciseName;

		if(eccon == "c")
			ecconLong = Catalog.GetString("Concentric");
		else if(eccon == "ec" || eccon == "ecS")
			ecconLong = Catalog.GetString("Eccentric-concentric");
		else
			ecconLong = Catalog.GetString("Concentric-eccentric");
	}

	//used on encoder table
	public enum Eccons { ALL, ecS, ceS, c } 

	public string GetDate(bool pretty) {
		int pointPos = filename.LastIndexOf('.');
		int dateLength = 19; //YYYY-MM-DD_hh-mm-ss
		string date = filename.Substring(pointPos - dateLength, dateLength);
		if(pretty) {
			string [] dateParts = date.Split(new char[] {'_'});
			date = dateParts[0] + " " + dateParts[1].Replace('-',':');
		}
		return date;
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
	public string [] ToStringArray (int count, bool checkboxes, bool video, bool encoderConfigPretty, bool showMeanPower) {
		int all = 9;
		if(checkboxes)
			all ++;
		if(video)
			all++;
		if(showMeanPower)
			all++;


		string [] str = new String [all];
		int i=0;
		str[i++] = uniqueID;
	
		if(checkboxes)
			str[i++] = "";	//checkboxes
	
		str[i++] = count.ToString();
		str[i++] = exerciseName;
		str[i++] = laterality;
		str[i++] = extraWeight;
		
		if(showMeanPower)
			str[i++] = future1;

		if(encoderConfigPretty)
			str[i++] = encoderConfiguration.ToStringPretty();
		else
			str[i++] = encoderConfiguration.code.ToString();
		
		str[i++] = ecconLong;
		str[i++] = GetDate(true);
		
		if(video) {
			if(videoURL != "")
				str[i++] = Catalog.GetString("Yes");
			else
				str[i++] = Catalog.GetString("No");
		}

		str[i++] = description;
		return str;
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
			newFilename = newPersonID + "-" + newPersonName + "-" + uniqueID + "-" + GetDate(false) + ".txt";
		else 
			newFilename = newPersonID + "-" + newPersonName + "-" + GetDate(false) + ".txt";

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


public class EncoderPersonCurvesInDB
{
	public int personID;
	public int sessionID;
	public string sessionName;
	public string sessionDate;
	public int countActive;
	public int countAll;
	
	public EncoderPersonCurvesInDB() {
	}
	public EncoderPersonCurvesInDB(int personID, int sessionID, string sessionName, string sessionDate,
			int countActive, int countAll) {
		this.personID =		personID;
		this.sessionID = 	sessionID;
		this.sessionName = 	sessionName;
		this.sessionDate = 	sessionDate;
		this.countActive = 	countActive;
		this.countAll =		countAll;
	}

	public string [] ToStringArray() {
		string [] s = { sessionID.ToString(), "", sessionName, sessionDate,
			countActive.ToString(), countAll.ToString()
		};
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

	public EncoderExercise() {
	}

	public EncoderExercise(string name) {
		this.name = name;
	}

	public EncoderExercise(int uniqueID, string name, int percentBodyWeight, 
			string ressistance, string description, double speed1RM)
	{
		this.uniqueID = uniqueID;
		this.name = name;
		this.percentBodyWeight = percentBodyWeight;
		this.ressistance = ressistance;
		this.description = description;
		this.speed1RM = speed1RM;
	}

	public bool IsPredefined() {
		if(
				name == "Bench press" ||
				name == "Squat" ||
				name == "Free" ||
				name == "Jump" ||
				name == "Inclinated plane" ||
				name == "Inclinated plane BW" )
			return true;
		else 
			return false;
	}

	~EncoderExercise() {}
}

public class Encoder1RM
{
	public int uniqueID;
	public int personID;
	public int sessionID;
	public int exerciseID;
	public double load1RM;
	
	public string personName;
	public string exerciseName;
	
	public Encoder1RM() {
	}

	public Encoder1RM(int uniqueID, int personID, int sessionID, int exerciseID, double load1RM)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.sessionID = sessionID;
		this.exerciseID = exerciseID;
		this.load1RM = load1RM;
	}

	public Encoder1RM(int uniqueID, int personID, int sessionID, int exerciseID, double load1RM, string personName, string exerciseName)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.sessionID = sessionID;
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
		string [] s = { uniqueID.ToString(), personName, exerciseName, load1RM.ToString() };
		return s;
	}


	~Encoder1RM() {}
}

public class EncoderCaptureCurve {
	public bool up;
	public int startFrame;
        public int endFrame;

	public EncoderCaptureCurve(bool up, int startFrame, int endFrame)
	{
		this.up = up;
		this.startFrame = startFrame;
		this.endFrame = endFrame;
	}

	public string DirectionAsString() {
		if(up)
			return "UP";
		else
			return "DOWN";
	}

	~EncoderCaptureCurve() {}
}

public class EncoderCaptureCurveArray {
	public ArrayList ecc;	//each of the EncoderCaptureCurve
	public int curvesAccepted; //starts at int 0. How many ecc have been accepted (will be rows in treeview_encoder_capture_curves)
	
	public EncoderCaptureCurveArray() {
		ecc = new ArrayList();
		curvesAccepted = 0;
	}
	
	~EncoderCaptureCurveArray() {}
}

public class EncoderBarsData {
	public double MeanSpeed;
	public double MaxSpeed;
	public double MeanForce;
	public double MaxForce;
	public double MeanPower;
	public double PeakPower;
	
	public EncoderBarsData(double meanSpeed, double maxSpeed, double meanForce, double maxForce, double meanPower, double peakPower) {
		this.MeanSpeed = meanSpeed;
		this.MaxSpeed  = maxSpeed;
		this.MeanForce = meanForce;
		this.MaxForce  = maxForce;
		this.MeanPower = meanPower;
		this.PeakPower = peakPower;
	}

	public double GetValue (string option) {
		if(option == Constants.MeanSpeed)
			return MeanSpeed;
		else if(option == Constants.MaxSpeed)
			return MaxSpeed;
		else if(option == Constants.MeanForce)
			return MeanForce;
		else if(option == Constants.MaxForce)
			return MaxForce;
		else if(option == Constants.MeanPower)
			return MeanPower;
		else // option == Constants.PeakPower
			return PeakPower;
	}
	
	~EncoderBarsData() {}
}

public class EncoderConfiguration {
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
	public bool rotaryFrictionOnAxis;
	public double d;	//axis
	public double D;	//external disc or pulley
	public int anglePush;
	public int angleWeight;
	
	public int inertiaMachine; //this is the inertia without the disc
	
	public int gearedDown;	//demultiplication
	
	public int inertiaTotal; //this is the inertia used by R
	public int extraWeightN; //how much extra weights (inertia)
	public int extraWeightGrams; //weight of each extra weight (inertia)
	public double extraWeightLength; //length from center to center (cm) (inertia)


	public string textDefault = Catalog.GetString("Linear encoder attached to a barbell.") + "\n" + 
		Catalog.GetString("Also common gym tests like jumps or chin-ups.");

	//this is the default values
	public EncoderConfiguration() {
		name = Constants.EncoderConfigurationNames.LINEAR;
		type = Constants.EncoderType.LINEAR;
		position = 0;
		image = Constants.FileNameEncoderLinearFreeWeight;
		code = Constants.DefaultEncoderConfigurationCode;
		text = textDefault;
		has_d = false;
		has_D = false;
		has_angle_push = false;
		has_angle_weight = false;
		has_inertia = false;
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
	}

	// note: if this changes, change also in:
	// UtilEncoder.EncoderConfigurationList(enum encoderType)
	
	public EncoderConfiguration(Constants.EncoderConfigurationNames name) {
		this.name = name;
		has_d = false;
		has_D = false;
		has_angle_push = false;
		has_angle_weight = false;
		has_inertia = false;
		rotaryFrictionOnAxis = false;
		gearedDown = 1;

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
			code = "Linear - inclinated plane";
			text = Catalog.GetString("Linear encoder on a inclinated plane.");
			
			has_angle_push = true;
			has_angle_weight = false;
		}
		else if(name == Constants.EncoderConfigurationNames.LINEARONPLANEWEIGHTDIFFANGLE) {
			type = Constants.EncoderType.LINEAR;
			position = 8;
			image = Constants.FileNameEncoderLinearOnPlaneWeightDiffAngle;
			code = "Linear - inclinated plane different angle";
			text = Catalog.GetString("Linear encoder on a inclinated plane moving a weight in different angle.");
			
			has_angle_push = true;
			has_angle_weight = true;
		}
		// ---- inertial
		else if(name == Constants.EncoderConfigurationNames.LINEARINERTIAL) {
			type = Constants.EncoderType.LINEAR;
			position = 0;
			image = Constants.FileNameEncoderLinearInertial;
			code = "Linear - inertial machine";
			text = Catalog.GetString("Linear encoder on inertia machine.") + "\n" + 
				Catalog.GetString("Configuration NOT Recommended! Please use a rotary encoder.") + "\n" +
				"*" + Catalog.GetString("Person has to start fully extended (on the toes).") + "*" + "\n" + 
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
				"*" + Catalog.GetString("Person has to start fully extended (on the toes).") + "*" + "\n" +
				Catalog.GetString("On inertial machines, 'd' means the average diameter where the pull-push string is rolled");

			has_d = true;
			has_D = true;
			has_inertia = true;
		}
		else if(name == Constants.EncoderConfigurationNames.ROTARYFRICTIONSIDEINERTIALLATERAL) {
			type = Constants.EncoderType.ROTARYFRICTION;
			position = 1;
			image = Constants.FileNameEncoderFrictionSideInertialLateral;
			code = "Rotary friction - inertial machine side - lateral movement";
			text = Catalog.GetString("Rotary friction encoder on inertial machine when person is moving laterally.") + "\n" +
				"*" + Catalog.GetString("Start capture with the string completely unwrapped.") + "*" + "\n" +
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
				"*" + Catalog.GetString("Person has to start fully extended (on the toes).") + "*" + "\n" +
				Catalog.GetString("On inertial machines, 'd' means the average diameter where the pull-push string is rolled") + "\n" + 
				Catalog.GetString("Inertial machine rolls twice faster than body."); 

			has_d = true;
			has_D = true;
			has_inertia = true;
			
			gearedDown = -2; //gearedDown is not used in inertial machines. It's hardcoded
		}

		// ---- rotary friction on axis
		else if(name == Constants.EncoderConfigurationNames.ROTARYFRICTIONAXISINERTIAL) {
			type = Constants.EncoderType.ROTARYFRICTION;
			position = 0;
			image = Constants.FileNameEncoderFrictionAxisInertial;
			code = "Rotary friction axis - inertial machine axis";
			text = Catalog.GetString("Rotary friction encoder on inertial machine axis.") + "\n" +
				"*" + Catalog.GetString("Person has to start fully extended (on the toes).") + "*" + "\n" + 
				Catalog.GetString("On inertial machines, 'd' means the average diameter where the pull-push string is rolled");

			has_d = true;
			has_inertia = true;
			rotaryFrictionOnAxis = true;
		}
		else if(name == Constants.EncoderConfigurationNames.ROTARYFRICTIONAXISINERTIALLATERAL) {
			type = Constants.EncoderType.ROTARYFRICTION;
			position = 1;
			image = Constants.FileNameEncoderFrictionAxisInertialLateral;
			code = "Rotary friction - inertial machine axis - lateral movement";
			text = Catalog.GetString("Rotary friction encoder on inertial machine when person is moving laterally.") + "\n" +
				"*" + Catalog.GetString("Start capture with the string completely unwrapped.") + "*" + "\n" +
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
				"*" + Catalog.GetString("Person has to start fully extended (on the toes).") + "*" + "\n" +
				Catalog.GetString("On inertial machines, 'd' means the average diameter where the pull-push string is rolled") + "\n" + 
				Catalog.GetString("Inertial machine rolls twice faster than body."); 

			has_d = true;
			has_inertia = true;
			rotaryFrictionOnAxis = true;
			
			gearedDown = -2; //gearedDown is not used in inertial machines. It's hardcoded
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
				"*" + Catalog.GetString("Person has to start fully extended (on the toes).") + "*" + "\n" +
				Catalog.GetString("On inertial machines, 'd' means the average diameter where the pull-push string is rolled");

			has_d = true;
			has_inertia = true;
		}
		else if(name == Constants.EncoderConfigurationNames.ROTARYAXISINERTIALLATERAL) {
			type = Constants.EncoderType.ROTARYAXIS;
			position = 1;
			image = Constants.FileNameEncoderAxisInertialLateral;
			code = "Rotary axis - inertial machine - lateral movement";
			text = Catalog.GetString("Rotary axis encoder on inertial machine when person is moving laterally.") + "\n" +
				"*" + Catalog.GetString("Start capture with the string completely unwrapped.") + "*" + "\n" +
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
				"*" + Catalog.GetString("Person has to start fully extended (on the toes).") + "*" + "\n" +
				Catalog.GetString("On inertial machines, 'd' means the average diameter where the pull-push string is rolled") + "\n" + 
				Catalog.GetString("Inertial machine rolls twice faster than body."); 

			has_d = true;
			has_inertia = true;
			
			gearedDown = -2; //gearedDown is not used in inertial machines. It's hardcoded
		}
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
	
		//this params started at 1.5.1
		if(strFull.Length > 7) {
			this.inertiaTotal = 	Convert.ToInt32(strFull[7]);
			this.extraWeightN = 	Convert.ToInt32(strFull[8]);
			this.extraWeightGrams = Convert.ToInt32(strFull[9]);
			this.extraWeightLength = Convert.ToDouble(Util.ChangeDecimalSeparator(strFull[10]));
		} else {
			this.inertiaTotal = 	inertiaMachine;
			this.extraWeightN = 	0;
			this.extraWeightGrams = 0;
			this.extraWeightLength = 1;
		}
	}

	//called on capture, recalculate, load
	public void SQLUpdate()
	{
		SqlitePreferences.Update("encoderConfiguration", this.ToStringOutput(Outputs.SQL), false);
	}

	public enum Outputs { ROPTIONS, RCSV, SQL} 
	
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
		else { //(o == Outputs.SQL) 
			sep = ":";
			return 
				name + sep + 
				str_d + sep + 
				str_D + sep + 
				anglePush.ToString() + sep + 
				angleWeight.ToString() + sep +
				inertiaMachine.ToString() + sep + 
				gearedDown.ToString() + sep + 
				inertiaTotal.ToString() + sep + 
				extraWeightN.ToString() + sep + 
				extraWeightGrams.ToString() + sep +
				extraWeightLength.ToString()
				;
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
}
