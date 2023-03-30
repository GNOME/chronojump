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
 *  Copyright (C) 2017   Xavier de Blas <xaviblas@gmail.com> 
 */

using System;
using System.IO.Ports;
using System.Collections.Generic; //List<T>

/*
 * this class allows reading always in order to know the angle of the string and the change of direction
 */
public class EncoderCaptureInertialBackground
{
	/*
	 * angleNow is useful also for showing "ecc" / "con" on gui, there needed to change  from con to ecc
	 * but for changing from ecc to con we need angleMaxAbsoluteThisPhase
	 */
	private int angleNow;
	private int angleMaxAbsoluteThisPhase;

	public enum Phases { ECC, CON, ATCALIBRATEDPOINT } 	//ATCALIBRATEDPOINT: Do not diplay ecc or con labels
	public Phases Phase;

	private bool finishBG;
	public bool StoreData;
	private SerialPort sp;
	private Random rand;

	//send port == "" for simulated
	public EncoderCaptureInertialBackground(string port)
	{
		angleNow = 0;
		angleMaxAbsoluteThisPhase = 0;
		finishBG = false;
		StoreData = false;
		EncoderCaptureInertialBackgroundStatic.Start();

		if(port != "")
		{
			sp = new SerialPort(port);
			sp.BaudRate = 115200;
			LogB.Information("sp created");
		}
	}

	public bool CaptureBG(bool simulated)
	{
		LogB.Information("CaptureBG!");
		if(simulated)
		{
			rand = new Random(40);
			SimulatedReset();
		}
		else {
			sp.Open();
			LogB.Information("sp opened");

			flush ();
		}

		int byteReaded;
		do {
			try {
				if(simulated)
					byteReaded = simulateByte();
				else
					byteReaded = readByte();
			} catch {
				LogB.Error("ERROR at InertialCaptureBackground: Maybe encoder cable is disconnected");
				return false;
			}

			byteReaded = convertByte(byteReaded);
			angleNow += byteReaded;

			//LogB.Information(string.Format("PRE: ANGLE: {0}, MAXABS: {1}, PHASE: {2}", angleNow, angleMaxAbsoluteThisPhase, Phase));
			if(angleNow == 0)
				Phase = Phases.ATCALIBRATEDPOINT;
			else if(angleNow == angleMaxAbsoluteThisPhase)
			{
				/*
				 * Do not do this
				 * Phase = Phases.NOTMOVED;
				 * because if we are at calibration point, then move 1 mm to ecc. (but by threads maybe is not shown on gui/encoder.cs)
				 * so then if phase is NOTMOVED, will continue showing the ATCALIBRATEDPOINT. So if speed is low maybe all the time both labels are not shown
				 * better do not do nothing and the Phase will be the same than before, in that case will be ECC
				 */
			}
			else if(angleNow > 0)
			{
				if(angleNow > angleMaxAbsoluteThisPhase)
				{
					Phase = Phases.ECC;
					angleMaxAbsoluteThisPhase = angleNow;
				} else //if(angleNow < angleMaxAbsoluteThisPhase)
					Phase = Phases.CON;
			}
			else //if(angleNow < 0)
			{
				if(angleNow < angleMaxAbsoluteThisPhase)
				{
					Phase = Phases.ECC;
					angleMaxAbsoluteThisPhase = angleNow;
				} else //if(angleNow > angleMaxAbsoluteThisPhase)
					Phase = Phases.CON;
			}
			//LogB.Information(string.Format("POST: ANGLE: {0}, MAXABS: {1}, PHASE: {2}", angleNow, angleMaxAbsoluteThisPhase, Phase));

			if(StoreData)
				EncoderCaptureInertialBackgroundStatic.ListCaptured.Add((short) byteReaded);
			//LogB.Information("angleNow = " + angleNow.ToString());
		} while (! finishBG);

		if(! simulated)
			sp.Close();

		return true;
	}

	private int readByte()
	{
		return sp.ReadByte();
	}

	/*
	 * methods copied from chronopic.cs
	 * it solves bad calibration at first inertial capture on windows
	 */
	private void flush()
	{
		LogB.Information("going to flush");

		//try, catch done because mono-1.2.3 throws an exception when there's a timeout
		//http://bugzilla.gnome.org/show_bug.cgi?id=420520
		bool success = false;
		//AbortFlush = false;
		do{
			try {
				for (int i = 0; i < 10; i ++)
					LogB.Information (sp.ReadByte ().ToString ());

				success = true;
				LogB.Debug(" flushed ");
			} catch {
				LogB.Warning(" catchedTimeOut at flushing ");
			}

		} while(! success);/* && ! AbortFlush);
		if(AbortFlush) {
			LogB.Information("Abort flush");
		} */

		LogB.Information("flushed");
	}

	private bool simulatedGoingUp = false;
	private int simulatedMaxValue = 400;
	private int simulatedLength;
	private int simulatedMaxLength = 10000; //ms when signal starts to shows 0s (will be stopped 3s again, depending on preferences)

	public void SimulatedReset()
	{
		simulatedLength = 0;
	}

	private int simulateByte()
	{
		System.Threading.Thread.Sleep(1);

		//return 0's to end if signal needs to end
		simulatedLength ++;
		if(simulatedLength > simulatedMaxLength)
			return 0;

		//get new value
		int simValue;
		if(simulatedGoingUp)
			simValue = rand.Next(0, 4);
		else
			simValue = rand.Next(-4, 0);

		//change direction if needed
		if(simulatedGoingUp && angleNow > simulatedMaxValue)
			simulatedGoingUp = false;
		else if(! simulatedGoingUp && angleNow < -1 * simulatedMaxValue)
			simulatedGoingUp = true;

		return simValue;
	}

	private int convertByte(int b)
	{
		if(b > 128)
			b = b - 256;

		return b;
	}

	public int AngleNow
	{
		get { return angleNow; }
		set { angleNow = value; } //if user recalibrates again
	}

	public void FinishBG()
	{
		finishBG = true;
	}

}

/*
 * Readed by EncoderCaptureInertial when calibration is done
 */
public static class EncoderCaptureInertialBackgroundStatic
{
	public static List<short> ListCaptured;
	private static int pos;

	//abort allow to finish the capture process and don't get waiting GetNext forever
	private static bool abort;

	public static void Start()
	{
		abort = false;
		ListCaptured = new List<short> ();
		Initialize();
	}

	public static void Initialize()
	{
		ListCaptured.Clear();
		pos = 0;
	}

	//TODO: write nicer
	public static short GetNext()
	{
		if(abort)
			return 0;

		do {
			if(ListCaptured != null && ListCaptured.Count > pos)
				return ListCaptured[pos ++];

			//LogB.Information("Problems at GetNext, L.Count: " + ListCaptured.Count.ToString() + ";pos: " + pos.ToString());
			System.Threading.Thread.Sleep(5);
		} while (! abort);

		return 0;
	}

	public static void Abort()
	{
		abort = true;
	}
}
