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
 * Copyright (C) 2004-2025   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;

public class FourPlatformsCaptureManageSteps
{
	public enum StepsStatusEnum { NOTSTARTED, DONEBOTTOM, DONETOP };
	protected StepsStatusEnum stepsStatus;

	protected FourPlatformsCaptureManage.CaptureEnum captureType;
	protected int stepsTotal;
	protected List<PointF> stepsBottom_l;
	protected List<PointF> stepsTop_l;

	protected double timeStart;
	protected double timeEnd;
	protected int stepsCompleted;


	// constructor (needed for inherit)
	public FourPlatformsCaptureManageSteps ()
	{
	}

	// constructor
	public FourPlatformsCaptureManageSteps (
			FourPlatformsCaptureManage.CaptureEnum captureType, int stepsTotal,
			ref List<PointF> stepsBottom_l, ref List<PointF> stepsTop_l)
	{
		init (captureType, stepsTotal, stepsBottom_l, stepsTop_l);
	}

	protected void init (FourPlatformsCaptureManage.CaptureEnum captureType, int stepsTotal,
			List<PointF> stepsBottom_l, List<PointF> stepsTop_l)
	{
		this.captureType = captureType;
		this.stepsTotal = stepsTotal;
		this.stepsBottom_l = stepsBottom_l;
		this.stepsTop_l = stepsTop_l;

		timeStart = 0;
		timeEnd = 0;
		stepsStatus = StepsStatusEnum.NOTSTARTED;
		stepsCompleted = 0;
	}

	public virtual void UpdateSteps (FourPlatformsEvent fpe, List<double> timeAccu_l, int y)
	{
		//mark the bottom
		if (stepsStatus != StepsStatusEnum.DONEBOTTOM && y == 1 && fpe.Time < 0)
		{
			stepsBottom_l.Add (new PointF (timeAccu_l[fpe.Button], 1));
			if(stepsCompleted == 0)
				timeStart = timeAccu_l[fpe.Button];

			stepsStatus = StepsStatusEnum.DONEBOTTOM;
		}
		//update the bottom as maybe has been repeated later
		else if (stepsStatus == StepsStatusEnum.DONEBOTTOM && y == 1 && fpe.Time < 0)
		{
			stepsBottom_l[stepsBottom_l.Count -1] = new PointF (timeAccu_l[fpe.Button], 1);
			if(stepsCompleted == 0)
				timeStart = timeAccu_l[fpe.Button];

			stepsStatus = StepsStatusEnum.DONEBOTTOM;
		}

		//do the top
		if (stepsStatus == StepsStatusEnum.DONEBOTTOM && fpe.Time > 0 && (
					(captureType == FourPlatformsCaptureManage.CaptureEnum.FROM1TO2 && y == 2) ||
					(captureType == FourPlatformsCaptureManage.CaptureEnum.FROM1TO3 && y == 3) ||
					(captureType == FourPlatformsCaptureManage.CaptureEnum.FROM1TO4 && y == 4)
					) )
		{
			stepsTop_l.Add (new PointF (timeAccu_l[fpe.Button], y));
			stepsStatus = StepsStatusEnum.DONETOP;
			stepsCompleted ++;

			if (stepsCompleted >= stepsTotal)
				timeEnd = timeAccu_l[fpe.Button];
		}
	}

	public double TimeStart { get { return timeStart; } }
	public double TimeEnd { get { return timeEnd; } }
	public int StepsCompleted { get { return stepsCompleted; } }
}

public class FourPlatformsCaptureManageStepsLowHigh : FourPlatformsCaptureManageSteps
{
	List<PointF> on_l; // when landed: all contacts
	List<PointF> off_l; // when lift off: all lifts

	List<PointF> on3_l; // when landed: contact 3
	List<PointF> on4_l; // when landed: contact 4
	List<PointF> off1_l; // when leave: contact 1
	List<PointF> off2_l; // when leave: contact 2

	private bool lastContactIs3;
	//private bool firstOffIs1;

	// constructor
	public FourPlatformsCaptureManageStepsLowHigh (
			FourPlatformsCaptureManage.CaptureEnum captureType, int stepsTotal,
			ref List<PointF> stepsBottom_l, ref List<PointF> stepsTop_l)
	{
		init (captureType, stepsTotal, stepsBottom_l, stepsTop_l);
		resetLists ();
	}

	public override void UpdateSteps (FourPlatformsEvent fpe, List<double> timeAccu_l, int y) //y is 1-4
	{
		/*
		LogB.Information ("timeAccu_l:");
		LogB.Information (UtilList.ListDoubleToString (timeAccu_l, 2, " "));
		LogB.Information ("fpe:");
		LogB.Information (fpe.ToString ());
		*/

		// 1 update lists
		bool processSteps = false; // if not process, just return
		if (fpe.Time > 0) // is contact (in positive we have time flying)
		{
			on_l.Add (new PointF (timeAccu_l[fpe.Button], y));

			if (fpe.Button == 2)
			{
				//LogB.Information ("3 on");
				on3_l.Add (new PointF (timeAccu_l[2], y));
				processSteps = true;
			}
			else if (fpe.Button == 3)
			{
				//LogB.Information ("4 on");
				on4_l.Add (new PointF (timeAccu_l[3], y));
				processSteps = true;
			}
		}
		else // is leaving
		{
			off_l.Add (new PointF (timeAccu_l[fpe.Button], y));

			if (fpe.Button == 0)
			{
				//LogB.Information ("1 off");
				off1_l.Add (new PointF (timeAccu_l[0], y));
			}
			else if (fpe.Button == 1)
			{
				//LogB.Information ("2 off");
				off2_l.Add (new PointF (timeAccu_l[1], y));
			}
		}

		if (! processSteps)
			return;

		LogB.Information ("processSteps ok");
		// 2 exit if any of the lists is empty
		if (on3_l.Count == 0 || on4_l.Count == 0 || off1_l.Count == 0 || off2_l.Count == 0)
			return;

		LogB.Information ("4 lists not empty");
		// if channel 1 off is more at right than 3 on and 4 on (strange, but ...) return
		if (PointF.Last (off1_l).X > PointF.Last (on3_l).X && PointF.Last (off1_l).X > PointF.Last (on4_l).X)
			return;
		// if channel 2 off is more at the right than 3 on and 4 on (strange, but ...) return
		if (PointF.Last (off2_l).X > PointF.Last (on3_l).X && PointF.Last (off2_l).X > PointF.Last (on4_l).X)
			return;

		LogB.Information ("no Off more at right than Ons");
		// 3 create onLast4Relevant_l list omitting same sensor consecutive repeated values
		List<PointF> last4Relevant_l = createLast4RelevantList ();

		if (existsAn12ContactAfter3or4Flight (last4Relevant_l))
			return;
		
		LogB.Information ("no exists an 1,2 contact after 3 or 4 flight");

		LogB.Information ("success! Saving step");
		stepsBottom_l.Add (last4Relevant_l[3]);
		stepsTop_l.Add (last4Relevant_l[0]);
	
		if (stepsCompleted == 0)
			timeStart = last4Relevant_l[3].X;

		stepsCompleted ++;

		//if (stepsCompleted >= stepsTotal) // commented to allow have timeTotal even on Finish (without finishing steps
			timeEnd = last4Relevant_l[0].X;

		resetLists (); // reset lists to not count again on next contact
	}

	private void resetLists ()
	{
		on_l = new List<PointF> ();
		off_l = new List<PointF> ();

		on3_l = new List<PointF> ();
		on4_l = new List<PointF> ();
		off1_l = new List<PointF> ();
		off2_l = new List<PointF> ();
	}

	/*
	 * add last two ON on 3, 4
	 * add last two OFF on 1, 2
	 */
	private List<PointF> createLast4RelevantList ()
	{
		List<PointF> l = new List<PointF> (); //from newest (right) to oldest (left)

		if (PointF.Last (on3_l).X > PointF.Last (on4_l).X)
		{
			lastContactIs3 = true;
			l.Add (PointF.Last (on3_l));
			l.Add (PointF.Last (on4_l));
		} else {
			lastContactIs3 = false;
			l.Add (PointF.Last (on4_l));
			l.Add (PointF.Last (on3_l));
		}

		if (PointF.Last (off1_l).X < PointF.Last (off2_l).X)
		{
			//firstOffIs1 = true;
			l.Add (PointF.Last (off2_l));
			l.Add (PointF.Last (off1_l));
		} else {
			//firstOffIs1 = false;
			l.Add (PointF.Last (off1_l));
			l.Add (PointF.Last (off2_l));
		}

		return l;
	}

	private bool existsAn12ContactAfter3or4Flight (List<PointF> last4Relevant_l)
	{
		LogB.Information ("at existsAn12...");
		int previousTopSensor = 3; // 1-4
		if (lastContactIs3)
			previousTopSensor = 4; // 1-4

		// check if previousTopSensor has lift and after there is a contact on 1 or 2
		//LogB.Information ("searching off at right of: " + last4Relevant_l[1].X.ToString ());
		double previousTopOffX = -1;
		foreach (PointF p in off_l)
		{
			//LogB.Information ("p: " + p.ToString ());
			if (p.Y == previousTopSensor &&
					p.X > last4Relevant_l[1].X && //this off has to be at right of penultimate (note last is 0)
					p.X > previousTopOffX)
				previousTopOffX = p.X;
		}

		if (previousTopOffX < 0)
			return false;

		//LogB.Information ("searching on at right of previousTopOffX: " + previousTopOffX.ToString ());
		foreach (PointF p in on_l)
		{
			//LogB.Information ("p: " + p.ToString ());
			if (p.Y <= 2 && p.X >= previousTopOffX)
				return true;
		}

		return false;
	}
}
