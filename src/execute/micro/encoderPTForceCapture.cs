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


/* data format
- forceSensor:
micros;force;2

- encoder
micros;pps;3
pps is always -10 or 10
*/

//EncoderPulseTimeForceCapture
public class EncoderPTForceCapture: ArduinoCapture
{
	private List<EncoderPTForceEvent> list = new List<EncoderPTForceEvent>();
	//private double runEncoderPPS; //TODO: name it pps
	private int bauds = 115200;
	private string firmwareVersion;
	private string portName;
	private bool testing;

	//constructor
	public EncoderPTForceCapture (string portName, bool testing)//, double runEncoderPPS)
	{
		this.portName = portName;
		this.testing = testing;
		//this.runEncoderPPS = runEncoderPPS;

		cancel = false;

		if (micro == null || micro.PortName != portName || micro.Bauds != bauds)
			micro = new Micro (portName, bauds);

		Reset ();
	}

	//after a first capture, put variales to zero
	public void Reset ()
	{
		initialize ();
	}

	public override bool CaptureStart()
	{
		LogB.Information("CaptureStart, micro.Opened: " + micro.Opened);
		// 0 connect if needed
		List<string> responseExpected_l = new List<string>();
		if(! micro.Opened)
		{
			responseExpected_l.Add("EncoderForce-"); //right now reading using RaceAnalyzer electronics

			if(! portConnect (true))
				return false;
			
			Thread.Sleep(1000); //need to sleep 1s more 

			if(! getVersion ("get_version:", responseExpected_l, false, 2000, false))
				return false;

			LogB.Information ("response: |" + micro.Response + "|");
			firmwareVersion = micro.Response;

			Match match = Regex.Match (firmwareVersion, @"EncoderForce-(\d+\.\d+)");
			if(match.Groups.Count == 2)
				firmwareVersion = match.Groups[1].ToString();
		}
		micro.Opened = true;

		LogB.Information ("version: |" + firmwareVersion + "|");
		double versionDouble = Convert.ToDouble(Util.ChangeDecimalSeparator(firmwareVersion));
		
		/* not implemented yet
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
		*/

		LogB.Information ("pps set to |" + micro.Response + "|");

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

		if (testing && ! sendCommand ("start_capture_testing:", "Catched ForceEncoder capture_testing"))
		//if(! sendCommand ("start_simulation:", "Catched run encoder capturing"))
		{
			return false;
		}
		
		if (! testing && ! sendCommand ("start_capture:", "Catched ForceEncoder capturing"))
		//if(! sendCommand ("start_simulation:", "Catched run encoder capturing"))
		{
			return false;
		}

		responseExpected_l = new List<string>();
		responseExpected_l.Add ("Starting capture");
	
		bool responseOk = waitResponse ("Starting capture", false, 2000, false);

		return responseOk;
	}

	public bool BytesToReadEnoughForASample ()
	{
		return micro.BytesToReadAtLeast (12);
	}

	public override bool CaptureSample ()
	{
		if (testing)
			return captureSampleTesting ();
		else
			return captureSampleNormal ();
	}

	//if true: continue capturing; if false: error, end
	private bool captureSampleTesting ()
	{
		/*
		 * if at CaptureStart device is disconnected,
		 * micro gets closed there and here it shoud not readLine
		 */
		if (! micro.Opened)
			return false;

		if(! readBinarySampleTesting ())
		{
			micro.ClosePort ();
			return false;
		}

		return true;
	}

	private bool captureSampleNormal ()
	{
		/*
		 * if at CaptureStart device is disconnected,
		 * micro gets closed there and here it shoud not readLine
		 */
		if (! micro.Opened)
			return false;

		List<int> row_l;
		if(! readBinarySample (out row_l))
		{
			micro.ClosePort ();
			return false;
		}

		EncoderPTForceEvent eptfe = new EncoderPTForceEvent (row_l);
		list.Add (eptfe);

		return true;
	}

	public override bool Stop()
	{
		LogB.Information("AT Capture: STOPPING");

		//empty any pending port read to be able to read correctly the Capture ended message
		flush();

		if (! sendCommand("end_capture:", "Catched at end_capture:"))
			return false;

		if (waitResponse ("Capture ended", false, 4000, false))
			LogB.Information("AT Capture: STOPPED");
		else
		{
			LogB.Information("AT Capture: cannot stop, going to Disconnect");
			Disconnect ();
		}

		LogB.Information("Stop, micro.Opened: " + micro.Opened);

		return true;
	}

	public override bool CanReadFromList ()
	{
		return (list.Count > readedPos);
	}

	public List<EncoderPTForceEvent> GetList()
	{
		return list;
	}

	public EncoderPTForceEvent ReadNext()
	{
		return list[readedPos++];
	}

	// protected stuff ---->

	protected override void emptyList()
	{
		list = new List<EncoderPTForceEvent>();
	}

	// private stuff ---->


	/*
		12 bytes
		sensor_t sensorType 2 galga, 3 encoder
		unsigned int time són 4 bytes
		float data: 4 bytes encoder o galga
		*/

	private bool readBinarySampleTesting ()
	{
		micro.BufferInit ();
		int bytesRead = 0;
		try {
			bytesRead = micro.ReadWithBuffer (0, 12);
		}
		catch (Exception ex)
		{
			if(ex is System.IO.IOException || ex is System.TimeoutException)
				LogB.Information ("catched on readBinarySampleTesting port.Read ()");

			return false;
		}

		LogB.Information("encoderPTCapture start reading binary data");
		int b0, b1, b2, b3;
		int count = 0;

                b0 = micro.GetBufferAtPos (count ++);
                b1 = micro.GetBufferAtPos (count ++);
                b2 = micro.GetBufferAtPos (count ++);
                b3 = micro.GetBufferAtPos (count ++);

                LogB.Information (string.Format ("\n\n- readed: {0} {1} {2} {3}", b0, b1, b2, b3));
		if (string.Format ("{0}{1}{2}{3}", b0, b1, b2, b3) != "1211109")
		{
			LogB.Information ("FAIL 1211109");
			return false;
		}

                b0 = micro.GetBufferAtPos (count ++);
                b1 = micro.GetBufferAtPos (count ++);
                b2 = micro.GetBufferAtPos (count ++);
                b3 = micro.GetBufferAtPos (count ++);

                LogB.Information (string.Format ("\n\n- readed: {0} {1} {2} {3}", b0, b1, b2, b3));
		if (string.Format ("{0}{1}{2}{3}", b0, b1, b2, b3) != "8765")
		{
			LogB.Information ("FAIL 8765");
			return false;
		}

                b0 = micro.GetBufferAtPos (count ++);
                b1 = micro.GetBufferAtPos (count ++);
                b2 = micro.GetBufferAtPos (count ++);
                b3 = micro.GetBufferAtPos (count ++);

                LogB.Information (string.Format ("\n\n- readed: {0} {1} {2} {3}", b0, b1, b2, b3));
		if (string.Format ("{0}{1}{2}{3}", b0, b1, b2, b3) != "4321")
		{
			LogB.Information ("FAIL 1234");
			return false;
		}


		return true;
	}
	
	private bool readBinarySample (out List<int> row_l)
	{
		row_l = new List<int> ();

		return true;
	}

	/*

                LogB.Information (string.Format ("\n\n- sensorType: {0} {1} {2} {3}", b0, b1, b2, b3));

                //iid.ia = Convert.ToInt32 (
		row_l.Add (Convert.ToInt32 (
					Math.Pow(256,3) * b3 +
					Math.Pow(256,2) * b2 +
					Math.Pow(256,1) * b1 +
					Math.Pow(256,0) * b0));

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

                LogB.Information (string.Format ("- time: {0} {1} {2} {3}", b0, b1, b2, b3));

                //iid.ib = Convert.ToInt32 (
		row_l.Add (Convert.ToInt32 (
					Math.Pow(256,3) * b3 +
					Math.Pow(256,2) * b2 +
					Math.Pow(256,1) * b1 +
					Math.Pow(256,0) * b0));
		
		// 3) read force or encoder
                if (! readByte(out b0))
			return false;
                if (! readByte(out b1))
			return false;
                if (! readByte(out b2))
			return false;
                if (! readByte(out b3))
			return false;

                LogB.Information (string.Format ("- sensorData: {0} {1} {2} {3}", b0, b1, b2, b3));

		//iid.d = 
		uint sensorData = Convert.ToUInt32 (
					Math.Pow(256,3) * b3 +
					Math.Pow(256,2) * b2 +
					Math.Pow(256,1) * b1 +
					Math.Pow(256,0) * b0);
		LogB.Information ("sensorData: " + sensorData.ToString ());

		// TODO:
		//care for negative values (4 bytes)
		if (sensorData > 2147483648) // 256^4 / 2
		{
			LogB.Information ("will be str: " + (-1 * (4294967296 - sensorData)).ToString ());
			LogB.Information (string.Format ("will be uint: {0}", Convert.ToInt32 ((4294967296 - sensorData))
						));
			LogB.Information (string.Format ("will be int: {0}", Convert.ToInt32 (-1 * (4294967296 - sensorData))
						));
			row_l.Add (Convert.ToInt32 (-1 * (4294967296 - sensorData)));
		} else
			row_l.Add (Convert.ToInt32 (sensorData));

		//LogB.Information("encoderPTCapture readed all binary data");
		//
		if (row_l[0] > 4)
		{
			LogB.Information ("arrggggg");
			return false;
		}

                return true;
	}
*/

	public string PortName {
		get { return portName; }
	}

	/*
	public double RunEncoderPPS {
		get { return runEncoderPPS; }
		set { runEncoderPPS = value; }
	}
	*/
}

public class EncoderPTForceEvent
{
	public int Type;
	public int Time;
	//public double SensorValue;
	public int SensorValue;

	/*
	public EncoderPTForceEvent()
	{
	}
	*/

	//public EncoderPTForceEvent (IntIntDouble iid)
	public EncoderPTForceEvent (List<int> row_l)
	{
		this.Type = row_l[0];
		this.Time = row_l[1];
		this.SensorValue = row_l[2];
	}

	public override string ToString()
	{
		return (string.Format("{0};{1};{2}", Type, Time, SensorValue));
	}
}
