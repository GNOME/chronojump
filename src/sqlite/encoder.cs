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
 * Copyright (C) 2004-2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Data;
using System.IO;
using System.Collections; //ArrayList
using System.Collections.Generic; //List<T>
using Mono.Data.Sqlite;
using Mono.Unix;


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
			"laterality TEXT, " +	//"RL" "R" "L". stored in english
			"extraWeight TEXT, " +	//string
			"signalOrCurve TEXT, " + //"signal" or "curve"
			"filename TEXT, " +
			"url TEXT, " +		//URL of data of signals and curves. stored as relative
			"time INT, " +
			"minHeight INT, " +
			"description TEXT, " +
			"status TEXT, " +	//"active", "inactive"
			"videoURL TEXT, " +	//URL of video of signals. stored as relative
			"encoderConfiguration TEXT, " +	//text separated by ':'
		       	"future1 TEXT, " +	//Since 1.4.4 (DB 1.06) this stores last meanPower detected on a curve 
						//(as string with '.' because future1 was created as TEXT)
			"future2 TEXT, " + 	//same as future1
			"future3 TEXT, " + 	//same as future1
			"repCriteria TEXT )"; 	//criteria of meanPower, meanSpeed, meanForce: ecc_con, ecc, con
		dbcmd.ExecuteNonQuery();
	}
	
	/*
	 * Encoder class methods
	 */
	
	public static int Insert(bool dbconOpened, EncoderSQL es)
	{
		if(! dbconOpened)
			Sqlite.Open();

		if(es.uniqueID == "-1")
			es.uniqueID = "NULL";

		dbcmd.CommandText = "INSERT INTO " + Constants.EncoderTable +  
			" (uniqueID, personID, sessionID, exerciseID, eccon, laterality, extraWeight, " + 
			"signalOrCurve, filename, url, time, minHeight, description, status, " +
			"videoURL, encoderConfiguration, future1, future2, future3, repCriteria)" +
			" VALUES (" + es.uniqueID + ", " +
			es.personID + ", " + es.sessionID + ", " +
			es.exerciseID + ", \"" + es.eccon + "\", \"" +
			es.LateralityToEnglish() + "\", \"" + Util.ConvertToPoint(es.extraWeight) + "\", \"" +
			es.signalOrCurve + "\", \"" + es.filename + "\", \"" +
			Util.MakeURLrelative(es.url) + "\", " +
			es.time + ", " + es.minHeight + ", \"" + es.description + 
			"\", \"" + es.status + "\", \"" + 
			Util.MakeURLrelative(es.videoURL) + "\", \"" +
			es.encoderConfiguration.ToStringOutput(EncoderConfiguration.Outputs.SQL) + "\", \"" +
			Util.ConvertToPoint(es.future1) + "\", \"" + Util.ConvertToPoint(es.future2) + "\", \"" + Util.ConvertToPoint(es.future3) + "\", \"" +
			es.repCriteria.ToString() + "\")";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		//int myLast = dbcon.LastInsertRowId;
		//http://stackoverflow.com/questions/4341178/getting-the-last-insert-id-with-sqlite-net-in-c
		string myString = @"select last_insert_rowid()";
		dbcmd.CommandText = myString;
		int myLast = Convert.ToInt32(dbcmd.ExecuteScalar()); // Need to type-cast since `ExecuteScalar` returns an object.

		if(! dbconOpened)
			Sqlite.Close();

		return myLast;
	}

	//normal Update call dbcmd will be used	
	public static void Update(bool dbconOpened, EncoderSQL es)
	{
		update(dbconOpened, es, dbcmd);
	}
	//Transaction Update call dbcmdTr will be used	
	public static void Update(bool dbconOpened, EncoderSQL es, SqliteCommand dbcmdTr) 
	{
		update(dbconOpened, es, dbcmdTr);
	}
	private static void update(bool dbconOpened, EncoderSQL es, SqliteCommand mycmd)
	{
		if(! dbconOpened)
			Sqlite.Open();

		if(es.uniqueID == "-1")
			es.uniqueID = "NULL";

		mycmd.CommandText = "UPDATE " + Constants.EncoderTable + " SET " +
				" personID = " + es.personID +
				", sessionID = " + es.sessionID +
				", exerciseID = " + es.exerciseID +
				", eccon = \"" + es.eccon +
				"\", laterality = \"" + es.LateralityToEnglish() +
				"\", extraWeight = \"" + Util.ConvertToPoint(es.extraWeight) +
				"\", signalOrCurve = \"" + es.signalOrCurve +
				"\", filename = \"" + es.filename +
				"\", url = \"" + Util.MakeURLrelative(es.url) +
				"\", time = " + es.time +
				", minHeight = " + es.minHeight +
				", description = \"" + es.description + 
				"\", status = \"" + es.status + 
				"\", videoURL = \"" + Util.MakeURLrelative(es.videoURL) +
				"\", encoderConfiguration = \"" + es.encoderConfiguration.ToStringOutput(EncoderConfiguration.Outputs.SQL) +
				"\", future1 = \"" + Util.ConvertToPoint(es.future1) +
				"\", future2 = \"" + Util.ConvertToPoint(es.future2) +
				"\", future3 = \"" + Util.ConvertToPoint(es.future3) +
				"\", repCriteria = \"" + es.repCriteria.ToString() +
				"\" WHERE uniqueID == " + es.uniqueID ;

		LogB.SQL(mycmd.CommandText.ToString());
		mycmd.ExecuteNonQuery();

		if(! dbconOpened)
			Sqlite.Close();
	}
	
	public static int UpdateTransaction(ArrayList data, string [] checkboxes)
	{
		int count = 0;
		int countActive = 0;

		LogB.SQL("Starting transaction");
		Sqlite.Open();
		
		using(SqliteTransaction tr = dbcon.BeginTransaction())
		{
			using (SqliteCommand dbcmdTr = dbcon.CreateCommand())
			{
				dbcmdTr.Transaction = tr;
	
				foreach(EncoderSQL eSQL in data) {
					if(count < checkboxes.Length && eSQL.status != checkboxes[count]) {
						eSQL.status = checkboxes[count];

						SqliteEncoder.Update(true, eSQL, dbcmdTr);
					}

					count ++;

					if(eSQL.status == "active") 
						countActive ++;
				}
			}
			tr.Commit();
		}

		Sqlite.Close();
		LogB.SQL("Ended transaction");
		return countActive;
	}

	/*
	   SqliteEncoder.Select
	   pass uniqueID value and then will return one record. do like this:
	   EncoderSQL eSQL = (EncoderSQL) SqliteEncoder.Select(false, myUniqueID, 0, 0, 0, "", EncoderSQL.Eccons.ALL, false, true)[0];

	   WARNING because SqliteEncoder.Select may not return nothing, and then cannot be assigned to eSQL
	   see: delete_encoder_curve(bool dbconOpened, int uniqueID)
		and: manageCurvesOfThisSignal

	   don't care for the 0, 0, 0  because selection will be based on the myUniqueID and only one row will be returned
	   or
	   pass uniqueID==-1 and personID, sessionID, signalOrCurve values, and will return some records
	   personID can be -1 to get all on that session
	   sessionID can be -1 to get all sessions
	   exerciseID can be -1 to get all exercises
	   signalOrCurve can be "all"

	   orderIDascendent is good for all the situations except when we want to convert from 1.05 to 1.06
	   in that conversion, we want first the last ones, and later the previous
	   (to delete them if they are old copies)

	   orderRepsByPosInSet uses encoderSignalCurve. encoder reps uniqueIDs are not correctly ordered by set,
	   eg if you save only the best (maybe the 4th), will have uniqueID 1, and then if you save it all,
	   then they will be saved as 2, 3, (4 not saved becuase it is already one), 4, 5, ... So 4th in order will be 1
	   orderRepsByPosInSet fixes this problem. this is used eg. in analyze session to sort them correctly
	   but note it Select will only work ok for curves
	 */
	public static ArrayList Select (
			bool dbconOpened, int uniqueID, int personID, int sessionID, Constants.EncoderGI encoderGI, 
			int exerciseID, string signalOrCurve, EncoderSQL.Eccons ecconSelect, string lateralityEnglish,
			bool onlyActive, bool orderIDascendent,
			bool orderRepsByPosInSet) // Attention! note this only selects curves
	{
		if(! dbconOpened)
			Sqlite.Open();

		string encT = Constants.EncoderTable;
		string encSCT = Constants.EncoderSignalCurveTable;
		string encExT = Constants.EncoderExerciseTable;

		string andString = "";
		string personIDStr = "";
		if(personID != -1) {
			personIDStr = " personID = " + personID;
			andString = " AND ";
		}

		string sessionIDStr = "";
		if(sessionID != -1) {
			sessionIDStr = andString + " sessionID = " + sessionID;
			andString = " AND ";
		}

		string exerciseIDStr = "";
		if(exerciseID != -1) {
			exerciseIDStr = andString + " exerciseID = " + exerciseID;
			andString = " AND ";
		}

		string lateralityEnglishStr = "";
		if(lateralityEnglish != "") {
			lateralityEnglishStr = andString + " laterality = \"" + lateralityEnglish + "\"";
			andString = " AND ";
		}

		string selectStr = "";
		if(uniqueID != -1)
			selectStr = encT + ".uniqueID = " + uniqueID;
		else {
			if(signalOrCurve == "all")
				selectStr = personIDStr + sessionIDStr + exerciseIDStr + lateralityEnglishStr;
			else
				selectStr = personIDStr + sessionIDStr + exerciseIDStr + lateralityEnglishStr + andString + " signalOrCurve = \"" + signalOrCurve + "\"";
		
			if(ecconSelect != EncoderSQL.Eccons.ALL)
				selectStr += andString + encT + ".eccon = \"" + EncoderSQL.Eccons.ecS.ToString() + "\"";
		}

		string fromString = " FROM " + encT  + ", " + encExT;
		if(orderRepsByPosInSet)
			fromString += ", " + encSCT;

		//ensure andString is defined if selectStr is != "" (bug on 2.1.2 release)
		if(selectStr != "")
			andString = " AND ";

		string onlyActiveString = "";
		if(onlyActive)
		{
			onlyActiveString = andString + encT + ".status = \"active\" ";
			andString = " AND ";
		}

		string orderRepsByPosInSetAndStr = "";
		if(orderRepsByPosInSet)
		{
			orderRepsByPosInSetAndStr = andString + encT + ".uniqueID = " +
				encSCT + ".curveID ";
			//andString = " AND ";
		}

		string orderRepsByPosInSetOrderStr = "";
		if(orderRepsByPosInSet)
			orderRepsByPosInSetOrderStr = encSCT + ".mscentral, ";

		string orderIDstr = "";
		if(! orderIDascendent)
			orderIDstr = " DESC";

		dbcmd.CommandText = "SELECT " + 
			encT + ".*, " + encExT + ".name " +
			fromString +
			" WHERE " + selectStr +
			andString + encT + ".exerciseID = " + 
				encExT + ".uniqueID " +
				onlyActiveString + orderRepsByPosInSetAndStr +
			" ORDER BY substr(filename,-23,19), " + //'filename,-23,19' has the date of capture signal
			orderRepsByPosInSetOrderStr +
			"uniqueID " + orderIDstr; 

		LogB.SQL(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList array = new ArrayList(1);

		EncoderSQL eSQL = new EncoderSQL();
		while(reader.Read())
		{
			string [] strFull = reader[15].ToString().Split(new char[] {':'});
			EncoderConfiguration econf = new EncoderConfiguration(
				(Constants.EncoderConfigurationNames) 
				Enum.Parse(typeof(Constants.EncoderConfigurationNames), strFull[0]) );
			econf.ReadParamsFromSQL(strFull);

			//if encoderGI != ALL discard non wanted repetitions
			if(encoderGI == Constants.EncoderGI.GRAVITATORY && econf.has_inertia)
				continue;
			else if(encoderGI == Constants.EncoderGI.INERTIAL && ! econf.has_inertia)
				continue;

			LogB.Debug("EncoderConfiguration = " + econf.ToStringOutput(EncoderConfiguration.Outputs.SQL));
			
			//if there's no video, will be "".
			//if there's video, will be with full path
			string videoURL = "";
			if(reader[14].ToString() != "")
				videoURL = Util.MakeURLabsolute(fixOSpath(reader[14].ToString()));
			
			//LogB.SQL(econf.ToString(":", true));
			eSQL = new EncoderSQL (
					reader[0].ToString(),			//uniqueID
					Convert.ToInt32(reader[1].ToString()),	//personID	
					Convert.ToInt32(reader[2].ToString()),	//sessionID
					Convert.ToInt32(reader[3].ToString()),	//exerciseID
					reader[4].ToString(),			//eccon
					Catalog.GetString(reader[5].ToString()),//laterality
					Util.ChangeDecimalSeparator(reader[6].ToString()),	//extraWeight
					reader[7].ToString(),			//signalOrCurve
					reader[8].ToString(),			//filename
					Util.MakeURLabsolute(fixOSpath(reader[9].ToString())),	//url
					Convert.ToInt32(reader[10].ToString()),	//time
					Convert.ToInt32(reader[11].ToString()),	//minHeight
					reader[12].ToString(),			//description
					reader[13].ToString(),			//status
					videoURL,				//videoURL
					econf,					//encoderConfiguration
					Util.ChangeDecimalSeparator(reader[16].ToString()),	//future1 (meanPower on curves)
					Util.ChangeDecimalSeparator(reader[17].ToString()),	//future2 (meanSpeed on curves)
					Util.ChangeDecimalSeparator(reader[18].ToString()),	//future3 (meanForce on curves)
					(Preferences.EncoderRepetitionCriteria) Enum.Parse(
						typeof (Preferences.EncoderRepetitionCriteria), reader[19].ToString()),
					reader[20].ToString()			//EncoderExercise.name
					);
			array.Add (eSQL);
		}

		reader.Close();
		if(! dbconOpened)
			Sqlite.Close();

		return array;
	}
	

	//used on EncoderSelectRepetitionsIndividualAllSessions
	//exerciseID can be -1 to get all exercises
	public static ArrayList SelectCompareIntersession (bool dbconOpened, Constants.EncoderGI encoderGI,
			int exerciseID, string lateralityCode, int personID)
	{
		if(! dbconOpened)
			Sqlite.Open();

		string exerciseIDStr = "";
		if(exerciseID != -1)
			exerciseIDStr = "encoder.exerciseID = " + exerciseID + " AND ";

		string lateralityCodeStr = "";
		if(lateralityCode != "")
			lateralityCodeStr = "laterality = \"" + lateralityCode + "\" AND ";

		//returns a row for each session where there are active or inactive
		dbcmd.CommandText = 
			"SELECT encoder.sessionID, session.name, session.date, encoder.extraWeight, " +
			" SUM(CASE WHEN encoder.status = \"active\" THEN 1 END) as active, " +
			" SUM(CASE WHEN encoder.status = \"inactive\" THEN 1 END) as inactive," + 
			" encoder.encoderConfiguration " +
			" FROM encoder, session, person77 " +
			" WHERE " +
			exerciseIDStr + lateralityCodeStr +
			" encoder.personID = " + personID + " AND signalOrCurve = \"curve\" AND " +
			" encoder.personID = person77.uniqueID AND encoder.sessionID = session.uniqueID " +
			" GROUP BY encoder.sessionID, encoder.extraWeight ORDER BY encoder.sessionID, encoder.extraWeight, encoder.status";
	
		LogB.SQL(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList array = new ArrayList();
		EncoderPersonCurvesInDB encPS = new EncoderPersonCurvesInDB();
		/*
		 * eg.
		 * sessID|sess name|date|extraWe|a|i (a: active, i: inactive)
		 * 20|Encoder tests|2012-12-10|7|3|
		 * 20|Encoder tests|2012-12-10|0||9
		 * 20|Encoder tests|2012-12-10|10||34
		 * 20|Encoder tests|2012-12-10|58||1
		 * 20|Encoder tests|2012-12-10|61||1
		 * 26|sessio-proves|2013-07-08|10|5|36
		 * 30|proves encoder|2013-11-08|0|2|
		 * 30|proves encoder|2013-11-08|100|5|
		 * 
		 * convert to:
		 *
		 * sessID|sess name|date|a|i|reps*weights	(a: active, i: inactive)
		 * 20|Encoder tests|2012-12-10|3|45|3*7 9*0 34*10 1*58 1*61 (but sorted)
		 *
		 */
		int sessIDDoing = -1; //of this sessionID
		int sessIDThisRow = -1; //of this SQL row
		List<EncoderPersonCurvesInDBDeep> lDeep = new List<EncoderPersonCurvesInDBDeep>();
		bool firstSession = true;
		int activeThisRow;
		int inactiveThisRow;
		int activeThisSession = 0;
		int inactiveThisSession = 0;

		while(reader.Read()) {
			//discard if != encoderGI
			string [] strFull = reader[6].ToString().Split(new char[] {':'});
			EncoderConfiguration econf = new EncoderConfiguration(
				(Constants.EncoderConfigurationNames) 
				Enum.Parse(typeof(Constants.EncoderConfigurationNames), strFull[0]) );

			//if encoderGI != ALL discard non wanted repetitions
			if(encoderGI == Constants.EncoderGI.GRAVITATORY && econf.has_inertia)
				continue;
			else if(encoderGI == Constants.EncoderGI.INERTIAL && ! econf.has_inertia)
				continue;

			//1 get sessionID of this row
			sessIDThisRow = Convert.ToInt32(reader[0].ToString());

			//2 get active an inactive curves of this row
			activeThisRow = 0;
			string activeStr = reader[4].ToString();
			if(Util.IsNumber(activeStr, false))
				activeThisRow = Convert.ToInt32(activeStr);
			
			inactiveThisRow = 0;
			string inactiveStr = reader[5].ToString();
			if(Util.IsNumber(inactiveStr, false))
				inactiveThisRow = Convert.ToInt32(inactiveStr);
			
			//3 if session of this row is different than previous row
			if(sessIDThisRow != sessIDDoing) 
			{
				sessIDDoing = sessIDThisRow;

				if(! firstSession) {
					//if is not first session (means we have processed a session before)
					//update encPS with the lDeep and then add to array
					encPS.lDeep = lDeep;
					encPS.countActive = activeThisSession;
					encPS.countAll = activeThisSession + inactiveThisSession;
					array.Add(encPS);
				}

				firstSession = false;

				//create new EncoderPersonCurvesInDB
				encPS = new EncoderPersonCurvesInDB (
						personID,
						Convert.ToInt32(reader[0].ToString()),	//sessionID
						reader[1].ToString(),			//sessionName
						reader[2].ToString());			//sessionDate
					
				activeThisSession = 0;
				inactiveThisSession = 0;
				//empty lDeep
				lDeep = new List<EncoderPersonCurvesInDBDeep>();
			}
			//4 add deep info: (weight, all reps)
			EncoderPersonCurvesInDBDeep deep = new EncoderPersonCurvesInDBDeep(
					Convert.ToDouble(Util.ChangeDecimalSeparator(reader[3].ToString())), activeThisRow + inactiveThisRow);
			//add to lDeep
			lDeep.Add(deep);
			
			activeThisSession += activeThisRow;
			inactiveThisSession += inactiveThisRow;
		}
		
		//store last row in array (once we are out the while)
		if(! firstSession) {
			//if is not first session (means we have processed a session before)
			//update encPS with the lDeep and then add to array
			encPS.lDeep = lDeep;
			encPS.countActive = activeThisSession;
			encPS.countAll = activeThisSession + inactiveThisSession;
			array.Add(encPS);
		}

		reader.Close();
		if(! dbconOpened)
			Sqlite.Close();

		return array;
	}
	
	public static ArrayList SelectSessionOverviewSets (bool dbconOpened, Constants.EncoderGI encoderGI, int sessionID)
	{
		if(! dbconOpened)
			Sqlite.Open();
	
		dbcmd.CommandText = 
			"SELECT person77.uniqueID, person77.name, person77.sex, encoder.encoderConfiguration, encoderExercise.name, (personSession77.weight * encoderExercise.percentBodyWeight/100) + encoder.extraWeight, COUNT(*)" +
			" FROM person77, personSession77, encoderExercise, encoder" + 
			" WHERE person77.uniqueID == encoder.personID AND personSession77.personID == encoder.personID AND personSession77.sessionID == encoder.sessionID AND encoderExercise.uniqueID==encoder.exerciseID AND signalOrCurve == \"signal\" AND encoder.sessionID == " + sessionID + 
			" GROUP BY encoder.personID, encoderConfiguration, exerciseID, extraWeight" +
			" ORDER BY person77.name";
		
		LogB.SQL(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList array = new ArrayList();
		while(reader.Read())
		{
			//discard if != encoderGI
			string [] strFull = reader[3].ToString().Split(new char[] {':'});
			EncoderConfiguration econf = new EncoderConfiguration(
				(Constants.EncoderConfigurationNames) 
				Enum.Parse(typeof(Constants.EncoderConfigurationNames), strFull[0]) );

			//if encoderGI != ALL discard non wanted repetitions
			if(encoderGI == Constants.EncoderGI.GRAVITATORY && econf.has_inertia)
				continue;
			else if(encoderGI == Constants.EncoderGI.INERTIAL && ! econf.has_inertia)
				continue;

			if(encoderGI == Constants.EncoderGI.GRAVITATORY)
			{
				string [] s = { 
					reader[0].ToString(), 	//person uniqueID
					reader[1].ToString(), 	//person name
					reader[2].ToString(), 	//person sex
					econf.ToStringPretty(), //encoder configuration
					reader[4].ToString(), 	//encoder exercise name
					reader[5].ToString(),	//displaced mass (includes percentBodyeight)
					reader[6].ToString()	//sets count
				};
				array.Add (s);
			} else {
				string [] s = { 
					reader[0].ToString(), 	//person uniqueID
					reader[1].ToString(), 	//person name
					reader[2].ToString(), 	//person sex
					econf.ToStringPretty(),	//encoder configuration
					reader[4].ToString(), 	//encoder exercise name
					reader[6].ToString()	//sets count
				};
				array.Add (s);
			}
		}

		reader.Close();
		if(! dbconOpened)
			Sqlite.Close();

		return array;
	}

	public static ArrayList SelectSessionOverviewReps (bool dbconOpened, Constants.EncoderGI encoderGI, int sessionID)
	{
		if(! dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText =
			"SELECT person77.uniqueID, person77.name, person77.sex, encoder.encoderConfiguration, encoderExercise.name, " + 
			"encoder.extraWeight, encoder.eccon, encoder.future1, encoder.future2, encoder.future3, encoder.repCriteria " +
			"FROM person77, encoderExercise, encoder " +
			"WHERE sessionID = " + sessionID.ToString() +
		        " AND signalOrCurve = \"curve\" " +
			" AND person77.uniqueID = encoder.personID " +
			" AND encoderExercise.uniqueID = encoder.exerciseID " +
			" ORDER BY person77.name";

		LogB.SQL(dbcmd.CommandText.ToString());

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList array = new ArrayList();
		while(reader.Read())
		{
			//discard if != encoderGI
			string [] strFull = reader[3].ToString().Split(new char[] {':'});
			EncoderConfiguration econf = new EncoderConfiguration(
				(Constants.EncoderConfigurationNames)
				Enum.Parse(typeof(Constants.EncoderConfigurationNames), strFull[0]) );

			//if encoderGI != ALL discard non wanted repetitions
			if(encoderGI == Constants.EncoderGI.GRAVITATORY && econf.has_inertia)
				continue;
			else if(encoderGI == Constants.EncoderGI.INERTIAL && ! econf.has_inertia)
				continue;

			string repCriteria = "";
			if(reader[6].ToString() != "c")
			{
				if(reader[10].ToString() == Preferences.EncoderRepetitionCriteria.ECC_CON.ToString())
					repCriteria = Catalog.GetString("Eccentric-concentric");
				else if(reader[10].ToString() == Preferences.EncoderRepetitionCriteria.ECC.ToString())
					repCriteria = Catalog.GetString("Eccentric");
				else if(reader[10].ToString() == Preferences.EncoderRepetitionCriteria.CON.ToString())
					repCriteria = Catalog.GetString("Concentric");
			}

			if(encoderGI == Constants.EncoderGI.GRAVITATORY)
			{
				string [] s = {
					reader[0].ToString(), 	//person uniqueID
					reader[1].ToString(), 	//person name
					reader[2].ToString(), 	//person sex
					econf.ToStringPretty(), //encoder configuration
					reader[4].ToString(), 	//encoder exercise name
					Util.ChangeDecimalSeparator(reader[5].ToString()),	//extra mass
					EncoderSQL.EcconLong(reader[6].ToString()),
					Util.ChangeDecimalSeparator (reader[7].ToString()),	//power
					Util.ChangeDecimalSeparator (reader[8].ToString()),	//speed
					Util.ChangeDecimalSeparator (reader[9].ToString()),	//force
					repCriteria
				};
				array.Add (s);
			} else {
				string [] s = {
					reader[0].ToString(), 	//person uniqueID
					reader[1].ToString(), 	//person name
					reader[2].ToString(), 	//person sex
					econf.ToStringPretty(), //encoder configuration
					reader[4].ToString(), 	//encoder exercise name
					EncoderSQL.EcconLong(reader[6].ToString()),
					Util.ChangeDecimalSeparator (reader[7].ToString()),	//power
					Util.ChangeDecimalSeparator (reader[8].ToString()),	//speed
					Util.ChangeDecimalSeparator (reader[9].ToString()),	//force
					repCriteria
				};
				array.Add (s);
			}
		}

		reader.Close();
		if(! dbconOpened)
			Sqlite.Close();

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
			"future1 TEXT )"; //right now unused. need future2, future3. Better to use alter table here and on encoder table
		dbcmd.ExecuteNonQuery();
	}
	
	public static void SignalCurveInsert(bool dbconOpened, int signalID, int curveID, int msCentral)
	{
		if(! dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "INSERT INTO " + Constants.EncoderSignalCurveTable +  
			" (uniqueID, signalID, curveID, msCentral, future1) " + 
			"VALUES (NULL, " + signalID + ", " + curveID + ", " + msCentral + ", \"\")";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		if(! dbconOpened)
			Sqlite.Close();
	}
	

	//signalID == -1 (any signal)
	//curveID == -1 (any curve)
	//if msStart and msEnd != -1 (means find a curve with msCentral contained between both values)
	public static ArrayList SelectSignalCurve (bool dbconOpened, int signalID, int curveID, double msStart, double msEnd)
	{
		if(! dbconOpened)
			Sqlite.Open();

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
		
		LogB.SQL(dbcmd.CommandText.ToString());
		
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
			Sqlite.Close();

		return array;
	}

	public static void DeleteSignalCurveWithCurveID(bool dbconOpened, int curveID)
	{
		if( ! dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "Delete FROM " + Constants.EncoderSignalCurveTable +
			" WHERE curveID == " + curveID.ToString();
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		if( ! dbconOpened)
			Sqlite.Close();
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
			"future3 TEXT, " +	//weightAngle (unused)
			"type TEXT DEFAULT \"ALL\")";	//ALL, GRAVITATORY, INERTIAL (enum constants.EncoderGI)
		dbcmd.ExecuteNonQuery();
	}

	//if uniqueID == -1, NULL will be used (correlative uniqueID)
	//uniqueID != -1 when an exercise is downloaded from server on compujump and need to have the same uniqueID as server
	public static void InsertExercise(bool dbconOpened, int uniqueID, string name, int percentBodyWeight,
			string ressistance, string description, string speed1RM, 	//speed1RM decimal point = '.'
			Constants.EncoderGI encoderGI)	 				//type
	{
		if(! dbconOpened)
			Sqlite.Open();

		string uniqueIDStr = "NULL";
		if(uniqueID != -1)
			uniqueIDStr = uniqueID.ToString();

		dbcmd.CommandText = "INSERT INTO " + Constants.EncoderExerciseTable +  
				" (uniqueID, name, percentBodyWeight, ressistance, description, future1, future2, future3, type)" +
				" VALUES (" + uniqueIDStr + ", \"" + name + "\", " + percentBodyWeight + ", \"" +
				ressistance + "\", \"" + description + "\", \"" + speed1RM + "\", '', '', \"" + encoderGI.ToString() + "\")";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		if(! dbconOpened)
			Sqlite.Close();
	}

	//Note: if this names change, or there are new, change them on both:
	//gui/encoder createEncoderCombos();	
	//gui/encoder on_button_encoder_exercise_add_accepted (object o, EventArgs args) 
	protected internal static void initializeTableEncoderExercise()
	{
		string [] iniEncoderExercises = {
			//name:percentBodyWeight:ressistance:description:speed1RM:pullAngle:weightAngle
			"Bench press:0:weight bar::0.185:::GRAVITATORY", //González-Badillo, J. 2010. Movement velocity as a measure of loading intensity in resistance training
			"Squat:100:weight bar::0.31:::GRAVITATORY" //González-Badillo, JJ.2000b http://foro.chronojump.org/showthread.php?tid=1288&page=3
		};
		
		foreach(string line in iniEncoderExercises) {
			string [] parts = line.Split(new char[] {':'});
			InsertExercise(true, -1, parts[0], Convert.ToInt32(parts[1]), parts[2], parts[3], parts[4],
					(Constants.EncoderGI) Enum.Parse(typeof(Constants.EncoderGI), parts[7]));
		}

		addEncoderFreeExercise();
		addEncoderJumpExercise();
		addEncoderInclinedExercises();
	}

	protected internal static void addEncoderFreeExercise()
	{
		bool exists = Sqlite.Exists (true, Constants.EncoderExerciseTable, "Free");
		if(! exists)
			InsertExercise(true, -1, "Free", 0, "", "", "", Constants.EncoderGI.ALL);
	}
	protected internal static void addEncoderJumpExercise()
	{
		bool exists = Sqlite.Exists (true, Constants.EncoderExerciseTable, "Jump");
		if(! exists)
			InsertExercise(true, -1, "Jump", 100, "", "", "", Constants.EncoderGI.GRAVITATORY);
	}
	protected internal static void addEncoderInclinedExercises()
	{
		string [] iniEncoderExercises = {
			//name:percentBodyWeight:ressistance:description:speed1RM:bodyAngle:weightAngle:type
			"Inclined plane:0:machine:::::GRAVITATORY",
			"Inclined plane BW:100:machine:::::GRAVITATORY",
		};
		
		foreach(string line in iniEncoderExercises) {
			string [] parts = line.Split(new char[] {':'});
			InsertExercise(true, -1, parts[0], Convert.ToInt32(parts[1]), parts[2], parts[3], parts[4],
					(Constants.EncoderGI) Enum.Parse(typeof(Constants.EncoderGI), parts[7]));
		}
	}

	public static void UpdateExercise (bool dbconOpened, EncoderExercise ex)
	{
		if(! dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "UPDATE " + Constants.EncoderExerciseTable + " SET" +
				" name = \"" + ex.Name +
				"\", percentBodyWeight = " + ex.PercentBodyWeight +
				", ressistance = \"" + ex.Ressistance +
				"\", description = \"" + ex.Description +
				"\", future1 = \"" + Util.ConvertToPoint(ex.Speed1RM) +
				"\", type = \"" + ex.Type +
				"\" WHERE uniqueID = " + ex.UniqueID;

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		if(! dbconOpened)
			Sqlite.Close();
	}
	public static void UpdateExerciseByName_old_do_not_use(bool dbconOpened, string nameOld, string name, int percentBodyWeight,
			string ressistance, string description, string speed1RM, Constants.EncoderGI type)
	{
		if(! dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "UPDATE " + Constants.EncoderExerciseTable + " SET " +
				" name = \"" + name +
				"\", percentBodyWeight = " + percentBodyWeight +
				", ressistance = \"" + ressistance +
				"\", description = \"" + description +
				"\", future1 = \"" + speed1RM +
				"\", type = \"" + type.ToString() +
				"\" WHERE name = \"" + nameOld + "\"" ;

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		if(! dbconOpened)
			Sqlite.Close();
	}
	
	//if uniqueID != -1, returns an especific EncoderExercise that can be read like this	
	//EncoderExercise ex = (EncoderExercise) SqliteEncoder.SelectEncoderExercises(eSQL.exerciseID)[0];
	//if encoderGI == GRAVITATORY, return GRAVITATORY and ALL
	//if encoderGI == INERTIAL, return INERTIAL and ALL
	//if encoderGI == ALL, return everything
	public static ArrayList SelectEncoderExercises(bool dbconOpened, int uniqueID, bool onlyNames, Constants.EncoderGI encoderGI)
	{
		if(! dbconOpened)
			Sqlite.Open();

		string encoderGIconnector = " WHERE ";

		string uniqueIDStr = "";
		if(uniqueID != -1) {
			uniqueIDStr = " WHERE " + Constants.EncoderExerciseTable + ".uniqueID = " + uniqueID;
			encoderGIconnector = " AND ";
		}

		string encoderGIstr = "";
		if(encoderGI == Constants.EncoderGI.GRAVITATORY)
			encoderGIstr = encoderGIconnector + " type != \"INERTIAL\"";
		else if(encoderGI == Constants.EncoderGI.INERTIAL)
			encoderGIstr = encoderGIconnector + " type != \"GRAVITATORY\"";
	
		if(onlyNames)
			dbcmd.CommandText = "SELECT name FROM " + Constants.EncoderExerciseTable + uniqueIDStr + encoderGIstr;
		else
			dbcmd.CommandText = "SELECT * FROM " + Constants.EncoderExerciseTable + uniqueIDStr + encoderGIstr;
		
		LogB.SQL(dbcmd.CommandText.ToString());
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
						reader[3].ToString(),			//resistance
						reader[4].ToString(),			//description
						speed1RM,
						(Constants.EncoderGI) Enum.Parse(typeof(Constants.EncoderGI), reader[8].ToString())
						);
				array.Add(ex);
			}
		}

		reader.Close();
		if(! dbconOpened)
			Sqlite.Close();

		return array;
	}

	//gets a list of the exercises in curves to show them on encoder analyze tab
	//-1 if all sessions or all persons
	public static List<int> SelectAnalyzeExercisesInCurves (bool dbconOpened, int personID, int sessionID, Constants.EncoderGI encoderGI)
	{
		if(! dbconOpened)
			Sqlite.Open();

		string whereStr = " WHERE signalOrCurve = 'curve' ";

		if(personID != -1)
			whereStr += " AND " + Constants.EncoderTable + ".personID = " + personID;

		if(sessionID != -1)
			whereStr += " AND " + Constants.EncoderTable + ".sessionID = " + sessionID;

		dbcmd.CommandText = "SELECT exerciseID, encoderConfiguration FROM " + Constants.EncoderTable + whereStr +
			" ORDER BY exerciseID";

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();
		List<int> l = new List<int>();

		while(reader.Read())
		{
			//discard if != encoderGI
			string [] strFull = reader[1].ToString().Split(new char[] {':'});
			EncoderConfiguration econf = new EncoderConfiguration(
				(Constants.EncoderConfigurationNames)
				Enum.Parse(typeof(Constants.EncoderConfigurationNames), strFull[0]) );

			//if encoderGI != ALL discard non wanted repetitions
			if(encoderGI == Constants.EncoderGI.GRAVITATORY && econf.has_inertia)
				continue;
			else if(encoderGI == Constants.EncoderGI.INERTIAL && ! econf.has_inertia)
				continue;

			int exID = Convert.ToInt32(reader[0].ToString());
			//Add to list l if not exists
			if(l.IndexOf(exID) == -1)
				l.Add(exID);
		}

		reader.Close();
		if(! dbconOpened)
			Sqlite.Close();

		return l;
	}

	public static ArrayList SelectEncoderSetsOfAnExercise(bool dbconOpened, int exerciseID)
	{
		if(! dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "select count(*), " + 
			Constants.PersonTable + ".name, " +
			Constants.SessionTable + ".name, " + 
			Constants.SessionTable + ".date " + 
			" FROM " + Constants.EncoderTable + ", " + Constants.PersonTable + ", " + Constants.SessionTable +
			" WHERE exerciseID == " + exerciseID +
		        " AND signalOrCurve = \"signal\" " +
			" AND " + Constants.PersonTable + ".uniqueID == " + Constants.EncoderTable + ".personID " +
		        " AND " + Constants.SessionTable + ".uniqueID == " + Constants.EncoderTable + ".sessionID " + 
			" GROUP BY sessionID, personID";
			
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList array = new ArrayList();
		int count = 0;
		while(reader.Read()) {
			array.Add(new string [] {
					count.ToString(),
					reader[0].ToString(), //count
					reader[1].ToString(), //person name
					reader[2].ToString(), //session name
					reader[3].ToString()  //session date
			});
			count ++;
		}

		reader.Close();
		if(! dbconOpened)
			Sqlite.Close();

		return array;
	}
	
	//conversion from DB 0.99 to 1.00
	protected internal static void putEncoderExerciseAnglesAt90() {
		dbcmd.CommandText = "UPDATE " + Constants.EncoderExerciseTable + 
			" SET future2 = 90, future3 = 90";

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}
	
	//conversion from DB 1.02 to 1.03
	protected internal static void removeEncoderExerciseAngles() {
		dbcmd.CommandText = "UPDATE " + Constants.EncoderExerciseTable + 
			" SET future2 = \"\", future3 = \"\"";

		LogB.SQL(dbcmd.CommandText.ToString());
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
	
	public static int Insert1RM(bool dbconOpened, int personID, int sessionID, int exerciseID, double load1RM)	
	{
		if(! dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "INSERT INTO " + Constants.Encoder1RMTable +  
				" (uniqueID, personID, sessionID, exerciseID, load1RM, future1, future2, future3)" +
				" VALUES (NULL, " + personID + ", " + sessionID + ", " + 
				exerciseID + ", " + Util.ConvertToPoint(load1RM) + ", \"\",\"\",\"\")";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		//int myLast = dbcon.LastInsertRowId;
		//http://stackoverflow.com/questions/4341178/getting-the-last-insert-id-with-sqlite-net-in-c
		string myString = @"select last_insert_rowid()";
		dbcmd.CommandText = myString;
		int myLast = Convert.ToInt32(dbcmd.ExecuteScalar()); // Need to type-cast since `ExecuteScalar` returns an object.

		if(! dbconOpened)
			Sqlite.Close();

		return myLast;
	}

	public static ArrayList Select1RM (bool dbconOpened, int personID, int sessionID, int exerciseID, bool returnPersonNameAndExerciseName)
	{
		if(! dbconOpened)
			Sqlite.Open();

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
			dbcmd.CommandText = "SELECT " + Constants.Encoder1RMTable + ".*, person77.name, encoderExercise.name, session.date" + 
				" FROM " + Constants.Encoder1RMTable + ", person77, encoderExercise, session " +
				whereStr + " AND " + Constants.Encoder1RMTable + ".sessionID = session.uniqueID " +
				" ORDER BY uniqueID DESC"; //this allows to select the last uniqueID because will be the first in the returned array 
		else
			dbcmd.CommandText = "SELECT " + Constants.Encoder1RMTable + ".*, session.date FROM " + 
				Constants.Encoder1RMTable + ", session" + whereStr +
				" ORDER BY uniqueID DESC"; //this allows to select the last uniqueID because will be the first in the returned array 

		LogB.SQL(dbcmd.CommandText.ToString());
		
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
						UtilDate.FromSql(reader[10].ToString()),//date
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
						UtilDate.FromSql(reader[5].ToString()),		//date
						Convert.ToInt32(reader[3].ToString()),	//exerciseID
						Convert.ToDouble(Util.ChangeDecimalSeparator(reader[4].ToString()))  //load1RM
						);
			array.Add (e1RM);
		}
		reader.Close();
		if(! dbconOpened)
			Sqlite.Close();

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
		LogB.SQL("found INTS");
		for(int i=0; i < signalInts.Length; i ++)
			Log.Write(signalInts[i] + " ");
		*/	

		int [] curveInts = Util.ReadFileAsInts(curveFile);
		/*
		LogB.SQL("found INTS");
		for(int i=0; i < curveInts.Length; i ++)
			Log.Write(curveInts[i] + " ");
		*/

		int c;
		for(int s=0; s < signalInts.Length; s ++) {
			for(c=0; c < curveInts.Length && (s + c < signalInts.Length); c ++)
				if(signalInts[s + c] != curveInts[c])
					break;
			
			if(c == curveInts.Length) {
				//LogB.SQL("Start at: " + s);
				//LogB.SQL("Middle at: " + s + Convert.ToInt32(c / 2));
				return s + Convert.ToInt32(c / 2);
			}
		}

		return -1;
	}
}
