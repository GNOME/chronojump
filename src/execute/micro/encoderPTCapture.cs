/*
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA 02111-1307, USA.
 *
 * Copyright (C) 2024  Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List<T>
using System.Diagnostics;  //Stopwatch
using System.IO.Ports;
using System.Threading;
using System.Text.RegularExpressions; //Regex

//EncoderPulseTimeCapture
public class EncoderPTCapture: ArduinoCapture
{
	private List<EncoderPTEvent> list = new List<EncoderPTEvent>();
	private double runEncoderPPS;

	//constructor
	public EncoderPTCapture (string portName, double runEncoderPPS)
	{
		this.runEncoderPPS = runEncoderPPS;

		cancel = false;
		micro = new Micro (portName, 115200);
		Reset ();
	}

	//after a first capture, put variales to zero
	public void Reset ()
	{
		initialize ();
	}

	public override bool CaptureStart()
	{
		LogB.Information("portOpened: " + micro.Opened);
		// 0 connect if needed
		List<string> responseExpected_l = new List<string>();
		if(! micro.Opened)
		{
			responseExpected_l.Add("Race_Analyzer-"); //right now reading using RaceAnalyzer electronics

			if(! portConnect (true))
				return false;
			
			Thread.Sleep(1000); //need to sleep 1s more 

			if(! getVersion ("get_version:", responseExpected_l, false, 2000, false))
				return false;

			LogB.Information ("response: |" + micro.Response + "|");
			string version = micro.Response;

			Match match = Regex.Match(version, @"Race_Analyzer-(\d+\.\d+)");
			if(match.Groups.Count == 2)
				version = match.Groups[1].ToString();
			else
				version = "0.3"; //if there is a problem default to 0.3. 0.2 was the first that will be distributed and will be on binary. 0.3 has the byte of encoderOrRCA
			
			LogB.Information ("version: |" + version + "|");

			double versionDouble = Convert.ToDouble(Util.ChangeDecimalSeparator(version));
			if(versionDouble >= Convert.ToDouble(Util.ChangeDecimalSeparator("0.3")))
			{
				if(! sendCommand(string.Format("set_pps:{0};", runEncoderPPS), "Catched at set_pps"))
				{
					//runEncoderProcessError = true;
					LogB.Information ("Error at set_pps");
					return false;
				}

				//read confirmation data
				if(! waitResponse ("pps set to", false, 2000, false))
				{
					//runEncoderProcessError = true;
					LogB.Information ("Error at receive pps set to");
					return false;
				}
					
				LogB.Information ("pps set to |" + micro.Response + "|");
			}
		}

		micro.Opened = true;

		//LogB.Information(string.Format("arduinoCapture portName: {0}, bauds: {1}", portName, bauds));

		//empty the port before new capture
		/*
		 * note a detected device if usb cable gets disconnected, then micro.Opened above is true,
		 * so previous to 22 may 2023 comes here and crashes. Now flush has a try/catch and returns a boolean,
		 * and CaptureStart return also is managed on execute/run.cs
		 */
		if (! flush())
		{
			LogB.Information ("device has been disconnected");
			micro.ClosePort ();
			return false;
		}

		if(! sendCommand ("start_capture:", "Catched run encoder capturing"))
		//if(! sendCommand ("start_simulation:", "Catched run encoder capturing"))
		{
			return false;
		}

		responseExpected_l = new List<string>();
		responseExpected_l.Add ("Starting capture");
	
		//return waitResponse (responseExpected_l, true, 2000);
		//return waitResponse (responseExpected_l, false, 2000);
		bool responseOk = waitResponse ("Starting capture", false, 2000, false);

		return responseOk;
	}

	public bool BytesToReadEnoughForASample ()
	{
		return micro.BytesToReadAtLeast (9);
	}

	//if true: continue capturing; if false: error, end
	public override bool CaptureSample ()
	{
		//string str = "";
                List<int> dataRow;

		/*
		 * if at CaptureStart device is disconnected,
		 * micro gets closed there and here it shoud not readLine
		 */
		if (! micro.Opened)
			return false;

		//if(! readLine (out str))
		if(! readBinarySample (out dataRow))
		{
			micro.ClosePort ();
			return false;
		}

		EncoderPTEvent epte = new EncoderPTEvent(dataRow);
		list.Add (epte);

		return true;
	}

	public override bool Stop()
	{
		LogB.Information("AT Capture: STOPPING");

		//empty any pending port read to be able to read correctly the Capture ended message
		flush();

		if (! sendCommand("end_capture:", "Catched at end_capture:"))
			return false;

		waitResponse ("Capture ended", false, 2000, false);

		LogB.Information("AT Capture: STOPPED");

		/*
		Disconnect ();
		micro.PortOpened = false;
		*/

		return true;
	}

	public override bool CanReadFromList ()
	{
		return (list.Count > readedPos);
	}

	public List<EncoderPTEvent> EncoderPTCaptureGetList()
	{
		return list;
	}

	public EncoderPTEvent EncoderPTCaptureReadNext()
	{
		return list[readedPos++];
	}

	// protected stuff ---->

	protected override void emptyList()
	{
		list = new List<EncoderPTEvent>();
	}

	// private stuff ---->

	//from gui/app1/runEncoder.cs readBinaryRunEncoderValues ()
	private bool readBinarySample (out List<int>dataRow)
	{
		//LogB.Information("encoderPTCapture start reading binary data");
		int b0, b1, b2, b3;
		dataRow = new List<int>();

		// 1) encoderDisplacement (2 bytes)
		//b0/b1  least/most significative
                if (! readByte(out b0))
			return false;
                if (! readByte(out b1))
			return false;

		int readedNum = Convert.ToInt32(256 * b1 + b0);

		//care for negative values
		if(readedNum > 32768)
			readedNum = -1 * (65536 - readedNum);

		//LogB.Information (string.Format ("displacement: {0}", readedNum));
		dataRow.Add(readedNum);

		// 2) read time, four bytes
		//b0: least significative
                //b3: most significative
		//b0/b3  least/most significative
                if (! readByte(out b0))
			return false;
                if (! readByte(out b1))
			return false;
                if (! readByte(out b2))
			return false;
                if (! readByte(out b3))
			return false;

		/*
		LogB.Information (string.Format ("time: {0}",
					Convert.ToInt32(
						Math.Pow(256,3) * b3 +
						Math.Pow(256,2) * b2 +
						Math.Pow(256,1) * b1 +
						Math.Pow(256,0) * b0))
				);
				*/

                dataRow.Add(Convert.ToInt32(
                                Math.Pow(256,3) * b3 +
                                Math.Pow(256,2) * b2 +
                                Math.Pow(256,1) * b1 +
                                Math.Pow(256,0) * b0));
		
		// 3) read force, two bytes //UNUSED
		//b0/b1  least/most significative
                if (! readByte(out b0))
			return false;
                if (! readByte(out b1))
			return false;
		readedNum = Convert.ToInt32(256 * b1 + b0);

		/*
		 * 4) byte for encoder or RCA
		 * 0 encoder data
		 * 1 RCA down (button is released)
		 * 2 RCA up (button is pressed)
		 * UNUSED
		 */
                if (! readByte(out b0))
			return false;

		//LogB.Information("encoderPTCapture readed all binary data");
                return true;
	}
}

public class EncoderPTEvent
{
	public int Distance;
	public int Time;

	/*
	public EncoderPTEvent()
	{
	}
	*/

	public EncoderPTEvent (List<int> dataRow)
	{
		this.Distance = dataRow[0];
		this.Time = dataRow[1];
	}

	public override string ToString()
	{
		return (string.Format("{0};{1}", Distance, Time));
	}
}
