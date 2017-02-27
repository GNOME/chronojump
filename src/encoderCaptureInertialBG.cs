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
	private int angleNow;
	private bool finishBG;
	public bool StoreData;
	private SerialPort sp;

	public EncoderCaptureInertialBackground(string port)
	{
		angleNow = 0;
		finishBG = false;
		StoreData = false;
		EncoderCaptureInertialBackgroundStatic.Start();

		sp = new SerialPort(port);
		sp.BaudRate = 115200;
		LogB.Information("sp created");
	}

	public bool CaptureBG()
	{
		LogB.Information("CaptureBG!");
		sp.Open();
		LogB.Information("sp opened");

		int byteReaded;
		do {
			try {
				byteReaded = readByte();
			} catch {
				LogB.Error("ERROR at InertialCaptureBackground: Maybe encoder cable is disconnected");
				return false;
			}

			byteReaded = convertByte(byteReaded);
			angleNow += byteReaded;

			if(StoreData)
				EncoderCaptureInertialBackgroundStatic.ListCaptured.Add(byteReaded);
			//LogB.Information("angleNow = " + angleNow.ToString());
		} while (! finishBG);

		sp.Close();
		return true;
	}

	private int readByte()
	{
		return sp.ReadByte();
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
	public static List<int> ListCaptured;
	private static int pos;

	//abort allow to finish the capture process and don't get waiting GetNext forever
	private static bool abort;

	public static void Start()
	{
		abort = false;
		ListCaptured = new List<int> ();
		Initialize();
	}

	public static void Initialize()
	{
		ListCaptured.Clear();
		pos = 0;
	}

	//TODO: write nicer
	public static int GetNext()
	{
		if(abort)
			return 0;

		do {
			if(ListCaptured.Count > pos)
				return ListCaptured[pos ++];

			System.Threading.Thread.Sleep(25);
		} while (! abort);

		return 0;
	}

	public static void Abort()
	{
		abort = true;
	}
}
