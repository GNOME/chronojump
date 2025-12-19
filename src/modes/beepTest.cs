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
 *  Copyright (C) 2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
//using System.Collections.Generic; //List<T>
//using System.Diagnostics;  //Stopwatch

/*
 * BeepTest test (like jump, run, ...)
 * this class has the object relative to the result of the person.
 * For test management check beepTestManage.cs
 */

public class BeepTest : Event
{
	private int exerciseID;
        private string options;
        private int stages;
        private int laps;
        private int totalMeters;
        private double maxSpeed;
	private string videoURL;

	//regular constructor
	public BeepTest (int uniqueID, int personID, int sessionID, int exerciseID,
                        string options, int stages, int laps, int totalMeters, double maxSpeed,
			string dateTime, string description, string videoURL)
        {
                this.uniqueID = uniqueID;
                this.personID = personID;
		this.sessionID = sessionID;
		this.exerciseID = exerciseID;
                this.options = options;
                this.stages = stages;
                this.laps = laps;
                this.totalMeters = totalMeters;
                this.maxSpeed = maxSpeed;
		this.dateTime = dateTime;
		this.description = description;
		this.videoURL = videoURL;
	}

	//used to select a beepTest SqliteBeepTest.SelectData
	public BeepTest (string [] eventStr)
	{
		this.uniqueID = Convert.ToInt32 (eventStr[0]);
		this.personID = Convert.ToInt32 (eventStr[1]);
		this.sessionID = Convert.ToInt32 (eventStr[2]);
		this.exerciseID = Convert.ToInt32 (eventStr[3]);
		this.options = eventStr[4];
		this.stages = Convert.ToInt32 (eventStr[5]);
		this.laps = Convert.ToInt32 (eventStr[6]);
		this.totalMeters = Convert.ToInt32 (eventStr[7]);
		this.maxSpeed = Convert.ToDouble (Util.CDS (eventStr[8]));
		this.dateTime = eventStr[9];
		this.description = eventStr[10];
		this.videoURL = eventStr[11];
	}

	public int InsertSQL (bool dbconOpened)
	{
		SqliteBeepTest sqliteBeepTestObject = new SqliteBeepTest ();
		return sqliteBeepTestObject.Insert (dbconOpened, toSQLInsertString());
	}
	private string toSQLInsertString()
	{
		string uniqueIDStr = "NULL";
		if(uniqueID != -1)
			uniqueIDStr = uniqueID.ToString();

		return
			"(" + uniqueIDStr + ", " + personID + ", " + sessionID + ", " + exerciseID +
                        ", '" + options + "', " + stages + ", " + laps + ", " + totalMeters + ", " + Util.CTP (maxSpeed) +
			", '" + dateTime + "', '" + description + "', '" + videoURL + "')";
	}

	public string ExerciseName {
		get {
			if (exerciseID < (BeepTestCM.TypesArray ()).Length)
				return (BeepTestCM.TypesArray ())[exerciseID];

			return "";
		}
	}

	public string GetAchievedStageName {
		get {
			// just for show the name of the stage on the treeview
			// so other construction params of the factory are not relevant
			BeepTestCM btcm = BeepTestCM.Factory (
					ExerciseName,
					0, true,
					0, 0, 0);

			LogB.Information ("btcm is " + btcm.ToString ());
			return btcm.GetStageNameOfStage (stages);
		}
	}
	
	public double GetVo2Max {
		get {
			// most of the construction params of the factory are not relevant
			BeepTestCM btcm = BeepTestCM.Factory (
					ExerciseName,
					0, (options == "Speed1stStage=8"),
					0, 0, 0);

			return btcm.Vo2max (stages, laps);
		}
	}

	public int Stages {
		get { return stages; }
	}
	public int Laps {
		get { return laps; }
	}
        public double MaxSpeed {
		get { return maxSpeed; }
	}
}
