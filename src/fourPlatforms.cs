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
using System.Collections.Generic; //List<T>
using System.Diagnostics;  //Stopwatch

//fourPlatforms test (like jump, run, ...)
public class FourPlatforms : Event
{
	private int exerciseID; //until fourPlatformsExercise table is not created, all will be 0
	private string dateTime;
	private string videoURL;
	private List<double> b0_1_l;
	private List<double> b0_0_l;
	private List<double> b1_1_l;
	private List<double> b1_0_l;
	private List<double> b2_1_l;
	private List<double> b2_0_l;
	private List<double> b3_1_l;
	private List<double> b3_0_l;
	private double totalTime;
	private string description;

	/*
	//constructor used after deleting a test
	public Wilight ()
	{
		this.uniqueID = -1;
	}
	*/

	//regular constructor
	public FourPlatforms (int uniqueID, int personID, int sessionID, int exerciseID,
			string dateTime, string videoURL,
			List<double> b0_1_l, List<double> b0_0_l,
			List<double> b1_1_l, List<double> b1_0_l,
			List<double> b2_1_l, List<double> b2_0_l,
			List<double> b3_1_l, List<double> b3_0_l,
			double totalTime,
			string description)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.sessionID = sessionID;
		this.exerciseID = exerciseID;
		this.dateTime = dateTime;
		this.videoURL = videoURL;
		this.b0_1_l = b0_1_l;
		this.b0_0_l = b0_0_l;
		this.b1_1_l = b1_1_l;
		this.b1_0_l = b1_0_l;
		this.b2_1_l = b2_1_l;
		this.b2_0_l = b2_0_l;
		this.b3_1_l = b3_1_l;
		this.b3_0_l = b3_0_l;
		this.totalTime = totalTime;
		this.description = description;
	}

	/* 
	 * TODO:
	//used to select a fourPlatforms SqliteFourPlatforms.SelectData
	public FourPlatforms (string [] eventString)
	{
		this.uniqueID = Convert.ToInt32(eventString[0]);
		this.personID = Convert.ToInt32(eventString[1]);
		this.sessionID = Convert.ToInt32(eventString[2]);
		this.exerciseID = Convert.ToInt32(eventString[3]);
		this.dateTime = eventString[4];
		this.videoURL = eventString[5];
		this.totalMs = Convert.ToInt32(eventString[6]);
		this.onString = eventString[7];
		this.description = "";
	}

	public static List<Event> WilightListToEventList (List<Wilight> ws)
	{
		List<Event> events = new List<Event>();
		foreach(Wilight w in ws)
			events.Add((Event) w);

		return events;
	}
	*/

	public int InsertSQL (bool dbconOpened)
	{
		return SqliteFourPlatforms.Insert (dbconOpened, toSQLInsertString());
	}
	private string toSQLInsertString()
	{
		string uniqueIDStr = "NULL";
		if(uniqueID != -1)
			uniqueIDStr = uniqueID.ToString();

		return
			"(" + uniqueIDStr + ", " + personID + ", " + sessionID + ", " + exerciseID +
			", '" + dateTime + "', '" + 
			Util.ConvertToPoint (UtilList.ListDoubleToString (b0_1_l, 3, "="))  + "', '" +
			Util.ConvertToPoint (UtilList.ListDoubleToString (b0_0_l, 3, "=")) + "', '" +
			Util.ConvertToPoint (UtilList.ListDoubleToString (b1_1_l, 3, "="))  + "', '" +
			Util.ConvertToPoint (UtilList.ListDoubleToString (b1_0_l, 3, "=")) + "', '" +
			Util.ConvertToPoint (UtilList.ListDoubleToString (b2_1_l, 3, "="))  + "', '" +
			Util.ConvertToPoint (UtilList.ListDoubleToString (b2_0_l, 3, "=")) + "', '" +
			Util.ConvertToPoint (UtilList.ListDoubleToString (b3_1_l, 3, "="))  + "', '" +
			Util.ConvertToPoint (UtilList.ListDoubleToString (b3_0_l, 3, "=")) + "', '" +
			description + "', '" + videoURL + "', " +
			Util.ConvertToPoint (totalTime) + ")";
	}

	/*
	public int TotalMs {
		get { return totalMs; }
	}
	public string DateTime {
		get { return dateTime; }
	}
	*/
}
