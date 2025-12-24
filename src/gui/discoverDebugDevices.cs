//TODO: afegir un manually assign (en el cas de races decidir quin dispositiu, i a salts s'hauria de poder decidir si és un fourPlatforms)

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
 *  Copyright (C) 2016-2025   Xavier de Blas <xaviblas@gmail.com>
 */

using System;
using System.Collections.Generic; //List<T>
using System.Threading;
using System.IO.Ports;
using System.Diagnostics;  //Stopwatch


public abstract class DebugDevices
{
	protected ChronopicRegisterPort crp;
	protected string title;
	protected string str;
	protected SerialPort port;
	protected bool done;
	protected string gettingVersionStr = "\n\n- Getting version …";

	protected abstract void debugDo ();

	protected bool portCreate ()
	{
		str += "\n\n- Creating port …";

		try {
			port = new SerialPort (crp.Port, 115200);
		}
		catch (System.IO.IOException)
		{
			str += "\n- Problems creating port";
			return false;
		}

		str += "\n- Successfully created port";
		return true;
	}

	protected bool portOpen ()
	{
		str += "\n\n- Opening port …";
		try {
			port.Open();
		}
		catch (System.IO.IOException)
		{
			//forceSensorOtherMessage = forceSensorNotConnectedString;
			str += "\n- Problems opening port";
			return false;
		}
		str += "\n- Successfully opened port";
		return true;
	}

	/*
		Thread.Sleep (3000); //sleep to let arduino start reading serial event

		LogB.Information ("Have wait 3 s");
                //double firmwareVersion = forceSensorCheckVersionDo(); //TODO: it uses portFS
	*/

	protected bool startCaptureArduino ()
	{
		// send message
		try {
			port.WriteLine ("start_capture:");
		}
		catch (Exception ex)
		{
			if(ex is System.IO.IOException || ex is System.TimeoutException)
			{
				str += "\n- Failed at sending message. Error: " + ex.ToString ();
				return false;
			}
		}

		// receive confirmation
		string s = "";
		do {
			Thread.Sleep(100); //sleep to let arduino start reading
			try {
				s = port.ReadLine().Trim();
			} catch (Exception ex) {
				str += "\n- Failed at receiving message. Error: " + ex.ToString ();
				return false;
			}
		}
		while(! s.Contains("Starting capture"));

		return true;
	}

	protected bool endCaptureArduino ()
	{
		// ending capture. Send message
		try {
			port.WriteLine ("end_capture:");
		}
		catch (Exception ex)
		{
			if(ex is System.IO.IOException || ex is System.TimeoutException)
			{
				str += "\n- Failed at sending message. Error: " + ex.ToString ();
				return false;
			}
		}

		// ending capture. Receive message
		string s = "";
		int notValidCommandCount = 0;
		do {
			Thread.Sleep(10);
			try {
				s = port.ReadLine();
			} catch (Exception ex) {
				str += "\n- Failed at receiving message. Error: " + ex.ToString ();
			}

			//2023 Aug 3: sometimes Arduino looses some chars. It seems only happens with this command because Arduino will be busy capturing
			//instead of "end_capture:" arrived "end_cture:" (found 2 times) "end_capte:", "end_ture:", "end_caure:"
			if (s.Contains ("Not a valid command"))
			{
				notValidCommandCount ++;

				if (notValidCommandCount > 10)
				{
					str += "\n- NotValidCommandCount > 10";
					return false;
				}

				try {
					port.WriteLine ("end_capture:");
				} catch (Exception ex) {
					str += "\n- Failed at sending message. Error: " + ex.ToString ();
					return false;
				}
			}
		}
		while(! s.Contains("Capture ended"));

		return true;
	}

	protected virtual bool readSomeData ()
	{
		return true;
	}

	protected bool portClose ()
	{
		str += "\n\n- Closing port …";
		try {
			port.Close();
		} 
		catch (System.IO.IOException)
		{
			str += "\n- Problems closing port";
			return false;
		}
		str += "\n- Closed! All ok.";

		return true;
	}

	// adapted from gui/app1/forceSensor.cs forceSensorCheckVersionDo ()
	protected bool getVersionArduino (string commandStr, string responseExpected)
	{
		str += gettingVersionStr;

		// send message
		try {
			port.WriteLine (commandStr);
		}
		catch (Exception ex)
		{
			if(ex is System.IO.IOException || ex is System.TimeoutException)
			{
				str += "\n- Failed at sending message. Error: " + ex.ToString ();
				return false;
			}
		}

		// get version
		string s = "";
		do {
			Thread.Sleep(100); //sleep to let arduino start reading
			try {
				s = port.ReadLine().Trim();
			} catch (Exception ex) {
				str += "\n- Failed at receiving message. Error: " + ex.ToString ();
				return false;
			}
		}
		while(! s.Contains(responseExpected));

		//if returns garbage and then the responseExpected, GetFromSubstring () removes the initial garbage. That garbage made the dialogMessage not shown anything from there.
		str += "\n- Version found is: " + Util.GetFromSubstring (s, responseExpected);
		return true;
	}

	public string Title {
		get { return title; }
	}
	public string Str {
		get { return str; }
	}
	public bool Done {
		get { return done; }
	}
}

public class DebugWichro : DebugDevices
{
	public DebugWichro (ChronopicRegisterPort crp)
	{
		this.crp = crp;
		title = "Testing WICHRO";
		gettingVersionStr = "\n\n- Getting version of the controller …";

		done = false;
		debugDo ();
		done = true;
	}

	protected override void debugDo ()
	{
		if (! portCreate ())
			return;

		if (! portOpen ())
			return;

		Thread.Sleep(3000); //sleep to let arduino start reading serial event
		LogB.Information ("Have wait 3 s");

		if (! getVersionArduino ("local:get_version:", "Wifi-Controller"))
			return;

		if (! discoverTerminals ()) // TODO: maybe in the future this will be shown on another button
			return;

		portClose ();
	}

	private bool discoverTerminals ()
	{
		str += "\n\n- Checking terminals …";

		// send message
		try {
			port.WriteLine ("local:discover;");
		}
		catch (Exception ex)
		{
			if(ex is System.IO.IOException || ex is System.TimeoutException)
			{
				str += "\n- Failed at sending message. Error: " + ex.ToString ();
				return false;
			}
		}

		// get discover message
		Stopwatch swTotal = new Stopwatch ();
		swTotal.Start ();
		List<IntInt> terminalVersion_l = new List<IntInt> ();

		string responseExpected = "terminals:";
		string s = "";
		do {
			Thread.Sleep(100); //sleep to let arduino start reading
			try {
				string line = port.ReadLine();

				/* example of returned data
				20;7578;1;1000025
				25;8640;1;1000013
				last column is the version: 1000025, we need to convert it to 25
				*/
				string [] sFull = line.Split(new char[] {';'});
				if (sFull.Length == 4 &&
						Util.IsNumber (sFull[0], false) &&
						sFull[3].StartsWith ("1") && sFull[3].Length > 1 &&
						Util.IsNumber (sFull[3].Substring (1, sFull[3].Length -1), false)) //note version is 1
					terminalVersion_l.Add (new IntInt (
								Convert.ToInt32 (sFull[0]),
								Convert.ToInt32 (sFull[3].Substring (1, sFull[3].Length -1))
								));

				s += line;
			} catch (Exception ex) {
				str += "\n- Failed at receiving message. Error: " + ex.ToString ();
				return false;
			}
		}
		while (! (s.Contains(responseExpected) || swTotal.Elapsed.TotalSeconds >= 30));

		if (! s.Contains(responseExpected))
		{
			str += "\n- Too much time (+30s) for receiving message.";
			return false;
		}

		if (terminalVersion_l.Count == 0)
			str += "\n- No terminals found.";
		else
			foreach (IntInt terminalVersion in terminalVersion_l)
				str += string.Format ("\n- Terminal: {0}, Version: {1}", terminalVersion.a, terminalVersion.b);

		return true;
	}
}

public class DebugRaceAnalyzer : DebugDevices
{
	public DebugRaceAnalyzer (ChronopicRegisterPort crp)
	{
		this.crp = crp;
		title = "Testing Race Analyzer";

		done = false;
		debugDo ();
		done = true;
	}

	protected override void debugDo ()
	{
		if (! portCreate ())
			return;

		if (! portOpen ())
			return;

		Thread.Sleep(3000); //sleep to let arduino start reading serial event
		LogB.Information ("Have wait 3 s");

		if (! getVersionArduino ("get_version:", "Race_Analyzer-"))
			return;

		if (! readSomeData ())
			return;

		portClose ();
	}

	protected override bool readSomeData ()
	{
		int seconds = 3;
		str += string.Format ("\n\n- Capturing {0} seconds …", seconds);

		if (! startCaptureArduino ())
			return false;

		// capture some data

		Stopwatch swTotal = new Stopwatch ();
		swTotal.Start ();

		int bytesToRead = 0;

		do {
			try {
				bytesToRead = port.BytesToRead;
			} catch {
				continue;
			}

			if (port.BytesToRead < 9) 	// readBinaryRunEncoder9Bytes will read 9 bytes
				continue;

			List<int> binaryReaded = readBinaryRunEncoder9Bytes ();

			// using pulses and not m because for m first we need to send pps. And check if version is ok for send pps.
			str += string.Format ("\n  {0} pulses \t {1} us",//; N\t {3} is RCA?",
					binaryReaded[0], binaryReaded[1]);//, binaryReaded[2], binaryReaded[3]);
					//binaryReaded[0] * 0.0030321, binaryReaded[1]);//, binaryReaded[2], binaryReaded[3]);

		} while (swTotal.Elapsed.TotalSeconds < 3);
		swTotal.Stop ();

		if (! endCaptureArduino ())
			return false;

		return true;
	}

	// copied from gui/app1/runEncoder.cs
	// time (4 bytes: long at Arduino, uint at c-sharp), force (2 bytes: uint), encoder/RCA (1 byte: uint)
	private List<int> readBinaryRunEncoder9Bytes ()
        {
                List<int> dataRow = new List<int>();

		var buffer = new byte[1024];
		int bytesRead = 0;
		try {
			bytesRead = port.Read (buffer, 0, 9);
		}
		catch (Exception ex)
		{
			if(ex is System.IO.IOException || ex is System.TimeoutException)
				LogB.Information ("catched on readBinaryRunEncoder9Bytes portRE.Read ()");

			return dataRow;
		}

		int count = 0;

		// 1) encoderDisplacement (2 bytes)
                int b0 = buffer[count ++]; //encoderDisplacement least significative
                int b1 = buffer[count ++]; //encoderDisplacement most significative
		int readedNum = Convert.ToInt32(256 * b1 + b0);

		//care for negative values
		if(readedNum > 32768)
			readedNum = -1 * (65536 - readedNum);

		dataRow.Add(readedNum);

		// 2) read time, four bytes
                b0 = buffer[count ++]; //least significative
                b1 = buffer[count ++];
                int b2 = buffer[count ++];
                int b3 = buffer[count ++]; //most significative

                dataRow.Add(Convert.ToInt32(
                                Math.Pow(256,3) * b3 +
                                Math.Pow(256,2) * b2 +
                                Math.Pow(256,1) * b1 +
                                Math.Pow(256,0) * b0));

		// 3) read force, two bytes
		b0 = buffer[count ++]; //least significative
		b1 = buffer[count ++]; //most significative
		readedNum = Convert.ToInt32(256 * b1 + b0);

		dataRow.Add(readedNum);

		/*
		 * 4) byte for encoder or RCA
		 * 0 encoder data
		 * 1 RCA down (button is released)
		 * 2 RCA up (button is pressed)
		 */
		b0 = buffer[count ++];
		dataRow.Add(Convert.ToInt32(b0));

                return dataRow;
        }
}

public class DebugForceSensor : DebugDevices
{
	public DebugForceSensor (ChronopicRegisterPort crp)
	{
		this.crp = crp;
		title = "Testing Force Sensor";

		done = false;
		debugDo ();
		done = true;
	}

	protected override void debugDo ()
	{
		if (! portCreate ())
			return;

		if (! portOpen ())
			return;

		Thread.Sleep(3000); //sleep to let arduino start reading serial event
		LogB.Information ("Have wait 3 s");

		if (! getVersionArduino ("get_version:", "Force_Sensor-"))
			return;

		if (! readSomeData ())
			return;

		portClose ();
	}

	// copied from gui/app1/forceSensor.cs
	protected override bool readSomeData ()
	{
		int samples = 10;
		str += string.Format ("\n\n- Capturing {0} samples …", samples);

		if (! startCaptureArduino ())
			return false;

		// capture some data
		string s = "";
		int count = 0;
		do {
			int time = 0;
			double force = 0;
			string triggerCode = "";
			s = port.ReadLine();
			if(! forceSensorProcessCapturedLine(s, out time, out force,
						false, out triggerCode)) //false: do not read triggers
				continue;

			count ++;
			//str += string.Format ("\n{0,12} us, {1,12} N", time, force);
			str += string.Format ("\n{0} us\t {1} N", time, force);
		} while (count < samples);

		if (! endCaptureArduino ())
			return false;

		return true;
	}

	// copied from gui/app1/forceSensor.cs
	private bool forceSensorProcessCapturedLine (string str,
			out int time, out double force,
			bool readTriggers, out string triggerCode)
	{
		time = 0;
		force = 0;
		triggerCode = "";

		//check if there is one and only one ';'
		if( ! (str.Contains(";") && str.IndexOf(";") == str.LastIndexOf(";")) )
			return false;

		string [] strFull = str.Split(new char[] {';'});

		if (! Util.IsNumber (Util.ChangeDecimalSeparator (strFull[0]), true))
			return false;

		if (Util.IsNumber (Util.ChangeDecimalSeparator (strFull[1]), true))
		{
		}
		else if (readTriggers)
		{
			time = Convert.ToInt32 (strFull[0]);
			triggerCode = strFull[1].Trim(); //now is coming from Arduino with an enter
			return true;
		} else
			return false;

		time = Convert.ToInt32 (strFull[0]);

		//bad tare or bad calibration or too much force
		if (Math.Abs (Convert.ToDouble(Util.ChangeDecimalSeparator(strFull[1]))) > 20000) // 20000 N (2000 kg) Chronojump force sensors are up to 5000 but we have special version with 20000
		{
			str += string.Format ("\n- Error. Force too big: " + Util.ChangeDecimalSeparator  (strFull[1]));
			return false;
		}

		force = Convert.ToDouble (Util.ChangeDecimalSeparator (strFull[1]));

		return true;
	}
}

public class DebugEncoder : DebugDevices
{
	public DebugEncoder (ChronopicRegisterPort crp)
	{
		this.crp = crp;
		title = "Testing Encoder";

		done = false;
		debugDo ();
		done = true;
	}

	protected override void debugDo ()
	{
		if (! portCreate ())
			return;

		if (! portOpen ())
			return;

		if (! readSomeData ())
			return;

		portClose ();
	}

	protected override bool readSomeData ()
	{
		int samples = 50;
		str += string.Format ("\n\n- Capturing {0} samples …", samples);

		var buffer = new byte[1024];
		int countPrinted = 0;
		do {
			try {
				int bytesRead = port.Read (buffer, 0, buffer.Length);
				if (bytesRead == 0)
					continue;

				for (int j = 0; j < bytesRead && countPrinted < samples; j ++)
				{
					if (countPrinted % 10 == 0)
						str += "\n";

					str += string.Format ("{0,3} ", convertByte (Convert.ToInt32  (buffer[j])));
					countPrinted ++;
				}
			}
			catch (Exception ex)
			{
				if(ex is System.IO.IOException || ex is System.TimeoutException)
				{
					str += "\n- Failed at sending message. Error: " + ex.ToString ();
					return false;
				}
			}
		} while (countPrinted < samples);

		return true;
	}

	private int convertByte (int b)
	{
		if(b > 128)
			b = b - 256;

		return b;
	}
}
