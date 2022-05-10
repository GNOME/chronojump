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
using System.IO; 		//for detect OS //TextWriter
using System.Collections; //ArrayList
using System.Collections.Generic; //List<T>
using System.Threading;
using Mono.Unix;

public class RunEncoder
{
	public enum Devices { MANUAL, RESISTED } //RESISTED will have two columns on the CSV (encoder, forecSensor)
	public static string DevicesStringMANUAL = "Manual race analyzer";
	public static string DevicesStringRESISTED = "Resisted race analyzer";

	private int uniqueID;
	private int personID;
	private int sessionID;
	private int exerciseID; //until runEncoderExercise table is not created, all will be 0
	//private int angle; //unused
	private Devices device;
	private int distance;
	private int temperature;
	private string filename;
	private string url;	//relative
	private string dateTime;
	private string comments;
	private string videoURL;
	private int angle;

	private string exerciseName;

	/* constructors */

	//have a uniqueID -1 contructor, useful when set is deleted
	public RunEncoder()
	{
		uniqueID = -1;
	}

	//constructor
	public RunEncoder(int uniqueID, int personID, int sessionID, int exerciseID, Devices device,
			int distance, int temperature, string filename, string url,
			string dateTime, string comments, string videoURL, int angle, string exerciseName)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.sessionID = sessionID;
		this.exerciseID = exerciseID;
		this.device = device;
		this.distance = distance;
		this.temperature = temperature;
		this.filename = filename;
		this.url = url;
		this.dateTime = dateTime;
		this.comments = comments;
		this.videoURL = videoURL;
		this.angle = angle;

		this.exerciseName = exerciseName;
	}

	/* methods */

	public int InsertSQL(bool dbconOpened)
	{
		return SqliteRunEncoder.Insert(dbconOpened, toSQLInsertString());
	}
	private string toSQLInsertString()
	{
		string uniqueIDStr = "NULL";
		if(uniqueID != -1)
			uniqueIDStr = uniqueID.ToString();

		return
			"(" + uniqueIDStr + ", " + personID + ", " + sessionID + ", " + exerciseID + ", \"" + device.ToString() + "\", " +
			distance + ", " + temperature + ", \"" + filename + "\", \"" + url + "\", \"" + dateTime + "\", \"" +
			comments + "\", \"" + videoURL + "\", " + angle + ")";
	}

	public void UpdateSQL(bool dbconOpened)
	{
		SqliteRunEncoder.Update(dbconOpened, toSQLUpdateString());
	}
	private string toSQLUpdateString()
	{
		return
			" uniqueID = " + uniqueID +
			", personID = " + personID +
			", sessionID = " + sessionID +
			", exerciseID = " + exerciseID +
			", device = \"" + device.ToString() +
			"\", distance = " + distance +
			", temperature = " + temperature +
			", filename = \"" + filename +
			"\", url = \"" + Util.MakeURLrelative(url) +
			"\", dateTime = \"" + dateTime +
			"\", comments = \"" + comments +
			"\", videoURL = \"" + Util.MakeURLrelative(videoURL) +
			"\", angle = " + angle +
			" WHERE uniqueID = " + uniqueID;
	}

	public void UpdateSQLJustComments(bool dbconOpened)
	{
		SqliteRunEncoder.UpdateComments (dbconOpened, uniqueID, comments); //SQL not opened
	}

	public string [] ToStringArray (bool showDevice, int count)
	{
		int all = 7;
		if(showDevice)
			all ++;

		string [] str = new String [all];
		int i=0;
		str[i++] = uniqueID.ToString();
		str[i++] = count.ToString();
		str[i++] = exerciseName;
		if(showDevice)
			str[i++] = Catalog.GetString(GetDeviceString(device));

		str[i++] = distance.ToString();
		str[i++] = dateTime;

		//str[i++] = videoURL;
		if(videoURL != "")
			str[i++] = Catalog.GetString("Yes");
		else
			str[i++] = Catalog.GetString("No");

		str[i++] = comments;

		return str;
	}

	public static string GetDeviceString(Devices d)
	{
		if(d == Devices.RESISTED)
			return DevicesStringRESISTED;
		else
			return DevicesStringMANUAL;
	}

	//uniqueID:name
	public RunEncoder ChangePerson(string newIDAndName)
	{
		int newPersonID = Util.FetchID(newIDAndName);
		string newPersonName = Util.FetchName(newIDAndName);
		string newFilename = filename;

		personID = newPersonID;
		newFilename = newPersonID + "-" + newPersonName + "-" + dateTime + ".csv";

		bool success = false;
		success = Util.FileMove(url, filename, newFilename);
		if(success)
			filename = newFilename;

		//will update SqliteRunEncoder
		return (this);
	}

	public static string GetScript() {
		return System.IO.Path.Combine(UtilEncoder.GetSprintPath(), "sprintEncoder.R");
	}
	public static string GetCSVInputMulti() {
		return Path.Combine(Path.GetTempPath(), "cj_race_analyzer_input_multi.csv");
	}
	//this contains only Pulses;Time(useconds);Force(N)
	public static string GetCSVFileName() {
		return Path.Combine(Path.GetTempPath(), "cj_race_analyzer_data.csv");
	}
	//this contains: "Mass";"Height";"Temperature";"Vw";"Ka";"K.fitted";"Vmax.fitted"; ...
	public static string GetCSVResultsFileName() {
		return "sprintResults.csv";
	}
	public static string GetCSVResultsURL() {
		return Path.Combine(Path.GetTempPath(), GetCSVResultsFileName());
	}
	public static string GetTempFileName() {
		return Path.Combine(Path.GetTempPath(), "cj_race_analyzer_graph.png");
	}


	public string FullURL
	{
		get { return Util.GetRunEncoderSessionDir(sessionID) + Path.DirectorySeparatorChar + filename; }
	}
	public string FullVideoURL
	{
		get {
			if(videoURL == "")
				return "";

			return Util.GetVideoFileName(sessionID, Constants.TestTypes.RACEANALYZER, uniqueID);
		}
	}
	public string Filename
	{
		get { return filename; }
	}

	public int UniqueID
	{
		get { return uniqueID; }
		set { uniqueID = value; }
	}
	public int PersonID
	{
		get { return personID; }
	}
	public int ExerciseID
	{
		get { return exerciseID; }
		set { exerciseID = value; }
	}
	public Devices Device
	{
		get { return device; }
		set { device = value; }
	}
	public int Distance
	{
		get { return distance; }
		set { distance = value; }
	}
	public int Temperature
	{
		get { return temperature; }
		set { temperature = value; }
	}
	public string DateTimePublic
	{
		get { return dateTime; }
	}
	public string Comments
	{
		get { return comments; }
		set { comments = value; }
	}
	public string VideoURL
	{
		get { return videoURL; }
		set { videoURL = value; }
	}
	public int Angle
	{
		get { return angle; }
		set { angle = value; }
	}
	public string ExerciseName
	{
		get { return exerciseName; }
		set { exerciseName = value; }
	}
}

//get speed, total distance, ...
public class RunEncoderCaptureGetSpeedAndDisplacement
{
	//to calcule the vertical lines on pos/time, speed/time, accel/time
	//it is calculated once and used on all
	private int segmentCm;
	private List<int> segmentVariableCm; //if segmentCm == -1 then this is used
	private int segmentVariableCmDistAccumulated;
	private RunEncoderSegmentCalcs segmentCalcs;

	private int encoderDisplacement;
	private int time;
	private int force;
	private int encoderOrRCA;

	private int timePre;
	private int timeAtEnoughAccel;

	private double runEncoderCaptureSpeed;
	private double runEncoderCaptureSpeedMax;
	private double runEncoderCaptureDistance; //m

	public RunEncoderCaptureGetSpeedAndDisplacement(int segmentCm, List<int> segmentVariableCm, double massKg, int angle)
	{
		this.segmentCm = segmentCm;
		this.segmentVariableCm = segmentVariableCm;
		segmentVariableCmDistAccumulated = 0;

		segmentCalcs = new RunEncoderSegmentCalcs (massKg, angle);
		timePre = 0;
		timeAtEnoughAccel = 0;
	}

	public void PassCapturedRow (List<int> binaryReaded)
	{
		this.encoderDisplacement = binaryReaded[0];
		this.time = binaryReaded[1];
		this.force = binaryReaded[2];
		this.encoderOrRCA = binaryReaded[3];
	}

	public bool PassLoadedRow (string row)
	{
		string [] cells = row.Split(new char[] {';'});
		if(cells.Length != 3)
			return false;

		if(! Util.IsNumber(cells[0], false) || ! Util.IsNumber(cells[1], false))
			return false;

		this.encoderDisplacement = Convert.ToInt32(cells[0]);

		this.time = Convert.ToInt32(cells[1]);
		//after enoughAccel, time has to be shifted to left
		if(timeAtEnoughAccel > 0)
			time -= timeAtEnoughAccel;

		return true;
	}

	//to sync time
	public void SetTimeAtEnoughAccel (string row)
	{
		string [] cells = row.Split(new char[] {';'});
		if(cells.Length != 3)
			return;

		if(! Util.IsNumber(cells[0], false) || ! Util.IsNumber(cells[1], false))
			return;

		timeAtEnoughAccel = Convert.ToInt32(cells[1]);
	}

	public bool Calcule ()
	{
		bool hasCalculed = false;
		if(time > timePre)
		{
			if(timePre > 0)
			{
				double runEncoderCaptureDistanceAtThisSample = Math.Abs(encoderDisplacement) * 0.0030321; //hardcoded: same as sprintEncoder.R
				runEncoderCaptureSpeed = UtilAll.DivideSafe(runEncoderCaptureDistanceAtThisSample, (time - timePre)) * 1000000;
				if(runEncoderCaptureSpeed > runEncoderCaptureSpeedMax)
					runEncoderCaptureSpeedMax = runEncoderCaptureSpeed;

				runEncoderCaptureDistance += runEncoderCaptureDistanceAtThisSample;

				if(segmentCm > 0)
					updateSegmentDistTimeFixed ();
				else if(segmentVariableCm.Count > 0)
					updateSegmentDistTimeVariable ();

				hasCalculed = true;
			}
			timePre = time;
		}
		return hasCalculed;
	}
	private void updateSegmentDistTimeFixed () //m
	{
		if(runEncoderCaptureDistance >= (segmentCm/100.0) * (segmentCalcs.Count +1))
			segmentCalcs.Add((segmentCm/100.0) * (segmentCalcs.Count +1), time, runEncoderCaptureSpeed);
		//note this is not very precise because time can be a bit later than the selected dist
	}
	private void updateSegmentDistTimeVariable () //cm
	{
		//care of overflow
		if(segmentCalcs.Count >= segmentVariableCm.Count)
			return;

		double distToBeat = (segmentVariableCm[segmentCalcs.Count] + segmentVariableCmDistAccumulated) / 100.0; //cm -> m
		if(runEncoderCaptureDistance >= distToBeat)
		{
			segmentVariableCmDistAccumulated += segmentVariableCm[segmentCalcs.Count];
			segmentCalcs.Add (distToBeat, time, runEncoderCaptureSpeed);
		}
	}

	public int EncoderDisplacement {
		get { return encoderDisplacement; }
	}
	public int Time { //microseconds
		get { return time; }
	}
	public int Force {
		get { return force; }
	}
	public int EncoderOrRCA {
		get { return encoderOrRCA; }
	}

	public double RunEncoderCaptureSpeed {
		get { return runEncoderCaptureSpeed; }
		set { runEncoderCaptureSpeed = value; }
	}
	public double RunEncoderCaptureSpeedMax {
		get { return runEncoderCaptureSpeedMax; }
	}
	public double RunEncoderCaptureDistance {
		get { return runEncoderCaptureDistance; }
	}
	public RunEncoderSegmentCalcs SegmentCalcs {
		get { return segmentCalcs; }
	}
}

/*
   first we had TwoListsOfDoubles SegmentDistTime_2l
   but as we want to implement (accel, F, P) avg on each track, we use this new class
   */
public class RunEncoderSegmentCalcs
{
	private const double g = 9.81;

	//passed variables
	private double massKg;
	private int angle;

	//calculated list of vars
	private List<double> dist_l;
	private List<double> time_l;
	private List<double> speedCont_l;
	private List<double> accel_l;
	private List<double> force_l;
	private List<double> power_l;
	/*
	accel = (V2 - V1)/(T2 - T1)
	F = m * (a + g*sin(alpha))
	P = 0.5 * m * ((V2^2 - V1^2) + m*g*(h2 - h1)) / (T2 - T1)
	h = pos * sin(alpha)
	*/

	public RunEncoderSegmentCalcs ()
	{
	}

	public RunEncoderSegmentCalcs (double massKg, int angle)
	{
		this.massKg = massKg;
		this.angle = angle;

		dist_l = new List<double> ();
		time_l = new List<double> ();
		speedCont_l = new List<double> ();
		accel_l = new List<double> ();
		force_l = new List<double> ();
		power_l = new List<double> ();
	}

	//speedCont is continuous (at this instant) (no avg: dist/time of the segment)
	public void Add (double dist, double time, double speedCont)
	{
		//store this variable before adding the dist
		bool isFirstOne = (dist_l.Count == 0);

		dist_l.Add (dist);
		time_l.Add (time);
		speedCont_l.Add(speedCont);

		if(isFirstOne)
		{
			double accel = UtilAll.DivideSafe(speedCont, time/1000000.0);
			accel_l.Add (accel);
			force_l.Add ( massKg * (accel + g * Math.Sin(angle)) );
			power_l.Add (UtilAll.DivideSafe(( 0.5 * massKg * Math.Pow(speedCont, 2) + massKg * g * (dist * Math.Sin(angle)) ) , time/1000000.0));
		}
		else
		{
			/*
			debug accel:
			LogB.Information(string.Format("speed now: {0}, speed pre: {1}, time now: {2}, time pre: {3}, result: {4}",
						speedCont, speedCont_l[Count -2], time/1000000.0, time_l[Count -2]/1000000.0,
						UtilAll.DivideSafe( (speedCont - speedCont_l[Count -2]), (time/1000000.0 - time_l[Count -2]/1000000.0) ) ));
						*/

			double accel = UtilAll.DivideSafe(
					(speedCont - speedCont_l[Count -2]), (time/1000000.0 - time_l[Count -2]/1000000.0) );
			accel_l.Add (accel);
			force_l.Add ( massKg * (accel + g * Math.Sin(angle)) );
			power_l.Add ( UtilAll.DivideSafe (
						0.5 * massKg * (Math.Pow(speedCont, 2) - Math.Pow(speedCont_l[Count -2], 2)) + //Kinetic Energy +
						massKg * g * (dist * Math.Sin(angle) - dist_l[Count -2] * Math.Sin(angle)), //Potential Energy
						(time - time_l[Count -2]) / 1000000.0 //Time
						) );
		}
	}

	public int Count
	{
		get { return dist_l.Count; }
	}

	public List<double> Dist_l {
		get { return dist_l; }
	}
	public List<double> Time_l {
		get { return time_l; }
	}
	//to debug
	public List<double> SpeedCont_l {
		get { return speedCont_l; }
	}
	public List<double> Accel_l {
		get { return accel_l; }
	}
	public List<double> Force_l {
		get { return force_l; }
	}
	public List<double> Power_l {
		get { return power_l; }
	}
}

public class RunEncoderExercise
{
	private int uniqueID;
	private string name;
	private string description;
	private int segmentCm;
	public static int SegmentCmDefault = 500;
	private List<int> segmentVariableCm; //if segmentCm == -1 then this is used
	private bool isSprint;
	private int angleDefault;

	public RunEncoderExercise()
	{
	}

	public RunEncoderExercise(string name)
	{
		this.name = name;
	}

	public RunEncoderExercise(int uniqueID, string name, string description,
			int segmentCm, List<int> segmentVariableCm, bool isSprint, int angleDefault)
	{
		this.uniqueID = uniqueID;
		this.name = name;
		this.description = description;
		this.segmentCm = segmentCm;
		this.segmentVariableCm = segmentVariableCm;
		this.isSprint = isSprint;
		this.angleDefault = angleDefault;
	}

	public override string ToString()
	{
		return string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}",
				uniqueID, name, description, segmentCm,
				Util.ListIntToSQLString (segmentVariableCm, ";"),
				isSprint, angleDefault);
	}

	public void InsertSQL (bool dbconOpened)
	{
		SqliteRunEncoderExercise.Insert(dbconOpened, toSQLInsertString());
	}

	private string toSQLInsertString()
	{
		string uniqueIDStr = "NULL";
		if(uniqueID != -1)
			uniqueIDStr = uniqueID.ToString();

		return
			"(" + uniqueIDStr + ", \"" + name + "\", \"" + description + "\", " +
			segmentCm + ", \"" + SegmentVariableCmToSQL + "\", " +
			Util.BoolToInt(isSprint) + ", " + angleDefault + ")";
	}

	public int UniqueID
	{
		get { return uniqueID; }
	}
	public string Name
	{
		get { return name; }
	}
	public string Description
	{
		get { return description; }
	}
	public int SegmentCm
	{
		get { return segmentCm; }
		set { segmentCm = value; }
	}
	public List<int> SegmentVariableCm
	{
		get { return segmentVariableCm; }
	}
	public string SegmentVariableCmToSQL
	{
		get { return Util.ListIntToSQLString (segmentVariableCm, ";"); }
	}
	//same as above but return -1 if empty
	public string SegmentVariableCmToR (string sep)
	{
		string str = Util.ListIntToSQLString (segmentVariableCm, sep);
		if(str == "")
			str = "-1";
		return str;
	}
	public bool IsSprint
	{
		get { return isSprint; }
	}
	public int AngleDefault
	{
		get { return angleDefault; }
	}
}

//results coming from analyze (load) using R. To be published on exportable table
public class RunEncoderCSV
{
	public double Mass;
	public double Height;
	public int Temperature;
	public double Vw;
	public double Ka;
	public double K_fitted;
	public double Vmax_fitted;
	public double Amax_fitted;
	public double Fmax_fitted;
	public double Fmax_rel_fitted;
	public double Sfv_fitted;
	public double Sfv_rel_fitted;
	public double Sfv_lm;
	public double Sfv_rel_lm;
	public double Pmax_fitted;
	public double Pmax_rel_fitted;
	public double Tpmax_fitted;
	public double F0;
	public double F0_rel;
	public double V0;
	public double Pmax_lm;
	public double Pmax_rel_lm;

	public RunEncoderCSV ()
	{
	}

	public RunEncoderCSV (double mass, double height, int temperature, double vw, double ka, double k_fitted, double vmax_fitted, double amax_fitted, double fmax_fitted, double fmax_rel_fitted, double sfv_fitted, double sfv_rel_fitted, double sfv_lm, double sfv_rel_lm, double pmax_fitted, double pmax_rel_fitted, double tpmax_fitted, double f0, double f0_rel, double v0, double pmax_lm, double pmax_rel_lm)
	{
		this.Mass = mass;
		this.Height = height;
		this.Temperature = temperature;
		this.Vw = vw;
		this.Ka = ka;
		this.K_fitted = k_fitted;
		this.Vmax_fitted = vmax_fitted;
		this.Amax_fitted = amax_fitted;
		this.Fmax_fitted = fmax_fitted;
		this.Fmax_rel_fitted = fmax_rel_fitted;
		this.Sfv_fitted = sfv_fitted;
		this.Sfv_rel_fitted = sfv_rel_fitted;
		this.Sfv_lm = sfv_lm;
		this.Sfv_rel_lm = sfv_rel_lm;
		this.Pmax_fitted = pmax_fitted;
		this.Pmax_rel_fitted = pmax_rel_fitted;
		this.Tpmax_fitted = tpmax_fitted;
		this.F0 = f0;
		this.F0_rel = f0_rel;
		this.V0 = v0;
		this.Pmax_lm = pmax_lm;
		this.Pmax_rel_lm = pmax_rel_lm;
	}

	public string [] ToTreeView()
	{
		return new string [] {
			Util.TrimDecimals(Mass, 1),
				Util.TrimDecimals(Height, 1),
				Temperature.ToString(),
				Util.TrimDecimals(Vw, 3),
				Util.TrimDecimals(Ka, 3),
				Util.TrimDecimals(K_fitted, 3),
				Util.TrimDecimals(Vmax_fitted, 3),
				Util.TrimDecimals(Amax_fitted, 3),
				Util.TrimDecimals(Fmax_fitted, 3),
				Util.TrimDecimals(Fmax_rel_fitted, 3),
				Util.TrimDecimals(Sfv_fitted, 3),
				Util.TrimDecimals(Sfv_rel_fitted, 3),
				Util.TrimDecimals(Sfv_lm, 3),
				Util.TrimDecimals(Sfv_rel_lm, 3),
				Util.TrimDecimals(Pmax_fitted, 3),
				Util.TrimDecimals(Pmax_rel_fitted, 3),
				Util.TrimDecimals(Tpmax_fitted, 3),
				Util.TrimDecimals(F0, 3),
				Util.TrimDecimals(F0_rel, 3),
				Util.TrimDecimals(V0, 3),
				Util.TrimDecimals(Pmax_lm, 3),
				Util.TrimDecimals(Pmax_rel_lm, 3)
		};
	}

	public string ToCSV(string decimalSeparator)
	{
		//latin:	2,3 ; 2,5
		//non-latin:	2.3 , 2.5

		string sep = ":::";
		string str = Util.StringArrayToString(ToTreeView(), sep);

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

//this class creates the rows of each set for the csv input multi that is read by R
public class RunEncoderGraphExport
{
	private bool isWindows;
	private string fullURL;
	private double mass;
	private double personHeight;
	private RunEncoder.Devices device;
	private double tempC;
	private int testLength;
	private RunEncoderExercise exercise;
	private string title;
	private string datetime;
	private TriggerList triggerList;
	private string comments;

	public RunEncoderGraphExport(
			bool isWindows,
			string fullURL,
			double mass, double personHeight,
			RunEncoder.Devices device,
			double tempC, int testLength,
			RunEncoderExercise exercise, string title, string datetime,
			TriggerList triggerList,
			string comments)
	{
		this.isWindows = isWindows;
		this.fullURL = fullURL; //filename
		this.mass = mass;
		this.personHeight = personHeight;
		this.device = device;
		this.tempC = tempC;
		this.testLength = testLength;
		this.exercise = exercise;
		this.title = title;
		this.datetime = datetime;
		this.comments = comments;
		this.triggerList = triggerList;
	}

	public string ToCSVRowOnExport()
	{
		string url = fullURL;
		if(isWindows)
			url = url.Replace("\\","/");

		string segmentM = "-1"; //variable segments
		string segmentVariableCm = "-1"; //fixed segments
		if(exercise.SegmentCm > 0)
			segmentM = Util.ConvertToPoint(exercise.SegmentCm / 100.0); //fixed segment
		else
		{
			//it is a row separated by ; so we need , here
			segmentVariableCm = exercise.SegmentVariableCmToR(",");
		}

		return url + ";" +
			Util.ConvertToPoint(mass) + ";" +
			Util.ConvertToPoint(personHeight / 100.0) + ";" + //in meters
			device.ToString() + ";" +
			Util.ConvertToPoint(tempC) + ";" +
			testLength.ToString() + ";" +
			segmentM + ";" + //fixed segments (m)
			segmentVariableCm + ";" + //variable segments (cm)
			title + ";" +
			datetime + ";" +
			printTriggers(TriggerList.Type3.ON) + ";" +
			printTriggers(TriggerList.Type3.OFF) + ";" +
			Util.RemoveNewLine(Util.RemoveChar(comments, ';'), true);
	}

	private string printTriggers(TriggerList.Type3 type3)
	{
		return triggerList.ToRCurvesString(type3, ','); //because we pass a csv separated by ;
	}

	public static string PrintCSVHeaderOnExport()
	{
		return "fullURL;mass;personHeight;device;tempC;testLength;" +
			"splitLength;" + //segmentCm on C#, splitLength on R
			"splitVariableCm;" +
			"title;datetime;triggersOn;triggersOff;comments";
	}

	public RunEncoderExercise Exercise
	{
		get { return exercise; }
	}
}

public class RunEncoderGraph
{
	private int testLength;
	private double mass;
	private double personHeight;
	private double tempC;
	private RunEncoder.Devices device;
	private RunEncoderExercise rex;
	private string title;
	private string datetime;
	private double startAccel;
	private bool plotRawAccel;
	private bool plotFittedAccel;
	private bool plotRawForce;
	private bool plotFittedForce;
	private bool plotRawPower;
	private bool plotFittedPower;
	private TriggerList triggerList;
	private char exportDecimalSeparator;
	private bool includeImagesOnExport;
	private bool includeInstantaneousOnExport;

	private void assignGenericParams(
			int testLength, double mass, double personHeight, double tempC, RunEncoder.Devices device,
			RunEncoderExercise rex,
			string title, string datetime, double startAccel,
			bool plotRawAccel, bool plotFittedAccel,
			bool plotRawForce, bool plotFittedForce,
			bool plotRawPower, bool plotFittedPower,
			TriggerList triggerList)
	{
		this.testLength = testLength;
		this.mass = mass;
		this.personHeight = personHeight;
		this.tempC = tempC;
		this.device = device;
		this.rex = rex;
		this.title = title;
		this.datetime = datetime;
		this.startAccel = startAccel;
		this.plotRawAccel = plotRawAccel;
		this.plotFittedAccel = plotFittedAccel;
		this.plotRawForce = plotRawForce;
		this.plotFittedForce = plotFittedForce;
		this.plotRawPower = plotRawPower;
		this.plotFittedPower = plotFittedPower;
		this.triggerList = triggerList;
	}

	//constructor for 1 set
	public RunEncoderGraph(
			int testLength, double mass, double personHeight, double tempC, RunEncoder.Devices device,
			RunEncoderExercise rex,
			string title, string datetime, double startAccel,
			bool plotRawAccel, bool plotFittedAccel,
			bool plotRawForce, bool plotFittedForce,
			bool plotRawPower, bool plotFittedPower,
			TriggerList triggerList)
	{
		assignGenericParams(
				testLength, mass, personHeight, tempC, device,
				rex,
				title, datetime, startAccel,
				plotRawAccel, plotFittedAccel,
				plotRawForce, plotFittedForce,
				plotRawPower, plotFittedPower,
				triggerList);

		this.exportDecimalSeparator = '.';
		this.includeImagesOnExport = false;
		this.includeInstantaneousOnExport = false;
	}

	//constructor for export (many sets of possible different persons)
	public RunEncoderGraph(
			double startAccel,
			bool plotRawAccel, bool plotFittedAccel,
			bool plotRawForce, bool plotFittedForce,
			bool plotRawPower, bool plotFittedPower,
			List<RunEncoderGraphExport> rege_l,
			char exportDecimalSeparator,
			bool includeImagesOnExport,
			bool includeInstantaneousOnExport
			)
	{
		assignGenericParams(
				0, 0, 0, 0, RunEncoder.Devices.MANUAL, //TODO do not pass to assignParams
				new RunEncoderExercise(), //TODO do not pass to assignParams
				"----", "----", startAccel, //TODO do not pass to assignParams
				plotRawAccel, plotFittedAccel,
				plotRawForce, plotFittedForce,
				plotRawPower, plotFittedPower,
				new TriggerList() //TODO do not pass to assignParams
				);

		this.exportDecimalSeparator = exportDecimalSeparator;
		this.includeImagesOnExport = includeImagesOnExport;
		this.includeInstantaneousOnExport = includeInstantaneousOnExport;

		writeMultipleFilesCSV(rege_l);
	}

	public bool CallR(int graphWidth, int graphHeight, bool singleOrMultiple)
	{
		LogB.Information("\nrunEncoder CallR ----->");
		writeOptionsFile(graphWidth, graphHeight, singleOrMultiple);
		return ExecuteProcess.CallR(RunEncoder.GetScript());
	}

	private void writeOptionsFile(int graphWidth, int graphHeight, bool singleOrMultiple)
	{
		string scriptsPath = UtilEncoder.GetSprintPath();
		if(UtilAll.IsWindows())
			scriptsPath = scriptsPath.Replace("\\","/");

		System.Globalization.NumberFormatInfo localeInfo = new System.Globalization.NumberFormatInfo();
		localeInfo = System.Globalization.NumberFormatInfo.CurrentInfo;

		string segmentM = "-1"; //variable segments
		string segmentVariableCm = "-1"; //fixed segments
		if(singleOrMultiple) //only on single
		{
			if(rex.SegmentCm > 0)
				segmentM = Util.ConvertToPoint(rex.SegmentCm / 100.0); //fixed segment
			else
				segmentVariableCm = rex.SegmentVariableCmToR(";");
		}

		string scriptOptions =
			"#scriptsPath\n" + 		UtilEncoder.GetScriptsPath() + "\n" +
			"#filename\n" + 		RunEncoder.GetCSVFileName() + "\n" +	//unused on multiple
			"#mass\n" + 			Util.ConvertToPoint(mass) + "\n" +	//unused on multiple
			"#personHeight\n" + 		Util.ConvertToPoint(personHeight / 100.0) + "\n" + 	//unused on multiple  //send it in meters
			"#tempC\n" + 			tempC + "\n" +	//unused on multiple
			"#testLength\n" + 		testLength.ToString() + "\n" +	//unused on multiple
			"#os\n" + 			UtilEncoder.OperatingSystemForRGraphs() + "\n" +
			"#graphWidth\n" + 		graphWidth.ToString() + "\n" +
			"#graphHeight\n" + 		graphHeight.ToString() + "\n" +
			"#device\n" + 			device.ToString() + "\n" + //unused on multiple
			"#segmentM\n" + 		segmentM + "\n" + 		//unused on multiple
			"#segmentVariableCm\n" + 	segmentVariableCm + "\n" + 		//unused on multiple
			"#title\n" + 			title + "\n" + 		//unused on multiple
			"#datetime\n" + 		datetime + "\n" + 	//unused on multiple
			"#startAccel\n" + 		Util.ConvertToPoint(startAccel) + "\n" +
			"#plotRawAccel\n" + 		Util.BoolToRBool(plotRawAccel) + "\n" +
			"#plotFittedAccel\n" + 		Util.BoolToRBool(plotFittedAccel) + "\n" +
			"#plotRawForce\n" + 		Util.BoolToRBool(plotRawForce) + "\n" +
			"#plotFittedForce\n" + 		Util.BoolToRBool(plotFittedForce) + "\n" +
			"#plotRawPower\n" + 		Util.BoolToRBool(plotRawPower) + "\n" +
			"#plotFittedPower\n" + 		Util.BoolToRBool(plotFittedPower) + "\n" +
			printTriggers(TriggerList.Type3.ON) + "\n" +		//unused on multiple
			printTriggers(TriggerList.Type3.OFF) + "\n" +		//unused on multiple
			"#singleOrMultiple\n" +		Util.BoolToRBool(singleOrMultiple) + "\n" +
			"#decimalCharAtExport\n" +	exportDecimalSeparator + "\n" +
			"#includeImagesOnExport\n" + 	Util.BoolToRBool(includeImagesOnExport) + "\n" +
			"#includeInstantaneousOnExport\n" + 	Util.BoolToRBool(includeInstantaneousOnExport) + "\n";


		TextWriter writer = File.CreateText(Path.GetTempPath() + "Roptions.txt");
		writer.Write(scriptOptions);
		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();
	}

	private void writeMultipleFilesCSV(List<RunEncoderGraphExport> rege_l)
	{
		LogB.Information("writeMultipleFilesCSV start");
		TextWriter writer = File.CreateText(RunEncoder.GetCSVInputMulti());

		//write header
		writer.WriteLine(RunEncoderGraphExport.PrintCSVHeaderOnExport());

		//write fsgAB_l for
		foreach(RunEncoderGraphExport rege in rege_l)
			writer.WriteLine(rege.ToCSVRowOnExport());

		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();
		LogB.Information("writeMultipleFilesCSV end");
	}

	private string printTriggers(TriggerList.Type3 type3)
	{
		return triggerList.ToRCurvesString(type3, ';');
	}

	public static string GetDataDir(int sessionID)
	{
		System.IO.DirectoryInfo folderSession =
			new System.IO.DirectoryInfo(Util.GetRunEncoderSessionDir(sessionID));
		System.IO.DirectoryInfo folderGeneric =
			new System.IO.DirectoryInfo(Util.GetRunEncoderDir());

		if(folderSession.Exists)
			return Util.GetRunEncoderSessionDir(sessionID);
		else if(folderGeneric.Exists)
			return Util.GetRunEncoderDir();
		else
			return "";
	}
}

public class RunEncoderLoadTryToAssignPersonAndComment
{
	private bool dbconOpened;
	private string filename; //filename comes without extension
	private int currentSessionID; //we get a person if already exists on that session

	public string Comment;

	public RunEncoderLoadTryToAssignPersonAndComment(bool dbconOpened, string filename, int currentSessionID)
	{
		this.dbconOpened = dbconOpened;
		this.filename = filename;
		this.currentSessionID = currentSessionID;

		Comment = "";
	}

	public Person GetPerson()
	{
		string personName = getNameAndComment();
		if(personName == "")
			return new Person(-1);

		Person p = SqlitePerson.SelectByName(dbconOpened, personName);
		if(SqlitePersonSession.PersonSelectExistsInSession(dbconOpened, p.UniqueID, currentSessionID))
			return p;

		return new Person(-1);
	}

	private string getNameAndComment()
	{
		string name = "";

		string [] strFull = filename.Split(new char[] {'_'});

		/*
		 * first filename was: personName_date_hour
		 * but we have lots of files with comments added manually like:
		 * first filename was: personName_date_hour_comment
		 * first filename was: personName_date_hour_comment_long_with_underscores
		 */
		if(strFull.Length >= 3)
			name = strFull[0];

		if(strFull.Length == 4) //with one comment
			Comment = strFull[3];
		else if(strFull.Length > 4) //with comments separated by underscores
		{
			string myComment = "";
			string sep = "";
			for(int i = 3; i <= strFull.Length -3; i ++)
			{
				myComment += sep + strFull[i];
				sep = "_";
			}

			Comment = myComment;
		}

		return name;
	}
}
