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
 *  Copyright (C) 2004-2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;

public class Preferences 
{
	/*
	 * these are sent to preferences window
	 */

	//appearance tab
	public enum MaximizedTypes { NO, YES, YESUNDECORATED};
	public MaximizedTypes maximized;
	public bool personWinHide;
	public bool encoderCaptureShowOnlyBars;

	public bool showPower;
	public bool showStiffness;
	public bool showInitialSpeed;
	public bool showAngle;
	public bool showQIndex;
	public bool showDjIndex;
	public bool weightStatsPercent;		//AKA weightPercentPreferred
	public bool heightPreferred;
	public bool metersSecondsPreferred;
	public bool runSpeedStartArrival;

	public Constants.DoubleContact runDoubleContactsMode; //default LAST
	public int runDoubleContactsMS; //milliseconds
	public Constants.DoubleContact runIDoubleContactsMode; //default AVERAGE
	public int runIDoubleContactsMS; //milliseconds

	public int thresholdJumps;
	public int thresholdRuns;
	public int thresholdOther;
	
	//encoder capture
	public int encoderCaptureTime;
	public int encoderCaptureInactivityEndTime;
	public Constants.EncoderVariablesCapture encoderCaptureMainVariable;
	public int encoderCaptureMinHeightGravitatory;
	public int encoderCaptureMinHeightInertial;
	public bool encoderCaptureCheckFullyExtended;
	public int encoderCaptureCheckFullyExtendedValue;
	public Constants.EncoderAutoSaveCurve encoderAutoSaveCurve;
	public int encoderCaptureBarplotFontSize;
	public bool encoderShowStartAndDuration;
	public bool encoderCaptureCutByTriggers;
	
	//encoder other
	public bool encoderPropulsive;
	public double encoderSmoothCon;
	public Constants.Encoder1RMMethod encoder1RMMethod;
	
	public int videoDeviceNum; 		//AKA videoDevice
	public string CSVExportDecimalSeparator;
	public string language;
	public bool RGraphsTranslate;
	public bool useHeightsOnJumpIndexes;

	//advanced tab
	public bool askDeletion;
	public int digitsNumber;
	public bool muteLogs;

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
				
	public int encoderCaptureTimeIM = 120; //hardcoded 2 minutes.

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

	~Preferences() {}
	   
}
