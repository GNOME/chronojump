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
	private bool bluetoothUse;
	private BluetoothDataList bluetoothDataList;
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
			bool bluetoothUse,
			BluetoothDataList bluetoothDataList, 	// the growing list of data
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
		this.bluetoothUse = bluetoothUse;
		this.bluetoothDataList = bluetoothDataList;
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

		if (! bluetoothUse)
		{
			fpc.Reset ();
			if (! fpc.CaptureStart ())
				return false;
		}

		return true;
	}

	public void Capture ()
	{
		finish = false;

		List<double> timeAccu_l = new List<double> (); //double to use PointF (in seconds)
		for (int i = 0; i <= 3 ; i ++)
			timeAccu_l.Add (0);

		int yPre = -1; //for FROMLOWTOHIGH
		FourPlatformsEvent fpe;

		while (! finish && ! cancel)// && ! error)
		{
			// 1) read fpe
			if (bluetoothUse)
			{
				if (! bluetoothDataList.CanReadFromList ())
					continue;

				fpe = (bluetoothDataList.ReadNext ()).ToFourPlatformsEvent ();
			} else {
				if(! fpc.CaptureSample ())
					cancel = true; //problem reading line (capturing)

				if (! fpc.CanReadFromList ())
					continue;

				fpe = fpc.FourPlatformsCaptureReadNext ();
			}
			LogB.Information("fpe: " + fpe.ToString());

			if (fpe.Button < 0)
			{
				LogB.Information("problem reading");
				continue;
			}
			//fpe.Time *= -1; //first buttons prototype board has the buttons behaviour opposite than the final board

			// 2) process fpe
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

		if (! bluetoothUse)
		{
			LogB.Information ("calling Stop");
			fpc.Stop ();
		}
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
