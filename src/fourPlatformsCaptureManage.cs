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

public class FourPlatformsCaptureManage
{
	// FROMLOWTOHIGH is from (1,2 or 2,1) to (3,4 or 4,3)
	public enum CaptureEnum { DEFAULT, FROM1TO2, FROM1TO3, FROM1TO4, FROMLOWTOHIGH };

	private Constants.Modes mode;
	private CaptureEnum captureType;
	private FourPlatformsCapture fpc;
	private bool finish;
	private bool cancel;
	//private bool error;
	private List<IDName> idName_l;

	//private List<PointF> points_l;
	private List<List<PointF>> points_ll; //[0] will have all and helps to configureTimeWindow (graphical info)
	private List<List<double>> timesOn_ll; //[0] will have all and helps to configureTimeWindow (time info to sql)
	private List<List<double>> timesOff_ll; //[0] will have all and helps to configureTimeWindow (time info to sql)
	private List<PointF> stepsBottom_l;
	private List<PointF> stepsTop_l;

	private FourPlatformsCaptureManageSteps fpcManageSteps;
	//private int stepsTotal = 15;
	private int stepsTotal = 3; // to debug now

	//both these will be used to record final time
	private double timeStart; //on steps is start of the 1st valid step. On default (not steps) is first on or off.
	private double timeEnd; //on steps end of the last valid step. On default (not steps) is last on or off.

	public FourPlatformsCaptureManage (
			Constants.Modes mode,
			CaptureEnum captureType,
			FourPlatformsCapture fpc,
			ref List<List<PointF>> points_ll,
			ref List<PointF> stepsBottom_l,
			ref List<PointF> stepsTop_l,
			List<IDName> idName_l
			)
	{
		this.mode = mode;
		this.captureType = captureType;
		this.fpc = fpc;
		this.points_ll = points_ll;
		this.stepsBottom_l = stepsBottom_l;
		this.stepsTop_l = stepsTop_l;
		this.idName_l = idName_l;

		timesOn_ll = new List<List<double>>();
		timesOff_ll = new List<List<double>>();

		for (int i = 0; i < 4; i ++)
		{
			timesOn_ll.Add (new List<double> ());
			timesOff_ll.Add (new List<double> ());
		}
	}

	public bool Init ()
	{
		if (captureType == CaptureEnum.FROM1TO2 ||
				captureType == CaptureEnum.FROM1TO3 ||
				captureType == CaptureEnum.FROM1TO4)
			fpcManageSteps = new FourPlatformsCaptureManageSteps (
					captureType, stepsTotal, ref stepsBottom_l, ref stepsTop_l);
		else if (captureType == CaptureEnum.FROMLOWTOHIGH)
			fpcManageSteps = new FourPlatformsCaptureManageStepsLowHigh (
					captureType, stepsTotal, ref stepsBottom_l, ref stepsTop_l);

		finish = false;
		cancel = false;
		//error = false;

		fpc.Reset ();
		if (! fpc.CaptureStart ())
			return false;

		return true;
	}

	public void Capture ()
	{
		finish = false;

		List<double> timeAccu_l = new List<double> (); //double to use PointF (in seconds)
		for (int i = 0; i <= 3 ; i ++)
			timeAccu_l.Add (0);

		int yPre = -1; //for FROMLOWTOHIGH

		while (! finish && ! cancel)// && ! error)
		{
			if(! fpc.CaptureSample ())
				cancel = true; //problem reading line (capturing)

			if (fpc.CanReadFromList ())
			{
				FourPlatformsEvent fpe = fpc.FourPlatformsCaptureReadNext();
				LogB.Information("fpe: " + fpe.ToString());

				if (fpe.Button < 0)
				{
					LogB.Information("problem reading");
					continue;
				}

				//fpe.Time *= -1; //first buttons prototype board has the buttons behaviour opposite than the final board

				int timeNow = fpe.Time; //millis

				//int button = fpe.Button + 1; //from 0-3 to 1-4
				//have button as positive or negative and put timeNow as positive
				if (timeNow < 0)
					timeNow = Math.Abs (timeNow);

				timeAccu_l[fpe.Button] += UtilAll.DivideSafe (timeNow, 1000);

				int y = fpe.Button + 1; //1 - 4
				double ySign;

				if (mode == Constants.Modes.JUMPSSIMPLE)
				{
					ySign = 0;
					if (fpe.Time < 0)
						ySign = .4;
				} else { //(mode == Constants.Modes.OTHER)
					ySign = .2;
					if (fpe.Time < 0)
						ySign = -.2;

					//steps stuff
					if (captureType == CaptureEnum.FROM1TO2 ||
							captureType == CaptureEnum.FROM1TO3 ||
							captureType == CaptureEnum.FROM1TO4 ||
							captureType == CaptureEnum.FROMLOWTOHIGH)
						fpcManageSteps.UpdateSteps (fpe, timeAccu_l, y);
					else {
						//1st contact will update timeStart
						if (timeStart == 0)
							timeStart = timeAccu_l[fpe.Button];

						//all contacts will update timeEnd
						timeEnd = timeAccu_l[fpe.Button];
					}
				}

				if (fpe.Time < 0)
					timesOff_ll[fpe.Button].Add (timeAccu_l[fpe.Button]); //0-3 each of the sensors
				else
					timesOn_ll[fpe.Button].Add (timeAccu_l[fpe.Button]); //0-3 each of the sensors

				//LogB.Information ("fpe.Button: " + fpe.Button);
				//LogB.Information ("y: " + y);
				//in seconds
				points_ll[0].Add (new PointF (timeAccu_l[fpe.Button], .1)); //0 has all //to debug
				points_ll[y].Add (new PointF (timeAccu_l[fpe.Button], 5-y+ySign)); //1-4 each of the sensors

				yPre = y; // for FROMLOWTOHIGH

				if (fpcManageSteps != null && fpcManageSteps.StepsCompleted >= stepsTotal)
					finish = true;
			}
		}
		LogB.Information ("calling Stop");
		fpc.Stop ();
	}

	public static string CaptureEnumStr (CaptureEnum cEnum)
	{
		if (cEnum == CaptureEnum.DEFAULT)
			return "Default";
		else if (cEnum == CaptureEnum.FROM1TO2)
			return "1->2";
		else if (cEnum == CaptureEnum.FROM1TO3)
			return "1->3";
		else if (cEnum == CaptureEnum.FROM1TO4)
			return "1->4";
		else if (cEnum == CaptureEnum.FROMLOWTOHIGH)
			return "Low->High";

		return "Default";
	}

	public List<List<double>> TimesOn_ll {
		get { return timesOn_ll; }
	}
	public List<List<double>> TimesOff_ll {
		get { return timesOff_ll; }
	}
	public List<IDName> IDName_l {
		get { return idName_l; }
	}
	public int StepsCompleted {
		get {
			if (fpcManageSteps == null)
				return 0; // precaution for the future

			return fpcManageSteps.StepsCompleted; }
	}
	public int StepsTotal {
		get { return stepsTotal; }
	}
	public double TimeStart {
		get {
			if (captureType == CaptureEnum.FROM1TO2 ||
					captureType == CaptureEnum.FROM1TO3 ||
					captureType == CaptureEnum.FROM1TO4 ||
					captureType == CaptureEnum.FROMLOWTOHIGH)
				return fpcManageSteps.TimeStart;
			else
				return timeStart;
		}
	}
	public double TimeEnd {
		//get { return timeEnd; }
		get {
			if (captureType == CaptureEnum.FROM1TO2 ||
					captureType == CaptureEnum.FROM1TO3 ||
					captureType == CaptureEnum.FROM1TO4 ||
					captureType == CaptureEnum.FROMLOWTOHIGH)
				return fpcManageSteps.TimeEnd;
			else
				return timeEnd;
		}
	}
	public bool Finish {
		get { return finish; }
		set { finish = value; }
	}
	public bool Cancel {
		set { cancel = value; }
	}
}

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

	//List<PointF> onLast4Relevant_l; // when landed (if 1, 2a, 2b, 3a, 3b, 4) a, b are two contacts on same platform, it will take just first one

	// constructor
	public FourPlatformsCaptureManageStepsLowHigh (
			FourPlatformsCaptureManage.CaptureEnum captureType, int stepsTotal,
			ref List<PointF> stepsBottom_l, ref List<PointF> stepsTop_l)
	{
		init (captureType, stepsTotal, stepsBottom_l, stepsTop_l);

		on_l = new List<PointF> ();
		off_l = new List<PointF> ();
		//onLast4Relevant_l = new List<PointF> ();
	}

	public override void UpdateSteps (FourPlatformsEvent fpe, List<double> timeAccu_l, int y)
	{
		// 1 update lists
		if (fpe.Time > 0) // is contact (in positive we have time flying)
			on_l.Add (new PointF (timeAccu_l[fpe.Button], y));
		else
			off_l.Add (new PointF (timeAccu_l[fpe.Button], y));

		// 2 exit if less than 4 contacts
		if (on_l.Count < 4)
			return;

		// 3 create onLast4Relevant_l list omitting same sensor consecutive repeated values
		List<PointF> onLast4Relevant_l = createLast4RelevantList ();

		// 2 exit if less than 4 relevant contacts
		if (onLast4Relevant_l.Count < 4)
			return;

		//TODO: continue
		// 3 exit if we cannot find the flight after the onLast4
		PointF ol4go = getStepStartOff (onLast4Relevant_l[3]);
		if (ol4go.Y < 0)
			return;

		// 4 check if step is completed
		if (
				onLast4Relevant_l[0].Y >= 3 && onLast4Relevant_l[1].Y >= 3 && 	// Last two contacts are 3 or 4
				onLast4Relevant_l[0].Y != onLast4Relevant_l[1].Y &&		// Last two contacts are different
				onLast4Relevant_l[2].Y <= 2 && onLast4Relevant_l[3].Y <= 2 &&
				onLast4Relevant_l[2].Y != onLast4Relevant_l[3].Y)
		{
			stepsBottom_l.Add (ol4go);
			stepsTop_l.Add (onLast4Relevant_l[0]);

			if (stepsCompleted == 0)
				timeStart = ol4go.X;

			stepsCompleted ++;

			if (stepsCompleted >= stepsTotal)
				timeEnd = onLast4Relevant_l[0].X;

			// reset on_l, off_l to not count again on next contact
			on_l = new List<PointF> ();
			off_l = new List<PointF> ();
			//onLast4Relevant_l = new List<PointF> ();
		}
	}

	// omit same sensor consecutive repeated values
	// if the penultimate or antepenultimate have duplicated contacts (with same sensor) take the first one
	// and manage correctly the pos for the next relavant contacts
	private List<PointF> createLast4RelevantList ()
	{
		List<PointF> l = new List<PointF> ();

		// last (take current)
		int pos = on_l.Count -1;
		l.Add (on_l[pos]);

		// penultimate
		pos --;
		int found = pos;
		for (int i = pos-1; i >= 0 && on_l[i].Y == on_l[pos].Y; i --)
			found --;

		if (found < 0)
			return l;

		pos = found;
		l.Add (on_l[pos]);

		// antepenultimate
		pos --;
		found = pos;
		for (int i = pos-1; i >= 0 && on_l[i].Y == on_l[pos].Y; i --)
			found --;

		if (found < 0)
			return l;

		pos = found;
		l.Add (on_l[pos]);

		// first (take always the more at right)
		pos --;
		if (pos < 0)
			return l;

		l.Add (on_l[pos]);
		return l;
	}

	private PointF getStepStartOff (PointF stepStartContact)
	{
		foreach (PointF p in off_l)
			if (p.X > stepStartContact.X && p.Y == stepStartContact.Y)
				return p;

		return new PointF (-1, -1);
	}
}
