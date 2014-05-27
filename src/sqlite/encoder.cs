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
 * Copyright (C) 2004-2012   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using System.IO;
using System.Collections; //ArrayList
using Mono.Data.Sqlite;


class SqliteEncoder : Sqlite
{
	public SqliteEncoder() {
	}
	
	~SqliteEncoder() {}

	/*
	 * create and initialize tables
	 */
	
	protected internal static void createTableEncoder()
	{
		dbcmd.CommandText = 
			"CREATE TABLE " + Constants.EncoderTable + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"personID INT, " +
			"sessionID INT, " +
			"exerciseID INT, " +
			"eccon TEXT, " +	//"c" or "ec"
			"laterality TEXT, " +	//"left" "right" "both"
			"extraWeight TEXT, " +	//string because can contain "33%" or "50Kg"
			"signalOrCurve TEXT, " + //"signal" or "curve"
			"filename TEXT, " +
			"url TEXT, " +
			"time INT, " +
			"minHeight INT, " +
			"description TEXT, " +
			"status TEXT, " +	//"active", "inactive"
			"videoURL TEXT, " +	//URL of video of signals
			"encoderConfiguration TEXT, " +	//text separated by ':'
		       	"future1 TEXT, " +	//Since 1.4.4 (DB 1.06) this stores last meanPower detected on a curve 
						//(as string with '.' because future1 was created as TEXT)
		       	"future2 TEXT, " + 
		       	"future3 TEXT )";
		dbcmd.ExecuteNonQuery();
	}
	
	/*
	 * Encoder class methods
	 */
	
	public static int Insert(bool dbconOpened, EncoderSQL es)
	{
		if(! dbconOpened)
			dbcon.Open();

		if(es.uniqueID == "-1")
			es.uniqueID = "NULL";

		dbcmd.CommandText = "INSERT INTO " + Constants.EncoderTable +  
			" (uniqueID, personID, sessionID, exerciseID, eccon, laterality, extraWeight, " + 
			"signalOrCurve, filename, url, time, minHeight, description, status, " +
			"videoURL, encoderConfiguration, future1, future2, future3)" +
			" VALUES (" + es.uniqueID + ", " +
			es.personID + ", " + es.sessionID + ", " +
			es.exerciseID + ", '" + es.eccon + "', '" +
			es.laterality + "', '" + es.extraWeight + "', '" +
			es.signalOrCurve + "', '" + es.filename + "', '" +
			es.url + "', " + es.time + ", " + es.minHeight + ", '" + es.description + 
			"', '" + es.status + "', '" + es.videoURL + "', '" + 
			es.encoderConfiguration.ToString(":",true) + "', '" + 
			Util.ConvertToPoint(es.future1) + "', '" + es.future2 + "', '" + es.future3 + "')";
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		//int myLast = dbcon.LastInsertRowId;
		//http://stackoverflow.com/questions/4341178/getting-the-last-insert-id-with-sqlite-net-in-c
		string myString = @"select last_insert_rowid()";
		dbcmd.CommandText = myString;
		int myLast = Convert.ToInt32(dbcmd.ExecuteScalar()); // Need to type-cast since `ExecuteScalar` returns an object.

		if(! dbconOpened)
			dbcon.Close();

		return myLast;
	}
	
	public static void Update(bool dbconOpened, EncoderSQL es)
	{
		if(! dbconOpened)
			dbcon.Open();

		if(es.uniqueID == "-1")
			es.uniqueID = "NULL";

		dbcmd.CommandText = "UPDATE " + Constants.EncoderTable + " SET " +
				" personID = " + es.personID +
				", sessionID = " + es.sessionID +
				", exerciseID = " + es.exerciseID +
				", eccon = '" + es.eccon +
				"', laterality = '" + es.laterality +
				"', extraWeight = '" + es.extraWeight +
				"', signalOrCurve = '" + es.signalOrCurve +
				"', filename = '" + es.filename +
				"', url = '" + es.url +
				"', time = " + es.time +
				", minHeight = " + es.minHeight +
				", description = '" + es.description + 
				"', status = '" + es.status + 
				"', videoURL = '" + es.videoURL + 
				"', encoderConfiguration = '" + es.encoderConfiguration.ToString(":",true) + 
				"', future1 = '" + Util.ConvertToPoint(es.future1) + 
				"', future2 = '" + es.future2 + 
				"', future3 = '" + es.future3 + 
				"' WHERE uniqueID == " + es.uniqueID ;

		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		if(! dbconOpened)
			dbcon.Close();
	}
	
	//pass uniqueID value and then will return one record. do like this:
	//EncoderSQL eSQL = (EncoderSQL) SqliteEncoder.Select(false, myUniqueID, 0, 0, "", false, true)[0];
	//don't care for the 0, 0 , because selection will be based on the myUniqueID and only one row will be returned
	//or
	//pass uniqueID==-1 and personID, sessionID, signalOrCurve values, and will return some records
	//personID can be -1 to get all on that session
	//sessionID can be -1 to get all sessions
	//signalOrCurve can be "all"
	
	//orderIDascendent is good for all the situations except when we want to convert from 1.05 to 1.06
	//in that conversion, we want first the last ones, and later the previous
	//	(to delete them if they are old copies)
	public static ArrayList Select (bool dbconOpened, 
			int uniqueID, int personID, int sessionID, string signalOrCurve, 
			bool onlyActive, bool orderIDascendent)
	{
		if(! dbconOpened)
			dbcon.Open();

		string personIDStr = "";
		if(personID != -1)
			personIDStr = " personID = " + personID + " AND ";

		string sessionIDStr = "";
		if(sessionID != -1)
			sessionIDStr = " sessionID = " + sessionID + " AND ";

		string selectStr = "";
		if(uniqueID != -1)
			selectStr = Constants.EncoderTable + ".uniqueID = " + uniqueID;
		else {
			if(signalOrCurve == "all")
				selectStr = personIDStr + sessionIDStr;
			else
				selectStr = personIDStr + sessionIDStr + " signalOrCurve = '" + signalOrCurve + "'";
		}

		string andString = "";
		if(selectStr != "")
			andString = " AND ";

		string onlyActiveString = "";
		if(onlyActive)
			onlyActiveString = " AND " + Constants.EncoderTable + ".status = 'active' ";

		string orderIDstr = "";
		if(! orderIDascendent)
			orderIDstr = " DESC";

		dbcmd.CommandText = "SELECT " + 
			Constants.EncoderTable + ".*, " + Constants.EncoderExerciseTable + ".name FROM " + 
			Constants.EncoderTable  + ", " + Constants.EncoderExerciseTable  + 
			" WHERE " + selectStr +
			andString + Constants.EncoderTable + ".exerciseID = " + 
				Constants.EncoderExerciseTable + ".uniqueID " +
				onlyActiveString +
			" ORDER BY substr(filename,-23,19), " + //'filename,-23,19' has the date of capture signal
			"uniqueID " + orderIDstr; 

		Log.WriteLine(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList array = new ArrayList(1);

		EncoderSQL es = new EncoderSQL();
		while(reader.Read()) {
			string [] strFull = reader[15].ToString().Split(new char[] {':'});
			EncoderConfiguration econf = new EncoderConfiguration(
				(Constants.EncoderConfigurationNames) 
				Enum.Parse(typeof(Constants.EncoderConfigurationNames), strFull[0]) );
			econf.FromSQL(strFull);
			
			//Log.WriteLine(econf.ToString(":", true));
			es = new EncoderSQL (
					reader[0].ToString(),			//uniqueID
					Convert.ToInt32(reader[1].ToString()),	//personID	
					Convert.ToInt32(reader[2].ToString()),	//sessionID
					Convert.ToInt32(reader[3].ToString()),	//exerciseID
					reader[4].ToString(),			//eccon
					reader[5].ToString(),			//laterality
					reader[6].ToString(),			//extraWeight
					reader[7].ToString(),			//signalOrCurve
					reader[8].ToString(),			//filename
					reader[9].ToString(),			//url
					Convert.ToInt32(reader[10].ToString()),	//time
					Convert.ToInt32(reader[11].ToString()),	//minHeight
					reader[12].ToString(),			//description
					reader[13].ToString(),			//status
					reader[14].ToString(),			//videoURL
					econf,					//encoderConfiguration
					Util.ChangeDecimalSeparator(reader[16].ToString()),	//future1 (meanPower on curves)
					reader[17].ToString(),			//future2
					reader[18].ToString(),			//future3
					reader[19].ToString()			//EncoderExercise.name
					);
			array.Add (es);
		}
		reader.Close();
		if(! dbconOpened)
			dbcon.Close();

		return array;
	}
	

	public static ArrayList SelectCompareIntersession (bool dbconOpened, int personID)
	{
		if(! dbconOpened)
			dbcon.Open();

		/* OLD, returns a row for active and a row for inactive at each session	
		dbcmd.CommandText = 
			"SELECT count(*), encoder.sessionID, session.name, session.date, encoder.status " +
			" FROM encoder, session, person77 " +
			" WHERE encoder.personID == " + personID + " AND signalOrCurve == 'curve' AND " + 
			" encoder.personID == person77.uniqueID AND encoder.sessionID == session.uniqueID " + 
			" GROUP BY encoder.sessionID, encoder.status ORDER BY encoder.sessionID, encoder.status";
			*/

		//returns a row for each session where there are active or inactive
		dbcmd.CommandText = 
			"SELECT encoder.sessionID, session.name, session.date, " +
			" SUM(CASE WHEN encoder.status = 'active' THEN 1 END) as active, " +
			" SUM(CASE WHEN encoder.status = 'inactive' THEN 1 END) as inactive " + 
			" FROM encoder, session, person77 " +
			" WHERE encoder.personID == " + personID + " AND signalOrCurve == 'curve' AND " +
			" encoder.personID == person77.uniqueID AND encoder.sessionID == session.uniqueID " +
			" GROUP BY encoder.sessionID ORDER BY encoder.sessionID, encoder.status";
	
		Log.WriteLine(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList array = new ArrayList();
		EncoderPersonCurvesInDB encPS = new EncoderPersonCurvesInDB();
		while(reader.Read()) {
			int active = 0;
			string activeStr = reader[3].ToString();
			if(Util.IsNumber(activeStr, false))
				active = Convert.ToInt32(activeStr);
			
			int inactive = 0;
			string inactiveStr = reader[4].ToString();
			if(Util.IsNumber(inactiveStr, false))
				inactive = Convert.ToInt32(inactiveStr);


			encPS = new EncoderPersonCurvesInDB (
					personID,
					Convert.ToInt32(reader[0].ToString()),	//sessionID
					reader[1].ToString(),			//sessionName
					reader[2].ToString(),			//sessionDate
					active,					//active
					active + inactive			//all: active + inactive 
					);
			array.Add(encPS);
		}
		reader.Close();
		if(! dbconOpened)
			dbcon.Close();

		return array;
	}

	/*
	 * EncoderSignalCurve
	 */
	
	protected internal static void createTableEncoderSignalCurve()
	{
		dbcmd.CommandText = 
			"CREATE TABLE " + Constants.EncoderSignalCurveTable + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"signalID INT, " +
			"curveID INT, " +
			"msCentral INT, " +
		       	"future1 TEXT )";
		dbcmd.ExecuteNonQuery();
	}
	
	public static void SignalCurveInsert(bool dbconOpened, int signalID, int curveID, int msCentral)
	{
		if(! dbconOpened)
			dbcon.Open();

		dbcmd.CommandText = "INSERT INTO " + Constants.EncoderSignalCurveTable +  
			" (uniqueID, signalID, curveID, msCentral, future1) " + 
			"VALUES (NULL, " + signalID + ", " + curveID + ", " + msCentral + ", '')";
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		if(! dbconOpened)
			dbcon.Close();
	}
	

	//signalID == -1 (any signal)
	//curveID == -1 (any curve)
	//if msStart and msEnd != -1 (means find a curve with msCentral contained between both values)
	public static ArrayList SelectSignalCurve (bool dbconOpened, int signalID, int curveID, double msStart, double msEnd)
	{
		if(! dbconOpened)
			dbcon.Open();

		string whereStr = "";
		if(signalID != -1 || curveID != -1 || msStart != -1)
			whereStr = " WHERE ";
		
		string signalIDstr = "";
		if(signalID != -1)
			signalIDstr = " signalID == " + signalID;
		
		string curveIDstr = "";
		if(curveID != -1) {
			curveIDstr = " curveID == " + curveID;
			if(signalID != -1)
				curveIDstr = " AND" + curveIDstr;
		}

		string msCentralstr = "";
		if(msStart != -1) {
			msCentralstr = " msCentral >= " + Util.ConvertToPoint(msStart) + " AND msCentral <= " + Util.ConvertToPoint(msEnd);
			if(signalID != -1 || curveID != -1)
				msCentralstr = " AND" + msCentralstr;
		}

		dbcmd.CommandText = 
			"SELECT uniqueID, signalID, curveID, msCentral " +
			" FROM " + Constants.EncoderSignalCurveTable + 
			whereStr + signalIDstr + curveIDstr + msCentralstr;
		
		Log.WriteLine(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList array = new ArrayList();
		while(reader.Read()) {
			EncoderSignalCurve esc = new EncoderSignalCurve(
					Convert.ToInt32(reader[0].ToString()),
					Convert.ToInt32(reader[1].ToString()),
					Convert.ToInt32(reader[2].ToString()),
					Convert.ToInt32(reader[3].ToString()));
			
			array.Add(esc);
		}
		reader.Close();
		if(! dbconOpened)
			dbcon.Close();

		return array;
	}

	public static void DeleteSignalCurveWithCurveID(bool dbconOpened, int curveID)
	{
		if( ! dbconOpened)
			dbcon.Open();

		dbcmd.CommandText = "Delete FROM " + Constants.EncoderSignalCurveTable +
			" WHERE curveID == " + curveID.ToString();
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		if( ! dbconOpened)
			dbcon.Close();
	}

	

	/*
	 * EncoderExercise stuff
	 */
	
	
	//ressistance (weight bar, machine, goma, none, inertial, ...)
	protected internal static void createTableEncoderExercise()
	{
		dbcmd.CommandText = 
			"CREATE TABLE " + Constants.EncoderExerciseTable + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"name TEXT, " +
			"percentBodyWeight INT, " +
			"ressistance TEXT, " +
			"description TEXT, " +
			"future1 TEXT, " +	//speed1RM: speed in m/s at 1RM with decimal point separator '.' ; 0 means undefined
			"future2 TEXT, " +	//bodyAngle (unused)
			"future3 TEXT )";	//weightAngle (unused)
		dbcmd.ExecuteNonQuery();
	}
	
	public static void InsertExercise(bool dbconOpened, string name, int percentBodyWeight, 
			string ressistance, string description, string speed1RM)	 //speed1RM decimal point = '.'
	{
		if(! dbconOpened)
			dbcon.Open();

		dbcmd.CommandText = "INSERT INTO " + Constants.EncoderExerciseTable +  
				" (uniqueID, name, percentBodyWeight, ressistance, description, future1, future2, future3)" +
				" VALUES (NULL, '" + name + "', " + percentBodyWeight + ", '" + 
				ressistance + "', '" + description + "', '" + speed1RM + "', '', '')";
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		if(! dbconOpened)
			dbcon.Close();
	}

	//Note: if this names change, or there are new, change them on both:
	//gui/encoder createEncoderCombos();	
	//gui/encoder on_button_encoder_exercise_add_accepted (object o, EventArgs args) 
	protected internal static void initializeTableEncoderExercise()
	{
		string [] iniEncoderExercises = {
			//name:percentBodyWeight:ressistance:description:speed1RM:pullAngle:weightAngle
			"Bench press:0:weight bar::0.185::", //González-Badillo, J. 2010. Movement velocity as a measure of loading intensity in resistance training
			"Squat:100:weight bar::0.31::" //González-Badillo, JJ.2000b http://foro.chronojump.org/showthread.php?tid=1288&page=3 
		};
		
		foreach(string line in iniEncoderExercises) {
			string [] parts = line.Split(new char[] {':'});
			InsertExercise(true, parts[0], Convert.ToInt32(parts[1]), parts[2], parts[3], parts[4]);
		}

		addEncoderFreeExercise();
		addEncoderJumpExercise();
		addEncoderInclinatedExercises();
	}
	
	protected internal static void addEncoderFreeExercise()
	{
		bool exists = Sqlite.Exists (true, Constants.EncoderExerciseTable, "Free");
		if(! exists)
			InsertExercise(true, "Free", 0, "", "", "");
	}
	protected internal static void addEncoderJumpExercise()
	{
		bool exists = Sqlite.Exists (true, Constants.EncoderExerciseTable, "Jump");
		if(! exists)
			InsertExercise(true, "Jump", 100, "", "", "");
	}
	protected internal static void addEncoderInclinatedExercises()
	{
		string [] iniEncoderExercises = {
			//name:percentBodyWeight:ressistance:description:speed1RM:bodyAngle:weightAngle
			"Inclinated plane:0:machine::::",
			"Inclinated plane BW:100:machine::::",
		};
		
		foreach(string line in iniEncoderExercises) {
			string [] parts = line.Split(new char[] {':'});
			InsertExercise(true, parts[0], Convert.ToInt32(parts[1]), parts[2], parts[3], parts[4]);
		}
	}

	public static void UpdateExercise(bool dbconOpened, string name, int percentBodyWeight, 
			string ressistance, string description, string speed1RM)
	{
		if(! dbconOpened)
			dbcon.Open();

		dbcmd.CommandText = "UPDATE " + Constants.EncoderExerciseTable + " SET " +
				" percentBodyWeight = " + percentBodyWeight +
				", ressistance = '" + ressistance +
				"', description = '" + description +
				"', future1 = '" + speed1RM +
				"' WHERE name = '" + name + "'" ;

		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		if(! dbconOpened)
			dbcon.Close();
	}
	
	//if uniqueID != -1, returns an especific EncoderExercise that can be read like this	
	//EncoderExercise ex = (EncoderExercise) SqliteEncoder.SelectEncoderExercises(eSQL.exerciseID)[0];
	public static ArrayList SelectEncoderExercises(bool dbconOpened, int uniqueID, bool onlyNames) 
	{
		if(! dbconOpened)
			dbcon.Open();

		string uniqueIDStr = "";
		if(uniqueID != -1)
			uniqueIDStr = " WHERE " + Constants.EncoderExerciseTable + ".uniqueID = " + uniqueID;
	
		if(onlyNames)
			dbcmd.CommandText = "SELECT name FROM " + Constants.EncoderExerciseTable + uniqueIDStr;
		else
			dbcmd.CommandText = "SELECT * FROM " + Constants.EncoderExerciseTable + uniqueIDStr;
		
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		
		ArrayList array = new ArrayList(1);
		EncoderExercise ex = new EncoderExercise();
		
		if(onlyNames) {
			while(reader.Read()) {
				ex = new EncoderExercise (reader[0].ToString());
				array.Add(ex);
			}
		} else {
			while(reader.Read()) {
				double speed1RM = 0;
			       	if(reader[5].ToString() != "")
					speed1RM = Convert.ToDouble(Util.ChangeDecimalSeparator(reader[5].ToString()));
				
				ex = new EncoderExercise (
						Convert.ToInt32(reader[0].ToString()),	//uniqueID
						reader[1].ToString(),			//name
						Convert.ToInt32(reader[2].ToString()),	//percentBodyWeight
						reader[3].ToString(),			//ressistance
						reader[4].ToString(),			//description
						speed1RM
						);
				array.Add(ex);
			}
		}

		reader.Close();
		if(! dbconOpened)
			dbcon.Close();

		return array;
	}
	
	//conversion from DB 0.99 to 1.00
	protected internal static void putEncoderExerciseAnglesAt90() {
		dbcmd.CommandText = "UPDATE " + Constants.EncoderExerciseTable + 
			" SET future2 = 90, future3 = 90";

		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}
	
	//conversion from DB 1.02 to 1.03
	protected internal static void removeEncoderExerciseAngles() {
		dbcmd.CommandText = "UPDATE " + Constants.EncoderExerciseTable + 
			" SET future2 = '', future3 = ''";

		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}
	
	/*
	 * 1RM stuff
	 */

	protected internal static void createTable1RM()
	{
		dbcmd.CommandText = 
			"CREATE TABLE " + Constants.Encoder1RMTable + " ( " +
			"uniqueID INTEGER PRIMARY KEY, " +
			"personID INT, " +
			"sessionID INT, " +
			"exerciseID INT, " +
			"load1RM FLOAT, " +
			"future1 TEXT, " +	
			"future2 TEXT, " +
			"future3 TEXT )";
		dbcmd.ExecuteNonQuery();
	}
	
	public static void Insert1RM(bool dbconOpened, int personID, int sessionID, int exerciseID, double load1RM)	
	{
		if(! dbconOpened)
			dbcon.Open();

		dbcmd.CommandText = "INSERT INTO " + Constants.Encoder1RMTable +  
				" (uniqueID, personID, sessionID, exerciseID, load1RM, future1, future2, future3)" +
				" VALUES (NULL, " + personID + ", " + sessionID + ", " + 
				exerciseID + ", " + Util.ConvertToPoint(load1RM) + ", '','','')";
		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		if(! dbconOpened)
			dbcon.Close();
	}
	
	public static ArrayList Select1RM (bool dbconOpened, int personID, int sessionID, int exerciseID, bool returnPersonNameAndExerciseName)
	{
		if(! dbconOpened)
			dbcon.Open();

		string whereStr = "";
		if(personID != -1 || sessionID != -1 || exerciseID != -1) {
			whereStr = " WHERE ";
			string andStr = "";

			if(personID != -1) {
				whereStr += " " + Constants.Encoder1RMTable + ".personID = " + personID;
				andStr = " AND ";
			}

			if(sessionID != -1) {
				whereStr += andStr + " " + Constants.Encoder1RMTable + ".sessionID = " + sessionID;
				andStr = " AND ";
			}

			if(exerciseID != -1)
				whereStr += andStr + " " + Constants.Encoder1RMTable + ".exerciseID = " + exerciseID;
		}

		if(returnPersonNameAndExerciseName) {
			if(whereStr == "")
				whereStr = " WHERE ";
			else
				whereStr += " AND ";
			whereStr += Constants.Encoder1RMTable + ".personID = person77.uniqueID AND " +
				Constants.Encoder1RMTable + ".exerciseID = encoderExercise.uniqueID";
		}

		if(returnPersonNameAndExerciseName)
			dbcmd.CommandText = "SELECT " + Constants.Encoder1RMTable + ".*, person77.name, encoderExercise.name" + 
				" FROM " + Constants.Encoder1RMTable + ", person77, encoderExercise " +
				whereStr +
				" ORDER BY uniqueID DESC"; //this allows to select the last uniqueID because will be the first in the returned array 
		else
			dbcmd.CommandText = "SELECT * FROM " + Constants.Encoder1RMTable + whereStr +
				" ORDER BY uniqueID DESC"; //this allows to select the last uniqueID because will be the first in the returned array 

		Log.WriteLine(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList array = new ArrayList(1);

		Encoder1RM e1RM = new Encoder1RM();
		while(reader.Read()) {
			if(returnPersonNameAndExerciseName)
				e1RM = new Encoder1RM (
						Convert.ToInt32(reader[0].ToString()),	//uniqueID
						Convert.ToInt32(reader[1].ToString()),	//personID	
						Convert.ToInt32(reader[2].ToString()),	//sessionID
						Convert.ToInt32(reader[3].ToString()),	//exerciseID
						Convert.ToDouble(Util.ChangeDecimalSeparator(reader[4].ToString())),  //load1RM
						reader[8].ToString(),	//personName
						reader[9].ToString()	//exerciseName
						);
			else
				e1RM = new Encoder1RM (
						Convert.ToInt32(reader[0].ToString()),	//uniqueID
						Convert.ToInt32(reader[1].ToString()),	//personID	
						Convert.ToInt32(reader[2].ToString()),	//sessionID
						Convert.ToInt32(reader[3].ToString()),	//exerciseID
						Convert.ToDouble(Util.ChangeDecimalSeparator(reader[4].ToString()))  //load1RM
						);
			array.Add (e1RM);
		}
		reader.Close();
		if(! dbconOpened)
			dbcon.Close();

		return array;
	}


	/* 
	 * database conversions	
	 */

	//convert from DB 1.05 to 1.06
	//1.06 have curves connected to signals
	//as curves detection on every signal can change depending on smoothing, minimal_height, ...
	//1.06 needs to know where the curve is located in the signal
	//starting ms is not reliable because changes with smoothing
	//use central millisecond.
	//
	//this method will find where the central millisecond of a curve is located in a signal
	//and this will be stored in 1.06 in new EncoderSignalCurve table
	//signalID,curveID,contraction(c,ecS,ceS),msCentral
	//encoder table will continue with signals and curves because we don't want to break things now
	//
	//as explained, following method is only used in conversions from 1.05 to 1.06
	//newly saved curves in 1.06 will write msCentral in EncoderSignalCurve table without needing this method
	public static int FindCurveInSignal(string signalFile, string curveFile) 
	{
		int [] signalInts = Util.ReadFileAsInts(signalFile);
		/*	
		Log.WriteLine("found INTS");
		for(int i=0; i < signalInts.Length; i ++)
			Log.Write(signalInts[i] + " ");
		*/	

		int [] curveInts = Util.ReadFileAsInts(curveFile);
		/*
		Log.WriteLine("found INTS");
		for(int i=0; i < curveInts.Length; i ++)
			Log.Write(curveInts[i] + " ");
		*/

		int c;
		for(int s=0; s < signalInts.Length; s ++) {
			for(c=0; c < curveInts.Length && (s + c < signalInts.Length); c ++)
				if(signalInts[s + c] != curveInts[c])
					break;
			
			if(c == curveInts.Length) {
				//Log.WriteLine("Start at: " + s);
				//Log.WriteLine("Middle at: " + s + Convert.ToInt32(c / 2));
				return s + Convert.ToInt32(c / 2);
			}
		}

		return -1;
	}

}
