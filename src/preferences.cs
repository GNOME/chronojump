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
 *  Copyright (C) 2004-2020   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;
using System.Collections.Generic; //List<T>

public class Preferences 
{
	/*
	 * these are sent to preferences window
	 */

	//appearance tab
	public enum MaximizedTypes { NO, YES, YESUNDECORATED};
	public MaximizedTypes maximized;
	public bool personWinHide;
	public bool personPhoto;
	//public string colorBackgroundString = "#0e1e46";
	public enum MenuTypes { ALL, TEXT, ICONS};
	public MenuTypes menuType;
	public string colorBackgroundString;
	public bool colorBackgroundIsDark; //this is assigned when colorBackgroundString changes. And this is used by the rest of the program. Not stored on SQL.
	public bool logoAnimatedShow;

	public enum UnitsEnum { METRIC, IMPERIAL };
	public UnitsEnum units;

	public bool encoderCaptureInfinite;
	public bool encoderCaptureShowOnlyBars;
	public int encoderCaptureShowNRepetitions;

	public bool showPower;
	public bool showStiffness;
	public bool showInitialSpeed;
	public bool showAngle;
	public bool showQIndex;
	public bool showDjIndex;
	public bool jumpsDjGraphHeights;
	public bool weightStatsPercent;		//AKA weightPercentPreferred
	public bool heightPreferred;
	public bool metersSecondsPreferred;
	public bool runSpeedStartArrival;

	public Constants.DoubleContact runDoubleContactsMode; //default LAST
	public int runDoubleContactsMS; //milliseconds
	public Constants.DoubleContact runIDoubleContactsMode; //As 1.8.1 is NONE or BIGGEST_TC
	public int runIDoubleContactsMS; //milliseconds

	public int thresholdJumps;
	public int thresholdRuns;
	public int thresholdOther;
	
	//encoder capture
	public int encoderCaptureTime;
	public int encoderCaptureInactivityEndTime; // -1 if not automatically end
	public int encoderCaptureMinHeightGravitatory;
	public int encoderCaptureMinHeightInertial;
	public int encoderCaptureInertialDiscardFirstN;
	public bool encoderCaptureCheckFullyExtended;
	public int encoderCaptureCheckFullyExtendedValue;
	public int encoderCaptureBarplotFontSize;
	public bool encoderShowStartAndDuration;
	public enum TriggerTypes { NO_TRIGGERS, START_AT_CAPTURE, START_AT_FIRST_ON};
	public TriggerTypes encoderCaptureCutByTriggers;
	
	//encoder other
	public bool encoderPropulsive;
	public bool encoderWorkKcal;
	public double encoderSmoothCon;
	public Constants.Encoder1RMMethod encoder1RMMethod;

	//multimedia
	public string videoDevice;
	public string videoDevicePixelFormat;
	public string videoDeviceResolution;
	public string videoDeviceFramerate; //cannot be a double because decimals seem exactly important on mac. if decimal will have always a '.' as needed by ffmpeg
	public int videoStopAfter;
	public string CSVExportDecimalSeparator;
	public string language;
	public string crashLogLanguage;
	public bool RGraphsTranslate;
	public bool useHeightsOnJumpIndexes;

	//forceSensor
	public int forceSensorCaptureWidthSeconds;
	public bool forceSensorCaptureScroll;
	public double forceSensorElasticEccMinDispl;
	public double forceSensorElasticConMinDispl;
	public int forceSensorNotElasticEccMinForce;
	public int forceSensorNotElasticConMinForce;
	public int forceSensorGraphsLineWidth;

	//runEncoder
	public double runEncoderMinAccel;

	public static string runEncoderAnalyzeAFPSqlNO = "NO";
	public static string runEncoderAnalyzeAFPSqlFITTED = "FITTED";
	public static string runEncoderAnalyzeAFPSqlRAW = "RAW";
	public static string runEncoderAnalyzeAFPSqlBOTH = "BOTH";

	public static List<string> runEncoderAnalyzeAFPSql_l = new List<string> {
		runEncoderAnalyzeAFPSqlNO, runEncoderAnalyzeAFPSqlFITTED,
		runEncoderAnalyzeAFPSqlRAW, runEncoderAnalyzeAFPSqlBOTH
	};

	public static LSqlEnTrans runEncoderAnalyzeAccel = new LSqlEnTrans(
			"runEncoderAnalyzeAccel",
			runEncoderAnalyzeAFPSql_l,
			1, 1,
			new List<string> {"No", "Fitted", "Raw", "Both"});
	public static LSqlEnTrans runEncoderAnalyzeForce = new LSqlEnTrans(
			"runEncoderAnalyzeForce",
			runEncoderAnalyzeAFPSql_l,
			1, 1,
			new List<string> {"No", "Fitted", "Raw", "Both"});
	public static LSqlEnTrans runEncoderAnalyzePower = new LSqlEnTrans(
			"runEncoderAnalyzePower",
			runEncoderAnalyzeAFPSql_l,
			1, 1,
			new List<string> {"No", "Fitted", "Raw", "Both"});

	//advanced tab
	public bool askDeletion;
	public int digitsNumber;
	public bool muteLogs;
	public bool networksAllowChangeDevices; //managed on preferences;

	public enum pythonVersionEnum { Python, Python2, Python3 };
	public pythonVersionEnum importerPythonVersion;

	/*
	 * these are NOT sent to preferences window
	 */
	
	public bool allowFinishRjAfterTime;
	public bool volumeOn;
	public bool videoOn;
	public int evaluatorServerID;
	public string versionAvailable;
	public string machineID;
	public Constants.MultimediaStorage multimediaStorage;
	public string databaseVersion;

	//encoder
	public Constants.EncoderAutoSaveCurve encoderAutoSaveCurve;
	public int encoderAutoSaveCurveBestNValue;
	//encoder rhythm
	public bool encoderRhythmActive;
	public bool encoderRhythmRepsOrPhases;
	public double encoderRhythmRepSeconds;
	public double encoderRhythmEccSeconds;
	public double encoderRhythmConSeconds;
	public double encoderRhythmRestRepsSeconds;
	public bool encoderRhythmRestAfterEcc;
	public int encoderRhythmRepsCluster;
	public double encoderRhythmRestClustersSeconds;

	public Constants.EncoderVariablesCapture encoderCaptureMainVariable;
	public Constants.EncoderVariablesCapture encoderCaptureSecondaryVariable;
	public bool encoderCaptureSecondaryVariableShow;
	public enum encoderCaptureEccOverloadModes { NOT_SHOW, SHOW_LINE, SHOW_LINE_AND_PERCENT };
	public encoderCaptureEccOverloadModes encoderCaptureInertialEccOverloadMode; //maybe on the future there is one not inertial
	public bool encoderCaptureMainVariableThisSetOrHistorical;
	public bool encoderCaptureMainVariableGreaterActive;
	public int encoderCaptureMainVariableGreaterValue;
	public bool encoderCaptureMainVariableLowerActive;
	public int encoderCaptureMainVariableLowerValue;
	public enum EncoderPhasesEnum { BOTH, ECC, CON}
	public EncoderPhasesEnum encoderCaptureFeedbackEccon;

	//forceSensor
	public string forceSensorTareDateTime;
	public double forceSensorTare;
	public string forceSensorCalibrationDateTime;
	public double forceSensorCalibrationWeight;
	public double forceSensorCalibrationFactor;
	public bool forceSensorCaptureFeedbackActive;
	public int forceSensorCaptureFeedbackAt;
	public int forceSensorCaptureFeedbackRange;
	public enum ForceSensorMIFDurationModes { SECONDS, PERCENT };
	public ForceSensorMIFDurationModes forceSensorMIFDurationMode;
	public double forceSensorMIFDurationSeconds;
	public int forceSensorMIFDurationPercent;
				
	public int encoderCaptureTimeIM = 180; //hardcoded 3 minutes.

	public enum GstreamerTypes { GST_0_1, GST_1_0, SYSTEMSOUNDS }; //SYSTEMSOUNDS is the default sounds played on Windows
	public GstreamerTypes gstreamer;
	public static string GstreamerStr = "gstreamer"; //in order to ensure write correctly on SQL

	public bool debugMode;

	/*
	 * these are unused on SqlitePreferences.SelectAll
	 */
	
	//public string chronopicPort;
	//public bool simulated;
	//public double encoderSmoothEccCon; 	//unused
	//public double inertialmomentum; 	//unused



	public Preferences() {
	}
	
	public static Preferences LoadAllFromSqlite() {
		return SqlitePreferences.SelectAll();
	}
	
	public int EncoderCaptureMinHeight(bool inertial) {
		if(inertial)
			return encoderCaptureMinHeightInertial;
		else
			return encoderCaptureMinHeightGravitatory;
	}
	
	public bool EncoderChangeMinHeight(bool inertial, int minHeight) 
	{
		bool changed = false;

		if(inertial && minHeight != encoderCaptureMinHeightInertial) 
		{
			encoderCaptureMinHeightInertial = minHeight;
			SqlitePreferences.Update("encoderCaptureMinHeightInertial", 
					minHeight.ToString(), false);
			changed = true;
		}
		else if(! inertial && minHeight != encoderCaptureMinHeightGravitatory)
		{
			encoderCaptureMinHeightGravitatory = minHeight;
			SqlitePreferences.Update("encoderCaptureMinHeightGravitatory", 
					minHeight.ToString(), false);
			changed = true;
		}
		return changed;
	}

	public void UpdateEncoderRhythm(EncoderRhythm er)
	{
		if(
				encoderRhythmActive != er.Active ||
				encoderRhythmRepsOrPhases != er.RepsOrPhases ||
				encoderRhythmRepSeconds != er.RepSeconds ||
				encoderRhythmEccSeconds != er.EccSeconds ||
				encoderRhythmConSeconds != er.ConSeconds ||
				encoderRhythmRestRepsSeconds != er.RestRepsSeconds ||
				encoderRhythmRestAfterEcc != er.RestAfterEcc ||
				encoderRhythmRepsCluster != er.RepsCluster ||
				encoderRhythmRestClustersSeconds != er.RestClustersSeconds
				)
			Sqlite.Open();
		else
			return;

		if(encoderRhythmActive != er.Active)
		{
			encoderRhythmActive = er.Active;
			SqlitePreferences.Update(SqlitePreferences.EncoderRhythmActiveStr,
					er.Active.ToString(), true); //bool
		}

		if(encoderRhythmRepsOrPhases != er.RepsOrPhases)
		{
			encoderRhythmRepsOrPhases = er.RepsOrPhases;
			SqlitePreferences.Update(SqlitePreferences.EncoderRhythmRepsOrPhasesStr,
					er.RepsOrPhases.ToString(), true); //bool
		}

		if(encoderRhythmRepSeconds != er.RepSeconds)
		{
			encoderRhythmRepSeconds = er.RepSeconds;
			SqlitePreferences.Update(SqlitePreferences.EncoderRhythmRepSecondsStr,
					Util.ConvertToPoint(er.RepSeconds), true); //double to point
		}

		if(encoderRhythmEccSeconds != er.EccSeconds)
		{
			encoderRhythmEccSeconds = er.EccSeconds;
			SqlitePreferences.Update(SqlitePreferences.EncoderRhythmEccSecondsStr,
					Util.ConvertToPoint(er.EccSeconds), true); //double to point
		}

		if(encoderRhythmConSeconds != er.ConSeconds)
		{
			encoderRhythmConSeconds = er.ConSeconds;
			SqlitePreferences.Update(SqlitePreferences.EncoderRhythmConSecondsStr,
					Util.ConvertToPoint(er.ConSeconds), true); //double to point
		}

		if(encoderRhythmRestRepsSeconds != er.RestRepsSeconds)
		{
			encoderRhythmRestRepsSeconds = er.RestRepsSeconds;
			SqlitePreferences.Update(SqlitePreferences.EncoderRhythmRestRepsSecondsStr,
					Util.ConvertToPoint(er.RestRepsSeconds), true); //double to point
		}

		if(encoderRhythmRestAfterEcc != er.RestAfterEcc)
		{
			encoderRhythmRestAfterEcc = er.RestAfterEcc;
			SqlitePreferences.Update(SqlitePreferences.EncoderRhythmRestAfterEccStr,
					er.RestAfterEcc.ToString(), true); //bool
		}

		if(encoderRhythmRepsCluster != er.RepsCluster)
		{
			encoderRhythmRepsCluster = er.RepsCluster;
			SqlitePreferences.Update(SqlitePreferences.EncoderRhythmRepsClusterStr,
					er.RepsCluster.ToString(), true); //int
		}

		if(encoderRhythmRestClustersSeconds != er.RestClustersSeconds)
		{
			encoderRhythmRestClustersSeconds = er.RestClustersSeconds;
			SqlitePreferences.Update(SqlitePreferences.EncoderRhythmRestClustersSecondsStr,
					Util.ConvertToPoint(er.RestClustersSeconds), true); //double to point
		}

		Sqlite.Close();
	}

	//force sensor
	public void UpdateForceSensorTare(double tare)
	{
		if(tare == -1)
			return;

		//change preferences object and SqlitePreferences
		forceSensorTareDateTime = UtilDate.ToFile(DateTime.Now);
		SqlitePreferences.Update(SqlitePreferences.ForceSensorTareDateTimeStr, forceSensorTareDateTime, false);

		forceSensorTare = tare;
		SqlitePreferences.Update(SqlitePreferences.ForceSensorTareStr, Util.ConvertToPoint(tare), false);
	}

	//force sensor
	public void UpdateForceSensorCalibration(double weight, double calibrationFactor)
	{
		if(calibrationFactor == -1)
			return;

		//change preferences object and SqlitePreferences
		DateTime dt = DateTime.Now;

		forceSensorCalibrationDateTime = UtilDate.ToFile(dt);
		SqlitePreferences.Update(SqlitePreferences.ForceSensorCalibrationDateTimeStr, forceSensorCalibrationDateTime, false);

		forceSensorCalibrationWeight = weight;
		SqlitePreferences.Update(SqlitePreferences.ForceSensorCalibrationWeightStr, Util.ConvertToPoint(weight), false);

		forceSensorCalibrationFactor = calibrationFactor;
		SqlitePreferences.Update(SqlitePreferences.ForceSensorCalibrationFactorStr, Util.ConvertToPoint(calibrationFactor), false);
	}

	public string GetForceSensorAdjustString()
	{
		return "\nLast tare:" +
			"\n\t- internal value: " + forceSensorTare.ToString() +
			"\n\t- at: " + forceSensorTareDateTime +
			"\n\nLast Calibrate:" +
			"\n\t- internal value: " + forceSensorCalibrationFactor.ToString() +
			"\n\t- with: " + forceSensorCalibrationWeight.ToString() +
			" Kg\n\t- at: " + forceSensorCalibrationDateTime +
			"\n\nNote this information is related only to the tares and calibrations of any force sensor on this machine.";
	}

	public char CSVColumnDelimiter
	{
		get {
			char columnDelimiter = ',';
			if(CSVExportDecimalSeparator == "COMMA")
				columnDelimiter = ';';

			return columnDelimiter;
		}
	}

	public bool IsVideoConfigured()
	{
		return (videoDevice != "" &&
				videoDevicePixelFormat != "" &&
				videoDeviceResolution != "" //&&
				//videoDeviceFramerate != "" 	//a really old Creative camera on a Linux machine does not return the framerate, so allow to be configured without it
				);
	}

	//this methods update the SQL and returns the value that is assigned to preferences object
	public static bool PreferencesChange(string prefName, bool prefValue, bool bNew)
	{
		if(prefValue != bNew)
			SqlitePreferences.Update(prefName, bNew.ToString(), true);

		return bNew;
	}
	public static int PreferencesChange(string prefName, int prefValue, int iNew)
	{
		if(prefValue != iNew)
			SqlitePreferences.Update(prefName, iNew.ToString(), true);

		return iNew;
	}
	public static double PreferencesChange(string prefName, double prefValue, double dNew)
	{
		if(prefValue != dNew)
			SqlitePreferences.Update(prefName, Util.ConvertToPoint(dNew), true);

		return dNew;
	}
	public static string PreferencesChange(string prefName, string prefValue, string sNew)
	{
		if(prefValue != sNew)
			SqlitePreferences.Update(prefName, sNew, true);

		return sNew;
	}

	~Preferences() {}
	   
}
