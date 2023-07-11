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
 *  Copyright (C) 2004-2023   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Data;
using System.Collections.Generic; //List<T>

public class Preferences 
{
	/*
	 * these are managed on preferences window
	 */

	//main tab
	public enum MaximizedTypes { NO, YES, YESUNDECORATED};
	public MaximizedTypes maximized;
	public bool personWinHide;
	public bool personPhoto;
	//public string colorBackgroundString = "#0e1e46";

	public enum MenuTypes { ALL, TEXT, ICONS}; 	//unused on 2.1.3
	public MenuTypes menuType;			//unused on 2.1.3

	public string colorBackgroundString; //"" means system color (do not do nothing)
	public bool colorBackgroundOsColor;
	public bool logoAnimatedShow;
	public enum FontTypes { Courier, Helvetica};
	public FontTypes fontType;
	public int restTimeMinutes; //-1 deactivated minutes and seconds
	public int restTimeSeconds;

	public string GetFontTypeWithSize(int size) {
		return string.Format("{0} {1}", fontType, size);
	}

	public bool loadLastSessionAtStart;
	public int lastSessionID;
	public bool loadLastModeAtStart;
	public Constants.Modes lastMode;
	public SessionLoadDisplay sessionLoadDisplay;
	public int lastPersonID;

	public enum UnitsEnum { METRIC, IMPERIAL };
	public UnitsEnum units;

	public ContactsCaptureDisplay contactsCaptureDisplay;
	public ContactsCaptureDisplay contactsCaptureDisplayStored; //to update sql on exit if changed

	public bool encoderCaptureInfinite;

	public EncoderCaptureDisplay encoderCaptureShowOnlyBars;
	public EncoderCaptureDisplay encoderCaptureShowOnlyBarsStored; //to update sql on exit if changed

	public int encoderCaptureShowNRepetitions;

	public bool showPower;
	public bool showStiffness;
	public bool showInitialSpeed;
	public bool showJumpRSI;
	public bool showAngle; //unused
	public bool showQIndex;
	public bool showDjIndex;
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

	public enum EncoderRepetitionCriteria { ECC_CON, ECC, CON };
	public EncoderRepetitionCriteria encoderRepetitionCriteriaGravitatory;
	public EncoderRepetitionCriteria encoderRepetitionCriteriaInertial;
	public EncoderRepetitionCriteria GetEncoderRepetitionCriteria (Constants.Modes mode)
	{
		if (mode == Constants.Modes.POWERINERTIAL)
			return encoderRepetitionCriteriaInertial;
		else
			return encoderRepetitionCriteriaGravitatory;
	}
	public EncoderRepetitionCriteria GetEncoderRepetitionCriteria (bool inertial)
	{
		if (inertial)
			return encoderRepetitionCriteriaInertial;
		else
			return encoderRepetitionCriteriaGravitatory;
	}
	
	//encoder other
	public bool encoderPropulsive;
	public bool encoderWorkKcal;
	public enum EncoderInertialGraphsXTypes { EQUIVALENT_MASS, INERTIA_MOMENT, DIAMETER };
	public EncoderInertialGraphsXTypes encoderInertialGraphsX;
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

	//jumpsRjFeedback
	public bool jumpsRjFeedbackShowBestTvTc;	//implemented
	public bool jumpsRjFeedbackShowWorstTvTc; 	//implemented
	public bool jumpsRjFeedbackHeightGreaterActive;
	public bool jumpsRjFeedbackHeightLowerActive;
	public bool jumpsRjFeedbackTvGreaterActive; 	//implemented
	public bool jumpsRjFeedbackTvLowerActive; 	//implemented
	public bool jumpsRjFeedbackTcGreaterActive; 	//implemented
	public bool jumpsRjFeedbackTcLowerActive; 	//implemented
	public bool jumpsRjFeedbackTvTcGreaterActive;
	public bool jumpsRjFeedbackTvTcLowerActive;
	public double jumpsRjFeedbackHeightGreater;
	public double jumpsRjFeedbackHeightLower;
	public double jumpsRjFeedbackTvGreater; 	//implemented
	public double jumpsRjFeedbackTvLower; 		//implemented
	public double jumpsRjFeedbackTcGreater; 	//implemented
	public double jumpsRjFeedbackTcLower; 		//implemented
	public double jumpsRjFeedbackTvTcGreater;
	public double jumpsRjFeedbackTvTcLower;

	//runsIFeedback
	public bool runsIFeedbackShowBestSpeed; //speed
	public bool runsIFeedbackShowWorstSpeed; //speed
	public bool runsIFeedbackShowBest; //time
	public bool runsIFeedbackShowWorst; //time
	public bool runsIFeedbackTimeGreaterActive; 	//implemented
	public bool runsIFeedbackTimeLowerActive; 	//implemented
	public bool runsIFeedbackSpeedGreaterActive; 	//implemented
	public bool runsIFeedbackSpeedLowerActive; 	//implemented
	public double runsIFeedbackTimeGreater; 	//implemented
	public double runsIFeedbackTimeLower; 		//implemented
	public double runsIFeedbackSpeedGreater; 	//implemented
	public double runsIFeedbackSpeedLower; 		//implemented

	//forceSensor
	public int forceSensorCaptureWidthSeconds;
	public bool forceSensorCaptureScroll;
	public double forceSensorElasticEccMinDispl;
	public double forceSensorElasticConMinDispl;
	public int forceSensorNotElasticEccMinForce;
	public int forceSensorNotElasticConMinForce;
	public enum VariabilityMethodEnum { CHRONOJUMP_OLD, CV, RMSSD, CVRMSSD };
	public VariabilityMethodEnum forceSensorVariabilityMethod;
	public int forceSensorVariabilityLag;
	public double forceSensorAnalyzeABSliderIncrement;
	public double forceSensorAnalyzeMaxAVGInWindow; //seconds
	public int forceSensorGraphsLineWidth;

	//runEncoder
	public bool runEncoderCaptureDisplaySimple;
	public bool runEncoderCaptureDisplaySimpleStored;

	public double runEncoderMinAccel;
	public double runEncoderPPS;

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

	public enum pythonVersionEnum { Python2, Python3 };
	public pythonVersionEnum importerPythonVersion;

	/*
	 * at DB: 2.06, vales pythonVersionEnum were Python2, Python3, we do not use "Python" anymore
	 *
	 * at DB: 1.95, vales pythonVersionEnum were Python, Python2, Python3
	 * so we need the executable: python, python2, python3
	 * chronojump_importer.py works on python2 and python3
	 */
	public static string GetPythonExecutable (pythonVersionEnum pv)
	{
		if(pv == pythonVersionEnum.Python2)
			return "python2";
		else if(pv == pythonVersionEnum.Python3)
			return "python3";

		return "python";
	}

	/*
	 * these are NOT managed on preferences window
	 */

	public bool personSelectWinImages;
	public bool allowFinishRjAfterTime;
	public bool volumeOn;
	public bool videoOn;
	public int evaluatorServerID;
	public string versionAvailable;
	public string machineID;
	public Constants.MultimediaStorage multimediaStorage;
	public string databaseVersion;

	//backup
	public string lastBackupDir;
	public DateTime lastBackupDatetime; 	// merely informational
	public DateTime backupScheduledCreatedDate;
	public int backupScheduledNextDays;

	//news
	public bool newsLanguageEs; 		// on SQL
	public string serverNewsDatetime;  	// NOT on SQL
	public string clientNewsDatetime; 	// on SQL

	//socialNetwork poll
	public string socialNetwork;
	public string socialNetworkDatetime; // "": not answered, -1: it should be sent when there's network (after a ping)

	//jumps
	public bool jumpsFVProfileOnlyBestInWeight;
	public bool jumpsFVProfileShowFullGraph;
	public bool jumpsEvolutionOnlyBestInSession;

	//runs
	public bool runsEvolutionOnlyBestInSession;
	public bool runsEvolutionShowTime;

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
	public bool encoderCaptureShowLoss;
	public bool encoderFeedbackAsteroidsActive;

	//forceSensor
	public string forceSensorTareDateTime;
	public double forceSensorTare;
	public string forceSensorCalibrationDateTime;
	public double forceSensorCalibrationWeight;
	public double forceSensorCalibrationFactor;
	public bool forceSensorStartEndOptimized;
	public enum ForceSensorMIFDurationModes { SECONDS, PERCENT };
	public ForceSensorMIFDurationModes forceSensorMIFDurationMode;
	public double forceSensorMIFDurationSeconds;
	public int forceSensorMIFDurationPercent;

	//forceSensor feedback
	public enum ForceSensorCaptureFeedbackActiveEnum { NO, RECTANGLE, PATH, ASTEROIDS, QUESTIONNAIRE };
	//rectangle
	public ForceSensorCaptureFeedbackActiveEnum forceSensorCaptureFeedbackActive;
	public int forceSensorCaptureFeedbackAt;
	public int forceSensorCaptureFeedbackRange;
	//path
	public int forceSensorFeedbackPathMax;
	public int forceSensorFeedbackPathMin;
	public int forceSensorFeedbackPathMasters;
	public int forceSensorFeedbackPathMasterSeconds;
	public int forceSensorFeedbackPathLineWidth;
	//asteroiods (TODO: as they are used also for encoder, remove "forceSensor")
	public int forceSensorFeedbackAsteroidsMax = 100;
	public int forceSensorFeedbackAsteroidsMin = 0;
	public bool forceSensorFeedbackAsteroidsDark = true;
	public int forceSensorFeedbackAsteroidsFrequency = 1;
	public int forceSensorFeedbackShotsFrequency = 1;
	//questionnaire
	public int forceSensorFeedbackQuestionnaireMax = 100;
	public int forceSensorFeedbackQuestionnaireMin = 0;
	public int forceSensorFeedbackQuestionnaireN = 10;
	public string forceSensorFeedbackQuestionnaireFile = ""; //if default will be blank
	//signal direction
	public bool signalDirectionHorizontal = true;

	public int encoderCaptureTimeIM = 180; //hardcoded 3 minutes.

	public enum GstreamerTypes { GST_0_1, GST_1_0, FFPLAY, SYSTEMSOUNDS };
	//SYSTEMSOUNDS is the default sounds played on Windows, FFPLAY on mac (to avoid installing gstreamer)

	public GstreamerTypes gstreamer;
	public static string GstreamerStr = "gstreamer"; //in order to ensure write correctly on SQL

	public bool debugMode;

	//export
	public int exportGraphWidth;
	public int exportGraphHeight;

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
				encoderRhythmActive != er.ActiveRhythm ||
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

		if(encoderRhythmActive != er.ActiveRhythm)
		{
			encoderRhythmActive = er.ActiveRhythm;
			SqlitePreferences.Update(SqlitePreferences.EncoderRhythmActiveStr,
					er.ActiveRhythm.ToString(), true); //bool
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

	//get the decimalChar for R . or ,
	public char CSVExportDecimalSeparatorChar
	{
		get {
			char c = '.';
			if(CSVExportDecimalSeparator == "COMMA")
				c = ',';

			return c;
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
	public static bool PreferencesChange (bool dbconOpened, string prefName, bool prefValue, bool bNew)
	{
		if(prefValue != bNew)
			SqlitePreferences.Update(prefName, bNew.ToString(), dbconOpened);

		return bNew;
	}
	public static int PreferencesChange (bool dbconOpened, string prefName, int prefValue, int iNew)
	{
		if(prefValue != iNew)
			SqlitePreferences.Update(prefName, iNew.ToString(), dbconOpened);

		return iNew;
	}
	public static double PreferencesChange (bool dbconOpened, string prefName, double prefValue, double dNew)
	{
		if(prefValue != dNew)
			SqlitePreferences.Update(prefName, Util.ConvertToPoint(dNew), dbconOpened);

		return dNew;
	}
	public static string PreferencesChange (bool dbconOpened, string prefName, string prefValue, string sNew)
	{
		if(prefValue != sNew)
			SqlitePreferences.Update(prefName, sNew, dbconOpened);

		return sNew;
	}

	public Gdk.RGBA colorBackground
	{
		get { return UtilGtk.ColorParse(colorBackgroundString); }
	}

	public enum RunEncoderPlotVariables { RAWACCEL, FITTEDACCEL, RAWFORCE, FITTEDFORCE, RAWPOWER, FITTEDPOWER};
	public static bool RunEncoderShouldPlotVariable(RunEncoderPlotVariables v)
	{
		if(v == RunEncoderPlotVariables.RAWACCEL)
			return ( runEncoderAnalyzeAccel.SqlCurrentName == runEncoderAnalyzeAFPSqlRAW ||
					runEncoderAnalyzeAccel.SqlCurrentName == runEncoderAnalyzeAFPSqlBOTH );
		else if(v == RunEncoderPlotVariables.FITTEDACCEL)
			return ( runEncoderAnalyzeAccel.SqlCurrentName == runEncoderAnalyzeAFPSqlFITTED ||
					runEncoderAnalyzeAccel.SqlCurrentName == runEncoderAnalyzeAFPSqlBOTH );
		else if(v == RunEncoderPlotVariables.RAWFORCE)
			return ( runEncoderAnalyzeForce.SqlCurrentName ==  runEncoderAnalyzeAFPSqlRAW ||
					runEncoderAnalyzeForce.SqlCurrentName == runEncoderAnalyzeAFPSqlBOTH );
		else if(v == RunEncoderPlotVariables.FITTEDFORCE)
			return ( runEncoderAnalyzeForce.SqlCurrentName == runEncoderAnalyzeAFPSqlFITTED ||
					runEncoderAnalyzeForce.SqlCurrentName == runEncoderAnalyzeAFPSqlBOTH );
		else if(v == RunEncoderPlotVariables.RAWPOWER)
			return ( runEncoderAnalyzePower.SqlCurrentName ==  runEncoderAnalyzeAFPSqlRAW ||
					runEncoderAnalyzePower.SqlCurrentName == runEncoderAnalyzeAFPSqlBOTH );
		else if(v == RunEncoderPlotVariables.FITTEDPOWER)
			return ( runEncoderAnalyzePower.SqlCurrentName == runEncoderAnalyzeAFPSqlFITTED ||
					runEncoderAnalyzePower.SqlCurrentName == runEncoderAnalyzeAFPSqlBOTH );

		return true;
	}

	~Preferences() {}
	   
}
