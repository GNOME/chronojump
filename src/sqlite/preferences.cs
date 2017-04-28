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
using Mono.Data.Sqlite;
using System.Collections; //ArrayList


class SqlitePreferences : Sqlite
{
	public const string EncoderExerciseIDGravitatory = "encoderExerciseIDGravitatory";
	public const string EncoderExerciseIDInertial = "encoderExerciseIDInertial";
	public const string EncoderContractionGravitatory = "encoderContractionGravitatory";
	public const string EncoderContractionInertial = "encoderContractionInertial";
	public const string EncoderLateralityGravitatory = "encoderLateralityGravitatory";
	public const string EncoderLateralityInertial = "encoderLateralityInertial";
	public const string EncoderMassGravitatory = "encoderMassGravitatory";
	public const string EncoderWeightsInertial = "encoderWeightsInertial";

	protected internal static new void createTable()
	{
		dbcmd.CommandText = 
			"CREATE TABLE " + Constants.PreferencesTable + " ( " +
			"name TEXT, " +
			"value TEXT) ";
		dbcmd.ExecuteNonQuery();
	}
	
	protected internal static void initializeTable(string databaseVersion, bool creatingBlankDatabase)
	{
		using(SqliteTransaction tr = dbcon.BeginTransaction())
		{
			using (SqliteCommand dbcmdTr = dbcon.CreateCommand())
			{
				dbcmdTr.Transaction = tr;

				Insert ("databaseVersion", databaseVersion, dbcmdTr); 

				if(UtilAll.IsWindows() || creatingBlankDatabase)
					Insert ("chronopicPort", Constants.ChronopicDefaultPortWindows, dbcmdTr);
				else
					Insert ("chronopicPort", Constants.ChronopicDefaultPortLinux, dbcmdTr);

				//appearance
				Insert ("maximized", Preferences.MaximizedTypes.NO.ToString(), dbcmdTr);
				Insert ("personWinHide", "False", dbcmdTr);
				Insert ("encoderCaptureShowOnlyBars", "False", dbcmdTr);
				Insert ("digitsNumber", "3", dbcmdTr);
				Insert ("showPower", "True", dbcmdTr);
				Insert ("showStiffness", "True", dbcmdTr);
				Insert ("showInitialSpeed", "True", dbcmdTr);
				Insert ("showAngle", "False", dbcmdTr); //for treeviewjumps
				Insert ("showQIndex", "False", dbcmdTr); //for treeviewJumps
				Insert ("showDjIndex", "False", dbcmdTr); //for treeviewJumps
				Insert ("simulated", "True", dbcmdTr);
				Insert ("weightStatsPercent", "False", dbcmdTr);
				Insert ("askDeletion", "True", dbcmdTr);
				Insert ("heightPreferred", "False", dbcmdTr);
				Insert ("metersSecondsPreferred", "True", dbcmdTr);
				Insert ("language", "", dbcmdTr); 
				Insert ("allowFinishRjAfterTime", "True", dbcmdTr); 
				Insert ("volumeOn", "True", dbcmdTr); 
				Insert ("videoOn", "True", dbcmdTr); 
				Insert ("evaluatorServerID", "-1", dbcmdTr);
				Insert ("versionAvailable", "", dbcmdTr);
				Insert ("runSpeedStartArrival", "True", dbcmdTr);

				Insert ("runDoubleContactsMode", 
						Constants.DoubleContact.LAST.ToString(), dbcmdTr); 
				Insert ("runDoubleContactsMS", "300", dbcmdTr);
				Insert ("runIDoubleContactsMode", 
						Constants.DoubleContact.AVERAGE.ToString(), dbcmdTr); 
				Insert ("runIDoubleContactsMS", "300", dbcmdTr);

				Insert ("thresholdJumps", "50", dbcmdTr);
				Insert ("thresholdRuns", "10", dbcmdTr);
				Insert ("thresholdOther", "50", dbcmdTr);

				Random rnd = new Random();
				string machineID = rnd.Next().ToString();
				Insert ("machineID", machineID, dbcmdTr);

				Insert ("multimediaStorage", Constants.MultimediaStorage.BYSESSION.ToString(), dbcmdTr);

				//encoder
				Insert ("encoderCaptureTime", "60", dbcmdTr);
				Insert ("encoderCaptureInactivityEndTime", "3", dbcmdTr);
				Insert ("encoderCaptureMainVariable", Constants.EncoderVariablesCapture.MeanPower.ToString(), dbcmdTr);
				Insert ("encoderCaptureMinHeightGravitatory", "20", dbcmdTr);
				Insert ("encoderCaptureMinHeightInertial", "5", dbcmdTr);
				Insert ("encoderCaptureCheckFullyExtended", "True", dbcmdTr);
				Insert ("encoderCaptureCheckFullyExtendedValue", "4", dbcmdTr);
				Insert ("encoderShowStartAndDuration", "False", dbcmdTr);
				Insert ("encoderCaptureCutByTriggers", "False", dbcmdTr);
				Insert ("encoderPropulsive", "True", dbcmdTr);
				Insert ("encoderSmoothEccCon", "0.6", dbcmdTr);
				Insert ("encoderSmoothCon", "0.7", dbcmdTr);
				Insert ("encoder1RMMethod", Constants.Encoder1RMMethod.WEIGHTED2.ToString(), dbcmdTr);

				ArrayList encoderExercises =
					SqliteEncoder.SelectEncoderExercises(true, -1, true);

				if(encoderExercises.Count > 0) {
					EncoderExercise ex = (EncoderExercise) encoderExercises[0];
					Insert (EncoderExerciseIDGravitatory, ex.uniqueID.ToString());
					Insert (EncoderExerciseIDInertial, ex.uniqueID.ToString());
				}
				else {
					Insert (EncoderExerciseIDGravitatory, "1");
					Insert (EncoderExerciseIDInertial, "1");
				}

				Insert (EncoderContractionGravitatory, Constants.Concentric);
				Insert (EncoderContractionInertial, Constants.EccentricConcentric);
				Insert (EncoderLateralityGravitatory, "RL");
				Insert (EncoderLateralityInertial, "RL");
				Insert (EncoderMassGravitatory, "10");
				Insert (EncoderWeightsInertial, "0");

				Insert ("videoDevice", "0", dbcmdTr); //first
				Insert ("inertialmomentum", "0.01", dbcmdTr);
				Insert ("CSVExportDecimalSeparator", Util.GetDecimalSeparatorFromLocale(), dbcmdTr);
				Insert ("RGraphsTranslate", "True", dbcmdTr);
				Insert ("useHeightsOnJumpIndexes", "True", dbcmdTr);
				Insert ("encoderAutoSaveCurve", Constants.EncoderAutoSaveCurve.BEST.ToString(), dbcmdTr); 
				Insert ("email", "", dbcmdTr);
				
				//removed on 1.37
				//Insert ("encoderConfiguration", new EncoderConfiguration().ToStringOutput(EncoderConfiguration.Outputs.SQL), dbcmdTr);

			}
			tr.Commit();
		}
	}

	//called from some Chronojump methods
	//adds dbcmd to be used on next Insert method
	public static void Insert(string myName, string myValue)
	{
		Insert(myName, myValue, dbcmd);
	}
	//Called from initialize
	public static void Insert(string myName, string myValue, SqliteCommand mycmd)
	{
		//Sqlite.Open();
		mycmd.CommandText = "INSERT INTO " + Constants.PreferencesTable + 
			" (name, value) VALUES (\"" + 
			myName + "\", \"" + myValue + "\")" ;
		LogB.SQL(mycmd.CommandText.ToString());
		mycmd.ExecuteNonQuery();
		//Sqlite.Close();
	}

	public static void Update(string myName, string myValue, bool dbconOpened)
	{
		if(! dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "UPDATE " + Constants.PreferencesTable +
			" SET value = \"" + myValue + 
			"\" WHERE name == \"" + myName + "\"" ;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		if(! dbconOpened)
			Sqlite.Close();
	}

	//Called from most of all old Chronojump methods
	public static string Select (string myName) 
	{
		return Select(myName, false);
	}
	//Called from new methods were dbcon is opened
	public static string Select (string myName, bool dbconOpened) 
	{
		if(! dbconOpened)
			Sqlite.Open();

		dbcmd.CommandText = "SELECT value FROM " + Constants.PreferencesTable + 
			" WHERE name == \"" + myName + "\"" ;
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		//SqliteDataReader reader;
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		string myReturn = "0";
	
		if(reader.Read()) {
			myReturn = reader[0].ToString();
		}
		reader.Close();
		
		if(! dbconOpened)
			Sqlite.Close();

		return myReturn;
	}
	
	public static Preferences SelectAll () 
	{
		Sqlite.Open();
		dbcmd.CommandText = "SELECT * FROM " + Constants.PreferencesTable; 
		LogB.SQL(dbcmd.CommandText.ToString());
		dbcmd.ExecuteNonQuery();
		
		SqliteDataReader reader;
		reader = dbcmd.ExecuteReader();

		Preferences preferences = new Preferences();

		while(reader.Read()) {
			//LogB.Debug("Reading preferences");
			//LogB.Information(reader[0].ToString() + ":" + reader[1].ToString());

	 		//these are sent to preferences window
			if(reader[0].ToString() == "maximized")
				preferences.maximized = (Preferences.MaximizedTypes)
					Enum.Parse(typeof(Preferences.MaximizedTypes), reader[1].ToString());
			else if(reader[0].ToString() == "personWinHide")
				preferences.personWinHide = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "encoderCaptureShowOnlyBars")
				preferences.encoderCaptureShowOnlyBars = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "digitsNumber")
				preferences.digitsNumber = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == "showPower")
				preferences.showPower = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "showStiffness")
				preferences.showStiffness = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "showInitialSpeed")
				preferences.showInitialSpeed = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "showAngle")
				preferences.showAngle = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "showQIndex")
				preferences.showQIndex = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "showDjIndex")
				preferences.showDjIndex = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "askDeletion")
				preferences.askDeletion = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "weightStatsPercent")
				preferences.weightStatsPercent = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "heightPreferred")
				preferences.heightPreferred = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "metersSecondsPreferred")
				preferences.metersSecondsPreferred = reader[1].ToString() == "True";
			//encoder capture
			else if(reader[0].ToString() == "encoderCaptureTime")
				preferences.encoderCaptureTime = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == "encoderCaptureInactivityEndTime")
				preferences.encoderCaptureInactivityEndTime = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == "encoderCaptureMainVariable")
				preferences.encoderCaptureMainVariable = (Constants.EncoderVariablesCapture) 
					Enum.Parse(typeof(Constants.EncoderVariablesCapture), reader[1].ToString()); 
			else if(reader[0].ToString() == "encoderCaptureMinHeightGravitatory")
				preferences.encoderCaptureMinHeightGravitatory = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == "encoderCaptureMinHeightInertial")
				preferences.encoderCaptureMinHeightInertial = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == "encoderCaptureCheckFullyExtended")
				preferences.encoderCaptureCheckFullyExtended = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "encoderCaptureCheckFullyExtendedValue")
				preferences.encoderCaptureCheckFullyExtendedValue = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == "encoderAutoSaveCurve")
				preferences.encoderAutoSaveCurve = (Constants.EncoderAutoSaveCurve) 
					Enum.Parse(typeof(Constants.EncoderAutoSaveCurve), reader[1].ToString()); 
			else if(reader[0].ToString() == "encoderShowStartAndDuration")
				preferences.encoderShowStartAndDuration = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "encoderCaptureCutByTriggers")
				preferences.encoderCaptureCutByTriggers = reader[1].ToString() == "True";
			//encoder other
			else if(reader[0].ToString() == "encoderPropulsive")
				preferences.encoderPropulsive = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "encoderSmoothCon")
				preferences.encoderSmoothCon = Convert.ToDouble(
						Util.ChangeDecimalSeparator(reader[1].ToString()));
			else if(reader[0].ToString() == "encoder1RMMethod")
				preferences.encoder1RMMethod = (Constants.Encoder1RMMethod) 
					Enum.Parse(typeof(Constants.Encoder1RMMethod), reader[1].ToString()); 
			//video... other
			else if(reader[0].ToString() == "videoDevice")
				preferences.videoDeviceNum = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == "CSVExportDecimalSeparator")
				preferences.CSVExportDecimalSeparator = reader[1].ToString();
			else if(reader[0].ToString() == "language")
				preferences.language = reader[1].ToString();
			else if(reader[0].ToString() == "RGraphsTranslate")
				preferences.RGraphsTranslate = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "useHeightsOnJumpIndexes")
				preferences.useHeightsOnJumpIndexes = reader[1].ToString() == "True";
	 		//these are NOT sent to preferences window
			else if(reader[0].ToString() == "allowFinishRjAfterTime")
				preferences.allowFinishRjAfterTime = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "volumeOn")
				preferences.volumeOn = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "videoOn")
				preferences.videoOn = reader[1].ToString() == "True";
			else if(reader[0].ToString() == "evaluatorServerID")
				preferences.evaluatorServerID = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == "versionAvailable")
				preferences.versionAvailable = reader[1].ToString();
			else if(reader[0].ToString() == "runSpeedStartArrival")
				preferences.runSpeedStartArrival = reader[1].ToString() == "True";
			
			else if(reader[0].ToString() == "runDoubleContactsMode")
				preferences.runDoubleContactsMode = (Constants.DoubleContact) 
					Enum.Parse(typeof(Constants.DoubleContact), reader[1].ToString()); 
			else if(reader[0].ToString() == "runDoubleContactsMS")
				preferences.runDoubleContactsMS = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == "runIDoubleContactsMode")
				preferences.runIDoubleContactsMode = (Constants.DoubleContact) 
					Enum.Parse(typeof(Constants.DoubleContact), reader[1].ToString()); 
			else if(reader[0].ToString() == "runIDoubleContactsMS")
				preferences.runIDoubleContactsMS = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == "thresholdJumps")
				preferences.thresholdJumps = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == "thresholdRuns")
				preferences.thresholdRuns = Convert.ToInt32(reader[1].ToString());
			else if(reader[0].ToString() == "thresholdOther")
				preferences.thresholdOther = Convert.ToInt32(reader[1].ToString());

			else if(reader[0].ToString() == "machineID")
				preferences.machineID = reader[1].ToString();
			else if(reader[0].ToString() == "multimediaStorage")
				preferences.multimediaStorage = (Constants.MultimediaStorage) 
					Enum.Parse(typeof(Constants.MultimediaStorage), reader[1].ToString()); 
			else if(reader[0].ToString() == "databaseVersion")
				preferences.databaseVersion = reader[1].ToString();
		}

		reader.Close();
		Sqlite.Close();

		return preferences;
	}
}

