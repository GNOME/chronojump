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

	private List<List<double>> bAll_l;

	/*
	//constructor used after deleting a test
	public Wilight ()
	{
		this.uniqueID = -1;
	}
	*/

	//regular constructor
	public FourPlatforms (int uniqueID, int personID, int sessionID, int exerciseID,
			List<double> b0_1_l, List<double> b0_0_l,
			List<double> b1_1_l, List<double> b1_0_l,
			List<double> b2_1_l, List<double> b2_0_l,
			List<double> b3_1_l, List<double> b3_0_l,
			string dateTime,
			string description,
			string videoURL,
			double totalTime)
	{
		this.uniqueID = uniqueID;
		this.personID = personID;
		this.sessionID = sessionID;
		this.exerciseID = exerciseID;
		this.b0_1_l = b0_1_l;
		this.b0_0_l = b0_0_l;
		this.b1_1_l = b1_1_l;
		this.b1_0_l = b1_0_l;
		this.b2_1_l = b2_1_l;
		this.b2_0_l = b2_0_l;
		this.b3_1_l = b3_1_l;
		this.b3_0_l = b3_0_l;
		this.dateTime = dateTime;
		this.description = description;
		this.videoURL = videoURL;
		this.totalTime = totalTime;

		bAll_l = new List<List<double>> ();
		bAll_l.Add (b0_1_l);
		bAll_l.Add (b0_0_l);
		bAll_l.Add (b1_1_l);
		bAll_l.Add (b1_0_l);
		bAll_l.Add (b2_1_l);
		bAll_l.Add (b2_0_l);
		bAll_l.Add (b3_1_l);
		bAll_l.Add (b3_0_l);
	}

	//used to select a fourPlatforms SqliteFourPlatforms.SelectData
	public FourPlatforms (string [] eventStr)
	{
		this.uniqueID = Convert.ToInt32 (eventStr[0]);
		this.personID = Convert.ToInt32 (eventStr[1]);
		this.sessionID = Convert.ToInt32 (eventStr[2]);
		this.exerciseID = Convert.ToInt32 (eventStr[3]);
		this.b0_1_l = UtilList.SQLStringToListDouble (eventStr[4].ToString (), "=");
		this.b0_0_l = UtilList.SQLStringToListDouble (eventStr[5].ToString (), "=");
		this.b1_1_l = UtilList.SQLStringToListDouble (eventStr[6].ToString (), "=");
		this.b1_0_l = UtilList.SQLStringToListDouble (eventStr[7].ToString (), "=");
		this.b2_1_l = UtilList.SQLStringToListDouble (eventStr[8].ToString (), "=");
		this.b2_0_l = UtilList.SQLStringToListDouble (eventStr[9].ToString (), "=");
		this.b3_1_l = UtilList.SQLStringToListDouble (eventStr[10].ToString (), "=");
		this.b3_0_l = UtilList.SQLStringToListDouble (eventStr[11].ToString (), "=");
		this.dateTime = eventStr[12].ToString ();
		this.description = eventStr[13].ToString ();
		this.videoURL = eventStr[14].ToString ();
		this.totalTime = Convert.ToDouble (eventStr[15].ToString());
	}

	public static List<Event> FourPlatformsListToEventList (List<FourPlatforms> fp_l)
	{
		List<Event> events = new List<Event>();
		foreach (FourPlatforms fp in fp_l)
			events.Add ((Event) fp);

		return events;
	}

	public void InsertSQL (bool dbconOpened)
	{
		this.uniqueID = SqliteFourPlatforms.Insert (dbconOpened, toSQLInsertString());
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

	private List<double> getList (int channel, bool isOn)
	{
		if (channel == 0 && isOn)
			return b0_1_l;
		else if (channel == 0 && ! isOn)
			return b0_0_l;
		else if (channel == 1 && isOn)
			return b1_1_l;
		else if (channel == 1 && ! isOn)
			return b1_0_l;
		else if (channel == 2 && isOn)
			return b2_1_l;
		else if (channel == 2 && ! isOn)
			return b2_0_l;
		else if (channel == 3 && isOn)
			return b3_1_l;
		else //if (channel == 3 && ! isOn)
			return b3_0_l;
	}

	/*
	public double GetTimeAtChannel (int channel, bool isOn, int i)
	{
		List<double> l = getList (channel, isOn);
		if (i < l.Count)
			return l[i];
		else
			return -1;
	}
	*/
	public string GetTimeAtChannelAsStr (int channel, bool isOn, int i)
	{
		List<double> l = getList (channel, isOn);
		if (i < l.Count)
			return l[i].ToString ();
		else
			return "";
	}

	public int GetMaxEventsOnAnyChannel
	{
		get {
			int max = 0;
			foreach (List<double> b_l in bAll_l)
				if (b_l.Count > max)
					max = b_l.Count;

			return max;
		}
	}

	public double TotalTime
	{
		get { return totalTime; }
	}
}
