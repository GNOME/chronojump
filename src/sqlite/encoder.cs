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
			"smooth FLOAT, " +  	//unused. since 1.3.7 is on preferences
			"description TEXT, " +
			"future1 TEXT, " +	//works as status: "active", "inactive"
			"future2 TEXT, " +	//URL of video of signals
			"future3 TEXT )";	//Inverted (encoder is upside down) only on signals
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
				" (uniqueID, personID, sessionID, exerciseID, eccon, laterality, extraWeight, signalOrCurve, filename, url, time, minHeight, smooth, description, future1, future2, future3)" +
				" VALUES (" + es.uniqueID + ", " +
				es.personID + ", " + es.sessionID + ", " +
				es.exerciseID + ", '" + es.eccon + "', '" +
				es.laterality + "', '" + es.extraWeight + "', '" +
				es.signalOrCurve + "', '" + es.filename + "', '" +
				es.url + "', " + es.time + ", " + es.minHeight + ", " +
				Util.ConvertToPoint(es.smooth) + ", '" + es.description + "', 'active', " + 
				"'','" + 		//future2 url (this is stored later)
			       	es.future3 + "')" ;	//future3 inverted?
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
				", smooth = " + Util.ConvertToPoint(es.smooth) +	//unused. in 1.3.7 is on preferences
				", description = '" + es.description + 
				"', future1 = '" + es.future1 + 
				"', future2 = '" + es.future2 + 
				"', future3 = '" + es.future3 + 
				"' WHERE uniqueID == " + es.uniqueID ;

		Log.WriteLine(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		if(! dbconOpened)
			dbcon.Close();
	}
	
	//pass uniqueID value and then will return one record. do like this:
	//EncoderSQL eSQL = (EncoderSQL) SqliteEncoder.Select(false, myUniqueID, 0, 0, "")[0];
	//or
	//pass uniqueID==-1 and personID, sessionID, signalOrCurve values, and will return some records
	//personID can be -1 to get all on that session
	//sessionID can be -1 to get all sessions
	public static ArrayList Select (bool dbconOpened, 
			int uniqueID, int personID, int sessionID, string signalOrCurve, bool onlyActive)
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
		else
			selectStr = personIDStr + sessionIDStr + 
			" signalOrCurve = '" + signalOrCurve + "'";

		string onlyActiveString = "";
		if(onlyActive)
			onlyActiveString = " AND " + Constants.EncoderTable + ".future1 = 'active' ";

		dbcmd.CommandText = "SELECT " + 
			Constants.EncoderTable + ".*, " + Constants.EncoderExerciseTable + ".name FROM " + 
			Constants.EncoderTable  + ", " + Constants.EncoderExerciseTable  + 
			" WHERE " + selectStr +
			" AND " + Constants.EncoderTable + ".exerciseID = " + 
				Constants.EncoderExerciseTable + ".uniqueID " +
				onlyActiveString +
			" ORDER BY substr(filename,-23,19)"; //this contains the date of capture signal

		Log.WriteLine(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList array = new ArrayList(1);

		EncoderSQL es = new EncoderSQL();
		while(reader.Read()) {
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
					Convert.ToDouble(Util.ChangeDecimalSeparator(reader[12].ToString())), //smooth UNUSED
					reader[13].ToString(),			//description
					reader[14].ToString(),			//future1
					reader[15].ToString(),			//future2
					reader[16].ToString(),			//future3
					reader[17].ToString()			//EncoderExercise.name
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
			"SELECT count(*), encoder.sessionID, session.name, session.date, encoder.future1 " +
			" FROM encoder, session, person77 " +
			" WHERE encoder.personID == " + personID + " AND signalOrCurve == 'curve' AND " + 
			" encoder.personID == person77.uniqueID AND encoder.sessionID == session.uniqueID " + 
			" GROUP BY encoder.sessionID, encoder.future1 ORDER BY encoder.sessionID, encoder.future1";
			*/

		//returns a row for each session where there are active or inactive
		dbcmd.CommandText = 
			"SELECT encoder.sessionID, session.name, session.date, " +
			" SUM(CASE WHEN encoder.future1 = 'active' THEN 1 END) as active, " +
			" SUM(CASE WHEN encoder.future1 = 'inactive' THEN 1 END) as inactive " + 
			" FROM encoder, session, person77 " +
			" WHERE encoder.personID == " + personID + " AND signalOrCurve == 'curve' AND " +
			" encoder.personID == person77.uniqueID AND encoder.sessionID == session.uniqueID " +
			" GROUP BY encoder.sessionID ORDER BY encoder.sessionID, encoder.future1";
	
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
			"future2 TEXT, " +
			"future3 TEXT )";
		dbcmd.ExecuteNonQuery();
	}
	
	public static void InsertExercise(bool dbconOpened, string name, int percentBodyWeight, 
			string ressistance, string description, string speed1RM) //speed1RM decimal point = '.'
	{
		if(! dbconOpened)
			dbcon.Open();

		dbcmd.CommandText = "INSERT INTO " + Constants.EncoderExerciseTable +  
				" (uniqueID, name, percentBodyWeight, ressistance, description, future1, future2, future3)" +
				" VALUES (NULL, '" + name + "', " + percentBodyWeight + ", '" + 
				ressistance + "', '" + description + "', '" + speed1RM + "','','')";
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
			//name:percentBodyWeight:ressistance:description:speed1RM
			"Bench press:0:weight bar::0.185", //González-Badillo, J. 2010. Movement velocity as a measure of loading intensity in resistance training
			"Squat:100:weight bar::0.31" //González-Badillo, JJ.2000b http://foro.chronojump.org/showthread.php?tid=1288&page=3 
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
	
	public static ArrayList Select1RM (bool dbconOpened, int personID, int sessionID, int exerciseID)
	{
		if(! dbconOpened)
			dbcon.Open();

		string whereStr = "";
		if(personID != -1 || sessionID != -1 || exerciseID != -1) {
			whereStr = " WHERE ";
			string andStr = "";

			if(personID != -1) {
				whereStr += " personID = " + personID;
				andStr = " AND ";
			}

			if(sessionID != -1) {
				whereStr += andStr + " sessionID = " + sessionID;
				andStr = " AND ";
			}

			if(exerciseID != -1)
				whereStr += andStr + " exerciseID = " + exerciseID;
		}

		dbcmd.CommandText = "SELECT * FROM " + Constants.Encoder1RMTable + whereStr +
			" ORDER BY uniqueID DESC"; //this allows to select the last uniqueID because will be the first in the returned array 

		Log.WriteLine(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList array = new ArrayList(1);

		Encoder1RM e1RM = new Encoder1RM();
		while(reader.Read()) {
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
	

}
