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
 *  Copyright (C) 2004-2012   Xavier de Blas <xaviblas@gmail.com> 
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
	private int minHeight;
	private int exercisePercentBodyWeight; //was private bool isJump; (if it's 0 is like "jump")
	private string eccon;
	private string analysis;
	private string analysisVariables;
	private string analysisOptions;		//p: propulsive
					
	//encoderMode conversions
	//in signals and curves, need to do conversions (invert, inertiaMomentum, diameter)
	private string encoderMode;	
	private int inertiaMomentum; 
	private double diameter;
	
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
	private bool inverted; //used only in runEncoderCapturePython. In graph.R will be used encoderMode

	public EncoderParams()
	{
	}

	//to encoder capture
	//RunEncoderCapturePython: pyserial_pyper.py and pyserial_pyper_windows.py . This will be deprecated soon 
	public EncoderParams(int time, int minHeight, int exercisePercentBodyWeight, string mass, 
			string smoothCon, string eccon, string analysisOptions,
			double heightHigherCondition, double heightLowerCondition, 
			double meanSpeedHigherCondition, double meanSpeedLowerCondition, 
			double maxSpeedHigherCondition, double maxSpeedLowerCondition, 
			int powerHigherCondition, int powerLowerCondition, 
			int peakPowerHigherCondition, int peakPowerLowerCondition,
			string mainVariable, bool inverted)
	{
		this.time = time;
		this.minHeight = minHeight;
		this.exercisePercentBodyWeight = exercisePercentBodyWeight;
		this.mass = mass;
		this.smoothCon = smoothCon;
		this.eccon = eccon;
		this.analysisOptions = analysisOptions;
		this.heightHigherCondition = heightHigherCondition;
		this.heightLowerCondition = heightLowerCondition;
		this.meanSpeedHigherCondition = meanSpeedHigherCondition;
		this.meanSpeedLowerCondition = meanSpeedLowerCondition;
		this.maxSpeedHigherCondition = maxSpeedHigherCondition;
		this.maxSpeedLowerCondition = maxSpeedLowerCondition;
		this.powerHigherCondition = powerHigherCondition;
		this.powerLowerCondition = powerLowerCondition;
		this.peakPowerHigherCondition = peakPowerHigherCondition;
		this.peakPowerLowerCondition = peakPowerLowerCondition;
		this.mainVariable = mainVariable;
		this.inverted = inverted;
	}

	//to encoder capture
	//RunEncoderCapturePython: pyserial_pyper.py and pyserial_pyper_windows.py . This will be deprecated soon 
	public string ToString1 () 
	{
		string analysisOptionsPrint = analysisOptions;
		if(analysisOptionsPrint == "")
			analysisOptionsPrint = "none";

		return time.ToString() + " " + minHeight.ToString() + " " + exercisePercentBodyWeight.ToString() + 
			" " + mass.ToString() + " " + smoothCon + " " + eccon + " " + analysisOptionsPrint +
			" " + heightHigherCondition.ToString() +	" " + heightLowerCondition.ToString() +
			" " + Util.ConvertToPoint(meanSpeedHigherCondition.ToString()) + 	
			" " + Util.ConvertToPoint(meanSpeedLowerCondition.ToString()) +
			" " + Util.ConvertToPoint(maxSpeedHigherCondition.ToString()) + 	
			" " + Util.ConvertToPoint(maxSpeedLowerCondition.ToString()) +
			" " + powerHigherCondition.ToString() + 	" " + powerLowerCondition.ToString() +
			" " + peakPowerHigherCondition.ToString() + 	" " + peakPowerLowerCondition.ToString() +
			" " + mainVariable + " " + Util.BoolToInt(inverted).ToString();
	}
	
	//to graph.R	
	public EncoderParams(int minHeight, int exercisePercentBodyWeight, string mass, string eccon, 
			string analysis, string analysisVariables, string analysisOptions, 
			string encoderMode, int inertiaMomentum, double diameter,
			string smoothCon, int curve, int width, int height, string decimalSeparator)
	{
		this.minHeight = minHeight;
		this.exercisePercentBodyWeight = exercisePercentBodyWeight;
		this.mass = mass;
		this.eccon = eccon;
		this.analysis = analysis;
		this.analysisVariables = analysisVariables;
		this.analysisOptions = analysisOptions;
		this.encoderMode = encoderMode;
		this.inertiaMomentum = inertiaMomentum;
		this.diameter = diameter;
		this.smoothCon = smoothCon;
		this.curve = curve;
		this.width = width;
		this.height = height;
		this.decimalSeparator = decimalSeparator;
	}
	
	public string ToString2 (string sep) 
	{
		return minHeight + sep + exercisePercentBodyWeight + sep + mass + sep + eccon + 
			sep + analysis + sep + analysisVariables + sep + analysisOptions + 
			sep + encoderMode + sep + inertiaMomentum.ToString() + sep + Util.ConvertToPoint(diameter) +
			sep + smoothCon + sep + curve + sep + width + sep + height + sep + decimalSeparator;
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

//used on TreeView
public class EncoderCurve
{
	public EncoderCurve () {
	}

	//used on TreeView capture
	public EncoderCurve (string n, 
			string start, string duration, string height, 
			string meanSpeed, string maxSpeed, string maxSpeedT,
			string meanPower, string peakPower, string peakPowerT, 
			string PP_PPT)
	{
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
	}

	//used on TreeView analyze
	public EncoderCurve (string n, string series, string exercise, 
			double extraWeight, double displacedWeight,
			string start, string duration, string height,
			string meanSpeed, string maxSpeed, string maxSpeedT,
			string meanPower, string peakPower, string peakPowerT, 
			string PP_PPT)
	{
		this.N = n;
		this.Series = series;
		this.Exercise = exercise;
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
	}

	public string ToCSV() {
		string sep = ";";
		return 
			N + sep + Series + sep + Exercise + sep + 
			ExtraWeight + sep + DisplacedWeight + sep + 
			Start + sep + Duration + sep + Height + sep + 
			MeanSpeed + sep + MaxSpeed + sep + MaxSpeedT + sep + 
			MeanPower + sep + PeakPower + sep + PeakPowerT + sep + 
			PP_PPT;
	}

	public string N;
	public string Series;
	public string Exercise;
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
	public string url;
	public int time;
	public int minHeight;
	public double smooth;	//unused on curves, since 1.3.7 it's in database
	public string description;
	public string status;	//active or inactive curves
	public string videoURL;	//URL of video of signals
	
	//encoderMode conversions
	//in signals and curves, need to do conversions (invert, inertiaMomentum, diameter)
	public string encoderMode;
	public int inertiaMomentum; //kg*cm^2
	public double diameter;
	
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
			string filename, string url, int time, int minHeight, double smooth, 
			string description, string status, string videoURL, 
			string encoderMode, int inertiaMomentum, double diameter,
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
		this.smooth = smooth;
		this.description = description;
		this.status = status;
		this.videoURL = videoURL;
		this.encoderMode = encoderMode;
		this.inertiaMomentum = inertiaMomentum;
		this.diameter = diameter;
		this.future1 = future1;
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

	public string [] ToStringArray (int count, bool checkboxes, bool video) {
		int all = 8;
		if(checkboxes)
			all ++;
		if(video)
			all++;

		string [] str = new String [all];
		int i=0;
		str[i++] = uniqueID;
	
		if(checkboxes)
			str[i++] = "";	//checkboxes
	
		str[i++] = count.ToString();
		str[i++] = exerciseName;
		str[i++] = ecconLong;
		str[i++] = extraWeight;
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
	public void ChangePerson(string newIDAndName) {
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

		//SqliteUpdate
		SqliteEncoder.Update(false, this);
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

	public EncoderExercise(int uniqueID, string name, int percentBodyWeight, string ressistance, string description, double speed1RM)
	{
		this.uniqueID = uniqueID;
		this.name = name;
		this.percentBodyWeight = percentBodyWeight;
		this.ressistance = ressistance;
		this.description = description;
		this.speed1RM = speed1RM;
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
	public int curvesDone;	//starts at int 0. How many ecc have been calculated by R
	public int curvesAccepted; //starts at int 0. How many ecc have been accepeted (will be rows in treeview_encoder_capture_curves)
	
	public EncoderCaptureCurveArray() {
		ecc = new ArrayList();
		curvesDone = 0;
		curvesAccepted = 0;
	}
	
	~EncoderCaptureCurveArray() {}
}

public class EncoderModeSelectionList 
{
	public ArrayList list;

	public EncoderModeSelectionList(string type) 
	{
		list = new ArrayList();
		EncoderModeSelection enc = new EncoderModeSelection();
		
		if(type == "linear") {
			enc = new EncoderModeSelection(
					Constants.EncoderMode.LINEAR.ToString(),
					Constants.FileNameEncoderLinearFreeWeight,
					"Linear encoder attached to a barbell.",
					false,	//d
					false,	//d2
					false,	//angle
					false	//inertia
					); 
			list.Add(enc);

			enc = new EncoderModeSelection(
					Constants.EncoderMode.LINEARINVERTED.ToString(),
					Constants.FileNameEncoderLinearFreeWeightInv,
					"Linear encoder inverted attached to a barbell.",
					false,	//d
					false,	//d2
					false,	//angle
					false	//inertia
					);
			list.Add(enc);
			
			enc = new EncoderModeSelection(
					Constants.EncoderMode.LINEARINERTIAL.ToString(),
					Constants.FileNameEncoderLinearInertial,
					"Linear encoder on inertia machine. NOT Recommended!",
					false,	//d
					false,	//d2
					false,	//angle
					true	//inertia
					);	
			list.Add(enc);

			enc = new EncoderModeSelection(
					Constants.EncoderMode.WEIGHTEDMOVPULLEYLINEARONPERSON1.ToString(),
					Constants.FileNameEncoderWeightedMovPulleyOnPerson1,
					"Linear encoder attached to a barbell. Barbell is connected to a weighted moving pulley.",
					false,	//d
					false,	//d2
					false,	//angle
					false	//inertia
					); 
			list.Add(enc);

			enc = new EncoderModeSelection(
					Constants.EncoderMode.WEIGHTEDMOVPULLEYLINEARONPERSON1INV.ToString(),
					Constants.FileNameEncoderWeightedMovPulleyOnPerson1Inv,
					"Linear encoder inverted attached to a barbell. Barbell is connected to a weighted moving pulley.",
					false,	//d
					false,	//d2
					false,	//angle
					false	//inertia
					); 
			list.Add(enc);

			enc = new EncoderModeSelection(
					Constants.EncoderMode.WEIGHTEDMOVPULLEYLINEARONPERSON2.ToString(),
					Constants.FileNameEncoderWeightedMovPulleyOnPerson2,
					"Linear encoder attached to a barbell. Barbell is connected to a fixed pulley that is connected to a weighted moving pulley.",
					false,	//d
					false,	//d2
					false,	//angle
					false	//inertia
					); 
			list.Add(enc);

			enc = new EncoderModeSelection(
					Constants.EncoderMode.WEIGHTEDMOVPULLEYLINEARONPERSON2INV.ToString(),
					Constants.FileNameEncoderWeightedMovPulleyOnPerson2Inv,
					"Linear encoder inverted attached to a barbell. Barbell is connected to a fixed pulley that is connected to a weighted moving pulley.",
					false,	//d
					false,	//d2
					false,	//angle
					false	//inertia
					); 
			list.Add(enc);

			enc = new EncoderModeSelection(
					Constants.EncoderMode.WEIGHTEDMOVPULLEYONLINEARENCODER.ToString(),
					Constants.FileNameEncoderWeightedMovPulleyOnLinearEncoder,
					"Linear encoder attached to a weighted moving pulley.",
					false,	//d
					false,	//d2
					false,	//angle
					false	//inertia
					); 
			list.Add(enc);

			enc = new EncoderModeSelection(
					Constants.EncoderMode.LINEARONPLANE.ToString(),
					Constants.FileNameEncoderLinearOnPlane,
					"Linear encoder on a inclinated plane.",
					false,	//d
					false,	//d2
					true,	//angle
					false	//inertia
					); 
			list.Add(enc);
		}
		else if(type == "rotary-friction") {
			enc = new EncoderModeSelection(
					Constants.EncoderMode.ROTARYFRICTIONSIDE.ToString(),
					Constants.FileNameEncoderFrictionSide,
					"Rotary friction encoder on pulley.",
					false,	//d
					false,	//d2
					false,	//angle
					false	//inertia
					); 
			list.Add(enc);

			enc = new EncoderModeSelection(
					Constants.EncoderMode.ROTARYFRICTIONAXIS.ToString(),
					Constants.FileNameEncoderFrictionAxis ,
					"Rotary friction encoder on pulley axis.",
					true,	//d
					true,	//d2
					false,	//angle
					false	//inertia
					); 
			list.Add(enc);

			enc = new EncoderModeSelection(
					Constants.EncoderMode.ROTARYFRICTIONINERTIAL.ToString(),
					Constants.FileNameEncoderFrictionInertial ,
					"Rotary friction encoder on inertial machine.",
					false,	//d
					false,	//d2
					false,	//angle
					true	//inertia
					); 
			list.Add(enc);

			enc = new EncoderModeSelection(
					Constants.EncoderMode.WEIGHTEDMOVPULLEYROTARYFRICTION.ToString(),
					Constants.FileNameEncoderFrictionWithMovPulley ,
					"Rotary friction encoder on weighted moving pulley.",
					false,	//d
					false,	//d2
					false,	//angle
					false	//inertia
					); 
			list.Add(enc);

		}
		else {	// rotary-axis
			enc = new EncoderModeSelection(
					Constants.EncoderMode.ROTARYAXIS.ToString(),
					Constants.FileNameEncoderRotaryAxisOnAxis ,
					"Rotary axis encoder on pulley axis.",
					true,	//d
					false,	//d2
					false,	//angle
					false	//inertia
					); 
			list.Add(enc);

			enc = new EncoderModeSelection(
					Constants.EncoderMode.ROTARYAXISINERTIAL.ToString(),
					Constants.FileNameEncoderAxisInertial ,
					"Rotary axis encoder on inertial machine.",
					false,	//d
					false,	//d2
					false,	//angle
					true	//inertia
					); 
			list.Add(enc);

			enc = new EncoderModeSelection(
					Constants.EncoderMode.WEIGHTEDMOVPULLEYROTARYAXIS.ToString(),
					Constants.FileNameEncoderAxisWithMovPulley ,
					"Rotary axis encoder on weighted moving pulley.",
					false,	//d
					false,	//d2
					false,	//angle
					false	//inertia
					); 
			list.Add(enc);
		}
	}
}

public class EncoderModeSelection {
	public string encoderMode;
	public string image;
	public string text;
	public bool d;
	public bool d2;
	public bool angle;
	public bool inertia;
	
	public EncoderModeSelection() {
	}

	public EncoderModeSelection(
			string encoderMode, string image, string text,
			bool d, bool d2, bool angle, bool inertia) {
		this.encoderMode = encoderMode;
		this.image = image;
		this.text = text;
		this.d = d;
		this.d2 = d2;
		this.angle = angle;
		this.inertia = inertia;
	}
}
