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
	public enum CaptureEnum { DEFAULT, FROM1TO2, FROM1TO3, FROM1TO4 };

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

	// for CaptureEnum: FROM1TO2, FROM1TO3, FROM1TO4
	private enum StepsStatusEnum { NOTSTARTED, DONEBOTTOM, DONETOP };
	private StepsStatusEnum stepsStatusEnum;
	private int stepsCompleted;
	private int stepsTotal = 15;

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
		stepsStatusEnum = StepsStatusEnum.NOTSTARTED;
		stepsCompleted = 0;

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

		List<double> timeAccu_l = new List<double> (); //double to use PointF
		for (int i = 0; i <= 3 ; i ++)
			timeAccu_l.Add (0);

		while (! finish && ! cancel)// && ! error)
		{
			if(! fpc.CaptureSample ())
				cancel = true; //problem reading line (capturing)

			if (fpc.CanReadFromList ())
			{
				FourPlatformsEvent fpe = fpc.FourPlatformsCaptureReadNext();
				LogB.Information("fpe: " + fpe.ToString());

				fpe.Time *= -1; //first buttons prototype board has the buttons behaviour opposite than the final board

				int timeNow = fpe.Time; //millis

				//int button = fpe.Button + 1; //from 0-3 to 1-4
				//have button as positive or negative and put timeNow as positive
				if (timeNow < 0)
					timeNow = Math.Abs (timeNow);

				timeAccu_l[fpe.Button] += timeNow;

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
					if (captureType == CaptureEnum.FROM1TO2 || captureType == CaptureEnum.FROM1TO3 || captureType == CaptureEnum.FROM1TO4)
						updateStepsCaptureVariables (fpe, timeAccu_l, y);
					else {
						//1st contact will update timeStart
						if (timeStart == 0)
							timeStart = UtilAll.DivideSafe (timeAccu_l[fpe.Button], 1000);

						//all contacts will update timeEnd
						timeEnd = UtilAll.DivideSafe (timeAccu_l[fpe.Button], 1000);
					}
				}

				if (fpe.Time < 0)
					timesOff_ll[fpe.Button].Add (UtilAll.DivideSafe (timeAccu_l[fpe.Button], 1000)); //0-3 each of the sensors
				else
					timesOn_ll[fpe.Button].Add (UtilAll.DivideSafe (timeAccu_l[fpe.Button], 1000)); //0-3 each of the sensors

				//LogB.Information ("fpe.Button: " + fpe.Button);
				//LogB.Information ("y: " + y);
				//points_ll[0].Add (new PointF (timeAccu_l[fpe.Button], y+ySign)); //0 has all
				//in seconds
				points_ll[0].Add (new PointF (UtilAll.DivideSafe (timeAccu_l[fpe.Button], 1000), .1)); //0 has all //to debug
				points_ll[y].Add (new PointF (UtilAll.DivideSafe (timeAccu_l[fpe.Button], 1000), 5-y+ySign)); //1-4 each of the sensors

				if (stepsCompleted >= stepsTotal)
					finish = true;
			}
		}
		LogB.Information ("calling Stop");
		fpc.Stop ();
	}

	private void updateStepsCaptureVariables (FourPlatformsEvent fpe, List<double> timeAccu_l, int y)
	{
		//mark the bottom
		if (stepsStatusEnum != StepsStatusEnum.DONEBOTTOM && y == 1 && fpe.Time < 0)
		{
			stepsBottom_l.Add (new PointF (UtilAll.DivideSafe (timeAccu_l[fpe.Button], 1000), 1));
			if(stepsCompleted == 0)
				timeStart = UtilAll.DivideSafe (timeAccu_l[fpe.Button], 1000);

			stepsStatusEnum = StepsStatusEnum.DONEBOTTOM;
		}
		//update the bottom as maybe has been repeated later
		else if (stepsStatusEnum == StepsStatusEnum.DONEBOTTOM && y == 1 && fpe.Time < 0)
		{
			stepsBottom_l[stepsBottom_l.Count -1] = new PointF (UtilAll.DivideSafe (timeAccu_l[fpe.Button], 1000), 1);
			if(stepsCompleted == 0)
				timeStart = UtilAll.DivideSafe (timeAccu_l[fpe.Button], 1000);

			stepsStatusEnum = StepsStatusEnum.DONEBOTTOM;
		}

		//do the top
		if (stepsStatusEnum == StepsStatusEnum.DONEBOTTOM && fpe.Time > 0 && (
					(captureType == CaptureEnum.FROM1TO2 && y == 2) ||
					(captureType == CaptureEnum.FROM1TO3 && y == 3) ||
					(captureType == CaptureEnum.FROM1TO4 && y == 4)
					) )
		{
			stepsTop_l.Add (new PointF (UtilAll.DivideSafe (timeAccu_l[fpe.Button], 1000), y));
			stepsStatusEnum = StepsStatusEnum.DONETOP;
			stepsCompleted ++;

			if (stepsCompleted >= stepsTotal)
				timeEnd = UtilAll.DivideSafe (timeAccu_l[fpe.Button], 1000);
		}
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
		get { return stepsCompleted; }
	}
	public double TimeStart {
		get { return timeStart; }
	}
	public double TimeEnd {
		get { return timeEnd; }
	}
	public bool Finish {
		get { return finish; }
		set { finish = value; }
	}
	public bool Cancel {
		set { cancel = value; }
	}
}


