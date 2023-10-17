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
 *  Copyright (C) 2017-2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.IO; 		//for detect OS //TextWriter
using System.Collections; //ArrayList
using System.Collections.Generic; //List<T>
using Mono.Unix;

public class ForceSensor
{
	public enum CaptureOptions { NORMAL, ABS, INVERTED }
	public static string CaptureOptionsStringNORMAL()
	{
		return Catalog.GetString("Standard capture");
	}
	public static string CaptureOptionsStringABS()
	{
		return Catalog.GetString("Absolute values");
	}
	public static string CaptureOptionsStringINVERTED()
	{
		return Catalog.GetString("Inverted values");
	}
	public static List<string> CaptureOptionsList()
	{
		List<string> l = new List<string>();

		l.Add(CaptureOptionsStringNORMAL());
		l.Add(CaptureOptionsStringABS());
		l.Add(CaptureOptionsStringINVERTED());

		return l;
	}

	public static int AngleUndefined = -1000;

	private int uniqueID;
	private int personID;
	private int sessionID;
	private int exerciseID;
	private int angle;
	private CaptureOptions captureOption;
	private string laterality;
	private string filename;
	private string url;	//relative
	private string dateTime;
	private string comments;
	private string videoURL;
	private double stiffness; //on not elastic capture will be -1 (just check if it is negative because it's a double and sometimes -1.0 comparisons don't work)
	private string stiffnessString; //id0*active0;id1*active1
	private double maxForceRaw;
	private double maxAvgForce1s;

	private string exerciseName;

	//have a uniqueID -1 contructor, useful when set is deleted
	public ForceSensor()
	{
		uniqueID = -1;
	}

	//constructor
	public ForceSensor(int uniqueID, int personID, int sessionID, int exerciseID, CaptureOptions captureOption, int angle,
			string laterality, string filename, string url, string dateTime, string comments, string videoURL,
			double stiffness, string stiffnessString, double maxForceRaw, double maxAvgForce1s,
			string exerciseName)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.sessionID = sessionID;
		this.exerciseID = exerciseID;
		this.captureOption = captureOption;
		this.angle = angle;
		this.laterality = laterality;
		this.filename = filename;
		this.url = url;
		this.dateTime = dateTime;
		this.comments = comments;
		this.videoURL = videoURL;
		this.stiffness = stiffness;
		this.stiffnessString = stiffnessString;
		this.maxForceRaw = maxForceRaw;
		this.maxAvgForce1s = maxAvgForce1s;

		this.exerciseName = exerciseName;
	}

	public int InsertSQL(bool dbconOpened)
	{
		return SqliteForceSensor.Insert(dbconOpened, toSQLInsertString());
	}
	private string toSQLInsertString()
	{
		string uniqueIDStr = "NULL";
		if(uniqueID != -1)
			uniqueIDStr = uniqueID.ToString();

		LogB.Information("toSQLInsert filename: " + filename);

		return
			"(" + uniqueIDStr + ", " + personID + ", " + sessionID + ", " + exerciseID + ", \"" + captureOption.ToString() + "\", " +
			angle + ", \"" + laterality + "\", \"" + filename + "\", \"" + url + "\", \"" + dateTime + "\", \"" +
			comments + "\", \"" + videoURL + "\", " + Util.ConvertToPoint(stiffness) + ", \"" + stiffnessString + "\", " +
			Util.ConvertToPoint(maxForceRaw) + ", " + Util.ConvertToPoint(maxAvgForce1s) +
			")";
	}

	public void UpdateSQL(bool dbconOpened)
	{
		SqliteForceSensor.Update(dbconOpened, toSQLUpdateString());
	}
	private string toSQLUpdateString()
	{
		return
			" uniqueID = " + uniqueID +
			", personID = " + personID +
			", sessionID = " + sessionID +
			", exerciseID = " + exerciseID +
			", captureOption = \"" + captureOption.ToString() +
			"\", angle = " + angle +
			", laterality = \"" + laterality +
			"\", filename = \"" + filename +
			"\", url = \"" + Util.MakeURLrelative(url) +
			"\", dateTime = \"" + dateTime +
			"\", comments = \"" + comments +
			"\", videoURL = \"" + Util.MakeURLrelative(videoURL) +
			"\", stiffness = " + Util.ConvertToPoint(stiffness) +
			", stiffnessString = \"" + stiffnessString +
			"\", maxForceRaw = " + Util.ConvertToPoint(maxForceRaw) +
			", maxAvgForce1s = " + Util.ConvertToPoint(maxAvgForce1s) +
			" WHERE uniqueID = " + uniqueID;
	}

	public void UpdateSQLJustComments(bool dbconOpened)
	{
		SqliteForceSensor.UpdateComments (dbconOpened, uniqueID, comments); //SQL not opened
	}

	//for load window
	public string [] ToStringArray (int count, Constants.Modes mode)
	{
		int all = 10;
		if (mode == Constants.Modes.FORCESENSORELASTIC)
			all = 11; 	//only show stiffness on elastic

		string [] str = new String [all];
		int i=0;
		str[i++] = uniqueID.ToString();
		str[i++] = count.ToString();
		str[i++] = exerciseName;

		if (mode == Constants.Modes.FORCESENSORELASTIC)
			str[i++] = exerciseElasticStiffnessString();

		str[i++] = Catalog.GetString(GetCaptureOptionsString(captureOption));
		str[i++] = Catalog.GetString(laterality);
		if(maxForceRaw == -1)
			str[i++] = "----";
		else
			str[i++] = Util.TrimDecimals(maxForceRaw, 2);
		if(maxAvgForce1s == -1)
			str[i++] = "----";
		else
			str[i++] = Util.TrimDecimals(maxAvgForce1s, 2);
		str[i++] = dateTime;

		//str[i++] = videoURL;
		if(videoURL != "")
			str[i++] = Catalog.GetString("Yes");
		else
			str[i++] = Catalog.GetString("No");

		str[i++] = comments;

		return str;
	}

	private string exerciseElasticStiffnessString ()
	{
		if(stiffness < 0) //aka == -1.0
			return Catalog.GetString("No");
		else
			//return Catalog.GetString("Yes") + " (" + stiffness + " N/m)";
			return stiffness.ToString();
	}

	//static methods

	//resultant force of a sample. Only used on capture.
	//if this method changes, change also forceSensorDynamics methods
	public static double CalculeForceResultantIfNeeded (double forceRaw, CaptureOptions fsco, ForceSensorExercise fse, double personMass)//, double stiffness)
	{
		if(! fse.ForceResultant)
			return calculeForceWithCaptureOptions(forceRaw, fsco);

		//forceResultant --->

		double totalMass = 0;
		if(fse.PercentBodyWeight > 0 && personMass > 0)
			totalMass = fse.PercentBodyWeight * personMass / 100.0;

		//TODO: right now only code for non-elastic
		//so on elastic we do a load after capture
		//in future show with cairo a graph of raw, and another with resultant and displacement
		double accel = 0;

		/*
		if(fse.Elastic)
		{
			double position = rawForce / stiffness;

		}
		*/

		//not elastic stuff ----->

		/*
		 * debug info
		LogB.Information("--------------");
		LogB.Information("exercise: " + fse.ToString());
		LogB.Information("forceRaw: " + forceRaw.ToString());
		LogB.Information("totalMass: " + totalMass.ToString());
		LogB.Information("AngleDefault: " + fse.AngleDefault.ToString());
		 */
		/*

		LogB.Information("horiz: " + (Math.Cos(fse.AngleDefault * Math.PI / 180.0) * (forceRaw + totalMass * accel)).ToString());
		LogB.Information("vertical: " + (Math.Sin(fse.AngleDefault * Math.PI / 180.0) * (forceRaw + totalMass * accel) + totalMass * 9.81).ToString());
		*/
		//TODO: now we are using fse.AngleDefault, but we have to implement especific angle on capture

		/*
		double forceResultant = Math.Sqrt(
				//Math.Pow(Math.Cos(fse.AngleDefault * Math.PI / 180.0) * (forceRaw + totalMass * accel),2) +                	//Horizontal component
				//Math.Pow(Math.Sin(fse.AngleDefault * Math.PI / 180.0) * (forceRaw + totalMass * accel) + totalMass * 9.81,2) 	//Vertical component
				Math.Pow(Math.Cos(fse.AngleDefault * Math.PI / 180.0) * (Math.Abs(forceRaw) + totalMass * accel),2) +                	//Horizontal component
				Math.Pow(Math.Sin(fse.AngleDefault * Math.PI / 180.0) * (Math.Abs(forceRaw) + totalMass * accel) + totalMass * 9.81,2) 	//Vertical component
				);
        */

		//on 2.2.1 ABS or inverted is not done on forceResultant,
		//is done on force coming from the sensor
		if(fsco != CaptureOptions.NORMAL)
			forceRaw = calculeForceWithCaptureOptions(forceRaw, fsco);

	        double forceResultant = forceRaw  +  totalMass*(accel + 9.81 * Math.Sin(fse.AngleDefault * Math.PI / 180.0));

		//LogB.Information(string.Format("Abs(forceRaw): {0}, totalMass: {1}, forceResultant: {2}",
		//			Math.Abs(forceRaw), totalMass, forceResultant));

		return forceResultant;
	}
	private static double calculeForceWithCaptureOptions(double force, CaptureOptions fsco)
	{
		if(fsco == CaptureOptions.ABS)
			return Math.Abs(force);
		if(fsco == CaptureOptions.INVERTED)
			return -1 * force;

		return force;
	}

	public static string GetCaptureOptionsString(CaptureOptions co)
	{
		if(co == ForceSensor.CaptureOptions.ABS)
			return CaptureOptionsStringABS();
		else if(co == ForceSensor.CaptureOptions.INVERTED)
			return CaptureOptionsStringINVERTED();
		else
			return CaptureOptionsStringNORMAL();

	}

	//uniqueID:name
	public ForceSensor ChangePerson(string newIDAndName)
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

		//will update SqliteForceSensor
		return (this);
	}

	public static string ReadTrigger (string s)
	{
		if(s == "R")
			return "RCA ON";
		else if(s == "r")
			return "RCA OFF";
		else if(s == "B")
			return "Button ON";
		else if(s == "b")
			return "Button OFF";
		else
			return "RCA or button unknown, read: " + s;
	}

	public string FullURL
	{
		get { return Util.GetForceSensorSessionDir(sessionID) + Path.DirectorySeparatorChar + filename; }
	}
	public string FullVideoURL
	{
		get {
			if(videoURL == "")
				return "";

			return Util.GetVideoFileName(sessionID, Constants.TestTypes.FORCESENSOR, uniqueID);
		}
	}

	public string DateTimePublic
	{
		get { return dateTime; }
	}
	public string DatePublic
	{
		get {
			if(dateTime.Split(new char[] {'_'}).Length == 2)
				return Util.ChangeChars(dateTime.Split(new char[] {'_'})[0], "-", "/");
			else
				return "";
		}
	}
	public string TimePublic
	{
		get {
			if(dateTime.Split(new char[] {'_'}).Length == 2)
				return Util.ChangeChars(dateTime.Split(new char[] {'_'})[1], "-", ":");
			else
				return "";
		}
	}

	//used to do selects on the software
	public static int GetElasticIntFromMode (Constants.Modes mode)
	{
		int elastic = -1;
		if (mode == Constants.Modes.FORCESENSORISOMETRIC)
			elastic = 0;
		else if (mode == Constants.Modes.FORCESENSORELASTIC)
			elastic = 1;

		return elastic;
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
	public CaptureOptions CaptureOption
	{
		get { return captureOption; }
		set { captureOption = value; }
	}
	public string Laterality
	{
		get { return laterality; }
		set { laterality = value; }
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
	public double Stiffness
	{
		get { return stiffness; }
		set { stiffness = value; }
	}
	public string StiffnessString
	{
		get { return stiffnessString; }
		set { stiffnessString = value; }
	}
	public string ExerciseName
	{
		get { return exerciseName; }
		set { exerciseName = value; }
	}
	public double MaxForceRaw
	{
		set { maxForceRaw = value; }
	}
	public double MaxAvgForce1s
	{
		set { maxAvgForce1s = value; }
	}
}

public class ForceSensorExercise
{
	private int uniqueID;
	private string name;
	private int percentBodyWeight;
	private string resistance;
	private int angleDefault;
	private string description;
	private bool tareBeforeCapture;
	private bool forceResultant;

	public enum Types { ISOMETRIC, ELASTIC, BOTH };
	private Types type;

	//private bool eccReps;
	public enum RepetitionsShowTypes { CONCENTRIC, BOTHTOGETHER, BOTHSEPARATED };
	private RepetitionsShowTypes repetitionsShow;
	private double eccMin;
	private double conMin;

	/*
	 * note percentBodyWeight and tareBeforeCapture will not be true at the same time, so there are three modes on total mass management (see diagrams/processes/forceSensorExerciseParameters)
	 * 	add effect of the mass (percentBodyWeight > 0, tareBeforeCapture = false)
	 * 	subtract effect off the mass (percentBodyWeight = 0, tareBeforeCapture = true)
	 * 	effect of the mass is included in raw data (percentBodyWeight = 0, tareBeforeCapture = false)
	 */

	public ForceSensorExercise()
	{
		//default values
		this.forceResultant = false;
		this.type = Types.ISOMETRIC;
	}

	public ForceSensorExercise(string name)
	{
		this.name = name;

		//default values
		this.forceResultant = false;
		this.type = Types.ISOMETRIC;
	}

	public ForceSensorExercise(int uniqueID, string name, int percentBodyWeight, string resistance, int angleDefault,
			string description, bool tareBeforeCapture, bool forceResultant,
			Types type, RepetitionsShowTypes repetitionsShow, double eccMin, double conMin)
	{
		this.uniqueID = uniqueID;
		this.name = name;
		this.percentBodyWeight = percentBodyWeight;
		this.resistance = resistance;
		this.angleDefault = angleDefault;
		this.description = description;
		this.tareBeforeCapture = tareBeforeCapture;
		this.forceResultant = forceResultant;
		this.type = type;
		this.repetitionsShow = repetitionsShow;
		this.eccMin = eccMin;
		this.conMin = conMin;
	}

	//constructor at DB: 1.86
	public ForceSensorExercise(int uniqueID, string name, int percentBodyWeight, string resistance, int angleDefault,
			string description, bool tareBeforeCapture, bool forceResultant, Types type)
	{
		this.uniqueID = uniqueID;
		this.name = name;
		this.percentBodyWeight = percentBodyWeight;
		this.resistance = resistance;
		this.angleDefault = angleDefault;
		this.description = description;
		this.tareBeforeCapture = tareBeforeCapture;
		this.forceResultant = forceResultant;
		this.type = type;
	}

	public override string ToString()
	{
		return uniqueID.ToString() + ":" + name + ":" + percentBodyWeight.ToString() + ":" +
			resistance + ":" + angleDefault.ToString() + ":" + description + ":" +
			tareBeforeCapture.ToString() + ":" + forceResultant.ToString() + ":" +
			type.ToString() + ":" +
			repetitionsShow.ToString() + ":" + eccMin.ToString() + ":" + conMin.ToString();
	}

	public int RepetitionsShowToCode()
	{
		/*
		  DB 2.21 and before	DB ~2.22 	SQL code
		  eccReps 0		CONCENTRIC		0
		  eccReps 1		BOTHTOGETHER		1
					BOTHSEPARATED		2

		  ~2.22 because is not really a change on DB
		*/

		if(repetitionsShow == RepetitionsShowTypes.CONCENTRIC)
			return 0;
		else if(repetitionsShow == RepetitionsShowTypes.BOTHTOGETHER)
			return 1;
		else if(repetitionsShow == RepetitionsShowTypes.BOTHSEPARATED)
			return 2;

		return 0;
	}
	public static RepetitionsShowTypes RepetitionsShowFromCode(int code)
	{
		if(code == 0)
			return RepetitionsShowTypes.CONCENTRIC;
		else if(code == 1)
			return RepetitionsShowTypes.BOTHTOGETHER;
		else if(code == 2)
			return RepetitionsShowTypes.BOTHSEPARATED;

		return RepetitionsShowTypes.CONCENTRIC;
	}

	public int TypeToInt ()
	{
		if (type == Types.ISOMETRIC)
			return 0;
		else if (type == Types.ELASTIC)
			return 1;
		else //if (type == Types.BOTH)
			return -1; //-1 as is the same than a select inespecific
	}
	public static Types IntToType (int i)
	{
		if (i == 0)
			return Types.ISOMETRIC;
		else if (i == 1)
			return Types.ELASTIC;
		else //if (type == -1)
			return Types.BOTH; //-1 as is the same than a select inespecific
	}

	public string ToSQLInsertString()
	{
		string uniqueIDStr = "NULL";
		if(uniqueID != -1)
			uniqueIDStr = uniqueID.ToString();

		return
			uniqueIDStr + ", \"" + name + "\", " + percentBodyWeight + ", \"" +
			resistance + "\", " + angleDefault + ", \"" + description + "\", " +
			Util.BoolToInt(tareBeforeCapture).ToString() + ", " +
			Util.BoolToInt(forceResultant).ToString() + ", " +
			TypeToInt ().ToString() + ", " +
			RepetitionsShowToCode().ToString() + ", " +
			Util.ConvertToPoint(eccMin) + ", " + Util.ConvertToPoint(conMin);
	}

	// to be able to import
	public string ToSQLInsertString_DB_1_68()
	{
		string uniqueIDStr = "NULL";
		if(uniqueID != -1)
			uniqueIDStr = uniqueID.ToString();

		return
			uniqueIDStr + ", \"" + name + "\", " + percentBodyWeight + ", \"" +
			resistance + "\", " + angleDefault + ", \"" + description + "\", " +
			Util.BoolToInt(tareBeforeCapture).ToString();
	}

	public bool Changed (ForceSensorExercise newEx)
	{
		if(
				name == newEx.Name &&
				percentBodyWeight == newEx.PercentBodyWeight &&
				resistance == newEx.Resistance &&
				angleDefault == newEx.AngleDefault &&
				description == newEx.Description &&
				tareBeforeCapture == newEx.TareBeforeCaptureOnExerciseEdit &&
				forceResultant == newEx.ForceResultant &&
				type == newEx.Type &&
				repetitionsShow == newEx.RepetitionsShow &&
				eccMin == newEx.EccMin &&
				conMin == newEx.ConMin)
			return false;

		return true;
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
	public string Resistance
	{
		get { return resistance; }
		set { resistance = value; }
	}
	public int AngleDefault
	{
		get { return angleDefault; }
	}
	public string Description
	{
		get { return description; }
		set { description = value; }
	}

	/*
	   at exercise edit with forceResultant, user can select tareBeforeCapture
	   if edit again and select forceRaw, tareBeforeCapture bool is still ok on sqlite,
	   just to be shown again if user want to go to forceResultant again.
	   So this applies to TareBeforeCaptureOnExerciseEdit

	   But, on capture, only apply tareBeforeCapture if forceResultant is true,
	   so useTareBeforeCaptureAndForceResultant
	   */

	public bool TareBeforeCaptureAndForceResultant
	{
		get { return (tareBeforeCapture && forceResultant); }
	}
	public bool TareBeforeCaptureOnExerciseEdit
	{
		get { return tareBeforeCapture; }
	}
	public bool ForceResultant
	{
		get { return forceResultant; }
	}

	public Types Type
	{
		get { return type; }
	}
	public bool ComputeAsElastic //use this
	{
		get { return forceResultant && type != Types.ISOMETRIC; }
	}
	public RepetitionsShowTypes RepetitionsShow
	{
		get { return repetitionsShow; }
		set { repetitionsShow = value; }
	}
	public double EccMin
	{
		get { return eccMin; }
	}
	public double ConMin
	{
		get { return conMin; }
	}

	public double GetEccOrConMinMaybePreferences(bool ecc, double prefsMinDispl, int prefsMinForce)
	{
		if(ecc && eccMin >= 0)
			return eccMin;
		if(! ecc && conMin >= 0)
			return conMin;

		if(ComputeAsElastic)
			return prefsMinDispl;
		else
			return prefsMinForce;
	}

}

public class ForceSensorElasticBand
{
	private int uniqueID;
	private int active;
	private string brand;
	private string color;
	private double stiffness;
	private string comments;


	// constructors ----

	public ForceSensorElasticBand()
	{
		uniqueID = -1; //undefined
	}

	public ForceSensorElasticBand(int uniqueID, int active, string brand, string color, double stiffness, string comments)
	{
		this.uniqueID = uniqueID;
		this.active = active;
		this.brand = brand;
		this.color = color;
		this.stiffness = stiffness;
		this.comments = comments;
	}

	// public methods ----

	public void Update(int active, string brand, string color, double stiffness, string comments)
	{
		this.active = active;
		this.brand = brand;
		this.color = color;
		this.stiffness = stiffness;
		this.comments = comments;
	}

	public string ToSQLInsertString()
	{
		string uniqueIDStr = "NULL";
		if(uniqueID != -1)
			uniqueIDStr = uniqueID.ToString();

		//LogB.Information("stiffness is: " + stiffness.ToString());
		return
			uniqueIDStr + ", " + active.ToString() +
			", \"" + brand + "\", \"" + color + "\", " +
			Util.ConvertToPoint(stiffness) + ", \"" + comments + "\"";
	}

	public string [] ToStringArray ()
	{
		int all = 6;
		string [] str = new String [all];
		int i=0;
		str[i++] = uniqueID.ToString();
		str[i++] = active.ToString();
		str[i++] = stiffness.ToString();
		str[i++] = brand;
		str[i++] = color;
		str[i++] = comments;

		return str;
	}

	//public static methods ----

	//with one SqliteForceSensorElasticBand.SelectAll call, we can use then this two methods
	public static double GetStiffnessOfActiveBands (List<ForceSensorElasticBand> list_fseb)
	{
		double sum = 0;
		foreach(ForceSensorElasticBand fseb in list_fseb)
			sum += fseb.Stiffness * fseb.Active;

		return sum;
	}
	public static string GetIDsOfActiveBands (List<ForceSensorElasticBand> list_fseb)
	{
		string str = "";
		string sep = "";
		foreach(ForceSensorElasticBand fseb in list_fseb)
		{
			LogB.Information("pppp fseb: " + Util.StringArrayToString(fseb.ToStringArray(), ";"));
			str += sep + string.Format("{0}*{1}", fseb.UniqueID, fseb.Active);
			sep = ";";
		}

		return str;
	}

	/*
	 * stiffnessString is the string of a loaded set
	 * stiffnessToBeReached is the double of stiffness of a loaded set
	 * processing them have to match if there are no deletions of elastic bands or their values have been changed
	 *
	 * return if stiffnessString with current elastic bands on SQL can achieve deired stiffness
	 */
	public static bool UpdateBandsStatusToSqlite (List<ForceSensorElasticBand> list_at_db, string stiffnessString, double stiffnessToBeReached)
	{
		List<ForceSensorElasticBand> list_to_db = new List<ForceSensorElasticBand>();

		double stiffnessAcumulated = 0;
		foreach(ForceSensorElasticBand fseb in list_at_db)
		{
			string [] strAll = stiffnessString.Split(new char[] {';'});
			bool found = false;
			ForceSensorElasticBand fsebNew = fseb;
			//LogB.Information("processing: " + Util.StringArrayToString(fsebNew.ToStringArray(), ";"));
			foreach(string strBand in strAll)
			{
				string [] strBandWithMult = strBand.Split(new char[] {'*'});
				if(strBandWithMult.Length == 2 && Util.IsNumber(strBandWithMult[0], false) &&
						Util.IsNumber(strBandWithMult[1], false) && Convert.ToInt32(strBandWithMult[0]) == fseb.UniqueID)
				{
					fsebNew.active = Convert.ToInt32(strBandWithMult[1]);
					stiffnessAcumulated += fsebNew.active * fsebNew.Stiffness;

					list_to_db.Add(fsebNew);
					found = true;
					break;
				}
			}

			if(! found) {
				fsebNew.active = 0;
				list_to_db.Add(fsebNew);
			}

		}
		SqliteForceSensorElasticBand.UpdateList(false, list_to_db);

		//LogB.Information(string.Format("stiffness match: {0}, {1}", stiffnessAcumulated, stiffnessToBeReached));
		return Util.SimilarDouble(stiffnessAcumulated, stiffnessToBeReached);
	}

	public int UniqueID
	{
		get { return uniqueID; }
	}
	public int Active
	{
		get { return active; }
		set { active = value; }
	}
	public string Brand
	{
		get { return brand; }
	}
	public string Color
	{
		get { return color; }
	}
	public double Stiffness
	{
		get { return stiffness; }
	}
	public string Comments
	{
		get { return comments; }
	}
}

//struct with relevant data used on various functions and threads
//for force, but can be also for position on elastic
public class ForceSensorValues
{
	public int TimeLast; //store last time
	public int TimeValueMax; //store time of max force (only used on force)
	public double ValueLast; //store last force (or displ)
	public double Max; //store max force (or displ)
	public double Min; //store min force (or displ)
	public double BestSecond; //max avg force in 1s
	public double BestRFD; //avg RFD in 50 ms

	public ForceSensorValues()
	{
		TimeLast = 0;
		TimeValueMax = 0;
		ValueLast = 0;
		Max = -10000;
		Min = 10000;
		BestSecond = 0;
		BestRFD = 0;
	}

	public void SetMaxMinIfNeeded(double newValue, int time)
	{
		if(newValue > Max)
		{
			Max = newValue;
			TimeValueMax = time;
		}
		if(newValue < Min)
			Min = newValue;
	}
}

public class TriggerXForce
{
	public Trigger trigger;
	public int x; //x on screen
	public double force; //force at trigger
	public bool painted;

	public TriggerXForce (Trigger trigger, int x, double force)
	{
		this.trigger = trigger;
		this.x = x;
		this.force = force;
		this.painted = false;
	}
}

public class CalculatedForceMaxAvgInWindow
{
	private int countA;
	private int countB;
	private double windowSeconds;
	private double result; //avgMax
	private int resultSampleStart; //avgMaxSampleStart
	private int resultSampleEnd; //avgMaxSampleEnd

	public CalculatedForceMaxAvgInWindow (int countA, int countB, double windowSeconds,
			double result, int resultSampleStart, int resultSampleEnd)
	{
		this.countA = countA;
		this.countB = countB;
		this.windowSeconds = windowSeconds;
		this.result = result;
		this.resultSampleStart = resultSampleStart;
		this.resultSampleEnd = resultSampleEnd;
	}
	public CalculatedForceMaxAvgInWindow (int countA, int countB, double windowSeconds)
	{
		this.countA = countA;
		this.countB = countB;
		this.windowSeconds = windowSeconds;
	}

	public string InputsToString()
	{
		return string.Format("{0};{1};{2}", countA, countB, windowSeconds);
	}

	public double Result
	{
		get { return result; }
	}
	public int ResultSampleStart
	{
		get { return resultSampleStart; }
	}
	public int ResultSampleEnd
	{
		get { return resultSampleEnd; }
	}
}

public class ForceSensorRFD
{
	//if these names change, change FunctionPrint() below
	public enum Functions { RAW, FITTED } //on SQL is inserted like this
	protected static string function_RAW_name = "RAW";
	protected static string function_FITTED_name = "Fitted";

	//if these names change, change TypePrint() below
	public enum Types { INSTANTANEOUS, AVERAGE, PERCENT_F_MAX, RFD_MAX, BEST_AVG_RFD_IN_X_MS, IMP_UNTIL_PERCENT_F_MAX, IMP_RANGE } //on SQL is inserted like this
	private static string type_INSTANTANEOUS_name = "Instantaneous";
	private static string type_AVERAGE_name = "Average";
	private static string type_PERCENT_F_MAX_name = "% Force max";
	private static string type_RFD_MAX_name = "RFD max";
	private static string type_BEST_AVG_RFD_IN_X_MS_name = "Best average RFD";

	public string code; //RFD1...4 //I: on impulse
	public bool active;
	public Functions function;
	public Types type;
	public int num1;
	public int num2;

	//constructor for inheritance
	public ForceSensorRFD()
	{
	}

	public ForceSensorRFD(string code, bool active, Functions function, Types type, int num1, int num2)
	{
		this.code = code;
		this.active = active;
		this.function = function;
		this.type = type;
		this.num1 = num1;
		this.num2 = num2;
	}

	public bool Changed(ForceSensorRFD newRFD)
	{
		if(
				code == newRFD.code && active == newRFD.active &&
				function == newRFD.function && type == newRFD.type &&
				num1 == newRFD.num1 && num2 == newRFD.num2)
			return false;

		return true;
	}

	public static string [] FunctionsArray(bool translated)
	{
		if(translated)
			return new string [] { Catalog.GetString(function_RAW_name), Catalog.GetString(function_FITTED_name) };
		else
			return new string [] { function_RAW_name, function_FITTED_name };
	}
	public static string [] TypesArray(bool translated)
	{
		if(translated)
			return new string [] {
				Catalog.GetString(type_INSTANTANEOUS_name), Catalog.GetString(type_AVERAGE_name),
					Catalog.GetString(type_PERCENT_F_MAX_name), Catalog.GetString(type_RFD_MAX_name),
					Catalog.GetString(type_BEST_AVG_RFD_IN_X_MS_name)
			};
		else
			return new string [] {
				type_INSTANTANEOUS_name, type_AVERAGE_name,
					type_PERCENT_F_MAX_name, type_RFD_MAX_name,
					type_BEST_AVG_RFD_IN_X_MS_name
			};
	}

	public string FunctionPrint(bool translated)
	{
		if(function == Functions.RAW) {
			if(translated)
				return Catalog.GetString(function_RAW_name);
			else
				return function_RAW_name;
		}

		if(translated)
			return Catalog.GetString(function_FITTED_name);
		else
			return function_FITTED_name;
	}

	public virtual string TypePrint(bool translated)
	{
		if (type == Types.INSTANTANEOUS) {
			if (translated)
				return Catalog.GetString (type_INSTANTANEOUS_name);
			else
				return type_INSTANTANEOUS_name;
		}
		else if (type == Types.AVERAGE) {
			if (translated)
				return Catalog.GetString (type_AVERAGE_name);
			else
				return type_AVERAGE_name;
		}
		else if (type == Types.PERCENT_F_MAX) {
			if (translated)
				return Catalog.GetString (type_PERCENT_F_MAX_name);
			else
				return type_PERCENT_F_MAX_name;
		}
		else if (type == Types.RFD_MAX) {
			if (translated)
				return Catalog.GetString (type_RFD_MAX_name);
			else
				return type_RFD_MAX_name;
		}
		else { //if (type == Types.BEST_AVG_RFD_IN_X_MS)
			if (translated)
				return Catalog.GetString (type_BEST_AVG_RFD_IN_X_MS_name);
			else
				return type_BEST_AVG_RFD_IN_X_MS_name;
		}
	}

	public string ToSQLInsertString()
	{
		return
			"\"" + code  + "\"" + "," +
			Util.BoolToInt(active).ToString() + "," +
			"\"" + function.ToString() + "\"" + "," +
			"\"" + type.ToString() + "\"" + "," +
			num1.ToString() + "," +
			num2.ToString();
	}

	public string ToR()
	{
		return function.ToString() + ";" + type.ToString() + ";" + num1.ToString() + ";" + num2.ToString();
	}

	public string ToExport(bool translated, string sep)
	{
		return FunctionPrint(translated) + sep +
			TypePrint(translated) + sep +
			num1 + sep +
			num2;
	}

	public bool Active
	{
		get { return active; }
	}

	public static string Function_RAW_name
	{
		get { return function_RAW_name; }
	}
	public static string Function_FITTED_name
	{
		get { return function_FITTED_name; }
	}

	public static string Type_INSTANTANEOUS_name
	{
		get { return type_INSTANTANEOUS_name; }
	}
	public static string Type_AVERAGE_name
	{
		get { return type_AVERAGE_name; }
	}
	public static string Type_PERCENT_F_MAX_name
	{
		get { return type_PERCENT_F_MAX_name; }
	}
	public static string Type_RFD_MAX_name
	{
		get { return type_RFD_MAX_name; }
	}
	public static string Type_BEST_AVG_RFD_IN_X_MS_name
	{
		get { return type_BEST_AVG_RFD_IN_X_MS_name; }
	}
}

public class ForceSensorImpulse : ForceSensorRFD
{
	//if these names change, change TypePrint() below
	private static string type_IMP_UNTIL_PERCENT_F_MAX_name = "Until % Force max";
	private static string type_IMP_RANGE_name = "Range";

	public ForceSensorImpulse()
	{
	}

	public ForceSensorImpulse(bool active, Functions function, Types type, int num1, int num2)
	{
		this.code = "I";
		this.active = active;
		this.function = function;
		this.type = type;
		this.num1 = num1;
		this.num2 = num2;
	}

	public bool Changed(ForceSensorImpulse newImpulse)
	{
		if(
				active == newImpulse.active &&
				function == newImpulse.function && type == newImpulse.type &&
				num1 == newImpulse.num1 && num2 == newImpulse.num2)
			return false;

		return true;
	}

	public static string [] TypesArrayImpulse(bool translated)
	{
		if(translated)
			return new string [] {
				Catalog.GetString(type_IMP_UNTIL_PERCENT_F_MAX_name), Catalog.GetString(type_IMP_RANGE_name),
			};
		else
			return new string [] {
				type_IMP_UNTIL_PERCENT_F_MAX_name, type_IMP_RANGE_name
			};
	}

	public override string TypePrint(bool translated)
	{
		if(type == Types.IMP_UNTIL_PERCENT_F_MAX) {
			if(translated)
				return Catalog.GetString(type_IMP_UNTIL_PERCENT_F_MAX_name);
			else
				return type_IMP_UNTIL_PERCENT_F_MAX_name;
		}
		else { // if(type == Types.IMP_RANGE)
			if(translated)
				return Catalog.GetString(type_IMP_RANGE_name);
			else
				return type_IMP_RANGE_name;
		}
	}

	public static string Type_IMP_UNTIL_PERCENT_F_MAX_name
	{
		get { return type_IMP_UNTIL_PERCENT_F_MAX_name; }
	}
	public static string Type_IMP_RANGE_name
	{
		get { return type_IMP_RANGE_name; }
	}
}

//A-B data sent to R
//can be just one on analyze or multiple (as a list) on export
public class ForceSensorGraphAB
{
	//for graph and for export
	public ForceSensor.CaptureOptions fsco;
	public int startSample;
	public int endSample;
	public string title;
	public string exercise;
	public string date;
	public string time;
	public TriggerList triggerList;

	protected void assignParams(ForceSensor.CaptureOptions fsco, int startSample, int endSample,
			string title, string exercise, string date, string time, TriggerList triggerList)
	{
		this.fsco = fsco;
		this.startSample = startSample;
		this.endSample = endSample;
		this.title = title;
		this.exercise = exercise;
		this.date = date;
		this.time = time;
		this.triggerList = triggerList;
	}

	//for inheritance
	public ForceSensorGraphAB ()
	{
	}

	//constructor for graph on analyze
	public ForceSensorGraphAB (
			ForceSensor.CaptureOptions fsco, int startSample, int endSample,
			string title, string exercise, string date, string time, TriggerList triggerList)
	{
		assignParams(fsco, startSample, endSample, title, exercise, date, time, triggerList);
	}


}
//this class creates the rows of each force sensor AB for the csv input multi that is read by R
public class ForceSensorGraphABExport: ForceSensorGraphAB
{
	private bool isWindows;
	public string fullURL;
	public bool decimalIsPoint;
	public double maxForceRaw;
	public double maxAvgForceInWindow;
	public double forceSensorAnalyzeMaxAVGInWindowSeconds;
	public double maxAvgForceInWindowSampleStart;
	public double maxAvgForceInWindowSampleEnd;
	public string laterality;
	public int setCount;
	public int repCount;
	public string commentOfSet;

	public ForceSensorGraphABExport (
			bool isWindows,
			string fullURL, bool decimalIsPoint, double maxForceRaw,
			double maxAvgForceInWindow, double forceSensorAnalyzeMaxAVGInWindowSeconds,
			double maxAvgForceInWindowSampleStart, double maxAvgForceInWindowSampleEnd,
			string laterality, int setCount, int repCount, string commentOfSet,
			ForceSensor.CaptureOptions fsco, int startSample, int endSample,
			string title, string exercise, string date, string time, TriggerList triggerList)
	{
		assignParams(fsco, startSample, endSample, title, exercise, date, time, triggerList);

		this.isWindows = isWindows;
		this.fullURL = fullURL;
		this.decimalIsPoint = decimalIsPoint;
		this.maxForceRaw = maxForceRaw;
		this.maxAvgForceInWindow = maxAvgForceInWindow;
		this.forceSensorAnalyzeMaxAVGInWindowSeconds = forceSensorAnalyzeMaxAVGInWindowSeconds;
		this.maxAvgForceInWindowSampleStart = maxAvgForceInWindowSampleStart;
		this.maxAvgForceInWindowSampleEnd = maxAvgForceInWindowSampleEnd;
		this.laterality = laterality;
		this.setCount = setCount;
		this.repCount = repCount;
		this.commentOfSet = commentOfSet;
	}

	public string ToCSVRowOnExport()
	{
		//since 2.0.3 decimalChar is . (before it was locale specific)
		string decimalChar = ".";
		if(! decimalIsPoint)
		{
			System.Globalization.NumberFormatInfo localeInfo = new System.Globalization.NumberFormatInfo();
			localeInfo = System.Globalization.NumberFormatInfo.CurrentInfo;
			decimalChar = localeInfo.NumberDecimalSeparator;
		}

		string url = fullURL;
		if(isWindows)
			url = url.Replace("\\","/");

		return url + ";" +
			decimalChar + ";" +
			Util.ConvertToPoint(maxForceRaw) + ";" +
			Util.ConvertToPoint(maxAvgForceInWindow) + ";" +
			maxAvgForceInWindowSampleStart + ";" +
			maxAvgForceInWindowSampleEnd + ";" +
			fsco.ToString() + ";" +
			title + ";" +
			exercise + ";" +
			date + ";" +
			time + ";" +
			laterality + ";" +
			setCount + ";" +
			repCount + ";" +
			"\"\";\"\";" + 	// triggers unused on export
			startSample.ToString() + ";" +
			endSample.ToString() + ";" +
			Util.RemoveNewLine(Util.RemoveChar(commentOfSet, ';'), true);
	}

	public static string PrintCSVHeaderOnExport()
	{
		return "fullURL;decimalChar;maxForceRaw;" +
			"maxAvgForceInWindow;maxAvgForceInWindowSampleStart;maxAvgForceInWindowSampleEnd;" +
			"captureOptions;title;exercise;date;time;laterality;set;rep;" +
			"triggersON;triggersOFF;" + //unused on export
			"startSample;endSample;comments";
	}
}


public class ForceSensorGraph
{
	ForceSensor.CaptureOptions fsco;
	List<ForceSensorRFD> rfdList;
	ForceSensorImpulse impulse;
	double averageLength;
	int percentChange;
	bool vlineT0;
	bool vline50fmax_raw;
	bool vline50fmax_fitted;
	bool hline50fmax_raw;
	bool hline50fmax_fitted;
	double testLength;
	string title;
	string exercise;
	string date;
	string time;
	private TriggerList triggerList;
	private int startSample;
	private int endSample;
	private bool startEndOptimized;
	private bool decimalIsPointAtReadFile; //but on export this will be related to each set
	private char exportDecimalSeparator;
	private double forceSensorAnalyzeMaxAVGInWindowSeconds; //on export
	private bool includeImagesOnExport;

	//private method to help on assigning params
	private void assignGenericParams(
			List<ForceSensorRFD> rfdList,
			ForceSensorImpulse impulse, double testLength, int percentChange,
			bool startEndOptimized,
			bool decimalIsPointAtReadFile, 	//at read
			char exportDecimalSeparator 	//at write
			)
	{
		//generic of any data
		this.rfdList = rfdList;
		this.impulse = impulse;
		this.testLength = testLength;
		this.percentChange = percentChange;
		this.startEndOptimized = startEndOptimized;
		this.decimalIsPointAtReadFile = decimalIsPointAtReadFile;
		this.exportDecimalSeparator = exportDecimalSeparator;

		averageLength = 0.1;
		vlineT0 = false;
		vline50fmax_raw = false;
		vline50fmax_fitted = false;
		hline50fmax_raw = false;
		hline50fmax_fitted = false;
		includeImagesOnExport = false;
	}

	//constructor for analyze one graph of a set from startSample to endSample. singleOrMultiple = true
	public ForceSensorGraph(
			List<ForceSensorRFD> rfdList,
			ForceSensorImpulse impulse, double testLength, int percentChange,
			bool startEndOptimized,
			bool decimalIsPointAtReadFile,
			char exportDecimalSeparator,
			ForceSensorGraphAB fsgAB
			)
	{
		assignGenericParams(rfdList, impulse, testLength, percentChange, startEndOptimized,
				decimalIsPointAtReadFile, exportDecimalSeparator);

		//this A-B data
		this.fsco = fsgAB.fsco;
		this.startSample = fsgAB.startSample;
		this.endSample = fsgAB.endSample;
		this.title = fsgAB.title;
		this.exercise = fsgAB.exercise;
		this.date = fsgAB.date;
		this.time = fsgAB.time;
		this.triggerList = fsgAB.triggerList;
	}

	//constructor for export. singleOrMultiple = false
	public ForceSensorGraph(
			List<ForceSensorRFD> rfdList,
			ForceSensorImpulse impulse, double testLength, int percentChange,
			bool startEndOptimized,
			bool decimalIsPointAtReadFile, //this param is used here to print results. but to read data what id is used is in fsgAB_l
			char exportDecimalSeparator,
			List<ForceSensorGraphABExport> fsgABe_l,
			double forceSensorAnalyzeMaxAVGInWindowSeconds,
			bool includeImagesOnExport
			)
	{
		assignGenericParams(rfdList, impulse, testLength, percentChange, startEndOptimized,
				decimalIsPointAtReadFile, exportDecimalSeparator);
			
		this.forceSensorAnalyzeMaxAVGInWindowSeconds = forceSensorAnalyzeMaxAVGInWindowSeconds;
		this.includeImagesOnExport = includeImagesOnExport;

		writeMultipleFilesCSV(fsgABe_l);
	}

	//multiple is export
	public bool CallR(int graphWidth, int graphHeight, bool singleOrMultiple)
	{
		LogB.Information("\nforceSensor CallR ----->");
		writeOptionsFile(graphWidth, graphHeight, singleOrMultiple);
		return ExecuteProcess.CallR(UtilEncoder.GetmifScript());
	}

	private void writeOptionsFile(int graphWidth, int graphHeight, bool singleOrMultiple)
	{
		LogB.Information("writeOptionsFile 0");
		string scriptsPath = UtilEncoder.GetSprintPath();
		if(UtilAll.IsWindows())
			scriptsPath = scriptsPath.Replace("\\","/");

		System.Globalization.NumberFormatInfo localeInfo = new System.Globalization.NumberFormatInfo();
		localeInfo = System.Globalization.NumberFormatInfo.CurrentInfo;

		LogB.Information("writeOptionsFile 1");
		//since 2.0.3 decimalChar is . (before it was locale specific)
		string decimalCharAtFile = ".";
		if(! decimalIsPointAtReadFile)
			decimalCharAtFile = localeInfo.NumberDecimalSeparator;

		string scriptOptions =
			"#os\n" + 			UtilEncoder.OperatingSystemForRGraphs() + "\n" +
			"#decimalCharAtFile\n" +	decimalCharAtFile + "\n" +
			"#graphWidth\n" + 		graphWidth.ToString() + "\n" +
			"#graphHeight\n" + 		graphHeight.ToString() + "\n" +
			"#averageLength\n" + 		Util.ConvertToPoint(averageLength) + "\n" +
			"#percentChange\n" + 		percentChange.ToString() + "\n" +
			"#vlineT0\n" + 			Util.BoolToRBool(vlineT0) + "\n" +
			"#vline50fmax.raw\n" + 		Util.BoolToRBool(vline50fmax_raw) + "\n" +
			"#vline50fmax.fitted\n" + 	Util.BoolToRBool(vline50fmax_fitted) + "\n" +
			"#hline50fmax.raw\n" + 		Util.BoolToRBool(hline50fmax_raw) + "\n" +
			"#hline50fmax.fitted\n" + 	Util.BoolToRBool(hline50fmax_fitted) + "\n" +
			"#RFDs";

		LogB.Information("writeOptionsFile 2");
		foreach(ForceSensorRFD rfd in rfdList)
			if(rfd.active)
				scriptOptions += "\n" + rfd.ToR();
			else
				scriptOptions += "\n-1";

		LogB.Information("writeOptionsFile 3");
		if(impulse.active)
			scriptOptions += "\n" + impulse.ToR();
		else
			scriptOptions += "\n-1";

		LogB.Information("writeOptionsFile 4");

		string captureOptionsStr = "-1";
		string triggersOnStr = TriggerList.TriggersNotFoundString;
		string triggersOffStr = TriggerList.TriggersNotFoundString;
		string forceSensorAnalyzeMaxAVGInWindowSecondsStr =
			Util.ConvertToPoint(forceSensorAnalyzeMaxAVGInWindowSeconds);

		if(singleOrMultiple)
		{
			captureOptionsStr = fsco.ToString();
			triggersOnStr = printTriggers(TriggerList.Type3.ON);
			triggersOffStr = printTriggers(TriggerList.Type3.OFF);
			forceSensorAnalyzeMaxAVGInWindowSecondsStr = "-1";
		} else {
			captureOptionsStr = "-1";
			title = "-1";
			exercise = "-1";
			date = "-1";
			time = "-1";
		}

		scriptOptions +=
			"\n#testLength\n" + 		Util.ConvertToPoint(testLength) + "\n" +
			"#captureOptions\n" + 		captureOptionsStr + "\n" + 	//unused on multiple
			"#title\n" + 			title + "\n" + 			//unused on multiple
			"#exercise\n" + 		exercise + "\n" +		//unused on multiple
			"#date\n" + 			date + "\n" +			//unused on multiple
			"#time\n" + 			time + "\n" +			//unused on multiple
			"#scriptsPath\n" + 		UtilEncoder.GetScriptsPath() + "\n" +
			triggersOnStr + "\n" + 						//unused on multiple
			triggersOffStr + "\n" + 						//unused on multiple
			"#startSample\n" + 		startSample.ToString() + "\n" +	//unused on multiple
			"#endSample\n" + 		endSample.ToString() + "\n" +	//unused on multiple
			"#startEndOptimized\n" +	Util.BoolToRBool(startEndOptimized) + "\n" +
			"#singleOrMultiple\n" +		Util.BoolToRBool(singleOrMultiple) + "\n" +
			"#decimalCharAtExport\n" +	exportDecimalSeparator + "\n" +
			"#maxAvgInWindowSeconds\n" + 	forceSensorAnalyzeMaxAVGInWindowSecondsStr + "\n" +
			"#includeImagesOnExport\n" + 	Util.BoolToRBool(includeImagesOnExport) + "\n";

		/*
		#startEndOptimized on gui can be:
		Range of analysis:
		- startEndOptimized FALSE: user AB selection (use AB selected range)
		- startEndOptimized TRUE (default): optimized range (program will find best fitting samples on user selected range)
		*/

		LogB.Information("writeOptionsFile 5");
		TextWriter writer = File.CreateText(Path.GetTempPath() + "Roptions.txt");
		writer.Write(scriptOptions);
		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();
		LogB.Information("writeOptionsFile 6");
	}

	private void writeMultipleFilesCSV(List<ForceSensorGraphABExport> fsgABe_l)
	{
		LogB.Information("writeMultipleFilesCSV start");
		TextWriter writer = File.CreateText(UtilEncoder.GetmifCSVInputMulti());

		//write header
		writer.WriteLine(ForceSensorGraphABExport.PrintCSVHeaderOnExport());

		//write fsgAB_l for
		foreach(ForceSensorGraphABExport fsgABe in fsgABe_l)
			writer.WriteLine(fsgABe.ToCSVRowOnExport());

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
			new System.IO.DirectoryInfo(Util.GetForceSensorSessionDir(sessionID));
		System.IO.DirectoryInfo folderGeneric =
			new System.IO.DirectoryInfo(Util.GetForceSensorDir());

		if(folderSession.Exists)
			return Util.GetForceSensorSessionDir(sessionID);
		else if(folderGeneric.Exists)
			return Util.GetForceSensorDir();
		else
			return "";
	}
}

//inherits ForceSensorAnalyzeInstant & RaceAnalyzerAnalyzeInstant
public abstract class AnalyzeInstant
{
	protected string idStr; //just to identify it
	protected List<PointF> p_l;

	/*
	 * forceSensor variables
	 */

	public List<ForceSensorRepetition> ForceSensorRepetition_l;
	//for elastic
	public bool CalculedElasticPSAP;
	public List<double> Position_l;
	public List<double> Speed_l;
	public List<double> Accel_l;
	public List<double> Power_l;

	public int GetLength()
	{
		return p_l.Count;
	}

	//gets an instant value
	public abstract double GetTimeMS (int count);

	/*
	 * forceSensor methods
	 */
	public virtual double GetForceAtCount (int count)
	{
		return 0;
	}

	public virtual double CalculateRFD (int countA, int countB)
	{
		return 0;
	}

	/*
	 * raceAnalyzer methods
	 */

	public virtual double GetSpeedAtCount (int count)
	{
		return 0;
	}

	public virtual double GetRaceAnalyzerAvg (int countA, int countB)
	{
		return 0;
	}

	public virtual double GetRaceAnalyzerMax (int countA, int countB)
	{
		return 0;
	}

	/*
	 * accessors
	 */

	public string IdStr
	{
		get { return idStr; }
	}

	public List<PointF> P_l
	{
		get { return p_l; }
	}
}

public class RaceAnalyzerAnalyzeInstant : AnalyzeInstant
{
	public RaceAnalyzerAnalyzeInstant (string idStr, List<PointF> p_l)
	{
		this.idStr = idStr;
		this.p_l = p_l;
	}

	//gets an instant value
	public override double GetTimeMS (int count)
	{
		return p_l[count].X * 1000; // s -> ms
	}

	public override double GetSpeedAtCount (int count)
	{
		return p_l[count].Y;
	}

	public override double GetRaceAnalyzerAvg (int countA, int countB)
	{
		//GetSubList will know which is minor (countA, countB)
		return PointF.GetAvgY (PointF.GetSubList (p_l, countA, countB));
	}

	public override double GetRaceAnalyzerMax (int countA, int countB)
	{
		//GetSubList will know which is minor (countA, countB)
		return PointF.GetMaxY (PointF.GetSubList (p_l, countA, countB));
	}
}

public class ForceSensorAnalyzeInstant : AnalyzeInstant
{
	public double ForceAVG;
	public double ForceMAX;
	public double SpeedAVG;
	public double SpeedMAX;
	public double AccelAVG;
	public double AccelMAX;
	public double PowerAVG;
	public double PowerMAX;

	private GetMaxAvgInWindow gmaiw;
	private GetBestRFDInWindow briw;
	private VariabilityAndAccuracy vaa;

	private ForceSensorValues forceSensorValues;
	private ForceSensorValues forceSensorValuesDispl; //this class can be used for force, displ, or whatever

//	private List<PointF> pDist_l;

	private ForceSensorExercise fse;

	public ForceSensorAnalyzeInstant(
			string idStr,
			string file,
			int startSample, int endSample,
			ForceSensorExercise fse, double personWeight, ForceSensor.CaptureOptions fsco, double stiffness,
			double eccMinDisplacement, double conMinDisplacement)
	{
		this.idStr = idStr;
		this.fse = fse;

		readFile(file, startSample, endSample, personWeight, fsco, stiffness, eccMinDisplacement, conMinDisplacement);
	}

	private void readFile(string file, int startSample, int endSample,
			double personWeight, ForceSensor.CaptureOptions fsco, double stiffness,
			double eccMinDisplacement, double conMinDisplacement)
	{
		LogB.Information(string.Format("at readFile, startSample: {0}, endSample: {1}", startSample, endSample));

		// 0 initialize

		p_l = new List<PointF> ();
		//if(fse.ComputeAsElastic)
		//	pDist_l = new List<PointF> ();


		List<string> contents = Util.ReadFileAsStringList(file);
		bool headersRow = true;

		forceSensorValues = new ForceSensorValues();
		if(fse.ComputeAsElastic)
			forceSensorValuesDispl = new ForceSensorValues();

		if(contents == null)
			return;

		List<int> times = new List<int>();
		List<double> forces = new List<double>();

		// 1 read all file

		foreach(string str in contents)
		{
			if(headersRow)
				headersRow = false;
			else {
				string [] strFull = str.Split(new char[] {';'});
				if(strFull.Length != 2)
					continue;

				//this can takt forces recorded as , or as . because before 2.0.3 forces decimal was locale specific.
				//since 2.0.3 forces are recorded with .

				if(Util.IsNumber(strFull[0], false) && Util.IsNumber(Util.ChangeDecimalSeparator(strFull[1]), true))
				{
					double timeD = Convert.ToDouble(strFull[0]);

					/*
					 * do not use this code, because  start/end cut will be after calculing forceSensorDynamics
					 * to have same data on that samples on zoom than on full signal
					 *
					//start can be -1 meaning that no zoom has to be applied
					if(startMs != -1)
					{
						if(timeD < startMS || timeD > endMs)
							continue;

						//put time at 0
						timeD -= startMs;
					}
					*/

					times.Add(Convert.ToInt32(timeD));
					forces.Add(Convert.ToDouble(Util.ChangeDecimalSeparator(strFull[1])));
				}
			}
		}

		// 2 calcule dynamics for all file

		ForceSensorDynamics forceSensorDynamics;
		if(fse.ComputeAsElastic)
			forceSensorDynamics = new ForceSensorDynamicsElastic(
					times, forces, fsco, fse, personWeight, stiffness, eccMinDisplacement, conMinDisplacement,
					(startSample >= 0 && endSample >= 0) //zoomed
					);
		else
			forceSensorDynamics = new ForceSensorDynamicsNotElastic(
					times, forces, fsco, fse, personWeight, stiffness, eccMinDisplacement, conMinDisplacement);

		// 3 remove times at start/end of the file to avoid first value from sensor,
		//   and values at start/end (RemoveNValues) needed for the shifted average

		//LogB.Information("not zoomed, second time is: " + times[1].ToString());
		times.RemoveAt(0); //always (not-elastic and elastic) 1st has to be removed, because time is not ok there.

		if(forceSensorDynamics.CalculedElasticPSAP)
			times = times.GetRange(forceSensorDynamics.RemoveNValues +1,
					times.Count -2*forceSensorDynamics.RemoveNValues); // (index, count)


		// 4 at zoom, cut time and force samples

		if(startSample >= 0 && endSample >= 0) //zoom in
		{
			// fix crash on two series where zoom range of a series does not fit in the other
			if (startSample >= times.Count)
				return;

			if (endSample >= times.Count)
				endSample = times.Count -1;

			forceSensorDynamics.CutSamplesForZoom(startSample, endSample); //this takes in account the RemoveNValues
			times = times.GetRange (startSample, endSample - startSample + 1);
		}


		// 5 shift times to the left (make first one zero)

		int startMsInt = times[0];
		for(int j = 0;  j < times.Count; j ++)
			times[j] -= startMsInt;

		forces = forceSensorDynamics.GetForces();

		// 6 get calculated data

		CalculedElasticPSAP = false;
		if(forceSensorDynamics.CalculedElasticPSAP)
		{
			Position_l = forceSensorDynamics.GetPositions();
			Speed_l = forceSensorDynamics.GetSpeeds();
			Accel_l = forceSensorDynamics.GetAccels();
			Power_l = forceSensorDynamics.GetPowers();
			CalculedElasticPSAP = true;
		}
		ForceSensorRepetition_l = forceSensorDynamics.GetRepetitions();

		// 7 fill values for the GUI

		LogB.Information(string.Format(
					"readFile, printing forces, times.Count: {0}, forces.Count: {1}",
					times.Count, forces.Count));
		int i = 0;
		foreach(int time in times)
		{
			p_l.Add (new PointF (time, forces[i]));
			forceSensorValues.TimeLast = time;
			forceSensorValues.ValueLast = forces[i];
			forceSensorValues.SetMaxMinIfNeeded(forces[i], time);

			if(fse.ComputeAsElastic)
			{
				//pDist_l.Add (new PointF (time, Position_l[i]));
				forceSensorValuesDispl.TimeLast = time;
				forceSensorValuesDispl.ValueLast = Position_l[i];
				forceSensorValuesDispl.SetMaxMinIfNeeded(Position_l[i], time);
			}

			i ++;
		}
	}

	//gets an instant value
	public override double GetTimeMS (int count)
	{
		return p_l[count].X / 1000.0; //microseconds to milliseconds
	}

	public double GetTimeMicros(int count)
	{
		return p_l[count].X; //microseconds to milliseconds
	}

	public override double GetForceAtCount(int count)
	{
		return p_l[count].Y;
	}

	public override double CalculateRFD (int countA, int countB)
	{
		return ForceCalcs.GetRFD (p_l, countA, countB);
	}

	//calculates from a range
	public bool CalculateRangeParams (int countA, int countB, double maxAVGInWindowSeconds)
	{
		//countA will be the lowest and countB the highest to calcule Avg and max correctly no matter if B is before A
		if(countA > countB) {
			int temp = countA;
			countA = countB;
			countB = temp;
		}

		ForceCalcs.GetAverageAndMaxForce (p_l, countA, countB, out ForceAVG, out ForceMAX);
		gmaiw = new GetMaxAvgInWindow (p_l, countA, countB, maxAVGInWindowSeconds);

		List<PointF> pAB_l = new List<PointF>();
		for (int i = countA; i <= countB; i ++)
			pAB_l.Add (p_l[i]);

		briw = new GetBestRFDInWindow (pAB_l, 0, pAB_l.Count -1, 0.05); //50 ms
		briw.MaxSampleStart += countA;
		briw.MaxSampleEnd += countA;

		if(CalculedElasticPSAP)
		{
			ForceCalcs.GetAverageAndMaxForce (Speed_l, countA, countB, out SpeedAVG, out SpeedMAX);
			ForceCalcs.GetAverageAndMaxForce (Accel_l, countA, countB, out AccelAVG, out AccelMAX);
			ForceCalcs.GetAverageAndMaxForce (Power_l, countA, countB, out PowerAVG, out PowerMAX);
		}

		return true;
	}

	public double CalculateImpulse (int countA, int countB)
	{
		return ForceCalcs.GetImpulse (p_l, countA, countB);
	}

	/*
	 * Calculates RFD in a point using previous and next point
	 */
//TODO: fer que es vagi recordant el max en un rang determinat pq no s'hagi de tornar a calcular
// Note the method: GetBestRFDInWindow is different because it checks if it fits in a second
	public double LastRFDMax;
	public int LastRFDMaxCount;
	public void CalculateMaxRFDInRange(int countA, int countB)
	{
		double max = 0;
		double current = 0;
		int countRFDMax = countA; //count where maxRFD is found

		for(int i = countA; i < countB -2; i ++)
		{
			//current = fscAIPoints.GetRFD(i-1, i+1);
			current = CalculateRFD (i, i+2);
			if(current > max)
			{
				max = current;
				countRFDMax = i;
			}
		}

		//stored to read them from forceSensorAnalyze manual table and graph
		LastRFDMax = max;
		LastRFDMaxCount = countRFDMax;
	}

	public void CalculateVariabilityAndAccuracy (int countA, int countB,
			int feedbackF, Preferences.VariabilityMethodEnum variabilityMethod, int lag)
	{
		vaa = new VariabilityAndAccuracy ();
		vaa.Calculate (p_l, countA, countB, feedbackF, variabilityMethod, lag);
	}

	public void ExportToCSV(int countA, int countB, string selectedFileName, string sepString)
	{
		//this overwrites if needed
		TextWriter writer = File.CreateText(selectedFileName);

		string sep = " ";
		if (sepString == "COMMA")
			sep = ";";
		else
			sep = ",";

		//write header
		writer.WriteLine(exportCSVHeader(CalculedElasticPSAP, sep, true));

		//write statistics

		//1. difference
		writer.WriteLine(exportCSVDifference(CalculedElasticPSAP, sep, sepString, countA, countB));

		//2. average
		writer.WriteLine(exportCSVAverage(CalculedElasticPSAP, sep, sepString, countA, countB));

		//3. maximum
		writer.WriteLine(exportCSVMax(CalculedElasticPSAP, sep, sepString, countA, countB));

		//blank line
		writer.WriteLine();

		//write header again (for iterating data)
		writer.WriteLine(exportCSVHeader(CalculedElasticPSAP, sep, false));

		//write data
		for(int i = countA; i < countB -2; i ++)
			writer.WriteLine(exportCSVIteration(CalculedElasticPSAP, sep, sepString, i));

		writer.Flush();
		writer.Close();
		((IDisposable)writer).Dispose();
	}

	private string exportCSVHeader(bool elastic, string sep, bool headerTable)
	{
		string str;
		if(headerTable)
			str = Catalog.GetString("Statistics") + sep;
		else
			str = Catalog.GetString("Sample") + sep;

		str += Catalog.GetString("Repetition") + sep +
			Catalog.GetString("Time") + " (micros)" + sep +
			Catalog.GetString("Force") + " (N)" + sep +
			Catalog.GetString("RFD") + " (N/s)";

		if(elastic)
			str += sep + Catalog.GetString("Position") + " (m)"+ sep +
				Catalog.GetString("Speed") + " (m/s)" + sep +
				Catalog.GetString("Acceleration") + " (m/s^2)" + sep +
				Catalog.GetString("Power") + " (W)";

		return str;
	}

	private string exportCSVDifference(bool elastic, string sep, string sepString, int countA, int countB)
	{
		double timeA = GetTimeMicros(countA);
		double timeB = GetTimeMicros(countB);
		double forceA = GetForceAtCount(countA);
		double forceB = GetForceAtCount(countB);

		/* as A and B cannot calcule rfd because pre and post values do not exists, just show ""
		//double rfdA = CalculateRFD(countA -1, countA +1);
		//double rfdB = CalculateRFD(countB -1, countB +1);
		double rfdA = CalculateRFD (countA, countA);
		double rfdB = CalculateRFD (countB, countB);
		*/

		double timeDiff = timeB - timeA;
		double forceDiff =forceB - forceA;
		//double rfdDiff = rfdB - rfdA;

		string str = Catalog.GetString("Difference") + sep;

		str += "" + sep + 		//repetition
			Util.DoubleToCSV(timeDiff, 3, sepString) + sep +
			Util.DoubleToCSV(forceDiff, 3, sepString) + sep +
			//Util.DoubleToCSV(rfdDiff, 3, sepString);
			"";

		if(elastic)
			str += sep + Util.DoubleToCSV(Position_l[countB] - Position_l[countA], 3, sepString) + sep +
				Util.DoubleToCSV(Speed_l[countB] - Speed_l[countA], 3, sepString) + sep +
				Util.DoubleToCSV(Accel_l[countB] - Accel_l[countA], 3, sepString) + sep +
				Util.DoubleToCSV(Power_l[countB] - Power_l[countA], 3, sepString);

		return str;
	}

	private string exportCSVAverage(bool elastic, string sep, string sepString, int countA, int countB)
	{
		double rfdAVG = CalculateRFD (countA, countB);

		string str = Catalog.GetString("Average") + sep;

		str += "" + sep + 		//repetition
			"" + sep +
			Util.DoubleToCSV(ForceAVG, 3, sepString) + sep +
			Util.DoubleToCSV(rfdAVG, 3, sepString);

		if(elastic)
			str += sep + "" + sep + 	//position
				Util.DoubleToCSV(SpeedAVG, 3, sepString) + sep +
				Util.DoubleToCSV(AccelAVG, 3, sepString) + sep +
				Util.DoubleToCSV(PowerAVG, 3, sepString);

		return str;
	}

	private string exportCSVMax(bool elastic, string sep, string sepString, int countA, int countB)
	{
		CalculateMaxRFDInRange(countA, countB);
		double rfdMax = LastRFDMax;

		string str = Catalog.GetString("Maximum") + sep;

		str += "" + sep + 	 	//repetition
			"" + sep +
			Util.DoubleToCSV(ForceMAX, 3, sepString) + sep +
			Util.DoubleToCSV(rfdMax, 3, sepString);

		if(elastic)
			str += sep + "" + sep + 	//position
				Util.DoubleToCSV(SpeedMAX, 3, sepString) + sep +
				Util.DoubleToCSV(AccelMAX, 3, sepString) + sep +
				Util.DoubleToCSV(PowerMAX, 3, sepString);

		return str;
	}

	private string exportCSVIteration(bool elastic, string sep, string sepString, int i)
	{
		string str = (i+1).ToString() + sep; //sample

		//str += ForceSensorRepetition.GetRepetitionNumFromList(ForceSensorRepetition_l, i).ToString() + sep + 	//repetition
		str += ForceSensorRepetition.GetRepetitionCodeFromList(ForceSensorRepetition_l, i, fse.RepetitionsShow) + sep + 	//repetition
			Util.DoubleToCSV (p_l[i].X, sepString) + sep +
			Util.DoubleToCSV (p_l[i].Y, sepString) + sep +
			Util.DoubleToCSV (CalculateRFD (i, i+2), 3, sepString);

		if(elastic)
			str += sep + Util.DoubleToCSV(Position_l[i], 3, sepString) + sep +
				Util.DoubleToCSV(Speed_l[i], 3, sepString) + sep +
				Util.DoubleToCSV(Accel_l[i], 3, sepString) + sep +
				Util.DoubleToCSV(Power_l[i], 3, sepString);

		return str;
	}

	public GetMaxAvgInWindow Gmaiw
	{
		get { return gmaiw; }
	}

	public GetBestRFDInWindow Briw
	{
		get { return briw; }
	}

	public VariabilityAndAccuracy Vaa
	{
		get { return vaa; }
	}
}

public class SignalPointsCairo
{
	public List<PointF> Force_l;

	public SignalPointsCairo ()
	{
		Force_l = new List<PointF> ();
	}
}

public class SignalPointsCairoForceElastic : SignalPointsCairo
{
	public List<PointF> Displ_l;
	public List<PointF> Speed_l;
	public List<PointF> Accel_l;
	public List<PointF> Power_l;

	// regular constructor
	public SignalPointsCairoForceElastic ()
	{
		Force_l = new List<PointF> ();
		Displ_l = new List<PointF> ();
		Speed_l = new List<PointF> ();
		Accel_l = new List<PointF> ();
		Power_l = new List<PointF> ();
	}

	// constructor for:
	// - capture to send to graph a copy of the capturing set (and avoid thread problems), a, b is the full set
	// - zoom being a and b the aBeforeZoom and bBeforeZoom, for copying an area from the full set
	//
	// on capture vertical: , all this variables have XY transposed and there is a new variable: ForcePaintHoriz_l where XY is ok to calculate briw and miw
	public List<PointF> ForcePaintHoriz_l;

	//acts like a Clone
	public SignalPointsCairoForceElastic (SignalPointsCairoForceElastic spfe, int a, int b, bool horizontal)
	{
		Force_l = new List<PointF> ();
		Displ_l = new List<PointF> ();
		Speed_l = new List<PointF> ();
		Accel_l = new List<PointF> ();
		Power_l = new List<PointF> ();

		if (horizontal)
		{
			for (int i = a; i <= b; i ++)
			{
				//fixes problem on zoom two sets where a set is longer than the other
				if (i >= spfe.Force_l.Count)
					break;

				Force_l.Add (spfe.Force_l[i]);
				if (spfe.Displ_l != null && spfe.Displ_l.Count > 0)
					Displ_l.Add (spfe.Displ_l[i]);
				if (spfe.Speed_l != null && spfe.Speed_l.Count > 0)
					Speed_l.Add (spfe.Speed_l[i]);
				if (spfe.Accel_l != null && spfe.Accel_l.Count > 0)
					Accel_l.Add (spfe.Accel_l[i]);
				if (spfe.Power_l != null && spfe.Power_l.Count > 0)
					Power_l.Add (spfe.Power_l[i]);
			}
		} else {
			ForcePaintHoriz_l = new List<PointF> ();
			for (int i = a; i <= b; i ++)
			{
				//fixes problem on zoom two sets where a set is longer than the other
				if (i >= spfe.Force_l.Count)
					break;

				ForcePaintHoriz_l.Add (spfe.Force_l[i]);
				Force_l.Add (spfe.Force_l[i].Transpose ());
				if (spfe.Displ_l != null && spfe.Displ_l.Count > 0)
					Displ_l.Add (spfe.Displ_l[i].Transpose ());
				if (spfe.Speed_l != null && spfe.Speed_l.Count > 0)
					Speed_l.Add (spfe.Speed_l[i].Transpose ());
				if (spfe.Accel_l != null && spfe.Accel_l.Count > 0)
					Accel_l.Add (spfe.Accel_l[i].Transpose ());
				if (spfe.Power_l != null && spfe.Power_l.Count > 0)
					Power_l.Add (spfe.Power_l[i].Transpose ());
			}
		}
	}

	//public bool TimeShifted = false; unused

	//only used on analyze superpose spCairoFESend_CD, so no vertical
	public void ShiftMicros (int micros)
	{
		// TimeShifted = true; //unused

		for (int i = 0; i < Force_l.Count; i ++)
			Force_l[i].X += micros;
		//TODO: same for displ, ...
	}
}

//we need this class because we started using forcesensor without database (only text files)
public class ForceSensorLoadTryToAssignPersonAndMore
{
	private bool dbconOpened;
	private string filename; //filename comes without extension
	private int currentSessionID; //we get a person if already exists on that session
	public string Exercise;
	public string Laterality;
	public string Comment;

	public ForceSensorLoadTryToAssignPersonAndMore(bool dbconOpened, string filename, int currentSessionID)
	{
		this.dbconOpened = dbconOpened;
		this.filename = filename;
		this.currentSessionID = currentSessionID;

		Exercise = "";
		Laterality = "";
		Comment = "";
	}

	public Person GetPerson()
	{
		string personName = getNameAndMore();
		LogB.Information("getPerson: " + personName);
		if(personName == "")
			return new Person(-1);

		Person p = SqlitePerson.SelectByName(dbconOpened, personName);
		LogB.Information("person: " + p.ToString());
		if(SqlitePersonSession.PersonSelectExistsInSession(dbconOpened, p.UniqueID, currentSessionID))
			return p;

		return new Person(-1);
	}

	private string getNameAndMore()
	{
		/*
		 * 	there was a period were exercise param exists but can be captured without defining it,
		 * 	it was represented as:
		 * 	  personName__laterality_date_hour
		 * 	  personName__laterality_comment_date_hour
		 * 	  ...
		 * 	so fix this __ to:
		 * 	  personName_none_laterality_date_hour
		 * 	  personName_none_laterality_comment_date_hour
		 */

		LogB.Information("filename: " + filename);
		bool exerciseMissing = false;
		if(filename.IndexOf("__") != -1)
		{
			filename = filename.Replace("__", "_none_");
			exerciseMissing = true; //this will return "" as exercise
		}

		string [] strFull = filename.Split(new char[] {'_'});

		/*
		 * At 1.8.1-95 filename was: personName_date_hour
		 * Later filename was:
		 * 	personName_exercisename_laterality_date_hour
		 * 	or
		 * 	personName_exercisename_laterality_comment_date_hour
		 * 	note comment can have more _ so it can be
		 * 	personName_exercisename_laterality_mycomment_with_some_underscores_date_hour
		 *
		 * Since there was database (2019 Sept 6), the filename is:
		 * 	currentPerson.UniqueID + "_" + currentPerson.Name + "_" + UtilDate.ToFile(forceSensorTimeStartCapture);
		 * 	but this method is not called since that date, because there's no need to call: import_from_1_68_to_1_69()
		 */

		if(strFull.Length == 3)
		{
			/*
			Match match = Regex.Match(file.Name, @"\A(\d+_)");
			if(match.Groups.Count == 2)
			*/

			return strFull[0]; //personName_date_hour
		}
		else if(strFull.Length >= 5)
		{
			//strFull[1] is the exercise, but check that it existst on database
			if(! exerciseMissing && Sqlite.Exists(dbconOpened, Constants.ForceSensorExerciseTable, strFull[1]))
				Exercise = strFull[1];

			if(
					strFull[2] == Catalog.GetString(Constants.ForceSensorLateralityBoth) ||
					strFull[2] == Catalog.GetString(Constants.ForceSensorLateralityLeft) ||
					strFull[2] == Catalog.GetString(Constants.ForceSensorLateralityRight) )
				Laterality = strFull[2];

			if(strFull.Length == 6)
				Comment = strFull[3];
			else if(strFull.Length > 6) //comments with underscores
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

			return strFull[0];
		}

		return "";
	}
}

public class PathAccuracy
{
	//as public because they are used as ref, so cannot be properties
	public int CountIn;
	public int CountOut;

	//constructor
	public PathAccuracy ()
	{
		CountIn = 0;
		CountOut = 0;
	}

	public double Accuracy
	{
		get { return 100 * UtilAll.DivideSafe(CountIn, 1.0 * (CountIn + CountOut)); }
	}
}
