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
 *  Copyright (C) 2017-2021   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.IO; 		//for detect OS //TextWriter
using System.Collections; //ArrayList
using System.Collections.Generic; //List<T>
using System.Threading;
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

	private string exerciseName;

	//have a uniqueID -1 contructor, useful when set is deleted
	public ForceSensor()
	{
		uniqueID = -1;
	}

	//constructor
	public ForceSensor(int uniqueID, int personID, int sessionID, int exerciseID, CaptureOptions captureOption, int angle,
			string laterality, string filename, string url, string dateTime, string comments, string videoURL,
			double stiffness, string stiffnessString, string exerciseName)
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
			comments + "\", \"" + videoURL + "\", " + Util.ConvertToPoint(stiffness) + ", \"" + stiffnessString + "\")";
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
			"\" WHERE uniqueID = " + uniqueID;
	}

	public void UpdateSQLJustComments(bool dbconOpened)
	{
		SqliteForceSensor.UpdateComments (dbconOpened, uniqueID, comments); //SQL not opened
	}

	//for load window
	public string [] ToStringArray (int count)
	{
		int all = 9;
		string [] str = new String [all];
		int i=0;
		str[i++] = uniqueID.ToString();
		str[i++] = count.ToString();
		str[i++] = exerciseName;
		str[i++] = exerciseElasticStiffnessString();
		str[i++] = Catalog.GetString(GetCaptureOptionsString(captureOption));
		str[i++] = Catalog.GetString(laterality);
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

	//of a single point (this method will disappear)
	public static double CalculeForceResultantIfNeeded (double forceRaw, CaptureOptions fsco, ForceSensorExercise fse, double personMass)//, double stiffness)
	{
		if(! fse.ForceResultant)
			return calculeForceWithCaptureOptions(forceRaw, fsco);

		double totalMass = 0;
		if(fse.PercentBodyWeight > 0 && personMass > 0)
			totalMass = fse.PercentBodyWeight * personMass / 100.0;

		//right now only code for non-elastic
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

		LogB.Information("horiz: " + (Math.Cos(fse.AngleDefault * Math.PI / 180.0) * (forceRaw + totalMass * accel)).ToString());
		LogB.Information("vertical: " + (Math.Sin(fse.AngleDefault * Math.PI / 180.0) * (forceRaw + totalMass * accel) + totalMass * 9.81).ToString());
		*/
		//TODO: now we are using fse.AngleDefault, but we have to implement especific angle on capture

		double forceResultant = Math.Sqrt(
				Math.Pow(Math.Cos(fse.AngleDefault * Math.PI / 180.0) * (forceRaw + totalMass * accel),2) +                	//Horizontal component
				Math.Pow(Math.Sin(fse.AngleDefault * Math.PI / 180.0) * (forceRaw + totalMass * accel) + totalMass * 9.81,2) 	//Vertical component
				);

		return calculeForceWithCaptureOptions(forceResultant, fsco);
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
	private bool elastic;
	private bool eccReps;
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
		this.elastic = false;
	}

	public ForceSensorExercise(string name)
	{
		this.name = name;

		//default values
		this.forceResultant = false;
		this.elastic = false;
	}

	public ForceSensorExercise(int uniqueID, string name, int percentBodyWeight, string resistance, int angleDefault,
			string description, bool tareBeforeCapture, bool forceResultant, bool elastic,
			bool eccReps, double eccMin, double conMin)
	{
		this.uniqueID = uniqueID;
		this.name = name;
		this.percentBodyWeight = percentBodyWeight;
		this.resistance = resistance;
		this.angleDefault = angleDefault;
		this.description = description;
		this.tareBeforeCapture = tareBeforeCapture;
		this.forceResultant = forceResultant;
		this.elastic = elastic;
		this.eccReps = eccReps;
		this.eccMin = eccMin;
		this.conMin = conMin;
	}

	//constructor at DB: 1.86
	public ForceSensorExercise(int uniqueID, string name, int percentBodyWeight, string resistance, int angleDefault,
			string description, bool tareBeforeCapture, bool forceResultant, bool elastic)
	{
		this.uniqueID = uniqueID;
		this.name = name;
		this.percentBodyWeight = percentBodyWeight;
		this.resistance = resistance;
		this.angleDefault = angleDefault;
		this.description = description;
		this.tareBeforeCapture = tareBeforeCapture;
		this.forceResultant = forceResultant;
		this.elastic = elastic;
	}

	public override string ToString()
	{
		return uniqueID.ToString() + ":" + name + ":" + percentBodyWeight.ToString() + ":" +
			resistance + ":" + angleDefault.ToString() + ":" + description + ":" +
			tareBeforeCapture.ToString() + ":" + forceResultant.ToString() + ":" + elastic.ToString() + ":" +
			eccReps.ToString() + ":" + eccMin.ToString() + ":" + conMin.ToString();
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
			Util.BoolToInt(elastic).ToString() + ", " +
			Util.BoolToInt(eccReps).ToString() + ", " +
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

	public bool Changed(ForceSensorExercise newEx)
	{
		if(
				name == newEx.Name &&
				percentBodyWeight == newEx.PercentBodyWeight &&
				resistance == newEx.Resistance &&
				angleDefault == newEx.AngleDefault &&
				description == newEx.Description &&
				tareBeforeCapture == newEx.TareBeforeCapture &&
				forceResultant == newEx.ForceResultant &&
				elastic == newEx.Elastic &&
				eccReps == newEx.EccReps &&
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
	public bool TareBeforeCapture
	{
		get { return tareBeforeCapture; }
	}
	public bool ForceResultant
	{
		get { return forceResultant; }
	}
	public bool Elastic //TODO: take care because ComputeAsElastic is much better criteria
	{
		get { return elastic; }
	}
	public bool ComputeAsElastic //use this
	{
		get { return forceResultant && elastic; }
	}
	public bool EccReps
	{
		get { return eccReps; }
		set { eccReps = value; }
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

	public ForceSensorValues()
	{
		TimeLast = 0;
		TimeValueMax = 0;
		ValueLast = 0;
		Max = 0;
		Min = 10000;
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
/*
 * TODO: this class only contains points plot stuff
 * currently all the code relevant to force sensor actions is on gui/forcesensor.cs
 * that code should be here and there only the gui stuff
 */
//to manage force, but can also manage displ on elastic
public class ForceSensorCapturePoints
{
	//ForceCapturePoints stored to be realtime displayed
	public List<Gdk.Point> Points;
	public int NumCaptured;
	public int NumPainted;

	public enum GraphTypes { FORCESIGNAL, FORCEAIFORCE, FORCEAIDISPL }
	private GraphTypes graphType;

	//used to redo all points if change RealWidthG or RealHeightG
	private List<int> times;
	private List<double> forces;
	private double forceMax;
	private double forceMin;
	private int scrollStartedAtCount;

	public int RealWidthG; //width of graph in microseconds (will be upgraded if needed, but not while capturing on scroll)

	public const int DefaultRealHeightG = 2;
	public const int DefaultRealHeightGNeg = 2;
	public int RealHeightG; //Newtons (will be upgraded if needed)
	public int RealHeightGNeg; //Newtons (negative) (will be upgraded if needed)

	//for displacement
	public const int DefaultRealHeightGDispl = 1;
	public const int DefaultRealHeightGNegDispl = 0;

	private int widthG;
	private int heightG;
	private int marginLeft = 45; //px
	private int marginRight = 40; //px
	private int marginTop = 30; //px
	private int marginBottom = 30; //px

	//initialize
	public ForceSensorCapturePoints(GraphTypes graphType, int widthG, int heightG, int widthInSeconds)
	{
		Points = new List<Gdk.Point>();
		NumCaptured = 0;
		NumPainted = 0; 	//-1 means delete screen
		times = new List<int>();
		forces = new List<double>();
		forceMax = 0;
		forceMin = 10000;
		scrollStartedAtCount = -1;

		InitRealWidthHeight(widthInSeconds);

		this.graphType = graphType;
		this.widthG = widthG;
		this.heightG = heightG;
	}

	public void InitRealWidthHeight(int widthInSeconds)
	{
		//width of graph in microseconds (will be upgraded if needed)
		if(widthInSeconds == -1)
			RealWidthG = 10000000; //default 10 seconds
		else
			RealWidthG = 1000000 * widthInSeconds;

		RealHeightG = DefaultRealHeightG; //Newtons (will be upgraded when needed) (nice to see the +25 -25 marks)
		RealHeightGNeg = DefaultRealHeightGNeg; //Newtons (will be upgraded when needed) (nice to see the +25 -25 marks)

		if(graphType == GraphTypes.FORCEAIDISPL)
		{
			RealHeightG = DefaultRealHeightGDispl;
			RealHeightGNeg = DefaultRealHeightGNegDispl;
		}
	}

	public void Add(int time, double force)
	{
		times.Add(time);
		forces.Add(force);
		Points.Add(new Gdk.Point(GetTimeInPx(time), GetForceInPx(force)));

		if(scrollStartedAtCount == -1 && scrollStarted())
			scrollStartedAtCount = GetLength();

		if(force > forceMax)
			forceMax = force;
		if(force < forceMin)
			forceMin = force;
	}

	private bool scrollStarted()
	{
		return (GetLastTime() > .9 * RealWidthG); //90% of screen
	}

	/*
	 * unused and not checked if it is ok
	private int getPxInTime(int px)
	{
		//without 1.0 calculation is done as int producing very buggy value
		return Convert.ToInt32(
				(px * RealWidthG) / (1.0 * (widthG -marginLeft -marginRight) * time))
			- marginLeft;
	}
	*/

	public int GetTimeInPx(int time)
	{
		//without 1.0 calculation is done as int producing very buggy value
		return marginLeft + Convert.ToInt32(1.0 * (widthG -marginLeft -marginRight) * time / RealWidthG);
	}

	public int GetForceInPx(double force)
	{
		/*
		 * simmetrical positive / negative
		return Convert.ToInt32(
				(heightG/2)
				- ( UtilAll.DivideSafe((force * heightG), (1.0 * RealHeightG)) )
				);
				*/
		return Convert.ToInt32(
				heightG
				- UtilAll.DivideSafe(
						(force * (heightG - (marginTop + marginBottom))),
						(1.0 * (RealHeightG + RealHeightGNeg))
						)
				- UtilAll.DivideSafe(
						RealHeightGNeg * (heightG - (marginTop + marginBottom)),
						(1.0 * (RealHeightG + RealHeightGNeg))
						)
				- marginBottom
				);
	}

	private Gdk.Point getLastPoint()
	{
		return Points[Points.Count -1];
	}

	// TODO: do all this in an inherited class
	public int GetLength()
	{
		return times.Count;
	}

	public int GetLastTime()
	{
		return times[times.Count -1];
	}

	public double GetLastForce()
	{
		return forces[forces.Count -1];
	}

	public double GetTimeAtCount(int count)
	{
		//LogB.Information(string.Format("At GetTimeAtCount, count:{0}, times.Count:{1}", count, times.Count));

		//safe check
		if(count < 0)
			return times[0];
		else if (count >= times.Count)
			return times[times.Count -1];

		return times[count];
	}
	public double GetForceAtCount(int count)
	{
		return forces[count];
	}

	public void GetAverageAndMaxForce(int countA, int countB, out double avg, out double max)
	{
		if(countA == countB) {
			avg = forces[countA];
			max = forces[countA];
			return;
		}

		double sum = 0;
		max = 0;
		for(int i = countA; i <= countB; i ++) {
			sum += forces[i];
			if(forces[i] > max)
				max = forces[i];
		}

		avg = sum / ((countB - countA) +1);
	}


	//stored, to not calculate again with same data
	CalculatedForceMaxAvgInWindow calculatedForceMaxAvgInWindow;

	public void GetForceMaxAvgInWindow (int countA, int countB, double windowSeconds,
			out double avgMax, out int avgMaxSampleStart, out int avgMaxSampleEnd, out string error)
	{
		// 1) check if ws calculated before
		if(calculatedForceMaxAvgInWindow != null &&
				calculatedForceMaxAvgInWindow.InputsToString() ==
				new CalculatedForceMaxAvgInWindow(countA, countB, windowSeconds).InputsToString())
		{
			LogB.Information("Was calculated before");
			avgMax = calculatedForceMaxAvgInWindow.Result;
			avgMaxSampleStart = calculatedForceMaxAvgInWindow.ResultSampleStart;
			avgMaxSampleEnd = calculatedForceMaxAvgInWindow.ResultSampleEnd;
			error = ""; //there will be no error, because when is stored is without error
			return;
		}

		// 2) check if countB - countA fits in window time
		double timeA = GetTimeAtCount(countA);
		
		if(GetTimeAtCount(countB) - timeA <= 1000000 * windowSeconds)
		{
			avgMax = 0;
			avgMaxSampleStart = countA; //there is an error, this will not be used
			avgMaxSampleEnd = countA; //there is an error, this will not be used
			error = "Need more time";
			return;
		}

		avgMax = 0;
		avgMaxSampleStart = countA; 	//sample where avgMax starts (to draw a line)
		avgMaxSampleEnd = countA; 	//sample where avgMax starts (to draw a line)
		error = "";

		double sum = 0;
		int count = 0;

		//note if countB - countA < 1s then can have higher values than all the set
		// 3) get the first second (or whatever in windowSeconds)
		int i;
		for(i = countA; i <= countB && GetTimeAtCount(i) - timeA <= 1000000 * windowSeconds; i ++)
		{
			sum += forces[i];
			count ++;
		}
		avgMax = sum / count;
		avgMaxSampleEnd = countA + count;

		LogB.Information(string.Format("avgMax 1st for: {0}", avgMax));
		//note "count" has the window size in samples

		// 4) continue until the end (countB)
		for(int j = i; j < countB; j ++)
		{
			sum -= forces[j - count];
			sum += forces[j];

			double avg = sum / count;
			if(avg > avgMax)
			{
				avgMax = avg;
				avgMaxSampleStart = j - count;
				avgMaxSampleEnd = j;
			}
		}

		LogB.Information(string.Format("Average max force in {0} seconds: {1}, started at sample range: {2}:{3}",
					windowSeconds, avgMax, avgMaxSampleStart, avgMaxSampleEnd));

		// 5) store data to not calculate it again if data is the same
		calculatedForceMaxAvgInWindow = new CalculatedForceMaxAvgInWindow (
				countA, countB, windowSeconds, avgMax, avgMaxSampleStart, avgMaxSampleEnd);
	}

	public double GetRFD(int countA, int countB)
	{
		double calc = (forces[countB] - forces[countA]) / (times[countB]/1000000.0 - times[countA]/1000000.0); //microsec to sec
		//LogB.Information(string.Format("GetRFD {0}, {1}, {2}, {3}, {4}, {5}, RFD: {6}",
		//			countA, countB, forces[countA], forces[countB], times[countA], times[countB], calc));
		return calc;
	}
	public double GetImpulse(int countA, int countB)
	{
		double sum = 0;
		int samples = 0;
		for(int i = countA; i <= countB; i ++)
		{
			sum += forces[i];
			samples ++;
		}

		double elapsedSeconds = times[countB]/1000000.0 - times[countA]/1000000.0;
		return sum * UtilAll.DivideSafe(elapsedSeconds, samples);
	}
	
	public void GetVariabilityAndAccuracy(int countA, int countB, int feedbackF, out double variability, out double feedbackDifference, bool cvRMSSD)
	{
		if(countA == countB)
		{
			variability = 0;
			feedbackDifference = 0;
			return;
		}

		// 1) calculate numSamples. Note countA and countB are included, so
		//countA = 2; countB = 4; samples are: 2,3,4; 3 samples
		int numSamples = (countB - countA) + 1;

		// 2) get variability
		if(cvRMSSD)
			variability = getVariabilityCVRMSSD (countA, countB, numSamples);
		else
			variability = getVariabilityOldMethod (countA, countB, numSamples);

		// 3) Calculate difference.
		// Average of the differences between force and average
		//feedbackDifference = Math.Abs(feedbackF - avg);
		double sum = 0;
		for(int i = countA; i <= countB; i ++)
		{
			sum += Math.Abs(forces[i]-feedbackF);
		}

		feedbackDifference = UtilAll.DivideSafe(sum, numSamples);
	}
	private double getVariabilityCVRMSSD (int countA, int countB, int numSamples)
	{
		//Σ(x_i - x_{i+1})^2 /(n-1))   //note pow should be inside the summation
		double sum = 0;
		for(int i = countA; i < countB; i ++)
		{
			sum += Math.Pow(forces[i] - forces[i+1], 2);
		}

		return UtilAll.DivideSafe(sum, numSamples -1);

	}
	private double getVariabilityOldMethod (int countA, int countB, int numSamples)
	{
		// 1) get average
		double sum = 0;
		for(int i = countA; i <= countB; i ++)
			sum += forces[i];

		double avg = sum / numSamples;

		// 2) Average of the differences between force and average
		sum = 0;
		for(int i = countA; i <= countB; i ++)
			sum += Math.Abs(forces[i]-avg);

		return UtilAll.DivideSafe(sum, numSamples);
	}

	public int MarginLeft
	{
		get { return marginLeft; }
	}
	public int MarginRight
	{
		get { return marginRight; }
	}
	// TODO: end of... do all this in an inherited class


	// this is called while capturing, checks if last captured value is outside the graph
	public bool OutsideGraphChangeValues (bool checkOnlyY)
	{
		Gdk.Point p = getLastPoint();
		//LogB.Information("p.Y: " + p.Y + "; heightG: " +  heightG);
		bool outsideGraph = false;

		if(! checkOnlyY && p.X > widthG)
		{
			RealWidthG *= 2;
			outsideGraph = true;
		}

		if(p.Y < marginTop)
		{
			//too drastic change that makes DrawingArea empty at capture
			//RealHeightG *= 2;
			RealHeightG += 20;
			outsideGraph = true;
		}
		else if(p.Y > heightG - marginBottom)
		{
			//too drastic change that makes DrawingArea empty at capture
			//RealHeightGNeg *= 2;
			RealHeightGNeg += 20;
			outsideGraph = true;
		}

		return outsideGraph;
	}
	// this is called at load signal, checks if last X is outside the graph and max/min force
	// mustChangeRealWidthG makes easier to sync width on Force and displacement
	public bool OutsideGraphChangeValues (int lastTime, double maxForce, double minForce, bool mustChangeRealWidthG)
	{
		/*
		LogB.Information(string.Format("At OutsideGraph: graphType: {0}, maxForce {1}, minForce: {2}, " +
					" forceInPx(maxForce): {3}, forceInPx(minForce): {4}",
					graphType, maxForce, minForce, GetForceInPx(maxForce), GetForceInPx(minForce)));
		LogB.Information(string.Format("conditions: {0}, {1}, {2}, {3}",
					lastTime + GetTimeInPx(marginLeft) + GetTimeInPx(marginRight),
					RealWidthG, GetForceInPx(minForce) > heightG, GetForceInPx(maxForce) < 0));
					*/

		bool change = false;
		if(lastTime + GetTimeInPx(marginLeft) + GetTimeInPx(marginRight) > RealWidthG ||
				GetForceInPx(minForce) > heightG ||
				GetForceInPx(maxForce) < 0 ||
				GetForceInPx(maxForce) == GetForceInPx(minForce)
				)
		{
			RealHeightG = Convert.ToInt32(Math.Ceiling(maxForce)); //Math.Ceiling to ensure the displ will fit

			//RealHeightGNeg = Convert.ToInt32(Math.Abs(minForce));
			if(minForce < 0)
				RealHeightGNeg = Convert.ToInt32(Math.Ceiling(Math.Abs(minForce)));
			else
				RealHeightGNeg = 0;

			change = true;
		}

		if(change || mustChangeRealWidthG)
		{
			RealWidthG = lastTime + GetTimeInPx(marginLeft) + GetTimeInPx(marginRight);
			//LogB.Information(string.Format("OutsideGraph: {0}, {1}, {2}, {3}",
			//			RealWidthG, lastTime, GetTimeInPx(marginLeft), GetTimeInPx(marginRight)));
		}

		return (change || mustChangeRealWidthG);
	}

	public void Zoom(int lastTime) //on zoom adjust width
	{
		LogB.Information("At Zoom with graphType: " + graphType.ToString());
		//X
		RealWidthG = lastTime + GetTimeInPx(marginLeft) + GetTimeInPx(marginRight);

		//Y
		RealHeightG = Convert.ToInt32(forceMax);
		if(forceMin < 0)
			RealHeightGNeg = Convert.ToInt32(Math.Abs(forceMin));
		else
			RealHeightGNeg = -1 * Convert.ToInt32(forceMin);

		//LogB.Information(string.Format("RealHeightG: {0}; RealHeightGNeg: {1}", RealHeightG, RealHeightGNeg));
	}

	public void Redo()
	{
		for(int i=0; i < NumCaptured; i ++)
			Points[i] = new Gdk.Point(GetTimeInPx(times[i]), GetForceInPx(forces[i]));
	}

	public int WidthG
	{
		get { return widthG; }
		set { widthG = value; }
	}

	public int HeightG
	{
		set { heightG = value; }
	}

	public double ForceMax
	{
		get { return forceMax; }
	}
	public double ForceMin
	{
		get { return forceMin; }
	}
	public int ScrollStartedAtCount
	{
		get { return scrollStartedAtCount; }
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
	public enum Types { INSTANTANEOUS, AVERAGE, PERCENT_F_MAX, RFD_MAX, IMP_UNTIL_PERCENT_F_MAX, IMP_RANGE } //on SQL is inserted like this
	private static string type_INSTANTANEOUS_name = "Instantaneous";
	private static string type_AVERAGE_name = "Average";
	private static string type_PERCENT_F_MAX_name = "% Force max";
	private static string type_RFD_MAX_name = "RFD max";

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
				Catalog.GetString(type_PERCENT_F_MAX_name), Catalog.GetString(type_RFD_MAX_name)
			};
		else
			return new string [] {
				type_INSTANTANEOUS_name, type_AVERAGE_name, type_PERCENT_F_MAX_name, type_RFD_MAX_name
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
		if(type == Types.INSTANTANEOUS) {
			if(translated)
				return Catalog.GetString(type_INSTANTANEOUS_name);
			else
				return type_INSTANTANEOUS_name;
		}
		else if(type == Types.AVERAGE) {
			if(translated)
				return Catalog.GetString(type_AVERAGE_name);
			else
				return type_AVERAGE_name;
		}
		else if(type == Types.PERCENT_F_MAX) {
			if(translated)
				return Catalog.GetString(type_PERCENT_F_MAX_name);
			else
				return type_PERCENT_F_MAX_name;
		}
		else { //if(type == Types.RFD_MAX)
			if(translated)
				return Catalog.GetString(type_RFD_MAX_name);
			else
				return type_RFD_MAX_name;
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
			string fullURL, bool decimalIsPoint, double maxForceRaw,
			double maxAvgForceInWindow, double forceSensorAnalyzeMaxAVGInWindowSeconds,
			double maxAvgForceInWindowSampleStart, double maxAvgForceInWindowSampleEnd,
			string laterality, int setCount, int repCount, string commentOfSet,
			ForceSensor.CaptureOptions fsco, int startSample, int endSample,
			string title, string exercise, string date, string time, TriggerList triggerList)
	{
		assignParams(fsco, startSample, endSample, title, exercise, date, time, triggerList);

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

		return fullURL + ";" +
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
			Util.RemoveChar(commentOfSet, ';');  //TODO: check this really removes
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
		return triggerList.ToRCurvesString(type3);
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

public class ForceSensorAnalyzeInstant
{
	public double ForceAVG;
	public double ForceMAX;
	public double SpeedAVG;
	public double SpeedMAX;
	public double AccelAVG;
	public double AccelMAX;
	public double PowerAVG;
	public double PowerMAX;

	public double ForceMaxAvgInWindow; 		//the result
	public int ForceMaxAvgInWindowSampleStart;	//the start sample of the result
	public int ForceMaxAvgInWindowSampleEnd;	//the end sample of the result
	public string ForceMaxAvgInWindowError; 	//if there is any error

	//for elastic
	public bool CalculedElasticPSAP;
	public List<double> Position_l;
	public List<double> Speed_l;
	public List<double> Accel_l;
	public List<double> Power_l;
	public List<ForceSensorRepetition> ForceSensorRepetition_l;

	private ForceSensorCapturePoints fscAIPoints; //Analyze Instant
	private ForceSensorValues forceSensorValues;

	private ForceSensorCapturePoints fscAIPointsDispl; //Analyze Instant only on elastic
	private ForceSensorValues forceSensorValuesDispl; //this class can be used for force, displ, or whatever

	private int graphWidth;
	private int graphHeight;
	private ForceSensorExercise fse;

	public ForceSensorAnalyzeInstant(
			string file, int graphWidth, int graphHeight, int startSample, int endSample,
			ForceSensorExercise fse, double personWeight, ForceSensor.CaptureOptions fsco, double stiffness,
			double eccMinDisplacement, double conMinDisplacement)
	{
		this.graphWidth = graphWidth;
		this.graphHeight = graphHeight;
		this.fse = fse;

		readFile(file, startSample, endSample, personWeight, fsco, stiffness, eccMinDisplacement, conMinDisplacement);

		//on zoom adjust width
		if(startSample >= 0 || endSample >= 0)
		{
			fscAIPoints.Zoom(forceSensorValues.TimeLast);
			LogB.Information("Redo normal at constructor");
			fscAIPoints.Redo();

			if(fse.ComputeAsElastic) {
				fscAIPointsDispl.Zoom(forceSensorValuesDispl.TimeLast);
				LogB.Information("Redo elastic at constructor");
				fscAIPointsDispl.Redo();
			}
		}

		//ensure points fit on display
		if(fscAIPoints.OutsideGraphChangeValues (forceSensorValues.TimeLast, forceSensorValues.Max, forceSensorValues.Min, false))
		{
			LogB.Information("Redo normal at constructor b");
			fscAIPoints.Redo();
		}

		if(fse.ComputeAsElastic)
		{
			//LogB.Information(string.Format("fscAiPointsDispl.GetLastTime: {0}, forceSensorValuesDispl.TimeLast: {1}, forceSensorValuesDispl.Max: {2}, forceSensorValuesDispl.Min: {3}", 
			//			fscAIPointsDispl.GetLastTime(), forceSensorValuesDispl.TimeLast, forceSensorValuesDispl.Max, forceSensorValuesDispl.Min));

			if(fscAIPointsDispl.RealWidthG != fscAIPoints.RealWidthG) {
				fscAIPointsDispl.RealWidthG = fscAIPoints.RealWidthG;
				if(fscAIPointsDispl.OutsideGraphChangeValues(forceSensorValuesDispl.TimeLast, forceSensorValuesDispl.Max, forceSensorValuesDispl.Min, true))
					fscAIPointsDispl.Redo();
			}

			if(fscAIPointsDispl.OutsideGraphChangeValues(forceSensorValuesDispl.TimeLast, forceSensorValuesDispl.Max, forceSensorValuesDispl.Min, false))
				fscAIPointsDispl.Redo();
		}
	}

	private void readFile(string file, int startSample, int endSample,
			double personWeight, ForceSensor.CaptureOptions fsco, double stiffness,
			double eccMinDisplacement, double conMinDisplacement)
	{
		LogB.Information(string.Format("at readFile, startSample: {0}, endSample: {1}", startSample, endSample));

		// 0 initialize

		fscAIPoints = new ForceSensorCapturePoints(ForceSensorCapturePoints.GraphTypes.FORCEAIFORCE, graphWidth, graphHeight, -1);
		if(fse.ComputeAsElastic)
			fscAIPointsDispl = new ForceSensorCapturePoints(ForceSensorCapturePoints.GraphTypes.FORCEAIDISPL, graphWidth, graphHeight, -1);

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
			forceSensorDynamics.CutSamplesForZoom(startSample, endSample); //this takes in account the RemoveNValues
			times = times.GetRange(startSample, endSample - startSample + 1);
		}


		// 5 shift times to the left (make first one zero)

		int startMsInt = times[0];
		for(int j = 0;  j < times.Count; j ++)
			times[j] -= startMsInt;

		forces = forceSensorDynamics.GetForces();

		// 6 get caculated data

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
			fscAIPoints.Add(time, forces[i]);
			fscAIPoints.NumCaptured ++;

			forceSensorValues.TimeLast = time;
			forceSensorValues.ValueLast = forces[i];
			forceSensorValues.SetMaxMinIfNeeded(forces[i], time);

			if(fse.ComputeAsElastic)
			{
				fscAIPointsDispl.Add(time, Position_l[i]);
				fscAIPointsDispl.NumCaptured ++;

				forceSensorValuesDispl.TimeLast = time;
				forceSensorValuesDispl.ValueLast = Position_l[i];
				forceSensorValuesDispl.SetMaxMinIfNeeded(Position_l[i], time);
			}

			i ++;
		}
	}

	//When B checkbutton is clicked or window is resized
	public void RedoGraph(int graphWidth, int graphHeight)
	{
		this.graphWidth = graphWidth;
		this.graphHeight = graphHeight;
		fscAIPoints.WidthG = graphWidth;
		fscAIPoints.HeightG = graphHeight;

		LogB.Information("RedoGraph normal at RedoGraph");
		fscAIPoints.Redo();

		if(fse.ComputeAsElastic) {
			fscAIPointsDispl.WidthG = graphWidth;
			fscAIPointsDispl.HeightG = graphHeight;
			LogB.Information("RedoGraph displ elastic at RedoGraph");
			fscAIPointsDispl.Redo();
		}
	}

	//gets an instant value
	public double GetTimeMS(int count)
	{
		return fscAIPoints.GetTimeAtCount(count) / 1000.0; //microseconds to milliseconds
	}
	public double GetForceAtCount(int count)
	{
		return fscAIPoints.GetForceAtCount(count);
	}

	public int GetLength()
	{
		//LogB.Information("GetLength: " + fscAIPoints.GetLength());
		return fscAIPoints.GetLength();
	}

	//public int GetXFromSampleCount(int currentPos, int totalPos)
	public int GetXFromSampleCount(int currentPos)
	{
		//LogB.Information(string.Format("currentPos: {0}", currentPos));
		//this can be called on expose event before calculating needed parameters
		if(graphWidth == 0)
			return 0;

		int leftMargin = fscAIPoints.MarginLeft;
		int rightMargin = fscAIPoints.MarginRight;

		/*
		 * note samples don't come at same time separation, so this does not work:
		double px = UtilAll.DivideSafe(
				(graphWidth - leftMargin - rightMargin) * currentPos,
				totalPos -1); //-1 ok
				//fscAIPoints.RealWidthG);
		*/
		//get the time of sample
		double currentTime = fscAIPoints.GetTimeAtCount(currentPos);
		double lastTime = fscAIPoints.GetLastTime();

		double px = UtilAll.DivideSafe(
				(graphWidth - leftMargin - rightMargin) * currentTime,
				lastTime);

		// fix margin
		//px = px + plt.x1 * graphWidth;
		px = px + leftMargin;

		return Convert.ToInt32(px);
	}

	public int GetPxAtForce(double f)
	{
		return fscAIPoints.GetForceInPx(f);
	}
	public int GetPxAtDispl(double f)
	{
		return fscAIPointsDispl.GetForceInPx(f);
	}

	//calculates from a range
	public bool CalculateRangeParams(int countA, int countB, double forceSensorAnalyzeMaxAVGInWindowSeconds)
	{
		//countA will be the lowest and countB the highest to calcule Avg and max correctly no matter if B is before A
		if(countA > countB) {
			int temp = countA;
			countA = countB;
			countB = temp;
		}

		fscAIPoints.GetAverageAndMaxForce(countA, countB, out ForceAVG, out ForceMAX);
		fscAIPoints.GetForceMaxAvgInWindow (countA, countB, forceSensorAnalyzeMaxAVGInWindowSeconds,
				out ForceMaxAvgInWindow, out ForceMaxAvgInWindowSampleStart, out ForceMaxAvgInWindowSampleEnd,
				out ForceMaxAvgInWindowError);

		if(CalculedElasticPSAP)
		{
			calculeElasticPSAPAveragesAndMax(countA, countB, Speed_l, out SpeedAVG, out SpeedMAX);
			calculeElasticPSAPAveragesAndMax(countA, countB, Accel_l, out AccelAVG, out AccelMAX);
			calculeElasticPSAPAveragesAndMax(countA, countB, Power_l, out PowerAVG, out PowerMAX);
		}

		return true;
	}

	private void calculeElasticPSAPAveragesAndMax(int countA, int countB, List<double> list, out double avg, out double max)
	{
		if(countA == countB) {
			avg = list[countA];
			max = list[countA];
			return;
		}

		double sum = 0;
		max = 0;
		for(int i = countA; i <= countB; i ++) {
			sum += list[i];
			if(list[i] > max)
				max = list[i];
		}

		avg = sum / ((countB - countA) +1);
	}

	public double CalculateRFD(int countA, int countB)
	{
		return fscAIPoints.GetRFD(countA, countB);
	}

	public double CalculateImpulse(int countA, int countB)
	{
		return fscAIPoints.GetImpulse(countA, countB);
	}

	public void CalculateVariabilityAndAccuracy(int countA, int countB,
			int feedbackF, out double variability, out double feedbackDifference, bool cvrmssd)
	{
		fscAIPoints.GetVariabilityAndAccuracy(countA, countB, feedbackF, out variability, out feedbackDifference, cvrmssd);
	}
	/*
	 * Calculates RFD in a point using previous and next point
	 */
//TODO: fer que es vagi recordant el max en un rang determinat pq no s'hagi de tornar a calcular
	public double LastRFDMax;
	public int LastRFDMaxCount;
	public void CalculateMaxRFDInRange(int countA, int countB)
	{
		double max = 0;
		double current = 0;
		int countRFDMax = countA; //count where maxRFD is found

		for(int i = countA; i < countB; i ++)
		{
			current = fscAIPoints.GetRFD(i-1, i+1);
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

	/* this method is not working
	public int CalculateXOfTangentLine(int x0, int y0, double RFD, int y, int height)
	{
	*/
		/*
		 * x0 and y0 are coordinates of RFD point
		 * RFD is the RFD value
		 * x is the returned value for an x value
		 * height is used to transform the y's in order to make following formula work
		 *
		 * y = RFD * x + y0 - x0*RFD
		 * y - y0 + x0*RFD = x*RFD
		 * x = (y - y0 + x0*RFD) / RFD
		 */
	/*
		y0 = height - y0;
		y = height -y;

		return Convert.ToInt32(UtilAll.DivideSafe(y - y0 + x0*RFD, RFD));
	}
	*/

	public void CalculateRFDTangentLine(int countRFDMax, out int lineXStart, out int lineXEnd, out int lineYStart, out int lineYEnd)
	{
		LogB.Information(string.Format("CalculateRFDTangentLine: {0}" , countRFDMax));

		// 1) calculate X and Y of points before and after RFD
		int pointXBefore = GetXFromSampleCount(countRFDMax -1);
		int pointXAfter = GetXFromSampleCount(countRFDMax +1);
		int pointYBefore = GetPxAtForce(GetForceAtCount(countRFDMax -1));
		int pointYAfter = GetPxAtForce(GetForceAtCount(countRFDMax +1));

		// 2) calculate the slope of the line that could pass across this points
		double slope = Math.Abs( UtilAll.DivideSafe( pointYAfter - pointYBefore,
					(1.0 * (pointXAfter- pointXBefore)) ) );

		// 3) get the RFD point
		int pointXRFD = GetXFromSampleCount(countRFDMax);
		int pointYRFD = GetPxAtForce(GetForceAtCount(countRFDMax));

		// 4) calculate line that cross RFD point with calculated slope
		lineXStart = pointXRFD - Convert.ToInt32(UtilAll.DivideSafe(
					(graphHeight - pointYRFD),
					slope));
		lineXEnd = pointXRFD + Convert.ToInt32(UtilAll.DivideSafe(
					(pointYRFD - 0),
					slope));
		lineYStart = graphHeight;
		lineYEnd = 0;
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
		for(int i = countA; i <= countB; i ++)
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
			Catalog.GetString("Time") + " (ms)" + sep +
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
		double timeA = GetTimeMS(countA);
		double timeB = GetTimeMS(countB);
		double forceA = GetForceAtCount(countA);
		double forceB = GetForceAtCount(countB);
		double rfdA = CalculateRFD(countA -1, countA +1);
		double rfdB = CalculateRFD(countB -1, countB +1);
		double timeDiff = timeB - timeA;
		double forceDiff =forceB - forceA;
		double rfdDiff = rfdB - rfdA;

		string str = Catalog.GetString("Difference") + sep;

		str += "" + sep + 		//repetition
			Util.DoubleToCSV(timeDiff, 3, sepString) + sep +
			Util.DoubleToCSV(forceDiff, 3, sepString) + sep +
			Util.DoubleToCSV(rfdDiff, 3, sepString);

		if(elastic)
			str += sep + Util.DoubleToCSV(Position_l[countB] - Position_l[countA], 3, sepString) + sep +
				Util.DoubleToCSV(Speed_l[countB] - Speed_l[countA], 3, sepString) + sep +
				Util.DoubleToCSV(Accel_l[countB] - Accel_l[countA], 3, sepString) + sep +
				Util.DoubleToCSV(Power_l[countB] - Power_l[countA], 3, sepString);

		return str;
	}

	private string exportCSVAverage(bool elastic, string sep, string sepString, int countA, int countB)
	{
		double rfdAVG = CalculateRFD(countA, countB);

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
		double timeAtCount = fscAIPoints.GetTimeAtCount(i);

		string str = (i+1).ToString() + sep; //sample

		//str += ForceSensorRepetition.GetRepetitionNumFromList(ForceSensorRepetition_l, i).ToString() + sep + 	//repetition
		str += ForceSensorRepetition.GetRepetitionCodeFromList(ForceSensorRepetition_l, i, fse.EccReps) + sep + 	//repetition
			Util.DoubleToCSV(timeAtCount, sepString) + sep +
			Util.DoubleToCSV(fscAIPoints.GetForceAtCount(i), sepString) + sep +
			Util.DoubleToCSV(CalculateRFD(i-1, i+1), 3, sepString);

		if(elastic)
			str += sep + Util.DoubleToCSV(Position_l[i], 3, sepString) + sep +
				Util.DoubleToCSV(Speed_l[i], 3, sepString) + sep +
				Util.DoubleToCSV(Accel_l[i], 3, sepString) + sep +
				Util.DoubleToCSV(Power_l[i], 3, sepString);

		return str;
	}

	public ForceSensorCapturePoints FscAIPoints
	{
		get { return fscAIPoints; }
	}
	public ForceSensorCapturePoints FscAIPointsDispl
	{
		get { return fscAIPointsDispl; }
	}
}

//to count sets according to person and exercise
public class ForceSensorExportSet
{
	public int pID; //personID
	public int exID; //forceSensor exercise ID
	public string lat; //laterality
	public int count; //how many sets with this pID && exID

	public ForceSensorExportSet (int pID, int exID, string lat)
	{
		this.pID = pID;
		this.exID = exID;
		this.lat = lat;
		this.count = 0;
	}

	public bool IsEqual (int pID, int exID, string lat)
	{
		return (this.pID == pID && this.exID == exID && this.lat == lat);
	}
}
public class ForceSensorExportSetManage
{
	List<ForceSensorExportSet> l;

	public ForceSensorExportSetManage()
	{
		l = new List<ForceSensorExportSet>();
	}

	public bool Exists (int pID, int exID, string lat)
	{
		foreach(ForceSensorExportSet fses in l)
			if(fses.IsEqual(pID, exID, lat))
				return true;

		return false;
	}

	public void AddForceSensorExportSet (int pID, int exID, string lat)
	{
		ForceSensorExportSet fses = new ForceSensorExportSet(pID, exID, lat);
		l.Add(fses);
	}

	public void AddSet (int pID, int exID, string lat)
	{
		foreach(ForceSensorExportSet fses in l)
			if(fses.IsEqual(pID, exID, lat))
				fses.count ++;
	}

	public int GetCount (int pID, int exID, string lat)
	{
		foreach(ForceSensorExportSet fses in l)
			if(fses.IsEqual(pID, exID, lat))
				return fses.count;

		return -1;
	}
}

public class ForceSensorExport
{
	public Gtk.Button Button_done;

	//passed variables
	private Gtk.Notebook notebook;
	private Gtk.ProgressBar progressbar;
	private Gtk.Label labelResult;
	private bool includeImages;
	private int imageWidth;
	private int imageHeight;
	private string exportURL; //folder or .csv depending on includeImages
	private bool isWindows;
	private int personID; // -1: all
	private int sessionID;
	private List<ForceSensorRFD> rfdList;
	private ForceSensorImpulse impulse;
	private double duration;
	private int durationPercent;
	private double forceSensorElasticEccMinDispl;
	private int forceSensorNotElasticEccMinForce;
	private double forceSensorElasticConMinDispl;
	private int forceSensorNotElasticConMinForce;
	private bool forceSensorStartEndOptimized;
	private char CSVExportDecimalSeparatorChar;
	private double forceSensorAnalyzeMaxAVGInWindowSeconds;

	private static Thread thread;
	private static bool cancel;
	private static bool noData;
	private static bool cannotCopy;
	private static string messageToProgressbar;
	//private static double pulseFraction; unused because its managed on pulse, better because on thread is working 100% on R call

	List<ForceSensor> fs_l;
	ArrayList personSession_l;
	ArrayList fsEx_l;
	private static int totalRepsToExport;
	List<string> exportedRFDs;

	//constructor
	public ForceSensorExport (
			Gtk.Notebook notebook,
			Gtk.ProgressBar progressbar,
			Gtk.Label labelResult,
			bool includeImages,
			int imageWidth, int imageHeight,
			bool isWindows, int personID, int sessionID,
			List<ForceSensorRFD> rfdList, ForceSensorImpulse impulse,
			double duration, int durationPercent,
			double forceSensorElasticEccMinDispl,
			int forceSensorNotElasticEccMinForce,
			double forceSensorElasticConMinDispl,
			int forceSensorNotElasticConMinForce,
			bool forceSensorStartEndOptimized,
			char CSVExportDecimalSeparatorChar,
			double forceSensorAnalyzeMaxAVGInWindowSeconds)

	{
		this.notebook = notebook;
		this.progressbar = progressbar;
		this.labelResult = labelResult;
		this.includeImages = includeImages;
		this.imageWidth = imageWidth;
		this.imageHeight = imageHeight;
		this.isWindows = isWindows;
		this.personID = personID;
		this.sessionID = sessionID;
		this.rfdList = rfdList;
		this.impulse = impulse;
		this.duration = duration;
		this.durationPercent = durationPercent;
		this.forceSensorElasticEccMinDispl = forceSensorElasticEccMinDispl;
		this.forceSensorNotElasticEccMinForce = forceSensorNotElasticEccMinForce;
		this.forceSensorElasticConMinDispl = forceSensorElasticConMinDispl;
		this.forceSensorNotElasticConMinForce = forceSensorNotElasticConMinForce;
		this.forceSensorStartEndOptimized = forceSensorStartEndOptimized;
		this.CSVExportDecimalSeparatorChar = CSVExportDecimalSeparatorChar;
		this.forceSensorAnalyzeMaxAVGInWindowSeconds = forceSensorAnalyzeMaxAVGInWindowSeconds;

		Button_done = new Gtk.Button();
	}

	///public method
	public void Start(string exportURL)
	{
		this.exportURL = exportURL;

		//create progressbar and graph files dirs or delete their contents
		createOrEmptyDir(Util.GetForceSensorTempProgressDir());
		createOrEmptyDir(Util.GetForceSensorTempGraphsDir());
		createOrEmptyDir(Util.GetForceSensorTempGraphsABDir());

		cancel = false;
		noData = false;
		cannotCopy = false;
		progressbar.Fraction = 0;
		messageToProgressbar = "";
		notebook.CurrentPage = 1;

		thread = new Thread (new ThreadStart (forceSensorExportDo));
		GLib.Idle.Add (new GLib.IdleHandler (pulseForceSensorExportGTK));
		thread.Start();
	}

	private void createOrEmptyDir(string dir)
	{
		if( ! Directory.Exists(dir))
			Directory.CreateDirectory (dir);
		else {
			DirectoryInfo dirInfo = new DirectoryInfo(dir);
			foreach (FileInfo file in dirInfo.GetFiles())
				file.Delete();
		}
	}

	public void Cancel()
	{
		cancel = true;
	}

	private bool pulseForceSensorExportGTK ()
	{
		if(! thread.IsAlive || cancel)
		{
			if(cancel)
				LogB.Information("pulseForceSensorExportGTK cancelled");

			LogB.Information("pulseForceSensorExportGTK ending here");
			LogB.ThreadEnded();

			progressbar.Fraction = 1;
			notebook.CurrentPage = 0;

			if(cancel)
				labelResult.Text = Catalog.GetString("Cancelled.");
			else if (noData)
				labelResult.Text = Catalog.GetString("Missing data.");
			else if (cannotCopy)
				labelResult.Text = string.Format(Catalog.GetString("Cannot copy to {0} "), exportURL);
			else
				labelResult.Text = string.Format(Catalog.GetString("Exported to {0}"), exportURL);// +
						//Constants.GetSpreadsheetString(CSVExportDecimalSeparator)
						//);

			Button_done.Click();

			return false;
		}

		DirectoryInfo dirInfo = new DirectoryInfo(Util.GetForceSensorTempProgressDir());
		//LogB.Information(string.Format("pulse files: {0}", dirInfo.GetFiles().Length));

		int files = dirInfo.GetFiles().Length;
		if(files == 0) {
			progressbar.Text = messageToProgressbar;
			progressbar.Pulse();
		} else {
			progressbar.Text = string.Format(Catalog.GetString("Exporting repetition {0}/{1}"), files, totalRepsToExport);
			progressbar.Fraction = UtilAll.DivideSafeFraction(files, totalRepsToExport);
		}

		Thread.Sleep (100);
		//Log.Write(" (pulseForceSensorExportGTK:" + thread.ThreadState.ToString() + ") ");
		return true;
	}

	private void forceSensorExportDo()
	{
		getData();

		if(fs_l.Count == 0)
		{
			LogB.Information("There's no data");
			noData = true;
			return;
		}

//		if(processForceSensorSets()) //false if cancelled
//			writeFile();
		processForceSensorSets();
	}

	private void getData ()
	{
		fs_l = SqliteForceSensor.Select(false, -1, personID, sessionID);
		personSession_l = SqlitePersonSession.SelectCurrentSessionPersons(sessionID, true);
		fsEx_l = SqliteForceSensorExercise.Select (false, -1, false);
		exportedRFDs = new List<string>();
		totalRepsToExport = 0;
	}

	private bool processForceSensorSets ()
	{
		Person p = new Person();
		PersonSession ps = new PersonSession();

		List<ForceSensorGraphABExport> fsgABe_l = new List<ForceSensorGraphABExport>();

		//to manage sets we need previousPerson and previousExercise
		ForceSensorExportSetManage fsesm = new ForceSensorExportSetManage();

		int count = 1;
		foreach(ForceSensor fs in fs_l)
		{
			messageToProgressbar = string.Format(Catalog.GetString("Preparing sets {0}/{1}"), count++, fs_l.Count);

			if(cancel)
				return false;

			// 1) checks
			//check fs is ok
			if(fs == null || ! Util.FileExists(fs.FullURL))
				continue;

			//check fs has data
			List<string> contents = Util.ReadFileAsStringList(fs.FullURL);
			if(contents.Count < 3)
			{
				//new DialogMessage(Constants.MessageTypes.WARNING, Constants.FileEmptyStr());
				//return;
				continue;
			}

			// 2) get the person
			bool found = false;
			foreach(PersonAndPS paps in personSession_l)
			{
				if(paps.p.UniqueID == fs.PersonID)
				{
					p = paps.p;
					ps = paps.ps;

					found = true;
					break;
				}
			}
			if(! found)
				continue;

			// 3) get the exercise
			found = false;
			ForceSensorExercise fsEx = new ForceSensorExercise();
			foreach(ForceSensorExercise fsExTemp in fsEx_l)
				if(fsExTemp.UniqueID == fs.ExerciseID)
				{
					fsEx = fsExTemp;
					found = true;
					break;
				}
			if(! found)
				continue;

			if(! fsesm.Exists(p.UniqueID, fsEx.UniqueID, fs.Laterality))
				fsesm.AddForceSensorExportSet(p.UniqueID, fsEx.UniqueID, fs.Laterality);

			//make the exercise have EccReps = true in order to have an AB wiht the concentric and eccentric part
			//and send both to R to be able to have the force window in that AB
			fsEx.EccReps = true;

			double eccMinDispl = fsEx.GetEccOrConMinMaybePreferences(true,
					forceSensorElasticEccMinDispl,
					forceSensorNotElasticEccMinForce);
			double conMinDispl = fsEx.GetEccOrConMinMaybePreferences(false,
					forceSensorElasticConMinDispl,
					forceSensorNotElasticConMinForce);

			// 4) create fsAI (includes the repetitions)
			ForceSensorAnalyzeInstant fsAI = new ForceSensorAnalyzeInstant(
					fs.FullURL,
					imageWidth, imageHeight,
					-1, -1,
					fsEx, ps.Weight,
					fs.CaptureOption, fs.Stiffness,
					eccMinDispl, conMinDispl
					);

			// 5) call R
			string title = p.Name;
			string exercise = fsEx.Name;
			if (isWindows) {
				title = Util.ConvertToUnicode(title);
				exercise = Util.ConvertToUnicode(exercise);
			}
			if (title == null || title == "")
				title = "unnamed";


			string destination = UtilEncoder.GetmifCSVInputMulti();
			Util.FileDelete(destination);


			/*
			//copy file to tmp to be written readed by R
			File.Copy(fs.FullURL, UtilEncoder.GetmifCSVFileName(), true); //can be overwritten
			*/

			//delete result file
			Util.FileDelete(UtilEncoder.GetmifExportFileName());

			bool addedSet = false;
			int repCount = 1;
			int repConcentricSampleStart = -1;
			bool lastIsCon = false;
			ForceSensorRepetition repLast = null;
			foreach(ForceSensorRepetition rep in fsAI.ForceSensorRepetition_l)
			{
				if(rep.type == ForceSensorRepetition.Types.CON)
				{
					repConcentricSampleStart = rep.sampleStart;
					repLast = rep;
					lastIsCon = true;
				}
				else if(rep.type == ForceSensorRepetition.Types.ECC && repConcentricSampleStart != -1)
				{
					double maxAvgForceInWindow = 0;
					double maxAvgForceInWindowSampleStart = 0;
					double maxAvgForceInWindowSampleEnd = 0;
					bool success = fsAI.CalculateRangeParams(repConcentricSampleStart, rep.sampleEnd,
							forceSensorAnalyzeMaxAVGInWindowSeconds);
					if(success) {
						maxAvgForceInWindow = fsAI.ForceMaxAvgInWindow;
						maxAvgForceInWindowSampleStart = fsAI.ForceMaxAvgInWindowSampleStart;
						maxAvgForceInWindowSampleEnd = fsAI.ForceMaxAvgInWindowSampleEnd;
					}

					if(! addedSet) {
						fsesm.AddSet(p.UniqueID, fsEx.UniqueID, fs.Laterality);
						addedSet = true;
					}
					fsgABe_l.Add(new ForceSensorGraphABExport (
								fs.FullURL,
								Util.CSVDecimalColumnIsPoint(fs.FullURL, 1),
								fsAI.ForceMAX,			//raw
								maxAvgForceInWindow,		//raw
								forceSensorAnalyzeMaxAVGInWindowSeconds, //raw
								maxAvgForceInWindowSampleStart,	//the start sample of the result
								maxAvgForceInWindowSampleEnd,	//the end sample of the result
								fs.Laterality,
								fsesm.GetCount(p.UniqueID, fsEx.UniqueID, fs.Laterality),
								repCount ++,
								fs.Comments,
								fs.CaptureOption,
								repConcentricSampleStart, 	//start of concentric rep
								rep.sampleEnd,			//end of eccentric rep
								title, exercise, fs.DatePublic, fs.TimePublic, new TriggerList()
								));

					lastIsCon = false;
				}
			}

			/*
			 *1 if the last rep is con, also send to R (no problem if there is no ending ecc phase)
			 *2 if we have not found any rep on this set, just pass from A to B on the set.
				This happens eg if the person starts with the maximum force (or using the forceSensor to weight things)
				or where the test has no force increase at all.
			*/
			if(
					(lastIsCon && repLast != null) 		// *1
					||
					(repCount == 1 && ! lastIsCon) 		// *2
					)
			{
				//if (repCount == 1 && ! lastIsCon) { 		// *2
					int sampleA = 1;
					int sampleB = fsAI.GetLength() -1;
				//}
				if(lastIsCon && repLast != null) { 		// *1
					sampleA = repConcentricSampleStart; //start of concentric rep
					sampleB = repLast.sampleEnd; 	//end of eccentric rep
				}

				double maxAvgForceInWindow = 0;
				double maxAvgForceInWindowSampleStart = 0;
				double maxAvgForceInWindowSampleEnd = 0;
				bool success = fsAI.CalculateRangeParams(sampleA, sampleB,
						forceSensorAnalyzeMaxAVGInWindowSeconds);
				if(success) {
					maxAvgForceInWindow = fsAI.ForceMaxAvgInWindow;
					maxAvgForceInWindowSampleStart = fsAI.ForceMaxAvgInWindowSampleStart;
					maxAvgForceInWindowSampleEnd = fsAI.ForceMaxAvgInWindowSampleEnd;
				}

				if(! addedSet) {
					fsesm.AddSet(p.UniqueID, fsEx.UniqueID, fs.Laterality);
					addedSet = true;
				}
				fsgABe_l.Add(new ForceSensorGraphABExport (
							fs.FullURL,
							Util.CSVDecimalColumnIsPoint(fs.FullURL, 1),
							fsAI.ForceMAX,			//raw
							maxAvgForceInWindow,		//raw
							forceSensorAnalyzeMaxAVGInWindowSeconds, //raw
							maxAvgForceInWindowSampleStart,	//the start sample of the result
							maxAvgForceInWindowSampleEnd,	//the end sample of the result
							fs.Laterality,
							fsesm.GetCount(p.UniqueID, fsEx.UniqueID, fs.Laterality),
							repCount ++,
							fs.Comments,
							fs.CaptureOption,
							sampleA,
							sampleB,
							title, exercise, fs.DatePublic, fs.TimePublic, new TriggerList()
							));
			}

			//TODO: or check cancel when there is a thread, also R should write something blank if there is any problem
			//also the problem with this code is: if R code fails for any reason (bad data), will exit R code and this file will never be created
/*
			// 6) write exportedRFDs (includes impulse)
			if(File.Exists(UtilEncoder.GetmifExportFileName()))
			{
				List<string> repRFDs = Util.ReadFileAsStringList(UtilEncoder.GetmifExportFileName());
				int countRep = 0; //0 will be the header
				foreach(string row in repRFDs)
				{
					//discard header
					if(countRep == 0)
					{
						countRep ++;
						continue;
					}
					//TODO: respect latin/not latin (decimal character, column separator)
					exportedRFDs.Add(p.Name + ";" + fs.DateTimePublic + ";" +
							fsEx.Name + ";" + (countRep++).ToString() + ";" + fs.Laterality + ";" +
							row);
				}
			}
			pulseFraction = UtilAll.DivideSafeFraction (count ++, fs_l.Count);
*/
		}

		if(fsgABe_l.Count > 0)
		{
			totalRepsToExport = fsgABe_l.Count;
			ForceSensorGraph fsg = new ForceSensorGraph(
					rfdList, impulse,
					duration, durationPercent,
					forceSensorStartEndOptimized,
					true, //not used to read data, but used to print data
					CSVExportDecimalSeparatorChar, // at write file
					fsgABe_l,
					forceSensorAnalyzeMaxAVGInWindowSeconds,
					includeImages
					);

			bool success = fsg.CallR(imageWidth, imageHeight, false);
		}

		LogB.Information("Waiting creation of file... ");
		while ( ! ( Util.FileReadable(UtilEncoder.GetmifExportFileName()) || cancel ) )
			;

		if(cancel)
			return false;

		if(includeImages)
		{
			LogB.Information("going to copy export files with images ...");
			if( ! Directory.Exists(exportURL))
                                Directory.CreateDirectory (exportURL);

			try{
				// 1) rfd graphs
				string sourceFolder = Path.Combine(Path.GetTempPath(), "chronojump_mif_graphs_rfd");
				DirectoryInfo sourceDirInfo = new DirectoryInfo(sourceFolder);

				string destFolder = Path.Combine(exportURL, "chronojump_mif_graphs_rfd");
				Directory.CreateDirectory (destFolder);

				foreach (FileInfo file in sourceDirInfo.GetFiles())
					file.CopyTo(destFolder, true);

				// 2) AB graphs
				sourceFolder = Path.Combine(Path.GetTempPath(), "chronojump_mif_graphs_ab");
				sourceDirInfo = new DirectoryInfo(sourceFolder);

				destFolder = Path.Combine(exportURL, "chronojump_mif_graphs_ab");
				Directory.CreateDirectory (destFolder);

				foreach (FileInfo file in sourceDirInfo.GetFiles())
					file.CopyTo(destFolder, true);
			} catch {
				return false;
			}

			//LogB.Information("done copy export files with images!");
		}

		//copy the CSV
		File.Copy(UtilEncoder.GetmifExportFileName(), exportURL, true);

		return true;
	}

	/*
	//this exports the csv... it should be done by R
	private bool writeFile()
	{
		string destination = UtilEncoder.GetmifExportFileName();
		Util.FileDelete(destination);
		try {
			//this overwrites if needed
			TextWriter writer = File.CreateText(destination);

			string sep = " ";
			if (CSVExportDecimalSeparator == "COMMA")
				sep = ";";
			else
				sep = ",";

			//write header
			string [] headers = { "Person", "Datetime", "Exercise", "Repetition", "Laterality", "Fmax"};
			int i = 0;
			foreach(ForceSensorRFD rfd in rfdList)
			{
				if(rfdList[i].Active)
					headers = Util.AddArrayString(headers,
							string.Format("RFD{0}", i+1) + "_" + rfd.ToExport(true, "_"),
							false);
				i ++;
			}
			if(impulse.Active)
				headers = Util.AddArrayString(headers,
						Catalog.GetString("Impulse") + "_" + impulse.ToExport(true, "_"),
						false);

			writer.WriteLine(Util.RemoveNewLine(Util.StringArrayToString(headers, sep), false));

			//write data
			foreach(string str in exportedRFDs)
				writer.WriteLine(str);

			writer.Flush();
			writer.Close();
			((IDisposable)writer).Dispose();

			return true;
		} catch {
			string myString = string.Format(
					Catalog.GetString("Cannot save file {0} "), destination);
			//new DialogMessage(Constants.MessageTypes.WARNING, myString);
			LogB.Information("Error: " + myString);

			return false;
		}
	}
	*/
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
