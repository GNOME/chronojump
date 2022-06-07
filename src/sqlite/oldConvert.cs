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
 * Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using System.IO;
using System.Collections; //ArrayList
using Mono.Data.Sqlite;

/* methods for convert from old tables to new tables */

class SqliteOldConvert : Sqlite
{

	/*
	 * DB 1.20 -> 1.21
	 * "Fixing loosing of encoder videoURL after recalculate"
	 * each encoder signal can have saved some encoder curves
	 * both are records on encoder table
	 * connection between them is found in encoderSignalCurve table.
	 * Problem since chronojump 1.4.9 and maybe earlier is on recalculate: videoURL is deleted on signal
	 * but hopefully not in curve
	 * Now this problem has been fixed in new code and it does not get deleted.
	 *
	 * Following  method: is to restore signals that lost their videoURL value
	 */

	public static void FixLostVideoURLAfterEncoderRecalculate()
	{
		dbcmd.CommandText = "SELECT eSignal.uniqueID, eCurve.videoURL " + 
			"FROM encoder AS eSignal, encoder AS eCurve, encoderSignalCurve " + 
			"WHERE eSignal.signalOrCurve = \"signal\" AND eCurve.signalOrCurve = \"curve\" " + 
			"AND eSignal.videoURL = \"\" AND eCurve.videoURL != \"\" " + 
			"AND encoderSignalCurve.signalID = eSignal.uniqueID " +
			"AND encoderSignalCurve.curveID = eCurve.uniqueID";

		LogB.SQL(dbcmd.CommandText.ToString());
		SqliteDataReader reader = dbcmd.ExecuteReader();

		IDNameList idnamelist = new IDNameList();
		while(reader.Read()) {
			idnamelist.Add(new IDName(
						Convert.ToInt32(reader[0].ToString()), //encoder signal uniqueID (this signal has lost his videoURL)
						reader[1].ToString()	//videoURL of encoder curve
					   ));
		}
		reader.Close();

		foreach(IDName idname in idnamelist.l) 
		{
			dbcmd.CommandText = "UPDATE encoder SET videoURL = \"" + idname.Name + "\" " + 
				"WHERE uniqueID = " + idname.UniqueID.ToString();
			LogB.SQL(dbcmd.CommandText.ToString());
			dbcmd.ExecuteNonQuery();
		}
	}
	
	//to be easier to move data between computers, absolute paths have to be converted to relative
	//DB 1.11 -> 1.12
	//dbcon is already opened
	public static void ConvertAbsolutePathsToRelative () 
	{
		//get parentDir with the final '/' or '\'
		string parentDir = Util.GetLocalDataDir (true);

		ConvertAbsolutePathsToRelativeDo(parentDir, "encoder", "videoURL");
		ConvertAbsolutePathsToRelativeDo(parentDir, "encoder", "url");

		//URLs of videos of contact tests: jump, run,... are not in the database
		//URLs of images of person77			 are not in the database
	}
	public static void ConvertAbsolutePathsToRelativeDo (string parentDir, string table, string column) {
		//eg. dbcmd.CommandText = "UPDATE encoder SET videoURL = replace( videoURL, '/home/user/.local/share/Chronojump/', '' ) " + 
		//	"WHERE videoURL LIKE '/home/user/.local/share/Chronojump/%'";

		dbcmd.CommandText = "UPDATE " + table + " SET " + column + " = replace( " + column + ", \"" + parentDir + "\", \"\" ) " + 
			"WHERE " + column + " LIKE \"" + parentDir + "%\"";

		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();

	}	

	//convert slCMJ to slCMJleft, slCMJright
	//DB 1.13 -> DB 1.14
	public static void slCMJDivide() {
		//it's a conversion, dbcon is opened

		//changes on jumpType table
		SqliteJumpType.Delete(Constants.JumpTypeTable, "slCMJ", true);
		SqliteJumpType.JumpTypeInsert("slCMJleft:1:0:Single-leg CMJ jump", true);
		SqliteJumpType.JumpTypeInsert("slCMJright:1:0:Single-leg CMJ jump", true);


		//changes on jump table
		dbcmd.CommandText = "UPDATE " + Constants.JumpTable + " SET type = \"slCMJleft\" WHERE description LIKE \"%Left%\"";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		dbcmd.CommandText = "UPDATE " + Constants.JumpTable + " SET type = \"slCMJright\" WHERE description LIKE \"%Right%\"";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		dbcmd.CommandText = "UPDATE " + Constants.JumpTable + " SET description=replace(description, \" Left\", \"\")";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		dbcmd.CommandText = "UPDATE " + Constants.JumpTable + " SET description=replace(description, \" Right\", \"\")";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}

	//pass uniqueID value and then will return one record. do like this:
	//EncoderSQL eSQL = (EncoderSQL) SqliteEncoder.Select(false, myUniqueID, 0, 0, "")[0];
	//or
	//pass uniqueID==-1 and personID, sessionID, signalOrCurve values, and will return some records
	//personID can be -1 to get all on that session
	//sessionID can be -1 to get all sessions
	//signalOrCurve can be "all"
	public static ArrayList EncoderSelect103 (bool dbconOpened, 
			int uniqueID, int personID, int sessionID, string signalOrCurve, bool onlyActive)
	{
		if(! dbconOpened)
			Sqlite.Open();

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
				selectStr = personIDStr + sessionIDStr + " signalOrCurve = \"" + signalOrCurve + "\"";
		}

		string andString = "";
		if(selectStr != "")
			andString = " AND ";

		string onlyActiveString = "";
		if(onlyActive)
			onlyActiveString = " AND " + Constants.EncoderTable + ".status = \"active\" ";

		dbcmd.CommandText = "SELECT " + 
			Constants.EncoderTable + ".*, " + Constants.EncoderExerciseTable + ".name FROM " + 
			Constants.EncoderTable  + ", " + Constants.EncoderExerciseTable  + 
			" WHERE " + selectStr +
			andString + Constants.EncoderTable + ".exerciseID = " + 
				Constants.EncoderExerciseTable + ".uniqueID " +
				onlyActiveString +
			" ORDER BY substr(filename,-23,19)"; //this contains the date of capture signal

		LogB.SQL(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList array = new ArrayList(1);

		EncoderSQL103 es = new EncoderSQL103();
		while(reader.Read()) {
			es = new EncoderSQL103 (
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
					reader[14].ToString(),			//status
					reader[15].ToString(),			//videoURL
					reader[16].ToString(),			//encoderConfigurationName
					Convert.ToInt32(reader[17].ToString()),	//inertiaMomentum
					Convert.ToDouble(Util.ChangeDecimalSeparator(reader[18].ToString())), //diameter
					reader[19].ToString(),			//future1
					reader[20].ToString(),			//future2
					reader[21].ToString(),			//future3
					reader[22].ToString()			//EncoderExercise.name
					);
			array.Add (es);
		}
		reader.Close();
		if(! dbconOpened)
			Sqlite.Close();

		return array;
	}
	
	protected internal static void createTableEncoder104()
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
		       	"future1 TEXT, " + 
		       	"future2 TEXT, " + 
		       	"future3 TEXT )";
		dbcmd.ExecuteNonQuery();
	}

	protected internal static void createTableEncoder99()
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
    			"smooth INT, " +
			"description TEXT, " +
			"status TEXT, " +	//"active", "inactive"
			"videoURL TEXT, " +	//URL of video of signals
    			"mode TEXT, " +
    			"inertiaMomentum INT, " +
			"diameter INT, " +
		       	"future1 TEXT, " + 
		       	"future2 TEXT, " + 
		       	"future3 TEXT )";
		dbcmd.ExecuteNonQuery();
	}
	

	//pass uniqueID value and then will return one record. do like this:
	//EncoderSQL eSQL = (EncoderSQL) SqliteEncoder.Select(false, myUniqueID, 0, 0, "")[0];
	//or
	//pass uniqueID==-1 and personID, sessionID, signalOrCurve values, and will return some records
	//personID can be -1 to get all on that session
	//sessionID can be -1 to get all sessions
	//signalOrCurve can be "all"
	public static ArrayList EncoderSelect098 (bool dbconOpened, 
			int uniqueID, int personID, int sessionID, string signalOrCurve, bool onlyActive)
	{
		if(! dbconOpened)
			Sqlite.Open();

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
				selectStr = personIDStr + sessionIDStr + " signalOrCurve = \"" + signalOrCurve + "\"";
		}

		string andString = "";
		if(selectStr != "")
			andString = " AND ";

		string onlyActiveString = "";
		if(onlyActive)
			onlyActiveString = " AND " + Constants.EncoderTable + ".future1 = \"active\" ";

		dbcmd.CommandText = "SELECT " + 
			Constants.EncoderTable + ".*, " + Constants.EncoderExerciseTable + ".name FROM " + 
			Constants.EncoderTable  + ", " + Constants.EncoderExerciseTable  + 
			" WHERE " + selectStr +
			andString + Constants.EncoderTable + ".exerciseID = " + 
				Constants.EncoderExerciseTable + ".uniqueID " +
				onlyActiveString +
			" ORDER BY substr(filename,-23,19)"; //this contains the date of capture signal

		LogB.SQL(dbcmd.CommandText.ToString());
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		ArrayList array = new ArrayList(1);

		EncoderSQL098 es = new EncoderSQL098();
		while(reader.Read()) {
			es = new EncoderSQL098 (
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
			Sqlite.Close();

		return array;
	}

	//DB 1.17 -> 1.18	
	protected internal static void deleteNegativeRuns() 
	{
		dbcmd.CommandText = "Delete FROM " + Constants.RunIntervalTable +
			" WHERE timeTotal < 0";
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
	}
}

//used in DB version 1.03 and before
public class EncoderSQL103
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
	
	//encoderConfiguration conversions
	//in signals and curves, need to do conversions (invert, inertiaMomentum, diameter)
	public string encoderConfigurationName;
	public int inertiaMomentum; //kg*cm^2
	public double diameter;
	
	public string future1;
	public string future2;
	public string future3;

	public string exerciseName;
	
	public EncoderSQL103 ()
	{
	}
	
	public EncoderSQL103 (string uniqueID, int personID, int sessionID, int exerciseID, 
			string eccon, string laterality, string extraWeight, string signalOrCurve, 
			string filename, string url, int time, int minHeight, double smooth, 
			string description, string status, string videoURL, 
			string encoderConfigurationName, int inertiaMomentum, double diameter,
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
		this.encoderConfigurationName = encoderConfigurationName;
		this.inertiaMomentum = inertiaMomentum;
		this.diameter = diameter;
		this.future1 = future1;
		this.future2 = future2;
		this.future3 = future3;
		this.exerciseName = exerciseName;
	}
	
}


//used in DB version 0.98 and before
public class EncoderSQL098
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
	public string future1;	//active or inactive curves
	public string future2;	//URL of video of signals
	public string future3;	//Constants.EncoderSignalMode (only on signals) (add "-0.01" for inertia momentum)

	public string exerciseName;
	
	public EncoderSQL098 ()
	{
	}
	
	public EncoderSQL098 (string uniqueID, int personID, int sessionID, int exerciseID, 
			string eccon, string laterality, string extraWeight, string signalOrCurve, 
			string filename, string url, int time, int minHeight, double smooth, 
			string description, string future1, string future2, string future3, 
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
		this.future1 = future1;
		this.future2 = future2;
		this.future3 = future3;
		this.exerciseName = exerciseName;
	}
}

