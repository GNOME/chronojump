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

		generate_bAll_l ();
	}

	//used to select a fourPlatforms SqliteFourPlatforms.SelectData
	public FourPlatforms (string [] eventStr)
	{
		this.uniqueID = Convert.ToInt32 (eventStr[0]);
		this.personID = Convert.ToInt32 (eventStr[1]);
		this.sessionID = Convert.ToInt32 (eventStr[2]);
		this.exerciseID = Convert.ToInt32 (eventStr[3]);
		this.b0_1_l = UtilList.SQLStringToListDouble (Util.CDSNoZero (eventStr[4].ToString ()), "=");
		this.b0_0_l = UtilList.SQLStringToListDouble (Util.CDSNoZero (eventStr[5].ToString ()), "=");
		this.b1_1_l = UtilList.SQLStringToListDouble (Util.CDSNoZero (eventStr[6].ToString ()), "=");
		this.b1_0_l = UtilList.SQLStringToListDouble (Util.CDSNoZero (eventStr[7].ToString ()), "=");
		this.b2_1_l = UtilList.SQLStringToListDouble (Util.CDSNoZero (eventStr[8].ToString ()), "=");
		this.b2_0_l = UtilList.SQLStringToListDouble (Util.CDSNoZero (eventStr[9].ToString ()), "=");
		this.b3_1_l = UtilList.SQLStringToListDouble (Util.CDSNoZero (eventStr[10].ToString ()), "=");
		this.b3_0_l = UtilList.SQLStringToListDouble (Util.CDSNoZero (eventStr[11].ToString ()), "=");
		this.dateTime = eventStr[12].ToString ();
		this.description = eventStr[13].ToString ();
		this.videoURL = eventStr[14].ToString ();
		this.totalTime = Convert.ToDouble (eventStr[15].ToString());

		generate_bAll_l ();
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
		SqliteTests sqliteTests = new SqliteFourPlatforms ();
		this.uniqueID = sqliteTests.Insert (dbconOpened, toSQLInsertString());
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

	private void generate_bAll_l ()
	{
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

	// used on getStepsBottomStepsTopLowHigh ()
	private List<PointF> pointFSorted_l ()
	{
		List<PointF> p_l = new List<PointF> ();
		foreach (double d in b0_0_l)
			p_l.Add (new PointF (d, -1));
		foreach (double d in b0_1_l)
			p_l.Add (new PointF (d, 1));
		foreach (double d in b1_0_l)
			p_l.Add (new PointF (d, -2));
		foreach (double d in b1_1_l)
			p_l.Add (new PointF (d, 2));
		foreach (double d in b2_0_l)
			p_l.Add (new PointF (d, -3));
		foreach (double d in b2_1_l)
			p_l.Add (new PointF (d, 3));
		foreach (double d in b3_0_l)
			p_l.Add (new PointF (d, -4));
		foreach (double d in b3_1_l)
			p_l.Add (new PointF (d, 4));

		// 3 sort the list by time (ascending)
		return PointF.ReverseList (PointF.SortListXDescending (p_l));
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
			return Util.TrimDecimals (l[i], 3);
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

	/*
	public List<List<double>> BAll_l {
		get { return bAll_l; }
	}
	*/

	// gets a reconstructed points_ll like the created in FourPlatformsCaptureManage.Capture ()
	public List<List<PointF>> Points_ll
	{
		get {
			List<List<PointF>> points_ll = new List<List<PointF>>();
			points_ll.Add (new List<PointF>()); // 0: all events
			for (int i = 0; i < 4; i ++)
				points_ll.Add (new List<PointF>()); // each of the sensors

			//1st platform
			foreach (double d in b0_1_l)
			{
				(points_ll[0]).Add (new PointF (d, .1));
				(points_ll[1]).Add (new PointF (d, 4+.2));
			}
			foreach (double d in b0_0_l)
			{
				(points_ll[0]).Add (new PointF (d, .1));
				(points_ll[1]).Add (new PointF (d, 4-.2));
			}

			//2nd platform
			foreach (double d in b1_1_l)
			{
				(points_ll[0]).Add (new PointF (d, .1));
				(points_ll[2]).Add (new PointF (d, 3+.2));
			}
			foreach (double d in b1_0_l)
			{
				(points_ll[0]).Add (new PointF (d, .1));
				(points_ll[2]).Add (new PointF (d, 3-.2));
			}

			//3rd platform
			foreach (double d in b2_1_l)
			{
				(points_ll[0]).Add (new PointF (d, .1));
				(points_ll[3]).Add (new PointF (d, 2+.2));
			}
			foreach (double d in b2_0_l)
			{
				(points_ll[0]).Add (new PointF (d, .1));
				(points_ll[3]).Add (new PointF (d, 2-.2));
			}


			//4th platform
			foreach (double d in b3_1_l)
			{
				(points_ll[0]).Add (new PointF (d, .1));
				(points_ll[4]).Add (new PointF (d, 1+.2));
			}
			foreach (double d in b3_0_l)
			{
				(points_ll[0]).Add (new PointF (d, .1));
				(points_ll[4]).Add (new PointF (d, 1-.2));
			}

			for (int i = 0; i < 5; i ++)
				points_ll[i] = PointF.ReverseList (PointF.SortListXDescending (points_ll[i]));

			return points_ll;
		}
	}

	//TODO move this to FourPlatformsCaptureManageSteps or inherited class, call statically if no other option
	private enum StepsStatusEnum { NOTSTARTED, DONEBOTTOM, DONETOP };

	// using b0_0_l, b1_1_l, b2_1_l, b3_1_l gets a reconstructed stepsBottom_l, stepsTop_l
	// like the created while capture in FourPlatformsCaptureManageSteps.updateSteps ()
	public void GetStepsBottomStepsTop (ref List<PointF> stepsBottom_l, ref List<PointF> stepsTop_l)
	{
		// 1 exit if no data and assign dTop_l
		if (exerciseID == 0)
			return;

		if (exerciseID >= 1 && exerciseID <= 3)
			getStepsBottomStepsTopTwoPlatforms (ref stepsBottom_l, ref stepsTop_l);
		else if (exerciseID == 4)
			getStepsBottomStepsTopLowHigh (ref stepsBottom_l, ref stepsTop_l);
	}

	// note this method is a rewrite of FourPlatformsCaptureManageSteps.UpdateSteps
	// better do like below method: getStepsBottomStepsTopLowHigh
	// that prepares data and uses FourPlatformsCaptureManageStepsLowHigh like if we were capturing
	private void getStepsBottomStepsTopTwoPlatforms (ref List<PointF> stepsBottom_l, ref List<PointF> stepsTop_l)
	{
		int y = exerciseID +1; //where it goes the dTop line
		List<double> dTop_l = new List<double> ();
		if (exerciseID == 1)
			dTop_l = b1_1_l;
		else if (exerciseID == 2)
			dTop_l = b2_1_l;
		else if (exerciseID == 3)
			dTop_l = b3_1_l;

		if ( b0_0_l.Count == 0 || dTop_l.Count == 0)
			return;

		// 2 create a PointF list with time < 0 for the bottom and time > 0 for the top
		List<PointF> p_l = new List<PointF>();
		foreach (double d in b0_0_l)
			p_l.Add (new PointF (d, -1));
		foreach (double d in dTop_l)
			p_l.Add (new PointF (d, +1));

		// 3 sort the list by time (ascending)
		p_l = PointF.ReverseList (PointF.SortListXDescending (p_l));

		// 4 iterate the list filing the values
		StepsStatusEnum stepsStatusEnum = StepsStatusEnum.NOTSTARTED;

		foreach (PointF p in p_l)
		{
			//mark the bottom
			if (stepsStatusEnum != StepsStatusEnum.DONEBOTTOM && p.Y < 0)
			{
				stepsBottom_l.Add (new PointF (p.X, 1));
				stepsStatusEnum = StepsStatusEnum.DONEBOTTOM;
			}
			//update the bottom as maybe has been repeated later
			else if (stepsStatusEnum == StepsStatusEnum.DONEBOTTOM && p.Y < 0)
			{
				stepsBottom_l[stepsBottom_l.Count -1] = new PointF (p.X, 1);
				stepsStatusEnum = StepsStatusEnum.DONEBOTTOM;
			}

			//do the top
			if (stepsStatusEnum == StepsStatusEnum.DONEBOTTOM && p.Y > 0)
			{
				 stepsTop_l.Add (new PointF (p.X, y));
				 stepsStatusEnum = StepsStatusEnum.DONETOP;
			}
		}
	}

	// this method provides the data to FourPlatformsCaptureManageStepsLowHigh like if we were capturing
	private void getStepsBottomStepsTopLowHigh (ref List<PointF> stepsBottom_l, ref List<PointF> stepsTop_l)
	{
		// 1 create the FourPlatformsCaptureManageStepsLowHigh object
		FourPlatformsCaptureManageStepsLowHigh fpcms = new FourPlatformsCaptureManageStepsLowHigh (
				FourPlatformsCaptureManage.CaptureEnum.FROMLOWTOHIGH, -1,
				ref stepsBottom_l, ref stepsTop_l);

		List<double> timeAccu_l = new List<double> (); //double to use PointF (in seconds)
		for (int i = 0; i <= 3 ; i ++)
			timeAccu_l.Add (0);

		foreach (PointF p in pointFSorted_l ())
		{
			FourPlatformsEvent fpe = new FourPlatformsEvent ("");
			if (p.Y < 0)
				fpe = new FourPlatformsEvent (string.Format ("{0}:{1}",
							(-1 * p.Y) -1, -1 * Convert.ToInt32 (p.X * 1000)));
			else
				fpe = new FourPlatformsEvent (string.Format ("{0}:{1}",
							p.Y -1, Convert.ToInt32 (p.X * 1000)));

			//LogB.Information ("fpe: " + fpe.ToString ());
			timeAccu_l[fpe.Button] = p.X;
			fpcms.UpdateSteps (fpe, timeAccu_l, fpe.Button +1);
		}
	}

	public FourPlatformsCaptureManage.CaptureEnum GetCaptureEnum ()
	{
		if (exerciseID == 0)
			return FourPlatformsCaptureManage.CaptureEnum.DEFAULT;
		else if (exerciseID == 1)
			return FourPlatformsCaptureManage.CaptureEnum.FROM1TO2;
		else if (exerciseID == 2)
			return FourPlatformsCaptureManage.CaptureEnum.FROM1TO3;
		else if (exerciseID == 3)
			return FourPlatformsCaptureManage.CaptureEnum.FROM1TO4;
		else if (exerciseID == 4)
			return FourPlatformsCaptureManage.CaptureEnum.FROMLOWTOHIGH;
		else //default
			return FourPlatformsCaptureManage.CaptureEnum.DEFAULT;
	}

	//used on treeview
	public string GetCaptureEnumStr ()
	{
		return FourPlatformsCaptureManage.CaptureEnumStr (GetCaptureEnum ());
	}

	public int ExerciseID { get { return exerciseID; } }
	public double TotalTime { get { return totalTime; } }
}
