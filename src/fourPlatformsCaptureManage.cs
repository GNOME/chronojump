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
	private int stepsTotal;

	//both these will be used to record final time
	private double timeStart; //on steps is start of the 1st valid step. On default (not steps) is first on or off.
	private double timeEnd; //on steps end of the last valid step. On default (not steps) is last on or off.

	public FourPlatformsCaptureManage (
			Constants.Modes mode,
			CaptureEnum captureType,
			int stepsTotal,
			FourPlatformsCapture fpc,
			ref List<List<PointF>> points_ll,
			ref List<PointF> stepsBottom_l,
			ref List<PointF> stepsTop_l,
			List<IDName> idName_l
			)
	{
		this.mode = mode;
		this.captureType = captureType;
		this.stepsTotal = stepsTotal;
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

				int timeNow = fpe.Time; //micros

				//int button = fpe.Button + 1; //from 0-3 to 1-4
				//have button as positive or negative and put timeNow as positive
				if (timeNow < 0)
					timeNow = Math.Abs (timeNow);

				timeAccu_l[fpe.Button] += UtilAll.DivideSafe (timeNow, 1000000);

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

	List<PointF> on3_l; // when landed: contact 3
	List<PointF> on4_l; // when landed: contact 4
	List<PointF> off1_l; // when leave: contact 1
	List<PointF> off2_l; // when leave: contact 2

	private bool lastContactIs3;
	private bool firstOffIs1;

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
		LogB.Information ("timeAccu_l:");
		LogB.Information (UtilList.ListDoubleToString (timeAccu_l, 2, " "));
		LogB.Information ("fpe:");
		LogB.Information (fpe.ToString ());

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
		// if off on 1 and 2 are more on the right than 3 on or 4 on (strange, but ...) return
		if (PointF.Last (off1_l).X > PointF.Last (on3_l).X && PointF.Last (off1_l).X > PointF.Last (on4_l).X)
			return;
		if (PointF.Last (off2_l).X > PointF.Last (on3_l).X && PointF.Last (off2_l).X > PointF.Last (on4_l).X)
			return;

		LogB.Information ("no off more at right than ons");
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

		if (stepsCompleted >= stepsTotal)
			timeEnd = last4Relevant_l[0].X;

		resetLists (); // reset lists to not count again on next contact
	}

	// check last two ON on 3, 4
	// check last two OFF on 1, 2
	// there should be an ON on absolute right
	// there should be an OFF on absolute left
	// TODO:
	// 	if repeated on the same channel, on ON take the left
	// 	if repeated on the same channel, on OFF take the right

	private void resetLists ()
	{
		on_l = new List<PointF> ();
		off_l = new List<PointF> ();

		on3_l = new List<PointF> ();
		on4_l = new List<PointF> ();
		off1_l = new List<PointF> ();
		off2_l = new List<PointF> ();
	}

	private List<PointF> createLast4RelevantList ()
	{
		List<PointF> l = new List<PointF> (); //from newest (right) to oldest (left)

		lastContactIs3 = false;
		if (PointF.Last (on3_l).X > PointF.Last (on4_l).X)
			lastContactIs3 = true;
		
		firstOffIs1 = false;
		if (PointF.Last (off1_l).X < PointF.Last (off2_l).X)
			firstOffIs1 = true;

		if (lastContactIs3) //TODO: add this code above
		{
			l.Add (PointF.Last (on3_l));
			l.Add (PointF.Last (on4_l));
		} else {
			l.Add (PointF.Last (on4_l));
			l.Add (PointF.Last (on3_l));
		}
			
		if (firstOffIs1) //TODO: add this code above
		{
			l.Add (PointF.Last (off2_l));
			l.Add (PointF.Last (off1_l));
		} else {
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

		foreach (PointF p in off_l)
			if (p.Y <= 2 && p.X >= previousTopOffX)
				return true;

		return false;
	}
}
