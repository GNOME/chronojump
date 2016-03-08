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
 *  Copyright (C) 2004-2016   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.Data;

public class Preferences 
{
	/*
	 * these are sent to preferences window
	 */

	public int digitsNumber;
	public bool showPower;
	public bool showStiffness;
	public bool showInitialSpeed;
	public bool showAngle;
	public bool showQIndex;
	public bool showDjIndex;
	public bool askDeletion;
	public bool weightStatsPercent;		//AKA weightPercentPreferred
	public bool heightPreferred;
	public bool metersSecondsPreferred;
	public bool runSpeedStartArrival;

	public Constants.DoubleContact runDoubleContactsMode; //default LAST
	public int runDoubleContactsMS; //milliseconds
	public Constants.DoubleContact runIDoubleContactsMode; //default AVERAGE
	public int runIDoubleContactsMS; //milliseconds
	
	public bool encoderPropulsive;
	public double encoderSmoothCon;
	public int videoDeviceNum; 		//AKA videoDevice
	public Constants.Encoder1RMMethod encoder1RMMethod;
	public string CSVExportDecimalSeparator;
	public string language;
	public bool RGraphsTranslate;
	public bool useHeightsOnJumpIndexes;
	public Constants.EncoderAutoSaveCurve encoderAutoSaveCurve;
	
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
	

	~Preferences() {}
	   
}
