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
			"smooth FLOAT, " +  
			"description TEXT, " +
			"future1 TEXT, " +
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
				" (uniqueID, personID, sessionID, exerciseID, eccon, laterality, extraWeight, signalOrCurve, filename, url, time, minHeight, smooth, description, future1, future2, future3)" +
				" VALUES (" + es.uniqueID + ", " +
				es.personID + ", " + es.sessionID + ", " +
				es.exerciseID + ", '" + es.eccon + "', '" +
				es.laterality + "', '" + es.extraWeight + "', '" +
				es.signalOrCurve + "', '" + es.filename + "', '" +
				es.url + "', " + es.time + ", " + es.minHeight + ", " +
				Util.ConvertToPoint(es.smooth) + ", '" + es.description + "', '', '', '')" ;
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
				", smooth = " + Util.ConvertToPoint(es.smooth) +
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
	public static ArrayList Select (bool dbconOpened, 
			int uniqueID, int personID, int sessionID, string signalOrCurve)
	{
		if(! dbconOpened)
			dbcon.Open();

		string selectStr = "";
		if(uniqueID != -1)
			selectStr = Constants.EncoderTable + ".uniqueID = " + uniqueID;
		else
			selectStr = "personID = " + personID + " AND sessionID = " + sessionID + 
			" AND signalOrCurve = '" + signalOrCurve + "'";

		dbcmd.CommandText = "SELECT " + 
			Constants.EncoderTable + ".*, " + Constants.EncoderExerciseTable + ".name FROM " + 
			Constants.EncoderTable  + ", " + Constants.EncoderExerciseTable  + 
			" WHERE " + selectStr +
			" AND " + Constants.EncoderTable + ".exerciseID = " + 
			Constants.EncoderExerciseTable + ".uniqueID ";

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
					Convert.ToDouble(Util.ChangeDecimalSeparator(reader[12].ToString())), //smooth
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
			"future1 TEXT, " +
			"future2 TEXT, " +
			"future3 TEXT )";
		dbcmd.ExecuteNonQuery();
	}
	
	public static void InsertExercise(bool dbconOpened, string name, int percentBodyWeight, 
			string ressistance, string description)
	{
		if(! dbconOpened)
			dbcon.Open();

		dbcmd.CommandText = "INSERT INTO " + Constants.EncoderExerciseTable +  
				" (uniqueID, name, percentBodyWeight, ressistance, description, future1, future2, future3)" +
				" VALUES (NULL, '" + name + "', " + percentBodyWeight + ", '" + 
				ressistance + "', '" + description + "', '','','')";
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
			//name:percentBodyWeight:ressistance:description
			"Bench press:0:weight bar:", 
			"Squat:75:weight bar:", 
			"Jump:100:none:"
		};
		
		foreach(string line in iniEncoderExercises) {
			string [] parts = line.Split(new char[] {':'});
			InsertExercise(false, parts[0], Convert.ToInt32(parts[1]), parts[2], parts[3]);
		}
	}

	//if submited a -1, returns an especific EncoderExercise that can be read like this	
	//EncoderExercise ex = (EncoderExercise) SqliteEncoder.SelectEncoderExercises(eSQL.exerciseID)[0];
	public static ArrayList SelectEncoderExercises(int uniqueID, bool onlyNames) 
	{
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
				ex = new EncoderExercise (
						Convert.ToInt32(reader[0].ToString()),	//uniqueID
						reader[1].ToString(),			//name
						Convert.ToInt32(reader[2].ToString()),	//percentBodyWeight
						reader[3].ToString(),			//ressistance
						reader[4].ToString()			//description
						);
				array.Add(ex);
			}
		}

		reader.Close();
		dbcon.Close();

		return array;
	}

}
